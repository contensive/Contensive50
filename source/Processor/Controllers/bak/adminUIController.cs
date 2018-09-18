

using Controllers;

// 

namespace Contensive.Core {
    
    // 
    // ====================================================================================================
    // '' <summary>
    // '' UI rendering for Admin
    // '' REFACTOR - add  try-catch
    // '' not IDisposable - not contained classes that need to be disposed
    // '' </summary>
    public class adminUIController {
        
        // 
        // ========================================================================
        // 
        private enum SortingStateEnum {
            
            NotSortable = 0,
            
            SortableSetAZ = 1,
            
            SortableSetza = 2,
            
            SortableNotSet = 3,
        }
        
        // 
        private coreClass cpCore;
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' constructor
        // '' </summary>
        // '' <param name="cp"></param>
        // '' <remarks></remarks>
        public adminUIController(coreClass cpCore) {
            this.cpCore = cpCore;
        }
        
        // 
        // ===========================================================================
        // 
        public string GetTitleBar(string Title, string Description) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            string Copy;
            // 
            GetTitleBar = ("<div class=\"ccAdminTitleBar\">" + Title);
            // GetTitleBar = "<div class=""ccAdminTitleBar"">" & Title & "</div>"
            Copy = Description;
            if ((genericController.vbInstr(1, Copy, "<p>", vbTextCompare) == 1)) {
                Copy = Copy.Substring(3);
                if ((InStrRev(Copy, "</p>", ,, vbTextCompare) 
                            == (Copy.Length - 4))) {
                    Copy = Copy.Substring(0, (Copy.Length - 4));
                }
                
            }
            
            // 
            //  Add Errors
            // 
            if ((cpCore.doc.debug_iUserError != "")) {
                Copy = (Copy + ("<div>" 
                            + (errorController.error_GetUserError(cpCore) + "</div>")));
            }
            
            // 
            if ((Copy != "")) {
                GetTitleBar = (GetTitleBar + ("<div> </div><div class=\"ccAdminInfoBar ccPanel3DReverse\">" 
                            + (Copy + "</div>")));
            }
            
            return (GetTitleBar + "</div>");
            
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            this.handleLegacyClassError("GetTitleBar");
        }
        
        // 
        // ========================================================================
        //  Get the Normal Edit Button Bar String
        // 
        //    used on Normal Edit and others
        // ========================================================================
        // 
        public string GetEditButtonBar2(
                    int MenuDepth, 
                    bool AllowDelete, 
                    bool AllowCancel, 
                    bool allowSave, 
                    bool AllowSpellCheck, 
                    bool ignorefalse, 
                    bool ignorefalse2, 
                    bool ignorefalse3, 
                    bool ignorefalse4, 
                    bool AllowAdd, 
                    bool ignore_AllowReloadCDef, 
                    bool HasChildRecords, 
                    bool IsPageContent, 
                    bool AllowMarkReviewed, 
                    bool AllowRefresh, 
                    bool AllowCreateDuplicate) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            string JSOnClick;
            // 
            GetEditButtonBar2 = "";
            if (AllowCancel) {
                if ((MenuDepth == 1)) {
                    // 
                    //  Close if this is the root depth of a popup window
                    // 
                    GetEditButtonBar2 = (GetEditButtonBar2 + ("<input TYPE=\"SUBMIT\" NAME=\"BUTTON\" VALUE=\"" 
                                + (ButtonClose + "\" OnClick=\"window.close();\">")));
                }
                else {
                    // 
                    //  Cancel
                    // 
                    GetEditButtonBar2 = (GetEditButtonBar2 + ("<input TYPE=\"SUBMIT\" NAME=\"BUTTON\" VALUE=\"" 
                                + (ButtonCancel + "\" onClick=\"return processSubmit(this);\">")));
                }
                
            }
            
            if (allowSave) {
                // 
                //  Save
                // 
                GetEditButtonBar2 = (GetEditButtonBar2 + ("<input TYPE=\"SUBMIT\" NAME=\"BUTTON\" VALUE=\"" 
                            + (ButtonSave + "\" onClick=\"return processSubmit(this);\">")));
                GetEditButtonBar2 = (GetEditButtonBar2 + ("<input TYPE=\"SUBMIT\" NAME=\"BUTTON\" VALUE=\"" 
                            + (ButtonOK + "\" onClick=\"return processSubmit(this);\">")));
                if (AllowAdd) {
                    // 
                    //  OK
                    // 
                    GetEditButtonBar2 = (GetEditButtonBar2 + ("<input TYPE=\"SUBMIT\" NAME=\"BUTTON\" VALUE=\"" 
                                + (ButtonSaveAddNew + "\" onClick=\"return processSubmit(this);\">")));
                }
                
            }
            
            if (AllowDelete) {
                // 
                //  Delete
                // 
                if (IsPageContent) {
                    JSOnClick = "if(!DeletePageCheck())return false;";
                }
                else if (HasChildRecords) {
                    JSOnClick = "if(!DeleteCheckWithChildren())return false;";
                }
                else {
                    JSOnClick = "if(!DeleteCheck())return false;";
                }
                
                GetEditButtonBar2 = (GetEditButtonBar2 + ("<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" 
                            + (ButtonDelete + ("\" onClick=\"" 
                            + (JSOnClick + "\">")))));
            }
            else {
                GetEditButtonBar2 = (GetEditButtonBar2 + ("<input TYPE=SUBMIT NAME=BUTTON disabled=\"disabled\" VALUE=\"" 
                            + (ButtonDelete + "\">")));
            }
            
