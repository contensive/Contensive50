
using System;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using System.Text;
using System.IO;
using Contensive.Processor.Controllers;
//
namespace Contensive.Addons.Diagnostics {
    //
    public class ServerDiagnosticClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// Returns OK on success
        /// + available drive space
        /// + log size
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            try {
                var result = new StringBuilder();
                var core = ((CPClass)cp).core;
                //
                // -- tmp, check for 10% free on C-drive and D-drive
                if (Directory.Exists(@"c:\")) {
                    DriveInfo driveTest = new DriveInfo("c");
                    double freeSpace = 100.0 * (Convert.ToDouble(driveTest.AvailableFreeSpace) / Convert.ToDouble(driveTest.TotalSize));
                    if (freeSpace < 10) { return "ERROR, Drive-C does not have 10% free"; };
                    result.AppendLine("ok, drive-c free space [" + freeSpace + "%], [" + (driveTest.AvailableFreeSpace / (1024 * 1024)) + " MB]");
                }
                if (Directory.Exists(@"d:\")) {
                    DriveInfo driveTest = new DriveInfo("d");
                    double freeSpace = 100.0 * (Convert.ToDouble(driveTest.AvailableFreeSpace) / Convert.ToDouble(driveTest.TotalSize));
                    if (freeSpace < 10) { return "ERROR, Drive-C does not have 10% free"; };
                    result.AppendLine("ok, drive-d free space [" + freeSpace + "%], [" + (driveTest.AvailableFreeSpace / (1024 * 1024)) + " MB]");
                }
                //
                // -- log files under 1MB
                foreach (var fileDetail in core.programDataFiles.getFileList("Logs/")) {
                    if (fileDetail.Size > 1000000) { return "ERROR, log file size error [" + fileDetail.Name + "], size [" + fileDetail.Size + "]"; }
                }
                result.AppendLine("ok, all log files under 1 MB");
                //
                // test default data connection
                try {
                    int TrapID = 0;
                    using (var csData = new CsModel(core)) {
                        if (csData.insert("Trap Log")) {
                            TrapID = csData.getInteger("ID");
                        }
                        if (TrapID == 0) {
                            return "ERROR, Failed to insert record in default data source.";
                        } else {
                            MetadataController.deleteContentRecord(core, "Trap Log", TrapID);
                        }
                    }
                } catch (Exception exDb) {
                    return "ERROR, exception occured during default data source record insert, [" + exDb.ToString() + "].";
                }
                result.AppendLine("ok, database connection passed.");
                //
                // -- test for taskscheduler not running
                if (AddonModel.createList(core, "(ProcessNextRun<" + DbController.encodeSQLDate(DateTime.Now.AddHours(-1)) + ")").Count > 0) {
                    return "ERROR, there are process addons unexecuted for over 1 hour. TaskScheduler may not be enabled, or no server is running the Contensive Task Service.";
                }
                //
                // -- test for taskrunner stuck
                if (TaskModel.createList(core, "(dateCompleted is null)and(dateStarted<" + DbController.encodeSQLDate(DateTime.Now.AddHours(-1)) + ")").Count > 0) {
                    return "ERROR, there are tasks that have been executing for over 1 hour. The Task Runner Server may have stopped.";
                }
                //
                // -- test for taskrunner not running
                if (TaskModel.createList(core, "(dateCompleted is null)and(dateStarted is null)").Count > 100 ) {
                    return "ERROR, there are over 100 task waiting to be execute. The Task Runner Server may have stopped.";
                }
                //
                // -- verify the email process is running.
                if (cp.Site.GetDate("EmailServiceLastCheck") < DateTime.Now.AddHours(-1)) {
                    return "ERROR, Email process has not executed for over 1 hour.";
                }
                //
                // -- last -- if alarm folder is not empty, fail diagnostic. Last so others can add an alarm entry
                foreach (var alarmFile in core.programDataFiles.getFileList("Alarms/")) {
                    return "ERROR, Alarm folder is not empty, [" + core.programDataFiles.readFileText(alarmFile.Name) + "].";
                }
                return "ok, all server diagnostics passed" + Environment.NewLine + result.ToString();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return "ERROR, unexpected exception during diagnostics";
            }
        }
    }
}
