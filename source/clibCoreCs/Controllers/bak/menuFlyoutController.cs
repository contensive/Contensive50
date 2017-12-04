

using Controllers;

// 

namespace Controllers {
    
    public class menuFlyoutController {
        
        // 
        //  ----- Each menu item has an MenuEntry
        // 
        private struct MenuEntryType {
            
            private string Caption;
            
            //  What is displayed for this entry (does not need to be unique)
            private string CaptionImage;
            
            //  If present, this is an image in front of the caption
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
            private bool NewWindow;
        }
        
        // 
        //  ----- Local storage
        // 
        private string iMenuFilePath;
        
        // 
        //  ----- Menu Entry storage
        // 
        private int iEntryCount;
        
        //  Count of Menus in the object
        private int iEntrySize;
        
        private MenuEntryType[] iEntry;
        
        // 
        private int iTreeCount;
        
        //  Count of Tree Menus for this instance
        private string iMenuCloseString;
        
        //  String returned for closing menus
        // 
        private string UsedEntries;
        
        //  String of EntryNames that have been used (for unique test)
        private keyPtrController EntryIndexName;
        
        // Private EntryIndexID As keyPtrIndex8Class
        // 
        //  ----- RollOverFlyout storage
        // 
        private string MenuFlyoutNamePrefix;
        
        //  Random prefix added to element IDs to avoid namespace collision
        private string MenuFlyoutIcon_Local;
        
        //  string used to mark a button that has a non-hover flyout
        private coreClass cpCore;
        
        // 
        // ==================================================================================================
        // '' <summary>
        // '' constructor
        // '' </summary>
        // '' <remarks></remarks>
        public menuFlyoutController(coreClass cpCore) {
            this.cpCore = cpCore;
            // 
            EntryIndexName = new keyPtrController();
            Randomize();
            MenuFlyoutNamePrefix = ("id" + Int((9999 * Rnd())).ToString());
            MenuFlyoutIcon_Local = " »";
        }
        
        // 
        // ========================================================================
        //  ----- main_Get the menu link for the menu name specified
        // ========================================================================
        // 
        public string getMenu(string menuName, int MenuStyle, string StyleSheetPrefix, void =, void ) {
            string returnHtml = "";
            // Warning!!! Optional parameters not supported
            try {
                string MenuFlyoutIcon;
                const object DefaultIcon = "»";
                MenuFlyoutIcon = cpCore.siteProperties.getText("MenuFlyoutIcon", DefaultIcon);
                if ((MenuFlyoutIcon != DefaultIcon)) {
                    MenuFlyoutIcon = MenuFlyoutIcon;
                }
                
                switch (MenuStyle) {
                    case 11:
                        returnHtml = this.getMenuType(menuName, false, 3, encodeEmptyText(StyleSheetPrefix, ""));
                        break;
                    case 10:
                        returnHtml = this.getMenuType(menuName, false, 2, encodeEmptyText(StyleSheetPrefix, ""));
                        break;
                    case 9:
                        returnHtml = this.getMenuType(menuName, false, 1, encodeEmptyText(StyleSheetPrefix, ""));
                        break;
                    case 8:
                        returnHtml = this.getMenuType(menuName, false, 0, encodeEmptyText(StyleSheetPrefix, ""));
                        break;
                    case 7:
                        returnHtml = this.getMenuType(menuName, true, 3, encodeEmptyText(StyleSheetPrefix, ""));
                        break;
                    case 6:
                        returnHtml = this.getMenuType(menuName, true, 2, encodeEmptyText(StyleSheetPrefix, ""));
                        break;
                    case 5:
                        returnHtml = this.getMenuType(menuName, true, 1, encodeEmptyText(StyleSheetPrefix, ""));
                        break;
                    default:
                        returnHtml = this.getMenuType(menuName, true, 0, encodeEmptyText(StyleSheetPrefix, ""));
                        break;
                }
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnHtml;
        }
        
        // 
        // ===============================================================================
        //    Returns the menu specified, if it is in local storage
        // ===============================================================================
        // 
        public string getMenuType(string MenuName, bool ClickToOpen, int Direction, string StyleSheetPrefix, void =, void ) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // Warning!!! Optional parameters not supported
            // 
            string MenuSource;
            int MenuPointer;
            int MenuStyle;
            // 
            //  ----- Search local storage for this MenuName
            // 
            if (!ClickToOpen) {
                switch (Direction) {
                    case 1:
                        MenuStyle = MenuStyleHoverRight;
                        break;
                    case 2:
                        MenuStyle = MenuStyleHoverUp;
                        break;
                    case 3:
                        MenuStyle = MenuStyleHoverLeft;
                        break;
                    default:
                        MenuStyle = MenuStyleHoverDown;
                        break;
                }
            }
            else {
                switch (Direction) {
                    case 1:
                        MenuStyle = MenuStyleFlyoutRight;
                        break;
                    case 2:
                        MenuStyle = MenuStyleFlyoutUp;
                        break;
                    case 3:
                        MenuStyle = MenuStyleFlyoutLeft;
                        break;
                    default:
                        MenuStyle = MenuStyleFlyoutDown;
                        break;
                }
            }
            
            getMenuType = this.GetMenuFlyout(MenuName, MenuStyle, StyleSheetPrefix);
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
        ErrorTrap:
            this.handleLegacyClassError("GetMenu", Err.Number, Err.Source, Err.Description);
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
                    string CaptionImageLink, 
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
                iEntry[iEntryCount].Link = encodeEmptyText(ImageLink, "");
                // 
                //  No image, must have a caption
                // 
                Caption = encodeEmptyText(Caption, iEntryName);
                // 
                //  Image present, caption is extra
                // 
                Caption = encodeEmptyText(Caption, "");
                ImageOverLink.NewWindow = NewWindow;
                genericController.vbUCase(encodeEmptyText(ParentiEntryName, "")).ImageOver = NewWindow;
                UcaseEntryName.ParentName = NewWindow;
                encodeEmptyText(CaptionImageLink, "").Name = NewWindow;
                CaptionImage = NewWindow;
            }
            
