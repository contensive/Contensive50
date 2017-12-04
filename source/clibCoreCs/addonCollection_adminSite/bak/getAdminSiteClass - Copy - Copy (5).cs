

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
        //  ----- Print the Normal Content Edit form
        // 
        //    Print the content fields and Topic Groups section
        // ========================================================================
        // 
        private string GetForm_Publish() {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter( "GetForm_Publish")
            // 
            string FieldList;
            string ModifiedDateString;
            string SubmittedDateString;
            string ApprovedDateString;
            adminUIController Adminui = new adminUIController(cpCore);
            string ButtonList = "";
            string Caption;
            int CS;
            string SQL;
            string RowColor;
            int RecordCount;
            int RecordLast;
            int RecordNext;
            int RecordPrevious;
            string RecordName;
            string Copy;
            int ContentID;
            string ContentName;
            int RecordID;
            string Link;
            int CSAuthoringRecord;
            string TableName;
            int PageNumber;
            // 
            bool IsInserted;
            bool IsDeleted;
            // 
            bool IsModified;
            string ModifiedName = "";
            DateTime ModifiedDate;
            // 
            bool IsSubmitted;
            string SubmitName = "";
            DateTime SubmittedDate;
            // 
            bool IsApproved;
            string ApprovedName = "";
            DateTime ApprovedDate;
            stringBuilderLegacyController Stream = new stringBuilderLegacyController();
            string Body = "";
            string Description;
            string Button;
            string BR = "";
            Button = cpCore.docProperties.getText(RequestNameButton);
            if ((Button == ButtonCancel)) {
                // 
                // 
                // 
                return cpCore.webServer.redirect(("/" + cpCore.serverConfig.appConfig.adminRoute), "Admin Publish, Cancel Button Pressed");
            }
            else if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                // 
                // 
                // 
                ButtonList = ButtonCancel;
                Adminui.GetFormBodyAdminOnly();
            }
            else {
                // 
                //  ----- Page Body
                // 
                BR = "<BR >";
                (cr + "<table border=\"0\" cellpadding=\"2\" cellspacing=\"2\" width=\"100%\">");
                (cr + "<tr>");
                (cr + ("<td width=\"50\" class=\"ccPanel\" align=\"center\" class=\"ccAdminSmall\">Pub" 
                            + (BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"42\" height=\"1\" ></td>")));
                (cr + ("<td width=\"50\" class=\"ccPanel\" align=\"center\" class=\"ccAdminSmall\">Sub\'d" 
                            + (BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"42\" height=\"1\" ></td>")));
                (cr + ("<td width=\"50\" class=\"ccPanel\" align=\"center\" class=\"ccAdminSmall\">Appr\'d" 
                            + (BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"42\" height=\"1\" ></td>")));
                (cr + ("<td width=\"50\" class=\"ccPanel\" class=\"ccAdminSmall\">Edit" 
                            + (BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"42\" height=\"1\" ></td>")));
                (cr + ("<td width=\"200\" class=\"ccPanel\" class=\"ccAdminSmall\">Name" 
                            + (BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"192\" height=\"1\" ></td>")));
                (cr + ("<td width=\"100\" class=\"ccPanel\" class=\"ccAdminSmall\">Content" 
                            + (BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"92\" height=\"1\" ></td>")));
                (cr + ("<td width=\"50\" class=\"ccPanel\" class=\"ccAdminSmall\">#" 
                            + (BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"92\" height=\"1\" ></td>")));
                (cr + ("<td width=\"100\" class=\"ccPanel\" class=\"ccAdminSmall\">Public" 
                            + (BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"92\" height=\"1\" ></td>")));
                (cr + ("<td width=\"100%\" class=\"ccPanel\" class=\"ccAdminSmall\">Status" 
                            + (BR + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"1\" ></td>")));
                (cr + "</tr>");
                SQL = ("SELECT DISTINCT top 100 ccAuthoringControls.ContentID AS ContentID, ccContent.Name AS ContentName, cc" +
                "AuthoringControls.RecordID, ccContentWatch.Link AS Link, ccContent.AllowWorkflowAuthoring AS Content" +
                "AllowWorkflowAuthoring,min(ccAuthoringControls.ID)" + (" FROM (ccAuthoringControls" + (" LEFT JOIN ccContent ON ccAuthoringControls.ContentID = ccContent.ID)" + (" LEFT JOIN ccContentWatch ON ccAuthoringControls.ContentRecordKey = ccContentWatch.ContentRecordKey" + (" Where (ccAuthoringControls.ControlType > 1)" + (" GROUP BY ccAuthoringControls.ContentID, ccContent.Name, ccAuthoringControls.RecordID, ccContentWatch" +
                ".Link, ccContent.AllowWorkflowAuthoring" + " order by min(ccAuthoringControls.ID) desc"))))));
                CS = cpCore.db.csOpenSql_rev("Default", SQL);
                // CS = cpCore.app_openCsSql_Rev_Internal("Default", SQL, RecordsPerPage, PageNumber)
                RecordCount = 0;
                if (cpCore.db.csOk(CS)) {
                    RowColor = "";
                    RecordLast = (RecordTop + RecordsPerPage);
                    // 
                    //  --- Print out the records
                    // 
                    while ((cpCore.db.csOk(CS) 
                                && (RecordCount < 100))) {
                        ContentID = cpCore.db.csGetInteger(CS, "contentID");
                        ContentName = cpCore.db.csGetText(CS, "contentname");
                        RecordID = cpCore.db.csGetInteger(CS, "recordid");
                        Link = pageContentController.getPageLink(cpCore, RecordID, "", true, false);
                        // Link = cpCore.main_GetPageLink3(RecordID, "", True)
                        // If Link = "" Then
                        //     Link = cpCore.db.cs_getText(CS, "Link")
                        // End If
                        if (((ContentID == 0) 
                                    || ((ContentName == "") 
                                    || (RecordID == 0)))) {
                            // 
                            //  This control is not valid, delete it
                            // 
                            SQL = ("delete from ccAuthoringControls where ContentID=" 
                                        + (ContentID + (" and RecordID=" + RecordID)));
                            cpCore.db.executeQuery(SQL);
                        }
                        else {
                            TableName = Models.Complex.cdefModel.GetContentProperty(cpCore, ContentName, "ContentTableName");
                            if (!cpCore.db.csGetBoolean(CS, "ContentAllowWorkflowAuthoring")) {
                                // 
                                //  Authoring bug -- This record should not be here, the content does not support workflow authoring
                                // 
                                handleLegacyClassError2("GetForm_Publish", ("Admin Workflow Publish selected an authoring control record [" 
                                                + (ContentID + ("." 
                                                + (RecordID + ("] for a content definition [" 
                                                + (ContentName + "] that does not AllowWorkflowAuthoring.")))))));
                                // Call HandleInternalError("GetForm_Publish", "Admin Workflow Publish selected an authoring control record [" & ContentID & "." & RecordID & "] for a content definition [" & ContentName & "] that does not AllowWorkflowAuthoring.")
                            }
                            else {
                                cpCore.doc.getAuthoringStatus(ContentName, RecordID, IsSubmitted, IsApproved, SubmitName, ApprovedName, IsInserted, IsDeleted, IsModified, ModifiedName, ModifiedDate, SubmittedDate, ApprovedDate);
                                if ((RowColor == "class=\"ccPanelRowOdd\"")) {
                                    RowColor = "class=\"ccPanelRowEven\"";
                                }
                                else {
                                    RowColor = "class=\"ccPanelRowOdd\"";
                                }
                                
                                // 
                                //  make sure the record exists
                                // 
                                if ((genericController.vbUCase(TableName) == "CCPAGECONTENT")) {
                                    FieldList = "ID,Name,Headline,MenuHeadline";
                                }
                                else {
                                    FieldList = "ID,Name,Name as Headline,Name as MenuHeadline";
                                }
                                
                                CSAuthoringRecord = cpCore.db.csOpenRecord(ContentName, RecordID, true, true, FieldList);
                                // CSAuthoringRecord = cpCore.app_openCsSql_Rev_Internal("Default", SQL, 1)
                                if (!cpCore.db.csOk(CSAuthoringRecord)) {
                                    // 
                                    //  This authoring control is not valid, delete it
                                    // 
                                    SQL = ("delete from ccAuthoringControls where ContentID=" 
                                                + (ContentID + (" and RecordID=" + RecordID)));
                                    cpCore.db.executeQuery(SQL);
                                }
                                else {
                                    RecordName = cpCore.db.csGet(CSAuthoringRecord, "name");
                                    if ((RecordName == "")) {
                                        RecordName = cpCore.db.csGet(CSAuthoringRecord, "headline");
                                        if ((RecordName == "")) {
                                            RecordName = cpCore.db.csGet(CSAuthoringRecord, "headline");
                                            if ((RecordName == "")) {
                                                RecordName = ("Record " + cpCore.db.csGet(CSAuthoringRecord, "ID"));
                                            }
                                            
                                        }
                                        
                                    }
                                    
                                    if (true) {
                                        if ((Link == "")) {
                                            Link = "unknown";
                                        }
                                        else {
                                            Link = ("<a href=\"" 
                                                        + (genericController.encodeHTML(Link) + ("\" target=\"_blank\">" 
                                                        + (Link + "</a>"))));
                                        }
                                        
                                        // 
                                        //  get approved status of the submitted record
                                        // 
                                        ("\n" + "<tr>");
                                        ("<td align=\"center\" valign=\"top\" " 
                                                    + (RowColor + (">" 
                                                    + (cpCore.html.html_GetFormInputCheckBox2(("row" + RecordCount), false) 
                                                    + (cpCore.html.html_GetFormInputHidden(("rowid" + RecordCount), RecordID) 
                                                    + (cpCore.html.html_GetFormInputHidden(("rowcontentname" + RecordCount), ContentName) + "</td>"))))));
                                        if (IsSubmitted) {
                                            Copy = "yes";
                                        }
                                        else {
                                            Copy = "no";
                                        }
                                        
                                        ("<td align=\"center\" valign=\"top\" " 
                                                    + (RowColor + (" class=\"ccAdminSmall\">" 
                                                    + (Copy + "</td>"))));
                                        if (IsApproved) {
                                            Copy = "yes";
                                        }
                                        else {
                                            Copy = "no";
                                        }
                                        
                                        ("<td align=\"center\" valign=\"top\" " 
                                                    + (RowColor + (" class=\"ccAdminSmall\">" 
                                                    + (Copy + "</td>"))));
                                        Body = (Body + ("<td align=\"left\" valign=\"top\" " 
                                                    + (RowColor + (" class=\"ccAdminSmall\">" + ("<a href=\"?" 
                                                    + (RequestNameAdminForm + ("=" 
                                                    + (AdminFormEdit + ("&cid=" 
                                                    + (ContentID + ("&id=" 
                                                    + (RecordID + ("&" 
                                                    + (RequestNameAdminDepth + ("=1\">Edit</a>" + "</td>")))))))))))))));
                                        ("<td align=\"left\" valign=\"top\" " 
                                                    + (RowColor + (" class=\"ccAdminSmall\"  style=\"white-space:nowrap;\">" 
                                                    + (RecordName + "</td>"))));
                                        ("<td align=\"left\" valign=\"top\" " 
                                                    + (RowColor + (" class=\"ccAdminSmall\">" 
                                                    + (ContentName + "</td>"))));
                                        ("<td align=\"left\" valign=\"top\" " 
                                                    + (RowColor + (" class=\"ccAdminSmall\">" 
                                                    + (RecordID + "</td>"))));
                                        if (IsInserted) {
                                            Link = (Link + "*");
                                        }
                                        else if (IsDeleted) {
                                            Link = (Link + "**");
                                        }
                                        
                                        ("<td align=\"left\" valign=\"top\" " 
                                                    + (RowColor + (" class=\"ccAdminSmall\" style=\"white-space:nowrap;\">" 
                                                    + (Link + "</td>"))));
                                        ("<td align=\"left\" valign=\"top\" " 
                                                    + (RowColor + (">" + SpanClassAdminNormal)));
                                        MinValue;
                                        ModifiedDateString = "unknown";
                                    }
                                    else {
                                        ModifiedDateString = ModifiedDate.ToString();
                                    }
                                    
                                    if ((ModifiedName == "")) {
                                        ModifiedName = "unknown";
                                    }
                                    
                                    if ((SubmitName == "")) {
                                        SubmitName = "unknown";
                                    }
                                    
                                    if ((ApprovedName == "")) {
                                        ApprovedName = "unknown";
                                    }
                                    
                                    if (IsInserted) {
                                        ("Added: " 
                                                    + (ModifiedDateString + (" by " 
                                                    + (ModifiedName + ("" 
                                                    + (BR + ""))))));
                                    }
                                    else if (IsDeleted) {
                                        ("Deleted: " 
                                                    + (ModifiedDateString + (" by " 
                                                    + (ModifiedName + ("" 
                                                    + (BR + ""))))));
                                    }
                                    else {
                                        ("Modified: " 
                                                    + (ModifiedDateString + (" by " 
                                                    + (ModifiedName + ("" 
                                                    + (BR + ""))))));
                                    }
                                    
                                    if (IsSubmitted) {
                                        MinValue;
                                        SubmittedDateString = "unknown";
                                    }
                                    else {
                                        SubmittedDateString = SubmittedDate.ToString();
                                    }
                                    
                                    ("Submitted: " 
                                                + (SubmittedDateString + (" by " 
                                                + (SubmitName + ("" 
                                                + (BR + ""))))));
                                }
                                
                                if (IsApproved) {
                                    MinValue;
                                    ApprovedDateString = "unknown";
                                }
                                else {
                                    ApprovedDateString = ApprovedDate.ToString();
                                }
                                
                                ("Approved: " 
                                            + (ApprovedDate + (" by " 
                                            + (ApprovedName + ("" 
                                            + (BR + ""))))));
                            }
                            
                            // Body &=  ("Admin Site: <a href=""?" & RequestNameAdminForm & "=" & AdminFormEdit & "&cid=" & ContentID & "&id=" & RecordID & "&" & RequestNameAdminDepth & "=1"" target=""_blank"">Open in New Window</a>" & br & "")
                            // Body &=  ("Public Site: " & Link & "" & br & "")
                            // 
                            "</td>";
                            ("\n" + "</tr>");
                            RecordCount = (RecordCount + 1);
                        }
                        
                        cpCore.db.csClose(CSAuthoringRecord);
                        cpCore.db.csGoNext(CS);
                    }
                    
                    // 
                    //  --- print out the stuff at the bottom
                    // 
                    RecordNext = RecordTop;
                    if (cpCore.db.csOk(CS)) {
                        RecordNext = RecordCount;
                    }
                    
                    RecordPrevious = (RecordTop - RecordsPerPage);
                    if ((RecordPrevious < 0)) {
                        RecordPrevious = 0;
                    }
                    
                }
                
                cpCore.db.csClose(CS);
                if ((RecordCount == 0)) {
                    // 
                    //  No records printed
                    // 
                    (cr + "<tr><td width=\"100%\" colspan=\"9\" class=\"ccAdminSmall\" style=\"padding-top:10px;\">There are no modified" +
                    " records to review</td></tr>");
                }
                else {
                    (cr + "<tr><td width=\"100%\" colspan=\"9\" class=\"ccAdminSmall\" style=\"padding-top:10px;\">* To view these recor" +
                    "ds on the public site you must enable Rendering Mode because they are new records that have not been" +
                    " published.</td></tr>");
                    (cr + "<tr><td width=\"100%\" colspan=\"9\" class=\"ccAdminSmall\">** To view these records on the public site you" +
                    " must disable Rendering Mode because they are deleted records that have not been published.</td></tr" +
                    ">");
                }
                
                (cr + "</table>");
                cpCore.html.html_GetFormInputHidden("RowCnt", RecordCount);
                Body = ("<div style=\"Background-color:white;\">" 
                            + (Body + "</div>"));
                ButtonList = "";
                if ((MenuDepth > 0)) {
                    ButtonList = (ButtonList + ("," + ButtonClose));
                }
                else {
                    ButtonList = (ButtonList + ("," + ButtonCancel));
                }
                
                // ButtonList = ButtonList & "," & ButtonWorkflowPublishApproved & "," & ButtonWorkflowPublishSelected
                ButtonList = ButtonList.Substring(1);
                // 
                //  Assemble Page
                // 
                cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormPublishing);
            }
            
            // 
            Caption = (SpanClassAdminNormal + "<strong>Workflow Publishing</strong></span>");
            Description = "Monitor and Approve Workflow Publishing Changes";
            if ((RecordCount >= 100)) {
                Description = (Description 
                            + (BR 
                            + (BR + "Only the first 100 record are displayed")));
            }
            
            GetForm_Publish = Adminui.GetBody(Caption, ButtonList, "", true, true, Description, "", 0, Body);
            cpCore.html.addTitle("Workflow Publishing");
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            handleLegacyClassError3("GetForm_Publish");
            // 
        }
        
        // 
        // ========================================================================
        //    Generate the content of a tab in the Edit Screen
        // ========================================================================
        // 
        private string GetForm_Edit_Tab(
                    Models.Complex.cdefModel adminContent, 
                    editRecordClass editRecord, 
                    int RecordID, 
                    int ContentID, 
                    bool ForceReadOnly, 
                    bool IsLandingPage, 
                    bool IsRootPage, 
                    string EditTab, 
                    csv_contentTypeEnum EditorContext, 
                    ref string return_NewFieldList, 
                    int TemplateIDForStyles, 
                    int HelpCnt, 
                    int[] HelpIDCache, 
                    string[] helpDefaultCache, 
                    string[] HelpCustomCache, 
                    bool AllowHelpMsgCustom, 
                    keyPtrController helpIdIndex, 
                    string[] fieldTypeDefaultEditors, 
                    string fieldEditorPreferenceList, 
                    string styleList, 
                    string styleOptionList, 
                    int emailIdForStyles, 
                    bool IsTemplateTable, 
                    string editorAddonListJSON) {
            string returnHtml = "";
            try {
                // 
                string AjaxQS;
                string fancyBoxLinkId;
                string fancyBoxContentId;
                int fieldTypeDefaultEditorAddonId;
                int fieldIdPos;
                int Pos;
                int editorAddonID;
                bool editorReadOnly;
                string addonOptionString = String.Empty;
                bool AllowHelpIcon;
                int fieldId;
                bool FieldHelpFound;
                string LcaseName;
                bool IsEmptyList;
                string HelpMsgCustom;
                string HelpMsgDefault;
                // 
                DateTime FieldValueDate;
                string WhyReadOnlyMsg;
                bool IsLongHelp;
                bool IsEmptyHelp;
                string HelpMsg;
                int CS;
                string EditorStyleModifier;
                string HelpClosedContentID;
                bool AllowHelpRow;
                string EditorRightSideIcon;
                string EditorHelp;
                string HelpEditorID;
                string HelpOpenedReadID;
                string HelpOpenedEditID;
                string HelpClosedID;
                string HelpID;
                string HelpMsgClosed;
                string HelpMsgOpenedRead;
                string HelpMsgOpenedEdit;
                bool NewWay;
                string RecordName;
                string GroupName;
                string SelectMessage;
                bool IsBaseField;
                bool FieldReadOnly;
                string NonEncodedLink;
                string EncodedLink;
                string Caption;
                string[] lookups;
                int CSPointer;
                string FieldName;
                string FieldValueText;
                int FieldValueInteger;
                double FieldValueNumber;
                bool FieldValueBoolean;
                int fieldTypeId;
                object FieldValueObject;
                bool FieldPreferenceHTML;
                int CSLookup;
                string RedirectPath;
                string LookupContentName;
                stringBuilderLegacyController s = new stringBuilderLegacyController();
                bool RecordReadOnly;
                string MethodName;
                string FormFieldLCaseName;
                int FieldRows;
                string EditorString;
                string FieldOptionRow;
                string MTMContent0;
                string MTMContent1;
                string MTMRuleContent;
                string MTMRuleField0;
                string MTMRuleField1;
                string AlphaSort;
                adminUIController Adminui = new adminUIController(cpCore);
                bool needUniqueEmailMessage;
                // 
                needUniqueEmailMessage = false;
                returnHtml = "";
                MethodName = "AdminClass.GetFormEdit_UserFields";
                NewWay = true;
                if ((adminContent.fields.Count <= 0)) {
                    // 
                    //  There are no visible fiels, return empty
                    // 
                    cpCore.handleException(new ApplicationException("The content definition for this record is invalid. It contains no valid fields."));
                }
                else {
                    RecordReadOnly = ForceReadOnly;
                    // 
                    //  ----- Build an index to sort the fields by EditSortOrder
                    // 
                    Dictionary<string, Models.Complex.CDefFieldModel> sortingFields = new Dictionary<string, Models.Complex.CDefFieldModel>();
                    foreach (keyValuePair in adminContent.fields) {
                        Models.Complex.CDefFieldModel field = keyValuePair.Value;
                        // With...
                        if (field.editTabName.ToLower) {
                            EditTab.ToLower();
                            if (IsVisibleUserField(field.adminOnly, field.developerOnly, field.active, field.authorable, field.nameLc, adminContent.ContentTableName)) {
                                AlphaSort = (genericController.GetIntegerString(field.editSortPriority, 10) + ("-" + genericController.GetIntegerString(field.id, 10)));
                                sortingFields.Add(AlphaSort, field);
                            }
                            
                        }
                        
                    }
                    
                    // 
                    //  ----- display the record fields
                    // 
                    AllowHelpIcon = cpCore.visitProperty.getBoolean("AllowHelpIcon");
                    foreach (kvp in sortingFields) {
                        Models.Complex.CDefFieldModel field = kvp.Value;
                        // With...
                        fieldId = field.id;
                        WhyReadOnlyMsg = "";
                        FieldName = field.nameLc;
                        FormFieldLCaseName = genericController.vbLCase(FieldName);
                        fieldTypeId = field.fieldTypeId;
                        FieldValueObject = editRecord.fieldsLc(field.nameLc).value;
                        FieldValueText = genericController.encodeText(FieldValueObject);
                        FieldRows = 1;
                        FieldOptionRow = " ";
                        FieldPreferenceHTML = field.htmlContent;
                        // 
                        Caption = field.caption;
                        if (field.UniqueName) {
                            Caption = (" **" + Caption);
                        }
                        else if ((field.nameLc.ToLower() == "email")) {
                            if (((adminContent.ContentTableName.ToLower() == "ccmembers") 
                                        && cpCore.siteProperties.getBoolean("allowemaillogin", false))) {
                                Caption = (" ***" + Caption);
                                needUniqueEmailMessage = true;
                            }
                            
                        }
                        
                        if (field.Required) {
                            Caption = (" *" + Caption);
                        }
                        
                        IsBaseField = field.blockAccess;
                        //  field renamed
                        FormInputCount = (FormInputCount + 1);
                        FieldReadOnly = false;
                        if (IsLandingPage) {
                            switch (genericController.vbLCase(field.nameLc)) {
                                case "active":
                                    FieldReadOnly = genericController.EncodeBoolean(FieldValueObject);
                                    if (FieldReadOnly) {
                                        WhyReadOnlyMsg = " (disabled because you can not mark the landing page inactive)";
                                    }
                                    
                                    break;
                                case "dateexpires":
                                case "pubdate":
                                case "datearchive":
                                case "blocksection":
                                case "hidemenu":
                                    FieldReadOnly = true;
                                    WhyReadOnlyMsg = " (disabled for the landing page)";
                                    break;
                            }
                        }
                        
                        // 
                        if (IsRootPage) {
                            switch (genericController.vbLCase(field.nameLc)) {
                                case "dateexpires":
                                case "pubdate":
                                case "datearchive":
                                case "archiveparentid":
                                    FieldReadOnly = true;
                                    WhyReadOnlyMsg = " (disabled for root pages)";
                                    break;
                                case "allowinmenus":
                                case "allowinchildlists":
                                    FieldValueBoolean = true;
                                    FieldValueObject = "1";
                                    FieldReadOnly = true;
                                    WhyReadOnlyMsg = " (disabled for root pages)";
                                    break;
                            }
                        }
                        
                        // 
                        //  Special Case - ccemail table Alloweid should be disabled if siteproperty AllowLinkLogin is false
                        // 
                        if (((genericController.vbLCase(adminContent.ContentTableName) == "ccemail") 
                                    && (genericController.vbLCase(FieldName) == "allowlinkeid"))) {
                            if (!cpCore.siteProperties.getBoolean("AllowLinkLogin", true)) {
                                // .ValueVariant = "0"
                                FieldValueObject = "0";
                                FieldReadOnly = true;
                                FieldValueBoolean = false;
                                FieldValueText = "0";
                            }
                            
                        }
                        
                        EditorStyleModifier = genericController.vbLCase(cpCore.db.getFieldTypeNameFromFieldTypeId(fieldTypeId));
                        EditorString = "";
                        editorReadOnly = (RecordReadOnly 
                                    || (field.ReadOnly 
                                    || (((editRecord.id != 0) 
                                    && field.NotEditable) 
                                    || FieldReadOnly)));
                        editorAddonID = 0;
                        // editorPreferenceAddonId = 0
                        fieldIdPos = genericController.vbInstr(1, ("," + fieldEditorPreferenceList), ("," 
                                        + (fieldId.ToString() + ":")));
                        while (((editorAddonID == 0) 
                                    && (fieldIdPos > 0))) {
                            fieldIdPos = (fieldIdPos + (1 + fieldId.ToString().Length));
                            Pos = genericController.vbInstr(fieldIdPos, (fieldEditorPreferenceList + ","), ",");
                            if ((Pos > 0)) {
                                editorAddonID = genericController.EncodeInteger(fieldEditorPreferenceList.Substring((fieldIdPos - 1), (Pos - fieldIdPos)));
                                // editorPreferenceAddonId = genericController.EncodeInteger(Mid(fieldEditorPreferenceList, fieldIdPos, Pos - fieldIdPos))
                                // editorAddonID = editorPreferenceAddonId
                            }
                            
                            fieldIdPos = genericController.vbInstr((fieldIdPos + 1), ("," + fieldEditorPreferenceList), ("," 
                                            + (fieldId.ToString() + ":")));
                        }
                        
                        if ((editorAddonID == 0)) {
                            fieldTypeDefaultEditorAddonId = genericController.EncodeInteger(fieldTypeDefaultEditors(fieldTypeId));
                            editorAddonID = fieldTypeDefaultEditorAddonId;
                        }
                        
                        bool useEditorAddon;
                        useEditorAddon = false;
                        if ((editorAddonID != 0)) {
                            // 
                            // --------------------------------------------------------------------------------------------
                            //  ----- Custom Editor
                            // --------------------------------------------------------------------------------------------
                            // 
                            //  generate the style list on demand
                            //  note: &editorFieldType should be deprecated
                            // 
                            cpCore.docProperties.setProperty("editorName", FormFieldLCaseName);
                            cpCore.docProperties.setProperty("editorValue", FieldValueText);
                            cpCore.docProperties.setProperty("editorFieldId", fieldId);
                            cpCore.docProperties.setProperty("editorFieldType", fieldTypeId);
                            cpCore.docProperties.setProperty("editorReadOnly", editorReadOnly);
                            cpCore.docProperties.setProperty("editorWidth", "");
                            cpCore.docProperties.setProperty("editorHeight", "");
                            // addonOptionString = "" _
                            //     & "editorName=" & genericController.encodeNvaArgument(FormFieldLCaseName) _
                            //     & "&editorValue=" & genericController.encodeNvaArgument(FieldValueText) _
                            //     & "&editorFieldId=" & fieldId _
                            //     & "&editorFieldType=" & fieldTypeId _
                            //     & "&editorReadOnly=" & editorReadOnly _
                            //     & "&editorWidth=" _
                            //     & "&editorHeight=" _
                            //     & ""
                            if (genericController.EncodeBoolean(((fieldTypeId == FieldTypeIdHTML) 
                                            || (fieldTypeId == FieldTypeIdFileHTML)))) {
                                // 
                                //  include html related arguments
                                // 
                                cpCore.docProperties.setProperty("editorAllowActiveContent", "1");
                                cpCore.docProperties.setProperty("editorAddonList", editorAddonListJSON);
                                cpCore.docProperties.setProperty("editorStyles", styleList);
                                cpCore.docProperties.setProperty("editorStyleOptions", styleOptionList);
                                // '                            ac = New innovaEditorAddonClassFPO
                                // '                            Call ac.Init()
                                // '                            editorAddonListJSON = ac.GetEditorAddonListJSON(IsTemplateTable, EditorContext)
                                // addonOptionString = addonOptionString _
                                //     & "&editorAllowActiveContent=1" _
                                //     & "&editorAddonList=" & genericController.encodeNvaArgument(editorAddonListJSON) _
                                //     & "&editorStyles=" & genericController.encodeNvaArgument(styleList) _
                                //     & "&editorStyleOptions=" & genericController.encodeNvaArgument(styleOptionList) _
                                //     & ""
                            }
                            
                            EditorString = cpCore.addon.execute(addonModel.create(cpCore, editorAddonID), new BaseClasses.CPUtilsBaseClass.addonExecuteContext(), With, {, .addonType=BaseClasses.CPUtilsBaseClass.addonContext.ContextEditor, .errorCaption=field editor id:&editorAddonID);
                            // EditorString = cpCore.addon.execute_legacy6(editorAddonID, "", addonOptionString, Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextEditor, "", 0, "", "", False, 0, "", False, Nothing, "", Nothing, "", 0, False)
                            useEditorAddon = !string.IsNullOrEmpty(EditorString);
                            if (useEditorAddon) {
                                // 
                                //  -- editor worked
                                return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                            }
                            else {
                                // 
                                //  -- editor failed, determine if it is missing (or inactive). If missing, remove it from the members preferences
                                string SQL = ("select id from ccaggregatefunctions where id=" + editorAddonID);
                                CS = cpCore.db.csOpenSql(SQL);
                                if (!cpCore.db.csOk(CS)) {
                                    // 
                                    //  -- missing, not just inactive
                                    EditorString = "";
                                    string tmpList = cpCore.userProperty.getText(("editorPreferencesForContent:" + adminContent.Id), "");
                                    int PosStart = genericController.vbInstr(1, ("," + tmpList), ("," 
                                                    + (fieldId + ":")));
                                    if ((PosStart > 0)) {
                                        int PosEnd = genericController.vbInstr((PosStart + 1), ("," + tmpList), ",");
                                        if ((PosEnd == 0)) {
                                            tmpList = tmpList.Substring(0, (PosStart - 1));
                                        }
                                        else {
                                            tmpList = (tmpList.Substring(0, (PosStart - 1)) + tmpList.Substring((PosEnd - 1)));
                                        }
                                        
                                        cpCore.userProperty.setProperty(("editorPreferencesForContent:" + adminContent.Id), tmpList);
                                    }
                                    
                                }
                                
                                cpCore.db.csClose(CS);
                            }
                            
                        }
                        
                        if (!useEditorAddon) {
                            // 
                            //  if custom editor not used or if it failed
                            // 
                            if ((fieldTypeId == FieldTypeIdRedirect)) {
                                // ElseIf (FieldType = FieldTypeRedirect) Then
                                // 
                                // --------------------------------------------------------------------------------------------
                                //  ----- Default Editor, Redirect fields (the same for normal/readonly/spelling)
                                // --------------------------------------------------------------------------------------------
                                // 
                                RedirectPath = cpCore.serverConfig.appConfig.adminRoute;
                                if ((field.RedirectPath != "")) {
                                    RedirectPath = field.RedirectPath;
                                }
                                
                                RedirectPath = (RedirectPath + ("?" 
                                            + (RequestNameTitleExtension + ("=" 
                                            + (genericController.EncodeRequestVariable((" For " 
                                                + (editRecord.nameLc + TitleExtension))) + ("&" 
                                            + (RequestNameAdminDepth + ("=" 
                                            + ((MenuDepth + 1) + ("&wl0=" 
                                            + (field.RedirectID + ("&wr0=" + editRecord.id))))))))))));
                                if ((field.RedirectContentID != 0)) {
                                    RedirectPath = (RedirectPath + ("&cid=" + field.RedirectContentID));
                                }
                                else {
                                    RedirectPath = (RedirectPath + ("&cid=" + editRecord.contentControlId));
                                }
                                
                                if ((editRecord.id == 0)) {
                                    "[available after save]";
                                }
                                else {
                                    RedirectPath = genericController.vbReplace(RedirectPath, "\'", "\\\'");
                                    "<a href=\"#\"";
                                    (" onclick=\"" + (" window.open(\'" 
                                                + (RedirectPath + ("\', \'_blank\', \'scrollbars=yes,toolbar=no,status=no,resizable=yes\');" + " return false;\""))));
                                    ">";
                                    "Open in New Window</A>";
                                }
                                
                                // s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal & EditorString & " </span></nobr></td>")
                            }
                            else if (editorReadOnly) {
                                // 
                                // --------------------------------------------------------------------------------------------
                                //  ----- Display fields as read only
                                // --------------------------------------------------------------------------------------------
                                // 
                                if ((WhyReadOnlyMsg != "")) {
                                    WhyReadOnlyMsg = ("<span class=\"ccDisabledReason\">" 
                                                + (WhyReadOnlyMsg + "</span>"));
                                }
                                
                                EditorStyleModifier = "";
                                switch (fieldTypeId) {
                                    case FieldTypeIdBoolean:
                                        // 
                                        //  ----- Boolean ReadOnly
                                        // 
                                        return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                                        FieldValueBoolean = genericController.EncodeBoolean(FieldValueObject);
                                        cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, genericController.encodeText(FieldValueBoolean));
                                        cpCore.html.html_GetFormInputCheckBox2(FormFieldLCaseName, FieldValueBoolean, ,, true, "checkBox");
                                        WhyReadOnlyMsg;
                                        // 
                                        break;
                                    case FieldTypeIdFile:
                                    case FieldTypeIdFileImage:
                                        // 
                                        //  ----- File ReadOnly
                                        // 
                                        return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        NonEncodedLink = (cpCore.webServer.requestDomain + genericController.getCdnFileLink(cpCore, FieldValueText));
                                        EncodedLink = genericController.EncodeURL(NonEncodedLink);
                                        cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, "");
                                        if ((FieldValueText == "")) {
                                            "[no file]";
                                        }
                                        else {
                                            string filename = "";
                                            string path = "";
                                            cpCore.cdnFiles.splitPathFilename(FieldValueText, path, filename);
                                            (" <a href=\"http://" 
                                                        + (EncodedLink + ("\" target=\"_blank\">" 
                                                        + (SpanClassAdminSmall + ("[" 
                                                        + (filename + "]</A>"))))));
                                        }
                                        
                                        WhyReadOnlyMsg;
                                        // 
                                        break;
                                    case FieldTypeIdLookup:
                                        // 
                                        //  ----- Lookup ReadOnly
                                        // 
                                        return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                                        FieldValueInteger = genericController.EncodeInteger(FieldValueObject);
                                        cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, genericController.encodeText(FieldValueInteger));
                                        LookupContentName = "";
                                        if ((field.lookupContentID != 0)) {
                                            LookupContentName = genericController.encodeText(Models.Complex.cdefModel.getContentNameByID(cpCore, field.lookupContentID));
                                        }
                                        
                                        if ((LookupContentName != "")) {
                                            CSLookup = cpCore.db.cs_open2(LookupContentName, FieldValueInteger, false, ,, "Name,ContentControlID");
                                            if (cpCore.db.csOk(CSLookup)) {
                                                if ((cpCore.db.csGet(CSLookup, "Name") == "")) {
                                                    "No Name";
                                                }
                                                else {
                                                    cpCore.html.main_encodeHTML(cpCore.db.csGet(CSLookup, "Name"));
                                                }
                                                
                                                (" [<a TabIndex=-1 href=\"?" 
                                                            + (RequestNameAdminForm + ("=4&cid=" 
                                                            + (field.lookupContentID + ("&id=" 
                                                            + (FieldValueObject.ToString + "\" target=\"_blank\">View details in new window</a>]"))))));
                                            }
                                            else {
                                                "None";
                                            }
                                            
                                            cpCore.db.csClose(CSLookup);
                                            (" [<a TabIndex=-1 href=\"?cid=" 
                                                        + (field.lookupContentID + ("\" target=\"_blank\">See all " 
                                                        + (LookupContentName + "</a>]"))));
                                        }
                                        else if ((field.lookupList != "")) {
                                            lookups = field.lookupList.Split(",");
                                            if ((FieldValueInteger < 1)) {
                                                "None";
                                            }
                                            else if ((FieldValueInteger 
                                                        > (UBound(lookups) + 1))) {
                                                "None";
                                            }
                                            else {
                                                lookups[(FieldValueInteger - 1)];
                                            }
                                            
                                        }
                                        
                                        WhyReadOnlyMsg;
                                        // 
                                        break;
                                    case FieldTypeIdMemberSelect:
                                        // 
                                        //  ----- Member Select ReadOnly
                                        // 
                                        return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                                        FieldValueInteger = genericController.EncodeInteger(FieldValueObject);
                                        cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, genericController.encodeText(FieldValueInteger));
                                        if ((FieldValueInteger == 0)) {
                                            "None";
                                        }
                                        else {
                                            RecordName = cpCore.db.getRecordName("people", FieldValueInteger);
                                            if ((RecordName == "")) {
                                                "No Name";
                                            }
                                            else {
                                                cpCore.html.main_encodeHTML(RecordName);
                                            }
                                            
                                            SelectMessage = "Select from Administrators";
                                            (" [<a TabIndex=-1 href=\"?af=4&cid=" 
                                                        + (Models.Complex.cdefModel.getContentId(cpCore, "people") + ("&id=" 
                                                        + (FieldValueObject.ToString + "\" target=\"_blank\">View details in new window</a>]"))));
                                        }
                                        
                                        WhyReadOnlyMsg;
                                        // 
                                        break;
                                    case FieldTypeIdManyToMany:
                                        // 
                                        //    Placeholder
                                        // 
                                        FieldValueText = genericController.encodeText(FieldValueObject);
                                        MTMContent0 = Models.Complex.cdefModel.getContentNameByID(cpCore, field.contentId);
                                        MTMContent1 = Models.Complex.cdefModel.getContentNameByID(cpCore, field.manyToManyContentID);
                                        MTMRuleContent = Models.Complex.cdefModel.getContentNameByID(cpCore, field.manyToManyRuleContentID);
                                        MTMRuleField0 = field.ManyToManyRulePrimaryField;
                                        MTMRuleField1 = field.ManyToManyRuleSecondaryField;
                                        cpCore.html.getCheckList(("ManyToMany" + field.id), MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1);
                                        // EditorString &= (cpCore.html.getInputCheckListCategories("ManyToMany" & .id, MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, , , True, MTMContent1))
                                        WhyReadOnlyMsg;
                                        // 
                                        break;
                                    case FieldTypeIdCurrency:
                                        // 
                                        //  ----- Currency ReadOnly
                                        // 
                                        return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                                        FieldValueNumber = genericController.EncodeNumber(FieldValueObject);
                                        cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, genericController.encodeText(FieldValueNumber));
                                        cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueNumber.ToString(), ,, ,, true, "text");
                                        FormatCurrency(FieldValueNumber);
                                        WhyReadOnlyMsg;
                                        // 
                                        break;
                                    case FieldTypeIdDate:
                                        // 
                                        //  ----- date
                                        // 
                                        return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                                        FieldValueDate = genericController.encodeDateMinValue(genericController.EncodeDate(FieldValueObject));
                                        field.MinValue;
                                        FieldValueText = "";
                                        break;
                                    default:
                                        FieldValueText = FieldValueDate.ToString();
                                        break;
                                }
                            }
                            
                            cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, FieldValueText);
                            cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, ,, ,, true, "date");
                            WhyReadOnlyMsg;
                            // 
                            FieldTypeIdAutoIdIncrement;
                            FieldTypeIdFloat;
                            FieldTypeIdInteger;
                            // 
                            //  ----- number
                            // 
                            return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                            FieldValueText = genericController.encodeText(FieldValueObject);
                            cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, FieldValueText);
                            cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, ,, ,, true, "number");
                            WhyReadOnlyMsg;
                            // 
                            FieldTypeIdHTML;
                            FieldTypeIdFileHTML;
                            // 
                            //  ----- HTML types
                            // 
                            if (field.htmlContent) {
                                // 
                                //  edit html as html (see the code)
                                // 
                                return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                                FieldValueText = genericController.encodeText(FieldValueObject);
                                cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, FieldValueText);
                                EditorStyleModifier = "textexpandable";
                                FieldRows = cpCore.userProperty.getInteger((adminContent.Name + ("." 
                                                + (FieldName + ".RowHeight"))), 10);
                                cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, FieldRows, ,, FormFieldLCaseName, false, true);
                            }
                            else {
                                // 
                                //  edit html as wysiwyg
                                // 
                                return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                                FieldValueText = genericController.encodeText(FieldValueObject);
                                cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, FieldValueText);
                                // 
                                EditorStyleModifier = "text";
                                FieldRows = cpCore.userProperty.getInteger((adminContent.Name + ("." 
                                                + (FieldName + ".PixelHeight"))), 500);
                                cpCore.html.getFormInputHTML(FormFieldLCaseName, FieldValueText, "500", ,, true, true, editorAddonListJSON, styleList, styleOptionList);
                                // innovaEditor = New innovaEditorAddonClassFPO
                                // EditorString &=  innovaEditor.getInnovaEditor( FormFieldLCaseName, EditorContext, FieldValueText, "", "", True, True, TemplateIDForStyles, emailIdForStyles)
                                EditorString = ("<div style=\"width:95%\">" 
                                            + (EditorString + "</div>"));
                                FieldOptionRow = " ";
                            }
                            
                            FieldTypeIdText;
                            FieldTypeIdLink;
                            FieldTypeIdResourceLink;
                            // 
                            //  ----- FieldTypeText
                            // 
                            return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                            FieldValueText = genericController.encodeText(FieldValueObject);
                            cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, FieldValueText);
                            if (field.Password) {
                                // 
                                //  Password forces simple text box
                                // 
                                cpCore.html.html_GetFormInputText2(FormFieldLCaseName, "*****", ,, ,, true, true, "password");
                            }
                            else {
                                // 
                                //  non-password
                                // 
                                cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, ,, ,, true, "text");
                            }
                            
                            FieldTypeIdLongText;
                            FieldTypeIdFileText;
                            // 
                            //  ----- LongText, TextFile
                            // 
                            return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                            FieldValueText = genericController.encodeText(FieldValueObject);
                            cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, FieldValueText);
                            EditorStyleModifier = "textexpandable";
                            FieldRows = cpCore.userProperty.getInteger((adminContent.Name + ("." 
                                            + (FieldName + ".RowHeight"))), 10);
                            cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, FieldRows, ,, FormFieldLCaseName, false, true);
                        }
                        else {
                            // 
                            //  ----- Legacy text type -- not used unless something was missed
                            // 
                            return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                            FieldValueText = genericController.encodeText(FieldValueObject);
                            cpCore.html.html_GetFormInputHidden(FormFieldLCaseName, FieldValueText);
                            if (field.Password) {
                                // 
                                //  Password forces simple text box
                                // 
                                cpCore.html.html_GetFormInputText2(FormFieldLCaseName, "*****", ,, ,, true, true, "password");
                            }
                            else if (!field.htmlContent) {
                                // 
                                //  not HTML capable, textarea with resizing
                                // 
                                if (((fieldTypeId == FieldTypeIdText) 
                                            && (((FieldValueText.IndexOf("\n", 0) + 1) 
                                            == 0) 
                                            && (FieldValueText.Length < 40)))) {
                                    // 
                                    //  text field shorter then 40 characters without a CR
                                    // 
                                    cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, ,, ,, true, "text");
                                }
                                else {
                                    // 
                                    //  longer text data, or text that contains a CR
                                    // 
                                    EditorStyleModifier = "textexpandable";
                                    cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, 10, ,, ,, true);
                                }
                                
                            }
                            else if ((field.htmlContent && FieldPreferenceHTML)) {
                                // 
                                //  HTMLContent true, and prefered
                                // 
                                EditorStyleModifier = "text";
                                FieldRows = cpCore.userProperty.getInteger((adminContent.Name + ("." 
                                                + (FieldName + ".PixelHeight"))), 500);
                                cpCore.html.getFormInputHTML(FormFieldLCaseName, FieldValueText, "500", ,, false, true, editorAddonListJSON, styleList, styleOptionList);
                                // innovaEditor = New innovaEditorAddonClassFPO
                                // EditorString &=  innovaEditor.getInnovaEditor( FormFieldLCaseName, EditorContext, FieldValueText, "", "", True, True, TemplateIDForStyles, emailIdForStyles)
                                EditorString = ("<div style=\"width:95%\">" 
                                            + (EditorString + "</div>"));
                                FieldOptionRow = " ";
                            }
                            else {
                                // 
                                //  HTMLContent true, but text editor selected
                                // 
                                EditorStyleModifier = "textexpandable";
                                FieldRows = cpCore.userProperty.getInteger((adminContent.Name + ("." 
                                                + (FieldName + ".RowHeight"))), 10);
                                cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, FieldRows, ,, FormFieldLCaseName, false, true);
                                // EditorString = cpCore.main_GetFormInputTextExpandable(FormFieldLCaseName, encodeHTML(FieldValueText), FieldRows, "600px", FormFieldLCaseName, False)
                            }
                            
                        }
                        
                        // 
                        // --------------------------------------------------------------------------------------------
                        //    Not Read Only - Display fields as form elements to be modified
                        // --------------------------------------------------------------------------------------------
                        // 
                        switch (fieldTypeId) {
                            case FieldTypeIdBoolean:
                                // 
                                //  ----- Boolean
                                // 
                                return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                                FieldValueBoolean = genericController.EncodeBoolean(FieldValueObject);
                                // s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal)
                                cpCore.html.html_GetFormInputCheckBox2(FormFieldLCaseName, FieldValueBoolean, ,, "checkBox");
                                break;
                            case FieldTypeIdFile:
                            case FieldTypeIdFileImage:
                                // 
                                //  ----- File
                                // 
                                return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                                FieldValueText = genericController.encodeText(FieldValueObject);
                                // Call s.Add("<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal)
                                if ((FieldValueText == "")) {
                                    cpCore.html.html_GetFormInputFile2(FormFieldLCaseName, ,, "file");
                                }
                                else {
                                    NonEncodedLink = (cpCore.webServer.requestDomain + genericController.getCdnFileLink(cpCore, FieldValueText));
                                    EncodedLink = genericController.encodeHTML(NonEncodedLink);
                                    string filename = "";
                                    string path = "";
                                    cpCore.cdnFiles.splitPathFilename(FieldValueText, path, filename);
                                    (" <a href=\"http://" 
                                                + (EncodedLink + ("\" target=\"_blank\">" 
                                                + (SpanClassAdminSmall + ("[" 
                                                + (filename + "]</A>"))))));
                                    ("   Delete: " + cpCore.html.html_GetFormInputCheckBox2((FormFieldLCaseName + ".DeleteFlag"), false));
                                    ("   Change: " + cpCore.html.html_GetFormInputFile2(FormFieldLCaseName, ,, "file"));
                                }
                                
                                // 
                                break;
                            case FieldTypeIdLookup:
                                // 
                                //  ----- Lookup
                                // 
                                FieldValueInteger = genericController.EncodeInteger(FieldValueObject);
                                LookupContentName = "";
                                if ((field.lookupContentID != 0)) {
                                    LookupContentName = genericController.encodeText(Models.Complex.cdefModel.getContentNameByID(cpCore, field.lookupContentID));
                                }
                                
                                if ((LookupContentName != "")) {
                                    return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                                    if (!field.Required) {
                                        cpCore.html.main_GetFormInputSelect2(FormFieldLCaseName, FieldValueInteger, LookupContentName, "", "None", "", IsEmptyList, "select");
                                    }
                                    else {
                                        cpCore.html.main_GetFormInputSelect2(FormFieldLCaseName, FieldValueInteger, LookupContentName, "", "", "", IsEmptyList, "select");
                                    }
                                    
                                    if ((FieldValueInteger != 0)) {
                                        CSPointer = cpCore.db.cs_open2(LookupContentName, FieldValueInteger, ,, "ID");
                                        if (cpCore.db.csOk(CSPointer)) {
                                            (" [<a TabIndex=-1 href=\"?" 
                                                        + (RequestNameAdminForm + ("=4&cid=" 
                                                        + (field.lookupContentID + ("&id=" 
                                                        + (FieldValueObject.ToString + "\" target=\"_blank\">Details</a>]"))))));
                                        }
                                        
                                        cpCore.db.csClose(CSPointer);
                                    }
                                    
                                    (" [<a TabIndex=-1 href=\"?cid=" 
                                                + (field.lookupContentID + ("\" target=\"_blank\">See all " 
                                                + (LookupContentName + "</a>]"))));
                                }
                                else if ((field.lookupList != "")) {
                                    return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                                    if (!field.Required) {
                                        cpCore.html.getInputSelectList2(FormFieldLCaseName, FieldValueInteger, field.lookupList, "Select One", "", "select");
                                    }
                                    else {
                                        cpCore.html.getInputSelectList2(FormFieldLCaseName, FieldValueInteger, field.lookupList, "", "", "select");
                                    }
                                    
                                }
                                else {
                                    // 
                                    //  -- log exception but dont throw
                                    cpCore.handleException(new ApplicationException(("Field [" 
                                                        + (FieldName + "] is a Lookup field, but no LookupContent or LookupList has been configured"))));
                                    "[Selection not configured]";
                                }
                                
                                // 
                                break;
                            case FieldTypeIdMemberSelect:
                                // 
                                //  ----- Member Select
                                // 
                                return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                                FieldValueInteger = genericController.EncodeInteger(FieldValueObject);
                                // s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal)
                                if (!field.Required) {
                                    cpCore.html.getInputMemberSelect(FormFieldLCaseName, FieldValueInteger, field.MemberSelectGroupID, "", "None", "select");
                                }
                                else {
                                    cpCore.html.getInputMemberSelect(FormFieldLCaseName, FieldValueInteger, field.MemberSelectGroupID, "", "", "select");
                                }
                                
                                if ((FieldValueInteger != 0)) {
                                    CSPointer = cpCore.db.cs_open2("people", FieldValueInteger, ,, "ID");
                                    if (cpCore.db.csOk(CSPointer)) {
                                        (" [<a TabIndex=-1 href=\"?" 
                                                    + (RequestNameAdminForm + ("=4&cid=" 
                                                    + (Models.Complex.cdefModel.getContentId(cpCore, "people") + ("&id=" 
                                                    + (FieldValueObject.ToString + "\" target=\"_blank\">Details</a>]"))))));
                                    }
                                    
                                    cpCore.db.csClose(CSPointer);
                                }
                                
                                GroupName = cpCore.db.getRecordName("groups", field.MemberSelectGroupID);
                                (" [<a TabIndex=-1 href=\"?cid=" 
                                            + (Models.Complex.cdefModel.getContentId(cpCore, "groups") + ("\" target=\"_blank\">Select from members of " 
                                            + (GroupName + "</a>]"))));
                                break;
                            case FieldTypeIdManyToMany:
                                // 
                                //    Placeholder
                                // 
                                FieldValueText = genericController.encodeText(FieldValueObject);
                                // Call s.Add("<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal)
                                // 
                                MTMContent0 = Models.Complex.cdefModel.getContentNameByID(cpCore, field.contentId);
                                MTMContent1 = Models.Complex.cdefModel.getContentNameByID(cpCore, field.manyToManyContentID);
                                MTMRuleContent = Models.Complex.cdefModel.getContentNameByID(cpCore, field.manyToManyRuleContentID);
                                MTMRuleField0 = field.ManyToManyRulePrimaryField;
                                MTMRuleField1 = field.ManyToManyRuleSecondaryField;
                                cpCore.html.getCheckList(("ManyToMany" + field.id), MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, ,, false, false, FieldValueText);
                                // EditorString &= (cpCore.html.getInputCheckListCategories("ManyToMany" & .id, MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, , , False, MTMContent1, FieldValueText))
                                break;
                            case FieldTypeIdDate:
                                // 
                                //  ----- Date
                                // 
                                return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                                FieldValueDate = genericController.encodeDateMinValue(genericController.EncodeDate(FieldValueObject));
                                field.MinValue;
                                FieldValueText = "";
                                break;
                            default:
                                FieldValueText = FieldValueDate.ToString();
                                break;
                        }
                        cpCore.html.html_GetFormInputDate(FormFieldLCaseName, FieldValueText);
                        FieldTypeIdAutoIdIncrement;
                        FieldTypeIdCurrency;
                        FieldTypeIdFloat;
                        FieldTypeIdInteger;
                        // 
                        //  ----- Others that simply print
                        // 
                        return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                        FieldValueText = genericController.encodeText(FieldValueObject);
                        // s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal)
                        Password;
                        cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, ,, ,, true, false, "password");
                        if ((FieldValueText == "")) {
                            cpCore.html.html_GetFormInputText2(FormFieldLCaseName, ,, ,, ,, "text");
                        }
                        else if ((bool.Parse((FieldValueText.IndexOf("\n", 0) + 1)) 
                                    || (FieldValueText.Length > 40))) {
                            cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, ,, ,, ,, "text");
                        }
                        else {
                            cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, ,, ,, "text");
                        }
                        
                        // s.Add( " </span></nobr></td>")
                        FieldTypeIdLink;
                        // 
                        //  ----- Link (href value
                        // 
                        return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                        FieldValueText = genericController.encodeText(FieldValueObject);
                        EditorString = ("" 
                                    + (cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, 80, FormFieldLCaseName, ,, "link") + (" <a href=\"#\" onClick=\"OpenResourceLinkWindow( \'" 
                                    + (FormFieldLCaseName + ("\' ) ;return false;\"><img src=\"/ccLib/images/ResourceLink1616.gif\" width=16 height=16 border=0 alt=\"Li" +
                                    "nk to a resource\" title=\"Link to a resource\"></a>" + (" <a href=\"#\" onClick=\"OpenSiteExplorerWindow( \'" 
                                    + (FormFieldLCaseName + "\' ) ;return false;\"><img src=\"/ccLib/images/PageLink1616.gif\" width=16 height=16 border=0 alt=\"Link t" +
                                    "o a page\" title=\"Link to a page\"></a>")))))));
                        FieldTypeIdResourceLink;
                        // 
                        //  ----- Resource Link (src value)
                        // 
                        return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                        FieldValueText = genericController.encodeText(FieldValueObject);
                        EditorString = ("" 
                                    + (cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, 80, FormFieldLCaseName, ,, "resourceLink") + (" <a href=\"#\" onClick=\"OpenResourceLinkWindow( \'" 
                                    + (FormFieldLCaseName + "\' ) ;return false;\"><img src=\"/ccLib/images/ResourceLink1616.gif\" width=16 height=16 border=0 alt=\"Li" +
                                    "nk to a resource\" title=\"Link to a resource\"></a>"))));
                        FieldTypeIdText;
                        // 
                        //  ----- Text Type
                        // 
                        return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                        FieldValueText = genericController.encodeText(FieldValueObject);
                        Password;
                        // 
                        //  Password forces simple text box
                        // 
                        EditorString = cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, ,, ,, true, ,, "password");
                        // 
                        //  non-password
                        // 
                        if ((((FieldValueText.IndexOf("\n", 0) + 1) 
                                    == 0) 
                                    && (FieldValueText.Length < 40))) {
                            // 
                            //  text field shorter then 40 characters without a CR
                            // 
                            EditorString = cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, ,, ,, "text");
                        }
                        else {
                            // 
                            //  longer text data, or text that contains a CR
                            // 
                            EditorStyleModifier = "textexpandable";
                            EditorString = cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, 10, ,, ,, "text");
                        }
                        
                        FieldTypeIdHTML;
                        FieldTypeIdFileHTML;
                        // 
                        //  content is html
                        // 
                        return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                        FieldValueText = genericController.encodeText(FieldValueObject);
                        // 
                        //  9/7/2012 -- added this to support:
                        //    html fields types mean they hold html
                        //    .htmlContent means edit it with text editor (so you edit the html)
                        // 
                        (htmlContent & FieldPreferenceHTML);
                        // 
                        //  View the content as Html, not wysiwyg
                        // 
                        EditorStyleModifier = "textexpandable";
                        EditorString = cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, 10, ,, ,, "text");
                        // 
                        //  wysiwyg editor
                        // 
                        if ((FieldValueText == "")) {
                            // 
                            //  editor needs a starting p tag to setup correctly
                            // 
                            FieldValueText = HTMLEditorDefaultCopyNoCr;
                        }
                        
                        EditorStyleModifier = "htmleditor";
                        FieldRows = cpCore.userProperty.getInteger((adminContent.Name + ("." 
                                        + (FieldName + ".PixelHeight"))), 500);
                        cpCore.html.getFormInputHTML(FormFieldLCaseName, FieldValueText, "500", ,, false, true, editorAddonListJSON, styleList, styleOptionList);
                        // innovaEditor = New innovaEditorAddonClassFPO
                        // EditorString = innovaEditor.getInnovaEditor( FormFieldLCaseName, EditorContext, FieldValueText, "", "", True, False, TemplateIDForStyles, emailIdForStyles)
                        EditorString = ("<div style=\"width:95%\">" 
                                    + (EditorString + "</div>"));
                        FieldOptionRow = " ";
                        FieldTypeIdLongText;
                        FieldTypeIdFileText;
                        // 
                        //  -- Long Text, use text editor
                        // 
                        return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                        FieldValueText = genericController.encodeText(FieldValueObject);
                        // 
                        EditorStyleModifier = "textexpandable";
                        FieldRows = cpCore.userProperty.getInteger((adminContent.Name + ("." 
                                        + (FieldName + ".RowHeight"))), 10);
                        EditorString = cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, FieldRows, ,, FormFieldLCaseName, false, ,, "text");
                        // 
                        FieldTypeIdFileCSS;
                        // 
                        //  ----- CSS field
                        // 
                        return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                        FieldValueText = genericController.encodeText(FieldValueObject);
                        EditorStyleModifier = "textexpandable";
                        FieldRows = cpCore.userProperty.getInteger((adminContent.Name + ("." 
                                        + (FieldName + ".RowHeight"))), 10);
                        EditorString = cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, 10, ,, ,, "styles");
                        FieldTypeIdFileJavascript;
                        // 
                        //  ----- Javascript field
                        // 
                        return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                        FieldValueText = genericController.encodeText(FieldValueObject);
                        EditorStyleModifier = "textexpandable";
                        FieldRows = cpCore.userProperty.getInteger((adminContent.Name + ("." 
                                        + (FieldName + ".RowHeight"))), 10);
                        EditorString = cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, FieldRows, ,, FormFieldLCaseName, false, ,, "text");
                        // 
                        FieldTypeIdFileXML;
                        // 
                        //  ----- xml field
                        // 
                        return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                        FieldValueText = genericController.encodeText(FieldValueObject);
                        EditorStyleModifier = "textexpandable";
                        FieldRows = cpCore.userProperty.getInteger((adminContent.Name + ("." 
                                        + (FieldName + ".RowHeight"))), 10);
                        EditorString = cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, FieldRows, ,, FormFieldLCaseName, false, ,, "text");
                        // 
                        // 
                        //  ----- Legacy text type -- not used unless something was missed
                        // 
                        return_NewFieldList = (return_NewFieldList + ("," + FieldName));
                        FieldValueText = genericController.encodeText(FieldValueObject);
                        Password;
                        // 
                        //  Password forces simple text box
                        // 
                        EditorString = cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, ,, ,, true, ,, "password");
                        htmlContent;
                        // 
                        //  not HTML capable, textarea with resizing
                        // 
                        if (((fieldTypeId == FieldTypeIdText) 
                                    && (((FieldValueText.IndexOf("\n", 0) + 1) 
                                    == 0) 
                                    && (FieldValueText.Length < 40)))) {
                            // 
                            //  text field shorter then 40 characters without a CR
                            // 
                            EditorString = cpCore.html.html_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, ,, ,, "text");
                        }
                        else {
                            // 
                            //  longer text data, or text that contains a CR
                            // 
                            EditorStyleModifier = "textexpandable";
                            EditorString = cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, FieldValueText, 10, ,, ,, "text");
                        }
                        
                        (htmlContent & FieldPreferenceHTML);
                        // 
                        //  HTMLContent true, and prefered
                        // 
                        if ((FieldValueText == "")) {
                            // 
                            //  editor needs a starting p tag to setup correctly
                            // 
                            FieldValueText = HTMLEditorDefaultCopyNoCr;
                        }
                        
                        EditorStyleModifier = "htmleditor";
                        FieldRows = cpCore.userProperty.getInteger((adminContent.Name + ("." 
                                        + (FieldName + ".PixelHeight"))), 500);
                        cpCore.html.getFormInputHTML(FormFieldLCaseName, FieldValueText, "500", ,, false, true, editorAddonListJSON, styleList, styleOptionList);
                        // innovaEditor = New innovaEditorAddonClassFPO
                        // EditorString = innovaEditor.getInnovaEditor( FormFieldLCaseName, EditorContext, FieldValueText, "", "", True, False, TemplateIDForStyles, emailIdForStyles)
                        EditorString = ("<div style=\"width:95%\">" 
                                    + (EditorString + "</div>"));
                        FieldOptionRow = " ";
                        // 
                        //  HTMLContent true, but text editor selected
                        // 
                        EditorStyleModifier = "textexpandable";
                        FieldRows = cpCore.userProperty.getInteger((adminContent.Name + ("." 
                                        + (FieldName + ".RowHeight"))), 10);
                        EditorString = cpCore.html.html_GetFormInputTextExpandable2(FormFieldLCaseName, genericController.encodeHTML(FieldValueText), FieldRows, "600px", FormFieldLCaseName, false, ,, "text");
                        if ((includeFancyBox == true)) {
                            FieldHelpFound = false;
                            EditorRightSideIcon = "";
                            HelpMsgDefault = "";
                            HelpMsgCustom = "";
                            EditorHelp = "";
                            LcaseName = genericController.vbLCase(., nameLc);
                            if (AllowHelpMsgCustom) {
                                HelpDefault;
                                HelpCustom;
                                // HelpPtr = helpIdIndex.getPtr(CStr(.id))
                                // If HelpPtr >= 0 Then
                                //     FieldHelpFound = True
                                //     HelpMsgDefault = helpDefaultCache(HelpPtr)
                                //     HelpMsgCustom = HelpCustomCache(HelpPtr)
                                // End If
                            }
                            
                            // 
                            //  12/4/2016 - REFACTOR - this is from the old system. Delete this after we varify it is no longer needed
                            // 
                            // If Not FieldHelpFound Then
                            //     Call getFieldHelpMsgs(adminContent.parentID, .nameLc, HelpMsgDefault, HelpMsgCustom)
                            //     CS = cpCore.app.csInsertRecord("Content Field Help")
                            //     If cpCore.app.csOk(CS) Then
                            //         Call cpCore.app.setCS(CS, "fieldid", fieldId)
                            //         Call cpCore.app.setCS(CS, "name", adminContent.Name & "." & .nameLc)
                            //         Call cpCore.app.setCS(CS, "HelpDefault", HelpMsgDefault)
                            //         Call cpCore.app.setCS(CS, "HelpCustom", HelpMsgCustom)
                            //     End If
                            //     Call cpCore.app.csClose(CS)
                            // End If
                            if ((HelpMsgCustom != "")) {
                                HelpMsg = HelpMsgCustom;
                            }
                            else {
                                HelpMsg = HelpMsgDefault;
                            }
                            
                            HelpMsgOpenedRead = HelpMsg;
                            HelpMsgClosed = HelpMsg;
                            HelpMsgClosed.Length = 0;
                            IsEmptyHelp = 0;
                            IsLongHelp = (HelpMsgClosed.Length > 100);
                            if (IsLongHelp) {
                                HelpMsgClosed = (HelpMsgClosed.Substring(0, 100) + "...");
                            }
                            
                            // 
                            HelpID = ("helpId" + fieldId);
                            HelpEditorID = ("helpEditorId" + fieldId);
                            HelpOpenedReadID = ("HelpOpenedReadID" + fieldId);
                            HelpOpenedEditID = ("HelpOpenedEditID" + fieldId);
                            HelpClosedID = ("helpClosedId" + fieldId);
                            HelpClosedContentID = ("helpClosedContentId" + fieldId);
                            AllowHelpRow = true;
                            AjaxQS = (RequestNameAjaxFunction + ("=" 
                                        + (ajaxGetFieldEditorPreferenceForm + ("&fieldid=" 
                                        + (fieldId + ("¤tEditorAddonId=" 
                                        + (editorAddonID + ("&fieldTypeDefaultEditorAddonId=" + fieldTypeDefaultEditorAddonId))))))));
                            fancyBoxLinkId = ("fbl" + fancyBoxPtr);
                            fancyBoxContentId = ("fbc" + fancyBoxPtr);
                            fancyBoxHeadJS = (fancyBoxHeadJS + ("\r\n" + ("jQuery(\'#" 
                                        + (fancyBoxLinkId + ("\').fancybox({" + ("\'titleShow\':false," + ("\'transitionIn\':\'elastic\'," + ("\'transitionOut\':\'elastic\'," + ("\'overlayOpacity\':\'.2\'," + ("\'overlayColor\':\'#000000\'," + ("\'onStart\':function(){cj.ajax.qs(\'" 
                                        + (AjaxQS + ("\',\'\',\'" 
                                        + (fancyBoxContentId + ("\')}" + "});")))))))))))))));
                            EditorHelp = (EditorHelp 
                                        + (cr + ("<div style=\"float:right;\">" 
                                        + (cr2 + ("<a id=\"" 
                                        + (fancyBoxLinkId + ("\" href=\"#" 
                                        + (fancyBoxContentId + ("\" title=\"select an alternate editor for this field.\" tabindex=\"-1\"><img src=\"/ccLib/images/NavAltEdit" +
                                        "or.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"select an alternate editor" +
                                        " for this field.\"></a>" 
                                        + (cr2 + ("<div style=\"display:none;\">" 
                                        + (cr3 + ("<div class=\"ccEditorPreferenceCon\" id=\"" 
                                        + (fancyBoxContentId + ("\"><div style=\"margin:20px auto auto auto;\"><img src=\"/ccLib/images/ajax-loader-big.gif\" width=\"32\" he" +
                                        "ight=\"32\"></div></div>" 
                                        + (cr2 + ("</div>" 
                                        + (cr + ("</div>" + "")))))))))))))))))));
                            fancyBoxPtr = (fancyBoxPtr + 1);
                            // 
                            // ------------------------------------------------------------------------------------------------------------
                            //  field help
                            // ------------------------------------------------------------------------------------------------------------
                            // 
                            if (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                                // 
                                //  Admin view
                                // 
                                if ((HelpMsgDefault == "")) {
                                    HelpMsgDefault = "Admin: No default help is available for this field.";
                                }
                                
                                HelpMsgOpenedRead = ("" + ("<!-- close icon --><div class=\"\" style=\"float:right\"><a href=\"javascript:cj.hide(\'" 
                                            + (HelpOpenedReadID + ("\');cj.show(\'" 
                                            + (HelpClosedID + ("\');\"><img src=\"/ccLib/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" " +
                                            "title=\"close\"></a></div>" + ("<div class=\"header\">Default Help</div>" + ("<div class=\"body\">" 
                                            + (HelpMsgDefault + ("</div>" + ("<div class=\"header\">Custom Help</div>" + ("<div class=\"body\">" 
                                            + (HelpMsgCustom + ("</div>" + ""))))))))))))));
                                HelpMsgOpenedEdit = ("" + ("<div class=\"header\">Default Help</div>" + ("<div class=\"body\">" 
                                            + (HelpMsgDefault + ("</div>" + ("<div class=\"header\">Custom Help</div>" + ("<div class=\"body\"><textarea id=\"" 
                                            + (HelpEditorID + ("\" ROWS=\"10\" style=\"width:100%;\">" 
                                            + (HelpMsgCustom + ("</TEXTAREA></div>" + ("<div class=\"\">" + ("<input type=\"submit\" name=\"button\" value=\"Update\" onClick=\"updateFieldHelp(\'" 
                                            + (fieldId + ("\',\'" 
                                            + (HelpEditorID + ("\',\'" 
                                            + (HelpClosedContentID + ("\');cj.hide(\'" 
                                            + (HelpOpenedEditID + ("\');cj.show(\'" 
                                            + (HelpClosedID + ("\');return false\">" + ("<input type=\"submit\" name=\"button\" value=\"Cancel\" onClick=\"cj.hide(\'" 
                                            + (HelpOpenedEditID + ("\');cj.show(\'" 
                                            + (HelpClosedID + ("\');return false\">" + ("</div>" + "")))))))))))))))))))))))))))));
                                if (IsLongHelp) {
                                    // 
                                    //  Long help, closed gets MoreHelpIcon (opens to HelpMsgOpenedRead) and HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                    // 
                                    HelpMsgClosed = ("" + ("<!-- open read icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide(\'" 
                                                + (HelpClosedID + ("\');cj.show(\'" 
                                                + (HelpOpenedReadID + ("\');\" tabindex=\"-1\"><img src=\"/ccLib/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-a" +
                                                "lign:middle;\" title=\"more help\"></a></div>" + ("<!-- open edit icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide(\'" 
                                                + (HelpClosedID + ("\');cj.show(\'" 
                                                + (HelpOpenedEditID + ("\');\" tabindex=\"-1\"><img src=\"/ccLib/images/NavHelpEdit.gif\" width=18 height=18 border=0 style=\"vertic" +
                                                "al-align:middle;\" title=\"edit help\"></a></div>" + ("<div id=\"" 
                                                + (HelpClosedContentID + ("\">" 
                                                + (HelpMsgClosed + ("</div>" + ""))))))))))))))));
                                }
                                else if (!IsEmptyHelp) {
                                    // 
                                    //  short help, closed gets helpmsgclosed + HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                    // 
                                    HelpMsgClosed = ("" + ("<!-- open edit icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide(\'" 
                                                + (HelpClosedID + ("\');cj.show(\'" 
                                                + (HelpOpenedEditID + ("\');\" tabindex=\"-1\"><img src=\"/ccLib/images/NavHelpEdit.gif\" width=18 height=18 border=0 style=\"vertic" +
                                                "al-align:middle;\" title=\"edit help\"></a></div>" + ("<div id=\"" 
                                                + (HelpClosedContentID + ("\">" 
                                                + (HelpMsgClosed + ("</div>" + "")))))))))));
                                }
                                else {
                                    // 
                                    //  Empty help, closed gets helpmsgclosed + HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                    // 
                                    HelpMsgClosed = ("" + ("<!-- open edit icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide(\'" 
                                                + (HelpClosedID + ("\');cj.show(\'" 
                                                + (HelpOpenedEditID + ("\');\" tabindex=\"-1\"><img src=\"/ccLib/images/NavHelpEdit.gif\" width=18 height=18 border=0 style=\"vertic" +
                                                "al-align:middle;\" title=\"edit help\"></a></div>" + ("<div id=\"" 
                                                + (HelpClosedContentID + ("\">" 
                                                + (HelpMsgClosed + ("</div>" + "")))))))))));
                                }
                                
                                EditorHelp = (EditorHelp + ("<div id=\"" 
                                            + (HelpOpenedReadID + ("\" class=\"opened\">" 
                                            + (HelpMsgOpenedRead + ("</div>" + ("<div id=\"" 
                                            + (HelpOpenedEditID + ("\" class=\"opened\">" 
                                            + (HelpMsgOpenedEdit + ("</div>" + ("<div id=\"" 
                                            + (HelpClosedID + ("\" class=\"closed\">" 
                                            + (HelpMsgClosed + ("</div>" + ""))))))))))))))));
                            }
                            else {
                                // 
                                //  Non-admin view
                                // 
                                HelpMsgOpenedRead = ("" + ("<div class=\"body\">" + ("<!-- close icon --><a href=\"javascript:cj.hide(\'" 
                                            + (HelpOpenedReadID + ("\');cj.show(\'" 
                                            + (HelpClosedID + ("\');\"><img src=\"/ccLib/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;fl" +
                                            "oat:right\" title=\"close\"></a>" 
                                            + (HelpMsg + ("</div>" + "")))))))));
                                HelpMsgOpenedEdit = ("" + "");
                                if (IsLongHelp) {
                                    // 
                                    //  Long help
                                    // 
                                    HelpMsgClosed = ("" + ("<div class=\"body\">" + ("<!-- open read icon --><a href=\"javascript:cj.hide(\'" 
                                                + (HelpClosedID + ("\');cj.show(\'" 
                                                + (HelpOpenedReadID + ("\');\"><img src=\"/ccLib/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;fl" +
                                                "oat:right;\" title=\"more help\"></a>" 
                                                + (HelpMsgClosed + ("</div>" + "")))))))));
                                }
                                else if (!IsEmptyHelp) {
                                    // 
                                    //  short help
                                    // 
                                    HelpMsgClosed = ("" + ("<div class=\"body\">" 
                                                + (HelpMsg + ("</div>" + ""))));
                                }
                                else {
                                    // 
                                    //  no help
                                    // 
                                    AllowHelpRow = false;
                                    HelpMsgClosed = ("" + "");
                                }
                                
                                EditorHelp = (EditorHelp + ("<div id=\"" 
                                            + (HelpOpenedReadID + ("\" class=\"opened\">" 
                                            + (HelpMsgOpenedRead + ("</div>" + ("<div id=\"" 
                                            + (HelpClosedID + ("\" class=\"closed\">" 
                                            + (HelpMsgClosed + ("</div>" + "")))))))))));
                            }
                            
                            // 
                            //  assemble the help line
                            // 
                            s.Add(("<tr>" + ("<td class=\"ccEditCaptionCon\"><div class=\"" 
                                            + (EditorStyleModifier + ("\">" 
                                            + (Caption + ("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"1\" height=\"15\" ></div></td>" + ("<td class=\"ccEditFieldCon\">" + ("<div class=\"ccEditorCon\">" 
                                            + (EditorString + "</div>"))))))))));
                            if (AllowHelpRow) {
                                s.Add(("<div class=\"ccEditorHelpCon\">" 
                                                + (EditorHelp + "</div>")));
                            }
                            
                            s.Add(("" + ("</td>" + "</tr>")));
                        }
                        
                    }
                    
                    // 
                    //  ----- add the *Required Fields footer
                    // 
                    s.Add(("" + ("<tr><td colspan=2 style=\"padding-top:10px;font-size:70%\">" + ("<div>* Field is required.</div>" + "<div>** Field must be unique.</div>"))));
                    if (needUniqueEmailMessage) {
                        s.Add("<div>*** Field must be unique because this site allows login by email.</div>");
                    }
                    
                    s.Add("</td></tr>");
                    // 
                    //  ----- close the panel
                    // 
                    if ((EditTab == "")) {
                        Caption = "Content Fields";
                    }
                    else {
                        Caption = ("Content Fields - " + EditTab);
                    }
                    
                    EditSectionPanelCount = (EditSectionPanelCount + 1);
                    returnHtml = Adminui.GetEditPanel(!allowAdminTabs, Caption, "", (Adminui.EditTableOpen 
                                    + (s.Text + Adminui.EditTableClose)));
                    EditSectionPanelCount = (EditSectionPanelCount + 1);
                    s = null;
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnHtml;
        }
        
        // 
        // ========================================================================
        //    Display field in the admin/edit
        // ========================================================================
        // 
        private string GetForm_Edit_ContentTracking(Models.Complex.cdefModel adminContent, editRecordClass editRecord) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_ContentTracking")
            // 
            int CSRules;
            string HTMLFieldString;
            //  converted array to dictionary - Dim FieldPointer As Integer
            int CSPointer;
            int RecordID;
            int ContentID;
            int CSLists;
            int RecordCount;
            int ContentWatchListID;
            stringBuilderLegacyController FastString;
            string Copy;
            adminUIController Adminui = new adminUIController(cpCore);
            // 
            if (adminContent.AllowContentTracking) {
                FastString = new stringBuilderLegacyController();
                // 
                if (!ContentWatchLoaded) {
                    // 
                    //  ----- Load in the record to print
                    // 
                    LoadContentTrackingDataBase(adminContent, editRecord);
                    LoadContentTrackingResponse(adminContent, editRecord);
                    //         Call LoadAndSaveCalendarEvents
                }
                
                CSLists = cpCore.db.csOpen("Content Watch Lists", ("name<>" + cpCore.db.encodeSQLText("")), "ID");
                if (cpCore.db.csOk(CSLists)) {
                    // 
                    //  ----- Open the panel
                    // 
                    // Call cpCore.main_PrintPanelTop("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                    // Call FastString.Add(adminui.EditTableOpen)
                    // Call FastString.Add(vbCrLf & "<tr><td colspan=""3"" class=""ccAdminEditSubHeader"">Content Tracking</td></tr>")
                    //             '
                    //             ' ----- Print matching Content Watch fields
                    //             '
                    //             Call FastString.Add(cpCore.main_GetFormInputHidden("WhatsNewResponse", -1))
                    //             Call FastString.Add(cpCore.main_GetFormInputHidden("contentwatchrecordid", ContentWatchRecordID))
                    // 
                    //  ----- Content Watch Lists, checking the ones that have active rules
                    // 
                    RecordCount = 0;
                    while (cpCore.db.csOk(CSLists)) {
                        ContentWatchListID = cpCore.db.csGetInteger(CSLists, "id");
                        // 
                        if ((ContentWatchRecordID != 0)) {
                            CSRules = cpCore.db.csOpen("Content Watch List Rules", ("(ContentWatchID=" 
                                            + (ContentWatchRecordID + (")AND(ContentWatchListID=" 
                                            + (ContentWatchListID + ")")))));
                            if (editRecord.Read_Only) {
                                HTMLFieldString = genericController.encodeText(cpCore.db.csOk(CSRules));
                            }
                            else {
                                HTMLFieldString = cpCore.html.html_GetFormInputCheckBox2(("ContentWatchList." + cpCore.db.csGet(CSLists, "ID")), cpCore.db.csOk(CSRules));
                            }
                            
                            cpCore.db.csClose(CSRules);
                        }
                        else if (editRecord.Read_Only) {
                            HTMLFieldString = genericController.encodeText(false);
                        }
                        else {
                            HTMLFieldString = cpCore.html.html_GetFormInputCheckBox2(("ContentWatchList." + cpCore.db.csGet(CSLists, "ID")), false);
                        }
                        
                        // 
                        FastString.Add(Adminui.GetEditRow(HTMLFieldString, ("Include in " + cpCore.db.csGet(CSLists, "name")), ("When true, this Content Record can be included in the \'" 
                                            + (cpCore.db.csGet(CSLists, "name") + "\' list")), false, false, ""));
                        cpCore.db.csGoNext(CSLists);
                        RecordCount = (RecordCount + 1);
                    }
                    
                    // 
                    //  ----- Whats New Headline (editable)
                    // 
                    if (editRecord.Read_Only) {
                        HTMLFieldString = cpCore.html.main_encodeHTML(ContentWatchLinkLabel);
                    }
                    else {
                        HTMLFieldString = cpCore.html.html_GetFormInputText2("ContentWatchLinkLabel", ContentWatchLinkLabel, 1, cpCore.siteProperties.defaultFormInputWidth);
                        // HTMLFieldString = "<textarea rows=""1"" name=""ContentWatchLinkLabel"" cols=""" & cpCore.app.SiteProperty_DefaultFormInputWidth & """>" & ContentWatchLinkLabel & "</textarea>"
                    }
                    
                    FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Caption", @"This caption is displayed on all Content Watch Lists, linked to the location on the web site where this content is displayed. RSS feeds created from Content Watch Lists will use this caption as the record title if not other field is selected in the Content Definition.", false, true, "ContentWatchLinkLabel"));
                    // 
                    //  ----- Whats New Expiration
                    // 
                    Copy = ContentWatchExpires.ToString;
                    if ((Copy == "12:00:00 AM")) {
                        Copy = "";
                    }
                    
                    if (editRecord.Read_Only) {
                        HTMLFieldString = cpCore.html.main_encodeHTML(Copy);
                    }
                    else {
                        HTMLFieldString = cpCore.html.html_GetFormInputDate("ContentWatchExpires", Copy, cpCore.siteProperties.defaultFormInputWidth.ToString);
                        // HTMLFieldString = "<textarea rows=""1"" name=""ContentWatchExpires"" cols=""" & cpCore.app.SiteProperty_DefaultFormInputWidth & """>" & Copy & "</textarea>"
                    }
                    
                    FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Expires", "When this record is included in a What\'s New list, this record is blocked from the list after this da" +
                            "te.", false, false, ""));
                    // 
                    //  ----- Public Link (read only)
                    // 
                    HTMLFieldString = ContentWatchLink;
                    if ((HTMLFieldString == "")) {
                        HTMLFieldString = "(must first be viewed on public site)";
                    }
                    
                    FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Location on Site", "The public site URL where this content was last viewed.", false, false, ""));
                    // 
                    //  removed 11/27/07 - RSS clicks not counted, rc/ri method of counting link clicks is not reliable.
                    //             '
                    //             ' ----- Clicks (read only)
                    //             '
                    //             HTMLFieldString = ContentWatchClicks
                    //             If HTMLFieldString = "" Then
                    //                 HTMLFieldString = 0
                    //                 End If
                    //             Call FastString.Add(AdminUI.GetEditRow( HTMLFieldString, "Clicks", "The number of site users who have clicked this link in what's new lists", False, False, ""))
                    // 
                    //  ----- close the panel
                    // 
                    string s;
                    s = ("" 
                                + (Adminui.EditTableOpen 
                                + (FastString.Text 
                                + (Adminui.EditTableClose 
                                + (cpCore.html.html_GetFormInputHidden("WhatsNewResponse", "-1") + cpCore.html.html_GetFormInputHidden("contentwatchrecordid", ContentWatchRecordID.ToString))))));
                    GetForm_Edit_ContentTracking = Adminui.GetEditPanel(!allowAdminTabs, "Content Tracking", "Include in Content Watch Lists", s);
                    EditSectionPanelCount = (EditSectionPanelCount + 1);
                    // 
                }
                
                cpCore.db.csClose(CSLists);
                FastString = null;
            }
            
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
        ErrorTrap:
            FastString = null;
            handleLegacyClassError3("GetForm_Edit_ContentTracking");
        }
        
        // 
        // ========================================================================
        //    Display field in the admin/edit
        // ========================================================================
        // 
        private string GetForm_Edit_Control(Models.Complex.cdefModel adminContent, editRecordClass editRecord) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_Control")
            // 
            string s;
            bool AllowEID;
            string EID;
            bool IsEmptyList;
            int ParentID;
            int ParentCID;
            string Criteria;
            int LimitContentSelectToThisID;
            string SQL;
            int TableID;
            int TableName;
            int ChildCID;
            string CIDList = "";
            string TableName2;
            string RecordContentName;
            bool ContentSupportsParentID;
            int CS;
            string HTMLFieldString;
            // Dim FieldPtr As Integer
            int CSPointer;
            int RecordID;
            string hiddenInputs = "";
            stringBuilderLegacyController FastString;
            int FieldValueInteger;
            bool FieldRequired;
            string FieldHelp;
            string AuthoringStatusMessage;
            string Delimiter;
            string Copy;
            adminUIController Adminui = new adminUIController(cpCore);
            // ''Dim arrayOfFields() As appServices_metaDataClass.CDefFieldClass
            // 
            FastString = new stringBuilderLegacyController();
            // 
            // arrayOfFields = adminContent.fields
            // With...
            // 
            //  ----- test admin content before using it
            // 
            if ((adminContent.Name == "")) {
                // 
                //  Content not found or not loaded
                // 
                if ((adminContent.Id == 0)) {
                    // 
                    //  Content Definition was not found
                    // 
                    handleLegacyClassError("GetForm_Edit_Control", "No content definition was specified for this page");
                }
                else {
                    // 
                    //  Content Definition was not specified
                    // 
                    handleLegacyClassError("GetForm_Edit_Control", ("The content definition specified for this page [" 
                                    + (adminContent.Id + "] was not found")));
                }
                
            }
            else {
                
            }
            
            // 
            bool Checked;
            // '
            // ' ----- Authoring status
            // '
            // FieldHelp = "In immediate authoring mode, the live site is changed when each record is saved. In Workflow authoring mode, there are several steps to publishing a change. This field displays the current stage of this record."
            // FieldRequired = False
            // AuthoringStatusMessage = cpCore.doc.authContext.main_GetAuthoringStatusMessage(cpCore, false, editRecord.EditLock, editRecord.EditLockMemberName, editRecord.EditLockExpires, editRecord.ApproveLock, editRecord.ApprovedName, editRecord.SubmitLock, editRecord.SubmittedName, editRecord.IsDeleted, editRecord.IsInserted, editRecord.IsModified, editRecord.LockModifiedName)
            // Call FastString.Add(Adminui.GetEditRow(AuthoringStatusMessage, "Authoring Status", FieldHelp, FieldRequired, False, ""))
            // 'Call FastString.Add(AdminUI.GetEditRow( AuthoringStatusMessage, "Authoring Status", FieldHelp, FieldRequired, False, ""))
            // 
            //  ----- RecordID
            // 
            FieldHelp = "This is the unique number that identifies this record within this content.";
            if ((editRecord.id == 0)) {
                HTMLFieldString = "(available after save)";
            }
            else {
                HTMLFieldString = genericController.encodeText(editRecord.id);
            }
            
            HTMLFieldString = cpCore.html.html_GetFormInputText2("ignore", HTMLFieldString, ,, ,, true);
            FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Record Number", FieldHelp, true, false, ""));
            // 
            //  -- Active
            Copy = "When unchecked, add-ons can ignore this record as if it was temporarily deleted.";
            HTMLFieldString = cpCore.html.html_GetFormInputCheckBox2("active", editRecord.active);
            FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Active", Copy, false, false, ""));
            // 
            //  ----- If Page Content , check if this is the default PageNotFound page
            // 
            if ((genericController.vbLCase(adminContent.ContentTableName) == "ccpagecontent")) {
                // 
                //  Landing Page
                // 
                Copy = @"If selected, this page will be displayed when a user comes to your website with just your domain name and no other page is requested. This is called your default Landing Page. Only one page on the site can be the default Landing Page. If you want a unique Landing Page for a specific domain name, add it in the 'Domains' content and the default will not be used for that docpCore.main_";
                Checked = ((editRecord.id != 0) 
                            && (editRecord.id == cpCore.siteProperties.getinteger("LandingPageID", 0)));
                if (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                    HTMLFieldString = cpCore.html.html_GetFormInputCheckBox2("LandingPageID", Checked);
                }
                else {
                    HTMLFieldString = ("<b>" 
                                + (genericController.GetYesNo(Checked) + ("</b>" + cpCore.html.html_GetFormInputHidden("LandingPageID", Checked))));
                }
                
                HTMLFieldString = HTMLFieldString;
                FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Set Default Landing Page", Copy, false, false, ""));
                // 
                //  Page Not Found
                // 
                Copy = "If selected, this content will be displayed when a page can not be found. Only one page on the site c" +
                "an be marked.";
                Checked = ((editRecord.id != 0) 
                            && (editRecord.id == cpCore.siteProperties.getinteger("PageNotFoundPageID", 0)));
                if (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                    HTMLFieldString = cpCore.html.html_GetFormInputCheckBox2("PageNotFound", Checked);
                }
                else {
                    HTMLFieldString = ("<b>" 
                                + (genericController.GetYesNo(Checked) + ("</b>" + cpCore.html.html_GetFormInputHidden("PageNotFound", Checked))));
                }
                
                //             If (EditRecord.ID <> 0) And (EditRecord.ID = cpCore.main_GetSiteProperty2("PageNotFoundPageID", "0", True)) Then
                //                 HTMLFieldString = cpCore.main_GetFormInputCheckBox2("PageNotFound", True)
                //             Else
                //                 HTMLFieldString = cpCore.main_GetFormInputCheckBox2("PageNotFound", False)
                //             End If
                HTMLFieldString = HTMLFieldString;
                FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Set Page Not Found", Copy, false, false, ""));
            }
            
            // 
            //  ----- Last Known Public Site URL
            // 
            if (((adminContent.ContentTableName.ToUpper() == "CCPAGECONTENT") 
                        || (adminContent.ContentTableName.ToUpper() == "ITEMS"))) {
                FieldHelp = "This is the URL where this record was last displayed on the site. It may be blank if the record has n" +
                "ot been displayed yet.";
                Copy = cpCore.doc.getContentWatchLinkByKey((editRecord.contentControlId + ("." + editRecord.id)), ,, false);
                if ((Copy == "")) {
                    HTMLFieldString = "unknown";
                }
                else {
                    HTMLFieldString = ("<a href=\"" 
                                + (genericController.encodeHTML(Copy) + ("\" target=\"_blank\">" 
                                + (Copy + "</a>"))));
                }
                
                FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Last Known Public URL", FieldHelp, false, false, ""));
            }
            
            // 
            //  ----- Widget Code
            // 
            if ((genericController.vbLCase(adminContent.ContentTableName) == "ccaggregatefunctions")) {
                // 
                //  ----- Add-ons
                // 
                bool AllowWidget;
                AllowWidget = false;
                if (editRecord.fieldsLc.ContainsKey("remotemethod")) {
                    AllowWidget = genericController.EncodeBoolean(editRecord.fieldsLc.Item["remotemethod"].value);
                }
                
                if (!AllowWidget) {
                    FieldHelp = "If you wish to use this add-on as a widget, enable \'Is Remote Method\' on the \'Placement\' tab and save" +
                    " the record. The necessary html code, or \'embed code\' will be created here for you to cut-and-paste " +
                    "into the website.";
                    HTMLFieldString = "";
                    HTMLFieldString = cpCore.html.html_GetFormInputTextExpandable2("ignore", HTMLFieldString, 1, ,, ,, true);
                    FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Widget Code", FieldHelp, true, false, ""));
                }
                else {
                    FieldHelp = "If you wish to use this add-on as a widget, cut and paste the \'Widget Code\' into the website content." +
                    " If any code appears in the \'Widget Head\', this will need to be pasted into the head section of the " +
                    "website.";
                    HTMLFieldString = ("" + ("<SCRIPT type=text/javascript>" + ("\r\n" + ("var ccProto=((\'https:\'==document.location.protocol) ? \'https://\' : \'http://\');" + ("\r\n" + ("document.write(unescape(\"%3Cscript src=\'\" + ccProto + \"" 
                                + (cpCore.webServer.requestDomain + ("/ccLib/ClientSide/Core.js\' type=\'text/javascript\'%3E%3C/script%3E\"));" + ("\r\n" + ("document.write(unescape(\"%3Cscript src=\'\" + ccProto + \"" 
                                + (cpCore.webServer.requestDomain + ("/" 
                                + (genericController.EncodeURL(editRecord.nameLc) + ("?requestjsform=1\' type=\'text/javascript\'%3E%3C/script%3E\"));" + ("\r\n" + "</SCRIPT>")))))))))))))));
                    HTMLFieldString = cpCore.html.html_GetFormInputTextExpandable2("ignore", HTMLFieldString, 8);
                    FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Widget Code", FieldHelp, true, false, ""));
                }
                
            }
            
            // 
            //  ----- GUID
            // 
            if (editRecord.fieldsLc.ContainsKey("ccguid")) {
                Models.Complex.CDefFieldModel contentField = adminContent.fields.Item["ccguid"];
                HTMLFieldString = genericController.encodeText(editRecord.fieldsLc.Item["ccguid"].value);
                FieldHelp = "This is a unique number that identifies this record globally. A GUID is not required, but when set it" +
                " should never be changed. GUIDs are used to synchronize records. When empty, you can create a new gu" +
                "id. Only Developers can modify the guid.";
                if ((HTMLFieldString == "")) {
                    // 
                    //  add a set button
                    // 
                    string ccGuid;
                    ccGuid = ("{" 
                                + (Guid.NewGuid.ToString() + "}"));
                    HTMLFieldString = (cpCore.html.html_GetFormInputText2("ccguid", HTMLFieldString, ,, "ccguid", ,, false) + ("<input type=button value=set onclick=\"var e=document.getElementById(\'ccguid\');e.value=\'" 
                                + (ccGuid + "\';this.disabled=true;\">")));
                }
                else {
                    // 
                    //  field is read-only except for developers
                    // 
                    if (cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore)) {
                        HTMLFieldString = (cpCore.html.html_GetFormInputText2("ccguid", HTMLFieldString, ,, ,, false) + "");
                    }
                    else {
                        HTMLFieldString = (cpCore.html.html_GetFormInputText2("ccguid", HTMLFieldString, ,, ,, true) + cpCore.html.html_GetFormInputHidden("ccguid", HTMLFieldString));
                    }
                    
                }
                
                FastString.Add(Adminui.GetEditRow(HTMLFieldString, "GUID", FieldHelp, false, false, ""));
            }
            
            // 
            //  ----- EID (Encoded ID)
            // 
            FieldHelp = "";
            if ((genericController.vbUCase(adminContent.ContentTableName) == genericController.vbUCase("ccMembers"))) {
                AllowEID = (cpCore.siteProperties.getBoolean("AllowLinkLogin", true) | cpCore.siteProperties.getBoolean("AllowLinkRecognize", true));
                if (!AllowEID) {
                    HTMLFieldString = "(link login and link recognize are disabled in security preferences)";
                }
                else if ((editRecord.id == 0)) {
                    HTMLFieldString = "(available after save)";
                }
                else {
                    EID = genericController.encodeText(cpCore.security.encodeToken(editRecord.id, cpCore.doc.profileStartTime));
                    if (cpCore.siteProperties.getBoolean("AllowLinkLogin", true)) {
                        HTMLFieldString = EID;
                        // HTMLFieldString = EID _
                        //             & "<div>Any visitor who hits the site with eid=" & EID & " will be logged in as this member.</div>"
                        FieldHelp = ("Any visitor who hits the site with eid=" 
                                    + (EID + " will be logged in as this member."));
                    }
                    else {
                        FieldHelp = ("Any visitor who hits the site with eid=" 
                                    + (EID + " will be recognized as this member, but not logged in."));
                        HTMLFieldString = EID;
                        // HTMLFieldString = EID _
                        //             & "<div>Any visitor who hits the site with eid=" & EID & " will be recognized as this member, but not logged in</div>"
                    }
                    
                    FieldHelp = (FieldHelp + " To enable, disable or modify this feature, use the security tab on the Preferences page.");
                }
                
                HTMLFieldString = cpCore.html.html_GetFormInputText2("ignore", HTMLFieldString);
                FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Member Link Login EID", FieldHelp, true, false, ""));
            }
            
            // 
            //  ----- Controlling Content
            // 
            HTMLFieldString = "";
            FieldHelp = "The content in which this record is stored. This is similar to a database table.";
            Models.Complex.CDefFieldModel field;
            if (adminContent.fields.ContainsKey("contentcontrolid")) {
                field = adminContent.fields("contentcontrolid");
                // With...
                // 
                //  if this record has a parent id, only include CDefs compatible with the parent record - otherwise get all for the table
                // 
                FieldHelp = genericController.encodeText(field.HelpMessage);
                FieldRequired = genericController.EncodeBoolean(field.Required);
                FieldValueInteger = editRecord.contentControlId;
                // 
                // 
                // 
                if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                    HTMLFieldString = (HTMLFieldString + cpCore.html.html_GetFormInputHidden("ContentControlID", FieldValueInteger));
                }
                else {
                    RecordContentName = editRecord.contentControlId_Name;
                    Models.Complex.cdefModel RecordCDef;
                    TableName2 = Models.Complex.cdefModel.getContentTablename(cpCore, RecordContentName);
                    TableID = cpCore.db.getRecordID("Tables", TableName2);
                    // 
                    //  Test for parentid
                    // 
                    ParentID = 0;
                    ContentSupportsParentID = false;
                    if ((editRecord.id > 0)) {
                        CS = cpCore.db.csOpenRecord(RecordContentName, editRecord.id);
                        if (cpCore.db.csOk(CS)) {
                            ContentSupportsParentID = cpCore.db.cs_isFieldSupported(CS, "ParentID");
                            if (ContentSupportsParentID) {
                                ParentID = cpCore.db.csGetInteger(CS, "ParentID");
                            }
                            
                        }
                        
                        cpCore.db.csClose(CS);
                    }
                    
                    // 
                    LimitContentSelectToThisID = 0;
                    if (ContentSupportsParentID) {
                        // 
                        //  Parentid - restrict CDefs to those compatible with the parentid
                        // 
                        if ((ParentID != 0)) {
                            // 
                            //  This record has a parent, set LimitContentSelectToThisID to the parent's CID
                            // 
                            CSPointer = cpCore.db.csOpenRecord(RecordContentName, ParentID, ,, "ContentControlID");
                            if (cpCore.db.csOk(CSPointer)) {
                                LimitContentSelectToThisID = cpCore.db.csGetInteger(CSPointer, "ContentControlID");
                            }
                            
                            cpCore.db.csClose(CSPointer);
                        }
                        
                    }
                    
                    if ((cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) 
                                && (LimitContentSelectToThisID == 0))) {
                        // 
                        //  administrator, and either ( no parentid or does not support it), let them select any content compatible with the table
                        // 
                        HTMLFieldString = (HTMLFieldString + cpCore.html.main_GetFormInputSelect2("ContentControlID", FieldValueInteger, "Content", ("ContentTableID=" + TableID), "", "", IsEmptyList));
                        FieldHelp = (FieldHelp + " (Only administrators have access to this control. Changing the Controlling Content allows you to cha" +
                        "nge who can author the record, as well as how it is edited.)");
                    }
                    else {
                        // 
                        //  Limit the list to only those cdefs that are within the record's parent contentid
                        // 
                        RecordContentName = editRecord.contentControlId_Name;
                        TableName2 = Models.Complex.cdefModel.getContentTablename(cpCore, RecordContentName);
                        TableID = cpCore.db.getRecordID("Tables", TableName2);
                        CSPointer = cpCore.db.csOpen("Content", ("ContentTableID=" + TableID), ,, ,, ,, "ContentControlID");
                        while (cpCore.db.csOk(CSPointer)) {
                            ChildCID = cpCore.db.csGetInteger(CSPointer, "ID");
                            if (Models.Complex.cdefModel.isWithinContent(cpCore, ChildCID, LimitContentSelectToThisID)) {
                                if ((cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) || cpCore.doc.authContext.isAuthenticatedContentManager(cpCore, Models.Complex.cdefModel.getContentNameByID(cpCore, ChildCID)))) {
                                    CIDList = (CIDList + ("," + ChildCID));
                                }
                                
                            }
                            
                            cpCore.db.csGoNext(CSPointer);
                        }
                        
                        cpCore.db.csClose(CSPointer);
                        if ((CIDList != "")) {
                            CIDList = CIDList.Substring(1);
                            HTMLFieldString = cpCore.html.main_GetFormInputSelect2("ContentControlID", FieldValueInteger, "Content", ("id in (" 
                                            + (CIDList + ")")), "", "", IsEmptyList);
                            FieldHelp = (FieldHelp + @" (Only administrators have access to this control. Changing the Controlling Content allows you to change who can author the record, as well as how it is edited. This record includes a Parent field, so your choices for controlling content are limited to those compatible with the parent of this record.)");
                        }
                        
                    }
                    
                }
                
            }
            
            if ((HTMLFieldString == "")) {
                HTMLFieldString = editRecord.contentControlId_Name;
                // HTMLFieldString = models.complex.cdefmodel.getContentNameByID(cpcore,EditRecord.ContentID)
            }
            
            FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Controlling Content", FieldHelp, FieldRequired, false, ""));
            // 
            //  ----- Created By
            // 
            FieldHelp = "The people account of the user who created this record.";
            if ((editRecord.id == 0)) {
                HTMLFieldString = "(available after save)";
            }
            else {
                FieldValueInteger = editRecord.createByMemberId;
                if ((FieldValueInteger == 0)) {
                    HTMLFieldString = "unknown";
                }
                else {
                    CSPointer = cpCore.db.cs_open2("people", FieldValueInteger, true);
                    if (!cpCore.db.csOk(CSPointer)) {
                        HTMLFieldString = "unknown";
                    }
                    else {
                        HTMLFieldString = cpCore.db.csGet(CSPointer, "name");
                    }
                    
                    cpCore.db.csClose(CSPointer);
                }
                
            }
            
            HTMLFieldString = cpCore.html.html_GetFormInputText2("ignore", HTMLFieldString, ,, ,, true);
            FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Created By", FieldHelp, FieldRequired, false, ""));
            // 
            //  ----- Created Date
            // 
            FieldHelp = "The date and time when this record was originally created.";
            if ((editRecord.id == 0)) {
                HTMLFieldString = "(available after save)";
            }
            else {
                HTMLFieldString = genericController.encodeText(genericController.EncodeDate(editRecord.dateAdded));
                if ((HTMLFieldString == "12:00:00 AM")) {
                    HTMLFieldString = "unknown";
                }
                
            }
            
            HTMLFieldString = cpCore.html.html_GetFormInputText2("ignore", HTMLFieldString, ,, ,, true);
            FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Created Date", FieldHelp, FieldRequired, false, ""));
            // 
            //  ----- Modified By
            // 
            FieldHelp = "The people account of the last user who modified this record.";
            if ((editRecord.id == 0)) {
                HTMLFieldString = "(available after save)";
            }
            else {
                FieldValueInteger = editRecord.modifiedByMemberID;
                HTMLFieldString = "unknown";
                if ((FieldValueInteger > 0)) {
                    CSPointer = cpCore.db.cs_open2("people", FieldValueInteger, true, ,, "name");
                    if (cpCore.db.csOk(CSPointer)) {
                        HTMLFieldString = cpCore.db.csGet(CSPointer, "name");
                    }
                    
                    cpCore.db.csClose(CSPointer);
                }
                
            }
            
            HTMLFieldString = cpCore.html.html_GetFormInputText2("ignore", HTMLFieldString, ,, ,, true);
            FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Modified By", FieldHelp, FieldRequired, false, ""));
            // 
            //  ----- Modified Date
            // 
            FieldHelp = "The date and time when this record was last modified";
            if ((editRecord.id == 0)) {
                HTMLFieldString = "(available after save)";
            }
            else {
                HTMLFieldString = genericController.encodeText(genericController.EncodeDate(editRecord.modifiedDate));
                if ((HTMLFieldString == "12:00:00 AM")) {
                    HTMLFieldString = "unknown";
                }
                
            }
            
            HTMLFieldString = cpCore.html.html_GetFormInputText2("ignore", HTMLFieldString, ,, ,, true);
            FastString.Add(Adminui.GetEditRow(HTMLFieldString, "Modified Date", FieldHelp, false, false, ""));
            s = ("" 
                        + (Adminui.EditTableOpen 
                        + (FastString.Text 
                        + (Adminui.EditTableClose 
                        + (hiddenInputs + "")))));
            GetForm_Edit_Control = Adminui.GetEditPanel(!allowAdminTabs, "Control Information", "", s);
            EditSectionPanelCount = (EditSectionPanelCount + 1);
            FastString = null;
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
        ErrorTrap:
            FastString = null;
            handleLegacyClassError3("GetForm_Edit_Control");
        }
        
        // 
        // ========================================================================
        //    Display field in the admin/edit
        // ========================================================================
        // 
        private string GetForm_Edit_SiteProperties(Models.Complex.cdefModel adminContent, editRecordClass editRecord) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_SiteProperties")
            // 
            string ExpandedSelector = "";
            string ignore = "";
            string OptionCaption;
            string OptionValue;
            string OptionValue_AddonEncoded;
            int OptionPtr;
            int OptionCnt;
            string[] OptionValues;
            string OptionSuffix = "";
            string LCaseOptionDefault;
            int Pos;
            bool Checked;
            int ParentID;
            int ParentCID;
            string Criteria;
            int RootCID;
            string SQL;
            int TableID;
            int TableName;
            int ChildCID;
            string CIDList;
            string TableName2;
            string RecordContentName;
            bool HasParentID;
            int CS;
            string HTMLFieldString;
            //  converted array to dictionary - Dim FieldPointer As Integer
            int CSPointer;
            int RecordID;
            stringBuilderLegacyController FastString;
            int FieldValueInteger;
            bool FieldRequired;
            string FieldHelp;
            string AuthoringStatusMessage;
            string Delimiter;
            string Copy = "";
            adminUIController Adminui = new adminUIController(cpCore);
            // 
            int FieldPtr;
            string SitePropertyName;
            string SitePropertyValue;
            string selector;
            string FieldName;
            // 
            FastString = new stringBuilderLegacyController();
            // 
            SitePropertyName = "";
            SitePropertyValue = "";
            selector = "";
            foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in adminContent.fields) {
                Models.Complex.CDefFieldModel field = keyValuePair.Value;
                // 
                FieldName = field.nameLc;
                if ((genericController.vbLCase(FieldName) == "name")) {
                    SitePropertyName = genericController.encodeText(editRecord.fieldsLc(field.nameLc).value);
                }
                else if ((FieldName.ToLower() == "fieldvalue")) {
                    SitePropertyValue = genericController.encodeText(editRecord.fieldsLc(field.nameLc).value);
                }
                else if ((FieldName.ToLower() == "selector")) {
                    selector = genericController.encodeText(editRecord.fieldsLc(field.nameLc).value);
                }
                
            }
            
            if ((SitePropertyName == "")) {
                HTMLFieldString = "This Site Property is not defined";
            }
            else {
                HTMLFieldString = cpCore.html.html_GetFormInputHidden("name", SitePropertyName);
                Dictionary<string, string> instanceOptions = new Dictionary<string, string>();
                Dictionary<string, string> addonInstanceProperties = new Dictionary<string, string>();
                instanceOptions.Add(SitePropertyName, SitePropertyValue);
                cpCore.addon.buildAddonOptionLists(addonInstanceProperties, ExpandedSelector, (SitePropertyName + ("=" + selector)), instanceOptions, "0", true);
                // --------------
                Pos = genericController.vbInstr(1, ExpandedSelector, "[");
                if ((Pos != 0)) {
                    // 
                    //  List of Options, might be select, radio or checkbox
                    // 
                    LCaseOptionDefault = genericController.vbLCase(ExpandedSelector.Substring(0, (Pos - 1)));
                    LCaseOptionDefault = genericController.decodeNvaArgument(LCaseOptionDefault);
                    ExpandedSelector = ExpandedSelector.Substring(Pos);
                    Pos = genericController.vbInstr(1, ExpandedSelector, "]");
                    if ((Pos > 0)) {
                        if ((Pos < ExpandedSelector.Length)) {
                            OptionSuffix = genericController.vbLCase(ExpandedSelector.Substring(Pos).Trim());
                        }
                        
                        ExpandedSelector = ExpandedSelector.Substring(0, (Pos - 1));
                    }
                    
                    OptionValues = ExpandedSelector.Split("|");
                    HTMLFieldString = "";
                    OptionCnt = (UBound(OptionValues) + 1);
                    for (OptionPtr = 0; (OptionPtr 
                                <= (OptionCnt - 1)); OptionPtr++) {
                        OptionValue_AddonEncoded = OptionValues[OptionPtr].Trim();
                        if ((OptionValue_AddonEncoded != "")) {
                            Pos = genericController.vbInstr(1, OptionValue_AddonEncoded, ":");
                            if ((Pos == 0)) {
                                OptionValue = genericController.decodeNvaArgument(OptionValue_AddonEncoded);
                                OptionCaption = OptionValue;
                            }
                            else {
                                OptionCaption = genericController.decodeNvaArgument(OptionValue_AddonEncoded.Substring(0, (Pos - 1)));
                                OptionValue = genericController.decodeNvaArgument(OptionValue_AddonEncoded.Substring(Pos));
                            }
                            
                            switch (OptionSuffix) {
                                case "checkbox":
                                    if ((genericController.vbInstr(1, ("," 
                                                    + (LCaseOptionDefault + ",")), ("," 
                                                    + (genericController.vbLCase(OptionValue) + ","))) != 0)) {
                                        HTMLFieldString = (HTMLFieldString + ("<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" 
                                                    + (SitePropertyName 
                                                    + (OptionPtr + ("\" value=\"" 
                                                    + (OptionValue + ("\" checked=\"checked\">" 
                                                    + (OptionCaption + "</div>"))))))));
                                    }
                                    else {
                                        HTMLFieldString = (HTMLFieldString + ("<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" 
                                                    + (SitePropertyName 
                                                    + (OptionPtr + ("\" value=\"" 
                                                    + (OptionValue + ("\" >" 
                                                    + (OptionCaption + "</div>"))))))));
                                    }
                                    
                                    break;
                                case "radio":
                                    if ((genericController.vbLCase(OptionValue) == LCaseOptionDefault)) {
                                        HTMLFieldString = (HTMLFieldString + ("<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" 
                                                    + (SitePropertyName + ("\" value=\"" 
                                                    + (OptionValue + ("\" checked=\"checked\" >" 
                                                    + (OptionCaption + "</div>")))))));
                                    }
                                    else {
                                        HTMLFieldString = (HTMLFieldString + ("<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" 
                                                    + (SitePropertyName + ("\" value=\"" 
                                                    + (OptionValue + ("\" >" 
                                                    + (OptionCaption + "</div>")))))));
                                    }
                                    
                                    break;
                                case LCaseOptionDefault:
                                    HTMLFieldString = (HTMLFieldString + ("<option value=\"" 
                                                + (OptionValue + ("\" selected>" 
                                                + (OptionCaption + "</option>")))));
                                    break;
                                default:
                                    HTMLFieldString = (HTMLFieldString + ("<option value=\"" 
                                                + (OptionValue + ("\">" 
                                                + (OptionCaption + "</option>")))));
                                    break;
                            }
                        }
                        
                    }
                    
                    switch (OptionSuffix) {
                        case "checkbox":
                            Copy = (Copy + ("<input type=\"hidden\" name=\"" 
                                        + (SitePropertyName + ("CheckBoxCnt\" value=\"" 
                                        + (OptionCnt + "\" >")))));
                            break;
                        case "radio":
                            break;
                        default:
                            HTMLFieldString = ("<select name=\"" 
                                        + (SitePropertyName + ("\">" 
                                        + (HTMLFieldString + "</select>"))));
                            break;
                    }
                }
                else {
                    // 
                    //  Create Text HTMLFieldString
                    // 
                    selector = genericController.decodeNvaArgument(selector);
                    HTMLFieldString = cpCore.html.html_GetFormInputText2(SitePropertyName, selector, 1, 20);
                }
                
                // --------------
                // HTMLFieldString = cpCore.main_GetFormInputText2( genericController.vbLCase(FieldName), VAlue)
            }
            
            FastString.Add(Adminui.GetEditRow(HTMLFieldString, SitePropertyName, "", false, false, ""));
            GetForm_Edit_SiteProperties = Adminui.GetEditPanel(!allowAdminTabs, "Control Information", "", (Adminui.EditTableOpen 
                            + (FastString.Text + Adminui.EditTableClose)));
            EditSectionPanelCount = (EditSectionPanelCount + 1);
            FastString = null;
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
        ErrorTrap:
            FastString = null;
            handleLegacyClassError3("GetForm_Edit_SiteProperties");
        }
    }
}