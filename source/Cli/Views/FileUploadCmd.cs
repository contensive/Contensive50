
using System;
using System.Collections.Generic;
using Contensive.Processor;

namespace Contensive.CLI {
    static class FileUploadCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        internal static readonly string  helpText = ""
            + Environment.NewLine
            + Environment.NewLine + "--fileUpload [cdn] [www] [private]"
            + Environment.NewLine + "    Copies all local files (this machine) to the remote file system (Amazon S3). Specify one or more file locals (cdn,www, or private).  An application name is required. (-a applicationName) Use for migration to a remote file system. "
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
                        cp.core.cdnFiles.copyPathLocalToRemote("");
                    }
                    if (privateDst) {
                        Console.Write("\n\rUploading private files remote file store...");
                        cp.core.privateFiles.copyPathLocalToRemote("");
                    }
                    if (wwwDst) {
                        Console.Write("\n\rUploading www files remote file store...");
                        cp.core.wwwFiles.copyPathLocalToRemote("");
                    }
                }
                //
                //
                //
            } catch (Exception ex) {
                Console.WriteLine("Error: [" + ex + "]");
            }
        }
    }
}
