
using System;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Addons.AdminSite.Controllers;
using System.Text;
using System.Collections.Generic;
using Contensive.Models.Db;
using System.Data;
//
namespace Contensive.Processor.Addons.Tools {
    //
    public class DbSchemaToolClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// addon method, deliver complete Html admin site
        /// blank return on OK or cancel button
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cpBase) {
            return get(((CPClass)cpBase).core);
        }
        //
        //=============================================================================
        //   Print the manual query form
        //=============================================================================
        //
        public static string get(CoreController core) {
            string result = "";
            try {
                bool StatusOK = false;
                int FieldCount = 0;
                object Retries = null;
                int RowMax = 0;
                int RowPointer = 0;
                int ColumnMax = 0;
                int ColumnPointer = 0;
                string ColumnStart = null;
                string ColumnEnd = null;
                string RowStart = null;
                string RowEnd = null;
                string[,] arrayOfSchema = null;
                string CellData = null;
                string TableName = "";
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                string ButtonList = null;
                DataTable RSSchema = null;
                var tmpList = new List<string> { };
                DataSourceModel datasource = DataSourceModel.create(core.cpParent, core.docProperties.getInteger("DataSourceID"), ref tmpList);
                //
                ButtonList = ButtonCancel + "," + ButtonRun;
                //
                Stream.Add(AdminUIController.getHeaderTitleDescription("Query Database Schema", "This tool examines the database schema for all tables available."));
                //
                StatusOK = true;
                if ((core.docProperties.getText("button")) != ButtonRun) {
                    //
                    // First pass, initialize
                    //
                    Retries = 0;
                } else {
                    //
                    // Read in arguments
                    //
                    TableName = core.docProperties.getText("TableName");
                    //
                    // Run the SQL
                    Stream.Add(SpanClassAdminSmall + "<br><br>");
                    Stream.Add(DateTime.Now + " Opening Table Schema on DataSource [" + datasource.name + "]<br>");
                    //
                    RSSchema = core.db.getTableSchemaData(TableName);
                    Stream.Add(DateTime.Now + " GetSchema executed successfully<br>");
                    if (!DbController.isDataTableOk(RSSchema)) {
                        //
                        // ----- no result
                        //
                        Stream.Add(DateTime.Now + " A schema was returned, but it contains no records.<br>");
                    } else {
                        //
                        // ----- print results
                        //
                        Stream.Add(DateTime.Now + " The following results were returned<br>");
                        //
                        // --- Create the Fields for the new table
                        //
                        FieldCount = RSSchema.Columns.Count;
                        Stream.Add("<table border=\"1\" cellpadding=\"1\" cellspacing=\"1\" width=\"100%\">");
                        Stream.Add("<tr>");
                        foreach (DataColumn RecordField in RSSchema.Columns) {
                            Stream.Add("<TD><B>" + SpanClassAdminSmall + RecordField.ColumnName + "</b></SPAN></td>");
                        }
                        Stream.Add("</tr>");
                        //
                        arrayOfSchema = core.db.convertDataTabletoArray(RSSchema);
                        //
                        RowMax = arrayOfSchema.GetUpperBound(1);
                        ColumnMax = arrayOfSchema.GetUpperBound(0);
                        RowStart = "<tr>";
                        RowEnd = "</tr>";
                        ColumnStart = "<td class=\"ccadminsmall\">";
                        ColumnEnd = "</td>";
                        for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                            Stream.Add(RowStart);
                            for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                CellData = arrayOfSchema[ColumnPointer, RowPointer];
                                if (isNull(CellData)) {
                                    Stream.Add(ColumnStart + "[null]" + ColumnEnd);
                                } else if ((CellData == null)) {
                                    Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
                                } else if (string.IsNullOrEmpty(CellData)) {
                                    Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
                                } else {
                                    Stream.Add(ColumnStart + CellData + ColumnEnd);
                                }
                            }
                            Stream.Add(RowEnd);
                        }
                        Stream.Add("</TABLE>");
                        RSSchema.Dispose();
                        RSSchema = null;
                    }
                    //
                    // Index Schema
                    //
                    //    RSSchema = DataSourceConnectionObjs(DataSourcePointer).Conn.OpenSchema(SchemaEnum.adSchemaColumns, Array(Empty, Empty, TableName, Empty))
                    Stream.Add(SpanClassAdminSmall + "<br><br>");
                    Stream.Add(DateTime.Now + " Opening Index Schema<br>");
                    //
                    RSSchema = core.db.getIndexSchemaData(TableName);
                    if (!DbController.isDataTableOk(RSSchema)) {
                        //
                        // ----- no result
                        //
                        Stream.Add(DateTime.Now + " A schema was returned, but it contains no records.<br>");
                    } else {
                        //
                        // ----- print results
                        //
                        Stream.Add(DateTime.Now + " The following results were returned<br>");
                        //
                        // --- Create the Fields for the new table
                        //
                        FieldCount = RSSchema.Columns.Count;
                        Stream.Add("<table border=\"1\" cellpadding=\"1\" cellspacing=\"1\" width=\"100%\">");
                        Stream.Add("<tr>");
                        foreach (DataColumn RecordField in RSSchema.Columns) {
                            Stream.Add("<TD><B>" + SpanClassAdminSmall + RecordField.ColumnName + "</b></SPAN></td>");
                        }
                        Stream.Add("</tr>");
                        //

                        arrayOfSchema = core.db.convertDataTabletoArray(RSSchema);
                        //
                        RowMax = arrayOfSchema.GetUpperBound(1);
                        ColumnMax = arrayOfSchema.GetUpperBound(0);
                        RowStart = "<tr>";
                        RowEnd = "</tr>";
                        ColumnStart = "<td class=\"ccadminsmall\">";
                        ColumnEnd = "</td>";
                        for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                            Stream.Add(RowStart);
                            for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                CellData = arrayOfSchema[ColumnPointer, RowPointer];
                                if (isNull(CellData)) {
                                    Stream.Add(ColumnStart + "[null]" + ColumnEnd);
                                } else if ((CellData == null)) {
                                    Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
                                } else if (string.IsNullOrEmpty(CellData)) {
                                    Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
                                } else {
                                    Stream.Add(ColumnStart + CellData + ColumnEnd);
                                }
                            }
                            Stream.Add(RowEnd);
                        }
                        Stream.Add("</TABLE>");
                        RSSchema.Dispose();
                        RSSchema = null;
                    }
                    //
                    // Column Schema
                    //
                    Stream.Add(SpanClassAdminSmall + "<br><br>");
                    Stream.Add(DateTime.Now + " Opening Column Schema<br>");
                    //
                    RSSchema = core.db.getColumnSchemaData(TableName);
                    Stream.Add(DateTime.Now + " GetSchema executed successfully<br>");
                    if (DbController.isDataTableOk(RSSchema)) {
                        //
                        // ----- no result
                        //
                        Stream.Add(DateTime.Now + " A schema was returned, but it contains no records.<br>");
                    } else {
                        //
                        // ----- print results
                        //
                        Stream.Add(DateTime.Now + " The following results were returned<br>");
                        //
                        // --- Create the Fields for the new table
                        //
                        FieldCount = RSSchema.Columns.Count;
                        Stream.Add("<table border=\"1\" cellpadding=\"1\" cellspacing=\"1\" width=\"100%\">");
                        Stream.Add("<tr>");
                        foreach (DataColumn RecordField in RSSchema.Columns) {
                            Stream.Add("<TD><B>" + SpanClassAdminSmall + RecordField.ColumnName + "</b></SPAN></td>");
                        }
                        Stream.Add("</tr>");
                        //
                        arrayOfSchema = core.db.convertDataTabletoArray(RSSchema);
                        //
                        RowMax = arrayOfSchema.GetUpperBound(1);
                        ColumnMax = arrayOfSchema.GetUpperBound(0);
                        RowStart = "<tr>";
                        RowEnd = "</tr>";
                        ColumnStart = "<td class=\"ccadminsmall\">";
                        ColumnEnd = "</td>";
                        for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                            Stream.Add(RowStart);
                            for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                CellData = arrayOfSchema[ColumnPointer, RowPointer];
                                if (isNull(CellData)) {
                                    Stream.Add(ColumnStart + "[null]" + ColumnEnd);
                                } else if ((CellData == null)) {
                                    Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
                                } else if (string.IsNullOrEmpty(CellData)) {
                                    Stream.Add(ColumnStart + "[empty]" + ColumnEnd);
                                } else {
                                    Stream.Add(ColumnStart + CellData + ColumnEnd);
                                }
                            }
                            Stream.Add(RowEnd);
                        }
                        Stream.Add("</TABLE>");
                        RSSchema.Dispose();
                        RSSchema = null;
                    }
                    if (!StatusOK) {
                        Stream.Add("There was a problem executing this query that may have prevented the results from printing.");
                    }
                    Stream.Add(DateTime.Now + " Done</SPAN>");
                }
                //
                // Display form
                //
                Stream.Add(SpanClassAdminNormal);
                //
                Stream.Add("<br>");
                Stream.Add("Table Name<br>");
                Stream.Add(HtmlController.inputText_Legacy(core, "Tablename", TableName));
                //
                Stream.Add("<br><br>");
                Stream.Add("Data Source<br>");
                Stream.Add(core.html.selectFromContent("DataSourceID", datasource.id, "Data Sources", "", "Default"));
                //
                Stream.Add("</SPAN>");
                //
                result = AdminUIController.getToolForm(core, Stream.Text, ButtonList);
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }
        //
    }
}

