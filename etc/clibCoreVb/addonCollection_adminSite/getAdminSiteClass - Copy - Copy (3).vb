
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
        ' ProcessActions
        '   perform the action called from the previous form
        '   when action is complete, replace the action code with one that will refresh
        '
        '   Request Variables
        '       ID = ID of record to edit
        '       AdminAction = action to be performed, defined below, required except for very first call to edit
        '   AdminAction Definitions
        '       edit - edit the record defined by ID, If ID="", edit a new record
        '       Save - saves an edit record and returns to the index
        '       Delete - hmmm.
        '       Cancel - returns to index
        '       Change Filex - uploads a file to a FieldTypeFile, x is a number 0...AdminContent.FieldMax
        '       Delete Filex - clears a file name for a FieldTypeFile, x is a number 0...AdminContent.FieldMax
        '       Upload - The action that actually uploads the file
        '       Email - (not done) Sends "body" field to "email" field in adminContent.id
        '========================================================================
        '
        Private Sub ProcessActions(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, UseContentWatchLink As Boolean)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "ProcessActions")
            '
            'Dim Upload As KMAUpload3.UploadClass
            '
            Dim CS As Integer
            Dim Filename As String
            Dim FieldCount As Integer
            Dim SQL As String
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim EMailToMemberID As Integer
            Dim EmailFrom As String
            Dim EmailSubject As String
            Dim EmailBody As String
            Dim EmailStatus As String
            Dim EmailStatusMessage As String
            Dim DataSourceName As String
            Dim ForceReadOnly As Boolean        ' set true for special cases that force a record read only
            Dim EmailBodySource As String
            Dim EmailBodyHTML As Boolean
            Dim EmailSubjectSource As String
            Dim EmailSubjectHTML As Boolean
            Dim ApprovedName As String
            Dim CSRecord As Integer
            Dim ContentID As Integer
            Dim RecordID As Integer
            Dim ContentName As String
            Dim Copy As String
            Dim CSEditRecord As Integer
            Dim EmailToConfirmationMemberID As Integer
            Dim ImageWidth As Integer
            Dim ImageHeight As Integer
            Dim Position As Integer
            Dim ByteArray() As Byte
            Dim VirtualFilePath As String
            Dim EmailAddLinkEID As Boolean
            Dim OpenTriggerCode As String
            Dim OpenTriggerCss As String
            Dim ClickFlagQuery As String
            Dim EmailSpamFooterFlag As String
            Dim ToAll As Boolean
            Dim CSLog As Integer
            Dim EmailDropID As Integer
            Dim EmailToAddress As String
            Dim EmailToName As String
            Dim ScheduletypeID As Integer
            Dim EMailTemplateID As Integer
            Dim EmailTemplate As String
            Dim RowCnt As Integer
            Dim RowPtr As Integer
            '
            If AdminAction <> AdminActionNop Then
                If Not UserAllowContentEdit Then
                    '
                    ' Action blocked by BlockCurrentRecord
                    '
                Else
                    '
                    ' Process actions
                    '
                    Select Case AdminAction
                        Case AdminActionEditRefresh
                            '
                            ' Load the record as if it will be saved, but skip the save
                            '
                            Call LoadEditRecord(adminContent, editRecord)
                            Call LoadEditRecord_Request(adminContent, editRecord)
                        Case AdminActionMarkReviewed
                            '
                            ' Mark the record reviewed without making any changes
                            '
                            Call cpCore.doc.markRecordReviewed(adminContent.Name, editRecord.id)
                        'Case AdminActionWorkflowPublishSelected
                        '    '
                        '    ' Publish everything selected
                        '    '
                        '    RowCnt = cpCore.docProperties.getInteger("RowCnt")
                        '    For RowPtr = 0 To RowCnt - 1
                        '        If cpCore.docProperties.getBoolean("Row" & RowPtr) Then
                        '            RecordID = cpCore.docProperties.getInteger("RowID" & RowPtr)
                        '            ContentName = cpCore.docProperties.getText("RowContentName" & RowPtr)
                        '            Call cpCore.workflow.publishEdit(ContentName, RecordID)
                        '            Call cpCore.doc.processAfterSave(False, ContentName, RecordID, "", 0, UseContentWatchLink)
                        '            Call cpCore.cache.invalidateObject(cacheController.getDbRecordCacheName(adminContent.ContentTableName, recordId))
                        '            Call cpCore.db.executeSql("delete from ccAuthoringControls where recordid=" & RecordID & " and Contentid=" & models.complex.cdefmodel.getcontentid(cpcore,ContentName))
                        '        End If
                        '    Next
                        'Case AdminActionWorkflowPublishApproved
                        '    '
                        '    ' Publish all approved workflow publishing records
                        '    '
                        '    CS = cpCore.db.cs_open("Authoring Controls", "ControlType=3", "ID")
                        '    Do While cpCore.db.cs_ok(CS)
                        '        ContentID = cpCore.db.cs_getInteger(CS, "ContentID")
                        '        RecordID = cpCore.db.cs_getInteger(CS, "RecordID")
                        '        ContentName = models.complex.cdefmodel.getContentNameByID(cpcore,ContentID)
                        '        If ContentName <> "" Then
                        '            Call cpCore.workflow.publishEdit(ContentName, RecordID)
                        '            Call cpCore.doc.processAfterSave(False, ContentName, RecordID, "", 0, UseContentWatchLink)
                        '            Call cpCore.cache.invalidateObject(cacheController.getDbRecordCacheName(adminContent.ContentTableName, recordId))
                        '        End If
                        '        cpCore.db.cs_goNext(CS)
                        '    Loop
                        '    Call cpCore.db.cs_Close(CS)
                        '    'AdminForm = AdminFormRoot
                        'Case AdminActionPublishApprove
                        '    If (editRecord.Read_Only) Then
                        '        Call errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is locked.")
                        '    ElseIf Not adminContent.AllowWorkflowAuthoring Then
                        '        Call errorController.error_AddUserError(cpCore, "Your request was blocked because content you selected does not support workflow authoring.")
                        '    Else
                        '        '
                        '        Call LoadEditRecord(adminContent, editRecord)
                        '        Call LoadEditResponse(adminContent, editRecord)
                        '        Call ProcessActionSave(adminContent, editRecord, UseContentWatchLink)
                        '        If Not (cpCore.doc.debug_iUserError <> "") Then
                        '            'no - if WF, on process on publish
                        '            'Call ProcessSpecialCaseAfterSave(false,AdminContent.Name, EditRecord.ID, EditRecord.Name, EditRecord.ParentID, UseContentWatchLink)
                        '            Call cpCore.workflow.approveEdit(adminContent.Name, editRecord.id)
                        '        Else
                        '            AdminForm = AdminSourceForm
                        '        End If
                        '    End If
                        '    AdminAction = AdminActionNop ' convert so action can be used in as a refresh
                        'Case AdminActionPublishSubmit
                        '    If (editRecord.Read_Only) Then
                        '        Call errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is locked.")
                        '    ElseIf Not adminContent.AllowWorkflowAuthoring Then
                        '        Call errorController.error_AddUserError(cpCore, "Your request was blocked because content you selected does not support workflow authoring.")
                        '    Else
                        '        '
                        '        Call LoadEditRecord(adminContent, editRecord)
                        '        Call LoadEditResponse(adminContent, editRecord)
                        '        Call ProcessActionSave(adminContent, editRecord, UseContentWatchLink)
                        '        If Not (cpCore.doc.debug_iUserError <> "") Then
                        '            'no - if WF, on process on publish
                        '            'Call ProcessSpecialCaseAfterSave(false,AdminContent.Name, EditRecord.ID, EditRecord.Name, EditRecord.ParentID, UseContentWatchLink)
                        '            Call cpCore.workflow.main_SubmitEdit(adminContent.Name, editRecord.id)
                        '            Call cpCore.doc.sendPublishSubmitNotice(adminContent.Name, editRecord.id, editRecord.nameLc)
                        '        Else
                        '            AdminForm = AdminSourceForm
                        '        End If
                        '    End If
                        '    AdminAction = AdminActionNop ' convert so action can be used in as a refresh
                        'Case AdminActionPublish
                        '    '
                        '    ' --- Publish edit record to live record - not AuthoringLock blocked
                        '    '
                        '    Call LoadEditRecord(adminContent, editRecord)
                        '    Call LoadEditResponse(adminContent, editRecord)
                        '    Call ProcessActionSave(adminContent, editRecord, UseContentWatchLink)
                        '    If Not (cpCore.doc.debug_iUserError <> "") Then
                        '        Call cpCore.workflow.publishEdit(adminContent.Name, editRecord.id)
                        '        CS = cpCore.db.csOpenRecord(adminContent.Name, editRecord.id)
                        '        Dim IsDeleted As Boolean
                        '        IsDeleted = Not cpCore.db.cs_ok(CS)
                        '        Call cpCore.db.cs_Close(CS)
                        '        Call cpCore.doc.processAfterSave(IsDeleted, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink)
                        '        Call cpCore.cache.invalidateObject(cacheController.getDbRecordCacheName(adminContent.ContentTableName, recordId))
                        '    Else
                        '        AdminForm = AdminSourceForm
                        '    End If
                        'Case AdminActionAbortEdit
                        '    '
                        '    ' --- copy live record over edit record
                        '    '
                        '    Call cpCore.workflow.abortEdit2(adminContent.Name, editRecord.id, cpCore.doc.authContext.user.id)
                        '    Call cpCore.doc.processAfterSave(False, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink)
                        '    If MenuDepth > 0 Then
                        '        '
                        '        ' opened as a child, close the window
                        '        '
                        '        AdminForm = AdminFormClose
                        '    Else
                        '        '
                        '        ' opened as a main window, go to the contents index page
                        '        '
                        '        AdminForm = AdminFormIndex
                        '    End If
                        Case AdminActionDelete
                            If (editRecord.Read_Only) Then
                                Call errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is now locked by another authcontext.user.")
                            Else
                                Call LoadEditRecord(adminContent, editRecord)
                                Call cpCore.db.deleteTableRecord(adminContent.ContentTableName, editRecord.id, adminContent.ContentDataSourceName)
                                Call cpCore.doc.processAfterSave(True, editRecord.contentControlId_Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink)
                            End If
                            AdminAction = AdminActionNop ' convert so action can be used in as a refresh
                            '                Case AdminActionSetHTMLEdit
                            '                    '
                            '                    ' Set member property for this field to HTML Edit
                            '                    '
                            '                    Call cpCore.main_SetMemberProperty("HTMLEditor." & AdminContent.Name & "." & InputFieldName, True)
                            '                    Call ProcessActionSave(AdminContent, editRecord,UseContentWatchLink)
                            '                    AdminForm = AdminSourceForm
                            '                Case AdminActionSetTextEdit
                            '                    '
                            '                    ' Set member property for this field to HTML Edit
                            '                    '
                            '                    Call cpCore.main_SetMemberProperty("HTMLEditor." & AdminContent.Name & "." & InputFieldName, False)
                            '                    Call ProcessActionSave(AdminContent, editRecord,UseContentWatchLink)
                            '                    AdminForm = AdminSourceForm
                            '                Case AdminActionSaveField
                            '                    If (editrecord.read_only) Then
                            '                        Call cpCore.htmldoc.main_AddUserError("Your request was blocked because the record you specified is now locked by another authcontext.user.")
                            '                    Else
                            '                        '
                            '                        ' --- preload array with values that may not come back in response
                            '                        '
                            '                        If (InputFieldName = "") Then
                            '                            Call HandleInternalError("ProcessActions", "SaveField action called but InputFieldName is null")
                            '                        Else
                            '                            Call LoadEditRecord
                            '                            Call LoadEditResponseByName(InputFieldName)
                            '                            '
                            '                            ' --- if no error, save values
                            '                            '
                            '                            If Not cpCore.main_IsUserError Then
                            '                                Call SaveEditRecord(adminContent, editRecord,ResponseFormID)
                            '                            End If
                            '                            If cpCore.main_IsUserError Then
                            '                                AdminForm = AdminSourceForm
                            '                            End If
                            '                            'record should be marked modified in cpCore.app.csv_SaveCSRecord
                            '                            'If AdminContent.AllowWorkflowAuthoring Then
                            '                            '    Call cpCore.main_SetAuthoringControl(AdminContent.Name, EditRecord.ID, AuthoringControlsModified)
                            '                            '    End If
                            '                            End If
                            '                        End If
                            '                    AdminAction = AdminActionNop ' convert so action can be used in as a refresh
                            '                    '
                        Case AdminActionSave
                            '
                            ' ----- Save Record
                            '
                            If (editRecord.Read_Only) Then
                                Call errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is now locked by another authcontext.user.")
                            Else
                                Call LoadEditRecord(adminContent, editRecord)
                                Call LoadEditRecord_Request(adminContent, editRecord)
                                Call ProcessActionSave(adminContent, editRecord, UseContentWatchLink)
                                Call cpCore.doc.processAfterSave(False, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink)
                            End If
                            AdminAction = AdminActionNop ' convert so action can be used in as a refresh
                            '
                        Case AdminActionSaveAddNew
                            '
                            ' ----- Save and add a new record
                            '
                            If (editRecord.Read_Only) Then
                                Call errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is now locked by another authcontext.user.")
                            Else
                                Call LoadEditRecord(adminContent, editRecord)
                                Call LoadEditRecord_Request(adminContent, editRecord)
                                Call ProcessActionSave(adminContent, editRecord, UseContentWatchLink)
                                Call cpCore.doc.processAfterSave(False, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink)
                                editRecord.id = 0
                                editRecord.Loaded = False
                                'If AdminContent.fields.Count > 0 Then
                                '    ReDim EditRecordValuesObject(AdminContent.fields.Count)
                                '    ReDim EditRecordDbValues(AdminContent.fields.Count)
                                'End If
                            End If
                            AdminAction = AdminActionNop ' convert so action can be used in as a refresh
                            '
                        Case AdminActionDuplicate
                            '
                            ' ----- Save Record
                            '
                            If allowSaveBeforeDuplicate Then
                                If (editRecord.Read_Only) Then
                                    Call errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is now locked by another authcontext.user.")
                                Else
                                    Call LoadEditRecord(adminContent, editRecord)
                                    Call LoadEditRecord_Request(adminContent, editRecord)
                                    Call ProcessActionSave(adminContent, editRecord, UseContentWatchLink)
                                    Call cpCore.doc.processAfterSave(False, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink)
                                    Call ProcessActionDuplicate(adminContent, editRecord)
                                End If
                            Else
                                Call ProcessActionDuplicate(adminContent, editRecord)
                            End If
                            AdminAction = AdminActionNop ' convert so action can be used in as a refresh
                            '
                        Case AdminActionSendEmail
                            '
                            ' ----- Send (Group Email Only)
                            '
                            If (editRecord.Read_Only) Then
                                Call errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is now locked by another authcontext.user.")
                            Else
                                Call LoadEditRecord(adminContent, editRecord)
                                Call LoadEditRecord_Request(adminContent, editRecord)
                                Call ProcessActionSave(adminContent, editRecord, UseContentWatchLink)
                                Call cpCore.doc.processAfterSave(False, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink)
                                If Not (cpCore.doc.debug_iUserError <> "") Then
                                    If Not Models.Complex.cdefModel.isWithinContent(cpCore, editRecord.contentControlId, Models.Complex.cdefModel.getContentId(cpCore, "Group Email")) Then
                                        Call errorController.error_AddUserError(cpCore, "The send action only supports Group Email.")
                                    Else
                                        CS = cpCore.db.csOpenRecord("Group Email", editRecord.id)
                                        If Not cpCore.db.csOk(CS) Then
                                            Throw New ApplicationException("Unexpected exception") ' throw new applicationException("Unexpected exception")'  cpCore.handleLegacyError23("Email ID [" & editRecord.id & "] could not be found in Group Email.")
                                        ElseIf cpCore.db.csGet(CS, "FromAddress") = "" Then
                                            Call errorController.error_AddUserError(cpCore, "A 'From Address' is required before sending an email.")
                                        ElseIf cpCore.db.csGet(CS, "Subject") = "" Then
                                            Call errorController.error_AddUserError(cpCore, "A 'Subject' is required before sending an email.")
                                        Else
                                            Call cpCore.db.csSet(CS, "submitted", True)
                                            Call cpCore.db.csSet(CS, "ConditionID", 0)
                                            If cpCore.db.csGetDate(CS, "ScheduleDate") = Date.MinValue Then
                                                Call cpCore.db.csSet(CS, "ScheduleDate", cpCore.doc.profileStartTime)
                                            End If
                                        End If
                                        Call cpCore.db.csClose(CS)
                                    End If
                                End If
                            End If
                            AdminAction = AdminActionNop ' convert so action can be used in as a refresh
                            '
                        Case AdminActionDeactivateEmail
                            '
                            ' ----- Deactivate (Conditional Email Only)
                            '
                            If (editRecord.Read_Only) Then
                                Call errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is now locked by another authcontext.user.")
                            Else
                                ' no save, page was read only - Call ProcessActionSave
                                Call LoadEditRecord(adminContent, editRecord)
                                If Not (cpCore.doc.debug_iUserError <> "") Then
                                    If Not Models.Complex.cdefModel.isWithinContent(cpCore, editRecord.contentControlId, Models.Complex.cdefModel.getContentId(cpCore, "Conditional Email")) Then
                                        Call errorController.error_AddUserError(cpCore, "The deactivate action only supports Conditional Email.")
                                    Else
                                        CS = cpCore.db.csOpenRecord("Conditional Email", editRecord.id)
                                        If Not cpCore.db.csOk(CS) Then
                                            Throw New ApplicationException("Unexpected exception") ' throw new applicationException("Unexpected exception")'  cpCore.handleLegacyError23("Email ID [" & editRecord.id & "] could not be opened.")
                                        Else
                                            Call cpCore.db.csSet(CS, "submitted", False)
                                        End If
                                        Call cpCore.db.csClose(CS)
                                    End If
                                End If
                            End If
                            AdminAction = AdminActionNop ' convert so action can be used in as a refresh
                        Case AdminActionActivateEmail
                            '
                            ' ----- Activate (Conditional Email Only)
                            '
                            If (editRecord.Read_Only) Then
                                Call errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is now locked by another authcontext.user.")
                            Else
                                Call LoadEditRecord(adminContent, editRecord)
                                Call LoadEditRecord_Request(adminContent, editRecord)
                                Call ProcessActionSave(adminContent, editRecord, UseContentWatchLink)
                                Call cpCore.doc.processAfterSave(False, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink)
                                If Not (cpCore.doc.debug_iUserError <> "") Then
                                    If Not Models.Complex.cdefModel.isWithinContent(cpCore, editRecord.contentControlId, Models.Complex.cdefModel.getContentId(cpCore, "Conditional Email")) Then
                                        Call errorController.error_AddUserError(cpCore, "The activate action only supports Conditional Email.")
                                    Else
                                        CS = cpCore.db.csOpenRecord("Conditional Email", editRecord.id)
                                        If Not cpCore.db.csOk(CS) Then
                                            Throw New ApplicationException("Unexpected exception") ' throw new applicationException("Unexpected exception")'  cpCore.handleLegacyError23("Email ID [" & editRecord.id & "] could not be opened.")
                                        ElseIf cpCore.db.csGetInteger(CS, "ConditionID") = 0 Then
                                            errorController.error_AddUserError(cpCore, "A condition must be set.")
                                        Else
                                            Call cpCore.db.csSet(CS, "submitted", True)
                                            If cpCore.db.csGetDate(CS, "ScheduleDate") = Date.MinValue Then
                                                Call cpCore.db.csSet(CS, "ScheduleDate", cpCore.doc.profileStartTime)
                                            End If
                                        End If
                                        Call cpCore.db.csClose(CS)
                                    End If
                                End If
                            End If
                            AdminAction = AdminActionNop ' convert so action can be used in as a refresh
                        Case AdminActionSendEmailTest
                            If (editRecord.Read_Only) Then
                                Call errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is now locked by another authcontext.user.")
                            Else
                                '
                                Call LoadEditRecord(adminContent, editRecord)
                                Call LoadEditRecord_Request(adminContent, editRecord)
                                Call ProcessActionSave(adminContent, editRecord, UseContentWatchLink)
                                Call cpCore.doc.processAfterSave(False, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink)
                                '
                                If Not (cpCore.doc.debug_iUserError <> "") Then
                                    '
                                    EmailToConfirmationMemberID = 0
                                    If editRecord.fieldsLc.ContainsKey("testmemberid") Then
                                        EmailToConfirmationMemberID = genericController.EncodeInteger(editRecord.fieldsLc.Item("testmemberid").value)
                                        Call cpCore.email.sendConfirmationTest(editRecord.id, EmailToConfirmationMemberID)
                                        '
                                        If editRecord.fieldsLc.ContainsKey("lastsendtestdate") Then
                                            editRecord.fieldsLc.Item("lastsendtestdate").value = cpCore.doc.profileStartTime
                                            Call cpCore.db.executeQuery("update ccemail Set lastsendtestdate=" & cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime) & " where id=" & editRecord.id)
                                        End If
                                    End If
                                End If
                            End If
                            AdminAction = AdminActionNop ' convert so action can be used in as a refresh
                            ' end case
                        Case AdminActionDeleteRows
                            '
                            ' Delete Multiple Rows
                            '
                            RowCnt = cpCore.docProperties.getInteger("rowcnt")
                            If RowCnt > 0 Then
                                For RowPtr = 0 To RowCnt - 1
                                    If cpCore.docProperties.getBoolean("row" & RowPtr) Then
                                        CSEditRecord = cpCore.db.cs_open2(adminContent.Name, cpCore.docProperties.getInteger("rowid" & RowPtr), True, True)
                                        If cpCore.db.csOk(CSEditRecord) Then
                                            RecordID = cpCore.db.csGetInteger(CSEditRecord, "ID")
                                            Call cpCore.db.csDeleteRecord(CSEditRecord)
                                            If (Not False) Then
                                                '
                                                ' non-Workflow Delete
                                                '
                                                ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, cpCore.db.csGetInteger(CSEditRecord, "ContentControlID"))
                                                Call cpCore.cache.invalidateContent(cacheController.getCacheKey_Entity(adminContent.ContentTableName, RecordID))
                                                Call cpCore.doc.processAfterSave(True, ContentName, RecordID, "", 0, UseContentWatchLink)
                                            End If
                                            '
                                            ' Page Content special cases
                                            '
                                            If genericController.vbLCase(adminContent.ContentTableName) = "ccpagecontent" Then
                                                'Call cpCore.pages.cache_pageContent_removeRow(RecordID, False, False)
                                                If RecordID = (cpCore.siteProperties.getinteger("PageNotFoundPageID", 0)) Then
                                                    Call cpCore.siteProperties.getText("PageNotFoundPageID", "0")
                                                End If
                                                If RecordID = (cpCore.siteProperties.getinteger("LandingPageID", 0)) Then
                                                    Call cpCore.siteProperties.getText("LandingPageID", "0")
                                                End If
                                            End If
                                        End If
                                        Call cpCore.db.csClose(CSEditRecord)
                                    End If
                                Next
                            End If
                        Case AdminActionReloadCDef
                            '
                            ' ccContent - save changes and reload content definitions
                            '
                            If (editRecord.Read_Only) Then
                                Call errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified Is now locked by another authcontext.user.")
                            Else
                                Call LoadEditRecord(adminContent, editRecord)
                                Call LoadEditRecord_Request(adminContent, editRecord)
                                Call ProcessActionSave(adminContent, editRecord, UseContentWatchLink)
                                cpCore.cache.invalidateAll()
                                cpCore.doc.clearMetaData()
                            End If
                            AdminAction = AdminActionNop ' convert so action can be used in as a refresh
                        Case Else
                            '
                            ' Nop action or anything unrecognized - read in database
                            '
                    End Select
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError2("ProcessActions")
            Call errorController.error_AddUserError(cpCore, "There was an unknown error processing this page at " & cpCore.doc.profileStartTime & ". Please try again, Or report this error To the site administrator.")
        End Sub
        '
        '========================================================================
        ' LoadAndSaveContentGroupRules
        '
        '   For a particular content, remove previous GroupRules, and Create new ones
        '========================================================================
        '
        Private Sub LoadAndSaveContentGroupRules(GroupID As Integer)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "LoadAndSaveContentGroupRules")
            '
            Dim GroupCount As Integer
            Dim GroupPointer As Integer
            Dim ContentCount As Integer
            Dim ContentPointer As Integer
            Dim CSPointer As Integer
            Dim ContentID As Integer
            Dim AllowAdd As Boolean
            Dim AllowDelete As Boolean
            Dim CSNew As Integer
            Dim RecordChanged As Boolean
            Dim RuleNeeded As Boolean
            Dim RuleFound As Boolean
            Dim SQL As String
            Dim DeleteIdList As String = ""
            Dim RuleId As Integer
            '
            ' ----- Delete duplicate Group Rules
            '
            SQL = "Select distinct DuplicateRules.ID" _
                & " from ccgrouprules" _
                & " Left join ccgrouprules As DuplicateRules On DuplicateRules.ContentID=ccGroupRules.ContentID" _
                & " where ccGroupRules.ID < DuplicateRules.ID" _
                & " And ccGroupRules.GroupID=DuplicateRules.GroupID"
            SQL = "Delete from ccGroupRules where ID In (" & SQL & ")"
            Call cpCore.db.executeQuery(SQL)
            '
            ' --- create GroupRule records for all selected
            '
            CSPointer = cpCore.db.csOpen("Group Rules", "GroupID=" & GroupID, "ContentID, ID", True)
            ContentCount = cpCore.docProperties.getInteger("ContentCount")
            If ContentCount > 0 Then
                For ContentPointer = 0 To ContentCount - 1
                    RuleNeeded = cpCore.docProperties.getBoolean("Content" & ContentPointer)
                    ContentID = cpCore.docProperties.getInteger("ContentID" & ContentPointer)
                    AllowAdd = cpCore.docProperties.getBoolean("ContentGroupRuleAllowAdd" & ContentPointer)
                    AllowDelete = cpCore.docProperties.getBoolean("ContentGroupRuleAllowDelete" & ContentPointer)
                    '
                    RuleFound = False
                    cpCore.db.cs_goFirst(CSPointer)
                    If cpCore.db.csOk(CSPointer) Then
                        Do While cpCore.db.csOk(CSPointer)
                            If cpCore.db.csGetInteger(CSPointer, "ContentID") = ContentID Then
                                RuleId = cpCore.db.csGetInteger(CSPointer, "id")
                                RuleFound = True
                                Exit Do
                            End If
                            cpCore.db.csGoNext(CSPointer)
                        Loop
                    End If
                    If RuleNeeded And Not RuleFound Then
                        CSNew = cpCore.db.csInsertRecord("Group Rules")
                        If cpCore.db.csOk(CSNew) Then
                            Call cpCore.db.csSet(CSNew, "GroupID", GroupID)
                            Call cpCore.db.csSet(CSNew, "ContentID", ContentID)
                            Call cpCore.db.csSet(CSNew, "AllowAdd", AllowAdd)
                            Call cpCore.db.csSet(CSNew, "AllowDelete", AllowDelete)
                        End If
                        cpCore.db.csClose(CSNew)
                        RecordChanged = True
                    ElseIf RuleFound And Not RuleNeeded Then
                        DeleteIdList &= ", " & RuleId
                        'Call cpCore.main_DeleteCSRecord(CSPointer)
                        RecordChanged = True
                    ElseIf RuleFound And RuleNeeded Then
                        If (AllowAdd <> cpCore.db.csGetBoolean(CSPointer, "AllowAdd")) Then
                            Call cpCore.db.csSet(CSPointer, "AllowAdd", AllowAdd)
                            RecordChanged = True
                        End If
                        If (AllowDelete <> cpCore.db.csGetBoolean(CSPointer, "AllowDelete")) Then
                            Call cpCore.db.csSet(CSPointer, "AllowDelete", AllowDelete)
                            RecordChanged = True
                        End If
                    End If
                Next
            End If
            Call cpCore.db.csClose(CSPointer)
            If DeleteIdList <> "" Then
                SQL = "delete from ccgrouprules where id In (" & Mid(DeleteIdList, 2) & ")"
                Call cpCore.db.executeQuery(SQL)
            End If
            If RecordChanged Then
                Call cpCore.cache.invalidateAllObjectsInContent("Group Rules")
            End If
            Exit Sub
            '
