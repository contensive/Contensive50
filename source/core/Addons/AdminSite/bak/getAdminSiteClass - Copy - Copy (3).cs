

using Controllers;

using System.Xml;
using Contensive.Core;
using Models.Entity;
// 

namespace Contensive.Addons.AdminSite {
    
    public class getAdminSiteClass : Contensive.BaseClasses.AddonBaseClass {
        
        // 
        // 
        // ========================================================================
        //  ProcessActions
        //    perform the action called from the previous form
        //    when action is complete, replace the action code with one that will refresh
        // 
        //    Request Variables
        //        ID = ID of record to edit
        //        AdminAction = action to be performed, defined below, required except for very first call to edit
        //    AdminAction Definitions
        //        edit - edit the record defined by ID, If ID="", edit a new record
        //        Save - saves an edit record and returns to the index
        //        Delete - hmmm.
        //        Cancel - returns to index
        //        Change Filex - uploads a file to a FieldTypeFile, x is a number 0...AdminContent.FieldMax
        //        Delete Filex - clears a file name for a FieldTypeFile, x is a number 0...AdminContent.FieldMax
        //        Upload - The action that actually uploads the file
        //        Email - (not done) Sends "body" field to "email" field in adminContent.id
        // ========================================================================
        // 
        private void ProcessActions(Models.Complex.cdefModel adminContent, editRecordClass editRecord, bool UseContentWatchLink) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter( "ProcessActions")
            // 
            // Dim Upload As KMAUpload3.UploadClass
            // 
            int CS;
            string Filename;
            int FieldCount;
            string SQL;
            //  converted array to dictionary - Dim FieldPointer As Integer
            int EMailToMemberID;
            string EmailFrom;
            string EmailSubject;
            string EmailBody;
            string EmailStatus;
            string EmailStatusMessage;
            string DataSourceName;
            bool ForceReadOnly;
            //  set true for special cases that force a record read only
            string EmailBodySource;
            bool EmailBodyHTML;
            string EmailSubjectSource;
            bool EmailSubjectHTML;
            string ApprovedName;
            int CSRecord;
            int ContentID;
            int RecordID;
            string ContentName;
            string Copy;
            int CSEditRecord;
            int EmailToConfirmationMemberID;
            int ImageWidth;
            int ImageHeight;
            int Position;
            byte[] ByteArray;
            string VirtualFilePath;
            bool EmailAddLinkEID;
            string OpenTriggerCode;
            string OpenTriggerCss;
            string ClickFlagQuery;
            string EmailSpamFooterFlag;
            bool ToAll;
            int CSLog;
            int EmailDropID;
            string EmailToAddress;
            string EmailToName;
            int ScheduletypeID;
            int EMailTemplateID;
            string EmailTemplate;
            int RowCnt;
            int RowPtr;
            // 
            if ((AdminAction != AdminActionNop)) {
                if (!UserAllowContentEdit) {
                    // 
                    //  Action blocked by BlockCurrentRecord
                    // 
                }
                else {
                    // 
                    //  Process actions
                    // 
                    switch (AdminAction) {
                        case AdminActionEditRefresh:
                            // 
                            //  Load the record as if it will be saved, but skip the save
                            // 
                            LoadEditRecord(adminContent, editRecord);
                            LoadEditRecord_Request(adminContent, editRecord);
                            break;
                        case AdminActionMarkReviewed:
                            // 
                            //  Mark the record reviewed without making any changes
                            // 
                            cpCore.doc.markRecordReviewed(adminContent.Name, editRecord.id);
                            // Case AdminActionWorkflowPublishSelected
                            //     '
                            //     ' Publish everything selected
                            //     '
                            //     RowCnt = cpCore.docProperties.getInteger("RowCnt")
                            //     For RowPtr = 0 To RowCnt - 1
                            //         If cpCore.docProperties.getBoolean("Row" & RowPtr) Then
                            //             RecordID = cpCore.docProperties.getInteger("RowID" & RowPtr)
                            //             ContentName = cpCore.docProperties.getText("RowContentName" & RowPtr)
                            //             Call cpCore.workflow.publishEdit(ContentName, RecordID)
                            //             Call cpCore.doc.processAfterSave(False, ContentName, RecordID, "", 0, UseContentWatchLink)
                            //             Call cpCore.cache.invalidateObject(cacheController.getDbRecordCacheName(adminContent.ContentTableName, recordId))
                            //             Call cpCore.db.executeSql("delete from ccAuthoringControls where recordid=" & RecordID & " and Contentid=" & models.complex.cdefmodel.getcontentid(cpcore,ContentName))
                            //         End If
                            //     Next
                            // Case AdminActionWorkflowPublishApproved
                            //     '
                            //     ' Publish all approved workflow publishing records
                            //     '
                            //     CS = cpCore.db.cs_open("Authoring Controls", "ControlType=3", "ID")
                            //     Do While cpCore.db.cs_ok(CS)
                            //         ContentID = cpCore.db.cs_getInteger(CS, "ContentID")
                            //         RecordID = cpCore.db.cs_getInteger(CS, "RecordID")
                            //         ContentName = models.complex.cdefmodel.getContentNameByID(cpcore,ContentID)
                            //         If ContentName <> "" Then
                            //             Call cpCore.workflow.publishEdit(ContentName, RecordID)
                            //             Call cpCore.doc.processAfterSave(False, ContentName, RecordID, "", 0, UseContentWatchLink)
                            //             Call cpCore.cache.invalidateObject(cacheController.getDbRecordCacheName(adminContent.ContentTableName, recordId))
                            //         End If
                            //         cpCore.db.cs_goNext(CS)
                            //     Loop
                            //     Call cpCore.db.cs_Close(CS)
                            //     'AdminForm = AdminFormRoot
                            // Case AdminActionPublishApprove
                            //     If (editRecord.Read_Only) Then
                            //         Call errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is locked.")
                            //     ElseIf Not adminContent.AllowWorkflowAuthoring Then
                            //         Call errorController.error_AddUserError(cpCore, "Your request was blocked because content you selected does not support workflow authoring.")
                            //     Else
                            //         '
                            //         Call LoadEditRecord(adminContent, editRecord)
                            //         Call LoadEditResponse(adminContent, editRecord)
                            //         Call ProcessActionSave(adminContent, editRecord, UseContentWatchLink)
                            //         If Not (cpCore.doc.debug_iUserError <> "") Then
                            //             'no - if WF, on process on publish
                            //             'Call ProcessSpecialCaseAfterSave(false,AdminContent.Name, EditRecord.ID, EditRecord.Name, EditRecord.ParentID, UseContentWatchLink)
                            //             Call cpCore.workflow.approveEdit(adminContent.Name, editRecord.id)
                            //         Else
                            //             AdminForm = AdminSourceForm
                            //         End If
                            //     End If
                            //     AdminAction = AdminActionNop ' convert so action can be used in as a refresh
                            // Case AdminActionPublishSubmit
                            //     If (editRecord.Read_Only) Then
                            //         Call errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is locked.")
                            //     ElseIf Not adminContent.AllowWorkflowAuthoring Then
                            //         Call errorController.error_AddUserError(cpCore, "Your request was blocked because content you selected does not support workflow authoring.")
                            //     Else
                            //         '
                            //         Call LoadEditRecord(adminContent, editRecord)
                            //         Call LoadEditResponse(adminContent, editRecord)
                            //         Call ProcessActionSave(adminContent, editRecord, UseContentWatchLink)
                            //         If Not (cpCore.doc.debug_iUserError <> "") Then
                            //             'no - if WF, on process on publish
                            //             'Call ProcessSpecialCaseAfterSave(false,AdminContent.Name, EditRecord.ID, EditRecord.Name, EditRecord.ParentID, UseContentWatchLink)
                            //             Call cpCore.workflow.main_SubmitEdit(adminContent.Name, editRecord.id)
                            //             Call cpCore.doc.sendPublishSubmitNotice(adminContent.Name, editRecord.id, editRecord.nameLc)
                            //         Else
                            //             AdminForm = AdminSourceForm
                            //         End If
                            //     End If
                            //     AdminAction = AdminActionNop ' convert so action can be used in as a refresh
                            // Case AdminActionPublish
                            //     '
                            //     ' --- Publish edit record to live record - not AuthoringLock blocked
                            //     '
                            //     Call LoadEditRecord(adminContent, editRecord)
                            //     Call LoadEditResponse(adminContent, editRecord)
                            //     Call ProcessActionSave(adminContent, editRecord, UseContentWatchLink)
                            //     If Not (cpCore.doc.debug_iUserError <> "") Then
                            //         Call cpCore.workflow.publishEdit(adminContent.Name, editRecord.id)
                            //         CS = cpCore.db.csOpenRecord(adminContent.Name, editRecord.id)
                            //         Dim IsDeleted As Boolean
                            //         IsDeleted = Not cpCore.db.cs_ok(CS)
                            //         Call cpCore.db.cs_Close(CS)
                            //         Call cpCore.doc.processAfterSave(IsDeleted, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink)
                            //         Call cpCore.cache.invalidateObject(cacheController.getDbRecordCacheName(adminContent.ContentTableName, recordId))
                            //     Else
                            //         AdminForm = AdminSourceForm
                            //     End If
                            // Case AdminActionAbortEdit
                            //     '
                            //     ' --- copy live record over edit record
                            //     '
                            //     Call cpCore.workflow.abortEdit2(adminContent.Name, editRecord.id, cpCore.doc.authContext.user.id)
                            //     Call cpCore.doc.processAfterSave(False, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink)
                            //     If MenuDepth > 0 Then
                            //         '
                            //         ' opened as a child, close the window
                            //         '
                            //         AdminForm = AdminFormClose
                            //     Else
                            //         '
                            //         ' opened as a main window, go to the contents index page
                            //         '
                            //         AdminForm = AdminFormIndex
                            //     End If
                            break;
                        case AdminActionDelete:
                            if (editRecord.Read_Only) {
                                errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                            }
                            else {
                                LoadEditRecord(adminContent, editRecord);
                                cpCore.db.deleteTableRecord(adminContent.ContentTableName, editRecord.id, adminContent.ContentDataSourceName);
                                cpCore.doc.processAfterSave(true, editRecord.contentControlId_Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink);
                            }
                            
                            AdminAction = AdminActionNop;
                            //  convert so action can be used in as a refresh
                            //                 Case AdminActionSetHTMLEdit
                            //                     '
                            //                     ' Set member property for this field to HTML Edit
                            //                     '
                            //                     Call cpCore.main_SetMemberProperty("HTMLEditor." & AdminContent.Name & "." & InputFieldName, True)
                            //                     Call ProcessActionSave(AdminContent, editRecord,UseContentWatchLink)
                            //                     AdminForm = AdminSourceForm
                            //                 Case AdminActionSetTextEdit
                            //                     '
                            //                     ' Set member property for this field to HTML Edit
                            //                     '
                            //                     Call cpCore.main_SetMemberProperty("HTMLEditor." & AdminContent.Name & "." & InputFieldName, False)
                            //                     Call ProcessActionSave(AdminContent, editRecord,UseContentWatchLink)
                            //                     AdminForm = AdminSourceForm
                            //                 Case AdminActionSaveField
                            //                     If (editrecord.read_only) Then
                            //                         Call cpCore.htmldoc.main_AddUserError("Your request was blocked because the record you specified is now locked by another authcontext.user.")
                            //                     Else
                            //                         '
                            //                         ' --- preload array with values that may not come back in response
                            //                         '
                            //                         If (InputFieldName = "") Then
                            //                             Call HandleInternalError("ProcessActions", "SaveField action called but InputFieldName is null")
                            //                         Else
                            //                             Call LoadEditRecord
                            //                             Call LoadEditResponseByName(InputFieldName)
                            //                             '
                            //                             ' --- if no error, save values
                            //                             '
                            //                             If Not cpCore.main_IsUserError Then
                            //                                 Call SaveEditRecord(adminContent, editRecord,ResponseFormID)
                            //                             End If
                            //                             If cpCore.main_IsUserError Then
                            //                                 AdminForm = AdminSourceForm
                            //                             End If
                            //                             'record should be marked modified in cpCore.app.csv_SaveCSRecord
                            //                             'If AdminContent.AllowWorkflowAuthoring Then
                            //                             '    Call cpCore.main_SetAuthoringControl(AdminContent.Name, EditRecord.ID, AuthoringControlsModified)
                            //                             '    End If
                            //                             End If
                            //                         End If
                            //                     AdminAction = AdminActionNop ' convert so action can be used in as a refresh
                            //                     '
                            break;
                        case AdminActionSave:
                            // 
                            //  ----- Save Record
                            // 
                            if (editRecord.Read_Only) {
                                errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                            }
                            else {
                                LoadEditRecord(adminContent, editRecord);
                                LoadEditRecord_Request(adminContent, editRecord);
                                ProcessActionSave(adminContent, editRecord, UseContentWatchLink);
                                cpCore.doc.processAfterSave(false, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink);
                            }
                            
                            AdminAction = AdminActionNop;
                            //  convert so action can be used in as a refresh
                            // 
                            break;
                        case AdminActionSaveAddNew:
                            // 
                            //  ----- Save and add a new record
                            // 
                            if (editRecord.Read_Only) {
                                errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                            }
                            else {
                                LoadEditRecord(adminContent, editRecord);
                                LoadEditRecord_Request(adminContent, editRecord);
                                ProcessActionSave(adminContent, editRecord, UseContentWatchLink);
                                cpCore.doc.processAfterSave(false, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink);
                                editRecord.id = 0;
                                editRecord.Loaded = false;
                            }
                            
                            AdminAction = AdminActionNop;
                            //  convert so action can be used in as a refresh
                            // 
                            break;
                        case AdminActionDuplicate:
                            // 
                            //  ----- Save Record
                            // 
                            if (allowSaveBeforeDuplicate) {
                                if (editRecord.Read_Only) {
                                    errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                                }
                                else {
                                    LoadEditRecord(adminContent, editRecord);
                                    LoadEditRecord_Request(adminContent, editRecord);
                                    ProcessActionSave(adminContent, editRecord, UseContentWatchLink);
                                    cpCore.doc.processAfterSave(false, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink);
                                    ProcessActionDuplicate(adminContent, editRecord);
                                }
                                
                            }
                            else {
                                ProcessActionDuplicate(adminContent, editRecord);
                            }
                            
                            AdminAction = AdminActionNop;
                            //  convert so action can be used in as a refresh
                            // 
                            break;
                        case AdminActionSendEmail:
                            // 
                            //  ----- Send (Group Email Only)
                            // 
                            if (editRecord.Read_Only) {
                                errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                            }
                            else {
                                LoadEditRecord(adminContent, editRecord);
                                LoadEditRecord_Request(adminContent, editRecord);
                                ProcessActionSave(adminContent, editRecord, UseContentWatchLink);
                                cpCore.doc.processAfterSave(false, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink);
                                if (!(cpCore.doc.debug_iUserError != "")) {
                                    if (!Models.Complex.cdefModel.isWithinContent(cpCore, editRecord.contentControlId, Models.Complex.cdefModel.getContentId(cpCore, "Group Email"))) {
                                        errorController.error_AddUserError(cpCore, "The send action only supports Group Email.");
                                    }
                                    else {
                                        CS = cpCore.db.csOpenRecord("Group Email", editRecord.id);
                                        if (!cpCore.db.csOk(CS)) {
                                            throw new ApplicationException("Unexpected exception");
                                            //  throw new applicationException("Unexpected exception")'  cpCore.handleLegacyError23("Email ID [" & editRecord.id & "] could not be found in Group Email.")
                                        }
                                        else if ((cpCore.db.csGet(CS, "FromAddress") == "")) {
                                            errorController.error_AddUserError(cpCore, "A \'From Address\' is required before sending an email.");
                                        }
                                        else if ((cpCore.db.csGet(CS, "Subject") == "")) {
                                            errorController.error_AddUserError(cpCore, "A \'Subject\' is required before sending an email.");
                                        }
                                        else {
                                            cpCore.db.csSet(CS, "submitted", true);
                                            cpCore.db.csSet(CS, "ConditionID", 0);
                                            MinValue;
                                            cpCore.db.csSet(CS, "ScheduleDate", cpCore.doc.profileStartTime);
                                        }
                                        
                                    }
                                    
                                    cpCore.db.csClose(CS);
                                }
                                
                            }
                            
                            break;
                    }
                }
                
                AdminAction = AdminActionNop;
                //  convert so action can be used in as a refresh
                // 
                AdminActionDeactivateEmail;
                // 
                //  ----- Deactivate (Conditional Email Only)
                // 
                if (editRecord.Read_Only) {
                    errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                }
                else {
                    //  no save, page was read only - Call ProcessActionSave
                    LoadEditRecord(adminContent, editRecord);
                    if (!(cpCore.doc.debug_iUserError != "")) {
                        if (!Models.Complex.cdefModel.isWithinContent(cpCore, editRecord.contentControlId, Models.Complex.cdefModel.getContentId(cpCore, "Conditional Email"))) {
                            errorController.error_AddUserError(cpCore, "The deactivate action only supports Conditional Email.");
                        }
                        else {
                            CS = cpCore.db.csOpenRecord("Conditional Email", editRecord.id);
                            if (!cpCore.db.csOk(CS)) {
                                throw new ApplicationException("Unexpected exception");
                                //  throw new applicationException("Unexpected exception")'  cpCore.handleLegacyError23("Email ID [" & editRecord.id & "] could not be opened.")
                            }
                            else {
                                cpCore.db.csSet(CS, "submitted", false);
                            }
                            
                            cpCore.db.csClose(CS);
                        }
                        
                    }
                    
                }
                
                AdminAction = AdminActionNop;
                //  convert so action can be used in as a refresh
                AdminActionActivateEmail;
                // 
                //  ----- Activate (Conditional Email Only)
                // 
                if (editRecord.Read_Only) {
                    errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
                }
                else {
                    LoadEditRecord(adminContent, editRecord);
                    LoadEditRecord_Request(adminContent, editRecord);
                    ProcessActionSave(adminContent, editRecord, UseContentWatchLink);
                    cpCore.doc.processAfterSave(false, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink);
                    if (!(cpCore.doc.debug_iUserError != "")) {
                        if (!Models.Complex.cdefModel.isWithinContent(cpCore, editRecord.contentControlId, Models.Complex.cdefModel.getContentId(cpCore, "Conditional Email"))) {
                            errorController.error_AddUserError(cpCore, "The activate action only supports Conditional Email.");
                        }
                        else {
                            CS = cpCore.db.csOpenRecord("Conditional Email", editRecord.id);
                            if (!cpCore.db.csOk(CS)) {
                                throw new ApplicationException("Unexpected exception");
                                //  throw new applicationException("Unexpected exception")'  cpCore.handleLegacyError23("Email ID [" & editRecord.id & "] could not be opened.")
                            }
                            else if ((cpCore.db.csGetInteger(CS, "ConditionID") == 0)) {
                                errorController.error_AddUserError(cpCore, "A condition must be set.");
                            }
                            else {
                                cpCore.db.csSet(CS, "submitted", true);
                                MinValue;
                                cpCore.db.csSet(CS, "ScheduleDate", cpCore.doc.profileStartTime);
                            }
                            
                        }
                        
                        cpCore.db.csClose(CS);
                    }
                    
                }
                
            }
            
