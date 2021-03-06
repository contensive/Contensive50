﻿
using System;

namespace Contensive.CLI {
    static class RepairCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        internal static readonly string  helpText = ""
            + Environment.NewLine
            + Environment.NewLine + "--repair (-r)"
            + Environment.NewLine + "    reinstall the base collection and all it's dependancies. For all applications, or just one if specified with -a"
            + "";
        //
        // ====================================================================================================
        /// <summary>
        /// Repair a single or all apps, forcing full install to include up-to-date collections (to fix broken collection addons)
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="repair"></param>
        public static void execute(Contensive.Processor.CPClass cp, string appName) {
            UpgradeCmd.execute(cp, appName, true);
        }
    }
}
