
using System;
using System.Collections.Generic;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;

namespace Contensive.Processor {
    //
    // comVisible to be activeScript compatible
    //
    //[ComVisible(true), Microsoft.VisualBasic.ComClass(CPRequestClass.ClassId, CPRequestClass.InterfaceId, CPRequestClass.EventsId)]
    public class CPRequestClass : BaseClasses.CPRequestBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "EF7782C1-76E4-45A7-BF30-E1CEBCBC56CF";
        public const string InterfaceId = "39D6A73F-C11A-44F4-8405-A4CE3FB0A486";
        public const string EventsId = "C8938AB2-26F0-41D2-A282-3313FD7BA490";
        #endregion
        //
        private Contensive.Processor.Controllers.CoreController core;
        protected bool disposed = false;
        //
        //====================================================================================================
        // Constructor
        public CPRequestClass(Contensive.Processor.Controllers.CoreController coreObj) : base() {
            core = coreObj;
        }
        //
        //====================================================================================================
        // dispose
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
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
        public override string Browser {
            get {
                return core.webServer.requestBrowser;
            }
        }
        //
        //====================================================================================================
        [Obsolete]
        public override bool BrowserIsIE { get { return false; } }
        //
        //====================================================================================================
        [Obsolete]
        public override bool BrowserIsMac { get { return false; } }
        //
        //====================================================================================================
        public override bool BrowserIsMobile {
            get {
                return core.session.visit.mobile;
            }
        }
        //
        //====================================================================================================
        [Obsolete]
        public override bool BrowserIsWindows {
            get {
                return false;
            }
        }
        //
        //====================================================================================================
        [Obsolete]
        public override string BrowserVersion {
            get {
                return "";
            }
        }
        //
        //====================================================================================================
        public override string Cookie(string CookieName) {
            return core.webServer.getRequestCookie(CookieName);
        }
        //
        //====================================================================================================
        /// <summary>
        /// return a string that includes the simple name value pairs for all request cookies
        /// </summary>
        /// <returns></returns>
        public override string CookieString {
            get {
                string returnCookies = "";
                foreach (KeyValuePair<string, WebServerController.CookieClass> kvp in core.webServer.requestCookies) {
                    returnCookies += "&" + kvp.Key + "=" + kvp.Value.value;
                }
                if (returnCookies.Length > 0) {
                    returnCookies = returnCookies.Substring(1);
                }
                return returnCookies;
            }
        }
        //
        //====================================================================================================
        public override string Form {
            get {
                return Controllers.GenericController.convertNameValueDictToREquestString(core.webServer.requestFormDict);
            }
        }
        //
        //====================================================================================================
        public override string FormAction {
            get {
                return core.webServer.serverFormActionURL;
            }
        }
        //
        //====================================================================================================
        public override bool GetBoolean(string RequestName) {
            return core.docProperties.getBoolean(RequestName);
        }
        //
        //====================================================================================================
        public override DateTime GetDate(string RequestName) {
            return core.docProperties.getDate(RequestName);
        }
        //
        //====================================================================================================
        public override int GetInteger(string RequestName) {
            return core.docProperties.getInteger(RequestName);
        }
        //
        //====================================================================================================
        public override double GetNumber(string RequestName) {
            return core.docProperties.getNumber(RequestName);
        }
        //
        //====================================================================================================
        public override string GetText(string RequestName) {
            return core.docProperties.getText(RequestName);
        }
        //
        //====================================================================================================
        public override string Host {
            get {
                return core.webServer.requestDomain;
            }
        }
        //
        //====================================================================================================
        public override string HTTPAccept {
            get {
                return core.webServer.requestHttpAccept;
            }
        }
        //
        //====================================================================================================
        public override string HTTPAcceptCharset {
            get {
                return core.webServer.requestHttpAcceptCharset;
            }
        }
        //
        //====================================================================================================
        public override string HTTPProfile {
            get {
                return core.webServer.requestHttpProfile;
            }
        }
        //
        //====================================================================================================
        public override string HTTPXWapProfile {
            get {
                return core.webServer.requestxWapProfile;
            }
        }
        //
        //====================================================================================================
        public override string Language {
            get {
                if (core.session.userLanguage == null) {
                    return "";
                }
                LanguageModel userLanguage = LanguageModel.create(core, core.session.user.LanguageID);
                if (userLanguage != null) {
                    return userLanguage.name;
                }
                return "English";
            }
        }
        //
        //====================================================================================================
        public override string Link {
            get {
                return core.webServer.requestUrl;
            }
        }
        //
        //====================================================================================================
        public override string LinkForwardSource {
            get {
                return core.webServer.linkForwardSource;
            }
        }
        //
        //====================================================================================================
        public override string LinkSource {
            get {
                return core.webServer.requestUrlSource;
            }
        }
        //
        //====================================================================================================
        public override string Page {
            get {
                return core.webServer.requestPage;
            }
        }
        //
        //====================================================================================================
        public override string Path {
            get {
                return core.webServer.requestPath;
            }
        }
        //
        //====================================================================================================
        public override string PathPage {
            get {
                return core.webServer.requestPathPage;
            }
        }
        //
        //====================================================================================================
        public override string Protocol {
            get {
                return core.webServer.requestProtocol;
            }
        }
        //
        //====================================================================================================
        public override string QueryString {
            get {
                return core.webServer.requestQueryString;
            }
        }
        //
        //====================================================================================================
        public override string Referer {
            get {
                return core.webServer.requestReferer;
            }
        }
        //
        //====================================================================================================
        public override string RemoteIP {
            get {
                return core.webServer.requestRemoteIP;
            }
        }
        //
        //====================================================================================================
        public override bool Secure {
            get {
                return core.webServer.requestSecure;
            }
        }
        //
        //====================================================================================================
        public override bool OK(string RequestName) {
            return core.docProperties.containsKey(RequestName);
        }
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPRequestClass() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}