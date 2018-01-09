
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
using Contensive.Core.Models.Complex;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core {
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
        private Contensive.Core.coreClass cpCore;
        protected bool disposed = false;
        //
        //====================================================================================================
        // Constructor
        public CPRequestClass(Contensive.Core.coreClass cpCoreObj) : base() {
            cpCore = cpCoreObj;
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
        public override string Browser {
            get {
                return cpCore.webServer.requestBrowser;
            }
        }
        //
        //====================================================================================================
        public override bool BrowserIsIE {
            get {
                return cpCore.doc.sessionContext.visit_browserIsIE;
            }
        }
        //
        //====================================================================================================
        public override bool BrowserIsMac {
            get {
                return cpCore.doc.sessionContext.visit_browserIsMac;
            }
        }
        //
        //====================================================================================================
        public override bool BrowserIsMobile {
            get {
                return cpCore.doc.sessionContext.visit.Mobile;
            }
        }
        //
        //====================================================================================================
        public override bool BrowserIsWindows {
            get {
                return cpCore.doc.sessionContext.visit_browserIsWindows;
            }
        }
        //
        //====================================================================================================
        public override string BrowserVersion {
            get {
                return cpCore.doc.sessionContext.visit_browserVersion;
            }
        }
        //
        //====================================================================================================
        public override string Cookie(string CookieName) {
            return cpCore.webServer.getRequestCookie(CookieName);
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
                foreach (KeyValuePair<string, iisController.cookieClass> kvp in cpCore.webServer.requestCookies) {
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
                return Controllers.genericController.convertNameValueDictToREquestString(cpCore.webServer.requestFormDict);
            }
        }
        //
        //====================================================================================================
        public override string FormAction {
            get {
                return cpCore.webServer.serverFormActionURL;
            }
        }
        //
        //====================================================================================================
        public override bool GetBoolean(string RequestName) {
            return cpCore.docProperties.getBoolean(RequestName);
        }
        //
        //====================================================================================================
        public override DateTime GetDate(string RequestName) {
            return cpCore.docProperties.getDate(RequestName);
        }
        //
        //====================================================================================================
        public override int GetInteger(string RequestName) {
            return cpCore.docProperties.getInteger(RequestName);
        }
        //
        //====================================================================================================
        public override double GetNumber(string RequestName) {
            return cpCore.docProperties.getNumber(RequestName);
        }
        //
        //====================================================================================================
        public override string GetText(string RequestName) {
            return cpCore.docProperties.getText(RequestName);
        }
        //
        //====================================================================================================
        public override string Host {
            get {
                return cpCore.webServer.requestDomain;
            }
        }
        //
        //====================================================================================================
        public override string HTTPAccept {
            get {
                return cpCore.webServer.requestHttpAccept;
            }
        }
        //
        //====================================================================================================
        public override string HTTPAcceptCharset {
            get {
                return cpCore.webServer.requestHttpAcceptCharset;
            }
        }
        //
        //====================================================================================================
        public override string HTTPProfile {
            get {
                return cpCore.webServer.requestHttpProfile;
            }
        }
        //
        //====================================================================================================
        public override string HTTPXWapProfile {
            get {
                return cpCore.webServer.requestxWapProfile;
            }
        }
        //
        //====================================================================================================
        public override string Language {
            get {
                if (cpCore.doc.sessionContext.userLanguage == null) {
                    return "";
                }
                languageModel userLanguage = languageModel.create(cpCore, cpCore.doc.sessionContext.user.LanguageID);
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
                return cpCore.webServer.requestUrl;
            }
        }
        //
        //====================================================================================================
        public override string LinkForwardSource {
            get {
                return cpCore.webServer.linkForwardSource;
            }
        }
        //
        //====================================================================================================
        public override string LinkSource {
            get {
                return cpCore.webServer.requestUrlSource;
            }
        }
        //
        //====================================================================================================
        public override string Page {
            get {
                return cpCore.webServer.requestPage;
            }
        }
        //
        //====================================================================================================
        public override string Path {
            get {
                return cpCore.webServer.requestPath;
            }
        }
        //
        //====================================================================================================
        public override string PathPage {
            get {
                return cpCore.webServer.requestPathPage;
            }
        }
        //
        //====================================================================================================
        public override string Protocol {
            get {
                return cpCore.webServer.requestProtocol;
            }
        }
        //
        //====================================================================================================
        public override string QueryString {
            get {
                return cpCore.webServer.requestQueryString;
            }
        }
        //
        //====================================================================================================
        public override string Referer {
            get {
                return cpCore.webServer.requestReferer;
            }
        }
        //
        //====================================================================================================
        public override string RemoteIP {
            get {
                return cpCore.webServer.requestRemoteIP;
            }
        }
        //
        //====================================================================================================
        public override bool Secure {
            get {
                return cpCore.webServer.requestSecure;
            }
        }
        //
        //====================================================================================================
        public override bool OK(string RequestName) {
            return cpCore.docProperties.containsKey(RequestName);
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