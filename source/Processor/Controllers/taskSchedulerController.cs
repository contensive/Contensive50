
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

using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using static Newtonsoft.Json.JsonConvert;
using Contensive.Processor.Models.Domain;
using Contensive.Models.Db;
//
namespace Contensive.Processor.Controllers {
    public class TaskSchedulerController : IDisposable {
        private System.Timers.Timer processTimer;
        private const int ProcessTimerMsecPerTick = 5000;
        private bool ProcessTimerInProcess;
        public bool StartServiceInProgress;
        protected bool disposed = false;
        //
        //========================================================================================================
        /// <summary>
        /// dispose
        /// </summary>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    //core.dispose()
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
            try {
                processTimer.Enabled = false;
                using (CPClass cp = new CPClass()) {
                    LogController.logTrace(cp.core, "stopTimerEvents");
                }
            } catch (Exception ex) {
                using (CPClass cp = new CPClass()) {
                    LogController.logError(cp.core, ex);
                }
            }
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
            bool returnStartedOk = false;
            try {
                // todo StartServiceInProgress does nothing. windows will not call it twice
                if (!StartServiceInProgress) {
                    StartServiceInProgress = true;
                    processTimer = new System.Timers.Timer(ProcessTimerMsecPerTick);
                    processTimer.Elapsed += processTimerTick;
                    //processTimer.Interval = ProcessTimerMsecPerTick;
                    processTimer.Enabled = true;
                    returnStartedOk = true;
                    StartServiceInProgress = false;
                }
                using (CPClass cp = new CPClass()) {
                    LogController.logTrace(cp.core, "stopTimerEvents");
                }
            } catch (Exception ex) {
                using (CPClass cp = new CPClass()) {
                    LogController.logError(cp.core, ex);
                }
            }
            return returnStartedOk;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Timer tick
        /// </summary>
        public void processTimerTick(object sender, EventArgs e) {
            try {
                // windows holds one instance of this class. This check needs a lock to catch the non-threadsafe check-then-set here
                if (!ProcessTimerInProcess) {
                    ProcessTimerInProcess = true;
                    using (CPClass cp = new CPClass()) {
                        if (cp.core.serverConfig.allowTaskSchedulerService) {
                            scheduleTasks(cp.core);
                        }
                        //
                        // -- log memory usage -- info
                        long workingSetMemory = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
                        long virtualMemory = System.Diagnostics.Process.GetCurrentProcess().VirtualMemorySize64;
                        long privateMemory = System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64;
                        LogController.log(cp.core, "TaskScheduler exit, workingSetMemory [" + workingSetMemory + "], virtualMemory [" + virtualMemory + "], privateMemory [" + privateMemory + "]", BaseClasses.CPLogBaseClass.LogLevel.Info);
                    }
                }
            } catch (Exception ex) {
                using (CPClass cp = new CPClass()) {
                    LogController.logError(cp.core, ex);
                }
            } finally {
                ProcessTimerInProcess = false;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Iterate through all apps, find addosn that need to run and add them to the task queue
        /// </summary>
        private void scheduleTasks(CoreController coreServer) {
            try {
                //
                // -- run tasks for each app
                foreach (KeyValuePair<string, Models.Domain.AppConfigModel> appKvp in coreServer.serverConfig.apps) {
                    if (appKvp.Value.enabled && appKvp.Value.appStatus.Equals(AppConfigModel.AppStatusEnum.ok)) {
                        LogController.logTrace(coreServer, "scheduleTasks, app=[" + appKvp.Value.name + "]");
                        using (CPClass cpApp = new CPClass(appKvp.Value.name)) {
                            //
                            // Execute Processes
                            try {
                                DateTime RightNow = DateTime.Now;
                                string sqlAddonsCriteria = ""
                                    + "(active<>0)"
                                    + " and(name<>'')"
                                    + " and("
                                    + "  ((ProcessRunOnce is not null)and(ProcessRunOnce<>0))"
                                    + "  or((ProcessInterval is not null)and(ProcessInterval<>0)and(ProcessNextRun is null))"
                                    + "  or(ProcessNextRun<" + DbController.encodeSQLDate(RightNow) + ")"
                                    + " )";
                                var addonList = DbBaseModel.createList<AddonModel>(cpApp, sqlAddonsCriteria);
                                foreach (var addon in addonList) {
                                    //
                                    // -- calculate next run if processInternal is not null and > 0
                                    DateTime nextRun = DateTime.MinValue;
                                    if (encodeInteger(addon.processInterval) > 0) {
                                        nextRun = RightNow.AddMinutes(encodeInteger(addon.processInterval));
                                    }
                                    if ((addon.processNextRun < RightNow) || (addon.processRunOnce)) {
                                        //
                                        LogController.logInfo(cpApp.core, "scheduleTasks, addon [" + addon.name + "], add task, addonProcessRunOnce [" + addon.processRunOnce + "], addonProcessNextRun [" + addon.processNextRun + "]");
                                        //
                                        // -- resolve triggering state
                                        addon.processRunOnce = false;
                                        if (addon.processNextRun < RightNow) { addon.processNextRun = nextRun; }
                                        //
                                        // -- add task to queue for runner
                                        addTaskToQueue(cpApp.core, new TaskModel.CmdDetailClass {
                                            addonId = addon.id,
                                            addonName = addon.name,
                                            args = GenericController.convertAddonArgumentstoDocPropertiesList(cpApp.core, addon.argumentList)
                                        }, false);
                                    } else if (addon.processNextRun == DateTime.MinValue) {
                                        //
                                        LogController.logInfo(cpApp.core, "scheduleTasks, addon [" + addon.name + "], setup next run, ProcessInterval set but no processNextRun, set processNextRun [" + nextRun + "]");
                                        //
                                        // -- Interval is OK but NextRun is 0, just set next run
                                        addon.processNextRun = nextRun;
                                    }
                                    addon.save(cpApp);
                                }
                            } catch (Exception ex) {
                                LogController.logTrace(cpApp.core, "scheduleTasks, exception [" + ex.ToString() + "]");
                                LogController.logError(cpApp.core, ex);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logTrace(coreServer, "scheduleTasks, exeception [" + ex.ToString() + "]");
                LogController.logError(coreServer, ex);
            }
        }
        //private void scheduleTasks(CoreController coreServer) {
        //    try {
        //        //
        //        // -- run tasks for each app
        //        foreach (KeyValuePair<string, Models.Domain.AppConfigModel> appKvp in coreServer.serverConfig.apps) {
        //            LogController.logTrace(coreServer, "scheduleTasks, app=[" + appKvp.Value.name + "]");
        //            using (CPClass cpApp = new CPClass(appKvp.Value.name)) {
        //                CoreController coreApp = cpApp.core;
        //                if (!(coreApp.appConfig.appStatus == AppConfigModel.AppStatusEnum.ok)) {
        //                    //
        //                    LogController.logTrace(coreApp, "scheduleTasks, app status not ok");
        //                //} else if (!(coreApp.appConfig.appMode == appConfigModel.appModeEnum.normal)) {
        //                //    //
        //                //    logController.logTrace(coreApp, "scheduleTasks, app mode not normal");
        //                } else {
        //                    //
        //                    // Execute Processes
        //                    try {
        //                        DateTime RightNow = DateTime.Now;
        //                        string SQLNow = DbController.encodeSQLDate(RightNow);
        //                        string sqlAddonsCriteria = ""
        //                            + "(Active<>0)"
        //                            + " and(name<>'')"
        //                            + " and("
        //                            + "  ((ProcessRunOnce is not null)and(ProcessRunOnce<>0))"
        //                            + "  or((ProcessInterval is not null)and(ProcessInterval<>0)and(ProcessNextRun is null))"
        //                            + "  or(ProcessNextRun<" + SQLNow + ")"
        //                            + " )";
        //                        int CS = coreApp.db.csOpen(AddonModel.contentName, sqlAddonsCriteria);
        //                        while (coreApp.db.csOk()) {
        //                            int addonProcessInterval = coreApp.db.csGetInteger("ProcessInterval");
        //                            string addonName = coreApp.db.csGetText("name");
        //                            bool addonProcessRunOnce = coreApp.db.csGetBoolean("ProcessRunOnce");
        //                            DateTime addonProcessNextRun = coreApp.db.csGetDate("ProcessNextRun");
        //                            DateTime nextRun = DateTime.MinValue;
        //                            if (addonProcessInterval > 0) {
        //                                nextRun = RightNow.AddMinutes(addonProcessInterval);
        //                            }
        //                            if ((addonProcessNextRun < RightNow) || (addonProcessRunOnce)) {
        //                                //
        //                                LogController.logTrace(coreApp, "scheduleTasks, addon [" + addonName + "], add task, addonProcessRunOnce [" + addonProcessRunOnce + "], addonProcessNextRun [" + addonProcessNextRun + "]");
        //                                //
        //                                // -- resolve triggering state
        //                                coreApp.db.csSet("ProcessRunOnce", false);
        //                                if (addonProcessNextRun < RightNow) {
        //                                    coreApp.db.csSet("ProcessNextRun", nextRun);
        //                                }
        //                                coreApp.db.csSave(CS);
        //                                //
        //                                // -- add task to queue for runner
        //                                var cmdDetail = new TaskModel.CmdDetailClass();
        //                                cmdDetail.addonId = coreApp.db.csGetInteger("ID");
        //                                cmdDetail.addonName = addonName;
        //                                cmdDetail.args = GenericController.convertAddonArgumentstoDocPropertiesList(coreApp, coreApp.db.csGetText("argumentlist"));
        //                                addTaskToQueue(coreApp, cmdDetail, false);
        //                            } else if (coreApp.db.csGetDate("ProcessNextRun") == DateTime.MinValue) {
        //                                //
        //                                LogController.logTrace(coreApp, "scheduleTasks, addon [" + addonName + "], setup next run, ProcessInterval set but no processNextRun, set processNextRun [" + nextRun + "]");
        //                                //
        //                                // -- Interval is OK but NextRun is 0, just set next run
        //                                coreApp.db.csSet("ProcessNextRun", nextRun);
        //                            }
        //                            coreApp.db.csGoNext();
        //                        }
        //                        coreApp.db.csClose();
        //                    } catch (Exception ex) {
        //                        //
        //                        LogController.logTrace(coreApp, "scheduleTasks, exception [" + ex.ToString() + "]");
        //                        LogController.handleError(coreApp, ex);
        //                    }
        //                }
        //            }
        //        }
        //    } catch (Exception ex) {
        //        LogController.logTrace(coreServer, "scheduleTasks, exeception [" + ex.ToString() + "]");
        //        LogController.handleError(coreServer, ex);
        //    }
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Add a command task to the taskQueue to be run by the taskRunner. Returns false if the task was already there (dups fround by command name and cmdDetailJson)
        /// </summary>
        /// <param name="core"></param>
        /// <param name="command"></param>
        /// <param name="cmdDetail"></param>
        /// <param name="downloadName"></param>
        /// <returns></returns>
        static public bool addTaskToQueue(CoreController core, TaskModel.CmdDetailClass cmdDetail, bool blockDuplicates, string downloadName, string downloadFilename) {
            bool resultTaskAdded = true;
            try {
                //
                int downloadId = 0;
                if (!string.IsNullOrEmpty(downloadName)) {
                    Dictionary<string, string> defaultValues = ContentMetadataModel.getDefaultValueDict(core, DownloadModel.contentName);
                    var download = DbBaseModel.addDefault<DownloadModel>(core.cpParent, defaultValues);
                    download.name = downloadName;
                    download.dateRequested = DateTime.Now;
                    download.requestedBy = core.session.user.id;
                    if (!string.IsNullOrEmpty(downloadFilename)) {
                        //
                        // -- if the donwloadfilename is specified, save it in the download record and force the file to save with a space in content
                        download.filename.filename = FileController.getVirtualRecordUnixPathFilename(DownloadModel.contentTableNameLowerCase, "filename", download.id, downloadFilename);
                        download.filename.content = " ";
                    }
                    downloadId = download.id;
                    download.save(core.cpParent);
                }
                string cmdDetailJson = SerializeObject(cmdDetail);
                if (blockDuplicates) {
                    //
                    // -- Search for a duplicate
                    string sql = "select top 1 id from cctasks where ((cmdDetail=" + cmdDetailJson + ")and(datestarted is not null))";
                    using (var csData = new CsModel(core)) {
                        resultTaskAdded = !csData.openSql(sql);
                    }
                }
                //
                // -- add it to the queue and shell out to the command
                if (resultTaskAdded) {
                    var task = TaskModel.addEmpty<TaskModel>(core.cpParent);
                    task.name = "addon [#" + cmdDetail.addonId + "," + cmdDetail.addonName + "]";
                    task.cmdDetail = cmdDetailJson;
                    task.resultDownloadId = downloadId;
                    task.save(core.cpParent);
                    LogController.logTrace(core, "addTaskToQueue, cmdDetailJson [" + cmdDetailJson + "]");
                }
            } catch (Exception ex) {
                LogController.logTrace(core, "addTaskToQueue, exeception [" + ex.ToString() + "]");
                LogController.logError(core, ex);
            }
            return resultTaskAdded;
        }
        //
        static public bool addTaskToQueue(CoreController core, TaskModel.CmdDetailClass cmdDetail, bool blockDuplicates)
            => addTaskToQueue(core, cmdDetail, blockDuplicates, "", "");
        //
        static public bool addTaskToQueue(CoreController core, TaskModel.CmdDetailClass cmdDetail, bool blockDuplicates, string downloadName)
            => addTaskToQueue(core, cmdDetail, blockDuplicates, downloadName, "");
        //
        //====================================================================================================
        //
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~TaskSchedulerController() {
            Dispose(false);


        }
        #endregion
    }
}
