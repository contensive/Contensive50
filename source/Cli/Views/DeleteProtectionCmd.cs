
using System;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;

namespace Contensive.CLI {
    //
    class DeleteProtectionCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        public const string helpText = ""
            + "\r\n"
            + "\r\n--deleteprotection on|off"
            + "\r\n    Enables or Disables delete protection for an application. You must specify the application first with -a appName."
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
                case "true":
                case "false":
                case "yes":
                case "no":
                case "on":
                case "off":
                case "0":
                case "1":
                case "-1":
                    //
                    // -- turn on/off
                    using (var cp = new CPClass(appName)) {
                        cp.core.appConfig.deleteProtection = cpServer.Utils.EncodeBoolean(arg);
                        cp.core.appConfig.save(cp.core);
                        Console.WriteLine("delete protection set " + cp.core.appConfig.deleteProtection.ToString());
                    }
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
