
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
    public class BodyIndexExportClass {
        //
        //=============================================================================
        //   Export the Admin List form results
        //=============================================================================
        //
        public static string get( CoreController core, AdminInfoDomainModel adminContext) {
            string result = "";
            try {
                //
                bool AllowContentAccess = false;
                string ButtonList = "";
                string ExportName = null;
                //adminUIController Adminui = new adminUIController(core);
                string Description = null;
                string Content = "";
                int ExportType = 0;
                string Button = null;
                int RecordLimit = 0;
                int recordCnt = 0;
                //Dim DataSourceName As String
                //Dim DataSourceType As Integer
                string sqlFieldList = "";
                string SQLFrom = "";
                string SQLWhere = "";
                string SQLOrderBy = "";
                bool IsLimitedToSubContent = false;
                string ContentAccessLimitMessage = "";
                Dictionary<string, bool> FieldUsedInColumns = new Dictionary<string, bool>();
                Dictionary<string, bool> IsLookupFieldValid = new Dictionary<string, bool>();
                IndexConfigClass IndexConfig = null;
                string SQL = null;
                int CS = 0;
                //Dim RecordTop As Integer
                //Dim RecordsPerPage As Integer
                bool IsRecordLimitSet = false;
                string RecordLimitText = null;
                bool allowContentEdit = false;
                bool allowContentAdd = false;
                bool allowContentDelete = false;
                var tmpList = new List<string>();
                DataSourceModel datasource = DataSourceModel.create(core, adminContext.adminContent.dataSourceId, ref tmpList);
                //
                // ----- Process Input
                //
                Button = core.docProperties.getText("Button");
                if (Button == ButtonCancelAll) {
                    //
                    // Cancel out to the main page
                    //
                    return core.webServer.redirect("?", "CancelAll button pressed on Index Export");
                } else if (Button != ButtonCancel) {
                    //
                    // get content access rights
                    //
                    core.session.getContentAccessRights(core, adminContext.adminContent.name, ref allowContentEdit, ref allowContentAdd, ref allowContentDelete);
                    if (!allowContentEdit) {
                        //If Not core.doc.authContext.user.main_IsContentManager2(adminContext.content.Name) Then
                        //
                        // You must be a content manager of this content to use this tool
                        //
                        Content = ""
                            + "<p>You must be a content manager of " + adminContext.adminContent.name + " to use this tool. Hit Cancel to return to main admin page.</p>"
                            + HtmlController.inputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormExport) + "";
                        ButtonList = ButtonCancelAll;
                    } else {
                        IsRecordLimitSet = false;
                        if (string.IsNullOrEmpty(Button)) {
                            //
                            // Set Defaults
                            //
                            ExportName = "";
                            ExportType = 1;
                            RecordLimit = 0;
                            RecordLimitText = "";
                        } else {
                            ExportName = core.docProperties.getText("ExportName");
                            ExportType = core.docProperties.getInteger("ExportType");
                            RecordLimitText = core.docProperties.getText("RecordLimit");
                            if (!string.IsNullOrEmpty(RecordLimitText)) {
                                IsRecordLimitSet = true;
                                RecordLimit = GenericController.encodeInteger(RecordLimitText);
                            }
                        }
                        if (string.IsNullOrEmpty(ExportName)) {
                            ExportName = adminContext.adminContent.name + " export for " + core.session.user.name;
                        }
                        //
                        // Get the SQL parts
                        //
                        //DataSourceName = core.db.getDataSourceNameByID(adminContext.content.dataSourceId)
                        //DataSourceType = core.db.getDataSourceType(DataSourceName)
                        IndexConfig = IndexConfigClass.get(core, adminContext);
                        //RecordTop = IndexConfig.RecordTop
                        //RecordsPerPage = IndexConfig.RecordsPerPage
                        BodyIndexClass.setIndexSQL(core, adminContext, IndexConfig, ref AllowContentAccess, ref sqlFieldList, ref SQLFrom, ref SQLWhere, ref SQLOrderBy, ref IsLimitedToSubContent, ref ContentAccessLimitMessage, ref FieldUsedInColumns, IsLookupFieldValid);
                        if (!AllowContentAccess) {
                            //
                            // This should be caught with check earlier, but since I added this, and I never make mistakes, I will leave this in case there is a mistake in the earlier code
                            //
                            ErrorController.addUserError(core, "Your account does not have access to any records in '" + adminContext.adminContent.name + "'.");
                        } else {
                            //
                            // Get the total record count
                            //
                            SQL = "select count(" + adminContext.adminContent.tableName + ".ID) as cnt from " + SQLFrom + " where " + SQLWhere;
                            CS = core.db.csOpenSql(SQL, datasource.name);
                            if (core.db.csOk(CS)) {
                                recordCnt = core.db.csGetInteger(CS, "cnt");
                            }
                            core.db.csClose(ref CS);
                            //
                            // Build the SQL
                            //
                            SQL = "select";
                            if (IsRecordLimitSet && (datasource.type != DataSourceTypeODBCMySQL)) {
                                SQL += " Top " + RecordLimit;
                            }
                            SQL += " " + adminContext.adminContent.tableName + ".* From " + SQLFrom + " WHERE " + SQLWhere;
                            if (!string.IsNullOrEmpty(SQLOrderBy)) {
                                SQL += " Order By" + SQLOrderBy;
                            }
                            if (IsRecordLimitSet && (datasource.type == DataSourceTypeODBCMySQL)) {
                                SQL += " Limit " + RecordLimit;
                            }
                            //
                            // Assumble the SQL
                            //
                            if (recordCnt == 0) {
                                //
                                // There are no records to request
                                //
                                Content = ""
                                    + "<p>This selection has no records.. Hit Cancel to return to the " + adminContext.adminContent.name + " list page.</p>"
                                    + HtmlController.inputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormExport) + "";
                                ButtonList = ButtonCancel;
                            } else if (Button == ButtonRequestDownload) {
                                //
                                // Request the download
                                //
                                switch (ExportType) {
                                    case 1:
                                        var ExportCSVAddon = Processor.Models.Db.AddonModel.create(core, addonGuidExportCSV);
                                        if (ExportCSVAddon == null) {
                                            LogController.handleError(core, new ApplicationException("ExportCSV addon not found. Task could not be added to task queue."));
                                        } else {
                                            var docProperties = new Dictionary<string, string> {
                                                { "sql", SQL },
                                                { "ExportName", ExportName },
                                                { "filename", "Export-" + GenericController.GetRandomInteger(core).ToString() + ".csv" }
                                            };
                                            var cmdDetail = new cmdDetailClass() {
                                                addonId = ExportCSVAddon.id,
                                                addonName = ExportCSVAddon.name,
                                                args = docProperties
                                            };
                                            TaskSchedulerControllerx.addTaskToQueue(core, taskCommandBuildCsv, cmdDetail, false);
                                        }
                                        break;
                                    default:
                                        var ExportXMLAddon = Processor.Models.Db.AddonModel.create(core, addonGuidExportXML);
                                        if (ExportXMLAddon == null) {
                                            LogController.handleError(core, new ApplicationException(message: "ExportXML addon not found. Task could not be added to task queue."));
                                        } else {
                                            var docProperties = new Dictionary<string, string> {
                                                { "sql", SQL },
                                                { "ExportName", ExportName },
                                                { "filename", "Export-" + GenericController.GetRandomInteger(core).ToString() + ".xml" }
                                            };
                                            var cmdDetail = new cmdDetailClass() {
                                                addonId = ExportXMLAddon.id,
                                                addonName = ExportXMLAddon.name,
                                                args = docProperties
                                            };
                                            TaskSchedulerControllerx.addTaskToQueue(core, taskCommandBuildXml, cmdDetail, false);
                                        }
                                        break;
                                }
                                //
                                Content = ""
                                    + "<p>Your export has been requested and will be available shortly in the <a href=\"?" + rnAdminForm + "=" + AdminFormDownloads + "\">Download Manager</a>. Hit Cancel to return to the " + adminContext.adminContent.name + " list page.</p>"
                                    + HtmlController.inputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormExport) + "";
                                //
                                ButtonList = ButtonCancel;
                            } else {
                                //
                                // no button or refresh button, Ask are you sure
                                //
                                Content = Content + "\r<tr>"
                                    + cr2 + "<td class=\"exportTblCaption\">Export Name</td>"
                                    + cr2 + "<td class=\"exportTblInput\">" + HtmlController.inputText(core, "ExportName", ExportName) + "</td>"
                                    + "\r</tr>";
                                Content = Content + "\r<tr>"
                                    + cr2 + "<td class=\"exportTblCaption\">Export Format</td>"
                                    + cr2 + "<td class=\"exportTblInput\">" + HtmlController.selectFromList(core, "ExportType", ExportType, new String[] { "Comma Delimited,XML" }, "", "") + "</td>"
                                    + "\r</tr>";
                                Content = Content + "\r<tr>"
                                    + cr2 + "<td class=\"exportTblCaption\">Records Found</td>"
                                    + cr2 + "<td class=\"exportTblInput\">" + HtmlController.inputText(core, "RecordCnt", recordCnt.ToString(), -1, -1, "", false, true) + "</td>"
                                    + "\r</tr>";
                                Content = Content + "\r<tr>"
                                    + cr2 + "<td class=\"exportTblCaption\">Record Limit</td>"
                                    + cr2 + "<td class=\"exportTblInput\">" + HtmlController.inputText(core, "RecordLimit", RecordLimitText) + "</td>"
                                    + "\r</tr>";
                                if (core.session.isAuthenticatedDeveloper(core)) {
                                    Content = Content + "\r<tr>"
                                        + cr2 + "<td class=\"exportTblCaption\">Results SQL</td>"
                                        + cr2 + "<td class=\"exportTblInput\"><div style=\"border:1px dashed #ccc;background-color:#fff;padding:10px;\">" + SQL + "</div></td>"
                                        + "\r</tr>"
                                        + "";
                                }
                                //
                                Content = ""
                                    + "\r<table>"
                                    + GenericController.nop(Content) + "\r</table>"
                                    + "";
                                //
                                Content = ""
                                    + "\r<style>"
                                    + cr2 + ".exportTblCaption {width:100px;}"
                                    + cr2 + ".exportTblInput {}"
                                    + "\r</style>"
                                    + Content + HtmlController.inputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormExport) + "";
                                ButtonList = ButtonCancel + "," + ButtonRequestDownload;
                                if (core.session.isAuthenticatedDeveloper(core)) {
                                    ButtonList = ButtonList + "," + ButtonRefresh;
                                }
                            }
                        }
                    }
                    //
                    Description = "<p>This tool creates an export of the current admin list page results. If you would like to download the current results, select a format and press OK. Your search results will be submitted for export. Your download will be ready shortly in the download manager. To exit without requesting an output, hit Cancel.</p>";
                    result = ""
                        + AdminUIController.getBody(core, adminContext.adminContent.name + " Export", ButtonList, "", false, false, Description, "", 10, Content);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
    }
}