            EntryIndexName.setPtr(UcaseEntryName, iEntryCount);
            iEntryCount = (iEntryCount + 1);
        }
        
        void ErrorTrap(void :, void Call, void handleLegacyClassError, void AddEntry, void Err.Number, void Err.Source, void Err.Description) {
        }
        
        // 
        // ===============================================================================
        //    Returns javascripts, etc. required after all menus on a page are complete
        // ===============================================================================
        // 
        public string GetMenuClose() {
            GetMenuClose = iMenuCloseString;
            iMenuCloseString = "";
        }
        
        // 
        // ===============================================================================
        //    Returns from the cursor position to the end of line
        //    Moves the Start Position to the next line
        // ===============================================================================
        // 
        private string ReadLine(int StartPosition, string Source) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            int EndOfLine;
            // 
            ReadLine = "";
            EndOfLine = genericController.vbInstr(StartPosition, Source, "\r\n");
            if ((EndOfLine != 0)) {
                ReadLine = Source.Substring((StartPosition - 1), EndOfLine);
                StartPosition = (EndOfLine + 2);
            }
            
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
        ErrorTrap:
            this.handleLegacyClassError("ReadLine", Err.Number, Err.Source, Err.Description);
        }
        
        // 
        // ===============================================================================
        //    Returns the menu specified, if it is in local storage
        //        It also creates the menu data in a close string that is returned in GetMenuClose.
        // ===============================================================================
        // 
        private string GetMenuFlyout(string MenuName, int MenuStyle, string StyleSheetPrefix, void =, void ) {
            string result = "";
            // Warning!!! Optional parameters not supported
            try {
                // 
                string Link;
                int EntryPointer;
                string UcaseMenuName;
                string FlyoutStyle;
                string HotSpotHTML = "";
                string FlyoutPanel;
                int FlyoutDirection;
                string LocalStyleSheetPrefix;
                bool FlyoutHover;
                string MouseClickCode;
                string MouseOverCode;
                string MouseOutCode;
                string ImageID;
                string JavaCode = "";
                int PanelButtonCount;
                bool IsTextHotSpot;
                // 
                if ((iEntryCount > 0)) {
                    // 
                    //  ----- Get the menu pointer
                    // 
                    LocalStyleSheetPrefix = encodeEmptyText(StyleSheetPrefix, "ccFlyout");
                    if ((LocalStyleSheetPrefix == "")) {
                        LocalStyleSheetPrefix = "ccFlyout";
                    }
                    
                    UcaseMenuName = genericController.vbUCase(MenuName);
                    for (EntryPointer = 0; (EntryPointer 
                                <= (iEntryCount - 1)); EntryPointer++) {
                        if ((iEntry[EntryPointer].Name == UcaseMenuName)) {
                            break;
                        }
                        
                    }
                    
                    if ((EntryPointer < iEntryCount)) {
                        MouseClickCode = "";
                        MouseOverCode = "";
                        MouseOutCode = "";
                        ImageID = ("img" 
                                    + (genericController.GetRandomInteger().ToString() + "s"));
                        FlyoutStyle = (LocalStyleSheetPrefix + "Button");
                        switch (MenuStyle) {
                            case MenuStyleFlyoutRight:
                            case MenuStyleFlyoutUp:
                            case MenuStyleFlyoutDown:
                            case MenuStyleFlyoutLeft:
                                FlyoutHover = false;
                                break;
                            default:
                                FlyoutHover = true;
                                break;
                        }
                        // 
                        // With...
                        Link = genericController.encodeHTML(iEntry[EntryPointer].Link);
                        if ((iEntry[EntryPointer].Image != "")) {
                            // 
                            //  Create hotspot from image
                            // 
                            HotSpotHTML = ("<img src=\"" 
                                        + (iEntry[EntryPointer].Image + ("\" border=\"0\" alt=\"" 
                                        + (iEntry[EntryPointer].Caption + ("\" ID=" 
                                        + (ImageID + (" Name=" 
                                        + (ImageID + ">"))))))));
                            if ((iEntry[EntryPointer].ImageOver != "")) {
                                JavaCode = (JavaCode + ("var " 
                                            + (ImageID + ("n=new Image; " 
                                            + (ImageID + ("n.src=\'" 
                                            + (iEntry[EntryPointer].Image + ("\'; " + ("var " 
                                            + (ImageID + ("o=new Image; " 
                                            + (ImageID + ("o.src=\'" 
                                            + (iEntry[EntryPointer].ImageOver + "\'; "))))))))))))));
                                MouseOverCode = (MouseOverCode + (" document." 
                                            + (ImageID + (".src=" 
                                            + (ImageID + "o.src;")))));
                                MouseOutCode = (MouseOutCode + (" document." 
                                            + (ImageID + (".src=" 
                                            + (ImageID + "n.src;")))));
                            }
                            
                        }
                        else if ((iEntry[EntryPointer].Caption != "")) {
                            // 
                            //  Create hotspot text
                            // 
                            if ((iEntry[EntryPointer].CaptionImage != "")) {
                                HotSpotHTML = ("<img alt=\"" 
                                            + (iEntry[EntryPointer].Caption + ("\" src=\"" 
                                            + (iEntry[EntryPointer].CaptionImage + "\" border=\"0\">"))));
                            }
                            
                            HotSpotHTML = (HotSpotHTML + iEntry[EntryPointer].Caption);
                            IsTextHotSpot = true;
                        }
                        else {
                            // 
                            //  Create hotspot from name
                            // 
                            HotSpotHTML = iEntry[EntryPointer].Name;
                            IsTextHotSpot = true;
                        }
                        
                        // 
                        FlyoutPanel = this.GetMenuFlyoutPanel(UcaseMenuName, "", LocalStyleSheetPrefix, FlyoutHover, PanelButtonCount);
                        // 
                        //  do not fix the navigation menus by making an exception with the menu object. It is also used for Record Add tags, which need a flyout of 1.
                        //    make the exception in the menuing code above this.
                        // 
                        if ((PanelButtonCount > 0)) {
                            // If PanelButtonCount = 1 Then
                            //     '
                            //     ' Single panel entry, just put the link on the button
                            //     '
                            //     FlyoutPanel = ""
                            //     MouseOverCode = ""
                            //     MouseOutCode = ""
                            // ElseIf PanelButtonCount > 1 Then
                            // If FlyoutPanel <> "" Then
                            // 
                            //  Panel exists, create flyout/hover link
                            // 
                            switch (MenuStyle) {
                                case MenuStyleFlyoutRight:
                                case MenuStyleHoverRight:
                                    FlyoutDirection = 1;
                                    break;
                                case MenuStyleFlyoutUp:
                                case MenuStyleHoverUp:
                                    FlyoutDirection = 2;
                                    break;
                                case MenuStyleFlyoutLeft:
                                case MenuStyleHoverLeft:
                                    FlyoutDirection = 3;
                                    break;
                                default:
                                    FlyoutDirection = 0;
                                    break;
                            }
                            if (FlyoutHover) {
                                MouseOverCode = (MouseOverCode + (" ccFlyoutHoverMode(1); return ccFlyoutButtonClick(event, \'" 
                                            + (MenuFlyoutNamePrefix + ("_" 
                                            + (UcaseMenuName + ("\',\'" 
                                            + (FlyoutDirection + ("\',\'" 
                                            + (LocalStyleSheetPrefix + "\',\'true\');")))))))));
                                MouseOutCode = (MouseOutCode + " ccFlyoutHoverMode(0);");
                            }
                            else {
                                if (IsTextHotSpot) {
                                    HotSpotHTML = (HotSpotHTML + MenuFlyoutIcon_Local);
                                }
                                
                                MouseClickCode = (MouseClickCode + (" return ccFlyoutButtonClick(event, \'" 
                                            + (MenuFlyoutNamePrefix + ("_" 
                                            + (UcaseMenuName + ("\',\'" 
                                            + (FlyoutDirection + ("\',\'" 
                                            + (LocalStyleSheetPrefix + "\');")))))))));
                                MouseOverCode = (MouseOverCode + (" ccFlyoutButtonHover(event,\'" 
                                            + (MenuFlyoutNamePrefix + ("_" 
                                            + (UcaseMenuName + ("\',\'" 
                                            + (FlyoutDirection + "\',\'false\');")))))));
                            }
                            
                        }
                        
                        // 
                        //  Convert js code to action
                        // 
                        if ((MouseClickCode != "")) {
                            MouseClickCode = (" onClick=\"" 
                                        + (MouseClickCode + "\" "));
                        }
                        
                        if ((MouseOverCode != "")) {
                            MouseOverCode = (" onMouseOver=\"" 
                                        + (MouseOverCode + "\" "));
                        }
                        
                        if ((MouseOutCode != "")) {
                            MouseOutCode = (" onMouseOut=\"" 
                                        + (MouseOutCode + "\" "));
                        }
                        
                        // 
                        if ((FlyoutPanel != "")) {
                            // 
                            //  Create a flyout link
                            // 
                            result = ("<a class=\"" 
                                        + (FlyoutStyle + ("\" " 
                                        + (MouseOutCode + (" " 
                                        + (MouseOverCode + (" " 
                                        + (MouseClickCode + (" HREF=\"" 
                                        + (Link + ("\">" 
                                        + (HotSpotHTML + "</a>"))))))))))));
                            iMenuCloseString = (iMenuCloseString + FlyoutPanel);
                        }
                        else if ((Link != "")) {
                            // 
                            //  Create a linked element
                            // 
                            result = ("<a class=\"" 
                                        + (FlyoutStyle + ("\" " 
                                        + (MouseOutCode + (" " 
                                        + (MouseOverCode + (" " 
                                        + (MouseClickCode + (" HREF=\"" 
                                        + (Link + ("\">" 
                                        + (HotSpotHTML + "</a>"))))))))))));
                        }
                        else {
                            // 
                            //  no links and no flyouts, create just the caption
                            // 
                        }
                        
                        // 
                        //  Add in the inline java code if required
                        // 
                        if ((JavaCode != "")) {
                            result = ("" + ("<SCRIPT language=javascript type=text/javascript>" 
                                        + (JavaCode + ("</script>" + result))));
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
        // ===============================================================================
        //    Gets the Menu Branch for the Default Menu
        // ===============================================================================
        // 
        private string GetMenuFlyoutPanel(string PanelName, string UsedEntries, string StyleSheetPrefix, bool FlyoutHover, int PanelButtonCount) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            int EntryPointer;
            string iUsedEntries;
            string SubMenuName;
            int SubMenuCount;
            string target;
            string SubMenus = "";
            string PanelChildren;
            string PanelButtons = "";
            string PanelButtonStyle;
            string HotSpotHTML;
            // 
            iUsedEntries = UsedEntries;
            for (EntryPointer = 0; (EntryPointer 
                        <= (iEntryCount - 1)); EntryPointer++) {
                // With...
                if (((iEntry[EntryPointer].ParentName == PanelName) 
                            && (iEntry[EntryPointer].Caption != ""))) {
                    if (!(((iUsedEntries + ",").IndexOf(("," 
                                    + (EntryPointer + ",")), 0) + 1) 
                                < 0)) {
                        PanelButtonCount = (PanelButtonCount + 1);
                        iUsedEntries = (iUsedEntries + ("," + EntryPointer));
                        PanelButtonStyle = (StyleSheetPrefix + "PanelButton");
                        target = "";
                        if (iEntry[EntryPointer].NewWindow) {
                            target = " target=\"_blank\"";
                        }
                        
                        PanelChildren = this.GetMenuFlyoutPanel(iEntry[EntryPointer].Name, iUsedEntries, StyleSheetPrefix, FlyoutHover, PanelButtonCount);
                        if ((iEntry[EntryPointer].Image != "")) {
                            HotSpotHTML = ("<img src=\"" 
                                        + (iEntry[EntryPointer].Image + ("\" border=\"0\" alt=\"" 
                                        + (iEntry[EntryPointer].Caption + "\">"))));
                        }
                        else {
                            HotSpotHTML = iEntry[EntryPointer].Caption;
                        }
                        
                        if ((PanelChildren == "")) {
                            if ((iEntry[EntryPointer].Link == "")) {
                                // 
                                //  ----- no link and no child panel
                                // 
                                // PanelButtons = PanelButtons & "<SPAN class=""" & PanelButtonStyle & """>" & HotSpotHTML & "</SPAN>"
                            }
                            else {
                                // 
                                //  ----- Link but no child panel
                                // 
                                PanelButtons = (PanelButtons + ("<a class=\"" 
                                            + (PanelButtonStyle + ("\" href=\"" 
                                            + (genericController.encodeHTML(iEntry[EntryPointer].Link) + ("\"" 
                                            + (target + (" onmouseover=\"ccFlyoutHoverMode(1); ccFlyoutPanelButtonHover(event,\'\',\'" 
                                            + (StyleSheetPrefix + ("\');\">" 
                                            + (HotSpotHTML + "</a>")))))))))));
                            }
                            
                        }
                        else if ((iEntry[EntryPointer].Link == "")) {
                            // 
                            //  ----- Child Panel and no link, block the href so menu "parent" buttons will not be clickable
                            // 
                            if (FlyoutHover) {
                                PanelButtons = (PanelButtons + ("<a class=\"" 
                                            + (PanelButtonStyle + ("\" onmouseover=\"ccFlyoutHoverMode(1); ccFlyoutPanelButtonHover(event,\'" 
                                            + (MenuFlyoutNamePrefix + ("_" 
                                            + (iEntry[EntryPointer].Name + ("\',\'" 
                                            + (StyleSheetPrefix + ("\');\" onmouseout=\"ccFlyoutHoverMode(0);\" onclick=\"return false;\" href=\"#\"" 
                                            + (target + (">" 
                                            + (HotSpotHTML 
                                            + (MenuFlyoutIcon_Local + "</a>"))))))))))))));
                            }
                            else {
                                PanelButtons = (PanelButtons + ("<a class=\"" 
                                            + (PanelButtonStyle + ("\" onmouseover=\"ccFlyoutPanelButtonHover(event,\'" 
                                            + (MenuFlyoutNamePrefix + ("_" 
                                            + (iEntry[EntryPointer].Name + ("\',\'" 
                                            + (StyleSheetPrefix + ("\');\" onclick=\"return false;\" href=\"#\"" 
                                            + (target + (">" 
                                            + (HotSpotHTML 
                                            + (MenuFlyoutIcon_Local + "</a>"))))))))))))));
                            }
                            
                        }
                        else {
                            // 
                            //  ----- Child Panel and a link
                            // 
                            if (FlyoutHover) {
                                PanelButtons = (PanelButtons + ("<a class=\"" 
                                            + (PanelButtonStyle + ("\" onmouseover=\"ccFlyoutHoverMode(1); ccFlyoutPanelButtonHover(event,\'" 
                                            + (MenuFlyoutNamePrefix + ("_" 
                                            + (iEntry[EntryPointer].Name + ("\',\'" 
                                            + (StyleSheetPrefix + ("\');\" onmouseout=\"ccFlyoutHoverMode(0);\" href=\"" 
                                            + (genericController.encodeHTML(iEntry[EntryPointer].Link) + ("\"" 
                                            + (target + (">" 
                                            + (HotSpotHTML 
                                            + (MenuFlyoutIcon_Local + "</a>"))))))))))))))));
                            }
                            else {
                                PanelButtons = (PanelButtons + ("<a class=\"" 
                                            + (PanelButtonStyle + ("\" onmouseover=\"ccFlyoutPanelButtonHover(event,\'" 
                                            + (MenuFlyoutNamePrefix + ("_" 
                                            + (iEntry[EntryPointer].Name + ("\',\'" 
                                            + (StyleSheetPrefix + ("\');\" href=\"" 
                                            + (genericController.encodeHTML(iEntry[EntryPointer].Link) + ("\"" 
                                            + (target + (">" 
                                            + (HotSpotHTML 
                                            + (MenuFlyoutIcon_Local + "</a>"))))))))))))))));
                            }
                            
                        }
                        
                        SubMenus = (SubMenus + PanelChildren);
                    }
                    
                }
                
            }
            
            if ((PanelButtons != "")) {
                // 
                //  ----- If panel buttons are returned, wrap them in a DIV
                // 
                if (FlyoutHover) {
                    GetMenuFlyoutPanel = ("<div style=\"position: absolute; left: 0px;visibility:hidden;\" class=\"kmaMenu " 
                                + (StyleSheetPrefix + ("Panel\" id=\"" 
                                + (MenuFlyoutNamePrefix + ("_" 
                                + (PanelName + ("\" onmouseover=\"ccFlyoutHoverMode(1); ccFlyoutPanelHover(event,\'" 
                                + (StyleSheetPrefix + ("\');\" onmouseout=\"ccFlyoutHoverMode(0);\">" 
                                + (PanelButtons 
                                + (SubMenus + ("</div>" + ""))))))))))));
                }
                else {
                    GetMenuFlyoutPanel = ("<div style=\"position: absolute; left: 0px;visibility:hidden;\" class=\"kmaMenu " 
                                + (StyleSheetPrefix + ("Panel\" id=\"" 
                                + (MenuFlyoutNamePrefix + ("_" 
                                + (PanelName + ("\" onmouseover=\"ccFlyoutPanelHover(event,\'" 
                                + (StyleSheetPrefix + ("\')\">" 
                                + (PanelButtons 
                                + (SubMenus + ("</div>" + ""))))))))))));
                }
                
            }
            
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
        ErrorTrap:
            this.handleLegacyClassError("GetMenuFlyoutPanel", Err.Number, Err.Source, Err.Description);
        }
        
        // 
        // ========================================================================
        // 
        // ========================================================================
        // 
        public string MenuFlyoutIcon {
            get {
                return MenuFlyoutIcon_Local;
            }
            set {
                MenuFlyoutIcon_Local = value;
            }
        }
        
        private void handleLegacyClassError(string MethodName, int ErrNumber, string ErrSource, string ErrDescription) {
            // 
            throw new Exception(("unexpected error in method [" 
                            + (MethodName + ("], errDescription [" 
                            + (ErrDescription + "]")))));
        }
        
        // 
        // ========================================================================
        //  ----- Add a new DHTML menu entry
        // ========================================================================
        // 
        public void menu_AddEntry(
                    string Name, 
                    string ParentName, 
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
                    string StyleSheet, 
                    void =, 
                    void , 
                    string StyleSheetHover, 
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
            // 'Dim th as integer : th = profileLogMethodEnter("AddMenuEntry")
            // 
            // If Not (true) Then Exit Sub
            string MethodName;
            string Image;
            string ImageOver = String.Empty;
            // 
            MethodName = "AddMenu()";
            Image = genericController.encodeText(ImageLink);
            if ((Image != "")) {
                ImageOver = genericController.encodeText(ImageOverLink);
                if ((Image == ImageOver)) {
                    ImageOver = "";
                }
                
            }
            
            cpCore.menuFlyout.AddEntry(genericController.encodeText(Name), ParentName, Image, ImageOver, Link, Caption, ,, NewWindow);
            // 
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            throw new ApplicationException("Unexpected exception");
            //  Call cpcore.handleLegacyError18(MethodName)
            // 
        }
        
        // 
        // ========================================================================
        //  ----- main_Get all the menu close scripts
        // 
        //    call this at the end of the page
        // ========================================================================
        // 
        public string menu_GetClose() {
            string result = String.Empty;
            try {
                string MenuFlyoutIcon;
                // 
                if (!(cpCore.menuFlyout == null)) {
                    cpCore.doc.menuSystemCloseCount = (cpCore.doc.menuSystemCloseCount + 1);
                    result = (result + cpCore.menuFlyout.GetMenuClose());
                    MenuFlyoutIcon = cpCore.siteProperties.getText("MenuFlyoutIcon", "»");
                    if ((MenuFlyoutIcon != "»")) {
                        result = genericController.vbReplace(result, "»</a>", (MenuFlyoutIcon + "</a>"));
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
            return result;
        }
    }
}