            //     If AllowSpellCheck Then
            //         '
            //         ' Spell Check
            //         '
            //         GetEditButtonBar2 = GetEditButtonBar2 & "<input TYPE=""SUBMIT"" NAME=""BUTTON"" VALUE=""" & ButtonSpellCheck & """ onClick=""return processSubmit(this);"">"
            //     End If
            // If ignorefalse Then
            //     '
            //     ' Publish
            //     '
            //     GetEditButtonBar2 = GetEditButtonBar2 & cpCore.html.html_GetFormButton(ButtonPublish, RequestNameButton)
            // End If
            // If ignorefalse2 Then
            //     '
            //     ' Abort
            //     '
            //     GetEditButtonBar2 = GetEditButtonBar2 & cpCore.html.html_GetFormButton(ButtonAbortEdit, RequestNameButton)
            // End If
            // If ignorefalse3 Then
            //     '
            //     ' Submit for Publishing
            //     '
            //     GetEditButtonBar2 = GetEditButtonBar2 & cpCore.html.html_GetFormButton(ButtonPublishSubmit, RequestNameButton)
            // End If
            // If ignorefalse4 Then
            //     '
            //     ' Approve Publishing
            //     '
            //     GetEditButtonBar2 = GetEditButtonBar2 & cpCore.html.html_GetFormButton(ButtonPublishApprove, RequestNameButton)
            // End If
            if (ignore_AllowReloadCDef) {
                // 
                //  Reload Content Definitions
                // 
                GetEditButtonBar2 = (GetEditButtonBar2 + cpCore.html.html_GetFormButton(ButtonSaveandInvalidateCache, RequestNameButton));
            }
            
            if (AllowMarkReviewed) {
                // 
                //  Reload Content Definitions
                // 
                GetEditButtonBar2 = (GetEditButtonBar2 + cpCore.html.html_GetFormButton(ButtonMarkReviewed, RequestNameButton));
            }
            
            if (AllowRefresh) {
                // 
                //  just like a save, but don't save jsut redraw
                // 
                GetEditButtonBar2 = (GetEditButtonBar2 + cpCore.html.html_GetFormButton(ButtonRefresh, RequestNameButton));
            }
            
            if (AllowCreateDuplicate) {
                // 
                //  just like a save, but don't save jsut redraw
                // 
                GetEditButtonBar2 = (GetEditButtonBar2 + cpCore.html.html_GetFormButton(ButtonCreateDuplicate, RequestNameButton, ,, "return processSubmit(this)"));
            }
            
