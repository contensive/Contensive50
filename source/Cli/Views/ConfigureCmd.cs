
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Amazon;

namespace Contensive.CLI {
    class ConfigureCmd {
        //
        // ====================================================================================================
        /// <summary>
        /// help text for this command
        /// </summary>
        internal static string  helpText = ""
            + Environment.NewLine
            + Environment.NewLine + "--configure"
            + Environment.NewLine + "    setup or review server configuration (Sql, cache, filesystem, etc)";

        //
        // ====================================================================================================
        public static void execute() {
            try {
                using (CPClass cp = new CPClass()) {
                    //
                    // -- Warning.
                    Console.WriteLine("\n\nThis server's configuration will be updated. If this is not correct, use Ctrl-C to exit.");
                    //
                    // -- serverGroup name
                    {
                        String prompt = "\nEnter the server group name (alpha-numeric string). For stand-alone servers, this can be the name of the server. For scaling configurations, this is a name for the group of servers:";
                        String defaultValue = cp.core.serverConfig.name;
                        cp.core.serverConfig.name = GenericController.promptForReply(prompt, defaultValue);
                    }
                    //
                    // -- production server?
                    {
                        Console.WriteLine("\n\nIs this instance a production server? Non-production server instances may disable or mock some services, like ecommerce billing or email notifications.");
                        String prompt = "Production Server (y/n)?";
                        String defaultValue = (cp.core.serverConfig.productionEnvironment) ? "y" : "n";
                        cp.core.serverConfig.productionEnvironment = Equals(GenericController.promptForReply(prompt, defaultValue).ToLowerInvariant(), "y");
                    }
                    ////
                    //// -- enable logging
                    //{
                    //    Console.WriteLine("\n\nWith logging disabled, the server log at c:\\ProgramData\\Contensive\\Logs only includes Error and Fatal level entries. With logging enabled, these logs are verbose and can be large.");
                    //    string prompt = "Enable Logging (y/n)?";
                    //    String defaultValue = (cp.core.serverConfig.enableLogging) ? "y" : "n";
                    //    cp.core.serverConfig.enableLogging = Equals(GenericController.promptForReply(prompt, defaultValue).ToLowerInvariant(), "y");
                    //}
                    //
                    // -- local or multiserver mode
                    {
                        Console.WriteLine("\n\nLocal or Remote File System");
                        Console.WriteLine("Local File System stores content files on the webserver. Remote File System store content in an Amazon AWS S3 bucket, using the webserver to cache files for read and write.");
                        String prompt = "Local File System (y/n)?";
                        String defaultValue =  (cp.core.serverConfig.isLocalFileSystem) ? "y" : "n";
                        cp.core.serverConfig.isLocalFileSystem = Equals(GenericController.promptForReply(prompt, defaultValue).ToLowerInvariant(), "y");
                    }
                    //
                    // -- local file location
                    {
                        Console.WriteLine("\n\nConfigure Local File System");
                        if (string.IsNullOrEmpty(cp.core.serverConfig.localDataDriveLetter)) cp.core.serverConfig.localDataDriveLetter = "d";
                        if (!(new System.IO.DriveInfo(cp.core.serverConfig.localDataDriveLetter).IsReady)) cp.core.serverConfig.localDataDriveLetter = "c";
                        cp.core.serverConfig.localDataDriveLetter = GenericController.promptForReply("Enter the Drive letter for data storage (c/d/etc)", cp.core.serverConfig.localDataDriveLetter);
                    }
                    //
                    // -- aws s3 bucket configure for non-local
                    if (!cp.core.serverConfig.isLocalFileSystem) {
                        //
                        Console.WriteLine("\n\nConfigure Remote File System");
                        string regionList = "";
                        foreach (var region in RegionEndpoint.EnumerableAllRegions) {
                            regionList += "," + region.SystemName;
                        }
                        regionList = regionList.Substring(1);
                        do {
                            string selectedRegion = GenericController.promptForReply("Enter the AWS bucket region (" + regionList + ")", cp.core.serverConfig.awsBucketRegionName).ToLowerInvariant();
                            cp.core.serverConfig.awsBucketRegionName = "";
                            foreach (var region in RegionEndpoint.EnumerableAllRegions) {
                                if (selectedRegion == region.SystemName.ToLowerInvariant()) {
                                    cp.core.serverConfig.awsBucketRegionName = region.SystemName;
                                    break;
                                }
                            }
                        } while (string.IsNullOrWhiteSpace(cp.core.serverConfig.awsBucketRegionName));
                        //
                        do {
                            cp.core.serverConfig.awsBucketName = GenericController.promptForReply("Enter the name of the AWS bucket for remote storage", cp.core.serverConfig.awsBucketName);
                        } while (string.IsNullOrWhiteSpace(cp.core.serverConfig.awsBucketName));
                        ////
                        //do {
                        //    cp.core.serverConfig.cdnFilesRemoteEndpoint = cliController.promptForReply("Enter the AWS bucket endpoint", cp.core.serverConfig.cdnFilesRemoteEndpoint);
                        //} while (string.IsNullOrWhiteSpace(cp.core.serverConfig.cdnFilesRemoteEndpoint));
                        //
                        do {
                            cp.core.serverConfig.awsAccessKey = GenericController.promptForReply("Enter the AWS Access Key", cp.core.serverConfig.awsAccessKey);
                        } while (string.IsNullOrWhiteSpace(cp.core.serverConfig.awsAccessKey));
                        //
                        do {
                            cp.core.serverConfig.awsSecretAccessKey = GenericController.promptForReply("Enter the AWS Access Secret", cp.core.serverConfig.awsSecretAccessKey);
                        } while (string.IsNullOrWhiteSpace(cp.core.serverConfig.awsSecretAccessKey));
                    }
                    //
                    // -- Sql Server Driver
                    cp.core.serverConfig.defaultDataSourceType = DataSourceModel.DataSourceTypeEnum.sqlServerNative;
                    //
                    // -- Sql Server end-point
                    {
                        Console.Write("\n\nSql Server endpoint. Use (local) for Sql Server on this machine, or the AWS RDS endpoint (url:port):");
                        if (!String.IsNullOrEmpty(cp.core.serverConfig.defaultDataSourceAddress)) Console.Write("(" + cp.core.serverConfig.defaultDataSourceAddress + ")");
                        string reply = Console.ReadLine();
                        if (String.IsNullOrEmpty(reply)) reply = cp.core.serverConfig.defaultDataSourceAddress;
                        cp.core.serverConfig.defaultDataSourceAddress = reply;
                    }
                    //
                    // -- Sql Server userId
                    {
                        Console.Write("native sqlserver userId:");
                        if (!String.IsNullOrEmpty(cp.core.serverConfig.defaultDataSourceUsername)) Console.Write("(" + cp.core.serverConfig.defaultDataSourceUsername + ")");
                        String reply = Console.ReadLine();
                        if (String.IsNullOrEmpty(reply)) reply = cp.core.serverConfig.defaultDataSourceUsername;
                        cp.core.serverConfig.defaultDataSourceUsername = reply;
                    }
                    //
                    // -- Sql Server password
                    {
                        Console.Write("native sqlserver password:");
                        if (!String.IsNullOrEmpty(cp.core.serverConfig.defaultDataSourcePassword)) Console.Write("(" + cp.core.serverConfig.defaultDataSourcePassword + ")");
                        String reply = Console.ReadLine();
                        if (String.IsNullOrEmpty(reply)) reply = cp.core.serverConfig.defaultDataSourcePassword;
                        cp.core.serverConfig.defaultDataSourcePassword = reply;
                    }
                    //
                    // -- cache server local or remote
                    {
                        String reply;
                        string defaultCacheValue = (String.IsNullOrEmpty(cp.core.serverConfig.awsElastiCacheConfigurationEndpoint)) ? "l" : "m";
                        do {
                            Console.WriteLine("\n\nThe server requires a caching service. You can choose either the systems local cache or an AWS Elasticache (memCacheD).");
                            Console.WriteLine("Use (l)ocal cache or (m)emcached server?");
                            Console.Write("(" + defaultCacheValue + ")");
                            reply = Console.ReadLine().ToLowerInvariant();
                            if (String.IsNullOrEmpty(reply)) reply = defaultCacheValue;
                        } while ((reply != "l") && (reply != "m"));
                        if ((reply == "l")) {
                            //
                            // -- local memory cache
                            cp.core.serverConfig.enableLocalFileCache = false;
                            cp.core.serverConfig.enableLocalMemoryCache = true;
                            cp.core.serverConfig.enableRemoteCache = false;
                            //cp.core.serverConfig.awsElastiCacheConfigurationEndpoint = "";
                        } else {
                            //
                            // -- remote mcached cache
                            cp.core.serverConfig.enableLocalFileCache = false;
                            cp.core.serverConfig.enableLocalMemoryCache = false;
                            cp.core.serverConfig.enableRemoteCache = true;
                            do {
                                Console.WriteLine("Enter the ElasticCache Configuration Endpoint (server:port):");
                                if (!String.IsNullOrEmpty(cp.core.serverConfig.awsElastiCacheConfigurationEndpoint)) Console.Write("(" + cp.core.serverConfig.awsElastiCacheConfigurationEndpoint + ")");
                                reply = Console.ReadLine();
                                if (String.IsNullOrEmpty(reply)) reply = cp.core.serverConfig.awsElastiCacheConfigurationEndpoint;
                                cp.core.serverConfig.awsElastiCacheConfigurationEndpoint = reply;
                            } while (string.IsNullOrEmpty(reply));
                            //
                            // -- enableEnyimNLog
                            {
                                Console.WriteLine("\nEnable Remote cache drive logging (Enyim Logging). This is helpful as a diagnostic but is a seriou performance hit.");
                                string prompt = "Enable Logging (y/n)?";
                                String defaultLoggingValue = (cp.core.serverConfig.enableEnyimNLog) ? "y" : "n";
                                cp.core.serverConfig.enableEnyimNLog = Equals(GenericController.promptForReply(prompt, defaultLoggingValue).ToLowerInvariant(), "y");
                            }
                        }
                    }
                    //
                    // -- tasks and logging
                    cp.core.serverConfig.allowTaskRunnerService = true;
                    cp.core.serverConfig.allowTaskSchedulerService = true;
                    //cp.core.serverConfig.enableLogging = true;
                    //
                    // -- save the configuration
                    cp.core.serverConfig.save(cp.core);
                }
            } catch (Exception ex) {
                Console.WriteLine("Error: [" + ex.ToString() + "]");
            }
        }
    }
}