            AdminAction = AdminActionNop;
            //  convert so action can be used in as a refresh
            AdminActionSendEmailTest;
            if (editRecord.Read_Only) {
                errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified is now locked by another authcontext.user.");
            }
            else {
                // 
                LoadEditRecord(adminContent, editRecord);
                LoadEditRecord_Request(adminContent, editRecord);
                ProcessActionSave(adminContent, editRecord, UseContentWatchLink);
                cpCore.doc.processAfterSave(false, adminContent.Name, editRecord.id, editRecord.nameLc, editRecord.parentID, UseContentWatchLink);
                // 
                if (!(cpCore.doc.debug_iUserError != "")) {
                    // 
                    EmailToConfirmationMemberID = 0;
                    if (editRecord.fieldsLc.ContainsKey("testmemberid")) {
                        EmailToConfirmationMemberID = genericController.EncodeInteger(editRecord.fieldsLc.Item["testmemberid"].value);
                        cpCore.email.sendConfirmationTest(editRecord.id, EmailToConfirmationMemberID);
                        // 
                        if (editRecord.fieldsLc.ContainsKey("lastsendtestdate")) {
                            editRecord.fieldsLc.Item["lastsendtestdate"].value = cpCore.doc.profileStartTime;
                            cpCore.db.executeQuery(("update ccemail Set lastsendtestdate=" 
                                            + (cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime) + (" where id=" + editRecord.id))));
                        }
                        
                    }
                    
                }
                
            }
            
