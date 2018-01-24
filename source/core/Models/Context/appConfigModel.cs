
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
    public class appConfigModel {
        //
        public string name = "";
        public appStatusEnum appStatus = appStatusEnum.building;
        public appModeEnum appMode = appModeEnum.maintainence; // must be set to normal after setup
        public bool enabled = false;
        public string privateKey = ""; // rename hashKey
        public string appRootFilesPath = ""; // local file path to the appRoot (i.e. d:\inetpub\myApp\wwwRoot\)
        public string cdnFilesPath = ""; // local file path to the content files (i.e. d:\inetpub\myApp\files\)
        public string privateFilesPath = ""; // local file path to the content files (i.e. d:\inetpub\myApp\private\)
        public string tempFilesPath = ""; // ephemeral storage, files live just during rendering life
        public string cdnFilesNetprefix = ""; // in some cases (like legacy), cdnFiles are iis virtual folder mapped to appRoot (/appName/files/). Some cases this is a URL (http:\\cdn.domain.com pointing to s3)
        public bool allowSiteMonitor = false;
        public List<string> domainList = new List<string>(); // primary domain is the first item in the list
        public string adminRoute = ""; // The url pathpath that executes the addon site
        public string defaultPage = "default.aspx"; // when exeecuting iis
        //
        //====================================================================================================
        /// <summary>
        /// status of the app in the appConfigModel. Only applies to the app loaded in the serverstatus.appconfig
        /// </summary>
        public enum appModeEnum {
            normal = 0,
            maintainence = 1
        }
        //
        //====================================================================================================
        /// <summary>
        /// status of the app in the appConfigModel. Only applies to the app loaded in the serverstatus.appconfig
        /// </summary>
        public enum appStatusEnum {
            OK = 2,
            building = 3
        }
        //
        //====================================================================================================
        /// <summary>
        /// Create an empty object. needed for deserialization. Use newModel() method as constructor, includes cache
        /// </summary>
        public appConfigModel() { }
        //
        //====================================================================================================
        /// <summary>
        /// get configuration and assign an application 
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>
        public static appConfigModel getObject(coreController core, serverConfigModel serverConfig, string appName) {
            appConfigModel returnModel = null;
            try {
                if (!string.IsNullOrEmpty(appName)) {
                    if (!serverConfig.apps.ContainsKey(appName.ToLower())) {
                        //
                        // -- application not configured
                        returnModel = null;
                        throw new Exception("application [" + appName + "] was not found in this server group.");
                    } else {
                        //
                        // -- return config object from serverConfig
                        returnModel = serverConfig.apps[appName.ToLower()];
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex, "exception in serverConfigModel.getObject");
            }
            return returnModel;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Save the current appConfig settings to the serverConfig.apps collection
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public void saveObject(coreController core) {
            try {
                if (!string.IsNullOrEmpty(name)) {
                    if (!core.serverConfig.apps.ContainsKey(name.ToLower())) {
                        //
                        // -- application not configured yet
                        core.serverConfig.apps.Add(name.ToLower(), this);
                    }
                    core.serverConfig.saveObject(core);
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
    }
}

