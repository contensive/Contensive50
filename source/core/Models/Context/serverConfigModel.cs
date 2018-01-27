﻿
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Models.Context {
    //
    //====================================================================================================
    // cached entity model pattern
    //   factory pattern creator, constructor is a shared method that returns a loaded object
    //   new() - to allow deserialization (so all methods must pass in cp)
    //   shared getObject( cp, id ) - returns loaded model
    //   saveObject( cp ) - saves instance properties, returns the record id
    //
    [Serializable()]
    public class serverConfigModel {
        //
        // -- public properties
        //
        // -- set during installation
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
        public dataSourceModel.dataSourceTypeEnum defaultDataSourceType;
        public string defaultDataSourceAddress = "";
        public string defaultDataSourceUsername = "";
        public string defaultDataSourcePassword = "";
        //
        // -- endpoint for cluster files (not sure how it works, maybe this will be an object taht includes permissions, for now an fpo)
        public bool isLocalFileSystem = true;
        public string localDataDriveLetter = "D";
        public string cdnFilesRemoteEndpoint;
        //
        // -- configuration of async command listener on render machines (not sure if used still)
        public int serverListenerPort = Port_ContentServerControlDefault;
        public int maxConcurrentTasksPerServer = 5;
        public string username = "";
        public string password = "";
        //
        public bool enableLogging = false;
        //
        // -- deprecated
        //Public appPattern As String
        //
        // -- List of all apps on this server
        public Dictionary<string, appConfigModel> apps = new Dictionary<string, appConfigModel>();
        //
        // -- the specific application in use for this instance (may be empty if this instance is not initialized
//        [NonSerialized()]
//        public appConfigModel appConfig;
        ////
        ////====================================================================================================
        ///// <summary>
        ///// application configuration class
        ///// </summary>
        //public class appConfigModel {
        //    public string name = "";
        //    public appStatusEnum appStatus = appStatusEnum.building;
        //    public appModeEnum appMode = appModeEnum.maintainence; // must be set to normal after setup
        //    public bool enabled = false;
        //    public string privateKey = ""; // rename hashKey
        //    public string appRootFilesPath = ""; // local file path to the appRoot (i.e. d:\inetpub\myApp\wwwRoot\)
        //    public string cdnFilesPath = ""; // local file path to the content files (i.e. d:\inetpub\myApp\files\)
        //    public string privateFilesPath = ""; // local file path to the content files (i.e. d:\inetpub\myApp\private\)
        //    public string tempFilesPath = ""; // ephemeral storage, files live just during rendering life
        //    public string cdnFilesNetprefix = ""; // in some cases (like legacy), cdnFiles are iis virtual folder mapped to appRoot (/appName/files/). Some cases this is a URL (http:\\cdn.domain.com pointing to s3)
        //    public bool allowSiteMonitor = false;
        //    public List<string> domainList = new List<string>(); // primary domain is the first item in the list
        //    public string adminRoute = ""; // The url pathpath that executes the addon site
        //    public string defaultPage = "default.aspx"; // when exeecuting iis
        //}
        ////
        ////====================================================================================================
        ///// <summary>
        ///// status of the app in the appConfigModel. Only applies to the app loaded in the serverstatus.appconfig
        ///// </summary>
        //public enum appModeEnum {
        //    normal = 0,
        //    maintainence = 1
        //}
        ////
        ////====================================================================================================
        ///// <summary>
        ///// status of the app in the appConfigModel. Only applies to the app loaded in the serverstatus.appconfig
        ///// </summary>
        //public enum appStatusEnum {
        //    OK = 2,
        //    building = 3
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Create an empty object. needed for deserialization. Use newModel() method as constructor, includes cache
        /// </summary>
        public serverConfigModel() {
            //
        }
        //
        //====================================================================================================
        /// <summary>
        /// get ServerConfig, returning only the server data section without specific serverConfig.app
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>
        public static serverConfigModel getObject(coreController core) {
            serverConfigModel returnModel = null;
            try {
                System.Web.Script.Serialization.JavaScriptSerializer json_serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string JSONTemp;
                //
                // ----- read/create serverConfig
                //
                JSONTemp = core.programDataFiles.readFile("config.json");
                if (string.IsNullOrEmpty(JSONTemp)) {
                    //
                    // for now it fails, maybe later let it autobuild a local cluster
                    //
                    returnModel = new Models.Context.serverConfigModel();
                    returnModel.allowTaskRunnerService = false;
                    returnModel.allowTaskSchedulerService = false;
                    core.programDataFiles.saveFile("config.json", json_serializer.Serialize(returnModel));
                } else {
                    returnModel = json_serializer.Deserialize<serverConfigModel>(JSONTemp);
                }
            } catch (Exception ex) {
                core.handleException(ex, "exception in serverConfigModel.getObject");
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
        //public static Models.Context.serverConfigModel getObject(coreController core, string appName) {
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
        //        core.handleException(ex, "exception in serverConfigModel.getObject");
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
        public int saveObject(coreController core) {
            try {
                string jsonTemp = core.json.Serialize(this);
                core.programDataFiles.saveFile("config.json", jsonTemp);
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return 0;
        }
    }
}
