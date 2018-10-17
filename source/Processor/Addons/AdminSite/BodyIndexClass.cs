
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
    public class BodyIndexClass {
        //
        const string FilterClosedLabel = "<div style=\"font-size:9px;text-align:center;\">&nbsp;<br>F<br>i<br>l<br>t<br>e<br>r<br>s</div>";
        //
        //========================================================================
        /// <summary>
        /// Print the index form, values and all creates a sql with leftjoins, and renames lookups as TableLookupxName where x is the TarGetFieldPtr of the field that is FieldTypeLookup
        /// </summary>
        /// <param name="adminContext.content"></param>
        /// <param name="editRecord"></param>
        /// <param name="IsEmailContent"></param>
        /// <returns></returns>
        public static string get(CoreController core, AdminInfoDomainModel adminContext, bool IsEmailContent) {
            string result = "";
            try {
                //
                // --- make sure required fields are present
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                if (adminContext.adminContent.id == 0) {
                    //
                    // Bad content id
                    Stream.Add(BodyErrorClass.get(core, "This form requires a valid content definition, and one was not found for content ID [" + adminContext.adminContent.id + "].", "No content definition was specified [ContentID=0]. Please contact your application developer for more assistance."));
                } else if (string.IsNullOrEmpty(adminContext.adminContent.name)) {
                    //
                    // Bad content name
                    Stream.Add(BodyErrorClass.get( core, "No content definition could be found for ContentID [" + adminContext.adminContent.id + "]. This could be a menu error. Please contact your application developer for more assistance.", "No content definition for ContentID [" + adminContext.adminContent.id + "] could be found."));
                } else if (adminContext.adminContent.tableName == "") {
                    //
                    // No tablename
                    Stream.Add(BodyErrorClass.get( core, "The content definition [" + adminContext.adminContent.name + "] is not associated with a valid database table. Please contact your application developer for more assistance.", "Content [" + adminContext.adminContent.name + "] ContentTablename is empty."));
                } else if (adminContext.adminContent.fields.Count == 0) {
                    //
                    // No Fields
                    Stream.Add(BodyErrorClass.get( core, "This content [" + adminContext.adminContent.name + "] cannot be accessed because it has no fields. Please contact your application developer for more assistance.", "Content [" + adminContext.adminContent.name + "] has no field records."));
                } else if (adminContext.adminContent.developerOnly & (!core.session.isAuthenticatedDeveloper(core))) {
                    //
                    // Developer Content and not developer
                    Stream.Add(BodyErrorClass.get( core, "Access to this content [" + adminContext.adminContent.name + "] requires developer permissions. Please contact your application developer for more assistance.", "Content [" + adminContext.adminContent.name + "] has no field records."));
                } else {
                    List<string> tmp = new List<string> { };
                    DataSourceModel datasource = DataSourceModel.create(core, adminContext.adminContent.dataSourceId, ref tmp);
                    //
                    // get access rights
                    bool allowCMEdit = false;
                    bool allowCMAdd = false;
                    bool allowCMDelete = false;
                    core.session.getContentAccessRights(core, adminContext.adminContent.name, ref allowCMEdit, ref allowCMAdd, ref allowCMDelete);
                    //
                    // detemine which subform to disaply
                    string Copy = "";
                    int SubForm = core.docProperties.getInteger(RequestNameAdminSubForm);
                    if (SubForm != 0) {
                        switch (SubForm) {
                            case AdminFormIndex_SubFormExport:
                                Copy = BodyIndexExportClass.get( core, adminContext);
                                break;
                            case AdminFormIndex_SubFormSetColumns:
                                Copy = ToolSetListColumnsClass.GetForm_Index_SetColumns(core, adminContext);
                                break;
                            case AdminFormIndex_SubFormAdvancedSearch:
                                Copy = BodyIndexAdvancedSearchClass.get( core, adminContext);
                                break;
                        }
                    }
                    Stream.Add(Copy);
                    if (string.IsNullOrEmpty(Copy)) {
                        //
                        // If subforms return empty, go to parent form
                        //
                        // -- Load Index page customizations
                        IndexConfigClass IndexConfig = IndexConfigClass.get(core, adminContext);
                        setIndexSQL_ProcessIndexConfigRequests(core, adminContext, ref IndexConfig);
                        GetHtmlBodyClass.setIndexSQL_SaveIndexConfig(core, IndexConfig);
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
                        setIndexSQL(core, adminContext, IndexConfig, ref AllowAccessToContent, ref sqlFieldList, ref sqlFrom, ref sqlWhere, ref sqlOrderBy, ref IsLimitedToSubContent, ref ContentAccessLimitMessage, ref FieldUsedInColumns, IsLookupFieldValid);
                        bool AllowAdd = adminContext.adminContent.allowAdd & (!IsLimitedToSubContent) && (allowCMAdd);
                        bool AllowDelete = (adminContext.adminContent.allowDelete) && (allowCMDelete);
                        if ((!allowCMEdit) || (!AllowAccessToContent)) {
                            //
                            // two conditions should be the same -- but not time to check - This user does not have access to this content
                            ErrorController.addUserError(core, "Your account does not have access to any records in '" + adminContext.adminContent.name + "'.");
                        } else {
                            //
                            // Get the total record count
                            string SQL = "select count(" + adminContext.adminContent.tableName + ".ID) as cnt from " + sqlFrom;
                            if (!string.IsNullOrEmpty(sqlWhere)) {
                                SQL += " where " + sqlWhere;
                            }
                            int recordCnt = 0;
                            int CS = core.db.csOpenSql(SQL, datasource.name);
                            if (core.db.csOk(CS)) {
                                recordCnt = core.db.csGetInteger(CS, "cnt");
                            }
                            core.db.csClose(ref CS);
                            //
                            // Assumble the SQL
                            //
                            SQL = "select";
                            if (datasource.type != DataSourceTypeODBCMySQL) {
                                SQL += " Top " + (IndexConfig.RecordTop + IndexConfig.RecordsPerPage);
                            }
                            SQL += " " + sqlFieldList + " From " + sqlFrom;
                            if (!string.IsNullOrEmpty(sqlWhere)) {
                                SQL += " WHERE " + sqlWhere;
                            }
                            if (!string.IsNullOrEmpty(sqlOrderBy)) {
                                SQL += " Order By" + sqlOrderBy;
                            }
                            if (datasource.type == DataSourceTypeODBCMySQL) {
                                SQL += " Limit " + (IndexConfig.RecordTop + IndexConfig.RecordsPerPage);
                            }
                            //
                            // Refresh Query String
                            //
                            core.doc.addRefreshQueryString("tr", IndexConfig.RecordTop.ToString());
                            core.doc.addRefreshQueryString("asf", adminContext.AdminForm.ToString());
                            core.doc.addRefreshQueryString("cid", adminContext.adminContent.id.ToString());
                            core.doc.addRefreshQueryString(RequestNameTitleExtension, GenericController.encodeRequestVariable(adminContext.TitleExtension));
                            if (adminContext.WherePairCount > 0) {
                                for (int WhereCount = 0; WhereCount < adminContext.WherePairCount; WhereCount++) {
                                    core.doc.addRefreshQueryString("wl" + WhereCount, adminContext.WherePair[0, WhereCount]);
                                    core.doc.addRefreshQueryString("wr" + WhereCount, adminContext.WherePair[1, WhereCount]);
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
                                    + "\r\n<div class=\"ccHeaderCon\">"
                                    + "\r\n<div id=\"IndexFilterHeCursorTypeEnum.ADOPENed\" class=\"opened\">"
                                    + "\r<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr>"
                                    + "\r<td valign=Middle class=\"left\">Filters</td>"
                                    + "\r<td valign=Middle class=\"right\"><a href=\"#\" onClick=\"CloseIndexFilter();return false\">" + iconClose + "</i></a></td>"
                                    + "\r</tr></table>"
                                    + "\r\n</div>"
                                    + "\r\n<div id=\"IndexFilterHeadClosed\" class=\"closed\" style=\"display:none;\">"
                                    + "\r<a href=\"#\" onClick=\"OpenIndexFilter();return false\">" + iconOpen + "</i></a>"
                                    + "\r\n</div>"
                                    + "\r\n</div>"
                                    + "";
                                IndexFilterContent = ""
                                    + "\r\n<div class=\"ccContentCon\">"
                                    + "\r\n<div id=\"IndexFilterContentOpened\" class=\"opened\">" + getForm_IndexFilterContent(core, adminContext) + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"200\" height=\"1\" style=\"clear:both\"></div>"
                                    + "\r\n<div id=\"IndexFilterContentClosed\" class=\"closed\" style=\"display:none;\">" + FilterClosedLabel + "</div>"
                                    + "\r\n</div>";
                                IndexFilterJS = ""
                                    + "\r\n<script Language=\"JavaScript\" type=\"text/javascript\">"
                                    + "\r\nfunction CloseIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','none');SetDisplay('IndexFilterContentOpened','none');SetDisplay('IndexFilterHeadClosed','block');SetDisplay('IndexFilterContentClosed','block');cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxCloseIndexFilter + "','','')}"
                                    + "\r\nfunction OpenIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','block');SetDisplay('IndexFilterContentOpened','block');SetDisplay('IndexFilterHeadClosed','none');SetDisplay('IndexFilterContentClosed','none');cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxOpenIndexFilter + "','','')}"
                                    + "\r\n</script>";
                            } else {
                                //
                                // Ajax Filter Closed
                                //
                                IndexFilterHead = ""
                                    + "\r\n<div class=\"ccHeaderCon\">"
                                    + "\r\n<div id=\"IndexFilterHeCursorTypeEnum.ADOPENed\" class=\"opened\" style=\"display:none;\">"
                                    + "\r<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr>"
                                    + "\r<td valign=Middle class=\"left\">Filter</td>"
                                    + "\r<td valign=Middle class=\"right\"><a href=\"#\" onClick=\"CloseIndexFilter();return false\"><i title=\"close\" class=\"fa fa-remove\" style=\"color:#f00\"></i></a></td>"
                                    + "\r</tr></table>"
                                    + "\r\n</div>"
                                    + "\r\n<div id=\"IndexFilterHeadClosed\" class=\"closed\">"
                                    + "\r<a href=\"#\" onClick=\"OpenIndexFilter();return false\"><i title=\"open\" class=\"fa fa-angle-double-right\" style=\"color:#fff\"></i></a>"
                                    + "\r\n</div>"
                                    + "\r\n</div>"
                                    + "";
                                IndexFilterContent = ""
                                    + "\r\n<div class=\"ccContentCon\">"
                                    + "\r\n<div id=\"IndexFilterContentOpened\" class=\"opened\" style=\"display:none;\"><div style=\"text-align:center;\"><img src=\"/ccLib/images/ajax-loader-small.gif\" width=16 height=16></div></div>"
                                    + "\r\n<div id=\"IndexFilterContentClosed\" class=\"closed\">" + FilterClosedLabel + "</div>"
                                    + "\r\n<div id=\"IndexFilterContentMinWidth\" style=\"display:none;\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"200\" height=\"1\" style=\"clear:both\"></div>"
                                    + "\r\n</div>";
                                string AjaxQS = GenericController.modifyQueryString(core.doc.refreshQueryString, RequestNameAjaxFunction, AjaxOpenIndexFilterGetContent);
                                IndexFilterJS = ""
                                    + "\r\n<script Language=\"JavaScript\" type=\"text/javascript\">"
                                    + "\r\nvar IndexFilterPop=false;"
                                    + "\r\nfunction CloseIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','none');SetDisplay('IndexFilterHeadClosed','block');SetDisplay('IndexFilterContentOpened','none');SetDisplay('IndexFilterContentMinWidth','none');SetDisplay('IndexFilterContentClosed','block');cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxCloseIndexFilter + "','','')}"
                                    + "\r\nfunction OpenIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','block');SetDisplay('IndexFilterHeadClosed','none');SetDisplay('IndexFilterContentOpened','block');SetDisplay('IndexFilterContentMinWidth','block');SetDisplay('IndexFilterContentClosed','none');if(!IndexFilterPop){cj.ajax.qs('" + AjaxQS + "','','IndexFilterContentOpened');IndexFilterPop=true;}else{cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxOpenIndexFilter + "','','');}}"
                                    + "\r\n</script>";
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
                            string DataTable_HdrRow = "<tr>";
                            //
                            // Edit Column
                            DataTable_HdrRow += "<td width=20 align=center valign=bottom class=\"small ccAdminListCaption\">Edit</td>";
                            //
                            // Row Number Column
                            //DataTable_HdrRow += "<td width=20 align=center valign=bottom class=\"small ccAdminListCaption\">Row</td>";
                            //
                            // Delete Select Box Columns
                            if (!AllowDelete) {
                                DataTable_HdrRow += "<td width=20 align=center valign=bottom class=\"small ccAdminListCaption\"><input TYPE=CheckBox disabled=\"disabled\"></td>";
                            } else {
                                DataTable_HdrRow += "<td width=20 align=center valign=bottom class=\"small ccAdminListCaption\"><input TYPE=CheckBox OnClick=\"CheckInputs('DelCheck',this.checked);\"></td>";
                            }
                            //
                            // field columns
                            foreach (var column in IndexConfig.columns) {
                                //
                                // ----- print column headers - anchored so they sort columns
                                //
                                int ColumnWidth = encodeInteger((100 * column.Width) / (double)ColumnWidthTotal);
                                //fieldId = column.FieldId
                                string FieldName = column.Name;
                                //
                                //if this is a current sort ,add the reverse flag
                                //
                                string ButtonHref = "/" + core.appConfig.adminRoute + "?" + rnAdminForm + "=" + AdminFormIndex + "&SetSortField=" + FieldName + "&RT=0&" + RequestNameTitleExtension + "=" + GenericController.encodeRequestVariable(adminContext.TitleExtension) + "&cid=" + adminContext.adminContent.id + "&ad=" + adminContext.ignore_legacyMenuDepth;
                                foreach (var sortKvp in IndexConfig.Sorts) {
                                    IndexConfigClass.IndexConfigSortClass sort = sortKvp.Value;

                                }
                                if (!IndexConfig.Sorts.ContainsKey(FieldName)) {
                                    ButtonHref += "&SetSortDirection=1";
                                } else {
                                    switch (IndexConfig.Sorts[FieldName].direction) {
                                        case 1:
                                            ButtonHref += "&SetSortDirection=2";
                                            break;
                                        case 2:
                                            ButtonHref += "&SetSortDirection=0";
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                //
                                //----- column header includes WherePairCount
                                //
                                if (adminContext.WherePairCount > 0) {
                                    for (int WhereCount = 0; WhereCount < adminContext.WherePairCount; WhereCount++) {
                                        if (adminContext.WherePair[0, WhereCount] != "") {
                                            ButtonHref += "&wl" + WhereCount + "=" + GenericController.encodeRequestVariable(adminContext.WherePair[0, WhereCount]);
                                            ButtonHref += "&wr" + WhereCount + "=" + GenericController.encodeRequestVariable(adminContext.WherePair[1, WhereCount]);
                                        }
                                    }
                                }
                                string ButtonFace = adminContext.adminContent.fields[FieldName.ToLower()].caption;
                                ButtonFace = GenericController.vbReplace(ButtonFace, " ", "&nbsp;");
                                string SortTitle = "Sort A-Z";
                                //
                                if (IndexConfig.Sorts.ContainsKey(FieldName)) {
                                    string sortSuffix = ((IndexConfig.Sorts.Count < 2) ? "" : IndexConfig.Sorts[FieldName].order.ToString());
                                    switch (IndexConfig.Sorts[FieldName].direction) {
                                        case 1:
                                            ButtonFace = iconArrowDown + sortSuffix + "&nbsp;" + ButtonFace;
                                            SortTitle = "Sort Z-A";
                                            break;
                                        case 2:
                                            ButtonFace = iconArrowUp + sortSuffix + "&nbsp;" + ButtonFace;
                                            SortTitle = "Remove Sort";
                                            break;
                                    }
                                }
                                //ButtonObject = "Button" + ButtonObjectCount;
                                adminContext.ButtonObjectCount += 1;
                                DataTable_HdrRow += "<td width=\"" + ColumnWidth + "%\" valign=bottom align=left class=\"small ccAdminListCaption\">";
                                DataTable_HdrRow += ("<a title=\"" + SortTitle + "\" href=\"" + HtmlController.encodeHtml(ButtonHref) + "\" class=\"ccAdminListCaption\">" + ButtonFace + "</A>");
                                DataTable_HdrRow += ("</td>");
                            }
                            DataTable_HdrRow += ("</tr>");
                            //
                            //   select and print Records
                            //
                            string DataTable_DataRows = "";
                            string RowColor = "";
                            int RecordPointer = 0;
                            int RecordLast = 0;
                            CS = core.db.csOpenSql(SQL, datasource.name, IndexConfig.RecordsPerPage, IndexConfig.PageNumber);
                            if (core.db.csOk(CS)) {
                                RecordPointer = IndexConfig.RecordTop;
                                RecordLast = IndexConfig.RecordTop + IndexConfig.RecordsPerPage;
                                //
                                // --- Print out the records
                                while ((core.db.csOk(CS)) && (RecordPointer < RecordLast)) {
                                    int RecordID = core.db.csGetInteger(CS, "ID");
                                    //RecordName = core.db.csGetText(CS, "name");
                                    //IsLandingPage = IsPageContent And (RecordID = LandingPageID)
                                    if (RowColor == "class=\"ccAdminListRowOdd\"") {
                                        RowColor = "class=\"ccAdminListRowEven\"";
                                    } else {
                                        RowColor = "class=\"ccAdminListRowOdd\"";
                                    }
                                    DataTable_DataRows += "\r\n<tr>";
                                    //
                                    // --- Edit button column
                                    DataTable_DataRows += "<td align=center " + RowColor + ">";
                                    string URI = "\\" + core.appConfig.adminRoute + "?" + rnAdminAction + "=" + AdminInfoDomainModel.AdminActionNop + "&cid=" + adminContext.adminContent.id + "&id=" + RecordID + "&" + RequestNameTitleExtension + "=" + GenericController.encodeRequestVariable(adminContext.TitleExtension) + "&ad=" + adminContext.ignore_legacyMenuDepth + "&" + rnAdminSourceForm + "=" + adminContext.AdminForm + "&" + rnAdminForm + "=" + AdminFormEdit;
                                    if (adminContext.WherePairCount > 0) {
                                        for (int WhereCount = 0; WhereCount < adminContext.WherePairCount; WhereCount++) {
                                            URI = URI + "&wl" + WhereCount + "=" + GenericController.encodeRequestVariable(adminContext.WherePair[0, WhereCount]) + "&wr" + WhereCount + "=" + GenericController.encodeRequestVariable(adminContext.WherePair[1, WhereCount]);
                                        }
                                    }
                                    DataTable_DataRows += AdminUIController.getIconEditLink(URI);
                                    DataTable_DataRows += ("</td>");
                                    //
                                    // --- Record Number column
                                    //DataTable_DataRows += "<td align=right " + RowColor + ">" + SpanClassAdminSmall + "[" + (RecordPointer + 1) + "]</span></td>";
                                    //
                                    // --- Delete Checkbox Columns
                                    if (AllowDelete) {
                                        DataTable_DataRows += "<td align=center " + RowColor + "><input TYPE=CheckBox NAME=row" + RecordPointer + " VALUE=1 ID=\"DelCheck\"><input type=hidden name=rowid" + RecordPointer + " VALUE=" + RecordID + "></span></td>";
                                    } else {
                                        DataTable_DataRows += "<td align=center " + RowColor + "><input TYPE=CheckBox disabled=\"disabled\" NAME=row" + RecordPointer + " VALUE=1><input type=hidden name=rowid" + RecordPointer + " VALUE=" + RecordID + "></span></td>";
                                    }
                                    //
                                    // --- field columns
                                    foreach (var column in IndexConfig.columns) {
                                        string columnNameLc = column.Name.ToLower();
                                        if (FieldUsedInColumns.ContainsKey(columnNameLc)) {
                                            if (FieldUsedInColumns[columnNameLc]) {
                                                DataTable_DataRows += ("\r\n<td valign=\"middle\" " + RowColor + " align=\"left\">" + SpanClassAdminNormal);
                                                DataTable_DataRows += getForm_Index_GetCell(core, adminContext, column.Name, CS, IsLookupFieldValid[columnNameLc], GenericController.vbLCase(adminContext.adminContent.tableName) == "ccemail");
                                                DataTable_DataRows += ("&nbsp;</span></td>");
                                            }
                                        }
                                    }
                                    DataTable_DataRows += ("\n    </tr>");
                                    core.db.csGoNext(CS);
                                    RecordPointer = RecordPointer + 1;
                                }
                                DataTable_DataRows += "<input type=hidden name=rowcnt value=" + RecordPointer + ">";
                                //
                                // --- print out the stuff at the bottom
                                //
                                int RecordTop_NextPage = IndexConfig.RecordTop;
                                if (core.db.csOk(CS)) {
                                    RecordTop_NextPage = RecordPointer;
                                }
                                int RecordTop_PreviousPage = IndexConfig.RecordTop - IndexConfig.RecordsPerPage;
                                if (RecordTop_PreviousPage < 0) {
                                    RecordTop_PreviousPage = 0;
                                }
                            }
                            core.db.csClose(ref CS);
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
                                DataTable_DataRows += ("<tr><td " + RowColor + " align=center>-</td><td " + RowColor + " align=center>-</td><td colspan=" + IndexConfig.columns.Count + " " + RowColor + " style=\"text-align:left ! important;\">no records were found</td></tr>");
                            } else {
                                if (RecordPointer < RecordLast) {
                                    //
                                    // End of list
                                    //
                                    DataTable_DataRows += ("<tr><td " + RowColor + " align=center>-</td><td " + RowColor + " align=center>-</td><td colspan=" + IndexConfig.columns.Count + " " + RowColor + " style=\"text-align:left ! important;\">----- end of list</td></tr>");
                                }
                                //
                                // Add another header to the data rows
                                //
                                DataTable_DataRows += DataTable_HdrRow;
                            }
                            //
                            // ----- DataTable_FindRow
                            //
                            string DataTable_FindRow = "<tr><td colspan=" + (2 + IndexConfig.columns.Count) + " style=\"background-color:black;height:1;\"></td></tr>";
                            DataTable_FindRow += "<tr>";
                            DataTable_FindRow += "<td valign=\"middle\" colspan=2 width=\"60\" class=\"ccPanel\" align=center style=\"vertical-align:middle;padding:8px;text-align:center ! important;\">";
                            DataTable_FindRow += AdminUIController.getButtonPrimary(ButtonFind, "", false, "FindButton") + "</td>";
                            int ColumnPointer = 0;
                            foreach (var column in IndexConfig.columns) {
                                int ColumnWidth = column.Width;
                                string FieldName = GenericController.vbLCase(column.Name);
                                string FindWordValue = "";
                                if (IndexConfig.FindWords.ContainsKey(FieldName)) {
                                    var tempVar = IndexConfig.FindWords[FieldName];
                                    if ((tempVar.MatchOption == FindWordMatchEnum.matchincludes) || (tempVar.MatchOption == FindWordMatchEnum.MatchEquals)) {
                                        FindWordValue = tempVar.Value;
                                    } else if (tempVar.MatchOption == FindWordMatchEnum.MatchTrue) {
                                        FindWordValue = "true";
                                    } else if (tempVar.MatchOption == FindWordMatchEnum.MatchFalse) {
                                        FindWordValue = "false";
                                    }
                                }
                                DataTable_FindRow += "\r\n<td valign=\"middle\" align=\"center\" class=\"ccPanel3DReverse\" style=\"padding:8px;\">"
                                    + "<input type=hidden name=\"FindName" + ColumnPointer + "\" value=\"" + FieldName + "\">"
                                    + "<input class=\"form-control\"  onkeypress=\"KeyCheck(event);\"  type=text id=\"F" + ColumnPointer + "\" name=\"FindValue" + ColumnPointer + "\" value=\"" + FindWordValue + "\" style=\"width:98%\">"
                                    + "</td>";
                                ColumnPointer += 1;
                            }
                            DataTable_FindRow += "</tr>";
                            //
                            // Assemble DataTable
                            //
                            string grid = ""
                                + "<table ID=\"DataTable\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"Background-Color:white;\">"
                                + DataTable_HdrRow + DataTable_DataRows + DataTable_FindRow + "</table>";
                            //DataTable = BodyIndexAdvancedSearchClass.get( core, )
                            //
                            // Assemble DataFilterTable
                            //
                            //string filterCell = "";
                            //if (!string.IsNullOrEmpty(IndexFilterContent)) {
                            //    filterCell = "<td valign=top style=\"border-right:1px solid black;\" class=\"ccToolsCon\">" + IndexFilterJS + IndexFilterHead + IndexFilterContent + "</td>";
                            //    //FilterColumn = "<td valign=top class=""ccPanel3DReverse ccAdminEditBody"" style=""border-right:1px solid black;"">" & IndexFilterJS & IndexFilterHead & IndexFilterContent & "</td>"
                            //}
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
                            string ButtonBar = AdminUIController.getForm_Index_ButtonBar(core, AllowAdd, AllowDelete, IndexConfig.PageNumber, IndexConfig.RecordsPerPage, recordCnt, adminContext.adminContent.name);
                            string titleRow = BodyIndexClass.getForm_Index_Header(core, IndexConfig, adminContext.adminContent, recordCnt, ContentAccessLimitMessage);
                            //
                            // Assemble LiveWindowTable
                            //
                            Stream.Add(ButtonBar);
                            Stream.Add(AdminUIController.getTitleBar(core, titleRow, ""));
                            Stream.Add(formContent);
                            Stream.Add(ButtonBar);
                            //Stream.Add(core.html.getPanel("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"1\", height=\"10\" >"));
                            Stream.Add(HtmlController.inputHidden(rnAdminSourceForm, AdminFormIndex));
                            Stream.Add(HtmlController.inputHidden("cid", adminContext.adminContent.id));
                            Stream.Add(HtmlController.inputHidden("indexGoToPage", ""));
                            Stream.Add(HtmlController.inputHidden("Columncnt", IndexConfig.columns.Count));
                            core.html.addTitle(adminContext.adminContent.name);
                        }
                    }
                    //End If
                    //
                }
                result = HtmlController.form(core, Stream.Text, "", "adminForm");
                //
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
        public static string getForm_Index_Header(CoreController core, IndexConfigClass IndexConfig, CDefModel content, int recordCnt, string ContentAccessLimitMessage) {
            //
            // ----- TitleBar
            //
            string Title = "";
            string filterLine = "";
            if (IndexConfig.ActiveOnly) {
                filterLine = filterLine + ", active records";
            }
            string filterLastEdited = "";
            if (IndexConfig.LastEditedByMe) {
                filterLastEdited = filterLastEdited + " by " + core.session.user.name;
            }
            if (IndexConfig.LastEditedPast30Days) {
                filterLastEdited = filterLastEdited + " in the past 30 days";
            }
            if (IndexConfig.LastEditedPast7Days) {
                filterLastEdited = filterLastEdited + " in the week";
            }
            if (IndexConfig.LastEditedToday) {
                filterLastEdited = filterLastEdited + " today";
            }
            if (!string.IsNullOrEmpty(filterLastEdited)) {
                filterLine = filterLine + ", last edited" + filterLastEdited;
            }
            foreach (var kvp in IndexConfig.FindWords) {
                IndexConfigClass.IndexConfigFindWordClass findWord = kvp.Value;
                if (!string.IsNullOrEmpty(findWord.Name)) {
                    string FieldCaption = CdefController.getContentFieldProperty(core, content.name, findWord.Name, "caption");
                    switch (findWord.MatchOption) {
                        case FindWordMatchEnum.MatchEmpty:
                            filterLine = filterLine + ", " + FieldCaption + " is empty";
                            break;
                        case FindWordMatchEnum.MatchEquals:
                            filterLine = filterLine + ", " + FieldCaption + " = '" + findWord.Value + "'";
                            break;
                        case FindWordMatchEnum.MatchFalse:
                            filterLine = filterLine + ", " + FieldCaption + " is false";
                            break;
                        case FindWordMatchEnum.MatchGreaterThan:
                            filterLine = filterLine + ", " + FieldCaption + " &gt; '" + findWord.Value + "'";
                            break;
                        case FindWordMatchEnum.matchincludes:
                            filterLine = filterLine + ", " + FieldCaption + " includes '" + findWord.Value + "'";
                            break;
                        case FindWordMatchEnum.MatchLessThan:
                            filterLine = filterLine + ", " + FieldCaption + " &lt; '" + findWord.Value + "'";
                            break;
                        case FindWordMatchEnum.MatchNotEmpty:
                            filterLine = filterLine + ", " + FieldCaption + " is not empty";
                            break;
                        case FindWordMatchEnum.MatchTrue:
                            filterLine = filterLine + ", " + FieldCaption + " is true";
                            break;
                    }

                }
            }
            if (IndexConfig.SubCDefID > 0) {
                string ContentName = CdefController.getContentNameByID(core, IndexConfig.SubCDefID);
                if (!string.IsNullOrEmpty(ContentName)) {
                    filterLine = filterLine + ", in Sub-content '" + ContentName + "'";
                }
            }
            //
            // add groups to caption
            //
            if ((content.tableName.ToLower() == "ccmembers") && (IndexConfig.GroupListCnt > 0)) {
                string GroupList = "";
                for (int Ptr = 0; Ptr < IndexConfig.GroupListCnt; Ptr++) {
                    if (IndexConfig.GroupList[Ptr] != "") {
                        GroupList += "\t" + IndexConfig.GroupList[Ptr];
                    }
                }
                if (!string.IsNullOrEmpty(GroupList)) {
                    string[] Groups = GroupList.Split('\t');
                    if (Groups.GetUpperBound(0) == 0) {
                        filterLine = filterLine + ", in group '" + Groups[0] + "'";
                    } else if (Groups.GetUpperBound(0) == 1) {
                        filterLine = filterLine + ", in groups '" + Groups[0] + "' and '" + Groups[1] + "'";
                    } else {
                        int Ptr;
                        string filterGroups = "";
                        for (Ptr = 0; Ptr < Groups.GetUpperBound(0); Ptr++) {
                            filterGroups += ", '" + Groups[Ptr] + "'";
                        }
                        filterLine = filterLine + ", in groups" + filterGroups.Substring(1) + " and '" + Groups[Ptr] + "'";
                    }

                }
            }
            //
            // add sort details to caption
            //
            string sortLine = "";
            foreach (var kvp in IndexConfig.Sorts) {
                IndexConfigClass.IndexConfigSortClass sort = kvp.Value;
                if (sort.direction > 0) {
                    sortLine = sortLine + ", then " + content.fields[sort.fieldName].caption;
                    if (sort.direction > 1) {
                        sortLine += " reverse";
                    }
                }
            }
            string pageNavigation = getForm_index_pageNavigation(core, IndexConfig.PageNumber, IndexConfig.RecordsPerPage, recordCnt, content.name);
            Title = HtmlController.div("<strong>" + content.name + "</strong><div style=\"float:right;\">" + pageNavigation + "</div>");
            int TitleRows = 0;
            if (!string.IsNullOrEmpty(filterLine)) {
                string link = "/" + core.appConfig.adminRoute + "?cid=" + content.id + "&af=1&IndexFilterRemoveAll=1";
                Title += HtmlController.div(getIconDeleteLink(link) + "&nbsp;Filter: " + HtmlController.encodeHtml(filterLine.Substring(2)));
                TitleRows = TitleRows + 1;
            }
            if (!string.IsNullOrEmpty(sortLine)) {
                string link = "/" + core.appConfig.adminRoute + "?cid=" + content.id + "&af=1&IndexSortRemoveAll=1";
                Title += HtmlController.div(getIconDeleteLink(link) + "&nbsp;Sort: " + HtmlController.encodeHtml(sortLine.Substring(6)));
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
        /// <param name="adminContext"></param>
        /// <param name="IndexConfig"></param>
        public static void setIndexSQL_ProcessIndexConfigRequests( CoreController core, AdminInfoDomainModel adminContext, ref IndexConfigClass IndexConfig) {
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
                if (!IndexConfig.Loaded) {
                    IndexConfig = IndexConfigClass.get(core, adminContext);
                }
                //
                // ----- Page number
                //
                VarText = core.docProperties.getText("rt");
                if (!string.IsNullOrEmpty(VarText)) {
                    IndexConfig.RecordTop = GenericController.encodeInteger(VarText);
                }
                //
                VarText = core.docProperties.getText("RS");
                if (!string.IsNullOrEmpty(VarText)) {
                    IndexConfig.RecordsPerPage = GenericController.encodeInteger(VarText);
                }
                if (IndexConfig.RecordsPerPage <= 0) {
                    IndexConfig.RecordsPerPage = AdminInfoDomainModel.RecordsPerPageDefault;
                }
                IndexConfig.PageNumber = encodeInteger(1 + Math.Floor(IndexConfig.RecordTop / (double)IndexConfig.RecordsPerPage));
                //
                // ----- Process indexGoToPage value
                //
                TestInteger = core.docProperties.getInteger("indexGoToPage");
                if (TestInteger > 0) {
                    IndexConfig.PageNumber = TestInteger;
                    IndexConfig.RecordTop = ((IndexConfig.PageNumber - 1) * IndexConfig.RecordsPerPage);
                } else {
                    //
                    // ----- Read filter changes and First/Next/Previous from form
                    //
                    Button = core.docProperties.getText(RequestNameButton);
                    if (!string.IsNullOrEmpty(Button)) {
                        switch (adminContext.requestButton) {
                            case ButtonFirst:
                                //
                                // Force to first page
                                //
                                IndexConfig.PageNumber = 1;
                                IndexConfig.RecordTop = ((IndexConfig.PageNumber - 1) * IndexConfig.RecordsPerPage);
                                break;
                            case ButtonNext:
                                //
                                // Go to next page
                                //
                                IndexConfig.PageNumber = IndexConfig.PageNumber + 1;
                                IndexConfig.RecordTop = ((IndexConfig.PageNumber - 1) * IndexConfig.RecordsPerPage);
                                break;
                            case ButtonPrevious:
                                //
                                // Go to previous page
                                //
                                IndexConfig.PageNumber = IndexConfig.PageNumber - 1;
                                if (IndexConfig.PageNumber <= 0) {
                                    IndexConfig.PageNumber = 1;
                                }
                                IndexConfig.RecordTop = ((IndexConfig.PageNumber - 1) * IndexConfig.RecordsPerPage);
                                break;
                            case ButtonFind:
                                //
                                // Find (change search criteria and go to first page)
                                //
                                IndexConfig.PageNumber = 1;
                                IndexConfig.RecordTop = ((IndexConfig.PageNumber - 1) * IndexConfig.RecordsPerPage);
                                ColumnCnt = core.docProperties.getInteger("ColumnCnt");
                                if (ColumnCnt > 0) {
                                    for (ColumnPtr = 0; ColumnPtr < ColumnCnt; ColumnPtr++) {
                                        FindName = core.docProperties.getText("FindName" + ColumnPtr).ToLower();
                                        if (!string.IsNullOrEmpty(FindName)) {
                                            if (adminContext.adminContent.fields.ContainsKey(FindName.ToLower())) {
                                                FindValue = encodeText(core.docProperties.getText("FindValue" + ColumnPtr)).Trim(' ');
                                                if (string.IsNullOrEmpty(FindValue)) {
                                                    //
                                                    // -- find blank, if name in list, remove it
                                                    if (IndexConfig.FindWords.ContainsKey(FindName)) {
                                                        IndexConfig.FindWords.Remove(FindName);
                                                    }
                                                } else {
                                                    //
                                                    // -- nonblank find, store it
                                                    if (IndexConfig.FindWords.ContainsKey(FindName)) {
                                                        IndexConfig.FindWords[FindName].Value = FindValue;
                                                    } else {
                                                        CDefFieldModel field = adminContext.adminContent.fields[FindName.ToLower()];
                                                        var findWord = new IndexConfigClass.IndexConfigFindWordClass {
                                                            Name = FindName,
                                                            Value = FindValue
                                                        };
                                                        switch (field.fieldTypeId) {
                                                            case FieldTypeIdAutoIdIncrement:
                                                            case FieldTypeIdCurrency:
                                                            case FieldTypeIdFloat:
                                                            case FieldTypeIdInteger:
                                                            case FieldTypeIdMemberSelect:
                                                            case FieldTypeIdDate:
                                                                findWord.MatchOption = FindWordMatchEnum.MatchEquals;
                                                                break;
                                                            case FieldTypeIdBoolean:
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
                                                        IndexConfig.FindWords.Add(FindName, findWord);
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
                    IndexConfig.FindWords = new Dictionary<string, IndexConfigClass.IndexConfigFindWordClass>();
                    IndexConfig.GroupListCnt = 0;
                    IndexConfig.SubCDefID = 0;
                    IndexConfig.ActiveOnly = false;
                    IndexConfig.LastEditedByMe = false;
                    IndexConfig.LastEditedToday = false;
                    IndexConfig.LastEditedPast7Days = false;
                    IndexConfig.LastEditedPast30Days = false;
                } else {
                    int VarInteger;
                    //
                    // Add CDef
                    //
                    VarInteger = core.docProperties.getInteger("IndexFilterAddCDef");
                    if (VarInteger != 0) {
                        IndexConfig.SubCDefID = VarInteger;
                        IndexConfig.PageNumber = 1;
                        //                If .SubCDefCnt > 0 Then
                        //                    For Ptr = 0 To .SubCDefCnt - 1
                        //                        If VarInteger = .SubCDefs[Ptr] Then
                        //                            Exit For
                        //                        End If
                        //                    Next
                        //                End If
                        //                If Ptr = .SubCDefCnt Then
                        //                    ReDim Preserve .SubCDefs(.SubCDefCnt)
                        //                    .SubCDefs(.SubCDefCnt) = VarInteger
                        //                    .SubCDefCnt = .SubCDefCnt + 1
                        //                    .PageNumber = 1
                        //                End If
                    }
                    //
                    // Remove CDef
                    //
                    VarInteger = core.docProperties.getInteger("IndexFilterRemoveCDef");
                    if (VarInteger != 0) {
                        IndexConfig.SubCDefID = 0;
                        IndexConfig.PageNumber = 1;
                        //                If .SubCDefCnt > 0 Then
                        //                    For Ptr = 0 To .SubCDefCnt - 1
                        //                        If .SubCDefs[Ptr] = VarInteger Then
                        //                            .SubCDefs[Ptr] = 0
                        //                            .PageNumber = 1
                        //                            Exit For
                        //                        End If
                        //                    Next
                        //                End If
                    }
                    //
                    // Add Groups
                    //
                    VarText = core.docProperties.getText("IndexFilterAddGroup").ToLower();
                    if (!string.IsNullOrEmpty(VarText)) {
                        if (IndexConfig.GroupListCnt > 0) {
                            for (Ptr = 0; Ptr < IndexConfig.GroupListCnt; Ptr++) {
                                if (VarText == IndexConfig.GroupList[Ptr]) {
                                    break;
                                }
                            }
                        }
                        if (Ptr == IndexConfig.GroupListCnt) {
                            Array.Resize(ref IndexConfig.GroupList, IndexConfig.GroupListCnt + 1);
                            IndexConfig.GroupList[IndexConfig.GroupListCnt] = VarText;
                            IndexConfig.GroupListCnt = IndexConfig.GroupListCnt + 1;
                            IndexConfig.PageNumber = 1;
                        }
                    }
                    //
                    // Remove Groups
                    //
                    VarText = core.docProperties.getText("IndexFilterRemoveGroup").ToLower();
                    if (!string.IsNullOrEmpty(VarText)) {
                        if (IndexConfig.GroupListCnt > 0) {
                            for (Ptr = 0; Ptr < IndexConfig.GroupListCnt; Ptr++) {
                                if (IndexConfig.GroupList[Ptr] == VarText) {
                                    IndexConfig.GroupList[Ptr] = "";
                                    IndexConfig.PageNumber = 1;
                                    break;
                                }
                            }
                        }
                    }
                    //
                    // Remove FindWords
                    //
                    VarText = core.docProperties.getText("IndexFilterRemoveFind").ToLower();
                    if (!string.IsNullOrEmpty(VarText)) {
                        if (IndexConfig.FindWords.ContainsKey(VarText)) {
                            IndexConfig.FindWords.Remove(VarText);
                        }
                        //If .FindWords.Count > 0 Then
                        //    For Ptr = 0 To .FindWords.Count - 1
                        //        If .FindWords[Ptr].Name = VarText Then
                        //            .FindWords[Ptr].MatchOption = FindWordMatchEnum.MatchIgnore
                        //            .FindWords[Ptr].Value = ""
                        //            .PageNumber = 1
                        //            Exit For
                        //        End If
                        //    Next
                        //End If
                    }
                    //
                    // Read ActiveOnly
                    //
                    VarText = core.docProperties.getText("IndexFilterActiveOnly");
                    if (!string.IsNullOrEmpty(VarText)) {
                        IndexConfig.ActiveOnly = GenericController.encodeBoolean(VarText);
                        IndexConfig.PageNumber = 1;
                    }
                    //
                    // Read LastEditedByMe
                    //
                    VarText = core.docProperties.getText("IndexFilterLastEditedByMe");
                    if (!string.IsNullOrEmpty(VarText)) {
                        IndexConfig.LastEditedByMe = GenericController.encodeBoolean(VarText);
                        IndexConfig.PageNumber = 1;
                    }
                    //
                    // Last Edited Past 30 Days
                    //
                    VarText = core.docProperties.getText("IndexFilterLastEditedPast30Days");
                    if (!string.IsNullOrEmpty(VarText)) {
                        IndexConfig.LastEditedPast30Days = GenericController.encodeBoolean(VarText);
                        IndexConfig.LastEditedPast7Days = false;
                        IndexConfig.LastEditedToday = false;
                        IndexConfig.PageNumber = 1;
                    } else {
                        //
                        // Past 7 Days
                        //
                        VarText = core.docProperties.getText("IndexFilterLastEditedPast7Days");
                        if (!string.IsNullOrEmpty(VarText)) {
                            IndexConfig.LastEditedPast30Days = false;
                            IndexConfig.LastEditedPast7Days = GenericController.encodeBoolean(VarText);
                            IndexConfig.LastEditedToday = false;
                            IndexConfig.PageNumber = 1;
                        } else {
                            //
                            // Read LastEditedToday
                            //
                            VarText = core.docProperties.getText("IndexFilterLastEditedToday");
                            if (!string.IsNullOrEmpty(VarText)) {
                                IndexConfig.LastEditedPast30Days = false;
                                IndexConfig.LastEditedPast7Days = false;
                                IndexConfig.LastEditedToday = GenericController.encodeBoolean(VarText);
                                IndexConfig.PageNumber = 1;
                            }
                        }
                    }
                    //
                    // Read IndexFilterOpen
                    //
                    VarText = core.docProperties.getText("IndexFilterOpen");
                    if (!string.IsNullOrEmpty(VarText)) {
                        IndexConfig.Open = GenericController.encodeBoolean(VarText);
                        IndexConfig.PageNumber = 1;
                    }
                    if (core.docProperties.getBoolean("IndexSortRemoveAll")) {
                        //
                        // Remove all filters
                        IndexConfig.Sorts = new Dictionary<string, IndexConfigClass.IndexConfigSortClass>();
                    } else {
                        //
                        // SortField
                        string setSortField = core.docProperties.getText("SetSortField").ToLower();
                        if (!string.IsNullOrEmpty(setSortField)) {
                            bool sortFound = IndexConfig.Sorts.ContainsKey(setSortField);
                            int sortDirection = core.docProperties.getInteger("SetSortDirection");
                            if (!sortFound) {
                                IndexConfig.Sorts.Add(setSortField, new IndexConfigClass.IndexConfigSortClass {
                                    fieldName = setSortField,
                                    direction = 1,
                                    order = IndexConfig.Sorts.Count + 1
                                });
                            } else if (sortDirection > 0) {
                                IndexConfig.Sorts[setSortField].direction = sortDirection;
                            } else {
                                IndexConfig.Sorts.Remove(setSortField);
                                int sortOrder = 1;
                                foreach (var kvp in IndexConfig.Sorts) {
                                    kvp.Value.order = sortOrder++;
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //   
        //========================================================================================
        //
        public static void setIndexSQL( CoreController core, AdminInfoDomainModel adminContext, IndexConfigClass IndexConfig, ref bool Return_AllowAccess, ref string return_sqlFieldList, ref string return_sqlFrom, ref string return_SQLWhere, ref string return_SQLOrderBy, ref bool return_IsLimitedToSubContent, ref string return_ContentAccessLimitMessage, ref Dictionary<string, bool> FieldUsedInColumns, Dictionary<string, bool> IsLookupFieldValid) {
            try {
                string LookupQuery = null;
                string ContentName = null;
                string SortFieldName = null;
                //
                int LookupPtr = 0;
                string[] lookups = null;
                string FindWordName = null;
                string FindWordValue = null;
                int FindMatchOption = 0;
                int WCount = 0;
                string SubContactList = "";
                int ContentID = 0;
                int Pos = 0;
                int Cnt = 0;
                string[] ListSplit = null;
                int SubContentCnt = 0;
                string list = null;
                string SubQuery = null;
                int GroupID = 0;
                string GroupName = null;
                string JoinTablename = null;
                //Dim FieldName As String
                int Ptr = 0;
                bool IncludedInLeftJoin = false;
                //  Dim SupportWorkflowFields As Boolean
                int FieldPtr = 0;
                bool IncludedInColumns = false;
                string LookupContentName = null;
                //Dim arrayOfFields() As appServices_metaDataClass.CDefFieldClass
                //
                Return_AllowAccess = true;
                //
                // ----- Workflow Fields
                //
                return_sqlFieldList = return_sqlFieldList + adminContext.adminContent.tableName + ".ID";
                //
                // ----- From Clause - build joins for Lookup fields in columns, in the findwords, and in sorts
                //
                return_sqlFrom = adminContext.adminContent.tableName;
                foreach (KeyValuePair<string, CDefFieldModel> keyValuePair in adminContext.adminContent.fields) {
                    CDefFieldModel field = keyValuePair.Value;
                    FieldPtr = field.id; // quick fix for a replacement for the old fieldPtr (so multiple for loops will always use the same "table"+ptr string
                    IncludedInColumns = false;
                    IncludedInLeftJoin = false;
                    if (!IsLookupFieldValid.ContainsKey(field.nameLc)) {
                        IsLookupFieldValid.Add(field.nameLc, false);
                    }
                    if (!FieldUsedInColumns.ContainsKey(field.nameLc)) {
                        FieldUsedInColumns.Add(field.nameLc, false);
                    }
                    //
                    // test if this field is one of the columns we are displaying
                    //
                    IncludedInColumns = (IndexConfig.columns.Find(x => (x.Name == field.nameLc)) != null);
                    //
                    // disallow IncludedInColumns if a non-supported field type
                    //
                    switch (field.fieldTypeId) {
                        case FieldTypeIdFileCSS:
                        case FieldTypeIdFile:
                        case FieldTypeIdFileImage:
                        case FieldTypeIdFileJavascript:
                        case FieldTypeIdLongText:
                        case FieldTypeIdManyToMany:
                        case FieldTypeIdRedirect:
                        case FieldTypeIdFileText:
                        case FieldTypeIdFileXML:
                        case FieldTypeIdHTML:
                        case FieldTypeIdFileHTML:
                            IncludedInColumns = false;
                            break;
                    }
                    //FieldName = genericController.vbLCase(.Name)
                    if ((field.fieldTypeId == FieldTypeIdMemberSelect) || ((field.fieldTypeId == FieldTypeIdLookup) && (field.lookupContentID != 0))) {
                        //
                        // This is a lookup field -- test if IncludedInLeftJoins
                        //
                        JoinTablename = "";
                        if (field.fieldTypeId == FieldTypeIdMemberSelect) {
                            LookupContentName = "people";
                        } else {
                            LookupContentName = CdefController.getContentNameByID(core, field.lookupContentID);
                        }
                        if (!string.IsNullOrEmpty(LookupContentName)) {
                            JoinTablename = CdefController.getContentTablename(core, LookupContentName);
                        }
                        IncludedInLeftJoin = IncludedInColumns;
                        if (IndexConfig.FindWords.Count > 0) {
                            //
                            // test findwords
                            //
                            if (IndexConfig.FindWords.ContainsKey(field.nameLc)) {
                                if (IndexConfig.FindWords[field.nameLc].MatchOption != FindWordMatchEnum.MatchIgnore) {
                                    IncludedInLeftJoin = true;
                                }
                            }
                        }
                        if ((!IncludedInLeftJoin) && IndexConfig.Sorts.Count > 0) {
                            //
                            // test sorts
                            //
                            if (IndexConfig.Sorts.ContainsKey(field.nameLc.ToLower())) {
                                IncludedInLeftJoin = true;
                            }
                        }
                        if (IncludedInLeftJoin) {
                            //
                            // include this lookup field
                            //
                            FieldUsedInColumns[field.nameLc] = true;
                            if (!string.IsNullOrEmpty(JoinTablename)) {
                                IsLookupFieldValid[field.nameLc] = true;
                                return_sqlFieldList = return_sqlFieldList + ", LookupTable" + FieldPtr + ".Name AS LookupTable" + FieldPtr + "Name";
                                return_sqlFrom = "(" + return_sqlFrom + " LEFT JOIN " + JoinTablename + " AS LookupTable" + FieldPtr + " ON " + adminContext.adminContent.tableName + "." + field.nameLc + " = LookupTable" + FieldPtr + ".ID)";
                            }
                            //End If
                        }
                    }
                    if (IncludedInColumns) {
                        //
                        // This field is included in the columns, so include it in the select
                        //
                        return_sqlFieldList = return_sqlFieldList + " ," + adminContext.adminContent.tableName + "." + field.nameLc;
                        FieldUsedInColumns[field.nameLc] = true;
                    }
                }
                //
                // Sub CDef filter
                //
                if (IndexConfig.SubCDefID > 0) {
                    ContentName = CdefController.getContentNameByID(core, IndexConfig.SubCDefID);
                    return_SQLWhere += "AND(" + CdefController.getContentControlCriteria(core, ContentName) + ")";
                }
                //
                // Return_sqlFrom and Where Clause for Groups filter
                //
                DateTime rightNow = DateTime.Now;
                string sqlRightNow = core.db.encodeSQLDate(rightNow);
                if (adminContext.adminContent.tableName.ToLower() == "ccmembers") {
                    if (IndexConfig.GroupListCnt > 0) {
                        for (Ptr = 0; Ptr < IndexConfig.GroupListCnt; Ptr++) {
                            GroupName = IndexConfig.GroupList[Ptr];
                            if (!string.IsNullOrEmpty(GroupName)) {
                                GroupID = core.db.getRecordID("Groups", GroupName);
                                if (GroupID == 0 && GroupName.IsNumeric()) {
                                    GroupID = GenericController.encodeInteger(GroupName);
                                }
                                string groupTableAlias = "GroupFilter" + Ptr;
                                return_SQLWhere += "AND(" + groupTableAlias + ".GroupID=" + GroupID + ")and((" + groupTableAlias + ".dateExpires is null)or(" + groupTableAlias + ".dateExpires>" + sqlRightNow + "))";
                                return_sqlFrom = "(" + return_sqlFrom + " INNER JOIN ccMemberRules AS GroupFilter" + Ptr + " ON GroupFilter" + Ptr + ".MemberID=ccMembers.ID)";
                                //Return_sqlFrom = "(" & Return_sqlFrom & " INNER JOIN ccMemberRules AS GroupFilter" & Ptr & " ON GroupFilter" & Ptr & ".MemberID=ccmembers.ID)"
                            }
                        }
                    }
                }
                //
                // Add Name into Return_sqlFieldList
                //
                //If Not SQLSelectIncludesName Then
                // SQLSelectIncludesName is declared, but not initialized
                return_sqlFieldList = return_sqlFieldList + " ," + adminContext.adminContent.tableName + ".Name";
                //End If
                //
                // paste sections together and do where clause
                //
                if (AdminInfoDomainModel.userHasContentAccess(core, adminContext.adminContent.id)) {
                    //
                    // This person can see all the records
                    //
                    return_SQLWhere += "AND(" + CdefController.getContentControlCriteria(core, adminContext.adminContent.name) + ")";
                } else {
                    //
                    // Limit the Query to what they can see
                    //
                    return_IsLimitedToSubContent = true;
                    SubQuery = "";
                    list = adminContext.adminContent.legacyContentControlCriteria;
                    adminContext.adminContent.id = adminContext.adminContent.id;
                    SubContentCnt = 0;
                    if (!string.IsNullOrEmpty(list)) {
                        Console.WriteLine("console - adminContext.adminContext.content.contentControlCriteria=" + list);
                        ////Debug.WriteLine("debug - adminContext.adminContext.content.contentControlCriteria=" + list);
                        LogController.logInfo(core, "appendlog - adminContext.adminContext.content.contentControlCriteria=" + list);
                        ListSplit = list.Split('=');
                        Cnt = ListSplit.GetUpperBound(0) + 1;
                        if (Cnt > 0) {
                            for (Ptr = 0; Ptr < Cnt; Ptr++) {
                                Pos = GenericController.vbInstr(1, ListSplit[Ptr], ")");
                                if (Pos > 0) {
                                    ContentID = GenericController.encodeInteger(ListSplit[Ptr].Left(Pos - 1));
                                    if (ContentID > 0 && (ContentID != adminContext.adminContent.id) & AdminInfoDomainModel.userHasContentAccess(core, ContentID)) {
                                        SubQuery = SubQuery + "OR(" + adminContext.adminContent.tableName + ".ContentControlID=" + ContentID + ")";
                                        return_ContentAccessLimitMessage = return_ContentAccessLimitMessage + ", '<a href=\"?cid=" + ContentID + "\">" + CdefController.getContentNameByID(core, ContentID) + "</a>'";
                                        SubContactList += "," + ContentID;
                                        SubContentCnt = SubContentCnt + 1;
                                    }
                                }
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(SubQuery)) {
                        //
                        // Person has no access
                        //
                        Return_AllowAccess = false;
                        return;
                    } else {
                        return_SQLWhere += "AND(" + SubQuery.Substring(2) + ")";
                        return_ContentAccessLimitMessage = "Your access to " + adminContext.adminContent.name + " is limited to Sub-content(s) " + return_ContentAccessLimitMessage.Substring(2);
                    }
                }
                //
                // Where Clause: Active Only
                //
                if (IndexConfig.ActiveOnly) {
                    return_SQLWhere += "AND(" + adminContext.adminContent.tableName + ".active<>0)";
                }
                //
                // Where Clause: edited by me
                //
                if (IndexConfig.LastEditedByMe) {
                    return_SQLWhere += "AND(" + adminContext.adminContent.tableName + ".ModifiedBy=" + core.session.user.id + ")";
                }
                //
                // Where Clause: edited today
                //
                if (IndexConfig.LastEditedToday) {
                    return_SQLWhere += "AND(" + adminContext.adminContent.tableName + ".ModifiedDate>=" + core.db.encodeSQLDate(core.doc.profileStartTime.Date) + ")";
                }
                //
                // Where Clause: edited past week
                //
                if (IndexConfig.LastEditedPast7Days) {
                    return_SQLWhere += "AND(" + adminContext.adminContent.tableName + ".ModifiedDate>=" + core.db.encodeSQLDate(core.doc.profileStartTime.Date.AddDays(-7)) + ")";
                }
                //
                // Where Clause: edited past month
                //
                if (IndexConfig.LastEditedPast30Days) {
                    return_SQLWhere += "AND(" + adminContext.adminContent.tableName + ".ModifiedDate>=" + core.db.encodeSQLDate(core.doc.profileStartTime.Date.AddDays(-30)) + ")";
                }
                //
                // Where Clause: Where Pairs
                //
                for (WCount = 0; WCount <= 9; WCount++) {
                    if (!string.IsNullOrEmpty(adminContext.WherePair[1, WCount])) {
                        //
                        // Verify that the fieldname called out is in this table
                        //
                        if (adminContext.adminContent.fields.Count > 0) {
                            foreach (KeyValuePair<string, CDefFieldModel> keyValuePair in adminContext.adminContent.fields) {
                                CDefFieldModel field = keyValuePair.Value;
                                if (GenericController.vbUCase(field.nameLc) == GenericController.vbUCase(adminContext.WherePair[0, WCount])) {
                                    //
                                    // found it, add it in the sql
                                    //
                                    return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + adminContext.WherePair[0, WCount] + "=";
                                    if (adminContext.WherePair[1, WCount].IsNumeric()) {
                                        return_SQLWhere += adminContext.WherePair[1, WCount] + ")";
                                    } else {
                                        return_SQLWhere += "'" + adminContext.WherePair[1, WCount] + "')";
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                //
                // Where Clause: findwords
                //
                if (IndexConfig.FindWords.Count > 0) {
                    foreach (var kvp in IndexConfig.FindWords) {
                        IndexConfigClass.IndexConfigFindWordClass findword = kvp.Value;
                        FindMatchOption = (int)findword.MatchOption;
                        if (FindMatchOption != (int)FindWordMatchEnum.MatchIgnore) {
                            FindWordName = GenericController.vbLCase(findword.Name);
                            FindWordValue = findword.Value;
                            //
                            // Get FieldType
                            //
                            if (adminContext.adminContent.fields.Count > 0) {
                                foreach (KeyValuePair<string, CDefFieldModel> keyValuePair in adminContext.adminContent.fields) {
                                    CDefFieldModel field = keyValuePair.Value;
                                    // quick fix for a replacement for the old fieldPtr (so multiple for loops will always use the same "table"+ptr string
                                    FieldPtr = field.id;
                                    if (GenericController.vbLCase(field.nameLc) == FindWordName) {
                                        switch (field.fieldTypeId) {
                                            case FieldTypeIdAutoIdIncrement:
                                            case FieldTypeIdInteger:
                                                //
                                                // integer
                                                //
                                                int FindWordValueInteger = GenericController.encodeInteger(FindWordValue);
                                                switch (FindMatchOption) {
                                                    case (int)FindWordMatchEnum.MatchEmpty:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + " is null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchNotEmpty:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + " is not null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchEquals:
                                                    case (int)FindWordMatchEnum.matchincludes:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + "=" + core.db.encodeSQLNumber(FindWordValueInteger) + ")";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchGreaterThan:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + ">" + core.db.encodeSQLNumber(FindWordValueInteger) + ")";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchLessThan:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + "<" + core.db.encodeSQLNumber(FindWordValueInteger) + ")";
                                                        break;
                                                }
                                                //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                //ORIGINAL LINE: Exit For
                                                goto ExitLabel1;

                                            case FieldTypeIdCurrency:
                                            case FieldTypeIdFloat:
                                                //
                                                // double
                                                //
                                                double FindWordValueDouble = GenericController.encodeNumber(FindWordValue);
                                                switch (FindMatchOption) {
                                                    case (int)FindWordMatchEnum.MatchEmpty:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + " is null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchNotEmpty:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + " is not null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchEquals:
                                                    case (int)FindWordMatchEnum.matchincludes:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + "=" + core.db.encodeSQLNumber(FindWordValueDouble) + ")";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchGreaterThan:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + ">" + core.db.encodeSQLNumber(FindWordValueDouble) + ")";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchLessThan:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + "<" + core.db.encodeSQLNumber(FindWordValueDouble) + ")";
                                                        break;
                                                }
                                                //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                //ORIGINAL LINE: Exit For
                                                goto ExitLabel1;
                                            case FieldTypeIdFile:
                                            case FieldTypeIdFileImage:
                                                //
                                                // Date
                                                //
                                                switch (FindMatchOption) {
                                                    case (int)FindWordMatchEnum.MatchEmpty:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + " is null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchNotEmpty:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + " is not null)";
                                                        break;
                                                }
                                                //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                //ORIGINAL LINE: Exit For
                                                goto ExitLabel1;
                                            case FieldTypeIdDate:
                                                //
                                                // Date
                                                //
                                                DateTime findDate = DateTime.MinValue;
                                                if (DateController.IsDate(FindWordValue)) {
                                                    findDate = DateTime.Parse(FindWordValue);
                                                }
                                                switch (FindMatchOption) {
                                                    case (int)FindWordMatchEnum.MatchEmpty:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + " is null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchNotEmpty:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + " is not null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchEquals:
                                                    case (int)FindWordMatchEnum.matchincludes:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + "=" + core.db.encodeSQLDate(findDate) + ")";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchGreaterThan:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + ">" + core.db.encodeSQLDate(findDate) + ")";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchLessThan:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + "<" + core.db.encodeSQLDate(findDate) + ")";
                                                        break;
                                                }
                                                //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                //ORIGINAL LINE: Exit For
                                                goto ExitLabel1;
                                            case FieldTypeIdLookup:
                                            case FieldTypeIdMemberSelect:
                                                //
                                                // Lookup
                                                //
                                                if (IsLookupFieldValid[field.nameLc]) {
                                                    //
                                                    // Content Lookup
                                                    //
                                                    switch (FindMatchOption) {
                                                        case (int)FindWordMatchEnum.MatchEmpty:
                                                            return_SQLWhere += "AND(LookupTable" + FieldPtr + ".ID is null)";
                                                            break;
                                                        case (int)FindWordMatchEnum.MatchNotEmpty:
                                                            return_SQLWhere += "AND(LookupTable" + FieldPtr + ".ID is not null)";
                                                            break;
                                                        case (int)FindWordMatchEnum.MatchEquals:
                                                            return_SQLWhere += "AND(LookupTable" + FieldPtr + ".Name=" + core.db.encodeSQLText(FindWordValue) + ")";
                                                            break;
                                                        case (int)FindWordMatchEnum.matchincludes:
                                                            return_SQLWhere += "AND(LookupTable" + FieldPtr + ".Name LIKE " + core.db.encodeSQLText("%" + FindWordValue + "%") + ")";
                                                            break;
                                                    }
                                                } else if (field.lookupList != "") {
                                                    //
                                                    // LookupList
                                                    //
                                                    switch (FindMatchOption) {
                                                        case (int)FindWordMatchEnum.MatchEmpty:
                                                            return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + " is null)";
                                                            break;
                                                        case (int)FindWordMatchEnum.MatchNotEmpty:
                                                            return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + " is not null)";
                                                            break;
                                                        case (int)FindWordMatchEnum.MatchEquals:
                                                        case (int)FindWordMatchEnum.matchincludes:
                                                            lookups = field.lookupList.Split(',');
                                                            LookupQuery = "";
                                                            for (LookupPtr = 0; LookupPtr <= lookups.GetUpperBound(0); LookupPtr++) {
                                                                if (!lookups[LookupPtr].Contains(FindWordValue)) {
                                                                    LookupQuery = LookupQuery + "OR(" + adminContext.adminContent.tableName + "." + FindWordName + "=" + core.db.encodeSQLNumber(LookupPtr + 1) + ")";
                                                                }
                                                                //if (genericController.vbInstr(1, lookups[LookupPtr], FindWordValue, 1) != 0) {
                                                                //    LookupQuery = LookupQuery + "OR(" + adminContext.adminContext.content.ContentTableName + "." + FindWordName + "=" + core.db.encodeSQLNumber(LookupPtr + 1) + ")";
                                                                //}
                                                            }
                                                            if (!string.IsNullOrEmpty(LookupQuery)) {
                                                                return_SQLWhere += "AND(" + LookupQuery.Substring(2) + ")";
                                                            }
                                                            break;
                                                    }
                                                }
                                                //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                //ORIGINAL LINE: Exit For
                                                goto ExitLabel1;
                                            case FieldTypeIdBoolean:
                                                //
                                                // Boolean
                                                //
                                                switch (FindMatchOption) {
                                                    case (int)FindWordMatchEnum.matchincludes:
                                                        if (GenericController.encodeBoolean(FindWordValue)) {
                                                            return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + "<>0)";
                                                        } else {
                                                            return_SQLWhere += "AND((" + adminContext.adminContent.tableName + "." + FindWordName + "=0)or(" + adminContext.adminContent.tableName + "." + FindWordName + " is null))";
                                                        }
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchTrue:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + "<>0)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchFalse:
                                                        return_SQLWhere += "AND((" + adminContext.adminContent.tableName + "." + FindWordName + "=0)or(" + adminContext.adminContent.tableName + "." + FindWordName + " is null))";
                                                        break;
                                                }
                                                //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                //ORIGINAL LINE: Exit For
                                                goto ExitLabel1;
                                            default:
                                                //
                                                // Text (and the rest)
                                                //
                                                switch (FindMatchOption) {
                                                    case (int)FindWordMatchEnum.MatchEmpty:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + " is null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchNotEmpty:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + " is not null)";
                                                        break;
                                                    case (int)FindWordMatchEnum.matchincludes:
                                                        FindWordValue = core.db.encodeSQLText(FindWordValue);
                                                        FindWordValue = FindWordValue.Substring(1, FindWordValue.Length - 2);
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + " LIKE '%" + FindWordValue + "%')";
                                                        break;
                                                    case (int)FindWordMatchEnum.MatchEquals:
                                                        return_SQLWhere += "AND(" + adminContext.adminContent.tableName + "." + FindWordName + "=" + core.db.encodeSQLText(FindWordValue) + ")";
                                                        break;
                                                }
                                                //todo  WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                //ORIGINAL LINE: Exit For
                                                goto ExitLabel1;
                                        }
                                        //break;
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
                //
                return_SQLOrderBy = "";
                string orderByDelim = " ";
                foreach (var kvp in IndexConfig.Sorts) {
                    IndexConfigClass.IndexConfigSortClass sort = kvp.Value;
                    SortFieldName = GenericController.vbLCase(sort.fieldName);
                    //
                    // Get FieldType
                    //
                    if (adminContext.adminContent.fields.ContainsKey(sort.fieldName)) {
                        var tempVar = adminContext.adminContent.fields[sort.fieldName];
                        FieldPtr = tempVar.id; // quick fix for a replacement for the old fieldPtr (so multiple for loops will always use the same "table"+ptr string
                        if ((tempVar.fieldTypeId == FieldTypeIdLookup) && IsLookupFieldValid[sort.fieldName]) {
                            return_SQLOrderBy += orderByDelim + "LookupTable" + FieldPtr + ".Name";
                        } else {
                            return_SQLOrderBy += orderByDelim + adminContext.adminContent.tableName + "." + SortFieldName;
                        }
                    }
                    if (sort.direction > 1) {
                        return_SQLOrderBy = return_SQLOrderBy + " Desc";
                    }
                    orderByDelim = ",";
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //==========================================================================================================================================
        /// <summary>
        /// Get index view filter content - remote method
        /// </summary>
        /// <param name="adminContext.content"></param>
        /// <returns></returns>
        public static string getForm_IndexFilterContent( CoreController core, AdminInfoDomainModel adminContext) {
            string returnContent = "";
            try {
                string TableName = null;
                string FieldCaption = null;
                string ContentName = null;
                int CS = 0;
                string SQL = null;
                string Caption = null;
                string Link = null;
                string RQS = null;
                string QS = null;
                int Ptr = 0;
                string SubFilterList = null;
                IndexConfigClass IndexConfig = null;
                string list = null;
                string[] ListSplit = null;
                int Cnt = 0;
                int Pos = 0;
                //
                IndexConfig = IndexConfigClass.get(core, adminContext);
                //
                ContentName = CdefController.getContentNameByID(core, adminContext.adminContent.id);
                RQS = "cid=" + adminContext.adminContent.id + "&af=1";
                //
                //-------------------------------------------------------------------------------------
                // Remove filters
                //-------------------------------------------------------------------------------------
                //
                if ((IndexConfig.SubCDefID > 0) || (IndexConfig.GroupListCnt != 0) | (IndexConfig.FindWords.Count != 0) | IndexConfig.ActiveOnly | IndexConfig.LastEditedByMe | IndexConfig.LastEditedToday | IndexConfig.LastEditedPast7Days | IndexConfig.LastEditedPast30Days) {
                    //
                    // Remove Filters
                    //
                    returnContent += "<div class=\"ccFilterHead\">Remove&nbsp;Filters</div>";
                    Link = "/" + core.appConfig.adminRoute + "?" + GenericController.modifyQueryString(RQS, "IndexFilterRemoveAll", "1");
                    returnContent += HtmlController.div(getIconDeleteLink(Link) + "&nbsp;Remove All", "ccFilterSubHead");
                    //
                    // Last Edited Edited by me
                    //
                    SubFilterList = "";
                    if (IndexConfig.LastEditedByMe) {
                        Link = "/" + core.appConfig.adminRoute + "?" + GenericController.modifyQueryString(RQS, "IndexFilterLastEditedByMe", 0.ToString(), true);
                        SubFilterList += HtmlController.div(getIconDeleteLink(Link) + "&nbsp;By&nbsp;Me", "ccFilterIndent ccFilterList");
                    }
                    if (IndexConfig.LastEditedToday) {
                        QS = RQS;
                        QS = GenericController.modifyQueryString(QS, "IndexFilterLastEditedToday", 0.ToString(), true);
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        SubFilterList += HtmlController.div(getIconDeleteLink(Link) + "&nbsp;Today", "ccFilterIndent ccFilterList");
                    }
                    if (IndexConfig.LastEditedPast7Days) {
                        QS = RQS;
                        QS = GenericController.modifyQueryString(QS, "IndexFilterLastEditedPast7Days", 0.ToString(), true);
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        SubFilterList += HtmlController.div(getIconDeleteLink(Link) + "&nbsp;Past Week", "ccFilterIndent ccFilterList");
                    }
                    if (IndexConfig.LastEditedPast30Days) {
                        QS = RQS;
                        QS = GenericController.modifyQueryString(QS, "IndexFilterLastEditedPast30Days", 0.ToString(), true);
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        SubFilterList += HtmlController.div(getIconDeleteLink(Link) + "&nbsp;Past 30 Days", "ccFilterIndent ccFilterList");
                    }
                    if (!string.IsNullOrEmpty(SubFilterList)) {
                        returnContent += "<div class=\"ccFilterSubHead\">Last&nbsp;Edited</div>" + SubFilterList;
                    }
                    //
                    // Sub Content definitions
                    //
                    string SubContentName = null;
                    SubFilterList = "";
                    if (IndexConfig.SubCDefID > 0) {
                        SubContentName = CdefController.getContentNameByID(core, IndexConfig.SubCDefID);
                        QS = RQS;
                        QS = GenericController.modifyQueryString(QS, "IndexFilterRemoveCDef", encodeText(IndexConfig.SubCDefID));
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        SubFilterList += HtmlController.div(getIconDeleteLink(Link) + "&nbsp;" + SubContentName + "", "ccFilterIndent");
                    }
                    if (!string.IsNullOrEmpty(SubFilterList)) {
                        returnContent += "<div class=\"ccFilterSubHead\">In Sub-content</div>" + SubFilterList;
                    }
                    //
                    // Group Filter List
                    //
                    string GroupName = null;
                    SubFilterList = "";
                    if (IndexConfig.GroupListCnt > 0) {
                        for (Ptr = 0; Ptr < IndexConfig.GroupListCnt; Ptr++) {
                            GroupName = IndexConfig.GroupList[Ptr];
                            if (IndexConfig.GroupList[Ptr] != "") {
                                if (GroupName.Length > 30) {
                                    GroupName = GroupName.Left(15) + "..." + GroupName.Substring(GroupName.Length - 15);
                                }
                                QS = RQS;
                                QS = GenericController.modifyQueryString(QS, "IndexFilterRemoveGroup", IndexConfig.GroupList[Ptr]);
                                Link = "/" + core.appConfig.adminRoute + "?" + QS;
                                SubFilterList += HtmlController.div(getIconDeleteLink(Link) + "&nbsp;" + GroupName + "", "ccFilterIndent");
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
                    if (IndexConfig.ActiveOnly) {
                        QS = RQS;
                        QS = GenericController.modifyQueryString(QS, "IndexFilterActiveOnly", 0.ToString(), true);
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        SubFilterList += HtmlController.div(getIconDeleteLink(Link) + "&nbsp;Active&nbsp;Only", "ccFilterIndent ccFilterList");
                    }
                    if (!string.IsNullOrEmpty(SubFilterList)) {
                        returnContent += "<div class=\"ccFilterSubHead\">Other</div>" + SubFilterList;
                    }
                    //
                    // FindWords
                    //
                    foreach (var findWordKvp in IndexConfig.FindWords) {
                        IndexConfigClass.IndexConfigFindWordClass findWord = findWordKvp.Value;
                        FieldCaption = GenericController.encodeText(CdefController.getContentFieldProperty(core, ContentName, findWord.Name, "caption"));
                        QS = RQS;
                        QS = GenericController.modifyQueryString(QS, "IndexFilterRemoveFind", findWord.Name);
                        Link = "/" + core.appConfig.adminRoute + "?" + QS;
                        switch (findWord.MatchOption) {
                            case FindWordMatchEnum.matchincludes:
                                returnContent += HtmlController.div(getIconDeleteLink(Link) + "&nbsp;" + FieldCaption + "&nbsp;includes&nbsp;'" + findWord.Value + "'", "ccFilterIndent");
                                break;
                            case FindWordMatchEnum.MatchEmpty:
                                returnContent += HtmlController.div(getIconDeleteLink(Link) + "&nbsp;" + FieldCaption + "&nbsp;is&nbsp;empty", "ccFilterIndent");
                                break;
                            case FindWordMatchEnum.MatchEquals:
                                returnContent += HtmlController.div(getIconDeleteLink(Link) + "&nbsp;" + FieldCaption + "&nbsp;=&nbsp;'" + findWord.Value + "'", "ccFilterIndent");
                                break;
                            case FindWordMatchEnum.MatchFalse:
                                returnContent += HtmlController.div(getIconDeleteLink(Link) + "&nbsp;" + FieldCaption + "&nbsp;is&nbsp;false", "ccFilterIndent");
                                break;
                            case FindWordMatchEnum.MatchGreaterThan:
                                returnContent += HtmlController.div(getIconDeleteLink(Link) + "&nbsp;" + FieldCaption + "&nbsp;&gt;&nbsp;'" + findWord.Value + "'", "ccFilterIndent");
                                break;
                            case FindWordMatchEnum.MatchLessThan:
                                returnContent += HtmlController.div(getIconDeleteLink(Link) + "&nbsp;" + FieldCaption + "&nbsp;&lt;&nbsp;'" + findWord.Value + "'", "ccFilterIndent");
                                break;
                            case FindWordMatchEnum.MatchNotEmpty:
                                returnContent += HtmlController.div(getIconDeleteLink(Link) + "&nbsp;" + FieldCaption + "&nbsp;is&nbsp;not&nbsp;empty", "ccFilterIndent");
                                break;
                            case FindWordMatchEnum.MatchTrue:
                                returnContent += HtmlController.div(getIconDeleteLink(Link) + "&nbsp;" + FieldCaption + "&nbsp;is&nbsp;true", "ccFilterIndent");
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
                if (!IndexConfig.LastEditedByMe) {
                    QS = RQS;
                    QS = GenericController.modifyQueryString(QS, "IndexFilterLastEditedByMe", "1", true);
                    Link = "/" + core.appConfig.adminRoute + "?" + QS;
                    SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">By&nbsp;Me</a></div>";
                }
                if (!IndexConfig.LastEditedToday) {
                    QS = RQS;
                    QS = GenericController.modifyQueryString(QS, "IndexFilterLastEditedToday", "1", true);
                    Link = "/" + core.appConfig.adminRoute + "?" + QS;
                    SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Today</a></div>";
                }
                if (!IndexConfig.LastEditedPast7Days) {
                    QS = RQS;
                    QS = GenericController.modifyQueryString(QS, "IndexFilterLastEditedPast7Days", "1", true);
                    Link = "/" + core.appConfig.adminRoute + "?" + QS;
                    SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Past Week</a></div>";
                }
                if (!IndexConfig.LastEditedPast30Days) {
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
                var contentList = ContentModel.createList(core, "(contenttableid in (select id from cctables where name=" + core.db.encodeSQLText(adminContext.adminContent.tableName) + "))");
                if (contentList.Count>1) {
                    foreach ( var subContent in contentList ) {
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
                TableName = CdefController.getContentTablename(core, ContentName);
                SubFilterList = "";
                if (GenericController.vbLCase(TableName) == GenericController.vbLCase("ccMembers")) {
                    SQL = core.db.getSQLSelect("default", "ccGroups", "ID,Caption,Name", "(active<>0)", "Caption,Name");
                    CS = core.db.csOpenSql(SQL, "Default");
                    while (core.db.csOk(CS)) {
                        string Name = core.db.csGetText(CS, "Name");
                        Ptr = 0;
                        if (IndexConfig.GroupListCnt > 0) {
                            for (Ptr = 0; Ptr < IndexConfig.GroupListCnt; Ptr++) {
                                if (Name == IndexConfig.GroupList[Ptr]) {
                                    break;
                                }
                            }
                        }
                        if (Ptr == IndexConfig.GroupListCnt) {
                            int RecordID = core.db.csGetInteger(CS, "ID");
                            Caption = core.db.csGetText(CS, "Caption");
                            if (string.IsNullOrEmpty(Caption)) {
                                Caption = Name;
                                if (string.IsNullOrEmpty(Caption)) {
                                    Caption = "Group " + RecordID;
                                }
                            }
                            if (Caption.Length > 30) {
                                Caption = Caption.Left(15) + "..." + Caption.Substring(Caption.Length - 15);
                            }
                            Caption = "<span style=\"white-space:nowrap;\">" + Caption + "</span>";
                            QS = RQS;
                            if (!string.IsNullOrEmpty(Name.Trim(' '))) {
                                QS = GenericController.modifyQueryString(QS, "IndexFilterAddGroup", Name, true);
                            } else {
                                QS = GenericController.modifyQueryString(QS, "IndexFilterAddGroup", RecordID.ToString(), true);
                            }
                            Link = "/" + core.appConfig.adminRoute + "?" + QS;
                            SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">" + Caption + "</a></div>";
                        }
                        core.db.csGoNext(CS);
                    }
                }
                if (!string.IsNullOrEmpty(SubFilterList)) {
                    returnContent += "<div class=\"ccFilterSubHead\">In Group(s)</div>" + SubFilterList;
                }
                //
                // Active Only
                //
                SubFilterList = "";
                if (!IndexConfig.ActiveOnly) {
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
                LogController.handleError(core, ex);
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
        /// <param name="adminContext"></param>
        /// <param name="fieldName"></param>
        /// <param name="CS"></param>
        /// <param name="IsLookupFieldValid"></param>
        /// <param name="IsEmailContent"></param>
        /// <returns></returns>
        public static string getForm_Index_GetCell( CoreController core, AdminInfoDomainModel adminContext, string fieldName, int CS, bool IsLookupFieldValid, bool IsEmailContent) {
            string return_formIndexCell = "";
            try {
                string FieldText = null;
                string Filename = null;
                string Copy = null;
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                string[] lookups = null;
                int LookupPtr = 0;
                int Pos = 0;
                int lookupTableCnt = 0;
                //
                var tempVar = adminContext.adminContent.fields[fieldName.ToLower()];
                lookupTableCnt = tempVar.id; // workaround for universally creating the left join tablename for each field
                switch (tempVar.fieldTypeId) {
                    //Case FieldTypeImage
                    //    Stream.Add( Mid(core.app.cs_get(CS, .Name), 7 + Len(.Name) + Len(adminContext.content.ContentTableName)))
                    case FieldTypeIdFile:
                    case FieldTypeIdFileImage:
                        Filename = core.db.csGet(CS, tempVar.nameLc);
                        Filename = GenericController.vbReplace(Filename, "\\", "/");
                        Pos = Filename.LastIndexOf("/") + 1;
                        if (Pos != 0) {
                            Filename = Filename.Substring(Pos);
                        }
                        Stream.Add(Filename);
                        break;
                    case FieldTypeIdLookup:
                        if (IsLookupFieldValid) {
                            Stream.Add(core.db.csGet(CS, "LookupTable" + lookupTableCnt + "Name"));
                            lookupTableCnt += 1;
                        } else if (tempVar.lookupList != "") {
                            lookups = tempVar.lookupList.Split(',');
                            LookupPtr = core.db.csGetInteger(CS, tempVar.nameLc) - 1;
                            if (LookupPtr <= lookups.GetUpperBound(0)) {
                                if (LookupPtr < 0) {
                                    //Stream.Add( "-1")
                                } else {
                                    Stream.Add(lookups[LookupPtr]);
                                }
                            } else {
                                //Stream.Add( "-2")
                            }

                        } else {
                            //Stream.Add( "-3")
                            Stream.Add(" ");
                        }
                        break;
                    case FieldTypeIdMemberSelect:
                        if (IsLookupFieldValid) {
                            Stream.Add(core.db.csGet(CS, "LookupTable" + lookupTableCnt + "Name"));
                            lookupTableCnt += 1;
                        } else {
                            Stream.Add(core.db.csGet(CS, tempVar.nameLc));
                        }
                        break;
                    case FieldTypeIdBoolean:
                        if (core.db.csGetBoolean(CS, tempVar.nameLc)) {
                            Stream.Add("yes");
                        } else {
                            Stream.Add("no");
                        }
                        break;
                    case FieldTypeIdCurrency:
                        Stream.Add(string.Format("{0:C}", core.db.csGetNumber(CS, tempVar.nameLc)));
                        break;
                    case FieldTypeIdLongText:
                    case FieldTypeIdHTML:
                        FieldText = core.db.csGet(CS, tempVar.nameLc);
                        if (FieldText.Length > 50) {
                            FieldText = FieldText.Left(50) + "[more]";
                        }
                        Stream.Add(FieldText);
                        break;
                    case FieldTypeIdFileText:
                    case FieldTypeIdFileCSS:
                    case FieldTypeIdFileXML:
                    case FieldTypeIdFileJavascript:
                    case FieldTypeIdFileHTML:
                        // rw( "n/a" )
                        Filename = core.db.csGet(CS, tempVar.nameLc);
                        if (!string.IsNullOrEmpty(Filename)) {
                            Copy = core.cdnFiles.readFileText(Filename);
                            // 20171103 - dont see why this is being converted, not html
                            //Copy = core.html.convertActiveContent_internal(Copy, core.doc.authContext.user.id, "", 0, 0, True, False, False, False, True, False, "", "", IsEmailContent, 0, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, core.doc.authContext.isAuthenticated, Nothing, core.doc.authContext.isEditingAnything())
                            Stream.Add(Copy);
                        }
                        break;
                    case FieldTypeIdRedirect:
                    case FieldTypeIdManyToMany:
                        Stream.Add("n/a");
                        break;
                    default:
                        if (tempVar.password) {
                            Stream.Add("****");
                        } else {
                            Stream.Add(core.db.csGet(CS, tempVar.nameLc));
                        }
                        break;
                }
                //
                return_formIndexCell = HtmlController.encodeHtml(Stream.Text);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return return_formIndexCell;
        }
        //
    }
}
