
using System;
using Contensive.Processor.Controllers;

namespace Contensive.Processor.Addons.Housekeeping {
    //
    public static class EmailLogClass {
        public static void housekeep(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                // delete email log entries not realted to a drop, older than archive.
                //
                LogController.logInfo(core, "Deleting non-drop email logs older then " + env.emailDropArchiveAgeDays + " days");
                DateTime ArchiveEmailDropDate = env.rightNow.AddDays(-env.emailDropArchiveAgeDays).Date;
                MetadataController.deleteContentRecords(core, "Email Log", "(emailDropId is null)and((DateAdded is null)or(DateAdded<=" + DbController.encodeSQLDate(ArchiveEmailDropDate) + "))");

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}