//
using Contensive.Processor.Controllers;
using System;

namespace Contensive.Processor.Addons.Housekeeping {
    /// <summary>
    /// Housekeep this content
    /// </summary>
    public static class ContentWatchListRulesClass {
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
                LogController.logAlarm(core, "Housekeep, exception, ex [" + ex.ToString() + "]");
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
                // ContentWatchListRules with bad ContentWatchID
                //
                LogController.logInfo(core, "Deleting ContentWatchList Rules with bad ContentWatchID.");
                string sql = "delete ccContentWatchListRules"
                    + " From ccContentWatchListRules"
                    + " LEFT JOIN ccContentWatch on ccContentWatch.ID=ccContentWatchListRules.ContentWatchID"
                    + " WHERE (ccContentWatch.ID is null)";
                core.db.executeNonQuery(sql);
                //
                // ContentWatchListRules with bad ContentWatchListID
                //
                LogController.logInfo(core, "Deleting ContentWatchList Rules with bad ContentWatchListID.");
                sql = "delete ccContentWatchListRules"
                    + " From ccContentWatchListRules"
                    + " LEFT JOIN ccContentWatchLists on ccContentWatchLists.ID=ccContentWatchListRules.ContentWatchListID"
                    + " WHERE (ccContentWatchLists.ID is null)";
                core.db.executeNonQuery(sql);

            } catch (Exception ex) {
                LogController.logError(core, ex);
                LogController.logAlarm(core, "Housekeep, exception, ex [" + ex.ToString() + "]");
                throw;

            }
        }
    }
}