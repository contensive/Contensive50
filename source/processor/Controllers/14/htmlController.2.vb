

Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports Contensive.Core.Models.Entity

Namespace Contensive.Core.Controllers
    ''' <summary>
    ''' Tools used to assemble html document elements. This is not a storage for assembling a document (see docController)
    ''' </summary>
    Public Class htmlController

        '
        '========================================================================
        '   Display an icon with a link to the login form/cclib.net/admin area
        '========================================================================
        '
        Public Function main_GetLoginLink() As String
            Dim result As String = String.Empty
            Try
                '
                'If Not (true) Then Exit Function
                '
                Dim Link As String
                Dim IconFilename As String
                '
                If cpCore.siteProperties.getBoolean("AllowLoginIcon", True) Then
                    result = result & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">"
                    result = result & "<tr><td align=""right"">"
                    If cpCore.doc.authContext.isAuthenticatedContentManager(cpCore) Then
                        result = result & "<a href=""" & encodeHTML("/" & cpCore.serverConfig.appConfig.adminRoute) & """ target=""_blank"">"
                    Else
                        Link = cpCore.webServer.requestPage & "?" & cpCore.doc.refreshQueryString
                        Link = genericController.modifyLinkQuery(Link, RequestNameHardCodedPage, HardCodedPageLogin, True)
                        'Link = genericController.modifyLinkQuery(Link, RequestNameInterceptpage, LegacyInterceptPageSNLogin, True)
                        result = result & "<a href=""" & encodeHTML(Link) & """ >"
                    End If
                    IconFilename = cpCore.siteProperties.LoginIconFilename
                    If genericController.vbLCase(Mid(IconFilename, 1, 7)) <> "/ccLib/" Then
                        IconFilename = genericController.getCdnFileLink(cpCore, IconFilename)
                    End If
                    result = result & "<img alt=""Login"" src=""" & IconFilename & """ border=""0"" >"
                    result = result & "</A>"
                    result = result & "</td></tr></table>"
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        ''
        ''========================================================================
        ''   legacy
        ''========================================================================
        ''
        'Public Function main_GetClosePage(Optional ByVal AllowLogin As Boolean = True, Optional ByVal AllowTools As Boolean = True) As String
        '    main_GetClosePage = main_GetClosePage3(AllowLogin, AllowTools, False, False)
        'End Function
        ''
        ''========================================================================
        ''   legacy
        ''========================================================================
        ''
        'Public Function main_GetClosePage2(AllowLogin As Boolean, AllowTools As Boolean, BlockNonContentExtras As Boolean) As String
        '    Try
        '        main_GetClosePage2 = main_GetClosePage3(AllowLogin, AllowTools, False, False)
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex) : Throw
        '    End Try
        'End Function
        ''
        ''========================================================================
        ''   main_GetClosePage3
        ''       Public interface to end the page call
        ''       Must be called last on every public page
        ''       internally, you can NOT writeAltBuffer( main_GetClosePage3 ) because the stream is closed
        ''       call main_GetEndOfBody - main_Gets toolspanel and all html,menuing,etc needed to finish page
        ''       optionally calls main_dispose
        ''========================================================================
        ''
        'Public Function main_GetClosePage3(AllowLogin As Boolean, AllowTools As Boolean, BlockNonContentExtras As Boolean, doNotDisposeOnExit As Boolean) As String
        '    Try
        '        Return getBeforeEndOfBodyHtml(AllowLogin, AllowTools, BlockNonContentExtras, False)
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex) : Throw
        '    End Try
        'End Function
        '        '
        '        '========================================================================
        '        '   Write to the HTML stream
        '        '========================================================================
        '        ' refactor -- if this conversion goes correctly, all writeStream will mvoe to teh executeRoute which returns the string 
        '        Public Sub writeAltBuffer(ByVal Message As Object)
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("WriteStream")
        '            '
        '            If cpCore.doc.continueProcessing Then
        '                Select Case cpCore.webServer.outStreamDevice
        '                    Case htmlDoc_OutStreamJavaScript
        '                        Call webServerIO_JavaStream_Add(genericController.encodeText(Message))
        '                    Case Else

        '                        If (cpCore.webServer.iisContext IsNot Nothing) Then
        '                            cpCore.doc.isStreamWritten = True
        '                            Call cpCore.webServer.iisContext.Response.Write(genericController.encodeText(Message))
        '                        Else
        '                            cpCore.doc.docBuffer = cpCore.doc.docBuffer & genericController.encodeText(Message)
        '                        End If
        '                End Select
        '            End If
        '            '
        '            Exit Sub
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("writeAltBuffer")
        '        End Sub

        '        '
        '        '
        '        Private Sub webServerIO_JavaStream_Add(ByVal NewString As String)
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00375")
        '            '
        '            If cpCore.doc.javascriptStreamCount >= cpCore.doc.javascriptStreamSize Then
        '                cpCore.doc.javascriptStreamSize = cpCore.doc.javascriptStreamSize + htmlDoc_JavaStreamChunk
        '                ReDim Preserve cpCore.doc.javascriptStreamHolder(cpCore.doc.javascriptStreamSize)
        '            End If
        '            cpCore.doc.javascriptStreamHolder(cpCore.doc.javascriptStreamCount) = NewString
        '            cpCore.doc.javascriptStreamCount = cpCore.doc.javascriptStreamCount + 1
        '            Exit Sub
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_JavaStream_Add")
        '        End Sub



        'Public ReadOnly Property webServerIO_JavaStream_Text() As String
        '    Get
        '        Dim MsgLabel As String

        '        MsgLabel = "Msg" & genericController.encodeText(genericController.GetRandomInteger)

        '        webServerIO_JavaStream_Text = Join(cpCore.doc.javascriptStreamHolder, "")
        '        webServerIO_JavaStream_Text = genericController.vbReplace(webServerIO_JavaStream_Text, "'", "'+""'""+'")
        '        webServerIO_JavaStream_Text = genericController.vbReplace(webServerIO_JavaStream_Text, vbCrLf, "\n")
        '        webServerIO_JavaStream_Text = genericController.vbReplace(webServerIO_JavaStream_Text, vbCr, "\n")
        '        webServerIO_JavaStream_Text = genericController.vbReplace(webServerIO_JavaStream_Text, vbLf, "\n")
        '        webServerIO_JavaStream_Text = "var " & MsgLabel & " = '" & webServerIO_JavaStream_Text & "'; document.write( " & MsgLabel & " ); " & vbCrLf

        '    End Get
        'End Property
        ''
        ''
        ''
        'Public Sub webServerIO_addRefreshQueryString(ByVal Name As String, Optional ByVal Value As String = "")
        '    Try
        '        Dim temp() As String
        '        '
        '        If (InStr(1, Name, "=") > 0) Then
        '            temp = Split(Name, "=")
        '            cpCore.doc.refreshQueryString = genericController.ModifyQueryString(cpCore.doc.refreshQueryString, temp(0), temp(1), True)
        '        Else
        '            cpCore.doc.refreshQueryString = genericController.ModifyQueryString(cpCore.doc.refreshQueryString, Name, Value, True)
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex) : Throw
        '    End Try
        '
        '  End Sub
        '
        Public Function html_GetLegacySiteStyles() As String
            On Error GoTo ErrorTrap
            '
            If Not cpCore.doc.legacySiteStyles_Loaded Then
                cpCore.doc.legacySiteStyles_Loaded = True
                '
                ' compatibility with old sites - if they do not main_Get the default style sheet, put it in here
                '
                If False Then
                    html_GetLegacySiteStyles = "" _
                        & cr & "<!-- compatibility with legacy framework --><style type=text/css>" _
                        & cr & " .ccEditWrapper {border-top:1px solid #6a6;border-left:1px solid #6a6;border-bottom:1px solid #cec;border-right:1px solid #cec;}" _
                        & cr & " .ccEditWrapperInner {border-top:1px solid #cec;border-left:1px solid #cec;border-bottom:1px solid #6a6;border-right:1px solid #6a6;}" _
                        & cr & " .ccEditWrapperCaption {text-align:left;border-bottom:1px solid #888;padding:4px;background-color:#40C040;color:black;}" _
                        & cr & " .ccEditWrapperContent{padding:4px;}" _
                        & cr & " .ccHintWrapper {border:1px dashed #888;margin-bottom:10px}" _
                        & cr & " .ccHintWrapperContent{padding:10px;background-color:#80E080;color:black;}" _
                        & "</style>"
                Else
                    html_GetLegacySiteStyles = "" _
                        & cr & "<!-- compatibility with legacy framework --><style type=text/css>" _
                        & cr & " .ccEditWrapper {border:1px dashed #808080;}" _
                        & cr & " .ccEditWrapperCaption {text-align:left;border-bottom:1px solid #808080;padding:4px;background-color:#40C040;color:black;}" _
                        & cr & " .ccEditWrapperContent{padding:4px;}" _
                        & cr & " .ccHintWrapper {border:1px dashed #808080;margin-bottom:10px}" _
                        & cr & " .ccHintWrapperContent{padding:10px;background-color:#80E080;color:black;}" _
                        & "</style>"
                End If
            End If
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_GetLegacySiteStyles")
        End Function
        '
        '===================================================================================================
        '   Wrap the content in a common wrapper if authoring is enabled
        '===================================================================================================
        '
        Public Function html_GetAdminHintWrapper(ByVal Content As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetAdminHintWrapper")
            '
            'If Not (true) Then Exit Function
            '
            html_GetAdminHintWrapper = ""
            If cpCore.doc.authContext.isEditing("") Or cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                html_GetAdminHintWrapper = html_GetAdminHintWrapper & html_GetLegacySiteStyles()
                html_GetAdminHintWrapper = html_GetAdminHintWrapper _
                    & "<table border=0 width=""100%"" cellspacing=0 cellpadding=0><tr><td class=""ccHintWrapper"">" _
                        & "<table border=0 width=""100%"" cellspacing=0 cellpadding=0><tr><td class=""ccHintWrapperContent"">" _
                        & "<b>Administrator</b>" _
                        & "<br>" _
                        & "<br>" & genericController.encodeText(Content) _
                        & "</td></tr></table>" _
                    & "</td></tr></table>"
            End If

            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetAdminHintWrapper")
        End Function
        '
        '
        '
        Public Sub enableOutputBuffer(BufferOn As Boolean)
            Try
                If cpCore.doc.outputBufferEnabled Then
                    '
                    ' ----- once on, can not be turned off Response Object
                    '
                    cpCore.doc.outputBufferEnabled = BufferOn
                Else
                    '
                    ' ----- StreamBuffer off, allow on and off
                    '
                    cpCore.doc.outputBufferEnabled = BufferOn
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub

        '
        '========================================================================
        ' ----- Starts an HTML form for uploads
        '       Should be closed with main_GetUploadFormEnd
        '========================================================================
        '
        Public Function html_GetUploadFormStart(Optional ByVal ActionQueryString As String = Nothing) As String

            If ActionQueryString Is Nothing Then
                ActionQueryString = cpCore.doc.refreshQueryString
            End If
            On Error GoTo ErrorTrap
            '
            Dim iActionQueryString As String
            '
            iActionQueryString = genericController.ModifyQueryString(ActionQueryString, RequestNameRequestBinary, True, True)
            '
            html_GetUploadFormStart = "<form action=""" & cpCore.webServer.serverFormActionURL & "?" & iActionQueryString & """ ENCTYPE=""MULTIPART/FORM-DATA"" METHOD=""POST""  style=""display: inline;"" >"
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetUploadFormStart")
        End Function
        '
        '========================================================================
        ' ----- Closes an HTML form for uploads
        '========================================================================
        '
        Public Function html_GetUploadFormEnd() As String
            html_GetUploadFormEnd = html_GetFormEnd()
        End Function
        '
        '========================================================================
        ' ----- Starts an HTML form
        '       Should be closed with PrintFormEnd
        '========================================================================
        '
        Public Function html_GetFormStart(Optional ByVal ActionQueryString As String = Nothing, Optional ByVal htmlName As String = "", Optional ByVal htmlId As String = "", Optional ByVal htmlMethod As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFormStart3")
            '
            'If Not (true) Then Exit Function
            '
            Dim Ptr As Integer
            Dim MethodName As String
            Dim ActionQS As String
            Dim iMethod As String
            Dim ActionParts() As String
            Dim Action As String
            Dim QSParts() As String
            Dim QSNameValues() As String
            Dim QSName As String
            Dim QSValue As String
            Dim RefreshHiddens As String
            '
            MethodName = "main_GetFormStart3"
            '
            If ActionQueryString Is Nothing Then
                ActionQS = cpCore.doc.refreshQueryString
            Else
                ActionQS = ActionQueryString
            End If
            iMethod = genericController.vbLCase(htmlMethod)
            If iMethod = "" Then
                iMethod = "post"
            End If
            RefreshHiddens = ""
            Action = cpCore.webServer.serverFormActionURL
            '
            If (ActionQS <> "") Then
                If (iMethod <> "main_Get") Then
                    '
                    ' non-main_Get, put Action QS on end of Action
                    '
                    Action = Action & "?" & ActionQS
                Else
                    '
                    ' main_Get method, build hiddens for actionQS
                    '
                    QSParts = Split(ActionQS, "&")
                    For Ptr = 0 To UBound(QSParts)
                        QSNameValues = Split(QSParts(Ptr), "=")
                        If UBound(QSNameValues) = 0 Then
                            QSName = genericController.DecodeResponseVariable(QSNameValues(0))
                        Else
                            QSName = genericController.DecodeResponseVariable(QSNameValues(0))
                            QSValue = genericController.DecodeResponseVariable(QSNameValues(1))
                            RefreshHiddens = RefreshHiddens & cr & "<input type=""hidden"" name=""" & encodeHTML(QSName) & """ value=""" & encodeHTML(QSValue) & """>"
                        End If
                    Next
                End If
            End If
            '
            html_GetFormStart = "" _
                & cr & "<form name=""" & htmlName & """ id=""" & htmlId & """ action=""" & Action & """ method=""" & iMethod & """ style=""display: inline;"" >" _
                & RefreshHiddens _
                & ""
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' ----- Ends an HTML form
        '========================================================================
        '
        Public Function html_GetFormEnd() As String
            '
            html_GetFormEnd = "</form>"
            '
        End Function
        '
        '
        '
        Public Function html_GetFormInputText(ByVal TagName As String, Optional ByVal DefaultValue As String = "", Optional ByVal Height As String = "", Optional ByVal Width As String = "", Optional ByVal Id As String = "", Optional ByVal PasswordField As Boolean = False) As String
            html_GetFormInputText = html_GetFormInputText2(genericController.encodeText(TagName), genericController.encodeText(DefaultValue), genericController.EncodeInteger(Height), genericController.EncodeInteger(Width), genericController.encodeText(Id), PasswordField, False)
        End Function
        '
        '
        '
        Public Function html_GetFormInputText2(ByVal htmlName As String, Optional ByVal DefaultValue As String = "", Optional ByVal Height As Integer = -1, Optional ByVal Width As Integer = -1, Optional ByVal HtmlId As String = "", Optional ByVal PasswordField As Boolean = False, Optional ByVal Disabled As Boolean = False, Optional ByVal HtmlClass As String = "") As String
            On Error GoTo ErrorTrap
            '
            Dim iDefaultValue As String
            Dim iWidth As Integer
            Dim iHeight As Integer
            Dim TagID As String
            Dim TagDisabled As String = String.Empty
            '
            If True Then
                TagID = ""
                '
                iDefaultValue = encodeHTML(DefaultValue)
                If HtmlId <> "" Then
                    TagID = TagID & " id=""" & genericController.encodeEmptyText(HtmlId, "") & """"
                End If
                '
                If HtmlClass <> "" Then
                    TagID = TagID & " class=""" & HtmlClass & """"
                End If
                '
                iWidth = Width
                If (iWidth <= 0) Then
                    iWidth = cpCore.siteProperties.defaultFormInputWidth
                End If
                '
                iHeight = Height
                If (iHeight <= 0) Then
                    iHeight = cpCore.siteProperties.defaultFormInputTextHeight
                End If
                '
                If Disabled Then
                    TagDisabled = " disabled=""disabled"""
                End If
                '
                If PasswordField Then
                    html_GetFormInputText2 = "<input TYPE=""password"" NAME=""" & htmlName & """ SIZE=""" & iWidth & """ VALUE=""" & iDefaultValue & """" & TagID & TagDisabled & ">"
                ElseIf (iHeight = 1) And (InStr(1, iDefaultValue, """") = 0) Then
                    html_GetFormInputText2 = "<input TYPE=""Text"" NAME=""" & htmlName & """ SIZE=""" & iWidth.ToString & """ VALUE=""" & iDefaultValue & """" & TagID & TagDisabled & ">"
                Else
                    html_GetFormInputText2 = "<textarea NAME=""" & htmlName & """ ROWS=""" & iHeight.ToString & """ COLS=""" & iWidth.ToString & """" & TagID & TagDisabled & ">" & iDefaultValue & "</TEXTAREA>"
                End If
                cpCore.doc.formInputTextCnt = cpCore.doc.formInputTextCnt + 1
            End If
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetFormInputText2")
        End Function
        '
        '========================================================================
        ' ----- main_Get an HTML Form text input (or text area)
        '========================================================================
        '
        Public Function html_GetFormInputTextExpandable(ByVal TagName As String, Optional ByVal Value As String = "", Optional ByVal Rows As Integer = 0, Optional ByVal styleWidth As String = "100%", Optional ByVal Id As String = "", Optional ByVal PasswordField As Boolean = False) As String
            If Rows = 0 Then
                Rows = cpCore.siteProperties.defaultFormInputTextHeight
            End If
            html_GetFormInputTextExpandable = html_GetFormInputTextExpandable2(TagName, Value, Rows, styleWidth, Id, PasswordField, False, "")
        End Function
        '
        '========================================================================
        ' ----- main_Get an HTML Form text input (or text area)
        '   added disabled case
        '========================================================================
        '
        Public Function html_GetFormInputTextExpandable2(ByVal TagName As String, Optional ByVal Value As String = "", Optional ByVal Rows As Integer = 0, Optional ByVal styleWidth As String = "100%", Optional ByVal Id As String = "", Optional ByVal PasswordField As Boolean = False, Optional ByVal Disabled As Boolean = False, Optional ByVal HtmlClass As String = "") As String
            On Error GoTo ErrorTrap : Dim Tn As String : Tn = "cpCoreClass.GetFormInputTextExpandable2" ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            'If Not (true) Then Exit Function
            '
            Dim AttrDisabled As String = String.Empty
            Dim Value_Local As String
            Dim StyleWidth_Local As String
            Dim Rows_Local As Integer
            Dim IDRoot As String
            Dim EditorClosed As String
            Dim EditorOpened As String
            '
            Value_Local = encodeHTML(Value)
            IDRoot = Id
            If IDRoot = "" Then
                IDRoot = "TextArea" & cpCore.doc.formInputTextCnt
            End If
            '
            StyleWidth_Local = styleWidth
            If StyleWidth_Local = "" Then
                StyleWidth_Local = "100%"
            End If
            '
            Rows_Local = Rows
            If Rows_Local = 0 Then
                '
                ' need a default for this -- it should be different from a text, it should be for a textarea -- bnecause it is used differently
                '
                'Rows_Local = app.SiteProperty_DefaultFormInputTextHeight
                If Rows_Local = 0 Then
                    Rows_Local = 10
                End If
            End If
            If Disabled Then
                AttrDisabled = " disabled=""disabled"""
            End If
            '
            EditorClosed = "" _
                & cr & "<div class=""ccTextAreaHead"" ID=""" & IDRoot & "Head"">" _
                & cr2 & "<a href=""#"" onClick=""OpenTextArea('" & IDRoot & "');return false""><img src=""/ccLib/images/OpenUpRev1313.gif"" width=13 height=13 border=0>&nbsp;Full Screen</a>" _
                & cr & "</div>" _
                & cr & "<div class=""ccTextArea"">" _
                & cr2 & "<textarea ID=""" & IDRoot & """ NAME=""" & TagName & """ ROWS=""" & Rows_Local & """ Style=""width:" & StyleWidth_Local & ";""" & AttrDisabled & " onkeydown=""return cj.encodeTextAreaKey(this, event);"">" & Value_Local & "</TEXTAREA>" _
                & cr & "</div>" _
                & ""
            '
            EditorOpened = "" _
                & cr & "<div class=""ccTextAreaHeCursorTypeEnum.ADOPENed"" style=""display:none;"" ID=""" & IDRoot & "HeCursorTypeEnum.ADOPENed"">" _
                & cr & "<a href=""#"" onClick=""CloseTextArea('" & IDRoot & "');return false""><img src=""/ccLib/images/OpenDownRev1313.gif"" width=13 height=13 border=0>&nbsp;Full Screen</a>" _
                & cr2 & "</div>" _
                & cr & "<textarea class=""ccTextAreaOpened"" style=""display:none;"" ID=""" & IDRoot & "Opened"" NAME=""" & IDRoot & "Opened""" & AttrDisabled & " onkeydown=""return cj.encodeTextAreaKey(this, event);""></TEXTAREA>"
            '
            html_GetFormInputTextExpandable2 = "" _
                & "<div class=""" & HtmlClass & """>" _
                & genericController.htmlIndent(EditorClosed) _
                & genericController.htmlIndent(EditorOpened) _
                & "</div>"
            cpCore.doc.formInputTextCnt = cpCore.doc.formInputTextCnt + 1
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            cpCore.handleException(New Exception("Unexpected exception"))
            '
        End Function
        '
        '
        '
        Public Function html_GetFormInputDate(ByVal TagName As String, Optional ByVal DefaultValue As String = "", Optional ByVal Width As String = "", Optional ByVal Id As String = "") As String
            Dim result As String = String.Empty
            Try
                Dim HeadJS As String
                Dim DateString As String = String.Empty
                Dim DateValue As Date
                Dim iDefaultValue As String
                Dim iWidth As Integer
                Dim MethodName As String
                Dim iTagName As String
                Dim TagID As String
                Dim CalendarObjName As String
                Dim AnchorName As String
                '
                MethodName = "main_GetFormInputDate"
                '
                iTagName = genericController.encodeText(TagName)
                iDefaultValue = genericController.encodeEmptyText(DefaultValue, "")
                If (iDefaultValue = "0") Or (iDefaultValue = "12:00:00 AM") Then
                    iDefaultValue = ""
                Else
                    iDefaultValue = encodeHTML(iDefaultValue)
                End If
                If genericController.encodeEmptyText(Id, "") <> "" Then
                    TagID = " ID=""" & genericController.encodeEmptyText(Id, "") & """"
                End If
                '
                iWidth = genericController.encodeEmptyInteger(Width, 20)
                If iWidth = 0 Then
                    iWidth = 20
                End If
                '
                CalendarObjName = "Cal" & cpCore.doc.inputDateCnt
                AnchorName = "ACal" & cpCore.doc.inputDateCnt

                If cpCore.doc.inputDateCnt = 0 Then
                    HeadJS = "" _
                    & vbCrLf & "<SCRIPT LANGUAGE=""JavaScript"" SRC=""/ccLib/mktree/CalendarPopup.js""></SCRIPT>" _
                    & vbCrLf & "<SCRIPT LANGUAGE=""JavaScript"">" _
                    & vbCrLf & "var cal = new CalendarPopup();" _
                    & vbCrLf & "cal.showNavigationDropdowns();" _
                    & vbCrLf & "</SCRIPT>"
                    Call addScriptLink_Head("/ccLib/mktree/CalendarPopup.js", "Calendar Popup")
                    Call addScriptCode_head("var cal=new CalendarPopup();cal.showNavigationDropdowns();", "Calendar Popup")
                End If

                If IsDate(iDefaultValue) Then
                    DateValue = genericController.EncodeDate(iDefaultValue)
                    If Month(DateValue) < 10 Then
                        DateString = DateString & "0"
                    End If
                    DateString = DateString & Month(DateValue) & "/"
                    If Day(DateValue) < 10 Then
                        DateString = DateString & "0"
                    End If
                    DateString = DateString & Day(DateValue) & "/" & Year(DateValue)
                End If


                result = result _
                & vbCrLf & "<input TYPE=""text"" NAME=""" & iTagName & """ ID=""" & iTagName & """ VALUE=""" & iDefaultValue & """  SIZE=""" & iWidth & """>" _
                & vbCrLf & "<a HREF=""#"" Onclick = ""cal.select(document.getElementById('" & iTagName & "'),'" & AnchorName & "','MM/dd/yyyy','" & DateString & "'); return false;"" NAME=""" & AnchorName & """ ID=""" & AnchorName & """><img title=""Select a date"" alt=""Select a date"" src=""/ccLib/images/table.jpg"" width=12 height=10 border=0></A>" _
                & vbCrLf & ""

                cpCore.doc.inputDateCnt = cpCore.doc.inputDateCnt + 1
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        ' ----- main_Get an HTML Form file upload input
        '========================================================================
        '
        Public Function html_GetFormInputFile2(ByVal TagName As String, Optional ByVal htmlId As String = "", Optional ByVal HtmlClass As String = "") As String
            '
            html_GetFormInputFile2 = "<input TYPE=""file"" name=""" & TagName & """ id=""" & htmlId & """ class=""" & HtmlClass & """>"
            '
        End Function
        '
        ' ----- main_Get an HTML Form file upload input
        '
        Public Function html_GetFormInputFile(ByVal TagName As String, Optional ByVal htmlId As String = "") As String
            '
            html_GetFormInputFile = html_GetFormInputFile2(TagName, htmlId)
            '
        End Function
        '
        '========================================================================
        ' ----- main_Get an HTML Form input
        '========================================================================
        '
        Public Function html_GetFormInputRadioBox(ByVal TagName As String, ByVal TagValue As String, ByVal CurrentValue As String, Optional ByVal htmlId As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFormInputRadioBox")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim iTagName As String
            Dim iTagValue As String
            Dim iCurrentValue As String
            Dim ihtmlId As String
            Dim TagID As String = String.Empty
            '
            iTagName = genericController.encodeText(TagName)
            iTagValue = genericController.encodeText(TagValue)
            iCurrentValue = genericController.encodeText(CurrentValue)
            ihtmlId = genericController.encodeEmptyText(htmlId, "")
            If ihtmlId <> "" Then
                TagID = " ID=""" & ihtmlId & """"
            End If
            '
            MethodName = "main_GetFormInputRadioBox"
            '
            If iTagValue = iCurrentValue Then
                html_GetFormInputRadioBox = "<input TYPE=""Radio"" NAME=""" & iTagName & """ VALUE=""" & iTagValue & """ checked" & TagID & ">"
            Else
                html_GetFormInputRadioBox = "<input TYPE=""Radio"" NAME=""" & iTagName & """ VALUE=""" & iTagValue & """" & TagID & ">"
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        '   Legacy
        '========================================================================
        '
        Public Function html_GetFormInputCheckBox(ByVal TagName As String, Optional ByVal DefaultValue As String = "", Optional ByVal htmlId As String = "") As String
            html_GetFormInputCheckBox = html_GetFormInputCheckBox2(genericController.encodeText(TagName), genericController.EncodeBoolean(DefaultValue), genericController.encodeText(htmlId))
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function html_GetFormInputCheckBox2(ByVal TagName As String, Optional ByVal DefaultValue As Boolean = False, Optional ByVal HtmlId As String = "", Optional ByVal Disabled As Boolean = False, Optional ByVal HtmlClass As String = "") As String
            On Error GoTo ErrorTrap
            '
            html_GetFormInputCheckBox2 = "<input TYPE=""CheckBox"" NAME=""" & TagName & """ VALUE=""1"""
            If HtmlId <> "" Then
                html_GetFormInputCheckBox2 = html_GetFormInputCheckBox2 & " id=""" & HtmlId & """"
            End If
            If HtmlClass <> "" Then
                html_GetFormInputCheckBox2 = html_GetFormInputCheckBox2 & " class=""" & HtmlClass & """"
            End If
            If DefaultValue Then
                html_GetFormInputCheckBox2 = html_GetFormInputCheckBox2 & " checked=""checked"""
            End If
            If Disabled Then
                html_GetFormInputCheckBox2 = html_GetFormInputCheckBox2 & " disabled=""disabled"""
            End If
            html_GetFormInputCheckBox2 = html_GetFormInputCheckBox2 & ">"
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetFormInputCheckBox2")
        End Function
        '
        '========================================================================
        '   Create a List of Checkboxes based on a contentname and a list of IDs that should be checked
        '
        '   For instance, list out a checklist of all public groups, with the ones checked that this member belongs to
        '       PrimaryContentName = "People"
        '       PrimaryRecordID = MemberID
        '       SecondaryContentName = "Groups"
        '       SecondaryContentSelectCriteria = "ccGroups.PublicJoin<>0"
        '       RulesContentName = "Member Rules"
        '       RulesPrimaryFieldName = "MemberID"
        '       RulesSecondaryFieldName = "GroupID"
        '========================================================================
        '
        Public Function html_GetFormInputCheckListByIDList(ByVal TagName As String, ByVal SecondaryContentName As String, ByVal CheckedIDList As String, Optional ByVal CaptionFieldName As String = "", Optional ByVal readOnlyField As Boolean = False) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFormInputCheckListByIDList")
            '
            'If Not (true) Then Exit Function
            '
            Dim SQL As String
            Dim CS As Integer
            Dim main_MemberShipCount As Integer
            Dim main_MemberShipSize As Integer
            Dim main_MemberShipPointer As Integer
            Dim SectionName As String
            Dim GroupCount As Integer
            Dim main_MemberShip() As Integer
            Dim SecondaryTablename As String
            Dim SecondaryContentID As Integer
            Dim rulesTablename As String
            Dim Result As String = String.Empty
            Dim MethodName As String
            Dim iCaptionFieldName As String
            Dim GroupName As String
            Dim GroupCaption As String
            Dim CanSeeHiddenFields As Boolean
            Dim SecondaryCDef As Models.Complex.cdefModel
            Dim ContentIDList As String = String.Empty
            Dim Found As Boolean
            Dim RecordID As Integer
            Dim SingularPrefix As String
            '
            MethodName = "main_GetFormInputCheckListByIDList"
            '
            iCaptionFieldName = genericController.encodeEmptyText(CaptionFieldName, "name")
            '
            ' ----- Gather all the SecondaryContent that associates to the PrimaryContent
            '
            SecondaryCDef = Models.Complex.cdefModel.getCdef(cpCore, SecondaryContentName)
            SecondaryTablename = SecondaryCDef.ContentTableName
            SecondaryContentID = SecondaryCDef.Id
            SecondaryCDef.childIdList(cpCore).Add(SecondaryContentID)
            SingularPrefix = genericController.GetSingular(SecondaryContentName) & "&nbsp;"
            '
            ' ----- Gather all the records, sorted by ContentName
            '
            SQL = "SELECT " & SecondaryTablename & ".ID AS ID, ccContent.Name AS SectionName, " & SecondaryTablename & "." & iCaptionFieldName & " AS GroupCaption, " & SecondaryTablename & ".name AS GroupName, " & SecondaryTablename & ".SortOrder" _
            & " FROM " & SecondaryTablename & " LEFT JOIN ccContent ON " & SecondaryTablename & ".ContentControlID = ccContent.ID" _
            & " Where (" & SecondaryTablename & ".Active<>" & SQLFalse & ")" _
            & " And (ccContent.Active<>" & SQLFalse & ")" _
            & " And (" & SecondaryTablename & ".ContentControlID IN (" & ContentIDList & "))"
            SQL &= "" _
                & " GROUP BY " & SecondaryTablename & ".ID, ccContent.Name, " & SecondaryTablename & "." & iCaptionFieldName & ", " & SecondaryTablename & ".name, " & SecondaryTablename & ".SortOrder" _
                & " ORDER BY ccContent.Name, " & SecondaryTablename & "." & iCaptionFieldName
            CS = cpCore.db.csOpenSql(SQL)
            If cpCore.db.csOk(CS) Then
                SectionName = ""
                GroupCount = 0
                CanSeeHiddenFields = cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore)
                Do While cpCore.db.csOk(CS)
                    GroupName = cpCore.db.csGetText(CS, "GroupName")
                    If (Mid(GroupName, 1, 1) <> "_") Or CanSeeHiddenFields Then
                        RecordID = cpCore.db.csGetInteger(CS, "ID")
                        GroupCaption = cpCore.db.csGetText(CS, "GroupCaption")
                        If GroupCaption = "" Then
                            GroupCaption = GroupName
                        End If
                        If GroupCaption = "" Then
                            GroupCaption = SingularPrefix & RecordID
                        End If
                        If GroupCount <> 0 Then
                            ' leave this between checkboxes - it is searched in the admin page
                            Result = Result & "<br >" & vbCrLf
                        End If
                        If genericController.IsInDelimitedString(CheckedIDList, CStr(RecordID), ",") Then
                            Found = True
                        Else
                            Found = False
                        End If
                        ' must leave the first hidden with the value in this form - it is searched in the admin pge
                        Result = Result & "<input type=hidden name=""" & TagName & "." & GroupCount & ".ID"" value=" & RecordID & ">"
                        If readOnlyField And Not Found Then
                            Result = Result & "<input type=checkbox disabled>"
                        ElseIf readOnlyField Then
                            Result = Result & "<input type=checkbox disabled checked>"
                            Result = Result & "<input type=""hidden"" name=""" & TagName & "." & GroupCount & ".ID"" value=" & RecordID & ">"
                        ElseIf Found Then
                            Result = Result & "<input type=checkbox name=""" & TagName & "." & GroupCount & """ checked>"
                        Else
                            Result = Result & "<input type=checkbox name=""" & TagName & "." & GroupCount & """>"
                        End If
                        Result = Result & SpanClassAdminNormal & GroupCaption
                        GroupCount = GroupCount + 1
                    End If
                    cpCore.db.csGoNext(CS)
                Loop
                Result = Result & "<input type=""hidden"" name=""" & TagName & ".RowCount"" value=""" & GroupCount & """>" & vbCrLf
            End If
            cpCore.db.csClose(CS)
            html_GetFormInputCheckListByIDList = Result
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18(MethodName)
            '
        End Function
        '
        ' -----
        '
        Public Function html_GetFormInputCS(ByVal CSPointer As Integer, ByVal ContentName As String, ByVal FieldName As String, Optional ByVal Height As Integer = 1, Optional ByVal Width As Integer = 40, Optional ByVal htmlId As String = "") As String
            Dim returnResult As String = String.Empty
            Try
                Dim IsEmptyList As Boolean
                Dim Stream As String
                Dim MethodName As String
                Dim FieldCaption As String
                Dim FieldValueVariant As String = String.Empty
                Dim FieldValueText As String
                Dim FieldValueInteger As Integer
                Dim fieldTypeId As Integer
                Dim FieldReadOnly As Boolean
                Dim FieldPassword As Boolean
                Dim fieldFound As Boolean
                Dim FieldLookupContentID As Integer
                Dim FieldMemberSelectGroupID As Integer
                Dim FieldLookupContentName As String
                Dim Contentdefinition As Models.Complex.cdefModel
                Dim FieldHTMLContent As Boolean
                Dim CSLookup As Integer
                Dim FieldLookupList As String = String.Empty
                '
                MethodName = "main_GetFormInputCS"
                '
                Stream = ""
                If True Then
                    fieldFound = False
                    Contentdefinition = Models.Complex.cdefModel.getCdef(cpCore, ContentName)
                    For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In Contentdefinition.fields
                        Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                        With field
                            If genericController.vbUCase(.nameLc) = genericController.vbUCase(FieldName) Then
                                FieldValueVariant = .defaultValue
                                fieldTypeId = .fieldTypeId
                                FieldReadOnly = .ReadOnly
                                FieldCaption = .caption
                                FieldPassword = .Password
                                FieldHTMLContent = .htmlContent
                                FieldLookupContentID = .lookupContentID
                                FieldLookupList = .lookupList
                                FieldMemberSelectGroupID = .MemberSelectGroupID
                                fieldFound = True
                            End If
                        End With
                    Next
                    If Not fieldFound Then
                        cpCore.handleException(New Exception("Field [" & FieldName & "] was not found in Content Definition [" & ContentName & "]"))
                    Else
                        '
                        ' main_Get the current value if the record was found
                        '
                        If cpCore.db.csOk(CSPointer) Then
                            FieldValueVariant = cpCore.db.cs_getValue(CSPointer, FieldName)
                        End If
                        '
                        If FieldPassword Then
                            '
                            ' Handle Password Fields
                            '
                            FieldValueText = genericController.encodeText(FieldValueVariant)
                            returnResult = html_GetFormInputText2(FieldName, FieldValueText, Height, Width, , True)
                        Else
                            '
                            ' Non Password field by fieldtype
                            '
                            Select Case fieldTypeId
                            '
                            '
                            '
                                Case FieldTypeIdHTML
                                    FieldValueText = genericController.encodeText(FieldValueVariant)
                                    If FieldReadOnly Then
                                        returnResult = FieldValueText
                                    Else
                                        returnResult = getFormInputHTML(FieldName, FieldValueText, , Width.ToString)
                                    End If
                                '
                                ' html files, read from cdnFiles and use html editor
                                '
                                Case FieldTypeIdFileHTML
                                    FieldValueText = genericController.encodeText(FieldValueVariant)
                                    If FieldValueText <> "" Then
                                        FieldValueText = cpCore.cdnFiles.readFile(FieldValueText)
                                    End If
                                    If FieldReadOnly Then
                                        returnResult = FieldValueText
                                    Else
                                        'Height = encodeEmptyInteger(Height, 4)
                                        returnResult = getFormInputHTML(FieldName, FieldValueText, , Width.ToString)
                                    End If
                                '
                                ' text cdnFiles files, read from cdnFiles and use text editor
                                '
                                Case FieldTypeIdFileText
                                    FieldValueText = genericController.encodeText(FieldValueVariant)
                                    If FieldValueText <> "" Then
                                        FieldValueText = cpCore.cdnFiles.readFile(FieldValueText)
                                    End If
                                    If FieldReadOnly Then
                                        returnResult = FieldValueText
                                    Else
                                        'Height = encodeEmptyInteger(Height, 4)
                                        returnResult = html_GetFormInputText2(FieldName, FieldValueText, Height, Width)
                                    End If
                                '
                                ' text public files, read from cpcore.cdnFiles and use text editor
                                '
                                Case FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript
                                    FieldValueText = genericController.encodeText(FieldValueVariant)
                                    If FieldValueText <> "" Then
                                        FieldValueText = cpCore.cdnFiles.readFile(FieldValueText)
                                    End If
                                    If FieldReadOnly Then
                                        returnResult = FieldValueText
                                    Else
                                        'Height = encodeEmptyInteger(Height, 4)
                                        returnResult = html_GetFormInputText2(FieldName, FieldValueText, Height, Width)
                                    End If
                                '
                                '
                                '
                                Case FieldTypeIdBoolean
                                    If FieldReadOnly Then
                                        returnResult = genericController.encodeText(genericController.EncodeBoolean(FieldValueVariant))
                                    Else
                                        returnResult = html_GetFormInputCheckBox2(FieldName, genericController.EncodeBoolean(FieldValueVariant))
                                    End If
                                '
                                '
                                '
                                Case FieldTypeIdAutoIdIncrement
                                    returnResult = genericController.encodeText(genericController.EncodeNumber(FieldValueVariant))
                                '
                                '
                                '
                                Case FieldTypeIdFloat, FieldTypeIdCurrency, FieldTypeIdInteger
                                    FieldValueVariant = genericController.EncodeNumber(FieldValueVariant).ToString()
                                    If FieldReadOnly Then
                                        returnResult = genericController.encodeText(FieldValueVariant)
                                    Else
                                        returnResult = html_GetFormInputText2(FieldName, genericController.encodeText(FieldValueVariant), Height, Width)
                                    End If
                                '
                                '
                                '
                                Case FieldTypeIdFile
                                    FieldValueText = genericController.encodeText(FieldValueVariant)
                                    If FieldReadOnly Then
                                        returnResult = FieldValueText
                                    Else
                                        returnResult = FieldValueText & "<BR >change: " & html_GetFormInputFile(FieldName, genericController.encodeText(FieldValueVariant))
                                    End If
                                '
                                '
                                '
                                Case FieldTypeIdFileImage
                                    FieldValueText = genericController.encodeText(FieldValueVariant)
                                    If FieldReadOnly Then
                                        returnResult = FieldValueText
                                    Else
                                        returnResult = "<img src=""" & genericController.getCdnFileLink(cpCore, FieldValueText) & """><BR >change: " & html_GetFormInputFile(FieldName, genericController.encodeText(FieldValueVariant))
                                    End If
                                '
                                '
                                '
                                Case FieldTypeIdLookup
                                    FieldValueInteger = genericController.EncodeInteger(FieldValueVariant)
                                    FieldLookupContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, FieldLookupContentID)
                                    If FieldLookupContentName <> "" Then
                                        '
                                        ' Lookup into Content
                                        '
                                        If FieldReadOnly Then
                                            CSPointer = cpCore.db.cs_open2(FieldLookupContentName, FieldValueInteger)
                                            If cpCore.db.csOk(CSLookup) Then
                                                returnResult = csController.getTextEncoded(cpCore, CSLookup, "name")
                                            End If
                                            Call cpCore.db.csClose(CSLookup)
                                        Else
                                            returnResult = main_GetFormInputSelect2(FieldName, FieldValueInteger, FieldLookupContentName, "", "", "", IsEmptyList)
                                        End If
                                    ElseIf FieldLookupList <> "" Then
                                        '
                                        ' Lookup into LookupList
                                        '
                                        returnResult = getInputSelectList2(FieldName, FieldValueInteger, FieldLookupList, "", "")
                                    Else
                                        '
                                        ' Just call it text
                                        '
                                        returnResult = html_GetFormInputText2(FieldName, CStr(FieldValueInteger), Height, Width)
                                    End If
                                '
                                '
                                '
                                Case FieldTypeIdMemberSelect
                                    FieldValueInteger = genericController.EncodeInteger(FieldValueVariant)
                                    returnResult = getInputMemberSelect(FieldName, FieldValueInteger, FieldMemberSelectGroupID)
                                    '
                                    '
                                    '
                                Case Else
                                    FieldValueText = genericController.encodeText(FieldValueVariant)
                                    If FieldReadOnly Then
                                        returnResult = FieldValueText
                                    Else
                                        If FieldHTMLContent Then
                                            returnResult = getFormInputHTML(FieldName, FieldValueText, CStr(Height), CStr(Width), FieldReadOnly, False)
                                            'main_GetFormInputCS = main_GetFormInputActiveContent(fieldname, FieldValueText, height, width)
                                        Else
                                            returnResult = html_GetFormInputText2(FieldName, FieldValueText, Height, Width)
                                        End If
                                    End If
                            End Select
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ' ----- Print an HTML Form Button element named BUTTON
        '========================================================================
        '
        Public Function html_GetFormButton(ByVal ButtonLabel As String, Optional ByVal Name As String = "", Optional ByVal htmlId As String = "", Optional ByVal OnClick As String = "") As String
            html_GetFormButton = html_GetFormButton2(ButtonLabel, Name, htmlId, OnClick, False)
        End Function
        '
        '========================================================================
        ' ----- Print an HTML Form Button element named BUTTON
        '========================================================================
        '
        Public Function html_GetFormButton2(ByVal ButtonLabel As String, Optional ByVal Name As String = "button", Optional ByVal htmlId As String = "", Optional ByVal OnClick As String = "", Optional ByVal Disabled As Boolean = False) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFormButton2")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim iOnClick As String
            Dim TagID As String
            Dim s As String
            '
            MethodName = "main_GetFormButton2"
            '
            s = "<input TYPE=""SUBMIT""" _
                & " NAME=""" & genericController.encodeEmptyText(Name, "button") & """" _
                & " VALUE=""" & genericController.encodeText(ButtonLabel) & """" _
                & " OnClick=""" & genericController.encodeEmptyText(OnClick, "") & """" _
                & " ID=""" & genericController.encodeEmptyText(htmlId, "") & """"
            If Disabled Then
                s = s & " disabled=""disabled"""
            End If
            html_GetFormButton2 = s & ">"
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' main_Gets a value in a hidden form field
        '   Handles name and value encoding
        '========================================================================
        '
        Public Function html_GetFormInputHidden(ByVal TagName As String, ByVal TagValue As String, Optional ByVal htmlId As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFormInputHidden")
            '
            'If Not (true) Then Exit Function
            '
            Dim iTagValue As String
            Dim ihtmlId As String
            Dim s As String
            '
            s = cr & "<input type=""hidden"" NAME=""" & encodeHTML(genericController.encodeText(TagName)) & """"
            '
            iTagValue = encodeHTML(genericController.encodeText(TagValue))
            If iTagValue <> "" Then
                s = s & " VALUE=""" & iTagValue & """"
            End If
            '
            ihtmlId = genericController.encodeText(htmlId)
            If ihtmlId <> "" Then
                s = s & " ID=""" & encodeHTML(ihtmlId) & """"
            End If
            '
            s = s & ">"
            '
            html_GetFormInputHidden = s
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetFormInputHidden")
        End Function
        '
        Public Function html_GetFormInputHidden(ByVal TagName As String, ByVal TagValue As Boolean, Optional ByVal htmlId As String = "") As String
            Return html_GetFormInputHidden(TagName, TagValue.ToString, htmlId)
        End Function
        '
        Public Function html_GetFormInputHidden(ByVal TagName As String, ByVal TagValue As Integer, Optional ByVal htmlId As String = "") As String
            Return html_GetFormInputHidden(TagName, TagValue.ToString, htmlId)
        End Function
        '
        ' Popup a separate window with the contents of a file
        '
        Public Function html_GetWindowOpenJScript(ByVal URI As String, Optional ByVal WindowWidth As String = "", Optional ByVal WindowHeight As String = "", Optional ByVal WindowScrollBars As String = "", Optional ByVal WindowResizable As Boolean = True, Optional ByVal WindowName As String = "_blank") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetWindowOpenJScript")
            '
            'If Not (true) Then Exit Function
            '
            Dim Delimiter As String
            Dim MethodName As String
            '
            html_GetWindowOpenJScript = ""
            WindowName = genericController.encodeEmptyText(WindowName, "_blank")
            '
            MethodName = "main_GetWindowOpenJScript()"
            '
            ' Added addl options from huhcorp.com sample
            '
            html_GetWindowOpenJScript = html_GetWindowOpenJScript & "window.open('" & URI & "', '" & WindowName & "'"
            html_GetWindowOpenJScript = html_GetWindowOpenJScript & ",'menubar=no,toolbar=no,location=no,status=no"
            Delimiter = ","
            If Not genericController.isMissing(WindowWidth) Then
                If WindowWidth <> "" Then
                    html_GetWindowOpenJScript = html_GetWindowOpenJScript & Delimiter & "width=" & WindowWidth
                    Delimiter = ","
                End If
            End If
            If Not genericController.isMissing(WindowHeight) Then
                If WindowHeight <> "" Then
                    html_GetWindowOpenJScript = html_GetWindowOpenJScript & Delimiter & "height=" & WindowHeight
                    Delimiter = ","
                End If
            End If
            If Not genericController.isMissing(WindowScrollBars) Then
                If WindowScrollBars <> "" Then
                    html_GetWindowOpenJScript = html_GetWindowOpenJScript & Delimiter & "scrollbars=" & WindowScrollBars
                    Delimiter = ","
                End If
            End If
            If WindowResizable Then
                html_GetWindowOpenJScript = html_GetWindowOpenJScript & Delimiter & "resizable"
                Delimiter = ","
            End If
            html_GetWindowOpenJScript = html_GetWindowOpenJScript & "')"
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18(MethodName)
            '
        End Function
        '
        ' Popup a separate window with the contents of a file
        '
        Public Function html_GetWindowDialogJScript(ByVal URI As String, Optional ByVal WindowWidth As String = "", Optional ByVal WindowHeight As String = "", Optional ByVal WindowScrollBars As Boolean = False, Optional ByVal WindowResizable As Boolean = False, Optional ByVal WindowName As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetWindowDialogJScript")
            '
            'If Not (true) Then Exit Function
            '
            Dim Delimiter As String
            Dim iWindowName As String
            Dim MethodName As String
            '
            iWindowName = genericController.encodeEmptyText(WindowName, "_blank")
            '
            MethodName = "main_GetWindowDialogJScript()"
            '
            ' Added addl options from huhcorp.com sample
            '
            html_GetWindowDialogJScript = ""
            html_GetWindowDialogJScript = html_GetWindowDialogJScript & "showModalDialog('" & URI & "', '" & iWindowName & "'"
            html_GetWindowDialogJScript = html_GetWindowDialogJScript & ",'status:false"
            If Not genericController.isMissing(WindowWidth) Then
                If WindowWidth <> "" Then
                    html_GetWindowDialogJScript = html_GetWindowDialogJScript & ";dialogWidth:" & WindowWidth & "px"
                End If
            End If
            If Not genericController.isMissing(WindowHeight) Then
                If WindowHeight <> "" Then
                    html_GetWindowDialogJScript = html_GetWindowDialogJScript & ";dialogHeight:" & WindowHeight & "px"
                End If
            End If
            If WindowScrollBars Then
                html_GetWindowDialogJScript = html_GetWindowDialogJScript & ";scroll:yes"
            End If
            If WindowResizable Then
                html_GetWindowDialogJScript = html_GetWindowDialogJScript & ";resizable:yes"
            End If
            html_GetWindowDialogJScript = html_GetWindowDialogJScript & "')"
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18(MethodName)
            '
        End Function
        '
        '=========================================================================================
        '
        '=========================================================================================
        '
        Public Sub html_AddEvent(ByVal HtmlId As String, ByVal DOMEvent As String, ByVal Javascript As String)
            Dim JSCodeAsString As String = Javascript
            JSCodeAsString = genericController.vbReplace(JSCodeAsString, "'", "'+""'""+'")
            JSCodeAsString = genericController.vbReplace(JSCodeAsString, vbCrLf, "\n")
            JSCodeAsString = genericController.vbReplace(JSCodeAsString, vbCr, "\n")
            JSCodeAsString = genericController.vbReplace(JSCodeAsString, vbLf, "\n")
            JSCodeAsString = "'" & JSCodeAsString & "'"
            Call addScriptCode_onLoad("" _
                & "cj.addListener(" _
                    & "document.getElementById('" & HtmlId & "')" _
                    & ",'" & DOMEvent & "'" _
                    & ",function(){eval(" & JSCodeAsString & ")}" _
                & ")", "")
        End Sub
        '
        '
        '
        Public Function html_GetFormInputField(ByVal ContentName As String, ByVal FieldName As String, Optional ByVal htmlName As String = "", Optional ByVal HtmlValue As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "", Optional ByVal HtmlStyle As String = "", Optional ByVal ManyToManySourceRecordID As Integer = 0) As String
            Dim result As String = String.Empty
            Try
                Dim IgnoreBoolean As Boolean
                Dim LookupContentName As String
                Dim fieldType As Integer
                Dim InputName As String
                Dim GroupID As Integer
                Dim CDef As Models.Complex.cdefModel
                Dim MTMContent0 As String
                Dim MTMContent1 As String
                Dim MTMRuleContent As String
                Dim MTMRuleField0 As String
                Dim MTMRuleField1 As String
                '
                InputName = htmlName
                If InputName = "" Then
                    InputName = FieldName
                End If
                '
                fieldType = genericController.EncodeInteger(Models.Complex.cdefModel.GetContentFieldProperty(cpCore, ContentName, FieldName, "type"))
                Select Case fieldType
                    Case FieldTypeIdBoolean
                        '
                        '
                        '
                        result = html_GetFormInputCheckBox2(InputName, genericController.EncodeBoolean(HtmlValue) = True, HtmlId, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdFileCSS
                        '
                        '
                        '
                        result = html_GetFormInputTextExpandable2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdCurrency
                        '
                        '
                        '
                        result = html_GetFormInputText2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdDate
                        '
                        '
                        '
                        result = html_GetFormInputDate(InputName, HtmlValue, , HtmlId)
                        If HtmlClass <> "" Then
                            result = genericController.vbReplace(result, ">", " class=""" & HtmlClass & """>")
                        End If
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdFile
                        '
                        '
                        '
                        If HtmlValue = "" Then
                            result = html_GetFormInputFile2(InputName, HtmlId, HtmlClass)
                        Else

                            Dim FieldValuefilename As String = ""
                            Dim FieldValuePath As String = ""
                            cpCore.cdnFiles.splitPathFilename(HtmlValue, FieldValuePath, FieldValuefilename)
                            result = result & "<a href=""http://" & genericController.EncodeURL(cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, HtmlValue)) & """ target=""_blank"">" & SpanClassAdminSmall & "[" & FieldValuefilename & "]</A>"
                            result = result & "&nbsp;&nbsp;&nbsp;Delete:&nbsp;" & html_GetFormInputCheckBox2(InputName & ".Delete", False)
                            result = result & "&nbsp;&nbsp;&nbsp;Change:&nbsp;" & html_GetFormInputFile2(InputName, HtmlId, HtmlClass)
                        End If
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdFloat
                        '
                        '
                        '
                        result = html_GetFormInputText2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdFileImage
                        '
                        '
                        '
                        If HtmlValue = "" Then
                            result = html_GetFormInputFile2(InputName, HtmlId, HtmlClass)
                        Else
                            Dim FieldValuefilename As String = ""
                            Dim FieldValuePath As String = ""
                            cpCore.cdnFiles.splitPathFilename(HtmlValue, FieldValuePath, FieldValuefilename)
                            result = result & "<a href=""http://" & genericController.EncodeURL(cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, HtmlValue)) & """ target=""_blank"">" & SpanClassAdminSmall & "[" & FieldValuefilename & "]</A>"
                            result = result & "&nbsp;&nbsp;&nbsp;Delete:&nbsp;" & html_GetFormInputCheckBox2(InputName & ".Delete", False)
                            result = result & "&nbsp;&nbsp;&nbsp;Change:&nbsp;" & html_GetFormInputFile2(InputName, HtmlId, HtmlClass)
                        End If
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdInteger
                        '
                        '
                        '
                        result = html_GetFormInputText2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdFileJavascript
                        '
                        '
                        '
                        result = html_GetFormInputTextExpandable2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdLink
                        '
                        '
                        '
                        result = html_GetFormInputText2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdLookup
                        '
                        '
                        '
                        CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName)
                        LookupContentName = ""
                        With CDef
                            For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In CDef.fields
                                Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                                With field
                                    If genericController.vbUCase(.nameLc) = genericController.vbUCase(FieldName) Then
                                        If .lookupContentID <> 0 Then
                                            LookupContentName = genericController.encodeText(Models.Complex.cdefModel.getContentNameByID(cpCore, .lookupContentID))
                                        End If
                                        If LookupContentName <> "" Then
                                            result = main_GetFormInputSelect2(InputName, genericController.EncodeInteger(HtmlValue), LookupContentName, "", "Select One", HtmlId, IgnoreBoolean, HtmlClass)
                                        ElseIf .lookupList <> "" Then
                                            result = getInputSelectList2(InputName, genericController.EncodeInteger(HtmlValue), .lookupList, "Select One", HtmlId, HtmlClass)
                                        End If
                                        If HtmlStyle <> "" Then
                                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                                        End If
                                        Exit For
                                    End If
                                End With
                            Next
                        End With
                    Case FieldTypeIdManyToMany
                        '
                        '
                        '
                        CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName)
                        With CDef.fields(FieldName.ToLower())
                            MTMContent0 = Models.Complex.cdefModel.getContentNameByID(cpCore, .contentId)
                            MTMContent1 = Models.Complex.cdefModel.getContentNameByID(cpCore, .manyToManyContentID)
                            MTMRuleContent = Models.Complex.cdefModel.getContentNameByID(cpCore, .manyToManyRuleContentID)
                            MTMRuleField0 = .ManyToManyRulePrimaryField
                            MTMRuleField1 = .ManyToManyRuleSecondaryField
                        End With
                        result = getCheckList(InputName, MTMContent0, ManyToManySourceRecordID, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, , , False)
                        'result = getInputCheckListCategories(InputName, MTMContent0, ManyToManySourceRecordID, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, , , False, MTMContent1, HtmlValue)
                    Case FieldTypeIdMemberSelect
                        '
                        '
                        '
                        GroupID = genericController.EncodeInteger(Models.Complex.cdefModel.GetContentFieldProperty(cpCore, ContentName, FieldName, "memberselectgroupid"))
                        result = getInputMemberSelect(InputName, genericController.EncodeInteger(HtmlValue), GroupID, , , HtmlId)
                        If HtmlClass <> "" Then
                            result = genericController.vbReplace(result, ">", " class=""" & HtmlClass & """>")
                        End If
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdResourceLink
                        '
                        '
                        '
                        result = html_GetFormInputText2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdText
                        '
                        '
                        '
                        result = html_GetFormInputText2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdLongText, FieldTypeIdFileText
                        '
                        '
                        '
                        result = html_GetFormInputTextExpandable2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdFileXML
                        '
                        '
                        '
                        result = html_GetFormInputTextExpandable2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdHTML, FieldTypeIdFileHTML
                        '
                        '
                        '
                        result = getFormInputHTML(InputName, HtmlValue)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                        If HtmlClass <> "" Then
                            result = genericController.vbReplace(result, ">", " class=""" & HtmlClass & """>")
                        End If
                    Case Else
                        '
                        ' unsupported field type
                        '
                End Select
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        ''
        ''   renamed to AllowDebugging
        ''
        'Public ReadOnly Property visitProperty_AllowVerboseReporting() As Boolean
        '    Get
        '        Return visitProperty.getBoolean("AllowDebugging")
        '    End Get
        'End Property
        '        '
        '        '
        '        '
        '        Public Function main_parseJSON(ByVal Source As String) As Object
        '            On Error GoTo ErrorTrap 'Const Tn = "parseJSON" : ''Dim th as integer : th = profileLogMethodEnter(Tn)    '
        '            '
        '            main_parseJSON = common_jsonDeserialize(Source)
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            cpCore.handleExceptionAndContinue(New Exception("Unexpected exception"))
        '            '
        '        End Function
        ''
        ''
        ''
        'Public Function main_GetStyleSheet2(ByVal ContentType As csv_contentTypeEnum, Optional ByVal templateId As Integer = 0, Optional ByVal EmailID As Integer = 0) As String
        '    main_GetStyleSheet2 = html_getStyleSheet2(ContentType, templateId, EmailID)
        'End Function
        '
	
	
    End Class
End Namespace
