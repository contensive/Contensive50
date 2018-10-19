
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
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.Domain {
    //
    //====================================================================================================
    /// <summary>
    /// configuration of the server (on or more apps in the serer)
    /// -- new() - to allow deserialization (so all methods must pass in cp)
    /// -- shared getObject( cp, id ) - returns loaded model
    /// -- saveObject( cp ) - saves instance properties, returns the record id
    /// </summary>
    [Serializable()]
    public class ServerConfigModel {
        //
        // -- public properties
        //
        // -- set during installation
        /// <summary>
        /// full dos path to the contensive program file installation. 
        /// </summary>
        public string programFilesPath;
        //
        // -- old serverConfig
        //Public clusterPath As String
        public bool allowTaskRunnerService;
        public bool allowTaskSchedulerService;
        //
        // -- old clusterConfig
        public string name = "";
        //
        // -- If true, use local dotnet memory cache backed by filesystem
        public bool enableLocalMemoryCache = true;
        public bool enableLocalFileCache = false;
        //
        // -- AWS dotnet elaticcache client wraps enyim, and provides node autodiscovery through the configuration object. this is the srver:port to the config file it uses.
        public bool enableRemoteCache = false;
        public string awsElastiCacheConfigurationEndpoint = "";
        //
        // -- datasource for the cluster (only sql support for now)
        public DataSourceModel.DataSourceTypeEnum defaultDataSourceType;
        public string defaultDataSourceAddress = "";
        public string defaultDataSourceUsername = "";
        public string defaultDataSourcePassword = "";
        //
        // -- endpoint for cluster files (not sure how it works, maybe this will be an object taht includes permissions, for now an fpo)
        public bool isLocalFileSystem = true;
        public string localDataDriveLetter = "D";
        //public string cdnFilesRemoteEndpoint;
        public string awsAccessKey;
        public string awsSecretAccessKey;
        public string awsBucketRegionName;
        public string awsBucketName;
        //
        // -- configuration of async command listener on render machines (not sure if used still)
        public int serverListenerPort = Port_ContentServerControlDefault;
        public int maxConcurrentTasksPerServer = 5;
        public string username = "";
        public string password = "";
        //
        // -- used by applications to enable/disable features, like 
        //      - ecommerce batch should only run in production
        // todo figure out how to expose this, add it to configuration setup
        public bool productionEnvironment = true;
        public bool enableLogging = false;
        //
        // -- deprecated
        //Public appPattern As String
        //
        // -- List of all apps on this server
        public CaseInsensitiveDictionary<string,AppConfigModel> apps = new CaseInsensitiveDictionary<string, AppConfigModel>();
        //
        //====================================================================================================
        /// <summary>
        /// Create an empty object. needed for deserialization. Use newModel() method as constructor, includes cache
        /// </summary>
        public ServerConfigModel() {}
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
                System.Web.Script.Serialization.JavaScriptSerializer json_serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string JSONTemp;
                //
                // ----- read/create serverConfig
                //
                JSONTemp = core.programDataFiles.readFileText("config.json");
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
        ////
        ////====================================================================================================
        ///// <summary>
        ///// get the server configuration and assign an application to the appConf
        ///// </summary>
        ///// <param name="cp"></param>
        ///// <param name="recordId"></param>
        //public static Models.Domain.serverConfigModel getObject(coreController core, string appName) {
        //    serverConfigModel returnModel = null;
        //    try {
        //        returnModel = getObject(core);
        //        if (!string.IsNullOrEmpty(appName)) {
        //            if (!returnModel.apps.ContainsKey(appName.ToLower())) {
        //                //
        //                // -- application not configured
        //                returnModel.appConfig = null;
        //                throw new Exception("application [" + appName + "] was not found in this server group.");
        //            } else {
        //                returnModel.appConfig = returnModel.apps[appName.ToLower()];
        //                //
        //                // -- no, leave the status setup with the last status saved -- there is not status that describes how it is running, because there is not server. This is the status of the configuration, OK or building
        //                // -- build is set when the app is created, and OK is set at the end of upgrade
        //                //returnModel.appConfig.appStatus = appStatusEnum.OK
        //            }
        //        }
        //    } catch (Exception ex) {
        //        logController.handleException( core,ex, "exception in serverConfigModel.getObject");
        //    }
        //    return returnModel;
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Save the object
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public int saveObject(CoreController core) {
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

