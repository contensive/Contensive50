
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
namespace Contensive.Core {
    //
    //====================================================================================================
    /// <summary>
    /// support for housekeeping functions
    /// </summary>

    public class houseKeepClass {
        //
        //
        //
        private CPClass cp;
        private DateTime LogCheckDateLast;
        //
        //
        //
        public void HouseKeep(bool DebugMode) {
            int EmailDropArchiveAgeDays = 0;
            int Pos = 0;
            string DomainNamePrimary = null;
            DateTime workingDate = default(DateTime);
            DateTime OldestVisitSummaryWeCareAbout = default(DateTime);
            string DefaultMemberName = null;
            int PeopleCID = 0;
            string RegisterList = "";
            string Content = null;
            DateTime ServerHousekeepTime = default(DateTime);
            string ErrorMessage = null;
            DateTime Yesterday = default(DateTime);
            DateTime LastTimeSummaryWasRun = default(DateTime);
            DateTime NextSummaryStartDate = default(DateTime);
            DateTime ALittleWhileAgo = default(DateTime);
            DateTime PeriodStartDate = default(DateTime);
            double PeriodDatePtr = 0;
            double PeriodStep = 0;
            DateTime StartOfHour = default(DateTime);
            int HoursPerDay = 0;
            DateTime OldestDateAdded = default(DateTime);
            object emptyData = null;
            bool NeedToClearCache = false;
            int ArchiveParentID = 0;
            int RecordID = 0;
            int CS = 0;
            XmlDocument LibraryCollections = new XmlDocument();
            XmlDocument LocalCollections = new XmlDocument();
            XmlDocument Doc = new XmlDocument();
            string AlarmTimeString = null;
            double AlarmTimeMinutesSinceMidnight = 0;
            string FolderName = null;
            int VisitArchiveAgeDays = 0;
            int GuestArchiveAgeDays = 0;
            DateTime VisitArchiveDate = default(DateTime);
            bool RunServerHousekeep = false;
            bool NewHour = false;
            DateTime rightNow = default(DateTime);
            DateTime LastCheckDateTime = default(DateTime);
            double LastCheckMinutesFromMidnight = 0;
            double minutesSinceMidnight = 0;
            string ConfigFilename = null;
            string Config = null;
            string[] ConfigLines = null;
            string Line = null;
            int LineCnt = 0;
            int LinePtr = 0;
            string[] NameValue = null;
            string SQLNow = null;
            string SQL = null;
            int DataSourceType = 0;
            CPClass cp = null;
            List<string> nonCriticalErrorList = new List<string>();
            //
            // put token in a config file
            //
            cp = new CPClass();
            //
            rightNow = DateTime.Now;
            Yesterday = rightNow.AddDays(-1).Date;
            ALittleWhileAgo = rightNow.AddDays(-90).Date;
            SQLNow = cp.core.db.encodeSQLDate(rightNow);
            //
            // ----- Read config file
            //
            ConfigFilename = "HouseKeepConfig.txt";
            Config = cp.core.privateFiles.readFile("config\\" + ConfigFilename);
            if (string.IsNullOrEmpty(Config)) {
                Config = cp.core.privateFiles.readFile("" + ConfigFilename);
            }
            if (!string.IsNullOrEmpty(Config)) {
                ConfigLines = Config.Split(new[] { "\r\n" },StringSplitOptions.None );
                LineCnt = ConfigLines.GetUpperBound(0) + 1;
                for (LinePtr = 0; LinePtr < LineCnt; LinePtr++) {
                    Line = ConfigLines[LinePtr].Trim(' ');
                    if (!string.IsNullOrEmpty(Line)) {
                        NameValue = Line.Split('=');
                        if (NameValue.GetUpperBound(0) > 0) {
                            if (NameValue[0].ToLower().Trim(' ') == "lastcheck") {
                                if (dateController.IsDate(NameValue[1])) {
                                    LastCheckDateTime = DateTime.Parse(NameValue[1]);
                                }
                                //Exit For
                            }
                            if (NameValue[0].ToLower().Trim(' ') == "serverhousekeeptime") {
                                if (dateController.IsDate(NameValue[1])) {
                                    ServerHousekeepTime = rightNow.Date.Add(genericController.EncodeDate(NameValue[1]).TimeOfDay);
                                }
                            }
                        }
                    }
                }
            }
            Content = ""
                + "lastcheck=" + rightNow + "\r\nserverhousekeeptime=" + ServerHousekeepTime + "\r\n";
            cp.core.privateFiles.saveFile("config\\" + ConfigFilename, Content);
            //
            // ----- Run Server Housekeep
            //
            if (rightNow.Date > LastCheckDateTime.Date) {
                //
                // new day since lastcheck, is alarm less then now
                //
                RunServerHousekeep = (ServerHousekeepTime < rightNow);
            } else {
                //
                // same day as lastcheck, is alarm between now and last time check
                //
                RunServerHousekeep = (rightNow > ServerHousekeepTime) && (LastCheckDateTime < ServerHousekeepTime);
            }
            NewHour = rightNow.Hour != LastCheckDateTime.Hour;
            if (DebugMode || RunServerHousekeep) {
                //
                // it is the next day, remove old log files
                //
                FolderName = "Logs";
                HousekeepLogFolder(cp.core, "server", FolderName);
                //
                System.IO.DirectoryInfo subDir = new System.IO.DirectoryInfo(cp.core.privateFiles.rootLocalPath + "\\logs\\");
                foreach (System.IO.DirectoryInfo SubDirInfo in subDir.GetDirectories()) {
                    FolderName = "logs\\" + SubDirInfo.Name;
                    HousekeepLogFolder(cp.core, "server", FolderName);
                }
                //FolderName = "Logs\Email"
                //Call HousekeepLogFolder("server", FolderName)
                //
                //FolderName = "Logs\Performance"
                //Call HousekeepLogFolder("server", FolderName)
                //
                //FolderName = "Logs\HouseKeep"
                //Call HousekeepLogFolder("server", FolderName)
                //
                //FolderName = "Logs\AddonInstall"
                //Call HousekeepLogFolder("server", FolderName)
                //
                //FolderName = "Logs\Monitor"
                //Call HousekeepLogFolder("server", FolderName)
                //
                //FolderName = "Logs\Admin"
                //Call HousekeepLogFolder("server", FolderName)LogCheckDateLast = now.date
                //
                //FolderName = "Logs\Process"
                //Call HousekeepLogFolder("server", FolderName)
                //
                // Download Updates
                //
                DownloadUpdates();
                //
                // Set LogCheckDate
                //
                LogCheckDateLast = DateTime.Now.Date;
            }
            //
            // Housekeep each application
            //
            foreach (KeyValuePair<string, Models.Context.serverConfigModel.appConfigModel> kvp in cp.core.serverConfig.apps) {
                Models.Context.serverConfigModel.appConfigModel appConfig = kvp.Value;
                if (true) {
                    if ((appConfig.appStatus == Models.Context.serverConfigModel.appStatusEnum.OK) && (appConfig.appMode == Models.Context.serverConfigModel.appModeEnum.normal)) {
                        cp = new CPClass(appConfig.name);
                        if (cp != null) {
                            if (true) {
                                //
                                // Register and unregister files in the Addon folder
                                //
                                housekeepAddonFolder(cp.core);
                                //
                                // Upgrade Local Collections, and all applications that use them
                                //
                                ErrorMessage = "";
                                AppendClassLog(cp.core, "", "HouseKeep", "Updating local collections from library, see Upgrade log for details during this period.");
                                string ignoreRefactorText = "";
                                bool ignoreRefactorBoolean = false;
                                if (!addonInstallClass.UpgradeLocalCollectionRepoFromRemoteCollectionRepo(cp.core, ref ErrorMessage, ref ignoreRefactorText, ref ignoreRefactorBoolean, false, ref nonCriticalErrorList)) {
                                    if (string.IsNullOrEmpty(ErrorMessage)) {
                                        ErrorMessage = "No detailed error message was returned from UpgradeAllLocalCollectionsFromLib2 although it returned 'not ok' status.";
                                    }
                                    AppendClassLog(cp.core, "", "HouseKeep", "Updating local collections from Library returned an error, " + ErrorMessage);
                                }
                                //
                                // Verify core installation
                                //
                                addonInstallClass.installCollectionFromRemoteRepo(cp.core, CoreCollectionGuid, ref ErrorMessage, "", false, ref nonCriticalErrorList);
                                //
                                DomainNamePrimary = cp.core.serverConfig.appConfig.domainList[0];
                                Pos = genericController.vbInstr(1, DomainNamePrimary, ",");
                                if (Pos > 1) {
                                    DomainNamePrimary = DomainNamePrimary.Left( Pos - 1);
                                }
                                //dataBuildVersion = cp.Core.app.getSiteProperty("BuildVersion", "0")
                                DataSourceType = cp.core.db.getDataSourceType("default");
                                //
                                DefaultMemberName = "";
                                PeopleCID = Models.Complex.cdefModel.getContentId(cp.core, "people");
                                SQL = "select defaultvalue from ccfields where name='name' and contentid=(" + PeopleCID + ")";
                                CS = cp.core.db.csOpenSql_rev("default", SQL);
                                if (cp.core.db.csOk(CS)) {
                                    DefaultMemberName = cp.core.db.csGetText(CS, "defaultvalue");
                                }
                                cp.core.db.csClose(ref CS);
                                //
                                // Get ArchiveAgeDays - use this as the oldest data they care about
                                //
                                VisitArchiveAgeDays = genericController.EncodeInteger(cp.core.siteProperties.getText("ArchiveRecordAgeDays", "365"));
                                if (VisitArchiveAgeDays < 2) {
                                    VisitArchiveAgeDays = 2;
                                    cp.core.siteProperties.setProperty("ArchiveRecordAgeDays", "2");
                                }
                                VisitArchiveDate = rightNow.AddDays(-VisitArchiveAgeDays).Date;
                                OldestVisitSummaryWeCareAbout = DateTime.Now.Date.AddDays(-120);
                                if (OldestVisitSummaryWeCareAbout < VisitArchiveDate) {
                                    OldestVisitSummaryWeCareAbout = VisitArchiveDate;
                                }
                                //OldestVisitSummaryWeCareAbout = now.date - VisitArchiveAgeDays
                                //
                                // Get GuestArchiveAgeDays
                                //
                                GuestArchiveAgeDays = genericController.EncodeInteger(cp.core.siteProperties.getText("ArchivePeopleAgeDays", "2"));
                                if (GuestArchiveAgeDays < 2) {
                                    GuestArchiveAgeDays = 2;
                                    cp.core.siteProperties.setProperty("ArchivePeopleAgeDays", GuestArchiveAgeDays.ToString());
                                }
                                //
                                // Get EmailDropArchiveAgeDays
                                //
                                EmailDropArchiveAgeDays = genericController.EncodeInteger(cp.core.siteProperties.getText("ArchiveEmailDropAgeDays", "90"));
                                if (EmailDropArchiveAgeDays < 2) {
                                    EmailDropArchiveAgeDays = 2;
                                    cp.core.siteProperties.setProperty("ArchiveEmailDropAgeDays", EmailDropArchiveAgeDays.ToString());
                                }
                                //
                                // Do non-optional housekeeping
                                //
                                if (RunServerHousekeep || DebugMode) {
                                    if (true) // 3.3.971" Then
                                    {
                                        //
                                        // Move Archived pages from their current parent to their archive parent
                                        //
                                        AppendClassLog(cp.core, appConfig.name, "HouseKeep", "Archive update for pages on [" + cp.core.serverConfig.appConfig.name + "]");
                                        SQL = "select * from ccpagecontent where (( DateArchive is not null )and(DateArchive<" + SQLNow + "))and(active<>0)";
                                        CS = cp.core.db.csOpenSql_rev("default", SQL);
                                        while (cp.core.db.csOk(CS)) {
                                            ArchiveParentID = cp.core.db.csGetInteger(CS, "ArchiveParentID");
                                            if (ArchiveParentID == 0) {
                                                SQL = "update ccpagecontent set DateArchive=null where (id=" + RecordID + ")";
                                                cp.core.db.executeQuery(SQL);
                                            } else {
                                                RecordID = cp.core.db.csGetInteger(CS, "ID");
                                                SQL = "update ccpagecontent set ArchiveParentID=null,DateArchive=null,parentid=" + ArchiveParentID + " where (id=" + RecordID + ")";
                                                cp.core.db.executeQuery(SQL);
                                                NeedToClearCache = true;
                                            }
                                            cp.core.db.csGoNext(CS);
                                        }
                                        cp.core.db.csClose(ref CS);
                                        //
                                        // Clear caches
                                        //
                                        if (NeedToClearCache) {
                                            emptyData = null;
                                            cp.core.cache.invalidateContent("Page Content");
                                            cp.core.cache.setContent("PCC", emptyData);
                                        }
                                    }
                                    if (true) {
                                        //
                                        // Delete any daily visit summary duplicates during this period(keep the first)
                                        //
                                        SQL = "delete from ccvisitsummary"
                                            + " where id in ("
                                            + " select d.id from ccvisitsummary d,ccvisitsummary f"
                                            + " where f.datenumber=d.datenumber"
                                            + " and f.datenumber>" + cp.core.db.encodeSQLDate(OldestVisitSummaryWeCareAbout) + " and f.datenumber<" + cp.core.db.encodeSQLDate(Yesterday) + " and f.TimeDuration=24"
                                            + " and d.TimeDuration=24"
                                            + " and f.id<d.id"
                                            + ")";
                                        cp.core.db.executeQuery(SQL);
                                        //
                                        // Find missing daily summaries, summarize that date
                                        //
                                        SQL = cp.core.db.GetSQLSelect("default", "ccVisitSummary", "DateNumber", "TimeDuration=24 and DateNumber>=" + OldestVisitSummaryWeCareAbout.Date.ToOADate(), "DateNumber,TimeNumber");
                                        CS = cp.core.db.csOpenSql_rev("default", SQL);
                                        DateTime datePtr = OldestVisitSummaryWeCareAbout;
                                        while (datePtr <= Yesterday) {
                                            if (!cp.core.db.csOk(CS)) {
                                                //
                                                // Out of data, start with this DatePtr
                                                //
                                                HouseKeep_VisitSummary(datePtr, datePtr, 24, cp.core.siteProperties.dataBuildVersion, OldestVisitSummaryWeCareAbout);
                                                //Exit For
                                            } else {
                                                workingDate = DateTime.MinValue.AddDays(cp.core.db.csGetInteger(CS, "DateNumber"));
                                                if (datePtr < workingDate) {
                                                    //
                                                    // There are missing dates, update them
                                                    //
                                                    HouseKeep_VisitSummary(datePtr, workingDate.AddDays(-1), 24, cp.core.siteProperties.dataBuildVersion, OldestVisitSummaryWeCareAbout);
                                                }
                                            }
                                            if (cp.core.db.csOk(CS)) {
                                                //
                                                // if there is more data, go to the next record
                                                //
                                                cp.core.db.csGoNext(CS);
                                            }
                                            datePtr = datePtr.AddDays(1).Date;
                                        }
                                        cp.core.db.csClose(ref CS);
                                    }
                                    //
                                    // Remote Query Expiration
                                    //
                                    SQL = "delete from ccRemoteQueries where (DateExpires is not null)and(DateExpires<" + cp.core.db.encodeSQLDate(DateTime.Now) + ")";
                                    cp.core.db.executeQuery(SQL);
                                    if (true) {
                                        //
                                        // Clean Navigation
                                        //
                                        if (DataSourceType == DataSourceTypeODBCMySQL) {
                                            SQL = "delete m from ccmenuEntries m left join ccAggregateFunctions a on a.id=m.AddonID where m.addonid<>0 and a.id is null";
                                        } else {
                                            SQL = "delete from ccmenuEntries where id in (select m.ID from ccMenuEntries m left join ccAggregateFunctions a on a.id=m.AddonID where m.addonid<>0 and a.id is null)";
                                        }
                                        cp.core.db.executeQuery(SQL);
                                        //
                                        if (DataSourceType == DataSourceTypeODBCMySQL) {
                                            SQL = "delete m from ccmenuEntries m left join ccAggregateFunctions a on a.id=m.helpaddonid where m.helpaddonid<>0 and a.id is null";
                                        } else {
                                            SQL = "delete from ccmenuEntries where id in (select m.ID from ccMenuEntries m left join ccAggregateFunctions a on a.id=m.helpaddonid where m.helpaddonid<>0 and a.id is null)";
                                        }
                                        cp.core.db.executeQuery(SQL);
                                        //
                                        if (DataSourceType == DataSourceTypeODBCMySQL) {
                                            SQL = "delete m from ccmenuEntries m left join ccAggregateFunctions a on a.id=m.helpcollectionid where m.helpcollectionid<>0 and a.id is null";
                                        } else {
                                            SQL = "delete from ccmenuEntries where id in (select m.ID from ccMenuEntries m left join ccAddonCollections c on c.id=m.helpcollectionid Where m.helpcollectionid <> 0 And c.Id Is Null)";
                                        }
                                        cp.core.db.executeQuery(SQL);
                                    }
                                    //
                                    // Page View Summary
                                    //
                                    if (true) // 4.1.187" Then
                                    {
                                        //
                                        // Delete duplicates
                                        //
                                        //SQL = "delete from ccviewingsummary" _
                                        //    & " where id in (" _
                                        //    & " select d.id from ccviewingsummary d,ccviewingsummary f" _
                                        //    & " where f.datenumber=d.datenumber" _
                                        //    & " and f.datenumber>" & encodeSQLDate(OldestVisitSummaryWeCareAbout) _
                                        //    & " and f.datenumber<" & encodeSQLDate(Yesterday) _
                                        //    & " and f.TimeDuration=24" _
                                        //    & " and d.TimeDuration=24" _
                                        //    & " and f.id<d.id" _
                                        //    & ")"
                                        //Call cp.Core.app.ExecuteSQL( SQL)
                                        //
                                        // Find the day of the last entry in the viewing summary table as start there
                                        // PageViewSummary should always add at least one entry for each day, even if 0
                                        //
                                        if (true) {
                                            DateTime datePtr = default(DateTime);
                                            SQL = cp.core.db.GetSQLSelect("default", "ccviewingsummary", "DateNumber", "TimeDuration=24 and DateNumber>=" + OldestVisitSummaryWeCareAbout.Date.ToOADate(), "DateNumber Desc", "", 1);
                                            CS = cp.core.db.csOpenSql_rev("default", SQL);
                                            if (!cp.core.db.csOk(CS)) {
                                                datePtr = OldestVisitSummaryWeCareAbout;
                                            } else {
                                                datePtr = DateTime.MinValue.AddDays(cp.core.db.csGetInteger(CS, "DateNumber"));
                                            }
                                            cp.core.db.csClose(ref CS);
                                            if (datePtr < OldestVisitSummaryWeCareAbout) {
                                                datePtr = OldestVisitSummaryWeCareAbout;
                                            }
                                            HouseKeep_PageViewSummary(datePtr, Yesterday, 24, cp.core.siteProperties.dataBuildVersion, OldestVisitSummaryWeCareAbout);
                                        }
                                    }
                                }
                                //
                                // Each hour, summarize the visits and viewings into the Visit Summary table
                                //
                                if (DebugMode || NewHour) {
                                    //
                                    // Set NextSummaryStartDate based on the last time we ran hourly summarization
                                    //
                                    LastTimeSummaryWasRun = VisitArchiveDate;
                                    //LastTimeSummaryWasRun = ALittleWhileAgo
                                    //sql="select top 1 dateadded from ccvisitsummary where (timeduration=1)and(Dateadded>" & encodeSQLDate(ALittleWhileAgo) & ") order by id desc"
                                    SQL = cp.core.db.GetSQLSelect("default", "ccVisitSummary", "DateAdded", "(timeduration=1)and(Dateadded>" + cp.core.db.encodeSQLDate(VisitArchiveDate) + ")", "id Desc","", 1);
                                    //SQL = cp.Core.app.csv_GetSQLSelect("default", "ccVisitSummary", "DateAdded", "(timeduration=1)and(Dateadded>" & encodeSQLDate(ALittleWhileAgo) & ")", "id Desc", , 1)
                                    CS = cp.core.db.csOpenSql_rev("default", SQL);
                                    if (cp.core.db.csOk(CS)) {
                                        LastTimeSummaryWasRun = cp.core.db.csGetDate(CS, "DateAdded");
                                        AppendClassLog(cp.core, cp.core.serverConfig.appConfig.name, "HouseKeep", "Update hourly visit summary, last time summary was run was [" + LastTimeSummaryWasRun + "]");
                                    } else {
                                        AppendClassLog(cp.core, cp.core.serverConfig.appConfig.name, "HouseKeep", "Update hourly visit summary, no hourly summaries were found, set start to [" + LastTimeSummaryWasRun + "]");
                                    }
                                    cp.core.db.csClose(ref CS);
                                    NextSummaryStartDate = LastTimeSummaryWasRun;
                                    //
                                    // Each hourly entry includes visits that started during that hour, but we do not know when they finished (maybe during last hour)
                                    //   Find the oldest starttime of all the visits with endtimes after the LastTimeSummaryWasRun. Resummarize all periods
                                    //   from then to now
                                    //
                                    //   For the past 24 hours, find the oldest visit with the last viewing during the last hour
                                    //
                                    //OldestDateAdded = LastTimeSummaryWasRun
                                    //PeriodStep = CDbl(1) / CDbl(24)
                                    StartOfHour = (new DateTime(LastTimeSummaryWasRun.Year, LastTimeSummaryWasRun.Month, LastTimeSummaryWasRun.Day, LastTimeSummaryWasRun.Hour, 1, 1)).AddHours(-1); // (Int(24 * LastTimeSummaryWasRun) / 24) - PeriodStep
                                    OldestDateAdded = StartOfHour;
                                    SQL = cp.core.db.GetSQLSelect("default", "ccVisits", "DateAdded", "LastVisitTime>" + cp.core.db.encodeSQLDate(StartOfHour), "dateadded", "", 1);
                                    //SQL = "select top 1 Dateadded from ccvisits where LastVisitTime>" & encodeSQLDate(StartOfHour) & " order by DateAdded"
                                    CS = cp.core.db.csOpenSql_rev("default", SQL);
                                    if (cp.core.db.csOk(CS)) {
                                        OldestDateAdded = cp.core.db.csGetDate(CS, "DateAdded");
                                        if (OldestDateAdded < NextSummaryStartDate) {
                                            NextSummaryStartDate = OldestDateAdded;
                                            AppendClassLog(cp.core, cp.core.serverConfig.appConfig.name, "HouseKeep", "Update hourly visit summary, found a visit with the last viewing during the past hour. It started [" + OldestDateAdded + "], before the last summary was run.");
                                        }
                                    }
                                    cp.core.db.csClose(ref CS);
                                    //
                                    // Verify there are 24 hour records for every day back the past 90 days
                                    //
                                    DateTime DateofMissingSummary = DateTime.MinValue;
                                    //Call AppendClassLog(cp.core, cp.core.appEnvironment.name, "HouseKeep", "Verify there are 24 hour records for the past 90 days")
                                    PeriodStartDate = rightNow.Date.AddDays(-90);
                                    PeriodStep = 1;
                                    //INSTANT C# TODO TASK: The step increment was not confirmed to be positive - confirm that the stopping condition is appropriate:
                                    //ORIGINAL LINE: For PeriodDatePtr = PeriodStartDate.ToOADate To OldestDateAdded.ToOADate Step PeriodStep
                                    for (PeriodDatePtr = PeriodStartDate.ToOADate(); PeriodDatePtr <= OldestDateAdded.ToOADate(); PeriodDatePtr += PeriodStep) {
                                        SQL = "select count(id) as HoursPerDay from ccVisitSummary where TimeDuration=1 and DateNumber=" + EncodeInteger(PeriodDatePtr) + " group by DateNumber";
                                        //SQL = "select count(id) as HoursPerDay from ccVisitSummary group by DateNumber having DateNumber=" & CLng(PeriodDatePtr)
                                        CS = cp.core.db.csOpenSql_rev("default", SQL);
                                        HoursPerDay = 0;
                                        if (cp.core.db.csOk(CS)) {
                                            HoursPerDay = cp.core.db.csGetInteger(CS, "HoursPerDay");
                                        }
                                        cp.core.db.csClose(ref CS);
                                        if (HoursPerDay < 24) {
                                            DateofMissingSummary = DateTime.FromOADate(PeriodDatePtr);
                                            break;
                                        }
                                    }
                                    if ((DateofMissingSummary != DateTime.MinValue) && (DateofMissingSummary < NextSummaryStartDate)) {
                                        AppendClassLog(cp.core, cp.core.serverConfig.appConfig.name, "HouseKeep", "Found a missing hourly period in the visit summary table [" + DateofMissingSummary + "], it only has [" + HoursPerDay + "] hourly summaries.");
                                        NextSummaryStartDate = DateofMissingSummary;
                                    }
                                    //
                                    // Now summarize all visits during all hourly periods between OldestDateAdded and the previous Hour
                                    //
                                    AppendClassLog(cp.core, cp.core.serverConfig.appConfig.name, "HouseKeep", "Summaryize visits hourly, starting [" + NextSummaryStartDate + "]");
                                    PeriodStep = (double)1 / (double)24;
                                    //PeriodStart = (Int(OldestDateAdded * 24) / 24)
                                    HouseKeep_VisitSummary(NextSummaryStartDate, rightNow, 1, cp.core.siteProperties.dataBuildVersion, OldestVisitSummaryWeCareAbout);
                                }
                                //
                                // OK to run archive
                                // During archive, non-cookie records are removed, so this has to run after summarizing
                                // and we can only delete non-cookie records older than 2 days (so we can be sure they have been summarized)
                                //
                                if (DebugMode) {
                                    //
                                    // debug mode - run achive if no times are given
                                    //
                                    HouseKeep_App_Daily(VisitArchiveAgeDays, GuestArchiveAgeDays, EmailDropArchiveAgeDays, DefaultMemberName, cp.core.siteProperties.dataBuildVersion);
                                } else {
                                    //
                                    // Check for site's archive time of day
                                    //
                                    AlarmTimeString = cp.core.siteProperties.getText("ArchiveTimeOfDay", "12:00:00 AM");
                                    if (string.IsNullOrEmpty(AlarmTimeString)) {
                                        AlarmTimeString = "12:00:00 AM";
                                        cp.core.siteProperties.setProperty("ArchiveTimeOfDate", AlarmTimeString);
                                    }
                                    if (!dateController.IsDate(AlarmTimeString)) {
                                        AlarmTimeString = "12:00:00 AM";
                                        cp.core.siteProperties.setProperty("ArchiveTimeOfDate", AlarmTimeString);
                                    }
                                    AlarmTimeMinutesSinceMidnight = genericController.EncodeDate(AlarmTimeString).TimeOfDay.TotalMinutes;
                                    minutesSinceMidnight = rightNow.TimeOfDay.TotalMinutes;
                                    LastCheckMinutesFromMidnight = LastCheckDateTime.TimeOfDay.TotalMinutes;
                                    if ((minutesSinceMidnight > LastCheckMinutesFromMidnight) && (LastCheckMinutesFromMidnight < minutesSinceMidnight)) {
                                        //
                                        // Same Day - Midnight is before last and after current
                                        //
                                        HouseKeep_App_Daily(VisitArchiveAgeDays, GuestArchiveAgeDays, EmailDropArchiveAgeDays, DefaultMemberName, cp.core.siteProperties.dataBuildVersion);
                                    } else if ((LastCheckMinutesFromMidnight > minutesSinceMidnight) && ((LastCheckMinutesFromMidnight < minutesSinceMidnight))) {
                                        //
                                        // New Day - Midnight is between Last and Set
                                        //
                                        HouseKeep_App_Daily(VisitArchiveAgeDays, GuestArchiveAgeDays, EmailDropArchiveAgeDays, DefaultMemberName, cp.core.siteProperties.dataBuildVersion);
                                    }
                                }
                            }
                        }
                        cp.Dispose();
                        cp = null;
                    }
                }
            }
            //
            return;
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception");
        }
        //
        //
        //
        private void HouseKeep_App_Daily(int VisitArchiveAgeDays, int GuestArchiveAgeDays, int EmailDropArchiveAgeDays, string DefaultMemberName, string BuildVersion) {
            try {
                //
                DateTime ArchiveEmailDropDate = default(DateTime);
                string VirtualFileName = null;
                string VirtualLink = null;
                string FilenameOriginal = null;
                int Pos = 0;
                string FilenameExt = null;
                string FilenameNoExt = null;
                string FilenameAltSize = null;
                FileInfo[] FileList = null;
                //
                long FileSize = 0;
                string PathNameRev = null;
                string[] FilenameDim = null;
                //
                int DaystoRemove = 0;
                int fieldType = 0;
                int FieldContentID = 0;
                string FieldCaption = null;
                string FieldLast = null;
                string FieldNew = null;
                int FieldRecordID = 0;
                int RecordID = 0;
                int ArchiveParentID = 0;
                DateTime OldestVisitDate = default(DateTime);
                DateTime ArchiveDate = default(DateTime);
                DateTime thirtyDaysAgo = default(DateTime);
                DateTime SingleDate = default(DateTime);
                int DataSourceType = 0;
                //
                //Dim Controller As controlClass
                int VisitArchiveDeleteSize = 0;
                //Dim AppService As appServicesClass
                //Dim KernelService As KernelServicesClass
                //    Dim CSConnection As appEnvironmentStruc
                string SQL = null;
                string SQLCriteria = null;
                string PathName = null;
                string TableName = null;
                string FieldName = null;
                int CS = 0;
                int CSTest = 0;
                string Filename = null;
                string[] FileSplit = null;
                string FolderName = null;
                string FolderList = null;
                string[] FolderArray = null;
                int FolderArrayCount = 0;
                int FolderArrayPointer = 0;
                string[] FolderSplit = null;
                int AdminLicenseCount = 0;
                string ArchiveDateSQL = null;
                string appName = null;
                string SQLTablePeople = null;
                string SQLTableMemberRules = null;
                string SQLTableGroups = null;
                int PeopleCID = 0;
                //Dim DefaultName As String
                string Hint = null;
                bool ArchiveDeleteNoCookie = false;
                DateTime MidnightTwoDaysAgo = default(DateTime);
                string SQLDateMidnightTwoDaysAgo = null;
                int TimeoutSave = 0;
                DateTime Yesterday = default(DateTime);
                DateTime rightNow = DateTime.Now;
                //
                Yesterday = rightNow.AddDays(-1).Date;
                MidnightTwoDaysAgo = rightNow.AddDays(-2).Date;
                thirtyDaysAgo = rightNow.AddDays(-30).Date;
                appName = cp.core.serverConfig.appConfig.name;
                ArchiveDeleteNoCookie = genericController.encodeBoolean(cp.core.siteProperties.getText("ArchiveDeleteNoCookie", "1"));
                DataSourceType = cp.core.db.getDataSourceType("default");
                TimeoutSave = cp.core.db.sqlCommandTimeout;
                cp.core.db.sqlCommandTimeout = 1800;
                //
                SQLTablePeople = Models.Complex.cdefModel.getContentTablename(cp.core, "People");
                SQLTableMemberRules = Models.Complex.cdefModel.getContentTablename(cp.core, "Member Rules");
                SQLTableGroups = Models.Complex.cdefModel.getContentTablename(cp.core, "Groups");
                SQLDateMidnightTwoDaysAgo = cp.core.db.encodeSQLDate(MidnightTwoDaysAgo);
                //
                // Any member records that were created outside contensive need to have CreatedByVisit=0 (past v4.1.152)
                cp.core.db.executeQuery("update ccmembers set CreatedByVisit=0 where createdbyvisit is null");
                //
                // delete nocookie visits
                // This must happen after the housekeep summarizing, and no sooner then 48 hours ago so all hits have been summarized before deleting
                //
                if (ArchiveDeleteNoCookie) {
                    //
                    // delete members from the non-cookie visits
                    // legacy records without createdbyvisit will have to be corrected by hand (or upgrade)
                    //
                    AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting members from visits with no cookie support older than Midnight, Two Days Ago");
                    switch (DataSourceType) {
                        case DataSourceTypeODBCAccess:
                            SQL = "delete m.*"
                                + " from " + SQLTablePeople + " m,ccvisits v"
                                + " where v.memberid=m.id"
                                + " and(m.Visits=1)"
                                + " and(m.createdbyvisit=1)"
                                + " and(m.Username is null)"
                                + " and(m.email is null)"
                                + " and(v.CookieSupport=0)and(v.LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")";
                            break;
                        case DataSourceTypeODBCMySQL:
                            SQL = "delete m"
                                + " from " + SQLTablePeople + " m,ccvisits v"
                                + " where v.memberid=m.id"
                                + " and(m.Visits=1)"
                                + " and(m.createdbyvisit=1)"
                                + " and(m.Username is null)"
                                + " and(m.email is null)"
                                + " and(v.CookieSupport=0)and(v.LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")";
                            break;
                        default:
                            SQL = "delete from " + SQLTablePeople + " from " + SQLTablePeople + " m,ccvisits v"
                                + " where v.memberid=m.id"
                                + " and(m.Visits=1)"
                                + " and(m.createdbyvisit=1)"
                                + " and(m.Username is null)"
                                + " and(m.email is null)"
                                + " and(v.CookieSupport=0)and(v.LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")";
                            break;
                    }
                    // removed name requirement bc spiders not have bot names
                    //        Select Case DataSourceType
                    //            Case DataSourceTypeODBCAccess
                    //                SQL = "delete m.*" _
                    //                    & " from " & SQLTablePeople & " m,ccvisits v" _
                    //                    & " where v.memberid=m.id" _
                    //                    & " and(m.Name=" & encodeSQLText(DefaultMemberName) & ")" _
                    //                    & " and(m.Visits=1)" _
                    //                    & " and(m.Username is null)" _
                    //                    & " and(m.email is null)" _
                    //                    & " and(v.CookieSupport=0)and(v.LastVisitTime<" & SQLDateMidnightTwoDaysAgo & ")"
                    //            Case DataSourceTypeODBCMySQL
                    //                SQL = "delete m" _
                    //                    & " from " & SQLTablePeople & " m,ccvisits v" _
                    //                    & " where v.memberid=m.id" _
                    //                    & " and(m.Name=" & encodeSQLText(DefaultMemberName) & ")" _
                    //                    & " and(m.Visits=1)" _
                    //                    & " and(m.Username is null)" _
                    //                    & " and(m.email is null)" _
                    //                    & " and(v.CookieSupport=0)and(v.LastVisitTime<" & SQLDateMidnightTwoDaysAgo & ")"
                    //            Case Else
                    //                SQL = "delete from " & SQLTablePeople _
                    //                    & " from " & SQLTablePeople & " m,ccvisits v" _
                    //                    & " where v.memberid=m.id" _
                    //                    & " and(m.Name=" & encodeSQLText(DefaultMemberName) & ")" _
                    //                    & " and(m.Visits=1)" _
                    //                    & " and(m.Username is null)" _
                    //                    & " and(m.email is null)" _
                    //                    & " and(v.CookieSupport=0)and(v.LastVisitTime<" & SQLDateMidnightTwoDaysAgo & ")"
                    //        End Select
                    try {
                        cp.core.db.executeQuery(SQL);
                    } catch (Exception ex) {
                    }

