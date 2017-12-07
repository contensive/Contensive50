
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
        //
        //
        public override string GetProperty(string FieldName, string DefaultValue = "") {
            return cpCore.siteProperties.getText(FieldName, DefaultValue);
        }
        //
        //====================================================================================================
        //
        public override bool GetBoolean(string PropertyName, string DefaultValue = "") {
            return genericController.EncodeBoolean(GetProperty(PropertyName, DefaultValue));
        }
        //
        //====================================================================================================
        //
        public override DateTime GetDate(string PropertyName, string DefaultValue = "") {
            return genericController.EncodeDate(GetProperty(PropertyName, DefaultValue));
        }
        //
        //====================================================================================================
        //
        public override int GetInteger(string PropertyName, string DefaultValue = "") {
            return CP.Utils.EncodeInteger(GetProperty(PropertyName, DefaultValue));
        }
        //
        //====================================================================================================
        //
        public override double GetNumber(string PropertyName, string DefaultValue = "") {
            return CP.Utils.EncodeNumber(GetProperty(PropertyName, DefaultValue));
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
        public override bool MultiDomainMode {
            get {
                return false;
            }
        }
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", true)]
        public override string PhysicalFilePath {
            get {
                //cpCore.handleException(New ApplicationException("PhysicalFilePath is no longer supported. Use cp.file.cdnFiles instead to support scale modes"))
                return cpCore.cdnFiles.rootLocalPath;
            }
        }
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", true)]
        public override string PhysicalInstallPath {
            get {
                //cpCore.handleException(New ApplicationException("physicalInstallPath is no longer supported"))
                return cpCore.privateFiles.rootLocalPath;
            }
        }
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", true)]
        public override string PhysicalWWWPath {
            get {
                //cpCore.handleException(New ApplicationException("PhysicalFilePath is no longer supported. Use cp.file.appRootFiles instead to support scale modes"))
                return cpCore.appRootFiles.rootLocalPath;
            }
        }
        //
        //====================================================================================================
        //
        public override bool TrapErrors {
            get {
                return genericController.EncodeBoolean(GetProperty("TrapErrors", "1"));
            }
        }
        //
        //====================================================================================================
        //
        public override string AppPath {
            get {
                return AppRootPath;
                //Return cpCore.web_requestAppPath
            }
        }
        //
        //====================================================================================================
        //
        public override string AppRootPath //Inherits BaseClasses.CPSiteBaseClass.AppRootPath
        {
            get {
                if (false) {
                    return "/";
                } else {
                    return requestAppRootPath;
                }
            }
        }
        //
        //====================================================================================================
        //
        public override string DomainPrimary //Inherits BaseClasses.CPSiteBaseClass.DomainPrimary
        {
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
        public override string Domain //Inherits BaseClasses.CPSiteBaseClass.Domain
        {
            get {
                if (false) {
                    return DomainPrimary;
                } else {
                    return cpCore.webServer.requestDomain;
                }
            }
        }
        //
        //====================================================================================================
        //
        public override string DomainList //Inherits BaseClasses.CPSiteBaseClass.DomainList
        {
            get {
                return string.Join(",", cpCore.serverConfig.appConfig.domainList);
            }
        }
        //
        //====================================================================================================
        //
        public override string FilePath //Inherits BaseClasses.CPSiteBaseClass.FilePath
        {
            get {
                return cpCore.serverConfig.appConfig.cdnFilesNetprefix; // "/" & cpCore.app.config.name & "/files/"
            }
        }
        //
        //====================================================================================================
        //
        public override string PageDefault //Inherits BaseClasses.CPSiteBaseClass.PageDefault
        {
            get {
                if (false) {
                    return GetProperty(siteproperty_serverPageDefault_name, "");
                } else {
                    return cpCore.siteProperties.serverPageDefault;
                }
            }
        }
        //
        //====================================================================================================
        //
        public override string VirtualPath //Inherits BaseClasses.CPSiteBaseClass.VirtualPath
        {
            get {
                return "/" + cpCore.serverConfig.appConfig.name;
            }
        }
        //
        //====================================================================================================
        //
        public override string EncodeAppRootPath(string Link) //Inherits BaseClasses.CPSiteBaseClass.EncodeAppRootPath
        {
            if (false) {
                return Link;
            } else {
                return genericController.EncodeAppRootPath(genericController.encodeText(Link), cpCore.webServer.requestVirtualFilePath, requestAppRootPath, cpCore.webServer.requestDomain);
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsTesting() //Inherits BaseClasses.CPSiteBaseClass.IsTesting
        {
            return false;
        }
        //
        //====================================================================================================
        //
        public override void LogActivity(string Message, int UserID, int OrganizationID) //Inherits BaseClasses.CPSiteBaseClass.LogActivity
        {
            logController.logActivity(cpCore, Message, 0, UserID, OrganizationID);
        }
        //
        //====================================================================================================
        //
        public override void LogWarning(string name, string description, string typeOfWarningKey, string instanceKey) {
            logController.csv_reportWarning(cpCore, name, description, "", 0, description, typeOfWarningKey, instanceKey);
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
        public override void ErrorReport(string Cause) //Inherits BaseClasses.CPSiteBaseClass.ErrorReport
        {
            //
            throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError8(Cause, "", True)
        }
        //
        //====================================================================================================
        //
        public override void ErrorReport(System.Exception Ex, string Message = "") //Inherits BaseClasses.CPSiteBaseClass.ErrorReport
        {
            if (string.IsNullOrEmpty(Message)) {
                cpCore.handleException(Ex, "n/a", 2);
            } else {
                cpCore.handleException(Ex, Message, 2);
            }
            //Dim s As String = ""
            //s = Ex.Source & ", " & Ex.ToString
            //If Message <> "" Then
            //    s = Message & " [" & s & "]"
            //End If
            //cpCore.handleLegacyError8(s, "", True)
        }
        //
        //====================================================================================================
        //
        public override void RequestTask(string Command, string SQL, string ExportName, string Filename) //Inherits BaseClasses.CPSiteBaseClass.RequestTask
        {
            taskSchedulerController.main_RequestTask(cpCore, Command, SQL, ExportName, Filename);
        }
        //
        //====================================================================================================
        //
        public override void TestPoint(string Message) //Inherits BaseClasses.CPSiteBaseClass.TestPoint
        {
            debugController.testPoint(cpCore, Message);
        }
        //
        //====================================================================================================
        //
        public override int LandingPageId(string DomainName = "") //Inherits BaseClasses.CPSiteBaseClass.LandingPageId
        {
            return Convert.ToInt32(GetProperty("LandingPageID", ""));
        }
        //
        //====================================================================================================
        //
        public override void addLinkAlias(string linkAlias, int pageId, string queryStringSuffix = "") {
            docController.addLinkAlias(cpCore, linkAlias, pageId, queryStringSuffix);
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