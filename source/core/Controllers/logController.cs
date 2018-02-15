
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
//
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
        public static string LogFileCopyPrep(string Source) {
            string Copy = Source;
            Copy = genericController.vbReplace(Copy, "\r\n", " ");
            Copy = genericController.vbReplace(Copy, "\n", " ");
            Copy = genericController.vbReplace(Copy, "\r", " ");
            return Copy;
        }
        //
        //=============================================================================
        /// <summary>
        /// add the log line to a log file with the folder and prefix
        /// </summary>
        /// <param name="core"></param>
        /// <param name="LogLine"></param>
        /// <param name="LogFolder"></param>
        /// <param name="LogNamePrefix"></param>
        /// <param name="allowErrorHandling"></param>
        /// <remarks></remarks>
        public static void appendLog(coreController core, string LogLine, string LogFolder = "", string LogNamePrefix = "") {
            try {
                if (string.IsNullOrEmpty(LogFolder) || core.serverConfig.enableLogging) {
                    //
                    // -- logging enabled or trace log, append log
                    string threadName = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString("00000000");
                    if (core.useNlog) {
                        //
                        string logContent = LogFileCopyPrep(DateTime.Now.ToString("")) + "\tthread:" + threadName + "\t" + LogLine;
                        Console.WriteLine(LogLine);
                        //
                        // todo: implement this singleton in coreclass so logController can be static
                        // todo: work log levels into the internal logging system
                        Log.Instance.Info(LogLine);
                    } else {
                        //
                        string logContent = LogFileCopyPrep(DateTime.Now.ToString("")) + "\tthread:" + threadName + "\t" + LogLine;
                        Console.WriteLine(LogLine);
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
                            string FilenameNoExt = getDateString(DateTime.Now);
                            string logPath = LogFolder;
                            if (!string.IsNullOrEmpty(logPath)) {
                                logPath = logPath + "\\";
                            }
                            logPath = "logs\\" + logPath;
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
        //========================================================================
        /// <summary>
        /// Append log, use the legacy row with tab delimited context
        /// </summary>
        /// <param name="core"></param>
        /// <param name="ContensiveAppName"></param>
        /// <param name="contextDescription"></param>
        /// <param name="processName"></param>
        /// <param name="ClassName"></param>
        /// <param name="MethodName"></param>
        /// <param name="ErrNumber"></param>
        /// <param name="ErrSource"></param>
        /// <param name="ErrDescription"></param>
        /// <param name="ErrorTrap"></param>
        /// <param name="ResumeNextAfterLogging"></param>
        /// <param name="URL"></param>
        /// <param name="LogFolder"></param>
        /// <param name="LogNamePrefix"></param>
        /// <remarks></remarks>
        internal static void appendLogWithLegacyRow(coreController core, string ContensiveAppName, string contextDescription, string processName, string ClassName, string MethodName, int ErrNumber, string ErrSource, string ErrDescription, bool ErrorTrap, bool ResumeNextAfterLogging, string URL, string LogFolder, string LogNamePrefix) {
            try {
                string ErrorMessage = null;
                string LogLine = null;
                string ResumeMessage = null;
                //
                if (ErrorTrap) {
                    ErrorMessage = "Error Trap";
                } else {
                    ErrorMessage = "Log Entry";
                }
                //
                if (ResumeNextAfterLogging) {
                    ResumeMessage = "Resume after logging";
                } else {
                    ResumeMessage = "Abort after logging";
                }
                //
                LogLine = ""
                    + LogFileCopyPrep(ContensiveAppName) + "\t" + LogFileCopyPrep(processName) + "\t" + LogFileCopyPrep(ClassName) + "\t" + LogFileCopyPrep(MethodName) + "\t" + LogFileCopyPrep(contextDescription) + "\t" + LogFileCopyPrep(ErrorMessage) + "\t" + LogFileCopyPrep(ResumeMessage) + "\t" + LogFileCopyPrep(ErrSource) + "\t" + LogFileCopyPrep(ErrNumber.ToString()) + "\t" + LogFileCopyPrep(ErrDescription) + "\t" + LogFileCopyPrep(URL) + "";
                //
                appendLog(core, LogLine, LogFolder, LogNamePrefix);
            } catch (Exception) {

            }
        }

        //
        //====================================================================================================
        /// <summary>
        /// Create a string with year, month, date in the form 20151206
        /// </summary>
        public static string getDateString(DateTime sourceDate) {
            return sourceDate.Year + sourceDate.Month.ToString().PadLeft(2, '0') + sourceDate.Day.ToString().PadLeft(2, '0');
        }
        //
        //=====================================================================================================
        //   Insert into the ActivityLog
        internal static void logActivity(coreController core, string Message, int ByMemberID, int SubjectMemberID, int SubjectOrganizationID, string Link = "", int VisitorID = 0, int VisitID = 0) {
            try {
                //
                int CS;
                //
                CS = core.db.csInsertRecord("Activity Log", ByMemberID);
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
        //
        internal static void logActivity2(coreController core, string Message, int SubjectMemberID, int SubjectOrganizationID) {
            logActivity(core, Message, core.doc.sessionContext.user.id, SubjectMemberID, SubjectOrganizationID, core.webServer.requestUrl, core.doc.sessionContext.visitor.id, core.doc.sessionContext.visit.id);
        }
        //
        //
        //
        internal static void appendLogPageNotFound(coreController core, string PageNotFoundLink) {
            appendLog(core, "bad link [" + PageNotFoundLink + "], referrer [" + core.webServer.requestReferrer + "]", "BadLink");
        }
        //
        //================================================================================================
        //   Report Warning
        //       A warning is logged in the site warnings log
        //           name - a generic description of the warning
        //               "bad link found on page"
        //           short description - a <255 character cause
        //               "bad link http://thisisabadlink.com"
        //           location - the URL, service or process that caused the problem
        //               "http://goodpageThankHasBadLink.com"
        //           pageid - the record id of the bad page.
        //               "http://goodpageThankHasBadLink.com"
        //           description - a specific description
        //               "link to http://www.this.com/pagename was found on http://www.this.com/About-us"
        //           generalKey - a generic string that describes the warning. the warning report
        //               will display one line for each generalKey (name matches guid)
        //               like "bad link"
        //           specificKey - a string created by the addon logging so it does not continue to log exactly the
        //               same warning over and over. If there are 100 different link not found warnings,
        //               there should be 100 entires with the same guid and name, but 100 different keys. If the
        //               an identical key is found the count increments.
        //               specifickey is like "link to http://www.this.com/pagename was found on http://www.this.com/About-us"
        //           count - the number of times the key was attempted to add. "This error was reported 100 times"
        //================================================================================================
        //
        public static void reportWarning(coreController core, string Name, string shortDescription, string location, int PageID, string Description, string generalKey, string specificKey) {
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
        //
        public static void appendLogCache(coreController core, string message) {
            appendLog(core, message, "cache");
        }
        //
        //====================================================================================================
        //
        public static void appendLogDebug(coreController core, string message) {
            appendLog(core, message, "debug");
        }
        //
        //====================================================================================================
        //
        public static void appendLogInstall(coreController core, string message) {
            appendLog(core, message, "install");
        }
        //
        //====================================================================================================
        //
        public static void appendLogTasks(coreController core, string message) {
            appendLog(core, message, "tasks");
        }
        //
        //====================================================================================================
        //
        public static void housekeepLogFolder(coreController core) {
            try {
                // -- deprecated, NLog handles housekeeping
                //
                //DateTime LogDate = default(DateTime);
                ////Dim fs As New fileSystemClass
                //FileInfo[] FileList = null;
                ////
                //LogDate = DateTime.Now.AddDays(-30);
                //FileList = core.privateFiles.getFileList("logs\\");
                //foreach (FileInfo file in FileList) {
                //    if (file.CreationTime < LogDate) {
                //        core.privateFiles.deleteFile("logs\\" + file.Name);
                //    }
                //}
                //
                return;
                //
            } catch (Exception) { }
        }
        //
        // ====================================================================================================
        /// <summary>
        /// log to the program data folder. Use only when the application object is not available
        /// </summary>
        /// <param name="message"></param>
        public static void appendLogProgramData(string message) {
            string programDataPath = fileController.normalizePath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)) + "Contensive\\";
            System.IO.File.AppendAllText(programDataPath + getDateString(DateTime.Now) + ".log", message);
        }
    }
    //
    // ====================================================================================================
    /// <summary>
    /// logging class from https://brutaldev.com/post/logging-setup-in-5-minutes-with-nlog
    /// </summary>
    internal static class Log {
        public static Logger Instance { get; private set; }
        static Log() {
#if DEBUG
            // Setup the logging view for Sentinel - http://sentinel.codeplex.com
            var sentinalTarget = new NLogViewerTarget() {
                Name = "sentinal",
                Address = "udp://127.0.0.1:9999",
                IncludeNLogData = false
            };
            var sentinalRule = new LoggingRule("*", LogLevel.Trace, sentinalTarget);
            LogManager.Configuration.AddTarget("sentinal", sentinalTarget);
            LogManager.Configuration.LoggingRules.Add(sentinalRule);            
#endif
            LogManager.ReconfigExistingLoggers();
            Instance = LogManager.GetCurrentClassLogger();
        }

    }
}