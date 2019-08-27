
using System;
using System.Data;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Diagnostics;
using System.Collections.Generic;
using NLog.AWS.Logger;
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
        //public enum LogLevel {
        //    /// <summary>
        //    /// Begin method X, end method X etc
        //    /// </summary>
        //    Trace = 0,
        //    /// <summary>
        //    /// Executed queries, user authenticated, session expired
        //    /// </summary>
        //    Debug = 1,
        //    /// <summary>
        //    /// Normal behavior like mail sent, user updated profile etc.
        //    /// </summary>
        //    Info = 2,
        //    /// <summary>
        //    /// Incorrect behavior but the application can continue
        //    /// </summary>
        //    Warn = 3,
        //    /// <summary>
        //    /// For example application crashes / exceptions.
        //    /// </summary>
        //    Error = 4,
        //    /// <summary>
        //    /// Highest level: important stuff down
        //    /// </summary>
        //    Fatal = 5
        //}
        //
        //=============================================================================
        /// <summary>
        /// configure target "aws" for AWS CloudWatch
        /// </summary>
        /// <param name="core"></param>
        public static void awsConfigure(CoreController core) {
            if (!string.IsNullOrWhiteSpace(core.serverConfig.awsCloudWatchLogGroup)) {
                var awsTarget = new AWSTarget() {
                    Layout = "${longdate}|${level:uppercase=true}|${callsite}|${message}",
                    LogGroup = core.serverConfig.awsCloudWatchLogGroup,
                    Region = core.serverConfig.awsRegionName,
                    Credentials = new Amazon.Runtime.BasicAWSCredentials(core.serverConfig.awsAccessKey, core.serverConfig.awsSecretAccessKey),
                    LogStreamNamePrefix = "abc",
                    LogStreamNameSuffix = "123",
                    MaxQueuedMessages = 5000
                     
                     
                };
                var config = new LoggingConfiguration();
                config.AddTarget("aws", awsTarget);
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, awsTarget));
                LogManager.Configuration = config;
            }
        }
        //
        //=============================================================================
        /// <summary>
        /// v51+ logging - each class has its own NLog logger and calls these methods for uniform output.
        /// use: logger.Log(LogLevel.Info, LogController.getMessageLine( core, "Sample informational message"));
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string getMessageLine(string appName, string message) {
            string threadName = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString("000");
            return "app [" + appName + "], thread [" + threadName + "], " + message.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " ");
        }
        //
        public static string getMessageLine(CoreController core, string message) {
            string appName = ((core.appConfig != null) ? core.appConfig.name : "no-app");
            return getMessageLine(appName, message);
        }
        //
        public static string getMessageLine(CoreController core, Exception ex) {
            return getMessageLine(core, ex.ToString());
        }
        //
        //=============================================================================
        /// <summary>
        /// log Executed queries, user authenticated, session expired
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public static void logDebug(CoreController core, string message) => log(core, message, BaseClasses.CPLogBaseClass.LogLevel.Debug);
        //
        //=============================================================================
        /// <summary>
        /// log application crashes / exceptions
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public static void logError(CoreController core, string message) => log(core, message, BaseClasses.CPLogBaseClass.LogLevel.Error);
        //
        //=============================================================================
        /// <summary>
        /// log highest level, most important messages
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public static void logFatal(CoreController core, string message) => log(core, message, BaseClasses.CPLogBaseClass.LogLevel.Fatal);
        //
        //=============================================================================
        /// <summary>
        /// Normal behavior like mail sent, user updated profile etc
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public static void logInfo(CoreController core, string message) => log(core, message, BaseClasses.CPLogBaseClass.LogLevel.Info);
        //
        //=============================================================================
        /// <summary>
        /// log begin method, end method, etc
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public static void logTrace(CoreController core, string message) => log(core, message, BaseClasses.CPLogBaseClass.LogLevel.Trace);
        //
        //=============================================================================
        /// <summary>
        /// log incorrect behavior but the application can continue
        /// </summary>
        /// <param name="core"></param>
        /// <param name="message"></param>
        /// <remarks></remarks>
        public static void logWarn(CoreController core, string message) => log(core, message, BaseClasses.CPLogBaseClass.LogLevel.Warn);
        //
        //=============================================================================
        /// <summary>
        /// log any level with NLOG without messageLine formatting. Only use in extreme cases where the application environment is not stable.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="messageLine"></param>
        /// <param name="level"></param>
        public static void logRaw(string messageLine, BaseClasses.CPLogBaseClass.LogLevel level) {
            try {
                //
                // -- decouple NLog types from internal enum
                NLog.LogLevel nLogLevel = NLog.LogLevel.Info;
                switch (level) {
                    case BaseClasses.CPLogBaseClass.LogLevel.Trace:
                        nLogLevel = NLog.LogLevel.Trace;
                        break;
                    case BaseClasses.CPLogBaseClass.LogLevel.Debug:
                        nLogLevel = NLog.LogLevel.Debug;
                        break;
                    case BaseClasses.CPLogBaseClass.LogLevel.Info:
                        nLogLevel = NLog.LogLevel.Info;
                        break;
                    case BaseClasses.CPLogBaseClass.LogLevel.Warn:
                        nLogLevel = NLog.LogLevel.Warn;
                        break;
                    case BaseClasses.CPLogBaseClass.LogLevel.Error:
                        nLogLevel = NLog.LogLevel.Error;
                        break;
                    case BaseClasses.CPLogBaseClass.LogLevel.Fatal:
                        nLogLevel = NLog.LogLevel.Fatal;
                        break;
                }
                //
                // -- log with even so we can pass in the type of our wrapper
                Logger nlogLogger = LogManager.GetCurrentClassLogger();
                nlogLogger.Log(typeof(LogController), new LogEventInfo(nLogLevel, nlogLogger.Name, messageLine));
            } catch (Exception ex) {
                string thing1 = ex.ToString();
                string thing2 = thing1;
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
        public static void log(CoreController core, string message, BaseClasses.CPLogBaseClass.LogLevel level) {
            awsConfigure(core);
            string messageLine = getMessageLine(core, message);
            logRaw(messageLine, level);
            //
            // add to doc exception list to display at top of webpage
            //
            if (level < BaseClasses.CPLogBaseClass.LogLevel.Warn) { return; }
            if (core.doc.errorList == null) { core.doc.errorList = new List<string>(); }
            if (core.doc.errorList.Count < 10) {
                core.doc.errorList.Add(messageLine);
                return;
            }
            if (core.doc.errorList.Count == 10) { core.doc.errorList.Add("Exception limit exceeded"); }
        }
        //
        //====================================================================================================
        //
        public static void logError(CoreController core, Exception ex, string cause) {
            log(core, cause + ", exception [" + ex.ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Error);
        }
        //
        //====================================================================================================
        //
        public static void logError(CoreController core, Exception ex) {
            log(core, "exception [" + ex.ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Error);
        }
        //
        //====================================================================================================
        //
        public static void logWarn(CoreController core, Exception ex, string cause) {
            log(core, cause + ", exception [" + ex.ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Warn);
        }
        //
        //====================================================================================================
        //
        public static void logWarn(CoreController core, Exception ex) {
            log(core, "exception [" + ex.ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Warn);
        }
        //
        //====================================================================================================
        //
        public static void logFatal(CoreController core, Exception ex, string cause) {
            log(core, cause + ", exception [" + ex.ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Fatal);
        }
        //
        //====================================================================================================
        //
        public static void logFatal(CoreController core, Exception ex) {
            log(core, "exception [" + ex.ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Fatal);
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
                LogController.logError(core, ex);
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
        ///           issueCategory - a generic string that describes the warning. the warning report
        ///               will display one line for each generalKey (name matches guid)
        ///               like "bad link"
        ///           location - the URL, service or process that caused the problem
        ///               "http://goodpageThankHasBadLink.com"
        ///           pageid - the record id of the bad page.
        ///               "http://goodpageThankHasBadLink.com"
        ///           description - a specific description
        ///               "link to http://www.this.com/pagename was found on http://www.this.com/About-us"
        ///           count - the number of times the name and issueCategory matched. "This error was reported 100 times"
        /// </summary>
        /// <param name="core"></param>
        /// <param name="Name">A generic description of the warning that describes the problem, but if the issue occurs again the name will match, like Page Not Found on /Home</param>
        /// <param name="ignore">To be deprecated - same as name</param>
        /// <param name="location">Where the issue occurred, like on a page, or in a background process.</param>
        /// <param name="PageID"></param>
        /// <param name="Description">Any detail the use will need to debug the problem.</param>
        /// <param name="issueCategory">A general description of the issue that can be grouped in a report, like Page Not Found</param>
        /// <param name="ignore2">to be deprecated, same a name.</param>
        //
        public static void addSiteWarning(CoreController core, string Name, string ignore, string location, int PageID, string Description, string issueCategory, string ignore2) {
            int warningId = 0;
            //
            warningId = 0;
            string SQL = "select top 1 ID from ccSiteWarnings"
                + " where (name=" + DbController.encodeSQLText(Name) + ")"
                + " and(generalKey=" + DbController.encodeSQLText(issueCategory) + ")"
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
                core.db.executeNonQuery(SQL);
            } else {
                //
                // insert new record
                //
                using (var csData = new CsModel(core)) {
                    if (csData.insert("Site Warnings")) {
                        csData.set("name", Name);
                        csData.set("description", Description);
                        csData.set("generalKey", issueCategory);
                        csData.set("count", 1);
                        csData.set("DateLastReported", DateTime.Now);
                        csData.set("location", location);
                        csData.set("pageId", PageID);
                    }
                }
            }
            //
        }
        /// <summary>
        /// Appends a log file in the /alarms folder in program files. The diagnostic monitor should signal a fail.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="cause"></param>
        public static void logAlarm(CoreController core, string cause) {
            LogController.logFatal(core, "logAlarm: " + cause);
            core.programDataFiles.appendFile("Alarms/", "LogAlarm called, cause [" + cause + "]");

        }
    }
}