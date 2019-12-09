
using System;

namespace Contensive.CLI {
    //
    class BlankCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        internal static string  helpText = ""
            + Environment.NewLine
            + Environment.NewLine + "--taskscheduler run|on|off"
            + Environment.NewLine + "    Use 'run' to execute the taskscheduler in the console (temporary). Use 'on' or 'off' to manage the taskscheduler service."
            + "";
        //
        // ====================================================================================================
        /// <summary>
        /// manage the task scheduler service
        /// </summary>
        /// <param name="cpServer"></param>
        /// <param name="appName"></param>
        /// <param name="arg">the argument following the command</param>
        public static void execute(Contensive.Processor.CPClass cpServer, string appName, string arg) {
            switch (arg.ToLowerInvariant()) {
                case "run":
                    //
                    // -- run the taskscheduler in the console
                    return;
                case "on":
                case "off":
                    //
                    // -- turn the windows service scheduler on/off
                    return;
                default:
                    //
                    // -- invalid argument
                    Console.Write(Environment.NewLine + "Invalid argument [" + arg + "].");
                    Console.Write(helpText);
                    Console.Write(Environment.NewLine + "Run cc --help for a full list of commands.");
                    return;
            }
        }
    }
}
