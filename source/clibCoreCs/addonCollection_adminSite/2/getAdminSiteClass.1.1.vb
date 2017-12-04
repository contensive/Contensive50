
Option Explicit On
Option Strict On
'
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Imports System.Xml
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
'
Namespace Contensive.Addons.AdminSite
    Partial Public Class getAdminSiteClass
        Inherits Contensive.BaseClasses.AddonBaseClass

        '
        '
        '=============================================================================================
        '   Create a duplicate record
        '=============================================================================================
        '
        Private Sub ProcessActionDuplicate(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("ProcessActionDuplicate")
            ' converted array to dictionary - Dim FieldPointer As Integer
            '
            If Not (cpCore.doc.debug_iUserError <> "") Then
                Select Case genericController.vbUCase(adminContent.ContentTableName)
                    Case "CCEMAIL"
                        '
                        ' --- preload array with values that may not come back in response
                        '
                        Call LoadEditRecord(adminContent, editRecord)
                        Call LoadEditRecord_Request(adminContent, editRecord)
                        '
                        If Not (cpCore.doc.debug_iUserError <> "") Then
                            '
                            ' ----- Convert this to the Duplicate
                            '
                            If adminContent.fields.ContainsKey("submitted") Then
                                editRecord.fieldsLc("submitted").value = False
                            End If
                            If adminContent.fields.ContainsKey("sent") Then
                                editRecord.fieldsLc("sent").value = False
                            End If
                            '
                            editRecord.id = 0
                            Call cpCore.doc.addRefreshQueryString("id", genericController.encodeText(editRecord.id))
                        End If
                    Case Else
                        '
                        '
                        '
                        '
                        ' --- preload array with values that may not come back in response
                        '
                        Call LoadEditRecord(adminContent, editRecord)
                        Call LoadEditRecord_Request(adminContent, editRecord)
                        '
                        If Not (cpCore.doc.debug_iUserError <> "") Then
                            '
                            ' ----- Convert this to the Duplicate
                            '
                            editRecord.id = 0
                            '
                            ' block fields that should not duplicate
                            '
                            If editRecord.fieldsLc.ContainsKey("ccguid") Then
                                editRecord.fieldsLc("ccguid").value = ""
                            End If
                            '
                            If editRecord.fieldsLc.ContainsKey("dateadded") Then
                                editRecord.fieldsLc("dateadded").value = Date.MinValue
                            End If
                            '
                            If editRecord.fieldsLc.ContainsKey("modifieddate") Then
                                editRecord.fieldsLc("modifieddate").value = Date.MinValue
                            End If
                            '
                            If editRecord.fieldsLc.ContainsKey("modifiedby") Then
                                editRecord.fieldsLc("modifiedby").value = 0
                            End If
                            '
                            ' block fields that must be unique
                            '
                            For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In adminContent.fields
                                Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                                With field
                                    If genericController.vbLCase(.nameLc) = "email" Then
                                        If (LCase(adminContent.ContentTableName) = "ccmembers") And (genericController.EncodeBoolean(cpCore.siteProperties.getBoolean("allowemaillogin", False))) Then
                                            editRecord.fieldsLc(.nameLc).value = ""
                                        End If
                                    End If
                                    If .UniqueName Then
                                        editRecord.fieldsLc(.nameLc).value = ""
                                    End If
                                End With
                            Next
                            '
                            Call cpCore.doc.addRefreshQueryString("id", genericController.encodeText(editRecord.id))
                        End If
                        'Call cpCore.htmldoc.main_AddUserError("The create duplicate action is not supported for this content.")
                End Select
                AdminForm = AdminSourceForm
                AdminAction = AdminActionNop ' convert so action can be used in as a refresh
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("ProcessActionDuplicate")
            '
        End Sub
        '
        '========================================================================
        ' PrintMenuTop()
        '   Prints the menu section of the admin page
        '========================================================================
        '
        Private Function GetMenuTopMode() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetMenuTopMode")
            '
            'Const MenuEntryContentName = cnNavigatorEntries
            '
            Dim CSMenus As Integer
            Dim Name As String
            Dim Id As Integer
            Dim ParentID As Integer
            Dim NewWindow As Boolean
            Dim Link As String
            Dim LinkLabel As String
            Dim LinkCID As Integer
            Dim MenuPointer As Integer
            Dim StyleSheet As String = ""
            Dim StyleSheetHover As String = ""
            Dim ImageLink As String
            Dim ImageOverLink As String
            Dim MenuDate As Date
            Dim BakeDate As Date
            Dim BakeName As String
            Dim MenuHeader As String
            Dim editableCdefIdList As List(Of Integer)
            Dim ContentID As Integer
            Dim IsAdminLocal As Boolean
            Dim MenuClose As String
            Dim MenuDelimiterPosition As Integer
            Dim AccessOK As Boolean
            '
            Const MenuDelimiter = vbCrLf & "<!-- Menus -->" & vbCrLf
            '
            ' Create the menu
            '
            If AdminMenuModeID = AdminMenuModeTop Then
                '
                ' ----- Get the baked version
                '
                BakeName = "AdminMenu" & Format(cpCore.doc.authContext.user.id, "00000000")
                GetMenuTopMode = genericController.encodeText(cpCore.cache.getObject(Of String)(BakeName))
                MenuDelimiterPosition = genericController.vbInstr(1, GetMenuTopMode, MenuDelimiter, vbTextCompare)
                If MenuDelimiterPosition > 1 Then
                    MenuClose = Mid(GetMenuTopMode, MenuDelimiterPosition + Len(MenuDelimiter))
                    GetMenuTopMode = Mid(GetMenuTopMode, 1, MenuDelimiterPosition - 1)
                Else
                    'If GetMenuTopMode = "" Then
                    '
                    ' ----- Bake the menu
                    '
                    CSMenus = GetMenuCSPointer("")
                    'CSMenus = cpCore.app_openCsSql_Rev_Internal("default", GetMenuSQLNew())
                    If cpCore.db.csOk(CSMenus) Then
                        '
                        ' There are menu items to bake
                        '
                        IsAdminLocal = cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)
                        If Not IsAdminLocal Then
                            '
                            ' content managers, need the ContentManagementList
                            '
                            editableCdefIdList = Models.Complex.cdefModel.getEditableCdefIdList(cpCore)
                        Else
                            editableCdefIdList = New List(Of Integer)
                        End If
                        ImageLink = ""
                        ImageOverLink = ""
                        Do While cpCore.db.csOk(CSMenus)
                            ContentID = cpCore.db.csGetInteger(CSMenus, "ContentID")
                            If IsAdminLocal Or ContentID = 0 Then
                                AccessOK = True
                            ElseIf editableCdefIdList.Contains(ContentID) Then
                                AccessOK = True
                            Else
                                AccessOK = False
                            End If
                            Id = cpCore.db.csGetInteger(CSMenus, "ID")
                            ParentID = cpCore.db.csGetInteger(CSMenus, "ParentID")
                            If AccessOK Then
                                Link = GetMenuLink(cpCore.db.csGet(CSMenus, "LinkPage"), ContentID)
                                If genericController.vbInstr(1, Link, "?") = 1 Then
                                    Link = cpCore.serverConfig.appConfig.adminRoute & Link
                                End If
                            Else
                                Link = ""
                            End If
                            LinkLabel = cpCore.db.csGet(CSMenus, "Name")
                            'If LinkLabel = "Calendar" Then
                            '    Link = Link
                            '    End If
                            NewWindow = cpCore.db.csGetBoolean(CSMenus, "NewWindow")
                            Call cpCore.menuFlyout.menu_AddEntry(genericController.encodeText(Id), ParentID.ToString, ImageLink, ImageOverLink, Link, LinkLabel, StyleSheet, StyleSheetHover, NewWindow)

                            Call cpCore.db.csGoNext(CSMenus)
                        Loop
                    End If
                    cpCore.db.csClose(CSMenus)
                    '            '
                    '            ' Add in top level node for "switch to navigator"
                    '            '
                    '            Call cpCore.htmldoc.main_AddMenuEntry("GoToNav", 0, "?" & cpCore.main_RefreshQueryString & "&mm=1", "", "", "Switch To Navigator", StyleSheet, StyleSheetHover, False)
                    '
                    ' Create menus
                    '
                    Dim ButtonCnt As Integer
                    CSMenus = GetMenuCSPointer("(ParentID is null)or(ParentID=0)")
                    If cpCore.db.csOk(CSMenus) Then
                        GetMenuTopMode = "<table border=""0"" cellpadding=""0"" cellspacing=""0""><tr>"
                        ButtonCnt = 0
                        Do While cpCore.db.csOk(CSMenus)
                            Name = cpCore.db.csGet(CSMenus, "Name")
                            Id = cpCore.db.csGetInteger(CSMenus, "ID")
                            NewWindow = cpCore.db.csGetBoolean(CSMenus, "NewWindow")
                            MenuHeader = cpCore.menuFlyout.getMenu(genericController.encodeText(Id), 0)
                            If MenuHeader <> "" Then
                                If ButtonCnt > 0 Then
                                    GetMenuTopMode = GetMenuTopMode & "<td class=""ccFlyoutDelimiter"">|</td>"
                                End If
                                'GetMenuTopMode = GetMenuTopMode & "<td width=""1"" class=""ccPanelShadow""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td>"
                                'GetMenuTopMode = GetMenuTopMode & "<td width=""1"" class=""ccPanelHilite""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td>"
                                '
                                ' --- Add New GetMenuTopMode Button and leave the column open
                                '
                                Link = ""
                                GetMenuTopMode = GetMenuTopMode & "<td class=""ccFlyoutButton"">" & MenuHeader & "</td>"
                                ' GetMenuTopMode = GetMenuTopMode & "<td><nobr>&nbsp;" & MenuHeader & "&nbsp;</nobr></td>"
                            End If
                            ButtonCnt = ButtonCnt + 1
                            Call cpCore.db.csGoNext(CSMenus)
                        Loop
                        GetMenuTopMode = GetMenuTopMode & "</tr></table>"
                        GetMenuTopMode = cpCore.html.main_GetPanel(GetMenuTopMode, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 1)
                    End If
                    cpCore.db.csClose(CSMenus)
                    '
                    ' Save the Baked Menu
                    '
                    MenuClose = cpCore.menuFlyout.menu_GetClose()
                    'GetMenuTopMode = GetMenuTopMode & cpCore.main_GetMenuClose
                    Call cpCore.cache.setContent(BakeName, GetMenuTopMode & MenuDelimiter & MenuClose, "Navigator Entries,People,Content,Groups,Group Rules")
                End If
                cpCore.doc.htmlForEndOfBody = cpCore.doc.htmlForEndOfBody & MenuClose
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetMenuTopMode")
            '
        End Function
        '
        '========================================================================
        ' Read and save a GetForm_InputCheckList
        '   see GetForm_InputCheckList for an explaination of the input
        '========================================================================
        '
        Private Sub SaveMemberRules(PeopleID As Integer)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("SaveMemberRules")
            '
            Dim GroupCount As Integer
            Dim GroupPointer As Integer
            Dim CSPointer As Integer
            Dim MethodName As String
            Dim GroupID As Integer
            Dim RuleNeeded As Boolean
            Dim CSRule As Integer
            Dim DateExpires As Date
            Dim DateExpiresVariant As Object
            Dim RuleActive As Boolean
            Dim RuleDateExpires As Date
            Dim MemberRuleID As Integer
            '
            MethodName = "SaveMemberRules"
            '
            ' --- create MemberRule records for all selected
            '
            GroupCount = cpCore.docProperties.getInteger("MemberRules.RowCount")
            If GroupCount > 0 Then
                For GroupPointer = 0 To GroupCount - 1
                    '
                    ' ----- Read Response
                    '
                    GroupID = cpCore.docProperties.getInteger("MemberRules." & GroupPointer & ".ID")
                    RuleNeeded = cpCore.docProperties.getBoolean("MemberRules." & GroupPointer)
                    DateExpires = cpCore.docProperties.getDate("MemberRules." & GroupPointer & ".DateExpires")
                    If DateExpires = Date.MinValue Then
                        DateExpiresVariant = DBNull.Value
                    Else
                        DateExpiresVariant = DateExpires
                    End If
                    '
                    ' ----- Update Record
                    '
                    CSRule = cpCore.db.csOpen("Member Rules", "(MemberID=" & PeopleID & ")and(GroupID=" & GroupID & ")", , False, , , , "Active,MemberID,GroupID,DateExpires")
                    If Not cpCore.db.csOk(CSRule) Then
                        '
                        ' No record exists
                        '
                        If RuleNeeded Then
                            '
                            ' No record, Rule needed, add it
                            '
                            Call cpCore.db.csClose(CSRule)
                            CSRule = cpCore.db.csInsertRecord("Member Rules")
                            If cpCore.db.csOk(CSRule) Then
                                Call cpCore.db.csSet(CSRule, "Active", True)
                                Call cpCore.db.csSet(CSRule, "MemberID", PeopleID)
                                Call cpCore.db.csSet(CSRule, "GroupID", GroupID)
                                Call cpCore.db.csSet(CSRule, "DateExpires", DateExpires)
                            End If
                            Call cpCore.db.csClose(CSRule)
                        Else
                            '
                            ' No record, no Rule needed, ignore it
                            '
                            Call cpCore.db.csClose(CSRule)
                        End If
                    Else
                        '
                        ' Record exists
                        '
                        If RuleNeeded Then
                            '
                            ' record exists, and it is needed, update the DateExpires if changed
                            '
                            RuleActive = cpCore.db.csGetBoolean(CSRule, "active")
                            RuleDateExpires = cpCore.db.csGetDate(CSRule, "DateExpires")
                            If (Not RuleActive) Or (RuleDateExpires <> DateExpires) Then
                                Call cpCore.db.csSet(CSRule, "Active", True)
                                Call cpCore.db.csSet(CSRule, "DateExpires", DateExpires)
                            End If
                            Call cpCore.db.csClose(CSRule)
                        Else
                            '
                            ' record exists and it is not needed, delete it
                            '
                            MemberRuleID = cpCore.db.csGetInteger(CSRule, "ID")
                            Call cpCore.db.csClose(CSRule)
                            Call cpCore.db.deleteTableRecord("ccMemberRules", MemberRuleID, "Default")
                        End If
                    End If
                Next
            End If
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("SaveMemberRules")
        End Sub
        '
        '===========================================================================
        '
        '===========================================================================
        '
        Private Function GetForm_Error(UserError As String, DeveloperError As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Error")
            '
            If DeveloperError <> "" Then
                Throw (New Exception("error [" & DeveloperError & "], user error [" & UserError & "]"))
            End If
            If UserError <> "" Then
                Call errorController.error_AddUserError(cpCore, UserError)
                GetForm_Error = AdminFormErrorOpen & errorController.error_GetUserError(cpCore) & AdminFormErrorClose
            End If
            '
            Exit Function
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Error")
        End Function
        '
        '=============================================================================
        ' Create a child content
        '=============================================================================
        '
        Private Function GetContentChildTool() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetContentChildTool")
            '
            Dim IsEmptyList As Boolean
            'Dim cmc As cpCoreClass
            'Dim GUIDGenerator As guidClass
            Dim ccGuid As String
            Dim SupportAddonID As Boolean
            Dim SupportGuid As Boolean
            Dim MenuContentName As String
            Dim ParentID As Integer
            Dim ParentName As String
            Dim CSEntry As Integer
            Dim ParentContentID As Integer
            Dim ParentContentName As String
            Dim ChildContentName As String = ""
            Dim ChildContentID As Integer
            Dim AddAdminMenuEntry As Boolean
            Dim CS As Integer
            Dim MenuName As String
            Dim AdminOnly As Boolean
            Dim DeveloperOnly As Boolean
            Dim Content As New stringBuilderLegacyController
            Dim FieldValue As String
            Dim NewGroup As Boolean
            Dim GroupID As Integer
            Dim NewGroupName As String = ""
            Dim ButtonBar As String
            Dim Button As String
            Dim Adminui As New adminUIController(cpCore)
            Dim Caption As String
            Dim Description As String = ""
            Dim ButtonList As String = ""
            Dim BlockForm As Boolean
            '
            Button = cpCore.docProperties.getText(RequestNameButton)
            If Button = ButtonCancel Then
                '
                '
                '
                Return cpCore.webServer.redirect("/" & cpCore.serverConfig.appConfig.adminRoute, "GetContentChildTool, Cancel Button Pressed")
            ElseIf Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                '
                '
                '
                ButtonList = ButtonCancel
                Content.Add(Adminui.GetFormBodyAdminOnly())
            Else
                '
                If (Button <> ButtonOK) Then
                    '
                    ' Load defaults
                    '
                    ParentContentID = cpCore.docProperties.getInteger("ParentContentID")
                    If ParentContentID = 0 Then
                        ParentContentID = Models.Complex.cdefModel.getContentId(cpCore, "Page Content")
                    End If
                    AddAdminMenuEntry = True
                    GroupID = 0
                Else
                    '
                    ' Process input
                    '
                    ParentContentID = cpCore.docProperties.getInteger("ParentContentID")
                    ParentContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ParentContentID)
                    ChildContentName = cpCore.docProperties.getText("ChildContentName")
                    AddAdminMenuEntry = cpCore.docProperties.getBoolean("AddAdminMenuEntry")
                    GroupID = cpCore.docProperties.getInteger("GroupID")
                    NewGroup = cpCore.docProperties.getBoolean("NewGroup")
                    NewGroupName = cpCore.docProperties.getText("NewGroupName")
                    '
                    If (ParentContentName = "") Or (ChildContentName = "") Then
                        errorController.error_AddUserError(cpCore, "You must select a parent and provide a child name.")
                    Else
                        '
                        ' Create Definition
                        '
                        Description = Description _
                            & "<div>&nbsp;</div>" _
                            & "<div>Creating content [" & ChildContentName & "] from [" & ParentContentName & "]</div>"
                        Call Models.Complex.cdefModel.createContentChild(cpCore, ChildContentName, ParentContentName, cpCore.doc.authContext.user.id)
                        ChildContentID = Models.Complex.cdefModel.getContentId(cpCore, ChildContentName)
                        '
                        ' Create Group and Rule
                        '
                        If NewGroup And (NewGroupName <> "") Then
                            CS = cpCore.db.csOpen("Groups", "name=" & cpCore.db.encodeSQLText(NewGroupName))
                            If cpCore.db.csOk(CS) Then
                                Description = Description _
                                    & "<div>Group [" & NewGroupName & "] already exists, using existing group.</div>"
                                GroupID = cpCore.db.csGetInteger(CS, "ID")
                            Else
                                Description = Description _
                                    & "<div>Creating new group [" & NewGroupName & "]</div>"
                                Call cpCore.db.csClose(CS)
                                CS = cpCore.db.csInsertRecord("Groups")
                                If cpCore.db.csOk(CS) Then
                                    GroupID = cpCore.db.csGetInteger(CS, "ID")
                                    Call cpCore.db.csSet(CS, "Name", NewGroupName)
                                    Call cpCore.db.csSet(CS, "Caption", NewGroupName)
                                End If
                            End If
                            Call cpCore.db.csClose(CS)
                        End If
                        If GroupID <> 0 Then
                            CS = cpCore.db.csInsertRecord("Group Rules")
                            If cpCore.db.csOk(CS) Then
                                Description = Description _
                                    & "<div>Assigning group [" & cpCore.db.getRecordName("Groups", GroupID) & "] to edit content [" & ChildContentName & "].</div>"
                                Call cpCore.db.csSet(CS, "GroupID", GroupID)
                                Call cpCore.db.csSet(CS, "ContentID", ChildContentID)
                            End If
                            Call cpCore.db.csClose(CS)
                        End If
                        '
                        ' Add Admin Menu Entry
                        '
                        If AddAdminMenuEntry Then
                            '
                            ' Add Navigator entries
                            '
                            '                    cmc = cpCore.main_cs_getv()
                            '                    MenuContentName = cnNavigatorEntries
                            '                    SupportAddonID = cpCore.csv_IsContentFieldSupported(MenuContentName, "AddonID")
                            '                    SupportGuid = cpCore.csv_IsContentFieldSupported(MenuContentName, "ccGuid")
                            '                    CS = cpCore.app.csOpen(cnNavigatorEntries, "ContentID=" & ParentContentID)
                            '                    Do While cpCore.app.csv_IsCSOK(CS)
                            '                        ParentID = cpCore.app.csv_cs_getText(CS, "ID")
                            '                        ParentName = cpCore.app.csv_cs_getText(CS, "name")
                            '                        AdminOnly = cpCore.db.cs_getBoolean(CS, "AdminOnly")
                            '                        DeveloperOnly = cpCore.db.cs_getBoolean(CS, "DeveloperOnly")
                            '                        CSEntry = cpCore.app.csv_InsertCSRecord(MenuContentName, SystemMemberID)
                            '                        If cpCore.app.csv_IsCSOK(CSEntry) Then
                            '                            If ParentID = 0 Then
                            '                                Call cpCore.app.csv_SetCS(CSEntry, "ParentID", Null)
                            '                            Else
                            '                                Call cpCore.app.csv_SetCS(CSEntry, "ParentID", ParentID)
                            '                            End If
                            '                            Call cpCore.app.csv_SetCS(CSEntry, "ContentID", ChildContentID)
                            '                            Call cpCore.app.csv_SetCS(CSEntry, "name", ChildContentName)
                            '                            Call cpCore.app.csv_SetCS(CSEntry, "LinkPage", "")
                            '                            Call cpCore.app.csv_SetCS(CSEntry, "SortOrder", "")
                            '                            Call cpCore.app.csv_SetCS(CSEntry, "AdminOnly", AdminOnly)
                            '                            Call cpCore.app.csv_SetCS(CSEntry, "DeveloperOnly", DeveloperOnly)
                            '                            Call cpCore.app.csv_SetCS(CSEntry, "NewWindow", False)
                            '                            Call cpCore.app.csv_SetCS(CSEntry, "Active", True)
                            '                            If SupportAddonID Then
                            '                                Call cpCore.app.csv_SetCS(CSEntry, "AddonID", "")
                            '                            End If
                            '                            If SupportGuid Then
                            '                                GuidGenerator = New guidClass
                            '                                ccGuid = Guid.NewGuid.ToString()
                            '                                GuidGenerator = Nothing
                            '                                Call cpCore.app.csv_SetCS(CSEntry, "ccGuid", ccGuid)
                            '                            End If
                            '                        End If
                            '                        Call cpCore.app.csv_CloseCS(CSEntry)
                            '                        'Call cpCore.csv_VerifyNavigatorEntry2(ccGuid, menuNameSpace, MenuName, ChildContenName, "", "", AdminOnly, DeveloperOnly, False, True, cnNavigatorEntries, "")
                            '                        'Call cpCore.main_CreateAdminMenu(MenuName, ChildContentName, ChildContentName, "", ChildContentName, AdminOnly, DeveloperOnly, False)
                            '                        Description = Description _
                            '                            & "<div>Creating navigator entry for [" & ChildContentName & "] under entry [" & ParentName & "].</div>"
                            '                        cpCore.main_NextCSRecord (CS)
                            '                    Loop
                            '                    Call cpCore.app.closeCS(CS)
                            '
                            ' Add Legacy Navigator Entries
                            '
                            ' -- deprecated
                            'CS = cpCore.db.cs_open(cnNavigatorEntries, "ContentID=" & ParentContentID)
                            'Do While cpCore.db.cs_ok(CS)
                            '    MenuName = cpCore.db.cs_get(CS, "name")
                            '    AdminOnly = cpCore.db.cs_getBoolean(CS, "AdminOnly")
                            '    DeveloperOnly = cpCore.db.cs_getBoolean(CS, "DeveloperOnly")
                            '    If MenuName = "" Then
                            '        MenuName = "Site Content"
                            '    End If
                            '    Call Controllers.appBuilderController.admin_VerifyAdminMenu(cpCore, MenuName, ChildContentName, ChildContentName, "", ChildContentName, AdminOnly, DeveloperOnly, False)
                            '    Description = Description _
                            '        & "<div>Creating Legacy site menu for [" & ChildContentName & "] under entry [" & MenuName & "].</div>"
                            '    cpCore.db.cs_goNext(CS)
                            'Loop
                            'Call cpCore.db.cs_Close(CS)
                        End If
                        '
                        Description = Description _
                            & "<div>&nbsp;</div>" _
                            & "<div>Your new content is ready. <a href=""?" & RequestNameAdminForm & "=22"">Click here</a> to create another Content Definition, or hit [Cancel] to return to the main menu.</div>"
                        ButtonList = ButtonCancel
                        BlockForm = True
                    End If
                    cpCore.doc.clearMetaData()
                    cpCore.cache.invalidateAll()
                End If
                '
                ' Get the form
                '
                If Not BlockForm Then
                    Call Content.Add(Adminui.EditTableOpen)
                    '
                    FieldValue = "<select size=""1"" name=""ParentContentID"" ID=""""><option value="""">Select One</option>"
                    FieldValue = FieldValue & GetContentChildTool_Options(0, ParentContentID)
                    FieldValue = FieldValue & "</select>"
                    'FieldValue = cpCore.htmldoc.main_GetFormInputSelect2("ParentContentID", CStr(ParentContentID), "Content", "(AllowContentChildTool<>0)")

                    Call Content.Add(Adminui.GetEditRow(FieldValue, "Parent Content Name", "", False, False, ""))
                    '
                    FieldValue = cpCore.html.html_GetFormInputText2("ChildContentName", ChildContentName, 1, 40)
                    Call Content.Add(Adminui.GetEditRow(FieldValue, "New Child Content Name", "", False, False, ""))
                    '
                    FieldValue = cpCore.html.html_GetFormInputRadioBox("NewGroup", False.ToString, NewGroup.ToString) & cpCore.html.main_GetFormInputSelect2("GroupID", GroupID, "Groups", "", "", "", IsEmptyList) & "(Select a current group)" _
                        & "<br>" & cpCore.html.html_GetFormInputRadioBox("NewGroup", True.ToString, NewGroup.ToString) & cpCore.html.html_GetFormInputText2("NewGroupName", NewGroupName) & "(Create a new group)"
                    Call Content.Add(Adminui.GetEditRow(FieldValue, "Content Manager Group", "", False, False, ""))
                    '            '
                    '            FieldValue = cpCore.main_GetFormInputCheckBox2("AddAdminMenuEntry", AddAdminMenuEntry) & "(Add Navigator Entry under Manager Site Content &gt; Advanced)"
                    '            Call Content.Add(AdminUI.GetEditRow( FieldValue, "Add Menu Entry", "", False, False, ""))
                    '
                    Call Content.Add(Adminui.EditTableClose)
                    Call Content.Add("</td></tr>" & kmaEndTable)
                    '
                    ButtonList = ButtonOK & "," & ButtonCancel
                End If
                Content.Add(cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormContentChildTool))
            End If
            '
            Caption = "Create Content Definition"
            Description = "<div>This tool is used to create content definitions that help segregate your content into authorable segments.</div>" & Description
            GetContentChildTool = Adminui.GetBody(Caption, ButtonList, "", False, False, Description, "", 0, Content.Text)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetContentChildTool")
        End Function
        '
        '=============================================================================
        ' Create a child content
        '=============================================================================
        '
        Private Function GetContentChildTool_Options(ParentID As Integer, DefaultValue As Integer) As String
            Dim returnOptions As String = ""
            Try
                '
                Dim SQL As String
                Dim CS As Integer
                Dim RecordID As Integer
                Dim RecordName As String
                '
                If ParentID = 0 Then
                    SQL = "select Name, ID from ccContent where ((ParentID<1)or(Parentid is null)) and (AllowContentChildTool<>0);"
                Else
                    SQL = "select Name, ID from ccContent where ParentID=" & ParentID & " and (AllowContentChildTool<>0) and not (allowcontentchildtool is null);"
                End If
                CS = cpCore.db.csOpenSql_rev("Default", SQL)
                Do While cpCore.db.csOk(CS)
                    RecordName = cpCore.db.csGet(CS, "Name")
                    RecordID = cpCore.db.csGetInteger(CS, "ID")
                    If RecordID = DefaultValue Then
                        returnOptions = returnOptions & "<option value=""" & RecordID & """ selected>" & cpCore.db.csGet(CS, "name") & "</option>"
                    Else
                        returnOptions = returnOptions & "<option value=""" & RecordID & """ >" & cpCore.db.csGet(CS, "name") & "</option>"
                    End If
                    returnOptions = returnOptions & GetContentChildTool_Options(RecordID, DefaultValue)
                    cpCore.db.csGoNext(CS)
                Loop
                Call cpCore.db.csClose(CS)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnOptions
        End Function
        ''
        ''=============================================================================
        '' Create a child content
        ''=============================================================================
        ''
        'Private Function GetForm_PageContentMap_OpenNodeList(Criteria As String) As String
        '    On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.GetForm_PageContentMap_OpenNodeList")
        '    '
        '    Dim CS as integer
        '    Dim ParentID as integer
        '    '
        '    CS = cpCore.app.csOpen("Page Content", Criteria, , False, , , "ID,ParentID")
        '    If cpCore.app.csv_IsCSOK(CS) Then
        '        Do While cpCore.app.csv_IsCSOK(CS)
        '            ParentID = cpCore.app.cs_getInteger(CS, "ParentID")
        '            If ParentID <> 0 Then
        '                GetForm_PageContentMap_OpenNodeList = GetForm_PageContentMap_OpenNodeList("ID=" & ParentID)
        '                End If
        '            GetForm_PageContentMap_OpenNodeList = GetForm_PageContentMap_OpenNodeList & "," & cpCore.app.cs_getInteger(CS, "ID")
        '            cpCore.main_NextCSRecord (CS)
        '            Loop
        '        End If
        '    Call cpCore.app.closeCS(CS)
        '    If GetForm_PageContentMap_OpenNodeList <> "" Then
        '        GetForm_PageContentMap_OpenNodeList = Mid(GetForm_PageContentMap_OpenNodeList, 2)
        '        End If
        '    '''Dim th as integer: Exit Function
        '    '
        '    ' ----- Error Trap
        '    '
        'ErrorTrap:
        '    Call HandleClassTrapErrorBubble("GetForm_PageContentMap_OpenNodeList")
        'End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Function GetForm_HouseKeepingControl() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_HouseKeepingControl")
            '
            Dim WhereCount As Integer
            Dim Content As New stringBuilderLegacyController
            Dim AllowContentSpider As Boolean
            Dim status As String
            Dim TargetDomain As String
            Dim EDGPublishToProduction As Boolean
            Dim CSServers As Integer
            Dim Copy As String
            Dim StagingServer As String
            Dim PagesFound As Integer
            Dim PagesComplete As Integer
            Dim SQL As String
            Dim Button As String
            Dim SpiderAuthUsername As String
            Dim SpiderAuthPassword As String
            Dim SpiderAppRootPath As String
            Dim SpiderPassword As String
            Dim SpiderUsername As String
            Dim SPIDERQUERYSTRINGEXCLUDELIST As String
            Dim SPIDERQUERYSTRINGIGNORELIST As String
            'Dim SPIDERREADTIME as integer
            'Dim SpiderURLIgnoreList As String
            Dim QueryString As String
            Dim Result As Integer
            Dim PagesTotal As Integer
            Dim LastCheckDate As Date
            Dim FirstCheckDate As Date
            Dim Caption As String
            Dim SpiderFontsAllowed As String
            Dim SpiderPDFBodyText As String
            Dim ProgressMessage As String
            Dim DateValue As Date
            Dim AgeInDays As String
            Dim ArchiveRecordAgeDays As Integer
            Dim ArchiveTimeOfDay As String
            Dim ArchiveAllowFileClean As Boolean
            Dim Adminui As New adminUIController(cpCore)
            Dim ButtonList As String = ""
            Dim Description As String
            '
            '
            Button = cpCore.docProperties.getText(RequestNameButton)
            If Button = ButtonCancel Then
                '
                '
                '
                Return cpCore.webServer.redirect("/" & cpCore.serverConfig.appConfig.adminRoute, "HouseKeepingControl, Cancel Button Pressed")
            ElseIf Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                '
                '
                '
                ButtonList = ButtonCancel
                Content.Add(Adminui.GetFormBodyAdminOnly())
            Else
                '
                Content.Add(Adminui.EditTableOpen)
                '
                ' Set defaults
                '
                ArchiveRecordAgeDays = (cpCore.siteProperties.getinteger("ArchiveRecordAgeDays", 0))
                ArchiveTimeOfDay = cpCore.siteProperties.getText("ArchiveTimeOfDay", "12:00:00 AM")
                ArchiveAllowFileClean = (cpCore.siteProperties.getBoolean("ArchiveAllowFileClean", False))
                'ArchiveAllowLogClean = genericController.EncodeBoolean(cpCore.main_GetSiteProperty2("ArchiveAllowLogClean", False))

                '
                ' Process Requests
                '
                Select Case Button
                    Case ButtonOK, ButtonSave
                        '
                        ArchiveRecordAgeDays = cpCore.docProperties.getInteger("ArchiveRecordAgeDays")
                        Call cpCore.siteProperties.setProperty("ArchiveRecordAgeDays", genericController.encodeText(ArchiveRecordAgeDays))
                        '
                        ArchiveTimeOfDay = cpCore.docProperties.getText("ArchiveTimeOfDay")
                        Call cpCore.siteProperties.setProperty("ArchiveTimeOfDay", ArchiveTimeOfDay)
                        '
                        ArchiveAllowFileClean = cpCore.docProperties.getBoolean("ArchiveAllowFileClean")
                        Call cpCore.siteProperties.setProperty("ArchiveAllowFileClean", genericController.encodeText(ArchiveAllowFileClean))
                End Select
                '
                If Button = ButtonOK Then
                    Return cpCore.webServer.redirect("/" & cpCore.serverConfig.appConfig.adminRoute, "StaticPublishControl, OK Button Pressed")
                End If
                '
                ' ----- Status
                '
                Call Content.Add(genericController.StartTableRow() & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>Status</b>" & kmaEndTableCell & kmaEndTableRow)
                '
                ' ----- Visits Found
                '
                PagesTotal = 0
                SQL = "SELECT Count(ID) as Result FROM ccVisits;"
                CSServers = cpCore.db.csOpenSql_rev("Default", SQL)
                If cpCore.db.csOk(CSServers) Then
                    PagesTotal = cpCore.db.csGetInteger(CSServers, "Result")
                End If
                Call cpCore.db.csClose(CSServers)
                Call Content.Add(Adminui.GetEditRow(SpanClassAdminNormal & PagesTotal, "Visits Found", "", False, False, ""))
                '
                ' ----- Oldest Visit
                '
                Copy = "unknown"
                AgeInDays = "unknown"
                SQL = cpCore.db.GetSQLSelect("default", "ccVisits", "DateAdded", , "ID", , 1)
                CSServers = cpCore.db.csOpenSql_rev("Default", SQL)
                'SQL = "SELECT Top 1 DateAdded FROM ccVisits order by ID;"
                'CSServers = cpCore.app_openCsSql_Rev_Internal("Default", SQL)
                If cpCore.db.csOk(CSServers) Then
                    DateValue = cpCore.db.csGetDate(CSServers, "DateAdded")
                    If DateValue <> Date.MinValue Then
                        Copy = genericController.encodeText(DateValue)
                        AgeInDays = genericController.encodeText(Int(cpCore.doc.profileStartTime - DateValue))
                    End If
                End If
                Call cpCore.db.csClose(CSServers)
                Call Content.Add(Adminui.GetEditRow(SpanClassAdminNormal & Copy & " (" & AgeInDays & " days)", "Oldest Visit", "", False, False, ""))
                '
                ' ----- Viewings Found
                '
                PagesTotal = 0
                SQL = "SELECT Count(ID) as result  FROM ccViewings;"
                CSServers = cpCore.db.csOpenSql_rev("Default", SQL)
                If cpCore.db.csOk(CSServers) Then
                    PagesTotal = cpCore.db.csGetInteger(CSServers, "Result")
                End If
                Call cpCore.db.csClose(CSServers)
                Call Content.Add(Adminui.GetEditRow(SpanClassAdminNormal & PagesTotal, "Viewings Found", "", False, False, ""))
                '
                Call Content.Add(genericController.StartTableRow() & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>Options</b>" & kmaEndTableCell & kmaEndTableRow)
                '
                Caption = "Archive Age"
                Copy = cpCore.html.html_GetFormInputText2("ArchiveRecordAgeDays", CStr(ArchiveRecordAgeDays), , 20) & "&nbsp;Number of days to keep visit records. 0 disables housekeeping."
                Call Content.Add(Adminui.GetEditRow(Copy, Caption))
                '
                Caption = "Housekeeping Time"
                Copy = cpCore.html.html_GetFormInputText2("ArchiveTimeOfDay", ArchiveTimeOfDay, , 20) & "&nbsp;The time of day when record deleting should start."
                Call Content.Add(Adminui.GetEditRow(Copy, Caption))
                '
                Caption = "Purge Content Files"
                Copy = cpCore.html.html_GetFormInputCheckBox2("ArchiveAllowFileClean", ArchiveAllowFileClean) & "&nbsp;Delete Contensive content files with no associated database record."
                Call Content.Add(Adminui.GetEditRow(Copy, Caption))
                '
                Content.Add(Adminui.EditTableClose)
                Content.Add(cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminformHousekeepingControl))
                ButtonList = ButtonCancel & ",Refresh," & ButtonSave & "," & ButtonOK
            End If
            '
            Caption = "Data Housekeeping Control"
            Description = "This tool is used to control the database record housekeeping process. This process deletes visit history records, so care should be taken before making any changes."
            GetForm_HouseKeepingControl = Adminui.GetBody(Caption, ButtonList, "", False, False, Description, "", 0, Content.Text)
            '
            Call cpCore.html.addTitle(Caption)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_HouseKeepingControl")
            '
        End Function
        ''
        ''
        ''
        'Private Function GetPropertyControl(Name As String, FieldType as integer, ProcessRequest As Boolean, DefaultValue As String) As String
        '    On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.GetPropertyControl")
        '    '
        '    Dim CurrentValue As String
        '    '
        '    If ProcessRequest Then
        '        CurrentValue = cpCore.main_GetStreamText2(Name)
        '        Call cpCore.app.setSiteProperty(Name, CurrentValue)
        '    Else
        '        CurrentValue = cpCore.main_GetSiteProperty2(Name, DefaultValue)
        '    End If
        '    Select Case FieldType
        '        Case FieldTypeBoolean
        '            GetPropertyControl = cpCore.main_GetFormInputCheckBox2(Name, genericController.EncodeBoolean(CurrentValue))
        '        Case Else
        '            GetPropertyControl = cpCore.main_GetFormInputText2(Name, CurrentValue)
        '    End Select
        '    '''Dim th as integer: Exit Function
        '    '
        '    ' ----- Error Trap
        '    '
        'ErrorTrap:
        '    Call HandleClassTrapErrorBubble("GetPropertyControl")
        'End Function
        '
        '
        '
        Private Function GetPropertyHTMLControl(ByVal Name As String, ByVal ProcessRequest As Boolean, ByVal DefaultValue As String, ByVal readOnlyField As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetPropertyHTMLControl")
            '
            Dim CurrentValue As String
            '
            If readOnlyField Then
                GetPropertyHTMLControl = "<div style=""border:1px solid #808080; padding:20px;"">" & genericController.decodeHtml(cpCore.siteProperties.getText(Name, DefaultValue)) & "</div>"
            ElseIf ProcessRequest Then
                CurrentValue = cpCore.docProperties.getText(Name)
                Call cpCore.siteProperties.setProperty(Name, CurrentValue)
                GetPropertyHTMLControl = cpCore.html.getFormInputHTML(Name, CurrentValue)
            Else
                CurrentValue = cpCore.siteProperties.getText(Name, DefaultValue)
                GetPropertyHTMLControl = cpCore.html.getFormInputHTML(Name, CurrentValue)
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetPropertyControl")
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Function admin_GetForm_StyleEditor() As String
            Return "<div><p>Site Styles are not longer supported. Instead add your styles to addons and add them with template dependencies.</p></div>"
            '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_StyleEditor")
            '            '
            '            Dim Content As New stringBuilderLegacyController
            '            Dim Button As String
            '            Dim Copy As String
            '            Dim ButtonList As String = ""
            '            Dim Adminui As New adminUIController(cpCore)
            '            Dim Caption As String
            '            Dim Description As String
            '            'Dim StyleSN as integer
            '            Dim AllowCSSReset As Boolean
            '            '
            '            Button = cpCore.docProperties.getText(RequestNameButton)
            '            If Button = ButtonCancel Then
            '                '
            '                '
            '                '
            '                Call cpCore.webServer.redirect("/" & cpCore.serverconfig.appconfig.adminRoute, "StyleEditor, Cancel Button Pressed", False)
            '            ElseIf Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
            '                '
            '                '
            '                '
            '                ButtonList = ButtonCancel
            '                Content.Add(Adminui.GetFormBodyAdminOnly())
            '            Else
            '                'StyleSN = genericController.EncodeInteger(cpCore.main_GetSiteProperty2("StylesheetSerialNumber", false ))
            '                AllowCSSReset = False
            '                If True Then ' 4.1.101" Then
            '                    AllowCSSReset = (cpCore.siteProperties.getBoolean("Allow CSS Reset", False))
            '                End If
            '                '
            '                Copy = cpCore.html.html_GetFormInputTextExpandable("StyleEditor", cpCore.cdnFiles.readFile(DynamicStylesFilename), 20)
            '                Copy = genericController.vbReplace(Copy, " cols=""100""", " style=""width:100%;""", 1, 99, vbTextCompare)
            '                Copy = "" _
            '                    & "<div style=""padding:10px;"">" & cpCore.html.html_GetFormInputCheckBox2(RequestNameAllowCSSReset, AllowCSSReset) & "&nbsp;Include Contensive reset styles</div>" _
            '                    & "<div style=""padding:10px;"">" & Copy & "</div>"

            '                '& "<div style=""padding:10px;"">" & cpCore.main_GetFormInputCheckBox2(RequestNameInlineStyles, (StyleSN = 0)) & "&nbsp;Force site styles inline</div>"

            '                Content.Add(Copy)
            '                ButtonList = ButtonCancel & "," & ButtonRefresh & "," & ButtonSave & "," & ButtonOK
            '                Content.Add(cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormStyleEditor))
            '            End If
            '            '
            '            Description = "" _
            '                & "<p>This tool is used to edit the site styles. When a public page is rendered, the head tag includes styles in this order:" _
            '                & "<ol>" _
            '                & "<li>Contensive reset styles (optional)</li>" _
            '                & "<li>Contensive styles</li>" _
            '                & "<li>These site styles (optionally inline)</li>" _
            '                & "<li>Shared styles from the template in use</li>" _
            '                & "<li>Exclusive styles from the template in use</li>" _
            '                & "<li>Add-on styles, first the default styles, then any custom styles included.</li>" _
            '                & "</ul>" _
            '                & ""
            '            admin_GetForm_StyleEditor = Adminui.GetBody("Site Styles", ButtonList, "", True, True, Description, "", 0, Content.Text)
            '            '
            '            Call cpCore.html.main_AddPagetitle("Style Editor")
            '            Exit Function
            '            '
            '            ' ----- Error Trap
            '            '
            'ErrorTrap:
            '            Call handleLegacyClassError3("GetForm_StyleEditor")
            '            '
        End Function
        '
        '
        '
        Private Function GetForm_ControlPage_CopyContent(ByVal Caption As String, ByVal CopyName As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_ControlPage_CopyContent")
            '
            Dim CS As Integer
            Dim RecordID As Integer
            Dim EditIcon As String
            Dim Copy As String = ""
            '
            Const ContentName = "Copy Content"
            '
            CS = cpCore.db.csOpen(ContentName, "Name=" & cpCore.db.encodeSQLText(CopyName))
            If cpCore.db.csOk(CS) Then
                RecordID = cpCore.db.csGetInteger(CS, "ID")
                Copy = cpCore.db.csGetText(CS, "copy")
            End If
            Call cpCore.db.csClose(CS)
            '
            If RecordID <> 0 Then
                EditIcon = "<a href=""?cid=" & Models.Complex.cdefModel.getContentId(cpCore, ContentName) & "&id=" & RecordID & "&" & RequestNameAdminForm & "=4"" target=_blank><img src=""/ccLib/images/IconContentEdit.gif"" border=0 alt=""Edit content"" valign=absmiddle></a>"
            Else
                EditIcon = "<a href=""?cid=" & Models.Complex.cdefModel.getContentId(cpCore, ContentName) & "&" & RequestNameAdminForm & "=4&" & RequestNameAdminAction & "=2&ad=1&wc=" & genericController.EncodeURL("name=" & CopyName) & """ target=_blank><img src=""/ccLib/images/IconContentEdit.gif"" border=0 alt=""Edit content"" valign=absmiddle></a>"
            End If
            If Copy = "" Then
                Copy = "&nbsp;"
            End If
            '
            GetForm_ControlPage_CopyContent = "" _
                & genericController.StartTable(4, 0, 1) & "<tr>" _
                & "<td width=150 align=right>" & Caption & "<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=150 height=1></td>" _
                & "<td width=20 align=center>" & EditIcon & "</td>" _
                & "<td>" & Copy & "&nbsp;</td>" _
                & "</tr></table>"
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_ControlPage_CopyContent")
            '
        End Function

    End Class
End Namespace
