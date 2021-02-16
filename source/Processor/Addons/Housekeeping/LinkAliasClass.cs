
using Contensive.Processor.Controllers;
using System;

namespace Contensive.Processor.Addons.Housekeeping {
    /// <summary>
    /// Housekeep this content
    /// </summary>
    public static class LinkAliasClass {
        //
        //====================================================================================================
        /// <summary>
        /// execute hourly tasks
        /// </summary>
        /// <param name="core"></param>
        public static void executeHourlyTasks(CoreController core) {
            try {
                //
                LogController.logInfo(core, "Housekeep, executeHourlyTasks, LinkAlias");
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
        /// execute Daily Tasks
        /// </summary>
        /// <param name="core"></param>
        /// <param name="env"></param>
        public static void executeDailyTasks(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                LogController.logInfo(core, "Housekeep, linkalias");
                //
                // -- delete dups
                string sql = "delete from ccLinkAliases where id in ( select b.id from cclinkaliases a,cclinkaliases b where a.id<b.id and a.name=b.name )";
                core.db.executeNonQuery(sql);
                //
                // -- delete blank name
                sql = "delete from ccLinkAliases where (name is null)or(name='')";
                core.db.executeNonQuery(sql);
                //
                // -- delete missing or invalid page
                sql = "delete from ccLinkAliases from ccLinkAliases l left join ccpagecontent p on p.id=l.pageId where p.id is null";
                core.db.executeNonQuery(sql);

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }

        }
    }
}