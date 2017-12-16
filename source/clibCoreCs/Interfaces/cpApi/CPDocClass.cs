
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
    public class CPDocClass : BaseClasses.CPDocBaseClass, IDisposable {
        #region COM GUIDs
        public const string ClassId = "414BD6A9-195F-4E0F-AE24-B7BF56749CDD";
        public const string InterfaceId = "347D06BC-4D68-4DBE-82FE-B72115E24A56";
        public const string EventsId = "95E8786B-E778-4617-96BA-B45C53E4AFD1";
        #endregion
        //
        private CPClass cp;
        private Contensive.Core.coreClass cpCore;
        protected bool disposed = false;
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cpParent"></param>
        public CPDocClass(CPClass cpParent) : base() {
            cp = cpParent;
            cpCore = cp.core;
        }
        //
        //====================================================================================================
        /// <summary>
        /// destructor
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                appendDebugLog(".dispose, dereference main, csv");
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    cpCore = null;
                    cp = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Returns the page content
        /// </summary>
        /// <returns></returns>
        public override string Content {
            get {
                return cpCore.doc.bodyContent;
            }
            set {
                cpCore.doc.bodyContent = value;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// deprecated.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use addon navigation.", true)]
        public override string NavigationStructure {
            get {
                return string.Empty ;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Returns to the current value of NoFollow, set by addon execution
        /// </summary>
        /// <returns></returns>
        public override bool NoFollow {
            get {
                return cpCore.webServer.response_NoFollow;
            }
            set {
                cpCore.webServer.response_NoFollow = value;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the pageId
        /// </summary>
        /// <returns></returns>
        public override int PageId {
            get {
                if (cpCore.doc.page == null) {
                    return 0;
                } else {
                    return cpCore.doc.page.id;
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the page name, set by the pagemenager addon
        /// </summary>
        /// <returns></returns>
        public override string PageName {
            get {
                if (cpCore.doc.page == null) {
                    return string.Empty;
                } else {
                    return cpCore.doc.page.name;
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the current value of refreshquerystring 
        /// </summary>
        /// <returns></returns>
        public override string RefreshQueryString {
            get {
                return cpCore.doc.refreshQueryString;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Returns the value of sectionId
        /// </summary>
        [Obsolete("Section is no longer supported", true)]
        public override int SectionId {
            get {
                return 0;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// the time and date when this document was started 
        /// </summary>
        /// <returns></returns>
        public override DateTime StartTime {
            get {
                return cpCore.doc.profileStartTime;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the id of the template, as set by the page manager
        /// </summary>
        /// <returns></returns>
        public override int TemplateId {
            get {
                if (cpCore.doc != null) {
                    if (cpCore.doc.template != null) {
                        return cpCore.doc.template.ID;
                    }
                }
                return 0;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the docType, set by the page manager settings 
        /// </summary>
        /// <returns></returns>
        public override string Type {
            get {
                return cpCore.siteProperties.docTypeDeclaration;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// adds javascript code to the head of the document
        /// </summary>
        /// <param name="NewCode"></param>
        public override void AddHeadJavascript(string NewCode) {
            cpCore.html.addScriptCode_head(NewCode, "");
        }
        //
        //====================================================================================================
        /// <summary>
        /// adds a javascript tag to the head of the document
        /// </summary>
        /// <param name="HeadTag"></param>
        public override void AddHeadTag(string HeadTag) {
            cpCore.html.addHeadTag(HeadTag);
        }
        //
        //====================================================================================================
        //
        public override void AddMetaDescription(string MetaDescription) {
            cpCore.html.addMetaDescription(MetaDescription);
        }
        //
        //====================================================================================================
        //
        public override void AddMetaKeywordList(string MetaKeywordList) {
            cpCore.html.addMetaKeywordList(MetaKeywordList);
        }
        //
        //====================================================================================================
        //
        public override void AddOnLoadJavascript(string NewCode) {
            cpCore.html.addScriptCode_onLoad(NewCode, "");
        }
        //
        //====================================================================================================
        //
        public override void AddTitle(string PageTitle) {
            cpCore.html.addTitle(PageTitle);
        }
        //
        //====================================================================================================
        //
        public override void AddRefreshQueryString(string Name, string Value) {
            cpCore.doc.addRefreshQueryString(Name, Value);
        }
        //
        //====================================================================================================
        //
        public override void AddHeadStyle(string StyleSheet) {
            AddHeadTag("\r\n\t<style type=\"text/css\">\r\n\t\t" + StyleSheet + "\r\n\t</style>");
        }
        //
        //====================================================================================================
        //
        public override void AddHeadStyleLink(string StyleSheetLink) {
            cpCore.html.addStyleLink(StyleSheetLink, "");
        }
        //
        //====================================================================================================
        //
        public override void AddBodyEnd(string NewCode) {
            cpCore.doc.htmlForEndOfBody += NewCode;
        }
        //
        //====================================================================================================
        //
        public override string Body {
            get {
                return cpCore.doc.docBodyFilter;
            }
            set {
                cpCore.doc.docBodyFilter = value;
            }
        }

        //
        //====================================================================================================
        //
        [Obsolete("Site styles are no longer supported. Include styles and javascript in addons.", true)]
        public override string SiteStylesheet {
            get {
                return "";
            }
        }
        //
        //====================================================================================================
        //   Decodes an argument parsed from an AddonOptionString for all non-allowed characters
        //       AddonOptionString is a & delimited string of name=value[selector]descriptor
        //
        //       to get a value from an AddonOptionString, first use getargument() to get the correct value[selector]descriptor
        //       then remove everything to the right of any '['
        //
        //       call encodeaddonoptionargument before parsing them together
        //       call decodeAddonOptionArgument after parsing them apart
        //
        //       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=VAlue[Option1|Option2]
        //
        //       This routine is needed for all Arg, Name, Value, Option values
        //
        //------------------------------------------------------------------------------------------------------------
        //
        public string decodeLegacyOptionStringArgument(string EncodedArg) {
            string tempdecodeLegacyOptionStringArgument = null;
            string a = null;
            //
            tempdecodeLegacyOptionStringArgument = "";
            if (!string.IsNullOrEmpty(EncodedArg)) {
                a = EncodedArg;
                a = genericController.vbReplace(a, "#0058#", ":");
                a = genericController.vbReplace(a, "#0093#", "]");
                a = genericController.vbReplace(a, "#0091#", "[");
                a = genericController.vbReplace(a, "#0124#", "|");
                a = genericController.vbReplace(a, "#0039#", "'");
                a = genericController.vbReplace(a, "#0034#", "\"");
                a = genericController.vbReplace(a, "#0044#", ",");
                a = genericController.vbReplace(a, "#0061#", "=");
                a = genericController.vbReplace(a, "#0038#", "&");
                a = genericController.vbReplace(a, "#0013#", "\r\n");
                tempdecodeLegacyOptionStringArgument = a;
            }
            return tempdecodeLegacyOptionStringArgument;
        }
        //
        //=======================================================================================================
        //
        public override string GetProperty(string PropertyName, string DefaultValue = "") {
            if (cpCore.docProperties.containsKey(PropertyName)) {
                return cpCore.docProperties.getText(PropertyName);
            } else {
                return DefaultValue;
            }
        }
        //
        //=======================================================================================================
        //
        public override bool GetBoolean(string PropertyName, string DefaultValue = "") {
            return genericController.encodeBoolean(GetProperty(PropertyName, DefaultValue));
        }
        //
        //=======================================================================================================
        //
        public override DateTime GetDate(string PropertyName, string DefaultValue = "") {
            return genericController.EncodeDate(GetProperty(PropertyName, DefaultValue));
        }
        //
        //=======================================================================================================
        //
        public override int GetInteger(string PropertyName, string DefaultValue = "") {
            return cp.Utils.EncodeInteger(GetProperty(PropertyName, DefaultValue));
        }
        //
        //=======================================================================================================
        //
        public override double GetNumber(string PropertyName, string DefaultValue = "") {
            return cp.Utils.EncodeNumber(GetProperty(PropertyName, DefaultValue));
        }
        //
        //=======================================================================================================
        //
        public override string GetText(string FieldName, string DefaultValue = "") {
            return GetProperty(FieldName, DefaultValue);
        }
        //
        //=======================================================================================================
        //
        public override bool IsProperty(string FieldName) {
            return cpCore.docProperties.containsKey(FieldName);
        }
        //
        //=======================================================================================================
        //
        public override void SetProperty(string FieldName, string FieldValue) {
            cpCore.docProperties.setProperty(FieldName, FieldValue);
        }
        //
        //=======================================================================================================
        //
        public override void Var(string Index, string Value) {
            SetProperty( Index, Value);
        }
        //
        //=======================================================================================================
        //
        public override string get_GlobalVar(string Index) {
            return get_Var( Index );
        }
        //
        //=======================================================================================================
        //
        public override bool get_IsGlobalVar(string Index) {
            return get_IsVar( Index );
        }
        //
        //=======================================================================================================
        //
        public override bool get_IsVar(string Index) {
            return cpCore.docProperties.containsKey(Index);
        }
        //
        //=======================================================================================================
        //
        public override string get_Var(string Index) {
            return cpCore.docProperties.getText(Index);
        }
        //
        //=======================================================================================================
        //
        public override void set_GlobalVar(string Index, string Value) {
            //
        }
        //
        //=======================================================================================================
        //
        public override bool IsAdminSite {
            get {
                bool returnIsAdmin = false;
                try {
                    returnIsAdmin = (cp.Request.PathPage.IndexOf(cp.Site.GetText("adminUrl"), System.StringComparison.OrdinalIgnoreCase)  != -1);
                } catch (Exception ex) {
                    cpCore.handleException(ex); 
                    throw;
                }
                return returnIsAdmin;
            }
        }
        //
        //=======================================================================================================
        //
        private void appendDebugLog(string copy) {
        }
        //
        //=======================================================================================================
        // debugging -- testpoint
        //
        private void tp(string msg) {
        }
        //
        //=======================================================================================================
        // IDisposable support
        //
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPDocClass() {
            Dispose(false);
            //INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}