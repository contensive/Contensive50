
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Controllers {
    public class MenuFlyoutController {
        //
        //==============================================================================
        //
        //   Creates custom menus
        //   Stores caches of the menus
        //   Stores the menu data, and can generate different kind
        //
        //==============================================================================
        //
        private const int MenuStyleFlyoutDown = 4;
        private const int MenuStyleFlyoutRight = 5;
        private const int MenuStyleFlyoutUp = 6;
        private const int MenuStyleFlyoutLeft = 7;
        private const int MenuStyleHoverDown = 8;
        private const int MenuStyleHoverRight = 9;
        private const int MenuStyleHoverUp = 10;
        private const int MenuStyleHoverLeft = 11;
        //
        // ----- Each menu item has an MenuEntry
        //
        private struct MenuEntryType {
            public string Caption; // What is displayed for this entry (does not need to be unique)
            public string CaptionImage; // If present, this is an image in front of the caption
            public string Name; // Unique name for this entry
            public string ParentName; // Unique name of the parent entry
            public string Link; // URL
            public string Image; // Image
            public string ImageOver; // Image Over
            public bool NewWindow; // True opens link in a new window
        }
        //
        // ----- Menu Entry storage
        //
        private int iEntryCount; // Count of Menus in the object
        private int iEntrySize;
        private MenuEntryType[] iEntry;
        private string iMenuCloseString;
        private string UsedEntries; 
        private keyPtrController EntryIndexName;
         //
        // ----- RollOverFlyout storage
        //
        private string MenuFlyoutNamePrefix; // Random prefix added to element IDs to avoid namespace collision
        private string MenuFlyoutIcon_Local; // string used to mark a button that has a non-hover flyout
        private CoreController core;
        //
        //==================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <remarks></remarks>
        public MenuFlyoutController(CoreController core) : base() {
            this.core = core;
            //
            EntryIndexName = new keyPtrController();
            Random rnd = new Random();
            MenuFlyoutNamePrefix = "id" + rnd.Next(9999).ToString();
            MenuFlyoutIcon_Local = "&nbsp;&#187;";
        }
        //
        //========================================================================
        // ----- main_Get the menu link for the menu name specified
        //========================================================================
        //
        public string getMenu(string menuName, int MenuStyle, string StyleSheetPrefix = "") {
            string returnHtml = "";
            try {
                string MenuFlyoutIcon = null;
                const string DefaultIcon = "&#187;";
                //
                MenuFlyoutIcon = core.siteProperties.getText("MenuFlyoutIcon", DefaultIcon);
                if (MenuFlyoutIcon != DefaultIcon) {
                    //MenuFlyoutIcon = MenuFlyoutIcon;
                }
                switch (MenuStyle) {
                    case 11:
                        returnHtml = getMenuType(menuName, false, 3, encodeEmpty(StyleSheetPrefix, ""));
                        break;
                    case 10:
                        returnHtml = getMenuType(menuName, false, 2, encodeEmpty(StyleSheetPrefix, ""));
                        break;
                    case 9:
                        returnHtml = getMenuType(menuName, false, 1, encodeEmpty(StyleSheetPrefix, ""));
                        break;
                    case 8:
                        returnHtml = getMenuType(menuName, false, 0, encodeEmpty(StyleSheetPrefix, ""));
                        break;
                    case 7:
                        returnHtml = getMenuType(menuName, true, 3, encodeEmpty(StyleSheetPrefix, ""));
                        break;
                    case 6:
                        returnHtml = getMenuType(menuName, true, 2, encodeEmpty(StyleSheetPrefix, ""));
                        break;
                    case 5:
                        returnHtml = getMenuType(menuName, true, 1, encodeEmpty(StyleSheetPrefix, ""));
                        break;
                    default:
                        returnHtml = getMenuType(menuName, true, 0, encodeEmpty(StyleSheetPrefix, ""));
                        break;
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return returnHtml;
        }

        //
        //===============================================================================
        //   Returns the menu specified, if it is in local storage
        //===============================================================================
        //
        public string getMenuType(string MenuName, bool ClickToOpen, int Direction, string StyleSheetPrefix = "") {
            string result = "";
            try {
                int MenuStyle = 0;
                //
                // ----- Search local storage for this MenuName
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
                } else {
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
                result = GetMenuFlyout(MenuName, MenuStyle, StyleSheetPrefix);
            } catch( Exception ex ) {
                logController.handleError( core,ex);
            }
            return result;
        }
        //
        //===============================================================================
        //   Create a new Menu Entry
        //===============================================================================
        //
        public void AddEntry(string EntryName, string ParentiEntryName = "", string ImageLink = "", string ImageOverLink = "", string Link = "", string Caption = "", string CaptionImageLink = "", bool NewWindow = false) {
            try {
                //
                string iEntryName = null;
                string UcaseEntryName = null;
                //
                iEntryName = genericController.vbReplace(encodeEmpty(EntryName, ""), ",", " ");
                UcaseEntryName = genericController.vbUCase(iEntryName);
                //
                if ((!string.IsNullOrEmpty(iEntryName)) && ((UsedEntries + ",").IndexOf("," + UcaseEntryName + ",")  == -1)) {
                    UsedEntries = UsedEntries + "," + UcaseEntryName;
                    if (iEntryCount >= iEntrySize) {
                        iEntrySize = iEntrySize + 10;
                        Array.Resize(ref iEntry, iEntrySize + 1);
                    }
                    iEntry[iEntryCount].Link = encodeEmpty(Link, "");
                    iEntry[iEntryCount].Image = encodeEmpty(ImageLink, "");
                    if (string.IsNullOrEmpty(iEntry[iEntryCount].Image)) {
                        //
                        // No image, must have a caption
                        //
                        iEntry[iEntryCount].Caption = encodeEmpty(Caption, iEntryName);
                    } else {
                        //
                        // Image present, caption is extra
                        //
                        iEntry[iEntryCount].Caption = encodeEmpty(Caption, "");
                    }
                    iEntry[iEntryCount].CaptionImage = encodeEmpty(CaptionImageLink, "");
                    iEntry[iEntryCount].Name = UcaseEntryName;
                    iEntry[iEntryCount].ParentName = genericController.vbUCase(encodeEmpty(ParentiEntryName, ""));
                    iEntry[iEntryCount].ImageOver = ImageOverLink;
                    iEntry[iEntryCount].NewWindow = NewWindow;
                    EntryIndexName.setPtr(UcaseEntryName, iEntryCount);
                    iEntryCount = iEntryCount + 1;
                }
                //
                return;
                //
            } catch( Exception ex ) {
                logController.handleError( core,ex);
            }
            //ErrorTrap:
            //todo  TASK: Calls to the VB 'Err' function are not converted by Instant C#:
            handleLegacyClassError("AddEntry", 0, "", "");
        }
        //
        //===============================================================================
        //   Returns javascripts, etc. required after all menus on a page are complete
        //===============================================================================
        //
        public string GetMenuClose() {
            string tempGetMenuClose = null;
            tempGetMenuClose = iMenuCloseString;
            iMenuCloseString = "";
            return tempGetMenuClose;
        }
        //
        //===============================================================================
        //   Returns from the cursor position to the end of line
        //   Moves the Start Position to the next line
        //===============================================================================
        //
        private string ReadLine(int StartPosition, string Source) {
            string tempReadLine = null;
            try {
                //
                int EndOfLine = 0;
                //
                tempReadLine = "";
                EndOfLine = genericController.vbInstr(StartPosition, Source, "\r\n");
                if (EndOfLine != 0) {
                    tempReadLine = Source.Substring(StartPosition - 1, EndOfLine);
                    StartPosition = EndOfLine + 2;
                }
                //
                return tempReadLine;
                //
            } catch( Exception ex ) {
                logController.handleError( core,ex);
            }
            //ErrorTrap:
            //todo  TASK: Calls to the VB 'Err' function are not converted by Instant C#:
            handleLegacyClassError("ReadLine", 0, "", "");
            return tempReadLine;
        }
        //
        //===============================================================================
        //   Returns the menu specified, if it is in local storage
        //       It also creates the menu data in a close string that is returned in GetMenuClose.
        //===============================================================================
        //
        private string GetMenuFlyout(string MenuName, int MenuStyle, string StyleSheetPrefix = "") {
            string result = "";
            try {

                //
                string Link = null;
                int EntryPointer = 0;
                string UcaseMenuName = null;
                string FlyoutStyle = null;
                string HotSpotHTML = "";
                string FlyoutPanel = null;
                int FlyoutDirection = 0;
                string LocalStyleSheetPrefix = null;
                bool FlyoutHover = false;
                string MouseClickCode = null;
                string MouseOverCode = null;
                string MouseOutCode = null;
                string ImageID = null;
                string JavaCode = "";
                int PanelButtonCount = 0;
                bool IsTextHotSpot = false;
                //
                if (iEntryCount > 0) {
                    //
                    // ----- Get the menu pointer
                    //
                    LocalStyleSheetPrefix = encodeEmpty(StyleSheetPrefix, "ccFlyout");
                    if (string.IsNullOrEmpty(LocalStyleSheetPrefix)) {
                        LocalStyleSheetPrefix = "ccFlyout";
                    }
                    UcaseMenuName = genericController.vbUCase(MenuName);
                    for (EntryPointer = 0; EntryPointer < iEntryCount; EntryPointer++) {
                        if (iEntry[EntryPointer].Name == UcaseMenuName) {
                            break;
                        }
                    }
                    if (EntryPointer < iEntryCount) {
                        MouseClickCode = "";
                        MouseOverCode = "";
                        MouseOutCode = "";
                        ImageID = "img" + encodeText(genericController.GetRandomInteger(core)) + "s";
                        FlyoutStyle = LocalStyleSheetPrefix + "Button";
                        //
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
                        Link = HtmlController.encodeHtml(iEntry[EntryPointer].Link);
                        if (!string.IsNullOrEmpty(iEntry[EntryPointer].Image)) {
                            //
                            // Create hotspot from image
                            //
                            HotSpotHTML = "<img src=\"" + iEntry[EntryPointer].Image + "\" border=\"0\" alt=\"" + iEntry[EntryPointer].Caption + "\" ID=" + ImageID + " Name=" + ImageID + ">";
                            if (!string.IsNullOrEmpty(iEntry[EntryPointer].ImageOver)) {
                                JavaCode = JavaCode + "var " + ImageID + "n=new Image; "
                                + ImageID + "n.src='" + iEntry[EntryPointer].Image + "'; "
                                + "var " + ImageID + "o=new Image; "
                                + ImageID + "o.src='" + iEntry[EntryPointer].ImageOver + "'; ";
                                MouseOverCode = MouseOverCode + " document." + ImageID + ".src=" + ImageID + "o.src;";
                                MouseOutCode = MouseOutCode + " document." + ImageID + ".src=" + ImageID + "n.src;";
                            }
                        } else if (!string.IsNullOrEmpty(iEntry[EntryPointer].Caption)) {
                            //
                            // Create hotspot text
                            //
                            if (!string.IsNullOrEmpty(iEntry[EntryPointer].CaptionImage)) {
                                HotSpotHTML = "<img alt=\"" + iEntry[EntryPointer].Caption + "\" src=\"" + iEntry[EntryPointer].CaptionImage + "\" border=\"0\">";
                            }
                            HotSpotHTML = HotSpotHTML + iEntry[EntryPointer].Caption;
                            IsTextHotSpot = true;
                        } else {
                            //
                            // Create hotspot from name
                            //
                            HotSpotHTML = iEntry[EntryPointer].Name;
                            IsTextHotSpot = true;
                        }
                        //
                        FlyoutPanel = GetMenuFlyoutPanel(UcaseMenuName, "", LocalStyleSheetPrefix, FlyoutHover, PanelButtonCount);
                        //
                        // do not fix the navigation menus by making an exception with the menu object. It is also used for Record Add tags, which need a flyout of 1.
                        //   make the exception in the menuing code above this.
                        //
                        if (PanelButtonCount > 0) {
                            //If PanelButtonCount = 1 Then
                            //    '
                            //    ' Single panel entry, just put the link on the button
                            //    '
                            //    FlyoutPanel = ""
                            //    MouseOverCode = ""
                            //    MouseOutCode = ""
                            //ElseIf PanelButtonCount > 1 Then
                            //If FlyoutPanel <> "" Then
                            //
                            // Panel exists, create flyout/hover link
                            //
                            switch (MenuStyle) {
                                //
                                // Set direction flag based on style
                                //
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
                                MouseOverCode = MouseOverCode + " ccFlyoutHoverMode(1); return ccFlyoutButtonClick(event, '" + MenuFlyoutNamePrefix + "_" + UcaseMenuName + "','" + FlyoutDirection + "','" + LocalStyleSheetPrefix + "','true');";
                                MouseOutCode = MouseOutCode + " ccFlyoutHoverMode(0);";
                            } else {
                                if (IsTextHotSpot) {
                                    HotSpotHTML = HotSpotHTML + MenuFlyoutIcon_Local;
                                }
                                MouseClickCode = MouseClickCode + " return ccFlyoutButtonClick(event, '" + MenuFlyoutNamePrefix + "_" + UcaseMenuName + "','" + FlyoutDirection + "','" + LocalStyleSheetPrefix + "');";
                                MouseOverCode = MouseOverCode + " ccFlyoutButtonHover(event,'" + MenuFlyoutNamePrefix + "_" + UcaseMenuName + "','" + FlyoutDirection + "','false');";
                            }

                        }
                        //
                        // Convert js code to action
                        //
                        if (!string.IsNullOrEmpty(MouseClickCode)) {
                            MouseClickCode = " onClick=\"" + MouseClickCode + "\" ";
                        }
                        if (!string.IsNullOrEmpty(MouseOverCode)) {
                            MouseOverCode = " onMouseOver=\"" + MouseOverCode + "\" ";
                        }
                        if (!string.IsNullOrEmpty(MouseOutCode)) {
                            MouseOutCode = " onMouseOut=\"" + MouseOutCode + "\" ";
                        }
                        //
                        if (!string.IsNullOrEmpty(FlyoutPanel)) {
                            //
                            // Create a flyout link
                            //
                            result = "<a class=\"" + FlyoutStyle + "\" " + MouseOutCode + " " + MouseOverCode + " " + MouseClickCode + " HREF=\"" + Link + "\">" + HotSpotHTML + "</a>";
                            iMenuCloseString = iMenuCloseString + FlyoutPanel;
                        } else if (!string.IsNullOrEmpty(Link)) {
                            //
                            // Create a linked element
                            //
                            result = "<a class=\"" + FlyoutStyle + "\" " + MouseOutCode + " " + MouseOverCode + " " + MouseClickCode + " HREF=\"" + Link + "\">" + HotSpotHTML + "</a>";
                        } else {
                            //
                            // no links and no flyouts, create just the caption
                            //
                        }
                        //
                        // Add in the inline java code if required
                        //
                        if (!string.IsNullOrEmpty(JavaCode)) {
                            result = ""
                                + "<SCRIPT language=javascript type=text/javascript>"
                                + JavaCode + "</script>"
                                + result;
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return result;
        }
        //
        //===============================================================================
        //   Gets the Menu Branch for the Default Menu
        //===============================================================================
        //
        private string GetMenuFlyoutPanel(string PanelName, string UsedEntries, string StyleSheetPrefix, bool FlyoutHover, int PanelButtonCount) {
            string tempGetMenuFlyoutPanel = null;
            try {
                //
                int EntryPointer = 0;
                string iUsedEntries = null;
                string target = null;
                string SubMenus = "";
                string PanelChildren = null;
                string PanelButtons = "";
                string PanelButtonStyle = null;
                string HotSpotHTML = null;
                //
                iUsedEntries = UsedEntries;
                for (EntryPointer = 0; EntryPointer < iEntryCount; EntryPointer++) {
                    if ((iEntry[EntryPointer].ParentName == PanelName) && (!string.IsNullOrEmpty(iEntry[EntryPointer].Caption))) {
                        if (!((iUsedEntries + ",").IndexOf("," + EntryPointer + ",") + 1 < 0)) {
                            PanelButtonCount = PanelButtonCount + 1;
                            iUsedEntries = iUsedEntries + "," + EntryPointer;
                            PanelButtonStyle = StyleSheetPrefix + "PanelButton";
                            target = "";
                            if (iEntry[EntryPointer].NewWindow) {
                                target = " target=\"_blank\"";
                            }
                            PanelChildren = GetMenuFlyoutPanel(iEntry[EntryPointer].Name, iUsedEntries, StyleSheetPrefix, FlyoutHover, PanelButtonCount);
                            if (!string.IsNullOrEmpty(iEntry[EntryPointer].Image)) {
                                HotSpotHTML = "<img src=\"" + iEntry[EntryPointer].Image + "\" border=\"0\" alt=\"" + iEntry[EntryPointer].Caption + "\">";
                            } else {
                                HotSpotHTML = iEntry[EntryPointer].Caption;
                            }
                            if (string.IsNullOrEmpty(PanelChildren)) {
                                if (string.IsNullOrEmpty(iEntry[EntryPointer].Link)) {
                                    //
                                    // ----- no link and no child panel
                                    //
                                    //PanelButtons = PanelButtons & "<SPAN class=""" & PanelButtonStyle & """>" & HotSpotHTML & "</SPAN>"
                                } else {
                                    //
                                    // ----- Link but no child panel
                                    //
                                    PanelButtons = PanelButtons + "<a class=\"" + PanelButtonStyle + "\" href=\"" + HtmlController.encodeHtml(iEntry[EntryPointer].Link) + "\"" + target + " onmouseover=\"ccFlyoutHoverMode(1); ccFlyoutPanelButtonHover(event,'','" + StyleSheetPrefix + "');\">" + HotSpotHTML + "</a>";
                                }
                            } else {
                                if (string.IsNullOrEmpty(iEntry[EntryPointer].Link)) {
                                    //
                                    // ----- Child Panel and no link, block the href so menu "parent" buttons will not be clickable
                                    //
                                    if (FlyoutHover) {
                                        PanelButtons = PanelButtons + "<a class=\"" + PanelButtonStyle + "\" onmouseover=\"ccFlyoutHoverMode(1); ccFlyoutPanelButtonHover(event,'" + MenuFlyoutNamePrefix + "_" + iEntry[EntryPointer].Name + "','" + StyleSheetPrefix + "');\" onmouseout=\"ccFlyoutHoverMode(0);\" onclick=\"return false;\" href=\"#\"" + target + ">" + HotSpotHTML + MenuFlyoutIcon_Local + "</a>";
                                    } else {
                                        PanelButtons = PanelButtons + "<a class=\"" + PanelButtonStyle + "\" onmouseover=\"ccFlyoutPanelButtonHover(event,'" + MenuFlyoutNamePrefix + "_" + iEntry[EntryPointer].Name + "','" + StyleSheetPrefix + "');\" onclick=\"return false;\" href=\"#\"" + target + ">" + HotSpotHTML + MenuFlyoutIcon_Local + "</a>";
                                    }
                                } else {
                                    //
                                    // ----- Child Panel and a link
                                    //
                                    if (FlyoutHover) {
                                        PanelButtons = PanelButtons + "<a class=\"" + PanelButtonStyle + "\" onmouseover=\"ccFlyoutHoverMode(1); ccFlyoutPanelButtonHover(event,'" + MenuFlyoutNamePrefix + "_" + iEntry[EntryPointer].Name + "','" + StyleSheetPrefix + "');\" onmouseout=\"ccFlyoutHoverMode(0);\" href=\"" + HtmlController.encodeHtml(iEntry[EntryPointer].Link) + "\"" + target + ">" + HotSpotHTML + MenuFlyoutIcon_Local + "</a>";
                                    } else {
                                        PanelButtons = PanelButtons + "<a class=\"" + PanelButtonStyle + "\" onmouseover=\"ccFlyoutPanelButtonHover(event,'" + MenuFlyoutNamePrefix + "_" + iEntry[EntryPointer].Name + "','" + StyleSheetPrefix + "');\" href=\"" + HtmlController.encodeHtml(iEntry[EntryPointer].Link) + "\"" + target + ">" + HotSpotHTML + MenuFlyoutIcon_Local + "</a>";
                                    }
                                }
                            }
                            SubMenus = SubMenus + PanelChildren;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(PanelButtons)) {
                    //
                    // ----- If panel buttons are returned, wrap them in a DIV
                    //
                    if (FlyoutHover) {
                        tempGetMenuFlyoutPanel = "<div style=\"position: absolute; left: 0px;visibility:hidden;\" class=\"kmaMenu " + StyleSheetPrefix + "Panel\" id=\"" + MenuFlyoutNamePrefix + "_" + PanelName + "\" onmouseover=\"ccFlyoutHoverMode(1); ccFlyoutPanelHover(event,'" + StyleSheetPrefix + "');\" onmouseout=\"ccFlyoutHoverMode(0);\">"
                            + PanelButtons + SubMenus + "</div>"
                            + "";
                    } else {
                        tempGetMenuFlyoutPanel = "<div style=\"position: absolute; left: 0px;visibility:hidden;\" class=\"kmaMenu " + StyleSheetPrefix + "Panel\" id=\"" + MenuFlyoutNamePrefix + "_" + PanelName + "\" onmouseover=\"ccFlyoutPanelHover(event,'" + StyleSheetPrefix + "')\">"
                            + PanelButtons + SubMenus + "</div>"
                            + "";
                    }
                }
                //
                return tempGetMenuFlyoutPanel;
                //
            } catch( Exception ex ) {
                logController.handleError( core,ex);
            }
            //ErrorTrap:
            //todo  TASK: Calls to the VB 'Err' function are not converted by Instant C#:
            handleLegacyClassError("GetMenuFlyoutPanel", 0, "", "");
            return tempGetMenuFlyoutPanel;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        public string MenuFlyoutIcon {
            get {
                return MenuFlyoutIcon_Local;
            }
            set {
                MenuFlyoutIcon_Local = value;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// handle legacy errors in this class
        /// </summary>
        /// <param name="MethodName"></param>
        /// <param name="ErrNumber"></param>
        /// <param name="ErrSource"></param>
        /// <param name="ErrDescription"></param>
        /// <remarks></remarks>
        private void handleLegacyClassError(string MethodName, int ErrNumber, string ErrSource, string ErrDescription) {
            throw (new Exception("unexpected error in method [" + MethodName + "], errDescription [" + ErrDescription + "]"));
        }
        //
        //========================================================================
        // ----- Add a new DHTML menu entry
        //========================================================================
        //
        public void menu_AddEntry(string Name, string ParentName = "", string ImageLink = "", string ImageOverLink = "", string Link = "", string Caption = "", string StyleSheet = "", string StyleSheetHover = "", bool NewWindow = false) {
            string Image = null;
            string ImageOver = "";
            //
            Image = genericController.encodeText(ImageLink);
            if (!string.IsNullOrEmpty(Image)) {
                ImageOver = genericController.encodeText(ImageOverLink);
                if (Image == ImageOver) {
                    ImageOver = "";
                }
            }
            core.menuFlyout.AddEntry(genericController.encodeText(Name), ParentName, Image, ImageOver, Link, Caption, "", NewWindow);
        }
        //
        //========================================================================
        // ----- main_Get all the menu close scripts
        //
        //   call this at the end of the page
        //========================================================================
        //
        public string menu_GetClose() {
            string result = "";
            try {
                string MenuFlyoutIcon = null;
                //
                if (core.menuFlyout != null) {
                    core.doc.menuSystemCloseCount = core.doc.menuSystemCloseCount + 1;
                    result += core.menuFlyout.GetMenuClose();
                    MenuFlyoutIcon = core.siteProperties.getText("MenuFlyoutIcon", "&#187;");
                    if (MenuFlyoutIcon != "&#187;") {
                        result = genericController.vbReplace(result, "&#187;</a>", MenuFlyoutIcon + "</a>");
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return result;
        }
    }
}
