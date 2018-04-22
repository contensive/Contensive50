
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Addons.Tools {
    //
    public class manualQueryClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// addon method, deliver complete Html admin site
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cpBase) {
            return GetForm_ManualQuery((CPClass)cpBase);
        }
        //
        //=============================================================================
        //   Print the manual query form
        //=============================================================================
        //
        public static string GetForm_ManualQuery(CPClass cp) {
            string returnHtml = "";
            coreController core = cp.core;
            try {
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                Stream.Add(adminUIController.getToolFormTitle("Run Manual Query", "This tool runs an SQL statement on a selected datasource. If there is a result set, the set is printed in a table."));
                //
                // Get the members SQL Queue
                //
                string SQLFilename = core.userProperty.getText("SQLArchive");
                if (string.IsNullOrEmpty(SQLFilename)) {
                    SQLFilename = "SQLArchive" + core.session.user.id.ToString("000000000") + ".txt";
                    core.userProperty.setProperty("SQLArchive", SQLFilename);
                }
                string SQLArchive = core.cdnFiles.readFileText(SQLFilename);
                //
                // Read in arguments if available
                //
                int Timeout = core.docProperties.getInteger("Timeout");
                if (Timeout == 0) {
                    Timeout = 30;
                }
                //
                int PageSize = core.docProperties.getInteger("PageSize");
                if (PageSize == 0) {
                    PageSize = 10;
                }
                //
                int PageNumber = core.docProperties.getInteger("PageNumber");
                if (PageNumber == 0) {
                    PageNumber = 1;
                }
                //
                string SQL = core.docProperties.getText("SQL");
                if (string.IsNullOrEmpty(SQL)) {
                    SQL = core.docProperties.getText("SQLList");
                }
                dataSourceModel datasource = dataSourceModel.create(core, core.docProperties.getInteger("dataSourceid"));
                //
                if ((core.docProperties.getText("button")) == ButtonRun) {
                    //
                    // Add this SQL to the members SQL list
                    //
                    if (!string.IsNullOrEmpty(SQL)) {
                        string SQLArchiveOld = SQLArchive.Replace(SQL + "\r\n", "");
                        SQLArchive = SQL.Replace( "\r\n", " ") + "\r\n";
                        int LineCounter = 0;
                        while ((LineCounter < 10) && (!string.IsNullOrEmpty(SQLArchiveOld))) {
                            string line = getLine(ref SQLArchiveOld).Trim();
                            if (!string.IsNullOrWhiteSpace(line)) {
                                SQLArchive += line + "\r\n";
                            }
                        }
                        core.cdnFiles.saveFile(SQLFilename, SQLArchive);
                    }
                    //
                    // Run the SQL
                    //
                    string errBefore = errorController.getDocExceptionHtmlList(core);
                    if (!string.IsNullOrWhiteSpace(errBefore)) {
                        // -- error in interface, should be fixed before attempting query
                        Stream.Add("<br>" + DateTime.Now + " SQL NOT executed. The following errors were detected before execution");
                        Stream.Add(errBefore);
                    } else {
                        Stream.Add("<p>" + DateTime.Now + " Executing sql [" + SQL + "] on DataSource [" + datasource.Name + "]");
                        DataTable dt = null;
                        try {
                            dt = core.db.executeQuery(SQL, datasource.Name, PageSize * (PageNumber - 1), PageSize);
                        } catch (Exception ex) {
                            //
                            // ----- error
                            Stream.Add("<br>" + DateTime.Now + " SQL execution returned the following error");
                            Stream.Add("<br>" + ex.Message);
                        }
                        string errSql = errorController.getDocExceptionHtmlList(core);
                        if (!string.IsNullOrWhiteSpace(errSql)) {
                            Stream.Add("<br>" + DateTime.Now + " SQL execution returned the following error");
                            Stream.Add("<br>" + errSql);
                            core.doc.errList.Clear();
                        } else {
                            Stream.Add("<br>" + DateTime.Now + " SQL executed successfully");
                            if (dt == null) {
                                Stream.Add("<br>" + DateTime.Now + " SQL returned invalid data.");
                            } else if (dt.Rows == null) {
                                Stream.Add("<br>" + DateTime.Now + " SQL returned invalid data rows.");
                            } else if (dt.Rows.Count == 0) {
                                Stream.Add("<br>" + DateTime.Now + " The SQL returned no data.");
                            } else {
                                //
                                // ----- print results
                                //
                                Stream.Add("<br>" + DateTime.Now + " The following results were returned");
                                Stream.Add("<br></p>");
                                //
                                // --- Create the Fields for the new table
                                //
                                int FieldCount = dt.Columns.Count;
                                Stream.Add("<table class=\"table table-bordered table-hover table-sm table-striped\">");
                                Stream.Add("<thead class=\"thead - inverse\"><tr>");
                                foreach (DataColumn dc in dt.Columns) Stream.Add("<th>" + dc.ColumnName + "</th>");
                                Stream.Add("</tr></thead>");
                                //
                                //Dim dtOK As Boolean
                                string[,] resultArray = core.db.convertDataTabletoArray(dt);
                                //
                                int RowMax = resultArray.GetUpperBound(1);
                                int ColumnMax = resultArray.GetUpperBound(0);
                                string RowStart = "<tr>";
                                string RowEnd = "</tr>";
                                string ColumnStart = "<td>";
                                string ColumnEnd = "</td>";
                                int RowPointer = 0;
                                for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                                    Stream.Add(RowStart);
                                    int ColumnPointer = 0;
                                    for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                        string CellData = resultArray[ColumnPointer, RowPointer];
                                        if (IsNull(CellData)) {
                                            Stream.Add(ColumnStart + "[null]" + ColumnEnd);
                                            //ElseIf IsEmpty(CellData) Then
                                            //    Stream.Add(ColumnStart & "[empty]" & ColumnEnd)
                                            //ElseIf IsArray(CellData) Then
                                            //    Stream.Add(ColumnStart & "[array]")
                                            //    Cnt = UBound(CellData)
                                            //    For Ptr = 0 To Cnt - 1
                                            //        Stream.Add("<br>(" & Ptr & ")&nbsp;[" & CellData[Ptr] & "]")
                                            //    Next
                                            //    Stream.Add(ColumnEnd)
                                        } else if (string.IsNullOrEmpty(CellData)) {
                                            Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
                                        } else {
                                            Stream.Add(ColumnStart + htmlController.encodeHtml(genericController.encodeText(CellData)) + ColumnEnd);
                                        }
                                    }
                                    Stream.Add(RowEnd);
                                }
                                Stream.Add("</table>");
                            }
                        }
                    }
                    Stream.Add("<p>" + DateTime.Now + " Done</p>");
                }
                //
                // Display form
                {
                    //
                    // -- sql form
                    int SQLRows = core.docProperties.getInteger("SQLRows");
                    if (SQLRows == 0) {
                        SQLRows = core.userProperty.getInteger("ManualQueryInputRows", 5);
                    } else {
                        core.userProperty.setProperty("ManualQueryInputRows", SQLRows.ToString());
                    }
                    Stream.Add(adminUIController.getDefaultEditor_TextArea(core, "SQL", SQL, false, "SQL"));
                    Stream.Add("&nbsp;<INPUT TYPE=\"Text\" TabIndex=-1 NAME=\"SQLRows\" SIZE=\"3\" VALUE=\"" + SQLRows + "\" ID=\"\"  onchange=\"SQL.rows=SQLRows.value; return true\"> Rows");
                }
                //
                // -- data source
                bool isEmptyList = false;
                Stream.Add(adminUIController.getToolFormInputRow(core, "Data Source", adminUIController.getDefaultEditor_LookupContent(core, "DataSourceID", datasource.ID, Models.Complex.cdefModel.getContentId(core, "data sources"), ref isEmptyList)));
                {
                    //
                    // -- sql list
                    var lookupList = new List<nameValueClass> { };
                    string[] delimiters = new string[] { "\r\n" };
                    List<string> SqlArchiveList = SQLArchive.Split( delimiters,StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach( string sql in SqlArchiveList) {
                        lookupList.Add(new nameValueClass() { name = sql, value=sql });
                    }

                    string inputSelect = adminUIController.getDefaultEditor_LookupList(core, "SQLList", "0" , lookupList,false, "SQLList");
                    inputSelect = inputSelect.Replace("<select ", "<select onChange=\"SQL.value=SQLList.value\" ");
                    Stream.Add(adminUIController.getToolFormInputRow(core, "Previous Queries", inputSelect));
                }
                //
                // -- page size
                if (IsNull(PageSize)) PageSize = 100;
                Stream.Add(adminUIController.getToolFormInputRow(core, "Page Size", adminUIController.getDefaultEditor_Text(core, "PageSize", PageSize.ToString())));
                //
                // -- page number
                if (IsNull(PageNumber)) PageNumber = 1;
                Stream.Add(adminUIController.getToolFormInputRow(core, "Page Number", adminUIController.getDefaultEditor_Text(core, "PageNumber", PageNumber.ToString())));
                //
                // -- timeout
                if (IsNull(Timeout)) Timeout = 30;
                Stream.Add(adminUIController.getToolFormInputRow(core, "Timeout (sec)", adminUIController.getDefaultEditor_Text(core, "Timeout", Timeout.ToString())));
                //
                // -- assemble form
                returnHtml = adminUIController.getToolForm(core, Stream.Text, ButtonCancel + "," + ButtonRun);
            } catch (Exception ex) {
                logController.handleError(core, ex);
                throw;
            }
            return returnHtml;
        }
    }
}

