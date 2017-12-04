
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
        ''
        ''========================================================================
        ''
        ''========================================================================
        ''
        'Private Function GetForm_EmailControl() As String
        '    On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.GetForm_EmailControl")
        '    '
        '    Dim Content As New fastStringClass
        '    Dim Copy As String
        '    Dim Button As String
        '    Dim ButtonList As String
        '    Dim SaveAction As Boolean
        '    Dim HelpCopy As String
        '    Dim FieldValue As String
        '    Dim PaymentProcessMethod as integer
        '    Dim Adminui As New adminUIclass(cpcore)
        '    Dim Description As String
        '    '
        '    if true then ' 3.3.009" Then
        '        SettingPageID = cpcore.htmldoc.main_GetRecordID_Internal("Setting Pages", "Email Settings")
        '    End If
        '    If SettingPageID <> 0 Then
        '        Call cpCore.htmldoc.main_AddRefreshQueryString(RequestNameOpenSettingPage, SettingPageID)
        '        GetForm_EmailControl = GetSettingPage(SettingPageID)
        '    Else
        '        Button = cpCore.main_GetStreamText2(RequestNameButton)
        '        If Button = ButtonCancel Then
        '            '
        '            '
        '            '
        '            Call cpCore.main_Redirect2(cpCore.app.SiteProperty_AdminURL, "Email Control, Cancel Button Pressed", False)
        '        ElseIf Not cpCore.main_IsAdmin Then
        '            '
        '            '
        '            '
        '            ButtonList = ButtonCancel
        '            Content.Add( AdminUI.GetFormBodyAdminOnly()
        '        Else
        '            '
        '            ' Process Requests
        '            '
        '            SaveAction = (Button = ButtonSave) Or (Button = ButtonOK)
        '            '
        '            ButtonList = ButtonCancel & "," & ButtonSave & "," & ButtonOK
        '            Content.Add( AdminUI.EditTableOpen)
        '            '
        '            ' Common email addresses
        '            '
        '            Call Content.Add(startTableRow & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>General Email Addresses</b>" & kmaEndTableCell & kmaEndTableRow)
        '            '
        '            HelpCopy = "This is the Email address displayed throughout the site when a visitor is prompted to contact the site administrator."
        '            Copy = (GetPropertyControl("EmailAdmin", FieldTypeText, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Admin Email Address", HelpCopy, False, False))
        '            '
        '            HelpCopy = "This is the Email address displayed throughout the site when a visitor is prompted to send site comments."
        '            Copy = (GetPropertyControl("EmailComments", FieldTypeText, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Comment Email Address", HelpCopy, False, False))
        '            '
        '            HelpCopy = "This is the Email address used on out-going Emails when no other address is available. For your Email to get to its destination, this Email address must be a valid Email account on a mail server."
        '            Copy = (GetPropertyControl("EmailFromAddress", FieldTypeText, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "General Email From Address", HelpCopy, False, False))
        '            '
        '            Call Content.Add(startTableRow & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>Trap Email Handling</b>" & kmaEndTableCell & kmaEndTableRow)
        '            '
        '            HelpCopy = "When checked, all system errors (called traps errors) generate an Email to the Trap Email address."
        '            Copy = (GetPropertyControl("AllowTrapemail", FieldTypeBoolean, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Allow Trap Error Email", HelpCopy, False, False))
        '            '
        '            HelpCopy = "This is the Email address to which all systems errors (called trap errors) are sent when Allow Trap Error Email is checked."
        '            Copy = (GetPropertyControl("TrapEmail", FieldTypeText, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Trap Error Email Address", HelpCopy, False, False))
        '            '
        '            ' Email Sending
        '            '
        '            Call Content.Add(startTableRow & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>Sending Email</b>" & kmaEndTableCell & kmaEndTableRow)
        '            '
        '            HelpCopy = "This is the domain name or IP address of the SMTP mail server you will use to send. If you are using the MS SMTP in IIS on this machine, use 127.0.0.1."
        '            Copy = (GetPropertyControl("SMTPServer", FieldTypeText, SaveAction, "127.0.0.1"))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "SMTP Email Server", HelpCopy, False, False))
        '            '
        '            HelpCopy = "When checked, the login box includes a section for users to enter their Email addresses. When submitted, all username and password matches for that Email address are sent to the Email address."
        '            Copy = (GetPropertyControl("AllowPasswordEmail", FieldTypeBoolean, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Allow Password Email", HelpCopy, False, False))
        '    '
        '    ' read-only - no longer user configurable
        '    '
        '    '        '
        '            HelpCopy = "This text is included at the bottom of each group, system, and conditional email. It contains a link that the Email recipient can click to block them from future emails from this site. Only site developers can modify this text."
        '            If cpCore.main_IsDeveloper Then
        '                HelpCopy = "<br><br>Developer: This text should conform to standards set by both local and federal law, as well as those required by your email server administrator. To create the clickable link, include link tags around your text (&lt%link&gt;click here&lt%/link&gt;). If you omit the link tag, a (click here) will be added to the end."
        '            End If
        '            Copy = (GetPropertyHTMLControl("EmailSpamFooter", SaveAction, DefaultSpamFooter, (Not cpCore.main_IsDeveloper)))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Email SpamFooter", HelpCopy, False, True))
        '            '
        '            HelpCopy = "Group and Conditional Email are delivered from another program that checks in about every minute. This is the time and date of the last check."
        '            Copy = cpCore.main_GetSiteProperty2("EmailServiceLastCheck")
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Last Send Email Status", HelpCopy, False, False))
        '            '
        '            ' Bounce Email Handling
        '            '
        '            Call Content.Add(startTableRow & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>Bounce Email Handling</b>" & kmaEndTableCell & kmaEndTableRow)
        '            '
        '            HelpCopy = "If present, all outbound Emails that can not be delivered will be returned to this address. This should be a valid Email address on an Email server."
        '            Copy = (GetPropertyControl("EmailBounceAddress", FieldTypeText, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Bounce Email Address", HelpCopy, False, False))
        '            '
        '            HelpCopy = "When checked, the system will attempt to retrieve bounced Emails from the following Email account and mark the members according to the processing rules included here."
        '            Copy = (GetPropertyControl("AllowEmailBounceProcessing", FieldTypeBoolean, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Process Bounced Emails", HelpCopy, False, False))
        '            '
        '            HelpCopy = "The POP Email server where Emails will be retrieved and processed."
        '            Copy = (GetPropertyControl("POPServer", FieldTypeText, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "POP Email Server", HelpCopy, False, False))
        '            '
        '            HelpCopy = "The account username to retrieve Emails for processing."
        '            Copy = (GetPropertyControl("POPServerUsername", FieldTypeText, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "POP Email Username", HelpCopy, False, False))
        '            '
        '            HelpCopy = "The account password to retrieve Emails for processing."
        '            Copy = (GetPropertyControl("POPServerPassword", FieldTypeText, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "POP Email Password", HelpCopy, False, False))
        '            '
        '            HelpCopy = "Set the action to be performed when an Email address is identified as invalid by the bounce process."
        '            If Not SaveAction Then
        '                FieldValue = genericController.EncodeInteger(cpCore.main_GetSiteProperty2("EMAILBOUNCEPROCESSACTION"))
        '            Else
        '                FieldValue = genericController.EncodeInteger(cpCore.main_GetStreamText2("EMAILBOUNCEPROCESSACTION"))
        '                Call cpCore.app.setSiteProperty("EMAILBOUNCEPROCESSACTION", FieldValue)
        '            End If
        '            Copy = "<select size=1 name=EMAILBOUNCEPROCESSACTION>" _
        '                & "<option value=0>Do Nothing</option>" _
        '                & "<option value=1>Clear the Allow Group Email field for all members with a matching Email address</option>" _
        '                & "<option value=2>Clear all member Email addresses that match the Email address</option>" _
        '                & "<option value=3>Delete all Members with a matching Email address</option>" _
        '                & "</select>"
        '            Copy = genericController.vbReplace(Copy, "value=" & FieldValue, "selected value=" & FieldValue)
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Bounce Email Action", HelpCopy, False, False))
        '            '
        '            HelpCopy = "Bounce emails are retrieved about every minute. This is the status of the last check."
        '            Copy = cpCore.main_GetSiteProperty2("POPServerStatus")
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Last Receive Email Status", HelpCopy, False, False))
        '            '
        '            Content.Add( AdminUI.EditTableClose)
        '            '
        '            ' Close form
        '            '
        '            If Button = ButtonOK Then
        '                Call cpCore.main_Redirect2(cpCore.app.SiteProperty_AdminURL, "EmailControl, OK Button Pressed", False)
        '                'Call cpCore.main_Redirect2(encodeAppRootPath(cpCore.main_GetSiteProperty2("AdminURL"), cpCore.main_ServerVirtualPath, cpCore.app.RootPath, cpCore.main_ServerHost))
        '            End If
        '            Content.Add( cpCore.main_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormEmailControl))
        '        End If
        '        '
        '        Description = "This tool is used to control the Contensive Email processes."
        '        GetForm_EmailControl = AdminUI.GetBody( "Email Control", ButtonList, "", True, True, Description, "", 0, Content.Text)
        '    End If
        '    '
        '    '''Dim th as integer: Exit Function
        '    '
        '    ' ----- Error Trap
        '    '
        'ErrorTrap:
        '    Call HandleClassTrapErrorBubble("GetForm_EmailControl")
        'End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Function GetForm_Downloads() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Downloads")
            '
            Dim IsEmptyList As Boolean
            Dim ResultMessage As String
            Dim LinkPrefix As String
            Dim LinkSuffix As String
            Dim RemoteKey As String
            Dim Copy As String
            Dim Button As String
            Dim ButtonPanel As String
            Dim SaveAction As Boolean
            Dim helpCopy As String
            Dim FieldValue As String
            Dim PaymentProcessMethod As Integer
            Dim Argument1 As String
            Dim CS As Integer
            Dim ContactGroupCriteria As String
            Dim GroupCount As Integer
            Dim GroupPointer As Integer
            Dim GroupChecked As Boolean
            Dim RecordName As String
            Dim ContentName As String
            Dim RecordID As Integer
            Dim RowEven As Boolean
            Dim SQL As String
            Dim RQS As String
            Dim SubTab As Integer
            Dim FormSave As Boolean
            Dim FormClear As Boolean
            Dim ContactContentID As Integer
            Dim Criteria As String
            Dim ContentGorupCriteria As String
            Dim ContactSearchCriteria As String
            Dim FieldParms() As String
            Dim CriteriaValues As Object
            Dim CriteriaCount As Integer
            Dim CriteriaPointer As Integer
            Dim PageSize As Integer
            Dim PageNumber As Integer
            Dim TopCount As Integer
            Dim RowPointer As Integer
            Dim DataRowCount As Integer
            Dim PreTableCopy As String = ""
            Dim PostTableCopy As String = ""
            Dim ColumnPtr As Integer
            Dim ColCaption() As String
            Dim ColAlign() As String
            Dim ColWidth() As String
            Dim Cells As String(,)
            Dim GroupID As Integer
            Dim GroupToolAction As Integer
            Dim ActionPanel As String
            Dim RowCount As Integer
            Dim GroupName As String
            Dim MemberID As Integer
            Dim QS As String
            Dim VisitsCell As String
            Dim VisitCount As Integer
            Dim AdminURL As String
            Dim CCID As Integer
            Dim SQLValue As String
            Dim DefaultName As String
            Dim SearchCaption As String
            Dim BlankPanel As String
            Dim RowPageSize As String
            Dim RowGroups As String
            Dim GroupIDs() As String
            Dim GroupPtr As Integer
            Dim GroupDelimiter As String
            Dim DateCompleted As Date
            Dim RowCnt As Integer
            Dim RowPtr As Integer
            Dim ContentID As Integer
            Dim Format As String
            Dim TableName As String
            Dim Filename As String
            Dim Name As String
            Dim Caption As String
            Dim Description As String = ""
            Dim ButtonListLeft As String
            Dim ButtonListRight As String
            Dim ContentPadding As Integer
            Dim ContentSummary As String = ""
            Dim Tab0 As New stringBuilderLegacyController
            Dim Tab1 As New stringBuilderLegacyController
            Dim Content As String = ""
            Dim Cell As String
            Dim Adminui As New adminUIController(cpCore)
            Dim SQLFieldName As String
            '
            Const ColumnCnt = 5
            '
            Button = cpCore.docProperties.getText(RequestNameButton)
            If Button = ButtonCancel Then
                Return cpCore.webServer.redirect("/" & cpCore.serverConfig.appConfig.adminRoute, "Downloads, Cancel Button Pressed")
            End If
            '
            If Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                '
                ' Must be a developer
                '
                ButtonListLeft = ButtonCancel
                ButtonListRight = ""
                Content = Content & Adminui.GetFormBodyAdminOnly()
            Else
                ContentID = cpCore.docProperties.getInteger("ContentID")
                Format = cpCore.docProperties.getText("Format")
                If False Then
                    SQLFieldName = "SQL"
                Else
                    SQLFieldName = "SQLQuery"
                End If
                '
                ' Process Requests
                '
                If Button <> "" Then
                    Select Case Button
                        Case ButtonDelete
                            RowCnt = cpCore.docProperties.getInteger("RowCnt")
                            If RowCnt > 0 Then
                                For RowPtr = 0 To RowCnt - 1
                                    If cpCore.docProperties.getBoolean("Row" & RowPtr) Then
                                        Call cpCore.db.deleteContentRecord("Tasks", cpCore.docProperties.getInteger("RowID" & RowPtr))
                                    End If
                                Next
                            End If
                        Case ButtonRequestDownload
                            '
                            ' Request the download again
                            '
                            RowCnt = cpCore.docProperties.getInteger("RowCnt")
                            If RowCnt > 0 Then
                                For RowPtr = 0 To RowCnt - 1
                                    If cpCore.docProperties.getBoolean("Row" & RowPtr) Then
                                        Dim CSSrc As Integer
                                        Dim CSDst As Integer

                                        CSSrc = cpCore.db.csOpenRecord("Tasks", cpCore.docProperties.getInteger("RowID" & RowPtr))
                                        If cpCore.db.csOk(CSSrc) Then
                                            CSDst = cpCore.db.csInsertRecord("Tasks")
                                            If cpCore.db.csOk(CSDst) Then
                                                Call cpCore.db.csSet(CSDst, "Name", cpCore.db.csGetText(CSSrc, "name"))
                                                Call cpCore.db.csSet(CSDst, SQLFieldName, cpCore.db.csGetText(CSSrc, SQLFieldName))
                                                If genericController.vbLCase(cpCore.db.csGetText(CSSrc, "command")) = "xml" Then
                                                    Call cpCore.db.csSet(CSDst, "Filename", "DupDownload_" & CStr(genericController.dateToSeconds(cpCore.doc.profileStartTime)) & CStr(genericController.GetRandomInteger()) & ".xml")
                                                    Call cpCore.db.csSet(CSDst, "Command", "BUILDXML")
                                                Else
                                                    Call cpCore.db.csSet(CSDst, "Filename", "DupDownload_" & CStr(genericController.dateToSeconds(cpCore.doc.profileStartTime)) & CStr(genericController.GetRandomInteger()) & ".csv")
                                                    Call cpCore.db.csSet(CSDst, "Command", "BUILDCSV")
                                                End If
                                            End If
                                            Call cpCore.db.csClose(CSDst)
                                        End If
                                        Call cpCore.db.csClose(CSSrc)
                                    End If
                                Next
                            End If
                            '
                            '
                            '
                            If (Format <> "") And (ContentID = 0) Then
                                Description = Description & "<p>Please select a Content before requesting a download</p>"
                            ElseIf (Format = "") And (ContentID <> 0) Then
                                Description = Description & "<p>Please select a Format before requesting a download</p>"
                            ElseIf Format = "CSV" Then
                                CS = cpCore.db.csInsertRecord("Tasks")
                                If cpCore.db.csOk(CS) Then
                                    ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID)
                                    TableName = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName)
                                    Criteria = Models.Complex.cdefModel.getContentControlCriteria(cpCore, ContentName)
                                    Name = "CSV Download, " & ContentName
                                    Filename = genericController.vbReplace(ContentName, " ", "") & "_" & CStr(genericController.dateToSeconds(cpCore.doc.profileStartTime)) & CStr(genericController.GetRandomInteger()) & ".csv"
                                    Call cpCore.db.csSet(CS, "Name", Name)
                                    Call cpCore.db.csSet(CS, "Filename", Filename)
                                    Call cpCore.db.csSet(CS, "Command", "BUILDCSV")
                                    Call cpCore.db.csSet(CS, SQLFieldName, "SELECT * from " & TableName & " where " & Criteria)
                                    Description = Description & "<p>Your CSV Download has been requested.</p>"
                                End If
                                Call cpCore.db.csClose(CS)
                                Format = ""
                                ContentID = 0
                            ElseIf Format = "XML" Then
                                CS = cpCore.db.csInsertRecord("Tasks")
                                If cpCore.db.csOk(CS) Then
                                    ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID)
                                    TableName = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName)
                                    Criteria = Models.Complex.cdefModel.getContentControlCriteria(cpCore, ContentName)
                                    Name = "XML Download, " & ContentName
                                    Filename = genericController.vbReplace(ContentName, " ", "") & "_" & CStr(genericController.dateToSeconds(cpCore.doc.profileStartTime)) & CStr(genericController.GetRandomInteger()) & ".xml"
                                    Call cpCore.db.csSet(CS, "Name", Name)
                                    Call cpCore.db.csSet(CS, "Filename", Filename)
                                    Call cpCore.db.csSet(CS, "Command", "BUILDXML")
                                    Call cpCore.db.csSet(CS, SQLFieldName, "SELECT * from " & TableName & " where " & Criteria)
                                    Description = Description & "<p>Your XML Download has been requested.</p>"
                                End If
                                Call cpCore.db.csClose(CS)
                                Format = ""
                                ContentID = 0
                            End If
                    End Select
                End If
                '
                ' Build Tab0
                '
                'Tab0.Add( "<p>The following is a list of available downloads</p>")
                ''
                RQS = cpCore.doc.refreshQueryString
                PageSize = cpCore.docProperties.getInteger(RequestNamePageSize)
                If PageSize = 0 Then
                    PageSize = 50
                End If
                PageNumber = cpCore.docProperties.getInteger(RequestNamePageNumber)
                If PageNumber = 0 Then
                    PageNumber = 1
                End If
                AdminURL = "/" & cpCore.serverConfig.appConfig.adminRoute
                TopCount = PageNumber * PageSize
                '
                ' Setup Headings
                '
                ReDim ColCaption(ColumnCnt)
                ReDim ColAlign(ColumnCnt)
                ReDim ColWidth(ColumnCnt)
                ReDim Cells(PageSize, ColumnCnt)
                '
                ColCaption(ColumnPtr) = "Select<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=10 height=1>"
                ColAlign(ColumnPtr) = "center"
                ColWidth(ColumnPtr) = "10"
                ColumnPtr = ColumnPtr + 1
                '
                ColCaption(ColumnPtr) = "Name"
                ColAlign(ColumnPtr) = "left"
                ColWidth(ColumnPtr) = "100%"
                ColumnPtr = ColumnPtr + 1
                '
                ColCaption(ColumnPtr) = "For<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=100 height=1>"
                ColAlign(ColumnPtr) = "left"
                ColWidth(ColumnPtr) = "100"
                ColumnPtr = ColumnPtr + 1
                '
                ColCaption(ColumnPtr) = "Requested<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=150 height=1>"
                ColAlign(ColumnPtr) = "left"
                ColWidth(ColumnPtr) = "150"
                ColumnPtr = ColumnPtr + 1
                '
                ColCaption(ColumnPtr) = "File<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=100 height=1>"
                ColAlign(ColumnPtr) = "Left"
                ColWidth(ColumnPtr) = "100"
                ColumnPtr = ColumnPtr + 1
                '
                '   Get Data
                '
                SQL = "select M.Name as CreatedByName, T.* from ccTasks T left join ccMembers M on M.ID=T.CreatedBy where (T.Command='BuildCSV')or(T.Command='BuildXML') order by T.DateAdded Desc"
                'Call cpCore.main_TestPoint("Selection SQL=" & SQL)
                CS = cpCore.db.csOpenSql_rev("default", SQL, PageSize, PageNumber)
                RowPointer = 0
                If Not cpCore.db.csOk(CS) Then
                    Cells(0, 1) = "There are no download requests"
                    RowPointer = 1
                Else
                    DataRowCount = cpCore.db.csGetRowCount(CS)
                    LinkPrefix = "<a href=""" & cpCore.serverConfig.appConfig.cdnFilesNetprefix
                    LinkSuffix = """ target=_blank>Available</a>"
                    Do While cpCore.db.csOk(CS) And (RowPointer < PageSize)
                        RecordID = cpCore.db.csGetInteger(CS, "ID")
                        DateCompleted = cpCore.db.csGetDate(CS, "DateCompleted")
                        ResultMessage = cpCore.db.csGetText(CS, "ResultMessage")
                        Cells(RowPointer, 0) = cpCore.html.html_GetFormInputCheckBox2("Row" & RowPointer) & cpCore.html.html_GetFormInputHidden("RowID" & RowPointer, RecordID)
                        Cells(RowPointer, 1) = cpCore.db.csGetText(CS, "name")
                        Cells(RowPointer, 2) = cpCore.db.csGetText(CS, "CreatedByName")
                        Cells(RowPointer, 3) = cpCore.db.csGetDate(CS, "DateAdded").ToShortDateString
                        If DateCompleted = Date.MinValue Then
                            RemoteKey = remoteQueryController.main_GetRemoteQueryKey(cpCore, "select DateCompleted,filename,resultMessage from cctasks where id=" & RecordID, "default", 1)
                            Cell = ""
                            Cell = Cell & vbCrLf & "<div id=""pending" & RowPointer & """>Pending <img src=""/ccLib/images/ajax-loader-small.gif"" width=16 height=16></div>"
                            '
                            Cell = Cell & vbCrLf & "<script>"
                            Cell = Cell & vbCrLf & "function statusHandler" & RowPointer & "(results) {"
                            Cell = Cell & vbCrLf & " var jo,isDone=false;"
                            Cell = Cell & vbCrLf & " eval('jo='+results);"
                            Cell = Cell & vbCrLf & " if (jo){"
                            Cell = Cell & vbCrLf & "  if(jo.DateCompleted) {"
                            Cell = Cell & vbCrLf & "    var dst=document.getElementById('pending" & RowPointer & "');"
                            Cell = Cell & vbCrLf & "    isDone=true;"
                            Cell = Cell & vbCrLf & "    if(jo.resultMessage=='OK') {"
                            Cell = Cell & vbCrLf & "      dst.innerHTML='" & LinkPrefix & "'+jo.filename+'" & LinkSuffix & "';"
                            Cell = Cell & vbCrLf & "    }else{"
                            Cell = Cell & vbCrLf & "      dst.innerHTML='error';"
                            Cell = Cell & vbCrLf & "    }"
                            Cell = Cell & vbCrLf & "  }"
                            Cell = Cell & vbCrLf & " }"
                            Cell = Cell & vbCrLf & " if(!isDone) setTimeout(""requestStatus" & RowPointer & "()"",5000)"
                            Cell = Cell & vbCrLf & "}"
                            '
                            Cell = Cell & vbCrLf & "function requestStatus" & RowPointer & "() {"
                            Cell = Cell & vbCrLf & "  cj.ajax.getNameValue(statusHandler" & RowPointer & ",'" & RemoteKey & "');"
                            Cell = Cell & vbCrLf & "}"
                            Cell = Cell & vbCrLf & "requestStatus" & RowPointer & "();"
                            Cell = Cell & vbCrLf & "</script>"
                            '
                            Cells(RowPointer, 4) = Cell
                        ElseIf ResultMessage = "ok" Then
                            Cells(RowPointer, 4) = "<div id=""pending" & RowPointer & """>" & LinkPrefix & cpCore.db.csGetText(CS, "filename") & LinkSuffix & "</div>"
                        Else
                            Cells(RowPointer, 4) = "<div id=""pending" & RowPointer & """><a href=""javascript:alert('" & genericController.EncodeJavascript(ResultMessage) & ";return false');"">error</a></div>"
                        End If
                        RowPointer = RowPointer + 1
                        Call cpCore.db.csGoNext(CS)
                    Loop
                End If
                Call cpCore.db.csClose(CS)
                Tab0.Add(cpCore.html.html_GetFormInputHidden("RowCnt", RowPointer))
                Cell = Adminui.GetReport(RowPointer, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, "ccPanel")
                Tab0.Add(Cell)
                'Tab0.Add( "<div style=""height:200px;"">" & Cell & "</div>"
                '        '
                '        ' Build RequestContent Form
                '        '
                '        Tab1.Add( "<p>Use this form to request a download. Select the criteria for the download and click the [Request Download] button. The request should then appear on the requested download list in the other tab. When the download has been created, it will be become available.</p>")
                '        '
                '        Tab1.Add( "<table border=""0"" cellpadding=""3"" cellspacing=""0"" width=""100%"">")
                '        '
                '        Call Tab1.Add("<tr>")
                '        Call Tab1.Add("<td align=right>Content</td>")
                '        Call Tab1.Add("<td>" & cpCore.htmldoc.main_GetFormInputSelect2("ContentID", ContentID, "Content", "", "", "", IsEmptyList) & "</td>")
                '        Call Tab1.Add("</tr>")
                '        '
                '        Call Tab1.Add("<tr>")
                '        Call Tab1.Add("<td align=right>Format</td>")
                '        Call Tab1.Add("<td><select name=Format value=""" & Format & """><option value=CSV>CSV</option><option name=XML value=XML>XML</option></select></td>")
                '        Call Tab1.Add("</tr>")
                '        '
                '        Call Tab1.Add("" _
                '            & "<tr>" _
                '            & "<td width=""120""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""120"" height=""1""></td>" _
                '            & "<td width=""100%"">&nbsp;</td>" _
                '            & "</tr>" _
                '            & "</table>")
                '        '
                '        ' Build and add tabs
                '        '
                '        Call cpCore.htmldoc.main_AddLiveTabEntry("Current&nbsp;Downloads", Tab0.Text, "ccAdminTab")
                '        Call cpCore.htmldoc.main_AddLiveTabEntry("Request&nbsp;New&nbsp;Download", Tab1.Text, "ccAdminTab")
                '        Content = cpCore.htmldoc.main_GetLiveTabs()
                Content = Tab0.Text
                '
                ButtonListLeft = ButtonCancel & "," & ButtonRefresh & "," & ButtonDelete
                'ButtonListLeft = ButtonCancel & "," & ButtonRefresh & "," & ButtonDelete & "," & ButtonRequestDownload
                ButtonListRight = ""
                Content = Content & cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormDownloads)
            End If
            '
            Caption = "Download Manager"
            Description = "" _
                & "<p>The Download Manager holds all downloads requested from anywhere on the website. It also provides tools to request downloads from any Content.</p>" _
                & "<p>To add a new download of any content in Contensive, click Export on the filter tab of the content listing page. To add a new download from a SQL statement, use Custom Reports under Reports on the Navigator.</p>"
            ContentPadding = 0
            GetForm_Downloads = Adminui.GetBody(Caption, ButtonListLeft, ButtonListRight, True, True, Description, ContentSummary, ContentPadding, Content)
            '
            Call cpCore.html.addTitle(Caption)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Downloads")
        End Function
        '
        '========================================================================
        '   Display field in the admin/edit
        '========================================================================
        '
        Private Function GetForm_Edit_AddTab(ByVal Caption As String, ByVal Content As String, ByVal AllowAdminTabs As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_AddTab")
            '
            If Content <> "" Then
                If Not AllowAdminTabs Then
                    GetForm_Edit_AddTab = Content
                Else
                    Call cpCore.html.menu_AddComboTabEntry(Replace(Caption, " ", "&nbsp;"), "", "", Content, False, "ccAdminTab")
                    'Call cpCore.htmldoc.main_AddLiveTabEntry(Replace(Caption, " ", "&nbsp;"), Content, "ccAdminTab")
                End If
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Edit_AddTab")
        End Function
        '
        '========================================================================
        '   Creates Tabbed content that is either Live (all content on page) or Ajax (click and ajax in the content)
        '========================================================================
        '
        Private Function GetForm_Edit_AddTab2(ByVal Caption As String, ByVal Content As String, ByVal AllowAdminTabs As Boolean, ByVal AjaxLink As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_AddTab2")
            '
            If Not AllowAdminTabs Then
                '
                ' non-tab mode
                '
                GetForm_Edit_AddTab2 = Content
            ElseIf AjaxLink <> "" Then
                '
                ' Ajax Tab
                '
                Call cpCore.html.menu_AddComboTabEntry(Replace(Caption, " ", "&nbsp;"), "", AjaxLink, "", False, "ccAdminTab")
            Else
                '
                ' Live Tab
                '
                Call cpCore.html.menu_AddComboTabEntry(Replace(Caption, " ", "&nbsp;"), "", "", Content, False, "ccAdminTab")
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Edit_AddTab2")
        End Function
        '
        '=============================================================================
        ' Create a child content
        '=============================================================================
        '
        Private Function GetForm_PageContentMap() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_PageContentMap")
            '
            GetForm_PageContentMap = "<p>The Page Content Map has been replaced with the Site Explorer, available as an Add-on through the Add-on Manager.</p>"
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_PageContentMap")
        End Function
        '
        '
        '
        Private Function GetForm_Edit_Tabs(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, ByVal readOnlyField As Boolean, ByVal IsLandingPage As Boolean, ByVal IsRootPage As Boolean, ByVal EditorContext As csv_contentTypeEnum, ByVal allowAjaxTabs As Boolean, ByVal TemplateIDForStyles As Integer, ByVal fieldTypeDefaultEditors As String(), ByVal fieldEditorPreferenceList As String, ByVal styleList As String, ByVal styleOptionList As String, ByVal emailIdForStyles As Integer, ByVal IsTemplateTable As Boolean, ByVal editorAddonListJSON As String) As String
            Dim returnHtml As String = ""
            Try
                '
                Dim tabContent As String
                Dim AjaxLink As String
                Dim TabsFound As New List(Of String)
                Dim editTabCaption As String
                Dim NewFormFieldList As String
                Dim FormFieldList As String
                Dim AllowHelpMsgCustom As Boolean
                Dim IDList As String
                Dim dt As DataTable
                Dim TempVar As String(,)
                Dim HelpCnt As Integer
                Dim fieldId As Integer
                Dim LastFieldID As Integer
                Dim HelpPtr As Integer
                Dim HelpIDCache() As Integer = {}
                Dim helpDefaultCache() As String = {}
                Dim HelpCustomCache() As String = {}
                Dim helpIdIndex As New keyPtrController
                Dim fieldNameLc As String
                '
                ' ----- read in help
                '
                IDList = ""
                For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In adminContent.fields
                    Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                    IDList = IDList & "," & field.id
                Next
                If IDList <> "" Then
                    IDList = Mid(IDList, 2)
                End If
                '
                dt = cpCore.db.executeQuery("select fieldid,helpdefault,helpcustom from ccfieldhelp where fieldid in (" & IDList & ") order by fieldid,id")
                TempVar = cpCore.db.convertDataTabletoArray(dt)
                If TempVar.GetLength(0) > 0 Then
                    HelpCnt = UBound(TempVar, 2) + 1
                    ReDim HelpIDCache(HelpCnt)
                    ReDim helpDefaultCache(HelpCnt)
                    ReDim HelpCustomCache(HelpCnt)
                    fieldId = -1
                    For HelpPtr = 0 To HelpCnt - 1
                        fieldId = genericController.EncodeInteger(TempVar(0, HelpPtr))
                        If fieldId <> LastFieldID Then
                            LastFieldID = fieldId
                            HelpIDCache(HelpPtr) = fieldId
                            Call helpIdIndex.setPtr(CStr(fieldId), HelpPtr)
                            helpDefaultCache(HelpPtr) = genericController.encodeText(TempVar(1, HelpPtr))
                            HelpCustomCache(HelpPtr) = genericController.encodeText(TempVar(2, HelpPtr))
                        End If
                    Next
                    AllowHelpMsgCustom = True
                End If
                '
                FormFieldList = ","
                For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In adminContent.fields
                    Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                    If (field.authorable) And (field.active) And (Not TabsFound.Contains(field.editTabName.ToLower())) Then
                        TabsFound.Add(field.editTabName.ToLower())
                        fieldNameLc = field.nameLc
                        editTabCaption = field.editTabName
                        If editTabCaption = "" Then
                            editTabCaption = "Details"
                        End If
                        NewFormFieldList = ""
                        If (Not allowAdminTabs) Or (Not allowAjaxTabs) Or (editTabCaption.ToLower() = "details") Then
                            '
                            ' Live Tab (non-tab mode, non-ajax mode, or details tab
                            '
                            tabContent = GetForm_Edit_Tab(adminContent, editRecord, editRecord.id, adminContent.Id, readOnlyField, IsLandingPage, IsRootPage, field.editTabName, EditorContext, NewFormFieldList, TemplateIDForStyles, HelpCnt, HelpIDCache, helpDefaultCache, HelpCustomCache, AllowHelpMsgCustom, helpIdIndex, fieldTypeDefaultEditors, fieldEditorPreferenceList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON)
                            If tabContent <> "" Then
                                returnHtml &= GetForm_Edit_AddTab2(editTabCaption, tabContent, allowAdminTabs, "")
                            End If
                        Else
                            '
                            ' Ajax Tab
                            '
                            'AjaxLink = "/admin/index.asp?"
                            AjaxLink = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" _
                            & RequestNameAjaxFunction & "=" & AjaxGetFormEditTabContent _
                            & "&ID=" & editRecord.id _
                            & "&CID=" & adminContent.Id _
                            & "&ReadOnly=" & readOnlyField _
                            & "&IsLandingPage=" & IsLandingPage _
                            & "&IsRootPage=" & IsRootPage _
                            & "&EditTab=" & genericController.EncodeRequestVariable(field.editTabName) _
                            & "&EditorContext=" & EditorContext _
                            & "&NewFormFieldList=" & genericController.EncodeRequestVariable(NewFormFieldList)
                            returnHtml &= GetForm_Edit_AddTab2(editTabCaption, "", True, AjaxLink)
                        End If
                        If NewFormFieldList <> "" Then
                            FormFieldList = NewFormFieldList & FormFieldList
                        End If
                    End If
                Next
                '
                ' ----- add the FormFieldList hidden - used on read to make sure all fields are returned
                '       this may not be needed, but we are having a problem with forms coming back without values
                '
                '
                ' moved this to GetEditTabContent - so one is added for each tab.
                '
                returnHtml &= cpCore.html.html_GetFormInputHidden("FormFieldList", FormFieldList)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnHtml
        End Function
        '        '
        '        ' Delete this when I can verify the Csvr patch to the instream process works
        '        '
        '        Private Sub VerifyDynamicMenuStyleSheet(ByVal MenuID As Integer)
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogAdminMethodEnter("VerifyDynamicMenuStyleSheet")
        '            '
        '            Dim StyleSN As String
        '            Dim EditTabCaption As String
        '            Dim ACTags() As String
        '            Dim TagPtr As Integer
        '            Dim QSPos As Integer
        '            Dim QSPosEnd As Integer
        '            Dim QS As String
        '            Dim MenuName As String
        '            Dim StylePrefix As String
        '            Dim CS As Integer
        '            Dim IsFound As Boolean
        '            Dim StyleSheet As String
        '            Dim DefaultStyles As String
        '            Dim DynamicStyles As String
        '            Dim AddStyles As String
        '            Dim StyleSplit() As String
        '            Dim StylePtr As Integer
        '            Dim StyleLine As String
        '            Dim Filename As String
        '            Dim NewStyleLine As String
        '            Dim TestSTyles As String

        '            '
        '            CS = cpCore.main_OpenCSContentRecord("Dynamic Menus", MenuID)
        '            If cpCore.app.IsCSOK(CS) Then
        '                StylePrefix = cpCore.db.cs_getText(CS, "StylePrefix")
        '                If StylePrefix <> "" And genericController.vbUCase(StylePrefix) <> "CCFLYOUT" Then
        '                    if true then ' 3.3.951" Then
        '                        TestSTyles = cpCore.app.cs_get(CS, "StylesFilename")
        '                    Else
        '                        TestSTyles = cpCore.main_GetStyleSheet
        '                    End If
        '                    If genericController.vbInstr(1, TestSTyles, "." & StylePrefix, vbTextCompare) = 0 Then
        '                        '
        '                        ' style not found, get the default ccFlyout styles
        '                        '
        '                        DefaultStyles = RemoveStyleTags(cpCore.cluster.programDataFiles.ReadFile("ccLib\" & "Styles\" & defaultStyleFilename))
        '                        'DefaultStyles = genericController.vbReplace(DefaultStyles, vbCrLf, " ")
        '                        Do While genericController.vbInstr(1, DefaultStyles, "  ") <> 0
        '                            DefaultStyles = genericController.vbReplace(DefaultStyles, "  ", " ")
        '                        Loop
        '                        StyleSplit = Split(DefaultStyles, "}")
        '                        For StylePtr = 0 To UBound(StyleSplit)
        '                            StyleLine = StyleSplit(StylePtr)
        '                            If StyleLine <> "" Then
        '                                If genericController.vbInstr(1, StyleLine, ".ccflyout", vbTextCompare) <> 0 Then
        '                                    StyleLine = genericController.vbReplace(StyleLine, vbCrLf, " ")
        '                                    StyleLine = genericController.vbReplace(StyleLine, ".ccflyout", "." & StylePrefix, vbTextCompare)
        '                                    Do While Left(StyleLine, 1) = " "
        '                                        StyleLine = Mid(StyleLine, 2)
        '                                    Loop
        '                                    AddStyles = AddStyles & StyleLine & "}" & vbCrLf
        '                                End If
        '                            End If
        '                        Next
        '                        If AddStyles <> "" Then
        '                            '
        '                            '
        '                            '
        '                            if true then ' 3.3.951" Then
        '                                '
        '                                ' Add new styles to the StylesFilename field
        '                                '
        '                                DynamicStyles = "" _
        '                                    & cpCore.app.cs_get(CS, "StylesFilename") _
        '                                    & vbCrLf & "" _
        '                                    & vbCrLf & "/* Menu Styles for Style Prefix [" & StylePrefix & "] created " & nt(cpCore.main_PageStartTime.toshortdateString & " */" _
        '                                    & vbCrLf & "" _
        '                                    & vbCrLf & AddStyles _
        '                                    & ""
        '                                Call cpCore.app.SetCS(CS, "StylesFilename", DynamicStyles)
        '                            Else
        '                                '
        '                                ' Legacy - add styles to the site stylesheet
        '                                '
        '                                Filename = cpCore.app.confxxxig.physicalFilePath & DynamicStylesFilename
        '                                DynamicStyles = RemoveStyleTags(cpCore.app.publicFiles.ReadFile(Filename)) & vbCrLf & AddStyles
        '                                Call cpCore.app.publicFiles.SaveFile(Filename, DynamicStyles)
        '                                '
        '                                ' Now create admin and public stylesheets from the styles.css styles
        '                                '
        '                                StyleSN = (cpCore.app.siteProperty_getInteger("StylesheetSerialNumber", "0"))
        '                                If StyleSN <> 0 Then
        '                                    ' mark to rebuild next fetch
        '                                    Call cpCore.app.siteProperty_set("StylesheetSerialNumber", "-1")
        '                                    '' Linked Styles
        '                                    '' Bump the Style Serial Number so next fetch is not cached
        '                                    ''
        '                                    'StyleSN = StyleSN + 1
        '                                    'Call cpCore.app.setSiteProperty("StylesheetSerialNumber", StyleSN)
        '                                    ''
        '                                    '' Save new public stylesheet
        '                                    ''
        '                                    '' 11/24/2009 - style sheet processing deprecated
        '                                    'Call cpCore.app.publicFiles.SaveFile("templates\Public" & StyleSN & ".css", cpCore.main_GetStyleSheet)
        '                                    ''Call cpCore.app.publicFiles.SaveFile("templates\Public" & StyleSN & ".css", cpCore.main_GetStyleSheetProcessed)
        '                                    'Call cpCore.app.publicFiles.SaveFile("templates\Admin" & StyleSN & ".css", cpCore.main_GetStyleSheetDefault)
        '                                End If
        '                            End If
        '                        End If
        '                    End If
        '                End If
        '            End If
        '            Call cpCore.app.closeCS(CS)
        '            '
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call handleLegacyClassError3("GetForm_Edit_UserFieldTabs")
        '        End Sub
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Function GetForm_CustomReports() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_CustomReports")
            '
            Dim Copy As String
            Dim Button As String
            Dim ButtonPanel As String
            Dim SaveAction As Boolean
            Dim helpCopy As String
            Dim FieldValue As String
            Dim PaymentProcessMethod As Integer
            Dim Argument1 As String
            Dim CS As Integer
            Dim ContactGroupCriteria As String
            Dim GroupCount As Integer
            Dim GroupPointer As Integer
            Dim GroupChecked As Boolean
            Dim RecordName As String
            Dim ContentName As String
            Dim RecordID As Integer
            Dim RowEven As Boolean
            Dim SQL As String
            Dim RQS As String
            Dim SubTab As Integer
            Dim FormSave As Boolean
            Dim FormClear As Boolean
            Dim ContactContentID As Integer
            Dim Criteria As String
            Dim ContentGorupCriteria As String
            Dim ContactSearchCriteria As String
            Dim FieldParms() As String
            Dim CriteriaValues As Object
            Dim CriteriaCount As Integer
            Dim CriteriaPointer As Integer
            Dim PageSize As Integer
            Dim PageNumber As Integer
            Dim TopCount As Integer
            Dim RowPointer As Integer
            Dim DataRowCount As Integer
            Dim PreTableCopy As String = ""
            Dim PostTableCopy As String = ""
            Dim ColumnPtr As Integer
            Dim ColCaption() As String
            Dim ColAlign() As String
            Dim ColWidth() As String
            Dim Cells As String(,)
            Dim GroupID As Integer
            Dim GroupToolAction As Integer
            Dim ActionPanel As String
            Dim RowCount As Integer
            Dim GroupName As String
            Dim MemberID As Integer
            Dim QS As String
            Dim VisitsCell As String
            Dim VisitCount As Integer
            Dim AdminURL As String
            Dim CCID As Integer
            Dim SQLValue As String
            Dim DefaultName As String
            Dim SearchCaption As String
            Dim BlankPanel As String
            Dim RowPageSize As String
            Dim RowGroups As String
            Dim GroupIDs() As String
            Dim GroupPtr As Integer
            Dim GroupDelimiter As String
            Dim DateCompleted As Date
            Dim RowCnt As Integer
            Dim RowPtr As Integer
            Dim ContentID As Integer
            Dim Format As String
            Dim TableName As String
            Dim Filename As String
            Dim Name As String
            Dim Caption As String
            Dim Description As String
            Dim ButtonListLeft As String
            Dim ButtonListRight As String
            Dim ContentPadding As Integer
            Dim ContentSummary As String = ""
            Dim Tab0 As New stringBuilderLegacyController
            Dim Tab1 As New stringBuilderLegacyController
            Dim Content As String = ""
            Dim SQLFieldName As String
            '
            Const ColumnCnt = 4
            '
            Button = cpCore.docProperties.getText(RequestNameButton)
            ContentID = cpCore.docProperties.getInteger("ContentID")
            Format = cpCore.docProperties.getText("Format")
            '
            Caption = "Custom Report Manager"
            Description = "Custom Reports are a way for you to create a snapshot of data to view or download. To request a report, select the Custom Reports tab, check the report(s) you want, and click the [Request Download] Button. When your report is ready, it will be available in the <a href=""?" & RequestNameAdminForm & "=30"">Download Manager</a>. To create a new custom report, select the Request New Report tab, enter a name and SQL statement, and click the Apply button."
            ContentPadding = 0
            ButtonListLeft = ButtonCancel & "," & ButtonDelete & "," & ButtonRequestDownload
            'ButtonListLeft = ButtonCancel & "," & ButtonDelete & "," & ButtonRequestDownload & "," & ButtonApply
            ButtonListRight = ""
            If False Then
                SQLFieldName = "SQL"
            Else
                SQLFieldName = "SQLQuery"
            End If
            '
            If Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                '
                ' Must be a developer
                '
                Description = Description & "You can not access the Custom Report Manager because your account is not configured as an administrator."
            Else
                '
                ' Process Requests
                '
                If Button <> "" Then
                    Select Case Button
                        Case ButtonCancel
                            Return cpCore.webServer.redirect("/" & cpCore.serverConfig.appConfig.adminRoute, "CustomReports, Cancel Button Pressed")
                            'Call cpCore.main_Redirect2(encodeAppRootPath(cpCore.main_GetSiteProperty2("AdminURL"), cpCore.main_ServerVirtualPath, cpCore.app.RootPath, cpCore.main_ServerHost))
                        Case ButtonDelete
                            RowCnt = cpCore.docProperties.getInteger("RowCnt")
                            If RowCnt > 0 Then
                                For RowPtr = 0 To RowCnt - 1
                                    If cpCore.docProperties.getBoolean("Row" & RowPtr) Then
                                        Call cpCore.db.deleteContentRecord("Custom Reports", cpCore.docProperties.getInteger("RowID" & RowPtr))
                                    End If
                                Next
                            End If
                        Case ButtonRequestDownload, ButtonApply
                            '
                            Name = cpCore.docProperties.getText("name")
                            SQL = cpCore.docProperties.getText(SQLFieldName)
                            If Name <> "" Or SQL <> "" Then
                                If (Name = "") Or (SQL = "") Then
                                    errorController.error_AddUserError(cpCore, "A name and SQL Query are required to save a new custom report.")
                                Else
                                    CS = cpCore.db.csInsertRecord("Custom Reports")
                                    If cpCore.db.csOk(CS) Then
                                        Call cpCore.db.csSet(CS, "Name", Name)
                                        Call cpCore.db.csSet(CS, SQLFieldName, SQL)
                                    End If
                                    Call cpCore.db.csClose(CS)
                                End If
                            End If
                            '
                            RowCnt = cpCore.docProperties.getInteger("RowCnt")
                            If RowCnt > 0 Then
                                For RowPtr = 0 To RowCnt - 1
                                    If cpCore.docProperties.getBoolean("Row" & RowPtr) Then
                                        RecordID = cpCore.docProperties.getInteger("RowID" & RowPtr)
                                        CS = cpCore.db.csOpenRecord("Custom Reports", RecordID)
                                        If cpCore.db.csOk(CS) Then
                                            SQL = cpCore.db.csGetText(CS, SQLFieldName)
                                            Name = cpCore.db.csGetText(CS, "Name")
                                        End If
                                        Call cpCore.db.csClose(CS)
                                        '
                                        CS = cpCore.db.csInsertRecord("Tasks")
                                        If cpCore.db.csOk(CS) Then
                                            RecordName = "CSV Download, Custom Report [" & Name & "]"
                                            Filename = "CustomReport_" & CStr(genericController.dateToSeconds(cpCore.doc.profileStartTime)) & CStr(genericController.GetRandomInteger()) & ".csv"
                                            Call cpCore.db.csSet(CS, "Name", RecordName)
                                            Call cpCore.db.csSet(CS, "Filename", Filename)
                                            If Format = "XML" Then
                                                Call cpCore.db.csSet(CS, "Command", "BUILDXML")
                                            Else
                                                Call cpCore.db.csSet(CS, "Command", "BUILDCSV")
                                            End If
                                            Call cpCore.db.csSet(CS, SQLFieldName, SQL)
                                            Description = Description & "<p>Your Download [" & Name & "] has been requested, and will be available in the <a href=""?" & RequestNameAdminForm & "=30"">Download Manager</a> when it is complete. This may take a few minutes depending on the size of the report.</p>"
                                        End If
                                        Call cpCore.db.csClose(CS)
                                    End If
                                Next
                            End If
                    End Select
                End If
                '
                ' Build Tab0
                '
                Tab0.Add("<p>The following is a list of available custom reports.</p>")
                '
                RQS = cpCore.doc.refreshQueryString
                PageSize = cpCore.docProperties.getInteger(RequestNamePageSize)
                If PageSize = 0 Then
                    PageSize = 50
                End If
                PageNumber = cpCore.docProperties.getInteger(RequestNamePageNumber)
                If PageNumber = 0 Then
                    PageNumber = 1
                End If
                AdminURL = "/" & cpCore.serverConfig.appConfig.adminRoute
                TopCount = PageNumber * PageSize
                '
                ' Setup Headings
                '
                ReDim ColCaption(ColumnCnt)
                ReDim ColAlign(ColumnCnt)
                ReDim ColWidth(ColumnCnt)
                ReDim Cells(PageSize, ColumnCnt)
                '
                ColCaption(ColumnPtr) = "Select<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=10 height=1>"
                ColAlign(ColumnPtr) = "center"
                ColWidth(ColumnPtr) = "10"
                ColumnPtr = ColumnPtr + 1
                '
                ColCaption(ColumnPtr) = "Name"
                ColAlign(ColumnPtr) = "left"
                ColWidth(ColumnPtr) = "100%"
                ColumnPtr = ColumnPtr + 1
                '
                ColCaption(ColumnPtr) = "Created By<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=100 height=1>"
                ColAlign(ColumnPtr) = "left"
                ColWidth(ColumnPtr) = "100"
                ColumnPtr = ColumnPtr + 1
                '
                ColCaption(ColumnPtr) = "Date Created<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=150 height=1>"
                ColAlign(ColumnPtr) = "left"
                ColWidth(ColumnPtr) = "150"
                ColumnPtr = ColumnPtr + 1
                ''
                'ColCaption(ColumnPtr) = "?<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=100 height=1>"
                'ColAlign(ColumnPtr) = "Left"
                'ColWidth(ColumnPtr) = "100"
                'ColumnPtr = ColumnPtr + 1
                '
                '   Get Data
                '
                CS = cpCore.db.csOpen("Custom Reports")
                RowPointer = 0
                If Not cpCore.db.csOk(CS) Then
                    Cells(0, 1) = "There are no custom reports defined"
                    RowPointer = 1
                Else
                    DataRowCount = cpCore.db.csGetRowCount(CS)
                    Do While cpCore.db.csOk(CS) And (RowPointer < PageSize)
                        RecordID = cpCore.db.csGetInteger(CS, "ID")
                        'DateCompleted = cpCore.db.cs_getDate(CS, "DateCompleted")
                        Cells(RowPointer, 0) = cpCore.html.html_GetFormInputCheckBox2("Row" & RowPointer) & cpCore.html.html_GetFormInputHidden("RowID" & RowPointer, RecordID)
                        Cells(RowPointer, 1) = cpCore.db.csGetText(CS, "name")
                        Cells(RowPointer, 2) = cpCore.db.csGet(CS, "CreatedBy")
                        Cells(RowPointer, 3) = cpCore.db.csGetDate(CS, "DateAdded").ToShortDateString
                        'Cells(RowPointer, 4) = "&nbsp;"
                        RowPointer = RowPointer + 1
                        Call cpCore.db.csGoNext(CS)
                    Loop
                End If
                Call cpCore.db.csClose(CS)
                Dim Cell As String
                Tab0.Add(cpCore.html.html_GetFormInputHidden("RowCnt", RowPointer))
                Dim Adminui As New adminUIController(cpCore)
                Cell = Adminui.GetReport(RowPointer, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, "ccPanel")
                Tab0.Add("<div>" & Cell & "</div>")
                '
                ' Build RequestContent Form
                '
                Tab1.Add("<p>Use this form to create a new custom report. Enter the SQL Query for the report, and a name that will be used as a caption.</p>")
                '
                Tab1.Add("<table border=""0"" cellpadding=""3"" cellspacing=""0"" width=""100%"">")
                '
                Call Tab1.Add("<tr>")
                Call Tab1.Add("<td align=right>Name</td>")
                Call Tab1.Add("<td>" & cpCore.html.html_GetFormInputText2("Name", "", 1, 40) & "</td>")
                Call Tab1.Add("</tr>")
                '
                Call Tab1.Add("<tr>")
                Call Tab1.Add("<td align=right>SQL Query</td>")
                Call Tab1.Add("<td>" & cpCore.html.html_GetFormInputText2(SQLFieldName, "", 8, 40) & "</td>")
                Call Tab1.Add("</tr>")
                '
                Call Tab1.Add("" _
                    & "<tr>" _
                    & "<td width=""120""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""120"" height=""1""></td>" _
                    & "<td width=""100%"">&nbsp;</td>" _
                    & "</tr>" _
                    & "</table>")
                '
                ' Build and add tabs
                '
                Call cpCore.html.main_AddLiveTabEntry("Custom&nbsp;Reports", Tab0.Text, "ccAdminTab")
                Call cpCore.html.main_AddLiveTabEntry("Request&nbsp;New&nbsp;Report", Tab1.Text, "ccAdminTab")
                Content = cpCore.html.main_GetLiveTabs()
                '
            End If
            '
            GetForm_CustomReports = admin_GetAdminFormBody(Caption, ButtonListLeft, ButtonListRight, True, True, Description, ContentSummary, ContentPadding, Content)
            '
            Call cpCore.html.addTitle("Custom Reports")
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_CustomReports")
        End Function
        '
        '========================================================================
        '   Print the index form, values and all
        '       creates a sql with leftjoins, and renames lookups as TableLookupxName
        '       where x is the TarGetFieldPtr of the field that is FieldTypeLookup
        '
        '   Input:
        '       AdminContent.contenttablename is required
        '       OrderByFieldPtr
        '       OrderByDirection
        '       RecordTop
        '       RecordsPerPage
        '       Findstring( ColumnPointer )
        '========================================================================
        '
        Private Function GetForm_Index(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, ByVal IsEmailContent As Boolean) As String
            Dim returnForm As String = ""
            Try
                Const FilterClosedLabel = "<div style=""font-size:9px;text-align:center;"">&nbsp;<br>F<br>i<br>l<br>t<br>e<br>r<br>s</div>"
                '
                Dim Copy As String = ""
                Dim RightCopy As String
                Dim TitleRows As Integer
                ' refactor -- is was using page manager code, and it only detected if the page is the current domain's 
                'Dim LandingPageID As Integer
                'Dim IsPageContent As Boolean
                'Dim IsLandingPage As Boolean
                Dim PageCount As Integer
                Dim AllowAdd As Boolean
                Dim AllowDelete As Boolean
                Dim recordCnt As Integer
                Dim AllowAccessToContent As Boolean
                Dim ContentName As String
                Dim ContentAccessLimitMessage As String = ""
                Dim IsLimitedToSubContent As Boolean
                Dim GroupList As String = ""
                Dim Groups() As String
                Dim FieldCaption As String
                Dim SubTitle As String
                Dim SubTitlePart As String
                Dim Title As String
                Dim AjaxQS As String
                Dim FilterColumn As String = ""
                Dim DataColumn As String
                Dim DataTable_DataRows As String = ""
                Dim FilterDataTable As String = ""
                Dim DataTable_FindRow As String = ""
                Dim DataTable As String
                Dim DataTable_HdrRow As String = ""
                Dim IndexFilterContent As String = ""
                Dim IndexFilterHead As String = ""
                Dim IndexFilterJS As String = ""
                Dim IndexFilterOpen As Boolean
                Dim IndexConfig As indexConfigClass
                Dim Ptr As Integer
                Dim SortTitle As String
                Dim HeaderDescription As String = ""
                Dim AllowFilterNav As Boolean
                Dim ColumnPointer As Integer
                Dim WhereCount As Integer
                Dim sqlWhere As String = ""
                Dim sqlOrderBy As String = ""
                Dim sqlFieldList As String = ""
                Dim sqlFrom As String = ""
                Dim CS As Integer
                Dim SQL As String
                Dim RowColor As String = ""
                Dim RecordPointer As Integer
                Dim RecordLast As Integer
                Dim RecordTop_NextPage As Integer
                Dim RecordTop_PreviousPage As Integer
                Dim ColumnWidth As Integer
                Dim ButtonBar As String
                Dim TitleBar As String
                Dim FindWordValue As String
                Dim ButtonObject As String
                Dim ButtonFace As String
                Dim ButtonHref As String
                Dim URI As String
                'Dim DataSourceName As String
                'Dim DataSourceType As Integer
                Dim FieldName As String
                Dim FieldUsedInColumns As New Dictionary(Of String, Boolean)                 ' used to prevent select SQL from being sorted by a field that does not appear
                Dim ColumnWidthTotal As Integer
                Dim SubForm As Integer
                Dim Stream As New stringBuilderLegacyController
                Dim RecordID As Integer
                Dim RecordName As String
                Dim LeftButtons As String = ""
                Dim RightButtons As String = ""
                Dim Adminui As New adminUIController(cpCore)
                Dim IsLookupFieldValid As New Dictionary(Of String, Boolean)
                Dim allowCMEdit As Boolean
                Dim allowCMAdd As Boolean
                Dim allowCMDelete As Boolean
                '
                ' --- make sure required fields are present
                '
                If adminContent.Id = 0 Then
                    '
                    ' Bad content id
                    '
                    Stream.Add(GetForm_Error(
                        "This form requires a valid content definition, and one was not found for content ID [" & adminContent.Id & "]." _
                        , "No content definition was specified [ContentID=0]. Please contact your application developer for more assistance."
                        ))
                ElseIf adminContent.Name = "" Then
                    '
                    ' Bad content name
                    '
                    Stream.Add(GetForm_Error(
                        "No content definition could be found for ContentID [" & adminContent.Id & "]. This could be a menu error. Please contact your application developer for more assistance." _
                        , "No content definition for ContentID [" & adminContent.Id & "] could be found."
                        ))
                ElseIf adminContent.ContentTableName = "" Then
                    '
                    ' No tablename
                    '
                    Stream.Add(GetForm_Error(
                        "The content definition [" & adminContent.Name & "] is not associated with a valid database table. Please contact your application developer for more assistance." _
                        , "Content [" & adminContent.Name & "] ContentTablename is empty."
                        ))
                ElseIf adminContent.fields.Count = 0 Then
                    '
                    ' No Fields
                    '
                    Stream.Add(GetForm_Error(
                        "This content [" & adminContent.Name & "] cannot be accessed because it has no fields. Please contact your application developer for more assistance." _
                        , "Content [" & adminContent.Name & "] has no field records."
                        ))
                ElseIf (adminContent.DeveloperOnly And (Not cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore))) Then
                    '
                    ' Developer Content and not developer
                    '
                    Stream.Add(GetForm_Error(
                        "Access to this content [" & adminContent.Name & "] requires developer permissions. Please contact your application developer for more assistance." _
                        , "Content [" & adminContent.Name & "] has no field records."
                        ))
                Else
                    Dim datasource As Models.Entity.dataSourceModel = Models.Entity.dataSourceModel.create(cpCore, adminContent.dataSourceId, New List(Of String))
                    '
                    ' get access rights
                    '
                    Call cpCore.doc.authContext.getContentAccessRights(cpCore, adminContent.Name, allowCMEdit, allowCMAdd, allowCMDelete)
                    '
                    ' detemine which subform to disaply
                    '
                    SubForm = cpCore.docProperties.getInteger(RequestNameAdminSubForm)
                    If SubForm <> 0 Then
                        Select Case SubForm
                            Case AdminFormIndex_SubFormExport
                                Copy = GetForm_Index_Export(adminContent, editRecord)
                            Case AdminFormIndex_SubFormSetColumns
                                Copy = GetForm_Index_SetColumns(adminContent, editRecord)
                            Case AdminFormIndex_SubFormAdvancedSearch
                                Copy = GetForm_Index_AdvancedSearch(adminContent, editRecord)
                        End Select
                    End If
                    Call Stream.Add(Copy)
                    If Copy = "" Then
                        '
                        ' If subforms return empty, go to parent form
                        '
                        AllowFilterNav = True
                        '
                        ' -- Load Index page customizations
                        IndexConfig = LoadIndexConfig(adminContent)
                        Call SetIndexSQL_ProcessIndexConfigRequests(adminContent, editRecord, IndexConfig)
                        Call SetIndexSQL_SaveIndexConfig(IndexConfig)
                        '
                        ' Get the SQL parts
                        '
                        Call SetIndexSQL(adminContent, editRecord, IndexConfig, AllowAccessToContent, sqlFieldList, sqlFrom, sqlWhere, sqlOrderBy, IsLimitedToSubContent, ContentAccessLimitMessage, FieldUsedInColumns, IsLookupFieldValid)
                        If (Not allowCMEdit) Or (Not AllowAccessToContent) Then
                            '
                            ' two conditions should be the same -- but not time to check - This user does not have access to this content
                            '
                            Call errorController.error_AddUserError(cpCore, "Your account does not have access to any records in '" & adminContent.Name & "'.")
                        Else
                            '
                            ' Get the total record count
                            '
                            SQL = "select count(" & adminContent.ContentTableName & ".ID) as cnt from " & sqlFrom
                            If sqlWhere <> "" Then
                                SQL &= " where " & sqlWhere
                            End If
                            CS = cpCore.db.csOpenSql_rev(datasource.Name, SQL)
                            If cpCore.db.csOk(CS) Then
                                recordCnt = cpCore.db.csGetInteger(CS, "cnt")
                            End If
                            Call cpCore.db.csClose(CS)
                            '
                            ' Assumble the SQL
                            '
                            SQL = "select"
                            If datasource.type <> DataSourceTypeODBCMySQL Then
                                SQL &= " Top " & (IndexConfig.RecordTop + IndexConfig.RecordsPerPage)
                            End If
                            SQL &= " " & sqlFieldList & " From " & sqlFrom
                            If sqlWhere <> "" Then
                                SQL &= " WHERE " & sqlWhere
                            End If
                            If sqlOrderBy <> "" Then
                                SQL &= " Order By" & sqlOrderBy
                            End If
                            If datasource.type = DataSourceTypeODBCMySQL Then
                                SQL &= " Limit " & (IndexConfig.RecordTop + IndexConfig.RecordsPerPage)
                            End If
                            '
                            ' Refresh Query String
                            '
                            Call cpCore.doc.addRefreshQueryString("tr", IndexConfig.RecordTop.ToString())
                            Call cpCore.doc.addRefreshQueryString("asf", AdminForm.ToString())
                            Call cpCore.doc.addRefreshQueryString("cid", adminContent.Id.ToString())
                            Call cpCore.doc.addRefreshQueryString(RequestNameTitleExtension, genericController.EncodeRequestVariable(TitleExtension))
                            If WherePairCount > 0 Then
                                For WhereCount = 0 To WherePairCount - 1
                                    Call cpCore.doc.addRefreshQueryString("wl" & WhereCount, WherePair(0, WhereCount))
                                    Call cpCore.doc.addRefreshQueryString("wr" & WhereCount, WherePair(1, WhereCount))
                                Next
                            End If
                            '
                            ' ----- ButtonBar
                            '
                            AllowAdd = adminContent.AllowAdd And (Not IsLimitedToSubContent) And (allowCMAdd)
                            If MenuDepth > 0 Then
                                LeftButtons = LeftButtons & cpCore.html.html_GetFormButton(ButtonClose, , , "window.close();")
                            Else
                                LeftButtons = LeftButtons & cpCore.html.html_GetFormButton(ButtonCancel)
                                'LeftButtons = LeftButtons & cpCore.main_GetFormButton(ButtonCancel, , , "return processSubmit(this)")
                            End If
                            If AllowAdd Then
                                LeftButtons = LeftButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonAdd & """>"
                                'LeftButtons = LeftButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonAdd & """ onClick=""return processSubmit(this);"">"
                            Else
                                LeftButtons = LeftButtons & "<input TYPE=SUBMIT NAME=BUTTON DISABLED VALUE=""" & ButtonAdd & """>"
                                'LeftButtons = LeftButtons & "<input TYPE=SUBMIT NAME=BUTTON DISABLED VALUE=""" & ButtonAdd & """ onClick=""return processSubmit(this);"">"
                            End If
                            AllowDelete = (adminContent.AllowDelete) And (allowCMDelete)
                            If AllowDelete Then
                                LeftButtons = LeftButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonDelete & """ onClick=""if(!DeleteCheck())return false;"">"
                            Else
                                LeftButtons = LeftButtons & "<input TYPE=SUBMIT NAME=BUTTON DISABLED VALUE=""" & ButtonDelete & """ onClick=""if(!DeleteCheck())return false;"">"
                            End If
                            If IndexConfig.PageNumber = 1 Then
                                RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonFirst & """ DISABLED>"
                                RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonPrevious & """ DISABLED>"
                            Else
                                RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonFirst & """>"
                                'RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonFirst & """ onClick=""return processSubmit(this);"">"
                                RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonPrevious & """>"
                                'RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonPrevious & """ onClick=""return processSubmit(this);"">"
                            End If
                            'RightButtons = RightButtons & cpCore.main_GetFormButton(ButtonFirst)
                            'RightButtons = RightButtons & cpCore.main_GetFormButton(ButtonPrevious)
                            If recordCnt > (IndexConfig.PageNumber * IndexConfig.RecordsPerPage) Then
                                RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonNext & """>"
                                'RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonNext & """ onClick=""return processSubmit(this);"">"
                            Else
                                RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonNext & """ DISABLED>"
                            End If
                            RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonRefresh & """>"
                            If recordCnt <= 1 Then
                                PageCount = 1
                            Else
                                PageCount = CInt(1 + Int((recordCnt - 1) / IndexConfig.RecordsPerPage))
                            End If
                            ButtonBar = Adminui.GetButtonBarForIndex(LeftButtons, RightButtons, IndexConfig.PageNumber, IndexConfig.RecordsPerPage, PageCount)
                            'ButtonBar = AdminUI.GetButtonBar(LeftButtons, RightButtons)
                            '
                            ' ----- TitleBar
                            '
                            Title = ""
                            SubTitle = ""
                            SubTitlePart = ""
                            With IndexConfig
                                If .ActiveOnly Then
                                    SubTitle = SubTitle & ", active records"
                                End If
                                SubTitlePart = ""
                                If .LastEditedByMe Then
                                    SubTitlePart = SubTitlePart & " by " & cpCore.doc.authContext.user.name
                                End If
                                If .LastEditedPast30Days Then
                                    SubTitlePart = SubTitlePart & " in the past 30 days"
                                End If
                                If .LastEditedPast7Days Then
                                    SubTitlePart = SubTitlePart & " in the week"
                                End If
                                If .LastEditedToday Then
                                    SubTitlePart = SubTitlePart & " today"
                                End If
                                If SubTitlePart <> "" Then
                                    SubTitle = SubTitle & ", last edited" & SubTitlePart
                                End If
                                For Each kvp In .FindWords
                                    Dim findWord As indexConfigFindWordClass = kvp.Value
                                    If Not String.IsNullOrEmpty(findWord.Name) Then
                                        FieldCaption = genericController.encodeText(Models.Complex.cdefModel.GetContentFieldProperty(cpCore, adminContent.Name, findWord.Name, "caption"))
                                        Select Case findWord.MatchOption
                                            Case FindWordMatchEnum.MatchEmpty
                                                SubTitle = SubTitle & ", " & FieldCaption & " is empty"
                                            Case FindWordMatchEnum.MatchEquals
                                                SubTitle = SubTitle & ", " & FieldCaption & " = '" & findWord.Value & "'"
                                            Case FindWordMatchEnum.MatchFalse
                                                SubTitle = SubTitle & ", " & FieldCaption & " is false"
                                            Case FindWordMatchEnum.MatchGreaterThan
                                                SubTitle = SubTitle & ", " & FieldCaption & " &gt; '" & findWord.Value & "'"
                                            Case FindWordMatchEnum.matchincludes
                                                SubTitle = SubTitle & ", " & FieldCaption & " includes '" & findWord.Value & "'"
                                            Case FindWordMatchEnum.MatchLessThan
                                                SubTitle = SubTitle & ", " & FieldCaption & " &lt; '" & findWord.Value & "'"
                                            Case FindWordMatchEnum.MatchNotEmpty
                                                SubTitle = SubTitle & ", " & FieldCaption & " is not empty"
                                            Case FindWordMatchEnum.MatchTrue
                                                SubTitle = SubTitle & ", " & FieldCaption & " is true"
                                        End Select

                                    End If
                                Next
                                If .SubCDefID > 0 Then
                                    ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, .SubCDefID)
                                    If ContentName <> "" Then
                                        SubTitle = SubTitle & ", in Sub-content '" & ContentName & "'"
                                    End If
                                End If
                                '
                                ' add groups to caption
                                '
                                If (LCase(adminContent.ContentTableName) = "ccmembers") And (.GroupListCnt > 0) Then
                                    'If (LCase(AdminContent.ContentTableName) = "ccmembers") And (.GroupListCnt > 0) Then
                                    SubTitlePart = ""
                                    For Ptr = 0 To .GroupListCnt - 1
                                        If .GroupList(Ptr) <> "" Then
                                            GroupList = GroupList & vbTab & .GroupList(Ptr)
                                        End If
                                    Next
                                    If GroupList <> "" Then
                                        Groups = Split(Mid(GroupList, 2), vbTab)
                                        If UBound(Groups) = 0 Then
                                            SubTitle = SubTitle & ", in group '" & Groups(0) & "'"
                                        ElseIf UBound(Groups) = 1 Then
                                            SubTitle = SubTitle & ", in groups '" & Groups(0) & "' and '" & Groups(1) & "'"
                                        Else
                                            For Ptr = 0 To UBound(Groups) - 1
                                                SubTitlePart = SubTitlePart & ", '" & Groups(Ptr) & "'"
                                            Next
                                            SubTitle = SubTitle & ", in groups" & Mid(SubTitlePart, 2) & " and '" & Groups(Ptr) & "'"
                                        End If

                                    End If
                                End If
                                '
                                ' add sort details to caption
                                '
                                SubTitlePart = ""
                                For Each kvp In .Sorts
                                    Dim sort As indexConfigSortClass = kvp.Value
                                    If (sort.direction > 0) Then
                                        SubTitlePart = SubTitlePart & " and " & adminContent.fields(sort.fieldName).caption
                                        If (sort.direction > 1) Then
                                            SubTitlePart &= " reverse"
                                        End If
                                    End If
                                Next
                                If SubTitlePart <> "" Then
                                    SubTitle &= ", sorted by" & Mid(SubTitlePart, 5)
                                End If
                            End With
                            '
                            Title = adminContent.Name
                            If TitleExtension <> "" Then
                                Title = Title & " " & TitleExtension
                            End If
                            Select Case recordCnt
                                Case 0
                                    RightCopy = "no records found"
                                Case 1
                                    RightCopy = "1 record found"
                                Case Else
                                    RightCopy = recordCnt & " records found"
                            End Select
                            RightCopy = RightCopy & ", page " & IndexConfig.PageNumber
                            Title = "<div>" _
                                & "<span style=""float:left;""><strong>" & Title & "</strong></span>" _
                                & "<span style=""float:right;"">" & RightCopy & "</span>" _
                                & "</div>"
                            TitleRows = 0
                            If SubTitle <> "" Then
                                Title = Title & "<div style=""clear:both"">Filter: " & genericController.encodeHTML(Mid(SubTitle, 3)) & "</div>"
                                TitleRows = TitleRows + 1
                            End If
                            If ContentAccessLimitMessage <> "" Then
                                Title = Title & "<div style=""clear:both"">" & ContentAccessLimitMessage & "</div>"
                                TitleRows = TitleRows + 1
                            End If
                            If TitleRows = 0 Then
                                Title = Title & "<div style=""clear:both"">&nbsp;</div>"
                            End If
                            '
                            TitleBar = SpanClassAdminNormal & Title & "</span>"
                            'TitleBar = TitleBar & cpCore.main_GetHelpLink(46, "Using the Admin Index Page", BubbleCopy_AdminIndexPage)
                            '
                            ' ----- Filter Data Table
                            '
                            If AllowFilterNav Then
                                '
                                ' Filter Nav - if enabled, just add another cell to the row
                                '
                                IndexFilterOpen = cpCore.visitProperty.getBoolean("IndexFilterOpen", False)
                                If IndexFilterOpen Then
                                    '
                                    ' Ajax Filter Open
                                    '
                                    IndexFilterHead = "" _
                                        & vbCrLf & "<div class=""ccHeaderCon"">" _
                                        & vbCrLf & "<div id=""IndexFilterHeCursorTypeEnum.ADOPENed"" class=""opened"">" _
                                        & cr & "<table border=0 cellpadding=0 cellspacing=0 width=""100%""><tr>" _
                                        & cr & "<td valign=Middle class=""left"">Filters</td>" _
                                        & cr & "<td valign=Middle class=""right""><a href=""#"" onClick=""CloseIndexFilter();return false""><img alt=""Close Filters"" title=""Close Filters"" src=""/ccLib/images/ClosexRev1313.gif"" width=13 height=13 border=0></a></td>" _
                                        & cr & "</tr></table>" _
                                        & vbCrLf & "</div>" _
                                        & vbCrLf & "<div id=""IndexFilterHeadClosed"" class=""closed"" style=""display:none;"">" _
                                        & cr & "<a href=""#"" onClick=""OpenIndexFilter();return false""><img title=""Open Navigator"" alt=""Open Filter"" src=""/ccLib/images/OpenRightRev1313.gif"" width=13 height=13 border=0 style=""text-align:right;""></a>" _
                                        & vbCrLf & "</div>" _
                                        & vbCrLf & "</div>" _
                                        & ""
                                    IndexFilterContent = "" _
                                        & vbCrLf & "<div class=""ccContentCon"">" _
                                        & vbCrLf & "<div id=""IndexFilterContentOpened"" class=""opened"">" & GetForm_IndexFilterContent(adminContent) & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""200"" height=""1"" style=""clear:both""></div>" _
                                        & vbCrLf & "<div id=""IndexFilterContentClosed"" class=""closed"" style=""display:none;"">" & FilterClosedLabel & "</div>" _
                                        & vbCrLf & "</div>"
                                    IndexFilterJS = "" _
                                        & vbCrLf & "<script Language=""JavaScript"" type=""text/javascript"">" _
                                        & vbCrLf & "function CloseIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','none');SetDisplay('IndexFilterContentOpened','none');SetDisplay('IndexFilterHeadClosed','block');SetDisplay('IndexFilterContentClosed','block');cj.ajax.qs('" & RequestNameAjaxFunction & "=" & AjaxCloseIndexFilter & "','','')}" _
                                        & vbCrLf & "function OpenIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','block');SetDisplay('IndexFilterContentOpened','block');SetDisplay('IndexFilterHeadClosed','none');SetDisplay('IndexFilterContentClosed','none');cj.ajax.qs('" & RequestNameAjaxFunction & "=" & AjaxOpenIndexFilter & "','','')}" _
                                        & vbCrLf & "</script>"
                                Else
                                    '
                                    ' Ajax Filter Closed
                                    '
                                    IndexFilterHead = "" _
                                        & vbCrLf & "<div class=""ccHeaderCon"">" _
                                        & vbCrLf & "<div id=""IndexFilterHeCursorTypeEnum.ADOPENed"" class=""opened"" style=""display:none;"">" _
                                        & cr & "<table border=0 cellpadding=0 cellspacing=0 width=""100%""><tr>" _
                                        & cr & "<td valign=Middle class=""left"">Filter</td>" _
                                        & cr & "<td valign=Middle class=""right""><a href=""#"" onClick=""CloseIndexFilter();return false""><img alt=""Close Filter"" title=""Close Navigator"" src=""/ccLib/images/ClosexRev1313.gif"" width=13 height=13 border=0></a></td>" _
                                        & cr & "</tr></table>" _
                                        & vbCrLf & "</div>" _
                                        & vbCrLf & "<div id=""IndexFilterHeadClosed"" class=""closed"">" _
                                        & cr & "<a href=""#"" onClick=""OpenIndexFilter();return false""><img title=""Open Navigator"" alt=""Open Navigator"" src=""/ccLib/images/OpenRightRev1313.gif"" width=13 height=13 border=0 style=""text-align:right;""></a>" _
                                        & vbCrLf & "</div>" _
                                        & vbCrLf & "</div>" _
                                        & ""
                                    IndexFilterContent = "" _
                                        & vbCrLf & "<div class=""ccContentCon"">" _
                                        & vbCrLf & "<div id=""IndexFilterContentOpened"" class=""opened"" style=""display:none;""><div style=""text-align:center;""><img src=""/ccLib/images/ajax-loader-small.gif"" width=16 height=16></div></div>" _
                                        & vbCrLf & "<div id=""IndexFilterContentClosed"" class=""closed"">" & FilterClosedLabel & "</div>" _
                                        & vbCrLf & "<div id=""IndexFilterContentMinWidth"" style=""display:none;""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""200"" height=""1"" style=""clear:both""></div>" _
                                        & vbCrLf & "</div>"
                                    AjaxQS = cpCore.doc.refreshQueryString
                                    AjaxQS = genericController.ModifyQueryString(AjaxQS, RequestNameAjaxFunction, AjaxOpenIndexFilterGetContent)
                                    IndexFilterJS = "" _
                                        & vbCrLf & "<script Language=""JavaScript"" type=""text/javascript"">" _
                                        & vbCrLf & "var IndexFilterPop=false;" _
                                        & vbCrLf & "function CloseIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','none');SetDisplay('IndexFilterHeadClosed','block');SetDisplay('IndexFilterContentOpened','none');SetDisplay('IndexFilterContentMinWidth','none');SetDisplay('IndexFilterContentClosed','block');cj.ajax.qs('" & RequestNameAjaxFunction & "=" & AjaxCloseIndexFilter & "','','')}" _
                                        & vbCrLf & "function OpenIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','block');SetDisplay('IndexFilterHeadClosed','none');SetDisplay('IndexFilterContentOpened','block');SetDisplay('IndexFilterContentMinWidth','block');SetDisplay('IndexFilterContentClosed','none');if(!IndexFilterPop){cj.ajax.qs('" & AjaxQS & "','','IndexFilterContentOpened');IndexFilterPop=true;}else{cj.ajax.qs('" & RequestNameAjaxFunction & "=" & AjaxOpenIndexFilter & "','','');}}" _
                                        & vbCrLf & "</script>"
                                End If
                            End If
                            '
                            ' Dual Window Right - Data
                            '
                            FilterDataTable &= "<td valign=top class=""ccPanel"">"
                            '
                            DataTable_HdrRow &= "<tr>"
                            '
                            ' Row Number Column
                            '
                            DataTable_HdrRow &= "<td width=20 align=center valign=bottom class=""ccAdminListCaption"">Row</td>"
                            '
                            ' Delete Select Box Columns
                            '
                            If Not AllowDelete Then
                                DataTable_HdrRow &= "<td width=20 align=center valign=bottom class=""ccAdminListCaption""><input TYPE=CheckBox disabled=""disabled""></td>"
                            Else
                                DataTable_HdrRow &= "<td width=20 align=center valign=bottom class=""ccAdminListCaption""><input TYPE=CheckBox OnClick=""CheckInputs('DelCheck',this.checked);""></td>"
                            End If
                            '
                            ' Calculate total width
                            '
                            ColumnWidthTotal = 0
                            For Each kvp In IndexConfig.Columns
                                Dim column As indexConfigColumnClass = kvp.Value
                                If column.Width < 1 Then
                                    column.Width = 1
                                End If
                                ColumnWidthTotal = ColumnWidthTotal + column.Width
                            Next
                            '
                            ' Edit Column
                            '
                            DataTable_HdrRow &= "<td width=20 align=center valign=bottom class=""ccAdminListCaption"">Edit</td>"
                            For Each kvp In IndexConfig.Columns
                                Dim column As indexConfigColumnClass = kvp.Value
                                '
                                ' ----- print column headers - anchored so they sort columns
                                '
                                ColumnWidth = CInt((100 * column.Width) / ColumnWidthTotal)
                                'fieldId = column.FieldId
                                FieldName = column.Name
                                '
                                'if this is a current sort ,add the reverse flag
                                '
                                ButtonHref = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & RequestNameAdminForm & "=" & AdminFormIndex & "&SetSortField=" & FieldName & "&RT=0&" & RequestNameTitleExtension & "=" & genericController.EncodeRequestVariable(TitleExtension) & "&cid=" & adminContent.Id & "&ad=" & MenuDepth
                                For Each sortKvp In IndexConfig.Sorts
                                    Dim sort As indexConfigSortClass = sortKvp.Value

                                Next
                                If Not IndexConfig.Sorts.ContainsKey(FieldName) Then
                                    ButtonHref &= "&SetSortDirection=1"
                                Else
                                    Select Case IndexConfig.Sorts(FieldName).direction
                                        Case 1
                                            ButtonHref &= "&SetSortDirection=2"
                                        Case 2
                                            ButtonHref &= "&SetSortDirection=0"
                                        Case Else
                                    End Select
                                End If
                                '
                                '----- column header includes WherePairCount
                                '
                                If WherePairCount > 0 Then
                                    For WhereCount = 0 To WherePairCount - 1
                                        If WherePair(0, WhereCount) <> "" Then
                                            ButtonHref &= "&wl" & WhereCount & "=" & genericController.EncodeRequestVariable(WherePair(0, WhereCount))
                                            ButtonHref &= "&wr" & WhereCount & "=" & genericController.EncodeRequestVariable(WherePair(1, WhereCount))
                                        End If
                                    Next
                                End If
                                ButtonFace = adminContent.fields(FieldName.ToLower()).caption
                                ButtonFace = genericController.vbReplace(ButtonFace, " ", "&nbsp;")
                                SortTitle = "Sort A-Z"
                                '
                                If IndexConfig.Sorts.ContainsKey(FieldName) Then
                                    Select Case IndexConfig.Sorts(FieldName).direction
                                        Case 1
                                            ButtonFace = ButtonFace & "<img src=""/ccLib/images/arrowdown.gif"" width=8 height=8 border=0>"
                                            SortTitle = "Sort Z-A"
                                        Case 2
                                            ButtonFace = ButtonFace & "<img src=""/ccLib/images/arrowup.gif"" width=8 height=8 border=0>"
                                            SortTitle = "Remove Sort"
                                        Case Else
                                    End Select
                                End If
                                ButtonObject = "Button" & ButtonObjectCount
                                ButtonObjectCount = ButtonObjectCount + 1
                                DataTable_HdrRow &= "<td width=""" & ColumnWidth & "%"" valign=bottom align=left class=""ccAdminListCaption"">"
                                DataTable_HdrRow &= ("<a title=""" & SortTitle & """ href=""" & genericController.encodeHTML(ButtonHref) & """ class=""ccAdminListCaption"">" & ButtonFace & "</A>")
                                DataTable_HdrRow &= ("</td>")
                            Next
                            DataTable_HdrRow &= ("</tr>")
                            '
                            '   select and print Records
                            '
                            'DataSourceName = cpCore.db.getDataSourceNameByID(adminContent.dataSourceId)
                            CS = cpCore.db.csOpenSql(SQL, datasource.Name, IndexConfig.RecordsPerPage, IndexConfig.PageNumber)
                            If cpCore.db.csOk(CS) Then
                                RowColor = ""
                                RecordPointer = IndexConfig.RecordTop
                                RecordLast = IndexConfig.RecordTop + IndexConfig.RecordsPerPage
                                '
                                ' --- Print out the records
                                '
                                'IsPageContent = (LCase(adminContent.ContentTableName) = "ccpagecontent")
                                'If IsPageContent Then
                                '    LandingPageID = cpCore.main_GetLandingPageID
                                'End If
                                Do While ((cpCore.db.csOk(CS)) And (RecordPointer < RecordLast))
                                    RecordID = cpCore.db.csGetInteger(CS, "ID")
                                    RecordName = cpCore.db.csGetText(CS, "name")
                                    'IsLandingPage = IsPageContent And (RecordID = LandingPageID)
                                    If RowColor = "class=""ccAdminListRowOdd""" Then
                                        RowColor = "class=""ccAdminListRowEven"""
                                    Else
                                        RowColor = "class=""ccAdminListRowOdd"""
                                    End If
                                    DataTable_DataRows &= vbCrLf & "<tr>"
                                    '
                                    ' --- Record Number column
                                    '
                                    DataTable_DataRows &= "<td align=right " & RowColor & ">" & SpanClassAdminSmall & "[" & RecordPointer + 1 & "]</span></td>"
                                    '
                                    ' --- Delete Checkbox Columns
                                    '
                                    If AllowDelete Then
                                        'If AllowDelete And Not IsLandingPage Then
                                        'If AdminContent.AllowDelete And Not IsLandingPage Then
                                        DataTable_DataRows &= "<td align=center " & RowColor & "><input TYPE=CheckBox NAME=row" & RecordPointer & " VALUE=1 ID=""DelCheck""><input type=hidden name=rowid" & RecordPointer & " VALUE=" & RecordID & "></span></td>"
                                    Else
                                        DataTable_DataRows &= "<td align=center " & RowColor & "><input TYPE=CheckBox disabled=""disabled"" NAME=row" & RecordPointer & " VALUE=1><input type=hidden name=rowid" & RecordPointer & " VALUE=" & RecordID & "></span></td>"
                                    End If
                                    '
                                    ' --- Edit button column
                                    '
                                    DataTable_DataRows &= "<td align=center " & RowColor & ">"
                                    URI = "\" & cpCore.serverConfig.appConfig.adminRoute _
                                        & "?" & RequestNameAdminAction & "=" & AdminActionNop _
                                        & "&cid=" & adminContent.Id _
                                        & "&id=" & RecordID _
                                        & "&" & RequestNameTitleExtension & "=" & genericController.EncodeRequestVariable(TitleExtension) _
                                        & "&ad=" & MenuDepth _
                                        & "&" & RequestNameAdminSourceForm & "=" & AdminForm _
                                        & "&" & RequestNameAdminForm & "=" & AdminFormEdit
                                    If WherePairCount > 0 Then
                                        For WhereCount = 0 To WherePairCount - 1
                                            URI = URI & "&wl" & WhereCount & "=" & genericController.EncodeRequestVariable(WherePair(0, WhereCount)) & "&wr" & WhereCount & "=" & genericController.EncodeRequestVariable(WherePair(1, WhereCount))
                                        Next
                                    End If
                                    DataTable_DataRows &= ("<a href=""" & genericController.encodeHTML(URI) & """><img src=""/ccLib/images/IconContentEdit.gif"" border=""0""></a>")
                                    DataTable_DataRows &= ("</td>")
                                    '
                                    ' --- field columns
                                    '
                                    For Each columnKvp In IndexConfig.Columns
                                        Dim column As indexConfigColumnClass = columnKvp.Value
                                        Dim columnNameLc As String = column.Name.ToLower()
                                        If FieldUsedInColumns.ContainsKey(columnNameLc) Then
                                            If FieldUsedInColumns.Item(columnNameLc) Then
                                                DataTable_DataRows &= (vbCrLf & "<td valign=""middle"" " & RowColor & " align=""left"">" & SpanClassAdminNormal)
                                                DataTable_DataRows &= GetForm_Index_GetCell(adminContent, editRecord, column.Name, CS, IsLookupFieldValid(columnNameLc), genericController.vbLCase(adminContent.ContentTableName) = "ccemail")
                                                DataTable_DataRows &= ("&nbsp;</span></td>")
                                            End If
                                        End If
                                    Next
                                    DataTable_DataRows &= (vbLf & "    </tr>")
                                    Call cpCore.db.csGoNext(CS)
                                    RecordPointer = RecordPointer + 1
                                Loop
                                DataTable_DataRows &= "<input type=hidden name=rowcnt value=" & RecordPointer & ">"
                                '
                                ' --- print out the stuff at the bottom
                                '
                                RecordTop_NextPage = IndexConfig.RecordTop
                                If cpCore.db.csOk(CS) Then
                                    RecordTop_NextPage = RecordPointer
                                End If
                                RecordTop_PreviousPage = IndexConfig.RecordTop - IndexConfig.RecordsPerPage
                                If RecordTop_PreviousPage < 0 Then
                                    RecordTop_PreviousPage = 0
                                End If
                            End If
                            Call cpCore.db.csClose(CS)
                            '
                            ' Header at bottom
                            '
                            If RowColor = "class=""ccAdminListRowOdd""" Then
                                RowColor = "class=""ccAdminListRowEven"""
                            Else
                                RowColor = "class=""ccAdminListRowOdd"""
                            End If
                            If (RecordPointer = 0) Then
                                '
                                ' No records found
                                '
                                DataTable_DataRows &= ("<tr>" _
                                    & "<td " & RowColor & " align=center>-</td>" _
                                    & "<td " & RowColor & " align=center>-</td>" _
                                    & "<td " & RowColor & " align=center>-</td>" _
                                    & "<td colspan=" & IndexConfig.Columns.Count & " " & RowColor & " style=""text-align:left ! important;"">no records were found</td>" _
                                    & "</tr>")
                            Else
                                If (RecordPointer < RecordLast) Then
                                    '
                                    ' End of list
                                    '
                                    DataTable_DataRows &= ("<tr>" _
                                        & "<td " & RowColor & " align=center>-</td>" _
                                        & "<td " & RowColor & " align=center>-</td>" _
                                        & "<td " & RowColor & " align=center>-</td>" _
                                        & "<td colspan=" & IndexConfig.Columns.Count & " " & RowColor & " style=""text-align:left ! important;"">----- end of list</td>" _
                                        & "</tr>")
                                End If
                                '
                                ' Add another header to the data rows
                                '
                                DataTable_DataRows &= DataTable_HdrRow
                            End If
                            ''
                            '' ----- DataTable_FindRow
                            ''
                            'ReDim Findstring(IndexConfig.Columns.Count)
                            'For ColumnPointer = 0 To IndexConfig.Columns.Count - 1
                            '    FieldName = IndexConfig.Columns(ColumnPointer).Name
                            '    If genericController.vbLCase(FieldName) = FindWordName Then
                            '        Findstring(ColumnPointer) = FindWordValue
                            '    End If
                            'Next
                            '        ReDim Findstring(CustomAdminColumnCount)
                            '        For ColumnPointer = 0 To CustomAdminColumnCount - 1
                            '            FieldPtr = CustomAdminColumn(ColumnPointer).FieldPointer
                            '            With AdminContent.fields(FieldPtr)
                            '                If genericController.vbLCase(.Name) = FindWordName Then
                            '                    Findstring(ColumnPointer) = FindWordValue
                            '                End If
                            '            End With
                            '        Next
                            '
                            DataTable_FindRow = DataTable_FindRow & "<tr><td colspan=" & (3 + IndexConfig.Columns.Count) & " style=""background-color:black;height:1;""></td></tr>"
                            'DataTable_FindRow = DataTable_FindRow & "<tr><td colspan=" & (3 + CustomAdminColumnCount) & " style=""background-color:black;height:1;""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td></tr>"
                            DataTable_FindRow = DataTable_FindRow & "<tr>"
                            DataTable_FindRow = DataTable_FindRow & "<td colspan=3 width=""60"" class=""ccPanel"" align=center style=""text-align:center ! important;"">"
                            DataTable_FindRow = DataTable_FindRow _
                                & vbCrLf & "<script language=""javascript"" type=""text/javascript"">" _
                                & vbCrLf & "function KeyCheck(e){" _
                                & vbCrLf & "  var code = e.keyCode;" _
                                & vbCrLf & "  if(code==13){" _
                                & vbCrLf & "    document.getElementById('FindButton').focus();" _
                                & vbCrLf & "    document.getElementById('FindButton').click();" _
                                & vbCrLf & "    return false;" _
                                & vbCrLf & "  }" _
                                & vbCrLf & "} " _
                                & vbCrLf & "</script>"
                            DataTable_FindRow = DataTable_FindRow & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""60"" height=""1"" ><br >" & cpCore.html.html_GetFormButton(ButtonFind, , "FindButton") & "</td>"
                            ColumnPointer = 0
                            For Each kvp In IndexConfig.Columns
                                Dim column As indexConfigColumnClass = kvp.Value
                                'For ColumnPointer = 0 To CustomAdminColumnCount - 1
                                With column
                                    ColumnWidth = .Width
                                    'fieldId = .FieldId
                                    FieldName = genericController.vbLCase(.Name)
                                End With
                                FindWordValue = ""
                                If IndexConfig.FindWords.ContainsKey(FieldName) Then
                                    With IndexConfig.FindWords(FieldName)
                                        If (.MatchOption = FindWordMatchEnum.matchincludes) Or (.MatchOption = FindWordMatchEnum.MatchEquals) Then
                                            FindWordValue = .Value
                                        End If
                                    End With
                                End If
                                DataTable_FindRow = DataTable_FindRow _
                                    & vbCrLf _
                                    & "<td valign=""top"" align=""center"" class=""ccPanel3DReverse"" style=""padding-top:2px;padding-bottom:2px;"">" _
                                    & "<input type=hidden name=""FindName" & ColumnPointer & """ value=""" & FieldName & """>" _
                                    & "<input onkeypress=""KeyCheck(event);""  type=text id=""F" & ColumnPointer & """ name=""FindValue" & ColumnPointer & """ value=""" & FindWordValue & """ style=""width:98%"">" _
                                    & "</td>"
                                ColumnPointer += 1
                            Next
                            DataTable_FindRow = DataTable_FindRow & "</tr>"
                            '
                            ' Assemble DataTable
                            '
                            DataTable = "" _
                                & "<table ID=""DataTable"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""Background-Color:white;"">" _
                                & DataTable_HdrRow _
                                & DataTable_DataRows _
                                & DataTable_FindRow _
                                & "</table>"
                            'DataTable = GetForm_Index_AdvancedSearch()
                            '
                            ' Assemble DataFilterTable
                            '
                            If IndexFilterContent <> "" Then
                                FilterColumn = "<td valign=top style=""border-right:1px solid black;"" class=""ccToolsCon"">" & IndexFilterJS & IndexFilterHead & IndexFilterContent & "</td>"
                                'FilterColumn = "<td valign=top class=""ccPanel3DReverse ccAdminEditBody"" style=""border-right:1px solid black;"">" & IndexFilterJS & IndexFilterHead & IndexFilterContent & "</td>"
                            End If
                            DataColumn = "<td width=""99%"" valign=top>" & DataTable & "</td>"
                            FilterDataTable = "" _
                                & "<table ID=""DataFilterTable"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""Background-Color:white;"">" _
                                & "<tr>" _
                                & FilterColumn _
                                & DataColumn _
                                & "</tr>" _
                                & "</table>"
                            '
                            ' Assemble LiveWindowTable
                            '
                            ' Stream.Add( OpenLiveWindowTable)
                            Stream.Add(vbCrLf & cpCore.html.html_GetFormStart(, "adminForm"))
                            Stream.Add("<input type=""hidden"" name=""indexGoToPage"" value="""">")
                            Stream.Add(ButtonBar)
                            Stream.Add(Adminui.GetTitleBar(TitleBar, HeaderDescription))
                            Stream.Add(FilterDataTable)
                            Stream.Add(ButtonBar)
                            Stream.Add(cpCore.html.main_GetPanel("<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"", height=""10"" >"))
                            Stream.Add("<input type=hidden name=Columncnt VALUE=" & IndexConfig.Columns.Count & ">")
                            Stream.Add("</form>")
                            '  Stream.Add( CloseLiveWindowTable)
                            Call cpCore.html.addTitle(adminContent.Name)
                        End If
                    End If
                    'End If
                    '
                End If
                returnForm = Stream.Text
                '
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnForm
        End Function
        '
        '========================================================================
        ' ----- Get an XML nodes attribute based on its name
        '========================================================================
        '
        Private Function GetXMLAttribute(ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As String) As String
            On Error GoTo ErrorTrap
            '
            Dim NodeAttribute As XmlAttribute
            Dim ResultNode As XmlNode
            Dim UcaseName As String
            '
            Found = False
            ResultNode = Node.Attributes.GetNamedItem(Name)
            If (ResultNode Is Nothing) Then
                UcaseName = genericController.vbUCase(Name)
                For Each NodeAttribute In Node.Attributes
                    If genericController.vbUCase(NodeAttribute.Name) = UcaseName Then
                        GetXMLAttribute = NodeAttribute.Value
                        Found = True
                        Exit For
                    End If
                Next
            Else
                GetXMLAttribute = ResultNode.Value
                Found = True
            End If
            If Not Found Then
                GetXMLAttribute = DefaultIfNotFound
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetXMLAttribute")
        End Function
        '
        ' REFACTOR -- THIS SHOULD BE A REMOTE METHOD AND NOT CALLED FROM CPCORE.
        '==========================================================================================================================================
        ''' <summary>
        ''' Get index view filter content - remote method
        ''' </summary>
        ''' <param name="adminContent"></param>
        ''' <returns></returns>
        Public Function GetForm_IndexFilterContent(adminContent As Models.Complex.cdefModel) As String
            Dim returnContent As String = ""
            Try
                Dim RecordID As Integer
                Dim Name As String
                Dim TableName As String
                Dim FieldCaption As String
                Dim ContentName As String
                Dim CS As Integer
                Dim SQL As String
                Dim Caption As String
                Dim Link As String
                Dim IsAuthoringMode As Boolean
                Dim FirstCaption As String = ""
                Dim RQS As String
                Dim QS As String
                Dim Ptr As Integer
                Dim SubFilterList As String
                Dim IndexConfig As indexConfigClass
                Dim list As String
                Dim ListSplit() As String
                Dim Cnt As Integer
                Dim Pos As Integer
                Dim subContentID As Integer
                '
                IndexConfig = LoadIndexConfig(adminContent)
                With IndexConfig
                    '
                    ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, adminContent.Id)
                    IsAuthoringMode = True
                    RQS = "cid=" & adminContent.Id & "&af=1"
                    '
                    '-------------------------------------------------------------------------------------
                    ' Remove filters
                    '-------------------------------------------------------------------------------------
                    '
                    If (.SubCDefID > 0) Or (.GroupListCnt <> 0) Or (.FindWords.Count <> 0) Or .ActiveOnly Or .LastEditedByMe Or .LastEditedToday Or .LastEditedPast7Days Or .LastEditedPast30Days Then
                        '
                        ' Remove Filters
                        '
                        returnContent &= "<div class=""ccFilterHead"">Remove&nbsp;Filters</div>"
                        QS = RQS
                        QS = genericController.ModifyQueryString(QS, "IndexFilterRemoveAll", "1")
                        Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                        returnContent &= "<div class=""ccFilterSubHead""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;Remove All</a></div>"
                        '
                        ' Last Edited Edited by me
                        '
                        SubFilterList = ""
                        If .LastEditedByMe Then
                            QS = RQS
                            QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedByMe", CStr(0), True)
                            Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                            SubFilterList = SubFilterList & "<div class=""ccFilterIndent ccFilterList""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">By&nbsp;Me</a></div>"
                        End If
                        If .LastEditedToday Then
                            QS = RQS
                            QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedToday", CStr(0), True)
                            Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                            SubFilterList = SubFilterList & "<div class=""ccFilterIndent ccFilterList""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">Today</a></div>"
                        End If
                        If .LastEditedPast7Days Then
                            QS = RQS
                            QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedPast7Days", CStr(0), True)
                            Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                            SubFilterList = SubFilterList & "<div class=""ccFilterIndent ccFilterList""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">Past Week</a></div>"
                        End If
                        If .LastEditedPast30Days Then
                            QS = RQS
                            QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedPast30Days", CStr(0), True)
                            Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                            SubFilterList = SubFilterList & "<div class=""ccFilterIndent ccFilterList""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">Past 30 Days</a></div>"
                        End If
                        If SubFilterList <> "" Then
                            returnContent &= "<div class=""ccFilterSubHead"">Last&nbsp;Edited</div>" & SubFilterList
                        End If
                        '
                        ' Sub Content definitions
                        '
                        Dim SubContentName As String
                        SubFilterList = ""
                        If .SubCDefID > 0 Then
                            SubContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, .SubCDefID)
                            QS = RQS
                            QS = genericController.ModifyQueryString(QS, "IndexFilterRemoveCDef", CStr(.SubCDefID))
                            Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                            SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">" & SubContentName & "</a></div>"
                        End If
                        If SubFilterList <> "" Then
                            returnContent &= "<div class=""ccFilterSubHead"">In Sub-content</div>" & SubFilterList
                        End If
                        '
                        ' Group Filter List
                        '
                        Dim GroupName As String
                        SubFilterList = ""
                        If .GroupListCnt > 0 Then
                            For Ptr = 0 To .GroupListCnt - 1
                                GroupName = .GroupList(Ptr)
                                If .GroupList(Ptr) <> "" Then
                                    If Len(GroupName) > 30 Then
                                        GroupName = Left(GroupName, 15) & "..." & Right(GroupName, 15)
                                    End If
                                    QS = RQS
                                    QS = genericController.ModifyQueryString(QS, "IndexFilterRemoveGroup", .GroupList(Ptr))
                                    Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                                    SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">" & GroupName & "</a></div>"
                                End If
                            Next
                        End If
                        If SubFilterList <> "" Then
                            returnContent &= "<div class=""ccFilterSubHead"">In Group(s)</div>" & SubFilterList
                        End If
                        '
                        ' Other Filter List
                        '
                        SubFilterList = ""
                        If .ActiveOnly Then
                            QS = RQS
                            QS = genericController.ModifyQueryString(QS, "IndexFilterActiveOnly", CStr(0), True)
                            Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                            SubFilterList = SubFilterList & "<div class=""ccFilterIndent ccFilterList""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">Active&nbsp;Only</a></div>"
                        End If
                        If SubFilterList <> "" Then
                            returnContent &= "<div class=""ccFilterSubHead"">Other</div>" & SubFilterList
                        End If
                        '
                        ' FindWords
                        '
                        For Each findWordKvp In .FindWords
                            Dim findWord As indexConfigFindWordClass = findWordKvp.Value
                            FieldCaption = genericController.encodeText(Models.Complex.cdefModel.GetContentFieldProperty(cpCore, ContentName, findWord.Name, "caption"))
                            QS = RQS
                            QS = genericController.ModifyQueryString(QS, "IndexFilterRemoveFind", findWord.Name)
                            Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                            Select Case findWord.MatchOption
                                Case FindWordMatchEnum.matchincludes
                                    returnContent &= "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;" & FieldCaption & "&nbsp;includes&nbsp;'" & findWord.Value & "'</a></div>"
                                Case FindWordMatchEnum.MatchEmpty
                                    returnContent &= "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;" & FieldCaption & "&nbsp;is&nbsp;empty</a></div>"
                                Case FindWordMatchEnum.MatchEquals
                                    returnContent &= "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;" & FieldCaption & "&nbsp;=&nbsp;'" & findWord.Value & "'</a></div>"
                                Case FindWordMatchEnum.MatchFalse
                                    returnContent &= "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;" & FieldCaption & "&nbsp;is&nbsp;false</a></div>"
                                Case FindWordMatchEnum.MatchGreaterThan
                                    returnContent &= "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;" & FieldCaption & "&nbsp;&gt;&nbsp;'" & findWord.Value & "'</a></div>"
                                Case FindWordMatchEnum.MatchLessThan
                                    returnContent &= "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;" & FieldCaption & "&nbsp;&lt;&nbsp;'" & findWord.Value & "'</a></div>"
                                Case FindWordMatchEnum.MatchNotEmpty
                                    returnContent &= "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;" & FieldCaption & "&nbsp;is&nbsp;not&nbsp;empty</a></div>"
                                Case FindWordMatchEnum.MatchTrue
                                    returnContent &= "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;" & FieldCaption & "&nbsp;is&nbsp;true</a></div>"
                            End Select
                        Next
                        '
                        returnContent &= "<div style=""border-bottom:1px dotted #808080;"">&nbsp;</div>"
                    End If
                    '
                    '-------------------------------------------------------------------------------------
                    ' Add filters
                    '-------------------------------------------------------------------------------------
                    '
                    returnContent &= "<div class=""ccFilterHead"">Add&nbsp;Filters</div>"
                    '
                    ' Last Edited
                    '
                    SubFilterList = ""
                    If Not .LastEditedByMe Then
                        QS = RQS
                        QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedByMe", "1", True)
                        Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                        SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """>By&nbsp;Me</a></div>"
                    End If
                    If Not .LastEditedToday Then
                        QS = RQS
                        QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedToday", "1", True)
                        Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                        SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """>Today</a></div>"
                    End If
                    If Not .LastEditedPast7Days Then
                        QS = RQS
                        QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedPast7Days", "1", True)
                        Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                        SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """>Past Week</a></div>"
                    End If
                    If Not .LastEditedPast30Days Then
                        QS = RQS
                        QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedPast30Days", "1", True)
                        Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                        SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """>Past 30 Days</a></div>"
                    End If
                    If SubFilterList <> "" Then
                        returnContent &= "<div class=""ccFilterSubHead"">Last&nbsp;Edited</div>" & SubFilterList
                    End If
                    '
                    ' Sub Content Definitions
                    '
                    SubFilterList = ""
                    list = Models.Complex.cdefModel.getContentControlCriteria(cpCore, ContentName)
                    If list <> "" Then
                        ListSplit = Split(list, "=")
                        Cnt = UBound(ListSplit) + 1
                        If Cnt > 0 Then
                            For Ptr = 0 To Cnt - 1
                                Pos = genericController.vbInstr(1, ListSplit(Ptr), ")")
                                If Pos > 0 Then
                                    subContentID = genericController.EncodeInteger(Mid(ListSplit(Ptr), 1, Pos - 1))
                                    If subContentID > 0 And (subContentID <> adminContent.Id) And (subContentID <> .SubCDefID) Then
                                        Caption = "<span style=""white-space:nowrap;"">" & Models.Complex.cdefModel.getContentNameByID(cpCore, subContentID) & "</span>"
                                        QS = RQS
                                        QS = genericController.ModifyQueryString(QS, "IndexFilterAddCDef", CStr(subContentID), True)
                                        Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                                        SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """>" & Caption & "</a></div>"
                                    End If
                                End If
                            Next
                        End If
                    End If
                    If SubFilterList <> "" Then
                        returnContent &= "<div class=""ccFilterSubHead"">In Sub-content</div>" & SubFilterList
                    End If
                    '
                    ' people filters
                    '
                    TableName = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName)
                    SubFilterList = ""
                    If genericController.vbLCase(TableName) = genericController.vbLCase("ccMembers") Then
                        SQL = cpCore.db.GetSQLSelect("default", "ccGroups", "ID,Caption,Name", "(active<>0)", "Caption,Name")
                        CS = cpCore.db.csOpenSql_rev("default", SQL)
                        Do While cpCore.db.csOk(CS)
                            Name = cpCore.db.csGetText(CS, "Name")
                            Ptr = 0
                            If .GroupListCnt > 0 Then
                                For Ptr = 0 To .GroupListCnt - 1
                                    If Name = .GroupList(Ptr) Then
                                        Exit For
                                    End If
                                Next
                            End If
                            If Ptr = .GroupListCnt Then
                                RecordID = cpCore.db.csGetInteger(CS, "ID")
                                Caption = cpCore.db.csGetText(CS, "Caption")
                                If Caption = "" Then
                                    Caption = Name
                                    If Caption = "" Then
                                        Caption = "Group " & RecordID
                                    End If
                                End If
                                If Len(Caption) > 30 Then
                                    Caption = Left(Caption, 15) & "..." & Right(Caption, 15)
                                End If
                                Caption = "<span style=""white-space:nowrap;"">" & Caption & "</span>"
                                QS = RQS
                                If Trim(Name) <> "" Then
                                    QS = genericController.ModifyQueryString(QS, "IndexFilterAddGroup", Name, True)
                                Else
                                    QS = genericController.ModifyQueryString(QS, "IndexFilterAddGroup", CStr(RecordID), True)
                                End If
                                Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                                SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """>" & Caption & "</a></div>"
                            End If
                            cpCore.db.csGoNext(CS)
                        Loop
                    End If
                    If SubFilterList <> "" Then
                        returnContent &= "<div class=""ccFilterSubHead"">In Group(s)</div>" & SubFilterList
                    End If
                    '
                    ' Active Only
                    '
                    SubFilterList = ""
                    If Not .ActiveOnly Then
                        QS = RQS
                        QS = genericController.ModifyQueryString(QS, "IndexFilterActiveOnly", "1", True)
                        Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                        SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """>Active&nbsp;Only</a></div>"
                    End If
                    If SubFilterList <> "" Then
                        returnContent &= "<div class=""ccFilterSubHead"">Other</div>" & SubFilterList
                    End If
                    '
                    returnContent &= "<div style=""border-bottom:1px dotted #808080;"">&nbsp;</div>"
                    '
                    ' Advanced Search Link
                    '
                    QS = RQS
                    QS = genericController.ModifyQueryString(QS, RequestNameAdminSubForm, AdminFormIndex_SubFormAdvancedSearch, True)
                    Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                    returnContent &= "<div class=""ccFilterHead""><a class=""ccFilterLink"" href=""" & Link & """>Advanced&nbsp;Search</a></div>"
                    '
                    returnContent &= "<div style=""border-bottom:1px dotted #808080;"">&nbsp;</div>"
                    '
                    ' Set Column Link
                    '
                    QS = RQS
                    QS = genericController.ModifyQueryString(QS, RequestNameAdminSubForm, AdminFormIndex_SubFormSetColumns, True)
                    Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                    returnContent &= "<div class=""ccFilterHead""><a class=""ccFilterLink"" href=""" & Link & """>Set&nbsp;Columns</a></div>"
                    '
                    returnContent &= "<div style=""border-bottom:1px dotted #808080;"">&nbsp;</div>"
                    '
                    ' Import Link
                    '
                    QS = RQS
                    QS = genericController.ModifyQueryString(QS, RequestNameAdminForm, AdminFormImportWizard, True)
                    Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                    returnContent &= "<div class=""ccFilterHead""><a class=""ccFilterLink"" href=""" & Link & """>Import</a></div>"
                    '
                    returnContent &= "<div style=""border-bottom:1px dotted #808080;"">&nbsp;</div>"
                    '
                    ' Export Link
                    '
                    QS = RQS
                    QS = genericController.ModifyQueryString(QS, RequestNameAdminSubForm, AdminFormIndex_SubFormExport, True)
                    Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                    returnContent &= "<div class=""ccFilterHead""><a class=""ccFilterLink"" href=""" & Link & """>Export</a></div>"
                    '
                    returnContent &= "<div style=""border-bottom:1px dotted #808080;"">&nbsp;</div>"
                    '
                    returnContent = "<div style=""padding-left:10px;padding-right:10px;"">" & returnContent & "</div>"
                End With
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnContent
        End Function
        '
    End Class
End Namespace
