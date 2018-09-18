

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
        //    Save Whats New values if present
        // 
        //    does NOT check AuthoringLocked -- you must check before calling
        // ========================================================================
        // 
        private void SaveContentTracking(Models.Complex.cdefModel adminContent, editRecordClass editRecord) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter( "SaveContentTracking")
            // 
            int ContentID;
            int CSPointer;
            int CSRules;
            int CSContentWatch;
            int CSContentWatchList;
            int ContentWatchID;
            //  converted array to dictionary - Dim FieldPointer As Integer
            string MethodName;
            // 
            MethodName = "Admin.SaveContentTracking()";
            if ((adminContent.AllowContentTracking 
                        && !editRecord.Read_Only)) {
                // 
                //  ----- Set default content watch link label
                // 
                if (((ContentWatchListIDCount > 0) 
                            && (ContentWatchLinkLabel == ""))) {
                    if ((editRecord.menuHeadline != "")) {
                        ContentWatchLinkLabel = editRecord.menuHeadline;
                    }
                    else if ((editRecord.nameLc != "")) {
                        ContentWatchLinkLabel = editRecord.nameLc;
                    }
                    else {
                        ContentWatchLinkLabel = "Click Here";
                    }
                    
                }
                
                //  ----- update/create the content watch record for this content record
                // 
                ContentID = editRecord.contentControlId;
                CSContentWatch = cpCore.db.csOpen("Content Watch", ("(ContentID=" 
                                + (cpCore.db.encodeSQLNumber(ContentID) + (")And(RecordID=" 
                                + (cpCore.db.encodeSQLNumber(editRecord.id) + ")")))));
                if (!cpCore.db.csOk(CSContentWatch)) {
                    cpCore.db.csClose(CSContentWatch);
                    CSContentWatch = cpCore.db.csInsertRecord("Content Watch");
                    cpCore.db.csSet(CSContentWatch, "contentid", ContentID);
                    cpCore.db.csSet(CSContentWatch, "recordid", editRecord.id);
                    cpCore.db.csSet(CSContentWatch, "ContentRecordKey", (ContentID + ("." + editRecord.id)));
                    cpCore.db.csSet(CSContentWatch, "clicks", 0);
                }
                
                if (!cpCore.db.csOk(CSContentWatch)) {
                    handleLegacyClassError(MethodName, "SaveContentTracking, can Not create New record");
                }
                else {
                    ContentWatchID = cpCore.db.csGetInteger(CSContentWatch, "ID");
                    cpCore.db.csSet(CSContentWatch, "LinkLabel", ContentWatchLinkLabel);
                    cpCore.db.csSet(CSContentWatch, "WhatsNewDateExpires", ContentWatchExpires);
                    cpCore.db.csSet(CSContentWatch, "Link", ContentWatchLink);
                    // 
                    //  ----- delete all rules for this ContentWatch record
                    // 
                    // Call cpCore.app.DeleteContentRecords("Content Watch List Rules", "(ContentWatchID=" & ContentWatchID & ")")
                    CSPointer = cpCore.db.csOpen("Content Watch List Rules", ("(ContentWatchID=" 
                                    + (ContentWatchID + ")")));
                    while (cpCore.db.csOk(CSPointer)) {
                        cpCore.db.csDeleteRecord(CSPointer);
                        cpCore.db.csGoNext(CSPointer);
                    }
                    
                    cpCore.db.csClose(CSPointer);
                    // 
                    //  ----- Update ContentWatchListRules for all entries in ContentWatchListID( ContentWatchListIDCount )
                    // 
                    int ListPointer;
                    if ((ContentWatchListIDCount > 0)) {
                        for (ListPointer = 0; (ListPointer 
                                    <= (ContentWatchListIDCount - 1)); ListPointer++) {
                            CSRules = cpCore.db.csInsertRecord("Content Watch List Rules");
                            if (cpCore.db.csOk(CSRules)) {
                                cpCore.db.csSet(CSRules, "ContentWatchID", ContentWatchID);
                                cpCore.db.csSet(CSRules, "ContentWatchListID", ContentWatchListID(ListPointer));
                            }
                            
                            cpCore.db.csClose(CSRules);
                        }
                        
                    }
                    
                }
                
                cpCore.db.csClose(CSContentWatch);
            }
            
            // 
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            handleLegacyClassError3("SaveContentTracking");
            // 
        }
        
        // 
        // ========================================================================
        //    Read in Whats New values if present
        // ========================================================================
        // 
        private void LoadContentTrackingResponse(Models.Complex.cdefModel adminContent, editRecordClass editRecord) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter( "LoadContentTrackingResponse")
            // 
            int CSContentWatchList;
            int RecordID;
            // 
            ContentWatchListIDCount = 0;
            if (((cpCore.docProperties.getText("WhatsNewResponse") != "") 
                        && adminContent.AllowContentTracking)) {
                // 
                //  ----- set single fields
                // 
                ContentWatchLinkLabel = cpCore.docProperties.getText("ContentWatchLinkLabel");
                ContentWatchExpires = cpCore.docProperties.getDate("ContentWatchExpires");
                // 
                //  ----- Update ContentWatchListRules for all checked boxes
                // 
                CSContentWatchList = cpCore.db.csOpen("Content Watch Lists");
                while (cpCore.db.csOk(CSContentWatchList)) {
                    RecordID = cpCore.db.csGetInteger(CSContentWatchList, "ID");
                    if (cpCore.docProperties.getBoolean(("ContentWatchList." + RecordID))) {
                        if ((ContentWatchListIDCount >= ContentWatchListIDSize)) {
                            ContentWatchListIDSize = (ContentWatchListIDSize + 50);
                            object Preserve;
                            ContentWatchListID(ContentWatchListIDSize);
                        }
                        
                        ContentWatchListID(ContentWatchListIDCount) = RecordID;
                        ContentWatchListIDCount = (ContentWatchListIDCount + 1);
                    }
                    
                    cpCore.db.csGoNext(CSContentWatchList);
                }
                
                cpCore.db.csClose(CSContentWatchList);
            }
            
            // 
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            handleLegacyClassError3("LoadContentTrackingResponse");
            // 
        }
        
        //         '
        //         '========================================================================
        //         '   Load and Save
        //         '       From both response and database
        //         '
        //         '   This needs to be updated:
        //         '       - Divide into LoadCalendarEvents / SaveCalendarEvents
        //         '       - Put LoadCalendarEvents in LoadResponse/LoadDatabase
        //         '       - Put SaveCalendarEvents with SaveEditRecord
        //         '       - this is so a usererror will preserve form responses
        //         '       - Dont delete all and recreate / just update records
        //         '========================================================================
        //         '
        //         Private Sub LoadAndSaveMetaContent()
        //             On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "LoadAndSaveMetaContent")
        //             '
        //             Dim CS As Integer
        //             Dim MetaContentID As Integer
        //             Dim MetaKeywordList As String
        //             '
        //             MetaContentID = cpCore.docProperties.getInteger("MetaContent.MetaContentID")
        //             If (MetaContentID <> 0) Then
        //                 '
        //                 ' ----- Load from Response
        //                 '
        //                 CS = cpCore.db.csOpenRecord("Meta Content", MetaContentID)
        //                 If cpCore.db.cs_ok(CS) Then
        //                     Call cpCore.db.cs_set(CS, "Name", cpCore.docProperties.getText("MetaContent.PageTitle"))
        //                     Call cpCore.db.cs_set(CS, "MetaDescription", cpCore.docProperties.getText("MetaContent.MetaDescription"))
        //                     If True Then ' 3.3.930" Then
        //                         Call cpCore.db.cs_set(CS, "OtherHeadTags", cpCore.docProperties.getText("MetaContent.OtherHeadTags"))
        //                         MetaKeywordList = cpCore.docProperties.getText("MetaContent.MetaKeywordList")
        //                         MetaKeywordList = genericController.vbReplace(MetaKeywordList, ",", vbCrLf)
        //                         Do While genericController.vbInstr(1, MetaKeywordList, vbCrLf & " ") <> 0
        //                             MetaKeywordList = genericController.vbReplace(MetaKeywordList, vbCrLf & " ", vbCrLf)
        //                         Loop
        //                         Do While genericController.vbInstr(1, MetaKeywordList, " " & vbCrLf) <> 0
        //                             MetaKeywordList = genericController.vbReplace(MetaKeywordList, " " & vbCrLf, vbCrLf)
        //                         Loop
        //                         Do While genericController.vbInstr(1, MetaKeywordList, vbCrLf & vbCrLf) <> 0
        //                             MetaKeywordList = genericController.vbReplace(MetaKeywordList, vbCrLf & vbCrLf, vbCrLf)
        //                         Loop
        //                         Do While (MetaKeywordList <> "") And (Right(MetaKeywordList, 2) = vbCrLf)
        //                             MetaKeywordList = Left(MetaKeywordList, Len(MetaKeywordList) - 2)
        //                         Loop
        //                         Call cpCore.db.cs_set(CS, "MetaKeywordList", MetaKeywordList)
        //                     ElseIf cpCore.db.cs_isFieldSupported(CS, "OtherHeadTags") Then
        //                         Call cpCore.db.cs_set(CS, "OtherHeadTags", cpCore.docProperties.getText("MetaContent.OtherHeadTags"))
        //                     End If
        //                     Call cpCore.html.main_ProcessCheckList("MetaContent.KeywordList", "Meta Content", genericController.encodeText(MetaContentID), "Meta Keywords", "Meta Keyword Rules", "MetaContentID", "MetaKeywordID")
        //                 End If
        //                 Call cpCore.db.cs_Close(CS)
        //                 '
        //                 ' Clear any bakes involving this content
        //                 '
        //                 Call cpCore.cache.invalidateContent("Meta Content")
        //                 Call cpCore.cache.invalidateContent("Meta Keyword Rules")
        //             End If
        //             '
        //             Exit Sub
        //             '
        //             ' ----- Error Trap
        //             '
        // ErrorTrap:
        //             Call handleLegacyClassError3("LoadAndSaveMetaContent")
        //             '
        //         End Sub
        // 
        // ========================================================================
        //    Save Link Alias field if it supported, and is non-authoring
        //    if it is authoring, it will be saved by the userfield routines
        //    if not, it appears in the LinkAlias tab, and must be saved here
        // ========================================================================
        // 
        private void SaveLinkAlias(Models.Complex.cdefModel adminContent, editRecordClass editRecord) {
            try {
                // 
                //  --use field ptr to test if the field is supported yet
                if (cpCore.siteProperties.allowLinkAlias) {
                    bool isDupError = false;
                    string linkAlias = cpCore.docProperties.getText("linkalias");
                    bool OverRideDuplicate = cpCore.docProperties.getBoolean("OverRideDuplicate");
                    bool DupCausesWarning = false;
                    if ((linkAlias == "")) {
                        // 
                        //  Link Alias is blank, use the record name
                        // 
                        linkAlias = editRecord.nameLc;
                        DupCausesWarning = true;
                    }
                    
                    if ((linkAlias != "")) {
                        if (OverRideDuplicate) {
                            cpCore.db.executeQuery(("update " 
                                            + (adminContent.ContentTableName + (" set linkalias=null where ( linkalias=" 
                                            + (cpCore.db.encodeSQLText(linkAlias) + (") and (id<>" 
                                            + (editRecord.id + ")")))))));
                        }
                        else {
                            int CS = cpCore.db.csOpen(adminContent.Name, ("( linkalias=" 
                                            + (cpCore.db.encodeSQLText(linkAlias) + (")and(id<>" 
                                            + (editRecord.id + ")")))));
                            if (cpCore.db.csOk(CS)) {
                                isDupError = true;
                                errorController.error_AddUserError(cpCore, ("The Link Alias you entered can not be used because another record uses this value [" 
                                                + (linkAlias + "]. Enter a different Link Alias, or check the Override Duplicates checkbox in the Link Alias tab.")));
                            }
                            
                            cpCore.db.csClose(CS);
                        }
                        
                        if (!isDupError) {
                            DupCausesWarning = true;
                            int CS = cpCore.db.cs_open2(adminContent.Name, editRecord.id, true, true);
                            if (cpCore.db.csOk(CS)) {
                                cpCore.db.csSet(CS, "linkalias", linkAlias);
                            }
                            
                            cpCore.db.csClose(CS);
                            // 
                            //  Update the Link Aliases
                            // 
                            docController.addLinkAlias(cpCore, linkAlias, editRecord.id, "", OverRideDuplicate, DupCausesWarning);
                        }
                        
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
        }
        
        // 
        // ========================================================================
        //    Read in Whats New values if present
        //    Field values must be loaded
        // ========================================================================
        // 
        private void LoadContentTrackingDataBase(Models.Complex.cdefModel adminContent, editRecordClass editRecord) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter( "LoadContentTrackingDataBase")
            // 
            int ContentID;
            int CSPointer;
            //  converted array to dictionary - Dim FieldPointer As Integer
            // 
            //  ----- check if admin record is present
            // 
            if (((editRecord.id != 0) 
                        && adminContent.AllowContentTracking)) {
                // 
                //  ----- Open the content watch record for this content record
                // 
                ContentID = editRecord.contentControlId;
                CSPointer = cpCore.db.csOpen("Content Watch", ("(ContentID=" 
                                + (cpCore.db.encodeSQLNumber(ContentID) + (")AND(RecordID=" 
                                + (cpCore.db.encodeSQLNumber(editRecord.id) + ")")))));
                if (cpCore.db.csOk(CSPointer)) {
                    ContentWatchLoaded = true;
                    ContentWatchRecordID = cpCore.db.csGetInteger(CSPointer, "ID");
                    ContentWatchLink = cpCore.db.csGet(CSPointer, "Link");
                    ContentWatchClicks = cpCore.db.csGetInteger(CSPointer, "Clicks");
                    ContentWatchLinkLabel = cpCore.db.csGet(CSPointer, "LinkLabel");
                    ContentWatchExpires = cpCore.db.csGetDate(CSPointer, "WhatsNewDateExpires");
                    cpCore.db.csClose(CSPointer);
                }
                
            }
            
            // 
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            handleLegacyClassError3("LoadContentTrackingDataBase");
            // 
        }
        
        // 
        // ========================================================================
        // 
        private void SaveEditRecord(Models.Complex.cdefModel adminContent, editRecordClass editRecord) {
            try {
                int SaveCCIDValue = 0;
                int ActivityLogOrganizationID = -1;
                if ((cpCore.doc.debug_iUserError != "")) {
                    // 
                    //  -- If There is an error, block the save
                    AdminAction = AdminActionNop;
                }
                else if (!cpCore.doc.authContext.isAuthenticatedContentManager(cpCore, adminContent.Name)) {
                    // 
                    //  -- must be content manager
                }
                else if (editRecord.Read_Only) {
                    // 
                    //  -- read only block
                }
                else {
                    // 
                    //  -- Record will be saved, create a new one if this is an add
                    bool NewRecord = false;
                    bool RecordChanged = false;
                    int CSEditRecord = -1;
                    if ((editRecord.id == 0)) {
                        NewRecord = true;
                        RecordChanged = true;
                        CSEditRecord = cpCore.db.csInsertRecord(adminContent.Name);
                    }
                    else {
                        NewRecord = false;
                        CSEditRecord = cpCore.db.cs_open2(adminContent.Name, editRecord.id, true, true);
                    }
                    
                    if (!cpCore.db.csOk(CSEditRecord)) {
                        // 
                        //  ----- Error: new record could not be created
                        // 
                        if (NewRecord) {
                            // 
                            //  Could not insert record
                            // 
                            cpCore.handleException(new ApplicationException(("A new record could not be inserted for content [" 
                                                + (adminContent.Name + "]. Verify the Database table and field DateAdded, CreateKey, and ID."))));
                        }
                        else {
                            // 
                            //  Could not locate record you requested
                            // 
                            cpCore.handleException(new ApplicationException(("The record you requested (ID=" 
                                                + (editRecord.id + (") could not be found for content [" 
                                                + (adminContent.Name + "]"))))));
                        }
                        
                    }
                    else {
                        // 
                        //  ----- Get the ID of the current record
                        // 
                        editRecord.id = cpCore.db.csGetInteger(CSEditRecord, "ID");
                        // 
                        //  ----- Create the update sql
                        // 
                        bool FieldChanged = false;
                        foreach (keyValuePair in adminContent.fields) {
                            Models.Complex.CDefFieldModel field = keyValuePair.Value;
                            // With...
                            editRecordFieldClass editRecordField = editRecord.fieldsLc(field.nameLc);
                            object fieldValueObject = editRecordField.value;
                            string FieldValueText = genericController.encodeText(fieldValueObject);
                            string FieldName = field.nameLc;
                            string UcaseFieldName = genericController.vbUCase(FieldName);
                            // 
                            //  ----- Handle special case fields
                            // 
                            switch (UcaseFieldName) {
                                case "NAME":
                                    editRecord.nameLc = genericController.encodeText(fieldValueObject);
                                    break;
                                case "CCGUID":
                                    string saveValue = genericController.encodeText(fieldValueObject);
                                    if ((cpCore.db.csGetText(CSEditRecord, FieldName) != saveValue)) {
                                        FieldChanged = true;
                                        RecordChanged = true;
                                        cpCore.db.csSet(CSEditRecord, FieldName, saveValue);
                                    }
                                    
                                    // RecordChanged = True
                                    // Call cpCore.app.SetCS(CSEditRecord, FieldName, FieldValueVariant)
                                    break;
                                case "CONTENTCONTROLID":
                                    int saveValue = genericController.EncodeInteger(fieldValueObject);
                                    if ((editRecord.contentControlId != saveValue)) {
                                        SaveCCIDValue = saveValue;
                                        RecordChanged = true;
                                    }
                                    
                                    break;
                                case "ACTIVE":
                                    bool saveValue = genericController.EncodeBoolean(fieldValueObject);
                                    if ((cpCore.db.csGetBoolean(CSEditRecord, FieldName) != saveValue)) {
                                        FieldChanged = true;
                                        RecordChanged = true;
                                        cpCore.db.csSet(CSEditRecord, FieldName, saveValue);
                                    }
                                    
                                    break;
                                case "DATEEXPIRES":
                                    if (!genericController.IsNull(fieldValueObject)) {
                                        if (IsDate(fieldValueObject)) {
                                            DateTime saveValue = genericController.EncodeDate(fieldValueObject);
                                            field.MinValue;
                                            ContentWatchExpires = saveValue;
                                        }
                                        else if ((ContentWatchExpires > saveValue)) {
                                            ContentWatchExpires = saveValue;
                                        }
                                        
                                    }
                                    
                                    break;
                            }
                            // 
                            "DATEARCHIVE";
                            if (!genericController.IsNull(fieldValueObject)) {
                                if (IsDate(fieldValueObject)) {
                                    DateTime saveValue = genericController.EncodeDate(fieldValueObject);
                                    MinValue;
                                    ContentWatchExpires = saveValue;
                                }
                                else if ((ContentWatchExpires > saveValue)) {
                                    ContentWatchExpires = saveValue;
                                }
                                
                            }
                            
                            NotEditable;
                            // 
                            //  ----- save the value by field type
                            // 
                            fieldTypeId;
                            FieldTypeIdAutoIdIncrement;
                            FieldTypeIdRedirect;
                            // 
                            //  do nothing with these
                            // 
                            FieldTypeIdFile;
                            FieldTypeIdFileImage;
                            // 
                            //  filenames, upload to cdnFiles
                            // 
                            if (cpCore.docProperties.getBoolean((FieldName + ".DeleteFlag"))) {
                                RecordChanged = true;
                                FieldChanged = true;
                                cpCore.db.csSet(CSEditRecord, FieldName, "");
                            }
                            
                            FieldValueText = genericController.encodeText(fieldValueObject);
                            if ((FieldValueText != "")) {
                                string Filename = encodeFilename(FieldValueText);
                                string unixPathFilename = cpCore.db.csGetFilename(CSEditRecord, FieldName, Filename, adminContent.Name);
                                string dosPathFilename = genericController.convertToDosSlash(unixPathFilename);
                                string dosPath = genericController.getPath(dosPathFilename);
                                cpCore.cdnFiles.upload(FieldName, dosPath, Filename);
                                cpCore.db.csSet(CSEditRecord, FieldName, unixPathFilename);
                                RecordChanged = true;
                                FieldChanged = true;
                            }
                            
                            FieldTypeIdBoolean;
                            // 
                            //  boolean
                            // 
                            bool saveValue = genericController.EncodeBoolean(fieldValueObject);
                            if ((cpCore.db.csGetBoolean(CSEditRecord, FieldName) != saveValue)) {
                                RecordChanged = true;
                                FieldChanged = true;
                                cpCore.db.csSet(CSEditRecord, FieldName, saveValue);
                            }
                            
                            FieldTypeIdCurrency;
                            FieldTypeIdFloat;
                            // 
                            //  Floating pointer numbers
                            // 
                            double saveValue = genericController.EncodeNumber(fieldValueObject);
                            if ((cpCore.db.csGetNumber(CSEditRecord, FieldName) != saveValue)) {
                                RecordChanged = true;
                                FieldChanged = true;
                                cpCore.db.csSet(CSEditRecord, FieldName, saveValue);
                            }
                            
                            FieldTypeIdDate;
                            // 
                            //  Date
                            // 
                            DateTime saveValue = genericController.EncodeDate(fieldValueObject);
                            if ((cpCore.db.csGetDate(CSEditRecord, FieldName) != saveValue)) {
                                FieldChanged = true;
                                RecordChanged = true;
                                cpCore.db.csSet(CSEditRecord, FieldName, saveValue);
                            }
                            
                            FieldTypeIdInteger;
                            FieldTypeIdLookup;
                            // 
                            //  Integers
                            // 
                            int saveValue = genericController.EncodeInteger(fieldValueObject);
                            if ((saveValue != cpCore.db.csGetInteger(CSEditRecord, FieldName))) {
                                FieldChanged = true;
                                RecordChanged = true;
                                cpCore.db.csSet(CSEditRecord, FieldName, saveValue);
                            }
                            
                            FieldTypeIdLongText;
                            FieldTypeIdText;
                            FieldTypeIdFileText;
                            FieldTypeIdFileCSS;
                            FieldTypeIdFileXML;
                            FieldTypeIdFileJavascript;
                            FieldTypeIdHTML;
                            FieldTypeIdFileHTML;
                            // 
                            //  Text
                            // 
                            string saveValue = genericController.encodeText(fieldValueObject);
                            if ((cpCore.db.csGet(CSEditRecord, FieldName) != saveValue)) {
                                FieldChanged = true;
                                RecordChanged = true;
                                cpCore.db.csSet(CSEditRecord, FieldName, saveValue);
                            }
                            
                            FieldTypeIdManyToMany;
                            // 
                            //  Many to Many checklist
                            // 
                            // MTMContent0 = models.complex.cdefmodel.getContentNameByID(cpcore,.contentId)
                            // MTMContent1 = models.complex.cdefmodel.getContentNameByID(cpcore,.manyToManyContentID)
                            // MTMRuleContent = models.complex.cdefmodel.getContentNameByID(cpcore,.manyToManyRuleContentID)
                            // MTMRuleField0 = .ManyToManyRulePrimaryField
                            // MTMRuleField1 = .ManyToManyRuleSecondaryField
                            cpCore.html.main_ProcessCheckList(ManyToMany&, ., id, Models.Complex.cdefModel.getContentNameByID(cpCore, ., contentId), editRecord.id.ToString(), Models.Complex.cdefModel.getContentNameByID(cpCore, ., manyToManyContentID), Models.Complex.cdefModel.getContentNameByID(cpCore, ., manyToManyRuleContentID), ., ManyToManyRulePrimaryField, ., ManyToManyRuleSecondaryField);
                            // 
                            //  Unknown other types
                            // 
                            string saveValue = genericController.encodeText(fieldValueObject);
                            FieldChanged = true;
                            RecordChanged = true;
                            cpCore.db.csSet(CSEditRecord, UcaseFieldName, saveValue);
                            // sql &=  "," & .Name & "=" & cpCore.app.EncodeSQL(FieldValueVariant, .FieldType)
                            if ((editRecordField.value == fieldValueObject)) {
                                // 
                                //  -- Log Activity for changes to people and organizattions
                                if (FieldChanged) {
                                    switch (genericController.vbLCase(adminContent.ContentTableName)) {
                                        case "ccmembers":
                                            if ((ActivityLogOrganizationID < 0)) {
                                                Models.Entity.personModel person = Models.Entity.personModel.create(cpCore, editRecord.id);
                                                if (person) {
                                                    IsNot;
                                                    null;
                                                    ActivityLogOrganizationID = person.OrganizationID;
                                                }
                                                
                                            }
                                            
                                            logController.logActivity2(cpCore, ("modifying field " + FieldName), editRecord.id, ActivityLogOrganizationID);
                                            break;
                                        case "organizations":
                                            logController.logActivity2(cpCore, ("modifying field " + FieldName), 0, editRecord.id);
                                            break;
                                        case "cclibraryfiles":
                                            if ((cpCore.docProperties.getText("filename") != "")) {
                                                cpCore.db.csSet(CSEditRecord, "altsizelist", "");
                                            }
                                            
                                            break;
                                    }
                                }
                                
                            }
                            
                        }
                        
                        // 
                        cpCore.db.csClose(CSEditRecord);
                        if (RecordChanged) {
                            // 
                            //  -- clear cache
                            string tableName = "";
                            if ((editRecord.contentControlId == 0)) {
                                tableName = Models.Complex.cdefModel.getContentTablename(cpCore, adminContent.Name);
                            }
                            else {
                                tableName = Models.Complex.cdefModel.getContentTablename(cpCore, editRecord.contentControlId_Name);
                            }
                            
                            switch (tableName.ToLower()) {
                                case linkAliasModel.contentTableName.ToLower():
                                    // 
                                    linkAliasModel.invalidateCache(cpCore, editRecord.id);
                                    // Models.Complex.routeDictionaryModel.invalidateCache(cpCore)
                                    break;
                                case addonModel.contentTableName.ToLower():
                                    // 
                                    addonModel.invalidateCache(cpCore, editRecord.id);
                                    // Models.Complex.routeDictionaryModel.invalidateCache(cpCore)
                                    break;
                                default:
                                    linkAliasModel.invalidateCache(cpCore, editRecord.id);
                                    break;
                            }
                        }
                        
                        // 
                        //  ----- Clear/Set PageNotFound
                        // 
                        if (editRecord.SetPageNotFoundPageID) {
                            cpCore.siteProperties.setProperty("PageNotFoundPageID", genericController.encodeText(editRecord.id));
                        }
                        
                        // 
                        //  ----- Clear/Set LandingPageID
                        // 
                        if (editRecord.SetLandingPageID) {
                            cpCore.siteProperties.setProperty("LandingPageID", genericController.encodeText(editRecord.id));
                        }
                        
                        // 
                        //  ----- clear/set authoring controls
                        // 
                        cpCore.workflow.ClearEditLock(adminContent.Name, editRecord.id);
                        // 
                        //  ----- if admin content is changed, reload the admincontent data in case this is a save, and not an OK
                        // 
                        if ((RecordChanged 
                                    && (SaveCCIDValue != 0))) {
                            Models.Complex.cdefModel.setContentControlId(cpCore, editRecord.contentControlId, editRecord.id, SaveCCIDValue);
                            editRecord.contentControlId_Name = Models.Complex.cdefModel.getContentNameByID(cpCore, SaveCCIDValue);
                            adminContent = Models.Complex.cdefModel.getCdef(cpCore, editRecord.contentControlId_Name);
                            adminContent.Id = adminContent.Id;
                            adminContent.Name = adminContent.Name;
                            //  false = cpCore.siteProperties.allowWorkflowAuthoring And adminContent.AllowWorkflowAuthoring
                        }
                        
                    }
                    
                    editRecord.Saved = true;
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
        }
        
        // 
        // ========================================================================
        //  Get Just the tablename from a sql statement
        //    This is to be compatible with the old way of setting up FieldTypeLookup
        // ========================================================================
        // 
        private string GetJustTableName(string SQL) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter( "GetJustTableName")
            // 
            GetJustTableName = SQL.ToUpper().Trim();
            while (((GetJustTableName != "") 
                        && ((GetJustTableName.IndexOf(" ") + 1) 
                        != 0))) {
                GetJustTableName = GetJustTableName.Substring(genericController.vbInstr(GetJustTableName, " "));
            }
            
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            handleLegacyClassError3("GetJustTableName");
            // 
        }
        
        // 
        // ========================================================================
        //  Test Content Access -- return based on Admin/Developer/MemberRules
        //    if developer, let all through
        //    if admin, block if table is developeronly
        //    if member, run blocking query (which also traps adminonly and developer only)
        //        if blockin query has a null RecordID, this member gets everything
        //        if not null recordid in blocking query, use RecordIDs in result for Where clause on this lookup
        // ========================================================================
        // 
        private bool userHasContentAccess(int ContentID) {
            bool returnHas = false;
            try {
                string ContentName;
                // 
                ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID);
                if ((ContentName != "")) {
                    returnHas = cpCore.doc.authContext.isAuthenticatedContentManager(cpCore, ContentName);
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnHas;
        }
        
        // 
        // ========================================================================
        //    Display a field in the admin index form
        // ========================================================================
        // 
        private string GetForm_Index_GetCell(Models.Complex.cdefModel adminContent, editRecordClass editRecord, string fieldName, int CS, bool IsLookupFieldValid, bool IsEmailContent) {
            string return_formIndexCell = "";
            try {
                string FieldText;
                string Filename;
                string Copy;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string[] lookups;
                int LookupPtr;
                int Pos;
                int lookupTableCnt = 0;
                // 
                // With...
                lookupTableCnt = adminContent.fields(fieldName.ToLower()).id;
                //  workaround for universally creating the left join tablename for each field
                switch (adminContent.fields(fieldName.ToLower()).fieldTypeId) {
                    case FieldTypeIdFile:
                    case FieldTypeIdFileImage:
                        Filename = cpCore.db.csGet(CS, adminContent.fields(fieldName.ToLower()).nameLc);
                        Filename = genericController.vbReplace(Filename, "\\", "/");
                        Pos = InStrRev(Filename, "/");
                        if ((Pos != 0)) {
                            Filename = Filename.Substring(Pos);
                        }
                        
                        Stream.Add(Filename);
                        break;
                    case FieldTypeIdLookup:
                        if (IsLookupFieldValid) {
                            Stream.Add(cpCore.db.csGet(CS, ("LookupTable" 
                                                + (lookupTableCnt + "Name"))));
                            lookupTableCnt++;
                        }
                        else if ((adminContent.fields(fieldName.ToLower()).lookupList != "")) {
                            lookups = adminContent.fields(fieldName.ToLower()).lookupList.Split(",");
                            LookupPtr = (cpCore.db.csGetInteger(CS, adminContent.fields(fieldName.ToLower()).nameLc) - 1);
                            if ((LookupPtr <= UBound(lookups))) {
                                if ((LookupPtr < 0)) {
                                    // Stream.Add( "-1")
                                }
                                else {
                                    Stream.Add(lookups[LookupPtr]);
                                }
                                
                            }
                            else {
                                // Stream.Add( "-2")
                            }
                            
                        }
                        else {
                            // Stream.Add( "-3")
                            Stream.Add(" ");
                        }
                        
                        break;
                    case FieldTypeIdMemberSelect:
                        if (IsLookupFieldValid) {
                            Stream.Add(cpCore.db.csGet(CS, ("LookupTable" 
                                                + (lookupTableCnt + "Name"))));
                            lookupTableCnt++;
                        }
                        else {
                            Stream.Add(cpCore.db.csGet(CS, adminContent.fields(fieldName.ToLower()).nameLc));
                        }
                        
                        break;
                    case FieldTypeIdBoolean:
                        if (cpCore.db.csGetBoolean(CS, adminContent.fields(fieldName.ToLower()).nameLc)) {
                            Stream.Add("yes");
                        }
                        else {
                            Stream.Add("no");
                        }
                        
                        break;
                    case FieldTypeIdCurrency:
                        Stream.Add(FormatCurrency(cpCore.db.csGetNumber(CS, adminContent.fields(fieldName.ToLower()).nameLc)));
                        break;
                    case FieldTypeIdLongText:
                    case FieldTypeIdHTML:
                        FieldText = cpCore.db.csGet(CS, adminContent.fields(fieldName.ToLower()).nameLc);
                        if ((FieldText.Length > 50)) {
                            FieldText = (FieldText.Substring(0, 50) + "[more]");
                        }
                        
                        Stream.Add(FieldText);
                        break;
                    case FieldTypeIdFileText:
                    case FieldTypeIdFileCSS:
                    case FieldTypeIdFileXML:
                    case FieldTypeIdFileJavascript:
                    case FieldTypeIdFileHTML:
                        //  rw( "n/a" )
                        Filename = cpCore.db.csGet(CS, adminContent.fields(fieldName.ToLower()).nameLc);
                        if ((Filename != "")) {
                            Copy = cpCore.cdnFiles.readFile(Filename);
                            //  20171103 - dont see why this is being converted, not html
                            // Copy = cpCore.html.convertActiveContent_internal(Copy, cpCore.doc.authContext.user.id, "", 0, 0, True, False, False, False, True, False, "", "", IsEmailContent, 0, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, cpCore.doc.authContext.isAuthenticated, Nothing, cpCore.doc.authContext.isEditingAnything())
                            Stream.Add(Copy);
                        }
                        
                        break;
                    case FieldTypeIdRedirect:
                    case FieldTypeIdManyToMany:
                        Stream.Add("n/a");
                        break;
                    default:
                        Stream.Add(cpCore.db.csGet(CS, adminContent.fields(fieldName.ToLower()).nameLc));
                        break;
                }
                // 
                return_formIndexCell = genericController.encodeHTML(Stream.Text);
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return return_formIndexCell;
        }
        
        // 
        // ========================================================================
        //  Get the Normal Edit Button Bar String
        // 
        //    used on Normal Edit and others
        // ========================================================================
        // 
        private string GetForm_Edit_ButtonBar(Models.Complex.cdefModel adminContent, editRecordClass editRecord, bool AllowDelete, bool allowSave, bool AllowAdd, bool AllowRefresh, void =, void False) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // Warning!!! Optional parameters not supported
            // 'Dim th as integer : th = profileLogAdminMethodEnter( "GetForm_Edit_ButtonBar")
            // 
            adminUIController Adminui = new adminUIController(cpCore);
            bool IncludeCDefReload;
            bool IsPageContent;
            bool HasChildRecords;
            int CS;
            bool AllowMarkReviewed;
            // 
            IsPageContent = Models.Complex.cdefModel.isWithinContent(cpCore, adminContent.Id, Models.Complex.cdefModel.getContentId(cpCore, "Page Content"));
            if (Models.Complex.cdefModel.isContentFieldSupported(cpCore, adminContent.Name, "parentid")) {
                CS = cpCore.db.csOpen(adminContent.Name, ("parentid=" + editRecord.id), ,, ,, ,, "ID");
                HasChildRecords = cpCore.db.csOk(CS);
                cpCore.db.csClose(CS);
            }
            
            IncludeCDefReload = ((adminContent.ContentTableName.ToLower() == "cccontent") 
                        | ((adminContent.ContentTableName.ToLower() == "ccdatasources") 
                        | ((adminContent.ContentTableName.ToLower() == "cctables") 
                        | (adminContent.ContentTableName.ToLower() == "ccfields"))));
            AllowMarkReviewed = cpCore.db.isSQLTableField("default", adminContent.ContentTableName, "DateReviewed");
            // 
            GetForm_Edit_ButtonBar = Adminui.GetEditButtonBar2(MenuDepth, (AllowDelete & editRecord.AllowDelete), editRecord.AllowCancel, (allowSave && editRecord.AllowSave), (SpellCheckSupported 
                            && !SpellCheckRequest), editRecord.AllowPublish, editRecord.AllowAbort, editRecord.AllowSubmit, editRecord.AllowApprove, (AllowAdd 
                            && (adminContent.AllowAdd && editRecord.AllowInsert)), IncludeCDefReload, HasChildRecords, IsPageContent, AllowMarkReviewed, AllowRefresh, (allowSave 
                            && (editRecord.AllowSave 
                            && (editRecord.id != 0))));
            // GetForm_Edit_ButtonBar = AdminUI.GetEditButtonBar2( MenuDepth, iAllowDelete And EditRecord.AllowDelete, EditRecord.AllowCancel, (iAllowSave And EditRecord.AllowSave), (SpellCheckSupported And (Not SpellCheckRequest)), EditRecord.AllowPublish, EditRecord.AllowAbort, EditRecord.AllowSubmit, EditRecord.AllowApprove, (AdminContent.allowAdd And EditRecord.AllowInsert), IncludeCDefReload, HasChildRecords, IsPageContent, AllowMarkReviewed)
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            handleLegacyClassError3("GetForm_Edit_ButtonBar");
            // 
        }
        
        // 
        // ========================================================================
        //  ----- Print the edit form
        // 
        //    Prints the correct form based on the current AdminContent.contenttablename
        //    AdminContent.type is not longer used
        // ========================================================================
        // 
        private string GetForm_Edit(Models.Complex.cdefModel adminContent, editRecordClass editRecord) {
            string returnHtml = "";
            try {
                csv_contentTypeEnum ContentType;
                string editorAddonListJSON;
                bool Active;
                string Name;
                string WFMessage = "";
                string IgnoreString = "";
                string styleList;
                string styleOptionList = "";
                string fieldEditorList;
                string[] fieldTypeDefaultEditors;
                string fieldEditorPreferencesList;
                DataTable dt;
                string[,] Cells;
                int fieldId;
                int TableID;
                DateTime LastSendTestDate;
                bool AllowEmailSendWithoutTest;
                Dictionary<string, int> fieldEditorOptions = new Dictionary<string, int>();
                int Ptr;
                int fieldEditorOptionCnt;
                string SQL;
                bool IsTemplateTable;
                int TemplateIDForStyles;
                int emailIdForStyles;
                //  Dim RootPageSectionID As Integer
                bool AllowajaxTabs;
                xmlController XMLTools = new xmlController(cpCore);
                bool IsPageContentTable;
                bool IsSectionTable;
                bool IsEmailTable;
                bool IsLandingPageTemp;
                int Pos;
                bool IsLandingPageParent;
                bool IsLandingSection;
                string CreatedCopy;
                string ModifiedCopy;
                int CS;
                string EditReferer;
                string CustomDescription;
                string EditSectionButtonBar;
                bool EmailSent;
                bool EmailSubmitted;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                int SystemEmailCID;
                int ConditionalEmailCID;
                string HeaderDescription;
                adminUIController Adminui = new adminUIController(cpCore);
                bool IsLandingPage;
                bool IsRootPage;
                string CreatedBy;
                bool allowCMEdit;
                bool allowCMAdd;
                bool allowCMDelete;
                bool AllowAdd;
                bool AllowDelete;
                bool allowSave;
                // 
                CustomDescription = "";
                AllowajaxTabs = cpCore.siteProperties.getBoolean("AllowAjaxEditTabBeta", false);
                if (((cpCore.doc.debug_iUserError != "") 
                            && editRecord.Loaded)) {
                    // 
                    //  block load if there was a user error and it is already loaded (assume error was from response )
                    // 
                }
                else if ((adminContent.Id <= 0)) {
                    // 
                    //  Invalid Content
                    // 
                    errorController.error_AddUserError(cpCore, "There was a problem identifying the content you requested. Please return to the previous form and ver" +
                        "ify your selection.");
                    return "";
                }
                else if ((editRecord.Loaded 
                            && !editRecord.Saved)) {
                    // 
                    //    File types need to be reloaded from the Db, because...
                    //        LoadDb - sets them to the path-page
                    //        LoadResponse - sets the blank if no change, filename if there is an upload
                    //        SaveEditRecord - if blank, no change. If a filename it saves the uploaded file
                    //        GetForm_Edit - expects the Db value to be in EditRecordValueVariants (path-page)
                    // 
                    //  xx This was added to bypass the load for the editrefresh case (reload the response so the editor preference can change)
                    //  xx  I do not know why the following section says "reload even if it is loaded", but lets try this
                    // 
                    foreach (keyValuePair in adminContent.fields) {
                        Models.Complex.CDefFieldModel field = keyValuePair.Value;
                        switch (field.fieldTypeId) {
                            case FieldTypeIdFile:
                            case FieldTypeIdFileImage:
                                editRecord.fieldsLc(field.nameLc).value = editRecord.fieldsLc(field.nameLc).dbValue;
                                break;
                        }
                    }
                    
                    // For Ptr = 0 To adminContent.fields.Count - 1
                    //     fieldType = arrayOfFields(Ptr).fieldType
                    //     Select Case fieldType
                    //         Case FieldTypeFile, FieldTypeImage
                    //             EditRecordValuesObject(Ptr) = EditRecordDbValues(Ptr)
                    //     End Select
                    // Next
                }
                else {
                    // 
                    //  otherwise, load the record, even if it was loaded during a previous form process
                    // 
                    LoadEditRecord(adminContent, editRecord, true);
                    if ((editRecord.contentControlId == 0)) {
                        if ((cpCore.doc.debug_iUserError != "")) {
                            // 
                            //  known user error, just return
                            // 
                        }
                        else {
                            // 
                            //  unknown error, set userError and return
                            // 
                            errorController.error_AddUserError(cpCore, "There was an unknown error in your request for data. Please let the site administrator know.");
                        }
                        
                        return "";
                    }
                    
                }
                
                // 
                //  Test if this editors has access to this record
                // 
                if (!this.userHasContentAccess(editRecord.contentControlId)) {
                    errorController.error_AddUserError(cpCore, "Your account on this system does not have access rights to edit this content.");
                    return "";
                }
                
                if (false) {
                    // 
                    //  Test for 100Mb available in Content Files drive
                    // 
                    if ((cpCore.appRootFiles.getDriveFreeSpace() < 100000000)) {
                        errorController.error_AddUserError(cpCore, "The server drive holding data for this site may not have enough free space to complete this edit oper" +
                            "ation. If you attempt to save, your data may be lost. Please contact the site administrator.");
                    }
                    
                    if ((cpCore.privateFiles.getDriveFreeSpace() < 100000000)) {
                        errorController.error_AddUserError(cpCore, "The server drive holding data for this site may not have enough free space to complete this edit oper" +
                            "ation. If you attempt to save, your data may be lost. Please contact the site administrator.");
                    }
                    
                }
                
                // 
                //  Setup Edit Referer
                // 
                EditReferer = cpCore.docProperties.getText(RequestNameEditReferer);
                if ((EditReferer == "")) {
                    EditReferer = cpCore.webServer.requestReferer;
                    if ((EditReferer != "")) {
                        // 
                        //  special case - if you are coming from the advanced search, go back to the list page
                        // 
                        EditReferer = genericController.vbReplace(EditReferer, "&af=39", "");
                        // 
                        //  if referer includes AdminWarningMsg (admin hint message), remove it -- this edit may fix the problem
                        // 
                        Pos = genericController.vbInstr(1, EditReferer, "AdminWarningMsg=", vbTextCompare);
                        if ((Pos != 0)) {
                            EditReferer = EditReferer.Substring(0, (Pos - 2));
                        }
                        
                    }
                    
                }
                
                cpCore.doc.addRefreshQueryString(RequestNameEditReferer, EditReferer);
                // 
                //  Print common form elements
                // 
                Stream.Add(GetForm_EditFormStart(AdminFormEdit));
                // 
                IsLandingPageParent = false;
                IsLandingPage = false;
                IsRootPage = false;
                TemplateIDForStyles = 0;
                IsTemplateTable = (adminContent.ContentTableName.ToLower() == "cctemplates");
                IsPageContentTable = (adminContent.ContentTableName.ToLower() == "ccpagecontent");
                IsSectionTable = (adminContent.ContentTableName.ToLower() == "ccsections");
                IsEmailTable = (adminContent.ContentTableName.ToLower() == "ccemail");
                if (IsEmailTable) {
                    // 
                    //  ----- special case - email
                    // 
                    emailIdForStyles = editRecord.id;
                }
                
                // 
                if (IsPageContentTable) {
                    // 
                    //  ----- special case - page content
                    // 
                    if ((editRecord.id != 0)) {
                        // 
                        //  landing page case
                        // 
                        // $$$$$ problem -- could be landing page based on domain, not property
                        // LandingPageID = (cpCore.siteProperties.getinteger("LandingPageID", 0))
                        if ((cpCore.siteProperties.landingPageID == 0)) {
                            // 
                            //  The call generated a user error because the landingpageid could not be determined
                            //  block it
                            // 
                            // Call cpCore.main_GetUserError
                        }
                        else {
                            IsLandingPage = (editRecord.id == cpCore.siteProperties.landingPageID);
                            IsRootPage = (IsPageContentTable 
                                        & (editRecord.parentID == 0));
                        }
                        
                    }
                    
                }
                
                // 
                if ((!IsLandingPage 
                            && (IsPageContentTable || IsSectionTable))) {
                    // '
                    // ' ----- special case, Is this page a LandingPageParent (Parent of the landing page), or is this section the landing page section
                    // '
                    // TestPageID = cpCore.siteProperties.landingPageID
                    // Do While LoopPtr < 20 And (TestPageID <> 0)
                    //     IsLandingPageParent = IsPageContentTable And (editRecord.id = TestPageID)
                    //     IsLandingSection = IsSectionTable And (EditRecordRootPageID = TestPageID)
                    //     If IsLandingPageParent Or IsLandingSection Then
                    //         Exit Do
                    //     End If
                    //     PCCPtr = cpCore.pages.cache_pageContent_getPtr(TestPageID, False, False)
                    //     If PCCPtr >= 0 Then
                    //         TestPageID = genericController.EncodeInteger(PCC(PCC_ParentID, PCCPtr))
                    //     End If
                    //     LoopPtr = LoopPtr + 1
                    // Loop
                }
                
                // 
                //  ----- special case messages
                // 
                if (IsLandingSection) {
                    CustomDescription = "<div>This is the default Landing Section for this website. This section is displayed when no specific" +
                    " page is requested. It should not be deleted, renamed, marked inactive, blocked or hidden.</div>";
                }
                else if (IsLandingPageTemp) {
                    CustomDescription = @"<div>This page is being used as the default Landing Page for this website, although it has not been set. This may be because a landing page has not been created, or it has been deleted. To make this page the permantent landing page, check the appropriate box in the control tab.</div>";
                }
                else if (IsLandingPage) {
                    CustomDescription = "<div>This is the default Landing Page for this website. It should not be deleted. You can not mark th" +
                    "is record inactive, or use the Publish Date, Expire Date or Archive Date features.</div>";
                }
                else if (IsLandingPageParent) {
                    CustomDescription = "<div>This page is a parent of the default Landing Page for this website. It should not be deleted. Yo" +
                    "u can not mark this record inactive, or use the Publish Date, Expire Date or Archive Date features.<" +
                    "/div>";
                }
                else if (IsRootPage) {
                    CustomDescription = "<div>This page is a Root Page. A Root Page is the primary page of a section. If you delete or inactiv" +
                    "ate this page, the section will create a new blank page in its place.</div>";
                }
                
                // 
                //  ----- Determine TemplateIDForStyles
                // 
                if (IsTemplateTable) {
                    TemplateIDForStyles = editRecord.id;
                }
                else if (IsPageContentTable) {
                    // Call cpCore.pages.getPageArgs(editRecord.id, false, False, ignoreInteger, TemplateIDForStyles, ignoreInteger, IgnoreString, IgnoreBoolean, ignoreInteger, IgnoreBoolean, "")
                }
                
                // 
                //  ----- create page headers
                // 
                if ((editRecord.id == 0)) {
                    HeaderDescription = "<div>New Record</div>";
                }
                else {
                    HeaderDescription = ("" + "<table border=0 cellpadding=0 cellspacing=0 style=\"width:90%\">");
                    if ((CustomDescription != "")) {
                        HeaderDescription = (HeaderDescription + ("<tr><td colspan=2>" 
                                    + (CustomDescription + "<div> </div></td></tr>")));
                    }
                    
                    HeaderDescription = (HeaderDescription + ("<tr><td width=\"50%\">" + ("Name: " 
                                + (editRecord.nameLc + ("<br>Record ID: " 
                                + (editRecord.id + "</td><td width=\"50%\">"))))));
                    CreatedCopy = "";
                    DateTime editRecordDateAdded;
                    editRecordDateAdded = genericController.EncodeDate(editRecord.fieldsLc("dateadded").value);
                    MinValue;
                    CreatedCopy = (CreatedCopy + (" " + editRecordDateAdded.ToString()));
                    //  editRecord.dateAdded
                }
                
                // 
                CreatedBy = "the system";
                if ((editRecord.createByMemberId != 0)) {
                    CS = cpCore.db.csOpenSql_rev("default", ("select Name,Active from ccMembers where id=" + editRecord.createByMemberId));
                    // CS = cpCore.app.openCsSql_rev("default", "select Name,Active from ccmembers where id=" & EditRecord.AddedByMemberID)
                    if (cpCore.db.csOk(CS)) {
                        Name = cpCore.db.csGetText(CS, "name");
                        Active = cpCore.db.csGetBoolean(CS, "active");
                        if ((!Active 
                                    && (Name != ""))) {
                            CreatedBy = ("Inactive user " + Name);
                        }
                        else if (!Active) {
                            CreatedBy = ("Inactive user #" + editRecord.createByMemberId);
                        }
                        else if ((Name == "")) {
                            CreatedBy = ("Unnamed user #" + editRecord.createByMemberId);
                        }
                        else {
                            CreatedBy = Name;
                        }
                        
                    }
                    else {
                        CreatedBy = ("deleted user #" + editRecord.createByMemberId);
                    }
                    
                    cpCore.db.csClose(CS);
                }
                
                if ((CreatedBy != "")) {
                    CreatedCopy = (CreatedCopy + (" by " + CreatedBy));
                }
                else {
                    
                }
                
                HeaderDescription = (HeaderDescription + ("Created:" + CreatedCopy));
                // 
                ModifiedCopy = "";
                MinValue;
                ModifiedCopy = CreatedCopy;
                ModifiedCopy = (ModifiedCopy + (" " + editRecord.modifiedDate));
                CreatedBy = "the system";
                if ((editRecord.modifiedByMemberID != 0)) {
                    CS = cpCore.db.csOpenSql_rev("default", ("select Name,Active from ccMembers where id=" + editRecord.modifiedByMemberID));
                    // CS = cpCore.app.openCsSql_rev("default", "select Name,Active from ccmembers where id=" & EditRecord.ModifiedByMemberID)
                    if (cpCore.db.csOk(CS)) {
                        Name = cpCore.db.csGetText(CS, "name");
                        Active = cpCore.db.csGetBoolean(CS, "active");
                        if ((!Active 
                                    && (Name != ""))) {
                            CreatedBy = ("Inactive user " + Name);
                        }
                        else if (!Active) {
                            CreatedBy = ("Inactive user #" + editRecord.modifiedByMemberID);
                        }
                        else if ((Name == "")) {
                            CreatedBy = ("Unnamed user #" + editRecord.modifiedByMemberID);
                        }
                        else {
                            CreatedBy = Name;
                        }
                        
                    }
                    else {
                        CreatedBy = ("deleted user #" + editRecord.modifiedByMemberID);
                    }
                    
                    cpCore.db.csClose(CS);
                }
                
                if ((CreatedBy != "")) {
                    ModifiedCopy = (ModifiedCopy + (" by " + CreatedBy));
                }
                else {
                    
                }
                
            }
            
            if (false) {
                if (editRecord.IsInserted) {
                    HeaderDescription = (HeaderDescription + "<BR >Last Published: not published");
                }
                else {
                    HeaderDescription = (HeaderDescription + ("<BR >Last Published:" + ModifiedCopy));
                }
                
            }
            else {
                HeaderDescription = (HeaderDescription + ("<BR >Last Modified:" + ModifiedCopy));
            }
            
            // 
            //  Add Edit Locking to right panel
            // 
            if (editRecord.EditLock) {
                HeaderDescription = (HeaderDescription + ("<BR ><b>Record is locked by " 
                            + (editRecord.EditLockMemberName + (" until " 
                            + (editRecord.EditLockExpires + "</b>")))));
            }
            
            // 
            HeaderDescription = (HeaderDescription + "</td></tr>");
            HeaderDescription = (HeaderDescription + "</table>");
        }
        
        // 
        //  add the addon editors assigned to each field
        //  !!!!! this should be added to metaData load
        // 
        private int Cnt;
    }
}
Next// 
//  ----- determine contentType for editor
// 
if ((genericController.vbLCase(adminContent.Name) == "email templates")) {
    ContentType = csv_contentTypeEnum.contentTypeEmailTemplate;
}
else if ((genericController.vbLCase(adminContent.ContentTableName) == "cctemplates")) {
    ContentType = csv_contentTypeEnum.contentTypeWebTemplate;
}
else if ((genericController.vbLCase(adminContent.ContentTableName) == "ccemail")) {
    ContentType = csv_contentTypeEnum.contentTypeEmail;
}
else {
    ContentType = csv_contentTypeEnum.contentTypeWeb;
}

// 
//  ----- editor strings needed - needs to be on-demand
// 
editorAddonListJSON = cpCore.html.main_GetEditorAddonListJSON(ContentType);
styleList = "";
switch (genericController.vbUCase(adminContent.ContentTableName)) {
    case genericController.vbUCase("ccMembers"):
        if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
            // 
            //  Must be admin
            // 
            Stream.Add(GetForm_Error("This edit form requires Member Administration access.", ""));
        }
        else {
            EditSectionButtonBar = this.GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd);
            EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeletePerson);
            Stream.Add(EditSectionButtonBar);
            Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
            Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
            Stream.Add(GetForm_Edit_AddTab("Groups", GetForm_Edit_MemberGroups(adminContent, editRecord), allowAdminTabs));
            // Call Stream.Add(GetForm_Edit_AddTab("Topics", GetForm_Edit_TopicRules, AllowAdminTabs))
            // Call Stream.Add(GetForm_Edit_AddTab("Calendar", GetForm_Edit_CalendarEvents, AllowAdminTabs))
            Stream.Add(GetForm_Edit_AddTab("Reports", GetForm_Edit_MemberReports(adminContent, editRecord), allowAdminTabs));
            Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
            if (allowAdminTabs) {
                Stream.Add(cpCore.html.menu_GetComboTabs());
            }
            
            Stream.Add(EditSectionButtonBar);
        }
        
        break;
    case "CCEMAIL":
        SystemEmailCID = Models.Complex.cdefModel.getContentId(cpCore, "System Email");
        ConditionalEmailCID = Models.Complex.cdefModel.getContentId(cpCore, "Conditional Email");
        MinValue;
        if (true) {
            //  3.4.201" Then
            AllowEmailSendWithoutTest = cpCore.siteProperties.getBoolean("AllowEmailSendWithoutTest", false);
            if (editRecord.fieldsLc.ContainsKey("lastsendtestdate")) {
                LastSendTestDate = genericController.EncodeDate(editRecord.fieldsLc("lastsendtestdate").value);
            }
            
        }
        
        if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
            // 
            //  Must be admin
            // 
            Stream.Add(GetForm_Error("This edit form requires Member Administration access.", "This edit form requires Member Administration access."));
        }
        else if (Models.Complex.cdefModel.isWithinContent(cpCore, editRecord.contentControlId, SystemEmailCID)) {
            // 
            //  System Email
            // 
            EmailSubmitted = false;
            if ((editRecord.id != 0)) {
                if (editRecord.fieldsLc.ContainsKey("testmemberid")) {
                    editRecord.fieldsLc.Item["testmemberid"].value = cpCore.doc.authContext.user.id;
                }
                
            }
            
            EditSectionButtonBar = "";
            if ((MenuDepth > 0)) {
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonClose, ,, "window.close();"));
            }
            else {
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonCancel, ,, "Return processSubmit(this)"));
            }
            
            if ((AllowDelete && cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore))) {
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonDeleteEmail, ,, "If(!DeleteCheck())Return False;"));
            }
            
            if ((!EmailSubmitted 
                        && !EmailSent)) {
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonSave, ,, "Return processSubmit(this)"));
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonOK, ,, "Return processSubmit(this)"));
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonSendTest, ,, "Return processSubmit(this)"));
            }
            else if (AllowAdd) {
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonCreateDuplicate, ,, "Return processSubmit(this)"));
            }
            
            EditSectionButtonBar = cpCore.html.main_GetPanel(EditSectionButtonBar, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 4);
            // 
            Stream.Add(EditSectionButtonBar);
            Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
            Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
            Stream.Add(GetForm_Edit_AddTab("Send To Groups", GetForm_Edit_EmailRules(adminContent, editRecord, (editRecord.Read_Only 
                                    & !cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore))), allowAdminTabs));
            Stream.Add(GetForm_Edit_AddTab("Send To Topics", GetForm_Edit_EmailTopics(adminContent, editRecord, (editRecord.Read_Only 
                                    & !cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore))), allowAdminTabs));
            Stream.Add(GetForm_Edit_AddTab("Bounce Control", GetForm_Edit_EmailBounceStatus(), allowAdminTabs));
            Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
            if (allowAdminTabs) {
                Stream.Add(cpCore.html.menu_GetComboTabs());
                // Call Stream.Add("<div Class=""ccPanelBackground"">" & cpCore.main_GetComboTabs() & "</div>")
            }
            
            Stream.Add(EditSectionButtonBar);
        }
        else if (Models.Complex.cdefModel.isWithinContent(cpCore, editRecord.contentControlId, ConditionalEmailCID)) {
            // 
            //  Conditional Email
            // 
            EmailSubmitted = false;
            if ((editRecord.id != 0)) {
                if (editRecord.fieldsLc.ContainsKey("testmemberid")) {
                    editRecord.fieldsLc.Item["testmemberid"].value = cpCore.doc.authContext.user.id;
                }
                
                if (editRecord.fieldsLc.ContainsKey("submitted")) {
                    EmailSubmitted = genericController.EncodeBoolean(editRecord.fieldsLc.Item["submitted"].value);
                }
                
            }
            
            EditSectionButtonBar = "";
            if ((MenuDepth > 0)) {
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonClose, ,, "window.close();"));
            }
            else {
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonCancel, ,, "Return processSubmit(this)"));
            }
            
            if (AllowDelete) {
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonDeleteEmail, ,, "If(!DeleteCheck())Return False;"));
            }
            
            if (!EmailSubmitted) {
                // 
                //  Not Submitted
                // 
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonSave, ,, "Return processSubmit(this)"));
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonOK, ,, "Return processSubmit(this)"));
                EditSectionButtonBar = ((EditSectionButtonBar + cpCore.html.html_GetFormButton2(ButtonActivate, ,, "Return processSubmit(this)", (LastSendTestDate=, Date, ., MinValue)) 
                            & !AllowEmailSendWithoutTest);
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonSendTest, ,, "Return processSubmit(this)"));
            }
            else {
                // 
                //  Submitted
                // 
                if (AllowAdd) {
                    EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonCreateDuplicate, ,, "Return processSubmit(this)"));
                }
                
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonDeactivate, ,, "Return processSubmit(this)"));
            }
            
            EditSectionButtonBar = cpCore.html.main_GetPanel(EditSectionButtonBar, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 4);
            // 
            Stream.Add(EditSectionButtonBar);
            Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
            Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, (editRecord.Read_Only | EmailSubmitted), false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
            Stream.Add(GetForm_Edit_AddTab("Condition Groups", GetForm_Edit_EmailRules(adminContent, editRecord, (editRecord.Read_Only | EmailSubmitted)), allowAdminTabs));
            // Call Stream.Add(GetForm_Edit_AddTab("Send To Topics", GetForm_Edit_EmailTopics(editrecord.read_only Or EmailSubmitted), AllowAdminTabs))
            Stream.Add(GetForm_Edit_AddTab("Bounce Control", GetForm_Edit_EmailBounceStatus(), allowAdminTabs));
            Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
            if (allowAdminTabs) {
                Stream.Add(cpCore.html.menu_GetComboTabs());
                // Call Stream.Add("<div Class=""ccPanelBackground"">" & cpCore.main_GetComboTabs() & "</div>")
            }
            
            Stream.Add(EditSectionButtonBar);
        }
        else {
            // 
            //  Group Email
            // 
            EmailSubmitted = false;
            EmailSent = false;
            if ((editRecord.id != 0)) {
                if (editRecord.fieldsLc.ContainsKey("testmemberid")) {
                    editRecord.fieldsLc.Item["testmemberid"].value = cpCore.doc.authContext.user.id;
                }
                
                if (editRecord.fieldsLc.ContainsKey("submitted")) {
                    EmailSubmitted = genericController.EncodeBoolean(editRecord.fieldsLc.Item["submitted"].value);
                }
                
                if (editRecord.fieldsLc.ContainsKey("sent")) {
                    EmailSent = genericController.EncodeBoolean(editRecord.fieldsLc.Item["sent"].value);
                }
                
            }
            
            EditSectionButtonBar = "";
            if ((MenuDepth > 0)) {
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonClose, ,, "window.close();"));
            }
            else {
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonCancel, ,, "Return processSubmit(this)"));
            }
            
            if ((editRecord.id != 0)) {
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonDeleteEmail, ,, "If(!DeleteCheck())Return False;"));
            }
            
            if ((!EmailSubmitted 
                        && !EmailSent)) {
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonSave, ,, "Return processSubmit(this)"));
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonOK, ,, "Return processSubmit(this)"));
                EditSectionButtonBar = ((EditSectionButtonBar + cpCore.html.html_GetFormButton2(ButtonSend, ,, "Return processSubmit(this)", (LastSendTestDate=, Date, ., MinValue)) 
                            & !AllowEmailSendWithoutTest);
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonSendTest, ,, "Return processSubmit(this)"));
            }
            else {
                // 
                //  Submitted
                // 
                EditSectionButtonBar = (EditSectionButtonBar + cpCore.html.html_GetFormButton(ButtonCreateDuplicate, ,, "Return processSubmit(this)"));
            }
            
            EditSectionButtonBar = cpCore.html.main_GetPanel(EditSectionButtonBar, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 4);
            // 
            Stream.Add(EditSectionButtonBar);
            Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
            Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, (editRecord.Read_Only 
                                | (EmailSubmitted | EmailSent)), false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
            Stream.Add(GetForm_Edit_AddTab("Send To Groups", GetForm_Edit_EmailRules(adminContent, editRecord, (editRecord.Read_Only 
                                    | (EmailSubmitted | EmailSent))), allowAdminTabs));
            Stream.Add(GetForm_Edit_AddTab("Send To Topics", GetForm_Edit_EmailTopics(adminContent, editRecord, (editRecord.Read_Only 
                                    | (EmailSubmitted | EmailSent))), allowAdminTabs));
            Stream.Add(GetForm_Edit_AddTab("Bounce Control", GetForm_Edit_EmailBounceStatus(), allowAdminTabs));
            Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
            if (allowAdminTabs) {
                Stream.Add(cpCore.html.menu_GetComboTabs());
                // Call Stream.Add("<div Class=""ccPanelBackground"">" & cpCore.main_GetComboTabs() & "</div>")
            }
            
            Stream.Add(EditSectionButtonBar);
        }
        
        break;
    case "CCCONTENT":
        if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
            // 
            //  Must be admin
            // 
            Stream.Add(GetForm_Error("This edit form requires Member Administration access.", "This edit form requires Member Administration access."));
        }
        else {
            EditSectionButtonBar = this.GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd);
            EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord);
            Stream.Add(EditSectionButtonBar);
            Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
            Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
            Stream.Add(GetForm_Edit_AddTab("Authoring Permissions", GetForm_Edit_GroupRules(adminContent, editRecord), allowAdminTabs));
            Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
            if (allowAdminTabs) {
                Stream.Add(cpCore.html.menu_GetComboTabs());
                // Call Stream.Add("<div class=""ccPanelBackground"">" & cpCore.main_GetComboTabs() & "</div>")
            }
            
            Stream.Add(EditSectionButtonBar);
        }
        
        // 
        break;
    case "CCPAGECONTENT":
        TableID = cpCore.db.getRecordID("Tables", "ccPageContent");
        EditSectionButtonBar = this.GetForm_Edit_ButtonBar(adminContent, editRecord, (!IsLandingPage 
                        & (!IsLandingPageParent 
                        & AllowDelete)), allowSave, AllowAdd, true);
        EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeletePage);
        Stream.Add(EditSectionButtonBar);
        Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
        Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, (IsLandingPage | IsLandingPageParent), IsRootPage, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
        // Call Stream.Add(GetForm_Edit_AddTab("Meta Content", GetForm_Edit_MetaContent(adminContent, editRecord, editRecord.Read_Only), allowAdminTabs))
        Stream.Add(GetForm_Edit_AddTab("Link Aliases", GetForm_Edit_LinkAliases(adminContent, editRecord, editRecord.Read_Only), allowAdminTabs));
        // Call Stream.Add(GetForm_Edit_AddTab("Topics", GetForm_Edit_TopicRules, AllowAdminTabs))
        // Call Stream.Add(GetForm_Edit_AddTab("RSS/Podcasts", GetForm_Edit_RSSFeeds(EditRecord.ContentName, EditRecord.ContentID, EditRecord.ID, cpCore.main_GetPageLink(EditRecord.ID)), AllowAdminTabs))
        Stream.Add(GetForm_Edit_AddTab("Content Watch", GetForm_Edit_ContentTracking(adminContent, editRecord), allowAdminTabs));
        // Call Stream.Add(GetForm_Edit_AddTab("Calendar", GetForm_Edit_CalendarEvents, AllowAdminTabs))
        Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
        if (allowAdminTabs) {
            Stream.Add(cpCore.html.menu_GetComboTabs());
        }
        
        Stream.Add(EditSectionButtonBar);
        // Case "CCSECTIONS"
        //     '
        //     ' Site Sections
        //     '
        //     EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, (Not IsLandingSection) And AllowDelete, allowSave, AllowAdd)
        //     EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
        //     Call Stream.Add(EditSectionButtonBar)
        //     Call Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
        //     Call Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, IsLandingSection, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
        //     Call Stream.Add(GetForm_Edit_AddTab("Select Menus", GetForm_Edit_SectionDynamicMenuRules(adminContent, editRecord), allowAdminTabs))
        //     Call Stream.Add(GetForm_Edit_AddTab("Section Blocking", GetForm_Edit_SectionBlockRules(adminContent, editRecord), allowAdminTabs))
        //     Call Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs))
        //     If allowAdminTabs Then
        //         Call Stream.Add(cpCore.htmlDoc.menu_GetComboTabs())
        //         'Call Stream.Add("<div class=""ccPanelBackground"">" & cpCore.main_GetComboTabs() & "</div>")
        //     End If
        //     Call Stream.Add(EditSectionButtonBar)
        // Case "CCDYNAMICMENUS"
        //     '
        //     ' Edit Dynamic Sections
        //     '
        //     EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd)
        //     EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
        //     Call Stream.Add(EditSectionButtonBar)
        //     Call Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
        //     Call Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, False, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
        //     Call Stream.Add(GetForm_Edit_AddTab("Select Sections", GetForm_Edit_DynamicMenuSectionRules(adminContent, editRecord), allowAdminTabs))
        //     Call Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs))
        //     If allowAdminTabs Then
        //         Call Stream.Add(cpCore.htmlDoc.menu_GetComboTabs())
        //         'Call Stream.Add("<div class=""ccPanelBackground"">" & cpCore.main_GetComboTabs() & "</div>")
        //     End If
        //     Call Stream.Add(EditSectionButtonBar)
        break;
    case "CCLIBRARYFOLDERS":
        EditSectionButtonBar = this.GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd);
        EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord);
        Stream.Add(EditSectionButtonBar);
        Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
        Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
        Stream.Add(GetForm_Edit_AddTab("Authoring Access", GetForm_Edit_LibraryFolderRules(adminContent, editRecord), allowAdminTabs));
        Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
        if (allowAdminTabs) {
            Stream.Add(cpCore.html.menu_GetComboTabs());
        }
        
        Stream.Add(EditSectionButtonBar);
        break;
    case genericController.vbUCase("ccGroups"):
        // Case "CCGROUPS"
        // 
        //  Groups
        // 
        EditSectionButtonBar = this.GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd);
        EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord);
        Stream.Add(EditSectionButtonBar);
        Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
        Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
        Stream.Add(GetForm_Edit_AddTab("Authoring Permissions", GetForm_Edit_ContentGroupRules(adminContent, editRecord), allowAdminTabs));
        // Call Stream.Add(GetForm_Edit_AddTab("Meta Content", GetForm_Edit_MetaContent(adminContent, editRecord, editRecord.Read_Only), allowAdminTabs))
        // Call Stream.Add(GetForm_Edit_AddTab("Topics", GetForm_Edit_TopicRules, AllowAdminTabs))
        Stream.Add(GetForm_Edit_AddTab("Content Watch", GetForm_Edit_ContentTracking(adminContent, editRecord), allowAdminTabs));
        // Call Stream.Add(GetForm_Edit_AddTab("Calendar", GetForm_Edit_CalendarEvents, AllowAdminTabs))
        Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
        if (allowAdminTabs) {
            Stream.Add(cpCore.html.menu_GetComboTabs());
        }
        
        Stream.Add(EditSectionButtonBar);
        // 
        //  This is the beginnings of a good idea. use a selector string to create the value input. The problem is
        //  both the selector and value appear on the same page. if you screw up the selector, you can not save it
        //  again without also saving the 'bad' value it creates.
        // 
        //  For now, skip this and put the higher-level interface in control pages (an add-on type).
        // 
        //         Case "CCSETUP"
        //             '
        //             '   Site Properties
        //             '
        //             EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord,)
        //             EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
        //             Call Stream.Add(EditSectionButtonBar)
        //             Call Stream.Add(Adminui.GetTitleBar( GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
        //             Call Stream.Add(GetForm_Edit_UserFieldTabs(adminContent, editRecord,FormID, editrecord.read_only, False, False, ContentType, AllowAjaxTabs))
        //             Call Stream.Add(GetForm_Edit_AddTab("Site Property", GetForm_Edit_SiteProperties(FormID), AllowAdminTabs))
        //             Call Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editrecord), AllowAdminTabs))
        //             If AllowAdminTabs Then
        //                 Call Stream.Add(cpCore.main_GetComboTabs())
        //             End If
        //             Call Stream.Add(EditSectionButtonBar)
        break;
    case "CCLAYOUTS":
        EditSectionButtonBar = this.GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd);
        EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord);
        Stream.Add(EditSectionButtonBar);
        Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
        Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
        Stream.Add(GetForm_Edit_AddTab("Reports", GetForm_Edit_LayoutReports(adminContent, editRecord), allowAdminTabs));
        Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
        if (allowAdminTabs) {
            Stream.Add(cpCore.html.menu_GetComboTabs());
        }
        
        Stream.Add(EditSectionButtonBar);
        break;
    default:
        EditSectionButtonBar = this.GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd);
        EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord);
        Stream.Add(EditSectionButtonBar);
        Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription));
        Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
        // Call Stream.Add(GetForm_Edit_AddTab("Meta Content", GetForm_Edit_MetaContent(adminContent, editRecord, editRecord.Read_Only), allowAdminTabs))
        Stream.Add(GetForm_Edit_AddTab("Content Watch", GetForm_Edit_ContentTracking(adminContent, editRecord), allowAdminTabs));
        Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs));
        if (allowAdminTabs) {
            Stream.Add(cpCore.html.menu_GetComboTabs());
        }
        
        Stream.Add(EditSectionButtonBar);
        break;
}
Stream.Add("</form>");
returnHtml = Stream.Text;
if ((editRecord.id == 0)) {
    cpCore.html.addTitle(("Add " + adminContent.Name));
}
else if ((editRecord.nameLc == "")) {
    cpCore.html.addTitle(("Edit #" 
                    + (editRecord.id + (" in " + editRecord.contentControlId_Name))));
}
else {
    cpCore.html.addTitle(("Edit " 
                    + (editRecord.nameLc + (" in " + editRecord.contentControlId_Name))));
}

