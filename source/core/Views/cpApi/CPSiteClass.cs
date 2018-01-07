
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
using Contensive.Core.Models.Entity;
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
        private Contensive.Core.coreClass cpCore;
        private CPClass CP;
        protected bool disposed = false;
        //
        //====================================================================================================
        //
        public CPSiteClass(Contensive.Core.coreClass cpCoreObj, CPClass CPParent) : base() {
            cpCore = cpCoreObj;
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
                    cpCore = null;
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
                return cpCore.serverConfig.appConfig.name;
            }
        }
        //
        //====================================================================================================
        //
        public override void SetProperty(string FieldName, string FieldValue) {
            cpCore.siteProperties.setProperty(FieldName, FieldValue);
        }
        //
        //====================================================================================================
        //
        public override string GetProperty(string propertyName, string DefaultValue = "") {
            return cpCore.siteProperties.getText(propertyName, DefaultValue);
        }
        //
        //====================================================================================================
        //
        public override bool GetBoolean(string propertyName, string DefaultValue = "") {
            return cpCore.siteProperties.getBoolean(propertyName, genericController.encodeBoolean(DefaultValue));
        }
        public override bool GetBoolean(string propertyName, bool DefaultValue ) {
            return cpCore.siteProperties.getBoolean(propertyName, DefaultValue);
        }

        //
        //====================================================================================================
        //
        public override DateTime GetDate(string propertyName, string DefaultValue = "") {
            return cpCore.siteProperties.getDate(propertyName, genericController.encodeDate(DefaultValue));
        }
        public override DateTime GetDate(string propertyName, DateTime DefaultValue) {
            return cpCore.siteProperties.getDate(propertyName, DefaultValue);
        }
        //
        //====================================================================================================
        //
        public override int GetInteger(string propertyName, string DefaultValue = "") {
            return cpCore.siteProperties.getInteger(propertyName, genericController.encodeInteger(DefaultValue));
        }
        public override int GetInteger(string propertyName, int DefaultValue ) {
            return cpCore.siteProperties.getInteger(propertyName, DefaultValue);
        }
        //
        //====================================================================================================
        //
        public override double GetNumber(string propertyName, string DefaultValue = "") {
            return cpCore.siteProperties.getNumber(propertyName, genericController.encodeNumber(DefaultValue));
        }
        public override double GetNumber(string propertyName, double DefaultValue) {
            return cpCore.siteProperties.getNumber(propertyName, DefaultValue);
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
                return cpCore.cdnFiles.rootLocalPath;
            }
        }
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", true)]
        public override string PhysicalInstallPath {
            get {
                return cpCore.privateFiles.rootLocalPath;
            }
        }
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", true)]
        public override string PhysicalWWWPath {
            get {
                return cpCore.appRootFiles.rootLocalPath;
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
                if (cpCore.serverConfig.appConfig.domainList.Count > 0) {
                    tempDomainPrimary = cpCore.serverConfig.appConfig.domainList[0];
                }
                return tempDomainPrimary;
            }
        }
        //
        //====================================================================================================
        //
        public override string Domain {
            get {
                return cpCore.webServer.requestDomain;
            }
        }
        //
        //====================================================================================================
        //
        public override string DomainList {
            get {
                return string.Join(",", cpCore.serverConfig.appConfig.domainList);
            }
        }
        //
        //====================================================================================================
        //
        public override string FilePath {
            get {
                return cpCore.serverConfig.appConfig.cdnFilesNetprefix;
            }
        }
        //
        //====================================================================================================
        //
        public override string PageDefault {
            get {
                return cpCore.siteProperties.serverPageDefault;
            }
        }
        //
        //====================================================================================================
        //
        public override string VirtualPath {
            get {
                return "/" + cpCore.serverConfig.appConfig.name;
            }
        }
        //
        //====================================================================================================
        //
        public override string EncodeAppRootPath(string Link) {
            return genericController.EncodeAppRootPath(genericController.encodeText(Link), cpCore.webServer.requestVirtualFilePath, requestAppRootPath, cpCore.webServer.requestDomain);
        }
        //
        //====================================================================================================
        //
        public override bool IsTesting() { return false; }
        //
        //====================================================================================================
        //
        public override void LogActivity(string Message, int UserID, int OrganizationID) {
            logController.logActivity(cpCore, Message, 0, UserID, OrganizationID);
        }
        //
        //====================================================================================================
        //
        public override void LogWarning(string name, string description, string typeOfWarningKey, string instanceKey) {
            logController.reportWarning(cpCore, name, description, "", 0, description, typeOfWarningKey, instanceKey);
        }
        //
        //====================================================================================================
        //
        public override void LogAlarm(string cause) {
            logController.appendLog(cpCore, cause, "Alarm");
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
                cpCore.handleException(Ex, "n/a", 2);
            } else {
                cpCore.handleException(Ex, Message, 2);
            }
        }
        //
        //====================================================================================================
        //
        public override void RequestTask(string Command, string SQL, string ExportName, string Filename) {
            try {
                var ExportCSVAddon = Models.Entity.addonModel.create(cpCore, addonGuidExportCSV);
                if (ExportCSVAddon == null) {
                    cpCore.handleException(new ApplicationException("ExportCSV addon not found. Task could not be added to task queue."));
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
                    taskSchedulerController.addTaskToQueue(cpCore, taskCommandBuildCsv, cmdDetail, false);
                }
            } catch (Exception) {
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void TestPoint(string Message) {
            debugController.testPoint(cpCore, Message);
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
            linkAliasController.addLinkAlias(cpCore, linkAlias, pageId, queryStringSuffix);
        }
        //
        //====================================================================================================
        //
        public override string ThrowEvent(string eventNameIdOrGuid) {
            return cpCore.addon.throwEvent(eventNameIdOrGuid);
        }
        //
        //====================================================================================================
        //
        public override Dictionary<string, routeClass> getRouteDictionary() {
            return cpCore.routeDictionary;
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