
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
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
//using Microsoft.Web.Administration;
using Contensive.Processor.Models.Domain;
using Microsoft.Web.Administration;
using Contensive.Processor.Exceptions;
//
namespace Contensive.Processor.Controllers {
    /// <summary>
    /// Code dedicated to processing iis input and output. lazy Constructed. (see coreHtmlClass for html processing)
    /// What belongs here is everything that would have to change if we converted to apache
    /// </summary>
    public class WebServerController {
        //
        // enum this, not consts --  https://en.wikipedia.org/wiki/List_of_HTTP_status_codes
        public const string httpResponseStatus200 = "200 OK";
        public const string httpResponseStatus404 = "404 Not Found";
        //
        private CoreController core;
        //
        // if this instance is a webRole, retain pointer for callbacks
        //
        public System.Web.HttpContext iisContext;
        //
        // -- Buffer request
        public string requestLanguage { get; set; } = ""; // set externally from HTTP_Accept_LANGUAGE
        public string requestHttpAccept { get; set; } = "";
        public string requestHttpAcceptCharset { get; set; } = "";
        public string requestHttpProfile { get; set; } = "";
        public string requestxWapProfile { get; set; } = "";
        public string requestHTTPVia { get; set; } = ""; // informs the server of proxies used during the request
        public string requestHTTPFrom { get; set; } = ""; // contains the email address of the requestor
        /// <summary>
        /// The path and page of the current request, without the leading slash which comes from the appRootPath
        /// </summary>
        public string requestPathPage { get; set; } = "";
        public string requestReferrer { get; set; } = "";
        /// <summary>
        /// The domain part of the current request URL
        /// </summary>
        public string requestDomain { get; set; } = "";
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
        public bool pageExcludeFromAnalytics { get; set; } = false; // For this page - true for remote methods and ajax
        public int memberAction { get; set; } = 0; // action to be performed during init
        public string adminMessage { get; set; } = ""; // For more information message
        public string requestPageReferer { get; set; } = ""; // replaced by main_ServerReferrer
        public string requestReferer { get; set; } = "";
        public string serverFormActionURL { get; set; } = ""; // The Action for all internal forms, if not set, default
        public string requestContentWatchPrefix { get; set; } = ""; // The different between the URL and the main_ContentWatch Pathpage
        /// <summary>
        /// The protocol used in the current quest
        /// </summary>
        public string requestProtocol { get; set; } = "";
        /// <summary>
        /// The requesting URL, from protocol to end of quesrystring
        /// </summary>
        public string requestUrl { get; set; } = "";
        /// <summary>
        /// The path between the requestDomain and the requestPage. NOTE - breaking change: this used to follow appRootPath and never started with /
        /// </summary>
        public string requestPath { get; set; } = "";
        /// <summary>
        /// The page or script name, typicall index.html or default.aspx or myPage.aspx
        /// </summary>
        public string requestPage { get; set; } = "";
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
        public class CookieClass {
            public string name;
            public string value;
        }
        public Dictionary<string, CookieClass> requestCookies;
        /// <summary>
        /// The body of the request
        /// </summary>
        public string requestBody;
        /// <summary>
        /// The content type of the request
        /// </summary>
        public string requestContentType;
        //
        //
        //====================================================================================================
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
                Copy = GenericController.vbReplace(Copy, "\r\n", "\\n");
                Copy = GenericController.vbReplace(Copy, "\r", "\\n");
                Copy = GenericController.vbReplace(Copy, "\n", "\\n");
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                Copy = GenericController.vbReplace(Copy, "\r\n", "\\n");
                Copy = GenericController.vbReplace(Copy, "\r", "\\n");
                Copy = GenericController.vbReplace(Copy, "\n", "\\n");
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                Copy = GenericController.vbReplace(Copy, "\r\n", "\\n");
                Copy = GenericController.vbReplace(Copy, "\r", "\\n");
                Copy = GenericController.vbReplace(Copy, "\n", "\\n");
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                LogController.handleError( core,ex);
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
                // todo convert this to lazy read from stored iisContext
                // -- read the request body into requestbody
                httpContext.Request.InputStream.Position = 0;
                System.IO.StreamReader str = new System.IO.StreamReader(httpContext.Request.InputStream);
                requestBody = str.ReadToEnd();
                //
                requestContentType = httpContext.Request.ContentType;
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
                        requestQueryString = GenericController.modifyQueryString(requestQueryString, key, keyValue);
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
                string instanceId = GenericController.getGUIDNaked();
                string[] formNames = iisContext.Request.Files.AllKeys;
                foreach (string formName in formNames) {
                    System.Web.HttpPostedFile file = iisContext.Request.Files[formName];
                    if (file != null) {
                        if ((file.ContentLength > 0) && (!string.IsNullOrEmpty(file.FileName))) {
                            DocPropertiesClass prop = new DocPropertiesClass {
                                Name = formName,
                                Value = file.FileName,
                                NameValue = encodeRequestVariable(formName) + "=" + encodeRequestVariable(file.FileName),
                                IsFile = true,
                                IsForm = true,
                                tempfilename = instanceId + "-" + filePtr.ToString() + ".bin"
                        };
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
                    keyValue = decodeResponseVariable(keyValue);
                    addRequestCookie(key, keyValue);
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
                        SecurityController.TokenData visitToken = SecurityController.decodeToken(core, CookieDetectKey);
                        if (visitToken.id != 0) {
                            core.db.executeNonQueryAsync("update ccvisits set CookieSupport=1 where id=" + visitToken.id);
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
                        core.domainDictionary = DomainModel.createDictionary(core, "(active<>0)and(name is not null)");
                        updateDomainCache = true;
                    }
                    //
                    // verify app config domainlist is in the domainlist cache
                    foreach (string domain in core.appConfig.domainList) {
                        if (!core.domainDictionary.ContainsKey(domain.ToLowerInvariant())) {
                            LogController.logTrace(core, "adding domain record because configList domain not found [" + domain.ToLowerInvariant() + "]");
                            var newDomain = DomainModel.addEmpty(core);
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
                            core.domainDictionary.Add(domain.ToLowerInvariant(), newDomain);
                            updateDomainCache = true;
                        }
                    }
                    //
                    // -- verify request domain
                    if (!core.domainDictionary.ContainsKey(requestDomain.ToLowerInvariant())) {
                        LogController.logTrace(core, "adding domain record because requestDomain [" + requestDomain.ToLowerInvariant() + "] not found");
                        var newDomain = DomainModel.addEmpty( core );
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
                        core.domainDictionary.Add(requestDomain.ToLowerInvariant(), newDomain);
                        updateDomainCache = true;
                    }
                    if (updateDomainCache) {
                        //
                        // if there was a change, update the cache
                        //
                        core.cache.storeObject("domainContentList", core.domainDictionary, new List<string> { DomainModel.getTableInvalidationKey(core) });
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
                        core.domain.id = domain.id;
                    }
                    if (!core.domain.visited) {
                        //
                        // set visited true
                        //
                        core.db.executeQuery("update ccdomains set visited=1 where name=" + DbController.encodeSQLText(requestDomain));
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
                        //
                        //
                        //Call AppendLog("main_init(), 1710 - exit for domain forward")
                        //
                        if (GenericController.vbInstr(1, core.domain.forwardUrl, "://") == 0) {
                            core.domain.forwardUrl = "http://" + core.domain.forwardUrl;
                        }
                        redirect(core.domain.forwardUrl, "Forwarding to [" + core.domain.forwardUrl + "] because the current domain [" + requestDomain + "] is in the domain content set to forward to this URL", false, false);
                        return core.doc.continueProcessing;
                    } else if ((core.domain.typeId == 3) && (core.domain.forwardDomainId != 0) && (core.domain.forwardDomainId != core.domain.id)) {
                        //
                        // forward to a replacement domain
                        //
                        string forwardDomain = MetadataController.getRecordName( core,"domains", core.domain.forwardDomainId);
                        if (!string.IsNullOrEmpty(forwardDomain)) {
                            int pos = requestUrlSource.ToLowerInvariant().IndexOf( requestDomain.ToLowerInvariant() );
                            if (pos > 0) {
                                core.domain.forwardUrl = requestUrlSource.ToString().Left( pos) + forwardDomain + requestUrlSource.ToString().Substring((pos + requestDomain.Length));
                                redirect(core.domain.forwardUrl, "Forwarding to [" + core.domain.forwardUrl + "] because the current domain [" + requestDomain + "] is in the domain content set to forward to this replacement domain", false, false);
                                return core.doc.continueProcessing;
                            }
                        }
                    }
                    //
                    // -- add default CORS headers to approved domains
                    Uri originUri = httpContext.Request.UrlReferrer;
                    if ( originUri != null ) {
                        if( core.domainDictionary.ContainsKey(originUri.Host.ToLowerInvariant())) {
                            if ( core.domainDictionary[originUri.Host.ToLowerInvariant()].allowCORS ) {
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
                    requestContentWatchPrefix = requestContentWatchPrefix.Left( requestContentWatchPrefix.Length - 1);
                    //
                    requestPath = "/";
                    if (string.IsNullOrWhiteSpace(requestPathPage)) {
                        requestPage = core.siteProperties.serverPageDefault;
                    } else {
                        requestPage = requestPathPage;
                        int slashPtr = requestPathPage.LastIndexOf("/");
                        if (slashPtr >=0) {
                            requestPage = "";
                            requestPath = requestPathPage.Left(slashPtr+1);
                            if(requestPathPage.Length>1) requestPage = requestPathPage.Substring(slashPtr+1);
                        }
                    }
                    requestSecureURLRoot = "https://" + requestDomain + "/";
                    //
                    // ----- Create Server Link property
                    //
                    requestUrl = requestProtocol + requestDomain + requestPath + requestPage;
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
                    requestUrl = requestProtocol + requestDomain + requestPath + requestPage;
                    if (requestQueryString != "") {
                        requestUrl = requestUrl + "?" + requestQueryString;
                    }
                    //
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
                LogController.handleError( core,ex);
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
                            if (!string.IsNullOrEmpty(domain)) {
                                s = domain;
                            }
                            bufferCookies = bufferCookies + "\r\n" + s;
                            //
                            s = "/";
                            if (!string.IsNullOrEmpty(Path)) {
                                s = Path;
                            }
                            bufferCookies = bufferCookies + "\r\n" + s;
                            //
                            s = "false";
                            if (Secure) {
                                s = "true";
                            }
                            bufferCookies = bufferCookies + "\r\n" + s;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
                        bufferResponseHeader = bufferResponseHeader + "\r\n";
                    }
                    bufferResponseHeader = bufferResponseHeader + GenericController.vbReplace(HeaderName, "\r\n", "") + "\r\n" + GenericController.vbReplace(GenericController.encodeText(HeaderValue), "\r\n", "");
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        public string redirect(string NonEncodedLink, string RedirectReason = "No explaination provided", bool IsPageNotFound = false, bool allowDebugMessage = true) {
            string result = HtmlController.div( "Redirecting to [" + NonEncodedLink + "], reason [" + RedirectReason + "]", "ccWarningBox" );
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
                    if (NonEncodedLink.Left( 4).ToLowerInvariant() == "http") {
                        FullLink = NonEncodedLink;
                    } else {
                        if (NonEncodedLink.Left( 1).ToLowerInvariant() == "/") {
                            //
                            // -- root relative - url starts with path, let it go
                        } else if (NonEncodedLink.Left( 1).ToLowerInvariant() == "?") {
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
                        LogController.handleError( core,new GenericException("Redirect was called with a blank Link. Redirect Reason [" + RedirectReason + "]"));
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
                        LogController.handleError( core,new GenericException("Redirect was called to the same URL, main_ServerLink is [" + requestUrl + "], main_ServerLinkSource is [" + requestUrlSource + "]. This redirect is only allowed if either the form or querystring has change to prevent cyclic redirects. Redirect Reason [" + RedirectReason + "]"));
                        return string.Empty;
                    } else if (IsPageNotFound) {
                        //
                        // Do a PageNotFound then redirect
                        //
                        LogController.addSiteWarning(core, "Page Not Found Redirect", "Page Not Found Redirect", "", 0, "Page Not Found Redirect [" + requestUrlSource + "]", "Page Not Found Redirect", "Page Not Found Redirect");
                        if (!string.IsNullOrEmpty(ShortLink)) {
                            core.db.executeNonQueryAsync("Update ccContentWatch set link=null where link=" + DbController.encodeSQLText(ShortLink));
                        }
                        //
                        if (allowDebugMessage && core.doc.visitPropertyAllowDebugging) {
                            //
                            // -- Verbose - do not redirect, just print the link
                            EncodedLink = NonEncodedLink;
                        } else {
                            setResponseStatus(WebServerController.httpResponseStatus404);
                        }
                    } else {

                        //
                        // Go ahead and redirect
                        //
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
                LogController.handleError( core,ex);
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
        public static void verifySite(CoreController core, string appName, string DomainName, string rootPublicFilesPath, string defaultDocOrBlank) {
            try {
                verifyAppPool(core, appName);
                verifyWebsite(core, appName, DomainName, rootPublicFilesPath, appName);
            } catch (Exception ex) {
                LogController.handleError( core,ex, "verifySite");
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
                LogController.handleError( core,ex, "verifyAppPool");
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
                    // -- create temp cclib virtual folder pointed to contensiveBase.
                    core.wwwfiles.deleteFolder("cclib");
                    core.wwwfiles.createPath("ContensiveBase");
                    verifyWebsite_VirtualDirectory(core, site, appName, "cclib", phyPath + @"ContensiveBase");
                    //
                    // -- commit any changes
                    iisManager.CommitChanges();
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex, "verifyWebsite");
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
                LogController.handleError( core,ex, "verifyWebsite_Binding");
            }
        }
        //
        //====================================================================================================
        //
        private static void verifyWebsite_VirtualDirectory(CoreController core, Site site, string appName, string virtualFolder, string physicalPath) {
            try {
                bool found = false;
                foreach ( Application iisApp in site.Applications) {
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
                LogController.handleError( core,ex, "verifyWebsite_VirtualDirectory");
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
            int HostRecordID = 0;
            bool BlockRedirect = false;
            string iContentName = null;
            int iRecordID = 0;
            string iFieldName = null;
            string LinkPrefix = "";
            string EncodedLink = null;
            string NonEncodedLink = "";
            //
            iContentName = GenericController.encodeText(contentName);
            iRecordID = GenericController.encodeInteger(recordId);
            iFieldName = GenericController.encodeEmpty(fieldName, "link");
            BlockRedirect = false;
            using (var csData = new CsModel(core)) {
                if (csData.open(iContentName, "ID=" + iRecordID)) {
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
                            case "CONTENT WATCH":
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
                                    HostRecordID = (csData.getInteger("RecordID"));
                                    if (HostRecordID == 0) {
                                        //
                                        // ----- Content Watch with a bad iRecordID, mark inactive
                                        //
                                        BlockRedirect = true;
                                        csData.set("active", 0);
                                    } else {
                                        using (var CSHost = new CsModel(core)) {
                                            CSHost.open(contentMeta.name, "ID=" + HostRecordID);
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
                                    MetadataController.deleteContentRules(core, contentMeta, HostRecordID);
                                }
                                break;
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
                        core.webServer.redirect(LinkPrefix + NonEncodedLink, "no reason given.", false, false);
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
                string AcceptLanguageString = GenericController.encodeText(core.webServer.requestLanguage) + ",";
                int CommaPosition = GenericController.vbInstr(1, AcceptLanguageString, ",");
                while (CommaPosition != 0) {
                    string AcceptLanguage = (AcceptLanguageString.Left( CommaPosition - 1)).Trim(' ');
                    AcceptLanguageString = AcceptLanguageString.Substring(CommaPosition);
                    if (AcceptLanguage.Length > 0) {
                        int DashPosition = GenericController.vbInstr(1, AcceptLanguage, "-");
                        if (DashPosition > 1) {
                            AcceptLanguage = AcceptLanguage.Left( DashPosition - 1);
                        }
                        DashPosition = GenericController.vbInstr(1, AcceptLanguage, ";");
                        if (DashPosition > 1) {
                            return AcceptLanguage.Left( DashPosition - 1);
                        }
                    }
                    CommaPosition = GenericController.vbInstr(1, AcceptLanguageString, ",");
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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