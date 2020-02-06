
using System;
using Contensive.Processor.Controllers;

namespace Contensive.Processor.Addons.Housekeeping {
    //
    public static class EmailDropClass {
        public static void housekeep(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                // email drops for only 365 days
                core.db.executeNonQuery("delete from ccemaildrops where (dateadded < DATEADD(day,-365,CAST(GETDATE() AS DATE)))");
                //
                // delete email drops older than archive.
                //
                LogController.logInfo(core, "Deleting email drops older then " + env.emailDropArchiveAgeDays + " days");
                //
                DateTime ArchiveEmailDropDate = default(DateTime);
                ArchiveEmailDropDate =  core.rightFrigginNow.AddDays(-env.emailDropArchiveAgeDays).Date;
                MetadataController.deleteContentRecords(core, "Email drops", "(DateAdded is null)or(DateAdded<=" + DbController.encodeSQLDate(ArchiveEmailDropDate) + ")");

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}