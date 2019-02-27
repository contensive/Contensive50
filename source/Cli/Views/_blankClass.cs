
using System;
using Contensive.Processor;

namespace Contensive.CLI {
    class _blankClass {
        /// <summary>
        /// create a new app. If appname is provided, create the app with defaults. if not appname, prompt for defaults
        /// </summary>
        /// <param name="appName"></param>
        public static void execute( CPClass cpServer, string appName) {
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
                if (cpServer.core.serverConfig.isLocalFileSystem) {
                    Console.WriteLine("This server is in localmode. Uploading is only valid if NOT in localmode.");
                    return;
                }
                using (var cp = new CPClass(appName)) {
                    Console.Write("\n\rUploading files from local file store to remove file store.");
                }
            } catch (Exception ex) {
                Console.WriteLine("Error: [" + ex.ToString() + "]");
            }
        }
    }
}
