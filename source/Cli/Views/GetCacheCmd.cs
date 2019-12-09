
using System;

namespace Contensive.CLI {
    static class GetCacheCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        internal static string helpText = ""
            + Environment.NewLine
            + Environment.NewLine + "-a appName --getcache key"
            + Environment.NewLine + "    gets the cache object for the specified key and displays the object in JSON format. appName and key are required."
            + "";
        //
        // ====================================================================================================
        /// <summary>
        /// Get a cache value from its key
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="repair"></param>
        public static void execute(Contensive.Processor.CPClass cpServer, string appName, string key) {
            if (!string.IsNullOrEmpty(appName)) {
                //
                if (!cpServer.core.serverConfig.apps.ContainsKey(appName)) {
                    Console.WriteLine("The application [" + appName + "] was not found in this server group.");
                    return;
                }
                //
                // -- get the key
                using (Contensive.Processor.CPClass cpApp = new Contensive.Processor.CPClass(appName)) {
                    object obj = cpApp.Cache.GetObject(key);
                    string result = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                    Console.Write(result);
                }
            }
        }
    }
}
