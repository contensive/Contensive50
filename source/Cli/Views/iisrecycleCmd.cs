
using System;

namespace Contensive.CLI {
    //
    static class IisRecycleCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        internal static readonly string  helpText = ""
            + Environment.NewLine
            + Environment.NewLine + "--iisrecycle"
            + Environment.NewLine + "    Runs an iis recycle, restarting each appPool selected. Use -a appName first to limit the recycle to one site. Requires elevated permissions."
            + "";
        //
        // ====================================================================================================
        /// <summary>
        /// manage the task scheduler service
        /// </summary>
        public static void execute(Contensive.Processor.CPClass cpServer, string appName) {
            if (string.IsNullOrEmpty(appName)) {
                foreach (var kvp in cpServer.core.serverConfig.apps) {
                    cpServer.core.webServer.recycle(kvp.Key);
                }
            } else {
                cpServer.core.webServer.recycle(appName);
            }
        }
    }
}
