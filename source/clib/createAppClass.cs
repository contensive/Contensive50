
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
                // create cp for cluster work, with no application
                //
                CPClass cp;
                //
                // if you get a cluster object from cp with a key, and the key gives you access, you have a cluster object to create an app
                //
                string dataSource;
                //string sql;
                string appName;
                string domainName;
                //string jsonTemp;
                string isLocalClusterText;
                string clusterName = "";
                bool isLocalCluster;
                string localDriveLetter;
                //bool localDriveIsReady;
                string cacheNode;
                bool useIis = false;
                string iisDefaultDoc = "";
                System.IO.DriveInfo drive;
                string authToken;
                string authTokenDefault = "909903";
                string appArchitecture = "";
                string cdnDomainName = "";
                //
                System.Web.Script.Serialization.JavaScriptSerializer json = new System.Web.Script.Serialization.JavaScriptSerializer();
                //
                //Console.WriteLine("Authentication token");
                //Console.WriteLine("This alphanumeric string will be used to encode data within the applications on this server.");
                //Console.Write("\n\nAuthentication token (" + authTokenDefault + "):");
                //authToken = Console.ReadLine();
                //if (string.IsNullOrEmpty(authToken)) {
                authToken = authTokenDefault;
                //}
                cp = new CPClass();
                //
                if (cp.clusterOk)
                {
                    Console.WriteLine("Cluster Configuration file [c:\\ProgramData\\Clib\\clusterConfig.json] found.");
                    Console.WriteLine("appPattern: " + cp.core.clusterConfig.appPattern);
                    Console.WriteLine("apps.Count: " + cp.core.clusterConfig.apps.Count);
                    foreach (KeyValuePair<string, appConfigClass> kvp in cp.core.clusterConfig.apps)
                    {
                        Console.WriteLine("\tapp: " + kvp.Key);
                    }
                    Console.WriteLine("ElastiCacheConfigurationEndpoint: " + cp.core.clusterConfig.awsElastiCacheConfigurationEndpoint);
                    Console.WriteLine("FilesEndpoint: " + cp.core.clusterConfig.clusterFilesEndpoint);
                    Console.WriteLine("defaultDataSourceAddress: " + cp.core.clusterConfig.defaultDataSourceAddress);
                    Console.WriteLine("isLocal: " + cp.core.clusterConfig.isLocal.ToString());
                    Console.WriteLine("name: " + cp.core.clusterConfig.name);
                }
                else {
                    //
                    // ----------------------------------------------------------------------------------------------------
                    // create cluster - Name
                    //
                    Console.WriteLine("The Cluster Configuration file [c:\\ProgramData\\Clib\\clusterConfig.json] was not found.");
                    Console.WriteLine("This server's cluster configuration will be initialized. If this is not correct, use Ctrl-C to stop this initialization.");
                    Console.Write("Enter the new cluster name (alpha-numeric string):");
                    clusterName = Console.ReadLine();
                    cp.core.clusterConfig.name = clusterName;
                    //
                    do
                    {
                        //
                        // ----------------------------------------------------------------------------------------------------
                        // local or multiserver mode
                        //
                        Console.WriteLine("\n\nSingle-Server or Multi-Server Mode");
                        Console.WriteLine("Single server installations run applications from a single server and store their data on that machine. Multi-server configurations run on multiple servers and require outside resources to store their data.");
                        Console.Write("Single-Server Application (y/n)?");
                        isLocalClusterText = Console.ReadLine().ToLower();
                    } while ((isLocalClusterText != "y") && (isLocalClusterText != "n"));
                    //
                    do
                    {
                        //
                        // ----------------------------------------------------------------------------------------------------
                        // files
                        //
                        Console.WriteLine("\n\nData Storage Location");
                        Console.WriteLine("Data will be stored on the server in the \\InetPub folder. This folder must be backed up regularly.");
                        Console.Write("Enter the Drive letter for data storage (c/d/etc)?");
                        localDriveLetter = Console.ReadLine().ToLower();
                        drive = new System.IO.DriveInfo(localDriveLetter);
                        if (!drive.IsReady)
                        {
                            Console.WriteLine("Drive " + localDriveLetter + " is not ready");
                        }
                    } while (!drive.IsReady);
                    cp.core.serverConfig.clusterPath = localDriveLetter + ":\\inetpub\\";
                    //
                    switch (isLocalClusterText.ToLower())
                    {
                        case "y":
                            //
                            // ----------------------------------------------------------------------------------------------------
                            // LOCAL MODE Data Source Location
                            //
                            isLocalCluster = true;
                            cp.core.clusterConfig.isLocal = true;
                            Console.WriteLine("\n\nDataSource type (1=odbc Sql Server, 2=native Sql Server):");
                            dataSource = Console.ReadLine();
                            switch (dataSource)
                            {
                                case "1":
                                    //
                                    // ----------------------------------------------------------------------------------------------------
                                    // ODBC
                                    //
                                    cp.core.clusterConfig.defaultDataSourceType = Contensive.Core.dataSourceTypeEnum.sqlServerOdbc;
                                    //
                                    Console.Write("\n\nodbc sqlserver connectionstring:");
                                    cp.core.clusterConfig.defaultDataSourceODBCConnectionString = Console.ReadLine();
                                    break;
                                case "2":
                                    //
                                    // ----------------------------------------------------------------------------------------------------
                                    // Native Sql Server Driver
                                    //
                                    cp.core.clusterConfig.defaultDataSourceType = dataSourceTypeEnum.sqlServerNative;
                                    //
                                    Console.Write("\n\nSql Server endpoint. Use (local) for Sql Server on this machine, or the AWS RDS endpoint (url:port):");
                                    cp.core.clusterConfig.defaultDataSourceAddress = Console.ReadLine();
                                    //
                                    Console.Write("native sqlserver userId:");
                                    cp.core.clusterConfig.defaultDataSourceUsername = Console.ReadLine();
                                    //
                                    Console.Write("native sqlserver password:");
                                    cp.core.clusterConfig.defaultDataSourcePassword = Console.ReadLine();
                                    break;
                            }
                            break;
                        case "n":
                            //
                            // ----------------------------------------------------------------------------------------------------
                            // non-LOCAL MODE Data Source Location
                            //
                            isLocalCluster = false;
                            cp.core.clusterConfig.defaultDataSourceType = dataSourceTypeEnum.sqlServerNative;
                            //
                            Console.Write("\n\nSql Server endpoint. Use the AWS RDS endpoint (url:port):");
                            cp.core.clusterConfig.defaultDataSourceAddress = Console.ReadLine();
                            //
                            Console.Write("native sqlserver userId:");
                            cp.core.clusterConfig.defaultDataSourceUsername = Console.ReadLine();
                            //
                            Console.Write("native sqlserver password:");
                            cp.core.clusterConfig.defaultDataSourcePassword = Console.ReadLine();
                            break;
                    }
                    do
                    {
                        Console.WriteLine("\n\nThe server requires a caching service. You can choose either the systems local cache or an AWS Elasticache (memCacheD).");
                        Console.WriteLine("NOTE: local cache is not yet implemented. if you select local cache will be disabled.");
                        Console.Write("Use (l)ocal cache or (m)emcached (l/m)?");
                        isLocalClusterText = Console.ReadLine().ToLower();
                    } while ((isLocalClusterText != "l") && (isLocalClusterText != "m"));
                    isLocalCluster = (isLocalClusterText == "l");
                    //
                    // if memcached, get servers
                    //
                    if ((isLocalClusterText == "m"))
                    {
                        do
                        {
                            Console.Write("\n\nEnter the ElasticCache Configuration Endpoint (server:port):");
                            cacheNode = Console.ReadLine().ToLower();
                            cp.core.clusterConfig.awsElastiCacheConfigurationEndpoint = cacheNode;
                        } while (string.IsNullOrEmpty(cacheNode));
                    }
                    //
                    // determine app pattern
                    //
                    //
                    coreFileSystemClass installFiles = new coreFileSystemClass(cp.core,  cp.core.clusterConfig.isLocal, coreFileSystemClass.fileSyncModeEnum.noSync, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
                    if (!cp.core.cluster.localClusterFiles.checkPath("clibCommonAssemblies\\"))
                    {
                        cp.core.cluster.localClusterFiles.createPath("clibCommonAssemblies\\");
                    }
                    int appPatternPtr;
                    List<string> appPatterns = new List<string>();
                    int appPatternCnt;
                    //
                    // upgrade the cluster resources folder from the installation
                    //
                    upgradeResources(cp, installFiles);
                    string appPatternsSrc = "clibResources\\appPatterns";
                    do
                    {
                        Console.WriteLine("\n\nApplication pattern for this server.");
                        Console.WriteLine("All applications in this cluster must be the same type (aspx, etc.)");
                        Console.WriteLine("Select one of the application patterns setup in the installation folder:");
                        appPatternCnt = 1;
                        appPatterns.Add("");
                        foreach (System.IO.DirectoryInfo di in cp.core.cluster.localClusterFiles.getFolders(appPatternsSrc))
                        {
                            appPatterns.Add(di.Name);
                            Console.Write("\n" + appPatternCnt.ToString() + ") " + di.Name);
                            appPatternCnt += 1;
                        }
                        Console.Write("\n\n:");
                        string appPatternReply = Console.ReadLine();
                        appPatternPtr = cp.Utils.EncodeInteger(appPatternReply);
                    } while ((appPatternPtr <= 0) || (appPatternPtr >= appPatternCnt));
                    Console.Write("you picked -- " + appPatterns[appPatternPtr]);
                    cp.core.clusterConfig.appPattern = appPatterns[appPatternPtr].ToLower();
                    //
                    // create new clusterConfig file
                    //
                    cp.cluster.saveConfig();
                    //
                    // reload the cluster config and test connections
                    //   
                    cp.Dispose();
                    cp = new CPClass();
                }
                //
                // ----------------------------------------------------------------------------------------------------
                // create app
                //
                Console.Write("\n\nCreate application within the cluster [" + cp.core.clusterConfig.name + "].");
                switch (cp.core.clusterConfig.appPattern)
                {
                    case "php":
                        useIis = true;
                        iisDefaultDoc = "index.php";
                        break;
                    case "iismodule":
                        useIis = true;
                        iisDefaultDoc = "index.html";
                        break;
                    case "aspnet":
                        useIis = true;
                        iisDefaultDoc = "default.aspx";
                        break;
                }
                DateTime rightNow = DateTime.Now;
                //
                // app name
                //
                string appNameDefault = "app" + rightNow.Year + rightNow.Month.ToString().PadLeft(2, '0') + rightNow.Day.ToString().PadLeft(2, '0') + rightNow.Hour.ToString().PadLeft(2, '0') + rightNow.Minute.ToString().PadLeft(2, '0') + rightNow.Second.ToString().PadLeft(2, '0');
                Console.Write("\n\nApplication Name (" + appNameDefault + "):");
                appName = Console.ReadLine();
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
                appConfigClass appConfig = new appConfigClass();
                appConfig.adminRoute = "/admin/";
                appConfig.allowSiteMonitor = false;
                appConfig.defaultConnectionString = "";
                appConfig.domainList.Add(domainName);
                appConfig.enableCache = true;
                appConfig.enabled = true;
                appConfig.name = appName.ToLower();
                appConfig.privateKey = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "");
                cp.core.clusterConfig.apps.Add(appName.ToLower(), appConfig);
                //
                //
                //
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
                cp.cluster.saveConfig();
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
                cp.core.cluster.db_executeSql("create database " + appName);
                //
                // copy in the pattern files
                //
                cp.core.cluster.localClusterFiles.copyFolder("clibResources\\appPatterns\\common\\", "apps\\" + appName + "\\");
                cp.core.cluster.localClusterFiles.copyFolder("clibResources\\appPatterns\\" + cp.core.clusterConfig.appPattern + "\\", "apps\\" + appName + "\\");
                //
                // replace "appName" with the name of this app in the default document in the apps public folder
                //
                string defaultFile = "apps\\" + appName + "\\appRoot\\" + iisDefaultDoc;
                string defaultContent = cp.core.cluster.localClusterFiles.ReadFile(defaultFile);
                defaultContent = defaultContent.Replace("appName", appName);
                cp.core.cluster.localClusterFiles.SaveFile(defaultFile, defaultContent);
                cp.Dispose();
                //
                // initialize the new app, use the save authentication that was used to authorize this object
                //
                CPClass cpNewApp = new CPClass(appName);
                coreBuilderClass builder = new coreBuilderClass(cpNewApp.core);
                if (useIis)
                {
                    builder.web_addSite(appName, domainName, "\\", iisDefaultDoc);
                    if (domainName != cdnDomainName)
                    {
                        builder.web_addSite(appName, cdnDomainName, "\\", iisDefaultDoc);
                    }
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
        //
        //====================================================================================================
        /// <summary>
        /// refresh the clibResources in the cluster folder from the files installed
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="installFiles"></param>
        public void upgradeResources(CPClass cp, coreFileSystemClass installFiles)
        {
            //string appPatternsSrc = "clibResources\\appPatterns";
            //if (!cp.core.cluster.clusterFiles.checkPath(appPatternsSrc))
            //{
                //
                // unpack the install resources
                //
                if (installFiles.checkFile("clibresources.zip"))
                {
                    installFiles.copyFile("clibresources.zip", "clibresources.zip", cp.core.cluster.localClusterFiles);
                    cp.core.cluster.localClusterFiles.UnzipFile("clibresources.zip");
                    cp.core.cluster.localClusterFiles.DeleteFile("clibresources.zip");
                };
            //}

        }
    }
}
