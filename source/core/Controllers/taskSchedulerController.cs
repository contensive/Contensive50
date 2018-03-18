
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
    public class taskSchedulerController : IDisposable {
        private const string LogMsg = "For more information, see the Contensive Trace Log.";
        public bool allowVerboseLogging = true;
        public bool allowConsoleWrite = false;
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
                GC.Collect();
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
                    logController.logTrace(cp.core,"stopTimerEvents");
                }
            } catch (Exception ex) {
                using (CPClass cp = new CPClass()) {
                    logController.handleError(cp.core, ex);
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
        public bool startTimerEvents(bool setVerbose, bool singleThreaded) {
            bool returnStartedOk = false;
            try {
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
                    logController.logTrace(cp.core, "stopTimerEvents");
                }
            } catch (Exception ex) {
                using (CPClass cp = new CPClass()) {
                    logController.handleError(cp.core,ex);
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
                if (!ProcessTimerInProcess) {
                    ProcessTimerInProcess = true;
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    using (CPClass cp = new CPClass()) {
                        if (cp.core.serverConfig.allowTaskSchedulerService) {
                            scheduleTasks(cp.core);
                        }
                    }
                }
            } catch (Exception ex) {
                using (CPClass cp = new CPClass()) {
                    logController.handleError(cp.core,ex);
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
        private void scheduleTasks(coreController coreServer) {
            try {
                //
                // -- run tasks for each app
                foreach (KeyValuePair<string, Models.Context.appConfigModel> appKvp in coreServer.serverConfig.apps) {
                    logController.logTrace(coreServer, "scheduleTasks, app=[" + appKvp.Value.name + "]");
                    using (CPClass cpApp = new CPClass(appKvp.Value.name)) {
                        coreController coreApp = cpApp.core;
                        if (!(coreApp.appConfig.appStatus == appConfigModel.appStatusEnum.ok)) {
                            //
                            logController.logTrace(coreApp, "scheduleTasks, app status not ok");
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
                                int CS = coreApp.db.csOpen(cnAddons, sqlAddonsCriteria);
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
                                        logController.logTrace(coreApp, "scheduleTasks, addon [" + addonName + "], add task, addonProcessRunOnce [" + addonProcessRunOnce + "], addonProcessNextRun [" + addonProcessNextRun + "]");
                                        //
                                        // -- resolve triggering state
                                        coreApp.db.csSet(CS, "ProcessRunOnce", false);
                                        if (addonProcessNextRun < RightNow) {
                                            coreApp.db.csSet(CS, "ProcessNextRun", nextRun);
                                        }
                                        coreApp.db.csSave(CS);
                                        //
                                        // -- add task to queue for runner
                                        cmdDetailClass cmdDetail = new cmdDetailClass();
                                        cmdDetail.addonId = coreApp.db.csGetInteger(CS, "ID");
                                        cmdDetail.addonName = addonName;
                                        cmdDetail.docProperties = genericController.convertAddonArgumentstoDocPropertiesList(coreApp, coreApp.db.csGetText(CS, "argumentlist"));
                                        addTaskToQueue(coreApp, taskQueueCommandEnumModule.runAddon, cmdDetail, false);
                                    } else if (coreApp.db.csGetDate(CS, "ProcessNextRun") == DateTime.MinValue) {
                                        //
                                        logController.logTrace(coreApp, "scheduleTasks, addon [" + addonName + "], setup next run, ProcessInterval set but no processNextRun, set processNextRun [" + nextRun + "]");
                                        //
                                        // -- Interval is OK but NextRun is 0, just set next run
                                        coreApp.db.csSet(CS, "ProcessNextRun", nextRun);
                                    }
                                    coreApp.db.csGoNext(CS);
                                }
                                coreApp.db.csClose(ref CS);
                            } catch (Exception ex) {
                                //
                                logController.logTrace(coreApp, "scheduleTasks, exception [" + ex.ToString() + "]");
                                logController.handleError(coreApp, ex);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                logController.logTrace(coreServer, "scheduleTasks, exeception [" + ex.ToString() + "]");
                logController.handleError(coreServer, ex);
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
        static public bool addTaskToQueue(coreController cpSiteCore, string command, cmdDetailClass cmdDetail, bool BlockDuplicates) {
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
                    logController.logTrace(cpSiteCore, "addTaskToQueue, command [" + command + "], cmdDetailJson [" + cmdDetailJson + "]");
                }
            } catch (Exception ex) {
                logController.logTrace(cpSiteCore, "addTaskToQueue, exeception [" + ex.ToString() + "]");
                logController.handleError(cpSiteCore, ex);
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
        ~taskSchedulerController() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}
