
using System;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;

namespace Contensive.Addons.Housekeeping {
    //
    public class EmailClass {
        public static void housekeep(CoreController core, HouseKeepEnvironmentModel env) {
            try {

            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}