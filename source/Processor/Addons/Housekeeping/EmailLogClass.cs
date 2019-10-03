
using System;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;

namespace Contensive.Addons.Housekeeping {
    //
    public static class EmailLogClass {
        public static void housekeep(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                // delete email log entries not realted to a drop, older than archive.
                //
                LogController.logInfo(core, "Deleting non-drop email logs older then " + env.EmailDropArchiveAgeDays + " days");
                DateTime ArchiveEmailDropDate = env.rightNow.AddDays(-env.EmailDropArchiveAgeDays).Date;
                MetadataController.deleteContentRecords(core, "Email Log", "(emailDropId is null)and((DateAdded is null)or(DateAdded<=" + DbController.encodeSQLDate(ArchiveEmailDropDate) + "))");

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}