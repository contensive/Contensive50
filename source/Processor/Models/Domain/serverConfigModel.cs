
using System;
using System.Collections.Generic;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Constants;

namespace Contensive.Processor.Models.Domain {
    //
    //====================================================================================================
    /// <summary>
    /// configuration of the server (on or more apps in the serer)
    /// -- new() - to allow deserialization (so all methods must pass in cp)
    /// -- shared getObject( cp, id ) - returns loaded model
    /// -- saveObject( cp ) - saves instance properties, returns the record id
    /// </summary>
    [Serializable]
    public class ServerConfigModel {
        //
        // -- public properties
        //
        // -- set during installation
        /// <summary>
        /// full dos path to the contensive program file installation. 
        /// </summary>
        public string programFilesPath { get; set; }
        //
        // -- old serverConfig
        public bool allowTaskRunnerService { get; set; }
        public bool allowTaskSchedulerService { get; set; }
        //
        // -- old clusterConfig
        public string name { get; set; }
        //
        // -- If true, use local dotnet memory cache backed by filesystem
        public bool enableLocalMemoryCache { get; set; }
        public bool enableLocalFileCache { get; set; }
        //
        // -- AWS dotnet elaticcache client wraps enyim, and provides node autodiscovery through the configuration object. this is the srver:port to the config file it uses.
        public bool enableRemoteCache { get; set; }
        public string awsElastiCacheConfigurationEndpoint { get; set; }
        //
        // -- datasource for the cluster (only sql support for now)
        public DataSourceModel.DataSourceTypeEnum defaultDataSourceType;
        public string defaultDataSourceAddress { get; set; }
        public string defaultDataSourceUsername { get; set; }
        public string defaultDataSourcePassword { get; set; }
        //
        // -- endpoint for cluster files (not sure how it works, maybe this will be an object taht includes permissions, for now an fpo)
        public bool isLocalFileSystem { get; set; }
        public string localDataDriveLetter { get; set; }
        public string awsAccessKey { get; set; }
        public string awsSecretAccessKey { get; set; }
        public string awsBucketRegionName { get; set; }
        public string awsBucketName { get; set; }
        //
        // -- configuration of async command listener on render machines (not sure if used still)
        public int serverListenerPort { get; set; }
        public int maxConcurrentTasksPerServer { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        //
        // -- used by applications to enable/disable features, like 
        //      - ecommerce batch should only run in production
        // todo figure out how to expose this, add it to configuration setup
        public bool productionEnvironment { get; set; }
        public bool enableLogging { get; set; }
        //
        // -- List of all apps on this server
        public CaseInsensitiveDictionary<string,AppConfigModel> apps { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// Create an empty object. needed for deserialization. Use newModel() method as constructor, includes cache
        /// </summary>
        public ServerConfigModel() {
            name = "";
            enableLocalMemoryCache = true;
            enableLocalFileCache = false;
            enableRemoteCache = false;
            awsElastiCacheConfigurationEndpoint = "";
            defaultDataSourceAddress = "";
            defaultDataSourceUsername = "";
            defaultDataSourcePassword = "";
            isLocalFileSystem = true;
            localDataDriveLetter = "D";
            serverListenerPort = Port_ContentServerControlDefault;
            maxConcurrentTasksPerServer = 5;
            username = "";
            password = "";
            productionEnvironment = true;
            enableLogging = false;
            apps = new CaseInsensitiveDictionary<string, AppConfigModel>();
        }
        //
        //====================================================================================================
        /// <summary>
        /// get ServerConfig, returning only the server data section without specific serverConfig.app
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>
        public static ServerConfigModel getObject(CoreController core) {
            ServerConfigModel returnModel = null;
            try {
                //
                // ----- read/create serverConfig
                var json_serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string JSONTemp = core.programDataFiles.readFileText("config.json");
                if (string.IsNullOrEmpty(JSONTemp)) {
                    //
                    // for now it fails, maybe later let it autobuild a local cluster
                    //
                    returnModel = new ServerConfigModel();
                    returnModel.allowTaskRunnerService = false;
                    returnModel.allowTaskSchedulerService = false;
                    core.programDataFiles.saveFile("config.json", json_serializer.Serialize(returnModel));
                } else {
                    returnModel = json_serializer.Deserialize<ServerConfigModel>(JSONTemp);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex, "exception in serverConfigModel.getObject");
            }
            return returnModel;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Save the object
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public int save(CoreController core) {
            try {
                string jsonTemp = core.json.Serialize(this);
                core.programDataFiles.saveFile("config.json", jsonTemp);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return 0;
        }
        //
        /// <summary>
        /// case insensative dictionary. Use for application lookup
        /// </summary>
        /// <typeparam name="V"></typeparam>
        public class CaseInsensitiveDictionary<S,V> : Dictionary<string, V> {
            public CaseInsensitiveDictionary() : base(StringComparer.OrdinalIgnoreCase) {
            }
        }
    }
}