ErrorTrap:
            Call handleLegacyClassError3("LoadAndSaveContentGroupRules")
        End Sub
        '
        '========================================================================
        ' LoadAndSaveGroupRules
        '   read groups from the edit form and modify Group Rules to match
        '========================================================================
        '
        Private Sub LoadAndSaveGroupRules(editRecord As editRecordClass)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "LoadAndSaveGroupRules")
            '
            If editRecord.id <> 0 Then
                Call LoadAndSaveGroupRules_ForContentAndChildren(editRecord.id, "")
            End If
            '
            Exit Sub
            '
ErrorTrap:
            Call handleLegacyClassError3("LoadAndSaveGroupRules")
        End Sub
        '
        '========================================================================
        ' LoadAndSaveGroupRules_ForContentAndChildren
        '   read groups from the edit form and modify Group Rules to match
        '========================================================================
        '
        Private Sub LoadAndSaveGroupRules_ForContentAndChildren(ContentID As Integer, ParentIDString As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "LoadAndSaveGroupRules_ForContentAndChildren")
            '
            Dim CSPointer As Integer
            Dim MyParentIDString As String
            '
            ' --- Create Group Rules for this content
            '
            If CBool(InStr(1, ParentIDString, "," & ContentID & ",")) Then
                Throw (New Exception("Child ContentID [" & ContentID & "] Is its own parent"))
            Else
                MyParentIDString = ParentIDString & "," & ContentID & ","
                Call LoadAndSaveGroupRules_ForContent(ContentID)
                '
                ' --- Create Group Rules for all child content
                '
                CSPointer = cpCore.db.csOpen("Content", "ParentID=" & ContentID)
                Do While cpCore.db.csOk(CSPointer)
                    Call LoadAndSaveGroupRules_ForContentAndChildren(cpCore.db.csGetInteger(CSPointer, "id"), MyParentIDString)
                    Call cpCore.db.csGoNext(CSPointer)
                Loop
                Call cpCore.db.csClose(CSPointer)
            End If
            '
            Exit Sub
            '
