using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Microsoft.Web.Administration;
using Controllers;


namespace Controllers
{
	/// <summary>
	/// Code dedicated to processing iis input and output. lazy Constructed. (see coreHtmlClass for html processing)
	/// What belongs here is everything that would have to change if we converted to apache
	/// </summary>
	public class iisController
	{
		//
		private coreClass cpCore;
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
		public string requestLanguage {get; set;} = ""; // set externally from HTTP_Accept_LANGUAGE
		public string requestHttpAccept {get; set;} = "";
		public string requestHttpAcceptCharset {get; set;} = "";
		public string requestHttpProfile {get; set;} = "";
		public string requestxWapProfile {get; set;} = "";
		public string requestHTTPVia {get; set;} = ""; // informs the server of proxies used during the request
		public string requestHTTPFrom {get; set;} = ""; // contains the email address of the requestor
		public string requestPathPage {get; set;} = ""; // The Path and Page part of the current URI
		public string requestReferrer {get; set;} = "";
		public string requestDomain {get; set;} = ""; // The Host part of the current URI
		public bool requestSecure {get; set;} = false; // Set in InitASPEnvironment, true if https
		public string requestRemoteIP {get; set;} = "";
		public string requestBrowser {get; set;} = ""; // The browser for this visit
		public string requestQueryString {get; set;} = ""; // The QueryString of the current URI
		public bool requestFormUseBinaryHeader {get; set;} = false; // When set true with RequestNameBinaryRead=true, InitEnvironment reads the form in with a binary read
		public byte[] requestFormBinaryHeader {get; set;} // For asp pages, this is the full multipart header
		public Dictionary<string, string> requestFormDict {get; set;} = new Dictionary<string, string>();
		public bool requestSpaceAsUnderscore {get; set;} = false; // when true, is it assumed that dots in request variable names will convert
		public bool requestDotAsUnderscore {get; set;} = false; // (php converts spaces and dots to underscores)
		public string requestUrlSource {get; set;} = "";
		public string linkForwardSource {get; set;} = ""; // main_ServerPathPage -- set during init
		public string linkForwardError {get; set;} = ""; // always 404
		//Public Property readStreamJSForm As Boolean = False                  ' When true, the request comes from a browser handling a JSPage script line
		public bool pageExcludeFromAnalytics {get; set;} = false; // For this page - true for remote methods and ajax
		//
		// refactor - this method stears the stream between controllers, put it in cpcore
		//Public Property outStreamDevice As Integer = 0
		public int memberAction {get; set;} = 0; // action to be performed during init
		public string adminMessage {get; set;} = ""; // For more information message
		public string requestPageReferer {get; set;} = ""; // replaced by main_ServerReferrer
		public string requestReferer {get; set;} = "";
		public string serverFormActionURL {get; set;} = ""; // The Action for all internal forms, if not set, default
		public string requestContentWatchPrefix {get; set;} = ""; // The different between the URL and the main_ContentWatch Pathpage
		public string requestProtocol {get; set;} = ""; // Set in InitASPEnvironment, http or https
		public string requestUrl {get; set;} = ""; // The current URL, from protocol to end of quesrystring
		public string requestVirtualFilePath {get; set;} = ""; // The Virtual path for the site (host+main_ServerVirtualPath+"/" is site URI)
		public string requestPath {get; set;} = ""; // The path part of the current URI
		public string requestPage {get; set;} = ""; // The page part of the current URI
		public string requestSecureURLRoot {get; set;} = ""; // The URL to the root of the secure area for this site
		//
		// -- response
		public bool response_NoFollow {get; set;} = false; // when set, Meta no follow is added
		//
		// -- Buffer responses
		public string bufferRedirect {get; set;} = "";
		public string bufferContentType {get; set;} = "";
		public string bufferCookies {get; set;} = "";
		public string bufferResponseHeader {get; set;} = "";
		public string bufferResponseStatus {get; set;} = "";
		//------------------------------------------------------------------------
		//
		//   QueryString, Form and cookie Processing variables
		public class cookieClass
		{
			public string name;
			public string value;
		}
		public Dictionary<string, cookieClass> requestCookies;
		//
		//====================================================================================================
		//
		public iisController(coreClass cpCore) : base()
		{
			this.cpCore = cpCore;
			requestCookies = new Dictionary<string, cookieClass>();
		}
		//
		//=======================================================================================
		//   IIS Reset
		//
		//   Must be called from a process running as admin
		//   This can be done using the command queue, which kicks off the ccCmd process from the Server
		//
		public void reset()
		{
			try
			{
				string Cmd = null;
				string arg = null;
				string LogFilename = null;
				string Copy = null;
				//
				Microsoft.VisualBasic.VBMath.Randomize();
				LogFilename = "Temp\\" + genericController.encodeText(genericController.GetRandomInteger()) + ".Log";
				Cmd = "IISReset.exe";
				arg = "/restart >> \"" + LogFilename + "\"";
				runProcess(cpCore, Cmd, arg, true);
				Copy = cpCore.privateFiles.readFile(LogFilename);
				cpCore.privateFiles.deleteFile(LogFilename);
				Copy = genericController.vbReplace(Copy, Environment.NewLine, "\\n");
				Copy = genericController.vbReplace(Copy, "\r", "\\n");
				Copy = genericController.vbReplace(Copy, "\n", "\\n");
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
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
		public void stop()
		{
			try
			{
				string Cmd = null;
				string LogFilename = null;
				string Copy = null;
				//
				Microsoft.VisualBasic.VBMath.Randomize();
				LogFilename = "Temp\\" + genericController.encodeText(genericController.GetRandomInteger()) + ".Log";
				Cmd = "%comspec% /c IISReset /stop >> \"" + LogFilename + "\"";
				runProcess(cpCore, Cmd,, true);
				Copy = cpCore.privateFiles.readFile(LogFilename);
				cpCore.privateFiles.deleteFile(LogFilename);
				Copy = genericController.vbReplace(Copy, Environment.NewLine, "\\n");
				Copy = genericController.vbReplace(Copy, "\r", "\\n");
				Copy = genericController.vbReplace(Copy, "\n", "\\n");
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
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
		public void start()
		{
			try
			{
				string Cmd = null;
				string LogFilename = cpCore.privateFiles.rootLocalPath + "iisResetPipe.log";
				string Copy = null;
				//
				Microsoft.VisualBasic.VBMath.Randomize();
				Cmd = "%comspec% /c IISReset /start >> \"" + LogFilename + "\"";
				runProcess(cpCore, Cmd,, true);
				Copy = cpCore.privateFiles.readFile(LogFilename);
				cpCore.privateFiles.deleteFile(LogFilename);
				Copy = genericController.vbReplace(Copy, Environment.NewLine, "\\n");
				Copy = genericController.vbReplace(Copy, "\r", "\\n");
				Copy = genericController.vbReplace(Copy, "\n", "\\n");
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//=======================================================================================
		// recycle iis process
		//
		public void recycle(string appName)
		{
			try
			{
				ServerManager serverManager = null;
				ApplicationPoolCollection appPoolColl = null;
				//
				serverManager = new ServerManager();
				appPoolColl = serverManager.ApplicationPools;
				foreach (ApplicationPool appPool in appPoolColl)
				{
					if (appPool.Name.ToLower() == appName.ToLower())
					{
						if (appPool.Start == ObjectState.Started)
						{
							appPool.Recycle();
						}
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//==================================================================================
		//   Initialize the application
		//       returns responseOpen
		//==================================================================================
		//
		public bool initWebContext(System.Web.HttpContext httpContext)
		{
			try
			{
				//
				// -- setup IIS Response
				iisContext = httpContext;
				iisContext.Response.CacheControl = "no-cache";
				iisContext.Response.Expires = -1;
				iisContext.Response.Buffer = true;
				//'
				//
				// -- basic request environment
				requestDomain = iisContext.Request.ServerVariables("SERVER_NAME");
				requestPathPage = Convert.ToString(iisContext.Request.ServerVariables("SCRIPT_NAME"));
				requestReferrer = Convert.ToString(iisContext.Request.ServerVariables("HTTP_REFERER"));
				requestSecure = Convert.ToBoolean(iisContext.Request.ServerVariables("SERVER_PORT_SECURE"));
				requestRemoteIP = Convert.ToString(iisContext.Request.ServerVariables("REMOTE_ADDR"));
				requestBrowser = Convert.ToString(iisContext.Request.ServerVariables("HTTP_USER_AGENT"));
				requestLanguage = Convert.ToString(iisContext.Request.ServerVariables("HTTP_ACCEPT_LANGUAGE"));
				requestHttpAccept = Convert.ToString(iisContext.Request.ServerVariables("HTTP_ACCEPT"));
				requestHttpAcceptCharset = Convert.ToString(iisContext.Request.ServerVariables("HTTP_ACCEPT_CHARSET"));
				requestHttpProfile = Convert.ToString(iisContext.Request.ServerVariables("HTTP_PROFILE"));
				//
				// -- http QueryString
				if (iisContext.Request.QueryString.Count > 0)
				{
					requestQueryString = "";
					foreach (string key in iisContext.Request.QueryString)
					{
						string keyValue = iisContext.Request.QueryString(key);
						cpCore.docProperties.setProperty(key, keyValue);
						requestQueryString = genericController.ModifyQueryString(requestQueryString, key, keyValue);
					}
				}
				//
				// -- form
				requestFormDict.Clear();
				foreach (string key in iisContext.Request.Form.Keys)
				{
					string keyValue = iisContext.Request.Form(key);
					cpCore.docProperties.setProperty(key, keyValue, true);
					if (requestFormDict.ContainsKey(keyValue))
					{
						requestFormDict.Remove(keyValue);
					}
					requestFormDict.Add(key, keyValue);
				}
				//
				// -- handle files
				int filePtr = 0;
				string instanceId = genericController.createGuid().Replace("{", "").Replace("-", "").Replace("}", "");
				string[] formNames = iisContext.Request.Files.AllKeys;
				foreach (string formName in formNames)
				{
					System.Web.HttpPostedFile file = iisContext.Request.Files(formName);
					if (file != null)
					{
						if ((file.ContentLength > 0) && (!string.IsNullOrEmpty(file.FileName)))
						{
							docPropertiesClass prop = new docPropertiesClass();
							prop.Name = formName;
							prop.Value = file.FileName;
							prop.NameValue = EncodeRequestVariable(prop.Name) + "=" + EncodeRequestVariable(prop.Value);
							prop.IsFile = true;
							prop.IsForm = true;
							prop.tempfilename = instanceId + "-" + filePtr.ToString() + ".bin";
							file.SaveAs(cpCore.tempFiles.joinPath(cpCore.tempFiles.rootLocalPath, prop.tempfilename));
							cpCore.tempFiles.deleteOnDisposeFileList.Add(prop.tempfilename);
							prop.FileSize = Convert.ToInt32(file.ContentLength);
							cpCore.docProperties.setProperty(formName, prop);
							filePtr += 1;
						}
					}
				}
				//
				// load request cookies
				//
				foreach (string key in iisContext.Request.Cookies)
				{
					string keyValue = iisContext.Request.Cookies(key).Value;
					keyValue = DecodeResponseVariable(keyValue);
					addRequestCookie(key, keyValue);
				}
				//
				//--------------------------------------------------------------------------
				//
				if (cpCore.serverConfig.appConfig.appStatus != Models.Entity.serverConfigModel.appStatusEnum.OK)
				{
					//
					// did not initialize correctly
					//
				}
				else
				{
					//
					// continue
					//
					//initCounter += 1
					//
					cpCore.html.enableOutputBuffer(true);
					cpCore.doc.continueProcessing = true;
					setResponseContentType("text/html");
					//
					//--------------------------------------------------------------------------
					// ----- Process QueryString to cpcore.doc.main_InStreamArray
					//       Do this first to set cpcore.main_ReadStreamJSForm, cpcore.main_ReadStreamJSProcess, cpcore.main_ReadStreamBinaryRead (must be in QS)
					//--------------------------------------------------------------------------
					//
					linkForwardSource = "";
					linkForwardError = "";
					//
					// start with the best guess for the source url, then improve the guess based on what iis might have done
					//
					requestUrlSource = "http://";
					if (requestSecure)
					{
						requestUrlSource = "https://";
					}
					requestUrlSource = requestUrlSource + requestDomain + requestPathPage;
					if (requestQueryString != "")
					{
						requestUrlSource = requestUrlSource + "?" + requestQueryString;
					}
					if (requestQueryString != "")
					{
						//
						// Add query string to stream
						//
						cpCore.docProperties.addQueryString(requestQueryString);
					}
					//
					// Other Server variables
					requestReferer = requestReferrer;
					requestPageReferer = requestReferrer;
					//
					if (requestSecure)
					{
						requestProtocol = "https://";
					}
					else
					{
						requestProtocol = "http://";
					}
					//
					cpCore.doc.blockExceptionReporting = false;
					//
					//   javascript cookie detect on page1 of all visits
					string CookieDetectKey = cpCore.docProperties.getText(RequestNameCookieDetectVisitID);
					if (!string.IsNullOrEmpty(CookieDetectKey))
					{
						//
						DateTime cookieDetectDate = new DateTime();
						int CookieDetectVisitId = 0;
						cpCore.security.decodeToken(CookieDetectKey, CookieDetectVisitId, cookieDetectDate);
						if (CookieDetectVisitId != 0)
						{
							cpCore.db.executeQuery("update ccvisits set CookieSupport=1 where id=" + CookieDetectVisitId);
							cpCore.doc.continueProcessing = false; //--- should be disposed by caller --- Call dispose
							return cpCore.doc.continueProcessing;
						}
					}
					//
					//   verify Domain table entry
					bool updateDomainCache = false;
					//
					cpCore.domainLegacyCache.domainDetails.name = requestDomain;
					cpCore.domainLegacyCache.domainDetails.rootPageId = 0;
					cpCore.domainLegacyCache.domainDetails.noFollow = false;
					cpCore.domainLegacyCache.domainDetails.typeId = 1;
					cpCore.domainLegacyCache.domainDetails.visited = false;
					cpCore.domainLegacyCache.domainDetails.id = 0;
					cpCore.domainLegacyCache.domainDetails.forwardUrl = "";
					//
					// REFACTOR -- move to cpcore.domains class 
					cpCore.domainLegacyCache.domainDetailsList = cpCore.cache.getObject<Dictionary<string, Models.Entity.domainLegacyModel.domainDetailsClass>>("domainContentList");
					if (cpCore.domainLegacyCache.domainDetailsList == null)
					{
						//
						//  no cache found, build domainContentList from database
						cpCore.domainLegacyCache.domainDetailsList = new Dictionary<string, Models.Entity.domainLegacyModel.domainDetailsClass>();
						List<Models.Entity.domainModel> domainList = Models.Entity.domainModel.createList(cpCore, "(active<>0)and(name is not null)");
						foreach (var domain in domainList)
						{
							Models.Entity.domainLegacyModel.domainDetailsClass domainDetailsNew = new Models.Entity.domainLegacyModel.domainDetailsClass();
							domainDetailsNew.name = domain.name;
							domainDetailsNew.rootPageId = domain.RootPageID;
							domainDetailsNew.noFollow = domain.NoFollow;
							domainDetailsNew.typeId = domain.TypeID;
							domainDetailsNew.visited = domain.Visited;
							domainDetailsNew.id = domain.id;
							domainDetailsNew.forwardUrl = domain.ForwardURL;
							domainDetailsNew.defaultTemplateId = domain.DefaultTemplateId;
							domainDetailsNew.pageNotFoundPageId = domain.PageNotFoundPageID;
							domainDetailsNew.forwardDomainId = domain.forwardDomainId;
							if (cpCore.domainLegacyCache.domainDetailsList.ContainsKey(domain.name.ToLower()))
							{
								//
								logController.appendLog(cpCore, "Duplicate domain record found when adding domains from table [" + domain.name.ToLower() + "], duplicate skipped.");
								//
							}
							else
							{
								cpCore.domainLegacyCache.domainDetailsList.Add(domain.name.ToLower(), domainDetailsNew);
							}
						}
						updateDomainCache = true;
					}
					//
					// verify app config domainlist is in the domainlist cache
					//
					foreach (string domain in cpCore.serverConfig.appConfig.domainList)
					{
						if (!cpCore.domainLegacyCache.domainDetailsList.ContainsKey(domain.ToLower()))
						{
							Models.Entity.domainLegacyModel.domainDetailsClass domainDetailsNew = new Models.Entity.domainLegacyModel.domainDetailsClass();
							domainDetailsNew.name = domain;
							domainDetailsNew.rootPageId = 0;
							domainDetailsNew.noFollow = false;
							domainDetailsNew.typeId = 1;
							domainDetailsNew.visited = false;
							domainDetailsNew.id = 0;
							domainDetailsNew.forwardUrl = "";
							domainDetailsNew.defaultTemplateId = 0;
							domainDetailsNew.pageNotFoundPageId = 0;
							domainDetailsNew.forwardDomainId = 0;
							if (cpCore.domainLegacyCache.domainDetailsList.ContainsKey(domain.ToLower()))
							{
								//
								logController.appendLog(cpCore, "Duplicate domain record found when adding appConfig.domainList [" + domain.ToLower() + "], duplicate skipped");
								//
							}
							else
							{
								cpCore.domainLegacyCache.domainDetailsList.Add(domain.ToLower(), domainDetailsNew);
							}
						}
					}
					if (!cpCore.domainLegacyCache.domainDetailsList.ContainsKey(requestDomain.ToLower()))
					{
						//
						// -- domain not found
						// -- current host not in domainContent, add it and re-save the cache
						Models.Entity.domainLegacyModel.domainDetailsClass domainDetailsNew = new Models.Entity.domainLegacyModel.domainDetailsClass();
						domainDetailsNew.name = requestDomain;
						domainDetailsNew.rootPageId = 0;
						domainDetailsNew.noFollow = false;
						domainDetailsNew.typeId = 1;
						domainDetailsNew.visited = false;
						domainDetailsNew.id = 0;
						domainDetailsNew.forwardUrl = "";
						domainDetailsNew.defaultTemplateId = 0;
						domainDetailsNew.pageNotFoundPageId = 0;
						domainDetailsNew.forwardDomainId = 0;
						cpCore.domainLegacyCache.domainDetailsList.Add(requestDomain.ToLower(), domainDetailsNew);
						//
						// -- update database
						Models.Entity.domainModel domain = Models.Entity.domainModel.add(cpCore, new List<string>());
						cpCore.domainLegacyCache.domainDetails.id = domain.id;
						domain.name = requestDomain;
						domain.TypeID = 1;
						domain.save(cpCore);
						//
						updateDomainCache = true;
					}
					//
					// domain found
					//
					cpCore.domainLegacyCache.domainDetails = cpCore.domainLegacyCache.domainDetailsList(requestDomain.ToLower());
					if (cpCore.domainLegacyCache.domainDetails.id == 0)
					{
						//
						// this is a default domain or a new domain -- add to the domain table
						//
						Models.Entity.domainModel domain = new Models.Entity.domainModel()
						{
							name = requestDomain,
							TypeID = 1,
							RootPageID = cpCore.domainLegacyCache.domainDetails.rootPageId,
							ForwardURL = cpCore.domainLegacyCache.domainDetails.forwardUrl,
							NoFollow = cpCore.domainLegacyCache.domainDetails.noFollow,
							Visited = cpCore.domainLegacyCache.domainDetails.visited,
							DefaultTemplateId = cpCore.domainLegacyCache.domainDetails.defaultTemplateId,
							PageNotFoundPageID = cpCore.domainLegacyCache.domainDetails.pageNotFoundPageId
						};
						cpCore.domainLegacyCache.domainDetails.id = domain.id;
					}
					if (!cpCore.domainLegacyCache.domainDetails.visited)
					{
						//
						// set visited true
						//
						cpCore.db.executeQuery("update ccdomains set visited=1 where name=" + cpCore.db.encodeSQLText(requestDomain));
						cpCore.cache.setContent("domainContentList", "", "domains");
					}
					if (cpCore.domainLegacyCache.domainDetails.typeId == 1)
					{
						//
						// normal domain, leave it
						//
					}
					else if (genericController.vbInstr(1, requestPathPage, "/" + cpCore.serverConfig.appConfig.adminRoute, Microsoft.VisualBasic.Constants.vbTextCompare) != 0)
					{
						//
						// forwarding does not work in the admin site
						//
					}
					else if ((cpCore.domainLegacyCache.domainDetails.typeId == 2) && (cpCore.domainLegacyCache.domainDetails.forwardUrl != ""))
					{
						//
						// forward to a URL
						//
						//
						//Call AppendLog("main_init(), 1710 - exit for domain forward")
						//
						if (genericController.vbInstr(1, cpCore.domainLegacyCache.domainDetails.forwardUrl, "://") == 0)
						{
							cpCore.domainLegacyCache.domainDetails.forwardUrl = "http://" + cpCore.domainLegacyCache.domainDetails.forwardUrl;
						}
						redirect(cpCore.domainLegacyCache.domainDetails.forwardUrl, "Forwarding to [" + cpCore.domainLegacyCache.domainDetails.forwardUrl + "] because the current domain [" + requestDomain + "] is in the domain content set to forward to this URL", false, false);
						return cpCore.doc.continueProcessing;
					}
					else if ((cpCore.domainLegacyCache.domainDetails.typeId == 3) && (cpCore.domainLegacyCache.domainDetails.forwardDomainId != 0) & (cpCore.domainLegacyCache.domainDetails.forwardDomainId != cpCore.domainLegacyCache.domainDetails.id))
					{
						//
						// forward to a replacement domain
						//
						string forwardDomain = cpCore.db.getRecordName("domains", cpCore.domainLegacyCache.domainDetails.forwardDomainId);
						if (!string.IsNullOrEmpty(forwardDomain))
						{
							int pos = genericController.vbInstr(1, requestUrlSource, requestDomain, Microsoft.VisualBasic.Constants.vbTextCompare);
							if (pos > 0)
							{
								cpCore.domainLegacyCache.domainDetails.forwardUrl = requestUrlSource.ToString().Substring(0, pos - 1) + forwardDomain + requestUrlSource.ToString().Substring((pos + Microsoft.VisualBasic.Strings.Len(requestDomain)) - 1);
								redirect(cpCore.domainLegacyCache.domainDetails.forwardUrl, "Forwarding to [" + cpCore.domainLegacyCache.domainDetails.forwardUrl + "] because the current domain [" + requestDomain + "] is in the domain content set to forward to this replacement domain", false, false);
								return cpCore.doc.continueProcessing;
							}
						}
					}
					if (cpCore.domainLegacyCache.domainDetails.noFollow)
					{
						response_NoFollow = true;
					}
					if (updateDomainCache)
					{
						//
						// if there was a change, update the cache
						//
						cpCore.cache.setContent("domainContentList", cpCore.domainLegacyCache.domainDetailsList, "domains");
						//domainDetailsListText = cpCore.json.Serialize(cpCore.domains.domainDetailsList)
						//Call cpCore.cache.setObject("domainContentList", domainDetailsListText, "domains")
					}
					//
					requestVirtualFilePath = "/" + cpCore.serverConfig.appConfig.name;
					//
					requestContentWatchPrefix = requestProtocol + requestDomain + requestAppRootPath;
					requestContentWatchPrefix = requestContentWatchPrefix.ToString().Substring(0, Microsoft.VisualBasic.Strings.Len(requestContentWatchPrefix) - 1);
					//
					requestPath = "/";
					requestPage = cpCore.siteProperties.serverPageDefault;
					int TextStartPointer = requestPathPage.ToString().LastIndexOf("/") + 1;
					if (TextStartPointer != 0)
					{
						requestPath = requestPathPage.ToString().Substring(0, TextStartPointer);
						requestPage = requestPathPage.ToString().Substring(TextStartPointer);
					}
					requestSecureURLRoot = "https://" + requestDomain + requestAppRootPath;
					//
					// ----- Create Server Link property
					//
					requestUrl = requestProtocol + requestDomain + requestAppRootPath + requestPath + requestPage;
					if (requestQueryString != "")
					{
						requestUrl = requestUrl + "?" + requestQueryString;
					}
					if (requestUrlSource == "")
					{
						requestUrlSource = requestUrl;
					}
					//
					// ----- Style tag
					adminMessage = "For more information, please contact the <a href=\"mailto:" + cpCore.siteProperties.emailAdmin + "?subject=Re: " + requestDomain + "\">Site Administrator</A>.";
					//
					requestUrl = requestProtocol + requestDomain + requestAppRootPath + requestPath + requestPage;
					if (requestQueryString != "")
					{
						requestUrl = requestUrl + "?" + requestQueryString;
					}
					//
					if (requestDomain.ToLower() != genericController.vbLCase(requestDomain))
					{
						string Copy = "Redirecting to domain [" + requestDomain + "] because this site is configured to run on the current domain [" + requestDomain + "]";
						if (requestQueryString != "")
						{
							redirect(requestProtocol + requestDomain + requestPath + requestPage + "?" + requestQueryString, Copy, false, false);
						}
						else
						{
							redirect(requestProtocol + requestDomain + requestPath + requestPage, Copy, false, false);
						}
						cpCore.doc.continueProcessing = false; //--- should be disposed by caller --- Call dispose
						return cpCore.doc.continueProcessing;
					}
					//
					// ----- Create cpcore.main_ServerFormActionURL if it has not been overridden manually
					if (serverFormActionURL == "")
					{
						serverFormActionURL = requestProtocol + requestDomain + requestPath + requestPage;
					}
				}
				//
				// -- done at last
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return cpCore.doc.continueProcessing;
		}
		//
		//========================================================================
		// Read a cookie to the stream
		//
		public string getRequestCookie(string CookieName)
		{
			string cookieValue = "";
			try
			{
				if (requestCookies.ContainsKey(CookieName))
				{
					cookieValue = requestCookies(CookieName).value;
				}
				//'
				//Dim Pointer As Integer
				//Dim UName As String
				//'
				//web_GetStreamCookie = ""
				//If web.cookieArrayCount > 0 Then
				//    UName = genericController.vbUCase(CookieName)
				//    For Pointer = 0 To web.cookieArrayCount - 1
				//        If UName = genericController.vbUCase(web.requestCookies(Pointer).Name) Then
				//            web_GetStreamCookie = web.requestCookies(Pointer).Value
				//            Exit For
				//        End If
				//    Next
				//End If
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return cookieValue;
		}
		//
		//====================================================================================================
		//
		public void addRequestCookie(string cookieKey, string cookieValue)
		{
			if (requestCookies.ContainsKey(cookieKey))
			{
				//
			}
			else
			{
				iisController.cookieClass newCookie = new iisController.cookieClass();
				newCookie.name = cookieKey;
				newCookie.value = cookieValue;
				requestCookies.Add(cookieKey, newCookie);
			}
		}
		//
		//========================================================================
		// Write a cookie to the stream
		//========================================================================
		//
		public void addResponseCookie(string CookieName, string CookieValue, DateTime DateExpires = default(DateTime), string domain = "", string Path = "", bool Secure = false)
		{
			try
			{
				string iCookieName = null;
				string iCookieValue = null;
				string MethodName = null;
				string s = null;
				string usedDomainList = "";
				//
				iCookieName = genericController.encodeText(CookieName);
				iCookieValue = genericController.encodeText(CookieValue);
				//
				MethodName = "main_addResponseCookie";
				//
				if (cpCore.doc.continueProcessing)
				{
					//If cpCore.doc.continueProcessing And cpCore.doc.outputBufferEnabled Then
					if (false)
					{
						//'
						//' no domain provided, new mode
						//'   - write cookie for current domains
						//'   - write an iframe that called the cross-Site login
						//'   - http://127.0.0.1/ccLib/clientside/cross.html?v=1&vPath=%2F&vExpires=1%2F1%2F2012
						//'
						//domainListSplit = Split(cpCore.main_ServerDomainCrossList, ",")
						//For Ptr = 0 To UBound(domainListSplit)
						//    domainSet = Trim(domainListSplit(Ptr))
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
						//                iisContext.Response.Cookies(iCookieName).Value = iCookieValue
						//                If Not isMinDate(DateExpires) Then
						//                    iisContext.Response.Cookies(iCookieName).Expires = DateExpires
						//                End If
						//                'main_ASPResponse.Cookies(iCookieName).domain = domainSet
						//                If Not isMissing(Path) Then
						//                    iisContext.Response.Cookies(iCookieName).Path = genericController.encodeText(Path)
						//                End If
						//                If Not isMissing(Secure) Then
						//                    iisContext.Response.Cookies(iCookieName).Secure = Secure
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
						//            Link = cpCore.htmlDoc.html_EncodeHTML(Link)
						//            cpCore.htmlDoc.htmlForEndOfBody = cpCore.htmlDoc.htmlForEndOfBody & vbCrLf & vbTab & "<iframe style=""display:none;"" width=""0"" height=""0"" src=""" & Link & """></iframe>"
						//        End If
						//    End If
						//Next
					}
					else
					{
						//
						// Legacy mode - if no domain given just leave it off
						//
						if (iisContext != null)
						{
							//
							// Pass cookie to asp (compatibility)
							//
							iisContext.Response.Cookies(iCookieName).Value = iCookieValue;
							if (!isMinDate(DateExpires))
							{
								iisContext.Response.Cookies(iCookieName).Expires = DateExpires;
							}
							//main_ASPResponse.Cookies(iCookieName).domain = domainSet
							if (!isMissing(Path))
							{
								iisContext.Response.Cookies(iCookieName).Path = genericController.encodeText(Path);
							}
							if (!isMissing(Secure))
							{
								iisContext.Response.Cookies(iCookieName).Secure = Secure;
							}
						}
						else
						{
							//
							// Pass Cookie to non-asp parent
							//   crlf delimited list of name,value,expires,domain,path,secure
							//
							if (bufferCookies != "")
							{
								bufferCookies = bufferCookies + Environment.NewLine;
							}
							bufferCookies = bufferCookies + CookieName;
							bufferCookies = bufferCookies + Environment.NewLine + iCookieValue;
							//
							s = "";
							if (!isMinDate(DateExpires))
							{
								s = DateExpires.ToString();
							}
							bufferCookies = bufferCookies + Environment.NewLine + s;
							//
							s = "";
							if (!isMissing(domain))
							{
								s = genericController.encodeText(domain);
							}
							bufferCookies = bufferCookies + Environment.NewLine + s;
							//
							s = "/";
							if (!isMissing(Path))
							{
								s = genericController.encodeText(Path);
							}
							bufferCookies = bufferCookies + Environment.NewLine + s;
							//
							s = "false";
							if (genericController.EncodeBoolean(Secure))
							{
								s = "true";
							}
							bufferCookies = bufferCookies + Environment.NewLine + s;
						}
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//
		//
		public void setResponseStatus(string status)
		{
			bufferResponseStatus = status;
		}
		//
		//
		//
		public void setResponseContentType(object ContentType)
		{
			bufferContentType = Convert.ToString(ContentType);
		}
		//
		//
		//
		public void addResponseHeader(object HeaderName, object HeaderValue)
		{
			try
			{
				//
				if (cpCore.doc.continueProcessing)
				{
					if (bufferResponseHeader != "")
					{
						bufferResponseHeader = bufferResponseHeader + Environment.NewLine;
					}
					bufferResponseHeader = bufferResponseHeader + genericController.vbReplace(genericController.encodeText(HeaderName), Environment.NewLine, "") + Environment.NewLine + genericController.vbReplace(genericController.encodeText(HeaderValue), Environment.NewLine, "");
				}
				//
				return;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_SetStreamHeader")
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
		public string redirect(string NonEncodedLink, string RedirectReason = "No explaination provided", bool IsPageNotFound = false, bool allowDebugMessage = true)
		{
			string result = "";
			try
			{
				const string rnRedirectCycleFlag = "cycleFlag";
				string EncodedLink = null;
				string Copy = null;
				string ShortLink = string.Empty;
				string FullLink = null;
				int redirectCycles = 0;
				//
				if (cpCore.doc.continueProcessing)
				{
					redirectCycles = cpCore.docProperties.getInteger(rnRedirectCycleFlag);
					//
					// convert link to a long link on this domain
					if (NonEncodedLink.Substring(0, 4).ToLower() == "http")
					{
						FullLink = NonEncodedLink;
					}
					else
					{
						if (NonEncodedLink.Substring(0, 1).ToLower() == "/")
						{
							//
							// -- root relative - url starts with path, let it go
						}
						else if (NonEncodedLink.Substring(0, 1).ToLower() == "?")
						{
							//
							// -- starts with qs, fix issue where iis consideres this on the physical page, not the link-alias vitrual route
							NonEncodedLink = requestPathPage + NonEncodedLink;
						}
						else
						{
							//
							// -- url starts with the page
							NonEncodedLink = requestPath + NonEncodedLink;
						}
						ShortLink = NonEncodedLink;
						ShortLink = genericController.ConvertLinkToShortLink(ShortLink, requestDomain, requestVirtualFilePath);
						ShortLink = genericController.EncodeAppRootPath(ShortLink, requestVirtualFilePath, requestAppRootPath, requestDomain);
						FullLink = requestProtocol + requestDomain + ShortLink;
					}

					if (string.IsNullOrEmpty(NonEncodedLink))
					{
						//
						// Link is not valid
						//
						cpCore.handleException(new ApplicationException("Redirect was called with a blank Link. Redirect Reason [" + RedirectReason + "]"));
						return string.Empty;
						//
						// changed to main_ServerLinksource because if a redirect is caused by a link forward, and the host page for the iis 404 is
						// the same as the destination of the link forward, this throws an error and does not forward. the only case where main_ServerLinksource is different
						// then main_ServerLink is the linkfforward/linkalias case.
						//
					}
					else if ((requestFormDict.Count == 0) && (requestUrlSource == FullLink))
					{
						//
						// Loop redirect error, throw trap and block redirect to prevent loop
						//
						cpCore.handleException(new ApplicationException("Redirect was called to the same URL, main_ServerLink is [" + requestUrl + "], main_ServerLinkSource is [" + requestUrlSource + "]. This redirect is only allowed if either the form or querystring has change to prevent cyclic redirects. Redirect Reason [" + RedirectReason + "]"));
						return string.Empty;
					}
					else if (IsPageNotFound)
					{
						//
						// Do a PageNotFound then redirect
						//
						logController.log_appendLogPageNotFound(cpCore, requestUrlSource);
						if (!string.IsNullOrEmpty(ShortLink))
						{
							cpCore.db.executeQuery("Update ccContentWatch set link=null where link=" + cpCore.db.encodeSQLText(ShortLink));
						}
						//
						if (allowDebugMessage && cpCore.doc.visitPropertyAllowDebugging)
						{
							//
							// -- Verbose - do not redirect, just print the link
							EncodedLink = NonEncodedLink;
						}
						else
						{
							setResponseStatus("404 Not Found");
						}
					}
					else
					{

						//
						// Go ahead and redirect
						//
						Copy = "\"" + cpCore.doc.profileStartTime.ToString("") + "\",\"" + requestDomain + "\",\"" + requestUrlSource + "\",\"" + NonEncodedLink + "\",\"" + RedirectReason + "\"";
						logController.appendLog(cpCore, Copy, "performance", "redirects");
						//
						if (allowDebugMessage && cpCore.doc.visitPropertyAllowDebugging)
						{
							//
							// -- Verbose - do not redirect, just print the link
							EncodedLink = NonEncodedLink;
							result = "<div style=\"padding:20px;border:1px dashed black;background-color:white;color:black;\">" + RedirectReason + "<p>Click to continue the redirect to <a href=" + EncodedLink + ">" + genericController.encodeHTML(NonEncodedLink) + "</a>...</p></div>";
						}
						else
						{
							//
							// -- Redirect now
							clearResponseBuffer();
							if (iisContext != null)
							{
								//
								// -- redirect and release application. HOWEVER -- the thread will continue so use responseOpen=false to abort as much activity as possible
								iisContext.Response.Redirect(NonEncodedLink, false);
								iisContext.ApplicationInstance.CompleteRequest();
							}
							else
							{
								bufferRedirect = NonEncodedLink;
							}
						}
					}
					//
					// -- close the output stream
					cpCore.doc.continueProcessing = false;
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}

		//
		//
		public void flushStream()
		{
			if (iisContext != null)
			{
				iisContext.Response.Flush();
			}
		}
		//'
		//'====================================================================================================
		//'
		//Private Structure fieldTypePrivate
		//    Dim Name As String
		//    Dim fieldTypePrivate As Integer
		//End Structure
		//
		//====================================================================================================
		/// <summary>
		/// Verify a site exists, it not add it, it is does, verify all its settings
		/// </summary>
		/// <param name="cpCore"></param>
		/// <param name="appName"></param>
		/// <param name="DomainName"></param>
		/// <param name="rootPublicFilesPath"></param>
		/// <param name="defaultDocOrBlank"></param>
		/// '
		public static void verifySite(coreClass cpCore, string appName, string DomainName, string rootPublicFilesPath, string defaultDocOrBlank)
		{
			try
			{
				verifyAppPool(cpCore, appName);
				verifyWebsite(cpCore, appName, DomainName, rootPublicFilesPath, appName);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex, "verifySite");
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// verify the application pool. If it exists, update it. If not, create it
		/// </summary>
		/// <param name="cpCore"></param>
		/// <param name="poolName"></param>
		private static void verifyAppPool(coreClass cpCore, string poolName)
		{
			try
			{
				using (ServerManager serverManager = new ServerManager())
				{
					bool poolFound = false;
					ApplicationPool appPool = null;
					foreach (ApplicationPool appPoolWithinLoop in serverManager.ApplicationPools)
					{
						appPool = appPoolWithinLoop;
						if (appPoolWithinLoop.Name == poolName)
						{
							poolFound = true;
							break;
						}
					}
					if (!poolFound)
					{
						appPool = serverManager.ApplicationPools.Add(poolName);
					}
					else
					{
						appPool = serverManager.ApplicationPools(poolName);
					}
					appPool.ManagedRuntimeVersion = "v4.0";
					appPool.Enable32BitAppOnWin64 = true;
					appPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
					serverManager.CommitChanges();
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex, "verifyAppPool");
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// verify the website. If it exists, update it. If not, create it
		/// </summary>
		/// <param name="cpCore"></param>
		/// <param name="appName"></param>
		/// <param name="domainName"></param>
		/// <param name="phyPath"></param>
		/// <param name="appPool"></param>
		private static void verifyWebsite(coreClass cpCore, string appName, string domainName, string phyPath, string appPool)
		{
			try
			{

				using (ServerManager iisManager = new ServerManager())
				{
					Site site = null;
					bool found = false;
					//
					// -- verify the site exists
					foreach (Site siteWithinLoop in iisManager.Sites)
					{
						site = siteWithinLoop;
						if (siteWithinLoop.Name.ToLower() == appName.ToLower())
						{
							found = true;
							break;
						}
					}
					if (!found)
					{
						iisManager.Sites.Add(appName, "http", "*:80:" + appName, phyPath);
					}
					site = iisManager.Sites(appName);
					//
					// -- verify the bindings
					verifyWebsite_Binding(cpCore, site, "*:80:" + appName, "http");
					verifyWebsite_Binding(cpCore, site, "*:80:" + domainName, "http");
					//
					// -- verify the application pool
					site.ApplicationDefaults.ApplicationPoolName = appPool;
					foreach (Application iisApp in site.Applications)
					{
						iisApp.ApplicationPoolName = appPool;
					}
					//
					// -- verify the cdn virtual directory (if configured)
					string cdnFilesPrefix = cpCore.serverConfig.appConfig.cdnFilesNetprefix;
					if (cdnFilesPrefix.IndexOf("://") < 0)
					{
						verifyWebsite_VirtualDirectory(cpCore, site, appName, cdnFilesPrefix, cpCore.serverConfig.appConfig.cdnFilesPath);
					}
					//
					// -- commit any changes
					iisManager.CommitChanges();
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex, "verifyWebsite");
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// Verify the binding
		/// </summary>
		/// <param name="cpCore"></param>
		/// <param name="site"></param>
		/// <param name="bindingInformation"></param>
		/// <param name="bindingProtocol"></param>
		private static void verifyWebsite_Binding(coreClass cpCore, Site site, string bindingInformation, string bindingProtocol)
		{
			try
			{
				using (ServerManager iisManager = new ServerManager())
				{
					Binding binding = null;
					bool found = false;
					found = false;
					foreach (Binding bindingWithinLoop in site.Bindings)
					{
						binding = bindingWithinLoop;
						if ((bindingWithinLoop.BindingInformation == bindingInformation) && (bindingWithinLoop.Protocol == bindingProtocol))
						{
							found = true;
							break;
						}
					}
					if (!found)
					{
						binding = site.Bindings.CreateElement();
						binding.BindingInformation = bindingInformation;
						binding.Protocol = bindingProtocol;
						site.Bindings.Add(binding);
						iisManager.CommitChanges();
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex, "verifyWebsite_Binding");
			}
		}
		//
		//====================================================================================================
		//
		private static void verifyWebsite_VirtualDirectory(coreClass cpCore, Site site, string appName, string virtualFolder, string physicalPath)
		{
			try
			{
				bool found = false;
				foreach (Application iisApp in site.Applications)
				{
					if (iisApp.ApplicationPoolName.ToLower() == appName.ToLower())
					{
						foreach (VirtualDirectory virtualDirectory in iisApp.VirtualDirectories)
						{
							if (virtualDirectory.Path == virtualFolder)
							{
								found = true;
								break;
							}
						}
						if (!found)
						{
							List<string> vpList = virtualFolder.Split('/').ToList();
							string newDirectoryPath = "";

							foreach (string newDirectoryFolderName in vpList)
							{
								if (!string.IsNullOrEmpty(newDirectoryFolderName))
								{
									newDirectoryPath += "/" + newDirectoryFolderName;
									bool directoryFound = false;
									foreach (VirtualDirectory currentDirectory in iisApp.VirtualDirectories)
									{
										if (currentDirectory.Path.ToLower() == newDirectoryPath.ToLower())
										{
											directoryFound = true;
											break;
										}
									}
									if (!directoryFound)
									{
										iisApp.VirtualDirectories.Add(newDirectoryPath, physicalPath);
									}
								}
							}
						}
					}
					if (found)
					{
						break;
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex, "verifyWebsite_VirtualDirectory");
			}
		}
		//========================================================================
		// main_RedirectByRecord( iContentName, iRecordID )
		//   looks up the record
		//   increments the 'clicks' field and redirects to the 'link' field
		//   returns true if the redirect happened OK
		//========================================================================
		//
		public static bool main_RedirectByRecord_ReturnStatus(coreClass cpcore, string ContentName, int RecordID, string FieldName = "")
		{
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
			string LinkPrefix = string.Empty;
			string EncodedLink = null;
			string NonEncodedLink = "";
			//
			iContentName = genericController.encodeText(ContentName);
			iRecordID = genericController.EncodeInteger(RecordID);
			iFieldName = genericController.encodeEmptyText(FieldName, "link");
			//
			MethodName = "main_RedirectByRecord_ReturnStatus( " + iContentName + ", " + iRecordID + ", " + genericController.encodeEmptyText(FieldName, "(fieldname empty)") + ")";
			//
			tempmain_RedirectByRecord_ReturnStatus = false;
			BlockRedirect = false;
			CSPointer = cpcore.db.csOpen(iContentName, "ID=" + iRecordID);
			if (cpcore.db.csOk(CSPointer))
			{
				// 2/18/2008 - EncodeLink change
				//
				// Assume all Link fields are already encoded -- as this is how they would appear if the admin cut and pasted
				//
				EncodedLink = Convert.ToString(cpcore.db.csGetText(CSPointer, iFieldName)).Trim(' ');
				if (string.IsNullOrEmpty(EncodedLink))
				{
					BlockRedirect = true;
				}
				else
				{
					//
					// ----- handle content special cases (prevent redirect to deleted records)
					//
					NonEncodedLink = genericController.DecodeResponseVariable(EncodedLink);
					switch (genericController.vbUCase(iContentName))
					{
						case "CONTENT WATCH":
							//
							// ----- special case
							//       if this is a content watch record, check the underlying content for
							//       inactive or expired before redirecting
							//
							LinkPrefix = cpcore.webServer.requestContentWatchPrefix;
							ContentID = (cpcore.db.csGetInteger(CSPointer, "ContentID"));
							HostContentName = models.complex.cdefmodel.getContentNameByID(cpcore,ContentID);
							if (string.IsNullOrEmpty(HostContentName))
							{
								//
								// ----- Content Watch with a bad ContentID, mark inactive
								//
								BlockRedirect = true;
								cpcore.db.csSet(CSPointer, "active", 0);
							}
							else
							{
								HostRecordID = (cpcore.db.csGetInteger(CSPointer, "RecordID"));
								if (HostRecordID == 0)
								{
									//
									// ----- Content Watch with a bad iRecordID, mark inactive
									//
									BlockRedirect = true;
									cpcore.db.csSet(CSPointer, "active", 0);
								}
								else
								{
									CSHost = cpcore.db.csOpen(HostContentName, "ID=" + HostRecordID);
									if (!cpcore.db.csOk(CSHost))
									{
										//
										// ----- Content Watch host record not found, mark inactive
										//
										BlockRedirect = true;
										cpcore.db.csSet(CSPointer, "active", 0);
									}
								}
								cpcore.db.csClose(CSHost);
							}
							if (BlockRedirect)
							{
								//
								// ----- if a content watch record is blocked, delete the content tracking
								//
								cpcore.db.deleteContentRules(models.complex.cdefmodel.getcontentid(cpcore,HostContentName), HostRecordID);
							}
							break;
					}
				}
				if (!BlockRedirect)
				{
					//
					// If link incorrectly includes the LinkPrefix, take it off first, then add it back
					//
					NonEncodedLink = genericController.ConvertShortLinkToLink(NonEncodedLink, LinkPrefix);
					if (cpcore.db.cs_isFieldSupported(CSPointer, "Clicks"))
					{
						cpcore.db.csSet(CSPointer, "Clicks", (cpcore.db.csGetNumber(CSPointer, "Clicks")) + 1);
					}
					cpcore.webServer.redirect(LinkPrefix + NonEncodedLink, "Call to " + MethodName + ", no reason given.", false, false);
					tempmain_RedirectByRecord_ReturnStatus = true;
				}
			}
			cpcore.db.csClose(CSPointer);
			return tempmain_RedirectByRecord_ReturnStatus;
		}
		//
		//========================================================================
		//
		public static string getBrowserAcceptLanguage(coreClass cpCore)
		{
			try
			{
				string AcceptLanguageString = genericController.encodeText(cpCore.webServer.requestLanguage) + ",";
				int CommaPosition = genericController.vbInstr(1, AcceptLanguageString, ",");
				while (CommaPosition != 0)
				{
					string AcceptLanguage = (AcceptLanguageString.Substring(0, CommaPosition - 1)).Trim(' ');
					AcceptLanguageString = AcceptLanguageString.Substring(CommaPosition);
					if (AcceptLanguage.Length > 0)
					{
						int DashPosition = genericController.vbInstr(1, AcceptLanguage, "-");
						if (DashPosition > 1)
						{
							AcceptLanguage = AcceptLanguage.Substring(0, DashPosition - 1);
						}
						DashPosition = genericController.vbInstr(1, AcceptLanguage, ";");
						if (DashPosition > 1)
						{
							return AcceptLanguage.Substring(0, DashPosition - 1);
						}
					}
					CommaPosition = genericController.vbInstr(1, AcceptLanguageString, ",");
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return "";
		}
		public void clearResponseBuffer()
		{
			bufferRedirect = "";
			bufferResponseHeader = "";
		}
	}
}