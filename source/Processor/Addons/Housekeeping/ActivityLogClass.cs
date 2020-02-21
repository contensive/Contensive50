//
using System;
using Contensive.Processor.Controllers;

namespace Contensive.Processor.Addons.Housekeeping {
    //
    public static class ActivityLogClass {
        public static void housekeep(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                LogController.logInfo(core, "Housekeep, activitylog");
                //
                //
                // activity log for only 30 days
                core.db.executeNonQuery("delete from abaccountinglog where (dateadded < DATEADD(day,-30,CAST(GETDATE() AS DATE)))");
                //
                // Activities with no Member
                //
                LogController.logInfo(core, "Deleting activities with no member record.");
                string sql = "delete ccactivitylog"
                    + " From ccactivitylog LEFT JOIN ccmembers on ccmembers.ID=ccactivitylog.memberid"
                    + " WHERE (ccmembers.ID is null)";
                core.db.executeNonQuery(sql);

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}