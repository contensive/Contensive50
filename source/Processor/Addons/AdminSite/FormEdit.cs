
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.constants;
using Contensive.Processor.Models.Domain;
using Contensive.Addons.Tools;
using static Contensive.Processor.AdminUIController;
//
namespace Contensive.Addons.AdminSite {
    public class FormEdit {
        //
        // ====================================================================================================
        /// <summary>
        /// Create the tabs for editing a record
        /// </summary>
        /// <param name="adminData.content"></param>
        /// <param name="editRecord"></param>
        /// <returns></returns>
        public static string get( CoreController core, AdminDataModel adminData) {
            string returnHtml = "";
            try {
                bool AllowajaxTabs = (core.siteProperties.getBoolean("AllowAjaxEditTabBeta", false));
                var adminMenu = new AdminMenuController();
                //
                if ((core.doc.debug_iUserError != "") & adminData.editRecord.Loaded) {
                    //
                    // block load if there was a user error and it is already loaded (assume error was from response )
                } else if (adminData.adminContent.id <= 0) {
                    //
                    // Invalid Content
                    Processor.Controllers.ErrorController.addUserError(core, "There was a problem identifying the content you requested. Please return to the previous form and verify your selection.");
                    return "";
                } else if (adminData.editRecord.Loaded & !adminData.editRecord.Saved) {
                    //
                    //   File types need to be reloaded from the Db, because...
                    //       LoadDb - sets them to the path-page
                    //       LoadResponse - sets the blank if no change, filename if there is an upload
                    //       SaveEditRecord - if blank, no change. If a filename it saves the uploaded file
                    //       GetForm_Edit - expects the Db value to be in EditRecordValueVariants (path-page)
                    //
                    // xx This was added to bypass the load for the editrefresh case (reload the response so the editor preference can change)
                    // xx  I do not know why the following section says "reload even if it is loaded", but lets try this
                    //
                    foreach (var keyValuePair in adminData.adminContent.fields) {
                        CDefFieldModel field = keyValuePair.Value;
                        switch (field.fieldTypeId) {
                            case FieldTypeIdFile:
                            case FieldTypeIdFileImage:
                                adminData.editRecord.fieldsLc[field.nameLc].value = adminData.editRecord.fieldsLc[field.nameLc].dbValue;
                                break;
                        }
                    }
                } else {
                    //
                    // otherwise, load the record, even if it was loaded during a previous form process
                    adminData.LoadEditRecord(core,true);
                    // -- allow for record to have no content control id
                    //if (adminInfo.editRecord.contentControlId == 0) {
                    //    if (core.doc.debug_iUserError != "") {
                    //        //
                    //        // known user error, just return
                    //    } else {
                    //        //
                    //        // unknown error, set userError and return
                    //        ErrorController.addUserError(core, "There was an unknown error in your request for data. Please let the site administrator know.");
                    //    }
                    //    return "";
                    //}
                }
                //
                // Test if this editors has access to this record

                if (!AdminDataModel.userHasContentAccess(core, ((adminData.editRecord.contentControlId.Equals(0)) ? adminData.adminContent.id : adminData.editRecord.contentControlId))) {
                    Processor.Controllers.ErrorController.addUserError(core, "Your account on this system does not have access rights to edit this content.");
                    return "";
                }
                //
                // Setup Edit Referer
                string EditReferer = core.docProperties.getText(RequestNameEditReferer);
                if (string.IsNullOrEmpty(EditReferer)) {
                    EditReferer = core.webServer.requestReferer;
                    if (!string.IsNullOrEmpty(EditReferer)) {
                        //
                        // special case - if you are coming from the advanced search, go back to the list page
                        EditReferer = GenericController.vbReplace(EditReferer, "&af=39", "");
                        //
                        // if referer includes AdminWarningMsg (admin hint message), remove it -- this edit may fix the problem
                        int Pos = EditReferer.IndexOf("AdminWarningMsg=");
                        if (Pos >= 0) {
                            EditReferer = EditReferer.Left(Pos - 2);
                        }
                    }
                }
                core.doc.addRefreshQueryString(RequestNameEditReferer, EditReferer);
                //
                // Print common form elements
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                Stream.Add(GetForm_EditFormStart(core,adminData, AdminFormEdit));
                bool IsLandingPageParent = false;
                int TemplateIDForStyles = 0;
                bool IsTemplateTable = (adminData.adminContent.tableName.ToLower() == Processor.Models.Db.PageTemplateModel.contentTableName);
                bool IsPageContentTable = (adminData.adminContent.tableName.ToLower() == Processor.Models.Db.PageContentModel.contentTableName);
                bool IsEmailTable = (adminData.adminContent.tableName.ToLower() == Processor.Models.Db.EmailModel.contentTableName);
                int emailIdForStyles = IsEmailTable ? adminData.editRecord.id : 0;
                bool IsLandingPage = false;
                bool IsRootPage = false;
                if (IsPageContentTable && (adminData.editRecord.id != 0)) {
                    //
                    // landing page case
                    if (core.siteProperties.landingPageID != 0) {
                        IsLandingPage = (adminData.editRecord.id == core.siteProperties.landingPageID);
                        IsRootPage = IsPageContentTable && (adminData.editRecord.parentID == 0);
                    }
                }
                //
                //bool IsLandingSection = false;
                //bool IsLandingPageTemp = false;
                ////
                //// ----- special case messages
                ////
                //string CustomDescription = "";
                //if (IsLandingSection) {
                //    CustomDescription = "<div>This is the default Landing Section for this website. This section is displayed when no specific page is requested. It should not be deleted, renamed, marked inactive, blocked or hidden.</div>";
                //} else if (IsLandingPageTemp) {
                //    CustomDescription = "<div>This page is being used as the default Landing Page for this website, although it has not been set. This may be because a landing page has not been created, or it has been deleted. To make this page the permantent landing page, check the appropriate box in the control tab.</div>";
                //} else if (IsLandingPage) {
                //    CustomDescription = "<div>This is the default Landing Page for this website. It should not be deleted. You can not mark this record inactive, or use the Publish Date, Expire Date or Archive Date features.</div>";
                //} else if (IsLandingPageParent) {
                //    CustomDescription = "<div>This page is a parent of the default Landing Page for this website. It should not be deleted. You can not mark this record inactive, or use the Publish Date, Expire Date or Archive Date features.</div>";
                //} else if (IsRootPage) {
                //    CustomDescription = "<div>This page is a Root Page. A Root Page is the primary page of a section. If you delete or inactivate this page, the section will create a new blank page in its place.</div>";
                //}
                //
                // ----- Determine TemplateIDForStyles
                if (IsTemplateTable) {
                    TemplateIDForStyles = adminData.editRecord.id;
                } else if (IsPageContentTable) {
                    //Call core.pages.getPageArgs(adminInfo.editRecord.id, false, False, ignoreInteger, TemplateIDForStyles, ignoreInteger, IgnoreString, IgnoreBoolean, ignoreInteger, IgnoreBoolean, "")
                }
                var headerInfo = new RecordEditHeaderInfoClass() {
                    recordId = adminData.editRecord.id,
                    //recordAddedById = adminInfo.editRecord.createdBy.id,
                    //recordDateAdded = adminInfo.editRecord.dateAdded,
                    //recordDateModified = adminInfo.editRecord.modifiedDate,
                    recordLockById = adminData.editRecord.EditLockMemberID,
                    recordLockExpiresDate = adminData.editRecord.EditLockExpires,
                    //recordModifiedById = adminInfo.editRecord.modifiedBy.id,
                    recordName = adminData.editRecord.nameLc
                };
                string titleBarDetails = AdminUIController.getEditForm_TitleBarDetails(core, headerInfo, adminData.editRecord);
                //
                // ----- determine access details
                //
                bool allowCMEdit = false;
                bool allowCMAdd = false;
                bool allowCMDelete = false;
                core.session.getContentAccessRights(core, adminData.adminContent.name, ref allowCMEdit, ref allowCMAdd, ref allowCMDelete);
                bool allowAdd = adminData.adminContent.allowAdd && allowCMAdd;
                bool AllowDelete = adminData.adminContent.allowDelete & allowCMDelete & (adminData.editRecord.id != 0);
                bool allowSave = allowCMEdit;
                bool AllowRefresh = true;
                //
                // ----- custom fieldEditors
                //
                //
                //   Editor Preference
                //   any addon can be an editor for a fieldtype with a checkbox in the addon
                //   the editor in any field can be over-ridden by just a single member with a popup next to the editor
                //       that popup (fancybox) sets the hidden fieldEditorPreference to fieldid:addonid and submits the form
                //       the edit form does a refresh action after setting the members property "editorPreferencesForContent:99"
                //   if no editor preference, the default editor is used from a drop-down selection in fieldtypes
                //   if nothing in field types, Contensive handles it internally
                //
                Stream.Add("\r<input type=\"hidden\" name=\"fieldEditorPreference\" id=\"fieldEditorPreference\" value=\"\">");
                string fieldEditorList = EditorController.getFieldTypeDefaultEditorAddonIdList(core);
                string[] fieldTypeDefaultEditors = fieldEditorList.Split(',');
                //
                // load user's editor preferences to fieldEditorPreferences() - this is the editor this user has picked when there are >1
                //   fieldId:addonId,fieldId:addonId,etc
                //   with custom FancyBox form in edit window with button "set editor preference"
                //   this button causes a 'refresh' action, reloads fields with stream without save
                //
                string fieldEditorPreferencesList = core.userProperty.getText("editorPreferencesForContent:" + adminData.adminContent.id, "");
                //
                // add the addon editors assigned to each field
                // !!!!! this should be added to metaData load
                //
                string SQL = "select"
                    + " f.id,f.editorAddonID"
                    + " from ccfields f"
                    + " where"
                    + " f.ContentID=" + adminData.adminContent.id + " and f.editorAddonId is not null";
                DataTable dt = core.db.executeQuery(SQL);

                string[,] Cells = core.db.convertDataTabletoArray(dt);
                for (int Ptr = 0; Ptr < Cells.GetLength(1); Ptr++) {
                    int fieldId = GenericController.encodeInteger(Cells[0, Ptr]);
                    if (fieldId > 0) {
                        fieldEditorPreferencesList = fieldEditorPreferencesList + "," + fieldId + ":" + Cells[1, Ptr];
                    }
                }
                //
                // load fieldEditorOptions - these are all the editors available for each field
                //
                Dictionary<string, int> fieldEditorOptions = new Dictionary<string, int>();
                int fieldEditorOptionCnt = 0;
                SQL = "select r.contentFieldTypeId,a.Id"
                    + " from ccAddonContentFieldTypeRules r"
                    + " left join ccaggregatefunctions a on a.id=r.addonid"
                    + " where (r.active<>0)and(a.active<>0)and(a.id is not null) order by r.contentFieldTypeID";
                dt = core.db.executeQuery(SQL);
                Cells = core.db.convertDataTabletoArray(dt);
                fieldEditorOptionCnt = Cells.GetUpperBound(1) + 1;
                for (int Ptr = 0; Ptr < fieldEditorOptionCnt; Ptr++) {
                    int fieldId = GenericController.encodeInteger(Cells[0, Ptr]);
                    if ((fieldId > 0) && (!fieldEditorOptions.ContainsKey(fieldId.ToString()))) {
                        fieldEditorOptions.Add(fieldId.ToString(), GenericController.encodeInteger(Cells[1, Ptr]));
                    }
                }
                //
                // ----- determine contentType for editor
                //
                ContentTypeEnum ContentType;
                if (GenericController.vbLCase(adminData.adminContent.name) == "email templates") {
                    ContentType = ContentTypeEnum.contentTypeEmailTemplate;
                } else if (GenericController.vbLCase(adminData.adminContent.tableName) == "cctemplates") {
                    ContentType = ContentTypeEnum.contentTypeWebTemplate;
                } else if (GenericController.vbLCase(adminData.adminContent.tableName) == "ccemail") {
                    ContentType = ContentTypeEnum.contentTypeEmail;
                } else {
                    ContentType = ContentTypeEnum.contentTypeWeb;
                }
                //
                //-----Create edit page
                string styleOptionList = "";
                string editorAddonListJSON = core.html.getWysiwygAddonList(ContentType);
                string styleList = "";
                string adminContentTableNameLower = adminData.adminContent.tableName.ToLower();
                //
                LogController.logTrace(core, "getFormEdit, adminInfo.editRecord.contentControlId [" + adminData.editRecord.contentControlId + "]");
                //
                if (adminContentTableNameLower == PersonModel.contentTableName.ToLower()) {
                    //
                    // -- people
                    if (!(core.session.isAuthenticatedAdmin(core))) {
                        //
                        // Must be admin
                        Stream.Add(ErrorController.get(core, "This edit form requires administrator access.", ""));
                    } else {
                        string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                            allowActivate = false,
                            allowAdd = (allowAdd && adminData.adminContent.allowAdd & adminData.editRecord.AllowInsert),
                            allowCancel = adminData.editRecord.AllowCancel,
                            allowCreateDuplicate = (allowSave && adminData.editRecord.AllowSave & (adminData.editRecord.id != 0)),
                            allowDelete = AllowDelete && adminData.editRecord.AllowDelete,
                            allowMarkReviewed = false,
                            allowRefresh = AllowRefresh,
                            allowSave = (allowSave && adminData.editRecord.AllowSave),
                            allowSend = false,
                            allowSendTest = false,
                            hasChildRecords = false,
                            isPageContent = false
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getTitleBar(core, GetForm_EditTitle(core,adminData), titleBarDetails));
                        Stream.Add(GetForm_Edit_Tabs(core, adminData, adminMenu, adminData.editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Groups", GroupRuleEditor.GetForm_Edit_MemberGroups(core, adminData), adminData.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Control&nbsp;Info", ControlEditor.get(core, adminData), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) Stream.Add(adminMenu.menuComboTab.GetTabs(core));
                        Stream.Add(EditSectionButtonBar);
                    }
                } else if (adminContentTableNameLower == EmailModel.contentTableName.ToLower()) {
                    //
                    LogController.logTrace(core, "getFormEdit, treat as email, adminContentTableNameLower [" + adminContentTableNameLower + "]");
                    //
                    // -- email
                    bool EmailSubmitted = false;
                    bool EmailSent = false;
                    int SystemEmailCID = CdefController.getContentId(core, "System Email");
                    int ConditionalEmailCID = CdefController.getContentId(core, "Conditional Email");
                    DateTime LastSendTestDate = DateTime.MinValue;
                    bool AllowEmailSendWithoutTest = (core.siteProperties.getBoolean("AllowEmailSendWithoutTest", false));
                    if (adminData.editRecord.fieldsLc.ContainsKey("lastsendtestdate")) {
                        LastSendTestDate = GenericController.encodeDate(adminData.editRecord.fieldsLc["lastsendtestdate"].value);
                    }
                    if (!(core.session.isAuthenticatedAdmin(core))) {
                        //
                        // Must be admin
                        Stream.Add(ErrorController.get(core, "This edit form requires Member Administration access.", "This edit form requires Member Administration access."));
                    } else if (CdefController.isWithinContent(core, adminData.editRecord.contentControlId, SystemEmailCID)) {
                        //
                        LogController.logTrace(core, "getFormEdit, System email");
                        //
                        // System Email
                        EmailSubmitted = false;
                        if (adminData.editRecord.id != 0) {
                            if (adminData.editRecord.fieldsLc.ContainsKey("testmemberid")) {
                                adminData.editRecord.fieldsLc["testmemberid"].value = core.session.user.id;
                            }
                        }
                        string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                            allowActivate = false,
                            allowAdd = (allowAdd && adminData.adminContent.allowAdd & adminData.editRecord.AllowInsert),
                            allowCancel = adminData.editRecord.AllowCancel,
                            allowCreateDuplicate = (allowSave && adminData.editRecord.AllowSave & (adminData.editRecord.id != 0)),
                            allowDelete = AllowDelete && adminData.editRecord.AllowDelete && core.session.isAuthenticatedDeveloper(core),
                            allowMarkReviewed = false,
                            allowRefresh = AllowRefresh,
                            allowSave = (allowSave && adminData.editRecord.AllowSave && (!EmailSubmitted) && (!EmailSent)),
                            allowSend = false,
                            allowSendTest = ((!EmailSubmitted) && (!EmailSent)),
                            hasChildRecords = false,
                            isPageContent = false
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getTitleBar(core, GetForm_EditTitle(core,adminData), titleBarDetails));
                        Stream.Add(GetForm_Edit_Tabs(core,adminData, adminMenu, adminData.editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Send&nbsp;To&nbsp;Groups", EmailRuleEditor.get(core, adminData, adminData.editRecord.Read_Only & (!core.session.isAuthenticatedDeveloper(core))), adminData.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Send&nbsp;To&nbsp;Topics", EmailTopicEditor.get(core, adminData, adminData.editRecord.Read_Only & (!core.session.isAuthenticatedDeveloper(core))), adminData.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Bounce&nbsp;Control", EmailBounceEditor.get(core, adminData), adminData.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Control&nbsp;Info", ControlEditor.get(core, adminData), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) Stream.Add(adminMenu.menuComboTab.GetTabs(core));
                        Stream.Add(EditSectionButtonBar);
                    } else if (CdefController.isWithinContent(core, adminData.editRecord.contentControlId, ConditionalEmailCID)) {
                        //
                        // Conditional Email
                        EmailSubmitted = false;
                        if (adminData.editRecord.id != 0) {
                            if (adminData.editRecord.fieldsLc.ContainsKey("submitted")) EmailSubmitted = GenericController.encodeBoolean(adminData.editRecord.fieldsLc["submitted"].value);
                        }
                        //
                        string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                            allowActivate = !EmailSubmitted & ((LastSendTestDate != DateTime.MinValue) | AllowEmailSendWithoutTest),
                            allowDeactivate = EmailSubmitted,
                            allowAdd = allowAdd && adminData.adminContent.allowAdd & adminData.editRecord.AllowInsert,
                            allowCancel = adminData.editRecord.AllowCancel,
                            allowCreateDuplicate = allowAdd && (adminData.editRecord.id != 0),
                            allowDelete = AllowDelete && adminData.editRecord.AllowDelete && core.session.isAuthenticatedDeveloper(core),
                            allowMarkReviewed = false,
                            allowRefresh = AllowRefresh,
                            allowSave = allowSave && adminData.editRecord.AllowSave && !EmailSubmitted,
                            allowSend = false,
                            allowSendTest = !EmailSubmitted,
                            hasChildRecords = false,
                            isPageContent = false
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getTitleBar(core, GetForm_EditTitle(core,adminData), titleBarDetails));
                        Stream.Add(GetForm_Edit_Tabs(core, adminData, adminMenu, adminData.editRecord.Read_Only || EmailSubmitted, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Condition&nbsp;Groups", EmailRuleEditor.get(core, adminData, adminData.editRecord.Read_Only || EmailSubmitted), adminData.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Bounce&nbsp;Control", EmailBounceEditor.get(core, adminData), adminData.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Control&nbsp;Info", ControlEditor.get(core, adminData), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) Stream.Add(adminMenu.menuComboTab.GetTabs(core));
                        Stream.Add(EditSectionButtonBar);
                    } else {
                        //
                        // Group Email
                        if (adminData.editRecord.id != 0) {
                            EmailSubmitted = encodeBoolean(adminData.editRecord.fieldsLc["submitted"].value);
                            EmailSent = encodeBoolean(adminData.editRecord.fieldsLc["sent"].value);
                        }
                        string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                            allowActivate = false,
                            allowDeactivate = false,
                            allowAdd = allowAdd && adminData.adminContent.allowAdd & adminData.editRecord.AllowInsert,
                            allowCancel = adminData.editRecord.AllowCancel,
                            allowCreateDuplicate = allowAdd && (adminData.editRecord.id != 0),
                            allowDelete = !EmailSubmitted & (AllowDelete && adminData.editRecord.AllowDelete),
                            allowMarkReviewed = false,
                            allowRefresh = AllowRefresh,
                            allowSave = !EmailSubmitted & (allowSave && adminData.editRecord.AllowSave),
                            allowSend = !EmailSubmitted & ((LastSendTestDate != DateTime.MinValue) | AllowEmailSendWithoutTest),
                            allowSendTest = !EmailSubmitted,
                            hasChildRecords = false,
                            isPageContent = false
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getTitleBar(core, GetForm_EditTitle(core,adminData), titleBarDetails));
                        Stream.Add(GetForm_Edit_Tabs(core, adminData, adminMenu, adminData.editRecord.Read_Only | EmailSubmitted || EmailSent, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Send&nbsp;To&nbsp;Groups", EmailRuleEditor.get(core, adminData, adminData.editRecord.Read_Only | EmailSubmitted || EmailSent), adminData.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Send&nbsp;To&nbsp;Topics", EmailTopicEditor.get(core, adminData, adminData.editRecord.Read_Only | EmailSubmitted || EmailSent), adminData.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Bounce&nbsp;Control", EmailBounceEditor.get(core, adminData), adminData.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Control&nbsp;Info", ControlEditor.get(core, adminData), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) Stream.Add(adminMenu.menuComboTab.GetTabs(core));
                        Stream.Add(EditSectionButtonBar);
                    }
                } else if (adminData.adminContent.tableName.ToLower() == ContentModel.contentTableName.ToLower()) {
                    if (!(core.session.isAuthenticatedAdmin(core))) {
                        //
                        // Must be admin
                        //
                        Stream.Add(ErrorController.get(core, "This edit form requires Member Administration access.", "This edit form requires Member Administration access."));
                    } else {
                        string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                            allowActivate = false,
                            allowAdd = (allowAdd && adminData.adminContent.allowAdd & adminData.editRecord.AllowInsert),
                            allowCancel = adminData.editRecord.AllowCancel,
                            allowCreateDuplicate = (allowSave && adminData.editRecord.AllowSave & (adminData.editRecord.id != 0)),
                            allowDelete = AllowDelete && adminData.editRecord.AllowDelete,
                            allowMarkReviewed = false,
                            allowRefresh = AllowRefresh,
                            allowSave = (allowSave && adminData.editRecord.AllowSave),
                            allowSend = false,
                            allowSendTest = false,
                            hasChildRecords = false,
                            isPageContent = false
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getTitleBar(core, GetForm_EditTitle(core,adminData), titleBarDetails));
                        Stream.Add(GetForm_Edit_Tabs(core, adminData, adminMenu, adminData.editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Authoring Permissions", GetForm_Edit_GroupRules(core,adminData), adminData.allowAdminTabs));
                        Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Control&nbsp;Info", ControlEditor.get(core, adminData), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) {
                            Stream.Add(adminMenu.menuComboTab.GetTabs(core));
                            //Call Stream.Add("<div class=""ccPanelBackground"">" & core.main_GetComboTabs() & "</div>")
                        }
                        Stream.Add(EditSectionButtonBar);
                    }
                    //
                } else if (adminContentTableNameLower == PageContentModel.contentTableName.ToLower()) {
                    //
                    // Page Content
                    //
                    int TableID = core.db.getRecordID("Tables", "ccPageContent");
                    string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                        allowActivate = false,
                        allowAdd = (allowAdd && adminData.adminContent.allowAdd & adminData.editRecord.AllowInsert),
                        allowCancel = adminData.editRecord.AllowCancel,
                        allowCreateDuplicate = (allowSave && adminData.editRecord.AllowSave & (adminData.editRecord.id != 0)),
                        allowDelete = AllowDelete && adminData.editRecord.AllowDelete,
                        allowMarkReviewed = false,
                        allowRefresh = AllowRefresh,
                        allowSave = (allowSave && adminData.editRecord.AllowSave),
                        allowSend = false,
                        allowSendTest = false,
                        hasChildRecords = false,
                        isPageContent = false
                    });
                    Stream.Add(EditSectionButtonBar);
                    Stream.Add(AdminUIController.getTitleBar(core, GetForm_EditTitle(core,adminData), titleBarDetails));
                    Stream.Add(GetForm_Edit_Tabs(core, adminData, adminMenu, adminData.editRecord.Read_Only, IsLandingPage || IsLandingPageParent, IsRootPage, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Link Aliases", LinkAliasEditor.GetForm_Edit_LinkAliases(core, adminData, adminData.editRecord.Read_Only), adminData.allowAdminTabs));
                    Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Content Watch", ContentTrackingEditor.get(core, adminData), adminData.allowAdminTabs));
                    Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Control Info", ControlEditor.get(core, adminData), adminData.allowAdminTabs));
                    if (adminData.allowAdminTabs) {
                        Stream.Add(adminMenu.menuComboTab.GetTabs(core));
                    }
                    Stream.Add(EditSectionButtonBar);
                    //else if (adminContentTableNameLower == sectionModel.contentTableName.ToLower()) {
                    //    '
                    //    ' Site Sections
                    //    '
                    //    EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContext.content, editRecord, (Not IsLandingSection) And AllowDelete, allowSave, AllowAdd)
                    //    EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
                    //    Call Stream.Add(EditSectionButtonBar)
                    //    Call Stream.Add(adminUIController.GetTitleBar(core,GetForm_EditTitle(core,adminContext.content, editRecord), HeaderDescription))
                    //    Call Stream.Add(GetForm_Edit_Tabs(core,adminContext.content, editRecord, adminInfo.editRecord.Read_Only, IsLandingSection, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                    //    Call Stream.Add(GetForm_Edit_AddTab(core,"Select Menus", GetForm_Edit_SectionDynamicMenuRules(adminContext.content, editRecord), allowAdminTabs))
                    //    Call Stream.Add(GetForm_Edit_AddTab(core,"Section Blocking", GetForm_Edit_SectionBlockRules(adminContext.content, editRecord), allowAdminTabs))
                    //    Call Stream.Add(GetForm_Edit_AddTab(core,"Control Info", GetForm_Edit_Control(adminContext.content, editRecord), allowAdminTabs))
                    //    If allowAdminTabs Then
                    //        Call Stream.Add(core.htmlDoc.menu_GetComboTabs())
                    //        'Call Stream.Add("<div class=""ccPanelBackground"">" & core.main_GetComboTabs() & "</div>")
                    //    End If
                    //    Call Stream.Add(EditSectionButtonBar)
                    //Case "CCDYNAMICMENUS"
                    //    '
                    //    ' Edit Dynamic Sections
                    //    '
                    //    EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContext.content, editRecord, AllowDelete, allowSave, AllowAdd)
                    //    EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
                    //    Call Stream.Add(EditSectionButtonBar)
                    //    Call Stream.Add(adminUIController.GetTitleBar(core,GetForm_EditTitle(core,adminContext.content, editRecord), HeaderDescription))
                    //    Call Stream.Add(GetForm_Edit_Tabs(core,adminContext.content, editRecord, adminInfo.editRecord.Read_Only, False, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                    //    Call Stream.Add(GetForm_Edit_AddTab(core,"Select Sections", GetForm_Edit_DynamicMenuSectionRules(adminContext.content, editRecord), allowAdminTabs))
                    //    Call Stream.Add(GetForm_Edit_AddTab(core,"Control Info", GetForm_Edit_Control(adminContext.content, editRecord), allowAdminTabs))
                    //    If allowAdminTabs Then
                    //        Call Stream.Add(core.htmlDoc.menu_GetComboTabs())
                    //        'Call Stream.Add("<div class=""ccPanelBackground"">" & core.main_GetComboTabs() & "</div>")
                    //    End If
                    //    Call Stream.Add(EditSectionButtonBar)

                    //} else if (adminContentTableNameLower == libraryFoldersModel.contentTableName.ToLower()) {
                    //    //
                    //    // Library Folders
                    //    //
                    //    EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContext.content, editRecord, AllowDelete, allowSave, AllowAdd);
                    //    EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord);
                    //    Stream.Add(EditSectionButtonBar);
                    //    Stream.Add(adminUIController.GetTitleBar(core, GetForm_EditTitle(core,adminContext.content, editRecord), HeaderDescription));
                    //    Stream.Add(GetForm_Edit_Tabs(core,adminContext.content, editRecord, adminInfo.editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    //    Stream.Add(GetForm_Edit_AddTab(core,"Authoring Access", GetForm_Edit_LibraryFolderRules(adminContext.content, editRecord), allowAdminTabs));
                    //    Stream.Add(GetForm_Edit_AddTab(core,"Control Info", GetForm_Edit_Control(adminContext.content, editRecord), allowAdminTabs));
                    //    if (allowAdminTabs) {
                    //        Stream.Add(adminMenu.menuComboTab.GetTabs(core));
                    //    }
                    //    Stream.Add(EditSectionButtonBar);
                    //
                    //ORIGINAL LINE: Case genericController.vbUCase("ccGroups")
                    //} else if (adminContentTableNameLower == groupModel.contentTableName.ToLower()) {
                    //    //
                    //    // Groups
                    //    //
                    //    string EditSectionButtonBar = adminUIController.getButtonBarForEdit(core, new editButtonBarInfoClass() {
                    //        allowActivate = false,
                    //        allowAdd = (allowAdd && adminContext.adminContent.allowAdd & adminInfo.editRecord.AllowInsert),
                    //        allowCancel = adminInfo.editRecord.AllowCancel,
                    //        allowCreateDuplicate = (allowSave && adminInfo.editRecord.AllowSave & (adminInfo.editRecord.id != 0)),
                    //        allowDelete = AllowDelete && adminInfo.editRecord.AllowDelete,
                    //        allowMarkReviewed = false,
                    //        allowRefresh = AllowRefresh,
                    //        allowSave = (allowSave && adminInfo.editRecord.AllowSave),
                    //        allowSend = false,
                    //        allowSendTest = false,
                    //        hasChildRecords = false,
                    //        isPageContent = false
                    //    });
                    //    Stream.Add(EditSectionButtonBar);
                    //    Stream.Add(adminUIController.GetTitleBar(core, GetForm_EditTitle(core,adminContext), titleBarDetails));
                    //    Stream.Add(GetForm_Edit_Tabs(core,adminContext, adminInfo.editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    //    Stream.Add(GetForm_Edit_AddTab(core,"Authoring Permissions", GetForm_Edit_ContentGroupRules(adminContext), adminContext.allowAdminTabs));
                    //    Stream.Add(GetForm_Edit_AddTab(core,"Content Watch", GetForm_Edit_ContentTracking(adminContext), adminContext.allowAdminTabs));
                    //    Stream.Add(GetForm_Edit_AddTab(core,"Control Info", GetForm_Edit_Control(adminContext), adminContext.allowAdminTabs));
                    //    if (adminContext.allowAdminTabs) {
                    //        Stream.Add(adminMenu.menuComboTab.GetTabs(core));
                    //    }
                    //    Stream.Add(EditSectionButtonBar);
                    //} else if (adminContentTableNameLower == layoutModel.contentTableName.ToLower()) {
                    //    //
                    //    // LAYOUTS
                    //    string EditSectionButtonBar = adminUIController.getButtonBarForEdit(core, new editButtonBarInfoClass() {
                    //        allowActivate = false,
                    //        allowAdd = (allowAdd && adminContext.adminContent.allowAdd & adminInfo.editRecord.AllowInsert),
                    //        allowCancel = adminInfo.editRecord.AllowCancel,
                    //        allowCreateDuplicate = (allowSave && adminInfo.editRecord.AllowSave & (adminInfo.editRecord.id != 0)),
                    //        allowDelete = AllowDelete && adminInfo.editRecord.AllowDelete,
                    //        allowMarkReviewed = false,
                    //        allowRefresh = AllowRefresh,
                    //        allowSave = (allowSave && adminInfo.editRecord.AllowSave),
                    //        allowSend = false,
                    //        allowSendTest = false,
                    //        hasChildRecords = false,
                    //        isPageContent = false
                    //    });
                    //    Stream.Add(EditSectionButtonBar);
                    //    Stream.Add(adminUIController.GetTitleBar(core, GetForm_EditTitle(core,adminContext), titleBarDetails));
                    //    Stream.Add(GetForm_Edit_Tabs(core,adminContext, adminInfo.editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    //    Stream.Add(GetForm_Edit_AddTab(core,"Reports", GetForm_Edit_LayoutReports(adminContext), adminContext.allowAdminTabs));
                    //    Stream.Add(GetForm_Edit_AddTab(core,"Control Info", GetForm_Edit_Control(adminContext), adminContext.allowAdminTabs));
                    //    if (adminContext.allowAdminTabs) {
                    //        Stream.Add(adminMenu.menuComboTab.GetTabs(core));
                    //    }
                    //    Stream.Add(EditSectionButtonBar);
                } else {
                    //
                    // All other tables (User definined)
                    bool IsPageContent = CdefController.isWithinContent(core, adminData.adminContent.id, CdefController.getContentId(core, "Page Content"));
                    bool HasChildRecords = CdefController.isContentFieldSupported(core, adminData.adminContent.name, "parentid");
                    bool AllowMarkReviewed = core.db.isSQLTableField("default", adminData.adminContent.tableName, "DateReviewed");
                    string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                        allowActivate = false,
                        allowAdd = (allowAdd && adminData.adminContent.allowAdd & adminData.editRecord.AllowInsert),
                        allowCancel = adminData.editRecord.AllowCancel,
                        allowCreateDuplicate = (allowSave && adminData.editRecord.AllowSave & (adminData.editRecord.id != 0)),
                        allowDelete = AllowDelete && adminData.editRecord.AllowDelete,
                        allowMarkReviewed = AllowMarkReviewed,
                        allowRefresh = AllowRefresh,
                        allowSave = (allowSave && adminData.editRecord.AllowSave),
                        allowSend = false,
                        allowSendTest = false,
                        hasChildRecords = HasChildRecords,
                        isPageContent = IsPageContent
                    });
                    Stream.Add(EditSectionButtonBar);
                    Stream.Add(AdminUIController.getTitleBar(core, GetForm_EditTitle(core,adminData), titleBarDetails));
                    Stream.Add(GetForm_Edit_Tabs(core, adminData, adminMenu, adminData.editRecord.Read_Only, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Content Watch", ContentTrackingEditor.get(core, adminData), adminData.allowAdminTabs));
                    Stream.Add(GetForm_Edit_AddTab(core, adminMenu, "Control Info", ControlEditor.get(core, adminData), adminData.allowAdminTabs));
                    if (adminData.allowAdminTabs) Stream.Add(adminMenu.menuComboTab.GetTabs(core));
                    Stream.Add(EditSectionButtonBar);
                }
                Stream.Add("</form>");
                returnHtml = Stream.Text;
                if (adminData.editRecord.id == 0) {
                    core.html.addTitle("Add " + adminData.adminContent.name);
                } else if (adminData.editRecord.nameLc == "") {
                    core.html.addTitle("Edit #" + adminData.editRecord.id + " in " + adminData.editRecord.contentControlId_Name);
                } else {
                    core.html.addTitle("Edit " + adminData.editRecord.nameLc + " in " + adminData.editRecord.contentControlId_Name);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnHtml;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Generate the content of a tab in the Edit Screen
        /// </summary>
        /// <param name="core"></param>
        /// <param name="adminData"></param>
        /// <param name="RecordID"></param>
        /// <param name="ContentID"></param>
        /// <param name="record_readOnly"></param>
        /// <param name="IsLandingPage"></param>
        /// <param name="IsRootPage"></param>
        /// <param name="EditTab"></param>
        /// <param name="EditorContext"></param>
        /// <param name="return_NewFieldList"></param>
        /// <param name="TemplateIDForStyles"></param>
        /// <param name="HelpCnt"></param>
        /// <param name="HelpIDCache"></param>
        /// <param name="helpDefaultCache"></param>
        /// <param name="HelpCustomCache"></param>
        /// <param name="AllowHelpMsgCustom"></param>
        /// <param name="helpIdIndex"></param>
        /// <param name="fieldTypeDefaultEditors"></param>
        /// <param name="fieldEditorPreferenceList"></param>
        /// <param name="styleList"></param>
        /// <param name="styleOptionList"></param>
        /// <param name="emailIdForStyles"></param>
        /// <param name="IsTemplateTable"></param>
        /// <param name="editorAddonListJSON"></param>
        /// <returns></returns>
        public static string GetForm_Edit_Tab(CoreController core, AdminDataModel adminData, int RecordID, int ContentID, bool record_readOnly, bool IsLandingPage, bool IsRootPage, string EditTab, ContentTypeEnum EditorContext, ref string return_NewFieldList, int TemplateIDForStyles, int HelpCnt, int[] HelpIDCache, string[] helpDefaultCache, string[] HelpCustomCache, bool AllowHelpMsgCustom, KeyPtrController helpIdIndex, string[] fieldTypeDefaultEditors, string fieldEditorPreferenceList, string styleList, string styleOptionList, int emailIdForStyles, bool IsTemplateTable, string editorAddonListJSON) {
            string returnHtml = "";
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
                //
                //
                string AjaxQS = null;
                string fancyBoxLinkId = null;
                string fancyBoxContentId = null;
                int fieldTypeDefaultEditorAddonId = 0;
                int fieldIdPos = 0;
                int Pos = 0;
                int editorAddonID = 0;
                bool editorReadOnly = false;
                bool AllowHelpIcon = false;
                int fieldId = 0;
                string LcaseName = null;
                bool IsEmptyList = false;
                string HelpMsgCustom = null;
                string HelpMsgDefault = null;
                //
                string WhyReadOnlyMsg = null;
                bool IsLongHelp = false;
                bool IsEmptyHelp = false;
                string HelpMsg = null;
                int CS = 0;
                string HelpClosedContentID = null;
                string EditorHelp = null;
                string HelpEditorID = null;
                string HelpOpenedReadID = null;
                string HelpOpenedEditID = null;
                string HelpClosedID = null;
                string HelpID = null;
                string HelpMsgClosed = null;
                string HelpMsgOpenedRead = null;
                string HelpMsgOpenedEdit = null;
                //string RecordName = null;
                //string GroupName = null;
                bool IsBaseField = false;
                bool field_readOnly = false;
                string NonEncodedLink = null;
                string EncodedLink = null;
                string fieldCaption = null;
                //string[] lookups = null;
                //int CSPointer = 0;
                string fieldValue_text = null;
                //int FieldValueInteger = 0;
                double FieldValueNumber = 0;
                int fieldTypeId = 0;
                object fieldValue_object = null;
                string RedirectPath = null;
                StringBuilderLegacyController resultBody = new StringBuilderLegacyController();
                int FieldRows = 0;
                string EditorString = null;
                //string MTMContent0 = null;
                //string MTMContent1 = null;
                //string MTMRuleContent = null;
                //string MTMRuleField0 = null;
                //string MTMRuleField1 = null;
                string AlphaSort = null;
                bool needUniqueEmailMessage = false;
                //
                // ----- Open the panel
                if (adminData.adminContent.fields.Count <= 0) {
                    //
                    // There are no visible fiels, return empty
                    LogController.handleError(core, new ApplicationException("There is no metadata for this field."));
                } else {
                    //
                    // ----- Build an index to sort the fields by EditSortOrder
                    Dictionary<string, CDefFieldModel> sortingFields = new Dictionary<string, CDefFieldModel>();
                    foreach (var keyValuePair in adminData.adminContent.fields) {
                        CDefFieldModel field = keyValuePair.Value;
                        if (field.editTabName.ToLower() == EditTab.ToLower()) {
                            if (AdminDataModel.IsVisibleUserField(core, field.adminOnly, field.developerOnly, field.active, field.authorable, field.nameLc, adminData.adminContent.tableName)) {
                                AlphaSort = GenericController.getIntegerString(field.editSortPriority, 10) + "-" + GenericController.getIntegerString(field.id, 10);
                                sortingFields.Add(AlphaSort, field);
                            }
                        }
                    }
                    //
                    // ----- display the record fields
                    //
                    AllowHelpIcon = core.visitProperty.getBoolean("AllowHelpIcon");
                    foreach (var kvp in sortingFields) {
                        CDefFieldModel field = kvp.Value;
                        fieldId = field.id;
                        WhyReadOnlyMsg = "";
                        fieldTypeId = field.fieldTypeId;
                        fieldValue_object = editRecord.fieldsLc[field.nameLc].value;
                        fieldValue_text = GenericController.encodeText(fieldValue_object);
                        FieldRows = 1;
                        string fieldHtmlId = field.nameLc + field.id.ToString();
                        //
                        fieldCaption = field.caption;
                        if (field.uniqueName) {
                            fieldCaption = "&nbsp;**" + fieldCaption;
                        } else {
                            if (field.nameLc.ToLower() == "email") {
                                if ((adminData.adminContent.tableName.ToLower() == "ccmembers") && ((core.siteProperties.getBoolean("allowemaillogin", false)))) {
                                    fieldCaption = "&nbsp;***" + fieldCaption;
                                    needUniqueEmailMessage = true;
                                }
                            }
                        }
                        if (field.required) {
                            fieldCaption = "&nbsp;*" + fieldCaption;
                        }
                        IsBaseField = field.blockAccess; // field renamed
                        adminData.FormInputCount = adminData.FormInputCount + 1;
                        field_readOnly = false;
                        //
                        // Read only Special Cases
                        //
                        if (IsLandingPage) {
                            switch (GenericController.vbLCase(field.nameLc)) {
                                case "active":
                                    //
                                    // if active, it is read only -- if inactive, let them set it active.
                                    //
                                    field_readOnly = (GenericController.encodeBoolean(fieldValue_object));
                                    if (field_readOnly) {
                                        WhyReadOnlyMsg = "&nbsp;(disabled because you can not mark the landing page inactive)";
                                    }
                                    break;
                                case "dateexpires":
                                case "pubdate":
                                case "datearchive":
                                case "blocksection":
                                case "hidemenu":
                                    //
                                    // These fields are read only on landing pages
                                    //
                                    field_readOnly = true;
                                    WhyReadOnlyMsg = "&nbsp;(disabled for the landing page)";
                                    break;
                            }
                        }
                        //
                        if (IsRootPage) {
                            switch (GenericController.vbLCase(field.nameLc)) {
                                case "dateexpires":
                                case "pubdate":
                                case "datearchive":
                                case "archiveparentid":
                                    field_readOnly = true;
                                    WhyReadOnlyMsg = "&nbsp;(disabled for root pages)";
                                    break;
                                case "allowinmenus":
                                case "allowinchildlists":
                                    //FieldValueBoolean = true;
                                    fieldValue_object = "1";
                                    field_readOnly = true;
                                    WhyReadOnlyMsg = "&nbsp;(disabled for root pages)";
                                    break;
                            }
                        }
                        //
                        // Special Case - ccemail table Alloweid should be disabled if siteproperty AllowLinkLogin is false
                        //
                        if (GenericController.vbLCase(adminData.adminContent.tableName) == "ccemail" && GenericController.vbLCase(field.nameLc) == "allowlinkeid") {
                            if (!(core.siteProperties.getBoolean("AllowLinkLogin", true))) {
                                //.ValueVariant = "0"
                                fieldValue_object = "0";
                                field_readOnly = true;
                                //FieldValueBoolean = false;
                                fieldValue_text = "0";
                            }
                        }
                        //EditorStyleModifier = genericController.vbLCase(core.db.getFieldTypeNameFromFieldTypeId(fieldTypeId));
                        EditorString = "";
                        editorReadOnly = (record_readOnly || field.readOnly | (editRecord.id != 0 & field.notEditable) || (field_readOnly));
                        //
                        // Determine the editor: Contensive editor, field type default, or add-on preference
                        //
                        editorAddonID = 0;
                        //editorPreferenceAddonId = 0
                        fieldIdPos = GenericController.vbInstr(1, "," + fieldEditorPreferenceList, "," + fieldId.ToString() + ":");
                        while ((editorAddonID == 0) && (fieldIdPos > 0)) {
                            fieldIdPos = fieldIdPos + 1 + fieldId.ToString().Length;
                            Pos = GenericController.vbInstr(fieldIdPos, fieldEditorPreferenceList + ",", ",");
                            if (Pos > 0) {
                                editorAddonID = GenericController.encodeInteger(fieldEditorPreferenceList.Substring(fieldIdPos - 1, Pos - fieldIdPos));
                                //editorPreferenceAddonId = genericController.EncodeInteger(Mid(fieldEditorPreferenceList, fieldIdPos, Pos - fieldIdPos))
                                //editorAddonID = editorPreferenceAddonId
                            }
                            fieldIdPos = GenericController.vbInstr(fieldIdPos + 1, "," + fieldEditorPreferenceList, "," + fieldId.ToString() + ":");
                        }
                        if (editorAddonID == 0) {
                            fieldTypeDefaultEditorAddonId = GenericController.encodeInteger(fieldTypeDefaultEditors[fieldTypeId]);
                            editorAddonID = fieldTypeDefaultEditorAddonId;
                        }
                        bool useEditorAddon = false;
                        if (editorAddonID != 0) {
                            //
                            //--------------------------------------------------------------------------------------------
                            // ----- Custom Editor
                            //--------------------------------------------------------------------------------------------
                            //
                            // generate the style list on demand
                            // note: &editorFieldType should be deprecated
                            //
                            core.docProperties.setProperty("editorName", field.nameLc);
                            core.docProperties.setProperty("editorValue", fieldValue_text);
                            core.docProperties.setProperty("editorFieldId", fieldId);
                            core.docProperties.setProperty("editorFieldType", fieldTypeId);
                            core.docProperties.setProperty("editorReadOnly", editorReadOnly);
                            core.docProperties.setProperty("editorWidth", "");
                            core.docProperties.setProperty("editorHeight", "");
                            if (GenericController.encodeBoolean((fieldTypeId == FieldTypeIdHTML) || (fieldTypeId == FieldTypeIdFileHTML))) {
                                //
                                // include html related arguments
                                core.docProperties.setProperty("editorAllowActiveContent", "1");
                                core.docProperties.setProperty("editorAddonList", editorAddonListJSON);
                                core.docProperties.setProperty("editorStyles", styleList);
                                core.docProperties.setProperty("editorStyleOptions", styleOptionList);
                            }
                            EditorString = core.addon.execute(editorAddonID, new BaseClasses.CPUtilsBaseClass.addonExecuteContext {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextEditor,
                                errorContextMessage = "field editor id:" + editorAddonID
                            });
                            useEditorAddon = !string.IsNullOrEmpty(EditorString);
                            if (useEditorAddon) {
                                //
                                // -- editor worked
                                return_NewFieldList += "," + field.nameLc;
                            } else {
                                //
                                // -- editor failed, determine if it is missing (or inactive). If missing, remove it from the members preferences
                                string SQL = "select id from ccaggregatefunctions where id=" + editorAddonID;
                                CS = core.db.csOpenSql(SQL);
                                if (!core.db.csOk(CS)) {
                                    //
                                    // -- missing, not just inactive
                                    EditorString = "";
                                    //
                                    // load user's editor preferences to fieldEditorPreferences() - this is the editor this user has picked when there are >1
                                    //   fieldId:addonId,fieldId:addonId,etc
                                    //   with custom FancyBox form in edit window with button "set editor preference"
                                    //   this button causes a 'refresh' action, reloads fields with stream without save
                                    //
                                    string tmpList = core.userProperty.getText("editorPreferencesForContent:" + adminData.adminContent.id, "");
                                    int PosStart = GenericController.vbInstr(1, "," + tmpList, "," + fieldId + ":");
                                    if (PosStart > 0) {
                                        int PosEnd = GenericController.vbInstr(PosStart + 1, "," + tmpList, ",");
                                        if (PosEnd == 0) {
                                            tmpList = tmpList.Left(PosStart - 1);
                                        } else {
                                            tmpList = tmpList.Left(PosStart - 1) + tmpList.Substring(PosEnd - 1);
                                        }
                                        core.userProperty.setProperty("editorPreferencesForContent:" + adminData.adminContent.id, tmpList);
                                    }
                                }
                                core.db.csClose(ref CS);
                            }
                        }
                        if (!useEditorAddon) {
                            //
                            // if custom editor not used or if it failed
                            //
                            if (fieldTypeId == FieldTypeIdRedirect) {
                                //
                                // ----- Default Editor, Redirect fields (the same for normal/readonly/spelling)
                                RedirectPath = core.appConfig.adminRoute;
                                if (field.redirectPath != "") {
                                    RedirectPath = field.redirectPath;
                                }
                                RedirectPath = RedirectPath + "?" + RequestNameTitleExtension + "=" + GenericController.encodeRequestVariable(" For " + editRecord.nameLc + adminData.TitleExtension) + "&" + RequestNameAdminDepth + "=" + (adminData.ignore_legacyMenuDepth + 1) + "&wl0=" + field.redirectID + "&wr0=" + editRecord.id;
                                if (field.redirectContentID != 0) {
                                    RedirectPath = RedirectPath + "&cid=" + field.redirectContentID;
                                } else {
                                    RedirectPath = RedirectPath + "&cid=" + ((editRecord.contentControlId.Equals(0)) ? adminData.adminContent.id : editRecord.contentControlId);
                                }
                                if (editRecord.id == 0) {
                                    EditorString += ("[available after save]");
                                } else {
                                    RedirectPath = GenericController.vbReplace(RedirectPath, "'", "\\'");
                                    EditorString += ("<a href=\"#\"");
                                    EditorString += (" onclick=\" window.open('" + RedirectPath + "', '_blank', 'scrollbars=yes,toolbar=no,status=no,resizable=yes'); return false;\"");
                                    EditorString += (">");
                                    EditorString += ("Open in New Window</A>");
                                }
                                //s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal & EditorString & "&nbsp;</span></nobr></td>")
                            } else if (editorReadOnly) {
                                //
                                //--------------------------------------------------------------------------------------------
                                // ----- Display fields as read only
                                //--------------------------------------------------------------------------------------------
                                //
                                if (!string.IsNullOrEmpty(WhyReadOnlyMsg)) {
                                    WhyReadOnlyMsg = "<span class=\"ccDisabledReason\">" + WhyReadOnlyMsg + "</span>";
                                }
                                //EditorStyleModifier = "";
                                switch (fieldTypeId) {
                                    case FieldTypeIdText:
                                    case FieldTypeIdLink:
                                    case FieldTypeIdResourceLink:
                                        //
                                        // ----- Text Type
                                        EditorString += AdminUIController.getDefaultEditor_Text(core, field.nameLc, fieldValue_text, true, fieldHtmlId);
                                        return_NewFieldList += "," + field.nameLc;
                                        break;
                                    case FieldTypeIdBoolean:
                                        //
                                        // ----- Boolean ReadOnly
                                        EditorString += AdminUIController.getDefaultEditor_Bool(core, field.nameLc, GenericController.encodeBoolean(fieldValue_object), true, fieldHtmlId);
                                        return_NewFieldList += "," + field.nameLc;
                                        break;
                                    case FieldTypeIdLookup:
                                        //
                                        // ----- Lookup, readonly
                                        if (field.lookupContentID != 0) {
                                            EditorString = AdminUIController.getDefaultEditor_LookupContent(core, field.nameLc, encodeInteger(fieldValue_object), field.lookupContentID, ref IsEmptyList, field.readOnly, fieldHtmlId, WhyReadOnlyMsg, field.required);
                                            return_NewFieldList += "," + field.nameLc;
                                        } else if (field.lookupList != "") {
                                            EditorString = AdminUIController.getDefaultEditor_LookupList(core, field.nameLc, encodeInteger(fieldValue_object), field.lookupList.Split(','), field.readOnly, fieldHtmlId, WhyReadOnlyMsg, field.required);
                                            return_NewFieldList += "," + field.nameLc;
                                        } else {
                                            //
                                            // -- log exception but dont throw
                                            LogController.handleWarn(core, new ApplicationException("Field [" + adminData.adminContent.name + "." + field.nameLc + "] is a Lookup field, but no LookupContent or LookupList has been configured"));
                                            EditorString += "[Selection not configured]";
                                        }
                                        break;
                                    case FieldTypeIdDate:
                                        //
                                        // ----- date, readonly
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = AdminUIController.getDefaultEditor_Date(core, field.nameLc, GenericController.encodeDate(fieldValue_object), field.readOnly, fieldHtmlId, field.required, WhyReadOnlyMsg);
                                        break;
                                    case FieldTypeIdMemberSelect:
                                        //
                                        // ----- Member Select ReadOnly
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = AdminUIController.getDefaultEditor_memberSelect(core, field.nameLc, encodeInteger(fieldValue_object), field.memberSelectGroupId_get(core), field.memberSelectGroupName_get(core), field.readOnly, fieldHtmlId, field.required, WhyReadOnlyMsg);
                                        //
                                        break;
                                    case FieldTypeIdManyToMany:
                                        //
                                        //   Placeholder
                                        //
                                        EditorString = AdminUIController.getDefaultEditor_manyToMany(core, field, "field" + field.id, fieldValue_text, editRecord.id, editorReadOnly, WhyReadOnlyMsg);
                                        //MTMContent0 = CdefController.getContentNameByID(core, field.contentId);
                                        //MTMContent1 = CdefController.getContentNameByID(core, field.manyToManyContentID);
                                        //MTMRuleContent = CdefController.getContentNameByID(core, field.manyToManyRuleContentID);
                                        //MTMRuleField0 = field.ManyToManyRulePrimaryField;
                                        //MTMRuleField1 = field.ManyToManyRuleSecondaryField;
                                        //EditorString += core.html.getCheckList("ManyToMany" + field.id, MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1);
                                        //EditorString += WhyReadOnlyMsg;
                                        ////
                                        break;
                                    case FieldTypeIdCurrency:
                                        //
                                        // ----- Currency ReadOnly
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        FieldValueNumber = GenericController.encodeNumber(fieldValue_object);
                                        EditorString += (HtmlController.inputHidden(field.nameLc, GenericController.encodeText(FieldValueNumber)));
                                        EditorString += (HtmlController.inputText(core, field.nameLc, FieldValueNumber.ToString(), -1, -1, "", false, true, "text form-control"));
                                        EditorString += (string.Format("{0:C}", FieldValueNumber));
                                        EditorString += WhyReadOnlyMsg;
                                        //
                                        break;
                                    case FieldTypeIdAutoIdIncrement:
                                    case FieldTypeIdFloat:
                                    case FieldTypeIdInteger:
                                        //
                                        // ----- number readonly
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString += (HtmlController.inputHidden(field.nameLc, fieldValue_text));
                                        EditorString += (HtmlController.inputText(core, field.nameLc, fieldValue_text, -1, -1, "", false, true, "number form-control"));
                                        EditorString += WhyReadOnlyMsg;
                                        //
                                        break;
                                    case FieldTypeIdHTML:
                                    case FieldTypeIdFileHTML:
                                        //
                                        // ----- HTML types readonly
                                        //
                                        if (field.htmlContent) {
                                            //
                                            // edit html as html (see the code)
                                            //
                                            return_NewFieldList += "," + field.nameLc;
                                            EditorString += HtmlController.inputHidden(field.nameLc, fieldValue_text);
                                            //EditorStyleModifier = "textexpandable";
                                            FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                            EditorString += HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, field.nameLc, false, true, "form-control");
                                        } else {
                                            //
                                            // edit html as wysiwyg readonly
                                            //
                                            return_NewFieldList += "," + field.nameLc;
                                            EditorString += AdminUIController.getDefaultEditor_Html(core, field.nameLc, fieldValue_text, editorAddonListJSON, styleList, styleOptionList, true);
                                        }
                                        break;
                                    case FieldTypeIdLongText:
                                    case FieldTypeIdFileText:
                                        //
                                        // ----- LongText, TextFile
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString += HtmlController.inputHidden(field.nameLc, fieldValue_text);
                                        //EditorStyleModifier = "textexpandable";
                                        FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                        EditorString += HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, field.nameLc, false, true, " form-control");
                                        break;
                                    case FieldTypeIdFile:
                                    case FieldTypeIdFileImage:
                                        //
                                        // ----- File ReadOnly
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        NonEncodedLink = GenericController.getCdnFileLink(core, fieldValue_text);
                                        //NonEncodedLink = core.webServer.requestDomain + genericController.getCdnFileLink(core, FieldValueText);
                                        EncodedLink = GenericController.encodeURL(NonEncodedLink);
                                        EditorString += (HtmlController.inputHidden(field.nameLc, ""));
                                        if (string.IsNullOrEmpty(fieldValue_text)) {
                                            EditorString += ("[no file]");
                                        } else {
                                            string filename = "";
                                            string path = "";
                                            core.cdnFiles.splitDosPathFilename(fieldValue_text, ref path, ref filename);
                                            EditorString += ("&nbsp;<a href=\"http://" + EncodedLink + "\" target=\"_blank\">" + SpanClassAdminSmall + "[" + filename + "]</A>");
                                        }
                                        EditorString += WhyReadOnlyMsg;
                                        //
                                        break;
                                    default:
                                        //
                                        // ----- Legacy text type -- not used unless something was missed
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString += HtmlController.inputHidden(field.nameLc, fieldValue_text);
                                        if (field.password) {
                                            //
                                            // Password forces simple text box
                                            //
                                            EditorString += HtmlController.inputText(core, field.nameLc, "*****", 0, 0, "", true, true, "password form-control");
                                        } else if (!field.htmlContent) {
                                            //
                                            // not HTML capable, textarea with resizing
                                            //
                                            if ((fieldTypeId == FieldTypeIdText) && (fieldValue_text.IndexOf("\n") == -1) && (fieldValue_text.Length < 40)) {
                                                //
                                                // text field shorter then 40 characters without a CR
                                                //
                                                EditorString += HtmlController.inputText(core, field.nameLc, fieldValue_text, 1, 0, "", false, true, "text form-control");
                                            } else {
                                                //
                                                // longer text data, or text that contains a CR
                                                //
                                                //EditorStyleModifier = "textexpandable";
                                                EditorString += HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, 10, -1, "", false, true, " form-control");
                                            }
                                        } else if (field.htmlContent) {
                                            //
                                            // HTMLContent true, and prefered
                                            //
                                            //EditorStyleModifier = "text";
                                            FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".PixelHeight", 500));
                                            EditorString += core.html.getFormInputHTML(field.nameLc, fieldValue_text, "500", "", false, true, editorAddonListJSON, styleList, styleOptionList);
                                            //innovaEditor = New innovaEditorAddonClassFPO
                                            //EditorString &=  innovaEditor.getInnovaEditor( FormFieldLCaseName, EditorContext, FieldValueText, "", "", True, True, TemplateIDForStyles, emailIdForStyles)
                                            EditorString = "<div style=\"width:95%\">" + EditorString + "</div>";
                                        } else {
                                            //
                                            // HTMLContent true, but text editor selected
                                            //
                                            //EditorStyleModifier = "textexpandable";
                                            FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                            EditorString += HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, field.nameLc, false, true);
                                            //EditorString = core.main_GetFormInputTextExpandable(FormFieldLCaseName, encodeHTML(FieldValueText), FieldRows, "600px", FormFieldLCaseName, False)
                                        }
                                        break;
                                }
                            } else {
                                //
                                // -- Not Read Only - Display fields as form elements to be modified
                                switch (fieldTypeId) {
                                    case FieldTypeIdText:
                                        //
                                        // ----- Text Type
                                        if (field.password) {
                                            EditorString += AdminUIController.getDefaultEditor_Password(core, field.nameLc, fieldValue_text, false, fieldHtmlId);
                                        } else {
                                            EditorString += AdminUIController.getDefaultEditor_Text(core, field.nameLc, fieldValue_text, false, fieldHtmlId);
                                        }
                                        return_NewFieldList += "," + field.nameLc;
                                        break;
                                    case FieldTypeIdBoolean:
                                        //
                                        // ----- Boolean
                                        EditorString += AdminUIController.getDefaultEditor_Bool(core, field.nameLc, GenericController.encodeBoolean(fieldValue_object), false, fieldHtmlId);
                                        return_NewFieldList += "," + field.nameLc;
                                        break;
                                    case FieldTypeIdLookup:
                                        //
                                        // ----- Lookup
                                        if (field.lookupContentID != 0) {
                                            EditorString = AdminUIController.getDefaultEditor_LookupContent(core, field.nameLc, encodeInteger(fieldValue_object), field.lookupContentID, ref IsEmptyList, field.readOnly, fieldHtmlId, WhyReadOnlyMsg, field.required);
                                            return_NewFieldList += "," + field.nameLc;
                                        } else if (field.lookupList != "") {
                                            EditorString = AdminUIController.getDefaultEditor_LookupList(core, field.nameLc, encodeInteger(fieldValue_object), field.lookupList.Split(','), field.readOnly, fieldHtmlId, WhyReadOnlyMsg, field.required);
                                            return_NewFieldList += "," + field.nameLc;
                                        } else {
                                            //
                                            // -- log exception but dont throw
                                            LogController.handleWarn(core, new ApplicationException("Field [" + adminData.adminContent.name + "." + field.nameLc + "] is a Lookup field, but no LookupContent or LookupList has been configured"));
                                            EditorString += "[Selection not configured]";
                                        }
                                        break;
                                    case FieldTypeIdDate:
                                        //
                                        // ----- Date
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = AdminUIController.getDefaultEditor_Date(core, field.nameLc, GenericController.encodeDate(fieldValue_object), field.readOnly, fieldHtmlId, field.required, WhyReadOnlyMsg);
                                        break;
                                    case FieldTypeIdMemberSelect:
                                        //
                                        // ----- Member Select
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = AdminUIController.getDefaultEditor_memberSelect(core, field.nameLc, encodeInteger(fieldValue_object), field.memberSelectGroupId_get(core), field.memberSelectGroupName_get(core), field.readOnly, fieldHtmlId, field.required, WhyReadOnlyMsg);
                                        break;
                                    case FieldTypeIdManyToMany:
                                        //
                                        //   Placeholder
                                        EditorString = AdminUIController.getDefaultEditor_manyToMany(core, field, "field" + field.id, fieldValue_text, editRecord.id, false, WhyReadOnlyMsg);
                                        //MTMContent0 = CdefController.getContentNameByID(core, field.contentId);
                                        //MTMContent1 = CdefController.getContentNameByID(core, field.manyToManyContentID);
                                        //MTMRuleContent = CdefController.getContentNameByID(core, field.manyToManyRuleContentID);
                                        //MTMRuleField0 = field.ManyToManyRulePrimaryField;
                                        //MTMRuleField1 = field.ManyToManyRuleSecondaryField;
                                        //EditorString += core.html.getCheckList("ManyToMany" + field.id, MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, "", "", false, false, fieldValue_text);
                                        ////EditorString &= (core.html.getInputCheckListCategories("ManyToMany" & .id, MTMContent0, editRecord.id, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, , , False, MTMContent1, FieldValueText))
                                        break;
                                    case FieldTypeIdFile:
                                    case FieldTypeIdFileImage:
                                        //
                                        // ----- File
                                        return_NewFieldList += "," + field.nameLc;
                                        if (string.IsNullOrEmpty(fieldValue_text)) {
                                            EditorString += (core.html.inputFile(field.nameLc, "", "file form-control"));
                                        } else {
                                            NonEncodedLink = GenericController.getCdnFileLink(core, fieldValue_text);
                                            //NonEncodedLink = core.webServer.requestDomain + genericController.getCdnFileLink(core, FieldValueText);
                                            EncodedLink = HtmlController.encodeHtml(NonEncodedLink);
                                            string filename = "";
                                            string path = "";
                                            core.cdnFiles.splitDosPathFilename(fieldValue_text, ref path, ref filename);
                                            EditorString += ("&nbsp;<a href=\"" + EncodedLink + "\" target=\"_blank\">" + SpanClassAdminSmall + "[" + filename + "]</A>");
                                            EditorString += ("&nbsp;&nbsp;&nbsp;Delete:&nbsp;" + HtmlController.checkbox(field.nameLc + ".DeleteFlag", false));
                                            EditorString += ("&nbsp;&nbsp;&nbsp;Change:&nbsp;" + core.html.inputFile(field.nameLc, "", "file form-control"));
                                        }
                                        //
                                        break;
                                    case FieldTypeIdAutoIdIncrement:
                                    case FieldTypeIdCurrency:
                                    case FieldTypeIdFloat:
                                    case FieldTypeIdInteger:
                                        //
                                        // ----- Others that simply print
                                        return_NewFieldList += "," + field.nameLc;
                                        if (field.password) {
                                            EditorString += (HtmlController.inputText(core, field.nameLc, fieldValue_text, -1, -1, "", true, false, "password form-control"));
                                        } else {
                                            if (string.IsNullOrEmpty(fieldValue_text)) {
                                                EditorString += (HtmlController.inputText(core, field.nameLc, "", -1, -1, "", false, false, "text form-control"));
                                            } else {
                                                if (encodeBoolean(fieldValue_text.IndexOf("\n") + 1) || (fieldValue_text.Length > 40)) {
                                                    EditorString += (HtmlController.inputText(core, field.nameLc, fieldValue_text, -1, -1, "", false, false, "text form-control"));
                                                } else {
                                                    EditorString += (HtmlController.inputText(core, field.nameLc, fieldValue_text, 1, -1, "", false, false, "text form-control"));
                                                }
                                            }
                                        }
                                        break;
                                    case FieldTypeIdLink:
                                        //
                                        // ----- Link (href value
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = ""
                                            + HtmlController.inputText(core, field.nameLc, fieldValue_text, 1, 80, field.nameLc, false, false, "link form-control") + "&nbsp;<a href=\"#\" onClick=\"OpenResourceLinkWindow( '" + field.nameLc + "' ) ;return false;\"><img src=\"/ccLib/images/ResourceLink1616.gif\" width=16 height=16 border=0 alt=\"Link to a resource\" title=\"Link to a resource\"></a>"
                                            + "&nbsp;<a href=\"#\" onClick=\"OpenSiteExplorerWindow( '" + field.nameLc + "' ) ;return false;\"><img src=\"/ccLib/images/PageLink1616.gif\" width=16 height=16 border=0 alt=\"Link to a page\" title=\"Link to a page\"></a>";
                                        break;
                                    case FieldTypeIdResourceLink:
                                        //
                                        // ----- Resource Link (src value)
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = HtmlController.inputText(core, field.nameLc, fieldValue_text, 1, 80, field.nameLc, false, false, "resourceLink form-control") + "&nbsp;<a href=\"#\" onClick=\"OpenResourceLinkWindow( '" + field.nameLc + "' ) ;return false;\"><img src=\"/ccLib/images/ResourceLink1616.gif\" width=16 height=16 border=0 alt=\"Link to a resource\" title=\"Link to a resource\"></a>";
                                        break;
                                    case FieldTypeIdHTML:
                                    case FieldTypeIdFileHTML:
                                        //
                                        // content is html
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        if (field.htmlContent) {
                                            //
                                            // View the content as Html, not wysiwyg
                                            EditorString = AdminUIController.getDefaultEditor_TextArea(core, field.nameLc, fieldValue_text, editorReadOnly);
                                            //EditorString = htmlController.inputTextarea( core,field.nameLc, fieldValue_text, 10, -1, "", false, false, "text form-control");
                                        } else {
                                            //
                                            // wysiwyg editor
                                            EditorString = AdminUIController.getDefaultEditor_Html(core, field.nameLc, fieldValue_text, editorAddonListJSON, styleList, styleOptionList, editorReadOnly);

                                            //if (string.IsNullOrEmpty(fieldValue_text)) {
                                            //    //
                                            //    // editor needs a starting p tag to setup correctly
                                            //    fieldValue_text = HTMLEditorDefaultCopyNoCr;
                                            //}
                                            //FieldRows = (core.userProperty.getInteger(adminContext.content.name + "." + field.nameLc + ".PixelHeight", 500));
                                            //EditorString += core.html.getFormInputHTML(field.nameLc, fieldValue_text, "500", "", false, true, editorAddonListJSON, styleList, styleOptionList);
                                            //EditorString = "<div style=\"width:95%\">" + EditorString + "</div>";
                                        }
                                        //
                                        break;
                                    case FieldTypeIdLongText:
                                    case FieldTypeIdFileText:
                                        //
                                        // -- Long Text, use text editor
                                        return_NewFieldList += "," + field.nameLc;
                                        FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                        EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, field.nameLc, false, false, "text form-control");
                                        //
                                        break;
                                    case FieldTypeIdFileCSS:
                                        //
                                        // ----- CSS field
                                        return_NewFieldList += "," + field.nameLc;
                                        FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                        EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, 10, -1, "", false, false, "styles form-control");
                                        break;
                                    case FieldTypeIdFileJavascript:
                                        //
                                        // ----- Javascript field
                                        return_NewFieldList += "," + field.nameLc;
                                        FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                        EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, field.nameLc, false, false, "text form-control");
                                        //
                                        break;
                                    case FieldTypeIdFileXML:
                                        //
                                        // ----- xml field
                                        return_NewFieldList += "," + field.nameLc;
                                        FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                        EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, field.nameLc, false, false, "text form-control");
                                        //
                                        break;
                                    default:
                                        //
                                        // ----- Legacy text type -- not used unless something was missed
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        if (field.password) {
                                            //
                                            // Password forces simple text box
                                            EditorString = HtmlController.inputText(core, field.nameLc, fieldValue_text, -1, -1, "", true, false, "password form-control");
                                        } else if (!field.htmlContent) {
                                            //
                                            // not HTML capable, textarea with resizing
                                            //
                                            if ((fieldTypeId == FieldTypeIdText) && (fieldValue_text.IndexOf("\n") == -1) && (fieldValue_text.Length < 40)) {
                                                //
                                                // text field shorter then 40 characters without a CR
                                                //
                                                EditorString = HtmlController.inputText(core, field.nameLc, fieldValue_text, 1, -1, "", false, false, "text form-control");
                                            } else {
                                                //
                                                // longer text data, or text that contains a CR
                                                //
                                                //EditorStyleModifier = "textexpandable";
                                                EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, 10, -1, "", false, false, "text form-control");
                                            }
                                        } else if (field.htmlContent) {
                                            //
                                            // HTMLContent true, and prefered
                                            //
                                            if (string.IsNullOrEmpty(fieldValue_text)) {
                                                //
                                                // editor needs a starting p tag to setup correctly
                                                //
                                                fieldValue_text = HTMLEditorDefaultCopyNoCr;
                                            }
                                            //EditorStyleModifier = "htmleditor";
                                            FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".PixelHeight", 500));
                                            EditorString += core.html.getFormInputHTML(field.nameLc, fieldValue_text, "500", "", false, true, editorAddonListJSON, styleList, styleOptionList);
                                            //innovaEditor = New innovaEditorAddonClassFPO
                                            //EditorString = innovaEditor.getInnovaEditor( FormFieldLCaseName, EditorContext, FieldValueText, "", "", True, False, TemplateIDForStyles, emailIdForStyles)
                                            EditorString = "<div style=\"width:95%\">" + EditorString + "</div>";
                                        } else {
                                            //
                                            // HTMLContent true, but text editor selected
                                            //
                                            //EditorStyleModifier = "textexpandable";
                                            FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                            EditorString = HtmlController.inputTextarea(core, field.nameLc, HtmlController.encodeHtml(fieldValue_text), FieldRows, -1, field.nameLc, false, false, "text");
                                        }
                                        //s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal & EditorString & "</span></nobr></td>")
                                        break;
                                }
                            }
                        }
                        //
                        // Build Help Line Below editor
                        //
                        adminData.includeFancyBox = true;
                        HelpMsgDefault = "";
                        HelpMsgCustom = "";
                        EditorHelp = "";
                        LcaseName = GenericController.vbLCase(field.nameLc);
                        if (AllowHelpMsgCustom) {
                            HelpMsgDefault = field.helpDefault;
                            HelpMsgCustom = field.helpCustom;
                        }
                        if (!string.IsNullOrEmpty(HelpMsgCustom)) {
                            HelpMsg = HelpMsgCustom;
                        } else {
                            HelpMsg = HelpMsgDefault;
                        }
                        HelpMsgOpenedRead = HelpMsg;
                        HelpMsgClosed = HelpMsg;
                        IsEmptyHelp = HelpMsgClosed.Length == 0;
                        IsLongHelp = (HelpMsgClosed.Length > 100);
                        if (IsLongHelp) {
                            HelpMsgClosed = HelpMsgClosed.Left(100) + "...";
                        }
                        //
                        HelpID = "helpId" + fieldId;
                        HelpEditorID = "helpEditorId" + fieldId;
                        HelpOpenedReadID = "HelpOpenedReadID" + fieldId;
                        HelpOpenedEditID = "HelpOpenedEditID" + fieldId;
                        HelpClosedID = "helpClosedId" + fieldId;
                        HelpClosedContentID = "helpClosedContentId" + fieldId;
                        //AllowHelpRow = true;
                        //
                        //------------------------------------------------------------------------------------------------------------
                        // editor preferences form - a fancybox popup that interfaces with a hardcoded ajax function in init() to set a member property
                        //------------------------------------------------------------------------------------------------------------
                        //
                        AjaxQS = RequestNameAjaxFunction + "=" + ajaxGetFieldEditorPreferenceForm + "&fieldid=" + fieldId + "&currentEditorAddonId=" + editorAddonID + "&fieldTypeDefaultEditorAddonId=" + fieldTypeDefaultEditorAddonId;
                        fancyBoxLinkId = "fbl" + adminData.fancyBoxPtr;
                        fancyBoxContentId = "fbc" + adminData.fancyBoxPtr;
                        adminData.fancyBoxHeadJS = adminData.fancyBoxHeadJS + "\r\njQuery('#" + fancyBoxLinkId + "').fancybox({"
                            + "'titleShow':false,"
                            + "'transitionIn':'elastic',"
                            + "'transitionOut':'elastic',"
                            + "'overlayOpacity':'.2',"
                            + "'overlayColor':'#000000',"
                            + "'onStart':function(){cj.ajax.qs('" + AjaxQS + "','','" + fancyBoxContentId + "')}"
                            + "});";
                        EditorHelp = EditorHelp + "\r<div style=\"float:right;\">"
                            + cr2 + "<a id=\"" + fancyBoxLinkId + "\" href=\"#" + fancyBoxContentId + "\" title=\"select an alternate editor for this field.\" tabindex=\"-1\"><img src=\"/ccLib/images/NavAltEditor.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"select an alternate editor for this field.\"></a>"
                            + cr2 + "<div style=\"display:none;\">"
                            + cr3 + "<div class=\"ccEditorPreferenceCon\" id=\"" + fancyBoxContentId + "\"><div style=\"margin:20px auto auto auto;\"><img src=\"/ccLib/images/ajax-loader-big.gif\" width=\"32\" height=\"32\"></div></div>"
                            + cr2 + "</div>"
                            + "\r</div>"
                            + "";
                        adminData.fancyBoxPtr = adminData.fancyBoxPtr + 1;
                        //
                        //------------------------------------------------------------------------------------------------------------
                        // field help
                        //------------------------------------------------------------------------------------------------------------
                        //
                        if (core.session.isAuthenticatedAdmin(core)) {
                            //
                            // Admin view
                            //
                            if (string.IsNullOrEmpty(HelpMsgDefault)) {
                                HelpMsgDefault = "Admin: No default help is available for this field.";
                            }
                            HelpMsgOpenedRead = ""
                                    + "<!-- close icon --><div class=\"\" style=\"float:right\"><a href=\"javascript:cj.hide('" + HelpOpenedReadID + "');cj.show('" + HelpClosedID + "');\"><img src=\"/ccLib/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"close\"></a></div>"
                                    + "<div class=\"header\">Default Help</div>"
                                    + "<div class=\"body\">" + HelpMsgDefault + "</div>"
                                    + "<div class=\"header\">Custom Help</div>"
                                    + "<div class=\"body\">" + HelpMsgCustom + "</div>"
                                + "";
                            string jsUpdate = "updateFieldHelp('" + fieldId + "','" + HelpEditorID + "','" + HelpClosedContentID + "');cj.hide('" + HelpOpenedEditID + "');cj.show('" + HelpClosedID + "');return false";
                            string jsCancel = "cj.hide('" + HelpOpenedEditID + "');cj.show('" + HelpClosedID + "');return false";
                            HelpMsgOpenedEdit = ""
                                    + "<div class=\"header\">Default Help</div>"
                                    + "<div class=\"body\">" + HelpMsgDefault + "</div>"
                                    + "<div class=\"header\">Custom Help</div>"
                                    + "<div class=\"body\"><textarea id=\"" + HelpEditorID + "\" ROWS=\"10\" style=\"width:100%;\">" + HelpMsgCustom + "</TEXTAREA></div>"
                                    + "<div class=\"\">"
                                        + AdminUIController.getButtonPrimary("Update", jsUpdate)
                                        + AdminUIController.getButtonPrimary("Cancel", jsCancel)
                                    + "</div>"
                                + "";
                            //HelpMsgOpenedEdit = ""
                            //    + "<div class=\"header\">Default Help</div>"
                            //    + "<div class=\"body\">" + HelpMsgDefault + "</div>"
                            //    + "<div class=\"header\">Custom Help</div>"
                            //    + "<div class=\"body\"><textarea id=\"" + HelpEditorID + "\" ROWS=\"10\" style=\"width:100%;\">" + HelpMsgCustom + "</TEXTAREA></div>"
                            //    + "<div class=\"\">"
                            //        + "<input type=\"submit\" name=\"button\" value=\"Update\" onClick=\"updateFieldHelp('" + fieldId + "','" + HelpEditorID + "','" + HelpClosedContentID + "');cj.hide('" + HelpOpenedEditID + "');cj.show('" + HelpClosedID + "');return false\">"
                            //        + "<input type=\"submit\" name=\"button\" value=\"Cancel\" onClick=\"cj.hide('" + HelpOpenedEditID + "');cj.show('" + HelpClosedID + "');return false\">"
                            //    + "</div>"
                            //+ "";
                            if (IsLongHelp) {
                                //
                                // Long help, closed gets MoreHelpIcon (opens to HelpMsgOpenedRead) and HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                //
                                HelpMsgClosed = ""
                                        + "<!-- open read icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide('" + HelpClosedID + "');cj.show('" + HelpOpenedReadID + "');\" tabindex=\"-1\"><img src=\"/ccLib/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"more help\"></a></div>"
                                        + "<!-- open edit icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide('" + HelpClosedID + "');cj.show('" + HelpOpenedEditID + "');\" tabindex=\"-1\"><img src=\"/ccLib/images/NavHelpEdit.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"edit help\"></a></div>"
                                        + "<div id=\"" + HelpClosedContentID + "\">" + HelpMsgClosed + "</div>"
                                    + "";
                            } else if (!IsEmptyHelp) {
                                //
                                // short help, closed gets helpmsgclosed + HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                //
                                HelpMsgClosed = ""
                                        + "<!-- open edit icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide('" + HelpClosedID + "');cj.show('" + HelpOpenedEditID + "');\" tabindex=\"-1\"><img src=\"/ccLib/images/NavHelpEdit.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"edit help\"></a></div>"
                                        + "<div id=\"" + HelpClosedContentID + "\">" + HelpMsgClosed + "</div>"
                                    + "";
                            } else {
                                //
                                // Empty help, closed gets helpmsgclosed + HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                //
                                HelpMsgClosed = ""
                                        + "<!-- open edit icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide('" + HelpClosedID + "');cj.show('" + HelpOpenedEditID + "');\" tabindex=\"-1\"><img src=\"/ccLib/images/NavHelpEdit.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"edit help\"></a></div>"
                                        + "<div id=\"" + HelpClosedContentID + "\">" + HelpMsgClosed + "</div>"
                                    + "";
                            }
                            EditorHelp = EditorHelp + "<div id=\"" + HelpOpenedReadID + "\" class=\"opened\">" + HelpMsgOpenedRead + "</div>"
                                + "<div id=\"" + HelpOpenedEditID + "\" class=\"opened\">" + HelpMsgOpenedEdit + "</div>"
                                + "<div id=\"" + HelpClosedID + "\" class=\"closed\">" + HelpMsgClosed + "</div>"
                                + "";
                        } else {
                            //
                            // Non-admin view
                            //
                            HelpMsgOpenedRead = ""
                                    + "<div class=\"body\">"
                                    + "<!-- close icon --><a href=\"javascript:cj.hide('" + HelpOpenedReadID + "');cj.show('" + HelpClosedID + "');\"><img src=\"/ccLib/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;float:right\" title=\"close\"></a>"
                                    + HelpMsg + "</div>"
                                + "";
                            //HelpMsgOpenedRead = "" _
                            //        & "<!-- close icon --><div class="""" style=""float:right""><a href=""javascript:cj.hide('" & HelpOpenedReadID & "');cj.show('" & HelpClosedID & "');""><img src=""/ccLib/images/NavHelp.gif"" width=18 height=18 border=0 style=""vertical-align:middle;"" title=""close""></a></div>" _
                            //        & "<div class=""body"">" & HelpMsg & "</div>" _
                            //    & ""
                            HelpMsgOpenedEdit = ""
                                + "";
                            if (IsLongHelp) {
                                //
                                // Long help
                                //
                                HelpMsgClosed = ""
                                    + "<div class=\"body\">"
                                    + "<!-- open read icon --><a href=\"javascript:cj.hide('" + HelpClosedID + "');cj.show('" + HelpOpenedReadID + "');\"><img src=\"/ccLib/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;float:right;\" title=\"more help\"></a>"
                                    + HelpMsgClosed + "</div>"
                                    + "";
                            } else if (!IsEmptyHelp) {
                                //
                                // short help
                                //
                                HelpMsgClosed = ""
                                    + "<div class=\"body\">"
                                        + HelpMsg + "</div>"
                                    + "";
                            } else {
                                //
                                // no help
                                //
                                //AllowHelpRow = false;
                                HelpMsgClosed = ""
                                    + "";
                            }
                            EditorHelp = EditorHelp + "<div id=\"" + HelpOpenedReadID + "\" class=\"opened\">" + HelpMsgOpenedRead + "</div>"
                                + "<div id=\"" + HelpClosedID + "\" class=\"closed\">" + HelpMsgClosed + "</div>"
                                + "";
                        }
                        //
                        // assemble the editor row
                        //
                        //string fieldHelp = htmlController.div(htmlController.small(field.helpDefault), "form-text text-muted","emailHelp");
                        string editorRow = AdminUIController.getEditRow(core, EditorString, fieldCaption, field.helpDefault, field.required, false, fieldHtmlId);
                        resultBody.Add("<tr><td colspan=2>" + editorRow + "</td></tr>");
                    }
                    //
                    // ----- add the *Required Fields footer
                    //
                    resultBody.Add("<tr><td colspan=2 style=\"padding-top:10px;font-size:70%\"><div>* Field is required.</div><div>** Field must be unique.</div>");
                    if (needUniqueEmailMessage) {
                        resultBody.Add("<div>*** Field must be unique because this site allows login by email.</div>");
                    }
                    resultBody.Add("</td></tr>");
                    //
                    // ----- close the panel
                    //
                    if (string.IsNullOrEmpty(EditTab)) {
                        fieldCaption = "Content Fields";
                    } else {
                        fieldCaption = "Content Fields - " + EditTab;
                    }
                    adminData.EditSectionPanelCount = adminData.EditSectionPanelCount + 1;
                    returnHtml = AdminUIController.getEditPanel(core, (!adminData.allowAdminTabs), fieldCaption, "", AdminUIController.editTableOpen + resultBody.Text + AdminUIController.editTableClose);
                    adminData.EditSectionPanelCount = adminData.EditSectionPanelCount + 1;
                    resultBody = null;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnHtml;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Create all the tabs for the edit form
        /// </summary>
        /// <param name="core"></param>
        /// <param name="adminData"></param>
        /// <param name="readOnlyField"></param>
        /// <param name="IsLandingPage"></param>
        /// <param name="IsRootPage"></param>
        /// <param name="EditorContext"></param>
        /// <param name="allowAjaxTabs"></param>
        /// <param name="TemplateIDForStyles"></param>
        /// <param name="fieldTypeDefaultEditors"></param>
        /// <param name="fieldEditorPreferenceList"></param>
        /// <param name="styleList"></param>
        /// <param name="styleOptionList"></param>
        /// <param name="emailIdForStyles"></param>
        /// <param name="IsTemplateTable"></param>
        /// <param name="editorAddonListJSON"></param>
        /// <returns></returns>
        public static string GetForm_Edit_Tabs(CoreController core, AdminDataModel adminData, AdminMenuController adminMenu, bool readOnlyField, bool IsLandingPage, bool IsRootPage, ContentTypeEnum EditorContext, bool allowAjaxTabs, int TemplateIDForStyles, string[] fieldTypeDefaultEditors, string fieldEditorPreferenceList, string styleList, string styleOptionList, int emailIdForStyles, bool IsTemplateTable, string editorAddonListJSON) {
            string returnHtml = "";
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
                //
                //
                string tabContent = null;
                string AjaxLink = null;
                List<string> TabsFound = new List<string>();
                string editTabCaption = null;
                string NewFormFieldList = null;
                string FormFieldList = null;
                bool AllowHelpMsgCustom = false;
                string IDList = null;
                DataTable dt = null;
                string[,] TempVar = null;
                int HelpCnt = 0;
                int fieldId = 0;
                int LastFieldID = 0;
                int HelpPtr = 0;
                int[] HelpIDCache = { };
                string[] helpDefaultCache = { };
                string[] HelpCustomCache = { };
                KeyPtrController helpIdIndex = new KeyPtrController();
                string fieldNameLc = null;
                //
                // ----- read in help
                //
                IDList = "";
                foreach (KeyValuePair<string, CDefFieldModel> keyValuePair in adminData.adminContent.fields) {
                    CDefFieldModel field = keyValuePair.Value;
                    IDList = IDList + "," + field.id;
                }
                if (!string.IsNullOrEmpty(IDList)) {
                    IDList = IDList.Substring(1);
                }
                //
                dt = core.db.executeQuery("select fieldid,helpdefault,helpcustom from ccfieldhelp where fieldid in (" + IDList + ") order by fieldid,id");
                TempVar = core.db.convertDataTabletoArray(dt);
                if (TempVar.GetLength(0) > 0) {
                    HelpCnt = TempVar.GetUpperBound(1) + 1;
                    HelpIDCache = new int[HelpCnt + 1];
                    helpDefaultCache = new string[HelpCnt + 1];
                    HelpCustomCache = new string[HelpCnt + 1];
                    fieldId = -1;
                    for (HelpPtr = 0; HelpPtr < HelpCnt; HelpPtr++) {
                        fieldId = GenericController.encodeInteger(TempVar[0, HelpPtr]);
                        if (fieldId != LastFieldID) {
                            LastFieldID = fieldId;
                            HelpIDCache[HelpPtr] = fieldId;
                            helpIdIndex.setPtr(fieldId.ToString(), HelpPtr);
                            helpDefaultCache[HelpPtr] = GenericController.encodeText(TempVar[1, HelpPtr]);
                            HelpCustomCache[HelpPtr] = GenericController.encodeText(TempVar[2, HelpPtr]);
                        }
                    }
                    AllowHelpMsgCustom = true;
                }
                //
                FormFieldList = ",";
                foreach (KeyValuePair<string, CDefFieldModel> keyValuePair in adminData.adminContent.fields) {
                    CDefFieldModel field = keyValuePair.Value;
                    if ((field.authorable) & (field.active) && (!TabsFound.Contains(field.editTabName.ToLower()))) {
                        TabsFound.Add(field.editTabName.ToLower());
                        fieldNameLc = field.nameLc;
                        editTabCaption = field.editTabName;
                        if (string.IsNullOrEmpty(editTabCaption)) {
                            editTabCaption = "Details";
                        }
                        NewFormFieldList = "";
                        if ((!adminData.allowAdminTabs) | (!allowAjaxTabs) || (editTabCaption.ToLower() == "details")) {
                            //
                            // Live Tab (non-tab mode, non-ajax mode, or details tab
                            //
                            tabContent = GetForm_Edit_Tab(core,adminData, editRecord.id, adminData.adminContent.id, readOnlyField, IsLandingPage, IsRootPage, field.editTabName, EditorContext, ref NewFormFieldList, TemplateIDForStyles, HelpCnt, HelpIDCache, helpDefaultCache, HelpCustomCache, AllowHelpMsgCustom, helpIdIndex, fieldTypeDefaultEditors, fieldEditorPreferenceList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON);
                            if (!string.IsNullOrEmpty(tabContent)) {
                                returnHtml += GetForm_Edit_AddTab2(core, adminMenu, editTabCaption, tabContent, adminData.allowAdminTabs, "");
                            }
                        } else {
                            //
                            // Ajax Tab
                            //
                            //AjaxLink = "/admin/index.asp?"
                            AjaxLink = "/" + core.appConfig.adminRoute + "?"
                            + RequestNameAjaxFunction + "=" + AjaxGetFormEditTabContent + "&ID=" + editRecord.id + "&CID=" + adminData.adminContent.id + "&ReadOnly=" + readOnlyField + "&IsLandingPage=" + IsLandingPage + "&IsRootPage=" + IsRootPage + "&EditTab=" + GenericController.encodeRequestVariable(field.editTabName) + "&EditorContext=" + EditorContext + "&NewFormFieldList=" + GenericController.encodeRequestVariable(NewFormFieldList);
                            returnHtml += GetForm_Edit_AddTab2(core, adminMenu, editTabCaption, "", true, AjaxLink);
                        }
                        if (!string.IsNullOrEmpty(NewFormFieldList)) {
                            FormFieldList = NewFormFieldList + FormFieldList;
                        }
                    }
                }
                //
                // ----- add the FormFieldList hidden - used on read to make sure all fields are returned
                //       this may not be needed, but we are having a problem with forms coming back without values
                //
                //
                // moved this to GetEditTabContent - so one is added for each tab.
                //
                returnHtml += HtmlController.inputHidden("FormFieldList", FormFieldList);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnHtml;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// add a tab to the list of tabs
        /// </summary>
        /// <param name="core"></param>
        /// <param name="Caption"></param>
        /// <param name="Content"></param>
        /// <param name="AllowAdminTabs"></param>
        /// <returns></returns>
        public static string GetForm_Edit_AddTab(CoreController core, AdminMenuController adminMenu, string Caption, string Content, bool AllowAdminTabs) {
            string tempGetForm_Edit_AddTab = null;
            try {
                //
                if (!string.IsNullOrEmpty(Content)) {
                    if (!AllowAdminTabs) {
                        tempGetForm_Edit_AddTab = Content;
                    } else {
                        adminMenu.menuComboTab.AddEntry(Caption.Replace(" ", "&nbsp;"), "", "", Content, false, "ccAdminTab");
                        //Call core.htmldoc.main_AddLiveTabEntry(Replace(Caption, " ", "&nbsp;"), Content, "ccAdminTab")
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetForm_Edit_AddTab;
        }
        //
        // ====================================================================================================
        //   Creates Tabbed content that is either Live (all content on page) or Ajax (click and ajax in the content)
        //
        public static string GetForm_Edit_AddTab2(CoreController core, AdminMenuController adminMenu, string Caption, string Content, bool AllowAdminTabs, string AjaxLink) {
            string tempGetForm_Edit_AddTab2 = null;
            try {
                //
                if (!AllowAdminTabs) {
                    //
                    // non-tab mode
                    //
                    tempGetForm_Edit_AddTab2 = Content;
                } else if (!string.IsNullOrEmpty(AjaxLink)) {
                    //
                    // Ajax Tab
                    //
                    adminMenu.menuComboTab.AddEntry(Caption.Replace(" ", "&nbsp;"), "", AjaxLink, "", false, "ccAdminTab");
                } else {
                    //
                    // Live Tab
                    //
                    adminMenu.menuComboTab.AddEntry(Caption.Replace(" ", "&nbsp;"), "", "", Content, false, "ccAdminTab");
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetForm_Edit_AddTab2;
        }
        //
        // ====================================================================================================
        //
        public static string GetForm_EditTitle(CoreController core, AdminDataModel adminData) {
            return "";
            // this info moved to description block
            //string tempGetForm_EditTitle = "";
            //try {
            //    //
            //    if (editRecord.id == 0) {
            //        tempGetForm_EditTitle = "Add an entry to " + editRecord.contentControlId_Name + TitleExtension;
            //    } else {
            //        tempGetForm_EditTitle = "Editing Record " + editRecord.id + " in " + editRecord.contentControlId_Name + " " + TitleExtension;
            //    }
            //} catch (Exception ex) {
            //    logController.handleError(core, ex);
            //}
            //return tempGetForm_EditTitle;
        }
        //
        // ====================================================================================================
        //
        public static string GetForm_EditTitleBar(CoreController core, AdminDataModel adminData) {
            // todo
            AdminUIController.EditRecordClass editRecord = adminData.editRecord;
            //
            //adminUIController Adminui = new adminUIController(core);
            return AdminUIController.getTitleBar(core, GetForm_EditTitle(core,adminData), "");
        }
        //
        //========================================================================
        // Print the Group Rules section for Content Edit form
        //   Group rules show which groups have authoring rights to a content
        //
        //   adminContext.content.id is the ContentID of the Content Definition being edited
        //   EditRecord.ContentID is the ContentControlID of the Edit Record
        //========================================================================
        //
        public static string GetForm_Edit_GroupRules(CoreController core, AdminDataModel adminData) {
            string tempGetForm_Edit_GroupRules = null;
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
                //
                //
                string SQL = null;
                int CS = 0;
                int GroupRulesCount = 0;
                int GroupRulesSize = 0;
                int GroupRulesPointer = 0;
                string SectionName = null;
                string GroupName = null;
                int GroupCount = 0;
                bool GroupFound = false;
                AdminDataModel.GroupRuleType[] GroupRules = { };
                StringBuilderLegacyController FastString = null;
                //adminUIController Adminui = new adminUIController(core);
                //
                // ----- Open the panel
                //
                FastString = new StringBuilderLegacyController();
                //
                //Call core.main_PrintPanelTop("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                //Call call FastString.Add(adminUIController.EditTableOpen)
                //
                // ----- Gather all the groups which have authoring rights to the content
                //
                GroupRulesCount = 0;
                GroupRulesSize = 0;
                if (editRecord.id != 0) {
                    SQL = "SELECT ccGroups.ID AS ID, ccGroupRules.AllowAdd as allowadd, ccGroupRules.AllowDelete as allowdelete"
                        + " FROM ccGroups LEFT JOIN ccGroupRules ON ccGroups.ID = ccGroupRules.GroupID"
                        + " WHERE (((ccGroupRules.ContentID)=" + editRecord.id + ") AND ((ccGroupRules.Active)<>0) AND ((ccGroups.Active)<>0))";
                    CS = core.db.csOpenSql(SQL, "Default");
                    if (core.db.csOk(CS)) {
                        if (true) {
                            GroupRulesSize = 100;
                            GroupRules = new AdminDataModel.GroupRuleType[GroupRulesSize + 1];
                            while (core.db.csOk(CS)) {
                                if (GroupRulesCount >= GroupRulesSize) {
                                    GroupRulesSize = GroupRulesSize + 100;
                                    Array.Resize(ref GroupRules, GroupRulesSize + 1);
                                }
                                GroupRules[GroupRulesCount].GroupID = core.db.csGetInteger(CS, "ID");
                                GroupRules[GroupRulesCount].AllowAdd = core.db.csGetBoolean(CS, "AllowAdd");
                                GroupRules[GroupRulesCount].AllowDelete = core.db.csGetBoolean(CS, "AllowDelete");
                                GroupRulesCount = GroupRulesCount + 1;
                                core.db.csGoNext(CS);
                            }
                        }
                    }
                }
                core.db.csClose(ref CS);
                //
                // ----- Gather all the groups, sorted by ContentName
                //
                SQL = "SELECT ccGroups.ID AS ID, ccContent.Name AS SectionName, ccGroups.Name AS GroupName, ccGroups.Caption AS GroupCaption, ccGroups.SortOrder"
                    + " FROM ccGroups LEFT JOIN ccContent ON ccGroups.ContentControlID = ccContent.ID"
                    + " Where (((ccGroups.Active) <> " + SQLFalse + ") And ((ccContent.Active) <> " + SQLFalse + "))"
                    + " GROUP BY ccGroups.ID, ccContent.Name, ccGroups.Name, ccGroups.Caption, ccGroups.SortOrder"
                    + " ORDER BY ccContent.Name, ccGroups.Caption, ccGroups.SortOrder";
                CS = core.db.csOpenSql(SQL, "Default");
                if (!core.db.csOk(CS)) {
                    FastString.Add("\r\n<tr><td colspan=\"3\">" + SpanClassAdminSmall + "There are no active groups</span></td></tr>");
                } else {
                    if (true) {
                        //Call FastString.Add(vbCrLf & "<tr><td colspan=""3"" class=""ccAdminEditSubHeader"">Groups with authoring access</td></tr>")
                        SectionName = "";
                        GroupCount = 0;
                        while (core.db.csOk(CS)) {
                            GroupName = core.db.csGet(CS, "GroupCaption");
                            if (string.IsNullOrEmpty(GroupName)) {
                                GroupName = core.db.csGet(CS, "GroupName");
                            }
                            FastString.Add("<tr>");
                            if (SectionName != core.db.csGet(CS, "SectionName")) {
                                //
                                // ----- create the next section
                                //
                                SectionName = core.db.csGet(CS, "SectionName");
                                FastString.Add("<td valign=\"top\" align=\"right\">" + SpanClassAdminSmall + SectionName + "</td>");
                            } else {
                                FastString.Add("<td valign=\"top\" align=\"right\">&nbsp;</td>");
                            }
                            FastString.Add("<td class=\"ccAdminEditField\" align=\"left\" colspan=\"2\">" + SpanClassAdminSmall);
                            GroupFound = false;
                            if (GroupRulesCount != 0) {
                                for (GroupRulesPointer = 0; GroupRulesPointer < GroupRulesCount; GroupRulesPointer++) {
                                    if (GroupRules[GroupRulesPointer].GroupID == core.db.csGetInteger(CS, "ID")) {
                                        GroupFound = true;
                                        break;
                                    }
                                }
                            }
                            FastString.Add("<input type=\"hidden\" name=\"GroupID" + GroupCount + "\" value=\"" + core.db.csGet(CS, "ID") + "\">");
                            FastString.Add("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"400\"><tr>");
                            if (GroupFound) {
                                FastString.Add("<td width=\"200\">" + SpanClassAdminSmall + HtmlController.checkbox("Group" + GroupCount, true) + GroupName + "</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + HtmlController.checkbox("GroupRuleAllowAdd" + GroupCount, GroupRules[GroupRulesPointer].AllowAdd) + " Allow Add</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + HtmlController.checkbox("GroupRuleAllowDelete" + GroupCount, GroupRules[GroupRulesPointer].AllowDelete) + " Allow Delete</span></td>");
                            } else {
                                FastString.Add("<td width=\"200\">" + SpanClassAdminSmall + HtmlController.checkbox("Group" + GroupCount, false) + GroupName + "</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + HtmlController.checkbox("GroupRuleAllowAdd" + GroupCount, false) + " Allow Add</span></td>");
                                FastString.Add("<td width=\"100\">" + SpanClassAdminSmall + HtmlController.checkbox("GroupRuleAllowDelete" + GroupCount, false) + " Allow Delete</span></td>");
                            }
                            FastString.Add("</tr></table>");
                            FastString.Add("</span></td>");
                            FastString.Add("</tr>");
                            GroupCount = GroupCount + 1;
                            core.db.csGoNext(CS);
                        }
                        FastString.Add("\r\n<input type=\"hidden\" name=\"GroupCount\" value=\"" + GroupCount + "\">");
                    }
                }
                core.db.csClose(ref CS);
                //
                // ----- close the panel
                //
                //Call FastString.Add(adminUIController.EditTableClose)
                //Call core.main_PrintPanelBottom("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                //
                tempGetForm_Edit_GroupRules = AdminUIController.getEditPanel(core, (!adminData.allowAdminTabs), "Authoring Permissions", "The following groups can edit this content.", AdminUIController.editTableOpen + FastString.Text + AdminUIController.editTableClose);
                adminData.EditSectionPanelCount = adminData.EditSectionPanelCount + 1;
                FastString = null;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetForm_Edit_GroupRules;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        public static string GetForm_EditFormStart(CoreController core, AdminDataModel adminData, int AdminFormID) {
            string s = "";
            try {
                core.html.addScriptCode("var docLoaded=false", "Form loader");
                core.html.addScriptCode_onLoad("docLoaded=true;", "Form loader");
                s = HtmlController.formMultipart_start(core, core.doc.refreshQueryString, "", "ccForm", "adminEditForm");
                s = GenericController.vbReplace(s, ">", " onSubmit=\"cj.admin.saveEmptyFieldList('FormEmptyFieldList');\" autocomplete=\"off\">");
                s += "\r\n<!-- block --><div class=\"d-none\"><input type=password name=\"password_block\" value=\"\"><input type=text name=\"username_block\" value=\"\"></div><!-- end block -->";
                s += "\r\n<input TYPE=\"hidden\" NAME=\"" + rnAdminSourceForm + "\" VALUE=\"" + AdminFormID.ToString() + "\">";
                s += "\r\n<input TYPE=\"hidden\" NAME=\"" + RequestNameTitleExtension + "\" VALUE=\"" + adminData.TitleExtension + "\">";
                s += "\r\n<input TYPE=\"hidden\" NAME=\"" + RequestNameAdminDepth + "\" VALUE=\"" + adminData.ignore_legacyMenuDepth + "\">";
                s += "\r\n<input TYPE=\"hidden\" NAME=\"FormEmptyFieldList\" ID=\"FormEmptyFieldList\" VALUE=\",\">";
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return s;
        }
        //====================================================================================================

    }
}
