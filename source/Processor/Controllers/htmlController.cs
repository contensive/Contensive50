
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Data;
using System.Collections.Generic;
using Contensive.BaseClasses;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Models.Domain;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Addons.AdminSite.Controllers;

namespace Contensive.Processor.Controllers {
    /// <summary>
    /// Tools used to assemble html document elements. This is not a storage for assembling a document (see docController)
    /// </summary>
    public class HtmlController {
        //
        private CoreController core;
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="core"></param>
        /// <remarks></remarks>
        public HtmlController(CoreController core) {
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
                if (core.session.isAuthenticatedContentManager(core) && (core.session.user.allowToolsPanel)) {
                    if (AllowTools) {
                        result.Add(core.html.getToolsPanel());
                    }
                } else {
                    if (AllowLogin) {
                        result.Add(getLoginLink());
                    }
                }
                //
                // -- body Javascript
                bool allowDebugging = core.visitProperty.getBoolean("AllowDebugging");
                var scriptOnLoad = new List<string>();
                foreach (var asset in core.doc.htmlAssetList.FindAll((a) => ((a.assetType == HtmlAssetTypeEnum.script) || (a.assetType == HtmlAssetTypeEnum.scriptOnLoad)) && (!a.inHead) && (!string.IsNullOrEmpty(a.content)))) {
                    if ((asset.addedByMessage != "") && allowDebugging) {
                        result.Add("\r\n<!-- from " + asset.addedByMessage + " -->\r\n");
                    }
                    if (asset.assetType == HtmlAssetTypeEnum.scriptOnLoad) {
                        scriptOnLoad.Add(asset.content + ";");
                    }
                    if (!asset.isLink) {
                        result.Add("<script Language=\"JavaScript\" type=\"text/javascript\">" + asset.content + "</script>");
                    } else {
                        result.Add("<script type=\"text/javascript\" src=\"" + asset.content + "\"></script>");
                    }
                }
                if (scriptOnLoad.Count > 0) {
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
                LogController.handleError( core,ex);
                throw;
            }
            return string.Join("\r", result);
        }
        //
        //====================================================================================================
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
                string LcaseCriteria = vbLCase(Criteria);
                return_IsEmptyList = true;
                //
                string CurrentValueText = CurrentValue.ToString();
                string SelectRaw = "";
                foreach (Constants.CacheInputSelectClass inputSelect in core.doc.inputSelectCache) {
                    if ((inputSelect.ContentName == ContentName) && (inputSelect.Criteria == null) && (inputSelect.CurrentValue == CurrentValueText)) {
                        SelectRaw = inputSelect.SelectRaw;
                        return_IsEmptyList = false;
                        break;
                    }
                }
                //
                if (string.IsNullOrEmpty(SelectRaw)) {
                    //
                    // Build the SelectRaw, Test selection size
                    ContentMetadataModel metaData = ContentMetadataModel.createByUniqueName(core, ContentName);
                    string ContentControlCriteria = metaData.legacyContentControlCriteria;
                    string SQL = "select count(*) as cnt from " + metaData.tableName + " where " + ContentControlCriteria;
                    if (!string.IsNullOrEmpty(null)) {
                        SQL += " and " + null;
                    }
                    DataTable dt = core.db.executeQuery(SQL);
                    int RowCnt = 0;
                    if (dt.Rows.Count > 0) {
                        RowCnt = GenericController.encodeInteger(dt.Rows[0]["cnt"]);
                    }
                    int RowMax = 0;
                    if (RowCnt == 0) {
                        RowMax = -1;
                    } else {
                        return_IsEmptyList = false;
                        RowMax = RowCnt - 1;
                    }
                    //
                    if (RowCnt > core.siteProperties.selectFieldLimit) {
                        //
                        // -- Selection is too big
                        ErrorController.addUserError(core, "The drop down list for " + ContentName + " called " + MenuName + " is too long to display. The site administrator has been notified and the problem will be resolved shortly. To fix this issue temporarily, go to the admin tab of the Preferences page and set the Select Field Limit larger than " + RowCnt + ".");
                        LogController.handleError( core,new Exception("Error creating select list from content [" + ContentName + "] called [" + MenuName + "]. Selection of [" + RowCnt + "] records exceeds [" + core.siteProperties.selectFieldLimit + "], the current Site Property SelectFieldLimit."));
                        result += inputHidden(MenuNameFPO, CurrentValue);
                        if (CurrentValue == 0) {
                            result = inputText(core, MenuName, "0");
                        } else {
                            using ( var csData = new CsModel( core )) {
                                if (csData.openRecord(ContentName, CurrentValue)) {
                                    result = csData.getText("name") + "&nbsp;";
                                }
                            }
                        }
                        result += "(Selection is too large to display option list)";
                    } else {
                        //
                        // -- Generate Drop Down Field Names
                        string DropDownFieldList = metaData.dropDownFieldList;
                        if (string.IsNullOrEmpty(DropDownFieldList)) { DropDownFieldList = "NAME"; }
                        int DropDownFieldCount = 0;
                        string CharAllowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                        int DropDownFieldListLength = DropDownFieldList.Length;
                        string[] DropDownDelimiter = { };
                        string[] DropDownFieldName = { };
                        string DropDownPreField = "";
                        string FieldName = "";
                        int CharPointer = 0;
                        string SortFieldList = "";
                        for (CharPointer = 1; CharPointer <= DropDownFieldListLength; CharPointer++) {
                            string CharTest = DropDownFieldList.Substring(CharPointer - 1, 1);
                            if (GenericController.vbInstr(1, CharAllowed, CharTest) == 0) {
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
                            LogController.handleError( core,new Exception("No drop down field names found for content [" + ContentName + "]."));
                        } else {
                            int[] DropDownFieldPointer = new int[DropDownFieldCount];
                            string SelectFields = "ID";
                            int Ptr = 0;
                            for (Ptr = 0; Ptr < DropDownFieldCount; Ptr++) {
                                SelectFields = SelectFields + "," + DropDownFieldName[Ptr];
                            }
                            //
                            // ----- Start select box
                            //
                            string TagID = "";
                            if (!string.IsNullOrEmpty(htmlId)) {
                                TagID = " ID=\"" + htmlId + "\"";
                            }
                            StringBuilderLegacyController FastString = new StringBuilderLegacyController();
                            FastString.Add("<select size=\"1\" name=\"" + MenuNameFPO + "\"" + TagID + ">");
                            FastString.Add("<option value=\"\">" + NoneCaptionFPO + "</option>");
                            //
                            // ----- select values
                            using (var csData = new CsModel(core)) {
                                if (csData.open(ContentName, Criteria, SortFieldList, false, 0, SelectFields)) {
                                    string[,] RowsArray = csData.getRows();
                                    string[] RowFieldArray = csData.getSelectFieldList().Split(',');
                                    int ColumnMax = RowsArray.GetUpperBound(0);
                                    RowMax = RowsArray.GetUpperBound(1);
                                    //
                                    // -- setup IDFieldPointer
                                    string UcaseFieldName = "ID";
                                    int IDFieldPointer = 0;
                                    int ColumnPointer = 0;
                                    for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                        if (UcaseFieldName == GenericController.vbUCase(RowFieldArray[ColumnPointer])) {
                                            IDFieldPointer = ColumnPointer;
                                            break;
                                        }
                                    }
                                    //
                                    // setup DropDownFieldPointer()
                                    //
                                    for (var FieldPointer = 0; FieldPointer < DropDownFieldCount; FieldPointer++) {
                                        UcaseFieldName = GenericController.vbUCase(DropDownFieldName[FieldPointer]);
                                        for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                            if (UcaseFieldName == GenericController.vbUCase(RowFieldArray[ColumnPointer])) {
                                                DropDownFieldPointer[FieldPointer] = ColumnPointer;
                                                break;
                                            }
                                        }
                                    }
                                    //
                                    // output select
                                    //
                                    bool SelectedFound = false;
                                    string Copy = null;
                                    int RowPointer = 0;
                                    int RecordID = 0;
                                    for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                                        RecordID = GenericController.encodeInteger(RowsArray[IDFieldPointer, RowPointer]);
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
                                        FastString.Add(">" + HtmlController.encodeHtml(Copy) + "</option>");
                                    }
                                    if (!SelectedFound && (CurrentValue != 0)) {
                                        csData.close();
                                        if (!string.IsNullOrEmpty(Criteria)) {
                                            Criteria = Criteria + "and";
                                        }
                                        Criteria = Criteria + "(id=" + GenericController.encodeInteger(CurrentValue) + ")";
                                        if (csData.open(ContentName, Criteria, SortFieldList, false, 0, SelectFields)) {
                                            RowsArray = csData.getRows();
                                            RowFieldArray = csData.getSelectFieldList().Split(',');
                                            RowMax = RowsArray.GetUpperBound(1);
                                            ColumnMax = RowsArray.GetUpperBound(0);
                                            RecordID = GenericController.encodeInteger(RowsArray[IDFieldPointer, 0]);
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
                                            FastString.Add(">" + HtmlController.encodeHtml(Copy) + "</option>");
                                        }
                                    }
                                }
                            }
                            FastString.Add("</select>");
                            SelectRaw = FastString.Text;
                        }
                    }
                    //
                    // Save the SelectRaw
                    //
                    if (!return_IsEmptyList) {
                        core.doc.inputSelectCache.Add(new Constants.CacheInputSelectClass() {
                            ContentName = ContentName,
                            Criteria = Criteria,
                            CurrentValue = CurrentValue.ToString(),
                            SelectRaw = SelectRaw
                        });
                    }
                }
                //
                SelectRaw = GenericController.vbReplace(SelectRaw, MenuNameFPO, MenuName);
                SelectRaw = GenericController.vbReplace(SelectRaw, NoneCaptionFPO, NoneCaption);
                if (!string.IsNullOrEmpty(HtmlClass)) {
                    SelectRaw = GenericController.vbReplace(SelectRaw, "<select ", "<select class=\"" + HtmlClass + "\"");
                }
                result = SelectRaw;
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public string selectUserFromGroup(string MenuName, int currentValue, int GroupID, string ignore = "", string noneCaption = "", string HtmlId = "", string HtmlClass = "select") {
            string result = "";
            try {
                StringBuilderLegacyController FastString = new StringBuilderLegacyController();
                //
                const string MenuNameFPO = "<MenuName>";
                const string NoneCaptionFPO = "<NoneCaption>";
                //
                string iMenuName = GenericController.encodeText(MenuName);
                currentValue = GenericController.encodeInteger(currentValue);
                noneCaption = GenericController.encodeEmpty(noneCaption, "Select One");
                string sqlCriteria = "";
                //
                string SelectRaw = "";
                foreach (Constants.CacheInputSelectClass cacheInputSelect in core.doc.inputSelectCache) {
                    if ((cacheInputSelect.ContentName == "Group:" + GroupID) && (cacheInputSelect.Criteria == sqlCriteria) && (GenericController.encodeInteger(cacheInputSelect.CurrentValue) == currentValue)) {
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
                    using (var csData = new CsModel(core)) {
                        csData.openSql(SQL);
                        if (csData.ok()) {
                            RowMax = RowMax + csData.getInteger("cnt");
                        }
                    }
                    if (RowMax > core.siteProperties.selectFieldLimit) {
                        //
                        // Selection is too big
                        //
                        LogController.handleError( core,new Exception("While building a group members list for group [" + GroupController.getGroupName(core, GroupID) + "], too many rows were selected. [" + RowMax + "] records exceeds [" + core.siteProperties.selectFieldLimit + "], the current Site Property app.SiteProperty_SelectFieldLimit."));
                        result += inputHidden(MenuNameFPO, currentValue);
                        if (currentValue != 0) {
                            using (var csData = new CsModel(core)) {
                                if (csData.openRecord("people", currentValue)) {
                                    result = csData.getText("name") + "&nbsp;";
                                }
                            }
                        }
                        result += "(Selection is too large to display)";
                    } else {
                        //
                        // ----- Generate Drop Down Field Names
                        //
                        string DropDownFieldList = "name";
                        var peopleMetaData = ContentMetadataModel.createByUniqueName(core, "people");
                        if ( peopleMetaData != null ) DropDownFieldList = peopleMetaData.dropDownFieldList;
                        int DropDownFieldCount = 0;
                        string CharAllowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                        string DropDownPreField = "";
                        string FieldName = "";
                        string SortFieldList = "";
                        string[] DropDownFieldName = { };
                        string[] DropDownDelimiter = { };
                        for (int CharPointer = 1; CharPointer <= DropDownFieldList.Length; CharPointer++) {
                            string CharTest = DropDownFieldList.Substring(CharPointer - 1, 1);
                            if (GenericController.vbInstr(1, CharAllowed, CharTest) == 0) {
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
                            LogController.handleError( core,new Exception("No drop down field names found for content [" + GroupID + "]."));
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
                            if (GenericController.encodeEmpty(HtmlClass, "") != "") {
                                TagClass = " Class=\"" + GenericController.encodeEmpty(HtmlClass, "") + "\"";
                            }
                            //
                            string TagID = "";
                            if (GenericController.encodeEmpty(HtmlId, "") != "") {
                                TagID = " ID=\"" + GenericController.encodeEmpty(HtmlId, "") + "\"";
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
                            using (var csData = new CsModel(core)) {
                                SQL = "select " + SelectFields + " from ccMemberRules R"
                                    + " inner join ccMembers P on R.MemberID=P.ID"
                                    + " where (R.GroupID=" + GroupID + ")"
                                    + " and((R.DateExpires is null)or(R.DateExpires>" + DbController.encodeSQLDate(DateTime.Now) + "))"
                                    + " and(P.active<>0)"
                                    + " order by P." + SortFieldList;
                                if (csData.openSql(SQL)) {
                                    string[,] RowsArray = csData.getRows();
                                    string[] RowFieldArray = csData.getSelectFieldList().Split(',');
                                    RowMax = RowsArray.GetUpperBound(1);
                                    int ColumnMax = RowsArray.GetUpperBound(0);
                                    //
                                    // setup IDFieldPointer
                                    //
                                    string UcaseFieldName = "ID";
                                    int IDFieldPointer = 0;
                                    for (int ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                        if (UcaseFieldName == GenericController.vbUCase(RowFieldArray[ColumnPointer])) {
                                            IDFieldPointer = ColumnPointer;
                                            break;
                                        }
                                    }
                                    //
                                    // setup DropDownFieldPointer()
                                    //
                                    for (var FieldPointer = 0; FieldPointer < DropDownFieldCount; FieldPointer++) {
                                        UcaseFieldName = GenericController.vbUCase(DropDownFieldName[FieldPointer]);
                                        for (int ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                            if (UcaseFieldName == GenericController.vbUCase(RowFieldArray[ColumnPointer])) {
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
                                        int RecordID = GenericController.encodeInteger(RowsArray[IDFieldPointer, RowPointer]);
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
                            }
                            FastString.Add("</select>");
                            SelectRaw = FastString.Text;
                        }
                    }
                    //
                    // Save the SelectRaw
                    //
                    core.doc.inputSelectCache.Add(new Constants.CacheInputSelectClass() {
                        ContentName = "Group:" + GroupID,
                        Criteria = sqlCriteria,
                        CurrentValue = currentValue.ToString(),
                        SelectRaw = SelectRaw
                    });
                }
                //
                SelectRaw = GenericController.vbReplace(SelectRaw, MenuNameFPO, iMenuName);
                SelectRaw = GenericController.vbReplace(SelectRaw, NoneCaptionFPO, noneCaption);
                result = SelectRaw;
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Create a select list from a comma separated list, returns an index into the list list, starting at 1, if an element is blank (,) no option is created
        /// </summary>
        /// <param name="MenuName"></param>
        /// <param name="CurrentValue"></param>
        /// <param name="SelectList"></param>
        /// <param name="NoneCaption"></param>
        /// <param name="htmlId"></param>
        /// <param name="HtmlClass"></param>
        /// <returns></returns>
        public static string selectFromList(CoreController core, string MenuName, string CurrentValue, List<NameValueClass> lookupList, string NoneCaption, string htmlId, string HtmlClass = "") {
            string result = "";
            try {
                StringBuilderLegacyController FastString = new StringBuilderLegacyController();
                FastString.Add("<select size=1 ");
                if (!string.IsNullOrEmpty(htmlId)) FastString.Add("id=\"" + htmlId + "\" ");
                if (!string.IsNullOrEmpty(HtmlClass)) FastString.Add("class=\"" + HtmlClass + "\" ");
                if (!string.IsNullOrEmpty(MenuName)) FastString.Add("name=\"" + MenuName + "\" ");
                if (!string.IsNullOrEmpty(NoneCaption)) {
                    FastString.Add("><option value=\"\">" + NoneCaption + "</option>");
                } else {
                    FastString.Add("><option value=\"\">Select One</option>");
                }
                //
                // ----- select values
                string CurrentValueLower = CurrentValue.ToLowerInvariant();
                foreach (NameValueClass nameValue in lookupList) {
                    string selected = (nameValue.value.ToLowerInvariant() == CurrentValueLower) ? " selected" : "";
                    FastString.Add("<option value=\"" + nameValue.value + "\" " + selected + ">" + nameValue.name + "</option>");
                }
                FastString.Add("</select>");
                result = FastString.Text;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Select from a list where the list is a comma delimited list
        /// </summary>
        /// <param name="core"></param>
        /// <param name="MenuName"></param>
        /// <param name="CurrentValue"></param>
        /// <param name="lookups"></param>
        /// <param name="NoneCaption"></param>
        /// <param name="htmlId"></param>
        /// <param name="HtmlClass"></param>
        /// <returns></returns>
        public static string selectFromList(CoreController core, string MenuName, int CurrentValue, string[] lookups, string NoneCaption, string htmlId, string HtmlClass = "") {
            string result = "";
            try {
                //
                StringBuilderLegacyController FastString = new StringBuilderLegacyController();
                //string[] lookups = null;
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
                //lookups = SelectList.Split(',');
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
                LogController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Display an icon with a link to the login form/cclib.net/admin area
        /// </summary>
        /// <returns></returns>
        public string getLoginLink() {
            string result = "";
            try {
                //
                //If Not (true) Then Exit Function
                //
                string Link = null;
                string IconFilename = null;
                //
                if (core.siteProperties.allowLoginIcon) {
                    result += "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">";
                    result += "<tr><td align=\"right\">";
                    if (core.session.isAuthenticatedContentManager(core)) {
                        result += "<a href=\"" + HtmlController.encodeHtml("/" + core.appConfig.adminRoute) + "\" target=\"_blank\">";
                    } else {
                        Link = core.webServer.requestPage + "?" + core.doc.refreshQueryString;
                        Link = GenericController.modifyLinkQuery(Link, RequestNameHardCodedPage, HardCodedPageLogin, true);
                        //Link = genericController.modifyLinkQuery(Link, RequestNameInterceptpage, LegacyInterceptPageSNLogin, True)
                        result += "<a href=\"" + HtmlController.encodeHtml(Link) + "\" >";
                    }
                    IconFilename = core.siteProperties.loginIconFilename;
                    if (GenericController.vbLCase(IconFilename.Left(7)) != "/ContensiveBase/") {
                        IconFilename = GenericController.getCdnFileLink(core, IconFilename);
                    }
                    // original  "<img alt=\"Login\" src=\"" + IconFilename + "\" border=\"0\" >"
                    result += HtmlController.img( IconFilename, "Login" );
                    result += "</A>";
                    result += "</td></tr></table>";
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Wrap the content in a common wrapper if authoring is enabled
        /// </summary>
        /// <param name="Content"></param>
        /// <returns></returns>
        public string getAdminHintWrapper(string Content) {
            string temphtml_GetAdminHintWrapper = null;
            try {
                //
                //If Not (true) Then Exit Function
                //
                temphtml_GetAdminHintWrapper = "";
                if ((core.session.isEditing("") || core.session.isAuthenticatedAdmin(core))) {
                    //temphtml_GetAdminHintWrapper = temphtml_GetAdminHintWrapper + html_GetLegacySiteStyles();
                    temphtml_GetAdminHintWrapper = temphtml_GetAdminHintWrapper + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccHintWrapper\">"
                            + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccHintWrapperContent\">"
                            + "<b>Administrator</b>"
                            + "<br>"
                            + "<br>" + GenericController.encodeText(Content) + "</td></tr></table>"
                        + "</td></tr></table>";
                }

                return temphtml_GetAdminHintWrapper;
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            //ErrorTrap:
            //throw new GenericException("Unexpected exception"); // Call core.handleLegacyError18("main_GetAdminHintWrapper")
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
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns a multipart form, required for file uploads
        /// </summary>
        /// <param name="core"></param>
        /// <param name="innerHtml"></param>
        /// <param name="actionQueryString"></param>
        /// <param name="htmlName"></param>
        /// <param name="htmlClass"></param>
        /// <param name="htmlId"></param>
        /// <returns></returns>
        public static string formMultipart(CoreController core, string innerHtml, string actionQueryString = "", string htmlName = "", string htmlClass = "", string htmlId = "") {
            return formMultipart_start(core, actionQueryString, htmlName, htmlClass, htmlId) + innerHtml + "</form>";
        }
        //
        //====================================================================================================
        /// <summary>
        /// Starts an HTML form for uploads, Should be closed with main_GetUploadFormEnd
        /// </summary>
        /// <param name="actionQueryString"></param>
        /// <returns></returns>
        public static string formMultipart_start( CoreController core, string actionQueryString = "", string htmlName = "", string htmlClass = "", string htmlId = "") {
            string result = "<form action=\"?" + ((actionQueryString == "") ? core.doc.refreshQueryString : actionQueryString) + "\" ENCTYPE=\"MULTIPART/FORM-DATA\" method=\"post\" style=\"display: inline;\"";
            if (!string.IsNullOrWhiteSpace(htmlName)) result += " name=\"" + htmlName + "\"";
            if (!string.IsNullOrWhiteSpace(htmlClass)) result += " class=\"" + htmlClass + "\"";
            if (!string.IsNullOrWhiteSpace(htmlId)) result += " id=\"" + htmlId + "\"";
            return result + ">";
        }
        //
        //====================================================================================================
        /// <summary>
        /// create an html form
        /// </summary>
        /// <param name="innerHtml"></param>
        /// <param name="actionQueryString"></param>
        /// <param name="htmlName"></param>
        /// <param name="htmlId"></param>
        /// <param name="htmlMethod"></param>
        /// <returns></returns>
        public static string form( CoreController core,  string innerHtml, string actionQueryString = "", string htmlName = "", string htmlClass = "", string htmlId = "", string htmlMethod = "post" ) {
            if (string.IsNullOrEmpty(actionQueryString)) actionQueryString = core.doc.refreshQueryString;
            string action = core.webServer.serverFormActionURL + (string.IsNullOrEmpty(actionQueryString) ? "" : "?" + actionQueryString);
            string result = "<form name=\"" + htmlName + "\" action=\"" + action + "\" method=\"" + htmlMethod + "\" style=\"display: inline;\"";
            result += (string.IsNullOrWhiteSpace(htmlId)) ? "" : "" + " id=\"" + htmlId + "\"";
            result += (string.IsNullOrWhiteSpace(htmlId)) ? "" : "" + " id=\"" + htmlId + "\"";
            result += (string.IsNullOrWhiteSpace(htmlClass)) ? "" : "" + " class=\"" + htmlClass + "\"";
            result += ">";
            return result + innerHtml + "</form>";
        }
        //
        //====================================================================================================
        //
        public static string inputText( CoreController core, string htmlName, string defaultValue = "", int heightRows = 1, int widthCharacters = 20, string htmlId = "", bool passwordField = false, bool readOnly = false, string htmlClass = "", int maxLength = -1, bool disabled = false, string placeholder = "" ) {
            string result = "";
            try {
                if ((heightRows>1) & !passwordField) {
                    result = inputTextarea(core, htmlName, defaultValue, heightRows, widthCharacters, htmlId, true, readOnly, htmlClass, disabled, maxLength);
                } else {
                    defaultValue = HtmlController.encodeHtml(defaultValue);
                    // todo replace concat with stringbuilder
                    string attrList = " name=\"" + htmlName + "\"";
                    attrList += (string.IsNullOrEmpty(htmlId)) ? "" : " id=\"" + htmlId + "\"";
                    attrList += (string.IsNullOrEmpty(htmlClass)) ? "" : " class=\"" + htmlClass + "\"";
                    attrList += (!readOnly) ? "" : " readonly";
                    attrList += (!disabled) ? "" : " disabled";
                    attrList += (maxLength <= 0) ? "" : " maxlength=" + maxLength.ToString();
                    attrList += (string.IsNullOrEmpty(placeholder)) ? "" : " placeholder=\"" + placeholder + "\"";
                    if (passwordField) {
                        attrList += (widthCharacters <= 0) ? " size=\"" + core.siteProperties.defaultFormInputWidth.ToString() + "\"" : " size=\"" + widthCharacters.ToString() + "\"";
                        result = "<input type=\"password\" value=\"" + defaultValue + "\"" + attrList + ">";
                    } else {
                        attrList += (widthCharacters <= 0) ? " size=\"" + core.siteProperties.defaultFormInputWidth.ToString() + "\"" : " size=\"" + widthCharacters.ToString() + "\"";
                        result = "<input TYPE=\"Text\" value=\"" + defaultValue + "\"" + attrList + ">";
                    }
                    core.doc.formInputTextCnt += 1;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// HTML Form text input (or text area), added disabled case
        /// </summary>
        /// <param name="htmlName"></param>
        /// <param name="defaultValue"></param>
        /// <param name="heightRows"></param>
        /// <param name="widthCharacters"></param>
        /// <param name="htmlId"></param>
        /// <param name="ignore"></param>
        /// <param name="disabled"></param>
        /// <param name="htmlClass"></param>
        /// <returns></returns>
        public static string inputTextarea( CoreController core, string htmlName, string defaultValue = "", int heightRows = 4, int widthCharacters = -1, string htmlId = "", bool ignore = false, bool readOnly = false, string htmlClass = "", bool disabled = false, int maxLength = 0) {
            string result = "";
            try {
                defaultValue = HtmlController.encodeHtml(defaultValue);
                string attrList = " name=\"" + htmlName + "\"";
                attrList += (string.IsNullOrEmpty(htmlId)) ? "" : " id=\"" + htmlId + "\"";
                attrList += (string.IsNullOrEmpty(htmlClass)) ? "" : " class=\"" + htmlClass + "\"";
                attrList += (!readOnly) ? "" : " readonly";
                attrList += (!disabled) ? "" : " disabled";
                attrList += (maxLength <= 0) ? "" : " maxlength=" + maxLength.ToString();
                attrList += (widthCharacters==-1) ? "" : " cols=" + widthCharacters.ToString();
                attrList += " rows=" + heightRows.ToString();
                result = "<textarea" + attrList + ">" + defaultValue + "</textarea>";
                core.doc.formInputTextCnt += 1;
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public static string inputDateTime(CoreController core, string htmlName, DateTime? htmlValue, string Width = "", string htmlId = "", string htmlClass = "", bool readOnly = false, bool required = false, bool disabled = false) {
            string result = "";
            try {
                // {0:s}
                // yyyy-MM-dd
                core.doc.formInputTextCnt += 1;
                core.doc.inputDateCnt = core.doc.inputDateCnt + 1;
                result = "<input type=\"date\"  name=\"" + HtmlController.encodeHtml(htmlName) + "\"";
                if ((htmlValue != null) && (htmlValue > DateTime.MinValue)) result += " value=\"" + String.Format("{0:s}", htmlValue) + "\"";
                result += (string.IsNullOrEmpty(htmlId)) ? "" : " id=\"" + htmlId + "\"";
                result += (string.IsNullOrEmpty(htmlClass)) ? "" : " class=\"" + htmlClass + "\"";
                result += (!readOnly) ? "" : " readonly";
                result += (!disabled) ? "" : " disabled";
                result += (!required) ? "" : " required";
                result += ">";

                result += "<input type=\"time\"  name=\"" + HtmlController.encodeHtml(htmlName + "_time") + "\"";
                if ((htmlValue != null) && (htmlValue > DateTime.MinValue)) result += " value=\"" + String.Format("{0:s}", htmlValue) + "\"";
                result += (!readOnly) ? "" : " readonly";
                result += (!disabled) ? "" : " disabled";
                result += ">";



            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// input for date
        /// </summary>
        /// <param name="core"></param>
        /// <param name="htmlName"></param>
        /// <param name="htmlValue"></param>
        /// <param name="Width"></param>
        /// <param name="htmlId"></param>
        /// <param name="htmlClass"></param>
        /// <param name="readOnly"></param>
        /// <param name="required"></param>
        /// <param name="disabled"></param>
        /// <returns></returns>
        public static string inputDate( CoreController core, string htmlName, DateTime? htmlValue, string Width = "", string htmlId = "", string htmlClass = "", bool readOnly = false, bool required = false, bool disabled = false) {
            string result = "";
            try {
                // {0:s} 
                // yyyy-MM-dd
                core.doc.formInputTextCnt += 1;
                core.doc.inputDateCnt = core.doc.inputDateCnt + 1;
                string attrList = " type=date name=\"" + HtmlController.encodeHtml(htmlName) + "\"";
                if ((htmlValue!=null) && (htmlValue > DateTime.MinValue)) attrList += " value=\"" + encodeDate( htmlValue ).ToString("yyyy-MM-dd") + "\"";              
                attrList += (string.IsNullOrEmpty(htmlId)) ? "" : " id=\"" + htmlId + "\"";
                attrList += (string.IsNullOrEmpty(htmlClass)) ? "" : " class=\"" + htmlClass + "\"";
                attrList += (!readOnly) ? "" : " readonly";
                attrList += (!disabled) ? "" : " disabled";
                attrList += (!required) ? "" : " required";
                return "<input" + attrList + ">";
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// HTML Form file upload input
        /// </summary>
        /// <param name="TagName"></param>
        /// <param name="htmlId"></param>
        /// <param name="HtmlClass"></param>
        /// <returns></returns>
        public string inputFile(string TagName, string htmlId = "", string HtmlClass = "") {
            //
            return "<input TYPE=\"file\" name=\"" + TagName + "\" id=\"" + htmlId + "\" class=\"" + HtmlClass + "\">";
            //
        }
        //
        //====================================================================================================
        //
        public string inputRadio(string htmlName, string htmlValue, string CurrentValue, string htmlId, string htmlClass) {
            string result = "<input type=radio name=\"" + htmlName + "\"";
            result += (string.IsNullOrEmpty(htmlValue)) ? "" : " value=\"" + htmlValue + "\"";
            result += (string.IsNullOrEmpty(htmlId)) ? "" : " id=\"" + htmlId + "\"";
            result += (string.IsNullOrEmpty(htmlClass)) ? "" : " class=\"" + htmlClass + "\"";
            result += (htmlValue != CurrentValue) ? "" : " checked";
            return result + ">";
        }
        //
        public string inputRadio(string htmlName, string htmlValue, string CurrentValue, string htmlId) => inputRadio(htmlName, htmlValue, CurrentValue, htmlId, "");
        //
        public string inputRadio(string htmlName, string htmlValue, string CurrentValue) => inputRadio(htmlName, htmlValue, CurrentValue, "", "");
        //
        public string inputRadio(string htmlName, int htmlValue, int CurrentValue, string htmlId, string htmlClass) {
            string result = "<input type=radio name=\"" + htmlName + "\" value=\"" + htmlValue + "\"";
            result += (string.IsNullOrEmpty(htmlId)) ? "" : " id=\"" + htmlId + "\"";
            result += (string.IsNullOrEmpty(htmlClass)) ? "" : " class=\"" + htmlClass + "\"";
            result += (htmlValue != CurrentValue) ? "" : " checked";
            return result + ">";
        }
        //
        public string inputRadio(string htmlName, int htmlValue, int CurrentValue, string htmlId) => inputRadio(htmlName, htmlValue, CurrentValue, htmlId, "");
        //
        public string inputRadio(string htmlName, int htmlValue, int CurrentValue) => inputRadio(htmlName, htmlValue, CurrentValue, "", "");
        //
        //====================================================================================================
        //
        public static string checkbox(string TagName, string DefaultValue) {
            return checkbox(TagName, GenericController.encodeBoolean(DefaultValue));
        }
        //
        //====================================================================================================
        //
        public static string checkbox(string htmlName, bool selected = false, string htmlId = "", bool disabled = false, string htmlClass = "", bool readOnly = false, string htmlValue="1", string label = "") {
            string result = "<input type=\"checkbox\" name=\"" + htmlName + "\" value=\"" + htmlValue + "\"";
            if (readOnly && !selected) {
                result += " disabled";
            } else if (readOnly) {
                result += " disabled checked";
            } else if (selected) {
                result += " checked";
            }
            if (!string.IsNullOrWhiteSpace(htmlId)) {
                result +=  " id=\"" + htmlId + "\"";
            }
            if (!string.IsNullOrWhiteSpace(htmlClass)) {
                result +=  " class=\"" + htmlClass + "\"";
            }
            result += ">";
            if (!string.IsNullOrWhiteSpace(label)) {
                result = div( "<label>" + result + "&nbsp;" + label + "</label>", "checkbox");
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return an html input element for the field in the current dataset
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="ContentName"></param>
        /// <param name="fieldName"></param>
        /// <param name="Height"></param>
        /// <param name="Width"></param>
        /// <param name="htmlId"></param>
        /// <returns></returns>
        public string inputCs(CsModel cs, string ContentName, string fieldName, int Height = 1, int Width = 40, string htmlId = "") {
            string returnResult = "";
            try {
                bool fieldFound = false;
                var contentMetadata = ContentMetadataModel.createByUniqueName(core, ContentName);
                string FieldValueVariant = "";
                CPContentBaseClass.fileTypeIdEnum fieldTypeId = 0;
                bool FieldReadOnly = false;
                bool FieldPassword = false;
                int FieldLookupContentID = 0;
                int FieldMemberSelectGroupID = 0;
                bool FieldHTMLContent = false;
                string FieldLookupList = "";
                foreach (KeyValuePair<string, ContentFieldMetadataModel> keyValuePair in contentMetadata.fields) {
                    ContentFieldMetadataModel field = keyValuePair.Value;
                    if (field.nameLc == fieldName.ToLower()) {
                        FieldValueVariant = field.defaultValue;
                        fieldTypeId = field.fieldTypeId;
                        FieldReadOnly = field.readOnly;
                        string FieldCaption = field.caption;
                        FieldPassword = field.password;
                        FieldHTMLContent = field.htmlContent;
                        FieldLookupContentID = field.lookupContentID;
                        FieldLookupList = field.lookupList;
                        FieldMemberSelectGroupID = field.memberSelectGroupId_get(core);
                        fieldFound = true;
                        break;
                    }
                }
                if (!fieldFound) {
                    LogController.handleError(core, new Exception("Field [" + fieldName + "] was not found in Content Definition [" + ContentName + "]"));
                } else {
                    //
                    // main_Get the current value if the record was found
                    //
                    if (cs.ok()) { FieldValueVariant = cs.getRawData(fieldName); }
                    string FieldValueText = null;
                    //
                    if (FieldPassword) {
                        //
                        // Handle Password Fields
                        //
                        FieldValueText = GenericController.encodeText(FieldValueVariant);
                        returnResult = inputText(core, fieldName, FieldValueText, Height, Width, "", true);
                    } else {
                        string FieldLookupContentName = null;
                        int FieldValueInteger = 0;
                        //
                        // Non Password field by fieldtype
                        //
                        switch (fieldTypeId) {
                            case CPContentBaseClass.fileTypeIdEnum.HTML:
                                FieldValueText = GenericController.encodeText(FieldValueVariant);
                                if (FieldReadOnly) {
                                    returnResult = FieldValueText;
                                } else {
                                    returnResult = getFormInputHTML(fieldName, FieldValueText, "", Width.ToString());
                                }
                                break;
                            case CPContentBaseClass.fileTypeIdEnum.FileHTML:
                                //
                                // html files, read from cdnFiles and use html editor
                                //
                                FieldValueText = GenericController.encodeText(FieldValueVariant);
                                if (!string.IsNullOrEmpty(FieldValueText)) {
                                    FieldValueText = core.cdnFiles.readFileText(FieldValueText);
                                }
                                if (FieldReadOnly) {
                                    returnResult = FieldValueText;
                                } else {
                                    //Height = encodeEmptyInteger(Height, 4)
                                    returnResult = getFormInputHTML(fieldName, FieldValueText, "", Width.ToString());
                                }
                                break;
                            case CPContentBaseClass.fileTypeIdEnum.FileText:
                                //
                                // text cdnFiles files, read from cdnFiles and use text editor
                                //
                                FieldValueText = GenericController.encodeText(FieldValueVariant);
                                if (!string.IsNullOrEmpty(FieldValueText)) {
                                    FieldValueText = core.cdnFiles.readFileText(FieldValueText);
                                }
                                if (FieldReadOnly) {
                                    returnResult = FieldValueText;
                                } else {
                                    //Height = encodeEmptyInteger(Height, 4)
                                    returnResult = inputText(core, fieldName, FieldValueText, Height, Width);
                                }
                                //
                                // text public files, read from core.cdnFiles and use text editor
                                //
                                break;
                            case CPContentBaseClass.fileTypeIdEnum.FileCSS:
                            case CPContentBaseClass.fileTypeIdEnum.FileXML:
                            case CPContentBaseClass.fileTypeIdEnum.FileJavascript:
                                FieldValueText = GenericController.encodeText(FieldValueVariant);
                                if (!string.IsNullOrEmpty(FieldValueText)) {
                                    FieldValueText = core.cdnFiles.readFileText(FieldValueText);
                                }
                                if (FieldReadOnly) {
                                    returnResult = FieldValueText;
                                } else {
                                    returnResult = inputText(core, fieldName, FieldValueText, Height, Width);
                                }
                                break;
                            case CPContentBaseClass.fileTypeIdEnum.Boolean:
                                if (FieldReadOnly) {
                                    returnResult = GenericController.encodeText(GenericController.encodeBoolean(FieldValueVariant));
                                } else {
                                    returnResult = checkbox(fieldName, GenericController.encodeBoolean(FieldValueVariant));
                                }
                                break;
                            case CPContentBaseClass.fileTypeIdEnum.AutoIdIncrement:
                                returnResult = GenericController.encodeText(GenericController.encodeNumber(FieldValueVariant));
                                break;
                            case CPContentBaseClass.fileTypeIdEnum.Float:
                            case CPContentBaseClass.fileTypeIdEnum.Currency:
                            case CPContentBaseClass.fileTypeIdEnum.Integer:
                                FieldValueVariant = GenericController.encodeNumber(FieldValueVariant).ToString();
                                if (FieldReadOnly) {
                                    returnResult = GenericController.encodeText(FieldValueVariant);
                                } else {
                                    returnResult = inputText(core, fieldName, GenericController.encodeText(FieldValueVariant), Height, Width);
                                }
                                break;
                            case CPContentBaseClass.fileTypeIdEnum.File:
                                FieldValueText = GenericController.encodeText(FieldValueVariant);
                                if (FieldReadOnly) {
                                    returnResult = FieldValueText;
                                } else {
                                    returnResult = FieldValueText + "<br>change: " + inputFile(fieldName, GenericController.encodeText(FieldValueVariant));
                                }
                                break;
                            case CPContentBaseClass.fileTypeIdEnum.FileImage:
                                FieldValueText = GenericController.encodeText(FieldValueVariant);
                                if (FieldReadOnly) {
                                    returnResult = FieldValueText;
                                } else {
                                    returnResult = "<img src=\"" + GenericController.getCdnFileLink(core, FieldValueText) + "\"><br>change: " + inputFile(fieldName, GenericController.encodeText(FieldValueVariant));
                                }
                                break;
                            case CPContentBaseClass.fileTypeIdEnum.Lookup:
                                FieldValueInteger = GenericController.encodeInteger(FieldValueVariant);
                                FieldLookupContentName = MetadataController.getContentNameByID(core, FieldLookupContentID);
                                if (!string.IsNullOrEmpty(FieldLookupContentName)) {
                                    //
                                    // Lookup into Content
                                    //
                                    if (FieldReadOnly) {
                                        using (CsModel csLookup = new CsModel(core)) {
                                            if (cs.openRecord(FieldLookupContentName, FieldValueInteger)) {
                                                returnResult = csLookup.getTextEncoded("name");
                                            }
                                        }
                                    } else {
                                        bool IsEmptyList = false;
                                        returnResult = selectFromContent(fieldName, FieldValueInteger, FieldLookupContentName, "", "", "", ref IsEmptyList);
                                    }
                                } else if (!string.IsNullOrEmpty(FieldLookupList)) {
                                    //
                                    // Lookup into LookupList
                                    //
                                    returnResult = selectFromList(core, fieldName, FieldValueInteger, FieldLookupList.Split(','), "", "");
                                } else {
                                    //
                                    // Just call it text
                                    //
                                    returnResult = inputText(core, fieldName, FieldValueInteger.ToString(), Height, Width);
                                }
                                break;
                            case CPContentBaseClass.fileTypeIdEnum.MemberSelect:
                                FieldValueInteger = GenericController.encodeInteger(FieldValueVariant);
                                returnResult = selectUserFromGroup(fieldName, FieldValueInteger, FieldMemberSelectGroupID);
                                break;
                            default:
                                FieldValueText = GenericController.encodeText(FieldValueVariant);
                                if (FieldReadOnly) {
                                    returnResult = FieldValueText;
                                } else {
                                    if (FieldHTMLContent) {
                                        returnResult = getFormInputHTML(fieldName, FieldValueText, Height.ToString(), Width.ToString(), FieldReadOnly, false);
                                    } else {
                                        returnResult = inputText(core, fieldName, FieldValueText, Height, Width);
                                    }
                                }
                                break;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnResult;
        }
        //
        //====================================================================================================
        //
        public static string inputSubmit(string htmlValue, string htmlName = "button", string htmlId = "", string onClick = "", bool disabled = false, string htmlClass = "") {
            string attrList = "<input type=submit name=\"" + HtmlController.encodeHtml(htmlName) + "\"";
            attrList += (string.IsNullOrEmpty(htmlName)) ? "" : " name=\"" + htmlName + "\"";
            attrList += (string.IsNullOrEmpty(htmlValue)) ? "" : " value=\"" + htmlValue + "\"";
            attrList += (string.IsNullOrEmpty(htmlId)) ? "" : " id=\"" + htmlId + "\"";
            attrList += (string.IsNullOrEmpty(htmlClass)) ? "" : " class=\"" + htmlClass + "\"";
            attrList += (string.IsNullOrEmpty(onClick)) ? "" : " onclick=\"" + onClick + "\"";
            attrList += (!disabled) ? "" : " disabled";
            return attrList + ">";
        }
        //
        //====================================================================================================
        //
        public static string inputHidden(string htmlName, string htmlValue, string htmlClass, string htmlId) {
            string attrList = "<input type=hidden name=\"" + HtmlController.encodeHtml(htmlName) + "\"";
            attrList += (string.IsNullOrEmpty(htmlName)) ? "" : " name=\"" + htmlName + "\"";
            attrList += (string.IsNullOrEmpty(htmlValue)) ? "" : " value=\"" + htmlValue + "\"";
            attrList += (string.IsNullOrEmpty(htmlId)) ? "" : " id=\"" + htmlId + "\"";
            attrList += (string.IsNullOrEmpty(htmlClass)) ? "" : " class=\"" + htmlClass + "\"";
            return attrList + ">";
        }
        public static string inputHidden(string name, string value) => inputHidden(name, value, "", "");
        public static string inputHidden(string name, string value, string htmlClass) => inputHidden(name, value, htmlClass, "");
        //
        public static string inputHidden(string TagName, bool TagValue) => inputHidden(TagName, TagValue.ToString());
        public static string inputHidden(string TagName, bool TagValue, string htmlClass) => inputHidden(TagName, TagValue.ToString(), htmlClass);
        public static string inputHidden(string TagName, bool TagValue, string htmlClass, string htmlId) => inputHidden(TagName, TagValue.ToString(), htmlClass, htmlId);
        //
        public static string inputHidden(string TagName, int TagValue) => inputHidden(TagName, TagValue.ToString());
        public static string inputHidden(string TagName, int TagValue, string htmlClass) => inputHidden(TagName, TagValue.ToString(), htmlClass);
        public static string inputHidden(string TagName, int TagValue, string htmlClass, string htmlId) => inputHidden(TagName, TagValue.ToString(), htmlClass, htmlId);
        //
        public static string inputHidden(string TagName, double TagValue) => inputHidden(TagName, TagValue.ToString());
        public static string inputHidden(string TagName, double TagValue, string htmlClass) => inputHidden(TagName, TagValue.ToString(), htmlClass);
        public static string inputHidden(string TagName, double TagValue, string htmlClass, string htmlId) => inputHidden(TagName, TagValue.ToString(), htmlClass, htmlId);
        //
        public static string inputHidden(string TagName, DateTime TagValue) => inputHidden(TagName, TagValue.ToString());
        public static string inputHidden(string TagName, DateTime TagValue, string htmlClass) => inputHidden(TagName, TagValue.ToString(), htmlClass);
        public static string inputHidden(string TagName, DateTime TagValue, string htmlClass, string htmlId) => inputHidden(TagName, TagValue.ToString(), htmlClass, htmlId);
        //
        //====================================================================================================
        //
        public void javascriptAddEvent(string HtmlId, string DOMEvent, string Javascript) {
            string JSCodeAsString = Javascript;
            JSCodeAsString = GenericController.vbReplace(JSCodeAsString, "'", "'+\"'\"+'");
            JSCodeAsString = GenericController.vbReplace(JSCodeAsString, "\r\n", "\\n");
            JSCodeAsString = GenericController.vbReplace(JSCodeAsString, "\r", "\\n");
            JSCodeAsString = GenericController.vbReplace(JSCodeAsString, "\n", "\\n");
            JSCodeAsString = "'" + JSCodeAsString + "'";
            addScriptCode_onLoad("cj.addListener(document.getElementById('" + HtmlId + "'),'" + DOMEvent + "',function(){eval(" + JSCodeAsString + ")})", "");
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns a json format list of all addons that can be placed into the contentType provided (page, mail). Cached in doc scope
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public string getWysiwygAddonList( BaseClasses.CPHtml5BaseClass.EditorContentType contentType) {
            string result = "";
            try {
                if (core.doc.wysiwygAddonList.ContainsKey(contentType)) {
                    result = core.doc.wysiwygAddonList[contentType];
                } else {
                    //
                    // -- AC Tags, Would like to replace these with Add-ons eventually
                    int ItemsSize = 100;
                    string[] Items = new string[101];
                    int ItemsCnt = 0;
                    var Index = new KeyPtrController();
                    //
                    // -- AC StartBlockText
                    string IconIDControlString = "AC," + ACTypeAggregateFunction + ",0,Block Text,";
                    string IconImg = AddonController.getAddonIconImg("/" + core.appConfig.adminRoute, 0, 0, 0, true, IconIDControlString, "", core.appConfig.cdnFileUrl, "Text Block Start", "Block text to all except selected groups starting at this point", "", 0);
                    IconImg = GenericController.EncodeJavascriptStringSingleQuote(IconImg);
                    Items[ItemsCnt] = "['Block Text','" + IconImg + "']";
                    Index.setPtr("Block Text", ItemsCnt);
                    ItemsCnt += 1;
                    //
                    // AC EndBlockText
                    //
                    IconIDControlString = "AC," + ACTypeAggregateFunction + ",0,Block Text End,";
                    IconImg = AddonController.getAddonIconImg("/" + core.appConfig.adminRoute, 0, 0, 0, true, IconIDControlString, "", core.appConfig.cdnFileUrl, "Text Block End", "End of text block", "", 0);
                    IconImg = GenericController.EncodeJavascriptStringSingleQuote(IconImg);
                    Items[ItemsCnt] = "['Block Text End','" + IconImg + "']";
                    Index.setPtr("Block Text", ItemsCnt);
                    ItemsCnt += 1;
                    //
                    if ((contentType == CPHtml5BaseClass.EditorContentType.contentTypeEmail) || (contentType == CPHtml5BaseClass.EditorContentType.contentTypeEmailTemplate)) {
                        //
                        // ----- Email Only AC tags
                        //
                        // Editing Email Body or Templates - Since Email can not process Add-ons, it main_Gets the legacy AC tags for now
                        //
                        // Personalization Tag
                        //
                        string selectOptions = "";
                        var peopleMetaData = ContentMetadataModel.createByUniqueName(core, "people");
                        if (peopleMetaData != null) selectOptions = string.Join("|", peopleMetaData.selectList);
                        IconIDControlString = "AC,PERSONALIZATION,0,Personalization,field=[" + selectOptions + "]";
                        IconImg = AddonController.getAddonIconImg("/" + core.appConfig.adminRoute, 0, 0, 0, true, IconIDControlString, "", core.appConfig.cdnFileUrl, "Any Personalization Field", "Renders as any Personalization Field", "", 0);
                        IconImg = GenericController.EncodeJavascriptStringSingleQuote(IconImg);
                        Items[ItemsCnt] = "['Personalization','" + IconImg + "']";
                        Index.setPtr("Personalization", ItemsCnt);
                        ItemsCnt += 1;
                        //
                        if (contentType == CPHtml5BaseClass.EditorContentType.contentTypeEmailTemplate) {
                            //
                            // Editing Email Templates
                            //   This is a special case
                            //   Email content processing can not process add-ons, and PageContentBox and TextBox are needed
                            //   So I added the old AC Tag into the menu for this case
                            //   Need a more consistant solution later
                            //
                            IconIDControlString = "AC," + ACTypeTemplateContent + ",0,Template Content,";
                            IconImg = AddonController.getAddonIconImg("/" + core.appConfig.adminRoute, 52, 64, 0, false, IconIDControlString, "/ContensiveBase/images/ACTemplateContentIcon.gif", core.appConfig.cdnFileUrl, "Content Box", "Renders as the content for a template", "", 0);
                            IconImg = GenericController.EncodeJavascriptStringSingleQuote(IconImg);
                            Items[ItemsCnt] = "['Content Box','" + IconImg + "']";
                            //Items(ItemsCnt) = "['Template Content','<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Content"" id=""AC," & ACTypeTemplateContent & ",0,Template Content,"" src=""/ContensiveBase/images/ACTemplateContentIcon.gif"" WIDTH=52 HEIGHT=64>']"
                            Index.setPtr("Content Box", ItemsCnt);
                            ItemsCnt += 1;
                            //
                            IconIDControlString = "AC," + ACTypeTemplateText + ",0,Template Text,Name=Default";
                            IconImg = AddonController.getAddonIconImg("/" + core.appConfig.adminRoute, 52, 52, 0, false, IconIDControlString, "/ContensiveBase/images/ACTemplateTextIcon.gif", core.appConfig.cdnFileUrl, "Template Text", "Renders as a template text block", "", 0);
                            IconImg = GenericController.EncodeJavascriptStringSingleQuote(IconImg);
                            Items[ItemsCnt] = "['Template Text','" + IconImg + "']";
                            //Items(ItemsCnt) = "['Template Text','<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Text"" id=""AC," & ACTypeTemplateText & ",0,Template Text,Name=Default"" src=""/ContensiveBase/images/ACTemplateTextIcon.gif"" WIDTH=52 HEIGHT=52>']"
                            Index.setPtr("Template Text", ItemsCnt);
                            ItemsCnt += 1;
                        }
                    } else {
                        //
                        // ----- Web Only AC Tags
                        //
                        // Watch Lists
                        //
                        using (var csData = new CsModel(core)) {
                            if (csData.open("Content Watch Lists", "", "Name,ID", false, 0, "Name,ID", 20, 1)) {
                                while (csData.ok()) {
                                    string FieldName = encodeText(csData.getText("name")).Trim(' ');
                                    if (!string.IsNullOrEmpty(FieldName)) {
                                        string FieldCaption = "Watch List [" + FieldName + "]";
                                        IconIDControlString = "AC,WATCHLIST,0," + FieldName + ",ListName=" + FieldName + "&SortField=[DateAdded|Link|LinkLabel|Clicks|WhatsNewDateExpires]&SortDirection=Z-A[A-Z|Z-A]";
                                        IconImg = AddonController.getAddonIconImg("/" + core.appConfig.adminRoute, 0, 0, 0, true, IconIDControlString, "", core.appConfig.cdnFileUrl, FieldCaption, "Rendered as the " + FieldCaption, "", 0);
                                        IconImg = GenericController.EncodeJavascriptStringSingleQuote(IconImg);
                                        FieldCaption = GenericController.EncodeJavascriptStringSingleQuote(FieldCaption);
                                        Items[ItemsCnt] = "['" + FieldCaption + "','" + IconImg + "']";
                                        Index.setPtr(FieldCaption, ItemsCnt);
                                        ItemsCnt += 1;
                                        if (ItemsCnt >= ItemsSize) {
                                            ItemsSize = ItemsSize + 100;
                                            Array.Resize(ref Items, ItemsSize + 1);
                                        }
                                    }
                                    csData.goNext();
                                }
                            }
                        }
                    }
                    //
                    // -- addons
                    string Criteria = "(1=1)";
                    if (contentType == CPHtml5BaseClass.EditorContentType.contentTypeEmail) {
                        //
                        // select only addons with email placement (dont need to check main_version bc if email, must be >4.0.325
                        //
                        Criteria = Criteria + "and(email<>0)";
                    } else {
                        if (contentType == CPHtml5BaseClass.EditorContentType.contentTypeWeb) {
                            //
                            // Non Templates
                            Criteria = Criteria + "and(content<>0)";
                        } else {
                            //
                            // Templates
                            Criteria = Criteria + "and(template<>0)";
                        }
                    }
                    string AddonContentName = Models.Db.AddonModel.contentName;
                    string SelectList = "Name,Link,ID,ArgumentList,ObjectProgramID,IconFilename,IconWidth,IconHeight,IconSprites,IsInline,ccguid";
                    using (var csData = new CsModel(core)) {
                        if (csData.open(AddonContentName, Criteria, "Name,ID", false, 0, SelectList)) {
                            string LastAddonName = "";
                            while (csData.ok()) {
                                string addonGuid = csData.getText("ccguid");
                                string ObjectProgramID2 = csData.getText("ObjectProgramID");
                                if ((contentType == CPHtml5BaseClass.EditorContentType.contentTypeEmail) && (!string.IsNullOrEmpty(ObjectProgramID2))) {
                                    //
                                    // Block activex addons from email
                                    //
                                    //ObjectProgramID2 = ObjectProgramID2;
                                } else {
                                    string addonName = encodeText(csData.getText("name")).Trim(' ');
                                    if (!string.IsNullOrEmpty(addonName) && (addonName != LastAddonName)) {
                                        //
                                        // Icon (fieldtyperesourcelink)
                                        //
                                        bool IsInline = csData.getBoolean("IsInline");
                                        string IconFilename = csData.getText("Iconfilename");
                                        int IconWidth = 0;
                                        int IconHeight = 0;
                                        int IconSprites = 0;
                                        if (!string.IsNullOrEmpty(IconFilename)) {
                                            IconWidth = csData.getInteger("IconWidth");
                                            IconHeight = csData.getInteger("IconHeight");
                                            IconSprites = csData.getInteger("IconSprites");
                                        }
                                        //
                                        // Calculate DefaultAddonOption_String
                                        //
                                        string ArgumentList = csData.getText("ArgumentList").Trim(' ');
                                        string defaultAddonOptions = AddonController.getDefaultAddonOptions(core, ArgumentList, addonGuid, IsInline);
                                        defaultAddonOptions = encodeHtml(defaultAddonOptions);
                                        LastAddonName = addonName;
                                        IconIDControlString = "AC,AGGREGATEFUNCTION,0," + addonName + "," + defaultAddonOptions + "," + addonGuid;
                                        IconImg = AddonController.getAddonIconImg("/" + core.appConfig.adminRoute, IconWidth, IconHeight, IconSprites, IsInline, IconIDControlString, IconFilename, core.appConfig.cdnFileUrl, addonName, "Rendered as the Add-on [" + addonName + "]", "", 0);
                                        Items[ItemsCnt] = "['" + GenericController.EncodeJavascriptStringSingleQuote(addonName) + "','" + GenericController.EncodeJavascriptStringSingleQuote(IconImg) + "']";
                                        Index.setPtr(addonName, ItemsCnt);
                                        ItemsCnt += 1;
                                        if (ItemsCnt >= ItemsSize) {
                                            ItemsSize = ItemsSize + 100;
                                            Array.Resize(ref Items, ItemsSize + 1);
                                        }
                                    }
                                }
                                csData.goNext();
                            }
                        }
                    }
                    //
                    // Build output sting in alphabetical order by name
                    //
                    string s = "";
                    int ItemsPtr = Index.getFirstPtr();
                    int LoopPtr = 0;
                    while ((ItemsPtr >= 0) && (LoopPtr < ItemsCnt)) {
                        s = s + "\r\n," + Items[ItemsPtr];
                        int PtrTest = Index.getNextPtr();
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
                    core.doc.wysiwygAddonList.Add(contentType, result);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// replace string new-line with html break
        /// </summary>
        public string convertNewLineToHtmlBreak(string text) {
            if (string.IsNullOrWhiteSpace(text)) return "";
            return text.Replace(windowsNewLine, "<br>").Replace(unixNewLine, "<br>").Replace(macNewLine, "<br>");
        }
        //
        //====================================================================================================
        /// <summary>
        /// Convert an HTML source to a text equivelent. converts CRLF to <br>, encodes reserved HTML characters to their equivalent
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string convertTextToHtml(string text) {
            return convertNewLineToHtmlBreak(HtmlController.encodeHtml(text));
        }
        //
        //====================================================================================================
        /// <summary>
        /// Get Addon Selector
        ///
        ///   The addon selector is the string sent out with the content in edit-mode. In the editor, it is converted by javascript
        ///   to the popup window that selects instance options. It is in this format:
        ///
        ///   Select (creates a list of names in a select box, returns the selected name)
        ///       name=currentvalue[optionname0:optionvalue0|optionname1:optionvalue1|...]
        ///   CheckBox (creates a list of names in checkboxes, and returns the selected names)
        /// </summary>
        /// <param name="SrcOptionName"></param>
        /// <param name="InstanceOptionValue_AddonEncoded"></param>
        /// <param name="SrcOptionValueSelector"></param>
        /// <returns></returns>
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
                //string SrcSelectorInner = null;
                string SrcSelector = SrcOptionValueSelector.Trim(' ');
                //
                string SrcSelectorInner = SrcSelector;
                int PosLeft = GenericController.vbInstr(1, SrcSelector, "[");
                string SrcSelectorSuffix = "";
                if (PosLeft != 0) {
                    int PosRight = GenericController.vbInstr(1, SrcSelector, "]");
                    if (PosRight != 0) {
                        if (PosRight < SrcSelector.Length) {
                            SrcSelectorSuffix = SrcSelector.Substring(PosRight);
                        }
                        SrcSelector = (SrcSelector.Substring(PosLeft - 1, PosRight - PosLeft + 1)).Trim(' ');
                        SrcSelectorInner = (SrcSelector.Substring(1, SrcSelector.Length - 2)).Trim(' ');
                    }
                }
                //
                // Break SrcSelectorInner up into individual choices to detect functions
                //
                string list = "";
                int Pos = 0;
                if (!string.IsNullOrEmpty(SrcSelectorInner)) {
                    string[] Choices = SrcSelectorInner.Split('|');
                    int ChoiceCnt = Choices.GetUpperBound(0) + 1;
                    int Ptr = 0;
                    for (Ptr = 0; Ptr < ChoiceCnt; Ptr++) {
                        bool IsContentList = false;
                        bool IsListField = false;
                        //
                        // List Function (and all the indecision that went along with it)
                        //
                        bool IncludeID = false;
                        int FnLen = 0;
                        string Choice = Choices[Ptr];
                        if (Pos == 0) {
                            Pos = GenericController.vbInstr(1, Choice, ACFunctionList1 + "(", 1);
                            if (Pos > 0) {
                                IsContentList = true;
                                IncludeID = false;
                                FnLen = ACFunctionList1.Length;
                            }
                        }
                        if (Pos == 0) {
                            Pos = GenericController.vbInstr(1, Choice, ACFunctionList2 + "(", 1);
                            if (Pos > 0) {
                                IsContentList = true;
                                IncludeID = false;
                                FnLen = ACFunctionList2.Length;
                            }
                        }
                        if (Pos == 0) {
                            Pos = GenericController.vbInstr(1, Choice, ACFunctionList3 + "(", 1);
                            if (Pos > 0) {
                                IsContentList = true;
                                IncludeID = false;
                                FnLen = ACFunctionList3.Length;
                            }
                        }
                        if (Pos == 0) {
                            Pos = GenericController.vbInstr(1, Choice, ACFunctionListID + "(", 1);
                            if (Pos > 0) {
                                IsContentList = true;
                                IncludeID = true;
                                FnLen = ACFunctionListID.Length;
                            }
                        }
                        if (Pos == 0) {
                            Pos = GenericController.vbInstr(1, Choice, ACFunctionList + "(", 1);
                            if (Pos > 0) {
                                IsContentList = true;
                                IncludeID = false;
                                FnLen = ACFunctionList.Length;
                            }
                        }
                        if (Pos == 0) {
                            Pos = GenericController.vbInstr(1, Choice, ACFunctionListFields + "(", 1);
                            if (Pos > 0) {
                                IsListField = true;
                                IncludeID = false;
                                FnLen = ACFunctionListFields.Length;
                            }
                        }
                        //
                        if (Pos > 0) {
                            //
                            string FnArgList = (Choice.Substring((Pos + FnLen) - 1)).Trim(' ');
                            string ContentName = "";
                            string ContentCriteria = "";
                            if ((FnArgList.Left(1) == "(") && (FnArgList.Substring(FnArgList.Length - 1) == ")")) {
                                //
                                // set ContentName and ContentCriteria from argument list
                                //
                                FnArgList = FnArgList.Substring(1, FnArgList.Length - 2);
                                string[] FnArgs = GenericController.SplitDelimited(FnArgList, ",");
                                int FnArgCnt = FnArgs.GetUpperBound(0) + 1;
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
                            using (var csData = new CsModel(core)) {
                                if (IsContentList) {
                                    //
                                    // ContentList - Open the Content and build the options from the names
                                    csData.open(ContentName, ContentCriteria, "name", true, 0, "ID,Name");
                                } else if (IsListField) {
                                    //
                                    //
                                    // ListField
                                    int CID = ContentMetadataModel.getContentId(core, ContentName);
                                    csData.open("Content Fields", "Contentid=" + CID, "name", true, 0, "ID,Name");
                                }

                                if (csData.ok()) {
                                    object[,] Cell = csData.getRows();
                                    int RowCnt = Cell.GetUpperBound(1) + 1;
                                    int RowPtr = 0;
                                    for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                                        //
                                        string RecordName = GenericController.encodeText(Cell[1, RowPtr]);
                                        RecordName = GenericController.vbReplace(RecordName, "\r\n", " ");
                                        int RecordID = GenericController.encodeInteger(Cell[0, RowPtr]);
                                        if (string.IsNullOrEmpty(RecordName)) {
                                            RecordName = "record " + RecordID;
                                        } else if (RecordName.Length > 50) {
                                            RecordName = RecordName.Left(50) + "...";
                                        }
                                        RecordName = GenericController.encodeNvaArgument(RecordName);
                                        list = list + "|" + RecordName;
                                        if (IncludeID) {
                                            list = list + ":" + RecordID;
                                        }
                                    }
                                }
                            }
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
                result = HtmlController.encodeHtml(GenericController.encodeNvaArgument(SrcOptionName)) + "=";
                if (!string.IsNullOrEmpty(InstanceOptionValue_AddonEncoded)) {
                    result += HtmlController.encodeHtml(InstanceOptionValue_AddonEncoded);
                }
                if (string.IsNullOrEmpty(SrcSelectorSuffix) && string.IsNullOrEmpty(list)) {
                    //
                    // empty list with no suffix, return with name=value
                    //
                } else if (GenericController.vbLCase(SrcSelectorSuffix) == "resourcelink") {
                    //
                    // resource link, exit with empty list
                    //
                    result += "[]ResourceLink";
                } else {
                    //
                    //
                    //
                    result += "[" + list + "]" + SrcSelectorSuffix;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public string getFormInputHTML(string htmlName, string DefaultValue = "", string styleHeight = "", string styleWidth = "", bool readOnlyfield = false, bool allowActiveContent = false, string addonListJSON = "", string styleList = "", string styleOptionList = "", bool allowResourceLibrary = false) {
            string returnHtml = "";
            try {
                var FieldTypeDefaultEditorAddonIds = EditorController.getFieldTypeDefaultEditorAddonIdDictionary(core);
                int FieldTypeDefaultEditorAddonId = GenericController.encodeInteger(FieldTypeDefaultEditorAddonIds[(int)CPContentBaseClass.fileTypeIdEnum.HTML]);
                if (FieldTypeDefaultEditorAddonId == 0) {
                    //
                    //    use default wysiwyg
                    returnHtml = inputTextarea(core, htmlName, DefaultValue);
                } else {
                    // todo ----- move this (defaulteditor override) to AdminUI
                    //
                    // use addon editor
                    Dictionary<string, string> arguments = new Dictionary<string, string> {
                        { "editorName", htmlName },
                        { "editorValue", DefaultValue },
                        { "editorFieldType", ((int)CPContentBaseClass.fileTypeIdEnum.HTML).ToString() },
                        { "editorReadOnly", readOnlyfield.ToString() },
                        { "editorWidth", styleWidth },
                        { "editorHeight", styleHeight },
                        { "editorAllowResourceLibrary", allowResourceLibrary.ToString() },
                        { "editorAllowActiveContent", allowActiveContent.ToString() },
                        { "editorAddonList", addonListJSON },
                        { "editorStyles", styleList },
                        { "editorStyleOptions", styleOptionList }
                    };
                    returnHtml = core.addon.execute(AddonModel.create(core, FieldTypeDefaultEditorAddonId), new CPUtilsBaseClass.addonExecuteContext() {
                        addonType = CPUtilsBaseClass.addonContext.ContextEditor,
                        argumentKeyValuePairs = arguments,
                        errorContextMessage = "calling editor addon for text field type, addon [" + FieldTypeDefaultEditorAddonId  + "]"
                    });
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnHtml;
        }
        //
        //====================================================================================================
        //
        public void processFormToolsPanel(string legacyFormSn = "") {
            try {
                string Button = null;
                string username = null;
                //
                // ----- Read in and save the Member profile values from the tools panel
                //
                if (core.session.user.id > 0) {
                    if (!(core.doc.debug_iUserError != "")) {
                        Button = core.docProperties.getText(legacyFormSn + "mb");
                        switch (Button) {
                            case ButtonLogout:
                                //
                                // Logout - This can only come from the Horizonal Tool Bar
                                //
                                core.session.logout(core);
                                break;
                            case ButtonLogin:
                                //
                                // Login - This can only come from the Horizonal Tool Bar
                                //
                                Controllers.LoginController.processFormLoginDefault(core);
                                break;
                            case ButtonApply:
                                //
                                // Apply
                                //
                                username = core.docProperties.getText(legacyFormSn + "username");
                                if (!string.IsNullOrEmpty(username)) {
                                    Controllers.LoginController.processFormLoginDefault(core);
                                }
                                //
                                // ----- AllowAdminLinks
                                //
                                core.visitProperty.setProperty("AllowEditing", GenericController.encodeText(core.docProperties.getBoolean(legacyFormSn + "AllowEditing")));
                                //
                                // ----- Quick Editor
                                //
                                core.visitProperty.setProperty("AllowQuickEditor", GenericController.encodeText(core.docProperties.getBoolean(legacyFormSn + "AllowQuickEditor")));
                                //
                                // ----- Advanced Editor
                                //
                                core.visitProperty.setProperty("AllowAdvancedEditor", GenericController.encodeText(core.docProperties.getBoolean(legacyFormSn + "AllowAdvancedEditor")));
                                //
                                // ----- Allow Workflow authoring Render Mode - Visit Property
                                //
                                core.visitProperty.setProperty("AllowWorkflowRendering", GenericController.encodeText(core.docProperties.getBoolean(legacyFormSn + "AllowWorkflowRendering")));
                                //
                                // ----- developer Only parts
                                //
                                core.visitProperty.setProperty("AllowDebugging", GenericController.encodeText(core.docProperties.getBoolean(legacyFormSn + "AllowDebugging")));
                                break;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
        }
        //
        //====================================================================================================
        //
        public void processAddonSettingsEditor() {
            //
            string constructor = null;
            bool ParseOK = false;
            int PosNameStart = 0;
            int PosNameEnd = 0;
            string AddonName = null;
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
            CPContentBaseClass.fileTypeIdEnum fieldType = 0;
            string Copy = "";
            int RecordID = 0;
            string FieldName = null;
            string ACInstanceID = null;
            string ContentName = null;
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
                    AddonModel addon = AddonModel.create(core, addonGuidChildList);
                    if (addon != null) {
                        FoundAddon = true;
                        AddonOptionConstructor = addon.argumentList;
                        AddonOptionConstructor = GenericController.vbReplace(AddonOptionConstructor, "\r\n", "\r");
                        AddonOptionConstructor = GenericController.vbReplace(AddonOptionConstructor, "\n", "\r");
                        AddonOptionConstructor = GenericController.vbReplace(AddonOptionConstructor, "\r", "\r\n");
                        if (true) {
                            if (!string.IsNullOrEmpty(AddonOptionConstructor)) {
                                AddonOptionConstructor = AddonOptionConstructor + "\r\n";
                            }
                            if (addon.isInline) {
                                AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Inline;
                            } else {
                                AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Block;
                            }
                        }

                        ConstructorSplit = GenericController.stringSplit(AddonOptionConstructor, "\r\n");
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
                                        ArgValueAddonEncoded = ArgValueAddonEncoded + "," + GenericController.encodeNvaArgument(ArgValue);
                                    }
                                }
                                if (!string.IsNullOrEmpty(ArgValueAddonEncoded)) {
                                    ArgValueAddonEncoded = ArgValueAddonEncoded.Substring(1);
                                }
                            } else {
                                ArgValue = core.docProperties.getText(ArgName);
                                ArgValueAddonEncoded = GenericController.encodeNvaArgument(ArgValue);
                            }
                            addonOption_String = addonOption_String + "&" + GenericController.encodeNvaArgument(ArgName) + "=" + ArgValueAddonEncoded;
                        }
                        if (!string.IsNullOrEmpty(addonOption_String)) {
                            addonOption_String = addonOption_String.Substring(1);
                        }

                    }
                    core.db.executeQuery("update ccpagecontent set ChildListInstanceOptions=" + DbController.encodeSQLText(addonOption_String) + " where id=" + RecordID);
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
                AddonModel addon = core.addonCache.getAddonByName(AddonName);
                if (addon != null) {
                    FoundAddon = true;
                    AddonOptionConstructor = addon.argumentList;
                    AddonOptionConstructor = GenericController.vbReplace(AddonOptionConstructor, "\r\n", "\r");
                    AddonOptionConstructor = GenericController.vbReplace(AddonOptionConstructor, "\n", "\r");
                    AddonOptionConstructor = GenericController.vbReplace(AddonOptionConstructor, "\r", "\r\n");
                    if (!string.IsNullOrEmpty(AddonOptionConstructor)) {
                        AddonOptionConstructor = AddonOptionConstructor + "\r\n";
                    }
                    if (GenericController.encodeBoolean(addon.isInline)) {
                        AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Inline;
                    } else {
                        AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Block;
                    }
                }
                if (!FoundAddon) {
                    //
                    // Hardcoded Addons
                    //
                    switch (GenericController.vbLCase(AddonName)) {
                        case "block text":
                            FoundAddon = true;
                            AddonOptionConstructor = AddonOptionConstructor_ForBlockText;
                            break;
                        case "":
                            break;
                    }
                }
                if (FoundAddon) {
                    ConstructorSplit = GenericController.stringSplit(AddonOptionConstructor, "\r\n");
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
                                        ArgValueAddonEncoded = ArgValueAddonEncoded + "," + GenericController.encodeNvaArgument(ArgValue);
                                    }
                                }
                                if (!string.IsNullOrEmpty(ArgValueAddonEncoded)) {
                                    ArgValueAddonEncoded = ArgValueAddonEncoded.Substring(1);
                                }
                            } else {
                                ArgValue = core.docProperties.getText(ArgName);
                                ArgValueAddonEncoded = GenericController.encodeNvaArgument(ArgValue);
                            }
                            addonOption_String = addonOption_String + "&" + GenericController.encodeNvaArgument(ArgName) + "=" + ArgValueAddonEncoded;
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
                LogController.handleError( core,new Exception("invalid content [" + ContentName + "], RecordID [" + RecordID + "]"));
            } else {
                //
                // ----- Normal Content Edit - find instance in the content
                //
                using (var csData = new CsModel(core)) {
                    if (!csData.openRecord(ContentName, RecordID)) {
                        LogController.handleError(core, new Exception("No record found with content [" + ContentName + "] and RecordID [" + RecordID + "]"));
                    } else {
                        if (!string.IsNullOrEmpty(FieldName)) {
                            //
                            // Field is given, find the position
                            //
                            Copy = csData.getText(FieldName);
                            PosACInstanceID = GenericController.vbInstr(1, Copy, "=\"" + ACInstanceID + "\" ", 1);
                        } else {
                            //
                            // Find the field, then find the position
                            //
                            FieldName = csData.getFirstFieldName();
                            while (!string.IsNullOrEmpty(FieldName)) {
                                fieldType = csData.getFieldTypeId(FieldName);
                                switch (fieldType) {
                                    case CPContentBaseClass.fileTypeIdEnum.LongText:
                                    case CPContentBaseClass.fileTypeIdEnum.Text:
                                    case CPContentBaseClass.fileTypeIdEnum.FileText:
                                    case CPContentBaseClass.fileTypeIdEnum.FileCSS:
                                    case CPContentBaseClass.fileTypeIdEnum.FileXML:
                                    case CPContentBaseClass.fileTypeIdEnum.FileJavascript:
                                    case CPContentBaseClass.fileTypeIdEnum.HTML:
                                    case CPContentBaseClass.fileTypeIdEnum.FileHTML:
                                        Copy = csData.getText(FieldName);
                                        PosACInstanceID = GenericController.vbInstr(1, Copy, "ACInstanceID=\"" + ACInstanceID + "\"", 1);
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
                                FieldName = csData.getNextFieldName();
                            }
                            ExitLabel1:;
                        }
                        //
                        // Parse out the Addon Name
                        //
                        if (PosACInstanceID == 0) {
                            LogController.handleError(core, new Exception("AC Instance [" + ACInstanceID + "] not found in record with content [" + ContentName + "] and RecordID [" + RecordID + "]"));
                        } else {
                            Copy = ActiveContentController.optimizeLibraryFileImagesInHtmlContent(core, Copy);
                            ParseOK = false;
                            PosStart = Copy.LastIndexOf("<ac ", PosACInstanceID - 1, System.StringComparison.OrdinalIgnoreCase) + 1;
                            if (PosStart != 0) {
                                //
                                // main_Get Addon Name to lookup Addon and main_Get most recent Argument List
                                //
                                PosNameStart = GenericController.vbInstr(PosStart, Copy, " name=", 1);
                                if (PosNameStart != 0) {
                                    PosNameStart = PosNameStart + 7;
                                    PosNameEnd = GenericController.vbInstr(PosNameStart, Copy, "\"");
                                    if (PosNameEnd != 0) {
                                        AddonName = Copy.Substring(PosNameStart - 1, PosNameEnd - PosNameStart);
                                        //????? test this
                                        FoundAddon = false;
                                        AddonModel embeddedAddon = core.addonCache.getAddonByName(AddonName);
                                        if (embeddedAddon != null) {
                                            FoundAddon = true;
                                            AddonOptionConstructor = GenericController.encodeText(embeddedAddon.argumentList);
                                            AddonOptionConstructor = GenericController.vbReplace(AddonOptionConstructor, "\r\n", "\r");
                                            AddonOptionConstructor = GenericController.vbReplace(AddonOptionConstructor, "\n", "\r");
                                            AddonOptionConstructor = GenericController.vbReplace(AddonOptionConstructor, "\r", "\r\n");
                                            if (!string.IsNullOrEmpty(AddonOptionConstructor)) {
                                                AddonOptionConstructor = AddonOptionConstructor + "\r\n";
                                            }
                                            if (GenericController.encodeBoolean(embeddedAddon.isInline)) {
                                                AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Inline;
                                            } else {
                                                AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Block;
                                            }
                                        } else {
                                            //
                                            // -- Hardcoded Addons
                                            switch (GenericController.vbLCase(AddonName)) {
                                                case "block text":
                                                    FoundAddon = true;
                                                    AddonOptionConstructor = AddonOptionConstructor_ForBlockText;
                                                    break;
                                                case "":
                                                    break;
                                            }
                                        }
                                        if (FoundAddon) {
                                            ConstructorSplit = GenericController.stringSplit(AddonOptionConstructor, "\r\n");
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
                                                                ArgValueAddonEncoded = ArgValueAddonEncoded + "," + GenericController.encodeNvaArgument(ArgValue);
                                                            }
                                                        }
                                                        if (!string.IsNullOrEmpty(ArgValueAddonEncoded)) {
                                                            ArgValueAddonEncoded = ArgValueAddonEncoded.Substring(1);
                                                        }
                                                    } else {
                                                        ArgValue = core.docProperties.getText(ArgName);
                                                        ArgValueAddonEncoded = GenericController.encodeNvaArgument(ArgValue);
                                                    }

                                                    addonOption_String = addonOption_String + "&" + GenericController.encodeNvaArgument(ArgName) + "=" + ArgValueAddonEncoded;
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
                                PosIDStart = GenericController.vbInstr(PosStart, Copy, " querystring=", 1);
                                if (PosIDStart != 0) {
                                    PosIDStart = PosIDStart + 14;
                                    if (PosIDStart != 0) {
                                        PosIDEnd = GenericController.vbInstr(PosIDStart, Copy, "\"");
                                        if (PosIDEnd != 0) {
                                            ParseOK = true;
                                            Copy = Copy.Left(PosIDStart - 1) + HtmlController.encodeHtml(addonOption_String) + Copy.Substring(PosIDEnd - 1);
                                            csData.set(FieldName, Copy);
                                            needToClearCache = true;
                                        }
                                    }
                                }
                            }
                            if (!ParseOK) {
                                LogController.handleError(core, new Exception("There was a problem parsing AC Instance [" + ACInstanceID + "] record with content [" + ContentName + "] and RecordID [" + RecordID + "]"));
                            }
                        }
                    }
                }
            }
            if (needToClearCache) {
                //
                // Clear Caches
                //
                if (!string.IsNullOrEmpty(ContentName)) {
                    string contentTablename = MetadataController.getContentTablename(core, ContentName);                    
                    core.cache.invalidateAllKeysInTable(contentTablename);
                }
            }
        }
        //
        //====================================================================================================
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
            switch (GenericController.vbLCase(IDSplit[0])) {
                case "userfield":
                    //
                    // main_Get the id of the field, and save the input as the caption and help
                    //
                    if (IDSplit.GetUpperBound(0) > 0) {
                        RecordID = GenericController.encodeInteger(IDSplit[1]);
                        if (RecordID > 0) {
                            HelpCaption = core.docProperties.getText("helpcaption");
                            HelpMessage = core.docProperties.getText("helptext");
                            SQL = "update ccfields set caption=" + DbController.encodeSQLText(HelpCaption) + ",HelpMessage=" + DbController.encodeSQLText(HelpMessage) + " where id=" + RecordID;
                            core.db.executeQuery(SQL);
                            core.cache.invalidateAll();
                            core.clearMetaData();
                        }
                    }
                    break;
            }
        }
        //
        //====================================================================================================
        //
        public string getCheckList2(string TagName, string PrimaryContentName, int PrimaryRecordID, string SecondaryContentName, string RulesContentName, string RulesPrimaryFieldname, string RulesSecondaryFieldName, string SecondaryContentSelectCriteria = "", string CaptionFieldName = "", bool readOnlyfield = false) {
            return getCheckList(TagName, PrimaryContentName, PrimaryRecordID, SecondaryContentName, RulesContentName, RulesPrimaryFieldname, RulesSecondaryFieldName, SecondaryContentSelectCriteria, GenericController.encodeText(CaptionFieldName), readOnlyfield, false, "");
        }
        //
        //====================================================================================================
        /// <summary>
        /// list of checkbox options based on a standard set of rules.
        ///   IncludeContentFolderDivs
        ///       When true, the list of options (checkboxes) are grouped by ContentFolder and wrapped in a Div with ID="ContentFolder99"
        ///   For instance, list out a options of all public groups, with the ones checked that this member belongs to
        ///       PrimaryContentName = "People"
        ///       PrimaryRecordID = MemberID
        ///       SecondaryContentName = "Groups"
        ///       SecondaryContentSelectCriteria = "ccGroups.PublicJoin<>0"
        ///       RulesContentName = "Member Rules"
        ///       RulesPrimaryFieldName = "MemberID"
        ///       RulesSecondaryFieldName = "GroupID"
        /// </summary>
        /// <param name="htmlNamePrefix"></param>
        /// <param name="PrimaryContentName"></param>
        /// <param name="PrimaryRecordID"></param>
        /// <param name="SecondaryContentName"></param>
        /// <param name="RulesContentName"></param>
        /// <param name="RulesPrimaryFieldname"></param>
        /// <param name="RulesSecondaryFieldName"></param>
        /// <param name="SecondaryContentSelectCriteria"></param>
        /// <param name="CaptionFieldName"></param>
        /// <param name="readOnlyfield"></param>
        /// <param name="IncludeContentFolderDivs"></param>
        /// <param name="DefaultSecondaryIDList"></param>
        /// <returns></returns>
        public string getCheckList(string htmlNamePrefix, string PrimaryContentName, int PrimaryRecordID, string SecondaryContentName, string RulesContentName, string RulesPrimaryFieldname, string RulesSecondaryFieldName, string SecondaryContentSelectCriteria = "", string CaptionFieldName = "", bool readOnlyfield = false, bool IncludeContentFolderDivs = false, string DefaultSecondaryIDList = "") {
            string returnHtml = "";
            try {
                bool CanSeeHiddenFields = false;
                ContentMetadataModel SecondaryMetaData = null;
                List<int> ContentIDList = new List<int>();
                bool Found = false;
                int RecordID = 0;
                string SingularPrefixHtmlEncoded = null;
                //bool IsRuleCopySupported = false;
                bool AllowRuleCopy = false;
                ////
                //// IsRuleCopySupported - if true, the rule records include an allow button, and copy
                ////   This is for a checkbox like [ ] Other [enter other copy here]
                ////
                //IsRuleCopySupported = Models.Complex.metadataModel.isContentFieldSupported(core, RulesContentName, "RuleCopy");
                //if (IsRuleCopySupported) {
                //    IsRuleCopySupported = IsRuleCopySupported && Models.Complex.metadataModel.isContentFieldSupported(core, SecondaryContentName, "AllowRuleCopy");
                //    if (IsRuleCopySupported) {
                //        IsRuleCopySupported = IsRuleCopySupported && Models.Complex.metadataModel.isContentFieldSupported(core, SecondaryContentName, "RuleCopyCaption");
                //    }
                //}
                if (string.IsNullOrEmpty(CaptionFieldName)) {
                    CaptionFieldName = "name";
                }
                CaptionFieldName = GenericController.encodeEmpty(CaptionFieldName, "name");
                if (string.IsNullOrEmpty(PrimaryContentName) || string.IsNullOrEmpty(SecondaryContentName) || string.IsNullOrEmpty(RulesContentName) || string.IsNullOrEmpty(RulesPrimaryFieldname) || string.IsNullOrEmpty(RulesSecondaryFieldName)) {
                    returnHtml = "[Checklist not configured]";
                    LogController.handleError( core,new Exception("Creating checklist, all required fields were not supplied, Caption=[" + CaptionFieldName + "], PrimaryContentName=[" + PrimaryContentName + "], SecondaryContentName=[" + SecondaryContentName + "], RulesContentName=[" + RulesContentName + "], RulesPrimaryFieldName=[" + RulesPrimaryFieldname + "], RulesSecondaryFieldName=[" + RulesSecondaryFieldName + "]"));
                } else {
                    //
                    // ----- Gather all the SecondaryContent that associates to the PrimaryContent
                    //
                    int PrimaryContentID = ContentMetadataModel.getContentId(core, PrimaryContentName);
                    SecondaryMetaData = ContentMetadataModel.createByUniqueName(core, SecondaryContentName);
                    string SecondaryTablename = SecondaryMetaData.tableName;
                    int SecondaryContentID = SecondaryMetaData.id;
                    ContentIDList.Add(SecondaryContentID);
                    ContentIDList.AddRange(SecondaryMetaData.childIdList(core));
                    //
                    //
                    //
                    string rulesTablename = MetadataController.getContentTablename(core, RulesContentName);
                    SingularPrefixHtmlEncoded = HtmlController.encodeHtml(GenericController.getSingular_Sortof(SecondaryContentName)) + "&nbsp;";
                    //
                    int main_MemberShipCount = 0;
                    int main_MemberShipSize = 0;
                    returnHtml = "";
                    if ((!string.IsNullOrEmpty(SecondaryTablename)) && (!string.IsNullOrEmpty(rulesTablename))) {
                        string OldFolderVar = "OldFolder" + core.doc.checkListCnt;
                        string javaScriptRequired = "";
                        javaScriptRequired += "var " + OldFolderVar + ";";
                        string SQL = null;
                        int[] main_MemberShip = { };
                        string[] main_MemberShipRuleCopy = { };
                        if (PrimaryRecordID == 0) {
                            //
                            // New record, use the DefaultSecondaryIDList
                            //
                            if (!string.IsNullOrEmpty(DefaultSecondaryIDList)) {

                                string[] main_MemberShipText = DefaultSecondaryIDList.Split(',');
                                int Ptr = 0;
                                for (Ptr = 0; Ptr <= main_MemberShipText.GetUpperBound(0); Ptr++) {
                                    int main_MemberShipID = GenericController.encodeInteger(main_MemberShipText[Ptr]);
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
                            using (var csData = new CsModel(core)) {
                                SQL = "SELECT " + SecondaryTablename + ".ID AS ID,'' as RuleCopy";
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
                                csData.openSql(SQL);
                                if (csData.ok()) {
                                    {
                                        main_MemberShipSize = 10;
                                        main_MemberShip = new int[main_MemberShipSize + 1];
                                        main_MemberShipRuleCopy = new string[main_MemberShipSize + 1];
                                        while (csData.ok()) {
                                            if (main_MemberShipCount >= main_MemberShipSize) {
                                                main_MemberShipSize = main_MemberShipSize + 10;
                                                Array.Resize(ref main_MemberShip, main_MemberShipSize + 1);
                                                Array.Resize(ref main_MemberShipRuleCopy, main_MemberShipSize + 1);
                                            }
                                            main_MemberShip[main_MemberShipCount] = csData.getInteger("ID");
                                            main_MemberShipRuleCopy[main_MemberShipCount] = csData.getText("RuleCopy");
                                            main_MemberShipCount = main_MemberShipCount + 1;
                                            csData.goNext();
                                        }
                                    }
                                }
                            }
                        }
                        //
                        // ----- Gather all the Secondary Records, sorted by ContentName
                        //
                        SQL = "SELECT " + SecondaryTablename + ".ID AS ID, " + SecondaryTablename + "." + CaptionFieldName + " AS OptionCaption, " + SecondaryTablename + ".name AS OptionName, " + SecondaryTablename + ".SortOrder";
                        SQL += ",0 as AllowRuleCopy,'' as RuleCopyCaption";
                        //if (IsRuleCopySupported) {
                        //    SQL += "," + SecondaryTablename + ".AllowRuleCopy," + SecondaryTablename + ".RuleCopyCaption";
                        //} else {
                        //    SQL += ",0 as AllowRuleCopy,'' as RuleCopyCaption";
                        //}
                        SQL += " from " + SecondaryTablename + " where (1=1)";
                        if (!string.IsNullOrEmpty(SecondaryContentSelectCriteria)) {
                            SQL += "AND(" + SecondaryContentSelectCriteria + ")";
                        }
                        SQL += " GROUP BY " + SecondaryTablename + ".ID, " + SecondaryTablename + "." + CaptionFieldName + ", " + SecondaryTablename + ".name, " + SecondaryTablename + ".SortOrder";
                        //if (IsRuleCopySupported) {
                        //    SQL += ", " + SecondaryTablename + ".AllowRuleCopy," + SecondaryTablename + ".RuleCopyCaption";
                        //}
                        SQL += " ORDER BY ";
                        SQL += SecondaryTablename + "." + CaptionFieldName;
                        using (var csData = new CsModel(core)) {
                            if (!csData.openSql(SQL)) {
                                returnHtml = "(No choices are available.)";
                            } else {
                                {
                                    string EndDiv = "";
                                    int CheckBoxCnt = 0;
                                    int DivCheckBoxCnt = 0;
                                    CanSeeHiddenFields = core.session.isAuthenticatedDeveloper(core);
                                    string DivName = htmlNamePrefix + ".All";
                                    while (csData.ok()) {
                                        string OptionName = csData.getText("OptionName");
                                        if ((OptionName.Left(1) != "_") || CanSeeHiddenFields) {
                                            //
                                            // Current checkbox is visible
                                            //
                                            RecordID = csData.getInteger("ID");
                                            AllowRuleCopy = csData.getBoolean("AllowRuleCopy");
                                            string RuleCopyCaption = csData.getText("RuleCopyCaption");
                                            string OptionCaption = csData.getText("OptionCaption");
                                            if (string.IsNullOrEmpty(OptionCaption)) {
                                                OptionCaption = OptionName;
                                            }
                                            string optionCaptionHtmlEncoded = null;
                                            if (string.IsNullOrEmpty(OptionCaption)) {
                                                optionCaptionHtmlEncoded = SingularPrefixHtmlEncoded + RecordID;
                                            } else {
                                                optionCaptionHtmlEncoded = HtmlController.encodeHtml(OptionCaption);
                                            }
                                            if (DivCheckBoxCnt != 0) {
                                                // leave this between checkboxes - it is searched in the admin page
                                                //returnHtml += "<br>\r\n";
                                            }
                                            string RuleCopy = "";
                                            Found = false;
                                            if (main_MemberShipCount != 0) {
                                                int main_MemberShipPointer = 0;
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
                                            //returnHtml += "\r\n";
                                            //returnHtml += "<table><tr><td style=\"vertical-align:top;margin-top:0;width:20px;\">";
                                            returnHtml += "<input type=hidden name=\"" + htmlNamePrefix + "." + CheckBoxCnt + ".id\" value=" + RecordID + ">";
                                            if (readOnlyfield && !Found) {
                                                returnHtml += "<div class=\"checkbox\"><label><input type=checkbox disabled>&nbsp;" + optionCaptionHtmlEncoded + "</label></div>";
                                                //returnHtml += "<input type=checkbox disabled>";
                                            } else if (readOnlyfield) {
                                                returnHtml += "<div class=\"checkbox\"><label><input type=checkbox disabled checked>&nbsp;" + optionCaptionHtmlEncoded + "</label></div>";
                                                //returnHtml += "<input type=checkbox disabled checked>";
                                                returnHtml += "<input type=\"hidden\" name=\"" + htmlNamePrefix + "." + CheckBoxCnt + ".ID\" value=" + RecordID + ">";
                                            } else if (Found) {
                                                returnHtml += "<div class=\"checkbox\"><label><input type=checkbox name=\"" + htmlNamePrefix + "." + CheckBoxCnt + "\" value=\"1\" checked>&nbsp;" + optionCaptionHtmlEncoded + "</label></div>";
                                                //returnHtml += "<input type=checkbox name=\"" + htmlNamePrefix + "." + CheckBoxCnt + "\" checked>";
                                            } else {
                                                returnHtml += "<div class=\"checkbox\"><label><input type=\"checkbox\" name=\"" + htmlNamePrefix + "." + CheckBoxCnt + "\" value=\"1\">&nbsp;" + optionCaptionHtmlEncoded + "</label></div>";
                                                //returnHtml += "<input type=checkbox name=\"" + htmlNamePrefix + "." + CheckBoxCnt + "\">";
                                            }
                                            //returnHtml += "</td><td style=\"vertical-align:top;padding-top:4px;\">";
                                            //returnHtml += SpanClassAdminNormal + optionCaptionHtmlEncoded;
                                            //if (AllowRuleCopy) {
                                            //    returnHtml += ", " + RuleCopyCaption + "&nbsp;" + inputText(core, htmlNamePrefix + "." + CheckBoxCnt + ".RuleCopy", RuleCopy, 1, 20);
                                            //}
                                            //returnHtml += "</td></tr></table>";
                                            CheckBoxCnt = CheckBoxCnt + 1;
                                            DivCheckBoxCnt = DivCheckBoxCnt + 1;
                                        }
                                        csData.goNext();
                                    }
                                    returnHtml += EndDiv;
                                    returnHtml += inputHidden(htmlNamePrefix + ".RowCount", CheckBoxCnt);
                                }
                            }
                        }
                        addScriptCode(javaScriptRequired, "CheckList Categories");
                    }
                    //End If
                    core.doc.checkListCnt = core.doc.checkListCnt + 1;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnHtml;
        }
        //
        //====================================================================================================
        //
        public string getPanel(string content, string stylePanel, string styleHilite, string styleShadow, string width, int padding, int heightMin) {
            string ContentPanelWidth = "";
            string contentPanelWidthStyle = "";
            if (width.IsNumeric()) {
                ContentPanelWidth = (int.Parse(width) - 2).ToString();
                contentPanelWidthStyle = ContentPanelWidth + "px";
            } else {
                ContentPanelWidth = "100%";
                contentPanelWidthStyle = ContentPanelWidth;
            }
            //
            string s0 = ""
                + "<td style=\"padding:" + padding + "px;vertical-align:top\" class=\"" + stylePanel + "\">"
                + content
                + "</td>"
                + "";
            //
            string s1 = ""
                + "<tr>"
                + s0
                + "</tr>"
                + "";
            string s2 = ""
                + "<table style=\"width:" + contentPanelWidthStyle + ";border:0px;\" class=\"" + stylePanel + "\" cellspacing=\"0\">"
                + s1
                + "</table>"
                + "";
            string s3 = ""
                + "<td colspan=\"3\" width=\"" + ContentPanelWidth + "\" valign=\"top\" align=\"left\" class=\"" + stylePanel + "\">"
                + s2
                + "</td>"
                + "";
            string s4 = ""
                + "<tr>"
                + s3
                + "</tr>"
                + "";
            string result = ""
                + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"" + width + "\" class=\"" + stylePanel + "\">"
                + s4
                + "</table>"
                + "";
            return result;
        }
        //
        public string getPanel(string content) => getPanel(content, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5, 1);
        //
        public string getPanel(string content, string stylePanel, string styleHilite, string styleShadow, string width, int padding) => getPanel(content, stylePanel, styleHilite, styleShadow, width, padding, 1);
        //
        //====================================================================================================
        //
        public string getPanelHeader(string HeaderMessage, string RightSideMessage = "") {
            string iRightSideMessage = GenericController.encodeEmpty(RightSideMessage, core.doc.profileStartTime.ToString("G"));
            return AdminUIController.getHeader(core, HeaderMessage, iRightSideMessage);
        }
        //
        //====================================================================================================
        //
        public string getPanelTop(string StylePanel = "", string StyleHilite = "", string StyleShadow = "", string Width = "", string Padding = "", string HeightMin = "") {
            string MyWidth = encodeEmpty(Width, "100%");
            string MyPadding = encodeEmpty(Padding, "5");
            string ContentPanelWidth = (MyWidth.IsNumeric()) ? (int.Parse(MyWidth) - 2).ToString() :  "100%";
            return ""
                + "\r<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"" + MyWidth + "\">"
                + cr2 + "<tr>"
                + cr3 + "<td colspan=\"3\" width=\"" + ContentPanelWidth + "\" valign=\"top\" align=\"left\">"
                + cr4 + "<table border=\"0\" cellpadding=\"" + MyPadding + "\" cellspacing=\"0\" width=\"" + ContentPanelWidth + "\">"
                + cr5 + "<tr>"
                + cr6 + "<td valign=\"top\">";
        }
        //
        //====================================================================================================
        //
        public string getPanelBottom() {
            return ""
                + cr6 + "</td>"
                + cr5 + "</tr>"
                + cr4 + "</table>"
                + cr3 + "</td>"
                + cr2 + "</tr>"
                + "\r</table>";
        }
        //
        //====================================================================================================
        //
        public string getPanelButtons(string ButtonValueList, string ButtonName, string PanelWidth = "", string PanelHeightMin = "") {
            return AdminUIController.getButtonBar(core, AdminUIController.getButtonsFromList(core, ButtonValueList, true, true, ButtonName), "");
        }
        //
        //====================================================================================================
        //
        public string getPanelInput(string PanelContent, string PanelWidth = "", string PanelHeightMin = "1") {
            return getPanel(PanelContent, "ccPanelInput", "ccPanelShadow", "ccPanelHilite", PanelWidth, 2, GenericController.encodeInteger(PanelHeightMin));
        }
        //
        //====================================================================================================
        /// <summary>
        /// standard tool panel at the bottom of every page
        /// </summary>
        /// <returns></returns>
        public string getToolsPanel() {
            string result = "";
            try {
                if (core.session.user.allowToolsPanel) {
                    bool ShowLegacyToolsPanel = core.siteProperties.getBoolean("AllowLegacyToolsPanel", true);
                    //
                    // --- Link Panel - used for both Legacy Tools Panel, and without it
                    StringBuilderLegacyController LinkPanel = new StringBuilderLegacyController();
                    LinkPanel.Add(SpanClassAdminSmall);
                    LinkPanel.Add("Contensive " + core.codeVersion() + " | ");
                    LinkPanel.Add(core.doc.profileStartTime.ToString("G") + " | ");
                    LinkPanel.Add("<a class=\"ccAdminLink\" target=\"_blank\" href=\"http://support.Contensive.com/\">Support</A> | ");
                    LinkPanel.Add("<a class=\"ccAdminLink\" href=\"" + HtmlController.encodeHtml("/" + core.appConfig.adminRoute) + "\">Admin Home</A> | ");
                    LinkPanel.Add("<a class=\"ccAdminLink\" href=\"" + HtmlController.encodeHtml("http://" + core.webServer.requestDomain) + "\">Public Home</A> | ");
                    LinkPanel.Add("<a class=\"ccAdminLink\" target=\"_blank\" href=\"" + HtmlController.encodeHtml("/" + core.appConfig.adminRoute + "?" + RequestNameHardCodedPage + "=" + HardCodedPageMyProfile) + "\">My Profile</A> | ");
                    if (core.siteProperties.getBoolean("AllowMobileTemplates", false)) {
                        string QS;
                        if (core.session.visit.mobile) {
                            QS = core.doc.refreshQueryString;
                            QS = GenericController.modifyQueryString(QS, "method", "forcenonmobile");
                            LinkPanel.Add("<a class=\"ccAdminLink\" href=\"?" + QS + "\">Non-Mobile Version</A> | ");
                        } else {
                            QS = core.doc.refreshQueryString;
                            QS = GenericController.modifyQueryString(QS, "method", "forcemobile");
                            LinkPanel.Add("<a class=\"ccAdminLink\" href=\"?" + QS + "\">Mobile Version</A> | ");
                        }
                    }
                    LinkPanel.Add("</span>");
                    //
                    if (ShowLegacyToolsPanel) {
                        StringBuilderLegacyController ToolsPanel = new StringBuilderLegacyController();
                        string WorkingQueryString = GenericController.modifyQueryString(core.doc.refreshQueryString, "ma", "", false);
                        //
                        // ----- Tools Panel Caption
                        string helpLink = "";
                        result += getPanelHeader("Contensive Tools Panel" + helpLink);
                        ToolsPanel.Add(HtmlController.inputHidden("Type", FormTypeToolsPanel));
                        string TagID = null;
                        string OptionsPanel = "";
                        //
                        if (true) {
                            string EditTagID = "AllowEditing";
                            string QuickEditTagID = "AllowQuickEditor";
                            string AdvancedEditTagID = "AllowAdvancedEditor";
                            //
                            // Edit
                            helpLink = "";
                            bool iValueBoolean = core.visitProperty.getBoolean("AllowEditing");
                            string Tag = HtmlController.checkbox(EditTagID, iValueBoolean, EditTagID);
                            Tag = GenericController.vbReplace(Tag, ">", " onClick=\"document.getElementById('" + QuickEditTagID + "').checked=false;document.getElementById('" + AdvancedEditTagID + "').checked=false;\">");
                            OptionsPanel = OptionsPanel + "\r<div class=\"ccAdminSmall\">"
                            + cr2 + "<LABEL for=\"" + EditTagID + "\">" + Tag + "&nbsp;Edit</LABEL>" + helpLink + "\r</div>";
                            //
                            // Quick Edit
                            helpLink = "";
                            iValueBoolean = core.visitProperty.getBoolean("AllowQuickEditor");
                            Tag = HtmlController.checkbox(QuickEditTagID, iValueBoolean, QuickEditTagID);
                            Tag = GenericController.vbReplace(Tag, ">", " onClick=\"document.getElementById('" + EditTagID + "').checked=false;document.getElementById('" + AdvancedEditTagID + "').checked=false;\">");
                            OptionsPanel = OptionsPanel + "\r<div class=\"ccAdminSmall\">"
                                + cr2 + "<LABEL for=\"" + QuickEditTagID + "\">" + Tag + "&nbsp;Quick Edit</LABEL>" + helpLink + "\r</div>";
                            //
                            // Advanced Edit
                            helpLink = "";
                            iValueBoolean = core.visitProperty.getBoolean("AllowAdvancedEditor");
                            Tag = HtmlController.checkbox(AdvancedEditTagID, iValueBoolean, AdvancedEditTagID);
                            Tag = GenericController.vbReplace(Tag, ">", " onClick=\"document.getElementById('" + QuickEditTagID + "').checked=false;document.getElementById('" + EditTagID + "').checked=false;\">");
                            OptionsPanel = OptionsPanel + "\r<div class=\"ccAdminSmall\">"
                                + cr2 + "<LABEL for=\"" + AdvancedEditTagID + "\">" + Tag + "&nbsp;Advanced Edit</LABEL>" + helpLink + "\r</div>";
                            //
                            // Workflow Authoring Render Mode
                            //
                            helpLink = "";
                            helpLink = "";
                            iValueBoolean = core.visitProperty.getBoolean("AllowDebugging");
                            TagID = "AllowDebugging";
                            Tag = HtmlController.checkbox(TagID, iValueBoolean, TagID);
                            OptionsPanel = OptionsPanel + "\r<div class=\"ccAdminSmall\">"
                                + cr2 + "<LABEL for=\"" + TagID + "\">" + Tag + "&nbsp;Debug</LABEL>" + helpLink + "\r</div>";
                            OptionsPanel = OptionsPanel + ""
                                + "\r<div class=\"border bg-white p-2\">"
                                + cr2 + inputSubmit(ButtonApply, "mb", "", "", false, "btn btn-primary mr-1 btn-sm")
                                + "\r</div>"
                                + "";
                        }
                        //
                        // ----- Create the Login Panel
                        string Copy;
                        if (string.IsNullOrEmpty(core.session.user.name.Trim(' '))) {
                            Copy = "You are logged in as member #" + core.session.user.id + ".";
                        } else {
                            Copy = "You are logged in as " + core.session.user.name + ".";
                        }
                        string LoginPanel = ""
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
                            + cr2 + "<LABEL for=\"" + TagID + "\">" + HtmlController.inputText(core, TagID, "", 1, 30, TagID, false) + "&nbsp;" + Caption + "</LABEL>"
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
                            + cr2 + "<LABEL for=\"" + TagID + "\">" + HtmlController.inputText(core, TagID, "", 1, 30, TagID, true) + "&nbsp;" + Caption + "</LABEL>"
                            + "\r</div>";
                        //
                        // Autologin checkbox
                        //
                        if (core.siteProperties.getBoolean("AllowAutoLogin", false)) {
                            if (core.session.visit.cookieSupport) {
                                TagID = "autologin";
                                LoginPanel = LoginPanel + ""
                                    + "\r<div class=\"ccAdminSmall\">"
                                    + cr2 + "<LABEL for=\"" + TagID + "\">" + HtmlController.checkbox(TagID, true, TagID) + "&nbsp;Login automatically from this computer</LABEL>"
                                    + "\r</div>";
                            }
                        }
                        //
                        // Buttons
                        //
                        LoginPanel = LoginPanel + AdminUIController.getButtonBar(core, AdminUIController.getButtonsFromList(core, ButtonLogin + "," + ButtonLogout, true, true, "mb"), "");
                        //
                        // ----- assemble tools panel
                        //
                        Copy = ""
                            + "\r<td width=\"50%\" class=\"ccPanelInput\" style=\"vertical-align:bottom;\">"
                            + GenericController.nop(LoginPanel) + "\r</td>"
                            + "\r<td width=\"50%\" class=\"ccPanelInput\" style=\"vertical-align:bottom;\">"
                            + GenericController.nop(OptionsPanel) + "\r</td>";
                        Copy = ""
                            + "\r<tr>"
                            + GenericController.nop(Copy) + "\r</tr>"
                            + "";
                        Copy = ""
                            + "\r<table border=\"0\" cellpadding=\"3\" cellspacing=\"0\" width=\"100%\">"
                            + GenericController.nop(Copy) + "\r</table>";
                        ToolsPanel.Add(getPanelInput(Copy));
                        result += getPanel(HtmlController.form(core, ToolsPanel.Text), "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5);
                        result += getPanel(LinkPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5);
                        LinkPanel = null;
                        ToolsPanel = null;
                    }
                    //
                    // --- Developer Debug Panel
                    if (core.visitProperty.getBoolean("AllowDebugging")) {
                        //
                        // --- Debug Panel Header
                        LinkPanel = new StringBuilderLegacyController();
                        LinkPanel.Add(SpanClassAdminSmall);
                        LinkPanel.Add("Contensive " + core.codeVersion() + " | ");
                        LinkPanel.Add(core.doc.profileStartTime.ToString("G") + " | ");
                        LinkPanel.Add("<a class=\"ccAdminLink\" target=\"_blank\" href=\"http://support.Contensive.com/\">Support</A> | ");
                        LinkPanel.Add("<a class=\"ccAdminLink\" href=\"" + HtmlController.encodeHtml("/" + core.appConfig.adminRoute) + "\">Admin Home</A> | ");
                        LinkPanel.Add("<a class=\"ccAdminLink\" href=\"" + HtmlController.encodeHtml("http://" + core.webServer.requestDomain) + "\">Public Home</A> | ");
                        LinkPanel.Add("Render " + (Convert.ToSingle(core.doc.appStopWatch.ElapsedMilliseconds) / 1000).ToString("0.000") + " sec | ");
                        LinkPanel.Add("</span>");
                        string DebugPanel = "";
                        //
                        DebugPanel += "\r<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">"
                            + cr2 + "<tr>"
                            + cr3 + "<td width=\"100\" class=\"ccPanel\"><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=\"100\" height=\"1\" ></td>"
                            + cr3 + "<td width=\"100%\" class=\"ccPanel\"><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=\"1\" height=\"1\" ></td>"
                            + cr2 + "</tr>";
                        DebugPanel += "</table>";
                        //
                        if (ShowLegacyToolsPanel) {
                            //
                            // Debug Panel as part of legacy tools panel
                            result += getPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5);
                        } else {
                            //
                            // Debug Panel without Legacy Tools panel
                            result += getPanelHeader("Debug Panel") + getPanel(LinkPanel.Text) + getPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5);
                        }
                    }
                    result = "\r<div class=\"ccCon\">" + GenericController.nop(result) + "\r</div>";
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
        }
        //
        //=================================================================================================================
        /// <summary>
        /// return value from name/value special form
        /// </summary>
        /// <param name="OptionName"></param>
        /// <param name="addonOptionString"></param>
        /// <returns></returns>
        public static string getAddonOptionStringValue(string OptionName, string addonOptionString) {
            string result = GenericController.getSimpleNameValue(OptionName, addonOptionString, "", "&");
            int Pos = GenericController.vbInstr(1, result, "[");
            if (Pos > 0) {
                result = result.Left(Pos - 1);
            }
            return encodeText(GenericController.decodeNvaArgument(result)).Trim(' ');
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
                LogController.handleError( core,ex);
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
                            headList.Add("\r\n<!-- added by " + HtmlController.encodeHtml(asset.addedByMessage) + " -->");
                        }
                        content += " | " + asset.content;
                    }
                    headList.Add("\r\n<title>" + HtmlController.encodeHtml(content.Substring(3)) + "</title>");
                }
                if (core.doc.htmlMetaContent_KeyWordList.Count > 0) {
                    string content = "";
                    foreach (var asset in core.doc.htmlMetaContent_KeyWordList.FindAll((a) => (!string.IsNullOrEmpty(a.content)))) {
                        if ((core.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage))) {
                            headList.Add("\r\n<!-- added by " + HtmlController.encodeHtml(asset.addedByMessage) + " -->");
                        }
                        content += "," + asset.content;
                    }
                    if (!string.IsNullOrEmpty(content)) {
                        headList.Add("\r\n<meta name=\"keywords\" content=\"" + HtmlController.encodeHtml(content.Substring(1)) + "\" >");
                    }
                }
                if (core.doc.htmlMetaContent_Description.Count > 0) {
                    string content = "";
                    foreach (var asset in core.doc.htmlMetaContent_Description) {
                        if ((core.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage))) {
                            headList.Add("\r\n<!-- added by " + HtmlController.encodeHtml(asset.addedByMessage) + " -->");
                        }
                        content += "," + asset.content;
                    }
                    headList.Add("\r\n<meta name=\"description\" content=\"" + HtmlController.encodeHtml(content.Substring(1)) + "\" >");
                }
                //
                // -- favicon
                string VirtualFilename = core.siteProperties.getText("faviconfilename");
                switch (Path.GetExtension(VirtualFilename).ToLowerInvariant()) {
                    case ".ico":
                        headList.Add("\r\n<link rel=\"icon\" type=\"image/vnd.microsoft.icon\" href=\"" + GenericController.getCdnFileLink(core, VirtualFilename) + "\" >");
                        break;
                    case ".png":
                        headList.Add("\r\n<link rel=\"icon\" type=\"image/png\" href=\"" + GenericController.getCdnFileLink(core, VirtualFilename) + "\" >");
                        break;
                    case ".gif":
                        headList.Add("\r\n<link rel=\"icon\" type=\"image/gif\" href=\"" + GenericController.getCdnFileLink(core, VirtualFilename) + "\" >");
                        break;
                    case ".jpg":
                        headList.Add("\r\n<link rel=\"icon\" type=\"image/jpg\" href=\"" + GenericController.getCdnFileLink(core, VirtualFilename) + "\" >");
                        break;
                }
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
                    foreach (var asset in core.doc.htmlAssetList.FindAll((HtmlAssetClass item) => (item.inHead))) {
                        string debugComment = "";
                        if ((core.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage))) {
                            debugComment = "\r\n<!-- added by " + HtmlController.encodeHtml(asset.addedByMessage) + " -->";
                        }
                        if (asset.assetType.Equals(HtmlAssetTypeEnum.style)) {
                            styleList.Add(debugComment);
                            if (asset.isLink) {
                                styleList.Add("\r\n<link rel=\"stylesheet\" type=\"text/css\" href=\"" + asset.content + "\" >");
                            } else {
                                styleList.Add("\r\n<style>" + asset.content + "</style>");
                            }
                        } else if (asset.assetType.Equals(HtmlAssetTypeEnum.script)) {
                            headScriptList.Add(debugComment);
                            if (asset.isLink) {
                                headScriptList.Add("\r\n<script type=\"text/javascript\" src=\"" + asset.content + "\"></script>");
                            } else {
                                headScriptList.Add("\r\n<script type=\"text/javascript\">" + asset.content + "</script>");
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
                        headList.Add("\r\n<!-- added by " + HtmlController.encodeHtml(asset.addedByMessage) + " -->");
                    }
                    headList.Add("\r\n" + asset.content);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return string.Join("\r", headList);
        }
        //
        //====================================================================================================
        //
        public void addScriptCode_onLoad(string code, string addedByMessage) {
            try {
                if (!string.IsNullOrEmpty(code)) {
                    core.doc.htmlAssetList.Add(new HtmlAssetClass() {
                        assetType = HtmlAssetTypeEnum.scriptOnLoad,
                        addedByMessage = addedByMessage,
                        isLink = false,
                        content = code
                    });
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
        }
        //
        //====================================================================================================
        //
        public void addScriptCode(string code, string addedByMessage, bool forceHead = false, int sourceAddonId = 0) {
            try {
                if (!string.IsNullOrWhiteSpace(code)) {
                    HtmlAssetClass asset = null;
                    if (sourceAddonId != 0) {
                        asset = core.doc.htmlAssetList.Find(t => ((t.sourceAddonId == sourceAddonId) && (!t.isLink)));
                    }
                    if (asset != null) {
                        //
                        // already in list, just mark it forceHead
                        asset.inHead = asset.inHead || forceHead;
                    } else {
                        //
                        // add to list
                        core.doc.htmlAssetList.Add(new HtmlAssetClass() {
                            assetType = HtmlAssetTypeEnum.script,
                            inHead = forceHead,
                            addedByMessage = addedByMessage,
                            isLink = false,
                            content = GenericController.removeScriptTag(code),
                            sourceAddonId = sourceAddonId
                        });
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
        }
        //
        //=========================================================================================================
        //
        public void addScriptLinkSrc( string scriptLinkSrc, string addedByMessage, bool forceHead = false, int sourceAddonId = 0) {
            try {
                if (!string.IsNullOrWhiteSpace(scriptLinkSrc)) {
                    HtmlAssetClass asset = null;
                    if (sourceAddonId != 0) {
                        asset = core.doc.htmlAssetList.Find(t => ((t.content == scriptLinkSrc) && (t.isLink)));
                    }
                    if (asset != null) {
                        //
                        // already in list, just mark it forceHead
                        asset.inHead = asset.inHead || forceHead;
                    } else {
                        //
                        // add to list
                        core.doc.htmlAssetList.Add(new HtmlAssetClass {
                            assetType = HtmlAssetTypeEnum.script,
                            addedByMessage = addedByMessage,
                            isLink = true,
                            inHead = forceHead,
                            content = scriptLinkSrc,
                            sourceAddonId = sourceAddonId
                        });
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
        }
        //
        public void addScriptLinkSrc(string scriptLinkSrc, string addedByMessage) => addScriptLinkSrc(scriptLinkSrc, addedByMessage, false, 0);
        //
        public void addScriptLinkSrc(string scriptLinkSrc, string addedByMessage, bool forceHead) => addScriptLinkSrc(scriptLinkSrc, addedByMessage, forceHead, 0);
       //
        //=========================================================================================================
        //
        public void addTitle(string pageTitle, string addedByMessage) {
            try {
                if (!string.IsNullOrEmpty(pageTitle.Trim())) {
                    core.doc.htmlMetaContent_TitleList.Add(new HtmlMetaClass() {
                        addedByMessage = addedByMessage,
                        content = pageTitle
                    });
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
        }
        //
        public void addTitle(string pageTitle) => addTitle(pageTitle, "");
        //
        //=========================================================================================================
        //
        public void addMetaDescription(string MetaDescription, string addedByMessage) {
            try {
                if (!string.IsNullOrEmpty(MetaDescription.Trim())) {
                    core.doc.htmlMetaContent_Description.Add(new HtmlMetaClass() {
                        addedByMessage = addedByMessage,
                        content = MetaDescription
                    });
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
        }
        //
        public void addMetaDescription(string MetaDescription) => addMetaDescription(MetaDescription, "");
        //
        //=========================================================================================================
        //
        public void addStyleLink(string StyleSheetLink, string addedByMessage) {
            try {
                if (!string.IsNullOrEmpty(StyleSheetLink.Trim())) {
                    core.doc.htmlAssetList.Add(new HtmlAssetClass() {
                        addedByMessage = addedByMessage,
                        assetType = HtmlAssetTypeEnum.style,
                        inHead = true,
                        isLink = true,
                        content = StyleSheetLink
                    });
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
        }
        //
        //=========================================================================================================
        //
        public void addMetaKeywordList(string metaKeywordList, string addedByMessage) {
            try {
                foreach (string keyword in metaKeywordList.Split(',')) {
                    if (!string.IsNullOrEmpty(keyword)) {
                        core.doc.htmlMetaContent_KeyWordList.Add(new HtmlMetaClass() {
                            addedByMessage = addedByMessage,
                            content = keyword
                        });
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
        }
        //
        public void addMetaKeywordList(string MetaKeywordList) => addMetaKeywordList(MetaKeywordList, "");
        //
        //=========================================================================================================
        //
        public void addHeadTag(string headTag, string addedByMessage) {
            try {
                if (!string.IsNullOrWhiteSpace(headTag)) {
                    core.doc.htmlMetaContent_OtherTags.Add(new HtmlMetaClass() {
                        addedByMessage = addedByMessage,
                        content = headTag
                    });
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
        }
        //
        public void addHeadTag(string headTag) => addHeadTag(headTag, "");
        //
        //============================================================================
        //
        public string getContentCopy(string CopyName, string DefaultContent, int personalizationPeopleId, bool AllowEditWrapper, bool personalizationIsAuthenticated) {
            string returnCopy = "";
            try {
                int RecordID = 0;
                int contactPeopleId = 0;
                //
                // honestly, not sure what to do with 'return_ErrorMessage'
                using (var csData = new CsModel(core)) {
                    if (!csData.open("copy content", "Name=" + DbController.encodeSQLText(CopyName), "ID", true, 0, "Name,ID,Copy,modifiedBy")) {
                        csData.close();
                        csData.insert("copy content");
                        if (csData.ok()) {
                            RecordID = csData.getInteger("ID");
                            csData.set("name", CopyName);
                            csData.set("copy", GenericController.encodeText(DefaultContent));
                            csData.save();
                            //   Call WorkflowController.publishEdit("copy content", RecordID)
                        }
                    }
                    if (csData.ok()) {
                        RecordID = csData.getInteger("ID");
                        contactPeopleId = csData.getInteger("modifiedBy");
                        returnCopy = csData.getText("Copy");
                        //returnCopy = contentCmdController.executeContentCommands(core, returnCopy, CPUtilsBaseClass.addonContext.ContextPage, personalizationPeopleId, personalizationIsAuthenticated, ref Return_ErrorMessage);
                        returnCopy = ActiveContentController.renderHtmlForWeb(core, returnCopy, "copy content", RecordID, personalizationPeopleId, "", 0, CPUtilsBaseClass.addonContext.ContextPage);
                        //
                        if (true) {
                            if (core.session.isEditingAnything()) {
                                returnCopy = csData.getRecordEditLink(false) + returnCopy;
                                if (AllowEditWrapper) {
                                    returnCopy = AdminUIController.getEditWrapper(core, "copy content", returnCopy);
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnCopy;
        }
        //
        // ====================================================================================================
        //
        public void setContentCopy(string copyName, string content) {
            using (var csData = new CsModel(core)) {
                csData.open("Copy Content", "name=" + DbController.encodeSQLText(copyName));
                if (!csData.ok()) {
                    csData.insert("Copy Content");
                }
                csData.set("name", copyName);
                csData.set("Copy", content);
            }
        }
        //
        //====================================================================================================
        //
        public string getResourceLibrary(string RootFolderName, bool AllowSelectResource, string SelectResourceEditorName, string SelectLinkObjectName, bool AllowGroupAdd) {
            string addonGuidResourceLibrary = "{564EF3F5-9673-4212-A692-0942DD51FF1A}";
            Dictionary<string, string> arguments = new Dictionary<string, string> {
                { "RootFolderName", RootFolderName },
                { "AllowSelectResource", AllowSelectResource.ToString() },
                { "SelectResourceEditorName", SelectResourceEditorName },
                { "SelectLinkObjectName", SelectLinkObjectName },
                { "AllowGroupAdd", AllowGroupAdd.ToString() }
            };
            return core.addon.execute(AddonModel.create(core, addonGuidResourceLibrary), new CPUtilsBaseClass.addonExecuteContext() {
                addonType = CPUtilsBaseClass.addonContext.ContextAdmin,
                argumentKeyValuePairs = arguments,
                errorContextMessage = "calling resource library addon [" + addonGuidResourceLibrary + "] from internal method"
            });
       }
        //
        //====================================================================================================
        /// <summary>
        /// process the input of a checklist, making changes to rule records
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="primaryContentName"></param>
        /// <param name="primaryRecordID"></param>
        /// <param name="secondaryContentName"></param>
        /// <param name="rulesContentName"></param>
        /// <param name="rulesPrimaryFieldname"></param>
        /// <param name="rulesSecondaryFieldName"></param>
        public void processCheckList(string tagName, string primaryContentName, string primaryRecordID, string secondaryContentName, string rulesContentName, string rulesPrimaryFieldname, string rulesSecondaryFieldName) {
            int GroupCnt = core.docProperties.getInteger(tagName + ".RowCount");
            bool RuleContentChanged = false;
            if (GroupCnt > 0) {
                //
                // Test if RuleCopy is supported
                var ruleContentMetadata = ContentMetadataModel.createByUniqueName(core, rulesContentName);
                if ( ruleContentMetadata==null) {
                    LogController.logWarn(core, "processCheckList called and ruleContentName not found [" + rulesContentName + "]");
                    return;
                }
                var secondaryContentMetadata = ContentMetadataModel.createByUniqueName(core, secondaryContentName);
                if (secondaryContentMetadata == null) {
                    LogController.logWarn(core, "processCheckList called and secondaryContentName not found [" + secondaryContentName + "]");
                    return;
                }
                bool SupportRuleCopy = ruleContentMetadata.containsField(core, "RuleCopy");
                if (SupportRuleCopy) {
                    SupportRuleCopy &= secondaryContentMetadata.containsField(core, "AllowRuleCopy");
                    if (SupportRuleCopy) {
                        SupportRuleCopy &= secondaryContentMetadata.containsField(core, "RuleCopyCaption");
                    }
                }
                //
                // Go through each checkbox and check for a rule
                string dupRuleIdList = "";
                string rulesTablename = MetadataController.getContentTablename(core, rulesContentName);
                string SQL = "select " + rulesSecondaryFieldName + ",id from " + rulesTablename + " where (" + rulesPrimaryFieldname + "=" + primaryRecordID + ")and(active<>0) order by " + rulesSecondaryFieldName;
                DataTable currentRules = core.db.executeQuery(SQL);
                int currentRulesCnt = currentRules.Rows.Count; 
                for (int GroupPtr = 0; GroupPtr < GroupCnt; GroupPtr++) {
                    //
                    // ----- Read Response
                    int SecondaryRecordID = core.docProperties.getInteger(tagName + "." + GroupPtr + ".ID");
                    string RuleCopy = core.docProperties.getText(tagName + "." + GroupPtr + ".RuleCopy");
                    bool RuleNeeded = core.docProperties.getBoolean(tagName + "." + GroupPtr);
                    //
                    // ----- Update Record
                    bool RuleFound = false;
                    int RuleId = 0;
                    int TestRecordIDLast = 0;
                    for (int Ptr = 0; Ptr < currentRulesCnt; Ptr++) {
                        int TestRecordID = GenericController.encodeInteger(currentRules.Rows[Ptr][0]);
                        if (TestRecordID == 0) {
                            //
                            // skip
                        } else if (TestRecordID == SecondaryRecordID) {
                            //
                            // hit
                            RuleFound = true;
                            RuleId = GenericController.encodeInteger(currentRules.Rows[Ptr][1]);
                            break;
                        } else if (TestRecordID == TestRecordIDLast) {
                            //
                            // dup
                            dupRuleIdList = dupRuleIdList + "," + GenericController.encodeInteger(currentRules.Rows[Ptr][1]);
                            currentRules.Rows[Ptr][0] = 0;
                        }
                        TestRecordIDLast = TestRecordID;
                    }
                    if (SupportRuleCopy && RuleNeeded && (RuleFound)) {
                        //
                        // Record exists and is needed, update the rule copy
                        SQL = "update " + rulesTablename + " set rulecopy=" + DbController.encodeSQLText(RuleCopy) + " where id=" + RuleId;
                        core.db.executeQuery(SQL);
                    } else if (RuleNeeded && (!RuleFound)) {
                        //
                        // No record exists, and one is needed                        
                        using (var csData = new CsModel(core)) {
                            csData.insert(rulesContentName);
                            if (csData.ok()) {
                                csData.set("Active", RuleNeeded);
                                csData.set(rulesPrimaryFieldname, primaryRecordID);
                                csData.set(rulesSecondaryFieldName, SecondaryRecordID);
                                if (SupportRuleCopy) {
                                    csData.set("RuleCopy", RuleCopy);
                                }
                            }
                        }
                        RuleContentChanged = true;
                    } else if ((!RuleNeeded) && RuleFound) {
                        //
                        // Record exists and it is not needed
                        SQL = "delete from " + rulesTablename + " where id=" + RuleId;
                        core.db.executeQuery(SQL);
                        RuleContentChanged = true;
                    }
                }
                //
                // delete dups
                if (!string.IsNullOrEmpty(dupRuleIdList)) {
                    SQL = "delete from " + rulesTablename + " where id in (" + dupRuleIdList.Substring(1) + ")";
                    core.db.executeQuery(SQL);
                    RuleContentChanged = true;
                }
            }
            if (RuleContentChanged) {
                string tablename = MetadataController.getContentTablename(core, rulesContentName);
                core.cache.invalidateAllKeysInTable(tablename);
            }
        }
        //
        //====================================================================================================
        //
        public static string genericBlockTag(string TagName, string InnerHtml) => genericBlockTag(TagName, InnerHtml, "", "", "");
        //
        public static string genericBlockTag(string TagName, string InnerHtml, string HtmlClass) => genericBlockTag(TagName, InnerHtml, HtmlClass, "", "");
        //
        public static string genericBlockTag(string TagName, string InnerHtml, string HtmlClass, string HtmlId) => genericBlockTag(TagName, InnerHtml, HtmlClass, HtmlId, "");
        //
        //====================================================================================================
        /// <summary>
        /// create an html block tag like div
        /// </summary>
        /// <param name="TagName"></param>
        /// <param name="InnerHtml"></param>
        /// <param name="HtmlName"></param>
        /// <param name="HtmlClass"></param>
        /// <param name="HtmlId"></param>
        /// <returns></returns>
        public static string genericBlockTag(string TagName, string InnerHtml, string HtmlClass, string HtmlId, string HtmlName) {
            var result = new StringBuilder("<");
            result.Append(TagName.Trim());
            result.Append(!string.IsNullOrEmpty(HtmlName) ? " name=\"" + HtmlName + "\"" : "");
            result.Append(!string.IsNullOrEmpty(HtmlClass) ? " class=\"" + HtmlClass + "\"" : "");
            result.Append(!string.IsNullOrEmpty(HtmlId) ? " id=\"" + HtmlId + "\"" : "");
            result.Append(">");
            result.Append(InnerHtml);
            result.Append("</");
            result.Append(TagName.Trim());
            result.Append(">");
            return result.ToString();
        }
        //
        //====================================================================================================
        //
        public static string li(string innerHtml) => genericBlockTag("li", innerHtml);
        //
        public static string li(string innerHtml, string htmlClass) => genericBlockTag("li", innerHtml, htmlClass, "");
        //
        public static string li(string innerHtml, string htmlClass, string htmlId) => genericBlockTag("li", innerHtml, htmlClass, htmlId);
        //
        //====================================================================================================
        //
        public static string ol(string innerHtml) => genericBlockTag("ol", innerHtml);
        //
        public static string ol(string innerHtml, string htmlClass) => genericBlockTag("ol", innerHtml, htmlClass);
        //
        public static string ol(string innerHtml, string htmlClass, string htmlId) => genericBlockTag("ol", innerHtml, htmlClass, htmlId);
        //
        //====================================================================================================
        //
        public static string ul(string innerHtml) => genericBlockTag("ul", innerHtml, "", "", "");
        //
        public static string ul(string innerHtml, string htmlClass) => genericBlockTag("ul", innerHtml, htmlClass);
        //
        public static string ul(string innerHtml, string htmlClass, string htmlId) => genericBlockTag("ul", innerHtml, htmlClass, htmlId);
        //
        //====================================================================================================
        //
        public static string div(string innerHtml) => genericBlockTag("div", innerHtml);
        //
        public static string div(string innerHtml, string htmlClass) => genericBlockTag("div", innerHtml, htmlClass);
        //
        public static string div(string innerHtml, string htmlClass, string htmlId) => genericBlockTag("div", innerHtml, htmlClass, htmlId);
        //
        //====================================================================================================
        //
        public static string p(string innerHtml) => genericBlockTag("p", innerHtml, "", "", "");
        //
        public static string p(string innerHtml, string htmlClass) => genericBlockTag("p", innerHtml, htmlClass, "", "");
        //
        public static string p(string innerHtml, string htmlClass, string htmlId) => genericBlockTag("p", innerHtml, htmlClass, htmlId, "");
        //
        //====================================================================================================
        //
        public static string h1(string innerHtml) => genericBlockTag("h1", innerHtml, "", "", "");
        //
        public static string h1(string innerHtml, string htmlClass) => genericBlockTag("h1", innerHtml, htmlClass, "", "");
        //
        public static string h1(string innerHtml, string htmlClass, string htmlId) => genericBlockTag("h1", innerHtml, htmlClass, htmlId, "");
        //
        //====================================================================================================
        //
        public static string h2(string innerHtml) => genericBlockTag("h2", "", "", "", "");
        //
        public static string h2(string innerHtml, string htmlClass) => genericBlockTag("h2", innerHtml, htmlClass, "", "");
        //
        public static string h2(string innerHtml, string htmlClass, string htmlId) => genericBlockTag("h2", innerHtml, htmlClass, htmlId, "");
        //
        //====================================================================================================
        //
        public static string h3(string innerHtml) => genericBlockTag("h3", innerHtml, "", "", "");
        //
        public static string h3(string innerHtml, string htmlClass) => genericBlockTag("h3", innerHtml, htmlClass, "", "");
        //
        public static string h3(string innerHtml, string htmlClass, string htmlId) => genericBlockTag("h3", innerHtml, htmlClass, htmlId, "");
        //
        //====================================================================================================
        //
        public static string h4(string innerHtml) => genericBlockTag("h4", innerHtml, "", "", "");
        //
        public static string h4(string innerHtml, string htmlClass) => genericBlockTag("h4", innerHtml, htmlClass, "", "");
        //
        public static string h4(string innerHtml, string htmlClass, string htmlId) => genericBlockTag("h4", innerHtml, htmlClass, htmlId, "");
        //
        //====================================================================================================
        //
        public static string h5(string innerHtml) => genericBlockTag("h5", innerHtml, "", "", "");
        //
        public static string h5(string innerHtml, string htmlClass) => genericBlockTag("h5", innerHtml, htmlClass, "", "");
        //
        public static string h5(string innerHtml, string htmlClass, string htmlId) => genericBlockTag("h5", innerHtml, htmlClass, htmlId, "");
        //
        //====================================================================================================
        //
        public static string h6(string innerHtml) => genericBlockTag("h6", innerHtml, "", "", "");
        //
        public static string h6(string innerHtml, string htmlClass) => genericBlockTag("h6", innerHtml, htmlClass, "", "");
        //
        public static string h6(string innerHtml, string htmlClass, string htmlId) => genericBlockTag("h6", innerHtml, htmlClass, htmlId, "");
        //
        //====================================================================================================
        //
        public static string label(string innerHtml, string forHtmlId = "", string htmlClass = "", string htmlId = "") {
            string s = "";
            if (!string.IsNullOrEmpty(forHtmlId)) {
                s += " for=\"" + forHtmlId + "\"";
            }
            if (!string.IsNullOrEmpty(htmlClass)) {
                s += " class=\"" + htmlClass + "\"";
            }
            if (!string.IsNullOrEmpty(htmlId)) {
                s += " id=\"" + htmlId + "\"";
            }
            return "<label " + s + ">" + innerHtml + "</label>";
        }
        //
        //====================================================================================================
        //
        public static string strong(string innerHtml, string htmlClass = "", string htmlId = "") {
            string s = "";
            if (!string.IsNullOrEmpty(htmlClass)) {
                s += " class=\"" + htmlClass + "\"";
            }
            if (!string.IsNullOrEmpty(htmlId)) {
                s += " id=\"" + htmlId + "\"";
            }
            return "<strong " + s + ">" + innerHtml + "</strong>";
        }
        //
        // ====================================================================================================
        /// <summary>
        /// img tag
        /// </summary>
        /// <param name="src"></param>
        /// <param name="alt"></param>
        /// <param name="widthPx"></param>
        /// <param name="heightPx"></param>
        /// <returns></returns>
        public static string img(string src, string alt, int widthPx, int heightPx) {
            var result = new StringBuilder("<img");
            result.Append(!string.IsNullOrEmpty(src) ? " src=\"" + src + "\"" : "");
            result.Append(!string.IsNullOrEmpty(alt) ? " alt=\"" + alt + "\"" : "");
            result.Append(!widthPx.Equals(0) ? " width=\"" + widthPx.ToString() + "\"" : "");
            result.Append(!heightPx.Equals(0) ? " height=\"" + heightPx.ToString() + "\"" : "");
            result.Append(">");
            return result.ToString();
        }
        //
        public static string img(string src, string alt, int widthPx) => img(src, alt, widthPx, 0);
        //
        public static string img(string src, string alt) => img(src, alt, 0, 0);
        //
        public static string img(string src) => img(src, "", 0, 0);
        //
        // ====================================================================================================
        /// <summary>
        /// create html span inline tag
        /// </summary>
        /// <param name="innerHtml"></param>
        /// <param name="htmlClass"></param>
        /// <returns></returns>
        public static string genericInlineTag(string htmlTag, string innerHtml, string htmlClass, string htmlId) => "<" + htmlTag + " class=\"" + htmlClass + "\" id=\"" + htmlId + "\">" + innerHtml + "</" + htmlTag + ">";
        public static string genericInlineTag(string htmlTag, string innerHtml, string htmlClass) => "<" + htmlTag + " class=\"" + htmlClass + "\">" + innerHtml + "</" + htmlTag + ">";
        public static string genericInlineTag(string htmlTag, string innerHtml) => "<" + htmlTag + ">" + innerHtml + "</" + htmlTag + ">";
        //
        // ====================================================================================================
        /// <summary>
        /// create html span inline tag
        /// </summary>
        /// <returns></returns>
        public static string span(string innerHtml, string htmlClass, string htmlId) { return genericInlineTag("span", innerHtml, htmlClass, htmlId); }
        public static string span(string innerHtml, string htmlClass) { return genericInlineTag("span", innerHtml, htmlClass); }
        public static string span(string innerHtml) { return genericInlineTag("span", innerHtml); }
        //
        // ====================================================================================================
        /// <summary>
        /// create html small inline tag
        /// </summary>
        /// <returns></returns>
        public static string small(string innerHtml, string htmlClass, string htmlId) { return genericInlineTag("small", innerHtml, htmlClass, htmlId); }
        public static string small(string innerHtml, string htmlClass) { return genericInlineTag("small", innerHtml, htmlClass); }
        public static string small(string innerHtml) { return genericInlineTag("small", innerHtml); }
        //
        // ====================================================================================================
        /// <summary>
        /// create a table start tag
        /// </summary>
        /// <param name="cellpadding"></param>
        /// <param name="cellspacing"></param>
        /// <param name="border"></param>
        /// <param name="htmlClass"></param>
        /// <returns></returns>
        public static string tableStart(int cellpadding, int cellspacing, int border, string htmlClass = "") {
            return "<table border=\"" + border + "\" cellpadding=\"" + cellpadding + "\" cellspacing=\"" + cellspacing + "\" class=\"" + htmlClass + "\" width=\"100%\">";
        }
        //
        // ====================================================================================================
        /// <summary>
        /// return a row start (tr tag)
        /// </summary>
        /// <returns></returns>
        public static string tableRowStart() {
            return "<tr>";
        }
        //
        // ====================================================================================================
        /// <summary>
        /// create a td tag (without /td)
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="ColSpan"></param>
        /// <param name="EvenRow"></param>
        /// <param name="Align"></param>
        /// <param name="BGColor"></param>
        /// <returns></returns>
        public static string tableCellStart(string Width = "", int ColSpan = 0, bool EvenRow = false, string Align = "", string BGColor = "") {
            string result = "";
            if (!string.IsNullOrEmpty(Width)) {
                result += " width=\"" + Width + "\"";
            }
            if (!string.IsNullOrEmpty(BGColor)) {
                result += " bgcolor=\"" + BGColor + "\"";
            } else if (EvenRow) {
                result += " class=\"ccPanelRowEven\"";
            } else {
                result += " class=\"ccPanelRowOdd\"";
            }
            if (ColSpan != 0) {
                result += " colspan=\"" + ColSpan + "\"";
            }
            if (!string.IsNullOrEmpty(Align)) {
                result += " align=\"" + Align + "\"";
            }
            return "<td" + result + ">";
        }
        //
        // ====================================================================================================
        /// <summary>
        /// create a table cell <td>content</td>
        /// </summary>
        /// <param name="Copy"></param>
        /// <param name="Width"></param>
        /// <param name="ColSpan"></param>
        /// <param name="EvenRow"></param>
        /// <param name="Align"></param>
        /// <param name="BGColor"></param>
        /// <returns></returns>
        public static string td(string Copy, string Width = "", int ColSpan = 0, bool EvenRow = false, string Align = "", string BGColor = "") {
            return tableCellStart(Width, ColSpan, EvenRow, Align, BGColor) + Copy + tableCellEnd;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// create a <tr><td>content</td></tr>
        /// </summary>
        /// <param name="Cell"></param>
        /// <param name="ColSpan"></param>
        /// <param name="EvenRow"></param>
        /// <returns></returns>
        public static string tableRow(string Cell, int ColSpan = 0, bool EvenRow = false) {
            return tableRowStart() + td(Cell, "100%", ColSpan, EvenRow) + kmaEndTableRow;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// convert html entities in a string
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static string encodeHtml(string Source) {
            return WebUtility.HtmlEncode(Source);
        }
        //
        // ====================================================================================================
        /// <summary>
        /// decodeHtml, Convert HTML entity strings to their text equiv
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static string decodeHtml(string Source) {
            return WebUtility.HtmlDecode(Source);
        }
        //
        // ====================================================================================================
        //
        public static string a(string innerHtml, string href) => a(innerHtml, href, "", "", "");
        public static string a(string innerHtml, string href, string htmlClass) => a(innerHtml, href, htmlClass, "", "");
        public static string a(string innerHtml, string href, string htmlClass, string htmlId) => a(innerHtml, href, htmlClass, htmlId, "");
        public static string a(string innerHtml, string href, string htmlClass, string htmlId, string tabIndex) {
            var tag = new StringBuilder("<a");
            if (!String.IsNullOrWhiteSpace(href)) { tag.Append(" href=\"").Append(href).Append("\""); }
            if (!String.IsNullOrWhiteSpace(htmlClass)) { tag.Append(" class=\"").Append(htmlClass).Append("\""); }
            if (!String.IsNullOrWhiteSpace(htmlId)) { tag.Append(" id=\"").Append(htmlId).Append("\""); }
            if (!String.IsNullOrWhiteSpace(tabIndex)) { tag.Append(" tabindex=\"").Append(tabIndex).Append("\""); }
            tag.Append(">").Append(innerHtml).Append("</a>");
            return tag.ToString();
        }
        //
        // ====================================================================================================
        //
        public static string style( string innerStyleSheet ) {
            var tag = new StringBuilder("<style type=\"text/css\">");
            tag.Append(innerStyleSheet).Append("</style>");
            return tag.ToString();
        }
        //
        // ====================================================================================================
        //
        public static string indent(string sourceHtml, int tabCnt) {
            string result = "";
            //
            //   Indent every line by 1 tab
            int posStart = GenericController.vbInstr(1, sourceHtml, "<![CDATA[", 1);
            if (posStart == 0) {
                //
                // no cdata
                //
                posStart = GenericController.vbInstr(1, sourceHtml, "<textarea", 1);
                if (posStart == 0) {
                    //
                    // no textarea
                    //
                    if (tabCnt > 0 && tabCnt < 99) {
                        result = sourceHtml.Replace("\r\n", "\r\n" + new string(Convert.ToChar("\t"), tabCnt));
                    } else {
                        result = sourceHtml.Replace("\r\n", "\r\n\t");
                    }
                    //Indent = genericController.vbReplace(SourceHtml, vbCrLf & vbTab, vbCrLf & vbTab & vbTab)
                } else {
                    //
                    // text area found, isolate it and indent before and after
                    //
                    int posEnd = GenericController.vbInstr(posStart, sourceHtml, "</textarea>", 1);
                    string pre = sourceHtml.Left(posStart - 1);
                    string post = "";
                    string target = "";
                    if (posEnd == 0) {
                        target = sourceHtml.Substring(posStart - 1);
                    } else {
                        target = sourceHtml.Substring(posStart - 1, posEnd - posStart + ((string)("</textarea>")).Length);
                        post = sourceHtml.Substring((posEnd + ((string)("</textarea>")).Length) - 1);
                    }
                    result = indent(pre, tabCnt) + target + indent(post, tabCnt);
                }
            } else {
                //
                // cdata found, isolate it and indent before and after
                //
                int posEnd = GenericController.vbInstr(posStart, sourceHtml, "]]>", 1);
                string pre = sourceHtml.Left(posStart - 1);
                string post = "";
                string target = "";
                if (posEnd == 0) {
                    target = sourceHtml.Substring(posStart - 1);
                } else {
                    target = sourceHtml.Substring(posStart - 1, posEnd - posStart + ((string)("]]>")).Length);
                    post = sourceHtml.Substring(posEnd + 2);
                }
                result = indent(pre, tabCnt) + target + indent(post, tabCnt);
            }
            return result;
        }
        //
        public static string indent(string sourceHtml) => indent(sourceHtml, 1);
    }
}
