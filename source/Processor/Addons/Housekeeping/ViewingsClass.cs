
using System;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;

namespace Contensive.Addons.Housekeeping {
    //
    public class ViewingsClass {
        public static void housekeep(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                // delete nocookie visits
                // This must happen after the housekeep summarizing, and no sooner then 48 hours ago so all hits have been summarized before deleting
                //
                if (env.ArchiveDeleteNoCookie) {
                    //
                    // delete viewings from the non-cookie visits
                    //
                    LogController.logInfo(core, "Deleting viewings from visits with no cookie support older than Midnight, Two Days Ago");
                    string sql = "delete from ccviewings"
                        + " from ccviewings h,ccvisits v"
                        + " where h.visitid=v.id"
                        + " and(v.CookieSupport=0)and(v.LastVisitTime<" + env.SQLDateMidnightTwoDaysAgo + ")";
                    // if this fails, continue with the rest of the work
                    try {
                        core.db.executeQuery(sql);
                    } catch (Exception) {
                    }
                }
                //
                // viewings with no visit
                //
                LogController.logInfo(core, "Deleting viewings with null or invalid VisitID");
                core.db.deleteTableRecordChunks("ccviewings", "(visitid=0 or visitid is null)", 1000, 10000);

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}