﻿
using System;
using System.Linq;
using System.Collections.Generic;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
using Microsoft.Web.Administration;
using Contensive.Processor.Exceptions;
using Contensive.Models.Db;
using System.Threading.Tasks;
//
namespace Contensive.Processor.Controllers {
    /// <summary>
    /// Code dedicated to processing iis input and output. lazy Constructed. (see coreHtmlClass for html processing)
    /// What belongs here is everything that would have to change if we converted to apache
    /// </summary>
    public class WebServerController {
        //
        // enum this, not consts --  https://en.wikipedia.org/wiki/List_of_HTTP_status_codes
        public const string httpResponseStatus200_Success = "200 OK";
        public const string httpResponseStatus401_Unauthorized = "401 Unauthorized";
        public const string httpResponseStatus403_Forbidden = "403 Forbidden";
        public const string httpResponseStatus404_NotFound = "404 Not Found";
        public const string httpResponseStatus500_ServerError = "500 Internal Server Error";
        //
        private CoreController core;
        //
        /// <summary>
        /// if this instance is a webRole, retain pointer for callbacks
        /// </summary>
        public System.Web.HttpContext iisContext;
        //
        // todo - create request domain model with constructor for both web-driven and non-web-driven environments
        //
        // ====================================================================================================
        /// <summary>
        /// request port
        /// </summary>
        public int requestPort {
            get {
                if (_requestPort == null) {
                    _requestPort = ((iisContext != null) && (iisContext.Request != null) && (iisContext.Request.Url != null)) ? iisContext.Request.Url.Port : 0;
                }
                return (int)_requestPort;
            }
        }
        private int? _requestPort = null;
        //
        // ====================================================================================================
        /// <summary>
        /// The path and page of the current request, without the leading slash which comes from the appRootPath
        /// </summary>
        public string requestPathPage {
            get {
                if (string.IsNullOrEmpty(_requestPathPage)) {
                    _requestPathPage = (core.webServer.serverEnvironment.ContainsKey("SCRIPT_NAME")) ? core.webServer.serverEnvironment["SCRIPT_NAME"] : "";
                }
                return _requestPathPage;
            }
        }
        private string _requestPathPage = null;
        //
        // ====================================================================================================
        // todo convert request variables on-demand pattern.
        /// <summary>
        /// The refering URL
        /// </summary>
        public string requestReferrer {
            get {
                return (core.webServer.serverEnvironment.ContainsKey("HTTP_REFERER")) ? core.webServer.serverEnvironment["HTTP_REFERER"] : "";
            }
        }
        //
        // ====================================================================================================
        /// <summary>
        /// The domain part of the current request URL
        /// </summary>
        public string requestDomain {
            get {
                return (core.webServer.serverEnvironment.ContainsKey("SERVER_NAME")) ? core.webServer.serverEnvironment["SERVER_NAME"] : "";
            }
        }
        //
        // ====================================================================================================
        /// <summary>
        /// true if the current request is secure
        /// </summary>
        public bool requestSecure {
            get {
                if (_requestSecure == null) {
                    _requestSecure = (core.webServer.serverEnvironment.ContainsKey("SERVER_PORT_SECURE")) ? encodeBoolean(core.webServer.serverEnvironment["SERVER_PORT_SECURE"]) : false;
                }
                return (bool)_requestSecure;
            }
        }
        private bool? _requestSecure = null;
        //
        // ====================================================================================================
        /// <summary>
        /// Legacy property - user's IP
        /// </summary>
        public string requestRemoteIP {
            get {
                return (core.webServer.serverEnvironment.ContainsKey("REMOTE_ADDR")) ? core.webServer.serverEnvironment["REMOTE_ADDR"] : "";
            }
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Legacy property - the browser
        /// </summary>
        public string requestBrowser {
            get {
                return (core.webServer.serverEnvironment.ContainsKey("HTTP_USER_AGENT")) ? core.webServer.serverEnvironment["HTTP_USER_AGENT"] : "";
            }
        }
        //
        // ====================================================================================================
        /// <summary>
        /// The QueryString of the current URI
        /// </summary>
        public string requestQueryString { get; set; } = "";
        //
        // ====================================================================================================
        /// <summary>
        /// The url as it was requested. if the url is a route, this is the original route call and requestUrl is the path it is routed to.
        /// </summary>
        public string requestUrlSource {
            get {
                if (_requestUrlSource == null) {
                    _requestUrlSource = iisContext.Request.Url.AbsoluteUri;
                }
                return _requestUrlSource;
            }
        }
        private string _requestUrlSource = null;
        //
        // ====================================================================================================
        //
        public string linkForwardSource { get; set; } = ""; // main_ServerPathPage -- set during init
        //
        // ====================================================================================================
        //
        public string linkForwardError { get; set; } = ""; // always 404
        //
        // ====================================================================================================
        //
        public bool pageExcludeFromAnalytics { get; set; } = false; // For this page - true for remote methods and ajax
        //
        // ====================================================================================================
        //
        public int memberAction { get; set; } = 0; // action to be performed during init
        //
        // ====================================================================================================
        //
        public string adminMessage { get; set; } = ""; // For more information message
        //
        // ====================================================================================================
        //
        public string requestPageReferer { get; set; } = ""; // replaced by main_ServerReferrer
        //
        // ====================================================================================================
        //
        public string requestReferer { get; set; } = "";
        //
        // ====================================================================================================
        /// <summary>
        /// The Action for all internal forms, if not set, default
        /// </summary>
        public string serverFormActionURL {
            get {
                if (string.IsNullOrEmpty(_serverFormActionURL)) {
                    _serverFormActionURL = requestUrl;
                }
                return _serverFormActionURL;
            }
            set {
                _serverFormActionURL = value;
            }
        }
        private string _serverFormActionURL = null;
        //
        // ====================================================================================================
        //
        public string requestContentWatchPrefix { get; set; } = ""; // The different between the URL and the main_ContentWatch Pathpage
        //
        // ====================================================================================================
        /// <summary>
        /// The protocol used in the current quest
        /// </summary>
        public string requestProtocol {
            get {
                if (requestSecure) return "https://";
                return "http://";
            }
        }
        //
        // ====================================================================================================
        /// <summary>
        /// The executed script. If the url is a route, requestUrlSource is the route and requestUrl is the script page that was run
        /// </summary>
        public string requestUrl {
            get {
                if (string.IsNullOrEmpty(_requestUrl)) {
                    _requestUrl = requestProtocol
                        + requestDomain
                        + (requestSecure && requestPort.Equals(443) ? "" : ((!requestSecure) && requestPort.Equals(80) ? "" : (":" + requestPort)))
                        + requestPath
                        + requestPage
                        + ((string.IsNullOrWhiteSpace(requestQueryString)) ? "" : "?" + requestQueryString);
                }
                return _requestUrl;
            }
        }
        private string _requestUrl = null;
        //
        // ====================================================================================================
        /// <summary>
        /// The path between the requestDomain and the requestPage. NOTE - breaking change: this used to follow appRootPath and never started with /
        /// </summary>
        public string requestPath { get; set; } = "";
        //
        // ====================================================================================================
        /// <summary>
        /// The page or script name, typicall index.html or default.aspx or myPage.aspx
        /// </summary>
        public string requestPage { get; set; } = "";
        //
        // ====================================================================================================
        /// <summary>
        /// The URL to the root of the secure area for this site
        /// </summary>
        public string requestSecureURLRoot { get; set; } = "";
        //
        // ====================================================================================================
        /// <summary>
        /// when set, Meta no follow is added
        /// </summary>
        public bool response_NoFollow { get; set; } = false;
        //
        // ====================================================================================================
        //
        public string bufferRedirect { get; set; } = "";
        //
        // ====================================================================================================
        //
        public string bufferContentType { get; set; } = "";
        //
        // ====================================================================================================
        //
        public string bufferCookies { get; set; } = "";
        //
        // ====================================================================================================
        //
        public string bufferResponseHeader { get; set; } = "";
        //
        // ====================================================================================================
        //
        public string bufferResponseStatus { get; set; } = "";
        //
        // ====================================================================================================
        //
        //   QueryString, Form and cookie Processing variables
        public class CookieClass {
            public string name;
            public string value;
        }
        public Dictionary<string, CookieClass> requestCookies;
        //
        // ====================================================================================================
        /// <summary>
        /// The body of the request
        /// </summary>
        public string requestBody;
        //
        // ====================================================================================================
        /// <summary>
        /// The content type of the request
        /// </summary>
        public string requestContentType;
        //
        // ====================================================================================================
        //
        public Dictionary<string, string> requestHeaders { get; set; } = new Dictionary<string, string>();
        //
        // ====================================================================================================
        //
        public Dictionary<string, string> requestForm { get; set; } = new Dictionary<string, string>();
        //
        // ====================================================================================================
        //
        public Dictionary<string, string> requestQuery { get; set; } = new Dictionary<string, string>();
        //
        // ====================================================================================================
        //
        public Dictionary<string, string> serverEnvironment { get; set; } = new Dictionary<string, string>();
        //
        // ====================================================================================================
        //
        public WebServerController(CoreController core) : base() {
            this.core = core;
            requestCookies = new Dictionary<string, CookieClass>();
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
                LogFilename = "Temp\\" + GenericController.encodeText(GenericController.GetRandomInteger(core)) + ".Log";
                Cmd = "IISReset.exe";
                arg = "/restart >> \"" + LogFilename + "\"";
                runProcess(core, Cmd, arg, true);
                Copy = core.privateFiles.readFileText(LogFilename);
                core.privateFiles.deleteFile(LogFilename);
                Copy = GenericController.vbReplace(Copy, Environment.NewLine, "\\n");
                Copy = GenericController.vbReplace(Copy, "\r", "\\n");
                Copy = GenericController.vbReplace(Copy, "\n", "\\n");
            } catch (Exception ex) {
                LogController.logError(core, ex);
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
                LogFilename = "Temp\\" + GenericController.encodeText(GenericController.GetRandomInteger(core)) + ".Log";
                Cmd = "%comspec% /c IISReset /stop >> \"" + LogFilename + "\"";
                runProcess(core, Cmd, "", true);
                Copy = core.privateFiles.readFileText(LogFilename);
                core.privateFiles.deleteFile(LogFilename);
                Copy = GenericController.vbReplace(Copy, Environment.NewLine, "\\n");
                Copy = GenericController.vbReplace(Copy, "\r", "\\n");
                Copy = GenericController.vbReplace(Copy, "\n", "\\n");
            } catch (Exception ex) {
                LogController.logError(core, ex);
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
                Copy = GenericController.vbReplace(Copy, Environment.NewLine, "\\n");
                Copy = GenericController.vbReplace(Copy, "\r", "\\n");
                Copy = GenericController.vbReplace(Copy, "\n", "\\n");
            } catch (Exception ex) {
                LogController.logError(core, ex);
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
                    if (appPool.Name.ToLowerInvariant() == appName.ToLowerInvariant()) {
                        if (appPool.Start() == ObjectState.Started) {
                            appPool.Recycle();
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
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
                // -- argument validation
                if ((httpContext == null) || (httpContext.Request == null) || (httpContext.Response == null)) {
                    Controllers.LogController.logWarn(core, new GenericException("Attempt to initialize webContext but iisContext or one of its objects is null."));
                    return false;
                }
                //
                // -- setup IIS default Response
                iisContext = httpContext;
                iisContext.Response.CacheControl = "no-cache";
                iisContext.Response.Expires = -1;
                iisContext.Response.Buffer = true;
                //
                // todo convert this to lazy read from stored iisContext
                // -- read the request body into requestbody
                httpContext.Request.InputStream.Position = 0;
                System.IO.StreamReader str = new System.IO.StreamReader(httpContext.Request.InputStream);
                requestBody = str.ReadToEnd();
                //
                requestContentType = httpContext.Request.ContentType;
                //
                //
                // todo consider dprecating custom and use docProperties
                // custom server variables
                //
                // -- server variables
                {
                    System.Collections.Specialized.NameValueCollection nameValues = iisContext.Request.ServerVariables;
                    for (int i = 0; i < nameValues.Count; i++) {
                        string key = nameValues.GetKey(i);
                        if (serverEnvironment.ContainsKey(key)) { serverEnvironment.Remove(key); }
                        serverEnvironment.Add(nameValues.GetKey(i), nameValues.Get(i));
                        core.docProperties.setProperty(nameValues.GetKey(i), nameValues.Get(i), DocPropertyController.DocPropertyTypesEnum.serverVariable);
                    }
                }
                //
                // -- headers
                {
                    System.Collections.Specialized.NameValueCollection nameValues = iisContext.Request.Headers;
                    for (int i = 0; i < nameValues.Count; i++) {
                        string key = nameValues.GetKey(i);
                        if (string.IsNullOrWhiteSpace(key)) continue;
                        if (requestHeaders.ContainsKey(key)) { requestForm.Remove(key); }
                        requestHeaders.Add(key, nameValues.Get(i));
                        core.docProperties.setProperty(key, nameValues.Get(i), DocPropertyController.DocPropertyTypesEnum.header);
                    }
                }
                //
                // -- queryString
                {
                    if (iisContext.Request.QueryString.Count > 0) {
                        requestQueryString = "";
                        foreach (string key in iisContext.Request.QueryString) {
                            if (string.IsNullOrWhiteSpace(key)) continue;
                            string keyValue = iisContext.Request.QueryString[key];
                            if (requestQuery.ContainsKey(key)) { requestQuery.Remove(key); }
                            requestQuery.Add(key, keyValue);
                            core.docProperties.setProperty(key, keyValue, DocPropertyController.DocPropertyTypesEnum.queryString);
                            requestQueryString = GenericController.modifyQueryString(requestQueryString, key, keyValue);
                        }
                    }
                }
                //
                // -- form
                {
                    foreach (string key in iisContext.Request.Form.Keys) {
                        if (string.IsNullOrWhiteSpace(key)) continue;
                        string keyValue = iisContext.Request.Form[key];
                        if (requestForm.ContainsKey(key)) { requestForm.Remove(key); }
                        requestForm.Add(key, keyValue);
                        core.docProperties.setProperty(key, keyValue, DocPropertyController.DocPropertyTypesEnum.form);
                    }
                }
                //
                // -- files
                {
                    int filePtr = 0;
                    string instanceId = GenericController.getGUIDNaked();
                    foreach (string key in iisContext.Request.Files.AllKeys) {
                        if (string.IsNullOrWhiteSpace(key)) continue;
                        System.Web.HttpPostedFile file = iisContext.Request.Files[key];
                        if (file != null) {
                            if ((file.ContentLength > 0) && (!string.IsNullOrEmpty(file.FileName))) {
                                DocPropertiesClass prop = new DocPropertiesClass {
                                    Name = key,
                                    Value = file.FileName,
                                    NameValue = encodeRequestVariable(key) + "=" + encodeRequestVariable(file.FileName),
                                    tempfilename = instanceId + "-" + filePtr.ToString() + ".bin",
                                    propertyType = DocPropertyController.DocPropertyTypesEnum.file
                                };
                                core.tempFiles.verifyPath(core.tempFiles.localAbsRootPath);
                                file.SaveAs(core.tempFiles.joinPath(core.tempFiles.localAbsRootPath, prop.tempfilename));
                                core.tempFiles.deleteOnDisposeFileList.Add(prop.tempfilename);
                                prop.FileSize = encodeInteger(file.ContentLength);
                                core.docProperties.setProperty(key, prop);
                                filePtr += 1;
                            }
                        }
                    }
                }
                //
                // -- cookies
                {
                    foreach (string key in iisContext.Request.Cookies) {
                        if (string.IsNullOrWhiteSpace(key)) continue;
                        string keyValue = iisContext.Request.Cookies[key].Value;
                        keyValue = decodeResponseVariable(keyValue);
                        addRequestCookie(key, keyValue);
                    }
                }
                //
                //--------------------------------------------------------------------------
                //
                if (core.appConfig.appStatus != AppConfigModel.AppStatusEnum.ok) {
                    //
                    // did not initialize correctly
                    //
                } else {
                    //
                    // continue
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
                    // Other Server variables
                    requestReferer = requestReferrer;
                    requestPageReferer = requestReferrer;
                    //
                    core.doc.blockExceptionReporting = false;
                    //
                    //   javascript cookie detect on page1 of all visits
                    string CookieDetectKey = core.docProperties.getText(RequestNameCookieDetectVisitId);
                    if (!string.IsNullOrEmpty(CookieDetectKey)) {
                        //
                        SecurityController.TokenData visitToken = SecurityController.decodeToken(core, CookieDetectKey);
                        if (visitToken.id != 0) {
                            string sql = "update ccvisits set CookieSupport=1 where id=" + visitToken.id;
                            Task.Run(() => core.db.executeNonQueryAsync(sql));
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
                    core.domainDictionary = core.cache.getObject<Dictionary<string, DomainModel>>("domainContentList");
                    if (core.domainDictionary == null) {
                        //
                        //  no cache found, build domainContentList from database
                        core.domainDictionary = DomainModel.createDictionary(core.cpParent, "(active<>0)and(name is not null)");
                        updateDomainCache = true;
                    }
                    //
                    // verify app config domainlist is in the domainlist cache
                    foreach (string domain in core.appConfig.domainList) {
                        if (!core.domainDictionary.ContainsKey(domain.ToLowerInvariant())) {
                            LogController.logTrace(core, "adding domain record because configList domain not found [" + domain.ToLowerInvariant() + "]");
                            var newDomain = DomainModel.addEmpty<DomainModel>(core.cpParent, 0);
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
                            newDomain.save(core.cpParent, 0);
                            core.domainDictionary.Add(domain.ToLowerInvariant(), newDomain);
                            updateDomainCache = true;
                        }
                    }
                    //
                    // -- verify request domain
                    if (!core.domainDictionary.ContainsKey(requestDomain.ToLowerInvariant())) {
                        LogController.logTrace(core, "adding domain record because requestDomain [" + requestDomain.ToLowerInvariant() + "] not found");
                        var newDomain = DomainModel.addEmpty<DomainModel>(core.cpParent, 0);
                        newDomain.name = requestDomain;
                        newDomain.rootPageId = 0;
                        newDomain.noFollow = false;
                        newDomain.typeId = 1;
                        newDomain.visited = false;
                        newDomain.forwardUrl = "";
                        newDomain.defaultTemplateId = 0;
                        newDomain.pageNotFoundPageId = 0;
                        newDomain.forwardDomainId = 0;
                        newDomain.save(core.cpParent, 0);
                        core.domainDictionary.Add(requestDomain.ToLowerInvariant(), newDomain);
                        updateDomainCache = true;
                    }
                    if (updateDomainCache) {
                        //
                        // if there was a change, update the cache
                        //
                        core.cache.storeObject("domainContentList", core.domainDictionary, new List<string> { DbBaseModel.createDependencyKeyInvalidateOnChange<DomainModel>(core.cpParent) });
                    }
                    //
                    // domain found
                    //
                    core.domain = core.domainDictionary[requestDomain.ToLowerInvariant()];
                    if (core.domain.id == 0) {
                        //
                        // this is a default domain or a new domain -- add to the domain table
                        var domain = new DomainModel() {
                            name = requestDomain,
                            typeId = 1,
                            rootPageId = core.domain.rootPageId,
                            forwardUrl = core.domain.forwardUrl,
                            noFollow = core.domain.noFollow,
                            visited = core.domain.visited,
                            defaultTemplateId = core.domain.defaultTemplateId,
                            pageNotFoundPageId = core.domain.pageNotFoundPageId
                        };
                        //
                        // todo - would prefer not save new template
                        // -- fix, must save or template selection fails.
                        domain.save(core.cpParent, 0);
                        core.domain.id = domain.id;
                    }
                    if (!core.domain.visited) {
                        //
                        // set visited true
                        //
                        core.db.executeNonQuery("update ccdomains set visited=1 where name=" + DbController.encodeSQLText(requestDomain));
                        core.cache.invalidate("domainContentList");
                    }
                    if (core.domain.typeId == 1) {
                        //
                        // normal domain, leave it
                        //
                    } else if (GenericController.vbInstr(1, requestPathPage, "/" + core.appConfig.adminRoute, 1) != 0) {
                        //
                        // forwarding does not work in the admin site
                        //
                    } else if ((core.domain.typeId == 2) && (core.domain.forwardUrl != "")) {
                        //
                        // forward to a URL
                        if (GenericController.vbInstr(1, core.domain.forwardUrl, "://") == 0) {
                            core.domain.forwardUrl = "http://" + core.domain.forwardUrl;
                        }
                        redirect(core.domain.forwardUrl, "Forwarding to [" + core.domain.forwardUrl + "] because the current domain [" + requestDomain + "] is in the domain content set to forward to this URL", false, false);
                        return core.doc.continueProcessing;
                    } else if ((core.domain.typeId == 3) && (core.domain.forwardDomainId != 0) && (core.domain.forwardDomainId != core.domain.id)) {
                        //
                        // forward to a replacement domain
                        //
                        string forwardDomain = MetadataController.getRecordName(core, "domains", core.domain.forwardDomainId);
                        if (!string.IsNullOrEmpty(forwardDomain)) {
                            int pos = requestUrlSource.ToLowerInvariant().IndexOf(requestDomain.ToLowerInvariant());
                            if (pos > 0) {
                                core.domain.forwardUrl = requestUrlSource.ToString().Left(pos) + forwardDomain + requestUrlSource.ToString().Substring((pos + requestDomain.Length));
                                redirect(core.domain.forwardUrl, "Forwarding to [" + core.domain.forwardUrl + "] because the current domain [" + requestDomain + "] is in the domain content set to forward to this replacement domain", false, false);
                                return core.doc.continueProcessing;
                            }
                        }
                    }
                    //
                    // todo - CORS cannot be generated dynamically because info http method does not execute code
                    // -- add default CORS headers to approved domains
                    Uri originUri = httpContext.Request.UrlReferrer;
                    if (originUri != null) {
                        if (core.domainDictionary.ContainsKey(originUri.Host.ToLowerInvariant())) {
                            if (core.domainDictionary[originUri.Host.ToLowerInvariant()].allowCORS) {
                                httpContext.Response.AddHeader("Access-Control-Allow-Credentials", "true");
                                httpContext.Response.AddHeader("Access-Control-Allow-Methods", "POST,GET,OPTIONS");
                                httpContext.Response.AddHeader("Access-Control-Headers", "Content-Type,soapaction,X-Requested-With");
                                httpContext.Response.AddHeader("Access-Control-Allow-Origin", originUri.GetLeftPart(UriPartial.Authority));
                            }
                        }
                    }
                    if (core.domain.noFollow) {
                        response_NoFollow = true;
                    }
                    //
                    requestContentWatchPrefix = requestProtocol + requestDomain + "/";
                    requestContentWatchPrefix = requestContentWatchPrefix.Left(requestContentWatchPrefix.Length - 1);
                    //
                    requestPath = "/";
                    if (string.IsNullOrWhiteSpace(requestPathPage)) {
                        requestPage = core.siteProperties.serverPageDefault;
                    } else {
                        requestPage = requestPathPage;
                        int slashPtr = requestPathPage.LastIndexOf("/");
                        if (slashPtr >= 0) {
                            requestPage = "";
                            requestPath = requestPathPage.Left(slashPtr + 1);
                            if (requestPathPage.Length > 1) requestPage = requestPathPage.Substring(slashPtr + 1);
                        }
                    }
                    requestSecureURLRoot = "https://" + requestDomain + "/";
                    //
                    // ----- Style tag
                    adminMessage = "For more information, please contact the <a href=\"mailto:" + core.siteProperties.emailAdmin + "?subject=Re: " + requestDomain + "\">Site Administrator</A>.";
                    //
                    // todo ???? this is always false
                    if (requestDomain.ToLowerInvariant() != GenericController.vbLCase(requestDomain)) {
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
                LogController.logError(core, ex);
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
            requestCookies.Add(cookieKey, new WebServerController.CookieClass {
                name = cookieKey,
                value = cookieValue
            });
        }
        //====================================================================================================
        /// <summary>
        /// set cookie in iis response
        /// </summary>
        /// <param name="cookieName"></param>
        /// <param name="iCookieValue"></param>
        /// <param name="DateExpires"></param>
        /// <param name="domain"></param>
        /// <param name="Path"></param>
        /// <param name="Secure"></param>
        public void addResponseCookie(string cookieName, string iCookieValue, DateTime DateExpires = default(DateTime), string domain = "", string Path = "/", bool Secure = false) {
            try {
                string s = null;
                //
                if (core.doc.continueProcessing) {
                    {
                        if (iisContext != null) {
                            //
                            // Pass cookie to iis
                            iisContext.Response.Cookies[cookieName].Value = iCookieValue;
                            if (!isMinDate(DateExpires)) {
                                iisContext.Response.Cookies[cookieName].Expires = DateExpires;
                            }
                            if (!string.IsNullOrEmpty(domain)) {
                                iisContext.Response.Cookies[cookieName].Domain = domain;
                            }
                            if (!string.IsNullOrEmpty(Path)) {
                                iisContext.Response.Cookies[cookieName].Path = Path;
                            }
                            if (Secure) {
                                iisContext.Response.Cookies[cookieName].Secure = Secure;
                            }
                        } else {
                            //
                            // Pass Cookie to non-asp parent crlf delimited list of name,value,expires,domain,path,secure
                            if (bufferCookies != "") {
                                bufferCookies = bufferCookies + Environment.NewLine;
                            }
                            bufferCookies = bufferCookies + cookieName;
                            bufferCookies = bufferCookies + Environment.NewLine + iCookieValue;
                            //
                            s = "";
                            if (!isMinDate(DateExpires)) {
                                s = DateExpires.ToString();
                            }
                            bufferCookies = bufferCookies + Environment.NewLine + s;
                            //
                            s = "";
                            if (!string.IsNullOrEmpty(domain)) {
                                s = domain;
                            }
                            bufferCookies = bufferCookies + Environment.NewLine + s;
                            //
                            s = "/";
                            if (!string.IsNullOrEmpty(Path)) {
                                s = Path;
                            }
                            bufferCookies = bufferCookies + Environment.NewLine + s;
                            //
                            s = "false";
                            if (Secure) {
                                s = "true";
                            }
                            bufferCookies = bufferCookies + Environment.NewLine + s;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //====================================================================================================
        /// <summary>
        /// Set iis response status
        /// </summary>
        /// <param name="status">A string starting with the response number (like 200 or 404) followed by the response message</param>
        public void setResponseStatus(string status) {
            if (core.doc.continueProcessing) {
                LogController.logTrace(core, "setResponseStatus [" + status + "]");
                if (iisContext != null) {
                    // add header to response
                    iisContext.Response.Status = status;
                }
                bufferResponseStatus = status;
            }
        }
        //
        //
        //
        public void setResponseContentType(string ContentType) {
            if (core.doc.continueProcessing) {
                if (iisContext != null) {
                    // add header to response
                    iisContext.Response.ContentType = ContentType;
                }
                bufferContentType = ContentType;
            }
        }
        //
        //
        //
        public void addResponseHeader(string HeaderName, string HeaderValue) {
            try {
                if (core.doc.continueProcessing) {
                    if (iisContext != null) {
                        // add header to response
                        iisContext.Response.AddHeader(HeaderName, HeaderValue);
                    }
                    if (bufferResponseHeader != "") {
                        bufferResponseHeader = bufferResponseHeader + Environment.NewLine;
                    }
                    bufferResponseHeader = bufferResponseHeader + GenericController.vbReplace(HeaderName, Environment.NewLine, "") + Environment.NewLine + GenericController.vbReplace(GenericController.encodeText(HeaderValue), Environment.NewLine, "");
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
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
        public string redirect(string NonEncodedLink, string RedirectReason, bool IsPageNotFound = false, bool allowDebugMessage = true) {
            string result = HtmlController.div("Redirecting to [" + NonEncodedLink + "], reason [" + RedirectReason + "]", "ccWarningBox");
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
                    if (NonEncodedLink.Left(4).ToLowerInvariant() == "http") {
                        FullLink = NonEncodedLink;
                    } else {
                        if (NonEncodedLink.Left(1).ToLowerInvariant() == "/") {
                            //
                            // -- root relative - url starts with path, let it go
                        } else if (NonEncodedLink.Left(1).ToLowerInvariant() == "?") {
                            //
                            // -- starts with qs, fix issue where iis consideres this on the physical page, not the link-alias vitrual route
                            NonEncodedLink = requestPathPage + NonEncodedLink;
                        } else {
                            //
                            // -- url starts with the page
                            NonEncodedLink = requestPath + NonEncodedLink;
                        }
                        ShortLink = NonEncodedLink;
                        ShortLink = GenericController.ConvertLinkToShortLink(ShortLink, requestDomain, core.appConfig.cdnFileUrl);
                        ShortLink = GenericController.encodeVirtualPath(ShortLink, core.appConfig.cdnFileUrl, appRootPath, requestDomain);
                        FullLink = requestProtocol + requestDomain + ShortLink;
                    }

                    if (string.IsNullOrEmpty(NonEncodedLink)) {
                        //
                        // Link is not valid
                        //
                        LogController.logError(core, new GenericException("Redirect was called with a blank Link. Redirect Reason [" + RedirectReason + "]"));
                        return string.Empty;
                        //
                        // changed to main_ServerLinksource because if a redirect is caused by a link forward, and the host page for the iis 404 is
                        // the same as the destination of the link forward, this throws an error and does not forward. the only case where main_ServerLinksource is different
                        // then main_ServerLink is the linkfforward/linkalias case.
                        //
                    } else if ((requestForm.Count == 0) && (requestUrlSource == FullLink)) {
                        //
                        // Loop redirect error, throw trap and block redirect to prevent loop
                        //
                        LogController.logError(core, new GenericException("Redirect was called to the same URL, main_ServerLink is [" + requestUrl + "], main_ServerLinkSource is [" + requestUrlSource + "]. This redirect is only allowed if either the form or querystring has change to prevent cyclic redirects. Redirect Reason [" + RedirectReason + "]"));
                        return string.Empty;
                    } else if (IsPageNotFound) {
                        //
                        // Do a PageNotFound then redirect
                        //
                        LogController.addSiteWarning(core, "Page Not Found Redirect", "Page Not Found Redirect", "", 0, "Page Not Found Redirect [" + requestUrlSource + "]", "Page Not Found Redirect", "Page Not Found Redirect");
                        if (!string.IsNullOrEmpty(ShortLink)) {
                            string sql = "Update ccContentWatch set link=null where link=" + DbController.encodeSQLText(ShortLink);
                            Task.Run(() => core.db.executeNonQueryAsync(sql));
                        }
                        //
                        if (allowDebugMessage && core.doc.visitPropertyAllowDebugging) {
                            //
                            // -- Verbose - do not redirect, just print the link
                            EncodedLink = NonEncodedLink;
                        } else {
                            setResponseStatus(WebServerController.httpResponseStatus404_NotFound);
                        }
                    } else {

                        //
                        // Go ahead and redirect
                        //
                        LogController.logInfo(core, "Redirect called, from [" + requestUrl + "], to [" + NonEncodedLink + "], reason [" + RedirectReason + "]");
                        if (allowDebugMessage && core.doc.visitPropertyAllowDebugging) {
                            //
                            // -- Verbose - do not redirect, just print the link
                            EncodedLink = NonEncodedLink;
                            result = "<div style=\"padding:20px;border:1px dashed black;background-color:white;color:black;\">" + RedirectReason + "<p>Click to continue the redirect to <a href=" + EncodedLink + ">" + HtmlController.encodeHtml(NonEncodedLink) + "</a>...</p></div>";
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
                LogController.logError(core, ex);
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
        /// <summary>
        /// Verify a site exists, it not add it, it is does, verify all its settings
        /// </summary>
        /// <param name="core"></param>
        /// <param name="appName"></param>
        /// <param name="DomainName"></param>
        /// <param name="rootPublicFilesPath"></param>
        /// <param name="defaultDocOrBlank"></param>
        /// '
        public static void verifySite(CoreController core, string appName, string DomainName, string rootPublicFilesPath, string defaultDocOrBlank) {
            try {
                verifyAppPool(core, appName);
                verifyWebsite(core, appName, DomainName, rootPublicFilesPath, appName);
            } catch (Exception ex) {
                LogController.logError(core, ex, "verifySite");
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// verify the application pool. If it exists, update it. If not, create it
        /// </summary>
        /// <param name="core"></param>
        /// <param name="poolName"></param>
        public static void verifyAppPool(CoreController core, string poolName) {
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
                LogController.logError(core, ex, "verifyAppPool");
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
        private static void verifyWebsite(CoreController core, string appName, string domainName, string phyPath, string appPool) {
            try {

                using (ServerManager iisManager = new ServerManager()) {
                    Site site = null;
                    bool found = false;
                    //
                    // -- verify the site exists
                    foreach (Site siteWithinLoop in iisManager.Sites) {
                        site = siteWithinLoop;
                        if (siteWithinLoop.Name.ToLowerInvariant() == appName.ToLowerInvariant()) {
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
                LogController.logError(core, ex, "verifyWebsite");
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
        private static void verifyWebsite_Binding(CoreController core, Site site, string bindingInformation, string bindingProtocol) {
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
                LogController.logError(core, ex, "verifyWebsite_Binding");
            }
        }
        //
        //====================================================================================================
        //
        private static void verifyWebsite_VirtualDirectory(CoreController core, Site site, string appName, string virtualFolder, string physicalPath) {
            try {
                bool found = false;
                foreach (Application iisApp in site.Applications) {
                    if (iisApp.ApplicationPoolName.ToLowerInvariant() == appName.ToLowerInvariant()) {
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
                                        if (currentDirectory.Path.ToLowerInvariant() == newDirectoryPath.ToLowerInvariant()) {
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
                LogController.logError(core, ex, "verifyWebsite_VirtualDirectory");
            }
        }
        //========================================================================
        // main_RedirectByRecord( iContentName, iRecordID )
        //   looks up the record
        //   increments the 'clicks' field and redirects to the 'link' field
        //   returns true if the redirect happened OK
        //========================================================================
        //
        public static bool redirectByRecord_ReturnStatus(CoreController core, string contentName, int recordId, string fieldName = "") {
            bool result = false;
            int contentId = 0;
            int HostRecordId = 0;
            bool BlockRedirect = false;
            string iContentName = null;
            int iRecordId = 0;
            string iFieldName = null;
            string LinkPrefix = "";
            string EncodedLink = null;
            string NonEncodedLink = "";
            //
            iContentName = GenericController.encodeText(contentName);
            iRecordId = GenericController.encodeInteger(recordId);
            iFieldName = GenericController.encodeEmpty(fieldName, "link");
            BlockRedirect = false;
            using (var csData = new CsModel(core)) {
                if (csData.open(iContentName, "ID=" + iRecordId)) {
                    //
                    // Assume all Link fields are already encoded -- as this is how they would appear if the admin cut and pasted
                    EncodedLink = encodeText(csData.getText(iFieldName)).Trim(' ');
                    if (string.IsNullOrEmpty(EncodedLink)) {
                        BlockRedirect = true;
                    } else {
                        //
                        // ----- handle content special cases (prevent redirect to deleted records)
                        //
                        NonEncodedLink = GenericController.decodeResponseVariable(EncodedLink);
                        switch (GenericController.vbUCase(iContentName)) {
                            case "CONTENT WATCH": {
                                    //
                                    // ----- special case
                                    //       if this is a content watch record, check the underlying content for
                                    //       inactive or expired before redirecting
                                    //
                                    LinkPrefix = core.webServer.requestContentWatchPrefix;
                                    contentId = csData.getInteger("ContentID");
                                    var contentMeta = ContentMetadataModel.create(core, contentId);
                                    contentMeta.name = MetadataController.getContentNameByID(core, contentId);
                                    if (string.IsNullOrEmpty(contentMeta.name)) {
                                        //
                                        // ----- Content Watch with a bad ContentID, mark inactive
                                        //
                                        BlockRedirect = true;
                                        csData.set("active", 0);
                                    } else {
                                        HostRecordId = (csData.getInteger("RecordID"));
                                        if (HostRecordId == 0) {
                                            //
                                            // ----- Content Watch with a bad iRecordID, mark inactive
                                            //
                                            BlockRedirect = true;
                                            csData.set("active", 0);
                                        } else {
                                            using (var CSHost = new CsModel(core)) {
                                                CSHost.open(contentMeta.name, "ID=" + HostRecordId);
                                                if (!CSHost.ok()) {
                                                    //
                                                    // ----- Content Watch host record not found, mark inactive
                                                    //
                                                    BlockRedirect = true;
                                                    csData.set("active", 0);
                                                }
                                            }
                                        }
                                    }
                                    if (BlockRedirect) {
                                        //
                                        // ----- if a content watch record is blocked, delete the content tracking
                                        //
                                        MetadataController.deleteContentRules(core, contentMeta, HostRecordId);
                                    }
                                    break;
                                }
                            default: {
                                    // nop
                                    break;
                                }
                        }
                    }
                    if (!BlockRedirect) {
                        //
                        // If link incorrectly includes the LinkPrefix, take it off first, then add it back
                        //
                        NonEncodedLink = GenericController.removeUrlPrefix(NonEncodedLink, LinkPrefix);
                        if (csData.isFieldSupported("Clicks")) {
                            csData.set("Clicks", (csData.getNumber("Clicks")) + 1);
                        }
                        core.webServer.redirect(LinkPrefix + NonEncodedLink, "Redirect by Record, content [" + contentName + "], recordId [" + recordId + "], field [" + fieldName + "], called from " + GenericController.getCallStack(), false, false);
                        result = true;
                    }
                }
            }
            return result;
        }
        //
        //========================================================================
        //
        public static string getBrowserAcceptLanguage(CoreController core) {
            try {
                string AcceptLanguageString = (core.webServer.serverEnvironment.ContainsKey("HTTP_ACCEPT_LANGUAGE")) ? core.webServer.serverEnvironment["HTTP_ACCEPT_LANGUAGE"] : "";
                int CommaPosition = GenericController.vbInstr(1, AcceptLanguageString, ",");
                while (CommaPosition != 0) {
                    string AcceptLanguage = (AcceptLanguageString.Left(CommaPosition - 1)).Trim(' ');
                    AcceptLanguageString = AcceptLanguageString.Substring(CommaPosition);
                    if (AcceptLanguage.Length > 0) {
                        int DashPosition = GenericController.vbInstr(1, AcceptLanguage, "-");
                        if (DashPosition > 1) {
                            AcceptLanguage = AcceptLanguage.Left(DashPosition - 1);
                        }
                        DashPosition = GenericController.vbInstr(1, AcceptLanguage, ";");
                        if (DashPosition > 1) {
                            return AcceptLanguage.Left(DashPosition - 1);
                        }
                    }
                    CommaPosition = GenericController.vbInstr(1, AcceptLanguageString, ",");
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return "";
        }
        public void clearResponseBuffer() {
            iisContext.Response.ClearHeaders();
            bufferRedirect = "";
            bufferResponseHeader = "";
        }
    }
}