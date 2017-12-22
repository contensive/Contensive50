
Option Explicit On
Option Strict On
'
Imports Contensive.BaseClasses
Imports Contensive.Core.Models.Entity
'
Namespace Contensive.Core.Controllers
    '
    '====================================================================================================
    ''' <summary>
    ''' build page content system. Persistence is the docController.
    ''' </summary>
    Public Class pageContentController
        Implements IDisposable
        '
        '
        '
        '
        Friend Shared Function getFormPage(cpcore As coreClass, FormPageName As String, GroupIDToJoinOnSuccess As Integer) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("main_GetFormPage")
            '
            Dim RepeatBody As String
            Dim PtrFront As Integer
            Dim PtrBack As Integer
            Dim i() As String
            Dim IPtr As Integer
            Dim IStart As Integer
            Dim IArgs() As String
            Dim IArgPtr As Integer
            Dim CSPeople As Integer
            Dim Body As String
            Dim Instruction As String
            Dim Formhtml As String = String.Empty
            Dim FormInstructions As String = String.Empty
            Dim CS As Integer
            Dim HasRequiredFields As Boolean
            Dim ArgCaption As String
            Dim ArgType As Integer
            Dim ArgRequired As Boolean
            Dim GroupName As String
            Dim GroupValue As Boolean
            Dim GroupRowPtr As Integer
            Dim FormPageID As Integer
            Dim f As main_FormPagetype
            Dim IsRetry As Boolean
            Dim CaptionSpan As String
            Dim Caption As String
            Dim IsRequiredByCDef As Boolean
            Dim PeopleCDef As Models.Complex.cdefModel
            '
            IsRetry = (cpcore.docProperties.getInteger("ContensiveFormPageID") <> 0)
            '
            CS = cpcore.db.csOpen("Form Pages", "name=" & cpcore.db.encodeSQLText(FormPageName))
            If cpcore.db.csOk(CS) Then
                FormPageID = cpcore.db.csGetInteger(CS, "ID")
                Formhtml = cpcore.db.csGetText(CS, "Body")
                FormInstructions = cpcore.db.csGetText(CS, "Instructions")
            End If
            Call cpcore.db.csClose(CS)
            f = loadFormPageInstructions(cpcore, FormInstructions, Formhtml)
            '
            '
            '
            RepeatBody = ""
            CSPeople = -1
            For IPtr = 0 To UBound(f.Inst)
                With f.Inst(IPtr)
                    Select Case .Type
                        Case 1
                            '
                            ' People Record
                            '
                            If IsRetry And cpcore.docProperties.getText(.PeopleField) = "" Then
                                CaptionSpan = "<span class=""ccError"">"
                            Else
                                CaptionSpan = "<span>"
                            End If
                            If Not cpcore.db.csOk(CSPeople) Then
                                CSPeople = cpcore.db.csOpenRecord("people", cpcore.doc.authContext.user.id)
                            End If
                            Caption = .Caption
                            If .REquired Or genericController.EncodeBoolean(Models.Complex.cdefModel.GetContentFieldProperty(cpcore, "People", .PeopleField, "Required")) Then
                                Caption = "*" & Caption
                            End If
                            If cpcore.db.csOk(CSPeople) Then
                                Body = f.RepeatCell
                                Body = genericController.vbReplace(Body, "{{CAPTION}}", CaptionSpan & Caption & "</span>", 1, 99, vbTextCompare)
                                Body = genericController.vbReplace(Body, "{{FIELD}}", cpcore.html.html_GetFormInputCS(CSPeople, "People", .PeopleField), 1, 99, vbTextCompare)
                                RepeatBody = RepeatBody & Body
                                HasRequiredFields = HasRequiredFields Or .REquired
                            End If
                        Case 2
                            '
                            ' Group main_MemberShip
                            '
                            GroupValue = cpcore.doc.authContext.IsMemberOfGroup2(cpcore, .GroupName)
                            Body = f.RepeatCell
                            Body = genericController.vbReplace(Body, "{{CAPTION}}", cpcore.html.html_GetFormInputCheckBox2("Group" & .GroupName, GroupValue), 1, 99, vbTextCompare)
                            Body = genericController.vbReplace(Body, "{{FIELD}}", .Caption)
                            RepeatBody = RepeatBody & Body
                            GroupRowPtr = GroupRowPtr + 1
                            HasRequiredFields = HasRequiredFields Or .REquired
                    End Select
                End With
            Next
            Call cpcore.db.csClose(CSPeople)
            If HasRequiredFields Then
                Body = f.RepeatCell
                Body = genericController.vbReplace(Body, "{{CAPTION}}", "&nbsp;", 1, 99, vbTextCompare)
                Body = genericController.vbReplace(Body, "{{FIELD}}", "*&nbsp;Required Fields")
                RepeatBody = RepeatBody & Body
            End If
            '
            getFormPage = "" _
            & errorController.error_GetUserError(cpcore) _
            & cpcore.html.html_GetUploadFormStart() _
            & cpcore.html.html_GetFormInputHidden("ContensiveFormPageID", FormPageID) _
            & cpcore.html.html_GetFormInputHidden("SuccessID", cpcore.security.encodeToken(GroupIDToJoinOnSuccess, cpcore.doc.profileStartTime)) _
            & f.PreRepeat _
            & RepeatBody _
            & f.PostRepeat _
            & cpcore.html.html_GetUploadFormEnd()
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_GetFormPage")
        End Function
        '
        '=============================================================================
        '   getContentBox
        '
        '   PageID is the page to display. If it is 0, the root page is displayed
        '   RootPageID has to be the ID of the root page for PageID
        '=============================================================================
        '
        Public Shared Function getContentBox(cpCore As coreClass, OrderByClause As String, AllowChildPageList As Boolean, AllowReturnLink As Boolean, ArchivePages As Boolean, ignoreme As Integer, UseContentWatchLink As Boolean, allowPageWithoutSectionDisplay As Boolean) As String
            Dim returnHtml As String = ""
            Try
                Dim DateModified As Date
                Dim PageRecordID As Integer
                Dim PageName As String
                Dim CS As Integer
                Dim SQL As String
                Dim ContentBlocked As Boolean
                Dim NewPageCreated As Boolean
                Dim SystemEMailID As Integer
                Dim ConditionID As Integer
                Dim ConditionGroupID As Integer
                Dim main_AddGroupID As Integer
                Dim RemoveGroupID As Integer
                Dim RegistrationGroupID As Integer
                Dim BlockedPages() As String
                Dim BlockedPageRecordID As Integer
                Dim BlockCopy As String
                Dim pageViewings As Integer
                Dim layoutError As String = ""
                '
                Call cpCore.html.addHeadTag("<meta name=""contentId"" content=""" & cpCore.doc.page.id & """ >", "page content")
                '
                returnHtml = getContentBox_content(cpCore, OrderByClause, AllowChildPageList, AllowReturnLink, ArchivePages, ignoreme, UseContentWatchLink, allowPageWithoutSectionDisplay)
                '
                ' ----- If Link field populated, do redirect
                If (cpCore.doc.page.PageLink <> "") Then
                    cpCore.doc.page.Clicks += 1
                    cpCore.doc.page.save(cpCore)
                    cpCore.doc.redirectLink = cpCore.doc.page.PageLink
                    cpCore.doc.redirectReason = "Redirect required because this page (PageRecordID=" & cpCore.doc.page.id & ") has a Link Override [" & cpCore.doc.page.PageLink & "]."
                    Return ""
                End If
                '
                ' -- build list of blocked pages
                Dim BlockedRecordIDList As String = ""
                If (returnHtml <> "") And (cpCore.doc.redirectLink = "") Then
                    NewPageCreated = True
                    For Each testPage As pageContentModel In cpCore.doc.pageToRootList
                        If testPage.BlockContent Or testPage.BlockPage Then
                            BlockedRecordIDList = BlockedRecordIDList & "," & testPage.id
                        End If
                    Next
                    If BlockedRecordIDList <> "" Then
                        BlockedRecordIDList = Mid(BlockedRecordIDList, 2)
                    End If
                End If
                '
                ' ----- Content Blocking
                If (BlockedRecordIDList <> "") Then
                    If cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                        '
                        ' Administrators are never blocked
                        '
                    ElseIf (Not cpCore.doc.authContext.isAuthenticated()) Then
                        '
                        ' non-authenticated are always blocked
                        '
                        ContentBlocked = True
                    Else
                        '
                        ' Check Access Groups, if in access groups, remove group from BlockedRecordIDList
                        '
                        SQL = "SELECT DISTINCT ccPageContentBlockRules.RecordID" _
                            & " FROM (ccPageContentBlockRules" _
                            & " LEFT JOIN ccgroups ON ccPageContentBlockRules.GroupID = ccgroups.ID)" _
                            & " LEFT JOIN ccMemberRules ON ccgroups.ID = ccMemberRules.GroupID" _
                            & " WHERE (((ccMemberRules.MemberID)=" & cpCore.db.encodeSQLNumber(cpCore.doc.authContext.user.id) & ")" _
                            & " AND ((ccPageContentBlockRules.RecordID) In (" & BlockedRecordIDList & "))" _
                            & " AND ((ccPageContentBlockRules.Active)<>0)" _
                            & " AND ((ccgroups.Active)<>0)" _
                            & " AND ((ccMemberRules.Active)<>0)" _
                            & " AND ((ccMemberRules.DateExpires) Is Null Or (ccMemberRules.DateExpires)>" & cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime) & "));"
                        CS = cpCore.db.csOpenSql(SQL)
                        BlockedRecordIDList = "," & BlockedRecordIDList
                        Do While cpCore.db.csOk(CS)
                            BlockedRecordIDList = genericController.vbReplace(BlockedRecordIDList, "," & cpCore.db.csGetText(CS, "RecordID"), "")
                            cpCore.db.csGoNext(CS)
                        Loop
                        Call cpCore.db.csClose(CS)
                        If BlockedRecordIDList <> "" Then
                            '
                            ' ##### remove the leading comma
                            BlockedRecordIDList = Mid(BlockedRecordIDList, 2)
                            ' Check the remaining blocked records against the members Content Management
                            ' ##### removed hardcoded mistakes from the sql
                            SQL = "SELECT DISTINCT ccPageContent.ID as RecordID" _
                                & " FROM ((ccPageContent" _
                                & " LEFT JOIN ccGroupRules ON ccPageContent.ContentControlID = ccGroupRules.ContentID)" _
                                & " LEFT JOIN ccgroups AS ManagementGroups ON ccGroupRules.GroupID = ManagementGroups.ID)" _
                                & " LEFT JOIN ccMemberRules AS ManagementMemberRules ON ManagementGroups.ID = ManagementMemberRules.GroupID" _
                                & " WHERE (((ccPageContent.ID) In (" & BlockedRecordIDList & "))" _
                                & " AND ((ccGroupRules.Active)<>0)" _
                                & " AND ((ManagementGroups.Active)<>0)" _
                                & " AND ((ManagementMemberRules.Active)<>0)" _
                                & " AND ((ManagementMemberRules.DateExpires) Is Null Or (ManagementMemberRules.DateExpires)>" & cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime) & ")" _
                                & " AND ((ManagementMemberRules.MemberID)=" & cpCore.doc.authContext.user.id & " ));"
                            CS = cpCore.db.csOpenSql(SQL)
                            Do While cpCore.db.csOk(CS)
                                BlockedRecordIDList = genericController.vbReplace(BlockedRecordIDList, "," & cpCore.db.csGetText(CS, "RecordID"), "")
                                cpCore.db.csGoNext(CS)
                            Loop
                            Call cpCore.db.csClose(CS)
                        End If
                        If BlockedRecordIDList <> "" Then
                            ContentBlocked = True
                        End If
                        Call cpCore.db.csClose(CS)
                    End If
                End If
                '
                '
                '
                If ContentBlocked Then
                    Dim CustomBlockMessageFilename As String = ""
                    Dim BlockSourceID As Integer = main_BlockSourceDefaultMessage
                    Dim ContentPadding As Integer = 20
                    BlockedPages = Split(BlockedRecordIDList, ",")
                    BlockedPageRecordID = genericController.EncodeInteger(BlockedPages(UBound(BlockedPages)))
                    If BlockedPageRecordID <> 0 Then
                        CS = cpCore.db.csOpenRecord("Page Content", BlockedPageRecordID, , , "CustomBlockMessage,BlockSourceID,RegistrationGroupID,ContentPadding")
                        If cpCore.db.csOk(CS) Then
                            BlockSourceID = cpCore.db.csGetInteger(CS, "BlockSourceID")
                            ContentPadding = cpCore.db.csGetInteger(CS, "ContentPadding")
                            CustomBlockMessageFilename = cpCore.db.csGetText(CS, "CustomBlockMessage")
                            RegistrationGroupID = cpCore.db.csGetInteger(CS, "RegistrationGroupID")
                        End If
                        Call cpCore.db.csClose(CS)
                    End If
                    '
                    ' Block Appropriately
                    '
                    Select Case BlockSourceID
                        Case main_BlockSourceCustomMessage
                            '
                            ' ----- Custom Message
                            '
                            returnHtml = cpCore.cdnFiles.readFile(CustomBlockMessageFilename)
                        Case main_BlockSourceLogin
                            '
                            ' ----- Login page
                            '
                            Dim BlockForm As String = ""
                            If Not cpCore.doc.authContext.isAuthenticated() Then
                                If Not cpCore.doc.authContext.isRecognized(cpCore) Then
                                    '
                                    ' -- not recognized
                                    BlockForm = "" _
                                        & "<p>This content has limited access. If you have an account, please login using this form.</p>" _
                                        & cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginForm), New CPUtilsBaseClass.addonExecuteContext With {.addonType = CPUtilsBaseClass.addonContext.ContextPage}) _
                                        & ""
                                Else
                                    '
                                    ' -- recognized, not authenticated
                                    BlockForm = "" _
                                        & "<p>This content has limited access. You were recognized as ""<b>" & cpCore.doc.authContext.user.name & "</b>"", but you need to login to continue. To login to this account or another, please use this form.</p>" _
                                        & cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginForm), New CPUtilsBaseClass.addonExecuteContext With {.addonType = CPUtilsBaseClass.addonContext.ContextPage}) _
                                        & ""
                                End If
                            Else
                                '
                                ' -- authenticated
                                BlockForm = "" _
                                    & "<p>You are currently logged in as ""<b>" & cpCore.doc.authContext.user.name & "</b>"". If this is not you, please <a href=""?" & cpCore.doc.refreshQueryString & "&method=logout"" rel=""nofollow"">Click Here</a>.</p>" _
                                    & "<p>This account does not have access to this content. If you want to login with a different account, please use this form.</p>" _
                                    & cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginForm), New CPUtilsBaseClass.addonExecuteContext With {.addonType = CPUtilsBaseClass.addonContext.ContextPage}) _
                                    & ""
                            End If
                            returnHtml = "" _
                                & "<div style=""margin: 100px, auto, auto, auto;text-align:left;"">" _
                                & errorController.error_GetUserError(cpCore) _
                                & BlockForm _
                                & "</div>"
                        Case main_BlockSourceRegistration
                            '
                            ' ----- Registration
                            '
                            Dim BlockForm As String = ""
                            If cpCore.docProperties.getInteger("subform") = main_BlockSourceLogin Then
                                '
                                ' login subform form
                                BlockForm = "" _
                                    & "<p>This content has limited access. If you have an account, please login using this form.</p>" _
                                    & "<p>If you do not have an account, <a href=?" & cpCore.doc.refreshQueryString & "&subform=0>click here to register</a>.</p>" _
                                    & cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginForm), New CPUtilsBaseClass.addonExecuteContext With {.addonType = CPUtilsBaseClass.addonContext.ContextPage}) _
                                    & ""
                            Else
                                '
                                ' Register Form
                                '
                                If Not cpCore.doc.authContext.isAuthenticated() And cpCore.doc.authContext.isRecognized(cpCore) Then
                                    '
                                    ' -- Can not take the chance, if you go to a registration page, and you are recognized but not auth -- logout first
                                    Call cpCore.doc.authContext.logout(cpCore)
                                End If
                                If Not cpCore.doc.authContext.isAuthenticated() Then
                                    '
                                    ' -- Not Authenticated
                                    Call cpCore.doc.verifyRegistrationFormPage(cpCore)
                                    BlockForm = "" _
                                        & "<p>This content has limited access. If you have an account, <a href=?" & cpCore.doc.refreshQueryString & "&subform=" & main_BlockSourceLogin & ">Click Here to login</a>.</p>" _
                                        & "<p>To view this content, please complete this form.</p>" _
                                        & getFormPage(cpCore, "Registration Form", RegistrationGroupID) _
                                        & ""
                                Else
                                    '
                                    ' -- Authenticated
                                    Call cpCore.doc.verifyRegistrationFormPage(cpCore)
                                    BlockCopy = "" _
                                        & "<p>You are currently logged in as ""<b>" & cpCore.doc.authContext.user.name & "</b>"". If this is not you, please <a href=""?" & cpCore.doc.refreshQueryString & "&method=logout"" rel=""nofollow"">Click Here</a>.</p>" _
                                        & "<p>This account does not have access to this content. To view this content, please complete this form.</p>" _
                                        & getFormPage(cpCore, "Registration Form", RegistrationGroupID) _
                                        & ""
                                End If
                            End If
                            returnHtml = "" _
                                & "<div style=""margin: 100px, auto, auto, auto;text-align:left;"">" _
                                & errorController.error_GetUserError(cpCore) _
                                & BlockForm _
                                & "</div>"
                        Case Else
                            '
                            ' ----- Content as blocked - convert from site property to content page
                            '
                            returnHtml = getDefaultBlockMessage(cpCore, UseContentWatchLink)
                    End Select
                    '
                    ' If the output is blank, put default message in
                    '
                    If returnHtml = "" Then
                        returnHtml = getDefaultBlockMessage(cpCore, UseContentWatchLink)
                    End If
                    '
                    ' Encode the copy
                    '
                    returnHtml = cpCore.html.executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpCore.doc.authContext.user.id, cpCore.doc.authContext.isAuthenticated, layoutError)
                    returnHtml = cpCore.html.convertActiveContentToHtmlForWebRender(returnHtml, pageContentModel.contentName, PageRecordID, cpCore.doc.page.ContactMemberID, "http://" & cpCore.webServer.requestDomain, cpCore.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage)
                    If cpCore.doc.refreshQueryString <> "" Then
                        returnHtml = genericController.vbReplace(returnHtml, "?method=login", "?method=Login&" & cpCore.doc.refreshQueryString, 1, 99, vbTextCompare)
                    End If
                    '
                    ' Add in content padding required for integration with the template
                    returnHtml = getContentBoxWrapper(cpCore, returnHtml, ContentPadding)
                End If
                '
                ' ----- Encoding, Tracking and Triggers
                If Not ContentBlocked Then
                    If cpCore.visitProperty.getBoolean("AllowQuickEditor") Then
                        '
                        ' Quick Editor, no encoding or tracking
                        '
                    Else
                        pageViewings = cpCore.doc.page.Viewings
                        If cpCore.doc.authContext.isEditing(pageContentModel.contentName) Or cpCore.visitProperty.getBoolean("AllowWorkflowRendering") Then
                            '
                            ' Link authoring, workflow rendering -> do encoding, but no tracking
                            '
                            returnHtml = cpCore.html.executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpCore.doc.authContext.user.id, cpCore.doc.authContext.isAuthenticated, layoutError)
                            returnHtml = cpCore.html.convertActiveContentToHtmlForWebRender(returnHtml, pageContentModel.contentName, PageRecordID, cpCore.doc.page.ContactMemberID, "http://" & cpCore.webServer.requestDomain, cpCore.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage)
                        Else
                            '
                            ' Live content
                            returnHtml = cpCore.html.executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpCore.doc.authContext.user.id, cpCore.doc.authContext.isAuthenticated, layoutError)
                            returnHtml = cpCore.html.convertActiveContentToHtmlForWebRender(returnHtml, pageContentModel.contentName, PageRecordID, cpCore.doc.page.ContactMemberID, "http://" & cpCore.webServer.requestDomain, cpCore.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage)
                            Call cpCore.db.executeQuery("update ccpagecontent set viewings=" & (pageViewings + 1) & " where id=" & cpCore.doc.page.id)
                        End If
                        '
                        ' Page Hit Notification
                        '
                        If (Not cpCore.doc.authContext.visit.ExcludeFromAnalytics) And (cpCore.doc.page.ContactMemberID <> 0) And (InStr(1, cpCore.webServer.requestBrowser, "kmahttp", vbTextCompare) = 0) Then
                            If cpCore.doc.page.AllowHitNotification Then
                                PageName = cpCore.doc.page.name
                                If PageName = "" Then
                                    PageName = cpCore.doc.page.MenuHeadline
                                    If PageName = "" Then
                                        PageName = cpCore.doc.page.Headline
                                        If PageName = "" Then
                                            PageName = "[no name]"
                                        End If
                                    End If
                                End If
                                Dim Body As String = ""
                                Body = Body & "<p><b>Page Hit Notification.</b></p>"
                                Body = Body & "<p>This email was sent to you by the Contensive Server as a notification of the following content viewing details.</p>"
                                Body = Body & genericController.StartTable(4, 1, 1)
                                Body = Body & "<tr><td align=""right"" width=""150"" Class=""ccPanelHeader"">Description<br><img alt=""image"" src=""http://" & cpCore.webServer.requestDomain & "/ccLib/images/spacer.gif"" width=""150"" height=""1""></td><td align=""left"" width=""100%"" Class=""ccPanelHeader"">Value</td></tr>"
                                Body = Body & getTableRow("Domain", cpCore.webServer.requestDomain, True)
                                Body = Body & getTableRow("Link", cpCore.webServer.requestUrl, False)
                                Body = Body & getTableRow("Page Name", PageName, True)
                                Body = Body & getTableRow("Member Name", cpCore.doc.authContext.user.name, False)
                                Body = Body & getTableRow("Member #", CStr(cpCore.doc.authContext.user.id), True)
                                Body = Body & getTableRow("Visit Start Time", CStr(cpCore.doc.authContext.visit.StartTime), False)
                                Body = Body & getTableRow("Visit #", CStr(cpCore.doc.authContext.visit.id), True)
                                Body = Body & getTableRow("Visit IP", cpCore.webServer.requestRemoteIP, False)
                                Body = Body & getTableRow("Browser ", cpCore.webServer.requestBrowser, True)
                                Body = Body & getTableRow("Visitor #", CStr(cpCore.doc.authContext.visitor.ID), False)
                                Body = Body & getTableRow("Visit Authenticated", CStr(cpCore.doc.authContext.visit.VisitAuthenticated), True)
                                Body = Body & getTableRow("Visit Referrer", cpCore.doc.authContext.visit.HTTP_REFERER, False)
                                Body = Body & kmaEndTable
                                Call cpCore.email.sendPerson(cpCore.doc.page.ContactMemberID, cpCore.siteProperties.getText("EmailFromAddress", "info@" & cpCore.webServer.requestDomain), "Page Hit Notification", Body, False, True, 0, "", False)
                            End If
                        End If
                        '
                        ' -- Process Trigger Conditions
                        ConditionID = cpCore.doc.page.TriggerConditionID
                        ConditionGroupID = cpCore.doc.page.TriggerConditionGroupID
                        main_AddGroupID = cpCore.doc.page.TriggerAddGroupID
                        RemoveGroupID = cpCore.doc.page.TriggerRemoveGroupID
                        SystemEMailID = cpCore.doc.page.TriggerSendSystemEmailID
                        Select Case ConditionID
                            Case 1
                                '
                                ' Always
                                '
                                If SystemEMailID <> 0 Then
                                    Call cpCore.email.sendSystem_Legacy(cpCore.db.getRecordName("System Email", SystemEMailID), "", cpCore.doc.authContext.user.id)
                                End If
                                If main_AddGroupID <> 0 Then
                                    Call groupController.group_AddGroupMember(cpCore, groupController.group_GetGroupName(cpCore, main_AddGroupID))
                                End If
                                If RemoveGroupID <> 0 Then
                                    Call groupController.group_DeleteGroupMember(cpCore, groupController.group_GetGroupName(cpCore, RemoveGroupID))
                                End If
                            Case 2
                                '
                                ' If in Condition Group
                                '
                                If ConditionGroupID <> 0 Then
                                    If cpCore.doc.authContext.IsMemberOfGroup2(cpCore, groupController.group_GetGroupName(cpCore, ConditionGroupID)) Then
                                        If SystemEMailID <> 0 Then
                                            Call cpCore.email.sendSystem_Legacy(cpCore.db.getRecordName("System Email", SystemEMailID), "", cpCore.doc.authContext.user.id)
                                        End If
                                        If main_AddGroupID <> 0 Then
                                            Call groupController.group_AddGroupMember(cpCore, groupController.group_GetGroupName(cpCore, main_AddGroupID))
                                        End If
                                        If RemoveGroupID <> 0 Then
                                            Call groupController.group_DeleteGroupMember(cpCore, groupController.group_GetGroupName(cpCore, RemoveGroupID))
                                        End If
                                    End If
                                End If
                            Case 3
                                '
                                ' If not in Condition Group
                                '
                                If ConditionGroupID <> 0 Then
                                    If Not cpCore.doc.authContext.IsMemberOfGroup2(cpCore, groupController.group_GetGroupName(cpCore, ConditionGroupID)) Then
                                        If main_AddGroupID <> 0 Then
                                            Call groupController.group_AddGroupMember(cpCore, groupController.group_GetGroupName(cpCore, main_AddGroupID))
                                        End If
                                        If RemoveGroupID <> 0 Then
                                            Call groupController.group_DeleteGroupMember(cpCore, groupController.group_GetGroupName(cpCore, RemoveGroupID))
                                        End If
                                        If SystemEMailID <> 0 Then
                                            Call cpCore.email.sendSystem_Legacy(cpCore.db.getRecordName("System Email", SystemEMailID), "", cpCore.doc.authContext.user.id)
                                        End If
                                    End If
                                End If
                        End Select
                        'End If
                        'Call app.closeCS(CS)
                    End If
                    '
                    '---------------------------------------------------------------------------------
                    ' ----- Add in ContentPadding (a table around content with the appropriate padding added)
                    '---------------------------------------------------------------------------------
                    '
                    returnHtml = getContentBoxWrapper(cpCore, returnHtml, cpCore.doc.page.ContentPadding)
                    '
                    '---------------------------------------------------------------------------------
                    ' ----- Set Headers
                    '---------------------------------------------------------------------------------
                    '
                    If DateModified <> Date.MinValue Then
                        Call cpCore.webServer.addResponseHeader("LAST-MODIFIED", genericController.GetGMTFromDate(DateModified))
                    End If
                    '
                    '---------------------------------------------------------------------------------
                    ' ----- Store page javascript
                    '---------------------------------------------------------------------------------
                    '
                    Call cpCore.html.addScriptCode_onLoad(cpCore.doc.page.JSOnLoad, "page content")
                    Call cpCore.html.addScriptCode_head(cpCore.doc.page.JSHead, "page content")
                    If cpCore.doc.page.JSFilename <> "" Then
                        Call cpCore.html.addScriptLink_Head(genericController.getCdnFileLink(cpCore, cpCore.doc.page.JSFilename), "page content")
                    End If
                    Call cpCore.html.addScriptCode_body(cpCore.doc.page.JSEndBody, "page content")
                    '
                    '---------------------------------------------------------------------------------
                    ' Set the Meta Content flag
                    '---------------------------------------------------------------------------------
                    '
                    Call cpCore.html.addTitle(genericController.encodeHTML(cpCore.doc.page.pageTitle), "page content")
                    Call cpCore.html.addMetaDescription(genericController.encodeHTML(cpCore.doc.page.metaDescription), "page content")
                    Call cpCore.html.addHeadTag(cpCore.doc.page.OtherHeadTags, "page content")
                    Call cpCore.html.addMetaKeywordList(cpCore.doc.page.MetaKeywordList, "page content")
                    '
                    Dim instanceArguments As New Dictionary(Of String, String)
                    instanceArguments.Add("CSPage", "-1")
                    Dim executeContext As New CPUtilsBaseClass.addonExecuteContext() With {.instanceGuid = "-1", .instanceArguments = instanceArguments}
                    '
                    ' -- OnPageStartEvent
                    cpCore.doc.bodyContent = returnHtml
                    executeContext.addonType = CPUtilsBaseClass.addonContext.ContextOnPageStart
                    Dim addonList As List(Of addonModel) = Models.Entity.addonModel.createList_OnPageStartEvent(cpCore, New List(Of String))
                    For Each addon As Models.Entity.addonModel In addonList
                        cpCore.doc.bodyContent = cpCore.addon.execute(addon, executeContext) & cpCore.doc.bodyContent
                        'AddonContent = cpCore.addon.execute_legacy5(addon.id, addon.name, "CSPage=-1", CPUtilsBaseClass.addonContext.ContextOnPageStart, "", 0, "", -1)
                    Next
                    returnHtml = cpCore.doc.bodyContent
                    '
                    ' -- OnPageEndEvent / filter
                    cpCore.doc.bodyContent = returnHtml
                    executeContext.addonType = CPUtilsBaseClass.addonContext.ContextOnPageEnd
                    For Each addon As addonModel In cpCore.addonCache.getOnPageEndAddonList
                        cpCore.doc.bodyContent &= cpCore.addon.execute(addon, executeContext)
                        'cpCore.doc.bodyContent &= cpCore.addon.execute_legacy5(addon.id, addon.name, "CSPage=-1", CPUtilsBaseClass.addonContext.ContextOnPageStart, "", 0, "", -1)
                    Next
                    returnHtml = cpCore.doc.bodyContent
                    '
                End If
                '
                ' -- title
                cpCore.html.addTitle(cpCore.doc.page.name)
                '
                ' -- add contentid and sectionid
                Call cpCore.html.addHeadTag("<meta name=""contentId"" content=""" & cpCore.doc.page.id & """ >", "page content")
                '
                ' Display Admin Warnings with Edits for record errors
                '
                If cpCore.doc.adminWarning <> "" Then
                    '
                    If cpCore.doc.adminWarningPageID <> 0 Then
                        cpCore.doc.adminWarning = cpCore.doc.adminWarning & "</p>" & cpCore.html.main_GetRecordEditLink2("Page Content", cpCore.doc.adminWarningPageID, True, "Page " & cpCore.doc.adminWarningPageID, cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) & "&nbsp;Edit the page<p>"
                        cpCore.doc.adminWarningPageID = 0
                    End If
                    returnHtml = "" _
                    & cpCore.html.html_GetAdminHintWrapper(cpCore.doc.adminWarning) _
                    & returnHtml _
                    & ""
                    cpCore.doc.adminWarning = ""
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnHtml
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' GetHtmlBody_GetSection_GetContentBox
        ''' </summary>
        ''' <param name="PageID"></param>
        ''' <param name="rootPageId"></param>
        ''' <param name="RootPageContentName"></param>
        ''' <param name="OrderByClause"></param>
        ''' <param name="AllowChildPageList"></param>
        ''' <param name="AllowReturnLink"></param>
        ''' <param name="ArchivePages"></param>
        ''' <param name="ignoreMe"></param>
        ''' <param name="UseContentWatchLink"></param>
        ''' <param name="allowPageWithoutSectionDisplay"></param>
        ''' <returns></returns>
        '
        Friend Shared Function getContentBox_content(cpcore As coreClass, OrderByClause As String, AllowChildPageList As Boolean, AllowReturnLink As Boolean, ArchivePages As Boolean, ignoreMe As Integer, UseContentWatchLink As Boolean, allowPageWithoutSectionDisplay As Boolean) As String
            Dim result As String = ""
            Try
                Dim isEditing As Boolean
                Dim LiveBody As String
                '
                If cpCore.doc.continueProcessing Then
                    If cpcore.doc.redirectLink = "" Then
                        isEditing = cpCore.doc.authContext.isEditing(pageContentModel.contentName)
                        '
                        ' ----- Render the Body
                        LiveBody = getContentBox_content_Body(cpcore, OrderByClause, AllowChildPageList, False, cpcore.doc.pageToRootList.Last.id, AllowReturnLink, pageContentModel.contentName, ArchivePages)
                        Dim isRootPage As Boolean = (cpcore.doc.pageToRootList.Count = 1)
                        If cpCore.doc.authContext.isAdvancedEditing(cpcore, "") Then
                            result = result & cpcore.html.main_GetRecordEditLink(pageContentModel.contentName, cpcore.doc.page.id, (Not isRootPage)) & LiveBody
                        ElseIf isEditing Then
                            result = result & cpcore.html.getEditWrapper("", cpcore.html.main_GetRecordEditLink(pageContentModel.contentName, cpcore.doc.page.id, (Not isRootPage)) & LiveBody)
                        Else
                            result = result & LiveBody
                        End If
                    End If
                End If
                '
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' render the page content
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="ContentID"></param>
        ''' <param name="OrderByClause"></param>
        ''' <param name="AllowChildList"></param>
        ''' <param name="Authoring"></param>
        ''' <param name="rootPageId"></param>
        ''' <param name="AllowReturnLink"></param>
        ''' <param name="RootPageContentName"></param>
        ''' <param name="ArchivePage"></param>
        ''' <returns></returns>

        Friend Shared Function getContentBox_content_Body(cpcore As coreClass, OrderByClause As String, AllowChildList As Boolean, Authoring As Boolean, rootPageId As Integer, AllowReturnLink As Boolean, RootPageContentName As String, ArchivePage As Boolean) As String
            Dim result As String = ""
            Try
                Dim allowChildListComposite As Boolean = AllowChildList And cpcore.doc.page.AllowChildListDisplay
                Dim allowReturnLinkComposite As Boolean = AllowReturnLink And cpcore.doc.page.AllowReturnLinkDisplay
                Dim bodyCopy As String = cpcore.doc.page.Copyfilename.content
                Dim breadCrumb As String = ""
                Dim BreadCrumbDelimiter As String
                Dim BreadCrumbPrefix As String
                Dim isRootPage As Boolean = cpcore.doc.pageToRootList.Count.Equals(1)
                '
                If allowReturnLinkComposite And (Not isRootPage) Then
                    '
                    ' ----- Print Heading if not at root Page
                    '
                    BreadCrumbPrefix = cpcore.siteProperties.getText("BreadCrumbPrefix", "Return to")
                    BreadCrumbDelimiter = cpcore.siteProperties.getText("BreadCrumbDelimiter", " &gt; ")
                    breadCrumb = cpcore.doc.getReturnBreadcrumb(RootPageContentName, cpcore.doc.page.ParentID, rootPageId, "", ArchivePage, BreadCrumbDelimiter)
                    If breadCrumb <> "" Then
                        breadCrumb = cr & "<p class=""ccPageListNavigation"">" & BreadCrumbPrefix & " " & breadCrumb & "</p>"
                    End If
                End If
                result = result & breadCrumb
                '
                If (True) Then
                    Dim IconRow As String = ""
                    If (Not cpcore.doc.authContext.visit.Bot) And (cpcore.doc.page.AllowPrinterVersion Or cpcore.doc.page.AllowEmailPage) Then
                        '
                        ' not a bot, and either print or email allowed
                        '
                        If cpcore.doc.page.AllowPrinterVersion Then
                            Dim QueryString As String = cpcore.doc.refreshQueryString
                            QueryString = genericController.ModifyQueryString(QueryString, rnPageId, genericController.encodeText(cpcore.doc.page.id), True)
                            QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, HardCodedPagePrinterVersion, True)
                            Dim Caption As String = cpcore.siteProperties.getText("PagePrinterVersionCaption", "Printer Version")
                            Caption = genericController.vbReplace(Caption, " ", "&nbsp;")
                            IconRow = IconRow & cr & "&nbsp;&nbsp;<a href=""" & genericController.encodeHTML(cpcore.webServer.requestPage & "?" & QueryString) & """ target=""_blank""><img alt=""image"" src=""/ccLib/images/IconSmallPrinter.gif"" width=""13"" height=""13"" border=""0"" align=""absmiddle""></a>&nbsp<a href=""" & genericController.encodeHTML(cpcore.webServer.requestPage & "?" & QueryString) & """ target=""_blank"" style=""text-decoration:none! important;font-family:sanserif,verdana,helvetica;font-size:11px;"">" & Caption & "</a>"
                        End If
                        If cpcore.doc.page.AllowEmailPage Then
                            Dim QueryString As String = cpcore.doc.refreshQueryString
                            If QueryString <> "" Then
                                QueryString = "?" & QueryString
                            End If
                            Dim EmailBody As String = cpcore.webServer.requestProtocol & cpcore.webServer.requestDomain & cpcore.webServer.requestPathPage & QueryString
                            Dim Caption As String = cpcore.siteProperties.getText("PageAllowEmailCaption", "Email This Page")
                            Caption = genericController.vbReplace(Caption, " ", "&nbsp;")
                            IconRow = IconRow & cr & "&nbsp;&nbsp;<a HREF=""mailto:?SUBJECT=You might be interested in this&amp;BODY=" & EmailBody & """><img alt=""image"" src=""/ccLib/images/IconSmallEmail.gif"" width=""13"" height=""13"" border=""0"" align=""absmiddle""></a>&nbsp;<a HREF=""mailto:?SUBJECT=You might be interested in this&amp;BODY=" & EmailBody & """ style=""text-decoration:none! important;font-family:sanserif,verdana,helvetica;font-size:11px;"">" & Caption & "</a>"
                        End If
                    End If
                    If IconRow <> "" Then
                        result = result _
                        & cr & "<div style=""text-align:right;"">" _
                        & genericController.htmlIndent(IconRow) _
                        & cr & "</div>"
                    End If
                End If
                '
                ' ----- Start Text Search
                '
                Dim Cell As String = ""
                If cpCore.doc.authContext.isQuickEditing(cpcore, pageContentModel.contentName) Then
                    Cell = Cell & cpcore.doc.getQuickEditing(rootPageId, OrderByClause, AllowChildList, AllowReturnLink, ArchivePage, cpcore.doc.page.ContactMemberID, cpcore.doc.page.ChildListSortMethodID, allowChildListComposite, ArchivePage)
                Else
                    '
                    ' ----- Headline
                    '
                    If cpcore.doc.page.Headline <> "" Then
                        Dim headline As String = cpcore.html.main_encodeHTML(cpcore.doc.page.Headline)
                        Cell = Cell & cr & "<h1>" & headline & "</h1>"
                        '
                        ' Add AC end here to force the end of any left over AC tags (like language)
                        Cell = Cell & ACTagEnd
                    End If
                    '
                    ' ----- Page Copy
                    If bodyCopy = "" Then
                        '
                        ' Page copy is empty if  Links Enabled put in a blank line to separate edit from add tag
                        If cpCore.doc.authContext.isEditing(pageContentModel.contentName) Then
                            bodyCopy = cr & "<p><!-- Empty Content Placeholder --></p>"
                        End If
                    Else
                        bodyCopy = bodyCopy & cr & ACTagEnd
                    End If
                    '
                    ' ----- Wrap content body
                    Cell = Cell _
                        & cr & "<!-- ContentBoxBodyStart -->" _
                        & genericController.htmlIndent(bodyCopy) _
                        & cr & "<!-- ContentBoxBodyEnd -->"
                    '
                    ' ----- Child pages
                    If allowChildListComposite Or cpCore.doc.authContext.isEditingAnything() Then
                        If Not allowChildListComposite Then
                            Cell = Cell & cpcore.html.html_GetAdminHintWrapper("Automatic Child List display is disabled for this page. It is displayed here because you are in editing mode. To enable automatic child list display, see the features tab for this page.")
                        End If
                        Dim AddonStatusOK As Boolean = False
                        Dim addon As Models.Entity.addonModel = Models.Entity.addonModel.create(cpcore, cpcore.siteProperties.childListAddonID)
                        Dim executeContext As New CPUtilsBaseClass.addonExecuteContext() With {
                            .addonType = CPUtilsBaseClass.addonContext.ContextPage,
                            .hostRecord = New CPUtilsBaseClass.addonExecuteHostRecordContext() With {
                                .contentName = Models.Entity.pageContentModel.contentName,
                                .fieldName = "",
                                .recordId = cpcore.doc.page.id
                            },
                            .instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpcore, cpcore.doc.page.ChildListInstanceOptions),
                            .instanceGuid = PageChildListInstanceID,
                            .wrapperID = cpcore.siteProperties.defaultWrapperID
                        }
                        Cell &= cpcore.addon.execute(addon, executeContext)
                        'Cell = Cell & cpcore.addon.execute_legacy2(cpcore.siteProperties.childListAddonID, "", cpcore.doc.page.ChildListInstanceOptions, CPUtilsBaseClass.addonContext.ContextPage, Models.Entity.pageContentModel.contentName, cpcore.doc.page.id, "", PageChildListInstanceID, False, cpcore.siteProperties.defaultWrapperID, "", AddonStatusOK, Nothing)
                    End If
                End If
                '
                ' ----- End Text Search
                result = result _
                    & cr & "<!-- TextSearchStart -->" _
                    & genericController.htmlIndent(Cell) _
                    & cr & "<!-- TextSearchEnd -->"
                '
                ' ----- Page See Also
                If cpcore.doc.page.AllowSeeAlso Then
                    result = result _
                        & cr & "<div>" _
                        & genericController.htmlIndent(getSeeAlso(cpcore, pageContentModel.contentName, cpcore.doc.page.id)) _
                        & cr & "</div>"
                End If
                '
                ' ----- Allow More Info
                If (cpcore.doc.page.ContactMemberID <> 0) And cpcore.doc.page.AllowMoreInfo Then
                    result = result & cr & "<ac TYPE=""" & ACTypeContact & """>"
                End If
                '
                ' ----- Feedback
                If (cpcore.doc.page.ContactMemberID <> 0) And cpcore.doc.page.AllowFeedback Then
                    result = result & cr & "<ac TYPE=""" & ACTypeFeedback & """>"
                End If
                '
                ' ----- Last Modified line
                If (cpcore.doc.page.ModifiedDate <> Date.MinValue) And cpcore.doc.page.AllowLastModifiedFooter Then
                    result = result & cr & "<p>This page was last modified " & FormatDateTime(cpcore.doc.page.ModifiedDate)
                    If cpCore.doc.authContext.isAuthenticatedAdmin(cpcore) Then
                        If cpcore.doc.page.ModifiedBy = 0 Then
                            result = result & " (admin only: modified by unknown)"
                        Else
                            Dim personName As String = cpcore.db.getRecordName("people", cpcore.doc.page.ModifiedBy)
                            If personName = "" Then
                                result = result & " (admin only: modified by person with unnamed or deleted record #" & cpcore.doc.page.ModifiedBy & ")"
                            Else
                                result = result & " (admin only: modified by " & personName & ")"
                            End If
                        End If
                    End If
                    result = result & "</p>"
                End If
                '
                ' ----- Last Reviewed line
                If (cpcore.doc.page.DateReviewed <> Date.MinValue) And cpcore.doc.page.AllowReviewedFooter Then
                    result = result & cr & "<p>This page was last reviewed " & FormatDateTime(cpcore.doc.page.DateReviewed, vbLongDate)
                    If cpCore.doc.authContext.isAuthenticatedAdmin(cpcore) Then
                        If cpcore.doc.page.ReviewedBy = 0 Then
                            result = result & " (by unknown)"
                        Else
                            Dim personName As String = cpcore.db.getRecordName("people", cpcore.doc.page.ReviewedBy)
                            If personName = "" Then
                                result = result & " (by person with unnamed or deleted record #" & cpcore.doc.page.ReviewedBy & ")"
                            Else
                                result = result & " (by " & personName & ")"
                            End If
                        End If
                        result = result & ".</p>"
                    End If
                End If
                '
                ' ----- Page Content Message Footer
                If cpcore.doc.page.AllowMessageFooter Then
                    Dim pageContentMessageFooter As String = cpcore.siteProperties.getText("PageContentMessageFooter", "")
                    If (pageContentMessageFooter <> "") Then
                        result = result & cr & "<p>" & pageContentMessageFooter & "</p>"
                    End If
                End If
                'Call cpcore.db.cs_Close(CS)
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '=============================================================================
        ' Print the See Also listing
        '   ContentName is the name of the parent table
        '   RecordID is the parent RecordID
        '=============================================================================
        '
        Public Shared Function getSeeAlso(cpcore As coreClass, ByVal ContentName As String, ByVal RecordID As Integer) As String
            Dim result As String = ""
            Try
                Dim CS As Integer
                Dim SeeAlsoLink As String
                Dim ContentID As Integer
                Dim SeeAlsoCount As Integer
                Dim Copy As String
                Dim MethodName As String
                Dim iContentName As String
                Dim iRecordID As Integer
                Dim IsEditingLocal As Boolean
                '
                iContentName = genericController.encodeText(ContentName)
                iRecordID = genericController.EncodeInteger(RecordID)
                '
                MethodName = "result"
                '
                SeeAlsoCount = 0
                If iRecordID > 0 Then
                    ContentID = models.complex.cdefmodel.getcontentid(cpcore, iContentName)
                    If (ContentID > 0) Then
                        '
                        ' ----- Set authoring only for valid ContentName
                        '
                        IsEditingLocal = cpCore.doc.authContext.isEditing(iContentName)
                    Else
                        '
                        ' ----- if iContentName was bad, maybe they put table in, no authoring
                        '
                        ContentID = Models.Complex.cdefModel.getContentIDByTablename(cpcore, iContentName)
                    End If
                    If (ContentID > 0) Then
                        '
                        CS = cpcore.db.csOpen("See Also", "((active<>0)AND(ContentID=" & ContentID & ")AND(RecordID=" & iRecordID & "))")
                        Do While (cpcore.db.csOk(CS))
                            SeeAlsoLink = (cpcore.db.csGetText(CS, "Link"))
                            If SeeAlsoLink <> "" Then
                                result = result & cr & "<li class=""ccListItem"">"
                                If genericController.vbInstr(1, SeeAlsoLink, "://") = 0 Then
                                    SeeAlsoLink = cpcore.webServer.requestProtocol & SeeAlsoLink
                                End If
                                If IsEditingLocal Then
                                    result = result & cpcore.html.main_GetRecordEditLink2("See Also", (cpcore.db.csGetInteger(CS, "ID")), False, "", cpCore.doc.authContext.isEditing("See Also"))
                                End If
                                result = result & "<a href=""" & genericController.encodeHTML(SeeAlsoLink) & """ target=""_blank"">" & (cpcore.db.csGetText(CS, "Name")) & "</A>"
                                Copy = (cpcore.db.csGetText(CS, "Brief"))
                                If Copy <> "" Then
                                    result = result & "<br >" & genericController.AddSpan(Copy, "ccListCopy")
                                End If
                                SeeAlsoCount = SeeAlsoCount + 1
                                result = result & "</li>"
                            End If
                            cpcore.db.csGoNext(CS)
                        Loop
                        cpcore.db.csClose(CS)
                        '
                        If IsEditingLocal Then
                            SeeAlsoCount = SeeAlsoCount + 1
                            result = result & cr & "<li class=""ccListItem"">" & cpcore.html.main_GetRecordAddLink("See Also", "RecordID=" & iRecordID & ",ContentID=" & ContentID) & "</LI>"
                        End If
                    End If
                    '
                    If SeeAlsoCount = 0 Then
                        result = ""
                    Else
                        result = "<p>See Also" & cr & "<ul class=""ccList"">" & genericController.htmlIndent(result) & cr & "</ul></p>"
                    End If
                End If
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        ' Print the "for more information, please contact" line
        '
        '========================================================================
        '
        Public Function main_GetMoreInfo(cpcore As coreClass, ByVal contactMemberID As Integer) As String
            Dim result As String = ""
            Try
                main_GetMoreInfo = getMoreInfoHtml(cpcore, genericController.EncodeInteger(contactMemberID))
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        ' ----- prints a link to the feedback popup form
        '
        '   Creates a sub-form that when submitted, is logged by the notes
        '   system (in MembersLib right now). When submitted, it prints a thank you
        '   message.
        '
        '========================================================================
        '
        Public Shared Function main_GetFeedbackForm(cpcore As coreClass, ByVal ContentName As String, ByVal RecordID As Integer, ByVal ToMemberID As Integer, Optional ByVal headline As String = "") As String
            Dim result As String = ""
            Try
                Dim Panel As String
                Dim Copy As String
                Dim FeedbackButton As String
                Dim NoteCopy As String = String.Empty
                Dim NoteFromEmail As String
                Dim NoteFromName As String
                Dim CS As Integer
                Dim iContentName As String
                Dim iRecordID As Integer
                Dim iToMemberID As Integer
                Dim iHeadline As String
                '
                iContentName = genericController.encodeText(ContentName)
                iRecordID = genericController.EncodeInteger(RecordID)
                iToMemberID = genericController.EncodeInteger(ToMemberID)
                iHeadline = genericController.encodeEmptyText(headline, "")
                '
                Const FeedbackButtonSubmit = "Submit"
                '
                FeedbackButton = cpcore.docProperties.getText("fbb")
                Select Case FeedbackButton
                    Case FeedbackButtonSubmit
                        '
                        ' ----- form was submitted, save the note, send it and say thanks
                        '
                        NoteFromName = cpcore.docProperties.getText("NoteFromName")
                        NoteFromEmail = cpcore.docProperties.getText("NoteFromEmail")
                        '
                        NoteCopy = NoteCopy & "Feedback Submitted" & BR
                        NoteCopy = NoteCopy & "From " & NoteFromName & " at " & NoteFromEmail & BR
                        NoteCopy = NoteCopy & "Replying to:" & BR
                        If iHeadline <> "" Then
                            NoteCopy = NoteCopy & "    Article titled [" & iHeadline & "]" & BR
                        End If
                        NoteCopy = NoteCopy & "    Record [" & iRecordID & "] in Content Definition [" & iContentName & "]" & BR
                        NoteCopy = NoteCopy & BR
                        NoteCopy = NoteCopy & "<b>Comments</b>" & BR
                        '
                        Copy = cpcore.docProperties.getText("NoteCopy")
                        If (Copy = "") Then
                            NoteCopy = NoteCopy & "[no comments entered]" & BR
                        Else
                            NoteCopy = NoteCopy & cpcore.html.convertCRLFToHtmlBreak(Copy) & BR
                        End If
                        '
                        NoteCopy = NoteCopy & BR
                        NoteCopy = NoteCopy & "<b>Content on which the comments are based</b>" & BR
                        '
                        CS = cpcore.db.csOpen(iContentName, "ID=" & iRecordID)
                        Copy = "[the content of this page is not available]" & BR
                        If cpcore.db.csOk(CS) Then
                            Copy = (cpcore.db.csGet(CS, "copyFilename"))
                            'Copy = main_EncodeContent5(Copy, c.authcontext.user.userid, iContentName, iRecordID, 0, False, False, True, True, False, True, "", "", False, 0)
                        End If
                        NoteCopy = NoteCopy & Copy & BR
                        Call cpcore.db.csClose(CS)
                        '
                        Call cpcore.email.sendPerson(iToMemberID, NoteFromEmail, "Feedback Form Submitted", NoteCopy, False, True, 0, "", False)
                        '
                        ' ----- Note sent, say thanks
                        '
                        result = result & "<p>Thank you. Your feedback was received.</p>"
                    Case Else
                        '
                        ' ----- print the feedback submit form
                        '
                        Panel = "<form Action=""" & cpcore.webServer.serverFormActionURL & "?" & cpcore.doc.refreshQueryString & """ Method=""post"">"
                        Panel = Panel & "<table border=""0"" cellpadding=""4"" cellspacing=""0"" width=""100%"">"
                        Panel = Panel & "<tr>"
                        Panel = Panel & "<td colspan=""2""><p>Your feedback is welcome</p></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- From Name
                        '
                        Copy = cpCore.doc.authContext.user.name
                        Panel = Panel & "<td align=""right"" width=""100""><p>Your Name</p></td>"
                        Panel = Panel & "<td align=""left""><input type=""text"" name=""NoteFromName"" value=""" & genericController.encodeHTML(Copy) & """></span></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- From Email address
                        '
                        Copy = cpCore.doc.authContext.user.Email
                        Panel = Panel & "<td align=""right"" width=""100""><p>Your Email</p></td>"
                        Panel = Panel & "<td align=""left""><input type=""text"" name=""NoteFromEmail"" value=""" & genericController.encodeHTML(Copy) & """></span></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- Message
                        '
                        Copy = ""
                        Panel = Panel & "<td align=""right"" width=""100"" valign=""top""><p>Feedback</p></td>"
                        Panel = Panel & "<td>" & cpcore.html.html_GetFormInputText2("NoteCopy", Copy, 4, 40, "TextArea", False) & "</td>"
                        'Panel = Panel & "<td><textarea ID=""TextArea"" rows=""4"" cols=""40"" name=""NoteCopy"">" & Copy & "</textarea></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- submit button
                        '
                        Panel = Panel & "<td>&nbsp;</td>"
                        Panel = Panel & "<td><input type=""submit"" name=""fbb"" value=""" & FeedbackButtonSubmit & """></td>"
                        Panel = Panel & "</tr></table>"
                        Panel = Panel & "</form>"
                        '
                        result = Panel
                End Select
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
            Return result
        End Function

		
		
    End Class
End Namespace