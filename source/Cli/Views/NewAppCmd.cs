
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
using Contensive.Processor.Models.Db;
using System.Reflection;

namespace Contensive.CLI {
    class NewAppCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        public const string helpText = ""
            + "\r\n"
            + "\r\n--newapp (-n)"
            + "\r\n    new application wizard";
        //
        // ====================================================================================================
        /// <summary>
        /// create a new app. If appname is provided, create the app with defaults. if not appname, prompt for defaults
        /// </summary>
        /// <param name="appName"></param>
        public static void execute(string appName) {
            try {
                //
                // -- if you get a cluster object from cp with a key, and the key gives you access, you have a cluster object to create an app
                bool promptForArguments = string.IsNullOrWhiteSpace(appName);
                string domainName = "";
                const string iisDefaultDoc = "default.aspx";
                string authToken;
                string authTokenDefault = "909903";
                DateTime rightNow = DateTime.Now;
                authToken = authTokenDefault;
                //
                using (CPClass cp = new CPClass()) {
                    if (!cp.serverOk) {
                        Console.WriteLine("The Server Group does not appear to be configured correctly. Please run --configure");
                        return;
                    }
                    Console.Write("\n\nCreate an application within the server group [" + cp.core.serverConfig.name + "]. The application name can contain upper and lower case alpha numeric characters with no spaces or special characters. This name will be used to create and identity resources and should not be changed later.");
                    AppConfigModel appConfig = new AppConfigModel();
                    //
                    // -- enable it
                    appConfig.enabled = true;
                    //
                    // -- private key
                    appConfig.privateKey = Processor.Controllers.GenericController.getGUIDNaked();
                    //
                    // -- allow site monitor
                    appConfig.allowSiteMonitor = false;
                    //
                    // -- create app
                    if (promptForArguments) {
                        //
                        // -- app name
                        while (true) {
                            Type myType = typeof(Processor.Controllers.CoreController);
                            Assembly myAssembly = Assembly.GetAssembly(myType);
                            AssemblyName myAssemblyname = myAssembly.GetName();
                            Version myVersion = myAssemblyname.Version;
                            string appNameDefault = "app" + rightNow.Year + rightNow.Month.ToString().PadLeft(2, '0') + rightNow.Day.ToString().PadLeft(2, '0') + "v" + myVersion.Major.ToString("0") + myVersion.Minor.ToString("0");
                            Console.Write("\n\nEnter your application name. It must start with a letter and contain only letters and numbers.");
                            appName = GenericController.promptForReply("\nApplication Name:", appNameDefault).ToLowerInvariant();
                            if ( string.IsNullOrWhiteSpace( appName )) {
                                Console.Write("\nThis application name is not valid because it cannot be blank.");
                                continue;
                            }
                            if (!char.IsLetter(appName,0)) {
                                Console.Write("\nThis application name is not valid because it must start with a letter.");
                                continue;
                            }
                            if (!appName.All(x => char.IsLetterOrDigit(x))) {
                                Console.Write("\nThis application name is not valid because it must contain only letters and numbers.");
                                continue;
                            }
                            if (cp.core.serverConfig.apps.ContainsKey(appName.ToLowerInvariant())) {
                                Console.Write("\nThere is already an application with this name. To get the current server configuration, use cc -s");
                                continue;
                            }
                            break;
                        }
                    }
                    appConfig.name = appName;
                    //
                    // -- admin route
                    appConfig.adminRoute = "admin";
                    if (promptForArguments) {
                        bool routeOk = false;
                        do {
                            appConfig.adminRoute = GenericController.promptForReply("Admin Route (non-blank, no leading or trailing slash)", appConfig.adminRoute);
                            appConfig.adminRoute = Contensive.Processor.Controllers.GenericController.convertToUnixSlash(appConfig.adminRoute);
                            if (!string.IsNullOrEmpty(appConfig.adminRoute)) {
                                if (!appConfig.adminRoute.Substring(0, 1).Equals("/")) {
                                    if (!appConfig.adminRoute.Substring(appConfig.adminRoute.Length - 1, 1).Equals("/")) {
                                        routeOk = true;
                                    }
                                }
                            }
                        } while (!routeOk);
                    }
                    //
                    // -- domain
                    domainName = "www." + appConfig.name + ".com";
                    if (promptForArguments) { domainName = GenericController.promptForReply("Primary Domain Name", domainName); }
                    appConfig.domainList.Add(domainName);
                    //
                    // -- file architectur
                    string appArchitecture;
                    if (!promptForArguments) {
                        //
                        // -- default file archtecture -- localmode
                        appArchitecture = "1";
                        appConfig.localWwwPath = cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\www\\";
                        appConfig.localFilesPath = cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\files\\";
                        appConfig.localPrivatePath = cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\private\\";
                        appConfig.localTempPath = cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\temp\\";
                        appConfig.remoteWwwPath = "";
                        appConfig.remoteFilePath = "";
                        appConfig.remotePrivatePath = "";
                        appConfig.cdnFileUrl = "/" + appConfig.name + "/files/";
                        //
                    } else {
                        Console.Write("\nSelect application architecture.");
                        Console.Write("\n1 = Local Mode, scale-up architecture. Files are stored and accessed on the local server.");
                        Console.Write("\n2 = Remote Mode, scale-out architecture. wwwRoot, public files and private files are stored in subfolders of an S3 Bucket. A local mirror is used to file transfer.");
                        appArchitecture = GenericController.promptForReply("Enter 1 or 2", "1");
                        switch (appArchitecture) {
                            case "1":
                                //
                                // Local Mode, compatible with v4.1, cdn in appRoot folder as /" + appConfig.name + "/files/
                                //
                                appConfig.localWwwPath = GenericController.promptForReply("\napp files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\www\\");
                                appConfig.localFilesPath = GenericController.promptForReply("cdn files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\files\\");
                                appConfig.localPrivatePath = GenericController.promptForReply("private files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\private\\");
                                appConfig.localTempPath = GenericController.promptForReply("temp files (ephemeral storage)", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\temp\\");
                                appConfig.remoteWwwPath = "";
                                appConfig.remoteFilePath = "";
                                appConfig.remotePrivatePath = "";
                                appConfig.cdnFileUrl = GenericController.promptForReply("files Url (typically a virtual path on the application website)", "/" + appConfig.name + "/files/");
                                break;
                            case "2":
                                //
                                // 2 Scale Mode, cdn as AWS S3 bucket, FilePrivate as AWS S3 bucket"
                                //
                                appConfig.localWwwPath = GenericController.promptForReply("\napp files (local mirror)", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\www\\");
                                appConfig.localFilesPath = GenericController.promptForReply("cdn files (local mirror)", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\files\\");
                                appConfig.localPrivatePath = GenericController.promptForReply("private files (local mirror)", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\private\\");
                                appConfig.localTempPath = GenericController.promptForReply("temp files (local only storage)", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\temp\\");
                                appConfig.remoteWwwPath = GenericController.promptForReply("AWS S3 folder for app www storage", "/" + appConfig.name + "/www/");
                                appConfig.remoteFilePath = GenericController.promptForReply("AWS S3 folder for cdn file storage", "/" + appConfig.name + "/files/");
                                appConfig.remotePrivatePath = GenericController.promptForReply("AWS S3 folder for private file storage", "/" + appConfig.name + "/private/");
                                appConfig.cdnFileUrl = GenericController.promptForReply("files Url (typically a public folder in CDN website)", "https://s3.amazonaws.com/" + cp.core.serverConfig.awsBucketName + "/" + appConfig.name + "/files/");
                                break;
                                //case "4":
                                //    //
                                //    // Local Mode, cdn in appRoot folder as /cdn/
                                //    //
                                //    appConfig.localWwwPath = cliController.promptForReply("www files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\wwwRoot");
                                //    appConfig.localFilesPath = cliController.promptForReply("cdn files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\files");
                                //    appConfig.localPrivatePath = cliController.promptForReply("private files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\private");
                                //    appConfig.localTempPath = cliController.promptForReply("temp files (ephemeral storage)", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\temp");
                                //    cdnDomainName = domainName;
                                //    appConfig.remoteFilesPath = cliController.promptForReply("CDN files Url (virtual path)", "/cdn/");
                                //    break;
                                //case "3":
                                //    //
                                //    // 3 Local Mode, cdn as second iis site as cdn." + appConfig.name
                                //    //
                                //    appConfig.localWwwPath = cliController.promptForReply("App Root", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\wwwRoot");
                                //    appConfig.localFilesPath = cliController.promptForReply("CDN files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\files");
                                //    appConfig.localPrivatePath = cliController.promptForReply("private files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\private");
                                //    appConfig.localTempPath = cliController.promptForReply("temp files (ephemeral storage)", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\temp");
                                //    cdnDomainName = cliController.promptForReply("domain for CDN", domainName);
                                //    if (cdnDomainName == domainName)
                                //    {
                                //        appConfig.remoteFilesPath = cliController.promptForReply("CDN files Url (virtual path)", "/cdn/");
                                //    }
                                //    else
                                //    {
                                //        appConfig.remoteFilesPath = cliController.promptForReply("CDN files Url (website)", "http://" + cdnDomainName + "/");
                                //    }
                                //    cdnDomainName = domainName;
                                //    break;
                        }
                    }
                    Contensive.Processor.Controllers.LogController.logInfo(cp.core, "Create local folders.");
                    setupDirectory(appConfig.localWwwPath);
                    setupDirectory(appConfig.localFilesPath);
                    setupDirectory(appConfig.localPrivatePath);
                    setupDirectory(appConfig.localTempPath);
                    //
                    // -- save the app configuration and reload the server using this app
                    Contensive.Processor.Controllers.LogController.logInfo(cp.core, "Save app configuration.");
                    appConfig.appStatus = AppConfigModel.AppStatusEnum.maintenance;
                    cp.core.serverConfig.apps.Add(appConfig.name, appConfig);
                    cp.core.serverConfig.save(cp.core);
                    cp.core.serverConfig = ServerConfigModel.getObject(cp.core);
                    cp.core.appConfig = AppConfigModel.getObject(cp.core, cp.core.serverConfig, appConfig.name);
                    // 
                    // update local host file
                    //
                    try {
                        Contensive.Processor.Controllers.LogController.logInfo(cp.core, "Update host file to add domain [127.0.0.1 " + appConfig.name + "].");
                        File.AppendAllText("c:\\windows\\system32\\drivers\\etc\\hosts", System.Environment.NewLine + "127.0.0.1\t" + appConfig.name);
                    } catch (Exception ex) {
                        Console.Write("Error attempting to update local host file:" + ex.ToString());
                        Console.Write("Please manually add the following line to your host file (c:\\windows\\system32\\drivers\\etc\\hosts):" + "127.0.0.1\t" + appConfig.name);
                    }
                    //
                    // create the database on the server
                    //
                    Contensive.Processor.Controllers.LogController.logInfo(cp.core, "Create database.");
                    cp.core.dbServer.createCatalog(appConfig.name);
                    //
                    Contensive.Processor.Controllers.LogController.logInfo(cp.core, "When app creating is complete, use IIS Import Application to install either you web application, or the Contensive IISDefault.zip application.");
                    //// copy in the pattern files 
                    ////  - the only pattern is aspx
                    ////  - this is cc running, so they are setting up new application which may or may not have a webrole here.
                    ////  - setup a basic webrole just in case this will include one -- maybe later make it an option
                    ////
                    //Contensive.Processor.Controllers.logController.logInfo(cp.core, "Copy default site to www folder.");
                    //cp.core.programFiles.copyFolder("resources\\iisDefaultSite\\", "\\", cp.core.appRootFiles);
                    //
                    // replace "appConfig.name" with blank to use iis siteName as appConfig.name, or the name of this app in the default document in the apps public folder
                    //
                    //Contensive.Processor.Controllers.logController.logInfo(cp.core, "Update web.config.");
                    //string defaultContent = cp.core.appRootFiles.readFileText("web.config");
                    //defaultContent = defaultContent.Replace("{{appName}}", appConfig.name);
                    //cp.core.appRootFiles.saveFile("web.config", defaultContent);
                }
                //
                // initialize the new app, use the save authentication that was used to authorize this object
                //
                using (CPClass cp = new CPClass(appName)) {
                    Contensive.Processor.Controllers.LogController.logInfo(cp.core, "Verify website.");
                    Processor.Controllers.WebServerController.verifySite(cp.core, appName, domainName, cp.core.appConfig.localWwwPath, iisDefaultDoc);
                    //
                    Contensive.Processor.Controllers.LogController.logInfo(cp.core, "Run db upgrade.");
                    Processor.Controllers.NewAppController.upgrade(cp.core, true, true);
                    //
                    // -- set the application back to normal mode
                    cp.core.serverConfig.save(cp.core);
                    cp.core.siteProperties.setProperty(Constants.siteproperty_serverPageDefault_name, iisDefaultDoc);
                    //
                    Contensive.Processor.Controllers.LogController.logInfo(cp.core, "Upgrade complete.");
                    Contensive.Processor.Controllers.LogController.logInfo(cp.core, "Use IIS Import Application to install either you web application, or the Contensive IISDefault.zip application.");
                }
                //
            } catch (Exception ex) {
                Console.WriteLine("Error: [" + ex.ToString() + "]");
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// create a folder and make it full access for everyone
        /// </summary>
        /// <param name="folderPathPage"></param>
        public static void setupDirectory(string folderPathPage) {
            System.IO.Directory.CreateDirectory(folderPathPage);
            DirectoryInfo dInfo = new DirectoryInfo(folderPathPage);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
        }
    }
}
