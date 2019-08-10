
using System;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;

namespace Contensive.CLI {
    class ExportCollectionCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        internal static string helpText = ""
            + Environment.NewLine
            + Environment.NewLine + "--exportcollection CollectionName"
            + Environment.NewLine + "    creates a collection zip file with the collections names"
            + "";
        //
        // ====================================================================================================
        /// <summary>
        /// create a new app. If appname is provided, create the app with defaults. if not appname, prompt for defaults
        /// </summary>
        /// <param name="appName"></param>
        public static void execute(CPClass cpServer, string appName, string collectionName) {
            try {
                //
                if (!cpServer.serverOk) {
                    Console.WriteLine("Server Configuration not loaded correctly. Please run --configure");
                    return;
                }
                //
                throw new NotImplementedException(); 
            } catch (Exception ex) {
                Console.WriteLine("Error: [" + ex.ToString() + "]");
            }
        }
        //
        // ====================================================================================================
        /// <summary>
        /// install a file to an app
        /// </summary>
        /// <param name="cpApp"></param>
        /// <param name="collectionPhysicalPathFilename"></param>
        private static void installCollectionFile( string appName, string collectionPhysicalPathFilename) {
            string logPrefix = "CLI";
            using (CPClass cpApp = new CPClass(appName)) {
                var contextLog = new Stack<string>();
                contextLog.Push("command line interface install command [" + collectionPhysicalPathFilename + "]");
                string returnErrorMessage = "";
                //
                // todo - this interface should all be tempFiles not private files (to avoid all the remote file system copies
                //
                // -- copy the file to private files
                string privatePath = "installTmp\\";
                string privatePathFilename = privatePath + System.IO.Path.GetFileName(collectionPhysicalPathFilename);
                string privatePhysicalPathFilename = cpApp.PrivateFiles.PhysicalFilePath + privatePathFilename;
                if (!cpApp.PrivateFiles.FolderExists(privatePath)) {
                    cpApp.PrivateFiles.CreateFolder(privatePath);
                }
                if (System.IO.File.Exists(privatePhysicalPathFilename)) {
                    System.IO.File.Delete(privatePhysicalPathFilename);
                }
                System.IO.File.Copy(collectionPhysicalPathFilename, privatePhysicalPathFilename);
                cpApp.PrivateFiles.CopyLocalToRemote(privatePathFilename);
                //
                // -- build the collection folders for all collection files in the download path and created a list of collection Guids that need to be installed
                var collectionsDownloaded = new List<string>();
                string return_ErrorMessage = "";
                var nonCriticalErrorList = new List<string>();
                var collectionsInstalled = new List<string>();
                string collectionGuidsInstalled = "";
                CollectionInstallController.installCollectionFromPrivateFile(cpApp.core, contextLog, privatePathFilename, ref return_ErrorMessage, ref collectionGuidsInstalled, false, false, ref nonCriticalErrorList, logPrefix, ref collectionsInstalled);
                if (!string.IsNullOrEmpty(returnErrorMessage)) {
                    Console.WriteLine("There was an error installing the collection: " + returnErrorMessage);
                }
                cpApp.PrivateFiles.DeleteFile(privatePhysicalPathFilename);
                cpApp.Cache.InvalidateAll();
            }
        }

    }
}
