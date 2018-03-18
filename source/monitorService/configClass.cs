
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

using Contensive.Core;
using static Contensive.Core.constants;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;

namespace Contensive.WindowsServices {
    public class monitorConfigClass {
        //
        public const int Port_SiteMonitorDefault = 4532;
        //
        private coreController cpCore;
        //
        public int TimerIntervalSec;
        public bool allowErrorRecovery;
        public bool allowIISReset;
        public int DiskSpaceMinMb;
        public bool ClearErrorsOnMonitorHit;
        public string AlarmEmailList;
        public string AlarmEmailServer;
        public string ServerName;
        public int LogFileSizeMax;
        public int SiteTimeout;
        public string scheduleList;
        public int ListenPort;
        public string StatusMethod;
        public string httpStatusOnError;
        //
        //
        //
        public monitorConfigClass(coreController cpCore) {
            try {
                //
                string Filename = null;
                string config = null;
                //Dim fs As New fileSystemClass
                string[] Lines = null;
                int LineCnt = 0;
                int LinePtr = 0;
                string LineBuffer = null;
                string[] NameValue = null;
                int Pos = 0;
                //Dim utils As New Contensive.utilsClass
                //
                this.cpCore = cpCore;
                //
                // Set default config
                //
                TimerIntervalSec = 900;
                allowErrorRecovery = true;
                allowIISReset = true;
                DiskSpaceMinMb = 500;
                ClearErrorsOnMonitorHit = false;
                AlarmEmailList = "";
                AlarmEmailServer = "Mail.MyServer.com";
                ServerName = "Unnamed Server";
                LogFileSizeMax = 1000000;
                SiteTimeout = 15;
                scheduleList = "0:00-24:00";
                ListenPort = Port_SiteMonitorDefault;
                StatusMethod = "status";
                httpStatusOnError = "500 Server Error";
                //
                // Get config file defaults
                //
                Filename = "config\\MonitorConfig.txt";
                config = cpCore.appRootFiles.readFileText(Filename);
                if (string.IsNullOrEmpty(config)) {
                    config = cpCore.appRootFiles.readFileText("MonitorConfig.txt");
                    if (!string.IsNullOrEmpty(config)) {
                        cpCore.appRootFiles.saveFile(Filename, config);
                    }
                }
                if (!string.IsNullOrEmpty(config)) {
                    Lines = genericController.stringSplit(config, Environment.NewLine);
                    LineCnt = Lines.GetUpperBound(0) + 1;
                    for (LinePtr = 0; LinePtr < LineCnt; LinePtr++) {
                        LineBuffer = Lines[LinePtr];
                        Pos = genericController.vbInstr(1, LineBuffer, "//");
                        if (Pos != 0) {
                            LineBuffer = LineBuffer.Substring(0, Pos - 1);
                        }
                        Pos = vbInstr(1, LineBuffer, "=");
                        if (Pos != 0) {
                            NameValue = LineBuffer.Split('=');
                            if (NameValue.GetUpperBound(0) > 0) {
                                switch (vbUCase(NameValue[0].Trim(' '))) {
                                    case "STATUSMETHOD":
                                        StatusMethod = NameValue[1].Trim(' ');
                                        break;
                                    case "LISTENPORT":
                                        ListenPort = encodeInteger(NameValue[1].Trim(' '));
                                        break;
                                    case "TIMERINTERVALSEC":
                                        TimerIntervalSec = encodeInteger(NameValue[1].Trim(' '));
                                        break;
                                    case "ALLOWERRORRECOVERY":
                                        allowErrorRecovery = encodeBoolean(NameValue[1].Trim(' '));
                                        break;
                                    case "ALLOWIISRESET":
                                        allowIISReset = encodeBoolean(NameValue[1].Trim(' '));
                                        break;
                                    case "DISKSPACEMINMB":
                                        DiskSpaceMinMb = encodeInteger(NameValue[1].Trim(' '));
                                        break;
                                    case "CLEARERRORSONMONITORHIT":
                                        ClearErrorsOnMonitorHit = encodeBoolean(NameValue[1].Trim(' '));
                                        break;
                                    case "ALARMEMAILLIST":
                                        AlarmEmailList = NameValue[1].Trim(' ');
                                        break;
                                    case "ALARMEMAILSERVER":
                                        AlarmEmailServer = NameValue[1].Trim(' ');
                                        break;
                                    case "SERVERNAME":
                                        ServerName = NameValue[1].Trim(' ');
                                        break;
                                    case "LOGFILESIZEMAX":
                                        LogFileSizeMax = encodeInteger(NameValue[1]);
                                        break;
                                    case "SITETIMEOUT":
                                        SiteTimeout = encodeInteger(NameValue[1]);
                                        break;
                                    case "SCHEDULE":
                                        scheduleList = NameValue[1].Trim(' ');
                                        break;
                                    case "HTTPSTATUSONERROR":
                                        httpStatusOnError = NameValue[1].Trim(' ');
                                        //
                                        // support for upgrade
                                        //
                                        break;
                                    case "IISRESETONERROR":
                                        allowErrorRecovery = true;
                                        allowIISReset = true;
                                        break;
                                }
                            }
                        }
                    }
                }
            } catch (Exception) {
            }
        }
        //
        //
        //
        public void Save() {
            try {
                //
                string Filename = null;
                //
                // Load config values from file
                //
                string config = "";
                //Dim fs As New fileSystemClass
                //
                // File not found, create file and use defaults
                //
                Filename = "config\\MonitorConfig.txt";
                config = config + Environment.NewLine + "//"
                    + Environment.NewLine + "// ccMonitor configuration file"
                    + Environment.NewLine + "//";
                config = config + Environment.NewLine + "//"
                    + Environment.NewLine + "// How often in seconds the monitor cycles"
                    + Environment.NewLine + "//"
                    + Environment.NewLine + "TimerIntervalSec=" + TimerIntervalSec;
                config = config + Environment.NewLine + "//"
                    + Environment.NewLine + "// If an application error is found, should error recovery be attempted"
                    + Environment.NewLine + "//"
                    + Environment.NewLine + "AllowErrorRecovery = " + allowErrorRecovery;
                config = config + Environment.NewLine + "//"
                    + Environment.NewLine + "// If error recovery is attempted, should it IIS be reset. Otherwise an App Pool Recycle will be attempted."
                    + Environment.NewLine + "//"
                    + Environment.NewLine + "AllowIISReset = " + allowIISReset;
                config = config + Environment.NewLine + "//"
                    + Environment.NewLine + "// Minimum disk space required on all ready drives"
                    + Environment.NewLine + "//"
                    + Environment.NewLine + "DiskSpaceMinMb = " + DiskSpaceMinMb;
                config = config + Environment.NewLine + "//"
                    + Environment.NewLine + "// Should monitor errors be cleared when http monitor page is hit"
                    + Environment.NewLine + "//"
                    + Environment.NewLine + "ClearErrorsOnMonitorHit = " + ClearErrorsOnMonitorHit;
                config = config + Environment.NewLine + "//"
                    + Environment.NewLine + "// On error, a text email will be sent to these comma delimited email address"
                    + Environment.NewLine + "//"
                    + Environment.NewLine + "AlarmEmailList = " + AlarmEmailList + Environment.NewLine + "AlarmEmailServer = " + AlarmEmailServer;
                config = config + Environment.NewLine + "//"
                    + Environment.NewLine + "// When an Alarm Email is sent, this name will identify this server"
                    + Environment.NewLine + "//"
                    + Environment.NewLine + "ServerName = " + ServerName;
                config = config + Environment.NewLine + "//"
                    + Environment.NewLine + "// Max size of a Contensive log size in Bytes (1000000 = 1Mb)"
                    + Environment.NewLine + "//"
                    + Environment.NewLine + "LogFileSizeMax = " + LogFileSizeMax;
                config = config + Environment.NewLine + "//"
                    + Environment.NewLine + "// Time in seconds to wait for each site to reply to the method=status call"
                    + Environment.NewLine + "//"
                    + Environment.NewLine + "SiteTimeout = " + SiteTimeout;
                config = config + Environment.NewLine + "//"
                    + Environment.NewLine + "// Comma separated list of times to run in 24 hour notation. When not running, it returns all OK"
                    + Environment.NewLine + "// For example, to run from 2am to 3am, then 6am to midnight 2:00-3:00,6:00-24:00"
                    + Environment.NewLine + "//"
                    + Environment.NewLine + "Schedule = " + scheduleList;
                config = config + Environment.NewLine + "//"
                    + Environment.NewLine + "// port used by monitor. For example, if the listenport is 4532, then you hit the monitor with http://thisurl.com:4532"
                    + Environment.NewLine + "//"
                    + Environment.NewLine + "ListenPort = " + ListenPort;
                config = config + Environment.NewLine + "//"
                    + Environment.NewLine + "// method used by monitor to display the status. For example, if the method is 'status', then you hit the monitor with http://thisurl.com:4532/status"
                    + Environment.NewLine + "//"
                    + Environment.NewLine + "StatusMethod = " + StatusMethod;
                config = config + Environment.NewLine + "//"
                    + Environment.NewLine + "// If there is a problem with one of the sites, the monitor page will diplay information about that error. This code will be sent to the browser (or monitor service) for this page. For instance, '200 OK' represents a good status, '500 Server Error' might represent an error."
                    + Environment.NewLine + "//"
                    + Environment.NewLine + "httpStatusOnError = " + httpStatusOnError;
                cpCore.appRootFiles.saveFile(Filename, config);
                //
                return;
                //
            } catch {
            }
        }
        //
        //
        //
        public bool isInSchedule() {
            bool return_isInSchedule = false;
            try {
                //
                string[] schedulePeriods = null;
                int Ptr = 0;
                string schedulePeriod = null;
                string[] schedulePeriodTimes = null;
                DateTime scheduleTimeStart = default(DateTime);
                DateTime scheduleTimeStop = default(DateTime);
                //Dim TimeNow As Date
                DateTime RightNow = default(DateTime);
                int timeHours = 0;
                int timeMinutes = 0;
                string schedulePeriodStartTime = null;
                //
                RightNow = DateTime.Now;
                //TimeNow = New Date(0, 1, 1, RightNow.Hour, RightNow.Minute, 1)
                if (!string.IsNullOrEmpty(scheduleList)) {
                    schedulePeriods = scheduleList.Split(',');
                    for (Ptr = 0; Ptr <= schedulePeriods.GetUpperBound(0); Ptr++) {
                        schedulePeriod = schedulePeriods[Ptr].Trim(' ');
                        if (!string.IsNullOrEmpty(schedulePeriod)) {
                            if (vbInstr(1, schedulePeriod, "-") != 0) {
                                schedulePeriodTimes = schedulePeriod.Split('-');
                                Ptr = schedulePeriodTimes[0].IndexOf(":");
                                schedulePeriodStartTime = schedulePeriodTimes[0];
                                if (Ptr == -1) {
                                    timeHours = encodeInteger(schedulePeriodStartTime);
                                    timeMinutes = 0;
                                } else {
                                    timeHours = encodeInteger(schedulePeriodStartTime.Substring(0, Ptr));
                                    timeMinutes = encodeInteger(schedulePeriodStartTime.Substring(Ptr));
                                }
                                if (timeHours >= 24) {
                                    timeHours = 23;
                                    timeMinutes = 59;
                                } else if (timeMinutes >= 60) {
                                    timeMinutes = 59;
                                }
                                scheduleTimeStart = new DateTime(RightNow.Year, RightNow.Month, RightNow.Day, timeHours, timeMinutes, 0);
                                if (RightNow > scheduleTimeStart) {
                                    if (schedulePeriodTimes.GetUpperBound(0) > 0) {
                                        Ptr = schedulePeriodTimes[1].IndexOf(":");
                                        if (Ptr == -1) {
                                            timeHours = encodeInteger(schedulePeriodTimes[1]);
                                            timeMinutes = 0;
                                        } else {
                                            timeHours = encodeInteger(schedulePeriodTimes[1].Substring(0, Ptr));
                                            timeMinutes = encodeInteger(schedulePeriodTimes[1].Substring(Ptr));
                                        }
                                        if (timeHours >= 24) {
                                            timeHours = 23;
                                            timeMinutes = 59;
                                        } else if (timeMinutes >= 60) {
                                            timeMinutes = 59;
                                        }
                                        scheduleTimeStop = new DateTime(RightNow.Year, RightNow.Month, RightNow.Day, timeHours, timeMinutes, 0);
                                        if (RightNow < scheduleTimeStop) {
                                            return_isInSchedule = true;
                                            break;
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError(cpCore, ex);
            }
            return return_isInSchedule;
        }
    }
}
