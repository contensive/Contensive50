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
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
using System.IO;
//
namespace Contensive.Core.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class logController {
        //
        // ----- constants
        //
        //Private Const invalidationDaysDefault As Double = 365
        //
        // ----- private instance storage
        //
        //Private remoteCacheDisabled As Boolean
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
        /// <param name="cpCore"></param>
        /// <param name="LogLine"></param>
        /// <param name="LogFolder"></param>
        /// <param name="LogNamePrefix"></param>
        /// <param name="allowErrorHandling"></param>
        /// <remarks></remarks>
        public static void appendLog(coreClass cpCore, string LogLine, string LogFolder = "", string LogNamePrefix = "", bool allowErrorHandling = true) {
            try {
                Console.WriteLine(LogLine);
                if (string.IsNullOrEmpty(LogFolder) || cpCore.serverConfig.enableLogging) {
                    int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                    string threadName = threadId.ToString("00000000");
                    string absContent = LogFileCopyPrep(DateTime.Now.ToString("")) + "\tthread:" + threadName + "\t" + LogLine;
                    fileController fileSystem = null;
                    if (cpCore.serverConfig != null) {
                        if (cpCore.serverConfig.appConfig != null) {
                            //
                            // -- use app log space
                            fileSystem = cpCore.privateFiles;
                        }
                    }
                    if (fileSystem == null) {
                        //
                        // -- no app or no server, use program data files
                        fileSystem = cpCore.programDataFiles;
                    }
                    //
                    if (useMicrosoftTraceLogging) {
                        string logName;
                        if (cpCore.serverConfig.appConfig == null) {
                            // -- no app, log to server
                            logName = ("server/log/" + LogFolder).ToLower();
                        } else {
                            // -- use app log
                            logName = (cpCore.serverConfig.appConfig.name + "/log/" + LogFolder).ToLower();
                        }
                        if (!cpCore.doc.logList.ContainsKey(logName)) {
                            string logPathFile = fileSystem.rootLocalPath + "logs\\" + LogFolder.ToLower() + "\\" + getDateString(DateTime.Now) + ".log";
                            if (!fileSystem.fileExists(logPathFile)) {
                                fileSystem.appendFile(logPathFile, "");
                            }
                            cpCore.doc.logList.Add(logName, new TextWriterTraceListener(logPathFile, logName));
                        }
                        cpCore.doc.logList[logName].WriteLine(absContent);
                    } else {
                        //
                        // -- until trace works
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
                                        fileSystem.appendFile(PathFilenameNoExt + FileSuffix + ".log", absContent + "\r\n");
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
        /// <param name="cpCore"></param>
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
        public static void appendLogWithLegacyRow(coreClass cpCore, string ContensiveAppName, string contextDescription, string processName, string ClassName, string MethodName, int ErrNumber, string ErrSource, string ErrDescription, bool ErrorTrap, bool ResumeNextAfterLogging, string URL, string LogFolder, string LogNamePrefix) {
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
                appendLog(cpCore, LogLine, LogFolder, LogNamePrefix);
            } catch (Exception ex) {

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
        public static void logActivity(coreClass cpcore, string Message, int ByMemberID, int SubjectMemberID, int SubjectOrganizationID, string Link = "", int VisitorID = 0, int VisitID = 0) {
            try {
                //
                int CS;
                //
                CS = cpcore.db.csInsertRecord("Activity Log", ByMemberID);
                if (cpcore.db.csOk(CS)) {
                    cpcore.db.csSet(CS, "MemberID", SubjectMemberID);
                    cpcore.db.csSet(CS, "OrganizationID", SubjectOrganizationID);
                    cpcore.db.csSet(CS, "Message", Message);
                    cpcore.db.csSet(CS, "Link", Link);
                    cpcore.db.csSet(CS, "VisitorID", VisitorID);
                    cpcore.db.csSet(CS, "VisitID", VisitID);
                }
                cpcore.db.csClose(ref CS);
                //
                return;
                //
            } catch (Exception ex) {
                cpcore.handleException(ex);
            }
            //ErrorTrap:
            throw (new Exception("Unexpected exception"));
        }
        //
        //
        public static void logActivity2(coreClass cpcore, string Message, int SubjectMemberID, int SubjectOrganizationID) {
            logActivity(cpcore, Message, cpcore.doc.sessionContext.user.id, SubjectMemberID, SubjectOrganizationID, cpcore.webServer.requestUrl, cpcore.doc.sessionContext.visitor.id, cpcore.doc.sessionContext.visit.id);
        }
        //
        //
        //
        internal static void appendLogPageNotFound(coreClass cpCore, string PageNotFoundLink) {
            appendLog(cpCore, "bad link [" + PageNotFoundLink + "], referrer [" + cpCore.webServer.requestReferrer + "]", "BadLink");
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
        public static void reportWarning(coreClass cpcore, string Name, string shortDescription, string location, int PageID, string Description, string generalKey, string specificKey) {
            string SQL = null;
            int warningId = 0;
            int CS = 0;
            //
            warningId = 0;
            SQL = "select top 1 ID from ccSiteWarnings"
                + " where (generalKey=" + cpcore.db.encodeSQLText(generalKey) + ")"
                + " and(specificKey=" + cpcore.db.encodeSQLText(specificKey) + ")"
                + "";
            DataTable dt = cpcore.db.executeQuery(SQL);
            if (dt.Rows.Count > 0) {
                warningId = genericController.encodeInteger(dt.Rows[0]["id"]);
            }
            //
            if (warningId != 0) {
                //
                // increment count for matching warning
                //
                SQL = "update ccsitewarnings set count=count+1,DateLastReported=" + cpcore.db.encodeSQLDate(DateTime.Now) + " where id=" + warningId;
                cpcore.db.executeQuery(SQL);
            } else {
                //
                // insert new record
                //
                CS = cpcore.db.csInsertRecord("Site Warnings", 0);
                if (cpcore.db.csOk(CS)) {
                    cpcore.db.csSet(CS, "name", Name);
                    cpcore.db.csSet(CS, "description", Description);
                    cpcore.db.csSet(CS, "generalKey", generalKey);
                    cpcore.db.csSet(CS, "specificKey", specificKey);
                    cpcore.db.csSet(CS, "count", 1);
                    cpcore.db.csSet(CS, "DateLastReported", DateTime.Now);
                    if (true) {
                        cpcore.db.csSet(CS, "shortDescription", shortDescription);
                        cpcore.db.csSet(CS, "location", location);
                        cpcore.db.csSet(CS, "pageId", PageID);
                    }
                }
                cpcore.db.csClose(ref CS);
            }
            //
        }
        //
        //====================================================================================================
        //
        public static void appendLogCache(coreClass cpCore, string message) {
            appendLog(cpCore, message, "cache");
        }
        //
        //====================================================================================================
        //
        public static void appendLogDebug(coreClass cpCore, string message) {
            appendLog(cpCore, message, "debug");
        }
        //
        //====================================================================================================
        //
        public static void appendLogInstall(coreClass cpCore, string message) {
            appendLog(cpCore, message, "install");
        }
        //
        //====================================================================================================
        //
        public static void appendLogTasks(coreClass cpCore, string message) {
            appendLog(cpCore, message, "tasks");
        }
        //
        //====================================================================================================
        //
        public static void housekeepLogFolder(coreClass cpCore) {
            try {
                //
                DateTime LogDate = default(DateTime);
                //Dim fs As New fileSystemClass
                FileInfo[] FileList = null;
                //
                LogDate = DateTime.Now.AddDays(-30);
                FileList = cpCore.privateFiles.getFileList("logs\\");
                foreach (FileInfo file in FileList) {
                    if (file.CreationTime < LogDate) {
                        cpCore.privateFiles.deleteFile("logs\\" + file.Name);
                    }
                }
                //
                return;
                //
            } catch (Exception ex) {
                ;
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception");
        }
    }
}