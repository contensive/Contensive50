
using System;
using Contensive.Processor.Controllers;

namespace Contensive.Processor.Addons.Housekeeping {
    //
    public static class EmailLogClass {
        public static void housekeep(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                // email log for only 365 days
                core.db.executeNonQuery("delete from ccemaillog where (dateadded < DATEADD(day,-365,CAST(GETDATE() AS DATE)))");
                //
                // delete email log entries not realted to a drop, older than archive.
                //
                LogController.logInfo(core, "Deleting non-drop email logs older then " + env.emailDropArchiveAgeDays + " days");
                DateTime ArchiveEmailDropDate = core.rightFrigginNow.AddDays(-env.emailDropArchiveAgeDays).Date;
                MetadataController.deleteContentRecords(core, "Email Log", "(emailDropId is null)and((DateAdded is null)or(DateAdded<=" + DbController.encodeSQLDate(ArchiveEmailDropDate) + "))");
                //
                // clear email body field for emails older than 7 days
                //
                LogController.logInfo(core, "Clear email body field for email logs older then " + env.emailLogBodyRetainDays + " days");
                DateTime emailLogBodyRetainDate = core.rightFrigginNow.AddDays(-env.emailLogBodyRetainDays).Date;
                core.db.executeNonQuery("update ccemaillog set body=null where dateadded<" + DbController.encodeSQLDate(emailLogBodyRetainDate));

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}