            AdminAction = AdminActionNop;
            //  convert so action can be used in as a refresh
            //  end case
            AdminActionDeleteRows;
            // 
            //  Delete Multiple Rows
            // 
            RowCnt = cpCore.docProperties.getInteger("rowcnt");
            if ((RowCnt > 0)) {
                for (RowPtr = 0; (RowPtr 
                            <= (RowCnt - 1)); RowPtr++) {
                    if (cpCore.docProperties.getBoolean(("row" + RowPtr))) {
                        CSEditRecord = cpCore.db.cs_open2(adminContent.Name, cpCore.docProperties.getInteger(("rowid" + RowPtr)), true, true);
                        if (cpCore.db.csOk(CSEditRecord)) {
                            RecordID = cpCore.db.csGetInteger(CSEditRecord, "ID");
                            cpCore.db.csDeleteRecord(CSEditRecord);
                            if (!false) {
                                // 
                                //  non-Workflow Delete
                                // 
                                ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, cpCore.db.csGetInteger(CSEditRecord, "ContentControlID"));
                                cpCore.cache.invalidateContent(cacheController.getCacheKey_Entity(adminContent.ContentTableName, RecordID));
                                cpCore.doc.processAfterSave(true, ContentName, RecordID, "", 0, UseContentWatchLink);
                            }
                            
                            // 
                            //  Page Content special cases
                            // 
                            if ((genericController.vbLCase(adminContent.ContentTableName) == "ccpagecontent")) {
                                // Call cpCore.pages.cache_pageContent_removeRow(RecordID, False, False)
                                if ((RecordID == cpCore.siteProperties.getinteger("PageNotFoundPageID", 0))) {
                                    cpCore.siteProperties.getText("PageNotFoundPageID", "0");
                                }
                                
                                if ((RecordID == cpCore.siteProperties.getinteger("LandingPageID", 0))) {
                                    cpCore.siteProperties.getText("LandingPageID", "0");
                                }
                                
                            }
                            
                        }
                        
                        cpCore.db.csClose(CSEditRecord);
                    }
                    
                }
                
            }
            
            AdminActionReloadCDef;
            // 
            //  ccContent - save changes and reload content definitions
            // 
            if (editRecord.Read_Only) {
                errorController.error_AddUserError(cpCore, "Your request was blocked because the record you specified Is now locked by another authcontext.user.");
            }
            else {
                LoadEditRecord(adminContent, editRecord);
                LoadEditRecord_Request(adminContent, editRecord);
                ProcessActionSave(adminContent, editRecord, UseContentWatchLink);
                cpCore.cache.invalidateAll();
                cpCore.doc.clearMetaData();
            }
            
            AdminAction = AdminActionNop;
            //  convert so action can be used in as a refresh
            // 
            //  Nop action or anything unrecognized - read in database
            // 
        }
    }
}
// 
return;
// 
//  ----- Error Trap
// 
ErrorTrap:
    handleLegacyClassError2("ProcessActions");
    errorController.error_AddUserError(cpCore, ("There was an unknown error processing this page at " 
                    + (cpCore.doc.profileStartTime + ". Please try again, Or report this error To the site administrator.")));
    EndSub// 
    Endclass End {
    }
    
        
        // 
        // ========================================================================
        //  LoadAndSaveContentGroupRules
        // 
        //    For a particular content, remove previous GroupRules, and Create new ones
        // ========================================================================
        // 
        private void LoadAndSaveContentGroupRules(int GroupID) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter( "LoadAndSaveContentGroupRules")
            // 
            int GroupCount;
            int GroupPointer;
            int ContentCount;
            int ContentPointer;
            int CSPointer;
            int ContentID;
            bool AllowAdd;
            bool AllowDelete;
            int CSNew;
            bool RecordChanged;
            bool RuleNeeded;
            bool RuleFound;
            string SQL;
            string DeleteIdList = "";
            int RuleId;
            // 
            //  ----- Delete duplicate Group Rules
            // 
            SQL = ("Select distinct DuplicateRules.ID" + (" from ccgrouprules" + (" Left join ccgrouprules As DuplicateRules On DuplicateRules.ContentID=ccGroupRules.ContentID" + (" where ccGroupRules.ID < DuplicateRules.ID" + " And ccGroupRules.GroupID=DuplicateRules.GroupID"))));
            SQL = ("Delete from ccGroupRules where ID In (" 
                        + (SQL + ")"));
            cpCore.db.executeQuery(SQL);
            // 
            //  --- create GroupRule records for all selected
            // 
            CSPointer = cpCore.db.csOpen("Group Rules", ("GroupID=" + GroupID), "ContentID, ID", true);
            ContentCount = cpCore.docProperties.getInteger("ContentCount");
            if ((ContentCount > 0)) {
                for (ContentPointer = 0; (ContentPointer 
                            <= (ContentCount - 1)); ContentPointer++) {
                    RuleNeeded = cpCore.docProperties.getBoolean(("Content" + ContentPointer));
                    ContentID = cpCore.docProperties.getInteger(("ContentID" + ContentPointer));
                    AllowAdd = cpCore.docProperties.getBoolean(("ContentGroupRuleAllowAdd" + ContentPointer));
                    AllowDelete = cpCore.docProperties.getBoolean(("ContentGroupRuleAllowDelete" + ContentPointer));
                    // 
                    RuleFound = false;
                    cpCore.db.cs_goFirst(CSPointer);
                    if (cpCore.db.csOk(CSPointer)) {
                        while (cpCore.db.csOk(CSPointer)) {
                            if ((cpCore.db.csGetInteger(CSPointer, "ContentID") == ContentID)) {
                                RuleId = cpCore.db.csGetInteger(CSPointer, "id");
                                RuleFound = true;
                                break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                            }
                            
                            cpCore.db.csGoNext(CSPointer);
                        }
                        
                    }
                    
                    if ((RuleNeeded 
                                && !RuleFound)) {
                        CSNew = cpCore.db.csInsertRecord("Group Rules");
                        if (cpCore.db.csOk(CSNew)) {
                            cpCore.db.csSet(CSNew, "GroupID", GroupID);
                            cpCore.db.csSet(CSNew, "ContentID", ContentID);
                            cpCore.db.csSet(CSNew, "AllowAdd", AllowAdd);
                            cpCore.db.csSet(CSNew, "AllowDelete", AllowDelete);
                        }
                        
                        cpCore.db.csClose(CSNew);
                        RecordChanged = true;
                    }
                    else if ((RuleFound 
                                && !RuleNeeded)) {
                        (", " + RuleId);
                        // Call cpCore.main_DeleteCSRecord(CSPointer)
                        RecordChanged = true;
                    }
                    else if ((RuleFound && RuleNeeded)) {
                        if ((AllowAdd != cpCore.db.csGetBoolean(CSPointer, "AllowAdd"))) {
                            cpCore.db.csSet(CSPointer, "AllowAdd", AllowAdd);
                            RecordChanged = true;
                        }
                        
                        if ((AllowDelete != cpCore.db.csGetBoolean(CSPointer, "AllowDelete"))) {
                            cpCore.db.csSet(CSPointer, "AllowDelete", AllowDelete);
                            RecordChanged = true;
                        }
                        
                    }
                    
                }
                
            }
            
            cpCore.db.csClose(CSPointer);
            if ((DeleteIdList != "")) {
                SQL = ("delete from ccgrouprules where id In (" 
                            + (DeleteIdList.Substring(1) + ")"));
                cpCore.db.executeQuery(SQL);
            }
            
            if (RecordChanged) {
                cpCore.cache.invalidateAllObjectsInContent("Group Rules");
            }
            
            return;
            // 
        ErrorTrap:
            handleLegacyClassError3("LoadAndSaveContentGroupRules");
        }
        
        // 
        // ========================================================================
        //  LoadAndSaveGroupRules
        //    read groups from the edit form and modify Group Rules to match
        // ========================================================================
        // 
        private void LoadAndSaveGroupRules(editRecordClass editRecord) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter( "LoadAndSaveGroupRules")
            // 
            if ((editRecord.id != 0)) {
                LoadAndSaveGroupRules_ForContentAndChildren(editRecord.id, "");
            }
            
            // 
            return;
            // 
        ErrorTrap:
            handleLegacyClassError3("LoadAndSaveGroupRules");
        }
        
        // 
        // ========================================================================
        //  LoadAndSaveGroupRules_ForContentAndChildren
        //    read groups from the edit form and modify Group Rules to match
        // ========================================================================
        // 
        private void LoadAndSaveGroupRules_ForContentAndChildren(int ContentID, string ParentIDString) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter( "LoadAndSaveGroupRules_ForContentAndChildren")
            // 
            int CSPointer;
            string MyParentIDString;
            // 
            //  --- Create Group Rules for this content
            // 
            if (bool.Parse((ParentIDString.IndexOf(("," 
                                + (ContentID + ",")), 0) + 1))) {
                throw new Exception(("Child ContentID [" 
                                + (ContentID + "] Is its own parent")));
            }
            else {
                MyParentIDString = (ParentIDString + ("," 
                            + (ContentID + ",")));
                LoadAndSaveGroupRules_ForContent(ContentID);
                // 
                //  --- Create Group Rules for all child content
                // 
                CSPointer = cpCore.db.csOpen("Content", ("ParentID=" + ContentID));
                while (cpCore.db.csOk(CSPointer)) {
                    LoadAndSaveGroupRules_ForContentAndChildren(cpCore.db.csGetInteger(CSPointer, "id"), MyParentIDString);
                    cpCore.db.csGoNext(CSPointer);
                }
                
                cpCore.db.csClose(CSPointer);
            }
            
            // 
            return;
            // 
        ErrorTrap:
            handleLegacyClassError3("LoadAndSaveGroupRules_ForContentAndChildren");
        }
        
        // 
        // ========================================================================
        //  LoadAndSaveGroupRules_ForContent
        // 
        //    For a particular content, remove previous GroupRules, and Create new ones
        // ========================================================================
        // 
        private void LoadAndSaveGroupRules_ForContent(int ContentID) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter( "LoadAndSaveGroupRules_ForContent")
            // 
            int GroupCount;
            int GroupPointer;
            int CSPointer;
            int GroupID;
            bool AllowAdd;
            bool AllowDelete;
            int CSNew;
            bool RecordChanged;
            bool RuleNeeded;
            bool RuleFound;
            string SQL;
            // 
            //  ----- Delete duplicate Group Rules
            // 
            SQL = ("Delete from ccGroupRules where ID In (" + ("Select distinct DuplicateRules.ID from ccgrouprules Left join ccgrouprules As DuplicateRules On Dupli" +
            "cateRules.GroupID=ccGroupRules.GroupID where ccGroupRules.ID < DuplicateRules.ID  And ccGroupRules.C" +
            "ontentID=DuplicateRules.ContentID" + ")"));
            cpCore.db.executeQuery(SQL);
            // 
            //  --- create GroupRule records for all selected
            // 
            CSPointer = cpCore.db.csOpen("Group Rules", ("ContentID=" + ContentID), "GroupID,ID", true);
            GroupCount = cpCore.docProperties.getInteger("GroupCount");
            if ((GroupCount > 0)) {
                for (GroupPointer = 0; (GroupPointer 
                            <= (GroupCount - 1)); GroupPointer++) {
                    RuleNeeded = cpCore.docProperties.getBoolean(("Group" + GroupPointer));
                    GroupID = cpCore.docProperties.getInteger(("GroupID" + GroupPointer));
                    AllowAdd = cpCore.docProperties.getBoolean(("GroupRuleAllowAdd" + GroupPointer));
                    AllowDelete = cpCore.docProperties.getBoolean(("GroupRuleAllowDelete" + GroupPointer));
                    // 
                    RuleFound = false;
                    cpCore.db.cs_goFirst(CSPointer);
                    if (cpCore.db.csOk(CSPointer)) {
                        while (cpCore.db.csOk(CSPointer)) {
                            if ((cpCore.db.csGetInteger(CSPointer, "GroupID") == GroupID)) {
                                RuleFound = true;
                                break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                            }
                            
                            cpCore.db.csGoNext(CSPointer);
                        }
                        
                    }
                    
                    if ((RuleNeeded 
                                && !RuleFound)) {
                        CSNew = cpCore.db.csInsertRecord("Group Rules");
                        if (cpCore.db.csOk(CSNew)) {
                            cpCore.db.csSet(CSNew, "ContentID", ContentID);
                            cpCore.db.csSet(CSNew, "GroupID", GroupID);
                            cpCore.db.csSet(CSNew, "AllowAdd", AllowAdd);
                            cpCore.db.csSet(CSNew, "AllowDelete", AllowDelete);
                        }
                        
                        cpCore.db.csClose(CSNew);
                        RecordChanged = true;
                    }
                    else if ((RuleFound 
                                && !RuleNeeded)) {
                        cpCore.db.csDeleteRecord(CSPointer);
                        RecordChanged = true;
                    }
                    else if ((RuleFound && RuleNeeded)) {
                        if ((AllowAdd != cpCore.db.csGetBoolean(CSPointer, "AllowAdd"))) {
                            cpCore.db.csSet(CSPointer, "AllowAdd", AllowAdd);
                            RecordChanged = true;
                        }
                        
                        if ((AllowDelete != cpCore.db.csGetBoolean(CSPointer, "AllowDelete"))) {
                            cpCore.db.csSet(CSPointer, "AllowDelete", AllowDelete);
                            RecordChanged = true;
                        }
                        
                    }
                    
                }
                
            }
            
            cpCore.db.csClose(CSPointer);
            if (RecordChanged) {
                cpCore.cache.invalidateAllObjectsInContent("Group Rules");
            }
            
            return;
            // 
        ErrorTrap:
            handleLegacyClassError3("LoadAndSaveGroupRules_ForContent");
        }
        
        // 
        // ========================================================================
        //  Load Array
        //    Get defaults if no record ID
        //    Then load in any response elements
        // ========================================================================
        // 
        private void LoadEditRecord(Models.Complex.cdefModel adminContent, editRecordClass editRecord, bool CheckUserErrors, void =, void False) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // Warning!!! Optional parameters not supported
            // 'Dim th as integer : th = profileLogAdminMethodEnter( "LoadEditRecord")
            // 
            //  converted array to dictionary - Dim FieldPointer As Integer
            DateTime ApprovedDate;
            int CS;
            // 'Dim arrayOfFields() As appServices_metaDataClass.CDefFieldClass
            // 
            if ((adminContent.Name == "")) {
                // 
                //  Can not load edit record because bad content definition
                // 
                if ((adminContent.Id == 0)) {
                    throw new Exception("The record can Not be edited because no content definition was specified.");
                }
                else {
                    throw new Exception(("The record can Not be edited because a content definition For ID [" 
                                    + (adminContent.Id + "] was Not found.")));
                }
                
            }
            else {
                // 
                if ((editRecord.id == 0)) {
                    // 
                    //  ----- New record, just load defaults
                    // 
                    LoadEditRecord_Default(adminContent, editRecord);
                    LoadEditRecord_WherePairs(adminContent, editRecord);
                }
                else {
                    // 
                    //  ----- Load the Live Record specified
                    // 
                    LoadEditRecord_Dbase(adminContent, editRecord, CheckUserErrors);
                    LoadEditRecord_WherePairs(adminContent, editRecord);
                }
                
                //         '
                //         ' ----- Test for a change of admincontent (the record is a child of admincontent )
                //         '
                //         If EditRecord.ContentID <> AdminContent.Id Then
                //             AdminContent = cpCore.app.getCdef(EditRecord.ContentName)
                //         End If
                // 
                //  ----- Capture core fields needed for processing
                // 
                editRecord.menuHeadline = "";
                if (editRecord.fieldsLc.ContainsKey("menuheadline")) {
                    editRecord.menuHeadline = genericController.encodeText(editRecord.fieldsLc.Item["menuheadline"].value);
                }
                
                // 
                editRecord.menuHeadline = "";
                if (editRecord.fieldsLc.ContainsKey("name")) {
                    // Dim editRecordField As editRecordFieldClass = editRecord.fieldsLc.Item("name")
                    // editRecord.nameLc = editRecordField.value.ToString()
                    editRecord.nameLc = genericController.encodeText(editRecord.fieldsLc.Item["name"].value);
                }
                
                // 
                editRecord.menuHeadline = "";
                if (editRecord.fieldsLc.ContainsKey("active")) {
                    editRecord.active = genericController.EncodeBoolean(editRecord.fieldsLc.Item["active"].value);
                }
                
                // 
                editRecord.menuHeadline = "";
                if (editRecord.fieldsLc.ContainsKey("contentcontrolid")) {
                    editRecord.contentControlId = genericController.EncodeInteger(editRecord.fieldsLc.Item["contentcontrolid"].value);
                }
                
                // 
                editRecord.menuHeadline = "";
                if (editRecord.fieldsLc.ContainsKey("parentid")) {
                    editRecord.parentID = genericController.EncodeInteger(editRecord.fieldsLc.Item["parentid"].value);
                }
                
                // 
                editRecord.menuHeadline = "";
                if (editRecord.fieldsLc.ContainsKey("rootpageid")) {
                    editRecord.RootPageID = genericController.EncodeInteger(editRecord.fieldsLc.Item["rootpageid"].value);
                }
                
                // 
                //  ----- Set the local global copy of Edit Record Locks
                // 
                cpCore.doc.getAuthoringStatus(adminContent.Name, editRecord.id, editRecord.SubmitLock, editRecord.ApproveLock, editRecord.SubmittedName, editRecord.ApprovedName, editRecord.IsInserted, editRecord.IsDeleted, editRecord.IsModified, editRecord.LockModifiedName, editRecord.LockModifiedDate, editRecord.SubmittedDate, editRecord.ApprovedDate);
                // 
                //  ----- Set flags used to determine the Authoring State
                // 
                cpCore.doc.getAuthoringPermissions(adminContent.Name, editRecord.id, editRecord.AllowInsert, editRecord.AllowCancel, editRecord.AllowSave, editRecord.AllowDelete, editRecord.AllowPublish, editRecord.AllowAbort, editRecord.AllowSubmit, editRecord.AllowApprove, editRecord.Read_Only);
                // 
                //  ----- Set Edit Lock
                // 
                if ((editRecord.id != 0)) {
                    editRecord.EditLock = cpCore.workflow.GetEditLockStatus(adminContent.Name, editRecord.id);
                    if (editRecord.EditLock) {
                        editRecord.EditLockMemberName = cpCore.workflow.GetEditLockMemberName(adminContent.Name, editRecord.id);
                        editRecord.EditLockExpires = cpCore.workflow.GetEditLockDateExpires(adminContent.Name, editRecord.id);
                    }
                    
                }
                
                // 
                //  ----- Set Read Only: for edit lock
                // 
                if (editRecord.EditLock) {
                    editRecord.Read_Only = true;
                }
                
                // 
                //  ----- Set Read Only: if non-developer tries to edit a developer record
                // 
                if ((genericController.vbUCase(adminContent.ContentTableName) == genericController.vbUCase("ccMembers"))) {
                    if (!cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore)) {
                        if (editRecord.fieldsLc.ContainsKey("developer")) {
                            if (genericController.EncodeBoolean(editRecord.fieldsLc.Item["developer"].value)) {
                                editRecord.Read_Only = true;
                                errorController.error_AddUserError(cpCore, "You Do Not have access rights To edit this record.");
                                BlockEditForm = true;
                            }
                            
                        }
                        
                    }
                    
                }
                
                // 
                //  ----- Now make sure this record is locked from anyone else
                // 
                if (!editRecord.Read_Only) {
                    cpCore.workflow.SetEditLock(adminContent.Name, editRecord.id);
                }
                
                editRecord.Loaded = true;
            }
            
            // 
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            handleLegacyClassError3("LoadEditRecord");
            // 
        }
        
        // 
        // ========================================================================
        //    Get the Wherepair value for a fieldname
        //        If there is a match with the left side, return the right
        //        If no match, return ""
        // ========================================================================
        // 
        private string GetWherePairValue(string FieldName) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter( "GetWherePairValue")
            // 
            int WhereCount;
            // 
            FieldName = genericController.vbUCase(FieldName);
            // 
            GetWherePairValue = "";
            if ((WherePairCount > 0)) {
                for (WhereCount = 0; (WhereCount 
                            <= (WherePairCount - 1)); WhereCount++) {
                    if ((FieldName == genericController.vbUCase(WherePair(0, WhereCount)))) {
                        GetWherePairValue = WherePair(1, WhereCount);
                        break;
                    }
                    
                }
                
            }
            
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            handleLegacyClassError3("GetWherePairValue");
            // 
        }
        
        // 
        // ========================================================================
        //    Load both Live and Edit Record values from definition defaults
        // ========================================================================
        // 
        private void LoadEditRecord_Default(Models.Complex.cdefModel adminContent, editRecordClass editrecord) {
            try {
                string DefaultValueText;
                string LookupContentName;
                string UCaseDefaultValueText;
                string[] lookups;
                int Ptr;
                // ' converted array to dictionary - Dim FieldPointer As Integer
                // Dim FieldCount As Integer
                string defaultValue;
                string MethodName;
                editRecordFieldClass editRecordField;
                Models.Complex.CDefFieldModel field;
                // 
                MethodName = "Admin.Method()";
                editrecord.active = true;
                editrecord.contentControlId = adminContent.Id;
                editrecord.contentControlId_Name = adminContent.Name;
                editrecord.EditLock = false;
                editrecord.Loaded = false;
                editrecord.Saved = false;
                foreach (keyValuePair in adminContent.fields) {
                    field = keyValuePair.Value;
                    // With...
                    if (!editrecord.fieldsLc.ContainsKey(field.nameLc)) {
                        editRecordField = new editRecordFieldClass();
                        editrecord.fieldsLc.Add(field.nameLc, editRecordField);
                    }
                    
                    defaultValue = field.defaultValue;
                    //     End If
                    if ((field.active 
                                && !genericController.IsNull(defaultValue))) {
                        switch (field.fieldTypeId) {
                            case FieldTypeIdInteger:
                            case FieldTypeIdAutoIdIncrement:
                            case FieldTypeIdMemberSelect:
                                // 
                                editrecord.fieldsLc(field.nameLc).value = genericController.EncodeInteger(defaultValue);
                                break;
                            case FieldTypeIdCurrency:
                            case FieldTypeIdFloat:
                                // 
                                editrecord.fieldsLc(field.nameLc).value = genericController.EncodeNumber(defaultValue);
                                break;
                            case FieldTypeIdBoolean:
                                // 
                                editrecord.fieldsLc(field.nameLc).value = genericController.EncodeBoolean(defaultValue);
                                break;
                            case FieldTypeIdDate:
                                // 
                                editrecord.fieldsLc(field.nameLc).value = genericController.EncodeDate(defaultValue);
                                break;
                            case FieldTypeIdLookup:
                                DefaultValueText = genericController.encodeText(field.defaultValue);
                                if ((DefaultValueText != "")) {
                                    if (genericController.vbIsNumeric(DefaultValueText)) {
                                        editrecord.fieldsLc(field.nameLc).value = DefaultValueText;
                                    }
                                    else if ((field.lookupContentID != 0)) {
                                        LookupContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, field.lookupContentID);
                                        if ((LookupContentName != "")) {
                                            editrecord.fieldsLc(field.nameLc).value = cpCore.db.getRecordID(LookupContentName, DefaultValueText);
                                        }
                                        
                                    }
                                    else if ((field.lookupList != "")) {
                                        UCaseDefaultValueText = genericController.vbUCase(DefaultValueText);
                                        lookups = field.lookupList.Split(",");
                                        for (Ptr = 0; (Ptr <= UBound(lookups)); Ptr++) {
                                            if ((UCaseDefaultValueText == genericController.vbUCase(lookups[Ptr]))) {
                                                editrecord.fieldsLc(field.nameLc).value = (Ptr + 1);
                                                break;
                                            }
                                            
                                        }
                                        
                                    }
                                    
                                }
                                
                                break;
                            default:
                                editrecord.fieldsLc(field.nameLc).value = genericController.encodeText(defaultValue);
                                break;
                        }
                    }
                    
                    // 
                    //  process reserved fields (set defaults just makes it look good)
                    //  (also, this presets readonly/devonly/adminonly fields not set to member)
                    // 
                    switch (genericController.vbUCase(field.nameLc)) {
                        case "MODIFIEDBY":
                            editrecord.fieldsLc(field.nameLc).value = cpCore.doc.authContext.user.id;
                            //     .readonlyfield = True
                            //     .Required = False
                            break;
                        case "CREATEDBY":
                            editrecord.fieldsLc(field.nameLc).value = cpCore.doc.authContext.user.id;
                            //     .readonlyfield = True
                            //     .Required = False
                            // Case "DATEADDED"
                            //     .readonlyfield = True
                            //     .Required = False
                            break;
                        case "CONTENTCONTROLID":
                            editrecord.fieldsLc(field.nameLc).value = adminContent.Id;
                            // Case "SORTORDER"
                            //  default to ID * 100, but must be done later
                            break;
                    }
                    editrecord.fieldsLc(field.nameLc).dbValue = editrecord.fieldsLc(field.nameLc).value;
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // ========================================================================
        //    Load both Live and Edit Record values from definition defaults
        // ========================================================================
        // 
        private void LoadEditRecord_WherePairs(Models.Complex.cdefModel Admincontent, editRecordClass editRecord) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter( "LoadEditRecord_WherePairs")
            // 
            //  converted array to dictionary - Dim FieldPointer As Integer
            int FieldCount;
            string DefaultValueText;
            string MethodName;
            // 
            MethodName = "Admin.LoadEditRecord_WherePairs(adminContent, editRecord)";
            foreach (keyValuePair in Admincontent.fields) {
                Models.Complex.CDefFieldModel field = keyValuePair.Value;
                // With...
                DefaultValueText = GetWherePairValue(field.nameLc);
                if ((field.active 
                            && (DefaultValueText != ""))) {
                    switch (field.fieldTypeId) {
                        case FieldTypeIdInteger:
                        case FieldTypeIdLookup:
                        case FieldTypeIdAutoIdIncrement:
                            // 
                            editRecord.fieldsLc.Item[field.nameLc].value = genericController.EncodeInteger(DefaultValueText);
                            break;
                        case FieldTypeIdCurrency:
                        case FieldTypeIdFloat:
                            // 
                            editRecord.fieldsLc.Item[field.nameLc].value = genericController.EncodeNumber(DefaultValueText);
                            break;
                        case FieldTypeIdBoolean:
                            // 
                            editRecord.fieldsLc.Item[field.nameLc].value = genericController.EncodeBoolean(DefaultValueText);
                            break;
                        case FieldTypeIdDate:
                            // 
                            editRecord.fieldsLc.Item[field.nameLc].value = genericController.EncodeDate(DefaultValueText);
                            break;
                        case FieldTypeIdManyToMany:
                            // 
                            //  Many to Many can capture a list of ID values representing the 'secondary' values in the Many-To-Many Rules table
                            // 
                            editRecord.fieldsLc.Item[field.nameLc].value = DefaultValueText;
                            break;
                        default:
                            editRecord.fieldsLc.Item[field.nameLc].value = DefaultValueText;
                            break;
                    }
                }
                
            }
            
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            handleLegacyClassError3("LoadEditRecord_WherePairs");
        }
        
        // 
        // ========================================================================
        //    Load Records from the database
        // ========================================================================
        // 
        private void LoadEditRecord_Dbase(Models.Complex.cdefModel adminContent, ref editRecordClass editrecord, bool CheckUserErrors, void =, void False) {
            try {
                // 
                // Warning!!! Optional parameters not supported
                object DBValueVariant;
                int CSEditRecord;
                object NullVariant;
                int CSPointer;
                // Dim WorkflowAuthoring As Boolean
                // 
                //  ----- test for content problem
                // 
                if ((editrecord.id == 0)) {
                    // 
                    //  ----- Skip load, this is a new record
                    // 
                }
                else if ((adminContent.Id == 0)) {
                    // 
                    //  ----- Error: no content ID
                    // 
                    BlockEditForm = true;
                    errorController.error_AddUserError(cpCore, ("No content definition was found For Content ID [" 
                                    + (editrecord.id + "]. Please contact your application developer For more assistance.")));
                    handleLegacyClassError("AdminClass.LoadEditRecord_Dbase", ("No content definition was found For Content ID [" 
                                    + (editrecord.id + "].")));
                }
                else if ((adminContent.Name == "")) {
                    // 
                    //  ----- Error: no content name
                    // 
                    BlockEditForm = true;
                    errorController.error_AddUserError(cpCore, ("No content definition could be found For ContentID [" 
                                    + (adminContent.Id + "]. This could be a menu Error. Please contact your application developer For more assistance.")));
                    handleLegacyClassError("AdminClass.LoadEditRecord_Dbase", ("No content definition For ContentID [" 
                                    + (adminContent.Id + "] could be found.")));
                }
                else if ((adminContent.ContentTableName == "")) {
                    // 
                    //  ----- Error: no content table
                    // 
                    BlockEditForm = true;
                    errorController.error_AddUserError(cpCore, ("The content definition [" 
                                    + (adminContent.Name + "] Is Not associated With a valid database table. Please contact your application developer For more a" +
                                    "ssistance.")));
                    handleLegacyClassError("AdminClass.LoadEditRecord_Dbase", ("No content definition For ContentID [" 
                                    + (adminContent.Id + "] could be found.")));
                    // 
                    //  move block to the edit and listing pages - to handle content editor cases - so they can edit 'pages', and just get the records they are allowed
                    // 
                    //     ElseIf Not UserAllowContentEdit Then
                    //         '
                    //         ' ----- Error: load blocked by UserAllowContentEdit
                    //         '
                    //         BlockEditForm = True
                    //         Call cpCore.htmldoc.main_AddUserError("Your account On this system does Not have access rights To edit this content.")
                    //         Call HandleInternalError("AdminClass.LoadEditRecord_Dbase", "User does Not have access To this content")
                }
                else if ((adminContent.fields.Count == 0)) {
                    // 
                    //  ----- Error: content definition is not complete
                    // 
                    BlockEditForm = true;
                    errorController.error_AddUserError(cpCore, ("The content definition [" 
                                    + (adminContent.Name + "] has no field records defined. Please contact your application developer For more assistance.")));
                    handleLegacyClassError("AdminClass.LoadEditRecord_Dbase", ("Content [" 
                                    + (adminContent.Name + "] has no fields defined.")));
                }
                else {
                    // 
                    //    Open Content Sets with the data
                    // 
                    CSEditRecord = cpCore.db.cs_open2(adminContent.Name, editrecord.id, true, true);
                    // 
                    // 
                    //  store fieldvalues in RecordValuesVariant
                    // 
                    if (!cpCore.db.csOk(CSEditRecord)) {
                        // 
                        //    Live or Edit records were not found
                        // 
                        BlockEditForm = true;
                        errorController.error_AddUserError(cpCore, "The information you have requested could not be found. The record could have been deleted, Or there m" +
                            "ay be a system Error.");
                        //  removed because it was throwing too many false positives (1/14/04 - tried to do it again)
                        //  If a CM hits the edit tag for a deleted record, this is hit. It should not cause the Developers to spend hours running down.
                        // Call HandleInternalError("AdminClass.LoadEditRecord_Dbase", "Content edit record For [" & AdminContent.Name & "." & EditRecord.ID & "] was Not found.")
                    }
                    else {
                        // 
                        //  Read database values into RecordValuesVariant array
                        // 
                        NullVariant = null;
                        foreach (keyValuePair in adminContent.fields) {
                            Models.Complex.CDefFieldModel adminContentField = keyValuePair.Value;
                            string fieldNameLc = adminContentField.nameLc;
                            editRecordFieldClass editRecordField;
                            // 
                            //  set editRecord.field to editRecordField and set values
                            // 
                            if (!editrecord.fieldsLc.ContainsKey(fieldNameLc)) {
                                editRecordField = new editRecordFieldClass();
                                editrecord.fieldsLc.Add(fieldNameLc, editRecordField);
                            }
                            else {
                                editRecordField = editrecord.fieldsLc(fieldNameLc);
                            }
                            
                            // 
                            //  1/21/2007 - added clause if required and null, set to default value
                            // 
                            object fieldValue;
                            fieldValue = NullVariant;
                            // With...
                            if ((adminContentField.ReadOnly || adminContentField.NotEditable)) {
                                // 
                                //  202-31245: quick fix. The CS should handle this instead.
                                //  Workflowauthoring, If read only, use the live record data
                                // 
                                CSPointer = CSEditRecord;
                            }
                            else {
                                CSPointer = CSEditRecord;
                            }
                            
                            // 
                            //  Load the current Database value
                            // 
                            switch (adminContentField.fieldTypeId) {
                                case FieldTypeIdRedirect:
                                case FieldTypeIdManyToMany:
                                    DBValueVariant = "";
                                    break;
                                case FieldTypeIdFileText:
                                case FieldTypeIdFileCSS:
                                case FieldTypeIdFileXML:
                                case FieldTypeIdFileJavascript:
                                case FieldTypeIdFileHTML:
                                    DBValueVariant = cpCore.db.csGet(CSPointer, adminContentField.nameLc);
                                    break;
                                default:
                                    DBValueVariant = cpCore.db.cs_getValue(CSPointer, adminContentField.nameLc);
                                    break;
                            }
                            // 
                            //  Check for required and null case loading error
                            // 
                            if ((CheckUserErrors 
                                        && (adminContentField.Required && genericController.IsNull(DBValueVariant)))) {
                                // 
                                //  if required and null
                                // 
                                if (string.IsNullOrEmpty(adminContentField.defaultValue)) {
                                    // 
                                    //  default is null
                                    // 
                                    if ((adminContentField.editTabName == "")) {
                                        errorController.error_AddUserError(cpCore, ("The value for [" 
                                                        + (adminContentField.caption + "] was empty but is required. This must be set before you can save this record.")));
                                    }
                                    else {
                                        errorController.error_AddUserError(cpCore, ("The value for [" 
                                                        + (adminContentField.caption + ("] in tab [" 
                                                        + (adminContentField.editTabName + "] was empty but is required. This must be set before you can save this record.")))));
                                    }
                                    
                                }
                                else {
                                    // 
                                    //  if required and null, set value to the default
                                    // 
                                    DBValueVariant = adminContentField.defaultValue;
                                    if ((adminContentField.editTabName == "")) {
                                        errorController.error_AddUserError(cpCore, ("The value for [" 
                                                        + (adminContentField.caption + "] was null but is required. The default value Is shown, And will be saved if you save this record.")));
                                    }
                                    else {
                                        errorController.error_AddUserError(cpCore, ("The value for [" 
                                                        + (adminContentField.caption + ("] in tab [" 
                                                        + (adminContentField.editTabName + "] was null but is required. The default value Is shown, And will be saved if you save this record.")))));
                                    }
                                    
                                }
                                
                            }
                            
                            // 
                            //  Save EditRecord values
                            // 
                            switch (genericController.vbUCase(adminContentField.nameLc)) {
                                case "DATEADDED":
                                    editrecord.dateAdded = cpCore.db.csGetDate(CSEditRecord, adminContentField.nameLc);
                                    break;
                                case "MODIFIEDDATE":
                                    editrecord.modifiedDate = cpCore.db.csGetDate(CSEditRecord, adminContentField.nameLc);
                                    break;
                                case "CREATEDBY":
                                    editrecord.createByMemberId = cpCore.db.csGetInteger(CSEditRecord, adminContentField.nameLc);
                                    break;
                                case "MODIFIEDBY":
                                    editrecord.modifiedByMemberID = cpCore.db.csGetInteger(CSEditRecord, adminContentField.nameLc);
                                    break;
                                case "ACTIVE":
                                    editrecord.active = cpCore.db.csGetBoolean(CSEditRecord, adminContentField.nameLc);
                                    break;
                                case "CONTENTCONTROLID":
                                    editrecord.contentControlId = cpCore.db.csGetInteger(CSEditRecord, adminContentField.nameLc);
                                    editrecord.contentControlId_Name = Models.Complex.cdefModel.getContentNameByID(cpCore, editrecord.contentControlId);
                                    break;
                                case "ID":
                                    editrecord.id = cpCore.db.csGetInteger(CSEditRecord, adminContentField.nameLc);
                                    break;
                                case "MENUHEADLINE":
                                    editrecord.menuHeadline = cpCore.db.csGetText(CSEditRecord, adminContentField.nameLc);
                                    break;
                                case "NAME":
                                    editrecord.nameLc = cpCore.db.csGetText(CSEditRecord, adminContentField.nameLc);
                                    break;
                                case "PARENTID":
                                    editrecord.parentID = cpCore.db.csGetInteger(CSEditRecord, adminContentField.nameLc);
                                    // Case Else
                                    //     EditRecordValuesVariant(FieldPointer) = DBValueVariant
                                    break;
                            }
                            // 
                            editRecordField.dbValue = DBValueVariant;
                            editRecordField.value = DBValueVariant;
                        }
                        
                    }
                    
                    cpCore.db.csClose(CSEditRecord);
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // ========================================================================
        //    Read the Form into the fields array
        // ========================================================================
        // 
        private void LoadEditRecord_Request(Models.Complex.cdefModel adminContent, editRecordClass editRecord) {
            try {
                int PageNotFoundPageID;
                string FormFieldListToBeLoaded;
                string FormEmptyFieldList;
                // 
                //  List of fields that were created for the form, and should be verified (starts and ends with a comma)
                // 
                FormFieldListToBeLoaded = cpCore.docProperties.getText("FormFieldList");
                if ((FormFieldListToBeLoaded == "")) {
                    FormFieldListToBeLoaded = ",";
                }
                else {
                    // FormFieldListToBeLoaded = "," & FormFieldListToBeLoaded & ","
                }
                
                // 
                //  List of fields coming from the form that are empty -- and should not be in stream (starts and ends with a comma)
                // 
                FormEmptyFieldList = cpCore.docProperties.getText("FormEmptyFieldList");
                // 
                if ((AllowAdminFieldCheck() 
                            && (FormFieldListToBeLoaded == ","))) {
                    // 
                    //  The field list was not returned
                    // 
                    errorController.error_AddUserError(cpCore, "There has been an Error reading the response from your browser. Please Try your change again. If this" +
                        " Error occurs again, please report this problem To your site administrator. The Error Is [no field l" +
                        "ist].");
                }
                else if ((AllowAdminFieldCheck() 
                            && (FormEmptyFieldList == ""))) {
                    // 
                    //  The field list was not returned
                    // 
                    errorController.error_AddUserError(cpCore, "There has been an Error reading the response from your browser. Please Try your change again. If this" +
                        " Error occurs again, please report this problem To your site administrator. The Error Is [no empty f" +
                        "ield list].");
                }
                else {
                    // 
                    //  fixup the string so it can be reduced by each field found, leaving and empty string if all correct
                    // 
                    Models.Entity.dataSourceModel datasource = Models.Entity.dataSourceModel.create(cpCore, adminContent.dataSourceId, new List<string>());
                    // DataSourceName = cpCore.db.getDataSourceNameByID(adminContent.dataSourceId)
                    foreach (keyValuePair in adminContent.fields) {
                        Models.Complex.CDefFieldModel field = keyValuePair.Value;
                        LoadEditRecord_RequestField(adminContent, editRecord, field, datasource.Name, FormFieldListToBeLoaded, FormEmptyFieldList);
                    }
                    
                    // 
                    //  If there are any form fields that were no loaded, flag the error now
                    // 
                    if ((AllowAdminFieldCheck() 
                                && (FormFieldListToBeLoaded != ","))) {
                        errorController.error_AddUserError(cpCore, ("There has been an Error reading the response from your browser. Please Try your change again. If this" +
                            " Error occurs again, please report this problem To your site administrator. The following fields whe" +
                            "re Not found [" 
                                        + (FormFieldListToBeLoaded.Substring(1, (FormFieldListToBeLoaded.Length - 2)) + "].")));
                        throw new ApplicationException("Unexpected exception");
                    }
                    else {
                        // 
                        //  if page content, check for the 'pagenotfound','landingpageid' checkboxes in control tab
                        // 
                        if ((genericController.vbLCase(adminContent.ContentTableName) == "ccpagecontent")) {
                            // 
                            PageNotFoundPageID = cpCore.siteProperties.getinteger("PageNotFoundPageID", 0);
                            if (cpCore.docProperties.getBoolean("PageNotFound")) {
                                editRecord.SetPageNotFoundPageID = true;
                            }
                            else if ((editRecord.id == PageNotFoundPageID)) {
                                cpCore.siteProperties.setProperty("PageNotFoundPageID", "0");
                            }
                            
                            // 
                            if (cpCore.docProperties.getBoolean("LandingPageID")) {
                                editRecord.SetLandingPageID = true;
                            }
                            else if ((editRecord.id == 0)) {
                                // 
                                //  New record, allow it to be set, but do not compare it to LandingPageID
                                // 
                            }
                            else if ((editRecord.id == cpCore.siteProperties.landingPageID)) {
                                // 
                                //  Do not reset the LandingPageID from here -- set another instead
                                // 
                                errorController.error_AddUserError(cpCore, @"This page was marked As the Landing Page For the website, And the checkbox has been cleared. This Is Not allowed. To remove this page As the Landing Page, locate a New landing page And Select it, Or go To Settings > Page Settings And Select a New Landing Page.");
                            }
                            
                        }
                        
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
        }
        
        // '
        // '========================================================================
        // '   Read in a Response value by name
        // '========================================================================
        // '
        // Private Sub LoadEditResponseByName(FieldName As String)
        //     On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.LoadEditResponseByName")
        //     '
        //     ' converted array to dictionary - Dim FieldPointer As Integer
        //     Dim FieldFound As Boolean
        //     Dim UcaseFieldName As String
        //     Dim DataSourceName As String
        //     Dim FormID As String
        //     '
        //     FieldFound = False
        //     DataSourceName = cpCore.db.getDataSourceNameByID(AdminContent.DataSourceID)
        //     If (FieldName <> "") Then
        //         UcaseFieldName = genericController.vbUCase(FieldName)
        //         If AdminContent.fields.count > 0 Then
        //             For FieldPointer = 0 To AdminContent.fields.count - 1
        //                 If genericController.vbUCase(AdminContent.fields(FieldPointer).Name) = UcaseFieldName Then
        //                     Call LoadEditResponseByPointer(FormID, FieldPointer, DataSourceName)
        //                     FieldFound = True
        //                     Exit For
        //                     End If
        //                 Next
        //             End If
        //         End If
        //     If Not FieldFound Then
        //         Call HandleInternalError("AdminClass.LoadEditResponseByName", "Field [" & FieldName & "] was Not found In content [" & AdminContent.Name & "]")
        //         End If
        //     '
        //     '''Dim th as integer: Exit Sub
        //     '
        //     ' ----- Error Trap
        //     '
        // ErrorTrap:
        //     Call HandleClassTrapErrorBubble("LoadEditResponseByName")
        //     '
        // End Sub
        // 
        // ========================================================================
        //    Read the Form into the fields array
        // ========================================================================
        // 
        private void LoadEditRecord_RequestField(Models.Complex.cdefModel adminContent, editRecordClass editRecord, Models.Complex.CDefFieldModel field, string ignore, ref string FormFieldListToBeLoaded, string FormEmptyFieldList) {
            try {
                const object LoopPtrMax = 100;
                bool blockDuplicateUsername;
                bool blockDuplicateEmail;
                string lcaseCopy;
                bool HasImg;
                bool HasInput;
                bool HasAC;
                int EditorPixelHeight;
                int EditorRowHeight;
                htmlToTextControllers HTMLDecode;
                string Copy;
                string FieldName;
                bool ResponseFieldValueIsOKToSave;
                int CSPointer;
                bool ResponseFieldIsEmpty;
                string ResponseFieldValueText;
                htmlParserController HTML = new htmlParserController(cpCore);
                string TabCopy = "";
                int ParentID;
                string UsedIDs;
                int LoopPtr;
                int CS;
                bool InLoadedFieldList;
                bool InEmptyFieldList;
                bool InResponse;
                string responseName;
                // 
                //    Read in form values
                // 
                // With...
                if (!field.active) {
                    // 
                    //  Exclude from all field testing, do not load a resposne for this field
                    // 
                }
                else {
                    // 
                    //  Read value in and test it for valid response
                    //  Assume OK, mark not ok if there is a problem
                    // 
                    ResponseFieldValueIsOKToSave = true;
                    FieldName = genericController.vbUCase(field.nameLc);
                    responseName = FieldName;
                    InLoadedFieldList = ((FormFieldListToBeLoaded.IndexOf(("," 
                                    + (FieldName + ",")), 0, System.StringComparison.OrdinalIgnoreCase) + 1) 
                                != 0);
                    InEmptyFieldList = ((FormEmptyFieldList.IndexOf(("," 
                                    + (responseName + ",")), 0, System.StringComparison.OrdinalIgnoreCase) + 1) 
                                != 0);
                    InResponse = cpCore.docProperties.containsKey(responseName);
                    FormFieldListToBeLoaded = genericController.vbReplace(FormFieldListToBeLoaded, ("," 
                                    + (FieldName + ",")), ",", 1, 99, vbTextCompare);
                    ResponseFieldValueText = cpCore.docProperties.getText(responseName);
                    ResponseFieldIsEmpty = string.IsNullOrEmpty(ResponseFieldValueText);
                    if ((field.editTabName != "")) {
                        TabCopy = (" In the " 
                                    + (field.editTabName + " tab"));
                    }
                    
                    // 
                    if ((genericController.vbInstr(1, FieldName, "PARENTID", vbTextCompare) != 0)) {
                        FieldName = FieldName;
                    }
                    
                    // 
                    //  process reserved fields
                    // 
                    switch (FieldName) {
                        case "CONTENTCONTROLID":
                            if (AllowAdminFieldCheck()) {
                                if (!cpCore.docProperties.containsKey(FieldName)) {
                                    if (!(cpCore.doc.debug_iUserError != "")) {
                                        // 
                                        //  Add user error only for the first missing field
                                        // 
                                        errorController.error_AddUserError(cpCore, (@"There has been an Error reading the response from your browser. Please Try again, taking care Not To submit the page until your browser has finished loading. If this Error occurs again, please report this problem To your site administrator. The first Error was [" 
                                                        + (FieldName + " Not found]. There may have been others.")));
                                    }
                                    
                                    throw new ApplicationException("Unexpected exception");
                                    return;
                                }
                                
                            }
                            
                            // 
                            ResponseFieldValueText = cpCore.docProperties.getText(FieldName);
                            // ResponseValueVariant = cpCore.main_ReadStreamText(FieldName)
                            // ResponseValueText = genericController.encodeText(ResponseValueVariant)
                            if ((genericController.EncodeInteger(ResponseFieldValueText) == genericController.EncodeInteger(editRecord.fieldsLc(field.nameLc).value))) {
                                // 
                                //  No change
                                // 
                            }
                            else {
                                // 
                                //  new value
                                // 
                                editRecord.fieldsLc(field.nameLc).value = ResponseFieldValueText;
                                ResponseFieldIsEmpty = false;
                            }
                            
                            break;
                        case "ACTIVE":
                            InEmptyFieldList = ((FormEmptyFieldList.IndexOf(("," 
                                            + (FieldName + ",")), 0, System.StringComparison.OrdinalIgnoreCase) + 1) 
                                        != 0);
                            InResponse = cpCore.docProperties.containsKey(FieldName);
                            if (AllowAdminFieldCheck()) {
                                if ((!InResponse 
                                            && !InEmptyFieldList)) {
                                    errorController.error_AddUserError(cpCore, ("There has been an Error reading the response from your browser. Please Try your change again. If this" +
                                        " Error occurs again, please report this problem To your site administrator. The Error Is [" 
                                                    + (FieldName + " Not found].")));
                                    throw new ApplicationException("Unexpected exception");
                                    return;
                                }
                                
                            }
                            
                            // 
                            bool responseValue = cpCore.docProperties.getBoolean(FieldName);
                            if (!responseValue.Equals(EncodeBoolean(editRecord.fieldsLc(field.nameLc).value))) {
                                // 
                                //  new value
                                // 
                                editRecord.fieldsLc(field.nameLc).value = responseValue;
                                ResponseFieldIsEmpty = false;
                            }
                            
                            break;
                        case "CCGUID":
                            InEmptyFieldList = ((FormEmptyFieldList.IndexOf(("," 
                                            + (FieldName + ",")), 0, System.StringComparison.OrdinalIgnoreCase) + 1) 
                                        != 0);
                            InResponse = cpCore.docProperties.containsKey(FieldName);
                            if (AllowAdminFieldCheck()) {
                                if ((!InResponse 
                                            && !InEmptyFieldList)) {
                                    errorController.error_AddUserError(cpCore, ("There has been an Error reading the response from your browser. Please Try your change again. If this" +
                                        " Error occurs again, please report this problem To your site administrator. The Error Is [" 
                                                    + (FieldName + " Not found].")));
                                    throw new ApplicationException("Unexpected exception");
                                    return;
                                }
                                
                            }
                            
                            // 
                            ResponseFieldValueText = cpCore.docProperties.getText(FieldName);
                            if ((ResponseFieldValueText == editRecord.fieldsLc(field.nameLc).value.ToString)) {
                                // 
                                //  No change
                                // 
                            }
                            else {
                                // 
                                //  new value
                                // 
                                editRecord.fieldsLc(field.nameLc).value = ResponseFieldValueText;
                                ResponseFieldIsEmpty = false;
                            }
                            
                            break;
                        case "ID":
                        case "MODIFIEDBY":
                        case "MODIFIEDDATE":
                        case "CREATEDBY":
                        case "DATEADDED":
                            ResponseFieldValueIsOKToSave = false;
                            break;
                        default:
                            if (!field.authorable) {
                                // 
                                //  Is blocked from authoring, leave current value
                                // 
                                ResponseFieldValueIsOKToSave = false;
                            }
                            else if (((field.fieldTypeId == FieldTypeIdAutoIdIncrement) 
                                        || ((field.fieldTypeId == FieldTypeIdRedirect) 
                                        || (field.fieldTypeId == FieldTypeIdManyToMany)))) {
                                // 
                                //  These fields types have no values to load, leave current value
                                //  (many to many is handled during save)
                                // 
                                ResponseFieldValueIsOKToSave = false;
                            }
                            else if ((field.adminOnly 
                                        && !cpCore.doc.authContext.isAuthenticatedAdmin(cpCore))) {
                                // 
                                //  non-admin and admin only field, leave current value
                                // 
                                ResponseFieldValueIsOKToSave = false;
                            }
                            else if ((field.developerOnly 
                                        && !cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore))) {
                                // 
                                //  non-developer and developer only field, leave current value
                                // 
                                ResponseFieldValueIsOKToSave = false;
                            }
                            else if ((field.ReadOnly 
                                        || (field.NotEditable 
                                        && (editRecord.id != 0)))) {
                                // 
                                //  read only field, leave current
                                // 
                                ResponseFieldValueIsOKToSave = false;
                            }
                            else if (!InLoadedFieldList) {
                                // 
                                //  Was not sent out, so just go with the current value
                                //  Also, if the loaded field list is not returned, and the field is not returned, this is the bestwe can do.
                                // 
                                ResponseFieldValueIsOKToSave = false;
                            }
                            else if ((AllowAdminFieldCheck() 
                                        && (!InResponse 
                                        && !InEmptyFieldList))) {
                                // 
                                //  Was sent out non-blank, and no response back, flag error and leave the current value to a retry
                                // 
                                errorController.error_AddUserError(cpCore, ("There has been an Error reading the response from your browser. The field [" 
                                                + (field.caption + ("]" 
                                                + (TabCopy + " was missing. Please Try your change again. If this Error happens repeatedly, please report this prob" +
                                                "lem To your site administrator.")))));
                                throw new ApplicationException("Unexpected exception");
                                ResponseFieldValueIsOKToSave = false;
                            }
                            else {
                                // 
                                //  Test input value for valid data
                                // 
                                switch (field.fieldTypeId) {
                                    case FieldTypeIdInteger:
                                        // 
                                        //  ----- Integer
                                        // 
                                        ResponseFieldIsEmpty = (ResponseFieldIsEmpty 
                                                    | (ResponseFieldValueText == ""));
                                        if (!ResponseFieldIsEmpty) {
                                            if (genericController.vbIsNumeric(ResponseFieldValueText)) {
                                                // ResponseValueVariant = genericController.EncodeInteger(ResponseValueVariant)
                                            }
                                            else {
                                                errorController.error_AddUserError(cpCore, ("The record cannot be saved because the field [" 
                                                                + (field.caption + ("]" 
                                                                + (TabCopy + " must be a numeric value.")))));
                                                ResponseFieldValueIsOKToSave = false;
                                            }
                                            
                                        }
                                        
                                        break;
                                    case FieldTypeIdCurrency:
                                    case FieldTypeIdFloat:
                                        // 
                                        //  ----- Floating point number
                                        // 
                                        ResponseFieldIsEmpty = (ResponseFieldIsEmpty 
                                                    | (ResponseFieldValueText == ""));
                                        if (!ResponseFieldIsEmpty) {
                                            if (genericController.vbIsNumeric(ResponseFieldValueText)) {
                                                // ResponseValueVariant = EncodeNumber(ResponseValueVariant)
                                            }
                                            else {
                                                errorController.error_AddUserError(cpCore, ("This record cannot be saved because the field [" 
                                                                + (field.caption + ("]" 
                                                                + (TabCopy + " must be a numeric value.")))));
                                                ResponseFieldValueIsOKToSave = false;
                                            }
                                            
                                        }
                                        
                                        break;
                                    case FieldTypeIdLookup:
                                        // 
                                        //  ----- Must be a recordID
                                        // 
                                        ResponseFieldIsEmpty = (ResponseFieldIsEmpty 
                                                    | (ResponseFieldValueText == ""));
                                        if (!ResponseFieldIsEmpty) {
                                            if (genericController.vbIsNumeric(ResponseFieldValueText)) {
                                                // ResponseValueVariant = genericController.EncodeInteger(ResponseValueVariant)
                                            }
                                            else {
                                                errorController.error_AddUserError(cpCore, ("This record cannot be saved because the field [" 
                                                                + (field.caption + ("]" 
                                                                + (TabCopy + " had an invalid selection.")))));
                                                ResponseFieldValueIsOKToSave = false;
                                            }
                                            
                                        }
                                        
                                        break;
                                    case FieldTypeIdDate:
                                        // 
                                        //  ----- Must be a Date value
                                        // 
                                        ResponseFieldIsEmpty = (ResponseFieldIsEmpty 
                                                    | (ResponseFieldValueText == ""));
                                        if (!ResponseFieldIsEmpty) {
                                            if (!IsDate(ResponseFieldValueText)) {
                                                errorController.error_AddUserError(cpCore, ("This record cannot be saved because the field [" 
                                                                + (field.caption + ("]" 
                                                                + (TabCopy + " must be a date And/Or time in the form mm/dd/yy 0000 AM(PM).")))));
                                                ResponseFieldValueIsOKToSave = false;
                                            }
                                            
                                        }
                                        
                                        // End Case
                                        break;
                                    case FieldTypeIdBoolean:
                                        // 
                                        //  ----- translate to boolean
                                        // 
                                        ResponseFieldValueText = genericController.EncodeBoolean(ResponseFieldValueText).ToString;
                                        break;
                                    case FieldTypeIdLink:
                                        // 
                                        //  ----- Link field - if it starts with 'www.', add the http:// automatically
                                        // 
                                        ResponseFieldValueText = genericController.encodeText(ResponseFieldValueText);
                                        if ((ResponseFieldValueText.ToLower().Substring(0, 4) == "www.")) {
                                            ResponseFieldValueText = ("http//" + ResponseFieldValueText);
                                        }
                                        
                                        break;
                                    case FieldTypeIdHTML:
                                    case FieldTypeIdFileHTML:
                                        // 
                                        //  ----- Html fields
                                        // 
                                        EditorRowHeight = cpCore.docProperties.getInteger((FieldName + "Rows"));
                                        if ((EditorRowHeight != 0)) {
                                            cpCore.userProperty.setProperty((adminContent.Name + ("." 
                                                            + (FieldName + ".RowHeight"))), EditorRowHeight);
                                        }
                                        
                                        EditorPixelHeight = cpCore.docProperties.getInteger((FieldName + "PixelHeight"));
                                        if ((EditorPixelHeight != 0)) {
                                            cpCore.userProperty.setProperty((adminContent.Name + ("." 
                                                            + (FieldName + ".PixelHeight"))), EditorPixelHeight);
                                        }
                                        
                                        // 
                                        if (!field.htmlContent) {
                                            lcaseCopy = genericController.vbLCase(ResponseFieldValueText);
                                            lcaseCopy = genericController.vbReplace(lcaseCopy, "\r", "");
                                            lcaseCopy = genericController.vbReplace(lcaseCopy, "\n", "");
                                            lcaseCopy = lcaseCopy.Trim();
                                            if (((lcaseCopy == HTMLEditorDefaultCopyNoCr) 
                                                        || (lcaseCopy == HTMLEditorDefaultCopyNoCr2))) {
                                                // 
                                                //  if the editor was left blank, remote the default copy
                                                // 
                                                ResponseFieldValueText = "";
                                            }
                                            else {
                                                if ((genericController.vbInstr(1, ResponseFieldValueText, HTMLEditorDefaultCopyStartMark) != 0)) {
                                                    // 
                                                    //  if the default copy was editing, remote the markers
                                                    // 
                                                    ResponseFieldValueText = genericController.vbReplace(ResponseFieldValueText, HTMLEditorDefaultCopyStartMark, "");
                                                    ResponseFieldValueText = genericController.vbReplace(ResponseFieldValueText, HTMLEditorDefaultCopyEndMark, "");
                                                    // ResponseValueVariant = ResponseValueText
                                                }
                                                
                                                // 
                                                //  If the response is only white space, remove it
                                                //  this is a fix for when Site Managers leave white space in the editor, and do not realize it
                                                //    then cannot fixgure out how to remove it
                                                // 
                                                ResponseFieldValueText = cpCore.html.convertEditorResponseToActiveContent(ResponseFieldValueText);
                                                if (string.IsNullOrEmpty(ResponseFieldValueText.ToLower().Replace(" ", c, "").Replace(" ", ""))) {
                                                    ResponseFieldValueText = String.Empty;
                                                }
                                                
                                            }
                                            
                                        }
                                        
                                        break;
                                    default:
                                        EditorRowHeight = cpCore.docProperties.getInteger((FieldName + "Rows"));
                                        if ((EditorRowHeight != 0)) {
                                            cpCore.userProperty.setProperty((adminContent.Name + ("." 
                                                            + (FieldName + ".RowHeight"))), EditorRowHeight);
                                        }
                                        
                                        EditorPixelHeight = cpCore.docProperties.getInteger((FieldName + "PixelHeight"));
                                        if ((EditorPixelHeight != 0)) {
                                            cpCore.userProperty.setProperty((adminContent.Name + ("." 
                                                            + (FieldName + ".PixelHeight"))), EditorPixelHeight);
                                        }
                                        
                                        break;
                                }
                                if ((FieldName.ToLower() == "parentid")) {
                                    // 
                                    //  check circular reference on all parentid fields
                                    // 
                                    ParentID = genericController.EncodeInteger(ResponseFieldValueText);
                                    LoopPtr = 0;
                                    UsedIDs = editRecord.id.ToString;
                                    while (((LoopPtr < LoopPtrMax) 
                                                && ((ParentID != 0) 
                                                && ((("," 
                                                + (UsedIDs + ",")).IndexOf(("," 
                                                    + (ParentID.ToString() + ",")), 0, System.StringComparison.Ordinal) + 1) 
                                                == 0)))) {
                                        UsedIDs = (UsedIDs + ("," + ParentID.ToString()));
                                        CS = cpCore.db.csOpen(adminContent.Name, ("ID=" + ParentID), ,, ,, ,, "ParentID");
                                        if (!cpCore.db.csOk(CS)) {
                                            ParentID = 0;
                                        }
                                        else {
                                            ParentID = cpCore.db.csGetInteger(CS, "ParentID");
                                        }
                                        
                                        cpCore.db.csClose(CS);
                                        LoopPtr = (LoopPtr + 1);
                                    }
                                    
                                    if ((LoopPtr == LoopPtrMax)) {
                                        // 
                                        //  Too deep
                                        // 
                                        errorController.error_AddUserError(cpCore, ("This record cannot be saved because the field [" 
                                                        + (field.caption + ("]" 
                                                        + (TabCopy + (" creates a relationship between records that Is too large. Please limit the depth of this relationshi" +
                                                        "p to " 
                                                        + (LoopPtrMax + " records.")))))));
                                        ResponseFieldValueIsOKToSave = false;
                                    }
                                    else if (((editRecord.id != 0) 
                                                && (editRecord.id == ParentID))) {
                                        // 
                                        //  Reference to iteslf
                                        // 
                                        errorController.error_AddUserError(cpCore, ("This record cannot be saved because the field [" 
                                                        + (field.caption + ("]" 
                                                        + (TabCopy + " contains a circular reference. This record points back to itself. This Is Not allowed.")))));
                                        ResponseFieldValueIsOKToSave = false;
                                    }
                                    else if ((ParentID != 0)) {
                                        // 
                                        //  Circular reference
                                        // 
                                        errorController.error_AddUserError(cpCore, ("This record cannot be saved because the field [" 
                                                        + (field.caption + ("]" 
                                                        + (TabCopy + " contains a circular reference. This field either points to other records which then point back to th" +
                                                        "is record. This Is Not allowed.")))));
                                        ResponseFieldValueIsOKToSave = false;
                                    }
                                    
                                }
                                
                                if (field.TextBuffered) {
                                    // 
                                    //  text buffering
                                    // 
                                    ResponseFieldValueText = genericController.main_RemoveControlCharacters(ResponseFieldValueText);
                                }
                                
                                if ((field.Required && ResponseFieldIsEmpty)) {
                                    // 
                                    //  field is required and is not given
                                    // 
                                    errorController.error_AddUserError(cpCore, ("This record cannot be saved because the field [" 
                                                    + (field.caption + ("]" 
                                                    + (TabCopy + " Is required but has no value.")))));
                                    ResponseFieldValueIsOKToSave = false;
                                }
                                
                                // 
                                //  special case - people records without Allowduplicateusername require username to be unique
                                // 
                                if ((genericController.vbLCase(adminContent.ContentTableName) == "ccmembers")) {
                                    if ((genericController.vbLCase(field.nameLc) == "username")) {
                                        blockDuplicateUsername = !cpCore.siteProperties.getBoolean("allowduplicateusername", false);
                                    }
                                    
                                    if ((genericController.vbLCase(field.nameLc) == "email")) {
                                        blockDuplicateEmail = cpCore.siteProperties.getBoolean("allowemaillogin", false);
                                    }
                                    
                                }
                                
                                if (((blockDuplicateUsername 
                                            || (blockDuplicateEmail || field.UniqueName)) 
                                            && !ResponseFieldIsEmpty)) {
                                    // 
                                    //  ----- Do the unique check for this field
                                    // 
                                    string SQLUnique = ("select id from " 
                                                + (adminContent.ContentTableName + (" where (" 
                                                + (FieldName + ("=" 
                                                + (cpCore.db.EncodeSQL(ResponseFieldValueText, field.fieldTypeId) + (")and(" 
                                                + (Models.Complex.cdefModel.getContentControlCriteria(cpCore, adminContent.Name) + ")"))))))));
                                    if ((editRecord.id > 0)) {
                                        // 
                                        //  --editing record
                                        SQLUnique = (SQLUnique + ("and(id<>" 
                                                    + (editRecord.id + ")")));
                                    }
                                    
                                    CSPointer = cpCore.db.csOpenSql_rev(adminContent.ContentDataSourceName, SQLUnique);
                                    if (cpCore.db.csOk(CSPointer)) {
                                        // 
                                        //  field is not unique, skip it and flag error
                                        // 
                                        if (blockDuplicateUsername) {
                                            // 
                                            // 
                                            // 
                                            errorController.error_AddUserError(cpCore, ("This record cannot be saved because the field [" 
                                                            + (field.caption + ("]" 
                                                            + (TabCopy + (" must be unique And there Is another record with [" 
                                                            + (ResponseFieldValueText + "]. This must be unique because the preference Allow Duplicate Usernames Is Not checked.")))))));
                                        }
                                        else if (blockDuplicateEmail) {
                                            // 
                                            // 
                                            // 
                                            errorController.error_AddUserError(cpCore, ("This record cannot be saved because the field [" 
                                                            + (field.caption + ("]" 
                                                            + (TabCopy + (" must be unique And there Is another record with [" 
                                                            + (ResponseFieldValueText + "]. This must be unique because the preference Allow Email Login Is checked.")))))));
                                        }
                                        else if (false) {
                                            
                                        }
                                        else {
                                            // 
                                            //  non-workflow
                                            // 
                                            errorController.error_AddUserError(cpCore, ("This record cannot be saved because the field [" 
                                                            + (field.caption + ("]" 
                                                            + (TabCopy + (" must be unique And there Is another record with [" 
                                                            + (ResponseFieldValueText + "].")))))));
                                        }
                                        
                                        ResponseFieldValueIsOKToSave = false;
                                    }
                                    
                                    cpCore.db.csClose(CSPointer);
                                }
                                
                            }
                            
                            //  end case
                            break;
                    }
                    // 
                    //  Save response if it is valid
                    // 
                    if (ResponseFieldValueIsOKToSave) {
                        editRecord.fieldsLc(field.nameLc).value = ResponseFieldValueText;
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
        }
    