ErrorTrap:
            Call handleLegacyClassError3("LoadAndSaveGroupRules_ForContentAndChildren")
        End Sub
        '
        '========================================================================
        ' LoadAndSaveGroupRules_ForContent
        '
        '   For a particular content, remove previous GroupRules, and Create new ones
        '========================================================================
        '
        Private Sub LoadAndSaveGroupRules_ForContent(ContentID As Integer)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "LoadAndSaveGroupRules_ForContent")
            '
            Dim GroupCount As Integer
            Dim GroupPointer As Integer
            Dim CSPointer As Integer
            Dim GroupID As Integer
            Dim AllowAdd As Boolean
            Dim AllowDelete As Boolean
            Dim CSNew As Integer
            Dim RecordChanged As Boolean
            Dim RuleNeeded As Boolean
            Dim RuleFound As Boolean
            Dim SQL As String
            '
            ' ----- Delete duplicate Group Rules
            '

            SQL = "Delete from ccGroupRules where ID In (" _
                & "Select distinct DuplicateRules.ID from ccgrouprules Left join ccgrouprules As DuplicateRules On DuplicateRules.GroupID=ccGroupRules.GroupID where ccGroupRules.ID < DuplicateRules.ID  And ccGroupRules.ContentID=DuplicateRules.ContentID" _
                & ")"
            Call cpCore.db.executeQuery(SQL)
            '
            ' --- create GroupRule records for all selected
            '
            CSPointer = cpCore.db.csOpen("Group Rules", "ContentID=" & ContentID, "GroupID,ID", True)
            GroupCount = cpCore.docProperties.getInteger("GroupCount")
            If GroupCount > 0 Then
                For GroupPointer = 0 To GroupCount - 1
                    RuleNeeded = cpCore.docProperties.getBoolean("Group" & GroupPointer)
                    GroupID = cpCore.docProperties.getInteger("GroupID" & GroupPointer)
                    AllowAdd = cpCore.docProperties.getBoolean("GroupRuleAllowAdd" & GroupPointer)
                    AllowDelete = cpCore.docProperties.getBoolean("GroupRuleAllowDelete" & GroupPointer)
                    '
                    RuleFound = False
                    cpCore.db.cs_goFirst(CSPointer)
                    If cpCore.db.csOk(CSPointer) Then
                        Do While cpCore.db.csOk(CSPointer)
                            If cpCore.db.csGetInteger(CSPointer, "GroupID") = GroupID Then
                                RuleFound = True
                                Exit Do
                            End If
                            cpCore.db.csGoNext(CSPointer)
                        Loop
                    End If
                    If RuleNeeded And Not RuleFound Then
                        CSNew = cpCore.db.csInsertRecord("Group Rules")
                        If cpCore.db.csOk(CSNew) Then
                            Call cpCore.db.csSet(CSNew, "ContentID", ContentID)
                            Call cpCore.db.csSet(CSNew, "GroupID", GroupID)
                            Call cpCore.db.csSet(CSNew, "AllowAdd", AllowAdd)
                            Call cpCore.db.csSet(CSNew, "AllowDelete", AllowDelete)
                        End If
                        cpCore.db.csClose(CSNew)
                        RecordChanged = True
                    ElseIf RuleFound And Not RuleNeeded Then
                        Call cpCore.db.csDeleteRecord(CSPointer)
                        RecordChanged = True
                    ElseIf RuleFound And RuleNeeded Then
                        If (AllowAdd <> cpCore.db.csGetBoolean(CSPointer, "AllowAdd")) Then
                            Call cpCore.db.csSet(CSPointer, "AllowAdd", AllowAdd)
                            RecordChanged = True
                        End If
                        If (AllowDelete <> cpCore.db.csGetBoolean(CSPointer, "AllowDelete")) Then
                            Call cpCore.db.csSet(CSPointer, "AllowDelete", AllowDelete)
                            RecordChanged = True
                        End If
                    End If
                Next
            End If
            Call cpCore.db.csClose(CSPointer)
            If RecordChanged Then
                Call cpCore.cache.invalidateAllObjectsInContent("Group Rules")
            End If
            Exit Sub
            '
