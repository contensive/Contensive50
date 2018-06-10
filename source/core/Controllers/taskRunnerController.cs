
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
using Contensive.Processor.Models.Context;
//
namespace Contensive.Processor.Controllers {
    public class taskRunnerController : IDisposable {
        //
        //==================================================================================================
        //
        // Only a single instance of this class exists, held by the taskService, which windows only creates once during service start.
        //
        //   taskScheduler queries each application from addons that need to run and adds them to the tasks queue (tasks sql table or SQS queue)
        //       taskScheduleTimer
        //
        //   taskRunner polls the task queue and runs commands when found
        //       taskRunnerTimer
        //==================================================================================================
        //
        private string runnerGuid { get; set; } // set in constructor. used to tag tasks assigned to this runner
        //
        // ----- Task Timer
        //
        private System.Timers.Timer processTimer { get; set; }
        private const int ProcessTimerMsecPerTick = 5000; // Check processs every 5 seconds
        private bool ProcessTimerInProcess { get; set; }
        //
        // ----- Alarms within Process Timer
        //
        //Private SiteProcessAlarmTime As Date            ' Run Site Processes every 30 seconds
        private const int SiteProcessIntervalSeconds = 30;
        //
        // ----- Debugging
        //
        protected bool disposed = false;
        //
        //========================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="core"></param>
        /// <remarks></remarks>
        public taskRunnerController() {
            runnerGuid = genericController.getGUID();
        }
        //
        //========================================================================================================
        /// <summary>
        /// dispose
        /// </summary>
        /// <param name="disposing"></param>
        /// <remarks></remarks>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                }
                //
                // cp  creates and destroys cmc
                //
                //GC.Collect(); -- no more activeX, so let GC take care of itself
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Stop all activity through the content server, but do not unload
        /// </summary>
        public void stopTimerEvents() {
            processTimer.Enabled = false;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Process the Start signal from the Server Control Manager
        /// </summary>
        /// <param name="setVerbose"></param>
        /// <param name="singleThreaded"></param>
        /// <returns></returns>
        public bool startTimerEvents() {
            bool returnStartedOk = true;
            processTimer = new System.Timers.Timer(ProcessTimerMsecPerTick);
            processTimer.Elapsed += processTimerTick;
            processTimer.Interval = ProcessTimerMsecPerTick;
            processTimer.Enabled = true;
            return returnStartedOk;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Timer tick
        /// </summary>
        protected void processTimerTick(object sender, EventArgs e) {
            try {
                // non-thread safe. Use lock to prevent re-entry?
                if (ProcessTimerInProcess) {
                    //
                    Console.WriteLine("taskRunner.processTimerTick, skip -- processTimerInProcess true");
                } else {
                    ProcessTimerInProcess = true;
                    //
                    // run tasks in task
                    //
                    using (CPClass cpCluster = new CPClass()) {
                        if (!cpCluster.core.serverConfig.allowTaskRunnerService) {
                            logController.logTrace(cpCluster.core, "taskRunner.processTimerTick, skip -- allowTaskRunnerService false");
                        } else {
                            runTasks(cpCluster.core);
                        }
                    }
                    ProcessTimerInProcess = false;
                }
            } catch (Exception ex) {
                using (CPClass cp = new CPClass()) {
                    logController.handleError(cp.core,ex);
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Iterate through all apps, find addosn that need to run and add them to the task queue
        /// </summary>
        private void runTasks(coreController serverCore) {
            try {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                //
                foreach (KeyValuePair<string, Models.Context.appConfigModel> appKVP in serverCore.serverConfig.apps) {
                    //
                    // query tasks that need to be run
                    //
                    using (CPClass cpApp = new CPClass(appKVP.Value.name)) {
                        //
                        logController.logTrace(cpApp.core, "runTasks, appname=[" + appKVP.Value.name + "]");
                        //
                        if (cpApp.core.appConfig.appStatus == appConfigModel.appStatusEnum.ok) {
                            try {
                                bool tasksRemaining = false;
                                do {
                                    //
                                    // for now run an sql to get processes, eventually cache in variant cache
                                    tasksRemaining = false;
                                    string sql = ""
                                        + "\r\n BEGIN TRANSACTION"
                                        + "\r\n update cctasks set cmdRunner=" + cpApp.core.db.encodeSQLText(runnerGuid) + " where id in (select top 1 id from cctasks where (cmdRunner is null)and(datestarted is null))"
                                        + "\r\n COMMIT TRANSACTION";
                                    cpApp.core.db.executeQuery(sql);
                                    var taskList = taskModel.createList(cpApp.core, "(cmdRunner=" + cpApp.core.db.encodeSQLText(runnerGuid) + ")and(datestarted is null)","id");
                                    foreach (var task in taskList) {
                                        //
                                        logController.logTrace(cpApp.core, "runTasks, task [" + task.name + "], command [" + task.Command + "], cmdDetail [" + task.cmdDetail + "]");
                                        //
                                        tasksRemaining = true;
                                        task.DateStarted = DateTime.Now;
                                        task.save(cpApp.core);
                                        cmdDetailClass cmdDetail = cpApp.core.json.Deserialize<cmdDetailClass>( task.cmdDetail);
                                        switch ((task.Command.ToLower())) {
                                            case taskQueueCommandEnumModule.runAddon:
                                                cpApp.core.addon.execute(addonModel.create(cpApp.core, cmdDetail.addonId), new BaseClasses.CPUtilsBaseClass.addonExecuteContext {
                                                    backgroundProcess = true,
                                                    addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple,
                                                    instanceArguments = cmdDetail.docProperties,
                                                    errorContextMessage = "running task, addon [" + cmdDetail.addonId + "]"
                                                });
                                                break;
                                        }
                                        task.DateCompleted = DateTime.Now;
                                        task.save(cpApp.core);
                                    }
                                } while (tasksRemaining);
                            } catch (Exception ex) {
                                logController.handleError(cpApp.core, ex);
                            }
                        }
                    }
                }
                Console.WriteLine("runTasks, exit (" + sw.ElapsedMilliseconds + "ms)");
            } catch (Exception ex) {
                logController.handleError(serverCore, ex);
            }
        }
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~taskRunnerController() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }

}
