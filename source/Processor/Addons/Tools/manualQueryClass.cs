
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Addons.AdminSite.Controllers;
using Contensive.Models.Db;
//
namespace Contensive.Processor.Addons.Tools {
    //
    public class ManualQueryClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// addon method, deliver complete Html admin site
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cpBase) {
            return get((CPClass)cpBase);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Run manual query
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public static string get(CPClass cp) {
            string returnHtml = "";
            CoreController core = cp.core;
            try {
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                Stream.add(AdminUIController.getHeaderTitleDescription("Run Manual Query", "This tool runs an SQL statement on a selected datasource. If there is a result set, the set is printed in a table."));
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
                DataSourceModel datasource = DataSourceModel.create(core.cpParent, core.docProperties.getInteger("dataSourceid"));
                //
                if ((core.docProperties.getText("button")) == ButtonRun) {
                    //
                    // Add this SQL to the members SQL list
                    //
                    if (!string.IsNullOrEmpty(SQL)) {
                        string SQLArchiveOld = SQLArchive.Replace(SQL + Environment.NewLine, "");
                        SQLArchive = SQL.Replace( Environment.NewLine, " ") + Environment.NewLine;
                        int LineCounter = 0;
                        while ((LineCounter < 10) && (!string.IsNullOrEmpty(SQLArchiveOld))) {
                            string line = getLine(ref SQLArchiveOld).Trim();
                            if (!string.IsNullOrWhiteSpace(line)) {
                                SQLArchive += line + Environment.NewLine;
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
                        Stream.add("<br>" + DateTime.Now + " SQL NOT executed. The following errors were detected before execution");
                        Stream.add(errBefore);
                    } else {
                        Stream.add("<p>" + DateTime.Now + " Executing sql [" + SQL + "] on DataSource [" + datasource.name + "]");
                        DataTable dt = null;
                        try {
                            dt = core.db.executeQuery(SQL, PageSize * (PageNumber - 1), PageSize);
                        } catch (Exception ex) {
                            //
                            // ----- error
                            Stream.add("<br>" + DateTime.Now + " SQL execution returned the following error");
                            Stream.add("<br>" + ex.Message);
                        }
                        string errSql = ErrorController.getDocExceptionHtmlList(core);
                        if (!string.IsNullOrWhiteSpace(errSql)) {
                            Stream.add("<br>" + DateTime.Now + " SQL execution returned the following error");
                            Stream.add("<br>" + errSql);
                            core.doc.errorList.Clear();
                        } else {
                            Stream.add("<br>" + DateTime.Now + " SQL executed successfully");
                            if (dt == null) {
                                Stream.add("<br>" + DateTime.Now + " SQL returned invalid data.");
                            } else if (dt.Rows == null) {
                                Stream.add("<br>" + DateTime.Now + " SQL returned invalid data rows.");
                            } else if (dt.Rows.Count == 0) {
                                Stream.add("<br>" + DateTime.Now + " The SQL returned no data.");
                            } else {
                                //
                                // ----- print results
                                //
                                Stream.add("<br>" + DateTime.Now + " The following results were returned");
                                Stream.add("<br></p>");
                                //
                                // --- Create the Fields for the new table
                                //
                                int FieldCount = dt.Columns.Count;
                                Stream.add("<table class=\"table table-bordered table-hover table-sm table-striped\">");
                                Stream.add("<thead class=\"thead - inverse\"><tr>");
                                foreach (DataColumn dc in dt.Columns) Stream.add("<th>" + dc.ColumnName + "</th>");
                                Stream.add("</tr></thead>");
                                //
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
                                    Stream.add(RowStart);
                                    int ColumnPointer = 0;
                                    for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                        string CellData = resultArray[ColumnPointer, RowPointer];
                                        if (isNull(CellData)) {
                                            Stream.add(ColumnStart + "[null]" + ColumnEnd);
                                        } else if (string.IsNullOrEmpty(CellData)) {
                                            Stream.add(ColumnStart + "[empty]" + ColumnEnd);
                                        } else {
                                            Stream.add(ColumnStart + HtmlController.encodeHtml(GenericController.encodeText(CellData)) + ColumnEnd);
                                        }
                                    }
                                    Stream.add(RowEnd);
                                }
                                Stream.add("</table>");
                            }
                        }
                    }
                    Stream.add("<p>" + DateTime.Now + " Done</p>");
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
                    Stream.add(AdminUIController.getDefaultEditor_TextArea(core, "SQL", SQL, false, "SQL"));
                    Stream.add("&nbsp;<INPUT TYPE=\"Text\" TabIndex=-1 NAME=\"SQLRows\" SIZE=\"3\" VALUE=\"" + SQLRows + "\" ID=\"\"  onchange=\"SQL.rows=SQLRows.value; return true\"> Rows");
                }
                //
                // -- data source
                bool isEmptyList = false;
                Stream.add(AdminUIController.getToolFormInputRow(core, "Data Source", AdminUIController.getDefaultEditor_lookupContent(core, "DataSourceID", datasource.id, ContentMetadataModel.getContentId(core, "data sources"), ref isEmptyList)));
                {
                    //
                    // -- sql list
                    var lookupList = new List<NameValueModel> { };
                    string[] delimiters = new string[] { Environment.NewLine };
                    List<string> SqlArchiveList = SQLArchive.Split( delimiters,StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach( string sql in SqlArchiveList) {
                        lookupList.Add(new NameValueModel { name = sql, value=sql });
                    }

                    string inputSelect = AdminUIController.getDefaultEditor_LookupList(core, "SQLList", "0" , lookupList,false, "SQLList");
                    inputSelect = inputSelect.Replace("<select ", "<select onChange=\"SQL.value=SQLList.value\" ");
                    Stream.add(AdminUIController.getToolFormInputRow(core, "Previous Queries", inputSelect));
                }
                //
                // -- page size
                if (isNull(PageSize)) PageSize = 100;
                Stream.add(AdminUIController.getToolFormInputRow(core, "Page Size", AdminUIController.getDefaultEditor_text(core, "PageSize", PageSize.ToString())));
                //
                // -- page number
                if (isNull(PageNumber)) PageNumber = 1;
                Stream.add(AdminUIController.getToolFormInputRow(core, "Page Number", AdminUIController.getDefaultEditor_text(core, "PageNumber", PageNumber.ToString())));
                //
                // -- timeout
                if (isNull(Timeout)) Timeout = 30;
                Stream.add(AdminUIController.getToolFormInputRow(core, "Timeout (sec)", AdminUIController.getDefaultEditor_text(core, "Timeout", Timeout.ToString())));
                //
                // -- assemble form
                returnHtml = AdminUIController.getToolForm(core, Stream.text, ButtonCancel + "," + ButtonRun);
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return returnHtml;
        }
    }
}

