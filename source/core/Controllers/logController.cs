
using System;
using System.Data;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;
//
namespace Contensive.Core.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// nlog: http://nlog-project.org/
    /// base configuration from: https://brutaldev.com/post/logging-setup-in-5-minutes-with-nlog
    /// </summary>
    public static class logController {
        //
        public enum logLevel {
            /// <summary>
            /// Begin method X, end method X etc
            /// </summary>
            Trace = 0,
            /// <summary>
            /// Executed queries, user authenticated, session expired
            /// </summary>
            Debug = 1,
            /// <summary>
            /// Normal behavior like mail sent, user updated profile etc.
            /// </summary>
            Info = 2,
            /// <summary>
            /// Incorrect behavior but the application can continue
            /// </summary>
            Warn = 3,
            /// <summary>
            /// For example application crashes / exceptions.
            /// </summary>
            Error = 4,
            /// <summary>
            /// Highest level: important stuff down
            /// </summary>
            Fatal = 5
        }
        //
        private static string LogFileCopyPrep(string Source) {
            string Copy = Source;
            Copy = genericController.vbReplace(Copy, "\r\n", " ");
            Copy = genericController.vbReplace(Copy, "\n", " ");
            Copy = genericController.vbReplace(Copy, "\r", " ");
            return Copy;
        }
        //
        //=============================================================================
        /// <summary>
        /// log Executed queries, user authenticated, session expired
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public static void logDebug(coreController core, string message) {
            log(core, message, logLevel.Debug);
        }
        //
        //=============================================================================
        /// <summary>
        /// log application crashes / exceptions
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public static void logError(coreController core, string message) {
            log(core, message, logLevel.Error);
        }
        //
        //=============================================================================
        /// <summary>
        /// log highest level, most important messages
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public static void logFatal(coreController core, string message) {
            log(core, message, logLevel.Fatal);
        }
        //
        //=============================================================================
        /// <summary>
        /// Normal behavior like mail sent, user updated profile etc
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public static void logInfo(coreController core, string message) {
            log(core, message, logLevel.Info);
        }
        //
        //=============================================================================
        /// <summary>
        /// log begin method, end method, etc
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public static void logTrace(coreController core, string message) {
            log(core, message, logLevel.Trace);
        }
        //
        //=============================================================================
        /// <summary>
        /// log incorrect behavior but the application can continue
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public static void logWarn(coreController core, string message) {
            log(core, message, logLevel.Warn);
        }
        //
        //=============================================================================
        /// <summary>
        /// log any level with NLOG without considering configuration. Onle use in extreme cases where the application environment is not stable.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <param name="level"></param>
        public static void logWithoutConfig(string message, logLevel level ) {
            try {
                //FileTarget target = LogManager.Configuration.FindTargetByName<FileTarget>("LogFile");
                //target.FileName = filename;
                string threadName = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString("00000000");
                string logContent = level.ToString() + "\tthread:" + threadName + "\t" + message;
                Logger nlogLogger = LogManager.GetCurrentClassLogger();
                //Logger nlogLogger = LogManager.GetLogger("loggerName"); //??? throws error
                //logger.Info("nlog test");
                //
                // decouple NLog types from internal enum
                switch (level) {
                    case logLevel.Trace:
                        //Console.WriteLine("Trace:" + message);
                        nlogLogger.Trace(message);
                        break;
                    case logLevel.Debug:
                        //Console.WriteLine("Debug:" + message);
                        nlogLogger.Debug(message);
                        break;
                    case logLevel.Info:
                        //Console.WriteLine("Info:" + message);
                        nlogLogger.Info(message);
                        break;
                    case logLevel.Warn:
                        //Console.WriteLine("Warn:" + message);
                        nlogLogger.Warn(message);
                        break;
                    case logLevel.Error:
                        //Console.WriteLine("Error:" + message);
                        nlogLogger.Error(message);
                        break;
                    case logLevel.Fatal:
                        //Console.WriteLine("Fatal:" + message);
                        nlogLogger.Fatal(message);
                        break;
                }
            } catch (Exception) {
                // -- ignore errors in error handling
            } finally {
                //
            }
        }
        //
        //=============================================================================
        /// <summary>
        /// Log all levels if configured, else log errors and above
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <param name="level"></param>
        public static void log(coreController core, string message, logLevel level) {
            try {
                if ((level >= logLevel.Error) || core.serverConfig.enableLogging) {
                    //
                    // -- logging enabled or trace log, append log
                    if (core.useNlog) {
                        //
                        // -- log to Nlog
                        logWithoutConfig(message, level);
                    } else {
                        string threadName = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString("00000000");
                        string logContent = LogFileCopyPrep(DateTime.Now.ToString("")) + "\t" + level.ToString() + "\tthread:" + threadName + "\t" + message;
                        //
                        Console.WriteLine(message);
                        //
                        fileController fileSystem = null;
                        if (core.serverConfig != null) {
                            if (core.appConfig != null) {
                                //
                                // -- use app log space
                                fileSystem = core.privateFiles;
                            }
                        }
                        //
                        // -- fall-back to simple appending
                        //
                        if (fileSystem == null) {
                            //
                            // -- no app or no server, use program data files
                            fileSystem = core.programDataFiles;
                        }
                        //
                        try {
                            DateTime rightNow = DateTime.Now;
                            string FilenameNoExt = rightNow.Year + rightNow.Month.ToString().PadLeft(2, '0') + rightNow.Day.ToString().PadLeft(2, '0');
                            string logPath = "logs\\";
                            //
                            // check for serverconfig, then for appConfig, else use programdata folder
                            //
                            int FileSize = 0;
                            if (!fileSystem.pathExists(logPath)) {
                                fileSystem.createPath(logPath);
                            } else {
                                //FileInfo[] logFiles = fileSystem.getFileList(logPath);
                                //foreach (FileInfo fileInfo in logFiles) {
                                //    if (fileInfo.Name.ToLower() == FilenameNoExt.ToLower() + ".log") {
                                //        FileSize = (int)fileInfo.Length;
                                //        break;
                                //    }
                                //}
                            }
                            string PathFilenameNoExt = logPath + FilenameNoExt;
                            //
                            // -- add to log file
                            if (FileSize < 10000000) {
                                int RetryCnt = 0;
                                bool SaveOK = false;
                                string FileSuffix = "";
                                while ((!SaveOK) && (RetryCnt < 10)) {
                                    SaveOK = true;
                                    try {
                                        fileSystem.appendFile(PathFilenameNoExt + FileSuffix + ".log", logContent + "\r\n");
                                    } catch (IOException) {
                                        //
                                        // permission denied - happens when more then one process are writing at once, go to the next suffix
                                        //
                                        FileSuffix = "-" + (RetryCnt + 1).ToString();
                                        RetryCnt = RetryCnt + 1;
                                        SaveOK = false;
                                    } catch (Exception) {
                                        //
                                        // unknown error
                                        //
                                        FileSuffix = "-" + (RetryCnt + 1).ToString();
                                        RetryCnt = RetryCnt + 1;
                                        SaveOK = false;
                                    }
                                }
                            }
                        } catch (Exception) {
                            // -- ignore errors in error handling
                        }
                    }
                }
            } catch (Exception) {
                // -- ignore errors in error handling
            } finally {
                //
            }
        }
        //
        //=====================================================================================================
        /// <summary>
        /// add activity about a user to the site's activity log for content managers to review
        /// </summary>
        /// <param name="core"></param>
        /// <param name="Message"></param>
        /// <param name="ByMemberID"></param>
        /// <param name="SubjectMemberID"></param>
        /// <param name="SubjectOrganizationID"></param>
        /// <param name="Link"></param>
        /// <param name="VisitorID"></param>
        /// <param name="VisitID"></param>
        public static void addSiteActivity(coreController core, string Message, int ByMemberID, int SubjectMemberID, int SubjectOrganizationID, string Link = "", int VisitorID = 0, int VisitID = 0) {
            try {
                //
                if (Message.Length > 255) Message = Message.Substring(0, 255);
                if (Link.Length > 255) Message = Link.Substring(0, 255);
                int CS = core.db.csInsertRecord("Activity Log", ByMemberID);
                if (core.db.csOk(CS)) {
                    core.db.csSet(CS, "MemberID", SubjectMemberID);
                    core.db.csSet(CS, "OrganizationID", SubjectOrganizationID);
                    core.db.csSet(CS, "Message", Message);
                    core.db.csSet(CS, "Link", Link);
                    core.db.csSet(CS, "VisitorID", VisitorID);
                    core.db.csSet(CS, "VisitID", VisitID);
                }
                core.db.csClose(ref CS);
                //
                return;
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            throw (new Exception("Unexpected exception"));
        }
        //
        //=====================================================================================================
        /// <summary>
        /// add activity about a user to the site's activity log for content managers to review
        /// </summary>
        /// <param name="core"></param>
        /// <param name="Message"></param>
        /// <param name="SubjectMemberID"></param>
        /// <param name="SubjectOrganizationID"></param>
        public static void addSiteActivity(coreController core, string Message, int SubjectMemberID, int SubjectOrganizationID) {
            addSiteActivity(core, Message, core.session.user.id, SubjectMemberID, SubjectOrganizationID, core.webServer.requestUrl, core.session.visitor.id, core.session.visit.id);
        }
        //
        //================================================================================================
        /// <summary>
        /// Add a site Warning: for content managers to make content changes with the site
        ///   Report Warning
        ///       A warning is logged in the site warnings log
        ///           name - a generic description of the warning
        ///               "bad link found on page"
        ///           short description - a 255 character cause
        ///               "bad link http://thisisabadlink.com"
        ///           location - the URL, service or process that caused the problem
        ///               "http://goodpageThankHasBadLink.com"
        ///           pageid - the record id of the bad page.
        ///               "http://goodpageThankHasBadLink.com"
        ///           description - a specific description
        ///               "link to http://www.this.com/pagename was found on http://www.this.com/About-us"
        ///           generalKey - a generic string that describes the warning. the warning report
        ///               will display one line for each generalKey (name matches guid)
        ///               like "bad link"
        ///           specificKey - a string created by the addon logging so it does not continue to log exactly the
        ///               same warning over and over. If there are 100 different link not found warnings,
        ///               there should be 100 entires with the same guid and name, but 100 different keys. If the
        ///               an identical key is found the count increments.
        ///               specifickey is like "link to http://www.this.com/pagename was found on http://www.this.com/About-us"
        ///           count - the number of times the key was attempted to add. "This error was reported 100 times"
        /// </summary>
        /// <param name="core"></param>
        /// <param name="Name"></param>
        /// <param name="shortDescription"></param>
        /// <param name="location"></param>
        /// <param name="PageID"></param>
        /// <param name="Description"></param>
        /// <param name="generalKey"></param>
        /// <param name="specificKey"></param>
        //
        public static void addSiteWarning(coreController core, string Name, string shortDescription, string location, int PageID, string Description, string generalKey, string specificKey) {
            string SQL = null;
            int warningId = 0;
            int CS = 0;
            //
            warningId = 0;
            SQL = "select top 1 ID from ccSiteWarnings"
                + " where (generalKey=" + core.db.encodeSQLText(generalKey) + ")"
                + " and(specificKey=" + core.db.encodeSQLText(specificKey) + ")"
                + "";
            DataTable dt = core.db.executeQuery(SQL);
            if (dt.Rows.Count > 0) {
                warningId = genericController.encodeInteger(dt.Rows[0]["id"]);
            }
            //
            if (warningId != 0) {
                //
                // increment count for matching warning
                //
                SQL = "update ccsitewarnings set count=count+1,DateLastReported=" + core.db.encodeSQLDate(DateTime.Now) + " where id=" + warningId;
                core.db.executeQuery(SQL);
            } else {
                //
                // insert new record
                //
                CS = core.db.csInsertRecord("Site Warnings", 0);
                if (core.db.csOk(CS)) {
                    core.db.csSet(CS, "name", Name);
                    core.db.csSet(CS, "description", Description);
                    core.db.csSet(CS, "generalKey", generalKey);
                    core.db.csSet(CS, "specificKey", specificKey);
                    core.db.csSet(CS, "count", 1);
                    core.db.csSet(CS, "DateLastReported", DateTime.Now);
                    if (true) {
                        core.db.csSet(CS, "shortDescription", shortDescription);
                        core.db.csSet(CS, "location", location);
                        core.db.csSet(CS, "pageId", PageID);
                    }
                }
                core.db.csClose(ref CS);
            }
            //
        }
        //
        //====================================================================================================
        /// <summary>
        /// not implemented, as current logging system does not need to be housekeeped. Keep the hood here for the future
        /// </summary>
        /// <param name="core"></param>
        public static void housekeepLogs(coreController core) {
            try {
                // -- deprecated, NLog handles housekeeping
                return;
            } catch (Exception) { }
        }
    }
//    //
//    // ====================================================================================================
//    /// <summary>
//    /// logging class from https://brutaldev.com/post/logging-setup-in-5-minutes-with-nlog
//    /// </summary>
//    internal static class NLogController {
//        public static Logger loggerInstance { get; private set; }
//        static NLogController() {
//// #if DEBUG
//            // from example - https://github.com/nlog/nlog/wiki/Configuration-API
//            // and - Setup the logging view for Sentinel - http://sentinel.codeplex.com
//            var config = new LoggingConfiguration();
//            var sentinalTarget = new NLogViewerTarget() {
//                Name = "sentinal",
//                Address = "udp://127.0.0.1:9999",
//                IncludeNLogData = false
//            };
//            var sentinalRule = new LoggingRule("*", LogLevel.Trace, sentinalTarget);
//            config.AddTarget("sentinal", sentinalTarget);
//            config.LoggingRules.Add(sentinalRule);
//            LogManager.Configuration = config;
//// #endif
//            LogManager.ReconfigExistingLoggers();
//            loggerInstance = LogManager.GetCurrentClassLogger();
//        }

//    }
}