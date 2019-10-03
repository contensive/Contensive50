
using Contensive.Processor.Controllers;
using System;

namespace Contensive.Addons.Housekeeping {
    //
    public static class HourlyTasksClass {
        //
        //====================================================================================================
        //
        public static void housekeep(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                // -- delete temp files
                TempFilesClass.deleteFiles(core);

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}