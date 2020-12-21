
using Contensive.Processor.Controllers;
using System;

namespace Contensive.Processor.Addons.Housekeeping {
    /// <summary>
    /// Housekeep this content
    /// </summary>
    public static class VisitorClass {
        //
        //====================================================================================================
        /// <summary>
        /// execute hourly tasks
        /// </summary>
        /// <param name="core"></param>
        public static void executeHourlyTasks(CoreController core) {
            try {
                //
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute Daily Tasks
        /// </summary>
        /// <param name="core"></param>
        /// <param name="env"></param>
        public static void executeDailyTasks(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                LogController.logInfo(core, "Housekeep, visitors");
                {
                    //
                    LogController.logInfo(core, "Deleting visitors with no visits");
                    //
                    core.db.sqlCommandTimeout = 180;
                    core.db.executeNonQuery("delete ccVisitors from ccVisitors Left Join ccVisits on ccVisits.VisitorID=ccVisitors.ID where ccVisits.ID is null");
                }
                //
                //
                // delete nocookie visits
                // This must happen after the housekeep summarizing, and no sooner then 48 hours ago so all hits have been summarized before deleting
                //
                if (env.archiveDeleteNoCookie) {
                    //
                    // delete visitors from the non-cookie visits
                    //
                    LogController.logInfo(core, "Deleting visitors from visits with no cookie support older than Midnight, Two Days Ago");
                    string sql = "delete from ccvisitors from ccvisitors r,ccvisits v where r.id=v.visitorid and(v.CookieSupport=0)and(v.LastVisitTime<DATEADD(day,-2,CAST(GETDATE() AS DATE)))";
                    core.db.sqlCommandTimeout = 180;
                    core.db.executeNonQuery(sql);
                }

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}