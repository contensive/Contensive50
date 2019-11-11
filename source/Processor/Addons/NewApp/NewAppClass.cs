
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;

using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using System.Text;
using Contensive.Models.Db;
using System.Globalization;
using Contensive.Processor.Models.Domain;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
//
namespace Contensive.Processor.Addons.NewApp {
    //
    public class NewAppClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// create a new app. Must run in in task-service with elevated permissions
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cpRootApp) {
            try {
                var coreRootApp = ((CPClass)(cpRootApp)).core;

                string appName = cpRootApp.Doc.GetText("appName");
                string domainName = cpRootApp.Doc.GetText("domainName");
                //
                const string iisDefaultDoc = "default.aspx";
                string authToken;
                string authTokenDefault = "909903";
                DateTime rightNow = DateTime.Now;
                authToken = authTokenDefault;
                //
                using (CPClass cpServer = new CPClass()) {
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
                    appConfig.name = appName;
                    //
                    // -- admin route
                    appConfig.adminRoute = "admin";
                    //
                    // -- domain
                    domainName = "www." + appConfig.name + ".com";
                    appConfig.domainList.Add(domainName);
                    //
                    // -- file architectur
                    if (cpServer.core.serverConfig.isLocalFileSystem) {
                        //
                        // -- no prompts, local file system
                        appConfig.localWwwPath = cpServer.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\www\\";
                        appConfig.localFilesPath = cpServer.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\files\\";
                        appConfig.localPrivatePath = cpServer.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\private\\";
                        appConfig.localTempPath = cpServer.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\temp\\";
                        appConfig.remoteWwwPath = "";
                        appConfig.remoteFilePath = "";
                        appConfig.remotePrivatePath = "";
                        appConfig.cdnFileUrl = "/" + appConfig.name + "/files/";
                    } else {
                        //
                        // -- no prompts, remote file system
                        appConfig.localWwwPath = cpServer.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\www\\";
                        appConfig.localFilesPath = cpServer.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\files\\";
                        appConfig.localPrivatePath = cpServer.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\private\\";
                        appConfig.localTempPath = cpServer.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appConfig.name + "\\temp\\";
                        appConfig.remoteWwwPath = "/" + appConfig.name + "/www/";
                        appConfig.remoteFilePath = "/" + appConfig.name + "/files/";
                        appConfig.remotePrivatePath = "/" + appConfig.name + "/private/";
                        appConfig.cdnFileUrl = "https://s3.amazonaws.com/" + cpServer.core.serverConfig.awsBucketName + "/" + appConfig.name + "/files/";
                    }
                    Contensive.Processor.Controllers.LogController.logInfo(cpServer.core, "Create local folders.");
                    setupDirectory(appConfig.localWwwPath);
                    setupDirectory(appConfig.localFilesPath);
                    setupDirectory(appConfig.localPrivatePath);
                    setupDirectory(appConfig.localTempPath);
                    //
                    // -- save the app configuration and reload the server using this app
                    Contensive.Processor.Controllers.LogController.logInfo(cpServer.core, "Save app configuration.");
                    appConfig.appStatus = AppConfigModel.AppStatusEnum.maintenance;
                    cpServer.core.serverConfig.apps.Add(appConfig.name, appConfig);
                    cpServer.core.serverConfig.save(cpServer.core);
                    cpServer.core.serverConfig = ServerConfigModel.getObject(cpServer.core);
                    cpServer.core.appConfig = AppConfigModel.getObject(cpServer.core, cpServer.core.serverConfig, appConfig.name);
                    // 
                    // update local host file
                    //
                    try {
                        LogController.logInfo(cpServer.core, "Update host file to add domain [127.0.0.1 " + appConfig.name + "].");
                        File.AppendAllText("c:\\windows\\system32\\drivers\\etc\\hosts", System.Environment.NewLine + "127.0.0.1\t" + appConfig.name);
                    } catch (Exception ex) {
                        Console.Write("Error attempting to update local host file:" + ex.ToString());
                        Console.Write("Please manually add the following line to your host file (c:\\windows\\system32\\drivers\\etc\\hosts):" + "127.0.0.1\t" + appConfig.name);
                    }
                    //
                    // create the database on the server
                    //
                    LogController.logInfo(cpServer.core, "Create database.");
                    cpServer.core.dbServer.createCatalog(appConfig.name);
                }
                //
                // initialize the new app, use the save authentication that was used to authorize this object
                //
                using (CPClass cp = new CPClass(appName)) {
                    LogController.logInfo(cp.core, "Verify website.");
                    WebServerController.verifySite(cp.core, appName, domainName, cp.core.appConfig.localWwwPath, iisDefaultDoc);
                    //
                    LogController.logInfo(cp.core, "Run db upgrade.");
                    BuildController.upgrade(cp.core, true, true);
                    //
                    // -- set the application back to normal mode
                    cp.core.serverConfig.save(cp.core);
                    cp.core.siteProperties.setProperty(Constants.siteproperty_serverPageDefault_name, iisDefaultDoc);
                    //
                    LogController.logInfo(cp.core, "Upgrade complete.");
                    LogController.logInfo(cp.core, "Use IIS Import Application to install either you web application, or the Contensive IISDefault.zip application.");
                }
                //
                return string.Empty;
            } catch (Exception ex) {
                cpRootApp.Site.ErrorReport(ex);
                return "ERROR, unexpected exception during NewApp";
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
