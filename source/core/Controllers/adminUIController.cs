﻿
using System;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
using Contensive.Core.Models.Complex;
//
namespace Contensive.Core {
    //
    //====================================================================================================
    /// <summary>
    /// UI rendering for Admin
    /// REFACTOR - add  try-catch
    /// not IDisposable - not contained classes that need to be disposed
    /// </summary>
    public class adminUIController {
        //
        //========================================================================
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
        //
        //
        public class indexConfigSortClass {
            //Dim FieldPtr As Integer
            public string fieldName;
            public int direction; // 1=forward, 2=reverse, 0=ignore/remove this sort
        }
        //
        public class indexConfigFindWordClass {
            public string Name;
            public string Value;
            public int Type;
            public FindWordMatchEnum MatchOption;
        }
        //
        public class indexConfigColumnClass {
            public string Name;
            //Public FieldId As Integer
            public int Width;
            public int SortPriority;
            public int SortDirection;
        }
        //
        public class indexConfigClass {
            public bool Loaded;
            public int ContentID;
            public int PageNumber;
            public int RecordsPerPage;
            public int RecordTop;

            //FindWordList As String
            public Dictionary<string, indexConfigFindWordClass> FindWords = new Dictionary<string, indexConfigFindWordClass>();
            //Public FindWordCnt As Integer
            public bool ActiveOnly;
            public bool LastEditedByMe;
            public bool LastEditedToday;
            public bool LastEditedPast7Days;
            public bool LastEditedPast30Days;
            public bool Open;
            //public SortCnt As Integer
            public Dictionary<string, indexConfigSortClass> Sorts = new Dictionary<string, indexConfigSortClass>();
            public int GroupListCnt;
            public string[] GroupList;
            //public ColumnCnt As Integer
            public Dictionary<string, indexConfigColumnClass> Columns = new Dictionary<string, indexConfigColumnClass>();
            //SubCDefs() as integer
            //SubCDefCnt as integer
            public int SubCDefID;
        }
        public class buttonMetadata {
            public string name = "button";
            public string value = "";
            public string classList = "";
            public bool isDelete = false;
            public bool isClose = false;
            public bool isAdd = false;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Title Bar
        /// </summary>
        /// <param name="core"></param>
        /// <param name="Title"></param>
        /// <param name="Description"></param>
        /// <returns></returns>
        public static string GetTitleBar(coreController core, string Title, string Description) {
            string result = "";
            try {
                result = Title;
                if (core.doc.debug_iUserError != "") {
                    Description += htmlController.div( errorController.getUserError(core));
                }
                if (!string.IsNullOrEmpty(Description)) {
                    result += htmlController.div( Description, "ccAdminInfoBar ccPanel3DReverse");
                }
            } catch (Exception ex) {
                logController.handleError(core, ex);
            }
            result = htmlController.div(result, "ccAdminTitleBar");
            return result;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Get the Normal Edit Button Bar String, used on Normal Edit and others
        /// </summary>
        public static string GetButtonBarForEdit(coreController core, editButtonBarInfoClass info) {
            string buttonsLeft = "";
            string buttonsRight = "";
            try {
                if (info.allowCancel) buttonsLeft += getButtonPrimary(ButtonCancel, "return processSubmit(this);");
                if (info.allowRefresh) buttonsLeft +=  getButtonPrimary(ButtonRefresh);
                if (info.allowSave) {
                    buttonsLeft += getButtonPrimary(ButtonOK, "return processSubmit(this);");
                    buttonsLeft += getButtonPrimary(ButtonSave, "return processSubmit(this);");
                    if (info.allowAdd) buttonsLeft += getButtonPrimary(ButtonSaveAddNew, "return processSubmit(this);");
                }
                if (info.allowSendTest) buttonsLeft += getButtonPrimary(ButtonSendTest, "Return processSubmit(this)");
                if (info.allowSend) buttonsLeft += getButtonPrimary(ButtonSendTest, "Return processSubmit(this)");
                if (info.allowMarkReviewed) buttonsLeft +=  getButtonPrimary(ButtonMarkReviewed);
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
            } catch (Exception ex) {
                logController.handleError(core, ex);
            }
            return GetButtonBar(core, buttonsLeft, buttonsRight);
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Return a panel header with the header message reversed out of the left
        /// </summary>
        /// <param name="core"></param>
        /// <param name="HeaderMessage"></param>
        /// <param name="RightSideMessage"></param>
        /// <returns></returns>
        public static string GetHeader(coreController core, string HeaderMessage, string RightSideMessage = "") {
            string s = "";
            try {
                if (string.IsNullOrEmpty(RightSideMessage)) {
                    RightSideMessage = core.doc.profileStartTime.ToString("G");
                }
                if (isInStr(1, HeaderMessage + RightSideMessage, "\r\n")) {
                    s = ""
                        + "\r<td width=\"50%\" valign=Middle class=\"cchLeft\">"
                        + nop(HeaderMessage) + "\r</td>"
                        + "\r<td width=\"50%\" valign=Middle class=\"cchRight\">"
                        + nop(RightSideMessage) + "\r</td>";
                } else {
                    s = ""
                        + "\r<td width=\"50%\" valign=Middle class=\"cchLeft\">" + HeaderMessage + "</td>"
                        + "\r<td width=\"50%\" valign=Middle class=\"cchRight\">" + RightSideMessage + "</td>";
                }
                s = ""
                    + "\r<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr>"
                    + nop(s) + "\r</tr></table>"
                    + "";
                s = ""
                    + "\r<div class=\"ccHeaderCon\">"
                    + nop(s) + "\r</div>"
                    + "";
                //
            } catch (Exception ex) {
                logController.handleError(core, ex);
            }
            return s;
        }
        //
        // ====================================================================================================
        //
        public static string GetButtonsFromList(coreController core, List<buttonMetadata> ButtonList, bool AllowDelete, bool AllowAdd) {
            string s = "";
            try {
                foreach (buttonMetadata button in ButtonList) {

                    if (button.isDelete) {
                        s += getButtonDanger(button.value, "if(!DeleteCheck()) return false;", !AllowDelete);
                    } else if (button.isAdd) {
                        s += getButtonPrimary(button.value, "return processSubmit(this);", !AllowAdd);
                    } else if (button.isClose) {
                        s += getButtonPrimary(button.value, "window.close();");
                    } else {
                        s += getButtonPrimary(button.value);
                    }

                }
            } catch (Exception ex) {
                logController.handleError(core, ex);
            }
            return s;
        }
        //
        // ====================================================================================================
        //
        public static string GetButtonsFromList(coreController core, string ButtonList, bool AllowDelete, bool AllowAdd, string ButtonName) {
            return GetButtonsFromList(core, buttonStringToButtonList(ButtonList), AllowDelete, AllowAdd);
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Return a bootstrap button bar
        /// </summary>
        /// <param name="LeftButtons"></param>
        /// <param name="RightButtons"></param>
        /// <returns></returns>
        public static string GetButtonBar(coreController core, string LeftButtons, string RightButtons) {
            if (string.IsNullOrWhiteSpace(LeftButtons + RightButtons)) {
                return "";
            } else if (string.IsNullOrWhiteSpace(RightButtons)) {
                return "<div class=\"border bg-white p-2\">" + LeftButtons + "</div>";
            } else {
                return "<div class=\"border bg-white p-2\">" + LeftButtons + "<div class=\"float-right\">" + RightButtons + "</div></div>";
            }
        }
        //
        // ====================================================================================================
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
        public static string getForm_Index_ButtonBar(coreController core, bool AllowAdd, bool AllowDelete, int pageNumber, int recordsPerPage, int recordCnt, string contentName) {
            string result = "";
            string LeftButtons = "";
            string RightButtons = "";
            LeftButtons = LeftButtons + adminUIController.getButtonPrimary(ButtonCancel);
            LeftButtons += adminUIController.getButtonPrimary(ButtonRefresh);
            if (AllowAdd) {
                LeftButtons += adminUIController.getButtonPrimary(ButtonAdd, "");
            } else {
                LeftButtons += adminUIController.getButtonPrimary(ButtonAdd, "", true);
            }
            LeftButtons += "<span class=\"custom-divider-vertical\">&nbsp&nbsp&nbsp</span>";
            if (pageNumber == 1) {
                LeftButtons += adminUIController.getButtonPrimary(ButtonFirst, "", true);
                LeftButtons += adminUIController.getButtonPrimary(ButtonPrevious, "", true);
            } else {
                LeftButtons += adminUIController.getButtonPrimary(ButtonFirst);
                LeftButtons += adminUIController.getButtonPrimary(ButtonPrevious);
            }
            if (recordCnt > (pageNumber * recordsPerPage)) {
                LeftButtons += adminUIController.getButtonPrimary(ButtonNext);
            } else {
                LeftButtons += adminUIController.getButtonPrimary(ButtonNext, "", true);
            }
            if (AllowDelete) {
                RightButtons += adminUIController.getButtonDanger(ButtonDelete, "if(!DeleteCheck())return false;");
            } else {
                RightButtons += adminUIController.getButtonDanger(ButtonDelete, "", true);
            }
            result = GetButtonBar(core, LeftButtons, RightButtons);
            return result;
            //return adminUIController.getForm_index_pageNavigation(core, LeftButtons, RightButtons, pageNumber, recordsPerPage, PageCount, recordCnt, contentName);
        }
        //
        // ====================================================================================================
        //
        public static string getForm_index_pageNavigation(coreController core, int PageNumber, int recordsPerPage, int recordCnt, string contentName) {
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
                Nav = genericController.vbReplace(Nav, ">" + PageNumber + "<", " class=\"hit\">" + PageNumber + "<");
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
                logController.handleError(core, ex);
            }
            return result;
        }
        //
        // ====================================================================================================
        //
        public static string GetBody(coreController core, string Caption, string ButtonListLeft, string ButtonListRight, bool AllowAdd, bool AllowDelete, string Description, string ContentSummary, int ContentPadding, string Content) {
            string result = "";
            try {
                string ContentCell = "";
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string ButtonBar = null;
                string LeftButtons = "";
                string RightButtons = "";
                string CellContentSummary = "";
                //
                // Build ButtonBar
                //
                if (!string.IsNullOrEmpty(ButtonListLeft.Trim(' '))) {
                    LeftButtons = GetButtonsFromList(core, ButtonListLeft, AllowDelete, AllowAdd, "Button");
                }
                if (!string.IsNullOrEmpty(ButtonListRight.Trim(' '))) {
                    RightButtons = GetButtonsFromList(core, ButtonListRight, AllowDelete, AllowAdd, "Button");
                }
                ButtonBar = GetButtonBar(core, LeftButtons, RightButtons);
                if (!string.IsNullOrEmpty(ContentSummary)) {
                    CellContentSummary = ""
                    + "\r<div class=\"ccPanelBackground\" style=\"padding:10px;\">"
                    + nop(core.html.getPanel(ContentSummary, "ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)) + "\r</div>";
                }
                //
                ContentCell = ""
                + "\r<div style=\"padding:" + ContentPadding + "px;\">"
                + nop(Content) + "\r</div>";
                result += nop(ButtonBar) + nop(GetTitleBar(core, Caption, Description)) + nop(CellContentSummary) + nop(ContentCell) + nop(ButtonBar) + "";

                result = '\r' + htmlController.formStartMultipart(core.doc.refreshQueryString,"","ccForm") + nop(result) + '\r' + htmlController.formEnd();
            } catch (Exception ex) {
                logController.handleError(core, ex);
            }
            return result;
        }
        //
        // ====================================================================================================
        //
        public static string GetEditSubheadRow(coreController core, string Caption) {
            return "<tr><td colspan=2 class=\"ccAdminEditSubHeader\">" + Caption + "</td></tr>";
        }
        //
        // ====================================================================================================
        // GetEditPanel, An edit panel is a section of an admin page, under a subhead. When in tab mode, the subhead is blocked, and the panel is assumed to go in its own tab windows
        //
        public static string GetEditPanel(coreController core, bool AllowHeading, string PanelHeading, string PanelDescription, string PanelBody) {
            string result = "";
            try {
                stringBuilderLegacyController FastString = new stringBuilderLegacyController();
                //
                result += "<div class=\"ccPanel3DReverse ccAdminEditBody\">";
                //
                // ----- Subhead
                //
                if (AllowHeading && (!string.IsNullOrEmpty(PanelHeading))) {
                    result += "<h3 class=\"p-2 ccAdminEditHeading\">" + PanelHeading + "</h3>";
                }
                //
                // ----- Description
                //
                if (!string.IsNullOrEmpty(PanelDescription)) {
                    result += "<p class=\"p-2 ccAdminEditDescription\">" + PanelDescription + "</p>";
                }
                //
                result += PanelBody + "</div>";
            } catch (Exception ex) {
                logController.handleError(core, ex);
            }
            return result;
        }
        //
        // ====================================================================================================
        // Edit Table Open
        public static string EditTableOpen {
            get {
                return "<table border=0 cellpadding=3 cellspacing=0 width=\"100%\">";
            }
        }
        //
        // ====================================================================================================
        // Edit Table Close
        public static string EditTableClose {
            get {
                string tempEditTableClose = null;
                tempEditTableClose = "<tr>"
                    + "<td width=20%><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=1 ></td>"
                    + "<td width=80%><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=1 ></td>"
                    + "</tr>"
                    + "</table>";
                return tempEditTableClose;
            }

        }
        //
        // ====================================================================================================
        //
        private static string GetReport_Cell(coreController core, string Copy, string Align, int Columns, int RowPointer) {
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
            tempGetReport_Cell = "\r\n<td valign=\"middle\" align=\"" + iAlign + "\" class=\"" + Style + "\"";
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
        // ====================================================================================================
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
        private static string GetReport_CellHeader(coreController core, int ColumnPtr, string Title, string Width, string Align, string ClassStyle, string RefreshQueryString, SortingStateEnum SortingState) {
            string result = "";
            try {
                string Style = null;
                string Copy = null;
                string QS = null;
                int WidthTest = 0;
                //
                if (string.IsNullOrEmpty(Title)) {
                    Copy = "&nbsp;";
                } else {
                    Copy = genericController.vbReplace(Title, " ", "&nbsp;");
                    //Copy = "<nobr>" & Title & "</nobr>"
                }
                Style = "VERTICAL-ALIGN:bottom;";
                if (string.IsNullOrEmpty(Align)) {
                } else {
                    Style = Style + "TEXT-ALIGN:" + Align + ";";
                }
                //
                switch (SortingState) {
                    case SortingStateEnum.SortableNotSet:
                        QS = genericController.modifyQueryString(RefreshQueryString, "ColSort", ((int)SortingStateEnum.SortableSetAZ).ToString(), true);
                        QS = genericController.modifyQueryString(QS, "ColPtr", ColumnPtr.ToString(), true);
                        Copy = "<a href=\"?" + QS + "\" title=\"Sort A-Z\" class=\"ccAdminListCaption\">" + Copy + "</a>";
                        break;
                    case SortingStateEnum.SortableSetza:
                        QS = genericController.modifyQueryString(RefreshQueryString, "ColSort", ((int)SortingStateEnum.SortableSetAZ).ToString(), true);
                        QS = genericController.modifyQueryString(QS, "ColPtr", ColumnPtr.ToString(), true);
                        Copy = "<a href=\"?" + QS + "\" title=\"Sort A-Z\" class=\"ccAdminListCaption\">" + Copy + "<img src=\"/ccLib/images/arrowup.gif\" width=8 height=8 border=0></a>";
                        break;
                    case SortingStateEnum.SortableSetAZ:
                        QS = genericController.modifyQueryString(RefreshQueryString, "ColSort", ((int)SortingStateEnum.SortableSetza).ToString(), true);
                        QS = genericController.modifyQueryString(QS, "ColPtr", ColumnPtr.ToString(), true);
                        Copy = "<a href=\"?" + QS + "\" title=\"Sort Z-A\" class=\"ccAdminListCaption\">" + Copy + "<img src=\"/ccLib/images/arrowdown.gif\" width=8 height=8 border=0></a>";
                        break;
                }
                //
                if (!string.IsNullOrEmpty(Width)) {
                    WidthTest = genericController.encodeInteger(Width.ToLower().Replace("px", ""));
                    if (WidthTest != 0) {
                        Style = Style + "width:" + WidthTest + "px;";
                        Copy += "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"" + WidthTest + "\" height=1 border=0>";
                        //Copy = Copy & "<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""" & WidthTest & """ height=1 border=0>"
                    } else {
                        Style = Style + "width:" + Width + ";";
                    }
                }
                result = "\r\n<td style=\"" + Style + "\" class=\"" + ClassStyle + "\">" + Copy + "</td>";
            } catch (Exception ex) {
                logController.handleError(core, ex);
            }
            return result;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// returns the integer column ptr of the column last selected
        /// </summary>
        /// <param name="core"></param>
        /// <param name="DefaultSortColumnPtr"></param>
        /// <returns></returns>
        public static int GetReportSortColumnPtr(coreController core, int DefaultSortColumnPtr) {
            int tempGetReportSortColumnPtr = 0;
            string VarText;
            //
            VarText = core.docProperties.getText("ColPtr");
            tempGetReportSortColumnPtr = genericController.encodeInteger(VarText);
            if ((tempGetReportSortColumnPtr == 0) && (VarText != "0")) {
                tempGetReportSortColumnPtr = DefaultSortColumnPtr;
            }
            return tempGetReportSortColumnPtr;
        }
        //
        // ====================================================================================================
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
        public static int GetReportSortType(coreController core) {
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
        // ====================================================================================================
        //
        public static string GetReport(coreController core, int RowCount, string[] ColCaption, string[] ColAlign, string[] ColWidth, string[,] Cells, int PageSize, int PageNumber, string PreTableCopy, string PostTableCopy, int DataRowCount, string ClassStyle) {
            string result = "";
            try {
                int ColCnt = Cells.GetUpperBound(1);
                bool[] ColSortable = new bool[ColCnt + 1];
                for (int Ptr = 0; Ptr < ColCnt; Ptr++) {
                    ColSortable[Ptr] = false;
                }
                //
                result = GetReport2(core, RowCount, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, ClassStyle, ColSortable, 0);
            } catch (Exception ex) {
                logController.handleError(core, ex);
            }
            return result;
        }
        //
        // ====================================================================================================
        //
        public static string GetReport2(coreController core, int RowCount, string[] ColCaption, string[] ColAlign, string[] ColWidth, string[,] Cells, int PageSize, int PageNumber, string PreTableCopy, string PostTableCopy, int DataRowCount, string ClassStyle, bool[] ColSortable, int DefaultSortColumnPtr) {
            string result = "";
            try {
                string RQS = null;
                int RowBAse = 0;
                stringBuilderLegacyController Content = new stringBuilderLegacyController();
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
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
                //If IsArray(Cells) Then
                ColumnCount = Cells.GetUpperBound(1);
                //End If
                RQS = core.doc.refreshQueryString;
                //
                SortColPtr = GetReportSortColumnPtr(core, DefaultSortColumnPtr);
                SortColType = GetReportSortType(core);
                //
                // ----- Start the table
                //
                Content.Add(htmlController.tableStart(3, 1, 0));
                //
                // ----- Header
                //
                Content.Add("\r\n<tr>");
                Content.Add(GetReport_CellHeader(core, 0, "Row", "50", "Right", "ccAdminListCaption", RQS, SortingStateEnum.NotSortable));
                for (ColumnPtr = 0; ColumnPtr < ColumnCount; ColumnPtr++) {
                    ColumnWidth = ColWidth[ColumnPtr];
                    if (!ColSortable[ColumnPtr]) {
                        //
                        // not sortable column
                        //
                        Content.Add(GetReport_CellHeader(core, ColumnPtr, ColCaption[ColumnPtr], ColumnWidth, ColAlign[ColumnPtr], "ccAdminListCaption", RQS, SortingStateEnum.NotSortable));
                    } else if (ColumnPtr == SortColPtr) {
                        //
                        // This is the current sort column
                        //
                        Content.Add(GetReport_CellHeader(core, ColumnPtr, ColCaption[ColumnPtr], ColumnWidth, ColAlign[ColumnPtr], "ccAdminListCaption", RQS, (SortingStateEnum)SortColType));
                    } else {
                        //
                        // Column is sortable, but not selected
                        //
                        Content.Add(GetReport_CellHeader(core, ColumnPtr, ColCaption[ColumnPtr], ColumnWidth, ColAlign[ColumnPtr], "ccAdminListCaption", RQS, SortingStateEnum.SortableNotSet));
                    }

                    //If ColumnPtr = SortColPtr Then
                    //    '
                    //    ' This column is currently the active sort
                    //    '
                    //    Call Content.Add(GetReport_CellHeader(ColumnPtr, ColCaption(ColumnPtr), ColumnWidth, ColAlign(ColumnPtr), "ccAdminListCaption", RQS, SortColType))
                    //Else
                    //    Call Content.Add(GetReport_CellHeader(ColumnPtr, ColCaption(ColumnPtr), ColumnWidth, ColAlign(ColumnPtr), "ccAdminListCaption", RQS, SortingStateEnum.SortableNotSet))
                    //End If
                }
                Content.Add("\r\n</tr>");
                //
                // ----- Data
                //
                if (RowCount == 0) {
                    Content.Add("\r\n<tr>");
                    Content.Add(GetReport_Cell(core, (RowBAse + RowPointer).ToString(), "right", 1, RowPointer));
                    Content.Add(GetReport_Cell(core, "-- End --", "left", ColumnCount, 0));
                    Content.Add("\r\n</tr>");
                } else {
                    RowBAse = (ReportPageSize * (ReportPageNumber - 1)) + 1;
                    for (RowPointer = 0; RowPointer < RowCount; RowPointer++) {
                        Content.Add("\r\n<tr>");
                        Content.Add(GetReport_Cell(core, (RowBAse + RowPointer).ToString(), "right", 1, RowPointer));
                        for (ColumnPtr = 0; ColumnPtr < ColumnCount; ColumnPtr++) {
                            Content.Add(GetReport_Cell(core, Cells[RowPointer, ColumnPtr], ColAlign[ColumnPtr], 1, RowPointer));
                        }
                        Content.Add("\r\n</tr>");
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
                            WorkingQS = genericController.modifyQueryString(WorkingQS, "GotoPage", "1", true);
                            result += "<a href=\"" + core.webServer.requestPage + "?" + WorkingQS + "\">1</A>...&nbsp;";
                        }
                        WorkingQS = core.doc.refreshQueryString;
                        WorkingQS = genericController.modifyQueryString(WorkingQS, RequestNamePageSize, ReportPageSize.ToString(), true);
                        while ((PagePointer <= PageCount) && (LinkCount < 20)) {
                            if (PagePointer == ReportPageNumber) {
                                result += PagePointer + "&nbsp;";
                            } else {
                                WorkingQS = genericController.modifyQueryString(WorkingQS, RequestNamePageNumber, PagePointer.ToString(), true);
                                result += "<a href=\"" + core.webServer.requestPage + "?" + WorkingQS + "\">" + PagePointer + "</A>&nbsp;";
                            }
                            PagePointer = PagePointer + 1;
                            LinkCount = LinkCount + 1;
                        }
                        if (PagePointer < PageCount) {
                            WorkingQS = genericController.modifyQueryString(WorkingQS, RequestNamePageNumber, PageCount.ToString(), true);
                            result += "...<a href=\"" + core.webServer.requestPage + "?" + WorkingQS + "\">" + PageCount + "</A>&nbsp;";
                        }
                        if (ReportPageNumber < PageCount) {
                            WorkingQS = genericController.modifyQueryString(WorkingQS, RequestNamePageNumber, (ReportPageNumber + 1).ToString(), true);
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
                logController.handleError(core, ex);
            }
            return result;
        }
        //
        // ====================================================================================================
        //
        public static string GetFormBodyAdminOnly() {
            return "<div class=\"ccError\" style=\"margin:10px;padding:10px;background-color:white;\">This page requires administrator permissions.</div>";
        }
        //
        // ====================================================================================================
        //
        public static string getButtonPrimary(string buttonValue, string onclick = "", bool disabled = false, string htmlId = "", string htmlName = "button") {
            return htmlController.getHtmlInputSubmit(buttonValue, htmlName, htmlId, onclick, disabled, "btn btn-primary mr-1 btn-sm");
            // string htmlClass = "btn btn-primary mr-1 btn-sm btn-sm";
            // string button = "<input type=submit name=button value=\"" + buttonValue + "\" id=\"" + htmlId + "\"OnClick=\"" + onclick + "\" class=\"" + htmlClass + "\">";
        }
        //
        // ====================================================================================================
        //
        public static string getButtonDanger(string buttonValue, string onclick = "", bool disabled = false, string htmlId = "") {
            return htmlController.getHtmlInputSubmit(buttonValue, "button", htmlId, onclick, disabled, "btn btn-danger mr-1 btn-sm");
            // string htmlClass = "btn btn-primary mr-1 btn-sm btn-sm";
            // string button = "<input type=submit name=button value=\"" + buttonValue + "\" id=\"" + htmlId + "\"OnClick=\"" + onclick + "\" class=\"" + htmlClass + "\">";
        }
        //
        // ====================================================================================================
        //
        public static List<buttonMetadata> buttonStringToButtonList(string ButtonList) {
            var result = new List<buttonMetadata>();
            string[] Buttons = null;
            if (!string.IsNullOrEmpty(ButtonList.Trim(' '))) {
                Buttons = ButtonList.Split(',');
                foreach (string buttonValue in Buttons) {
                    string buttonValueTrim = buttonValue.Trim();
                    result.Add(new buttonMetadata() {
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
        /// <summary>
        /// The panel at the top of the edit page that describes the content being edited
        /// </summary>
        /// <param name="core"></param>
        /// <param name="headerInfo"></param>
        /// <returns></returns>
        public static string getTitleBarDetails(coreController core, recordEditHeaderInfoClass headerInfo) {
            string result = "";
            if (headerInfo.recordId == 0) {
                result = "<div>New Record</div>";
            } else {
                result = "<table border=0 cellpadding=0 cellspacing=0 style=\"width:90%\">";
                result += "<tr><td width=\"50%\">"
                + "Name: " + headerInfo.recordName + "<br>Record ID: " + headerInfo.recordId + "</td><td width=\"50%\">";
                //
                string CreatedCopy = "";
                CreatedCopy = CreatedCopy + " " + headerInfo.recordDateAdded.ToString();
                //
                string CreatedBy = "the system";
                if (headerInfo.recordAddedById != 0) {
                    int CS = core.db.csOpenSql_rev("default", "select Name,Active from ccMembers where id=" + headerInfo.recordAddedById);
                    if (core.db.csOk(CS)) {
                        string Name = core.db.csGetText(CS, "name");
                        bool Active = core.db.csGetBoolean(CS, "active");
                        if (!Active && (!string.IsNullOrEmpty(Name))) {
                            CreatedBy = "Inactive user " + Name;
                        } else if (!Active) {
                            CreatedBy = "Inactive user #" + headerInfo.recordAddedById;
                        } else if (string.IsNullOrEmpty(Name)) {
                            CreatedBy = "Unnamed user #" + headerInfo.recordAddedById;
                        } else {
                            CreatedBy = Name;
                        }
                    } else {
                        CreatedBy = "deleted user #" + headerInfo.recordAddedById;
                    }
                    core.db.csClose(ref CS);
                }
                if (!string.IsNullOrEmpty(CreatedBy)) {
                    CreatedCopy = CreatedCopy + " by " + CreatedBy;
                } else {
                }
                result += "Created:" + CreatedCopy;
                //
                string ModifiedCopy = "";
                if (headerInfo.recordDateModified == DateTime.MinValue) {
                    ModifiedCopy = CreatedCopy;
                } else {
                    ModifiedCopy = ModifiedCopy + " " + headerInfo.recordDateModified;
                    CreatedBy = "the system";
                    if (headerInfo.recordModifiedById != 0) {
                        int CS = core.db.csOpenSql_rev("default", "select Name,Active from ccMembers where id=" + headerInfo.recordModifiedById);
                        if (core.db.csOk(CS)) {
                            string Name = core.db.csGetText(CS, "name");
                            bool Active = core.db.csGetBoolean(CS, "active");
                            if (!Active && (!string.IsNullOrEmpty(Name))) {
                                CreatedBy = "Inactive user " + Name;
                            } else if (!Active) {
                                CreatedBy = "Inactive user #" + headerInfo.recordModifiedById;
                            } else if (string.IsNullOrEmpty(Name)) {
                                CreatedBy = "Unnamed user #" + headerInfo.recordModifiedById;
                            } else {
                                CreatedBy = Name;
                            }
                        } else {
                            CreatedBy = "deleted user #" + headerInfo.recordModifiedById;
                        }
                        core.db.csClose(ref CS);
                    }
                    if (!string.IsNullOrEmpty(CreatedBy)) {
                        ModifiedCopy = ModifiedCopy + " by " + CreatedBy;
                    } else {
                    }
                }
                result += "<br>Last Modified:" + ModifiedCopy;
                if ((headerInfo.recordLockExpiresDate == null) | (headerInfo.recordLockExpiresDate < DateTime.Now)) {
                    //
                    // Add Edit Locking to right panel
                    personModel personLock = personModel.create(core, headerInfo.recordLockById);
                    if (personLock != null) {
                        result += "<br><b>Record is locked by " + personLock.name + " until " + headerInfo.recordLockExpiresDate + "</b>";
                    }
                }
                //
                result += "</td></tr>";
                result += "</table>";
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
        public static string getDefaultEditor_Bool(coreController core, string htmlName, bool htmlValue, bool readOnly = false, string htmlId = "") {
            string result = htmlController.div(htmlController.checkbox(htmlName, htmlValue, htmlId, readOnly), "checkbox");
            if (readOnly)  result += htmlController.inputHidden(htmlName, htmlValue);
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
        public static string getDefaultEditor_Text(coreController core, string fieldName, string fieldValue, bool readOnly = false, string htmlId = "") {
            if ((fieldValue.IndexOf("\n") == -1) && (fieldValue.Length < 80)) {
                //
                // text field shorter then 40 characters without a CR
                return htmlController.inputText( core,fieldName, fieldValue, 1, -1, htmlId, false, readOnly, "text form-control", 255);
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
        public static string getDefaultEditor_TextArea(coreController core, string fieldName, string fieldValue, bool readOnly = false, string htmlId = "") {
            //
            // longer text data, or text that contains a CR
            return htmlController.inputTextarea(core, fieldName, fieldValue, 10, -1, htmlId, false, readOnly, "text form-control", false, 255);
        }
        //
        // ====================================================================================================
        //
        public static string getDefaultEditor_Html( coreController core, string fieldName, string fieldValue, string editorAddonListJSON, string styleList, string styleOptionList, bool readONly = false, string htmlId = "" ) {
            string result = "";
            if (readONly) {
                result += htmlController.inputHidden(fieldName, fieldValue);
            } else if (string.IsNullOrEmpty(fieldValue)) {
                //
                // editor needs a starting p tag to setup correctly
                fieldValue = HTMLEditorDefaultCopyNoCr;
            }
            result += core.html.getFormInputHTML(fieldName.ToLower(), fieldValue, "500", "", readONly, true, editorAddonListJSON, styleList, styleOptionList);
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
        public static string getDefaultEditor_Password(coreController core, string fieldName, string fieldValue, bool readOnly = false, string htmlId = "") {
            return htmlController.inputText( core,fieldName, fieldValue, -1, -1, htmlId, true, readOnly, "password form-control", 255);
        }
        //
        // ====================================================================================================
        /// <summary>
        /// admin editor for a lookup field into a content table
        /// </summary>
        /// <param name="core"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="lookupContentID"></param>
        /// <param name="readOnly"></param>
        /// <param name="htmlId"></param>
        /// <param name="WhyReadOnlyMsg"></param>
        /// <param name="fieldRequired"></param>
        /// <param name="IsEmptyList"></param>
        /// <returns></returns>
        public static string getDefaultEditor_LookupContent( coreController core, string fieldName, int fieldValue, int lookupContentID, ref bool IsEmptyList, bool readOnly = false, string htmlId = "", string WhyReadOnlyMsg = "", bool fieldRequired = false, string sqlFilter = "") {
            string result = "";
            string LookupContentName = "";
            if (lookupContentID != 0) LookupContentName = genericController.encodeText(Models.Complex.cdefModel.getContentNameByID(core, lookupContentID));
            if (readOnly) {
                //
                // ----- Lookup ReadOnly
                result += (htmlController.inputHidden(fieldName, genericController.encodeText(fieldValue)));
                if (!string.IsNullOrEmpty(LookupContentName)) {
                    int CSLookup = core.db.csOpen2(LookupContentName, fieldValue, false, false, "Name,ContentControlID");
                    if (core.db.csOk(CSLookup)) {
                        if (core.db.csGet(CSLookup, "Name") == "") {
                            result += ("No Name");
                        } else {
                            result += (htmlController.encodeHtml(core.db.csGet(CSLookup, "Name")));
                        }
                        result += ("&nbsp;[<a TabIndex=-1 href=\"?" + rnAdminForm + "=4&cid=" + lookupContentID + "&id=" + fieldValue.ToString() + "\" target=\"_blank\">View details in new window</a>]");
                    } else {
                        result += ("None");
                    }
                    core.db.csClose(ref CSLookup);
                    result += ("&nbsp;[<a TabIndex=-1 href=\"?cid=" + lookupContentID + "\" target=\"_blank\">See all " + LookupContentName + "</a>]");
                }
                result += WhyReadOnlyMsg;
            } else {
                //
                // -- not readonly
                string nonLabel = (fieldRequired) ? "" : "None";
                result += core.html.selectFromContent(fieldName, fieldValue, LookupContentName, sqlFilter, nonLabel, "", ref IsEmptyList, "select form-control");
                if (fieldValue != 0) {
                    int CSPointer = core.db.csOpen2(LookupContentName, fieldValue, false, false, "ID");
                    if (core.db.csOk(CSPointer)) {
                        result += ("&nbsp;[<a TabIndex=-1 href=\"?" + rnAdminForm + "=4&cid=" + lookupContentID + "&id=" + fieldValue.ToString() + "\" target=\"_blank\">Details</a>]");
                    }
                    core.db.csClose(ref CSPointer);
                }
                result += ("&nbsp;[<a TabIndex=-1 href=\"?cid=" + lookupContentID + "\" target=\"_blank\">See all " + LookupContentName + "</a>]");

            }
            return result;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// admin editor for a lookup field into a static list
        /// </summary>
        /// <param name="core"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="lookups"></param>
        /// <param name="readOnly"></param>
        /// <param name="htmlId"></param>
        /// <param name="WhyReadOnlyMsg"></param>
        /// <param name="fieldRequired"></param>
        /// <returns></returns>
        public static string getDefaultEditor_LookupList(coreController core, string fieldName, int fieldValue, string[] lookups, bool readOnly = false, string htmlId = "", string WhyReadOnlyMsg = "", bool fieldRequired = false) {
            string result = "";
            if (readOnly) {
                //
                // ----- Lookup ReadOnly
                result += (htmlController.inputHidden(fieldName, genericController.encodeText(fieldValue)));
                if (fieldValue < 1) {
                    result += ("None");
                } else if (fieldValue > (lookups.GetUpperBound(0) + 1)) {
                    result += ("None");
                } else {
                    result += lookups[fieldValue - 1];
                }
                result += WhyReadOnlyMsg;
            } else {
                if (!fieldRequired) {
                    result += core.html.selectFromList(fieldName, fieldValue, lookups, "Select One", "", "select form-control");
                } else {
                    result += core.html.selectFromList(fieldName, fieldValue, lookups, "", "", "select form-control");
                }

            }
            return result;
        }
        //
        // ====================================================================================================
        //
        public static string getDefaultEditor_Date( coreController core, string fieldName, DateTime FieldValueDate, bool readOnly = false, string htmlId = "", bool fieldRequired = false, string WhyReadOnlyMsg = "") {
            string result = "";
            string fieldValue_text = "";
            if (FieldValueDate == DateTime.MinValue) {
                fieldValue_text = "";
            } else {
                fieldValue_text = encodeText(FieldValueDate);
            }
            if (readOnly) {
                //
                // -- readOnly
                result += htmlController.inputHidden(fieldName, fieldValue_text);
                result += htmlController.inputText( core,fieldName, fieldValue_text, -1, -1, "", false, true, "date form-control");
                result += WhyReadOnlyMsg;
            } else {
                //
                // -- editable
                result += htmlController.inputDate( core,fieldName, encodeDate(fieldValue_text),"",htmlId, "date form-control", readOnly, fieldRequired);
            }
            return result;
        }
        //
        // ====================================================================================================
        //
        public static string getDefaultEditor_memberSelect(coreController core, string htmlName, int selectedRecordId, int groupId, string groupName, bool readOnly = false, string htmlId = "", bool fieldRequired = false, string WhyReadOnlyMsg = "") {
            string EditorString = "";
            if (readOnly) {
                //
                // -- readOnly
                EditorString += htmlController.inputHidden(htmlName, selectedRecordId.ToString() );
                if (selectedRecordId == 0) {
                    EditorString += "None";
                } else {
                    var selectedUser = personModel.create(core, selectedRecordId);
                    if ( selectedUser==null) {
                        EditorString += "Deleted";
                    } else {
                        EditorString += (string.IsNullOrWhiteSpace(selectedUser.name)) ? "No Name" : htmlController.encodeHtml(selectedUser.name);
                        EditorString += ("&nbsp;[<a TabIndex=-1 href=\"?af=4&cid=" + selectedUser.contentControlID.ToString() + "&id=" + selectedRecordId.ToString() + "\" target=\"_blank\">View details in new window</a>]");
                    }
                }
                EditorString += WhyReadOnlyMsg;
            } else {
                //
                // -- editable
                EditorString += core.html.selectUserFromGroup(htmlName, selectedRecordId, groupId, "", (fieldRequired) ? "" : "None", htmlId, "select form-control");
                if (selectedRecordId != 0) {
                    var selectedUser = personModel.create(core, selectedRecordId);
                    if (selectedUser == null) {
                        EditorString += "Deleted";
                    } else {
                        string recordName = (string.IsNullOrWhiteSpace(selectedUser.name)) ? "No Name" : htmlController.encodeHtml(selectedUser.name);
                        EditorString += "&nbsp;[Edit <a TabIndex=-1 href=\"?af=4&cid=" + selectedUser.contentControlID.ToString() + "&id=" + selectedRecordId.ToString() + "\">" + htmlController.encodeHtml( recordName ) + "</a>]";
                    }
                }
                EditorString += ("&nbsp;[Select from members of <a TabIndex=-1 href=\"?cid=" + Models.Complex.cdefModel.getContentId(core, "groups") + "\">" + groupName + "</a>]");
            }
            return EditorString;
        }
        //
        // ====================================================================================================
        //
        public static string getDefaultEditor_manyToMany(coreController core, cdefFieldModel field, string htmlName, string currentValueCommaList, int editRecordId, bool readOnly = false, string WhyReadOnlyMsg = "" ) {
            string result = "";
            //
            string MTMContent0 =   cdefModel.getContentNameByID(core, field.contentId);
            string MTMContent1 = cdefModel.getContentNameByID(core, field.manyToManyContentID);
            string MTMRuleContent = cdefModel.getContentNameByID(core, field.manyToManyRuleContentID);
            string MTMRuleField0 = field.ManyToManyRulePrimaryField;
            string MTMRuleField1 = field.ManyToManyRuleSecondaryField;
            result += core.html.getCheckList(htmlName, MTMContent0, editRecordId, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, "", "", false, false, currentValueCommaList);
            //result += core.html.getCheckList("ManyToMany" + field.id, MTMContent0, editRecordId, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1);
            result += WhyReadOnlyMsg;
            return result;
        }
        //
        //====================================================================================================
        //
        public static string getDefaultEditor_SelectorString(coreController core, string SitePropertyName, string SitePropertyValue, string selector) {
            string result = "";
            try {
                Dictionary<string, string> instanceOptions = new Dictionary<string, string> {
                    { SitePropertyName, SitePropertyValue }
                };
                string ExpandedSelector = "";
                Dictionary<string, string> addonInstanceProperties = new Dictionary<string, string>();
                core.addon.buildAddonOptionLists(ref addonInstanceProperties, ref ExpandedSelector, SitePropertyName + "=" + selector, instanceOptions, "0", true);
                //buildAddonOptionLists(ref addonInstanceProperties, ref ExpandedSelector, SitePropertyName + "=" + selector, instanceOptions, "0", true);
                int Pos = genericController.vbInstr(1, ExpandedSelector, "[");
                if (Pos != 0) {
                    //
                    // List of Options, might be select, radio or checkbox
                    //
                    string LCaseOptionDefault = genericController.vbLCase(ExpandedSelector.Left(Pos - 1));
                    int PosEqual = genericController.vbInstr(1, LCaseOptionDefault, "=");

                    if (PosEqual > 0) {
                        LCaseOptionDefault = LCaseOptionDefault.Substring(PosEqual);
                    }

                    LCaseOptionDefault = genericController.decodeNvaArgument(LCaseOptionDefault);
                    ExpandedSelector = ExpandedSelector.Substring(Pos);
                    Pos = genericController.vbInstr(1, ExpandedSelector, "]");
                    string OptionSuffix = "";
                    if (Pos > 0) {
                        if (Pos < ExpandedSelector.Length) {
                            OptionSuffix = genericController.vbLCase((ExpandedSelector.Substring(Pos)).Trim(' '));
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
                            Pos = genericController.vbInstr(1, OptionValue_AddonEncoded, ":");
                            string OptionCaption = null;
                            string OptionValue = null;
                            if (Pos == 0) {
                                OptionValue = genericController.decodeNvaArgument(OptionValue_AddonEncoded);
                                OptionCaption = OptionValue;
                            } else {
                                OptionCaption = genericController.decodeNvaArgument(OptionValue_AddonEncoded.Left(Pos - 1));
                                OptionValue = genericController.decodeNvaArgument(OptionValue_AddonEncoded.Substring(Pos));
                            }
                            switch (OptionSuffix) {
                                case "checkbox":
                                    //
                                    // Create checkbox addon_execute_getFormContent_decodeSelector
                                    //
                                    bool selected = (genericController.vbInstr(1, "," + LCaseOptionDefault + ",", "," + genericController.vbLCase(OptionValue) + ",") != 0);
                                    result = htmlController.checkbox(SitePropertyName + OptionPtr, selected, "", false, "", false, OptionValue, OptionCaption);
                                    //if (genericController.vbInstr(1, "," + LCaseOptionDefault + ",", "," + genericController.vbLCase(OptionValue) + ",") != 0) {
                                    //    result += "<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" + SitePropertyName + OptionPtr + "\" value=\"" + OptionValue + "\" checked=\"checked\">" + OptionCaption + "</div>";
                                    //} else {
                                    //    result += "<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" + SitePropertyName + OptionPtr + "\" value=\"" + OptionValue + "\" >" + OptionCaption + "</div>";
                                    //}
                                    break;
                                case "radio":
                                    //
                                    // Create Radio addon_execute_getFormContent_decodeSelector
                                    //
                                    if (genericController.vbLCase(OptionValue) == LCaseOptionDefault) {
                                        result += "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + SitePropertyName + "\" value=\"" + OptionValue + "\" checked=\"checked\" >" + OptionCaption + "</div>";
                                    } else {
                                        result += "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + SitePropertyName + "\" value=\"" + OptionValue + "\" >" + OptionCaption + "</div>";
                                    }
                                    break;
                                default:
                                    //
                                    // Create select addon_execute_result
                                    //
                                    if (genericController.vbLCase(OptionValue) == LCaseOptionDefault) {
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
                            Copy += htmlController.inputHidden(SitePropertyName + "CheckBoxCnt", OptionCnt);
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

                    selector = genericController.decodeNvaArgument(selector);
                    result = getDefaultEditor_Text(core, SitePropertyName, selector);
                }

                //FastString = null;
            } catch (Exception ex) {
                logController.handleError(core, ex);
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
        public static string getEditRow(coreController core, string EditorString, string Caption, string editorHelpRow, bool fieldRequired = false, bool ignore = false, string fieldHtmlId = "") {
            return htmlController.div(htmlController.label(Caption, fieldHtmlId) + htmlController.div(EditorString, "ml-5") + htmlController.div(editorHelpRow, "small ml-5"), "p-2 ");
        }
        //
        // ====================================================================================================
        //
        public static string getEditRowLegacy(coreController core, string HTMLFieldString, string Caption, string HelpMessage = "", bool FieldRequired = false, bool AllowActiveEdit = false, string ignore0 = "") {
            return getEditRow(core, HTMLFieldString, Caption, HelpMessage, FieldRequired, AllowActiveEdit, ignore0);
            //string tempGetEditRow = null;
            //try {
            //    stringBuilderLegacyController FastString = new stringBuilderLegacyController();
            //    string Copy = null;
            //    //
            //    // Left Side
            //    //
            //    Copy = Caption;
            //    if (string.IsNullOrEmpty(Copy)) {
            //        Copy = "&nbsp;";
            //    }
            //    tempGetEditRow = "<tr><td class=\"ccEditCaptionCon\"><nobr>" + Copy + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=1 height=15 >";
            //    tempGetEditRow = tempGetEditRow + "</nobr></td>";
            //    //
            //    // Right Side
            //    //
            //    Copy = HTMLFieldString;
            //    if (string.IsNullOrEmpty(Copy)) {
            //        Copy = "&nbsp;";
            //    }
            //    Copy = "<div class=\"ccEditorCon\">" + Copy + "</div>";
            //    Copy += "<div class=\"ccEditorHelpCon\"><div class=\"closed\">" + HelpMessage + "</div></div>";
            //    tempGetEditRow += "<td class=\"ccEditFieldCon\">" + Copy + "</td></tr>";
            //} catch (Exception ex) {
            //    logController.handleError(core, ex);
            //}
            //return tempGetEditRow;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Title Bar for the index page
        /// </summary>
        /// <param name="core"></param>
        /// <param name="IndexConfig"></param>
        /// <param name="adminContent"></param>
        /// <param name="recordCnt"></param>
        /// <param name="ContentAccessLimitMessage"></param>
        /// <returns></returns>
        public static string getForm_Index_Header(coreController core, indexConfigClass IndexConfig, cdefModel adminContent, int recordCnt, string ContentAccessLimitMessage) {
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
                indexConfigFindWordClass findWord = kvp.Value;
                if (!string.IsNullOrEmpty(findWord.Name)) {
                    string FieldCaption = cdefModel.GetContentFieldProperty(core, adminContent.name, findWord.Name, "caption");
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
                string ContentName = cdefModel.getContentNameByID(core, IndexConfig.SubCDefID);
                if (!string.IsNullOrEmpty(ContentName)) {
                    filterLine = filterLine + ", in Sub-content '" + ContentName + "'";
                }
            }
            //
            // add groups to caption
            //
            if ((adminContent.contentTableName.ToLower() == "ccmembers") && (IndexConfig.GroupListCnt > 0)) {
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
                indexConfigSortClass sort = kvp.Value;
                if (sort.direction > 0) {
                    sortLine = sortLine + ", then " + adminContent.fields[sort.fieldName].caption;
                    if (sort.direction > 1) {
                        sortLine += " reverse";
                    }
                }
            }
            string pageNavigation = getForm_index_pageNavigation(core, IndexConfig.PageNumber, IndexConfig.RecordsPerPage, recordCnt, adminContent.name);
            Title = htmlController.div("<strong>" + adminContent.name + "</strong><div style=\"float:right;\">" + pageNavigation + "</div>");
            int TitleRows = 0;
            if (!string.IsNullOrEmpty(filterLine)) {
                string link = "/" + core.appConfig.adminRoute + "?cid=" + adminContent.id + "&af=1&IndexFilterRemoveAll=1";
                Title += htmlController.div(getIconRemove(link) + "&nbsp;Filter: " + htmlController.encodeHtml(filterLine.Substring(2)));
                TitleRows = TitleRows + 1;
            }
            if (!string.IsNullOrEmpty(sortLine)) {
                string link = "/" + core.appConfig.adminRoute + "?cid=" + adminContent.id + "&af=1&IndexSortRemoveAll=1";
                Title += htmlController.div(getIconRemove(link) + "&nbsp;Sort: " + htmlController.encodeHtml(sortLine.Substring(6)));
                TitleRows = TitleRows + 1;
            }
            if (!string.IsNullOrEmpty(ContentAccessLimitMessage)) {
                Title +=  "<div style=\"clear:both\">" + ContentAccessLimitMessage + "</div>";
                TitleRows = TitleRows + 1;
            }
            return Title;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// get linked icon for remove (red x)
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public static string getIconRemove(string link, string cssColor = "red") {
            return "<a href=\"" + link + "\"><span class=\"fa fa-remove\" style=\"color:" + cssColor + ";\"></span></a>";
        }
        //
        // ====================================================================================================
        /// <summary>
        /// get linked icon for refresh (green refresh)
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public static  string getIconRefresh(string link, string cssColor = "green") {
            return "<a href=\"" + link + "\"><span class=\"fa fa-refresh\" style=\"color:" + cssColor + ";\"></span></a>";
        }
    }
    //
    //====================================================================================================
    /// <summary>
    /// structure used in admin edit forms at the top
    /// </summary>
    public class recordEditHeaderInfoClass {
        //public string description;
        public string recordName;
        public int recordId;
        public DateTime recordDateAdded;
        public DateTime recordDateModified;
        public int recordAddedById;
        public int recordModifiedById;
        public DateTime recordLockExpiresDate;
        public int recordLockById;
    }
    //
    public class editButtonBarInfoClass {
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
    }
}
