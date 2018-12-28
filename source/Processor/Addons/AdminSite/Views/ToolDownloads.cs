
using System;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Addons.AdminSite.Controllers;

namespace Contensive.Addons.AdminSite {
    public class ToolDownloads {
        //
        //========================================================================
        //
        //========================================================================
        //
        public static string GetForm_Downloads(CoreController core) {
            string tempGetForm_Downloads = null;
            try {
                string ResultMessage = null;
                string LinkPrefix = null;
                string LinkSuffix = null;
                string RemoteKey = null;
                string Button = null;
                int CS = 0;
                string ContentName = null;
                int RecordID = 0;
                string SQL = null;
                string RQS = null;
                string Criteria = null;
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
                DateTime DateCompleted = default(DateTime);
                int RowCnt = 0;
                int RowPtr = 0;
                int ContentID = 0;
                string Format = null;
                string TableName = null;
                string Filename = null;
                string Name = null;
                string Caption = null;
                string Description = "";
                string ButtonListLeft = null;
                string ButtonListRight = null;
                int ContentPadding = 0;
                string ContentSummary = "";
                StringBuilderLegacyController Tab0 = new StringBuilderLegacyController();
                StringBuilderLegacyController Tab1 = new StringBuilderLegacyController();
                string Content = "";
                string Cell = null;
                //adminUIController Adminui = new adminUIController(core);
                string SQLFieldName = null;
                //
                const int ColumnCnt = 5;
                //
                Button = core.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    return core.webServer.redirect("/" + core.appConfig.adminRoute, "Downloads, Cancel Button Pressed");
                }
                //
                if (!core.session.isAuthenticatedAdmin(core)) {
                    //
                    // Must be a developer
                    //
                    ButtonListLeft = ButtonCancel;
                    ButtonListRight = "";
                    Content = Content + AdminUIController.getFormBodyAdminOnly();
                } else {
                    ContentID = core.docProperties.getInteger("ContentID");
                    Format = core.docProperties.getText("Format");
                    SQLFieldName = "SQLQuery";
                    //
                    // Process Requests
                    //
                    if (!string.IsNullOrEmpty(Button)) {
                        switch (Button) {
                            case ButtonDelete:
                                RowCnt = core.docProperties.getInteger("RowCnt");
                                if (RowCnt > 0) {
                                    for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                                        if (core.docProperties.getBoolean("Row" + RowPtr)) {
                                            core.db.deleteContentRecord("Tasks", core.docProperties.getInteger("RowID" + RowPtr));
                                        }
                                    }
                                }
                                break;
                            case ButtonRequestDownload:
                                //
                                // Request the download again
                                //
                                RowCnt = core.docProperties.getInteger("RowCnt");
                                if (RowCnt > 0) {
                                    for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                                        if (core.docProperties.getBoolean("Row" + RowPtr)) {
                                            int CSSrc = 0;
                                            int CSDst = 0;

                                            CSSrc = csXfer.csOpenRecord("Tasks", core.docProperties.getInteger("RowID" + RowPtr));
                                            if (csXfer.csOk(CSSrc)) {
                                                CSDst = csXfer.csInsert("Tasks");
                                                if (csXfer.csOk(CSDst)) {
                                                    csXfer.csSet(CSDst, "Name", csXfer.csGetText(CSSrc, "name"));
                                                    csXfer.csSet(CSDst, SQLFieldName, csXfer.csGetText(CSSrc, SQLFieldName));
                                                    if (GenericController.vbLCase(csXfer.csGetText(CSSrc, "command")) == "xml") {
                                                        csXfer.csSet(CSDst, "Filename", "DupDownload_" + encodeText(GenericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(GenericController.GetRandomInteger(core)) + ".xml");
                                                        csXfer.csSet(CSDst, "Command", "BUILDXML");
                                                    } else {
                                                        csXfer.csSet(CSDst, "Filename", "DupDownload_" + encodeText(GenericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(GenericController.GetRandomInteger(core)) + ".csv");
                                                        csXfer.csSet(CSDst, "Command", "BUILDCSV");
                                                    }
                                                }
                                                csXfer.csClose(ref CSDst);
                                            }
                                            csXfer.csClose(ref CSSrc);
                                        }
                                    }
                                }
                                //
                                //
                                //
                                if ((!string.IsNullOrEmpty(Format)) && (ContentID == 0)) {
                                    Description = Description + "<p>Please select a Content before requesting a download</p>";
                                } else if ((string.IsNullOrEmpty(Format)) && (ContentID != 0)) {
                                    Description = Description + "<p>Please select a Format before requesting a download</p>";
                                } else if (Format == "CSV") {
                                    csXfer.csInsert("Tasks");
                                    if (csXfer.csOk()) {
                                        ContentName = MetaController.getContentNameByID(core, ContentID);
                                        TableName = MetaController.getContentTablename(core, ContentName);
                                        Criteria = MetaController.getContentControlCriteria(core, ContentName);
                                        Name = "CSV Download, " + ContentName;
                                        Filename = GenericController.vbReplace(ContentName, " ", "") + "_" + encodeText(GenericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(GenericController.GetRandomInteger(core)) + ".csv";
                                        csXfer.csSet(CS, "Name", Name);
                                        csXfer.csSet(CS, "Filename", Filename);
                                        csXfer.csSet(CS, "Command", "BUILDCSV");
                                        csXfer.csSet(CS, SQLFieldName, "SELECT * from " + TableName + " where " + Criteria);
                                        Description = Description + "<p>Your CSV Download has been requested.</p>";
                                    }
                                    csXfer.csClose();
                                    Format = "";
                                    ContentID = 0;
                                } else if (Format == "XML") {
                                    csXfer.csInsert("Tasks");
                                    if (csXfer.csOk()) {
                                        ContentName = MetaController.getContentNameByID(core, ContentID);
                                        TableName = MetaController.getContentTablename(core, ContentName);
                                        Criteria = MetaController.getContentControlCriteria(core, ContentName);
                                        Name = "XML Download, " + ContentName;
                                        Filename = GenericController.vbReplace(ContentName, " ", "") + "_" + encodeText(GenericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(GenericController.GetRandomInteger(core)) + ".xml";
                                        csXfer.csSet(CS, "Name", Name);
                                        csXfer.csSet(CS, "Filename", Filename);
                                        csXfer.csSet(CS, "Command", "BUILDXML");
                                        csXfer.csSet(CS, SQLFieldName, "SELECT * from " + TableName + " where " + Criteria);
                                        Description = Description + "<p>Your XML Download has been requested.</p>";
                                    }
                                    csXfer.csClose();
                                    Format = "";
                                    ContentID = 0;
                                }
                                break;
                        }
                    }
                    //
                    // Build Tab0
                    //
                    //Tab0.Add( "<p>The following is a list of available downloads</p>")
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
                    ColCaption[ColumnPtr] = "Select<br><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=10 height=1>";
                    ColAlign[ColumnPtr] = "center";
                    ColWidth[ColumnPtr] = "10";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    ColCaption[ColumnPtr] = "Name";
                    ColAlign[ColumnPtr] = "left";
                    ColWidth[ColumnPtr] = "100%";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    ColCaption[ColumnPtr] = "For<br><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=100 height=1>";
                    ColAlign[ColumnPtr] = "left";
                    ColWidth[ColumnPtr] = "100";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    ColCaption[ColumnPtr] = "Requested<br><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=150 height=1>";
                    ColAlign[ColumnPtr] = "left";
                    ColWidth[ColumnPtr] = "150";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    ColCaption[ColumnPtr] = "File<br><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=100 height=1>";
                    ColAlign[ColumnPtr] = "Left";
                    ColWidth[ColumnPtr] = "100";
                    ColumnPtr = ColumnPtr + 1;
                    //
                    //   Get Data
                    //
                    SQL = "select M.Name as CreatedByName, T.* from ccTasks T left join ccMembers M on M.ID=T.CreatedBy where (T.Command='BuildCSV')or(T.Command='BuildXML') order by T.DateAdded Desc";
                    //Call core.main_TestPoint("Selection SQL=" & SQL)
                    csXfer.csOpenSql(SQL, "Default", PageSize, PageNumber);
                    RowPointer = 0;
                    if (!csXfer.csOk()) {
                        Cells[0, 1] = "There are no download requests";
                        RowPointer = 1;
                    } else {
                        DataRowCount = csXfer.csGetRowCount(CS);
                        LinkPrefix = "<a href=\"" + core.appConfig.cdnFileUrl;
                        LinkSuffix = "\" target=_blank>Available</a>";
                        while (csXfer.csOk() && (RowPointer < PageSize)) {
                            RecordID = csXfer.csGetInteger(CS, "ID");
                            DateCompleted = csXfer.csGetDate(CS, "DateCompleted");
                            ResultMessage = csXfer.csGetText(CS, "ResultMessage");
                            Cells[RowPointer, 0] = HtmlController.checkbox("Row" + RowPointer) + HtmlController.inputHidden("RowID" + RowPointer, RecordID);
                            Cells[RowPointer, 1] = csXfer.csGetText(CS, "name");
                            Cells[RowPointer, 2] = csXfer.csGetText(CS, "CreatedByName");
                            Cells[RowPointer, 3] = csXfer.csGetDate(CS, "DateAdded").ToShortDateString();
                            if (DateCompleted == DateTime.MinValue) {
                                RemoteKey = RemoteQueryController.main_GetRemoteQueryKey(core, "select DateCompleted,filename,resultMessage from cctasks where id=" + RecordID, "default", 1);
                                Cell = "";
                                Cell = Cell + "\r\n<div id=\"pending" + RowPointer + "\">Pending <img src=\"/ContensiveBase/images/ajax-loader-small.gif\" width=16 height=16></div>";
                                //
                                Cell = Cell + "\r\n<script>";
                                Cell = Cell + "\r\nfunction statusHandler" + RowPointer + "(results) {";
                                Cell = Cell + "\r\n var jo,isDone=false;";
                                Cell = Cell + "\r\n eval('jo='+results);";
                                Cell = Cell + "\r\n if (jo){";
                                Cell = Cell + "\r\n  if(jo.DateCompleted) {";
                                Cell = Cell + "\r\n    var dst=document.getElementById('pending" + RowPointer + "');";
                                Cell = Cell + "\r\n    isDone=true;";
                                Cell = Cell + "\r\n    if(jo.resultMessage=='OK') {";
                                Cell = Cell + "\r\n      dst.innerHTML='" + LinkPrefix + "'+jo.filename+'" + LinkSuffix + "';";
                                Cell = Cell + "\r\n    }else{";
                                Cell = Cell + "\r\n      dst.innerHTML='error';";
                                Cell = Cell + "\r\n    }";
                                Cell = Cell + "\r\n  }";
                                Cell = Cell + "\r\n }";
                                Cell = Cell + "\r\n if(!isDone) setTimeout(\"requestStatus" + RowPointer + "()\",5000)";
                                Cell = Cell + "\r\n}";
                                //
                                Cell = Cell + "\r\nfunction requestStatus" + RowPointer + "() {";
                                Cell = Cell + "\r\n  cj.ajax.getNameValue(statusHandler" + RowPointer + ",'" + RemoteKey + "');";
                                Cell = Cell + "\r\n}";
                                Cell = Cell + "\r\nrequestStatus" + RowPointer + "();";
                                Cell = Cell + "\r\n</script>";
                                //
                                Cells[RowPointer, 4] = Cell;
                            } else if (ResultMessage == "ok") {
                                Cells[RowPointer, 4] = "<div id=\"pending" + RowPointer + "\">" + LinkPrefix + csXfer.csGetText(CS, "filename") + LinkSuffix + "</div>";
                            } else {
                                Cells[RowPointer, 4] = "<div id=\"pending" + RowPointer + "\"><a href=\"javascript:alert('" + GenericController.EncodeJavascriptStringSingleQuote(ResultMessage) + ";return false');\">error</a></div>";
                            }
                            RowPointer = RowPointer + 1;
                            csXfer.csGoNext(CS);
                        }
                    }
                    csXfer.csClose();
                    Tab0.Add(HtmlController.inputHidden("RowCnt", RowPointer));
                    Cell = AdminUIController.getReport(core, RowPointer, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, "ccPanel");
                    Tab0.Add(Cell);
                    //Tab0.Add( "<div style=""height:200px;"">" & Cell & "</div>"
                    //        '
                    //        ' Build RequestContent Form
                    //        '
                    //        Tab1.Add( "<p>Use this form to request a download. Select the criteria for the download and click the [Request Download] button. The request should then appear on the requested download list in the other tab. When the download has been created, it will be become available.</p>")
                    //        '
                    //        Tab1.Add( "<table border=""0"" cellpadding=""3"" cellspacing=""0"" width=""100%"">")
                    //        '
                    //        Call Tab1.Add("<tr>")
                    //        Call Tab1.Add("<td align=right>Content</td>")
                    //        Call Tab1.Add("<td>" & core.htmldoc.main_GetFormInputSelect2("ContentID", ContentID, "Content", "", "", "", IsEmptyList) & "</td>")
                    //        Call Tab1.Add("</tr>")
                    //        '
                    //        Call Tab1.Add("<tr>")
                    //        Call Tab1.Add("<td align=right>Format</td>")
                    //        Call Tab1.Add("<td><select name=Format value=""" & Format & """><option value=CSV>CSV</option><option name=XML value=XML>XML</option></select></td>")
                    //        Call Tab1.Add("</tr>")
                    //        '
                    //        Call Tab1.Add("" _
                    //            & "<tr>" _
                    //            & "<td width=""120""><img alt=""space"" src=""/ContensiveBase/images/spacer.gif"" width=""120"" height=""1""></td>" _
                    //            & "<td width=""100%"">&nbsp;</td>" _
                    //            & "</tr>" _
                    //            & "</table>")
                    //        '
                    //        ' Build and add tabs
                    //        '
                    //        Call core.htmldoc.main_AddLiveTabEntry("Current&nbsp;Downloads", Tab0.Text, "ccAdminTab")
                    //        Call core.htmldoc.main_AddLiveTabEntry("Request&nbsp;New&nbsp;Download", Tab1.Text, "ccAdminTab")
                    //        Content = core.htmldoc.main_GetLiveTabs()
                    Content = Tab0.Text;
                    //
                    ButtonListLeft = ButtonCancel + "," + ButtonRefresh + "," + ButtonDelete;
                    //ButtonListLeft = ButtonCancel & "," & ButtonRefresh & "," & ButtonDelete & "," & ButtonRequestDownload
                    ButtonListRight = "";
                    Content = Content + HtmlController.inputHidden(rnAdminSourceForm, AdminFormDownloads);
                }
                //
                Caption = "Download Manager";
                Description = ""
                    + "<p>The Download Manager holds all downloads requested from anywhere on the website. It also provides tools to request downloads from any Content.</p>"
                    + "<p>To add a new download of any content in Contensive, click Export on the filter tab of the content listing page. To add a new download from a SQL statement, use Custom Reports under Reports on the Navigator.</p>";
                ContentPadding = 0;
                tempGetForm_Downloads = AdminUIController.getBody(core, Caption, ButtonListLeft, ButtonListRight, true, true, Description, ContentSummary, ContentPadding, Content);
                //
                core.html.addTitle(Caption);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetForm_Downloads;
        }
        //
    }
}
