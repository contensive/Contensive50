
using System;
using Contensive.Processor.Controllers;

namespace Contensive.Processor.Addons.Housekeeping {
    //
    public static class EmailDropClass {
        public static void housekeep(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                LogController.logInfo(core, "HousekeepDaily, emaildrops older then " + env.emailDropArchiveAgeDays + " days");
                //
                // email drops for only 365 days
                core.db.executeNonQuery("delete from ccemaildrops where (dateadded < dateadd(day,-" + env.emailDropArchiveAgeDays + ",cast(getdate() as date)))");
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}