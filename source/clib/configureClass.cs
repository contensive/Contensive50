
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Contensive.Core;

namespace  Contensive.Core {
    class configureClass {
        public static void configure() {
            try
            {
                //
                // create cp for cluster work, with no application
                //
                CPClass cp;
                //
                // if you get a cluster object from cp with a key, and the key gives you access, you have a cluster object to create an app
                //
                string isLocalClusterText;
                string clusterName = "";
                bool isLocalCluster;
                string localDriveLetter;
                string cacheNode;
                System.IO.DriveInfo drive;
                string authToken;
                string authTokenDefault = "909903";
                System.Web.Script.Serialization.JavaScriptSerializer json = new System.Web.Script.Serialization.JavaScriptSerializer();
                authToken = authTokenDefault;
                //
                cp = new CPClass();
                if (cp.serverOk)
                {
                    Console.WriteLine("Cluster Configuration file [" + cp.core.programDataFiles.rootLocalPath + "config.json] found.");
                    Console.WriteLine("appPattern: " + cp.core.serverConfig.appPattern);
                    Console.WriteLine("apps.Count: " + cp.core.serverConfig.apps.Count);
                    foreach (KeyValuePair<string, Models.Entity.serverConfigModel.appConfigModel> kvp in cp.core.serverConfig.apps)
                    {
                        Console.WriteLine("\tapp: " + kvp.Key);
                    }
                    Console.WriteLine("ElastiCacheConfigurationEndpoint: " + cp.core.serverConfig.awsElastiCacheConfigurationEndpoint);
                    Console.WriteLine("FilesEndpoint: " + cp.core.serverConfig.cdnFilesRemoteEndpoint);
                    Console.WriteLine("defaultDataSourceAddress: " + cp.core.serverConfig.defaultDataSourceAddress);
                    Console.WriteLine("isLocal: " + cp.core.serverConfig.isLocalFileSystem.ToString());
                    Console.WriteLine("name: " + cp.core.serverConfig.name);
                }
                else {
                    //
                    // ----------------------------------------------------------------------------------------------------
                    // create cluster - Name
                    //
                    Console.WriteLine("The Cluster Configuration file [c:\\ProgramData\\Contensive\\config.json] was not found.");
                    Console.WriteLine("This server's cluster configuration will be initialized. If this is not correct, use Ctrl-C to stop this initialization.");
                    Console.Write("Enter the new cluster name (alpha-numeric string):");
                    clusterName = Console.ReadLine();
                    cp.core.serverConfig.name = clusterName;
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
                    //cp.core.serverConfig.clusterPath = localDriveLetter + ":\\inetpub\\";
                    //
                    switch (isLocalClusterText.ToLower())
                    {
                        case "y":
                            //
                            // ----------------------------------------------------------------------------------------------------
                            // LOCAL MODE Data Source Location
                            //
                            isLocalCluster = true;
                            cp.core.serverConfig.isLocalFileSystem = true;
                            //
                            // ----------------------------------------------------------------------------------------------------
                            // Native Sql Server Driver
                            //
                            cp.core.serverConfig.defaultDataSourceType = dataSourceTypeEnum.sqlServerNative;
                            //
                            Console.Write("\n\nSql Server endpoint. Use (local) for Sql Server on this machine, or the AWS RDS endpoint (url:port):");
                            cp.core.serverConfig.defaultDataSourceAddress = Console.ReadLine();
                            //
                            Console.Write("native sqlserver userId:");
                            cp.core.serverConfig.defaultDataSourceUsername = Console.ReadLine();
                            //
                            Console.Write("native sqlserver password:");
                            cp.core.serverConfig.defaultDataSourcePassword = Console.ReadLine();
                            break;
                        case "n":
                            //
                            // ----------------------------------------------------------------------------------------------------
                            // non-LOCAL MODE Data Source Location
                            //
                            isLocalCluster = false;
                            cp.core.serverConfig.defaultDataSourceType = dataSourceTypeEnum.sqlServerNative;
                            //
                            Console.Write("\n\nSql Server endpoint. Use the AWS RDS endpoint (url:port):");
                            cp.core.serverConfig.defaultDataSourceAddress = Console.ReadLine();
                            //
                            Console.Write("native sqlserver userId:");
                            cp.core.serverConfig.defaultDataSourceUsername = Console.ReadLine();
                            //
                            Console.Write("native sqlserver password:");
                            cp.core.serverConfig.defaultDataSourcePassword = Console.ReadLine();
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
                            cp.core.serverConfig.awsElastiCacheConfigurationEndpoint = cacheNode;
                        } while (string.IsNullOrEmpty(cacheNode));
                    }
                    coreFileSystemClass installFiles = new coreFileSystemClass(cp.core,  cp.core.serverConfig.isLocalFileSystem, coreFileSystemClass.fileSyncModeEnum.noSync, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
                    //
                    // create new serverConfig file
                    //
                    cp.core.serverConfig.saveObject(cp.core) ;
                    //
                    // reload the cluster config and test connections
                    //   
                    cp.Dispose();
                    cp = new CPClass();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: [" + ex.ToString() + "]");
            }
        }
    }
}
