
using System;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;

namespace Contensive.CLI {
    //
    class LoggingCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        public const string helpText = ""
            + "\r\n"
            + "\r\n--logging on|off"
            + "\r\n    Enable or disable logging at the server level"
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
                case "on":
                case "off":
                    //
                    // -- turn on/off
                    cpServer.core.serverConfig.enableLogging = Processor.Controllers.GenericController.encodeBoolean(arg);
                    cpServer.core.serverConfig.save(cpServer.core);
                    Console.WriteLine("enableLogging set " + cpServer.core.serverConfig.enableLogging.ToString());
                    return;
                default:
                    //
                    // -- invalid argument
                    Console.Write("Invalid argument [" + arg + "].");
                    Console.Write(helpText);
                    return;
            }
        }
    }
}
