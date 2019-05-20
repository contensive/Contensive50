
using System;
using System.Collections.Generic;
using System.Data;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
using static Contensive.Addons.AdminSite.Controllers.AdminUIController;
using Contensive.Processor.Exceptions;
using Contensive.Addons.AdminSite.Controllers;
using Contensive.BaseClasses;
using Contensive.Processor;

namespace Contensive.Addons.AdminSite {
    public static class FormEdit {
        //
        // ====================================================================================================
        /// <summary>
        /// Create the tabs for editing a record
        /// </summary>
        /// <param name="adminData.content"></param>
        /// <param name="editRecord"></param>
        /// <returns></returns>
        public static string get(CoreController core, AdminDataModel adminData) {
            string returnHtml = "";
            try {
                bool AllowajaxTabs = (core.siteProperties.getBoolean("AllowAjaxEditTabBeta", false));
                var adminMenu = new TabController();
                //
                if ((!core.doc.userErrorList.Count.Equals(0)) & adminData.editRecord.Loaded) {
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
                        ContentFieldMetadataModel field = keyValuePair.Value;
                        if ((keyValuePair.Value.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.File) || (keyValuePair.Value.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.FileImage)) {
                            adminData.editRecord.fieldsLc[field.nameLc].value = adminData.editRecord.fieldsLc[field.nameLc].dbValue;
                        }
                    }
                } else {
                    //
                    // otherwise, load the record, even if it was loaded during a previous form process
                    adminData.LoadEditRecord(core, true);
                }
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
                Stream.Add(getForm_EditFormStart(core, adminData, AdminFormEdit));
                bool IsLandingPageParent = false;
                int TemplateIDForStyles = 0;
                bool IsTemplateTable = (adminData.adminContent.tableName.ToLowerInvariant() == Processor.Models.Db.PageTemplateModel.contentTableNameLowerCase);
                bool IsPageContentTable = (adminData.adminContent.tableName.ToLowerInvariant() == Processor.Models.Db.PageContentModel.contentTableNameLowerCase);
                bool IsEmailTable = (adminData.adminContent.tableName.ToLowerInvariant() == Processor.Models.Db.EmailModel.contentTableNameLowerCase);
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
                    recordLockById = adminData.editRecord.EditLock.editLockByMemberId,
                    recordLockExpiresDate = adminData.editRecord.EditLock.editLockExpiresDate,
                    //recordModifiedById = adminInfo.editRecord.modifiedBy.id,
                    recordName = adminData.editRecord.nameLc
                };
                string titleBarDetails = AdminUIController.getEditForm_TitleBarDetails(core, headerInfo, adminData.editRecord);
                //
                // ----- determine access details
                //
                var userContentPermissions = PermissionController.getUserContentPermissions(core, adminData.adminContent);
                bool allowAdd = adminData.adminContent.allowAdd && userContentPermissions.allowAdd;
                bool AllowDelete = adminData.adminContent.allowDelete & userContentPermissions.allowDelete & (adminData.editRecord.id != 0);
                bool allowSave = userContentPermissions.allowSave;
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
                var fieldTypeDefaultEditors = EditorController.getFieldTypeDefaultEditorAddonIdDictionary(core);
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
                CPHtml5BaseClass.EditorContentType ContentType;
                if (GenericController.vbLCase(adminData.adminContent.name) == "email templates") {
                    ContentType = CPHtml5BaseClass.EditorContentType.contentTypeEmailTemplate;
                } else if (GenericController.vbLCase(adminData.adminContent.tableName) == "cctemplates") {
                    ContentType = CPHtml5BaseClass.EditorContentType.contentTypeWebTemplate;
                } else if (GenericController.vbLCase(adminData.adminContent.tableName) == "ccemail") {
                    ContentType = CPHtml5BaseClass.EditorContentType.contentTypeEmail;
                } else {
                    ContentType = CPHtml5BaseClass.EditorContentType.contentTypeWeb;
                }
                //
                //-----Create edit page
                string styleOptionList = "";
                string editorAddonListJSON = core.html.getWysiwygAddonList(ContentType);
                string styleList = "";
                string adminContentTableNameLower = adminData.adminContent.tableName.ToLowerInvariant();
                //
                LogController.logTrace(core, "getFormEdit, adminInfo.editRecord.contentControlId [" + adminData.editRecord.contentControlId + "]");
                //
                if (adminContentTableNameLower == PersonModel.contentTableNameLowerCase) {
                    //
                    // -- people
                    if (!(core.session.isAuthenticatedAdmin(core))) {
                        //
                        // Must be admin
                        Stream.Add(AdminErrorController.get(core, "This edit form requires administrator access.", ""));
                    } else {
                        string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                            allowActivate = false,
                            allowAdd = (allowAdd && adminData.adminContent.allowAdd & adminData.editRecord.AllowUserAdd),
                            allowCancel = true,
                            allowCreateDuplicate = (allowSave && adminData.editRecord.AllowUserSave & (adminData.editRecord.id != 0)),
                            allowDelete = AllowDelete && adminData.editRecord.AllowUserDelete,
                            allowMarkReviewed = false,
                            allowRefresh = AllowRefresh,
                            allowSave = (allowSave && adminData.editRecord.AllowUserSave),
                            allowSend = false,
                            allowSendTest = false,
                            hasChildRecords = false,
                            isPageContent = false
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getTitleBar(core, titleBarDetails));
                        Stream.Add(getTabs(core, adminData, adminMenu, adminData.editRecord.userReadOnly, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(addTab(core, adminMenu, "Groups", GroupRuleEditor.get(core, adminData), adminData.allowAdminTabs));
                        Stream.Add(addTab(core, adminMenu, "Control&nbsp;Info", ControlEditor.get(core, adminData), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) Stream.Add(adminMenu.getTabs(core));
                        Stream.Add(EditSectionButtonBar);
                    }
                } else if (adminContentTableNameLower == EmailModel.contentTableNameLowerCase) {
                    //
                    LogController.logTrace(core, "getFormEdit, treat as email, adminContentTableNameLower [" + adminContentTableNameLower + "]");
                    //
                    // -- email
                    bool EmailSubmitted = false;
                    bool EmailSent = false;
                    DateTime LastSendTestDate = DateTime.MinValue;
                    bool AllowEmailSendWithoutTest = (core.siteProperties.getBoolean("AllowEmailSendWithoutTest", false));
                    if (adminData.editRecord.fieldsLc.ContainsKey("lastsendtestdate")) {
                        LastSendTestDate = GenericController.encodeDate(adminData.editRecord.fieldsLc["lastsendtestdate"].value);
                    }
                    if (!(core.session.isAuthenticatedAdmin(core))) {
                        //
                        // Must be admin
                        Stream.Add(AdminErrorController.get(core, "This edit form requires Member Administration access.", "This edit form requires Member Administration access."));
                    } else if (adminData.adminContent.id == ContentMetadataModel.getContentId(core, "System Email")) {
                        //
                        LogController.logTrace(core, "getFormEdit, System email");
                        //
                        // System Email
                        EmailSubmitted = false;
                        if (adminData.editRecord.id != 0) {
                            if (adminData.editRecord.fieldsLc.ContainsKey("testmemberid")) {
                                if (encodeInteger(adminData.editRecord.fieldsLc["testmemberid"].value) == 0) {
                                    adminData.editRecord.fieldsLc["testmemberid"].value = core.session.user.id;
                                }
                            }
                        }
                        string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                            allowActivate = false,
                            allowAdd = (allowAdd && adminData.adminContent.allowAdd & adminData.editRecord.AllowUserAdd),
                            allowCancel = true,
                            allowCreateDuplicate = (allowSave && adminData.editRecord.AllowUserSave & (adminData.editRecord.id != 0)),
                            allowDelete = AllowDelete && adminData.editRecord.AllowUserDelete && core.session.isAuthenticatedDeveloper(core),
                            allowMarkReviewed = false,
                            allowRefresh = AllowRefresh,
                            allowSave = (allowSave && adminData.editRecord.AllowUserSave && (!EmailSubmitted) && (!EmailSent)),
                            allowSend = false,
                            allowSendTest = ((!EmailSubmitted) && (!EmailSent)),
                            hasChildRecords = false,
                            isPageContent = false
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getTitleBar(core, titleBarDetails));
                        Stream.Add(getTabs(core, adminData, adminMenu, adminData.editRecord.userReadOnly, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(addTab(core, adminMenu, "Control&nbsp;Info", ControlEditor.get(core, adminData), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) Stream.Add(adminMenu.getTabs(core));
                        Stream.Add(EditSectionButtonBar);
                    } else if (adminData.adminContent.id == ContentMetadataModel.getContentId(core, "Conditional Email")) {
                        //
                        // Conditional Email
                        EmailSubmitted = false;
                        if (adminData.editRecord.id != 0) {
                            if (adminData.editRecord.fieldsLc.ContainsKey("submitted")) EmailSubmitted = GenericController.encodeBoolean(adminData.editRecord.fieldsLc["submitted"].value);
                        }
                        //
                        string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                            allowActivate = !EmailSubmitted & ((LastSendTestDate != DateTime.MinValue) || AllowEmailSendWithoutTest),
                            allowDeactivate = EmailSubmitted,
                            allowAdd = allowAdd && adminData.adminContent.allowAdd & adminData.editRecord.AllowUserAdd,
                            allowCancel = true,
                            allowCreateDuplicate = allowAdd && (adminData.editRecord.id != 0),
                            allowDelete = AllowDelete && adminData.editRecord.AllowUserDelete && core.session.isAuthenticatedDeveloper(core),
                            allowMarkReviewed = false,
                            allowRefresh = AllowRefresh,
                            allowSave = allowSave && adminData.editRecord.AllowUserSave && !EmailSubmitted,
                            allowSend = false,
                            allowSendTest = !EmailSubmitted,
                            hasChildRecords = false,
                            isPageContent = false
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getTitleBar(core, titleBarDetails));
                        Stream.Add(getTabs(core, adminData, adminMenu, adminData.editRecord.userReadOnly || EmailSubmitted, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(addTab(core, adminMenu, "Control&nbsp;Info", ControlEditor.get(core, adminData), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) Stream.Add(adminMenu.getTabs(core));
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
                            allowAdd = allowAdd && adminData.adminContent.allowAdd & adminData.editRecord.AllowUserAdd,
                            allowCancel = true,
                            allowCreateDuplicate = allowAdd && (adminData.editRecord.id != 0),
                            allowDelete = !EmailSubmitted & (AllowDelete && adminData.editRecord.AllowUserDelete),
                            allowMarkReviewed = false,
                            allowRefresh = AllowRefresh,
                            allowSave = !EmailSubmitted & (allowSave && adminData.editRecord.AllowUserSave),
                            allowSend = !EmailSubmitted & ((LastSendTestDate != DateTime.MinValue) || AllowEmailSendWithoutTest),
                            allowSendTest = !EmailSubmitted,
                            hasChildRecords = false,
                            isPageContent = false
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getTitleBar(core, titleBarDetails));
                        Stream.Add(getTabs(core, adminData, adminMenu, adminData.editRecord.userReadOnly || EmailSubmitted || EmailSent, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(addTab(core, adminMenu, "Control&nbsp;Info", ControlEditor.get(core, adminData), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) Stream.Add(adminMenu.getTabs(core));
                        Stream.Add(EditSectionButtonBar);
                    }
                } else if (adminData.adminContent.tableName.ToLowerInvariant() == ContentModel.contentTableNameLowerCase) {
                    if (!(core.session.isAuthenticatedAdmin(core))) {
                        //
                        // Must be admin
                        //
                        Stream.Add(AdminErrorController.get(core, "This edit form requires Member Administration access.", "This edit form requires Member Administration access."));
                    } else {
                        string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                            allowActivate = false,
                            allowAdd = (allowAdd && adminData.adminContent.allowAdd & adminData.editRecord.AllowUserAdd),
                            allowCancel = true,
                            allowCreateDuplicate = (allowSave && adminData.editRecord.AllowUserSave & (adminData.editRecord.id != 0)),
                            allowDelete = AllowDelete && adminData.editRecord.AllowUserDelete,
                            allowMarkReviewed = false,
                            allowRefresh = AllowRefresh,
                            allowSave = (allowSave && adminData.editRecord.AllowUserSave),
                            allowSend = false,
                            allowSendTest = false,
                            hasChildRecords = false,
                            isPageContent = false
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getTitleBar(core, titleBarDetails));
                        Stream.Add(getTabs(core, adminData, adminMenu, adminData.editRecord.userReadOnly, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(addTab(core, adminMenu, "Control&nbsp;Info", ControlEditor.get(core, adminData), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) {
                            Stream.Add(adminMenu.getTabs(core));
                        }
                        Stream.Add(EditSectionButtonBar);
                    }
                    //
                } else if (adminContentTableNameLower == PageContentModel.contentTableNameLowerCase) {
                    //
                    // Page Content
                    //
                    int TableID = MetadataController.getRecordIdByUniqueName(core, "Tables", "ccPageContent");
                    string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                        allowActivate = false,
                        allowAdd = (allowAdd && adminData.adminContent.allowAdd & adminData.editRecord.AllowUserAdd),
                        allowCancel = true,
                        allowCreateDuplicate = (allowSave && adminData.editRecord.AllowUserSave & (adminData.editRecord.id != 0)),
                        allowDelete = AllowDelete && adminData.editRecord.AllowUserDelete,
                        allowMarkReviewed = false,
                        allowRefresh = AllowRefresh,
                        allowSave = (allowSave && adminData.editRecord.AllowUserSave),
                        allowSend = false,
                        allowSendTest = false,
                        hasChildRecords = false,
                        isPageContent = false
                    });
                    Stream.Add(EditSectionButtonBar);
                    Stream.Add(AdminUIController.getTitleBar(core, titleBarDetails));
                    Stream.Add(getTabs(core, adminData, adminMenu, adminData.editRecord.userReadOnly, IsLandingPage || IsLandingPageParent, IsRootPage, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    Stream.Add(addTab(core, adminMenu, "Link Aliases", LinkAliasEditor.GetForm_Edit_LinkAliases(core, adminData, adminData.editRecord.userReadOnly), adminData.allowAdminTabs));
                    Stream.Add(addTab(core, adminMenu, "Content Watch", ContentTrackingEditor.get(core, adminData), adminData.allowAdminTabs));
                    Stream.Add(addTab(core, adminMenu, "Control Info", ControlEditor.get(core, adminData), adminData.allowAdminTabs));
                    if (adminData.allowAdminTabs) {
                        Stream.Add(adminMenu.getTabs(core));
                    }
                    Stream.Add(EditSectionButtonBar);
                } else {
                    //
                    // All other tables (User definined)
                    var pageContentMetadata = ContentMetadataModel.createByUniqueName(core, "page content");
                    bool HasChildRecords = adminData.adminContent.containsField(core, "parentid");
                    bool AllowMarkReviewed = core.db.isSQLTableField(adminData.adminContent.tableName, "DateReviewed");
                    string EditSectionButtonBar = AdminUIController.getButtonBarForEdit(core, new EditButtonBarInfoClass() {
                        allowActivate = false,
                        allowAdd = (allowAdd && adminData.adminContent.allowAdd & adminData.editRecord.AllowUserAdd),
                        allowCancel = true,
                        allowCreateDuplicate = (allowSave && adminData.editRecord.AllowUserSave & (adminData.editRecord.id != 0)),
                        allowDelete = AllowDelete && adminData.editRecord.AllowUserDelete,
                        allowMarkReviewed = AllowMarkReviewed,
                        allowRefresh = AllowRefresh,
                        allowSave = (allowSave && adminData.editRecord.AllowUserSave),
                        allowSend = false,
                        allowSendTest = false,
                        hasChildRecords = HasChildRecords,
                        isPageContent = pageContentMetadata.isParentOf(core, adminData.adminContent.id)
                    });
                    Stream.Add(EditSectionButtonBar);
                    Stream.Add(AdminUIController.getTitleBar(core, titleBarDetails));
                    Stream.Add(getTabs(core, adminData, adminMenu, adminData.editRecord.userReadOnly, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    Stream.Add(addTab(core, adminMenu, "Content Watch", ContentTrackingEditor.get(core, adminData), adminData.allowAdminTabs));
                    Stream.Add(addTab(core, adminMenu, "Control Info", ControlEditor.get(core, adminData), adminData.allowAdminTabs));
                    if (adminData.allowAdminTabs) Stream.Add(adminMenu.getTabs(core));
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
                LogController.logError(core, ex);
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
        public static string getTab(CoreController core, AdminDataModel adminData, int RecordID, int ContentID, bool record_readOnly, bool IsLandingPage, bool IsRootPage, string EditTab, CPHtml5BaseClass.EditorContentType EditorContext, ref string return_NewFieldList, int TemplateIDForStyles, int HelpCnt, int[] HelpIDCache, string[] helpDefaultCache, string[] HelpCustomCache, bool AllowHelpMsgCustom, KeyPtrController helpIdIndex, Dictionary<int, int> fieldTypeDefaultEditors, string fieldEditorPreferenceList, string styleList, string styleOptionList, int emailIdForStyles, bool IsTemplateTable, string editorAddonListJSON) {
            string returnHtml = "";
            try {
                //
                // ----- Open the panel
                if (adminData.adminContent.fields.Count <= 0) {
                    //
                    // There are no visible fiels, return empty
                    LogController.logError(core, new GenericException("There is no metadata for this field."));
                } else {
                    //
                    // ----- Build an index to sort the fields by EditSortOrder
                    Dictionary<string, ContentFieldMetadataModel> sortingFields = new Dictionary<string, ContentFieldMetadataModel>();
                    foreach (var keyValuePair in adminData.adminContent.fields) {
                        ContentFieldMetadataModel field = keyValuePair.Value;
                        if (field.editTabName.ToLowerInvariant() == EditTab.ToLowerInvariant()) {
                            if (AdminDataModel.IsVisibleUserField(core, field.adminOnly, field.developerOnly, field.active, field.authorable, field.nameLc, adminData.adminContent.tableName)) {
                                string AlphaSort = GenericController.getIntegerString(field.editSortPriority, 10) + "-" + GenericController.getIntegerString(field.id, 10);
                                sortingFields.Add(AlphaSort, field);
                            }
                        }
                    }
                    //
                    // ----- display the record fields
                    bool AllowHelpIcon = core.visitProperty.getBoolean("AllowHelpIcon");
                    string fieldCaption = "";
                    StringBuilderLegacyController resultBody = new StringBuilderLegacyController();
                    bool needUniqueEmailMessage = false;
                    foreach (var kvp in sortingFields) {
                        ContentFieldMetadataModel field = kvp.Value;
                        string WhyReadOnlyMsg = "";
                        CPContentBaseClass.fileTypeIdEnum fieldTypeId = field.fieldTypeId;
                        EditRecordClass editRecord = adminData.editRecord;
                        object fieldValueObject = editRecord.fieldsLc[field.nameLc].value;
                        string fieldValue_text = GenericController.encodeText(fieldValueObject);
                        int FieldRows = 1;
                        string fieldHtmlId = field.nameLc + field.id.ToString();
                        fieldCaption = field.caption;
                        if (field.uniqueName) {
                            fieldCaption = "&nbsp;**" + fieldCaption;
                        } else {
                            if (field.nameLc.ToLowerInvariant() == "email") {
                                if ((adminData.adminContent.tableName.ToLowerInvariant() == "ccmembers") && ((core.siteProperties.getBoolean("allowemaillogin", false)))) {
                                    fieldCaption = "&nbsp;***" + fieldCaption;
                                    needUniqueEmailMessage = true;
                                }
                            }
                        }
                        if (field.required) {
                            fieldCaption = "&nbsp;*" + fieldCaption;
                        }
                        bool IsBaseField = field.blockAccess;
                        adminData.FormInputCount = adminData.FormInputCount + 1;
                        bool fieldForceReadOnly = false;
                        //
                        // Read only Special Cases
                        if (IsLandingPage) {
                            switch (GenericController.vbLCase(field.nameLc)) {
                                case "active":
                                    //
                                    // if active, it is read only -- if inactive, let them set it active.
                                    fieldForceReadOnly = (GenericController.encodeBoolean(fieldValueObject));
                                    if (fieldForceReadOnly) {
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
                                    fieldForceReadOnly = true;
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
                                    fieldForceReadOnly = true;
                                    WhyReadOnlyMsg = "&nbsp;(disabled for root pages)";
                                    break;
                                case "allowinmenus":
                                case "allowinchildlists":
                                    fieldValueObject = "1";
                                    fieldForceReadOnly = true;
                                    WhyReadOnlyMsg = "&nbsp;(disabled for root pages)";
                                    break;
                            }
                        }
                        //
                        // Special Case - ccemail table Alloweid should be disabled if siteproperty AllowLinkLogin is false
                        //
                        if (GenericController.vbLCase(adminData.adminContent.tableName) == "ccemail" && GenericController.vbLCase(field.nameLc) == "allowlinkeid") {
                            if (!(core.siteProperties.getBoolean("AllowLinkLogin", true))) {
                                fieldValueObject = "0";
                                fieldForceReadOnly = true;
                                fieldValue_text = "0";
                            }
                        }
                        string EditorString = "";
                        bool editorReadOnly = (record_readOnly || field.readOnly || (editRecord.id != 0 & field.notEditable) || (fieldForceReadOnly));
                        //
                        // Determine the editor: Contensive editor, field type default, or add-on preference
                        int editorAddonID = 0;
                        int fieldIdPos = GenericController.vbInstr(1, "," + fieldEditorPreferenceList, "," + field.id.ToString() + ":");
                        while ((editorAddonID == 0) && (fieldIdPos > 0)) {
                            fieldIdPos = fieldIdPos + 1 + field.id.ToString().Length;
                            int Pos = GenericController.vbInstr(fieldIdPos, fieldEditorPreferenceList + ",", ",");
                            if (Pos > 0) {
                                editorAddonID = GenericController.encodeInteger(fieldEditorPreferenceList.Substring(fieldIdPos - 1, Pos - fieldIdPos));
                            }
                            fieldIdPos = GenericController.vbInstr(fieldIdPos + 1, "," + fieldEditorPreferenceList, "," + field.id.ToString() + ":");
                        }
                        int fieldTypeDefaultEditorAddonId = 0;
                        if ((editorAddonID == 0) && (fieldTypeDefaultEditors.ContainsKey((int)fieldTypeId))) {
                            fieldTypeDefaultEditorAddonId = fieldTypeDefaultEditors[(int)fieldTypeId];
                            editorAddonID = fieldTypeDefaultEditorAddonId;
                        }
                        bool useEditorAddon = false;
                        if (editorAddonID != 0) {
                            //
                            //--------------------------------------------------------------------------------------------
                            // ----- Custom Editor
                            //--------------------------------------------------------------------------------------------
                            //
                            core.docProperties.setProperty("editorName", field.nameLc);
                            core.docProperties.setProperty("editorValue", fieldValue_text);
                            core.docProperties.setProperty("editorFieldId", field.id);
                            core.docProperties.setProperty("editorFieldType", (int)fieldTypeId);
                            core.docProperties.setProperty("editorReadOnly", editorReadOnly);
                            core.docProperties.setProperty("editorWidth", "");
                            core.docProperties.setProperty("editorHeight", "");
                            if (GenericController.encodeBoolean((fieldTypeId == CPContentBaseClass.fileTypeIdEnum.HTML) || (fieldTypeId == CPContentBaseClass.fileTypeIdEnum.FileHTML))) {
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
                                using (var csData = new CsModel(core)) {
                                    if (!csData.openSql("select id from ccaggregatefunctions where id=" + editorAddonID)) {
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
                                        int PosStart = GenericController.vbInstr(1, "," + tmpList, "," + field.id + ":");
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
                                }
                            }
                        }
                        if (!useEditorAddon) {
                            bool IsEmptyList = false;
                            string NonEncodedLink = null;
                            string EncodedLink = null;
                            //
                            // if custom editor not used or if it failed
                            //
                            if (fieldTypeId == CPContentBaseClass.fileTypeIdEnum.Redirect) {
                                //
                                // ----- Default Editor, Redirect fields (the same for normal/readonly/spelling)
                                string RedirectPath = core.appConfig.adminRoute;
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
                            } else if (editorReadOnly) {
                                //
                                //--------------------------------------------------------------------------------------------
                                // ----- Display fields as read only
                                //--------------------------------------------------------------------------------------------
                                //
                                if (!string.IsNullOrEmpty(WhyReadOnlyMsg)) {
                                    WhyReadOnlyMsg = "<span class=\"ccDisabledReason\">" + WhyReadOnlyMsg + "</span>";
                                }
                                double FieldValueNumber = 0;
                                switch (fieldTypeId) {
                                    case CPContentBaseClass.fileTypeIdEnum.Text:
                                    case CPContentBaseClass.fileTypeIdEnum.Link:
                                    case CPContentBaseClass.fileTypeIdEnum.ResourceLink:
                                        //
                                        // ----- Text Type
                                        EditorString += AdminUIController.getDefaultEditor_text(core, field.nameLc, fieldValue_text, editorReadOnly, fieldHtmlId);
                                        return_NewFieldList += "," + field.nameLc;
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.Boolean:
                                        //
                                        // ----- Boolean ReadOnly
                                        EditorString += AdminUIController.getDefaultEditor_bool(core, field.nameLc, GenericController.encodeBoolean(fieldValueObject), editorReadOnly, fieldHtmlId);
                                        return_NewFieldList += "," + field.nameLc;
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.Lookup:
                                        //
                                        // ----- Lookup, readonly
                                        if (field.lookupContentID != 0) {
                                            EditorString = AdminUIController.getDefaultEditor_lookupContent(core, field.nameLc, encodeInteger(fieldValueObject), field.lookupContentID, ref IsEmptyList, editorReadOnly, fieldHtmlId, WhyReadOnlyMsg, field.required);
                                            return_NewFieldList += "," + field.nameLc;
                                        } else if (field.lookupList != "") {
                                            EditorString = AdminUIController.getDefaultEditor_lookupList(core, field.nameLc, encodeInteger(fieldValueObject), field.lookupList.Split(','), editorReadOnly, fieldHtmlId, WhyReadOnlyMsg, field.required);
                                            return_NewFieldList += "," + field.nameLc;
                                        } else {
                                            //
                                            // -- log exception but dont throw
                                            LogController.logWarn(core, new GenericException("Field [" + adminData.adminContent.name + "." + field.nameLc + "] is a Lookup field, but no LookupContent or LookupList has been configured"));
                                            EditorString += "[Selection not configured]";
                                        }
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.Date:
                                        //
                                        // ----- date, readonly
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = AdminUIController.getDefaultEditor_dateTime(core, field.nameLc, encodeDate(fieldValueObject), editorReadOnly, fieldHtmlId, field.required, WhyReadOnlyMsg);
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.MemberSelect:
                                        //
                                        // ----- Member Select ReadOnly
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = AdminUIController.getDefaultEditor_memberSelect(core, field.nameLc, encodeInteger(fieldValueObject), field.memberSelectGroupId_get(core), field.memberSelectGroupName_get(core), editorReadOnly, fieldHtmlId, field.required, WhyReadOnlyMsg);
                                        //
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.ManyToMany:
                                        //
                                        //   Placeholder
                                        EditorString = AdminUIController.getDefaultEditor_manyToMany(core, field, "field" + field.id, fieldValue_text, editRecord.id, editorReadOnly, WhyReadOnlyMsg);
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.Currency:
                                        //
                                        // ----- Currency ReadOnly
                                        return_NewFieldList += "," + field.nameLc;
                                        FieldValueNumber = GenericController.encodeNumber(fieldValueObject);
                                        EditorString += (HtmlController.inputHidden(field.nameLc, GenericController.encodeText(FieldValueNumber)));
                                        EditorString += (HtmlController.inputText(core, field.nameLc, FieldValueNumber.ToString(), -1, -1, fieldHtmlId, false, editorReadOnly, "text form-control"));
                                        EditorString += (string.Format("{0:C}", FieldValueNumber));
                                        EditorString += WhyReadOnlyMsg;
                                        //
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.AutoIdIncrement:
                                    case CPContentBaseClass.fileTypeIdEnum.Float:
                                    case CPContentBaseClass.fileTypeIdEnum.Integer:
                                        //
                                        // ----- number readonly
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString += (HtmlController.inputHidden(field.nameLc, fieldValue_text));
                                        EditorString += (HtmlController.inputText(core, field.nameLc, fieldValue_text, -1, -1, fieldHtmlId, false, editorReadOnly, "number form-control"));
                                        EditorString += WhyReadOnlyMsg;
                                        //
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.HTML:
                                    case CPContentBaseClass.fileTypeIdEnum.FileHTML:
                                        //
                                        // ----- HTML types readonly
                                        if (field.htmlContent) {
                                            //
                                            // edit html as html (see the code)
                                            return_NewFieldList += "," + field.nameLc;
                                            EditorString += HtmlController.inputHidden(field.nameLc, fieldValue_text);
                                            FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                            EditorString += HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, fieldHtmlId, false, editorReadOnly, "form-control");
                                        } else {
                                            //
                                            // edit html as wysiwyg readonly
                                            return_NewFieldList += "," + field.nameLc;
                                            EditorString += AdminUIController.getDefaultEditor_Html(core, field.nameLc, fieldValue_text, editorAddonListJSON, styleList, styleOptionList, editorReadOnly, fieldHtmlId);
                                        }
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.LongText:
                                    case CPContentBaseClass.fileTypeIdEnum.FileText:
                                        //
                                        // ----- LongText, TextFile
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString += HtmlController.inputHidden(field.nameLc, fieldValue_text);
                                        FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                        EditorString += HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, fieldHtmlId, false, editorReadOnly, " form-control");
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.File:
                                    case CPContentBaseClass.fileTypeIdEnum.FileImage:
                                        //
                                        // ----- File ReadOnly
                                        return_NewFieldList += "," + field.nameLc;
                                        NonEncodedLink = GenericController.getCdnFileLink(core, fieldValue_text);
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
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString += HtmlController.inputHidden(field.nameLc, fieldValue_text);
                                        if (field.password) {
                                            //
                                            // Password forces simple text box
                                            EditorString += HtmlController.inputText(core, field.nameLc, "*****", 0, 0, fieldHtmlId, true, true, "password form-control");
                                        } else if (!field.htmlContent) {
                                            //
                                            // not HTML capable, textarea with resizing
                                            if ((fieldTypeId == CPContentBaseClass.fileTypeIdEnum.Text) && (fieldValue_text.IndexOf("\n") == -1) && (fieldValue_text.Length < 40)) {
                                                //
                                                // text field shorter then 40 characters without a CR
                                                EditorString += HtmlController.inputText(core, field.nameLc, fieldValue_text, 1, 0, fieldHtmlId, false, true, "text form-control");
                                            } else {
                                                //
                                                // longer text data, or text that contains a CR
                                                EditorString += HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, 10, -1, fieldHtmlId, false, true, " form-control");
                                            }
                                        } else if (field.htmlContent) {
                                            //
                                            // HTMLContent true, and prefered
                                            FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".PixelHeight", 500));
                                            EditorString += core.html.getFormInputHTML(field.nameLc, fieldValue_text, "500", "", false, true, editorAddonListJSON, styleList, styleOptionList);
                                            EditorString = "<div style=\"width:95%\">" + EditorString + "</div>";
                                        } else {
                                            //
                                            // HTMLContent true, but text editor selected
                                            FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                            EditorString += HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, fieldHtmlId, false, editorReadOnly);
                                        }
                                        break;
                                }
                            } else {
                                //
                                // -- Not Read Only - Display fields as form elements to be modified
                                switch (fieldTypeId) {
                                    case CPContentBaseClass.fileTypeIdEnum.Text:
                                        //
                                        // ----- Text Type
                                        if (field.password) {
                                            EditorString += AdminUIController.getDefaultEditor_Password(core, field.nameLc, fieldValue_text, false, fieldHtmlId);
                                        } else {
                                            EditorString += AdminUIController.getDefaultEditor_text(core, field.nameLc, fieldValue_text, false, fieldHtmlId);
                                        }
                                        return_NewFieldList += "," + field.nameLc;
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.Boolean:
                                        //
                                        // ----- Boolean
                                        EditorString += AdminUIController.getDefaultEditor_bool(core, field.nameLc, GenericController.encodeBoolean(fieldValueObject), false, fieldHtmlId);
                                        return_NewFieldList += "," + field.nameLc;
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.Lookup:
                                        //
                                        // ----- Lookup
                                        if (field.lookupContentID != 0) {
                                            EditorString = AdminUIController.getDefaultEditor_lookupContent(core, field.nameLc, encodeInteger(fieldValueObject), field.lookupContentID, ref IsEmptyList, field.readOnly, fieldHtmlId, WhyReadOnlyMsg, field.required);
                                            return_NewFieldList += "," + field.nameLc;
                                        } else if (field.lookupList != "") {
                                            EditorString = AdminUIController.getDefaultEditor_lookupList(core, field.nameLc, encodeInteger(fieldValueObject), field.lookupList.Split(','), field.readOnly, fieldHtmlId, WhyReadOnlyMsg, field.required);
                                            return_NewFieldList += "," + field.nameLc;
                                        } else {
                                            //
                                            // -- log exception but dont throw
                                            LogController.logWarn(core, new GenericException("Field [" + adminData.adminContent.name + "." + field.nameLc + "] is a Lookup field, but no LookupContent or LookupList has been configured"));
                                            EditorString += "[Selection not configured]";
                                        }
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.Date:
                                        //
                                        // ----- Date
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = AdminUIController.getDefaultEditor_dateTime(core, field.nameLc, GenericController.encodeDate(fieldValueObject), field.readOnly, fieldHtmlId, field.required, WhyReadOnlyMsg);
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.MemberSelect:
                                        //
                                        // ----- Member Select
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = AdminUIController.getDefaultEditor_memberSelect(core, field.nameLc, encodeInteger(fieldValueObject), field.memberSelectGroupId_get(core), field.memberSelectGroupName_get(core), field.readOnly, fieldHtmlId, field.required, WhyReadOnlyMsg);
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.ManyToMany:
                                        //
                                        //   Placeholder
                                        EditorString = AdminUIController.getDefaultEditor_manyToMany(core, field, "field" + field.id, fieldValue_text, editRecord.id, false, WhyReadOnlyMsg);
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.File:
                                    case CPContentBaseClass.fileTypeIdEnum.FileImage:
                                        //
                                        // ----- File
                                        return_NewFieldList += "," + field.nameLc;
                                        if (string.IsNullOrEmpty(fieldValue_text)) {
                                            EditorString += (core.html.inputFile(field.nameLc, "", "file form-control"));
                                        } else {
                                            NonEncodedLink = GenericController.getCdnFileLink(core, fieldValue_text);
                                            EncodedLink = HtmlController.encodeHtml(NonEncodedLink);
                                            string filename = "";
                                            string path = "";
                                            core.cdnFiles.splitDosPathFilename(fieldValue_text, ref path, ref filename);
                                            EditorString += ("&nbsp;<a href=\"" + EncodedLink + "\" target=\"_blank\">" + SpanClassAdminSmall + "[" + filename + "]</A>");
                                            EditorString += ("&nbsp;&nbsp;&nbsp;Delete:&nbsp;" + HtmlController.checkbox(field.nameLc + ".DeleteFlag", false));
                                            EditorString += ("&nbsp;&nbsp;&nbsp;Change:&nbsp;" + core.html.inputFile(field.nameLc, fieldHtmlId, "file form-control"));
                                        }
                                        //
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.AutoIdIncrement:
                                    case CPContentBaseClass.fileTypeIdEnum.Currency:
                                    case CPContentBaseClass.fileTypeIdEnum.Float:
                                    case CPContentBaseClass.fileTypeIdEnum.Integer:
                                        //
                                        // ----- Others that simply print
                                        return_NewFieldList += "," + field.nameLc;
                                        if (field.password) {
                                            EditorString += (HtmlController.inputText(core, field.nameLc, fieldValue_text, -1, -1, fieldHtmlId, true, false, "password form-control"));
                                        } else {
                                            if (string.IsNullOrEmpty(fieldValue_text)) {
                                                EditorString += (HtmlController.inputText(core, field.nameLc, "", -1, -1, fieldHtmlId, false, false, "text form-control"));
                                            } else {
                                                if (encodeBoolean(fieldValue_text.IndexOf("\n") + 1) || (fieldValue_text.Length > 40)) {
                                                    EditorString += (HtmlController.inputText(core, field.nameLc, fieldValue_text, -1, -1, fieldHtmlId, false, false, "text form-control"));
                                                } else {
                                                    EditorString += (HtmlController.inputText(core, field.nameLc, fieldValue_text, 1, -1, fieldHtmlId, false, false, "text form-control"));
                                                }
                                            }
                                        }
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.Link:
                                        //
                                        // ----- Link (href value
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = ""
                                            + HtmlController.inputText(core, field.nameLc, fieldValue_text, 1, 80, fieldHtmlId, false, false, "link form-control") + "&nbsp;<a href=\"#\" onClick=\"OpenResourceLinkWindow( '" + field.nameLc + "' ) ;return false;\"><img src=\"/ContensiveBase/images/ResourceLink1616.gif\" width=16 height=16 border=0 alt=\"Link to a resource\" title=\"Link to a resource\"></a>"
                                            + "&nbsp;<a href=\"#\" onClick=\"OpenSiteExplorerWindow( '" + field.nameLc + "' ) ;return false;\"><img src=\"/ContensiveBase/images/PageLink1616.gif\" width=16 height=16 border=0 alt=\"Link to a page\" title=\"Link to a page\"></a>";
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.ResourceLink:
                                        //
                                        // ----- Resource Link (src value)
                                        //
                                        return_NewFieldList += "," + field.nameLc;
                                        EditorString = HtmlController.inputText(core, field.nameLc, fieldValue_text, 1, 80, fieldHtmlId, false, false, "resourceLink form-control") + "&nbsp;<a href=\"#\" onClick=\"OpenResourceLinkWindow( '" + field.nameLc + "' ) ;return false;\"><img src=\"/ContensiveBase/images/ResourceLink1616.gif\" width=16 height=16 border=0 alt=\"Link to a resource\" title=\"Link to a resource\"></a>";
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.HTML:
                                    case CPContentBaseClass.fileTypeIdEnum.FileHTML:
                                        //
                                        // content is html
                                        return_NewFieldList += "," + field.nameLc;
                                        if (field.htmlContent) {
                                            //
                                            // View the content as Html, not wysiwyg
                                            EditorString = AdminUIController.getDefaultEditor_TextArea(core, field.nameLc, fieldValue_text, editorReadOnly, fieldHtmlId);
                                        } else {
                                            //
                                            // wysiwyg editor
                                            EditorString = AdminUIController.getDefaultEditor_Html(core, field.nameLc, fieldValue_text, editorAddonListJSON, styleList, styleOptionList, editorReadOnly, fieldHtmlId);
                                        }
                                        //
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.LongText:
                                    case CPContentBaseClass.fileTypeIdEnum.FileText:
                                        //
                                        // -- Long Text, use text editor
                                        return_NewFieldList += "," + field.nameLc;
                                        FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                        EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, fieldHtmlId, false, false, "text form-control");
                                        //
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.FileCSS:
                                        //
                                        // ----- CSS field
                                        return_NewFieldList += "," + field.nameLc;
                                        FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                        EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, 10, -1, fieldHtmlId, false, false, "styles form-control");
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.FileJavascript:
                                        //
                                        // ----- Javascript field
                                        return_NewFieldList += "," + field.nameLc;
                                        FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                        EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, fieldHtmlId, false, false, "text form-control");
                                        //
                                        break;
                                    case CPContentBaseClass.fileTypeIdEnum.FileXML:
                                        //
                                        // ----- xml field
                                        return_NewFieldList += "," + field.nameLc;
                                        FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                        EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, fieldHtmlId, false, false, "text form-control");
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
                                            EditorString = HtmlController.inputText(core, field.nameLc, fieldValue_text, -1, -1, fieldHtmlId, true, false, "password form-control");
                                        } else if (!field.htmlContent) {
                                            //
                                            // not HTML capable, textarea with resizing
                                            //
                                            if ((fieldTypeId == CPContentBaseClass.fileTypeIdEnum.Text) && (fieldValue_text.IndexOf("\n") == -1) && (fieldValue_text.Length < 40)) {
                                                //
                                                // text field shorter then 40 characters without a CR
                                                //
                                                EditorString = HtmlController.inputText(core, field.nameLc, fieldValue_text, 1, -1, fieldHtmlId, false, false, "text form-control");
                                            } else {
                                                //
                                                // longer text data, or text that contains a CR
                                                //
                                                //EditorStyleModifier = "textexpandable";
                                                EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, 10, -1, fieldHtmlId, false, false, "text form-control");
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
                                            FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".PixelHeight", 500));
                                            EditorString += core.html.getFormInputHTML(field.nameLc, fieldValue_text, "500", "", false, true, editorAddonListJSON, styleList, styleOptionList);
                                            EditorString = "<div style=\"width:95%\">" + EditorString + "</div>";
                                        } else {
                                            //
                                            // HTMLContent true, but text editor selected
                                            FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                            EditorString = HtmlController.inputTextarea(core, field.nameLc, HtmlController.encodeHtml(fieldValue_text), FieldRows, -1, fieldHtmlId, false, false, "text");
                                        }
                                       break;
                                }
                            }
                        }
                        //
                        // Build Help Line Below editor
                        //
                        adminData.includeFancyBox = true;
                        string HelpMsgDefault = "";
                        string HelpMsgCustom = "";
                        string EditorHelp = "";
                        string LcaseName = GenericController.vbLCase(field.nameLc);
                        if (AllowHelpMsgCustom) {
                            HelpMsgDefault = field.helpDefault;
                            HelpMsgCustom = field.helpCustom;
                        }
                        string HelpMsg = null;
                        if (!string.IsNullOrEmpty(HelpMsgCustom)) {
                            HelpMsg = HelpMsgCustom;
                        } else {
                            HelpMsg = HelpMsgDefault;
                        }
                        string HelpMsgOpenedRead = HelpMsg;
                        string HelpMsgClosed = HelpMsg;
                        bool IsEmptyHelp = HelpMsgClosed.Length == 0;
                        bool IsLongHelp = (HelpMsgClosed.Length > 100);
                        if (IsLongHelp) {
                            HelpMsgClosed = HelpMsgClosed.Left(100) + "...";
                        }
                        //
                        string HelpID = "helpId" + field.id;
                        string HelpEditorID = "helpEditorId" + field.id;
                        string HelpOpenedReadID = "HelpOpenedReadID" + field.id;
                        string HelpOpenedEditID = "HelpOpenedEditID" + field.id;
                        string HelpClosedID = "helpClosedId" + field.id;
                        string HelpClosedContentID = "helpClosedContentId" + field.id;
                        //
                        // editor preferences form - a fancybox popup that interfaces with a hardcoded ajax function in init() to set a member property
                        string AjaxQS = RequestNameAjaxFunction + "=" + ajaxGetFieldEditorPreferenceForm + "&fieldid=" + field.id + "&currentEditorAddonId=" + editorAddonID + "&fieldTypeDefaultEditorAddonId=" + fieldTypeDefaultEditorAddonId;
                        string fancyBoxLinkId = "fbl" + adminData.fancyBoxPtr;
                        string fancyBoxContentId = "fbc" + adminData.fancyBoxPtr;
                        adminData.fancyBoxHeadJS = adminData.fancyBoxHeadJS + "\r\njQuery('#" + fancyBoxLinkId + "').fancybox({"
                            + "'titleShow':false,"
                            + "'transitionIn':'elastic',"
                            + "'transitionOut':'elastic',"
                            + "'overlayOpacity':'.2',"
                            + "'overlayColor':'#000000',"
                            + "'onStart':function(){cj.ajax.qs('" + AjaxQS + "','','" + fancyBoxContentId + "')}"
                            + "});";
                        EditorHelp = EditorHelp + "\r<div style=\"float:right;\">"
                            + cr2 + "<a id=\"" + fancyBoxLinkId + "\" href=\"#" + fancyBoxContentId + "\" title=\"select an alternate editor for this field.\" tabindex=\"-1\"><img src=\"/ContensiveBase/images/NavAltEditor.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"select an alternate editor for this field.\"></a>"
                            + cr2 + "<div style=\"display:none;\">"
                            + cr3 + "<div class=\"ccEditorPreferenceCon\" id=\"" + fancyBoxContentId + "\"><div style=\"margin:20px auto auto auto;\"><img src=\"/ContensiveBase/images/ajax-loader-big.gif\" width=\"32\" height=\"32\"></div></div>"
                            + cr2 + "</div>"
                            + "\r</div>"
                            + "";
                        adminData.fancyBoxPtr = adminData.fancyBoxPtr + 1;
                        string HelpMsgOpenedEdit = null;
                        //
                        // field help
                        if (core.session.isAuthenticatedAdmin(core)) {
                            //
                            // Admin view
                            //
                            if (string.IsNullOrEmpty(HelpMsgDefault)) {
                                HelpMsgDefault = "Admin: No default help is available for this field.";
                            }
                            HelpMsgOpenedRead = ""
                                    + "<!-- close icon --><div class=\"\" style=\"float:right\"><a href=\"javascript:cj.hide('" + HelpOpenedReadID + "');cj.show('" + HelpClosedID + "');\"><img src=\"/ContensiveBase/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"close\"></a></div>"
                                    + "<div class=\"header\">Default Help</div>"
                                    + "<div class=\"body\">" + HelpMsgDefault + "</div>"
                                    + "<div class=\"header\">Custom Help</div>"
                                    + "<div class=\"body\">" + HelpMsgCustom + "</div>"
                                + "";
                            string jsUpdate = "updateFieldHelp('" + field.id + "','" + HelpEditorID + "','" + HelpClosedContentID + "');cj.hide('" + HelpOpenedEditID + "');cj.show('" + HelpClosedID + "');return false";
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
                            if (IsLongHelp) {
                                //
                                // Long help, closed gets MoreHelpIcon (opens to HelpMsgOpenedRead) and HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                HelpMsgClosed = ""
                                        + "<!-- open read icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide('" + HelpClosedID + "');cj.show('" + HelpOpenedReadID + "');\" tabindex=\"-1\"><img src=\"/ContensiveBase/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"more help\"></a></div>"
                                        + "<!-- open edit icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide('" + HelpClosedID + "');cj.show('" + HelpOpenedEditID + "');\" tabindex=\"-1\"><img src=\"/ContensiveBase/images/NavHelpEdit.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"edit help\"></a></div>"
                                        + "<div id=\"" + HelpClosedContentID + "\">" + HelpMsgClosed + "</div>"
                                    + "";
                            } else if (!IsEmptyHelp) {
                                //
                                // short help, closed gets helpmsgclosed + HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                HelpMsgClosed = ""
                                        + "<!-- open edit icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide('" + HelpClosedID + "');cj.show('" + HelpOpenedEditID + "');\" tabindex=\"-1\"><img src=\"/ContensiveBase/images/NavHelpEdit.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"edit help\"></a></div>"
                                        + "<div id=\"" + HelpClosedContentID + "\">" + HelpMsgClosed + "</div>"
                                    + "";
                            } else {
                                //
                                // Empty help, closed gets helpmsgclosed + HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                HelpMsgClosed = ""
                                        + "<!-- open edit icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide('" + HelpClosedID + "');cj.show('" + HelpOpenedEditID + "');\" tabindex=\"-1\"><img src=\"/ContensiveBase/images/NavHelpEdit.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"edit help\"></a></div>"
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
                            HelpMsgOpenedRead = ""
                                    + "<div class=\"body\">"
                                    + "<!-- close icon --><a href=\"javascript:cj.hide('" + HelpOpenedReadID + "');cj.show('" + HelpClosedID + "');\"><img src=\"/ContensiveBase/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;float:right\" title=\"close\"></a>"
                                    + HelpMsg + "</div>"
                                + "";
                           HelpMsgOpenedEdit = ""
                                + "";
                            if (IsLongHelp) {
                                //
                                // Long help
                                HelpMsgClosed = ""
                                    + "<div class=\"body\">"
                                    + "<!-- open read icon --><a href=\"javascript:cj.hide('" + HelpClosedID + "');cj.show('" + HelpOpenedReadID + "');\"><img src=\"/ContensiveBase/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;float:right;\" title=\"more help\"></a>"
                                    + HelpMsgClosed + "</div>"
                                    + "";
                            } else if (!IsEmptyHelp) {
                                //
                                // short help
                                HelpMsgClosed = ""
                                    + "<div class=\"body\">"
                                        + HelpMsg + "</div>"
                                    + "";
                            } else {
                                //
                                // no help
                                HelpMsgClosed = ""
                                    + "";
                            }
                            EditorHelp = EditorHelp + "<div id=\"" + HelpOpenedReadID + "\" class=\"opened\">" + HelpMsgOpenedRead + "</div>"
                                + "<div id=\"" + HelpClosedID + "\" class=\"closed\">" + HelpMsgClosed + "</div>"
                                + "";
                        }
                        //
                        // assemble the editor row
                        string editorRow = AdminUIController.getEditRow(core, EditorString, fieldCaption, field.helpDefault, field.required, false, fieldHtmlId);
                        resultBody.Add("<tr><td colspan=2>" + editorRow + "</td></tr>");
                    }
                    //
                    // ----- add the *Required Fields footer
                    resultBody.Add("<tr><td colspan=2 style=\"padding-top:10px;font-size:70%\"><div>* Field is required.</div><div>** Field must be unique.</div>");
                    if (needUniqueEmailMessage) {
                        resultBody.Add("<div>*** Field must be unique because this site allows login by email.</div>");
                    }
                    resultBody.Add("</td></tr>");
                    //
                    // ----- close the panel
                    if (string.IsNullOrEmpty(EditTab)) {
                        fieldCaption = "Content Fields";
                    } else {
                        fieldCaption = "Content Fields - " + EditTab;
                    }
                    adminData.EditSectionPanelCount = adminData.EditSectionPanelCount + 1;
                    returnHtml = AdminUIController.getEditPanel(core, (!adminData.allowAdminTabs), fieldCaption, "", AdminUIController.editTable(resultBody.Text));
                    adminData.EditSectionPanelCount = adminData.EditSectionPanelCount + 1;
                    resultBody = null;
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
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
        public static string getTabs(CoreController core, AdminDataModel adminData, TabController adminMenu, bool readOnlyField, bool IsLandingPage, bool IsRootPage, CPHtml5BaseClass.EditorContentType EditorContext, bool allowAjaxTabs, int TemplateIDForStyles, Dictionary<int, int> fieldTypeDefaultEditors, string fieldEditorPreferenceList, string styleList, string styleOptionList, int emailIdForStyles, bool IsTemplateTable, string editorAddonListJSON) {
            string returnHtml = "";
            try {
                // todo
                string IDList = "";
                foreach (KeyValuePair<string, ContentFieldMetadataModel> keyValuePair in adminData.adminContent.fields) {
                    ContentFieldMetadataModel field = keyValuePair.Value;
                    IDList = IDList + "," + field.id;
                }
                if (!string.IsNullOrEmpty(IDList)) {
                    IDList = IDList.Substring(1);
                }
                //
                DataTable dt = core.db.executeQuery("select fieldid,helpdefault,helpcustom from ccfieldhelp where fieldid in (" + IDList + ") order by fieldid,id");
                string[,] fieldHelpArray = core.db.convertDataTabletoArray(dt);
                bool AllowHelpMsgCustom = false;
                int HelpCnt = 0;
                int[] HelpIDCache = { };
                string[] helpDefaultCache = { };
                string[] HelpCustomCache = { };
                KeyPtrController helpIdIndex = new KeyPtrController();
                if (fieldHelpArray.GetLength(0) > 0) {
                    HelpCnt = fieldHelpArray.GetUpperBound(1) + 1;
                    HelpIDCache = new int[HelpCnt + 1];
                    helpDefaultCache = new string[HelpCnt + 1];
                    HelpCustomCache = new string[HelpCnt + 1];
                    int fieldId = -1;
                    int HelpPtr = 0;
                    for (HelpPtr = 0; HelpPtr < HelpCnt; HelpPtr++) {
                        fieldId = GenericController.encodeInteger(fieldHelpArray[0, HelpPtr]);
                        int LastFieldID = 0;
                        if (fieldId != LastFieldID) {
                            LastFieldID = fieldId;
                            HelpIDCache[HelpPtr] = fieldId;
                            helpIdIndex.setPtr(fieldId.ToString(), HelpPtr);
                            helpDefaultCache[HelpPtr] = GenericController.encodeText(fieldHelpArray[1, HelpPtr]);
                            HelpCustomCache[HelpPtr] = GenericController.encodeText(fieldHelpArray[2, HelpPtr]);
                        }
                    }
                    AllowHelpMsgCustom = true;
                }
                //
                string FormFieldList = ",";
                List<string> TabsFound = new List<string>();
                foreach (KeyValuePair<string, ContentFieldMetadataModel> keyValuePair in adminData.adminContent.fields) {
                    ContentFieldMetadataModel field = keyValuePair.Value;
                    if ((field.authorable) && (field.active) && (!TabsFound.Contains(field.editTabName.ToLowerInvariant()))) {
                        TabsFound.Add(field.editTabName.ToLowerInvariant());
                        string editTabCaption = field.editTabName;
                        if (string.IsNullOrEmpty(editTabCaption)) { editTabCaption = "Details"; }
                        string NewFormFieldList = "";
                        string tabContent = getTab(core, adminData, adminData.editRecord.id, adminData.adminContent.id, readOnlyField, IsLandingPage, IsRootPage, field.editTabName, EditorContext, ref NewFormFieldList, TemplateIDForStyles, HelpCnt, HelpIDCache, helpDefaultCache, HelpCustomCache, AllowHelpMsgCustom, helpIdIndex, fieldTypeDefaultEditors, fieldEditorPreferenceList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON);
                        if (!string.IsNullOrEmpty(tabContent)) {
                            returnHtml += addTab(core, adminMenu, editTabCaption, tabContent, adminData.allowAdminTabs);
                        }
                        if (!string.IsNullOrEmpty(NewFormFieldList)) { FormFieldList = NewFormFieldList + FormFieldList; }
                    }
                }
                returnHtml += HtmlController.inputHidden("FormFieldList", FormFieldList);
            } catch (Exception ex) {
                LogController.logError(core, ex);
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
        public static string addTab(CoreController core, TabController adminMenu, string Caption, string Content, bool AllowAdminTabs) {
            try {
                if (string.IsNullOrEmpty(Content)) { return string.Empty; }
                if (!AllowAdminTabs) { return Content; }
                adminMenu.addEntry(Caption.Replace(" ", "&nbsp;"), "", "", Content, false, "ccAdminTab");
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return string.Empty;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        public static string getForm_EditFormStart(CoreController core, AdminDataModel adminData, int AdminFormID) {
            string result = "";
            try {
                core.html.addScriptCode("var docLoaded=false", "Form loader");
                core.html.addScriptCode_onLoad("docLoaded=true;", "Form loader");
                result = HtmlController.formMultipart_start(core, core.doc.refreshQueryString, "", "ccForm", "adminEditForm");
                result = GenericController.vbReplace(result, ">", " onSubmit=\"cj.admin.saveEmptyFieldList('FormEmptyFieldList');\" autocomplete=\"off\">");
                result += "\r\n<!-- block --><div class=\"d-none\"><input type=password name=\"password_block\" value=\"\"><input type=text name=\"username_block\" value=\"\"></div><!-- end block -->";
                result += "\r\n<input TYPE=\"hidden\" NAME=\"" + rnAdminSourceForm + "\" VALUE=\"" + AdminFormID.ToString() + "\">";
                result += "\r\n<input TYPE=\"hidden\" NAME=\"" + RequestNameTitleExtension + "\" VALUE=\"" + adminData.TitleExtension + "\">";
                result += "\r\n<input TYPE=\"hidden\" NAME=\"" + RequestNameAdminDepth + "\" VALUE=\"" + adminData.ignore_legacyMenuDepth + "\">";
                result += "\r\n<input TYPE=\"hidden\" NAME=\"FormEmptyFieldList\" ID=\"FormEmptyFieldList\" VALUE=\",\">";
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }
        //====================================================================================================

    }
}
