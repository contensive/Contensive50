﻿
using System;
using Contensive.Processor.Controllers;

namespace Contensive.Processor.Addons.Housekeeping {
    //
    public static class VisitorClass {
        public static void housekeep(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                LogController.logInfo(core, "Housekeep, visitors");
                //
                //
                // delete nocookie visits
                // This must happen after the housekeep summarizing, and no sooner then 48 hours ago so all hits have been summarized before deleting
                //
                if (env.archiveDeleteNoCookie) {
                    //
                    // delete visitors from the non-cookie visits
                    //
                    LogController.logInfo(core, "Deleting visitors from visits with no cookie support older than Midnight, Two Days Ago");
                    string sql = "delete from ccvisitors"
                        + " from ccvisitors r,ccvisits v"
                        + " where r.id=v.visitorid"
                        + " and(v.CookieSupport=0)and(v.LastVisitTime<" + env.sqlDateMidnightTwoDaysAgo + ")";
                    core.db.executeNonQuery(sql);
                }

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}