ErrorTrap:
            Call handleLegacyClassError3("LoadAndSaveGroupRules_ForContent")
        End Sub
        '
        '========================================================================
        ' Load Array
        '   Get defaults if no record ID
        '   Then load in any response elements
        '========================================================================
        '
        Private Sub LoadEditRecord(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, Optional ByVal CheckUserErrors As Boolean = False)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "LoadEditRecord")
            '
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim ApprovedDate As Date
            Dim CS As Integer
            ''Dim arrayOfFields() As appServices_metaDataClass.CDefFieldClass
            '
            If adminContent.Name = "" Then
                '
                ' Can not load edit record because bad content definition
                '
                If adminContent.Id = 0 Then
                    Throw (New Exception("The record can Not be edited because no content definition was specified."))
                Else
                    Throw (New Exception("The record can Not be edited because a content definition For ID [" & adminContent.Id & "] was Not found."))
                End If
            Else
                '
                If editRecord.id = 0 Then
                    '
                    ' ----- New record, just load defaults
                    '
                    Call LoadEditRecord_Default(adminContent, editRecord)
                    Call LoadEditRecord_WherePairs(adminContent, editRecord)
                Else
                    '
                    ' ----- Load the Live Record specified
                    '
                    Call LoadEditRecord_Dbase(adminContent, editRecord, CheckUserErrors)
                    Call LoadEditRecord_WherePairs(adminContent, editRecord)
                End If
                '        '
                '        ' ----- Test for a change of admincontent (the record is a child of admincontent )
                '        '
                '        If EditRecord.ContentID <> AdminContent.Id Then
                '            AdminContent = cpCore.app.getCdef(EditRecord.ContentName)
                '        End If
                '
                ' ----- Capture core fields needed for processing
                '
                editRecord.menuHeadline = ""
                If editRecord.fieldsLc.ContainsKey("menuheadline") Then
                    editRecord.menuHeadline = genericController.encodeText(editRecord.fieldsLc.Item("menuheadline").value)
                End If
                '
                editRecord.menuHeadline = ""
                If editRecord.fieldsLc.ContainsKey("name") Then
                    'Dim editRecordField As editRecordFieldClass = editRecord.fieldsLc.Item("name")
                    'editRecord.nameLc = editRecordField.value.ToString()
                    editRecord.nameLc = genericController.encodeText(editRecord.fieldsLc.Item("name").value)
                End If
                '
                editRecord.menuHeadline = ""
                If editRecord.fieldsLc.ContainsKey("active") Then
                    editRecord.active = genericController.EncodeBoolean(editRecord.fieldsLc.Item("active").value)
                End If
                '
                editRecord.menuHeadline = ""
                If editRecord.fieldsLc.ContainsKey("contentcontrolid") Then
                    editRecord.contentControlId = genericController.EncodeInteger(editRecord.fieldsLc.Item("contentcontrolid").value)
                End If
                '
                editRecord.menuHeadline = ""
                If editRecord.fieldsLc.ContainsKey("parentid") Then
                    editRecord.parentID = genericController.EncodeInteger(editRecord.fieldsLc.Item("parentid").value)
                End If
                '
                editRecord.menuHeadline = ""
                If editRecord.fieldsLc.ContainsKey("rootpageid") Then
                    editRecord.RootPageID = genericController.EncodeInteger(editRecord.fieldsLc.Item("rootpageid").value)
                End If
                '
                ' ----- Set the local global copy of Edit Record Locks
                '
                Call cpCore.doc.getAuthoringStatus(adminContent.Name, editRecord.id, editRecord.SubmitLock, editRecord.ApproveLock, editRecord.SubmittedName, editRecord.ApprovedName, editRecord.IsInserted, editRecord.IsDeleted, editRecord.IsModified, editRecord.LockModifiedName, editRecord.LockModifiedDate, editRecord.SubmittedDate, editRecord.ApprovedDate)
                '
                ' ----- Set flags used to determine the Authoring State
                '
                Call cpCore.doc.getAuthoringPermissions(adminContent.Name, editRecord.id, editRecord.AllowInsert, editRecord.AllowCancel, editRecord.AllowSave, editRecord.AllowDelete, editRecord.AllowPublish, editRecord.AllowAbort, editRecord.AllowSubmit, editRecord.AllowApprove, editRecord.Read_Only)
                '
                ' ----- Set Edit Lock
                '
                If editRecord.id <> 0 Then
                    editRecord.EditLock = cpCore.workflow.GetEditLockStatus(adminContent.Name, editRecord.id)
                    If editRecord.EditLock Then
                        editRecord.EditLockMemberName = cpCore.workflow.GetEditLockMemberName(adminContent.Name, editRecord.id)
                        editRecord.EditLockExpires = cpCore.workflow.GetEditLockDateExpires(adminContent.Name, editRecord.id)
                    End If
                End If
                '
                ' ----- Set Read Only: for edit lock
                '
                If editRecord.EditLock Then
                    editRecord.Read_Only = True
                End If
                '
                ' ----- Set Read Only: if non-developer tries to edit a developer record
                '
                If genericController.vbUCase(adminContent.ContentTableName) = genericController.vbUCase("ccMembers") Then
                    If Not cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore) Then
                        If editRecord.fieldsLc.ContainsKey("developer") Then
                            If genericController.EncodeBoolean(editRecord.fieldsLc.Item("developer").value) Then
                                editRecord.Read_Only = True
                                errorController.error_AddUserError(cpCore, "You Do Not have access rights To edit this record.")
                                BlockEditForm = True
                            End If
                        End If
                    End If
                End If
                '
                ' ----- Now make sure this record is locked from anyone else
                '
                If Not (editRecord.Read_Only) Then
                    Call cpCore.workflow.SetEditLock(adminContent.Name, editRecord.id)
                End If
                editRecord.Loaded = True
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("LoadEditRecord")
            '
        End Sub
        '
        '========================================================================
        '   Get the Wherepair value for a fieldname
        '       If there is a match with the left side, return the right
        '       If no match, return ""
        '========================================================================
        '
        Private Function GetWherePairValue(FieldName As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "GetWherePairValue")
            '
            Dim WhereCount As Integer
            '
            FieldName = genericController.vbUCase(FieldName)
            '
            GetWherePairValue = ""
            If WherePairCount > 0 Then
                For WhereCount = 0 To WherePairCount - 1
                    If FieldName = genericController.vbUCase(WherePair(0, WhereCount)) Then
                        GetWherePairValue = WherePair(1, WhereCount)
                        Exit For
                    End If
                Next
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetWherePairValue")
            '
        End Function
        '
        '========================================================================
        '   Load both Live and Edit Record values from definition defaults
        '========================================================================
        '
        Private Sub LoadEditRecord_Default(adminContent As Models.Complex.cdefModel, editrecord As editRecordClass)
            Try
                Dim DefaultValueText As String
                Dim LookupContentName As String
                Dim UCaseDefaultValueText As String
                Dim lookups() As String
                Dim Ptr As Integer
                '' converted array to dictionary - Dim FieldPointer As Integer
                'Dim FieldCount As Integer
                Dim defaultValue As String
                Dim MethodName As String
                Dim editRecordField As editRecordFieldClass
                Dim field As Models.Complex.CDefFieldModel
                '
                MethodName = "Admin.Method()"
                '
                editrecord.active = True
                editrecord.contentControlId = adminContent.Id
                editrecord.contentControlId_Name = adminContent.Name
                editrecord.EditLock = False
                editrecord.Loaded = False
                editrecord.Saved = False
                For Each keyValuePair In adminContent.fields
                    field = keyValuePair.Value
                    With field
                        If Not (editrecord.fieldsLc.ContainsKey(field.nameLc)) Then
                            editRecordField = New editRecordFieldClass
                            editrecord.fieldsLc.Add(field.nameLc, editRecordField)
                        End If
                        defaultValue = .defaultValue
                        '    End If
                        If .active And Not genericController.IsNull(defaultValue) Then
                            Select Case .fieldTypeId
                                Case FieldTypeIdInteger, FieldTypeIdAutoIdIncrement, FieldTypeIdMemberSelect
                                    '
                                    editrecord.fieldsLc(field.nameLc).value = genericController.EncodeInteger(defaultValue)
                                Case FieldTypeIdCurrency, FieldTypeIdFloat
                                    '
                                    editrecord.fieldsLc(field.nameLc).value = genericController.EncodeNumber(defaultValue)
                                Case FieldTypeIdBoolean
                                    '
                                    editrecord.fieldsLc(field.nameLc).value = genericController.EncodeBoolean(defaultValue)
                                Case FieldTypeIdDate
                                    '
                                    editrecord.fieldsLc(field.nameLc).value = genericController.EncodeDate(defaultValue)
                                Case FieldTypeIdLookup

                                    DefaultValueText = genericController.encodeText(.defaultValue)
                                    If DefaultValueText <> "" Then
                                        If genericController.vbIsNumeric(DefaultValueText) Then
                                            editrecord.fieldsLc(field.nameLc).value = DefaultValueText
                                        Else
                                            If .lookupContentID <> 0 Then
                                                LookupContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, .lookupContentID)
                                                If LookupContentName <> "" Then
                                                    editrecord.fieldsLc(field.nameLc).value = cpCore.db.getRecordID(LookupContentName, DefaultValueText)
                                                End If
                                            ElseIf .lookupList <> "" Then
                                                UCaseDefaultValueText = genericController.vbUCase(DefaultValueText)
                                                lookups = Split(.lookupList, ",")
                                                For Ptr = 0 To UBound(lookups)
                                                    If UCaseDefaultValueText = genericController.vbUCase(lookups(Ptr)) Then
                                                        editrecord.fieldsLc(field.nameLc).value = Ptr + 1
                                                        Exit For
                                                    End If
                                                Next
                                            End If
                                        End If
                                    End If

                                Case Else
                                    '
                                    editrecord.fieldsLc(field.nameLc).value = genericController.encodeText(defaultValue)
                            End Select
                        End If
                        '
                        ' process reserved fields (set defaults just makes it look good)
                        ' (also, this presets readonly/devonly/adminonly fields not set to member)
                        '
                        Select Case genericController.vbUCase(.nameLc)
                            'Case "ID"
                            '    .readonlyfield = True
                            '    .Required = False
                            Case "MODIFIEDBY"
                                editrecord.fieldsLc(field.nameLc).value = cpCore.doc.authContext.user.id
                                '    .readonlyfield = True
                                '    .Required = False
                            Case "CREATEDBY"
                                editrecord.fieldsLc(field.nameLc).value = cpCore.doc.authContext.user.id
                                '    .readonlyfield = True
                                '    .Required = False
                                'Case "DATEADDED"
                                '    .readonlyfield = True
                                '    .Required = False
                            Case "CONTENTCONTROLID"
                                editrecord.fieldsLc(field.nameLc).value = adminContent.Id
                                'Case "SORTORDER"
                                ' default to ID * 100, but must be done later
                        End Select
                        editrecord.fieldsLc(field.nameLc).dbValue = editrecord.fieldsLc(field.nameLc).value
                    End With
                Next
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        '   Load both Live and Edit Record values from definition defaults
        '========================================================================
        '
        Private Sub LoadEditRecord_WherePairs(Admincontent As Models.Complex.cdefModel, editRecord As editRecordClass)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "LoadEditRecord_WherePairs")
            '
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim FieldCount As Integer
            Dim DefaultValueText As String
            Dim MethodName As String
            '
            MethodName = "Admin.LoadEditRecord_WherePairs(adminContent, editRecord)"
            '
            For Each keyValuePair In Admincontent.fields
                Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                With field
                    DefaultValueText = GetWherePairValue(.nameLc)
                    If .active And (DefaultValueText <> "") Then
                        Select Case .fieldTypeId
                            Case FieldTypeIdInteger, FieldTypeIdLookup, FieldTypeIdAutoIdIncrement
                                '
                                editRecord.fieldsLc.Item(.nameLc).value = genericController.EncodeInteger(DefaultValueText)
                            Case FieldTypeIdCurrency, FieldTypeIdFloat
                                '
                                editRecord.fieldsLc.Item(.nameLc).value = genericController.EncodeNumber(DefaultValueText)
                            Case FieldTypeIdBoolean
                                '
                                editRecord.fieldsLc.Item(.nameLc).value = genericController.EncodeBoolean(DefaultValueText)
                            Case FieldTypeIdDate
                                '
                                editRecord.fieldsLc.Item(.nameLc).value = genericController.EncodeDate(DefaultValueText)
                            Case FieldTypeIdManyToMany
                                '
                                ' Many to Many can capture a list of ID values representing the 'secondary' values in the Many-To-Many Rules table
                                '
                                editRecord.fieldsLc.Item(.nameLc).value = DefaultValueText
                            Case Else
                                '
                                editRecord.fieldsLc.Item(.nameLc).value = DefaultValueText
                        End Select
                    End If
                End With
            Next
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("LoadEditRecord_WherePairs")
        End Sub
        '
        '========================================================================
        '   Load Records from the database
        '========================================================================
        '
        Private Sub LoadEditRecord_Dbase(ByVal adminContent As Models.Complex.cdefModel, ByRef editrecord As editRecordClass, Optional ByVal CheckUserErrors As Boolean = False)
            Try
                '
                Dim DBValueVariant As Object
                Dim CSEditRecord As Integer
                Dim NullVariant As Object
                Dim CSPointer As Integer
                'Dim WorkflowAuthoring As Boolean
                '
                ' ----- test for content problem
                '
                If editrecord.id = 0 Then
                    '
                    ' ----- Skip load, this is a new record
                    '
                ElseIf adminContent.Id = 0 Then
                    '
                    ' ----- Error: no content ID
                    '
                    BlockEditForm = True
                    Call errorController.error_AddUserError(cpCore, "No content definition was found For Content ID [" & editrecord.id & "]. Please contact your application developer For more assistance.")
                    Call handleLegacyClassError("AdminClass.LoadEditRecord_Dbase", "No content definition was found For Content ID [" & editrecord.id & "].")
                ElseIf adminContent.Name = "" Then
                    '
                    ' ----- Error: no content name
                    '
                    BlockEditForm = True
                    Call errorController.error_AddUserError(cpCore, "No content definition could be found For ContentID [" & adminContent.Id & "]. This could be a menu Error. Please contact your application developer For more assistance.")
                    Call handleLegacyClassError("AdminClass.LoadEditRecord_Dbase", "No content definition For ContentID [" & adminContent.Id & "] could be found.")
                ElseIf adminContent.ContentTableName = "" Then
                    '
                    ' ----- Error: no content table
                    '
                    BlockEditForm = True
                    Call errorController.error_AddUserError(cpCore, "The content definition [" & adminContent.Name & "] Is Not associated With a valid database table. Please contact your application developer For more assistance.")
                    Call handleLegacyClassError("AdminClass.LoadEditRecord_Dbase", "No content definition For ContentID [" & adminContent.Id & "] could be found.")
                    '
                    ' move block to the edit and listing pages - to handle content editor cases - so they can edit 'pages', and just get the records they are allowed
                    '
                    '    ElseIf Not UserAllowContentEdit Then
                    '        '
                    '        ' ----- Error: load blocked by UserAllowContentEdit
                    '        '
                    '        BlockEditForm = True
                    '        Call cpCore.htmldoc.main_AddUserError("Your account On this system does Not have access rights To edit this content.")
                    '        Call HandleInternalError("AdminClass.LoadEditRecord_Dbase", "User does Not have access To this content")
                ElseIf adminContent.fields.Count = 0 Then
                    '
                    ' ----- Error: content definition is not complete
                    '
                    BlockEditForm = True
                    Call errorController.error_AddUserError(cpCore, "The content definition [" & adminContent.Name & "] has no field records defined. Please contact your application developer For more assistance.")
                    Call handleLegacyClassError("AdminClass.LoadEditRecord_Dbase", "Content [" & adminContent.Name & "] has no fields defined.")
                Else
                    '
                    '   Open Content Sets with the data
                    '
                    CSEditRecord = cpCore.db.cs_open2(adminContent.Name, editrecord.id, True, True)
                    '
                    '
                    ' store fieldvalues in RecordValuesVariant
                    '
                    If Not (cpCore.db.csOk(CSEditRecord)) Then
                        '
                        '   Live or Edit records were not found
                        '
                        BlockEditForm = True
                        Call errorController.error_AddUserError(cpCore, "The information you have requested could not be found. The record could have been deleted, Or there may be a system Error.")
                        ' removed because it was throwing too many false positives (1/14/04 - tried to do it again)
                        ' If a CM hits the edit tag for a deleted record, this is hit. It should not cause the Developers to spend hours running down.
                        'Call HandleInternalError("AdminClass.LoadEditRecord_Dbase", "Content edit record For [" & AdminContent.Name & "." & EditRecord.ID & "] was Not found.")
                    Else
                        '
                        ' Read database values into RecordValuesVariant array
                        '
                        NullVariant = Nothing
                        For Each keyValuePair In adminContent.fields
                            Dim adminContentField As Models.Complex.CDefFieldModel = keyValuePair.Value
                            Dim fieldNameLc As String = adminContentField.nameLc
                            Dim editRecordField As editRecordFieldClass
                            '
                            ' set editRecord.field to editRecordField and set values
                            '
                            If Not editrecord.fieldsLc.ContainsKey(fieldNameLc) Then
                                editRecordField = New editRecordFieldClass
                                editrecord.fieldsLc.Add(fieldNameLc, editRecordField)
                            Else
                                editRecordField = editrecord.fieldsLc(fieldNameLc)
                            End If
                            '
                            ' 1/21/2007 - added clause if required and null, set to default value
                            '
                            Dim fieldValue As Object
                            fieldValue = NullVariant
                            With adminContentField
                                If (.ReadOnly Or .NotEditable) Then
                                    '
                                    ' 202-31245: quick fix. The CS should handle this instead.
                                    ' Workflowauthoring, If read only, use the live record data
                                    '
                                    CSPointer = CSEditRecord
                                Else
                                    CSPointer = CSEditRecord
                                End If
                                '
                                ' Load the current Database value
                                '
                                Select Case .fieldTypeId
                                    Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                                        DBValueVariant = ""
                                    Case FieldTypeIdFileText, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript, FieldTypeIdFileHTML
                                        DBValueVariant = cpCore.db.csGet(CSPointer, .nameLc)
                                    Case Else
                                        DBValueVariant = cpCore.db.cs_getValue(CSPointer, .nameLc)
                                End Select
                                '
                                ' Check for required and null case loading error
                                '
                                If CheckUserErrors And .Required And (genericController.IsNull(DBValueVariant)) Then
                                    '
                                    ' if required and null
                                    '
                                    If (String.IsNullOrEmpty(.defaultValue)) Then
                                        '
                                        ' default is null
                                        '
                                        If .editTabName = "" Then
                                            Call errorController.error_AddUserError(cpCore, "The value for [" & .caption & "] was empty but is required. This must be set before you can save this record.")
                                        Else
                                            Call errorController.error_AddUserError(cpCore, "The value for [" & .caption & "] in tab [" & .editTabName & "] was empty but is required. This must be set before you can save this record.")
                                        End If
                                    Else
                                        '
                                        ' if required and null, set value to the default
                                        '
                                        DBValueVariant = .defaultValue
                                        If .editTabName = "" Then
                                            Call errorController.error_AddUserError(cpCore, "The value for [" & .caption & "] was null but is required. The default value Is shown, And will be saved if you save this record.")
                                        Else
                                            Call errorController.error_AddUserError(cpCore, "The value for [" & .caption & "] in tab [" & .editTabName & "] was null but is required. The default value Is shown, And will be saved if you save this record.")
                                        End If
                                    End If
                                End If
                                '
                                ' Save EditRecord values
                                '
                                Select Case genericController.vbUCase(.nameLc)
                                    Case "DATEADDED"
                                        editrecord.dateAdded = cpCore.db.csGetDate(CSEditRecord, .nameLc)
                                    Case "MODIFIEDDATE"
                                        editrecord.modifiedDate = cpCore.db.csGetDate(CSEditRecord, .nameLc)
                                    Case "CREATEDBY"
                                        editrecord.createByMemberId = cpCore.db.csGetInteger(CSEditRecord, .nameLc)
                                    Case "MODIFIEDBY"
                                        editrecord.modifiedByMemberID = cpCore.db.csGetInteger(CSEditRecord, .nameLc)
                                    Case "ACTIVE"
                                        editrecord.active = cpCore.db.csGetBoolean(CSEditRecord, .nameLc)
                                    Case "CONTENTCONTROLID"
                                        editrecord.contentControlId = cpCore.db.csGetInteger(CSEditRecord, .nameLc)
                                        editrecord.contentControlId_Name = Models.Complex.cdefModel.getContentNameByID(cpCore, editrecord.contentControlId)
                                    Case "ID"
                                        editrecord.id = cpCore.db.csGetInteger(CSEditRecord, .nameLc)
                                    Case "MENUHEADLINE"
                                        editrecord.menuHeadline = cpCore.db.csGetText(CSEditRecord, .nameLc)
                                    Case "NAME"
                                        editrecord.nameLc = cpCore.db.csGetText(CSEditRecord, .nameLc)
                                    Case "PARENTID"
                                        editrecord.parentID = cpCore.db.csGetInteger(CSEditRecord, .nameLc)
                                        'Case Else
                                        '    EditRecordValuesVariant(FieldPointer) = DBValueVariant
                                End Select
                            End With
                            '
                            editRecordField.dbValue = DBValueVariant
                            editRecordField.value = DBValueVariant
                        Next
                    End If
                    Call cpCore.db.csClose(CSEditRecord)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        '   Read the Form into the fields array
        '========================================================================
        '
        Private Sub LoadEditRecord_Request(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass)
            Try
                Dim PageNotFoundPageID As Integer
                Dim FormFieldListToBeLoaded As String
                Dim FormEmptyFieldList As String
                '
                ' List of fields that were created for the form, and should be verified (starts and ends with a comma)
                '
                FormFieldListToBeLoaded = cpCore.docProperties.getText("FormFieldList")
                If FormFieldListToBeLoaded = "" Then
                    FormFieldListToBeLoaded = ","
                Else
                    'FormFieldListToBeLoaded = "," & FormFieldListToBeLoaded & ","
                End If
                '
                ' List of fields coming from the form that are empty -- and should not be in stream (starts and ends with a comma)
                '
                FormEmptyFieldList = cpCore.docProperties.getText("FormEmptyFieldList")
                '
                If AllowAdminFieldCheck() And (FormFieldListToBeLoaded = ",") Then
                    '
                    ' The field list was not returned
                    '
                    Call errorController.error_AddUserError(cpCore, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The Error Is [no field list].")
                ElseIf AllowAdminFieldCheck() And (FormEmptyFieldList = "") Then
                    '
                    ' The field list was not returned
                    '
                    Call errorController.error_AddUserError(cpCore, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The Error Is [no empty field list].")
                Else
                    '
                    ' fixup the string so it can be reduced by each field found, leaving and empty string if all correct
                    '
                    Dim datasource As dataSourceModel = dataSourceModel.create(cpCore, adminContent.dataSourceId, New List(Of String))
                    'DataSourceName = cpCore.db.getDataSourceNameByID(adminContent.dataSourceId)
                    For Each keyValuePair In adminContent.fields
                        Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                        Call LoadEditRecord_RequestField(adminContent, editRecord, field, datasource.Name, FormFieldListToBeLoaded, FormEmptyFieldList)
                    Next
                    '
                    ' If there are any form fields that were no loaded, flag the error now
                    '
                    If AllowAdminFieldCheck() And (FormFieldListToBeLoaded <> ",") Then
                        Call errorController.error_AddUserError(cpCore, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The following fields where Not found [" & Mid(FormFieldListToBeLoaded, 2, Len(FormFieldListToBeLoaded) - 2) & "].")
                        Throw (New ApplicationException("Unexpected exception")) '  cpCore.handleLegacyError2("AdminClass", "LoadEditResponse", cpCore.serverConfig.appConfig.name & ", There were fields In the fieldlist sent out To the browser that did Not Return, [" & Mid(FormFieldListToBeLoaded, 2, Len(FormFieldListToBeLoaded) - 2) & "]")
                    Else
                        '
                        ' if page content, check for the 'pagenotfound','landingpageid' checkboxes in control tab
                        '
                        If genericController.vbLCase(adminContent.ContentTableName) = "ccpagecontent" Then
                            '
                            PageNotFoundPageID = (cpCore.siteProperties.getinteger("PageNotFoundPageID", 0))
                            If cpCore.docProperties.getBoolean("PageNotFound") Then
                                editRecord.SetPageNotFoundPageID = True
                            ElseIf editRecord.id = PageNotFoundPageID Then
                                Call cpCore.siteProperties.setProperty("PageNotFoundPageID", "0")
                            End If
                            '
                            If cpCore.docProperties.getBoolean("LandingPageID") Then
                                editRecord.SetLandingPageID = True
                            ElseIf (editRecord.id = 0) Then
                                '
                                ' New record, allow it to be set, but do not compare it to LandingPageID
                                '
                            ElseIf (editRecord.id = cpCore.siteProperties.landingPageID) Then
                                '
                                ' Do not reset the LandingPageID from here -- set another instead
                                '
                                Call errorController.error_AddUserError(cpCore, "This page was marked As the Landing Page For the website, And the checkbox has been cleared. This Is Not allowed. To remove this page As the Landing Page, locate a New landing page And Select it, Or go To Settings &gt; Page Settings And Select a New Landing Page.")
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        ''
        ''========================================================================
        ''   Read in a Response value by name
        ''========================================================================
        ''
        'Private Sub LoadEditResponseByName(FieldName As String)
        '    On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.LoadEditResponseByName")
        '    '
        '    ' converted array to dictionary - Dim FieldPointer As Integer
        '    Dim FieldFound As Boolean
        '    Dim UcaseFieldName As String
        '    Dim DataSourceName As String
        '    Dim FormID As String
        '    '
        '    FieldFound = False
        '    DataSourceName = cpCore.db.getDataSourceNameByID(AdminContent.DataSourceID)
        '    If (FieldName <> "") Then
        '        UcaseFieldName = genericController.vbUCase(FieldName)
        '        If AdminContent.fields.count > 0 Then
        '            For FieldPointer = 0 To AdminContent.fields.count - 1
        '                If genericController.vbUCase(AdminContent.fields(FieldPointer).Name) = UcaseFieldName Then
        '                    Call LoadEditResponseByPointer(FormID, FieldPointer, DataSourceName)
        '                    FieldFound = True
        '                    Exit For
        '                    End If
        '                Next
        '            End If
        '        End If
        '    If Not FieldFound Then
        '        Call HandleInternalError("AdminClass.LoadEditResponseByName", "Field [" & FieldName & "] was Not found In content [" & AdminContent.Name & "]")
        '        End If
        '    '
        '    '''Dim th as integer: Exit Sub
        '    '
        '    ' ----- Error Trap
        '    '
        'ErrorTrap:
        '    Call HandleClassTrapErrorBubble("LoadEditResponseByName")
        '    '
        'End Sub
        '
        '========================================================================
        '   Read the Form into the fields array
        '========================================================================
        '
        Private Sub LoadEditRecord_RequestField(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, field As Models.Complex.CDefFieldModel, ignore As String, ByRef FormFieldListToBeLoaded As String, FormEmptyFieldList As String)
            Try
                Const LoopPtrMax = 100
                Dim blockDuplicateUsername As Boolean
                Dim blockDuplicateEmail As Boolean
                Dim lcaseCopy As String
                Dim HasImg As Boolean
                Dim HasInput As Boolean
                Dim HasAC As Boolean
                Dim EditorPixelHeight As Integer
                Dim EditorRowHeight As Integer
                Dim HTMLDecode As htmlToTextControllers
                Dim Copy As String
                Dim FieldName As String
                Dim ResponseFieldValueIsOKToSave As Boolean

                Dim CSPointer As Integer
                Dim ResponseFieldIsEmpty As Boolean
                Dim ResponseFieldValueText As String
                Dim HTML As New htmlParserController(cpCore)
                Dim TabCopy As String = ""
                Dim ParentID As Integer
                Dim UsedIDs As String
                Dim LoopPtr As Integer
                Dim CS As Integer
                Dim InLoadedFieldList As Boolean
                Dim InEmptyFieldList As Boolean
                Dim InResponse As Boolean
                Dim responseName As String
                '
                '   Read in form values
                '
                With field
                    If Not .active Then
                        '
                        ' Exclude from all field testing, do not load a resposne for this field
                        '
                    Else
                        '
                        ' Read value in and test it for valid response
                        ' Assume OK, mark not ok if there is a problem
                        '
                        ResponseFieldValueIsOKToSave = True
                        FieldName = genericController.vbUCase(.nameLc)
                        responseName = FieldName
                        InLoadedFieldList = (InStr(1, FormFieldListToBeLoaded, "," & FieldName & ",", vbTextCompare) <> 0)
                        InEmptyFieldList = (InStr(1, FormEmptyFieldList, "," & responseName & ",", vbTextCompare) <> 0)
                        InResponse = cpCore.docProperties.containsKey(responseName)
                        FormFieldListToBeLoaded = genericController.vbReplace(FormFieldListToBeLoaded, "," & FieldName & ",", ",", 1, 99, vbTextCompare)
                        ResponseFieldValueText = cpCore.docProperties.getText(responseName)
                        ResponseFieldIsEmpty = String.IsNullOrEmpty(ResponseFieldValueText)
                        If .editTabName <> "" Then
                            TabCopy = " In the " & .editTabName & " tab"
                        End If
                        '
                        If genericController.vbInstr(1, FieldName, "PARENTID", vbTextCompare) <> 0 Then
                            FieldName = FieldName
                        End If
                        '
                        ' process reserved fields
                        '
                        Select Case FieldName
                        '
                        ' ----- block control fields by name
                        '
                            Case "CONTENTCONTROLID"
                                '
                                '
                                '
                                If AllowAdminFieldCheck() Then
                                    If (Not cpCore.docProperties.containsKey(FieldName)) Then
                                        If Not (cpCore.doc.debug_iUserError <> "") Then
                                            '
                                            ' Add user error only for the first missing field
                                            '
                                            Call errorController.error_AddUserError(cpCore, "There has been an Error reading the response from your browser. Please Try again, taking care Not To submit the page until your browser has finished loading. If this Error occurs again, please report this problem To your site administrator. The first Error was [" & FieldName & " Not found]. There may have been others.")
                                        End If
                                        Throw (New ApplicationException("Unexpected exception")) '  cpCore.handleLegacyError2("AdminClass", "LoadEditResponse", cpCore.serverConfig.appConfig.name & ", Field [" & FieldName & "] was In the forms field list, but Not found In the response stream.")
                                        Exit Sub
                                    End If
                                End If
                                '
                                ResponseFieldValueText = cpCore.docProperties.getText(FieldName)
                                'ResponseValueVariant = cpCore.main_ReadStreamText(FieldName)
                                'ResponseValueText = genericController.encodeText(ResponseValueVariant)
                                If genericController.EncodeInteger(ResponseFieldValueText) = genericController.EncodeInteger(editRecord.fieldsLc(.nameLc).value) Then
                                    '
                                    ' No change
                                    '
                                Else
                                    '
                                    ' new value
                                    '
                                    editRecord.fieldsLc(.nameLc).value = ResponseFieldValueText
                                    ResponseFieldIsEmpty = False
                                End If
                            Case "ACTIVE"
                                '
                                '
                                '
                                InEmptyFieldList = (InStr(1, FormEmptyFieldList, "," & FieldName & ",", vbTextCompare) <> 0)
                                InResponse = cpCore.docProperties.containsKey(FieldName)
                                If AllowAdminFieldCheck() Then
                                    If (Not InResponse) And (Not InEmptyFieldList) Then
                                        Call errorController.error_AddUserError(cpCore, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The Error Is [" & FieldName & " Not found].")
                                        Throw (New ApplicationException("Unexpected exception")) '  cpCore.handleLegacyError2("AdminClass", "LoadEditResponse", cpCore.serverConfig.appConfig.name & ", Field [" & FieldName & "] was In the forms field list, but Not found In the response stream.")
                                        Exit Sub
                                    End If
                                End If
                                '
                                Dim responseValue As Boolean = cpCore.docProperties.getBoolean(FieldName)

                                If Not responseValue.Equals(EncodeBoolean(editRecord.fieldsLc(.nameLc).value)) Then
                                    '
                                    ' new value
                                    '
                                    editRecord.fieldsLc(.nameLc).value = responseValue
                                    ResponseFieldIsEmpty = False
                                End If
                            Case "CCGUID"
                                '
                                '
                                '
                                InEmptyFieldList = (InStr(1, FormEmptyFieldList, "," & FieldName & ",", vbTextCompare) <> 0)
                                InResponse = cpCore.docProperties.containsKey(FieldName)
                                If AllowAdminFieldCheck() Then
                                    If (Not InResponse) And (Not InEmptyFieldList) Then
                                        Call errorController.error_AddUserError(cpCore, "There has been an Error reading the response from your browser. Please Try your change again. If this Error occurs again, please report this problem To your site administrator. The Error Is [" & FieldName & " Not found].")
                                        Throw (New ApplicationException("Unexpected exception")) '  cpCore.handleLegacyError2("AdminClass", "LoadEditResponse", cpCore.serverConfig.appConfig.name & ", Field [" & FieldName & "] was In the forms field list, but Not found In the response stream.")
                                        Exit Sub
                                    End If
                                End If
                                '
                                ResponseFieldValueText = cpCore.docProperties.getText(FieldName)
                                If ResponseFieldValueText = editRecord.fieldsLc(.nameLc).value.ToString Then
                                    '
                                    ' No change
                                    '
                                Else
                                    '
                                    ' new value
                                    '
                                    editRecord.fieldsLc(.nameLc).value = ResponseFieldValueText
                                    ResponseFieldIsEmpty = False
                                End If
                            Case "ID", "MODIFIEDBY", "MODIFIEDDATE", "CREATEDBY", "DATEADDED"
                                '
                                ' -----Control fields that cannot be edited
                                '       9/24/2009 - do not save these into the response
                                ResponseFieldValueIsOKToSave = False
                                '
                            Case Else
                                '
                                ' ----- Read response for user fields
                                '       9/24/2009 - if fieldname is not in FormFieldListToBeLoaded, go with what is there (Db value or default value)
                                '
                                If (Not .authorable) Then
                                    '
                                    ' Is blocked from authoring, leave current value
                                    '
                                    ResponseFieldValueIsOKToSave = False
                                ElseIf (.fieldTypeId = FieldTypeIdAutoIdIncrement) Or (.fieldTypeId = FieldTypeIdRedirect) Or (.fieldTypeId = FieldTypeIdManyToMany) Then
                                    '
                                    ' These fields types have no values to load, leave current value
                                    ' (many to many is handled during save)
                                    '
                                    ResponseFieldValueIsOKToSave = False
                                ElseIf (.adminOnly) And (Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) Then
                                    '
                                    ' non-admin and admin only field, leave current value
                                    '
                                    ResponseFieldValueIsOKToSave = False
                                ElseIf (.developerOnly) And (Not cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore)) Then
                                    '
                                    ' non-developer and developer only field, leave current value
                                    '
                                    ResponseFieldValueIsOKToSave = False
                                ElseIf (.ReadOnly) Or (.NotEditable And (editRecord.id <> 0)) Then
                                    '
                                    ' read only field, leave current
                                    '
                                    ResponseFieldValueIsOKToSave = False
                                ElseIf (Not InLoadedFieldList) Then
                                    '
                                    ' Was not sent out, so just go with the current value
                                    ' Also, if the loaded field list is not returned, and the field is not returned, this is the bestwe can do.
                                    '
                                    ResponseFieldValueIsOKToSave = False
                                    'ElseIf (InEmptyFieldList And (Not InResponse)) Then
                                    '    '
                                    '    ' NO - InEmptyFieldList is what comes back from the browser as a list of fields that are blank after the submit button is pressed
                                    '    ' Was sent out blank, and nothing was returned back, so go with current value
                                    '    '
                                    '    ResponseValueIsOKToSave = False
                                ElseIf AllowAdminFieldCheck() And (Not InResponse) And (Not InEmptyFieldList) Then
                                    '
                                    ' Was sent out non-blank, and no response back, flag error and leave the current value to a retry
                                    '
                                    Call errorController.error_AddUserError(cpCore, "There has been an Error reading the response from your browser. The field [" & .caption & "]" & TabCopy & " was missing. Please Try your change again. If this Error happens repeatedly, please report this problem To your site administrator.")
                                    Throw (New ApplicationException("Unexpected exception")) '  cpCore.handleLegacyError2("AdminClass", "LoadEditResponse", cpCore.serverConfig.appConfig.name & ", Field [" & FieldName & "] was In the forms field list, but Not found In the response stream.")
                                    ResponseFieldValueIsOKToSave = False
                                Else
                                    '
                                    ' Test input value for valid data
                                    '
                                    Select Case .fieldTypeId
                                        Case FieldTypeIdInteger
                                            '
                                            ' ----- Integer
                                            '
                                            ResponseFieldIsEmpty = ResponseFieldIsEmpty Or (ResponseFieldValueText = "")
                                            If Not ResponseFieldIsEmpty Then
                                                If genericController.vbIsNumeric(ResponseFieldValueText) Then
                                                    'ResponseValueVariant = genericController.EncodeInteger(ResponseValueVariant)
                                                Else
                                                    errorController.error_AddUserError(cpCore, "The record cannot be saved because the field [" & .caption & "]" & TabCopy & " must be a numeric value.")
                                                    ResponseFieldValueIsOKToSave = False
                                                End If
                                            End If
                                        Case FieldTypeIdCurrency, FieldTypeIdFloat
                                            '
                                            ' ----- Floating point number
                                            '
                                            ResponseFieldIsEmpty = ResponseFieldIsEmpty Or (ResponseFieldValueText = "")
                                            If Not ResponseFieldIsEmpty Then
                                                If genericController.vbIsNumeric(ResponseFieldValueText) Then
                                                    'ResponseValueVariant = EncodeNumber(ResponseValueVariant)
                                                Else
                                                    errorController.error_AddUserError(cpCore, "This record cannot be saved because the field [" & .caption & "]" & TabCopy & " must be a numeric value.")
                                                    ResponseFieldValueIsOKToSave = False
                                                End If
                                            End If
                                        Case FieldTypeIdLookup
                                            '
                                            ' ----- Must be a recordID
                                            '
                                            ResponseFieldIsEmpty = ResponseFieldIsEmpty Or (ResponseFieldValueText = "")
                                            If Not ResponseFieldIsEmpty Then
                                                If genericController.vbIsNumeric(ResponseFieldValueText) Then
                                                    'ResponseValueVariant = genericController.EncodeInteger(ResponseValueVariant)
                                                Else
                                                    errorController.error_AddUserError(cpCore, "This record cannot be saved because the field [" & .caption & "]" & TabCopy & " had an invalid selection.")
                                                    ResponseFieldValueIsOKToSave = False
                                                End If
                                            End If
                                        Case FieldTypeIdDate
                                            '
                                            ' ----- Must be a Date value
                                            '
                                            ResponseFieldIsEmpty = ResponseFieldIsEmpty Or (ResponseFieldValueText = "")
                                            If Not ResponseFieldIsEmpty Then
                                                If Not IsDate(ResponseFieldValueText) Then
                                                    errorController.error_AddUserError(cpCore, "This record cannot be saved because the field [" & .caption & "]" & TabCopy & " must be a date And/Or time in the form mm/dd/yy 0000 AM(PM).")
                                                    ResponseFieldValueIsOKToSave = False
                                                End If
                                            End If
                                        'End Case
                                        Case FieldTypeIdBoolean
                                            '
                                            ' ----- translate to boolean
                                            '
                                            ResponseFieldValueText = genericController.EncodeBoolean(ResponseFieldValueText).ToString
                                        Case FieldTypeIdLink
                                            '
                                            ' ----- Link field - if it starts with 'www.', add the http:// automatically
                                            '
                                            ResponseFieldValueText = genericController.encodeText(ResponseFieldValueText)
                                            If Left(LCase(ResponseFieldValueText), 4) = "www." Then
                                                ResponseFieldValueText = "http//" & ResponseFieldValueText
                                            End If
                                        Case FieldTypeIdHTML, FieldTypeIdFileHTML
                                            '
                                            ' ----- Html fields
                                            '
                                            EditorRowHeight = cpCore.docProperties.getInteger(FieldName & "Rows")
                                            If EditorRowHeight <> 0 Then
                                                Call cpCore.userProperty.setProperty(adminContent.Name & "." & FieldName & ".RowHeight", EditorRowHeight)
                                            End If
                                            EditorPixelHeight = cpCore.docProperties.getInteger(FieldName & "PixelHeight")
                                            If EditorPixelHeight <> 0 Then
                                                Call cpCore.userProperty.setProperty(adminContent.Name & "." & FieldName & ".PixelHeight", EditorPixelHeight)
                                            End If
                                            '
                                            If Not .htmlContent Then
                                                lcaseCopy = genericController.vbLCase(ResponseFieldValueText)
                                                lcaseCopy = genericController.vbReplace(lcaseCopy, vbCr, "")
                                                lcaseCopy = genericController.vbReplace(lcaseCopy, vbLf, "")
                                                lcaseCopy = Trim(lcaseCopy)
                                                If (lcaseCopy = HTMLEditorDefaultCopyNoCr) Or (lcaseCopy = HTMLEditorDefaultCopyNoCr2) Then
                                                    '
                                                    ' if the editor was left blank, remote the default copy
                                                    '
                                                    ResponseFieldValueText = ""
                                                Else
                                                    If genericController.vbInstr(1, ResponseFieldValueText, HTMLEditorDefaultCopyStartMark) <> 0 Then
                                                        '
                                                        ' if the default copy was editing, remote the markers
                                                        '
                                                        ResponseFieldValueText = genericController.vbReplace(ResponseFieldValueText, HTMLEditorDefaultCopyStartMark, "")
                                                        ResponseFieldValueText = genericController.vbReplace(ResponseFieldValueText, HTMLEditorDefaultCopyEndMark, "")
                                                        'ResponseValueVariant = ResponseValueText
                                                    End If
                                                    '
                                                    ' If the response is only white space, remove it
                                                    ' this is a fix for when Site Managers leave white space in the editor, and do not realize it
                                                    '   then cannot fixgure out how to remove it
                                                    '
                                                    ResponseFieldValueText = cpCore.html.convertEditorResponseToActiveContent(ResponseFieldValueText)
                                                    If (String.IsNullOrEmpty(ResponseFieldValueText.ToLower().Replace(" "c, "").Replace("&nbsp;", ""))) Then
                                                        ResponseFieldValueText = String.Empty
                                                    End If
                                                End If
                                            End If
                                        Case Else
                                            '
                                            ' ----- text types
                                            '
                                            EditorRowHeight = cpCore.docProperties.getInteger(FieldName & "Rows")
                                            If EditorRowHeight <> 0 Then
                                                Call cpCore.userProperty.setProperty(adminContent.Name & "." & FieldName & ".RowHeight", EditorRowHeight)
                                            End If
                                            EditorPixelHeight = cpCore.docProperties.getInteger(FieldName & "PixelHeight")
                                            If EditorPixelHeight <> 0 Then
                                                Call cpCore.userProperty.setProperty(adminContent.Name & "." & FieldName & ".PixelHeight", EditorPixelHeight)
                                            End If
                                    End Select
                                    If (LCase(FieldName) = "parentid") Then
                                        '
                                        ' check circular reference on all parentid fields
                                        '

                                        ParentID = genericController.EncodeInteger(ResponseFieldValueText)
                                        LoopPtr = 0
                                        UsedIDs = editRecord.id.ToString
                                        Do While (LoopPtr < LoopPtrMax) And (ParentID <> 0) And (InStr(1, "," & UsedIDs & ",", "," & CStr(ParentID) & ",", vbBinaryCompare) = 0)
                                            UsedIDs = UsedIDs & "," & CStr(ParentID)
                                            CS = cpCore.db.csOpen(adminContent.Name, "ID=" & ParentID, , , , , , "ParentID")
                                            If Not cpCore.db.csOk(CS) Then
                                                ParentID = 0
                                            Else
                                                ParentID = cpCore.db.csGetInteger(CS, "ParentID")
                                            End If
                                            Call cpCore.db.csClose(CS)
                                            LoopPtr = LoopPtr + 1
                                        Loop
                                        If LoopPtr = LoopPtrMax Then
                                            '
                                            ' Too deep
                                            '
                                            errorController.error_AddUserError(cpCore, "This record cannot be saved because the field [" & .caption & "]" & TabCopy & " creates a relationship between records that Is too large. Please limit the depth of this relationship to " & LoopPtrMax & " records.")
                                            ResponseFieldValueIsOKToSave = False
                                        ElseIf (editRecord.id <> 0) And (editRecord.id = ParentID) Then
                                            '
                                            ' Reference to iteslf
                                            '
                                            errorController.error_AddUserError(cpCore, "This record cannot be saved because the field [" & .caption & "]" & TabCopy & " contains a circular reference. This record points back to itself. This Is Not allowed.")
                                            ResponseFieldValueIsOKToSave = False
                                        ElseIf ParentID <> 0 Then
                                            '
                                            ' Circular reference
                                            '
                                            errorController.error_AddUserError(cpCore, "This record cannot be saved because the field [" & .caption & "]" & TabCopy & " contains a circular reference. This field either points to other records which then point back to this record. This Is Not allowed.")
                                            ResponseFieldValueIsOKToSave = False
                                        End If
                                    End If
                                    If .TextBuffered Then
                                        '
                                        ' text buffering
                                        '
                                        ResponseFieldValueText = genericController.main_RemoveControlCharacters(ResponseFieldValueText)
                                    End If
                                    If (.Required) And (ResponseFieldIsEmpty) Then
                                        '
                                        ' field is required and is not given
                                        '
                                        errorController.error_AddUserError(cpCore, "This record cannot be saved because the field [" & .caption & "]" & TabCopy & " Is required but has no value.")
                                        ResponseFieldValueIsOKToSave = False
                                    End If
                                    '
                                    ' special case - people records without Allowduplicateusername require username to be unique
                                    '
                                    If genericController.vbLCase(adminContent.ContentTableName) = "ccmembers" Then
                                        If genericController.vbLCase(.nameLc) = "username" Then
                                            blockDuplicateUsername = Not (cpCore.siteProperties.getBoolean("allowduplicateusername", False))
                                        End If
                                        If genericController.vbLCase(.nameLc) = "email" Then
                                            blockDuplicateEmail = (cpCore.siteProperties.getBoolean("allowemaillogin", False))
                                        End If
                                    End If
                                    If (blockDuplicateUsername Or blockDuplicateEmail Or .UniqueName) And (Not ResponseFieldIsEmpty) Then
                                        '
                                        ' ----- Do the unique check for this field
                                        '
                                        Dim SQLUnique As String = "select id from " & adminContent.ContentTableName & " where (" & FieldName & "=" & cpCore.db.EncodeSQL(ResponseFieldValueText, .fieldTypeId) & ")and(" & Models.Complex.cdefModel.getContentControlCriteria(cpCore, adminContent.Name) & ")"
                                        If editRecord.id > 0 Then
                                            '
                                            ' --editing record
                                            SQLUnique = SQLUnique & "and(id<>" & editRecord.id & ")"
                                        End If
                                        CSPointer = cpCore.db.csOpenSql_rev(adminContent.ContentDataSourceName, SQLUnique)
                                        If cpCore.db.csOk(CSPointer) Then
                                            '
                                            ' field is not unique, skip it and flag error
                                            '
                                            If blockDuplicateUsername Then
                                                '
                                                '
                                                '
                                                errorController.error_AddUserError(cpCore, "This record cannot be saved because the field [" & .caption & "]" & TabCopy & " must be unique And there Is another record with [" & ResponseFieldValueText & "]. This must be unique because the preference Allow Duplicate Usernames Is Not checked.")
                                            ElseIf blockDuplicateEmail Then
                                                '
                                                '
                                                '
                                                errorController.error_AddUserError(cpCore, "This record cannot be saved because the field [" & .caption & "]" & TabCopy & " must be unique And there Is another record with [" & ResponseFieldValueText & "]. This must be unique because the preference Allow Email Login Is checked.")
                                            ElseIf False Then
                                            Else
                                                '
                                                ' non-workflow
                                                '
                                                errorController.error_AddUserError(cpCore, "This record cannot be saved because the field [" & .caption & "]" & TabCopy & " must be unique And there Is another record with [" & ResponseFieldValueText & "].")
                                            End If
                                            ResponseFieldValueIsOKToSave = False
                                        End If
                                        Call cpCore.db.csClose(CSPointer)
                                    End If
                                End If
                                ' end case
                        End Select
                        '
                        ' Save response if it is valid
                        '
                        If ResponseFieldValueIsOKToSave Then
                            editRecord.fieldsLc(.nameLc).value = ResponseFieldValueText
                        End If
                    End If
                End With
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
    End Class
End Namespace
