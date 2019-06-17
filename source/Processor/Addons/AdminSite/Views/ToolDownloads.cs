
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
                string Button = core.docProperties.getText(RequestNameButton);
                if (Button == ButtonCancel) {
                    return core.webServer.redirect("/" + core.appConfig.adminRoute, "Downloads, Cancel Button Pressed");
                }
                string Description = "";
                string ButtonListLeft = "";
                string ButtonListRight = "";
                string Content = "";
                //
                if (!core.session.isAuthenticatedAdmin()) {
                    //
                    // Must be a developer
                    //
                    ButtonListLeft = ButtonCancel;
                    ButtonListRight = "";
                    Content = Content + AdminUIController.getFormBodyAdminOnly();
                } else {
                    int ContentID = core.docProperties.getInteger("ContentID");
                    string Format = core.docProperties.getText("Format");
                    //adminUIController Adminui = new adminUIController(core);
                    string SQLFieldName = "SQLQuery";
                    //
                    // Process Requests
                    //
                    if (!string.IsNullOrEmpty(Button)) {
                        var contentMetadata = Contensive.Processor.Models.Domain.ContentMetadataModel.create(core, ContentID);
                        //string Criteria = null;
                        int RowPtr = 0;
                        //string TableName = null;
                        string Filename = null;
                        string Name = null;
                        switch (Button) {
                            case ButtonDelete: {
                                    int RowCnt = core.docProperties.getInteger("RowCnt");
                                    if (RowCnt > 0) {
                                        for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                                            if (core.docProperties.getBoolean("Row" + RowPtr)) {
                                                MetadataController.deleteContentRecord(core, "Tasks", core.docProperties.getInteger("RowID" + RowPtr));
                                            }
                                        }
                                    }
                                }
                                break;
                            case ButtonRequestDownload: {
                                    //
                                    // Request the download again
                                    int RowCnt = core.docProperties.getInteger("RowCnt");
                                    if (RowCnt > 0) {
                                        for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                                            if (core.docProperties.getBoolean("Row" + RowPtr)) {
                                                using (var CSSrc = new CsModel(core)) {
                                                    if (CSSrc.openRecord("Tasks", core.docProperties.getInteger("RowID" + RowPtr))) {
                                                        using (var CSDst = new CsModel(core)) {
                                                            if (CSDst.insert("Tasks")) {
                                                                CSDst.set("Name", CSSrc.getText("name"));
                                                                CSDst.set(SQLFieldName, CSSrc.getText(SQLFieldName));
                                                                if (GenericController.vbLCase(CSSrc.getText("command")) == "xml") {
                                                                    CSDst.set("Filename", "DupDownload_" + encodeText(GenericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(GenericController.GetRandomInteger(core)) + ".xml");
                                                                    CSDst.set("Command", "BUILDXML");
                                                                } else {
                                                                    CSDst.set("Filename", "DupDownload_" + encodeText(GenericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(GenericController.GetRandomInteger(core)) + ".csv");
                                                                    CSDst.set("Command", "BUILDCSV");
                                                                }
                                                            }
                                                            CSDst.close();
                                                        }
                                                    }
                                                }
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
                                        using (var csData = new CsModel(core)) {
                                            csData.insert("Tasks");
                                            if (csData.ok()) {
                                                //ContentName = MetadataController.getContentNameByID(core, ContentID);
                                                //TableName = MetadataController.getContentTablename(core, ContentName);
                                                //Criteria = contentMetadata.legacyContentControlCriteria;
                                                Name = "CSV Download, " + contentMetadata.name;
                                                Filename = GenericController.vbReplace(contentMetadata.name, " ", "") + "_" + encodeText(GenericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(GenericController.GetRandomInteger(core)) + ".csv";
                                                csData.set("Name", Name);
                                                csData.set("Filename", Filename);
                                                csData.set("Command", "BUILDCSV");
                                                csData.set(SQLFieldName, "SELECT * from " + contentMetadata.tableName + " where " + contentMetadata.legacyContentControlCriteria);
                                                Description = Description + "<p>Your CSV Download has been requested.</p>";
                                            }
                                            csData.close();
                                        }
                                        Format = "";
                                        ContentID = 0;
                                    } else if (Format == "XML") {
                                        using (var csData = new CsModel(core)) {
                                            csData.insert("Tasks");
                                            if (csData.ok()) {
                                                //ContentName = MetadataController.getContentNameByID(core, ContentID);
                                                //TableName = MetadataController.getContentTablename(core, ContentName);
                                                //Criteria = contentMetadata.legacyContentControlCriteria;
                                                Name = "XML Download, " + contentMetadata.name;
                                                Filename = GenericController.vbReplace(contentMetadata.name, " ", "") + "_" + encodeText(GenericController.dateToSeconds(core.doc.profileStartTime)) + encodeText(GenericController.GetRandomInteger(core)) + ".xml";
                                                csData.set("Name", Name);
                                                csData.set("Filename", Filename);
                                                csData.set("Command", "BUILDXML");
                                                csData.set(SQLFieldName, "SELECT * from " + contentMetadata.tableName + " where " + contentMetadata.legacyContentControlCriteria);
                                                Description = Description + "<p>Your XML Download has been requested.</p>";
                                            }
                                        }
                                        Format = "";
                                        ContentID = 0;
                                    }
                                }
                                break;
                        }
                    }
                    //
                    // Build Tab0
                    //
                    //Tab0.Add( "<p>The following is a list of available downloads</p>")
                    //
                    string RQS = core.doc.refreshQueryString;
                    int PageSize = core.docProperties.getInteger(RequestNamePageSize);
                    if (PageSize == 0) {
                        PageSize = 50;
                    }
                    int PageNumber = core.docProperties.getInteger(RequestNamePageNumber);
                    if (PageNumber == 0) {
                        PageNumber = 1;
                    }
                    string AdminURL = "/" + core.appConfig.adminRoute;
                    int TopCount = PageNumber * PageSize;
                    //
                    const int ColumnCnt = 5;
                    //
                    // Setup Headings
                    //
                    string[] ColCaption = new string[ColumnCnt + 1];
                    string[] ColAlign = new string[ColumnCnt + 1];
                    string[] ColWidth = new string[ColumnCnt + 1];
                    string[,] Cells = new string[PageSize + 1, ColumnCnt + 1];
                    int ColumnPtr = 0;
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
                    int RowPointer = 0;
                    int DataRowCount = 0;
                    string Cell = null;
                    //
                    //   Get Data
                    //
                    using (var csData = new CsModel(core)) {
                        string SQL = "select M.Name as CreatedByName, T.* from ccTasks T left join ccMembers M on M.ID=T.CreatedBy where (T.Command='BuildCSV')or(T.Command='BuildXML') order by T.DateAdded Desc";
                        csData.openSql(SQL, "Default", PageSize, PageNumber);
                        RowPointer = 0;
                        if (!csData.ok()) {
                            Cells[0, 1] = "There are no download requests";
                            RowPointer = 1;
                        } else {
                            DataRowCount = csData.getRowCount();
                            string LinkPrefix = "<a href=\"" + core.appConfig.cdnFileUrl;
                            string LinkSuffix = "\" target=_blank>Available</a>";
                            while (csData.ok() && (RowPointer < PageSize)) {
                                int RecordID = csData.getInteger("ID");
                                DateTime DateCompleted = default(DateTime);
                                DateCompleted = csData.getDate("DateCompleted");
                                string ResultMessage = csData.getText("ResultMessage");
                                Cells[RowPointer, 0] = HtmlController.checkbox("Row" + RowPointer) + HtmlController.inputHidden("RowID" + RowPointer, RecordID);
                                Cells[RowPointer, 1] = csData.getText("name");
                                Cells[RowPointer, 2] = csData.getText("CreatedByName");
                                Cells[RowPointer, 3] = csData.getDate("DateAdded").ToShortDateString();
                                if (DateCompleted == DateTime.MinValue) {
                                    string RemoteKey = RemoteQueryController.main_GetRemoteQueryKey(core, "select DateCompleted,filename,resultMessage from cctasks where id=" + RecordID, "default", 1);
                                    Cell = "";
                                    Cell = Cell + Environment.NewLine + "<div id=\"pending" + RowPointer + "\">Pending <img src=\"/ContensiveBase/images/ajax-loader-small.gif\" width=16 height=16></div>";
                                    //
                                    Cell = Cell + Environment.NewLine + "<script>";
                                    Cell = Cell + Environment.NewLine + "function statusHandler" + RowPointer + "(results) {";
                                    Cell = Cell + Environment.NewLine + " var jo,isDone=false;";
                                    Cell = Cell + Environment.NewLine + " eval('jo='+results);";
                                    Cell = Cell + Environment.NewLine + " if (jo){";
                                    Cell = Cell + Environment.NewLine + "  if(jo.DateCompleted) {";
                                    Cell = Cell + Environment.NewLine + "    var dst=document.getElementById('pending" + RowPointer + "');";
                                    Cell = Cell + Environment.NewLine + "    isDone=true;";
                                    Cell = Cell + Environment.NewLine + "    if(jo.resultMessage=='OK') {";
                                    Cell = Cell + Environment.NewLine + "      dst.innerHTML='" + LinkPrefix + "'+jo.filename+'" + LinkSuffix + "';";
                                    Cell = Cell + Environment.NewLine + "    }else{";
                                    Cell = Cell + Environment.NewLine + "      dst.innerHTML='error';";
                                    Cell = Cell + Environment.NewLine + "    }";
                                    Cell = Cell + Environment.NewLine + "  }";
                                    Cell = Cell + Environment.NewLine + " }";
                                    Cell = Cell + Environment.NewLine + " if(!isDone) setTimeout(\"requestStatus" + RowPointer + "()\",5000)";
                                    Cell = Cell + Environment.NewLine + "}";
                                    //
                                    Cell = Cell + Environment.NewLine + "function requestStatus" + RowPointer + "() {";
                                    Cell = Cell + Environment.NewLine + "  cj.ajax.getNameValue(statusHandler" + RowPointer + ",'" + RemoteKey + "');";
                                    Cell = Cell + Environment.NewLine + "}";
                                    Cell = Cell + Environment.NewLine + "requestStatus" + RowPointer + "();";
                                    Cell = Cell + Environment.NewLine + "</script>";
                                    //
                                    Cells[RowPointer, 4] = Cell;
                                } else if (ResultMessage == "ok") {
                                    Cells[RowPointer, 4] = "<div id=\"pending" + RowPointer + "\">" + LinkPrefix + csData.getText("filename") + LinkSuffix + "</div>";
                                } else {
                                    Cells[RowPointer, 4] = "<div id=\"pending" + RowPointer + "\"><a href=\"javascript:alert('" + GenericController.EncodeJavascriptStringSingleQuote(ResultMessage) + ";return false');\">error</a></div>";
                                }
                                RowPointer = RowPointer + 1;
                                csData.goNext();
                            }
                        }
                        csData.close();
                    }
                    StringBuilderLegacyController Tab0 = new StringBuilderLegacyController();
                    Tab0.Add(HtmlController.inputHidden("RowCnt", RowPointer));
                    string PreTableCopy = "";
                    string PostTableCopy = "";
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
                string Caption = "Download Manager";
                Description = ""
                    + "<p>The Download Manager holds all downloads requested from anywhere on the website. It also provides tools to request downloads from any Content.</p>"
                    + "<p>To add a new download of any content in Contensive, click Export on the filter tab of the content listing page. To add a new download from a SQL statement, use Custom Reports under Reports on the Navigator.</p>";
                int ContentPadding = 0;
                string ContentSummary = "";
                tempGetForm_Downloads = AdminUIController.getBody(core, Caption, ButtonListLeft, ButtonListRight, true, true, Description, ContentSummary, ContentPadding, Content);
                //
                core.html.addTitle(Caption);
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return tempGetForm_Downloads;
        }
        //
    }
}
