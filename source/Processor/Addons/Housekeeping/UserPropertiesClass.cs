
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;
using System;

namespace Contensive.Processor.Addons.Housekeeping {
    /// <summary>
    /// Housekeep user properties
    /// </summary>
    public static class UserProperyClass {
        //
        //====================================================================================================
        /// <summary>
        /// execute hourly tasks
        /// </summary>
        /// <param name="core"></param>
        public static void executeHourlyTasks(CoreController core) {
            try {
                //
                LogController.logInfo(core, "Housekeep, executeHourlyTasks, UserProperties");
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
        /// daily housekeep. delete orphan user properties
        /// </summary>
        /// <param name="core"></param>
        public static void executeDailyTasks(CoreController core) {
            try {
                //
                LogController.logInfo(core, "Housekeep, userproperites");
                //
                string sql = "delete from ccProperties from ccProperties p left join ccmembers m on m.id=p.KeyID where (p.TypeID=" + (int)PropertyModelClass.PropertyTypeEnum.user + ") and (m.ID is null)";
                core.db.sqlCommandTimeout = 180;
                core.db.executeNonQuery(sql);
                //
                // Member Properties with no member
                //
                LogController.logInfo(core, "Deleting member properties with no member record.");
                sql = "delete ccproperties from ccproperties left join ccmembers on ccmembers.id=ccproperties.keyid where (ccproperties.typeid=0) and (ccmembers.id is null)";
                core.db.sqlCommandTimeout = 180;
                core.db.executeNonQuery(sql);

            } catch (Exception ex) {
                LogController.logError(core, ex);
                LogController.logAlarm(core, "Housekeep, exception, ex [" + ex.ToString() + "]");
                throw;
            }
        }
        //
    }
}