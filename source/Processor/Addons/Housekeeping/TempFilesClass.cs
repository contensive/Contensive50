
using Contensive.Processor.Controllers;
using System;
using static Contensive.Processor.Controllers.GenericController;

namespace Contensive.Processor.Addons.Housekeeping {
    /// <summary>
    /// Housekeep the temp file system
    /// </summary>
    public static class TempFilesClass {
        //
        //====================================================================================================
        /// <summary>
        /// delete all files over 1 hour old
        /// </summary>
        /// <param name="core"></param>
        public static void deleteFiles(CoreController core) {
            try {
                //
                LogController.logInfo(core, "Housekeep, delete temp files over 1 hour old");
                //
                deleteFiles(core, "\\");

            } catch (Exception ex) {
                LogController.logError(core, ex);
                LogController.logAlarm(core, "Housekeep, exception, ex [" + ex + "]");
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// delete all files over 1 hour old from the current path, recursive
        /// </summary>
        /// <param name="core"></param>
        /// <param name="path"></param>
        public static void deleteFiles(CoreController core, string path) {
            try {
                foreach (var folder in core.tempFiles.getFolderList(path)) {
                    deleteFiles(core, path + folder.Name + "\\");
                }
                foreach (var file in core.tempFiles.getFileList(path)) {
                    if (encodeDate(file.DateCreated).AddHours(1) < core.dateTimeNowMockable) {
                        core.tempFiles.deleteFile(path + file.Name);
                    }
                }

            } catch (Exception ex) {
                LogController.logError(core, ex);
                LogController.logAlarm(core, "Housekeep, exception, ex [" + ex + "]");
                throw;
            }
        }

    }
}