CatchException ex;
cpCore.handleException(ex);
throw;
Endtry {
    return returnHtml;
}

// '
// '========================================================================
// ' Print the DHTMLEdit form
// '========================================================================
// '
// Private Function GetForm_EditHTML() As String
//     On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.GetForm_EditHTML")
//     '
//     Dim WhereCount as integer
//     Dim FastString As fastStringClass
//     '
//     FastString = New FastStringClass
//     FastString.Add( cpCore.main_GetFormInputHidden(RequestNameAdminDepth, MenuDepth))
//     FastString.Add( cpCore.main_GetFormInputHidden(RequestNameTitleExtension, TitleExtension))
//     If WherePairCount > 0 Then
//         For WhereCount = 0 To WherePairCount - 1
//             FastString.Add( cpCore.main_GetFormInputHidden("wl" & WhereCount, WherePair(0, WhereCount)))
//             FastString.Add( cpCore.main_GetFormInputHidden("wr" & WhereCount, WherePair(1, WhereCount)))
//             Next
//         End If
//     GetForm_EditHTML = cpCore.main_GetActiveEditor(AdminContent.Name, EditRecord.ID, InputFieldName, FastString.Text)
//     FastString = Nothing
//     '
//     '''Dim th as integer: Exit Function
//     '
//     ' ----- Error Trap
//     '
// ErrorTrap:
//     FastString = Nothing
//     Call HandleClassTrapErrorBubble("PrintDHTMLEditForm")
//     '
// End Function
//         '
//         '========================================================================
//         ' Print the DHTMLEdit form
//         '========================================================================
//         '
//         Private Function GetForm_StaticPublishControl() As String
//             On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "GetForm_StaticPublishControl")
//             '
//             Dim WhereCount As Integer
//             Dim Content As New stringBuilderLegacyController
//             Dim EDGPublishNow As Boolean
//             Dim Activity As String
//             Dim TargetDomain As String
//             Dim EDGCreateSnapShot As Boolean
//             Dim EDGPublishToProduction As Boolean
//             Dim CSPointer As Integer
//             Dim Copy As String
//             Dim StagingServer As String
//             Dim PagesFound As Integer
//             Dim PagesComplete As Integer
//             Dim SQL As String
//             Dim Button As String
//             Dim EDGAuthUsername As String
//             Dim EDGAuthPassword As String
//             Dim QueryString As String
//             Dim Adminui As New adminUIController(cpCore)
//             Dim Description As String
//             Dim ButtonList As String = ""
//             '
//             Button = cpCore.docProperties.getText(RequestNameButton)
//             If Button = ButtonCancel Then
//                 '
//                 '
//                 '
//                 Return cpCore.webServer.redirect("/" & cpCore.serverConfig.appConfig.adminRoute, "StaticPublishControl, Cancel Button Pressed")
//             ElseIf Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
//                 '
//                 '
//                 '
//                 ButtonList = ButtonCancel
//                 Content.Add(Adminui.GetFormBodyAdminOnly())
//             Else
//                 Content.Add(Adminui.EditTableOpen)
//                 '
//                 ' Set defaults
//                 '
//                 EDGCreateSnapShot = (cpCore.siteProperties.getBoolean("EDGCreateSnapShot", True))
//                 EDGPublishToProduction = (cpCore.siteProperties.getBoolean("EDGPublishToProduction", True))
//                 EDGPublishNow = (cpCore.siteProperties.getBoolean("EDGPublishNow"))
//                 EDGAuthUsername = cpCore.siteProperties.getText("EDGAuthUsername", "")
//                 EDGAuthPassword = cpCore.siteProperties.getText("EDGAuthPassword", "")
//                 '
//                 ' Process Requests
//                 '
//                 Select Case Button
//                     Case ButtonBegin
//                         '
//                         ' Save form values
//                         '
//                         EDGAuthUsername = cpCore.docProperties.getText("EDGAuthUsername")
//                         Call cpCore.siteProperties.setProperty("EDGAuthUsername", EDGAuthUsername)
//                         '
//                         EDGAuthPassword = cpCore.docProperties.getText("EDGAuthPassword")
//                         Call cpCore.siteProperties.setProperty("EDGAuthPassword", EDGAuthPassword)
//                         '
//                         EDGCreateSnapShot = cpCore.docProperties.getBoolean("EDGCreateSnapShot")
//                         Call cpCore.siteProperties.setProperty("EDGCreateSnapShot", genericController.encodeText(EDGCreateSnapShot))
//                         '
//                         EDGPublishToProduction = cpCore.docProperties.getBoolean("EDGPublishToProduction")
//                         Call cpCore.siteProperties.setProperty("EDGPublishToProduction", genericController.encodeText(EDGPublishToProduction))
//                         '
//                         ' Begin Publish
//                         '
//                         EDGPublishNow = (EDGCreateSnapShot Or EDGPublishToProduction)
//                         Call cpCore.siteProperties.setProperty("EDGPublishNow", genericController.encodeText(EDGPublishNow))
//                     Case ButtonAbort
//                         '
//                         ' Abort Publish
//                         '
//                         EDGPublishNow = False
//                         Call cpCore.siteProperties.setProperty("EDGPublishNow", genericController.encodeText(EDGPublishNow))
//                     Case ButtonRefresh
//                         '
//                         ' Refresh (no action)
//                         '
//                 End Select
//                 '
//                 ' ----- Status
//                 '
//                 If EDGPublishNow Then
//                     Copy = "Started"
//                 Else
//                     Copy = "Stopped"
//                 End If
//                 Call Content.Add(Adminui.GetEditRow(Copy, "Status", "", False, False, ""))
//                 '
//                 ' ----- activity
//                 '
//                 Copy = genericController.encodeText(cpCore.siteProperties.getText("EDGPublishStatus", "Waiting"))
//                 Call Content.Add(Adminui.GetEditRow(Copy, "Activity", "", False, False, ""))
//                 '
//                 ' ----- Pages Found
//                 '
//                 Copy = "n/a"
//                 SQL = "SELECT Count(ccEDGPublishDocs.ID) AS PagesFound FROM ccEDGPublishDocs;"
//                 CSPointer = cpCore.db.csOpenSql_rev("Default", SQL)
//                 If cpCore.db.csOk(CSPointer) Then
//                     Copy = genericController.encodeText(cpCore.db.csGetInteger(CSPointer, "PagesFound"))
//                 End If
//                 Call cpCore.db.csClose(CSPointer)
//                 Call Content.Add(Adminui.GetEditRow(Copy, "Links Found", "", False, False, ""))
//                 '
//                 ' ----- Pages Complete
//                 '
//                 Copy = "n/a"
//                 SQL = "SELECT Count(ccEDGPublishDocs.ID) AS PagesFound FROM ccEDGPublishDocs where (UpToDate=1);"
//                 CSPointer = cpCore.db.csOpenSql_rev("Default", SQL)
//                 If cpCore.db.csOk(CSPointer) Then
//                     Copy = genericController.encodeText(cpCore.db.csGetInteger(CSPointer, "PagesFound"))
//                 End If
//                 Call cpCore.db.csClose(CSPointer)
//                 Call Content.Add(Adminui.GetEditRow(Copy, "Pages Complete", "", False, False, ""))
//                 '
//                 ' ----- Bad Links
//                 '
//                 Copy = "n/a"
//                 QueryString = genericController.ModifyQueryString(cpCore.doc.refreshQueryString, RequestNameAdminForm, AdminFormReports, True)
//                 QueryString = genericController.ModifyQueryString(QueryString, RequestNameReportForm, ReportFormEDGDocErrors, True)
//                 SQL = "SELECT Count(ccEDGPublishDocs.ID) AS PagesFound FROM ccEDGPublishDocs where (UpToDate=1) And (LinkAlias Is Not null) And ((HTTPResponse Is null) Or ((Not (HTTPResponse Like '% 200 %'))and (not (HTTPResponse like '% 302 %'))));"
//                 CSPointer = cpCore.db.csOpenSql_rev("Default", SQL)
//                 If cpCore.db.csOk(CSPointer) Then
//                     Copy = genericController.encodeText(cpCore.db.csGetInteger(CSPointer, "PagesFound"))
//                 End If
//                 Call cpCore.db.csClose(CSPointer)
//                 Call Content.Add(Adminui.GetEditRow("<a href=""/" & genericController.encodeHTML(cpCore.serverConfig.appConfig.adminRoute & "?" & QueryString) & """ target=""_blank"">" & SpanClassAdminNormal & Copy & "</a>", "Bad Links", "", False, False, ""))
//                 '
//                 ' ----- Options
//                 '
//                 'Content.Add( "<tr><td align=""right"">" & SpanClassAdminSmall & "Options</span></td>")
//                 If EDGPublishNow Then
//                     '
//                     ' Publishing
//                     '
//                     Call Content.Add(Adminui.GetEditRow(genericController.main_GetYesNo(EDGCreateSnapShot), "Create Staging Snap-Shot", "", False, False, ""))
//                     Call Content.Add(Adminui.GetEditRow(genericController.main_GetYesNo(EDGPublishToProduction), "Publish Snap-Shot to Production", "", False, False, ""))
//                 Else
//                     '
//                     ' Ready
//                     '
//                     Call Content.Add(Adminui.GetEditRow(cpCore.html.html_GetFormInputCheckBox2("EDGCreateSnapShot", EDGCreateSnapShot), "Create Staging Snap-Shot", "", False, False, ""))
//                     Call Content.Add(Adminui.GetEditRow(cpCore.html.html_GetFormInputCheckBox2("EDGPublishToProduction", EDGPublishToProduction), "Publish Snap-Shot to Production", "", False, False, ""))
//                 End If
//                 '
//                 ' Username
//                 '
//                 Call Content.Add(Adminui.GetEditRow(cpCore.html.html_GetFormInputText2("EDGAuthUsername", EDGAuthUsername), "Username", "", False, False, ""))
//                 '
//                 ' Password
//                 '
//                 Call Content.Add(Adminui.GetEditRow(cpCore.html.html_GetFormInputText2("EDGAuthPassword", EDGAuthPassword), "Password", "", False, False, ""))
//                 '
//                 ' Seed Documents
//                 '
//                 Copy = ""
//                 CSPointer = cpCore.db.csOpen("EDG Publish Seeds")
//                 Do While cpCore.db.csOk(CSPointer)
//                     If Copy <> "" Then
//                         Copy = Copy & "<br>"
//                     End If
//                     Copy = Copy & cpCore.db.csGetRecordEditLink(CSPointer) & cpCore.db.csGet(CSPointer, "Name")
//                     cpCore.db.csGoNext(CSPointer)
//                 Loop
//                 Call cpCore.db.csClose(CSPointer)
//                 Copy = Copy & "<br>" & cpCore.html.main_cs_getRecordAddLink(CSPointer)
//                 Call Content.Add(Adminui.GetEditRow(Copy, "Seed URLs", "", False, False, ""))
//                 '
//                 ' Production Servers
//                 '
//                 Copy = ""
//                 CSPointer = cpCore.db.csOpen("EDG Publish Servers")
//                 Do While cpCore.db.csOk(CSPointer)
//                     If Copy <> "" Then
//                         Copy = Copy & "<br>"
//                     End If
//                     Copy = Copy & cpCore.db.csGetRecordEditLink(CSPointer) & cpCore.db.csGet(CSPointer, "Name")
//                     cpCore.db.csGoNext(CSPointer)
//                 Loop
//                 Call cpCore.db.csClose(CSPointer)
//                 'If cpCore.visitProperty_AllowEditing Then
//                 '    If Copy <> "" Then
//                 '        'Copy = Copy & "<br>"
//                 '        End If
//                 Copy = Copy & "<br>" & cpCore.html.main_cs_getRecordAddLink(CSPointer)
//                 '    End If
//                 Call Content.Add(Adminui.GetEditRow(Copy, "Production Servers", "", False, False, ""))
//                 '
//                 ' Buttons
//                 '
//                 If Not EDGPublishNow Then
//                     ButtonList = ButtonBegin
//                 Else
//                     ButtonList = ButtonAbort & "," & ButtonRefresh
//                 End If
//                 '
//                 Content.Add(Adminui.EditTableClose)
//                 Content.Add(cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormEDGControl))
//             End If
//             '
//             Description = "Static Publishing lets you create a completely static version of your website on remote servers. Some dynamic features such as personalization will not work on a static site."
//             GetForm_StaticPublishControl = Adminui.GetBody("Static Publish Control", ButtonList, "", True, True, Description, "", 0, Content.Text)
//             Content = Nothing
//             '
//             Exit Function
//             '
//             ' ----- Error Trap
//             '
// ErrorTrap:
//             Content = Nothing
//             Call handleLegacyClassError3("PrintDHTMLEditForm")
//             '
//         End Function
// 
Endclass End {
}