
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.BaseClasses;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
using System.IO;
using static Contensive.BaseClasses.CPFileSystemBaseClass;
using Contensive.Processor.Exceptions;
using Contensive.Processor.Models.Domain;
//
namespace Contensive.Addons.Housekeeping {
    /// <summary>
    /// support for housekeeping functions
    /// </summary>
    public class HouseKeepClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// addon interface
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(CPBaseClass cp) {
            string result = "";
            try {
                //
                // -- ok to cast cpbase to cp because they build from the same solution
                //this.cp = (CPClass)cp;
                CoreController core = ((CPClass)cp).core;
                houseKeep(core, core.docProperties.getBoolean("force"));
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public void houseKeep(CoreController core, bool force) {
            try {
                DateTime LastCheckDateTime = core.siteProperties.getDate("housekeep, last check", default(DateTime));
                int ServerHousekeepHour = core.siteProperties.getInteger("housekeep, run time hour", 2);
                var installedCollections = new List<string>();
                //
                // ----- Run Server Housekeep
                //
                DateTime rightNow = DateTime.Now;
                bool RunServerHousekeep = ((rightNow.Date > LastCheckDateTime.Date) && (ServerHousekeepHour < rightNow.Hour));
                if (force || RunServerHousekeep) {
                    core.siteProperties.setProperty("housekeep, last check", rightNow);
                    //CPClass cp = new CPClass();
                    DateTime Yesterday = rightNow.AddDays(-1).Date;
                    DateTime ALittleWhileAgo = rightNow.AddDays(-90).Date;
                    string SQLNow = DbController.encodeSQLDate(rightNow);
                    //
                    // it is the next day, remove old log files
                    //
                    LogController.housekeepLogs(core);
                    //
                    // Download Updates
                    DownloadUpdates(core);
                    //
                    // Register and unregister files in the Addon folder
                    housekeepAddonFolder(core);
                    //
                    // Upgrade Local Collections, and all applications that use them
                    string ErrorMessage = "";
                    logHousekeeping(core, "Updating local collections from library, see Upgrade log for details during this period.");
                    List<string> nonCriticalErrorList = new List<string>();
                    string logPrefix = "Housekeep";
                    if (!CollectionController.upgradeLocalCollectionRepoFromRemoteCollectionRepo(core, ref ErrorMessage, false, false, ref nonCriticalErrorList, logPrefix, ref installedCollections)) {
                        if (string.IsNullOrEmpty(ErrorMessage)) {
                            ErrorMessage = "No detailed error message was returned from UpgradeAllLocalCollectionsFromLib2 although it returned 'not ok' status.";
                        }
                        logHousekeeping(core, "Updating local collections from Library returned an error, " + ErrorMessage);
                    }
                    //
                    // 20180816, no, core is v.41
                    // Verify core installation
                    //CollectionController.installCollectionFromRemoteRepo(core, CoreCollectionGuid, ref ErrorMessage, "", false, false, ref nonCriticalErrorList);
                    //
                    string DomainNamePrimary = core.appConfig.domainList[0];
                    int Pos = GenericController.vbInstr(1, DomainNamePrimary, ",");
                    if (Pos > 1) {
                        DomainNamePrimary = DomainNamePrimary.Left(Pos - 1);
                    }
                    int DataSourceType = core.db.getDataSourceType("default");
                    //
                    string DefaultMemberName = "";
                    int PeopleCID = MetaModel.getContentId(core, "people");
                    string SQL = "select defaultvalue from ccfields where name='name' and contentid=(" + PeopleCID + ")";
                    using (var csXfer = new CsModel(core)) {
                        csXfer.csOpenSql(SQL, "Default");
                        if (csXfer.csOk()) {
                            DefaultMemberName = csXfer.csGetText("defaultvalue");
                        }
                        csXfer.close();
                    }
                    //
                    // Get ArchiveAgeDays - use this as the oldest data they care about
                    //
                    int VisitArchiveAgeDays = GenericController.encodeInteger(core.siteProperties.getText("ArchiveRecordAgeDays", "365"));
                    if (VisitArchiveAgeDays < 2) {
                        VisitArchiveAgeDays = 2;
                        core.siteProperties.setProperty("ArchiveRecordAgeDays", "2");
                    }
                    DateTime VisitArchiveDate = rightNow.AddDays(-VisitArchiveAgeDays).Date;
                    DateTime OldestVisitSummaryWeCareAbout = DateTime.Now.Date.AddDays(-120);
                    if (OldestVisitSummaryWeCareAbout < VisitArchiveDate) {
                        OldestVisitSummaryWeCareAbout = VisitArchiveDate;
                    }
                    //OldestVisitSummaryWeCareAbout = now.date - VisitArchiveAgeDays
                    //
                    // Get GuestArchiveAgeDays
                    //
                    int GuestArchiveAgeDays = GenericController.encodeInteger(core.siteProperties.getText("ArchivePeopleAgeDays", "2"));
                    if (GuestArchiveAgeDays < 2) {
                        GuestArchiveAgeDays = 2;
                        core.siteProperties.setProperty("ArchivePeopleAgeDays", GuestArchiveAgeDays.ToString());
                    }
                    //
                    // Get EmailDropArchiveAgeDays
                    //
                    int EmailDropArchiveAgeDays = GenericController.encodeInteger(core.siteProperties.getText("ArchiveEmailDropAgeDays", "90"));
                    if (EmailDropArchiveAgeDays < 2) {
                        EmailDropArchiveAgeDays = 2;
                        core.siteProperties.setProperty("ArchiveEmailDropAgeDays", EmailDropArchiveAgeDays.ToString());
                    }
                    //
                    // Do non-optional housekeeping
                    //
                    if (RunServerHousekeep || force) {
                        {
                            //
                            // Move Archived pages from their current parent to their archive parent
                            //
                            bool NeedToClearCache = false;
                            logHousekeeping(core, "Archive update for pages on [" + core.appConfig.name + "]");
                            SQL = "select * from ccpagecontent where (( DateArchive is not null )and(DateArchive<" + SQLNow + "))and(active<>0)";
                            using (var csXfer = new CsModel(core)) {
                                csXfer.csOpenSql(SQL, "Default");
                                while (csXfer.csOk()) {
                                    int RecordID = csXfer.csGetInteger("ID");
                                    int ArchiveParentID = csXfer.csGetInteger("ArchiveParentID");
                                    if (ArchiveParentID == 0) {
                                        SQL = "update ccpagecontent set DateArchive=null where (id=" + RecordID + ")";
                                        core.db.executeQuery(SQL);
                                    } else {
                                        SQL = "update ccpagecontent set ArchiveParentID=null,DateArchive=null,parentid=" + ArchiveParentID + " where (id=" + RecordID + ")";
                                        core.db.executeQuery(SQL);
                                        NeedToClearCache = true;
                                    }
                                    csXfer.csGoNext();
                                }
                                csXfer.close();
                            }
                            //
                            // Clear caches
                            //
                            if (NeedToClearCache) {
                                object emptyData = null;
                                core.cache.invalidate("Page Content");
                                core.cache.storeObject("PCC", emptyData);
                            }
                        }
                        {
                            //
                            // Delete any daily visit summary duplicates during this period(keep the first)
                            //
                            SQL = "delete from ccvisitsummary"
                                + " where id in ("
                                + " select d.id from ccvisitsummary d,ccvisitsummary f"
                                + " where f.datenumber=d.datenumber"
                                + " and f.datenumber>" + OldestVisitSummaryWeCareAbout.ToOADate() + " and f.datenumber<" + Yesterday.ToOADate() + " and f.TimeDuration=24"
                                + " and d.TimeDuration=24"
                                + " and f.id<d.id"
                                + ")";
                            core.db.executeQuery(SQL);
                            //
                            // Find missing daily summaries, summarize that date
                            //
                            SQL = core.db.getSQLSelect("default", "ccVisitSummary", "DateNumber", "TimeDuration=24 and DateNumber>=" + OldestVisitSummaryWeCareAbout.Date.ToOADate(), "DateNumber,TimeNumber");
                            using (var csXfer = new CsModel(core)) {
                                csXfer.csOpenSql(SQL, "Default");
                                DateTime datePtr = OldestVisitSummaryWeCareAbout;
                                while (datePtr <= Yesterday) {
                                    if (!csXfer.csOk()) {
                                        //
                                        // Out of data, start with this DatePtr
                                        //
                                        houseKeep_VisitSummary(core, datePtr, datePtr, 24, core.siteProperties.dataBuildVersion, OldestVisitSummaryWeCareAbout);
                                        //Exit For
                                    } else {
                                        DateTime workingDate = DateTime.MinValue.AddDays(csXfer.csGetInteger("DateNumber"));
                                        if (datePtr < workingDate) {
                                            //
                                            // There are missing dates, update them
                                            //
                                            houseKeep_VisitSummary(core, datePtr, workingDate.AddDays(-1), 24, core.siteProperties.dataBuildVersion, OldestVisitSummaryWeCareAbout);
                                        }
                                    }
                                    if (csXfer.csOk()) {
                                        //
                                        // if there is more data, go to the next record
                                        //
                                        csXfer.csGoNext();
                                    }
                                    datePtr = datePtr.AddDays(1).Date;
                                }
                                csXfer.close();
                            }
                        }
                        //
                        // Remote Query Expiration
                        //
                        SQL = "delete from ccRemoteQueries where (DateExpires is not null)and(DateExpires<" + DbController.encodeSQLDate(DateTime.Now) + ")";
                        core.db.executeQuery(SQL);
                        if (DataSourceType == DataSourceTypeODBCMySQL) {
                            SQL = "delete m from ccmenuEntries m left join ccAggregateFunctions a on a.id=m.AddonID where m.addonid<>0 and a.id is null";
                        } else {
                            SQL = "delete from ccmenuEntries where id in (select m.ID from ccMenuEntries m left join ccAggregateFunctions a on a.id=m.AddonID where m.addonid<>0 and a.id is null)";
                        }
                        core.db.executeQuery(SQL);
                        //
                        if (DataSourceType == DataSourceTypeODBCMySQL) {
                            SQL = "delete m from ccmenuEntries m left join ccAggregateFunctions a on a.id=m.helpaddonid where m.helpaddonid<>0 and a.id is null";
                        } else {
                            SQL = "delete from ccmenuEntries where id in (select m.ID from ccMenuEntries m left join ccAggregateFunctions a on a.id=m.helpaddonid where m.helpaddonid<>0 and a.id is null)";
                        }
                        core.db.executeQuery(SQL);
                        //
                        if (DataSourceType == DataSourceTypeODBCMySQL) {
                            SQL = "delete m from ccmenuEntries m left join ccAggregateFunctions a on a.id=m.helpcollectionid where m.helpcollectionid<>0 and a.id is null";
                        } else {
                            SQL = "delete from ccmenuEntries where id in (select m.ID from ccMenuEntries m left join ccAddonCollections c on c.id=m.helpcollectionid Where m.helpcollectionid <> 0 And c.Id Is Null)";
                        }
                        core.db.executeQuery(SQL);
                        //
                        // Page View Summary
                        //
                        {
                            DateTime datePtr = default(DateTime);
                            using (var csXfer = new CsModel(core)) {
                                if (!csXfer.csOpenSql(core.db.getSQLSelect("default", "ccviewingsummary", "DateNumber", "TimeDuration=24 and DateNumber>=" + OldestVisitSummaryWeCareAbout.Date.ToOADate(), "DateNumber Desc", "", 1))) {
                                    datePtr = OldestVisitSummaryWeCareAbout;
                                } else {
                                    datePtr = DateTime.MinValue.AddDays(csXfer.csGetInteger("DateNumber"));
                                }
                            }
                            if (datePtr < OldestVisitSummaryWeCareAbout) { datePtr = OldestVisitSummaryWeCareAbout; }
                            houseKeep_PageViewSummary(core, datePtr, Yesterday, 24, core.siteProperties.dataBuildVersion, OldestVisitSummaryWeCareAbout);
                        }
                        //
                        // -- Properties
                        housekeep_userProperties(core);
                        housekeep_visitProperties(core);
                        housekeep_visitorProperties(core);
                    }
                    //
                    // Each hour, summarize the visits and viewings into the Visit Summary table
                    //
                    bool NewHour = (rightNow.Hour != LastCheckDateTime.Hour);
                    if (force || NewHour) {
                        //
                        // Set NextSummaryStartDate based on the last time we ran hourly summarization
                        //
                        DateTime LastTimeSummaryWasRun = VisitArchiveDate;
                        using (var csXfer = new CsModel(core)) {
                            if (csXfer.csOpenSql(core.db.getSQLSelect("default", "ccVisitSummary", "DateAdded", "(timeduration=1)and(Dateadded>" + DbController.encodeSQLDate(VisitArchiveDate) + ")", "id Desc", "", 1))) {
                                LastTimeSummaryWasRun = csXfer.csGetDate("DateAdded");
                                logHousekeeping(core, "Update hourly visit summary, last time summary was run was [" + LastTimeSummaryWasRun + "]");
                            } else {
                                logHousekeeping(core, "Update hourly visit summary, no hourly summaries were found, set start to [" + LastTimeSummaryWasRun + "]");
                            }
                        }
                        DateTime NextSummaryStartDate = LastTimeSummaryWasRun;
                        //
                        // Each hourly entry includes visits that started during that hour, but we do not know when they finished (maybe during last hour)
                        //   Find the oldest starttime of all the visits with endtimes after the LastTimeSummaryWasRun. Resummarize all periods
                        //   from then to now
                        //
                        //   For the past 24 hours, find the oldest visit with the last viewing during the last hour
                        //
                        DateTime StartOfHour = (new DateTime(LastTimeSummaryWasRun.Year, LastTimeSummaryWasRun.Month, LastTimeSummaryWasRun.Day, LastTimeSummaryWasRun.Hour, 1, 1)).AddHours(-1); // (Int(24 * LastTimeSummaryWasRun) / 24) - PeriodStep
                        DateTime OldestDateAdded = StartOfHour;
                        using (var csXfer = new CsModel(core)) {
                            if (csXfer.csOpenSql(core.db.getSQLSelect("default", "ccVisits", "DateAdded", "LastVisitTime>" + DbController.encodeSQLDate(StartOfHour), "dateadded", "", 1))) {
                                OldestDateAdded = csXfer.csGetDate("DateAdded");
                                if (OldestDateAdded < NextSummaryStartDate) {
                                    NextSummaryStartDate = OldestDateAdded;
                                    logHousekeeping(core, "Update hourly visit summary, found a visit with the last viewing during the past hour. It started [" + OldestDateAdded + "], before the last summary was run.");
                                }
                            }
                        }
                        //
                        // Verify there are 24 hour records for every day back the past 90 days
                        //
                        DateTime DateofMissingSummary = DateTime.MinValue;
                        //Call AppendClassLog(core, core.appEnvironment.name, "HouseKeep", "Verify there are 24 hour records for the past 90 days")
                        DateTime PeriodStartDate = rightNow.Date.AddDays(-90);
                        double PeriodStep = 1;
                        int HoursPerDay = 0;
                        for (double PeriodDatePtr = PeriodStartDate.ToOADate(); PeriodDatePtr <= OldestDateAdded.ToOADate(); PeriodDatePtr += PeriodStep) {
                            using (var csXfer = new CsModel(core)) {
                                if (csXfer.csOpenSql("select count(id) as HoursPerDay from ccVisitSummary where TimeDuration=1 and DateNumber=" + encodeInteger(PeriodDatePtr) + " group by DateNumber")) {
                                    HoursPerDay = csXfer.csGetInteger("HoursPerDay");
                                }
                                csXfer.close();
                                if (HoursPerDay < 24) {
                                    DateofMissingSummary = DateTime.FromOADate(PeriodDatePtr);
                                    break;
                                }
                            }
                            if ((DateofMissingSummary != DateTime.MinValue) && (DateofMissingSummary < NextSummaryStartDate)) {
                                logHousekeeping(core, "Found a missing hourly period in the visit summary table [" + DateofMissingSummary + "], it only has [" + HoursPerDay + "] hourly summaries.");
                                NextSummaryStartDate = DateofMissingSummary;
                            }
                            //
                            // Now summarize all visits during all hourly periods between OldestDateAdded and the previous Hour
                            //
                            logHousekeeping(core, "Summaryize visits hourly, starting [" + NextSummaryStartDate + "]");
                            PeriodStep = (double)1 / (double)24;
                            //PeriodStart = (Int(OldestDateAdded * 24) / 24)
                            houseKeep_VisitSummary(core, NextSummaryStartDate, rightNow, 1, core.siteProperties.dataBuildVersion, OldestVisitSummaryWeCareAbout);
                        }
                        //
                        // OK to run archive
                        // During archive, non-cookie records are removed, so this has to run after summarizing
                        // and we can only delete non-cookie records older than 2 days (so we can be sure they have been summarized)
                        //
                        if (force) {
                            //
                            // debug mode - run achive if no times are given
                            //
                            HouseKeep_App_Daily(core, VisitArchiveAgeDays, GuestArchiveAgeDays, EmailDropArchiveAgeDays, DefaultMemberName, core.siteProperties.dataBuildVersion);
                        } else {
                            //
                            // Check for site's archive time of day
                            //
                            string AlarmTimeString = core.siteProperties.getText("ArchiveTimeOfDay", "12:00:00 AM");
                            if (string.IsNullOrEmpty(AlarmTimeString)) {
                                AlarmTimeString = "12:00:00 AM";
                                core.siteProperties.setProperty("ArchiveTimeOfDate", AlarmTimeString);
                            }
                            if (!GenericController.IsDate(AlarmTimeString)) {
                                AlarmTimeString = "12:00:00 AM";
                                core.siteProperties.setProperty("ArchiveTimeOfDate", AlarmTimeString);
                            }
                            //AlarmTimeMinutesSinceMidnight = genericController.encodeDate(AlarmTimeString).TimeOfDay.TotalMinutes;
                            double minutesSinceMidnight = rightNow.TimeOfDay.TotalMinutes;
                            double LastCheckMinutesFromMidnight = LastCheckDateTime.TimeOfDay.TotalMinutes;
                            if ((minutesSinceMidnight > LastCheckMinutesFromMidnight) && (LastCheckMinutesFromMidnight < minutesSinceMidnight)) {
                                //
                                // Same Day - Midnight is before last and after current
                                //
                                HouseKeep_App_Daily(core, VisitArchiveAgeDays, GuestArchiveAgeDays, EmailDropArchiveAgeDays, DefaultMemberName, core.siteProperties.dataBuildVersion);
                            } else if ((LastCheckMinutesFromMidnight > minutesSinceMidnight) && ((LastCheckMinutesFromMidnight < minutesSinceMidnight))) {
                                //
                                // New Day - Midnight is between Last and Set
                                //
                                HouseKeep_App_Daily(core, VisitArchiveAgeDays, GuestArchiveAgeDays, EmailDropArchiveAgeDays, DefaultMemberName, core.siteProperties.dataBuildVersion);
                            }
                        }
                    }
                }
            } catch (Exception) {
                throw;
            }
        }
        //
        //====================================================================================================
        //
        private void HouseKeep_App_Daily(CoreController core, int VisitArchiveAgeDays, int GuestArchiveAgeDays, int EmailDropArchiveAgeDays, string DefaultMemberName, string BuildVersion) {
            try {
                DateTime rightNow = DateTime.Now;
                //
                DateTime Yesterday = rightNow.AddDays(-1).Date;
                DateTime MidnightTwoDaysAgo = rightNow.AddDays(-2).Date;
                DateTime thirtyDaysAgo = rightNow.AddDays(-30).Date;
                string appName = core.appConfig.name;
                bool ArchiveDeleteNoCookie = GenericController.encodeBoolean(core.siteProperties.getText("ArchiveDeleteNoCookie", "1"));
                int DataSourceType = core.db.getDataSourceType("default");
                int TimeoutSave = core.db.sqlCommandTimeout;
                core.db.sqlCommandTimeout = 1800;
                string SQLDateMidnightTwoDaysAgo = DbController.encodeSQLDate(MidnightTwoDaysAgo);
                //
                // Any member records that were created outside contensive need to have CreatedByVisit=0 (past v4.1.152)
                core.db.executeQuery("update ccmembers set CreatedByVisit=0 where createdbyvisit is null");
                string sql = null;
                //
                // delete nocookie visits
                // This must happen after the housekeep summarizing, and no sooner then 48 hours ago so all hits have been summarized before deleting
                //
                if (ArchiveDeleteNoCookie) {
                    //
                    // delete members from the non-cookie visits
                    // legacy records without createdbyvisit will have to be corrected by hand (or upgrade)
                    //
                    logHousekeeping(core, "Deleting members from visits with no cookie support older than Midnight, Two Days Ago");
                    switch (DataSourceType) {
                        case DataSourceTypeODBCAccess:
                            sql = "delete m.*"
                                + " from ccmembers m,ccvisits v"
                                + " where v.memberid=m.id"
                                + " and(m.Visits=1)"
                                + " and(m.createdbyvisit=1)"
                                + " and(m.Username is null)"
                                + " and(m.email is null)"
                                + " and(v.CookieSupport=0)and(v.LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")";
                            break;
                        case DataSourceTypeODBCMySQL:
                            sql = "delete m"
                                + " from ccmembers m,ccvisits v"
                                + " where v.memberid=m.id"
                                + " and(m.Visits=1)"
                                + " and(m.createdbyvisit=1)"
                                + " and(m.Username is null)"
                                + " and(m.email is null)"
                                + " and(v.CookieSupport=0)and(v.LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")";
                            break;
                        default:
                            sql = "delete from ccmembers from ccmembers m,ccvisits v"
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
                        core.db.executeQuery(sql);
                    } catch (Exception) {
                    }

                    //
                    // delete viewings from the non-cookie visits
                    //
                    logHousekeeping(core, "Deleting viewings from visits with no cookie support older than Midnight, Two Days Ago");
                    switch (DataSourceType) {
                        case DataSourceTypeODBCAccess:
                            sql = "delete h.*"
                                + " from ccviewings h,ccvisits v"
                                + " where h.visitid=v.id"
                                + " and(v.CookieSupport=0)and(v.LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")";
                            break;
                        case DataSourceTypeODBCMySQL:
                            sql = "delete h"
                                + " from ccviewings h,ccvisits v"
                                + " where h.visitid=v.id"
                                + " and(v.CookieSupport=0)and(v.LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")";
                            break;
                        default:
                            sql = "delete from ccviewings"
                                + " from ccviewings h,ccvisits v"
                                + " where h.visitid=v.id"
                                + " and(v.CookieSupport=0)and(v.LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")";
                            break;
                    }
                    // if this fails, continue with the rest of the work
                    try {
                        core.db.executeQuery(sql);
                    } catch (Exception) {
                    }
                    //
                    // delete visitors from the non-cookie visits
                    //
                    logHousekeeping(core, "Deleting visitors from visits with no cookie support older than Midnight, Two Days Ago");
                    switch (DataSourceType) {
                        case DataSourceTypeODBCAccess:
                            sql = "delete r.*"
                                + " from ccvisitors r,ccvisits v"
                                + " where r.id=v.visitorid"
                                + " and(v.CookieSupport=0)and(v.LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")";
                            break;
                        case DataSourceTypeODBCMySQL:
                            sql = "delete r"
                                + " from ccvisitors r,ccvisits v"
                                + " where r.id=v.visitorid"
                                + " and(v.CookieSupport=0)and(v.LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")";
                            break;
                        default:
                            sql = "delete from ccvisitors"
                                + " from ccvisitors r,ccvisits v"
                                + " where r.id=v.visitorid"
                                + " and(v.CookieSupport=0)and(v.LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")";
                            break;
                    }
                    try {
                        core.db.executeQuery(sql);
                    } catch (Exception) {
                    }
                    //
                    // delete visits from the non-cookie visits
                    //
                    logHousekeeping(core, "Deleting visits with no cookie support older than Midnight, Two Days Ago");
                    core.db.deleteTableRecordChunks("default", "ccvisits", "(CookieSupport=0)and(LastVisitTime<" + SQLDateMidnightTwoDaysAgo + ")", 1000, 10000);
                }
                //
                // Visits with no DateAdded
                //
                logHousekeeping(core, "Deleting visits with no DateAdded");
                core.db.deleteTableRecordChunks("default", "ccvisits", "(DateAdded is null)or(DateAdded<=" + DbController.encodeSQLDate(new DateTime(1995, 1, 1)) + ")", 1000, 10000);
                //
                // Visits with no visitor
                //
                logHousekeeping(core, "Deleting visits with no DateAdded");
                core.db.deleteTableRecordChunks("default", "ccvisits", "(VisitorID is null)or(VisitorID=0)", 1000, 10000);
                //
                // viewings with no visit
                //
                logHousekeeping(core, "Deleting viewings with null or invalid VisitID");
                core.db.deleteTableRecordChunks("default", "ccviewings", "(visitid=0 or visitid is null)", 1000, 10000);
                DateTime OldestVisitDate = default(DateTime);
                //
                // Get Oldest Visit
                using (var csXfer = new CsModel(core)) {
                    if (csXfer.csOpenSql(core.db.getSQLSelect("default", "ccVisits", "DateAdded", "", "dateadded", "", 1))) {
                        OldestVisitDate = csXfer.csGetDate("DateAdded").Date;
                    }
                }
                DateTime ArchiveDate = default(DateTime);
                //
                // Remove old visit records
                //   if > 30 days in visit table, limit one pass to just 30 days
                //   this is to prevent the entire server from being bogged down for one site change
                //
                if (OldestVisitDate == DateTime.MinValue) {
                    logHousekeeping(core, "No records were removed because no visit records were found while requesting the oldest visit.");
                } else if (VisitArchiveAgeDays <= 0) {
                    logHousekeeping(core, "No records were removed because Housekeep ArchiveRecordAgeDays is 0.");
                } else {
                    ArchiveDate = rightNow.AddDays(-VisitArchiveAgeDays).Date;
                    int DaystoRemove = encodeInteger(ArchiveDate.Subtract(OldestVisitDate).TotalDays);
                    if (DaystoRemove > 30) {
                        ArchiveDate = OldestVisitDate.AddDays(30);
                    }
                    if (OldestVisitDate >= ArchiveDate) {
                        logHousekeeping(core, "No records were removed because Oldest Visit Date [" + OldestVisitDate + "] >= ArchiveDate [" + ArchiveDate + "].");
                    } else {
                        logHousekeeping(core, "Removing records from [" + OldestVisitDate + "] to [" + ArchiveDate + "].");
                        DateTime SingleDate = default(DateTime);
                        SingleDate = OldestVisitDate;
                        do {
                            HouseKeep_App_Daily_RemoveVisitRecords(core, SingleDate, DataSourceType);
                            SingleDate = SingleDate.AddDays(1);
                        } while (SingleDate < ArchiveDate);
                    }
                }
                //
                // Remove old guest records
                //
                ArchiveDate = rightNow.AddDays(-GuestArchiveAgeDays).Date;
                HouseKeep_App_Daily_RemoveGuestRecords(core, ArchiveDate, DataSourceType);
                //
                // delete 'guests' Members with one visits but no valid visit record
                //
                logHousekeeping(core, "Deleting 'guest' members with no visits (name is default name, visits=1, username null, email null,dateadded=lastvisit)");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        sql = "delete m.*"
                            + " from ccmembers m,ccvisits v"
                            + " where v.memberid=m.id"
                            + " and(m.createdbyvisit=1)"
                            + " and(m.Visits=1)"
                            + " and(m.Username is null)"
                            + " and(m.email is null)"
                            + " and(m.dateadded=m.lastvisit)"
                            + " and(v.id is null)";
                        break;
                    case DataSourceTypeODBCMySQL:
                        sql = "delete m"
                            + " from ccmembers m,ccvisits v"
                            + " where v.memberid=m.id"
                            + " and(m.createdbyvisit=1)"
                            + " and(m.Visits=1)"
                            + " and(m.Username is null)"
                            + " and(m.email is null)"
                            + " and(m.dateadded=m.lastvisit)"
                            + " and(v.id is null)";
                        break;
                    default:
                        sql = "delete from ccmembers from ccmembers m,ccvisits v"
                            + " where v.memberid=m.id"
                            + " and(m.createdbyvisit=1)"
                            + " and(m.Visits=1)"
                            + " and(m.Username is null)"
                            + " and(m.email is null)"
                            + " and(m.dateadded=m.lastvisit)"
                            + " and(v.id is null)";
                        break;
                }
                core.db.executeNonQuery(sql);
                //
                // delete 'guests' Members created before ArchivePeopleAgeDays
                //
                logHousekeeping(core, "Deleting 'guest' members with no visits (name is default name, visits=1, username null, email null,dateadded=lastvisit)");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        sql = "delete m.*"
                            + " from ccmembers m left join ccvisits v on v.memberid=m.id"
                            + " where(m.createdbyvisit=1)"
                            + " and(m.Visits=1)"
                            + " and(m.Username is null)"
                            + " and(m.email is null)"
                            + " and(m.dateadded=m.lastvisit)"
                            + " and(v.id is null)";
                        break;
                    case DataSourceTypeODBCMySQL:
                        sql = "delete m"
                            + " from ccmembers m left join ccvisits v on v.memberid=m.id"
                            + " where(m.createdbyvisit=1)"
                            + " and(m.Visits=1)"
                            + " and(m.Username is null)"
                            + " and(m.email is null)"
                            + " and(m.dateadded=m.lastvisit)"
                            + " and(v.id is null)";
                        break;
                    default:
                        sql = "delete from ccmembers from ccmembers m left join ccvisits v on v.memberid=m.id"
                            + " where(m.createdbyvisit=1)"
                            + " and(m.Visits=1)"
                            + " and(m.Username is null)"
                            + " and(m.email is null)"
                            + " and(m.dateadded=m.lastvisit)"
                            + " and(v.id is null)";
                        break;
                }
                core.db.executeNonQuery(sql);
                //
                // delete email drops older than archive.
                //
                logHousekeeping(core, "Deleting email drops older then " + EmailDropArchiveAgeDays + " days");
                //
                DateTime ArchiveEmailDropDate = default(DateTime);
                ArchiveEmailDropDate = rightNow.AddDays(-EmailDropArchiveAgeDays).Date;
                MetaController.deleteContentRecords(core, "Email drops", "(DateAdded is null)or(DateAdded<=" + DbController.encodeSQLDate(ArchiveEmailDropDate) + ")");
                //
                // delete email log entries not realted to a drop, older than archive.
                //
                logHousekeeping(core, "Deleting non-drop email logs older then " + EmailDropArchiveAgeDays + " days");
                ArchiveEmailDropDate = rightNow.AddDays(-EmailDropArchiveAgeDays).Date;
                MetaController.deleteContentRecords(core, "Email Log", "(emailDropId is null)and((DateAdded is null)or(DateAdded<=" + DbController.encodeSQLDate(ArchiveEmailDropDate) + "))");
                //
                // block duplicate redirect fields (match contentid+fieldtype+caption)
                //
                logHousekeeping(core, "Inactivate duplicate redirect fields");
                int FieldContentID = 0;
                string FieldLast = null;
                string FieldNew = null;
                int FieldRecordID = 0;
                using (var csXfer = new CsModel(core)) {
                    csXfer.csOpenSql("Select ID, ContentID, Type, Caption from ccFields where (active<>0)and(Type=" + fieldTypeIdRedirect + ") Order By ContentID, Caption, ID");
                    FieldLast = "";
                    while (csXfer.csOk()) {
                        FieldContentID = csXfer.csGetInteger("Contentid");
                        string FieldCaption = csXfer.csGetText("Caption");
                        FieldNew = FieldContentID + FieldCaption;
                        if (FieldNew == FieldLast) {
                            FieldRecordID = csXfer.csGetInteger("ID");
                            core.db.executeNonQuery("Update ccFields set active=0 where ID=" + FieldRecordID + ";");
                        }
                        FieldLast = FieldNew;
                        csXfer.csGoNext();
                    }
                }
                //
                // block duplicate non-redirect fields (match contentid+fieldtype+name)
                logHousekeeping(core, "Inactivate duplicate non-redirect fields");
                using (var csXfer = new CsModel(core)) {
                    FieldLast = "";
                    string FieldName = null;
                    while (csXfer.csOpenSql("Select ID, Name, ContentID, Type from ccFields where (active<>0)and(Type<>" + fieldTypeIdRedirect + ") Order By ContentID, Name, Type, ID")) {
                        int fieldType = csXfer.csGetInteger("Type");
                        FieldContentID = csXfer.csGetInteger("Contentid");
                        FieldName = csXfer.csGetText("Name");
                        FieldRecordID = csXfer.csGetInteger("ID");
                        FieldNew = FieldContentID + FieldName + fieldType;
                        if (FieldNew == FieldLast) {
                            core.db.executeQuery("Update ccFields set active=0 where ID=" + FieldRecordID + ";");
                        }
                        FieldLast = FieldNew;
                        csXfer.csGoNext();
                    }
                }
                //
                // Activities with no Member
                //
                logHousekeeping(core, "Deleting activities with no member record.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        sql = "delete ccactivitylog.*"
                            + " From ccactivitylog LEFT JOIN ccmembers on ccmembers.ID=ccactivitylog.memberid"
                            + " WHERE (ccmembers.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        sql = "delete from ccactivitylog"
                            + " From ccactivitylog LEFT JOIN ccmembers on ccmembers.ID=ccactivitylog.memberid"
                            + " WHERE (ccmembers.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                    default:
                        sql = "delete ccactivitylog"
                            + " From ccactivitylog LEFT JOIN ccmembers on ccmembers.ID=ccactivitylog.memberid"
                            + " WHERE (ccmembers.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                }
                //
                // Member Properties with no member
                //
                logHousekeeping(core, "Deleting member properties with no member record.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        sql = "delete ccProperties.*"
                            + " From ccProperties LEFT JOIN ccmembers on ccmembers.ID=ccProperties.KeyID"
                            + " WHERE (ccProperties.TypeID=0)"
                            + " AND (ccmembers.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        sql = "delete From ccProperties"
                            + " From ccProperties LEFT JOIN ccmembers on ccmembers.ID=ccProperties.KeyID"
                            + " WHERE (ccProperties.TypeID=0)"
                            + " AND (ccmembers.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                    default:
                        sql = "delete ccProperties"
                            + " From ccProperties LEFT JOIN ccmembers on ccmembers.ID=ccProperties.KeyID"
                            + " WHERE (ccProperties.TypeID=0)"
                            + " AND (ccmembers.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                }
                //
                // Visit Properties with no visits
                //
                logHousekeeping(core, "Deleting visit properties with no visit record.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        sql = "delete ccProperties.*"
                            + " from ccProperties LEFT JOIN ccVisits on ccVisits.ID=ccProperties.KeyID"
                            + " WHERE (ccProperties.TypeID=1)"
                            + " AND (ccVisits.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        sql = "delete From ccProperties"
                            + " from ccProperties LEFT JOIN ccVisits on ccVisits.ID=ccProperties.KeyID"
                            + " WHERE (ccProperties.TypeID=1)"
                            + " AND (ccVisits.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                    default:
                        sql = "delete ccProperties"
                            + " from ccProperties LEFT JOIN ccVisits on ccVisits.ID=ccProperties.KeyID"
                            + " WHERE (ccProperties.TypeID=1)"
                            + " AND (ccVisits.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                }
                //
                // Visitor Properties with no visitor
                //
                logHousekeeping(core, "Deleting visitor properties with no visitor record.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        sql = "delete ccProperties.*"
                            + " from ccProperties LEFT JOIN ccvisitors on ccvisitors.ID=ccProperties.KeyID"
                            + " where ccproperties.typeid=2"
                            + " and ccvisitors.id is null";
                        core.db.executeQuery(sql);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        sql = "delete From ccProperties"
                            + " from ccProperties LEFT JOIN ccvisitors on ccvisitors.ID=ccProperties.KeyID"
                            + " where ccproperties.typeid=2"
                            + " and ccvisitors.id is null";
                        core.db.executeQuery(sql);
                        break;
                    default:
                        sql = "delete ccProperties"
                            + " from ccProperties LEFT JOIN ccvisitors on ccvisitors.ID=ccProperties.KeyID"
                            + " where ccproperties.typeid=2"
                            + " and ccvisitors.id is null";
                        core.db.executeQuery(sql);
                        break;
                }
                //
                // MemberRules with bad MemberID
                //
                logHousekeeping(core, "Deleting Member Rules with bad MemberID.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        sql = "delete ccmemberrules.*"
                            + " From ccmemberrules"
                            + " LEFT JOIN ccmembers on ccmembers.ID=ccmemberrules.MemberID"
                            + " WHERE (ccmembers.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        sql = "delete From ccmemberrules"
                            + " From ccmemberrules"
                            + " LEFT JOIN ccmembers on ccmembers.ID=ccmemberrules.MemberID"
                            + " WHERE (ccmembers.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                    default:
                        sql = "delete ccmemberrules"
                            + " From ccmemberrules"
                            + " LEFT JOIN ccmembers on ccmembers.ID=ccmemberrules.MemberID"
                            + " WHERE (ccmembers.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                }
                //
                // MemberRules with bad GroupID
                //
                logHousekeeping(core, "Deleting Member Rules with bad GroupID.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        sql = "delete ccmemberrules.*"
                            + " From ccmemberrules"
                            + " LEFT JOIN ccgroups on ccgroups.ID=ccmemberrules.GroupID"
                            + " WHERE (ccgroups.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        sql = "delete From ccmemberrules"
                            + " From ccmemberrules"
                            + " LEFT JOIN ccgroups on ccgroups.ID=ccmemberrules.GroupID"
                            + " WHERE (ccgroups.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                    default:
                        sql = "delete ccmemberrules"
                            + " From ccmemberrules"
                            + " LEFT JOIN ccgroups on ccgroups.ID=ccmemberrules.GroupID"
                            + " WHERE (ccgroups.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                }
                //
                // GroupRules with bad ContentID
                //   Handled record by record removed to prevent CDEF reload
                //
                logHousekeeping(core, "Deleting Group Rules with bad ContentID.");
                sql = "Select ccGroupRules.ID"
                    + " From ccGroupRules LEFT JOIN ccContent on ccContent.ID=ccGroupRules.ContentID"
                    + " WHERE (ccContent.ID is null)";
                using (var csXfer = new CsModel(core)) {
                    csXfer.csOpenSql(sql);
                    while (csXfer.csOk()) {
                        MetaController.deleteContentRecord(core, "Group Rules", csXfer.csGetInteger("ID"));
                        csXfer.csGoNext();
                    }
                }
                //
                // GroupRules with bad GroupID
                //
                logHousekeeping(core, "Deleting Group Rules with bad GroupID.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        sql = "delete ccGroupRules.*"
                            + " From ccGroupRules"
                            + " LEFT JOIN ccgroups on ccgroups.ID=ccGroupRules.GroupID"
                            + " WHERE (ccgroups.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        sql = "delete from ccGroupRules"
                            + " From ccGroupRules"
                            + " LEFT JOIN ccgroups on ccgroups.ID=ccGroupRules.GroupID"
                            + " WHERE (ccgroups.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                    default:
                        sql = "delete ccGroupRules"
                            + " From ccGroupRules"
                            + " LEFT JOIN ccgroups on ccgroups.ID=ccGroupRules.GroupID"
                            + " WHERE (ccgroups.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                }
                //
                // ContentWatch with bad CContentID
                //     must be deleted manually
                //
                logHousekeeping(core, "Deleting Content Watch with bad ContentID.");
                using (var csXfer = new CsModel(core)) {
                    sql = "Select ccContentWatch.ID"
                        + " From ccContentWatch LEFT JOIN ccContent on ccContent.ID=ccContentWatch.ContentID"
                        + " WHERE (ccContent.ID is null)or(ccContent.Active=0)or(ccContent.Active is null)";
                    csXfer.csOpenSql(sql);
                    while (csXfer.csOk()) {
                        MetaController.deleteContentRecord(core, "Content Watch", csXfer.csGetInteger("ID"));
                        csXfer.csGoNext();
                    }
                }
                //
                // ContentWatchListRules with bad ContentWatchID
                //
                logHousekeeping(core, "Deleting ContentWatchList Rules with bad ContentWatchID.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        sql = "delete ccContentWatchListRules.*"
                            + " From ccContentWatchListRules"
                            + " LEFT JOIN ccContentWatch on ccContentWatch.ID=ccContentWatchListRules.ContentWatchID"
                            + " WHERE (ccContentWatch.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        sql = "delete from ccContentWatchListRules"
                            + " From ccContentWatchListRules"
                            + " LEFT JOIN ccContentWatch on ccContentWatch.ID=ccContentWatchListRules.ContentWatchID"
                            + " WHERE (ccContentWatch.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                    default:
                        sql = "delete ccContentWatchListRules"
                            + " From ccContentWatchListRules"
                            + " LEFT JOIN ccContentWatch on ccContentWatch.ID=ccContentWatchListRules.ContentWatchID"
                            + " WHERE (ccContentWatch.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                }
                //
                // ContentWatchListRules with bad ContentWatchListID
                //
                logHousekeeping(core, "Deleting ContentWatchList Rules with bad ContentWatchListID.");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        sql = "delete ccContentWatchListRules.*"
                            + " From ccContentWatchListRules"
                            + " LEFT JOIN ccContentWatchLists on ccContentWatchLists.ID=ccContentWatchListRules.ContentWatchListID"
                            + " WHERE (ccContentWatchLists.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                    case DataSourceTypeODBCSQLServer:
                        sql = "delete from ccContentWatchListRules"
                            + " From ccContentWatchListRules"
                            + " LEFT JOIN ccContentWatchLists on ccContentWatchLists.ID=ccContentWatchListRules.ContentWatchListID"
                            + " WHERE (ccContentWatchLists.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                    default:
                        sql = "delete ccContentWatchListRules"
                            + " From ccContentWatchListRules"
                            + " LEFT JOIN ccContentWatchLists on ccContentWatchLists.ID=ccContentWatchListRules.ContentWatchListID"
                            + " WHERE (ccContentWatchLists.ID is null)";
                        core.db.executeQuery(sql);
                        break;
                }
                //
                // Field help with no field
                //
                logHousekeeping(core, "Deleting field help with no field.");
                sql = ""
                    + "delete from ccfieldhelp where id in ("
                    + " select h.id"
                    + " from ccfieldhelp h"
                    + " left join ccfields f on f.id=h.fieldid where f.id is null"
                    + ")";
                core.db.executeQuery(sql);
                //
                // Field help duplicates - messy, but I am not sure where they are coming from, and this patchs the edit page performance problem
                //
                logHousekeeping(core, "Deleting duplicate field help records.");
                sql = ""
                    + "delete from ccfieldhelp where id in ("
                    + " select b.id"
                    + " from ccfieldhelp a"
                    + " left join ccfieldhelp b on a.fieldid=b.fieldid where a.id< b.id"
                    + ")";
                core.db.executeQuery(sql);
                //
                //addon editor rules with no addon
                //
                sql = "delete from ccAddonContentFieldTypeRules where id in ("
                    + "select r.id from ccAddonContentFieldTypeRules r left join ccaggregatefunctions a on a.id=r.addonid where a.Id Is Null"
                    + ")";
                core.db.executeQuery(sql);
                //
                // convert FieldTypeLongText + htmlContent to FieldTypeHTML
                //
                logHousekeeping(core, "convert FieldTypeLongText + htmlContent to FieldTypeHTML.");
                sql = "update ccfields set type=" + fieldTypeIdHTML + " where type=" + fieldTypeIdLongText + " and ( htmlcontent<>0 )";
                core.db.executeQuery(sql);
                //
                // convert FieldTypeTextFile + htmlContent to FieldTypeHTMLFile
                //
                //Call AppendClassLog(appName, "HouseKeep_App_Daily(" & appName & ")", "convert FieldTypeTextFile + htmlContent to FieldTypeHTMLFile.")
                //SQL = "update ccfields set type=" & FieldTypeIdFileHTMLPrivate & " where type=" & FieldTypeIdFileTextPrivate & " and ( htmlcontent<>0 )"
                //Call core.app.executeSql(SQL)
                //
                // Log files Older then 30 days
                //
                HouseKeep_App_Daily_LogFolder(core, "temp", thirtyDaysAgo);
                HouseKeep_App_Daily_LogFolder(core, "TrapLogs", thirtyDaysAgo);
                HouseKeep_App_Daily_LogFolder(core, "BounceLog", thirtyDaysAgo);
                HouseKeep_App_Daily_LogFolder(core, "BounceProcessing", thirtyDaysAgo);
                HouseKeep_App_Daily_LogFolder(core, "SMTPLog", thirtyDaysAgo);
                HouseKeep_App_Daily_LogFolder(core, "DebugLog", thirtyDaysAgo);
                //
                // Content TextFile types with no controlling record
                //
                if (GenericController.encodeBoolean(core.siteProperties.getText("ArchiveAllowFileClean", "false"))) {
                    //
                    int DSType = core.db.getDataSourceType("");
                    logHousekeeping(core, "Content TextFile types with no controlling record.");
                    using (var csXfer = new CsModel(core)) {
                        sql = "SELECT DISTINCT ccTables.Name as TableName, ccFields.Name as FieldName"
                            + " FROM (ccFields LEFT JOIN ccContent ON ccFields.ContentID = ccContent.ID) LEFT JOIN ccTables ON ccContent.ContentTableID = ccTables.ID"
                            + " Where (((ccFields.Type) = 10))"
                            + " ORDER BY ccTables.Name";
                        csXfer.csOpenSql(sql);
                        while (csXfer.csOk()) {
                            //
                            // Get all the files in this path, and check that the record exists with this in its field
                            //
                            string FieldName = csXfer.csGetText("FieldName");
                            string TableName = csXfer.csGetText("TableName");
                            string PathName = TableName + "\\" + FieldName;
                            List<CPFileSystemBaseClass.FileDetail> FileList = core.cdnFiles.getFileList(PathName);
                            if (FileList.Count > 0) {
                                core.db.executeQuery("CREATE INDEX temp" + FieldName + " ON " + TableName + " (" + FieldName + ")");
                                foreach (CPFileSystemBaseClass.FileDetail file in FileList) {
                                    string Filename = file.Name;
                                    string VirtualFileName = PathName + "\\" + Filename;
                                    string VirtualLink = GenericController.vbReplace(VirtualFileName, "\\", "/");
                                    long FileSize = file.Size;
                                    if (FileSize == 0) {
                                        sql = "update " + TableName + " set " + FieldName + "=null where (" + FieldName + "=" + DbController.encodeSQLText(VirtualFileName) + ")or(" + FieldName + "=" + DbController.encodeSQLText(VirtualLink) + ")";
                                        core.db.executeQuery(sql);
                                        core.cdnFiles.deleteFile(VirtualFileName);
                                    } else {
                                        using (var csTest = new CsModel(core)) {
                                            sql = "SELECT ID FROM " + TableName + " WHERE (" + FieldName + "=" + DbController.encodeSQLText(VirtualFileName) + ")or(" + FieldName + "=" + DbController.encodeSQLText(VirtualLink) + ")";
                                            if (!csTest.csOpenSql(sql)) {
                                                core.cdnFiles.deleteFile(VirtualFileName);
                                            }
                                        }
                                    }
                                }
                                if (DSType == 1) {
                                    // access
                                    sql = "Drop INDEX temp" + FieldName + " ON " + TableName;
                                } else if (DSType == 2) {
                                    // sql server
                                    sql = "DROP INDEX " + TableName + ".temp" + FieldName;
                                } else {
                                    // mysql
                                    sql = "ALTER TABLE " + TableName + " DROP INDEX temp" + FieldName;
                                }
                                core.db.executeQuery(sql);
                            }
                            csXfer.csGoNext();
                        }
                    }
                }
                core.db.sqlCommandTimeout = TimeoutSave;
                return;
                //
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //====================================================================================================
        //
        private void HouseKeep_App_Daily_RemoveVisitRecords(CoreController core, DateTime DeleteBeforeDate, int DataSourceType) {
            try {
                //
                int TimeoutSave = 0;
                string SQL = null;
                string DeleteBeforeDateSQL = null;
                string appName = null;
                string SQLTablePeople = null;
                //
                // Set long timeout (30 min) needed for heavy work on big tables
                TimeoutSave = core.db.sqlCommandTimeout;
                core.db.sqlCommandTimeout = 1800;
                //
                SQLTablePeople = MetaController.getContentTablename(core, "People");
                //
                appName = core.appConfig.name;
                DeleteBeforeDateSQL = DbController.encodeSQLDate(DeleteBeforeDate);
                //
                // Visits older then archive age
                //
                logHousekeeping(core, "Deleting visits before [" + DeleteBeforeDateSQL + "]");
                core.db.deleteTableRecordChunks("default", "ccVisits", "(DateAdded<" + DeleteBeforeDateSQL + ")", 1000, 10000);
                //
                // Viewings with visits before the first
                //
                logHousekeeping(core, "Deleting viewings with visitIDs lower then the lowest ccVisits.ID");
                core.db.deleteTableRecordChunks("default", "ccviewings", "(visitid<(select min(ID) from ccvisits))", 1000, 10000);
                //
                // Visitors with no visits
                //
                logHousekeeping(core, "Deleting visitors with no visits");
                switch (DataSourceType) {
                    case DataSourceTypeODBCAccess:
                        SQL = "delete ccVisitors.*"
                            + " from ccVisitors Left Join ccVisits on ccVisits.VisitorID=ccVisitors.ID"
                            + " where ccVisits.ID is null";
                        core.db.executeQuery(SQL);

                        break;
                    case DataSourceTypeODBCSQLServer:
                        SQL = "delete From ccVisitors"
                            + " from ccVisitors Left Join ccVisits on ccVisits.VisitorID=ccVisitors.ID"
                            + " where ccVisits.ID is null";
                        core.db.executeQuery(SQL);
                        break;
                    default:
                        SQL = "delete ccVisitors"
                            + " from ccVisitors Left Join ccVisits on ccVisits.VisitorID=ccVisitors.ID"
                            + " where ccVisits.ID is null";
                        core.db.executeQuery(SQL);
                        break;
                }
                //
                // restore sved timeout
                //
                core.db.sqlCommandTimeout = TimeoutSave;
                return;
                //
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //====================================================================================================
        //
        private void HouseKeep_App_Daily_RemoveGuestRecords(CoreController core, DateTime DeleteBeforeDate, int DataSourceType) {
            int TimeoutSave = core.db.sqlCommandTimeout;
            try {
                //
                // Set long timeout (30 min) needed for heavy work on big tables
                core.db.sqlCommandTimeout = 1800;
                string SQLTablePeople = MetaController.getContentTablename(core, "People");
                string DeleteBeforeDateSQL = DbController.encodeSQLDate(DeleteBeforeDate);
                //
                logHousekeeping(core, "Deleting members with  LastVisit before DeleteBeforeDate [" + DeleteBeforeDate + "], exactly one total visit, a null username and a null email address.");
                //
                string SQLCriteria = ""
                    + " (LastVisit<" + DeleteBeforeDateSQL + ")"
                    + " and(createdbyvisit=1)"
                    + " and(Visits=1)"
                    + " and(Username is null)"
                    + " and(email is null)";
                core.db.deleteTableRecordChunks("default", "ccmembers", SQLCriteria, 1000, 10000);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            } finally {
                //
                // restore sved timeout
                core.db.sqlCommandTimeout = TimeoutSave;
            }
            return;
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
        public void houseKeep_VisitSummary(CoreController core, DateTime StartTimeDate, DateTime EndTimeDate, int HourDuration, string BuildVersion, DateTime OldestVisitSummaryWeCareAbout) {
            try {
                double StartTimeHoursSinceMidnight = 0;
                DateTime PeriodStart = default(DateTime);
                double TotalTimeOnSite = 0;
                int MultiPageVisitCnt = 0;
                int MultiPageHitCnt = 0;
                double MultiPageTimetoLastHitSum = 0;
                double TimeOnSite = 0;
                DateTime PeriodDatePtr = default(DateTime);
                int DateNumber = 0;
                int TimeNumber = 0;
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
                XmlDocument LibraryCollections = new XmlDocument();
                XmlDocument LocalCollections = new XmlDocument();
                XmlDocument Doc = new XmlDocument();
                string SQL = null;
                int AveReadTime = 0;
                //
                if (string.CompareOrdinal(BuildVersion, core.codeVersion()) < 0) {
                } else {
                    PeriodStart = StartTimeDate;
                    if (PeriodStart < OldestVisitSummaryWeCareAbout) {
                        PeriodStart = OldestVisitSummaryWeCareAbout;
                    }
                    StartTimeHoursSinceMidnight = PeriodStart.TimeOfDay.TotalHours;
                    PeriodStart = PeriodStart.Date.AddHours(StartTimeHoursSinceMidnight);
                    PeriodDatePtr = PeriodStart;
                    while (PeriodDatePtr < EndTimeDate) {
                        //
                        DateNumber = encodeInteger(PeriodDatePtr.AddHours(HourDuration / 2.0).ToOADate());
                        TimeNumber = encodeInteger(PeriodDatePtr.TimeOfDay.TotalHours);
                        DateStart = PeriodDatePtr.Date;
                        DateEnd = PeriodDatePtr.AddHours(HourDuration).Date;
                        //
                        VisitCnt = 0;
                        HitCnt = 0;
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
                            + " and(v.dateadded>=" + DbController.encodeSQLDate(DateStart) + ")"
                            + " and (v.dateadded<" + DbController.encodeSQLDate(DateEnd) + ")"
                            + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                            + "";
                        using (var csXfer = new CsModel(core)) {
                            csXfer.csOpenSql(SQL, "Default");
                            if (csXfer.csOk()) {
                                NoCookieVisits = csXfer.csGetInteger("NoCookieVisits");
                            }
                        }
                        //
                        // Total Visits
                        //
                        SQL = "select count(v.id) as VisitCnt ,Sum(v.PageVisits) as HitCnt ,sum(v.TimetoLastHit) as TimeOnSite"
                            + " from ccvisits v"
                            + " where (v.CookieSupport<>0)"
                            + " and(v.dateadded>=" + DbController.encodeSQLDate(DateStart) + ")"
                            + " and (v.dateadded<" + DbController.encodeSQLDate(DateEnd) + ")"
                            + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                            + "";
                        using (var csXfer = new CsModel(core)) {
                            csXfer.csOpenSql(SQL, "Default");
                            if (csXfer.csOk()) {
                                VisitCnt = csXfer.csGetInteger("VisitCnt");
                                HitCnt = csXfer.csGetInteger("HitCnt");
                                TimeOnSite = csXfer.csGetNumber("TimeOnSite");
                            }
                        }
                        //
                        // Visits by new visitors
                        //
                        if (VisitCnt > 0) {
                            SQL = "select count(v.id) as NewVisitorVisits"
                                + " from ccvisits v"
                                + " where (v.CookieSupport<>0)"
                                + " and(v.dateadded>=" + DbController.encodeSQLDate(DateStart) + ")"
                                + " and (v.dateadded<" + DbController.encodeSQLDate(DateEnd) + ")"
                                + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                                + " and(v.VisitorNew<>0)"
                                + "";
                            using (var csXfer = new CsModel(core)) {
                                csXfer.csOpenSql(SQL, "Default");
                                if (csXfer.csOk()) {
                                    NewVisitorVisits = csXfer.csGetInteger("NewVisitorVisits");
                                }
                            }
                            //
                            // Single Page Visits
                            //
                            SQL = "select count(v.id) as SinglePageVisits"
                                + " from ccvisits v"
                                + " where (v.CookieSupport<>0)"
                                + " and(v.dateadded>=" + DbController.encodeSQLDate(DateStart) + ")"
                                + " and (v.dateadded<" + DbController.encodeSQLDate(DateEnd) + ")"
                                + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                                + " and(v.PageVisits=1)"
                                + "";
                            using (var csXfer = new CsModel(core)) {
                                csXfer.csOpenSql(SQL, "Default");
                                if (csXfer.csOk()) {
                                    SinglePageVisits = csXfer.csGetInteger("SinglePageVisits");
                                }
                            }
                            //
                            // Multipage Visits
                            //
                            SQL = "select count(v.id) as VisitCnt ,sum(v.PageVisits) as HitCnt ,sum(v.TimetoLastHit) as TimetoLastHitSum "
                                + " from ccvisits v"
                                + " where (v.CookieSupport<>0)"
                                + " and(v.dateadded>=" + DbController.encodeSQLDate(DateStart) + ")"
                                + " and (v.dateadded<" + DbController.encodeSQLDate(DateEnd) + ")"
                                + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                                + " and(PageVisits>1)"
                                + "";
                            using (var csXfer = new CsModel(core)) {
                                csXfer.csOpenSql(SQL, "Default");
                                if (csXfer.csOk()) {
                                    MultiPageVisitCnt = csXfer.csGetInteger("VisitCnt");
                                    MultiPageHitCnt = csXfer.csGetInteger("HitCnt");
                                    MultiPageTimetoLastHitSum = csXfer.csGetNumber("TimetoLastHitSum");
                                }
                            }
                            //
                            // Authenticated Visits
                            //
                            SQL = "select count(v.id) as AuthenticatedVisits "
                                + " from ccvisits v"
                                + " where (v.CookieSupport<>0)"
                                + " and(v.dateadded>=" + DbController.encodeSQLDate(DateStart) + ")"
                                + " and (v.dateadded<" + DbController.encodeSQLDate(DateEnd) + ")"
                                + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                                + " and(VisitAuthenticated<>0)"
                                + "";
                            using (var csXfer = new CsModel(core)) {
                                csXfer.csOpenSql(SQL, "Default");
                                if (csXfer.csOk()) {
                                    AuthenticatedVisits = csXfer.csGetInteger("AuthenticatedVisits");
                                }
                            }
                            // 
                            //
                            // Mobile Visits
                            //
                            SQL = "select count(v.id) as cnt "
                                + " from ccvisits v"
                                + " where (v.CookieSupport<>0)"
                                + " and(v.dateadded>=" + DbController.encodeSQLDate(DateStart) + ")"
                                + " and (v.dateadded<" + DbController.encodeSQLDate(DateEnd) + ")"
                                + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                                + " and(Mobile<>0)"
                                + "";
                            using (var csXfer = new CsModel(core)) {
                                csXfer.csOpenSql(SQL, "Default");
                                if (csXfer.csOk()) {
                                    MobileVisits = csXfer.csGetInteger("cnt");
                                }
                            }
                            //
                            // Bot Visits
                            //
                            SQL = "select count(v.id) as cnt "
                                + " from ccvisits v"
                                + " where (v.CookieSupport<>0)"
                                + " and(v.dateadded>=" + DbController.encodeSQLDate(DateStart) + ")"
                                + " and (v.dateadded<" + DbController.encodeSQLDate(DateEnd) + ")"
                                + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                                + " and(Bot<>0)"
                                + "";
                            using (var csXfer = new CsModel(core)) {
                                csXfer.csOpenSql(SQL, "Default");
                                if (csXfer.csOk()) {
                                    BotVisits = csXfer.csGetInteger("cnt");
                                }
                            }
                            //
                            if ((MultiPageHitCnt > MultiPageVisitCnt) && (HitCnt > 0)) {
                                AveReadTime = encodeInteger(MultiPageTimetoLastHitSum / (MultiPageHitCnt - MultiPageVisitCnt));
                                TotalTimeOnSite = MultiPageTimetoLastHitSum + (AveReadTime * VisitCnt);
                                AveTimeOnSite = TotalTimeOnSite / VisitCnt;
                            }
                        }
                        //
                        // Add or update the Visit Summary Record
                        //
                        using (var csXfer = new CsModel(core)) {
                            csXfer.csOpen("Visit Summary", "(timeduration=" + HourDuration + ")and(DateNumber=" + DateNumber + ")and(TimeNumber=" + TimeNumber + ")");
                            if (!csXfer.csOk()) {
                                csXfer.close();
                                csXfer.insert("Visit Summary");
                            }
                            //
                            if (csXfer.csOk()) {
                                csXfer.csSet("name", HourDuration + " hr summary for " + DateTime.FromOADate(DateNumber).ToShortDateString() + " " + TimeNumber + ":00");
                                csXfer.csSet("DateNumber", DateNumber);
                                csXfer.csSet("TimeNumber", TimeNumber);
                                csXfer.csSet("Visits", VisitCnt);
                                csXfer.csSet("PagesViewed", HitCnt);
                                csXfer.csSet("TimeDuration", HourDuration);
                                csXfer.csSet("NewVisitorVisits", NewVisitorVisits);
                                csXfer.csSet("SinglePageVisits", SinglePageVisits);
                                csXfer.csSet("AuthenticatedVisits", AuthenticatedVisits);
                                csXfer.csSet("NoCookieVisits", NoCookieVisits);
                                csXfer.csSet("AveTimeOnSite", AveTimeOnSite);
                                if (true) {
                                    csXfer.csSet("MobileVisits", MobileVisits);
                                    csXfer.csSet("BotVisits", BotVisits);
                                }
                            }
                        }
                        PeriodDatePtr = PeriodDatePtr.AddHours(HourDuration);
                    }
                }
                //
                return;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //====================================================================================================
        //
        public void logHousekeeping(CoreController core, string LogCopy) {
            LogController.logInfo(core, "housekeeping: " + LogCopy);
        }
        //
        //====================================================================================================
        //
        private void HouseKeep_App_Daily_LogFolder(CoreController core, string FolderName, DateTime LastMonth) {
            try {
                logHousekeeping(core, "Deleting files from folder [" + FolderName + "] older than " + LastMonth);
                List<CPFileSystemBaseClass.FileDetail> FileList = core.privateFiles.getFileList(FolderName);
                foreach (CPFileSystemBaseClass.FileDetail file in FileList) {
                    if (file.DateCreated < LastMonth) {
                        core.privateFiles.deleteFile(FolderName + "/" + file.Name);
                    }
                }
                return;
                //
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //====================================================================================================
        //
        private bool DownloadUpdates(CoreController core) {
            bool loadOK = true;
            try {
                XmlDocument Doc = null;
                string URL = null;
                string Copy = null;
                //
                Doc = new XmlDocument();
                URL = "http://support.contensive.com/GetUpdates?iv=" + core.codeVersion();
                loadOK = true;
                Doc.Load(URL);
                if ((Doc.DocumentElement.Name.ToLowerInvariant() == GenericController.vbLCase("ContensiveUpdate")) && (Doc.DocumentElement.ChildNodes.Count != 0)) {
                    foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                        Copy = CDefSection.InnerText;
                        switch (GenericController.vbLCase(CDefSection.Name)) {
                            case "mastervisitnamelist":
                                //
                                // Read in the interfaces and save to Add-ons
                                core.privateFiles.saveFile("config\\VisitNameList.txt", Copy);
                                break;
                            case "masteremailbouncefilters":
                                //
                                // save the updated filters file
                                core.privateFiles.saveFile("config\\EmailBounceFilters.txt", Copy);
                                break;
                            case "mastermobilebrowserlist":
                                //
                                // save the updated filters file
                                //
                                core.privateFiles.saveFile("config\\MobileBrowserList.txt", Copy);
                                break;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return loadOK;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Summarize the page views, excludes non-cookie visits, excludes administrator and developer visits, excludes authenticated users with ExcludeFromReporting
        /// </summary>
        /// <param name="core"></param>
        /// <param name="StartTimeDate"></param>
        /// <param name="EndTimeDate"></param>
        /// <param name="HourDuration"></param>
        /// <param name="BuildVersion"></param>
        /// <param name="OldestVisitSummaryWeCareAbout"></param>
        //
        public void houseKeep_PageViewSummary(CoreController core, DateTime StartTimeDate, DateTime EndTimeDate, int HourDuration, string BuildVersion, DateTime OldestVisitSummaryWeCareAbout) {
            int hint = 0;
            string hinttxt = "";
            try {
                XmlDocument LibraryCollections = new XmlDocument();
                XmlDocument LocalCollections = new XmlDocument();
                XmlDocument Doc = new XmlDocument();
                if (string.CompareOrdinal(BuildVersion, core.codeVersion()) < 0) {
                    LogController.handleError(core, new GenericException("Can not summarize analytics until this site's data needs been upgraded."));
                } else {
                    hint = 1;
                    DateTime PeriodStart = default(DateTime);
                    PeriodStart = StartTimeDate;
                    if (PeriodStart < OldestVisitSummaryWeCareAbout) {
                        PeriodStart = OldestVisitSummaryWeCareAbout;
                    }
                    DateTime PeriodDatePtr = default(DateTime);
                    PeriodDatePtr = PeriodStart.Date;
                    double PeriodStep = (double)HourDuration / 24.0F;
                    while (PeriodDatePtr < EndTimeDate) {
                        hint = 2;
                        //
                        hinttxt = ", HourDuration [" + HourDuration + "], PeriodDatePtr [" + PeriodDatePtr + "], PeriodDatePtr.AddHours(HourDuration / 2.0) [" + PeriodDatePtr.AddHours(HourDuration / 2.0) + "]";
                        int DateNumber = encodeInteger(PeriodDatePtr.AddHours(HourDuration / 2.0).ToOADate());
                        int TimeNumber = encodeInteger(PeriodDatePtr.TimeOfDay.TotalHours);
                        DateTime DateStart = default(DateTime);
                        DateStart = PeriodDatePtr.Date;
                        DateTime DateEnd = default(DateTime);
                        DateEnd = PeriodDatePtr.AddHours(HourDuration).Date;
                        string PageTitle = "";
                        int PageID = 0;
                        int PageViews = 0;
                        int AuthenticatedPageViews = 0;
                        int MobilePageViews = 0;
                        int BotPageViews = 0;
                        int NoCookiePageViews = 0;
                        //
                        // Loop through all the pages hit during this period
                        //
                        //
                        // for now ignore the problem caused by addons like Blogs creating multiple 'pages' within on pageid
                        // One way to handle this is to expect the addon to set something unquie in he page title
                        // then use the title to distinguish a page. The problem with this is the current system puts the
                        // visit number and page number in the name. if we select on district name, they will all be.
                        //
                        using (var csPages = new CsModel(core)) {
                            string sql = "select distinct recordid,pagetitle from ccviewings h"
                                + " where (h.recordid<>0)"
                                + " and(h.dateadded>=" + DbController.encodeSQLDate(DateStart) + ")"
                                + " and (h.dateadded<" + DbController.encodeSQLDate(DateEnd) + ")"
                                + " and((h.ExcludeFromAnalytics is null)or(h.ExcludeFromAnalytics=0))"
                                + "order by recordid";
                            hint = 3;
                            if (!csPages.csOpenSql(sql)) {
                                //
                                // no hits found - add or update a single record for this day so we know it has been calculated
                                csPages.csOpen("Page View Summary", "(timeduration=" + HourDuration + ")and(DateNumber=" + DateNumber + ")and(TimeNumber=" + TimeNumber + ")and(pageid=" + PageID + ")and(pagetitle=" + DbController.encodeSQLText(PageTitle) + ")");
                                if (!csPages.csOk()) {
                                    csPages.close();
                                    csPages.insert("Page View Summary");
                                }
                                //
                                if (csPages.csOk()) {
                                    csPages.csSet("name", HourDuration + " hr summary for " + DateTime.FromOADate((double)DateNumber) + " " + TimeNumber + ":00, " + PageTitle);
                                    csPages.csSet("DateNumber", DateNumber);
                                    csPages.csSet("TimeNumber", TimeNumber);
                                    csPages.csSet("TimeDuration", HourDuration);
                                    csPages.csSet("PageViews", PageViews);
                                    csPages.csSet("PageID", PageID);
                                    csPages.csSet("PageTitle", PageTitle);
                                    csPages.csSet("AuthenticatedPageViews", AuthenticatedPageViews);
                                    csPages.csSet("NoCookiePageViews", NoCookiePageViews);
                                    if (true) {
                                        csPages.csSet("MobilePageViews", MobilePageViews);
                                        csPages.csSet("BotPageViews", BotPageViews);
                                    }
                                }
                                csPages.close();
                                hint = 4;
                            } else {
                                hint = 5;
                                //
                                // add an entry for each page hit on this day
                                //
                                while (csPages.csOk()) {
                                    PageID = csPages.csGetInteger("recordid");
                                    PageTitle = csPages.csGetText("pagetitle");
                                    string baseCriteria = ""
                                        + " (h.recordid=" + PageID + ")"
                                        + " "
                                        + " and(h.dateadded>=" + DbController.encodeSQLDate(DateStart) + ")"
                                        + " and(h.dateadded<" + DbController.encodeSQLDate(DateEnd) + ")"
                                        + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                                        + " and((h.ExcludeFromAnalytics is null)or(h.ExcludeFromAnalytics=0))"
                                        + "";
                                    if (!string.IsNullOrEmpty(PageTitle)) {
                                        baseCriteria = baseCriteria + "and(h.pagetitle=" + DbController.encodeSQLText(PageTitle) + ")";
                                    }
                                    hint = 6;
                                    //
                                    // Total Page Views
                                    using (var csPageViews = new CsModel(core)) {
                                        sql = "select count(h.id) as cnt"
                                            + " from ccviewings h left join ccvisits v on h.visitid=v.id"
                                            + " where " + baseCriteria + " and (v.CookieSupport<>0)"
                                            + "";
                                        csPageViews.csOpenSql(sql);
                                        if (csPageViews.csOk()) {
                                            PageViews = csPageViews.csGetInteger("cnt");
                                        }
                                    }
                                    //
                                    // Authenticated Visits
                                    //
                                    using (var csAuthPages = new CsModel(core)) {
                                        sql = "select count(h.id) as cnt"
                                            + " from ccviewings h left join ccvisits v on h.visitid=v.id"
                                            + " where " + baseCriteria + " and(v.CookieSupport<>0)"
                                            + " and(v.visitAuthenticated<>0)"
                                            + "";
                                        csAuthPages.csOpenSql(sql, "Default");
                                        if (csAuthPages.csOk()) {
                                            AuthenticatedPageViews = csAuthPages.csGetInteger("cnt");
                                        }
                                    }
                                    //
                                    // No Cookie Page Views
                                    //
                                    using (var csNoCookie = new CsModel(core)) {
                                        sql = "select count(h.id) as NoCookiePageViews"
                                            + " from ccviewings h left join ccvisits v on h.visitid=v.id"
                                            + " where " + baseCriteria + " and((v.CookieSupport=0)or(v.CookieSupport is null))"
                                            + "";
                                        csNoCookie.csOpenSql(sql, "Default");
                                        if (csNoCookie.csOk()) {
                                            NoCookiePageViews = csNoCookie.csGetInteger("NoCookiePageViews");
                                        }
                                    }
                                    //
                                    //
                                    // Mobile Visits
                                    using (var csMobileVisits = new CsModel(core)) {
                                        sql = "select count(h.id) as cnt"
                                            + " from ccviewings h left join ccvisits v on h.visitid=v.id"
                                            + " where " + baseCriteria + " and(v.CookieSupport<>0)"
                                            + " and(v.mobile<>0)"
                                            + "";
                                        csMobileVisits.csOpenSql(sql);
                                        if (csMobileVisits.csOk()) {
                                            MobilePageViews = csMobileVisits.csGetInteger("cnt");
                                        }
                                    }
                                    //
                                    // Bot Visits
                                    using (var csBotVisits = new CsModel(core)) {
                                        sql = "select count(h.id) as cnt"
                                            + " from ccviewings h left join ccvisits v on h.visitid=v.id"
                                            + " where " + baseCriteria + " and(v.CookieSupport<>0)"
                                            + " and(v.bot<>0)"
                                            + "";
                                        csBotVisits.csOpenSql(sql);
                                        if (csBotVisits.csOk()) {
                                            BotPageViews = csBotVisits.csGetInteger("cnt");
                                        }
                                    }
                                    //
                                    // Add or update the Visit Summary Record
                                    //
                                    using (var csPVS = new CsModel(core)) {
                                        if (!csPVS.csOpen("Page View Summary", "(timeduration=" + HourDuration + ")and(DateNumber=" + DateNumber + ")and(TimeNumber=" + TimeNumber + ")and(pageid=" + PageID + ")and(pagetitle=" + DbController.encodeSQLText(PageTitle) + ")")) {
                                            csPVS.insert("Page View Summary");
                                        }
                                        //
                                        if (csPVS.csOk()) {
                                            hint = 11;
                                            string PageName = "";
                                            if (string.IsNullOrEmpty(PageTitle)) {
                                                PageName = MetaController.getRecordName(core, "page content", PageID);
                                                csPVS.csSet("name", HourDuration + " hr summary for " + DateTime.FromOADate((double)DateNumber) + " " + TimeNumber + ":00, " + PageName);
                                                csPVS.csSet("PageTitle", PageName);
                                            } else {
                                                csPVS.csSet("name", HourDuration + " hr summary for " + DateTime.FromOADate((double)DateNumber) + " " + TimeNumber + ":00, " + PageTitle);
                                                csPVS.csSet("PageTitle", PageTitle);
                                            }
                                            csPVS.csSet("DateNumber", DateNumber);
                                            csPVS.csSet("TimeNumber", TimeNumber);
                                            csPVS.csSet("TimeDuration", HourDuration);
                                            csPVS.csSet("PageViews", PageViews);
                                            csPVS.csSet("PageID", PageID);
                                            csPVS.csSet("AuthenticatedPageViews", AuthenticatedPageViews);
                                            csPVS.csSet("NoCookiePageViews", NoCookiePageViews);
                                            hint = 12;
                                            if (true) {
                                                csPVS.csSet("MobilePageViews", MobilePageViews);
                                                csPVS.csSet("BotPageViews", BotPageViews);
                                            }
                                        }
                                    }
                                    csPages.csGoNext();
                                }
                            }
                        }
                        PeriodDatePtr = PeriodDatePtr.AddHours(HourDuration);
                    }
                }
                //
                return;
            } catch (Exception ex) {
                LogController.handleError(core, ex, "hint [" + hint + "]");
            }
        }
        //
        //====================================================================================================
        //
        public void housekeepAddonFolder(CoreController core) {
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
                string LocalGuid = null;
                XmlDocument Doc = new XmlDocument();
                int FolderPtr = 0;
                string CollectionPath = null;
                DateTime LastChangeDate = default(DateTime);
                string hint = "";
                string LocalName = null;
                int Ptr = 0;
                string collectionFileFilename = null;
                //
                logHousekeeping(core, "Entering RegisterAddonFolder");
                //
                bool loadOK = true;
                try {
                    collectionFileFilename = core.addon.getPrivateFilesAddonPath() + "Collections.xml";
                    string collectionFileContent = core.privateFiles.readFileText(collectionFileFilename);
                    Doc.LoadXml(collectionFileContent);
                } catch (Exception) {
                    logHousekeeping(core, "RegisterAddonFolder, Hint=[" + hint + "], Error loading Collections.xml file.");
                    loadOK = false;
                }
                if (loadOK) {
                    //
                    logHousekeeping(core, "Collection.xml loaded ok");
                    //
                    if (GenericController.vbLCase(Doc.DocumentElement.Name) != GenericController.vbLCase(CollectionListRootNode)) {
                        logHousekeeping(core, "RegisterAddonFolder, Hint=[" + hint + "], The Collections.xml file has an invalid root node, [" + Doc.DocumentElement.Name + "] was received and [" + CollectionListRootNode + "] was expected.");
                    } else {
                        //
                        logHousekeeping(core, "Collection.xml root name ok");
                        //
                        if (true) {
                            //If genericController.vbLCase(.name) <> "collectionlist" Then
                            //    Call AppendClassLog(core,"Server", "", "RegisterAddonFolder, basename was not collectionlist, [" & .name & "].")
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
                                switch (GenericController.vbLCase(LocalListNode.Name)) {
                                    case "collection":
                                        LocalGuid = "";
                                        foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                            switch (GenericController.vbLCase(CollectionNode.Name)) {
                                                case "name":
                                                    //
                                                    LocalName = GenericController.vbLCase(CollectionNode.InnerText);
                                                    break;
                                                case "guid":
                                                    //
                                                    LocalGuid = GenericController.vbLCase(CollectionNode.InnerText);
                                                    break;
                                                case "path":
                                                    //
                                                    CollectionPath = GenericController.vbLCase(CollectionNode.InnerText);
                                                    break;
                                                case "lastchangedate":
                                                    LastChangeDate = GenericController.encodeDate(CollectionNode.InnerText);
                                                    break;
                                            }
                                        }
                                        break;
                                }
                                //
                                logHousekeeping(core, "Node[" + NodeCnt + "], LocalName=[" + LocalName + "], LastChangeDate=[" + LastChangeDate + "], CollectionPath=[" + CollectionPath + "], LocalGuid=[" + LocalGuid + "]");
                                //
                                // Go through all subpaths of the collection path, register the version match, unregister all others
                                //
                                //fs = New fileSystemClass
                                if (string.IsNullOrEmpty(CollectionPath)) {
                                    //
                                    logHousekeeping(core, "no collection path, skipping");
                                    //
                                } else {
                                    CollectionPath = GenericController.vbLCase(CollectionPath);
                                    CollectionRootPath = CollectionPath;
                                    Pos = CollectionRootPath.LastIndexOf("\\") + 1;
                                    if (Pos <= 0) {
                                        //
                                        logHousekeeping(core, "CollectionPath has no '\\', skipping");
                                        //
                                    } else {
                                        CollectionRootPath = CollectionRootPath.Left(Pos - 1);
                                        Path = core.addon.getPrivateFilesAddonPath() + "\\" + CollectionRootPath + "\\";
                                        List<FolderDetail> FolderList = new List<FolderDetail>();
                                        if (core.privateFiles.pathExists(Path)) {
                                            FolderList = core.privateFiles.getFolderList(Path);
                                        }
                                        if (FolderList.Count == 0) {
                                            //
                                            logHousekeeping(core, "no subfolders found in physical path [" + Path + "], skipping");
                                            //
                                        } else {
                                            foreach (FolderDetail dir in FolderList) {
                                                IsActiveFolder = false;
                                                //
                                                // register or unregister all files in this folder
                                                //
                                                if (string.IsNullOrEmpty(dir.Name)) {
                                                    //
                                                    logHousekeeping(core, "....empty folder [" + dir.Name + "], skipping");
                                                    //
                                                } else {
                                                    //
                                                    logHousekeeping(core, "....Folder [" + dir.Name + "]");
                                                    IsActiveFolder = (CollectionRootPath + "\\" + dir.Name == CollectionPath);
                                                    if (IsActiveFolder && (FolderPtr != (FolderList.Count - 1))) {
                                                        //
                                                        // This one is active, but not the last
                                                        //
                                                        logHousekeeping(core, "....Active addon is not the most current, this folder is the active folder, but there are more recent folders. This folder will be preserved.");
                                                    }
                                                    // 20161005 - no longer need to register activeX
                                                    //FileList = core.app.privateFiles.GetFolderFiles(Path & "\" & dir.Name)
                                                    //For Each file As FileInfo In FileList
                                                    //    If Right(file.Name, 4) = ".dll" Then
                                                    //        If IsActiveFolder Then
                                                    //            '
                                                    //            ' register this file
                                                    //            '
                                                    //            RegisterPathList = RegisterPathList & vbCrLf & Path & dir.Name & "\" & file.Name
                                                    //            '                                                                Cmd = "%comspec% /c regsvr32 """ & RegisterPathList & """ /s"
                                                    //            '                                                                Call AppendClassLog(core,"Server", "RegisterAddonFolder", "....Regsiter DLL [" & Cmd & "]")
                                                    //            '                                                                Call runProcess(core,Cmd, , True)
                                                    //        Else
                                                    //            '
                                                    //            ' unregister this file
                                                    //            '
                                                    //            Cmd = "%comspec% /c regsvr32 /u """ & Path & dir.Name & "\" & file.Name & """ /s"
                                                    //            Call AppendClassLog(core,"Server", "RegisterAddonFolder", "....Unregsiter DLL [" & Cmd & "]")
                                                    //            Call runProcess(core, Cmd, , True)
                                                    //        End If
                                                    //    End If
                                                    //Next
                                                    //
                                                    // only keep last two non-matching folders and the active folder
                                                    //
                                                    if (IsActiveFolder) {
                                                        //IsActiveFolder = IsActiveFolder;
                                                    } else {
                                                        if (FolderPtr < (FolderList.Count - 3)) {
                                                            logHousekeeping(core, "....Deleting path because non-active and not one of the newest 2 [" + Path + dir.Name + "]");
                                                            core.privateFiles.deleteFolder(Path + dir.Name);
                                                        }
                                                    }
                                                }
                                            }
                                            //
                                            // register files found in the active folder last
                                            //
                                            if (!string.IsNullOrEmpty(RegisterPathList)) {
                                                RegisterPaths = GenericController.stringSplit(RegisterPathList, "\r\n");
                                                for (Ptr = 0; Ptr <= RegisterPaths.GetUpperBound(0); Ptr++) {
                                                    RegisterPath = RegisterPaths[Ptr].Trim(' ');
                                                    if (!string.IsNullOrEmpty(RegisterPath)) {
                                                        Cmd = "%comspec% /c regsvr32 \"" + RegisterPath + "\" /s";
                                                        logHousekeeping(core, "....Register DLL [" + Cmd + "]");
                                                        runProcess(core, Cmd, "", true);
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
                            //            Call AppendClassLog(core,"Server", "RegisterAddonFolder", "....Register DLL [" & Cmd & "]")
                            //            Call runProcess(core, Cmd, , True)
                            //        End If
                            //    Next
                            //End If
                        }
                    }
                }
                //
                logHousekeeping(core, "Exiting RegisterAddonFolder");
            } catch (Exception ex) {
                throw new GenericException("Unexpected Exception", ex);
            }
        }
        //
        //====================================================================================================
        //
        public void housekeep_userProperties(CoreController core) {
            string sqlInner = "select p.id from ccProperties p left join ccmembers m on m.id=p.KeyID where (p.TypeID=" + PropertyModelClass.PropertyTypeEnum.user + ") and (m.ID is null)";
            string sql = "delete from ccProperties where id in (" + sqlInner + ")";
            core.db.executeNonQueryAsync(sql);
        }
        //
        //====================================================================================================
        //
        public void housekeep_visitProperties(CoreController core) {
            string sqlInner = "select p.id from ccProperties p left join ccvisits m on m.id=p.KeyID where (p.TypeID=" + PropertyModelClass.PropertyTypeEnum.visit + ") and (m.ID is null)";
            string sql = "delete from ccProperties where id in (" + sqlInner + ")";
            core.db.executeNonQueryAsync(sql);
        }
        //
        //====================================================================================================
        //
        public void housekeep_visitorProperties(CoreController core) {
            string sqlInner = "select p.id from ccProperties p left join ccvisitors m on m.id=p.KeyID where (p.TypeID=" + PropertyModelClass.PropertyTypeEnum.visitor + ") and (m.ID is null)";
            string sql = "delete from ccProperties where id in (" + sqlInner + ")";
            core.db.executeNonQueryAsync(sql);
        }
    }
}
