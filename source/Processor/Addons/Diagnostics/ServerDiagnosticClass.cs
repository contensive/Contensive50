
using System;
using Contensive.Processor;

using System.Text;
using System.IO;
using Contensive.Processor.Controllers;
using Contensive.Models.Db;
using System.Globalization;
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
                    double freeSpace = Math.Round(100.0 * (Convert.ToDouble(driveTest.AvailableFreeSpace) / Convert.ToDouble(driveTest.TotalSize)), 2);
                    if (freeSpace < 10) { return "ERROR, Drive-C does not have 10% free"; };
                    result.AppendLine("ok, drive-c free space [" + freeSpace + "%], [" + (driveTest.AvailableFreeSpace / (1024 * 1024)).ToString("F2", CultureInfo.InvariantCulture) + " MB]");
                }
                if (Directory.Exists(@"d:\")) {
                    DriveInfo driveTest = new DriveInfo("d");
                    double freeSpace = Math.Round( 100.0 * (Convert.ToDouble(driveTest.AvailableFreeSpace) / Convert.ToDouble(driveTest.TotalSize)), 2);
                    if (freeSpace < 10) { return "ERROR, Drive-D does not have 10% free"; };
                    result.AppendLine("ok, drive-D free space [" + freeSpace + "%], [" + (driveTest.AvailableFreeSpace / (1024 * 1024)).ToString("F2", CultureInfo.InvariantCulture) + " MB]");
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
                if (DbBaseModel.createList<AddonModel>(core.cpParent, "(ProcessNextRun<" + DbController.encodeSQLDate(DateTime.Now.AddHours(-1)) + ")").Count > 0) {
                    return "ERROR, there are process addons unexecuted for over 1 hour. TaskScheduler may not be enabled, or no server is running the Contensive Task Service.";
                }
                if (DbBaseModel.createList<TaskModel>(core.cpParent, "(dateCompleted is null)and(dateStarted<" + DbController.encodeSQLDate(DateTime.Now.AddHours(-1)) + ")").Count > 0) {
                    return "ERROR, there are tasks that have been executing for over 1 hour. The Task Runner Server may have stopped.";
                }
                result.AppendLine("ok, taskscheduler running.");
                //
                // -- test for taskrunner not running
                if (DbBaseModel.createList<TaskModel>(core.cpParent, "(dateCompleted is null)and(dateStarted is null)").Count > 100) {
                    return "ERROR, there are over 100 task waiting to be execute. The Task Runner Server may have stopped.";
                }
                result.AppendLine("ok, taskrunner running.");
                //
                // -- verify the email process is running.
                if (cp.Site.GetDate("EmailServiceLastCheck") < DateTime.Now.AddHours(-1)) {
                    return "ERROR, Email process has not executed for over 1 hour.";
                }
                result.AppendLine("ok, email process running.");
                //
                // -- last -- if alarm folder is not empty, fail diagnostic. Last so others can add an alarm entry
                foreach (var alarmFile in core.programDataFiles.getFileList("Alarms/")) {
                    return "ERROR, Alarm folder is not empty, [" + core.programDataFiles.readFileText("Alarms/" + alarmFile.Name) + "].";
                }
                return "ok, all server diagnostics passed" + Environment.NewLine + result.ToString();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return "ERROR, unexpected exception during diagnostics";
            }
        }
    }
}
