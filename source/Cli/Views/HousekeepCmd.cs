
using System;

namespace Contensive.CLI {
    static class HousekeepCmd {
        //
        // ====================================================================================================
        internal static readonly string  helpText = ""
            + Environment.NewLine
            + Environment.NewLine + "--housekeep (-h)"
            + Environment.NewLine + "    housekeep all appications, or just one if specifid with -a"
            + "";
        //
        // ====================================================================================================
        public static void execute(Contensive.Processor.CPClass cpServer, string appName) {
            if (!string.IsNullOrEmpty(appName)) {
                //
                // -- housekeep app
                using (Contensive.Processor.CPClass cpApp = new Contensive.Processor.CPClass(appName)) {
                    cpApp.Doc.SetProperty("force", true);
                    cpApp.executeAddon(Contensive.Processor.Constants.addonGuidHousekeep, BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple);
                    cpApp.Cache.InvalidateAll();
                }
            } else {
                //
                // -- housekeep all apps
                foreach (var kvp in cpServer.core.serverConfig.apps) {
                    String housekeepAppName = kvp.Key;
                    using (Contensive.Processor.CPClass cpApp = new Contensive.Processor.CPClass(housekeepAppName)) {
                        cpApp.Doc.SetProperty("force", true);
                        cpApp.executeAddon(Contensive.Processor.Constants.addonGuidHousekeep, BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple);
                        cpApp.Cache.InvalidateAll();
                    }
                }
            }
        }
    }
}
