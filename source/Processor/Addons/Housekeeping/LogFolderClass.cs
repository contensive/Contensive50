
using System;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using System.Collections.Generic;
using Contensive.BaseClasses;

namespace Contensive.Addons.Housekeeping {
    //
    public static class LogFolderClass {
        public static void housekeep(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                // Log files Older then 30 days
                //
                houseKeep_LogFolder(core, env, "temp", env.thirtyDaysAgo);
                houseKeep_LogFolder(core, env, "TrapLogs", env.thirtyDaysAgo);
                houseKeep_LogFolder(core, env, "BounceLog", env.thirtyDaysAgo);
                houseKeep_LogFolder(core, env, "BounceProcessing", env.thirtyDaysAgo);
                houseKeep_LogFolder(core, env, "SMTPLog", env.thirtyDaysAgo);
                houseKeep_LogFolder(core, env, "DebugLog", env.thirtyDaysAgo);

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
        //
        //====================================================================================================
        //
        public static void houseKeep_LogFolder(CoreController core, HouseKeepEnvironmentModel env, string FolderName, DateTime LastMonth) {
            try {
                LogController.logInfo(core, "Deleting files from folder [" + FolderName + "] older than " + LastMonth);
                List<CPFileSystemBaseClass.FileDetail> FileList = core.privateFiles.getFileList(FolderName);
                foreach (CPFileSystemBaseClass.FileDetail file in FileList) {
                    if (file.DateCreated < LastMonth) {
                        core.privateFiles.deleteFile(FolderName + "/" + file.Name);
                    }
                }
                return;
                //
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}