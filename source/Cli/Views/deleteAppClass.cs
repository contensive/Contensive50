
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Contensive.Processor;
using System.Security.Permissions;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using Contensive.Processor.Models.Domain;
using System.Reflection;

namespace Contensive.CLI {
    class DeleteAppClass {
        /// <summary>
        /// create a new app. If appname is provided, create the app with defaults. if not appname, prompt for defaults
        /// </summary>
        /// <param name="appName"></param>
        public static void deleteApp( CPClass cpServer, string appName) {
            try {
                //
                if (!cpServer.serverOk) {
                    Console.WriteLine("Server Configuration not loaded correctly. Please run --configure");
                    return;
                }
                if (!cpServer.core.serverConfig.apps.ContainsKey( appName )) {
                    Console.WriteLine("The application [" + appName + "] was not found in this server group.");
                    return;
                }
                Console.Write("\n\rDeleting application [" + appName + "] from server group [" + cpServer.core.serverConfig.name + "].");
                //
                // delete the local file folders
                //
                //
                // drop the database on the server
                //
                //Contensive.Processor.Controllers.LogController.logInfo(cp.core, "Create database.");
                //cp.core.dbServer.createCatalog(appConfig.name);
                //
                //
                //remove the configuraion
                cpServer.core.serverConfig.apps.Remove( appName );
                cpServer.core.serverConfig.save(cpServer.core);
            } catch (Exception ex) {
                Console.WriteLine("Error: [" + ex.ToString() + "]");
            }
        }
    }
}
