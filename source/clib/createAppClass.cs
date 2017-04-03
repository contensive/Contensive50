
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Contensive.Core;

namespace  Contensive.Core {
    class createAppClass {
        public void createApp() {
            try
            {
                //
                // -- create cp for cluster work, with no application
                CPClass cp;
                //
                // -- if you get a cluster object from cp with a key, and the key gives you access, you have a cluster object to create an app
                string appName;
                string domainName;
                string iisDefaultDoc = "";
                string authToken;
                string authTokenDefault = "909903";
                string appArchitecture = "";
                string cdnDomainName = "";
                System.Web.Script.Serialization.JavaScriptSerializer json = new System.Web.Script.Serialization.JavaScriptSerializer();
                authToken = authTokenDefault;
                //
                cp = new CPClass();
                if (!cp.serverOk)
                {
                    Console.WriteLine("Server Configuration not loaded correctly. Please run --configure");
                    return;
                }
                else {
                    // -- server configuration ok
                }
                //
                // ----------------------------------------------------------------------------------------------------
                // create app
                //
                Console.Write("\n\nCreate application within the server group [" + cp.core.serverConfig.name + "].");
                iisDefaultDoc = "default.aspx";
                DateTime rightNow = DateTime.Now;
                //
                // app name
                //
                string appNameDefault = "app" + rightNow.Year + rightNow.Month.ToString().PadLeft(2, '0') + rightNow.Day.ToString().PadLeft(2, '0') + rightNow.Hour.ToString().PadLeft(2, '0') + rightNow.Minute.ToString().PadLeft(2, '0') + rightNow.Second.ToString().PadLeft(2, '0');
                Console.Write("\n\nApplication Name (" + appNameDefault + "):");
                appName = Console.ReadLine();
                if (cp.core.serverConfig.apps.ContainsKey(appName.ToLower()))
                {
                    Console.Write("\n\nThere is already an application with this name. To get the current server configuration, use clib -s");
                }
                if (string.IsNullOrEmpty(appName))
                {
                    appName = appNameDefault;
                }
                //
                // app mode (local-compatibility/local/scale
                //
                Console.Write("\n\rApplication Architecture");
                Console.Write("\n\r\t1 Local Mode, compatible with v4.1, cdn in appRoot folder as /" + appName + "/files/");
                Console.Write("\n\r\t2 Local Mode, cdn in appRoot folder as /cdn/");
                Console.Write("\n\r\t3 Local Mode, cdn as second iis site as cdn." + appName);
                Console.Write("\n\r\t4 Scale Mode, cdn as AWS S3 bucket, privateFiles as AWS S3 bucket");
                Console.Write("\n\rSelect 1,2,3, or 4 (1):");
                appArchitecture = Console.ReadLine();
                if (string.IsNullOrEmpty(appArchitecture))
                {
                    appArchitecture = "1";
                }
                //
                // domain
                //
                Console.Write("Primary Domain Name (www." + appName + ".com):");
                domainName = Console.ReadLine();
                if (string.IsNullOrEmpty(domainName))
                {
                    domainName = "www." + appName + ".com";
                }
                //
                // setup application config
                //
                //string jsonText;
                Models.Entity.serverConfigModel.appConfigModel appConfig = new Models.Entity.serverConfigModel.appConfigModel();
                appConfig.adminRoute = "/admin/";
                appConfig.allowSiteMonitor = false;
                appConfig.defaultConnectionString = "";
                appConfig.domainList.Add(domainName);
                appConfig.enableCache = true;
                appConfig.enabled = true;
                appConfig.name = appName.ToLower();
                appConfig.privateKey = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "");
                switch (appArchitecture)
                {
                    case "1":
                        //
                        // Local Mode, compatible with v4.1, cdn in appRoot folder as /" + appName + "/files/
                        //
                        appConfig.appRootFilesPath = "apps\\" + appName + "\\appRoot";
                        appConfig.cdnFilesPath = "apps\\" + appName + "\\appRoot\\" + appName + "\\files\\";
                        appConfig.privateFilesPath = "apps\\" + appName + "\\privateFiles";
                        appConfig.cdnFilesNetprefix = "/" + appName + "/files/";
                        cdnDomainName = domainName;
                        break;
                    case "2":
                        //
                        // Local Mode, cdn in appRoot folder as /cdn/
                        //
                        appConfig.appRootFilesPath = "apps\\" + appName + "\\appRoot";
                        appConfig.cdnFilesPath = "apps\\" + appName + "\\appRoot\\cdn\\";
                        appConfig.privateFilesPath = "apps\\" + appName + "\\privateFiles";
                        appConfig.cdnFilesNetprefix = "/cdn/";
                        cdnDomainName = domainName;
                        break;
                    case "3":
                        //
                        // 3 Local Mode, cdn as second iis site as cdn." + appName
                        //
                        Console.Write("CDN Domain Name (cdn." + appName + ".com):");
                        cdnDomainName = Console.ReadLine();
                        if (string.IsNullOrEmpty(cdnDomainName))
                        {
                            cdnDomainName = "cdn." + appName + ".com";
                        }
                        appConfig.appRootFilesPath = "apps\\" + appName + "\\appRoot";
                        appConfig.cdnFilesPath = "apps\\" + appName + "\\cdnFiles";
                        appConfig.privateFilesPath = "apps\\" + appName + "\\privateFiles";
                        appConfig.cdnFilesNetprefix = cdnDomainName + "\\";
                        break;
                    case "4":
                        //
                        // 4 Scale Mode, cdn as AWS S3 bucket, privateFiles as AWS S3 bucket"
                        //
                        Console.Write("CDN Domain Name (cdn." + appName + ".com):");
                        cdnDomainName = Console.ReadLine();
                        if (string.IsNullOrEmpty(cdnDomainName))
                        {
                            cdnDomainName = "cdn." + appName + ".com";
                        }
                        appConfig.appRootFilesPath = "apps\\" + appName + "\\appRoot";
                        appConfig.cdnFilesPath = "apps\\" + appName + "\\cdnFiles";
                        appConfig.privateFilesPath = "apps\\" + appName + "\\privateFiles";
                        appConfig.cdnFilesNetprefix = cdnDomainName + "\\";
                        Console.Write("\nLocal cdn mirror = " + appConfig.cdnFilesPath);
                        Console.Write("\nLocal privateFiles mirror = " + appConfig.privateFilesPath);
                        Console.Write("\nAWS S3 configuration must be setup manually for cdn and privateFiles.");
                        Console.Write("\n\npress a key to continue.");
                        cdnDomainName = Console.ReadLine();
                        break;
                }
                //
                // -- save the app configuration and reload the server using this app
                cp.core.serverConfig.apps.Add(appName, appConfig);
                cp.core.serverConfig.saveObject(cp.core);
                cp.core.serverConfig = Models.Entity.serverConfigModel.getObject(cp.core, appName);
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
                cp.core.db.createCatalog(appName);
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
                string defaultFile = "apps\\" + appName + "\\appRoot\\" + iisDefaultDoc;
                string defaultContent = cp.core.appRootFiles.readFile("default.aspx");
                defaultContent = defaultContent.Replace("ReplaceWithAppName", appName);
                cp.core.appRootFiles.saveFile(defaultFile, defaultContent);
                cp.Dispose();
                //
                // initialize the new app, use the save authentication that was used to authorize this object
                //
                CPClass cpNewApp = new CPClass(appName);
                coreBuilderClass builder = new coreBuilderClass(cpNewApp.core);
                builder.web_addSite(appName, domainName, "\\", iisDefaultDoc);
                if (domainName != cdnDomainName)
                {
                    builder.web_addSite(appName, cdnDomainName, "\\", iisDefaultDoc);
                }
                builder.upgrade(true);
                cpNewApp.core.siteProperties.setProperty(Contensive.Core.coreCommonModule.siteproperty_serverPageDefault_name, iisDefaultDoc);
                cpNewApp.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: [" + ex.ToString() + "]");
            }
        }
    }
}
