
using System;
using System.Collections.Generic;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
using System.Text;
using Contensive.Processor.Exceptions;
using Contensive.Processor;

namespace Contensive.Addons.AdminSite.Controllers {
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
        /// <summary>
        /// Storage for current EditRecord, loaded in LoadEditRecord
        /// </summary>
        public class EditRecordFieldClass {
            public object dbValue;
            public object value;
        }
        /// <summary>
        /// 
        /// </summary>
        public class EditRecordClass {
            public Dictionary<string, EditRecordFieldClass> fieldsLc = new Dictionary<string, EditRecordFieldClass>();
            /// <summary>
            /// ID field of edit record (Record to be edited)
            /// </summary>
            public int id;
            /// <summary>
            /// ParentID field of edit record (Record to be edited)
            /// </summary>
            public int parentID;
            /// <summary>
            /// name field of edit record
            /// </summary>
            public string nameLc;
            /// <summary>
            /// active field of the edit record
            /// </summary>
            public bool active;
            /// <summary>
            /// ContentControlID of the edit record
            /// </summary>
            public int contentControlId;
            /// <summary>
            /// denormalized name from contentControlId property
            /// </summary>
            public string contentControlId_Name;
            /// <summary>
            /// Used for Content Watch Link Label if default
            /// </summary>
            public string menuHeadline;
            /// <summary>
            /// Used for control section display
            /// </summary>
            public DateTime modifiedDate;
            public PersonModel modifiedBy;
            public DateTime dateAdded;
            public PersonModel createdBy;
            public bool Loaded; // true/false - set true when the field array values are loaded
            public bool Saved; // true if edit record was saved during this page
            public bool userReadOnly; // set if this record can not be edited, for various reasons
            public bool IsDeleted; // true means the edit record has been deleted
            public bool IsInserted; // set if Workflow authoring insert
            public bool IsModified; // record has been modified since last published
            public string LockModifiedName; // member who first edited the record
            public DateTime LockModifiedDate; // Date when member modified record
            public bool SubmitLock; // set if a submit Lock, even if the current user is admin
            public string SubmittedName; // member who submitted the record
            public DateTime SubmittedDate; // Date when record was submitted
            public bool ApproveLock; // set if an approve Lock
            public string ApprovedName; // member who approved the record
            public DateTime ApprovedDate; // Date when record was approved
            /// <summary>
            /// This user can add records to this content
            /// </summary>
            public bool AllowUserAdd;
            /// <summary>
            /// This user can save the current record
            /// </summary>
            public bool AllowUserSave;
            /// <summary>
            /// This user can delete the current record
            /// </summary>
            public bool AllowUserDelete;
            /// <summary>
            /// set if an edit Lock by anyone else besides the current user
            /// </summary>
            public WorkflowController.editLockClass EditLock; 
        }
        //
        //====================================================================================================
        /// <summary>
        /// Title Bar
        /// </summary>
        /// <param name="core"></param>
        /// <param name="Title"></param>
        /// <param name="Description"></param>
        /// <returns></returns>
        public static string getTitleBar(CoreController core, string Title, string Description) {
            string result = "";
            try {
                result = Title;
                if (core.doc.debug_iUserError != "") {
                    Description += HtmlController.div( ErrorController.getUserError(core));
                }
                if (!string.IsNullOrEmpty(Description)) {
                    result += HtmlController.div(Description);
                    //result += htmlController.div(Description, "ccAdminInfoBar ccPanel3DReverse");
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            result = HtmlController.div(result, "ccAdminTitleBar");
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Get the Normal Edit Button Bar String, used on Normal Edit and others
        /// </summary>
        public static string getButtonBarForEdit(CoreController core, EditButtonBarInfoClass info) {
            //
            LogController.logTrace(core, "getButtonBarForEdit, enter, info.allowActivate [" + info.allowActivate + "]");
            //
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
                if (info.allowSend) buttonsLeft += getButtonPrimary(ButtonSend, "Return processSubmit(this)");
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
                LogController.handleError(core, ex);
            }
            //
            LogController.logTrace(core, "getButtonBarForEdit, exit");
            //
            return getButtonBar(core, buttonsLeft, buttonsRight);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a panel header with the header message reversed out of the left
        /// </summary>
        /// <param name="core"></param>
        /// <param name="HeaderMessage"></param>
        /// <param name="RightSideMessage"></param>
        /// <returns></returns>
        public static string getHeader(CoreController core, string HeaderMessage, string RightSideMessage = "") {
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
                LogController.handleError(core, ex);
            }
            return s;
        }
        //
        //====================================================================================================
        public static string getButtonsFromList(CoreController core, List<ButtonMetadata> ButtonList, bool AllowDelete, bool AllowAdd) {
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
                LogController.handleError(core, ex);
            }
            return s;
        }
        //
        //====================================================================================================
        public static string getButtonsFromList(CoreController core, string ButtonList, bool AllowDelete, bool AllowAdd, string ButtonName) {
            return getButtonsFromList(core, buttonStringToButtonList(ButtonList), AllowDelete, AllowAdd);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a bootstrap button bar
        /// </summary>
        /// <param name="LeftButtons"></param>
        /// <param name="RightButtons"></param>
        /// <returns></returns>
        public static string getButtonBar(CoreController core, string LeftButtons, string RightButtons) {
            if (string.IsNullOrWhiteSpace(LeftButtons + RightButtons)) {
                return "";
            } else if (string.IsNullOrWhiteSpace(RightButtons)) {
                return "<div class=\"border bg-white p-2\">" + LeftButtons + "</div>";
            } else {
                return "<div class=\"border bg-white p-2\">" + LeftButtons + "<div class=\"float-right\">" + RightButtons + "</div></div>";
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
            result = getButtonBar(core, LeftButtons, RightButtons);
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
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        //====================================================================================================
        public static string getBody(CoreController core, string Caption, string ButtonListLeft, string ButtonListRight, bool AllowAdd, bool AllowDelete, string Description, string ContentSummary, int ContentPadding, string Content) {
            string result = "";
            try {
                string ButtonBar = null;
                string LeftButtons = "";
                string RightButtons = "";
                string CellContentSummary = "";
                //
                // Build ButtonBar
                //
                if (!string.IsNullOrEmpty(ButtonListLeft.Trim(' '))) {
                    LeftButtons = getButtonsFromList(core, ButtonListLeft, AllowDelete, AllowAdd, "Button");
                }
                if (!string.IsNullOrEmpty(ButtonListRight.Trim(' '))) {
                    RightButtons = getButtonsFromList(core, ButtonListRight, AllowDelete, AllowAdd, "Button");
                }
                ButtonBar = getButtonBar(core, LeftButtons, RightButtons);
                if (!string.IsNullOrEmpty(ContentSummary)) {
                    CellContentSummary = ""
                        + "\r<div class=\"ccPanelBackground\" style=\"padding:10px;\">"
                        + core.html.getPanel(ContentSummary, "ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5) 
                        + "\r</div>";
                }
                result += ""
                    + ButtonBar 
                    + getTitleBar(core, Caption, Description)
                    + CellContentSummary 
                    + "<div style=\"padding:" + ContentPadding + "px;\">" + Content + "\r</div>"
                    + ButtonBar;
                result = HtmlController.formMultipart(core, result, core.doc.refreshQueryString,"","ccForm");
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
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
                LogController.handleError(core, ex);
            }
            return result.ToString();
        }
        //
        //====================================================================================================
        // Edit Table
        public static string editTable( string innerHtml ) {
            return ""
                + "<table border=0 cellpadding=3 cellspacing=0 width=\"100%\">" 
                    + innerHtml
                    + "<tr>"
                        + "<td width=20%><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=\"100%\" height=1 ></td>"
                        + "<td width=80%><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=\"100%\" height=1 ></td>"
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
                if (!string.IsNullOrEmpty(Align)) { Style +="TEXT-ALIGN:" + Align + ";"; }
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
                        Copy = "<a href=\"?" + QS + "\" title=\"Sort A-Z\" class=\"ccAdminListCaption\">" + Copy + "<img src=\"/ContensiveBase/images/arrowup.gif\" width=8 height=8 border=0></a>";
                        break;
                    }
                    case SortingStateEnum.SortableSetAZ: {
                        string QS = GenericController.modifyQueryString(RefreshQueryString, "ColSort", ((int)SortingStateEnum.SortableSetza).ToString(), true);
                        QS = GenericController.modifyQueryString(QS, "ColPtr", ColumnPtr.ToString(), true);
                        Copy = "<a href=\"?" + QS + "\" title=\"Sort Z-A\" class=\"ccAdminListCaption\">" + Copy + "<img src=\"/ContensiveBase/images/arrowdown.gif\" width=8 height=8 border=0></a>";
                        break;
                    }
                }
                //
                if (!string.IsNullOrEmpty(Width)) {
                    Style = Style + "width:" + Width + ";";
                    //WidthTest = GenericController.encodeInteger(Width.ToLower().Replace("px", ""));
                    //if (WidthTest != 0) {
                    //    Style = Style + "width:" + WidthTest + "px;";
                    //    Copy += "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"" + WidthTest + "\" height=1 border=0>";
                    //    //Copy = Copy & "<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""" & WidthTest & """ height=1 border=0>"
                    //} else {
                    //}
                }
                result = "\r\n<td style=\"" + Style + "\" class=\"" + ClassStyle + "\">" + Copy + "</td>";
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                LogController.handleError(core, ex);
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
                //If IsArray(Cells) Then
                ColumnCount = Cells.GetUpperBound(1);
                //End If
                RQS = core.doc.refreshQueryString;
                //
                SortColPtr = getReportSortColumnPtr(core, DefaultSortColumnPtr);
                SortColType = getReportSortType(core);
                //
                // ----- Start the table
                //
                Content.Add(HtmlController.tableStart(3, 1, 0));
                //
                // ----- Header
                //
                Content.Add("\r\n<tr>");
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
                    Content.Add(getReport_Cell(core, (RowBAse + RowPointer).ToString(), "right", 1, RowPointer));
                    Content.Add(getReport_Cell(core, "-- End --", "left", ColumnCount, 0));
                    Content.Add("\r\n</tr>");
                } else {
                    RowBAse = (ReportPageSize * (ReportPageNumber - 1)) + 1;
                    for (RowPointer = 0; RowPointer < RowCount; RowPointer++) {
                        Content.Add("\r\n<tr>");
                        Content.Add(getReport_Cell(core, (RowBAse + RowPointer).ToString(), "right", 1, RowPointer));
                        for (ColumnPtr = 0; ColumnPtr < ColumnCount; ColumnPtr++) {
                            Content.Add(getReport_Cell(core, Cells[RowPointer, ColumnPtr], ColAlign[ColumnPtr], 1, RowPointer));
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
                LogController.handleError(core, ex);
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
        public static string getButtonPrimary(string buttonValue, string onclick, bool disabled, string htmlId, string htmlName) => HtmlController.getHtmlInputSubmit(buttonValue, htmlName, htmlId, onclick, disabled, "btn btn-primary mr-1 btn-sm");
        //
        public static string getButtonPrimary(string buttonValue, string onclick, bool disabled, string htmlId) => HtmlController.getHtmlInputSubmit(buttonValue, "button", htmlId, onclick, disabled, "btn btn-primary mr-1 btn-sm");
        //
        public static string getButtonPrimary(string buttonValue, string onclick, bool disabled) => HtmlController.getHtmlInputSubmit(buttonValue, "button", "", onclick, disabled, "btn btn-primary mr-1 btn-sm");
        //
        public static string getButtonPrimary(string buttonValue, string onclick) => HtmlController.getHtmlInputSubmit(buttonValue, "button", "", onclick, false, "btn btn-primary mr-1 btn-sm");
        //
        public static string getButtonPrimary(string buttonValue) => HtmlController.getHtmlInputSubmit(buttonValue, "button", "", "", false, "btn btn-primary mr-1 btn-sm");
        //
        // ====================================================================================================
        //
        public static string getButtonDanger(string buttonValue, string onclick, bool disabled, string htmlId) => HtmlController.getHtmlInputSubmit(buttonValue, "button", htmlId, onclick, disabled, "btn btn-danger mr-1 btn-sm");
        //
        public static string getButtonDanger(string buttonValue, string onclick, bool disabled) => HtmlController.getHtmlInputSubmit(buttonValue, "button", "", onclick, disabled, "btn btn-danger mr-1 btn-sm");
        //
        public static string getButtonDanger(string buttonValue, string onclick) => HtmlController.getHtmlInputSubmit(buttonValue, "button", "", onclick, false, "btn btn-danger mr-1 btn-sm");
        //
        public static string getButtonDanger(string buttonValue) => HtmlController.getHtmlInputSubmit(buttonValue, "button", "", "", false, "btn btn-danger mr-1 btn-sm");
        //
        // ====================================================================================================
        //
        public static string getButtonPrimaryAnchor(string buttonCaption, string href) =>  HtmlController.a(buttonCaption, href, "btn btn-primary mr-1 btn-sm");
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
        public static string getEditForm_TitleBarDetails(CoreController core, RecordEditHeaderInfoClass headerInfo, EditRecordClass editRecord) {
            string result = "";
            bool alt = true;
            if (alt) {
                if (editRecord.id == 0) {
                    result += HtmlController.div( "New record" , "col-sm-12");
                } else {
                    result += HtmlController.div(HtmlController.strong( editRecord.contentControlId_Name + ":&nbsp;#" ) + headerInfo.recordId + ", " + editRecord.nameLc , "col-sm-4");
                    result += HtmlController.div(HtmlController.strong("Created:&nbsp;" ) + getEditForm_TitleBarDetails_EditorString(editRecord.dateAdded, editRecord.createdBy, "unknown"), "col-sm-4");
                    result += HtmlController.div(HtmlController.strong("Modified:&nbsp;") + getEditForm_TitleBarDetails_EditorString( editRecord.modifiedDate, editRecord.modifiedBy, "not modified" ), "col-sm-4");
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
                    if ( editRecord.createdBy.id != 0) {
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
                        if ( editRecord.modifiedBy.id != 0) {
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
                        PersonModel personLock = PersonModel.create(core, headerInfo.recordLockById);
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
        public static string getDefaultEditor_Bool(CoreController core, string htmlName, bool htmlValue, bool readOnly = false, string htmlId = "") {
            string result = HtmlController.div(HtmlController.checkbox(htmlName, htmlValue, htmlId, false, "", readOnly), "checkbox");
            if (readOnly)  result += HtmlController.inputHidden(htmlName, htmlValue);
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
        public static string getDefaultEditor_Text(CoreController core, string fieldName, string fieldValue, bool readOnly = false, string htmlId = "") {
            if ((fieldValue.IndexOf("\n") == -1) && (fieldValue.Length < 80)) {
                //
                // text field shorter then 40 characters without a CR
                return HtmlController.inputText( core,fieldName, fieldValue, 1, -1, htmlId, false, readOnly, "text form-control", 255);
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
        public static string getDefaultEditor_Html( CoreController core, string fieldName, string fieldValue, string editorAddonListJSON, string styleList, string styleOptionList, bool readONly = false, string htmlId = "" ) {
            string result = "";
            if (readONly) {
                result += HtmlController.inputHidden(fieldName, fieldValue);
            } else if (string.IsNullOrEmpty(fieldValue)) {
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
            return HtmlController.inputText( core,fieldName, fieldValue, -1, -1, htmlId, true, readOnly, "password form-control", 255);
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
        public static string getDefaultEditor_LookupContent( CoreController core, string fieldName, int fieldValue, int lookupContentId, ref bool IsEmptyList, bool readOnly = false, string htmlId = "", string WhyReadOnlyMsg = "", bool fieldRequired = false, string sqlFilter = "") {
            string result = "";
            ContentMetadataModel lookupContentMetacontent = ContentMetadataModel.create( core, lookupContentId );
            if ( lookupContentMetacontent == null) {
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
                            result += ("No Name");
                        } else {
                            result += (HtmlController.encodeHtml(csData.getText("Name")));
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
        public static string getDefaultEditor_LookupList(CoreController core, string htmlName, int defaultLookupIndexBaseOne, string[] lookupArray, bool readOnly = false, string htmlId = "", string WhyReadOnlyMsg = "", bool fieldRequired = false) {
            string result = "";
            if (readOnly) {
                //
                // ----- Lookup ReadOnly
                result += (HtmlController.inputHidden(htmlName, defaultLookupIndexBaseOne.ToString()));
                if (defaultLookupIndexBaseOne < 1) {
                    result += "None";
                } else if (defaultLookupIndexBaseOne > (lookupArray.GetUpperBound(0) + 1)) {
                    result += "None";
                } else {
                    result += lookupArray[defaultLookupIndexBaseOne - 1];
                }
                result += WhyReadOnlyMsg;
            } else {
                if (!fieldRequired) {
                    result += HtmlController.selectFromList( core, htmlName, defaultLookupIndexBaseOne, lookupArray, "Select One", "", "select form-control");
                } else {
                    result += HtmlController.selectFromList( core, htmlName, defaultLookupIndexBaseOne, lookupArray, "", "", "select form-control");
                }

            }
            return result;
        }
        public static string getDefaultEditor_LookupList(CoreController core, string htmlName, string defaultValue, List<NameValueClass> lookupList, bool readOnly = false, string htmlId = "", string WhyReadOnlyMsg = "", bool fieldRequired = false) {
            string result = "";
            if (readOnly) {
                //
                // ----- Lookup ReadOnly
                result += (HtmlController.inputHidden(htmlName, GenericController.encodeText(defaultValue)));
                NameValueClass nameValue = lookupList.Find(x => x.name.ToLowerInvariant() == htmlName.ToLowerInvariant());
                if (nameValue == null) {
                    result += "none";
                } else {
                    result += nameValue.value;
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
        public static string getDefaultEditor_DateTime( CoreController core, string fieldName, DateTime FieldValueDate, bool readOnly = false, string htmlId = "", bool fieldRequired = false, string WhyReadOnlyMsg = "") {
            string inputDate = "";
            if (FieldValueDate.CompareTo(new DateTime(1900,1,1))>0) {
                if (FieldValueDate.Hour.Equals(0) && FieldValueDate.Minute.Equals(0) && FieldValueDate.Second.Equals(0)) {
                    inputDate = FieldValueDate.ToShortDateString();
                } else {
                    inputDate = FieldValueDate.ToString();
                }
            }
            return getDefaultEditor_Text(core, fieldName, inputDate, readOnly, htmlId);
            //string result = "";
            //string fieldValue_text = "";
            //if (FieldValueDate == DateTime.MinValue) {
            //    fieldValue_text = "";
            //} else {
            //    fieldValue_text = encodeText(FieldValueDate);
            //}
            //if (readOnly) {
            //    //
            //    // -- readOnly
            //    result += HtmlController.inputHidden(fieldName, fieldValue_text);
            //    result += HtmlController.inputText( core,fieldName, fieldValue_text, -1, -1, "", false, true, "date form-control");
            //    result += WhyReadOnlyMsg;
            //} else {
            //    //
            //    // -- editable
            //    result += HtmlController.inputDateTime( core,fieldName, encodeDate(fieldValue_text),"",htmlId, "date form-control", readOnly, fieldRequired);
            //}
            //return result;
        }
        //
        // ====================================================================================================
        //
        public static string getDefaultEditor_memberSelect(CoreController core, string htmlName, int selectedRecordId, int groupId, string groupName, bool readOnly = false, string htmlId = "", bool fieldRequired = false, string WhyReadOnlyMsg = "") {
            string EditorString = "";
            if ( (groupId>0)&(string.IsNullOrWhiteSpace(groupName))) {
                var group = GroupModel.create(core, groupId);
                if ( group != null) {
                    groupName = "Group " + group.id.ToString();
                    group.save(core);
                }
            }
            if (readOnly) {
                //
                // -- readOnly
                EditorString += HtmlController.inputHidden(htmlName, selectedRecordId.ToString() );
                if (selectedRecordId == 0) {
                    EditorString += "None";
                } else {
                    var selectedUser = PersonModel.create(core, selectedRecordId);
                    if ( selectedUser==null) {
                        EditorString += "Deleted";
                    } else {
                        EditorString += (string.IsNullOrWhiteSpace(selectedUser.name)) ? "No Name" : HtmlController.encodeHtml(selectedUser.name);
                        EditorString += ("&nbsp;[<a TabIndex=-1 href=\"?af=4&cid=" + selectedUser.contentControlID.ToString() + "&id=" + selectedRecordId.ToString() + "\" target=\"_blank\">View details in new window</a>]");
                    }
                }
                EditorString += WhyReadOnlyMsg;
            } else {
                //
                // -- editable
                EditorString += core.html.selectUserFromGroup(htmlName, selectedRecordId, groupId, "", (fieldRequired) ? "" : "None", htmlId, "select form-control");
                if (selectedRecordId != 0) {
                    var selectedUser = PersonModel.create(core, selectedRecordId);
                    if (selectedUser == null) {
                        EditorString += "Deleted";
                    } else {
                        string recordName = (string.IsNullOrWhiteSpace(selectedUser.name)) ? "No Name" : HtmlController.encodeHtml(selectedUser.name);
                        EditorString += "&nbsp;[Edit <a TabIndex=-1 href=\"?af=4&cid=" + selectedUser.contentControlID.ToString() + "&id=" + selectedRecordId.ToString() + "\">" + HtmlController.encodeHtml( recordName ) + "</a>]";
                    }
                }
                EditorString += ("&nbsp;[Select from members of <a TabIndex=-1 href=\"?cid=" + ContentMetadataModel.getContentId(core, "groups") + "\">" + groupName + "</a>]");
            }
            return EditorString;
        }
        //
        // ====================================================================================================
        //
        public static string getDefaultEditor_manyToMany(CoreController core, ContentFieldMetadataModel field, string htmlName, string currentValueCommaList, int editRecordId, bool readOnly = false, string WhyReadOnlyMsg = "" ) {
            string result = "";
            //
            string MTMContent0 =   MetadataController.getContentNameByID(core, field.contentId);
            string MTMContent1 = MetadataController.getContentNameByID(core, field.manyToManyContentID);
            string MTMRuleContent = MetadataController.getContentNameByID(core, field.manyToManyRuleContentID);
            string MTMRuleField0 = field.manyToManyRulePrimaryField;
            string MTMRuleField1 = field.manyToManyRuleSecondaryField;
            result += core.html.getCheckList(htmlName, MTMContent0, editRecordId, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, "", "", false, false, currentValueCommaList);
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
                    result = getDefaultEditor_Text(core, SitePropertyName, selector);
                }

                //FastString = null;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
            return HtmlController.div(HtmlController.label(Caption, fieldHtmlId) + HtmlController.div(EditorString, "ml-5") + HtmlController.div( HtmlController.small( editorHelpRow, "form-text text-muted"), "ml-5"), "p-2 ccEditRow");
        }
        //
        // ====================================================================================================
        //
        public static string getEditRowLegacy(CoreController core, string HTMLFieldString, string Caption, string HelpMessage = "", bool FieldRequired = false, bool AllowActiveEdit = false, string ignore0 = "") {
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
            //    tempGetEditRow = "<tr><td class=\"ccEditCaptionCon\"><nobr>" + Copy + "<img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=1 height=15 >";
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
        /// get linked icon for remove (red x)
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public static string getIconDeleteLink(string link) { return HtmlController.a(iconDelete_Red, link); }
        public static string getIconArrowRightLink(string link) { return HtmlController.a(iconArrowRight, link); }
        public static string getIconArrowLeftLink(string link) { return HtmlController.a(iconArrowLeft, link); }
        public static string getIconPlusLink(string link, string caption = "") { return HtmlController.a(iconAdd_Green + caption, link); }
        public static string getIconExpandLink(string link) { return HtmlController.a(iconExpand, link); }
        public static string getIconContractLink(string link) { return HtmlController.a(iconContract, link); }
        //
        //
        // ====================================================================================================
        /// <summary>
        /// get linked icon for refresh (green refresh)
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public static  string getIconRefreshLink(string link) {return HtmlController.a( iconRefresh, link );}
        //
        // ====================================================================================================
        /// <summary>
        /// get linked icon for edit (green edit)
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public static string getIconEditLink(string link) { return HtmlController.a(iconEdit_Green, link); }
        public static string getIconEditLink(string link, string htmlClass) { return HtmlController.a(iconEdit_Green, link, htmlClass); }
        /// <summary>
        /// get a link to edit a record in the admin site
        /// </summary>
        /// <param name="core"></param>
        /// <param name="cdef"></param>
        /// <returns></returns>
        public static string getIconEditAdminLink(CoreController core, ContentMetadataModel cdef) { return getIconEditLink("/" + core.appConfig.adminRoute + "?cid=" + cdef.id, "ccRecordEditLink");}
        public static string getIconEditAdminLink(CoreController core, ContentMetadataModel cdef, int recordId) {return getIconEditLink("/" + core.appConfig.adminRoute + "?af=4&aa=2&ad=1&cid=" + cdef.id + "&id=" + recordId, "ccRecordEditLink");}
        //
        //====================================================================================================
        //
        public static string getToolFormTitle(string Title, string Description) {
            return HtmlController.h2(Title) + HtmlController.p(Description);
        }
        //
        //====================================================================================================
        //
        public static string getToolForm(CoreController core, string innerHtml, string buttonList) {
            string buttonHtml = (string.IsNullOrWhiteSpace(buttonList)) ? "" : core.html.getPanelButtons(buttonList, "Button");
            string result = ""
                + buttonHtml 
                + HtmlController.div(innerHtml, "p-4 bg-light") 
                + buttonHtml;
            return HtmlController.form(core, result);
        }
        //
        //====================================================================================================
        //
        public static string getToolFormRow( CoreController core, string asdf) {
            return HtmlController.div(asdf, "p-1"); 
        }
        //
        //====================================================================================================
        //
        public static string getToolFormInputRow( CoreController core, string label, string input ) {
            return getToolFormRow(core, HtmlController.label(label) + "<br>" + input);

            //"<div class=\"p-1\">Page Number:<br>" + htmlController.inputText(core, "PageNumber", PageNumber.ToString()) + "</div>";
        }
        //
        //====================================================================================================
        /// <summary>
        /// create UI edit record link
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <param name="AllowCut"></param>
        /// <returns></returns>
        public static string getRecordEditLink(CoreController core, string ContentName, int RecordID, bool AllowCut = false) {
            return getRecordEditLink(core, ContentName, RecordID, AllowCut, "", core.session.isEditing(ContentName));
        }
        //
        //===================================================================================================
        //
        public static  string getEditWrapper(CoreController core, string caption, string content) {
            string result = content;
            if (core.session.isEditingAnything()) {
                result = HtmlController.div(result, "ccEditWrapperContent");
                if (!string.IsNullOrEmpty(caption)) {
                    result = HtmlController.div(caption, "ccEditWrapperCaption") + result;
                }
                result = HtmlController.div(result, "ccEditWrapper", "editWrapper" + core.doc.editWrapperCnt++);
                //result = "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccEditWrapper\">";
                ////result = html_GetLegacySiteStyles() + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccEditWrapper\">";
                //if (!string.IsNullOrEmpty(Caption)) {
                //    result += ""
                //            + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccEditWrapperCaption\">"
                //            + genericController.encodeText(Caption)
                //            + "</td></tr></table>";
                //}
                //result += ""
                //        + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccEditWrapperContent\" id=\"editWrapper" + core.doc.editWrapperCnt + "\">"
                //        + genericController.encodeText(Content) + "</td></tr></table>"
                //        + "</td></tr></table>";
                //core.doc.editWrapperCnt = core.doc.editWrapperCnt + 1;
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public static string getRecordEditLink(CoreController core, string contentName, int recordID, bool allowCut, string RecordName, bool IsEditing) {
            string result = "";
            try {
                string ContentCaption = HtmlController.encodeHtml(contentName);
                ContentCaption += " record";
                if (!string.IsNullOrEmpty(RecordName)) ContentCaption += ", named '" + RecordName + "'";
                if (string.IsNullOrEmpty(contentName)) {
                    throw (new GenericException("ContentName [" + contentName + "] is invalid"));
                } else {
                    if (recordID < 1) {
                        throw (new GenericException("RecordID [" + recordID + "] is invalid"));
                    } else {
                        if (IsEditing) {
                            var cdef = ContentMetadataModel.createByUniqueName(core, contentName);
                            if ( cdef==null) {
                                throw new GenericException("getRecordEditLink called with contentName [" + contentName + "], but no content found with this name.");
                            } else {
                                result += AdminUIController.getIconEditAdminLink(core, cdef, recordID);
                                if (allowCut) {
                                    int ContentID = 0;
                                    string WorkingLink = GenericController.modifyLinkQuery(core.webServer.requestPage + "?" + core.doc.refreshQueryString, RequestNameCut, GenericController.encodeText(ContentID) + "." + GenericController.encodeText(recordID), true);
                                    result += "<a class=\"ccRecordCutLink\" TabIndex=\"-1\" href=\"" + HtmlController.encodeHtml(WorkingLink) + "\">" + iconContentCut.Replace("content cut", "Cut this " + ContentCaption + " to clipboard") + "</a>";
                                }
                                result = "<span class=\"ccRecordLinkCon\" style=\"white-space:nowrap;\">" + result + "</span>";
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }



        //
        //====================================================================================================
        //
        public static List<string> getRecordAddLink(CoreController core, string ContentName, string PresetNameValueList) => getRecordAddLink(core, ContentName, PresetNameValueList, false, core.session.isEditing(ContentName));
        //
        public static List<string> getRecordAddLink(CoreController core, string ContentName, string PresetNameValueList, bool AllowPaste) => getRecordAddLink(core, ContentName, PresetNameValueList, AllowPaste, core.session.isEditing(ContentName));
        //
        public static List<string> getRecordAddLink(CoreController core, string contentName, string presetNameValueList, bool allowPaste, bool IsEditing) {
            List<string> result = new List<string>();
            try {
                if (IsEditing) {
                    if (!string.IsNullOrEmpty(contentName)) {
                        //
                        // -- convert older QS format to command delimited format
                        presetNameValueList = presetNameValueList.Replace("&", ",");
                        var content = ContentModel.createByUniqueName(core, contentName);
                        result.AddRange(getRecordAddLink_GetChildContentLinks(core, content, presetNameValueList, new List<int>()));
                        //
                        // Add in the paste entry, if needed
                        //
                        if (allowPaste) {
                            string ClipBoard = core.visitProperty.getText("Clipboard", "");
                            if (!string.IsNullOrEmpty(ClipBoard)) {
                                int Position = GenericController.vbInstr(1, ClipBoard, ".");
                                if (Position != 0) {
                                    string[] ClipBoardArray = ClipBoard.Split('.');
                                    if (ClipBoardArray.GetUpperBound(0) > 0) {
                                        int ClipboardContentID = GenericController.encodeInteger(ClipBoardArray[0]);
                                        int ClipChildRecordID = GenericController.encodeInteger(ClipBoardArray[1]);
                                        if (content.isParentOf<ContentModel>(core, ClipboardContentID)) {
                                            int ParentID = 0;
                                            if (GenericController.vbInstr(1, presetNameValueList, "PARENTID=", 1) != 0) {
                                                //
                                                // must test for main_IsChildRecord
                                                //
                                                string BufferString = presetNameValueList;
                                                BufferString = BufferString.Replace("(", "");
                                                BufferString = BufferString.Replace(")", "");
                                                BufferString = BufferString.Replace(",", "&");
                                                ParentID = encodeInteger(GenericController.main_GetNameValue_Internal(core, BufferString, "Parentid"));
                                            }
                                            if ((ParentID != 0) && (! DbModel.isChildOf<PageContentModel>(core,ParentID,0,new List<int>()))) {
                                                //
                                                // Can not paste as child of itself
                                                //
                                                string PasteLink = core.webServer.requestPage + "?" + core.doc.refreshQueryString;
                                                PasteLink = GenericController.modifyLinkQuery(PasteLink, RequestNamePaste, "1", true);
                                                PasteLink = GenericController.modifyLinkQuery(PasteLink, RequestNamePasteParentContentID, content.id.ToString(), true);
                                                PasteLink = GenericController.modifyLinkQuery(PasteLink, RequestNamePasteParentRecordID, ParentID.ToString(), true);
                                                PasteLink = GenericController.modifyLinkQuery(PasteLink, RequestNamePasteFieldList, presetNameValueList, true);
                                                result.Add(HtmlController.div(HtmlController.a(iconContentPaste_Green, PasteLink, "ccRecordPasteLink", "", "-1"), "ccRecordLinkCon"));
                                                //                                result = "<span class=\"ccRecordLinkCon\" style=\"white-space:nowrap;\">" + result + "</span>";

                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //if (!string.IsNullOrEmpty(result)) { result = HtmlController.div(result, "d-inline"); }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
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
                    if (core.session.isAuthenticatedAdmin(core)) {
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
                                    + " LEFT JOIN ccMembers ON ccMemberRules.MemberID=ccMembers.ID"
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
                        result.Add(HtmlController.div(HtmlController.a(iconAdd_Green, Link, "ccRecordAddLink", "", "-1") + HtmlController.a(content.name, Link, "ccRecordAddLink", "", "-1"), "ccRecordLinkCon"));
                        //core.menuFlyout.menu_AddEntry(MenuName + ":" + content.name, ParentMenuName, "", "", Link, ButtonCaption, "", "", true);
                        //
                        // Create child submenu if Child Entries found
                        var childList = Processor.Models.Db.ContentModel.createList(core, "ParentID=" + content.id);
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


    }
}
