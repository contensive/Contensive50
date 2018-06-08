
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
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor {
    //
    // todo implement or deprecate. might be nice to have this convenient api, but a model does the same, costs one query but will
    // always have the model at the save version as the addon code - this cp interface will match the database, but not the addon.
    // not sure which is better
    public class CPAddonClass : BaseClasses.CPAddonBaseClass, IDisposable {
        //
        // todo remove all com guid references
        #region COM GUIDs
        public const string ClassId = "6F43E5CA-6367-475C-AE65-FC988234922A";
        public const string InterfaceId = "440D19E3-47A9-4CA2-B20C-077221015525";
        public const string EventsId = "70B800AA-148A-4338-9EDB-70C85E1ADBDD";
        #endregion
        //
        private CPClass cp;
        //private bool UpgradeOK { get; set; }
        //
        // ====================================================================================================
        //
        public CPAddonClass(CPClass cp) : base() {
            this.cp = cp;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// must call to dispose
        /// </summary>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    cp = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        protected bool disposed = false;
        //
        //====================================================================================================
        //
        public override bool Admin {
            get {
                return cp.core.doc.addonModelStack.Peek().Admin;
            }
        }
        //
        //====================================================================================================
        //
        public override string ArgumentList {
            get {
                return cp.core.doc.addonModelStack.Peek().ArgumentList;
            }
        }
        //
        //====================================================================================================
        //
        public override bool AsAjax {
            get {
                return cp.core.doc.addonModelStack.Peek().Admin;
            }
        }
        //
        //====================================================================================================
        //
        [Obsolete("This is no longer supported. Add a overriding style in another stylesheet instead of modifying", true)]
        public override string BlockDefaultStyles {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override string ccGuid {
            get {
                return cp.core.doc.addonModelStack.Peek().ccguid;
            }
        }
        //
        //====================================================================================================
        //
        public override int CollectionID {
            get {
                return cp.core.doc.addonModelStack.Peek().CollectionID;
            }
        }
        //
        //====================================================================================================
        //
        public override bool Content {
            get {
                return cp.core.doc.addonModelStack.Peek().Content;
            }
        }
        //
        //====================================================================================================
        //
        public override string Copy {
            get {
                return cp.core.doc.addonModelStack.Peek().Copy;

            }
        }
        //
        //====================================================================================================
        //
        public override string CopyText {
            get {
                return cp.core.doc.addonModelStack.Peek().CopyText;
            }
        }
        //
        //====================================================================================================
        //
        [Obsolete("This is no longer supported.", true)]
        public override string CustomStyles {
            get {
                return "";
            }
        }
        //
        //====================================================================================================
        //
        [Obsolete("This is no longer supported.", true)]
        public override string DefaultStyles {
            get {
                return cp.core.doc.addonModelStack.Peek().StylesFilename.content;
            }
        }
        //
        //====================================================================================================
        // 
        [Obsolete("This is no longer supported.", true)]
        public override string Description {
            get {
                return "";
            }
        }
        //
        //====================================================================================================
        // todo finish the methods -- read from doc addonmodelstack peak
        public override string DotNetClass {
            get {
                return cp.core.doc.addonModelStack.Peek().DotNetClass;
            }
        }
        //
        //====================================================================================================
        //
        public override string FormXML {
            get {
                return cp.core.doc.addonModelStack.Peek().FormXML;
            }
        }
        //
        //====================================================================================================
        //
        public override string Help {
            get {
                return cp.core.doc.addonModelStack.Peek().Help;
            }
        }
        //
        //====================================================================================================
        //
        public override string HelpLink {
            get {
                return cp.core.doc.addonModelStack.Peek().HelpLink;
            }
        }
        //
        //====================================================================================================
        //
        public override string IconFilename {
            get {
                return cp.core.doc.addonModelStack.Peek().IconFilename;
            }
        }
        //
        //====================================================================================================
        //
        public override int IconHeight {
            get {
                return cp.core.doc.addonModelStack.Peek().IconHeight;
            }
        }
        //
        //====================================================================================================
        //
        public override int IconSprites {
            get {
                return cp.core.doc.addonModelStack.Peek().IconSprites;
            }
        }
        //
        //====================================================================================================
        //
        public override int IconWidth {
            get {
                return cp.core.doc.addonModelStack.Peek().IconWidth;
            }
        }
        //
        //====================================================================================================
        //
        public override int ID {
            get {
                return cp.core.doc.addonModelStack.Peek().id;
            }
        }
        //
        //====================================================================================================
        //
        public override bool InFrame {
            get {
                return cp.core.doc.addonModelStack.Peek().InFrame;
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsInline {
            get {
                return cp.core.doc.addonModelStack.Peek().IsInline;
            }
        }
        //
        //====================================================================================================
        //
        [Obsolete("This is no longer supported.", true)]
        public override string JavaScriptBodyEnd {
            get {
                return "";
            }
        }
        //
        //====================================================================================================
        //
        [Obsolete("This is no longer supported.", true)]
        public override string JavascriptInHead {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        [Obsolete("Create onready or onload events within your javascript. This method will be deprecated.", false)]
        public override string JavaScriptOnLoad {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override string Link {
            get {
                return cp.core.doc.addonModelStack.Peek().Link;
            }
        }
        //
        //====================================================================================================
        //
        public override string MetaDescription {
            get {
                return cp.core.doc.addonModelStack.Peek().MetaDescription;
            }
        }
        //
        //====================================================================================================
        //
        public override string MetaKeywordList {
            get {
                return cp.core.doc.addonModelStack.Peek().MetaKeywordList;
            }
        }
        //
        //====================================================================================================
        //
        public override string Name {
            get {
                return cp.core.doc.addonModelStack.Peek().name;
            }
        }
        //
        //====================================================================================================
        //
        public override string NavIconType {
            get {
                string result = "";
                switch(cp.core.doc.addonModelStack.Peek().NavTypeID) {
                    case 2:
                        result = "Report";
                        break;
                    case 3:
                        result = "Setting";
                        break;
                    case 4:
                        result = "Tool";
                        break;
                    default:
                        result = "Add-on";
                        break;
                }
                return result ;
            }
        }
        //
        //====================================================================================================
        //
        public override string ObjectProgramID {
            get {
                return cp.core.doc.addonModelStack.Peek().ObjectProgramID;
            }
        }
        //
        //====================================================================================================
        //
        public override bool OnBodyEnd {
            get {
                return cp.core.doc.addonModelStack.Peek().OnBodyEnd;
            }
        }
        //
        //====================================================================================================
        //
        public override bool OnBodyStart {
            get {
                return cp.core.doc.addonModelStack.Peek().OnBodyStart;
            }
        }
        //
        //====================================================================================================
        //
        public override bool OnContentEnd {
            get {
                return cp.core.doc.addonModelStack.Peek().OnPageEndEvent;
            }
        }
        //
        //====================================================================================================
        //
        public override bool OnContentStart {
            get {
                return cp.core.doc.addonModelStack.Peek().OnPageStartEvent;
            }
        }
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override bool Open(int AddonId) {
            return false;
        }
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override bool Open(string AddonNameOrGuid) {
            return false;
        }
        //
        //====================================================================================================
        //
        public override string OtherHeadTags {
            get {
                return cp.core.doc.addonModelStack.Peek().OtherHeadTags;
            }
        }
        //
        //====================================================================================================
        //
        public override string PageTitle {
            get {
                return cp.core.doc.addonModelStack.Peek().PageTitle;
            }
        }
        //
        //====================================================================================================
        //
        public override string ProcessInterval {
            get {
                return cp.core.doc.addonModelStack.Peek().ProcessInterval.ToString();
            }
        }
        //
        //====================================================================================================
        //
        public override DateTime ProcessNextRun {
            get {
                return cp.core.doc.addonModelStack.Peek().ProcessNextRun;
            }
        }
        //
        //====================================================================================================
        //
        public override bool ProcessRunOnce {
            get {
                return cp.core.doc.addonModelStack.Peek().ProcessRunOnce;
            }
        }
        //
        //====================================================================================================
        //
        public override string RemoteAssetLink {
            get {
                return cp.core.doc.addonModelStack.Peek().RemoteAssetLink;
            }
        }
        //
        //====================================================================================================
        //
        public override bool RemoteMethod {
            get {
                return cp.core.doc.addonModelStack.Peek().RemoteMethod;
            }
        }
        //
        //====================================================================================================
        //
        public override string RobotsTxt {
            get {
                return cp.core.doc.addonModelStack.Peek().RobotsTxt;
            }
        }
        //
        //====================================================================================================
        //
        public override string ScriptCode {
            get {
                return cp.core.doc.addonModelStack.Peek().ScriptingCode;
            }
        }
        //
        //====================================================================================================
        //
        public override string ScriptEntryPoint {
            get {
                return cp.core.doc.addonModelStack.Peek().ScriptingEntryPoint;
            }
        }
        //
        //====================================================================================================
        //
        public override string ScriptLanguage {
            get {
                if (cp.core.doc.addonModelStack.Peek().ScriptingLanguageID.Equals(2)) {
                    return "javascript";
                } else {
                    return "vbscript";
                }
            }
        }
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated", true)]
        public override string SharedStyles {
            get {
                return "";
            }
        }
        //
        //====================================================================================================
        //
        public override bool Template {
            get {
                return cp.core.doc.addonModelStack.Peek().Template;
            }
        }
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPAddonClass() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}