
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Contensive.Core;

namespace  Contensive.Core {
    class createAppClass {
        public void createApp() {
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
            string jsonTemp;
            string isLocalClusterText ;
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
            Console.Write("\n\nAuthentication token (" + authTokenDefault + "):");
            authToken = Console.ReadLine();
            if (string.IsNullOrEmpty(authToken)) {
                authToken = authTokenDefault;
            }
            cp = new CPClass("cluster-mode-not-implemented-yet");
            //
            if (!cp.clusterOk) {
                //
                // ----------------------------------------------------------------------------------------------------
                // create cluster
                //
                Console.WriteLine("ClusterConfig not found.");
                Console.Write("New Cluster Name:");
                clusterName = Console.ReadLine();
                cp.core.cluster.config.name = clusterName;
                //
                do {
                    Console.Write("Local Cluster (y/n)?");
                    isLocalClusterText = Console.ReadLine().ToLower();
                } while ((isLocalClusterText!="y")&&(isLocalClusterText!="n"));
                //
                do {
                    Console.Write("Drive letter for local cluster data storage (c/d/etc)?");
                    localDriveLetter = Console.ReadLine().ToLower();
                    drive = new System.IO.DriveInfo(localDriveLetter);
                    if (!drive.IsReady) {
                        Console.WriteLine("Drive " + localDriveLetter + " is not ready");
                    }
                } while (!drive.IsReady);
                cp.cluster.config.clusterPhysicalPath = localDriveLetter + ":\\inetpub\\";
                //
                switch (isLocalClusterText) {
                    case "y":
                        //
                        // create local cluster
                        //
                        isLocalCluster = true;
                        cp.cluster.config.isLocal = true;
                        Console.Write("DataSource type (1=odbcSqlServer, 2=nativeSqlServer):");
                        dataSource = Console.ReadLine();
                        switch (dataSource) {
                            case "1":
                                cp.cluster.config.defaultDataSourceType = Contensive.Core.dataSourceTypeEnum.sqlServerOdbc;
                                //
                                Console.Write("odbc sqlserver connectionstring:");
                                cp.cluster.config.defaultDataSourceODBCConnectionString = Console.ReadLine();
                                break;
                            case "2":
                                cp.cluster.config.defaultDataSourceType = dataSourceTypeEnum.sqlServerNative;
                                //
                                Console.Write("native sqlserver address:");
                                cp.cluster.config.defaultDataSourceAddress = Console.ReadLine();
                                //
                                Console.Write("native sqlserver userId:");
                                cp.cluster.config.defaultDataSourceUsername = Console.ReadLine();
                                //
                                Console.Write("native sqlserver password:");
                                cp.cluster.config.defaultDataSourcePassword = Console.ReadLine();
                                break;
                        }
                        break;
                    case "n":
                        isLocalCluster = false;
                        cp.cluster.config.defaultDataSourceType = dataSourceTypeEnum.sqlServerNative;
                        //
                        Console.Write("native sqlserver address:");
                        cp.cluster.config.defaultDataSourceAddress = Console.ReadLine();
                        //
                        Console.Write("native sqlserver userId:");
                        cp.cluster.config.defaultDataSourceUsername = Console.ReadLine();
                        //
                        Console.Write("native sqlserver password:");
                        cp.cluster.config.defaultDataSourcePassword = Console.ReadLine();
                        break;
                }
                do {
                    Console.Write("Use (l)ocal cache or (m)emcached (l/m)?");
                    isLocalClusterText = Console.ReadLine().ToLower();
                } while ((isLocalClusterText != "l") && (isLocalClusterText != "m"));
                isLocalCluster = (isLocalClusterText == "l");
                //
                // if memcached, get servers
                //
                do
                {
                    Console.Write("Enter the ElasticCache Configuration Endpoint (server:port):");
                    cacheNode = Console.ReadLine().ToLower();
                    cp.cluster.config.awsElastiCacheConfigurationEndpoint = cacheNode;
                } while (string.IsNullOrEmpty( cacheNode));
                //
                // determine app pattern
                //
                //
                int appPatternPtr;
                List<string> appPatterns = new List<string>();
                int appPatternCnt;
                Console.Write("\nCreate application within the cluster [" + cp.cluster.config.name + "].");
                do {
                    Console.Write("\n\nSelect an application pattern:\n");
                    appPatternCnt = 1;
                    appPatterns.Add("");
                    foreach (System.IO.DirectoryInfo di in cp.cluster.files.getFolders("clibResources\\appPatterns")) {
                        appPatterns.Add(di.Name);
                        Console.Write("\n" + appPatternCnt.ToString() + ") " + di.Name);
                        appPatternCnt += 1;
                    }
                    Console.Write("\n\n:");
                    string appPatternReply = Console.ReadLine();
                    appPatternPtr = cp.Utils.EncodeInteger(appPatternReply);
                } while ((appPatternPtr <= 0) || (appPatternPtr >= appPatternCnt));
                Console.Write("you picked -- " + appPatterns[appPatternPtr]);
                cp.cluster.config.appPattern = appPatterns[appPatternPtr].ToLower();
                //
                // create new clusterConfig file
                //
                cp.cluster.saveConfig();
                //
                // reload the cluster config and test connections
                //   
                cp.Dispose();
                cp = new CPClass("cluster-mode-not-implemented-yet");
            }
            //
            // ----------------------------------------------------------------------------------------------------
            // create app
            //
            switch (cp.cluster.config.appPattern)
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
            Console.Write("\n\rApplication Name (" + appNameDefault + "):");
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
            Console.Write("Primary Domain Name (www." + appName +  ".com):");
            domainName = Console.ReadLine();
            if ( string.IsNullOrEmpty( domainName ))  {
                domainName = "www." + appName +  ".com";
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
            appConfig.enableCache = false;
            appConfig.enabled = true;
            appConfig.name = appName.ToLower();
            appConfig.privateKey = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "");
            cp.cluster.config.apps.Add(appName.ToLower(), appConfig);
            //
            //
            //
            switch (appArchitecture)
            {
                case "1":
                    //
                    // Local Mode, compatible with v4.1, cdn in appRoot folder as /" + appName + "/files/
                    //
                    appConfig.appRootPath = "apps\\" + appName + "\\appRoot";
                    appConfig.cdnFilesPath = "apps\\" + appName + "\\appRoot\\" + appName + "\\files\\";
                    appConfig.privateFilesPath = "apps\\" + appName + "\\privateFiles";
                    appConfig.cdnFilesNetprefix = "/" + appName + "/files/";
                    cdnDomainName = domainName;
                    break;
                case "2":
                    //
                    // Local Mode, cdn in appRoot folder as /cdn/
                    //
                    appConfig.appRootPath = "apps\\" + appName + "\\appRoot";
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
                    appConfig.appRootPath = "apps\\" + appName + "\\appRoot";
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
                    appConfig.appRootPath = "apps\\" + appName + "\\appRoot";
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
            try{
                File.AppendAllText("c:\\windows\\system32\\drivers\\etc\\hosts", System.Environment.NewLine +  "127.0.0.1\t" +  appName);
            }
            catch(Exception ex){
                Console.Write("Error attempting to update local host file:" + ex.ToString() );
            }
            //
            // create the database on the server
            //
            cp.core.cluster.executeMasterSql("create database " + appName);
            //
            // copy in the pattern files
            //
            cp.core.cluster.files.copyFolder("clibResources\\appPatterns\\common\\", "apps\\" + appName + "\\");
            cp.core.cluster.files.copyFolder("clibResources\\appPatterns\\" + cp.core.cluster.config.appPattern + "\\", "apps\\" + appName + "\\");
            //
            // replace "appName" with the name of this app in the default document in the apps public folder
            //
            string defaultFile = "apps\\" + appName + "\\appRoot\\" + iisDefaultDoc;
            string defaultContent = cp.core.cluster.files.ReadFile(defaultFile);
            defaultContent = defaultContent.Replace("appName", appName);
            cp.core.cluster.files.SaveFile(defaultFile, defaultContent);
            cp.Dispose();
            //
            // initialize the new app, use the save authentication that was used to authorize this object
            //
            CPClass cpNewApp = new CPClass(appName);
            builderClass builder = new builderClass(cpNewApp.core);
            if (useIis)
            {
                builder.web_addSite(appName, domainName, "\\", iisDefaultDoc);
                if (domainName != cdnDomainName)
                {
                    builder.web_addSite(appName, cdnDomainName, "\\", iisDefaultDoc);
                }
            }
            builder.upgrade(true);
            cpNewApp.core.app.siteProperty_set(Contensive.Core.ccCommonModule.siteproperty_serverPageDefault_name, iisDefaultDoc);
            cpNewApp.Dispose();
        }
    }
}
