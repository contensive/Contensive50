
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
//
namespace Contensive.Addons.Tools {
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
            CoreController core = cp.core;
            try {
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                Stream.Add(AdminUIController.getToolFormTitle("Run Manual Query", "This tool runs an SQL statement on a selected datasource. If there is a result set, the set is printed in a table."));
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
                DataSourceModel datasource = DataSourceModel.create(core, core.docProperties.getInteger("dataSourceid"));
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
                    string errBefore = ErrorController.getDocExceptionHtmlList(core);
                    if (!string.IsNullOrWhiteSpace(errBefore)) {
                        // -- error in interface, should be fixed before attempting query
                        Stream.Add("<br>" + DateTime.Now + " SQL NOT executed. The following errors were detected before execution");
                        Stream.Add(errBefore);
                    } else {
                        Stream.Add("<p>" + DateTime.Now + " Executing sql [" + SQL + "] on DataSource [" + datasource.name + "]");
                        DataTable dt = null;
                        try {
                            dt = core.db.executeQuery(SQL, datasource.name, PageSize * (PageNumber - 1), PageSize);
                        } catch (Exception ex) {
                            //
                            // ----- error
                            Stream.Add("<br>" + DateTime.Now + " SQL execution returned the following error");
                            Stream.Add("<br>" + ex.Message);
                        }
                        string errSql = ErrorController.getDocExceptionHtmlList(core);
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
                                            Stream.Add(ColumnStart + HtmlController.encodeHtml(GenericController.encodeText(CellData)) + ColumnEnd);
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
                    Stream.Add(AdminUIController.getDefaultEditor_TextArea(core, "SQL", SQL, false, "SQL"));
                    Stream.Add("&nbsp;<INPUT TYPE=\"Text\" TabIndex=-1 NAME=\"SQLRows\" SIZE=\"3\" VALUE=\"" + SQLRows + "\" ID=\"\"  onchange=\"SQL.rows=SQLRows.value; return true\"> Rows");
                }
                //
                // -- data source
                bool isEmptyList = false;
                Stream.Add(AdminUIController.getToolFormInputRow(core, "Data Source", AdminUIController.getDefaultEditor_LookupContent(core, "DataSourceID", datasource.id, Processor.Models.Domain.CDefModel.getContentId(core, "data sources"), ref isEmptyList)));
                {
                    //
                    // -- sql list
                    var lookupList = new List<nameValueClass> { };
                    string[] delimiters = new string[] { "\r\n" };
                    List<string> SqlArchiveList = SQLArchive.Split( delimiters,StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach( string sql in SqlArchiveList) {
                        lookupList.Add(new nameValueClass() { name = sql, value=sql });
                    }

                    string inputSelect = AdminUIController.getDefaultEditor_LookupList(core, "SQLList", "0" , lookupList,false, "SQLList");
                    inputSelect = inputSelect.Replace("<select ", "<select onChange=\"SQL.value=SQLList.value\" ");
                    Stream.Add(AdminUIController.getToolFormInputRow(core, "Previous Queries", inputSelect));
                }
                //
                // -- page size
                if (IsNull(PageSize)) PageSize = 100;
                Stream.Add(AdminUIController.getToolFormInputRow(core, "Page Size", AdminUIController.getDefaultEditor_Text(core, "PageSize", PageSize.ToString())));
                //
                // -- page number
                if (IsNull(PageNumber)) PageNumber = 1;
                Stream.Add(AdminUIController.getToolFormInputRow(core, "Page Number", AdminUIController.getDefaultEditor_Text(core, "PageNumber", PageNumber.ToString())));
                //
                // -- timeout
                if (IsNull(Timeout)) Timeout = 30;
                Stream.Add(AdminUIController.getToolFormInputRow(core, "Timeout (sec)", AdminUIController.getDefaultEditor_Text(core, "Timeout", Timeout.ToString())));
                //
                // -- assemble form
                returnHtml = AdminUIController.getToolForm(core, Stream.Text, ButtonCancel + "," + ButtonRun);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnHtml;
        }
    }
}

