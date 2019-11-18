
using System;
using System.Collections.Generic;

using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
using System.Text;
using Contensive.Processor.Exceptions;
using Contensive.Processor;
using Contensive.Models;
using Contensive.Models.Db;
using System.Globalization;
using Contensive.Processor.Addons.AdminSite.Models;

namespace Contensive.Processor.Addons.AdminSite.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// UI rendering for Admin
    /// REFACTOR - add  try-catch
    /// not IDisposable - not contained classes that need to be disposed
    /// </summary>
    public class AdminUIController {
        //
        //====================================================================================================
        /// <summary>
        /// structure used in admin edit forms at the top
        /// </summary>
        public class RecordEditHeaderInfoClass {
            public string recordName;
            public int recordId;
            public DateTime recordLockExpiresDate;
            public int recordLockById;
        }
        //
        public class EditButtonBarInfoClass {
            public bool allowDelete = false;
            public bool allowCancel = false;
            public bool allowSave = false;
            public bool allowAdd = false;
            public bool allowActivate = false;
            public bool allowSendTest = false;
            public bool allowSend = false;
            public bool hasChildRecords = false;
            public bool isPageContent = false;
            public bool allowMarkReviewed = false;
            public bool allowRefresh = false;
            public bool allowCreateDuplicate = false;
            public bool allowDeactivate = false;
            public int contentId = 0;
        }
        //
        //====================================================================================================
        /// <summary>
        /// admin filter methods (includes, equals, greaterthan, etc)
        /// </summary>
        public enum FindWordMatchEnum {
            MatchIgnore = 0,
            MatchEmpty = 1,
            MatchNotEmpty = 2,
            MatchGreaterThan = 3,
            MatchLessThan = 4,
            matchincludes = 5,
            MatchEquals = 6,
            MatchTrue = 7,
            MatchFalse = 8
        }
        private enum SortingStateEnum {
            NotSortable = 0,
            SortableSetAZ = 1,
            SortableSetza = 2,
            SortableNotSet = 3
        }
        public class ButtonMetadata {
            public string name = "button";
            public string value = "";
            public string classList = "";
            public bool isDelete = false;
            public bool isClose = false;
            public bool isAdd = false;
        }
        //
        //====================================================================================================
        /// <summary>
        /// The title and description section at the top of a tool
        /// </summary>
        /// <param name="core"></param>
        /// <param name="Title"></param>
        /// <param name="Description"></param>
        /// <returns></returns>
        public static string getSectionHeader(CoreController core, string Title, string Description) {
            try {
                string result = getHeaderTitleDescription(Title, Description);
                if (!core.doc.userErrorList.Count.Equals(0)) { result += HtmlController.div(ErrorController.getUserError(core)); }
                result = HtmlController.div(result, "p-2");
                return HtmlController.section(result);
                //return HtmlController.div(result, "ccAdminTitleBar");
            } catch (Exception ex) {
                LogController.logError(core, ex);
                return "";
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// The button bar section at the top and bottom of the edit page
        /// </summary>
        public static string getSectionButtonBarForEdit(CoreController core, EditButtonBarInfoClass info) {
            try {
                //
                LogController.logTrace(core, "getSectionButtonBarForEdit, enter");
                //
                string buttonsLeft = "";
                string buttonsRight = "";
                if (info.allowCancel) buttonsLeft += getButtonPrimary(ButtonCancel, "return processSubmit(this);");
                if (info.allowRefresh) buttonsLeft += getButtonPrimary(ButtonRefresh);
                if (info.allowSave) {
                    buttonsLeft += getButtonPrimary(ButtonOK, "return processSubmit(this);");
                    buttonsLeft += getButtonPrimary(ButtonSave, "return processSubmit(this);");
                    if (info.allowAdd) buttonsLeft += getButtonPrimary(ButtonSaveAddNew, "return processSubmit(this);");
                }
                if (info.allowSendTest) buttonsLeft += getButtonPrimary(ButtonSendTest, "Return processSubmit(this)");
                if (info.allowSend) buttonsLeft += getButtonPrimary(ButtonSend, "Return processSubmit(this)");
                if (info.allowMarkReviewed) buttonsLeft += getButtonPrimary(ButtonMarkReviewed);
                if (info.allowCreateDuplicate) buttonsLeft += getButtonPrimary(ButtonCreateDuplicate, "return processSubmit(this)");
                if (info.allowActivate) buttonsLeft += getButtonPrimary(ButtonActivate, "return processSubmit(this)");
                if (info.allowDeactivate) buttonsLeft += getButtonPrimary(ButtonDeactivate, "return processSubmit(this)");
                string JSOnClick = "if(!DeleteCheck())return false;";
                if (info.isPageContent) {
                    JSOnClick = "if(!DeletePageCheck())return false;";
                } else if (info.hasChildRecords) {
                    JSOnClick = "if(!DeleteCheckWithChildren())return false;";
                }
                buttonsRight += getButtonDanger(ButtonDelete, JSOnClick, !info.allowDelete);
                if(core.session.isAuthenticatedAdmin()) {
                    buttonsRight += getButtonDanger(ButtonModifyEditForm, "window.location='?af=105&button=select&contentid=" + info.contentId + "';return false;", info.contentId.Equals(0));
                }
                //
                LogController.logTrace(core, "getButtonBarForEdit, exit");
                //
                return getSectionButtonBar(core, buttonsLeft, buttonsRight);
            } catch (Exception ex) {
                LogController.logError(core, ex);
                return toolExceptionMessage;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a panel header with the header message reversed out of the left
        /// </summary>
        /// <param name="core"></param>
        /// <param name="leftSideMessage"></param>
        /// <param name="rightSideMessage"></param>
        /// <returns></returns>
        public static string getHeader(CoreController core, string leftSideMessage, string rightSideMessage = "", string rightSideNavHtml = "") {
            string result = Processor.Properties.Resources.adminNavBarHtml;
            result = result.Replace("{navBrand}", core.appConfig.name);
            result = result.Replace("{leftSideMessage}", leftSideMessage);
            result = result.Replace("{rightSideMessage}", rightSideMessage);
            result = result.Replace("{rightSideNavHtml}", rightSideNavHtml);
            return result;
            //string s = "";
            //try {
            //    if (string.IsNullOrEmpty(rightSideMessage)) {
            //        rightSideMessage = core.doc.profileStartTime.ToString("G");
            //    }
            //    if (isInStr(1, leftSideMessage + rightSideMessage, Environment.NewLine)) {
            //        s = ""
            //            + "\r<td width=\"50%\" valign=Middle class=\"cchLeft\">"
            //            + nop(leftSideMessage) + "\r</td>"
            //            + "\r<td width=\"50%\" valign=Middle class=\"cchRight\">"
            //            + nop(rightSideMessage) + "\r</td>";
            //    } else {
            //        s = ""
            //            + "\r<td width=\"50%\" valign=Middle class=\"cchLeft\">" + leftSideMessage + "</td>"
            //            + "\r<td width=\"50%\" valign=Middle class=\"cchRight\">" + rightSideMessage + "</td>";
            //    }
            //    s = ""
            //        + "\r<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr>"
            //        + nop(s) + "\r</tr></table>"
            //        + "";
            //    s = ""
            //        + "\r<div class=\"ccHeaderCon\">"
            //        + nop(s) + "\r</div>"
            //        + "";
            //    //
            //} catch (Exception ex) {
            //    LogController.handleError(core, ex);
            //}
            //return s;
        }
        //
        //====================================================================================================
        public static string getButtonHtmlFromList(CoreController core, List<ButtonMetadata> ButtonList, bool AllowDelete, bool AllowAdd) {
            string s = "";
            try {
                foreach (ButtonMetadata button in ButtonList) {

                    if (button.isDelete) {
                        s += getButtonDanger(button.value, "if(!DeleteCheck()) { return false; }", !AllowDelete);
                    } else if (button.isAdd) {
                        s += getButtonPrimary(button.value, "return processSubmit(this);", !AllowAdd);
                    } else if (button.isClose) {
                        s += getButtonPrimary(button.value, "window.close();");
                    } else {
                        s += getButtonPrimary(button.value);
                    }

                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return s;
        }
        //
        //====================================================================================================
        public static string getButtonHtmlFromCommaList(CoreController core, string ButtonList, bool AllowDelete, bool AllowAdd, string ButtonName) {
            return getButtonHtmlFromList(core, buttonStringToButtonList(ButtonList), AllowDelete, AllowAdd);
        }
        //
        //====================================================================================================
        /// <summary>
        /// The button bar section at the top and bottom of a tool
        /// </summary>
        /// <param name="leftButtonHtml"></param>
        /// <param name="rightButtonHtml"></param>
        /// <returns></returns>
        public static string getSectionButtonBar(CoreController core, string leftButtonHtml, string rightButtonHtml) {
            if (string.IsNullOrWhiteSpace(leftButtonHtml + rightButtonHtml)) {
                return "";
            } else if (string.IsNullOrWhiteSpace(rightButtonHtml)) {
                return HtmlController.section( HtmlController.div(leftButtonHtml, "border bg-white p-2"));
            } else {
                //return "<div class=\"border bg-white p-2\">" + leftButtonHtml + "<div class=\"float-right\">" + rightButtonHtml + "</div></div>";
                return HtmlController.section(HtmlController.div(leftButtonHtml + HtmlController.div(rightButtonHtml, "float-right"), "border bg-white p-2"));
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// get button bar for the index form
        /// </summary>
        /// <param name="core"></param>
        /// <param name="AllowAdd"></param>
        /// <param name="AllowDelete"></param>
        /// <param name="pageNumber"></param>
        /// <param name="recordsPerPage"></param>
        /// <param name="recordCnt"></param>
        /// <param name="contentName"></param>
        /// <returns></returns>
        public static string getForm_Index_ButtonBar(CoreController core, bool AllowAdd, bool AllowDelete, int pageNumber, int recordsPerPage, int recordCnt, string contentName) {
            string result = "";
            string LeftButtons = "";
            string RightButtons = "";
            LeftButtons = LeftButtons + AdminUIController.getButtonPrimary(ButtonCancel);
            LeftButtons += AdminUIController.getButtonPrimary(ButtonRefresh);
            if (AllowAdd) {
                LeftButtons += AdminUIController.getButtonPrimary(ButtonAdd, "");
            } else {
                LeftButtons += AdminUIController.getButtonPrimary(ButtonAdd, "", true);
            }
            LeftButtons += "<span class=\"custom-divider-vertical\">&nbsp&nbsp&nbsp</span>";
            if (pageNumber == 1) {
                LeftButtons += AdminUIController.getButtonPrimary(ButtonFirst, "", true);
                LeftButtons += AdminUIController.getButtonPrimary(ButtonPrevious, "", true);
            } else {
                LeftButtons += AdminUIController.getButtonPrimary(ButtonFirst);
                LeftButtons += AdminUIController.getButtonPrimary(ButtonPrevious);
            }
            if (recordCnt > (pageNumber * recordsPerPage)) {
                LeftButtons += AdminUIController.getButtonPrimary(ButtonNext);
            } else {
                LeftButtons += AdminUIController.getButtonPrimary(ButtonNext, "", true);
            }
            if (AllowDelete) {
                RightButtons += AdminUIController.getButtonDanger(ButtonDelete, "if(!DeleteCheck())return false;");
            } else {
                RightButtons += AdminUIController.getButtonDanger(ButtonDelete, "", true);
            }
            result = getSectionButtonBar(core, LeftButtons, RightButtons);
            return result;
            //return adminUIController.getForm_index_pageNavigation(core, LeftButtons, RightButtons, pageNumber, recordsPerPage, PageCount, recordCnt, contentName);
        }
        //
        //====================================================================================================
        public static string getForm_index_pageNavigation(CoreController core, int PageNumber, int recordsPerPage, int recordCnt, string contentName) {
            string result = null;
            try {
                int PageCount = 1;
                if (recordCnt > 1) {
                    PageCount = encodeInteger(1 + encodeInteger(Math.Floor(encodeNumber((recordCnt - 1) / recordsPerPage))));
                }

                int NavStart = PageNumber - 9;
                if (NavStart < 1) {
                    NavStart = 1;
                }
                int NavEnd = NavStart + 20;
                if (NavEnd > PageCount) {
                    NavEnd = PageCount;
                    NavStart = NavEnd - 20;
                    if (NavStart < 1) {
                        NavStart = 1;
                    }
                }
                string Nav = "";
                if (NavStart > 1) {
                    Nav = Nav + "<li onclick=\"bbj(this);\">1</li><li class=\"delim\">&#171;</li>";
                }
                for (int Ptr = NavStart; Ptr <= NavEnd; Ptr++) {
                    Nav = Nav + "<li onclick=\"bbj(this);\">" + Ptr + "</li>";
                }
                if (NavEnd < PageCount) {
                    Nav = Nav + "<li class=\"delim\">&#187;</li><li onclick=\"bbj(this);\">" + PageCount + "</li>";
                }
                Nav = GenericController.vbReplace(Nav, ">" + PageNumber + "<", " class=\"hit\">" + PageNumber + "<");
                string recordDetails = "";
                switch (recordCnt) {
                    case 0:
                        recordDetails = "no records found";
                        break;
                    case 1:
                        recordDetails = "1 record found";
                        break;
                    default:
                        recordDetails = recordCnt + " records found";
                        break;
                }
                Nav = "" + "\r<script language=\"javascript\">function bbj(p){document.getElementsByName('indexGoToPage')[0].value=p.innerHTML;document.adminForm.submit();}</script>"
                    + "\r<div class=\"ccJumpCon\">"
                    + cr2 + "<ul><li class=\"caption\">" + recordDetails + ", page</li>"
                    + cr3 + Nav + cr2 + "</ul>"
                    + "\r</div>";
                result += Nav;
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        public static string getToolBody(CoreController core, string title, string ButtonCommaListLeft, string ButtonCommaListRight, bool AllowAdd, bool AllowDelete, string description, string ContentSummary, int ContentPadding, string Content) {
            string body = "";
            try {
                string CellContentSummary = "";
                //
                // Build ButtonBar
                //
                string LeftHtmlButtonList = "";
                if (!string.IsNullOrEmpty(ButtonCommaListLeft.Trim(' '))) {
                    LeftHtmlButtonList = getButtonHtmlFromCommaList(core, ButtonCommaListLeft, AllowDelete, AllowAdd, "Button");
                }
                string RightHtmlButtonList = "";
                if (!string.IsNullOrEmpty(ButtonCommaListRight.Trim(' '))) {
                    RightHtmlButtonList = getButtonHtmlFromCommaList(core, ButtonCommaListRight, AllowDelete, AllowAdd, "Button");
                }
                //string ButtonBar = getButtonBar(core, LeftHtmlButtonList, RightHtmlButtonList);
                if (!string.IsNullOrEmpty(ContentSummary)) {
                    CellContentSummary = ""
                        + "\r<div class=\"ccPanelBackground\" style=\"padding:10px;\">"
                        + core.html.getPanel(ContentSummary, "ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                        + "\r</div>";
                }
                body += ""
                    + getSectionHeader(core, title, description)
                    + CellContentSummary
                    + "<div style=\"padding:" + ContentPadding + "px;\">" + Content + "\r</div>"
                    + "";
                //
                // -- assemble form
                body = getToolForm(core, body, ButtonCommaListLeft, ButtonCommaListRight);
                return body;
                //return HtmlController.formMultipart(core, body, core.doc.refreshQueryString, "", "ccForm");
            } catch (Exception ex) {
                LogController.logError(core, ex);
                return toolExceptionMessage;
            }
        }
        //
        //====================================================================================================
        public static string getEditSubheadRow(CoreController core, string Caption) {
            return "<tr><td colspan=2 class=\"ccAdminEditSubHeader\">" + Caption + "</td></tr>";
        }
        //
        //====================================================================================================
        // 
        /// <summary>
        /// GetEditPanel, An edit panel is a section of an admin page, under a subhead. When in tab mode, the subhead is blocked, and the panel is assumed to go in its own tab windows
        /// </summary>
        /// <param name="core"></param>
        /// <param name="AllowHeading"></param>
        /// <param name="PanelHeading"></param>
        /// <param name="PanelDescription"></param>
        /// <param name="PanelBody"></param>
        /// <returns></returns>
        public static string getEditPanel(CoreController core, bool AllowHeading, string PanelHeading, string PanelDescription, string PanelBody) {
            var result = new StringBuilder();
            try {
                result.Append("<div class=\"ccPanel3DReverse ccAdminEditBody\">");
                result.Append((AllowHeading && (!string.IsNullOrEmpty(PanelHeading))) ? "<h3 class=\"p-2 ccAdminEditHeading\">" + PanelHeading + "</h3>" : "");
                result.Append((!string.IsNullOrEmpty(PanelDescription)) ? "<p class=\"p-2 ccAdminEditDescription\">" + PanelDescription + "</p>" : "");
                result.Append(PanelBody + "</div>");
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result.ToString();
        }
        //
        //====================================================================================================
        // Edit Table
        public static string editTable(string innerHtml) {
            return ""
                + "<table border=0 cellpadding=3 cellspacing=0 width=\"100%\">"
                    + innerHtml
                    + "<tr>"
                        + "<td width=20%><img alt=\"space\" src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/spacer.gif\" width=\"100%\" height=1 ></td>"
                        + "<td width=80%><img alt=\"space\" src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/spacer.gif\" width=\"100%\" height=1 ></td>"
                    + "</tr>"
                + "</table>";
        }
        //
        //====================================================================================================
        private static string getReport_Cell(CoreController core, string Copy, string Align, int Columns, int RowPointer) {
            string tempGetReport_Cell = null;
            string iAlign = null;
            string Style = null;
            string CellCopy = null;
            //
            iAlign = encodeEmpty(Align, "left");
            //
            if ((RowPointer % 2) > 0) {
                Style = "ccAdminListRowOdd";
            } else {
                Style = "ccAdminListRowEven";
            }
            //
            tempGetReport_Cell = Environment.NewLine + "<td valign=\"middle\" align=\"" + iAlign + "\" class=\"" + Style + "\"";
            if (Columns > 1) {
                tempGetReport_Cell = tempGetReport_Cell + " colspan=\"" + Columns + "\"";
            }
            //
            CellCopy = Copy;
            if (string.IsNullOrEmpty(CellCopy)) {
                CellCopy = "&nbsp;";
            }
            return tempGetReport_Cell + "><span class=\"ccSmall\">" + CellCopy + "</span></td>";
        }
        //
        //====================================================================================================
        //   Report Cell Header
        //       ColumnPtr       :   0 based column number
        //       Title
        //       Width           :   if just a number, assumed to be px in style and an image is added
        //                       :   if 00px, image added with the numberic part
        //                       :   if not number, added to style as is
        //       align           :   style value
        //       ClassStyle      :   class
        //       RQS
        //       SortingState
        //
        private static string getReport_CellHeader(CoreController core, int ColumnPtr, string Title, string Width, string Align, string ClassStyle, string RefreshQueryString, SortingStateEnum SortingState) {
            string result = "";
            try {
                string Copy = "&nbsp;";
                if (!string.IsNullOrEmpty(Title)) { Copy = GenericController.vbReplace(Title, " ", "&nbsp;"); }
                string Style = "VERTICAL-ALIGN:bottom;";
                if (!string.IsNullOrEmpty(Align)) { Style += "TEXT-ALIGN:" + Align + ";"; }
                switch (SortingState) {
                    case SortingStateEnum.SortableNotSet: {
                            string QS = GenericController.modifyQueryString(RefreshQueryString, "ColSort", ((int)SortingStateEnum.SortableSetAZ).ToString(), true);
                            QS = GenericController.modifyQueryString(QS, "ColPtr", ColumnPtr.ToString(), true);
                            Copy = "<a href=\"?" + QS + "\" title=\"Sort A-Z\" class=\"ccAdminListCaption\">" + Copy + "</a>";
                            break;
                        }
                    case SortingStateEnum.SortableSetza: {
                            string QS = GenericController.modifyQueryString(RefreshQueryString, "ColSort", ((int)SortingStateEnum.SortableSetAZ).ToString(), true);
                            QS = GenericController.modifyQueryString(QS, "ColPtr", ColumnPtr.ToString(), true);
                            Copy = "<a href=\"?" + QS + "\" title=\"Sort A-Z\" class=\"ccAdminListCaption\">" + Copy + "<img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/arrowup.gif\" width=8 height=8 border=0></a>";
                            break;
                        }
                    case SortingStateEnum.SortableSetAZ: {
                            string QS = GenericController.modifyQueryString(RefreshQueryString, "ColSort", ((int)SortingStateEnum.SortableSetza).ToString(), true);
                            QS = GenericController.modifyQueryString(QS, "ColPtr", ColumnPtr.ToString(), true);
                            Copy = "<a href=\"?" + QS + "\" title=\"Sort Z-A\" class=\"ccAdminListCaption\">" + Copy + "<img src=\"https://s3.amazonaws.com/cdn.contensive.com/assets/20191111/images/arrowdown.gif\" width=8 height=8 border=0></a>";
                            break;
                        }
                }
                //
                if (!string.IsNullOrEmpty(Width)) {
                    Style = Style + "width:" + Width + ";";
                }
                result = Environment.NewLine + "<td style=\"" + Style + "\" class=\"" + ClassStyle + "\">" + Copy + "</td>";
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the integer column ptr of the column last selected
        /// </summary>
        /// <param name="core"></param>
        /// <param name="DefaultSortColumnPtr"></param>
        /// <returns></returns>
        public static int getReportSortColumnPtr(CoreController core, int DefaultSortColumnPtr) {
            int tempGetReportSortColumnPtr = 0;
            string VarText;
            //
            VarText = core.docProperties.getText("ColPtr");
            tempGetReportSortColumnPtr = GenericController.encodeInteger(VarText);
            if ((tempGetReportSortColumnPtr == 0) && (VarText != "0")) {
                tempGetReportSortColumnPtr = DefaultSortColumnPtr;
            }
            return tempGetReportSortColumnPtr;
        }
        //
        //====================================================================================================
        //   Get Sort Column Type
        //
        //   returns the integer for the type of sorting last requested
        //       0 = nothing selected
        //       1 = sort A-Z
        //       2 = sort Z-A
        //
        //   Orderby is generated by the selection of headers captions by the user
        //   It is up to the calling program to call GetReportOrderBy to get the orderby and use it in the query to generate the cells
        //   This call returns a comma delimited list of integers representing the columns to sort
        //
        public static int getReportSortType(CoreController core) {
            int tempGetReportSortType = 0;
            string VarText;
            //
            VarText = core.docProperties.getText("ColPtr");
            if ((encodeInteger(VarText) != 0) || (VarText == "0")) {
                //
                // A valid ColPtr was found
                //
                tempGetReportSortType = core.docProperties.getInteger("ColSort");
            } else {
                tempGetReportSortType = (int)SortingStateEnum.SortableSetAZ;
            }
            return tempGetReportSortType;
        }
        //
        //====================================================================================================
        public static string getReport(CoreController core, int RowCount, string[] ColCaption, string[] ColAlign, string[] ColWidth, string[,] Cells, int PageSize, int PageNumber, string PreTableCopy, string PostTableCopy, int DataRowCount, string ClassStyle) {
            string result = "";
            try {
                int ColCnt = Cells.GetUpperBound(1);
                bool[] ColSortable = new bool[ColCnt + 1];
                for (int Ptr = 0; Ptr < ColCnt; Ptr++) {
                    ColSortable[Ptr] = false;
                }
                //
                result = getReport2(core, RowCount, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, ClassStyle, ColSortable, 0);
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        public static string getReport2(CoreController core, int RowCount, string[] ColCaption, string[] ColAlign, string[] ColWidth, string[,] Cells, int PageSize, int PageNumber, string PreTableCopy, string PostTableCopy, int DataRowCount, string ClassStyle, bool[] ColSortable, int DefaultSortColumnPtr) {
            string result = "";
            try {
                string RQS = null;
                int RowBAse = 0;
                StringBuilderLegacyController Content = new StringBuilderLegacyController();
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                int ColumnCount = 0;
                int ColumnPtr = 0;
                string ColumnWidth = null;
                int RowPointer = 0;
                string WorkingQS = null;
                //
                int PageCount = 0;
                int PagePointer = 0;
                int LinkCount = 0;
                int ReportPageNumber = 0;
                int ReportPageSize = 0;
                string iClassStyle = null;
                int SortColPtr = 0;
                int SortColType = 0;
                //
                ReportPageNumber = PageNumber;
                if (ReportPageNumber == 0) {
                    ReportPageNumber = 1;
                }
                ReportPageSize = PageSize;
                if (ReportPageSize < 1) {
                    ReportPageSize = 50;
                }
                //
                iClassStyle = ClassStyle;
                if (string.IsNullOrEmpty(iClassStyle)) {
                    iClassStyle = "ccPanel";
                }
                ColumnCount = Cells.GetUpperBound(1);
                RQS = core.doc.refreshQueryString;
                //
                SortColPtr = getReportSortColumnPtr(core, DefaultSortColumnPtr);
                SortColType = getReportSortType(core);
                //
                // ----- Start the table
                Content.Add(HtmlController.tableStart(3, 1, 0));
                //
                // ----- Header
                Content.Add(Environment.NewLine + "<tr>");
                Content.Add(getReport_CellHeader(core, 0, "&nbsp", "50px", "Right", "ccAdminListCaption", RQS, SortingStateEnum.NotSortable));
                for (ColumnPtr = 0; ColumnPtr < ColumnCount; ColumnPtr++) {
                    ColumnWidth = ColWidth[ColumnPtr];
                    if (!ColSortable[ColumnPtr]) {
                        //
                        // not sortable column
                        //
                        Content.Add(getReport_CellHeader(core, ColumnPtr, ColCaption[ColumnPtr], ColumnWidth, ColAlign[ColumnPtr], "ccAdminListCaption", RQS, SortingStateEnum.NotSortable));
                    } else if (ColumnPtr == SortColPtr) {
                        //
                        // This is the current sort column
                        //
                        Content.Add(getReport_CellHeader(core, ColumnPtr, ColCaption[ColumnPtr], ColumnWidth, ColAlign[ColumnPtr], "ccAdminListCaption", RQS, (SortingStateEnum)SortColType));
                    } else {
                        //
                        // Column is sortable, but not selected
                        //
                        Content.Add(getReport_CellHeader(core, ColumnPtr, ColCaption[ColumnPtr], ColumnWidth, ColAlign[ColumnPtr], "ccAdminListCaption", RQS, SortingStateEnum.SortableNotSet));
                    }
                }
                Content.Add(Environment.NewLine + "</tr>");
                //
                // ----- Data
                //
                if (RowCount == 0) {
                    Content.Add(Environment.NewLine + "<tr>");
                    Content.Add(getReport_Cell(core, (RowBAse + RowPointer).ToString(), "right", 1, RowPointer));
                    Content.Add(getReport_Cell(core, "-- End --", "left", ColumnCount, 0));
                    Content.Add(Environment.NewLine + "</tr>");
                } else {
                    RowBAse = (ReportPageSize * (ReportPageNumber - 1)) + 1;
                    for (RowPointer = 0; RowPointer < RowCount; RowPointer++) {
                        Content.Add(Environment.NewLine + "<tr>");
                        Content.Add(getReport_Cell(core, (RowBAse + RowPointer).ToString(), "right", 1, RowPointer));
                        for (ColumnPtr = 0; ColumnPtr < ColumnCount; ColumnPtr++) {
                            Content.Add(getReport_Cell(core, Cells[RowPointer, ColumnPtr], ColAlign[ColumnPtr], 1, RowPointer));
                        }
                        Content.Add(Environment.NewLine + "</tr>");
                    }
                }
                //
                // ----- End Table
                //
                Content.Add(kmaEndTable);
                result += Content.Text;
                //
                // ----- Post Table copy
                //
                if ((DataRowCount / (double)ReportPageSize) != Math.Floor((DataRowCount / (double)ReportPageSize))) {
                    PageCount = encodeInteger((DataRowCount / (double)ReportPageSize) + 0.5);
                } else {
                    PageCount = encodeInteger(DataRowCount / (double)ReportPageSize);
                }
                if (PageCount > 1) {
                    result += "<br>Page " + ReportPageNumber + " (Row " + (RowBAse) + " of " + DataRowCount + ")";
                    if (PageCount > 20) {
                        PagePointer = ReportPageNumber - 10;
                    }
                    if (PagePointer < 1) {
                        PagePointer = 1;
                    }
                    if (PageCount > 1) {
                        result += "<br>Go to Page ";
                        if (PagePointer != 1) {
                            WorkingQS = core.doc.refreshQueryString;
                            WorkingQS = GenericController.modifyQueryString(WorkingQS, "GotoPage", "1", true);
                            result += "<a href=\"" + core.webServer.requestPage + "?" + WorkingQS + "\">1</A>...&nbsp;";
                        }
                        WorkingQS = core.doc.refreshQueryString;
                        WorkingQS = GenericController.modifyQueryString(WorkingQS, RequestNamePageSize, ReportPageSize.ToString(), true);
                        while ((PagePointer <= PageCount) && (LinkCount < 20)) {
                            if (PagePointer == ReportPageNumber) {
                                result += PagePointer + "&nbsp;";
                            } else {
                                WorkingQS = GenericController.modifyQueryString(WorkingQS, RequestNamePageNumber, PagePointer.ToString(), true);
                                result += "<a href=\"" + core.webServer.requestPage + "?" + WorkingQS + "\">" + PagePointer + "</A>&nbsp;";
                            }
                            PagePointer = PagePointer + 1;
                            LinkCount = LinkCount + 1;
                        }
                        if (PagePointer < PageCount) {
                            WorkingQS = GenericController.modifyQueryString(WorkingQS, RequestNamePageNumber, PageCount.ToString(), true);
                            result += "...<a href=\"" + core.webServer.requestPage + "?" + WorkingQS + "\">" + PageCount + "</A>&nbsp;";
                        }
                        if (ReportPageNumber < PageCount) {
                            WorkingQS = GenericController.modifyQueryString(WorkingQS, RequestNamePageNumber, (ReportPageNumber + 1).ToString(), true);
                            result += "...<a href=\"" + core.webServer.requestPage + "?" + WorkingQS + "\">next</A>&nbsp;";
                        }
                        result += "<br>&nbsp;";
                    }
                }
                //
                result = ""
                + PreTableCopy + "<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr><td style=\"padding:10px;\">"
                + result + "</td></tr></table>"
                + PostTableCopy + "";
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }
        //
        // ====================================================================================================
        //
        public static string getFormBodyAdminOnly() => HtmlController.div("This page requires administrator permissions.", "ccError").Replace(">", " style=\"margin:10px;padding:10px;background-color:white;\">");
        //
        // ====================================================================================================
        //
        public static string getButtonPrimary(string buttonValue, string onclick, bool disabled, string htmlId, string htmlName) => HtmlController.inputSubmit(buttonValue, htmlName, htmlId, onclick, disabled, "btn btn-primary mr-1 btn-sm");
        //
        public static string getButtonPrimary(string buttonValue, string onclick, bool disabled, string htmlId) => HtmlController.inputSubmit(buttonValue, "button", htmlId, onclick, disabled, "btn btn-primary mr-1 btn-sm");
        //
        public static string getButtonPrimary(string buttonValue, string onclick, bool disabled) => HtmlController.inputSubmit(buttonValue, "button", "", onclick, disabled, "btn btn-primary mr-1 btn-sm");
        //
        public static string getButtonPrimary(string buttonValue, string onclick) => HtmlController.inputSubmit(buttonValue, "button", "", onclick, false, "btn btn-primary mr-1 btn-sm");
        //
        public static string getButtonPrimary(string buttonValue) => HtmlController.inputSubmit(buttonValue, "button", "", "", false, "btn btn-primary mr-1 btn-sm");
        //
        // ====================================================================================================
        //
        public static string getButtonDanger(string buttonValue, string onclick, bool disabled, string htmlId) => HtmlController.inputSubmit(buttonValue, "button", htmlId, onclick, disabled, "btn btn-danger mr-1 btn-sm");
        //
        public static string getButtonDanger(string buttonValue, string onclick, bool disabled) => HtmlController.inputSubmit(buttonValue, "button", "", onclick, disabled, "btn btn-danger mr-1 btn-sm");
        //
        public static string getButtonDanger(string buttonValue, string onclick) => HtmlController.inputSubmit(buttonValue, "button", "", onclick, false, "btn btn-danger mr-1 btn-sm");
        //
        public static string getButtonDanger(string buttonValue) => HtmlController.inputSubmit(buttonValue, "button", "", "", false, "btn btn-danger mr-1 btn-sm");
        //
        // ====================================================================================================
        //
        public static string getButtonPrimaryAnchor(string buttonCaption, string href) => HtmlController.a(buttonCaption, href, "btn btn-primary mr-1 btn-sm");
        //
        // ====================================================================================================
        //
        public static string getButtonDangerAnchor(string buttonCaption, string href) => HtmlController.a(buttonCaption, href, "btn btn-danger mr-1 btn-sm");
        //
        // ====================================================================================================
        //
        public static List<ButtonMetadata> buttonStringToButtonList(string ButtonList) {
            var result = new List<ButtonMetadata>();
            if (!string.IsNullOrEmpty(ButtonList.Trim(' '))) {
                foreach (string buttonValue in ButtonList.Split(',')) {
                    string buttonValueTrim = buttonValue.Trim();
                    result.Add(new ButtonMetadata() {
                        name = "button",
                        value = buttonValue,
                        isAdd = buttonValueTrim.Equals(ButtonAdd),
                        isClose = buttonValueTrim.Equals(ButtonClose),
                        isDelete = buttonValueTrim.Equals(ButtonDelete)
                    });
                }
            }
            return result;
        }
        //
        // ====================================================================================================
        //
        public static string getEditForm_TitleBarDetails_EditorString(DateTime editDate, PersonModel editor, string notEditedMessage) {
            if (editDate < new DateTime(1990, 1, 1)) {
                return "unknown date";
            }
            string result = editDate.ToString() + ", by ";
            if (editor == null) {
                result += "unknown user";
            } else {
                result += editor.getDisplayName();
            }
            return result;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// The panel at the top of the edit page that describes the content being edited
        /// </summary>
        /// <param name="core"></param>
        /// <param name="headerInfo"></param>
        /// <returns></returns>
        public static string getEditForm_TitleBarDetails(CoreController core, RecordEditHeaderInfoClass headerInfo, EditRecordModel editRecord) {
            string result = "";
            bool alt = true;
            if (alt) {
                if (editRecord.id == 0) {
                    result += HtmlController.div(HtmlController.strong(editRecord.contentControlId_Name) + ":&nbsp;New record", "col-sm-12");
                } else {
                    result += HtmlController.div(HtmlController.strong(editRecord.contentControlId_Name + ":&nbsp;#") + headerInfo.recordId + ", " + editRecord.nameLc, "col-sm-4");
                    result += HtmlController.div(HtmlController.strong("Created:&nbsp;") + getEditForm_TitleBarDetails_EditorString(editRecord.dateAdded, editRecord.createdBy, "unknown"), "col-sm-4");
                    result += HtmlController.div(HtmlController.strong("Modified:&nbsp;") + getEditForm_TitleBarDetails_EditorString(editRecord.modifiedDate, editRecord.modifiedBy, "not modified"), "col-sm-4");
                }
                result = HtmlController.div(result, "row");
            } else {
                if (headerInfo.recordId == 0) {
                    result = "<div>New Record</div>";
                } else {
                    result = "<table border=0 cellpadding=0 cellspacing=0 style=\"width:90%\">";
                    result += "<tr><td width=\"50%\">"
                    + "Name: " + headerInfo.recordName + "<br>Record ID: " + headerInfo.recordId + "</td><td width=\"50%\">";
                    //
                    string CreatedCopy = "";
                    CreatedCopy = CreatedCopy + " " + editRecord.dateAdded.ToString();
                    //
                    string CreatedBy = "the system";
                    if (editRecord.createdBy.id != 0) {
                        using (var csData = new CsModel(core)) {
                            if (csData.openSql("select Name,Active from ccMembers where id=" + editRecord.createdBy.id)) {
                                string Name = csData.getText("name");
                                bool Active = csData.getBoolean("active");
                                if (!Active && (!string.IsNullOrEmpty(Name))) {
                                    CreatedBy = "Inactive user " + Name;
                                } else if (!Active) {
                                    CreatedBy = "Inactive user #" + editRecord.createdBy.id;
                                } else if (string.IsNullOrEmpty(Name)) {
                                    CreatedBy = "Unnamed user #" + editRecord.createdBy.id;
                                } else {
                                    CreatedBy = Name;
                                }
                            } else {
                                CreatedBy = "deleted user #" + editRecord.createdBy.id;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(CreatedBy)) {
                        CreatedCopy = CreatedCopy + " by " + CreatedBy;
                    } else {
                    }
                    result += "Created:" + CreatedCopy;
                    //
                    string ModifiedCopy = "";
                    if (editRecord.modifiedDate == DateTime.MinValue) {
                        ModifiedCopy = CreatedCopy;
                    } else {
                        ModifiedCopy = ModifiedCopy + " " + editRecord.modifiedDate;
                        CreatedBy = "the system";
                        if (editRecord.modifiedBy.id != 0) {
                            using (var csData = new CsModel(core)) {
                                if (csData.openSql("select Name,Active from ccMembers where id=" + editRecord.modifiedBy.id)) {
                                    string Name = csData.getText("name");
                                    bool Active = csData.getBoolean("active");
                                    if (!Active && (!string.IsNullOrEmpty(Name))) {
                                        CreatedBy = "Inactive user " + Name;
                                    } else if (!Active) {
                                        CreatedBy = "Inactive user #" + editRecord.modifiedBy.id;
                                    } else if (string.IsNullOrEmpty(Name)) {
                                        CreatedBy = "Unnamed user #" + editRecord.modifiedBy.id;
                                    } else {
                                        CreatedBy = Name;
                                    }
                                } else {
                                    CreatedBy = "deleted user #" + editRecord.modifiedBy.id;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(CreatedBy)) {
                            ModifiedCopy = ModifiedCopy + " by " + CreatedBy;
                        } else {
                        }
                    }
                    result += "<br>Last Modified:" + ModifiedCopy;
                    if ((headerInfo.recordLockExpiresDate == null) || (headerInfo.recordLockExpiresDate < DateTime.Now)) {
                        //
                        // Add Edit Locking to right panel
                        PersonModel personLock = DbBaseModel.create<PersonModel>(core.cpParent, headerInfo.recordLockById);
                        if (personLock != null) {
                            result += "<br><b>Record is locked by " + personLock.name + " until " + headerInfo.recordLockExpiresDate + "</b>";
                        }
                    }
                    //
                    result += "</td></tr>";
                    result += "</table>";
                }
            }
            return result;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// return the default admin editor for this field type
        /// </summary>
        /// <param name="core"></param>
        /// <param name="htmlName"></param>
        /// <param name="htmlValue"></param>
        /// <returns></returns>
        public static string getDefaultEditor_bool(CoreController core, string htmlName, bool htmlValue, bool readOnly = false, string htmlId = "") {
            string result = HtmlController.div(HtmlController.checkbox(htmlName, htmlValue, htmlId, false, "", readOnly), "checkbox");
            if (readOnly) result += HtmlController.inputHidden(htmlName, htmlValue);
            return result;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// return the default admin editor for this field type
        /// </summary>
        /// <param name="core"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="readOnly"></param>
        /// <param name="htmlId"></param>
        /// <param name="isPassword"></param>
        /// <returns></returns>
        public static string getDefaultEditor_text(CoreController core, string fieldName, string fieldValue, bool readOnly = false, string htmlId = "") {
            if ((fieldValue.IndexOf("\n") == -1) && (fieldValue.Length < 80)) {
                //
                // text field shorter then 40 characters without a CR
                return HtmlController.inputText_Legacy(core, fieldName, fieldValue, 1, -1, htmlId, false, readOnly, "text form-control", 255);
            }
            return getDefaultEditor_TextArea(core, fieldName, fieldValue, readOnly, htmlId);
        }
        //
        // ====================================================================================================
        /// <summary>
        /// return the default admin editor for this field type
        /// </summary>
        /// <param name="core"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="readOnly"></param>
        /// <param name="htmlId"></param>
        /// <param name="isPassword"></param>
        /// <returns></returns>
        public static string getDefaultEditor_TextArea(CoreController core, string fieldName, string fieldValue, bool readOnly = false, string htmlId = "") {
            //
            // longer text data, or text that contains a CR
            return HtmlController.inputTextarea(core, fieldName, fieldValue, 10, -1, htmlId, false, readOnly, "text form-control", false);
        }
        //
        // ====================================================================================================
        //
        public static string getDefaultEditor_Html(CoreController core, string fieldName, string fieldValue, string editorAddonListJSON, string styleList, string styleOptionList, bool readONly = false, string htmlId = "") {
            string result = "";
            if (readONly) {
                result += HtmlController.inputHidden(fieldName, fieldValue);
                result += getDefaultEditor_TextArea(core, fieldName, fieldValue, readONly, htmlId);
                return result;
            }
            if (string.IsNullOrEmpty(fieldValue)) {
                //
                // editor needs a starting p tag to setup correctly
                fieldValue = HTMLEditorDefaultCopyNoCr;
            }
            result += core.html.getFormInputHTML(fieldName.ToLowerInvariant(), fieldValue, "500", "", readONly, true, editorAddonListJSON, styleList, styleOptionList);
            result = "<div style=\"width:95%\">" + result + "</div>";
            return result;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// return an admin edit page row for one field in a list of fields within a tab
        /// </summary>
        /// <param name="core"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="readOnly"></param>
        /// <param name="htmlId"></param>
        /// <returns></returns>
        public static string getDefaultEditor_Password(CoreController core, string fieldName, string fieldValue, bool readOnly = false, string htmlId = "") {
            return HtmlController.inputText_Legacy(core, fieldName, fieldValue, -1, -1, htmlId, true, readOnly, "password form-control", 255);
        }
        //
        // ====================================================================================================
        /// <summary>
        /// admin editor for a lookup field into a content table
        /// </summary>
        /// <param name="core"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="lookupContentId"></param>
        /// <param name="readOnly"></param>
        /// <param name="htmlId"></param>
        /// <param name="WhyReadOnlyMsg"></param>
        /// <param name="fieldRequired"></param>
        /// <param name="IsEmptyList"></param>
        /// <returns></returns>
        public static string getDefaultEditor_lookupContent(CoreController core, string fieldName, int fieldValue, int lookupContentId, ref bool IsEmptyList, bool readOnly = false, string htmlId = "", string WhyReadOnlyMsg = "", bool fieldRequired = false, string sqlFilter = "") {
            string result = "";
            ContentMetadataModel lookupContentMetacontent = ContentMetadataModel.create(core, lookupContentId);
            if (lookupContentMetacontent == null) {
                LogController.logWarn(core, "Lookup content not set, field [" + fieldName + "], lookupContentId [" + lookupContentId + "]");
                return string.Empty;
            }
            if (readOnly) {
                //
                // ----- Lookup ReadOnly
                result += (HtmlController.inputHidden(fieldName, GenericController.encodeText(fieldValue)));
                using (var csData = new CsModel(core)) {
                    csData.openRecord(lookupContentMetacontent.name, fieldValue, "Name,ContentControlID");
                    if (csData.ok()) {
                        if (csData.getText("Name") == "") {
                            result += getDefaultEditor_text(core, fieldName + "-readonly-fpo", "No Name", readOnly, htmlId);
                        } else {
                            result += getDefaultEditor_text(core, fieldName + "-readonly-fpo", csData.getText("Name"), readOnly, htmlId);
                        }
                        result += ("&nbsp;[<a TabIndex=-1 href=\"?" + rnAdminForm + "=4&cid=" + lookupContentId + "&id=" + fieldValue.ToString() + "\" target=\"_blank\">View details in new window</a>]");
                    } else {
                        result += ("None");
                    }
                }
                result += ("&nbsp;[<a TabIndex=-1 href=\"?cid=" + lookupContentId + "\" target=\"_blank\">See all " + lookupContentMetacontent.name + "</a>]");
                result += WhyReadOnlyMsg;
            } else {
                //
                // -- not readonly
                string nonLabel = (fieldRequired) ? "" : "None";
                result += core.html.selectFromContent(fieldName, fieldValue, lookupContentMetacontent.name, sqlFilter, nonLabel, "", ref IsEmptyList, "select form-control");
                if (fieldValue != 0) {
                    using (var csData = new CsModel(core)) {
                        if (csData.openRecord(lookupContentMetacontent.name, fieldValue, "ID")) {
                            result += ("&nbsp;[<a TabIndex=-1 href=\"?" + rnAdminForm + "=4&cid=" + lookupContentId + "&id=" + fieldValue.ToString() + "\" target=\"_blank\">Details</a>]");
                        }
                        csData.close();
                    }
                }
                result += ("&nbsp;[<a TabIndex=-1 href=\"?cid=" + lookupContentMetacontent.id + "\" target=\"_blank\">See all " + lookupContentMetacontent.name + "</a>]");

            }
            return result;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// admin editor for a lookup field into a static list
        /// </summary>
        /// <param name="core"></param>
        /// <param name="htmlName"></param>
        /// <param name="defaultLookupIndexBaseOne"></param>
        /// <param name="lookupArray"></param>
        /// <param name="readOnly"></param>
        /// <param name="htmlId"></param>
        /// <param name="WhyReadOnlyMsg"></param>
        /// <param name="fieldRequired"></param>
        /// <returns></returns>
        public static string getDefaultEditor_lookupList(CoreController core, string htmlName, int defaultLookupIndexBaseOne, string[] lookupArray, bool readOnly = false, string htmlId = "", string WhyReadOnlyMsg = "", bool fieldRequired = false) {
            string result = "";
            if (readOnly) {
                //
                // ----- Lookup ReadOnly
                result += (HtmlController.inputHidden(htmlName, defaultLookupIndexBaseOne.ToString()));
                if (defaultLookupIndexBaseOne < 1) {
                    result += getDefaultEditor_text(core, htmlName + "-readonly-fpo", "None", readOnly, htmlId);
                } else if (defaultLookupIndexBaseOne > (lookupArray.GetUpperBound(0) + 1)) {
                    result += getDefaultEditor_text(core, htmlName + "-readonly-fpo", "None", readOnly, htmlId);
                } else {
                    result += getDefaultEditor_text(core, htmlName + "-readonly-fpo", lookupArray[defaultLookupIndexBaseOne - 1], readOnly, htmlId);
                }
                result += WhyReadOnlyMsg;
            } else {
                if (!fieldRequired) {
                    result += HtmlController.selectFromList(core, htmlName, defaultLookupIndexBaseOne, lookupArray, "Select One", "", "select form-control");
                } else {
                    result += HtmlController.selectFromList(core, htmlName, defaultLookupIndexBaseOne, lookupArray, "", "", "select form-control");
                }

            }
            return result;
        }
        public static string getDefaultEditor_LookupList(CoreController core, string htmlName, string defaultValue, List<NameValueModel> lookupList, bool readOnly = false, string htmlId = "", string WhyReadOnlyMsg = "", bool fieldRequired = false) {
            string result = "";
            if (readOnly) {
                //
                // ----- Lookup ReadOnly
                result += (HtmlController.inputHidden(htmlName, GenericController.encodeText(defaultValue)));
                NameValueModel nameValue = lookupList.Find(x => x.name.ToLowerInvariant() == htmlName.ToLowerInvariant());
                if (nameValue == null) {
                    result += getDefaultEditor_text(core, htmlName + "-readonly-fpo", "None", readOnly, htmlId);
                } else {
                    result += getDefaultEditor_text(core, htmlName + "-readonly-fpo", nameValue.value, readOnly, htmlId);
                }
                result += WhyReadOnlyMsg;
            } else {
                if (!fieldRequired) {
                    result += HtmlController.selectFromList(core, htmlName, defaultValue, lookupList, "Select One", "", "select form-control");
                } else {
                    result += HtmlController.selectFromList(core, htmlName, defaultValue, lookupList, "", "", "select form-control");
                }

            }
            return result;
        }

        //
        // ====================================================================================================
        //
        public static string getDefaultEditor_dateTime(CoreController core, string fieldName, DateTime FieldValueDate, bool readOnly = false, string htmlId = "", bool fieldRequired = false, string WhyReadOnlyMsg = "") {
            string inputDate = "";
            if (FieldValueDate.CompareTo(new DateTime(1900, 1, 1)) > 0) {
                if (FieldValueDate.Hour.Equals(0) && FieldValueDate.Minute.Equals(0) && FieldValueDate.Second.Equals(0)) {
                    inputDate = FieldValueDate.ToShortDateString();
                } else {
                    inputDate = FieldValueDate.ToString();
                }
            }
            return getDefaultEditor_text(core, fieldName, inputDate, readOnly, htmlId);
        }
        //
        // ====================================================================================================
        //
        public static string getDefaultEditor_memberSelect(CoreController core, string htmlName, int selectedRecordId, int groupId, string groupName, bool readOnly = false, string htmlId = "", bool fieldRequired = false, string WhyReadOnlyMsg = "") {
            string EditorString = "";
            if ((groupId > 0) & (string.IsNullOrWhiteSpace(groupName))) {
                var group = DbBaseModel.create<GroupModel>(core.cpParent, groupId);
                if (group != null) {
                    groupName = "Group " + group.id.ToString();
                    group.save(core.cpParent);
                }
            }
            if (readOnly) {
                //
                // -- readOnly
                EditorString += HtmlController.inputHidden(htmlName, selectedRecordId.ToString());
                if (selectedRecordId == 0) {
                    EditorString += "None";
                } else {
                    var selectedUser = DbBaseModel.create<PersonModel>(core.cpParent, selectedRecordId);
                    if (selectedUser == null) {
                        EditorString += getDefaultEditor_text(core, htmlName + "-readonly-fpo", "(deleted)", readOnly, htmlId);
                    } else {
                        EditorString += getDefaultEditor_text(core, htmlName + "-readonly-fpo", (string.IsNullOrWhiteSpace(selectedUser.name)) ? "No Name" : HtmlController.encodeHtml(selectedUser.name), readOnly, htmlId);
                        EditorString += ("&nbsp;[<a TabIndex=-1 href=\"?af=4&cid=" + selectedUser.contentControlId.ToString() + "&id=" + selectedRecordId.ToString() + "\" target=\"_blank\">View details in new window</a>]");
                    }
                }
                EditorString += WhyReadOnlyMsg;
            } else {
                //
                // -- editable
                EditorString += core.html.selectUserFromGroup(htmlName, selectedRecordId, groupId, "", (fieldRequired) ? "" : "None", htmlId, "select form-control");
                if (selectedRecordId != 0) {
                    var selectedUser = DbBaseModel.create<PersonModel>(core.cpParent, selectedRecordId);
                    if (selectedUser == null) {
                        EditorString += "Deleted";
                    } else {
                        string recordName = (string.IsNullOrWhiteSpace(selectedUser.name)) ? "No Name" : HtmlController.encodeHtml(selectedUser.name);
                        EditorString += "&nbsp;[Edit <a TabIndex=-1 href=\"?af=4&cid=" + selectedUser.contentControlId.ToString() + "&id=" + selectedRecordId.ToString() + "\">" + HtmlController.encodeHtml(recordName) + "</a>]";
                    }
                }
                EditorString += ("&nbsp;[Select from members of <a TabIndex=-1 href=\"?cid=" + ContentMetadataModel.getContentId(core, "groups") + "\">" + groupName + "</a>]");
            }
            return EditorString;
        }
        //
        // ====================================================================================================
        //
        public static string getDefaultEditor_manyToMany(CoreController core, ContentFieldMetadataModel field, string htmlName, string currentValueCommaList, int editRecordId, bool readOnly = false, string WhyReadOnlyMsg = "") {
            string result = "";
            //
            string MTMContent0 = MetadataController.getContentNameByID(core, field.contentId);
            string MTMContent1 = MetadataController.getContentNameByID(core, field.manyToManyContentId);
            string MTMRuleContent = MetadataController.getContentNameByID(core, field.manyToManyRuleContentId);
            string MTMRuleField0 = field.manyToManyRulePrimaryField;
            string MTMRuleField1 = field.manyToManyRuleSecondaryField;
            result += core.html.getCheckList(htmlName, MTMContent0, editRecordId, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, "", "", readOnly, false, currentValueCommaList);
            //result += core.html.getCheckList("ManyToMany" + field.id, MTMContent0, editRecordId, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1);
            result += WhyReadOnlyMsg;
            return result;
        }
        //
        //====================================================================================================
        //
        public static string getDefaultEditor_SelectorString(CoreController core, string SitePropertyName, string SitePropertyValue, string selector) {
            string result = "";
            try {
                Dictionary<string, string> instanceOptions = new Dictionary<string, string> {
                    { SitePropertyName, SitePropertyValue }
                };
                string ExpandedSelector = "";
                Dictionary<string, string> addonInstanceProperties = new Dictionary<string, string>();
                core.addon.buildAddonOptionLists(ref addonInstanceProperties, ref ExpandedSelector, SitePropertyName + "=" + selector, instanceOptions, "0", true);
                //buildAddonOptionLists(ref addonInstanceProperties, ref ExpandedSelector, SitePropertyName + "=" + selector, instanceOptions, "0", true);
                int Pos = GenericController.vbInstr(1, ExpandedSelector, "[");
                if (Pos != 0) {
                    //
                    // List of Options, might be select, radio or checkbox
                    //
                    string LCaseOptionDefault = GenericController.vbLCase(ExpandedSelector.Left(Pos - 1));
                    int PosEqual = GenericController.vbInstr(1, LCaseOptionDefault, "=");

                    if (PosEqual > 0) {
                        LCaseOptionDefault = LCaseOptionDefault.Substring(PosEqual);
                    }

                    LCaseOptionDefault = GenericController.decodeNvaArgument(LCaseOptionDefault);
                    ExpandedSelector = ExpandedSelector.Substring(Pos);
                    Pos = GenericController.vbInstr(1, ExpandedSelector, "]");
                    string OptionSuffix = "";
                    if (Pos > 0) {
                        if (Pos < ExpandedSelector.Length) {
                            OptionSuffix = GenericController.vbLCase((ExpandedSelector.Substring(Pos)).Trim(' '));
                        }
                        ExpandedSelector = ExpandedSelector.Left(Pos - 1);
                    }
                    string[] OptionValues = ExpandedSelector.Split('|');
                    result = "";
                    int OptionCnt = OptionValues.GetUpperBound(0) + 1;
                    int OptionPtr = 0;
                    for (OptionPtr = 0; OptionPtr < OptionCnt; OptionPtr++) {
                        string OptionValue_AddonEncoded = OptionValues[OptionPtr].Trim(' ');
                        if (!string.IsNullOrEmpty(OptionValue_AddonEncoded)) {
                            Pos = GenericController.vbInstr(1, OptionValue_AddonEncoded, ":");
                            string OptionCaption = null;
                            string OptionValue = null;
                            if (Pos == 0) {
                                OptionValue = GenericController.decodeNvaArgument(OptionValue_AddonEncoded);
                                OptionCaption = OptionValue;
                            } else {
                                OptionCaption = GenericController.decodeNvaArgument(OptionValue_AddonEncoded.Left(Pos - 1));
                                OptionValue = GenericController.decodeNvaArgument(OptionValue_AddonEncoded.Substring(Pos));
                            }
                            switch (OptionSuffix) {
                                case "checkbox":
                                    //
                                    // Create checkbox addon_execute_getFormContent_decodeSelector
                                    //
                                    bool selected = (GenericController.vbInstr(1, "," + LCaseOptionDefault + ",", "," + GenericController.vbLCase(OptionValue) + ",") != 0);
                                    result = HtmlController.checkbox(SitePropertyName + OptionPtr, selected, "", false, "", false, OptionValue, OptionCaption);
                                    break;
                                case "radio":
                                    //
                                    // Create Radio addon_execute_getFormContent_decodeSelector
                                    //
                                    if (GenericController.vbLCase(OptionValue) == LCaseOptionDefault) {
                                        result += "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + SitePropertyName + "\" value=\"" + OptionValue + "\" checked=\"checked\" >" + OptionCaption + "</div>";
                                    } else {
                                        result += "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + SitePropertyName + "\" value=\"" + OptionValue + "\" >" + OptionCaption + "</div>";
                                    }
                                    break;
                                default:
                                    //
                                    // Create select addon_execute_result
                                    //
                                    if (GenericController.vbLCase(OptionValue) == LCaseOptionDefault) {
                                        result += "<option value=\"" + OptionValue + "\" selected>" + OptionCaption + "</option>";
                                    } else {
                                        result += "<option value=\"" + OptionValue + "\">" + OptionCaption + "</option>";
                                    }
                                    break;
                            }
                        }
                    }
                    //stringBuilderLegacyController FastString = null;
                    string Copy = "";
                    switch (OptionSuffix) {
                        case "checkbox":
                            //
                            //
                            Copy += HtmlController.inputHidden(SitePropertyName + "CheckBoxCnt", OptionCnt);
                            break;
                        case "radio":
                            //
                            // Create Radio addon_execute_result
                            //
                            //addon_execute_result = "<div>" & genericController.vbReplace(addon_execute_result, "><", "></div><div><") & "</div>"
                            break;
                        default:
                            //
                            // Create select addon_execute_result
                            //
                            result = "<select name=\"" + SitePropertyName + "\" class=\"select form-control\">" + result + "</select>";
                            break;
                    }
                } else {
                    //
                    // Create Text addon_execute_result
                    //

                    selector = GenericController.decodeNvaArgument(selector);
                    result = getDefaultEditor_text(core, SitePropertyName, selector);
                }

                //FastString = null;
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }

        //
        // ====================================================================================================
        /// <summary>
        /// return an admin edit page row for one field in a list of fields within a tab
        /// </summary>
        /// <param name="core"></param>
        /// <param name="Caption"></param>
        /// <param name="fieldHtmlId"></param>
        /// <param name="EditorString"></param>
        /// <param name="editorHelpRow"></param>
        /// <returns></returns>
        public static string getEditRow(CoreController core, string EditorString, string Caption, string editorHelpRow, bool fieldRequired = false, bool ignore = false, string fieldHtmlId = "") {
            return HtmlController.div(HtmlController.label(Caption, fieldHtmlId) + HtmlController.div(EditorString, "ml-5") + HtmlController.div(HtmlController.small(editorHelpRow, "form-text text-muted"), "ml-5"), "p-2 ccEditRow");
        }
        //
        // ====================================================================================================
        //
        public static string getEditRowLegacy(CoreController core, string HTMLFieldString, string Caption, string HelpMessage = "", bool FieldRequired = false, bool AllowActiveEdit = false, string ignore0 = "") {
            return getEditRow(core, HTMLFieldString, Caption, HelpMessage, FieldRequired, AllowActiveEdit, ignore0);
        }
        //
        //====================================================================================================
        //
        public static string getHeaderTitleDescription(string Title, string Description) {
            return "" 
                + ((string.IsNullOrWhiteSpace(Title)) ? "" : HtmlController.h2(Title)) 
                + ((string.IsNullOrWhiteSpace(Description)) ? "" : HtmlController.div(Description));
        }
        //
        //====================================================================================================
        //
        public static string getToolForm(CoreController core, string innerHtml, string leftButtonCommaList, string rightButtonCommaList) {
            string buttonBar = core.html.getPanelButtons(leftButtonCommaList, rightButtonCommaList);
            string result = ""
                + buttonBar
                + HtmlController.div(innerHtml, "p-4 bg-light")
                + buttonBar;
            return HtmlController.formMultipart(core, result);
        }
        //
        public static string getToolForm(CoreController core, string innerHtml, string leftButtonCommaList)
            => getToolForm(core, innerHtml, leftButtonCommaList, "");
        //
        //====================================================================================================
        //
        public static string getToolFormRow(CoreController core, string asdf) {
            return HtmlController.div(asdf, "p-1");
        }
        //
        //====================================================================================================
        //
        public static string getToolFormInputRow(CoreController core, string label, string input) {
            return getToolFormRow(core, HtmlController.label(label) + "<br>" + input);
        }
        //
        //===================================================================================================
        /// <summary>
        /// Wrap an edit region in a dotted border
        /// </summary>
        /// <param name="core"></param>
        /// <param name="caption"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string getEditWrapper(CoreController core, string caption, string content) {
            if (!core.session.isEditing()) { return content; }
            string result = HtmlController.div(content, "ccEditWrapperContent");
            if (!string.IsNullOrEmpty(caption)) { result = HtmlController.div(caption, "ccEditWrapperCaption") + result; }
            return HtmlController.div(result, "ccEditWrapper", "editWrapper" + core.doc.editWrapperCnt++);
        }
        //
        //===================================================================================================
        /// <summary>
        /// Wrap an edit region in a dotted border
        /// </summary>
        /// <param name="core"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string getEditWrapper(CoreController core, string content)
            => getEditWrapper(core, "", content);
        //
        // ====================================================================================================
        /// <summary>
        /// get linked icon for remove (red x)
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public static string getDeleteLink(string link) { return HtmlController.a(iconDelete_Red, link); }
        public static string getArrowRightLink(string link) { return HtmlController.a(iconArrowRight, link); }
        public static string getArrowLeftLink(string link) { return HtmlController.a(iconArrowLeft, link); }
        public static string getPlusLink(string link, string caption = "") { return HtmlController.a(iconAdd_Green + caption, link); }
        public static string getExpandLink(string link) { return HtmlController.a(iconExpand, link); }
        public static string getContractLink(string link) { return HtmlController.a(iconContract, link); }
        public static string getRefreshLink(string link) { return HtmlController.a(iconRefresh, link); }
        //
        //====================================================================================================
        /// <summary>
        /// create UI edit record link
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <param name="AllowCut"></param>
        /// <returns></returns>
        public static string getRecordEditAndCutLink(CoreController core, string ContentName, int RecordID, bool AllowCut) {
            return getRecordEditAndCutLink(core, ContentName, RecordID, AllowCut, "");
        }
        //
        public static string getRecordEditLink(CoreController core, string ContentName, int RecordID) {
            return getRecordEditAndCutLink(core, ContentName, RecordID, false, "");
        }
        //
        public static string getRecordEditLink(CoreController core, string ContentName, int RecordID, string recordName) {
            return getRecordEditAndCutLink(core, ContentName, RecordID, false, recordName);
        }
        //
        //====================================================================================================
        //
        public static string getRecordEditAndCutLink(CoreController core, string contentName, int recordId, bool allowCut, string recordName) {
            try {
                if (!core.session.isEditing()) { return string.Empty; }
                var editSegmentList = new List<string> {
                    getRecordEditSegment(core, contentName, recordId, recordName)
                };
                if (allowCut) { editSegmentList.Add(getRecordCutSegment(core, contentName, recordId)); }
                return getRecordEditLink(core, editSegmentList);
            } catch (Exception ex) {
                LogController.logError(core, ex);
                return string.Empty;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// return caption for edit, cut, paste links
        /// </summary>
        /// <param name="verb">edit, cut, paste, etc</param>
        /// <param name="contentName"></param>
        /// <param name="recordId"></param>
        /// <param name="recordName"></param>
        /// <returns></returns>
        public static string getEditSegmentRecordCaption(string verb, string contentName, int recordId, string recordName) {
            string captionName;
            if (!string.IsNullOrWhiteSpace(recordName)) {
                //
                // -- named record
                captionName = "'" + HtmlController.encodeHtml(recordName) + "'";
            } else if (recordId != 0) {
                //
                // -- record #123
                captionName = "record #" + recordId.ToString();
            } else {
                //
                // -- record
                captionName = "record";
            }
            string result = verb + "&nbsp;";
            switch (contentName.ToLower(CultureInfo.InvariantCulture)) {
                case "page content":
                    result += "Page&nbsp;" + captionName;
                    break;
                case "page templates":
                    result += "Template&nbsp;" + captionName;
                    break;
                default:
                    result += captionName + "&nbsp;in&nbsp;" + HtmlController.encodeHtml(encodeInitialCaps(contentName));
                    break;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return an addon edit link
        /// </summary>
        /// <param name="core"></param>
        /// <param name="cdef"></param>
        /// <param name="addonId"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public static string getAddonEditLink(CoreController core, int addonId, string caption) {
            var cdef = ContentMetadataModel.createByUniqueName(core, "add-ons");
            if (cdef == null) { return string.Empty; }
            string link = "/" + core.appConfig.adminRoute + "?af=4&aa=2&ad=1&cid=" + cdef.id + "&id=" + addonId;
            return HtmlController.a(iconAddon_Green + ((string.IsNullOrWhiteSpace(caption)) ? "" : "&nbsp;" + caption), link, "ccAddonEditLink");
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns an addon edit link to be included getEditLink wrapper
        /// </summary>
        /// <param name="core"></param>
        /// <param name="addonId"></param>
        /// <param name="addonName"></param>
        /// <returns></returns>
        public static string getAddonEditSegment(CoreController core, int addonId, string addonName) {
            try {
                if (!core.session.isEditing()) { return string.Empty; }
                if (addonId < 1) { throw (new GenericException("RecordID [" + addonId + "] is invalid")); }
                return AdminUIController.getAddonEditLink(core, addonId, "Edit Add-on" + ((string.IsNullOrEmpty(addonName)) ? "" : " '" + addonName + "'"));
            } catch (Exception ex) {
                LogController.logError(core, ex);
                return string.Empty;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns a record edit link to be included getEditLink wrapper
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentName"></param>
        /// <param name="recordId"></param>
        /// <param name="recordName"></param>
        /// <returns></returns>
        public static string getRecordEditSegment(CoreController core, string contentName, int recordId, string recordName) {
            try {
                if (!core.session.isEditing()) { return string.Empty; }
                if (string.IsNullOrWhiteSpace(contentName)) { throw (new GenericException("ContentName [" + contentName + "] is invalid")); }
                if (recordId < 1) { throw (new GenericException("RecordID [" + recordId + "] is invalid")); }
                var cdef = ContentMetadataModel.createByUniqueName(core, contentName);
                if (cdef == null) { throw new GenericException("getRecordEditLink called with contentName [" + contentName + "], but no content found with this name."); }
                return AdminUIController.getRecordEditLink(core, cdef, recordId, getEditSegmentRecordCaption("Edit", contentName, recordId, recordName));
            } catch (Exception ex) {
                LogController.logError(core, ex);
                return string.Empty;
            }
        }
        //
        public static string getRecordEditSegment(CoreController core, string contentName, int recordID)
            => getRecordEditSegment(core, contentName, recordID, "");
        //
        //====================================================================================================
        /// <summary>
        /// returns a record cut link to be included getEditLink wrapper
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentName"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public static string getRecordCutSegment(CoreController core, string contentName, int recordId) {
            try {
                if (!core.session.isEditing()) { return string.Empty; }
                var cdef = ContentMetadataModel.createByUniqueName(core, contentName);
                if (cdef == null) { throw new GenericException("getRecordEditLink called with contentName [" + contentName + "], but no content found with this name."); }
                string WorkingLink = GenericController.modifyLinkQuery(core.webServer.requestPage + "?" + core.doc.refreshQueryString, RequestNameCut, GenericController.encodeText(cdef.id) + "." + GenericController.encodeText(recordId), true);
                return "<a class=\"ccRecordCutLink\" TabIndex=\"-1\" href=\"" + HtmlController.encodeHtml(WorkingLink) + "\">" + iconContentCut.Replace("content cut", getEditSegmentRecordCaption("Cut", contentName, recordId, "")) + "</a>";
            } catch (Exception ex) {
                LogController.logError(core, ex);
                return string.Empty;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// An edit link (or left justified pill) is composed of a sequence of edit links (content edit links, cut, paste, addon edit) plus an endcap
        /// </summary>
        /// <param name="core"></param>
        /// <param name="editSegmentList"></param>
        /// <returns></returns>
        public static string getRecordEditLink(CoreController core, List<string> editSegmentList) {
            if (!core.session.isEditing()) { return string.Empty; }
            string result = string.Join("", editSegmentList) + HtmlController.div("&nbsp;", "ccEditLinkEndCap");
            return HtmlController.div(result, "ccRecordLinkCon");
        }
        //
        //====================================================================================================
        /// <summary>
        /// An edit link (or left justified pill) is composed of a sequence of edit links (content edit links, cut, paste, addon edit) plus an endcap
        /// </summary>
        /// <param name="core"></param>
        /// <param name="editSegmentList"></param>
        /// <returns></returns>
        public static string getAddonEditLink(CoreController core, List<string> editSegmentList) {
            if (!core.session.isAdvancedEditing()) { return string.Empty; }
            string result = string.Join("", editSegmentList) + HtmlController.div("&nbsp;", "ccEditLinkEndCap");
            return HtmlController.div(result, "ccAddonEditLinkCon");
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a list of add links for the content plus all valid subbordinate content
        /// </summary>
        /// <param name="core"></param>
        /// <param name="contentName"></param>
        /// <param name="presetNameValueList"></param>
        /// <param name="allowPaste"></param>
        /// <param name="IsEditing"></param>
        /// <returns></returns>
        public static List<string> getRecordAddLink(CoreController core, string contentName, string presetNameValueList, bool allowPaste, bool IsEditing) {
            List<string> result = new List<string>();
            try {
                if (!IsEditing) { return result; }
                if (string.IsNullOrWhiteSpace(contentName)) { throw (new GenericException("ContentName [" + contentName + "] is invalid")); }
                //
                // -- convert older QS format to command delimited format
                presetNameValueList = presetNameValueList.Replace("&", ",");
                var content = DbBaseModel.createByUniqueName<ContentModel>(core.cpParent, contentName);
                result.AddRange(getRecordAddLink_GetChildContentLinks(core, content, presetNameValueList, new List<int>()));
                //
                // -- Add in the paste entry, if needed
                if (!allowPaste) { return result; }
                string ClipBoard = core.visitProperty.getText("Clipboard", "");
                if (string.IsNullOrEmpty(ClipBoard)) { return result; }
                int Position = GenericController.vbInstr(1, ClipBoard, ".");
                if (Position == 0) { return result; }
                string[] ClipBoardArray = ClipBoard.Split('.');
                if (ClipBoardArray.GetUpperBound(0) == 0) { return result; }
                int ClipboardContentId = GenericController.encodeInteger(ClipBoardArray[0]);
                int ClipChildRecordId = GenericController.encodeInteger(ClipBoardArray[1]);
                if (content.isParentOf<ContentModel>(core.cpParent, ClipboardContentId)) {
                    int ParentId = 0;
                    if (GenericController.vbInstr(1, presetNameValueList, "PARENTID=", 1) != 0) {
                        //
                        // must test for main_IsChildRecord
                        //
                        string BufferString = presetNameValueList;
                        BufferString = BufferString.Replace("(", "");
                        BufferString = BufferString.Replace(")", "");
                        BufferString = BufferString.Replace(",", "&");
                        ParentId = encodeInteger(GenericController.main_GetNameValue_Internal(core, BufferString, "Parentid"));
                    }
                    if ((ParentId != 0) && (!DbBaseModel.isChildOf<PageContentModel>(core.cpParent, ParentId, 0, new List<int>()))) {
                        //
                        // Can not paste as child of itself
                        string PasteLink = core.webServer.requestPage + "?" + core.doc.refreshQueryString;
                        PasteLink = GenericController.modifyLinkQuery(PasteLink, RequestNamePaste, "1", true);
                        PasteLink = GenericController.modifyLinkQuery(PasteLink, RequestNamePasteParentContentId, content.id.ToString(), true);
                        PasteLink = GenericController.modifyLinkQuery(PasteLink, RequestNamePasteParentRecordId, ParentId.ToString(), true);
                        PasteLink = GenericController.modifyLinkQuery(PasteLink, RequestNamePasteFieldList, presetNameValueList, true);
                        //result.Add(HtmlController.div(HtmlController.a(iconContentPaste_Green, PasteLink, "ccRecordPasteLink", "", "-1"), "ccRecordLinkCon"));
                        string pasteLinkAnchor = HtmlController.a(iconContentPaste_Green + "&nbsp;Paste Record", PasteLink, "ccRecordPasteLink", "", "-1");
                        result.Add(HtmlController.div(pasteLinkAnchor + HtmlController.div("&nbsp;", "ccEditLinkEndCap"), "ccRecordLinkCon"));
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }
        //
        public static List<string> getRecordAddLink(CoreController core, string ContentName, string PresetNameValueList, bool AllowPaste) => getRecordAddLink(core, ContentName, PresetNameValueList, AllowPaste, core.session.isEditing(ContentName));
        //
        public static List<string> getRecordAddLink(CoreController core, string ContentName, string PresetNameValueList) => getRecordAddLink(core, ContentName, PresetNameValueList, false, core.session.isEditing(ContentName));
        //
        //====================================================================================================
        /// <summary>
        /// return record add links for all the child
        /// </summary>
        /// <param name="core"></param>
        /// <param name="content"></param>
        /// <param name="PresetNameValueList"></param>
        /// <param name="usedContentIdList"></param>
        /// <param name="MenuName"></param>
        /// <param name="ParentMenuName"></param>
        /// <returns></returns>
        private static List<string> getRecordAddLink_GetChildContentLinks(CoreController core, ContentModel content, string PresetNameValueList, List<int> usedContentIdList) {
            var result = new List<string>();
            string Link = "";
            if (content != null) {
                if (usedContentIdList.Contains(content.id)) {
                    throw (new ApplicationException("result , Content Child [" + content.name + "] is one of its own parents"));
                } else {
                    usedContentIdList.Add(content.id);
                    //
                    // -- Determine if use has access
                    bool userHasAccess = false;
                    bool contentAllowAdd = false;
                    bool groupRulesAllowAdd = false;
                    DateTime memberRulesDateExpires = default(DateTime);
                    bool memberRulesAllow = false;
                    if (core.session.isAuthenticatedAdmin()) {
                        //
                        // Entry was found
                        userHasAccess = true;
                        contentAllowAdd = true;
                        groupRulesAllowAdd = true;
                        memberRulesDateExpires = DateTime.MinValue;
                        memberRulesAllow = true;
                    } else {
                        //
                        // non-admin member, first check if they have access and main_Get true markers
                        //
                        using (var csData = new CsModel(core)) {
                            string sql = "SELECT ccContent.ID as ContentID, ccContent.AllowAdd as ContentAllowAdd, ccGroupRules.AllowAdd as GroupRulesAllowAdd, ccMemberRules.DateExpires as MemberRulesDateExpires"
                                + " FROM (((ccContent"
                                    + " LEFT JOIN ccGroupRules ON ccGroupRules.ContentID=ccContent.ID)"
                                    + " LEFT JOIN ccgroups ON ccGroupRules.GroupID=ccgroups.ID)"
                                    + " LEFT JOIN ccMemberRules ON ccgroups.ID=ccMemberRules.GroupID)"
                                    + " LEFT JOIN ccMembers ON ccMemberRules.memberId=ccMembers.ID"
                                + " WHERE ("
                                + " (ccContent.id=" + content.id + ")"
                                + " AND(ccContent.active<>0)"
                                + " AND(ccGroupRules.active<>0)"
                                + " AND(ccMemberRules.active<>0)"
                                + " AND((ccMemberRules.DateExpires is Null)or(ccMemberRules.DateExpires>" + DbController.encodeSQLDate(core.doc.profileStartTime) + "))"
                                + " AND(ccgroups.active<>0)"
                                + " AND(ccMembers.active<>0)"
                                + " AND(ccMembers.ID=" + core.session.user.id + ")"
                                + " );";
                            csData.openSql(sql);
                            if (csData.ok()) {
                                //
                                // ----- Entry was found, member has some kind of access
                                //
                                userHasAccess = true;
                                contentAllowAdd = content.allowAdd;
                                groupRulesAllowAdd = csData.getBoolean("GroupRulesAllowAdd");
                                memberRulesDateExpires = csData.getDate("MemberRulesDateExpires");
                                memberRulesAllow = false;
                                if (memberRulesDateExpires == DateTime.MinValue) {
                                    memberRulesAllow = true;
                                } else if (memberRulesDateExpires > core.doc.profileStartTime) {
                                    memberRulesAllow = true;
                                }
                            } else {
                                //
                                // ----- No entry found, this member does not have access, just main_Get ContentID
                                //
                                userHasAccess = true;
                                contentAllowAdd = false;
                                groupRulesAllowAdd = false;
                                memberRulesAllow = false;
                            }
                        }
                    }
                    if (userHasAccess) {
                        //
                        // Add the Menu Entry* to the current menu (MenuName)
                        //
                        Link = "";
                        //string ButtonCaption = content.name;
                        //result = MenuName;
                        if (contentAllowAdd && groupRulesAllowAdd && memberRulesAllow) {
                            Link = "/" + core.appConfig.adminRoute + "?cid=" + content.id + "&af=4&aa=2&ad=1";
                            if (!string.IsNullOrEmpty(PresetNameValueList)) {
                                string NameValueList = PresetNameValueList;
                                Link = Link + "&wc=" + GenericController.encodeRequestVariable(PresetNameValueList);
                            }
                        }
                        result.Add(HtmlController.div(HtmlController.a(iconAdd_Green + "&nbsp;Add " + content.name + " Record", Link, "ccRecordAddLink", "", "-1") + HtmlController.div("&nbsp;", "ccEditLinkEndCap"), "ccRecordLinkCon"));
                        //core.menuFlyout.menu_AddEntry(MenuName + ":" + content.name, ParentMenuName, "", "", Link, ButtonCaption, "", "", true);
                        //
                        // Create child submenu if Child Entries found
                        var childList = DbBaseModel.createList<ContentModel>(core.cpParent, "ParentID=" + content.id);
                        if (childList.Count > 0) {
                            //
                            // Add the child menu
                            //string ChildMenuName = MenuName + ":" + content.name;
                            //int ChildMenuButtonCount = 0;
                            //
                            // ----- Create the ChildPanel with all Children found
                            //
                            foreach (var child in childList) {
                                result.AddRange(getRecordAddLink_GetChildContentLinks(core, child, PresetNameValueList, usedContentIdList));
                            }
                        }
                    }
                }
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a link to edit a record in the admin site
        /// </summary>
        /// <param name="core"></param>
        /// <param name="cdef"></param>
        /// <returns></returns>
        public static string getRecordEditLink(CoreController core, ContentMetadataModel cdef) => getRecordEditLink(core, cdef, 0, "");
        public static string getRecordEditLink(CoreController core, ContentMetadataModel cdef, int recordId) => getRecordEditLink(core, cdef, recordId, "");
        public static string getRecordEditLink(CoreController core, ContentMetadataModel cdef, int recordId, string caption) {
            return getRecordEditLink("/" + core.appConfig.adminRoute + "?af=4&aa=2&ad=1&cid=" + cdef.id + "&id=" + recordId, caption, "ccRecordEditLink");
        }
        public static string getRecordEditLink(string link) => getRecordEditLink(link, "", "");
        public static string getRecordEditLink(string link, string caption) => getRecordEditLink(link, caption, "");
        public static string getRecordEditLink(string link, string caption, string htmlClass) {
            return HtmlController.a(iconEdit_Green + ((string.IsNullOrWhiteSpace(caption)) ? "" : "&nbsp;" + caption), link, htmlClass);
        }

    }
}
