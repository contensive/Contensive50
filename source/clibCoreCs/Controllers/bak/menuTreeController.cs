


// 

namespace Controllers {
    
    public class menuTreeController {
        
        // 
        //  ----- Each menu item has an MenuEntry
        // 
        private struct MenuEntryType {
            
            private string Caption;
            
            //  What is displayed for this entry (does not need to be unique)
            private string Name;
            
            //  Unique name for this entry
            private string ParentName;
            
            //  Unique name of the parent entry
            private string Link;
            
            //  URL
            private string Image;
            
            //  Image
            private string ImageOver;
            
            //  Image Over
            private string ImageOpen;
            
            //  Image when menu is open
            // StyleSheet As String        ' Stylesheet
            // StyleSheetHover As String   ' Hover Stylesheet
            private bool NewWindow;
            
            //  True opens link in a new window
            private string OnClick;
        }
        
        // 
        //  ----- A collection of Navigator Entries that have a single function (unique name)
        // 
        // private structure MenuType
        //     Name As String              ' Unique name for this menu
        //     Link As String              ' The linked text at the top of this menu
        //     LinkLabel As String         ' The linked text at the top of this menu
        //     CreateDate As Date          ' DateTime when this panel was created
        //     PositionX as integer           ' pixel position on the screen, (default -1)
        //     PositionY as integer           ' pixel position on the screen, (default -1)
        //     StyleSheet As String        ' Stylesheet to put on the whole menu
        //     StyleSheetHover As String   ' Hover Stylesheet to put on the whole menu
        //     EntryCount as integer          ' Number of Entries in this panel
        //     EntrySize as integer           ' Number of Entries in this panel
        //     Entries() As MenuEntryType  ' The Navigator Entries
        //     End structure
        // 
        //  ----- Local storage
        // 
        private coreClass cpCore;
        
        private string iMenuFilePath;
        
        // 
        //  ----- Menu Entry storage
        // 
        private int iEntryCount;
        
        //  Count of Menus in the object
        private int iEntrySize;
        
        private MenuEntryType[] iEntry;
        
        // 
        //  Private iDQMCount as integer           ' Count of Default Menus for this instance
        //  Private iDQMCLosed As Boolean       ' true if the menu has been closed
        // 
        private int iTreeCount;
        
        //  Count of Tree Menus for this instance
        private string iMenuCloseString;
        
        //  String returned for closing menus
        // 
        private string UsedEntries;
        
        //  String of EntryNames that have been used (for unique test)
        private keyPtrController EntryIndexName;
        
        //  Private EntryIndexID As keyPtrIndex8Class
        // 
        //  ----- RollOverFlyout storage
        // 
        // Private MenuFlyoutCount as integer           ' Count of Default Menus for this instance
        private string MenuFlyoutNamePrefix;
        
        //  Random prefix added to element IDs to avoid namespace collision
        private string MenuFlyoutIcon_Local;
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' constructor
        // '' </summary>
        // '' <param name="cpCore"></param>
        // '' <remarks></remarks>
        public menuTreeController(coreClass cpCore) {
            this.cpCore = cpCore;
            EntryIndexName = new keyPtrController();
            Randomize();
            MenuFlyoutNamePrefix = ("id" + Int((9999 * Rnd())).ToString());
        }
        
