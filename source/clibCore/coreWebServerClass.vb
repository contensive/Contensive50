
Option Explicit On
Option Strict On

Imports Microsoft.Web.Administration

Namespace Contensive.Core
    ''' <summary>
    ''' Code dedicated to processing iis input and output. lazy Constructed. (see coreHtmlClass for html processing)
    ''' </summary>
    Public Class coreWebServerClass
        '
        Dim cpCore As cpCoreClass
        '
        '   State values that must be initialized before Init()
        '   Everything else is derived from these
        '
        Public web_InitCounter As Integer = 0
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
        Public Class cookieClass
            Public name As String
            Public value As String
        End Class
        Public requestCookies As Dictionary(Of String, cookieClass)
        '
        '====================================================================================================
        '
        Public Sub New(cpCore As cpCoreClass)
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
                LogFilename = "Temp\" & EncodeText(GetRandomInteger()) & ".Log"
                Cmd = "IISReset.exe"
                arg = "/restart >> """ & LogFilename & """"
                Call runProcess(cpCore, Cmd, arg, True)
                Copy = cpCore.privateFiles.ReadFile(LogFilename)
                Call cpCore.privateFiles.DeleteFile(LogFilename)
                Copy = Replace(Copy, vbCrLf, "\n")
                Copy = Replace(Copy, vbCr, "\n")
                Copy = Replace(Copy, vbLf, "\n")
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
                LogFilename = "Temp\" & EncodeText(GetRandomInteger()) & ".Log"
                Cmd = "%comspec% /c IISReset /stop >> """ & LogFilename & """"
                Call runProcess(cpCore, Cmd, , True)
                Copy = cpCore.privateFiles.ReadFile(LogFilename)
                Call cpCore.privateFiles.DeleteFile(LogFilename)
                Copy = Replace(Copy, vbCrLf, "\n")
                Copy = Replace(Copy, vbCr, "\n")
                Copy = Replace(Copy, vbLf, "\n")
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
                Dim LogFilename As String
                Dim Copy As String
                '
                Call Randomize()
                Cmd = "%comspec% /c IISReset /start >> """ & LogFilename & """"
                Call runProcess(cpCore, Cmd, , True)
                Copy = cpCore.privateFiles.ReadFile(LogFilename)
                Call cpCore.privateFiles.DeleteFile(LogFilename)
                Copy = Replace(Copy, vbCrLf, "\n")
                Copy = Replace(Copy, vbCr, "\n")
                Copy = Replace(Copy, vbLf, "\n")
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
        Friend Function web_Init() As Boolean
            Try
                '
                Dim forwardDomain As String
                Dim fileError As String
                Dim domainFound As Boolean
                Dim defaultDomainContentList As String = ""
                Dim domainSet As String
                Dim max As Integer
                Dim domainDetailsListText As String
                Dim domainArray() As String
                Dim domainRow As String
                Dim domainAttr() As String
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
                Dim Pos As Integer
                Dim RedirectLink As String = ""
                Dim Ptr As Integer
                Dim TextStartPointer As Integer
                Dim SQL As String
                'Dim TestHookValue As String
                Dim ShortPath As String = ""
                Dim ContentName As String = ""
                Dim Link As String
                Dim HardCodedPage As String
                Dim Filename As String
                Dim PathTest As String
                Dim Key As String
                Dim FormStart As Integer
                Dim Pointer As Integer
                Dim Copy As String
                Dim NameValue As String
                Dim NameValues() As String
                Dim Upload As coreUploadClass
                Dim KeyPointer As Integer
                Dim LinkSplit() As String
                Dim BinaryHeader() As Byte
                Dim ampSplit() As String
                Dim ampSplitCount As Integer
                Dim ampSplitPointer As Integer
                Dim ValuePair() As String
                Dim CS As Integer
                Dim Id As Integer
                Dim GroupName As String = ""
                Dim ErrorMessage As String
                Dim AjaxFunction As String
                Dim AjaxFastFunction As String
                Dim LinkForwardCriteria As String = ""
                Dim RemoteMethodFromPage As String = ""
                Dim RemoteMethodFromQueryString As String
                '
                '--------------------------------------------------------------------------
                ' 
                '--------------------------------------------------------------------------
                '
                'app = New appServicesClass(cp, initApplicationName)
                If (cpCore.appStatus <> applicationStatusEnum.ApplicationStatusReady) Then
                    '
                    ' did not initialize correctly
                    '
                Else
                    '
                    ' continue
                    '
                    web_InitCounter += 1
                    '
                    Call cpCore.web_SetStreamBuffer(True)
                    cpCore.docOpen = True
                    Call cpCore.web_setResponseContentType("text/html")
                    '
                    '--------------------------------------------------------------------------
                    ' ----- Process QueryString to cpcore.doc.main_InStreamArray
                    '       Do this first to set cpcore.main_ReadStreamJSForm, cpcore.main_ReadStreamJSProcess, cpcore.main_ReadStreamBinaryRead (must be in QS)
                    '--------------------------------------------------------------------------
                    '
                    cpCore.web_LinkForwardSource = ""
                    cpCore.web_LinkForwardError = ""
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
                    If cpCore.web_ReadStreamJSForm Then
                        '
                        ' Request comes from the browser while processing the javascript line
                        ' Add JSProcessQuery to QS
                        ' Add cpcore.main_ServerReferrerQS to QS
                        ' Add JSProcessForm to form
                        '
                        cpCore.web_BlockClosePageCopyright = True
                        cpCore.web_OutStreamDevice = cpCore.web_OutStreamJavaScript ' refactor - these should just be setContentType as a string so developers can set whatever
                        Call cpCore.web_setResponseContentType("application/javascript") ' refactor -- this should be setContentType
                        '
                        ' Add the cpcore.main_ServerReferrer QS to the cpcore.doc.main_InStreamArray()
                        '
                        ' ***** put back in because Add-ons need it for things like BID
                        ' /***** removed so the new system does not cpcore.main_Get the referrers QS
                        ' /***** the best we remember, only Aspen uses this, and they are moving
                        '
                        If InStr(1, requestReferrer, "?") <> 0 Then
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
                    ' ReCreate cpcore.main_ServerQueryString public
                    '--------------------------------------------------------------------------
                    ' strings created when context added
                    If False Then
                        For Each kvp As KeyValuePair(Of String, docPropertiesClass) In cpCore.docProperties.docPropertiesDict
                            requestQueryString &= "&" & kvp.Key & "=" & kvp.Value.Value
                        Next
                    End If
                    '
                    '--------------------------------------------------------------------------
                    ' ----- Process Form input to cpcore.doc.main_InStreamArray
                    '--------------------------------------------------------------------------
                    ' strings created when context added
                    If False Then
                        If requesFilesString <> "" Then
                            '
                            ' Form File input (main_ServerFormFile string)
                            '   0formname=formname&0filename=filename&0type=fileType&0file=tempfile&0error=errors&0size=fileSize&2..."
                            '
                            Ptr = 0
                            Do
                                Key = getSimpleNameValue(Ptr & "formname", requesFilesString, "", "&")
                                If Key <> "" Then
                                    Filename = DecodeURL(getSimpleNameValue(Ptr & "filename", requesFilesString, "", "&"))
                                    If InStr(1, Filename, "%") <> 0 Then
                                        '
                                        ' fix step 1 -- see if they accidently double encoded
                                        '
                                        Filename = DecodeURL(Filename)
                                    End If
                                    If InStr(1, Filename, "%") <> 0 Then
                                        '
                                        ' fix step 1 -- see if they accidently double encoded
                                        '
                                        Filename = Replace(Filename, "%", "-")
                                    End If
                                    fileError = DecodeURL(getSimpleNameValue(Ptr & "error", requesFilesString, "", "&"))
                                    If (fileError <> "") And (fileError <> "0") Then
                                        '
                                        ' log the error but still create the form field - it may be used as a flag for a form action
                                        '
                                        Throw New ApplicationException("Error uploading file, form file [" & Key & "], filename [" & Filename & "]. The error reported was [" & fileError & "]")
                                    End If
                                    Dim newProp As New docPropertiesClass
                                    With newProp
                                        .Name = Key
                                        .Value = Filename
                                        .NameValue = .Name & "=" & .Value
                                        .IsForm = True
                                        .IsFile = True
                                        .tmpPrivatefile = DecodeURL(getSimpleNameValue(Ptr & "tmpfile", requesFilesString, "", "&"))
                                        .FileSize = EncodeInteger(DecodeURL(getSimpleNameValue(Ptr & "size", requesFilesString, "", "&")))
                                        .fileType = DecodeURL(getSimpleNameValue(Ptr & "type", requesFilesString, "", "&"))
                                    End With
                                    cpCore.docProperties.setProperty(Key, newProp)
                                End If
                                Ptr = Ptr + 1
                            Loop While (Key <> "")
                        End If
                        If requestFormUseBinaryHeader Then
                            '
                            ' Form input through the Binary upload object
                            '
                            If Not IsNothing(requestFormBinaryHeader) Then
                                BinaryHeader = requestFormBinaryHeader
                                Upload = New coreUploadClass
                                Upload.binaryHeader = BinaryHeader
                                If Upload.Count > 0 Then
                                    For KeyPointer = 0 To Upload.Count - 1
                                        Key = Upload.Key(KeyPointer)
                                        Dim newProp As New docPropertiesClass
                                        With newProp
                                            If Upload.Form(Key).IsFile Then
                                                '
                                                ' Store a file
                                                '
                                                .Name = CStr(Key)
                                                .Value = Upload.Form(Key).Filename
                                                .Value = Replace(.Value, "#", "_")
                                                .Value = Replace(.Value, "?", "_")
                                                .Value = Replace(.Value, ":", "_")
                                                .NameValue = .Name & "=" & .Value
                                                .IsForm = True
                                                .IsFile = True
                                                .FileContent = Upload.Form(Key).Value
                                            Else
                                                '
                                                ' Store a form element
                                                '
                                                .Name = CStr(Key)
                                                .Value = System.Text.Encoding.Default.GetString(Upload.Form(Key).Value)
                                                .NameValue = .Name & "=" & .Value
                                                .IsForm = True
                                                .IsFile = False
                                            End If
                                        End With
                                        cpCore.docProperties.setProperty(Key, newProp)
                                    Next
                                End If
                                Upload = Nothing
                            End If
                        Else
                            '
                            ' Non-Binary Form - Read the form in with the standard Request Object
                            '
                            If requestFormString <> "" Then
                                ampSplit = Split(requestFormString, "&")
                                ampSplitCount = UBound(ampSplit) + 1
                                For ampSplitPointer = 0 To ampSplitCount - 1
                                    Dim newProp As New docPropertiesClass
                                    Dim propName As String
                                    Dim propValue As String = ""
                                    Dim propNameValue As String = ampSplit(ampSplitPointer)
                                    Dim propNameValuePair() As String = Split(propNameValue, "=")
                                    propName = DecodeResponseVariable(propNameValuePair(0))
                                    If UBound(propNameValuePair) > 0 Then
                                        propValue = DecodeResponseVariable(propNameValuePair(1))
                                    End If
                                    newProp.IsForm = True
                                    newProp.IsFile = False
                                    newProp.Name = propName
                                    newProp.Value = propValue
                                    newProp.NameValue = EncodeRequestVariable(propName) & "&" & EncodeRequestVariable(propValue)
                                    cpCore.docProperties.setProperty(propName, newProp)
                                Next
                            End If
                        End If
                    End If
                    '
                    '--------------------------------------------------------------------------
                    ' ReCreate cpCore.main_ServerForm, buffer out passwords
                    '--------------------------------------------------------------------------
                    '
                    'If (FormStart <> cpCore.docPropertiesArrayCount) Then
                    '    requestForm = ""
                    '    cpCore.main_ServerFormOriginal = ""
                    '    For Pointer = FormStart To cpCore.docPropertiesArrayCount - 1
                    '        NameValue = cpCore.docPropertiesDict(Pointer).NameValue
                    '        cpCore.main_ServerFormOriginal = cpCore.main_ServerFormOriginal & "&" & NameValue
                    '        NameValues = Split(NameValue, "=")
                    '        If UBound(NameValues) > 0 Then
                    '            If InStr(1, NameValues(0), "password", vbTextCompare) <> 0 Then
                    '                NameValue = NameValues(0) & "=[blocked]"
                    '            End If
                    '        End If
                    '        requestForm = requestForm & "&" & NameValue
                    '    Next
                    '    requestForm = Mid(requestForm, 2)
                    'End If
                    'cpCore.docPropertiesArrayLoaded = True
                    ''
                    ''--------------------------------------------------------------------------
                    '' Process cpCore.main_ServerCookies
                    ''--------------------------------------------------------------------------
                    ''
                    'If requestCookieString <> "" Then
                    '    ampSplit = Split(requestCookieString, "&")
                    '    ampSplitCount = UBound(ampSplit) + 1
                    '    For ampSplitPointer = 0 To ampSplitCount - 1
                    '        Dim newCookie As New docPropertiesClass
                    '        With newCookie
                    '            .NameValue = ampSplit(ampSplitPointer)
                    '            .Name = DecodeResponseVariable(CStr(ValuePair(0)))
                    '            If UBound(ValuePair) > 0 Then
                    '                .Value = DecodeResponseVariable(CStr(ValuePair(1)))
                    '            End If
                    '        End With
                    '        'If cookieArrayCount >= cookieArraySize Then
                    '        '    cookieArraySize = cookieArraySize + 10
                    '        '    ReDim Preserve requestCookies(cookieArraySize)
                    '        'End If
                    '        'With requestCookies(cookieArrayCount)
                    '        '    .NameValue = ampSplit(ampSplitPointer)
                    '        '    ValuePair = Split(.NameValue, "=")
                    '        '    .Name = DecodeResponseVariable(CStr(ValuePair(0)))
                    '        '    If UBound(ValuePair) > 0 Then
                    '        '        .Value = DecodeResponseVariable(CStr(ValuePair(1)))
                    '        '    End If
                    '        'End With
                    '        'cookieArrayCount = cookieArrayCount + 1
                    '    Next
                    'End If
                    '
                    '--------------------------------------------------------------------------
                    ' Set misc Server publics
                    '--------------------------------------------------------------------------
                    '
                    cpCore.web_MemberAction = cpCore.docProperties.getInteger("ma")
                        If (cpCore.web_MemberAction = 3) Or (cpCore.web_MemberAction = 2) Then
                            cpCore.web_MemberAction = 0
                            HardCodedPage = HardCodedPageLogoutLogin
                        End If
                        '
                        ' calculate now - but recalculate later - this does not include the /RemoteMethodFromQueryString case
                        '
                        '
                        cpCore.web_PageExcludeFromAnalytics = (AjaxFunction <> "") Or (AjaxFastFunction <> "") Or (RemoteMethodFromQueryString <> "")
                        '
                        '
                        ' Other Server variables
                        '
                        cpCore.web_requestReferer = requestReferrer
                        cpCore.web_requestPageReferer = requestReferrer
                        '
                        If requestSecure Then
                            cpCore.web_requestProtocol = "https://"
                        Else
                            cpCore.web_requestProtocol = "http://"
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
                        cpCore.main_ServerDomain = requestDomain
                        '
                        ' set cpcore.main_ServerDomainPrmary to the first valid defaultDomain entry
                        '
                        If cpCore.appConfig.domainList.Count > 0 Then
                            cpCore.main_ServerDomainPrimary = cpCore.appConfig.domainList(0)
                        Else
                            cpCore.main_ServerDomainPrimary = ""
                        End If
                        '
                        ' REFACTOR -- move to cpcore.domains class 
                        domainDetailsListText = EncodeText(cpCore.cache.getObject(Of String)("domainContentList"))
                        If Not String.IsNullOrEmpty(domainDetailsListText) Then
                            Try
                                cpCore.domains.domainDetailsList = cpCore.json.Deserialize(Of Dictionary(Of String, coreDomainsClass.domainDetailsClass))(domainDetailsListText)
                            Catch ex As Exception
                                cpCore.domains.domainDetailsList = Nothing
                            End Try
                        End If
                        If (cpCore.domains.domainDetailsList Is Nothing) Then
                            '
                            '  no cache found, build domainContentList from database
                            '
                            cpCore.domains.domainDetailsList = New Dictionary(Of String, coreDomainsClass.domainDetailsClass)
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
                                                Dim domainDetailsNew As New coreDomainsClass.domainDetailsClass
                                                domainDetailsNew.name = domainNameNew
                                                domainDetailsNew.rootPageId = EncodeInteger(row.Item(1).ToString)
                                                domainDetailsNew.noFollow = EncodeBoolean(row.Item(2).ToString)
                                                domainDetailsNew.typeId = EncodeInteger(row.Item(3).ToString)
                                                domainDetailsNew.visited = EncodeBoolean(row.Item(4).ToString)
                                                domainDetailsNew.id = EncodeInteger(row.Item(5).ToString)
                                                domainDetailsNew.forwardUrl = row.Item(6).ToString
                                                domainDetailsNew.defaultTemplateId = EncodeInteger(row.Item(7).ToString)
                                                domainDetailsNew.pageNotFoundPageId = EncodeInteger(row.Item(8).ToString)
                                                domainDetailsNew.allowCrossLogin = EncodeBoolean(row.Item(9).ToString)
                                                domainDetailsNew.forwardDomainId = EncodeInteger(row.Item(10).ToString)
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
                        For Each domain As String In cpCore.appConfig.domainList
                            If Not cpCore.domains.domainDetailsList.ContainsKey(domain.ToLower()) Then
                                Dim domainDetailsNew As New coreDomainsClass.domainDetailsClass
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
                                CS = cpCore.db.db_csInsertRecord("domains")
                                If cpCore.db.db_csOk(CS) Then
                                    cpCore.domains.domainDetails.id = cpCore.db.db_GetCSInteger(CS, "id")
                                    Call cpCore.db.db_setCS(CS, "name", requestDomain)
                                    Call cpCore.db.db_setCS(CS, "typeId", "1")
                                    Call cpCore.db.db_setCS(CS, "RootPageId", cpCore.domains.domainDetails.rootPageId.ToString)
                                    Call cpCore.db.db_setCS(CS, "ForwardUrl", cpCore.domains.domainDetails.forwardUrl)
                                    Call cpCore.db.db_setCS(CS, "NoFollow", cpCore.domains.domainDetails.noFollow.ToString)
                                    Call cpCore.db.db_setCS(CS, "Visited", cpCore.domains.domainDetails.visited.ToString)
                                    Call cpCore.db.db_setCS(CS, "DefaultTemplateId", cpCore.domains.domainDetails.defaultTemplateId.ToString)
                                    Call cpCore.db.db_setCS(CS, "PageNotFoundPageId", cpCore.domains.domainDetails.pageNotFoundPageId.ToString)
                                    Call cpCore.db.db_setCS(CS, "allowCrossLogin", cpCore.domains.domainDetails.allowCrossLogin.ToString)
                                End If
                                Call cpCore.db.db_csClose(CS)
                            End If
                            If Not cpCore.domains.domainDetails.visited Then
                                '
                                ' set visited true
                                '
                                SQL = "update ccdomains set visited=1 where name=" & cpCore.db.encodeSQLText(requestDomain)
                                Call cpCore.db.executeSql(SQL)
                                Call cpCore.cache.setKey("domainContentList", "", "domains")
                            End If
                            If cpCore.domains.domainDetails.typeId = 1 Then
                                '
                                ' normal domain, leave it
                                '
                            ElseIf InStr(1, requestPathPage, cpCore.siteProperties.adminURL, vbTextCompare) <> 0 Then
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
                                If InStr(1, cpCore.domains.domainDetails.forwardUrl, "://") = 0 Then
                                    cpCore.domains.domainDetails.forwardUrl = "http://" & cpCore.domains.domainDetails.forwardUrl
                                End If
                                Call cpCore.web_Redirect2(cpCore.domains.domainDetails.forwardUrl, "Forwarding to [" & cpCore.domains.domainDetails.forwardUrl & "] because the current domain [" & requestDomain & "] is in the domain content set to forward to this URL", False)
                                Return cpCore.docOpen
                            ElseIf (cpCore.domains.domainDetails.typeId = 3) And (cpCore.domains.domainDetails.forwardDomainId <> 0) And (cpCore.domains.domainDetails.forwardDomainId <> cpCore.domains.domainDetails.id) Then
                                '
                                ' forward to a replacement domain
                                '
                                forwardDomain = cpCore.main_GetRecordName("domains", cpCore.domains.domainDetails.forwardDomainId)
                                If forwardDomain <> "" Then
                                    Pos = InStr(1, requestLinkSource, requestDomain, vbTextCompare)
                                    If (Pos > 0) Then
                                        '
                                        'Call AppendLog("main_init(), 1720 - exit for forward domain")
                                        '
                                        cpCore.domains.domainDetails.forwardUrl = Mid(requestLinkSource, 1, Pos - 1) & forwardDomain & Mid(requestLinkSource, Pos + Len(requestDomain))
                                        'main_domainForwardUrl = Replace(main_ServerLinkSource, cpcore.main_ServerHost, forwardDomain)
                                        Call cpCore.web_Redirect2(cpCore.domains.domainDetails.forwardUrl, "Forwarding to [" & cpCore.domains.domainDetails.forwardUrl & "] because the current domain [" & requestDomain & "] is in the domain content set to forward to this replacement domain", False)
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
                                cpCore.main_MetaContent_NoFollow = True
                            End If

                        Else
                            '
                            ' domain not found
                            ' current host not in domainContent, add it and re-save the cache
                            '
                            Dim domainDetailsNew As New coreDomainsClass.domainDetailsClass
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
                            CS = cpCore.db.db_csInsertRecord("domains")
                            If cpCore.db.db_csOk(CS) Then
                                cpCore.domains.domainDetails.id = cpCore.db.db_GetCSInteger(CS, "id")
                                Call cpCore.db.db_setCS(CS, "name", requestDomain)
                                Call cpCore.db.db_setCS(CS, "typeid", "1")
                            End If
                            Call cpCore.db.db_csClose(CS)
                            '
                            updateDomainCache = True
                        End If
                        If (updateDomainCache) Then
                            '
                            ' if there was a change, update the cache
                            '
                            domainDetailsListText = cpCore.json.Serialize(cpCore.domains.domainDetailsList)
                            Call cpCore.cache.setKey("domainContentList", domainDetailsListText, "domains")
                        End If

                        '
                        cpCore.properties_site__AllowVisitTracking = cpCore.siteProperties.getBoolean("allowVisitTracking", True)
                        '
                        cpCore.web_requestVirtualFilePath = "/" & cpCore.appConfig.name
                        '
                        cpCore.web_requestContentWatchPrefix = cpCore.web_requestProtocol & requestDomain & cpCore.www_requestRootPath
                        cpCore.web_requestContentWatchPrefix = Mid(cpCore.web_requestContentWatchPrefix, 1, Len(cpCore.web_requestContentWatchPrefix) - 1)
                        '
                        'ServerSocketLoaded = False
                        '
                        ' ----- Server Identification
                        '       keep case from AppRootPath, but do not redirect
                        '       all cpcore.main_ContentWatch URLs should be checked (and changed to) AppRootPath
                        '
                        '
                        cpCore.web_requestPath = "/"
                        cpCore.web_requestPage = cpCore.siteProperties.serverPageDefault
                        TextStartPointer = InStrRev(requestPathPage, "/")
                        If TextStartPointer <> 0 Then
                            cpCore.web_requestPath = Mid(requestPathPage, 1, TextStartPointer)
                            cpCore.web_requestPage = Mid(requestPathPage, TextStartPointer + 1)
                        End If
                        ' cpcore.web_requestAppPath = Mid(cpcore.web_requestPath, Len(appRootPath) + 1)
                        cpCore.web_requestSecureURLRoot = "https://" & cpCore.main_ServerDomain & cpCore.www_requestRootPath
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
                        '        cpcore. docopen = False '--- should be disposed by caller --- Call dispose
                        '        Return cpcore. docopen
                        '    End If
                        'End If
                        '
                        ' ----- cpcore.main_RefreshQueryString
                        '
                        Id = cpCore.docProperties.getInteger("bid")
                        If Id <> 0 Then
                            Call cpCore.web_addRefreshQueryString("bid", Id.ToString)
                        End If
                        Id = cpCore.docProperties.getInteger("sid")
                        If Id <> 0 Then
                            Call cpCore.web_addRefreshQueryString("sid", Id.ToString)
                        End If
                        '
                        ' ----- Create Server Link property
                        '
                        cpCore.main_ServerLink = cpCore.web_requestProtocol & requestDomain & cpCore.www_requestRootPath & cpCore.web_requestPath & cpCore.web_requestPage
                        If requestQueryString <> "" Then
                            cpCore.main_ServerLink = cpCore.main_ServerLink & "?" & requestQueryString
                        End If
                        If requestLinkSource = "" Then
                            requestLinkSource = cpCore.main_ServerLink
                        End If
                        '
                        ' ----- File storage
                        '
                        'app.siteProperty_publicFileContentPathPrefix = cpcore.main_ServerVirtualPath & "/files/"
                        '
                        ' ----- Style tag
                        '
                        cpCore.main_AdminMessage = "For more information, please contact the <a href=""mailto:" & cpCore.siteProperties.emailAdmin & "?subject=Re: " & cpCore.main_ServerDomain & """>Site Administrator</A>."

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
                        cpCore.main_ServerLink = cpCore.web_requestProtocol & requestDomain & cpCore.www_requestRootPath & cpCore.web_requestPath & cpCore.web_requestPage
                        If requestQueryString <> "" Then
                            cpCore.main_ServerLink = cpCore.main_ServerLink & "?" & requestQueryString
                        End If
                        '
                        '--------------------------------------------------------------------------
                        ' ----- Domain and path checks
                        '       must be before cookie check, because the cookie is only availabel on teh right path
                        '--------------------------------------------------------------------------
                        '
                        'Call AppendLog("main_init(), 2300")
                        '
                        If (RedirectLink = "") And (Not cpCore.domains.ServerMultiDomainMode) And (LCase(requestDomain) <> LCase(cpCore.main_ServerDomain)) Then
                            '
                            'Call AppendLog("main_init(), 2310 - exit in domain and path check")
                            '
                            Copy = "Redirecting to domain [" & cpCore.main_ServerDomain & "] because this site is configured to run on the current domain [" & requestDomain & "]"
                            If requestQueryString <> "" Then
                                Call cpCore.web_Redirect2(cpCore.web_requestProtocol & cpCore.main_ServerDomain & cpCore.web_requestPath & cpCore.web_requestPage & "?" & requestQueryString, Copy, False)
                            Else
                                Call cpCore.web_Redirect2(cpCore.web_requestProtocol & cpCore.main_ServerDomain & cpCore.web_requestPath & cpCore.web_requestPage, Copy, False)
                            End If
                            cpCore.docOpen = False '--- should be disposed by caller --- Call dispose
                            Return cpCore.docOpen
                        End If
                        '
                        ' ----- Verify virtual path is not used on non-virtual sites
                        '
                        If (RedirectLink = "") And (cpCore.www_requestRootPath = "/") And (InStr(1, cpCore.web_requestPath, cpCore.web_requestVirtualFilePath & "/", vbTextCompare) = 1) Then
                            Copy = "Redirecting because this site can not be run in the path [" & cpCore.web_requestVirtualFilePath & "]"
                            cpCore.web_requestPath = Replace(cpCore.web_requestPath, cpCore.appConfig.name & "/", "", , , vbTextCompare)
                            If requestQueryString <> "" Then
                                Call cpCore.web_Redirect2(cpCore.web_requestProtocol & cpCore.main_ServerDomain & cpCore.www_requestRootPath & cpCore.web_requestPath & cpCore.web_requestPage & "?" & requestQueryString, Copy, False)
                            Else
                                Call cpCore.web_Redirect2(cpCore.web_requestProtocol & cpCore.main_ServerDomain & cpCore.www_requestRootPath & cpCore.web_requestPath & cpCore.web_requestPage, Copy, False)
                            End If
                        End If
                        '
                        ' ----- Create cpcore.main_ServerFormActionURL if it has not been overridden manually
                        '
                        If cpCore.web_ServerFormActionURL = "" Then
                            cpCore.web_ServerFormActionURL = cpCore.web_requestProtocol & requestDomain & cpCore.web_requestPath & cpCore.web_requestPage
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
                        Call cpCore.web_init_initVisit(cpCore.properties_site__AllowVisitTracking)
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
                            Call cpCore.web_Redirect2(RedirectLink, RedirectReason, IsPageNotFound)
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
                        If Not cpCore.main_VisitProperty_AllowDebugging Then
                            cpCore.main_PageTestPointPrinting = False
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
                    cookieValue = requestCookies(CookieName).Value
                End If
                ''
                'Dim Pointer As Integer
                'Dim UName As String
                ''
                'web_GetStreamCookie = ""
                'If web.cookieArrayCount > 0 Then
                '    UName = UCase(CookieName)
                '    For Pointer = 0 To web.cookieArrayCount - 1
                '        If UName = UCase(web.requestCookies(Pointer).Name) Then
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
                Dim newCookie As New coreWebServerClass.cookieClass
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
                Dim domainList As String
                Dim domainListSplit() As String
                Dim domainSet As String
                Dim usedDomainList As String = ""
                Dim Ptr As Integer
                '
                '
                iCookieName = EncodeText(CookieName)
                iCookieValue = EncodeText(CookieValue)
                '
                MethodName = "main_addResponseCookie"
                '
                If cpCore.docOpen And cpCore.docBufferEnabled Then
                    If (isMissing(domain)) And cpCore.domains.domainDetails.allowCrossLogin And EncodeBoolean(cpCore.siteProperties.getBoolean("Write Cookies to All Domains", True)) Then
                        '
                        ' no domain provided, new mode
                        '   - write cookie for current domains
                        '   - write an iframe that called the cross-Site login
                        '   - http://127.0.0.1/cclib/clientside/cross.html?v=1&vPath=%2F&vExpires=1%2F1%2F2012
                        '
                        domainListSplit = Split(cpCore.main_ServerDomainCrossList, ",")
                        For Ptr = 0 To UBound(domainListSplit)
                            domainSet = Trim(domainListSplit(Ptr))
                            If (domainSet <> "") And (InStr(1, "," & usedDomainList & ",", "," & domainSet & ",", vbTextCompare) = 0) Then
                                usedDomainList = usedDomainList & "," & domainSet
                                '
                                ' valid, non-repeat domain
                                '
                                If LCase(domainSet) = LCase(requestDomain) Then
                                    '
                                    ' current domain, set cookie
                                    '
                                    If (cpCore.iisContext IsNot Nothing) Then
                                        '
                                        ' Pass cookie to asp (compatibility)
                                        '
                                        cpCore.iisContext.Response.Cookies(iCookieName).Value = iCookieValue
                                        If Not isMinDate(DateExpires) Then
                                            cpCore.iisContext.Response.Cookies(iCookieName).Expires = DateExpires
                                        End If
                                        'main_ASPResponse.Cookies(iCookieName).domain = domainSet
                                        If Not isMissing(Path) Then
                                            cpCore.iisContext.Response.Cookies(iCookieName).Path = EncodeText(Path)
                                        End If
                                        If Not isMissing(Secure) Then
                                            cpCore.iisContext.Response.Cookies(iCookieName).Secure = Secure
                                        End If
                                    Else
                                        '
                                        ' Pass Cookie to non-asp parent
                                        '   crlf delimited list of name,value,expires,domain,path,secure
                                        '
                                        If cpCore._docBufferCookies <> "" Then
                                            cpCore._docBufferCookies = cpCore._docBufferCookies & vbCrLf
                                        End If
                                        cpCore._docBufferCookies = cpCore._docBufferCookies & CookieName
                                        cpCore._docBufferCookies = cpCore._docBufferCookies & vbCrLf & iCookieValue
                                        '
                                        s = ""
                                        If Not isMinDate(DateExpires) Then
                                            s = DateExpires.ToString
                                        End If
                                        cpCore._docBufferCookies = cpCore._docBufferCookies & vbCrLf & s
                                        ' skip bc this is exactly the current domain and /rfc2109 requires a leading dot if explicit
                                        cpCore._docBufferCookies = cpCore._docBufferCookies & vbCrLf
                                        'responseBufferCookie = responseBufferCookie & vbCrLf & domainSet
                                        '
                                        s = "/"
                                        If Not isMissing(Path) Then
                                            s = EncodeText(Path)
                                        End If
                                        cpCore._docBufferCookies = cpCore._docBufferCookies & vbCrLf & s
                                        '
                                        s = "false"
                                        If EncodeBoolean(Secure) Then
                                            s = "true"
                                        End If
                                        cpCore._docBufferCookies = cpCore._docBufferCookies & vbCrLf & s
                                    End If
                                Else
                                    '
                                    ' other domain, add iframe
                                    '
                                    Dim C As String
                                    Link = "http://" & domainSet & "/cclib/clientside/cross.html"
                                    Link = Link & "?n=" & EncodeRequestVariable(iCookieName)
                                    Link = Link & "&v=" & EncodeRequestVariable(iCookieValue)
                                    If Not isMissing(Path) Then
                                        C = EncodeText(Path)
                                        C = EncodeRequestVariable(C)
                                        C = Replace(C, "/", "%2F")
                                        Link = Link & "&p=" & C
                                    End If
                                    If Not isMinDate(DateExpires) Then
                                        C = EncodeText(DateExpires)
                                        C = EncodeRequestVariable(C)
                                        C = Replace(C, "/", "%2F")
                                        Link = Link & "&e=" & C
                                    End If
                                    Link = html_EncodeHTML(Link)
                                    cpCore.main_ClosePageHTML = cpCore.main_ClosePageHTML & vbCrLf & vbTab & "<iframe style=""display:none;"" width=""0"" height=""0"" src=""" & Link & """></iframe>"
                                End If
                            End If
                        Next
                    Else
                        '
                        ' Legacy mode - if no domain given just leave it off
                        '
                        If (cpCore.iisContext IsNot Nothing) Then
                            '
                            ' Pass cookie to asp (compatibility)
                            '
                            cpCore.iisContext.Response.Cookies(iCookieName).Value = iCookieValue
                            If Not isMinDate(DateExpires) Then
                                cpCore.iisContext.Response.Cookies(iCookieName).Expires = DateExpires
                            End If
                            'main_ASPResponse.Cookies(iCookieName).domain = domainSet
                            If Not isMissing(Path) Then
                                cpCore.iisContext.Response.Cookies(iCookieName).Path = EncodeText(Path)
                            End If
                            If Not isMissing(Secure) Then
                                cpCore.iisContext.Response.Cookies(iCookieName).Secure = Secure
                            End If
                        Else
                            '
                            ' Pass Cookie to non-asp parent
                            '   crlf delimited list of name,value,expires,domain,path,secure
                            '
                            If cpCore._docBufferCookies <> "" Then
                                cpCore._docBufferCookies = cpCore._docBufferCookies & vbCrLf
                            End If
                            cpCore._docBufferCookies = cpCore._docBufferCookies & CookieName
                            cpCore._docBufferCookies = cpCore._docBufferCookies & vbCrLf & iCookieValue
                            '
                            s = ""
                            If Not isMinDate(DateExpires) Then
                                s = DateExpires.ToString
                            End If
                            cpCore._docBufferCookies = cpCore._docBufferCookies & vbCrLf & s
                            '
                            s = ""
                            If Not isMissing(domain) Then
                                s = EncodeText(domain)
                            End If
                            cpCore._docBufferCookies = cpCore._docBufferCookies & vbCrLf & s
                            '
                            s = "/"
                            If Not isMissing(Path) Then
                                s = EncodeText(Path)
                            End If
                            cpCore._docBufferCookies = cpCore._docBufferCookies & vbCrLf & s
                            '
                            s = "false"
                            If EncodeBoolean(Secure) Then
                                s = "true"
                            End If
                            cpCore._docBufferCookies = cpCore._docBufferCookies & vbCrLf & s
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
    End Class
End Namespace