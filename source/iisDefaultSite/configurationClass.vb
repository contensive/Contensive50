
Imports System.Web.Routing
Imports Contensive
Imports Contensive.Processor
Imports Contensive.Processor.Controllers
Imports Contensive.Processor.Models.Domain

Public Class ConfigurationClass
    '
    '====================================================================================================
    ''' <summary>
    ''' if true, the route map is not loaded or invalid and needs to be loaded
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function routeMapDateInvalid() As Boolean
        If (HttpContext.Current.Application("RouteMapDateCreated") Is Nothing) Then Return True
        Dim dateResult As Date
        Return (Not Date.TryParse(HttpContext.Current.Application("RouteMapDateCreated").ToString(), dateResult))
    End Function
    '
    '====================================================================================================
    ''' <summary>
    ''' determine the Contensive application name from the webconfig or iis sitename
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function getAppName() As String
        '
        ' -- app name matches iis site name unless overridden by aspx app setting "ContensiveAppName"
        Dim appName As String = ConfigurationManager.AppSettings("ContensiveAppName")
        If (String.IsNullOrEmpty(appName)) Then
            appName = System.Web.Hosting.HostingEnvironment.ApplicationHost.GetSiteName()
        End If
        Return appName
    End Function
    '
    ' ====================================================================================================
    ''' <summary>
    ''' verify the routemap is not stale. This was the legacy reload process that reloads without an application load.
    ''' </summary>
    ''' <param name="cp"></param>
    Public Shared Sub verifyRouteMap(cp As CPClass)
        '
        ' -- if application var does not equal routemap.datecreated rebuild
        If (routeMapDateInvalid() OrElse (cp.routeMap.dateCreated <> CDate(HttpContext.Current.Application("RouteMapDateCreated")))) Then
            If routeMapDateInvalid() Then
                LogController.logLocalOnly("configurationClass, loadRouteMap, [" + cp.Site.Name + "], rebuild because HttpContext.Current.Application(RouteMapDateCreated) is not valid", BaseClasses.CPLogBaseClass.LogLevel.Info)
            Else
                LogController.logLocalOnly("configurationClass, loadRouteMap, [" + cp.Site.Name + "], rebuild because not equal, cp.routeMap.dateCreated [" + cp.routeMap.dateCreated.ToString() + "], HttpContext.Current.Application(RouteMapDateCreated) [" + HttpContext.Current.Application("RouteMapDateCreated").ToString() + "]", BaseClasses.CPLogBaseClass.LogLevel.Info)
            End If
            loadRouteMap(cp)
        End If
    End Sub
    '
    ' ====================================================================================================
    ''' <summary>
    ''' load the routemap
    ''' </summary>
    ''' <param name="cp"></param>
    Public Shared Sub loadRouteMap(cp As CPClass)
        SyncLock RouteTable.Routes
            '
            LogController.logLocalOnly("configurationClass, loadRouteMap enter, [" + cp.Site.Name + "]", BaseClasses.CPLogBaseClass.LogLevel.Trace)
            '
            HttpContext.Current.Application("routeMapDateCreated") = cp.routeMap.dateCreated
            '
            RouteTable.Routes.Clear()
            For Each newRouteKeyValuePair In cp.routeMap.routeDictionary
                Try
                    RouteTable.Routes.Remove(RouteTable.Routes(newRouteKeyValuePair.Key))
                    RouteTable.Routes.MapPageRoute(newRouteKeyValuePair.Value.virtualRoute, newRouteKeyValuePair.Value.virtualRoute, newRouteKeyValuePair.Value.physicalRoute)
                Catch ex As Exception
                    cp.Site.ErrorReport(ex, "Unexpected exception adding virtualRoute, key [" & newRouteKeyValuePair.Key & "], route [" & newRouteKeyValuePair.Value.virtualRoute & "]")
                End Try
            Next
        End SyncLock
        '
        LogController.logLocalOnly("configurationClass, loadRouteMap exit, [" + cp.Site.Name + "]", BaseClasses.CPLogBaseClass.LogLevel.Info)
        '
    End Sub
    '
    ' ====================================================================================================
    ''' <summary>
    ''' build the http context from the iis httpContext object
    ''' </summary>
    ''' <param name="appName"></param>
    ''' <param name="iisContext"></param>
    ''' <returns></returns>
    Public Shared Function buildContext(appName As String, ByVal iisContext As HttpContext) As Contensive.Processor.Models.Domain.HttpContextModel
        Try
            Dim context As New HttpContextModel
            'iisContext.Request.ServerVariables

            If (iisContext Is Nothing) OrElse (iisContext.Request Is Nothing) OrElse (iisContext.Response Is Nothing) Then
                LogController.logLocalOnly("ConfigurationClass.buildContext - Attempt to initialize webContext but iisContext or one of its objects is null., [" + appName + "]", BaseClasses.CPLogBaseClass.LogLevel.Fatal)
                Throw New ApplicationException("ConfigurationClass.buildContext - Attempt to initialize webContext but iisContext or one of its objects is null., [" + appName + "]")
            End If
            '
            ' -- setup http cache
            context.Response.CacheControl = "no-cache"
            context.Response.Expires = -1
            context.Response.Buffer = True
            '
            ' -- xfer body to context
            iisContext.Request.InputStream.Position = 0
            Dim str As System.IO.StreamReader = New System.IO.StreamReader(iisContext.Request.InputStream)
            context.Request.requestBody = str.ReadToEnd()
            context.Request.ContentType = iisContext.Request.ContentType
            '
            ' -- server variables
            If True Then
                Dim nameValues As NameValueCollection = iisContext.Request.ServerVariables
                For i As Integer = 0 To nameValues.Count - 1
                    Dim key As String = nameValues.GetKey(i)
                    If String.IsNullOrWhiteSpace(key) Then
                        Continue For
                    End If
                    If context.Request.ServerVariables.ContainsKey(key) Then
                        context.Request.ServerVariables.Remove(key)
                    End If
                    context.Request.ServerVariables.Add(key, nameValues.Get(i))
                Next
            End If
            '
            ' -- request headers
            If True Then
                Dim nameValues As NameValueCollection = iisContext.Request.Headers
                For i As Integer = 0 To nameValues.Count - 1
                    Dim key As String = nameValues.GetKey(i)
                    If String.IsNullOrWhiteSpace(key) Then
                        Continue For
                    End If
                    If context.Request.Headers.ContainsKey(key) Then
                        context.Request.Headers.Remove(key)
                    End If
                    context.Request.Headers.Add(key, nameValues.Get(i))
                Next
            End If
            '
            ' -- request querystring
            If True Then
                Dim nameValues As NameValueCollection = iisContext.Request.QueryString
                For i As Integer = 0 To nameValues.Count - 1
                    Dim key As String = nameValues.GetKey(i)
                    If String.IsNullOrWhiteSpace(key) Then
                        Continue For
                    End If
                    If context.Request.Headers.ContainsKey(key) Then
                        context.Request.Headers.Remove(key)
                    End If
                    context.Request.QueryString.Add(key, nameValues.Get(i))
                Next
            End If
            '
            ' -- request form
            If True Then
                Dim nameValues As NameValueCollection = iisContext.Request.Form
                For i As Integer = 0 To nameValues.Count - 1
                    Dim key As String = nameValues.GetKey(i)
                    If String.IsNullOrWhiteSpace(key) Then
                        Continue For
                    End If
                    If context.Request.Headers.ContainsKey(key) Then
                        context.Request.Headers.Remove(key)
                    End If
                    context.Request.Form.Add(key, nameValues.Get(i))
                Next
            End If

            If True Then
                Dim filePtr As Integer = 0
                Dim instanceId As String = GenericController.getGUIDNaked()
                For Each key As String In iisContext.Request.Files.AllKeys
                    If String.IsNullOrWhiteSpace(key) Then
                        Continue For
                    End If
                    Dim file As HttpPostedFile = iisContext.Request.Files(key)
                    If file IsNot Nothing Then
                        Dim normalizedFilename As String = FileController.normalizeDosFilename(file.FileName)
                        If (file.ContentLength > 0) AndAlso (Not String.IsNullOrWhiteSpace(normalizedFilename)) Then
                            Dim docProperty As DocPropertyModel = New DocPropertyModel With {
                                    .name = key,
                                    .value = normalizedFilename,
                                    .nameValue = System.Uri.EscapeDataString(key) & "=" + System.Uri.EscapeDataString(normalizedFilename),
                                    .tempfilename = instanceId & "-" & filePtr.ToString() & ".bin",
                                    .propertyType = DocPropertyModel.DocPropertyTypesEnum.file
                                }
                            core.tempFiles.verifyPath(core.tempFiles.localAbsRootPath)
                            file.SaveAs(core.tempFiles.joinPath(core.tempFiles.localAbsRootPath, docProperty.tempfilename))
                            core.tempFiles.deleteOnDisposeFileList.Add(docProperty.tempfilename)
                            docProperty.fileSize = file.ContentLength
                            core.docProperties.setProperty(key, docProperty)
                            filePtr += 1
                        End If
                    End If
                Next
            End If

            If True Then
                For Each cookieKey As String In iisContext.Request.Cookies.Keys
                    If String.IsNullOrWhiteSpace(cookieKey) Then
                        Continue For
                    End If
                    Dim cookieValue As String = context.Request.Cookies(cookieKey).Value
                    cookieValue = System.Uri.EscapeDataString(cookieValue)
                    If (context.Request.Cookies.ContainsKey(cookieKey)) Then
                        context.Request.Cookies.Remove(cookieKey)
                    End If
                    context.Request.Cookies.Add(cookieKey, New HttpContextRequestCookie() With {
                        .Name = cookieKey,
                        .Value = cookieValue
                    })
                Next
            End If

            'If core.appConfig.appStatus.Equals(BaseModels.AppConfigBaseModel.AppStatusEnum.ok) Then
            '    core.html.enableOutputBuffer(True)
            '    core.doc.continueProcessing = True
            '    setResponseContentType("text/html")
            '    linkForwardSource = ""
            '    linkForwardError = ""
            '    requestReferer = requestReferrer
            '    requestPageReferer = requestReferrer
            '    core.doc.blockExceptionReporting = False
            '    Dim CookieDetectKey As String = core.docProperties.getText(rnCookieDetect)

            '    If Not String.IsNullOrEmpty(CookieDetectKey) Then
            '        Dim visitToken As SecurityController.TokenData = SecurityController.decodeToken(core, CookieDetectKey)

            '        If visitToken.id <> 0 Then
            '            Dim sql As String = "update ccvisits set CookieSupport=1 where id=" & visitToken.id
            '            core.db.executeNonQuery(sql)
            '            core.doc.continueProcessing = False
            '            Return core.doc.continueProcessing
            '        End If
            '    End If

            '    Dim updateDomainCache As Boolean = False
            '    core.domain.name = requestDomain
            '    core.domain.rootPageId = 0
            '    core.domain.noFollow = False
            '    core.domain.typeId = 1
            '    core.domain.visited = False
            '    core.domain.id = 0
            '    core.domain.forwardUrl = ""
            '    core.domainDictionary = core.cache.getObject(Of Dictionary(Of String, DomainModel))("domainContentList")

            '    If core.domainDictionary Is Nothing Then
            '        core.domainDictionary = DomainModel.createDictionary(core.cpParent, "(active<>0)and(name is not null)")
            '        updateDomainCache = True
            '    End If

            '    For Each domain As String In core.appConfig.domainList

            '        If Not core.domainDictionary.ContainsKey(domain.ToLowerInvariant()) Then
            '            LogController.logTrace(core, "adding domain record because configList domain not found [" & domain.ToLowerInvariant() & "]")
            '            Dim newDomain = DbBaseModel.addEmpty(Of DomainModel)(core.cpParent, 0)
            '            newDomain.name = domain
            '            newDomain.rootPageId = 0
            '            newDomain.noFollow = False
            '            newDomain.typeId = 1
            '            newDomain.visited = False
            '            newDomain.forwardUrl = ""
            '            newDomain.defaultTemplateId = 0
            '            newDomain.pageNotFoundPageId = 0
            '            newDomain.forwardDomainId = 0
            '            newDomain.defaultRouteId = core.siteProperties.getInteger("")
            '            newDomain.save(core.cpParent, 0)
            '            core.domainDictionary.Add(domain.ToLowerInvariant(), newDomain)
            '            updateDomainCache = True
            '        End If
            '    Next

            '    If Not core.domainDictionary.ContainsKey(requestDomain.ToLowerInvariant()) Then
            '        LogController.logTrace(core, "adding domain record because requestDomain [" & requestDomain.ToLowerInvariant() & "] not found")
            '        Dim newDomain = DomainModel.addEmpty(Of DomainModel)(core.cpParent, 0)
            '        newDomain.name = requestDomain
            '        newDomain.rootPageId = 0
            '        newDomain.noFollow = False
            '        newDomain.typeId = 1
            '        newDomain.visited = False
            '        newDomain.forwardUrl = ""
            '        newDomain.defaultTemplateId = 0
            '        newDomain.pageNotFoundPageId = 0
            '        newDomain.forwardDomainId = 0
            '        newDomain.save(core.cpParent, 0)
            '        core.domainDictionary.Add(requestDomain.ToLowerInvariant(), newDomain)
            '        updateDomainCache = True
            '    End If

            '    If updateDomainCache Then
            '        Dim dependencyKey As String = DbBaseModel.createDependencyKeyInvalidateOnChange(Of DomainModel)(core.cpParent)
            '        Dim dependencyKeyHash As CacheKeyHashClass = core.cache.createKeyHash(dependencyKey)
            '        Dim keyHashList As List(Of CacheKeyHashClass) = New List(Of CacheKeyHashClass) From {
            '                dependencyKeyHash
            '            }
            '        core.cache.storeObject("domainContentList", core.domainDictionary, keyHashList)
            '    End If

            '    core.domain = core.domainDictionary(requestDomain.ToLowerInvariant())

            '    If core.domain.id = 0 Then
            '        Dim domain = New DomainModel With {
            '                .name = requestDomain,
            '                .typeId = 1,
            '                .rootPageId = core.domain.rootPageId,
            '                .forwardUrl = core.domain.forwardUrl,
            '                .noFollow = core.domain.noFollow,
            '                .visited = core.domain.visited,
            '                .defaultTemplateId = core.domain.defaultTemplateId,
            '                .pageNotFoundPageId = core.domain.pageNotFoundPageId
            '            }
            '        domain.save(core.cpParent, 0)
            '        core.domain.id = domain.id
            '    End If

            '    If Not core.domain.visited Then
            '        core.db.executeNonQuery("update ccdomains set visited=1 where name=" & DbController.encodeSQLText(requestDomain))
            '        core.cache.invalidate("domainContentList")
            '    End If

            '    If core.domain.typeId = 1 Then
            '    ElseIf GenericController.strInstr(1, requestPathPage, "/" & core.appConfig.adminRoute, 1) <> 0 Then
            '    ElseIf core.domain.typeId.Equals(2) AndAlso Not String.IsNullOrEmpty(core.domain.forwardUrl) Then

            '        If GenericController.strInstr(1, core.domain.forwardUrl, "://") = 0 Then
            '            core.domain.forwardUrl = "http://" & core.domain.forwardUrl
            '        End If

            '        redirect(core.domain.forwardUrl, "Forwarding to [" & core.domain.forwardUrl & "] because the current domain [" + requestDomain & "] is in the domain content set to forward to this URL", False, False)
            '        Return core.doc.continueProcessing
            '    ElseIf (core.domain.typeId = 3) AndAlso (core.domain.forwardDomainId <> 0) AndAlso (core.domain.forwardDomainId <> core.domain.id) Then
            '        Dim forwardDomain As String = MetadataController.getRecordName(core, "domains", core.domain.forwardDomainId)

            '        If Not String.IsNullOrEmpty(forwardDomain) Then
            '            Dim pos As Integer = requestUrlSource.IndexOf(requestDomain, StringComparison.InvariantCultureIgnoreCase)

            '            If pos > 0 Then
            '                core.domain.forwardUrl = requestUrlSource.left(pos) & forwardDomain + requestUrlSource.Substring((pos + requestDomain.Length))
            '                redirect(core.domain.forwardUrl, "Forwarding to [" & core.domain.forwardUrl & "] because the current domain [" + requestDomain & "] is in the domain content set to forward to this replacement domain", False, False)
            '                Return core.doc.continueProcessing
            '            End If
            '        End If
            '    End If

            '    Dim originUri As Uri = HttpContext.Request.UrlReferrer

            '    If originUri IsNot Nothing Then

            '        If core.domainDictionary.ContainsKey(originUri.Host.ToLowerInvariant()) Then

            '            If core.domainDictionary(originUri.Host.ToLowerInvariant()).allowCORS Then
            '                HttpContext.Response.AddHeader("Access-Control-Allow-Credentials", "true")
            '                HttpContext.Response.AddHeader("Access-Control-Allow-Methods", "POST,GET,OPTIONS")
            '                HttpContext.Response.AddHeader("Access-Control-Headers", "Content-Type,soapaction,X-Requested-With")
            '                HttpContext.Response.AddHeader("Access-Control-Allow-Origin", originUri.GetLeftPart(UriPartial.Authority))
            '            End If
            '        End If
            '    End If

            '    If core.domain.noFollow Then
            '        response_NoFollow = True
            '    End If

            '    requestContentWatchPrefix = requestProtocol + requestDomain & "/"
            '    requestContentWatchPrefix = requestContentWatchPrefix.left(requestContentWatchPrefix.Length - 1)
            '    requestPath = "/"

            '    If String.IsNullOrWhiteSpace(requestPathPage) Then
            '        requestPage = core.siteProperties.serverPageDefault
            '    Else
            '        requestPage = requestPathPage
            '        Dim slashPtr As Integer = requestPathPage.LastIndexOf("/", StringComparison.InvariantCulture)

            '        If slashPtr >= 0 Then
            '            requestPage = ""
            '            requestPath = requestPathPage.left(slashPtr + 1)

            '            If requestPathPage.Length > 1 Then
            '                requestPage = requestPathPage.Substring(slashPtr + 1)
            '            End If
            '        End If
            '    End If

            '    requestSecureURLRoot = "https://" & requestDomain & "/"
            '    adminMessage = "For more information, please contact the <a href=""mailto:" & core.siteProperties.emailAdmin & "?subject=Re: " + requestDomain & """>Site Administrator</A>."

            '    If String.IsNullOrEmpty(serverFormActionURL) Then
            '        serverFormActionURL = requestProtocol + requestDomain + requestPath + requestPage
            '    End If
            'End If

        Catch ex As Exception
            LogController.logError(core, ex)
            Throw
        End Try

        Return core.doc.continueProcessing
    End Function


End Class

