
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
using static Contensive.Processor.Constants;
//
namespace Contensive.Processor.Models.Domain {
    //
    //====================================================================================================
    /// <summary>
    /// Configuration of an application
    /// - new() - to allow deserialization (so all methods must pass in cp)
    /// - shared getObject( cp, id ) - returns loaded model
    /// - saveObject( cp ) - saves instance properties, returns the record id
    /// </summary>
    [Serializable]
    public class AppConfigModel {
        /// <summary>
        /// name for the app. Must be unique within the server group. Difficulate to change later.
        /// </summary>
        public string name = "";
        /// <summary>
        /// status used to signal that the app is ok
        /// </summary>
        public AppStatusEnum appStatus = AppStatusEnum.maintenance;
        /// <summary>
        /// when false, app throws exception
        /// </summary>
        public bool enabled = false;
        /// <summary>
        /// key used for all encoding, two=way and one-way encoding. 
        /// </summary>
        public string privateKey = "";
        /// <summary>
        /// if the privateKey decoding fails and this key is not blank, an attempt is made with this key on reads.
        /// When changing keys, put the old key here. For two-way encoding, read will use fallback, and written back primary.
        /// For one-way, if primary fails, attempt secondary.
        /// </summary>
        public string privateKeyFallBack = "";
        /// <summary>
        /// local abs path to wwwroot. Paths end in slash. (i.e. d:\inetpub\myApp\www\)
        /// </summary>
        public string localWwwPath = "";
        /// <summary>
        /// local file path to the content files. Paths end in slash. (i.e. d:\inetpub\myApp\files\)
        /// </summary>
        public string localFilesPath = "";
        /// <summary>
        /// local file path to the content files. Paths end in slash. (i.e. d:\inetpub\myApp\private\)
        /// </summary>
        public string localPrivatePath = "";
        /// <summary>
        /// temp file storage, files used by just one process, scope just during rendering life. Paths end in slash. (i.e. d:\inetpub\myApp\temp\)
        /// </summary>
        public string localTempPath = "";
        /// <summary>
        /// path within AWS S3 bucket where www files are stored
        /// </summary>
        public string remoteWwwPath = "";
        /// <summary>
        /// path within AWS S3 bucket where cdn files are stored.
        /// in some cases (like legacy), cdnFiles are in an iis virtual folder mapped to appRoot (like /appName/files/). Some cases this is a URL (http:\\cdn.domain.com pointing to s3)
        /// </summary>
        public string remoteFilePath = "";
        /// <summary>
        /// path within AWS S3 bucket where private files are stored.
        /// </summary>
        public string remotePrivatePath = "";
        /// <summary>
        /// url for cdn files (for upload files, etc). For local files is can be /appname/files/) for remote cdn, it includes protocol-host
        /// </summary>
        public string cdnFileUrl = "";
        /// <summary>
        /// set true to be included in server monitor testing
        /// </summary>
        public bool allowSiteMonitor = false;
        /// <summary>
        /// domain(s) for the app. primary domain is the first item in the list
        /// </summary>
        public List<string> domainList = new List<string>();
        /// <summary>
        /// route to admin site. The url pathpath that executes the addon site
        /// </summary>
        public string adminRoute = "";
        /// <summary>
        /// when exeecuting iis, this is the default page.
        /// </summary>
        public string defaultPage = "default.aspx";
        //
        //====================================================================================================
        /// <summary>
        /// status of the app in the appConfigModel. Only applies to the app loaded in the serverstatus.appconfig
        /// </summary>
        public enum AppModeEnum {
            normal = 0,
            maintainence = 1
        }
        //
        //====================================================================================================
        /// <summary>
        /// status of the app in the appConfigModel. Only applies to the app loaded in the serverstatus.appconfig
        /// </summary>
        public enum AppStatusEnum {
            ok = 0,
            maintenance = 1
        }
        //
        //====================================================================================================
        /// <summary>
        /// Create an empty object. needed for deserialization. Use newModel() method as constructor, includes cache
        /// </summary>
        public AppConfigModel() { }
        //
        //====================================================================================================
        /// <summary>
        /// get configuration and assign an application 
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>
        public static AppConfigModel getObject(CoreController core, ServerConfigModel serverConfig, string appName) {
            AppConfigModel returnModel = null;
            try {
                if (!string.IsNullOrEmpty(appName)) {
                    if (!serverConfig.apps.ContainsKey(appName.ToLowerInvariant())) {
                        //
                        // -- application not configured
                        returnModel = null;
                        throw new Exception("application [" + appName + "] was not found in this server group.");
                    } else {
                        //
                        // -- return config object from serverConfig
                        returnModel = serverConfig.apps[appName.ToLowerInvariant()];
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex, "exception in serverConfigModel.getObject");
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
        public void saveObject(CoreController core) {
            try {
                if (!string.IsNullOrEmpty(name)) {
                    if (!core.serverConfig.apps.ContainsKey(name.ToLowerInvariant())) {
                        //
                        // -- application not configured yet
                        core.serverConfig.apps.Add(name.ToLowerInvariant(), this);
                    }
                    core.serverConfig.saveObject(core);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
        }
    }
}

