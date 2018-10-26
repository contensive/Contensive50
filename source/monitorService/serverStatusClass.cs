
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.constants;
using static Contensive.MonitorService.constants;
using static Contensive.BaseClasses.CPFileSystemBaseClass;

namespace Contensive.MonitorService {
    public class StatusServerClass {
        private IpDaemonController cmdListener = new IpDaemonController();
        private CoreController cpCore;
        //
        public static AppLogType[] AppLog;
        public static int AppLogCnt;
        //
        public void startListening() {
            try {
                //
                monitorConfigClass config = new monitorConfigClass(cpCore);
                cmdListener.startListening(this, config.ListenPort);
            } catch (Exception ex) {
                reportError(ex.ToString(), "startListennig");
            }
        }
        //
        public void stopListening() {
            try {
                cmdListener.stopListening();
            } catch (Exception ex) {
                reportError(ex.ToString(), "startListennig");
            }
        }
        //
        //
        //
        public string ipDaemonCallback(string Cmd, string arguments, string RemoteIP) {
            string returnString = "";
            //
            // can never return an error back to the calling routine
            //
            try {
                returnString = GetStatusPage(Cmd, arguments, "1");
            } catch (Exception ex) {
                reportError(ex.ToString(), "ipDaemonCallback");
            }
            return returnString;
        }
        //
        //
        //
        private string StatusLine(int Indent, string Copy) {
            string tempStatusLine = null;
            int Cnt = 0;
            tempStatusLine = Environment.NewLine + "<BR>";
            if (Indent > 0) {
                //todo  NOTE: The ending condition of VB 'For' loops is tested only on entry to the loop. Instant C# has created a temporary variable in order to use the initial value of Indent * 2 for every iteration:
                int tempVar = Indent * 2;
                for (Cnt = 1; Cnt <= tempVar; Cnt++) {
                    tempStatusLine = tempStatusLine + "&nbsp;";
                }
            }
            return tempStatusLine + Copy;
        }
        //
        //
        //
        private string GetStatusPage(string RequestPath, string RequestQuery, string remoteHost) {
            string returnHtml = "";
            try {
                string Content = "";
                double FreeSpace = 0;
                int AppLogPtr = 0;
                string AppName = null;
                double Meg = 0;
                int LoopCount = 0;
                string[] errors = null;
                int ErrorCount = 0;
                int ErrorPtr = 0;
                int LogCnt = 0;
                int LargestLogSize = 0;
                bool ClearAppErrors = false;
                monitorConfigClass monitorConfig = null;
                bool DisplayStatusMethod = false;
                string logMessage = null;
                CPClass cpApp = null;
                CPClass cp = new CPClass("");
                //
                monitorConfig = new monitorConfigClass(cpCore);
                //
                errors = new string[1];
                //
                ClearAppErrors = (RequestPath.ToLowerInvariant().Substring(0, 6) == "/reset");
                DisplayStatusMethod = ClearAppErrors || (RequestPath.ToLowerInvariant().Substring(0, monitorConfig.StatusMethod.Length + 1) == "/" + GenericController.vbLCase(monitorConfig.StatusMethod));
                logMessage = "GetStatusPage hit, RequestPath=" + RequestPath + ", from " + remoteHost;
                if (!(DisplayStatusMethod || ClearAppErrors)) {
                    //
                    // unknown hit
                    //
                    logMessage = logMessage + ", connection reset";
                    //Call IPDaemon1.Disconnect(connId)
                } else {
                    //
                    // status or reset
                    //
                    //Control = New ControlClass
                    //Control = CreateObject("ccCSrvr3.ControlClass")
                    //KernelServices = CreateObject("ccKrnl.KernelServicesClass")
                    //
                    // Check for clear
                    //
                    if (ClearAppErrors) {
                        logMessage = logMessage + ", logs cleared";
                        if (AppLogCnt > 0) {
                            //hint = "Set AppLog().LastCheckOK from /RESET flag"
                            for (AppLogPtr = 0; AppLogPtr < AppLogCnt; AppLogPtr++) {
                                //hint = "Set AppLog().LastCheckOK from /RESET flag, AppLogPtr=" & AppLogPtr
                                AppLog[AppLogPtr].LastCheckOK = true;
                            }
                        }
                    }
                    //
                    // Check for status method
                    //
                    if (DisplayStatusMethod) {
                        //
                        // Commands
                        //
                        logMessage = logMessage + ", status displayed";
                        Content = Content + StatusLine(0, "");
                        Content = Content + StatusLine(0, "Commands");
                        Content = Content + StatusLine(1, "<a href=\"/Reset\">Clear Last Application Errors</a>");
                        //
                        // Parameters
                        //
                        Content = Content + StatusLine(0, "");
                        Content = Content + StatusLine(0, "Parameters");
                        if (ClearAppErrors) {
                            Content = Content + StatusLine(1, "Clear Application Error Flag");
                        }
                        Content = Content + StatusLine(1, "Minimum Drive Space: " + monitorConfig.DiskSpaceMinMb + " MB");
                        Content = Content + StatusLine(1, "Maximum Log Size: " + monitorConfig.LogFileSizeMax + " Bytes");
                        Content = Content + StatusLine(1, "Site Timeout: " + monitorConfig.SiteTimeout + " seconds");
                        Content = Content + StatusLine(1, "Each site must show email activity 1 time per day");
                        Content = Content + StatusLine(1, "Clear Errors every monitor check: " + Convert.ToString(monitorConfig.ClearErrorsOnMonitorHit));
                        Content = Content + StatusLine(1, "Attempt to recover app when error detected: " + Convert.ToString(monitorConfig.allowErrorRecovery));
                        Content = Content + StatusLine(1, "When recovering error, use IIS Reset (not just App Pool recycle): " + Convert.ToString(monitorConfig.allowIISReset));
                        Content = Content + StatusLine(1, "Schedule: " + Convert.ToString(monitorConfig.scheduleList));
                        //
                        // Drive Space
                        //
                        AppName = "";
                        //hint = "Checking drives for space available"
                        Content = Content + StatusLine(0, "");
                        Content = Content + StatusLine(0, "Drive Space Check");
                        Meg = 1024;
                        Meg = Meg * 1024;
                        LoopCount = 0;
                        System.IO.DriveInfo[] drives2 = null;
                        //todo  NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                        //						System.IO.DriveInfo drive2 = null;
                        drives2 = System.IO.DriveInfo.GetDrives();
                        foreach (System.IO.DriveInfo drive2 in drives2) {
                            //hint = "Checking drive " & drive2.Name & " for READY"
                            if (drive2.IsReady) {
                                //hint = "Checking drive " & drive2.Name & " for space available"
                                FreeSpace = Math.Floor((drive2.AvailableFreeSpace / Meg) + 0.5);
                                Content = Content + StatusLine(1, "Drive " + drive2.Name + " FreeSpace " + FreeSpace + " MB");
                                if (FreeSpace < monitorConfig.DiskSpaceMinMb) {
                                    Array.Resize(ref errors, ErrorCount + 1);
                                    errors[ErrorCount] = "ERROR - All ready drives must have at least " + monitorConfig.DiskSpaceMinMb + " MB free";
                                    Content = Content + StatusLine(2, errors[ErrorCount]);
                                    ErrorCount = ErrorCount + 1;
                                }
                            } else {
                                Content = Content + StatusLine(1, "Drive " + drive2.Name + " not Ready");
                            }
                            LoopCount = LoopCount + 1;
                            if (LoopCount > 100) {
                                Array.Resize(ref errors, ErrorCount + 1);
                                errors[ErrorCount] = "ERROR - Drive limit of 100 exceeded.";
                                Content = Content + StatusLine(2, errors[ErrorCount]);
                                ErrorCount = ErrorCount + 1;
                                break;
                            }
                        }
                        //
                        // Log Size
                        //
                        Content = Content + StatusLine(0, "");
                        Content = Content + StatusLine(0, "Log Check");
                        List<FileDetail> LogFileStruct = cpCore.appRootFiles.getFileList(cp.core.serverConfig.programFilesPath + "logs");
                        foreach (FileDetail logFile in LogFileStruct) {
                            if (logFile.Size > LargestLogSize) {
                                LargestLogSize = GenericController.encodeInteger(logFile.Size);
                            }
                            if (logFile.Size > monitorConfig.LogFileSizeMax) {
                                Array.Resize(ref errors, ErrorCount + 1);
                                errors[ErrorCount] = "ERROR - A log file exceeds the limit of " + monitorConfig.LogFileSizeMax + " bytes.";
                                Content = Content + StatusLine(1, errors[ErrorCount]);
                                ErrorCount = ErrorCount + 1;
                                break;
                            }
                        }
                        Content = Content + StatusLine(1, "Logs found " + LogCnt);
                        if (monitorConfig.LogFileSizeMax <= 0) {
                            Content = Content + StatusLine(1, "Max log size " + LargestLogSize + " bytes.");
                        } else {
                            Content = Content + StatusLine(1, "Max log size " + LargestLogSize + " bytes, " + Math.Floor((LargestLogSize * 100) / (double)monitorConfig.LogFileSizeMax) + "% of Maximum.");
                        }
                        //
                        // Alarms Log - if not empty ring alarm and diplay content
                        //
                        Content = Content + StatusLine(0, "");
                        Content = Content + StatusLine(0, "Alarms Log Check");
                        LogFileStruct = cpCore.appRootFiles.getFileList(cpCore.serverConfig.programFilesPath + "\\logs\\alarms");
                        if (!LogFileStruct.Any()) {
                            Content = Content + StatusLine(1, "No alarm logs.");
                        } else {
                            Array.Resize(ref errors, ErrorCount + 1);
                            errors[ErrorCount] = "ERROR - Alarms log folder is not empty.";
                            Content = Content + StatusLine(1, errors[ErrorCount]);
                            ErrorCount = ErrorCount + 1;
                        }
                        //
                        //
                        //
                        if (!monitorConfig.isInSchedule()) {
                            //
                            // unscheduled time
                            //
                            Content = Content + StatusLine(1, "The monitor is not scheduled to run during this period.");
                        } else {
                            //
                            // Check Applications and monitor log
                            //
                            //hint = "Getting application list for Kernel"
                            Content = Content + StatusLine(0, "");
                            Content = Content + StatusLine(0, "Applications");

                            foreach (KeyValuePair<string, AppConfigModel> kvp in cpCore.serverConfig.apps) {
                                AppName = kvp.Value.name;
                                cpApp = new CPClass(AppName);
                                if (cpApp.core.appConfig.allowSiteMonitor) {


                                    //hint = "Checking status for application [" & AppName & "]"
                                    switch (cpApp.core.appConfig.appStatus) {
                                        //Case appConfigModel.appStatusEnum.errorConnectionObjectFailure
                                        //    ReDim Preserve errors(ErrorCount)
                                        //    errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned Connection Object failure")
                                        //    Content = Content & StatusLine(2, errors(ErrorCount))
                                        //    ErrorCount = ErrorCount + 1
                                        //Case appConfigModel.appStatusEnum.errorConnectionStringFailure
                                        //    ReDim Preserve errors(ErrorCount)
                                        //    errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned Connection String Failure")
                                        //    Content = Content & StatusLine(2, errors(ErrorCount))
                                        //    ErrorCount = ErrorCount + 1
                                        //Case appConfigModel.appStatusEnum.errorDataSourceFailure
                                        //    ReDim Preserve errors(ErrorCount)
                                        //    errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned Data Source Failure")
                                        //    Content = Content & StatusLine(2, errors(ErrorCount))
                                        //    ErrorCount = ErrorCount + 1
                                        //Case appConfigModel.appStatusEnum.errorDbBad
                                        //    ReDim Preserve errors(ErrorCount)
                                        //    errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned Bad Database")
                                        //    Content = Content & StatusLine(2, errors(ErrorCount))
                                        //    ErrorCount = ErrorCount + 1
                                        //Case appConfigModel.appStatusEnum.errorDbNotFound
                                        //    ReDim Preserve errors(ErrorCount)
                                        //    errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned Database Failure")
                                        //    Content = Content & StatusLine(2, errors(ErrorCount))
                                        //    ErrorCount = ErrorCount + 1
                                        //Case appConfigModel.appStatusEnum.errorKernelFailure
                                        //    ReDim Preserve errors(ErrorCount)
                                        //    errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned Kernel Failure")
                                        //    Content = Content & StatusLine(2, errors(ErrorCount))
                                        //    ErrorCount = ErrorCount + 1
                                        //Case appConfigModel.appStatusEnum.errorLicenseFailure
                                        //    ReDim Preserve errors(ErrorCount)
                                        //    errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned License Failure")
                                        //    Content = Content & StatusLine(2, errors(ErrorCount))
                                        //    ErrorCount = ErrorCount + 1
                                        //Case appConfigModel.appStatusEnum.errorNoHostService
                                        //    ReDim Preserve errors(ErrorCount)
                                        //    errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned ccServer Service is not running")
                                        //    Content = Content & StatusLine(2, errors(ErrorCount))
                                        //    ErrorCount = ErrorCount + 1
                                        //Case appConfigModel.appStatusEnum.notFound
                                        //    ReDim Preserve errors(ErrorCount)
                                        //    errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned application not found")
                                        //    Content = Content & StatusLine(2, errors(ErrorCount))
                                        //    ErrorCount = ErrorCount + 1
                                        //Case appConfigModel.appStatusEnum.notEnabled
                                        //    ReDim Preserve errors(ErrorCount)
                                        //    errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned application not running")
                                        //    Content = Content & StatusLine(2, errors(ErrorCount))
                                        //    ErrorCount = ErrorCount + 1
                                        //        'Case ccCommonModule.applicationStatusEnum.ApplicationStatusStarting
                                        //        '    ReDim Preserve errors(ErrorCount)
                                        //        '    errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned application starting")
                                        //        '    Content = Content & StatusLine(2, errors(ErrorCount))
                                        //        '    ErrorCount = ErrorCount + 1
                                        case AppConfigModel.AppStatusEnum.ok:
                                            if (true) {
                                                //
                                                // access content server for this application
                                                //
                                                if (true) {
                                                    string link;
                                                    //hint = "Returning monitor log entry for application [" & AppName & "]"
                                                    link = AppLog[AppLogPtr].SiteLink;
                                                    Content = Content + StatusLine(2, "Link [<a target=_blank href=\"" + link + "\">" + link + "</a>]");
                                                    link = AppLog[AppLogPtr].StatusLink;
                                                    Content = Content + StatusLine(2, "status Link [<a target=_blank href=\"" + link + "\">" + link + "</a>]");
                                                    Content = Content + StatusLine(2, "status Checks [" + AppLog[AppLogPtr].StatusCheckCount + "]");
                                                    Content = Content + StatusLine(2, "status Response [" + AppLog[AppLogPtr].LastStatusResponse + "]");
                                                    if (!(AppLog[AppLogPtr].LastCheckOK)) {
                                                        Array.Resize(ref errors, ErrorCount + 1);
                                                        errors[ErrorCount] = ("[" + AppName + "] ERROR - Last status check returned [" + AppLog[AppLogPtr].LastStatusResponse + "]");
                                                        ErrorCount = ErrorCount + 1;
                                                    }
                                                    if (AppLog[AppLogPtr].ErrorCount != 0) {
                                                        if (monitorConfig.ClearErrorsOnMonitorHit) {
                                                            Content = Content + StatusLine(2, "errors detected since last monitor hit [" + AppLog[AppLogPtr].ErrorCount + "]");
                                                            AppLog[AppLogPtr].ErrorCount = 0;
                                                            AppLog[AppLogPtr].LastCheckOK = true;
                                                        } else {
                                                            Content = Content + StatusLine(2, "errors detected since monitor started [" + AppLog[AppLogPtr].ErrorCount + "]");
                                                        }
                                                    } else {
                                                        Content = Content + StatusLine(2, "no monitor errors detected");
                                                    }
                                                }
                                                //
                                                // Check Last Email drop
                                                //
                                                DateTime EmailServicelastCheck = cp.Site.GetDate("EmailServicelastCheck");
                                                if (EmailServicelastCheck < (DateTime.Now.AddDays(-1))) {
                                                    //
                                                    // Email has not sent in a day
                                                    //
                                                    Array.Resize(ref errors, ErrorCount + 1);
                                                    errors[ErrorCount] = ("[" + AppName + "] ERROR - Group email drop has not sent in the last 24 hours");
                                                    Content = Content + StatusLine(2, errors[ErrorCount]);
                                                    ErrorCount = ErrorCount + 1;
                                                } else {
                                                    //
                                                    // Email send is OK
                                                    //
                                                    Content = Content + StatusLine(2, "Group email drop OK [" + EmailServicelastCheck + "]");
                                                }
                                                //
                                                // Get Misc properties
                                                //
                                                //hint = "Get AppServices object for [" & AppName & "]"
                                                Content = Content + StatusLine(2, "Contensive Version [" + cp.Version + "]");
                                                Content = Content + StatusLine(2, "Data Build Version [" + cpCore.siteProperties.dataBuildVersion + "]");
                                                Content = Content + StatusLine(2, "Active Connections [" + "AppServices.ConnectionsActive" + "]");
                                                Content = Content + StatusLine(2, "Started [" + "AppServices.DateStarted" + "]");
                                                Content = Content + StatusLine(2, "Hits [" + "AppServices.HitCounter" + "]");
                                                Content = Content + StatusLine(2, "Errors [" + "AppServices.ErrorCount" + "]");
                                                //
                                                cp.Dispose();
                                                cp = null;
                                            }
                                            //Case ccCommonModule.applicationStatusEnum.ApplicationStatusPaused
                                            //    '
                                            //    ' Paused
                                            //    '
                                            //    Content = Content & StatusLine(2, "Application is paused")
                                            break;
                                        default:
                                            Array.Resize(ref errors, ErrorCount + 1);
                                            errors[ErrorCount] = ("[" + AppName + "] ERROR - Contensive returned unknown error");
                                            Content = Content + StatusLine(2, errors[ErrorCount]);
                                            ErrorCount = ErrorCount + 1;
                                            break;
                                    }
                                }
                            }
                        }
                        //
                        // Return page
                        //
                        if (ErrorCount > 0) {
                            returnHtml = ""
                                + Environment.NewLine + "<PRE>"
                                + Environment.NewLine + "ERRORS " + ErrorCount;
                            for (ErrorPtr = 0; ErrorPtr < ErrorCount; ErrorPtr++) {
                                returnHtml += StatusLine(1, errors[ErrorPtr]);
                            }
                            returnHtml += ""
                                + Environment.NewLine + "<BR>"
                                + Environment.NewLine + Content + Environment.NewLine + "</PRE>"
                                + "";
                        } else {
                            returnHtml = ""
                                + Environment.NewLine + "<PRE>"
                                + Environment.NewLine + "ERRORS 0"
                                + Environment.NewLine + "<BR>"
                                + Environment.NewLine + Content + Environment.NewLine + "</PRE>"
                                + "";

                        }
                    }
                }
                appendMonitorLog(logMessage);
            } catch (Exception ex) {
                reportError(ex.ToString(), "getStatusPage");
            }
            return returnHtml;
        }
        //
        //
        //
        private void appendMonitorLog(string message) {
            LogController.logError(cpCore, "Monitor-serverStatus:" + message);
        }
        //
        //
        //
        private void reportError(string exToString, string methodName) {
            appendMonitorLog("unexepected error in " + methodName + ", " + exToString);
        }

        public StatusServerClass(CoreController cpCore) : base() {
            this.cpCore = cpCore;
        }
    }
}
