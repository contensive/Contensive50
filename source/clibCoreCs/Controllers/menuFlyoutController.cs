﻿using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
//
namespace Contensive.Core.Controllers
{
	public class menuFlyoutController
	{
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
		private struct MenuEntryType
		{
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
		// ----- Local storage
		//
		private string iMenuFilePath;
		//
		// ----- Menu Entry storage
		//
		private int iEntryCount; // Count of Menus in the object
		private int iEntrySize;
		private MenuEntryType[] iEntry;
		//
		private int iTreeCount; // Count of Tree Menus for this instance
		private string iMenuCloseString; // String returned for closing menus
		//
		private string UsedEntries; // String of EntryNames that have been used (for unique test)
		private keyPtrController EntryIndexName;
		//Private EntryIndexID As keyPtrIndex8Class
		//
		// ----- RollOverFlyout storage
		//
		private string MenuFlyoutNamePrefix; // Random prefix added to element IDs to avoid namespace collision
		private string MenuFlyoutIcon_Local; // string used to mark a button that has a non-hover flyout
		private coreClass cpCore;
		//
		//==================================================================================================
		/// <summary>
		/// constructor
		/// </summary>
		/// <remarks></remarks>
		public menuFlyoutController(coreClass cpCore) : base()
		{
			this.cpCore = cpCore;
			//
			EntryIndexName = new keyPtrController();
            Random rnd = new Random(); 
			MenuFlyoutNamePrefix = "id" + rnd.Next(9999).ToString() ;
			MenuFlyoutIcon_Local = "&nbsp;&#187;";
		}
		//
		//========================================================================
		// ----- main_Get the menu link for the menu name specified
		//========================================================================
		//
		public string getMenu(string menuName, int MenuStyle, string StyleSheetPrefix = "")
		{
			string returnHtml = "";
			try
			{
				string MenuFlyoutIcon = null;
				const string DefaultIcon = "&#187;";
				//
				MenuFlyoutIcon = cpCore.siteProperties.getText("MenuFlyoutIcon", DefaultIcon);
				if (MenuFlyoutIcon != DefaultIcon)
				{
					//MenuFlyoutIcon = MenuFlyoutIcon;
				}
				switch (MenuStyle)
				{
					case 11:
						returnHtml = getMenuType(menuName, false, 3, encodeEmptyText(StyleSheetPrefix, ""));
						break;
					case 10:
						returnHtml = getMenuType(menuName, false, 2, encodeEmptyText(StyleSheetPrefix, ""));
						break;
					case 9:
						returnHtml = getMenuType(menuName, false, 1, encodeEmptyText(StyleSheetPrefix, ""));
						break;
					case 8:
						returnHtml = getMenuType(menuName, false, 0, encodeEmptyText(StyleSheetPrefix, ""));
						break;
					case 7:
						returnHtml = getMenuType(menuName, true, 3, encodeEmptyText(StyleSheetPrefix, ""));
						break;
					case 6:
						returnHtml = getMenuType(menuName, true, 2, encodeEmptyText(StyleSheetPrefix, ""));
						break;
					case 5:
						returnHtml = getMenuType(menuName, true, 1, encodeEmptyText(StyleSheetPrefix, ""));
						break;
					default:
						returnHtml = getMenuType(menuName, true, 0, encodeEmptyText(StyleSheetPrefix, ""));
						break;
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return returnHtml;
		}

		//
		//===============================================================================
		//   Returns the menu specified, if it is in local storage
		//===============================================================================
		//
		public string getMenuType(string MenuName, bool ClickToOpen, int Direction, string StyleSheetPrefix = "")
		{
			try
			{
				//
				string MenuSource = null;
				int MenuPointer = 0;
				int MenuStyle = 0;
				//
				// ----- Search local storage for this MenuName
				//
				if (!ClickToOpen)
				{
					switch (Direction)
					{
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
				else
				{
					switch (Direction)
					{
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
				return GetMenuFlyout(MenuName, MenuStyle, StyleSheetPrefix);
				//
				//
			}
			catch
			{
				cpCore.handleException( ex );
			}
//ErrorTrap:
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
			handleLegacyClassError("GetMenu", Microsoft.VisualBasic.Information.Err().Number, Microsoft.VisualBasic.Information.Err().Source, Microsoft.VisualBasic.Information.Err().Description);
		}
		//
		//===============================================================================
		//   Create a new Menu Entry
		//===============================================================================
		//
		public void AddEntry(string EntryName, string ParentiEntryName = "", string ImageLink = "", string ImageOverLink = "", string Link = "", string Caption = "", string CaptionImageLink = "", bool NewWindow = false)
		{
			try
			{
				//
				int MenuEntrySize = 0;
				string iEntryName = null;
				string UcaseEntryName = null;
				bool iNewWindow = false;
				//
				iEntryName = genericController.vbReplace(encodeEmptyText(EntryName, ""), ",", " ");
				UcaseEntryName = genericController.vbUCase(iEntryName);
				//
				if ((!string.IsNullOrEmpty(iEntryName)) && (UsedEntries + ",".IndexOf("," + UcaseEntryName + ",") + 1 == 0))
				{
					UsedEntries = UsedEntries + "," + UcaseEntryName;
					if (iEntryCount >= iEntrySize)
					{
						iEntrySize = iEntrySize + 10;
						Array.Resize(ref iEntry, iEntrySize + 1);
					}
					iEntry[iEntryCount].Link = encodeEmptyText(Link, "");
					iEntry[iEntryCount].Image = encodeEmptyText(ImageLink, "");
					if (string.IsNullOrEmpty(iEntry[iEntryCount].Image))
					{
						//
						// No image, must have a caption
						//
						iEntry[iEntryCount].Caption = encodeEmptyText(Caption, iEntryName);
					}
					else
					{
						//
						// Image present, caption is extra
						//
						iEntry[iEntryCount].Caption = encodeEmptyText(Caption, "");
					}
					iEntry[iEntryCount].CaptionImage = encodeEmptyText(CaptionImageLink, "");
					iEntry[iEntryCount].Name = UcaseEntryName;
					iEntry[iEntryCount].ParentName = genericController.vbUCase(encodeEmptyText(ParentiEntryName, ""));
					iEntry[iEntryCount].ImageOver = ImageOverLink;
					iEntry[iEntryCount].NewWindow = NewWindow;
					EntryIndexName.setPtr(UcaseEntryName, iEntryCount);
					iEntryCount = iEntryCount + 1;
				}
				//
				return;
				//
			}
			catch
			{
				cpCore.handleException( ex );
			}
//ErrorTrap:
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
			handleLegacyClassError("AddEntry", Microsoft.VisualBasic.Information.Err().Number, Microsoft.VisualBasic.Information.Err().Source, Microsoft.VisualBasic.Information.Err().Description);
		}
		//
		//===============================================================================
		//   Returns javascripts, etc. required after all menus on a page are complete
		//===============================================================================
		//
		public string GetMenuClose()
		{
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
		private string ReadLine(int StartPosition, string Source)
		{
				string tempReadLine = null;
			try
			{
				//
				int EndOfLine = 0;
				//
				tempReadLine = "";
				EndOfLine = genericController.vbInstr(StartPosition, Source, Environment.NewLine);
				if (EndOfLine != 0)
				{
					tempReadLine = Source.Substring(StartPosition - 1, EndOfLine);
					StartPosition = EndOfLine + 2;
				}
				//
				return tempReadLine;
				//
			}
			catch
			{
				cpCore.handleException( ex );
			}
//ErrorTrap:
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
			handleLegacyClassError("ReadLine", Microsoft.VisualBasic.Information.Err().Number, Microsoft.VisualBasic.Information.Err().Source, Microsoft.VisualBasic.Information.Err().Description);
			return tempReadLine;
		}
		//
		//===============================================================================
		//   Returns the menu specified, if it is in local storage
		//       It also creates the menu data in a close string that is returned in GetMenuClose.
		//===============================================================================
		//
		private string GetMenuFlyout(string MenuName, int MenuStyle, string StyleSheetPrefix = "")
		{
			string result = "";
			try
			{

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
				if (iEntryCount > 0)
				{
					//
					// ----- Get the menu pointer
					//
					LocalStyleSheetPrefix = encodeEmptyText(StyleSheetPrefix, "ccFlyout");
					if (string.IsNullOrEmpty(LocalStyleSheetPrefix))
					{
						LocalStyleSheetPrefix = "ccFlyout";
					}
					UcaseMenuName = genericController.vbUCase(MenuName);
					for (EntryPointer = 0; EntryPointer < iEntryCount; EntryPointer++)
					{
						if (iEntry[EntryPointer].Name == UcaseMenuName)
						{
							break;
						}
					}
					if (EntryPointer < iEntryCount)
					{
						MouseClickCode = "";
						MouseOverCode = "";
						MouseOutCode = "";
						ImageID = "img" + Convert.ToString(genericController.GetRandomInteger()) + "s";
						FlyoutStyle = LocalStyleSheetPrefix + "Button";
						//
						switch (MenuStyle)
						{
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
						Link = genericController.encodeHTML(iEntry[EntryPointer].Link);
						if (!string.IsNullOrEmpty(iEntry[EntryPointer].Image))
						{
							//
							// Create hotspot from image
							//
							HotSpotHTML = "<img src=\"" + iEntry[EntryPointer].Image + "\" border=\"0\" alt=\"" + iEntry[EntryPointer].Caption + "\" ID=" + ImageID + " Name=" + ImageID + ">";
							if (!string.IsNullOrEmpty(iEntry[EntryPointer].ImageOver))
							{
								JavaCode = JavaCode + "var " + ImageID + "n=new Image; "
								+ ImageID + "n.src='" + iEntry[EntryPointer].Image + "'; "
								+ "var " + ImageID + "o=new Image; "
								+ ImageID + "o.src='" + iEntry[EntryPointer].ImageOver + "'; ";
								MouseOverCode = MouseOverCode + " document." + ImageID + ".src=" + ImageID + "o.src;";
								MouseOutCode = MouseOutCode + " document." + ImageID + ".src=" + ImageID + "n.src;";
							}
						}
						else if (!string.IsNullOrEmpty(iEntry[EntryPointer].Caption))
						{
							//
							// Create hotspot text
							//
							if (!string.IsNullOrEmpty(iEntry[EntryPointer].CaptionImage))
							{
								HotSpotHTML = "<img alt=\"" + iEntry[EntryPointer].Caption + "\" src=\"" + iEntry[EntryPointer].CaptionImage + "\" border=\"0\">";
							}
							HotSpotHTML = HotSpotHTML + iEntry[EntryPointer].Caption;
							IsTextHotSpot = true;
						}
						else
						{
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
						if (PanelButtonCount > 0)
						{
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
							switch (MenuStyle)
							{
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
							if (FlyoutHover)
							{
								MouseOverCode = MouseOverCode + " ccFlyoutHoverMode(1); return ccFlyoutButtonClick(event, '" + MenuFlyoutNamePrefix + "_" + UcaseMenuName + "','" + FlyoutDirection + "','" + LocalStyleSheetPrefix + "','true');";
								MouseOutCode = MouseOutCode + " ccFlyoutHoverMode(0);";
							}
							else
							{
								if (IsTextHotSpot)
								{
									HotSpotHTML = HotSpotHTML + MenuFlyoutIcon_Local;
								}
								MouseClickCode = MouseClickCode + " return ccFlyoutButtonClick(event, '" + MenuFlyoutNamePrefix + "_" + UcaseMenuName + "','" + FlyoutDirection + "','" + LocalStyleSheetPrefix + "');";
								MouseOverCode = MouseOverCode + " ccFlyoutButtonHover(event,'" + MenuFlyoutNamePrefix + "_" + UcaseMenuName + "','" + FlyoutDirection + "','false');";
							}

						}
						//
						// Convert js code to action
						//
						if (!string.IsNullOrEmpty(MouseClickCode))
						{
							MouseClickCode = " onClick=\"" + MouseClickCode + "\" ";
						}
						if (!string.IsNullOrEmpty(MouseOverCode))
						{
							MouseOverCode = " onMouseOver=\"" + MouseOverCode + "\" ";
						}
						if (!string.IsNullOrEmpty(MouseOutCode))
						{
							MouseOutCode = " onMouseOut=\"" + MouseOutCode + "\" ";
						}
						//
						if (!string.IsNullOrEmpty(FlyoutPanel))
						{
							//
							// Create a flyout link
							//
							result = "<a class=\"" + FlyoutStyle + "\" " + MouseOutCode + " " + MouseOverCode + " " + MouseClickCode + " HREF=\"" + Link + "\">" + HotSpotHTML + "</a>";
							iMenuCloseString = iMenuCloseString + FlyoutPanel;
						}
						else if (!string.IsNullOrEmpty(Link))
						{
							//
							// Create a linked element
							//
							result = "<a class=\"" + FlyoutStyle + "\" " + MouseOutCode + " " + MouseOverCode + " " + MouseClickCode + " HREF=\"" + Link + "\">" + HotSpotHTML + "</a>";
						}
						else
						{
							//
							// no links and no flyouts, create just the caption
							//
						}
						//
						// Add in the inline java code if required
						//
						if (!string.IsNullOrEmpty(JavaCode))
						{
							result = ""
								+ "<SCRIPT language=javascript type=text/javascript>"
								+ JavaCode + "</script>"
								+ result;
						}
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//
		//===============================================================================
		//   Gets the Menu Branch for the Default Menu
		//===============================================================================
		//
		private string GetMenuFlyoutPanel(string PanelName, string UsedEntries, string StyleSheetPrefix, bool FlyoutHover, int PanelButtonCount)
		{
				string tempGetMenuFlyoutPanel = null;
			try
			{
				//
				int EntryPointer = 0;
				string iUsedEntries = null;
				string SubMenuName = null;
				int SubMenuCount = 0;
				string target = null;
				string SubMenus = "";
				string PanelChildren = null;
				string PanelButtons = "";
				string PanelButtonStyle = null;
				string HotSpotHTML = null;
				//
				iUsedEntries = UsedEntries;
				for (EntryPointer = 0; EntryPointer < iEntryCount; EntryPointer++)
				{
					if ((iEntry[EntryPointer].ParentName == PanelName) && (!string.IsNullOrEmpty(iEntry[EntryPointer].Caption)))
					{
						if (!(iUsedEntries + ",".IndexOf("," + EntryPointer + ",") + 1 < 0))
						{
							PanelButtonCount = PanelButtonCount + 1;
							iUsedEntries = iUsedEntries + "," + EntryPointer;
							PanelButtonStyle = StyleSheetPrefix + "PanelButton";
							target = "";
							if (iEntry[EntryPointer].NewWindow)
							{
								target = " target=\"_blank\"";
							}
							PanelChildren = GetMenuFlyoutPanel(iEntry[EntryPointer].Name, iUsedEntries, StyleSheetPrefix, FlyoutHover, PanelButtonCount);
							if (!string.IsNullOrEmpty(iEntry[EntryPointer].Image))
							{
								HotSpotHTML = "<img src=\"" + iEntry[EntryPointer].Image + "\" border=\"0\" alt=\"" + iEntry[EntryPointer].Caption + "\">";
							}
							else
							{
								HotSpotHTML = iEntry[EntryPointer].Caption;
							}
							if (string.IsNullOrEmpty(PanelChildren))
							{
								if (string.IsNullOrEmpty(iEntry[EntryPointer].Link))
								{
									//
									// ----- no link and no child panel
									//
									//PanelButtons = PanelButtons & "<SPAN class=""" & PanelButtonStyle & """>" & HotSpotHTML & "</SPAN>"
								}
								else
								{
									//
									// ----- Link but no child panel
									//
									PanelButtons = PanelButtons + "<a class=\"" + PanelButtonStyle + "\" href=\"" + genericController.encodeHTML(iEntry[EntryPointer].Link) + "\"" + target + " onmouseover=\"ccFlyoutHoverMode(1); ccFlyoutPanelButtonHover(event,'','" + StyleSheetPrefix + "');\">" + HotSpotHTML + "</a>";
								}
							}
							else
							{
								if (string.IsNullOrEmpty(iEntry[EntryPointer].Link))
								{
									//
									// ----- Child Panel and no link, block the href so menu "parent" buttons will not be clickable
									//
									if (FlyoutHover)
									{
										PanelButtons = PanelButtons + "<a class=\"" + PanelButtonStyle + "\" onmouseover=\"ccFlyoutHoverMode(1); ccFlyoutPanelButtonHover(event,'" + MenuFlyoutNamePrefix + "_" + iEntry[EntryPointer].Name + "','" + StyleSheetPrefix + "');\" onmouseout=\"ccFlyoutHoverMode(0);\" onclick=\"return false;\" href=\"#\"" + target + ">" + HotSpotHTML + MenuFlyoutIcon_Local + "</a>";
									}
									else
									{
										PanelButtons = PanelButtons + "<a class=\"" + PanelButtonStyle + "\" onmouseover=\"ccFlyoutPanelButtonHover(event,'" + MenuFlyoutNamePrefix + "_" + iEntry[EntryPointer].Name + "','" + StyleSheetPrefix + "');\" onclick=\"return false;\" href=\"#\"" + target + ">" + HotSpotHTML + MenuFlyoutIcon_Local + "</a>";
									}
								}
								else
								{
									//
									// ----- Child Panel and a link
									//
									if (FlyoutHover)
									{
										PanelButtons = PanelButtons + "<a class=\"" + PanelButtonStyle + "\" onmouseover=\"ccFlyoutHoverMode(1); ccFlyoutPanelButtonHover(event,'" + MenuFlyoutNamePrefix + "_" + iEntry[EntryPointer].Name + "','" + StyleSheetPrefix + "');\" onmouseout=\"ccFlyoutHoverMode(0);\" href=\"" + genericController.encodeHTML(iEntry[EntryPointer].Link) + "\"" + target + ">" + HotSpotHTML + MenuFlyoutIcon_Local + "</a>";
									}
									else
									{
										PanelButtons = PanelButtons + "<a class=\"" + PanelButtonStyle + "\" onmouseover=\"ccFlyoutPanelButtonHover(event,'" + MenuFlyoutNamePrefix + "_" + iEntry[EntryPointer].Name + "','" + StyleSheetPrefix + "');\" href=\"" + genericController.encodeHTML(iEntry[EntryPointer].Link) + "\"" + target + ">" + HotSpotHTML + MenuFlyoutIcon_Local + "</a>";
									}
								}
							}
							SubMenus = SubMenus + PanelChildren;
						}
					}
				}
				if (!string.IsNullOrEmpty(PanelButtons))
				{
					//
					// ----- If panel buttons are returned, wrap them in a DIV
					//
					if (FlyoutHover)
					{
						tempGetMenuFlyoutPanel = "<div style=\"position: absolute; left: 0px;visibility:hidden;\" class=\"kmaMenu " + StyleSheetPrefix + "Panel\" id=\"" + MenuFlyoutNamePrefix + "_" + PanelName + "\" onmouseover=\"ccFlyoutHoverMode(1); ccFlyoutPanelHover(event,'" + StyleSheetPrefix + "');\" onmouseout=\"ccFlyoutHoverMode(0);\">"
							+ PanelButtons + SubMenus + "</div>"
							+ "";
					}
					else
					{
						tempGetMenuFlyoutPanel = "<div style=\"position: absolute; left: 0px;visibility:hidden;\" class=\"kmaMenu " + StyleSheetPrefix + "Panel\" id=\"" + MenuFlyoutNamePrefix + "_" + PanelName + "\" onmouseover=\"ccFlyoutPanelHover(event,'" + StyleSheetPrefix + "')\">"
							+ PanelButtons + SubMenus + "</div>"
							+ "";
					}
				}
				//
				return tempGetMenuFlyoutPanel;
				//
			}
			catch
			{
				cpCore.handleException( ex );
			}
//ErrorTrap:
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
			handleLegacyClassError("GetMenuFlyoutPanel", Microsoft.VisualBasic.Information.Err().Number, Microsoft.VisualBasic.Information.Err().Source, Microsoft.VisualBasic.Information.Err().Description);
			return tempGetMenuFlyoutPanel;
		}
		//
		//========================================================================
		//
		//========================================================================
		//
		public string MenuFlyoutIcon
		{
			get
			{
				return MenuFlyoutIcon_Local;
			}
			set
			{
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
		private void handleLegacyClassError(string MethodName, int ErrNumber, string ErrSource, string ErrDescription)
		{
            throw (new Exception("unexpected error in method [" + MethodName + "], errDescription [" + ErrDescription + "]"));
        }
        //
        //========================================================================
        // ----- Add a new DHTML menu entry
        //========================================================================
        //
        public void menu_AddEntry(string Name, string ParentName = "", string ImageLink = "", string ImageOverLink = "", string Link = "", string Caption = "", string StyleSheet = "", string StyleSheetHover = "", bool NewWindow = false)
		{
            string Image = null;
            string ImageOver = string.Empty;
            //
            Image = genericController.encodeText(ImageLink);
            if (!string.IsNullOrEmpty(Image)) {
                ImageOver = genericController.encodeText(ImageOverLink);
                if (Image == ImageOver) {
                    ImageOver = "";
                }
            }
            cpCore.menuFlyout.AddEntry(genericController.encodeText(Name), ParentName, Image, ImageOver, Link, Caption,"", NewWindow);
        }
        //
        //========================================================================
        // ----- main_Get all the menu close scripts
        //
        //   call this at the end of the page
        //========================================================================
        //
        public string menu_GetClose()
		{
			string result = string.Empty;
			try
			{
				string MenuFlyoutIcon = null;
				//
				if (cpCore.menuFlyout != null)
				{
					cpCore.doc.menuSystemCloseCount = cpCore.doc.menuSystemCloseCount + 1;
					result = result + cpCore.menuFlyout.GetMenuClose();
					MenuFlyoutIcon = cpCore.siteProperties.getText("MenuFlyoutIcon", "&#187;");
					if (MenuFlyoutIcon != "&#187;")
					{
						result = genericController.vbReplace(result, "&#187;</a>", MenuFlyoutIcon + "</a>");
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
	}
}