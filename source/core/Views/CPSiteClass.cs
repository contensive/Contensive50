
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
namespace Contensive.Core {
    public class CPSiteClass : BaseClasses.CPSiteBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "7C159DA2-6677-426B-8631-3F235F24BCF0";
        public const string InterfaceId = "50DD3209-AE54-46EF-8344-C6CD8960DD65";
        public const string EventsId = "5E88DB23-E8D7-4CE8-9793-9C7A20F4CF3A";
        #endregion
        //
        private Contensive.Core.Controllers.coreController core;
        private CPClass CP;
        protected bool disposed = false;
        //
        //====================================================================================================
        //
        public CPSiteClass(Contensive.Core.Controllers.coreController coreObj, CPClass CPParent) : base() {
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
            return core.siteProperties.getBoolean(propertyName, genericController.encodeBoolean(DefaultValue));
        }
        public override bool GetBoolean(string propertyName, bool DefaultValue ) {
            return core.siteProperties.getBoolean(propertyName, DefaultValue);
        }

        //
        //====================================================================================================
        //
        public override DateTime GetDate(string propertyName, string DefaultValue = "") {
            return core.siteProperties.getDate(propertyName, genericController.encodeDate(DefaultValue));
        }
        public override DateTime GetDate(string propertyName, DateTime DefaultValue) {
            return core.siteProperties.getDate(propertyName, DefaultValue);
        }
        //
        //====================================================================================================
        //
        public override int GetInteger(string propertyName, string DefaultValue = "") {
            return core.siteProperties.getInteger(propertyName, genericController.encodeInteger(DefaultValue));
        }
        public override int GetInteger(string propertyName, int DefaultValue ) {
            return core.siteProperties.getInteger(propertyName, DefaultValue);
        }
        //
        //====================================================================================================
        //
        public override double GetNumber(string propertyName, string DefaultValue = "") {
            return core.siteProperties.getNumber(propertyName, genericController.encodeNumber(DefaultValue));
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
                return genericController.encodeBoolean(GetProperty("TrapErrors", "1"));
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
                return requestAppRootPath;
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
            return genericController.EncodeAppRootPath(genericController.encodeText(Link), core.webServer.requestVirtualFilePath, requestAppRootPath, core.webServer.requestDomain);
        }
        //
        //====================================================================================================
        //
        public override bool IsTesting() { return false; }
        //
        //====================================================================================================
        //
        public override void LogActivity(string Message, int UserID, int OrganizationID) {
            logController.addSiteActivity(core, Message, 0, UserID, OrganizationID);
        }
        //
        //====================================================================================================
        //
        public override void LogWarning(string name, string description, string typeOfWarningKey, string instanceKey) {
            logController.addSiteWarning(core, name, description, "", 0, description, typeOfWarningKey, instanceKey);
        }
        //
        //====================================================================================================
        //
        public override void LogAlarm(string cause) {
            logController.logFatal(core, "logAlarm: " + cause);
        }
        //
        //====================================================================================================
        //
        public override void ErrorReport(string Cause) { ErrorReport(new ApplicationException("Unexpected exception"), Cause);}
        //
        //====================================================================================================
        //
        public override void ErrorReport(System.Exception Ex, string Message = "") {
            if (string.IsNullOrEmpty(Message)) {
                core.handleException(Ex, "n/a", 2);
            } else {
                core.handleException(Ex, Message, 2);
            }
        }
        //
        //====================================================================================================
        //
        public override void RequestTask(string Command, string SQL, string ExportName, string Filename) {
            try {
                var ExportCSVAddon = Models.DbModels.addonModel.create(core, addonGuidExportCSV);
                if (ExportCSVAddon == null) {
                    core.handleException(new ApplicationException("ExportCSV addon not found. Task could not be added to task queue."));
                } else {
                    var docProperties = new Dictionary<string, string>();
                    docProperties.Add("sql", SQL);
                    docProperties.Add("ExportName", ExportName);
                    docProperties.Add("filename", Filename);
                    var cmdDetail = new cmdDetailClass() {
                        addonId = ExportCSVAddon.id,
                        addonName = ExportCSVAddon.name,
                        docProperties = docProperties
                    };
                    taskSchedulerController.addTaskToQueue(core, taskCommandBuildCsv, cmdDetail, false);
                }
            } catch (Exception) {
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void TestPoint(string Message) {
            debugController.testPoint(core, Message);
        }
        //
        //====================================================================================================
        //
        public override int LandingPageId(string DomainName = "") {
            if ( string.IsNullOrWhiteSpace(DomainName)) {
                return GetInteger("LandingPageID", 0);
            } else {
                var domain = domainModel.createByName(CP.core, DomainName);
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
            linkAliasController.addLinkAlias(core, linkAlias, pageId, queryStringSuffix);
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
                returnOk = collectionController.InstallCollectionsFromPrivateFile(core, privatePathFilename, ref returnUserError, ref ignoreReturnedCollectionGuid, false, ref tmpList);
            } catch (Exception ex) {
                core.handleException(ex);
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