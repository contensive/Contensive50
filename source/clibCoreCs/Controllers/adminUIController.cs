
using System;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Contensive.Core;
using Contensive.Core.Models.Entity;
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
        //
        private coreClass cpCore;
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        public adminUIController(coreClass cpCore) : base() {
            this.cpCore = cpCore;
        }
        //
        //===========================================================================
        //
        public string GetTitleBar(string Title, string Description) {
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
                if (cpCore.doc.debug_iUserError != "") {
                    Copy += "<div>" + errorController.error_GetUserError(cpCore) + "</div>";
                }
                //
                if (!string.IsNullOrEmpty(Copy)) {
                    tempGetTitleBar = tempGetTitleBar + "<div>&nbsp;</div><div class=\"ccAdminInfoBar ccPanel3DReverse\">" + Copy + "</div>";
                }
                return tempGetTitleBar + "</div>";
                //
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            handleLegacyClassError("GetTitleBar");
            return tempGetTitleBar;
        }
        //
        //========================================================================
        // Get the Normal Edit Button Bar String
        //
        //   used on Normal Edit and others
        //========================================================================
        //
        public string GetEditButtonBar2(int MenuDepth, bool AllowDelete, bool AllowCancel, bool allowSave, bool AllowSpellCheck, bool ignorefalse, bool ignorefalse2, bool ignorefalse3, bool ignorefalse4, bool AllowAdd, bool ignore_AllowReloadCDef, bool HasChildRecords, bool IsPageContent, bool AllowMarkReviewed, bool AllowRefresh, bool AllowCreateDuplicate) {
            string tempGetEditButtonBar2 = null;
            try {
                //
                string JSOnClick = null;
                //
                tempGetEditButtonBar2 = "";
                //
                if (AllowCancel) {
                    if (MenuDepth == 1) {
                        //
                        // Close if this is the root depth of a popup window
                        //
                        tempGetEditButtonBar2 = tempGetEditButtonBar2 + "<input TYPE=\"SUBMIT\" NAME=\"BUTTON\" VALUE=\"" + ButtonClose + "\" OnClick=\"window.close();\">";
                    } else {
                        //
                        // Cancel
                        //
                        tempGetEditButtonBar2 = tempGetEditButtonBar2 + "<input TYPE=\"SUBMIT\" NAME=\"BUTTON\" VALUE=\"" + ButtonCancel + "\" onClick=\"return processSubmit(this);\">";
                    }
                }
                if (allowSave) {
                    //
                    // Save
                    //
                    tempGetEditButtonBar2 = tempGetEditButtonBar2 + "<input TYPE=\"SUBMIT\" NAME=\"BUTTON\" VALUE=\"" + ButtonSave + "\" onClick=\"return processSubmit(this);\">";
                    //
                    // OK
                    //
                    tempGetEditButtonBar2 = tempGetEditButtonBar2 + "<input TYPE=\"SUBMIT\" NAME=\"BUTTON\" VALUE=\"" + ButtonOK + "\" onClick=\"return processSubmit(this);\">";
                    if (AllowAdd) {
                        //
                        // OK
                        //
                        tempGetEditButtonBar2 = tempGetEditButtonBar2 + "<input TYPE=\"SUBMIT\" NAME=\"BUTTON\" VALUE=\"" + ButtonSaveAddNew + "\" onClick=\"return processSubmit(this);\">";
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
                    tempGetEditButtonBar2 = tempGetEditButtonBar2 + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonDelete + "\" onClick=\"" + JSOnClick + "\">";
                } else {
                    tempGetEditButtonBar2 = tempGetEditButtonBar2 + "<input TYPE=SUBMIT NAME=BUTTON disabled=\"disabled\" VALUE=\"" + ButtonDelete + "\">";
                }
                //    If AllowSpellCheck Then
                //        '
                //        ' Spell Check
                //        '
                //        GetEditButtonBar2 = GetEditButtonBar2 & "<input TYPE=""SUBMIT"" NAME=""BUTTON"" VALUE=""" & ButtonSpellCheck & """ onClick=""return processSubmit(this);"">"
                //    End If
                //If ignorefalse Then
                //    '
                //    ' Publish
                //    '
                //    GetEditButtonBar2 = GetEditButtonBar2 & cpCore.html.html_GetFormButton(ButtonPublish, RequestNameButton)
                //End If
                //If ignorefalse2 Then
                //    '
                //    ' Abort
                //    '
                //    GetEditButtonBar2 = GetEditButtonBar2 & cpCore.html.html_GetFormButton(ButtonAbortEdit, RequestNameButton)
                //End If
                //If ignorefalse3 Then
                //    '
                //    ' Submit for Publishing
                //    '
                //    GetEditButtonBar2 = GetEditButtonBar2 & cpCore.html.html_GetFormButton(ButtonPublishSubmit, RequestNameButton)
                //End If
                //If ignorefalse4 Then
                //    '
                //    ' Approve Publishing
                //    '
                //    GetEditButtonBar2 = GetEditButtonBar2 & cpCore.html.html_GetFormButton(ButtonPublishApprove, RequestNameButton)
                //End If
                if (ignore_AllowReloadCDef) {
                    //
                    // Reload Content Definitions
                    //
                    tempGetEditButtonBar2 = tempGetEditButtonBar2 + cpCore.html.html_GetFormButton(ButtonSaveandInvalidateCache, RequestNameButton);
                }
                if (AllowMarkReviewed) {
                    //
                    // Reload Content Definitions
                    //
                    tempGetEditButtonBar2 = tempGetEditButtonBar2 + cpCore.html.html_GetFormButton(ButtonMarkReviewed, RequestNameButton);
                }
                if (AllowRefresh) {
                    //
                    // just like a save, but don't save jsut redraw
                    //
                    tempGetEditButtonBar2 = tempGetEditButtonBar2 + cpCore.html.html_GetFormButton(ButtonRefresh, RequestNameButton);
                }
                if (AllowCreateDuplicate) {
                    //
                    // just like a save, but don't save jsut redraw
                    //
                    tempGetEditButtonBar2 = tempGetEditButtonBar2 + cpCore.html.html_GetFormButton(ButtonCreateDuplicate, RequestNameButton, "", "return processSubmit(this)");
                }
                //
                tempGetEditButtonBar2 = ""
                    + "\r\n\t" + GetHTMLComment("ButtonBar") + "\r\n\t<div class=\"ccButtonCon\">"
                    + htmlIndent(tempGetEditButtonBar2) + "\r\n\t</div><!-- ButtonBar End -->"
                    + "";
                return tempGetEditButtonBar2;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            handleLegacyClassError("GetEditButtonBar2");
            //
            return tempGetEditButtonBar2;
        }
        //
        //========================================================================
        // Return a panel header with the header message reversed out of the left
        //========================================================================
        //
        public string GetHeader(string HeaderMessage, string RightSideMessage = "") {
            string s = string.Empty;
            try {
                if (string.IsNullOrEmpty(RightSideMessage)) {
                    RightSideMessage = cpCore.doc.profileStartTime.ToString("G");
                }
                if (isInStr(1, HeaderMessage + RightSideMessage, "\r\n")) {
                    s = ""
                        + "\r<td width=\"50%\" valign=Middle class=\"cchLeft\">"
                        + htmlIndent(HeaderMessage) + "\r</td>"
                        + "\r<td width=\"50%\" valign=Middle class=\"cchRight\">"
                        + htmlIndent(RightSideMessage) + "\r</td>";
                } else {
                    s = ""
                        + "\r<td width=\"50%\" valign=Middle class=\"cchLeft\">" + HeaderMessage + "</td>"
                        + "\r<td width=\"50%\" valign=Middle class=\"cchRight\">" + RightSideMessage + "</td>";
                }
                s = ""
                    + "\r<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr>"
                    + htmlIndent(s) + "\r</tr></table>"
                    + "";
                s = ""
                    + "\r<div class=\"ccHeaderCon\">"
                    + htmlIndent(s) + "\r</div>"
                    + "";
                //
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return s;
        }
        //
        //
        //
        public string GetButtonsFromList(string ButtonList, bool AllowDelete, bool AllowAdd, string ButtonName) {
            string s = string.Empty;
            try {
                string[] Buttons = null;
                int Ptr = 0;
                if (!string.IsNullOrEmpty(ButtonList.Trim(' '))) {
                    Buttons = ButtonList.Split(',');
                    for (Ptr = 0; Ptr <= Buttons.GetUpperBound(0); Ptr++) {
                        //INSTANT C# NOTE: The following VB 'Select Case' included either a non-ordinal switch expression or non-ordinal, range-type, or non-constant 'Case' expressions and was converted to C# 'if-else' logic:
                        //						Select Case Trim(Buttons[Ptr])
                        //ORIGINAL LINE: Case Trim(ButtonDelete)
                        if (Buttons[Ptr].Trim(' ') == encodeText(ButtonDelete).Trim(' ')) {
                            if (AllowDelete) {
                                s = s + "<input TYPE=SUBMIT NAME=\"" + ButtonName + "\" VALUE=\"" + Buttons[Ptr] + "\" onClick=\"if(!DeleteCheck())return false;\">";
                            } else {
                                s = s + "<input TYPE=SUBMIT NAME=\"" + ButtonName + "\" DISABLED VALUE=\"" + Buttons[Ptr] + "\">";
                            }
                        }
                        //ORIGINAL LINE: Case Trim(ButtonClose)
                        else if (Buttons[Ptr].Trim(' ') == encodeText(ButtonClose).Trim(' ')) {
                            s = s + cpCore.html.html_GetFormButton(Buttons[Ptr], "", "", "window.close();");
                        }
                        //ORIGINAL LINE: Case Trim(ButtonAdd)
                        else if (Buttons[Ptr].Trim(' ') == encodeText(ButtonAdd).Trim(' ')) {
                            if (AllowAdd) {
                                s = s + "<input TYPE=SUBMIT NAME=\"" + ButtonName + "\" VALUE=\"" + Buttons[Ptr] + "\" onClick=\"return processSubmit(this);\">";
                            } else {
                                s = s + "<input TYPE=SUBMIT NAME=\"" + ButtonName + "\" DISABLED VALUE=\"" + Buttons[Ptr] + "\" onClick=\"return processSubmit(this);\">";
                            }
                        }
                        //ORIGINAL LINE: Case ""
                        else if (string.IsNullOrEmpty(Buttons[Ptr].Trim(' '))) {
                        }
                        //ORIGINAL LINE: Case Else
                        else {
                            s = s + cpCore.html.html_GetFormButton(Buttons[Ptr], ButtonName);
                        }
                    }
                }


                //
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return s;
        }
        //
        //
        //
        public string GetButtonBar(string LeftButtons, string RightButtons) {
            string tempGetButtonBar = null;
            try {
                //
                if (string.IsNullOrEmpty(LeftButtons + RightButtons)) {
                    //
                    // nothing there
                    //
                } else if (string.IsNullOrEmpty(RightButtons)) {
                    tempGetButtonBar = ""
                        + "\r<table border=0 cellpadding=0 cellspacing=0 width=\"100%\">"
                        + cr2 + "<tr>"
                        + cr3 + "<td align=left  class=\"ccButtonCon\">" + LeftButtons + "</td>"
                        + cr2 + "</tr>"
                        + "\r</table>";
                } else {
                    tempGetButtonBar = ""
                        + "\r<table border=0 cellpadding=0 cellspacing=0 width=\"100%\">"
                        + cr2 + "<tr>"
                        + cr3 + "<td align=left  class=\"ccButtonCon\">"
                        + cr4 + "<table border=0 cellpadding=0 cellspacing=0 width=\"100%\">"
                        + cr5 + "<tr>"
                        + cr6 + "<td width=\"50%\" align=left>" + LeftButtons + "</td>"
                        + cr6 + "<td width=\"50%\" align=right>" + RightButtons + "</td>"
                        + cr5 + "</tr>"
                        + cr4 + "</table>"
                        + cr3 + "</td>"
                        + cr2 + "</tr>"
                        + "\r</table>";
                }
                return tempGetButtonBar;
                //
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            handleLegacyClassError("GetButtonBar");
            return tempGetButtonBar;
        }
        //
        //
        //
        public string GetButtonBarForIndex(string LeftButtons, string RightButtons, int PageNumber, int RecordsPerPage, int PageCount) {
            string tempGetButtonBarForIndex = null;
            try {
                //
                int Ptr = 0;
                string Nav = string.Empty;
                int NavStart = 0;
                int NavEnd = 0;
                //
                tempGetButtonBarForIndex = GetButtonBar(LeftButtons, RightButtons);
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
                //    GetButtonBarForIndex = GetButtonBarForIndex _
                //        & CR & "<script language=""javascript"">function bbj(p){document.getElementsByName('indexGoToPage')[0].value=p.innerHTML;document.adminForm.submit();}</script>" _
                //        & CR & "<div class=""ccJumpCon"">" _
                //        & cr2 & "<ul>Page&nbsp;" _
                //        & cr3 & Nav _
                //        & cr2 & "</ul>" _
                //        & CR & "</div>"
                return tempGetButtonBarForIndex;
                //
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            handleLegacyClassError("GetButtonBar");
            return tempGetButtonBarForIndex;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        public string GetBody(string Caption, string ButtonListLeft, string ButtonListRight, bool AllowAdd, bool AllowDelete, string Description, string ContentSummary, int ContentPadding, string Content) {
            string result = string.Empty;
            try {
                string ContentCell = string.Empty;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string ButtonBar = null;
                string LeftButtons = string.Empty;
                string RightButtons = string.Empty;
                string CellContentSummary = string.Empty;
                //
                // Build ButtonBar
                //
                if (!string.IsNullOrEmpty(ButtonListLeft.Trim(' '))) {
                    LeftButtons = GetButtonsFromList(ButtonListLeft, AllowDelete, AllowAdd, "Button");
                }
                if (!string.IsNullOrEmpty(ButtonListRight.Trim(' '))) {
                    RightButtons = GetButtonsFromList(ButtonListRight, AllowDelete, AllowAdd, "Button");
                }
                ButtonBar = GetButtonBar(LeftButtons, RightButtons);
                if (!string.IsNullOrEmpty(ContentSummary)) {
                    CellContentSummary = ""
                    + "\r<div class=\"ccPanelBackground\" style=\"padding:10px;\">"
                    + htmlIndent(cpCore.html.main_GetPanel(ContentSummary, "ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)) + "\r</div>";
                }
                //
                ContentCell = ""
                + "\r<div style=\"padding:" + ContentPadding + "px;\">"
                + htmlIndent(Content) + "\r</div>";
                result = result + htmlIndent(ButtonBar) + htmlIndent(GetTitleBar(Caption, Description)) + htmlIndent(CellContentSummary) + htmlIndent(ContentCell) + htmlIndent(ButtonBar) + "";

                result = '\r' + cpCore.html.html_GetUploadFormStart() + htmlIndent(result) + '\r' + cpCore.html.html_GetUploadFormEnd();
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //
        //
        public string GetEditRow(string HTMLFieldString, string Caption, string HelpMessage = "", bool FieldRequired = false, bool AllowActiveEdit = false, string ignore0 = "") {
            string tempGetEditRow = null;
            try {
                //
                stringBuilderLegacyController FastString = new stringBuilderLegacyController();
                string Copy = null;
                string FormInputName = null;
                //
                // Left Side
                //
                Copy = Caption;
                if (string.IsNullOrEmpty(Copy)) {
                    Copy = "&nbsp;";
                }
                tempGetEditRow = "<tr><td class=\"ccEditCaptionCon\"><nobr>" + Copy + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=1 height=15 >";
                //GetEditRow = "<tr><td class=""ccAdminEditCaption""><nobr>" & Copy & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=1 height=15 >"
                //If cpCore.visitProperty.getboolean("AllowHelpIcon") Then
                //    'If HelpMessage <> "" Then
                //        GetEditRow = GetEditRow & "&nbsp;" & cpCore.main_GetHelpLinkEditable(0, Caption, HelpMessage, FormInputName)
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
                //If HelpMessage <> "" Then
                Copy += "<div class=\"ccEditorHelpCon\"><div class=\"closed\">" + HelpMessage + "</div></div>";
                //Copy = Copy & "<div style=""padding:10px;white-space:normal"">" & HelpMessage & "</div>"
                //End If
                return tempGetEditRow + "<td class=\"ccEditFieldCon\">" + Copy + "</td></tr>";
                //GetEditRow = GetEditRow & "<td class=""ccAdminEditField"">" & Copy & "</td></tr>"
                //
                //
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            handleLegacyClassError("GetEditRow");
            return tempGetEditRow;
        }
        //
        //
        //
        public string GetEditRowWithHelpEdit(string HTMLFieldString, string Caption, string HelpMessage = "", bool FieldRequired = false, bool AllowActiveEdit = false, string ignore0 = "") {
            string tempGetEditRowWithHelpEdit = null;
            try {
                //
                stringBuilderLegacyController FastString = new stringBuilderLegacyController();
                string Copy = null;
                string FormInputName = null;
                //
                Copy = Caption;
                if (string.IsNullOrEmpty(Copy)) {
                    Copy = "&nbsp;";
                }
                tempGetEditRowWithHelpEdit = "<tr><td class=\"ccAdminEditCaption\"><nobr>" + Copy;
                if (cpCore.visitProperty.getBoolean("AllowHelpIcon")) {
                    //If HelpMessage <> "" Then
                    //GetEditRowWithHelpEdit = GetEditRowWithHelpEdit & "&nbsp;" & cpCore.main_GetHelpLinkEditable(0, Caption, HelpMessage, FormInputName)
                    //Else
                    //    GetEditRowWithHelpEdit = GetEditRowWithHelpEdit & "&nbsp;<img alt=""space"" src=""/ccLib/images/spacer.gif"" " & IconWidthHeight & ">"
                    //End If
                }
                tempGetEditRowWithHelpEdit = tempGetEditRowWithHelpEdit + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=1 height=15 ></nobr></td>";
                Copy = HTMLFieldString;
                if (string.IsNullOrEmpty(Copy)) {
                    Copy = "&nbsp;";
                }
                return tempGetEditRowWithHelpEdit + "<td class=\"ccAdminEditField\">" + Copy + "</td></tr>";
                //
                //
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            handleLegacyClassError("GetEditRowWithHelpEdit");
            return tempGetEditRowWithHelpEdit;
        }
        //
        //
        //
        public string GetEditSubheadRow(string Caption) {
            return "<tr><td colspan=2 class=\"ccAdminEditSubHeader\">" + Caption + "</td></tr>";
        }
        //
        //========================================================================
        // GetEditPanel
        //
        //   An edit panel is a section of an admin page, under a subhead.
        //   When in tab mode, the subhead is blocked, and the panel is assumed to
        //   go in its own tab windows
        //========================================================================
        //
        public string GetEditPanel(bool AllowHeading, string PanelHeading, string PanelDescription, string PanelBody) {
            string result = string.Empty;
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
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        // Edit Table Open
        //========================================================================
        //
        public string EditTableOpen {
            get {
                return "<table border=0 cellpadding=3 cellspacing=0 width=\"100%\">";
            }
        }
        //
        //========================================================================
        // Edit Table Close
        //========================================================================
        //
        public string EditTableClose {
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
        private string GetReport_Cell(string Copy, string Align, int Columns, int RowPointer) {
            string tempGetReport_Cell = null;
            string iAlign = null;
            string Style = null;
            string CellCopy = null;
            //
            iAlign = encodeEmptyText(Align, "left");
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
        private string GetReport_CellHeader(int ColumnPtr, string Title, string Width, string Align, string ClassStyle, string RefreshQueryString, SortingStateEnum SortingState) {
            string result = string.Empty;
            try {
                string Style = null;
                string Copy = null;
                string QS = null;
                int WidthTest = 0;
                string LinkTitle = null;
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
                        QS = genericController.ModifyQueryString(RefreshQueryString, "ColSort", ((int)SortingStateEnum.SortableSetAZ).ToString(), true);
                        QS = genericController.ModifyQueryString(QS, "ColPtr", ColumnPtr.ToString(), true);
                        Copy = "<a href=\"?" + QS + "\" title=\"Sort A-Z\" class=\"ccAdminListCaption\">" + Copy + "</a>";
                        break;
                    case SortingStateEnum.SortableSetza:
                        QS = genericController.ModifyQueryString(RefreshQueryString, "ColSort", ((int)SortingStateEnum.SortableSetAZ).ToString(), true);
                        QS = genericController.ModifyQueryString(QS, "ColPtr", ColumnPtr.ToString(), true);
                        Copy = "<a href=\"?" + QS + "\" title=\"Sort A-Z\" class=\"ccAdminListCaption\">" + Copy + "<img src=\"/ccLib/images/arrowup.gif\" width=8 height=8 border=0></a>";
                        break;
                    case SortingStateEnum.SortableSetAZ:
                        QS = genericController.ModifyQueryString(RefreshQueryString, "ColSort", ((int)SortingStateEnum.SortableSetza).ToString(), true);
                        QS = genericController.ModifyQueryString(QS, "ColPtr", ColumnPtr.ToString(), true);
                        Copy = "<a href=\"?" + QS + "\" title=\"Sort Z-A\" class=\"ccAdminListCaption\">" + Copy + "<img src=\"/ccLib/images/arrowdown.gif\" width=8 height=8 border=0></a>";
                        break;
                }
                //
                if (!string.IsNullOrEmpty(Width)) {
                    WidthTest = genericController.EncodeInteger(Width.ToLower().Replace("px", ""));
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
                cpCore.handleException(ex);
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
        public int GetReportSortColumnPtr(int DefaultSortColumnPtr) {
            int tempGetReportSortColumnPtr = 0;
            string VarText;
            //
            VarText = cpCore.docProperties.getText("ColPtr");
            tempGetReportSortColumnPtr = genericController.EncodeInteger(VarText);
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
        public int GetReportSortType() {
            int tempGetReportSortType = 0;
            string VarText;
            //
            VarText = cpCore.docProperties.getText("ColPtr");
            if ((EncodeInteger(VarText) != 0) || (VarText == "0")) {
                //
                // A valid ColPtr was found
                //
                tempGetReportSortType = cpCore.docProperties.getInteger("ColSort");
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
        public string GetReport(int RowCount, string[] ColCaption, string[] ColAlign, string[] ColWidth, string[,] Cells, int PageSize, int PageNumber, string PreTableCopy, string PostTableCopy, int DataRowCount, string ClassStyle) {
            string result = string.Empty;
            try {
                int ColCnt = Cells.GetUpperBound(1);
                bool[] ColSortable = new bool[ColCnt + 1];
                for (int Ptr = 0; Ptr < ColCnt; Ptr++) {
                    ColSortable[Ptr] = false;
                }
                //
                result = GetReport2(RowCount, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, ClassStyle, ColSortable, 0);
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
        public string GetReport2(int RowCount, string[] ColCaption, string[] ColAlign, string[] ColWidth, string[,] Cells, int PageSize, int PageNumber, string PreTableCopy, string PostTableCopy, int DataRowCount, string ClassStyle, bool[] ColSortable, int DefaultSortColumnPtr) {
            string result = string.Empty;
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
                RQS = cpCore.doc.refreshQueryString;
                //
                SortColPtr = GetReportSortColumnPtr(DefaultSortColumnPtr);
                SortColType = GetReportSortType();
                //
                // ----- Start the table
                //
                Content.Add(StartTable(3, 1, 0));
                //
                // ----- Header
                //
                Content.Add("\r\n<tr>");
                Content.Add(GetReport_CellHeader(0, "Row", "50", "Right", "ccAdminListCaption", RQS, SortingStateEnum.NotSortable));
                for (ColumnPtr = 0; ColumnPtr < ColumnCount; ColumnPtr++) {
                    ColumnWidth = ColWidth[ColumnPtr];
                    if (!ColSortable[ColumnPtr]) {
                        //
                        // not sortable column
                        //
                        Content.Add(GetReport_CellHeader(ColumnPtr, ColCaption[ColumnPtr], ColumnWidth, ColAlign[ColumnPtr], "ccAdminListCaption", RQS, SortingStateEnum.NotSortable));
                    } else if (ColumnPtr == SortColPtr) {
                        //
                        // This is the current sort column
                        //
                        Content.Add(GetReport_CellHeader(ColumnPtr, ColCaption[ColumnPtr], ColumnWidth, ColAlign[ColumnPtr], "ccAdminListCaption", RQS, (SortingStateEnum)SortColType));
                    } else {
                        //
                        // Column is sortable, but not selected
                        //
                        Content.Add(GetReport_CellHeader(ColumnPtr, ColCaption[ColumnPtr], ColumnWidth, ColAlign[ColumnPtr], "ccAdminListCaption", RQS, SortingStateEnum.SortableNotSet));
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
                    Content.Add(GetReport_Cell((RowBAse + RowPointer).ToString(), "right", 1, RowPointer));
                    Content.Add(GetReport_Cell("-- End --", "left", ColumnCount, 0));
                    Content.Add("\r\n</tr>");
                } else {
                    RowBAse = (ReportPageSize * (ReportPageNumber - 1)) + 1;
                    for (RowPointer = 0; RowPointer < RowCount; RowPointer++) {
                        Content.Add("\r\n<tr>");
                        Content.Add(GetReport_Cell((RowBAse + RowPointer).ToString(), "right", 1, RowPointer));
                        for (ColumnPtr = 0; ColumnPtr < ColumnCount; ColumnPtr++) {
                            Content.Add(GetReport_Cell(Cells[RowPointer, ColumnPtr], ColAlign[ColumnPtr], 1, RowPointer));
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
                    PageCount = EncodeInteger((DataRowCount / (double)ReportPageSize) + 0.5);
                } else {
                    PageCount = EncodeInteger(DataRowCount / (double)ReportPageSize);
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
                            WorkingQS = cpCore.doc.refreshQueryString;
                            WorkingQS = genericController.ModifyQueryString(WorkingQS, "GotoPage", "1", true);
                            result = result + "<a href=\"" + cpCore.webServer.requestPage + "?" + WorkingQS + "\">1</A>...&nbsp;";
                        }
                        WorkingQS = cpCore.doc.refreshQueryString;
                        WorkingQS = genericController.ModifyQueryString(WorkingQS, RequestNamePageSize, ReportPageSize.ToString(), true);
                        while ((PagePointer <= PageCount) && (LinkCount < 20)) {
                            if (PagePointer == ReportPageNumber) {
                                result = result + PagePointer + "&nbsp;";
                            } else {
                                WorkingQS = genericController.ModifyQueryString(WorkingQS, RequestNamePageNumber, PagePointer.ToString(), true);
                                result = result + "<a href=\"" + cpCore.webServer.requestPage + "?" + WorkingQS + "\">" + PagePointer + "</A>&nbsp;";
                            }
                            PagePointer = PagePointer + 1;
                            LinkCount = LinkCount + 1;
                        }
                        if (PagePointer < PageCount) {
                            WorkingQS = genericController.ModifyQueryString(WorkingQS, RequestNamePageNumber, PageCount.ToString(), true);
                            result = result + "...<a href=\"" + cpCore.webServer.requestPage + "?" + WorkingQS + "\">" + PageCount + "</A>&nbsp;";
                        }
                        if (ReportPageNumber < PageCount) {
                            WorkingQS = genericController.ModifyQueryString(WorkingQS, RequestNamePageNumber, (ReportPageNumber + 1).ToString(), true);
                            result = result + "...<a href=\"" + cpCore.webServer.requestPage + "?" + WorkingQS + "\">next</A>&nbsp;";
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
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        // Get the Normal Edit Button Bar String
        //
        //   used on Normal Edit and others
        //========================================================================
        //
        public string GetEditButtonBar(int MenuDepth, bool AllowDelete, bool AllowCancel, bool allowSave, bool AllowSpellCheck, bool AllowPublish, bool AllowAbort, bool AllowSubmit, bool AllowApprove) {
            return GetEditButtonBar2(MenuDepth, AllowDelete, AllowCancel, allowSave, AllowSpellCheck, AllowPublish, AllowAbort, AllowSubmit, AllowApprove, false, false, false, false, false, false, false);
        }
        //
        //========================================================================
        // Get the Normal Edit Button Bar String
        //
        //   used on Normal Edit and others
        //========================================================================
        //
        public string GetFormBodyAdminOnly() {
            return "<div class=\"ccError\" style=\"margin:10px;padding:10px;background-color:white;\">This page requires administrator permissions.</div>";
        }
        //
        //========================================================================
        //   Builds a single entry in the ReportFilter at the bottom of the page
        //========================================================================
        //
        public string GetReportFilterRow(string FormInput, string Caption) {
            string tempGetReportFilterRow = null;
            //
            tempGetReportFilterRow = ""
                + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">"
                + "<tr><td width=\"200\" align=\"right\" class=RowInput>" + FormInput + "</td>"
                + "<td width=\"100%\" align=\"left\" class=RowCaption>&nbsp;" + Caption + "</td></tr>"
                + "</table>";
            //
            return tempGetReportFilterRow;
        }
        //
        //=============================================================================
        //   Builds the panel around the filters at the bottom of the report
        //=============================================================================
        //
        public string GetReportFilter(string Title, string Body) {
            string result = "";
            result = result + "<div class=\"ccReportFilter\">";
            result = result + "<div class=\"Title\">" + Title + "</div>";
            result = result + "<div class=\"Body\">" + Body + "</div>";
            result = result + "</div>";
            return result;
        }
        //
        //===========================================================================
        /// <summary>
        /// handle legacy errors for this class, v1
        /// </summary>
        /// <param name="MethodName"></param>
        /// <remarks></remarks>
        private void handleLegacyClassError(string MethodName) {
            //
            throw (new Exception("unexpected exception in method [" + MethodName + "]"));
            //
        }
    }
}
