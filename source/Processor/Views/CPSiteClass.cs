
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
namespace Contensive.Processor {
    public class CPSiteClass : BaseClasses.CPSiteBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "7C159DA2-6677-426B-8631-3F235F24BCF0";
        public const string InterfaceId = "50DD3209-AE54-46EF-8344-C6CD8960DD65";
        public const string EventsId = "5E88DB23-E8D7-4CE8-9793-9C7A20F4CF3A";
        #endregion
        //
        private Contensive.Processor.Controllers.CoreController core;
        private CPClass CP;
        protected bool disposed = false;
        //
        //====================================================================================================
        //
        public CPSiteClass(Contensive.Processor.Controllers.CoreController coreObj, CPClass CPParent) : base() {
            core = coreObj;
            CP = CPParent;
        }
        //
        //====================================================================================================
        //
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    CP = null;
                    core = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        //====================================================================================================
        //
        public override string Name {
            get {
                return core.appConfig.name;
            }
        }
        //
        //====================================================================================================
        //
        public override void SetProperty(string FieldName, string FieldValue) {
            core.siteProperties.setProperty(FieldName, FieldValue);
        }
        //
        //====================================================================================================
        //
        public override string GetProperty(string propertyName, string DefaultValue = "") {
            return core.siteProperties.getText(propertyName, DefaultValue);
        }
        //
        //====================================================================================================
        //
        public override bool GetBoolean(string propertyName, string DefaultValue = "") {
            return core.siteProperties.getBoolean(propertyName, GenericController.encodeBoolean(DefaultValue));
        }
        public override bool GetBoolean(string propertyName, bool DefaultValue ) {
            return core.siteProperties.getBoolean(propertyName, DefaultValue);
        }

