
using Contensive.Processor.Controllers;
using System;

namespace Contensive.Processor.Addons.Housekeeping {
    /// <summary>
    /// Activity Log
    /// </summary>
    public static class ActivityLogClass {
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
                LogController.logInfo(core, "Housekeep, activitylog");
                {
                    //
                    //
                    LogController.logInfo(core, "Deleting activities older than archiveAgeDays (" + env.archiveAgeDays + ").");
                    //
                    core.db.executeNonQuery("delete from ccactivitylog where (DateAdded is null)or(DateAdded<DATEADD(day,-" + env.archiveAgeDays + ",CAST(GETDATE() AS DATE)))");

                }
                {
                    //
                    LogController.logInfo(core, "Deleting activities with no member record.");
                    //
                    core.db.executeNonQuery("delete ccactivitylog from ccactivitylog left join ccmembers on ccmembers.id=ccactivitylog.memberid where (ccmembers.id is null)");
                }

            } catch (Exception ex) {
                LogController.logError(core, ex);
                LogController.logAlarm(core, "Housekeep, exception, ex [" + ex.ToString() + "]");
                throw;

            }
        }
    }
}