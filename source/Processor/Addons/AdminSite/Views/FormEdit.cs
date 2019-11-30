
using System;
using System.Collections.Generic;
using System.Data;

using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
using static Contensive.Processor.Addons.AdminSite.Controllers.AdminUIController;
using Contensive.Processor.Exceptions;
using Contensive.Processor.Addons.AdminSite.Controllers;
using Contensive.BaseClasses;
using Contensive.Processor;
using Contensive.Models.Db;

namespace Contensive.Processor.Addons.AdminSite {
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
                        if ((keyValuePair.Value.fieldTypeId == CPContentBaseClass.FieldTypeIdEnum.File) || (keyValuePair.Value.fieldTypeId == CPContentBaseClass.FieldTypeIdEnum.FileImage)) {
                            adminData.editRecord.fieldsLc[field.nameLc].value = adminData.editRecord.fieldsLc[field.nameLc].dbValue;
                        }
                    }
                } else {
                    //
                    // otherwise, load the record, even if it was loaded during a previous form process
                    adminData.loadEditRecord(core, true);
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
                        EditReferer = GenericController.strReplace(EditReferer, "&af=39", "");
                        //
                        // if referer includes AdminWarningMsg (admin hint message), remove it -- this edit may fix the problem
                        int Pos = EditReferer.IndexOf("AdminWarningMsg=");
                        if (Pos >= 0) {
                            EditReferer = EditReferer.left(Pos - 2);
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
                bool IsTemplateTable = (adminData.adminContent.tableName.ToLowerInvariant() == PageTemplateModel.tableMetadata.tableNameLower);
                bool IsPageContentTable = (adminData.adminContent.tableName.ToLowerInvariant() == PageContentModel.tableMetadata.tableNameLower);
                bool IsEmailTable = (adminData.adminContent.tableName.ToLowerInvariant() == EmailModel.tableMetadata.tableNameLower);
                int emailIdForStyles = IsEmailTable ? adminData.editRecord.id : 0;
                bool IsLandingPage = false;
                bool IsRootPage = false;
                if (IsPageContentTable && (adminData.editRecord.id != 0)) {
                    //
                    // landing page case
                    if (core.siteProperties.landingPageID != 0) {
                        IsLandingPage = (adminData.editRecord.id == core.siteProperties.landingPageID);
                        IsRootPage = IsPageContentTable && (adminData.editRecord.parentId == 0);
                    }
                }
                if (IsTemplateTable) {
                    TemplateIDForStyles = adminData.editRecord.id;
                } else if (IsPageContentTable) {
                }
                var headerInfo = new RecordEditHeaderInfoClass() {
                    recordId = adminData.editRecord.id,
                    recordLockById = adminData.editRecord.EditLock.editLockByMemberId,
                    recordLockExpiresDate = encodeDate(adminData.editRecord.EditLock.editLockExpiresDate),
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
                //
                // load user's editor preferences to fieldEditorPreferences() - this is the editor this user has picked when there are >1
                //   fieldId:addonId,fieldId:addonId,etc
                //   with custom FancyBox form in edit window with button "set editor preference"
                //   this button causes a 'refresh' action, reloads fields with stream without save
                //
                //
                // ----- determine contentType for editor
                //
                CPHtml5BaseClass.EditorContentType ContentType;
                if (GenericController.toLCase(adminData.adminContent.name) == "email templates") {
                    ContentType = CPHtml5BaseClass.EditorContentType.contentTypeEmailTemplate;
                } else if (GenericController.toLCase(adminData.adminContent.tableName) == "cctemplates") {
                    ContentType = CPHtml5BaseClass.EditorContentType.contentTypeWebTemplate;
                } else if (GenericController.toLCase(adminData.adminContent.tableName) == "ccemail") {
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
                if (adminContentTableNameLower == PersonModel.tableMetadata.tableNameLower) {
                    //
                    // -- people
                    if (!(core.session.isAuthenticatedAdmin())) {
                        //
                        // Must be admin
                        Stream.Add(AdminErrorController.get(core, "This edit form requires administrator access.", ""));
                    } else {
                        string EditSectionButtonBar = AdminUIController.getSectionButtonBarForEdit(core, new EditButtonBarInfoClass() {
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
                            isPageContent = false,
                            contentId = adminData.adminContent.id
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getSectionHeader(core, "", titleBarDetails));
                        Stream.Add(FormEditTabs.getTabs(core, adminData, adminMenu, adminData.editRecord.userReadOnly, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(FormEditTabs.addTab(core, adminMenu, "Groups", GroupRuleEditor.get(core, adminData), adminData.allowAdminTabs));
                        Stream.Add(FormEditTabs.addTab(core, adminMenu, "Control&nbsp;Info", FormEditTabControlInfo.get(core, adminData), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) Stream.Add(adminMenu.getTabs(core));
                        Stream.Add(EditSectionButtonBar);
                    }
                } else if (adminContentTableNameLower == EmailModel.tableMetadata.tableNameLower) {
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
                    if (!(core.session.isAuthenticatedAdmin())) {
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
                        string EditSectionButtonBar = AdminUIController.getSectionButtonBarForEdit(core, new EditButtonBarInfoClass() {
                            allowActivate = false,
                            allowAdd = (allowAdd && adminData.adminContent.allowAdd & adminData.editRecord.AllowUserAdd),
                            allowCancel = true,
                            allowCreateDuplicate = (allowSave && adminData.editRecord.AllowUserSave & (adminData.editRecord.id != 0)),
                            allowDelete = AllowDelete && adminData.editRecord.AllowUserDelete && core.session.isAuthenticatedDeveloper(),
                            allowMarkReviewed = false,
                            allowRefresh = AllowRefresh,
                            allowSave = (allowSave && adminData.editRecord.AllowUserSave && (!EmailSubmitted) && (!EmailSent)),
                            allowSend = false,
                            allowSendTest = ((!EmailSubmitted) && (!EmailSent)),
                            hasChildRecords = false,
                            isPageContent = false,
                            contentId = adminData.adminContent.id
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getSectionHeader(core, "", titleBarDetails));
                        Stream.Add(FormEditTabs.getTabs(core, adminData, adminMenu, adminData.editRecord.userReadOnly, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(FormEditTabs.addTab(core, adminMenu, "Control&nbsp;Info", FormEditTabControlInfo.get(core, adminData), adminData.allowAdminTabs));
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
                        string EditSectionButtonBar = AdminUIController.getSectionButtonBarForEdit(core, new EditButtonBarInfoClass() {
                            allowActivate = !EmailSubmitted & ((LastSendTestDate != DateTime.MinValue) || AllowEmailSendWithoutTest),
                            allowDeactivate = EmailSubmitted,
                            allowAdd = allowAdd && adminData.adminContent.allowAdd & adminData.editRecord.AllowUserAdd,
                            allowCancel = true,
                            allowCreateDuplicate = allowAdd && (adminData.editRecord.id != 0),
                            allowDelete = AllowDelete && adminData.editRecord.AllowUserDelete && core.session.isAuthenticatedDeveloper(),
                            allowMarkReviewed = false,
                            allowRefresh = AllowRefresh,
                            allowSave = allowSave && adminData.editRecord.AllowUserSave && !EmailSubmitted,
                            allowSend = false,
                            allowSendTest = !EmailSubmitted,
                            hasChildRecords = false,
                            isPageContent = false,
                            contentId = adminData.adminContent.id
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getSectionHeader(core, "", titleBarDetails));
                        Stream.Add(FormEditTabs.getTabs(core, adminData, adminMenu, adminData.editRecord.userReadOnly || EmailSubmitted, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(FormEditTabs.addTab(core, adminMenu, "Control&nbsp;Info", FormEditTabControlInfo.get(core, adminData), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) Stream.Add(adminMenu.getTabs(core));
                        Stream.Add(EditSectionButtonBar);
                    } else {
                        //
                        // Group Email
                        if (adminData.editRecord.id != 0) {
                            EmailSubmitted = encodeBoolean(adminData.editRecord.fieldsLc["submitted"].value);
                            EmailSent = encodeBoolean(adminData.editRecord.fieldsLc["sent"].value);
                        }
                        string EditSectionButtonBar = AdminUIController.getSectionButtonBarForEdit(core, new EditButtonBarInfoClass() {
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
                            isPageContent = false,
                            contentId = adminData.adminContent.id
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getSectionHeader(core, "", titleBarDetails));
                        Stream.Add(FormEditTabs.getTabs(core, adminData, adminMenu, adminData.editRecord.userReadOnly || EmailSubmitted || EmailSent, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(FormEditTabs.addTab(core, adminMenu, "Control&nbsp;Info", FormEditTabControlInfo.get(core, adminData), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) Stream.Add(adminMenu.getTabs(core));
                        Stream.Add(EditSectionButtonBar);
                    }
                } else if (adminData.adminContent.tableName.ToLowerInvariant() == ContentModel.tableMetadata.tableNameLower) {
                    if (!(core.session.isAuthenticatedAdmin())) {
                        //
                        // Must be admin
                        //
                        Stream.Add(AdminErrorController.get(core, "This edit form requires Member Administration access.", "This edit form requires Member Administration access."));
                    } else {
                        string EditSectionButtonBar = AdminUIController.getSectionButtonBarForEdit(core, new EditButtonBarInfoClass() {
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
                            isPageContent = false,
                            contentId = adminData.adminContent.id
                        });
                        Stream.Add(EditSectionButtonBar);
                        Stream.Add(AdminUIController.getSectionHeader(core, "", titleBarDetails));
                        Stream.Add(FormEditTabs.getTabs(core, adminData, adminMenu, adminData.editRecord.userReadOnly, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                        Stream.Add(FormEditTabs.addTab(core, adminMenu, "Control&nbsp;Info", FormEditTabControlInfo.get(core, adminData), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) {
                            Stream.Add(adminMenu.getTabs(core));
                        }
                        Stream.Add(EditSectionButtonBar);
                    }
                    //
                } else if (adminContentTableNameLower == PageContentModel.tableMetadata.tableNameLower) {
                    //
                    // Page Content
                    //
                    int TableId = MetadataController.getRecordIdByUniqueName(core, "Tables", "ccPageContent");
                    string EditSectionButtonBar = AdminUIController.getSectionButtonBarForEdit(core, new EditButtonBarInfoClass() {
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
                        isPageContent = false,
                        contentId = adminData.adminContent.id
                    });
                    Stream.Add(EditSectionButtonBar);
                    Stream.Add(AdminUIController.getSectionHeader(core, "", titleBarDetails));
                    Stream.Add(FormEditTabs.getTabs(core, adminData, adminMenu, adminData.editRecord.userReadOnly, IsLandingPage || IsLandingPageParent, IsRootPage, ContentType, AllowajaxTabs, TemplateIDForStyles, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    Stream.Add(FormEditTabs.addTab(core, adminMenu, "Link Aliases", LinkAliasEditor.GetForm_Edit_LinkAliases(core, adminData, adminData.editRecord.userReadOnly), adminData.allowAdminTabs));
                    Stream.Add(FormEditTabs.addTab(core, adminMenu, "Content Watch", ContentTrackingEditor.get(core, adminData), adminData.allowAdminTabs));
                    Stream.Add(FormEditTabs.addTab(core, adminMenu, "Control Info", FormEditTabControlInfo.get(core, adminData), adminData.allowAdminTabs));
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
                    string EditSectionButtonBar = AdminUIController.getSectionButtonBarForEdit(core, new EditButtonBarInfoClass() {
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
                        isPageContent = pageContentMetadata.isParentOf(core, adminData.adminContent.id),
                        contentId = adminData.adminContent.id
                    });
                    Stream.Add(EditSectionButtonBar);
                    Stream.Add(AdminUIController.getSectionHeader(core, "", titleBarDetails));
                    Stream.Add(FormEditTabs.getTabs(core, adminData, adminMenu, adminData.editRecord.userReadOnly, false, false, ContentType, AllowajaxTabs, TemplateIDForStyles, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON));
                    Stream.Add(FormEditTabs.addTab(core, adminMenu, "Content Watch", ContentTrackingEditor.get(core, adminData), adminData.allowAdminTabs));
                    Stream.Add(FormEditTabs.addTab(core, adminMenu, "Control Info", FormEditTabControlInfo.get(core, adminData), adminData.allowAdminTabs));
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
        //========================================================================
        //
        public static string getForm_EditFormStart(CoreController core, AdminDataModel adminData, int AdminFormID) {
            string result = "";
            try {
                core.html.addScriptCode("var docLoaded=false", "Form loader");
                core.html.addScriptCode_onLoad("docLoaded=true;", "Form loader");
                result = HtmlController.formMultipart_start(core, core.doc.refreshQueryString, "", "ccForm", "adminEditForm");
                result = GenericController.strReplace(result, ">", " onSubmit=\"cj.admin.saveEmptyFieldList('FormEmptyFieldList');\" autocomplete=\"off\">");
                result += Environment.NewLine + "<!-- block --><div class=\"d-none\"><input type=password name=\"password_block\" value=\"\"><input type=text name=\"username_block\" value=\"\"></div><!-- end block -->";
                result += Environment.NewLine + "<input TYPE=\"hidden\" NAME=\"" + rnAdminSourceForm + "\" VALUE=\"" + AdminFormID + "\">";
                result += Environment.NewLine + "<input TYPE=\"hidden\" NAME=\"" + RequestNameTitleExtension + "\" VALUE=\"" + adminData.titleExtension + "\">";
                result += Environment.NewLine + "<input TYPE=\"hidden\" NAME=\"" + RequestNameAdminDepth + "\" VALUE=\"" + adminData.ignore_legacyMenuDepth + "\">";
                result += Environment.NewLine + "<input TYPE=\"hidden\" NAME=\"FormEmptyFieldList\" ID=\"FormEmptyFieldList\" VALUE=\",\">";
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //


    }
}