        //
        //====================================================================================================
        //
        public override DateTime GetDate(string propertyName, string DefaultValue = "") {
            return core.siteProperties.getDate(propertyName, GenericController.encodeDate(DefaultValue));
        }
        public override DateTime GetDate(string propertyName, DateTime DefaultValue) {
            return core.siteProperties.getDate(propertyName, DefaultValue);
        }
        //
        //====================================================================================================
        //
        public override int GetInteger(string propertyName, string DefaultValue = "") {
            return core.siteProperties.getInteger(propertyName, GenericController.encodeInteger(DefaultValue));
        }
        public override int GetInteger(string propertyName, int DefaultValue ) {
            return core.siteProperties.getInteger(propertyName, DefaultValue);
        }
        //
        //====================================================================================================
        //
        public override double GetNumber(string propertyName, string DefaultValue = "") {
            return core.siteProperties.getNumber(propertyName, GenericController.encodeNumber(DefaultValue));
        }
        public override double GetNumber(string propertyName, double DefaultValue) {
            return core.siteProperties.getNumber(propertyName, DefaultValue);
        }
        //
        //====================================================================================================
        //
        public override string GetText(string FieldName, string DefaultValue = "") {
            return GetProperty(FieldName, DefaultValue);
        }
        //
        //====================================================================================================
        //
        public override bool MultiDomainMode { get {return false; }}
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", true)]
        public override string PhysicalFilePath {
            get {
                return core.cdnFiles.localAbsRootPath;
            }
        }
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", true)]
        public override string PhysicalInstallPath {
            get {
                return core.privateFiles.localAbsRootPath;
            }
        }
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", true)]
        public override string PhysicalWWWPath {
            get {
                return core.appRootFiles.localAbsRootPath;
            }
        }
        //
        //====================================================================================================
        //
        public override bool TrapErrors {
            get {
                return GenericController.encodeBoolean(GetProperty("TrapErrors", "1"));
            }
        }
        //
        //====================================================================================================
        //
        public override string AppPath {
            get {
                return AppRootPath;
            }
        }
        //
        //====================================================================================================
        //
        public override string AppRootPath {
            get {
                return appRootPath;
            }
        }
        //
        //====================================================================================================
        //
        public override string DomainPrimary {
            get {
                string tempDomainPrimary = null;
                tempDomainPrimary = "";
                if (core.appConfig.domainList.Count > 0) {
                    tempDomainPrimary = core.appConfig.domainList[0];
                }
                return tempDomainPrimary;
            }
        }
        //
        //====================================================================================================
        //
        public override string Domain {
            get {
                return core.webServer.requestDomain;
            }
        }
        //
        //====================================================================================================
        //
        public override string DomainList {
            get {
                return string.Join(",", core.appConfig.domainList);
            }
        }
        //
        //====================================================================================================
        //
        public override string FilePath {
            get {
                return core.appConfig.cdnFileUrl;
            }
        }
        //
        //====================================================================================================
        //
        public override string PageDefault {
            get {
                return core.siteProperties.serverPageDefault;
            }
        }
        //
        //====================================================================================================
        //
        public override string VirtualPath {
            get {
                return "/" + core.appConfig.name;
            }
        }
        //
        //====================================================================================================
        //
        public override string EncodeAppRootPath(string Link) {
            return GenericController.encodeVirtualPath(GenericController.encodeText(Link), core.appConfig.cdnFileUrl, appRootPath, core.webServer.requestDomain);
        }
        //
        //====================================================================================================
        //
        public override bool IsTesting() { return false; }
        //
        //====================================================================================================
        //
        public override void LogActivity(string Message, int UserID, int OrganizationID) {
            LogController.addSiteActivity(core, Message, 0, UserID, OrganizationID);
        }
        //
        //====================================================================================================
        //
        public override void LogWarning(string name, string description, string typeOfWarningKey, string instanceKey) {
            LogController.addSiteWarning(core, name, description, "", 0, description, typeOfWarningKey, instanceKey);
        }
        //
        //====================================================================================================
        //
        public override void LogAlarm(string cause) {
            LogController.logFatal(core, "logAlarm: " + cause);
        }
        //
        //====================================================================================================
        //
        public override void ErrorReport(string Message) {
            LogController.handleException(core, new ApplicationException("Unexpected exception"), LogController.logLevel.Error, Message, 2);
        }
        //
        //====================================================================================================
        //
        public override void ErrorReport(System.Exception Ex, string Message = "") {
            LogController.handleException(core, Ex, LogController.logLevel.Error, Message, 2);
        }
        //
        //====================================================================================================
        //
        public override void RequestTask(string Command, string SQL, string ExportName, string Filename) {
            try {
                var ExportCSVAddon = Models.Db.AddonModel.create(core, addonGuidExportCSV);
                if (ExportCSVAddon == null) {
                    LogController.handleError( core,new ApplicationException("ExportCSV addon not found. Task could not be added to task queue."));
                } else {
                    var cmdDetail = new TaskModel.cmdDetailClass() {
                        addonId = ExportCSVAddon.id,
                        addonName = ExportCSVAddon.name,
                        args = new Dictionary<string, string> {
                            { "sql", SQL },
                            { "ExportName", ExportName },
                            { "filename", Filename }
                        }
                    };
                    TaskSchedulerControllerx.addTaskToQueue(core, cmdDetail, false, false );
                }
            } catch (Exception) {
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void TestPoint(string Message) {
            DebugController.testPoint(core, Message);
        }
        //
        //====================================================================================================
        //
        public override int LandingPageId(string DomainName = "") {
            if ( string.IsNullOrWhiteSpace(DomainName)) {
                return GetInteger("LandingPageID", 0);
            } else {
                var domain = DomainModel.createByName(CP.core, DomainName);
                if ( domain == null) {
                    return GetInteger("LandingPageID", 0);
                } else {
                    return domain.rootPageId;
                }
            }
        }
        //
        //====================================================================================================
        //
        public override void addLinkAlias(string linkAlias, int pageId, string queryStringSuffix = "") {
            LinkAliasController.addLinkAlias(core, linkAlias, pageId, queryStringSuffix);
        }
        //
        //====================================================================================================
        //
        public override string ThrowEvent(string eventNameIdOrGuid) {
            return core.addon.throwEvent(eventNameIdOrGuid);
        }
        //==========================================================================================
        /// <summary>
        /// Install an uploaded collection file from a private folder. Return true if successful, else the issue is in the returnUserError
        /// </summary>
        /// <param name="privatePathFilename"></param>
        /// <param name="returnUserError"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public override bool installCollectionFile(string privatePathFilename, ref string returnUserError) {
            bool returnOk = false;
            try {
                string ignoreReturnedCollectionGuid = "";
                var tmpList = new List<string> { };
                string logPrefix = "CPSiteClass.installCollectionFile";
                var installedCollections = new List<string>();
                returnOk = CollectionController.installCollectionsFromPrivateFile(core, privatePathFilename, ref returnUserError, ref ignoreReturnedCollectionGuid, false, true, ref tmpList, logPrefix, ref installedCollections);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                if (!core.siteProperties.trapErrors) {
                    throw;
                }
            }
            return returnOk;
        }
        //
        //====================================================================================================
        //
        public override bool installCollectionFromLibrary(string collectionGuid, ref string returnUserError) {
            return false;
        }
        //
        //====================================================================================================
        //
        public override Dictionary<string, routeClass> getRouteDictionary() {
            return core.routeDictionary;
        }

        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~CPSiteClass() {
            Dispose(false);
        }
        #endregion
    }
}