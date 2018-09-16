
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
    public class CPContextClass : IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "9FC3B58E-F6A4-4DEA-BE39-B40B09FBE0B7";
        public const string InterfaceId = "4CD84EB3-175C-4004-8811-8257849F549A";
        public const string EventsId = "8C6AC359-68B4-49A3-A3BC-7A53CA16EA45";
        #endregion
        //
        protected bool disposed { get; set; } = false;
        private CPClass cp { get; set; }
        private bool localallowProfileLog { get; set; } = false;
        //
        //==========================================================================================
        //
        public CPContextClass( CPClass cpParent) : base() {
            cp = cpParent;
        }
        //
        //==========================================================================================
        //
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                }
                //
                // Add code here to release the unmanaged resource.
                //
                this.disposed = true;
            }
        }
        //
        //==========================================================================================
        //
        public bool allowDebugLog {
            get {
                return cp.core.doc.allowDebugLog;
            }
            set {
                cp.core.doc.allowDebugLog = value;
            }
        }
        //
        //==========================================================================================
        //
        public string pathPage {
            get {
                return cp.core.webServer.requestPathPage;
            }
            set {
                cp.core.webServer.requestPathPage = value;
            }
        }
        //
        //==========================================================================================
        //
        public string referrer {
            get {
                return cp.core.webServer.requestReferrer;
            }
            set {
                cp.core.webServer.requestReferrer = value;
            }
        }
        //
        //==========================================================================================
        //
        public string domain {
            get {
                return cp.core.webServer.requestDomain;
            }
            set {
                cp.core.webServer.requestDomain = value;
            }
        }
        //
        //==========================================================================================
        //
        public string queryString {
            get {
                return cp.core.webServer.requestQueryString;
            }
            set {
                cp.core.webServer.requestQueryString = value;
            }
        }
        //
        //==========================================================================================
        //
        public bool isSecure {
            get {
                return cp.core.webServer.requestSecure;
            }
            set {
                cp.core.webServer.requestSecure = value;
            }
        }
        //
        //==========================================================================================
        //
        public string remoteIp {
            get {
                return cp.core.webServer.requestRemoteIP;
            }
            set {
                cp.core.webServer.requestRemoteIP = value;
            }
        }
        //
        //==========================================================================================
        //
        public string browserUserAgent {
            get {
                return cp.core.webServer.requestBrowser;
            }
            set {
                cp.core.webServer.requestBrowser = value;
            }
        }
        //
        //==========================================================================================
        //
        public string acceptLanguage {
            get {
                return cp.core.webServer.requestLanguage;
            }
            set {
                cp.core.webServer.requestLanguage = value;
            }
        }
        //
        //==========================================================================================
        //
        public string accept {
            get {
                return cp.core.webServer.requestHttpAccept;
            }
            set {
                cp.core.webServer.requestHttpAccept = value;
            }
        }
        //
        //==========================================================================================
        //
        public string acceptCharSet {
            get {
                return cp.core.webServer.requestHttpAcceptCharset;
            }
            set {
                cp.core.webServer.requestHttpAcceptCharset = value;
            }
        }
        //
        //==========================================================================================
        //
        public string profileUrl {
            get {
                return cp.core.webServer.requestHttpProfile;
            }
            set {
                cp.core.webServer.requestHttpProfile = value;
            }
        }
        //
        //==========================================================================================
        //
        public string xWapProfile {
            get {
                return cp.core.webServer.requestxWapProfile;
            }
            set {
                cp.core.webServer.requestxWapProfile = value;
            }
        }
        //
        //==========================================================================================
        //
        public bool isBinaryRequest {
            get {
                return cp.core.webServer.requestFormUseBinaryHeader;
            }
            set {
                cp.core.webServer.requestFormUseBinaryHeader = value;
            }
        }
        //
        //==========================================================================================
        //
        public byte[] binaryRequest {
            get {
                return cp.core.webServer.requestFormBinaryHeader;
            }
            set {
                cp.core.webServer.requestFormBinaryHeader = value;
            }
        }
        //
        //==========================================================================================
        /// <summary>
        /// get or set request cookies using a name value string similar to a querystring 
        /// </summary>
        /// <returns></returns>
        public string cookies {
            get {
                string cookieString = "";
                foreach (KeyValuePair<string, WebServerController.CookieClass> kvp in cp.core.webServer.requestCookies) {
                    cookieString += "&" + kvp.Key + "=" + kvp.Value.value;
                }
                if (cookieString.Length > 0) {
                    cookieString.Substring(1);
                }
                return cookieString;
            }
            set {
                string[] ampSplit = null;
                int ampSplitCount = 0;
                if (!string.IsNullOrEmpty(value)) {
                    ampSplit = value.Split('&');
                    ampSplitCount = ampSplit.GetUpperBound(0) + 1;
                    for (var ampSplitPointer = 0; ampSplitPointer < ampSplitCount; ampSplitPointer++) {
                        WebServerController.CookieClass newCookie = new WebServerController.CookieClass();
                        string cookieName = null;
                        string NameValue = ampSplit[ampSplitPointer];
                        string[] ValuePair = NameValue.Split('=');
                        cookieName = decodeResponseVariable(encodeText(ValuePair[0]));
                        newCookie.name = cookieName;
                        if (ValuePair.GetUpperBound(0) > 0) {
                            newCookie.value = decodeResponseVariable(encodeText(ValuePair[1]));
                        }
                        if (cp.core.webServer.requestCookies.ContainsKey(cookieName)) {
                            cp.core.webServer.requestCookies.Remove(cookieName);
                        }
                        cp.core.webServer.requestCookies.Add(cookieName, newCookie);
                    }
                }
            }
        }
        //
        //==========================================================================================
        //
        public string form {
            get {
                return genericController.convertNameValueDictToREquestString(cp.core.webServer.requestFormDict);
            }
            set {
                cp.core.webServer.requestFormDict.Clear();
                if (!string.IsNullOrEmpty(value)) {
                    string[] keyValuePairs = value.Split('&');
                    foreach (string keyValuePair in keyValuePairs) {
                        if (!string.IsNullOrEmpty(keyValuePair)) {
                            string[] keyValue = keyValuePair.Split('=');
                            if (keyValue.Length > 1) {
                                cp.core.webServer.requestFormDict.Add(keyValue[0], keyValue[1]);
                            } else {
                                cp.core.webServer.requestFormDict.Add(keyValue[0], "");
                            }
                        }
                    }
                }
            }
        }
        //==========================================================================================
        /// <summary>
        /// A URL encoded name=value string that contains the context for uploaded files. Each file contains five nameValues. The names are prefixed with a counter. The format is as follows: 0formname=formname&0filename=filename&0type=fileType&0tmpFile=tempfilename&0error=errors&0size=fileSize
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        [Obsolete("Deprecated", true)]
        public string formFiles {
            get {
                return "";
            }
            set {
                //
            }
        }
        //
        //==========================================================================================
        //
        public bool requestNameSpaceAsUnderscore {
            get {
                return cp.core.webServer.requestSpaceAsUnderscore;
            }
            set {
                cp.core.webServer.requestSpaceAsUnderscore = value;
            }
        }
        //
        //==========================================================================================
        //
        public bool requestNameDotAsUnderscore {
            get {
                return cp.core.webServer.requestDotAsUnderscore;
            }
            set {
                cp.core.webServer.requestDotAsUnderscore = value;
            }
        }
        //
        //==========================================================================================
        //
        public string responseRedirect {
            get {
                return cp.core.webServer.bufferRedirect;
            }
        }
        //
        //==========================================================================================
        //
        [Obsolete("responseBuffer is deprecated. Each method returns its content.")]
        public string responseBuffer {
            get {
                return string.Empty;
            }
        }
        //
        //==========================================================================================
        //
        public string responseContentType {
            get {
                return cp.core.webServer.bufferContentType;
            }
        }
        //
        //==========================================================================================
        //
        public string responseCookies {
            get {
                return cp.core.webServer.bufferCookies;
            }
        }
        //
        //==========================================================================================
        //
        public string responseHeaders {
            get {
                return cp.core.webServer.bufferResponseHeader;
            }
        }
        //
        //==========================================================================================
        //
        public string responseStatus {
            get {
                return cp.core.webServer.bufferResponseStatus;
            }
        }
        //
        //==========================================================================================
        //
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPContextClass() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}