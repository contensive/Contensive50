
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
    //
    //====================================================================================================
    /// <summary>
    /// housekeep environment, to facilitate argument passing
    /// </summary>
    public class HouseKeepEnvironmentModel {
        public bool force;
        public bool RunDailyTasks;
        public DateTime rightNow;
        public DateTime LastCheckDateTime;
        public int ServerHousekeepHour;
        public DateTime Yesterday;
        public DateTime ALittleWhileAgo;
        public string SQLNow;
        public DateTime OldestVisitSummaryWeCareAbout;
        public int VisitArchiveAgeDays;
        public DateTime VisitArchiveDate;
        public string defaultMemberName;
        public int GuestArchiveAgeDays;
        public int EmailDropArchiveAgeDays;
        public string DefaultMemberName;
        public bool ArchiveDeleteNoCookie;
        public string SQLDateMidnightTwoDaysAgo;
        public DateTime MidnightTwoDaysAgo;
        public DateTime thirtyDaysAgo;
        public bool archiveAlarm;
        //
        public HouseKeepEnvironmentModel(CoreController core) {
            try {
                archiveAlarm = false;
                rightNow = DateTime.Now;
                LastCheckDateTime = core.siteProperties.getDate("housekeep, last check", default(DateTime));
                core.siteProperties.setProperty("housekeep, last check", rightNow);
                force = core.docProperties.getBoolean("force");
                ServerHousekeepHour = core.siteProperties.getInteger("housekeep, run time hour", 2); ;
                RunDailyTasks = ((rightNow.Date > LastCheckDateTime.Date) && (ServerHousekeepHour < rightNow.Hour));
                Yesterday = rightNow.AddDays(-1).Date;
                ALittleWhileAgo = rightNow.AddDays(-90).Date;
                SQLNow = DbController.encodeSQLDate(rightNow);
                DefaultMemberName = ContentFieldMetadataModel.getDefaultValue(core, "people", "name");
                ArchiveDeleteNoCookie = core.siteProperties.getBoolean("ArchiveDeleteNoCookie", true);
                SQLDateMidnightTwoDaysAgo = DbController.encodeSQLDate(MidnightTwoDaysAgo);
                Yesterday = rightNow.AddDays(-1).Date;
                MidnightTwoDaysAgo = rightNow.AddDays(-2).Date;
                thirtyDaysAgo = rightNow.AddDays(-30).Date;
                //
                // -- Get ArchiveAgeDays - use this as the oldest data they care about
                VisitArchiveAgeDays = GenericController.encodeInteger(core.siteProperties.getText("ArchiveRecordAgeDays", "365"));
                if (VisitArchiveAgeDays < 2) {
                    VisitArchiveAgeDays = 2;
                    core.siteProperties.setProperty("ArchiveRecordAgeDays", "2");
                }
                VisitArchiveDate = rightNow.AddDays(-VisitArchiveAgeDays).Date;
                OldestVisitSummaryWeCareAbout = DateTime.Now.Date.AddDays(-120);
                if (OldestVisitSummaryWeCareAbout < VisitArchiveDate) {
                    OldestVisitSummaryWeCareAbout = VisitArchiveDate;
                }
                //
                // -- Get GuestArchiveAgeDays
                GuestArchiveAgeDays = GenericController.encodeInteger(core.siteProperties.getText("ArchivePeopleAgeDays", "2"));
                if (GuestArchiveAgeDays < 2) {
                    GuestArchiveAgeDays = 2;
                    core.siteProperties.setProperty("ArchivePeopleAgeDays", GuestArchiveAgeDays.ToString());
                }
                //
                // -- Get EmailDropArchiveAgeDays
                EmailDropArchiveAgeDays = GenericController.encodeInteger(core.siteProperties.getText("ArchiveEmailDropAgeDays", "90"));
                if (EmailDropArchiveAgeDays < 2) {
                    EmailDropArchiveAgeDays = 2;
                    core.siteProperties.setProperty("ArchiveEmailDropAgeDays", EmailDropArchiveAgeDays.ToString());
                }
                defaultMemberName = ContentFieldMetadataModel.getDefaultValue(core, "people", "name");
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
                double minutesSinceMidnight = rightNow.TimeOfDay.TotalMinutes;
                double LastCheckMinutesFromMidnight = LastCheckDateTime.TimeOfDay.TotalMinutes;
                if ((minutesSinceMidnight > LastCheckMinutesFromMidnight) && (LastCheckMinutesFromMidnight < minutesSinceMidnight)) {
                    //
                    // Same Day - Midnight is before last and after current
                    //
                    archiveAlarm = true;
                } else if ((LastCheckMinutesFromMidnight > minutesSinceMidnight) && ((LastCheckMinutesFromMidnight < minutesSinceMidnight))) {
                    //
                    // New Day - Midnight is between Last and Set
                    //
                    archiveAlarm = true;
                }

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}