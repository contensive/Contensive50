
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

namespace Contensive.CLI {
    class UploadFilesClass {
        /// <summary>
        /// create a new app. If appname is provided, create the app with defaults. if not appname, prompt for defaults
        /// </summary>
        /// <param name="appName"></param>
        public static void uploadFiles( CPClass cpServer, string appName) {
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

                using (var cp = new CPClass(appName)) {
                    Console.Write("\n\rUploading files from local file store to remove file store.");
                    // verify the server has a remote file path configured
                    // create a local filesystem to the cdn's local path
                    var cdnRootLocalPath = cp.CdnFiles.PhysicalFilePath;
                    var cdnLocalFileController = new Processor.Controllers.FileController(cp.core, true, cdnRootLocalPath, "");
                    CPFileSystemClass cdnLocalFiles = new CPFileSystemClass(cp, cdnLocalFileController);
                    uploadPath(cp, cdnLocalFiles, "\\");

                }
                //
                //
                //
            } catch (Exception ex) {
                Console.WriteLine("Error: [" + ex.ToString() + "]");
            }
        }
        //
        private static void  uploadPath( CPClass cp, CPFileSystemClass cdnLocalFiles, string sourcePath ) {
            foreach (var folder in cdnLocalFiles.FolderList(sourcePath)) {
                uploadPath(cp, cdnLocalFiles, sourcePath  + folder.Name + "\\");                 
            }
            foreach ( var file in cdnLocalFiles.FileList( sourcePath)) {
                cp.CdnFiles.CopyLocalToRemote(sourcePath + file.Name );
            }
        }
    }
}
