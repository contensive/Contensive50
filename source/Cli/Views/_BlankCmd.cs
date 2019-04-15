
using System;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;

namespace Contensive.CLI {
    //
    class BlankCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        public const string helpText = ""
            + "\r\n"
            + "\r\n--taskscheduler run|on|off"
            + "\r\n    Use 'run' to execute the taskscheduler in the console (temporary). Use 'on' or 'off' to manage the taskscheduler service."
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
                    Console.Write("\r\nInvalid argument [" + arg + "].");
                    Console.Write(helpText);
                    Console.Write("\r\nRun cc --help for a full list of commands.");
                    return;
            }
        }
    }
}
