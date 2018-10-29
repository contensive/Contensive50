
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
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
//
namespace Contensive.Processor.Controllers {
    /// <summary>
    /// taskRunner polls the task queue and runs commands when found
    /// </summary>
    public class TaskRunnerController : IDisposable {
        /// <summary>
        /// set in constructor. used to tag tasks assigned to this runner
        /// </summary>
        private string runnerGuid { get; set; }
        /// <summary>
        /// Task Timer
        /// </summary>
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
        public TaskRunnerController() {
            runnerGuid = GenericController.getGUID();
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
                            LogController.logTrace(cpCluster.core, "taskRunner.processTimerTick, skip -- allowTaskRunnerService false");
                        } else {
                            runTasks(cpCluster.core);
                        }
                    }
                    ProcessTimerInProcess = false;
                }
            } catch (Exception ex) {
                using (CPClass cp = new CPClass()) {
                    LogController.handleError(cp.core,ex);
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Iterate through all apps, find addosn that need to run and add them to the task queue
        /// </summary>
        private void runTasks(CoreController serverCore) {
            try {
                Stopwatch swProcess = new Stopwatch();
                swProcess.Start();
                //
                foreach (KeyValuePair<string, Models.Domain.AppConfigModel> appKVP in serverCore.serverConfig.apps) {
                    //
                    // query tasks that need to be run
                    //
                    using (CPClass cpApp = new CPClass(appKVP.Value.name)) {
                        //
                        LogController.logTrace(cpApp.core, "runTasks, appname=[" + appKVP.Value.name + "]");
                        //
                        if (cpApp.core.appConfig.appStatus == AppConfigModel.AppStatusEnum.ok) {
                            try {
                                int recordsAffected = 0;
                                int sequentialTaskCount = 0;
                                do {
                                    //
                                    // for now run an sql to get processes, eventually cache in variant cache
                                    string sql = ""
                                        + "\r\n BEGIN TRANSACTION"
                                        + "\r\n update cctasks set cmdRunner=" + cpApp.core.db.encodeSQLText(runnerGuid) + " where id in (select top 1 id from cctasks where (cmdRunner is null)and(datestarted is null))"
                                        + "\r\n COMMIT TRANSACTION";
                                    cpApp.core.db.executeNonQuery(sql,"",ref recordsAffected);
                                    if (recordsAffected == 0) {
                                        //
                                        // -- no tasks found
                                        LogController.logTrace(cpApp.core, "runTasks, appname=[" + appKVP.Value.name + "], no tasks");
                                    } else {
                                        Stopwatch swTask = new Stopwatch();
                                        swTask.Start();
                                        //
                                        // -- track multiple executions
                                        if (sequentialTaskCount>0) {
                                            LogController.logTrace(cpApp.core, "runTasks, appname=[" + appKVP.Value.name + "], multiple tasks run in a single cycle, sequentialTaskCount [" + sequentialTaskCount + "]");
                                        }
                                        //
                                        // -- two execution methods, 1) run task here, 2) start process and wait (so bad addon code does not memory link)
                                        if (cpApp.Site.GetBoolean("Run tasks in service process")) {
                                            //
                                            // -- execute here
                                            runTask(cpApp.Site.Name, runnerGuid);
                                        } else {
                                            //
                                            // -- execute in new  process
                                            Process process = new Process();
                                            process.StartInfo.CreateNoWindow = true;
                                            process.StartInfo.FileName = "cc.exe";
                                            process.StartInfo.WorkingDirectory = cpApp.core.programFiles.localAbsRootPath;
                                            process.StartInfo.Arguments = " --runTask " + appKVP.Value.name + " " + runnerGuid;
                                            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                            process.Start();
                                            //
                                            // todo manage multiple executing processes
                                            process.WaitForExit();
                                        }
                                        Console.WriteLine("runTasks, task complete (" + swTask.ElapsedMilliseconds + "ms)");
                                    }
                                    sequentialTaskCount++;
                                } while (recordsAffected > 0);
                            } catch (Exception ex) {
                                LogController.handleError(cpApp.core, ex);
                            }
                        }
                    }
                }
                Console.WriteLine("runTasks, exit (" + swProcess.ElapsedMilliseconds + "ms)");
            } catch (Exception ex) {
                LogController.handleError(serverCore, ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// run as single task from the cctasks table of an app, makred with a runnerGuid
        /// called from runTasks or from the cli in a different process
        /// </summary>
        public static void runTask(string appName, string runnerGuid) {
            try {
                using (var cp = new Contensive.Processor.CPClass(appName)) {
                    //
                    // -- execute here
                    foreach (var task in TaskModel.createList(cp.core, "(cmdRunner=" + cp.core.db.encodeSQLText(runnerGuid) + ")and(datestarted is null)", "id")) {
                        //
                        Console.WriteLine("runTask, runTask, task [" + task.name + "], command [" + task.command + "], cmdDetail [" + task.cmdDetail + "]");
                        LogController.logTrace(cp.core, "runTask, task [" + task.name + "], command [" + task.command + "], cmdDetail [" + task.cmdDetail + "]");
                        //
                        //tasksRemaining = true;
                        task.dateStarted = DateTime.Now;
                        task.save(cp.core);
                        CmdDetailClass cmdDetail = cp.core.json.Deserialize<CmdDetailClass>(task.cmdDetail);
                        switch ((task.command.ToLowerInvariant())) {
                            case TaskQueueCommandEnumModule.runAddon:
                                cp.core.addon.execute(AddonModel.create(cp.core, cmdDetail.addonId), new BaseClasses.CPUtilsBaseClass.addonExecuteContext {
                                    backgroundProcess = true,
                                    addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple,
                                    instanceArguments = cmdDetail.args,
                                    errorContextMessage = "running task, addon [" + cmdDetail.addonId + "]"
                                });
                                break;
                        }
                        task.dateCompleted = DateTime.Now;
                        task.save(cp.core);
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine("Error: [" + ex.ToString() + "]");
            }
        }
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~TaskRunnerController() {
            Dispose(false);
            
            
        }
        #endregion
    }

}
