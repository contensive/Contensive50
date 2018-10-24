
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
    public class ToolCustomReports {
        //
        //========================================================================
        //
        //========================================================================
        //
        //========================================================================
        //
        public static string GetForm_CustomReports(CoreController core) {
            string tempGetForm_CustomReports = null;
            try {
                //
                string Button = null;
                int CS = 0;
                string RecordName = null;
                int RecordID = 0;
                string SQL = null;
                string RQS = null;
                int PageSize = 0;
                int PageNumber = 0;
                int TopCount = 0;
                int RowPointer = 0;
                int DataRowCount = 0;
                string PreTableCopy = "";
                string PostTableCopy = "";
                int ColumnPtr = 0;
                string[] ColCaption = null;
                string[] ColAlign = null;
                string[] ColWidth = null;
                string[,] Cells = null;
                string AdminURL = null;
                int RowCnt = 0;
                int RowPtr = 0;
                int ContentID = 0;
                string Format = null;
                string Filename = null;
                string Name = null;
                string Caption = null;
                string Description = null;
                string ButtonListLeft = null;
                string ButtonListRight = null;
                int ContentPadding = 0;
                string ContentSummary = "";
                StringBuilderLegacyController Tab0 = new StringBuilderLegacyController();
                StringBuilderLegacyController Tab1 = new StringBuilderLegacyController();
                string Content = "";
                string SQLFieldName = null;
                var adminMenu = new AdminMenuController();
                //
                const int ColumnCnt = 4;
                //
                Button = core.docProperties.getText(RequestNameButton);
                ContentID = core.docProperties.getInteger("ContentID");
                Format = core.docProperties.getText("Format");
                //
                Caption = "Custom Report Manager";
                Description = "Custom Reports are a way for you to create a snapshot of data to view or download. To request a report, select the Custom Reports tab, check the report(s) you want, and click the [Request Download] Button. When your report is ready, it will be available in the <a href=\"?" + rnAdminForm + "=30\">Download Manager</a>. To create a new custom report, select the Request New Report tab, enter a name and SQL statement, and click the Apply button.";
                ContentPadding = 0;
                ButtonListLeft = ButtonCancel + "," + ButtonDelete + "," + ButtonRequestDownload;
                //ButtonListLeft = ButtonCancel & "," & ButtonDelete & "," & ButtonRequestDownload & "," & ButtonApply
                ButtonListRight = "";
                SQLFieldName = "SQLQuery";
                //
                if (!core.session.isAuthenticatedAdmin(core)) {
                    //
                    // Must be a developer
                    //
                    Description = Description + "You can not access the Custom Report Manager because your account is not configured as an administrator.";
                } else {
                    //
                    // Process Requests
                    //
                    if (!string.IsNullOrEmpty(Button)) {
                        switch (Button) {
                            case ButtonCancel:
                                return core.webServer.redirect("/" + core.appConfig.adminRoute, "CustomReports, Cancel Button Pressed");
                            //Call core.main_Redirect2(encodeAppRootPath(core.main_GetSiteProperty2("AdminURL"), core.main_ServerVirtualPath, core.app.RootPath, core.main_ServerHost))
                            case ButtonDelete:
                                RowCnt = core.docProperties.getInteger("RowCnt");
                                if (RowCnt > 0) {
                                    for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                                        if (core.docProperties.getBoolean("Row" + RowPtr)) {
                                            core.db.deleteContentRecord("Custom Reports", core.docProperties.getInteger("RowID" + RowPtr));
                                        }
                                    }
                                }
                                break;
                            case ButtonRequestDownload:
                            case ButtonApply:
                                //
                                Name = core.docProperties.getText("name");
                                SQL = core.docProperties.getText(SQLFieldName);
                                if (!string.IsNullOrEmpty(Name) | !string.IsNullOrEmpty(SQL)) {
                                    if ((string.IsNullOrEmpty(Name)) || (string.IsNullOrEmpty(SQL))) {
                                        Processor.Controllers.ErrorController.addUserError(core, "A name and SQL Query are required to save a new custom report.");
                                    } else {
                                        CS = core.db.csInsertRecord("Custom Reports");
                                        if (core.db.csOk(CS)) {
                                            core.db.csSet(CS, "Name", Name);
                                            core.db.csSet(CS, SQLFieldName, SQL);
                                        }
                                        core.db.csClose(ref CS);
                                    }
                                }
                                //
                                RowCnt = core.docProperties.getInteger("RowCnt");
                                if (RowCnt > 0) {
                                    for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                                        if (core.docProperties.getBoolean("Row" + RowPtr)) {
                                            RecordID = core.docProperties.getInteger("RowID" + RowPtr);
                                            CS = core.db.csOpenRecord("Custom Reports", RecordID);
                                            if (core.db.csOk(CS)) {
                                                SQL = core.db.csGetText(CS, SQLFieldName);
                                                Name = core.db.csGetText(CS, "Name");
                                            }
                                            core.db.csClose(ref CS);
                                            //
                                            CS = core.db.csInsertRecord("Tasks");
                                            if (core.db.csOk(CS)) {
                                                RecordName = "CSV Download, Custom Report [" + Name + "]";
                                                Filename = "CustomReport_" + encodeText(GenericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(GenericController.GetRandomInteger(core)) + ".csv";
                                                core.db.csSet(CS, "Name", RecordName);
                                                core.db.csSet(CS, "Filename", Filename);
                                                if (Format == "XML") {
                                                    core.db.csSet(CS, "Command", "BUILDXML");
                                                } else {
                                                    core.db.csSet(CS, "Command", "BUILDCSV");
                                                }
                                                core.db.csSet(CS, SQLFieldName, SQL);
                                                Description = Description + "<p>Your Download [" + Name + "] has been requested, and will be available in the <a href=\"?" + rnAdminForm + "=30\">Download Manager</a> when it is complete. This may take a few minutes depending on the size of the report.</p>";
                                            }
                                            core.db.csClose(ref CS);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    //
                    // Build Tab0
                    //
                    Tab0.Add("<p>The following is a list of available custom reports.</p>");
                    //
                    RQS = core.doc.refreshQueryString;
                    PageSize = core.docProperties.getInteger(RequestNamePageSize);
                    if (PageSize == 0) {
                        PageSize = 50;
                    }
                    PageNumber = core.docProperties.getInteger(RequestNamePageNumber);
                    if (PageNumber == 0) {
                        PageNumber = 1;
                    }
                    AdminURL = "/" + core.appConfig.adminRoute;
                    TopCount = PageNumber * PageSize;
                    //
                    // Setup Headings
                    //
                    ColCaption = new string[ColumnCnt + 1];
                    ColAlign = new string[ColumnCnt + 1];
                    ColWidth = new string[ColumnCnt + 1];
                    Cells = new string[PageSize + 1, ColumnCnt + 1];
                    //
                    ColCaption[ColumnPtr] = "Select<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=10 height=1>";
                    ColAlign[ColumnPtr] = "center";
                    ColWidth[ColumnPtr] = "10";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    ColCaption[ColumnPtr] = "Name";
                    ColAlign[ColumnPtr] = "left";
                    ColWidth[ColumnPtr] = "100%";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    ColCaption[ColumnPtr] = "Created By<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=100 height=1>";
                    ColAlign[ColumnPtr] = "left";
                    ColWidth[ColumnPtr] = "100";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    ColCaption[ColumnPtr] = "Date Created<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=150 height=1>";
                    ColAlign[ColumnPtr] = "left";
                    ColWidth[ColumnPtr] = "150";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    //ColCaption(ColumnPtr) = "?<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=100 height=1>"
                    //ColAlign(ColumnPtr) = "Left"
                    //ColWidth(ColumnPtr) = "100"
                    //ColumnPtr = ColumnPtr + 1
                    //
                    //   Get Data
                    //
                    CS = core.db.csOpen("Custom Reports");
                    RowPointer = 0;
                    if (!core.db.csOk(CS)) {
                        Cells[0, 1] = "There are no custom reports defined";
                        RowPointer = 1;
                    } else {
                        DataRowCount = core.db.csGetRowCount(CS);
                        while (core.db.csOk(CS) && (RowPointer < PageSize)) {
                            RecordID = core.db.csGetInteger(CS, "ID");
                            //DateCompleted = core.db.cs_getDate(CS, "DateCompleted")
                            Cells[RowPointer, 0] = HtmlController.checkbox("Row" + RowPointer) + HtmlController.inputHidden("RowID" + RowPointer, RecordID);
                            Cells[RowPointer, 1] = core.db.csGetText(CS, "name");
                            Cells[RowPointer, 2] = core.db.csGet(CS, "CreatedBy");
                            Cells[RowPointer, 3] = core.db.csGetDate(CS, "DateAdded").ToShortDateString();
                            //Cells(RowPointer, 4) = "&nbsp;"
                            RowPointer = RowPointer + 1;
                            core.db.csGoNext(CS);
                        }
                    }
                    core.db.csClose(ref CS);
                    string Cell = null;
                    Tab0.Add(HtmlController.inputHidden("RowCnt", RowPointer));
                    //adminUIController Adminui = new adminUIController(core);
                    Cell = AdminUIController.getReport(core, RowPointer, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, "ccPanel");
                    Tab0.Add("<div>" + Cell + "</div>");
                    //
                    // Build RequestContent Form
                    //
                    Tab1.Add("<p>Use this form to create a new custom report. Enter the SQL Query for the report, and a name that will be used as a caption.</p>");
                    //
                    Tab1.Add("<table border=\"0\" cellpadding=\"3\" cellspacing=\"0\" width=\"100%\">");
                    //
                    Tab1.Add("<tr>");
                    Tab1.Add("<td align=right>Name</td>");
                    Tab1.Add("<td>" + HtmlController.inputText(core, "Name", "", 1, 40) + "</td>");
                    Tab1.Add("</tr>");
                    //
                    Tab1.Add("<tr>");
                    Tab1.Add("<td align=right>SQL Query</td>");
                    Tab1.Add("<td>" + HtmlController.inputText(core, SQLFieldName, "", 8, 40) + "</td>");
                    Tab1.Add("</tr>");
                    //
                    Tab1.Add("<tr><td width=\"120\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"120\" height=\"1\"></td><td width=\"100%\">&nbsp;</td></tr></table>");
                    //
                    // Build and add tabs
                    //
                    adminMenu.menuLiveTab.AddEntry("Custom&nbsp;Reports", Tab0.Text, "ccAdminTab");
                    adminMenu.menuLiveTab.AddEntry("Request&nbsp;New&nbsp;Report", Tab1.Text, "ccAdminTab");
                    Content = adminMenu.menuLiveTab.GetTabs(core);
                    //
                }
                //
                tempGetForm_CustomReports =  AdminUIController.getBody(core, Caption, ButtonListLeft, ButtonListRight, true, true, Description, ContentSummary, ContentPadding, Content);
                //
                core.html.addTitle("Custom Reports");
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetForm_CustomReports;
        }
        //
    }
}
