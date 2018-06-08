
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

using Contensive.Processor;
using static Contensive.Processor.constants;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using System.Reflection;
using System.Timers;
using System.Threading;
using Contensive.Processor.Models.Complex;
using Contensive.Processor.Models.Context;

namespace Contensive.MonitorService {
    public class siteCheckClass {
        //========================================================================
        //
        // This page and its contents are copyright by Kidwell McGowan Associates.
        //
        //==================================================================================================
        //
        //   Timer runs while the form is loaded
        //       DisplayStatus runs on Timer
        //   NTService is installed with -install option
        //   NTService is uninstalled with teh -uninstall option
        //   NTService Start event starts ContentServer features
        //   NTService Pause and stop stops the contentserver
        //
        //==================================================================================================
        //
        // ----- global scope variables
        //
        private coreController cpCore;
        private const string serviceDisplayName = "Contensive Monitor";
        private int TimerIntervalSec;
        private bool allowErrorRecovery;
        private bool allowIISReset;
        private int DiskSpaceMinMb;
        private bool ClearErrorsOnMonitorHit;
        private string AlarmEmailList;
        private string AlarmEmailServer;
        private string ServerName;
        private int LogFileSizeMax;
        private int SiteTimeout;
        private string ScheduleList;
        private int ListenPort;
        private string StatusMethod;
        private string version;
        //
        //------------------------------------------------------------------------------------------
        //   Timer
        //------------------------------------------------------------------------------------------
        //
        private const int TimerMsecPerTick = 1000;
        private System.Timers.Timer processTimer = new System.Timers.Timer(10000);
        private int ProcessTimerTickCnt;
        private DateTime ServiceStartTime;
        //
        //------------------------------------------------------------------------------------------
        //   Process status flags
        //------------------------------------------------------------------------------------------
        //
        private bool ServiceInProcess;
        private bool AbortProcess;
        private int HTTPLastError;
        private const int DocLengthMax = 500000;
        //
        //------------------------------------------------------------------------------------------
        //   HTTP Response
        //------------------------------------------------------------------------------------------
        //
        private double HTTPResponseTime; 
        private const int HTTPResponseHeaderCountMax = 100;
        //
        //=========================================================================================
        //
        private void HandleMonitorError(string MethodName, string ErrorMessage) {
            //
            appendMonitorLog("siteMonitorClass." + MethodName + "[" + ErrorMessage + "]");
        }
        //
        //=========================================================================================
        //   Use the MS Winsock to get a page and return it in the page buffer
        //=========================================================================================
        //
        private string GetDoc(string Link, string AppName, int RequestTimeout, ref bool return_needsErrorRecovery) {
            string tempGetDoc = null;
            try {
                //
                httpRequestController kmaHTTP = new httpRequestController();
                string URLWorking = null;
                Stopwatch stopWatch = new Stopwatch();
                //
                //   Get the document
                //
                return_needsErrorRecovery = false;
                kmaHTTP.userAgent = "Contensive Monitor";
                HTTPLastError = 0;
                URLWorking = Contensive.Processor.Controllers.genericController.encodeURL(Link);
                kmaHTTP.timeout = RequestTimeout;
                tempGetDoc = kmaHTTP.getURL(ref URLWorking);
                HTTPLastError = 0;
                HandleMonitorError("MonitorForm", "GetDoc getting URL [" + Link + "]");
                switch (HTTPLastError) {
                    case 20302:
                        appendMonitorLog("GetDoc(" + URLWorking + ") returned error [" + HTTPLastError + " - timeout waiting for server]");
                        return_needsErrorRecovery = true;
                        break;
                    case 25061:
                        appendMonitorLog("GetDoc(" + URLWorking + ") returned error [" + HTTPLastError + " - timeout waiting for server]");
                        return_needsErrorRecovery = true;
                        break;
                    case 25065:
                        appendMonitorLog("GetDoc(" + URLWorking + ") returned error [" + HTTPLastError + " - host is down]");
                        return_needsErrorRecovery = true;
                        break;
                    case 26002:
                        appendMonitorLog("GetDoc(" + URLWorking + ") returned error [" + HTTPLastError + " - host not found]");
                        break;
                    case 25069:
                    case 25068:
                        appendMonitorLog("GetDoc(" + URLWorking + ") returned error [" + HTTPLastError + " - server too busy]");
                        return_needsErrorRecovery = true;
                        break;
                    case 26005:
                        appendMonitorLog("GetDoc(" + URLWorking + ") returned error [" + HTTPLastError + " - invalid domain name]");
                        break;
                    default:
                        appendMonitorLog("GetDoc(" + URLWorking + ") returned error [" + HTTPLastError + " - error requesting document]");
                        return_needsErrorRecovery = true;
                        break;
                }
                HTTPResponseTime = stopWatch.ElapsedMilliseconds / 1000.0;
                //
                return tempGetDoc;
                //
                // ----- Error Trap
                //
            } catch {
                goto ErrorTrap;
            }
            ErrorTrap:
            //kmaHTTP = null;
            HandleMonitorError("GetDoc", "getting URL [" + Link + "]");
            return tempGetDoc;
        }
        //
        //======================================================================================
        //   Log a reported error
        //======================================================================================
        //
        private void SaveToLogFile(string MethodName, string AppName, string LogCopy) {
            try {
                //
                appendMonitorLog(getAppExeName() + ".MonitorForm." + MethodName + "[" + AppName + "], " + LogCopy);
                //
                return;
                //
            } catch {
                goto ErrorTrap;
            }
            ErrorTrap:
            HandleMonitorError("SaveToLogFile", "TrapError");
        }
        //
        //======================================================================================
        //   Log a reported error
        //======================================================================================
        //
        public void SaveToEventLogAndLogFile(string MethodName, string AppName, string LogCopy) {
            try {
                //
                //Call App.LogEvent(LogCopy)
                SaveToLogFile(MethodName, AppName, LogCopy);
                // Call NTService.LogEvent(NTServiceEventInformation, NTServiceIDInfo, LogCopy)
                return;
                //
            } catch {
                goto ErrorTrap;
            }
            ErrorTrap:
            HandleMonitorError("SaveToEventLogAndLogFile", "TrapError");
        }
        //
        //
        //
        public void StartMonitoring() {
            try {
                //
                // convert to cluster-level object, then do applicaiton work by enumerating applications and using cp for each app
                monitorConfigClass config = new monitorConfigClass(cpCore);
                //
                AlarmEmailList = config.AlarmEmailList;
                AlarmEmailServer = config.AlarmEmailServer;
                ClearErrorsOnMonitorHit = config.ClearErrorsOnMonitorHit;
                DiskSpaceMinMb = config.DiskSpaceMinMb;
                allowErrorRecovery = config.allowErrorRecovery;
                allowIISReset = config.allowIISReset;
                LogFileSizeMax = config.LogFileSizeMax;
                ScheduleList = config.scheduleList;
                ServerName = config.ServerName;
                SiteTimeout = config.SiteTimeout;
                TimerIntervalSec = config.TimerIntervalSec;
                ListenPort = config.ListenPort;
                StatusMethod = config.StatusMethod;
                //
                ServiceStartTime = DateTime.Now;
                AbortProcess = false;
                //
                // Setup Timer
                //
                ServiceInProcess = false;
                processTimer.Interval = TimerMsecPerTick;
                processTimer.Enabled = true;
                SaveToLogFile("StartMonitoring", "-", serviceDisplayName + " Started");
                config.Save();
                return;
            } catch {
                goto ErrorTrap;
            }
            ErrorTrap:
            HandleMonitorError("StartMonitoring", "TrapError");
        }
        //
        //   Begin the stop service action.
        //       It can complete by 3 different ways
        //       1 - if it is not busy, call complete from here
        //       2 - if busy and it shuts down correctly, complete is at end of GetDoc
        //       3 - if busy and TopTimeoutTimer goes off, abort GetDoc
        //
        public void StopMonitoring() {
            try {
                processTimer.Enabled = false;
                SaveToLogFile("StopMonitoring", "-", "Stopping " + serviceDisplayName);
                AbortProcess = true;
            } catch {
            }
        }
        //
        //==========================================================================================
        //   Event Timer Tick
        //==========================================================================================
        //
        public void ProcessTimerTick(object source, ElapsedEventArgs e) {
            try {
                //
                string AlarmEmailBody = null;
                //Dim Mailer As New SMTP5Class
                string[] Emails = null;
                int Ptr = 0;
                string ToAddress = null;
                CPClass cp = null;
                monitorConfigClass config = null;
                var appLog = new constants.AppLogType[]   { };
                int appLogCnt = 0;
                //
                ProcessTimerTickCnt = ProcessTimerTickCnt + 1;
                if ((!AbortProcess) && (ProcessTimerTickCnt >= TimerIntervalSec)) {
                    if (ServiceInProcess) {
                        //
                        // Overlapping request - ignore it and reset count
                        //
                        ProcessTimerTickCnt = 0;
                        appendMonitorLog("Monitor is attempting a check, but a previous check is not complete. Timer will be reset.");
                    } else {
                        //
                        // service the request
                        //
                        ProcessTimerTickCnt = ProcessTimerTickCnt - TimerIntervalSec;
                        ServiceInProcess = true;
                        // convert to cluster-level object, then do applicaiton work by enumerating applications and using cp for each app
                        cp = new CPClass();
                        config = new monitorConfigClass(cpCore);
                        if (!config.isInSchedule()) {
                            //
                            // not scueduled
                            //
                            appendMonitorLog("Monitor is not schedule for testing now, " + DateTime.Now);
                        } else {
                            AlarmEmailBody = monitorAllSites_returnEmailBody(appLog, appLogCnt);
                            //
                            // send alarm email if there was a problem
                            //
                            if (!string.IsNullOrEmpty(AlarmEmailBody)) {
                                appendMonitorLog("Monitor detected a problem");
                                if (string.IsNullOrEmpty(AlarmEmailServer)) {
                                    appendMonitorLog("Alarm not sent because AlarmEmailServer is not configured in " + cpCore.programDataFiles.localAbsRootPath + "config\\MonitorConfig.txt");
                                } else {
                                    if (string.IsNullOrEmpty(AlarmEmailList)) {
                                        appendMonitorLog("Alarm not sent because AlarmEmailList is not configured in " + cpCore.programDataFiles.localAbsRootPath + "config\\MonitorConfig.txt");
                                    } else {
                                        AlarmEmailBody = ""
                                            + "Errors Found on Server [" + ServerName + "]"
                                            + Environment.NewLine + AlarmEmailBody;
                                        Emails = AlarmEmailList.Split(',');
                                        for (Ptr = 0; Ptr <= Emails.GetUpperBound(0); Ptr++) {
                                            ToAddress = Emails[Ptr];
                                            if (!string.IsNullOrEmpty(ToAddress)) {
                                                //emailstatus = Mailer.sendEmail5(AlarmEmailServer, Emails(Ptr), "Monitor@Contensive.com", "Monitor Alarm [" & ServerName & "]", AlarmEmailBody)
                                                //Call appendMonitorLog("Sending Alarm Notification to " & ToAddress & ", status=" & emailstatus)
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        ServiceInProcess = false;
                        cp.Dispose();
                    }
                }
                return;
            } catch {
                goto ErrorTrap;
            }
            ErrorTrap:
            ServiceInProcess = false;
            HandleMonitorError("ProcessTimerTick", "TrapError");
        }
        //
        //==========================================================================================
        //   Returns "" if no errors
        //   Returns the AlarmEmail message if there were errors
        //==========================================================================================
        //
        private string monitorAllSites_returnEmailBody(constants.AppLogType[] AppLog, int AppLogCnt) {
            string tempmonitorAllSites_returnEmailBody = null;
            try {
                //
                bool needsErrorRecovery = false;
                string AppList = null;
                string[] AppLine = null;
                int AppCnt = 0;
                int AppPtr = 0;
                string[] AppDetails = null;
                int AppDetailsCnt = 0;
                string AppName = null;
                int AppStatus = 0;
                string DomainName = null;
                string[] ListSplit = null;
                string Response = null;
                string AppRootPath = null;
                string DomainNameList = null;
                string DefaultPageName = null;
                string ResponseStatusLine = null;
                int AppLogPtr = 0;
                string ErrorMessageResponse = null;
                //
                tempmonitorAllSites_returnEmailBody = "";
                if (string.IsNullOrEmpty(AppList)) {
                    //
                    // Problem
                    tempmonitorAllSites_returnEmailBody = tempmonitorAllSites_returnEmailBody + Environment.NewLine + " Error, no Contensive Applications were found";
                    appendMonitorLog("GetApplicationList call returned no Contensive Applications were found");
                } else {
                    AppLine = genericController.stringSplit(AppList, Environment.NewLine );
                    AppCnt = AppLine.GetUpperBound(0) + 1;
                    //hint = ""
                    while (AppPtr < AppCnt && (!AbortProcess)) {
                        AppDetails = genericController.stringSplit(AppLine[AppPtr], "\t" );
                        AppDetailsCnt = AppDetails.GetUpperBound(0) + 1;
                        if (AppDetailsCnt < AppListCount) {
                            //
                            // Problem - can not monitor
                            //
                            appendMonitorLog("GetApplicationList call return less than " + AppListCount + " arguments, aborting monitor pass for this application [" + AppLine[AppPtr] + "]");
                        } else {
                            AppName = AppDetails[AppList_Name];
                            if (!encodeBoolean(AppDetails[AppList_AllowSiteMonitor])) {
                                //
                                // Monitoring Disabled
                                //
                                appendMonitorLog(AppName + " monitoring disabled");
                            } else {
                                //
                                // Monitor site
                                //
                                AppStatus = encodeInteger(AppDetails[AppList_Status]);
                                if (false) {
                                    //
                                    //ElseIf AppStatus = applicationStatusEnum.ApplicationStatusPaused Then
                                    //    '
                                    //    ' Paused
                                    //    '
                                    //    Call appendMonitorLog(AppName & " paused")
                                } else if (AppStatus == (int)appConfigModel.appStatusEnum.ok) {
                                    //
                                    // Running
                                    //
                                    if (AppLogCnt > 0) {
                                        for (AppLogPtr = 0; AppLogPtr < AppLogCnt; AppLogPtr++) {
                                            if (AppLog[AppLogPtr].Name == AppName) {
                                                break;
                                            }
                                        }
                                    }
                                    if (AppLogPtr >= AppLogCnt) {
                                        //
                                        // App not found in log, add a new log entry
                                        //
                                        AppLogPtr = AppLogCnt;
                                        AppLogCnt = AppLogCnt + 1;
                                        //todo  TASK: The following 'ReDim' could not be resolved. A possible reason may be that the object of the ReDim was not declared as an array:
                                        //ReDim Preserve AppLog[AppLogPtr];
                                        AppLog[AppLogPtr].Name = AppName;
                                    }
                                    //
                                    // Check Site
                                    //
                                    AppLog[AppLogPtr].LastStatusResponse = "";
                                    AppLog[AppLogPtr].LastCheckOK = true;
                                    DomainName = "";
                                    DomainNameList = AppDetails[AppList_DomainName];
                                    if (!string.IsNullOrEmpty(DomainNameList)) {
                                        ListSplit = DomainNameList.Split(',');
                                        DomainName = ListSplit[0];
                                    }
                                    DefaultPageName = AppDetails[AppList_DefaultPage];
                                    AppRootPath = AppDetails[AppList_RootPath];
                                    if (string.IsNullOrEmpty(DomainName)) {
                                        AppLog[AppLogPtr].LastCheckOK = false;
                                        tempmonitorAllSites_returnEmailBody = tempmonitorAllSites_returnEmailBody + Environment.NewLine + "Contensive application [" + AppName + "] has no valid domain name";
                                        AppLog[AppLogPtr].StatusCheckCount = AppLog[AppLogPtr].StatusCheckCount + 1;
                                        AppLog[AppLogPtr].ErrorCount = AppLog[AppLogPtr].ErrorCount + 1;
                                        appendMonitorLog(AppName + " has no valid domain name");
                                    } else {
                                        if (vbInstr(1, DomainName, "http://", 1) == 0) {
                                            DomainName = "http://" + DomainName;
                                        }
                                        DomainName = DomainName + AppRootPath + DefaultPageName;
                                        AppLog[AppLogPtr].SiteLink = DomainName;
                                        DomainName = DomainName + "?method=status";
                                        AppLog[AppLogPtr].StatusLink = DomainName;
                                        ErrorMessageResponse = "";
                                        AppLog[AppLogPtr].StatusCheckCount = AppLog[AppLogPtr].StatusCheckCount + 1;
                                        //
                                        // test
                                        //
                                        Response = GetDoc(DomainName, AppName, SiteTimeout, ref needsErrorRecovery);
                                        ResponseStatusLine = getLine(ref Response);
                                        AppLog[AppLogPtr].LastStatusResponse = ResponseStatusLine;
                                        if (vbInstr(1, ResponseStatusLine, "Contensive OK", 1) == 1) {
                                            //
                                            // no error
                                            //
                                            appendMonitorLog(AppName + " returned OK");
                                            AppLog[AppLogPtr].LastCheckOK = true;
                                        } else {
                                            //
                                            // error
                                            //
                                            ErrorMessageResponse = ResponseStatusLine;
                                            appendMonitorLog(AppName + " returned Error [" + ErrorMessageResponse + "]");
                                            if (false) {
                                                //
                                                // this is a problem -- an internal contensive error problem would not be recovered (?)
                                                //   ACNM errored for 30 minutes before someone restarted.
                                                //
                                                //                                If Not needsErrorRecovery Then
                                                //                                    '
                                                //                                    ' This error does not require recovery
                                                //                                    '
                                                //                                    AppLog[AppLogPtr].LastCheckOK = False
                                                //                                    AppLog[AppLogPtr].ErrorCount = AppLog[AppLogPtr].ErrorCount + 1
                                                //                                    monitorAllSites_returnEmailBody = monitorAllSites_returnEmailBody _
                                                //                                        & vbCrLf & DomainName & " Error [" & ErrorMessageResponse & "], no error recovery run because this error would not be affected."
                                                //                                    Call AppendMonitorLog("This error does not require recovery")
                                            } else if (!allowErrorRecovery) {
                                                //
                                                // recoverable  error, but no error recovery allowed
                                                //
                                                AppLog[AppLogPtr].LastCheckOK = false;
                                                AppLog[AppLogPtr].ErrorCount = AppLog[AppLogPtr].ErrorCount + 1;
                                                if (!allowIISReset) {
                                                    //
                                                    // no reset allowed
                                                    //
                                                    tempmonitorAllSites_returnEmailBody = tempmonitorAllSites_returnEmailBody + Environment.NewLine + DomainName + " Error [" + ErrorMessageResponse + "], error recovery and iisreset disabled, abort testing and setting alarm.";
                                                    appendMonitorLog("Error recovery and iisreset disabled, aborting test and setting alarm.");
                                                } else {
                                                    //
                                                    // iis reset and abort
                                                    //
                                                    tempmonitorAllSites_returnEmailBody = tempmonitorAllSites_returnEmailBody + Environment.NewLine + DomainName + " Error [" + ErrorMessageResponse + "], error recovery disabled but iisreset disabled, calling iisreset, setting alarm and aborting test.";
                                                    appendMonitorLog("Recovery failed, allowIISReset true, flag monitor error, calling iisreset, setting alarm and aborting test.");
                                                    recoverError(allowErrorRecovery, true, AppName);
                                                    break;
                                                }
                                            } else {
                                                //
                                                // recoverable error, attempt app pool recycle and retry
                                                //
                                                appendMonitorLog("Attempting app pool recycle and retry");
                                                recoverError(true, false, AppName);
                                                Response = GetDoc(DomainName, AppName, SiteTimeout, ref needsErrorRecovery);
                                                ResponseStatusLine = getLine(ref Response);
                                                AppLog[AppLogPtr].LastStatusResponse = "[" + AppLog[AppLogPtr].LastStatusResponse + "], after recovery [" + ResponseStatusLine + "]";
                                                if (vbInstr(1, ResponseStatusLine, "Contensive OK", 1) == 1) {
                                                    //
                                                    // recovered, continue without error
                                                    //
                                                    AppLog[AppLogPtr].LastCheckOK = true;
                                                    appendMonitorLog("Recycle and retry returned OK, no alarm.");
                                                } else {
                                                    //
                                                    // did not recover, try iisreset and abort
                                                    //
                                                    AppLog[AppLogPtr].LastCheckOK = false;
                                                    AppLog[AppLogPtr].ErrorCount = AppLog[AppLogPtr].ErrorCount + 1;
                                                    if (!allowIISReset) {
                                                        //
                                                        // no reset allowed
                                                        //
                                                        tempmonitorAllSites_returnEmailBody = tempmonitorAllSites_returnEmailBody + Environment.NewLine + DomainName + " Error [" + ErrorMessageResponse + "], recycle failed to recover, IISReset disabled and alarm set.";
                                                        appendMonitorLog("Recovery failed, allowIISReset false, flag monitor error, skip app and continue testing.");
                                                    } else {
                                                        //
                                                        // iis reset and abort
                                                        //
                                                        tempmonitorAllSites_returnEmailBody = tempmonitorAllSites_returnEmailBody + Environment.NewLine + DomainName + " Error [" + ErrorMessageResponse + "], recycle failed to recover, IISReset called and alarm set.";
                                                        appendMonitorLog("Recovery failed, allowIISReset true, flag monitor error, iisreset and abort pass.");
                                                        recoverError(allowErrorRecovery, true, AppName);
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                } else {
                                    appendMonitorLog(AppName + " is not running, ApplicationStatus=" + AppDetails[1]);
                                }
                            }
                        }
                        AppPtr = AppPtr + 1;
                    }
                }
                //
                if (!string.IsNullOrEmpty(tempmonitorAllSites_returnEmailBody)) {
                    tempmonitorAllSites_returnEmailBody = tempmonitorAllSites_returnEmailBody.Substring(2);
                }
                //
                return tempmonitorAllSites_returnEmailBody;
            } catch {
                goto ErrorTrap;
            }
            ErrorTrap:
            ServiceInProcess = false;
            HandleMonitorError("monitorAllSites_returnEmailBody", "TrapError");
            //todo  TASK: Calls to the VB 'Err' function are not converted by Instant C#:
            //Microsoft.VisualBasic.Information.Err().Clear();
            return tempmonitorAllSites_returnEmailBody;
        }
        //'
        //'==========================================================================================
        //'
        //'==========================================================================================
        //'
        //Private Function IsSiteOK(DomainNameList As String, DefaultPageName As String, AppName As String) As Integer
        //    On Error GoTo ErrorTrap
        //    '
        //    Dim DomainName As String
        //    Dim ListSplit() As String
        //    Dim Response As String
        //    Dim ErrorMessageResponse As String
        //    '
        //    ListSplit = Split(DomainNameList, ",")
        //    DomainName = ListSplit(0)
        //    If DomainName <> "" Then
        //        If genericController.vbInstr(1, DomainName, "http://", 1) = 0 Then
        //            DomainName = "http://" & DomainName
        //        End If
        //        DomainName = DomainName & "/" & DefaultPageName & "?method=status"
        //        Response = GetDoc(DomainName, AppName, SiteTimeout, ErrorMessageResponse)
        //        If genericController.vbInstr(1, Response, "Contensive OK", vbTextCompare) <> 0 Then
        //            IsSiteOK = True
        //        Else
        //            IsSiteOK = False
        //        End If
        //    End If
        //
        //    '
        //    Exit Function
        //ErrorTrap:
        //    ServiceInProcess = False
        //    Call HandleMonitorError("IsSiteOK", "TrapError")
        //    Err.Clear
        //End Function
        //
        //==========================================================================================
        //
        //==========================================================================================
        //
        private void recoverError(bool allowErrorRecovery, bool allowIISReset, string appPoolName) {
            try {
                string Cmd = null;
                //Dim Filename As String
                //Dim Content As String
                //
                // Run IISReset
                //
                if (!allowErrorRecovery) {
                    //
                    // error recover not enabled
                    //
                    appendMonitorLog("Error condition detected but no recovery attempted because AllowErrorRecover is false.");
                } else {
                    if (allowIISReset || (string.IsNullOrEmpty(appPoolName))) {
                        //
                        // iisreset the server
                        //
                        if ((!allowIISReset) && (string.IsNullOrEmpty(appPoolName))) {
                            appendMonitorLog("Error condition detected and IISReset called to recover, allowIISReset is false but AppName is blank");
                        } else {
                            appendMonitorLog("Error condition detected and IISReset called to recover.");
                        }
                        Cmd = "%comspec% /c IISReset";
                        try {
                            genericController.executeCommandSync(Cmd);
                        } catch (Exception ex) {
                            appendMonitorLog("IISReset attempt failed, error=" + ex.ToString());
                        }
                    } else {
                        //
                        // recycle the app
                        //
                        appendMonitorLog("attempting IIS app pool recycle on " + appPoolName + ".");
                        Cmd = "%comspec% /c %systemroot%\\system32\\inetsrv\\appcmd recycle appPool " + appPoolName;
                        try {
                            genericController.executeCommandSync(Cmd);
                        } catch (Exception ex) {
                            appendMonitorLog("IIS app pool recycle attempt failed, error=" + ex.ToString());
                        }
                    }
                }
            } catch (Exception ex) {
                appendMonitorLog("recoverError, unexpected exception=" + ex.ToString());
            } finally {
                ServiceInProcess = false;
            }
        }
        //
        //
        //
        private void appendMonitorLog(string message) {
            logController.logError(cpCore, "Monitor-siteCheck:" + message);
        }
        //
        // ====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cpCore"></param>
        public siteCheckClass(coreController cpCore) : base() {
            this.cpCore = cpCore;
            version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            processTimer.Elapsed += ProcessTimerTick;
        }
    }
}