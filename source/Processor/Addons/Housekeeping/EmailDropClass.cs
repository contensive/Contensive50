
using Contensive.Processor.Controllers;
using System;

namespace Contensive.Processor.Addons.Housekeeping {
    /// <summary>
    /// Housekeep this content
    /// </summary>
    public static class EmailDropClass {
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
                LogController.logInfo(core, "HousekeepDaily, emaildrops older then " + env.emailDropArchiveAgeDays + " days");
                //
                // email drops for only 365 days
                core.db.executeNonQuery("delete from ccemaildrops where (dateadded < dateadd(day,-" + env.emailDropArchiveAgeDays + ",cast(getdate() as date)))");
                //
                // email drops with no email
                core.db.executeNonQuery("delete from ccemaildrops from ccemaildrops d left join ccemail e on e.id=d.emailid where e.id is null");
                //
                //
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}