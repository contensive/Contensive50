
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;
using DefaultWebForm.Controllers;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Web;
using System.Web.Routing;

namespace DefaultSite {
    public static class ConfigurationClass {
        // 
        // ====================================================================================================
        /// <summary>
        ///         ''' if true, the route map is not loaded or invalid and needs to be loaded
        ///         ''' </summary>
        ///         ''' <returns></returns>
        public static bool routeMapDateInvalid() {
            if ((HttpContext.Current.Application["RouteMapDateCreated"] == null))
                return true;
            return !DateTime.TryParse(HttpContext.Current.Application["RouteMapDateCreated"].ToString(), out _);
        }
        // 
        // ====================================================================================================
        /// <summary>
        ///         ''' determine the Contensive application name from the webconfig or iis sitename
        ///         ''' </summary>
        ///         ''' <returns></returns>
        public static string getAppName() {
            // 
            // -- app name matches iis site name unless overridden by aspx app setting "ContensiveAppName"
            string appName = ConfigurationManager.AppSettings["ContensiveAppName"];
            if ((string.IsNullOrEmpty(appName)))
                appName = System.Web.Hosting.HostingEnvironment.ApplicationHost.GetSiteName();
            return appName;
        }
        // 
        // ====================================================================================================
        /// <summary>
        ///         ''' verify the routemap is not stale. This was the legacy reload process that reloads without an application load.
        ///         ''' </summary>
        ///         ''' <param name="cp"></param>
        public static void verifyRouteMap(CPClass cp) {
            // 
            // -- if application var does not equal routemap.datecreated rebuild
            if ((routeMapDateInvalid() || (cp.routeMap.dateCreated != (DateTime)HttpContext.Current.Application["RouteMapDateCreated"]))) {
                if (routeMapDateInvalid())
                    LogController.logLocalOnly("configurationClass, loadRouteMap, [" + cp.Site.Name + "], rebuild because HttpContext.Current.Application(RouteMapDateCreated) is not valid", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Info);
                else
                    LogController.logLocalOnly("configurationClass, loadRouteMap, [" + cp.Site.Name + "], rebuild because not equal, cp.routeMap.dateCreated [" + cp.routeMap.dateCreated.ToString() + "], HttpContext.Current.Application(RouteMapDateCreated) [" + HttpContext.Current.Application["RouteMapDateCreated"].ToString() + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Info);
                loadRouteMap(cp);
            }
        }
        // 
        // ====================================================================================================
        /// <summary>
        ///         ''' load the routemap
        ///         ''' </summary>
        ///         ''' <param name="cp"></param>
        public static void loadRouteMap(CPClass cp) {
            lock (RouteTable.Routes) {
                // 
                LogController.logLocalOnly("configurationClass, loadRouteMap enter, [" + cp.Site.Name + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Trace);
                // 
                HttpContext.Current.Application["routeMapDateCreated"] = cp.routeMap.dateCreated;
                // 
                RouteTable.Routes.Clear();
                foreach (var newRouteKeyValuePair in cp.routeMap.routeDictionary) {
                    try {
                        RouteTable.Routes.Remove(RouteTable.Routes[newRouteKeyValuePair.Key]);
                        RouteTable.Routes.MapPageRoute(newRouteKeyValuePair.Value.virtualRoute, newRouteKeyValuePair.Value.virtualRoute, newRouteKeyValuePair.Value.physicalRoute);
                    } catch (Exception ex) {
                        cp.Site.ErrorReport(ex, "Unexpected exception adding virtualRoute, key [" + newRouteKeyValuePair.Key + "], route [" + newRouteKeyValuePair.Value.virtualRoute + "]");
                    }
                }
            }
            // 
            LogController.logLocalOnly("configurationClass, loadRouteMap exit, [" + cp.Site.Name + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Info);
        }
        // 
        // ====================================================================================================
        /// <summary>
        ///         ''' build the http context from the iis httpContext object
        ///         ''' </summary>
        ///         ''' <param name="appName"></param>
        ///         ''' <param name="iisContext"></param>
        ///         ''' <returns></returns>
        public static HttpContextModel buildContext(string appName, HttpContext iisContext) {
            try {
                HttpContextModel context = new HttpContextModel();
                // iisContext.Request.ServerVariables

                if ((iisContext == null) || (iisContext.Request == null) || (iisContext.Response == null)) {
                    LogController.logLocalOnly("ConfigurationClass.buildContext - Attempt to initialize webContext but iisContext or one of its objects is null., [" + appName + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Fatal);
                    throw new ApplicationException("ConfigurationClass.buildContext - Attempt to initialize webContext but iisContext or one of its objects is null., [" + appName + "]");
                }
                // 
                // -- xfer body to context
                iisContext.Request.InputStream.Position = 0;
                System.IO.StreamReader str = new System.IO.StreamReader(iisContext.Request.InputStream);
                context.Request.requestBody = str.ReadToEnd();
                context.Request.ContentType = iisContext.Request.ContentType;
                // 
                // -- server variables
                if (true) {
                    NameValueCollection nameValues = iisContext.Request.ServerVariables;
                    for (int i = 0; i <= nameValues.Count - 1; i++) {
                        string key = nameValues.GetKey(i);
                        if (string.IsNullOrWhiteSpace(key))
                            continue;
                        if (context.Request.ServerVariables.ContainsKey(key))
                            context.Request.ServerVariables.Remove(key);
                        context.Request.ServerVariables.Add(key, nameValues.Get(i));
                    }
                }
                // 
                // -- request headers
                if (true) {
                    NameValueCollection nameValues = iisContext.Request.Headers;
                    for (int i = 0; i <= nameValues.Count - 1; i++) {
                        string key = nameValues.GetKey(i);
                        if (string.IsNullOrWhiteSpace(key))
                            continue;
                        if (context.Request.Headers.ContainsKey(key))
                            context.Request.Headers.Remove(key);
                        context.Request.Headers.Add(key, nameValues.Get(i));
                    }
                }
                // 
                // -- request querystring
                if (true) {
                    NameValueCollection nameValues = iisContext.Request.QueryString;
                    for (int i = 0; i <= nameValues.Count - 1; i++) {
                        string key = nameValues.GetKey(i);
                        if (string.IsNullOrWhiteSpace(key))
                            continue;
                        if (context.Request.Headers.ContainsKey(key))
                            context.Request.Headers.Remove(key);
                        context.Request.QueryString.Add(key, nameValues.Get(i));
                    }
                }
                // 
                // -- request form
                if (true) {
                    NameValueCollection nameValues = iisContext.Request.Form;
                    for (int i = 0; i <= nameValues.Count - 1; i++) {
                        string key = nameValues.GetKey(i);
                        if (string.IsNullOrWhiteSpace(key))
                            continue;
                        if (context.Request.Headers.ContainsKey(key))
                            context.Request.Headers.Remove(key);
                        context.Request.Form.Add(key, nameValues.Get(i));
                    }
                }
                //
                // -- request files
                // -- save file to windows temp files. Delete these files after response processed
                if (true) {
                    int filePtr = 0;
                    string instanceId = GenericController.getGUIDNaked();
                    foreach (string key in iisContext.Request.Files.AllKeys) {
                        if (string.IsNullOrWhiteSpace(key))
                            continue;
                        HttpPostedFile file = iisContext.Request.Files[key];
                        if (file == null)
                            continue;
                        string normalizedFilename = FileController.normalizeDosFilename(file.FileName);
                        if (file.ContentLength.Equals(0) || string.IsNullOrWhiteSpace(normalizedFilename))
                            continue;
                        string windowsTempFile = DefaultWebForm.Controllers.WindowsTempFileController.CreateTmpFile();
                        file.SaveAs(windowsTempFile);
                        DocPropertyModel docProperty = new DocPropertyModel() {
                            name = key,
                            value = normalizedFilename,
                            nameValue = System.Uri.EscapeDataString(key) + "=" + System.Uri.EscapeDataString(normalizedFilename),
                            tempfilename = windowsTempFile,
                            propertyType = DocPropertyModel.DocPropertyTypesEnum.file
                        };
                        context.Request.Files.Add(docProperty);
                        filePtr += 1;

                    }
                }

                if (true) {
                    foreach (string cookieKey in iisContext.Request.Cookies.Keys) {
                        if (string.IsNullOrWhiteSpace(cookieKey))
                            continue;
                        string cookieValue = context.Request.Cookies[cookieKey].Value;
                        cookieValue = System.Uri.EscapeDataString(cookieValue);
                        if ((context.Request.Cookies.ContainsKey(cookieKey)))
                            context.Request.Cookies.Remove(cookieKey);
                        context.Request.Cookies.Add(cookieKey, new HttpContextRequestCookie() {
                            Name = cookieKey,
                            Value = cookieValue
                        });
                    }
                }
                return context;
            }

            // If core.appConfig.appStatus.Equals(BaseModels.AppConfigBaseModel.AppStatusEnum.ok) Then
            // core.html.enableOutputBuffer(True)
            // core.doc.continueProcessing = True
            // setResponseContentType("text/html")
            // linkForwardSource = ""
            // linkForwardError = ""
            // requestReferer = requestReferrer
            // requestPageReferer = requestReferrer
            // core.doc.blockExceptionReporting = False
            // Dim CookieDetectKey As String = core.docProperties.getText(rnCookieDetect)

            // If Not String.IsNullOrEmpty(CookieDetectKey) Then
            // Dim visitToken As SecurityController.TokenData = SecurityController.decodeToken(core, CookieDetectKey)

            // If visitToken.id <> 0 Then
            // Dim sql As String = "update ccvisits set CookieSupport=1 where id=" & visitToken.id
            // core.db.executeNonQuery(sql)
            // core.doc.continueProcessing = False
            // Return core.doc.continueProcessing
            // End If
            // End If

            // Dim updateDomainCache As Boolean = False
            // core.domain.name = requestDomain
            // core.domain.rootPageId = 0
            // core.domain.noFollow = False
            // core.domain.typeId = 1
            // core.domain.visited = False
            // core.domain.id = 0
            // core.domain.forwardUrl = ""
            // core.domainDictionary = core.cache.getObject(Of Dictionary(Of String, DomainModel))("domainContentList")

            // If core.domainDictionary Is Nothing Then
            // core.domainDictionary = DomainModel.createDictionary(core.cpParent, "(active<>0)and(name is not null)")
            // updateDomainCache = True
            // End If

            // For Each domain As String In core.appConfig.domainList

            // If Not core.domainDictionary.ContainsKey(domain.ToLowerInvariant()) Then
            // LogController.logTrace(core, "adding domain record because configList domain not found [" & domain.ToLowerInvariant() & "]")
            // Dim newDomain = DbBaseModel.addEmpty(Of DomainModel)(core.cpParent, 0)
            // newDomain.name = domain
            // newDomain.rootPageId = 0
            // newDomain.noFollow = False
            // newDomain.typeId = 1
            // newDomain.visited = False
            // newDomain.forwardUrl = ""
            // newDomain.defaultTemplateId = 0
            // newDomain.pageNotFoundPageId = 0
            // newDomain.forwardDomainId = 0
            // newDomain.defaultRouteId = core.siteProperties.getInteger("")
            // newDomain.save(core.cpParent, 0)
            // core.domainDictionary.Add(domain.ToLowerInvariant(), newDomain)
            // updateDomainCache = True
            // End If
            // Next

            // If Not core.domainDictionary.ContainsKey(requestDomain.ToLowerInvariant()) Then
            // LogController.logTrace(core, "adding domain record because requestDomain [" & requestDomain.ToLowerInvariant() & "] not found")
            // Dim newDomain = DomainModel.addEmpty(Of DomainModel)(core.cpParent, 0)
            // newDomain.name = requestDomain
            // newDomain.rootPageId = 0
            // newDomain.noFollow = False
            // newDomain.typeId = 1
            // newDomain.visited = False
            // newDomain.forwardUrl = ""
            // newDomain.defaultTemplateId = 0
            // newDomain.pageNotFoundPageId = 0
            // newDomain.forwardDomainId = 0
            // newDomain.save(core.cpParent, 0)
            // core.domainDictionary.Add(requestDomain.ToLowerInvariant(), newDomain)
            // updateDomainCache = True
            // End If

            // If updateDomainCache Then
            // Dim dependencyKey As String = DbBaseModel.createDependencyKeyInvalidateOnChange(Of DomainModel)(core.cpParent)
            // Dim dependencyKeyHash As CacheKeyHashClass = core.cache.createKeyHash(dependencyKey)
            // Dim keyHashList As List(Of CacheKeyHashClass) = New List(Of CacheKeyHashClass) From {
            // dependencyKeyHash
            // }
            // core.cache.storeObject("domainContentList", core.domainDictionary, keyHashList)
            // End If

            // core.domain = core.domainDictionary(requestDomain.ToLowerInvariant())

            // If core.domain.id = 0 Then
            // Dim domain = New DomainModel With {
            // .name = requestDomain,
            // .typeId = 1,
            // .rootPageId = core.domain.rootPageId,
            // .forwardUrl = core.domain.forwardUrl,
            // .noFollow = core.domain.noFollow,
            // .visited = core.domain.visited,
            // .defaultTemplateId = core.domain.defaultTemplateId,
            // .pageNotFoundPageId = core.domain.pageNotFoundPageId
            // }
            // domain.save(core.cpParent, 0)
            // core.domain.id = domain.id
            // End If

            // If Not core.domain.visited Then
            // core.db.executeNonQuery("update ccdomains set visited=1 where name=" & DbController.encodeSQLText(requestDomain))
            // core.cache.invalidate("domainContentList")
            // End If

            // If core.domain.typeId = 1 Then
            // ElseIf GenericController.strInstr(1, requestPathPage, "/" & core.appConfig.adminRoute, 1) <> 0 Then
            // ElseIf core.domain.typeId.Equals(2) AndAlso Not String.IsNullOrEmpty(core.domain.forwardUrl) Then

            // If GenericController.strInstr(1, core.domain.forwardUrl, "://") = 0 Then
            // core.domain.forwardUrl = "http://" & core.domain.forwardUrl
            // End If

            // redirect(core.domain.forwardUrl, "Forwarding to [" & core.domain.forwardUrl & "] because the current domain [" + requestDomain & "] is in the domain content set to forward to this URL", False, False)
            // Return core.doc.continueProcessing
            // ElseIf (core.domain.typeId = 3) AndAlso (core.domain.forwardDomainId <> 0) AndAlso (core.domain.forwardDomainId <> core.domain.id) Then
            // Dim forwardDomain As String = MetadataController.getRecordName(core, "domains", core.domain.forwardDomainId)

            // If Not String.IsNullOrEmpty(forwardDomain) Then
            // Dim pos As Integer = requestUrlSource.IndexOf(requestDomain, StringComparison.InvariantCultureIgnoreCase)

            // If pos > 0 Then
            // core.domain.forwardUrl = requestUrlSource.left(pos) & forwardDomain + requestUrlSource.Substring((pos + requestDomain.Length))
            // redirect(core.domain.forwardUrl, "Forwarding to [" & core.domain.forwardUrl & "] because the current domain [" + requestDomain & "] is in the domain content set to forward to this replacement domain", False, False)
            // Return core.doc.continueProcessing
            // End If
            // End If
            // End If

            // Dim originUri As Uri = HttpContext.Request.UrlReferrer

            // If originUri IsNot Nothing Then

            // If core.domainDictionary.ContainsKey(originUri.Host.ToLowerInvariant()) Then

            // If core.domainDictionary(originUri.Host.ToLowerInvariant()).allowCORS Then
            // HttpContext.Response.AddHeader("Access-Control-Allow-Credentials", "true")
            // HttpContext.Response.AddHeader("Access-Control-Allow-Methods", "POST,GET,OPTIONS")
            // HttpContext.Response.AddHeader("Access-Control-Headers", "Content-Type,soapaction,X-Requested-With")
            // HttpContext.Response.AddHeader("Access-Control-Allow-Origin", originUri.GetLeftPart(UriPartial.Authority))
            // End If
            // End If
            // End If

            // If core.domain.noFollow Then
            // response_NoFollow = True
            // End If

            // requestContentWatchPrefix = requestProtocol + requestDomain & "/"
            // requestContentWatchPrefix = requestContentWatchPrefix.left(requestContentWatchPrefix.Length - 1)
            // requestPath = "/"

            // If String.IsNullOrWhiteSpace(requestPathPage) Then
            // requestPage = core.siteProperties.serverPageDefault
            // Else
            // requestPage = requestPathPage
            // Dim slashPtr As Integer = requestPathPage.LastIndexOf("/", StringComparison.InvariantCulture)

            // If slashPtr >= 0 Then
            // requestPage = ""
            // requestPath = requestPathPage.left(slashPtr + 1)

            // If requestPathPage.Length > 1 Then
            // requestPage = requestPathPage.Substring(slashPtr + 1)
            // End If
            // End If
            // End If

            // requestSecureURLRoot = "https://" & requestDomain & "/"
            // adminMessage = "For more information, please contact the <a href=""mailto:" & core.siteProperties.emailAdmin & "?subject=Re: " + requestDomain & """>Site Administrator</A>."

            // If String.IsNullOrEmpty(serverFormActionURL) Then
            // serverFormActionURL = requestProtocol + requestDomain + requestPath + requestPage
            // End If
            // End If

            catch (Exception ex) {
                LogController.logLocalOnly("configurationClass.buildContext, ex, [" + ex.ToString() + "]", Contensive.BaseClasses.CPLogBaseClass.LogLevel.Info);
                throw;
            }
        }
    }
}
