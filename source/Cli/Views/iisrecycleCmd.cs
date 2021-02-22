
using Contensive.Processor;
using System;

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
            + Environment.NewLine + "    Runs an iis recycle, restarting each appPool selected. Use -a appName first to recycle to one site. Requires elevated permissions."
            + "";
        //
        // ====================================================================================================
        /// <summary>
        /// manage the task scheduler service
        /// </summary>
        public static void execute(CPClass cpServer, string appName) {
            if (string.IsNullOrEmpty(appName)) {
                //
                // -- all apps
                foreach( var appKvp in cpServer.core.serverConfig.apps) {
                    using var cp = new CPClass(appKvp.Key);
                    cp.core.webServer.recycle();
                }
            } else {
                //
                // -- appName specified
                using var cp = new CPClass(appName);
                cp.core.webServer.recycle();
            }
        }
    }
}
