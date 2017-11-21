
Option Explicit On
Option Strict On

Imports Microsoft.Web.Administration
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Namespace Contensive.Core.Controllers
    ''' <summary>
    ''' Code dedicated to processing iis input and output. lazy Constructed. (see coreHtmlClass for html processing)
    ''' What belongs here is everything that would have to change if we converted to apache
    ''' </summary>
    Public Class iisController
        '
        Dim cpCore As coreClass
        '
        ' if this instance is a webRole, retain pointer for callbacks
        '
        Public iisContext As System.Web.HttpContext
        '
        '   State values that must be initialized before Init()
        '   Everything else is derived from these
        '
        'Public Property initCounter As Integer = 0        '
        '
        ' -- Buffer request
        Public Property requestLanguage As String = ""            ' set externally from HTTP_Accept_LANGUAGE
        Public Property requestHttpAccept As String = ""
        Public Property requestHttpAcceptCharset As String = ""
        Public Property requestHttpProfile As String = ""
        Public Property requestxWapProfile As String = ""
        Public Property requestHTTPVia As String = ""                   ' informs the server of proxies used during the request
        Public Property requestHTTPFrom As String = ""                  ' contains the email address of the requestor
        Public Property requestPathPage As String = ""               ' The Path and Page part of the current URI
        Public Property requestReferrer As String = ""
        Public Property requestDomain As String = ""                 ' The Host part of the current URI
        Public Property requestSecure As Boolean = False             ' Set in InitASPEnvironment, true if https
        Public Property requestRemoteIP As String = ""               '
        Public Property requestBrowser As String = ""                ' The browser for this visit
        Public Property requestQueryString As String = ""            ' The QueryString of the current URI
        Public Property requestFormUseBinaryHeader As Boolean = False ' When set true with RequestNameBinaryRead=true, InitEnvironment reads the form in with a binary read
        Public Property requestFormBinaryHeader As Byte()            ' For asp pages, this is the full multipart header
        Public Property requestFormDict As New Dictionary(Of String, String)
        Public Property requestSpaceAsUnderscore As Boolean = False  ' when true, is it assumed that dots in request variable names will convert
        Public Property requestDotAsUnderscore As Boolean = False    ' (php converts spaces and dots to underscores)
        Public Property requestUrlSource As String = ""
        Public Property linkForwardSource As String = ""          ' main_ServerPathPage -- set during init
        Public Property linkForwardError As String = ""           ' always 404
        'Public Property readStreamJSForm As Boolean = False                  ' When true, the request comes from a browser handling a JSPage script line
        Public Property pageExcludeFromAnalytics As Boolean = False    ' For this page - true for remote methods and ajax
        '
        ' refactor - this method stears the stream between controllers, put it in cpcore
        'Public Property outStreamDevice As Integer = 0
        Public Property memberAction As Integer = 0              ' action to be performed during init
        Public Property adminMessage As String = ""          ' For more information message
        Public Property requestPageReferer As String = ""                    ' replaced by main_ServerReferrer
        Public Property requestReferer As String = ""
        Public Property serverFormActionURL As String = ""        ' The Action for all internal forms, if not set, default
        Public Property requestContentWatchPrefix As String = ""   ' The different between the URL and the main_ContentWatch Pathpage
        Public Property requestProtocol As String = ""             ' Set in InitASPEnvironment, http or https
        Public Property requestUrl As String = ""                 ' The current URL, from protocol to end of quesrystring
        Public Property requestVirtualFilePath As String = ""          ' The Virtual path for the site (host+main_ServerVirtualPath+"/" is site URI)
        Public Property requestPath As String = ""                 ' The path part of the current URI
        Public Property requestPage As String = ""                 ' The page part of the current URI
        Public Property requestSecureURLRoot As String = ""        ' The URL to the root of the secure area for this site
        '
        ' -- response
        Public Property response_NoFollow As Boolean = False   ' when set, Meta no follow is added
        '
        ' -- Buffer responses
        Public Property bufferRedirect As String = ""
        Public Property bufferContentType As String = ""
        Public Property bufferCookies As String = ""
        Public Property bufferResponseHeader As String = ""
        Public Property bufferResponseStatus As String = ""
        '------------------------------------------------------------------------
        '
        '   QueryString, Form and cookie Processing variables
        Public Class cookieClass
            Public name As String
            Public value As String
        End Class
        Public requestCookies As Dictionary(Of String, cookieClass)
        '
        '====================================================================================================
        '
        Public Sub New(cpCore As coreClass)
            MyBase.New
            Me.cpCore = cpCore
            requestCookies = New Dictionary(Of String, cookieClass)
        End Sub
        '
        '=======================================================================================
        '   IIS Reset
        '
        '   Must be called from a process running as admin
        '   This can be done using the command queue, which kicks off the ccCmd process from the Server
        '
        Public Sub reset()
            Try
                Dim Cmd As String
                Dim arg As String
                Dim LogFilename As String
                Dim Copy As String
                '
                Call Randomize()
                LogFilename = "Temp\" & genericController.encodeText(genericController.GetRandomInteger()) & ".Log"
                Cmd = "IISReset.exe"
                arg = "/restart >> """ & LogFilename & """"
                Call runProcess(cpCore, Cmd, arg, True)
                Copy = cpCore.privateFiles.readFile(LogFilename)
                Call cpCore.privateFiles.deleteFile(LogFilename)
                Copy = genericController.vbReplace(Copy, vbCrLf, "\n")
                Copy = genericController.vbReplace(Copy, vbCr, "\n")
                Copy = genericController.vbReplace(Copy, vbLf, "\n")
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '=======================================================================================
        '   Stop IIS
        '
        '   Must be called from a process running as admin
        '   This can be done using the command queue, which kicks off the ccCmd process from the Server
        '
        Public Sub [stop]()
            Try
                Dim Cmd As String
                Dim LogFilename As String
                Dim Copy As String
                '
                Call Randomize()
                LogFilename = "Temp\" & genericController.encodeText(genericController.GetRandomInteger()) & ".Log"
                Cmd = "%comspec% /c IISReset /stop >> """ & LogFilename & """"
                Call runProcess(cpCore, Cmd, , True)
                Copy = cpCore.privateFiles.readFile(LogFilename)
                Call cpCore.privateFiles.deleteFile(LogFilename)
                Copy = genericController.vbReplace(Copy, vbCrLf, "\n")
                Copy = genericController.vbReplace(Copy, vbCr, "\n")
                Copy = genericController.vbReplace(Copy, vbLf, "\n")
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '=======================================================================================
        '   Start IIS
        '
        '   Must be called from a process running as admin
        '   This can be done using the command queue, which kicks off the ccCmd process from the Server
        '=======================================================================================
        '
        Public Sub start()
            Try
                Dim Cmd As String
                Dim LogFilename As String = cpCore.privateFiles.rootLocalPath & "iisResetPipe.log"
                Dim Copy As String
                '
                Call Randomize()
                Cmd = "%comspec% /c IISReset /start >> """ & LogFilename & """"
                Call runProcess(cpCore, Cmd, , True)
                Copy = cpCore.privateFiles.readFile(LogFilename)
                Call cpCore.privateFiles.deleteFile(LogFilename)
                Copy = genericController.vbReplace(Copy, vbCrLf, "\n")
                Copy = genericController.vbReplace(Copy, vbCr, "\n")
                Copy = genericController.vbReplace(Copy, vbLf, "\n")
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '=======================================================================================
        ' recycle iis process
        '
        Public Sub recycle(appName As String)
            Try
                Dim serverManager As ServerManager
                Dim appPoolColl As ApplicationPoolCollection
                '
                serverManager = New ServerManager
                appPoolColl = serverManager.ApplicationPools
                For Each appPool As ApplicationPool In appPoolColl
                    If appPool.Name.ToLower = appName.ToLower Then
                        If appPool.Start = ObjectState.Started Then
                            appPool.Recycle()
                        End If
                    End If
                Next
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '==================================================================================
        '   Initialize the application
        '       returns responseOpen
        '==================================================================================
        '
        Public Function initWebContext(httpContext As System.Web.HttpContext) As Boolean
            Try
                '
                ' -- setup IIS Response
                iisContext = httpContext
                iisContext.Response.CacheControl = "no-cache"
                iisContext.Response.Expires = -1
                iisContext.Response.Buffer = True
                ''
                '
                ' -- basic request environment
                requestDomain = iisContext.Request.ServerVariables("SERVER_NAME")
                requestPathPage = CStr(iisContext.Request.ServerVariables("SCRIPT_NAME"))
                requestReferrer = CStr(iisContext.Request.ServerVariables("HTTP_REFERER"))
                requestSecure = CBool(iisContext.Request.ServerVariables("SERVER_PORT_SECURE"))
                requestRemoteIP = CStr(iisContext.Request.ServerVariables("REMOTE_ADDR"))
                requestBrowser = CStr(iisContext.Request.ServerVariables("HTTP_USER_AGENT"))
                requestLanguage = CStr(iisContext.Request.ServerVariables("HTTP_ACCEPT_LANGUAGE"))
                requestHttpAccept = CStr(iisContext.Request.ServerVariables("HTTP_ACCEPT"))
                requestHttpAcceptCharset = CStr(iisContext.Request.ServerVariables("HTTP_ACCEPT_CHARSET"))
                requestHttpProfile = CStr(iisContext.Request.ServerVariables("HTTP_PROFILE"))
                '
                ' -- http QueryString
                If (iisContext.Request.QueryString.Count > 0) Then
                    requestQueryString = ""
                    For Each key As String In iisContext.Request.QueryString
                        Dim keyValue As String = iisContext.Request.QueryString(key)
                        cpCore.docProperties.setProperty(key, keyValue)
                        requestQueryString = genericController.ModifyQueryString(requestQueryString, key, keyValue)
                    Next
                End If
                '
                ' -- form
                requestFormDict.Clear()
                For Each key As String In iisContext.Request.Form.Keys
                    Dim keyValue As String = iisContext.Request.Form(key)
                    cpCore.docProperties.setProperty(key, keyValue, True)
                    If requestFormDict.ContainsKey(keyValue) Then
                        requestFormDict.Remove(keyValue)
                    End If
                    requestFormDict.Add(key, keyValue)
                Next
                '
                ' -- handle files
                Dim filePtr As Integer = 0
                Dim instanceId As String = genericController.createGuid().Replace("{", "").Replace("-", "").Replace("}", "")
                Dim formNames As String() = iisContext.Request.Files.AllKeys
                For Each formName As String In formNames
                    Dim file As System.Web.HttpPostedFile = iisContext.Request.Files(formName)
                    If (file IsNot Nothing) Then
                        If (file.ContentLength > 0) And (Not String.IsNullOrEmpty(file.FileName)) Then
                            Dim prop As New docPropertiesClass
                            prop.Name = formName
                            prop.Value = file.FileName
                            prop.NameValue = EncodeRequestVariable(prop.Name) & "=" & EncodeRequestVariable(prop.Value)
                            prop.IsFile = True
                            prop.IsForm = True
                            prop.tempfilename = instanceId & "-" & filePtr.ToString() & ".bin"
                            file.SaveAs(cpCore.tempFiles.joinPath(cpCore.tempFiles.rootLocalPath, prop.tempfilename))
                            cpCore.tempFiles.deleteOnDisposeFileList.Add(prop.tempfilename)
                            prop.FileSize = CInt(file.ContentLength)
                            cpCore.docProperties.setProperty(formName, prop)
                            filePtr += 1
                        End If
                    End If
                Next
                '
                ' load request cookies
                '
                For Each key As String In iisContext.Request.Cookies
                    Dim keyValue As String = iisContext.Request.Cookies(key).Value
                    keyValue = DecodeResponseVariable(keyValue)
                    addRequestCookie(key, keyValue)
                Next
                '
                '--------------------------------------------------------------------------
                '
                If (cpCore.serverConfig.appConfig.appStatus <> Models.Entity.serverConfigModel.appStatusEnum.OK) Then
                    '
                    ' did not initialize correctly
                    '
                Else
                    '
                    ' continue
                    '
                    'initCounter += 1
                    '
                    Call cpCore.html.enableOutputBuffer(True)
                    cpCore.doc.continueProcessing = True
                    Call setResponseContentType("text/html")
                    '
                    '--------------------------------------------------------------------------
                    ' ----- Process QueryString to cpcore.doc.main_InStreamArray
                    '       Do this first to set cpcore.main_ReadStreamJSForm, cpcore.main_ReadStreamJSProcess, cpcore.main_ReadStreamBinaryRead (must be in QS)
                    '--------------------------------------------------------------------------
                    '
                    linkForwardSource = ""
                    linkForwardError = ""
                    '
                    ' start with the best guess for the source url, then improve the guess based on what iis might have done
                    '
                    requestUrlSource = "http://"
                    If requestSecure Then
                        requestUrlSource = "https://"
                    End If
                    requestUrlSource = requestUrlSource & requestDomain & requestPathPage
                    If requestQueryString <> "" Then
                        requestUrlSource = requestUrlSource & "?" & requestQueryString
                    End If
                    If requestQueryString <> "" Then
                        '
                        ' Add query string to stream
                        '
                        Call cpCore.docProperties.addQueryString(requestQueryString)
                    End If
                    '
                    ' Other Server variables
                    requestReferer = requestReferrer
                    requestPageReferer = requestReferrer
                    '
                    If requestSecure Then
                        requestProtocol = "https://"
                    Else
                        requestProtocol = "http://"
                    End If
                    '
                    cpCore.doc.blockExceptionReporting = False
                    '
                    '   javascript cookie detect on page1 of all visits
                    Dim CookieDetectKey As String = cpCore.docProperties.getText(RequestNameCookieDetectVisitID)
                    If CookieDetectKey <> "" Then
                        '
                        Dim cookieDetectDate As Date = New Date
                        Dim CookieDetectVisitId As Integer = 0
                        Call cpCore.security.decodeToken(CookieDetectKey, CookieDetectVisitId, cookieDetectDate)
                        If CookieDetectVisitId <> 0 Then
                            Call cpCore.db.executeQuery("update ccvisits set CookieSupport=1 where id=" & CookieDetectVisitId)
                            cpCore.doc.continueProcessing = False '--- should be disposed by caller --- Call dispose
                            Return cpCore.doc.continueProcessing
                        End If
                    End If
                    '
                    '   verify Domain table entry
                    Dim updateDomainCache As Boolean = False
                    '
                    cpCore.domainLegacyCache.domainDetails.name = requestDomain
                    cpCore.domainLegacyCache.domainDetails.rootPageId = 0
                    cpCore.domainLegacyCache.domainDetails.noFollow = False
                    cpCore.domainLegacyCache.domainDetails.typeId = 1
                    cpCore.domainLegacyCache.domainDetails.visited = False
                    cpCore.domainLegacyCache.domainDetails.id = 0
                    cpCore.domainLegacyCache.domainDetails.forwardUrl = ""
                    '
                    ' REFACTOR -- move to cpcore.domains class 
                    cpCore.domainLegacyCache.domainDetailsList = cpCore.cache.getObject(Of Dictionary(Of String, Models.Entity.domainLegacyModel.domainDetailsClass))("domainContentList")
                    If (cpCore.domainLegacyCache.domainDetailsList Is Nothing) Then
                        '
                        '  no cache found, build domainContentList from database
                        cpCore.domainLegacyCache.domainDetailsList = New Dictionary(Of String, Models.Entity.domainLegacyModel.domainDetailsClass)
                        Dim domainList As List(Of Models.Entity.domainModel) = Models.Entity.domainModel.createList(cpCore, "(active<>0)and(name is not null)")
                        For Each domain In domainList
                            Dim domainDetailsNew As New Models.Entity.domainLegacyModel.domainDetailsClass
                            domainDetailsNew.name = domain.name
                            domainDetailsNew.rootPageId = domain.RootPageID
                            domainDetailsNew.noFollow = domain.NoFollow
                            domainDetailsNew.typeId = domain.TypeID
                            domainDetailsNew.visited = domain.Visited
                            domainDetailsNew.id = domain.id
                            domainDetailsNew.forwardUrl = domain.ForwardURL
                            domainDetailsNew.defaultTemplateId = domain.DefaultTemplateId
                            domainDetailsNew.pageNotFoundPageId = domain.PageNotFoundPageID
                            domainDetailsNew.forwardDomainId = domain.forwardDomainId
                            If (cpCore.domainLegacyCache.domainDetailsList.ContainsKey(domain.name.ToLower())) Then
                                '
                                logController.appendLog(cpCore, "Duplicate domain record found when adding domains from table [" & domain.name.ToLower() & "], duplicate skipped.")
                                '
                            Else
                                cpCore.domainLegacyCache.domainDetailsList.Add(domain.name.ToLower(), domainDetailsNew)
                            End If
                        Next
                        updateDomainCache = True
                    End If
                    '
                    ' verify app config domainlist is in the domainlist cache
                    '
                    For Each domain As String In cpCore.serverConfig.appConfig.domainList
                        If Not cpCore.domainLegacyCache.domainDetailsList.ContainsKey(domain.ToLower()) Then
                            Dim domainDetailsNew As New Models.Entity.domainLegacyModel.domainDetailsClass
                            domainDetailsNew.name = domain
                            domainDetailsNew.rootPageId = 0
                            domainDetailsNew.noFollow = False
                            domainDetailsNew.typeId = 1
                            domainDetailsNew.visited = False
                            domainDetailsNew.id = 0
                            domainDetailsNew.forwardUrl = ""
                            domainDetailsNew.defaultTemplateId = 0
                            domainDetailsNew.pageNotFoundPageId = 0
                            domainDetailsNew.forwardDomainId = 0
                            If (cpCore.domainLegacyCache.domainDetailsList.ContainsKey(domain.ToLower())) Then
                                '
                                logController.appendLog(cpCore, "Duplicate domain record found when adding appConfig.domainList [" & domain.ToLower() & "], duplicate skipped")
                                '
                            Else
                                cpCore.domainLegacyCache.domainDetailsList.Add(domain.ToLower(), domainDetailsNew)
                            End If
                        End If
                    Next
                    If Not cpCore.domainLegacyCache.domainDetailsList.ContainsKey(requestDomain.ToLower()) Then
                        '
                        ' -- domain not found
                        ' -- current host not in domainContent, add it and re-save the cache
                        Dim domainDetailsNew As New Models.Entity.domainLegacyModel.domainDetailsClass
                        domainDetailsNew.name = requestDomain
                        domainDetailsNew.rootPageId = 0
                        domainDetailsNew.noFollow = False
                        domainDetailsNew.typeId = 1
                        domainDetailsNew.visited = False
                        domainDetailsNew.id = 0
                        domainDetailsNew.forwardUrl = ""
                        domainDetailsNew.defaultTemplateId = 0
                        domainDetailsNew.pageNotFoundPageId = 0
                        domainDetailsNew.forwardDomainId = 0
                        cpCore.domainLegacyCache.domainDetailsList.Add(requestDomain.ToLower(), domainDetailsNew)
                        '
                        ' -- update database
                        Dim domain As Models.Entity.domainModel = Models.Entity.domainModel.add(cpCore, New List(Of String))
                        cpCore.domainLegacyCache.domainDetails.id = domain.id
                        domain.name = requestDomain
                        domain.TypeID = 1
                        domain.save(cpCore)
                        '
                        updateDomainCache = True
                    End If
                    '
                    ' domain found
                    '
                    cpCore.domainLegacyCache.domainDetails = cpCore.domainLegacyCache.domainDetailsList(requestDomain.ToLower())
                    If (cpCore.domainLegacyCache.domainDetails.id = 0) Then
                        '
                        ' this is a default domain or a new domain -- add to the domain table
                        '
                        Dim domain As New Models.Entity.domainModel() With {
                                .name = requestDomain,
                                .TypeID = 1,
                                .RootPageID = cpCore.domainLegacyCache.domainDetails.rootPageId,
                                .ForwardURL = cpCore.domainLegacyCache.domainDetails.forwardUrl,
                                .NoFollow = cpCore.domainLegacyCache.domainDetails.noFollow,
                                .Visited = cpCore.domainLegacyCache.domainDetails.visited,
                                .DefaultTemplateId = cpCore.domainLegacyCache.domainDetails.defaultTemplateId,
                                .PageNotFoundPageID = cpCore.domainLegacyCache.domainDetails.pageNotFoundPageId
                            }
                        cpCore.domainLegacyCache.domainDetails.id = domain.id
                    End If
                    If Not cpCore.domainLegacyCache.domainDetails.visited Then
                        '
                        ' set visited true
                        '
                        Call cpCore.db.executeQuery("update ccdomains set visited=1 where name=" & cpCore.db.encodeSQLText(requestDomain))
                        Call cpCore.cache.setContent("domainContentList", "", "domains")
                    End If
                    If cpCore.domainLegacyCache.domainDetails.typeId = 1 Then
                        '
                        ' normal domain, leave it
                        '
                    ElseIf genericController.vbInstr(1, requestPathPage, "/" & cpCore.serverConfig.appConfig.adminRoute, vbTextCompare) <> 0 Then
                        '
                        ' forwarding does not work in the admin site
                        '
                    ElseIf (cpCore.domainLegacyCache.domainDetails.typeId = 2) And (cpCore.domainLegacyCache.domainDetails.forwardUrl <> "") Then
                        '
                        ' forward to a URL
                        '
                        '
                        'Call AppendLog("main_init(), 1710 - exit for domain forward")
                        '
                        If genericController.vbInstr(1, cpCore.domainLegacyCache.domainDetails.forwardUrl, "://") = 0 Then
                            cpCore.domainLegacyCache.domainDetails.forwardUrl = "http://" & cpCore.domainLegacyCache.domainDetails.forwardUrl
                        End If
                        Call redirect(cpCore.domainLegacyCache.domainDetails.forwardUrl, "Forwarding to [" & cpCore.domainLegacyCache.domainDetails.forwardUrl & "] because the current domain [" & requestDomain & "] is in the domain content set to forward to this URL", False)
                        Return cpCore.doc.continueProcessing
                    ElseIf (cpCore.domainLegacyCache.domainDetails.typeId = 3) And (cpCore.domainLegacyCache.domainDetails.forwardDomainId <> 0) And (cpCore.domainLegacyCache.domainDetails.forwardDomainId <> cpCore.domainLegacyCache.domainDetails.id) Then
                        '
                        ' forward to a replacement domain
                        '
                        Dim forwardDomain As String = cpCore.db.getRecordName("domains", cpCore.domainLegacyCache.domainDetails.forwardDomainId)
                        If forwardDomain <> "" Then
                            Dim pos As Integer = genericController.vbInstr(1, requestUrlSource, requestDomain, vbTextCompare)
                            If (pos > 0) Then
                                cpCore.domainLegacyCache.domainDetails.forwardUrl = Mid(requestUrlSource, 1, pos - 1) & forwardDomain & Mid(requestUrlSource, pos + Len(requestDomain))
                                Call redirect(cpCore.domainLegacyCache.domainDetails.forwardUrl, "Forwarding to [" & cpCore.domainLegacyCache.domainDetails.forwardUrl & "] because the current domain [" & requestDomain & "] is in the domain content set to forward to this replacement domain", False)
                                Return cpCore.doc.continueProcessing
                            End If
                        End If
                    End If
                    If cpCore.domainLegacyCache.domainDetails.noFollow Then
                        response_NoFollow = True
                    End If
                    If (updateDomainCache) Then
                        '
                        ' if there was a change, update the cache
                        '
                        Call cpCore.cache.setContent("domainContentList", cpCore.domainLegacyCache.domainDetailsList, "domains")
                        'domainDetailsListText = cpCore.json.Serialize(cpCore.domains.domainDetailsList)
                        'Call cpCore.cache.setObject("domainContentList", domainDetailsListText, "domains")
                    End If
                    '
                    requestVirtualFilePath = "/" & cpCore.serverConfig.appConfig.name
                    '
                    requestContentWatchPrefix = requestProtocol & requestDomain & requestAppRootPath
                    requestContentWatchPrefix = Mid(requestContentWatchPrefix, 1, Len(requestContentWatchPrefix) - 1)
                    '
                    requestPath = "/"
                    requestPage = cpCore.siteProperties.serverPageDefault
                    Dim TextStartPointer As Integer = InStrRev(requestPathPage, "/")
                    If TextStartPointer <> 0 Then
                        requestPath = Mid(requestPathPage, 1, TextStartPointer)
                        requestPage = Mid(requestPathPage, TextStartPointer + 1)
                    End If
                    requestSecureURLRoot = "https://" & requestDomain & requestAppRootPath
                    '
                    ' ----- Create Server Link property
                    '
                    requestUrl = requestProtocol & requestDomain & requestAppRootPath & requestPath & requestPage
                    If requestQueryString <> "" Then
                        requestUrl = requestUrl & "?" & requestQueryString
                    End If
                    If requestUrlSource = "" Then
                        requestUrlSource = requestUrl
                    End If
                    '
                    ' ----- Style tag
                    adminMessage = "For more information, please contact the <a href=""mailto:" & cpCore.siteProperties.emailAdmin & "?subject=Re: " & requestDomain & """>Site Administrator</A>."
                    '
                    requestUrl = requestProtocol & requestDomain & requestAppRootPath & requestPath & requestPage
                    If requestQueryString <> "" Then
                        requestUrl = requestUrl & "?" & requestQueryString
                    End If
                    '
                    If (LCase(requestDomain) <> genericController.vbLCase(requestDomain)) Then
                        Dim Copy As String = "Redirecting to domain [" & requestDomain & "] because this site is configured to run on the current domain [" & requestDomain & "]"
                        If requestQueryString <> "" Then
                            Call redirect(requestProtocol & requestDomain & requestPath & requestPage & "?" & requestQueryString, Copy, False)
                        Else
                            Call redirect(requestProtocol & requestDomain & requestPath & requestPage, Copy, False)
                        End If
                        cpCore.doc.continueProcessing = False '--- should be disposed by caller --- Call dispose
                        Return cpCore.doc.continueProcessing
                    End If
                    '
                    ' ----- Create cpcore.main_ServerFormActionURL if it has not been overridden manually
                    If serverFormActionURL = "" Then
                        serverFormActionURL = requestProtocol & requestDomain & requestPath & requestPage
                    End If
                End If
                '
                ' -- done at last
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return cpCore.doc.continueProcessing
        End Function
        '
        '========================================================================
        ' Read a cookie to the stream
        '
        Public Function getRequestCookie(ByVal CookieName As String) As String
            Dim cookieValue As String = ""
            Try
                If requestCookies.ContainsKey(CookieName) Then
                    cookieValue = requestCookies(CookieName).value
                End If
                ''
                'Dim Pointer As Integer
                'Dim UName As String
                ''
                'web_GetStreamCookie = ""
                'If web.cookieArrayCount > 0 Then
                '    UName = genericController.vbUCase(CookieName)
                '    For Pointer = 0 To web.cookieArrayCount - 1
                '        If UName = genericController.vbUCase(web.requestCookies(Pointer).Name) Then
                '            web_GetStreamCookie = web.requestCookies(Pointer).Value
                '            Exit For
                '        End If
                '    Next
                'End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return cookieValue
        End Function
        '
        '====================================================================================================
        '
        Public Sub addRequestCookie(cookieKey As String, cookieValue As String)
            If requestCookies.ContainsKey(cookieKey) Then
                '
            Else
                Dim newCookie As New iisController.cookieClass
                newCookie.name = cookieKey
                newCookie.value = cookieValue
                requestCookies.Add(cookieKey, newCookie)
            End If
        End Sub
        '
        '========================================================================
        ' Write a cookie to the stream
        '========================================================================
        '
        Public Sub addResponseCookie(ByVal CookieName As String, ByVal CookieValue As String, Optional ByVal DateExpires As Date = Nothing, Optional ByVal domain As String = "", Optional ByVal Path As String = "", Optional ByVal Secure As Boolean = False)
            Try
                Dim iCookieName As String
                Dim iCookieValue As String
                Dim MethodName As String
                Dim s As String
                Dim usedDomainList As String = ""
                '
                iCookieName = genericController.encodeText(CookieName)
                iCookieValue = genericController.encodeText(CookieValue)
                '
                MethodName = "main_addResponseCookie"
                '
                If cpCore.doc.continueProcessing Then
                    'If cpCore.doc.continueProcessing And cpCore.doc.outputBufferEnabled Then
                    If (False) Then
                        ''
                        '' no domain provided, new mode
                        ''   - write cookie for current domains
                        ''   - write an iframe that called the cross-Site login
                        ''   - http://127.0.0.1/ccLib/clientside/cross.html?v=1&vPath=%2F&vExpires=1%2F1%2F2012
                        ''
                        'domainListSplit = Split(cpCore.main_ServerDomainCrossList, ",")
                        'For Ptr = 0 To UBound(domainListSplit)
                        '    domainSet = Trim(domainListSplit(Ptr))
                        '    If (domainSet <> "") And (InStr(1, "," & usedDomainList & ",", "," & domainSet & ",", vbTextCompare) = 0) Then
                        '        usedDomainList = usedDomainList & "," & domainSet
                        '        '
                        '        ' valid, non-repeat domain
                        '        '
                        '        If genericController.vbLCase(domainSet) = genericController.vbLCase(requestDomain) Then
                        '            '
                        '            ' current domain, set cookie
                        '            '
                        '            If (iisContext IsNot Nothing) Then
                        '                '
                        '                ' Pass cookie to asp (compatibility)
                        '                '
                        '                iisContext.Response.Cookies(iCookieName).Value = iCookieValue
                        '                If Not isMinDate(DateExpires) Then
                        '                    iisContext.Response.Cookies(iCookieName).Expires = DateExpires
                        '                End If
                        '                'main_ASPResponse.Cookies(iCookieName).domain = domainSet
                        '                If Not isMissing(Path) Then
                        '                    iisContext.Response.Cookies(iCookieName).Path = genericController.encodeText(Path)
                        '                End If
                        '                If Not isMissing(Secure) Then
                        '                    iisContext.Response.Cookies(iCookieName).Secure = Secure
                        '                End If
                        '            Else
                        '                '
                        '                ' Pass Cookie to non-asp parent
                        '                '   crlf delimited list of name,value,expires,domain,path,secure
                        '                '
                        '                If webServerIO_bufferCookies <> "" Then
                        '                    webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf
                        '                End If
                        '                webServerIO_bufferCookies = webServerIO_bufferCookies & CookieName
                        '                webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf & iCookieValue
                        '                '
                        '                s = ""
                        '                If Not isMinDate(DateExpires) Then
                        '                    s = DateExpires.ToString
                        '                End If
                        '                webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf & s
                        '                ' skip bc this is exactly the current domain and /rfc2109 requires a leading dot if explicit
                        '                webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf
                        '                'responseBufferCookie = responseBufferCookie & vbCrLf & domainSet
                        '                '
                        '                s = "/"
                        '                If Not isMissing(Path) Then
                        '                    s = genericController.encodeText(Path)
                        '                End If
                        '                webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf & s
                        '                '
                        '                s = "false"
                        '                If genericController.EncodeBoolean(Secure) Then
                        '                    s = "true"
                        '                End If
                        '                webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf & s
                        '            End If
                        '        Else
                        '            '
                        '            ' other domain, add iframe
                        '            '
                        '            Dim C As String
                        '            Link = "http://" & domainSet & "/ccLib/clientside/cross.html"
                        '            Link = Link & "?n=" & EncodeRequestVariable(iCookieName)
                        '            Link = Link & "&v=" & EncodeRequestVariable(iCookieValue)
                        '            If Not isMissing(Path) Then
                        '                C = genericController.encodeText(Path)
                        '                C = EncodeRequestVariable(C)
                        '                C = genericController.vbReplace(C, "/", "%2F")
                        '                Link = Link & "&p=" & C
                        '            End If
                        '            If Not isMinDate(DateExpires) Then
                        '                C = genericController.encodeText(DateExpires)
                        '                C = EncodeRequestVariable(C)
                        '                C = genericController.vbReplace(C, "/", "%2F")
                        '                Link = Link & "&e=" & C
                        '            End If
                        '            Link = cpCore.htmlDoc.html_EncodeHTML(Link)
                        '            cpCore.htmlDoc.htmlForEndOfBody = cpCore.htmlDoc.htmlForEndOfBody & vbCrLf & vbTab & "<iframe style=""display:none;"" width=""0"" height=""0"" src=""" & Link & """></iframe>"
                        '        End If
                        '    End If
                        'Next
                    Else
                        '
                        ' Legacy mode - if no domain given just leave it off
                        '
                        If (iisContext IsNot Nothing) Then
                            '
                            ' Pass cookie to asp (compatibility)
                            '
                            iisContext.Response.Cookies(iCookieName).Value = iCookieValue
                            If Not isMinDate(DateExpires) Then
                                iisContext.Response.Cookies(iCookieName).Expires = DateExpires
                            End If
                            'main_ASPResponse.Cookies(iCookieName).domain = domainSet
                            If Not isMissing(Path) Then
                                iisContext.Response.Cookies(iCookieName).Path = genericController.encodeText(Path)
                            End If
                            If Not isMissing(Secure) Then
                                iisContext.Response.Cookies(iCookieName).Secure = Secure
                            End If
                        Else
                            '
                            ' Pass Cookie to non-asp parent
                            '   crlf delimited list of name,value,expires,domain,path,secure
                            '
                            If bufferCookies <> "" Then
                                bufferCookies = bufferCookies & vbCrLf
                            End If
                            bufferCookies = bufferCookies & CookieName
                            bufferCookies = bufferCookies & vbCrLf & iCookieValue
                            '
                            s = ""
                            If Not isMinDate(DateExpires) Then
                                s = DateExpires.ToString
                            End If
                            bufferCookies = bufferCookies & vbCrLf & s
                            '
                            s = ""
                            If Not isMissing(domain) Then
                                s = genericController.encodeText(domain)
                            End If
                            bufferCookies = bufferCookies & vbCrLf & s
                            '
                            s = "/"
                            If Not isMissing(Path) Then
                                s = genericController.encodeText(Path)
                            End If
                            bufferCookies = bufferCookies & vbCrLf & s
                            '
                            s = "false"
                            If genericController.EncodeBoolean(Secure) Then
                                s = "true"
                            End If
                            bufferCookies = bufferCookies & vbCrLf & s
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '
        '
        Public Sub setResponseStatus(status As String)
            bufferResponseStatus = status
        End Sub
        '
        '
        '
        Public Sub setResponseContentType(ContentType As Object)
            bufferContentType = CStr(ContentType)
        End Sub
        '
        '
        '
        Public Sub addResponseHeader(HeaderName As Object, HeaderValue As Object)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SetStreamHeader")
            '
            If cpCore.doc.continueProcessing Then
                If bufferResponseHeader <> "" Then
                    bufferResponseHeader = bufferResponseHeader & vbCrLf
                End If
                bufferResponseHeader = bufferResponseHeader _
                    & genericController.vbReplace(genericController.encodeText(HeaderName), vbCrLf, "") _
                    & vbCrLf & genericController.vbReplace(genericController.encodeText(HeaderValue), vbCrLf, "")
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_SetStreamHeader")
            '
        End Sub
        '
        '===========================================================================================
        ''' <summary>
        ''' redirect
        ''' </summary>
        ''' <param name="NonEncodedLink"></param>
        ''' <param name="RedirectReason"></param>
        ''' <param name="IsPageNotFound"></param>
        Public Function redirect(ByVal NonEncodedLink As String, Optional ByVal RedirectReason As String = "No explaination provided", Optional ByVal IsPageNotFound As Boolean = False) As String
            Dim result As String = ""
            Try
                Const rnRedirectCycleFlag = "cycleFlag"
                Dim EncodedLink As String
                Dim Copy As String
                Dim ShortLink As String = String.Empty
                Dim FullLink As String
                Dim redirectCycles As Integer
                '
                If cpCore.doc.continueProcessing Then
                    redirectCycles = cpCore.docProperties.getInteger(rnRedirectCycleFlag)
                    '
                    ' convert link to a long link on this domain
                    If (NonEncodedLink.Substring(0, 4).ToLower() = "http") Then
                        FullLink = NonEncodedLink
                    Else
                        If (NonEncodedLink.Substring(0, 1).ToLower() = "/") Then
                            '
                            ' -- root relative - url starts with path, let it go
                        ElseIf (NonEncodedLink.Substring(0, 1).ToLower() = "?") Then
                            '
                            ' -- starts with qs, fix issue where iis consideres this on the physical page, not the link-alias vitrual route
                            NonEncodedLink = requestPathPage & NonEncodedLink
                        Else
                            '
                            ' -- url starts with the page
                            NonEncodedLink = requestPath & NonEncodedLink
                        End If
                        ShortLink = NonEncodedLink
                        ShortLink = genericController.ConvertLinkToShortLink(ShortLink, requestDomain, requestVirtualFilePath)
                        ShortLink = genericController.EncodeAppRootPath(ShortLink, requestVirtualFilePath, requestAppRootPath, requestDomain)
                        FullLink = requestProtocol & requestDomain & ShortLink
                    End If

                    If (NonEncodedLink = "") Then
                        '
                        ' Link is not valid
                        '
                        cpCore.handleException(New ApplicationException("Redirect was called with a blank Link. Redirect Reason [" & RedirectReason & "]"))
                        Return String.Empty
                        '
                        ' changed to main_ServerLinksource because if a redirect is caused by a link forward, and the host page for the iis 404 is
                        ' the same as the destination of the link forward, this throws an error and does not forward. the only case where main_ServerLinksource is different
                        ' then main_ServerLink is the linkfforward/linkalias case.
                        '
                    ElseIf (requestFormDict.Count = 0) And (requestUrlSource = FullLink) Then
                        '
                        ' Loop redirect error, throw trap and block redirect to prevent loop
                        '
                        cpCore.handleException(New ApplicationException("Redirect was called to the same URL, main_ServerLink is [" & requestUrl & "], main_ServerLinkSource is [" & requestUrlSource & "]. This redirect is only allowed if either the form or querystring has change to prevent cyclic redirects. Redirect Reason [" & RedirectReason & "]"))
                        Return String.Empty
                    ElseIf IsPageNotFound Then
                        '
                        ' Do a PageNotFound then redirect
                        '
                        Call logController.log_appendLogPageNotFound(cpCore, requestUrlSource)
                        If ShortLink <> "" Then
                            Call cpCore.db.executeQuery("Update ccContentWatch set link=null where link=" & cpCore.db.encodeSQLText(ShortLink))
                        End If
                        '
                        If cpCore.doc.testPointPrinting Then
                            '
                            ' -- Verbose - do not redirect, just print the link
                            EncodedLink = NonEncodedLink
                        Else
                            Call setResponseStatus("404 Not Found")
                        End If
                    Else

                        '
                        ' Go ahead and redirect
                        '
                        Copy = """" & FormatDateTime(cpCore.doc.profileStartTime, vbGeneralDate) & """,""" & requestDomain & """,""" & requestUrlSource & """,""" & NonEncodedLink & """,""" & RedirectReason & """"
                        logController.appendLog(cpCore, Copy, "performance", "redirects")
                        '
                        If cpCore.doc.testPointPrinting Then
                            '
                            ' -- Verbose - do not redirect, just print the link
                            EncodedLink = NonEncodedLink
                            result = "<div style=""padding:20px;border:1px dashed black;background-color:white;color:black;"">" & RedirectReason & "<p>Click to continue the redirect to <a href=" & EncodedLink & ">" & genericController.encodeHTML(NonEncodedLink) & "</a>...</p></div>"
                        Else
                            '
                            ' -- Redirect now
                            Call clearResponseBuffer()
                            If (Not iisContext Is Nothing) Then
                                '
                                ' -- redirect and release application. HOWEVER -- the thread will continue so use responseOpen=false to abort as much activity as possible
                                iisContext.Response.Redirect(NonEncodedLink, False)
                                iisContext.ApplicationInstance.CompleteRequest()
                            Else
                                bufferRedirect = NonEncodedLink
                            End If
                        End If
                    End If
                    '
                    ' -- close the output stream
                    cpCore.doc.continueProcessing = False
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function

        '
        '
        Public Sub flushStream()
            If (iisContext IsNot Nothing) Then
                iisContext.Response.Flush()
            End If
        End Sub
        ''
        ''====================================================================================================
        ''
        'Private Structure fieldTypePrivate
        '    Dim Name As String
        '    Dim fieldTypePrivate As Integer
        'End Structure
        '
        '====================================================================================================
        ''' <summary>
        ''' Verify a site exists, it not add it, it is does, verify all its settings
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="appName"></param>
        ''' <param name="DomainName"></param>
        ''' <param name="rootPublicFilesPath"></param>
        ''' <param name="defaultDocOrBlank"></param>
        ''' '
        Public Shared Sub verifySite(cpCore As coreClass, ByVal appName As String, ByVal DomainName As String, ByVal rootPublicFilesPath As String, ByVal defaultDocOrBlank As String)
            Try
                verifyAppPool(cpCore, appName)
                verifyWebsite(cpCore, appName, DomainName, rootPublicFilesPath, appName)
            Catch ex As Exception
                cpCore.handleException(ex, "verifySite")
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' verify the application pool. If it exists, update it. If not, create it
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="poolName"></param>
        Private Shared Sub verifyAppPool(cpCore As coreClass, poolName As String)
            Try
                Using serverManager As ServerManager = New ServerManager()
                    Dim poolFound As Boolean = False
                    Dim appPool As ApplicationPool
                    For Each appPool In serverManager.ApplicationPools
                        If (appPool.Name = poolName) Then
                            poolFound = True
                            Exit For
                        End If
                    Next
                    If Not poolFound Then
                        appPool = serverManager.ApplicationPools.Add(poolName)
                    Else
                        appPool = serverManager.ApplicationPools(poolName)
                    End If
                    appPool.ManagedRuntimeVersion = "v4.0"
                    appPool.Enable32BitAppOnWin64 = True
                    appPool.ManagedPipelineMode = ManagedPipelineMode.Integrated
                    serverManager.CommitChanges()
                End Using
            Catch ex As Exception
                cpCore.handleException(ex, "verifyAppPool")
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' verify the website. If it exists, update it. If not, create it
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="appName"></param>
        ''' <param name="domainName"></param>
        ''' <param name="phyPath"></param>
        ''' <param name="appPool"></param>
        Private Shared Sub verifyWebsite(cpCore As coreClass, appName As String, domainName As String, phyPath As String, appPool As String)
            Try

                Using iisManager As ServerManager = New ServerManager()
                    Dim site As Site
                    Dim found As Boolean = False
                    '
                    ' -- verify the site exists
                    For Each site In iisManager.Sites
                        If site.Name.ToLower() = appName.ToLower() Then
                            found = True
                            Exit For
                        End If
                    Next
                    If Not found Then
                        iisManager.Sites.Add(appName, "http", "*:80:" & appName, phyPath)
                    End If
                    site = iisManager.Sites(appName)
                    '
                    ' -- verify the bindings
                    verifyWebsite_Binding(cpCore, site, "*:80:" & appName, "http")
                    verifyWebsite_Binding(cpCore, site, "*:80:" & domainName, "http")
                    '
                    ' -- verify the application pool
                    site.ApplicationDefaults.ApplicationPoolName = appPool
                    For Each iisApp As Application In site.Applications
                        iisApp.ApplicationPoolName = appPool
                    Next
                    '
                    ' -- verify the cdn virtual directory (if configured)
                    Dim cdnFilesPrefix As String = cpCore.serverConfig.appConfig.cdnFilesNetprefix
                    If (cdnFilesPrefix.IndexOf("://") < 0) Then
                        verifyWebsite_VirtualDirectory(cpCore, site, appName, cdnFilesPrefix, cpCore.serverConfig.appConfig.cdnFilesPath)
                    End If
                    '
                    ' -- commit any changes
                    iisManager.CommitChanges()
                End Using
            Catch ex As Exception
                cpCore.handleException(ex, "verifyWebsite")
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Verify the binding
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="site"></param>
        ''' <param name="bindingInformation"></param>
        ''' <param name="bindingProtocol"></param>
        Private Shared Sub verifyWebsite_Binding(cpCore As coreClass, site As Site, bindingInformation As String, bindingProtocol As String)
            Try
                Using iisManager As ServerManager = New ServerManager()
                    Dim binding As Binding
                    Dim found As Boolean = False
                    found = False
                    For Each binding In site.Bindings
                        If (binding.BindingInformation = bindingInformation) And (binding.Protocol = bindingProtocol) Then
                            found = True
                            Exit For
                        End If
                    Next
                    If Not found Then
                        binding = site.Bindings.CreateElement()
                        binding.BindingInformation = bindingInformation
                        binding.Protocol = bindingProtocol
                        site.Bindings.Add(binding)
                        iisManager.CommitChanges()
                    End If
                End Using
            Catch ex As Exception
                cpCore.handleException(ex, "verifyWebsite_Binding")
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Private Shared Sub verifyWebsite_VirtualDirectory(cpCore As coreClass, site As Site, appName As String, virtualFolder As String, physicalPath As String)
            Try
                Dim found As Boolean = False
                For Each iisApp As Application In site.Applications
                    If iisApp.ApplicationPoolName.ToLower() = appName.ToLower() Then
                        For Each virtualDirectory As VirtualDirectory In iisApp.VirtualDirectories
                            If virtualDirectory.Path = virtualFolder Then
                                found = True
                                Exit For
                            End If
                        Next
                        If Not found Then
                            Dim vpList As List(Of String) = virtualFolder.Split("/"c).ToList
                            Dim newDirectoryPath As String = ""

                            For Each newDirectoryFolderName As String In vpList
                                If (Not String.IsNullOrEmpty(newDirectoryFolderName)) Then
                                    newDirectoryPath &= "/" & newDirectoryFolderName
                                    Dim directoryFound As Boolean = False
                                    For Each currentDirectory As VirtualDirectory In iisApp.VirtualDirectories
                                        If (currentDirectory.Path.ToLower() = newDirectoryPath.ToLower()) Then
                                            directoryFound = True
                                            Exit For
                                        End If
                                    Next
                                    If (Not directoryFound) Then
                                        iisApp.VirtualDirectories.Add(newDirectoryPath, physicalPath)
                                    End If
                                End If
                            Next
                        End If
                    End If
                    If found Then Exit For
                Next
            Catch ex As Exception
                cpCore.handleException(ex, "verifyWebsite_VirtualDirectory")
            End Try
        End Sub
        '========================================================================
        ' main_RedirectByRecord( iContentName, iRecordID )
        '   looks up the record
        '   increments the 'clicks' field and redirects to the 'link' field
        '   returns true if the redirect happened OK
        '========================================================================
        '
        Public Shared Function main_RedirectByRecord_ReturnStatus(cpcore As coreClass, ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal FieldName As String = "") As Boolean
            Dim CSPointer As Integer
            Dim MethodName As String
            Dim ContentID As Integer
            Dim CSHost As Integer
            Dim HostContentName As String
            Dim HostRecordID As Integer
            Dim BlockRedirect As Boolean
            Dim iContentName As String
            Dim iRecordID As Integer
            Dim iFieldName As String
            Dim LinkPrefix As String = String.Empty
            Dim EncodedLink As String
            Dim NonEncodedLink As String = ""
            '
            iContentName = genericController.encodeText(ContentName)
            iRecordID = genericController.EncodeInteger(RecordID)
            iFieldName = genericController.encodeEmptyText(FieldName, "link")
            '
            MethodName = "main_RedirectByRecord_ReturnStatus( " & iContentName & ", " & iRecordID & ", " & genericController.encodeEmptyText(FieldName, "(fieldname empty)") & ")"
            '
            main_RedirectByRecord_ReturnStatus = False
            BlockRedirect = False
            CSPointer = cpcore.db.csOpen(iContentName, "ID=" & iRecordID)
            If cpcore.db.csOk(CSPointer) Then
                ' 2/18/2008 - EncodeLink change
                '
                ' Assume all Link fields are already encoded -- as this is how they would appear if the admin cut and pasted
                '
                EncodedLink = Trim(cpcore.db.csGetText(CSPointer, iFieldName))
                If EncodedLink = "" Then
                    BlockRedirect = True
                Else
                    '
                    ' ----- handle content special cases (prevent redirect to deleted records)
                    '
                    NonEncodedLink = genericController.DecodeResponseVariable(EncodedLink)
                    Select Case genericController.vbUCase(iContentName)
                        Case "CONTENT WATCH"
                            '
                            ' ----- special case
                            '       if this is a content watch record, check the underlying content for
                            '       inactive or expired before redirecting
                            '
                            LinkPrefix = cpcore.webServer.requestContentWatchPrefix
                            ContentID = (cpcore.db.csGetInteger(CSPointer, "ContentID"))
                            HostContentName = models.complex.cdefmodel.getContentNameByID(cpcore,ContentID)
                            If (HostContentName = "") Then
                                '
                                ' ----- Content Watch with a bad ContentID, mark inactive
                                '
                                BlockRedirect = True
                                Call cpcore.db.csSet(CSPointer, "active", 0)
                            Else
                                HostRecordID = (cpcore.db.csGetInteger(CSPointer, "RecordID"))
                                If HostRecordID = 0 Then
                                    '
                                    ' ----- Content Watch with a bad iRecordID, mark inactive
                                    '
                                    BlockRedirect = True
                                    Call cpcore.db.csSet(CSPointer, "active", 0)
                                Else
                                    CSHost = cpcore.db.csOpen(HostContentName, "ID=" & HostRecordID)
                                    If Not cpcore.db.csOk(CSHost) Then
                                        '
                                        ' ----- Content Watch host record not found, mark inactive
                                        '
                                        BlockRedirect = True
                                        Call cpcore.db.csSet(CSPointer, "active", 0)
                                    End If
                                End If
                                Call cpcore.db.csClose(CSHost)
                            End If
                            If BlockRedirect Then
                                '
                                ' ----- if a content watch record is blocked, delete the content tracking
                                '
                                Call cpcore.db.deleteContentRules(models.complex.cdefmodel.getcontentid(cpcore,HostContentName), HostRecordID)
                            End If
                    End Select
                End If
                If Not BlockRedirect Then
                    '
                    ' If link incorrectly includes the LinkPrefix, take it off first, then add it back
                    '
                    NonEncodedLink = genericController.ConvertShortLinkToLink(NonEncodedLink, LinkPrefix)
                    If cpcore.db.cs_isFieldSupported(CSPointer, "Clicks") Then
                        Call cpcore.db.csSet(CSPointer, "Clicks", (cpcore.db.csGetNumber(CSPointer, "Clicks")) + 1)
                    End If
                    Call cpcore.webServer.redirect(LinkPrefix & NonEncodedLink, "Call to " & MethodName & ", no reason given.", False)
                    main_RedirectByRecord_ReturnStatus = True
                End If
            End If
            Call cpcore.db.csClose(CSPointer)
        End Function
        '
        '========================================================================
        '
        Public Shared Function getBrowserAcceptLanguage(cpCore As coreClass) As String
            Try
                Dim AcceptLanguageString As String = genericController.encodeText(cpCore.webServer.requestLanguage) & ","
                Dim CommaPosition As Integer = genericController.vbInstr(1, AcceptLanguageString, ",")
                Do While CommaPosition <> 0
                    Dim AcceptLanguage As String = Trim(Mid(AcceptLanguageString, 1, CommaPosition - 1))
                    AcceptLanguageString = Mid(AcceptLanguageString, CommaPosition + 1)
                    If Len(AcceptLanguage) > 0 Then
                        Dim DashPosition As Integer = genericController.vbInstr(1, AcceptLanguage, "-")
                        If DashPosition > 1 Then
                            AcceptLanguage = Mid(AcceptLanguage, 1, DashPosition - 1)
                        End If
                        DashPosition = genericController.vbInstr(1, AcceptLanguage, ";")
                        If DashPosition > 1 Then
                            Return Mid(AcceptLanguage, 1, DashPosition - 1)
                        End If
                    End If
                    CommaPosition = genericController.vbInstr(1, AcceptLanguageString, ",")
                Loop
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return ""
        End Function
        Public Sub clearResponseBuffer()
            bufferRedirect = ""
            bufferResponseHeader = ""
        End Sub
    End Class
End Namespace