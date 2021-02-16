
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;
using System;

namespace Contensive.Processor.Addons.Housekeeping {
    /// <summary>
    /// Housekeep this content
    /// </summary>
    public static class VisitorPropertyClass {
        //
        //====================================================================================================
        /// <summary>
        /// execute hourly tasks
        /// </summary>
        /// <param name="core"></param>
        public static void executeHourlyTasks(CoreController core) {
            try {
                //
                LogController.logInfo(core, "Housekeep, executeHourlyTasks, VisitorProperties");
                //
            } catch (Exception ex) {
                LogController.logError(core, ex);
                LogController.logAlarm(core, "Housekeep, exception, ex [" + ex + "]");
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// delete orphan visitor properties
        /// </summary>
        /// <param name="core"></param>
        public static void executeDailyTasks(CoreController core) {
            try {
                //
                LogController.logInfo(core, "Housekeep, visitorproperties");
                //
                string sql = "delete from ccProperties from ccProperties p left join ccvisitors m on m.id=p.KeyID where (p.TypeID=" + (int)PropertyModelClass.PropertyTypeEnum.visitor + ") and (m.ID is null)";
                core.db.sqlCommandTimeout = 180;
                core.db.executeNonQuery(sql);
                //
                // Visitor Properties with no visitor
                //
                LogController.logInfo(core, "Deleting visitor properties with no visitor record.");
                sql = "delete ccProperties from ccProperties LEFT JOIN ccvisitors on ccvisitors.ID=ccProperties.KeyID where ccproperties.typeid=2 and ccvisitors.id is null";
                core.db.sqlCommandTimeout = 180;
                core.db.executeNonQuery(sql);
            } catch (Exception ex) {
                LogController.logError(core, ex);
                LogController.logAlarm(core, "Housekeep, exception, ex [" + ex + "]");
                throw;
            }
        }
        //
    }
}