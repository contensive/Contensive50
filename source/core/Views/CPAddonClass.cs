
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
                return cp.core.doc.addonModelStack.Peek().admin;
            }
        }
        //
        //====================================================================================================
        //
        public override string ArgumentList {
            get {
                return cp.core.doc.addonModelStack.Peek().argumentList;
            }
        }
        //
        //====================================================================================================
        //
        public override bool AsAjax {
            get {
                return cp.core.doc.addonModelStack.Peek().admin;
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
                return cp.core.doc.addonModelStack.Peek().collectionID;
            }
        }
        //
        //====================================================================================================
        //
        public override bool Content {
            get {
                return cp.core.doc.addonModelStack.Peek().content;
            }
        }
        //
        //====================================================================================================
        //
        public override string Copy {
            get {
                return cp.core.doc.addonModelStack.Peek().copy;

            }
        }
        //
        //====================================================================================================
        //
        public override string CopyText {
            get {
                return cp.core.doc.addonModelStack.Peek().copyText;
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
                return cp.core.doc.addonModelStack.Peek().stylesFilename.content;
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
                return cp.core.doc.addonModelStack.Peek().dotNetClass;
            }
        }
        //
        //====================================================================================================
        //
        public override string FormXML {
            get {
                return cp.core.doc.addonModelStack.Peek().formXML;
            }
        }
        //
        //====================================================================================================
        //
        public override string Help {
            get {
                return cp.core.doc.addonModelStack.Peek().help;
            }
        }
        //
        //====================================================================================================
        //
        public override string HelpLink {
            get {
                return cp.core.doc.addonModelStack.Peek().helpLink;
            }
        }
        //
        //====================================================================================================
        //
        public override string IconFilename {
            get {
                return cp.core.doc.addonModelStack.Peek().iconFilename;
            }
        }
        //
        //====================================================================================================
        //
        public override int IconHeight {
            get {
                return cp.core.doc.addonModelStack.Peek().iconHeight;
            }
        }
        //
        //====================================================================================================
        //
        public override int IconSprites {
            get {
                return cp.core.doc.addonModelStack.Peek().iconSprites;
            }
        }
        //
        //====================================================================================================
        //
        public override int IconWidth {
            get {
                return cp.core.doc.addonModelStack.Peek().iconWidth;
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
                return cp.core.doc.addonModelStack.Peek().inFrame;
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsInline {
            get {
                return cp.core.doc.addonModelStack.Peek().isInline;
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
                return cp.core.doc.addonModelStack.Peek().link;
            }
        }
        //
        //====================================================================================================
        //
        public override string MetaDescription {
            get {
                return cp.core.doc.addonModelStack.Peek().metaDescription;
            }
        }
        //
        //====================================================================================================
        //
        public override string MetaKeywordList {
            get {
                return cp.core.doc.addonModelStack.Peek().metaKeywordList;
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
                switch(cp.core.doc.addonModelStack.Peek().navTypeID) {
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
                return cp.core.doc.addonModelStack.Peek().objectProgramID;
            }
        }
        //
        //====================================================================================================
        //
        public override bool OnBodyEnd {
            get {
                return cp.core.doc.addonModelStack.Peek().onBodyEnd;
            }
        }
        //
        //====================================================================================================
        //
        public override bool OnBodyStart {
            get {
                return cp.core.doc.addonModelStack.Peek().onBodyStart;
            }
        }
        //
        //====================================================================================================
        //
        public override bool OnContentEnd {
            get {
                return cp.core.doc.addonModelStack.Peek().onPageEndEvent;
            }
        }
        //
        //====================================================================================================
        //
        public override bool OnContentStart {
            get {
                return cp.core.doc.addonModelStack.Peek().onPageStartEvent;
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
                return cp.core.doc.addonModelStack.Peek().otherHeadTags;
            }
        }
        //
        //====================================================================================================
        //
        public override string PageTitle {
            get {
                return cp.core.doc.addonModelStack.Peek().pageTitle;
            }
        }
        //
        //====================================================================================================
        //
        public override string ProcessInterval {
            get {
                return cp.core.doc.addonModelStack.Peek().processInterval.ToString();
            }
        }
        //
        //====================================================================================================
        //
        public override DateTime ProcessNextRun {
            get {
                return cp.core.doc.addonModelStack.Peek().processNextRun;
            }
        }
        //
        //====================================================================================================
        //
        public override bool ProcessRunOnce {
            get {
                return cp.core.doc.addonModelStack.Peek().processRunOnce;
            }
        }
        //
        //====================================================================================================
        //
        public override string RemoteAssetLink {
            get {
                return cp.core.doc.addonModelStack.Peek().remoteAssetLink;
            }
        }
        //
        //====================================================================================================
        //
        public override bool RemoteMethod {
            get {
                return cp.core.doc.addonModelStack.Peek().remoteMethod;
            }
        }
        //
        //====================================================================================================
        //
        public override string RobotsTxt {
            get {
                return cp.core.doc.addonModelStack.Peek().robotsTxt;
            }
        }
        //
        //====================================================================================================
        //
        public override string ScriptCode {
            get {
                return cp.core.doc.addonModelStack.Peek().scriptingCode;
            }
        }
        //
        //====================================================================================================
        //
        public override string ScriptEntryPoint {
            get {
                return cp.core.doc.addonModelStack.Peek().scriptingEntryPoint;
            }
        }
        //
        //====================================================================================================
        //
        public override string ScriptLanguage {
            get {
                if (cp.core.doc.addonModelStack.Peek().scriptingLanguageID.Equals(2)) {
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
                return cp.core.doc.addonModelStack.Peek().template;
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