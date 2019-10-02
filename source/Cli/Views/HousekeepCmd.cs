
using System;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;

namespace Contensive.CLI {
    class HousekeepCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        internal static string  helpText = ""
            + Environment.NewLine
            + Environment.NewLine + "--housekeep (-h)"
            + Environment.NewLine + "    housekeep all appications, or just one if specifid with -a"
            + "";
        //
        // ====================================================================================================
        /// <summary>
        /// Upgrade a single or all apps, optionally forcing full install to include up-to-date collections (to fix broken collection addons)
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="repair"></param>
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
