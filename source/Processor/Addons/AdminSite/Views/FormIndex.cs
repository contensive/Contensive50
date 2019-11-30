
using System;
using System.Collections.Generic;
using Contensive.Processor;

using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Addons.AdminSite.Controllers;
using static Contensive.Processor.Addons.AdminSite.Controllers.AdminUIController;
using Contensive.BaseClasses;
using System.Text;
using Contensive.Models.Db;
using System.Globalization;

namespace Contensive.Processor.Addons.AdminSite {
    public static class FormIndex {
        //
        //========================================================================
        /// <summary>
        /// Print the index form, values and all creates a sql with leftjoins, and renames lookups as TableLookupxName where x is the TarGetFieldPtr of the field that is FieldTypeLookup
        /// </summary>
        /// <param name="adminData.content"></param>
        /// <param name="editRecord"></param>
        /// <param name="IsEmailContent"></param>
        /// <returns></returns>
        public static string get(CPClass cp, CoreController core, AdminDataModel adminData, bool IsEmailContent) {
            string result = "";
            try {
                //
                // --- make sure required fields are present
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                if (adminData.adminContent.id == 0) {
                    //
                    // Bad content id
                    Stream.Add(AdminErrorController.get(core, "This form requires a valid content definition, and one was not found for content ID [" + adminData.adminContent.id + "].", "No content definition was specified [ContentID=0]. Please contact your application developer for more assistance."));
                } else if (string.IsNullOrEmpty(adminData.adminContent.name)) {
                    //
                    // Bad content name
                    Stream.Add(AdminErrorController.get(core, "No content definition could be found for ContentID [" + adminData.adminContent.id + "]. This could be a menu error. Please contact your application developer for more assistance.", "No content definition for ContentID [" + adminData.adminContent.id + "] could be found."));
                } else if (adminData.adminContent.tableName == "") {
                    //
                    // No tablename
                    Stream.Add(AdminErrorController.get(core, "The content definition [" + adminData.adminContent.name + "] is not associated with a valid database table. Please contact your application developer for more assistance.", "Content [" + adminData.adminContent.name + "] ContentTablename is empty."));
                } else if (adminData.adminContent.fields.Count == 0) {
                    //
                    // No Fields
                    Stream.Add(AdminErrorController.get(core, "This content [" + adminData.adminContent.name + "] cannot be accessed because it has no fields. Please contact your application developer for more assistance.", "Content [" + adminData.adminContent.name + "] has no field records."));
                } else if (adminData.adminContent.developerOnly & (!core.session.isAuthenticatedDeveloper())) {
                    //
                    // Developer Content and not developer
                    Stream.Add(AdminErrorController.get(core, "Access to this content [" + adminData.adminContent.name + "] requires developer permissions. Please contact your application developer for more assistance.", "Content [" + adminData.adminContent.name + "] has no field records."));
                } else {
                    List<string> tmp = new List<string> { };
                    DataSourceModel datasource = DataSourceModel.create(core.cpParent, adminData.adminContent.dataSourceId, ref tmp);
                    //
                    // get access rights
                    var userContentPermissions = PermissionController.getUserContentPermissions(core, adminData.adminContent);
                    //
                    // detemine which subform to disaply
                    string Copy = "";
                    int SubForm = core.docProperties.getInteger(RequestNameAdminSubForm);
                    if (SubForm != 0) {
                        switch (SubForm) {
                            case AdminFormIndex_SubFormExport: {
                                    Copy = FormIndexExport.get(core, adminData);
                                    break;
                                }
                            case AdminFormIndex_SubFormSetColumns: {
                                    Copy = FormIndexSetColumnsClass.get(cp, core, adminData);
                                    break;
                                }
                            case AdminFormIndex_SubFormAdvancedSearch: {
                                    Copy = FormIndexAdvancedSearchClass.get(cp, core, adminData);
                                    break;
                                }
                            default: {
                                    // empty
                                    break;
                                }
                        }
                    }
                    Stream.Add(Copy);
                    if (string.IsNullOrEmpty(Copy)) {
                        //
                        // If subforms return empty, go to parent form
                        //
                        // -- Load Index page customizations
                        IndexConfigClass IndexConfig = IndexConfigClass.get(core, adminData);
                        setIndexSQL_ProcessIndexConfigRequests(core, adminData, ref IndexConfig);
                        GetHtmlBodyClass.setIndexSQL_SaveIndexConfig(cp, core, IndexConfig);
                        //
                        // Get the SQL parts
                        bool AllowAccessToContent = false;
                        string ContentAccessLimitMessage = "";
                        bool IsLimitedToSubContent = false;
                        string sqlWhere = "";
                        string sqlOrderBy = "";
                        string sqlFieldList = "";
                        string sqlFrom = "";
                        Dictionary<string, bool> FieldUsedInColumns = new Dictionary<string, bool>(); // used to prevent select SQL from being sorted by a field that does not appear
                        Dictionary<string, bool> IsLookupFieldValid = new Dictionary<string, bool>();
                        setIndexSQL(core, adminData, IndexConfig, ref AllowAccessToContent, ref sqlFieldList, ref sqlFrom, ref sqlWhere, ref sqlOrderBy, ref IsLimitedToSubContent, ref ContentAccessLimitMessage, ref FieldUsedInColumns, IsLookupFieldValid);
                        bool AllowAdd = adminData.adminContent.allowAdd & (!IsLimitedToSubContent) && (userContentPermissions.allowAdd);
                        bool AllowDelete = (adminData.adminContent.allowDelete) && (userContentPermissions.allowDelete);
                        if ((!userContentPermissions.allowEdit) || (!AllowAccessToContent)) {
                            //
                            // two conditions should be the same -- but not time to check - This user does not have access to this content
                            Processor.Controllers.ErrorController.addUserError(core, "Your account does not have access to any records in '" + adminData.adminContent.name + "'.");
                        } else {
                            //
                            // Get the total record count
                            string SQL = "select count(" + adminData.adminContent.tableName + ".ID) as cnt from " + sqlFrom;
                            if (!string.IsNullOrEmpty(sqlWhere)) {
                                SQL += " where " + sqlWhere;
                            }
                            int recordCnt = 0;
                            using (var csData = new CsModel(core)) {
                                if (csData.openSql(SQL, datasource.name)) {
                                    recordCnt = csData.getInteger("cnt");
                                }
                            }
                            //
                            // Assumble the SQL
                            //
                            SQL = "select";
                            if (datasource.dbTypeId != DataSourceTypeODBCMySQL) {
                                SQL += " Top " + (IndexConfig.recordTop + IndexConfig.recordsPerPage);
                            }
                            SQL += " " + sqlFieldList + " From " + sqlFrom;
                            if (!string.IsNullOrEmpty(sqlWhere)) {
                                SQL += " WHERE " + sqlWhere;
                            }
                            if (!string.IsNullOrEmpty(sqlOrderBy)) {
                                SQL += " Order By" + sqlOrderBy;
                            }
                            if (datasource.dbTypeId == DataSourceTypeODBCMySQL) {
                                SQL += " Limit " + (IndexConfig.recordTop + IndexConfig.recordsPerPage);
                            }
                            //
                            // Refresh Query String
                            //
                            core.doc.addRefreshQueryString("tr", IndexConfig.recordTop.ToString());
                            core.doc.addRefreshQueryString("asf", adminData.adminForm.ToString());
                            core.doc.addRefreshQueryString("cid", adminData.adminContent.id.ToString());
                            core.doc.addRefreshQueryString(RequestNameTitleExtension, GenericController.encodeRequestVariable(adminData.titleExtension));
                            if (adminData.wherePairCount > 0) {
                                for (int WhereCount = 0; WhereCount < adminData.wherePairCount; WhereCount++) {
                                    core.doc.addRefreshQueryString("wl" + WhereCount, adminData.wherePair[0, WhereCount]);
                                    core.doc.addRefreshQueryString("wr" + WhereCount, adminData.wherePair[1, WhereCount]);
                                }
                            }
                            //
                            // ----- Filter Data Table
                            //
                            string IndexFilterContent = "";
                            string IndexFilterHead = "";
                            string IndexFilterJS = "";
                            //
                            // Filter Nav - if enabled, just add another cell to the row
                            if (core.visitProperty.getBoolean("IndexFilterOpen", false)) {
                                //
                                // Ajax Filter Open
                                //
                                IndexFilterHead = ""
                                    + Environment.NewLine + "<div class=\"ccHeaderCon\">"
                                    + Environment.NewLine + "<div id=\"IndexFilterHeCursorTypeEnum.ADOPENed\" class=\"opened\">"
                                    + "\r<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr>"
                                    + "\r<td valign=Middle class=\"left\">Filters</td>"
                                    + "\r<td valign=Middle class=\"right\"><a href=\"#\" onClick=\"CloseIndexFilter();return false\">" + iconClose_White + "</i></a></td>"
                                    + "\r</tr></table>"
                                    + Environment.NewLine + "</div>"
                                    + Environment.NewLine + "<div id=\"IndexFilterHeadClosed\" class=\"closed\" style=\"display:none;\">"
                                    + "\r<a href=\"#\" onClick=\"OpenIndexFilter();return false\">" + iconOpen_White + "</i></a>"
                                    + Environment.NewLine + "</div>"
                                    + Environment.NewLine + "</div>"
                                    + "";
                                IndexFilterContent = ""
                                    + Environment.NewLine + "<div class=\"ccContentCon\">"
                                    + Environment.NewLine + "<div id=\"IndexFilterContentOpened\" class=\"opened\">" + getForm_IndexFilterContent(core, adminData) + "<img alt=\"space\" src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/spacer.gif\" width=\"200\" height=\"1\" style=\"clear:both\"></div>"
                                    + Environment.NewLine + "<div id=\"IndexFilterContentClosed\" class=\"closed\" style=\"display:none;\">" + adminIndexFilterClosedLabel + "</div>"
                                    + Environment.NewLine + "</div>";
                                IndexFilterJS = ""
                                    + Environment.NewLine + "<script Language=\"JavaScript\" type=\"text/javascript\">"
                                    + Environment.NewLine + "function CloseIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','none');SetDisplay('IndexFilterContentOpened','none');SetDisplay('IndexFilterHeadClosed','block');SetDisplay('IndexFilterContentClosed','block');cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxCloseIndexFilter + "','','')}"
                                    + Environment.NewLine + "function OpenIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','block');SetDisplay('IndexFilterContentOpened','block');SetDisplay('IndexFilterHeadClosed','none');SetDisplay('IndexFilterContentClosed','none');cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxOpenIndexFilter + "','','')}"
                                    + Environment.NewLine + "</script>";
                            } else {
                                //
                                // Ajax Filter Closed
                                //
                                IndexFilterHead = ""
                                    + Environment.NewLine + "<div class=\"ccHeaderCon\">"
                                    + Environment.NewLine + "<div id=\"IndexFilterHeCursorTypeEnum.ADOPENed\" class=\"opened\" style=\"display:none;\">"
                                    + "\r<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr>"
                                    + "\r<td valign=Middle class=\"left\">Filter</td>"
                                    + "\r<td valign=Middle class=\"right\"><a href=\"#\" onClick=\"CloseIndexFilter();return false\">" + iconClose_White + "</i></a></td>"
                                    + "\r</tr></table>"
                                    + Environment.NewLine + "</div>"
                                    + Environment.NewLine + "<div id=\"IndexFilterHeadClosed\" class=\"closed\">"
                                    + "\r<a href=\"#\" onClick=\"OpenIndexFilter();return false\">" + iconOpen_White + "</i></a>"
                                    + Environment.NewLine + "</div>"
                                    + Environment.NewLine + "</div>"
                                    + "";
                                IndexFilterContent = ""
                                    + Environment.NewLine + "<div class=\"ccContentCon\">"
                                    + Environment.NewLine + "<div id=\"IndexFilterContentOpened\" class=\"opened\" style=\"display:none;\"><div style=\"text-align:center;\"><img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/ajax-loader-small.gif\" width=16 height=16></div></div>"
                                    + Environment.NewLine + "<div id=\"IndexFilterContentClosed\" class=\"closed\">" + adminIndexFilterClosedLabel + "</div>"
                                    + Environment.NewLine + "<div id=\"IndexFilterContentMinWidth\" style=\"display:none;\"><img alt=\"space\" src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/spacer.gif\" width=\"200\" height=\"1\" style=\"clear:both\"></div>"
                                    + Environment.NewLine + "</div>";
                                string AjaxQS = GenericController.modifyQueryString(core.doc.refreshQueryString, RequestNameAjaxFunction, AjaxOpenIndexFilterGetContent);
                                IndexFilterJS = ""
                                    + Environment.NewLine + "<script Language=\"JavaScript\" type=\"text/javascript\">"
                                    + Environment.NewLine + "var IndexFilterPop=false;"
                                    + Environment.NewLine + "function CloseIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','none');SetDisplay('IndexFilterHeadClosed','block');SetDisplay('IndexFilterContentOpened','none');SetDisplay('IndexFilterContentMinWidth','none');SetDisplay('IndexFilterContentClosed','block');cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxCloseIndexFilter + "','','')}"
                                    + Environment.NewLine + "function OpenIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','block');SetDisplay('IndexFilterHeadClosed','none');SetDisplay('IndexFilterContentOpened','block');SetDisplay('IndexFilterContentMinWidth','block');SetDisplay('IndexFilterContentClosed','none');if(!IndexFilterPop){cj.ajax.qs('" + AjaxQS + "','','IndexFilterContentOpened');IndexFilterPop=true;}else{cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxOpenIndexFilter + "','','');}}"
                                    + Environment.NewLine + "</script>";
                            }
                            //
                            // Calculate total width
                            int ColumnWidthTotal = 0;
                            foreach (var column in IndexConfig.columns) {
                                if (column.Width < 1) {
                                    column.Width = 1;
                                }
                                ColumnWidthTotal += column.Width;
                            }
                            var DataTable_HdrRow = new StringBuilder("<tr>");
                            //
                            // Edit Column
                            DataTable_HdrRow.Append("<td width=20 align=center valign=bottom class=\"small ccAdminListCaption\">Edit</td>");
                            //
                            // Delete Select Box Columns
                            if (!AllowDelete) {
                                DataTable_HdrRow.Append("<td width=20 align=center valign=bottom class=\"small ccAdminListCaption\"><input TYPE=CheckBox disabled=\"disabled\"></td>");
                            } else {
                                DataTable_HdrRow.Append("<td width=20 align=center valign=bottom class=\"small ccAdminListCaption\"><input TYPE=CheckBox OnClick=\"CheckInputs('DelCheck',this.checked);\"></td>");
                            }
                            //
                            // field columns
                            foreach (var column in IndexConfig.columns) {
                                //
                                // ----- print column headers - anchored so they sort columns
                                //
                                int ColumnWidth = encodeInteger((100 * column.Width) / (double)ColumnWidthTotal);
                                string FieldName = column.Name;
                                //
                                // if this is a current sort ,add the reverse flag
                                //
                                StringBuilder ButtonHref = new StringBuilder();
                                ButtonHref.Append("/" + core.appConfig.adminRoute + "?" + rnAdminForm + "=" + AdminFormIndex + "&SetSortField=" + FieldName + "&RT=0&" + RequestNameTitleExtension + "=" + GenericController.encodeRequestVariable(adminData.titleExtension) + "&cid=" + adminData.adminContent.id + "&ad=" + adminData.ignore_legacyMenuDepth);
                                if (!IndexConfig.sorts.ContainsKey(FieldName)) {
                                    ButtonHref.Append("&SetSortDirection=1");
                                } else {
                                    switch (IndexConfig.sorts[FieldName].direction) {
                                        case 1: {
                                                ButtonHref.Append("&SetSortDirection=2");
                                                break;
                                            }
                                        case 2: {
                                                ButtonHref.Append("&SetSortDirection=0");
                                                break;
                                            }
                                        default: {
                                                // nothing
                                                break;
                                            }
                                    }
                                }
                                //
                                //----- column header includes WherePairCount
                                //
                                if (adminData.wherePairCount > 0) {
                                    for (int WhereCount = 0; WhereCount < adminData.wherePairCount; WhereCount++) {
                                        if (adminData.wherePair[0, WhereCount] != "") {
                                            ButtonHref.Append("&wl" + WhereCount + "=" + GenericController.encodeRequestVariable(adminData.wherePair[0, WhereCount]));
                                            ButtonHref.Append("&wr" + WhereCount + "=" + GenericController.encodeRequestVariable(adminData.wherePair[1, WhereCount]));
                                        }
                                    }
                                }
                                string ButtonFace = adminData.adminContent.fields[FieldName.ToLowerInvariant()].caption;
                                ButtonFace = GenericController.strReplace(ButtonFace, " ", "&nbsp;");
                                string SortTitle = "Sort A-Z";
                                //
                                if (IndexConfig.sorts.ContainsKey(FieldName)) {
                                    string sortSuffix = ((IndexConfig.sorts.Count < 2) ? "" : IndexConfig.sorts[FieldName].order.ToString());
                                    switch (IndexConfig.sorts[FieldName].direction) {
                                        case 1: {
                                                ButtonFace = iconArrowDown + sortSuffix + "&nbsp;" + ButtonFace;
                                                SortTitle = "Sort Z-A";
                                                break;
                                            }
                                        case 2: {
                                                ButtonFace = iconArrowUp + sortSuffix + "&nbsp;" + ButtonFace;
                                                SortTitle = "Remove Sort";
                                                break;
                                            }
                                        default: {
                                                // nothing
                                                break;
                                            }
                                    }
                                }
                                adminData.buttonObjectCount += 1;
                                DataTable_HdrRow.Append("<td width=\"" + ColumnWidth + "%\" valign=bottom align=left class=\"small ccAdminListCaption\">");
                                DataTable_HdrRow.Append("<a title=\"" + SortTitle + "\" href=\"" + HtmlController.encodeHtml(ButtonHref.ToString()) + "\" class=\"ccAdminListCaption\">" + ButtonFace + "</A>");
                                DataTable_HdrRow.Append("</td>");
                            }
                            DataTable_HdrRow.Append("</tr>");
                            //
                            //   select and print Records
                            //
                            var DataTableRows = new StringBuilder();
                            string RowColor = "";
                            int RecordPointer = 0;
                            int RecordLast = 0;
                            using (var csData = new CsModel(core)) {
                                if (csData.openSql(SQL, datasource.name, IndexConfig.recordsPerPage, IndexConfig.pageNumber)) {
                                    RecordPointer = IndexConfig.recordTop;
                                    RecordLast = IndexConfig.recordTop + IndexConfig.recordsPerPage;
                                    //
                                    // --- Print out the records
                                    while ((csData.ok()) && (RecordPointer < RecordLast)) {
                                        int RecordId = csData.getInteger("ID");
                                        if (RowColor == "class=\"ccAdminListRowOdd\"") {
                                            RowColor = "class=\"ccAdminListRowEven\"";
                                        } else {
                                            RowColor = "class=\"ccAdminListRowOdd\"";
                                        }
                                        DataTableRows.Append(Environment.NewLine + "<tr>");
                                        //
                                        // --- Edit button column
                                        DataTableRows.Append("<td align=center " + RowColor + ">");
                                        string URI = "\\" + core.appConfig.adminRoute + "?" + rnAdminAction + "=" + Constants.AdminActionNop + "&cid=" + adminData.adminContent.id + "&id=" + RecordId + "&" + RequestNameTitleExtension + "=" + GenericController.encodeRequestVariable(adminData.titleExtension) + "&ad=" + adminData.ignore_legacyMenuDepth + "&" + rnAdminSourceForm + "=" + adminData.adminForm + "&" + rnAdminForm + "=" + AdminFormEdit;
                                        if (adminData.wherePairCount > 0) {
                                            for (int WhereCount = 0; WhereCount < adminData.wherePairCount; WhereCount++) {
                                                URI = URI + "&wl" + WhereCount + "=" + GenericController.encodeRequestVariable(adminData.wherePair[0, WhereCount]) + "&wr" + WhereCount + "=" + GenericController.encodeRequestVariable(adminData.wherePair[1, WhereCount]);
                                            }
                                        }
                                        DataTableRows.Append(AdminUIController.getRecordEditLink(URI));
                                        DataTableRows.Append(("</td>"));
                                        //
                                        // --- Delete Checkbox Columns
                                        if (AllowDelete) {
                                            DataTableRows.Append("<td align=center " + RowColor + "><input TYPE=CheckBox NAME=row" + RecordPointer + " VALUE=1 ID=\"DelCheck\"><input type=hidden name=rowid" + RecordPointer + " VALUE=" + RecordId + "></span></td>");
                                        } else {
                                            DataTableRows.Append("<td align=center " + RowColor + "><input TYPE=CheckBox disabled=\"disabled\" NAME=row" + RecordPointer + " VALUE=1><input type=hidden name=rowid" + RecordPointer + " VALUE=" + RecordId + "></span></td>");
                                        }
                                        //
                                        // --- field columns
                                        foreach (var column in IndexConfig.columns) {
                                            string columnNameLc = column.Name.ToLowerInvariant();
                                            if (FieldUsedInColumns.ContainsKey(columnNameLc)) {
                                                if (FieldUsedInColumns[columnNameLc]) {
                                                    DataTableRows.Append((Environment.NewLine + "<td valign=\"middle\" " + RowColor + " align=\"left\">" + SpanClassAdminNormal));
                                                    DataTableRows.Append(getForm_Index_GetCell(core, adminData, column.Name, csData, IsLookupFieldValid[columnNameLc], GenericController.toLCase(adminData.adminContent.tableName) == "ccemail"));
                                                    DataTableRows.Append(("&nbsp;</span></td>"));
                                                }
                                            }
                                        }
                                        DataTableRows.Append(("\n    </tr>"));
                                        csData.goNext();
                                        RecordPointer = RecordPointer + 1;
                                    }
                                    DataTableRows.Append("<input type=hidden name=rowcnt value=" + RecordPointer + ">");
                                    //
                                    // --- print out the stuff at the bottom
                                    //
                                    int RecordTop_NextPage = IndexConfig.recordTop;
                                    if (csData.ok()) {
                                        RecordTop_NextPage = RecordPointer;
                                    }
                                    int RecordTop_PreviousPage = IndexConfig.recordTop - IndexConfig.recordsPerPage;
                                    if (RecordTop_PreviousPage < 0) {
                                        RecordTop_PreviousPage = 0;
                                    }
                                }
                            }
                            //
                            // Header at bottom
                            //
                            if (RowColor == "class=\"ccAdminListRowOdd\"") {
                                RowColor = "class=\"ccAdminListRowEven\"";
                            } else {
                                RowColor = "class=\"ccAdminListRowOdd\"";
                            }
                            if (RecordPointer == 0) {
                                //
                                // No records found
                                //
                                DataTableRows.Append(("<tr><td " + RowColor + " align=center>-</td><td " + RowColor + " align=center>-</td><td colspan=" + IndexConfig.columns.Count + " " + RowColor + " style=\"text-align:left ! important;\">no records were found</td></tr>"));
                            } else {
                                if (RecordPointer < RecordLast) {
                                    //
                                    // End of list
                                    //
                                    DataTableRows.Append(("<tr><td " + RowColor + " align=center>-</td><td " + RowColor + " align=center>-</td><td colspan=" + IndexConfig.columns.Count + " " + RowColor + " style=\"text-align:left ! important;\">----- end of list</td></tr>"));
                                }
                                //
                                // Add another header to the data rows
                                //
                                DataTableRows.Append(DataTable_HdrRow.ToString());
                            }
                            //
                            // ----- DataTable_FindRow
                            //
                            var DataTable_FindRow  = new StringBuilder( "<tr><td colspan=" + (2 + IndexConfig.columns.Count) + " style=\"background-color:black;height:1;\"></td></tr>");
                            DataTable_FindRow.Append("<tr>");
                            DataTable_FindRow.Append("<td valign=\"middle\" colspan=2 width=\"60\" class=\"ccPanel\" align=center style=\"vertical-align:middle;padding:8px;text-align:center ! important;\">");
                            DataTable_FindRow.Append(AdminUIController.getButtonPrimary(ButtonFind, "", false, "FindButton") + "</td>");
                            int ColumnPointer = 0;
                            foreach (var column in IndexConfig.columns) {
                                string FieldName = GenericController.toLCase(column.Name);
                                string FindWordValue = "";
                                if (IndexConfig.findWords.ContainsKey(FieldName)) {
                                    var tempVar = IndexConfig.findWords[FieldName];
                                    if ((tempVar.MatchOption == FindWordMatchEnum.matchincludes) || (tempVar.MatchOption == FindWordMatchEnum.MatchEquals)) {
                                        FindWordValue = tempVar.Value;
                                    } else if (tempVar.MatchOption == FindWordMatchEnum.MatchTrue) {
                                        FindWordValue = "true";
                                    } else if (tempVar.MatchOption == FindWordMatchEnum.MatchFalse) {
                                        FindWordValue = "false";
                                    }
                                }
                                DataTable_FindRow.Append(Environment.NewLine + "<td valign=\"middle\" align=\"center\" class=\"ccPanel3DReverse\" style=\"padding:8px;\">"
                                    + "<input type=hidden name=\"FindName" + ColumnPointer + "\" value=\"" + FieldName + "\">"
                                    + "<input class=\"form-control findInput\"  onkeypress=\"KeyCheck(event);\"  type=text id=\"F" + ColumnPointer + "\" name=\"FindValue" + ColumnPointer + "\" value=\"" + FindWordValue + "\" style=\"width:98%\">"
                                    + "</td>");
                                ColumnPointer += 1;
                            }
                            DataTable_FindRow.Append("</tr>");
                            //
                            // Assemble DataTable
                            //
                            string grid = ""
                                + "<table ID=\"DataTable\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"Background-Color:white;\">"
                                + DataTable_HdrRow + DataTableRows + DataTable_FindRow + "</table>";
                            string formContent = ""
                                + "<table ID=\"DataFilterTable\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"Background-Color:white;\">"
                                + "<tr>"
                                + "<td valign=top style=\"border-right:1px solid black;\" class=\"ccToolsCon\">" + IndexFilterJS + IndexFilterHead + IndexFilterContent + "</td>"
                                + "<td width=\"99%\" valign=top>" + grid + "</td>"
                                + "</tr>"
                                + "</table>";
                            //
                            // ----- ButtonBar
                            //
                            string ButtonBar = AdminUIController.getForm_Index_ButtonBar(core, AllowAdd, AllowDelete, IndexConfig.pageNumber, IndexConfig.recordsPerPage, recordCnt, adminData.adminContent.name);
                            string titleRow = FormIndex.getForm_Index_Header(core, IndexConfig, adminData.adminContent, recordCnt, ContentAccessLimitMessage);
                            //
                            // Assemble LiveWindowTable
                            //
                            Stream.Add(ButtonBar);
                            Stream.Add(AdminUIController.getSectionHeader(core, "", titleRow));
                            Stream.Add(formContent);
                            Stream.Add(ButtonBar);
                            Stream.Add(HtmlController.inputHidden(rnAdminSourceForm, AdminFormIndex));
                            Stream.Add(HtmlController.inputHidden("cid", adminData.adminContent.id));
                            Stream.Add(HtmlController.inputHidden("indexGoToPage", ""));
                            Stream.Add(HtmlController.inputHidden("Columncnt", IndexConfig.columns.Count));
                            core.html.addTitle(adminData.adminContent.name);
                        }
                    }
                }
                result = HtmlController.form(core, Stream.Text, "", "adminForm");
                //
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return result;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Title Bar for the index page
        /// </summary>
        /// <param name="core"></param>
        /// <param name="IndexConfig"></param>
        /// <param name="adminContext.content"></param>
        /// <param name="recordCnt"></param>
        /// <param name="ContentAccessLimitMessage"></param>
        /// <returns></returns>
        public static string getForm_Index_Header(CoreController core, IndexConfigClass IndexConfig, ContentMetadataModel content, int recordCnt, string ContentAccessLimitMessage) {
            var filterLine = new StringBuilder();
            if (IndexConfig.activeOnly) {
                filterLine.Append( ", active records");
            }
            string filterLastEdited = "";
            if (IndexConfig.lastEditedByMe) {
                filterLastEdited = filterLastEdited + " by " + core.session.user.name;
            }
            if (IndexConfig.lastEditedPast30Days) {
                filterLastEdited = filterLastEdited + " in the past 30 days";
            }
            if (IndexConfig.lastEditedPast7Days) {
                filterLastEdited = filterLastEdited + " in the week";
            }
            if (IndexConfig.lastEditedToday) {
                filterLastEdited = filterLastEdited + " today";
            }
            if (!string.IsNullOrEmpty(filterLastEdited)) {
                filterLine.Append( ", last edited" + filterLastEdited);
            }
            foreach (var kvp in IndexConfig.findWords) {
                IndexConfigClass.IndexConfigFindWordClass findWord = kvp.Value;
                if (!string.IsNullOrEmpty(findWord.Name)) {
                    var fieldMeta = ContentMetadataModel.getField(core, content, findWord.Name);
                    if (fieldMeta != null) {
                        string FieldCaption = fieldMeta.caption;
                        switch (findWord.MatchOption) {
                            case FindWordMatchEnum.MatchEmpty:
                            filterLine.Append( ", " + FieldCaption + " is empty");
                            break;
                            case FindWordMatchEnum.MatchEquals:
                            filterLine.Append( ", " + FieldCaption + " = '" + findWord.Value + "'");
                            break;
                            case FindWordMatchEnum.MatchFalse:
                            filterLine.Append( ", " + FieldCaption + " is false");
                            break;
                            case FindWordMatchEnum.MatchGreaterThan:
                            filterLine.Append( ", " + FieldCaption + " &gt; '" + findWord.Value + "'");
                            break;
                            case FindWordMatchEnum.matchincludes:
                            filterLine.Append( ", " + FieldCaption + " includes '" + findWord.Value + "'");
                            break;
                            case FindWordMatchEnum.MatchLessThan:
                            filterLine.Append( ", " + FieldCaption + " &lt; '" + findWord.Value + "'");
                            break;
                            case FindWordMatchEnum.MatchNotEmpty:
                            filterLine.Append( ", " + FieldCaption + " is not empty");
                            break;
                            case FindWordMatchEnum.MatchTrue:
                            filterLine.Append( ", " + FieldCaption + " is true");
                            break;
                        }

                    }
                }
            }
            if (IndexConfig.subCDefID > 0) {
                string ContentName = MetadataController.getContentNameByID(core, IndexConfig.subCDefID);
                if (!string.IsNullOrEmpty(ContentName)) {
                    filterLine.Append( ", in Sub-content '" + ContentName + "'");
                }
            }
            //
            // add groups to caption
            //
            if ((content.tableName.ToLowerInvariant() == "ccmembers") && (IndexConfig.groupListCnt > 0)) {
                string GroupList = "";
                for (int Ptr = 0; Ptr < IndexConfig.groupListCnt; Ptr++) {
                    if (IndexConfig.groupList[Ptr] != "") {
                        GroupList += "\t" + IndexConfig.groupList[Ptr];
                    }
                }
                if (!string.IsNullOrEmpty(GroupList)) {
                    string[] Groups = GroupList.Split('\t');
                    if (Groups.GetUpperBound(0) == 0) {
                        filterLine.Append( ", in group '" + Groups[0] + "'");
                    } else if (Groups.GetUpperBound(0) == 1) {
                        filterLine.Append( ", in groups '" + Groups[0] + "' and '" + Groups[1] + "'");
                    } else {
                        int Ptr;
                        string filterGroups = "";
                        for (Ptr = 0; Ptr < Groups.GetUpperBound(0); Ptr++) {
                            filterGroups += ", '" + Groups[Ptr] + "'";
                        }
                        filterLine.Append( ", in groups" + filterGroups.Substring(1) + " and '" + Groups[Ptr] + "'");
                    }

                }
            }
            //
            // add sort details to caption
            //
            string sortLine = "";
            foreach (var kvp in IndexConfig.sorts) {
                IndexConfigClass.IndexConfigSortClass sort = kvp.Value;
                if (sort.direction > 0) {
                    sortLine = sortLine + ", then " + content.fields[sort.fieldName].caption;
                    if (sort.direction > 1) {
                        sortLine += " reverse";
                    }
                }
            }
            string pageNavigation = getForm_index_pageNavigation(core, IndexConfig.pageNumber, IndexConfig.recordsPerPage, recordCnt, content.name);
            //
            // ----- TitleBar
            //
            string Title = HtmlController.div("<strong>" + content.name + "</strong><div style=\"float:right;\">" + pageNavigation + "</div>");
            int TitleRows = 0;
            if (!filterLine.Length.Equals(0)) {
                string link = "/" + core.appConfig.adminRoute + "?cid=" + content.id + "&af=1&IndexFilterRemoveAll=1";
                Title += HtmlController.div(getDeleteLink(link) + "&nbsp;Filter: " + HtmlController.encodeHtml(filterLine.ToString()));
                TitleRows = TitleRows + 1;
            }
            if (!string.IsNullOrEmpty(sortLine)) {
                string link = "/" + core.appConfig.adminRoute + "?cid=" + content.id + "&af=1&IndexSortRemoveAll=1";
                Title += HtmlController.div(getDeleteLink(link) + "&nbsp;Sort: " + HtmlController.encodeHtml(sortLine.Substring(6)));
                TitleRows = TitleRows + 1;
            }
            if (!string.IsNullOrEmpty(ContentAccessLimitMessage)) {
                Title += "<div style=\"clear:both\">" + ContentAccessLimitMessage + "</div>";
                TitleRows = TitleRows + 1;
            }
            return Title;
        }
        //   
        //========================================================================================
        /// <summary>
        /// Process request input on the IndexConfig
        /// </summary>
        /// <param name="core"></param>
        /// <param name="adminData"></param>
        /// <param name="IndexConfig"></param>
        public static void setIndexSQL_ProcessIndexConfigRequests(CoreController core, AdminDataModel adminData, ref IndexConfigClass IndexConfig) {
            try {
                //
                int TestInteger = 0;
                string VarText = null;
                string FindName = null;
                string FindValue = null;
                int Ptr = 0;
                int ColumnCnt = 0;
                int ColumnPtr = 0;
                string Button = null;
                if (!IndexConfig.loaded) {
                    IndexConfig = IndexConfigClass.get(core, adminData);
                }
                //
                // ----- Page number
                //
                VarText = core.docProperties.getText("rt");
                if (!string.IsNullOrEmpty(VarText)) {
                    IndexConfig.recordTop = GenericController.encodeInteger(VarText);
                }
                //
                VarText = core.docProperties.getText("RS");
                if (!string.IsNullOrEmpty(VarText)) {
                    IndexConfig.recordsPerPage = GenericController.encodeInteger(VarText);
                }
                if (IndexConfig.recordsPerPage <= 0) {
                    IndexConfig.recordsPerPage = Constants.RecordsPerPageDefault;
                }
                IndexConfig.pageNumber = encodeInteger(1 + Math.Floor(IndexConfig.recordTop / (double)IndexConfig.recordsPerPage));
                //
                // ----- Process indexGoToPage value
                //
                TestInteger = core.docProperties.getInteger("indexGoToPage");
                if (TestInteger > 0) {
                    IndexConfig.pageNumber = TestInteger;
                    IndexConfig.recordTop = ((IndexConfig.pageNumber - 1) * IndexConfig.recordsPerPage);
                } else {
                    //
                    // ----- Read filter changes and First/Next/Previous from form
                    //
                    Button = core.docProperties.getText(RequestNameButton);
                    if (!string.IsNullOrEmpty(Button)) {
                        switch (adminData.requestButton) {
                            case ButtonFirst:
                            //
                            // Force to first page
                            //
                            IndexConfig.pageNumber = 1;
                            IndexConfig.recordTop = ((IndexConfig.pageNumber - 1) * IndexConfig.recordsPerPage);
                            break;
                            case ButtonNext:
                            //
                            // Go to next page
                            //
                            IndexConfig.pageNumber = IndexConfig.pageNumber + 1;
                            IndexConfig.recordTop = ((IndexConfig.pageNumber - 1) * IndexConfig.recordsPerPage);
                            break;
                            case ButtonPrevious:
                            //
                            // Go to previous page
                            //
                            IndexConfig.pageNumber = IndexConfig.pageNumber - 1;
                            if (IndexConfig.pageNumber <= 0) {
                                IndexConfig.pageNumber = 1;
                            }
                            IndexConfig.recordTop = ((IndexConfig.pageNumber - 1) * IndexConfig.recordsPerPage);
                            break;
                            case ButtonFind:
                            //
                            // Find (change search criteria and go to first page)
                            //
                            IndexConfig.pageNumber = 1;
                            IndexConfig.recordTop = ((IndexConfig.pageNumber - 1) * IndexConfig.recordsPerPage);
                            ColumnCnt = core.docProperties.getInteger("ColumnCnt");
                            if (ColumnCnt > 0) {
                                for (ColumnPtr = 0; ColumnPtr < ColumnCnt; ColumnPtr++) {
                                    FindName = core.docProperties.getText("FindName" + ColumnPtr).ToLowerInvariant();
                                    if (!string.IsNullOrEmpty(FindName)) {
                                        if (adminData.adminContent.fields.ContainsKey(FindName.ToLowerInvariant())) {
                                            FindValue = encodeText(core.docProperties.getText("FindValue" + ColumnPtr)).Trim(' ');
                                            if (string.IsNullOrEmpty(FindValue)) {
                                                //
                                                // -- find blank, if name in list, remove it
                                                if (IndexConfig.findWords.ContainsKey(FindName)) {
                                                    IndexConfig.findWords.Remove(FindName);
                                                }
                                            } else {
                                                //
                                                // -- nonblank find, store it
                                                if (IndexConfig.findWords.ContainsKey(FindName)) {
                                                    IndexConfig.findWords[FindName].Value = FindValue;
                                                } else {
                                                    ContentFieldMetadataModel field = adminData.adminContent.fields[FindName.ToLowerInvariant()];
                                                    var findWord = new IndexConfigClass.IndexConfigFindWordClass {
                                                        Name = FindName,
                                                        Value = FindValue
                                                    };
                                                    switch (field.fieldTypeId) {
                                                        case CPContentBaseClass.FieldTypeIdEnum.AutoIdIncrement:
                                                        case CPContentBaseClass.FieldTypeIdEnum.Currency:
                                                        case CPContentBaseClass.FieldTypeIdEnum.Float:
                                                        case CPContentBaseClass.FieldTypeIdEnum.Integer:
                                                        case CPContentBaseClass.FieldTypeIdEnum.MemberSelect:
                                                        case CPContentBaseClass.FieldTypeIdEnum.Date:
                                                        findWord.MatchOption = FindWordMatchEnum.MatchEquals;
                                                        break;
                                                        case CPContentBaseClass.FieldTypeIdEnum.Boolean:
                                                        if (encodeBoolean(FindValue)) {
                                                            findWord.MatchOption = FindWordMatchEnum.MatchTrue;
                                                        } else {
                                                            findWord.MatchOption = FindWordMatchEnum.MatchFalse;
                                                        }
                                                        break;
                                                        default:
                                                        findWord.MatchOption = FindWordMatchEnum.matchincludes;
                                                        break;
                                                    }
                                                    IndexConfig.findWords.Add(FindName, findWord);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
                //
                // Process Filter form
                //
                if (core.docProperties.getBoolean("IndexFilterRemoveAll")) {
                    //
                    // Remove all filters
                    //
                    IndexConfig.findWords = new Dictionary<string, IndexConfigClass.IndexConfigFindWordClass>();
                    IndexConfig.groupListCnt = 0;
                    IndexConfig.subCDefID = 0;
                    IndexConfig.activeOnly = false;
                    IndexConfig.lastEditedByMe = false;
                    IndexConfig.lastEditedToday = false;
                    IndexConfig.lastEditedPast7Days = false;
                    IndexConfig.lastEditedPast30Days = false;
                } else {
                    int VarInteger;
                    //
                    // Add CDef
                    //
                    VarInteger = core.docProperties.getInteger("IndexFilterAddCDef");
                    if (VarInteger != 0) {
                        IndexConfig.subCDefID = VarInteger;
                        IndexConfig.pageNumber = 1;
                    }
                    //
                    // Remove CDef
                    //
                    VarInteger = core.docProperties.getInteger("IndexFilterRemoveCDef");
                    if (VarInteger != 0) {
                        IndexConfig.subCDefID = 0;
                        IndexConfig.pageNumber = 1;
                    }
                    //
                    // Add Groups
                    //
                    VarText = core.docProperties.getText("IndexFilterAddGroup").ToLowerInvariant();
                    if (!string.IsNullOrEmpty(VarText)) {
                        if (IndexConfig.groupListCnt > 0) {
                            for (Ptr = 0; Ptr < IndexConfig.groupListCnt; Ptr++) {
                                if (VarText == IndexConfig.groupList[Ptr]) {
                                    break;
                                }
                            }
                        }
                        if (Ptr == IndexConfig.groupListCnt) {
                            Array.Resize(ref IndexConfig.groupList, IndexConfig.groupListCnt + 1);
                            IndexConfig.groupList[IndexConfig.groupListCnt] = VarText;
                            IndexConfig.groupListCnt = IndexConfig.groupListCnt + 1;
                            IndexConfig.pageNumber = 1;
                        }
                    }
                    //
                    // Remove Groups
                    //
                    VarText = core.docProperties.getText("IndexFilterRemoveGroup").ToLowerInvariant();
                    if (!string.IsNullOrEmpty(VarText)) {
                        if (IndexConfig.groupListCnt > 0) {
                            for (Ptr = 0; Ptr < IndexConfig.groupListCnt; Ptr++) {
                                if (IndexConfig.groupList[Ptr] == VarText) {
                                    IndexConfig.groupList[Ptr] = "";
                                    IndexConfig.pageNumber = 1;
                                    break;
                                }
                            }
                        }
                    }
                    //
                    // Remove FindWords
                    //
                    VarText = core.docProperties.getText("IndexFilterRemoveFind").ToLowerInvariant();
                    if (!string.IsNullOrEmpty(VarText)) {
                        if (IndexConfig.findWords.ContainsKey(VarText)) {
                            IndexConfig.findWords.Remove(VarText);
                        }
                    }
                    //
                    // Read ActiveOnly
                    //
                    VarText = core.docProperties.getText("IndexFilterActiveOnly");
                    if (!string.IsNullOrEmpty(VarText)) {
                        IndexConfig.activeOnly = GenericController.encodeBoolean(VarText);
                        IndexConfig.pageNumber = 1;
                    }
                    //
                    // Read LastEditedByMe
                    //
                    VarText = core.docProperties.getText("IndexFilterLastEditedByMe");
                    if (!string.IsNullOrEmpty(VarText)) {
                        IndexConfig.lastEditedByMe = GenericController.encodeBoolean(VarText);
                        IndexConfig.pageNumber = 1;
                    }
                    //
                    // Last Edited Past 30 Days
                    //
                    VarText = core.docProperties.getText("IndexFilterLastEditedPast30Days");
                    if (!string.IsNullOrEmpty(VarText)) {
                        IndexConfig.lastEditedPast30Days = GenericController.encodeBoolean(VarText);
                        IndexConfig.lastEditedPast7Days = false;
                        IndexConfig.lastEditedToday = false;
                        IndexConfig.pageNumber = 1;
                    } else {
                        //
                        // Past 7 Days
                        //
                        VarText = core.docProperties.getText("IndexFilterLastEditedPast7Days");
                        if (!string.IsNullOrEmpty(VarText)) {
                            IndexConfig.lastEditedPast30Days = false;
                            IndexConfig.lastEditedPast7Days = GenericController.encodeBoolean(VarText);
                            IndexConfig.lastEditedToday = false;
                            IndexConfig.pageNumber = 1;
                        } else {
                            //
                            // Read LastEditedToday
                            //
                            VarText = core.docProperties.getText("IndexFilterLastEditedToday");
                            if (!string.IsNullOrEmpty(VarText)) {
                                IndexConfig.lastEditedPast30Days = false;
                                IndexConfig.lastEditedPast7Days = false;
                                IndexConfig.lastEditedToday = GenericController.encodeBoolean(VarText);
                                IndexConfig.pageNumber = 1;
                            }
                        }
                    }
                    //
                    // Read IndexFilterOpen
                    //
                    VarText = core.docProperties.getText("IndexFilterOpen");
                    if (!string.IsNullOrEmpty(VarText)) {
                        IndexConfig.open = GenericController.encodeBoolean(VarText);
                        IndexConfig.pageNumber = 1;
                    }
                    if (core.docProperties.getBoolean("IndexSortRemoveAll")) {
                        //
                        // Remove all filters
                        IndexConfig.sorts = new Dictionary<string, IndexConfigClass.IndexConfigSortClass>();
                    } else {
                        //
                        // SortField
                        string setSortField = core.docProperties.getText("SetSortField").ToLowerInvariant();
                        if (!string.IsNullOrEmpty(setSortField)) {
                            bool sortFound = IndexConfig.sorts.ContainsKey(setSortField);
                            int sortDirection = core.docProperties.getInteger("SetSortDirection");
                            if (!sortFound) {
                                IndexConfig.sorts.Add(setSortField, new IndexConfigClass.IndexConfigSortClass {
                                    fieldName = setSortField,
                                    direction = 1,
                                    order = IndexConfig.sorts.Count + 1
                                });
                            } else if (sortDirection > 0) {
                                IndexConfig.sorts[setSortField].direction = sortDirection;
                            } else {
                                IndexConfig.sorts.Remove(setSortField);
                                int sortOrder = 1;
                                foreach (var kvp in IndexConfig.sorts) {
                                    kvp.Value.order = sortOrder++;
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
        //   
        //========================================================================================
        //
        public static void setIndexSQL(CoreController core, AdminDataModel adminData, IndexConfigClass IndexConfig, ref bool Return_AllowAccess, ref string return_sqlFieldList, ref string return_sqlFrom, ref string return_SQLWhere, ref string return_SQLOrderBy, ref bool return_IsLimitedToSubContent, ref string return_ContentAccessLimitMessage, ref Dictionary<string, bool> FieldUsedInColumns, Dictionary<string, bool> IsLookupFieldValid) {
            try {
                Return_AllowAccess = true;
                //
                // ----- Workflow Fields
                return_sqlFieldList = return_sqlFieldList + adminData.adminContent.tableName + ".ID";
                //
                // ----- From Clause - build joins for Lookup fields in columns, in the findwords, and in sorts
                return_sqlFrom = adminData.adminContent.tableName;
                int FieldPtr = 0;
                foreach (KeyValuePair<string, ContentFieldMetadataModel> keyValuePair in adminData.adminContent.fields) {
                    ContentFieldMetadataModel field = keyValuePair.Value;
                    //
                    // quick fix for a replacement for the old fieldPtr (so multiple for loops will always use the same "table"+ptr string
                    FieldPtr = field.id;
                    bool IncludedInColumns = false;
                    bool IncludedInLeftJoin = false;
                    if (!IsLookupFieldValid.ContainsKey(field.nameLc)) {
                        IsLookupFieldValid.Add(field.nameLc, false);
                    }
                    if (!FieldUsedInColumns.ContainsKey(field.nameLc)) {
                        FieldUsedInColumns.Add(field.nameLc, false);
                    }
                    //
                    // test if this field is one of the columns we are displaying
                    IncludedInColumns = (IndexConfig.columns.Find(x => (x.Name == field.nameLc)) != null);
                    //
                    // disallow IncludedInColumns if a non-supported field type
                    switch (field.fieldTypeId) {
                        case CPContentBaseClass.FieldTypeIdEnum.FileCSS:
                        case CPContentBaseClass.FieldTypeIdEnum.File:
                        case CPContentBaseClass.FieldTypeIdEnum.FileImage:
                        case CPContentBaseClass.FieldTypeIdEnum.FileJavascript:
                        case CPContentBaseClass.FieldTypeIdEnum.LongText:
                        case CPContentBaseClass.FieldTypeIdEnum.ManyToMany:
                        case CPContentBaseClass.FieldTypeIdEnum.Redirect:
                        case CPContentBaseClass.FieldTypeIdEnum.FileText:
                        case CPContentBaseClass.FieldTypeIdEnum.FileXML:
                        case CPContentBaseClass.FieldTypeIdEnum.HTML:
                        case CPContentBaseClass.FieldTypeIdEnum.FileHTML:
                        IncludedInColumns = false;
                        break;
                    }
                    if ((field.fieldTypeId == CPContentBaseClass.FieldTypeIdEnum.MemberSelect) || ((field.fieldTypeId == CPContentBaseClass.FieldTypeIdEnum.Lookup) && (field.lookupContentId != 0))) {
                        //
                        // This is a lookup field -- test if IncludedInLeftJoins
                        IncludedInLeftJoin = IncludedInColumns;
                        if (IndexConfig.findWords.Count > 0) {
                            //
                            // test findwords
                            if (IndexConfig.findWords.ContainsKey(field.nameLc)) {
                                if (IndexConfig.findWords[field.nameLc].MatchOption != FindWordMatchEnum.MatchIgnore) {
                                    IncludedInLeftJoin = true;
                                }
                            }
                        }
                        if ((!IncludedInLeftJoin) && IndexConfig.sorts.Count > 0) {
                            //
                            // test sorts
                            if (IndexConfig.sorts.ContainsKey(field.nameLc.ToLowerInvariant())) {
                                IncludedInLeftJoin = true;
                            }
                        }
                        if (IncludedInLeftJoin) {
                            //
                            // include this lookup field
                            ContentMetadataModel lookupContentMetadata = null;
                            if (field.fieldTypeId == CPContentBaseClass.FieldTypeIdEnum.MemberSelect) {
                                lookupContentMetadata = ContentMetadataModel.createByUniqueName(core, "people");
                            } else {
                                lookupContentMetadata = ContentMetadataModel.create(core, field.lookupContentId);
                            }
                            if (lookupContentMetadata != null) {
                                FieldUsedInColumns[field.nameLc] = true;
                                IsLookupFieldValid[field.nameLc] = true;
                                return_sqlFieldList = return_sqlFieldList + ", LookupTable" + FieldPtr + ".Name AS LookupTable" + FieldPtr + "Name";
                                return_sqlFrom = "(" + return_sqlFrom + " LEFT JOIN " + lookupContentMetadata.tableName + " AS LookupTable" + FieldPtr + " ON " + adminData.adminContent.tableName + "." + field.nameLc + " = LookupTable" + FieldPtr + ".ID)";
                            }
                        }
                        //
                    }
                    if (IncludedInColumns) {
                        //
                        // This field is included in the columns, so include it in the select
                        return_sqlFieldList = return_sqlFieldList + " ," + adminData.adminContent.tableName + "." + field.nameLc;
                        FieldUsedInColumns[field.nameLc] = true;
                    }
                }
                //
                // Sub CDef filter
                if (IndexConfig.subCDefID > 0) {
                    var contentMetadata = Contensive.Processor.Models.Domain.ContentMetadataModel.create(core, IndexConfig.subCDefID);
                    if (contentMetadata != null) { return_SQLWhere += "AND(" + contentMetadata.legacyContentControlCriteria + ")"; }
                }
                //
                // Return_sqlFrom and Where Clause for Groups filter
                DateTime rightNow = DateTime.Now;
                string sqlRightNow = DbController.encodeSQLDate(rightNow);
                int Ptr = 0;
                if (adminData.adminContent.tableName.ToLowerInvariant() == "ccmembers") {
                    if (IndexConfig.groupListCnt > 0) {
                        for (Ptr = 0; Ptr < IndexConfig.groupListCnt; Ptr++) {
                            string GroupName = IndexConfig.groupList[Ptr];
                            if (!string.IsNullOrEmpty(GroupName)) {
                                int GroupID = MetadataController.getRecordIdByUniqueName(core, "Groups", GroupName);
                                if (GroupID == 0 && GroupName.isNumeric()) {
                                    GroupID = GenericController.encodeInteger(GroupName);
                                }
                                string groupTableAlias = "GroupFilter" + Ptr;
                                return_SQLWhere += "AND(" + groupTableAlias + ".GroupID=" + GroupID + ")and((" + groupTableAlias + ".dateExpires is null)or(" + groupTableAlias + ".dateExpires>" + sqlRightNow + "))";
                                return_sqlFrom = "(" + return_sqlFrom + " INNER JOIN ccMemberRules AS GroupFilter" + Ptr + " ON GroupFilter" + Ptr + ".memberId=ccMembers.ID)";
                            }
                        }
                    }
                }
                //
                // Add Name into Return_sqlFieldList
                return_sqlFieldList = return_sqlFieldList + " ," + adminData.adminContent.tableName + ".Name";
                //
                // paste sections together and do where clause
                if (AdminDataModel.userHasContentAccess(core, adminData.adminContent.id)) {
                    //
                    // This person can see all the records
                    return_SQLWhere += "AND(" + adminData.adminContent.legacyContentControlCriteria + ")";
                } else {
                    //
                    // Limit the Query to what they can see
                    return_IsLimitedToSubContent = true;
                    string SubQuery = "";
                    string list = adminData.adminContent.legacyContentControlCriteria;
                    adminData.adminContent.id = adminData.adminContent.id;
                    int SubContentCnt = 0;
                    if (!string.IsNullOrEmpty(list)) {
                        LogController.logInfo(core, "appendlog - adminContext.adminContext.content.contentControlCriteria=" + list);
                        string[] ListSplit = list.Split('=');
                        int Cnt = ListSplit.GetUpperBound(0) + 1;
                        if (Cnt > 0) {
                            for (Ptr = 0; Ptr < Cnt; Ptr++) {
                                int Pos = GenericController.strInstr(1, ListSplit[Ptr], ")");
                                if (Pos > 0) {
                                    int ContentId = GenericController.encodeInteger(ListSplit[Ptr].left(Pos - 1));
                                    if (ContentId > 0 && (ContentId != adminData.adminContent.id) & AdminDataModel.userHasContentAccess(core, ContentId)) {
                                        SubQuery = SubQuery + "OR(" + adminData.adminContent.tableName + ".ContentControlID=" + ContentId + ")";
                                        return_ContentAccessLimitMessage = return_ContentAccessLimitMessage + ", '<a href=\"?cid=" + ContentId + "\">" + MetadataController.getContentNameByID(core, ContentId) + "</a>'";
                                        string SubContactList = "";
                                        SubContactList += "," + ContentId;
                                        SubContentCnt = SubContentCnt + 1;
                                    }
                                }
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(SubQuery)) {
                        //
                        // Person has no access
                        Return_AllowAccess = false;
                        return;
                    } else {
                        return_SQLWhere += "AND(" + SubQuery.Substring(2) + ")";
                        return_ContentAccessLimitMessage = "Your access to " + adminData.adminContent.name + " is limited to Sub-content(s) " + return_ContentAccessLimitMessage.Substring(2);
                    }
                }
                //
                // Where Clause: Active Only
                //
                if (IndexConfig.activeOnly) {
                    return_SQLWhere += "AND(" + adminData.adminContent.tableName + ".active<>0)";
                }
                //
                // Where Clause: edited by me
                if (IndexConfig.lastEditedByMe) {
                    return_SQLWhere += "AND(" + adminData.adminContent.tableName + ".ModifiedBy=" + core.session.user.id + ")";
                }
                //
                // Where Clause: edited today
                if (IndexConfig.lastEditedToday) {
                    return_SQLWhere += "AND(" + adminData.adminContent.tableName + ".ModifiedDate>=" + DbController.encodeSQLDate(core.doc.profileStartTime.Date) + ")";
                }
                //
                // Where Clause: edited past week
                if (IndexConfig.lastEditedPast7Days) {
                    return_SQLWhere += "AND(" + adminData.adminContent.tableName + ".ModifiedDate>=" + DbController.encodeSQLDate(core.doc.profileStartTime.Date.AddDays(-7)) + ")";
                }
                //
                // Where Clause: edited past month
                if (IndexConfig.lastEditedPast30Days) {
                    return_SQLWhere += "AND(" + adminData.adminContent.tableName + ".ModifiedDate>=" + DbController.encodeSQLDate(core.doc.profileStartTime.Date.AddDays(-30)) + ")";
                }
                int WCount = 0;
                //
                // Where Clause: Where Pairs
                for (WCount = 0; WCount <= 9; WCount++) {
                    if (!string.IsNullOrEmpty(adminData.wherePair[1, WCount])) {
                        //
                        // Verify that the fieldname called out is in this table
                        if (adminData.adminContent.fields.Count > 0) {
                            foreach (KeyValuePair<string, ContentFieldMetadataModel> keyValuePair in adminData.adminContent.fields) {
                                ContentFieldMetadataModel field = keyValuePair.Value;
                                if (GenericController.toUCase(field.nameLc) == GenericController.toUCase(adminData.wherePair[0, WCount])) {
                                    //
                                    // found it, add it in the sql
                                    return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + adminData.wherePair[0, WCount] + "=";
                                    if (adminData.wherePair[1, WCount].isNumeric()) {
                                        return_SQLWhere += adminData.wherePair[1, WCount] + ")";
                                    } else {
                                        return_SQLWhere += "'" + adminData.wherePair[1, WCount] + "')";
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                //
                // Where Clause: findwords
                if (IndexConfig.findWords.Count > 0) {
                    foreach (var kvp in IndexConfig.findWords) {
                        IndexConfigClass.IndexConfigFindWordClass findword = kvp.Value;
                        int FindMatchOption = (int)findword.MatchOption;
                        if (FindMatchOption != (int)FindWordMatchEnum.MatchIgnore) {
                            string FindWordName = GenericController.toLCase(findword.Name);
                            string FindWordValue = findword.Value;
                            //
                            // Get FieldType
                            if (adminData.adminContent.fields.Count > 0) {
                                foreach (KeyValuePair<string, ContentFieldMetadataModel> keyValuePair in adminData.adminContent.fields) {
                                    ContentFieldMetadataModel field = keyValuePair.Value;
                                    //
                                    // quick fix for a replacement for the old fieldPtr (so multiple for loops will always use the same "table"+ptr string
                                    FieldPtr = field.id;
                                    if (GenericController.toLCase(field.nameLc) == FindWordName) {
                                        switch (field.fieldTypeId) {
                                            case CPContentBaseClass.FieldTypeIdEnum.AutoIdIncrement:
                                            case CPContentBaseClass.FieldTypeIdEnum.Integer:
                                            //
                                            // integer
                                            //
                                            int FindWordValueInteger = GenericController.encodeInteger(FindWordValue);
                                            switch (FindMatchOption) {
                                                case (int)FindWordMatchEnum.MatchEmpty:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + " is null)";
                                                break;
                                                case (int)FindWordMatchEnum.MatchNotEmpty:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + " is not null)";
                                                break;
                                                case (int)FindWordMatchEnum.MatchEquals:
                                                case (int)FindWordMatchEnum.matchincludes:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + "=" + DbController.encodeSQLNumber(FindWordValueInteger) + ")";
                                                break;
                                                case (int)FindWordMatchEnum.MatchGreaterThan:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + ">" + DbController.encodeSQLNumber(FindWordValueInteger) + ")";
                                                break;
                                                case (int)FindWordMatchEnum.MatchLessThan:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + "<" + DbController.encodeSQLNumber(FindWordValueInteger) + ")";
                                                break;
                                            }
                                            goto ExitLabel1;

                                            case CPContentBaseClass.FieldTypeIdEnum.Currency:
                                            case CPContentBaseClass.FieldTypeIdEnum.Float:
                                            //
                                            // double
                                            double FindWordValueDouble = GenericController.encodeNumber(FindWordValue);
                                            switch (FindMatchOption) {
                                                case (int)FindWordMatchEnum.MatchEmpty:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + " is null)";
                                                break;
                                                case (int)FindWordMatchEnum.MatchNotEmpty:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + " is not null)";
                                                break;
                                                case (int)FindWordMatchEnum.MatchEquals:
                                                case (int)FindWordMatchEnum.matchincludes:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + "=" + DbController.encodeSQLNumber(FindWordValueDouble) + ")";
                                                break;
                                                case (int)FindWordMatchEnum.MatchGreaterThan:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + ">" + DbController.encodeSQLNumber(FindWordValueDouble) + ")";
                                                break;
                                                case (int)FindWordMatchEnum.MatchLessThan:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + "<" + DbController.encodeSQLNumber(FindWordValueDouble) + ")";
                                                break;
                                            }
                                            goto ExitLabel1;
                                            case CPContentBaseClass.FieldTypeIdEnum.File:
                                            case CPContentBaseClass.FieldTypeIdEnum.FileImage:
                                            //
                                            // Date
                                            switch (FindMatchOption) {
                                                case (int)FindWordMatchEnum.MatchEmpty:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + " is null)";
                                                break;
                                                case (int)FindWordMatchEnum.MatchNotEmpty:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + " is not null)";
                                                break;
                                            }
                                            goto ExitLabel1;
                                            case CPContentBaseClass.FieldTypeIdEnum.Date:
                                            //
                                            // Date
                                            DateTime findDate = DateTime.MinValue;
                                            if (GenericController.isDate(FindWordValue)) {
                                                findDate = DateTime.Parse(FindWordValue);
                                            }
                                            switch (FindMatchOption) {
                                                case (int)FindWordMatchEnum.MatchEmpty:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + " is null)";
                                                break;
                                                case (int)FindWordMatchEnum.MatchNotEmpty:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + " is not null)";
                                                break;
                                                case (int)FindWordMatchEnum.MatchEquals:
                                                case (int)FindWordMatchEnum.matchincludes:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + "=" + DbController.encodeSQLDate(findDate) + ")";
                                                break;
                                                case (int)FindWordMatchEnum.MatchGreaterThan:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + ">" + DbController.encodeSQLDate(findDate) + ")";
                                                break;
                                                case (int)FindWordMatchEnum.MatchLessThan:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + "<" + DbController.encodeSQLDate(findDate) + ")";
                                                break;
                                            }
                                            goto ExitLabel1;
                                            case CPContentBaseClass.FieldTypeIdEnum.Lookup:
                                            case CPContentBaseClass.FieldTypeIdEnum.MemberSelect:
                                            //
                                            // Lookup
                                            if (IsLookupFieldValid[field.nameLc]) {
                                                //
                                                // Content Lookup
                                                switch (FindMatchOption) {
                                                    case (int)FindWordMatchEnum.MatchEmpty:
                                                    return_SQLWhere += "AND(LookupTable" + FieldPtr + ".ID is null)";
                                                    break;
                                                    case (int)FindWordMatchEnum.MatchNotEmpty:
                                                    return_SQLWhere += "AND(LookupTable" + FieldPtr + ".ID is not null)";
                                                    break;
                                                    case (int)FindWordMatchEnum.MatchEquals:
                                                    return_SQLWhere += "AND(LookupTable" + FieldPtr + ".Name=" + DbController.encodeSQLText(FindWordValue) + ")";
                                                    break;
                                                    case (int)FindWordMatchEnum.matchincludes:
                                                    return_SQLWhere += "AND(LookupTable" + FieldPtr + ".Name LIKE " + DbController.encodeSQLText("%" + FindWordValue + "%") + ")";
                                                    break;
                                                }
                                            } else if (field.lookupList != "") {
                                                string LookupQuery = null;
                                                //
                                                int LookupPtr = 0;
                                                string[] lookups = null;
                                                //
                                                // LookupList
                                                switch (FindMatchOption) {
                                                    case (int)FindWordMatchEnum.MatchEmpty:
                                                    return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + " is null)";
                                                    break;
                                                    case (int)FindWordMatchEnum.MatchNotEmpty:
                                                    return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + " is not null)";
                                                    break;
                                                    case (int)FindWordMatchEnum.MatchEquals:
                                                    case (int)FindWordMatchEnum.matchincludes:
                                                    lookups = field.lookupList.ToLower(CultureInfo.InvariantCulture).Split(',');
                                                    LookupQuery = "(1=0)";
                                                    for (LookupPtr = 0; LookupPtr <= lookups.GetUpperBound(0); LookupPtr++) {
                                                        if (lookups[LookupPtr].Contains(FindWordValue.ToLower(CultureInfo.InvariantCulture))) {
                                                            LookupQuery = LookupQuery + "OR(" + adminData.adminContent.tableName + "." + FindWordName + "=" + DbController.encodeSQLNumber(LookupPtr + 1) + ")";
                                                        }
                                                    }
                                                    if (!string.IsNullOrEmpty(LookupQuery)) {
                                                        return_SQLWhere += "AND(" + LookupQuery + ")";
                                                    }
                                                    break;
                                                }
                                            }
                                            goto ExitLabel1;
                                            case CPContentBaseClass.FieldTypeIdEnum.Boolean:
                                            //
                                            // Boolean
                                            switch (FindMatchOption) {
                                                case (int)FindWordMatchEnum.matchincludes:
                                                if (GenericController.encodeBoolean(FindWordValue)) {
                                                    return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + "<>0)";
                                                } else {
                                                    return_SQLWhere += "AND((" + adminData.adminContent.tableName + "." + FindWordName + "=0)or(" + adminData.adminContent.tableName + "." + FindWordName + " is null))";
                                                }
                                                break;
                                                case (int)FindWordMatchEnum.MatchTrue:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + "<>0)";
                                                break;
                                                case (int)FindWordMatchEnum.MatchFalse:
                                                return_SQLWhere += "AND((" + adminData.adminContent.tableName + "." + FindWordName + "=0)or(" + adminData.adminContent.tableName + "." + FindWordName + " is null))";
                                                break;
                                            }
                                            goto ExitLabel1;
                                            default:
                                            //
                                            // Text (and the rest)
                                            switch (FindMatchOption) {
                                                case (int)FindWordMatchEnum.MatchEmpty:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + " is null)";
                                                break;
                                                case (int)FindWordMatchEnum.MatchNotEmpty:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + " is not null)";
                                                break;
                                                case (int)FindWordMatchEnum.matchincludes:
                                                FindWordValue = DbController.encodeSQLText(FindWordValue);
                                                FindWordValue = FindWordValue.Substring(1, FindWordValue.Length - 2);
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + " LIKE '%" + FindWordValue + "%')";
                                                break;
                                                case (int)FindWordMatchEnum.MatchEquals:
                                                return_SQLWhere += "AND(" + adminData.adminContent.tableName + "." + FindWordName + "=" + DbController.encodeSQLText(FindWordValue) + ")";
                                                break;
                                            }
                                            goto ExitLabel1;
                                        }
                                    }
                                }
                            ExitLabel1:;
                            }
                        }
                    }
                }
                return_SQLWhere = return_SQLWhere.Substring(3);
                //
                // SQL Order by
                return_SQLOrderBy = "";
                string orderByDelim = " ";
                foreach (var kvp in IndexConfig.sorts) {
                    IndexConfigClass.IndexConfigSortClass sort = kvp.Value;
                    string SortFieldName = GenericController.toLCase(sort.fieldName);
                    //
                    // Get FieldType
                    if (adminData.adminContent.fields.ContainsKey(sort.fieldName)) {
                        var tempVar = adminData.adminContent.fields[sort.fieldName];
                        FieldPtr = tempVar.id; // quick fix for a replacement for the old fieldPtr (so multiple for loops will always use the same "table"+ptr string
                        if ((tempVar.fieldTypeId == CPContentBaseClass.FieldTypeIdEnum.Lookup) && IsLookupFieldValid[sort.fieldName]) {
                            return_SQLOrderBy += orderByDelim + "LookupTable" + FieldPtr + ".Name";
                        } else {
                            return_SQLOrderBy += orderByDelim + adminData.adminContent.tableName + "." + SortFieldName;
                        }
                    }
                    if (sort.direction > 1) {
                        return_SQLOrderBy = return_SQLOrderBy + " Desc";
                    }
                    orderByDelim = ",";
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //==========================================================================================================================================
        /// <summary>
        /// Get index view filter content - remote method
        /// </summary>
        /// <param name="adminData.content"></param>
        /// <returns></returns>
        public static string getForm_IndexFilterContent(CoreController core, AdminDataModel adminData) {
            string returnContent = "";
            try {
                var IndexConfig = IndexConfigClass.get(core, adminData);
                string RQS = "cid=" + adminData.adminContent.id + "&af=1";
                string Link = string.Empty;
                string QS = string.Empty;
                int Ptr = 0;
                string SubFilterList = string.Empty;
                //
                //-------------------------------------------------------------------------------------
                // Remove filters
                //-------------------------------------------------------------------------------------
                //
                if ((IndexConfig.subCDefID > 0) || (IndexConfig.groupListCnt != 0) || (IndexConfig.findWords.Count != 0) || IndexConfig.activeOnly || IndexConfig.lastEditedByMe || IndexConfig.lastEditedToday || IndexConfig.lastEditedPast7Days || IndexConfig.lastEditedPast30Days) {
                    //
                    // Remove Filters
                    //
                    returnContent += "<div class=\"ccFilterHead\">Remove&nbsp;Filters</div>";
                    Link = "/" + core.appConfig.adminRoute + "?" + GenericController.modifyQueryString(RQS, "IndexFilterRemoveAll", "1");
                    returnContent += HtmlController.div(getDeleteLink(Link) + "&nbsp;Remove All", "ccFilterSubHead");
                    //
                    // Last Edited Edited by me
                    //
                    SubFilterList = "";
                    if (IndexConfig.lastEditedByMe) {
                        Link = "/" + core.appConfig.adminRoute + "?" + GenericController.modifyQueryString(RQS, "IndexFilterLastEditedByMe", 0.ToString(), true);
                        SubFilterList += HtmlController.div(getDeleteLink(Link) + "&nbsp;By&nbsp;Me", "ccFilterIndent ccFilterList");
                    }
                    if (IndexConfig.lastEditedToday) {
                        QS = RQS;
                        QS = GenericController.modifyQueryString(QS, "IndexFilterLastEditedToday", 0.ToString(), true);
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        SubFilterList += HtmlController.div(getDeleteLink(Link) + "&nbsp;Today", "ccFilterIndent ccFilterList");
                    }
                    if (IndexConfig.lastEditedPast7Days) {
                        QS = RQS;
                        QS = GenericController.modifyQueryString(QS, "IndexFilterLastEditedPast7Days", 0.ToString(), true);
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        SubFilterList += HtmlController.div(getDeleteLink(Link) + "&nbsp;Past Week", "ccFilterIndent ccFilterList");
                    }
                    if (IndexConfig.lastEditedPast30Days) {
                        QS = RQS;
                        QS = GenericController.modifyQueryString(QS, "IndexFilterLastEditedPast30Days", 0.ToString(), true);
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        SubFilterList += HtmlController.div(getDeleteLink(Link) + "&nbsp;Past 30 Days", "ccFilterIndent ccFilterList");
                    }
                    if (!string.IsNullOrEmpty(SubFilterList)) {
                        returnContent += "<div class=\"ccFilterSubHead\">Last&nbsp;Edited</div>" + SubFilterList;
                    }
                    //
                    // Sub Content definitions
                    //
                    string SubContentName = null;
                    SubFilterList = "";
                    if (IndexConfig.subCDefID > 0) {
                        SubContentName = MetadataController.getContentNameByID(core, IndexConfig.subCDefID);
                        QS = RQS;
                        QS = GenericController.modifyQueryString(QS, "IndexFilterRemoveCDef", encodeText(IndexConfig.subCDefID));
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        SubFilterList += HtmlController.div(getDeleteLink(Link) + "&nbsp;" + SubContentName + "", "ccFilterIndent");
                    }
                    if (!string.IsNullOrEmpty(SubFilterList)) {
                        returnContent += "<div class=\"ccFilterSubHead\">In Sub-content</div>" + SubFilterList;
                    }
                    //
                    // Group Filter List
                    //
                    string GroupName = null;
                    SubFilterList = "";
                    if (IndexConfig.groupListCnt > 0) {
                        for (Ptr = 0; Ptr < IndexConfig.groupListCnt; Ptr++) {
                            GroupName = IndexConfig.groupList[Ptr];
                            if (IndexConfig.groupList[Ptr] != "") {
                                if (GroupName.Length > 30) {
                                    GroupName = GroupName.left(15) + "..." + GroupName.Substring(GroupName.Length - 15);
                                }
                                QS = RQS;
                                QS = GenericController.modifyQueryString(QS, "IndexFilterRemoveGroup", IndexConfig.groupList[Ptr]);
                                Link = "/" + core.appConfig.adminRoute + "?" + QS;
                                SubFilterList += HtmlController.div(getDeleteLink(Link) + "&nbsp;" + GroupName + "", "ccFilterIndent");
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(SubFilterList)) {
                        returnContent += "<div class=\"ccFilterSubHead\">In Group(s)</div>" + SubFilterList;
                    }
                    //
                    // Other Filter List
                    //
                    SubFilterList = "";
                    if (IndexConfig.activeOnly) {
                        QS = RQS;
                        QS = GenericController.modifyQueryString(QS, "IndexFilterActiveOnly", 0.ToString(), true);
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        SubFilterList += HtmlController.div(getDeleteLink(Link) + "&nbsp;Active&nbsp;Only", "ccFilterIndent ccFilterList");
                    }
                    if (!string.IsNullOrEmpty(SubFilterList)) {
                        returnContent += "<div class=\"ccFilterSubHead\">Other</div>" + SubFilterList;
                    }
                    //
                    // FindWords
                    //
                    foreach (var findWordKvp in IndexConfig.findWords) {
                        IndexConfigClass.IndexConfigFindWordClass findWord = findWordKvp.Value;
                        string fieldCaption = (!adminData.adminContent.fields.ContainsKey(findWord.Name.ToLower(CultureInfo.InvariantCulture))) ? findWord.Name : adminData.adminContent.fields[findWord.Name.ToLower(CultureInfo.InvariantCulture)].caption;
                        QS = RQS;
                        QS = GenericController.modifyQueryString(QS, "IndexFilterRemoveFind", findWord.Name);
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        switch (findWord.MatchOption) {
                            case FindWordMatchEnum.matchincludes:
                            returnContent += HtmlController.div(getDeleteLink(Link) + "&nbsp;" + fieldCaption + "&nbsp;includes&nbsp;'" + findWord.Value + "'", "ccFilterIndent");
                            break;
                            case FindWordMatchEnum.MatchEmpty:
                            returnContent += HtmlController.div(getDeleteLink(Link) + "&nbsp;" + fieldCaption + "&nbsp;is&nbsp;empty", "ccFilterIndent");
                            break;
                            case FindWordMatchEnum.MatchEquals:
                            returnContent += HtmlController.div(getDeleteLink(Link) + "&nbsp;" + fieldCaption + "&nbsp;=&nbsp;'" + findWord.Value + "'", "ccFilterIndent");
                            break;
                            case FindWordMatchEnum.MatchFalse:
                            returnContent += HtmlController.div(getDeleteLink(Link) + "&nbsp;" + fieldCaption + "&nbsp;is&nbsp;false", "ccFilterIndent");
                            break;
                            case FindWordMatchEnum.MatchGreaterThan:
                            returnContent += HtmlController.div(getDeleteLink(Link) + "&nbsp;" + fieldCaption + "&nbsp;&gt;&nbsp;'" + findWord.Value + "'", "ccFilterIndent");
                            break;
                            case FindWordMatchEnum.MatchLessThan:
                            returnContent += HtmlController.div(getDeleteLink(Link) + "&nbsp;" + fieldCaption + "&nbsp;&lt;&nbsp;'" + findWord.Value + "'", "ccFilterIndent");
                            break;
                            case FindWordMatchEnum.MatchNotEmpty:
                            returnContent += HtmlController.div(getDeleteLink(Link) + "&nbsp;" + fieldCaption + "&nbsp;is&nbsp;not&nbsp;empty", "ccFilterIndent");
                            break;
                            case FindWordMatchEnum.MatchTrue:
                            returnContent += HtmlController.div(getDeleteLink(Link) + "&nbsp;" + fieldCaption + "&nbsp;is&nbsp;true", "ccFilterIndent");
                            break;
                        }
                    }
                    //
                    returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
                }
                //
                //-------------------------------------------------------------------------------------
                // Add filters
                //-------------------------------------------------------------------------------------
                //
                returnContent += "<div class=\"ccFilterHead\">Add&nbsp;Filters</div>";
                //
                // Last Edited
                //
                SubFilterList = "";
                if (!IndexConfig.lastEditedByMe) {
                    QS = RQS;
                    QS = GenericController.modifyQueryString(QS, "IndexFilterLastEditedByMe", "1", true);
                    Link = "/" + core.appConfig.adminRoute + "?" + QS;
                    SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">By&nbsp;Me</a></div>";
                }
                if (!IndexConfig.lastEditedToday) {
                    QS = RQS;
                    QS = GenericController.modifyQueryString(QS, "IndexFilterLastEditedToday", "1", true);
                    Link = "/" + core.appConfig.adminRoute + "?" + QS;
                    SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Today</a></div>";
                }
                if (!IndexConfig.lastEditedPast7Days) {
                    QS = RQS;
                    QS = GenericController.modifyQueryString(QS, "IndexFilterLastEditedPast7Days", "1", true);
                    Link = "/" + core.appConfig.adminRoute + "?" + QS;
                    SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Past Week</a></div>";
                }
                if (!IndexConfig.lastEditedPast30Days) {
                    QS = RQS;
                    QS = GenericController.modifyQueryString(QS, "IndexFilterLastEditedPast30Days", "1", true);
                    Link = "/" + core.appConfig.adminRoute + "?" + QS;
                    SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Past 30 Days</a></div>";
                }
                if (!string.IsNullOrEmpty(SubFilterList)) {
                    returnContent += "<div class=\"ccFilterSubHead\">Last&nbsp;Edited</div>" + SubFilterList;
                }
                //
                // Sub Content Definitions
                //
                SubFilterList = "";
                var contentList = ContentModel.createList<ContentModel>(core.cpParent, "(contenttableid in (select id from cctables where name=" + DbController.encodeSQLText(adminData.adminContent.tableName) + "))");
                string Caption = null;
                if (contentList.Count > 1) {
                    foreach (var subContent in contentList) {
                        Caption = "<span style=\"white-space:nowrap;\">" + subContent.name + "</span>";
                        QS = RQS;
                        QS = GenericController.modifyQueryString(QS, "IndexFilterAddCDef", subContent.id.ToString(), true);
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">" + Caption + "</a></div>";
                    }
                    returnContent += "<div class=\"ccFilterSubHead\">In Sub-content</div>" + SubFilterList;
                }
                //
                // people filters
                //
                SubFilterList = "";
                if (adminData.adminContent.tableName.ToLower(CultureInfo.InvariantCulture) == GenericController.toLCase("ccmembers")) {
                    using (var csData = new CsModel(core)) {
                        csData.openSql(core.db.getSQLSelect("ccGroups", "ID,Caption,Name", "(active<>0)", "Caption,Name"));
                        while (csData.ok()) {
                            string Name = csData.getText("Name");
                            Ptr = 0;
                            if (IndexConfig.groupListCnt > 0) {
                                for (Ptr = 0; Ptr < IndexConfig.groupListCnt; Ptr++) {
                                    if (Name == IndexConfig.groupList[Ptr]) {
                                        break;
                                    }
                                }
                            }
                            if (Ptr == IndexConfig.groupListCnt) {
                                int RecordId = csData.getInteger("ID");
                                Caption = csData.getText("Caption");
                                if (string.IsNullOrEmpty(Caption)) {
                                    Caption = Name;
                                    if (string.IsNullOrEmpty(Caption)) {
                                        Caption = "Group " + RecordId;
                                    }
                                }
                                if (Caption.Length > 30) {
                                    Caption = Caption.left(15) + "..." + Caption.Substring(Caption.Length - 15);
                                }
                                Caption = "<span style=\"white-space:nowrap;\">" + Caption + "</span>";
                                QS = RQS;
                                if (!string.IsNullOrEmpty(Name.Trim(' '))) {
                                    QS = GenericController.modifyQueryString(QS, "IndexFilterAddGroup", Name, true);
                                } else {
                                    QS = GenericController.modifyQueryString(QS, "IndexFilterAddGroup", RecordId.ToString(), true);
                                }
                                Link = "/" + core.appConfig.adminRoute + "?" + QS;
                                SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">" + Caption + "</a></div>";
                            }
                            csData.goNext();
                        }
                    }
                }
                if (!string.IsNullOrEmpty(SubFilterList)) {
                    returnContent += "<div class=\"ccFilterSubHead\">In Group(s)</div>" + SubFilterList;
                }
                //
                // Active Only
                //
                SubFilterList = "";
                if (!IndexConfig.activeOnly) {
                    QS = RQS;
                    QS = GenericController.modifyQueryString(QS, "IndexFilterActiveOnly", "1", true);
                    Link = "/" + core.appConfig.adminRoute + "?" + QS;
                    SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Active&nbsp;Only</a></div>";
                }
                if (!string.IsNullOrEmpty(SubFilterList)) {
                    returnContent += "<div class=\"ccFilterSubHead\">Other</div>" + SubFilterList;
                }
                //
                returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
                //
                // Advanced Search Link
                //
                QS = RQS;
                QS = GenericController.modifyQueryString(QS, RequestNameAdminSubForm, AdminFormIndex_SubFormAdvancedSearch, true);
                Link = "/" + core.appConfig.adminRoute + "?" + QS;
                returnContent += "<div class=\"ccFilterHead\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Advanced&nbsp;Search</a></div>";
                //
                returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
                //
                // Set Column Link
                //
                QS = RQS;
                QS = GenericController.modifyQueryString(QS, RequestNameAdminSubForm, AdminFormIndex_SubFormSetColumns, true);
                Link = "/" + core.appConfig.adminRoute + "?" + QS;
                returnContent += "<div class=\"ccFilterHead\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Set&nbsp;Columns</a></div>";
                //
                returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
                //
                // Import Link
                //
                QS = RQS;
                QS = GenericController.modifyQueryString(QS, rnAdminForm, AdminFormImportWizard, true);
                Link = "/" + core.appConfig.adminRoute + "?" + QS;
                returnContent += "<div class=\"ccFilterHead\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Import</a></div>";
                //
                returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
                //
                // Export Link
                //
                QS = RQS;
                QS = GenericController.modifyQueryString(QS, RequestNameAdminSubForm, AdminFormIndex_SubFormExport, true);
                Link = "/" + core.appConfig.adminRoute + "?" + QS;
                returnContent += "<div class=\"ccFilterHead\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Export</a></div>";
                //
                returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
                //
                returnContent = "<div style=\"padding-left:10px;padding-right:10px;\">" + returnContent + "</div>";
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return returnContent;
        }
        //   
        //========================================================================
        /// <summary>
        /// Display a field in the admin index form
        /// </summary>
        /// <param name="core"></param>
        /// <param name="adminData"></param>
        /// <param name="fieldName"></param>
        /// <param name="CS"></param>
        /// <param name="IsLookupFieldValid"></param>
        /// <param name="IsEmailContent"></param>
        /// <returns></returns>
        public static string getForm_Index_GetCell(CoreController core, AdminDataModel adminData, string fieldName, CsModel csData, bool IsLookupFieldValid, bool IsEmailContent) {
            try {
                var Stream = new StringBuilderLegacyController();
                var field = adminData.adminContent.fields[fieldName.ToLowerInvariant()];
                if (field.password) {
                    //
                    // -- do not list password fields
                    Stream.Add("****");
                } else {
                    int Pos = 0;
                    switch (field.fieldTypeId) {
                        case CPContentBaseClass.FieldTypeIdEnum.File:
                        case CPContentBaseClass.FieldTypeIdEnum.FileImage: {
                                string filename = csData.getText(field.nameLc);
                                filename = GenericController.strReplace(filename, "\\", "/");
                                Pos = filename.LastIndexOf("/") + 1;
                                if (Pos != 0) {
                                    filename = filename.Substring(Pos);
                                }
                                Stream.Add(filename);
                                break;
                            }
                        case CPContentBaseClass.FieldTypeIdEnum.Lookup: {
                                if (IsLookupFieldValid) {
                                    Stream.Add(csData.getText("LookupTable" + field.id + "Name"));
                                } else if (field.lookupList != "") {
                                    string[] lookups = field.lookupList.Split(',');
                                    int LookupPtr = csData.getInteger(field.nameLc) - 1;
                                    if (LookupPtr <= lookups.GetUpperBound(0)) {
                                        if (LookupPtr >= 0) {
                                            Stream.Add(lookups[LookupPtr]);
                                        }
                                    }
                                } else {
                                    Stream.Add(" ");
                                }
                                break;
                            }
                        case CPContentBaseClass.FieldTypeIdEnum.MemberSelect: {
                                if (IsLookupFieldValid) {
                                    Stream.Add(csData.getText("LookupTable" + field.id + "Name"));
                                } else {
                                    Stream.Add(csData.getText(field.nameLc));
                                }
                                break;
                            }
                        case CPContentBaseClass.FieldTypeIdEnum.Boolean: {
                                if (csData.getBoolean(field.nameLc)) {
                                    Stream.Add("yes");
                                } else {
                                    Stream.Add("no");
                                }
                                break;
                            }
                        case CPContentBaseClass.FieldTypeIdEnum.Currency: {
                                string fieldValueText = csData.getText(field.nameLc);
                                if (string.IsNullOrWhiteSpace(fieldValueText)) {
                                    Stream.Add(fieldValueText);
                                    break;
                                }
                                Stream.Add(string.Format("{0:C}", csData.getNumber(field.nameLc)));
                                break;
                            }
                        case CPContentBaseClass.FieldTypeIdEnum.LongText:
                        case CPContentBaseClass.FieldTypeIdEnum.HTML: {
                                string fieldValueText = csData.getText(field.nameLc);
                                if (fieldValueText.Length > 50) {
                                    fieldValueText = fieldValueText.left(50) + "[more]";
                                }
                                Stream.Add(fieldValueText);
                                break;
                            }
                        case CPContentBaseClass.FieldTypeIdEnum.FileText:
                        case CPContentBaseClass.FieldTypeIdEnum.FileCSS:
                        case CPContentBaseClass.FieldTypeIdEnum.FileXML:
                        case CPContentBaseClass.FieldTypeIdEnum.FileJavascript:
                        case CPContentBaseClass.FieldTypeIdEnum.FileHTML: {
                                string filename = csData.getText(field.nameLc);
                                if (!string.IsNullOrEmpty(filename)) {
                                    string Copy = core.cdnFiles.readFileText(filename);
                                    Stream.Add(Copy);
                                }
                                break;
                            }
                        case CPContentBaseClass.FieldTypeIdEnum.Redirect:
                        case CPContentBaseClass.FieldTypeIdEnum.ManyToMany: {
                                Stream.Add("n/a");
                                break;
                            }
                        case CPContentBaseClass.FieldTypeIdEnum.Date: {
                                //
                                // -- if minvalue, use blank, if no time-part, do short-date
                                DateTime cellValueDate = csData.getDate(field.nameLc);
                                if (cellValueDate.Equals(DateTime.MinValue)) {
                                    Stream.Add("");
                                } else if (cellValueDate.Equals(cellValueDate.Date)) {
                                    Stream.Add(cellValueDate.ToShortDateString());
                                } else {
                                    Stream.Add(cellValueDate.ToString());
                                }
                                break;
                            }
                        default: {
                                string valueString = csData.getText(field.nameLc);
                                if (string.IsNullOrWhiteSpace(valueString)) {
                                    Stream.Add(valueString);
                                    break;
                                }
                                Stream.Add(csData.getText(field.nameLc));
                                break;
                            }
                    }
                }
                return HtmlController.encodeHtml(Stream.Text);
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
    }
}
