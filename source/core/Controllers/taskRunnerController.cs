﻿
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
using Contensive.Core.Models.Context;
//
namespace Contensive.Core.Controllers {
    public class taskRunnerController : IDisposable {
        //
        //==================================================================================================
        //   Code copied from workerClass to both taskScheduler and taskRunner - remove what does not apply
        //
        //   taskScheduler queries each application from addons that need to run and adds them to the tasks queue (tasks sql table or SQS queue)
        //       taskScheduleTimer
        //
        //   taskRunner polls the task queue and runs commands when found
        //       taskRunnerTimer
        //==================================================================================================
        //
        //Private core As coreClass
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
            //Me.core = core
            runnerGuid = Guid.NewGuid().ToString();
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
                    // core.dispose()
                }
                //
                // cp  creates and destroys cmc
                //
                GC.Collect();
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
                            logController.appendLogTasks(cpCluster.core, "taskRunner.processTimerTick, skip -- allowTaskRunnerService false");
                        } else {
                            runTasks(cpCluster.core);
                        }
                    }
                    ProcessTimerInProcess = false;
                }
            } catch (Exception ex) {
                using (CPClass cp = new CPClass()) {
                    cp.core.handleException(ex);
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Iterate through all apps, find addosn that need to run and add them to the task queue
        /// </summary>
        private void runTasks(coreController cpClusterCore) {
            try {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                //
                string command = null;
                cmdDetailClass cmdDetail = null;
                string cmdDetailText = null;
                bool recordsRemaining = false;
                int CS = 0;
                string sql = null;
                string AppName = null;
                //
                foreach (KeyValuePair<string, Models.Context.appConfigModel> kvp in cpClusterCore.serverConfig.apps) {
                    AppName = kvp.Value.name;
                    //
                    logController.appendLogTasks(cpClusterCore, "runTasks, appname=[" + AppName + "]");
                    //
                    // query tasks that need to be run
                    //
                    using (CPClass cpSite = new CPClass(AppName)) {
                        if ((cpSite.core.appConfig.appStatus == appConfigModel.appStatusEnum.OK) && (cpSite.core.appConfig.appMode == appConfigModel.appModeEnum.normal)) {
                            try {
                                do {
                                    //
                                    // for now run an sql to get processes, eventually cache in variant cache
                                    recordsRemaining = false;
                                    sql = ""
                                    + "\r\n BEGIN TRANSACTION"
                                    + "\r\n update cctasks set cmdRunner=" + cpSite.core.db.encodeSQLText(runnerGuid) + " where id in (select top 1 id from cctasks where (cmdRunner is null)and(datestarted is null))"
                                    + "\r\n COMMIT TRANSACTION";
                                    cpSite.core.db.executeQuery(sql);
                                    CS = cpSite.core.db.csOpen("tasks", "(cmdRunner=" + cpSite.core.db.encodeSQLText(runnerGuid) + ")and(datestarted is null)", "id");
                                    if (cpSite.core.db.csOk(CS)) {
                                        //
                                        // -- execute a task
                                        recordsRemaining = true;
                                        cpSite.core.db.csSet(CS, "datestarted", DateTime.Now);
                                        cpSite.core.db.csSave2(CS);
                                        //
                                        command = cpSite.core.db.csGetText(CS, "command");
                                        cmdDetailText = cpSite.core.db.csGetText(CS, "cmdDetail");
                                        cmdDetail = cpSite.core.json.Deserialize<cmdDetailClass>(cmdDetailText);
                                        //
                                        logController.appendLogTasks(cpClusterCore, "runTasks, task [" + cpSite.core.db.csGetText(CS, "name") + "], command=[" + command + "], cmdDetailText=[" + cmdDetailText + "]");
                                        //
                                        switch ((command.ToLower())) {
                                            case taskQueueCommandEnumModule.runAddon:
                                                cpSite.core.addon.execute(Models.DbModels.addonModel.create(cpSite.core, cmdDetail.addonId), new BaseClasses.CPUtilsBaseClass.addonExecuteContext {
                                                    backgroundProcess = true,
                                                    addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple,
                                                    instanceArguments = cmdDetail.docProperties
                                                });
                                                //Call cpSite.core.addon.execute_legacy7(cmdDetail.addonId, cmdDetail.docProperties, Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple)
                                                break;
                                        }
                                        cpSite.core.db.csSet(CS, "datecompleted", DateTime.Now);
                                    }
                                    cpSite.core.db.csClose(ref CS);
                                } while (recordsRemaining);
                            } catch (Exception ex) {
                                cpClusterCore.handleException(ex);
                            }
                        }
                    }
                }
                Console.WriteLine("runTasks, exit (" + sw.ElapsedMilliseconds + "ms)");
            } catch (Exception ex) {
                cpClusterCore.handleException(ex);
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