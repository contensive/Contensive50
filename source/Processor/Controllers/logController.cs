
using System;
using System.Data;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Diagnostics;
using System.Collections.Generic;
//
namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// nlog: http://nlog-project.org/
    /// base configuration from: https://brutaldev.com/post/logging-setup-in-5-minutes-with-nlog
    /// </summary>
    public static class LogController {
        //
        public enum LogLevel {
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
            Copy = GenericController.vbReplace(Copy, "\r\n", " ");
            Copy = GenericController.vbReplace(Copy, "\n", " ");
            Copy = GenericController.vbReplace(Copy, "\r", " ");
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
        public static void logDebug(CoreController core, string message) {
            log(core, message, LogLevel.Debug);
        }
        //
        //=============================================================================
        /// <summary>
        /// log application crashes / exceptions
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public static void logError(CoreController core, string message) {
            log(core, message, LogLevel.Error);
        }
        //
        //=============================================================================
        /// <summary>
        /// log highest level, most important messages
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public static void logFatal(CoreController core, string message) {
            log(core, message, LogLevel.Fatal);
        }
        //
        //=============================================================================
        /// <summary>
        /// Normal behavior like mail sent, user updated profile etc
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public static void logInfo(CoreController core, string message) {
            log(core, message, LogLevel.Info);
        }
        //
        //=============================================================================
        /// <summary>
        /// log begin method, end method, etc
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public static void logTrace(CoreController core, string message) {
            log(core, message, LogLevel.Trace);
        }
        //
        //=============================================================================
        /// <summary>
        /// log incorrect behavior but the application can continue
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public static void logWarn(CoreController core, string message) {
            log(core, message, LogLevel.Warn);
        }
        //
        //=============================================================================
        /// <summary>
        /// log any level with NLOG without considering configuration. Only use in extreme cases where the application environment is not stable.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <param name="level"></param>
        public static void forceNLog(string message, LogLevel level ) {
            try {
                string threadName = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString("00000000");
                string logContent = level.ToString() + "\tthread:" + threadName + "\t" + message;
                Logger nlogLogger = LogManager.GetCurrentClassLogger();
                //
                // -- decouple NLog types from internal enum
                NLog.LogLevel nLogLevel = NLog.LogLevel.Info;
                switch (level) {
                    case LogLevel.Trace:
                        nLogLevel = NLog.LogLevel.Trace;
                        break;
                    case LogLevel.Debug:
                        nLogLevel = NLog.LogLevel.Debug;
                        break;
                    case LogLevel.Info:
                        nLogLevel = NLog.LogLevel.Info;
                        break;
                    case LogLevel.Warn:
                        nLogLevel = NLog.LogLevel.Warn;
                        break;
                    case LogLevel.Error:
                        nLogLevel = NLog.LogLevel.Error;
                        break;
                    case LogLevel.Fatal:
                        nLogLevel = NLog.LogLevel.Fatal;
                        break;
                }
                //
                // -- log with even so we can pass in the type of our wrapper
                LogEventInfo logEvent = new LogEventInfo(nLogLevel, nlogLogger.Name, message);
                nlogLogger.Log(typeof(LogController), logEvent);
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
        public static void log(CoreController core, string message, LogLevel level) {
            try {
                //
                // -- use enableLogging to block application logging
                if ((level >= LogLevel.Error) || core.serverConfig.enableLogging) {
                    //
                    // -- log to Nlog
                    if (core.appConfig != null) {
                        forceNLog("app [" + core.appConfig.name + "], " + message, level);
                    } else {
                        forceNLog("non-app instance, " + message, level);
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
        public static void addSiteActivity(CoreController core, string Message, int ByMemberID, int SubjectMemberID, int SubjectOrganizationID, string Link = "", int VisitorID = 0, int VisitID = 0) {
            try {
                //
                if (Message.Length > 255) Message = Message.Substring(0, 255);
                if (Link.Length > 255) Message = Link.Substring(0, 255);
                using (var csData = new CsModel(core)) {
                    if (csData.insert("Activity Log")) {
                        csData.set("MemberID", SubjectMemberID);
                        csData.set("OrganizationID", SubjectOrganizationID);
                        csData.set("Message", Message);
                        csData.set("Link", Link);
                        csData.set("VisitorID", VisitorID);
                        csData.set("VisitID", VisitID);
                    }
                }
                //
                return;
                //
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        public static void addSiteActivity(CoreController core, string Message, int SubjectMemberID, int SubjectOrganizationID) {
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
        public static void addSiteWarning(CoreController core, string Name, string shortDescription, string location, int PageID, string Description, string generalKey, string specificKey) {
            int warningId = 0;
            //
            warningId = 0;
            string SQL = "select top 1 ID from ccSiteWarnings"
                + " where (generalKey=" + DbController.encodeSQLText(generalKey) + ")"
                + " and(specificKey=" + DbController.encodeSQLText(specificKey) + ")"
                + "";
            DataTable dt = core.db.executeQuery(SQL);
            if (dt.Rows.Count > 0) {
                warningId = GenericController.encodeInteger(dt.Rows[0]["id"]);
            }
            //
            if (warningId != 0) {
                //
                // increment count for matching warning
                //
                SQL = "update ccsitewarnings set count=count+1,DateLastReported=" + DbController.encodeSQLDate(DateTime.Now) + " where id=" + warningId;
                core.db.executeQuery(SQL);
            } else {
                //
                // insert new record
                //
                using (var csData = new CsModel(core)) {
                    if (csData.insert("Site Warnings")) {
                        csData.set("name", Name);
                        csData.set("description", Description);
                        csData.set("generalKey", generalKey);
                        csData.set("specificKey", specificKey);
                        csData.set("count", 1);
                        csData.set("DateLastReported", DateTime.Now);
                        csData.set("shortDescription", shortDescription);
                        csData.set("location", location);
                        csData.set("pageId", PageID);
                    }
                }
            }
            //
        }
        //
        //====================================================================================================
        /// <summary>
        /// not implemented, as current logging system does not need to be housekeeped. Keep the hood here for the future
        /// </summary>
        /// <param name="core"></param>
        public static void housekeepLogs(CoreController core) {
            try {
                // -- deprecated, NLog handles housekeeping
                return;
            } catch (Exception) { }
        }
        //
        //==========================================================================================
        /// <summary>
        /// Generic handle exception. Determines method name and class of caller from stack. 
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="ex"></param>
        /// <param name="cause"></param>
        /// <param name="stackPtr">How far down in the stack to look for the method error. Pass 1 if the method calling has the error, 2 if there is an intermediate routine.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static void handleException(CoreController core, Exception ex, LogLevel level, string cause, int stackPtr) {
            if (!core._handlingExceptionRecursionBlock) {
                core._handlingExceptionRecursionBlock = true;
                StackFrame frame = new StackFrame(stackPtr);
                System.Reflection.MethodBase method = frame.GetMethod();
                System.Type type = method.DeclaringType;
                string methodName = method.Name;
                string errMsg = type.Name + "." + methodName + ", cause=[" + cause + "], ex=[" + ex.ToString() + "]";
                //
                // append to daily trace log
                //
                LogController.log(core, errMsg, level);
                //
                // add to doc exception list to display at top of webpage
                //
                if (core.doc.errList == null) {
                    core.doc.errList = new List<string>();
                }
                if (core.doc.errList.Count == 10) {
                    core.doc.errList.Add("Exception limit exceeded");
                } else if (core.doc.errList.Count < 10) {
                    core.doc.errList.Add(errMsg);
                }
                //
                core._handlingExceptionRecursionBlock = false;
            }
        }
        //
        //====================================================================================================
        //
        public static void handleError(CoreController core, Exception ex, string cause) {
            handleException(core, ex, LogLevel.Error, cause, 2);
        }
        //
        //====================================================================================================
        //
        public static void handleError(CoreController core, Exception ex) {
            handleException(core, ex, LogLevel.Error, "n/a", 2);
        }
        //
        //====================================================================================================
        //
        public static void handleWarn(CoreController core, Exception ex, string cause) {
            handleException(core, ex, LogLevel.Warn, cause, 2);
        }
        //
        //====================================================================================================
        //
        public static void handleWarn(CoreController core, Exception ex) {
            handleException(core, ex, LogLevel.Warn, "n/a", 2);
        }
        //
        //====================================================================================================
        //
        public static void handleFatal(CoreController core, Exception ex, string cause) {
            handleException(core, ex, LogLevel.Fatal, cause, 2);
        }
        //
        //====================================================================================================
        //
        public static void handleFatal(CoreController core, Exception ex) {
            handleException(core, ex, LogLevel.Fatal, "n/a", 2);
        }
    }
}