            // 
            return ("" + ("\r\n" + ('\t' 
                        + (GetHTMLComment("ButtonBar") + ("\r\n" + ('\t' + ("<div class=\"ccButtonCon\">" 
                        + (htmlIndent(GetEditButtonBar2) + ("\r\n" + ('\t' + ("</div><!-- ButtonBar End -->" + "")))))))))));
            
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            this.handleLegacyClassError("GetEditButtonBar2");
            // 
        }
        
        // 
        // ========================================================================
        //  Return a panel header with the header message reversed out of the left
        // ========================================================================
        // 
        public string GetHeader(string HeaderMessage, string RightSideMessage, void =, void ) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // Warning!!! Optional parameters not supported
            // 
            string s;
            // 
            if ((RightSideMessage == "")) {
                RightSideMessage = FormatDateTime(cpCore.doc.profileStartTime);
            }
            
            // 
            if (isInStr(1, (HeaderMessage + RightSideMessage), "\r\n")) {
                s = ("" 
                            + (cr + ("<td width=\"50%\" valign=Middle class=\"cchLeft\">" 
                            + (htmlIndent(HeaderMessage) 
                            + (cr + ("</td>" 
                            + (cr + ("<td width=\"50%\" valign=Middle class=\"cchRight\">" 
                            + (htmlIndent(RightSideMessage) 
                            + (cr + "</td>"))))))))));
            }
            else {
                s = ("" 
                            + (cr + ("<td width=\"50%\" valign=Middle class=\"cchLeft\">" 
                            + (HeaderMessage + ("</td>" 
                            + (cr + ("<td width=\"50%\" valign=Middle class=\"cchRight\">" 
                            + (RightSideMessage + "</td>"))))))));
            }
            
            s = ("" 
                        + (cr + ("<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr>" 
                        + (htmlIndent(s) 
                        + (cr + ("</tr></table>" + ""))))));
            s = ("" 
                        + (cr + ("<div class=\"ccHeaderCon\">" 
                        + (htmlIndent(s) 
                        + (cr + ("</div>" + ""))))));
            return s;
            
            // 
        ErrorTrap:
            this.handleLegacyClassError("GetHeader");
        }
        
        // 
        // 
        // 
        public string GetButtonsFromList(string ButtonList, bool AllowDelete, bool AllowAdd, string ButtonName) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            string s = String.Empty;
            string[] Buttons;
            int Ptr;
            // 
            if ((ButtonList.Trim() != "")) {
                Buttons = ButtonList.Split(",");
                for (Ptr = 0; (Ptr <= UBound(Buttons)); Ptr++) {
                    switch (Buttons[Ptr].Trim()) {
                        case ButtonDelete.Trim():
                            if (AllowDelete) {
                                s = (s + ("<input TYPE=SUBMIT NAME=\"" 
                                            + (ButtonName + ("\" VALUE=\"" 
                                            + (Buttons[Ptr] + "\" onClick=\"if(!DeleteCheck())return false;\">")))));
                            }
                            else {
                                s = (s + ("<input TYPE=SUBMIT NAME=\"" 
                                            + (ButtonName + ("\" DISABLED VALUE=\"" 
                                            + (Buttons[Ptr] + "\">")))));
                            }
                            
                            break;
                        case ButtonClose.Trim():
                            s = (s + cpCore.html.html_GetFormButton(Buttons[Ptr], ,, "window.close();"));
                            break;
                        case ButtonAdd.Trim():
                            if (AllowAdd) {
                                s = (s + ("<input TYPE=SUBMIT NAME=\"" 
                                            + (ButtonName + ("\" VALUE=\"" 
                                            + (Buttons[Ptr] + "\" onClick=\"return processSubmit(this);\">")))));
                            }
                            else {
                                s = (s + ("<input TYPE=SUBMIT NAME=\"" 
                                            + (ButtonName + ("\" DISABLED VALUE=\"" 
                                            + (Buttons[Ptr] + "\" onClick=\"return processSubmit(this);\">")))));
                            }
                            
                            break;
                        case "":
                            break;
                        default:
                            s = (s + cpCore.html.html_GetFormButton(Buttons[Ptr], ButtonName));
                            break;
                    }
                }
                
            }
            
            return s;
            
            // 
        ErrorTrap:
            this.handleLegacyClassError("GetButtonsFromList");
        }
        
        // 
        // 
        // 
        public string GetButtonBar(string LeftButtons, string RightButtons) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            if ((LeftButtons 
                        + (RightButtons == ""))) {
                // 
                //  nothing there
                // 
            }
            else if ((RightButtons == "")) {
                GetButtonBar = ("" 
                            + (cr + ("<table border=0 cellpadding=0 cellspacing=0 width=\"100%\">" 
                            + (cr2 + ("<tr>" 
                            + (cr3 + ("<td align=left  class=\"ccButtonCon\">" 
                            + (LeftButtons + ("</td>" 
                            + (cr2 + ("</tr>" 
                            + (cr + "</table>"))))))))))));
            }
            else {
                GetButtonBar = ("" 
                            + (cr + ("<table border=0 cellpadding=0 cellspacing=0 width=\"100%\">" 
                            + (cr2 + ("<tr>" 
                            + (cr3 + ("<td align=left  class=\"ccButtonCon\">" 
                            + (cr4 + ("<table border=0 cellpadding=0 cellspacing=0 width=\"100%\">" 
                            + (cr5 + ("<tr>" 
                            + (cr6 + ("<td width=\"50%\" align=left>" 
                            + (LeftButtons + ("</td>" 
                            + (cr6 + ("<td width=\"50%\" align=right>" 
                            + (RightButtons + ("</td>" 
                            + (cr5 + ("</tr>" 
                            + (cr4 + ("</table>" 
                            + (cr3 + ("</td>" 
                            + (cr2 + ("</tr>" 
                            + (cr + "</table>"))))))))))))))))))))))))))));
            }
            
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
        ErrorTrap:
            this.handleLegacyClassError("GetButtonBar");
        }
        
        // 
        // 
        // 
        public string GetButtonBarForIndex(string LeftButtons, string RightButtons, int PageNumber, int RecordsPerPage, int PageCount) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            int Ptr;
            string Nav = String.Empty;
            int NavStart;
            int NavEnd;
            // 
            GetButtonBarForIndex = this.GetButtonBar(LeftButtons, RightButtons);
            NavStart = (PageNumber - 9);
            if ((NavStart < 1)) {
                NavStart = 1;
            }
            
            NavEnd = (NavStart + 20);
            if ((NavEnd > PageCount)) {
                NavEnd = PageCount;
                NavStart = (NavEnd - 20);
                if ((NavStart < 1)) {
                    NavStart = 1;
                }
                
            }
            
            if ((NavStart > 1)) {
                Nav = (Nav + "<li onclick=\"bbj(this);\">1</li><li class=\"delim\">«</li>");
            }
            
            for (Ptr = NavStart; (Ptr <= NavEnd); Ptr++) {
                Nav = (Nav + ("<li onclick=\"bbj(this);\">" 
                            + (Ptr + "</li>")));
            }
            
            if ((NavEnd < PageCount)) {
                Nav = (Nav + ("<li class=\"delim\">»</li><li onclick=\"bbj(this);\">" 
                            + (PageCount + "</li>")));
            }
            
            Nav = genericController.vbReplace(Nav, (">" 
                            + (PageNumber + "<")), (" class=\"hit\">" 
                            + (PageNumber + "<")));
            return (GetButtonBarForIndex 
                        + (cr + ("<script language=\"javascript\">function bbj(p){document.getElementsByName(\'indexGoToPage\')[0].value=p." +
                        "innerHTML;document.adminForm.submit();}</script>" 
                        + (cr + ("<div class=\"ccJumpCon\">" 
                        + (cr2 + ("<ul><li class=\"caption\">Page</li>" 
                        + (cr3 
                        + (Nav 
                        + (cr2 + ("</ul>" 
                        + (cr + "</div>"))))))))))));
            
            // 
        ErrorTrap:
            this.handleLegacyClassError("GetButtonBar");
        }
        
        // 
        // ========================================================================
        // 
        // ========================================================================
        // 
        public string GetBody(string Caption, string ButtonListLeft, string ButtonListRight, bool AllowAdd, bool AllowDelete, string Description, string ContentSummary, int ContentPadding, string Content) {
            string result = String.Empty;
            try {
                string ContentCell = String.Empty;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                string ButtonBar;
                string LeftButtons = String.Empty;
                string RightButtons = String.Empty;
                string CellContentSummary = String.Empty;
                // 
                //  Build ButtonBar
                // 
                if ((ButtonListLeft.Trim() != "")) {
                    LeftButtons = this.GetButtonsFromList(ButtonListLeft, AllowDelete, AllowAdd, "Button");
                }
                
                if ((ButtonListRight.Trim() != "")) {
                    RightButtons = this.GetButtonsFromList(ButtonListRight, AllowDelete, AllowAdd, "Button");
                }
                
                ButtonBar = this.GetButtonBar(LeftButtons, RightButtons);
                if ((ContentSummary != "")) {
                    CellContentSummary = ("" 
                                + (cr + ("<div class=\"ccPanelBackground\" style=\"padding:10px;\">" 
                                + (htmlIndent(cpCore.html.main_GetPanel(ContentSummary, "ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)) 
                                + (cr + "</div>")))));
                }
                
                // 
                ContentCell = ("" 
                            + (cr + ("<div style=\"padding:" 
                            + (ContentPadding + ("px;\">" 
                            + (htmlIndent(Content) 
                            + (cr + "</div>")))))));
                result = (result 
                            + (htmlIndent(ButtonBar) 
                            + (htmlIndent(this.GetTitleBar(Caption, Description)) 
                            + (htmlIndent(CellContentSummary) 
                            + (htmlIndent(ContentCell) 
                            + (htmlIndent(ButtonBar) + ""))))));
                result = ("" 
                            + (cr 
                            + (cpCore.html.html_GetUploadFormStart() 
                            + (htmlIndent(result) 
                            + (cr + cpCore.html.html_GetUploadFormEnd)))));
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
            return result;
        }
        
        // 
        // 
        // 
        public string GetEditRow(string HTMLFieldString, string Caption, string HelpMessage, void =, void , bool FieldRequired, void =, void False, bool AllowActiveEdit, void =, void False, string ignore0, void =, void ) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // 
            stringBuilderLegacyController FastString = new stringBuilderLegacyController();
            string Copy;
            string FormInputName;
            // 
            //  Left Side
            // 
            Copy = Caption;
            if ((Copy == "")) {
                Copy = " ";
            }
            
            GetEditRow = ("<tr><td class=\"ccEditCaptionCon\"><nobr>" 
                        + (Copy + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=1 height=15 >"));
            GetEditRow = (GetEditRow + "</nobr></td>");
            Copy = HTMLFieldString;
            if ((Copy == "")) {
                Copy = " ";
            }
            
            Copy = ("<div class=\"ccEditorCon\">" 
                        + (Copy + "</div>"));
            Copy = (Copy + ("<div class=\"ccEditorHelpCon\"><div class=\"closed\">" 
                        + (HelpMessage + "</div></div>")));
            return (GetEditRow + ("<td class=\"ccEditFieldCon\">" 
                        + (Copy + "</td></tr>")));
            
            // 
        ErrorTrap:
            this.handleLegacyClassError("GetEditRow");
        }
        
        // 
        // 
        // 
        public string GetEditRowWithHelpEdit(string HTMLFieldString, string Caption, string HelpMessage, void =, void , bool FieldRequired, void =, void False, bool AllowActiveEdit, void =, void False, string ignore0, void =, void ) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // 
            stringBuilderLegacyController FastString = new stringBuilderLegacyController();
            string Copy;
            string FormInputName;
            // 
            Copy = Caption;
            if ((Copy == "")) {
                Copy = " ";
            }
            
            GetEditRowWithHelpEdit = ("<tr><td class=\"ccAdminEditCaption\"><nobr>" + Copy);
            if (cpCore.visitProperty.getBoolean("AllowHelpIcon")) {
                // If HelpMessage <> "" Then
                // GetEditRowWithHelpEdit = GetEditRowWithHelpEdit & " " & cpCore.main_GetHelpLinkEditable(0, Caption, HelpMessage, FormInputName)
                // Else
                //     GetEditRowWithHelpEdit = GetEditRowWithHelpEdit & " <img alt=""space"" src=""/ccLib/images/spacer.gif"" " & IconWidthHeight & ">"
                // End If
            }
            
            GetEditRowWithHelpEdit = (GetEditRowWithHelpEdit + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=1 height=15 ></nobr></td>");
            Copy = HTMLFieldString;
            if ((Copy == "")) {
                Copy = " ";
            }
            
            return (GetEditRowWithHelpEdit + ("<td class=\"ccAdminEditField\">" 
                        + (Copy + "</td></tr>")));
            
            // 
        ErrorTrap:
            this.handleLegacyClassError("GetEditRowWithHelpEdit");
        }
        
        // 
        // 
        // 
        public string GetEditSubheadRow(string Caption) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            return ("<tr><td colspan=2 class=\"ccAdminEditSubHeader\">" 
                        + (Caption + "</td></tr>"));
            
            // 
        ErrorTrap:
            this.handleLegacyClassError("GetEditSubheadRow");
        }
        
        // 
        // ========================================================================
        //  GetEditPanel
        // 
        //    An edit panel is a section of an admin page, under a subhead.
        //    When in tab mode, the subhead is blocked, and the panel is assumed to
        //    go in its own tab windows
        // ========================================================================
        // 
        public string GetEditPanel(bool AllowHeading, string PanelHeading, string PanelDescription, string PanelBody) {
            string result = String.Empty;
            try {
                stringBuilderLegacyController FastString = new stringBuilderLegacyController();
                // 
                result = (result + "<div class=\"ccPanel3DReverse ccAdminEditBody\">");
                if ((AllowHeading 
                            && (PanelHeading != ""))) {
                    result = (result + ("<div class=\"ccAdminEditHeading\">" 
                                + (PanelHeading + "</div>")));
                }
                
                // 
                //  ----- Description
                // 
                if ((PanelDescription != "")) {
                    result = (result + ("<div class=\"ccAdminEditDescription\">" 
                                + (PanelDescription + "</div>")));
                }
                
                // 
                result = (result 
                            + (PanelBody + "</div>"));
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
            return result;
        }
        
        // 
        // ========================================================================
        //  Edit Table Open
        // ========================================================================
        // 
        public string EditTableOpen {
            get {
                return "<table border=0 cellpadding=3 cellspacing=0 width=\"100%\">";
            }
        }
        
        public string EditTableClose {
            get {
                return ("<tr>" + ("<td width=20%><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=1 ></td>" + ("<td width=80%><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=1 ></td>" + ("</tr>" + "</table>"))));
            }
        }
        
        private string GetReport_Cell(string Copy, string Align, int Columns, int RowPointer) {
            string iAlign;
            string Style;
            string CellCopy;
            // 
            iAlign = encodeEmptyText(Align, "left");
            // 
            if (((RowPointer % 2) 
                        > 0)) {
                Style = "ccAdminListRowOdd";
            }
            else {
                Style = "ccAdminListRowEven";
            }
            
            // 
            GetReport_Cell = ("\r\n" + ("<td valign=\"middle\" align=\"" 
                        + (iAlign + ("\" class=\"" 
                        + (Style + "\"")))));
            if ((Columns > 1)) {
                GetReport_Cell = (GetReport_Cell + (" colspan=\"" 
                            + (Columns + "\"")));
            }
            
            // 
            CellCopy = Copy;
            if ((CellCopy == "")) {
                CellCopy = " ";
            }
            
            return (GetReport_Cell + ("><span class=\"ccSmall\">" 
                        + (CellCopy + "</span></td>")));
        }
        
        // 
        // ==========================================================================================
        //    Report Cell Header
        //        ColumnPtr       :   0 based column number
        //        Title
        //        Width           :   if just a number, assumed to be px in style and an image is added
        //                        :   if 00px, image added with the numberic part
        //                        :   if not number, added to style as is
        //        align           :   style value
        //        ClassStyle      :   class
        //        RQS
        //        SortingState
        // ==========================================================================================
        // 
        private string GetReport_CellHeader(int ColumnPtr, string Title, string Width, string Align, string ClassStyle, string RefreshQueryString, SortingStateEnum SortingState) {
            // 
            //  See new GetReportOrderBy for the method to setup sorting links
            // 
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            string Style;
            string Copy;
            string QS;
            int WidthTest;
            string LinkTitle;
            // 
            if ((Title == "")) {
                Copy = " ";
            }
            else {
                Copy = genericController.vbReplace(Title, " ", " ");
                // Copy = "<nobr>" & Title & "</nobr>"
            }
            
            Style = "VERTICAL-ALIGN:bottom;";
            if ((Align == "")) {
                
            }
            else {
                Style = (Style + ("TEXT-ALIGN:" 
                            + (Align + ";")));
            }
            
            // 
            switch (SortingState) {
                case SortingStateEnum.SortableNotSet:
                    QS = genericController.ModifyQueryString(RefreshQueryString, "ColSort", SortingStateEnum.SortableSetAZ.ToString(), true);
                    QS = genericController.ModifyQueryString(QS, "ColPtr", ColumnPtr.ToString(), true);
                    Copy = ("<a href=\"?" 
                                + (QS + ("\" title=\"Sort A-Z\" class=\"ccAdminListCaption\">" 
                                + (Copy + "</a>"))));
                    break;
                case SortingStateEnum.SortableSetza:
                    QS = genericController.ModifyQueryString(RefreshQueryString, "ColSort", SortingStateEnum.SortableSetAZ.ToString(), true);
                    QS = genericController.ModifyQueryString(QS, "ColPtr", ColumnPtr.ToString(), true);
                    Copy = ("<a href=\"?" 
                                + (QS + ("\" title=\"Sort A-Z\" class=\"ccAdminListCaption\">" 
                                + (Copy + "<img src=\"/ccLib/images/arrowup.gif\" width=8 height=8 border=0></a>"))));
                    break;
                case SortingStateEnum.SortableSetAZ:
                    QS = genericController.ModifyQueryString(RefreshQueryString, "ColSort", SortingStateEnum.SortableSetza.ToString(), true);
                    QS = genericController.ModifyQueryString(QS, "ColPtr", ColumnPtr.ToString(), true);
                    Copy = ("<a href=\"?" 
                                + (QS + ("\" title=\"Sort Z-A\" class=\"ccAdminListCaption\">" 
                                + (Copy + "<img src=\"/ccLib/images/arrowdown.gif\" width=8 height=8 border=0></a>"))));
                    break;
            }
            // 
            if ((Width != "")) {
                WidthTest = genericController.EncodeInteger(Replace(Width, "px", "", vbTextCompare));
                if ((WidthTest != 0)) {
                    Style = (Style + ("width:" 
                                + (WidthTest + "px;")));
                    Copy = (Copy + ("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"" 
                                + (WidthTest + "\" height=1 border=0>")));
                }
                else {
                    Style = (Style + ("width:" 
                                + (Width + ";")));
                }
                
            }
            
            // 
            return ("\r\n" + ("<td style=\"" 
                        + (Style + ("\" class=\"" 
                        + (ClassStyle + ("\">" 
                        + (Copy + "</td>")))))));
            
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            this.handleLegacyClassError("GetReport_CellHeader");
        }
        
        // 
        // =============================================================================
        //    Get Sort Column Ptr
        // 
        //    returns the integer column ptr of the column last selected
        // =============================================================================
        // 
        public int GetReportSortColumnPtr(int DefaultSortColumnPtr) {
            string VarText;
            // 
            VarText = cpCore.docProperties.getText("ColPtr");
            GetReportSortColumnPtr = genericController.EncodeInteger(VarText);
            if (((GetReportSortColumnPtr == 0) 
                        && (VarText != "0"))) {
                GetReportSortColumnPtr = DefaultSortColumnPtr;
            }
            
        }
        
        // 
        // =============================================================================
        //    Get Sort Column Type
        // 
        //    returns the integer for the type of sorting last requested
        //        0 = nothing selected
        //        1 = sort A-Z
        //        2 = sort Z-A
        // 
        //    Orderby is generated by the selection of headers captions by the user
        //    It is up to the calling program to call GetReportOrderBy to get the orderby and use it in the query to generate the cells
        //    This call returns a comma delimited list of integers representing the columns to sort
        // =============================================================================
        // 
        public int GetReportSortType() {
            string VarText;
            // 
            VarText = cpCore.docProperties.getText("ColPtr");
            if (((EncodeInteger(VarText) != 0) 
                        || (VarText == "0"))) {
                // 
                //  A valid ColPtr was found
                // 
                GetReportSortType = cpCore.docProperties.getInteger("ColSort");
            }
            else {
                GetReportSortType = SortingStateEnum.SortableSetAZ;
            }
            
        }
        
        // 
        // =============================================================================
        //    Translate the old GetReport to the new GetReport2
        // =============================================================================
        // 
        public string GetReport(int RowCount, string[] ColCaption, string[] ColAlign, string[] ColWidth, string[,] Cells, int PageSize, int PageNumber, string PreTableCopy, string PostTableCopy, int DataRowCount, string ClassStyle) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            bool[] ColSortable;
            int ColCnt;
            int Ptr;
            // 
            ColCnt = UBound(Cells, 2);
            object ColSortable;
            for (Ptr = 0; (Ptr 
                        <= (ColCnt - 1)); Ptr++) {
                ColSortable[Ptr] = false;
            }
            
            // 
            GetReport = this.GetReport2(RowCount, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, ClassStyle, ColSortable, 0);
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            this.handleLegacyClassError("GetReport");
        }
        
        // 
        // =============================================================================
        //    Report
        // 
        //    This is a list report that you have to fill in all the cells and pass them in arrays.
        //    The column headers are always assumed to include the orderby options. they are linked. To get the correct orderby, the calling program
        //    has to call GetReport2orderby
        // 
        // 
        // =============================================================================
        // 
        public string GetReport2(int RowCount, string[] ColCaption, string[] ColAlign, string[] ColWidth, string[,] Cells, int PageSize, int PageNumber, string PreTableCopy, string PostTableCopy, int DataRowCount, string ClassStyle, bool[] ColSortable, int DefaultSortColumnPtr) {
            string result = String.Empty;
            try {
                string RQS;
                int RowBAse;
                stringBuilderLegacyController Content = new stringBuilderLegacyController();
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                int ColumnCount;
                int ColumnPtr;
                string ColumnWidth;
                int RowPointer;
                string WorkingQS;
                // 
                int PageCount;
                int PagePointer;
                int LinkCount;
                int ReportPageNumber;
                int ReportPageSize;
                string iClassStyle;
                int SortColPtr;
                int SortColType;
                // 
                ReportPageNumber = PageNumber;
                if ((ReportPageNumber == 0)) {
                    ReportPageNumber = 1;
                }
                
                ReportPageSize = PageSize;
                if ((ReportPageSize < 1)) {
                    ReportPageSize = 50;
                }
                
                // 
                iClassStyle = ClassStyle;
                if ((iClassStyle == "")) {
                    iClassStyle = "ccPanel";
                }
                
                // If IsArray(Cells) Then
                ColumnCount = UBound(Cells, 2);
                // End If
                RQS = cpCore.doc.refreshQueryString;
                // 
                SortColPtr = this.GetReportSortColumnPtr(DefaultSortColumnPtr);
                SortColType = this.GetReportSortType();
                // 
                //  ----- Start the table
                // 
                Content.Add(StartTable(3, 1, 0));
                // 
                //  ----- Header
                // 
                Content.Add(("\r\n" + "<tr>"));
                Content.Add(this.GetReport_CellHeader(0, "Row", "50", "Right", "ccAdminListCaption", RQS, SortingStateEnum.NotSortable));
                for (ColumnPtr = 0; (ColumnPtr 
                            <= (ColumnCount - 1)); ColumnPtr++) {
                    ColumnWidth = ColWidth(ColumnPtr);
                    if (!ColSortable(ColumnPtr)) {
                        // 
                        //  not sortable column
                        // 
                        Content.Add(this.GetReport_CellHeader(ColumnPtr, ColCaption(ColumnPtr), ColumnWidth, ColAlign(ColumnPtr), "ccAdminListCaption", RQS, SortingStateEnum.NotSortable));
                    }
                    else if ((ColumnPtr == SortColPtr)) {
                        // 
                        //  This is the current sort column
                        // 
                        Content.Add(this.GetReport_CellHeader(ColumnPtr, ColCaption(ColumnPtr), ColumnWidth, ColAlign(ColumnPtr), "ccAdminListCaption", RQS, DirectCast, SortColType, SortingStateEnum));
                    }
                    else {
                        // 
                        //  Column is sortable, but not selected
                        // 
                        Content.Add(this.GetReport_CellHeader(ColumnPtr, ColCaption(ColumnPtr), ColumnWidth, ColAlign(ColumnPtr), "ccAdminListCaption", RQS, SortingStateEnum.SortableNotSet));
                    }
                    
                    // If ColumnPtr = SortColPtr Then
                    //     '
                    //     ' This column is currently the active sort
                    //     '
                    //     Call Content.Add(GetReport_CellHeader(ColumnPtr, ColCaption(ColumnPtr), ColumnWidth, ColAlign(ColumnPtr), "ccAdminListCaption", RQS, SortColType))
                    // Else
                    //     Call Content.Add(GetReport_CellHeader(ColumnPtr, ColCaption(ColumnPtr), ColumnWidth, ColAlign(ColumnPtr), "ccAdminListCaption", RQS, SortingStateEnum.SortableNotSet))
                    // End If
                }
                
                Content.Add(("\r\n" + "</tr>"));
                // 
                //  ----- Data
                // 
                if ((RowCount == 0)) {
                    Content.Add(("\r\n" + "<tr>"));
                    Content.Add(this.GetReport_Cell(((RowBAse + RowPointer)).ToString(), "right", 1, RowPointer));
                    Content.Add(this.GetReport_Cell("-- End --", "left", ColumnCount, 0));
                    Content.Add(("\r\n" + "</tr>"));
                }
                else {
                    RowBAse = ((ReportPageSize 
                                * (ReportPageNumber - 1)) 
                                + 1);
                    for (RowPointer = 0; (RowPointer 
                                <= (RowCount - 1)); RowPointer++) {
                        Content.Add(("\r\n" + "<tr>"));
                        Content.Add(this.GetReport_Cell(((RowBAse + RowPointer)).ToString(), "right", 1, RowPointer));
                        for (ColumnPtr = 0; (ColumnPtr 
                                    <= (ColumnCount - 1)); ColumnPtr++) {
                            Content.Add(this.GetReport_Cell(Cells(RowPointer, ColumnPtr), ColAlign(ColumnPtr), 1, RowPointer));
                        }
                        
                        Content.Add(("\r\n" + "</tr>"));
                    }
                    
                }
                
                // 
                //  ----- End Table
                // 
                Content.Add(kmaEndTable);
                result = (result + Content.Text);
                // 
                //  ----- Post Table copy
                // 
                if (((DataRowCount / ReportPageSize) 
                            != Int((DataRowCount / ReportPageSize)))) {
                    PageCount = int.Parse(((DataRowCount / ReportPageSize) 
                                    + 0.5));
                }
                else {
                    PageCount = int.Parse((DataRowCount / ReportPageSize));
                }
                
                if ((PageCount > 1)) {
                    result = (result + ("<br>Page " 
                                + (ReportPageNumber + (" (Row " 
                                + (RowBAse + (" of " 
                                + (DataRowCount + ")")))))));
                    if ((PageCount > 20)) {
                        PagePointer = (ReportPageNumber - 10);
                    }
                    
                    if ((PagePointer < 1)) {
                        PagePointer = 1;
                    }
                    
                    if ((PageCount > 1)) {
                        result = (result + "<br>Go to Page ");
                        if ((PagePointer != 1)) {
                            WorkingQS = cpCore.doc.refreshQueryString;
                            WorkingQS = genericController.ModifyQueryString(WorkingQS, "GotoPage", "1", true);
                            result = (result + ("<a href=\"" 
                                        + (cpCore.webServer.requestPage + ("?" 
                                        + (WorkingQS + "\">1</A>... ")))));
                        }
                        
                        WorkingQS = cpCore.doc.refreshQueryString;
                        WorkingQS = genericController.ModifyQueryString(WorkingQS, RequestNamePageSize, ReportPageSize.ToString(), true);
                        while (((PagePointer <= PageCount) 
                                    && (LinkCount < 20))) {
                            if ((PagePointer == ReportPageNumber)) {
                                result = (result 
                                            + (PagePointer + " "));
                            }
                            else {
                                WorkingQS = genericController.ModifyQueryString(WorkingQS, RequestNamePageNumber, PagePointer.ToString(), true);
                                result = (result + ("<a href=\"" 
                                            + (cpCore.webServer.requestPage + ("?" 
                                            + (WorkingQS + ("\">" 
                                            + (PagePointer + "</A> ")))))));
                            }
                            
                            PagePointer = (PagePointer + 1);
                            LinkCount = (LinkCount + 1);
                        }
                        
                        if ((PagePointer < PageCount)) {
                            WorkingQS = genericController.ModifyQueryString(WorkingQS, RequestNamePageNumber, PageCount.ToString(), true);
                            result = (result + ("...<a href=\"" 
                                        + (cpCore.webServer.requestPage + ("?" 
                                        + (WorkingQS + ("\">" 
                                        + (PageCount + "</A> ")))))));
                        }
                        
                        if ((ReportPageNumber < PageCount)) {
                            WorkingQS = genericController.ModifyQueryString(WorkingQS, RequestNamePageNumber, ((ReportPageNumber + 1)).ToString(), true);
                            result = (result + ("...<a href=\"" 
                                        + (cpCore.webServer.requestPage + ("?" 
                                        + (WorkingQS + "\">next</A> ")))));
                        }
                        
                        result = (result + "<br> ");
                    }
                    
                }
                
                // 
                result = ("" 
                            + (PreTableCopy + ("<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr><td style=\"padding:10px;\">" 
                            + (result + ("</td></tr></table>" 
                            + (PostTableCopy + ""))))));
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
            return result;
        }
        
        // 
        // ========================================================================
        //  Get the Normal Edit Button Bar String
        // 
        //    used on Normal Edit and others
        // ========================================================================
        // 
        public string GetEditButtonBar(int MenuDepth, bool AllowDelete, bool AllowCancel, bool allowSave, bool AllowSpellCheck, bool AllowPublish, bool AllowAbort, bool AllowSubmit, bool AllowApprove) {
            return this.GetEditButtonBar2(MenuDepth, AllowDelete, AllowCancel, allowSave, AllowSpellCheck, AllowPublish, AllowAbort, AllowSubmit, AllowApprove, false, false, false, false, false, false, false);
        }
        
        // 
        // ========================================================================
        //  Get the Normal Edit Button Bar String
        // 
        //    used on Normal Edit and others
        // ========================================================================
        // 
        public string GetFormBodyAdminOnly() {
            return "<div class=\"ccError\" style=\"margin:10px;padding:10px;background-color:white;\">This page requires admi" +
            "nistrator permissions.</div>";
        }
        
        // 
        // ========================================================================
        //    Builds a single entry in the ReportFilter at the bottom of the page
        // ========================================================================
        // 
        public string GetReportFilterRow(string FormInput, string Caption) {
            // 
            return ("" + ("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">" + ("<tr><td width=\"200\" align=\"right\" class=RowInput>" 
                        + (FormInput + ("</td>" + ("<td width=\"100%\" align=\"left\" class=RowCaption> " 
                        + (Caption + ("</td></tr>" + "</table>"))))))));
        }
        
        // 
        // =============================================================================
        //    Builds the panel around the filters at the bottom of the report
        // =============================================================================
        // 
        public string GetReportFilter(string Title, string Body) {
            string result = "";
            result = (result + "<div class=\"ccReportFilter\">");
            result = (result + ("<div class=\"Title\">" 
                        + (Title + "</div>")));
            result = (result + ("<div class=\"Body\">" 
                        + (Body + "</div>")));
            result = (result + "</div>");
            return result;
        }
        
        // 
        // ===========================================================================
        // '' <summary>
        // '' handle legacy errors for this class, v1
        // '' </summary>
        // '' <param name="MethodName"></param>
        // '' <remarks></remarks>
        private void handleLegacyClassError(string MethodName) {
            // 
            throw new Exception(("unexpected exception in method [" 
                            + (MethodName + "]")));
        }
    }
}