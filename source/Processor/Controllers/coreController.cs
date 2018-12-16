
using System;
using System.Reflection;
using Contensive.BaseClasses;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Models.Db;
using System.Collections.Generic;
using static Contensive.Processor.Constants;
using static Contensive.Processor.Controllers.GenericController;
using System.Diagnostics;
using Contensive.Processor.Exceptions;
//
namespace Contensive.Processor.Controllers {
    //
    //===================================================================================================
    /// <summary>
    /// central object, passed for dependancy injection, provides access to persistent objects (document persistence/scope)
    /// </summary>
    public class CoreController : IDisposable {
        //
        //===================================================================================================
        /// <summary>
        /// a reference to the cp api interface that parents this object. CP is the api to addons, based on the abstract classes exposed to developers.
        /// </summary>
        internal CPClass cp_forAddonExecutionOnly { get; set; }
        //
        //===================================================================================================
        /// <summary>
        /// server configuration - this is the node's configuration, including everything needed to attach to resources required (db,cache,filesystem,etc)
        /// and the configuration of all applications within this group of servers. This file is shared between all servers in the group.
        /// </summary>
        public ServerConfigModel serverConfig { get; set; }
        //
        //===================================================================================================
        /// <summary>
        /// An instance of the sessionController, populated for the current session (user state, visit state, etc)
        /// </summary>
        public SessionController session;
        //
        //===================================================================================================
        // todo - this should be a pointer into the serverConfig
        /// <summary>
        /// The configuration for this app, a copy of the data in the serverconfig file
        /// </summary>
        public AppConfigModel appConfig { get; set; }
        //
        //===================================================================================================
        // todo move persistent objects to .doc (keeping of document scope persistence)
        /// <summary>
        /// rnd resource used during this scope
        /// </summary>
        public Random random = new Random();
        //
        //===================================================================================================
        /// <summary>
        /// Set true and sendSmtp adds all email to mockSmtpList of smtpEmailClass
        /// </summary>
        public bool mockSmtp = false;
        public List<SmtpEmailClass> mockSmtpList = new List<SmtpEmailClass>();
        public class SmtpEmailClass {
            public EmailController.EmailClass email;
            public string smtpServer;
            public string AttachmentFilename;
        }
        //
        /// <summary>
        /// when enable, use MS trace logging. An attempt to stop file append permission issues
        /// </summary>
        public bool useNlog = true;
        //
        /// <summary>
        /// tmp, block to prevent core.handleException recursion. Will refactor out
        /// </summary>
        public bool _handlingExceptionRecursionBlock = false;
        //
        /// <summary>
        /// Dictionary of cdef, index by name
        /// </summary>
        internal Dictionary<string, Models.Domain.CDefDomainModel> cdefDictionary { get; set; }
        //
        /// <summary>
        /// Dictionary of tableschema, index by name
        /// </summary>
        internal Dictionary<string, Models.Domain.TableSchemaModel> tableSchemaDictionary { get; set; }
        //
        /// <summary>
        /// lookup contentId by contentName
        /// </summary>
        internal Dictionary<string, int> contentNameIdDictionary {
            get {
                if (_contentNameIdDictionary == null) {
                    _contentNameIdDictionary = new Dictionary<string, int>();
                }
                return _contentNameIdDictionary;
            }
        } internal Dictionary<string, int> _contentNameIdDictionary = null;
        //
        //===================================================================================================
        /// <summary>
        /// list of DLLs in the addon assembly path that are not adds. As they are discovered, they are added to this list
        /// and not loaded in the future. The me.dispose compares the list count to the loaded count and caches if different.
        /// </summary>
        /// <returns></returns>
        public List<string> assemblySkipList {
            get {
                if (_assemblySkipList == null) {
                    _assemblySkipList = cache.getObject<List<string>>(cacheNameAssemblySkipList);
                    if (_assemblySkipList == null) {
                        _assemblySkipList = new List<string> {
                            programFiles.localAbsRootPath + "v8-base-ia32.dll",
                            programFiles.localAbsRootPath + "v8-ia32.dll",
                            programFiles.localAbsRootPath + "ClearScriptV8-32.dll"
                        };
                    }
                    _assemblySkipList_CountWhenLoaded = _assemblySkipList.Count;
                }
                return _assemblySkipList;
            }
        }
        private List<string> _assemblySkipList;
        private int _assemblySkipList_CountWhenLoaded;
        //
        //===================================================================================================
        public Dictionary<string, DataSourceModel> dataSourceDictionary {
            get {
                if (_dataSources == null) {
                    _dataSources = DataSourceModel.getNameDict(this);
                }
                return _dataSources;
            }
        }
        private Dictionary<string, DataSourceModel> _dataSources = null;
        //
        //===================================================================================================
        public DocController doc {
            get {
                if (_doc == null) {
                    _doc = new DocController(this);
                }
                return _doc;
            }
        }
        private DocController _doc;
        //
        //===================================================================================================
        public Controllers.HtmlController html {
            get {
                if (_html == null) {
                    _html = new Controllers.HtmlController(this);
                }
                return _html;
            }
        }
        private Controllers.HtmlController _html;
        //
        //===================================================================================================
        public Controllers.AddonController addon {
            get {
                if (_addon == null) {
                    _addon = new Controllers.AddonController(this);
                }
                return _addon;
            }
        }
        private Controllers.AddonController _addon;
        //
        //===================================================================================================
        //public MenuFlyoutController menuFlyout {
        //    get {
        //        if (_menuFlyout == null) {
        //            _menuFlyout = new MenuFlyoutController(this);
        //        }
        //        return _menuFlyout;
        //    }
        //}
        //private MenuFlyoutController _menuFlyout;
        //
        //===================================================================================================
        public PropertyModelClass userProperty {
            get {
                if (_userProperty == null) {
                    _userProperty = new PropertyModelClass(this, PropertyModelClass.PropertyTypeEnum.user);
                }
                return _userProperty;
            }
        }
        private PropertyModelClass _userProperty;
        //
        //===================================================================================================
        public PropertyModelClass visitorProperty {
            get {
                if (_visitorProperty == null) {
                    _visitorProperty = new PropertyModelClass(this, PropertyModelClass.PropertyTypeEnum.visitor);
                }
                return _visitorProperty;
            }
        }
        private PropertyModelClass _visitorProperty;
        //
        //===================================================================================================
        public PropertyModelClass visitProperty {
            get {
                if (_visitProperty == null) {
                    _visitProperty = new PropertyModelClass(this, PropertyModelClass.PropertyTypeEnum.visit);
                }
                return _visitProperty;
            }
        }
        private PropertyModelClass _visitProperty;
        //
        //===================================================================================================
        public DocPropertyController docProperties {
            get {
                if (_docProperties == null) {
                    _docProperties = new DocPropertyController(this);
                }
                return _docProperties;
            }
        }
        private DocPropertyController _docProperties = null;
        //
        //===================================================================================================
        /// <summary>
        /// siteProperties object
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public SitePropertiesController siteProperties {
            get {
                if (_siteProperties == null) {
                    _siteProperties = new SitePropertiesController(this);
                }
                return _siteProperties;
            }
        }
        private SitePropertiesController _siteProperties = null;
        //
        //===================================================================================================
        public WebServerController webServer {
            get {
                if (_webServer == null) {
                    _webServer = new WebServerController(this);
                }
                return _webServer;
            }
        }
        private WebServerController _webServer;
        //
        //===================================================================================================
        public FileController appRootFiles {
            get {
                if (_appRootFiles == null) {
                    if (appConfig != null) {
                        if (appConfig.enabled) {
                            _appRootFiles = new FileController(this, serverConfig.isLocalFileSystem, appConfig.localWwwPath, appConfig.remoteWwwPath);
                        }
                    }
                }
                return _appRootFiles;
            }
        }
        private FileController _appRootFiles = null;
        //
        //===================================================================================================
        public FileController tempFiles {
            get {
                if (_tmpFiles == null) {
                    //
                    // local server -- everything is ephemeral
                    _tmpFiles = new FileController(this, true, appConfig.localTempPath,"");
                }
                return _tmpFiles;
            }
        }
        private FileController _tmpFiles = null;
        //
        //===================================================================================================
        public FileController privateFiles {
            get {
                if (_privateFiles == null) {
                    if (appConfig != null) {
                        if (appConfig.enabled) {
                            _privateFiles = new FileController(this, serverConfig.isLocalFileSystem, appConfig.localPrivatePath, appConfig.remotePrivatePath);
                        }
                    }
                }
                return _privateFiles;
            }
        }
        private FileController _privateFiles = null;
        //
        //===================================================================================================
        public FileController programDataFiles {
            get {
                if (_programDataFiles == null) {
                    //
                    // -- always local -- must be because this object is used to read serverConfig, before the object is valid
                    string programDataPath = FileController.normalizeDosPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)) + "Contensive\\";
                    _programDataFiles = new FileController(this, true, programDataPath, "");
                }
                return _programDataFiles;
            }
        }
        private FileController _programDataFiles = null;
        //
        //===================================================================================================
        public FileController programFiles {
            get {
                if (_programFiles == null) {
                    //
                    // -- always local
                    _programFiles = new FileController(this, true, serverConfig.programFilesPath,"");
                }
                return _programFiles;
            }
        }
        private FileController _programFiles = null;
        //
        //===================================================================================================
        public FileController cdnFiles {
            get {
                if (_cdnFiles == null) {
                    if (appConfig != null) {
                        if (appConfig.enabled) {
                            _cdnFiles = new FileController(this, serverConfig.isLocalFileSystem, appConfig.localFilesPath,appConfig.remoteFilePath);
                        }
                    }
                }
                return _cdnFiles;
            }
        }
        private FileController _cdnFiles = null;
        //
        //===================================================================================================
        /// <summary>
        /// provide an addon cache object lazy populated from the Domain.addonCacheModel. This object provides an
        /// interface to lookup read addon data and common lists
        /// </summary>
        public AddonCacheModel addonCache {
            get {
                if (_addonCache == null) {
                    _addonCache = cache.getObject<AddonCacheModel>(cacheObject_addonCache);
                    if (_addonCache == null) {
                        _addonCache = new AddonCacheModel(this);
                        cache.storeObject(cacheObject_addonCache, _addonCache);
                    }
                }
                return _addonCache;
            }
        }
        private AddonCacheModel _addonCache = null;
        //
        //===================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DomainModel domain {
            get {
                if (_domains == null) {
                    _domains = new DomainModel();
                }
                return _domains;
            }
            set {
                _domains = value;
            }
        }
        private DomainModel _domains = null;
        /// <summary>
        /// domains configured for this app. keys are lowercase
        /// </summary>
        public Dictionary<string, DomainModel> domainDictionary;
        //
        //===================================================================================================
        public System.Web.Script.Serialization.JavaScriptSerializer json {
            get {
                if (_json == null) {
                    _json = new System.Web.Script.Serialization.JavaScriptSerializer();
                }
                return _json;
            }
        }
        private System.Web.Script.Serialization.JavaScriptSerializer _json;
        //
        //===================================================================================================
        public Controllers.CacheController cache {
            get {
                if (_cache == null) {
                    _cache = new Controllers.CacheController(this);
                }
                return _cache;
            }
        }
        private Controllers.CacheController _cache = null;
        //
        //===================================================================================================
        // todo - convert to dictionary, one entry per datasource, remove datasource selection from all methods
        /// <summary>
        /// controller for the application's database
        /// </summary>
        public DbController db {
            get {
                if (_db == null) {
                    _db = new DbController(this);
                }
                return _db;
            }
        }
        private DbController _db;
        //
        //===================================================================================================
        /// <summary>
        /// db access to the server to add and query catalogs
        /// </summary>
        public DbServerController dbServer {
            get {
                if (_dbEngine == null) {
                    _dbEngine = new DbServerController(this);
                }
                return _dbEngine;
            }
        }
        private DbServerController _dbEngine;
        //
        //====================================================================================================
        /// <summary>
        /// coreClass constructor for cluster use.
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        public CoreController(CPClass cp) : base() {
            cp_forAddonExecutionOnly = cp;
            LogController.forceNLog( "CoreController constructor-0, enter", LogController.logLevel.Trace);
            //
            cdefDictionary = new Dictionary<string, Models.Domain.CDefDomainModel>();
            tableSchemaDictionary = null;
            //
            // -- create default auth objects for non-user methods, or until auth is available
            session = new SessionController(this);
            //
            serverConfig = ServerConfigModel.getObject(this);
            this.serverConfig.defaultDataSourceType = DataSourceModel.DataSourceTypeEnum.sqlServerNative;
            webServer.iisContext = null;
            constructorInitialize(false);
            LogController.forceNLog( "CoreController constructor-0, exit", LogController.logLevel.Trace);
        }
        //
        //====================================================================================================
        /// <summary>
        /// coreClass constructor for app, non-Internet use. coreClass is the primary object internally, created by cp.
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        public CoreController(CPClass cp, string applicationName) : base() {
            this.cp_forAddonExecutionOnly = cp;
            LogController.forceNLog( "CoreController constructor-1, enter", LogController.logLevel.Trace);
            //
            cdefDictionary = new Dictionary<string, Models.Domain.CDefDomainModel>();
            tableSchemaDictionary = null;
            //
            // -- create default auth objects for non-user methods, or until auth is available
            session = new SessionController(this);
            //
            serverConfig = ServerConfigModel.getObject(this);
            serverConfig.defaultDataSourceType = DataSourceModel.DataSourceTypeEnum.sqlServerNative;
            appConfig = AppConfigModel.getObject(this, serverConfig, applicationName);
            if (appConfig != null) {
                webServer.iisContext = null;
                constructorInitialize(false);
            }
            LogController.forceNLog( "CoreController constructor-1, exit", LogController.logLevel.Trace);
        }
        //
        //====================================================================================================
        /// <summary>
        /// The route map is a dictionary of route names plus route details that tell how to execute the route.
        /// local instance is used to speed up multiple local requests.
        /// If during a page hit the route table entries are updated, cache is cleared and the instance object is cleared. When the next reference is 
        /// made the data is refreshed. At the end of every pageload, if the routemap updates it reloads the iis route table so if a new route
        /// is added and the next hit is to that method, it will be loaded.
        /// </summary>
        public RouteMapModel routeMap {
            get {
                if (_routeMap == null) {
                    _routeMap = RouteMapModel.create(this);
                }
                return _routeMap;
            }
        }
        private RouteMapModel _routeMap = null;
        /// <summary>
        /// method to clear the core instance of routeMap. Explained in routeMap.
        /// </summary>
        public void routeMapClearLocalCache() {
            _routeMap = null;
        }
        //
        //====================================================================================================
        /// <summary>
        /// coreClass constructor for app, non-Internet use. coreClass is the primary object internally, created by cp.
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        public CoreController(CPClass cp, string applicationName, ServerConfigModel serverConfig) : base() {
            cp_forAddonExecutionOnly = cp;
            LogController.forceNLog( "CoreController constructor-2, enter", LogController.logLevel.Trace);
            //
            cdefDictionary = new Dictionary<string, Models.Domain.CDefDomainModel>();
            tableSchemaDictionary = null;
            //
            // -- create default auth objects for non-user methods, or until auth is available
            session = new SessionController(this);
            //
            this.serverConfig = serverConfig;
            this.serverConfig.defaultDataSourceType = DataSourceModel.DataSourceTypeEnum.sqlServerNative;
            appConfig = AppConfigModel.getObject(this, serverConfig, applicationName);
            appConfig.appStatus = AppConfigModel.AppStatusEnum.ok;
            webServer.iisContext = null;
            constructorInitialize(false);
            LogController.forceNLog( "CoreController constructor-2, exit", LogController.logLevel.Trace);
        }
        //
        //====================================================================================================
        /// <summary>
        /// coreClass constructor for app, non-Internet use. coreClass is the primary object internally, created by cp.
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        public CoreController(CPClass cp, string applicationName, ServerConfigModel serverConfig, System.Web.HttpContext httpContext) : base() {
            this.cp_forAddonExecutionOnly = cp;
            LogController.forceNLog( "CoreController constructor-3, enter", LogController.logLevel.Trace);
            //
            // -- create default auth objects for non-user methods, or until auth is available
            session = new SessionController(this);
            //
            this.serverConfig = serverConfig;
            this.serverConfig.defaultDataSourceType = DataSourceModel.DataSourceTypeEnum.sqlServerNative;
            appConfig = AppConfigModel.getObject(this, serverConfig, applicationName);
            this.appConfig.appStatus = AppConfigModel.AppStatusEnum.ok;
            webServer.initWebContext(httpContext);
            constructorInitialize(true);
            LogController.forceNLog( "CoreController constructor-3, exit", LogController.logLevel.Trace);
        }
        //====================================================================================================
        /// <summary>
        /// coreClass constructor for a web request/response environment. coreClass is the primary object internally, created by cp.
        /// </summary>
        public CoreController(CPClass cp, string applicationName, System.Web.HttpContext httpContext) : base() {
            this.cp_forAddonExecutionOnly = cp;
            LogController.forceNLog( "CoreController constructor-4, enter", LogController.logLevel.Trace);
            //
            cdefDictionary = new Dictionary<string, Models.Domain.CDefDomainModel>();
            tableSchemaDictionary = null;
            //
            // -- create default auth objects for non-user methods, or until auth is available
            session = new SessionController(this);
            //
            serverConfig = ServerConfigModel.getObject(this);
            serverConfig.defaultDataSourceType = DataSourceModel.DataSourceTypeEnum.sqlServerNative;
            appConfig = AppConfigModel.getObject(this, serverConfig, applicationName);
            if (appConfig != null) {
                webServer.initWebContext(httpContext);
                constructorInitialize(true);
            }
            LogController.forceNLog( "CoreController constructor-4, exit", LogController.logLevel.Trace);
        }
        //=============================================================================
        /// <summary>
        /// Executes the current route. To determine the route:
        /// route can be from URL, or from routeOverride
        /// how to process route
        /// -- urlParameters - /urlParameter(0)/urlParameter(1)/etc.
        /// -- first try full url, then remove them from the left and test until last, try just urlParameter(0)
        /// ---- so url /a/b/c, with addon /a and addon /a/b -> would run addon /a/b
        /// 
        /// </summary>
        /// <returns>The doc created by the default addon. (html, json, etc)</returns>
        public string executeRoute(string routeOverride = "") {
            string result = "";
            var sw = new Stopwatch();
            sw.Start();
            LogController.forceNLog("CoreController executeRoute, enter", LogController.logLevel.Trace);
            try {
                if (appConfig != null) {
                    //
                    // -- test fix for 404 response during routing - could it be a response left over from processing before we are called
                    webServer.setResponseStatus(WebServerController.httpResponseStatus200);
                    //
                    // -- execute intercept methods first, like login, that run before the route that returns the page
                    // -- intercept routes should be addons alos
                    //
                    // -- determine the route: try routeOverride
                    string normalizedRoute = GenericController.normalizeRoute(routeOverride);
                    if (string.IsNullOrEmpty(normalizedRoute)) {
                        //
                        // -- no override, try argument route (remoteMethodAddon=)
                        normalizedRoute = GenericController.normalizeRoute(docProperties.getText(RequestNameRemoteMethodAddon));
                        if (string.IsNullOrEmpty(normalizedRoute)) {
                            //
                            // -- no override or argument, use the url as the route
                            normalizedRoute = GenericController.normalizeRoute(webServer.requestPathPage.ToLowerInvariant());
                        }
                    }
                    //
                    // -- legacy ajaxfn methods
                    string AjaxFunction = docProperties.getText(RequestNameAjaxFunction);
                    if (!string.IsNullOrEmpty(AjaxFunction)) {
                        //
                        // -- Need to be converted to Url parameter addons
                        switch ((AjaxFunction)) {
                            case ajaxGetFieldEditorPreferenceForm:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.GetFieldEditorPreference()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxGetDefaultAddonOptionString:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.GetAjaxDefaultAddonOptionStringClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxSetVisitProperty:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.SetAjaxVisitPropertyClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxGetVisitProperty:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.GetAjaxVisitPropertyClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxData:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.ProcessAjaxDataClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxPing:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.GetOKClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxOpenIndexFilter:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.OpenAjaxIndexFilterClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxOpenIndexFilterGetContent:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.OpenAjaxIndexFilterGetContentClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxCloseIndexFilter:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.CloseAjaxIndexFilterClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxOpenAdminNav:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.OpenAjaxAdminNavClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            default:
                                //
                                // -- unknown method, log warning
                                doc.continueProcessing = false;
                                return string.Empty;
                        }
                    }
                    //
                    // -- legacy email intercept methods
                    if (docProperties.getInteger(rnEmailOpenFlag) > 0) {
                        //
                        // -- Process Email Open
                        doc.continueProcessing = false;
                        return (new Addons.Primitives.openEmailClass()).Execute(cp_forAddonExecutionOnly).ToString();
                    }
                    if (docProperties.getInteger(rnEmailClickFlag) > 0) {
                        //
                        // -- Process Email click
                        doc.continueProcessing = false;
                        return (new Addons.Primitives.clickEmailClass()).Execute(cp_forAddonExecutionOnly).ToString();
                    }
                    if (docProperties.getInteger(rnEmailBlockRecipientEmail) > 0) {
                        //
                        // -- Process Email block
                        doc.continueProcessing = false;
                        return (new Addons.Primitives.blockEmailClass()).Execute(cp_forAddonExecutionOnly).ToString();
                    }
                    //
                    // -- legacy form process methods 
                    string formType = docProperties.getText(docProperties.getText("ccformsn") + "type");
                    if (!string.IsNullOrEmpty(formType)) {
                        //
                        // set the meta content flag to show it is not needed for the head tag
                        switch (formType) {
                            case FormTypeAddonStyleEditor:
                                //
                                result = (new Addons.Primitives.processAddonStyleEditorClass()).Execute(cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypeAddonSettingsEditor:
                                //
                                result = (new Addons.Primitives.processAddonSettingsEditorClass()).Execute(cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypeSendPassword:
                                //
                                result = (new Addons.Primitives.processSendPasswordFormClass()).Execute(cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypeLogin:
                            case "l09H58a195":
                                //
                                result = (new Addons.Primitives.processFormLoginDefaultClass()).Execute(cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypeToolsPanel:
                                //
                                result = (new Addons.Primitives.processFormToolsPanelClass()).Execute(cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypePageAuthoring:
                                //
                                result = (new Addons.Primitives.processFormQuickEditingClass()).Execute(cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypeActiveEditor:
                                //
                                result = (new Addons.Primitives.processActiveEditorClass()).Execute(cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypeSiteStyleEditor:
                                //
                                result = (new Addons.Primitives.processSiteStyleEditorClass()).Execute(cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypeHelpBubbleEditor:
                                //
                                result = (new Addons.Primitives.processHelpBubbleEditorClass()).Execute(cp_forAddonExecutionOnly).ToString();
                                break;
                            case FormTypeJoin:
                                //
                                result = (new Addons.Primitives.processJoinFormClass()).Execute(cp_forAddonExecutionOnly).ToString();
                                break;
                        }
                    }
                    //
                    // -- legacy methods=
                    string HardCodedPage = docProperties.getText(RequestNameHardCodedPage);
                    if (!string.IsNullOrEmpty(HardCodedPage)) {
                        switch (GenericController.vbLCase(HardCodedPage)) {
                            case HardCodedPageLogout:
                                //
                                // -- logout intercept -- after logout continue
                                (new Addons.Primitives.processLogoutMethodClass()).Execute(cp_forAddonExecutionOnly);
                                break;
                            case HardCodedPageSendPassword:
                                //
                                return (new Addons.Primitives.processSendPasswordMethodClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case HardCodedPageResourceLibrary:
                                //
                                return (new Addons.Primitives.processResourceLibraryMethodClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case HardCodedPageLoginDefault:
                                //
                                return (new Addons.Primitives.processLoginDefaultMethodClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case HardCodedPageLogin:
                                //
                                return (new Addons.Primitives.processLoginMethodClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case HardCodedPageLogoutLogin:
                                //
                                return (new Addons.Primitives.processLogoutLoginMethodClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case HardCodedPageSiteExplorer:
                                //
                                return (new Addons.Primitives.processSiteExplorerMethodClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case HardCodedPageStatus:
                                //
                                return (new Addons.Primitives.processStatusMethodClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case HardCodedPageRedirect:
                                //
                                return (new Addons.Primitives.processRedirectMethodClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case HardCodedPageExportAscii:
                                //
                                return (new Addons.Primitives.processExportAsciiMethodClass()).Execute(cp_forAddonExecutionOnly).ToString();
                                //case HardCodedPagePayPalConfirm:
                                //    //
                                //    return (new Addons.Primitives.processPayPalConformMethodClass()).Execute(cp_forAddonExecutionOnly).ToString();
                        }
                    }
                    //
                    // -- try route Dictionary (addons, admin, link forwards, link alias), from full route to first segment one at a time
                    // -- so route /this/and/that would first test /this/and/that, then test /this/and, then test /this
                    string routeTest = normalizedRoute;
                    bool routeFound = false;
                    int routeCnt = 100;
                    do {
                        routeFound = routeMap.routeDictionary.ContainsKey(routeTest);
                        if (routeFound) {
                            break;
                        }
                        if (routeTest.IndexOf("/") < 0) {
                            break;
                        }
                        routeTest = routeTest.Left(routeTest.LastIndexOf("/"));
                        routeCnt -= 1;
                    } while ((routeCnt > 0) && (!routeFound));
                    //
                    // -- execute route
                    if (routeFound) {
                         RouteMapModel.routeClass route = routeMap.routeDictionary[routeTest];
                        switch (route.routeType) {
                            case RouteMapModel.routeTypeEnum.admin: {
                                    //
                                    // -- admin site
                                    AddonModel addon = AddonModel.create(this, addonGuidAdminSite);
                                    if (addon == null) {
                                        LogController.handleError( this,new GenericException("The admin site addon could not be found by guid [" + addonGuidAdminSite + "]."));
                                        return "The default admin site addon could not be found. Please run an upgrade on this application to restore default services (command line> cc -a appName -r )";
                                    } else {
                                        return this.addon.execute(addon, new CPUtilsBaseClass.addonExecuteContext() {
                                            addonType = CPUtilsBaseClass.addonContext.ContextAdmin,
                                            errorContextMessage = "calling admin route [" + addonGuidAdminSite + "] during execute route method"
                                        });
                                    }
                                }
                            case RouteMapModel.routeTypeEnum.remoteMethod: {
                                    //
                                    // -- remote method
                                    AddonModel addon = addonCache.getAddonById(route.remoteMethodAddonId);
                                    if (addon == null) {
                                        LogController.handleError( this,new GenericException("The addon for remoteMethodAddonId [" + route.remoteMethodAddonId + "] could not be opened."));
                                        return "";
                                    } else { 
                                        CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                                            addonType = CPUtilsBaseClass.addonContext.ContextRemoteMethodJson,
                                            cssContainerClass = "",
                                            cssContainerId = "",
                                            hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                                                contentName = docProperties.getText("hostcontentname"),
                                                fieldName = "",
                                                recordId = docProperties.getInteger("HostRecordID")
                                            },
                                            personalizationAuthenticated = session.isAuthenticated,
                                            personalizationPeopleId = session.user.id,
                                            errorContextMessage = "calling remote method addon [" + route.remoteMethodAddonId + "] during execute route method"
                                        };
                                        return this.addon.execute(addon, executeContext);
                                    }
                                }
                            case RouteMapModel.routeTypeEnum.linkAlias:
                                //
                                // - link alias
                                LinkAliasModel linkAlias = LinkAliasModel.create(this, route.linkAliasId);
                                if (linkAlias != null) {
                                    docProperties.setProperty("bid", linkAlias.pageID);
                                    if (!string.IsNullOrEmpty(linkAlias.queryStringSuffix)) {
                                        string[] nvp = linkAlias.queryStringSuffix.Split('&');
                                        foreach (var nv in nvp) {
                                            string[] keyValue = nv.Split('=');
                                            if (!string.IsNullOrEmpty(keyValue[0])) {
                                                if (keyValue.Length > 1) {
                                                    siteProperties.setProperty(keyValue[0], keyValue[1]);
                                                } else {
                                                    siteProperties.setProperty(keyValue[0], string.Empty);
                                                }
                                            }
                                        }
                                    }

                                }
                                break;
                            case RouteMapModel.routeTypeEnum.linkForward:
                                //
                                // -- link forward
                                LinkForwardModel linkForward = LinkForwardModel.create(this, route.linkForwardId);
                                return webServer.redirect(linkForward.DestinationLink, "Link Forward #" + linkForward.id + ", " + linkForward.name);
                        }
                    }
                    if (normalizedRoute.Equals("favicon.ico")) {
                        //
                        // -- Favicon.ico
                        doc.continueProcessing = false;
                        return (new Addons.Primitives.faviconIcoClass()).Execute(cp_forAddonExecutionOnly).ToString();
                    }
                    if (normalizedRoute.Equals("robots.txt")) {
                        //
                        // -- Favicon.ico
                        doc.continueProcessing = false;
                        return (new Addons.Primitives.robotsTxtClass()).Execute(cp_forAddonExecutionOnly).ToString();
                    }
                    //
                    // -- default route 
                    int defaultAddonId = 0;
                    if (doc.domain != null) {
                        defaultAddonId = doc.domain.defaultRouteId;
                    }
                    if (defaultAddonId == 0) {
                        defaultAddonId = siteProperties.defaultRouteId;
                    }
                    if (defaultAddonId > 0) {
                        //
                        // -- default route is run if no other route is found, which includes the route=defaultPage (default.aspx)
                        CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                            addonType = CPUtilsBaseClass.addonContext.ContextPage,
                            cssContainerClass = "",
                            cssContainerId = "",
                            hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                                contentName = "",
                                fieldName = "",
                                recordId = 0
                            },
                            personalizationAuthenticated = session.visit.visitAuthenticated,
                            personalizationPeopleId = session.user.id,
                            errorContextMessage = "calling default route addon [" + defaultAddonId + "] during execute route method"
                        };
                        return this.addon.execute(Models.Db.AddonModel.create(this, defaultAddonId), executeContext);
                    }
                    //
                    // -- no route
                    result = "<p>This site is not configured for website traffic. Please set the default route.</p>";
                }
            } catch (Exception ex) {
                LogController.handleError( this,ex);
            } finally {
                // if (doc.routeDictionaryChanges) { DefaultSite.configurationClass.loadRouteMap(cp))}
                LogController.forceNLog("CoreController executeRoute, exit", LogController.logLevel.Trace);
            }
            return result;
        }
        //
        //=================================================================================================
        /// <summary>
        /// Run and return results from a remotequery call from cj.ajax.data(handler,key,args,pagesize,pagenumber)
        /// This routine builds an xml object inside a <result></result> node. 
        /// Right now, the response is in JSON format, and conforms to the google data visualization spec 0.5
        /// </summary>
        /// <returns></returns>
        public string executeRoute_ProcessAjaxData() {
            string result = "";
            try {
                LogController.handleError( this,new GenericException("executeRoute_ProcessAjaxData deprecated"));
                //string RemoteKey = docProperties.getText("key");
                //string EncodedArgs = docProperties.getText("args");
                //int PageSize = docProperties.getInteger("pagesize");
                //int PageNumber = docProperties.getInteger("pagenumber");
                //RemoteFormatEnum RemoteFormat = null;
                //switch (genericController.vbLCase(docProperties.getText("responseformat"))) {
                //    case "jsonnamevalue":
                //        RemoteFormat = RemoteFormatEnum.RemoteFormatJsonNameValue;
                //        break;
                //    case "jsonnamearray":
                //        RemoteFormat = RemoteFormatEnum.RemoteFormatJsonNameArray;
                //        break;
                //    default: //jsontable
                //        RemoteFormat = RemoteFormatEnum.RemoteFormatJsonTable;
                //        break;
                //}
                ////
                //return "";
                ////
                ////
                //// Handle common work
                ////
                //if (PageNumber == 0) {
                //    PageNumber = 1;
                //}
                //if (PageSize == 0) {
                //    PageSize = 100;
                //}
                //int maxRows = 0;
                //if (maxRows != 0 && PageSize > maxRows) {
                //    PageSize = maxRows;
                //}
                ////
                //string[] ArgName = { };
                //string[] ArgValue = { };
                //if (!string.IsNullOrEmpty(EncodedArgs)) {
                //    string Args = EncodedArgs;
                //    string[] ArgArray = Args.Split('&');
                //    int ArgCnt = ArgArray.GetUpperBound(0) + 1;
                //    ArgName = new string[ArgCnt + 1];
                //    ArgValue = new string[ArgCnt + 1];
                //    for (var Ptr = 0; Ptr < ArgCnt; Ptr++) {
                //        int Pos = genericController.vbInstr(1, ArgArray[Ptr], "=");
                //        if (Pos > 0) {
                //            ArgName[Ptr] = genericController.DecodeResponseVariable(ArgArray[Ptr].Left( Pos - 1));
                //            ArgValue[Ptr] = genericController.DecodeResponseVariable(ArgArray[Ptr].Substring(Pos));
                //        }
                //    }
                //}
                ////
                //// main_Get values out of the remote query record
                ////
                //GoogleVisualizationType gv = new GoogleVisualizationType();
                //gv.status = GoogleVisualizationStatusEnum.OK;
                ////
                //if (gv.status == GoogleVisualizationStatusEnum.OK) {
                //    string SetPairString = "";
                //    int QueryType = 0;
                //    string ContentName = "";
                //    string Criteria = "";
                //    string SortFieldList = "";
                //    bool AllowInactiveRecords2 = false;
                //    string SelectFieldList = "";
                //    int CS = db.csOpen("Remote Queries", "((VisitId=" + doc.authContext.visit.id + ")and(remotekey=" + DbController.encodeSQLText(RemoteKey) + "))");
                //    if (db.csOk(CS)) {
                //        //
                //        // Use user definied query
                //        //
                //        string SQLQuery = db.csGetText(CS, "sqlquery");
                //        //DataSource = dataSourceModel.create(Me, db.cs_getInteger(CS, "datasourceid"), New List(Of String))
                //        maxRows = db.csGetInteger(CS, "maxrows");
                //        QueryType = db.csGetInteger(CS, "QueryTypeID");
                //        ContentName = db.csGet(CS, "ContentID");
                //        Criteria = db.csGetText(CS, "Criteria");
                //        SortFieldList = db.csGetText(CS, "SortFieldList");
                //        AllowInactiveRecords2 = db.csGetBoolean(CS, "AllowInactiveRecords");
                //        SelectFieldList = db.csGetText(CS, "SelectFieldList");
                //    } else {
                //        //
                //        // Try Hardcoded queries
                //        //
                //        switch (genericController.vbLCase(RemoteKey)) {
                //            case "ccfieldhelpupdate":
                //                //
                //                // developers editing field help
                //                //
                //                if (!doc.authContext.user.Developer) {
                //                    gv.status = GoogleVisualizationStatusEnum.ErrorStatus;
                //                    int Ptr = 0;
                //                    if (gv.errors.GetType().IsArray) {
                //                        Ptr = gv.errors.GetUpperBound(0) + 1;
                //                    }
                //                    Array.Resize(ref gv.errors, Ptr);
                //                    gv.errors[Ptr] = "permission error";
                //                } else {
                //                    QueryType = QueryTypeUpdateContent;
                //                    ContentName = "Content Field Help";
                //                    Criteria = "";
                //                    AllowInactiveRecords2 = false;
                //                }
                //                //Case Else
                //                //    '
                //                //    ' query not found
                //                //    '
                //                //    gv.status = GoogleVisualizationStatusEnum.ErrorStatus
                //                //    If IsArray(gv.errors) Then
                //                //        Ptr = 0
                //                //    Else
                //                //        Ptr = UBound(gv.errors) + 1
                //                //    End If
                //                //    ReDim gv.errors[Ptr]
                //                //    gv.errors[Ptr] = "query not found"
                //                break;
                //        }
                //    }
                //    db.csClose(ref CS);
                //    //
                //    if (gv.status == GoogleVisualizationStatusEnum.OK) {
                //        switch (QueryType) {
                //            case QueryTypeUpdateContent:
                //                //
                //                // Contensive Content Update, args are field=value updates
                //                // !!!! only allow inbound hits with a referrer from this site - later use the aggregate access table
                //                //
                //                //
                //                // Go though args and main_Get Set and Criteria
                //                //
                //                SetPairString = "";
                //                Criteria = "";
                //                for (var Ptr = 0; Ptr < ArgName.Length; Ptr++) {
                //                    if (genericController.vbLCase(ArgName[Ptr]) == "setpairs") {
                //                        SetPairString = ArgValue[Ptr];
                //                    } else if (genericController.vbLCase(ArgName[Ptr]) == "criteria") {
                //                        Criteria = ArgValue[Ptr];
                //                    }
                //                }
                //                //
                //                // Open the content and cycle through each setPair
                //                //
                //                CS = db.csOpen(ContentName, Criteria, SortFieldList, AllowInactiveRecords2, 0, false, false, SelectFieldList);
                //                if (db.csOk(CS)) {
                //                    //
                //                    // update by looping through the args and setting name=values
                //                    //
                //                    string[] SetPairs = SetPairString.Split('&');
                //                    for (var Ptr = 0; Ptr <= SetPairs.GetUpperBound(0); Ptr++) {
                //                        if (!string.IsNullOrEmpty(SetPairs[Ptr])) {
                //                            int Pos = genericController.vbInstr(1, SetPairs[Ptr], "=");
                //                            if (Pos > 0) {
                //                                string FieldValue = genericController.DecodeResponseVariable(SetPairs[Ptr].Substring(Pos));
                //                                string FieldName = genericController.DecodeResponseVariable(SetPairs[Ptr].Left( Pos - 1));
                //                                if (!Models.Complex.cdefModel.isContentFieldSupported(this, ContentName, FieldName)) {
                //                                    string errorMessage = "result, QueryTypeUpdateContent, key [" + RemoteKey + "], bad field [" + FieldName + "] skipped";
                //                                    throw (new GenericException(errorMessage));
                //                                } else {
                //                                    db.csSet(CS, FieldName, FieldValue);
                //                                }
                //                            }
                //                        }
                //                    }
                //                }
                //                db.csClose(ref CS);
                //                //Case QueryTypeInsertContent
                //                //    '
                //                //    ' !!!! only allow inbound hits with a referrer from this site - later use the aggregate access table
                //                //    '
                //                //    '
                //                //    ' Contensive Content Insert, args are field=value
                //                //    '
                //                //    'CS = main_InsertCSContent(ContentName)
                //                break;
                //            default:
                //                break;
                //        }
                //        //
                //        // output
                //        //
                //        GoogleDataType gd = new GoogleDataType();
                //        gd.IsEmpty = true;
                //        //
                //        string Copy = remoteQueryController.main_FormatRemoteQueryOutput(this, gd, RemoteFormat);
                //        Copy = htmlController.encodeHTML(Copy);
                //        result = "<data>" + Copy + "</data>";
                //    }
                //}
            } catch (Exception ex) {
                throw (ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// coreClass constructor common tasks.
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        private void constructorInitialize(bool allowVisit) {
            try {
                //
                doc.docGuid = GenericController.getGUID();
                doc.allowDebugLog = true;
                doc.profileStartTime = DateTime.Now;
                doc.visitPropertyAllowDebugging = true;
                //
                // -- attempt auth load
                if (appConfig == null) {
                    //
                    // -- server mode, there is no application
                    session = SessionController.create(this, false);
                } else if (appConfig.appStatus != AppConfigModel.AppStatusEnum.ok) {
                    //} else if ((appConfig.appMode != appConfigModel.appModeEnum.normal) || (appConfig.appStatus != appConfigModel.appStatusEnum.OK)) {
                    //
                    // -- application is not ready, might be error, or in maintainence mode
                    session = SessionController.create(this, false);
                } else {
                    session = SessionController.create(this, allowVisit && siteProperties.allowVisitTracking);
                    //
                    // -- debug defaults on, so if not on, set it off and clear what was collected
                    doc.visitPropertyAllowDebugging = visitProperty.getBoolean("AllowDebugging");
                    if (!doc.visitPropertyAllowDebugging) {
                        doc.testPointMessage = "";
                    }
                }
            } catch (Exception ex) {
                throw (ex);
            }
        }
        //
        //
        //====================================================================================================
        /// <summary>
        /// version for core assembly
        /// </summary>
        /// <remarks></remarks>
        public string codeVersion() {
            Type myType = typeof(CoreController);
            Assembly myAssembly = Assembly.GetAssembly(myType);
            AssemblyName myAssemblyname = myAssembly.GetName();
            Version myVersion = myAssemblyname.Version;
            return myVersion.Major.ToString("0") + "." + myVersion.Minor.ToString("00") + "." + myVersion.Build.ToString("00000000");
        }
        //
        //====================================================================================================
        /// <summary>
        /// Clear all data from the metaData current instance. Next request will load from cache.
        /// </summary>
        public void clearMetaData() {
            if (cdefDictionary != null) {
                cdefDictionary.Clear();
            }
            if (tableSchemaDictionary != null) {
                tableSchemaDictionary.Clear();
            }
            contentNameIdDictionaryClear();
        }
        //
        //====================================================================================================
        //
        internal void contentNameIdDictionaryClear() {
            _contentNameIdDictionary = null;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Process manual changes needed for special cases
        /// </summary>
        /// <param name="IsDelete"></param>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <param name="RecordName"></param>
        /// <param name="RecordParentID"></param>
        /// <param name="UseContentWatchLink"></param>
        public void processAfterSave(bool IsDelete, string ContentName, int RecordID, string RecordName, int RecordParentID, bool UseContentWatchLink) {
            try {
                int ContentID = CdefController.getContentId(this, ContentName);
                string TableName = CdefController.getContentTablename(this, ContentName);
                PageContentModel.markReviewed(this, RecordID);
                //
                // -- invalidate the specific cache for this record
                cache.invalidateDbRecord(RecordID, TableName);
                int CS = 0;
                int ActivityLogOrganizationID = 0;
                //
                switch (GenericController.vbLCase(TableName)) {
                    case LinkForwardModel.contentTableName:
                        //
                        Models.Domain.RouteMapModel.invalidateCache(this);
                        routeMapClearLocalCache();
                        break;
                    case LinkAliasModel.contentTableName:
                        //
                        Models.Domain.RouteMapModel.invalidateCache(this);
                        routeMapClearLocalCache();
                        break;
                    case AddonModel.contentTableName:
                        //
                        Models.Domain.RouteMapModel.invalidateCache(this);
                        routeMapClearLocalCache();
                        cache.invalidate(cacheObject_addonCache);
                        cache.invalidateDbRecord(RecordID, TableName);
                        break;
                    case PersonModel.contentTableName:
                        //
                        CS = db.csOpen2("people", RecordID, false, false, "Name,OrganizationID");
                        if (db.csOk(CS)) {
                            ActivityLogOrganizationID = db.csGetInteger(CS, "OrganizationID");
                        }
                        db.csClose(ref CS);
                        if (IsDelete) {
                            LogController.addSiteActivity(this, "deleting user #" + RecordID + " (" + RecordName + ")", RecordID, ActivityLogOrganizationID);
                        } else {
                            LogController.addSiteActivity(this, "saving changes to user #" + RecordID + " (" + RecordName + ")", RecordID, ActivityLogOrganizationID);
                        }
                        break;
                    case "organizations":
                        //
                        // Log Activity for changes to people and organizattions
                        //
                        //hint = hint & ",120"
                        if (IsDelete) {
                            LogController.addSiteActivity(this, "deleting organization #" + RecordID + " (" + RecordName + ")", 0, RecordID);
                        } else {
                            LogController.addSiteActivity(this, "saving changes to organization #" + RecordID + " (" + RecordName + ")", 0, RecordID);
                        }
                        break;
                    case "ccsetup":
                        //
                        // Site Properties
                        //
                        //hint = hint & ",130"
                        switch (GenericController.vbLCase(RecordName)) {
                            case "allowlinkalias":
                                PageContentModel.invalidateTableCache(this);
                                break;
                            case "sectionlandinglink":
                                PageContentModel.invalidateTableCache(this);
                                break;
                            case _siteproperty_serverPageDefault_name:
                                PageContentModel.invalidateTableCache(this);
                                break;
                        }
                        break;
                    case "ccpagecontent":
                        //
                        // set ChildPagesFound true for parent page
                        //
                        //hint = hint & ",140"
                        if (RecordParentID > 0) {
                            if (!IsDelete) {
                                db.executeQuery("update ccpagecontent set ChildPagesfound=1 where ID=" + RecordParentID);
                            }
                        }
                        //
                        // Page Content special cases for delete
                        //
                        if (IsDelete) {
                            //
                            // Clear the Landing page and page not found site properties
                            //
                            if (RecordID == GenericController.encodeInteger(siteProperties.getText("PageNotFoundPageID", "0"))) {
                                siteProperties.setProperty("PageNotFoundPageID", "0");
                            }
                            if (RecordID == siteProperties.landingPageID) {
                                siteProperties.setProperty("landingPageId", "0");
                            }
                            //
                            // Delete Link Alias entries with this PageID
                            //
                            db.executeQuery("delete from cclinkAliases where PageID=" + RecordID);
                        }
                        PageContentModel.invalidateRecordCache(this, RecordID);
                        break;
                    case "cclibraryfiles":
                        //
                        // if a AltSizeList is blank, make large,medium,small and thumbnails
                        //
                        //hint = hint & ",180"
                        if (siteProperties.getBoolean("ImageAllowSFResize", true)) {
                            if (!IsDelete) {
                                CS = db.csOpenRecord("library files", RecordID);
                                if (db.csOk(CS)) {
                                    string Filename = db.csGet(CS, "filename");
                                    int Pos = Filename.LastIndexOf("/") + 1;
                                    string FilePath = "";
                                    if (Pos > 0) {
                                        FilePath = Filename.Left(Pos);
                                        Filename = Filename.Substring(Pos);
                                    }
                                    db.csSet(CS, "filesize", appRootFiles.getFileSize(FilePath + Filename));
                                    Pos = Filename.LastIndexOf(".") + 1;
                                    if (Pos > 0) {
                                        string FilenameExt = Filename.Substring(Pos);
                                        string FilenameNoExt = Filename.Left(Pos - 1);
                                        if (GenericController.vbInstr(1, "jpg,gif,png", FilenameExt, 1) != 0) {
                                            ImageEditController sf = new ImageEditController();
                                            if (sf.load(FilePath + Filename, appRootFiles)) {
                                                //
                                                //
                                                //
                                                db.csSet(CS, "height", sf.height);
                                                db.csSet(CS, "width", sf.width);
                                                string AltSizeList = db.csGetText(CS, "AltSizeList");
                                                bool RebuildSizes = (string.IsNullOrEmpty(AltSizeList));
                                                if (RebuildSizes) {
                                                    AltSizeList = "";
                                                    //
                                                    // Attempt to make 640x
                                                    //
                                                    if (sf.width >= 640) {
                                                        sf.height = encodeInteger(sf.height * (640 / sf.width));
                                                        sf.width = 640;
                                                        sf.save(FilePath + FilenameNoExt + "-640x" + sf.height + "." + FilenameExt, appRootFiles);
                                                        AltSizeList = AltSizeList + "\r\n640x" + sf.height;
                                                    }
                                                    //
                                                    // Attempt to make 320x
                                                    //
                                                    if (sf.width >= 320) {
                                                        sf.height = encodeInteger(sf.height * (320 / sf.width));
                                                        sf.width = 320;
                                                        sf.save(FilePath + FilenameNoExt + "-320x" + sf.height + "." + FilenameExt, appRootFiles);

                                                        AltSizeList = AltSizeList + "\r\n320x" + sf.height;
                                                    }
                                                    //
                                                    // Attempt to make 160x
                                                    //
                                                    if (sf.width >= 160) {
                                                        sf.height = encodeInteger(sf.height * (160 / sf.width));
                                                        sf.width = 160;
                                                        sf.save(FilePath + FilenameNoExt + "-160x" + sf.height + "." + FilenameExt, appRootFiles);
                                                        AltSizeList = AltSizeList + "\r\n160x" + sf.height;
                                                    }
                                                    //
                                                    // Attempt to make 80x
                                                    //
                                                    if (sf.width >= 80) {
                                                        sf.height = encodeInteger(sf.height * (80 / sf.width));
                                                        sf.width = 80;
                                                        sf.save(FilePath + FilenameNoExt + "-180x" + sf.height + "." + FilenameExt, appRootFiles);
                                                        AltSizeList = AltSizeList + "\r\n80x" + sf.height;
                                                    }
                                                    db.csSet(CS, "AltSizeList", AltSizeList);
                                                }
                                                sf.Dispose();
                                                sf = null;
                                            }
                                            //                                sf.Algorithm = genericController.EncodeInteger(main_GetSiteProperty("ImageResizeSFAlgorithm", "5"))
                                            //                                //On Error //Resume Next
                                            //                                sf.LoadFromFile (app.publicFiles.rootFullPath & FilePath & Filename)
                                            //                                If Err.Number = 0 Then
                                            //                                    Call app.SetCS(CS, "height", sf.Height)
                                            //                                    Call app.SetCS(CS, "width", sf.Width)
                                            //                                Else
                                            //                                    Err.Clear
                                            //                                End If
                                            //                                AltSizeList = cs_getText(CS, "AltSizeList")
                                            //                                RebuildSizes = (AltSizeList = "")
                                            //                                If RebuildSizes Then
                                            //                                    AltSizeList = ""
                                            //                                    '
                                            //                                    ' Attempt to make 640x
                                            //                                    '
                                            //                                    If sf.Width >= 640 Then
                                            //                                        sf.Width = 640
                                            //                                        Call sf.DoResize
                                            //                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-640x" & sf.Height & "." & FilenameExt)
                                            //                                        AltSizeList = AltSizeList & vbCrLf & "640x" & sf.Height
                                            //                                    End If
                                            //                                    '
                                            //                                    ' Attempt to make 320x
                                            //                                    '
                                            //                                    If sf.Width >= 320 Then
                                            //                                        sf.Width = 320
                                            //                                        Call sf.DoResize
                                            //                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-320x" & sf.Height & "." & FilenameExt)
                                            //                                        AltSizeList = AltSizeList & vbCrLf & "320x" & sf.Height
                                            //                                    End If
                                            //                                    '
                                            //                                    ' Attempt to make 160x
                                            //                                    '
                                            //                                    If sf.Width >= 160 Then
                                            //                                        sf.Width = 160
                                            //                                        Call sf.DoResize
                                            //                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-160x" & sf.Height & "." & FilenameExt)
                                            //                                        AltSizeList = AltSizeList & vbCrLf & "160x" & sf.Height
                                            //                                    End If
                                            //                                    '
                                            //                                    ' Attempt to make 80x
                                            //                                    '
                                            //                                    If sf.Width >= 80 Then
                                            //                                        sf.Width = 80
                                            //                                        Call sf.DoResize
                                            //                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-80x" & sf.Height & "." & FilenameExt)
                                            //                                        AltSizeList = AltSizeList & vbCrLf & "80x" & sf.Height
                                            //                                    End If
                                            //                                    Call app.SetCS(CS, "AltSizeList", AltSizeList)
                                            //                                End If
                                            //                                sf = Nothing
                                        }
                                    }
                                }
                                db.csClose(ref CS);
                            }
                        }
                        break;
                    default:
                        break;
                }
                //
                // Process Addons marked to trigger a process call on content change
                //
                Dictionary<string, string> instanceArguments;
                bool onChangeAddonsAsync = siteProperties.getBoolean("execute oncontentchange addons async", false);
                CS = db.csOpen("Add-on Content Trigger Rules", "ContentID=" + ContentID, "", false, 0, false, false, "addonid");
                string Option_String = null;
                if (IsDelete) {
                    instanceArguments = new Dictionary<string, string>() {
                    {"action","contentdelete"},
                    {"contentid",ContentID.ToString()},
                    {"recordid",RecordID.ToString()}
                };
                    Option_String = ""
                        + "\r\naction=contentdelete"
                        + "\r\ncontentid=" + ContentID
                        + "\r\nrecordid=" + RecordID + "";
                } else {
                    instanceArguments = new Dictionary<string, string>() {
                    {"action","contentchange"},
                    {"contentid",ContentID.ToString()},
                    {"recordid",RecordID.ToString()}
                };
                    Option_String = ""
                        + "\r\naction=contentchange"
                        + "\r\ncontentid=" + ContentID
                        + "\r\nrecordid=" + RecordID + "";
                }
                while (db.csOk(CS)) {
                    int addonId = db.csGetInteger(CS, "Addonid");
                    //hint = hint & ",210 addonid=[" & addonId & "]"
                    // convert for forground execution for now
                    if (onChangeAddonsAsync) {
                        //
                        // -- execute addon async
                        addon.executeAsync(AddonModel.create(this, addonId), instanceArguments);
                    } else {
                        //
                        // -- execute addon
                        addon.execute(addonId, new CPUtilsBaseClass.addonExecuteContext() {
                            addonType = CPUtilsBaseClass.addonContext.ContextOnContentChange,
                            backgroundProcess = false,
                            errorContextMessage = "",
                            argumentKeyValuePairs = instanceArguments,
                            personalizationPeopleId = session.user.id
                        });
                    }
                    db.csGoNext(CS);
                }
                db.csClose(ref CS);
            } catch (Exception ex) {
                LogController.handleError(this, ex);
            }
        }
        //   
        #region  IDisposable Support 
        //
        protected bool disposed = false;
        //====================================================================================================
        /// <summary>
        /// dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            LogController.forceNLog("CoreController dispose, enter", LogController.logLevel.Trace);
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    // ----- Block all output from underlying routines
                    //
                    doc.blockExceptionReporting = true;
                    doc.continueProcessing = false;
                    //
                    // -- save assemblySkipList
                    if (_assemblySkipList != null) {
                        if (_assemblySkipList.Count > _assemblySkipList_CountWhenLoaded) {
                            LogController.forceNLog("CoreController dispose, save assemblySkipList to cache, _assemblySkipList.Count [" + _assemblySkipList.Count + "], _assemblySkipList_CountWhenLoaded [" + _assemblySkipList_CountWhenLoaded + "]", LogController.logLevel.Trace);
                            cache.storeObject(cacheNameAssemblySkipList, _assemblySkipList);
                        }
                    }
                    //
                    // content server object is valid
                    //
                    if (serverConfig != null) {
                        if (appConfig != null) {
                            if (appConfig.appStatus == AppConfigModel.AppStatusEnum.ok) {
                                //if ((appConfig.appMode == appConfigModel.appModeEnum.normal) && (appConfig.appStatus == appConfigModel.appStatusEnum.OK))
                                if (siteProperties.allowVisitTracking) {
                                    //
                                    // If visit tracking, save the viewing record
                                    //
                                    string ViewingName = ((string)(session.visit.id + "." + session.visit.pageVisits)).Left(10);
                                    int PageID = 0;
                                    if (_doc != null) {
                                        if (doc.pageController.page != null) {
                                            PageID = doc.pageController.page.id;
                                        }
                                    }
                                    //
                                    // -- convert requestFormDict to a name=value string for Db storage
                                    string requestFormSerialized = GenericController.convertNameValueDictToREquestString(webServer.requestFormDict);
                                    string pagetitle = "";
                                    if (!doc.htmlMetaContent_TitleList.Count.Equals(0)) {
                                        pagetitle = doc.htmlMetaContent_TitleList[0].content;
                                    }
                                    string SQL = "insert into ccviewings ("
                                        + "Name,VisitId,MemberID,Host,Path,Page,QueryString,Form,Referer,DateAdded,StateOK,pagetime,Active,RecordID,ExcludeFromAnalytics,pagetitle"
                                        + ")values("
                                        + " " + DbController.encodeSQLText(ViewingName)
                                        + "," + session.visit.id.ToString()
                                        + "," + session.user.id.ToString()
                                        + "," + DbController.encodeSQLText(webServer.requestDomain)
                                        + "," + DbController.encodeSQLText(webServer.requestPath)
                                        + "," + DbController.encodeSQLText(webServer.requestPage)
                                        + "," + DbController.encodeSQLText(webServer.requestQueryString.Left(255))
                                        + "," + DbController.encodeSQLText(requestFormSerialized.Left(255))
                                        + "," + DbController.encodeSQLText(webServer.requestReferrer.Left(255))
                                        + "," + DbController.encodeSQLDate(doc.profileStartTime)
                                        + "," + DbController.encodeSQLBoolean(session.visitStateOk)
                                        + "," + doc.appStopWatch.ElapsedMilliseconds.ToString()
                                        + ",1"
                                        + "," + PageID.ToString()
                                        + "," + DbController.encodeSQLBoolean(webServer.pageExcludeFromAnalytics)
                                        + "," + DbController.encodeSQLText(pagetitle);
                                    SQL += ");";
                                    db.executeNonQueryAsync(SQL);
                                }
                            }
                        }
                    }
                    //
                    // ----- dispose objects created here
                    //
                    if (_addon != null) {
                        _addon.Dispose();
                        _addon = null;
                    }
                    //
                    if (_db != null) {
                        _db.Dispose();
                        _db = null;
                    }
                    //
                    if (_cache != null) {
                        _cache.Dispose();
                        _cache = null;
                    }
                    //
                    if (_siteProperties != null) {
                        // no dispose
                        //Call _siteProperties.Dispose()
                        _siteProperties = null;
                    }
                    //
                    if (_json != null) {
                        // no dispose
                        //Call _json.Dispose()
                        _json = null;
                    }
                    //
                    //If Not (_user Is Nothing) Then
                    //    ' no dispose
                    //    'Call _user.Dispose()
                    //    _user = Nothing
                    //End If
                    //
                    if (_domains != null) {
                        // no dispose
                        //Call _domains.Dispose()
                        _domains = null;
                    }
                    //
                    if (_docProperties != null) {
                        // no dispose
                        //Call _doc.Dispose()
                        _docProperties = null;
                    }
                    //
                    if (_webServer != null) {
                        // no dispose
                        //Call _webServer.Dispose()
                        _webServer = null;
                    }
                    //
                    //if (_menuFlyout != null) {
                    //    // no dispose
                    //    //Call _menuFlyout.Dispose()
                    //    _menuFlyout = null;
                    //}
                    //
                    if (_visitProperty != null) {
                        // no dispose
                        //Call _visitProperty.Dispose()
                        _visitProperty = null;
                    }
                    //
                    if (_visitorProperty != null) {
                        // no dispose
                        //Call _visitorProperty.Dispose()
                        _visitorProperty = null;
                    }
                    //
                    if (_userProperty != null) {
                        // no dispose
                        //Call _userProperty.Dispose()
                        _userProperty = null;
                    }
                    //
                    if (_db != null) {
                        _db.Dispose();
                        _db = null;
                    }
                    //
                    if (useNlog) {
                        // flush all the logs used
                        foreach (var kvp in doc.logList) {
                            kvp.Value.Flush();
                        }
                    }
                }
                //
                // cleanup non-managed objects
                //
            }
            LogController.forceNLog("CoreController dispose, exit", LogController.logLevel.Trace);
        }
        //
        public void Dispose() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        ~CoreController() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(false);
        }
        #endregion
    }
}
