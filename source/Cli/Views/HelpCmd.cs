
using System;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;

namespace Contensive.CLI {
    class HelpCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cpServer"></param>
        public static void consoleWriteAll(Contensive.Processor.CPClass cpServer) {
            Console.Write(helpText);
            Console.Write(ConfigureCmd.helpText);
            Console.Write(NewAppCmd.helpText);
            Console.Write(DeleteAppCmd.helpText);
            Console.Write(DeleteProtectionCmd.helpText);
            Console.Write(ExecuteAddonCmd.helpText);
            Console.Write(FixTableFolderCaseCmd.helpText);
            Console.Write(FlushCacheCmd.helpText);
            Console.Write(HousekeepCmd.helpText);
            Console.Write(InstallCollectionCmd.helpText);
            //Console.Write(LoggingCmd.helpText);
            Console.Write(RepairCmd.helpText);
            Console.Write(RunTaskCmd.helpText);
            Console.Write(StatusCmd.helpText);
            Console.Write(TaskRunnerCmd.helpText);
            Console.Write(TaskSchedulerCmd.helpText);
            Console.Write(TasksCmd.helpText);
            Console.Write(UpgradeCmd.helpText);
            Console.Write(UploadFilesCmd.helpText);
            Console.Write(VersionCmd.helpText);
        }
        //
        const string helpText = ""
            + "\r\ncc command line"
            + "\r\n"
            + "\r\n-a appName"
            + "\r\n    apply the current command to just one application"
            + "";
        //
    }
}
