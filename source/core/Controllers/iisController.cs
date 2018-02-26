
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
using Microsoft.Web.Administration;
using Contensive.Core.Models.Context;
//
namespace Contensive.Core.Controllers {
    /// <summary>
    /// Code dedicated to processing iis input and output. lazy Constructed. (see coreHtmlClass for html processing)
    /// What belongs here is everything that would have to change if we converted to apache
    /// </summary>
    public class iisController {
        //
        private coreController core;
        //
        // if this instance is a webRole, retain pointer for callbacks
        //
        public System.Web.HttpContext iisContext;
        //
        //   State values that must be initialized before Init()
        //   Everything else is derived from these
        //
        //Public Property initCounter As Integer = 0        '
        //
        // -- Buffer request
        public string requestLanguage { get; set; } = ""; // set externally from HTTP_Accept_LANGUAGE
        public string requestHttpAccept { get; set; } = "";
        public string requestHttpAcceptCharset { get; set; } = "";
        public string requestHttpProfile { get; set; } = "";
        public string requestxWapProfile { get; set; } = "";
        public string requestHTTPVia { get; set; } = ""; // informs the server of proxies used during the request
        public string requestHTTPFrom { get; set; } = ""; // contains the email address of the requestor
        public string requestPathPage { get; set; } = ""; // The Path and Page part of the current URI
        public string requestReferrer { get; set; } = "";
        public string requestDomain { get; set; } = ""; // The Host part of the current URI
        public bool requestSecure { get; set; } = false; // Set in InitASPEnvironment, true if https
        public string requestRemoteIP { get; set; } = "";
        public string requestBrowser { get; set; } = ""; // The browser for this visit
        public string requestQueryString { get; set; } = ""; // The QueryString of the current URI
        public bool requestFormUseBinaryHeader { get; set; } = false; // When set true with RequestNameBinaryRead=true, InitEnvironment reads the form in with a binary read
        public byte[] requestFormBinaryHeader { get; set; } // For asp pages, this is the full multipart header
        public Dictionary<string, string> requestFormDict { get; set; } = new Dictionary<string, string>();
        public bool requestSpaceAsUnderscore { get; set; } = false; // when true, is it assumed that dots in request variable names will convert
        public bool requestDotAsUnderscore { get; set; } = false; // (php converts spaces and dots to underscores)
        public string requestUrlSource { get; set; } = "";
        public string linkForwardSource { get; set; } = ""; // main_ServerPathPage -- set during init
        public string linkForwardError { get; set; } = ""; // always 404
                                                           //Public Property readStreamJSForm As Boolean = False                  ' When true, the request comes from a browser handling a JSPage script line
        public bool pageExcludeFromAnalytics { get; set; } = false; // For this page - true for remote methods and ajax
                                                                    //
                                                                    // refactor - this method stears the stream between controllers, put it in core
                                                                    //Public Property outStreamDevice As Integer = 0
        public int memberAction { get; set; } = 0; // action to be performed during init
        public string adminMessage { get; set; } = ""; // For more information message
        public string requestPageReferer { get; set; } = ""; // replaced by main_ServerReferrer
        public string requestReferer { get; set; } = "";
        public string serverFormActionURL { get; set; } = ""; // The Action for all internal forms, if not set, default
        public string requestContentWatchPrefix { get; set; } = ""; // The different between the URL and the main_ContentWatch Pathpage
        public string requestProtocol { get; set; } = ""; // Set in InitASPEnvironment, http or https
        public string requestUrl { get; set; } = ""; // The current URL, from protocol to end of quesrystring
        public string requestVirtualFilePath { get; set; } = ""; // The Virtual path for the site (host+main_ServerVirtualPath+"/" is site URI)
        public string requestPath { get; set; } = ""; // The path part of the current URI
        public string requestPage { get; set; } = ""; // The page part of the current URI
        public string requestSecureURLRoot { get; set; } = ""; // The URL to the root of the secure area for this site
                                                               //
                                                               // -- response
        public bool response_NoFollow { get; set; } = false; // when set, Meta no follow is added
                                                             //
                                                             // -- Buffer responses
        public string bufferRedirect { get; set; } = "";
        public string bufferContentType { get; set; } = "";
        public string bufferCookies { get; set; } = "";
        public string bufferResponseHeader { get; set; } = "";
        public string bufferResponseStatus { get; set; } = "";
        //------------------------------------------------------------------------
        //
        //   QueryString, Form and cookie Processing variables
        public class cookieClass {
            public string name;
            public string value;
        }
        public Dictionary<string, cookieClass> requestCookies;
        //
        //====================================================================================================
        //
        public iisController(coreController core) : base() {
            this.core = core;
            requestCookies = new Dictionary<string, cookieClass>();
        }
        //
        //=======================================================================================
        //   IIS Reset
        //
        //   Must be called from a process running as admin
        //   This can be done using the command queue, which kicks off the ccCmd process from the Server
        //
        public void reset() {
            try {
                string Cmd = null;
                string arg = null;
                string LogFilename = null;
                string Copy = null;
                //
                LogFilename = "Temp\\" + genericController.encodeText(genericController.GetRandomInteger(core)) + ".Log";
                Cmd = "IISReset.exe";
                arg = "/restart >> \"" + LogFilename + "\"";
                runProcess(core, Cmd, arg, true);
                Copy = core.privateFiles.readFileText(LogFilename);
                core.privateFiles.deleteFile(LogFilename);
                Copy = genericController.vbReplace(Copy, "\r\n", "\\n");
                Copy = genericController.vbReplace(Copy, "\r", "\\n");
                Copy = genericController.vbReplace(Copy, "\n", "\\n");
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //=======================================================================================
        //   Stop IIS
        //
        //   Must be called from a process running as admin
        //   This can be done using the command queue, which kicks off the ccCmd process from the Server
        //
        public void stop() {
            try {
                string Cmd = null;
                string LogFilename = null;
                string Copy = null;
                //
                LogFilename = "Temp\\" + genericController.encodeText(genericController.GetRandomInteger(core)) + ".Log";
                Cmd = "%comspec% /c IISReset /stop >> \"" + LogFilename + "\"";
                runProcess(core, Cmd, "", true);
                Copy = core.privateFiles.readFileText(LogFilename);
                core.privateFiles.deleteFile(LogFilename);
                Copy = genericController.vbReplace(Copy, "\r\n", "\\n");
                Copy = genericController.vbReplace(Copy, "\r", "\\n");
                Copy = genericController.vbReplace(Copy, "\n", "\\n");
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //=======================================================================================
        //   Start IIS
        //
        //   Must be called from a process running as admin
        //   This can be done using the command queue, which kicks off the ccCmd process from the Server
        //=======================================================================================
        //
        public void start() {
            try {
                string Cmd = null;
                string LogFilename = core.privateFiles.localAbsRootPath + "iisResetPipe.log";
                string Copy = null;
                //
                Cmd = "%comspec% /c IISReset /start >> \"" + LogFilename + "\"";
                runProcess(core, Cmd, "", true);
                Copy = core.privateFiles.readFileText(LogFilename);
                core.privateFiles.deleteFile(LogFilename);
                Copy = genericController.vbReplace(Copy, "\r\n", "\\n");
                Copy = genericController.vbReplace(Copy, "\r", "\\n");
                Copy = genericController.vbReplace(Copy, "\n", "\\n");
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //=======================================================================================
        // recycle iis process
        //
        public void recycle(string appName) {
            try {
                ServerManager serverManager = null;
                ApplicationPoolCollection appPoolColl = null;
                //
                serverManager = new ServerManager();
                appPoolColl = serverManager.ApplicationPools;
                foreach (ApplicationPool appPool in appPoolColl) {
                    if (appPool.Name.ToLower() == appName.ToLower()) {
                        if (appPool.Start() == ObjectState.Started) {
                            appPool.Recycle();
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //==================================================================================
        //   Initialize the application
        //       returns responseOpen
        //==================================================================================
        //
        public bool initWebContext(System.Web.HttpContext httpContext) {
            try {
                //
                // -- setup IIS Response
                iisContext = httpContext;
                iisContext.Response.CacheControl = "no-cache";
                iisContext.Response.Expires = -1;
                iisContext.Response.Buffer = true;
                //
                //
                // -- basic request environment
                requestDomain = iisContext.Request.ServerVariables["SERVER_NAME"];
                requestPathPage = encodeText(iisContext.Request.ServerVariables["SCRIPT_NAME"]);
                requestReferrer = encodeText(iisContext.Request.ServerVariables["HTTP_REFERER"]);
                requestSecure = encodeBoolean(iisContext.Request.ServerVariables["SERVER_PORT_SECURE"]);
                requestRemoteIP = encodeText(iisContext.Request.ServerVariables["REMOTE_ADDR"]);
                requestBrowser = encodeText(iisContext.Request.ServerVariables["HTTP_USER_AGENT"]);
                requestLanguage = encodeText(iisContext.Request.ServerVariables["HTTP_ACCEPT_LANGUAGE"]);
                requestHttpAccept = encodeText(iisContext.Request.ServerVariables["HTTP_ACCEPT"]);
                requestHttpAcceptCharset = encodeText(iisContext.Request.ServerVariables["HTTP_ACCEPT_CHARSET"]);
                requestHttpProfile = encodeText(iisContext.Request.ServerVariables["HTTP_PROFILE"]);
                //
                // -- http QueryString
                if (iisContext.Request.QueryString.Count > 0) {
                    requestQueryString = "";
                    foreach (string key in iisContext.Request.QueryString) {
                        string keyValue = iisContext.Request.QueryString[key];
                        core.docProperties.setProperty(key, keyValue);
                        requestQueryString = genericController.ModifyQueryString(requestQueryString, key, keyValue);
                    }
                }
                //
                // -- form
                requestFormDict.Clear();
                foreach (string key in iisContext.Request.Form.Keys) {
                    string keyValue = iisContext.Request.Form[key];
                    core.docProperties.setProperty(key, keyValue, true);
                    if (requestFormDict.ContainsKey(keyValue)) {
                        requestFormDict.Remove(keyValue);
                    }
                    requestFormDict.Add(key, keyValue);
                }
                //
                // -- handle files
                int filePtr = 0;
                string instanceId = genericController.createGuid().Replace("{", "").Replace("-", "").Replace("}", "");
                string[] formNames = iisContext.Request.Files.AllKeys;
                foreach (string formName in formNames) {
                    System.Web.HttpPostedFile file = iisContext.Request.Files[formName];
                    if (file != null) {
                        if ((file.ContentLength > 0) && (!string.IsNullOrEmpty(file.FileName))) {
                            docPropertiesClass prop = new docPropertiesClass();
                            prop.Name = formName;
                            prop.Value = file.FileName;
                            prop.NameValue = EncodeRequestVariable(prop.Name) + "=" + EncodeRequestVariable(prop.Value);
                            prop.IsFile = true;
                            prop.IsForm = true;
                            prop.tempfilename = instanceId + "-" + filePtr.ToString() + ".bin";
                            file.SaveAs(core.tempFiles.joinPath(core.tempFiles.localAbsRootPath, prop.tempfilename));
                            core.tempFiles.deleteOnDisposeFileList.Add(prop.tempfilename);
                            prop.FileSize = encodeInteger(file.ContentLength);
                            core.docProperties.setProperty(formName, prop);
                            filePtr += 1;
                        }
                    }
                }
                //
                // load request cookies
                //
                foreach (string key in iisContext.Request.Cookies) {
                    string keyValue = iisContext.Request.Cookies[key].Value;
                    keyValue = DecodeResponseVariable(keyValue);
                    addRequestCookie(key, keyValue);
                }
                //
                //--------------------------------------------------------------------------
                //
                if (core.appConfig.appStatus != appConfigModel.appStatusEnum.ok) {
                    //
                    // did not initialize correctly
                    //
                } else {
                    //
                    // continue
                    //
                    //initCounter += 1
                    //
                    core.html.enableOutputBuffer(true);
                    core.doc.continueProcessing = true;
                    setResponseContentType("text/html");
                    //
                    //--------------------------------------------------------------------------
                    // ----- Process QueryString to core.doc.main_InStreamArray
                    //       Do this first to set core.main_ReadStreamJSForm, core.main_ReadStreamJSProcess, core.main_ReadStreamBinaryRead (must be in QS)
                    //--------------------------------------------------------------------------
                    //
                    linkForwardSource = "";
                    linkForwardError = "";
                    //
                    // start with the best guess for the source url, then improve the guess based on what iis might have done
                    //
                    requestUrlSource = "http://";
                    if (requestSecure) {
                        requestUrlSource = "https://";
                    }
                    requestUrlSource = requestUrlSource + requestDomain + requestPathPage;
                    if (requestQueryString != "") {
                        requestUrlSource = requestUrlSource + "?" + requestQueryString;
                    }
                    if (requestQueryString != "") {
                        //
                        // Add query string to stream
                        //
                        core.docProperties.addQueryString(requestQueryString);
                    }
                    //
                    // Other Server variables
                    requestReferer = requestReferrer;
                    requestPageReferer = requestReferrer;
                    //
                    if (requestSecure) {
                        requestProtocol = "https://";
                    } else {
                        requestProtocol = "http://";
                    }
                    //
                    core.doc.blockExceptionReporting = false;
                    //
                    //   javascript cookie detect on page1 of all visits
                    string CookieDetectKey = core.docProperties.getText(RequestNameCookieDetectVisitID);
                    if (!string.IsNullOrEmpty(CookieDetectKey)) {
                        //
                        DateTime cookieDetectDate = new DateTime();
                        int CookieDetectVisitId = 0;
                        securityController.decodeToken(core,CookieDetectKey, ref CookieDetectVisitId, ref  cookieDetectDate);
                        if (CookieDetectVisitId != 0) {
                            core.db.executeQuery("update ccvisits set CookieSupport=1 where id=" + CookieDetectVisitId);
                            core.doc.continueProcessing = false; //--- should be disposed by caller --- Call dispose
                            return core.doc.continueProcessing;
                        }
                    }
                    //
                    //   verify Domain table entry
                    bool updateDomainCache = false;
                    //
                    core.domain.name = requestDomain;
                    core.domain.rootPageId = 0;
                    core.domain.noFollow = false;
                    core.domain.typeId = 1;
                    core.domain.visited = false;
                    core.domain.id = 0;
                    core.domain.forwardUrl = ""; 
                    core.domainDictionary = core.cache.getObject<Dictionary<string, domainModel>>("domainContentList");
                    if (core.domainDictionary == null) {
                        //
                        //  no cache found, build domainContentList from database
                        core.domainDictionary = domainModel.createDictionary(core, "(active<>0)and(name is not null)");
                        updateDomainCache = true;
                    }
                    //
                    // verify app config domainlist is in the domainlist cache
                    foreach (string domain in core.appConfig.domainList) {
                        if (!core.domainDictionary.ContainsKey(domain.ToLower())) {
                            var newDomain = domainModel.add(core);
                            newDomain.name = domain;
                            newDomain.rootPageId = 0;
                            newDomain.noFollow = false;
                            newDomain.typeId = 1;
                            newDomain.visited = false;
                            newDomain.forwardUrl = "";
                            newDomain.defaultTemplateId = 0;
                            newDomain.pageNotFoundPageId = 0;
                            newDomain.forwardDomainId = 0;
                            newDomain.defaultRouteId = core.siteProperties.getInteger("");
                            core.domainDictionary.Add(domain.ToLower(), newDomain);
                            updateDomainCache = true;
                        }
                    }
                    //
                    // -- verify request domain
                    if (!core.domainDictionary.ContainsKey(requestDomain.ToLower())) {
                        var newDomain = domainModel.add( core );
                        newDomain.name = requestDomain;
                        newDomain.rootPageId = 0;
                        newDomain.noFollow = false;
                        newDomain.typeId = 1;
                        newDomain.visited = false;
                        newDomain.forwardUrl = "";
                        newDomain.defaultTemplateId = 0;
                        newDomain.pageNotFoundPageId = 0;
                        newDomain.forwardDomainId = 0;
                        newDomain.save(core);
                        core.domainDictionary.Add(requestDomain.ToLower(), newDomain);
                        updateDomainCache = true;
                    }
                    if (updateDomainCache) {
                        //
                        // if there was a change, update the cache
                        //
                        core.cache.setObject("domainContentList", core.domainDictionary, "domains");
                    }
                    //
                    // domain found
                    //
                    core.domain = core.domainDictionary[requestDomain.ToLower()];
                    if (core.domain.id == 0) {
                        //
                        // this is a default domain or a new domain -- add to the domain table
                        var domain = new domainModel() {
                            name = requestDomain,
                            typeId = 1,
                            rootPageId = core.domain.rootPageId,
                            forwardUrl = core.domain.forwardUrl,
                            noFollow = core.domain.noFollow,
                            visited = core.domain.visited,
                            defaultTemplateId = core.domain.defaultTemplateId,
                            pageNotFoundPageId = core.domain.pageNotFoundPageId
                        };
                        core.domain.id = domain.id;
                    }
                    if (!core.domain.visited) {
                        //
                        // set visited true
                        //
                        core.db.executeQuery("update ccdomains set visited=1 where name=" + core.db.encodeSQLText(requestDomain));
                        core.cache.invalidate("domainContentList");
                    }
                    if (core.domain.typeId == 1) {
                        //
                        // normal domain, leave it
                        //
                    } else if (genericController.vbInstr(1, requestPathPage, "/" + core.appConfig.adminRoute, 1) != 0) {
                        //
                        // forwarding does not work in the admin site
                        //
                    } else if ((core.domain.typeId == 2) && (core.domain.forwardUrl != "")) {
                        //
                        // forward to a URL
                        //
                        //
                        //Call AppendLog("main_init(), 1710 - exit for domain forward")
                        //
                        if (genericController.vbInstr(1, core.domain.forwardUrl, "://") == 0) {
                            core.domain.forwardUrl = "http://" + core.domain.forwardUrl;
                        }
                        redirect(core.domain.forwardUrl, "Forwarding to [" + core.domain.forwardUrl + "] because the current domain [" + requestDomain + "] is in the domain content set to forward to this URL", false, false);
                        return core.doc.continueProcessing;
                    } else if ((core.domain.typeId == 3) && (core.domain.forwardDomainId != 0) & (core.domain.forwardDomainId != core.domain.id)) {
                        //
                        // forward to a replacement domain
                        //
                        string forwardDomain = core.db.getRecordName("domains", core.domain.forwardDomainId);
                        if (!string.IsNullOrEmpty(forwardDomain)) {
                            int pos = requestUrlSource.IndexOf( requestDomain ,0,1,StringComparison.CurrentCultureIgnoreCase);
                            if (pos > 0) {
                                core.domain.forwardUrl = requestUrlSource.ToString().Left( pos - 1) + forwardDomain + requestUrlSource.ToString().Substring((pos + requestDomain.Length) - 1);
                                redirect(core.domain.forwardUrl, "Forwarding to [" + core.domain.forwardUrl + "] because the current domain [" + requestDomain + "] is in the domain content set to forward to this replacement domain", false, false);
                                return core.doc.continueProcessing;
                            }
                        }
                    }
                    if (core.domain.noFollow) {
                        response_NoFollow = true;
                    }
                    //
                    requestVirtualFilePath = "/" + core.appConfig.name;
                    //
                    requestContentWatchPrefix = requestProtocol + requestDomain + requestAppRootPath;
                    requestContentWatchPrefix = requestContentWatchPrefix.Left( requestContentWatchPrefix.Length - 1);
                    //
                    requestPath = "/";
                    requestPage = core.siteProperties.serverPageDefault;
                    int TextStartPointer = requestPathPage.ToString().LastIndexOf("/") + 1;
                    if (TextStartPointer != 0) {
                        requestPath = requestPathPage.ToString().Left( TextStartPointer);
                        requestPage = requestPathPage.ToString().Substring(TextStartPointer);
                    }
                    requestSecureURLRoot = "https://" + requestDomain + requestAppRootPath;
                    //
                    // ----- Create Server Link property
                    //
                    requestUrl = requestProtocol + requestDomain + requestAppRootPath + requestPath + requestPage;
                    if (requestQueryString != "") {
                        requestUrl = requestUrl + "?" + requestQueryString;
                    }
                    if (requestUrlSource == "") {
                        requestUrlSource = requestUrl;
                    }
                    //
                    // ----- Style tag
                    adminMessage = "For more information, please contact the <a href=\"mailto:" + core.siteProperties.emailAdmin + "?subject=Re: " + requestDomain + "\">Site Administrator</A>.";
                    //
                    requestUrl = requestProtocol + requestDomain + requestAppRootPath + requestPath + requestPage;
                    if (requestQueryString != "") {
                        requestUrl = requestUrl + "?" + requestQueryString;
                    }
                    //
                    if (requestDomain.ToLower() != genericController.vbLCase(requestDomain)) {
                        string Copy = "Redirecting to domain [" + requestDomain + "] because this site is configured to run on the current domain [" + requestDomain + "]";
                        if (requestQueryString != "") {
                            redirect(requestProtocol + requestDomain + requestPath + requestPage + "?" + requestQueryString, Copy, false, false);
                        } else {
                            redirect(requestProtocol + requestDomain + requestPath + requestPage, Copy, false, false);
                        }
                        core.doc.continueProcessing = false; //--- should be disposed by caller --- Call dispose
                        return core.doc.continueProcessing;
                    }
                    //
                    // ----- Create core.main_ServerFormActionURL if it has not been overridden manually
                    if (serverFormActionURL == "") {
                        serverFormActionURL = requestProtocol + requestDomain + requestPath + requestPage;
                    }
                }
                //
                // -- done at last
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return core.doc.continueProcessing;
        }
        //
        //========================================================================
        // Read a cookie to the stream
        //
        public string getRequestCookie(string CookieName) {
            string cookieValue = "";
            if (requestCookies.ContainsKey(CookieName)) {
                cookieValue = requestCookies[CookieName].value;
            }
            return cookieValue;
        }
        //
        //====================================================================================================
        //
        public void addRequestCookie(string cookieKey, string cookieValue) {
            if (requestCookies.ContainsKey(cookieKey)) {
                requestCookies.Remove(cookieKey);
            }
            requestCookies.Add(cookieKey, new iisController.cookieClass {
                name = cookieKey,
                value = cookieValue
            });
        }
        //
        //========================================================================
        // Write a cookie to the stream
        //========================================================================
        //
        public void addResponseCookie(string cookieName, string iCookieValue, DateTime DateExpires = default(DateTime), string domain = "", string Path = "", bool Secure = false) {
            try {
                string s = null;
                //
                if (core.doc.continueProcessing) {
                    //If core.doc.continueProcessing And core.doc.outputBufferEnabled Then
                    if (false) {
                        //
                        // no domain provided, new mode
                        //   - write cookie for current domains
                        //   - write an iframe that called the cross-Site login
                        //   - http://127.0.0.1/ccLib/clientside/cross.html?v=1&vPath=%2F&vExpires=1%2F1%2F2012
                        //
                        //domainListSplit = Split(core.main_ServerDomainCrossList, ",")
                        //For Ptr = 0 To UBound(domainListSplit)
                        //    domainSet = Trim(domainListSplit[Ptr])
                        //    If (domainSet <> "") And (InStr(1, "," & usedDomainList & ",", "," & domainSet & ",", vbTextCompare) = 0) Then
                        //        usedDomainList = usedDomainList & "," & domainSet
                        //        '
                        //        ' valid, non-repeat domain
                        //        '
                        //        If genericController.vbLCase(domainSet) = genericController.vbLCase(requestDomain) Then
                        //            '
                        //            ' current domain, set cookie
                        //            '
                        //            If (iisContext IsNot Nothing) Then
                        //                '
                        //                ' Pass cookie to asp (compatibility)
                        //                '
                        //                iisContext.Response.Cookies[iCookieName].Value = iCookieValue
                        //                If Not isMinDate(DateExpires) Then
                        //                    iisContext.Response.Cookies[iCookieName].Expires = DateExpires
                        //                End If
                        //                'main_ASPResponse.Cookies[iCookieName].domain = domainSet
                        //                If Not isMissing(Path) Then
                        //                    iisContext.Response.Cookies[iCookieName].Path = genericController.encodeText(Path)
                        //                End If
                        //                If Not isMissing(Secure) Then
                        //                    iisContext.Response.Cookies[iCookieName].Secure = Secure
                        //                End If
                        //            Else
                        //                '
                        //                ' Pass Cookie to non-asp parent
                        //                '   crlf delimited list of name,value,expires,domain,path,secure
                        //                '
                        //                If webServerIO_bufferCookies <> "" Then
                        //                    webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf
                        //                End If
                        //                webServerIO_bufferCookies = webServerIO_bufferCookies & CookieName
                        //                webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf & iCookieValue
                        //                '
                        //                s = ""
                        //                If Not isMinDate(DateExpires) Then
                        //                    s = DateExpires.ToString
                        //                End If
                        //                webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf & s
                        //                ' skip bc this is exactly the current domain and /rfc2109 requires a leading dot if explicit
                        //                webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf
                        //                'responseBufferCookie = responseBufferCookie & vbCrLf & domainSet
                        //                '
                        //                s = "/"
                        //                If Not isMissing(Path) Then
                        //                    s = genericController.encodeText(Path)
                        //                End If
                        //                webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf & s
                        //                '
                        //                s = "false"
                        //                If genericController.EncodeBoolean(Secure) Then
                        //                    s = "true"
                        //                End If
                        //                webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf & s
                        //            End If
                        //        Else
                        //            '
                        //            ' other domain, add iframe
                        //            '
                        //            Dim C As String
                        //            Link = "http://" & domainSet & "/ccLib/clientside/cross.html"
                        //            Link = Link & "?n=" & EncodeRequestVariable(iCookieName)
                        //            Link = Link & "&v=" & EncodeRequestVariable(iCookieValue)
                        //            If Not isMissing(Path) Then
                        //                C = genericController.encodeText(Path)
                        //                C = EncodeRequestVariable(C)
                        //                C = genericController.vbReplace(C, "/", "%2F")
                        //                Link = Link & "&p=" & C
                        //            End If
                        //            If Not isMinDate(DateExpires) Then
                        //                C = genericController.encodeText(DateExpires)
                        //                C = EncodeRequestVariable(C)
                        //                C = genericController.vbReplace(C, "/", "%2F")
                        //                Link = Link & "&e=" & C
                        //            End If
                        //            Link = core.htmlDoc.html_EncodeHTML(Link)
                        //            core.htmlDoc.htmlForEndOfBody = core.htmlDoc.htmlForEndOfBody & vbCrLf & vbTab & "<iframe style=""display:none;"" width=""0"" height=""0"" src=""" & Link & """></iframe>"
                        //        End If
                        //    End If
                        //Next
                    } else {
                        //
                        // Legacy mode - if no domain given just leave it off
                        //
                        if (iisContext != null) {
                            //
                            // Pass cookie to asp (compatibility)
                            //
                            iisContext.Response.Cookies[cookieName].Value = iCookieValue;
                            if (!isMinDate(DateExpires)) {
                                iisContext.Response.Cookies[cookieName].Expires = DateExpires;
                            }
                            //main_ASPResponse.Cookies[iCookieName].domain = domainSet
                            if (!isMissing(Path)) {
                                iisContext.Response.Cookies[cookieName].Path = genericController.encodeText(Path);
                            }
                            if (!isMissing(Secure)) {
                                iisContext.Response.Cookies[cookieName].Secure = Secure;
                            }
                        } else {
                            //
                            // Pass Cookie to non-asp parent
                            //   crlf delimited list of name,value,expires,domain,path,secure
                            //
                            if (bufferCookies != "") {
                                bufferCookies = bufferCookies + "\r\n";
                            }
                            bufferCookies = bufferCookies + cookieName;
                            bufferCookies = bufferCookies + "\r\n" + iCookieValue;
                            //
                            s = "";
                            if (!isMinDate(DateExpires)) {
                                s = DateExpires.ToString();
                            }
                            bufferCookies = bufferCookies + "\r\n" + s;
                            //
                            s = "";
                            if (!isMissing(domain)) {
                                s = genericController.encodeText(domain);
                            }
                            bufferCookies = bufferCookies + "\r\n" + s;
                            //
                            s = "/";
                            if (!isMissing(Path)) {
                                s = genericController.encodeText(Path);
                            }
                            bufferCookies = bufferCookies + "\r\n" + s;
                            //
                            s = "false";
                            if (genericController.encodeBoolean(Secure)) {
                                s = "true";
                            }
                            bufferCookies = bufferCookies + "\r\n" + s;
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //
        //
        public void setResponseStatus(string status) {
            bufferResponseStatus = status;
        }
        //
        //
        //
        public void setResponseContentType(object ContentType) {
            bufferContentType = encodeText(ContentType);
        }
        //
        //
        //
        public void addResponseHeader(object HeaderName, object HeaderValue) {
            try {
                //
                if (core.doc.continueProcessing) {
                    if (bufferResponseHeader != "") {
                        bufferResponseHeader = bufferResponseHeader + "\r\n";
                    }
                    bufferResponseHeader = bufferResponseHeader + genericController.vbReplace(genericController.encodeText(HeaderName), "\r\n", "") + "\r\n" + genericController.vbReplace(genericController.encodeText(HeaderValue), "\r\n", "");
                }
                //
                return;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError18("main_SetStreamHeader")
                                                                    //
        }
        //
        //===========================================================================================
        /// <summary>
        /// redirect
        /// </summary>
        /// <param name="NonEncodedLink"></param>
        /// <param name="RedirectReason"></param>
        /// <param name="IsPageNotFound"></param>
        /// <param name="allowDebugMessage">If true, when visit property debugging is enabled, the routine returns </param>
        public string redirect(string NonEncodedLink, string RedirectReason = "No explaination provided", bool IsPageNotFound = false, bool allowDebugMessage = true) {
            string result = "";
            try {
                const string rnRedirectCycleFlag = "cycleFlag";
                string EncodedLink = null;
                string ShortLink = "";
                string FullLink = null;
                int redirectCycles = 0;
                //
                if (core.doc.continueProcessing) {
                    redirectCycles = core.docProperties.getInteger(rnRedirectCycleFlag);
                    //
                    // convert link to a long link on this domain
                    if (NonEncodedLink.Left( 4).ToLower() == "http") {
                        FullLink = NonEncodedLink;
                    } else {
                        if (NonEncodedLink.Left( 1).ToLower() == "/") {
                            //
                            // -- root relative - url starts with path, let it go
                        } else if (NonEncodedLink.Left( 1).ToLower() == "?") {
                            //
                            // -- starts with qs, fix issue where iis consideres this on the physical page, not the link-alias vitrual route
                            NonEncodedLink = requestPathPage + NonEncodedLink;
                        } else {
                            //
                            // -- url starts with the page
                            NonEncodedLink = requestPath + NonEncodedLink;
                        }
                        ShortLink = NonEncodedLink;
                        ShortLink = genericController.ConvertLinkToShortLink(ShortLink, requestDomain, requestVirtualFilePath);
                        ShortLink = genericController.EncodeAppRootPath(ShortLink, requestVirtualFilePath, requestAppRootPath, requestDomain);
                        FullLink = requestProtocol + requestDomain + ShortLink;
                    }

                    if (string.IsNullOrEmpty(NonEncodedLink)) {
                        //
                        // Link is not valid
                        //
                        core.handleException(new ApplicationException("Redirect was called with a blank Link. Redirect Reason [" + RedirectReason + "]"));
                        return string.Empty;
                        //
                        // changed to main_ServerLinksource because if a redirect is caused by a link forward, and the host page for the iis 404 is
                        // the same as the destination of the link forward, this throws an error and does not forward. the only case where main_ServerLinksource is different
                        // then main_ServerLink is the linkfforward/linkalias case.
                        //
                    } else if ((requestFormDict.Count == 0) && (requestUrlSource == FullLink)) {
                        //
                        // Loop redirect error, throw trap and block redirect to prevent loop
                        //
                        core.handleException(new ApplicationException("Redirect was called to the same URL, main_ServerLink is [" + requestUrl + "], main_ServerLinkSource is [" + requestUrlSource + "]. This redirect is only allowed if either the form or querystring has change to prevent cyclic redirects. Redirect Reason [" + RedirectReason + "]"));
                        return string.Empty;
                    } else if (IsPageNotFound) {
                        //
                        // Do a PageNotFound then redirect
                        //
                        logController.addSiteWarning(core, "Page Not Found Redirect", "Page Not Found Redirect", "", 0, "Page Not Found Redirect [" + requestUrlSource + "]", "Page Not Found Redirect", "Page Not Found Redirect");
                        if (!string.IsNullOrEmpty(ShortLink)) {
                            core.db.executeQuery("Update ccContentWatch set link=null where link=" + core.db.encodeSQLText(ShortLink));
                        }
                        //
                        if (allowDebugMessage && core.doc.visitPropertyAllowDebugging) {
                            //
                            // -- Verbose - do not redirect, just print the link
                            EncodedLink = NonEncodedLink;
                        } else {
                            setResponseStatus("404 Not Found");
                        }
                    } else {

                        //
                        // Go ahead and redirect
                        //
                        if (allowDebugMessage && core.doc.visitPropertyAllowDebugging) {
                            //
                            // -- Verbose - do not redirect, just print the link
                            EncodedLink = NonEncodedLink;
                            result = "<div style=\"padding:20px;border:1px dashed black;background-color:white;color:black;\">" + RedirectReason + "<p>Click to continue the redirect to <a href=" + EncodedLink + ">" + genericController.encodeHTML(NonEncodedLink) + "</a>...</p></div>";
                        } else {
                            //
                            // -- Redirect now
                            clearResponseBuffer();
                            if (iisContext != null) {
                                //
                                // -- redirect and release application. HOWEVER -- the thread will continue so use responseOpen=false to abort as much activity as possible
                                iisContext.Response.Redirect(NonEncodedLink, false);
                                iisContext.ApplicationInstance.CompleteRequest();
                            } else {
                                bufferRedirect = NonEncodedLink;
                            }
                        }
                    }
                    //
                    // -- close the output stream
                    core.doc.continueProcessing = false;
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }

        //
        //
        public void flushStream() {
            if (iisContext != null) {
                iisContext.Response.Flush();
            }
        }
        //
        //====================================================================================================
        //
        //Private Structure fieldTypePrivate
        //    Dim Name As String
        //    Dim fieldTypePrivate As Integer
        //End Structure
        //
        //====================================================================================================
        /// <summary>
        /// Verify a site exists, it not add it, it is does, verify all its settings
        /// </summary>
        /// <param name="core"></param>
        /// <param name="appName"></param>
        /// <param name="DomainName"></param>
        /// <param name="rootPublicFilesPath"></param>
        /// <param name="defaultDocOrBlank"></param>
        /// '
        public static void verifySite(coreController core, string appName, string DomainName, string rootPublicFilesPath, string defaultDocOrBlank) {
            try {
                verifyAppPool(core, appName);
                verifyWebsite(core, appName, DomainName, rootPublicFilesPath, appName);
            } catch (Exception ex) {
                core.handleException(ex, "verifySite");
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// verify the application pool. If it exists, update it. If not, create it
        /// </summary>
        /// <param name="core"></param>
        /// <param name="poolName"></param>
        public static void verifyAppPool(coreController core, string poolName) {
            try {
                using (ServerManager serverManager = new ServerManager()) {
                    bool poolFound = false;
                    ApplicationPool appPool = null;
                    foreach (ApplicationPool appPoolWithinLoop in serverManager.ApplicationPools) {
                        appPool = appPoolWithinLoop;
                        if (appPoolWithinLoop.Name == poolName) {
                            poolFound = true;
                            break;
                        }
                    }
                    if (!poolFound) {
                        appPool = serverManager.ApplicationPools.Add(poolName);
                    } else {
                        appPool = serverManager.ApplicationPools[poolName];
                    }
                    appPool.ManagedRuntimeVersion = "v4.0";
                    appPool.Enable32BitAppOnWin64 = true;
                    appPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
                    serverManager.CommitChanges();
                }
            } catch (Exception ex) {
                core.handleException(ex, "verifyAppPool");
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// verify the website. If it exists, update it. If not, create it
        /// </summary>
        /// <param name="core"></param>
        /// <param name="appName"></param>
        /// <param name="domainName"></param>
        /// <param name="phyPath"></param>
        /// <param name="appPool"></param>
        private static void verifyWebsite(coreController core, string appName, string domainName, string phyPath, string appPool) {
            try {

                using (ServerManager iisManager = new ServerManager()) {
                    Site site = null;
                    bool found = false;
                    //
                    // -- verify the site exists
                    foreach (Site siteWithinLoop in iisManager.Sites) {
                        site = siteWithinLoop;
                        if (siteWithinLoop.Name.ToLower() == appName.ToLower()) {
                            found = true;
                            break;
                        }
                    }
                    if (!found) {
                        iisManager.Sites.Add(appName, "http", "*:80:" + appName, phyPath);
                    }
                    site = iisManager.Sites[appName];
                    //
                    // -- verify the domain binding
                    //verifyWebsite_Binding(core, site, "*:80:" + appName, "http");
                    verifyWebsite_Binding(core, site, "*:80:" + domainName, "http");
                    //
                    // -- verify the application pool
                    site.ApplicationDefaults.ApplicationPoolName = appPool;
                    foreach (Application iisApp in site.Applications) {
                        iisApp.ApplicationPoolName = appPool;
                    }
                    //
                    // -- verify the cdn virtual directory (if configured)
                    string cdnFilesPrefix = core.appConfig.cdnFileUrl;
                    if (cdnFilesPrefix.IndexOf("://") < 0) {
                        verifyWebsite_VirtualDirectory(core, site, appName, cdnFilesPrefix, core.appConfig.localFilesPath);
                    }
                    //
                    // -- commit any changes
                    iisManager.CommitChanges();
                }
            } catch (Exception ex) {
                core.handleException(ex, "verifyWebsite");
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Verify the binding
        /// </summary>
        /// <param name="core"></param>
        /// <param name="site"></param>
        /// <param name="bindingInformation"></param>
        /// <param name="bindingProtocol"></param>
        private static void verifyWebsite_Binding(coreController core, Site site, string bindingInformation, string bindingProtocol) {
            try {
                using (ServerManager iisManager = new ServerManager()) {
                    Binding binding = null;
                    bool found = false;
                    found = false;
                    foreach (Binding bindingWithinLoop in site.Bindings) {
                        binding = bindingWithinLoop;
                        if ((bindingWithinLoop.BindingInformation == bindingInformation) && (bindingWithinLoop.Protocol == bindingProtocol)) {
                            found = true;
                            break;
                        }
                    }
                    if (!found) {
                        binding = site.Bindings.CreateElement();
                        binding.BindingInformation = bindingInformation;
                        binding.Protocol = bindingProtocol;
                        site.Bindings.Add(binding);
                        iisManager.CommitChanges();
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex, "verifyWebsite_Binding");
            }
        }
        //
        //====================================================================================================
        //
        private static void verifyWebsite_VirtualDirectory(coreController core, Site site, string appName, string virtualFolder, string physicalPath) {
            try {
                bool found = false;
                foreach ( Application iisApp in site.Applications) {
                    if (iisApp.ApplicationPoolName.ToLower() == appName.ToLower()) {
                        foreach (VirtualDirectory virtualDirectory in iisApp.VirtualDirectories) {
                            if (virtualDirectory.Path == virtualFolder) {
                                found = true;
                                break;
                            }
                        }
                        if (!found) {
                            List<string> vpList = virtualFolder.Split('/').ToList();
                            string newDirectoryPath = "";

                            foreach (string newDirectoryFolderName in vpList) {
                                if (!string.IsNullOrEmpty(newDirectoryFolderName)) {
                                    newDirectoryPath += "/" + newDirectoryFolderName;
                                    bool directoryFound = false;
                                    foreach (VirtualDirectory currentDirectory in iisApp.VirtualDirectories) {
                                        if (currentDirectory.Path.ToLower() == newDirectoryPath.ToLower()) {
                                            directoryFound = true;
                                            break;
                                        }
                                    }
                                    if (!directoryFound) {
                                        iisApp.VirtualDirectories.Add(newDirectoryPath, physicalPath);
                                    }
                                }
                            }
                        }
                    }
                    if (found) {
                        break;
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex, "verifyWebsite_VirtualDirectory");
            }
        }
        //========================================================================
        // main_RedirectByRecord( iContentName, iRecordID )
        //   looks up the record
        //   increments the 'clicks' field and redirects to the 'link' field
        //   returns true if the redirect happened OK
        //========================================================================
        //
        public static bool main_RedirectByRecord_ReturnStatus(coreController core, string ContentName, int RecordID, string FieldName = "") {
            bool tempmain_RedirectByRecord_ReturnStatus = false;
            int CSPointer = 0;
            string MethodName = null;
            int ContentID = 0;
            int CSHost = 0;
            string HostContentName = null;
            int HostRecordID = 0;
            bool BlockRedirect = false;
            string iContentName = null;
            int iRecordID = 0;
            string iFieldName = null;
            string LinkPrefix = "";
            string EncodedLink = null;
            string NonEncodedLink = "";
            //
            iContentName = genericController.encodeText(ContentName);
            iRecordID = genericController.encodeInteger(RecordID);
            iFieldName = genericController.encodeEmptyText(FieldName, "link");
            //
            MethodName = "main_RedirectByRecord_ReturnStatus( " + iContentName + ", " + iRecordID + ", " + genericController.encodeEmptyText(FieldName, "(fieldname empty)") + ")";
            //
            tempmain_RedirectByRecord_ReturnStatus = false;
            BlockRedirect = false;
            CSPointer = core.db.csOpen(iContentName, "ID=" + iRecordID);
            if (core.db.csOk(CSPointer)) {
                // 2/18/2008 - EncodeLink change
                //
                // Assume all Link fields are already encoded -- as this is how they would appear if the admin cut and pasted
                //
                EncodedLink = encodeText(core.db.csGetText(CSPointer, iFieldName)).Trim(' ');
                if (string.IsNullOrEmpty(EncodedLink)) {
                    BlockRedirect = true;
                } else {
                    //
                    // ----- handle content special cases (prevent redirect to deleted records)
                    //
                    NonEncodedLink = genericController.DecodeResponseVariable(EncodedLink);
                    switch (genericController.vbUCase(iContentName)) {
                        case "CONTENT WATCH":
                            //
                            // ----- special case
                            //       if this is a content watch record, check the underlying content for
                            //       inactive or expired before redirecting
                            //
                            LinkPrefix = core.webServer.requestContentWatchPrefix;
                            ContentID = (core.db.csGetInteger(CSPointer, "ContentID"));
                            HostContentName = Models.Complex.cdefModel.getContentNameByID(core, ContentID);
                            if (string.IsNullOrEmpty(HostContentName)) {
                                //
                                // ----- Content Watch with a bad ContentID, mark inactive
                                //
                                BlockRedirect = true;
                                core.db.csSet(CSPointer, "active", 0);
                            } else {
                                HostRecordID = (core.db.csGetInteger(CSPointer, "RecordID"));
                                if (HostRecordID == 0) {
                                    //
                                    // ----- Content Watch with a bad iRecordID, mark inactive
                                    //
                                    BlockRedirect = true;
                                    core.db.csSet(CSPointer, "active", 0);
                                } else {
                                    CSHost = core.db.csOpen(HostContentName, "ID=" + HostRecordID);
                                    if (!core.db.csOk(CSHost)) {
                                        //
                                        // ----- Content Watch host record not found, mark inactive
                                        //
                                        BlockRedirect = true;
                                        core.db.csSet(CSPointer, "active", 0);
                                    }
                                }
                                core.db.csClose(ref CSHost);
                            }
                            if (BlockRedirect) {
                                //
                                // ----- if a content watch record is blocked, delete the content tracking
                                //
                                core.db.deleteContentRules(Models.Complex.cdefModel.getContentId(core, HostContentName), HostRecordID);
                            }
                            break;
                    }
                }
                if (!BlockRedirect) {
                    //
                    // If link incorrectly includes the LinkPrefix, take it off first, then add it back
                    //
                    NonEncodedLink = genericController.ConvertShortLinkToLink(NonEncodedLink, LinkPrefix);
                    if (core.db.csIsFieldSupported(CSPointer, "Clicks")) {
                        core.db.csSet(CSPointer, "Clicks", (core.db.csGetNumber(CSPointer, "Clicks")) + 1);
                    }
                    core.webServer.redirect(LinkPrefix + NonEncodedLink, "Call to " + MethodName + ", no reason given.", false, false);
                    tempmain_RedirectByRecord_ReturnStatus = true;
                }
            }
            core.db.csClose(ref CSPointer);
            return tempmain_RedirectByRecord_ReturnStatus;
        }
        //
        //========================================================================
        //
        public static string getBrowserAcceptLanguage(coreController core) {
            try {
                string AcceptLanguageString = genericController.encodeText(core.webServer.requestLanguage) + ",";
                int CommaPosition = genericController.vbInstr(1, AcceptLanguageString, ",");
                while (CommaPosition != 0) {
                    string AcceptLanguage = (AcceptLanguageString.Left( CommaPosition - 1)).Trim(' ');
                    AcceptLanguageString = AcceptLanguageString.Substring(CommaPosition);
                    if (AcceptLanguage.Length > 0) {
                        int DashPosition = genericController.vbInstr(1, AcceptLanguage, "-");
                        if (DashPosition > 1) {
                            AcceptLanguage = AcceptLanguage.Left( DashPosition - 1);
                        }
                        DashPosition = genericController.vbInstr(1, AcceptLanguage, ";");
                        if (DashPosition > 1) {
                            return AcceptLanguage.Left( DashPosition - 1);
                        }
                    }
                    CommaPosition = genericController.vbInstr(1, AcceptLanguageString, ",");
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return "";
        }
        public void clearResponseBuffer() {
            bufferRedirect = "";
            bufferResponseHeader = "";
        }
    }
}