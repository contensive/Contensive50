
using System;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;

namespace Contensive.Processor.Addons.Housekeeping {
    //
    public static class VisitClass {
        public static void housekeep( CoreController core, HouseKeepEnvironmentModel env ) {
            try {
                //
                // Visits with no DateAdded
                //
                LogController.logInfo(core, "Deleting visits with no DateAdded");
                core.db.deleteTableRecordChunks("ccvisits", "(DateAdded is null)or(DateAdded<=" + DbController.encodeSQLDate(new DateTime(1995, 1, 1)) + ")", 1000, 10000);
                //
                // Visits with no visitor
                //
                LogController.logInfo(core, "Deleting visits with no visitor");
                core.db.deleteTableRecordChunks("ccvisits", "(VisitorID is null)or(VisitorID=0)", 1000, 10000);
                //
                // delete nocookie visits
                // This must happen after the housekeep summarizing, and no sooner then 48 hours ago so all hits have been summarized before deleting
                //
                if (env.archiveDeleteNoCookie) {
                    //
                    // delete visits from the non-cookie visits
                    //
                    LogController.logInfo(core, "Deleting visits with no cookie support older than Midnight, Two Days Ago");
                    core.db.deleteTableRecordChunks("ccvisits", "(CookieSupport=0)and(LastVisitTime<" + env.sqlDateMidnightTwoDaysAgo + ")", 1000, 10000);
                }
                DateTime OldestVisitDate = default(DateTime);
                //
                // Get Oldest Visit
                using (var csData = new CsModel(core)) {
                    if (csData.openSql(core.db.getSQLSelect("ccVisits", "DateAdded", "", "dateadded", "", 1))) {
                        OldestVisitDate = csData.getDate("DateAdded").Date;
                    }
                }
                //
                // Remove old visit records
                //   if > 30 days in visit table, limit one pass to just 30 days
                //   this is to prevent the entire server from being bogged down for one site change
                //
                if (OldestVisitDate == DateTime.MinValue) {
                    LogController.logInfo(core, "No records were removed because no visit records were found while requesting the oldest visit.");
                } else if (env.visitArchiveAgeDays <= 0) {
                    LogController.logInfo(core, "No records were removed because Housekeep ArchiveRecordAgeDays is 0.");
                } else {
                    DateTime ArchiveDate = core.dateTimeNowMockable.AddDays(-env.visitArchiveAgeDays).Date;
                    int DaystoRemove = encodeInteger(ArchiveDate.Subtract(OldestVisitDate).TotalDays);
                    if (DaystoRemove > 30) {
                        ArchiveDate = OldestVisitDate.AddDays(30);
                    }
                    if (OldestVisitDate >= ArchiveDate) {
                        LogController.logInfo(core, "No records were removed because Oldest Visit Date [" + OldestVisitDate + "] >= ArchiveDate [" + ArchiveDate + "].");
                    } else {
                        LogController.logInfo(core, "Removing records from [" + OldestVisitDate + "] to [" + ArchiveDate + "].");
                        DateTime SingleDate = default(DateTime);
                        SingleDate = OldestVisitDate;
                        do {
                            houseKeep_App_Daily_RemoveVisitRecords(core, SingleDate);
                            SingleDate = SingleDate.AddDays(1);
                        } while (SingleDate < ArchiveDate);
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }

        //====================================================================================================
        //
        public static void houseKeep_App_Daily_RemoveVisitRecords(CoreController core, DateTime DeleteBeforeDate) {
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
                SQLTablePeople = MetadataController.getContentTablename(core, "People");
                //
                appName = core.appConfig.name;
                DeleteBeforeDateSQL = DbController.encodeSQLDate(DeleteBeforeDate);
                //
                // Visits older then archive age
                //
                LogController.logInfo(core, "Deleting visits before [" + DeleteBeforeDateSQL + "]");
                core.db.deleteTableRecordChunks("ccVisits", "(DateAdded<" + DeleteBeforeDateSQL + ")", 1000, 10000);
                //
                // Viewings with visits before the first
                //
                LogController.logInfo(core, "Deleting viewings with visitIDs lower then the lowest ccVisits.ID");
                core.db.deleteTableRecordChunks("ccviewings", "(visitid<(select min(ID) from ccvisits))", 1000, 10000);
                //
                // Visitors with no visits
                //
                LogController.logInfo(core, "Deleting visitors with no visits");
                SQL = "delete ccVisitors"
                    + " from ccVisitors Left Join ccVisits on ccVisits.VisitorID=ccVisitors.ID"
                    + " where ccVisits.ID is null";
                core.db.executeNonQuery(SQL);
                //
                // restore sved timeout
                //
                core.db.sqlCommandTimeout = TimeoutSave;
                return;
                //
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}