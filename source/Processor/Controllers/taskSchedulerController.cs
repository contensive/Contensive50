
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
    public class TaskSchedulerControllerx : IDisposable {
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
                    LogController.logTrace(cp.core,"stopTimerEvents");
                }
            } catch (Exception ex) {
                using (CPClass cp = new CPClass()) {
                    LogController.handleError(cp.core, ex);
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
                    LogController.handleError(cp.core,ex);
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
                    }
                }
            } catch (Exception ex) {
                using (CPClass cp = new CPClass()) {
                    LogController.handleError(cp.core,ex);
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
                    LogController.logTrace(coreServer, "scheduleTasks, app=[" + appKvp.Value.name + "]");
                    using (CPClass cpApp = new CPClass(appKvp.Value.name)) {
                        CoreController coreApp = cpApp.core;
                        if (!(coreApp.appConfig.appStatus == AppConfigModel.AppStatusEnum.ok)) {
                            //
                            LogController.logTrace(coreApp, "scheduleTasks, app status not ok");
                        //} else if (!(coreApp.appConfig.appMode == appConfigModel.appModeEnum.normal)) {
                        //    //
                        //    logController.logTrace(coreApp, "scheduleTasks, app mode not normal");
                        } else {
                            //
                            // Execute Processes
                            try {
                                DateTime RightNow = DateTime.Now;
                                string SQLNow = coreApp.db.encodeSQLDate(RightNow);
                                string sqlAddonsCriteria = ""
                                    + "(Active<>0)"
                                    + " and(name<>'')"
                                    + " and("
                                    + "  ((ProcessRunOnce is not null)and(ProcessRunOnce<>0))"
                                    + "  or((ProcessInterval is not null)and(ProcessInterval<>0)and(ProcessNextRun is null))"
                                    + "  or(ProcessNextRun<" + SQLNow + ")"
                                    + " )";
                                int CS = coreApp.db.csOpen(Models.Db.AddonModel.contentName, sqlAddonsCriteria);
                                while (coreApp.db.csOk(CS)) {
                                    int addonProcessInterval = coreApp.db.csGetInteger(CS, "ProcessInterval");
                                    string addonName = coreApp.db.csGetText(CS, "name");
                                    bool addonProcessRunOnce = coreApp.db.csGetBoolean(CS, "ProcessRunOnce");
                                    DateTime addonProcessNextRun = coreApp.db.csGetDate(CS, "ProcessNextRun");
                                    DateTime nextRun = DateTime.MinValue;
                                    if (addonProcessInterval > 0) {
                                        nextRun = RightNow.AddMinutes(addonProcessInterval);
                                    }
                                    if ((addonProcessNextRun < RightNow) || (addonProcessRunOnce)) {
                                        //
                                        LogController.logTrace(coreApp, "scheduleTasks, addon [" + addonName + "], add task, addonProcessRunOnce [" + addonProcessRunOnce + "], addonProcessNextRun [" + addonProcessNextRun + "]");
                                        //
                                        // -- resolve triggering state
                                        coreApp.db.csSet(CS, "ProcessRunOnce", false);
                                        if (addonProcessNextRun < RightNow) {
                                            coreApp.db.csSet(CS, "ProcessNextRun", nextRun);
                                        }
                                        coreApp.db.csSave(CS);
                                        //
                                        // -- add task to queue for runner
                                        CmdDetailClass cmdDetail = new CmdDetailClass();
                                        cmdDetail.addonId = coreApp.db.csGetInteger(CS, "ID");
                                        cmdDetail.addonName = addonName;
                                        cmdDetail.args = GenericController.convertAddonArgumentstoDocPropertiesList(coreApp, coreApp.db.csGetText(CS, "argumentlist"));
                                        addTaskToQueue(coreApp, TaskQueueCommandEnumModule.runAddon, cmdDetail, false);
                                    } else if (coreApp.db.csGetDate(CS, "ProcessNextRun") == DateTime.MinValue) {
                                        //
                                        LogController.logTrace(coreApp, "scheduleTasks, addon [" + addonName + "], setup next run, ProcessInterval set but no processNextRun, set processNextRun [" + nextRun + "]");
                                        //
                                        // -- Interval is OK but NextRun is 0, just set next run
                                        coreApp.db.csSet(CS, "ProcessNextRun", nextRun);
                                    }
                                    coreApp.db.csGoNext(CS);
                                }
                                coreApp.db.csClose(ref CS);
                            } catch (Exception ex) {
                                //
                                LogController.logTrace(coreApp, "scheduleTasks, exception [" + ex.ToString() + "]");
                                LogController.handleError(coreApp, ex);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logTrace(coreServer, "scheduleTasks, exeception [" + ex.ToString() + "]");
                LogController.handleError(coreServer, ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Add a command task to the taskQueue to be run by the taskRunner. Returns false if the task was already there (dups fround by command name and cmdDetailJson)
        /// </summary>
        /// <param name="cpSiteCore"></param>
        /// <param name="command"></param>
        /// <param name="cmdDetail"></param>
        /// <param name="BlockDuplicates"></param>
        /// <returns></returns>
        static public bool addTaskToQueue(CoreController cpSiteCore, string command, CmdDetailClass cmdDetail, bool BlockDuplicates) {
            bool returnTaskAdded = true;
            try {
                string cmdDetailJson = cpSiteCore.json.Serialize(cmdDetail);
                if (BlockDuplicates) {
                    //
                    // -- Search for a duplicate
                    string sql = "select top 1 id from cctasks where ((command=" + cpSiteCore.db.encodeSQLText(command) + ")and(cmdDetail=" + cmdDetailJson + ")and(datestarted is not null))";
                    int cs = cpSiteCore.db.csOpenSql(sql);
                    returnTaskAdded = !cpSiteCore.db.csOk(cs);
                    cpSiteCore.db.csClose(ref cs);
                }
                //
                // -- add it to the queue and shell out to the command
                if (returnTaskAdded) {
                    int cs = cpSiteCore.db.csInsertRecord("tasks");
                    if (cpSiteCore.db.csOk(cs)) {
                        cpSiteCore.db.csSet(cs, "name", "command [" + command + "], addon [#" + cmdDetail.addonId + "," + cmdDetail.addonName + "]");
                        cpSiteCore.db.csSet(cs, "command", command);
                        cpSiteCore.db.csSet(cs, "cmdDetail", cmdDetailJson);
                    }
                    cpSiteCore.db.csClose(ref cs);
                    LogController.logTrace(cpSiteCore, "addTaskToQueue, command [" + command + "], cmdDetailJson [" + cmdDetailJson + "]");
                }
            } catch (Exception ex) {
                LogController.logTrace(cpSiteCore, "addTaskToQueue, exeception [" + ex.ToString() + "]");
                LogController.handleError(cpSiteCore, ex);
            }
            return returnTaskAdded;
        }
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
        ~TaskSchedulerControllerx() {
            Dispose(false);
            
            
        }
        #endregion
    }
}
