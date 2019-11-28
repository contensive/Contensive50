
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
    public static class FormEditTabs {
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
        /// <param name="fieldEditorList"></param>
        /// <param name="styleList"></param>
        /// <param name="styleOptionList"></param>
        /// <param name="emailIdForStyles"></param>
        /// <param name="IsTemplateTable"></param>
        /// <param name="editorAddonListJSON"></param>
        /// <returns></returns>
        public static string getTab(CoreController core, AdminDataModel adminData, int RecordID, int ContentID, bool record_readOnly, bool IsLandingPage, bool IsRootPage, string EditTab, CPHtml5BaseClass.EditorContentType EditorContext, ref string return_NewFieldList, int TemplateIDForStyles, int HelpCnt, int[] HelpIDCache, string[] helpDefaultCache, string[] HelpCustomCache, bool AllowHelpMsgCustom, KeyPtrController helpIdIndex, string styleList, string styleOptionList, int emailIdForStyles, bool IsTemplateTable, string editorAddonListJSON) {
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
                            if (AdminDataModel.isVisibleUserField(core, field.adminOnly, field.developerOnly, field.active, field.authorable, field.nameLc, adminData.adminContent.tableName)) {
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
                        CPContentBaseClass.FieldTypeIdEnum fieldTypeId = field.fieldTypeId;
                        Contensive.Processor.Addons.AdminSite.Models.EditRecordModel editRecord = adminData.editRecord;
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
                        adminData.formInputCount = adminData.formInputCount + 1;
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
                        AddonModel editorAddon = null;
                        int fieldTypeDefaultEditorAddonId = 0;
                        var fieldEditor = adminData.fieldTypeEditors.Find(x => (x.fieldTypeId == (int)field.fieldTypeId));
                        if (fieldEditor != null) {
                            fieldTypeDefaultEditorAddonId = (int)fieldEditor.editorAddonId;
                            editorAddon = DbBaseModel.create<AddonModel>(core.cpParent, fieldTypeDefaultEditorAddonId);
                        }
                        bool useEditorAddon = false;
                        if (editorAddon != null) {
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
                            if (GenericController.encodeBoolean((fieldTypeId == CPContentBaseClass.FieldTypeIdEnum.HTML) || (fieldTypeId == CPContentBaseClass.FieldTypeIdEnum.FileHTML))) {
                                //
                                // include html related arguments
                                core.docProperties.setProperty("editorAllowActiveContent", "1");
                                core.docProperties.setProperty("editorAddonList", editorAddonListJSON);
                                core.docProperties.setProperty("editorStyles", styleList);
                                core.docProperties.setProperty("editorStyleOptions", styleOptionList);
                            }
                            EditorString = core.addon.execute(editorAddon, new BaseClasses.CPUtilsBaseClass.addonExecuteContext {
                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextEditor,
                                errorContextMessage = "field editor id:" + editorAddon.id
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
                                    if (!csData.openSql("select id from ccaggregatefunctions where id=" + editorAddon.id)) {
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
                            if (fieldTypeId == CPContentBaseClass.FieldTypeIdEnum.Redirect) {
                                //
                                // ----- Default Editor, Redirect fields (the same for normal/readonly/spelling)
                                string RedirectPath = core.appConfig.adminRoute;
                                if (field.redirectPath != "") {
                                    RedirectPath = field.redirectPath;
                                }
                                RedirectPath = RedirectPath + "?" + RequestNameTitleExtension + "=" + GenericController.encodeRequestVariable(" For " + editRecord.nameLc + adminData.titleExtension) + "&" + RequestNameAdminDepth + "=" + (adminData.ignore_legacyMenuDepth + 1) + "&wl0=" + field.redirectId + "&wr0=" + editRecord.id;
                                if (field.redirectContentId != 0) {
                                    RedirectPath = RedirectPath + "&cid=" + field.redirectContentId;
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
                                    case CPContentBaseClass.FieldTypeIdEnum.Text:
                                    case CPContentBaseClass.FieldTypeIdEnum.Link:
                                    case CPContentBaseClass.FieldTypeIdEnum.ResourceLink:
                                    //
                                    // ----- Text Type
                                    EditorString += AdminUIController.getDefaultEditor_text(core, field.nameLc, fieldValue_text, editorReadOnly, fieldHtmlId);
                                    return_NewFieldList += "," + field.nameLc;
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.Boolean:
                                    //
                                    // ----- Boolean ReadOnly
                                    EditorString += AdminUIController.getDefaultEditor_bool(core, field.nameLc, GenericController.encodeBoolean(fieldValueObject), editorReadOnly, fieldHtmlId);
                                    return_NewFieldList += "," + field.nameLc;
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.Lookup:
                                    //
                                    // ----- Lookup, readonly
                                    if (field.lookupContentId != 0) {
                                        EditorString = AdminUIController.getDefaultEditor_lookupContent(core, field.nameLc, encodeInteger(fieldValueObject), field.lookupContentId, ref IsEmptyList, editorReadOnly, fieldHtmlId, WhyReadOnlyMsg, field.required);
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
                                    case CPContentBaseClass.FieldTypeIdEnum.Date:
                                    //
                                    // ----- date, readonly
                                    return_NewFieldList += "," + field.nameLc;
                                    EditorString = AdminUIController.getDefaultEditor_dateTime(core, field.nameLc, encodeDate(fieldValueObject), editorReadOnly, fieldHtmlId, field.required, WhyReadOnlyMsg);
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.MemberSelect:
                                    //
                                    // ----- Member Select ReadOnly
                                    return_NewFieldList += "," + field.nameLc;
                                    EditorString = AdminUIController.getDefaultEditor_memberSelect(core, field.nameLc, encodeInteger(fieldValueObject), field.memberSelectGroupId_get(core), field.memberSelectGroupName_get(core), editorReadOnly, fieldHtmlId, field.required, WhyReadOnlyMsg);
                                    //
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.ManyToMany:
                                    //
                                    //   Placeholder
                                    EditorString = AdminUIController.getDefaultEditor_manyToMany(core, field, "field" + field.id, fieldValue_text, editRecord.id, editorReadOnly, WhyReadOnlyMsg);
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.Currency:
                                    //
                                    // ----- Currency ReadOnly
                                    return_NewFieldList += "," + field.nameLc;
                                    FieldValueNumber = GenericController.encodeNumber(fieldValueObject);
                                    EditorString += (HtmlController.inputHidden(field.nameLc, GenericController.encodeText(FieldValueNumber)));
                                    EditorString += (HtmlController.inputNumber(core, field.nameLc, FieldValueNumber, fieldHtmlId, "text form-control", editorReadOnly, false));
                                    EditorString += WhyReadOnlyMsg;
                                    //
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.AutoIdIncrement:
                                    case CPContentBaseClass.FieldTypeIdEnum.Float:
                                    case CPContentBaseClass.FieldTypeIdEnum.Integer:
                                    //
                                    // ----- number readonly
                                    //
                                    return_NewFieldList += "," + field.nameLc;
                                    FieldValueNumber = GenericController.encodeNumber(fieldValueObject);
                                    EditorString += (HtmlController.inputHidden(field.nameLc, fieldValue_text));
                                    EditorString += (HtmlController.inputNumber(core, field.nameLc, FieldValueNumber, fieldHtmlId, "text form-control", editorReadOnly, false));
                                    EditorString += WhyReadOnlyMsg;
                                    //
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.HTML:
                                    case CPContentBaseClass.FieldTypeIdEnum.FileHTML:
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
                                    case CPContentBaseClass.FieldTypeIdEnum.LongText:
                                    case CPContentBaseClass.FieldTypeIdEnum.FileText:
                                    //
                                    // ----- LongText, TextFile
                                    return_NewFieldList += "," + field.nameLc;
                                    EditorString += HtmlController.inputHidden(field.nameLc, fieldValue_text);
                                    FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                    EditorString += HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, fieldHtmlId, false, editorReadOnly, " form-control");
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.File:
                                    case CPContentBaseClass.FieldTypeIdEnum.FileImage:
                                    //
                                    // ----- File ReadOnly
                                    return_NewFieldList += "," + field.nameLc;
                                    NonEncodedLink = GenericController.getCdnFileLink(core, fieldValue_text);
                                    EncodedLink = System.Net.WebUtility.HtmlEncode(NonEncodedLink);
                                    EditorString += (HtmlController.inputHidden(field.nameLc, ""));
                                    if (string.IsNullOrEmpty(fieldValue_text)) {
                                        EditorString += ("[no file]");
                                    } else {
                                        string filename = "";
                                        string path = "";
                                        core.cdnFiles.splitDosPathFilename(fieldValue_text, ref path, ref filename);
                                        EditorString += ("&nbsp;<a href=\"" + EncodedLink + "\" target=\"_blank\">" + SpanClassAdminSmall + "[" + filename + "]</A>");
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
                                        EditorString += HtmlController.inputText_Legacy(core, field.nameLc, "*****", 0, 0, fieldHtmlId, true, true, "password form-control");
                                    } else if (!field.htmlContent) {
                                        //
                                        // not HTML capable, textarea with resizing
                                        if ((fieldTypeId == CPContentBaseClass.FieldTypeIdEnum.Text) && (fieldValue_text.IndexOf("\n") == -1) && (fieldValue_text.Length < 40)) {
                                            //
                                            // text field shorter then 40 characters without a CR
                                            EditorString += HtmlController.inputText_Legacy(core, field.nameLc, fieldValue_text, 1, 0, fieldHtmlId, false, true, "text form-control");
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
                                    case CPContentBaseClass.FieldTypeIdEnum.Text:
                                    //
                                    // ----- Text Type
                                    if (field.password) {
                                        EditorString += AdminUIController.getDefaultEditor_Password(core, field.nameLc, fieldValue_text, false, fieldHtmlId);
                                    } else {
                                        EditorString += AdminUIController.getDefaultEditor_text(core, field.nameLc, fieldValue_text, false, fieldHtmlId);
                                    }
                                    return_NewFieldList += "," + field.nameLc;
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.Boolean:
                                    //
                                    // ----- Boolean
                                    EditorString += AdminUIController.getDefaultEditor_bool(core, field.nameLc, GenericController.encodeBoolean(fieldValueObject), false, fieldHtmlId);
                                    return_NewFieldList += "," + field.nameLc;
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.Lookup:
                                    //
                                    // ----- Lookup
                                    if (field.lookupContentId != 0) {
                                        EditorString = AdminUIController.getDefaultEditor_lookupContent(core, field.nameLc, encodeInteger(fieldValueObject), field.lookupContentId, ref IsEmptyList, field.readOnly, fieldHtmlId, WhyReadOnlyMsg, field.required);
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
                                    case CPContentBaseClass.FieldTypeIdEnum.Date:
                                    //
                                    // ----- Date
                                    return_NewFieldList += "," + field.nameLc;
                                    EditorString = AdminUIController.getDefaultEditor_dateTime(core, field.nameLc, GenericController.encodeDate(fieldValueObject), field.readOnly, fieldHtmlId, field.required, WhyReadOnlyMsg);
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.MemberSelect:
                                    //
                                    // ----- Member Select
                                    return_NewFieldList += "," + field.nameLc;
                                    EditorString = AdminUIController.getDefaultEditor_memberSelect(core, field.nameLc, encodeInteger(fieldValueObject), field.memberSelectGroupId_get(core), field.memberSelectGroupName_get(core), field.readOnly, fieldHtmlId, field.required, WhyReadOnlyMsg);
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.ManyToMany:
                                    //
                                    //   Placeholder
                                    EditorString = AdminUIController.getDefaultEditor_manyToMany(core, field, "field" + field.id, fieldValue_text, editRecord.id, false, WhyReadOnlyMsg);
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.File:
                                    case CPContentBaseClass.FieldTypeIdEnum.FileImage:
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
                                    case CPContentBaseClass.FieldTypeIdEnum.AutoIdIncrement:
                                    case CPContentBaseClass.FieldTypeIdEnum.Currency:
                                    case CPContentBaseClass.FieldTypeIdEnum.Float:
                                    case CPContentBaseClass.FieldTypeIdEnum.Integer:
                                    //
                                    // ----- Others that simply print
                                    return_NewFieldList += "," + field.nameLc;
                                    if (field.password) {
                                        EditorString += (HtmlController.inputText_Legacy(core, field.nameLc, fieldValue_text, -1, -1, fieldHtmlId, true, false, "password form-control"));
                                    } else {
                                        if (string.IsNullOrEmpty(fieldValue_text)) {
                                            EditorString += (HtmlController.inputNumber(core, field.nameLc, null, fieldHtmlId, "text form-control", editorReadOnly, false));
                                        } else {
                                            EditorString += (HtmlController.inputNumber(core, field.nameLc, encodeNumber(fieldValue_text), fieldHtmlId, "text form-control", editorReadOnly, false));
                                        }
                                    }
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.Link:
                                    //
                                    // ----- Link (href value
                                    //
                                    return_NewFieldList += "," + field.nameLc;
                                    EditorString = ""
                                        + HtmlController.inputText_Legacy(core, field.nameLc, fieldValue_text, 1, 80, fieldHtmlId, false, false, "link form-control") + "&nbsp;<a href=\"#\" onClick=\"OpenResourceLinkWindow( '" + field.nameLc + "' ) ;return false;\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/ResourceLink1616.gif\" width=16 height=16 border=0 alt=\"Link to a resource\" title=\"Link to a resource\"></a>"
                                        + "&nbsp;<a href=\"#\" onClick=\"OpenSiteExplorerWindow( '" + field.nameLc + "' ) ;return false;\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/PageLink1616.gif\" width=16 height=16 border=0 alt=\"Link to a page\" title=\"Link to a page\"></a>";
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.ResourceLink:
                                    //
                                    // ----- Resource Link (src value)
                                    //
                                    return_NewFieldList += "," + field.nameLc;
                                    EditorString = HtmlController.inputText_Legacy(core, field.nameLc, fieldValue_text, 1, 80, fieldHtmlId, false, false, "resourceLink form-control") + "&nbsp;<a href=\"#\" onClick=\"OpenResourceLinkWindow( '" + field.nameLc + "' ) ;return false;\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/ResourceLink1616.gif\" width=16 height=16 border=0 alt=\"Link to a resource\" title=\"Link to a resource\"></a>";
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.HTML:
                                    case CPContentBaseClass.FieldTypeIdEnum.FileHTML:
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
                                    case CPContentBaseClass.FieldTypeIdEnum.LongText:
                                    case CPContentBaseClass.FieldTypeIdEnum.FileText:
                                    //
                                    // -- Long Text, use text editor
                                    return_NewFieldList += "," + field.nameLc;
                                    FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                    EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, fieldHtmlId, false, false, "text form-control");
                                    //
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.FileCSS:
                                    //
                                    // ----- CSS field
                                    return_NewFieldList += "," + field.nameLc;
                                    FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                    EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, 10, -1, fieldHtmlId, false, false, "styles form-control");
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.FileJavascript:
                                    //
                                    // ----- Javascript field
                                    return_NewFieldList += "," + field.nameLc;
                                    FieldRows = (core.userProperty.getInteger(adminData.adminContent.name + "." + field.nameLc + ".RowHeight", 10));
                                    EditorString = HtmlController.inputTextarea(core, field.nameLc, fieldValue_text, FieldRows, -1, fieldHtmlId, false, false, "text form-control");
                                    //
                                    break;
                                    case CPContentBaseClass.FieldTypeIdEnum.FileXML:
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
                                        EditorString = HtmlController.inputText_Legacy(core, field.nameLc, fieldValue_text, -1, -1, fieldHtmlId, true, false, "password form-control");
                                    } else if (!field.htmlContent) {
                                        //
                                        // not HTML capable, textarea with resizing
                                        //
                                        if ((fieldTypeId == CPContentBaseClass.FieldTypeIdEnum.Text) && (fieldValue_text.IndexOf("\n") == -1) && (fieldValue_text.Length < 40)) {
                                            //
                                            // text field shorter then 40 characters without a CR
                                            //
                                            EditorString = HtmlController.inputText_Legacy(core, field.nameLc, fieldValue_text, 1, -1, fieldHtmlId, false, false, "text form-control");
                                        } else {
                                            //
                                            // longer text data, or text that contains a CR
                                            //
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
                        string HelpId = "helpId" + field.id;
                        string HelpEditorId = "helpEditorId" + field.id;
                        string HelpOpenedReadID = "HelpOpenedReadID" + field.id;
                        string HelpOpenedEditId = "HelpOpenedEditID" + field.id;
                        string HelpClosedId = "helpClosedId" + field.id;
                        string HelpClosedContentId = "helpClosedContentId" + field.id;
                        //
                        // editor preferences form - a fancybox popup that interfaces with a hardcoded ajax function in init() to set a member property
                        int editorAddonId = (editorAddon == null) ? 0 : editorAddon.id;
                        string AjaxQS = RequestNameAjaxFunction + "=" + ajaxGetFieldEditorPreferenceForm + "&fieldid=" + field.id + "&currentEditorAddonId=" + editorAddonId + "&fieldTypeDefaultEditorAddonId=" + fieldTypeDefaultEditorAddonId;
                        string fancyBoxLinkId = "fbl" + adminData.fancyBoxPtr;
                        string fancyBoxContentId = "fbc" + adminData.fancyBoxPtr;
                        adminData.fancyBoxHeadJS = adminData.fancyBoxHeadJS + Environment.NewLine + "jQuery('#" + fancyBoxLinkId + "').fancybox({"
                            + "'titleShow':false,"
                            + "'transitionIn':'elastic',"
                            + "'transitionOut':'elastic',"
                            + "'overlayOpacity':'.2',"
                            + "'overlayColor':'#000000',"
                            + "'onStart':function(){cj.ajax.qs('" + AjaxQS + "','','" + fancyBoxContentId + "')}"
                            + "});";
                        EditorHelp = EditorHelp + "\r<div style=\"float:right;\">"
                            + cr2 + "<a id=\"" + fancyBoxLinkId + "\" href=\"#" + fancyBoxContentId + "\" title=\"select an alternate editor for this field.\" tabindex=\"-1\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/NavAltEditor.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"select an alternate editor for this field.\"></a>"
                            + cr2 + "<div style=\"display:none;\">"
                            + cr3 + "<div class=\"ccEditorPreferenceCon\" id=\"" + fancyBoxContentId + "\"><div style=\"margin:20px auto auto auto;\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/ajax-loader-big.gif\" width=\"32\" height=\"32\"></div></div>"
                            + cr2 + "</div>"
                            + "\r</div>"
                            + "";
                        adminData.fancyBoxPtr = adminData.fancyBoxPtr + 1;
                        string HelpMsgOpenedEdit = null;
                        //
                        // field help
                        if (core.session.isAuthenticatedAdmin()) {
                            //
                            // Admin view
                            //
                            if (string.IsNullOrEmpty(HelpMsgDefault)) {
                                HelpMsgDefault = "Admin: No default help is available for this field.";
                            }
                            HelpMsgOpenedRead = ""
                                    + "<!-- close icon --><div class=\"\" style=\"float:right\"><a href=\"javascript:cj.hide('" + HelpOpenedReadID + "');cj.show('" + HelpClosedId + "');\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"close\"></a></div>"
                                    + "<div class=\"header\">Default Help</div>"
                                    + "<div class=\"body\">" + HelpMsgDefault + "</div>"
                                    + "<div class=\"header\">Custom Help</div>"
                                    + "<div class=\"body\">" + HelpMsgCustom + "</div>"
                                + "";
                            string jsUpdate = "updateFieldHelp('" + field.id + "','" + HelpEditorId + "','" + HelpClosedContentId + "');cj.hide('" + HelpOpenedEditId + "');cj.show('" + HelpClosedId + "');return false";
                            string jsCancel = "cj.hide('" + HelpOpenedEditId + "');cj.show('" + HelpClosedId + "');return false";
                            HelpMsgOpenedEdit = ""
                                    + "<div class=\"header\">Default Help</div>"
                                    + "<div class=\"body\">" + HelpMsgDefault + "</div>"
                                    + "<div class=\"header\">Custom Help</div>"
                                    + "<div class=\"body\"><textarea id=\"" + HelpEditorId + "\" ROWS=\"10\" style=\"width:100%;\">" + HelpMsgCustom + "</TEXTAREA></div>"
                                    + "<div class=\"\">"
                                        + AdminUIController.getButtonPrimary("Update", jsUpdate)
                                        + AdminUIController.getButtonPrimary("Cancel", jsCancel)
                                    + "</div>"
                                + "";
                            if (IsLongHelp) {
                                //
                                // Long help, closed gets MoreHelpIcon (opens to HelpMsgOpenedRead) and HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                HelpMsgClosed = ""
                                        + "<!-- open read icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide('" + HelpClosedId + "');cj.show('" + HelpOpenedReadID + "');\" tabindex=\"-1\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"more help\"></a></div>"
                                        + "<!-- open edit icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide('" + HelpClosedId + "');cj.show('" + HelpOpenedEditId + "');\" tabindex=\"-1\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/NavHelpEdit.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"edit help\"></a></div>"
                                        + "<div id=\"" + HelpClosedContentId + "\">" + HelpMsgClosed + "</div>"
                                    + "";
                            } else if (!IsEmptyHelp) {
                                //
                                // short help, closed gets helpmsgclosed + HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                HelpMsgClosed = ""
                                        + "<!-- open edit icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide('" + HelpClosedId + "');cj.show('" + HelpOpenedEditId + "');\" tabindex=\"-1\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/NavHelpEdit.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"edit help\"></a></div>"
                                        + "<div id=\"" + HelpClosedContentId + "\">" + HelpMsgClosed + "</div>"
                                    + "";
                            } else {
                                //
                                // Empty help, closed gets helpmsgclosed + HelpEditIcon (opens to readonly default copy plus editor with custom copy)
                                HelpMsgClosed = ""
                                        + "<!-- open edit icon --><div style=\"float:right;\"><a href=\"javascript:cj.hide('" + HelpClosedId + "');cj.show('" + HelpOpenedEditId + "');\" tabindex=\"-1\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/NavHelpEdit.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;\" title=\"edit help\"></a></div>"
                                        + "<div id=\"" + HelpClosedContentId + "\">" + HelpMsgClosed + "</div>"
                                    + "";
                            }
                            EditorHelp = EditorHelp + "<div id=\"" + HelpOpenedReadID + "\" class=\"opened\">" + HelpMsgOpenedRead + "</div>"
                                + "<div id=\"" + HelpOpenedEditId + "\" class=\"opened\">" + HelpMsgOpenedEdit + "</div>"
                                + "<div id=\"" + HelpClosedId + "\" class=\"closed\">" + HelpMsgClosed + "</div>"
                                + "";
                        } else {
                            //
                            // Non-admin view
                            HelpMsgOpenedRead = ""
                                    + "<div class=\"body\">"
                                    + "<!-- close icon --><a href=\"javascript:cj.hide('" + HelpOpenedReadID + "');cj.show('" + HelpClosedId + "');\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;float:right\" title=\"close\"></a>"
                                    + HelpMsg + "</div>"
                                + "";
                            HelpMsgOpenedEdit = ""
                                 + "";
                            if (IsLongHelp) {
                                //
                                // Long help
                                HelpMsgClosed = ""
                                    + "<div class=\"body\">"
                                    + "<!-- open read icon --><a href=\"javascript:cj.hide('" + HelpClosedId + "');cj.show('" + HelpOpenedReadID + "');\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/NavHelp.gif\" width=18 height=18 border=0 style=\"vertical-align:middle;float:right;\" title=\"more help\"></a>"
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
                                + "<div id=\"" + HelpClosedId + "\" class=\"closed\">" + HelpMsgClosed + "</div>"
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
                    adminData.editSectionPanelCount = adminData.editSectionPanelCount + 1;
                    returnHtml = AdminUIController.getEditPanel(core, (!adminData.allowAdminTabs), fieldCaption, "", AdminUIController.editTable(resultBody.Text));
                    adminData.editSectionPanelCount = adminData.editSectionPanelCount + 1;
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

        /// <param name="fieldEditorList"></param>
        /// <param name="styleList"></param>
        /// <param name="styleOptionList"></param>
        /// <param name="emailIdForStyles"></param>
        /// <param name="IsTemplateTable"></param>
        /// <param name="editorAddonListJSON"></param>
        /// <returns></returns>
        public static string getTabs(CoreController core, AdminDataModel adminData, TabController adminMenu, bool readOnlyField, bool IsLandingPage, bool IsRootPage, CPHtml5BaseClass.EditorContentType EditorContext, bool allowAjaxTabs, int TemplateIDForStyles, string styleList, string styleOptionList, int emailIdForStyles, bool IsTemplateTable, string editorAddonListJSON) {
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
                        int LastFieldId = 0;
                        if (fieldId != LastFieldId) {
                            LastFieldId = fieldId;
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
                        string tabContent = getTab(core, adminData, adminData.editRecord.id, adminData.adminContent.id, readOnlyField, IsLandingPage, IsRootPage, field.editTabName, EditorContext, ref NewFormFieldList, TemplateIDForStyles, HelpCnt, HelpIDCache, helpDefaultCache, HelpCustomCache, AllowHelpMsgCustom, helpIdIndex, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON);
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
        //

    }
}
