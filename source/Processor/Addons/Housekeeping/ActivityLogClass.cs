
using System;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor;

namespace Contensive.Processor.Addons.Housekeeping {
    //
    public static class ActivityLogClass {
        public static void housekeep(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                // Activities with no Member
                //
                LogController.logInfo(core, "Deleting activities with no member record.");
                string sql = "delete ccactivitylog"
                    + " From ccactivitylog LEFT JOIN ccmembers on ccmembers.ID=ccactivitylog.memberid"
                    + " WHERE (ccmembers.ID is null)";
                core.db.executeQuery(sql);

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}