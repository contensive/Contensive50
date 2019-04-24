
using System;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;

namespace Contensive.CLI {
    class FlushCacheCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        internal const string  helpText = ""
            + "\r\n"
            + "\r\n--flushcache"
            + "\r\n    invalidate cache. Use with -a"
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
                // -- flush this app
                using (Contensive.Processor.CPClass cpApp = new Contensive.Processor.CPClass(appName)) {
                    cpApp.Cache.InvalidateAll();
                }
            } else {
                //
                // -- flush all apps
                foreach (KeyValuePair<String, AppConfigModel> kvp in cpServer.core.serverConfig.apps) {
                    String housekeepAppName = kvp.Key;
                    using (Contensive.Processor.CPClass cpApp = new Contensive.Processor.CPClass(housekeepAppName)) {
                        cpApp.Cache.InvalidateAll();
                    }
                }
            }
        }
    }
}
