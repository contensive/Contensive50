
using System;

namespace Contensive.CLI {
    static class FlushCacheCmd {
        //
        // ====================================================================================================
        internal static readonly string  helpText = ""
            + Environment.NewLine
            + Environment.NewLine + "--flushcache"
            + Environment.NewLine + "    invalidate cache. Use with -a"
            + "";
        //
        // ====================================================================================================
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
                foreach (var kvp in cpServer.core.serverConfig.apps) {
                    String housekeepAppName = kvp.Key;
                    using (Contensive.Processor.CPClass cpApp = new Contensive.Processor.CPClass(housekeepAppName)) {
                        cpApp.Cache.InvalidateAll();
                    }
                }
            }
        }
    }
}
