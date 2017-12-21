
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
        '========================================================================
        ' ----- Print the Normal Content Edit form
        '
        '   Print the content fields and Topic Groups section
        '========================================================================
        '
        Private Function GetForm_Publish() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "GetForm_Publish")
            '
            Dim FieldList As String
            Dim ModifiedDateString As String
            Dim SubmittedDateString As String
            Dim ApprovedDateString As String
            Dim Adminui As New adminUIController(cpCore)
            Dim ButtonList As String = ""
            Dim Caption As String
            Dim CS As Integer
            Dim SQL As String
            Dim RowColor As String
            Dim RecordCount As Integer
            Dim RecordLast As Integer
            Dim RecordNext As Integer
            Dim RecordPrevious As Integer
            Dim RecordName As String
            Dim Copy As String
            Dim ContentID As Integer
            Dim ContentName As String
            Dim RecordID As Integer
            Dim Link As String
            Dim CSAuthoringRecord As Integer
            Dim TableName As String
            Dim PageNumber As Integer
            '
            Dim IsInserted As Boolean
            Dim IsDeleted As Boolean
            '
            Dim IsModified As Boolean
            Dim ModifiedName As String = ""
            Dim ModifiedDate As Date
            '
            Dim IsSubmitted As Boolean
            Dim SubmitName As String = ""
            Dim SubmittedDate As Date
            '
            Dim IsApproved As Boolean
            Dim ApprovedName As String = ""
            Dim ApprovedDate As Date
            Dim Stream As New stringBuilderLegacyController
            Dim Body As String = ""
            Dim Description As String
            Dim Button As String
            Dim BR As String = ""
            '
            Button = cpCore.docProperties.getText(RequestNameButton)
            If Button = ButtonCancel Then
                '
                '
                '
                Return cpCore.webServer.redirect("/" & cpCore.serverConfig.appConfig.adminRoute, "Admin Publish, Cancel Button Pressed")
            ElseIf Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                '
                '
                '
                ButtonList = ButtonCancel
                Body &= Adminui.GetFormBodyAdminOnly()
            Else
                '
                ' ----- Page Body
                '
                BR = "<br>"
                Body &= cr & "<table border=""0"" cellpadding=""2"" cellspacing=""2"" width=""100%"">"
                Body &= cr & "<tr>"
                Body &= cr & "<td width=""50"" class=""ccPanel"" align=""center"" class=""ccAdminSmall"">Pub" & BR & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""42"" height=""1"" ></td>"
                Body &= cr & "<td width=""50"" class=""ccPanel"" align=""center"" class=""ccAdminSmall"">Sub'd" & BR & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""42"" height=""1"" ></td>"
                Body &= cr & "<td width=""50"" class=""ccPanel"" align=""center"" class=""ccAdminSmall"">Appr'd" & BR & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""42"" height=""1"" ></td>"
                Body &= cr & "<td width=""50"" class=""ccPanel"" class=""ccAdminSmall"">Edit" & BR & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""42"" height=""1"" ></td>"
                Body &= cr & "<td width=""200"" class=""ccPanel"" class=""ccAdminSmall"">Name" & BR & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""192"" height=""1"" ></td>"
                Body &= cr & "<td width=""100"" class=""ccPanel"" class=""ccAdminSmall"">Content" & BR & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""92"" height=""1"" ></td>"
                Body &= cr & "<td width=""50"" class=""ccPanel"" class=""ccAdminSmall"">#" & BR & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""92"" height=""1"" ></td>"
                Body &= cr & "<td width=""100"" class=""ccPanel"" class=""ccAdminSmall"">Public" & BR & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""92"" height=""1"" ></td>"
                Body &= cr & "<td width=""100%"" class=""ccPanel"" class=""ccAdminSmall"">Status" & BR & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""1"" ></td>"
                Body &= cr & "</tr>"
                '
                ' ----- select modified,submitted,approved records (all non-editing controls)
                '
                SQL = "SELECT DISTINCT top 100 ccAuthoringControls.ContentID AS ContentID, ccContent.Name AS ContentName, ccAuthoringControls.RecordID, ccContentWatch.Link AS Link, ccContent.AllowWorkflowAuthoring AS ContentAllowWorkflowAuthoring,min(ccAuthoringControls.ID)" _
                    & " FROM (ccAuthoringControls" _
                    & " LEFT JOIN ccContent ON ccAuthoringControls.ContentID = ccContent.ID)" _
                    & " LEFT JOIN ccContentWatch ON ccAuthoringControls.ContentRecordKey = ccContentWatch.ContentRecordKey" _
                    & " Where (ccAuthoringControls.ControlType > 1)" _
                    & " GROUP BY ccAuthoringControls.ContentID, ccContent.Name, ccAuthoringControls.RecordID, ccContentWatch.Link, ccContent.AllowWorkflowAuthoring" _
                    & " order by min(ccAuthoringControls.ID) desc"
                ''PageNumber = 1 + (RecordTop / RecordsPerPage)
                'SQL = "SELECT DISTINCT ccContent.ID AS ContentID, ccContent.Name AS ContentName, ccAuthoringControls.RecordID, ccContentWatch.Link AS Link, ccContent.AllowWorkflowAuthoring AS ContentAllowWorkflowAuthoring,max(ccAuthoringControls.DateAdded) as DateAdded" _
                '    & " FROM (ccAuthoringControls LEFT JOIN ccContent ON ccAuthoringControls.ContentID = ccContent.ID) LEFT JOIN ccContentWatch ON ccAuthoringControls.ContentRecordKey = ccContentWatch.ContentRecordKey" _
                '    & " GROUP BY ccAuthoringControls.ID,ccContent.ID, ccContent.Name, ccAuthoringControls.RecordID, ccContentWatch.Link, ccContent.AllowWorkflowAuthoring, ccAuthoringControls.ControlType" _
                '    & " HAVING (ccAuthoringControls.ControlType>1)" _
                '    & " order by max(ccAuthoringControls.DateAdded) Desc"
                CS = cpCore.db.csOpenSql_rev("Default", SQL)
                'CS = cpCore.app_openCsSql_Rev_Internal("Default", SQL, RecordsPerPage, PageNumber)
                RecordCount = 0
                If cpCore.db.csOk(CS) Then
                    RowColor = ""
                    RecordLast = RecordTop + RecordsPerPage
                    '
                    ' --- Print out the records
                    '
                    Do While cpCore.db.csOk(CS) And RecordCount < 100
                        ContentID = cpCore.db.csGetInteger(CS, "contentID")
                        ContentName = cpCore.db.csGetText(CS, "contentname")
                        RecordID = cpCore.db.csGetInteger(CS, "recordid")
                        Link = pageContentController.getPageLink(cpCore, RecordID, "", True, False)
                        'Link = cpCore.main_GetPageLink3(RecordID, "", True)
                        'If Link = "" Then
                        '    Link = cpCore.db.cs_getText(CS, "Link")
                        'End If
                        If (ContentID = 0) Or (ContentName = "") Or (RecordID = 0) Then
                            '
                            ' This control is not valid, delete it
                            '
                            SQL = "delete from ccAuthoringControls where ContentID=" & ContentID & " and RecordID=" & RecordID
                            Call cpCore.db.executeQuery(SQL)
                        Else
                            TableName = Models.Complex.cdefModel.GetContentProperty(cpCore, ContentName, "ContentTableName")
                            If Not (cpCore.db.csGetBoolean(CS, "ContentAllowWorkflowAuthoring")) Then
                                '
                                ' Authoring bug -- This record should not be here, the content does not support workflow authoring
                                '
                                Call handleLegacyClassError2("GetForm_Publish", "Admin Workflow Publish selected an authoring control record [" & ContentID & "." & RecordID & "] for a content definition [" & ContentName & "] that does not AllowWorkflowAuthoring.")
                                'Call HandleInternalError("GetForm_Publish", "Admin Workflow Publish selected an authoring control record [" & ContentID & "." & RecordID & "] for a content definition [" & ContentName & "] that does not AllowWorkflowAuthoring.")
                            Else

                                Call cpCore.doc.getAuthoringStatus(ContentName, RecordID, IsSubmitted, IsApproved, SubmitName, ApprovedName, IsInserted, IsDeleted, IsModified, ModifiedName, ModifiedDate, SubmittedDate, ApprovedDate)
                                If RowColor = "class=""ccPanelRowOdd""" Then
                                    RowColor = "class=""ccPanelRowEven"""
                                Else
                                    RowColor = "class=""ccPanelRowOdd"""
                                End If
                                '
                                ' make sure the record exists
                                '
                                If genericController.vbUCase(TableName) = "CCPAGECONTENT" Then
                                    FieldList = "ID,Name,Headline,MenuHeadline"
                                    'SQL = "SELECT ID,Name,Headline,MenuHeadline from " & TableName & " WHERE ID=" & RecordID
                                Else
                                    FieldList = "ID,Name,Name as Headline,Name as MenuHeadline"
                                    'SQL = "SELECT ID,Name,Name as Headline,Name as MenuHeadline from " & TableName & " WHERE ID=" & RecordID
                                End If
                                CSAuthoringRecord = cpCore.db.csOpenRecord(ContentName, RecordID, True, True, FieldList)
                                'CSAuthoringRecord = cpCore.app_openCsSql_Rev_Internal("Default", SQL, 1)
                                If Not cpCore.db.csOk(CSAuthoringRecord) Then
                                    '
                                    ' This authoring control is not valid, delete it
                                    '
                                    SQL = "delete from ccAuthoringControls where ContentID=" & ContentID & " and RecordID=" & RecordID
                                    Call cpCore.db.executeQuery(SQL)
                                Else
                                    RecordName = cpCore.db.csGet(CSAuthoringRecord, "name")
                                    If RecordName = "" Then
                                        RecordName = cpCore.db.csGet(CSAuthoringRecord, "headline")
                                        If RecordName = "" Then
                                            RecordName = cpCore.db.csGet(CSAuthoringRecord, "headline")
                                            If RecordName = "" Then
                                                RecordName = "Record " & cpCore.db.csGet(CSAuthoringRecord, "ID")
                                            End If
                                        End If
                                    End If
                                    If True Then
                                        If Link = "" Then
                                            Link = "unknown"
                                        Else
                                            Link = "<a href=""" & genericController.encodeHTML(Link) & """ target=""_blank"">" & Link & "</a>"
                                        End If
                                        '
                                        ' get approved status of the submitted record
                                        '
                                        Body &= (vbLf & "<tr>")
                                        '
                                        ' Publish Checkbox
                                        '
                                        Body &= ("<td align=""center"" valign=""top"" " & RowColor & ">" _
                                            & cpCore.html.html_GetFormInputCheckBox2("row" & RecordCount, False) _
                                            & cpCore.html.html_GetFormInputHidden("rowid" & RecordCount, RecordID) _
                                            & cpCore.html.html_GetFormInputHidden("rowcontentname" & RecordCount, ContentName) _
                                            & "</td>")
                                        '
                                        ' Submitted
                                        '
                                        If IsSubmitted Then
                                            Copy = "yes"
                                        Else
                                            Copy = "no"
                                        End If
                                        Body &= ("<td align=""center"" valign=""top"" " & RowColor & " class=""ccAdminSmall"">" & Copy & "</td>")
                                        '
                                        ' Approved
                                        '
                                        If IsApproved Then
                                            Copy = "yes"
                                        Else
                                            Copy = "no"
                                        End If
                                        Body &= ("<td align=""center"" valign=""top"" " & RowColor & " class=""ccAdminSmall"">" & Copy & "</td>")
                                        '
                                        ' Edit
                                        '
                                        Body = Body _
                                            & "<td align=""left"" valign=""top"" " & RowColor & " class=""ccAdminSmall"">" _
                                            & "<a href=""?" & RequestNameAdminForm & "=" & AdminFormEdit & "&cid=" & ContentID & "&id=" & RecordID & "&" & RequestNameAdminDepth & "=1"">Edit</a>" _
                                            & "</td>"
                                        '
                                        ' Name
                                        '
                                        Body &= ("<td align=""left"" valign=""top"" " & RowColor & " class=""ccAdminSmall""  style=""white-space:nowrap;"">" & RecordName & "</td>")
                                        '
                                        ' Content
                                        '
                                        Body &= ("<td align=""left"" valign=""top"" " & RowColor & " class=""ccAdminSmall"">" & ContentName & "</td>")
                                        '
                                        ' RecordID
                                        '
                                        Body &= ("<td align=""left"" valign=""top"" " & RowColor & " class=""ccAdminSmall"">" & RecordID & "</td>")
                                        '
                                        ' Public
                                        '
                                        If IsInserted Then
                                            Link = Link & "*"
                                        ElseIf IsDeleted Then
                                            Link = Link & "**"
                                        End If
                                        Body &= ("<td align=""left"" valign=""top"" " & RowColor & " class=""ccAdminSmall"" style=""white-space:nowrap;"">" & Link & "</td>")
                                        '
                                        ' Description
                                        '
                                        'Call cpCore.app.closeCS(CSLink)
                                        Body &= ("<td align=""left"" valign=""top"" " & RowColor & ">" & SpanClassAdminNormal)
                                        '
                                        'If RecordName <> "" Then
                                        '    Body &=  (cpCore.htmldoc.main_encodeHTML(RecordName) & ", ")
                                        'End If
                                        'Body &=  ("Content: " & ContentName & ", RecordID: " & RecordID & "" & br & "")
                                        If ModifiedDate = Date.MinValue Then
                                            ModifiedDateString = "unknown"
                                        Else
                                            ModifiedDateString = CStr(ModifiedDate)
                                        End If
                                        If ModifiedName = "" Then
                                            ModifiedName = "unknown"
                                        End If
                                        If SubmitName = "" Then
                                            SubmitName = "unknown"
                                        End If
                                        If ApprovedName = "" Then
                                            ApprovedName = "unknown"
                                        End If
                                        If IsInserted Then
                                            Body &= ("Added: " & ModifiedDateString & " by " & ModifiedName & "" & BR & "")
                                        ElseIf IsDeleted Then
                                            Body &= ("Deleted: " & ModifiedDateString & " by " & ModifiedName & "" & BR & "")
                                        Else
                                            Body &= ("Modified: " & ModifiedDateString & " by " & ModifiedName & "" & BR & "")
                                        End If
                                        If IsSubmitted Then
                                            If SubmittedDate = Date.MinValue Then
                                                SubmittedDateString = "unknown"
                                            Else
                                                SubmittedDateString = CStr(SubmittedDate)
                                            End If
                                            Body &= ("Submitted: " & SubmittedDateString & " by " & SubmitName & "" & BR & "")
                                        End If
                                        If IsApproved Then
                                            If ApprovedDate = Date.MinValue Then
                                                ApprovedDateString = "unknown"
                                            Else
                                                ApprovedDateString = CStr(ApprovedDate)
                                            End If
                                            Body &= ("Approved: " & ApprovedDate & " by " & ApprovedName & "" & BR & "")
                                        End If
                                        'Body &=  ("Admin Site: <a href=""?" & RequestNameAdminForm & "=" & AdminFormEdit & "&cid=" & ContentID & "&id=" & RecordID & "&" & RequestNameAdminDepth & "=1"" target=""_blank"">Open in New Window</a>" & br & "")
                                        'Body &=  ("Public Site: " & Link & "" & br & "")
                                        '
                                        Body &= ("</td>")
                                        '
                                        Body &= (vbLf & "</tr>")
                                        RecordCount = RecordCount + 1
                                    End If
                                End If
                                Call cpCore.db.csClose(CSAuthoringRecord)
                            End If
                        End If
                        Call cpCore.db.csGoNext(CS)
                    Loop
                    '
                    ' --- print out the stuff at the bottom
                    '
                    RecordNext = RecordTop
                    If cpCore.db.csOk(CS) Then
                        RecordNext = RecordCount
                    End If
                    RecordPrevious = RecordTop - RecordsPerPage
                    If RecordPrevious < 0 Then
                        RecordPrevious = 0
                    End If
                End If
                Call cpCore.db.csClose(CS)
                If RecordCount = 0 Then
                    '
                    ' No records printed
                    '
                    Body &= cr & "<tr><td width=""100%"" colspan=""9"" class=""ccAdminSmall"" style=""padding-top:10px;"">There are no modified records to review</td></tr>"
                Else
                    Body &= cr & "<tr><td width=""100%"" colspan=""9"" class=""ccAdminSmall"" style=""padding-top:10px;"">* To view these records on the public site you must enable Rendering Mode because they are new records that have not been published.</td></tr>"
                    Body &= cr & "<tr><td width=""100%"" colspan=""9"" class=""ccAdminSmall"">** To view these records on the public site you must disable Rendering Mode because they are deleted records that have not been published.</td></tr>"
                End If
                Body &= cr & "</table>"
                Body &= cpCore.html.html_GetFormInputHidden("RowCnt", RecordCount)
                Body = "<div style=""Background-color:white;"">" & Body & "</div>"
                '
                ' Headers, etc
                '
                ButtonList = ""
                If MenuDepth > 0 Then
                    ButtonList = ButtonList & "," & ButtonClose
                Else
                    ButtonList = ButtonList & "," & ButtonCancel
                End If
                'ButtonList = ButtonList & "," & ButtonWorkflowPublishApproved & "," & ButtonWorkflowPublishSelected
                ButtonList = Mid(ButtonList, 2)
                '
                ' Assemble Page
                '
                Body &= cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormPublishing)
            End If
            '
            Caption = SpanClassAdminNormal & "<strong>Workflow Publishing</strong></span>"
            Description = "Monitor and Approve Workflow Publishing Changes"
            If RecordCount >= 100 Then
                Description = Description & BR & BR & "Only the first 100 record are displayed"
            End If
            GetForm_Publish = Adminui.GetBody(Caption, ButtonList, "", True, True, Description, "", 0, Body)
            Call cpCore.html.addTitle("Workflow Publishing")
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Publish")
            '
        End Function
        '
        '========================================================================
        '   Generate the content of a tab in the Edit Screen
        '========================================================================
        '
        Private Function GetForm_Edit_Tab(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, ByVal RecordID As Integer, ByVal ContentID As Integer, ByVal ForceReadOnly As Boolean, ByVal IsLandingPage As Boolean, ByVal IsRootPage As Boolean, ByVal EditTab As String, ByVal EditorContext As csv_contentTypeEnum, ByRef return_NewFieldList As String, ByVal TemplateIDForStyles As Integer, ByVal HelpCnt As Integer, ByVal HelpIDCache() As Integer, ByVal helpDefaultCache() As String, ByVal HelpCustomCache() As String, ByVal AllowHelpMsgCustom As Boolean, ByVal helpIdIndex As keyPtrController, ByVal fieldTypeDefaultEditors As String(), ByVal fieldEditorPreferenceList As String, ByVal styleList As String, ByVal styleOptionList As String, ByVal emailIdForStyles As Integer, ByVal IsTemplateTable As Boolean, ByVal editorAddonListJSON As String) As String
            Dim returnHtml As String = ""
            Try
                '
                Dim AjaxQS As String
                Dim fancyBoxLinkId As String
                Dim fancyBoxContentId As String
                Dim fieldTypeDefaultEditorAddonId As Integer
                Dim fieldIdPos As Integer
                Dim Pos As Integer
                Dim editorAddonID As Integer
                Dim editorReadOnly As Boolean
                Dim addonOptionString As String = String.Empty
                Dim AllowHelpIcon As Boolean
                Dim fieldId As Integer
                Dim FieldHelpFound As Boolean
                Dim LcaseName As String
                Dim IsEmptyList As Boolean
                Dim HelpMsgCustom As String
                Dim HelpMsgDefault As String
                '
                Dim FieldValueDate As Date
                Dim WhyReadOnlyMsg As String
                Dim IsLongHelp As Boolean
                Dim IsEmptyHelp As Boolean
                Dim HelpMsg As String
                Dim CS As Integer
                Dim EditorStyleModifier As String
                Dim HelpClosedContentID As String
                Dim AllowHelpRow As Boolean
                Dim EditorRightSideIcon As String
                Dim EditorHelp As String
                Dim HelpEditorID As String
                Dim HelpOpenedReadID As String
                Dim HelpOpenedEditID As String
                Dim HelpClosedID As String
                Dim HelpID As String
                Dim HelpMsgClosed As String
                Dim HelpMsgOpenedRead As String
                Dim HelpMsgOpenedEdit As String
                Dim NewWay As Boolean
                Dim RecordName As String
                Dim GroupName As String
                Dim SelectMessage As String
                Dim IsBaseField As Boolean
                Dim FieldReadOnly As Boolean
                Dim NonEncodedLink As String
                Dim EncodedLink As String
                Dim Caption As String
                Dim lookups() As String
                Dim CSPointer As Integer
                Dim FieldName As String
                Dim FieldValueText As String
                Dim FieldValueInteger As Integer
                Dim FieldValueNumber As Double
                Dim FieldValueBoolean As Boolean
                Dim fieldTypeId As Integer
                Dim FieldValueObject As Object
                Dim FieldPreferenceHTML As Boolean
                Dim CSLookup As Integer
                Dim RedirectPath As String
                Dim LookupContentName As String
                Dim s As New stringBuilderLegacyController
                Dim RecordReadOnly As Boolean
                Dim MethodName As String
                Dim FormFieldLCaseName As String
                Dim FieldRows As Integer
                Dim EditorString As String
                Dim FieldOptionRow As String
                Dim MTMContent0 As String
                Dim MTMContent1 As String
                Dim MTMRuleContent As String
                Dim MTMRuleField0 As String
                Dim MTMRuleField1 As String
                Dim AlphaSort As String
                Dim Adminui As New adminUIController(cpCore)
                Dim needUniqueEmailMessage As Boolean
                '
                needUniqueEmailMessage = False
                '
                returnHtml = ""
                MethodName = "AdminClass.GetFormEdit_UserFields"
                NewWay = True
                '
                ' ----- Open the panel
                '
                If adminContent.fields.Count <= 0 Then
                    '
                    ' There are no visible fiels, return empty
                    '
                    cpCore.handleException(New ApplicationException("The content definition for this record is invalid. It contains no valid fields."))
                Else
                    RecordReadOnly = ForceReadOnly
                    '
                    ' ----- Build an index to sort the fields by EditSortOrder
                    '
                    Dim sortingFields As New Dictionary(Of String, Models.Complex.CDefFieldModel)
                    '
                    For Each keyValuePair In adminContent.fields
                        Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                        With field
                            If .editTabName.ToLower() = EditTab.ToLower() Then
                                If IsVisibleUserField(.adminOnly, .developerOnly, .active, .authorable, .nameLc, adminContent.ContentTableName) Then
                                    AlphaSort = genericController.GetIntegerString(.editSortPriority, 10) & "-" & genericController.GetIntegerString(.id, 10)
                                    sortingFields.Add(AlphaSort, field)
                                End If
                            End If
                        End With
                    Next
                    '
                    ' ----- display the record fields
                    '
                    AllowHelpIcon = cpCore.visitProperty.getBoolean("AllowHelpIcon")
                    For Each kvp In sortingFields
                        Dim field As Models.Complex.CDefFieldModel = kvp.Value
                        With field
                            fieldId = .id
                            WhyReadOnlyMsg = ""
                            FieldName = .nameLc
                            FormFieldLCaseName = genericController.vbLCase(FieldName)
                            fieldTypeId = .fieldTypeId
                            FieldValueObject = editRecord.fieldsLc(.nameLc).value
                            FieldValueText = genericController.encodeText(FieldValueObject)
                            FieldRows = 1
                            FieldOptionRow = "&nbsp;"
                            FieldPreferenceHTML = .htmlContent
                            '
                            Caption = .caption
                            If .UniqueName Then
                                Caption = "&nbsp;**" & Caption
                            Else
                                If (LCase(.nameLc) = "email") Then
                                    If (LCase(adminContent.ContentTableName) = "ccmembers") And ((cpCore.siteProperties.getBoolean("allowemaillogin", False))) Then
                                        Caption = "&nbsp;***" & Caption
                                        needUniqueEmailMessage = True
                                    End If
                                End If
                            End If
                            If .Required Then
                                Caption = "&nbsp;*" & Caption
                            End If
                            IsBaseField = .blockAccess ' field renamed
                            FormInputCount = FormInputCount + 1
                            FieldReadOnly = False
                            '
                            ' Read only Special Cases
                            '
                            If IsLandingPage Then
                                Select Case genericController.vbLCase(.nameLc)
                                    Case "active"
                                        '
                                        ' if active, it is read only -- if inactive, let them set it active.
                                        '
                                        FieldReadOnly = (genericController.EncodeBoolean(FieldValueObject))
                                        If FieldReadOnly Then
                                            WhyReadOnlyMsg = "&nbsp;(disabled because you can not mark the landing page inactive)"
                                        End If
                                    Case "dateexpires", "pubdate", "datearchive", "blocksection", "hidemenu"
                                        '
                                        ' These fields are read only on landing pages
                                        '
                                        FieldReadOnly = True
                                        WhyReadOnlyMsg = "&nbsp;(disabled for the landing page)"
                                End Select
                            End If
                            '
                            If IsRootPage Then
                                Select Case genericController.vbLCase(.nameLc)
                                    Case "dateexpires", "pubdate", "datearchive", "archiveparentid"
                                        FieldReadOnly = True
                                        WhyReadOnlyMsg = "&nbsp;(disabled for root pages)"
                                    Case "allowinmenus", "allowinchildlists"
                                        FieldValueBoolean = True
                                        FieldValueObject = "1"
                                        FieldReadOnly = True
                                        WhyReadOnlyMsg = "&nbsp;(disabled for root pages)"
                                End Select
                            End If
                            '
                            ' Special Case - ccemail table Alloweid should be disabled if siteproperty AllowLinkLogin is false
                            '
                            If genericController.vbLCase(adminContent.ContentTableName) = "ccemail" And genericController.vbLCase(FieldName) = "allowlinkeid" Then
                                If Not (cpCore.siteProperties.getBoolean("AllowLinkLogin", True)) Then
                                    '.ValueVariant = "0"
                                    FieldValueObject = "0"
                                    FieldReadOnly = True
                                    FieldValueBoolean = False
                                    FieldValueText = "0"
                                End If
                            End If
                            EditorStyleModifier = genericController.vbLCase(cpCore.db.getFieldTypeNameFromFieldTypeId(fieldTypeId))
                            EditorString = ""
                            editorReadOnly = (RecordReadOnly Or .ReadOnly Or (editRecord.id <> 0 And .NotEditable) Or (FieldReadOnly))
                            '
                            ' Determine the editor: Contensive editor, field type default, or add-on preference
                            '
                            editorAddonID = 0
                            'editorPreferenceAddonId = 0
                            fieldIdPos = genericController.vbInstr(1, "," & fieldEditorPreferenceList, "," & CStr(fieldId) & ":")
                            Do While (editorAddonID = 0) And (fieldIdPos > 0)
                                fieldIdPos = fieldIdPos + 1 + Len(CStr(fieldId))
                                Pos = genericController.vbInstr(fieldIdPos, fieldEditorPreferenceList & ",", ",")
                                If Pos > 0 Then
                                    editorAddonID = genericController.EncodeInteger(Mid(fieldEditorPreferenceList, fieldIdPos, Pos - fieldIdPos))
                                    'editorPreferenceAddonId = genericController.EncodeInteger(Mid(fieldEditorPreferenceList, fieldIdPos, Pos - fieldIdPos))
                                    'editorAddonID = editorPreferenceAddonId
                                End If
                                fieldIdPos = genericController.vbInstr(fieldIdPos + 1, "," & fieldEditorPreferenceList, "," & CStr(fieldId) & ":")
                            Loop
                            If editorAddonID = 0 Then
                                fieldTypeDefaultEditorAddonId = genericController.EncodeInteger(fieldTypeDefaultEditors(fieldTypeId))
                                editorAddonID = fieldTypeDefaultEditorAddonId
                            End If
                            Dim useEditorAddon As Boolean
                            useEditorAddon = False
                            If (editorAddonID <> 0) Then
                                '
                                '--------------------------------------------------------------------------------------------
                                ' ----- Custom Editor
                                '--------------------------------------------------------------------------------------------
                                '
                                ' generate the style list on demand
                                ' note: &editorFieldType should be deprecated
                                '
                                cpCore.docProperties.setProperty("editorName", FormFieldLCaseName)
                                cpCore.docProperties.setProperty("editorValue", FieldValueText)
                                cpCore.docProperties.setProperty("editorFieldId", fieldId)
                                cpCore.docProperties.setProperty("editorFieldType", fieldTypeId)
                                cpCore.docProperties.setProperty("editorReadOnly", editorReadOnly)
                                cpCore.docProperties.setProperty("editorWidth", "")
                                cpCore.docProperties.setProperty("editorHeight", "")

                                'addonOptionString = "" _
                                '    & "editorName=" & genericController.encodeNvaArgument(FormFieldLCaseName) _
                                '    & "&editorValue=" & genericController.encodeNvaArgument(FieldValueText) _
                                '    & "&editorFieldId=" & fieldId _
                                '    & "&editorFieldType=" & fieldTypeId _
                                '    & "&editorReadOnly=" & editorReadOnly _
                                '    & "&editorWidth=" _
                                '    & "&editorHeight=" _
                                '    & ""
                                If genericController.EncodeBoolean((fieldTypeId = FieldTypeIdHTML) Or (fieldTypeId = FieldTypeIdFileHTML)) Then
                                    '
                                    ' include html related arguments
                                    '
                                    cpCore.docProperties.setProperty("editorAllowActiveContent", "1")
                                    cpCore.docProperties.setProperty("editorAddonList", editorAddonListJSON)
                                    cpCore.docProperties.setProperty("editorStyles", styleList)
                                    cpCore.docProperties.setProperty("editorStyleOptions", styleOptionList)
                                    ''                            ac = New innovaEditorAddonClassFPO
                                    ''                            Call ac.Init()
                                    ''                            editorAddonListJSON = ac.GetEditorAddonListJSON(IsTemplateTable, EditorContext)

                                    'addonOptionString = addonOptionString _
                                    '    & "&editorAllowActiveContent=1" _
                                    '    & "&editorAddonList=" & genericController.encodeNvaArgument(editorAddonListJSON) _
                                    '    & "&editorStyles=" & genericController.encodeNvaArgument(styleList) _
                                    '    & "&editorStyleOptions=" & genericController.encodeNvaArgument(styleOptionList) _
                                    '    & ""
                                End If
                                EditorString = cpCore.addon.execute(addonModel.create(cpCore, editorAddonID), New BaseClasses.CPUtilsBaseClass.addonExecuteContext With {.addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextEditor, .errorCaption = "field editor id:" & editorAddonID})
                                'EditorString = cpCore.addon.execute_legacy6(editorAddonID, "", addonOptionString, Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextEditor, "", 0, "", "", False, 0, "", False, Nothing, "", Nothing, "", 0, False)
                                useEditorAddon = Not String.IsNullOrEmpty(EditorString)
                                If useEditorAddon Then
                                    '
                                    ' -- editor worked
                                    return_NewFieldList = return_NewFieldList & "," & FieldName
                                Else
                                    '
                                    ' -- editor failed, determine if it is missing (or inactive). If missing, remove it from the members preferences
                                    Dim SQL As String = "select id from ccaggregatefunctions where id=" & editorAddonID
                                    CS = cpCore.db.csOpenSql(SQL)
                                    If Not cpCore.db.csOk(CS) Then
                                        '
                                        ' -- missing, not just inactive
                                        EditorString = ""
                                        '
                                        ' load user's editor preferences to fieldEditorPreferences() - this is the editor this user has picked when there are >1
                                        '   fieldId:addonId,fieldId:addonId,etc
                                        '   with custom FancyBox form in edit window with button "set editor preference"
                                        '   this button causes a 'refresh' action, reloads fields with stream without save
                                        '
                                        Dim tmpList As String = cpCore.userProperty.getText("editorPreferencesForContent:" & adminContent.Id, "")
                                        Dim PosStart As Integer = genericController.vbInstr(1, "," & tmpList, "," & fieldId & ":")
                                        If PosStart > 0 Then
                                            Dim PosEnd As Integer = genericController.vbInstr(PosStart + 1, "," & tmpList, ",")
                                            If PosEnd = 0 Then
                                                tmpList = Mid(tmpList, 1, PosStart - 1)
                                            Else
                                                tmpList = Mid(tmpList, 1, PosStart - 1) & Mid(tmpList, PosEnd)
                                            End If
                                            Call cpCore.userProperty.setProperty("editorPreferencesForContent:" & adminContent.Id, tmpList)
                                        End If
                                    End If
                                    Call cpCore.db.csClose(CS)
                                End If
                            End If
                            If Not useEditorAddon Then
                                '
                                ' if custom editor not used or if it failed
                                '
                                If (fieldTypeId = FieldTypeIdRedirect) Then
                                    'ElseIf (FieldType = FieldTypeRedirect) Then
                                    '
                                    '--------------------------------------------------------------------------------------------
                                    ' ----- Default Editor, Redirect fields (the same for normal/readonly/spelling)
                                    '--------------------------------------------------------------------------------------------
                                    '
                                    RedirectPath = cpCore.serverConfig.appConfig.adminRoute
                                    If .RedirectPath <> "" Then
                                        RedirectPath = .RedirectPath
                                    End If
                                    RedirectPath = RedirectPath _
                                        & "?" & RequestNameTitleExtension & "=" & genericController.EncodeRequestVariable(" For " & editRecord.nameLc & TitleExtension) _
                                        & "&" & RequestNameAdminDepth & "=" & (MenuDepth + 1) _
                                        & "&wl0=" & .RedirectID _
                                        & "&wr0=" & editRecord.id
                                    If .RedirectContentID <> 0 Then
                                        RedirectPath = RedirectPath & "&cid=" & .RedirectContentID
                                    Else
                                        RedirectPath = RedirectPath & "&cid=" & editRecord.contentControlId
                                    End If
                                    If editRecord.id = 0 Then
                                        EditorString &= ("[available after save]")
                                    Else
                                        RedirectPath = genericController.vbReplace(RedirectPath, "'", "\'")
                                        EditorString &= ("<a href=""#""")
                                        EditorString &= (" onclick=""" _
                                            & " window.open('" & RedirectPath & "', '_blank', 'scrollbars=yes,toolbar=no,status=no,resizable=yes');" _
                                            & " return false;""")
                                        EditorString &= (">")
                                        EditorString &= ("Open in New Window</A>")
                                    End If
                                    's.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal & EditorString & "&nbsp;</span></nobr></td>")
                                ElseIf editorReadOnly Then
                                    '
                                    '--------------------------------------------------------------------------------------------
                                    ' ----- Display fields as read only
                                    '--------------------------------------------------------------------------------------------
                                    '
                                    If WhyReadOnlyMsg <> "" Then
                                        WhyReadOnlyMsg = "<span class=""ccDisabledReason"">" & WhyReadOnlyMsg & "</span>"
                                    End If
                                    EditorStyleModifier = ""
                                    Select Case fieldTypeId
                                        Case FieldTypeIdBoolean
                                            '
                                            ' ----- Boolean ReadOnly
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueBoolean = genericController.EncodeBoolean(FieldValueObject)
                                            EditorString &= (cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, genericController.encodeText(FieldValueBoolean)))
                                            EditorString &= (cpCore.html.html_GetFormInputCheckBox2(FormFieldLCaseName, FieldValueBoolean, , True, "checkBox"))
                                            EditorString &= WhyReadOnlyMsg
                                            '
                                        Case FieldTypeIdFile, FieldTypeIdFileImage
                                            '
                                            ' ----- File ReadOnly
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            NonEncodedLink = cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, FieldValueText)
                                            EncodedLink = genericController.EncodeURL(NonEncodedLink)
                                            EditorString &= (cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, ""))
                                            If FieldValueText = "" Then
                                                EditorString &= ("[no file]")
                                            Else
                                                Dim filename As String = ""
                                                Dim path As String = ""
                                                cpCore.cdnFiles.splitPathFilename(FieldValueText, path, filename)
                                                EditorString &= ("&nbsp;<a href=""http://" & EncodedLink & """ target=""_blank"">" & SpanClassAdminSmall & "[" & filename & "]</A>")
                                            End If
                                            EditorString &= WhyReadOnlyMsg
                                            '
                                        Case FieldTypeIdLookup
                                            '
                                            ' ----- Lookup ReadOnly
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueInteger = genericController.EncodeInteger(FieldValueObject)
                                            EditorString &= (cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, genericController.encodeText(FieldValueInteger)))
                                            'Call s.Add("<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal)
                                            LookupContentName = ""
                                            If .lookupContentID <> 0 Then
                                                LookupContentName = genericController.encodeText(Models.Complex.cdefModel.getContentNameByID(cpCore, .lookupContentID))
                                            End If
                                            If LookupContentName <> "" Then
                                                CSLookup = cpCore.db.cs_open2(LookupContentName, FieldValueInteger, False, , "Name,ContentControlID")
                                                If cpCore.db.csOk(CSLookup) Then
                                                    If cpCore.db.csGet(CSLookup, "Name") = "" Then
                                                        EditorString &= ("No Name")
                                                    Else
                                                        EditorString &= (cpCore.html.main_encodeHTML(cpCore.db.csGet(CSLookup, "Name")))
                                                    End If
                                                    EditorString &= ("&nbsp;[<a TabIndex=-1 href=""?" & RequestNameAdminForm & "=4&cid=" & .lookupContentID & "&id=" & FieldValueObject.ToString & """ target=""_blank"">View details in new window</a>]")
                                                Else
                                                    EditorString &= ("None")
                                                End If
                                                Call cpCore.db.csClose(CSLookup)
                                                EditorString &= ("&nbsp;[<a TabIndex=-1 href=""?cid=" & .lookupContentID & """ target=""_blank"">See all " & LookupContentName & "</a>]")
                                            ElseIf .lookupList <> "" Then
                                                lookups = Split(.lookupList, ",")
                                                If FieldValueInteger < 1 Then
                                                    EditorString &= ("None")
                                                ElseIf FieldValueInteger > (UBound(lookups) + 1) Then
                                                    EditorString &= ("None")
                                                Else
                                                    EditorString &= (lookups(FieldValueInteger - 1))
                                                End If
                                            End If
                                            EditorString &= WhyReadOnlyMsg
                                            '
                                        Case FieldTypeIdMemberSelect
                                            '
                                            ' ----- Member Select ReadOnly
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueInteger = genericController.EncodeInteger(FieldValueObject)
                                            EditorString &= (cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, genericController.encodeText(FieldValueInteger)))
                                            If FieldValueInteger = 0 Then
                                                EditorString &= ("None")
                                            Else
                                                RecordName = cpCore.db.getRecordName("people", FieldValueInteger)
                                                If RecordName = "" Then
                                                    EditorString &= ("No Name")
                                                Else
                                                    EditorString &= (cpCore.html.main_encodeHTML(RecordName))
                                                End If
                                                SelectMessage = "Select from Administrators"
                                                'If .MemberSelectGroupID <> 0 Then
                                                '    GroupName = cpcore.htmldoc.main_GetRecordName("groups", .MemberSelectGroupID)
                                                '    If GroupName <> "" Then
                                                '        SelectMessage = SelectMessage & " and members of " & GroupName
                                                '    End If
                                                'End If
                                                'EditorString &=  ("&nbsp;[<a TabIndex=-1 href=""?cid=" & cpCore.main_GetContentID("groups") & """ target=""_blank"">" & SelectMessage & "</a>]")
                                                EditorString &= ("&nbsp;[<a TabIndex=-1 href=""?af=4&cid=" & Models.Complex.cdefModel.getContentId(cpCore, "people") & "&id=" & FieldValueObject.ToString & """ target=""_blank"">View details in new window</a>]")
                                            End If
                                            EditorString &= WhyReadOnlyMsg
                                            '
                                        Case FieldTypeIdManyToMany
                                            '
                                            '   Placeholder
                                            '
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            MTMContent0 = Models.Complex.cdefModel.getContentNameByID(cpCore, .contentId)
                                            MTMContent1 = Models.Complex.cdefModel.getContentNameByID(cpCore, .manyToManyContentID)
                                            MTMRuleContent = Models.Complex.cdefModel.getContentNameByID(cpCore, .manyToManyRuleContentID)
                                            MTMRuleField0 = .ManyToManyRulePrimaryField
                                            MTMRuleField1 = .ManyToManyRuleSecondaryField
                                            EditorString &= cpCore.html.getCheckList("ManyToMany" & .id, MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1)
                                            'EditorString &= (cpCore.html.getInputCheckListCategories("ManyToMany" & .id, MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, , , True, MTMContent1))
                                            EditorString &= WhyReadOnlyMsg
                                            '
                                        Case FieldTypeIdCurrency
                                            '
                                            ' ----- Currency ReadOnly
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueNumber = genericController.EncodeNumber(FieldValueObject)
                                            EditorString &= (cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, genericController.encodeText(FieldValueNumber)))
                                            EditorString &= (cpCore.html.html_GetFormInputText2(FormFieldLCaseName, CStr(FieldValueNumber), , , , , True, "text"))
                                            EditorString &= (FormatCurrency(FieldValueNumber))
                                            EditorString &= WhyReadOnlyMsg
                                            '
                                        Case FieldTypeIdDate
                                            '
                                            ' ----- date
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueDate = genericController.encodeDateMinValue(genericController.EncodeDate(FieldValueObject))
                                            If FieldValueDate = Date.MinValue Then
                                                FieldValueText = ""
                                            Else
                                                FieldValueText = CStr(FieldValueDate)
                                            End If
                                            EditorString &= (cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, FieldValueText))
                                            EditorString &= (cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, , , , , True, "date"))
                                            EditorString &= WhyReadOnlyMsg
                                            '
                                        Case FieldTypeIdAutoIdIncrement, FieldTypeIdFloat, FieldTypeIdInteger
                                            '
                                            ' ----- number
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            EditorString &= (cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, FieldValueText))
                                            EditorString &= (cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, , , , , True, "number"))
                                            EditorString &= WhyReadOnlyMsg
                                            '
                                        Case FieldTypeIdHTML, FieldTypeIdFileHTML
                                            '
                                            ' ----- HTML types
                                            '
                                            If .htmlContent Then
                                                '
                                                ' edit html as html (see the code)
                                                '
                                                return_NewFieldList = return_NewFieldList & "," & FieldName
                                                FieldValueText = genericController.encodeText(FieldValueObject)
                                                EditorString &= cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, FieldValueText)
                                                EditorStyleModifier = "textexpandable"
                                                FieldRows = (cpCore.userProperty.getInteger(adminContent.Name & "." & FieldName & ".RowHeight", 10))
                                                EditorString &= cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, FieldRows, , FormFieldLCaseName, False, True)
                                            Else
                                                '
                                                ' edit html as wysiwyg
                                                '
                                                return_NewFieldList = return_NewFieldList & "," & FieldName
                                                FieldValueText = genericController.encodeText(FieldValueObject)
                                                EditorString &= cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, FieldValueText)
                                                '
                                                EditorStyleModifier = "text"
                                                FieldRows = (cpCore.userProperty.getInteger(adminContent.Name & "." & FieldName & ".PixelHeight", 500))
                                                'EditorString &=  cpCore.main_GetFormInputHTML(FormFieldLCaseName, FieldValueText)
                                                '
                                                EditorString &= cpCore.html.getFormInputHTML(FormFieldLCaseName, FieldValueText, "500", , True, True, editorAddonListJSON, styleList, styleOptionList)
                                                'innovaEditor = New innovaEditorAddonClassFPO
                                                'EditorString &=  innovaEditor.getInnovaEditor( FormFieldLCaseName, EditorContext, FieldValueText, "", "", True, True, TemplateIDForStyles, emailIdForStyles)
                                                EditorString = "<div style=""width:95%"">" & EditorString & "</div>"
                                                FieldOptionRow = "&nbsp;"
                                            End If
                                        Case FieldTypeIdText, FieldTypeIdLink, FieldTypeIdResourceLink
                                            '
                                            ' ----- FieldTypeText
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            EditorString &= cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, FieldValueText)
                                            If .Password Then
                                                '
                                                ' Password forces simple text box
                                                '
                                                EditorString &= cpCore.html.html_GetFormInputText2(FormFieldLCaseName, "*****", , , , True, True, "password")
                                            Else
                                                '
                                                ' non-password
                                                '
                                                EditorString &= cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, , , , True, "text")
                                            End If
                                        Case FieldTypeIdLongText, FieldTypeIdFileText
                                            '
                                            ' ----- LongText, TextFile
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            EditorString &= cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, FieldValueText)
                                            EditorStyleModifier = "textexpandable"
                                            FieldRows = (cpCore.userProperty.getInteger(adminContent.Name & "." & FieldName & ".RowHeight", 10))
                                            EditorString &= cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, FieldRows, , FormFieldLCaseName, False, True)
                                        Case Else
                                            '
                                            ' ----- Legacy text type -- not used unless something was missed
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            EditorString &= cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, FieldValueText)
                                            If .Password Then
                                                '
                                                ' Password forces simple text box
                                                '
                                                EditorString &= cpCore.html.html_GetFormInputText2(FormFieldLCaseName, "*****", , , , True, True, "password")
                                            ElseIf (Not .htmlContent) Then
                                                '
                                                ' not HTML capable, textarea with resizing
                                                '
                                                If (fieldTypeId = FieldTypeIdText) And (InStr(1, FieldValueText, vbLf) = 0) And (Len(FieldValueText) < 40) Then
                                                    '
                                                    ' text field shorter then 40 characters without a CR
                                                    '
                                                    EditorString &= cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, , , , True, "text")
                                                Else
                                                    '
                                                    ' longer text data, or text that contains a CR
                                                    '
                                                    EditorStyleModifier = "textexpandable"
                                                    EditorString &= cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, 10, , , , True)
                                                End If
                                            ElseIf .htmlContent And FieldPreferenceHTML Then
                                                '
                                                ' HTMLContent true, and prefered
                                                '
                                                EditorStyleModifier = "text"
                                                FieldRows = (cpCore.userProperty.getInteger(adminContent.Name & "." & FieldName & ".PixelHeight", 500))
                                                EditorString &= cpCore.html.getFormInputHTML(FormFieldLCaseName, FieldValueText, "500", , False, True, editorAddonListJSON, styleList, styleOptionList)
                                                'innovaEditor = New innovaEditorAddonClassFPO
                                                'EditorString &=  innovaEditor.getInnovaEditor( FormFieldLCaseName, EditorContext, FieldValueText, "", "", True, True, TemplateIDForStyles, emailIdForStyles)
                                                EditorString = "<div style=""width:95%"">" & EditorString & "</div>"
                                                FieldOptionRow = "&nbsp;"
                                            Else
                                                '
                                                ' HTMLContent true, but text editor selected
                                                '
                                                EditorStyleModifier = "textexpandable"
                                                FieldRows = (cpCore.userProperty.getInteger(adminContent.Name & "." & FieldName & ".RowHeight", 10))
                                                EditorString &= cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, FieldRows, , FormFieldLCaseName, False, True)
                                                'EditorString = cpCore.main_GetFormInputTextExpandable(FormFieldLCaseName, encodeHTML(FieldValueText), FieldRows, "600px", FormFieldLCaseName, False)
                                            End If
                                    End Select
                                Else
                                    '
                                    '--------------------------------------------------------------------------------------------
                                    '   Not Read Only - Display fields as form elements to be modified
                                    '--------------------------------------------------------------------------------------------
                                    '
                                    Select Case fieldTypeId
                                        Case FieldTypeIdBoolean
                                            '
                                            ' ----- Boolean
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueBoolean = genericController.EncodeBoolean(FieldValueObject)
                                            's.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal)
                                            EditorString &= (cpCore.html.html_GetFormInputCheckBox2(FormFieldLCaseName, FieldValueBoolean, , , "checkBox"))
                                            's.Add( "&nbsp;</span></nobr></td>")
                                            '
                                        Case FieldTypeIdFile, FieldTypeIdFileImage
                                            '
                                            ' ----- File
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            'Call s.Add("<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal)
                                            If FieldValueText = "" Then
                                                EditorString &= (cpCore.html.html_GetFormInputFile2(FormFieldLCaseName, , "file"))
                                            Else
                                                NonEncodedLink = cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, FieldValueText)
                                                EncodedLink = genericController.encodeHTML(NonEncodedLink)
                                                Dim filename As String = ""
                                                Dim path As String = ""
                                                cpCore.cdnFiles.splitPathFilename(FieldValueText, path, filename)
                                                EditorString &= ("&nbsp;<a href=""http://" & EncodedLink & """ target=""_blank"">" & SpanClassAdminSmall & "[" & filename & "]</A>")
                                                EditorString &= ("&nbsp;&nbsp;&nbsp;Delete:&nbsp;" & cpCore.html.html_GetFormInputCheckBox2(FormFieldLCaseName & ".DeleteFlag", False))
                                                EditorString &= ("&nbsp;&nbsp;&nbsp;Change:&nbsp;" & cpCore.html.html_GetFormInputFile2(FormFieldLCaseName, , "file"))
                                            End If
                                            '
                                        Case FieldTypeIdLookup
                                            '
                                            ' ----- Lookup
                                            '
                                            FieldValueInteger = genericController.EncodeInteger(FieldValueObject)
                                            LookupContentName = ""
                                            If .lookupContentID <> 0 Then
                                                LookupContentName = genericController.encodeText(Models.Complex.cdefModel.getContentNameByID(cpCore, .lookupContentID))
                                            End If
                                            If LookupContentName <> "" Then
                                                return_NewFieldList = return_NewFieldList & "," & FieldName
                                                If Not .Required Then
                                                    EditorString &= (cpCore.html.main_GetFormInputSelect2(FormFieldLCaseName, FieldValueInteger, LookupContentName, "", "None", "", IsEmptyList, "select"))
                                                Else
                                                    EditorString &= (cpCore.html.main_GetFormInputSelect2(FormFieldLCaseName, FieldValueInteger, LookupContentName, "", "", "", IsEmptyList, "select"))
                                                End If
                                                If FieldValueInteger <> 0 Then
                                                    CSPointer = cpCore.db.cs_open2(LookupContentName, FieldValueInteger, , , "ID")
                                                    If cpCore.db.csOk(CSPointer) Then
                                                        EditorString &= ("&nbsp;[<a TabIndex=-1 href=""?" & RequestNameAdminForm & "=4&cid=" & .lookupContentID & "&id=" & FieldValueObject.ToString & """ target=""_blank"">Details</a>]")
                                                    End If
                                                    Call cpCore.db.csClose(CSPointer)
                                                End If
                                                EditorString &= ("&nbsp;[<a TabIndex=-1 href=""?cid=" & .lookupContentID & """ target=""_blank"">See all " & LookupContentName & "</a>]")
                                            ElseIf .lookupList <> "" Then
                                                return_NewFieldList = return_NewFieldList & "," & FieldName
                                                If Not .Required Then
                                                    EditorString &= cpCore.html.getInputSelectList2(FormFieldLCaseName, FieldValueInteger, .lookupList, "Select One", "", "select")
                                                Else
                                                    EditorString &= cpCore.html.getInputSelectList2(FormFieldLCaseName, FieldValueInteger, .lookupList, "", "", "select")
                                                End If
                                            Else
                                                '
                                                ' -- log exception but dont throw
                                                cpCore.handleException(New ApplicationException("Field [" & FieldName & "] is a Lookup field, but no LookupContent or LookupList has been configured"))
                                                EditorString &= "[Selection not configured]"
                                            End If
                                            '
                                        Case FieldTypeIdMemberSelect
                                            '
                                            ' ----- Member Select
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueInteger = genericController.EncodeInteger(FieldValueObject)
                                            's.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal)
                                            If Not .Required Then
                                                EditorString &= (cpCore.html.getInputMemberSelect(FormFieldLCaseName, FieldValueInteger, .MemberSelectGroupID, "", "None", "select"))
                                            Else
                                                EditorString &= (cpCore.html.getInputMemberSelect(FormFieldLCaseName, FieldValueInteger, .MemberSelectGroupID, "", "", "select"))
                                            End If
                                            If FieldValueInteger <> 0 Then
                                                CSPointer = cpCore.db.cs_open2("people", FieldValueInteger, , , "ID")
                                                If cpCore.db.csOk(CSPointer) Then
                                                    EditorString &= ("&nbsp;[<a TabIndex=-1 href=""?" & RequestNameAdminForm & "=4&cid=" & Models.Complex.cdefModel.getContentId(cpCore, "people") & "&id=" & FieldValueObject.ToString & """ target=""_blank"">Details</a>]")
                                                End If
                                                Call cpCore.db.csClose(CSPointer)
                                            End If
                                            GroupName = cpCore.db.getRecordName("groups", .MemberSelectGroupID)
                                            EditorString &= ("&nbsp;[<a TabIndex=-1 href=""?cid=" & Models.Complex.cdefModel.getContentId(cpCore, "groups") & """ target=""_blank"">Select from members of " & GroupName & "</a>]")
                                            's.Add( "</span></nobr></td>")
                                            '
                                        Case FieldTypeIdManyToMany
                                            '
                                            '   Placeholder
                                            '
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            'Call s.Add("<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal)
                                            '
                                            MTMContent0 = Models.Complex.cdefModel.getContentNameByID(cpCore, .contentId)
                                            MTMContent1 = Models.Complex.cdefModel.getContentNameByID(cpCore, .manyToManyContentID)
                                            MTMRuleContent = Models.Complex.cdefModel.getContentNameByID(cpCore, .manyToManyRuleContentID)
                                            MTMRuleField0 = .ManyToManyRulePrimaryField
                                            MTMRuleField1 = .ManyToManyRuleSecondaryField
                                            EditorString &= cpCore.html.getCheckList("ManyToMany" & .id, MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, , , False, False, FieldValueText)
                                            'EditorString &= (cpCore.html.getInputCheckListCategories("ManyToMany" & .id, MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, , , False, MTMContent1, FieldValueText))
                                        Case FieldTypeIdDate
                                            '
                                            ' ----- Date
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueDate = genericController.encodeDateMinValue(genericController.EncodeDate(FieldValueObject))
                                            If FieldValueDate = Date.MinValue Then
                                                FieldValueText = ""
                                            Else
                                                FieldValueText = CStr(FieldValueDate)
                                            End If
                                            EditorString &= (cpCore.html.html_GetFormInputDate(FormFieldLCaseName, FieldValueText))
                                            's.Add( "&nbsp;</span></nobr></td>")
                                        Case FieldTypeIdAutoIdIncrement, FieldTypeIdCurrency, FieldTypeIdFloat, FieldTypeIdInteger
                                            '
                                            ' ----- Others that simply print
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            's.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal)
                                            If .Password Then
                                                EditorString &= (cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, , , , True, False, "password"))
                                            Else
                                                If (FieldValueText = "") Then
                                                    EditorString &= (cpCore.html.html_GetFormInputText2(FormFieldLCaseName, , , , , , , "text"))
                                                Else
                                                    If CBool(InStr(1, FieldValueText, vbLf)) Or (Len(FieldValueText) > 40) Then
                                                        EditorString &= (cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, , , , , , "text"))
                                                    Else
                                                        EditorString &= (cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, , , , , "text"))
                                                    End If
                                                End If
                                                's.Add( "&nbsp;</span></nobr></td>")
                                            End If
                                            '
                                        Case FieldTypeIdLink
                                            '
                                            ' ----- Link (href value
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            EditorString = "" _
                                                & cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, 80, FormFieldLCaseName, , , "link") _
                                                & "&nbsp;<a href=""#"" onClick=""OpenResourceLinkWindow( '" & FormFieldLCaseName & "' ) ;return false;""><img src=""/ccLib/images/ResourceLink1616.gif"" width=16 height=16 border=0 alt=""Link to a resource"" title=""Link to a resource""></a>" _
                                                & "&nbsp;<a href=""#"" onClick=""OpenSiteExplorerWindow( '" & FormFieldLCaseName & "' ) ;return false;""><img src=""/ccLib/images/PageLink1616.gif"" width=16 height=16 border=0 alt=""Link to a page"" title=""Link to a page""></a>"
                                            's.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal & EditorString & "</span></nobr></td>")
                                        Case FieldTypeIdResourceLink
                                            '
                                            ' ----- Resource Link (src value)
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            EditorString = "" _
                                                & cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, 80, FormFieldLCaseName, , , "resourceLink") _
                                                & "&nbsp;<a href=""#"" onClick=""OpenResourceLinkWindow( '" & FormFieldLCaseName & "' ) ;return false;""><img src=""/ccLib/images/ResourceLink1616.gif"" width=16 height=16 border=0 alt=""Link to a resource"" title=""Link to a resource""></a>"
                                            '
                                        Case FieldTypeIdText
                                            '
                                            ' ----- Text Type
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            If .Password Then
                                                '
                                                ' Password forces simple text box
                                                '
                                                EditorString = cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, , , , True, , "password")
                                            Else
                                                '
                                                ' non-password
                                                '
                                                If (InStr(1, FieldValueText, vbLf) = 0) And (Len(FieldValueText) < 40) Then
                                                    '
                                                    ' text field shorter then 40 characters without a CR
                                                    '
                                                    EditorString = cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, , , , , "text")
                                                Else
                                                    '
                                                    ' longer text data, or text that contains a CR
                                                    '
                                                    EditorStyleModifier = "textexpandable"
                                                    EditorString = cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, 10, , , , , "text")
                                                End If
                                            End If
                                            '
                                        Case FieldTypeIdHTML, FieldTypeIdFileHTML
                                            '
                                            ' content is html
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            '
                                            ' 9/7/2012 -- added this to support:
                                            '   html fields types mean they hold html
                                            '   .htmlContent means edit it with text editor (so you edit the html)
                                            '
                                            If .htmlContent And FieldPreferenceHTML Then
                                                '
                                                ' View the content as Html, not wysiwyg
                                                '
                                                EditorStyleModifier = "textexpandable"
                                                EditorString = cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, 10, , , , , "text")
                                            Else
                                                '
                                                ' wysiwyg editor
                                                '
                                                If FieldValueText = "" Then
                                                    '
                                                    ' editor needs a starting p tag to setup correctly
                                                    '
                                                    FieldValueText = HTMLEditorDefaultCopyNoCr
                                                End If
                                                EditorStyleModifier = "htmleditor"
                                                FieldRows = (cpCore.userProperty.getInteger(adminContent.Name & "." & FieldName & ".PixelHeight", 500))
                                                EditorString &= cpCore.html.getFormInputHTML(FormFieldLCaseName, FieldValueText, "500", , False, True, editorAddonListJSON, styleList, styleOptionList)
                                                'innovaEditor = New innovaEditorAddonClassFPO
                                                'EditorString = innovaEditor.getInnovaEditor( FormFieldLCaseName, EditorContext, FieldValueText, "", "", True, False, TemplateIDForStyles, emailIdForStyles)
                                                EditorString = "<div style=""width:95%"">" & EditorString & "</div>"
                                                FieldOptionRow = "&nbsp;"
                                            End If
                                            '
                                        Case FieldTypeIdLongText, FieldTypeIdFileText
                                            '
                                            ' -- Long Text, use text editor
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            '
                                            EditorStyleModifier = "textexpandable"
                                            FieldRows = (cpCore.userProperty.getInteger(adminContent.Name & "." & FieldName & ".RowHeight", 10))
                                            EditorString = cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, FieldRows, , FormFieldLCaseName, False, , "text")
                                            '
                                        Case FieldTypeIdFileCSS
                                            '
                                            ' ----- CSS field
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            EditorStyleModifier = "textexpandable"
                                            FieldRows = (cpCore.userProperty.getInteger(adminContent.Name & "." & FieldName & ".RowHeight", 10))
                                            EditorString = cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, 10, , , , , "styles")
                                        Case FieldTypeIdFileJavascript
                                            '
                                            ' ----- Javascript field
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            EditorStyleModifier = "textexpandable"
                                            FieldRows = (cpCore.userProperty.getInteger(adminContent.Name & "." & FieldName & ".RowHeight", 10))
                                            EditorString = cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, FieldRows, , FormFieldLCaseName, False, , "text")
                                            '
                                        Case FieldTypeIdFileXML
                                            '
                                            ' ----- xml field
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            EditorStyleModifier = "textexpandable"
                                            FieldRows = (cpCore.userProperty.getInteger(adminContent.Name & "." & FieldName & ".RowHeight", 10))
                                            EditorString = cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, FieldRows, , FormFieldLCaseName, False, , "text")
                                            '
                                        Case Else
                                            '
                                            ' ----- Legacy text type -- not used unless something was missed
                                            '
                                            return_NewFieldList = return_NewFieldList & "," & FieldName
                                            FieldValueText = genericController.encodeText(FieldValueObject)
                                            If .Password Then
                                                '
                                                ' Password forces simple text box
                                                '
                                                EditorString = cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, , , , True, , "password")
                                            ElseIf (Not .htmlContent) Then
                                                '
                                                ' not HTML capable, textarea with resizing
                                                '
                                                If (fieldTypeId = FieldTypeIdText) And (InStr(1, FieldValueText, vbLf) = 0) And (Len(FieldValueText) < 40) Then
                                                    '
                                                    ' text field shorter then 40 characters without a CR
                                                    '
                                                    EditorString = cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, , , , , "text")
                                                Else
                                                    '
                                                    ' longer text data, or text that contains a CR
                                                    '
                                                    EditorStyleModifier = "textexpandable"
                                                    EditorString = cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, 10, , , , , "text")
                                                End If
                                            ElseIf .htmlContent And FieldPreferenceHTML Then
                                                '
                                                ' HTMLContent true, and prefered
                                                '
                                                If FieldValueText = "" Then
                                                    '
                                                    ' editor needs a starting p tag to setup correctly
                                                    '
                                                    FieldValueText = HTMLEditorDefaultCopyNoCr
                                                End If
                                                EditorStyleModifier = "htmleditor"
                                                FieldRows = (cpCore.userProperty.getInteger(adminContent.Name & "." & FieldName & ".PixelHeight", 500))
                                                EditorString &= cpCore.html.getFormInputHTML(FormFieldLCaseName, FieldValueText, "500", , False, True, editorAddonListJSON, styleList, styleOptionList)
                                                'innovaEditor = New innovaEditorAddonClassFPO
                                                'EditorString = innovaEditor.getInnovaEditor( FormFieldLCaseName, EditorContext, FieldValueText, "", "", True, False, TemplateIDForStyles, emailIdForStyles)
                                                EditorString = "<div style=""width:95%"">" & EditorString & "</div>"
                                                FieldOptionRow = "&nbsp;"
                                            Else
                                                '
                                                ' HTMLContent true, but text editor selected
                                                '
                                                EditorStyleModifier = "textexpandable"
                                                FieldRows = (cpCore.userProperty.getInteger(adminContent.Name & "." & FieldName & ".RowHeight", 10))
                                                EditorString = cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, genericController.encodeHTML(FieldValueText), FieldRows, "600px", FormFieldLCaseName, False, , "text")
                                            End If
                                            's.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal & EditorString & "</span></nobr></td>")
                                    End Select
                                End If
                            End If
                            '
                            ' Build Help Line Below editor
                            '
                            includeFancyBox = True
                            FieldHelpFound = False
                            EditorRightSideIcon = ""
                            HelpMsgDefault = ""
                            HelpMsgCustom = ""
                            EditorHelp = ""
                            LcaseName = genericController.vbLCase(.nameLc)
                            If AllowHelpMsgCustom Then
                                HelpMsgDefault = .HelpDefault
                                HelpMsgCustom = .HelpCustom
                                'HelpPtr = helpIdIndex.getPtr(CStr(.id))
                                'If HelpPtr >= 0 Then
                                '    FieldHelpFound = True
                                '    HelpMsgDefault = helpDefaultCache(HelpPtr)
                                '    HelpMsgCustom = HelpCustomCache(HelpPtr)
                                'End If
                            End If
                            '
                            ' 12/4/2016 - REFACTOR - this is from the old system. Delete this after we varify it is no longer needed
                            '
                            'If Not FieldHelpFound Then
                            '    Call getFieldHelpMsgs(adminContent.parentID, .nameLc, HelpMsgDefault, HelpMsgCustom)
                            '    CS = cpCore.app.csInsertRecord("Content Field Help")
                            '    If cpCore.app.csOk(CS) Then
                            '        Call cpCore.app.setCS(CS, "fieldid", fieldId)
                            '        Call cpCore.app.setCS(CS, "name", adminContent.Name & "." & .nameLc)
                            '        Call cpCore.app.setCS(CS, "HelpDefault", HelpMsgDefault)
                            '        Call cpCore.app.setCS(CS, "HelpCustom", HelpMsgCustom)
                            '    End If
                            '    Call cpCore.app.csClose(CS)
                            'End If
                            If HelpMsgCustom <> "" Then
                                HelpMsg = HelpMsgCustom
                            Else
                                HelpMsg = HelpMsgDefault
                            End If
                            HelpMsgOpenedRead = HelpMsg
                            HelpMsgClosed = HelpMsg
                            IsEmptyHelp = Len(HelpMsgClosed) = 0
                            IsLongHelp = (Len(HelpMsgClosed) > 100)
                            If IsLongHelp Then
                                HelpMsgClosed = Left(HelpMsgClosed, 100) & "..."
                            End If
                            '
                            HelpID = "helpId" & fieldId
                            HelpEditorID = "helpEditorId" & fieldId
                            HelpOpenedReadID = "HelpOpenedReadID" & fieldId
                            HelpOpenedEditID = "HelpOpenedEditID" & fieldId
                            HelpClosedID = "helpClosedId" & fieldId
                            HelpClosedContentID = "helpClosedContentId" & fieldId
                            AllowHelpRow = True
                            '
                            '------------------------------------------------------------------------------------------------------------
                            ' editor preferences form - a fancybox popup that interfaces with a hardcoded ajax function in init() to set a member property
                            '------------------------------------------------------------------------------------------------------------
                            '
                            AjaxQS = RequestNameAjaxFunction & "=" & ajaxGetFieldEditorPreferenceForm & "&fieldid=" & fieldId & "&currentEditorAddonId=" & editorAddonID & "&fieldTypeDefaultEditorAddonId=" & fieldTypeDefaultEditorAddonId
                            fancyBoxLinkId = "fbl" & fancyBoxPtr
                            fancyBoxContentId = "fbc" & fancyBoxPtr
                            fancyBoxHeadJS = fancyBoxHeadJS _
                                & vbCrLf _
                                & "jQuery('#" & fancyBoxLinkId & "').fancybox({" _
                                & "'titleShow':false," _
                                & "'transitionIn':'elastic'," _
                                & "'transitionOut':'elastic'," _
                                & "'overlayOpacity':'.2'," _
                                & "'overlayColor':'#000000'," _
                                & "'onStart':function(){cj.ajax.qs('" & AjaxQS & "','','" & fancyBoxContentId & "')}" _
                                & "});"
                            EditorHelp = EditorHelp _
                                & cr & "<div style=""float:right;"">" _
                                & cr2 & "<a id=""" & fancyBoxLinkId & """ href=""#" & fancyBoxContentId & """ title=""select an alternate editor for this field."" tabindex=""-1""><img src=""/ccLib/images/NavAltEditor.gif"" width=18 height=18 border=0 style=""vertical-align:middle;"" title=""select an alternate editor for this field.""></a>" _
                                & cr2 & "<div style=""display:none;"">" _
                                & cr3 & "<div class=""ccEditorPreferenceCon"" id=""" & fancyBoxContentId & """><div style=""margin:20px auto auto auto;""><img src=""/ccLib/images/ajax-loader-big.gif"" width=""32"" height=""32""></div></div>" _
                                & cr2 & "</div>" _
                                & cr & "</div>" _
                                & ""
                            fancyBoxPtr = fancyBoxPtr + 1
                            '
                            '------------------------------------------------------------------------------------------------------------
                            ' field help
                            '------------------------------------------------------------------------------------------------------------
                            '
                            If cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                                '
                                ' Admin view
                                '
                                If HelpMsgDefault = "" Then
                                    HelpMsgDefault = "Admin: No default help is available for this field."
                                End If
                                HelpMsgOpenedRead = "" _
                                        & "<!-- close icon --><div class="""" style=""float:right""><a href=""javascript:cj.hide('" & HelpOpenedReadID & "');cj.show('" & HelpClosedID & "');""><img src=""/ccLib/images/NavHelp.gif"" width=18 height=18 border=0 style=""vertical-align:middle;"" title=""close""></a></div>" _
                                        & "<div class=""header"">Default Help</div>" _
                                        & "<div class=""body"">" & HelpMsgDefault & "</div>" _
                                        & "<div class=""header"">Custom Help</div>" _
                                        & "<div class=""body"">" & HelpMsgCustom & "</div>" _
                                    & ""
                                HelpMsgOpenedEdit = "" _
                                        & "<div class=""header"">Default Help</div>" _
                                        & "<div class=""body"">" & HelpMsgDefault & "</div>" _
                                        & "<div class=""header"">Custom Help</div>" _
                                        & "<div class=""body""><textarea id=""" & HelpEditorID & """ ROWS=""10"" style=""width:100%;"">" & HelpMsgCustom & "</TEXTAREA></div>" _
                                        & "<div class="""">" _
                                            & "<input type=""submit"" name=""button"" value=""Update"" onClick=""updateFieldHelp('" & fieldId & "','" & HelpEditorID & "','" & HelpClosedContentID & "');cj.hide('" & HelpOpenedEditID & "');cj.show('" & HelpClosedID & "');return false"">" _
                                            & "<input type=""submit"" name=""button"" value=""Cancel"" onClick=""cj.hide('" & HelpOpenedEditID & "');cj.show('" & HelpClosedID & "');return false"">" _
                                        & "</div>" _
                                    & ""
                                If IsLongHelp Then
                                    '
                                    ' Long help, closed gets MoreHelpIcon (opens to HelpMsgOpenedRead) and HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                    '
                                    HelpMsgClosed = "" _
                                            & "<!-- open read icon --><div style=""float:right;""><a href=""javascript:cj.hide('" & HelpClosedID & "');cj.show('" & HelpOpenedReadID & "');"" tabindex=""-1""><img src=""/ccLib/images/NavHelp.gif"" width=18 height=18 border=0 style=""vertical-align:middle;"" title=""more help""></a></div>" _
                                            & "<!-- open edit icon --><div style=""float:right;""><a href=""javascript:cj.hide('" & HelpClosedID & "');cj.show('" & HelpOpenedEditID & "');"" tabindex=""-1""><img src=""/ccLib/images/NavHelpEdit.gif"" width=18 height=18 border=0 style=""vertical-align:middle;"" title=""edit help""></a></div>" _
                                            & "<div id=""" & HelpClosedContentID & """>" & HelpMsgClosed & "</div>" _
                                        & ""
                                ElseIf Not IsEmptyHelp Then
                                    '
                                    ' short help, closed gets helpmsgclosed + HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                    '
                                    HelpMsgClosed = "" _
                                            & "<!-- open edit icon --><div style=""float:right;""><a href=""javascript:cj.hide('" & HelpClosedID & "');cj.show('" & HelpOpenedEditID & "');"" tabindex=""-1""><img src=""/ccLib/images/NavHelpEdit.gif"" width=18 height=18 border=0 style=""vertical-align:middle;"" title=""edit help""></a></div>" _
                                            & "<div id=""" & HelpClosedContentID & """>" & HelpMsgClosed & "</div>" _
                                        & ""
                                Else
                                    '
                                    ' Empty help, closed gets helpmsgclosed + HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                    '
                                    HelpMsgClosed = "" _
                                            & "<!-- open edit icon --><div style=""float:right;""><a href=""javascript:cj.hide('" & HelpClosedID & "');cj.show('" & HelpOpenedEditID & "');"" tabindex=""-1""><img src=""/ccLib/images/NavHelpEdit.gif"" width=18 height=18 border=0 style=""vertical-align:middle;"" title=""edit help""></a></div>" _
                                            & "<div id=""" & HelpClosedContentID & """>" & HelpMsgClosed & "</div>" _
                                        & ""
                                End If
                                EditorHelp = EditorHelp _
                                    & "<div id=""" & HelpOpenedReadID & """ class=""opened"">" & HelpMsgOpenedRead & "</div>" _
                                    & "<div id=""" & HelpOpenedEditID & """ class=""opened"">" & HelpMsgOpenedEdit & "</div>" _
                                    & "<div id=""" & HelpClosedID & """ class=""closed"">" & HelpMsgClosed & "</div>" _
                                    & ""
                            Else
                                '
                                ' Non-admin view
                                '
                                HelpMsgOpenedRead = "" _
                                        & "<div class=""body"">" _
                                        & "<!-- close icon --><a href=""javascript:cj.hide('" & HelpOpenedReadID & "');cj.show('" & HelpClosedID & "');""><img src=""/ccLib/images/NavHelp.gif"" width=18 height=18 border=0 style=""vertical-align:middle;float:right"" title=""close""></a>" _
                                        & HelpMsg _
                                        & "</div>" _
                                    & ""
                                'HelpMsgOpenedRead = "" _
                                '        & "<!-- close icon --><div class="""" style=""float:right""><a href=""javascript:cj.hide('" & HelpOpenedReadID & "');cj.show('" & HelpClosedID & "');""><img src=""/ccLib/images/NavHelp.gif"" width=18 height=18 border=0 style=""vertical-align:middle;"" title=""close""></a></div>" _
                                '        & "<div class=""body"">" & HelpMsg & "</div>" _
                                '    & ""
                                HelpMsgOpenedEdit = "" _
                                    & ""
                                If IsLongHelp Then
                                    '
                                    ' Long help
                                    '
                                    HelpMsgClosed = "" _
                                        & "<div class=""body"">" _
                                        & "<!-- open read icon --><a href=""javascript:cj.hide('" & HelpClosedID & "');cj.show('" & HelpOpenedReadID & "');""><img src=""/ccLib/images/NavHelp.gif"" width=18 height=18 border=0 style=""vertical-align:middle;float:right;"" title=""more help""></a>" _
                                        & HelpMsgClosed _
                                        & "</div>" _
                                        & ""
                                ElseIf Not IsEmptyHelp Then
                                    '
                                    ' short help
                                    '
                                    HelpMsgClosed = "" _
                                        & "<div class=""body"">" _
                                            & HelpMsg _
                                        & "</div>" _
                                        & ""
                                Else
                                    '
                                    ' no help
                                    '
                                    AllowHelpRow = False
                                    HelpMsgClosed = "" _
                                        & ""
                                End If
                                EditorHelp = EditorHelp _
                                    & "<div id=""" & HelpOpenedReadID & """ class=""opened"">" & HelpMsgOpenedRead & "</div>" _
                                    & "<div id=""" & HelpClosedID & """ class=""closed"">" & HelpMsgClosed & "</div>" _
                                    & ""
                            End If
                            '
                            ' assemble the help line
                            '
                            s.Add("<tr>" _
                                & "<td class=""ccEditCaptionCon""><div class=""" & EditorStyleModifier & """>" & Caption & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""15"" ></div></td>" _
                                & "<td class=""ccEditFieldCon"">" _
                                & "<div class=""ccEditorCon"">" & EditorString & "</div>")
                            If AllowHelpRow Then
                                s.Add("<div class=""ccEditorHelpCon"">" & EditorHelp & "</div>")
                            End If
                            s.Add("" _
                                & "</td>" _
                                & "</tr>")
                        End With
                    Next
                    '
                    ' ----- add the *Required Fields footer
                    '
                    Call s.Add("" _
                    & "<tr><td colspan=2 style=""padding-top:10px;font-size:70%"">" _
                    & "<div>* Field is required.</div>" _
                    & "<div>** Field must be unique.</div>")
                    If needUniqueEmailMessage Then
                        Call s.Add("<div>*** Field must be unique because this site allows login by email.</div>")
                    End If
                    Call s.Add("</td></tr>")
                    '
                    ' ----- close the panel
                    '
                    If EditTab = "" Then
                        Caption = "Content Fields"
                    Else
                        Caption = "Content Fields - " & EditTab
                    End If
                    EditSectionPanelCount = EditSectionPanelCount + 1
                    returnHtml = Adminui.GetEditPanel((Not allowAdminTabs), Caption, "", Adminui.EditTableOpen & s.Text & Adminui.EditTableClose)
                    EditSectionPanelCount = EditSectionPanelCount + 1
                    s = Nothing
                    'If Return_NewFieldList <> "" Then
                    '    returnHtml = returnHtml & cpCore.main_GetFormInputHidden( "FormFieldList", Mid(Return_NewFieldList, 2))
                    'End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnHtml
        End Function
        '
        '========================================================================
        '   Display field in the admin/edit
        '========================================================================
        '
        Private Function GetForm_Edit_ContentTracking(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_ContentTracking")
            '
            Dim CSRules As Integer
            Dim HTMLFieldString As String
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim CSPointer As Integer
            Dim RecordID As Integer
            Dim ContentID As Integer
            Dim CSLists As Integer
            Dim RecordCount As Integer
            Dim ContentWatchListID As Integer
            Dim FastString As stringBuilderLegacyController
            Dim Copy As String
            Dim Adminui As New adminUIController(cpCore)
            '
            If adminContent.AllowContentTracking Then
                FastString = New stringBuilderLegacyController
                '
                If Not ContentWatchLoaded Then
                    '
                    ' ----- Load in the record to print
                    '
                    Call LoadContentTrackingDataBase(adminContent, editRecord)
                    Call LoadContentTrackingResponse(adminContent, editRecord)
                    '        Call LoadAndSaveCalendarEvents
                End If
                CSLists = cpCore.db.csOpen("Content Watch Lists", "name<>" & cpCore.db.encodeSQLText(""), "ID")
                If cpCore.db.csOk(CSLists) Then
                    '
                    ' ----- Open the panel
                    '
                    'Call cpCore.main_PrintPanelTop("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                    'Call FastString.Add(adminui.EditTableOpen)
                    'Call FastString.Add(vbCrLf & "<tr><td colspan=""3"" class=""ccAdminEditSubHeader"">Content Tracking</td></tr>")
                    '            '
                    '            ' ----- Print matching Content Watch fields
                    '            '
                    '            Call FastString.Add(cpCore.main_GetFormInputHidden("WhatsNewResponse", -1))
                    '            Call FastString.Add(cpCore.main_GetFormInputHidden("contentwatchrecordid", ContentWatchRecordID))
                    '
                    ' ----- Content Watch Lists, checking the ones that have active rules
                    '
                    RecordCount = 0
                    Do While cpCore.db.csOk(CSLists)
                        ContentWatchListID = cpCore.db.csGetInteger(CSLists, "id")
                        '
                        If ContentWatchRecordID <> 0 Then
                            CSRules = cpCore.db.csOpen("Content Watch List Rules", "(ContentWatchID=" & ContentWatchRecordID & ")AND(ContentWatchListID=" & ContentWatchListID & ")")
                            If editRecord.Read_Only Then
                                HTMLFieldString = genericController.encodeText(cpCore.db.csOk(CSRules))
                            Else
                                HTMLFieldString = cpCore.html.html_GetFormInputCheckBox2("ContentWatchList." & cpCore.db.csGet(CSLists, "ID"), cpCore.db.csOk(CSRules))
                            End If
                            Call cpCore.db.csClose(CSRules)
                        Else
                            If editRecord.Read_Only Then
                                HTMLFieldString = genericController.encodeText(False)
                            Else
                                HTMLFieldString = cpCore.html.html_GetFormInputCheckBox2("ContentWatchList." & cpCore.db.csGet(CSLists, "ID"), False)
                            End If
                        End If
                        '
                        Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Include in " & cpCore.db.csGet(CSLists, "name"), "When true, this Content Record can be included in the '" & cpCore.db.csGet(CSLists, "name") & "' list", False, False, ""))
                        Call cpCore.db.csGoNext(CSLists)
                        RecordCount = RecordCount + 1
                    Loop
                    '
                    ' ----- Whats New Headline (editable)
                    '
                    If editRecord.Read_Only Then
                        HTMLFieldString = cpCore.html.main_encodeHTML(ContentWatchLinkLabel)
                    Else
                        HTMLFieldString = cpCore.html.html_GetFormInputText2("ContentWatchLinkLabel", ContentWatchLinkLabel, 1, cpCore.siteProperties.defaultFormInputWidth)
                        'HTMLFieldString = "<textarea rows=""1"" name=""ContentWatchLinkLabel"" cols=""" & cpCore.app.SiteProperty_DefaultFormInputWidth & """>" & ContentWatchLinkLabel & "</textarea>"
                    End If
                    Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Caption", "This caption is displayed on all Content Watch Lists, linked to the location on the web site where this content is displayed. RSS feeds created from Content Watch Lists will use this caption as the record title if not other field is selected in the Content Definition.", False, True, "ContentWatchLinkLabel"))
                    '
                    ' ----- Whats New Expiration
                    '
                    Copy = ContentWatchExpires.ToString
                    If Copy = "12:00:00 AM" Then
                        Copy = ""
                    End If
                    If editRecord.Read_Only Then
                        HTMLFieldString = cpCore.html.main_encodeHTML(Copy)
                    Else
                        HTMLFieldString = cpCore.html.html_GetFormInputDate("ContentWatchExpires", Copy, cpCore.siteProperties.defaultFormInputWidth.ToString)
                        'HTMLFieldString = "<textarea rows=""1"" name=""ContentWatchExpires"" cols=""" & cpCore.app.SiteProperty_DefaultFormInputWidth & """>" & Copy & "</textarea>"
                    End If
                    Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Expires", "When this record is included in a What's New list, this record is blocked from the list after this date.", False, False, ""))
                    '
                    ' ----- Public Link (read only)
                    '
                    HTMLFieldString = ContentWatchLink
                    If HTMLFieldString = "" Then
                        HTMLFieldString = "(must first be viewed on public site)"
                    End If
                    Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Location on Site", "The public site URL where this content was last viewed.", False, False, ""))
                    '
                    ' removed 11/27/07 - RSS clicks not counted, rc/ri method of counting link clicks is not reliable.
                    '            '
                    '            ' ----- Clicks (read only)
                    '            '
                    '            HTMLFieldString = ContentWatchClicks
                    '            If HTMLFieldString = "" Then
                    '                HTMLFieldString = 0
                    '                End If
                    '            Call FastString.Add(AdminUI.GetEditRow( HTMLFieldString, "Clicks", "The number of site users who have clicked this link in what's new lists", False, False, ""))
                    '
                    ' ----- close the panel
                    '
                    Dim s As String
                    s = "" _
                        & Adminui.EditTableOpen _
                        & FastString.Text _
                        & Adminui.EditTableClose _
                        & cpCore.html.html_GetFormInputHidden("WhatsNewResponse", "-1") _
                        & cpCore.html.html_GetFormInputHidden("contentwatchrecordid", ContentWatchRecordID.ToString)
                    GetForm_Edit_ContentTracking = Adminui.GetEditPanel((Not allowAdminTabs), "Content Tracking", "Include in Content Watch Lists", s)
                    EditSectionPanelCount = EditSectionPanelCount + 1
                    '
                End If
                Call cpCore.db.csClose(CSLists)
                FastString = Nothing
            End If
            '
            Exit Function
            '
ErrorTrap:
            FastString = Nothing
            Call handleLegacyClassError3("GetForm_Edit_ContentTracking")
        End Function
        '
        '========================================================================
        '   Display field in the admin/edit
        '========================================================================
        '
        Private Function GetForm_Edit_Control(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_Control")
            '
            Dim s As String
            Dim AllowEID As Boolean
            Dim EID As String
            Dim IsEmptyList As Boolean
            Dim ParentID As Integer
            Dim ParentCID As Integer
            Dim Criteria As String
            Dim LimitContentSelectToThisID As Integer
            Dim SQL As String
            Dim TableID As Integer
            Dim TableName As Integer
            Dim ChildCID As Integer
            Dim CIDList As String = ""
            Dim TableName2 As String
            Dim RecordContentName As String
            Dim ContentSupportsParentID As Boolean
            Dim CS As Integer
            Dim HTMLFieldString As String
            'Dim FieldPtr As Integer
            Dim CSPointer As Integer
            Dim RecordID As Integer
            Dim hiddenInputs As String = ""
            Dim FastString As stringBuilderLegacyController
            Dim FieldValueInteger As Integer
            Dim FieldRequired As Boolean
            Dim FieldHelp As String
            Dim AuthoringStatusMessage As String
            Dim Delimiter As String
            Dim Copy As String
            Dim Adminui As New adminUIController(cpCore)
            '''Dim arrayOfFields() As appServices_metaDataClass.CDefFieldClass
            '
            FastString = New stringBuilderLegacyController
            '
            'arrayOfFields = adminContent.fields
            With adminContent
                '
                ' ----- test admin content before using it
                '
                If .Name = "" Then
                    '
                    ' Content not found or not loaded
                    '
                    If .Id = 0 Then
                        '
                        ' Content Definition was not found
                        '
                        Call handleLegacyClassError("GetForm_Edit_Control", "No content definition was specified for this page")
                    Else
                        '
                        ' Content Definition was not specified
                        '
                        Call handleLegacyClassError("GetForm_Edit_Control", "The content definition specified for this page [" & .Id & "] was not found")
                    End If
                Else
                End If
                '
                Dim Checked As Boolean
                ''
                '' ----- Authoring status
                ''
                'FieldHelp = "In immediate authoring mode, the live site is changed when each record is saved. In Workflow authoring mode, there are several steps to publishing a change. This field displays the current stage of this record."
                'FieldRequired = False
                'AuthoringStatusMessage = cpCore.doc.authContext.main_GetAuthoringStatusMessage(cpCore, false, editRecord.EditLock, editRecord.EditLockMemberName, editRecord.EditLockExpires, editRecord.ApproveLock, editRecord.ApprovedName, editRecord.SubmitLock, editRecord.SubmittedName, editRecord.IsDeleted, editRecord.IsInserted, editRecord.IsModified, editRecord.LockModifiedName)
                'Call FastString.Add(Adminui.GetEditRow(AuthoringStatusMessage, "Authoring Status", FieldHelp, FieldRequired, False, ""))
                ''Call FastString.Add(AdminUI.GetEditRow( AuthoringStatusMessage, "Authoring Status", FieldHelp, FieldRequired, False, ""))
                '
                ' ----- RecordID
                '
                FieldHelp = "This is the unique number that identifies this record within this content."
                If editRecord.id = 0 Then
                    HTMLFieldString = "(available after save)"
                Else
                    HTMLFieldString = genericController.encodeText(editRecord.id)
                End If
                HTMLFieldString = cpCore.html.html_GetFormInputText2("ignore", HTMLFieldString, , , , , True)
                Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Record Number", FieldHelp, True, False, ""))
                '
                ' -- Active
                Copy = "When unchecked, add-ons can ignore this record as if it was temporarily deleted."
                HTMLFieldString = cpCore.html.html_GetFormInputCheckBox2("active", editRecord.active)
                Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Active", Copy, False, False, ""))
                '
                ' ----- If Page Content , check if this is the default PageNotFound page
                '
                If genericController.vbLCase(adminContent.ContentTableName) = "ccpagecontent" Then
                    '
                    ' Landing Page
                    '
                    Copy = "If selected, this page will be displayed when a user comes to your website with just your domain name and no other page is requested. This is called your default Landing Page. Only one page on the site can be the default Landing Page. If you want a unique Landing Page for a specific domain name, add it in the 'Domains' content and the default will not be used for that docpCore.main_"
                    Checked = ((editRecord.id <> 0) And (editRecord.id = (cpCore.siteProperties.getinteger("LandingPageID", 0))))
                    If cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                        HTMLFieldString = cpCore.html.html_GetFormInputCheckBox2("LandingPageID", Checked)
                    Else
                        HTMLFieldString = "<b>" & genericController.GetYesNo(Checked) & "</b>" & cpCore.html.html_GetFormInputHidden("LandingPageID", Checked)
                    End If
                    HTMLFieldString = HTMLFieldString
                    Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Set Default Landing Page", Copy, False, False, ""))
                    '
                    ' Page Not Found
                    '
                    Copy = "If selected, this content will be displayed when a page can not be found. Only one page on the site can be marked."
                    Checked = ((editRecord.id <> 0) And (editRecord.id = (cpCore.siteProperties.getinteger("PageNotFoundPageID", 0))))
                    If cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                        HTMLFieldString = cpCore.html.html_GetFormInputCheckBox2("PageNotFound", Checked)
                    Else
                        HTMLFieldString = "<b>" & genericController.GetYesNo(Checked) & "</b>" & cpCore.html.html_GetFormInputHidden("PageNotFound", Checked)
                    End If
                    '            If (EditRecord.ID <> 0) And (EditRecord.ID = cpCore.main_GetSiteProperty2("PageNotFoundPageID", "0", True)) Then
                    '                HTMLFieldString = cpCore.main_GetFormInputCheckBox2("PageNotFound", True)
                    '            Else
                    '                HTMLFieldString = cpCore.main_GetFormInputCheckBox2("PageNotFound", False)
                    '            End If
                    HTMLFieldString = HTMLFieldString
                    Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Set Page Not Found", Copy, False, False, ""))
                End If
                '
                ' ----- Last Known Public Site URL
                '
                If (UCase(adminContent.ContentTableName) = "CCPAGECONTENT") Or (UCase(adminContent.ContentTableName) = "ITEMS") Then
                    FieldHelp = "This is the URL where this record was last displayed on the site. It may be blank if the record has not been displayed yet."
                    Copy = cpCore.doc.getContentWatchLinkByKey(editRecord.contentControlId & "." & editRecord.id, , False)
                    If Copy = "" Then
                        HTMLFieldString = "unknown"
                    Else
                        HTMLFieldString = "<a href=""" & genericController.encodeHTML(Copy) & """ target=""_blank"">" & Copy & "</a>"
                    End If
                    Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Last Known Public URL", FieldHelp, False, False, ""))
                End If
                '
                ' ----- Widget Code
                '
                If genericController.vbLCase(adminContent.ContentTableName) = "ccaggregatefunctions" Then
                    '
                    ' ----- Add-ons
                    '
                    Dim AllowWidget As Boolean
                    AllowWidget = False
                    If editRecord.fieldsLc.ContainsKey("remotemethod") Then
                        AllowWidget = genericController.EncodeBoolean(editRecord.fieldsLc.Item("remotemethod").value)
                    End If
                    If Not AllowWidget Then
                        FieldHelp = "If you wish to use this add-on as a widget, enable 'Is Remote Method' on the 'Placement' tab and save the record. The necessary html code, or 'embed code' will be created here for you to cut-and-paste into the website."
                        HTMLFieldString = ""
                        HTMLFieldString = cpCore.html.html_GetFormInputTextExpandable2("ignore", HTMLFieldString, 1, , , , True)
                        Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Widget Code", FieldHelp, True, False, ""))
                    Else
                        FieldHelp = "If you wish to use this add-on as a widget, cut and paste the 'Widget Code' into the website content. If any code appears in the 'Widget Head', this will need to be pasted into the head section of the website."
                        HTMLFieldString = "" _
                            & "<SCRIPT type=text/javascript>" _
                            & vbCrLf & "var ccProto=(('https:'==document.location.protocol) ? 'https://' : 'http://');" _
                            & vbCrLf & "document.write(unescape(""%3Cscript src='"" + ccProto + """ & cpCore.webServer.requestDomain & "/ccLib/ClientSide/Core.js' type='text/javascript'%3E%3C/script%3E""));" _
                            & vbCrLf & "document.write(unescape(""%3Cscript src='"" + ccProto + """ & cpCore.webServer.requestDomain & "/" & genericController.EncodeURL(editRecord.nameLc) & "?requestjsform=1' type='text/javascript'%3E%3C/script%3E""));" _
                            & vbCrLf & "</SCRIPT>"
                        '<SCRIPT type=text/javascript>
                        'var gaJsHost = (("https:" == document.location.protocol) ? "https://ssl." : "http://www.");
                        'document.write(unescape("%3Cscript src='" + gaJsHost + "google-analytics.com/ga.js' type='text/javascript'%3E%3C/script%3E"));
                        '</SCRIPT>
                        '                HTMLFieldString = "" _
                        '                    & "<script language=""javascript"" type=""text/javascript"" src=""http://" & cpCore.main_ServerDomain & "/ccLib/ClientSide/Core.js""></script>" _
                        '                    & vbCrLf & "<script language=""javascript"" type=""text/javascript"" src=""http://" & cpCore.main_ServerDomain & "/" & EditRecord.Name & "?requestjsform=1""></script>" _
                        '                    & ""
                        HTMLFieldString = cpCore.html.html_GetFormInputTextExpandable2("ignore", HTMLFieldString, 8)
                        Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Widget Code", FieldHelp, True, False, ""))
                    End If
                End If
                '
                ' ----- GUID
                '
                If editRecord.fieldsLc.ContainsKey("ccguid") Then
                    Dim contentField As Models.Complex.CDefFieldModel = adminContent.fields.Item("ccguid")
                    HTMLFieldString = genericController.encodeText(editRecord.fieldsLc.Item("ccguid").value)
                    FieldHelp = "This is a unique number that identifies this record globally. A GUID is not required, but when set it should never be changed. GUIDs are used to synchronize records. When empty, you can create a new guid. Only Developers can modify the guid."
                    If HTMLFieldString = "" Then
                        '
                        ' add a set button
                        '
                        Dim ccGuid As String
                        ccGuid = "{" & Guid.NewGuid.ToString() & "}"
                        HTMLFieldString = cpCore.html.html_GetFormInputText2("ccguid", HTMLFieldString, , , "ccguid", , False) & "<input type=button value=set onclick=""var e=document.getElementById('ccguid');e.value='" & ccGuid & "';this.disabled=true;"">"
                    Else
                        '
                        ' field is read-only except for developers
                        '
                        If cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore) Then
                            HTMLFieldString = cpCore.html.html_GetFormInputText2("ccguid", HTMLFieldString, , , , , False) & ""
                        Else
                            HTMLFieldString = cpCore.html.html_GetFormInputText2("ccguid", HTMLFieldString, , , , , True) & cpCore.html.html_GetFormInputHidden("ccguid", HTMLFieldString)
                        End If
                    End If
                    Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "GUID", FieldHelp, False, False, ""))
                End If
                '
                ' ----- EID (Encoded ID)
                '
                FieldHelp = ""
                If genericController.vbUCase(adminContent.ContentTableName) = genericController.vbUCase("ccMembers") Then
                    AllowEID = (cpCore.siteProperties.getBoolean("AllowLinkLogin", True)) Or (cpCore.siteProperties.getBoolean("AllowLinkRecognize", True))
                    If (Not AllowEID) Then
                        HTMLFieldString = "(link login and link recognize are disabled in security preferences)"
                    ElseIf editRecord.id = 0 Then
                        HTMLFieldString = "(available after save)"
                    Else
                        EID = genericController.encodeText(cpCore.security.encodeToken(editRecord.id, cpCore.doc.profileStartTime))
                        If (cpCore.siteProperties.getBoolean("AllowLinkLogin", True)) Then
                            HTMLFieldString = EID
                            'HTMLFieldString = EID _
                            '            & "<div>Any visitor who hits the site with eid=" & EID & " will be logged in as this member.</div>"
                            FieldHelp = "Any visitor who hits the site with eid=" & EID & " will be logged in as this member."
                        Else
                            FieldHelp = "Any visitor who hits the site with eid=" & EID & " will be recognized as this member, but not logged in."
                            HTMLFieldString = EID
                            'HTMLFieldString = EID _
                            '            & "<div>Any visitor who hits the site with eid=" & EID & " will be recognized as this member, but not logged in</div>"
                        End If
                        FieldHelp = FieldHelp & " To enable, disable or modify this feature, use the security tab on the Preferences page."
                    End If
                    HTMLFieldString = cpCore.html.html_GetFormInputText2("ignore", HTMLFieldString)
                    Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Member Link Login EID", FieldHelp, True, False, ""))
                End If
                '
                ' ----- Controlling Content
                '
                HTMLFieldString = ""
                FieldHelp = "The content in which this record is stored. This is similar to a database table."
                Dim field As Models.Complex.CDefFieldModel
                If adminContent.fields.ContainsKey("contentcontrolid") Then
                    field = adminContent.fields("contentcontrolid")
                    With field
                        '
                        ' if this record has a parent id, only include CDefs compatible with the parent record - otherwise get all for the table
                        '
                        FieldHelp = genericController.encodeText(.HelpMessage)
                        FieldRequired = genericController.EncodeBoolean(.Required)
                        FieldValueInteger = editRecord.contentControlId
                        '
                        '
                        '
                        If Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                            HTMLFieldString = HTMLFieldString & cpCore.html.html_GetFormInputHidden("ContentControlID", FieldValueInteger)
                        Else
                            RecordContentName = editRecord.contentControlId_Name
                            Dim RecordCDef As Models.Complex.cdefModel
                            TableName2 = Models.Complex.cdefModel.getContentTablename(cpCore, RecordContentName)
                            TableID = cpCore.db.getRecordID("Tables", TableName2)
                            '
                            ' Test for parentid
                            '
                            ParentID = 0
                            ContentSupportsParentID = False
                            If editRecord.id > 0 Then
                                CS = cpCore.db.csOpenRecord(RecordContentName, editRecord.id)
                                If cpCore.db.csOk(CS) Then
                                    ContentSupportsParentID = cpCore.db.cs_isFieldSupported(CS, "ParentID")
                                    If ContentSupportsParentID Then
                                        ParentID = cpCore.db.csGetInteger(CS, "ParentID")
                                    End If
                                End If
                                Call cpCore.db.csClose(CS)
                            End If
                            '
                            LimitContentSelectToThisID = 0
                            If ContentSupportsParentID Then
                                '
                                ' Parentid - restrict CDefs to those compatible with the parentid
                                '
                                If ParentID <> 0 Then
                                    '
                                    ' This record has a parent, set LimitContentSelectToThisID to the parent's CID
                                    '
                                    CSPointer = cpCore.db.csOpenRecord(RecordContentName, ParentID, , , "ContentControlID")
                                    If cpCore.db.csOk(CSPointer) Then
                                        LimitContentSelectToThisID = cpCore.db.csGetInteger(CSPointer, "ContentControlID")
                                    End If
                                    Call cpCore.db.csClose(CSPointer)
                                End If

                            End If
                            If cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) And (LimitContentSelectToThisID = 0) Then
                                '
                                ' administrator, and either ( no parentid or does not support it), let them select any content compatible with the table
                                '
                                HTMLFieldString = HTMLFieldString & cpCore.html.main_GetFormInputSelect2("ContentControlID", FieldValueInteger, "Content", "ContentTableID=" & TableID, "", "", IsEmptyList)
                                FieldHelp = FieldHelp & " (Only administrators have access to this control. Changing the Controlling Content allows you to change who can author the record, as well as how it is edited.)"
                            Else
                                '
                                ' Limit the list to only those cdefs that are within the record's parent contentid
                                '
                                RecordContentName = editRecord.contentControlId_Name
                                TableName2 = Models.Complex.cdefModel.getContentTablename(cpCore, RecordContentName)
                                TableID = cpCore.db.getRecordID("Tables", TableName2)
                                CSPointer = cpCore.db.csOpen("Content", "ContentTableID=" & TableID, , , , , , "ContentControlID")
                                Do While cpCore.db.csOk(CSPointer)
                                    ChildCID = cpCore.db.csGetInteger(CSPointer, "ID")
                                    If (Models.Complex.cdefModel.isWithinContent(cpCore, ChildCID, LimitContentSelectToThisID)) Then
                                        If (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) Or (cpCore.doc.authContext.isAuthenticatedContentManager(cpCore, Models.Complex.cdefModel.getContentNameByID(cpCore, ChildCID))) Then
                                            CIDList = CIDList & "," & ChildCID
                                        End If
                                    End If
                                    cpCore.db.csGoNext(CSPointer)
                                Loop
                                Call cpCore.db.csClose(CSPointer)
                                If CIDList <> "" Then
                                    CIDList = Mid(CIDList, 2)
                                    HTMLFieldString = cpCore.html.main_GetFormInputSelect2("ContentControlID", FieldValueInteger, "Content", "id in (" & CIDList & ")", "", "", IsEmptyList)
                                    FieldHelp = FieldHelp & " (Only administrators have access to this control. Changing the Controlling Content allows you to change who can author the record, as well as how it is edited. This record includes a Parent field, so your choices for controlling content are limited to those compatible with the parent of this record.)"
                                End If
                            End If
                        End If
                    End With
                End If
                If HTMLFieldString = "" Then
                    HTMLFieldString = editRecord.contentControlId_Name
                    'HTMLFieldString = models.complex.cdefmodel.getContentNameByID(cpcore,EditRecord.ContentID)
                End If
                Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Controlling Content", FieldHelp, FieldRequired, False, ""))
                '
                ' ----- Created By
                '
                FieldHelp = "The people account of the user who created this record."
                If editRecord.id = 0 Then
                    HTMLFieldString = "(available after save)"
                Else
                    FieldValueInteger = editRecord.createByMemberId
                    If FieldValueInteger = 0 Then
                        HTMLFieldString = "unknown"
                    Else
                        CSPointer = cpCore.db.cs_open2("people", FieldValueInteger, True)
                        If Not cpCore.db.csOk(CSPointer) Then
                            HTMLFieldString = "unknown"
                        Else
                            HTMLFieldString = cpCore.db.csGet(CSPointer, "name")
                        End If
                        Call cpCore.db.csClose(CSPointer)
                    End If
                End If
                HTMLFieldString = cpCore.html.html_GetFormInputText2("ignore", HTMLFieldString, , , , , True)
                Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Created By", FieldHelp, FieldRequired, False, ""))
                '
                ' ----- Created Date
                '
                FieldHelp = "The date and time when this record was originally created."
                If editRecord.id = 0 Then
                    HTMLFieldString = "(available after save)"
                Else
                    HTMLFieldString = genericController.encodeText(genericController.EncodeDate(editRecord.dateAdded))
                    If HTMLFieldString = "12:00:00 AM" Then
                        HTMLFieldString = "unknown"
                    End If
                End If
                HTMLFieldString = cpCore.html.html_GetFormInputText2("ignore", HTMLFieldString, , , , , True)
                Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Created Date", FieldHelp, FieldRequired, False, ""))
                '
                ' ----- Modified By
                '
                FieldHelp = "The people account of the last user who modified this record."
                If editRecord.id = 0 Then
                    HTMLFieldString = "(available after save)"
                Else
                    FieldValueInteger = editRecord.modifiedByMemberID
                    HTMLFieldString = "unknown"
                    If FieldValueInteger > 0 Then
                        CSPointer = cpCore.db.cs_open2("people", FieldValueInteger, True, , "name")
                        If cpCore.db.csOk(CSPointer) Then
                            HTMLFieldString = cpCore.db.csGet(CSPointer, "name")
                        End If
                        Call cpCore.db.csClose(CSPointer)
                    End If
                End If
                HTMLFieldString = cpCore.html.html_GetFormInputText2("ignore", HTMLFieldString, , , , , True)
                Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Modified By", FieldHelp, FieldRequired, False, ""))
                '
                ' ----- Modified Date
                '
                FieldHelp = "The date and time when this record was last modified"
                If editRecord.id = 0 Then
                    HTMLFieldString = "(available after save)"
                Else
                    HTMLFieldString = genericController.encodeText(genericController.EncodeDate(editRecord.modifiedDate))
                    If HTMLFieldString = "12:00:00 AM" Then
                        HTMLFieldString = "unknown"
                    End If
                End If
                HTMLFieldString = cpCore.html.html_GetFormInputText2("ignore", HTMLFieldString, , , , , True)
                Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Modified Date", FieldHelp, False, False, ""))
            End With
            s = "" _
                & Adminui.EditTableOpen _
                & FastString.Text _
                & Adminui.EditTableClose _
                & hiddenInputs _
                & ""
            GetForm_Edit_Control = Adminui.GetEditPanel((Not allowAdminTabs), "Control Information", "", s)
            EditSectionPanelCount = EditSectionPanelCount + 1
            FastString = Nothing
            '
            Exit Function
            '
