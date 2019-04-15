
using System;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;

namespace Contensive.CLI {
    //
    class RunTaskCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        public const string helpText = ""
            + "\r\n"
            + "\r\n--runtask guid-of-assigned-taskrunner"
            + "\r\n    Run a task in the task table. Used internally to run tasks in other processes. Requires you first set the application with -appname."
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
            if (string.IsNullOrEmpty(appName)) {
                //
                // -- invalid argument
                Console.Write("\r\nInvalid argument. Runtask requires you first set the application with -a appname.");
                Console.Write(helpText);
                Console.Write("\r\nRun cc --help for a full list of commands.");
                return;
            }
            if (string.IsNullOrEmpty(arg) || !Contensive.Processor.Controllers.GenericController.common_isGuid(arg)) {
                //
                // -- invalid argument
                Console.Write("\r\nInvalid argument, task guid [" + arg + "]. Runtask requires a valid task guid argument.");
                Console.Write(helpText);
                Console.Write("\r\nRun cc --help for a full list of commands.");
                return;
            }
            //
            // -- execute task
            TaskRunnerController.executeRunnerTasks(appName, arg);
        }
    }
}
