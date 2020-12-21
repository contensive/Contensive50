
using Contensive.Processor.Controllers;
using System;

namespace Contensive.Processor.Addons.Housekeeping {
    /// <summary>
    /// Housekeep this content
    /// </summary>
    public static class VisitPropertyClass {
        //
        //====================================================================================================
        /// <summary>
        /// houorly housekeep tasks
        /// </summary>
        /// <param name="core"></param>
        public static void executeHourlyTasks(CoreController core) {
            {
                //
                LogController.logInfo(core, "executeHourlyTasks, visitproperites");
                //
                //
                // -- delete properties of visits over 1 hour old
                string sql = "delete from ccproperties from ccproperties p left join  ccvisits v on (v.id=p.keyid and p.typeid=1) where v.lastvisittime<dateadd(hour, -1, getdate())";
                core.db.sqlCommandTimeout = 180;
                core.db.executeNonQuery(sql);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// daily housekeep tasks
        /// </summary>
        /// <param name="core"></param>
        public static void executeDailyTasks(CoreController core) {
            try {
                //
                LogController.logInfo(core, "executeDailyTasks, visitproperites");
                //
                {
                    //
                    // Visit Properties with no visits
                    string sql = "delete ccproperties from ccproperties left join ccvisits on ccvisits.id=ccproperties.keyid where (ccproperties.typeid=1) and (ccvisits.id is null)";
                    core.db.sqlCommandTimeout = 180;
                    core.db.executeNonQuery(sql);
                }
                {
                    //
                    // delete all visit properties over 24 hours old
                    string sql = "delete from ccProperties where (TypeID=1)and(dateAdded<dateadd(hour, -24, getdate()))";
                    core.db.sqlCommandTimeout = 180;
                    core.db.executeNonQuery(sql);
                    //Task.Run(() => core.db.executeNonQueryAsync(sql));
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
        //
    }
}