        // 
        // ===============================================================================
        //    Create a new Menu Entry
        // ===============================================================================
        // 
        public void AddEntry(
                    string EntryName, 
                    string ParentiEntryName, 
                    void =, 
                    void , 
                    string ImageLink, 
                    void =, 
                    void , 
                    string ImageOverLink, 
                    void =, 
                    void , 
                    string Link, 
                    void =, 
                    void , 
                    string Caption, 
                    void =, 
                    void , 
                    string OnClickJavascript, 
                    void =, 
                    void , 
                    string Ignore1, 
                    void =, 
                    void , 
                    string ImageOpenLink, 
                    void =, 
                    void , 
                    bool NewWindow, 
                    void =, 
                    void False) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // 
            int MenuEntrySize;
            string iEntryName;
            string UcaseEntryName;
            bool iNewWindow;
            // 
            iEntryName = genericController.vbReplace(encodeEmptyText(EntryName, ""), ",", " ");
            UcaseEntryName = genericController.vbUCase(iEntryName);
            // 
            if (((iEntryName != "") 
                        && (((UsedEntries + ",").IndexOf(("," 
                            + (UcaseEntryName + ",")), 0, System.StringComparison.Ordinal) + 1) 
                        == 0))) {
                UsedEntries = (UsedEntries + ("," + UcaseEntryName));
                if ((iEntryCount >= iEntrySize)) {
                    iEntrySize = (iEntrySize + 10);
                    object Preserve;
                    iEntry[iEntrySize];
                }
                
                // With...
                Image = "";
                encodeEmptyText(Link, "").Image = encodeEmptyText(OnClickJavascript, "");
                iEntry[iEntryCount].Link = encodeEmptyText(OnClickJavascript, "");
                // 
                //  No image, must have a caption
                // 
                Caption = encodeEmptyText(Caption, iEntryName);
                // 
                //  Image present, caption is extra
                // 
                Caption = encodeEmptyText(Caption, "");
                encodeEmptyText(ImageOpenLink, "").NewWindow = NewWindow;
                encodeEmptyText(ImageOverLink, "").ImageOpen = NewWindow;
                genericController.vbUCase(encodeEmptyText(ParentiEntryName, "")).ImageOver = NewWindow;
                UcaseEntryName.ParentName = NewWindow;
                Name = NewWindow;
            }
            
            EntryIndexName.setPtr(UcaseEntryName, iEntryCount);
            iEntryCount = (iEntryCount + 1);
        }
        
        void ErrorTrap(void :, void Throw, void New, void Exception, void Unexpected exception) {
            //  Call HandleClassError("AddEntry", Err.Number, Err.Source, Err.Description)
        }
        
