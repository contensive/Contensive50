﻿
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Contensive.Core;
using System.Security.Permissions;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using Contensive.Core.Models.Context;

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
                    appConfigModel appConfig = new appConfigModel();
                    //
                    // -- app name
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
                    //
                    // -- admin route
                    appConfig.adminRoute = "";
                    bool routeOk = false;
                    do
                    {
                        appConfig.adminRoute = cliController.promptForReply("Admin Route (non-blank, no leading or trailing slash)", "admin");
                        appConfig.adminRoute = Contensive.Core.Controllers.genericController.convertToUnixSlash(appConfig.adminRoute);
                        if (!string.IsNullOrEmpty(appConfig.adminRoute))
                        {
                            if (!appConfig.adminRoute.Substring(0, 1).Equals("/"))
                            {
                                if (!appConfig.adminRoute.Substring(appConfig.adminRoute.Length - 1, 1).Equals("/"))
                                {
                                    routeOk = true;
                                }
                            }
                        }
                    } while (!routeOk);
                    //
                    // -- 
                    appConfig.allowSiteMonitor = false;
                    domainName = cliController.promptForReply("Primary Domain Name", "www." + appName + ".com");
                    appConfig.domainList.Add(domainName);
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
                            appConfig.tempFilesPath = cliController.promptForReply("temp files (ephemeral storage)", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\temp");
                            cdnDomainName = domainName;
                            appConfig.cdnFilesNetprefix = cliController.promptForReply("CDN files Url (virtual path)", "/" + appName + "/files/");
                            break;
                        case "2":
                            //
                            // Local Mode, cdn in appRoot folder as /cdn/
                            //
                            appConfig.appRootFilesPath = cliController.promptForReply("App Root", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\wwwRoot");
                            appConfig.cdnFilesPath = cliController.promptForReply("CDN files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\files");
                            appConfig.privateFilesPath = cliController.promptForReply("private files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\private");
                            appConfig.tempFilesPath = cliController.promptForReply("temp files (ephemeral storage)", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\temp");
                            cdnDomainName = domainName;
                            appConfig.cdnFilesNetprefix = cliController.promptForReply("CDN files Url (virtual path)", "/cdn/");
                            break;
                        case "3":
                            //
                            // 3 Local Mode, cdn as second iis site as cdn." + appName
                            //
                            appConfig.appRootFilesPath = cliController.promptForReply("App Root", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\wwwRoot");
                            appConfig.cdnFilesPath = cliController.promptForReply("CDN files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\files");
                            appConfig.privateFilesPath = cliController.promptForReply("private files", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\private");
                            appConfig.tempFilesPath = cliController.promptForReply("temp files (ephemeral storage)", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\temp");
                            cdnDomainName = cliController.promptForReply("domain for CDN", domainName);
                            if (cdnDomainName == domainName)
                            {
                                appConfig.cdnFilesNetprefix = cliController.promptForReply("CDN files Url (virtual path)", "/cdn/");
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
                            appConfig.tempFilesPath = cliController.promptForReply("temp files (ephemeral storage)", cp.core.serverConfig.localDataDriveLetter + ":\\inetpub\\" + appName + "\\temp");
                            cdnDomainName = cliController.promptForReply("domain for CDN", domainName);
                            appConfig.cdnFilesNetprefix = cliController.promptForReply("CDN files Url (website)", "http://" + cdnDomainName + "/");
                            break;
                    }
                    setupDirectory(appConfig.appRootFilesPath);
                    setupDirectory(appConfig.cdnFilesPath);
                    setupDirectory(appConfig.privateFilesPath);
                    setupDirectory(appConfig.tempFilesPath);
                    //
                    //FileIOPermission f2;
                    //
                    // -- setup appRoot
                    //f2 = new FileIOPermission(FileIOPermissionAccess.AllAccess, appConfig.appRootFilesPath);
                    //try
                    //{
                    //    f2.Demand();
                    //}
                    //catch (SecurityException s)
                    //{
                    //    Console.WriteLine(s.Message);
                    //}
                    //
                    // -- setup cdn
                    //System.IO.Directory.CreateDirectory(appConfig.cdnFilesPath);
                    //f2 = new FileIOPermission(FileIOPermissionAccess.AllAccess, appConfig.cdnFilesPath);
                    //try
                    //{
                    //    f2.Demand();
                    //}
                    //catch (SecurityException s)
                    //{
                    //    Console.WriteLine(s.Message);
                    //}
                    ////
                    //// -- setup private
                    //System.IO.Directory.CreateDirectory(appConfig.privateFilesPath);
                    //f2 = new FileIOPermission(FileIOPermissionAccess.AllAccess, appConfig.privateFilesPath);
                    //try
                    //{
                    //    f2.Demand();
                    //}
                    //catch (SecurityException s)
                    //{
                    //    Console.WriteLine(s.Message);
                    //}
                    ////
                    //// -- setup temp
                    //System.IO.Directory.CreateDirectory(appConfig.tempFilesPath);
                    //f2 = new FileIOPermission(FileIOPermissionAccess.AllAccess, appConfig.tempFilesPath);
                    //try
                    //{
                    //    f2.Demand();
                    //}
                    //catch (SecurityException s)
                    //{
                    //    Console.WriteLine(s.Message);
                    //}

                    //
                    // -- save the app configuration and reload the server using this app
                    appConfig.appStatus = appConfigModel.appStatusEnum.building;
                    cp.core.serverConfig.apps.Add(appName, appConfig);
                    cp.core.serverConfig.saveObject(cp.core);
                    cp.core.serverConfig = serverConfigModel.getObject(cp.core);
                    cp.core.appConfig = appConfigModel.getObject(cp.core, cp.core.serverConfig, appName);
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
                        Console.Write("Please manually add the following line to your host file (c:\\windows\\system32\\drivers\\etc\\hosts):" + "127.0.0.1\t" + appName);
                    }
                    //
                    // create the database on the server
                    //
                    cp.core.dbServer.createCatalog(appName);
                    //
                    // copy in the pattern files 
                    //  - the only pattern is aspx
                    //  - this is clib running, so they are setting up new application which may or may not have a webrole here.
                    //  - setup a basic webrole just in case this will include one -- maybe later make it an option
                    //
                    cp.core.programFiles.copyFolder("resources\\iisDefaultSite\\", "\\", cp.core.appRootFiles);
                    //
                    // replace "appName" with blank to use iis siteName as appName, or the name of this app in the default document in the apps public folder
                    //
                    string defaultContent = cp.core.appRootFiles.readFile("web.config");
                    defaultContent = defaultContent.Replace("{{appName}}", appName);
                    cp.core.appRootFiles.saveFile("web.config", defaultContent);
                }
                //
                // initialize the new app, use the save authentication that was used to authorize this object
                //
                using (CPClass cp = new CPClass(appName))
                {
                    Core.Controllers.iisController.verifySite(cp.core,appName, domainName, cp.core.appConfig.appRootFilesPath, iisDefaultDoc);
                    Core.Controllers.appBuilderController.upgrade(cp.core,true);
                    //
                    // -- set the application back to normal mode
                    cp.core.serverConfig.apps[appName].appMode = appConfigModel.appModeEnum.normal;
                    cp.core.serverConfig.saveObject(cp.core);
                    cp.core.siteProperties.setProperty(constants.siteproperty_serverPageDefault_name, iisDefaultDoc);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: [" + ex.ToString() + "]");
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// create a folder and make it full access for everyone
        /// </summary>
        /// <param name="folderPathPage"></param>
        public void setupDirectory(string folderPathPage)
        {
            System.IO.Directory.CreateDirectory(folderPathPage);
            DirectoryInfo dInfo = new DirectoryInfo(folderPathPage);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
            // -- child objects were not given acces
            //dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
            // -- child objects were not given acces
            //dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
            //
            //
            //
            //DirectoryInfo dInfo = new DirectoryInfo(file);
            //DirectorySecurity dSecurity = dInfo.GetAccessControl();
            //dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            //dInfo.SetAccessControl(dSecurity);


        }
    }
}