                    //
                    // delete viewings from the non-cookie visits
                    //
                    AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting viewings from visits with no cookie support older than Midnight, Two Days Ago");
                    switch (DataSourceType) {
                        case DataSourceTypeODBCAccess:
                            SQL = "delete h.*"
                                + " from ccviewings h,ccvisits v"
                                + " where h.visitid=v.id"
                                + " and(v.CookieSupport=0)and(v.LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")";
                            break;
                        case DataSourceTypeODBCMySQL:
                            SQL = "delete h"
                                + " from ccviewings h,ccvisits v"
                                + " where h.visitid=v.id"
                                + " and(v.CookieSupport=0)and(v.LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")";
                            break;
                        default:
                            SQL = "delete from ccviewings"
                                + " from ccviewings h,ccvisits v"
                                + " where h.visitid=v.id"
                                + " and(v.CookieSupport=0)and(v.LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")";
                            break;
                    }
                    // if this fails, continue with the rest of the work
                    try {
                        cp.core.db.executeQuery(SQL);
                    } catch (Exception) {
                    }
                    //
                    // delete visitors from the non-cookie visits
                    //
                    AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting visitors from visits with no cookie support older than Midnight, Two Days Ago");
                    switch (DataSourceType) {
                        case DataSourceTypeODBCAccess:
                            SQL = "delete r.*"
                                + " from ccvisitors r,ccvisits v"
                                + " where r.id=v.visitorid"
                                + " and(v.CookieSupport=0)and(v.LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")";
                            break;
                        case DataSourceTypeODBCMySQL:
                            SQL = "delete r"
                                + " from ccvisitors r,ccvisits v"
                                + " where r.id=v.visitorid"
                                + " and(v.CookieSupport=0)and(v.LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")";
                            break;
                        default:
                            SQL = "delete from ccvisitors"
                                + " from ccvisitors r,ccvisits v"
                                + " where r.id=v.visitorid"
                                + " and(v.CookieSupport=0)and(v.LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")";
                            break;
                    }
                    try {
                        cp.core.db.executeQuery(SQL);
                    } catch (Exception) {
                    }
                    //
                    // delete visits from the non-cookie visits
                    //
                    AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting visits with no cookie support older than Midnight, Two Days Ago");
                    cp.core.db.DeleteTableRecordChunks("default", "ccvisits", "(CookieSupport=0)and(LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")", 1000, 10000);
                }
                //
                // Visits with no DateAdded
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting visits with no DateAdded");
                cp.core.db.DeleteTableRecordChunks("default", "ccvisits", "(DateAdded is null)or(DateAdded<=" + cp.core.db.encodeSQLDate(new DateTime(1995, 1, 1)) + ")", 1000, 10000);
                //
                // Visits with no visitor
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting visits with no DateAdded");
                cp.core.db.DeleteTableRecordChunks("default", "ccvisits", "(VisitorID is null)or(VisitorID=0)", 1000, 10000);
                //
                // viewings with no visit
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting viewings with null or invalid VisitID");
                cp.core.db.DeleteTableRecordChunks("default", "ccviewings", "(visitid=0 or visitid is null)", 1000, 10000);
                //
                // Get Oldest Visit
                //
                //SQL = "select top 1 DateAdded from ccVisits where dateadded>0 order by DateAdded"
                SQL = cp.core.db.GetSQLSelect("default", "ccVisits", "DateAdded", "", "dateadded", "", 1);
                CS = cp.core.db.csOpenSql_rev("default", SQL);
                if (cp.core.db.csOk(CS)) {
                    OldestVisitDate = cp.core.db.csGetDate(CS, "DateAdded").Date;
                }
                cp.core.db.csClose(ref CS);
                //
                // Remove old visit records
                //   if > 30 days in visit table, limit one pass to just 30 days
                //   this is to prevent the entire server from being bogged down for one site change
                //
                if (OldestVisitDate == DateTime.MinValue) {
                    AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "No records were removed because no visit records were found while requesting the oldest visit.");
                } else if (VisitArchiveAgeDays <= 0) {
                    AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "No records were removed because Housekeep ArchiveRecordAgeDays is 0.");
                } else {
                    ArchiveDate = rightNow.AddDays(-VisitArchiveAgeDays).Date;
                    DaystoRemove = EncodeInteger(ArchiveDate.Subtract(OldestVisitDate).TotalDays);
                    if (DaystoRemove > 30) {
                        ArchiveDate = OldestVisitDate.AddDays(30);
                    }
                    if (OldestVisitDate >= ArchiveDate) {
                        AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "No records were removed because Oldest Visit Date [" + OldestVisitDate + "] >= ArchiveDate [" + ArchiveDate + "].");
                    } else {
                        AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Removing records from [" + OldestVisitDate + "] to [" + ArchiveDate + "].");
                        SingleDate = OldestVisitDate;
                        do {
                            HouseKeep_App_Daily_RemoveVisitRecords(SingleDate, DataSourceType);
                            SingleDate = SingleDate.AddDays(1);
                        } while (SingleDate < ArchiveDate);
                    }
                }
                //
                // Remove old guest records
                //
                ArchiveDate = rightNow.AddDays(-GuestArchiveAgeDays).Date;
                HouseKeep_App_Daily_RemoveGuestRecords(ArchiveDate, DataSourceType);
                //
                // delete 'guests' Members with one visits but no valid visit record
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting 'guest' members with no visits (name is default name, visits=1, username null, email null,dateadded=lastvisit)");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        SQL = "delete m.*"
                            + " from " + SQLTablePeople + " m,ccvisits v"
                            + " where v.memberid=m.id"
                            + " and(m.createdbyvisit=1)"
                            + " and(m.Visits=1)"
                            + " and(m.Username is null)"
                            + " and(m.email is null)"
                            + " and(m.dateadded=m.lastvisit)"
                            + " and(v.id is null)";
                        break;
                    case DataSourceTypeODBCMySQL:
                        SQL = "delete m"
                            + " from " + SQLTablePeople + " m,ccvisits v"
                            + " where v.memberid=m.id"
                            + " and(m.createdbyvisit=1)"
                            + " and(m.Visits=1)"
                            + " and(m.Username is null)"
                            + " and(m.email is null)"
                            + " and(m.dateadded=m.lastvisit)"
                            + " and(v.id is null)";
                        break;
                    default:
                        SQL = "delete from " + SQLTablePeople + " from " + SQLTablePeople + " m,ccvisits v"
                            + " where v.memberid=m.id"
                            + " and(m.createdbyvisit=1)"
                            + " and(m.Visits=1)"
                            + " and(m.Username is null)"
                            + " and(m.email is null)"
                            + " and(m.dateadded=m.lastvisit)"
                            + " and(v.id is null)";
                        break;
                }
                //INSTANT C# TODO TASK: The '////On Error //Resume Next' statement is not converted by Instant C#:
                ////On Error //Resume Next

                cp.core.db.executeQuery(SQL);
                //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                if (0 != 0) {
                    //throw new ApplicationException("Unexpected exception");
                }
                //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                //Microsoft.VisualBasic.Information.Err().Clear();
                // moved to upgrade code
                //    '
                //    ' Update CreatedByVisit for older records where this field is null
                //    '
                //    ////On Error //Resume Next
                //    SQL = "update ccmembers set createdbyvisit=1 where (createdbyvisit Is Null) And (dateadded<" & encodeSQLDate("1/1/2010") & ") and (username Is Null) And (email Is Null) And ((visits <> 0) And (visits Is Not Null))"
                //    Call cp.Core.app.ExecuteSQL( SQL)
                //    Err.Clear
                //    On Error GoTo ErrorTrap
                //
                // delete 'guests' Members created before ArchivePeopleAgeDays
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting 'guest' members with no visits (name is default name, visits=1, username null, email null,dateadded=lastvisit)");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        SQL = "delete m.*"
                            + " from " + SQLTablePeople + " m left join ccvisits v on v.memberid=m.id"
                            + " where(m.createdbyvisit=1)"
                            + " and(m.Visits=1)"
                            + " and(m.Username is null)"
                            + " and(m.email is null)"
                            + " and(m.dateadded=m.lastvisit)"
                            + " and(v.id is null)";
                        break;
                    case DataSourceTypeODBCMySQL:
                        SQL = "delete m"
                            + " from " + SQLTablePeople + " m left join ccvisits v on v.memberid=m.id"
                            + " where(m.createdbyvisit=1)"
                            + " and(m.Visits=1)"
                            + " and(m.Username is null)"
                            + " and(m.email is null)"
                            + " and(m.dateadded=m.lastvisit)"
                            + " and(v.id is null)";
                        break;
                    default:
                        SQL = "delete from " + SQLTablePeople + " from " + SQLTablePeople + " m left join ccvisits v on v.memberid=m.id"
                            + " where(m.createdbyvisit=1)"
                            + " and(m.Visits=1)"
                            + " and(m.Username is null)"
                            + " and(m.email is null)"
                            + " and(m.dateadded=m.lastvisit)"
                            + " and(v.id is null)";
                        break;
                }
                //INSTANT C# TODO TASK: The '////On Error //Resume Next' statement is not converted by Instant C#:
                ////On Error //Resume Next

                cp.core.db.executeQuery(SQL);
                //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                if (0 != 0) {
                    //throw new ApplicationException("Unexpected exception");
                }
                //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                //Microsoft.VisualBasic.Information.Err().Clear();
                //
                // delete email drops older than archive.
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting email drops older then " + EmailDropArchiveAgeDays + " days");
                ArchiveEmailDropDate = rightNow.AddDays(-EmailDropArchiveAgeDays).Date;
                //INSTANT C# TODO TASK: The '////On Error //Resume Next' statement is not converted by Instant C#:
                ////On Error //Resume Next

                cp.core.db.deleteContentRecords("Email drops", "(DateAdded is null)or(DateAdded<=" + cp.core.db.encodeSQLDate(ArchiveEmailDropDate) + ")");
                //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                if (0 != 0) {
                    //throw new ApplicationException("Unexpected exception");
                }
                //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                //Microsoft.VisualBasic.Information.Err().Clear();
                //
                // delete email log entries not realted to a drop, older than archive.
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting non-drop email logs older then " + EmailDropArchiveAgeDays + " days");
                ArchiveEmailDropDate = rightNow.AddDays(-EmailDropArchiveAgeDays).Date;
                //INSTANT C# TODO TASK: The '////On Error //Resume Next' statement is not converted by Instant C#:
                ////On Error //Resume Next

                cp.core.db.deleteContentRecords("Email Log", "(emailDropId is null)and((DateAdded is null)or(DateAdded<=" + cp.core.db.encodeSQLDate(ArchiveEmailDropDate) + "))");
                //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                if (0 != 0) {
                    //throw new ApplicationException("Unexpected exception");
                }
                //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                //Microsoft.VisualBasic.Information.Err().Clear();
                //
                // delete email log entries without email drops
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting drop email log entries for drops without a valid drop record.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        SQL = "delete l.*"
                            + " from ccemaillog l"
                            + " left join ccemaildrops d on d.id=l.emaildropid"
                            + " where l.emaildropid Is Not Null"
                            + " and d.id is null"
                            + "";
                        break;
                    case DataSourceTypeODBCMySQL:
                        SQL = "delete l"
                            + " from ccemaillog l"
                            + " left join ccemaildrops d on d.id=l.emaildropid"
                            + " where l.emaildropid Is Not Null"
                            + " and d.id is null"
                            + "";
                        break;
                    default:
                        SQL = "delete from ccemaillog"
                            + " from ccemaillog l"
                            + " left join ccemaildrops d on d.id=l.emaildropid"
                            + " where l.emaildropid Is Not Null"
                            + " and d.id is null"
                            + "";
                        break;
                }
                //INSTANT C# TODO TASK: The '////On Error //Resume Next' statement is not converted by Instant C#:
                ////On Error //Resume Next

                cp.core.db.executeQuery(SQL);
                //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                if (0 != 0) {
                    //throw new ApplicationException("Unexpected exception");
                }
                //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                //Microsoft.VisualBasic.Information.Err().Clear();

                //
                // block duplicate redirect fields (match contentid+fieldtype+caption)
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Inactivate duplicate redirect fields");
                CS = cp.core.db.csOpenSql_rev("Default", "Select ID, ContentID, Type, Caption from ccFields where (active<>0)and(Type=" + FieldTypeIdRedirect + ") Order By ContentID, Caption, ID");
                FieldLast = "";
                while (cp.core.db.csOk(CS)) {
                    //FieldType = cp.Core.app.csv_cs_getInteger(CS, "Type")
                    FieldContentID = cp.core.db.csGetInteger(CS, "Contentid");
                    FieldCaption = cp.core.db.csGetText(CS, "Caption");
                    FieldNew = FieldContentID + FieldCaption;
                    if (FieldNew == FieldLast) {
                        FieldRecordID = cp.core.db.csGetInteger(CS, "ID");
                        cp.core.db.executeQuery("Update ccFields set active=0 where ID=" + FieldRecordID + ";");
                    }
                    FieldLast = FieldNew;
                    cp.core.db.csGoNext(CS);
                }
                cp.core.db.csClose(ref CS);
                //
                // block duplicate non-redirect fields (match contentid+fieldtype+name)
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Inactivate duplicate non-redirect fields");
                CS = cp.core.db.csOpenSql_rev("Default", "Select ID, Name, ContentID, Type from ccFields where (active<>0)and(Type<>" + FieldTypeIdRedirect + ") Order By ContentID, Name, Type, ID");
                FieldLast = "";
                while (cp.core.db.csOk(CS)) {
                    fieldType = cp.core.db.csGetInteger(CS, "Type");
                    FieldContentID = cp.core.db.csGetInteger(CS, "Contentid");
                    FieldName = cp.core.db.csGetText(CS, "Name");
                    FieldRecordID = cp.core.db.csGetInteger(CS, "ID");
                    FieldNew = FieldContentID + FieldName + fieldType;
                    if (FieldNew == FieldLast) {
                        cp.core.db.executeQuery("Update ccFields set active=0 where ID=" + FieldRecordID + ";");
                    }
                    FieldLast = FieldNew;
                    cp.core.db.csGoNext(CS);
                }
                cp.core.db.csClose(ref CS);
                //
                // Activities with no Member
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting activities with no member record.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        SQL = "delete ccactivitylog.*"
                            + " From ccactivitylog LEFT JOIN " + SQLTablePeople + " on " + SQLTablePeople + ".ID=ccactivitylog.memberid"
                            + " WHERE (" + SQLTablePeople + ".ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        SQL = "delete from ccactivitylog"
                            + " From ccactivitylog LEFT JOIN " + SQLTablePeople + " on " + SQLTablePeople + ".ID=ccactivitylog.memberid"
                            + " WHERE (" + SQLTablePeople + ".ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                    default:
                        SQL = "delete ccactivitylog"
                            + " From ccactivitylog LEFT JOIN " + SQLTablePeople + " on " + SQLTablePeople + ".ID=ccactivitylog.memberid"
                            + " WHERE (" + SQLTablePeople + ".ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                }
                //
                // Member Properties with no member
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting member properties with no member record.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        SQL = "delete ccProperties.*"
                            + " From ccProperties LEFT JOIN " + SQLTablePeople + " on " + SQLTablePeople + ".ID=ccProperties.KeyID"
                            + " WHERE (ccProperties.TypeID=0)"
                            + " AND (" + SQLTablePeople + ".ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        SQL = "delete From ccProperties"
                            + " From ccProperties LEFT JOIN " + SQLTablePeople + " on " + SQLTablePeople + ".ID=ccProperties.KeyID"
                            + " WHERE (ccProperties.TypeID=0)"
                            + " AND (" + SQLTablePeople + ".ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                    default:
                        SQL = "delete ccProperties"
                            + " From ccProperties LEFT JOIN " + SQLTablePeople + " on " + SQLTablePeople + ".ID=ccProperties.KeyID"
                            + " WHERE (ccProperties.TypeID=0)"
                            + " AND (" + SQLTablePeople + ".ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                }
                //
                // Visit Properties with no visits
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting visit properties with no visit record.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        SQL = "delete ccProperties.*"
                            + " from ccProperties LEFT JOIN ccVisits on ccVisits.ID=ccProperties.KeyID"
                            + " WHERE (ccProperties.TypeID=1)"
                            + " AND (ccVisits.ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        SQL = "delete From ccProperties"
                            + " from ccProperties LEFT JOIN ccVisits on ccVisits.ID=ccProperties.KeyID"
                            + " WHERE (ccProperties.TypeID=1)"
                            + " AND (ccVisits.ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                    default:
                        SQL = "delete ccProperties"
                            + " from ccProperties LEFT JOIN ccVisits on ccVisits.ID=ccProperties.KeyID"
                            + " WHERE (ccProperties.TypeID=1)"
                            + " AND (ccVisits.ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                }
                //
                // Visitor Properties with no visitor
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting visitor properties with no visitor record.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        SQL = "delete ccProperties.*"
                            + " from ccProperties LEFT JOIN ccvisitors on ccvisitors.ID=ccProperties.KeyID"
                            + " where ccproperties.typeid=2"
                            + " and ccvisitors.id is null";
                        cp.core.db.executeQuery(SQL);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        SQL = "delete From ccProperties"
                            + " from ccProperties LEFT JOIN ccvisitors on ccvisitors.ID=ccProperties.KeyID"
                            + " where ccproperties.typeid=2"
                            + " and ccvisitors.id is null";
                        cp.core.db.executeQuery(SQL);
                        break;
                    default:
                        SQL = "delete ccProperties"
                            + " from ccProperties LEFT JOIN ccvisitors on ccvisitors.ID=ccProperties.KeyID"
                            + " where ccproperties.typeid=2"
                            + " and ccvisitors.id is null";
                        cp.core.db.executeQuery(SQL);
                        break;
                }
                //
                // MemberRules with bad MemberID
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting Member Rules with bad MemberID.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        SQL = "delete " + SQLTableMemberRules + ".*"
                            + " From " + SQLTableMemberRules + ""
                            + " LEFT JOIN " + SQLTablePeople + " on " + SQLTablePeople + ".ID=" + SQLTableMemberRules + ".MemberID"
                            + " WHERE (" + SQLTablePeople + ".ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        SQL = "delete From " + SQLTableMemberRules + ""
                            + " From " + SQLTableMemberRules + ""
                            + " LEFT JOIN " + SQLTablePeople + " on " + SQLTablePeople + ".ID=" + SQLTableMemberRules + ".MemberID"
                            + " WHERE (" + SQLTablePeople + ".ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                    default:
                        SQL = "delete " + SQLTableMemberRules + ""
                            + " From " + SQLTableMemberRules + ""
                            + " LEFT JOIN " + SQLTablePeople + " on " + SQLTablePeople + ".ID=" + SQLTableMemberRules + ".MemberID"
                            + " WHERE (" + SQLTablePeople + ".ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                }
                //
                // MemberRules with bad GroupID
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting Member Rules with bad GroupID.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        SQL = "delete " + SQLTableMemberRules + ".*"
                            + " From " + SQLTableMemberRules + ""
                            + " LEFT JOIN " + SQLTableGroups + " on " + SQLTableGroups + ".ID=" + SQLTableMemberRules + ".GroupID"
                            + " WHERE (" + SQLTableGroups + ".ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        SQL = "delete From " + SQLTableMemberRules + ""
                            + " From " + SQLTableMemberRules + ""
                            + " LEFT JOIN " + SQLTableGroups + " on " + SQLTableGroups + ".ID=" + SQLTableMemberRules + ".GroupID"
                            + " WHERE (" + SQLTableGroups + ".ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                    default:
                        SQL = "delete " + SQLTableMemberRules + ""
                            + " From " + SQLTableMemberRules + ""
                            + " LEFT JOIN " + SQLTableGroups + " on " + SQLTableGroups + ".ID=" + SQLTableMemberRules + ".GroupID"
                            + " WHERE (" + SQLTableGroups + ".ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                }
                //
                // GroupRules with bad ContentID
                //   Handled record by record removed to prevent CDEF reload
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting Group Rules with bad ContentID.");
                SQL = "Select ccGroupRules.ID"
                    + " From ccGroupRules LEFT JOIN ccContent on ccContent.ID=ccGroupRules.ContentID"
                    + " WHERE (ccContent.ID is null)";
                CS = cp.core.db.csOpenSql_rev("default", SQL);
                while (cp.core.db.csOk(CS)) {
                    cp.core.db.deleteContentRecord("Group Rules", cp.core.db.csGetInteger(CS, "ID"));
                    cp.core.db.csGoNext(CS);
                }
                cp.core.db.csClose(ref CS);
                //
                // GroupRules with bad GroupID
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting Group Rules with bad GroupID.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        SQL = "delete ccGroupRules.*"
                            + " From ccGroupRules"
                            + " LEFT JOIN " + SQLTableGroups + " on " + SQLTableGroups + ".ID=ccGroupRules.GroupID"
                            + " WHERE (" + SQLTableGroups + ".ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        SQL = "delete from ccGroupRules"
                            + " From ccGroupRules"
                            + " LEFT JOIN " + SQLTableGroups + " on " + SQLTableGroups + ".ID=ccGroupRules.GroupID"
                            + " WHERE (" + SQLTableGroups + ".ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                    default:
                        SQL = "delete ccGroupRules"
                            + " From ccGroupRules"
                            + " LEFT JOIN " + SQLTableGroups + " on " + SQLTableGroups + ".ID=ccGroupRules.GroupID"
                            + " WHERE (" + SQLTableGroups + ".ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                }
                //
                // TopicRules with bad ContentID
                // delete manually to prevent cdef reload
                //
                //Call AppendClassLog(AppName, "HouseKeep_App_Daily(" & AppName & ")", "Deleting Topic Rules with bad ContentID.")
                //SQL = "Select ccTopicRules.ID" _
                //    & " From ccTopicRules LEFT JOIN ccContent on ccContent.ID=ccTopicRules.ContentID" _
                //    & " WHERE (ccContent.ID is null)"
                //CS = cp.Core.app.csv_OpenCSSQL("default", SQL)
                //Do While cp.Core.app.csv_IsCSOK(CS)
                //    Call cp.Core.csv_DeleteContentRecord("Topic Rules", cp.Core.app.csv_cs_getInteger(CS, "ID"))
                //    Call cp.Core.app.csv_NextCSRecord(CS)
                //    Loop
                //Call cp.Core.app.csv_CloseCS(CS)
                //
                // TopicRules with bad TopicID
                //
                //Call AppendClassLog(AppName, "HouseKeep_App_Daily(" & AppName & ")", "Deleting Topic Rules with bad TopicID.")
                //Select Case DataSourceType
                //    Case DataSourceTypeODBCAccess
                //        SQL = "delete ccTopicRules.*" _
                //            & " From ccTopicRules" _
                //            & " LEFT JOIN ccTopics on ccTopics.ID=ccTopicRules.topicID" _
                //            & " WHERE (ccTopics.ID is null)"
                //        Call cp.Core.app.executeSql(sql)
                //    Case DataSourceTypeODBCSQLServer
                //        SQL = "delete from ccTopicRules" _
                //            & " From ccTopicRules" _
                //            & " LEFT JOIN ccTopics on ccTopics.ID=ccTopicRules.topicID" _
                //            & " WHERE (ccTopics.ID is null)"
                //        Call cp.Core.app.executeSql(sql)
                //    Case Else
                //        SQL = "delete ccTopicRules" _
                //            & " From ccTopicRules" _
                //            & " LEFT JOIN ccTopics on ccTopics.ID=ccTopicRules.topicID" _
                //            & " WHERE (ccTopics.ID is null)"
                //        Call cp.Core.app.executeSql(sql)
                //End Select
                //
                // ContentWatch with bad CContentID
                //     must be deleted manually
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting Content Watch with bad ContentID.");
                SQL = "Select ccContentWatch.ID"
                    + " From ccContentWatch LEFT JOIN ccContent on ccContent.ID=ccContentWatch.ContentID"
                    + " WHERE (ccContent.ID is null)or(ccContent.Active=0)or(ccContent.Active is null)";
                CS = cp.core.db.csOpenSql_rev("default", SQL);
                while (cp.core.db.csOk(CS)) {
                    cp.core.db.deleteContentRecord("Content Watch", cp.core.db.csGetInteger(CS, "ID"));
                    cp.core.db.csGoNext(CS);
                }
                cp.core.db.csClose(ref CS);
                //
                // ContentWatchListRules with bad ContentWatchID
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting ContentWatchList Rules with bad ContentWatchID.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        SQL = "delete ccContentWatchListRules.*"
                            + " From ccContentWatchListRules"
                            + " LEFT JOIN ccContentWatch on ccContentWatch.ID=ccContentWatchListRules.ContentWatchID"
                            + " WHERE (ccContentWatch.ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        SQL = "delete from ccContentWatchListRules"
                            + " From ccContentWatchListRules"
                            + " LEFT JOIN ccContentWatch on ccContentWatch.ID=ccContentWatchListRules.ContentWatchID"
                            + " WHERE (ccContentWatch.ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                    default:
                        SQL = "delete ccContentWatchListRules"
                            + " From ccContentWatchListRules"
                            + " LEFT JOIN ccContentWatch on ccContentWatch.ID=ccContentWatchListRules.ContentWatchID"
                            + " WHERE (ccContentWatch.ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                }
                //
                // ContentWatchListRules with bad ContentWatchListID
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting ContentWatchList Rules with bad ContentWatchListID.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        SQL = "delete ccContentWatchListRules.*"
                            + " From ccContentWatchListRules"
                            + " LEFT JOIN ccContentWatchLists on ccContentWatchLists.ID=ccContentWatchListRules.ContentWatchListID"
                            + " WHERE (ccContentWatchLists.ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        SQL = "delete from ccContentWatchListRules"
                            + " From ccContentWatchListRules"
                            + " LEFT JOIN ccContentWatchLists on ccContentWatchLists.ID=ccContentWatchListRules.ContentWatchListID"
                            + " WHERE (ccContentWatchLists.ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                    default:
                        SQL = "delete ccContentWatchListRules"
                            + " From ccContentWatchListRules"
                            + " LEFT JOIN ccContentWatchLists on ccContentWatchLists.ID=ccContentWatchListRules.ContentWatchListID"
                            + " WHERE (ccContentWatchLists.ID is null)";
                        cp.core.db.executeQuery(SQL);
                        break;
                }
                //
                // Field help with no field
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting field help with no field.");
                SQL = ""
                    + "delete from ccfieldhelp where id in ("
                    + " select h.id"
                    + " from ccfieldhelp h"
                    + " left join ccfields f on f.id=h.fieldid where f.id is null"
                    + ")";
                cp.core.db.executeQuery(SQL);
                //
                // Field help duplicates - messy, but I am not sure where they are coming from, and this patchs the edit page performance problem
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Deleting duplicate field help records.");
                SQL = ""
                    + "delete from ccfieldhelp where id in ("
                    + " select b.id"
                    + " from ccfieldhelp a"
                    + " left join ccfieldhelp b on a.fieldid=b.fieldid where a.id< b.id"
                    + ")";
                cp.core.db.executeQuery(SQL);
                //
                //addon editor rules with no addon
                //
                SQL = "delete from ccAddonContentFieldTypeRules where id in ("
                    + "select r.id from ccAddonContentFieldTypeRules r left join ccaggregatefunctions a on a.id=r.addonid where a.Id Is Null"
                    + ")";
                cp.core.db.executeQuery(SQL);
                //
                // convert FieldTypeLongText + htmlContent to FieldTypeHTML
                //
                AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "convert FieldTypeLongText + htmlContent to FieldTypeHTML.");
                SQL = "update ccfields set type=" + FieldTypeIdHTML + " where type=" + FieldTypeIdLongText + " and ( htmlcontent<>0 )";
                cp.core.db.executeQuery(SQL);
                //
                // convert FieldTypeTextFile + htmlContent to FieldTypeHTMLFile
                //
                //Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "convert FieldTypeTextFile + htmlContent to FieldTypeHTMLFile.")
                //SQL = "update ccfields set type=" & FieldTypeIdFileHTMLPrivate & " where type=" & FieldTypeIdFileTextPrivate & " and ( htmlcontent<>0 )"
                //Call cp.core.app.executeSql(SQL)
                //
                // Log files Older then 30 days
                //
                HouseKeep_App_Daily_LogFolder("temp", thirtyDaysAgo);
                HouseKeep_App_Daily_LogFolder("TrapLogs", thirtyDaysAgo);
                HouseKeep_App_Daily_LogFolder("BounceLog", thirtyDaysAgo);
                HouseKeep_App_Daily_LogFolder("BounceProcessing", thirtyDaysAgo);
                HouseKeep_App_Daily_LogFolder("SMTPLog", thirtyDaysAgo);
                HouseKeep_App_Daily_LogFolder("DebugLog", thirtyDaysAgo);
                //
                // Content TextFile types with no controlling record
                //
                if (genericController.encodeBoolean(cp.core.siteProperties.getText("ArchiveAllowFileClean", "false"))) {
                    //
                    int DSType = cp.core.db.getDataSourceType("");
                    AppendClassLog(cp.core, appName, "HouseKeep_App_Daily(" + appName + ")", "Content TextFile types with no controlling record.");
                    SQL = "SELECT DISTINCT ccTables.Name as TableName, ccFields.Name as FieldName"
                        + " FROM (ccFields LEFT JOIN ccContent ON ccFields.ContentID = ccContent.ID) LEFT JOIN ccTables ON ccContent.ContentTableID = ccTables.ID"
                        + " Where (((ccFields.Type) = 10))"
                        + " ORDER BY ccTables.Name";
                    CS = cp.core.db.csOpenSql_rev("Default", SQL);
                    while (cp.core.db.csOk(CS)) {
                        //
                        // Get all the files in this path, and check that the record exists with this in its field
                        //
                        FieldName = cp.core.db.csGetText(CS, "FieldName");
                        TableName = cp.core.db.csGetText(CS, "TableName");
                        PathName = TableName + "\\" + FieldName;
                        FileList = cp.core.cdnFiles.getFileList(PathName);
                        if (FileList.Length > 0) {
                            cp.core.db.executeQuery("CREATE INDEX temp" + FieldName + " ON " + TableName + " (" + FieldName + ")");
                            foreach (FileInfo file in FileList) {
                                Filename = file.Name;
                                VirtualFileName = PathName + "\\" + Filename;
                                VirtualLink = genericController.vbReplace(VirtualFileName, "\\", "/");
                                FileSize = file.Length;
                                if (FileSize == 0) {
                                    SQL = "update " + TableName + " set " + FieldName + "=null where (" + FieldName + "=" + cp.core.db.encodeSQLText(VirtualFileName) + ")or(" + FieldName + "=" + cp.core.db.encodeSQLText(VirtualLink) + ")";
                                    cp.core.db.executeQuery(SQL);
                                    cp.core.cdnFiles.deleteFile(VirtualFileName);
                                } else {
                                    SQL = "SELECT ID FROM " + TableName + " WHERE (" + FieldName + "=" + cp.core.db.encodeSQLText(VirtualFileName) + ")or(" + FieldName + "=" + cp.core.db.encodeSQLText(VirtualLink) + ")";
                                    CSTest = cp.core.db.csOpenSql_rev("default", SQL);
                                    if (!cp.core.db.csOk(CSTest)) {
                                        cp.core.cdnFiles.deleteFile(VirtualFileName);
                                    }
                                    cp.core.db.csClose(ref CSTest);
                                }
                            }
                            if (DSType == 1) {
                                // access
                                SQL = "Drop INDEX temp" + FieldName + " ON " + TableName;
                            } else if (DSType == 2) {
                                // sql server
                                SQL = "DROP INDEX " + TableName + ".temp" + FieldName;
                            } else {
                                // mysql
                                SQL = "ALTER TABLE " + TableName + " DROP INDEX temp" + FieldName;
                            }
                            cp.core.db.executeQuery(SQL);
                        }
                        cp.core.db.csGoNext(CS);
                    }
                    cp.core.db.csClose(ref CS);
                    //
                    // problem here is 1) images may have resized images in the folder
                    // 2) files may be in the wrong recordID if workflow.
                    //
                    //        '
                    //        ' Content File types with no controlling record
                    //        '
                    //        SQL = "SELECT DISTINCT ccTables.Name as TableName, ccFields.Name as FieldName" _
                    //            & " FROM (ccFields LEFT JOIN ccContent ON ccFields.ContentID = ccContent.ID) LEFT JOIN ccTables ON ccContent.ContentTableID = ccTables.ID" _
                    //            & " Where ((ccFields.Type=6)OR(ccFields.Type=11)OR(ccFields.Type=16)OR(ccFields.Type=17)OR(ccFields.Type=18))" _
                    //            & " ORDER BY ccTables.Name"
                    //        CS = cp.Core.app.csv_OpenCSSQL("Default", SQL)
                    //        Do While cp.Core.app.csv_IsCSOK(CS)
                    //            '
                    //            ' Get all the files in this path, and check that the record exists with this in its field
                    //            '
                    //            FieldName = cp.Core.app.csv_cs_getText(CS, "FieldName")
                    //            TableName = cp.Core.app.csv_cs_getText(CS, "TableName")
                    //            If cp.Core.csv_IsSQLTableField("Default", TableName, FieldName) Then
                    //                PathName = TableName & "\" & FieldName
                    //                PathNameRev = TableName & "/" & FieldName
                    //                FolderList = cp.Core.contentFiles.getFolderList(PathName)
                    //                If FolderList <> "" Then
                    //                    FolderArray = Split(FolderList, vbCrLf)
                    //                    FolderArrayCount = UBound(FolderArray) + 1
                    //                    For FolderArrayPointer = 0 To FolderArrayCount - 1
                    //                        If FolderArray(FolderArrayPointer) <> "" Then
                    //                            FolderSplit = Split(FolderArray(FolderArrayPointer), ",")
                    //                            FolderName = FolderSplit(0)
                    //                            '
                    //                            ' just verify the record exists -- all files in the folder are valid
                    //                            '
                    //                            SQL = "select id from " & TableName & " where id=" & encodeSQLNumber(FolderName)
                    //                            CSTest = cp.Core.app.csv_OpenCSSQL("default", SQL)
                    //                            If Not cp.Core.app.csv_IsCSOK(CSTest) Then
                    //                            '    Call cp.Core.csv_DeleteVirtualFolder(PathNameRev & "\" & FolderName)
                    //                            End If
                    //                            Call cp.Core.app.csv_CloseCS(CSTest)
                    //
                    //                            FileList = cp.Core.csv_GetVirtualFileList(PathName & "\" & FolderName)
                    //                            If FileList <> "" Then
                    //                                FileArray = Split(FileList, vbCrLf)
                    //                                FileArrayCount = UBound(FileArray) + 1
                    //                                For FileArrayPointer = 0 To FileArrayCount - 1
                    //                                    If FileArray(FileArrayPointer) <> "" Then
                    //                                        FileSplit = Split(FileArray(FileArrayPointer), ",")
                    //                                        FilenameOriginal = FileSplit(0)
                    //                                        Filename = FilenameOriginal
                    //                                        FilenameAltSize = ""
                    //                                        Pos = InStrRev(Filename, ".")
                    //
                    //                                        If Pos > 0 Then
                    //                                            FilenameExt = Mid(Filename, Pos + 1)
                    //                                            FilenameNoExt = Mid(Filename, 1, Pos - 1)
                    //                                            Pos = InStrRev(FilenameNoExt, "-")
                    //                                            If Pos > 0 Then
                    //                                                FilenameAltSize = Mid(FilenameNoExt, Pos + 1)
                    //                                                If FilenameAltSize <> "" Then
                    //                                                FilenameDim = Split(FilenameAltSize, "x")
                    //                                                If UBound(FilenameDim) = 1 Then
                    //                                                    If genericController.vbIsNumeric(FilenameDim(0)) And genericController.vbIsNumeric(FilenameDim(1)) Then
                    //                                                        FilenameNoExt = Mid(FilenameNoExt, 1, Pos - 1)
                    //                                                    End If
                    //                                                End If
                    //                                                End If
                    //                                            End If
                    //                                            Filename = FilenameNoExt & "." & FilenameExt
                    //                                        End If
                    //                                        If FilenameAltSize <> "" Then
                    //                                            SQL = "SELECT ID FROM " & TableName & " WHERE (" & FieldName & "=" & encodeSQLText(PathNameRev & "/" & FolderName & "/" & FilenameOriginal) & ")or(" & FieldName & "=" & encodeSQLText(PathNameRev & "/" & FolderName & "/" & Filename) & ")"
                    //                                        Else
                    //                                            SQL = "SELECT ID FROM " & TableName & " WHERE " & FieldName & "=" & encodeSQLText(PathNameRev & "/" & FolderName & "/" & FilenameOriginal)
                    //                                        End If
                    //                                        CSTest = cp.Core.app.csv_OpenCSSQL("default", SQL)
                    //                                        If Not cp.Core.app.csv_IsCSOK(CSTest) Then
                    //                                            Call cp.Core.virtualFiles.DeleteFile(PathNameRev & "/" & FolderName & "/" & FilenameOriginal)
                    //                                        End If
                    //                                        Call cp.Core.app.csv_CloseCS(CSTest)
                    //                                    End If
                    //                                Next
                    //                            End If
                    //                        End If
                    //                    Next
                    //                End If
                    //            End If
                    //            Call cp.Core.app.csv_NextCSRecord(CS)
                    //        Loop
                    //        Call cp.Core.app.csv_CloseCS(CS)
                }
                cp.core.db.sqlCommandTimeout = TimeoutSave;
                return;
                //
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
        }
        //
        //
        //
        private void HouseKeep_App_Daily_RemoveVisitRecords(DateTime DeleteBeforeDate, int DataSourceType) {
            try {
                //
                int TimeoutSave = 0;
                //Dim Controller As controlClass
                int VisitArchiveAgeDays = 0;
                int VisitArchiveDeleteSize = 0;
                //Dim AppService As appServicesClass
                //Dim KernelService As KernelServicesClass
                //      Dim CSConnection As appEnvironmentStruc
                string SQL = null;
                string SQLCriteria = null;
                string PathName = null;
                string TableName = null;
                string FieldName = null;
                string FileList = null;
                string[] FileArray = null;
                int FileArrayCount = 0;
                int FileArrayPointer = 0;
                int CS = 0;
                int CSTest = 0;
                string Filename = null;
                string[] FileSplit = null;
                string FolderName = null;
                string FolderList = null;
                string[] FolderArray = null;
                int FolderArrayCount = 0;
                int FolderArrayPointer = 0;
                string[] FolderSplit = null;
                int AdminLicenseCount = 0;
                string DeleteBeforeDateSQL = null;
                string appName = null;
                string SQLTablePeople = null;
                //Dim SQLTableMemberRules As String
                //Dim SQLTableGroups As String
                //Dim PeopleCID as integer
                string DefaultName = null;
                //
                // Set long timeout (30 min) needed for heavy work on big tables
                //
                TimeoutSave = cp.core.db.sqlCommandTimeout;
                cp.core.db.sqlCommandTimeout = 1800;
                //
                SQLTablePeople = Models.Complex.cdefModel.getContentTablename(cp.core, "People");
                //SQLTableMemberRules = cp.Core.csv_GetContentTablename("Member Rules")
                //SQLTableGroups = cp.Core.csv_GetContentTablename("Groups")
                //
                //VisitArchiveAgeDays = genericController.EncodeInteger(cp.Core.csv_GetSiteProperty("ArchiveRecordAgeDays", "0"))
                if (true) {
                    appName = cp.core.serverConfig.appConfig.name;
                    DeleteBeforeDateSQL = cp.core.db.encodeSQLDate(DeleteBeforeDate);
                    //
                    // Visits older then archive age
                    //
                    AppendClassLog(cp.core, cp.core.serverConfig.appConfig.name, "HouseKeep_App_Daily_RemoveVisitRecords(" + appName + ")", "Deleting visits before [" + DeleteBeforeDateSQL + "]");
                    cp.core.db.DeleteTableRecordChunks("default", "ccVisits", "(DateAdded<" + DeleteBeforeDateSQL + ")", 1000, 10000);
                    //
                    // Viewings with visits before the first
                    //
                    AppendClassLog(cp.core, appName, "HouseKeep_App_Daily_RemoveVisitRecords(" + appName + ")", "Deleting viewings with visitIDs lower then the lowest ccVisits.ID");
                    cp.core.db.DeleteTableRecordChunks("default", "ccviewings", "(visitid<(select min(ID) from ccvisits))", 1000, 10000);
                    //
                    // Visitors with no visits
                    //
                    AppendClassLog(cp.core, appName, "HouseKeep_App_Daily_RemoveVisitRecords(" + appName + ")", "Deleting visitors with no visits");
                    switch (DataSourceType) {
                        case DataSourceTypeODBCAccess:
                            SQL = "delete ccVisitors.*"
                                + " from ccVisitors Left Join ccVisits on ccVisits.VisitorID=ccVisitors.ID"
                                + " where ccVisits.ID is null";
                            cp.core.db.executeQuery(SQL);

                            break;
                        case DataSourceTypeODBCSQLServer:
                            SQL = "delete From ccVisitors"
                                + " from ccVisitors Left Join ccVisits on ccVisits.VisitorID=ccVisitors.ID"
                                + " where ccVisits.ID is null";
                            cp.core.db.executeQuery(SQL);
                            break;
                        default:
                            SQL = "delete ccVisitors"
                                + " from ccVisitors Left Join ccVisits on ccVisits.VisitorID=ccVisitors.ID"
                                + " where ccVisits.ID is null";
                            cp.core.db.executeQuery(SQL);
                            break;
                    }
                    //        '
                    //        ' Delete People
                    //        '   created before DeleteBeforeDate,
                    //        '   created during a visit (not created by another process),
                    //        '   with default name (created during a hit)
                    //        '   with no username (they are not planning on returning)
                    //        '   with 1 visit (not created with 0 visits, has not returned)
                    //        '
                    //        Call AppendClassLog(AppName, "HouseKeep_App_Daily_RemoveVisitRecords(" & AppName & ")", "Deleting members with default name [" & DefaultName & "], LastVisit before DeleteBeforeDate [" & DeleteBeforeDate & "], exactly one total visit, a null username and a null email address.")
                    //        SQLCriteria = "" _
                    //            & "(" & SQLTablePeople & ".Name=" & encodeSQLText(DefaultName) & ")" _
                    //            & " and(LastVisit<" & DeleteBeforeDateSQL & ")" _
                    //            & " and(createdbyvisit=1)" _
                    //            & " and(Visits=1)" _
                    //            & " and(Username is null)" _
                    //            & " and(email is null)"
                    //        Call cp.Core.csv_DeleteTableRecordChunks("default", "" & SQLTablePeople & "", SQLCriteria, 1000, 10000)
                }
                //
                // restore sved timeout
                //
                cp.core.db.sqlCommandTimeout = TimeoutSave;
                return;
                //
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
        }
        //
        //
        //
        private void HouseKeep_App_Daily_RemoveGuestRecords(DateTime DeleteBeforeDate, int DataSourceType) {
            try {
                //
                int TimeoutSave = 0;
                //Dim Controller As controlClass
                int VisitArchiveAgeDays = 0;
                int VisitArchiveDeleteSize = 0;
                //Dim AppService As appServicesClass
                //Dim KernelService As KernelServicesClass
                //   Dim CSConnection As appEnvironmentStruc
                string SQL = null;
                string SQLCriteria = null;
                string PathName = null;
                string TableName = null;
                string FieldName = null;
                string FileList = null;
                string[] FileArray = null;
                int FileArrayCount = 0;
                int FileArrayPointer = 0;
                int CS = 0;
                int CSTest = 0;
                string Filename = null;
                string[] FileSplit = null;
                string FolderName = null;
                string FolderList = null;
                string[] FolderArray = null;
                int FolderArrayCount = 0;
                int FolderArrayPointer = 0;
                string[] FolderSplit = null;
                int AdminLicenseCount = 0;
                string DeleteBeforeDateSQL = null;
                string appName = null;
                string SQLTablePeople = null;
                //Dim SQLTableMemberRules As String
                //Dim SQLTableGroups As String
                //Dim PeopleCID as integer
                string DefaultName = null;
                //
                // Set long timeout (30 min) needed for heavy work on big tables
                //
                TimeoutSave = cp.core.db.sqlCommandTimeout;
                cp.core.db.sqlCommandTimeout = 1800;
                //
                SQLTablePeople = Models.Complex.cdefModel.getContentTablename(cp.core, "People");
                //SQLTableMemberRules = cp.Core.csv_GetContentTablename("Member Rules")
                //SQLTableGroups = cp.Core.csv_GetContentTablename("Groups")
                //
                //VisitArchiveAgeDays = genericController.EncodeInteger(cp.Core.csv_GetSiteProperty("ArchiveRecordAgeDays", "0"))
                if (true) {
                    appName = cp.core.serverConfig.appConfig.name;
                    DeleteBeforeDateSQL = cp.core.db.encodeSQLDate(DeleteBeforeDate);
                    //        '
                    //        ' Visits older then archive age
                    //        '
                    //        Call AppendClassLog(cp.core, cp.core.appEnvironment.name, "HouseKeep_App_Daily_RemoveGuestRecords(" & AppName & ")", "Deleting visits before [" & DeleteBeforeDateSQL & "]")
                    //        Call cp.Core.csv_DeleteTableRecordChunks("default", "ccVisits", "(DateAdded<" & DeleteBeforeDateSQL & ")", 1000, 10000)
                    //        '
                    //        ' Viewings with visits before the first
                    //        '
                    //        Call AppendClassLog(AppName, "HouseKeep_App_Daily_RemoveGuestRecords(" & AppName & ")", "Deleting viewings with visitIDs lower then the lowest ccVisits.ID")
                    //        Call cp.Core.csv_DeleteTableRecordChunks("default", "ccviewings", "(visitid<(select min(ID) from ccvisits))", 1000, 10000)
                    //        '
                    //        ' Visitors with no visits
                    //        '
                    //        Call AppendClassLog(AppName, "HouseKeep_App_Daily_RemoveGuestRecords(" & AppName & ")", "Deleting visitors with no visits")
                    //        Select Case DataSourceType
                    //            Case DataSourceTypeODBCAccess
                    //                SQL = "delete ccVisitors.*" _
                    //                    & " from ccVisitors Left Join ccVisits on ccVisits.VisitorID=ccVisitors.ID" _
                    //                    & " where ccVisits.ID is null"
                    //                Call cp.Core.app.executeSql(sql)
                    //
                    //            Case DataSourceTypeODBCSQLServer
                    //                SQL = "delete From ccVisitors" _
                    //                    & " from ccVisitors Left Join ccVisits on ccVisits.VisitorID=ccVisitors.ID" _
                    //                    & " where ccVisits.ID is null"
                    //                Call cp.Core.app.executeSql(sql)
                    //            Case Else
                    //                SQL = "delete ccVisitors" _
                    //                    & " from ccVisitors Left Join ccVisits on ccVisits.VisitorID=ccVisitors.ID" _
                    //                    & " where ccVisits.ID is null"
                    //                Call cp.Core.app.executeSql(sql)
                    //        End Select
                    //
                    // Delete People
                    //   created before DeleteBeforeDate,
                    //   created during a visit (not created by another process),
                    //   x with default name (created during a hit) - no, spider detect changes name
                    //   with no username (they are not planning on returning)
                    //   with 1 visit (not created with 0 visits, has not returned)
                    //
                    AppendClassLog(cp.core, appName, "HouseKeep_App_Daily_RemoveGuestRecords(" + appName + ")", "Deleting members with  LastVisit before DeleteBeforeDate [" + DeleteBeforeDate + "], exactly one total visit, a null username and a null email address.");
                    SQLCriteria = ""
                        + " (LastVisit<" + DeleteBeforeDateSQL + ")"
                        + " and(createdbyvisit=1)"
                        + " and(Visits=1)"
                        + " and(Username is null)"
                        + " and(email is null)";
                    cp.core.db.DeleteTableRecordChunks("default", "" + SQLTablePeople + "", SQLCriteria, 1000, 10000);
                    //& "(" & SQLTablePeople & ".Name=" & encodeSQLText(DefaultName) & ")"
                }
                //
                // restore sved timeout
                //
                cp.core.db.sqlCommandTimeout = TimeoutSave;
                return;
                //
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
        }
        //
        //=========================================================================================
        // Summarize the visits
        //   excludes non-cookie visits
        //   excludes administrator and developer visits
        //   excludes authenticated users with ExcludeFromReporting
        //
        // Average time on site
        //
        //   Example data
        //   Pages       TimeToLastHit
        //   1           0           - hit 1 page, start time = last time
        //   10          3510        - hit 10 pages, first hit time - last hit time = 3510
        //   2           30          - hit 2 pages, first hit time - last hit time = 30
        //
        // AveReadTime is the average time spent reading pages
        //   this is calculated from the multi-page visits only
        //   = MultiPageTimeToLastHitSum / ( MultiPageHitCnt - MultiPageVisitCnt )
        //   = ( 3510 + 30 ) / ((10+2) - 2 )
        //   = 354
        //
        // TotalTimeOnSite is the total time people spent reading pages
        //   There are two parts:
        //     1) the TimeToLastHit, which covers all but the last hit of each visit
        //     2) assume the last hit of each visit is the AveReadTime
        //   = MultiPageTimeToLastHitSum + ( AveReadTime * VisitCnt )
        //   = ( 3510 + 30 ) + ( 354 * 3 )
        //   = 4602
        //
        // AveTimeOnSite
        //   = TotalTimeOnSite / TotalHits
        //   = 4602 / 3
        //   = 1534
        //
        //=========================================================================================
        //
        public void HouseKeep_VisitSummary(DateTime StartTimeDate, DateTime EndTimeDate, int HourDuration, string BuildVersion, DateTime OldestVisitSummaryWeCareAbout) {
            try {
                //
                //
                //Dim StartDate As Date
                double StartTimeHoursSinceMidnight = 0;
                DateTime PeriodStart = default(DateTime);
                double TotalTimeOnSite = 0;
                int MultiPageVisitCnt = 0;
                int MultiPageHitCnt = 0;
                double MultiPageTimetoLastHitSum = 0;
                double TimeOnSite = 0;
                //Dim PeriodStepInHours As Double
                DateTime PeriodDatePtr = default(DateTime);
                DateTime StartOfHour = default(DateTime);
                int DateNumber = 0;
                int TimeNumber = 0;
                double SumStartTime = 0;
                double SumStopTime = 0;
                int HoursPerDay = 0;
                DateTime DateStart = default(DateTime);
                DateTime DateEnd = default(DateTime);
                int NewVisitorVisits = 0;
                int SinglePageVisits = 0;
                int AuthenticatedVisits = 0;
                int MobileVisits = 0;
                int BotVisits = 0;
                int NoCookieVisits = 0;
                double AveTimeOnSite = 0;
                int HitCnt = 0;
                int VisitCnt = 0;
                DateTime OldestDateAdded = default(DateTime);
                object EmptyVariant = null;
                bool NeedToClearCache = false;
                int ArchiveParentID = 0;
                int RecordID = 0;
                int CS = 0;
                int LoopPtr = 0;
                int Ptr = 0;
                string LocalFile = null;
                string LocalFilename = null;
                string[] Folders = null;
                int FolderCnt = 0;
                string CollectionGUID = null;
                string CollectionName = null;
                int Pos = 0;
                string LastChangeDate = null;
                string SubFolderList = null;
                string[] SubFolders = null;
                string SubFolder = null;
                int Cnt = 0;
                string LocalGUID = null;
                string LocalLastChangeDateStr = null;
                DateTime LocalLastChangeDate = default(DateTime);
                string LibGUID = null;
                string LibLastChangeDateStr = null;
                DateTime LibLastChangeDate = default(DateTime);
                XmlNode LibListNode = null;
                XmlNode LocalListNode = null;
                XmlNode CollectionNode = null;
                XmlDocument LibraryCollections = new XmlDocument();
                XmlDocument LocalCollections = new XmlDocument();
                XmlDocument Doc = new XmlDocument();
                //Dim AppService As appServicesClass
                //Dim KernelService As KernelServicesClass
                string SetTimeCheckString = null;
                double SetTimeCheck = 0;
                DateTime LogDate = default(DateTime);
                string FolderName = null;
                string FileList = null;
                string[] FileArray = null;
                int FileArrayCount = 0;
                int FileArrayPointer = 0;
                string[] FileSplit = null;
                string FolderList = null;
                string[] FolderArray = null;
                int FolderArrayCount = 0;
                int FolderArrayPointer = 0;
                string[] FolderSplit = null;
                //Dim fs As New fileSystemClass
                int VisitArchiveAgeDays = 0;
                bool NewDay = false;
                bool NewHour = false;
                //
                DateTime LastTimeCheck = default(DateTime);
                //
                string ConfigFilename = null;
                string Config = null;
                string[] ConfigLines = null;
                //
                string Line = null;
                int LineCnt = 0;
                int LinePtr = 0;
                string[] NameValue = null;
                string SQLNow = null;
                string SQL = null;
                int AveReadTime = 0;
                //Dim AddonInstall As New addonInstallClass
                //
                if (string.CompareOrdinal(BuildVersion, cp.Version) < 0) {
                    //throw new ApplicationException("Unexpected exception");
                } else {
                    PeriodStart = StartTimeDate;
                    if (PeriodStart < OldestVisitSummaryWeCareAbout) {
                        PeriodStart = OldestVisitSummaryWeCareAbout;
                    }
                    StartTimeHoursSinceMidnight = PeriodStart.TimeOfDay.TotalHours;
                    PeriodStart = PeriodStart.Date.AddHours(StartTimeHoursSinceMidnight);
                    //PeriodStepInHours = CDbl(HourDuration) / 24.0!
                    PeriodDatePtr = PeriodStart;
                    while (PeriodDatePtr < EndTimeDate) {
                        //
                        DateNumber = EncodeInteger(PeriodDatePtr.AddHours(HourDuration / 2.0).ToOADate());
                        TimeNumber = EncodeInteger(PeriodDatePtr.TimeOfDay.TotalHours);
                        DateStart = PeriodDatePtr.Date;
                        DateEnd = PeriodDatePtr.AddHours(HourDuration).Date;
                        //
                        VisitCnt = 0;
                        HitCnt = 0;
                        SumStartTime = 0;
                        SumStopTime = 0;
                        NewVisitorVisits = 0;
                        SinglePageVisits = 0;
                        MultiPageVisitCnt = 0;
                        MultiPageTimetoLastHitSum = 0;
                        AuthenticatedVisits = 0;
                        MobileVisits = 0;
                        BotVisits = 0;
                        NoCookieVisits = 0;
                        AveTimeOnSite = 0;
                        //
                        // No Cookie Visits
                        //
                        SQL = "select count(v.id) as NoCookieVisits"
                            + " from ccvisits v"
                            + " where (v.CookieSupport<>1)"
                            + " and(v.dateadded>=" + cp.core.db.encodeSQLDate(DateStart) + ")"
                            + " and (v.dateadded<" + cp.core.db.encodeSQLDate(DateEnd) + ")"
                            + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                            + "";
                        //SQL = "select count(id) as NoCookieVisits from ccvisits where (CookieSupport<>1)and(dateadded>=" & encodeSQLDate(DateStart) & ")and(dateadded<" & encodeSQLDate(DateEnd) & ")"
                        CS = cp.core.db.csOpenSql_rev("default", SQL);
                        if (cp.core.db.csOk(CS)) {
                            NoCookieVisits = cp.core.db.csGetInteger(CS, "NoCookieVisits");
                        }
                        cp.core.db.csClose(ref CS);
                        //
                        // Total Visits
                        //
                        SQL = "select count(v.id) as VisitCnt ,Sum(v.PageVisits) as HitCnt ,sum(v.TimetoLastHit) as TimeOnSite"
                            + " from ccvisits v"
                            + " where (v.CookieSupport<>0)"
                            + " and(v.dateadded>=" + cp.core.db.encodeSQLDate(DateStart) + ")"
                            + " and (v.dateadded<" + cp.core.db.encodeSQLDate(DateEnd) + ")"
                            + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                            + "";
                        //SQL = "select count(id) as VisitCnt ,Sum(PageVisits) as HitCnt ,sum(TimetoLastHit) as TimeOnSite from ccvisits where (CookieSupport<>0)and(dateadded>=" & encodeSQLDate(DateStart) & ")and (dateadded<" & encodeSQLDate(DateEnd) & ")"
                        CS = cp.core.db.csOpenSql_rev("default", SQL);
                        if (cp.core.db.csOk(CS)) {
                            VisitCnt = cp.core.db.csGetInteger(CS, "VisitCnt");
                            HitCnt = cp.core.db.csGetInteger(CS, "HitCnt");
                            TimeOnSite = cp.core.db.csGetNumber(CS, "TimeOnSite");
                        }
                        cp.core.db.csClose(ref CS);
                        //
                        // Visits by new visitors
                        //
                        if (VisitCnt > 0) {
                            SQL = "select count(v.id) as NewVisitorVisits"
                                + " from ccvisits v"
                                + " where (v.CookieSupport<>0)"
                                + " and(v.dateadded>=" + cp.core.db.encodeSQLDate(DateStart) + ")"
                                + " and (v.dateadded<" + cp.core.db.encodeSQLDate(DateEnd) + ")"
                                + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                                + " and(v.VisitorNew<>0)"
                                + "";
                            //SQL = "select count(id) as NewVisitorVisits from ccvisits where (CookieSupport<>0)and(VisitorNew<>0)and(dateadded>=" & encodeSQLDate(DateStart) & ")and(dateadded<" & encodeSQLDate(DateEnd) & ")"
                            CS = cp.core.db.csOpenSql_rev("default", SQL);
                            if (cp.core.db.csOk(CS)) {
                                NewVisitorVisits = cp.core.db.csGetInteger(CS, "NewVisitorVisits");
                            }
                            cp.core.db.csClose(ref CS);
                            //
                            // Single Page Visits
                            //
                            SQL = "select count(v.id) as SinglePageVisits"
                                + " from ccvisits v"
                                + " where (v.CookieSupport<>0)"
                                + " and(v.dateadded>=" + cp.core.db.encodeSQLDate(DateStart) + ")"
                                + " and (v.dateadded<" + cp.core.db.encodeSQLDate(DateEnd) + ")"
                                + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                                + " and(v.PageVisits=1)"
                                + "";
                            //SQL = "select count(id) as SinglePageVisits from ccvisits where (CookieSupport<>0)and(PageVisits=1)and(dateadded>=" & encodeSQLDate(DateStart) & ")and(dateadded<" & encodeSQLDate(DateEnd) & ")"
                            CS = cp.core.db.csOpenSql_rev("default", SQL);
                            if (cp.core.db.csOk(CS)) {
                                SinglePageVisits = cp.core.db.csGetInteger(CS, "SinglePageVisits");
                            }
                            cp.core.db.csClose(ref CS);
                            //
                            // Multipage Visits
                            //
                            SQL = "select count(v.id) as VisitCnt ,sum(v.PageVisits) as HitCnt ,sum(v.TimetoLastHit) as TimetoLastHitSum "
                                + " from ccvisits v"
                                + " where (v.CookieSupport<>0)"
                                + " and(v.dateadded>=" + cp.core.db.encodeSQLDate(DateStart) + ")"
                                + " and (v.dateadded<" + cp.core.db.encodeSQLDate(DateEnd) + ")"
                                + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                                + " and(PageVisits>1)"
                                + "";
                            //SQL = "select count(id) as VisitCnt ,sum(PageVisits) as HitCnt ,sum(TimetoLastHit) as TimetoLastHitSum from ccvisits where (CookieSupport<>0)and(PageVisits>1)and(dateadded>=" & encodeSQLDate(DateStart) & ")and (dateadded<" & encodeSQLDate(DateEnd) & ")"
                            CS = cp.core.db.csOpenSql_rev("default", SQL);
                            if (cp.core.db.csOk(CS)) {
                                MultiPageVisitCnt = cp.core.db.csGetInteger(CS, "VisitCnt");
                                MultiPageHitCnt = cp.core.db.csGetInteger(CS, "HitCnt");
                                MultiPageTimetoLastHitSum = cp.core.db.csGetNumber(CS, "TimetoLastHitSum");
                            }
                            cp.core.db.csClose(ref CS);
                            //
                            // Authenticated Visits
                            //
                            SQL = "select count(v.id) as AuthenticatedVisits "
                                + " from ccvisits v"
                                + " where (v.CookieSupport<>0)"
                                + " and(v.dateadded>=" + cp.core.db.encodeSQLDate(DateStart) + ")"
                                + " and (v.dateadded<" + cp.core.db.encodeSQLDate(DateEnd) + ")"
                                + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                                + " and(VisitAuthenticated<>0)"
                                + "";
                            //SQL = "select count(id) as AuthenticatedVisits from ccvisits where (CookieSupport<>0)and(VisitAuthenticated<>0)and(dateadded>=" & encodeSQLDate(DateStart) & ")and(dateadded<" & encodeSQLDate(DateEnd) & ")"
                            CS = cp.core.db.csOpenSql_rev("default", SQL);
                            if (cp.core.db.csOk(CS)) {
                                AuthenticatedVisits = cp.core.db.csGetInteger(CS, "AuthenticatedVisits");
                            }
                            cp.core.db.csClose(ref CS);
                            //
                            if (true) {
                                //
                                // Mobile Visits
                                //
                                SQL = "select count(v.id) as cnt "
                                    + " from ccvisits v"
                                    + " where (v.CookieSupport<>0)"
                                    + " and(v.dateadded>=" + cp.core.db.encodeSQLDate(DateStart) + ")"
                                    + " and (v.dateadded<" + cp.core.db.encodeSQLDate(DateEnd) + ")"
                                    + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                                    + " and(Mobile<>0)"
                                    + "";
                                //SQL = "select count(id) as AuthenticatedVisits from ccvisits where (CookieSupport<>0)and(VisitAuthenticated<>0)and(dateadded>=" & encodeSQLDate(DateStart) & ")and(dateadded<" & encodeSQLDate(DateEnd) & ")"
                                CS = cp.core.db.csOpenSql_rev("default", SQL);
                                if (cp.core.db.csOk(CS)) {
                                    MobileVisits = cp.core.db.csGetInteger(CS, "cnt");
                                }
                                cp.core.db.csClose(ref CS);
                                //
                                // Bot Visits
                                //
                                SQL = "select count(v.id) as cnt "
                                    + " from ccvisits v"
                                    + " where (v.CookieSupport<>0)"
                                    + " and(v.dateadded>=" + cp.core.db.encodeSQLDate(DateStart) + ")"
                                    + " and (v.dateadded<" + cp.core.db.encodeSQLDate(DateEnd) + ")"
                                    + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                                    + " and(Bot<>0)"
                                    + "";
                                CS = cp.core.db.csOpenSql_rev("default", SQL);
                                if (cp.core.db.csOk(CS)) {
                                    BotVisits = cp.core.db.csGetInteger(CS, "cnt");
                                }
                                cp.core.db.csClose(ref CS);
                            }
                            //
                            if ((MultiPageHitCnt > MultiPageVisitCnt) && (HitCnt > 0)) {
                                AveReadTime = EncodeInteger(MultiPageTimetoLastHitSum / (MultiPageHitCnt - MultiPageVisitCnt));
                                TotalTimeOnSite = MultiPageTimetoLastHitSum + (AveReadTime * VisitCnt);
                                AveTimeOnSite = TotalTimeOnSite / VisitCnt;
                            }
                        }
                        //
                        // Add or update the Visit Summary Record
                        //
                        CS = cp.core.db.csOpen("Visit Summary", "(timeduration=" + HourDuration + ")and(DateNumber=" + DateNumber + ")and(TimeNumber=" + TimeNumber + ")");
                        if (!cp.core.db.csOk(CS)) {
                            cp.core.db.csClose(ref CS);
                            CS = cp.core.db.csInsertRecord("Visit Summary", 0);
                        }
                        //
                        if (cp.core.db.csOk(CS)) {
                            cp.core.db.csSet(CS, "name", HourDuration + " hr summary for " + DateTime.FromOADate(DateNumber).ToShortDateString() + " " + TimeNumber + ":00");
                            cp.core.db.csSet(CS, "DateNumber", DateNumber);
                            cp.core.db.csSet(CS, "TimeNumber", TimeNumber);
                            cp.core.db.csSet(CS, "Visits", VisitCnt);
                            cp.core.db.csSet(CS, "PagesViewed", HitCnt);
                            cp.core.db.csSet(CS, "TimeDuration", HourDuration);
                            cp.core.db.csSet(CS, "NewVisitorVisits", NewVisitorVisits);
                            cp.core.db.csSet(CS, "SinglePageVisits", SinglePageVisits);
                            cp.core.db.csSet(CS, "AuthenticatedVisits", AuthenticatedVisits);
                            cp.core.db.csSet(CS, "NoCookieVisits", NoCookieVisits);
                            cp.core.db.csSet(CS, "AveTimeOnSite", AveTimeOnSite);
                            if (true) {
                                cp.core.db.csSet(CS, "MobileVisits", MobileVisits);
                                cp.core.db.csSet(CS, "BotVisits", BotVisits);
                            }
                        }
                        cp.core.db.csClose(ref CS);
                        PeriodDatePtr = PeriodDatePtr.AddHours(HourDuration);
                    }
                }
                //
                return;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception");
        }
        //
        //======================================================================================
        //   Log a reported error
        //======================================================================================
        //
        public void AppendClassLog(coreClass cpcore, string ApplicationName, string MethodName, string LogCopy) {
            logController.appendLogWithLegacyRow(cpcore, ApplicationName, LogCopy, "ccHouseKeep", "HouseKeepClass", MethodName, 0, "", "", false, true, "", "HouseKeep", "");
        }
        //
        //
        //
        private void HousekeepLogFolder(coreClass cpCore, string appName, string FolderName) {
            try {
                //
                DateTime LogDate = default(DateTime);
                //Dim fs As New fileSystemClass
                FileInfo[] FileList = null;
                //
                LogDate = DateTime.Now.AddDays(-30);
                AppendClassLog(cpCore, "", "HouseKeep", "Deleting Logs [" + FolderName + "] older than 30 days");
                FileList = cp.core.programDataFiles.getFileList(FolderName);
                foreach (FileInfo file in FileList) {
                    if (file.CreationTime < LogDate) {
                        cp.core.privateFiles.deleteFile(FolderName + "\\" + file.Name);
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
        //
        //Private Sub HandleClassTrapError(ByVal ApplicationName As String, ByVal MethodName As String, ByVal Cause As String, ByVal ResumeNext As Boolean)
        //    '
        //    //throw new ApplicationException("Unexpected exception")
        //    '
        //End Sub
        //
        // ----- temp solution to convert error reporting without spending the time right now
        //
        //Private Sub HandleClassInternalError(ByVal ApplicationName As String, ByVal MethodName As String, ByVal ErrNumber As Integer, ByVal Cause As String)
        //    '
        //    //throw new ApplicationException("Unexpected exception")
        //    '
        //End Sub
        //
        //
        //
        private void HouseKeep_App_Daily_LogFolder(string FolderName, DateTime LastMonth) {
            try {
                //
                FileInfo[] FileList = null;
                string[] FileArray = null;
                int FileArrayCount = 0;
                int FileArrayPointer = 0;
                string[] FileSplit = null;
                //
                AppendClassLog(cp.core, cp.core.serverConfig.appConfig.name, "HouseKeep_App_Daily_LogFolder(" + cp.core.serverConfig.appConfig.name + ")", "Deleting files from folder [" + FolderName + "] older than " + LastMonth);
                FileList = cp.core.privateFiles.getFileList(FolderName);
                foreach (FileInfo file in FileList) {
                    if (file.CreationTime < LastMonth) {
                        cp.core.privateFiles.deleteFile(FolderName + "/" + file.Name);
                    }
                }
                return;
                //
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception");
        }
        //
        //
        //
        private bool DownloadUpdates() {
            bool tempDownloadUpdates = false;
            bool loadOK = true;
            try {
                XmlDocument Doc = null;
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode CDefSection = null;
                string URL = null;
                string Copy = null;
                //
                Doc = new XmlDocument();
                URL = "http://support.contensive.com/GetUpdates?iv=" + cp.Version;
                loadOK = true;
                Doc.Load(URL);
                if (Doc.DocumentElement.Name.ToLower() != genericController.vbLCase("ContensiveUpdate")) {
                    tempDownloadUpdates = false;
                } else {
                    if (Doc.DocumentElement.ChildNodes.Count == 0) {
                        tempDownloadUpdates = false;
                    } else {
                        foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                            Copy = CDefSection.InnerText;
                            switch (genericController.vbLCase(CDefSection.Name)) {
                                case "mastervisitnamelist":
                                    //
                                    // Read in the interfaces and save to Add-ons
                                    //
                                    cp.core.privateFiles.saveFile("config\\VisitNameList.txt", Copy);
                                    //Call cp.Core.app.privateFiles.SaveFile(getAppPath & "\config\DefaultBotNameList.txt", copy)
                                    break;
                                case "masteremailbouncefilters":
                                    //
                                    // save the updated filters file
                                    //
                                    cp.core.privateFiles.saveFile("config\\EmailBounceFilters.txt", Copy);
                                    //Call cp.Core.app.privateFiles.SaveFile(getAppPath & "\cclib\config\Filters.txt", copy)
                                    break;
                                case "mastermobilebrowserlist":
                                    //
                                    // save the updated filters file
                                    //
                                    cp.core.privateFiles.saveFile("config\\MobileBrowserList.txt", Copy);
                                    break;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                //
                // error - Need a way to reach the user that submitted the file
                //
                tempDownloadUpdates = false;
                //throw new ApplicationException("Unexpected exception");
            }
            return loadOK;
        }
        //
        //=========================================================================================
        // Summarize the page views
        //   excludes non-cookie visits
        //   excludes administrator and developer visits
        //   excludes authenticated users with ExcludeFromReporting
        //
        //=========================================================================================
        //
        public void HouseKeep_PageViewSummary(DateTime StartTimeDate, DateTime EndTimeDate, int HourDuration, string BuildVersion, DateTime OldestVisitSummaryWeCareAbout) {
            try {
                //
                //
                string baseCriteria = null;
                DateTime StartDate = default(DateTime);
                //Dim StartTime As Date
                DateTime PeriodStart = default(DateTime);
                //Dim TotalTimeOnSite
                int MultiPageVisitCnt = 0;
                int MultiPageHitCnt = 0;
                double MultiPageTimetoLastHitSum = 0;
                double TimeOnSite = 0;
                double PeriodStep = 0;
                DateTime PeriodDatePtr = default(DateTime);
                DateTime StartOfHour = default(DateTime);
                int DateNumber = 0;
                int TimeNumber = 0;
                double SumStartTime = 0;
                double SumStopTime = 0;
                int HoursPerDay = 0;
                DateTime DateStart = default(DateTime);
                DateTime DateEnd = default(DateTime);
                int NewVisitorVisits = 0;
                int SinglePageVisits = 0;
                int AuthenticatedVisits = 0;
                int NoCookieVisits = 0;
                double AveTimeOnSite = 0;
                int HitCnt = 0;
                int VisitCnt = 0;
                DateTime OldestDateAdded = default(DateTime);
                object EmptyVariant = null;
                bool NeedToClearCache = false;
                int ArchiveParentID = 0;
                int RecordID = 0;
                int CS = 0;
                //Dim AddonInstall As New addonInstallClass
                int LoopPtr = 0;
                int Ptr = 0;
                string LocalFile = null;
                string LocalFilename = null;
                string[] Folders = null;
                int FolderCnt = 0;
                string CollectionGUID = null;
                string CollectionName = null;
                int Pos = 0;
                string LastChangeDate = null;
                string SubFolderList = null;
                string[] SubFolders = null;
                string SubFolder = null;
                int Cnt = 0;
                string LocalGUID = null;
                string LocalLastChangeDateStr = null;
                DateTime LocalLastChangeDate = default(DateTime);
                string LibGUID = null;
                string LibLastChangeDateStr = null;
                DateTime LibLastChangeDate = default(DateTime);
                XmlNode LibListNode = null;
                XmlNode LocalListNode = null;
                XmlNode CollectionNode = null;
                XmlDocument LibraryCollections = new XmlDocument();
                XmlDocument LocalCollections = new XmlDocument();
                XmlDocument Doc = new XmlDocument();
                //Dim AppService As appServicesClass
                //Dim KernelService As KernelServicesClass
                string SetTimeCheckString = null;
                double SetTimeCheck = 0;
                DateTime LogDate = default(DateTime);
                string FolderName = null;
                string FileList = null;
                string[] FileArray = null;
                int FileArrayCount = 0;
                int FileArrayPointer = 0;
                string[] FileSplit = null;
                string FolderList = null;
                string[] FolderArray = null;
                int FolderArrayCount = 0;
                int FolderArrayPointer = 0;
                string[] FolderSplit = null;
                //Dim fs As New fileSystemClass
                int VisitArchiveAgeDays = 0;
                bool NewDay = false;
                bool NewHour = false;
                int CSPages = 0;
                int PageID = 0;
                string PageTitle = null;
                int NoCookiePageViews = 0;
                int PageViews = 0;
                int AuthenticatedPageViews = 0;
                int MobilePageViews = 0;
                int BotPageViews = 0;
                //
                DateTime LastTimeCheck = default(DateTime);
                //
                string ConfigFilename = null;
                string Config = null;
                string[] ConfigLines = null;
                //
                string Line = null;
                int LineCnt = 0;
                int LinePtr = 0;
                string[] NameValue = null;
                string SQLNow = null;
                string SQL = null;
                int AveReadTime = 0;

                //
                if (string.CompareOrdinal(BuildVersion, cp.Version) < 0) {
                    cp.core.handleException(new ApplicationException("Can not summarize analytics until this site's data needs been upgraded."));
                } else {
                    PeriodStart = StartTimeDate;
                    if (PeriodStart < OldestVisitSummaryWeCareAbout) {
                        PeriodStart = OldestVisitSummaryWeCareAbout;
                    }
                    StartDate = PeriodStart.Date;
                    PeriodStep = (double)HourDuration / 24.0F;
                    while (PeriodDatePtr < EndTimeDate) {
                        //
                        DateNumber = EncodeInteger(PeriodDatePtr.AddHours(HourDuration / 2.0).ToOADate());
                        TimeNumber = EncodeInteger(PeriodDatePtr.TimeOfDay.TotalHours);
                        DateStart = PeriodDatePtr.Date;
                        DateEnd = PeriodDatePtr.AddHours(HourDuration).Date;
                        //
                        VisitCnt = 0;
                        HitCnt = 0;
                        SumStartTime = 0;
                        SumStopTime = 0;
                        NewVisitorVisits = 0;
                        SinglePageVisits = 0;
                        MultiPageVisitCnt = 0;
                        MultiPageTimetoLastHitSum = 0;
                        AuthenticatedVisits = 0;
                        NoCookieVisits = 0;
                        AveTimeOnSite = 0;
                        PageTitle = "";
                        PageID = 0;
                        PageViews = 0;
                        AuthenticatedPageViews = 0;
                        MobilePageViews = 0;
                        BotPageViews = 0;
                        NoCookiePageViews = 0;
                        //
                        // Loop through all the pages hit during this period
                        //
                        //
                        // for now ignore the problem caused by addons like Blogs creating multiple 'pages' within on pageid
                        // One way to handle this is to expect the addon to set something unquie in he page title
                        // then use the title to distinguish a page. The problem with this is the current system puts the
                        // visit number and page number in the name. if we select on district name, they will all be.
                        //
                        SQL = "select distinct recordid,pagetitle from ccviewings h"
                            + " where (h.recordid<>0)"
                            + " and(h.dateadded>=" + cp.core.db.encodeSQLDate(DateStart) + ")"
                            + " and (h.dateadded<" + cp.core.db.encodeSQLDate(DateEnd) + ")"
                            + " and((h.ExcludeFromAnalytics is null)or(h.ExcludeFromAnalytics=0))"
                            + "order by recordid";
                        CSPages = cp.core.db.csOpenSql_rev("default", SQL);
                        if (!cp.core.db.csOk(CSPages)) {
                            //
                            // no hits found - add or update a single record for this day so we know it has been calculated
                            //
                            CS = cp.core.db.csOpen("Page View Summary", "(timeduration=" + HourDuration + ")and(DateNumber=" + DateNumber + ")and(TimeNumber=" + TimeNumber + ")and(pageid=" + PageID + ")and(pagetitle=" + cp.core.db.encodeSQLText(PageTitle) + ")");
                            if (!cp.core.db.csOk(CS)) {
                                cp.core.db.csClose(ref CS);
                                CS = cp.core.db.csInsertRecord("Page View Summary");
                            }
                            //
                            if (cp.core.db.csOk(CS)) {
                                cp.core.db.csSet(CS, "name", HourDuration + " hr summary for " + DateTime.FromOADate((double)DateNumber) + " " + TimeNumber + ":00, " + PageTitle);
                                cp.core.db.csSet(CS, "DateNumber", DateNumber);
                                cp.core.db.csSet(CS, "TimeNumber", TimeNumber);
                                cp.core.db.csSet(CS, "TimeDuration", HourDuration);
                                cp.core.db.csSet(CS, "PageViews", PageViews);
                                cp.core.db.csSet(CS, "PageID", PageID);
                                cp.core.db.csSet(CS, "PageTitle", PageTitle);
                                cp.core.db.csSet(CS, "AuthenticatedPageViews", AuthenticatedPageViews);
                                cp.core.db.csSet(CS, "NoCookiePageViews", NoCookiePageViews);
                                if (true) {
                                    cp.core.db.csSet(CS, "MobilePageViews", MobilePageViews);
                                    cp.core.db.csSet(CS, "BotPageViews", BotPageViews);
                                }
                            }
                            cp.core.db.csClose(ref CS);
                        } else {
                            //
                            // add an entry for each page hit on this day
                            //
                            while (cp.core.db.csOk(CSPages)) {
                                PageID = cp.core.db.csGetInteger(CSPages, "recordid");
                                PageTitle = cp.core.db.csGetText(CSPages, "pagetitle");
                                baseCriteria = ""
                                    + " (h.recordid=" + PageID + ")"
                                    + " "
                                    + " and(h.dateadded>=" + cp.core.db.encodeSQLDate(DateStart) + ")"
                                    + " and(h.dateadded<" + cp.core.db.encodeSQLDate(DateEnd) + ")"
                                    + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                                    + " and((h.ExcludeFromAnalytics is null)or(h.ExcludeFromAnalytics=0))"
                                    + "";
                                if (!string.IsNullOrEmpty(PageTitle)) {
                                    baseCriteria = baseCriteria + "and(h.pagetitle=" + cp.core.db.encodeSQLText(PageTitle) + ")";
                                }
                                //
                                // Total Page Views
                                //
                                SQL = "select count(h.id) as cnt"
                                    + " from ccviewings h left join ccvisits v on h.visitid=v.id"
                                    + " where " + baseCriteria + " and (v.CookieSupport<>0)"
                                    + "";
                                CS = cp.core.db.csOpenSql_rev("default", SQL);
                                if (cp.core.db.csOk(CS)) {
                                    PageViews = cp.core.db.csGetInteger(CS, "cnt");
                                }
                                cp.core.db.csClose(ref CS);
                                //
                                // Authenticated Visits
                                //
                                SQL = "select count(h.id) as cnt"
                                    + " from ccviewings h left join ccvisits v on h.visitid=v.id"
                                    + " where " + baseCriteria + " and(v.CookieSupport<>0)"
                                    + " and(v.visitAuthenticated<>0)"
                                    + "";
                                CS = cp.core.db.csOpenSql_rev("default", SQL);
                                if (cp.core.db.csOk(CS)) {
                                    AuthenticatedPageViews = cp.core.db.csGetInteger(CS, "cnt");
                                }
                                cp.core.db.csClose(ref CS);
                                //
                                // No Cookie Page Views
                                //
                                SQL = "select count(h.id) as NoCookiePageViews"
                                    + " from ccviewings h left join ccvisits v on h.visitid=v.id"
                                    + " where " + baseCriteria + " and((v.CookieSupport=0)or(v.CookieSupport is null))"
                                    + "";
                                CS = cp.core.db.csOpenSql_rev("default", SQL);
                                if (cp.core.db.csOk(CS)) {
                                    NoCookiePageViews = cp.core.db.csGetInteger(CS, "NoCookiePageViews");
                                }
                                cp.core.db.csClose(ref CS);
                                //
                                if (true) {
                                    //
                                    // Mobile Visits
                                    //
                                    SQL = "select count(h.id) as cnt"
                                        + " from ccviewings h left join ccvisits v on h.visitid=v.id"
                                        + " where " + baseCriteria + " and(v.CookieSupport<>0)"
                                        + " and(v.mobile<>0)"
                                        + "";
                                    CS = cp.core.db.csOpenSql_rev("default", SQL);
                                    if (cp.core.db.csOk(CS)) {
                                        MobilePageViews = cp.core.db.csGetInteger(CS, "cnt");
                                    }
                                    cp.core.db.csClose(ref CS);
                                    //
                                    // Bot Visits
                                    //
                                    SQL = "select count(h.id) as cnt"
                                        + " from ccviewings h left join ccvisits v on h.visitid=v.id"
                                        + " where " + baseCriteria + " and(v.CookieSupport<>0)"
                                        + " and(v.bot<>0)"
                                        + "";
                                    CS = cp.core.db.csOpenSql_rev("default", SQL);
                                    if (cp.core.db.csOk(CS)) {
                                        BotPageViews = cp.core.db.csGetInteger(CS, "cnt");
                                    }
                                    cp.core.db.csClose(ref CS);
                                }
                                //
                                // Add or update the Visit Summary Record
                                //
                                CS = cp.core.db.csOpen("Page View Summary", "(timeduration=" + HourDuration + ")and(DateNumber=" + DateNumber + ")and(TimeNumber=" + TimeNumber + ")and(pageid=" + PageID + ")and(pagetitle=" + cp.core.db.encodeSQLText(PageTitle) + ")");
                                if (!cp.core.db.csOk(CS)) {
                                    cp.core.db.csClose(ref CS);
                                    CS = cp.core.db.csInsertRecord("Page View Summary");
                                }
                                //
                                if (cp.core.db.csOk(CS)) {
                                    string PageName = null;

                                    if (string.IsNullOrEmpty(PageTitle)) {
                                        PageName = cp.core.db.getRecordName("page content", PageID);
                                        cp.core.db.csSet(CS, "name", HourDuration + " hr summary for " + DateTime.FromOADate((double)DateNumber) + " " + TimeNumber + ":00, " + PageName);
                                        cp.core.db.csSet(CS, "PageTitle", PageName);
                                    } else {
                                        cp.core.db.csSet(CS, "name", HourDuration + " hr summary for " + DateTime.FromOADate((double)DateNumber) + " " + TimeNumber + ":00, " + PageTitle);
                                        cp.core.db.csSet(CS, "PageTitle", PageTitle);
                                    }
                                    cp.core.db.csSet(CS, "DateNumber", DateNumber);
                                    cp.core.db.csSet(CS, "TimeNumber", TimeNumber);
                                    cp.core.db.csSet(CS, "TimeDuration", HourDuration);
                                    cp.core.db.csSet(CS, "PageViews", PageViews);
                                    cp.core.db.csSet(CS, "PageID", PageID);
                                    cp.core.db.csSet(CS, "AuthenticatedPageViews", AuthenticatedPageViews);
                                    cp.core.db.csSet(CS, "NoCookiePageViews", NoCookiePageViews);
                                    if (true) {
                                        cp.core.db.csSet(CS, "MobilePageViews", MobilePageViews);
                                        cp.core.db.csSet(CS, "BotPageViews", BotPageViews);
                                    }
                                }
                                cp.core.db.csClose(ref CS);
                                cp.core.db.csGoNext(CSPages);
                            }
                        }
                        cp.core.db.csClose(ref CSPages);
                        PeriodDatePtr = PeriodDatePtr.AddHours(HourDuration);
                    }
                }
                //
                return;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception");
        }
        //
        //====================================================================================================
        public void housekeepAddonFolder(coreClass cpcore) {
            try {
                string RegisterPathList = null;
                string RegisterPath = null;
                string[] RegisterPaths = null;
                string Path = null;
                int NodeCnt = 0;
                bool IsActiveFolder = false;
                string Cmd = null;
                string CollectionRootPath = null;
                int Pos = 0;
                DirectoryInfo[] FolderList = null;
                string LocalPath = null;
                string LocalGuid = null;
                XmlDocument Doc = new XmlDocument();
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode CollectionNode = null;
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode LocalListNode = null;
                int FolderPtr = 0;
                string CollectionPath = null;
                DateTime LastChangeDate = default(DateTime);
                string hint = "";
                string LocalName = null;
                int Ptr = 0;
                string collectionFileFilename = null;
                //
                AppendClassLog(cpcore, "Server", "RegisterAddonFolder", "Entering RegisterAddonFolder");
                //
                bool loadOK = true;
                try {
                    collectionFileFilename = cp.core.addon.getPrivateFilesAddonPath() + "Collections.xml";
                    Doc.LoadXml(collectionFileFilename);
                } catch (Exception ex) {
                    AppendClassLog(cpcore, "Server", "", "RegisterAddonFolder, Hint=[" + hint + "], Error loading Collections.xml file.");
                    loadOK = false;
                }
                if (loadOK) {
                    //
                    AppendClassLog(cpcore, "Server", "RegisterAddonFolder", "Collection.xml loaded ok");
                    //
                    if (genericController.vbLCase(Doc.DocumentElement.Name) != genericController.vbLCase(CollectionListRootNode)) {
                        AppendClassLog(cpcore, "Server", "", "RegisterAddonFolder, Hint=[" + hint + "], The Collections.xml file has an invalid root node, [" + Doc.DocumentElement.Name + "] was received and [" + CollectionListRootNode + "] was expected.");
                    } else {
                        //
                        AppendClassLog(cpcore, "Server", "RegisterAddonFolder", "Collection.xml root name ok");
                        //
                        if (true) {
                            //If genericController.vbLCase(.name) <> "collectionlist" Then
                            //    Call AppendClassLog(cpcore,"Server", "", "RegisterAddonFolder, basename was not collectionlist, [" & .name & "].")
                            //Else
                            NodeCnt = 0;
                            RegisterPathList = "";
                            foreach (XmlNode LocalListNode in Doc.DocumentElement.ChildNodes) {
                                //
                                // Get the collection path
                                //
                                CollectionPath = "";
                                LocalGuid = "";
                                LocalName = "no name found";
                                LocalPath = "";
                                switch (genericController.vbLCase(LocalListNode.Name)) {
                                    case "collection":
                                        LocalGuid = "";
                                        foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                            switch (genericController.vbLCase(CollectionNode.Name)) {
                                                case "name":
                                                    //
                                                    LocalName = genericController.vbLCase(CollectionNode.InnerText);
                                                    break;
                                                case "guid":
                                                    //
                                                    LocalGuid = genericController.vbLCase(CollectionNode.InnerText);
                                                    break;
                                                case "path":
                                                    //
                                                    CollectionPath = genericController.vbLCase(CollectionNode.InnerText);
                                                    break;
                                                case "lastchangedate":
                                                    LastChangeDate = genericController.EncodeDate(CollectionNode.InnerText);
                                                    break;
                                            }
                                        }
                                        break;
                                }
                                //
                                AppendClassLog(cpcore, "Server", "RegisterAddonFolder", "Node[" + NodeCnt + "], LocalName=[" + LocalName + "], LastChangeDate=[" + LastChangeDate + "], CollectionPath=[" + CollectionPath + "], LocalGuid=[" + LocalGuid + "]");
                                //
                                // Go through all subpaths of the collection path, register the version match, unregister all others
                                //
                                //fs = New fileSystemClass
                                if (string.IsNullOrEmpty(CollectionPath)) {
                                    //
                                    AppendClassLog(cpcore, "Server", "RegisterAddonFolder", "no collection path, skipping");
                                    //
                                } else {
                                    CollectionPath = genericController.vbLCase(CollectionPath);
                                    CollectionRootPath = CollectionPath;
                                    Pos = CollectionRootPath.LastIndexOf("\\") + 1;
                                    if (Pos <= 0) {
                                        //
                                        AppendClassLog(cpcore, "Server", "RegisterAddonFolder", "CollectionPath has no '\\', skipping");
                                        //
                                    } else {
                                        CollectionRootPath = CollectionRootPath.Left( Pos - 1);
                                        Path = cp.core.addon.getPrivateFilesAddonPath() + "\\" + CollectionRootPath + "\\";
                                        FolderList = new DirectoryInfo[0];
                                        if (cp.core.privateFiles.pathExists(Path)) {
                                            FolderList = cp.core.privateFiles.getFolderList(Path);
                                            //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                                            if (0 != 0) {
                                                //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                                                //Microsoft.VisualBasic.Information.Err().Clear();
                                            }
                                        }
                                        if (FolderList.Length == 0) {
                                            //
                                            AppendClassLog(cpcore, "Server", "RegisterAddonFolder", "no subfolders found in physical path [" + Path + "], skipping");
                                            //
                                        } else {
                                            foreach (DirectoryInfo dir in FolderList) {
                                                IsActiveFolder = false;
                                                //
                                                // register or unregister all files in this folder
                                                //
                                                if (string.IsNullOrEmpty(dir.Name)) {
                                                    //
                                                    AppendClassLog(cpcore, "Server", "RegisterAddonFolder", "....empty folder [" + dir.Name + "], skipping");
                                                    //
                                                } else {
                                                    //
                                                    AppendClassLog(cpcore, "Server", "RegisterAddonFolder", "....Folder [" + dir.Name + "]");
                                                    IsActiveFolder = (CollectionRootPath + "\\" + dir.Name == CollectionPath);
                                                    if (IsActiveFolder && (FolderPtr != (FolderList.Length - 1))) {
                                                        //
                                                        // This one is active, but not the last
                                                        //
                                                        AppendClassLog(cpcore, "Server", "RegisterAddonFolder", "....Active addon is not the most current, this folder is the active folder, but there are more recent folders. This folder will be preserved.");
                                                    }
                                                    // 20161005 - no longer need to register activeX
                                                    //FileList = cp.core.app.privateFiles.GetFolderFiles(Path & "\" & dir.Name)
                                                    //For Each file As FileInfo In FileList
                                                    //    If Right(file.Name, 4) = ".dll" Then
                                                    //        If IsActiveFolder Then
                                                    //            '
                                                    //            ' register this file
                                                    //            '
                                                    //            RegisterPathList = RegisterPathList & vbCrLf & Path & dir.Name & "\" & file.Name
                                                    //            '                                                                Cmd = "%comspec% /c regsvr32 """ & RegisterPathList & """ /s"
                                                    //            '                                                                Call AppendClassLog(cpcore,"Server", "RegisterAddonFolder", "....Regsiter DLL [" & Cmd & "]")
                                                    //            '                                                                Call runProcess(cp.core,Cmd, , True)
                                                    //        Else
                                                    //            '
                                                    //            ' unregister this file
                                                    //            '
                                                    //            Cmd = "%comspec% /c regsvr32 /u """ & Path & dir.Name & "\" & file.Name & """ /s"
                                                    //            Call AppendClassLog(cpcore,"Server", "RegisterAddonFolder", "....Unregsiter DLL [" & Cmd & "]")
                                                    //            Call runProcess(cp.core, Cmd, , True)
                                                    //        End If
                                                    //    End If
                                                    //Next
                                                    //
                                                    // only keep last two non-matching folders and the active folder
                                                    //
                                                    if (IsActiveFolder) {
                                                        //IsActiveFolder = IsActiveFolder;
                                                    } else {
                                                        if (FolderPtr < (FolderList.Length - 3)) {
                                                            AppendClassLog(cpcore, "Server", "RegisterAddonFolder", "....Deleting path because non-active and not one of the newest 2 [" + Path + dir.Name + "]");
                                                            cp.core.privateFiles.DeleteFileFolder(Path + dir.Name);
                                                        }
                                                    }
                                                }
                                            }
                                            //
                                            // register files found in the active folder last
                                            //
                                            if (!string.IsNullOrEmpty(RegisterPathList)) {
                                                RegisterPaths = genericController.customSplit(RegisterPathList, "\r\n");
                                                for (Ptr = 0; Ptr <= RegisterPaths.GetUpperBound(0); Ptr++) {
                                                    RegisterPath = RegisterPaths[Ptr].Trim(' ');
                                                    if (!string.IsNullOrEmpty(RegisterPath)) {
                                                        Cmd = "%comspec% /c regsvr32 \"" + RegisterPath + "\" /s";
                                                        AppendClassLog(cpcore, "Server", "RegisterAddonFolder", "....Register DLL [" + Cmd + "]");
                                                        runProcess(cp.core, Cmd,"", true);
                                                    }
                                                }
                                                RegisterPathList = "";
                                            }
                                        }
                                    }
                                }
                                NodeCnt = NodeCnt + 1;
                            }
                            // 20161005 - no longer need to register activeX
                            //
                            // register files found in the active folder last
                            //
                            //If RegisterPathList <> "" Then
                            //    RegisterPaths = Split(RegisterPathList, vbCrLf)
                            //    For Ptr = 0 To UBound(RegisterPaths)
                            //        RegisterPath = Trim(RegisterPaths[Ptr])
                            //        If RegisterPath <> "" Then
                            //            Cmd = "%comspec% /c regsvr32 """ & RegisterPath & """ /s"
                            //            Call AppendClassLog(cpcore,"Server", "RegisterAddonFolder", "....Register DLL [" & Cmd & "]")
                            //            Call runProcess(cp.core, Cmd, , True)
                            //        End If
                            //    Next
                            //End If
                        }
                    }
                }
                //
                AppendClassLog(cpcore, "Server", "RegisterAddonFolder", "Exiting RegisterAddonFolder");
            } catch (Exception ex) {
                throw new ApplicationException("Unexpected Exception", ex);
            }
        }
    }
}