        //         '
        //         '===============================================================================
        //         '   Returns the menu specified, if it is in local storage
        //         '
        //         '   It also creates the menu data in a close string that is returned in GetMenuClose.
        //         '   It must be done there so the link buttons height can be calculated.
        //         '===============================================================================
        //         '
        //         Public Function GetMenu(ByVal MenuName As String, Optional ByVal StyleSheetPrefix As String = "") As String
        //             On Error GoTo ErrorTrap
        //             GetMenu = GetTree(MenuName, "", encodeEmptyText(StyleSheetPrefix, "ccTree"))
        //             Exit Function
        //             '
        //             Dim Link As String
        //             Dim EntryPointer As Integer
        //             Dim UcaseMenuName As String
        //             Dim LocalStyleSheetPrefix As String
        //             '
        //             ' ----- Get the menu pointer
        //             '
        //             If iEntryCount > 0 Then
        //                 UcaseMenuName = MenuName
        //                 LocalStyleSheetPrefix = encodeEmptyText(StyleSheetPrefix, "ccTree")
        //                 For EntryPointer = 0 To iEntryCount - 1
        //                     If iEntry(EntryPointer).Name = UcaseMenuName Then
        //                         Exit For
        //                     End If
        //                 Next
        //                 If EntryPointer < iEntryCount Then
        //                     '
        //                     ' ----- Build the linked -button-
        //                     '
        //                     Link = iEntry(EntryPointer).Link
        //                     If Link = "" Then
        //                         Link = "javascript: ;"
        //                     End If
        //                     '
        //                     GetMenu = vbCrLf _
        //                         & "<DIV id=""tree"" class=""" & LocalStyleSheetPrefix & "Root"" ></DIV>" & vbCrLf
        //                     '
        //                     '   Find the Menu Entry, and create the top element here
        //                     '
        //                     For EntryPointer = 0 To iEntryCount - 1
        //                         With iEntry(EntryPointer)
        //                             If .Name = UcaseMenuName Then
        //                                 'iMenuCloseString = iMenuCloseString
        //                                 GetMenu = GetMenu _
        //                                     & "<SCRIPT Language=""JavaScript"" type=""text/javascript"">" & vbCrLf _
        //                                     & "var DivLeft,DivTop,ElementObject; " & vbCrLf _
        //                                     & "DivTop = -18; " & vbCrLf _
        //                                     & "DivLeft = 0; " & vbCrLf _
        //                                     & "for (ElementObject=tree;  ElementObject.tagName!='BODY'; ElementObject = ElementObject.offsetParent) { " & vbCrLf _
        //                                     & "    DivTop = DivTop+ElementObject.offsetTop; " & vbCrLf _
        //                                     & "    DivLeft = DivLeft+ElementObject.offsetLeft; " & vbCrLf _
        //                                     & "    } " & vbCrLf _
        //                                     & "var menuBase = new  menuObject(DivTop,DivLeft); " & vbCrLf _
        //                                     & "menuBase.s[0] = new so(0,'" & .Caption & "','" & .Link & "','_blank',''); " & vbCrLf _
        //                                     & GetMenuTreeBranch(.Name, "menuBase.s[0]", "," & EntryPointer) _
        //                                     & "</SCRIPT>" & vbCrLf
        //                                 ' & "<SCRIPT LANGUAGE=""JavaScript"" src=""/ccLib/ClientSide/tree30.js""></SCRIPT>" & vbCrLf
        //                                 Exit For
        //                             End If
        //                         End With
        //                     Next
        //                     '
        //                     ' ----- Add what is needed to the close string, be carefull of the order
        //                     '
        //                     '
        //                     ' increment the menu count
        //                     '
        //                     iTreeCount = iTreeCount + 1
        //                 End If
        //             End If
        //             Exit Function
        //             '
        // ErrorTrap:
        //             throw (New Exception("Unexpected exception")) ' Call HandleClassError("GetMenu", Err.Number, Err.Source, Err.Description)
        //         End Function
        // 
        // ===============================================================================
        //    Gets the Menu Branch for the Tree Menu
        // ===============================================================================
        // 
        private string GetMenuTreeBranch(string ParentName, string JSObject, string UsedEntries) {
            string result = "";
            try {
                int EntryPointer;
                string iUsedEntries;
                string JSChildObject;
                int SubMenuCount;
                // 
                iUsedEntries = UsedEntries;
                SubMenuCount = 0;
                for (EntryPointer = 0; (EntryPointer 
                            <= (iEntryCount - 1)); EntryPointer++) {
                    // With...
                    if ((iEntry[EntryPointer].ParentName == ParentName)) {
                        if ((((iUsedEntries + ",").IndexOf(("," 
                                        + (EntryPointer + ",")), 0) + 1) 
                                    == 0)) {
                            JSChildObject = (JSObject + (".s[" 
                                        + (SubMenuCount + "]")));
                            iUsedEntries = (iUsedEntries + ("," + EntryPointer));
                            result = (result 
                                        + (JSChildObject + (" = new so(0,\'" 
                                        + (iEntry[EntryPointer].Caption + ("\',\'" 
                                        + (iEntry[EntryPointer].Link + ("\',\'_blank\',\'\'); " + ("\r\n" + this.GetMenuTreeBranch(iEntry[EntryPointer].Name, JSChildObject, iUsedEntries)))))))));
                            SubMenuCount = (SubMenuCount + 1);
                        }
                        
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
            return result;
        }
        
        // 
        //  Temp log file solution
        // 
        private void AppendLog(string Message) {
            logController.appendLog(cpCore, Message, "menuing");
        }
        
        // 
        // ===============================================================================
        //    Returns the menu specified, if it is in local storage
        // 
        //    It also creates the menu data in a close string that is returned in GetMenuClose.
        //    It must be done there so the link buttons height can be calculated.
        //    Uses a simple UL/Stylesheet method, returning to the server with every click
        // ===============================================================================
        // 
        private string GetMenuTreeList(string MenuName, string OpenNodesList) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            int EntryPointer;
            string UcaseMenuName;
            // 
            //  ----- Get the menu pointer
            // 
            if ((iEntryCount > 0)) {
                UcaseMenuName = genericController.vbUCase(MenuName);
                EntryPointer = EntryIndexName.getPtr(UcaseMenuName);
                return this.GetMenuTreeListBranch2(EntryPointer, "", OpenNodesList);
                
            }
            
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
        ErrorTrap:
            throw new Exception("Unexpected exception");
        }
        
        // 
        // ===============================================================================
        //    Gets the Menu Branch for the Tree Menu
        // ===============================================================================
        // 
        private string GetMenuTreeListBranch2(int NodePointer, string UsedEntriesList, string OpenNodesList) {
            string result = "";
            try {
                // 
                string Link;
                int EntryPointer;
                string UcaseNodeName;
                string Image;
                string Caption;
                // 
                if ((iEntryCount > 0)) {
                    // 
                    //  Output this node
                    // 
                    if ((genericController.vbInstr(1, ("," 
                                    + (NodePointer.ToString() + ",")), ("," 
                                    + (UsedEntriesList + ","))) == 0)) {
                        result = (result + "<ul Style=\"list-style-type: none; margin-left: 20px\">");
                        Caption = iEntry[NodePointer].Caption;
                        Link = genericController.encodeHTML(iEntry[NodePointer].Link);
                        if ((Link != "")) {
                            Caption = ("<A TARGET=\"_blank\" HREF=\"" 
                                        + (Link + ("\">" 
                                        + (Caption + "</A>"))));
                        }
                        
                        // 
                        if ((genericController.vbInstr(1, ("," 
                                        + (OpenNodesList + ",")), ("," 
                                        + (NodePointer.ToString() + ","))) == 0)) {
                            // 
                            //  The branch is closed
                            // 
                            Image = iEntry[NodePointer].Image;
                            result = (result + ("<li><A HREF=\"?OpenNodesList=" 
                                        + (OpenNodesList + ("&OpenNode=" 
                                        + (NodePointer + ("\"><IMG SRC=\"" 
                                        + (Image + ("\" HEIGHT=\"18\" WIDTH=\"18\" BORDER=0 ALT=\"Open Folder\"></A> " 
                                        + (Caption + "</li>")))))))));
                        }
                        else {
                            // 
                            //  The branch is open
                            // 
                            Image = iEntry[NodePointer].ImageOpen;
                            if ((Image == "")) {
                                Image = iEntry[NodePointer].Image;
                            }
                            
                            result = (result + ("<li>" + ("<A HREF=\"?OpenNodesList=" 
                                        + (OpenNodesList + ("&CloseNode=" 
                                        + (NodePointer + ("\">" + ("<IMG SRC=\"" 
                                        + (Image + ("\" HEIGHT=\"18\" WIDTH=\"18\" BORDER=0 ALT=\"Close Folder\">" + ("</A> " 
                                        + (Caption + "</li>"))))))))))));
                            UcaseNodeName = genericController.vbUCase(iEntry[NodePointer].Name);
                            for (EntryPointer = 0; (EntryPointer 
                                        <= (iEntryCount - 1)); EntryPointer++) {
                                if ((iEntry[EntryPointer].ParentName == UcaseNodeName)) {
                                    result = (result + this.GetMenuTreeListBranch2(EntryPointer, (UsedEntriesList + ("," + NodePointer)), OpenNodesList));
                                }
                                
                            }
                            
                        }
                        
                        result = (result + ("</ul>" + "\r\n"));
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
            return result;
        }
        
        //         '
        //         '===============================================================================
        //         '   Returns the menu specified, if it is in local storage
        //         '
        //         '   It also creates the menu data in a close string that is returned in GetTreeClose.
        //         '   It must be done there so the link buttons height can be calculated.
        //         '===============================================================================
        //         '
        //         Public Function GetTree(ByVal MenuName As String, ByVal OpenMenuName As String, Optional ByVal StyleSheetPrefix As String = "") As String
        //             On Error GoTo ErrorTrap
        //             '
        //             Dim Link As String
        //             Dim EntryPointer As Integer
        //             Dim UcaseMenuName As String
        //             Dim RootFound As Boolean
        //             Dim UsedEntries As String
        //             Dim Caption As String
        //             Dim JSString As String
        //             '
        //             ' ----- Get the menu pointer
        //             '
        //             If iEntryCount > 0 Then
        //                 UcaseMenuName = genericController.vbUCase(MenuName)
        //                 If StyleSheetPrefix = "" Then
        //                     StyleSheetPrefix = "ccTree"
        //                 End If
        //                 If True Then
        //                     '
        //                     ' ----- Build the linked -button-
        //                     '
        //                     Link = iEntry(EntryPointer).Link
        //                     If Link = "" Then
        //                         Link = "javascript: ;"
        //                     End If
        //                     '
        //                     '   Find the Menu Entry, and create the top element here
        //                     '
        //                     UsedEntries = ""
        //                     For EntryPointer = 0 To iEntryCount - 1
        //                         With iEntry(EntryPointer)
        //                             If .Name = UcaseMenuName Then
        //                                 Caption = .Caption
        //                                 If .Link <> "" Then
        //                                     Caption = "<a href=""" & cpcore.html.html_EncodeHTML(.Link) & """>" & Caption & "</a>"
        //                                 End If
        //                                 UsedEntries = UsedEntries & "," & CStr(EntryPointer)
        //                                 GetTree = "" _
        //                                     & vbCrLf & "<ul class=mktree id=tree" & iTreeCount & ">" & vbCrLf _
        //                                     & vbCrLf & " <li id=""" & .Name & """><span class=mkc>" & Caption & "</span>" _
        //                                     & vbCrLf & " <ul>" & vbCrLf _
        //                                     & GetMKTreeBranch(UcaseMenuName, UsedEntries, 2) _
        //                                     & vbCrLf & " </ul>" & vbCrLf _
        //                                     & vbCrLf & "</li></ul>" & vbCrLf
        //                                 Exit For
        //                             End If
        //                         End With
        //                     Next
        //                     If UsedEntries = "" Then
        //                         GetTree = "" _
        //                             & vbCrLf & "<ul class=mktree id=tree" & iTreeCount & ">" _
        //                             & GetMKTreeBranch(UcaseMenuName, UsedEntries, 1) _
        //                             & vbCrLf & "</ul>" & vbCrLf
        //                     End If
        //                     '
        //                     'Call cmc.main_AddStylesheetLink("/ccLib/mktree/mktree.css")
        //                     'Call cmc.main_AddHeadScriptLink("/ccLib/mktree/mktree.js", "mktree")
        //                     'Call cmc.main_AddOnLoadJavascript("convertTrees();")
        //                     GetTree = "" _
        //                         & vbCrLf & "<link rel=stylesheet href=/ccLib/mktree/mktree.css type=text/css>" _
        //                         & vbCrLf & "<script type=""text/javascript"" src=/ccLib/mktree/mktree.js></script>" _
        //                         & GetTree
        //                     GetTree = GetTree & "<script type=""text/javascript"">convertTrees();"
        //                     If OpenMenuName <> "" Then
        //                         JSString = genericController.vbUCase(OpenMenuName)
        //                         JSString = genericController.vbReplace(JSString, "\", "\\")
        //                         JSString = genericController.vbReplace(JSString, vbCrLf, "\n")
        //                         JSString = genericController.vbReplace(JSString, "'", "\'")
        //                         'Call cmc.main_AddOnLoadJavascript("expandToItem('tree" & iTreeCount & "','" & JSString & "');")
        //                         GetTree = GetTree & "expandToItem('tree" & iTreeCount & "','" & JSString & "');"
        //                     End If
        //                     GetTree = GetTree & "</script>"
        //                     '
        //                     ' increment the menu count
        //                     '
        //                     iTreeCount = iTreeCount + 1
        //                 End If
        //             End If
        //             Exit Function
        //             '
        // ErrorTrap:
        //             throw (New Exception("Unexpected exception")) ' Call HandleClassError("GetTree", Err.Number, Err.Source, Err.Description)
        //         End Function
        //         '
        //         '===============================================================================
        //         '   Gets the Menu Branch for the Tree Menu
        //         '===============================================================================
        //         '
        //         Private Function GetMKTreeBranch(ByVal ParentName As String, ByVal UsedEntries As String, ByVal Depth As Integer) As String
        //             On Error GoTo ErrorTrap
        //             '
        //             Dim EntryPointer As Integer
        //             Dim iUsedEntries As String
        //             Dim SubMenuCount As Integer
        //             Dim ChildMenu As String
        //             Dim Caption As String
        //             '
        //             iUsedEntries = UsedEntries
        //             SubMenuCount = 0
        //             For EntryPointer = 0 To iEntryCount - 1
        //                 With iEntry(EntryPointer)
        //                     If .ParentName = ParentName Then
        //                         If (InStr(1, iUsedEntries & ",", "," & EntryPointer & ",") = 0) Then
        //                             Caption = .Caption
        //                             If .OnClick <> "" And .Link <> "" Then
        //                                 Caption = "<a href=""" & cpcore.html.html_EncodeHTML(.Link) & """ onClick=""" & .OnClick & """>" & Caption & "</a>"
        //                             ElseIf .OnClick <> "" Then
        //                                 Caption = "<a href=""#"" onClick=""" & .OnClick & """>" & Caption & "</a>"
        //                             ElseIf .Link <> "" Then
        //                                 Caption = "<a href=""" & cpcore.html.html_EncodeHTML(.Link) & """>" & Caption & "</a>"
        //                             Else
        //                                 Caption = Caption
        //                             End If
        //                             iUsedEntries = iUsedEntries & "," & EntryPointer
        //                             ChildMenu = GetMKTreeBranch(.Name, iUsedEntries, Depth + 1)
        //                             If newmode Then
        //                                 If ChildMenu = "" Then
        //                                     GetMKTreeBranch = GetMKTreeBranch _
        //                                         & vbCrLf & New String(" "c, Depth) & "<li class=mklb id=""" & .Name & """ >" _
        //                                         & "<div class=""mkd"">" _
        //                                         & "<span class=mkb> </span>" _
        //                                         & "</div>" _
        //                                         & Caption _
        //                                         & "</li>"
        //                                 Else
        //                                     '
        //                                     ' 3/18/2010 changes to keep firefox from blocking clicks
        //                                     '
        //                                     GetMKTreeBranch = GetMKTreeBranch _
        //                                         & vbCrLf & New String(" "c, Depth) & "<li class=""mklc"" id=""" & .Name & """ >" _
        //                                         & "<div class=""mkd"" >" _
        //                                         & "<span class=mkb onclick=""mkClick(this)""> </span>" _
        //                                         & "</div>" _
        //                                         & Caption _
        //                                         & vbCrLf & New String(" "c, Depth + 1) & "<ul>" _
        //                                         & ChildMenu _
        //                                         & vbCrLf & New String(" "c, Depth + 1) & "</ul>" _
        //                                         & "</li>"
        //                                 End If
        //                             Else
        //                                 If ChildMenu <> "" Then
        //                                     ChildMenu = "" _
        //                                         & vbCrLf & New String(" "c, Depth + 1) & "<ul>" _
        //                                         & ChildMenu _
        //                                         & vbCrLf & New String(" "c, Depth + 1) & "</ul>" _
        //                                         & ""
        //                                 End If
        //                                 GetMKTreeBranch = GetMKTreeBranch _
        //                                     & vbCrLf & New String(" "c, Depth) & "<li class=mklc id=""" & .Name & """>" _
        //                                     & Caption _
        //                                     & ChildMenu _
        //                                     & vbCrLf & New String(" "c, Depth) & "</li>"
        //                             End If
        //                             SubMenuCount = SubMenuCount + 1
        //                         End If
        //                     End If
        //                 End With
        //             Next
        //             '
        //             Exit Function
        //             '
        // ErrorTrap:
        //             throw (New Exception("Unexpected exception")) ' Call HandleClassError("GetMKTreeBranch", Err.Number, Err.Source, Err.Description)
        //         End Function
        // 
        // ========================================================================
        // '' <summary>
        // '' handle legacy errors in the is class
        // '' </summary>
        // '' <param name="MethodName"></param>
        // '' <param name="ErrNumber"></param>
        // '' <param name="ErrSource"></param>
        // '' <param name="ErrDescription"></param>
        // '' <remarks></remarks>
        private void handleLegacyClassError(string MethodName, int ErrNumber, string ErrSource, string ErrDescription) {
            // 
            throw new Exception(("unexpected exception in method [" 
                            + (MethodName + ("], errDescription [" 
                            + (ErrDescription + "]")))));
        }
    }
}