
using System;

namespace Contensive.CLI {
    //
    static class IisResetCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        internal static readonly string  helpText = ""
            + Environment.NewLine
            + Environment.NewLine + "--iisreset"
            + Environment.NewLine + "    Runs an iisreset, stopping and restarted the webserver (all sites). Requires elevated permissions."
            + "";
        //
        // ====================================================================================================
        /// <summary>
        /// manage the task scheduler service
        /// </summary>
        public static void execute(Contensive.Processor.CPClass cpServer) {
            cpServer.core.webServer.reset();
        }
    }
}
