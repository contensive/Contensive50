
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
using static Contensive.BaseClasses.CPFileSystemBaseClass;
using Contensive.BaseClasses;

namespace Contensive.CLI {
    class UploadFilesCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        public const string helpText = ""
            + "\r\n"
            + "\r\n--uploadfiles"
            + "\r\n    Copies all local files to remote files. Use for migration to a remote file system."
            + "";
        //
        // ====================================================================================================
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
                    // verify the server has a remote file path configured
                    //
                    // create a local cdn file system
                    cp.core.cdnFiles.copyPathLocalToRemote("");
                    //var cdnLocalFileController = new Processor.Controllers.FileController(cp.core, true, cp.CdnFiles.PhysicalFilePath, "");
                    //CPFileSystemBaseClass cdnLocalFiles = new CPFileSystemClass(cp, cdnLocalFileController);
                    //uploadPath(cp, cp.CdnFiles, cdnLocalFiles, "\\");
                    //
                    // create a local private file system
                    cp.core.privateFiles.copyPathLocalToRemote("");
                    //var privateLocalFileController = new Processor.Controllers.FileController(cp.core, true, cp.PrivateFiles.PhysicalFilePath, "");
                    //CPFileSystemBaseClass privateLocalFiles = new CPFileSystemClass(cp, privateLocalFileController);
                    //uploadPath(cp, cp.PrivateFiles, privateLocalFiles, "\\");
                    //
                    // create a local www file system
                    cp.core.wwwFiles.copyPathLocalToRemote("");
                    //var wwwLocalFileController = new Processor.Controllers.FileController(cp.core, true, cp.WwwFiles.PhysicalFilePath, "");
                    //CPFileSystemBaseClass wwwLocalFiles = new CPFileSystemClass(cp, wwwLocalFileController);
                    //uploadPath(cp, cp.WwwFiles, wwwLocalFiles, "\\");
                }
                //
                //
                //
            } catch (Exception ex) {
                Console.WriteLine("Error: [" + ex.ToString() + "]");
            }
        }
        //
        private static void  uploadPath( CPClass cp, CPFileSystemBaseClass remoteFileSystem, CPFileSystemBaseClass localFileSystem, string sourcePath ) {
            foreach (var folder in localFileSystem.FolderList(sourcePath)) {
                uploadPath(cp, remoteFileSystem, localFileSystem, sourcePath  + folder.Name + "\\");                 
            }
            foreach ( var file in localFileSystem.FileList( sourcePath)) {
                DateTime rightNow = DateTime.Now;
                cp.TempFiles.Append("Upload" + rightNow.Year + rightNow.Month.ToString().PadLeft(2, '0') + rightNow.Day.ToString().PadLeft(2, '0') + ".log", Environment.NewLine + "Copying local file [" + sourcePath + file.Name + "] to remote public files.");
                Console.WriteLine("Copying local file [" + sourcePath + file.Name + "] to remote public files.");
                remoteFileSystem.CopyLocalToRemote(sourcePath + file.Name );
            }
        }
    }
}
