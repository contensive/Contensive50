
using System;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor;

namespace Contensive.Processor.Addons.Housekeeping {
    //
    public static class EmailDropClass {
        public static void housekeep(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                // delete email drops older than archive.
                //
                LogController.logInfo(core, "Deleting email drops older then " + env.emailDropArchiveAgeDays + " days");
                //
                DateTime ArchiveEmailDropDate = default(DateTime);
                ArchiveEmailDropDate = env.rightNow.AddDays(-env.emailDropArchiveAgeDays).Date;
                MetadataController.deleteContentRecords(core, "Email drops", "(DateAdded is null)or(DateAdded<=" + DbController.encodeSQLDate(ArchiveEmailDropDate) + ")");

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}