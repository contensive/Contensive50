
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
    public class CPAddonClass : BaseClasses.CPAddonBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "6F43E5CA-6367-475C-AE65-FC988234922A";
        public const string InterfaceId = "440D19E3-47A9-4CA2-B20C-077221015525";
        public const string EventsId = "70B800AA-148A-4338-9EDB-70C85E1ADBDD";
        #endregion
        //
        private CPClass cp;
        private bool UpgradeOK { get; set; }
        //
        public CPAddonClass(CPClass cp) : base() {
            this.cp = cp;
        }
        //
        // dispose
        //
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                appendDebugLog(".dispose, dereference main, csv");
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
                return false;
            }
        }
        //
        //====================================================================================================
        //
        public override string ArgumentList {
            get {
                return "";
            }
        }
        //
        //====================================================================================================
        //
        public override bool AsAjax {
            get {
                return false;
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
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override int CollectionID {
            get {
                return 0;
            }
        }
        //
        //====================================================================================================
        //
        public override bool Content {
            get {
                return false;
            }
        }
        //
        //====================================================================================================
        //
        public override string Copy {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override string CopyText {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override string CustomStyles {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override string DefaultStyles {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override string Description {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override string DotNetClass {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override string FormXML {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override string Help {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override string HelpLink {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override string IconFilename {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override int IconHeight {
            get {
                return 0;
            }
        }
        //
        //====================================================================================================
        //
        public override int IconSprites {
            get {
                return 0;

            }
        }
        //
        //====================================================================================================
        //
        public override int IconWidth {
            get {
                return 0;

            }
        }
        //
        //====================================================================================================
        //
        public override int ID {
            get {
                return 0;

            }
        }
        //
        //====================================================================================================
        //
        public override bool InFrame {
            get {

                return false;
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsInline {
            get {
                return false;

            }
        }
        //
        //====================================================================================================
        //
        public override string JavaScriptBodyEnd {
            get {

                return "";
            }
        }
        //
        //====================================================================================================
        //
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
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override string MetaDescription {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override string MetaKeywordList {
            get {

                return "";
            }
        }
        //
        //====================================================================================================
        //
        public override string Name {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override string NavIconType {
            get {
                return "";
            }
        }
        //
        //====================================================================================================
        //
        public override string ObjectProgramID {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override bool OnBodyEnd {
            get {
                return false;

            }
        }
        //
        //====================================================================================================
        //
        public override bool OnBodyStart {
            get {
                return false;

            }
        }
        //
        //====================================================================================================
        //
        public override bool OnContentEnd {
            get {
                return false;

            }
        }
        //
        //====================================================================================================
        //
        public override bool OnContentStart {
            get {
                return false;

            }
        }
        //
        //====================================================================================================
        //
        public override bool Open(int AddonId) {
            return false;

        }
        //
        //====================================================================================================
        //
        public override bool Open(string AddonNameOrGuid) {

            return false;
        }
        //
        //====================================================================================================
        //
        public override string OtherHeadTags {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override string PageTitle {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override string ProcessInterval {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override DateTime ProcessNextRun {
            get {
                return Convert.ToDateTime("12:00:00 AM");
            }
        }
        //
        //====================================================================================================
        //
        public override bool ProcessRunOnce {
            get {
                return false;
            }
        }
        //
        //====================================================================================================
        //
        public override string RemoteAssetLink {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override bool RemoteMethod {
            get {
                return false;
            }
        }
        //
        //====================================================================================================
        //
        public override string RobotsTxt {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
        public override string ScriptCode {
            get {

                return "";
            }
        }
        //
        //====================================================================================================
        //
        public override string ScriptEntryPoint {
            get {

                return "";
            }
        }
        //
        //====================================================================================================
        //
        public override string ScriptLanguage {
            get {
                return "";

            }
        }
        //
        //====================================================================================================
        //
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
                return false;
            }
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
                returnOk = addonInstallClass.InstallCollectionsFromPrivateFile(cp.core, privatePathFilename, returnUserError, ignoreReturnedCollectionGuid, false, new List<string>());
            } catch (Exception ex) {
                cp.core.handleException(ex);
                if (!cp.core.siteProperties.trapErrors) {
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
        private void appendDebugLog(string copy) {
            //
        }
        //
        //====================================================================================================
        //
        private void tp(string msg) {
            //
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
            //INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}