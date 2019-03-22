
using System;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;

namespace Contensive.CLI {
    class installAddonClass {
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
                CollectionController.getRemoteCollectionStoreList(cpServer.core, ref collectionList);
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
                    if (string.IsNullOrEmpty(appName)) {
                        foreach (KeyValuePair<String, AppConfigModel> kvp in cpServer.core.serverConfig.apps) {
                            using (Contensive.Processor.CPClass cpApp = new Contensive.Processor.CPClass(kvp.Key)) {
                                string returnErrorMessage = "";
                                string logPrefix = "CLI";
                                var installedCollections = new List<string>();
                                var nonCritialErrorList = new List<string>();
                                CollectionController.installCollectionFromRegistry(cpApp.core, collectionGuid, ref returnErrorMessage, "", false, false, ref nonCritialErrorList, logPrefix, ref installedCollections);
                                if (!string.IsNullOrEmpty(returnErrorMessage)) {
                                    Console.WriteLine("There was an error installing the collection: " + returnErrorMessage);
                                }
                                cpApp.Cache.InvalidateAll();
                            }
                        }
                    } else {
                        using (Contensive.Processor.CPClass cpApp = new Contensive.Processor.CPClass(appName)) {
                            string returnErrorMessage = "";
                            string logPrefix = "CLI";
                            var installedCollections = new List<string>();
                            var nonCritialErrorList = new List<string>();
                            CollectionController.installCollectionFromRegistry(cpApp.core, collectionGuid, ref returnErrorMessage, "", false, false, ref nonCritialErrorList, logPrefix, ref installedCollections);
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
