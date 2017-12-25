
using System;
using System.Reflection;
using Contensive.BaseClasses;
using Contensive.Core.Controllers;
using Contensive.Core.Models.Context;
using Contensive.Core.Models.Entity;
using System.Collections.Generic;
using static Contensive.Core.constants;
using System.Diagnostics;
using System.Linq;
//
namespace Contensive.Core {
    // todo - this is not 'cpCore'. It is a 
    public class coreClass : IDisposable {
        //
        //======================================================================
        // -- provides object dependancy injection
        //
        internal CPClass cp_forAddonExecutionOnly { get; set; }
        // todo - take appConfig out of serverConfig. saved server structure should not include it.
        public Models.Context.serverConfigModel serverConfig { get; set; }
        // todo move persistent objects to .doc (keeping of document scope persistence)
        public Random random = new Random();
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
                        _assemblySkipList = new List<string>();
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
        public Dictionary<string, dataSourceModel> dataSourceDictionary {
            get {
                if (_dataSources == null) {
                    _dataSources = dataSourceModel.getNameDict(this);
                }
                return _dataSources;
            }
        }
        private Dictionary<string, dataSourceModel> _dataSources = null;
        //
        //===================================================================================================
        public emailController email {
            get {
                if (_email == null) {
                    _email = new emailController(this);
                }
                return _email;
            }
        }
        private emailController _email;
        //
        //===================================================================================================
        public docController doc {
            get {
                if (_doc == null) {
                    _doc = new docController(this);
                }
                return _doc;
            }
        }
        private docController _doc;
        //
        //===================================================================================================
        public menuTabController menuTab {
            get {
                if (_menuTab == null) {
                    _menuTab = new menuTabController(this);
                }
                return _menuTab;
            }
        }
        private menuTabController _menuTab;
        //
        //===================================================================================================
        public Controllers.htmlController html {
            get {
                if (_html == null) {
                    _html = new Controllers.htmlController(this);
                }
                return _html;
            }
        }
        private Controllers.htmlController _html;
        //
        //===================================================================================================
        public Controllers.addonController addon {
            get {
                if (_addon == null) {
                    _addon = new Controllers.addonController(this);
                }
                return _addon;
            }
        }
        private Controllers.addonController _addon;
        //
        //===================================================================================================
        public menuFlyoutController menuFlyout {
            get {
                if (_menuFlyout == null) {
                    _menuFlyout = new menuFlyoutController(this);
                }
                return _menuFlyout;
            }
        }
        private menuFlyoutController _menuFlyout;
        //
        //===================================================================================================
        public propertyModelClass userProperty {
            get {
                if (_userProperty == null) {
                    _userProperty = new propertyModelClass(this, PropertyTypeMember);
                }
                return _userProperty;
            }
        }
        private propertyModelClass _userProperty;
        //
        //===================================================================================================
        public propertyModelClass visitorProperty {
            get {
                if (_visitorProperty == null) {
                    _visitorProperty = new propertyModelClass(this, PropertyTypeVisitor);
                }
                return _visitorProperty;
            }
        }
        private propertyModelClass _visitorProperty;
        //
        //===================================================================================================
        public propertyModelClass visitProperty {
            get {
                if (_visitProperty == null) {
                    _visitProperty = new propertyModelClass(this, PropertyTypeVisit);
                }
                return _visitProperty;
            }
        }
        private propertyModelClass _visitProperty;
        //
        //===================================================================================================
        public docPropertyController docProperties {
            get {
                if (_docProperties == null) {
                    _docProperties = new docPropertyController(this);
                }
                return _docProperties;
            }
        }
        private docPropertyController _docProperties = null;
        //
        //===================================================================================================
        /// <summary>
        /// siteProperties object
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public sitePropertiesController siteProperties {
            get {
                if (_siteProperties == null) {
                    _siteProperties = new sitePropertiesController(this);
                }
                return _siteProperties;
            }
        }
        private sitePropertiesController _siteProperties = null;
        //
        //===================================================================================================
        public iisController webServer {
            get {
                if (_webServer == null) {
                    _webServer = new iisController(this);
                }
                return _webServer;
            }
        }
        private iisController _webServer;
        //
        //===================================================================================================
        public securityController security {
            get {
                if (_security == null) {
                    _security = new securityController(this, serverConfig.appConfig.privateKey);
                }
                return _security;
            }
        }
        private securityController _security = null;
        //
        //===================================================================================================
        public fileController appRootFiles {
            get {
                if (_appRootFiles == null) {
                    if (serverConfig.appConfig != null) {
                        if (serverConfig.appConfig.enabled) {
                            if (serverConfig.isLocalFileSystem) {
                                //
                                // local server -- everything is ephemeral
                                _appRootFiles = new fileController(this, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(serverConfig.appConfig.appRootFilesPath));
                            } else {
                                //
                                // cluster mode - each filesystem is configured accordingly
                                _appRootFiles = new fileController(this, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.activeSync, fileController.normalizePath(serverConfig.appConfig.appRootFilesPath));
                            }
                        }
                    }
                }
                return _appRootFiles;
            }
        }
        private fileController _appRootFiles = null;
        //
        //===================================================================================================
        public fileController tempFiles {
            get {
                if (_tmpFiles == null) {
                    //
                    // local server -- everything is ephemeral
                    _tmpFiles = new fileController(this, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(serverConfig.appConfig.tempFilesPath));
                }
                return _tmpFiles;
            }
        }
        private fileController _tmpFiles = null;
        //
        //===================================================================================================
        public fileController privateFiles {
            get {
                if (_privateFiles == null) {
                    if (serverConfig.appConfig != null) {
                        if (serverConfig.appConfig.enabled) {
                            if (serverConfig.isLocalFileSystem) {
                                //
                                // local server -- everything is ephemeral
                                _privateFiles = new fileController(this, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(serverConfig.appConfig.privateFilesPath));
                            } else {
                                //
                                // cluster mode - each filesystem is configured accordingly
                                _privateFiles = new fileController(this, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.passiveSync, fileController.normalizePath(serverConfig.appConfig.privateFilesPath));
                            }
                        }
                    }
                }
                return _privateFiles;
            }
        }
        private fileController _privateFiles = null;
        //
        //===================================================================================================
        public fileController programDataFiles {
            get {
                if (_programDataFiles == null) {
                    //
                    // -- always local -- must be because this object is used to read serverConfig, before the object is valid
                    string programDataPath = fileController.normalizePath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)) + "Contensive\\";
                    _programDataFiles = new fileController(this, true, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(programDataPath));
                }
                return _programDataFiles;
            }
        }
        private fileController _programDataFiles = null;
        //
        //===================================================================================================
        public fileController programFiles {
            get {
                if (_programFiles == null) {
                    //
                    // -- always local
                    _programFiles = new fileController(this, true, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(serverConfig.programFilesPath));
                }
                return _programFiles;
            }
        }
        private fileController _programFiles = null;
        //
        //===================================================================================================
        public fileController cdnFiles {
            get {
                if (_cdnFiles == null) {
                    if (serverConfig.appConfig != null) {
                        if (serverConfig.appConfig.enabled) {
                            if (serverConfig.isLocalFileSystem) {
                                //
                                // local server -- everything is ephemeral
                                _cdnFiles = new fileController(this, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(serverConfig.appConfig.cdnFilesPath));
                            } else {
                                //
                                // cluster mode - each filesystem is configured accordingly
                                _cdnFiles = new fileController(this, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.passiveSync, fileController.normalizePath(serverConfig.appConfig.cdnFilesPath));
                            }
                        }
                    }
                }
                return _cdnFiles;
            }
        }
        private fileController _cdnFiles = null;
        //
        //===================================================================================================
        public addonModel.addonCacheClass addonCache {
            get {
                if (_addonCache == null) {
                    _addonCache = cache.getObject<addonModel.addonCacheClass>("addonCache");
                    if (_addonCache == null) {
                        _addonCache = new addonModel.addonCacheClass();
                        foreach (addonModel addon in addonModel.createList(this, "")) {
                            _addonCache.add(this, addon);
                        }
                        cache.setContent("addonCache", _addonCache);
                    }
                }
                return _addonCache;
            }
        }
        private addonModel.addonCacheClass _addonCache = null;
        //
        //===================================================================================================
        /// <summary>
        /// siteProperties object
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public domainLegacyModel domainLegacyCache {
            get {
                if (_domains == null) {
                    _domains = new domainLegacyModel(this);
                }
                return _domains;
            }
        }
        private domainLegacyModel _domains = null;
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
        public workflowController workflow {
            get {
                if (_workflow == null) {
                    _workflow = new workflowController(this);
                }
                return _workflow;
            }
        }
        private workflowController _workflow = null;
        //
        //===================================================================================================
        public Controllers.cacheController cache {
            get {
                if (_cache == null) {
                    _cache = new Controllers.cacheController(this);
                }
                return _cache;
            }
        }
        private Controllers.cacheController _cache = null;
        //
        //===================================================================================================
        public dbController db {
            get {
                if (_db == null) {
                    _db = new dbController(this);
                }
                return _db;
            }
        }
        private dbController _db;
        //
        //===================================================================================================
        public dbServerController dbServer {
            get {
                if (_dbEngine == null) {
                    _dbEngine = new dbServerController(this);
                }
                return _dbEngine;
            }
        }
        private dbServerController _dbEngine;
        //
        //====================================================================================================
        /// <summary>
        /// cpCoreClass constructor for cluster use.
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        public coreClass(CPClass cp) : base() {
            cp_forAddonExecutionOnly = cp;
            //
            // -- create default auth objects for non-user methods, or until auth is available
            doc.authContext = new authContextModel(this);
            //
            serverConfig = Models.Context.serverConfigModel.getObject(this);
            this.serverConfig.defaultDataSourceType = dataSourceModel.dataSourceTypeEnum.sqlServerNative;
            webServer.iisContext = null;
            constructorInitialize();
        }
        //
        //====================================================================================================
        /// <summary>
        /// cpCoreClass constructor for app, non-Internet use. cpCoreClass is the primary object internally, created by cp.
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        public coreClass(CPClass cp, Models.Context.serverConfigModel serverConfig) : base() {
            this.cp_forAddonExecutionOnly = cp;
            //
            // -- create default auth objects for non-user methods, or until auth is available
            doc.authContext = new authContextModel(this);
            //
            this.serverConfig = serverConfig;
            this.serverConfig.defaultDataSourceType = dataSourceModel.dataSourceTypeEnum.sqlServerNative;
            this.serverConfig.appConfig.appStatus = Models.Context.serverConfigModel.appStatusEnum.OK;
            webServer.iisContext = null;
            constructorInitialize();
        }
        //
        //====================================================================================================
        /// <summary>
        /// cpCoreClass constructor for app, non-Internet use. cpCoreClass is the primary object internally, created by cp.
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        public coreClass(CPClass cp, Models.Context.serverConfigModel serverConfig, System.Web.HttpContext httpContext) : base() {
            this.cp_forAddonExecutionOnly = cp;
            //
            // -- create default auth objects for non-user methods, or until auth is available
            doc.authContext = new authContextModel(this);
            //
            this.serverConfig = serverConfig;
            this.serverConfig.defaultDataSourceType = dataSourceModel.dataSourceTypeEnum.sqlServerNative;
            this.serverConfig.appConfig.appStatus = Models.Context.serverConfigModel.appStatusEnum.OK;
            webServer.initWebContext(httpContext);
            constructorInitialize();
        }
        //
        //====================================================================================================
        /// <summary>
        /// cpCoreClass constructor for app, non-Internet use. cpCoreClass is the primary object internally, created by cp.
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        public coreClass(CPClass cp, string applicationName) : base() {
            this.cp_forAddonExecutionOnly = cp;
            //
            // -- create default auth objects for non-user methods, or until auth is available
            doc.authContext = new authContextModel(this);
            //
            serverConfig = Models.Context.serverConfigModel.getObject(this, applicationName);
            serverConfig.defaultDataSourceType = dataSourceModel.dataSourceTypeEnum.sqlServerNative;
            if (serverConfig.appConfig != null) {
                webServer.iisContext = null;
                constructorInitialize();
            }
        }
        //====================================================================================================
        /// <summary>
        /// cpCoreClass constructor for a web request/response environment. cpCoreClass is the primary object internally, created by cp.
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks>
        /// All iis httpContext is loaded here and the context should not be used after this method.
        /// </remarks>
        public coreClass(CPClass cp, string applicationName, System.Web.HttpContext httpContext) : base() {
            this.cp_forAddonExecutionOnly = cp;
            //
            // -- create default auth objects for non-user methods, or until auth is available
            doc.authContext = new authContextModel(this);
            //
            serverConfig = Models.Context.serverConfigModel.getObject(this, applicationName);
            serverConfig.defaultDataSourceType = dataSourceModel.dataSourceTypeEnum.sqlServerNative;
            if (serverConfig.appConfig != null) {
                webServer.initWebContext(httpContext);
                constructorInitialize();
            }
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
            try {
                if (serverConfig.appConfig != null) {
                    //
                    // -- execute intercept methods first, like login, that run before the route that returns the page
                    // -- intercept routes should be addons alos
                    //
                    // -- determine the route: try routeOverride
                    string normalizedRoute = genericController.normalizeRoute(routeOverride);
                    if (string.IsNullOrEmpty(normalizedRoute)) {
                        //
                        // -- no override, try argument route (remoteMethodAddon=)
                        normalizedRoute = genericController.normalizeRoute(docProperties.getText(RequestNameRemoteMethodAddon));
                        if (string.IsNullOrEmpty(normalizedRoute)) {
                            //
                            // -- no override or argument, use the url as the route
                            normalizedRoute = genericController.normalizeRoute(webServer.requestPathPage.ToLower());
                        }
                    }
                    //
                    // -- legacy ajaxfn methods
                    string AjaxFunction = docProperties.getText(RequestNameAjaxFunction);
                    if (!string.IsNullOrEmpty(AjaxFunction)) {
                        //
                        // -- Need to be converted to Url parameter addons
                        result = "";
                        switch ((AjaxFunction)) {
                            case ajaxGetFieldEditorPreferenceForm:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.getFieldEditorPreference()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxGetDefaultAddonOptionString:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.getAjaxDefaultAddonOptionStringClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxSetVisitProperty:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.setAjaxVisitPropertyClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxGetVisitProperty:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.getAjaxVisitPropertyClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxData:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.processAjaxDataClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxPing:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.getOKClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxOpenIndexFilter:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.openAjaxIndexFilterClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxOpenIndexFilterGetContent:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.openAjaxIndexFilterGetContentClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxCloseIndexFilter:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.closeAjaxIndexFilterClass()).Execute(cp_forAddonExecutionOnly).ToString();
                            case AjaxOpenAdminNav:
                                //
                                // moved to Addons.AdminSite
                                doc.continueProcessing = false;
                                return (new Addons.AdminSite.openAjaxAdminNavClass()).Execute(cp_forAddonExecutionOnly).ToString();
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
                        switch (genericController.vbLCase(HardCodedPage)) {
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
                            case HardCodedPageLogout:
                                //
                                return (new Addons.Primitives.processLogoutMethodClass()).Execute(cp_forAddonExecutionOnly).ToString();
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
                            case HardCodedPagePayPalConfirm:
                                //
                                return (new Addons.Primitives.processPayPalConformMethodClass()).Execute(cp_forAddonExecutionOnly).ToString();
                        }
                    }
                    //
                    // -- try route Dictionary (addons, admin, link forwards, link alias), from full route to first segment one at a time
                    // -- so route /this/and/that would first test /this/and/that, then test /this/and, then test /this
                    string routeTest = normalizedRoute;
                    bool routeFound = false;
                    int routeCnt = 100;
                    do {
                        routeFound = routeDictionary.ContainsKey(routeTest);
                        if (routeFound) {
                            break;
                        }
                        if (routeTest.IndexOf("/") < 0) {
                            break;
                        }
                        routeTest = routeTest.Left( routeTest.LastIndexOf("/"));
                        routeCnt -= 1;
                    } while ((routeCnt > 0) && (!routeFound));
                    //
                    // -- execute route
                    if (routeFound) {
                        CPSiteBaseClass.routeClass route = routeDictionary[routeTest];
                        switch (route.routeType) {
                            case CPSiteBaseClass.routeTypeEnum.admin:
                                //
                                // -- admin site
                                //
                                return this.addon.execute(addonModel.create(this, addonGuidAdminSite), new CPUtilsBaseClass.addonExecuteContext() { addonType = CPUtilsBaseClass.addonContext.ContextAdmin });
                            case CPSiteBaseClass.routeTypeEnum.remoteMethod:
                                //
                                // -- remote method
                                addonModel addon = addonCache.getAddonById(route.remoteMethodAddonId);
                                if (addon != null) {
                                    CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                                        addonType = CPUtilsBaseClass.addonContext.ContextRemoteMethodJson,
                                        cssContainerClass = "",
                                        cssContainerId = "",
                                        hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                                            contentName = docProperties.getText("hostcontentname"),
                                            fieldName = "",
                                            recordId = docProperties.getInteger("HostRecordID")
                                        },
                                        personalizationAuthenticated = doc.authContext.isAuthenticated,
                                        personalizationPeopleId = doc.authContext.user.id
                                    };
                                    return this.addon.execute(addon, executeContext);
                                }
                                break;
                            case CPSiteBaseClass.routeTypeEnum.linkAlias:
                                //
                                // - link alias
                                linkAliasModel linkAlias = linkAliasModel.create(this, route.linkAliasId);
                                if (linkAlias != null) {
                                    docProperties.setProperty("bid", linkAlias.PageID);
                                    if (!string.IsNullOrEmpty(linkAlias.QueryStringSuffix)) {
                                        string[] nvp = linkAlias.QueryStringSuffix.Split('&');
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
                            case CPSiteBaseClass.routeTypeEnum.linkForward:
                                //
                                // -- link forward
                                linkForwardModel linkForward = linkForwardModel.create(this, route.linkForwardId);
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
                    int defaultAddonId = siteProperties.getInteger(spDefaultRouteAddonId);
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
                            personalizationAuthenticated = doc.authContext.visit.VisitAuthenticated,
                            personalizationPeopleId = doc.authContext.user.id
                        };
                        return this.addon.execute(Models.Entity.addonModel.create(this, defaultAddonId), executeContext);
                    }
                    //
                    // -- no route
                    result = "<p>This site is not configured for website traffic. Please set the default route.</p>";
                }
            } catch (Exception ex) {
                handleException(ex);
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
                handleException(new ApplicationException("executeRoute_ProcessAjaxData deprecated"));
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
                //    int CS = db.csOpen("Remote Queries", "((VisitId=" + doc.authContext.visit.id + ")and(remotekey=" + db.encodeSQLText(RemoteKey) + "))");
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
                //                                    throw (new ApplicationException(errorMessage));
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
                //        Copy = genericController.encodeHTML(Copy);
                //        result = "<data>" + Copy + "</data>";
                //    }
                //}
            } catch (Exception ex) {
                throw (ex);
            }
            return result;
        }
        //
        //==========================================================================================
        /// <summary>
        /// Generic handle exception. Determines method name and class of caller from stack. 
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="ex"></param>
        /// <param name="cause"></param>
        /// <param name="stackPtr">How far down in the stack to look for the method error. Pass 1 if the method calling has the error, 2 if there is an intermediate routine.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public void handleException(Exception ex, string cause, int stackPtr) {
            if (!_handlingExceptionRecursionBlock) {
                _handlingExceptionRecursionBlock = true;
                StackFrame frame = new StackFrame(stackPtr);
                System.Reflection.MethodBase method = frame.GetMethod();
                System.Type type = method.DeclaringType;
                string methodName = method.Name;
                string errMsg = type.Name + "." + methodName + ", cause=[" + cause + "], ex=[" + ex.ToString() + "]";
                //
                // append to application event log
                //
                string sSource = "Contensive";
                string sLog = "Application";
                int eventId = 1001;
                try {
                    //
                    // if command line has been run on this server, this will work. Otherwise skip
                    //
                    EventLog.WriteEntry(sSource, errMsg, EventLogEntryType.Error, eventId);
                } catch (Exception exEvent) {
                    // ignore error. Can be caused if source has not been created. It is created automatically in command line installation util.
                }
                //
                // append to daily trace log
                //
                logController.appendLog(this, errMsg);
                //
                // add to doc exception list to display at top of webpage
                //
                if (doc.errList == null) {
                    doc.errList = new List<string>();
                }
                if (doc.errList.Count == 10) {
                    doc.errList.Add("Exception limit exceeded");
                } else if (doc.errList.Count < 10) {
                    doc.errList.Add(errMsg);
                }
                //
                // write consol for debugging
                //
                Console.WriteLine(errMsg);
                //
                _handlingExceptionRecursionBlock = false;
            }
        }
        private bool _handlingExceptionRecursionBlock = false;
        //
        //====================================================================================================
        //
        public void handleException(Exception ex, string cause) {
            handleException(ex, cause, 2);
        }
        //
        //====================================================================================================
        //
        public void handleException(Exception ex) {
            handleException(ex, "n/a", 2);
        }
        //
        //====================================================================================================
        /// <summary>
        /// cpCoreClass constructor common tasks.
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        private void constructorInitialize() {
            try {
                //
                doc.docGuid = genericController.createGuid();
                doc.allowDebugLog = true;
                doc.profileStartTime = DateTime.Now;
                doc.visitPropertyAllowDebugging = true;
                //
                // -- attempt auth load
                if (serverConfig.appConfig == null) {
                    //
                    // -- server mode, there is no application
                    doc.authContext = Models.Context.authContextModel.create(this, false);
                } else if ((serverConfig.appConfig.appMode != Models.Context.serverConfigModel.appModeEnum.normal) | (serverConfig.appConfig.appStatus != Models.Context.serverConfigModel.appStatusEnum.OK)) {
                    //
                    // -- application is not ready, might be error, or in maintainence mode
                    doc.authContext = Models.Context.authContextModel.create(this, false);
                } else {
                    doc.authContext = Models.Context.authContextModel.create(this, siteProperties.allowVisitTracking);
                    //
                    // -- debug printed defaults on, so if not on, set it off and clear what was collected
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
        /// version for cpCore assembly
        /// </summary>
        /// <remarks></remarks>
        public string codeVersion() {
            Type myType = typeof(coreClass);
            Assembly myAssembly = Assembly.GetAssembly(myType);
            AssemblyName myAssemblyname = myAssembly.GetName();
            Version myVersion = myAssemblyname.Version;
            return myVersion.Major.ToString("0") + "." + myVersion.Minor.ToString("00") + "." + myVersion.Build.ToString("00000000");
        }
        //
        //===================================================================================================
        public Dictionary<string, CPSiteBaseClass.routeClass> routeDictionary {
            get {
                // -- when an addon changes, the route map has to reload on page exit so it is ready on the next hit. lazy cache here clears on page load, so this does work
                return Models.Complex.routeDictionaryModel.create(this);
                //If (_routeDictionary Is Nothing) Then
                //    _routeDictionary = Models.Complex.routeDictionaryModel.create(Me)
                //End If
                //Return _routeDictionary
            }
        }
        //Private _routeDictionary As Dictionary(Of String, CPSiteBaseClass.routeClass) = Nothing
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
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        ~coreClass() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(false);
            //INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
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
                    // ----- Block all output from underlying routines
                    //
                    doc.blockExceptionReporting = true;
                    doc.continueProcessing = false;
                    //
                    // -- save addoncache
                    if (_assemblySkipList != null) {
                        if (_assemblySkipList.Count > _assemblySkipList_CountWhenLoaded) {
                            cache.setContent(cacheNameAssemblySkipList, _assemblySkipList);
                        }
                    }
                    //
                    // content server object is valid
                    //
                    if (serverConfig != null) {
                        if (serverConfig.appConfig != null) {
                            if ((serverConfig.appConfig.appMode == serverConfigModel.appModeEnum.normal) && (serverConfig.appConfig.appStatus == serverConfigModel.appStatusEnum.OK)) {
                                if (siteProperties.allowVisitTracking) {
                                    //
                                    // If visit tracking, save the viewing record
                                    //
                                    string ViewingName = ((string)(doc.authContext.visit.id + "." + doc.authContext.visit.PageVisits)).Left( 10);
                                    int PageID = 0;
                                    if (_doc != null) {
                                        if (doc.page != null) {
                                            PageID = doc.page.id;
                                        }
                                    }
                                    //
                                    // -- convert requestFormDict to a name=value string for Db storage
                                    string requestFormSerialized = genericController.convertNameValueDictToREquestString(webServer.requestFormDict);
                                    string pagetitle = "";
                                    if (!doc.htmlMetaContent_TitleList.Count.Equals(0)) {
                                        pagetitle = doc.htmlMetaContent_TitleList[0].content;
                                    }
                                    string SQL = "insert into ccviewings ("
                                        + "Name,VisitId,MemberID,Host,Path,Page,QueryString,Form,Referer,DateAdded,StateOK,ContentControlID,pagetime,Active,CreateKey,RecordID,ExcludeFromAnalytics,pagetitle"
                                        + ")values("
                                        + " " + db.encodeSQLText(ViewingName) + "," + db.encodeSQLNumber(doc.authContext.visit.id) + "," + db.encodeSQLNumber(doc.authContext.user.id) + "," + db.encodeSQLText(webServer.requestDomain) + "," + db.encodeSQLText(webServer.requestPath) + "," + db.encodeSQLText(webServer.requestPage) + "," + db.encodeSQLText(webServer.requestQueryString.Left( 255)) + "," + db.encodeSQLText(requestFormSerialized.Left( 255)) + "," + db.encodeSQLText(webServer.requestReferrer.Left( 255)) + "," + db.encodeSQLDate(doc.profileStartTime) + "," + db.encodeSQLBoolean(doc.authContext.visit_stateOK) + "," + db.encodeSQLNumber(Models.Complex.cdefModel.getContentId(this, "Viewings")) + "," + db.encodeSQLNumber(doc.appStopWatch.ElapsedMilliseconds) + ",1"
                                        + "," + db.encodeSQLNumber(0) + "," + db.encodeSQLNumber(PageID);
                                    SQL += "," + db.encodeSQLBoolean(webServer.pageExcludeFromAnalytics);
                                    SQL += "," + db.encodeSQLText(pagetitle);
                                    SQL += ");";
                                    db.executeQuery(SQL);
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
                    if (_workflow != null) {
                        _workflow.Dispose();
                        _workflow = null;
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
                    if (_security != null) {
                        // no dispose
                        //Call _security.Dispose()
                        _security = null;
                    }
                    //
                    if (_webServer != null) {
                        // no dispose
                        //Call _webServer.Dispose()
                        _webServer = null;
                    }
                    //
                    if (_menuFlyout != null) {
                        // no dispose
                        //Call _menuFlyout.Dispose()
                        _menuFlyout = null;
                    }
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
                }
                //
                // cleanup non-managed objects
                //
            }
        }
        #endregion
    }
}
