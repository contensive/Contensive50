﻿
using System;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;

namespace Contensive.CLI {
    class InstallCollectionCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        public const string helpText = ""
            + "\r\n"
            + "\r\n--install CollectionName"
            + "\r\n    downloads and installed the addon collection named from the Contensive Support Library"
            + "";
        //
        // ====================================================================================================
        /// <summary>
        /// create a new app. If appname is provided, create the app with defaults. if not appname, prompt for defaults
        /// </summary>
        /// <param name="appName"></param>
        public static void execute( CPClass cpServer, string appName, string collectionName) {
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
                //
                // -- determine guid of collection
                var collectionList = new List<CollectionController.CollectionStoreClass>();
                CollectionController.getRegistryCollectionList(cpServer.core, ref collectionList);
                string collectionGuid = "";
                foreach (var collection in collectionList) {
                    if (collection.name.ToLowerInvariant() == collectionName.ToLowerInvariant()) {
                        collectionGuid = collection.guid;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(collectionGuid)) {
                    Console.WriteLine("Collection was not found on the distribution server");
                } else {
                    string returnErrorMessage = "";
                    string logPrefix = "CLI";
                    var collectionsInstalledList = new List<string>();
                    var nonCritialErrorList = new List<string>();
                    if (string.IsNullOrEmpty(appName)) {
                        foreach (KeyValuePair<String, AppConfigModel> kvp in cpServer.core.serverConfig.apps) {
                            using (CPClass cpApp = new CPClass(kvp.Key)) {
                                var context = new Stack<string>(new string[] { "command line interface install command [" + collectionName + ", " + collectionGuid + "]" });
                                CollectionController.installCollectionFromLibrary(cpApp.core, context, collectionGuid, ref returnErrorMessage, false, false, ref nonCritialErrorList, logPrefix, ref collectionsInstalledList);
                                if (!string.IsNullOrEmpty(returnErrorMessage)) {
                                    Console.WriteLine("There was an error installing the collection: " + returnErrorMessage);
                                }
                                cpApp.Cache.InvalidateAll();
                            }
                        }
                    } else {
                        using (CPClass cpApp = new CPClass(appName)) {
                            var context = new Stack<string>(new string[] { "command line interface install command [" + collectionName + ", " + collectionGuid + "]" });
                            CollectionController.installCollectionFromLibrary(cpApp.core, context, collectionGuid, ref returnErrorMessage, false, false, ref nonCritialErrorList, logPrefix, ref collectionsInstalledList);
                            if (!string.IsNullOrEmpty(returnErrorMessage)) {
                                Console.WriteLine("There was an error installing the collection: " + returnErrorMessage);
                            }
                            cpApp.Cache.InvalidateAll();
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine("Error: [" + ex.ToString() + "]");
            }
        }
    }
}
