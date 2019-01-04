﻿
using System;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using Contensive.BaseClasses;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Exceptions;

namespace Contensive.Processor {
    public class CPClass : CPBaseClass, IDisposable {
        #region COM GUIDs
        public const string ClassId = "2EF01C6F-5288-411D-A5DE-76C8923CE1D3";
        public const string InterfaceId = "58E04B36-2C75-4D11-9A8D-22A52E8417EB";
        public const string EventsId = "4FADD1C2-6A89-4A8E-ADD0-9850D3EB6DBC";
        #endregion
        //
        public CoreController core { get; set; }
        //
        //=========================================================================================================
        /// <summary>
        /// constructor for server use. No application context will be available. Use to create new apps or iterate through apps.
        /// </summary>
        /// <remarks></remarks>
        public CPClass() : base() {
            core = new CoreController(this);
        }
        //
        //=========================================================================================================
        /// <summary>
        /// constructor for non-Internet app use. Configuration read from programdata json
        /// </summary>
        /// <remarks></remarks>
        public CPClass(string appName) : base() {
            core = new CoreController(this, appName);
        }
        //
        //=========================================================================================================
        /// <summary>
        /// constructor for non-Internet app use. Configuration provided manually
        /// </summary>
        /// <remarks></remarks>
        public CPClass(string appName, ServerConfigModel serverConfig ) : base() {
            core = new CoreController(this, appName, serverConfig);
        }
        //
        //=========================================================================================================
        /// <summary>
        /// constructor for iis site use. Configuration provided manually (maybe from webconfig)
        /// </summary>
        /// <param name="httpContext"></param>
        /// <remarks></remarks>
        public CPClass(string appName, ServerConfigModel serverConfig, System.Web.HttpContext httpContext) : base() {
            core = new CoreController(this, appName, serverConfig, httpContext);
        }
        //
        //=========================================================================================================
        /// <summary>
        /// constructor for iis site use. Configuration read from programdata json
        /// </summary>
        /// <param name="httpContext"></param>
        /// <remarks></remarks>
        public CPClass(string appName, System.Web.HttpContext httpContext) : base() {
            core = new CoreController(this, appName, httpContext);
        }
        //
        //=========================================================================================================
        /// <summary>
        /// return ok if the application is running correctly. Use statusMessage to display the status
        /// </summary>
        public AppConfigModel.AppStatusEnum status {
            get {
                return core.appConfig.appStatus;
            }
        }
        //
        //=========================================================================================================
        /// <summary>
        /// return a message that can be used to display status
        /// </summary>
        public string statusMessage {
                get {
                    return GenericController.GetApplicationStatusMessage(core.appConfig.appStatus);
                }
            }
        //
        //====================================================================================================
        /// <summary>
        /// returns true if the server config file is valid (currently only requires a valid db)
        /// </summary>
        /// <returns></returns>
        public bool serverOk {
            get {
                bool result = false;
                if (core == null) {
                    //
                } else if (core.serverConfig == null) {
                    //
                } else {
                    result = !string.IsNullOrEmpty(core.serverConfig.defaultDataSourceAddress);
                }
                return result;
            }
        }
        //
        //=========================================================================================================
        /// <summary>
        /// returns true if the current application has status set OK (not disabled)
        /// </summary>
        public bool appOk {
            get {
                if (core != null) {
                    if (core.serverConfig != null) {
                        if (core.appConfig != null) {
                            return (core.appConfig.appStatus == AppConfigModel.AppStatusEnum.ok);
                        }
                    }
                }
                return false;
            }
        }
        //
        //==========================================================================================
        /// <summary>
        /// Executes a specific route. The route can be a remote method, link alias, admin route, etc.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public string executeRoute(string route) {
            try {
                return core.executeRoute(route);
            } catch (Exception ex) {
                Site.ErrorReport(ex);
            }
            return String.Empty;
        }
        //
        //==========================================================================================
        /// <summary>
        /// Executes the default route set in the admin settings is used.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public string executeRoute() {
            try {
                return core.executeRoute();
            } catch (Exception ex) {
                Site.ErrorReport(ex);
            }
            return String.Empty;
        }
        //
        //====================================================================================================
        /// <summary>
        /// executes an addon with the name or guid provided, in the context specified.
        /// </summary>
        /// <param name="addonNameOrGuid"></param>
        /// <param name="addonContext"></param>
        /// <returns></returns>
        public string executeAddon(string addonNameOrGuid, CPUtilsBaseClass.addonContext addonContext = CPUtilsBaseClass.addonContext.ContextSimple) {
            try {
                if (GenericController.isGuid(addonNameOrGuid)) {
                    //
                    // -- call by guid
                    AddonModel addon = Models.Db.AddonModel.create(core, addonNameOrGuid);
                    if ( addon == null ) {
                        throw new GenericException("Addon [" + addonNameOrGuid + "] could not be found.");
                    } else {
                        return core.addon.execute(addon, new CPUtilsBaseClass.addonExecuteContext {
                            addonType = addonContext,
                            errorContextMessage = "external call to execute addon [" + addonNameOrGuid + "]"
                        });
                    }
                } else {
                    AddonModel addon = Models.Db.AddonModel.createByUniqueName(core, addonNameOrGuid);
                    if ( addon != null ) {
                        //
                        // -- call by name
                        return core.addon.execute(addon, new CPUtilsBaseClass.addonExecuteContext {
                            addonType = addonContext,
                            errorContextMessage = "external call to execute addon [" + addonNameOrGuid + "]"
                        });
                    } else if (addonNameOrGuid.IsNumeric() ) {
                        //
                        // -- compatibility - call by id
                        return executeAddon(GenericController.encodeInteger(addonNameOrGuid), addonContext);
                    } else {
                        throw new GenericException("Addon [" + addonNameOrGuid + "] could not be found.");
                    }
                }
            } catch (Exception ex) {
                Site.ErrorReport(ex);
            }
            return string.Empty;
        }
        //
        //====================================================================================================
        /// <summary>
        /// executes an addon with the id provided, in the context specified.
        /// </summary>
        /// <param name="addonId"></param>
        /// <param name="addonContext"></param>
        /// <returns></returns>
        public string executeAddon(int addonId, Contensive.BaseClasses.CPUtilsBaseClass.addonContext addonContext = Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple) {
            try {
                AddonModel addon = AddonModel.create(core, addonId);
                if ( addon == null) {
                    throw new GenericException("Addon [#" + addonId.ToString() + "] could not be found.");
                } else {
                    return core.addon.execute(addon, new CPUtilsBaseClass.addonExecuteContext {
                        addonType = addonContext,
                        errorContextMessage = "external call to execute addon [" + addonId + "]"
                    });
                }
            } catch (Exception ex) {
                Site.ErrorReport(ex);
            }
            return string.Empty;
        }
        //
        //=========================================================================================================
        /// <summary>
        /// Create a new block object, used to manipulate html elements using htmlClass and htmlId. Alternatively create a block object with its constructor.
        /// </summary>
        /// <returns></returns>
        public override CPBlockBaseClass BlockNew() {
            return new CPBlockClass(this);
        }
        //
        //=========================================================================================================
        /// <summary>
        /// Create a new data set object used to run queries and open tables with soft table names (determined run-time). Alternatively create a data set object with its constructor
        /// </summary>
        /// <returns></returns>
        public override CPCSBaseClass CSNew() {
            return new CPCSClass(this);
        }
        //
        //=========================================================================================================
        /// <summary>
        /// Create a datasource. The default datasource is CP.Db
        /// </summary>
        /// <param name="DataSourceName"></param>
        /// <returns></returns>
        public override CPDbBaseClass DbNew(string DataSourceName) {
            return new CPDbClass(this, DataSourceName);
        }
        //
        //=========================================================================================================
        /// <summary>
        /// system version
        /// </summary>
        public override string Version {
            get {
                return core.codeVersion();
            }
        }
        //
        //=========================================================================================================
        /// <summary>
        /// expose the user error object
        /// </summary>
        public override CPUserErrorBaseClass UserError {
            get {
                if (_userErrorObj == null) {
                    _userErrorObj = new CPUserErrorClass(this);
                }
                return _userErrorObj;
            }
        }
        private CPUserErrorClass _userErrorObj;
        //
        //=========================================================================================================
        /// <summary>
        /// expose the user object
        /// </summary>
        public override CPUserBaseClass User {
            get {
                if (_userObj == null) {
                    _userObj = new CPUserClass(this);
                }
                return _userObj;
            }
        }
        private CPUserClass _userObj;
        //
        //=========================================================================================================
        /// <summary>
        /// expose object for managing addons
        /// </summary>
        public override CPAddonBaseClass Addon {
            get {
                if (_addonObj == null) {
                    _addonObj = new CPAddonClass(this);
                }
                return _addonObj;
            }
        }
        private CPAddonClass _addonObj;
        //
        //=========================================================================================================
        /// <summary>
        /// expose object for managing content delivery files. This is a publically accessable location that holds content contributed. If remote file mode, this is an AWS S3 bucket
        /// </summary>
        public override CPFileSystemBaseClass FileCdn {
            get {
                if (_FileCdn == null) {
                    _FileCdn = new CPFileSystemClass(this, core.fileCdn);
                }
                return _FileCdn;
            }
        }
        private CPFileSystemClass _FileCdn;
        //
        //=========================================================================================================
        /// <summary>
        /// expose object for managing cache. This cache designed to cache Db record objects and Domain Objects
        /// </summary>
        public override CPCacheBaseClass Cache {
            get {
                if (_cacheObj == null) {
                    _cacheObj = new CPCacheClass(this);
                }
                return _cacheObj;
            }
        }
        private CPCacheClass _cacheObj;
        //
        //=========================================================================================================
        /// <summary>
        /// expose an object for managing content
        /// </summary>
        public override CPContentBaseClass Content {
            get {
                if (_contentObj == null) {
                    _contentObj = new CPContentClass(this);
                }
                return _contentObj;
            }
        }
        private CPContentClass _contentObj;
        //
        //=========================================================================================================
        /// <summary>
        /// expose object that can be used to configure the web environment if IIS is not used.
        /// </summary>
        public CPContextClass context {
            get {
                if (_contextObj == null) {
                    _contextObj = new CPContextClass(this);
                }
                return _contextObj;
            }
        }
        private CPContextClass _contextObj;
        //
        //=========================================================================================================
        /// <summary>
        /// Properties and methods helpful in access the database
        /// </summary>
        public override CPDbBaseClass Db {
            get {
                if (_dbObj == null) {
                    _dbObj = new CPDbClass(this,"");
                }
                return _dbObj;
            }
        }
        private CPDbClass _dbObj;
        //
        //=========================================================================================================
        /// <summary>
        /// Properties and methods helpful in creating a return document. 
        /// </summary>
        public override CPDocBaseClass Doc {
            get {
                if (_docObj == null) {
                    _docObj = new CPDocClass(this);
                }
                return _docObj;
            }
        }
        private CPDocClass _docObj;
        //
        //====================================================================================================
        /// <summary>
        /// Properties and methods helpful in managing email
        /// </summary>
        public override CPEmailBaseClass Email {
            get {
                if (_emailObj == null) {
                    _emailObj = new CPEmailClass(this);
                }
                return _emailObj;
            }
        }
        private CPEmailClass _emailObj;
        //
        //====================================================================================================
        /// <summary>
        /// Legacy method that provides access the current application server. AS of v5, access is limited to that provided by FilePrivate, wwwRoot, temp and cdnFiles
        /// </summary>
        [Obsolete("deprecated",true)] public override CPFileBaseClass File {
            get {
                if (_fileObj == null) {
                    _fileObj = new CPFileClass(this);
                }
                return _fileObj;
            }
        }
        private CPFileClass _fileObj;
        //
        //====================================================================================================
        /// <summary>
        /// Properties and methods helpful in managing groups
        /// </summary>
        public override CPGroupBaseClass Group {
            get {
                if (_groupObj == null) {
                    _groupObj = new CPGroupClass(this);
                }
                return _groupObj;
            }
        }
        private CPGroupClass _groupObj;
        //
        //====================================================================================================
        /// <summary>
        /// Properties and methods helpful in creating html documents
        /// </summary>
        public override CPHtml5BaseClass Html {
            get {
                if (_htmlObj == null) {
                    _htmlObj = new CPHtml5Class(this);
                }
                return _htmlObj;
            }
        }
        private CPHtml5Class _htmlObj;
        //
        [Obsolete("Deprecated. To access addon details of the addon running, create a model with the cp.addon.id",true)]
        public override CPAddonBaseClass MyAddon {
            get {
                return Addon;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Access to private files for the application. Private files are not available online.
        /// </summary>
        public override CPFileSystemBaseClass FilePrivate {
            get {
                if (_FilePrivate == null) {
                    _FilePrivate = new CPFileSystemClass(this, core.filePrivate);
                }
                return _FilePrivate;
            }
        }
        private CPFileSystemClass _FilePrivate;
        //
        //====================================================================================================
        /// <summary>
        /// Object that provides access to the application request, typically a webserver request.
        /// </summary>
        public override CPRequestBaseClass Request {
            get {
                if (_requestObj == null) {
                    _requestObj = new CPRequestClass(this);
                }
                return _requestObj;
            }
        }
        private CPRequestClass _requestObj;
        //
        //====================================================================================================
        /// <summary>
        /// Object that provides access to the application response, typically a webserver response.
        /// </summary>
        public override CPResponseBaseClass Response {
            get {
                if (_responseObj == null) {
                    _responseObj = new CPResponseClass(this);
                }
                return _responseObj;
            }
        }
        private CPResponseClass _responseObj;
        //
        //====================================================================================================
        /// <summary>
        /// An object that includes properties and methods that descript the application
        /// </summary>
        public override CPSiteBaseClass Site {
            get {
                if (_siteObj == null) {
                    _siteObj = new CPSiteClass(this);
                }
                return _siteObj;
            }
        }
        private CPSiteClass _siteObj;
        //
        //====================================================================================================
        /// <summary>
        /// Temporary file storarge
        /// </summary>
        public override CPFileSystemBaseClass FileTemp {
            get {
                if (_FileTemp == null) {
                    _FileTemp = new CPFileSystemClass(this, core.tempFiles);
                }
                return _FileTemp;
            }
        }
        private CPFileSystemClass _FileTemp;
 
        //
        //====================================================================================================
        /// <summary>
        /// An object that provides basic methods helpful is application execute.
        /// </summary>
        public override CPUtilsBaseClass Utils {
            get {
                if (_utilsObj == null) {
                    _utilsObj = new CPUtilsClass(this);
                }
                return _utilsObj;
            }
        }
        private CPUtilsClass _utilsObj;
        //
        //====================================================================================================
        /// <summary>
        /// An object that represents the visit. A visit is typically used for Internet applications and represents a sequence of route hits
        /// </summary>
        public override CPVisitBaseClass Visit {
            get {
                if (_visitObj == null) {
                    _visitObj = new CPVisitClass(this);
                }
                return _visitObj;
            }
        }
        private CPVisitClass _visitObj;
        //
        //====================================================================================================
        /// <summary>
        /// An object that represents the visitor. The visitor is typically used for Internet applications and represents a sequence of visits
        /// </summary>
        public override CPVisitorBaseClass Visitor {
            get {
                if (_visitorObj == null) {
                    _visitorObj = new CPVisitorClass(this);
                }
                return _visitorObj;
            }
        }
        private CPVisitorClass _visitorObj;
        //
        //====================================================================================================
        /// <summary>
        /// A file object with access to the domain's primary web root files. This is typically where design files are stored, like styles sheets, js, etc.
        /// </summary>
        public override CPFileSystemBaseClass FileAppRoot {
            get {
                if (_FileAppRoot == null) {
                    _FileAppRoot = new CPFileSystemClass(this, core.fileAppRoot);
                }
                return _FileAppRoot;
            }
        } private CPFileSystemClass _FileAppRoot;
        //
        //====================================================================================================
        /// <summary>
        /// The route map is a dictionary of route names and route details that tell how to execute the route
        /// </summary>
        public RouteMapModel routeMap {
            get {
                return core.routeMap;
            }
        }
        //
        //=========================================================================================================
        // deprecated
        //
        [Obsolete("Use cp.Doc.SetProperty.", true)]
        public void addVar(string key, string value) {
            try {
                if (!string.IsNullOrEmpty(key)) {
                    this.Doc.SetProperty(key, value);
                }
            } catch (Exception ex) {
                Site.ErrorReport(ex);
            }
        }
        //
        //====================================================================================================
        #region  IDisposable Support 
        //
        // this class must implement System.IDisposable
        // never throw an exception in dispose
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        //====================================================================================================
        //
        protected bool disposed = false;
        //
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        ~CPClass() {
            Dispose(false);
        }
        //
        //====================================================================================================
        /// <summary>
        /// dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    if (_addonObj != null) {
                        _addonObj.Dispose();
                    }
                    if (_cacheObj != null) {
                        _cacheObj.Dispose();
                    }
                    if (_contentObj != null) {
                        _contentObj.Dispose();
                    }
                    if (_contextObj != null) {
                        _contextObj.Dispose();
                    }
                    if (_dbObj != null) {
                        _dbObj.Dispose();
                    }
                    if (_docObj != null) {
                        _docObj.Dispose();
                    }
                    if (_emailObj != null) {
                        _emailObj.Dispose();
                    }
                    if (_fileObj != null) {
                        _fileObj.Dispose();
                    }
                    if (_groupObj != null) {
                        _groupObj.Dispose();
                    }
                    if (_htmlObj != null) {
                        _htmlObj.Dispose();
                    }
                    //if (_myAddonObj != null) {
                    //    _myAddonObj.Dispose();
                    //}
                    if (_requestObj != null) {
                        _requestObj.Dispose();
                    }
                    if (_responseObj != null) {
                        _responseObj.Dispose();
                    }
                    if (_siteObj != null) {
                        _siteObj.Dispose();
                    }
                    if (_userErrorObj != null) {
                        _userErrorObj.Dispose();
                    }
                    if (_userObj != null) {
                        _userObj.Dispose();
                    }
                    if (_utilsObj != null) {
                        _utilsObj.Dispose();
                    }
                    if (_visitObj != null) {
                        _visitObj.Dispose();
                    }
                    if (_visitorObj != null) {
                        _visitorObj.Dispose();
                    }
                    if (_FileCdn != null) {
                        _FileCdn.Dispose();
                    }
                    if (_FileAppRoot != null) {
                        _FileAppRoot.Dispose();
                    }
                    if (_FilePrivate != null) {
                        _FilePrivate.Dispose();
                    }
                    if (core != null) {
                        core.Dispose();
                    }
                }
                //
                // cleanup non-managed objects
                //
            }
        }
        #endregion
    }

}