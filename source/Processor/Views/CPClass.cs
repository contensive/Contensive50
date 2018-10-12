
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
//using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
using Contensive.BaseClasses;
using Contensive.Processor.Models.Domain;
//
namespace Contensive.Processor {
    public class CPClass : CPBaseClass, IDisposable {
        #region COM GUIDs
        public const string ClassId = "2EF01C6F-5288-411D-A5DE-76C8923CE1D3";
        public const string InterfaceId = "58E04B36-2C75-4D11-9A8D-22A52E8417EB";
        public const string EventsId = "4FADD1C2-6A89-4A8E-ADD0-9850D3EB6DBC";
        #endregion
        //
        public Contensive.Processor.Controllers.CoreController core { get; set; }
        private int MyAddonID { get; set; }
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
        //
        public AppConfigModel.AppStatusEnum status {
            get {
                return core.appConfig.appStatus;
            }
        }
        /// <summary>
        /// When a change is make data used for the route table, this flag is set in core. Use this flag to reload the webserver's route tables
        /// </summary>
        public bool routeDictionaryChanges {
            get {
                return core.doc.routeDictionaryChanges;
            }
        }
        //
        //=========================================================================================================
        //
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
        /// Executes a specific route. The route can be a remote method, link alias, admin route, etc. If the route is not provided, the default route set in the admin settings is used.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public string executeRoute(string route = "") {
            string result = "";
            try {
                result = core.executeRoute(route);
            } catch (Exception ex) {
                Site.ErrorReport(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// executes an addon with the name or guid provided, in the context specified.
        /// </summary>
        /// <param name="addonNameOrGuid"></param>
        /// <param name="addonContext"></param>
        /// <returns></returns>
        public string executeAddon(string addonNameOrGuid, Contensive.BaseClasses.CPUtilsBaseClass.addonContext addonContext = Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple) {
            string result = "";
            try {
                if (GenericController.isGuid(addonNameOrGuid)) {
                    //
                    // -- call by guid
                    AddonModel addon = Models.Db.AddonModel.create(core, addonNameOrGuid);
                    if ( addon == null ) {
                        throw new ApplicationException("Addon [" + addonNameOrGuid + "] could not be found.");
                    } else {
                        result = core.addon.execute(addon, new CPUtilsBaseClass.addonExecuteContext {
                            addonType = addonContext,
                            errorContextMessage = "external call to execute addon [" + addonNameOrGuid + "]"
                        });
                    }
                } else {
                    AddonModel addon = Models.Db.AddonModel.createByUniqueName(core, addonNameOrGuid);
                    if ( addon != null ) {
                        //
                        // -- call by name
                        result = core.addon.execute(addon, new CPUtilsBaseClass.addonExecuteContext {
                            addonType = addonContext,
                            errorContextMessage = "external call to execute addon [" + addonNameOrGuid + "]"
                        });
                    } else if (addonNameOrGuid.IsNumeric() ) {
                        //
                        // -- compatibility - call by id
                        result = executeAddon(GenericController.encodeInteger(addonNameOrGuid), addonContext);
                    } else {
                        throw new ApplicationException("Addon [" + addonNameOrGuid + "] could not be found.");
                    }
                }
                //result = core.addon.execute_legacy4(addonNameOrGuid, core.docProperties.getLegacyOptionStringFromVar(), addonContext, Nothing)
            } catch (Exception ex) {
                Site.ErrorReport(ex);
            }
            return result;
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
            string result = "";
            try {
                AddonModel addon = AddonModel.create(core, addonId);
                if ( addon == null) {
                    throw new ApplicationException("Addon [#" + addonId.ToString() + "] could not be found.");
                } else {
                    result = core.addon.execute(addon, new CPUtilsBaseClass.addonExecuteContext {
                        addonType = addonContext,
                        errorContextMessage = "external call to execute addon [" + addonId + "]"
                    });
                }
            } catch (Exception ex) {
                Site.ErrorReport(ex);
            }
            return result;
        }
        //
        //=========================================================================================================
        //
        public void AddVar(string OptionName, string OptionValue) {
            try {
                if (!string.IsNullOrEmpty(OptionName)) {
                    this.Doc.SetProperty(OptionName, OptionValue);
                }
            } catch (Exception ex) {
                Site.ErrorReport(ex);
            }
        }
        //
        //=========================================================================================================
        //
        public override CPBlockBaseClass BlockNew() {
            CPClass tempVar = this;
            return new CPBlockClass( tempVar);
        }
        //
        //=========================================================================================================
        //
        public override CPCSBaseClass CSNew() {
            return new CPCSClass(this);
        }
        //
        //=========================================================================================================
        //
        public override string Version {
            get {
                return core.codeVersion();
            }
        }
        //
        //=========================================================================================================
        //
        public override CPUserErrorBaseClass UserError {
            get {
                if (_userErrorObj == null) {
                    _userErrorObj = new CPUserErrorClass(core);
                }
                return _userErrorObj;
            }
        }
        private CPUserErrorClass _userErrorObj;
        //
        //=========================================================================================================
        //
        public override CPUserBaseClass User {
            get {
                if (_userObj == null) {
                    _userObj = new CPUserClass(core, this);
                }
                return _userObj;
            }
        }
        private CPUserClass _userObj;
        //
        //=========================================================================================================
        //
        public override CPAddonBaseClass Addon {
            get {
                if (core.doc.addonModelStack.Count == 0) {
                    //
                    // -- if no addon running, return null
                    return null;
                } else {
                    //
                    // -- return class
                    if (_addonObj == null) {
                        _addonObj = new CPAddonClass(this);
                    }
                    return _addonObj;
                }
            }
        }
        private CPAddonClass _addonObj;
        //
        //=========================================================================================================
        //
        public override CPFileSystemBaseClass cdnFiles {
            get {
                if (_cdnFiles == null) {
                    _cdnFiles = new CPFileSystemClass(core, core.cdnFiles);
                }
                return _cdnFiles;
            }
        }
        private CPFileSystemClass _cdnFiles;
        //
        //=========================================================================================================
        //
        public override CPCacheBaseClass Cache {
            get {
                if (_cacheObj == null) {
                    CPClass tempVar = this;
                    _cacheObj = new CPCacheClass(tempVar);
                }
                return _cacheObj;
            }
        }
        private CPCacheClass _cacheObj;
        //
        //=========================================================================================================
        //
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
        //
        public CPContextClass Context {
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
        //
        public override CPDbBaseClass Db {
            get {
                if (_dbObj == null) {
                    _dbObj = new CPDbClass(this);
                }
                return _dbObj;
            }
        }
        private CPDbClass _dbObj;
        //
        //=========================================================================================================
        //
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
        //
        public override CPEmailBaseClass Email {
            get {
                if (_emailObj == null) {
                    _emailObj = new CPEmailClass(core);
                }
                return _emailObj;
            }
        }
        private CPEmailClass _emailObj;
        //
        //====================================================================================================
        //
        [Obsolete()] public override CPFileBaseClass File {
            get {
                if (_fileObj == null) {
                    _fileObj = new CPFileClass(core);
                }
                return _fileObj;
            }
        }
        private CPFileClass _fileObj;
        //
        //====================================================================================================
        //
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
        //
        public override CPHtmlBaseClass Html {
            get {
                if (_htmlObj == null) {
                    _htmlObj = new CPHtmlClass(this);
                }
                return _htmlObj;
            }
        }
        private CPHtmlClass _htmlObj;
        //
        //====================================================================================================
        //
        public override CPAddonBaseClass MyAddon {
            get {
                return Addon;
            }
        }
        //
        //====================================================================================================
        //
        public override CPFileSystemBaseClass privateFiles {
            get {
                if (_privateFiles == null) {
                    _privateFiles = new CPFileSystemClass(core, core.privateFiles);
                }
                return _privateFiles;
            }
        }
        private CPFileSystemClass _privateFiles;
        //
        //====================================================================================================
        //
        public override CPRequestBaseClass Request {
            get {
                if (_requestObj == null) {
                    _requestObj = new CPRequestClass(core);
                }
                return _requestObj;
            }
        }
        private CPRequestClass _requestObj;
        //
        //====================================================================================================
        //
        public override CPResponseBaseClass Response {
            get {
                if (_responseObj == null) {
                    _responseObj = new CPResponseClass(core);
                }
                return _responseObj;
            }
        }
        private CPResponseClass _responseObj;
        //
        //====================================================================================================
        //
        public override CPSiteBaseClass Site {
            get {
                if (_siteObj == null) {
                    _siteObj = new CPSiteClass(core, this);
                }
                return _siteObj;
            }
        }
        private CPSiteClass _siteObj;
        //
        //====================================================================================================
        //
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
        //
        public override CPVisitBaseClass Visit {
            get {
                if (_visitObj == null) {
                    _visitObj = new CPVisitClass(core, this);
                }
                return _visitObj;
            }
        }
        private CPVisitClass _visitObj;
        //
        //====================================================================================================
        //
        public override CPVisitorBaseClass Visitor {
            get {
                if (_visitorObj == null) {
                    _visitorObj = new CPVisitorClass(core, this);
                }
                return _visitorObj;
            }
        }
        private CPVisitorClass _visitorObj;
        //
        //====================================================================================================
        //
        public override CPFileSystemBaseClass wwwFiles {
            get {
                if (_appRootFiles == null) {
                    _appRootFiles = new CPFileSystemClass(core, core.appRootFiles);
                }
                return _appRootFiles;
            }
        }
        private CPFileSystemClass _appRootFiles;
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
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
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
                    if (_cdnFiles != null) {
                        _cdnFiles.Dispose();
                    }
                    if (_appRootFiles != null) {
                        _appRootFiles.Dispose();
                    }
                    if (_privateFiles != null) {
                        _privateFiles.Dispose();
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