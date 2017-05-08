
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Contensive.Core;

namespace  Contensive.CLI {
    class createAppClass {
        public void createApp() {
            try
            {
                //
                // -- if you get a cluster object from cp with a key, and the key gives you access, you have a cluster object to create an app
                String appName;
                string domainName;
                string iisDefaultDoc = "default.aspx";
                string authToken;
                string authTokenDefault = "909903";
                string appArchitecture = "";
                string cdnDomainName = "";
                DateTime rightNow = DateTime.Now;
                System.Web.Script.Serialization.JavaScriptSerializer json = new System.Web.Script.Serialization.JavaScriptSerializer();
                authToken = authTokenDefault;
                //
                using (CPClass cp = new CPClass())
                {
                    if (!cp.serverOk)
                    {
                        Console.WriteLine("Server Configuration not loaded correctly. Please run --configure");
                        return;
                    }
                    //
                    // -- create app
                    Console.Write("\n\nCreate application within the server group [" + cp.core.serverConfig.name + "].");
                    Core.Models.Entity.serverConfigModel.appConfigModel appConfig = new Core.Models.Entity.serverConfigModel.appConfigModel();
                    //
                    // app name
                    bool appNameOk = false;
                    do
                    {
                        string appNameDefault = "app" + rightNow.Year + rightNow.Month.ToString().PadLeft(2, '0') + rightNow.Day.ToString().PadLeft(2, '0') + rightNow.Hour.ToString().PadLeft(2, '0') + rightNow.Minute.ToString().PadLeft(2, '0') + rightNow.Second.ToString().PadLeft(2, '0');
                        appName = cliController.promptForReply("Application Name", appNameDefault).ToLower();
                        appNameOk = !cp.core.serverConfig.apps.ContainsKey(appName.ToLower());
                        if (!appNameOk) { Console.Write("\n\nThere is already an application with this name. To get the current server configuration, use clib -s"); }
                    } while (!appNameOk);
                    appConfig.name = appName;
                    Console.Write("\n\rApplication Architecture");
                    Console.Write("\n\r\t1 Local Mode, compatible with v4.1, cdn is virtual folder /" + appName + "/files/");
                    Console.Write("\n\r\t2 Local Mode, cdn is virtual folder /cdn/");
                    Console.Write("\n\r\t3 Local Mode, cdn as second iis site as cdn." + appName);
                    Console.Write("\n\r\t4 Scale Mode, cdn as AWS S3 bucket, privateFiles as AWS S3 bucket");
                    appArchitecture = cliController.promptForReply("Enter 1,2,3, or 4", "1");
                    appConfig.adminRoute = cliController.promptForReply("Admin Route", "/admin/");
                    appConfig.allowSiteMonitor = false;
                    domainName = cliController.promptForReply("Primary Domain Name", "www." + appName + ".com");
                    appConfig.domainList.Add(domainName);
                    appConfig.enableCache = true;
                    appConfig.enabled = true;
                    appConfig.privateKey = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "");
                    switch (appArchitecture)
                    {
                        case "1":
                            //
                            // Local Mode, compatible with v4.1, cdn in appRoot folder as /" + appName + "/files/
                            //
                            appConfig.appRootFilesPath = cliController.promptForReply("App Root", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\wwwRoot");
                            appConfig.cdnFilesPath = cliController.promptForReply("CDN files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\files");
                            appConfig.privateFilesPath = cliController.promptForReply("private files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\private");
                            cdnDomainName = domainName;
                            appConfig.cdnFilesNetprefix = cliController.promptForReply("CDN files Url (virtual path)", "\\" + appName + "\\files\\");
                            break;
                        case "2":
                            //
                            // Local Mode, cdn in appRoot folder as /cdn/
                            //
                            appConfig.appRootFilesPath = cliController.promptForReply("App Root", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\wwwRoot");
                            appConfig.cdnFilesPath = cliController.promptForReply("CDN files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\files");
                            appConfig.privateFilesPath = cliController.promptForReply("private files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\private");
                            cdnDomainName = domainName;
                            appConfig.cdnFilesNetprefix = cliController.promptForReply("CDN files Url (virtual path)", "\\cdn\\");
                            break;
                        case "3":
                            //
                            // 3 Local Mode, cdn as second iis site as cdn." + appName
                            //
                            appConfig.appRootFilesPath = cliController.promptForReply("App Root", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\wwwRoot");
                            appConfig.cdnFilesPath = cliController.promptForReply("CDN files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\files");
                            appConfig.privateFilesPath = cliController.promptForReply("private files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\private");
                            cdnDomainName = cliController.promptForReply("domain for CDN", domainName);
                            if (cdnDomainName == domainName)
                            {
                                appConfig.cdnFilesNetprefix = cliController.promptForReply("CDN files Url (virtual path)", "\\cdn\\");
                            }
                            else
                            {
                                appConfig.cdnFilesNetprefix = cliController.promptForReply("CDN files Url (website)", "http://" + cdnDomainName + "/");
                            }
                            cdnDomainName = domainName;
                            break;
                        case "4":
                            //
                            // 4 Scale Mode, cdn as AWS S3 bucket, privateFiles as AWS S3 bucket"
                            //
                            appConfig.appRootFilesPath = cliController.promptForReply("App Root (local mirror)", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\wwwRoot");
                            appConfig.cdnFilesPath = cliController.promptForReply("CDN files (local mirror)", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\files");
                            appConfig.privateFilesPath = cliController.promptForReply("private files (local mirror)", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\private");
                            cdnDomainName = cliController.promptForReply("domain for CDN", domainName);
                            appConfig.cdnFilesNetprefix = cliController.promptForReply("CDN files Url (website)", "http://" + cdnDomainName + "/");
                            break;
                    }
                    System.IO.Directory.CreateDirectory(appConfig.appRootFilesPath);
                    System.IO.Directory.CreateDirectory(appConfig.cdnFilesPath);
                    System.IO.Directory.CreateDirectory(appConfig.privateFilesPath);
                    //
                    // -- save the app configuration and reload the server using this app
                    cp.core.serverConfig.apps.Add(appName, appConfig);
                    cp.core.serverConfig.saveObject(cp.core);
                    cp.core.serverConfig = Contensive.Core.Models.Entity.serverConfigModel.getObject(cp.core, appName);
                    // 
                    // update local host file
                    //
                    try
                    {
                        File.AppendAllText("c:\\windows\\system32\\drivers\\etc\\hosts", System.Environment.NewLine + "127.0.0.1\t" + appName);
                    }
                    catch (Exception ex)
                    {
                        Console.Write("Error attempting to update local host file:" + ex.ToString());
                    }
                    //
                    // create the database on the server
                    //
                    cp.core.dbEngine.createCatalog(appName);
                    //
                    // copy in the pattern files 
                    //  - the only pattern is aspx
                    //  - this is clib running, so they are setting up new application which may or may not have a webrole here.
                    //  - setup a basic webrole just in case this will include one -- maybe later make it an option
                    //
                    cp.core.programFiles.copyFolder("resources\\aspxDefaultApp\\", "\\", cp.core.appRootFiles);
                    //
                    // replace "appName" with the name of this app in the default document in the apps public folder
                    //
                    string defaultContent = cp.core.appRootFiles.readFile(iisDefaultDoc);
                    defaultContent = defaultContent.Replace("DefaultAppName", appName);
                    cp.core.appRootFiles.saveFile(iisDefaultDoc, defaultContent);
                }
                //
                // initialize the new app, use the save authentication that was used to authorize this object
                //
                using (CPClass cp = new CPClass(appName))
                {
                    Core.Controllers.iisController.verifySite(cp.core,appName, domainName, cp.core.serverConfig.appConfig.appRootFilesPath, iisDefaultDoc);
                    Core.Controllers.appBuilderController.upgrade(cp.core,true);
                    cp.core.siteProperties.setProperty(constants.siteproperty_serverPageDefault_name, iisDefaultDoc);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: [" + ex.ToString() + "]");
            }
        }
    }
}
