//
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;
using static Contensive.Processor.Addons.AdminSite.Controllers.AdminUIController;
using Contensive.Processor.Addons.AdminSite.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using Contensive.BaseClasses;
using static Contensive.Processor.Constants;
using Contensive.Models.Db;
using System;
using System.Globalization;

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
                bool allowajaxTabs = (core.siteProperties.getBoolean("AllowAjaxEditTabBeta", false));
                var adminMenu = new TabController();
                //
                if ((!core.doc.userErrorList.Count.Equals(0)) && adminData.editRecord.Loaded) {
                    //
                    // block load if there was a user error and it is already loaded (assume error was from response )
                } else if (adminData.adminContent.id <= 0) {
                    //
                    // Invalid Content
                    Processor.Controllers.ErrorController.addUserError(core, "There was a problem identifying the content you requested. Please return to the previous form and verify your selection.");
                    return "";
                } else if (adminData.editRecord.Loaded && !adminData.editRecord.Saved) {
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
                        int Pos = EditReferer.IndexOf("AdminWarningMsg=", StringComparison.CurrentCulture);
                        if (Pos >= 0) {
                            EditReferer = EditReferer.left(Pos - 2);
                        }
                    }
                }
                core.doc.addRefreshQueryString(RequestNameEditReferer, EditReferer);
                //
                // Print common form elements
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                Stream.add(getForm_EditFormStart(core, adminData, AdminFormEdit));
                bool IsLandingPageParent = false;
                int templateIDForStyles = 0;
                bool isTemplateTable = (adminData.adminContent.tableName.ToLowerInvariant() == PageTemplateModel.tableMetadata.tableNameLower);
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
                if (isTemplateTable) {
                    templateIDForStyles = adminData.editRecord.id;
                } else if (IsPageContentTable) {
                    // do nothing
                }
                var headerInfo = new RecordEditHeaderInfoClass {
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
                bool allowDelete = adminData.adminContent.allowDelete && userContentPermissions.allowDelete && (adminData.editRecord.id != 0);
                bool allowSave = userContentPermissions.allowSave;
                bool allowRefresh = true;
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
                Stream.add("\r<input type=\"hidden\" name=\"fieldEditorPreference\" id=\"fieldEditorPreference\" value=\"\">");
                //
                // load user's editor preferences to fieldEditorPreferences() - this is the editor this user has picked when there are >1
                //   fieldId:addonId,fieldId:addonId,etc
                //   with custom FancyBox form in edit window with button "set editor preference"
                //   this button causes a 'refresh' action, reloads fields with stream without save
                //
                //
                // ----- determine contentType for editor
                //
                CPHtml5BaseClass.EditorContentType contentType;
                if (GenericController.toLCase(adminData.adminContent.name) == "email templates") {
                    contentType = CPHtml5BaseClass.EditorContentType.contentTypeEmailTemplate;
                } else if (GenericController.toLCase(adminData.adminContent.tableName) == "cctemplates") {
                    contentType = CPHtml5BaseClass.EditorContentType.contentTypeWebTemplate;
                } else if (GenericController.toLCase(adminData.adminContent.tableName) == "ccemail") {
                    contentType = CPHtml5BaseClass.EditorContentType.contentTypeEmail;
                } else {
                    contentType = CPHtml5BaseClass.EditorContentType.contentTypeWeb;
                }
                //
                //-----Create edit page
                string styleList = "";
                string adminContentTableNameLower = adminData.adminContent.tableName.ToLowerInvariant();
                EditorEnvironmentModel editorEnv = new EditorEnvironmentModel {
                    AllowHelpMsgCustom = false,
                    editorAddonListJSON = core.html.getWysiwygAddonList(contentType),
                    IsLandingPage = IsLandingPage,
                    IsRootPage = IsRootPage,
                    needUniqueEmailMessage = false,
                    record_readOnly = adminData.editRecord.userReadOnly,
                    styleList = styleList,
                    styleOptionList = "",
                    formFieldList = ""
                };
                //
                LogController.logTrace(core, "getFormEdit, adminInfo.editRecord.contentControlId [" + adminData.editRecord.contentControlId + "]");
                //
                if (adminContentTableNameLower == PersonModel.tableMetadata.tableNameLower) {
                    //
                    // -- people
                    if (!(core.session.isAuthenticatedAdmin())) {
                        //
                        // Must be admin
                        Stream.add(AdminErrorController.get(core, "This edit form requires administrator access.", ""));
                    } else {
                        var EditButtonBarInfo = new EditButtonBarInfoClass(core, adminData, allowDelete, allowRefresh, allowSave, allowAdd);
                        string EditSectionButtonBar = AdminUIController.getSectionButtonBarForEdit(core, EditButtonBarInfo);
                        Stream.add(EditSectionButtonBar);
                        Stream.add(AdminUIController.getSectionHeader(core, "", titleBarDetails));
                        Stream.add(FormEditTabs.getTabs(core, adminData, adminMenu, editorEnv, contentType, allowajaxTabs, templateIDForStyles, styleList, emailIdForStyles, isTemplateTable));
                        Stream.add(FormEditTabs.addTab(core, adminMenu, "Groups", GroupRuleEditor.get(core, adminData), adminData.allowAdminTabs));
                        Stream.add(FormEditTabs.addTab(core, adminMenu, "Control&nbsp;Info", FormEditTabControlInfo.get(core, adminData, editorEnv), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) { Stream.add(adminMenu.getTabs(core)); }
                        Stream.add(EditSectionButtonBar);
                    }
                } else if (adminContentTableNameLower == EmailModel.tableMetadata.tableNameLower) {
                    //
                    LogController.logTrace(core, "getFormEdit, treat as email, adminContentTableNameLower [" + adminContentTableNameLower + "]");
                    //
                    // -- email
                    bool emailSubmitted = false;
                    bool emailSent = false;
                    DateTime LastSendTestDate = DateTime.MinValue;
                    bool AllowEmailSendWithoutTest = (core.siteProperties.getBoolean("AllowEmailSendWithoutTest", false));
                    if (adminData.editRecord.fieldsLc.ContainsKey("lastsendtestdate")) {
                        LastSendTestDate = GenericController.encodeDate(adminData.editRecord.fieldsLc["lastsendtestdate"].value);
                    }
                    if (!(core.session.isAuthenticatedAdmin())) {
                        //
                        // Must be admin
                        Stream.add(AdminErrorController.get(core, "This edit form requires Member Administration access.", "This edit form requires Member Administration access."));
                    } else if (adminData.adminContent.id == ContentMetadataModel.getContentId(core, "System Email")) {
                        //
                        LogController.logTrace(core, "getFormEdit, System email");
                        //
                        // System Email
                        emailSubmitted = false;
                        if (adminData.editRecord.id != 0) {
                            if (adminData.editRecord.fieldsLc.ContainsKey("testmemberid")) {
                                if (encodeInteger(adminData.editRecord.fieldsLc["testmemberid"].value) == 0) {
                                    adminData.editRecord.fieldsLc["testmemberid"].value = core.session.user.id;
                                }
                            }
                        }
                        var editButtonBarInfo = new EditButtonBarInfoClass(core, adminData, allowDelete, allowRefresh, allowSave, allowAdd) {
                            allowSave = (allowSave && adminData.editRecord.AllowUserSave && (!emailSubmitted) && (!emailSent)),
                            allowSendTest = ((!emailSubmitted) && (!emailSent))
                        };
                        var EditSectionButtonBar = AdminUIController.getSectionButtonBarForEdit(core, editButtonBarInfo);
                        Stream.add(EditSectionButtonBar);
                        Stream.add(AdminUIController.getSectionHeader(core, "", titleBarDetails));
                        Stream.add(FormEditTabs.getTabs(core, adminData, adminMenu, editorEnv, contentType, allowajaxTabs, templateIDForStyles, styleList, emailIdForStyles, isTemplateTable));
                        Stream.add(FormEditTabs.addTab(core, adminMenu, "Control&nbsp;Info", FormEditTabControlInfo.get(core, adminData, editorEnv), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) { Stream.add(adminMenu.getTabs(core)); }
                        Stream.add(EditSectionButtonBar);
                    } else if (adminData.adminContent.id == ContentMetadataModel.getContentId(core, "Conditional Email")) {
                        //
                        // Conditional Email
                        emailSubmitted = false;
                        editorEnv.record_readOnly = adminData.editRecord.userReadOnly || emailSubmitted;
                        if (adminData.editRecord.id != 0) {
                            if (adminData.editRecord.fieldsLc.ContainsKey("submitted")) { emailSubmitted = GenericController.encodeBoolean(adminData.editRecord.fieldsLc["submitted"].value); }
                        }
                        var editButtonBarInfo = new EditButtonBarInfoClass(core, adminData, allowDelete, allowRefresh, allowSave, allowAdd) {
                            allowActivate = !emailSubmitted && ((LastSendTestDate != DateTime.MinValue) || AllowEmailSendWithoutTest),
                            allowDeactivate = emailSubmitted,
                            allowSave = allowSave && adminData.editRecord.AllowUserSave && !emailSubmitted
                        };
                        var EditSectionButtonBar = AdminUIController.getSectionButtonBarForEdit(core, editButtonBarInfo);
                        Stream.add(EditSectionButtonBar);
                        Stream.add(AdminUIController.getSectionHeader(core, "", titleBarDetails));
                        Stream.add(FormEditTabs.getTabs(core, adminData, adminMenu, editorEnv, contentType, allowajaxTabs, templateIDForStyles, styleList, emailIdForStyles, isTemplateTable));
                        Stream.add(FormEditTabs.addTab(core, adminMenu, "Control&nbsp;Info", FormEditTabControlInfo.get(core, adminData, editorEnv), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) { Stream.add(adminMenu.getTabs(core)); }
                        Stream.add(EditSectionButtonBar);
                    } else {
                        //
                        // Group Email
                        if (adminData.editRecord.id != 0) {
                            emailSubmitted = encodeBoolean(adminData.editRecord.fieldsLc["submitted"].value);
                            emailSent = encodeBoolean(adminData.editRecord.fieldsLc["sent"].value);
                        }
                        var editButtonBarInfo = new EditButtonBarInfoClass(core, adminData, allowDelete, allowRefresh, allowSave, allowAdd) {
                            allowSave = !emailSubmitted && (allowSave && adminData.editRecord.AllowUserSave),
                            allowSend = !emailSubmitted && ((LastSendTestDate != DateTime.MinValue) || AllowEmailSendWithoutTest),
                            allowSendTest = !emailSubmitted
                        };
                        editorEnv.record_readOnly = adminData.editRecord.userReadOnly || emailSubmitted || emailSent;
                        var EditSectionButtonBar = AdminUIController.getSectionButtonBarForEdit(core, editButtonBarInfo);
                        Stream.add(EditSectionButtonBar);
                        Stream.add(AdminUIController.getSectionHeader(core, "", titleBarDetails));
                        Stream.add(FormEditTabs.getTabs(core, adminData, adminMenu, editorEnv, contentType, allowajaxTabs, templateIDForStyles, styleList, emailIdForStyles, isTemplateTable));
                        Stream.add(FormEditTabs.addTab(core, adminMenu, "Control&nbsp;Info", FormEditTabControlInfo.get(core, adminData, editorEnv), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) { Stream.add(adminMenu.getTabs(core)); }
                        Stream.add(EditSectionButtonBar);
                    }
                } else if (adminData.adminContent.tableName.ToLowerInvariant() == ContentModel.tableMetadata.tableNameLower) {
                    if (!(core.session.isAuthenticatedAdmin())) {
                        //
                        // Must be admin
                        //
                        Stream.add(AdminErrorController.get(core, "This edit form requires Member Administration access.", "This edit form requires Member Administration access."));
                    } else {
                        var editButtonBarInfo = new EditButtonBarInfoClass(core, adminData, allowDelete, allowRefresh, allowSave, allowAdd);
                        var EditSectionButtonBar = AdminUIController.getSectionButtonBarForEdit(core, editButtonBarInfo);
                        Stream.add(EditSectionButtonBar);
                        Stream.add(AdminUIController.getSectionHeader(core, "", titleBarDetails));
                        Stream.add(FormEditTabs.getTabs(core, adminData, adminMenu, editorEnv, contentType, allowajaxTabs, templateIDForStyles, styleList, emailIdForStyles, isTemplateTable));
                        Stream.add(FormEditTabs.addTab(core, adminMenu, "Control&nbsp;Info", FormEditTabControlInfo.get(core, adminData, editorEnv), adminData.allowAdminTabs));
                        if (adminData.allowAdminTabs) {
                            Stream.add(adminMenu.getTabs(core));
                        }
                        Stream.add(EditSectionButtonBar);
                    }
                    //
                } else if (adminContentTableNameLower == PageContentModel.tableMetadata.tableNameLower) {
                    //
                    // Page Content
                    //
                    editorEnv.IsLandingPage = IsLandingPage || IsLandingPageParent;
                    var editButtonBarInfo = new EditButtonBarInfoClass(core, adminData, allowDelete, allowRefresh, allowSave, allowAdd) {
                        allowMarkReviewed = true,
                        isPageContent = true,
                        hasChildRecords = true
                    };
                    var EditSectionButtonBar = AdminUIController.getSectionButtonBarForEdit(core, editButtonBarInfo);
                    Stream.add(EditSectionButtonBar);
                    Stream.add(AdminUIController.getSectionHeader(core, "", titleBarDetails));
                    Stream.add(FormEditTabs.getTabs(core, adminData, adminMenu, editorEnv, contentType, allowajaxTabs, templateIDForStyles, styleList, emailIdForStyles, isTemplateTable));
                    Stream.add(FormEditTabs.addTab(core, adminMenu, "Link Aliases", LinkAliasEditor.GetForm_Edit_LinkAliases(core, adminData, adminData.editRecord.userReadOnly), adminData.allowAdminTabs));
                    Stream.add(FormEditTabs.addTab(core, adminMenu, "Content Watch", ContentTrackingEditor.get(core, adminData, editorEnv), adminData.allowAdminTabs));
                    Stream.add(FormEditTabs.addTab(core, adminMenu, "Control Info", FormEditTabControlInfo.get(core, adminData, editorEnv), adminData.allowAdminTabs));
                    if (adminData.allowAdminTabs) {
                        Stream.add(adminMenu.getTabs(core));
                    }
                    Stream.add(EditSectionButtonBar);
                } else {
                    //
                    // All other tables (User definined)
                    var pageContentMetadata = ContentMetadataModel.createByUniqueName(core, "page content");
                    var editButtonBarInfo = new EditButtonBarInfoClass(core, adminData, allowDelete, allowRefresh, allowSave, allowAdd) {
                        isPageContent = pageContentMetadata.isParentOf(core, adminData.adminContent.id),
                        hasChildRecords = adminData.adminContent.containsField(core, "parentid"),
                        allowMarkReviewed = core.db.isSQLTableField(adminData.adminContent.tableName, "DateReviewed")
                    };
                    var editSectionButtonBar = AdminUIController.getSectionButtonBarForEdit(core, editButtonBarInfo);
                    Stream.add(editSectionButtonBar);
                    Stream.add(AdminUIController.getSectionHeader(core, "", titleBarDetails));
                    Stream.add(FormEditTabs.getTabs(core, adminData, adminMenu, editorEnv, contentType, allowajaxTabs, templateIDForStyles, styleList, emailIdForStyles, isTemplateTable));
                    Stream.add(FormEditTabs.addTab(core, adminMenu, "Content Watch", ContentTrackingEditor.get(core, adminData, editorEnv), adminData.allowAdminTabs));
                    Stream.add(FormEditTabs.addTab(core, adminMenu, "Control Info", FormEditTabControlInfo.get(core, adminData, editorEnv), adminData.allowAdminTabs));
                    if (adminData.allowAdminTabs) { Stream.add(adminMenu.getTabs(core)); }
                    Stream.add(editSectionButtonBar);
                }
                Stream.add(HtmlController.inputHidden("FormFieldList", editorEnv.formFieldList));
                Stream.add("</form>");
                returnHtml = Stream.text;
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
