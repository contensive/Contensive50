
using System;
using System.Collections.Generic;
using System.Data;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Exceptions;
using Contensive.Processor.Addons.AdminSite.Controllers;
using Contensive.BaseClasses;
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
        public static string getTab(CoreController core, AdminDataModel adminData, EditorEnvironmentModel editorEnv, int RecordID, int ContentID, string EditTab, CPHtml5BaseClass.EditorContentType EditorContext, int TemplateIDForStyles, int HelpCnt, int[] HelpIDCache, string[] helpDefaultCache, string[] HelpCustomCache, KeyPtrController helpIdIndex, string styleList, int emailIdForStyles, bool IsTemplateTable) {
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
                        string editorRow = EditorRowClass.getEditorRow(core, field, adminData, editorEnv);
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
        public static string getTabs(CoreController core, AdminDataModel adminData, TabController adminMenu, EditorEnvironmentModel editorEnv, CPHtml5BaseClass.EditorContentType EditorContext, bool allowAjaxTabs, int TemplateIDForStyles, string styleList, int emailIdForStyles, bool IsTemplateTable) {
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
                    editorEnv.AllowHelpMsgCustom = true;
                }
                //
                string FormFieldList = ",";
                List<string> TabsFound = new List<string>();
                foreach (KeyValuePair<string, ContentFieldMetadataModel> keyValuePair in adminData.adminContent.fields) {
                    editorEnv.tabFieldList = "";
                    ContentFieldMetadataModel field = keyValuePair.Value;
                    if ((!field.editTabName.ToLowerInvariant().Equals("control info")) && (field.authorable) && (field.active) && (!TabsFound.Contains(field.editTabName.ToLowerInvariant()))) {
                        TabsFound.Add(field.editTabName.ToLowerInvariant());
                        string editTabCaption = field.editTabName;
                        if (string.IsNullOrEmpty(editTabCaption)) { editTabCaption = "Details"; }
                        string tabContent = getTab(core, adminData, editorEnv, adminData.editRecord.id, adminData.adminContent.id, field.editTabName, EditorContext, TemplateIDForStyles, HelpCnt, HelpIDCache, helpDefaultCache, HelpCustomCache,  helpIdIndex, styleList, emailIdForStyles, IsTemplateTable);
                        if (!string.IsNullOrEmpty(tabContent)) {
                            returnHtml += addTab(core, adminMenu, editTabCaption, tabContent, adminData.allowAdminTabs);
                        }
                        if (!string.IsNullOrEmpty(editorEnv.tabFieldList)) { FormFieldList = editorEnv.tabFieldList + FormFieldList; }
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
