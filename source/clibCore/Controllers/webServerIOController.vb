
Option Explicit On
Option Strict On

Imports Microsoft.Web.Administration
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Namespace Contensive.Core
    ''' <summary>
    ''' Code dedicated to processing iis input and output. lazy Constructed. (see coreHtmlClass for html processing)
    ''' What belongs here is everything that would have to change if we converted to apache
    ''' </summary>
    Public Class webServerController
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
        Public webServerIO_InitCounter As Integer = 0
        '
        Public RequestLanguage As String = ""            ' set externally from HTTP_Accept_LANGUAGE
        Public requestHttpAccept As String = ""
        Public requestHttpAcceptCharset As String = ""
        Public requestHttpProfile As String = ""
        Public requestxWapProfile As String = ""
        Public requestHTTPVia As String = ""                   ' informs the server of proxies used during the request
        Public requestHTTPFrom As String = ""                  ' contains the email address of the requestor
        Public requestPathPage As String = ""               ' The Path and Page part of the current URI
        Public requestReferrer As String = ""
        Public requestDomain As String = ""                 ' The Host part of the current URI
        Public requestSecure As Boolean = False             ' Set in InitASPEnvironment, true if https
        Public requestRemoteIP As String = ""               '
        Public requestBrowser As String = ""                ' The browser for this visit
        Public requestQueryString As String = ""            ' The QueryString of the current URI
        Public requestFormUseBinaryHeader As Boolean = False ' When set true with RequestNameBinaryRead=true, InitEnvironment reads the form in with a binary read
        Public requestFormBinaryHeader As Byte()            ' For asp pages, this is the full multipart header
        Public requestFormString As String = ""             ' String from an HTML form post - buffered to remove passwords
        Public requesFilesString As String = ""             ' String from an HTML form post
        'Public requestCookieString As String = ""          ' Set in InitASPEnvironment, the full cookie string
        Public requestSpaceAsUnderscore As Boolean = False  ' when true, is it assumed that dots in request variable names will convert
        Public requestDotAsUnderscore As Boolean = False    ' (php converts spaces and dots to underscores)
        Public requestLinkSource As String = ""
        '
        Public webServerIO_LinkForwardSource As String = ""          ' main_ServerPathPage -- set during init
        Public webServerIO_LinkForwardError As String = ""           ' always 404
        Public webServerIO_ReadStreamJSForm As Boolean = False                  ' When true, the request comes from a browser handling a JSPage script line
        Public webServerIO_PageExcludeFromAnalytics As Boolean = False    ' For this page - true for remote methods and ajax
        Public webServerIO_BlockClosePageCopyright As Boolean = False ' if true, block the copyright message
        Public webServerIO_PageTestPointPrinting As Boolean = False    ' if true, send main_TestPoint messages to the stream
        '
        ' refactor - this method stears the stream between controllers, put it in cpcore
        Public webServerIO_OutStreamDevice As Integer = 0
        Public webServerIO_MemberAction As Integer = 0              ' action to be performed during init
        Public webServerIO_AdminMessage As String = ""          ' For more information message
        Public webServerIO_requestPageReferer As String = ""                    ' replaced by main_ServerReferrer
        Public webServerIO_requestReferer As String = ""
        Public webServerIO_ServerFormActionURL As String = ""        ' The Action for all internal forms, if not set, default
        Public webServerIO_requestContentWatchPrefix As String = ""   ' The different between the URL and the main_ContentWatch Pathpage
        Public webServerIO_requestProtocol As String = ""             ' Set in InitASPEnvironment, http or https
        Public webServerIO_ServerLink As String = ""                 ' The current URL, from protocol to end of quesrystring
        Public webServerIO_requestVirtualFilePath As String = ""          ' The Virtual path for the site (host+main_ServerVirtualPath+"/" is site URI)
        Public webServerIO_requestDomain As String = ""               ' This is the proper domain for the site (may not always match the current SERVER_NAME
        Public webServerIO_requestPath As String = ""                 ' The path part of the current URI
        Public webServerIO_requestPage As String = ""                 ' The page part of the current URI
        Public webServerIO_requestSecureURLRoot As String = ""        ' The URL to the root of the secure area for this site
        '
        Public webServerIO_response_NoFollow As Boolean = False   ' when set, Meta no follow is added
        Public webServerIO_bufferRedirect As String = ""
        Public webServerIO_bufferContentType As String = ""
        Public webServerIO_bufferCookies As String = ""
        Public webServerIO_bufferResponseHeader As String = ""
        Public webServerIO_bufferResponseStatus As String = ""
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
                cpCore.handleExceptionAndRethrow(ex)
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
                cpCore.handleExceptionAndRethrow(ex)
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
                cpCore.handleExceptionAndRethrow(ex)
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
                cpCore.handleExceptionAndRethrow(ex)
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
                ' web client initialize
                '
                iisContext = httpContext
                Dim key As String
                Dim isMultipartPost As Boolean
                Dim keyValue As String
                Dim parser As HttpMultipartParser.MultipartFormDataParser
                Dim isAdmin As Boolean = False
                Dim pos As Integer
                Dim aliasRoute As String
                Dim SourceProtocol As String = ""
                Dim aliasDomain As String = ""
                Dim aliasPort As String = ""
                Dim aliasPathPage As String = ""
                Dim testPage As String
                Dim SourceExtension As String = ""
                Dim qsCnt As Integer = 0
                '
                '
                Dim forwardDomain As String
                Dim defaultDomainContentList As String = ""
                Dim domainDetailsListText As String
                Dim InitAddGroupList As String = ""
                Dim nothingObject As Object = Nothing
                Dim CookieDetectKey As String
                Dim CookieDetectVisitId As Integer
                Dim PageNotFoundReason As String = ""
                Dim PageNotFoundSource As String = ""
                Dim IsPageNotFound As Boolean
                Dim RedirectReason As String = ""
                Dim RefProtocol As String = ""
                Dim RefHost As String = ""
                Dim Refpath As String = ""
                Dim RefQueryString As String = ""
                Dim RefPage As String = ""
                'Dim Pos As Integer
                Dim RedirectLink As String = ""
                Dim TextStartPointer As Integer
                Dim SQL As String
                Dim ShortPath As String = ""
                Dim ContentName As String = ""
                Dim HardCodedPage As String
                Dim Copy As String
                Dim LinkSplit() As String
                Dim ampSplit() As String
                Dim ampSplitCount As Integer
                Dim ampSplitPointer As Integer
                Dim CS As Integer
                Dim Id As Integer
                Dim GroupName As String = ""
                Dim AjaxFunction As String = ""
                Dim AjaxFastFunction As String = ""
                Dim LinkForwardCriteria As String = ""
                Dim RemoteMethodFromPage As String = ""
                Dim RemoteMethodFromQueryString As String = ""
                '
                ' setup IIS Response
                '
                iisContext.Response.CacheControl = "no-cache"
                iisContext.Response.Expires = -1
                iisContext.Response.Buffer = True
                ''
                '
                ' ----- basic request environment
                '
                requestDomain = iisContext.Request.ServerVariables("SERVER_NAME")
                requestPathPage = CStr(iisContext.Request.ServerVariables("SCRIPT_NAME"))
                requestReferrer = CStr(iisContext.Request.ServerVariables("HTTP_REFERER"))
                requestSecure = CBool(iisContext.Request.ServerVariables("SERVER_PORT_SECURE"))
                requestRemoteIP = CStr(iisContext.Request.ServerVariables("REMOTE_ADDR"))
                requestBrowser = CStr(iisContext.Request.ServerVariables("HTTP_USER_AGENT"))
                RequestLanguage = CStr(iisContext.Request.ServerVariables("HTTP_ACCEPT_LANGUAGE"))
                requestHttpAccept = CStr(iisContext.Request.ServerVariables("HTTP_ACCEPT"))
                requestHttpAcceptCharset = CStr(iisContext.Request.ServerVariables("HTTP_ACCEPT_CHARSET"))
                requestHttpProfile = CStr(iisContext.Request.ServerVariables("HTTP_PROFILE"))
                '
                ' ----- http QueryString
                '
                isMultipartPost = False
                If (iisContext.Request.QueryString.Count > 0) Then
                    requestQueryString = ""
                    aliasRoute = ""
                    qsCnt = 0
                    For Each key In iisContext.Request.QueryString
                        keyValue = iisContext.Request.QueryString(key)
                        cpCore.docProperties.setProperty(key, keyValue)
                        If (qsCnt > 0) Then
                            '
                            ' normal non-first elements
                            '
                            requestQueryString = genericController.ModifyQueryString(requestQueryString, key, keyValue)
                        Else
                            '
                            ' first element - test first querystring element for iis 404
                            '
                            If ((keyValue & "    ").Substring(0, 4) = "404;") Then
                                ' 404 hit with url like http://domain/page, qsName is http://domain/page qsValue is value0
                                aliasRoute = keyValue.Substring(4)
                                requestQueryString = genericController.ModifyQueryString(requestQueryString, key, keyValue)
                            Else
                                ' test for special 404 case where first element of qs starts 404;url
                                If ((key & "    ").Substring(0, 4) = "404;") Then
                                    ' 404 hit with url like 404;http://domain/page?name0=value0&etc... , qsName is http://domain/page?name0 qsValue is value0
                                    key = key.Substring(4)
                                    pos = genericController.vbInstr(1, key, "?")
                                    If pos <> 0 Then
                                        aliasRoute = Mid(key, 1, pos - 1)
                                        key = Mid(key, pos + 1)
                                    Else
                                        aliasRoute = key
                                        key = ""
                                    End If
                                    requestQueryString = genericController.ModifyQueryString(requestQueryString, key, keyValue)
                                Else
                                    requestQueryString = genericController.ModifyQueryString(requestQueryString, key, keyValue)
                                End If
                            End If
                            '
                            ' set context domain and pathPath from the URL from in the 404 string 
                            '
                            pos = genericController.vbInstr(1, aliasRoute, "://")
                            If pos > 0 Then
                                '
                                ' remove protocol
                                '
                                testPage = aliasRoute
                                SourceProtocol = Mid(testPage, 1, pos + 2)
                                testPage = Mid(testPage, pos + 3)
                                pos = genericController.vbInstr(1, testPage, "/")
                                If pos > 0 Then
                                    '
                                    ' remove domain and port
                                    '
                                    aliasDomain = Mid(testPage, 1, pos - 1)
                                    aliasPathPage = Mid(testPage, pos)
                                    pos = genericController.vbInstr(1, aliasDomain, ":")
                                    If pos > 0 Then
                                        aliasPort = Mid(aliasDomain, pos + 1)
                                        aliasDomain = Left(aliasDomain, pos - 1)
                                    End If
                                End If
                                requestDomain = aliasDomain
                                If (aliasPathPage.Substring(aliasPathPage.Length - 1) = "/") Then
                                    aliasPathPage = aliasPathPage.Substring(0, aliasPathPage.Length - 1)
                                End If
                                requestPathPage = aliasPathPage
                            End If
                        End If
                        isMultipartPost = isMultipartPost Or (LCase(key) = "requestbinary")
                        qsCnt += 1
                    Next
                End If
                '
                ' ----- http Form
                '
                requestFormString = ""
                Dim postError As Boolean = False
                Try
                    Dim inputStream As IO.Stream = iisContext.Request.InputStream
                Catch ex As httpException
                    Call cpCore.handleExceptionAndRethrow(ex)
                    cpCore.error_AddUserError(ex.Message)
                    postError = True
                Catch ex As Exception
                    Call cpCore.handleExceptionAndRethrow(ex)
                    cpCore.error_AddUserError(ex.Message)
                    postError = True
                End Try
                If Not postError Then
                    If Not isMultipartPost Then
                        '
                        ' ----- non-multipart form
                        '
                        For Each key In iisContext.Request.Form.Keys
                            keyValue = iisContext.Request.Form(key)
                            cpCore.docProperties.setProperty(key, keyValue, True)
                            requestFormString = genericController.ModifyQueryString(requestFormString, key, keyValue)
                        Next
                    Else
                        '
                        ' ----- multipart form (and file uploads)
                        '
                        Try
                            If (iisContext.Request.InputStream.Length <= 0) Then
                                '
                            Else
                                parser = New HttpMultipartParser.MultipartFormDataParser(iisContext.Request.InputStream)
                                For Each parameter As HttpMultipartParser.ParameterPart In parser.Parameters
                                    key = parameter.Name
                                    keyValue = parameter.Data
                                    cpCore.docProperties.setProperty(key, keyValue, True)
                                    requestFormString = genericController.ModifyQueryString(requestFormString, key, keyValue)
                                Next
                                '
                                ' file uploads, add to doc properties
                                '
                                If parser.Files.Count > 0 Then
                                    Dim ptr As Integer = 0
                                    Dim ptrText As String
                                    Dim instanceId As String = cpCore.createGuid().Replace("{", "").Replace("-", "").Replace("}", "")
                                    For Each file As HttpMultipartParser.FilePart In parser.Files
                                        If file.FileName.Length > 0 Then
                                            Dim prop As New docPropertiesClass
                                            ptrText = ptr.ToString
                                            prop.Name = file.Name
                                            prop.Value = file.FileName
                                            prop.NameValue = EncodeRequestVariable(prop.Name) & "=" & EncodeRequestVariable(prop.Value)
                                            prop.IsFile = True
                                            prop.IsForm = True
                                            prop.tmpPrivatePathfilename = instanceId & "-" & ptrText & ".bin"
                                            cpCore.deleteOnDisposeFileList.Add(prop.tmpPrivatePathfilename)
                                            Using fileStream As System.IO.FileStream = System.IO.File.OpenWrite(cpCore.privateFiles.rootLocalPath & prop.tmpPrivatePathfilename)
                                                file.Data.CopyTo(fileStream)
                                            End Using
                                            prop.FileSize = CInt(file.Data.Length)
                                            cpCore.docProperties.setProperty(file.Name, prop)
                                            '
                                            requesFilesString = "" _
                                        & "&" & ptrText & "formname=" & EncodeRequestVariable(prop.Name) _
                                        & "&" & ptrText & "filename=" & EncodeRequestVariable(prop.Value) _
                                        & "&" & ptrText & "type=" _
                                        & "&" & ptrText & "tmpFile=" & EncodeRequestVariable(prop.tmpPrivatePathfilename) _
                                        & "&" & ptrText & "error=" _
                                        & "&" & ptrText & "size=" & prop.FileSize _
                                        & ""
                                            ptr += 1
                                        End If
                                    Next
                                End If
                                'https://github.com/Vodurden/Http-Multipart-Data-Parser
                            End If
                        Catch ex As Exception
                            cpCore.handleExceptionAndContinue(ex, "Exception processing multipart form input")
                        End Try
                    End If
                End If
                '
                ' load request cookies
                '
                For Each key In iisContext.Request.Cookies
                    keyValue = iisContext.Request.Cookies(key).Value
                    keyValue = DecodeResponseVariable(keyValue)
                    addRequestCookie(key, keyValue)
                Next
                '
                '--------------------------------------------------------------------------
                '
                If (cpCore.serverConfig.appConfig.appStatus <> Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusReady) Then
                    '
                    ' did not initialize correctly
                    '
                Else
                    '
                    ' continue
                    '
                    webServerIO_InitCounter += 1
                    '
                    Call cpCore.htmlDoc.enableOutputBuffer(True)
                    cpCore.docOpen = True
                    Call webServerIO_setResponseContentType("text/html")
                    '
                    '--------------------------------------------------------------------------
                    ' ----- Process QueryString to cpcore.doc.main_InStreamArray
                    '       Do this first to set cpcore.main_ReadStreamJSForm, cpcore.main_ReadStreamJSProcess, cpcore.main_ReadStreamBinaryRead (must be in QS)
                    '--------------------------------------------------------------------------
                    '
                    webServerIO_LinkForwardSource = ""
                    webServerIO_LinkForwardError = ""
                    '
                    ' start with the best guess for the source url, then improve the guess based on what iis might have done
                    '
                    requestLinkSource = "http://"
                    If requestSecure Then
                        requestLinkSource = "https://"
                    End If
                    requestLinkSource = requestLinkSource & requestDomain & requestPathPage
                    If requestQueryString <> "" Then
                        requestLinkSource = requestLinkSource & "?" & requestQueryString
                    End If
                    If requestQueryString <> "" Then
                        '
                        ' Add query string to stream
                        '
                        Call cpCore.docProperties.addQueryString(requestQueryString)
                    End If
                    '
                    '--------------------------------------------------------------------------
                    ' ----- Handle RequestJSForm (hit caused by a browser processing the <script...></script> tag)
                    '--------------------------------------------------------------------------
                    '
                    If webServerIO_ReadStreamJSForm Then
                        '
                        ' Request comes from the browser while processing the javascript line
                        ' Add JSProcessQuery to QS
                        ' Add cpcore.main_ServerReferrerQS to QS
                        ' Add JSProcessForm to form
                        '
                        webServerIO_BlockClosePageCopyright = True
                        webServerIO_OutStreamDevice = htmlDocController.htmlDoc_OutStreamJavaScript ' refactor - these should just be setContentType as a string so developers can set whatever
                        Call webServerIO_setResponseContentType("application/javascript") ' refactor -- this should be setContentType
                        '
                        ' Add the cpcore.main_ServerReferrer QS to the cpcore.doc.main_InStreamArray()
                        '
                        ' ***** put back in because Add-ons need it for things like BID
                        ' /***** removed so the new system does not cpcore.main_Get the referrers QS
                        ' /***** the best we remember, only Aspen uses this, and they are moving
                        '
                        If genericController.vbInstr(1, requestReferrer, "?") <> 0 Then
                            LinkSplit = Split(requestReferrer, "?")
                            ampSplit = Split(LinkSplit(1), "&")
                            ampSplitCount = UBound(ampSplit) + 1
                            For ampSplitPointer = 0 To ampSplitCount - 1
                                Dim propName As String
                                Dim propValue As String = ""
                                Dim propNameValue As String = ampSplit(ampSplitPointer)
                                Dim propNameValuePair() As String = Split(propNameValue, "=")
                                propName = DecodeResponseVariable(propNameValuePair(0))
                                If UBound(propNameValuePair) > 0 Then
                                    propValue = DecodeResponseVariable(propNameValuePair(1))
                                End If
                                cpCore.docProperties.setProperty(propName, propValue, False)
                            Next
                        End If
                    End If
                    '
                    '--------------------------------------------------------------------------
                    ' Set misc Server publics
                    '--------------------------------------------------------------------------
                    '
                    webServerIO_MemberAction = cpCore.docProperties.getInteger("ma")
                    If (webServerIO_MemberAction = 3) Or (webServerIO_MemberAction = 2) Then
                        webServerIO_MemberAction = 0
                        HardCodedPage = HardCodedPageLogoutLogin
                    End If
                    '
                    ' calculate now - but recalculate later - this does not include the /RemoteMethodFromQueryString case
                    '
                    '
                    webServerIO_PageExcludeFromAnalytics = (AjaxFunction <> "") Or (AjaxFastFunction <> "") Or (RemoteMethodFromQueryString <> "")
                    '
                    '
                    ' Other Server variables
                    '
                    webServerIO_requestReferer = requestReferrer
                    webServerIO_requestPageReferer = requestReferrer
                    '
                    If requestSecure Then
                        webServerIO_requestProtocol = "https://"
                    Else
                        webServerIO_requestProtocol = "http://"
                    End If
                    '
                    cpCore.blockExceptionReporting = False
                    '
                    '--------------------------------------------------------------------------
                    ' ----- initialize server connection
                    '--------------------------------------------------------------------------
                    '
                    If cpCore.domains.getDomainDbList.Contains("*") Then
                        cpCore.domains.ServerMultiDomainMode = True
                    End If
                    '
                    '--------------------------------------------------------------------------
                    '   javascript cookie detect on page1 of all visits
                    '--------------------------------------------------------------------------
                    '
                    CookieDetectKey = cpCore.docProperties.getText(RequestNameCookieDetectVisitID)
                    If CookieDetectKey <> "" Then
                        '
                        'Call AppendLog("cpCore.main_init(), 1630 - exit for cookie key detected/processed")
                        '
                        Dim cookieDetectDate As Date = New Date
                        Call cpCore.security.decodeToken(CookieDetectKey, CookieDetectVisitId, cookieDetectDate)
                        'CookieDetectVisitId = cpCore.main_DecodeKeyNumber(CookieDetectKey)
                        If CookieDetectVisitId <> 0 Then
                            Call cpCore.db.executeSql("update ccvisits set CookieSupport=1 where id=" & CookieDetectVisitId)
                            cpCore.docOpen = False '--- should be disposed by caller --- Call dispose
                            Return cpCore.docOpen
                        End If
                    End If
                    '
                    '--------------------------------------------------------------------------
                    '   verify Domain table entry
                    '--------------------------------------------------------------------------
                    '
                    Dim updateDomainCache As Boolean = False
                    '
                    cpCore.domains.domainDetails.name = requestDomain
                    cpCore.domains.domainDetails.rootPageId = 0
                    cpCore.domains.domainDetails.noFollow = False
                    cpCore.domains.domainDetails.typeId = 1
                    cpCore.domains.domainDetails.visited = False
                    cpCore.domains.domainDetails.id = 0
                    cpCore.domains.domainDetails.forwardUrl = ""
                    webServerIO_requestDomain = requestDomain
                    '
                    ' REFACTOR -- move to cpcore.domains class 
                    cpCore.domains.domainDetailsList = cpCore.cache.getObject(Of Dictionary(Of String, Models.Entity.domainLegacyModel.domainDetailsClass))("domainContentList")
                    If (cpCore.domains.domainDetailsList Is Nothing) Then
                        '
                        '  no cache found, build domainContentList from database
                        '
                        cpCore.domains.domainDetailsList = New Dictionary(Of String, Models.Entity.domainLegacyModel.domainDetailsClass)
                        domainDetailsListText = vbCrLf
                        SQL = "select name,rootpageid,nofollow,typeid,visited,id,ForwardURL,DefaultTemplateId,PageNotFoundPageID,allowCrossLogin,ForwardDomainId from ccdomains where (active<>0)and(name is not null) order by id"
                        Dim dt As DataTable
                        dt = cpCore.db.executeSql(SQL)
                        If dt.Rows.Count > 0 Then
                            If Not (dt.Columns Is Nothing) Then
                                Dim colCnt As Integer = dt.Columns.Count
                                For Each row As DataRow In dt.Rows
                                    Dim domainNameNew As String = row.Item(0).ToString.Trim
                                    If Not String.IsNullOrEmpty(domainNameNew) Then
                                        If Not cpCore.domains.domainDetailsList.ContainsKey(domainNameNew.ToLower) Then
                                            Dim domainDetailsNew As New Models.Entity.domainLegacyModel.domainDetailsClass
                                            domainDetailsNew.name = domainNameNew
                                            domainDetailsNew.rootPageId = genericController.EncodeInteger(row.Item(1).ToString)
                                            domainDetailsNew.noFollow = genericController.EncodeBoolean(row.Item(2).ToString)
                                            domainDetailsNew.typeId = genericController.EncodeInteger(row.Item(3).ToString)
                                            domainDetailsNew.visited = genericController.EncodeBoolean(row.Item(4).ToString)
                                            domainDetailsNew.id = genericController.EncodeInteger(row.Item(5).ToString)
                                            domainDetailsNew.forwardUrl = row.Item(6).ToString
                                            domainDetailsNew.defaultTemplateId = genericController.EncodeInteger(row.Item(7).ToString)
                                            domainDetailsNew.pageNotFoundPageId = genericController.EncodeInteger(row.Item(8).ToString)
                                            domainDetailsNew.allowCrossLogin = genericController.EncodeBoolean(row.Item(9).ToString)
                                            domainDetailsNew.forwardDomainId = genericController.EncodeInteger(row.Item(10).ToString)
                                            cpCore.domains.domainDetailsList.Add(domainNameNew.ToLower(), domainDetailsNew)
                                        End If
                                    End If
                                Next
                            End If
                        End If
                        updateDomainCache = True
                    End If
                    '
                    ' verify app config domainlist is in the domainlist cache
                    '
                    For Each domain As String In cpCore.serverConfig.appConfig.domainList
                        If Not cpCore.domains.domainDetailsList.ContainsKey(domain.ToLower()) Then
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
                            domainDetailsNew.allowCrossLogin = False
                            domainDetailsNew.forwardDomainId = 0
                            cpCore.domains.domainDetailsList.Add(domain.ToLower(), domainDetailsNew)
                        End If
                    Next
                    If cpCore.domains.domainDetailsList.ContainsKey(requestDomain.ToLower()) Then
                        '
                        ' domain found
                        '
                        cpCore.domains.domainDetails = cpCore.domains.domainDetailsList(requestDomain.ToLower())
                        If (cpCore.domains.domainDetails.id = 0) Then
                            '
                            ' this is a default domain or a new domain -- add to the domain table
                            '
                            CS = cpCore.db.cs_insertRecord("domains")
                            If cpCore.db.cs_ok(CS) Then
                                cpCore.domains.domainDetails.id = cpCore.db.cs_getInteger(CS, "id")
                                Call cpCore.db.cs_set(CS, "name", requestDomain)
                                Call cpCore.db.cs_set(CS, "typeId", "1")
                                Call cpCore.db.cs_set(CS, "RootPageId", cpCore.domains.domainDetails.rootPageId.ToString)
                                Call cpCore.db.cs_set(CS, "ForwardUrl", cpCore.domains.domainDetails.forwardUrl)
                                Call cpCore.db.cs_set(CS, "NoFollow", cpCore.domains.domainDetails.noFollow.ToString)
                                Call cpCore.db.cs_set(CS, "Visited", cpCore.domains.domainDetails.visited.ToString)
                                Call cpCore.db.cs_set(CS, "DefaultTemplateId", cpCore.domains.domainDetails.defaultTemplateId.ToString)
                                Call cpCore.db.cs_set(CS, "PageNotFoundPageId", cpCore.domains.domainDetails.pageNotFoundPageId.ToString)
                                Call cpCore.db.cs_set(CS, "allowCrossLogin", cpCore.domains.domainDetails.allowCrossLogin.ToString)
                            End If
                            Call cpCore.db.cs_Close(CS)
                        End If
                        If Not cpCore.domains.domainDetails.visited Then
                            '
                            ' set visited true
                            '
                            SQL = "update ccdomains set visited=1 where name=" & cpCore.db.encodeSQLText(requestDomain)
                            Call cpCore.db.executeSql(SQL)
                            Call cpCore.cache.setObject("domainContentList", "", "domains")
                        End If
                        If cpCore.domains.domainDetails.typeId = 1 Then
                            '
                            ' normal domain, leave it
                            '
                        ElseIf genericController.vbInstr(1, requestPathPage, cpCore.siteProperties.adminURL, vbTextCompare) <> 0 Then
                            '
                            ' forwarding does not work in the admin site
                            '
                        ElseIf (cpCore.domains.domainDetails.typeId = 2) And (cpCore.domains.domainDetails.forwardUrl <> "") Then
                            '
                            ' forward to a URL
                            '
                            '
                            'Call AppendLog("main_init(), 1710 - exit for domain forward")
                            '
                            If genericController.vbInstr(1, cpCore.domains.domainDetails.forwardUrl, "://") = 0 Then
                                cpCore.domains.domainDetails.forwardUrl = "http://" & cpCore.domains.domainDetails.forwardUrl
                            End If
                            Call webServerIO_Redirect2(cpCore.domains.domainDetails.forwardUrl, "Forwarding to [" & cpCore.domains.domainDetails.forwardUrl & "] because the current domain [" & requestDomain & "] is in the domain content set to forward to this URL", False)
                            Return cpCore.docOpen
                        ElseIf (cpCore.domains.domainDetails.typeId = 3) And (cpCore.domains.domainDetails.forwardDomainId <> 0) And (cpCore.domains.domainDetails.forwardDomainId <> cpCore.domains.domainDetails.id) Then
                            '
                            ' forward to a replacement domain
                            '
                            forwardDomain = cpCore.content_GetRecordName("domains", cpCore.domains.domainDetails.forwardDomainId)
                            If forwardDomain <> "" Then
                                pos = genericController.vbInstr(1, requestLinkSource, requestDomain, vbTextCompare)
                                If (pos > 0) Then
                                    '
                                    'Call AppendLog("main_init(), 1720 - exit for forward domain")
                                    '
                                    cpCore.domains.domainDetails.forwardUrl = Mid(requestLinkSource, 1, pos - 1) & forwardDomain & Mid(requestLinkSource, pos + Len(requestDomain))
                                    'main_domainForwardUrl = genericController.vbReplace(main_ServerLinkSource, cpcore.main_ServerHost, forwardDomain)
                                    Call webServerIO_Redirect2(cpCore.domains.domainDetails.forwardUrl, "Forwarding to [" & cpCore.domains.domainDetails.forwardUrl & "] because the current domain [" & requestDomain & "] is in the domain content set to forward to this replacement domain", False)
                                    Return cpCore.docOpen
                                End If
                                '                                cpcore.main_domainForwardUrl = "http://"
                                '                                If cpcore.main_ServerPageSecure Then
                                '                                    cpcore.main_domainForwardUrl = "https://"
                                '                                End If
                                '                                cpcore.main_domainForwardUrl = cpcore.main_domainForwardUrl & forwardDomain & cpcore.main_ServerPathPage
                                '                                If cpcore.main_ServerQueryString <> "" Then
                                '                                    cpcore.main_domainForwardUrl = cpcore.main_domainForwardUrl & "?" & cpcore.main_ServerQueryString
                                '                                End If
                                '                                Call cpcore.main_Redirect2(main_domainForwardUrl, "Forwarding to [" & cpcore.main_domainForwardUrl & "] because the current domain [" & cpcore.main_ServerHost & "] is in the domain content set to forward to this replacement domain", False)
                            End If
                        End If
                        If cpCore.domains.domainDetails.noFollow Then
                            webServerIO_response_NoFollow = True
                        End If

                    Else
                        '
                        ' domain not found
                        ' current host not in domainContent, add it and re-save the cache
                        '
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
                        domainDetailsNew.allowCrossLogin = False
                        domainDetailsNew.forwardDomainId = 0
                        cpCore.domains.domainDetailsList.Add(requestDomain.ToLower(), domainDetailsNew)
                        '
                        CS = cpCore.db.cs_insertRecord("domains")
                        If cpCore.db.cs_ok(CS) Then
                            cpCore.domains.domainDetails.id = cpCore.db.cs_getInteger(CS, "id")
                            Call cpCore.db.cs_set(CS, "name", requestDomain)
                            Call cpCore.db.cs_set(CS, "typeid", "1")
                        End If
                        Call cpCore.db.cs_Close(CS)
                        '
                        updateDomainCache = True
                    End If
                    If (updateDomainCache) Then
                        '
                        ' if there was a change, update the cache
                        '
                        Call cpCore.cache.setObject("domainContentList", cpCore.domains.domainDetailsList, "domains")
                        'domainDetailsListText = cpCore.json.Serialize(cpCore.domains.domainDetailsList)
                        'Call cpCore.cache.setObject("domainContentList", domainDetailsListText, "domains")
                    End If
                    '
                    webServerIO_requestVirtualFilePath = "/" & cpCore.serverConfig.appConfig.name
                    '
                    webServerIO_requestContentWatchPrefix = webServerIO_requestProtocol & requestDomain & requestAppRootPath
                    webServerIO_requestContentWatchPrefix = Mid(webServerIO_requestContentWatchPrefix, 1, Len(webServerIO_requestContentWatchPrefix) - 1)
                    '
                    'ServerSocketLoaded = False
                    '
                    ' ----- Server Identification
                    '       keep case from AppRootPath, but do not redirect
                    '       all cpcore.main_ContentWatch URLs should be checked (and changed to) AppRootPath
                    '
                    '
                    webServerIO_requestPath = "/"
                    webServerIO_requestPage = cpCore.siteProperties.serverPageDefault
                    TextStartPointer = InStrRev(requestPathPage, "/")
                    If TextStartPointer <> 0 Then
                        webServerIO_requestPath = Mid(requestPathPage, 1, TextStartPointer)
                        webServerIO_requestPage = Mid(requestPathPage, TextStartPointer + 1)
                    End If
                    ' cpcore.web_requestAppPath = Mid(cpcore.web_requestPath, Len(appRootPath) + 1)
                    webServerIO_requestSecureURLRoot = "https://" & webServerIO_requestDomain & requestAppRootPath
                    ''
                    '' ----- If virtual site, check RootPath case against current URL
                    ''
                    'If appRootPath <> "/" Then
                    '    PathTest = Left(cpcore.web_requestPath, Len(appRootPath))
                    '    If PathTest <> appRootPath Then
                    '        '
                    '        ' Case mismatch, redirect to correct case so cookies will be valid
                    '        '
                    '        'Call AppendLog("main_init(), 1810 - exit for rootpath mismatch (?)")
                    '        '
                    '        If web.requestQueryString = "" Then
                    '            Link = cpcore.web_requestProtocol & cpcore.main_ServerDomain & appRootPath & cpcore.web_requestAppPath & cpcore.web_requestPage
                    '        Else
                    '            Link = cpcore.web_requestProtocol & cpcore.main_ServerDomain & appRootPath & cpcore.web_requestAppPath & cpcore.web_requestPage & "?" & web.requestQueryString
                    '        End If
                    '        Call cpcore.web_Redirect2(Link, "Redirecting because this site is configured to only run in the path [" & appRootPath & "]. See the IIS Virtual Folder property of the Contensive Application Manager.", False)
                    '        cpcore. cpcore.docOpen = False '--- should be disposed by caller --- Call dispose
                    '        Return cpcore. cpcore.docOpen
                    '    End If
                    'End If
                    '
                    ' ----- cpcore.main_RefreshQueryString
                    '
                    Id = cpCore.docProperties.getInteger("bid")
                    If Id <> 0 Then
                        Call cpCore.htmlDoc.webServerIO_addRefreshQueryString("bid", Id.ToString)
                    End If
                    Id = cpCore.docProperties.getInteger("sid")
                    If Id <> 0 Then
                        Call cpCore.htmlDoc.webServerIO_addRefreshQueryString("sid", Id.ToString)
                    End If
                    '
                    ' ----- Create Server Link property
                    '
                    webServerIO_ServerLink = webServerIO_requestProtocol & requestDomain & requestAppRootPath & webServerIO_requestPath & webServerIO_requestPage
                    If requestQueryString <> "" Then
                        webServerIO_ServerLink = webServerIO_ServerLink & "?" & requestQueryString
                    End If
                    If requestLinkSource = "" Then
                        requestLinkSource = webServerIO_ServerLink
                    End If
                    '
                    ' ----- File storage
                    '
                    'app.siteProperty_publicFileContentPathPrefix = cpcore.main_ServerVirtualPath & "/files/"
                    '
                    ' ----- Style tag
                    '
                    webServerIO_AdminMessage = "For more information, please contact the <a href=""mailto:" & cpCore.siteProperties.emailAdmin & "?subject=Re: " & webServerIO_requestDomain & """>Site Administrator</A>."

                    '
                    '
                    '
                    ' START - this goes in getRoute (link alias and link forwarding hooks)
                    '
                    '
                    '
                    '
                    '
                    '
                    '--------------------------------------------------------------------------
                    ' ----- Create Server Link property
                    '--------------------------------------------------------------------------
                    '
                    webServerIO_ServerLink = webServerIO_requestProtocol & requestDomain & requestAppRootPath & webServerIO_requestPath & webServerIO_requestPage
                    If requestQueryString <> "" Then
                        webServerIO_ServerLink = webServerIO_ServerLink & "?" & requestQueryString
                    End If
                    '
                    '--------------------------------------------------------------------------
                    ' ----- Domain and path checks
                    '       must be before cookie check, because the cookie is only availabel on teh right path
                    '--------------------------------------------------------------------------
                    '
                    'Call AppendLog("main_init(), 2300")
                    '
                    If (RedirectLink = "") And (Not cpCore.domains.ServerMultiDomainMode) And (LCase(requestDomain) <> genericController.vbLCase(webServerIO_requestDomain)) Then
                        '
                        'Call AppendLog("main_init(), 2310 - exit in domain and path check")
                        '
                        Copy = "Redirecting to domain [" & webServerIO_requestDomain & "] because this site is configured to run on the current domain [" & requestDomain & "]"
                        If requestQueryString <> "" Then
                            Call webServerIO_Redirect2(webServerIO_requestProtocol & webServerIO_requestDomain & webServerIO_requestPath & webServerIO_requestPage & "?" & requestQueryString, Copy, False)
                        Else
                            Call webServerIO_Redirect2(webServerIO_requestProtocol & webServerIO_requestDomain & webServerIO_requestPath & webServerIO_requestPage, Copy, False)
                        End If
                        cpCore.docOpen = False '--- should be disposed by caller --- Call dispose
                        Return cpCore.docOpen
                    End If
                    '
                    ' -- this is to prevent an html page from coming out of the virtual path. (there should not be a link to it.)
                    ''
                    '' ----- Verify virtual path is not used on non-virtual sites
                    ''
                    'If (RedirectLink = "") And (requestAppRootPath = "/") And (InStr(1, webServerIO_requestPath, webServerIO_requestVirtualFilePath & "/", vbTextCompare) = 1) Then
                    '    Copy = "Redirecting because this site can not be run in the path [" & webServerIO_requestVirtualFilePath & "]"
                    '    webServerIO_requestPath = genericController.vbReplace(webServerIO_requestPath, cpCore.serverConfig.appConfig.name & "/", "", 1, 99, vbTextCompare)
                    '    Dim dstUrl As String = webServerIO_requestProtocol & webServerIO_requestDomain & webServerIO_requestPath & webServerIO_requestPage
                    '    If requestQueryString <> "" Then
                    '        dstUrl &= "?" & requestQueryString
                    '    End If
                    '    Call webServerIO_Redirect2(dstUrl, Copy, False)
                    'End If
                    '
                    ' ----- Create cpcore.main_ServerFormActionURL if it has not been overridden manually
                    '
                    If webServerIO_ServerFormActionURL = "" Then
                        webServerIO_ServerFormActionURL = webServerIO_requestProtocol & requestDomain & webServerIO_requestPath & webServerIO_requestPage
                    End If
                    '
                    '--------------------------------------------------------------------------
                    ' ----- Initialize Visit
                    '   AjaxFast does NOT support visit tracking
                    '   Ajax and RemoteMethods DO support visit tracking so they can handle authentication based permissions
                    '--------------------------------------------------------------------------
                    '
                    'Call AppendLog("main_init(), 2400")
                    '
                    ''hint = "Initializing Visit"
                    Call cpCore.visit.visit_init(cpCore.siteProperties.allowVisitTracking)
                    '
                    '--------------------------------------------------------------------------
                    ' ----- Process Early redirects, like PageNotFound
                    '       Must wait for here so admin message can be displayed explaining problem
                    '       Visit is necessary to identify admin
                    '--------------------------------------------------------------------------
                    '
                    If (RedirectLink <> "") Then
                        '
                        'Call AppendLog("main_init(), 2510 - exit for redirect")
                        '
                        Call webServerIO_Redirect2(RedirectLink, RedirectReason, IsPageNotFound)
                        cpCore.docOpen = False '--- should be disposed by caller --- Call dispose
                        Return cpCore.docOpen
                    End If
                    '
                    '--------------------------------------------------------------------------
                    '   Setup Debugging
                    '       must be on good domainname (for cookie), after authentication, after visit detect and after cpcore.main_ProcessFormToolsPanel
                    '--------------------------------------------------------------------------
                    '
                    ''hint = "Checking Debugging Hook (was Verbose Reporting)"
                    '
                    ' debug printed defaults on, so if not on, set it off and clear what was collected
                    '
                    If Not cpCore.visitProperty.getBoolean("AllowDebugging") Then
                        webServerIO_PageTestPointPrinting = False
                        cpCore.main_testPointMessage = ""
                    End If
                End If
                '
                '--------------------------------------------------------------------------------
                ' done at last
                '--------------------------------------------------------------------------------
                '
                '
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return cpCore.docOpen
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
                cpCore.handleExceptionAndRethrow(ex)
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
                Dim newCookie As New webServerController.cookieClass
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
                '
                Dim Link As String
                Dim iCookieName As String
                Dim iCookieValue As String
                Dim MethodName As String
                Dim s As String
                Dim domainListSplit() As String
                Dim domainSet As String
                Dim usedDomainList As String = ""
                Dim Ptr As Integer
                '
                iCookieName = genericController.encodeText(CookieName)
                iCookieValue = genericController.encodeText(CookieValue)
                '
                MethodName = "main_addResponseCookie"
                '
                If cpCore.docOpen And cpCore.htmlDoc.outputBufferEnabled Then
                    If (isMissing(domain)) And cpCore.domains.domainDetails.allowCrossLogin And genericController.EncodeBoolean(cpCore.siteProperties.getBoolean("Write Cookies to All Domains", True)) Then
                        '
                        ' no domain provided, new mode
                        '   - write cookie for current domains
                        '   - write an iframe that called the cross-Site login
                        '   - http://127.0.0.1/ccLib/clientside/cross.html?v=1&vPath=%2F&vExpires=1%2F1%2F2012
                        '
                        domainListSplit = Split(cpCore.main_ServerDomainCrossList, ",")
                        For Ptr = 0 To UBound(domainListSplit)
                            domainSet = Trim(domainListSplit(Ptr))
                            If (domainSet <> "") And (InStr(1, "," & usedDomainList & ",", "," & domainSet & ",", vbTextCompare) = 0) Then
                                usedDomainList = usedDomainList & "," & domainSet
                                '
                                ' valid, non-repeat domain
                                '
                                If genericController.vbLCase(domainSet) = genericController.vbLCase(requestDomain) Then
                                    '
                                    ' current domain, set cookie
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
                                        If webServerIO_bufferCookies <> "" Then
                                            webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf
                                        End If
                                        webServerIO_bufferCookies = webServerIO_bufferCookies & CookieName
                                        webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf & iCookieValue
                                        '
                                        s = ""
                                        If Not isMinDate(DateExpires) Then
                                            s = DateExpires.ToString
                                        End If
                                        webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf & s
                                        ' skip bc this is exactly the current domain and /rfc2109 requires a leading dot if explicit
                                        webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf
                                        'responseBufferCookie = responseBufferCookie & vbCrLf & domainSet
                                        '
                                        s = "/"
                                        If Not isMissing(Path) Then
                                            s = genericController.encodeText(Path)
                                        End If
                                        webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf & s
                                        '
                                        s = "false"
                                        If genericController.EncodeBoolean(Secure) Then
                                            s = "true"
                                        End If
                                        webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf & s
                                    End If
                                Else
                                    '
                                    ' other domain, add iframe
                                    '
                                    Dim C As String
                                    Link = "http://" & domainSet & "/ccLib/clientside/cross.html"
                                    Link = Link & "?n=" & EncodeRequestVariable(iCookieName)
                                    Link = Link & "&v=" & EncodeRequestVariable(iCookieValue)
                                    If Not isMissing(Path) Then
                                        C = genericController.encodeText(Path)
                                        C = EncodeRequestVariable(C)
                                        C = genericController.vbReplace(C, "/", "%2F")
                                        Link = Link & "&p=" & C
                                    End If
                                    If Not isMinDate(DateExpires) Then
                                        C = genericController.encodeText(DateExpires)
                                        C = EncodeRequestVariable(C)
                                        C = genericController.vbReplace(C, "/", "%2F")
                                        Link = Link & "&e=" & C
                                    End If
                                    Link = cpCore.htmlDoc.html_EncodeHTML(Link)
                                    cpCore.htmlDoc.htmlForEndOfBody = cpCore.htmlDoc.htmlForEndOfBody & vbCrLf & vbTab & "<iframe style=""display:none;"" width=""0"" height=""0"" src=""" & Link & """></iframe>"
                                End If
                            End If
                        Next
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
                            If webServerIO_bufferCookies <> "" Then
                                webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf
                            End If
                            webServerIO_bufferCookies = webServerIO_bufferCookies & CookieName
                            webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf & iCookieValue
                            '
                            s = ""
                            If Not isMinDate(DateExpires) Then
                                s = DateExpires.ToString
                            End If
                            webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf & s
                            '
                            s = ""
                            If Not isMissing(domain) Then
                                s = genericController.encodeText(domain)
                            End If
                            webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf & s
                            '
                            s = "/"
                            If Not isMissing(Path) Then
                                s = genericController.encodeText(Path)
                            End If
                            webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf & s
                            '
                            s = "false"
                            If genericController.EncodeBoolean(Secure) Then
                                s = "true"
                            End If
                            webServerIO_bufferCookies = webServerIO_bufferCookies & vbCrLf & s
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '
        '
        Public Sub web_setResponseStatus(status As String)
            webServerIO_bufferResponseStatus = status
        End Sub
        '
        '
        '
        Public Sub webServerIO_setResponseContentType(ContentType As Object)
            webServerIO_bufferContentType = CStr(ContentType)
        End Sub
        '
        '
        '
        Public Sub web_addResponseHeader(HeaderName As Object, HeaderValue As Object)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SetStreamHeader")
            '
            If cpCore.docOpen Then
                If webServerIO_bufferResponseHeader <> "" Then
                    webServerIO_bufferResponseHeader = webServerIO_bufferResponseHeader & vbCrLf
                End If
                webServerIO_bufferResponseHeader = webServerIO_bufferResponseHeader _
                    & genericController.vbReplace(genericController.encodeText(HeaderName), vbCrLf, "") _
                    & vbCrLf & genericController.vbReplace(genericController.encodeText(HeaderValue), vbCrLf, "")
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleLegacyError18("main_SetStreamHeader")
            '
        End Sub
        '
        '===========================================================================================
        ' ----- Redirect
        '
        '   Link is where you are going
        '   The Link argument should NOT be encoded. (it should still include spaces)
        '   Link may be '/index.asp', or 'index.asp' or 'http://www.docmc.main_com/index.asp'
        '       ShortLink is '/index.asp'
        '       FullLink is 'http://www.docmc.main_com/index.asp'
        '===========================================================================================
        '
        Public Sub webServerIO_Redirect2(ByVal NonEncodedLink As String, ByVal RedirectReason As String, ByVal IsPageNotFound As Boolean)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Redirect2")
            '
            Const rnRedirectCycleFlag = "cycleFlag"
            '
            Dim MethodName As String
            Dim Protocol As String
            Dim ProtocolEnd As String
            Dim EncodedLink As String
            'Dim kmafs As fileSystemClass
            Dim Copy As String
            '
            Dim RedirectLink As String
            Dim PageNotFoundSource As String
            Dim ShortLink As String
            Dim ServerShortLink As String
            Dim FullLink As String
            Dim redirectCycles As Integer
            '
            MethodName = "main_Redirect2(" & NonEncodedLink & "," & RedirectReason & "," & IsPageNotFound & ")"
            If cpCore.docOpen Then
                redirectCycles = cpCore.docProperties.getInteger(rnRedirectCycleFlag)
                '
                ' convert link to a long link on this domain
                '
                If genericController.vbLCase(Mid(NonEncodedLink, 1, 4)) = "http" Then
                    FullLink = NonEncodedLink
                Else
                    ShortLink = NonEncodedLink
                    ShortLink = genericController.ConvertLinkToShortLink(ShortLink, requestDomain, webServerIO_requestVirtualFilePath)
                    ShortLink = genericController.EncodeAppRootPath(ShortLink, webServerIO_requestVirtualFilePath, requestAppRootPath, requestDomain)
                    FullLink = webServerIO_requestProtocol & requestDomain & ShortLink
                End If
                If (NonEncodedLink = "") Then
                    '
                    ' Link is not valid
                    '
                    Call Err.Raise(ignoreInteger, "dll", "Redirect was called with a blank Link. Redirect Reason [" & RedirectReason & "]")
                    Exit Sub
                    '
                    ' changed to main_ServerLinksource because if a redirect is caused by a link forward, and the host page for the iis 404 is
                    ' the same as the destination of the link forward, this throws an error and does not forward. the only case where main_ServerLinksource is different
                    ' then main_ServerLink is the linkfforward/linkalias case.
                    '
                ElseIf (requestFormString = "") And (requestLinkSource = FullLink) Then
                    '
                    ' Loop redirect error, throw trap and block redirect to prevent loop
                    '
                    Call Err.Raise(ignoreInteger, "dll", "Redirect was called to the same URL, main_ServerLink is [" & webServerIO_ServerLink & "], main_ServerLinkSource is [" & requestLinkSource & "]. This redirect is only allowed if either the form or querystring has change to prevent cyclic redirects. Redirect Reason [" & RedirectReason & "]")
                    Exit Sub
                ElseIf IsPageNotFound Then
                    '
                    ' Do a PageNotFound then redirect
                    '
                    Call cpCore.log_appendLogPageNotFound(requestLinkSource)
                    If ShortLink <> "" Then
                        Call cpCore.db.executeSql("Update ccContentWatch set link=null where link=" & cpCore.db.encodeSQLText(ShortLink))
                    End If
                    '
                    If webServerIO_PageTestPointPrinting Then
                        '
                        ' Verbose - do not redirect, just print the link
                        '
                        EncodedLink = NonEncodedLink
                        'EncodedLink = encodeURL(NonEncodedLink)
                        'writeAltBuffer("<div style=""padding:20px;border:1px dashed black;background-color:white;color:black;"">" & RedirectReason & "<p>Click to continue the redirect to <a href=" & EncodedLink & ">" & EncodeHTML(NonEncodedLink) & "</a>...</p></div>")
                    Else
                        '
                        Call web_setResponseStatus("404 Not Found")
                        'Call writeAltBuffer("" _
                        '    & "<html>" _
                        '    & "<head>" _
                        '    & "<meta http-equiv=""Refresh"" content=""0; url=" & NonEncodedLink & """>" _
                        '    & "</head>" _
                        '    & "<body>" _
                        '    & "The page you requested could not be found. If you are not automatically forwarded, please click <a href=""" & NonEncodedLink & """>here</a>." _
                        '    & "</body>" _
                        '    & "</html>" _
                        '    & "")
                    End If
                Else

                    '
                    ' Go ahead and redirect
                    '
                    Copy = """" & FormatDateTime(cpCore.app_startTime, vbGeneralDate) & """,""" & requestDomain & """,""" & requestLinkSource & """,""" & NonEncodedLink & """,""" & RedirectReason & """"
                    logController.log_appendLog(cpCore, Copy, "performance", "redirects")
                    '
                    If webServerIO_PageTestPointPrinting Then
                        '
                        ' Verbose - do not redirect, just print the link
                        '

                        EncodedLink = NonEncodedLink
                        'EncodedLink = encodeURL(NonEncodedLink)
                        cpCore.htmlDoc.writeAltBuffer("<div style=""padding:20px;border:1px dashed black;background-color:white;color:black;"">" & RedirectReason & "<p>Click to continue the redirect to <a href=" & EncodedLink & ">" & cpCore.htmlDoc.html_EncodeHTML(NonEncodedLink) & "</a>...</p></div>")
                    Else
                        '
                        ' Redirect now
                        '
                        Call cpCore.main_ClearStream()
                        EncodedLink = genericController.EncodeURL(NonEncodedLink)

                        If (Not iisContext Is Nothing) Then
                            '
                            ' redirect and release application. HOWEVER -- the thread will continue so use responseOpen=false to abort as much activity as possible
                            '
                            iisContext.Response.Redirect(NonEncodedLink, False)
                            iisContext.ApplicationInstance.CompleteRequest()
                        Else
                            webServerIO_bufferRedirect = NonEncodedLink
                        End If
                        'responseBufferRedirect = NonEncodedLink
                    End If
                End If
                '
                ' ----- close the output stream
                '
                Call cpCore.doc_close()
            End If
            '
            ' Edit
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleLegacyError13(MethodName)
        End Sub
        '
        '
        Public Sub webServerIO_FlushStream()
            If (iisContext IsNot Nothing) Then
                iisContext.Response.Flush()
            End If
        End Sub


        ' main_Get the Head innerHTML for any page
        '
        Public Function webServerIO_GetHTMLInternalHead(ByVal main_IsAdminSite As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetHTMLInternalHead")
            '
            'If Not (true) Then Exit Function
            '
            Dim Parts() As String
            Dim FileList As String
            Dim Files() As String
            Dim Ptr As Integer
            Dim Pos As Integer
            Dim BaseHref As String
            Dim StyleTag As String
            Dim IDList As String
            Dim Cnt As Integer
            Dim StyleSheetLink As String
            Dim SQL As String
            Dim CS As Integer
            Dim OtherHeadTags As String
            Dim Copy As String
            Dim VirtualFilename As String
            Dim Ext As String
            '
            webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & "<!-- main_GetHTMLInternalHead called out of order. It must follow a content call, such as pageManager_GetHtmlBody, main_GetSectionPage, and main_GetContentPage -->"
            '
            ' stylesheets first -- for performance
            ' put stylesheets inline without processing
            '
            If cpCore.siteProperties.getBoolean("Allow CSS Reset") Then
                '
                ' reset styles
                '
                webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & "<link rel=""stylesheet"" type=""text/css"" href=""" & webServerIO_requestProtocol & webServerIO_requestDomain & "/ccLib/styles/ccreset.css"" >"
            End If
            webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & "<link rel=""stylesheet"" type=""text/css"" href=""/ccLib/Styles/" & defaultStyleFilename & """>"
            If Not main_IsAdminSite Then
                '
                ' site styles
                '
                webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & "<link rel=""stylesheet"" type=""text/css"" href=""" & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, "templates/styles.css") & """ >"
            End If
            '
            ' Template shared styles
            '
            ' !!!!! dont know why this was blocked. Running add-ons with shared styles need this in the admin site.
            FileList = cpCore.main_GetSharedStyleFileList(cpCore.htmlDoc.main_MetaContent_SharedStyleIDList, main_IsAdminSite)
            cpCore.htmlDoc.main_MetaContent_SharedStyleIDList = ""
            If FileList <> "" Then
                Files = Split(FileList, vbCrLf)
                For Ptr = 0 To UBound(Files)
                    If Files(Ptr) <> "" Then
                        Parts = Split(Files(Ptr) & "<<", "<")
                        If Parts(1) <> "" Then
                            webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & genericController.decodeHtml(Parts(1))
                        End If
                        webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & "<link rel=""stylesheet"" type=""text/css"" href=""" & webServerIO_requestProtocol & requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, Parts(0)) & """ >"
                        If Parts(2) <> "" Then
                            webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & genericController.decodeHtml(Parts(2))
                        End If
                        'End If
                    End If
                Next
            End If
            '
            ' Template exclusive styles
            '
            If cpCore.htmlDoc.main_MetaContent_TemplateStyleSheetTag <> "" Then
                webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cpCore.htmlDoc.main_MetaContent_TemplateStyleSheetTag
            End If
            '
            ' Page Styles
            '
            If cpCore.htmlDoc.main_MetaContent_StyleSheetTags <> "" Then
                webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cpCore.htmlDoc.main_MetaContent_StyleSheetTags
                cpCore.htmlDoc.main_MetaContent_StyleSheetTags = ""
            End If
            '
            ' Member Styles
            '
            If cpCore.user.styleFilename <> "" Then
                Call cpCore.htmlDoc.main_AddStylesheetLink2(webServerIO_requestProtocol & requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, cpCore.user.styleFilename), "member style")
                cpCore.user.styleFilename = ""
            End If
            '
            ' meta content
            '
            Copy = cpCore.main_GetLastMetaTitle()
            If Copy <> "" Then
                webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & "<title>" & Copy & "</title>"
            End If
            '
            Copy = cpCore.main_GetLastMetaKeywordList()
            If Copy <> "" Then
                webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & "<meta name=""keywords"" content=""" & Copy & """ >"
            End If
            '
            Copy = cpCore.main_GetLastMetaDescription()
            If Copy <> "" Then
                webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & "<meta name=""description"" content=""" & Copy & """ >"
            End If
            '
            ' favicon
            '
            VirtualFilename = cpCore.siteProperties.getText("faviconfilename")
            If VirtualFilename <> "" Then
                Pos = InStrRev(VirtualFilename, ".")
                If Pos > 0 Then
                    Ext = genericController.vbLCase(Mid(VirtualFilename, Pos))
                    Select Case Ext
                        Case ".ico"
                            webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & "<link rel=""icon"" type=""image/vnd.microsoft.icon"" href=""" & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, VirtualFilename) & """ >"
                        Case ".png"
                            webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & "<link rel=""icon"" type=""image/png"" href=""" & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, VirtualFilename) & """ >"
                        Case ".gif"
                            webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & "<link rel=""icon"" type=""image/gif"" href=""" & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, VirtualFilename) & """ >"
                        Case ".jpg"
                            webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & "<link rel=""icon"" type=""image/jpg"" href=""" & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, VirtualFilename) & """ >"
                    End Select
                End If
            End If
            '
            ' misc caching, etc
            '
            Dim encoding As String
            encoding = cpCore.htmlDoc.html_EncodeHTML(cpCore.siteProperties.getText("Site Character Encoding", "utf-8"))
            webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead _
                & OtherHeadTags _
                & cr & "<meta http-equiv=""content-type"" content=""text/html; charset=" & encoding & """ >" _
                & cr & "<meta http-equiv=""content-language"" content=""en-us"" >" _
                & cr & "<meta http-equiv=""cache-control"" content=""no-cache"" >" _
                & cr & "<meta http-equiv=""expires"" content=""-1"" >" _
                & cr & "<meta http-equiv=""pragma"" content=""no-cache"" >" _
                & cr & "<meta name=""generator"" content=""Contensive"" >"
            '& CR & "<meta http-equiv=""cache-control"" content=""no-store"" >"
            '
            ' no-follow
            '
            If webServerIO_response_NoFollow Then
                webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead _
                    & cr & "<meta name=""robots"" content=""nofollow"" >" _
                    & cr & "<meta name=""mssmarttagspreventparsing"" content=""true"" >"
            End If
            '
            ' Base is needed for Link Alias case where a slash is in the URL (page named 1/2/3/4/5)
            '
            BaseHref = webServerIO_ServerFormActionURL
            If main_IsAdminSite Then
                '
                ' no base in admin site
                '
            ElseIf BaseHref <> "" Then
                If cpCore.web_RefreshQueryString <> "" Then
                    BaseHref = BaseHref & "?" & cpCore.web_RefreshQueryString
                End If
                webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & "<base href=""" & BaseHref & """ >"
            End If
            '
            ' Head Javascript -- (should be) last for performance
            '
            webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead _
                & cr & "<script language=""JavaScript"" type=""text/javascript""  src=""" & webServerIO_requestProtocol & webServerIO_requestDomain & "/ccLib/ClientSide/Core.js""></script>" _
                & ""
            If cpCore.main_HeadScriptCnt > 0 Then
                For Ptr = 0 To cpCore.main_HeadScriptCnt - 1
                    With cpCore.main_HeadScripts(Ptr)
                        If (.addedByMessage <> "") And cpCore.visitProperty.getBoolean("AllowDebugging") Then
                            webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & "<!-- from " & .addedByMessage & " -->"
                        End If
                        If Not .IsLink Then
                            webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & "<script Language=""JavaScript"" type=""text/javascript"">" & .Text & cr & "</script>"
                        Else
                            webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & cr & "<script type=""text/javascript"" src=""" & .Text & """></script>"
                        End If
                    End With
                Next
                cpCore.main_HeadScriptCnt = 0
            End If
            '
            ' other head tags - always last
            '
            OtherHeadTags = cpCore.main_GetLastOtherHeadTags()
            If OtherHeadTags <> "" Then
                If Left(OtherHeadTags, 2) <> vbCrLf Then
                    OtherHeadTags = vbCrLf & OtherHeadTags
                End If
                webServerIO_GetHTMLInternalHead = webServerIO_GetHTMLInternalHead & genericController.vbReplace(OtherHeadTags, vbCrLf, cr)
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call cpCore.handleLegacyError18("main_GetHTMLInternalHead")
        End Function
    End Class
End Namespace