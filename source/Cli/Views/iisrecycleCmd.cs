
using Contensive.Processor;
using System;
using System.Linq;

namespace Contensive.CLI {
    //
    static class IisRecycleCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        internal static readonly string helpText = ""
            + Environment.NewLine
            + Environment.NewLine + "--iisrecycle"
            + Environment.NewLine + "    Runs an iis recycle, restarting each appPool selected. Use -a appName first to limit the recycle to one site. Requires elevated permissions."
            + "";
        //
        // ====================================================================================================
        /// <summary>
        /// manage the task scheduler service
        /// </summary>
        public static void execute(CPClass cpServer, string appName) {
            if (string.IsNullOrEmpty(appName)) {
                //
                // -- use the first app
                if (cpServer.core.serverConfig.apps.Count > 0) {
                    using (CPClass cp = new CPClass(cpServer.core.serverConfig.apps.First().Key)) {
                        cp.core.webServer.recycle();
                    }
                }
            } else {
                using (var cp = new CPClass(appName)) {
                    cp.core.webServer.recycle();
                }
            }
        }
    }
}
