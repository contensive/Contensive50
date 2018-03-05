
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
        //
        private enum SortingStateEnum {
            NotSortable = 0,
            SortableSetAZ = 1,
            SortableSetza = 2,
            SortableNotSet = 3
        }
        // ====================================================================================================
        /// <summary>
        /// Title Bar
        /// </summary>
        /// <param name="core"></param>
        /// <param name="Title"></param>
        /// <param name="Description"></param>
        /// <returns></returns>
        public static string GetTitleBar(coreController core, string Title, string Description) {
            string tempGetTitleBar = null;
            try {
                //
                string Copy = null;
                //
                tempGetTitleBar = "<div class=\"ccAdminTitleBar\">" + Title;
                Copy = Description;
                //
                // Add Errors
                //
                if (core.doc.debug_iUserError != "") {
                    Copy += "<div>" + errorController.getUserError(core) + "</div>";
                }
                //
                if (!string.IsNullOrEmpty(Copy)) {
                    tempGetTitleBar = tempGetTitleBar + "<div>&nbsp;</div><div class=\"ccAdminInfoBar ccPanel3DReverse\">" + Copy + "</div>";
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetTitleBar + "</div>";
        }
        // ====================================================================================================
        /// <summary>
        /// Get the Normal Edit Button Bar String, used on Normal Edit and others
        /// </summary>
        public static string GetEditButtonBar2(coreController core, int MenuDepth, bool AllowDelete, bool AllowCancel, bool allowSave, bool AllowSpellCheck, bool ignorefalse, bool ignorefalse2, bool ignorefalse3, bool ignorefalse4, bool AllowAdd, bool ignore_AllowReloadCDef, bool HasChildRecords, bool IsPageContent, bool AllowMarkReviewed, bool AllowRefresh, bool AllowCreateDuplicate) {
            string result = "";
            try {
                string buttonsLeft = "";
                //
                string JSOnClick = null;
                //
                buttonsLeft = "";
                //
                if (AllowCancel) {
                    if (MenuDepth == 1) {
                        //
                        // Close if this is the root depth of a popup window
                        //
                        buttonsLeft += getButtonPrimary(ButtonClose, "window.close();");
                        //buttonsLeft = buttonsLeft + "<input TYPE=\"SUBMIT\" NAME=\"BUTTON\" VALUE=\"" + ButtonClose + "\" OnClick=\"window.close();\" class=\"btn btn-primary mr-1 btn-sm btn-sm\">";
                    } else {
                        //
                        // Cancel
                        //
                        buttonsLeft += getButtonPrimary(ButtonCancel, "return processSubmit(this);");
                        //buttonsLeft = buttonsLeft + "<input TYPE=\"SUBMIT\" NAME=\"BUTTON\" VALUE=\"" + ButtonCancel + "\" onClick=\"return processSubmit(this);\" class=\"btn btn-primary mr-1 btn-sm\">";
                    }
                }
                if (allowSave) {
                    //
                    // Save
                    //
                    buttonsLeft += getButtonPrimary(ButtonSave, "return processSubmit(this);");
                    //buttonsLeft = buttonsLeft + "<input TYPE=\"SUBMIT\" NAME=\"BUTTON\" VALUE=\"" + ButtonSave + "\" onClick=\"return processSubmit(this);\" class=\"btn btn-primary mr-1 btn-sm\">";
                    //
                    // OK
                    //
                    buttonsLeft += getButtonPrimary(ButtonOK, "return processSubmit(this);");
                    //buttonsLeft = buttonsLeft + "<input TYPE=\"SUBMIT\" NAME=\"BUTTON\" VALUE=\"" + ButtonOK + "\" onClick=\"return processSubmit(this);\" class=\"btn btn-primary mr-1 btn-sm\">";
                    if (AllowAdd) {
                        //
                        // OK
                        //
                        buttonsLeft += getButtonPrimary(ButtonSaveAddNew, "return processSubmit(this);");
                        //buttonsLeft = buttonsLeft + "<input TYPE=\"SUBMIT\" NAME=\"BUTTON\" VALUE=\"" + ButtonSaveAddNew + "\" onClick=\"return processSubmit(this);\" class=\"btn btn-primary mr-1 btn-sm\">";
                    }
                }
                if (AllowDelete) {
                    //
                    // Delete
                    //
                    if (IsPageContent) {
                        JSOnClick = "if(!DeletePageCheck())return false;";
                    } else if (HasChildRecords) {
                        JSOnClick = "if(!DeleteCheckWithChildren())return false;";
                    } else {
                        JSOnClick = "if(!DeleteCheck())return false;";
                    }
                    buttonsLeft += getButtonPrimary(ButtonDelete, JSOnClick );
                    //buttonsLeft = buttonsLeft + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonDelete + "\" onClick=\"" + JSOnClick + "\" class=\"btn btn-primary mr-1 btn-sm\">";
                } else {
                    buttonsLeft += getButtonPrimary(ButtonDelete, "", true );
                }
                if (ignore_AllowReloadCDef) {
                    //
                    // Reload Content Definitions
                    //
                    buttonsLeft = buttonsLeft + getButtonPrimary(ButtonSaveandInvalidateCache);
                }
                if (AllowMarkReviewed) {
                    //
                    // Reload Content Definitions
                    //
                    buttonsLeft = buttonsLeft + getButtonPrimary(ButtonMarkReviewed);
                }
                if (AllowRefresh) {
                    //
                    // just like a save, but don't save jsut redraw
                    //
                    buttonsLeft = buttonsLeft + getButtonPrimary(ButtonRefresh);
                }
                if (AllowCreateDuplicate) {
                    //
                    // just like a save, but don't save jsut redraw
                    //
                    buttonsLeft = buttonsLeft + getButtonPrimary(ButtonCreateDuplicate, "return processSubmit(this)");
                }
                //
                result = GetButtonBar( core, buttonsLeft, "");
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
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
                core.handleException(ex);
            }
            return s;
        }
        // ====================================================================================================
        //
        public static string GetButtonsFromList(coreController core, string ButtonList, bool AllowDelete, bool AllowAdd, string ButtonName) {
            string s = "";
            try {
                string[] Buttons = null;
                int Ptr = 0;
                if (!string.IsNullOrEmpty(ButtonList.Trim(' '))) {
                    Buttons = ButtonList.Split(',');
                    for (Ptr = 0; Ptr <= Buttons.GetUpperBound(0); Ptr++) {
                        if (Buttons[Ptr].Trim(' ') == encodeText(ButtonDelete).Trim(' ')) {
                            if (AllowDelete) {
                                s += getButtonDanger(Buttons[Ptr], "if(!DeleteCheck()) return false;");
                                //s = s + "<input TYPE=SUBMIT NAME=\"" + ButtonName + "\" VALUE=\"" + Buttons[Ptr] + "\" onClick=\"if(!DeleteCheck())return false;\" class=\"btn btn-danger mr-1\">";
                            } else {
                                s += getButtonDanger(Buttons[Ptr], "if(!DeleteCheck()) return false;",true);
                                //s = s + "<input TYPE=SUBMIT NAME=\"" + ButtonName + "\" DISABLED VALUE=\"" + Buttons[Ptr] + "\" class=\"btn btn-primary mr-1 btn-sm\">";
                            }
                        }
                        else if (Buttons[Ptr].Trim(' ') == encodeText(ButtonClose).Trim(' ')) {
                            s += getButtonPrimary(Buttons[Ptr], "window.close();");
                            //s = s + htmlController.button(Buttons[Ptr], "", "", "window.close();");
                        }
                        else if (Buttons[Ptr].Trim(' ') == encodeText(ButtonAdd).Trim(' ')) {
                            if (AllowAdd) {
                                s += getButtonPrimary(Buttons[Ptr], "return processSubmit(this);");
                                //s = s + "<input type=submit name=\"" + ButtonName + "\" value=\"" + Buttons[Ptr] + "\" onClick=\"return processSubmit(this);\" class=\"btn btn-primary mr-1 btn-sm\">";
                            } else {
                                s += getButtonPrimary(Buttons[Ptr], "return processSubmit(this);",true );
                                //s = s + "<input TYPE=SUBMIT NAME=\"" + ButtonName + "\" DISABLED VALUE=\"" + Buttons[Ptr] + "\" onClick=\"return processSubmit(this);\" class=\"btn btn-primary mr-1 btn-sm\">";
                            }
                        } else if (string.IsNullOrEmpty(Buttons[Ptr].Trim(' '))) {
                            //
                        } else {
                            s += getButtonPrimary(Buttons[Ptr]);
                            //s = s + htmlController.button(Buttons[Ptr], ButtonName);
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return s;
        }
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
                //
                return "<div class=\"border bg-white p-2\">" + LeftButtons + "<div class=\"float-right\">" + RightButtons + "</div></div>";
                //return "<div class=\"container-fluid\"><div class=\"border bg-white p-2 text-right\"><div class=\"row\"><div class=\"col text-left\">" + LeftButtons + "</div><div class=\"col text-right\">" + RightButtons + "</div></div></div></div>";
            }
        }
        // ====================================================================================================
        //
        public static string GetButtonBarForIndex(coreController core, string LeftButtons, string RightButtons, int PageNumber, int RecordsPerPage, int PageCount) {
            string tempGetButtonBarForIndex = null;
            try {
                //
                int Ptr = 0;
                string Nav = "";
                int NavStart = 0;
                int NavEnd = 0;
                //
                tempGetButtonBarForIndex = GetButtonBar(core, LeftButtons, RightButtons);
                NavStart = PageNumber - 9;
                if (NavStart < 1) {
                    NavStart = 1;
                }
                NavEnd = NavStart + 20;
                if (NavEnd > PageCount) {
                    NavEnd = PageCount;
                    NavStart = NavEnd - 20;
                    if (NavStart < 1) {
                        NavStart = 1;
                    }
                }
                if (NavStart > 1) {
                    Nav = Nav + "<li onclick=\"bbj(this);\">1</li><li class=\"delim\">&#171;</li>";
                }
                for (Ptr = NavStart; Ptr <= NavEnd; Ptr++) {
                    Nav = Nav + "<li onclick=\"bbj(this);\">" + Ptr + "</li>";
                }
                if (NavEnd < PageCount) {
                    Nav = Nav + "<li class=\"delim\">&#187;</li><li onclick=\"bbj(this);\">" + PageCount + "</li>";
                }
                Nav = genericController.vbReplace(Nav, ">" + PageNumber + "<", " class=\"hit\">" + PageNumber + "<");
                tempGetButtonBarForIndex = tempGetButtonBarForIndex + "\r<script language=\"javascript\">function bbj(p){document.getElementsByName('indexGoToPage')[0].value=p.innerHTML;document.adminForm.submit();}</script>"
                    + "\r<div class=\"ccJumpCon\">"
                    + cr2 + "<ul><li class=\"caption\">Page</li>"
                    + cr3 + Nav + cr2 + "</ul>"
                    + "\r</div>";
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetButtonBarForIndex;
        }
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
                result = result + nop(ButtonBar) + nop(GetTitleBar(core,Caption, Description)) + nop(CellContentSummary) + nop(ContentCell) + nop(ButtonBar) + "";

                result = '\r' + core.html.formStartMultipart() + nop(result) + '\r' + core.html.formEnd();
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        // ====================================================================================================
        //
        public static string GetEditRow(coreController core, string HTMLFieldString, string Caption, string HelpMessage = "", bool FieldRequired = false, bool AllowActiveEdit = false, string ignore0 = "") {
            string tempGetEditRow = null;
            try {
                //
                stringBuilderLegacyController FastString = new stringBuilderLegacyController();
                string Copy = null;
                //
                // Left Side
                //
                Copy = Caption;
                if (string.IsNullOrEmpty(Copy)) {
                    Copy = "&nbsp;";
                }
                tempGetEditRow = "<tr><td class=\"ccEditCaptionCon\"><nobr>" + Copy + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=1 height=15 >";
                //GetEditRow = "<tr><td class=""ccAdminEditCaption""><nobr>" & Copy & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=1 height=15 >"
                //If core.visitProperty.getboolean("AllowHelpIcon") Then
                //    'If HelpMessage <> "" Then
                //        GetEditRow = GetEditRow & "&nbsp;" & core.main_GetHelpLinkEditable(0, Caption, HelpMessage, FormInputName)
                //    'Else
                //    '    GetEditRow = GetEditRow & "&nbsp;<img alt=""space"" src=""/ccLib/images/spacer.gif"" " & IconWidthHeight & ">"
                //    'End If
                //End If
                tempGetEditRow = tempGetEditRow + "</nobr></td>";
                //
                // Right Side
                //
                Copy = HTMLFieldString;
                if (string.IsNullOrEmpty(Copy)) {
                    Copy = "&nbsp;";
                }
                Copy = "<div class=\"ccEditorCon\">" + Copy + "</div>";
                Copy += "<div class=\"ccEditorHelpCon\"><div class=\"closed\">" + HelpMessage + "</div></div>";
                tempGetEditRow += "<td class=\"ccEditFieldCon\">" + Copy + "</td></tr>";
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return tempGetEditRow;
        }
        // ====================================================================================================
        //
        public static string GetEditSubheadRow(coreController core, string Caption) {
            return "<tr><td colspan=2 class=\"ccAdminEditSubHeader\">" + Caption + "</td></tr>";
        }
        //
        //========================================================================
        // GetEditPanel, An edit panel is a section of an admin page, under a subhead. When in tab mode, the subhead is blocked, and the panel is assumed to go in its own tab windows
        //
        public static string GetEditPanel(coreController core, bool AllowHeading, string PanelHeading, string PanelDescription, string PanelBody) {
            string result = "";
            try {
                stringBuilderLegacyController FastString = new stringBuilderLegacyController();
                //
                result = result + "<div class=\"ccPanel3DReverse ccAdminEditBody\">";
                //
                // ----- Subhead
                //
                if (AllowHeading && (!string.IsNullOrEmpty(PanelHeading))) {
                    result = result + "<div class=\"ccAdminEditHeading\">" + PanelHeading + "</div>";
                }
                //
                // ----- Description
                //
                if (!string.IsNullOrEmpty(PanelDescription)) {
                    result = result + "<div class=\"ccAdminEditDescription\">" + PanelDescription + "</div>";
                }
                //
                result = result + PanelBody + "</div>";
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        // Edit Table Open
        //
        public static string EditTableOpen {
            get {
                return "<table border=0 cellpadding=3 cellspacing=0 width=\"100%\">";
            }
        }
        //
        //========================================================================
        // Edit Table Close
        //
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
        //==========================================================================================
        //   Report Cell
        //==========================================================================================
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
        //==========================================================================================
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
        //==========================================================================================
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
                core.handleException(ex);
            }
            return result;
        }
        //
        //=============================================================================
        //   Get Sort Column Ptr
        //
        //   returns the integer column ptr of the column last selected
        //=============================================================================
        //
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
        //=============================================================================
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
        //=============================================================================
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
        //=============================================================================
        //   Translate the old GetReport to the new GetReport2
        //=============================================================================
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
                core.handleException(ex);
            }
            return result;
        }
        //
        //=============================================================================
        //   Report
        //
        //   This is a list report that you have to fill in all the cells and pass them in arrays.
        //   The column headers are always assumed to include the orderby options. they are linked. To get the correct orderby, the calling program
        //   has to call GetReport2orderby
        //
        //
        //=============================================================================
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
                result = result + Content.Text;
                //
                // ----- Post Table copy
                //
                if ((DataRowCount / (double)ReportPageSize) != Math.Floor((DataRowCount / (double)ReportPageSize))) {
                    PageCount = encodeInteger((DataRowCount / (double)ReportPageSize) + 0.5);
                } else {
                    PageCount = encodeInteger(DataRowCount / (double)ReportPageSize);
                }
                if (PageCount > 1) {
                    result = result + "<br>Page " + ReportPageNumber + " (Row " + (RowBAse) + " of " + DataRowCount + ")";
                    if (PageCount > 20) {
                        PagePointer = ReportPageNumber - 10;
                    }
                    if (PagePointer < 1) {
                        PagePointer = 1;
                    }
                    if (PageCount > 1) {
                        result = result + "<br>Go to Page ";
                        if (PagePointer != 1) {
                            WorkingQS = core.doc.refreshQueryString;
                            WorkingQS = genericController.modifyQueryString(WorkingQS, "GotoPage", "1", true);
                            result = result + "<a href=\"" + core.webServer.requestPage + "?" + WorkingQS + "\">1</A>...&nbsp;";
                        }
                        WorkingQS = core.doc.refreshQueryString;
                        WorkingQS = genericController.modifyQueryString(WorkingQS, RequestNamePageSize, ReportPageSize.ToString(), true);
                        while ((PagePointer <= PageCount) && (LinkCount < 20)) {
                            if (PagePointer == ReportPageNumber) {
                                result = result + PagePointer + "&nbsp;";
                            } else {
                                WorkingQS = genericController.modifyQueryString(WorkingQS, RequestNamePageNumber, PagePointer.ToString(), true);
                                result = result + "<a href=\"" + core.webServer.requestPage + "?" + WorkingQS + "\">" + PagePointer + "</A>&nbsp;";
                            }
                            PagePointer = PagePointer + 1;
                            LinkCount = LinkCount + 1;
                        }
                        if (PagePointer < PageCount) {
                            WorkingQS = genericController.modifyQueryString(WorkingQS, RequestNamePageNumber, PageCount.ToString(), true);
                            result = result + "...<a href=\"" + core.webServer.requestPage + "?" + WorkingQS + "\">" + PageCount + "</A>&nbsp;";
                        }
                        if (ReportPageNumber < PageCount) {
                            WorkingQS = genericController.modifyQueryString(WorkingQS, RequestNamePageNumber, (ReportPageNumber + 1).ToString(), true);
                            result = result + "...<a href=\"" + core.webServer.requestPage + "?" + WorkingQS + "\">next</A>&nbsp;";
                        }
                        result = result + "<br>&nbsp;";
                    }
                }
                //
                result = ""
                + PreTableCopy + "<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr><td style=\"padding:10px;\">"
                + result + "</td></tr></table>"
                + PostTableCopy + "";
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        // Get the Normal Edit Button Bar String used on Normal Edit and others
        //
        public static string GetFormBodyAdminOnly() {
            return "<div class=\"ccError\" style=\"margin:10px;padding:10px;background-color:white;\">This page requires administrator permissions.</div>";
        }
        //
        // ====================================================================================================
        //
        public static string getButtonPrimary(string buttonValue, string onclick = "", bool disabled = false, string htmlId = "", string htmlName = "button") {
            return htmlController.getButton(buttonValue, htmlName, htmlId, onclick, disabled, "btn btn-primary mr-1 btn-sm");
           // string htmlClass = "btn btn-primary mr-1 btn-sm btn-sm";
           // string button = "<input type=submit name=button value=\"" + buttonValue + "\" id=\"" + htmlId + "\"OnClick=\"" + onclick + "\" class=\"" + htmlClass + "\">";
       }
        //
        // ====================================================================================================
        //
        public static string getButtonDanger(string buttonValue, string onclick = "", bool disabled = false, string htmlId = "") {
            return htmlController.getButton(buttonValue, "button", htmlId, onclick, disabled, "btn btn-danger mr-1 btn-sm");
            // string htmlClass = "btn btn-primary mr-1 btn-sm btn-sm";
            // string button = "<input type=submit name=button value=\"" + buttonValue + "\" id=\"" + htmlId + "\"OnClick=\"" + onclick + "\" class=\"" + htmlClass + "\">";
        }
    }
}
