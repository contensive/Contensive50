
using System;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;

namespace Contensive.CLI {
    class upgradeClass {
        //
        // ====================================================================================================
        /// <summary>
        /// Upgrade a single or all apps, optionally forcing full install to include up-to-date collections (to fix broken collection addons)
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="repair"></param>
        public static void upgrade(Contensive.Processor.CPClass cp, string appName, bool repair) {
            if (!string.IsNullOrEmpty(appName)) {
                //
                // -- upgrade app
                using (CPClass upgradeApp = new Contensive.Processor.CPClass(appName)) {
                    AppBuilderController.upgrade(upgradeApp.core, false, repair);
                    upgradeApp.Cache.InvalidateAll();
                }
            } else {
                //
                // -- upgrade all apps
                foreach (KeyValuePair<String, AppConfigModel> kvp in cp.core.serverConfig.apps) {
                    using (CPClass upgradeApp = new Contensive.Processor.CPClass(kvp.Key)) {
                        AppBuilderController.upgrade(upgradeApp.core, false, repair);
                        upgradeApp.Cache.InvalidateAll();
                    }
                }
            }
        }
    }
}