ErrorTrap:
            FastString = Nothing
            Call handleLegacyClassError3("GetForm_Edit_Control")
        End Function
        '
        '========================================================================
        '   Display field in the admin/edit
        '========================================================================
        '
        Private Function GetForm_Edit_SiteProperties(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_SiteProperties")
            '
            Dim ExpandedSelector As String = ""
            Dim ignore As String = ""
            Dim OptionCaption As String
            Dim OptionValue As String
            Dim OptionValue_AddonEncoded As String
            Dim OptionPtr As Integer
            Dim OptionCnt As Integer
            Dim OptionValues() As String
            Dim OptionSuffix As String = ""
            Dim LCaseOptionDefault As String
            Dim Pos As Integer
            Dim Checked As Boolean
            Dim ParentID As Integer
            Dim ParentCID As Integer
            Dim Criteria As String
            Dim RootCID As Integer
            Dim SQL As String
            Dim TableID As Integer
            Dim TableName As Integer
            Dim ChildCID As Integer
            Dim CIDList As String
            Dim TableName2 As String
            Dim RecordContentName As String
            Dim HasParentID As Boolean
            Dim CS As Integer
            Dim HTMLFieldString As String
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim CSPointer As Integer
            Dim RecordID As Integer
            Dim FastString As stringBuilderLegacyController
            Dim FieldValueInteger As Integer
            Dim FieldRequired As Boolean
            Dim FieldHelp As String
            Dim AuthoringStatusMessage As String
            Dim Delimiter As String
            Dim Copy As String = ""
            Dim Adminui As New adminUIController(cpCore)
            '
            Dim FieldPtr As Integer
            Dim SitePropertyName As String
            Dim SitePropertyValue As String
            Dim selector As String
            Dim FieldName As String
            '
            FastString = New stringBuilderLegacyController
            '
            SitePropertyName = ""
            SitePropertyValue = ""
            selector = ""
            For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In adminContent.fields
                Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                '
                FieldName = field.nameLc
                If genericController.vbLCase(FieldName) = "name" Then
                    SitePropertyName = genericController.encodeText(editRecord.fieldsLc(field.nameLc).value)
                ElseIf (LCase(FieldName) = "fieldvalue") Then
                    SitePropertyValue = genericController.encodeText(editRecord.fieldsLc(field.nameLc).value)
                ElseIf (LCase(FieldName) = "selector") Then
                    selector = genericController.encodeText(editRecord.fieldsLc(field.nameLc).value)
                End If
            Next
            If SitePropertyName = "" Then
                HTMLFieldString = "This Site Property is not defined"
            Else
                HTMLFieldString = cpCore.html.html_GetFormInputHidden("name", SitePropertyName)
                Dim instanceOptions As New Dictionary(Of String, String)
                Dim addonInstanceProperties As New Dictionary(Of String, String)
                instanceOptions.Add(SitePropertyName, SitePropertyValue)
                Call cpCore.addon.buildAddonOptionLists(addonInstanceProperties, ExpandedSelector, SitePropertyName & "=" & selector, instanceOptions, "0", True)

                '--------------

                Pos = genericController.vbInstr(1, ExpandedSelector, "[")
                If Pos <> 0 Then
                    '
                    ' List of Options, might be select, radio or checkbox
                    '
                    LCaseOptionDefault = genericController.vbLCase(Mid(ExpandedSelector, 1, Pos - 1))
                    LCaseOptionDefault = genericController.decodeNvaArgument(LCaseOptionDefault)

                    ExpandedSelector = Mid(ExpandedSelector, Pos + 1)
                    Pos = genericController.vbInstr(1, ExpandedSelector, "]")
                    If Pos > 0 Then
                        If Pos < Len(ExpandedSelector) Then
                            OptionSuffix = genericController.vbLCase(Trim(Mid(ExpandedSelector, Pos + 1)))
                        End If
                        ExpandedSelector = Mid(ExpandedSelector, 1, Pos - 1)
                    End If
                    OptionValues = Split(ExpandedSelector, "|")
                    HTMLFieldString = ""
                    OptionCnt = UBound(OptionValues) + 1
                    For OptionPtr = 0 To OptionCnt - 1
                        OptionValue_AddonEncoded = Trim(OptionValues(OptionPtr))
                        If OptionValue_AddonEncoded <> "" Then
                            Pos = genericController.vbInstr(1, OptionValue_AddonEncoded, ":")
                            If Pos = 0 Then
                                OptionValue = genericController.decodeNvaArgument(OptionValue_AddonEncoded)
                                OptionCaption = OptionValue
                            Else
                                OptionCaption = genericController.decodeNvaArgument(Mid(OptionValue_AddonEncoded, 1, Pos - 1))
                                OptionValue = genericController.decodeNvaArgument(Mid(OptionValue_AddonEncoded, Pos + 1))
                            End If
                            Select Case OptionSuffix
                                Case "checkbox"
                                    '
                                    ' Create checkbox HTMLFieldString
                                    '
                                    If genericController.vbInstr(1, "," & LCaseOptionDefault & ",", "," & genericController.vbLCase(OptionValue) & ",") <> 0 Then
                                        HTMLFieldString = HTMLFieldString & "<div style=""white-space:nowrap""><input type=""checkbox"" name=""" & SitePropertyName & OptionPtr & """ value=""" & OptionValue & """ checked=""checked"">" & OptionCaption & "</div>"
                                    Else
                                        HTMLFieldString = HTMLFieldString & "<div style=""white-space:nowrap""><input type=""checkbox"" name=""" & SitePropertyName & OptionPtr & """ value=""" & OptionValue & """ >" & OptionCaption & "</div>"
                                    End If
                                Case "radio"
                                    '
                                    ' Create Radio HTMLFieldString
                                    '
                                    If genericController.vbLCase(OptionValue) = LCaseOptionDefault Then
                                        HTMLFieldString = HTMLFieldString & "<div style=""white-space:nowrap""><input type=""radio"" name=""" & SitePropertyName & """ value=""" & OptionValue & """ checked=""checked"" >" & OptionCaption & "</div>"
                                    Else
                                        HTMLFieldString = HTMLFieldString & "<div style=""white-space:nowrap""><input type=""radio"" name=""" & SitePropertyName & """ value=""" & OptionValue & """ >" & OptionCaption & "</div>"
                                    End If
                                Case Else
                                    '
                                    ' Create select HTMLFieldString
                                    '
                                    If genericController.vbLCase(OptionValue) = LCaseOptionDefault Then
                                        HTMLFieldString = HTMLFieldString & "<option value=""" & OptionValue & """ selected>" & OptionCaption & "</option>"
                                    Else
                                        HTMLFieldString = HTMLFieldString & "<option value=""" & OptionValue & """>" & OptionCaption & "</option>"
                                    End If
                            End Select
                        End If
                    Next
                    Select Case OptionSuffix
                        Case "checkbox"
                            '
                            ' FormID-SitePropertyName-Cnt is the count of Options used with checkboxes
                            ' This is used
                            '
                            Copy = Copy & "<input type=""hidden"" name=""" & SitePropertyName & "CheckBoxCnt"" value=""" & OptionCnt & """ >"
                        Case "radio"
                            '
                            ' Create Radio HTMLFieldString
                            '
                            'HTMLFieldString = "<div>" & genericController.vbReplace(HTMLFieldString, "><", "></div><div><") & "</div>"
                        Case Else
                            '
                            ' Create select HTMLFieldString
                            '
                            HTMLFieldString = "<select name=""" & SitePropertyName & """>" & HTMLFieldString & "</select>"
                    End Select
                Else
                    '
                    ' Create Text HTMLFieldString
                    '

                    selector = genericController.decodeNvaArgument(selector)
                    HTMLFieldString = cpCore.html.html_GetFormInputText2(SitePropertyName, selector, 1, 20)
                End If
                '--------------

                'HTMLFieldString = cpCore.main_GetFormInputText2( genericController.vbLCase(FieldName), VAlue)
            End If
            Call FastString.Add(Adminui.GetEditRow(HTMLFieldString, SitePropertyName, "", False, False, ""))
            GetForm_Edit_SiteProperties = Adminui.GetEditPanel((Not allowAdminTabs), "Control Information", "", Adminui.EditTableOpen & FastString.Text & Adminui.EditTableClose)
            EditSectionPanelCount = EditSectionPanelCount + 1
            FastString = Nothing
            Exit Function
            '
ErrorTrap:
            FastString = Nothing
            Call handleLegacyClassError3("GetForm_Edit_SiteProperties")
        End Function
        '
    End Class
End Namespace
