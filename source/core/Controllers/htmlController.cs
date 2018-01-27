
using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Contensive.BaseClasses;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;

namespace Contensive.Core.Controllers {
    /// <summary>
    /// Tools used to assemble html document elements. This is not a storage for assembling a document (see docController)
    /// </summary>
    public class htmlController {
        //
        private coreController core;
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="core"></param>
        /// <remarks></remarks>
        public htmlController(coreController core) {
            this.core = core;
        }
        //
        //====================================================================================================
        //
        public string getHtmlBodyEnd(bool AllowLogin, bool AllowTools) {
            List<string> result = new List<string>();
            try {
                //
                // -- content extras like tool panel
                if (core.doc.sessionContext.isAuthenticatedContentManager(core) & (core.doc.sessionContext.user.AllowToolsPanel)) {
                    if (AllowTools) {
                        result.Add(core.html.getToolsPanel());
                    }
                } else {
                    if (AllowLogin) {
                        result.Add(getLoginLink());
                    }
                }
                //
                // TODO -- closing the menu attaches the flyout panels -- should be done when the menu is returned, not at page end
                // -- output the menu system
                if (core.menuFlyout != null) {
                    result.Add(core.menuFlyout.menu_GetClose());
                }
                //
                // -- body Javascript
                bool allowDebugging = core.visitProperty.getBoolean("AllowDebugging");
                var scriptOnLoad = new List<string>();
                foreach (var asset in core.doc.htmlAssetList.FindAll((a) => ((a.assetType == htmlAssetTypeEnum.script) || (a.assetType == htmlAssetTypeEnum.scriptOnLoad)) && (!a.inHead) && (!string.IsNullOrEmpty(a.content)))) {
                    if ((asset.addedByMessage != "") && allowDebugging) {
                        result.Add("\r\n<!-- from " + asset.addedByMessage + " -->\r\n");
                    }
                    if (asset.assetType == htmlAssetTypeEnum.scriptOnLoad) {
                        scriptOnLoad.Add( asset.content + ";" );
                    } if (!asset.isLink) {
                        result.Add("<script Language=\"JavaScript\" type=\"text/javascript\">" + asset.content + "</script>");
                    } else {
                        result.Add("<script type=\"text/javascript\" src=\"" + asset.content + "\"></script>");
                    }
                }
                if (scriptOnLoad.Count>0) {
                    result.Add(""
                        + "\r\n<script Language=\"JavaScript\" type=\"text/javascript\">" 
                        + "function ready(callback){" 
                            + "if (document.readyState!='loading') callback(); "
                            + "else if (document.addEventListener) document.addEventListener('DOMContentLoaded', callback); "
                            + "else document.attachEvent('onreadystatechange', function(){"
                                + "if (document.readyState=='complete') callback();"
                            + "});"
                        + "} ready(function(){" + string.Join("\r\n", scriptOnLoad) + "\r\n});"
                        + "</script>");

                }
                //
                // -- Include any other close page
                if (core.doc.htmlForEndOfBody != "") {
                    result.Add(core.doc.htmlForEndOfBody);
                }
                if (core.doc.testPointMessage != "") {
                    result.Add("<div class=\"ccTestPointMessageCon\">" + core.doc.testPointMessage + "</div>");
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return string.Join("\r", result);
        }
        //
        //====================================================================================================
        // main_Get a string with a Drop Down Select Box, see PrintFormInputSelect
        //
        public string selectFromContent(string MenuName, int CurrentValue, string ContentName, string Criteria = "", string NoneCaption = "", string htmlId = "") {
            bool tempVar = false;
            return selectFromContent(MenuName, CurrentValue, ContentName, Criteria, NoneCaption, htmlId, ref tempVar, "");
        }
        //
        //====================================================================================================
        //
        public string selectFromContent(string MenuName, int CurrentValue, string ContentName, string Criteria, string NoneCaption, string htmlId, ref bool return_IsEmptyList, string HtmlClass = "") {
            string result = "";
            try {
                const string MenuNameFPO = "<MenuName>";
                const string NoneCaptionFPO = "<NoneCaption>";
                Models.Complex.cdefModel CDef = null;
                string ContentControlCriteria = null;
                string LcaseCriteria = null;
                int CSPointer = 0;
                bool SelectedFound = false;
                int RecordID = 0;
                string Copy = null;
                string DropDownFieldList = null;
                string[] DropDownFieldName = { };
                string[] DropDownDelimiter = { };
                int DropDownFieldCount = 0;
                string DropDownPreField = "";
                int DropDownFieldListLength = 0;
                string FieldName = "";
                string CharAllowed = null;
                string CharTest = null;
                int CharPointer = 0;
                int IDFieldPointer = 0;
                stringBuilderLegacyController FastString = new stringBuilderLegacyController();
                string[,] RowsArray = null;
                string[] RowFieldArray = null;
                int RowCnt = 0;
                int RowMax = 0;
                int ColumnMax = 0;
                int RowPointer = 0;
                int ColumnPointer = 0;
                int[] DropDownFieldPointer = null;
                string UcaseFieldName = null;
                string SortFieldList = "";
                string SQL = null;
                string TableName = null;
                string DataSource = null;
                string SelectFields = null;
                int Ptr = 0;
                string SelectRaw = "";
                string TagID = null;
                string CurrentValueText = null;
                //
                LcaseCriteria = genericController.vbLCase(Criteria);
                return_IsEmptyList = true;
                //
                CurrentValueText = CurrentValue.ToString();
                foreach (constants.cacheInputSelectClass inputSelect in core.doc.inputSelectCache) {
                    if ((inputSelect.ContentName == ContentName) && (inputSelect.Criteria == LcaseCriteria) && (inputSelect.CurrentValue == CurrentValueText)) {
                        SelectRaw = inputSelect.SelectRaw;
                        return_IsEmptyList = false;
                        break;
                    }
                }
                //
                //
                //
                if (string.IsNullOrEmpty(SelectRaw)) {
                    //
                    // Build the SelectRaw
                    // Test selection size
                    //
                    // This was commented out -- I really do not know why -- seems like the best way
                    //
                    CDef = Models.Complex.cdefModel.getCdef(core, ContentName);
                    TableName = CDef.ContentTableName;
                    DataSource = CDef.ContentDataSourceName;
                    ContentControlCriteria = CDef.ContentControlCriteria;
                    //
                    // This is what was there
                    //
                    //        TableName = main_GetContentProperty(ContentName, "ContentTableName")
                    //        DataSource = main_GetContentProperty(ContentName, "ContentDataSourceName")
                    //        ContentControlCriteria = main_GetContentProperty(ContentName, "ContentControlCriteria")
                    //
                    SQL = "select count(*) as cnt from " + TableName + " where " + ContentControlCriteria;
                    if (!string.IsNullOrEmpty(LcaseCriteria)) {
                        SQL += " and " + LcaseCriteria;
                    }
                    DataTable dt = core.db.executeQuery(SQL);
                    if (dt.Rows.Count > 0) {
                        RowCnt = genericController.encodeInteger(dt.Rows[0]["cnt"]);
                    }
                    if (RowCnt == 0) {
                        RowMax = -1;
                    } else {
                        return_IsEmptyList = false;
                        RowMax = RowCnt - 1;
                    }
                    //
                    if (RowCnt > core.siteProperties.selectFieldLimit) {
                        //
                        // Selection is too big
                        //
                        errorController.addUserError(core, "The drop down list for " + ContentName + " called " + MenuName + " is too long to display. The site administrator has been notified and the problem will be resolved shortly. To fix this issue temporarily, go to the admin tab of the Preferences page and set the Select Field Limit larger than " + RowCnt + ".");
                        //                    core.handleException(New Exception("Legacy error, MethodName=[" & MethodName & "], cause=[" & Cause & "] #" & Err.Number & "," & Err.Source & "," & Err.Description & ""), Cause, 2)

                        core.handleException(new Exception("Error creating select list from content [" + ContentName + "] called [" + MenuName + "]. Selection of [" + RowCnt + "] records exceeds [" + core.siteProperties.selectFieldLimit + "], the current Site Property SelectFieldLimit."));
                        result = result + inputHidden(MenuNameFPO, CurrentValue);
                        if (CurrentValue == 0) {
                            result = inputText(MenuName, "0");
                        } else {
                            CSPointer = core.db.csOpenRecord(ContentName, CurrentValue);
                            if (core.db.csOk(CSPointer)) {
                                result = core.db.csGetText(CSPointer, "name") + "&nbsp;";
                            }
                            core.db.csClose(ref CSPointer);
                        }
                        result = result + "(Selection is too large to display option list)";
                    } else {
                        //
                        // ----- Generate Drop Down Field Names
                        //
                        DropDownFieldList = CDef.DropDownFieldList;
                        //DropDownFieldList = main_GetContentProperty(ContentName, "DropDownFieldList")
                        if (string.IsNullOrEmpty(DropDownFieldList)) {
                            DropDownFieldList = "NAME";
                        }
                        DropDownFieldCount = 0;
                        CharAllowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                        DropDownFieldListLength = DropDownFieldList.Length;
                        for (CharPointer = 1; CharPointer <= DropDownFieldListLength; CharPointer++) {
                            CharTest = DropDownFieldList.Substring(CharPointer - 1, 1);
                            if (genericController.vbInstr(1, CharAllowed, CharTest) == 0) {
                                //
                                // Character not allowed, delimit Field name here
                                //
                                if (!string.IsNullOrEmpty(FieldName)) {
                                    //
                                    // ----- main_Get new Field Name and save it
                                    //
                                    if (string.IsNullOrEmpty(SortFieldList)) {
                                        SortFieldList = FieldName;
                                    }
                                    Array.Resize(ref DropDownFieldName, DropDownFieldCount + 1);
                                    Array.Resize(ref DropDownDelimiter, DropDownFieldCount + 1);
                                    DropDownFieldName[DropDownFieldCount] = FieldName;
                                    DropDownDelimiter[DropDownFieldCount] = CharTest;
                                    DropDownFieldCount = DropDownFieldCount + 1;
                                    FieldName = "";
                                } else {
                                    //
                                    // ----- Save Field Delimiter
                                    //
                                    if (DropDownFieldCount == 0) {
                                        //
                                        // ----- Before any field, add to DropDownPreField
                                        //
                                        DropDownPreField = DropDownPreField + CharTest;
                                    } else {
                                        //
                                        // ----- after a field, add to last DropDownDelimiter
                                        //
                                        DropDownDelimiter[DropDownFieldCount - 1] = DropDownDelimiter[DropDownFieldCount - 1] + CharTest;
                                    }
                                }
                            } else {
                                //
                                // Character Allowed, Put character into fieldname and continue
                                //
                                FieldName = FieldName + CharTest;
                            }
                        }
                        if (!string.IsNullOrEmpty(FieldName)) {
                            if (string.IsNullOrEmpty(SortFieldList)) {
                                SortFieldList = FieldName;
                            }
                            Array.Resize(ref DropDownFieldName, DropDownFieldCount + 1);
                            Array.Resize(ref DropDownDelimiter, DropDownFieldCount + 1);
                            DropDownFieldName[DropDownFieldCount] = FieldName;
                            DropDownDelimiter[DropDownFieldCount] = "";
                            DropDownFieldCount = DropDownFieldCount + 1;
                        }
                        if (DropDownFieldCount == 0) {
                            core.handleException(new Exception("No drop down field names found for content [" + ContentName + "]."));
                        } else {
                            DropDownFieldPointer = new int[DropDownFieldCount];
                            SelectFields = "ID";
                            for (Ptr = 0; Ptr < DropDownFieldCount; Ptr++) {
                                SelectFields = SelectFields + "," + DropDownFieldName[Ptr];
                            }
                            //
                            // ----- Start select box
                            //
                            TagID = "";
                            if (!string.IsNullOrEmpty(htmlId)) {
                                TagID = " ID=\"" + htmlId + "\"";
                            }
                            FastString.Add("<select size=\"1\" name=\"" + MenuNameFPO + "\"" + TagID + ">");
                            FastString.Add("<option value=\"\">" + NoneCaptionFPO + "</option>");
                            //
                            // ----- select values
                            //
                            CSPointer = core.db.csOpen(ContentName, Criteria, SortFieldList, false, 0, false, false, SelectFields);
                            if (core.db.csOk(CSPointer)) {
                                RowsArray = core.db.cs_getRows(CSPointer);
                                RowFieldArray = core.db.cs_getSelectFieldList(CSPointer).Split(',');
                                ColumnMax = RowsArray.GetUpperBound(0);
                                RowMax = RowsArray.GetUpperBound(1);
                                //
                                // -- setup IDFieldPointer
                                UcaseFieldName = "ID";
                                for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                    if (UcaseFieldName == genericController.vbUCase(RowFieldArray[ColumnPointer])) {
                                        IDFieldPointer = ColumnPointer;
                                        break;
                                    }
                                }
                                //
                                // setup DropDownFieldPointer()
                                //
                                for (var FieldPointer = 0; FieldPointer < DropDownFieldCount; FieldPointer++) {
                                    UcaseFieldName = genericController.vbUCase(DropDownFieldName[FieldPointer]);
                                    for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                        if (UcaseFieldName == genericController.vbUCase(RowFieldArray[ColumnPointer])) {
                                            DropDownFieldPointer[FieldPointer] = ColumnPointer;
                                            break;
                                        }
                                    }
                                }
                                //
                                // output select
                                //
                                SelectedFound = false;
                                for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                                    RecordID = genericController.encodeInteger(RowsArray[IDFieldPointer, RowPointer]);
                                    Copy = DropDownPreField;
                                    for (var FieldPointer = 0; FieldPointer < DropDownFieldCount; FieldPointer++) {
                                        Copy += RowsArray[DropDownFieldPointer[FieldPointer], RowPointer] + DropDownDelimiter[FieldPointer];
                                    }
                                    if (string.IsNullOrEmpty(Copy)) {
                                        Copy = "no name";
                                    }
                                    FastString.Add("\r\n<option value=\"" + RecordID + "\" ");
                                    if (RecordID == CurrentValue) {
                                        FastString.Add("selected");
                                        SelectedFound = true;
                                    }
                                    if (core.siteProperties.selectFieldWidthLimit != 0) {
                                        if (Copy.Length > core.siteProperties.selectFieldWidthLimit) {
                                            Copy = Copy.Left(core.siteProperties.selectFieldWidthLimit) + "...+";
                                        }
                                    }
                                    FastString.Add(">" + genericController.encodeHTML(Copy) + "</option>");
                                }
                                if (!SelectedFound && (CurrentValue != 0)) {
                                    core.db.csClose(ref CSPointer);
                                    if (!string.IsNullOrEmpty(Criteria)) {
                                        Criteria = Criteria + "and";
                                    }
                                    Criteria = Criteria + "(id=" + genericController.encodeInteger(CurrentValue) + ")";
                                    CSPointer = core.db.csOpen(ContentName, Criteria, SortFieldList, false, 0, false, false, SelectFields);
                                    if (core.db.csOk(CSPointer)) {
                                        RowsArray = core.db.cs_getRows(CSPointer);
                                        RowFieldArray = core.db.cs_getSelectFieldList(CSPointer).Split(',');
                                        RowMax = RowsArray.GetUpperBound(1);
                                        ColumnMax = RowsArray.GetUpperBound(0);
                                        RecordID = genericController.encodeInteger(RowsArray[IDFieldPointer, 0]);
                                        Copy = DropDownPreField;
                                        for (var FieldPointer = 0; FieldPointer < DropDownFieldCount; FieldPointer++) {
                                            Copy += RowsArray[DropDownFieldPointer[FieldPointer], 0] + DropDownDelimiter[FieldPointer];
                                        }
                                        if (string.IsNullOrEmpty(Copy)) {
                                            Copy = "no name";
                                        }
                                        FastString.Add("\r\n<option value=\"" + RecordID + "\" selected");
                                        SelectedFound = true;
                                        if (core.siteProperties.selectFieldWidthLimit != 0) {
                                            if (Copy.Length > core.siteProperties.selectFieldWidthLimit) {
                                                Copy = Copy.Left(core.siteProperties.selectFieldWidthLimit) + "...+";
                                            }
                                        }
                                        FastString.Add(">" + genericController.encodeHTML(Copy) + "</option>");
                                    }
                                }
                            }
                            FastString.Add("</select>");
                            core.db.csClose(ref CSPointer);
                            SelectRaw = FastString.Text;
                        }
                    }
                    //
                    // Save the SelectRaw
                    //
                    if (!return_IsEmptyList) {
                        core.doc.inputSelectCache.Add(new constants.cacheInputSelectClass() {
                            ContentName = ContentName,
                            Criteria = Criteria,
                            CurrentValue = CurrentValue.ToString(),
                            SelectRaw = SelectRaw
                        });
                    }
                }
                //
                SelectRaw = genericController.vbReplace(SelectRaw, MenuNameFPO, MenuName);
                SelectRaw = genericController.vbReplace(SelectRaw, NoneCaptionFPO, NoneCaption);
                if (!string.IsNullOrEmpty(HtmlClass)) {
                    SelectRaw = genericController.vbReplace(SelectRaw, "<select ", "<select class=\"" + HtmlClass + "\"");
                }
                result = SelectRaw;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        ////
        ////====================================================================================================
        ////
        //public string selectUserFromGroup(string MenuName, int CurrentValue, int GroupID, string ignore = "", string NoneCaption = "", string htmlId = "") {
        //    return selectUserFromGroup(MenuName, CurrentValue, GroupID, "", NoneCaption, htmlId);
        //}
        //
        //====================================================================================================
        //
        public string selectUserFromGroup(string MenuName, int currentValue, int GroupID, string ignore = "", string noneCaption = "", string HtmlId = "", string HtmlClass = "select") {
            string result = "";
            try {
                stringBuilderLegacyController FastString = new stringBuilderLegacyController();
                //
                const string MenuNameFPO = "<MenuName>";
                const string NoneCaptionFPO = "<NoneCaption>";
                //
                string iMenuName = genericController.encodeText(MenuName);
                currentValue = genericController.encodeInteger(currentValue);
                noneCaption = genericController.encodeEmptyText(noneCaption, "Select One");
                string sqlCriteria = "";
                //
                string SelectRaw = "";
                foreach (constants.cacheInputSelectClass cacheInputSelect in core.doc.inputSelectCache) {
                    if ((cacheInputSelect.ContentName == "Group:" + GroupID) && (cacheInputSelect.Criteria == sqlCriteria) && (genericController.encodeInteger(cacheInputSelect.CurrentValue) == currentValue)) {
                        SelectRaw = cacheInputSelect.SelectRaw;
                        break;
                    }
                }
                //
                //
                //
                if (string.IsNullOrEmpty(SelectRaw)) {
                    //
                    // Build the SelectRaw
                    // Test selection size
                    //
                    int RowMax = 0;
                    string SQL = "select count(*) as cnt"
                        + " from ccMemberRules R"
                        + " inner join ccMembers P on R.MemberID=P.ID"
                        + " where (P.active<>0)"
                        + " and (R.GroupID=" + GroupID + ")";
                    int CSPointer = core.db.csOpenSql(SQL);
                    if (core.db.csOk(CSPointer)) {
                        RowMax = RowMax + core.db.csGetInteger(CSPointer, "cnt");
                    }
                    core.db.csClose(ref CSPointer);
                    if (RowMax > core.siteProperties.selectFieldLimit) {
                        //
                        // Selection is too big
                        //
                        core.handleException(new Exception("While building a group members list for group [" + groupController.group_GetGroupName(core, GroupID) + "], too many rows were selected. [" + RowMax + "] records exceeds [" + core.siteProperties.selectFieldLimit + "], the current Site Property app.SiteProperty_SelectFieldLimit."));
                        result = result + inputHidden(MenuNameFPO, currentValue);
                        if (currentValue != 0) {
                            CSPointer = core.db.csOpenRecord("people", currentValue);
                            if (core.db.csOk(CSPointer)) {
                                result = core.db.csGetText(CSPointer, "name") + "&nbsp;";
                            }
                            core.db.csClose(ref CSPointer);
                        }
                        result = result + "(Selection is too large to display)";
                    } else {
                        //
                        // ----- Generate Drop Down Field Names
                        //
                        string DropDownFieldList = Models.Complex.cdefModel.GetContentProperty(core, "people", "DropDownFieldList");
                        if (string.IsNullOrEmpty(DropDownFieldList)) {
                            DropDownFieldList = "NAME";
                        }
                        int DropDownFieldCount = 0;
                        string CharAllowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                        string DropDownPreField = "";
                        string FieldName = "";
                        string SortFieldList = "";
                        string[] DropDownFieldName = { };
                        string[] DropDownDelimiter = { };
                        for (int CharPointer = 1; CharPointer <= DropDownFieldList.Length; CharPointer++) {
                            string CharTest = DropDownFieldList.Substring(CharPointer - 1, 1);
                            if (genericController.vbInstr(1, CharAllowed, CharTest) == 0) {
                                //
                                // Character not allowed, delimit Field name here
                                //
                                if (!string.IsNullOrEmpty(FieldName)) {
                                    //
                                    // ----- main_Get new Field Name and save it
                                    //
                                    if (string.IsNullOrEmpty(SortFieldList)) {
                                        SortFieldList = FieldName;
                                    }
                                    Array.Resize(ref DropDownFieldName, DropDownFieldCount + 1);
                                    Array.Resize(ref DropDownDelimiter, DropDownFieldCount + 1);
                                    DropDownFieldName[DropDownFieldCount] = FieldName;
                                    DropDownDelimiter[DropDownFieldCount] = CharTest;
                                    DropDownFieldCount = DropDownFieldCount + 1;
                                    FieldName = "";
                                } else {
                                    //
                                    // ----- Save Field Delimiter
                                    //
                                    if (DropDownFieldCount == 0) {
                                        //
                                        // ----- Before any field, add to DropDownPreField
                                        //
                                        DropDownPreField = DropDownPreField + CharTest;
                                    } else {
                                        //
                                        // ----- after a field, add to last DropDownDelimiter
                                        //
                                        DropDownDelimiter[DropDownFieldCount - 1] = DropDownDelimiter[DropDownFieldCount - 1] + CharTest;
                                    }
                                }
                            } else {
                                //
                                // Character Allowed, Put character into fieldname and continue
                                //
                                FieldName = FieldName + CharTest;
                            }
                        }
                        if (!string.IsNullOrEmpty(FieldName)) {
                            if (string.IsNullOrEmpty(SortFieldList)) {
                                SortFieldList = FieldName;
                            }
                            Array.Resize(ref DropDownFieldName, DropDownFieldCount + 1);
                            Array.Resize(ref DropDownDelimiter, DropDownFieldCount + 1);
                            DropDownFieldName[DropDownFieldCount] = FieldName;
                            DropDownDelimiter[DropDownFieldCount] = "";
                            DropDownFieldCount = DropDownFieldCount + 1;
                        }
                        if (DropDownFieldCount == 0) {
                            core.handleException(new Exception("No drop down field names found for content [" + GroupID + "]."));
                        } else {
                            int[] DropDownFieldPointer = new int[DropDownFieldCount];
                            string SelectFields = "P.ID";
                            for (int Ptr = 0; Ptr < DropDownFieldCount; Ptr++) {
                                SelectFields = SelectFields + ",P." + DropDownFieldName[Ptr];
                            }
                            //
                            // ----- Start select box
                            //
                            string TagClass = "";
                            if (genericController.encodeEmptyText(HtmlClass, "") != "") {
                                TagClass = " Class=\"" + genericController.encodeEmptyText(HtmlClass, "") + "\"";
                            }
                            //
                            string TagID = "";
                            if (genericController.encodeEmptyText(HtmlId, "") != "") {
                                TagID = " ID=\"" + genericController.encodeEmptyText(HtmlId, "") + "\"";
                            }
                            //
                            FastString.Add("<select size=\"1\" name=\"" + MenuNameFPO + "\"" + TagID + TagClass + ">");
                            FastString.Add("<option value=\"\">" + NoneCaptionFPO + "</option>");
                            //
                            // ----- select values
                            //
                            if (string.IsNullOrEmpty(SortFieldList)) {
                                SortFieldList = "name";
                            }
                            SQL = "select " + SelectFields + " from ccMemberRules R"
                                + " inner join ccMembers P on R.MemberID=P.ID"
                                + " where (R.GroupID=" + GroupID + ")"
                                + " and((R.DateExpires is null)or(R.DateExpires>" + core.db.encodeSQLDate(DateTime.Now) + "))"
                                + " and(P.active<>0)"
                                + " order by P." + SortFieldList;
                            CSPointer = core.db.csOpenSql(SQL);
                            if (core.db.csOk(CSPointer)) {
                                string[,] RowsArray = core.db.cs_getRows(CSPointer);
                                string[] RowFieldArray = core.db.cs_getSelectFieldList(CSPointer).Split(',');
                                RowMax = RowsArray.GetUpperBound(1);
                                int ColumnMax = RowsArray.GetUpperBound(0);
                                //
                                // setup IDFieldPointer
                                //
                                string UcaseFieldName = "ID";
                                int IDFieldPointer = 0;
                                for (int ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                    if (UcaseFieldName == genericController.vbUCase(RowFieldArray[ColumnPointer])) {
                                        IDFieldPointer = ColumnPointer;
                                        break;
                                    }
                                }
                                //
                                // setup DropDownFieldPointer()
                                //
                                for (var FieldPointer = 0; FieldPointer < DropDownFieldCount; FieldPointer++) {
                                    UcaseFieldName = genericController.vbUCase(DropDownFieldName[FieldPointer]);
                                    for (int ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                        if (UcaseFieldName == genericController.vbUCase(RowFieldArray[ColumnPointer])) {
                                            DropDownFieldPointer[FieldPointer] = ColumnPointer;
                                            break;
                                        }
                                    }
                                }
                                //
                                // output select
                                //
                                int LastRecordID = -1;
                                for (int RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                                    int RecordID = genericController.encodeInteger(RowsArray[IDFieldPointer, RowPointer]);
                                    if (RecordID != LastRecordID) {
                                        string Copy = DropDownPreField;
                                        for (var FieldPointer = 0; FieldPointer < DropDownFieldCount; FieldPointer++) {
                                            Copy += RowsArray[DropDownFieldPointer[FieldPointer], RowPointer] + DropDownDelimiter[FieldPointer];
                                        }
                                        if (string.IsNullOrEmpty(Copy)) {
                                            Copy = "no name";
                                        }
                                        FastString.Add("\r\n<option value=\"" + RecordID + "\" ");
                                        if (RecordID == currentValue) {
                                            FastString.Add("selected");
                                        }
                                        if (core.siteProperties.selectFieldWidthLimit != 0) {
                                            if (Copy.Length > core.siteProperties.selectFieldWidthLimit) {
                                                Copy = Copy.Left(core.siteProperties.selectFieldWidthLimit) + "...+";
                                            }
                                        }
                                        FastString.Add(">" + Copy + "</option>");
                                        LastRecordID = RecordID;
                                    }
                                }
                            }
                            FastString.Add("</select>");
                            core.db.csClose(ref CSPointer);
                            SelectRaw = FastString.Text;
                        }
                    }
                    //
                    // Save the SelectRaw
                    //
                    core.doc.inputSelectCache.Add(new constants.cacheInputSelectClass() {
                        ContentName = "Group:" + GroupID,
                        Criteria = sqlCriteria,
                        CurrentValue = currentValue.ToString(),
                        SelectRaw = SelectRaw
                    });
                }
                //
                SelectRaw = genericController.vbReplace(SelectRaw, MenuNameFPO, iMenuName);
                SelectRaw = genericController.vbReplace(SelectRaw, NoneCaptionFPO, noneCaption);
                result = SelectRaw;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public string selectFromList(string MenuName, string CurrentValue, string SelectList, string NoneCaption = "", string htmlId = "") {
            return selectFromList(genericController.encodeText(MenuName), genericController.encodeInteger(CurrentValue), genericController.encodeText(SelectList), genericController.encodeText(NoneCaption), genericController.encodeText(htmlId));
        }
        //
        //====================================================================================================
        //   Create a select list from a comma separated list, returns an index into the list list, starting at 1, if an element is blank (,) no option is created
        //
        public string selectFromList(string MenuName, int CurrentValue, string SelectList, string NoneCaption, string htmlId, string HtmlClass = "") {
            string result = "";
            try {
                //
                stringBuilderLegacyController FastString = new stringBuilderLegacyController();
                string[] lookups = null;
                int Ptr = 0;
                int RecordID = 0;
                string Copy = null;
                int SelectFieldWidthLimit;
                //
                SelectFieldWidthLimit = core.siteProperties.selectFieldWidthLimit;
                if (SelectFieldWidthLimit == 0) {
                    SelectFieldWidthLimit = 256;
                }
                //
                // ----- Start select box
                //
                FastString.Add("<select id=\"" + htmlId + "\" class=\"" + HtmlClass + "\" size=\"1\" name=\"" + MenuName + "\">");
                if (!string.IsNullOrEmpty(NoneCaption)) {
                    FastString.Add("<option value=\"\">" + NoneCaption + "</option>");
                } else {
                    FastString.Add("<option value=\"\">Select One</option>");
                }
                //
                // ----- select values
                //
                lookups = SelectList.Split(',');
                for (Ptr = 0; Ptr <= lookups.GetUpperBound(0); Ptr++) {
                    RecordID = Ptr + 1;
                    Copy = lookups[Ptr];
                    if (!string.IsNullOrEmpty(Copy)) {
                        FastString.Add("\r\n<option value=\"" + RecordID + "\" ");
                        if (RecordID == CurrentValue) {
                            FastString.Add("selected");
                            //SelectedFound = True
                        }
                        if (Copy.Length > SelectFieldWidthLimit) {
                            Copy = Copy.Left(SelectFieldWidthLimit) + "...+";
                        }
                        FastString.Add(">" + Copy + "</option>");
                    }
                }
                FastString.Add("</select>");
                result = FastString.Text;
                //
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //   Display an icon with a link to the login form/cclib.net/admin area
        //
        public string getLoginLink() {
            string result = "";
            try {
                //
                //If Not (true) Then Exit Function
                //
                string Link = null;
                string IconFilename = null;
                //
                if (core.siteProperties.getBoolean("AllowLoginIcon", true)) {
                    result = result + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">";
                    result = result + "<tr><td align=\"right\">";
                    if (core.doc.sessionContext.isAuthenticatedContentManager(core)) {
                        result = result + "<a href=\"" + genericController.encodeHTML("/" + core.appConfig.adminRoute) + "\" target=\"_blank\">";
                    } else {
                        Link = core.webServer.requestPage + "?" + core.doc.refreshQueryString;
                        Link = genericController.modifyLinkQuery(Link, RequestNameHardCodedPage, HardCodedPageLogin, true);
                        //Link = genericController.modifyLinkQuery(Link, RequestNameInterceptpage, LegacyInterceptPageSNLogin, True)
                        result = result + "<a href=\"" + genericController.encodeHTML(Link) + "\" >";
                    }
                    IconFilename = core.siteProperties.LoginIconFilename;
                    if (genericController.vbLCase(IconFilename.Left(7)) != "/ccLib/") {
                        IconFilename = genericController.getCdnFileLink(core, IconFilename);
                    }
                    result = result + "<img alt=\"Login\" src=\"" + IconFilename + "\" border=\"0\" >";
                    result = result + "</A>";
                    result = result + "</td></tr></table>";
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public string html_GetLegacySiteStyles() {
            string temphtml_GetLegacySiteStyles = null;
            try {
                //
                if (!core.doc.legacySiteStyles_Loaded) {
                    core.doc.legacySiteStyles_Loaded = true;
                    //
                    // compatibility with old sites - if they do not main_Get the default style sheet, put it in here
                    temphtml_GetLegacySiteStyles = ""
                        + "\r<!-- compatibility with legacy framework --><style type=text/css>"
                        + "\r .ccEditWrapper {border:1px dashed #808080;}"
                        + "\r .ccEditWrapperCaption {text-align:left;border-bottom:1px solid #808080;padding:4px;background-color:#40C040;color:black;}"
                        + "\r .ccEditWrapperContent{padding:4px;}"
                        + "\r .ccHintWrapper {border:1px dashed #808080;margin-bottom:10px}"
                        + "\r .ccHintWrapperContent{padding:10px;background-color:#80E080;color:black;}"
                        + "</style>";
                }
                //
                return temphtml_GetLegacySiteStyles;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError13("main_GetLegacySiteStyles")
            return temphtml_GetLegacySiteStyles;
        }
        //
        //====================================================================================================
        //   Wrap the content in a common wrapper if authoring is enabled
        //
        public string getAdminHintWrapper(string Content) {
            string temphtml_GetAdminHintWrapper = null;
            try {
                //
                //If Not (true) Then Exit Function
                //
                temphtml_GetAdminHintWrapper = "";
                if ((core.doc.sessionContext.isEditing("") | core.doc.sessionContext.isAuthenticatedAdmin(core))) {
                    temphtml_GetAdminHintWrapper = temphtml_GetAdminHintWrapper + html_GetLegacySiteStyles();
                    temphtml_GetAdminHintWrapper = temphtml_GetAdminHintWrapper + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccHintWrapper\">"
                            + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccHintWrapperContent\">"
                            + "<b>Administrator</b>"
                            + "<br>"
                            + "<br>" + genericController.encodeText(Content) + "</td></tr></table>"
                        + "</td></tr></table>";
                }

                return temphtml_GetAdminHintWrapper;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError18("main_GetAdminHintWrapper")
            return temphtml_GetAdminHintWrapper;
        }
        //
        //====================================================================================================
        //
        public void enableOutputBuffer(bool BufferOn) {
            try {
                if (core.doc.outputBufferEnabled) {
                    //
                    // ----- once on, can not be turned off Response Object
                    //
                    core.doc.outputBufferEnabled = BufferOn;
                } else {
                    //
                    // ----- StreamBuffer off, allow on and off
                    //
                    core.doc.outputBufferEnabled = BufferOn;
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        // Starts an HTML form for uploads, Should be closed with main_GetUploadFormEnd
        //
        public string formStartMultipart(string ActionQueryString = null) {
            string result = "";
            try {
                if (ActionQueryString == null) {
                    ActionQueryString = core.doc.refreshQueryString;
                }
                string iActionQueryString = genericController.ModifyQueryString(ActionQueryString, RequestNameRequestBinary, true, true);
                result = "<form action=\"" + core.webServer.serverFormActionURL + "?" + iActionQueryString + "\" ENCTYPE=\"MULTIPART/FORM-DATA\" METHOD=\"POST\"  style=\"display: inline;\" >";
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }

        //
        //====================================================================================================
        // Starts an HTML form, Should be closed with PrintFormEnd
        //
        public string formStart(string ActionQueryString = null, string htmlName = "", string htmlId = "", string htmlMethod = "") {
            string temphtml_GetFormStart = null;
            try {
                //
                //If Not (true) Then Exit Function
                //
                int Ptr = 0;
                string ActionQS = null;
                string iMethod = null;
                string Action = null;
                string[] QSParts = null;
                string[] QSNameValues = null;
                string QSName = null;
                string QSValue = null;
                string RefreshHiddens = null;
                //
                if (ActionQueryString == null) {
                    ActionQS = core.doc.refreshQueryString;
                } else {
                    ActionQS = ActionQueryString;
                }
                iMethod = genericController.vbLCase(htmlMethod);
                if (string.IsNullOrEmpty(iMethod)) {
                    iMethod = "post";
                }
                RefreshHiddens = "";
                Action = core.webServer.serverFormActionURL;
                //
                if (!string.IsNullOrEmpty(ActionQS)) {
                    if (iMethod != "main_Get") {
                        //
                        // non-main_Get, put Action QS on end of Action
                        //
                        Action = Action + "?" + ActionQS;
                    } else {
                        //
                        // main_Get method, build hiddens for actionQS
                        //
                        QSParts = ActionQS.Split('&');
                        for (Ptr = 0; Ptr <= QSParts.GetUpperBound(0); Ptr++) {
                            QSNameValues = QSParts[Ptr].Split('=');
                            if (QSNameValues.GetUpperBound(0) == 0) {
                                QSName = genericController.DecodeResponseVariable(QSNameValues[0]);
                            } else {
                                QSName = genericController.DecodeResponseVariable(QSNameValues[0]);
                                QSValue = genericController.DecodeResponseVariable(QSNameValues[1]);
                                RefreshHiddens = RefreshHiddens + "\r<input type=\"hidden\" name=\"" + genericController.encodeHTML(QSName) + "\" value=\"" + genericController.encodeHTML(QSValue) + "\">";
                            }
                        }
                    }
                }
                //
                temphtml_GetFormStart = ""
                    + "\r<form name=\"" + htmlName + "\" id=\"" + htmlId + "\" action=\"" + Action + "\" method=\"" + iMethod + "\" style=\"display: inline;\" >"
                    + RefreshHiddens + "";
                //
                return temphtml_GetFormStart;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError18(MethodName)
            //
            return temphtml_GetFormStart;
        }
        //
        //====================================================================================================
        // Ends an HTML form
        //
        public string formEnd() {
            return "</form>";
        }
        //
        //====================================================================================================
        //
        public string inputText(string TagName, string DefaultValue = "", string Height = "", string Width = "", string Id = "", bool PasswordField = false) {
            return inputText(TagName, DefaultValue, genericController.encodeInteger(Height), genericController.encodeInteger(Width), Id, PasswordField, false);
        }
        //
        //====================================================================================================
        //
        public string inputText(string htmlName, string DefaultValue, int Height, int Width, string HtmlId = "", bool PasswordField = false, bool Disabled = false, string HtmlClass = "") {
            string temphtml_GetFormInputText2 = null;
            try {
                //
                string iDefaultValue = null;
                int iWidth = 0;
                int iHeight = 0;
                string TagID = null;
                string TagDisabled = "";
                //
                if (true) {
                    TagID = "";
                    //
                    iDefaultValue = genericController.encodeHTML(DefaultValue);
                    if (!string.IsNullOrEmpty(HtmlId)) {
                        TagID = TagID + " id=\"" + genericController.encodeEmptyText(HtmlId, "") + "\"";
                    }
                    //
                    if (!string.IsNullOrEmpty(HtmlClass)) {
                        TagID = TagID + " class=\"" + HtmlClass + "\"";
                    }
                    //
                    iWidth = Width;
                    if (iWidth <= 0) {
                        iWidth = core.siteProperties.defaultFormInputWidth;
                    }
                    //
                    iHeight = Height;
                    if (iHeight <= 0) {
                        iHeight = core.siteProperties.defaultFormInputTextHeight;
                    }
                    //
                    if (Disabled) {
                        TagDisabled = " disabled=\"disabled\"";
                    }
                    //
                    if (PasswordField) {
                        temphtml_GetFormInputText2 = "<input TYPE=\"password\" NAME=\"" + htmlName + "\" SIZE=\"" + iWidth + "\" VALUE=\"" + iDefaultValue + "\"" + TagID + TagDisabled + ">";
                    } else if ((iHeight == 1) && (iDefaultValue.IndexOf("\"") == -1)) {
                        temphtml_GetFormInputText2 = "<input TYPE=\"Text\" NAME=\"" + htmlName + "\" SIZE=\"" + iWidth.ToString() + "\" VALUE=\"" + iDefaultValue + "\"" + TagID + TagDisabled + ">";
                    } else {
                        temphtml_GetFormInputText2 = "<textarea NAME=\"" + htmlName + "\" ROWS=\"" + iHeight.ToString() + "\" COLS=\"" + iWidth.ToString() + "\"" + TagID + TagDisabled + ">" + iDefaultValue + "</TEXTAREA>";
                    }
                    core.doc.formInputTextCnt = core.doc.formInputTextCnt + 1;
                }
                //
                return temphtml_GetFormInputText2;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError18("main_GetFormInputText2")
            return temphtml_GetFormInputText2;
        }
        ////
        ////====================================================================================================
        //// main_Get an HTML Form text input (or text area)
        ////
        //public string inputTextExpandable(string TagName, string Value = "", int Rows = 0, string styleWidth = "100%", string Id = "", bool PasswordField = false) {
        //    if (Rows == 0) {
        //        Rows = core.siteProperties.defaultFormInputTextHeight;
        //    }
        //    return html_GetFormInputTextExpandable2(TagName, Value, Rows, styleWidth, Id, PasswordField, false, "");
        //}
        //
        //====================================================================================================
        // main_Get an HTML Form text input (or text area), added disabled case
        //
        public string inputTextExpandable(string TagName, string Value = "", int Rows = 0, string styleWidth = "100%", string Id = "", bool PasswordField = false, bool Disabled = false, string HtmlClass = "") {
            string temphtml_GetFormInputTextExpandable2 = null;
            try {
                //
                //If Not (true) Then Exit Function
                //
                string AttrDisabled = "";
                string Value_Local = null;
                string StyleWidth_Local = null;
                int Rows_Local = 0;
                string IDRoot = null;
                string EditorClosed = null;
                string EditorOpened = null;
                //
                Value_Local = genericController.encodeHTML(Value);
                IDRoot = Id;
                if (string.IsNullOrEmpty(IDRoot)) {
                    IDRoot = "TextArea" + core.doc.formInputTextCnt;
                }
                //
                StyleWidth_Local = styleWidth;
                if (string.IsNullOrEmpty(StyleWidth_Local)) {
                    StyleWidth_Local = "100%";
                }
                //
                Rows_Local = Rows;
                if (Rows_Local == 0) {
                    //
                    // need a default for this -- it should be different from a text, it should be for a textarea -- bnecause it is used differently
                    //
                    //Rows_Local = app.SiteProperty_DefaultFormInputTextHeight
                    if (Rows_Local == 0) {
                        Rows_Local = 10;
                    }
                }
                if (Disabled) {
                    AttrDisabled = " disabled=\"disabled\"";
                }
                //
                EditorClosed = ""
                    + "\r<div class=\"ccTextAreaHead\" ID=\"" + IDRoot + "Head\">"
                    + cr2 + "<a href=\"#\" onClick=\"OpenTextArea('" + IDRoot + "');return false\"><img src=\"/ccLib/images/OpenUpRev1313.gif\" width=13 height=13 border=0>&nbsp;Full Screen</a>"
                    + "\r</div>"
                    + "\r<div class=\"ccTextArea\">"
                    + cr2 + "<textarea ID=\"" + IDRoot + "\" NAME=\"" + TagName + "\" ROWS=\"" + Rows_Local + "\" Style=\"width:" + StyleWidth_Local + ";\"" + AttrDisabled + " onkeydown=\"return cj.encodeTextAreaKey(this, event);\">" + Value_Local + "</TEXTAREA>"
                    + "\r</div>"
                    + "";
                //
                EditorOpened = ""
                    + "\r<div class=\"ccTextAreaHeCursorTypeEnum.ADOPENed\" style=\"display:none;\" ID=\"" + IDRoot + "HeCursorTypeEnum.ADOPENed\">"
                    + "\r<a href=\"#\" onClick=\"CloseTextArea('" + IDRoot + "');return false\"><img src=\"/ccLib/images/OpenDownRev1313.gif\" width=13 height=13 border=0>&nbsp;Full Screen</a>"
                    + cr2 + "</div>"
                    + "\r<textarea class=\"ccTextAreaOpened\" style=\"display:none;\" ID=\"" + IDRoot + "Opened\" NAME=\"" + IDRoot + "Opened\"" + AttrDisabled + " onkeydown=\"return cj.encodeTextAreaKey(this, event);\"></TEXTAREA>";
                //
                temphtml_GetFormInputTextExpandable2 = ""
                    + "<div class=\"" + HtmlClass + "\">"
                    + genericController.htmlIndent(EditorClosed) + genericController.htmlIndent(EditorOpened) + "</div>";
                core.doc.formInputTextCnt = core.doc.formInputTextCnt + 1;
                return temphtml_GetFormInputTextExpandable2;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            core.handleException(new Exception("Unexpected exception"));
            //
            return temphtml_GetFormInputTextExpandable2;
        }
        //
        //====================================================================================================
        //
        public string inputDate(string TagName, string DefaultValue = "", string Width = "", string Id = "") {
            string result = "";
            try {
                string DateString = "";
                DateTime DateValue = default(DateTime);
                string iDefaultValue = null;
                int iWidth = 0;
                string iTagName = null;
                string TagID = null;
                string CalendarObjName = null;
                string AnchorName = null;
                //
                iTagName = genericController.encodeText(TagName);
                iDefaultValue = genericController.encodeEmptyText(DefaultValue, "");
                if ((iDefaultValue == "0") || (iDefaultValue == "12:00:00 AM")) {
                    iDefaultValue = "";
                } else {
                    iDefaultValue = genericController.encodeHTML(iDefaultValue);
                }
                if (genericController.encodeEmptyText(Id, "") != "") {
                    TagID = " ID=\"" + genericController.encodeEmptyText(Id, "") + "\"";
                }
                //
                iWidth = genericController.encodeEmptyInteger(Width, 20);
                if (iWidth == 0) {
                    iWidth = 20;
                }
                //
                CalendarObjName = "Cal" + core.doc.inputDateCnt;
                AnchorName = "ACal" + core.doc.inputDateCnt;

                if (core.doc.inputDateCnt == 0) {
                    addScriptLinkSrc("/ccLib/mktree/CalendarPopup.js", "Calendar Popup");
                    addScriptCode("var cal=new CalendarPopup();cal.showNavigationDropdowns();", "Calendar Popup");
                }

                if (dateController.IsDate(iDefaultValue)) {
                    DateValue = genericController.encodeDate(iDefaultValue);
                    if (DateValue.Month < 10) {
                        DateString = DateString + "0";
                    }
                    DateString = DateString + DateValue.Month + "/";
                    if (DateValue.Day < 10) {
                        DateString = DateString + "0";
                    }
                    DateString = DateString + DateValue.Day + "/" + DateValue.Year;
                }


                result = result + "\r\n<input TYPE=\"text\" NAME=\"" + iTagName + "\" ID=\"" + iTagName + "\" VALUE=\"" + iDefaultValue + "\"  SIZE=\"" + iWidth + "\">"
                + "\r\n<a HREF=\"#\" Onclick = \"cal.select(document.getElementById('" + iTagName + "'),'" + AnchorName + "','MM/dd/yyyy','" + DateString + "'); return false;\" NAME=\"" + AnchorName + "\" ID=\"" + AnchorName + "\"><img title=\"Select a date\" alt=\"Select a date\" src=\"/ccLib/images/table.jpg\" width=12 height=10 border=0></A>"
                + "\r\n";

                core.doc.inputDateCnt = core.doc.inputDateCnt + 1;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        // main_Get an HTML Form file upload input
        //
        public string inputFile(string TagName, string htmlId = "", string HtmlClass = "") {
            //
            return "<input TYPE=\"file\" name=\"" + TagName + "\" id=\"" + htmlId + "\" class=\"" + HtmlClass + "\">";
            //
        }
        ////
        ////====================================================================================================
        //// main_Get an HTML Form file upload input
        ////
        //public string inputFile(string TagName, string htmlId = "") {
        //    //
        //    return inputFile(TagName, htmlId);
        //    //
        //}
        //
        //====================================================================================================
        // main_Get an HTML Form input
        //
        public string inputRadio(string TagName, string TagValue, string CurrentValue, string htmlId = "") {
            string temphtml_GetFormInputRadioBox = null;
            try {
                string iTagName = null;
                string iTagValue = null;
                string iCurrentValue = null;
                string ihtmlId = null;
                string TagID = "";
                //
                iTagName = genericController.encodeText(TagName);
                iTagValue = genericController.encodeText(TagValue);
                iCurrentValue = genericController.encodeText(CurrentValue);
                ihtmlId = genericController.encodeEmptyText(htmlId, "");
                if (!string.IsNullOrEmpty(ihtmlId)) {
                    TagID = " ID=\"" + ihtmlId + "\"";
                }
                //
                if (iTagValue == iCurrentValue) {
                    temphtml_GetFormInputRadioBox = "<input TYPE=\"Radio\" NAME=\"" + iTagName + "\" VALUE=\"" + iTagValue + "\" checked" + TagID + ">";
                } else {
                    temphtml_GetFormInputRadioBox = "<input TYPE=\"Radio\" NAME=\"" + iTagName + "\" VALUE=\"" + iTagValue + "\"" + TagID + ">";
                }
                //
                return temphtml_GetFormInputRadioBox;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError18(MethodName)
            //
            return temphtml_GetFormInputRadioBox;
        }
        //
        //====================================================================================================
        //
        public string inputCheckbox(string TagName, string DefaultValue) {
            return inputCheckbox(genericController.encodeText(TagName), genericController.encodeBoolean(DefaultValue));
        }
        //
        //====================================================================================================
        //
        public string inputCheckbox(string TagName, bool DefaultValue = false, string HtmlId = "", bool Disabled = false, string HtmlClass = "") {
            string temphtml_GetFormInputCheckBox2 = null;
            try {
                //
                temphtml_GetFormInputCheckBox2 = "<input type=\"CheckBox\" NAME=\"" + TagName + "\" value=\"1\"";
                if (!string.IsNullOrEmpty(HtmlId)) {
                    temphtml_GetFormInputCheckBox2 = temphtml_GetFormInputCheckBox2 + " id=\"" + HtmlId + "\"";
                }
                if (!string.IsNullOrEmpty(HtmlClass)) {
                    temphtml_GetFormInputCheckBox2 = temphtml_GetFormInputCheckBox2 + " class=\"" + HtmlClass + "\"";
                }
                if (DefaultValue) {
                    temphtml_GetFormInputCheckBox2 = temphtml_GetFormInputCheckBox2 + " checked=\"checked\"";
                }
                if (Disabled) {
                    temphtml_GetFormInputCheckBox2 = temphtml_GetFormInputCheckBox2 + " disabled=\"disabled\"";
                }
                return temphtml_GetFormInputCheckBox2 + ">";
                //
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError18("main_GetFormInputCheckBox2")
            return temphtml_GetFormInputCheckBox2;
        }
        //
        //====================================================================================================
        //   Create a List of Checkboxes based on a contentname and a list of IDs that should be checked
        //      For instance, list out a checklist of all public groups, with the ones checked that this member belongs to
        //       PrimaryContentName = "People"
        //       PrimaryRecordID = MemberID
        //       SecondaryContentName = "Groups"
        //       SecondaryContentSelectCriteria = "ccGroups.PublicJoin<>0"
        //       RulesContentName = "Member Rules"
        //       RulesPrimaryFieldName = "MemberID"
        //       RulesSecondaryFieldName = "GroupID"
        //========================================================================
        //
        public string html_GetFormInputCheckListByIDList(string TagName, string SecondaryContentName, string CheckedIDList, string CaptionFieldName = "", bool readOnlyField = false) {
            string Result = "";
            try {
                string SQL = null;
                int CS = 0;
                int GroupCount = 0;
                string SecondaryTablename = null;
                int SecondaryContentID = 0;
                string iCaptionFieldName = null;
                string GroupName = null;
                string GroupCaption = null;
                bool CanSeeHiddenFields = false;
                Models.Complex.cdefModel SecondaryCDef = null;
                string ContentIDList = "";
                bool Found = false;
                int RecordID = 0;
                string SingularPrefix = null;
                //
                iCaptionFieldName = genericController.encodeEmptyText(CaptionFieldName, "name");
                //
                // ----- Gather all the SecondaryContent that associates to the PrimaryContent
                //
                SecondaryCDef = Models.Complex.cdefModel.getCdef(core, SecondaryContentName);
                SecondaryTablename = SecondaryCDef.ContentTableName;
                SecondaryContentID = SecondaryCDef.Id;
                SecondaryCDef.get_childIdList(core).Add(SecondaryContentID);
                SingularPrefix = genericController.GetSingular(SecondaryContentName) + "&nbsp;";
                //
                // ----- Gather all the records, sorted by ContentName
                //
                SQL = "SELECT " + SecondaryTablename + ".ID AS ID, ccContent.Name AS SectionName, " + SecondaryTablename + "." + iCaptionFieldName + " AS GroupCaption, " + SecondaryTablename + ".name AS GroupName, " + SecondaryTablename + ".SortOrder"
                + " FROM " + SecondaryTablename + " LEFT JOIN ccContent ON " + SecondaryTablename + ".ContentControlID = ccContent.ID"
                + " Where (" + SecondaryTablename + ".Active<>" + SQLFalse + ")"
                + " And (ccContent.Active<>" + SQLFalse + ")"
                + " And (" + SecondaryTablename + ".ContentControlID IN (" + ContentIDList + "))";
                SQL += ""
                    + " GROUP BY " + SecondaryTablename + ".ID, ccContent.Name, " + SecondaryTablename + "." + iCaptionFieldName + ", " + SecondaryTablename + ".name, " + SecondaryTablename + ".SortOrder"
                    + " ORDER BY ccContent.Name, " + SecondaryTablename + "." + iCaptionFieldName;
                CS = core.db.csOpenSql(SQL);
                if (core.db.csOk(CS)) {
                    //SectionName = "";
                    GroupCount = 0;
                    CanSeeHiddenFields = core.doc.sessionContext.isAuthenticatedDeveloper(core);
                    while (core.db.csOk(CS)) {
                        GroupName = core.db.csGetText(CS, "GroupName");
                        if ((GroupName.Left(1) != "_") || CanSeeHiddenFields) {
                            RecordID = core.db.csGetInteger(CS, "ID");
                            GroupCaption = core.db.csGetText(CS, "GroupCaption");
                            if (string.IsNullOrEmpty(GroupCaption)) {
                                GroupCaption = GroupName;
                            }
                            if (string.IsNullOrEmpty(GroupCaption)) {
                                GroupCaption = SingularPrefix + RecordID;
                            }
                            if (GroupCount != 0) {
                                // leave this between checkboxes - it is searched in the admin page
                                Result = Result + "<br>\r\n";
                            }
                            if (genericController.IsInDelimitedString(CheckedIDList, RecordID.ToString(), ",")) {
                                Found = true;
                            } else {
                                Found = false;
                            }
                            // must leave the first hidden with the value in this form - it is searched in the admin pge
                            Result = Result + "<input type=hidden name=\"" + TagName + "." + GroupCount + ".ID\" value=" + RecordID + ">";
                            if (readOnlyField && !Found) {
                                Result = Result + "<input type=checkbox disabled>";
                            } else if (readOnlyField) {
                                Result = Result + "<input type=checkbox disabled checked>";
                                Result = Result + "<input type=\"hidden\" name=\"" + TagName + "." + GroupCount + ".ID\" value=" + RecordID + ">";
                            } else if (Found) {
                                Result = Result + "<input type=checkbox name=\"" + TagName + "." + GroupCount + "\" checked>";
                            } else {
                                Result = Result + "<input type=checkbox name=\"" + TagName + "." + GroupCount + "\">";
                            }
                            Result = Result + SpanClassAdminNormal + GroupCaption;
                            GroupCount = GroupCount + 1;
                        }
                        core.db.csGoNext(CS);
                    }
                    Result = Result + "<input type=\"hidden\" name=\"" + TagName + ".RowCount\" value=\"" + GroupCount + "\">\r\n";
                }
                core.db.csClose(ref CS);
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return Result;
        }
        //
        //====================================================================================================
        //
        public string inputCs(int CSPointer, string ContentName, string FieldName, int Height = 1, int Width = 40, string htmlId = "") {
            string returnResult = "";
            try {
                bool IsEmptyList = false;
                string FieldCaption = null;
                string FieldValueVariant = "";
                string FieldValueText = null;
                int FieldValueInteger = 0;
                int fieldTypeId = 0;
                bool FieldReadOnly = false;
                bool FieldPassword = false;
                bool fieldFound = false;
                int FieldLookupContentID = 0;
                int FieldMemberSelectGroupID = 0;
                string FieldLookupContentName = null;
                Models.Complex.cdefModel Contentdefinition = null;
                bool FieldHTMLContent = false;
                int CSLookup = 0;
                string FieldLookupList = "";
                //
                if (true) {
                    fieldFound = false;
                    Contentdefinition = Models.Complex.cdefModel.getCdef(core, ContentName);
                    foreach (KeyValuePair<string, Models.Complex.cdefFieldModel> keyValuePair in Contentdefinition.fields) {
                        Models.Complex.cdefFieldModel field = keyValuePair.Value;
                        if (genericController.vbUCase(field.nameLc) == genericController.vbUCase(FieldName)) {
                            FieldValueVariant = field.defaultValue;
                            fieldTypeId = field.fieldTypeId;
                            FieldReadOnly = field.readOnly;
                            FieldCaption = field.caption;
                            FieldPassword = field.password;
                            FieldHTMLContent = field.htmlContent;
                            FieldLookupContentID = field.lookupContentID;
                            FieldLookupList = field.lookupList;
                            FieldMemberSelectGroupID = field.memberSelectGroupId_get(core);
                            fieldFound = true;
                        }
                    }
                    if (!fieldFound) {
                        core.handleException(new Exception("Field [" + FieldName + "] was not found in Content Definition [" + ContentName + "]"));
                    } else {
                        //
                        // main_Get the current value if the record was found
                        //
                        if (core.db.csOk(CSPointer)) {
                            FieldValueVariant = core.db.cs_getValue(CSPointer, FieldName);
                        }
                        //
                        if (FieldPassword) {
                            //
                            // Handle Password Fields
                            //
                            FieldValueText = genericController.encodeText(FieldValueVariant);
                            returnResult = inputText(FieldName, FieldValueText, Height, Width, "", true);
                        } else {
                            //
                            // Non Password field by fieldtype
                            //
                            switch (fieldTypeId) {
                                //
                                //
                                //
                                case FieldTypeIdHTML:
                                    FieldValueText = genericController.encodeText(FieldValueVariant);
                                    if (FieldReadOnly) {
                                        returnResult = FieldValueText;
                                    } else {
                                        returnResult = getFormInputHTML(FieldName, FieldValueText, "", Width.ToString());
                                    }
                                    //
                                    // html files, read from cdnFiles and use html editor
                                    //
                                    break;
                                case FieldTypeIdFileHTML:
                                    FieldValueText = genericController.encodeText(FieldValueVariant);
                                    if (!string.IsNullOrEmpty(FieldValueText)) {
                                        FieldValueText = core.cdnFiles.readFile(FieldValueText);
                                    }
                                    if (FieldReadOnly) {
                                        returnResult = FieldValueText;
                                    } else {
                                        //Height = encodeEmptyInteger(Height, 4)
                                        returnResult = getFormInputHTML(FieldName, FieldValueText, "", Width.ToString());
                                    }
                                    //
                                    // text cdnFiles files, read from cdnFiles and use text editor
                                    //
                                    break;
                                case FieldTypeIdFileText:
                                    FieldValueText = genericController.encodeText(FieldValueVariant);
                                    if (!string.IsNullOrEmpty(FieldValueText)) {
                                        FieldValueText = core.cdnFiles.readFile(FieldValueText);
                                    }
                                    if (FieldReadOnly) {
                                        returnResult = FieldValueText;
                                    } else {
                                        //Height = encodeEmptyInteger(Height, 4)
                                        returnResult = inputText(FieldName, FieldValueText, Height, Width);
                                    }
                                    //
                                    // text public files, read from core.cdnFiles and use text editor
                                    //
                                    break;
                                case FieldTypeIdFileCSS:
                                case FieldTypeIdFileXML:
                                case FieldTypeIdFileJavascript:
                                    FieldValueText = genericController.encodeText(FieldValueVariant);
                                    if (!string.IsNullOrEmpty(FieldValueText)) {
                                        FieldValueText = core.cdnFiles.readFile(FieldValueText);
                                    }
                                    if (FieldReadOnly) {
                                        returnResult = FieldValueText;
                                    } else {
                                        //Height = encodeEmptyInteger(Height, 4)
                                        returnResult = inputText(FieldName, FieldValueText, Height, Width);
                                    }
                                    //
                                    //
                                    //
                                    break;
                                case FieldTypeIdBoolean:
                                    if (FieldReadOnly) {
                                        returnResult = genericController.encodeText(genericController.encodeBoolean(FieldValueVariant));
                                    } else {
                                        returnResult = inputCheckbox(FieldName, genericController.encodeBoolean(FieldValueVariant));
                                    }
                                    //
                                    //
                                    //
                                    break;
                                case FieldTypeIdAutoIdIncrement:
                                    returnResult = genericController.encodeText(genericController.encodeNumber(FieldValueVariant));
                                    //
                                    //
                                    //
                                    break;
                                case FieldTypeIdFloat:
                                case FieldTypeIdCurrency:
                                case FieldTypeIdInteger:
                                    FieldValueVariant = genericController.encodeNumber(FieldValueVariant).ToString();
                                    if (FieldReadOnly) {
                                        returnResult = genericController.encodeText(FieldValueVariant);
                                    } else {
                                        returnResult = inputText(FieldName, genericController.encodeText(FieldValueVariant), Height, Width);
                                    }
                                    //
                                    //
                                    //
                                    break;
                                case FieldTypeIdFile:
                                    FieldValueText = genericController.encodeText(FieldValueVariant);
                                    if (FieldReadOnly) {
                                        returnResult = FieldValueText;
                                    } else {
                                        returnResult = FieldValueText + "<br>change: " + inputFile(FieldName, genericController.encodeText(FieldValueVariant));
                                    }
                                    //
                                    //
                                    //
                                    break;
                                case FieldTypeIdFileImage:
                                    FieldValueText = genericController.encodeText(FieldValueVariant);
                                    if (FieldReadOnly) {
                                        returnResult = FieldValueText;
                                    } else {
                                        returnResult = "<img src=\"" + genericController.getCdnFileLink(core, FieldValueText) + "\"><br>change: " + inputFile(FieldName, genericController.encodeText(FieldValueVariant));
                                    }
                                    //
                                    //
                                    //
                                    break;
                                case FieldTypeIdLookup:
                                    FieldValueInteger = genericController.encodeInteger(FieldValueVariant);
                                    FieldLookupContentName = Models.Complex.cdefModel.getContentNameByID(core, FieldLookupContentID);
                                    if (!string.IsNullOrEmpty(FieldLookupContentName)) {
                                        //
                                        // Lookup into Content
                                        //
                                        if (FieldReadOnly) {
                                            CSPointer = core.db.cs_open2(FieldLookupContentName, FieldValueInteger);
                                            if (core.db.csOk(CSLookup)) {
                                                returnResult = csController.getTextEncoded(core, CSLookup, "name");
                                            }
                                            core.db.csClose(ref CSLookup);
                                        } else {
                                            returnResult = selectFromContent(FieldName, FieldValueInteger, FieldLookupContentName, "", "", "", ref IsEmptyList);
                                        }
                                    } else if (!string.IsNullOrEmpty(FieldLookupList)) {
                                        //
                                        // Lookup into LookupList
                                        //
                                        returnResult = selectFromList(FieldName, FieldValueInteger, FieldLookupList, "", "");
                                    } else {
                                        //
                                        // Just call it text
                                        //
                                        returnResult = inputText(FieldName, FieldValueInteger.ToString(), Height, Width);
                                    }
                                    //
                                    //
                                    //
                                    break;
                                case FieldTypeIdMemberSelect:
                                    FieldValueInteger = genericController.encodeInteger(FieldValueVariant);
                                    returnResult = selectUserFromGroup(FieldName, FieldValueInteger, FieldMemberSelectGroupID);
                                    //
                                    //
                                    //
                                    break;
                                default:
                                    FieldValueText = genericController.encodeText(FieldValueVariant);
                                    if (FieldReadOnly) {
                                        returnResult = FieldValueText;
                                    } else {
                                        if (FieldHTMLContent) {
                                            returnResult = getFormInputHTML(FieldName, FieldValueText, Height.ToString(), Width.ToString(), FieldReadOnly, false);
                                            //main_GetFormInputCS = main_GetFormInputActiveContent(fieldname, FieldValueText, height, width)
                                        } else {
                                            returnResult = inputText(FieldName, FieldValueText, Height, Width);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnResult;
        }
        ////
        ////====================================================================================================
        //// Print an HTML Form Button element named BUTTON
        ////
        //public string inputButton(string ButtonLabel, string Name = "", string htmlId = "", string OnClick = "") {
        //    return inputButton(ButtonLabel, Name, htmlId, OnClick, false);
        //}
        //
        //====================================================================================================
        // Print an HTML Form Button element named BUTTON
        //
        public string button(string ButtonLabel, string Name = "button", string htmlId = "", string OnClick = "", bool Disabled = false) {
            string s = null;
            try {
                s = "<input TYPE=\"SUBMIT\""
                    + " NAME=\"" + genericController.encodeEmptyText(Name, "button") + "\""
                    + " VALUE=\"" + genericController.encodeText(ButtonLabel) + "\""
                    + " OnClick=\"" + genericController.encodeEmptyText(OnClick, "") + "\""
                    + " ID=\"" + genericController.encodeEmptyText(htmlId, "") + "\"";
                if (Disabled) {
                    s = s + " disabled=\"disabled\"";
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return s + ">";
        }
        //
        //====================================================================================================
        // main_Gets a value in a hidden form field, Handles name and value encoding
        //
        public string inputHidden(string TagName, string TagValue, string htmlId = "") {
            string result = "";
            try {
                result = "\r<input type=\"hidden\" NAME=\"" + genericController.encodeHTML(genericController.encodeText(TagName)) + "\"";
                //
                string iTagValue = genericController.encodeHTML(genericController.encodeText(TagValue));
                if (!string.IsNullOrEmpty(iTagValue)) {
                    result = result + " VALUE=\"" + iTagValue + "\"";
                }
                //
                string ihtmlId = genericController.encodeText(htmlId);
                if (!string.IsNullOrEmpty(ihtmlId)) {
                    result = result + " ID=\"" + genericController.encodeHTML(ihtmlId) + "\"";
                }
                //
                result = result + ">";
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public string inputHidden(string TagName, bool TagValue, string htmlId = "") {
            return inputHidden(TagName, TagValue.ToString(), htmlId);
        }
        //
        //====================================================================================================
        //
        public string inputHidden(string TagName, int TagValue, string htmlId = "") {
            return inputHidden(TagName, TagValue.ToString(), htmlId);
        }
        //
        //====================================================================================================
        // Popup a separate window with the contents of a file
        //
        public string javascriptOpenWindow(string URI, string WindowWidth = "", string WindowHeight = "", string WindowScrollBars = "", bool WindowResizable = true, string WindowName = "_blank") {
            string temphtml_GetWindowOpenJScript = null;
            try {
                //
                //If Not (true) Then Exit Function
                //
                string Delimiter = null;
                //
                temphtml_GetWindowOpenJScript = "";
                WindowName = genericController.encodeEmptyText(WindowName, "_blank");
                //
                // Added addl options from huhcorp.com sample
                //
                temphtml_GetWindowOpenJScript = temphtml_GetWindowOpenJScript + "window.open('" + URI + "', '" + WindowName + "'";
                temphtml_GetWindowOpenJScript = temphtml_GetWindowOpenJScript + ",'menubar=no,toolbar=no,location=no,status=no";
                Delimiter = ",";
                if (!genericController.isMissing(WindowWidth)) {
                    if (!string.IsNullOrEmpty(WindowWidth)) {
                        temphtml_GetWindowOpenJScript = temphtml_GetWindowOpenJScript + Delimiter + "width=" + WindowWidth;
                        Delimiter = ",";
                    }
                }
                if (!genericController.isMissing(WindowHeight)) {
                    if (!string.IsNullOrEmpty(WindowHeight)) {
                        temphtml_GetWindowOpenJScript = temphtml_GetWindowOpenJScript + Delimiter + "height=" + WindowHeight;
                        Delimiter = ",";
                    }
                }
                if (!genericController.isMissing(WindowScrollBars)) {
                    if (!string.IsNullOrEmpty(WindowScrollBars)) {
                        temphtml_GetWindowOpenJScript = temphtml_GetWindowOpenJScript + Delimiter + "scrollbars=" + WindowScrollBars;
                        Delimiter = ",";
                    }
                }
                if (WindowResizable) {
                    temphtml_GetWindowOpenJScript = temphtml_GetWindowOpenJScript + Delimiter + "resizable";
                    Delimiter = ",";
                }
                return temphtml_GetWindowOpenJScript + "')";
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError18(MethodName)
            //
            return temphtml_GetWindowOpenJScript;
        }
        //
        //====================================================================================================
        // Popup a separate window with the contents of a file
        //
        public string javascriptOpenModal(string URI, string WindowWidth = "", string WindowHeight = "", bool WindowScrollBars = false, bool WindowResizable = false, string WindowName = "") {
            string temphtml_GetWindowDialogJScript = null;
            try {
                string iWindowName = null;
                //
                iWindowName = genericController.encodeEmptyText(WindowName, "_blank");
                //
                // Added addl options from huhcorp.com sample
                //
                temphtml_GetWindowDialogJScript = "";
                temphtml_GetWindowDialogJScript = temphtml_GetWindowDialogJScript + "showModalDialog('" + URI + "', '" + iWindowName + "'";
                temphtml_GetWindowDialogJScript = temphtml_GetWindowDialogJScript + ",'status:false";
                if (!genericController.isMissing(WindowWidth)) {
                    if (!string.IsNullOrEmpty(WindowWidth)) {
                        temphtml_GetWindowDialogJScript = temphtml_GetWindowDialogJScript + ";dialogWidth:" + WindowWidth + "px";
                    }
                }
                if (!genericController.isMissing(WindowHeight)) {
                    if (!string.IsNullOrEmpty(WindowHeight)) {
                        temphtml_GetWindowDialogJScript = temphtml_GetWindowDialogJScript + ";dialogHeight:" + WindowHeight + "px";
                    }
                }
                if (WindowScrollBars) {
                    temphtml_GetWindowDialogJScript = temphtml_GetWindowDialogJScript + ";scroll:yes";
                }
                if (WindowResizable) {
                    temphtml_GetWindowDialogJScript = temphtml_GetWindowDialogJScript + ";resizable:yes";
                }
                return temphtml_GetWindowDialogJScript + "')";
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError18(MethodName)
            //
            return temphtml_GetWindowDialogJScript;
        }
        //
        //====================================================================================================
        //
        public void javascriptAddEvent(string HtmlId, string DOMEvent, string Javascript) {
            string JSCodeAsString = Javascript;
            JSCodeAsString = genericController.vbReplace(JSCodeAsString, "'", "'+\"'\"+'");
            JSCodeAsString = genericController.vbReplace(JSCodeAsString, "\r\n", "\\n");
            JSCodeAsString = genericController.vbReplace(JSCodeAsString, "\r", "\\n");
            JSCodeAsString = genericController.vbReplace(JSCodeAsString, "\n", "\\n");
            JSCodeAsString = "'" + JSCodeAsString + "'";
            addScriptCode_onLoad("cj.addListener(document.getElementById('" + HtmlId + "'),'" + DOMEvent + "',function(){eval(" + JSCodeAsString + ")})", "");
        }
        //
        //====================================================================================================
        //
        public string inputField(string ContentName, string FieldName, string htmlName = "", string HtmlValue = "", string HtmlClass = "", string HtmlId = "", string HtmlStyle = "", int ManyToManySourceRecordID = 0) {
            string result = "";
            try {
                bool IgnoreBoolean = false;
                string LookupContentName = null;
                int fieldType = 0;
                string InputName = null;
                int GroupID = 0;
                Models.Complex.cdefModel CDef = null;
                string MTMContent0 = null;
                string MTMContent1 = null;
                string MTMRuleContent = null;
                string MTMRuleField0 = null;
                string MTMRuleField1 = null;
                //
                InputName = htmlName;
                if (string.IsNullOrEmpty(InputName)) {
                    InputName = FieldName;
                }
                //
                fieldType = genericController.encodeInteger(Models.Complex.cdefModel.GetContentFieldProperty(core, ContentName, FieldName, "type"));
                switch (fieldType) {
                    case FieldTypeIdBoolean: {
                            //
                            //
                            //
                            result = inputCheckbox(InputName, genericController.encodeBoolean(HtmlValue) == true, HtmlId, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdFileCSS: {
                            //
                            //
                            //
                            result = inputTextExpandable(InputName, HtmlValue, 0, "100%", HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdCurrency: {
                            //
                            //
                            //
                            result = inputText(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdDate: {
                            //
                            //
                            //
                            result = inputDate(InputName, HtmlValue, "", HtmlId);
                            if (!string.IsNullOrEmpty(HtmlClass)) {
                                result = genericController.vbReplace(result, ">", " class=\"" + HtmlClass + "\">");
                            }
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdFile: {
                            //
                            //
                            //
                            if (string.IsNullOrEmpty(HtmlValue)) {
                                result = inputFile(InputName, HtmlId, HtmlClass);
                            } else {

                                string FieldValuefilename = "";
                                string FieldValuePath = "";
                                core.cdnFiles.splitPathFilename(HtmlValue, ref FieldValuePath, ref FieldValuefilename);
                                result = result + "<a href=\"http://" + genericController.EncodeURL(core.webServer.requestDomain + genericController.getCdnFileLink(core, HtmlValue)) + "\" target=\"_blank\">" + SpanClassAdminSmall + "[" + FieldValuefilename + "]</A>";
                                result = result + "&nbsp;&nbsp;&nbsp;Delete:&nbsp;" + inputCheckbox(InputName + ".Delete", false);
                                result = result + "&nbsp;&nbsp;&nbsp;Change:&nbsp;" + inputFile(InputName, HtmlId, HtmlClass);
                            }
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdFloat: {
                            //
                            //
                            //
                            result = inputText(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdFileImage: {
                            //
                            //
                            //
                            if (string.IsNullOrEmpty(HtmlValue)) {
                                result = inputFile(InputName, HtmlId, HtmlClass);
                            } else {
                                string FieldValuefilename = "";
                                string FieldValuePath = "";
                                core.cdnFiles.splitPathFilename(HtmlValue, ref FieldValuePath, ref FieldValuefilename);
                                result = result + "<a href=\"http://" + genericController.EncodeURL(core.webServer.requestDomain + genericController.getCdnFileLink(core, HtmlValue)) + "\" target=\"_blank\">" + SpanClassAdminSmall + "[" + FieldValuefilename + "]</A>";
                                result = result + "&nbsp;&nbsp;&nbsp;Delete:&nbsp;" + inputCheckbox(InputName + ".Delete", false);
                                result = result + "&nbsp;&nbsp;&nbsp;Change:&nbsp;" + inputFile(InputName, HtmlId, HtmlClass);
                            }
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdInteger: {
                            //
                            //
                            //
                            result = inputText(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdFileJavascript: {
                            //
                            //
                            //
                            result = inputTextExpandable(InputName, HtmlValue, 0, "100%", HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdLink: {
                            //
                            //
                            //
                            result = inputText(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdLookup: {
                            //
                            //
                            //
                            CDef = Models.Complex.cdefModel.getCdef(core, ContentName);
                            LookupContentName = "";
                            foreach (KeyValuePair<string, Models.Complex.cdefFieldModel> keyValuePair in CDef.fields) {
                                Models.Complex.cdefFieldModel field = keyValuePair.Value;
                                if (genericController.vbUCase(field.nameLc) == genericController.vbUCase(FieldName)) {
                                    if (field.lookupContentID != 0) {
                                        LookupContentName = genericController.encodeText(Models.Complex.cdefModel.getContentNameByID(core, field.lookupContentID));
                                    }
                                    if (!string.IsNullOrEmpty(LookupContentName)) {
                                        result = selectFromContent(InputName, genericController.encodeInteger(HtmlValue), LookupContentName, "", "Select One", HtmlId, ref IgnoreBoolean, HtmlClass);
                                    } else if (field.lookupList != "") {
                                        result = selectFromList(InputName, genericController.encodeInteger(HtmlValue), field.lookupList, "Select One", HtmlId, HtmlClass);
                                    }
                                    if (!string.IsNullOrEmpty(HtmlStyle)) {
                                        result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    case FieldTypeIdManyToMany: {
                            //
                            //
                            //
                            CDef = Models.Complex.cdefModel.getCdef(core, ContentName);
                            var tempVar = CDef.fields[FieldName.ToLower()];
                            MTMContent0 = Models.Complex.cdefModel.getContentNameByID(core, tempVar.contentId);
                            MTMContent1 = Models.Complex.cdefModel.getContentNameByID(core, tempVar.manyToManyContentID);
                            MTMRuleContent = Models.Complex.cdefModel.getContentNameByID(core, tempVar.manyToManyRuleContentID);
                            MTMRuleField0 = tempVar.ManyToManyRulePrimaryField;
                            MTMRuleField1 = tempVar.ManyToManyRuleSecondaryField;
                            result = getCheckList(InputName, MTMContent0, ManyToManySourceRecordID, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, "", "", false);
                            //result = getInputCheckListCategories(InputName, MTMContent0, ManyToManySourceRecordID, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, , , False, MTMContent1, HtmlValue)
                            break;
                        }
                    case FieldTypeIdMemberSelect: {
                            //
                            //
                            //
                            GroupID = genericController.encodeInteger(Models.Complex.cdefModel.GetContentFieldProperty(core, ContentName, FieldName, "memberselectgroupid"));
                            result = selectUserFromGroup(InputName, genericController.encodeInteger(HtmlValue), GroupID, "", "", HtmlId);
                            if (!string.IsNullOrEmpty(HtmlClass)) {
                                result = genericController.vbReplace(result, ">", " class=\"" + HtmlClass + "\">");
                            }
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdResourceLink: {
                            //
                            //
                            //
                            result = inputText(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdText: {
                            //
                            //
                            //
                            result = inputText(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdLongText:
                    case FieldTypeIdFileText: {
                            //
                            //
                            //
                            result = inputTextExpandable(InputName, HtmlValue, 0, "100%", HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdFileXML: {
                            //
                            //
                            //
                            result = inputTextExpandable(InputName, HtmlValue, 0, "100%", HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdHTML:
                    case FieldTypeIdFileHTML: {
                            //
                            //
                            //
                            result = getFormInputHTML(InputName, HtmlValue);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            if (!string.IsNullOrEmpty(HtmlClass)) {
                                result = genericController.vbReplace(result, ">", " class=\"" + HtmlClass + "\">");
                            }
                            break;
                        }
                    default: {
                            //
                            // unsupported field type
                            //
                            break;
                        }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public string JSONeditorAddonList(csv_contentTypeEnum ContentType) {
            string result = "";
            try {
                string AddonName = null;
                string LastAddonName = "";
                int CSAddons = 0;
                string DefaultAddonOption_String = null;
                bool UseAjaxDefaultAddonOptions = false;
                int PtrTest = 0;
                string s = null;
                int IconWidth = 0;
                int IconHeight = 0;
                int IconSprites = 0;
                bool IsInline = false;
                string AddonGuid = null;
                string IconIDControlString = null;
                string IconImg = null;
                string AddonContentName = null;
                string ObjectProgramID2 = null;
                int LoopPtr = 0;
                string FieldCaption = null;
                string SelectList = null;
                string IconFilename = null;
                string FieldName = null;
                string ArgumentList = null;
                keyPtrController Index = null;
                string[] Items = null;
                int ItemsSize = 0;
                int ItemsCnt = 0;
                int ItemsPtr = 0;
                string Criteria = null;
                int CSLists = 0;
                string FieldList = null;
                string cacheKey;
                //
                // can not save this because there are multiple main_versions
                //
                cacheKey = "editorAddonList:" + ContentType;
                result = core.docProperties.getText(cacheKey);
                if (string.IsNullOrEmpty(result)) {
                    //
                    // ----- AC Tags, Would like to replace these with Add-ons eventually
                    //
                    ItemsSize = 100;
                    Items = new string[101];
                    ItemsCnt = 0;
                    Index = new keyPtrController();
                    //Set main_cmc = main_cs_getv()
                    //
                    // AC StartBlockText
                    //
                    IconIDControlString = "AC," + ACTypeAggregateFunction + ",0,Block Text,";
                    IconImg = genericController.GetAddonIconImg("/" + core.appConfig.adminRoute, 0, 0, 0, true, IconIDControlString, "", core.appConfig.cdnFilesNetprefix, "Text Block Start", "Block text to all except selected groups starting at this point", "", 0);
                    IconImg = genericController.EncodeJavascript(IconImg);
                    Items[ItemsCnt] = "['Block Text','" + IconImg + "']";
                    Index.setPtr("Block Text", ItemsCnt);
                    ItemsCnt = ItemsCnt + 1;
                    //
                    // AC EndBlockText
                    //
                    IconIDControlString = "AC," + ACTypeAggregateFunction + ",0,Block Text End,";
                    IconImg = genericController.GetAddonIconImg("/" + core.appConfig.adminRoute, 0, 0, 0, true, IconIDControlString, "", core.appConfig.cdnFilesNetprefix, "Text Block End", "End of text block", "", 0);
                    IconImg = genericController.EncodeJavascript(IconImg);
                    Items[ItemsCnt] = "['Block Text End','" + IconImg + "']";
                    Index.setPtr("Block Text", ItemsCnt);
                    ItemsCnt = ItemsCnt + 1;
                    //
                    if ((ContentType == csv_contentTypeEnum.contentTypeEmail) || (ContentType == csv_contentTypeEnum.contentTypeEmailTemplate)) {
                        //
                        // ----- Email Only AC tags
                        //
                        // Editing Email Body or Templates - Since Email can not process Add-ons, it main_Gets the legacy AC tags for now
                        //
                        // Personalization Tag
                        //
                        FieldList = Models.Complex.cdefModel.GetContentProperty(core, "people", "SelectFieldList");
                        FieldList = genericController.vbReplace(FieldList, ",", "|");
                        IconIDControlString = "AC,PERSONALIZATION,0,Personalization,field=[" + FieldList + "]";
                        IconImg = genericController.GetAddonIconImg("/" + core.appConfig.adminRoute, 0, 0, 0, true, IconIDControlString, "", core.appConfig.cdnFilesNetprefix, "Any Personalization Field", "Renders as any Personalization Field", "", 0);
                        IconImg = genericController.EncodeJavascript(IconImg);
                        Items[ItemsCnt] = "['Personalization','" + IconImg + "']";
                        Index.setPtr("Personalization", ItemsCnt);
                        ItemsCnt = ItemsCnt + 1;
                        //
                        if (ContentType == csv_contentTypeEnum.contentTypeEmailTemplate) {
                            //
                            // Editing Email Templates
                            //   This is a special case
                            //   Email content processing can not process add-ons, and PageContentBox and TextBox are needed
                            //   So I added the old AC Tag into the menu for this case
                            //   Need a more consistant solution later
                            //
                            IconIDControlString = "AC," + ACTypeTemplateContent + ",0,Template Content,";
                            IconImg = genericController.GetAddonIconImg("/" + core.appConfig.adminRoute, 52, 64, 0, false, IconIDControlString, "/ccLib/images/ACTemplateContentIcon.gif", core.appConfig.cdnFilesNetprefix, "Content Box", "Renders as the content for a template", "", 0);
                            IconImg = genericController.EncodeJavascript(IconImg);
                            Items[ItemsCnt] = "['Content Box','" + IconImg + "']";
                            //Items(ItemsCnt) = "['Template Content','<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Content"" id=""AC," & ACTypeTemplateContent & ",0,Template Content,"" src=""/ccLib/images/ACTemplateContentIcon.gif"" WIDTH=52 HEIGHT=64>']"
                            Index.setPtr("Content Box", ItemsCnt);
                            ItemsCnt = ItemsCnt + 1;
                            //
                            IconIDControlString = "AC," + ACTypeTemplateText + ",0,Template Text,Name=Default";
                            IconImg = genericController.GetAddonIconImg("/" + core.appConfig.adminRoute, 52, 52, 0, false, IconIDControlString, "/ccLib/images/ACTemplateTextIcon.gif", core.appConfig.cdnFilesNetprefix, "Template Text", "Renders as a template text block", "", 0);
                            IconImg = genericController.EncodeJavascript(IconImg);
                            Items[ItemsCnt] = "['Template Text','" + IconImg + "']";
                            //Items(ItemsCnt) = "['Template Text','<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Text"" id=""AC," & ACTypeTemplateText & ",0,Template Text,Name=Default"" src=""/ccLib/images/ACTemplateTextIcon.gif"" WIDTH=52 HEIGHT=52>']"
                            Index.setPtr("Template Text", ItemsCnt);
                            ItemsCnt = ItemsCnt + 1;
                        }
                    } else {
                        //
                        // ----- Web Only AC Tags
                        //
                        // Watch Lists
                        //
                        CSLists = core.db.csOpen("Content Watch Lists", "", "Name,ID", false, 0, false, false, "Name,ID", 20, 1);
                        if (core.db.csOk(CSLists)) {
                            while (core.db.csOk(CSLists)) {
                                FieldName = encodeText(core.db.csGetText(CSLists, "name")).Trim(' ');
                                if (!string.IsNullOrEmpty(FieldName)) {
                                    FieldCaption = "Watch List [" + FieldName + "]";
                                    IconIDControlString = "AC,WATCHLIST,0," + FieldName + ",ListName=" + FieldName + "&SortField=[DateAdded|Link|LinkLabel|Clicks|WhatsNewDateExpires]&SortDirection=Z-A[A-Z|Z-A]";
                                    IconImg = genericController.GetAddonIconImg("/" + core.appConfig.adminRoute, 0, 0, 0, true, IconIDControlString, "", core.appConfig.cdnFilesNetprefix, FieldCaption, "Rendered as the " + FieldCaption, "", 0);
                                    IconImg = genericController.EncodeJavascript(IconImg);
                                    FieldCaption = genericController.EncodeJavascript(FieldCaption);
                                    Items[ItemsCnt] = "['" + FieldCaption + "','" + IconImg + "']";
                                    //Items(ItemsCnt) = "['" & FieldCaption & "','<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the " & FieldCaption & """ id=""AC,WATCHLIST,0," & FieldName & ",ListName=" & FieldName & "&SortField=[DateAdded|Link|LinkLabel|Clicks|WhatsNewDateExpires]&SortDirection=Z-A[A-Z|Z-A]"" src=""/ccLib/images/ACWatchList.GIF"">']"
                                    Index.setPtr(FieldCaption, ItemsCnt);
                                    ItemsCnt = ItemsCnt + 1;
                                    if (ItemsCnt >= ItemsSize) {
                                        ItemsSize = ItemsSize + 100;
                                        Array.Resize(ref Items, ItemsSize + 1);
                                    }
                                }
                                core.db.csGoNext(CSLists);
                            }
                        }
                        core.db.csClose(ref CSLists);
                    }
                    //
                    // ----- Add-ons (AC Aggregate Functions)
                    //
                    if ((false) && (ContentType == csv_contentTypeEnum.contentTypeEmail)) {
                        //
                        // Email did not support add-ons
                        //
                    } else {
                        //
                        // Either non-email or > 4.0.325
                        //
                        Criteria = "(1=1)";
                        if (ContentType == csv_contentTypeEnum.contentTypeEmail) {
                            //
                            // select only addons with email placement (dont need to check main_version bc if email, must be >4.0.325
                            //
                            Criteria = Criteria + "and(email<>0)";
                        } else {
                            if (true) {
                                if (ContentType == csv_contentTypeEnum.contentTypeWeb) {
                                    //
                                    // Non Templates
                                    //
                                    Criteria = Criteria + "and(content<>0)";
                                } else {
                                    //
                                    // Templates
                                    //
                                    Criteria = Criteria + "and(template<>0)";
                                }
                            }
                        }
                        AddonContentName = cnAddons;
                        SelectList = "Name,Link,ID,ArgumentList,ObjectProgramID,IconFilename,IconWidth,IconHeight,IconSprites,IsInline,ccguid";
                        CSAddons = core.db.csOpen(AddonContentName, Criteria, "Name,ID", false, 0, false, false, SelectList);
                        if (core.db.csOk(CSAddons)) {
                            while (core.db.csOk(CSAddons)) {
                                AddonGuid = core.db.csGetText(CSAddons, "ccguid");
                                ObjectProgramID2 = core.db.csGetText(CSAddons, "ObjectProgramID");
                                if ((ContentType == csv_contentTypeEnum.contentTypeEmail) && (!string.IsNullOrEmpty(ObjectProgramID2))) {
                                    //
                                    // Block activex addons from email
                                    //
                                    //ObjectProgramID2 = ObjectProgramID2;
                                } else {
                                    AddonName = encodeText(core.db.csGet(CSAddons, "name")).Trim(' ');
                                    if (!string.IsNullOrEmpty(AddonName) & (AddonName != LastAddonName)) {
                                        //
                                        // Icon (fieldtyperesourcelink)
                                        //
                                        IsInline = core.db.csGetBoolean(CSAddons, "IsInline");
                                        IconFilename = core.db.csGet(CSAddons, "Iconfilename");
                                        if (string.IsNullOrEmpty(IconFilename)) {
                                            IconWidth = 0;
                                            IconHeight = 0;
                                            IconSprites = 0;
                                        } else {
                                            IconWidth = core.db.csGetInteger(CSAddons, "IconWidth");
                                            IconHeight = core.db.csGetInteger(CSAddons, "IconHeight");
                                            IconSprites = core.db.csGetInteger(CSAddons, "IconSprites");
                                        }
                                        //
                                        // Calculate DefaultAddonOption_String
                                        //
                                        UseAjaxDefaultAddonOptions = true;
                                        if (UseAjaxDefaultAddonOptions) {
                                            DefaultAddonOption_String = "";
                                        } else {
                                            ArgumentList = encodeText(core.db.csGet(CSAddons, "ArgumentList")).Trim(' ');
                                            DefaultAddonOption_String = addonController.main_GetDefaultAddonOption_String(core, ArgumentList, AddonGuid, IsInline);
                                            DefaultAddonOption_String = encodeHTML(DefaultAddonOption_String);
                                        }
                                        //
                                        // Changes necessary to support commas in AddonName and OptionString
                                        //   Remove commas in Field Name
                                        //   Then in Javascript, when spliting on comma, anything past position 4, put back onto 4
                                        //
                                        LastAddonName = AddonName;
                                        IconIDControlString = "AC,AGGREGATEFUNCTION,0," + AddonName + "," + DefaultAddonOption_String + "," + AddonGuid;
                                        IconImg = genericController.GetAddonIconImg("/" + core.appConfig.adminRoute, IconWidth, IconHeight, IconSprites, IsInline, IconIDControlString, IconFilename, core.appConfig.cdnFilesNetprefix, AddonName, "Rendered as the Add-on [" + AddonName + "]", "", 0);
                                        Items[ItemsCnt] = "['" + genericController.EncodeJavascript(AddonName) + "','" + genericController.EncodeJavascript(IconImg) + "']";
                                        Index.setPtr(AddonName, ItemsCnt);
                                        ItemsCnt = ItemsCnt + 1;
                                        if (ItemsCnt >= ItemsSize) {
                                            ItemsSize = ItemsSize + 100;
                                            Array.Resize(ref Items, ItemsSize + 1);
                                        }
                                    }
                                }
                                core.db.csGoNext(CSAddons);
                            }
                        }
                        core.db.csClose(ref CSAddons);
                    }
                    //
                    // Build output sting in alphabetical order by name
                    //
                    s = "";
                    ItemsPtr = Index.getFirstPtr();
                    while (ItemsPtr >= 0 && LoopPtr < ItemsCnt) {
                        s = s + "\r\n," + Items[ItemsPtr];
                        PtrTest = Index.getNextPtr();
                        if (PtrTest < 0) {
                            break;
                        } else {
                            ItemsPtr = PtrTest;
                        }
                        LoopPtr = LoopPtr + 1;
                    }
                    if (!string.IsNullOrEmpty(s)) {
                        s = "[" + s.Substring(3) + "]";
                    }
                    //
                    result = s;
                    core.docProperties.setProperty(cacheKey, result, false);
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        // Modify a string to be printed through the HTML stream
        //   convert carriage returns ( 0x10 ) to <br>
        //   remove linefeeds ( 0x13 )
        //====================================================================================================
        //
        public string convertCRLFToHtmlBreak(object Source) {
            string tempconvertCRLFToHtmlBreak = null;
            try {
                //
                string iSource;
                //
                iSource = genericController.encodeText(Source);
                tempconvertCRLFToHtmlBreak = "";
                if (!string.IsNullOrEmpty(iSource)) {
                    tempconvertCRLFToHtmlBreak = iSource;
                    tempconvertCRLFToHtmlBreak = genericController.vbReplace(tempconvertCRLFToHtmlBreak, "\r", "");
                    tempconvertCRLFToHtmlBreak = genericController.vbReplace(tempconvertCRLFToHtmlBreak, "\n", "<br>");
                }
                return tempconvertCRLFToHtmlBreak;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError18("main_EncodeCRLF")
            return tempconvertCRLFToHtmlBreak;
        }
        //
        //====================================================================================================
        //   Encodes characters to be compatibile with HTML
        //   i.e. it converts the equation 5 > 6 to th sequence "5 &gt; 6"
        //
        //   convert carriage returns ( 0x10 ) to <br>
        //   remove linefeeds ( 0x13 )
        //====================================================================================================
        //
        public string encodeHTML(object Source) {
            return genericController.encodeHTML(genericController.encodeText(Source));
        }
        //
        //========================================================================
        //   Convert an HTML source to a text equivelent
        //
        //       converts CRLF to <br>
        //       encodes reserved HTML characters to their equivalent
        //========================================================================
        //
        public string convertTextToHTML(string Source) {
            return convertCRLFToHtmlBreak(genericController.encodeHTML(Source));
        }
        //
        //===============================================================================================================================
        //   Get Addon Selector
        //
        //   The addon selector is the string sent out with the content in edit-mode. In the editor, it is converted by javascript
        //   to the popup window that selects instance options. It is in this format:
        //
        //   Select (creates a list of names in a select box, returns the selected name)
        //       name=currentvalue[optionname0:optionvalue0|optionname1:optionvalue1|...]
        //   CheckBox (creates a list of names in checkboxes, and returns the selected names)
        //===============================================================================================================================
        //
        public string getAddonSelector(string SrcOptionName, string InstanceOptionValue_AddonEncoded, string SrcOptionValueSelector) {
            string result = "";
            try {
                //
                const string ACFunctionList = "List";
                const string ACFunctionList1 = "selectname";
                const string ACFunctionList2 = "listname";
                const string ACFunctionList3 = "selectcontentname";
                const string ACFunctionListID = "ListID";
                const string ACFunctionListFields = "ListFields";
                //
                int CID = 0;
                bool IsContentList = false;
                bool IsListField = false;
                string Choice = null;
                string[] Choices = null;
                int ChoiceCnt = 0;
                int Ptr = 0;
                bool IncludeID = false;
                int FnLen = 0;
                int RecordID = 0;
                int CS = 0;
                string ContentName = null;
                int Pos = 0;
                string list = null;
                string FnArgList = null;
                string[] FnArgs = null;
                int FnArgCnt = 0;
                string ContentCriteria = null;
                string RecordName = null;
                string SrcSelectorInner = null;
                string SrcSelectorSuffix = "";
                object[,] Cell = null;
                int RowCnt = 0;
                int RowPtr = 0;
                string SrcSelector = SrcOptionValueSelector.Trim(' ');
                //
                SrcSelectorInner = SrcSelector;
                int PosLeft = genericController.vbInstr(1, SrcSelector, "[");
                if (PosLeft != 0) {
                    int PosRight = genericController.vbInstr(1, SrcSelector, "]");
                    if (PosRight != 0) {
                        if (PosRight < SrcSelector.Length) {
                            SrcSelectorSuffix = SrcSelector.Substring(PosRight);
                        }
                        SrcSelector = (SrcSelector.Substring(PosLeft - 1, PosRight - PosLeft + 1)).Trim(' ');
                        SrcSelectorInner = (SrcSelector.Substring(1, SrcSelector.Length - 2)).Trim(' ');
                    }
                }
                list = "";
                //
                // Break SrcSelectorInner up into individual choices to detect functions
                //
                if (!string.IsNullOrEmpty(SrcSelectorInner)) {
                    Choices = SrcSelectorInner.Split('|');
                    ChoiceCnt = Choices.GetUpperBound(0) + 1;
                    for (Ptr = 0; Ptr < ChoiceCnt; Ptr++) {
                        Choice = Choices[Ptr];
                        IsContentList = false;
                        IsListField = false;
                        //
                        // List Function (and all the indecision that went along with it)
                        //
                        Pos = 0;
                        if (Pos == 0) {
                            Pos = genericController.vbInstr(1, Choice, ACFunctionList1 + "(", 1);
                            if (Pos > 0) {
                                IsContentList = true;
                                IncludeID = false;
                                FnLen = ACFunctionList1.Length;
                            }
                        }
                        if (Pos == 0) {
                            Pos = genericController.vbInstr(1, Choice, ACFunctionList2 + "(", 1);
                            if (Pos > 0) {
                                IsContentList = true;
                                IncludeID = false;
                                FnLen = ACFunctionList2.Length;
                            }
                        }
                        if (Pos == 0) {
                            Pos = genericController.vbInstr(1, Choice, ACFunctionList3 + "(", 1);
                            if (Pos > 0) {
                                IsContentList = true;
                                IncludeID = false;
                                FnLen = ACFunctionList3.Length;
                            }
                        }
                        if (Pos == 0) {
                            Pos = genericController.vbInstr(1, Choice, ACFunctionListID + "(", 1);
                            if (Pos > 0) {
                                IsContentList = true;
                                IncludeID = true;
                                FnLen = ACFunctionListID.Length;
                            }
                        }
                        if (Pos == 0) {
                            Pos = genericController.vbInstr(1, Choice, ACFunctionList + "(", 1);
                            if (Pos > 0) {
                                IsContentList = true;
                                IncludeID = false;
                                FnLen = ACFunctionList.Length;
                            }
                        }
                        if (Pos == 0) {
                            Pos = genericController.vbInstr(1, Choice, ACFunctionListFields + "(", 1);
                            if (Pos > 0) {
                                IsListField = true;
                                IncludeID = false;
                                FnLen = ACFunctionListFields.Length;
                            }
                        }
                        //
                        if (Pos > 0) {
                            //
                            FnArgList = (Choice.Substring((Pos + FnLen) - 1)).Trim(' ');
                            ContentName = "";
                            ContentCriteria = "";
                            if ((FnArgList.Left(1) == "(") && (FnArgList.Substring(FnArgList.Length - 1) == ")")) {
                                //
                                // set ContentName and ContentCriteria from argument list
                                //
                                FnArgList = FnArgList.Substring(1, FnArgList.Length - 2);
                                FnArgs = genericController.SplitDelimited(FnArgList, ",");
                                FnArgCnt = FnArgs.GetUpperBound(0) + 1;
                                if (FnArgCnt > 0) {
                                    ContentName = FnArgs[0].Trim(' ');
                                    if ((ContentName.Left(1) == "\"") && (ContentName.Substring(ContentName.Length - 1) == "\"")) {
                                        ContentName = (ContentName.Substring(1, ContentName.Length - 2)).Trim(' ');
                                    } else if ((ContentName.Left(1) == "'") && (ContentName.Substring(ContentName.Length - 1) == "'")) {
                                        ContentName = (ContentName.Substring(1, ContentName.Length - 2)).Trim(' ');
                                    }
                                }
                                if (FnArgCnt > 1) {
                                    ContentCriteria = FnArgs[1].Trim(' ');
                                    if ((ContentCriteria.Left(1) == "\"") && (ContentCriteria.Substring(ContentCriteria.Length - 1) == "\"")) {
                                        ContentCriteria = (ContentCriteria.Substring(1, ContentCriteria.Length - 2)).Trim(' ');
                                    } else if ((ContentCriteria.Left(1) == "'") && (ContentCriteria.Substring(ContentCriteria.Length - 1) == "'")) {
                                        ContentCriteria = (ContentCriteria.Substring(1, ContentCriteria.Length - 2)).Trim(' ');
                                    }
                                }
                            }
                            CS = -1;
                            if (IsContentList) {
                                //
                                // ContentList - Open the Content and build the options from the names
                                //
                                if (!string.IsNullOrEmpty(ContentCriteria)) {
                                    CS = core.db.csOpen(ContentName, ContentCriteria, "name", true, 0, false, false, "ID,Name");
                                } else {
                                    CS = core.db.csOpen(ContentName, "", "name", true, 0, false, false, "ID,Name");
                                }
                            } else if (IsListField) {
                                //
                                // ListField
                                //
                                CID = Models.Complex.cdefModel.getContentId(core, ContentName);
                                if (CID > 0) {
                                    CS = core.db.csOpen("Content Fields", "Contentid=" + CID, "name", true, 0, false, false, "ID,Name");
                                }
                            }

                            if (core.db.csOk(CS)) {
                                Cell = core.db.cs_getRows(CS);
                                RowCnt = Cell.GetUpperBound(1) + 1;
                                for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                                    //
                                    RecordName = genericController.encodeText(Cell[1, RowPtr]);
                                    RecordName = genericController.vbReplace(RecordName, "\r\n", " ");
                                    RecordID = genericController.encodeInteger(Cell[0, RowPtr]);
                                    if (string.IsNullOrEmpty(RecordName)) {
                                        RecordName = "record " + RecordID;
                                    } else if (RecordName.Length > 50) {
                                        RecordName = RecordName.Left(50) + "...";
                                    }
                                    RecordName = genericController.encodeNvaArgument(RecordName);
                                    list = list + "|" + RecordName;
                                    if (IncludeID) {
                                        list = list + ":" + RecordID;
                                    }
                                }
                            }
                            core.db.csClose(ref CS);
                        } else {
                            //
                            // choice is not a function, just add the choice back to the list
                            //
                            list = list + "|" + Choices[Ptr];
                        }
                    }
                    if (!string.IsNullOrEmpty(list)) {
                        list = list.Substring(1);
                    }
                }
                //
                // Build output string
                //
                //csv_result = encodeNvaArgument(SrcOptionName)
                result = genericController.encodeHTML(genericController.encodeNvaArgument(SrcOptionName)) + "=";
                if (!string.IsNullOrEmpty(InstanceOptionValue_AddonEncoded)) {
                    result = result + genericController.encodeHTML(InstanceOptionValue_AddonEncoded);
                }
                if (string.IsNullOrEmpty(SrcSelectorSuffix) && string.IsNullOrEmpty(list)) {
                    //
                    // empty list with no suffix, return with name=value
                    //
                } else if (genericController.vbLCase(SrcSelectorSuffix) == "resourcelink") {
                    //
                    // resource link, exit with empty list
                    //
                    result = result + "[]ResourceLink";
                } else {
                    //
                    //
                    //
                    result = result + "[" + list + "]" + SrcSelectorSuffix;
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        // main_Get an HTML Form text input (or text area)
        //
        public string getFormInputHTML(string htmlName, string DefaultValue = "", string styleHeight = "", string styleWidth = "", bool readOnlyfield = false, bool allowActiveContent = false, string addonListJSON = "", string styleList = "", string styleOptionList = "", bool allowResourceLibrary = false) {
            string returnHtml = "";
            try {
                string FieldTypeDefaultEditorAddonIdList = editorController.getFieldTypeDefaultEditorAddonIdList(core);
                string[] FieldTypeDefaultEditorAddonIds = FieldTypeDefaultEditorAddonIdList.Split(',');
                int FieldTypeDefaultEditorAddonId = genericController.encodeInteger(FieldTypeDefaultEditorAddonIds[FieldTypeIdHTML]);
                if (FieldTypeDefaultEditorAddonId == 0) {
                    //
                    //    use default wysiwyg
                    returnHtml = inputTextExpandable(htmlName, DefaultValue);
                } else {
                    //
                    // use addon editor
                    Dictionary<string, string> arguments = new Dictionary<string, string>();
                    arguments.Add("editorName", htmlName);
                    arguments.Add("editorValue", DefaultValue);
                    arguments.Add("editorFieldType", FieldTypeIdHTML.ToString());
                    arguments.Add("editorReadOnly", readOnlyfield.ToString());
                    arguments.Add("editorWidth", styleWidth);
                    arguments.Add("editorHeight", styleHeight);
                    arguments.Add("editorAllowResourceLibrary", allowResourceLibrary.ToString());
                    arguments.Add("editorAllowActiveContent", allowActiveContent.ToString());
                    arguments.Add("editorAddonList", addonListJSON);
                    arguments.Add("editorStyles", styleList);
                    arguments.Add("editorStyleOptions", styleOptionList);
                    returnHtml = core.addon.execute(addonModel.create(core, FieldTypeDefaultEditorAddonId), new CPUtilsBaseClass.addonExecuteContext() {
                        addonType = CPUtilsBaseClass.addonContext.ContextEditor,
                        instanceArguments = arguments
                    });
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnHtml;
        }
        //
        //========================================================================
        // ----- Process the reply from the Tools Panel form
        //========================================================================
        //
        public void processFormToolsPanel(string legacyFormSn = "") {
            try {
                string Button = null;
                string username = null;
                //
                // ----- Read in and save the Member profile values from the tools panel
                //
                if (core.doc.sessionContext.user.id > 0) {
                    if (!(core.doc.debug_iUserError != "")) {
                        Button = core.docProperties.getText(legacyFormSn + "mb");
                        switch (Button) {
                            case ButtonLogout:
                                //
                                // Logout - This can only come from the Horizonal Tool Bar
                                //
                                core.doc.sessionContext.logout(core);
                                break;
                            case ButtonLogin:
                                //
                                // Login - This can only come from the Horizonal Tool Bar
                                //
                                Controllers.loginController.processFormLoginDefault(core);
                                break;
                            case ButtonApply:
                                //
                                // Apply
                                //
                                username = core.docProperties.getText(legacyFormSn + "username");
                                if (!string.IsNullOrEmpty(username)) {
                                    Controllers.loginController.processFormLoginDefault(core);
                                }
                                //
                                // ----- AllowAdminLinks
                                //
                                core.visitProperty.setProperty("AllowEditing", genericController.encodeText(core.docProperties.getBoolean(legacyFormSn + "AllowEditing")));
                                //
                                // ----- Quick Editor
                                //
                                core.visitProperty.setProperty("AllowQuickEditor", genericController.encodeText(core.docProperties.getBoolean(legacyFormSn + "AllowQuickEditor")));
                                //
                                // ----- Advanced Editor
                                //
                                core.visitProperty.setProperty("AllowAdvancedEditor", genericController.encodeText(core.docProperties.getBoolean(legacyFormSn + "AllowAdvancedEditor")));
                                //
                                // ----- Allow Workflow authoring Render Mode - Visit Property
                                //
                                core.visitProperty.setProperty("AllowWorkflowRendering", genericController.encodeText(core.docProperties.getBoolean(legacyFormSn + "AllowWorkflowRendering")));
                                //
                                // ----- developer Only parts
                                //
                                core.visitProperty.setProperty("AllowDebugging", genericController.encodeText(core.docProperties.getBoolean(legacyFormSn + "AllowDebugging")));
                                break;
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        // -----
        //========================================================================
        //
        public void processAddonSettingsEditor() {
            //
            string constructor = null;
            bool ParseOK = false;
            int PosNameStart = 0;
            int PosNameEnd = 0;
            string AddonName = null;
            //Dim CSAddon As Integer
            int OptionPtr = 0;
            string ArgValueAddonEncoded = null;
            int OptionCnt = 0;
            bool needToClearCache = false;
            string[] ConstructorSplit = null;
            int Ptr = 0;
            string[] Arg = null;
            string ArgName = null;
            string ArgValue = null;
            string AddonOptionConstructor = "";
            string addonOption_String = "";
            int fieldType = 0;
            string Copy = "";
            int RecordID = 0;
            string FieldName = null;
            string ACInstanceID = null;
            string ContentName = null;
            int CS = 0;
            int PosACInstanceID = 0;
            int PosStart = 0;
            int PosIDStart = 0;
            int PosIDEnd = 0;
            //
            ContentName = core.docProperties.getText("ContentName");
            RecordID = core.docProperties.getInteger("RecordID");
            FieldName = core.docProperties.getText("FieldName");
            ACInstanceID = core.docProperties.getText("ACInstanceID");
            bool FoundAddon = false;
            if (ACInstanceID == PageChildListInstanceID) {
                //
                // ----- Page Content Child List Add-on
                //
                if (RecordID != 0) {
                    addonModel addon = core.addonCache.getAddonById(core.siteProperties.childListAddonID);
                    if (addon != null) {
                        FoundAddon = true;
                        AddonOptionConstructor = addon.ArgumentList;
                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\r\n", "\r");
                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\n", "\r");
                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\r", "\r\n");
                        if (true) {
                            if (!string.IsNullOrEmpty(AddonOptionConstructor)) {
                                AddonOptionConstructor = AddonOptionConstructor + "\r\n";
                            }
                            if (addon.IsInline) {
                                AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Inline;
                            } else {
                                AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Block;
                            }
                        }

                        ConstructorSplit = genericController.stringSplit(AddonOptionConstructor, "\r\n");
                        AddonOptionConstructor = "";
                        //
                        // main_Get all responses from current Argument List and build new addonOption_String
                        //
                        for (Ptr = 0; Ptr <= ConstructorSplit.GetUpperBound(0); Ptr++) {
                            Arg = ConstructorSplit[Ptr].Split('=');
                            ArgName = Arg[0];
                            OptionCnt = core.docProperties.getInteger(ArgName + "CheckBoxCnt");
                            if (OptionCnt > 0) {
                                ArgValueAddonEncoded = "";
                                for (OptionPtr = 0; OptionPtr < OptionCnt; OptionPtr++) {
                                    ArgValue = core.docProperties.getText(ArgName + OptionPtr);
                                    if (!string.IsNullOrEmpty(ArgValue)) {
                                        ArgValueAddonEncoded = ArgValueAddonEncoded + "," + genericController.encodeNvaArgument(ArgValue);
                                    }
                                }
                                if (!string.IsNullOrEmpty(ArgValueAddonEncoded)) {
                                    ArgValueAddonEncoded = ArgValueAddonEncoded.Substring(1);
                                }
                            } else {
                                ArgValue = core.docProperties.getText(ArgName);
                                ArgValueAddonEncoded = genericController.encodeNvaArgument(ArgValue);
                            }
                            addonOption_String = addonOption_String + "&" + genericController.encodeNvaArgument(ArgName) + "=" + ArgValueAddonEncoded;
                        }
                        if (!string.IsNullOrEmpty(addonOption_String)) {
                            addonOption_String = addonOption_String.Substring(1);
                        }

                    }
                    core.db.executeQuery("update ccpagecontent set ChildListInstanceOptions=" + core.db.encodeSQLText(addonOption_String) + " where id=" + RecordID);
                    needToClearCache = true;
                    //CS = main_OpenCSContentRecord("page content", RecordID)
                    //If app.csv_IsCSOK(CS) Then
                    //    Call app.SetCS(CS, "ChildListInstanceOptions", addonOption_String)
                    //    needToClearCache = True
                    //End If
                    //Call app.closeCS(CS)
                }
            } else if ((ACInstanceID == "-2") && (!string.IsNullOrEmpty(FieldName))) {
                //
                // ----- Admin Addon, ACInstanceID=-2, FieldName=AddonName
                //
                AddonName = FieldName;
                FoundAddon = false;
                addonModel addon = core.addonCache.getAddonByName(AddonName);
                if (addon != null) {
                    FoundAddon = true;
                    AddonOptionConstructor = addon.ArgumentList;
                    AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\r\n", "\r");
                    AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\n", "\r");
                    AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\r", "\r\n");
                    if (!string.IsNullOrEmpty(AddonOptionConstructor)) {
                        AddonOptionConstructor = AddonOptionConstructor + "\r\n";
                    }
                    if (genericController.encodeBoolean(addon.IsInline)) {
                        AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Inline;
                    } else {
                        AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Block;
                    }
                }
                if (!FoundAddon) {
                    //
                    // Hardcoded Addons
                    //
                    switch (genericController.vbLCase(AddonName)) {
                        case "block text":
                            FoundAddon = true;
                            AddonOptionConstructor = AddonOptionConstructor_ForBlockText;
                            break;
                        case "":
                            break;
                    }
                }
                if (FoundAddon) {
                    ConstructorSplit = genericController.stringSplit(AddonOptionConstructor, "\r\n");
                    addonOption_String = "";
                    //
                    // main_Get all responses from current Argument List
                    //
                    for (Ptr = 0; Ptr <= ConstructorSplit.GetUpperBound(0); Ptr++) {
                        string nvp = ConstructorSplit[Ptr].Trim(' ');
                        if (!string.IsNullOrEmpty(nvp)) {
                            Arg = ConstructorSplit[Ptr].Split('=');
                            ArgName = Arg[0];
                            OptionCnt = core.docProperties.getInteger(ArgName + "CheckBoxCnt");
                            if (OptionCnt > 0) {
                                ArgValueAddonEncoded = "";
                                for (OptionPtr = 0; OptionPtr < OptionCnt; OptionPtr++) {
                                    ArgValue = core.docProperties.getText(ArgName + OptionPtr);
                                    if (!string.IsNullOrEmpty(ArgValue)) {
                                        ArgValueAddonEncoded = ArgValueAddonEncoded + "," + genericController.encodeNvaArgument(ArgValue);
                                    }
                                }
                                if (!string.IsNullOrEmpty(ArgValueAddonEncoded)) {
                                    ArgValueAddonEncoded = ArgValueAddonEncoded.Substring(1);
                                }
                            } else {
                                ArgValue = core.docProperties.getText(ArgName);
                                ArgValueAddonEncoded = genericController.encodeNvaArgument(ArgValue);
                            }
                            addonOption_String = addonOption_String + "&" + genericController.encodeNvaArgument(ArgName) + "=" + ArgValueAddonEncoded;
                        }
                    }
                    if (!string.IsNullOrEmpty(addonOption_String)) {
                        addonOption_String = addonOption_String.Substring(1);
                    }
                    core.userProperty.setProperty("Addon [" + AddonName + "] Options", addonOption_String);
                    needToClearCache = true;
                }
            } else if (string.IsNullOrEmpty(ContentName) || RecordID == 0) {
                //
                // ----- Public Site call, must have contentname and recordid
                //
                core.handleException(new Exception("invalid content [" + ContentName + "], RecordID [" + RecordID + "]"));
            } else {
                //
                // ----- Normal Content Edit - find instance in the content
                //
                CS = core.db.csOpenRecord(ContentName, RecordID);
                if (!core.db.csOk(CS)) {
                    core.handleException(new Exception("No record found with content [" + ContentName + "] and RecordID [" + RecordID + "]"));
                } else {
                    if (!string.IsNullOrEmpty(FieldName)) {
                        //
                        // Field is given, find the position
                        //
                        Copy = core.db.csGet(CS, FieldName);
                        PosACInstanceID = genericController.vbInstr(1, Copy, "=\"" + ACInstanceID + "\" ", 1);
                    } else {
                        //
                        // Find the field, then find the position
                        //
                        FieldName = core.db.cs_getFirstFieldName(CS);
                        while (!string.IsNullOrEmpty(FieldName)) {
                            fieldType = core.db.cs_getFieldTypeId(CS, FieldName);
                            switch (fieldType) {
                                case FieldTypeIdLongText:
                                case FieldTypeIdText:
                                case FieldTypeIdFileText:
                                case FieldTypeIdFileCSS:
                                case FieldTypeIdFileXML:
                                case FieldTypeIdFileJavascript:
                                case FieldTypeIdHTML:
                                case FieldTypeIdFileHTML:
                                    Copy = core.db.csGet(CS, FieldName);
                                    PosACInstanceID = genericController.vbInstr(1, Copy, "ACInstanceID=\"" + ACInstanceID + "\"", 1);
                                    if (PosACInstanceID != 0) {
                                        //
                                        // found the instance
                                        //
                                        PosACInstanceID = PosACInstanceID + 13;
                                        //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                        //ORIGINAL LINE: Exit Do
                                        goto ExitLabel1;
                                    }
                                    break;
                            }
                            FieldName = core.db.cs_getNextFieldName(CS);
                        }
                        ExitLabel1:;
                    }
                    //
                    // Parse out the Addon Name
                    //
                    if (PosACInstanceID == 0) {
                        core.handleException(new Exception("AC Instance [" + ACInstanceID + "] not found in record with content [" + ContentName + "] and RecordID [" + RecordID + "]"));
                    } else {
                        Copy = activeContentController.optimizeLibraryFileImagesInHtmlContent(core, Copy);
                        ParseOK = false;
                        PosStart = Copy.LastIndexOf("<ac ", PosACInstanceID - 1, System.StringComparison.OrdinalIgnoreCase) + 1;
                        if (PosStart != 0) {
                            //
                            // main_Get Addon Name to lookup Addon and main_Get most recent Argument List
                            //
                            PosNameStart = genericController.vbInstr(PosStart, Copy, " name=", 1);
                            if (PosNameStart != 0) {
                                PosNameStart = PosNameStart + 7;
                                PosNameEnd = genericController.vbInstr(PosNameStart, Copy, "\"");
                                if (PosNameEnd != 0) {
                                    AddonName = Copy.Substring(PosNameStart - 1, PosNameEnd - PosNameStart);
                                    //????? test this
                                    FoundAddon = false;
                                    addonModel embeddedAddon = core.addonCache.getAddonByName(AddonName);
                                    if (embeddedAddon != null) {
                                        FoundAddon = true;
                                        AddonOptionConstructor = genericController.encodeText(embeddedAddon.ArgumentList);
                                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\r\n", "\r");
                                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\n", "\r");
                                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\r", "\r\n");
                                        if (!string.IsNullOrEmpty(AddonOptionConstructor)) {
                                            AddonOptionConstructor = AddonOptionConstructor + "\r\n";
                                        }
                                        if (genericController.encodeBoolean(embeddedAddon.IsInline)) {
                                            AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Inline;
                                        } else {
                                            AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Block;
                                        }
                                    } else {
                                        //
                                        // -- Hardcoded Addons
                                        switch (genericController.vbLCase(AddonName)) {
                                            case "block text":
                                                FoundAddon = true;
                                                AddonOptionConstructor = AddonOptionConstructor_ForBlockText;
                                                break;
                                            case "":
                                                break;
                                        }
                                    }
                                    if (FoundAddon) {
                                        ConstructorSplit = genericController.stringSplit(AddonOptionConstructor, "\r\n");
                                        addonOption_String = "";
                                        //
                                        // main_Get all responses from current Argument List
                                        //
                                        for (Ptr = 0; Ptr <= ConstructorSplit.GetUpperBound(0); Ptr++) {
                                            constructor = ConstructorSplit[Ptr];
                                            if (!string.IsNullOrEmpty(constructor)) {
                                                Arg = constructor.Split('=');
                                                ArgName = Arg[0];
                                                OptionCnt = core.docProperties.getInteger(ArgName + "CheckBoxCnt");
                                                if (OptionCnt > 0) {
                                                    ArgValueAddonEncoded = "";
                                                    for (OptionPtr = 0; OptionPtr < OptionCnt; OptionPtr++) {
                                                        ArgValue = core.docProperties.getText(ArgName + OptionPtr);
                                                        if (!string.IsNullOrEmpty(ArgValue)) {
                                                            ArgValueAddonEncoded = ArgValueAddonEncoded + "," + genericController.encodeNvaArgument(ArgValue);
                                                        }
                                                    }
                                                    if (!string.IsNullOrEmpty(ArgValueAddonEncoded)) {
                                                        ArgValueAddonEncoded = ArgValueAddonEncoded.Substring(1);
                                                    }
                                                } else {
                                                    ArgValue = core.docProperties.getText(ArgName);
                                                    ArgValueAddonEncoded = genericController.encodeNvaArgument(ArgValue);
                                                }

                                                addonOption_String = addonOption_String + "&" + genericController.encodeNvaArgument(ArgName) + "=" + ArgValueAddonEncoded;
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(addonOption_String)) {
                                            addonOption_String = addonOption_String.Substring(1);
                                        }
                                    }
                                }
                            }
                            //
                            // Replace the new querystring into the AC tag in the content
                            //
                            PosIDStart = genericController.vbInstr(PosStart, Copy, " querystring=", 1);
                            if (PosIDStart != 0) {
                                PosIDStart = PosIDStart + 14;
                                if (PosIDStart != 0) {
                                    PosIDEnd = genericController.vbInstr(PosIDStart, Copy, "\"");
                                    if (PosIDEnd != 0) {
                                        ParseOK = true;
                                        Copy = Copy.Left(PosIDStart - 1) + genericController.encodeHTML(addonOption_String) + Copy.Substring(PosIDEnd - 1);
                                        core.db.csSet(CS, FieldName, Copy);
                                        needToClearCache = true;
                                    }
                                }
                            }
                        }
                        if (!ParseOK) {
                            core.handleException(new Exception("There was a problem parsing AC Instance [" + ACInstanceID + "] record with content [" + ContentName + "] and RecordID [" + RecordID + "]"));
                        }
                    }
                }
                core.db.csClose(ref CS);
            }
            if (needToClearCache) {
                //
                // Clear Caches
                //
                if (!string.IsNullOrEmpty(ContentName)) {
                    core.cache.invalidateAllInContent(ContentName);
                }
            }
        }
        //
        //========================================================================
        // ----- Process the little edit form in the help bubble
        //========================================================================
        //
        public void processHelpBubbleEditor() {
            //
            string SQL = null;
            string HelpBubbleID = null;
            string[] IDSplit = null;
            int RecordID = 0;
            string HelpCaption = null;
            string HelpMessage = null;
            //
            HelpBubbleID = core.docProperties.getText("HelpBubbleID");
            IDSplit = HelpBubbleID.Split('-');
            switch (genericController.vbLCase(IDSplit[0])) {
                case "userfield":
                    //
                    // main_Get the id of the field, and save the input as the caption and help
                    //
                    if (IDSplit.GetUpperBound(0) > 0) {
                        RecordID = genericController.encodeInteger(IDSplit[1]);
                        if (RecordID > 0) {
                            HelpCaption = core.docProperties.getText("helpcaption");
                            HelpMessage = core.docProperties.getText("helptext");
                            SQL = "update ccfields set caption=" + core.db.encodeSQLText(HelpCaption) + ",HelpMessage=" + core.db.encodeSQLText(HelpMessage) + " where id=" + RecordID;
                            core.db.executeQuery(SQL);
                            core.cache.invalidateAll();
                            core.doc.clearMetaData();
                        }
                    }
                    break;
            }
        }

        //
        //========================================================================
        // Print the Member Edit form
        //
        //   For instance, list out a checklist of all public groups, with the ones checked that this member belongs to
        //       PrimaryContentName = "People"
        //       PrimaryRecordID = MemberID
        //       SecondaryContentName = "Groups"
        //       SecondaryContentSelectCriteria = "ccGroups.PublicJoin<>0"
        //       RulesContentName = "Member Rules"
        //       RulesPrimaryFieldName = "MemberID"
        //       RulesSecondaryFieldName = "GroupID"
        //========================================================================
        //
        public string getCheckList2(string TagName, string PrimaryContentName, int PrimaryRecordID, string SecondaryContentName, string RulesContentName, string RulesPrimaryFieldname, string RulesSecondaryFieldName, string SecondaryContentSelectCriteria = "", string CaptionFieldName = "", bool readOnlyfield = false) {
            return getCheckList(TagName, PrimaryContentName, PrimaryRecordID, SecondaryContentName, RulesContentName, RulesPrimaryFieldname, RulesSecondaryFieldName, SecondaryContentSelectCriteria, genericController.encodeText(CaptionFieldName), readOnlyfield, false, "");
        }
        //
        //========================================================================
        //   main_Get a list of checkbox options based on a standard set of rules
        //
        //   IncludeContentFolderDivs
        //       When true, the list of options (checkboxes) are grouped by ContentFolder and wrapped in a Div with ID="ContentFolder99"
        //
        //   For instance, list out a options of all public groups, with the ones checked that this member belongs to
        //       PrimaryContentName = "People"
        //       PrimaryRecordID = MemberID
        //       SecondaryContentName = "Groups"
        //       SecondaryContentSelectCriteria = "ccGroups.PublicJoin<>0"
        //       RulesContentName = "Member Rules"
        //       RulesPrimaryFieldName = "MemberID"
        //       RulesSecondaryFieldName = "GroupID"
        //========================================================================
        //
        public string getCheckList(string TagName, string PrimaryContentName, int PrimaryRecordID, string SecondaryContentName, string RulesContentName, string RulesPrimaryFieldname, string RulesSecondaryFieldName, string SecondaryContentSelectCriteria = "", string CaptionFieldName = "", bool readOnlyfield = false, bool IncludeContentFolderDivs = false, string DefaultSecondaryIDList = "") {
            string returnHtml = "";
            try {
                string[] main_MemberShipText = null;
                int Ptr = 0;
                int main_MemberShipID = 0;
                string javaScriptRequired = "";
                string DivName = null;
                string OldFolderVar = null;
                string EndDiv = null;
                string RuleCopyCaption = null;
                string RuleCopy = null;
                string SQL = null;
                int CS = 0;
                int main_MemberShipCount = 0;
                int main_MemberShipSize = 0;
                int main_MemberShipPointer = 0;
                int CheckBoxCnt = 0;
                int DivCheckBoxCnt = 0;
                int[] main_MemberShip = { };
                string[] main_MemberShipRuleCopy = { };
                int PrimaryContentID = 0;
                string SecondaryTablename = null;
                int SecondaryContentID = 0;
                string rulesTablename = null;
                string OptionName = null;
                string OptionCaption = null;
                string optionCaptionHtmlEncoded = null;
                bool CanSeeHiddenFields = false;
                Models.Complex.cdefModel SecondaryCDef = null;
                List<int> ContentIDList = new List<int>();
                bool Found = false;
                int RecordID = 0;
                string SingularPrefixHtmlEncoded = null;
                bool IsRuleCopySupported = false;
                bool AllowRuleCopy = false;
                //
                // IsRuleCopySupported - if true, the rule records include an allow button, and copy
                //   This is for a checkbox like [ ] Other [enter other copy here]
                //
                IsRuleCopySupported = Models.Complex.cdefModel.isContentFieldSupported(core, RulesContentName, "RuleCopy");
                if (IsRuleCopySupported) {
                    IsRuleCopySupported = IsRuleCopySupported && Models.Complex.cdefModel.isContentFieldSupported(core, SecondaryContentName, "AllowRuleCopy");
                    if (IsRuleCopySupported) {
                        IsRuleCopySupported = IsRuleCopySupported && Models.Complex.cdefModel.isContentFieldSupported(core, SecondaryContentName, "RuleCopyCaption");
                    }
                }
                if (string.IsNullOrEmpty(CaptionFieldName)) {
                    CaptionFieldName = "name";
                }
                CaptionFieldName = genericController.encodeEmptyText(CaptionFieldName, "name");
                if (string.IsNullOrEmpty(PrimaryContentName) || string.IsNullOrEmpty(SecondaryContentName) || string.IsNullOrEmpty(RulesContentName) || string.IsNullOrEmpty(RulesPrimaryFieldname) || string.IsNullOrEmpty(RulesSecondaryFieldName)) {
                    returnHtml = "[Checklist not configured]";
                    core.handleException(new Exception("Creating checklist, all required fields were not supplied, Caption=[" + CaptionFieldName + "], PrimaryContentName=[" + PrimaryContentName + "], SecondaryContentName=[" + SecondaryContentName + "], RulesContentName=[" + RulesContentName + "], RulesPrimaryFieldName=[" + RulesPrimaryFieldname + "], RulesSecondaryFieldName=[" + RulesSecondaryFieldName + "]"));
                } else {
                    //
                    // ----- Gather all the SecondaryContent that associates to the PrimaryContent
                    //
                    PrimaryContentID = Models.Complex.cdefModel.getContentId(core, PrimaryContentName);
                    SecondaryCDef = Models.Complex.cdefModel.getCdef(core, SecondaryContentName);
                    SecondaryTablename = SecondaryCDef.ContentTableName;
                    SecondaryContentID = SecondaryCDef.Id;
                    ContentIDList.Add(SecondaryContentID);
                    ContentIDList.AddRange(SecondaryCDef.get_childIdList(core));
                    //
                    //
                    //
                    rulesTablename = Models.Complex.cdefModel.getContentTablename(core, RulesContentName);
                    SingularPrefixHtmlEncoded = genericController.encodeHTML(genericController.GetSingular(SecondaryContentName)) + "&nbsp;";
                    //
                    main_MemberShipCount = 0;
                    main_MemberShipSize = 0;
                    returnHtml = "";
                    if ((!string.IsNullOrEmpty(SecondaryTablename)) & (!string.IsNullOrEmpty(rulesTablename))) {
                        OldFolderVar = "OldFolder" + core.doc.checkListCnt;
                        javaScriptRequired += "var " + OldFolderVar + ";";
                        if (PrimaryRecordID == 0) {
                            //
                            // New record, use the DefaultSecondaryIDList
                            //
                            if (!string.IsNullOrEmpty(DefaultSecondaryIDList)) {

                                main_MemberShipText = DefaultSecondaryIDList.Split(',');
                                for (Ptr = 0; Ptr <= main_MemberShipText.GetUpperBound(0); Ptr++) {
                                    main_MemberShipID = genericController.encodeInteger(main_MemberShipText[Ptr]);
                                    if (main_MemberShipID != 0) {
                                        Array.Resize(ref main_MemberShip, Ptr + 1);
                                        main_MemberShip[Ptr] = main_MemberShipID;
                                        main_MemberShipCount = Ptr + 1;
                                    }
                                }
                                if (main_MemberShipCount > 0) {
                                    main_MemberShipRuleCopy = new string[main_MemberShipCount];
                                }
                                //main_MemberShipCount = UBound(main_MemberShip) + 1
                                main_MemberShipSize = main_MemberShipCount;
                            }
                        } else {
                            //
                            // ----- Determine main_MemberShip (which secondary records are associated by a rule)
                            // ----- (exclude new record issue ID=0)
                            //
                            if (IsRuleCopySupported) {
                                SQL = "SELECT " + SecondaryTablename + ".ID AS ID," + rulesTablename + ".RuleCopy";
                            } else {
                                SQL = "SELECT " + SecondaryTablename + ".ID AS ID,'' as RuleCopy";
                            }
                            SQL += ""
                            + " FROM " + SecondaryTablename + " LEFT JOIN"
                            + " " + rulesTablename + " ON " + SecondaryTablename + ".ID = " + rulesTablename + "." + RulesSecondaryFieldName + " WHERE "
                            + " (" + rulesTablename + "." + RulesPrimaryFieldname + "=" + PrimaryRecordID + ")"
                            + " AND (" + rulesTablename + ".Active<>0)"
                            + " AND (" + SecondaryTablename + ".Active<>0)"
                            + " And (" + SecondaryTablename + ".ContentControlID IN (" + string.Join(",", ContentIDList) + "))";
                            if (!string.IsNullOrEmpty(SecondaryContentSelectCriteria)) {
                                SQL += "AND(" + SecondaryContentSelectCriteria + ")";
                            }
                            CS = core.db.csOpenSql(SQL);
                            if (core.db.csOk(CS)) {
                                if (true) {
                                    main_MemberShipSize = 10;
                                    main_MemberShip = new int[main_MemberShipSize + 1];
                                    main_MemberShipRuleCopy = new string[main_MemberShipSize + 1];
                                    while (core.db.csOk(CS)) {
                                        if (main_MemberShipCount >= main_MemberShipSize) {
                                            main_MemberShipSize = main_MemberShipSize + 10;
                                            Array.Resize(ref main_MemberShip, main_MemberShipSize + 1);
                                            Array.Resize(ref main_MemberShipRuleCopy, main_MemberShipSize + 1);
                                        }
                                        main_MemberShip[main_MemberShipCount] = core.db.csGetInteger(CS, "ID");
                                        main_MemberShipRuleCopy[main_MemberShipCount] = core.db.csGetText(CS, "RuleCopy");
                                        main_MemberShipCount = main_MemberShipCount + 1;
                                        core.db.csGoNext(CS);
                                    }
                                }
                            }
                            core.db.csClose(ref CS);
                        }
                        //
                        // ----- Gather all the Secondary Records, sorted by ContentName
                        //
                        SQL = "SELECT " + SecondaryTablename + ".ID AS ID, " + SecondaryTablename + "." + CaptionFieldName + " AS OptionCaption, " + SecondaryTablename + ".name AS OptionName, " + SecondaryTablename + ".SortOrder";
                        if (IsRuleCopySupported) {
                            SQL += "," + SecondaryTablename + ".AllowRuleCopy," + SecondaryTablename + ".RuleCopyCaption";
                        } else {
                            SQL += ",0 as AllowRuleCopy,'' as RuleCopyCaption";
                        }
                        SQL += " from " + SecondaryTablename + " where (1=1)";
                        if (!string.IsNullOrEmpty(SecondaryContentSelectCriteria)) {
                            SQL += "AND(" + SecondaryContentSelectCriteria + ")";
                        }
                        SQL += " GROUP BY " + SecondaryTablename + ".ID, " + SecondaryTablename + "." + CaptionFieldName + ", " + SecondaryTablename + ".name, " + SecondaryTablename + ".SortOrder";
                        if (IsRuleCopySupported) {
                            SQL += ", " + SecondaryTablename + ".AllowRuleCopy," + SecondaryTablename + ".RuleCopyCaption";
                        }
                        SQL += " ORDER BY ";
                        SQL += SecondaryTablename + "." + CaptionFieldName;
                        CS = core.db.csOpenSql(SQL);
                        if (!core.db.csOk(CS)) {
                            returnHtml = "(No choices are available.)";
                        } else {
                            if (true) {
                                EndDiv = "";
                                CheckBoxCnt = 0;
                                DivCheckBoxCnt = 0;
                                CanSeeHiddenFields = core.doc.sessionContext.isAuthenticatedDeveloper(core);
                                DivName = TagName + ".All";
                                while (core.db.csOk(CS)) {
                                    OptionName = core.db.csGetText(CS, "OptionName");
                                    if ((OptionName.Left(1) != "_") || CanSeeHiddenFields) {
                                        //
                                        // Current checkbox is visible
                                        //
                                        RecordID = core.db.csGetInteger(CS, "ID");
                                        AllowRuleCopy = core.db.csGetBoolean(CS, "AllowRuleCopy");
                                        RuleCopyCaption = core.db.csGetText(CS, "RuleCopyCaption");
                                        OptionCaption = core.db.csGetText(CS, "OptionCaption");
                                        if (string.IsNullOrEmpty(OptionCaption)) {
                                            OptionCaption = OptionName;
                                        }
                                        if (string.IsNullOrEmpty(OptionCaption)) {
                                            optionCaptionHtmlEncoded = SingularPrefixHtmlEncoded + RecordID;
                                        } else {
                                            optionCaptionHtmlEncoded = genericController.encodeHTML(OptionCaption);
                                        }
                                        if (DivCheckBoxCnt != 0) {
                                            // leave this between checkboxes - it is searched in the admin page
                                            returnHtml += "<br>\r\n";
                                        }
                                        RuleCopy = "";
                                        Found = false;
                                        if (main_MemberShipCount != 0) {
                                            for (main_MemberShipPointer = 0; main_MemberShipPointer < main_MemberShipCount; main_MemberShipPointer++) {
                                                if (main_MemberShip[main_MemberShipPointer] == (RecordID)) {
                                                    //s = s & main_GetFormInputHidden(TagName & "." & CheckBoxCnt, True)
                                                    RuleCopy = main_MemberShipRuleCopy[main_MemberShipPointer];
                                                    Found = true;
                                                    break;
                                                }
                                            }
                                        }
                                        // must leave the first hidden with the value in this form - it is searched in the admin pge
                                        returnHtml += "\r\n";
                                        returnHtml += "<table><tr><td style=\"vertical-align:top;margin-top:0;width:20px;\">";
                                        returnHtml += "<input type=hidden name=\"" + TagName + "." + CheckBoxCnt + ".ID\" value=" + RecordID + ">";
                                        if (readOnlyfield && !Found) {
                                            returnHtml += "<input type=checkbox disabled>";
                                        } else if (readOnlyfield) {
                                            returnHtml += "<input type=checkbox disabled checked>";
                                            returnHtml += "<input type=\"hidden\" name=\"" + TagName + "." + CheckBoxCnt + ".ID\" value=" + RecordID + ">";
                                        } else if (Found) {
                                            returnHtml += "<input type=checkbox name=\"" + TagName + "." + CheckBoxCnt + "\" checked>";
                                        } else {
                                            returnHtml += "<input type=checkbox name=\"" + TagName + "." + CheckBoxCnt + "\">";
                                        }
                                        returnHtml += "</td><td style=\"vertical-align:top;padding-top:4px;\">";
                                        returnHtml += SpanClassAdminNormal + optionCaptionHtmlEncoded;
                                        if (AllowRuleCopy) {
                                            returnHtml += ", " + RuleCopyCaption + "&nbsp;" + inputText(TagName + "." + CheckBoxCnt + ".RuleCopy", RuleCopy, 1, 20);
                                        }
                                        returnHtml += "</td></tr></table>";
                                        CheckBoxCnt = CheckBoxCnt + 1;
                                        DivCheckBoxCnt = DivCheckBoxCnt + 1;
                                    }
                                    core.db.csGoNext(CS);
                                }
                                returnHtml += EndDiv;
                                returnHtml += "<input type=\"hidden\" name=\"" + TagName + ".RowCount\" value=\"" + CheckBoxCnt + "\">\r\n";
                            }
                        }
                        core.db.csClose(ref CS);
                        addScriptCode(javaScriptRequired, "CheckList Categories");
                    }
                    //End If
                    core.doc.checkListCnt = core.doc.checkListCnt + 1;
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnHtml;
        }
        //
        //========================================================================
        // main_GetRecordEditLink2( iContentName, iRecordID, AllowCut, RecordName )
        //
        //   ContentName The content for this link
        //   RecordID    The ID of the record in the Table
        //   AllowCut
        //   RecordName
        //   IsEditing
        //========================================================================
        //
        public string getRecordEditLink2(string ContentName, int RecordID, bool AllowCut, string RecordName, bool IsEditing) {
            string tempmain_GetRecordEditLink2 = null;
            try {
                int ContentID = 0;
                string iContentName = null;
                int iRecordID = 0;
                string WorkingLink = null;
                bool iAllowCut = false;
                string ContentCaption = null;
                //
                iContentName = genericController.encodeText(ContentName);
                iRecordID = genericController.encodeInteger(RecordID);
                iAllowCut = genericController.encodeBoolean(AllowCut);
                ContentCaption = genericController.encodeHTML(iContentName);
                if (genericController.vbLCase(ContentCaption) == "aggregate functions") {
                    ContentCaption = "Add-on";
                }
                if (genericController.vbLCase(ContentCaption) == "aggregate function objects") {
                    ContentCaption = "Add-on";
                }
                ContentCaption = ContentCaption + " record";
                if (!string.IsNullOrEmpty(RecordName)) {
                    ContentCaption = ContentCaption + ", named '" + RecordName + "'";
                }
                //
                tempmain_GetRecordEditLink2 = "";
                if (string.IsNullOrEmpty(iContentName)) {
                    throw (new ApplicationException("ContentName [" + ContentName + "] is invalid")); // handleLegacyError14(MethodName, "")
                } else {
                    if (iRecordID < 1) {
                        throw (new ApplicationException("RecordID [" + RecordID + "] is invalid")); // handleLegacyError14(MethodName, "")
                    } else {
                        if (IsEditing) {
                            //
                            // Edit link, main_Get the CID
                            //
                            ContentID = Models.Complex.cdefModel.getContentId(core, iContentName);
                            //
                            tempmain_GetRecordEditLink2 = tempmain_GetRecordEditLink2 + "<a"
                                + " class=\"ccRecordEditLink\" "
                                + " TabIndex=-1"
                                + " href=\"" + genericController.encodeHTML("/" + core.appConfig.adminRoute + "?cid=" + ContentID + "&id=" + iRecordID + "&af=4&aa=2&ad=1") + "\"";
                            tempmain_GetRecordEditLink2 = tempmain_GetRecordEditLink2 + "><img"
                                + " src=\"/ccLib/images/IconContentEdit.gif\""
                                + " border=\"0\""
                                + " alt=\"Edit this " + genericController.encodeHTML(ContentCaption) + "\""
                                + " title=\"Edit this " + genericController.encodeHTML(ContentCaption) + "\""
                                + " align=\"absmiddle\""
                                + "></a>";
                            //
                            // Cut Link if enabled
                            //
                            if (iAllowCut) {
                                WorkingLink = genericController.modifyLinkQuery(core.webServer.requestPage + "?" + core.doc.refreshQueryString, RequestNameCut, genericController.encodeText(ContentID) + "." + genericController.encodeText(RecordID), true);
                                tempmain_GetRecordEditLink2 = ""
                                    + tempmain_GetRecordEditLink2 + "<a class=\"ccRecordCutLink\" TabIndex=\"-1\" href=\"" + genericController.encodeHTML(WorkingLink) + "\"><img src=\"/ccLib/images/Contentcut.gif\" border=\"0\" alt=\"Cut this " + ContentCaption + " to clipboard\" title=\"Cut this " + ContentCaption + " to clipboard\" align=\"absmiddle\"></a>";
                            }
                            //
                            // Help link if enabled
                            //
                            string helpLink = "";
                            //helpLink = main_GetHelpLink(5, "Editing " & ContentCaption, "Turn on Edit icons by checking 'Edit' in the tools panel, and click apply.<br><br><img src=""/ccLib/images/IconContentEdit.gif"" style=""vertical-align:middle""> Edit-Content icon<br><br>Edit-Content icons appear in your content. Click them to edit your content.")
                            tempmain_GetRecordEditLink2 = ""
                                + tempmain_GetRecordEditLink2 + helpLink;
                            //
                            tempmain_GetRecordEditLink2 = "<span class=\"ccRecordLinkCon\" style=\"white-space:nowrap;\">" + tempmain_GetRecordEditLink2 + "</span>";
                            //
                            //main_GetRecordEditLink2 = "" _
                            //    & cr & "<div style=""position:absolute;"">" _
                            //    & genericController.kmaIndent(main_GetRecordEditLink2) _
                            //    & cr & "</div>"
                            //
                            //main_GetRecordEditLink2 = "" _
                            //    & cr & "<div style=""position:relative;display:inline;"">" _
                            //    & genericController.kmaIndent(main_GetRecordEditLink2) _
                            //    & cr & "</div>"
                        }

                    }
                }
                //
                return tempmain_GetRecordEditLink2;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError18(MethodName)
            //
            return tempmain_GetRecordEditLink2;
        }
        //
        //========================================================================
        // Print an add link for the current ContentSet
        //   iCSPointer is the content set to be added to
        //   PresetNameValueList is a name=value pair to force in the added record
        //========================================================================
        //
        public string cs_getRecordAddLink(int CSPointer, string PresetNameValueList = "", bool AllowPaste = false) {
            string tempmain_cs_getRecordAddLink = null;
            try {
                //
                //If Not (true) Then Exit Function
                //
                string ContentName = null;
                string iPresetNameValueList = null;
                int iCSPointer;
                //
                iCSPointer = genericController.encodeInteger(CSPointer);
                iPresetNameValueList = genericController.encodeEmptyText(PresetNameValueList, "");
                //
                if (iCSPointer < 0) {
                    throw (new ApplicationException("invalid ContentSet pointer [" + iCSPointer + "]")); // handleLegacyError14(MethodName, "main_cs_getRecordAddLink was called with ")
                } else {
                    //
                    // Print an add tag to the iCSPointers Content
                    //
                    ContentName = core.db.cs_getContentName(iCSPointer);
                    if (string.IsNullOrEmpty(ContentName)) {
                        throw (new ApplicationException("main_cs_getRecordAddLink was called with a ContentSet that was created with an SQL statement. The function requires a ContentSet opened with an OpenCSContent.")); // handleLegacyError14(MethodName, "")
                    } else {
                        tempmain_cs_getRecordAddLink = getRecordAddLink(ContentName, iPresetNameValueList, AllowPaste);
                    }
                }
                return tempmain_cs_getRecordAddLink;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError18(MethodName)
            //
            return tempmain_cs_getRecordAddLink;
        }
        //
        //========================================================================
        // main_GetRecordAddLink( iContentName, iPresetNameValueList )
        //
        //   Returns a string of add tags for the Content Definition included, and all
        //   child contents of that area.
        //
        //   iContentName The content for this link
        //   iPresetNameValueList The sql equivalent used to select the record.
        //           translates to name0=value0,name1=value1.. pairs separated by ,
        //
        //   LowestRootMenu - The Menu in the flyout structure that is the furthest down
        //   in the chain that the user has content access to. This is so a content manager
        //   does not have to navigate deep into a structure to main_Get to content he can
        //   edit.
        //   Basically, the entire menu is created down from the MenuName, and populated
        //   with all the entiries this user has access to. The LowestRequiredMenuName is
        //   is returned from the _branch routine, and that is to root on-which the
        //   main_GetMenu uses
        //========================================================================
        //
        public string getRecordAddLink(string ContentName, string PresetNameValueList, bool AllowPaste = false) {
            return getRecordAddLink2(genericController.encodeText(ContentName), genericController.encodeText(PresetNameValueList), AllowPaste, core.doc.sessionContext.isEditing(ContentName));
        }
        //
        //========================================================================
        // main_GetRecordAddLink2
        //
        //   Returns a string of add tags for the Content Definition included, and all
        //   child contents of that area.
        //
        //   iContentName The content for this link
        //   iPresetNameValueList The sql equivalent used to select the record.
        //           translates to name0=value0,name1=value1.. pairs separated by ,
        //
        //   LowestRootMenu - The Menu in the flyout structure that is the furthest down
        //   in the chain that the user has content access to. This is so a content manager
        //   does not have to navigate deep into a structure to main_Get to content he can
        //   edit.
        //   Basically, the entire menu is created down from the MenuName, and populated
        //   with all the entiries this user has access to. The LowestRequiredMenuName is
        //   is returned from the _branch routine, and that is to root on-which the
        //   main_GetMenu uses
        //========================================================================
        //
        public string getRecordAddLink2(string ContentName, string PresetNameValueList, bool AllowPaste, bool IsEditing) {
            string tempmain_GetRecordAddLink2 = null;
            try {
                int ParentID = 0;
                string BufferString = null;
                string iContentName = null;
                int iContentID = 0;
                string iPresetNameValueList = null;
                string MenuName = null;
                string LowestRequiredMenuName = "";
                string ClipBoard = null;
                string PasteLink = "";
                int Position = 0;
                string[] ClipBoardArray = null;
                int ClipboardContentID = 0;
                int ClipChildRecordID = 0;
                bool iAllowPaste = false;
                bool useFlyout = false;
                int csChildContent = 0;
                string Link = null;
                //
                tempmain_GetRecordAddLink2 = "";
                if (IsEditing) {
                    iContentName = genericController.encodeText(ContentName);
                    iPresetNameValueList = genericController.encodeText(PresetNameValueList);
                    iPresetNameValueList = genericController.vbReplace(iPresetNameValueList, "&", ",");
                    iAllowPaste = genericController.encodeBoolean(AllowPaste);

                    if (string.IsNullOrEmpty(iContentName)) {
                        throw (new ApplicationException("Method called with blank ContentName")); // handleLegacyError14(MethodName, "")
                    } else {
                        iContentID = Models.Complex.cdefModel.getContentId(core, iContentName);
                        csChildContent = core.db.csOpen("Content", "ParentID=" + iContentID, "", true, 0, false, false, "id");
                        useFlyout = core.db.csOk(csChildContent);
                        core.db.csClose(ref csChildContent);
                        //
                        if (!useFlyout) {
                            Link = "/" + core.appConfig.adminRoute + "?cid=" + iContentID + "&af=4&aa=2&ad=1";
                            if (!string.IsNullOrEmpty(PresetNameValueList)) {
                                Link = Link + "&wc=" + genericController.EncodeRequestVariable(PresetNameValueList);
                            }
                            tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + "<a"
                                + " TabIndex=-1"
                                + " href=\"" + genericController.encodeHTML(Link) + "\"";
                            tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + "><img"
                                + " src=\"/ccLib/images/IconContentAdd.gif\""
                                + " border=\"0\""
                                + " alt=\"Add record\""
                                + " title=\"Add record\""
                                + " align=\"absmiddle\""
                                + "></a>";
                        } else {
                            //
                            MenuName = genericController.GetRandomInteger(core).ToString();
                            core.menuFlyout.menu_AddEntry(MenuName, "", "/ccLib/images/IconContentAdd.gif", "", "", "", "stylesheet", "stylesheethover");
                            LowestRequiredMenuName = getRecordAddLink_AddMenuEntry(iContentName, iPresetNameValueList, "", MenuName, MenuName);
                        }
                        //
                        // Add in the paste entry, if needed
                        //
                        if (iAllowPaste) {
                            ClipBoard = core.visitProperty.getText("Clipboard", "");
                            if (!string.IsNullOrEmpty(ClipBoard)) {
                                Position = genericController.vbInstr(1, ClipBoard, ".");
                                if (Position != 0) {
                                    ClipBoardArray = ClipBoard.Split('.');
                                    if (ClipBoardArray.GetUpperBound(0) > 0) {
                                        ClipboardContentID = genericController.encodeInteger(ClipBoardArray[0]);
                                        ClipChildRecordID = genericController.encodeInteger(ClipBoardArray[1]);
                                        //iContentID = main_GetContentID(iContentName)
                                        if (Models.Complex.cdefModel.isWithinContent(core, ClipboardContentID, iContentID)) {
                                            if (genericController.vbInstr(1, iPresetNameValueList, "PARENTID=", 1) != 0) {
                                                //
                                                // must test for main_IsChildRecord
                                                //
                                                BufferString = iPresetNameValueList;
                                                BufferString = genericController.vbReplace(BufferString, "(", "");
                                                BufferString = genericController.vbReplace(BufferString, ")", "");
                                                BufferString = genericController.vbReplace(BufferString, ",", "&");
                                                ParentID = genericController.encodeInteger(genericController.main_GetNameValue_Internal(core, BufferString, "Parentid"));
                                            }


                                            if ((ParentID != 0) & (!pageContentController.isChildRecord(core, iContentName, ParentID, ClipChildRecordID))) {
                                                //
                                                // Can not paste as child of itself
                                                //
                                                PasteLink = core.webServer.requestPage + "?" + core.doc.refreshQueryString;
                                                PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePaste, "1", true);
                                                PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteParentContentID, iContentID.ToString(), true);
                                                PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteParentRecordID, ParentID.ToString(), true);
                                                PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteFieldList, iPresetNameValueList, true);
                                                tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + "<a class=\"ccRecordCutLink\" TabIndex=\"-1\" href=\"" + genericController.encodeHTML(PasteLink) + "\"><img src=\"/ccLib/images/ContentPaste.gif\" border=\"0\" alt=\"Paste record in clipboard here\" title=\"Paste record in clipboard here\" align=\"absmiddle\"></a>";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //
                        // Add in the available flyout Navigator Entries
                        //
                        if (!string.IsNullOrEmpty(LowestRequiredMenuName)) {
                            tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + core.menuFlyout.getMenu(LowestRequiredMenuName, 0);
                            tempmain_GetRecordAddLink2 = genericController.vbReplace(tempmain_GetRecordAddLink2, "class=\"ccFlyoutButton\" ", "", 1, 99, 1);
                            if (!string.IsNullOrEmpty(PasteLink)) {
                                tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + "<a TabIndex=-1 href=\"" + genericController.encodeHTML(PasteLink) + "\"><img src=\"/ccLib/images/ContentPaste.gif\" border=\"0\" alt=\"Paste content from clipboard\" align=\"absmiddle\"></a>";
                            }
                        }
                        //
                        // Help link if enabled
                        //
                        string helpLink = "";
                        //helpLink = main_GetHelpLink(6, "Adding " & iContentName, "Turn on Edit icons by checking 'Edit' in the tools panel, and click apply.<br><br><img src=""/ccLib/images/IconContentAdd.gif"" " & IconWidthHeight & " style=""vertical-align:middle""> Add-Content icon<br><br>Add-Content icons appear in your content. Click them to add content.")
                        tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + helpLink;
                        if (!string.IsNullOrEmpty(tempmain_GetRecordAddLink2)) {
                            tempmain_GetRecordAddLink2 = ""
                                + "\r\n\t<div style=\"display:inline;\">"
                                + genericController.htmlIndent(tempmain_GetRecordAddLink2) + "\r\n\t</div>";
                        }
                        //
                        // ----- Add the flyout panels to the content to return
                        //       This must be here so if the call is made after main_ClosePage, the panels will still deliver
                        //
                        if (!string.IsNullOrEmpty(LowestRequiredMenuName)) {
                            tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + core.menuFlyout.menu_GetClose();
                            if (genericController.vbInstr(1, tempmain_GetRecordAddLink2, "IconContentAdd.gif", 1) != 0) {
                                tempmain_GetRecordAddLink2 = genericController.vbReplace(tempmain_GetRecordAddLink2, "IconContentAdd.gif\" ", "IconContentAdd.gif\" align=\"absmiddle\" ");
                            }
                        }
                        tempmain_GetRecordAddLink2 = genericController.vbReplace(tempmain_GetRecordAddLink2, "target=", "xtarget=", 1, 99, 1);
                    }
                }
                //
                return tempmain_GetRecordAddLink2;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError18(MethodName)
            //
            return tempmain_GetRecordAddLink2;
        }
        //
        //========================================================================
        // main_GetRecordAddLink_AddMenuEntry( ContentName, PresetNameValueList, ContentNameList, MenuName )
        //
        //   adds an add entry for the content name, and all the child content
        //   returns the MenuName of the lowest branch that has valid
        //   Navigator Entries.
        //
        //   ContentName The content for this link
        //   PresetNameValueList The sql equivalent used to select the record.
        //           translates to (name0=value0)&(name1=value1).. pairs separated by &
        //   ContentNameList is a comma separated list of names of the content included so far
        //   MenuName is the name of the root branch, for flyout menu
        //
        //   IsMember(), main_IsAuthenticated() And Member_AllowLinkAuthoring must already be checked
        //========================================================================
        //
        private string getRecordAddLink_AddMenuEntry(string ContentName, string PresetNameValueList, string ContentNameList, string MenuName, string ParentMenuName) {
            string result = "";
            string Copy = null;
            int CS = 0;
            string SQL = null;
            int csChildContent = 0;
            int ContentID = 0;
            string Link = null;
            string MyContentNameList = null;
            string ButtonCaption = null;
            bool ContentRecordFound = false;
            bool ContentAllowAdd = false;
            bool GroupRulesAllowAdd = false;
            DateTime MemberRulesDateExpires = default(DateTime);
            bool MemberRulesAllow = false;
            int ChildMenuButtonCount = 0;
            string ChildMenuName = null;
            string ChildContentName = null;
            //
            Link = "";
            MyContentNameList = ContentNameList;
            if (string.IsNullOrEmpty(ContentName)) {
                throw (new ApplicationException("main_GetRecordAddLink, ContentName is empty")); // handleLegacyError14(MethodName, "")
            } else {
                if (MyContentNameList.IndexOf("," + genericController.vbUCase(ContentName) + ",") + 1 >= 0) {
                    throw (new ApplicationException("result , Content Child [" + ContentName + "] is one of its own parents")); // handleLegacyError14(MethodName, "")
                } else {
                    MyContentNameList = MyContentNameList + "," + genericController.vbUCase(ContentName) + ",";
                    //
                    // ----- Select the Content Record for the Menu Entry selected
                    //
                    ContentRecordFound = false;
                    if (core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                        //
                        // ----- admin member, they have access, main_Get ContentID and set markers true
                        //
                        SQL = "SELECT ID as ContentID, AllowAdd as ContentAllowAdd, 1 as GroupRulesAllowAdd, null as MemberRulesDateExpires"
                            + " FROM ccContent"
                            + " WHERE ("
                            + " (ccContent.Name=" + core.db.encodeSQLText(ContentName) + ")"
                            + " AND(ccContent.active<>0)"
                            + " );";
                        CS = core.db.csOpenSql(SQL);
                        if (core.db.csOk(CS)) {
                            //
                            // Entry was found
                            //
                            ContentRecordFound = true;
                            ContentID = core.db.csGetInteger(CS, "ContentID");
                            ContentAllowAdd = core.db.csGetBoolean(CS, "ContentAllowAdd");
                            GroupRulesAllowAdd = true;
                            MemberRulesDateExpires = DateTime.MinValue;
                            MemberRulesAllow = true;
                        }
                        core.db.csClose(ref CS);
                    } else {
                        //
                        // non-admin member, first check if they have access and main_Get true markers
                        //
                        SQL = "SELECT ccContent.ID as ContentID, ccContent.AllowAdd as ContentAllowAdd, ccGroupRules.AllowAdd as GroupRulesAllowAdd, ccMemberRules.DateExpires as MemberRulesDateExpires"
                            + " FROM (((ccContent"
                                + " LEFT JOIN ccGroupRules ON ccGroupRules.ContentID=ccContent.ID)"
                                + " LEFT JOIN ccgroups ON ccGroupRules.GroupID=ccgroups.ID)"
                                + " LEFT JOIN ccMemberRules ON ccgroups.ID=ccMemberRules.GroupID)"
                                + " LEFT JOIN ccMembers ON ccMemberRules.MemberID=ccMembers.ID"
                            + " WHERE ("
                            + " (ccContent.Name=" + core.db.encodeSQLText(ContentName) + ")"
                            + " AND(ccContent.active<>0)"
                            + " AND(ccGroupRules.active<>0)"
                            + " AND(ccMemberRules.active<>0)"
                            + " AND((ccMemberRules.DateExpires is Null)or(ccMemberRules.DateExpires>" + core.db.encodeSQLDate(core.doc.profileStartTime) + "))"
                            + " AND(ccgroups.active<>0)"
                            + " AND(ccMembers.active<>0)"
                            + " AND(ccMembers.ID=" + core.doc.sessionContext.user.id + ")"
                            + " );";
                        CS = core.db.csOpenSql(SQL);
                        if (core.db.csOk(CS)) {
                            //
                            // ----- Entry was found, member has some kind of access
                            //
                            ContentRecordFound = true;
                            ContentID = core.db.csGetInteger(CS, "ContentID");
                            ContentAllowAdd = core.db.csGetBoolean(CS, "ContentAllowAdd");
                            GroupRulesAllowAdd = core.db.csGetBoolean(CS, "GroupRulesAllowAdd");
                            MemberRulesDateExpires = core.db.csGetDate(CS, "MemberRulesDateExpires");
                            MemberRulesAllow = false;
                            if (MemberRulesDateExpires == DateTime.MinValue) {
                                MemberRulesAllow = true;
                            } else if (MemberRulesDateExpires > core.doc.profileStartTime) {
                                MemberRulesAllow = true;
                            }
                        } else {
                            //
                            // ----- No entry found, this member does not have access, just main_Get ContentID
                            //
                            ContentRecordFound = true;
                            ContentID = Models.Complex.cdefModel.getContentId(core, ContentName);
                            ContentAllowAdd = false;
                            GroupRulesAllowAdd = false;
                            MemberRulesAllow = false;
                        }
                        core.db.csClose(ref CS);
                    }
                    if (ContentRecordFound) {
                        //
                        // Add the Menu Entry* to the current menu (MenuName)
                        //
                        Link = "";
                        ButtonCaption = ContentName;
                        result = MenuName;
                        if (ContentAllowAdd && GroupRulesAllowAdd && MemberRulesAllow) {
                            Link = "/" + core.appConfig.adminRoute + "?cid=" + ContentID + "&af=4&aa=2&ad=1";
                            if (!string.IsNullOrEmpty(PresetNameValueList)) {
                                string NameValueList = PresetNameValueList;
                                Link = Link + "&wc=" + genericController.EncodeRequestVariable(PresetNameValueList);
                            }
                        }
                        core.menuFlyout.menu_AddEntry(MenuName + ":" + ContentName, ParentMenuName, "", "", Link, ButtonCaption, "", "", true);
                        //
                        // Create child submenu if Child Entries found
                        //
                        csChildContent = core.db.csOpen("Content", "ParentID=" + ContentID, "", true, 0, false, false, "name");
                        if (!core.db.csOk(csChildContent)) {
                            //
                            // No child menu
                            //
                        } else {
                            //
                            // Add the child menu
                            //
                            ChildMenuName = MenuName + ":" + ContentName;
                            ChildMenuButtonCount = 0;
                            //
                            // ----- Create the ChildPanel with all Children found
                            //
                            while (core.db.csOk(csChildContent)) {
                                ChildContentName = core.db.csGetText(csChildContent, "name");
                                Copy = getRecordAddLink_AddMenuEntry(ChildContentName, PresetNameValueList, MyContentNameList, MenuName, ParentMenuName);
                                if (!string.IsNullOrEmpty(Copy)) {
                                    ChildMenuButtonCount = ChildMenuButtonCount + 1;
                                }
                                if ((string.IsNullOrEmpty(result)) && (!string.IsNullOrEmpty(Copy))) {
                                    result = Copy;
                                }
                                core.db.csGoNext(csChildContent);
                            }
                        }
                    }
                }
                core.db.csClose(ref csChildContent);
            }
            return result;
        }
        //
        //========================================================================
        //   main_GetPanel( Panel, Optional StylePanel, Optional StyleHilite, Optional StyleShadow, Optional Width, Optional Padding, Optional HeightMin) As String
        // Return a panel with the input as center
        //========================================================================
        //
        public string getPanel(string content, string StylePanel = "", string StyleHilite = "ccPanelHilite", string StyleShadow = "ccPanelShadow", string Width = "100%", int Padding = 5, int HeightMin = 1) {
            string result = null;
            string ContentPanelWidth = null;
            string MyStylePanel = null;
            string MyStyleHilite = null;
            string MyStyleShadow = null;
            string MyWidth = null;
            string MyPadding = null;
            string MyHeightMin = null;
            string s0 = null;
            string s1 = null;
            string s2 = null;
            string s3 = null;
            string s4 = null;
            string contentPanelWidthStyle = null;
            //
            MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel");
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite");
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow");
            MyWidth = genericController.encodeEmptyText(Width, "100%");
            MyPadding = Padding.ToString();
            MyHeightMin = HeightMin.ToString();
            //
            if (MyWidth.IsNumeric()) {
                ContentPanelWidth = (int.Parse(MyWidth) - 2).ToString();
                contentPanelWidthStyle = ContentPanelWidth + "px";
            } else {
                ContentPanelWidth = "100%";
                contentPanelWidthStyle = ContentPanelWidth;
            }
            //
            //
            //
            s0 = ""
                + "\r<td style=\"padding:" + MyPadding + "px;vertical-align:top\" class=\"" + MyStylePanel + "\">"
                + genericController.htmlIndent(genericController.encodeText(content))
                + "\r</td>"
                + "";
            //
            s1 = ""
                + "\r<tr>"
                + genericController.htmlIndent(s0)
                + "\r</tr>"
                + "";
            s2 = ""
                + "\r<table style=\"width:" + contentPanelWidthStyle + ";border:0px;\" class=\"" + MyStylePanel + "\" cellspacing=\"0\">"
                + genericController.htmlIndent(s1)
                + "\r</table>"
                + "";
            s3 = ""
                + "\r<td colspan=\"3\" width=\"" + ContentPanelWidth + "\" valign=\"top\" align=\"left\" class=\"" + MyStylePanel + "\">"
                + genericController.htmlIndent(s2)
                + "\r</td>"
                + "";
            s4 = ""
                + "\r<tr>"
                + genericController.htmlIndent(s3)
                + "\r</tr>"
                + "";
            result = ""
                + "\r<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"" + MyWidth + "\" class=\"" + MyStylePanel + "\">"
                + genericController.htmlIndent(s4)
                + "\r</table>"
                + "";
            return result;
        }
        //
        //========================================================================
        //   main_GetPanel( Panel, Optional StylePanel, Optional StyleHilite, Optional StyleShadow, Optional Width, Optional Padding, Optional HeightMin) As String
        // Return a panel with the input as center
        //========================================================================
        //
        public string getReversePanel(string Panel, string StylePanel = "", string StyleHilite = "ccPanelShadow", string StyleShadow = "ccPanelHilite", string Width = "", string Padding = "", string HeightMin = "") {
            string MyStyleHilite = null;
            string MyStyleShadow = null;
            //
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelShadow");
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelHilite");

            return getPanelTop(StylePanel, MyStyleHilite, MyStyleShadow, Width, Padding, HeightMin) + genericController.encodeText(Panel) + getPanelBottom(StylePanel, MyStyleHilite, MyStyleShadow, Width, Padding);
        }
        //
        //========================================================================
        // Return a panel header with the header message reversed out of the left
        //========================================================================
        //
        public string getPanelHeader(string HeaderMessage, string RightSideMessage = "") {
            string iHeaderMessage = null;
            string iRightSideMessage = null;
            adminUIController Adminui = new adminUIController(core);
            //
            //If Not (true) Then Exit Function
            //
            iHeaderMessage = genericController.encodeText(HeaderMessage);
            iRightSideMessage = genericController.encodeEmptyText(RightSideMessage, core.doc.profileStartTime.ToString("G"));
            return Adminui.GetHeader(iHeaderMessage, iRightSideMessage);
        }

        //
        //========================================================================
        // Prints the top of display panel
        //   Must be closed with PrintPanelBottom
        //========================================================================
        //
        public string getPanelTop(string StylePanel = "", string StyleHilite = "", string StyleShadow = "", string Width = "", string Padding = "", string HeightMin = "") {
            string tempmain_GetPanelTop = null;
            string ContentPanelWidth = null;
            string MyStylePanel = null;
            string MyStyleHilite = null;
            string MyStyleShadow = null;
            string MyWidth = null;
            string MyPadding = null;
            string MyHeightMin = null;
            //
            tempmain_GetPanelTop = "";
            MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel");
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite");
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow");
            MyWidth = genericController.encodeEmptyText(Width, "100%");
            MyPadding = genericController.encodeEmptyText(Padding, "5");
            MyHeightMin = genericController.encodeEmptyText(HeightMin, "1");
            if (MyWidth.IsNumeric()) {
                ContentPanelWidth = (int.Parse(MyWidth) - 2).ToString();
            } else {
                ContentPanelWidth = "100%";
            }
            tempmain_GetPanelTop = tempmain_GetPanelTop + "\r<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"" + MyWidth + "\" class=\"" + MyStylePanel + "\">";
            //
            // --- center row with Panel
            //
            tempmain_GetPanelTop = tempmain_GetPanelTop
                + cr2 + "<tr>"
                + cr3 + "<td colspan=\"3\" width=\"" + ContentPanelWidth + "\" valign=\"top\" align=\"left\" class=\"" + MyStylePanel + "\">"
                + cr4 + "<table border=\"0\" cellpadding=\"" + MyPadding + "\" cellspacing=\"0\" width=\"" + ContentPanelWidth + "\" class=\"" + MyStylePanel + "\">"
                + cr5 + "<tr>"
                + cr6 + "<td valign=\"top\" class=\"" + MyStylePanel + "\"><Span class=\"" + MyStylePanel + "\">";
            return tempmain_GetPanelTop;
        }
        //
        //========================================================================
        // Return a panel with the input as center
        //========================================================================
        //
        public string getPanelBottom(string StylePanel = "", string StyleHilite = "", string StyleShadow = "", string Width = "", string Padding = "") {
            string result = "";
            try {
                result = result + cr6 + "</span></td>"
                    + cr5 + "</tr>"
                    + cr4 + "</table>"
                    + cr3 + "</td>"
                    + cr2 + "</tr>"
                    + "\r</table>";
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        public string getPanelButtons(string ButtonValueList, string ButtonName, string PanelWidth = "", string PanelHeightMin = "") {
            adminUIController Adminui = new adminUIController(core);
            return Adminui.GetButtonBar(Adminui.GetButtonsFromList(ButtonValueList, true, true, ButtonName), "");
        }
        //
        //
        //
        public string getPanelInput(string PanelContent, string PanelWidth = "", string PanelHeightMin = "1") {
            return getPanel(PanelContent, "ccPanelInput", "ccPanelShadow", "ccPanelHilite", PanelWidth, 2, genericController.encodeInteger(PanelHeightMin));
        }
        //
        //========================================================================
        // Print the tools panel at the bottom of the page
        //========================================================================
        //
        public string getToolsPanel() {
            string result = "";
            try {
                string DebugPanel = "";
                string Copy = null;
                string EditTagID = null;
                string QuickEditTagID = null;
                string AdvancedEditTagID = null;
                string Tag = null;
                string TagID = null;
                stringBuilderLegacyController ToolsPanel = null;
                string OptionsPanel = "";
                stringBuilderLegacyController LinkPanel = null;
                string LoginPanel = "";
                bool iValueBoolean = false;
                string WorkingQueryString = null;
                adminUIController Adminui = new adminUIController(core);
                bool ShowLegacyToolsPanel = false;
                string QS = null;
                //
                if (core.doc.sessionContext.user.AllowToolsPanel) {
                    ShowLegacyToolsPanel = core.siteProperties.getBoolean("AllowLegacyToolsPanel", true);
                    //
                    // --- Link Panel - used for both Legacy Tools Panel, and without it
                    //
                    LinkPanel = new stringBuilderLegacyController();
                    LinkPanel.Add(SpanClassAdminSmall);
                    LinkPanel.Add("Contensive " + core.codeVersion() + " | ");
                    LinkPanel.Add(core.doc.profileStartTime.ToString("G") + " | ");
                    LinkPanel.Add("<a class=\"ccAdminLink\" target=\"_blank\" href=\"http://support.Contensive.com/\">Support</A> | ");
                    LinkPanel.Add("<a class=\"ccAdminLink\" href=\"" + genericController.encodeHTML("/" + core.appConfig.adminRoute) + "\">Admin Home</A> | ");
                    LinkPanel.Add("<a class=\"ccAdminLink\" href=\"" + genericController.encodeHTML("http://" + core.webServer.requestDomain) + "\">Public Home</A> | ");
                    LinkPanel.Add("<a class=\"ccAdminLink\" target=\"_blank\" href=\"" + genericController.encodeHTML("/" + core.appConfig.adminRoute + "?" + RequestNameHardCodedPage + "=" + HardCodedPageMyProfile) + "\">My Profile</A> | ");
                    if (core.siteProperties.getBoolean("AllowMobileTemplates", false)) {
                        if (core.doc.sessionContext.visit.Mobile) {
                            QS = core.doc.refreshQueryString;
                            QS = genericController.ModifyQueryString(QS, "method", "forcenonmobile");
                            LinkPanel.Add("<a class=\"ccAdminLink\" href=\"?" + QS + "\">Non-Mobile Version</A> | ");
                        } else {
                            QS = core.doc.refreshQueryString;
                            QS = genericController.ModifyQueryString(QS, "method", "forcemobile");
                            LinkPanel.Add("<a class=\"ccAdminLink\" href=\"?" + QS + "\">Mobile Version</A> | ");
                        }
                    }
                    LinkPanel.Add("</span>");
                    //
                    if (ShowLegacyToolsPanel) {
                        ToolsPanel = new stringBuilderLegacyController();
                        WorkingQueryString = genericController.ModifyQueryString(core.doc.refreshQueryString, "ma", "", false);
                        //
                        // ----- Tools Panel Caption
                        //
                        string helpLink = "";
                        //helpLink = main_GetHelpLink("2", "Contensive Tools Panel", BubbleCopy)
                        result += getPanelHeader("Contensive Tools Panel" + helpLink);
                        //
                        ToolsPanel.Add(core.html.formStart(WorkingQueryString));
                        ToolsPanel.Add(core.html.inputHidden("Type", FormTypeToolsPanel));
                        //
                        if (true) {
                            //
                            // ----- Create the Options Panel
                            //
                            //PathsContentID = main_GetContentID("Paths")
                            //                '
                            //                ' Allow Help Links
                            //                '
                            //                iValueBoolean = visitProperty.getboolean("AllowHelpIcon")
                            //                TagID =  "AllowHelpIcon"
                            //                OptionsPanel = OptionsPanel & "" _
                            //                    & CR & "<div class=""ccAdminSmall"">" _
                            //                    & cr2 & "<LABEL for=""" & TagID & """>" & main_GetFormInputCheckBox2(TagID, iValueBoolean, TagID) & "&nbsp;Help</LABEL>" _
                            //                    & CR & "</div>"
                            //
                            EditTagID = "AllowEditing";
                            QuickEditTagID = "AllowQuickEditor";
                            AdvancedEditTagID = "AllowAdvancedEditor";
                            //
                            // Edit
                            //
                            helpLink = "";
                            //helpLink = main_GetHelpLink(7, "Enable Editing", "Display the edit tools for basic content, such as pages, copy and sections. ")
                            iValueBoolean = core.visitProperty.getBoolean("AllowEditing");
                            Tag = core.html.inputCheckbox(EditTagID, iValueBoolean, EditTagID);
                            Tag = genericController.vbReplace(Tag, ">", " onClick=\"document.getElementById('" + QuickEditTagID + "').checked=false;document.getElementById('" + AdvancedEditTagID + "').checked=false;\">");
                            OptionsPanel = OptionsPanel + "\r<div class=\"ccAdminSmall\">"
                            + cr2 + "<LABEL for=\"" + EditTagID + "\">" + Tag + "&nbsp;Edit</LABEL>" + helpLink + "\r</div>";
                            //
                            // Quick Edit
                            //
                            helpLink = "";
                            //helpLink = main_GetHelpLink(8, "Enable Quick Edit", "Display the quick editor to edit the main page content.")
                            iValueBoolean = core.visitProperty.getBoolean("AllowQuickEditor");
                            Tag = core.html.inputCheckbox(QuickEditTagID, iValueBoolean, QuickEditTagID);
                            Tag = genericController.vbReplace(Tag, ">", " onClick=\"document.getElementById('" + EditTagID + "').checked=false;document.getElementById('" + AdvancedEditTagID + "').checked=false;\">");
                            OptionsPanel = OptionsPanel + "\r<div class=\"ccAdminSmall\">"
                            + cr2 + "<LABEL for=\"" + QuickEditTagID + "\">" + Tag + "&nbsp;Quick Edit</LABEL>" + helpLink + "\r</div>";
                            //
                            // Advanced Edit
                            //
                            helpLink = "";
                            //helpLink = main_GetHelpLink(0, "Enable Advanced Edit", "Display the edit tools for advanced content, such as templates and add-ons. Basic content edit tools are also displayed.")
                            iValueBoolean = core.visitProperty.getBoolean("AllowAdvancedEditor");
                            Tag = core.html.inputCheckbox(AdvancedEditTagID, iValueBoolean, AdvancedEditTagID);
                            Tag = genericController.vbReplace(Tag, ">", " onClick=\"document.getElementById('" + QuickEditTagID + "').checked=false;document.getElementById('" + EditTagID + "').checked=false;\">");
                            OptionsPanel = OptionsPanel + "\r<div class=\"ccAdminSmall\">"
                            + cr2 + "<LABEL for=\"" + AdvancedEditTagID + "\">" + Tag + "&nbsp;Advanced Edit</LABEL>" + helpLink + "\r</div>";
                            //
                            // Workflow Authoring Render Mode
                            //
                            helpLink = "";
                            //helpLink = main_GetHelpLink(9, "Enable Workflow Rendering", "Control the display of workflow rendering. With workflow rendering enabled, any changes saved to content records that have not been published will be visible for your review.")
                            //If core.siteProperties.allowWorkflowAuthoring Then
                            //    iValueBoolean = core.visitProperty.getBoolean("AllowWorkflowRendering")
                            //    Tag = core.html.html_GetFormInputCheckBox2(WorkflowTagID, iValueBoolean, WorkflowTagID)
                            //    OptionsPanel = OptionsPanel _
                            //    & cr & "<div class=""ccAdminSmall"">" _
                            //    & cr2 & "<LABEL for=""" & WorkflowTagID & """>" & Tag & "&nbsp;Render Workflow Authoring Changes</LABEL>" & helpLink _
                            //    & cr & "</div>"
                            //End If
                            helpLink = "";
                            iValueBoolean = core.visitProperty.getBoolean("AllowDebugging");
                            TagID = "AllowDebugging";
                            Tag = core.html.inputCheckbox(TagID, iValueBoolean, TagID);
                            OptionsPanel = OptionsPanel + "\r<div class=\"ccAdminSmall\">"
                            + cr2 + "<LABEL for=\"" + TagID + "\">" + Tag + "&nbsp;Debug</LABEL>" + helpLink + "\r</div>";
                            //
                            // Create Path Block Row
                            //
                            //If core.doc.authContext.isAuthenticatedDeveloper(core) Then
                            //    TagID = "CreatePathBlock"
                            //    If core.siteProperties.allowPathBlocking Then
                            //        '
                            //        ' Path blocking allowed
                            //        '
                            //        'OptionsPanel = OptionsPanel & SpanClassAdminSmall & "<LABEL for=""" & TagID & """>"
                            //        CS = core.db.cs_open("Paths", "name=" & core.db.encodeSQLText(core.webServer.requestPath), , , , , , "ID")
                            //        If core.db.cs_ok(CS) Then
                            //            PathID = (core.db.cs_getInteger(CS, "ID"))
                            //        End If
                            //        Call core.db.cs_Close(CS)
                            //        If PathID <> 0 Then
                            //            '
                            //            ' Path is blocked
                            //            '
                            //            Tag = core.html.html_GetFormInputCheckBox2(TagID, True, TagID) & "&nbsp;Path is blocked [" & core.webServer.requestPath & "] [<a href=""" & genericController.encodeHTML("/" & core.appConfig.adminRoute & "?af=" & AdminFormEdit & "&id=" & PathID & "&cid=" & Models.Complex.cdefModel.getContentId(core,"paths") & "&ad=1") & """ target=""_blank"">edit</a>]</LABEL>"
                            //        Else
                            //            '
                            //            ' Path is not blocked
                            //            '
                            //            Tag = core.html.html_GetFormInputCheckBox2(TagID, False, TagID) & "&nbsp;Block this path [" & core.webServer.requestPath & "]</LABEL>"
                            //        End If
                            //        helpLink = ""
                            //        'helpLink = main_GetHelpLink(10, "Enable Debugging", "Debugging is a developer only debugging tool. With Debugging enabled, ccLib.TestPoints(...) will print, ErrorTrapping will be displayed, redirections are blocked, and more.")
                            //        OptionsPanel = OptionsPanel _
                            //        & cr & "<div class=""ccAdminSmall"">" _
                            //        & cr2 & "<LABEL for=""" & TagID & """>" & Tag & "</LABEL>" & helpLink _
                            //        & cr & "</div>"
                            //    End If
                            //End If
                            //
                            // Buttons
                            //
                            OptionsPanel = OptionsPanel + ""
                            + "\r<div class=\"ccButtonCon\">"
                            + cr2 + "<input type=submit name=mb value=\"" + ButtonApply + "\">"
                            + "\r</div>"
                            + "";
                        }
                        //
                        // ----- Create the Login Panel
                        //
                        if (string.IsNullOrEmpty(core.doc.sessionContext.user.name.Trim(' '))) {
                            Copy = "You are logged in as member #" + core.doc.sessionContext.user.id + ".";
                        } else {
                            Copy = "You are logged in as " + core.doc.sessionContext.user.name + ".";
                        }
                        LoginPanel = LoginPanel + ""
                        + "\r<div class=\"ccAdminSmall\">"
                        + cr2 + Copy + ""
                        + "\r</div>";
                        //
                        // Username
                        //
                        string Caption = null;
                        if (core.siteProperties.getBoolean("allowEmailLogin", false)) {
                            Caption = "Username&nbsp;or&nbsp;Email";
                        } else {
                            Caption = "Username";
                        }
                        TagID = "Username";
                        LoginPanel = LoginPanel + ""
                        + "\r<div class=\"ccAdminSmall\">"
                        + cr2 + "<LABEL for=\"" + TagID + "\">" + core.html.inputText(TagID, "", 1, 30, TagID, false) + "&nbsp;" + Caption + "</LABEL>"
                        + "\r</div>";
                        //
                        // Username
                        //
                        if (core.siteProperties.getBoolean("allownopasswordLogin", false)) {
                            Caption = "Password&nbsp;(optional)";
                        } else {
                            Caption = "Password";
                        }
                        TagID = "Password";
                        LoginPanel = LoginPanel + ""
                        + "\r<div class=\"ccAdminSmall\">"
                        + cr2 + "<LABEL for=\"" + TagID + "\">" + core.html.inputText(TagID, "", 1, 30, TagID, true) + "&nbsp;" + Caption + "</LABEL>"
                        + "\r</div>";
                        //
                        // Autologin checkbox
                        //
                        if (core.siteProperties.getBoolean("AllowAutoLogin", false)) {
                            if (core.doc.sessionContext.visit.CookieSupport) {
                                TagID = "autologin";
                                LoginPanel = LoginPanel + ""
                                + "\r<div class=\"ccAdminSmall\">"
                                + cr2 + "<LABEL for=\"" + TagID + "\">" + core.html.inputCheckbox(TagID, true, TagID) + "&nbsp;Login automatically from this computer</LABEL>"
                                + "\r</div>";
                            }
                        }
                        //
                        // Buttons
                        //
                        LoginPanel = LoginPanel + Adminui.GetButtonBar(Adminui.GetButtonsFromList(ButtonLogin + "," + ButtonLogout, true, true, "mb"), "");
                        //
                        // ----- assemble tools panel
                        //
                        Copy = ""
                        + "\r<td width=\"50%\" class=\"ccPanelInput\" style=\"vertical-align:bottom;\">"
                        + genericController.htmlIndent(LoginPanel) + "\r</td>"
                        + "\r<td width=\"50%\" class=\"ccPanelInput\" style=\"vertical-align:bottom;\">"
                        + genericController.htmlIndent(OptionsPanel) + "\r</td>";
                        Copy = ""
                        + "\r<tr>"
                        + genericController.htmlIndent(Copy) + "\r</tr>"
                        + "";
                        Copy = ""
                        + "\r<table border=\"0\" cellpadding=\"3\" cellspacing=\"0\" width=\"100%\">"
                        + genericController.htmlIndent(Copy) + "\r</table>";
                        ToolsPanel.Add(getPanelInput(Copy));
                        ToolsPanel.Add(core.html.formEnd());
                        result += getPanel(ToolsPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5);
                        //
                        result += getPanel(LinkPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5);
                        //
                        LinkPanel = null;
                        ToolsPanel = null;
                    }
                    //
                    // --- Developer Debug Panel
                    //
                    if (core.visitProperty.getBoolean("AllowDebugging")) {
                        //
                        // --- Debug Panel Header
                        //
                        LinkPanel = new stringBuilderLegacyController();
                        LinkPanel.Add(SpanClassAdminSmall);
                        //LinkPanel.Add( "WebClient " & main_WebClientVersion & " | "
                        LinkPanel.Add("Contensive " + core.codeVersion() + " | ");
                        LinkPanel.Add(core.doc.profileStartTime.ToString("G") + " | ");
                        LinkPanel.Add("<a class=\"ccAdminLink\" target=\"_blank\" href=\"http://support.Contensive.com/\">Support</A> | ");
                        LinkPanel.Add("<a class=\"ccAdminLink\" href=\"" + genericController.encodeHTML("/" + core.appConfig.adminRoute) + "\">Admin Home</A> | ");
                        LinkPanel.Add("<a class=\"ccAdminLink\" href=\"" + genericController.encodeHTML("http://" + core.webServer.requestDomain) + "\">Public Home</A> | ");
                        LinkPanel.Add("Render " + (Convert.ToSingle(core.doc.appStopWatch.ElapsedMilliseconds) / 1000).ToString("0.000") + " sec | ");
                        LinkPanel.Add("</span>");
                        //
                        DebugPanel +=  "\r<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">"
                            + cr2 + "<tr>"
                            + cr3 + "<td width=\"100\" class=\"ccPanel\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100\" height=\"1\" ></td>"
                            + cr3 + "<td width=\"100%\" class=\"ccPanel\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"1\" height=\"1\" ></td>"
                            + cr2 + "</tr>";
                        DebugPanel +=  "</table>";
                        //
                        if (ShowLegacyToolsPanel) {
                            //
                            // Debug Panel as part of legacy tools panel
                            //
                            result += getPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5);
                        } else {
                            //
                            // Debug Panel without Legacy Tools panel
                            //
                            result += getPanelHeader("Debug Panel") + getPanel(LinkPanel.Text) + getPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5);
                        }
                    }
                    result = "\r<div class=\"ccCon\">" + genericController.htmlIndent(result) + "\r</div>";
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //
        //
        private string getDebugPanelRow(string Label, string Value) {
            return cr2 + "<tr><td valign=\"top\" class=\"ccPanel ccAdminSmall\">" + Label + "</td><td valign=\"top\" class=\"ccPanel ccAdminSmall\">" + Value + "</td></tr>";
        }

        //
        //=================================================================================================================
        //   csv_GetAddonOptionStringValue
        //
        //   gets the value from a list matching the name
        //
        //   InstanceOptionstring is an "AddonEncoded" name=AddonEncodedValue[selector]descriptor&name=value string
        //=================================================================================================================
        //
        public static string getAddonOptionStringValue(string OptionName, string addonOptionString) {
            string result = genericController.getSimpleNameValue(OptionName, addonOptionString, "", "&");
            int Pos = genericController.vbInstr(1, result, "[");
            if (Pos > 0) {
                result = result.Left(Pos - 1);
            }
            return encodeText(genericController.decodeNvaArgument(result)).Trim(' ');
        }
        //
        //====================================================================================================
        /// <summary>
        /// Create the full html doc from the accumulated elements
        /// </summary>
        /// <param name="htmlBody"></param>
        /// <param name="htmlBodyTag"></param>
        /// <param name="allowLogin"></param>
        /// <param name="allowTools"></param>
        /// <param name="blockNonContentExtras"></param>
        /// <param name="isAdminSite"></param>
        /// <returns></returns>
        public string getHtmlDoc(string htmlBody, string htmlBodyTag, bool allowLogin = true, bool allowTools = true) {
            string result = "";
            try {
                string encoding = genericController.encodeHTML(core.siteProperties.getText("Site Character Encoding", "utf-8"));
                addHeadTag("<meta http-equiv=\"content-type\" content=\"text/html; charset=" + encoding + "\">");
                //addHeadTag("<meta http-equiv=\"content-language\" content=\"en-us\">");
                //addHeadTag("<meta http-equiv=\"cache-control\" content=\"no-cache\">");
                //addHeadTag("<meta http-equiv=\"expires\" content=\"-1\">");
                //addHeadTag("<meta http-equiv=\"pragma\" content=\"no-cache\">");
                string htmlHead = getHtmlHead();
                string htmlBeforeEndOfBody = getHtmlBodyEnd(allowLogin, allowTools);
                result = ""
                    + core.siteProperties.docTypeDeclaration
                    + "\r\n<html>"
                    + "\r\n<head>"
                    + "\r\n" + htmlHead
                    + "\r\n</head>"
                    + "\r\n" + htmlBodyTag + htmlBody + htmlBeforeEndOfBody + "\r\n</body>"
                    + "\r\n</html>"
                    + "";
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// legacy compatibility
        /// </summary>
        /// <param name="core"></param>
        /// <param name="ButtonList"></param>
        /// <returns></returns>
        public static string legacy_closeFormTable(coreController core, string ButtonList) {
            string templegacy_closeFormTable = null;
            if (!string.IsNullOrEmpty(ButtonList)) {
                templegacy_closeFormTable = "</td></tr></TABLE>" + core.html.getPanelButtons(ButtonList, "Button") + "</form>";
            } else {
                templegacy_closeFormTable = "</td></tr></TABLE></form>";
            }
            return templegacy_closeFormTable;
        }
        //
        //====================================================================================================
        /// <summary>
        /// legacy compatibility
        /// </summary>
        /// <param name="core"></param>
        /// <param name="ButtonList"></param>
        /// <returns></returns>
        public static string legacy_openFormTable(coreController core, string ButtonList) {
            string result = "";
            try {
                result = core.html.formStart();
                if (!string.IsNullOrEmpty(ButtonList)) {
                    result = result + core.html.getPanelButtons(ButtonList, "Button");
                }
                result = result + "<table border=\"0\" cellpadding=\"10\" cellspacing=\"0\" width=\"100%\"><tr><TD>";
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public string getHtmlHead() {
            List<string> headList = new List<string>();
            try {
                //
                // -- meta content
                if (core.doc.htmlMetaContent_TitleList.Count > 0) {
                    string content = "";
                    foreach (var asset in core.doc.htmlMetaContent_TitleList) {
                        if ((core.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage))) {
                            headList.Add("\r\n<!-- added by " + genericController.encodeHTML(asset.addedByMessage) + " -->");
                        }
                        content += " | " + asset.content;
                    }
                    headList.Add("\r\n<title>" + genericController.encodeHTML(content.Substring(3)) + "</title>");
                }
                if (core.doc.htmlMetaContent_KeyWordList.Count > 0) {
                    string content = "";
                    foreach (var asset in core.doc.htmlMetaContent_KeyWordList.FindAll((a) => (!string.IsNullOrEmpty(a.content)))) {
                        if ((core.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage))) {
                            headList.Add("\r\n<!-- added by " + genericController.encodeHTML(asset.addedByMessage) + " -->");
                        }
                        content += "," + asset.content;
                    }
                    if (!string.IsNullOrEmpty(content)) {
                        headList.Add("\r\n<meta name=\"keywords\" content=\"" + genericController.encodeHTML(content.Substring(1)) + "\" >");
                    }
                }
                if (core.doc.htmlMetaContent_Description.Count > 0) {
                    string content = "";
                    foreach (var asset in core.doc.htmlMetaContent_Description) {
                        if ((core.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage))) {
                            headList.Add("\r\n<!-- added by " + genericController.encodeHTML(asset.addedByMessage) + " -->");
                        }
                        content += "," + asset.content;
                    }
                    headList.Add("\r\n<meta name=\"description\" content=\"" + genericController.encodeHTML(content.Substring(1)) + "\" >");
                }
                //
                // -- favicon
                string VirtualFilename = core.siteProperties.getText("faviconfilename");
                switch (Path.GetExtension(VirtualFilename).ToLower()) {
                    case ".ico":
                        headList.Add("\r\n<link rel=\"icon\" type=\"image/vnd.microsoft.icon\" href=\"" + genericController.getCdnFileLink(core, VirtualFilename) + "\" >");
                        break;
                    case ".png":
                        headList.Add("\r\n<link rel=\"icon\" type=\"image/png\" href=\"" + genericController.getCdnFileLink(core, VirtualFilename) + "\" >");
                        break;
                    case ".gif":
                        headList.Add("\r\n<link rel=\"icon\" type=\"image/gif\" href=\"" + genericController.getCdnFileLink(core, VirtualFilename) + "\" >");
                        break;
                    case ".jpg":
                        headList.Add("\r\n<link rel=\"icon\" type=\"image/jpg\" href=\"" + genericController.getCdnFileLink(core, VirtualFilename) + "\" >");
                        break;
                }
                //
                // -- misc caching, etc
                //string encoding = genericController.encodeHTML(core.siteProperties.getText("Site Character Encoding", "utf-8"));
                //headList.Add("<meta http-equiv=\"content-type\" content=\"text/html; charset=" + encoding + "\">");
                //headList.Add("<meta http-equiv=\"content-language\" content=\"en-us\">");
                //headList.Add("<meta http-equiv=\"cache-control\" content=\"no-cache\">");
                //headList.Add("<meta http-equiv=\"expires\" content=\"-1\">");
                //headList.Add("<meta http-equiv=\"pragma\" content=\"no-cache\">");
                headList.Add("\r\n<meta name=\"generator\" content=\"Contensive\">");
                //
                // -- no-follow
                if (core.webServer.response_NoFollow) {
                    headList.Add("\r\n<meta name=\"robots\" content=\"nofollow\" >");
                    headList.Add("\r\n<meta name=\"mssmarttagspreventparsing\" content=\"true\" >");
                }
                //
                // -- base is needed for Link Alias case where a slash is in the URL (page named 1/2/3/4/5)
                if (!string.IsNullOrEmpty(core.webServer.serverFormActionURL)) {
                    string BaseHref = core.webServer.serverFormActionURL;
                    if (!string.IsNullOrEmpty(core.doc.refreshQueryString)) {
                        BaseHref += "?" + core.doc.refreshQueryString;
                    }
                    headList.Add("\r\n<base href=\"" + BaseHref + "\" >");
                }
                //
                // -- css and js
                // -- only select assets with .inHead, which includes those whose depencies are .inHead
                if (core.doc.htmlAssetList.Count > 0) {
                    List<string> headScriptList = new List<string>();
                    List<string> styleList = new List<string>();
                    foreach (var asset in core.doc.htmlAssetList.FindAll((htmlAssetClass item) => (item.inHead))) {
                        string debugComment = "\r\n";
                        if ((core.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage))) {
                            debugComment = "\r\n<!-- added by " + genericController.encodeHTML(asset.addedByMessage) + " -->";
                        }
                        if (asset.assetType.Equals(htmlAssetTypeEnum.style)) {
                            styleList.Add(debugComment);
                            if (asset.isLink) {
                                styleList.Add("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + asset.content + "\" >");
                            } else {
                                styleList.Add("<style>" + asset.content + "</style>");
                            }
                        } else if (asset.assetType.Equals(htmlAssetTypeEnum.script)) {
                            headScriptList.Add(debugComment);
                            if (asset.isLink) {
                                headScriptList.Add("<script type=\"text/javascript\" src=\"" + asset.content + "\"></script>");
                            } else {
                                headScriptList.Add("<script type=\"text/javascript\">" + asset.content + "</script>");
                            }
                        }
                    }
                    headList.AddRange(styleList);
                    headList.AddRange(headScriptList);
                }
                //
                // -- other head tags - always last
                foreach (var asset in core.doc.htmlMetaContent_OtherTags.FindAll((a) => (!string.IsNullOrEmpty(a.content)))) {
                    if ((core.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage))) {
                        headList.Add("\r\n<!-- added by " + genericController.encodeHTML(asset.addedByMessage) + " -->");
                    }
                    headList.Add(asset.content);
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return string.Join("\r", headList);
        }

        //
        //====================================================================================================
        //
        public void addScriptCode_onLoad(string code, string addedByMessage) {
            try {
                if (!string.IsNullOrEmpty(code)) {
                    core.doc.htmlAssetList.Add(new htmlAssetClass() {
                        assetType = htmlAssetTypeEnum.scriptOnLoad,
                        addedByMessage = addedByMessage,
                        isLink = false,
                        content = code
                    });
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }

        //
        //====================================================================================================
        //
        public void addScriptCode(string code, string addedByMessage, bool forceHead = false, int sourceAddonId = 0) {
            try {
                if (!string.IsNullOrWhiteSpace(code)) {
                    htmlAssetClass asset = null;
                    if (sourceAddonId != 0) {
                        asset = core.doc.htmlAssetList.Find(t => t.sourceAddonId == sourceAddonId);
                    }
                    if (asset != null) {
                        //
                        // already in list, just mark it forceHead
                        asset.inHead = asset.inHead || forceHead;
                    } else {
                        //
                        // add to list
                        core.doc.htmlAssetList.Add(new htmlAssetClass() {
                            assetType = htmlAssetTypeEnum.script,
                            inHead = forceHead,
                            addedByMessage = addedByMessage,
                            isLink = false,
                            content = genericController.removeScriptTag(code),
                            sourceAddonId = sourceAddonId
                        });
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //=========================================================================================================
        //
        public void addScriptLinkSrc(string scriptLinkSrc, string addedByMessage, bool forceHead = false, int sourceAddonId = 0) {
            try {
                if (!string.IsNullOrWhiteSpace(scriptLinkSrc)) {
                    htmlAssetClass asset = null;
                    if (sourceAddonId != 0) {
                        asset = core.doc.htmlAssetList.Find(t => t.sourceAddonId == sourceAddonId);
                    }
                    if (asset != null) {
                        //
                        // already in list, just mark it forceHead
                        asset.inHead = asset.inHead || forceHead;
                    } else {
                        //
                        // add to list
                        core.doc.htmlAssetList.Add(new htmlAssetClass {
                            assetType = htmlAssetTypeEnum.script,
                            addedByMessage = addedByMessage,
                            isLink = true,
                            inHead = forceHead,
                            content = scriptLinkSrc,
                            sourceAddonId = sourceAddonId
                        });
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        ////
        ////=========================================================================================================
        ////
        //public void addScriptLink_Body(string Filename, string addedByMessage) {
        //    try {
        //        if (!string.IsNullOrEmpty(Filename)) {
        //            core.doc.htmlAssetList.Add(new htmlAssetClass {
        //                assetType = htmlAssetTypeEnum.script,
        //                addedByMessage = addedByMessage,
        //                isLink = true,
        //                content = Filename
        //            });
        //        }
        //    } catch (Exception ex) {
        //        core.handleException(ex);
        //    }
        //}
        //
        //=========================================================================================================
        //
        public void addTitle(string pageTitle, string addedByMessage = "") {
            try {
                if (!string.IsNullOrEmpty(pageTitle.Trim())) {
                    core.doc.htmlMetaContent_TitleList.Add(new htmlMetaClass() {
                        addedByMessage = addedByMessage,
                        content = pageTitle
                    });
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //=========================================================================================================
        //
        public void addMetaDescription(string MetaDescription, string addedByMessage = "") {
            try {
                if (!string.IsNullOrEmpty(MetaDescription.Trim())) {
                    core.doc.htmlMetaContent_Description.Add(new htmlMetaClass() {
                        addedByMessage = addedByMessage,
                        content = MetaDescription
                    });
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //=========================================================================================================
        //
        public void addStyleLink(string StyleSheetLink, string addedByMessage) {
            try {
                if (!string.IsNullOrEmpty(StyleSheetLink.Trim())) {
                    core.doc.htmlAssetList.Add(new htmlAssetClass() {
                        addedByMessage = addedByMessage,
                        assetType = htmlAssetTypeEnum.style,
                        inHead = true,
                        isLink = true,
                        content = StyleSheetLink
                    });
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //=========================================================================================================
        //
        public void addStyleCode(string code, string addedByMessage = "") {
            try {
                if (!string.IsNullOrEmpty(code.Trim())) {
                    core.doc.htmlAssetList.Add(new htmlAssetClass() {
                        addedByMessage = addedByMessage,
                        assetType = htmlAssetTypeEnum.style,
                        inHead = true,
                        isLink = false,
                        content = code
                    });
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //=========================================================================================================
        //
        public void addMetaKeywordList(string MetaKeywordList, string addedByMessage = "") {
            try {
                foreach (string keyword in MetaKeywordList.Split(',')) {
                    if (!string.IsNullOrEmpty(keyword)) {
                        core.doc.htmlMetaContent_KeyWordList.Add(new htmlMetaClass() {
                            addedByMessage = addedByMessage,
                            content = keyword
                        });
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //=========================================================================================================
        //
        public void addHeadTag(string HeadTag, string addedByMessage = "") {
            try {
                core.doc.htmlMetaContent_OtherTags.Add(new htmlMetaClass() {
                    addedByMessage = addedByMessage,
                    content = HeadTag
                });
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //===================================================================================================
        //
        public string getEditWrapper(string Caption, string Content) {
            string result = Content;
            try {
                if (core.doc.sessionContext.isEditingAnything()) {
                    result = html_GetLegacySiteStyles() + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccEditWrapper\">";
                    if (!string.IsNullOrEmpty(Caption)) {
                        result += ""
                                + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccEditWrapperCaption\">"
                                + genericController.encodeText(Caption)
                                + "</td></tr></table>";
                    }
                    result += ""
                            + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccEditWrapperContent\" id=\"editWrapper" + core.doc.editWrapperCnt + "\">"
                            + genericController.encodeText(Content) + "</td></tr></table>"
                            + "</td></tr></table>";
                    core.doc.editWrapperCnt = core.doc.editWrapperCnt + 1;
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //============================================================================
        //   csv_GetContentCopy3
        //       To get them, cp.content.getCopy must call the coreClass version, which calls this for the content
        //============================================================================
        //
        public string getContentCopy(string CopyName, string DefaultContent, int personalizationPeopleId, bool AllowEditWrapper, bool personalizationIsAuthenticated) {
            string returnCopy = "";
            try {
                //
                int CS = 0;
                int RecordID = 0;
                int contactPeopleId = 0;
                string Return_ErrorMessage = "";
                //
                // honestly, not sure what to do with 'return_ErrorMessage'
                //
                CS = core.db.csOpen("copy content", "Name=" + core.db.encodeSQLText(CopyName), "ID", true, 0, false, false, "Name,ID,Copy,modifiedBy");
                if (!core.db.csOk(CS)) {
                    core.db.csClose(ref CS);
                    CS = core.db.csInsertRecord("copy content", 0);
                    if (core.db.csOk(CS)) {
                        RecordID = core.db.csGetInteger(CS, "ID");
                        core.db.csSet(CS, "name", CopyName);
                        core.db.csSet(CS, "copy", genericController.encodeText(DefaultContent));
                        core.db.csSave2(CS);
                        //   Call core.workflow.publishEdit("copy content", RecordID)
                    }
                }
                if (core.db.csOk(CS)) {
                    RecordID = core.db.csGetInteger(CS, "ID");
                    contactPeopleId = core.db.csGetInteger(CS, "modifiedBy");
                    returnCopy = core.db.csGet(CS, "Copy");
                    returnCopy = contentCmdController.executeContentCommands(core, returnCopy, CPUtilsBaseClass.addonContext.ContextPage, personalizationPeopleId, personalizationIsAuthenticated, ref Return_ErrorMessage);
                    returnCopy = activeContentController.renderHtmlForWeb(core, returnCopy, "copy content", RecordID, personalizationPeopleId, "", 0, CPUtilsBaseClass.addonContext.ContextPage);
                    //
                    if (true) {
                        if (core.doc.sessionContext.isEditingAnything()) {
                            returnCopy = core.db.csGetRecordEditLink(CS, false) + returnCopy;
                            if (AllowEditWrapper) {
                                returnCopy = getEditWrapper("copy content", returnCopy);
                            }
                        }
                    }
                }
                core.db.csClose(ref CS);
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnCopy;
        }
        //
        //
        //
        public void addTabEntry(string Caption, string Link, bool IsHit, string StylePrefix = "", string LiveBody = "") {
            try {
                //
                // should use the ccNav object, no the ccCommon module for this code
                //
                core.menuTab.AddEntry(genericController.encodeText(Caption), genericController.encodeText(Link), genericController.encodeBoolean(IsHit), genericController.encodeText(StylePrefix));

                //Call ccAddTabEntry(genericController.encodeText(Caption), genericController.encodeText(Link), genericController.EncodeBoolean(IsHit), genericController.encodeText(StylePrefix), genericController.encodeText(LiveBody))
                //
                return;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError18("main_AddTabEntry")
        }
        //
        //====================================================================================================
        //
        //
        public void addLiveTabEntry(string Caption, string LiveBody, string StylePrefix = "") {
            try {
                //
                // should use the ccNav object, no the ccCommon module for this code
                //
                if (core.doc.menuLiveTab == null) {
                    core.doc.menuLiveTab = new menuLiveTabController();
                }
                core.doc.menuLiveTab.AddEntry(genericController.encodeText(Caption), genericController.encodeText(LiveBody), genericController.encodeText(StylePrefix));
                //
                return;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError18("main_AddLiveTabEntry")
        }
        //
        //
        //
        public string getLiveTabs() {
            string result = "";
            try {
                if (core.doc.menuLiveTab == null) {
                    core.doc.menuLiveTab = new menuLiveTabController();
                }
                result = core.doc.menuLiveTab.GetTabs(core);
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //
        //
        public void addComboTabEntry(string Caption, string Link, string AjaxLink, string LiveBody, bool IsHit, string ContainerClass) {
            try {
                //
                // should use the ccNav object, no the ccCommon module for this code
                //
                if (core.doc.menuComboTab == null) {
                    core.doc.menuComboTab = new menuComboTabController();
                }
                core.doc.menuComboTab.AddEntry(Caption, Link, AjaxLink, LiveBody, IsHit, ContainerClass);
                //
                return;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            //throw new ApplicationException("Unexpected exception"); // Call core.handleLegacyError18("main_AddComboTabEntry")
        }
        //
        //
        //
        public string getComboTabs() {
            string result = "";
            try {
                if (core.doc.menuComboTab == null) {
                    core.doc.menuComboTab = new menuComboTabController();
                }
                result = core.doc.menuComboTab.GetTabs(core);
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public string getResourceLibrary2(string RootFolderName, bool AllowSelectResource, string SelectResourceEditorName, string SelectLinkObjectName, bool AllowGroupAdd) {
            string addonGuidResourceLibrary = "{564EF3F5-9673-4212-A692-0942DD51FF1A}";
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            arguments.Add("RootFolderName", RootFolderName);
            arguments.Add("AllowSelectResource", AllowSelectResource.ToString());
            arguments.Add("SelectResourceEditorName", SelectResourceEditorName);
            arguments.Add("SelectLinkObjectName", SelectLinkObjectName);
            arguments.Add("AllowGroupAdd", AllowGroupAdd.ToString());
            return core.addon.execute(addonModel.create(core, addonGuidResourceLibrary), new CPUtilsBaseClass.addonExecuteContext() {
                addonType = CPUtilsBaseClass.addonContext.ContextAdmin,
                instanceArguments = arguments
            });
            //Dim Option_String As String
            //Option_String = "" _
            //    & "RootFolderName=" & RootFolderName _
            //    & "&AllowSelectResource=" & AllowSelectResource _
            //    & "&SelectResourceEditorName=" & SelectResourceEditorName _
            //    & "&SelectLinkObjectName=" & SelectLinkObjectName _
            //    & "&AllowGroupAdd=" & AllowGroupAdd _
            //    & ""

            //Return core.addon.execute_legacy4(addonGuidResourceLibrary, Option_String, CPUtilsBaseClass.addonContext.ContextAdmin)
        }
        //
        //========================================================================
        // Read and save a main_GetFormInputCheckList
        //   see main_GetFormInputCheckList for an explaination of the input
        //========================================================================
        //
        public void processCheckList(string TagName, string PrimaryContentName, string PrimaryRecordID, string SecondaryContentName, string RulesContentName, string RulesPrimaryFieldname, string RulesSecondaryFieldName) {
            //
            string rulesTablename = null;
            string SQL = null;
            DataTable currentRules = null;
            int currentRulesCnt = 0;
            bool RuleFound = false;
            int RuleId = 0;
            int Ptr = 0;
            int TestRecordIDLast = 0;
            int TestRecordID = 0;
            string dupRuleIdList = null;
            int GroupCnt = 0;
            int GroupPtr = 0;
            int SecondaryRecordID = 0;
            bool RuleNeeded = false;
            int CSRule = 0;
            bool RuleContentChanged = false;
            bool SupportRuleCopy = false;
            string RuleCopy = null;
            //
            // --- create Rule records for all selected
            //
            GroupCnt = core.docProperties.getInteger(TagName + ".RowCount");
            if (GroupCnt > 0) {
                //
                // Test if RuleCopy is supported
                //
                SupportRuleCopy = Models.Complex.cdefModel.isContentFieldSupported(core, RulesContentName, "RuleCopy");
                if (SupportRuleCopy) {
                    SupportRuleCopy = SupportRuleCopy && Models.Complex.cdefModel.isContentFieldSupported(core, SecondaryContentName, "AllowRuleCopy");
                    if (SupportRuleCopy) {
                        SupportRuleCopy = SupportRuleCopy && Models.Complex.cdefModel.isContentFieldSupported(core, SecondaryContentName, "RuleCopyCaption");
                    }
                }
                //
                // Go through each checkbox and check for a rule
                //
                //
                // try
                //
                currentRulesCnt = 0;
                dupRuleIdList = "";
                rulesTablename = Models.Complex.cdefModel.getContentTablename(core, RulesContentName);
                SQL = "select " + RulesSecondaryFieldName + ",id from " + rulesTablename + " where (" + RulesPrimaryFieldname + "=" + PrimaryRecordID + ")and(active<>0) order by " + RulesSecondaryFieldName;
                currentRulesCnt = 0;
                currentRules = core.db.executeQuery(SQL);
                currentRulesCnt = currentRules.Rows.Count;
                for (GroupPtr = 0; GroupPtr < GroupCnt; GroupPtr++) {
                    //
                    // ----- Read Response
                    //
                    SecondaryRecordID = core.docProperties.getInteger(TagName + "." + GroupPtr + ".ID");
                    RuleCopy = core.docProperties.getText(TagName + "." + GroupPtr + ".RuleCopy");
                    RuleNeeded = core.docProperties.getBoolean(TagName + "." + GroupPtr);
                    //
                    // ----- Update Record
                    //
                    RuleFound = false;
                    RuleId = 0;
                    TestRecordIDLast = 0;
                    for (Ptr = 0; Ptr < currentRulesCnt; Ptr++) {
                        TestRecordID = genericController.encodeInteger(currentRules.Rows[Ptr][0]);
                        if (TestRecordID == 0) {
                            //
                            // skip
                            //
                        } else if (TestRecordID == SecondaryRecordID) {
                            //
                            // hit
                            //
                            RuleFound = true;
                            RuleId = genericController.encodeInteger(currentRules.Rows[Ptr][1]);
                            break;
                        } else if (TestRecordID == TestRecordIDLast) {
                            //
                            // dup
                            //
                            dupRuleIdList = dupRuleIdList + "," + genericController.encodeInteger(currentRules.Rows[Ptr][1]);
                            currentRules.Rows[Ptr][0] = 0;
                        }
                        TestRecordIDLast = TestRecordID;
                    }
                    if (SupportRuleCopy && RuleNeeded && (RuleFound)) {
                        //
                        // Record exists and is needed, update the rule copy
                        //
                        SQL = "update " + rulesTablename + " set rulecopy=" + core.db.encodeSQLText(RuleCopy) + " where id=" + RuleId;
                        core.db.executeQuery(SQL);
                    } else if (RuleNeeded && (!RuleFound)) {
                        //
                        // No record exists, and one is needed
                        //
                        CSRule = core.db.csInsertRecord(RulesContentName);
                        if (core.db.csOk(CSRule)) {
                            core.db.csSet(CSRule, "Active", RuleNeeded);
                            core.db.csSet(CSRule, RulesPrimaryFieldname, PrimaryRecordID);
                            core.db.csSet(CSRule, RulesSecondaryFieldName, SecondaryRecordID);
                            if (SupportRuleCopy) {
                                core.db.csSet(CSRule, "RuleCopy", RuleCopy);
                            }
                        }
                        core.db.csClose(ref CSRule);
                        RuleContentChanged = true;
                    } else if ((!RuleNeeded) && RuleFound) {
                        //
                        // Record exists and it is not needed
                        //
                        SQL = "delete from " + rulesTablename + " where id=" + RuleId;
                        core.db.executeQuery(SQL);
                        RuleContentChanged = true;
                    }
                }
                //
                // delete dups
                //
                if (!string.IsNullOrEmpty(dupRuleIdList)) {
                    SQL = "delete from " + rulesTablename + " where id in (" + dupRuleIdList.Substring(1) + ")";
                    core.db.executeQuery(SQL);
                    RuleContentChanged = true;
                }
            }
            if (RuleContentChanged) {
                core.cache.invalidateAllInContent(RulesContentName);
            }
        }
        //
        //====================================================================================================
        // main_GetRecordEditLink( iContentName, iRecordID )
        //
        //   iContentName The content for this link
        //   iRecordID    The ID of the record in the Table
        //========================================================================
        //
        public string getRecordEditLink(string ContentName, int RecordID, bool AllowCut = false) {
            return getRecordEditLink2(ContentName, RecordID, genericController.encodeBoolean(AllowCut), "", core.doc.sessionContext.isEditing(ContentName));
        }
        //
        //====================================================================================================
        public static string genericBlockTag(string TagName, string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "") {
            string s = "";
            //
            if (!string.IsNullOrEmpty(HtmlName)) {
                s += " name=\"" + HtmlName + "\"";
            }
            if (!string.IsNullOrEmpty(HtmlClass)) {
                s += " class=\"" + HtmlClass + "\"";
            }
            if (!string.IsNullOrEmpty(HtmlId)) {
                s += " id=\"" + HtmlId + "\"";
            }
            return "<" + TagName.Trim() + s + ">" + InnerHtml + "</" + TagName.Trim() + ">";
        }
        //
        //====================================================================================================
        //
        public static string div(string innerHtml, string htmlName = "", string htmlClass = "", string htmlId = "") {
            return genericBlockTag("div", innerHtml, htmlName, htmlClass, htmlId);
        }
    }
}
