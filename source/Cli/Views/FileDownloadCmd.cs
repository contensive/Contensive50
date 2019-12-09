
using System;
using System.Collections.Generic;
using Contensive.Processor;

namespace Contensive.CLI {
    class FileDownloadCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        internal static string  helpText = ""
            + Environment.NewLine
            + Environment.NewLine + "--fileDownload [cdn] [www] [private]"
            + Environment.NewLine + "    Copies all remote files (Amazon S3) to the local file system (this machine). Specify one or more file locals (cdn,www, or private).  An application name is required. (-a applicationName) Use for migration to a remote file system. "
            + "";
        //
        // ====================================================================================================
        /// <summary>
        /// create a new app. If appname is provided, create the app with defaults. if not appname, prompt for defaults
        /// </summary>
        /// <param name="appName"></param>
        public static void execute( CPClass cpServer, string appName, List<string> dstList) {
            try {
                //
                if (!cpServer.serverOk) {
                    Console.WriteLine("Server Configuration not loaded correctly. Please run --configure");
                    return;
                }
                if (string.IsNullOrEmpty(appName)) {
                    Console.WriteLine("The application [" + appName + "] was not found in this server group.");
                    return;
                }
                if (!cpServer.core.serverConfig.apps.ContainsKey(appName)) {
                    Console.WriteLine("The application [" + appName + "] was not found in this server group.");
                    return;
                }
                if (cpServer.core.serverConfig.isLocalFileSystem) {
                    Console.WriteLine("This server is in localmode. Uploading is only valid if NOT in localmode.");
                    return;
                }
                using (var cp = new CPClass(appName)) {
                    bool wwwDst = false;
                    bool cdnDst = false;
                    bool privateDst = false;
                    foreach( var dst in dstList ) {
                        if(!string.IsNullOrEmpty(dst)) {
                            switch (dst.ToLower()) {
                                case "www":
                                    wwwDst = true;
                                    break;
                                case "cdn":
                                    cdnDst = true;
                                    break;
                                case "private":
                                    privateDst = true;
                                    break;
                                default:
                                    Console.WriteLine("Error, the destination file system is not valid [" + dst + "]");
                                    return;
                            }
                        }
                    }
                    if (!(wwwDst || cdnDst || privateDst )) {
                        Console.WriteLine("Error, no file system was included." + helpText);
                        return;
                    }
                    if (cdnDst) {
                        Console.Write("\n\rUploading cdn files remote file store...");
                        cp.core.cdnFiles.copyPathRemoteToLocal("");
                    }
                    if (privateDst) {
                        Console.Write("\n\rUploading private files remote file store...");
                        cp.core.privateFiles.copyPathRemoteToLocal("");
                    }
                    if (wwwDst) {
                        Console.Write("\n\rUploading www files remote file store...");
                        cp.core.wwwFiles.copyPathRemoteToLocal("");
                    }
                }
                //
                //
                //
            } catch (Exception ex) {
                Console.WriteLine("Error: [" + ex.ToString() + "]");
            }
        }
        //
        //private static void  uploadPath( CPClass cp, CPFileSystemBaseClass remoteFileSystem, CPFileSystemBaseClass localFileSystem, string sourcePath ) {
        //    foreach (var folder in localFileSystem.FolderList(sourcePath)) {
        //        uploadPath(cp, remoteFileSystem, localFileSystem, sourcePath  + folder.Name + "\\");                 
        //    }
        //    foreach ( var file in localFileSystem.FileList( sourcePath)) {
        //        DateTime rightNow = DateTime.Now;
        //        cp.TempFiles.Append("Upload" + rightNow.Year + rightNow.Month.ToString().PadLeft(2, '0') + rightNow.Day.ToString().PadLeft(2, '0') + ".log", Environment.NewLine + "Copying local file [" + sourcePath + file.Name + "] to remote public files.");
        //        Console.WriteLine("Copying local file [" + sourcePath + file.Name + "] to remote public files.");
        //        remoteFileSystem.CopyLocalToRemote(sourcePath + file.Name );
        //    }
        //}
    }
}
