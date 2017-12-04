using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.BaseClasses;
using Contensive.Core.Controllers;
using Contensive.Core.Controllers.genericController;
using Contensive.Core.Models.Entity;

namespace Contensive.Core.Controllers
{
	/// <summary>
	/// Tools used to assemble html document elements. This is not a storage for assembling a document (see docController)
	/// </summary>
	public class htmlController
	{
		//
		//========================================================================
		// main_GetRecordEditLink2( iContentName, iRecordID, AllowCut, RecordName )
		//
		//   ContentName The content for this link
		//   RecordID    The ID of the record in the Table
		//   AllowCut
		//   RecordName
		//   IsEditing
		//========================================================================
		//
		public string main_GetRecordEditLink2(string ContentName, int RecordID, bool AllowCut, string RecordName, bool IsEditing)
		{
				string tempmain_GetRecordEditLink2 = null;
			try
			{
				//
				//If Not (true) Then Exit Function
				//
				int CS = 0;
				string SQL = null;
				int ContentID = 0;
				string Link = null;
				string MethodName = null;
				string iContentName = null;
				int iRecordID = 0;
				string RootEntryName = null;
				string ClipBoard = null;
				string WorkingLink = null;
				bool iAllowCut = false;
				string Icon = null;
				string ContentCaption = null;
				//
				iContentName = genericController.encodeText(ContentName);
				iRecordID = genericController.EncodeInteger(RecordID);
				iAllowCut = genericController.EncodeBoolean(AllowCut);
				ContentCaption = genericController.encodeHTML(iContentName);
				if (genericController.vbLCase(ContentCaption) == "aggregate functions")
				{
					ContentCaption = "Add-on";
				}
				if (genericController.vbLCase(ContentCaption) == "aggregate function objects")
				{
					ContentCaption = "Add-on";
				}
				ContentCaption = ContentCaption + " record";
				if (!string.IsNullOrEmpty(RecordName))
				{
					ContentCaption = ContentCaption + ", named '" + RecordName + "'";
				}
				//
				MethodName = "main_GetRecordEditLink2";
				//
				tempmain_GetRecordEditLink2 = "";
				if (string.IsNullOrEmpty(iContentName))
				{
					throw (new ApplicationException("ContentName [" + ContentName + "] is invalid")); // handleLegacyError14(MethodName, "")
				}
				else
				{
					if (iRecordID < 1)
					{
						throw (new ApplicationException("RecordID [" + RecordID + "] is invalid")); // handleLegacyError14(MethodName, "")
					}
					else
					{
						if (IsEditing)
						{
							//
							// Edit link, main_Get the CID
							//
							ContentID = Models.Complex.cdefModel.getContentId(cpCore, iContentName);
							//
							tempmain_GetRecordEditLink2 = tempmain_GetRecordEditLink2 + "<a"
								+ " class=\"ccRecordEditLink\" "
								+ " TabIndex=-1"
								+ " href=\"" + genericController.encodeHTML("/" + cpCore.serverConfig.appConfig.adminRoute + "?cid=" + ContentID + "&id=" + iRecordID + "&af=4&aa=2&ad=1") + "\"";
							tempmain_GetRecordEditLink2 = tempmain_GetRecordEditLink2 + "><img"
								+ " src=\"/ccLib/images/IconContentEdit.gif\""
								+ " border=\"0\""
								+ " alt=\"Edit this " + genericController.encodeHTML(ContentCaption) + "\""
								+ " title=\"Edit this " + genericController.encodeHTML(ContentCaption) + "\""
								+ " align=\"absmiddle\""
								+ "></a>";
							//
							// Cut Link if enabled
							//
							if (iAllowCut)
							{
								WorkingLink = genericController.modifyLinkQuery(cpCore.webServer.requestPage + "?" + cpCore.doc.refreshQueryString, RequestNameCut, genericController.encodeText(ContentID) + "." + genericController.encodeText(RecordID), true);
								tempmain_GetRecordEditLink2 = ""
									+ tempmain_GetRecordEditLink2 + "<a class=\"ccRecordCutLink\" TabIndex=\"-1\" href=\"" + genericController.encodeHTML(WorkingLink) + "\"><img src=\"/ccLib/images/Contentcut.gif\" border=\"0\" alt=\"Cut this " + ContentCaption + " to clipboard\" title=\"Cut this " + ContentCaption + " to clipboard\" align=\"absmiddle\"></a>";
							}
							//
							// Help link if enabled
							//
							string helpLink = "";
							//helpLink = main_GetHelpLink(5, "Editing " & ContentCaption, "Turn on Edit icons by checking 'Edit' in the tools panel, and click apply.<br><br><img src=""/ccLib/images/IconContentEdit.gif"" style=""vertical-align:middle""> Edit-Content icon<br><br>Edit-Content icons appear in your content. Click them to edit your content.")
							tempmain_GetRecordEditLink2 = ""
								+ tempmain_GetRecordEditLink2 + helpLink;
							//
							tempmain_GetRecordEditLink2 = "<span class=\"ccRecordLinkCon\" style=\"white-space:nowrap;\">" + tempmain_GetRecordEditLink2 + "</span>";
							//'
							//main_GetRecordEditLink2 = "" _
							//    & cr & "<div style=""position:absolute;"">" _
							//    & genericController.kmaIndent(main_GetRecordEditLink2) _
							//    & cr & "</div>"
							//
							//main_GetRecordEditLink2 = "" _
							//    & cr & "<div style=""position:relative;display:inline;"">" _
							//    & genericController.kmaIndent(main_GetRecordEditLink2) _
							//    & cr & "</div>"
						}

					}
				}
				//
				return tempmain_GetRecordEditLink2;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError18(MethodName)
			//
			return tempmain_GetRecordEditLink2;
		}
		//
		//========================================================================
		// Print an add link for the current ContentSet
		//   iCSPointer is the content set to be added to
		//   PresetNameValueList is a name=value pair to force in the added record
		//========================================================================
		//
		public string main_cs_getRecordAddLink(int CSPointer, string PresetNameValueList = "", bool AllowPaste = false)
		{
				string tempmain_cs_getRecordAddLink = null;
			try
			{
				//
				//If Not (true) Then Exit Function
				//
				string ContentName = null;
				string iPresetNameValueList = null;
				string MethodName = null;
				int iCSPointer;
				//
				iCSPointer = genericController.EncodeInteger(CSPointer);
				iPresetNameValueList = genericController.encodeEmptyText(PresetNameValueList, "");
				//
				MethodName = "main_cs_getRecordAddLink";
				//
				if (iCSPointer < 0)
				{
					throw (new ApplicationException("invalid ContentSet pointer [" + iCSPointer + "]")); // handleLegacyError14(MethodName, "main_cs_getRecordAddLink was called with ")
				}
				else
				{
					//
					// Print an add tag to the iCSPointers Content
					//
					ContentName = cpCore.db.cs_getContentName(iCSPointer);
					if (string.IsNullOrEmpty(ContentName))
					{
						throw (new ApplicationException("main_cs_getRecordAddLink was called with a ContentSet that was created with an SQL statement. The function requires a ContentSet opened with an OpenCSContent.")); // handleLegacyError14(MethodName, "")
					}
					else
					{
						tempmain_cs_getRecordAddLink = main_GetRecordAddLink(ContentName, iPresetNameValueList, AllowPaste);
					}
				}
				return tempmain_cs_getRecordAddLink;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError18(MethodName)
			//
			return tempmain_cs_getRecordAddLink;
		}
		//
		//========================================================================
		// main_GetRecordAddLink( iContentName, iPresetNameValueList )
		//
		//   Returns a string of add tags for the Content Definition included, and all
		//   child contents of that area.
		//
		//   iContentName The content for this link
		//   iPresetNameValueList The sql equivalent used to select the record.
		//           translates to name0=value0,name1=value1.. pairs separated by ,
		//
		//   LowestRootMenu - The Menu in the flyout structure that is the furthest down
		//   in the chain that the user has content access to. This is so a content manager
		//   does not have to navigate deep into a structure to main_Get to content he can
		//   edit.
		//   Basically, the entire menu is created down from the MenuName, and populated
		//   with all the entiries this user has access to. The LowestRequiredMenuName is
		//   is returned from the _branch routine, and that is to root on-which the
		//   main_GetMenu uses
		//========================================================================
		//
		public string main_GetRecordAddLink(string ContentName, string PresetNameValueList, bool AllowPaste = false)
		{
			return main_GetRecordAddLink2(genericController.encodeText(ContentName), genericController.encodeText(PresetNameValueList), AllowPaste, cpCore.doc.authContext.isEditing(ContentName));
		}
		//
		//========================================================================
		// main_GetRecordAddLink2
		//
		//   Returns a string of add tags for the Content Definition included, and all
		//   child contents of that area.
		//
		//   iContentName The content for this link
		//   iPresetNameValueList The sql equivalent used to select the record.
		//           translates to name0=value0,name1=value1.. pairs separated by ,
		//
		//   LowestRootMenu - The Menu in the flyout structure that is the furthest down
		//   in the chain that the user has content access to. This is so a content manager
		//   does not have to navigate deep into a structure to main_Get to content he can
		//   edit.
		//   Basically, the entire menu is created down from the MenuName, and populated
		//   with all the entiries this user has access to. The LowestRequiredMenuName is
		//   is returned from the _branch routine, and that is to root on-which the
		//   main_GetMenu uses
		//========================================================================
		//
		public string main_GetRecordAddLink2(string ContentName, string PresetNameValueList, bool AllowPaste, bool IsEditing)
		{
				string tempmain_GetRecordAddLink2 = null;
			try
			{
				//
				//If Not (true) Then Exit Function
				//
				int ParentID = 0;
				string BufferString = null;
				string MethodName = null;
				string iContentName = null;
				int iContentID = 0;
				string iPresetNameValueList = null;
				string MenuName = null;
				bool MenuHasBranches = false;
				string LowestRequiredMenuName = string.Empty;
				string ClipBoard = null;
				string PasteLink = string.Empty;
				int Position = 0;
				string[] ClipBoardArray = null;
				int ClipboardContentID = 0;
				int ClipChildRecordID = 0;
				bool iAllowPaste = false;
				bool useFlyout = false;
				int csChildContent = 0;
				string Link = null;
				//
				MethodName = "main_GetRecordAddLink";
				//
				tempmain_GetRecordAddLink2 = "";
				if (IsEditing)
				{
					iContentName = genericController.encodeText(ContentName);
					iPresetNameValueList = genericController.encodeText(PresetNameValueList);
					iPresetNameValueList = genericController.vbReplace(iPresetNameValueList, "&", ",");
					iAllowPaste = genericController.EncodeBoolean(AllowPaste);

					if (string.IsNullOrEmpty(iContentName))
					{
						throw (new ApplicationException("Method called with blank ContentName")); // handleLegacyError14(MethodName, "")
					}
					else
					{
						iContentID = Models.Complex.cdefModel.getContentId(cpCore, iContentName);
						csChildContent = cpCore.db.csOpen("Content", "ParentID=" + iContentID,,,,,, "id");
						useFlyout = cpCore.db.csOk(csChildContent);
						cpCore.db.csClose(csChildContent);
						//
						if (!useFlyout)
						{
							Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?cid=" + iContentID + "&af=4&aa=2&ad=1";
							if (!string.IsNullOrEmpty(PresetNameValueList))
							{
								Link = Link + "&wc=" + genericController.EncodeRequestVariable(PresetNameValueList);
							}
							tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + "<a"
								+ " TabIndex=-1"
								+ " href=\"" + genericController.encodeHTML(Link) + "\"";
							tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + "><img"
								+ " src=\"/ccLib/images/IconContentAdd.gif\""
								+ " border=\"0\""
								+ " alt=\"Add record\""
								+ " title=\"Add record\""
								+ " align=\"absmiddle\""
								+ "></a>";
						}
						else
						{
							//
							MenuName = genericController.GetRandomInteger().ToString();
							cpCore.menuFlyout.menu_AddEntry(MenuName,, "/ccLib/images/IconContentAdd.gif",,,, "stylesheet", "stylesheethover");
							LowestRequiredMenuName = main_GetRecordAddLink_AddMenuEntry(iContentName, iPresetNameValueList, "", MenuName, MenuName);
						}
						//
						// Add in the paste entry, if needed
						//
						if (iAllowPaste)
						{
							ClipBoard = cpCore.visitProperty.getText("Clipboard", "");
							if (!string.IsNullOrEmpty(ClipBoard))
							{
								Position = genericController.vbInstr(1, ClipBoard, ".");
								if (Position != 0)
								{
									ClipBoardArray = ClipBoard.Split('.');
									if (ClipBoardArray.GetUpperBound(0) > 0)
									{
										ClipboardContentID = genericController.EncodeInteger(ClipBoardArray[0]);
										ClipChildRecordID = genericController.EncodeInteger(ClipBoardArray[1]);
										//iContentID = main_GetContentID(iContentName)
										if (Models.Complex.cdefModel.isWithinContent(cpCore, ClipboardContentID, iContentID))
										{
											if (genericController.vbInstr(1, iPresetNameValueList, "PARENTID=", Microsoft.VisualBasic.Constants.vbTextCompare) != 0)
											{
												//
												// must test for main_IsChildRecord
												//
												BufferString = iPresetNameValueList;
												BufferString = genericController.vbReplace(BufferString, "(", "");
												BufferString = genericController.vbReplace(BufferString, ")", "");
												BufferString = genericController.vbReplace(BufferString, ",", "&");
												ParentID = genericController.EncodeInteger(genericController.main_GetNameValue_Internal(cpCore, BufferString, "Parentid"));
											}


											if ((ParentID != 0) & (!pageContentController.isChildRecord(cpCore, iContentName, ParentID, ClipChildRecordID)))
											{
												//
												// Can not paste as child of itself
												//
												PasteLink = cpCore.webServer.requestPage + "?" + cpCore.doc.refreshQueryString;
												PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePaste, "1", true);
												PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteParentContentID, iContentID.ToString(), true);
												PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteParentRecordID, ParentID.ToString(), true);
												PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteFieldList, iPresetNameValueList, true);
												tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + "<a class=\"ccRecordCutLink\" TabIndex=\"-1\" href=\"" + genericController.encodeHTML(PasteLink) + "\"><img src=\"/ccLib/images/ContentPaste.gif\" border=\"0\" alt=\"Paste record in clipboard here\" title=\"Paste record in clipboard here\" align=\"absmiddle\"></a>";
											}
										}
									}
								}
							}
						}
						//
						// Add in the available flyout Navigator Entries
						//
						if (!string.IsNullOrEmpty(LowestRequiredMenuName))
						{
							tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + cpCore.menuFlyout.getMenu(LowestRequiredMenuName, 0);
							tempmain_GetRecordAddLink2 = genericController.vbReplace(tempmain_GetRecordAddLink2, "class=\"ccFlyoutButton\" ", "", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
							if (!string.IsNullOrEmpty(PasteLink))
							{
								tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + "<a TabIndex=-1 href=\"" + genericController.encodeHTML(PasteLink) + "\"><img src=\"/ccLib/images/ContentPaste.gif\" border=\"0\" alt=\"Paste content from clipboard\" align=\"absmiddle\"></a>";
							}
						}
						//
						// Help link if enabled
						//
						string helpLink = "";
						//helpLink = main_GetHelpLink(6, "Adding " & iContentName, "Turn on Edit icons by checking 'Edit' in the tools panel, and click apply.<br><br><img src=""/ccLib/images/IconContentAdd.gif"" " & IconWidthHeight & " style=""vertical-align:middle""> Add-Content icon<br><br>Add-Content icons appear in your content. Click them to add content.")
						tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + helpLink;
						if (!string.IsNullOrEmpty(tempmain_GetRecordAddLink2))
						{
							tempmain_GetRecordAddLink2 = ""
								+ Environment.NewLine + "\t" + "<div style=\"display:inline;\">"
								+ genericController.htmlIndent(tempmain_GetRecordAddLink2) + Environment.NewLine + "\t" + "</div>";
						}
						//
						// ----- Add the flyout panels to the content to return
						//       This must be here so if the call is made after main_ClosePage, the panels will still deliver
						//
						if (!string.IsNullOrEmpty(LowestRequiredMenuName))
						{
							tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + cpCore.menuFlyout.menu_GetClose();
							if (genericController.vbInstr(1, tempmain_GetRecordAddLink2, "IconContentAdd.gif", Microsoft.VisualBasic.Constants.vbTextCompare) != 0)
							{
								tempmain_GetRecordAddLink2 = genericController.vbReplace(tempmain_GetRecordAddLink2, "IconContentAdd.gif\" ", "IconContentAdd.gif\" align=\"absmiddle\" ");
							}
						}
						tempmain_GetRecordAddLink2 = genericController.vbReplace(tempmain_GetRecordAddLink2, "target=", "xtarget=", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
					}
				}
				//
				return tempmain_GetRecordAddLink2;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError18(MethodName)
			//
			return tempmain_GetRecordAddLink2;
		}
		//
		//========================================================================
		// main_GetRecordAddLink_AddMenuEntry( ContentName, PresetNameValueList, ContentNameList, MenuName )
		//
		//   adds an add entry for the content name, and all the child content
		//   returns the MenuName of the lowest branch that has valid
		//   Navigator Entries.
		//
		//   ContentName The content for this link
		//   PresetNameValueList The sql equivalent used to select the record.
		//           translates to (name0=value0)&(name1=value1).. pairs separated by &
		//   ContentNameList is a comma separated list of names of the content included so far
		//   MenuName is the name of the root branch, for flyout menu
		//
		//   IsMember(), main_IsAuthenticated() And Member_AllowLinkAuthoring must already be checked
		//========================================================================
		//
		private string main_GetRecordAddLink_AddMenuEntry(string ContentName, string PresetNameValueList, string ContentNameList, string MenuName, string ParentMenuName)
		{
			string result = "";
			string Copy = null;
			int CS = 0;
			string SQL = null;
			int csChildContent = 0;
			int ContentID = 0;
			string Link = null;
			string MyContentNameList = null;
			string ButtonCaption = null;
			bool ContentRecordFound = false;
			bool ContentAllowAdd = false;
			bool GroupRulesAllowAdd = false;
			DateTime MemberRulesDateExpires = default(DateTime);
			bool MemberRulesAllow = false;
			int ChildMenuButtonCount = 0;
			string ChildMenuName = null;
			string ChildContentName = null;
			//
			Link = "";
			MyContentNameList = ContentNameList;
			if (string.IsNullOrEmpty(ContentName))
			{
				throw (new ApplicationException("main_GetRecordAddLink, ContentName is empty")); // handleLegacyError14(MethodName, "")
			}
			else
			{
				if (MyContentNameList.IndexOf("," + genericController.vbUCase(ContentName) + ",") + 1 >= 0)
				{
					throw (new ApplicationException("result , Content Child [" + ContentName + "] is one of its own parents")); // handleLegacyError14(MethodName, "")
				}
				else
				{
					MyContentNameList = MyContentNameList + "," + genericController.vbUCase(ContentName) + ",";
					//
					// ----- Select the Content Record for the Menu Entry selected
					//
					ContentRecordFound = false;
					if (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore))
					{
						//
						// ----- admin member, they have access, main_Get ContentID and set markers true
						//
						SQL = "SELECT ID as ContentID, AllowAdd as ContentAllowAdd, 1 as GroupRulesAllowAdd, null as MemberRulesDateExpires"
							+ " FROM ccContent"
							+ " WHERE ("
							+ " (ccContent.Name=" + cpCore.db.encodeSQLText(ContentName) + ")"
							+ " AND(ccContent.active<>0)"
							+ " );";
						CS = cpCore.db.csOpenSql(SQL);
						if (cpCore.db.csOk(CS))
						{
							//
							// Entry was found
							//
							ContentRecordFound = true;
							ContentID = cpCore.db.csGetInteger(CS, "ContentID");
							ContentAllowAdd = cpCore.db.csGetBoolean(CS, "ContentAllowAdd");
							GroupRulesAllowAdd = true;
							MemberRulesDateExpires = DateTime.MinValue;
							MemberRulesAllow = true;
						}
						cpCore.db.csClose(CS);
					}
					else
					{
						//
						// non-admin member, first check if they have access and main_Get true markers
						//
						SQL = "SELECT ccContent.ID as ContentID, ccContent.AllowAdd as ContentAllowAdd, ccGroupRules.AllowAdd as GroupRulesAllowAdd, ccMemberRules.DateExpires as MemberRulesDateExpires"
							+ " FROM (((ccContent"
								+ " LEFT JOIN ccGroupRules ON ccGroupRules.ContentID=ccContent.ID)"
								+ " LEFT JOIN ccgroups ON ccGroupRules.GroupID=ccgroups.ID)"
								+ " LEFT JOIN ccMemberRules ON ccgroups.ID=ccMemberRules.GroupID)"
								+ " LEFT JOIN ccMembers ON ccMemberRules.MemberID=ccMembers.ID"
							+ " WHERE ("
							+ " (ccContent.Name=" + cpCore.db.encodeSQLText(ContentName) + ")"
							+ " AND(ccContent.active<>0)"
							+ " AND(ccGroupRules.active<>0)"
							+ " AND(ccMemberRules.active<>0)"
							+ " AND((ccMemberRules.DateExpires is Null)or(ccMemberRules.DateExpires>" + cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime) + "))"
							+ " AND(ccgroups.active<>0)"
							+ " AND(ccMembers.active<>0)"
							+ " AND(ccMembers.ID=" + cpCore.doc.authContext.user.id + ")"
							+ " );";
						CS = cpCore.db.csOpenSql(SQL);
						if (cpCore.db.csOk(CS))
						{
							//
							// ----- Entry was found, member has some kind of access
							//
							ContentRecordFound = true;
							ContentID = cpCore.db.csGetInteger(CS, "ContentID");
							ContentAllowAdd = cpCore.db.csGetBoolean(CS, "ContentAllowAdd");
							GroupRulesAllowAdd = cpCore.db.csGetBoolean(CS, "GroupRulesAllowAdd");
							MemberRulesDateExpires = cpCore.db.csGetDate(CS, "MemberRulesDateExpires");
							MemberRulesAllow = false;
							if (MemberRulesDateExpires == DateTime.MinValue)
							{
								MemberRulesAllow = true;
							}
							else if (MemberRulesDateExpires > cpCore.doc.profileStartTime)
							{
								MemberRulesAllow = true;
							}
						}
						else
						{
							//
							// ----- No entry found, this member does not have access, just main_Get ContentID
							//
							ContentRecordFound = true;
							ContentID = Models.Complex.cdefModel.getContentId(cpCore, ContentName);
							ContentAllowAdd = false;
							GroupRulesAllowAdd = false;
							MemberRulesAllow = false;
						}
						cpCore.db.csClose(CS);
					}
					if (ContentRecordFound)
					{
						//
						// Add the Menu Entry* to the current menu (MenuName)
						//
						Link = "";
						ButtonCaption = ContentName;
						result = MenuName;
						if (ContentAllowAdd && GroupRulesAllowAdd && MemberRulesAllow)
						{
							Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?cid=" + ContentID + "&af=4&aa=2&ad=1";
							if (!string.IsNullOrEmpty(PresetNameValueList))
							{
								string NameValueList = PresetNameValueList;
								Link = Link + "&wc=" + genericController.EncodeRequestVariable(PresetNameValueList);
							}
						}
						cpCore.menuFlyout.menu_AddEntry(MenuName + ":" + ContentName, ParentMenuName,,, Link, ButtonCaption, "", "", true);
						//
						// Create child submenu if Child Entries found
						//
						csChildContent = cpCore.db.csOpen("Content", "ParentID=" + ContentID,,,,,, "name");
						if (!cpCore.db.csOk(csChildContent))
						{
							//
							// No child menu
							//
						}
						else
						{
							//
							// Add the child menu
							//
							ChildMenuName = MenuName + ":" + ContentName;
							ChildMenuButtonCount = 0;
							//
							// ----- Create the ChildPanel with all Children found
							//
							while (cpCore.db.csOk(csChildContent))
							{
								ChildContentName = cpCore.db.csGetText(csChildContent, "name");
								Copy = main_GetRecordAddLink_AddMenuEntry(ChildContentName, PresetNameValueList, MyContentNameList, MenuName, ParentMenuName);
								if (!string.IsNullOrEmpty(Copy))
								{
									ChildMenuButtonCount = ChildMenuButtonCount + 1;
								}
								if ((string.IsNullOrEmpty(result)) && (!string.IsNullOrEmpty(Copy)))
								{
									result = Copy;
								}
								cpCore.db.csGoNext(csChildContent);
							}
						}
					}
				}
				cpCore.db.csClose(csChildContent);
			}
			return result;
		}
		//
		//========================================================================
		//   main_GetPanel( Panel, Optional StylePanel, Optional StyleHilite, Optional StyleShadow, Optional Width, Optional Padding, Optional HeightMin) As String
		// Return a panel with the input as center
		//========================================================================
		//
		public string main_GetPanel(string Panel, string StylePanel = "", string StyleHilite = "ccPanelHilite", string StyleShadow = "ccPanelShadow", string Width = "100%", int Padding = 5, int HeightMin = 1)
		{
			string tempmain_GetPanel = null;
			string ContentPanelWidth = null;
			string MethodName = null;
			string MyStylePanel = null;
			string MyStyleHilite = null;
			string MyStyleShadow = null;
			string MyWidth = null;
			string MyPadding = null;
			string MyHeightMin = null;
			string s0 = null;
			string s1 = null;
			string s2 = null;
			string s3 = null;
			string s4 = null;
			string contentPanelWidthStyle = null;
			//
			MethodName = "main_GetPanelTop";
			//
			MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel");
			MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite");
			MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow");
			MyWidth = genericController.encodeEmptyText(Width, "100%");
			MyPadding = Padding.ToString();
			MyHeightMin = HeightMin.ToString();
			//
			if (genericController.vbIsNumeric(MyWidth))
			{
				ContentPanelWidth = (int.Parse(MyWidth) - 2).ToString();
				contentPanelWidthStyle = ContentPanelWidth + "px";
			}
			else
			{
				ContentPanelWidth = "100%";
				contentPanelWidthStyle = ContentPanelWidth;
			}
			//
			//
			//
			s0 = ""
				+ "\r" + "<td style=\"padding:" + MyPadding + "px;vertical-align:top\" class=\"" + MyStylePanel + "\">"
				+ genericController.htmlIndent(genericController.encodeText(Panel)) + "\r" + "</td>"
				+ "";
			//
			s1 = ""
				+ "\r" + "<tr>"
				+ genericController.htmlIndent(s0) + "\r" + "</tr>"
				+ "";
			s2 = ""
				+ "\r" + "<table style=\"width:" + contentPanelWidthStyle + ";border:0px;\" class=\"" + MyStylePanel + "\" cellspacing=\"0\">"
				+ genericController.htmlIndent(s1) + "\r" + "</table>"
				+ "";
			s3 = ""
				+ "\r" + "<td width=\"1\" height=\"" + MyHeightMin + "\" class=\"" + MyStyleHilite + "\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"" + MyHeightMin + "\" width=\"1\" ></td>"
				+ "\r" + "<td width=\"" + ContentPanelWidth + "\" valign=\"top\" align=\"left\" class=\"" + MyStylePanel + "\">"
				+ genericController.htmlIndent(s2) + "\r" + "</td>"
				+ "\r" + "<td width=\"1\" class=\"" + MyStyleShadow + "\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"1\" ></td>"
				+ "";
			s4 = ""
				+ "\r" + "<tr>"
				+ cr2 + "<td colspan=\"3\" class=\"" + MyStyleHilite + "\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"" + MyWidth + "\" ></td>"
				+ "\r" + "</tr>"
				+ "\r" + "<tr>"
				+ genericController.htmlIndent(s3) + "\r" + "</tr>"
				+ "\r" + "<tr>"
				+ cr2 + "<td colspan=\"3\" class=\"" + MyStyleShadow + "\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"" + MyWidth + "\" ></td>"
				+ "\r" + "</tr>"
				+ "";
			tempmain_GetPanel = ""
				+ "\r" + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"" + MyWidth + "\" class=\"" + MyStylePanel + "\">"
				+ genericController.htmlIndent(s4) + "\r" + "</table>"
				+ "";
				return tempmain_GetPanel;
		}
		//
		//========================================================================
		//   main_GetPanel( Panel, Optional StylePanel, Optional StyleHilite, Optional StyleShadow, Optional Width, Optional Padding, Optional HeightMin) As String
		// Return a panel with the input as center
		//========================================================================
		//
		public string main_GetReversePanel(string Panel, string StylePanel = "", string StyleHilite = "ccPanelShadow", string StyleShadow = "ccPanelHilite", string Width = "", string Padding = "", string HeightMin = "")
		{
			string MyStyleHilite = null;
			string MyStyleShadow = null;
			//
			MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelShadow");
			MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelHilite");

			return main_GetPanelTop(StylePanel, MyStyleHilite, MyStyleShadow, Width, Padding, HeightMin) + genericController.encodeText(Panel) + main_GetPanelBottom(StylePanel, MyStyleHilite, MyStyleShadow, Width, Padding);
		}
		//
		//========================================================================
		// Return a panel header with the header message reversed out of the left
		//========================================================================
		//
		public string main_GetPanelHeader(string HeaderMessage, string RightSideMessage = "")
		{
			string iHeaderMessage = null;
			string iRightSideMessage = null;
			adminUIController Adminui = new adminUIController(cpCore);
			//
			//If Not (true) Then Exit Function
			//
			iHeaderMessage = genericController.encodeText(HeaderMessage);
			iRightSideMessage = genericController.encodeEmptyText(RightSideMessage, cpCore.doc.profileStartTime.ToString("G"));
			return Adminui.GetHeader(iHeaderMessage, iRightSideMessage);
		}

		//
		//========================================================================
		// Prints the top of display panel
		//   Must be closed with PrintPanelBottom
		//========================================================================
		//
		public string main_GetPanelTop(string StylePanel = "", string StyleHilite = "", string StyleShadow = "", string Width = "", string Padding = "", string HeightMin = "")
		{
			string tempmain_GetPanelTop = null;
			string ContentPanelWidth = null;
			string MethodName = null;
			string MyStylePanel = null;
			string MyStyleHilite = null;
			string MyStyleShadow = null;
			string MyWidth = null;
			string MyPadding = null;
			string MyHeightMin = null;
			//
			tempmain_GetPanelTop = "";
			MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel");
			MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite");
			MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow");
			MyWidth = genericController.encodeEmptyText(Width, "100%");
			MyPadding = genericController.encodeEmptyText(Padding, "5");
			MyHeightMin = genericController.encodeEmptyText(HeightMin, "1");
			MethodName = "main_GetPanelTop";
			if (genericController.vbIsNumeric(MyWidth))
			{
				ContentPanelWidth = (int.Parse(MyWidth) - 2).ToString();
			}
			else
			{
				ContentPanelWidth = "100%";
			}
			tempmain_GetPanelTop = tempmain_GetPanelTop + "\r" + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"" + MyWidth + "\" class=\"" + MyStylePanel + "\">";
			//
			// --- top hilite row
			//
			tempmain_GetPanelTop = tempmain_GetPanelTop + cr2 + "<tr>"
				+ cr3 + "<td colspan=\"3\" class=\"" + MyStyleHilite + "\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"" + MyWidth + "\" ></td>"
				+ cr2 + "</tr>";
			//
			// --- center row with Panel
			//
			tempmain_GetPanelTop = tempmain_GetPanelTop + cr2 + "<tr>"
				+ cr3 + "<td width=\"1\" height=\"" + MyHeightMin + "\" class=\"" + MyStyleHilite + "\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"" + MyHeightMin + "\" width=\"1\" ></td>"
				+ cr3 + "<td width=\"" + ContentPanelWidth + "\" valign=\"top\" align=\"left\" class=\"" + MyStylePanel + "\">"
				+ cr4 + "<table border=\"0\" cellpadding=\"" + MyPadding + "\" cellspacing=\"0\" width=\"" + ContentPanelWidth + "\" class=\"" + MyStylePanel + "\">"
				+ cr5 + "<tr>"
				+ cr6 + "<td valign=\"top\" class=\"" + MyStylePanel + "\"><Span class=\"" + MyStylePanel + "\">";
				return tempmain_GetPanelTop;
		}
		//
		//========================================================================
		// Return a panel with the input as center
		//========================================================================
		//
		public string main_GetPanelBottom(string StylePanel = "", string StyleHilite = "", string StyleShadow = "", string Width = "", string Padding = "")
		{
			string result = string.Empty;
			try
			{
				//Dim MyStylePanel As String
				//Dim MyStyleHilite As String
				string MyStyleShadow = null;
				string MyWidth = null;
				//Dim MyPadding As String
				//
				//MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel")
				//MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite")
				MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow");
				MyWidth = genericController.encodeEmptyText(Width, "100%");
				//MyPadding = genericController.encodeEmptyText(Padding, "5")
				//
				result = result + cr6 + "</span></td>"
					+ cr5 + "</tr>"
					+ cr4 + "</table>"
					+ cr3 + "</td>"
					+ cr3 + "<td width=\"1\" class=\"" + MyStyleShadow + "\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"1\" ></td>"
					+ cr2 + "</tr>"
					+ cr2 + "<tr>"
					+ cr3 + "<td colspan=\"3\" class=\"" + MyStyleShadow + "\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"" + MyWidth + "\" ></td>"
					+ cr2 + "</tr>"
					+ "\r" + "</table>";
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//
		//========================================================================
		//
		//========================================================================
		//
		public string main_GetPanelButtons(string ButtonValueList, string ButtonName, string PanelWidth = "", string PanelHeightMin = "")
		{
			adminUIController Adminui = new adminUIController(cpCore);
			return Adminui.GetButtonBar(Adminui.GetButtonsFromList(ButtonValueList, true, true, ButtonName), "");
		}
		//
		//
		//
		public string main_GetPanelRev(string PanelContent, string PanelWidth = "", string PanelHeightMin = "")
		{
			return main_GetPanel(PanelContent, "ccPanel", "ccPanelShadow", "ccPanelHilite", PanelWidth, 2, genericController.EncodeInteger(PanelHeightMin));
		}
		//
		//
		//
		public string main_GetPanelInput(string PanelContent, string PanelWidth = "", string PanelHeightMin = "1")
		{
			return main_GetPanel(PanelContent, "ccPanelInput", "ccPanelShadow", "ccPanelHilite", PanelWidth, 2, genericController.EncodeInteger(PanelHeightMin));
		}
		//
		//========================================================================
		// Print the tools panel at the bottom of the page
		//========================================================================
		//
		public string main_GetToolsPanel()
		{
			string result = string.Empty;
			try
			{
				string copyNameValue = null;
				string CopyName = null;
				string copyValue = null;
				string[] copyNameValueSplit = null;
				int VisitMin = 0;
				int VisitHrs = 0;
				int VisitSec = 0;
				string DebugPanel = string.Empty;
				string Copy = null;
				string[] CopySplit = null;
				int Ptr = 0;
				string EditTagID = null;
				string QuickEditTagID = null;
				string AdvancedEditTagID = null;
				string WorkflowTagID = null;
				string Tag = null;
				string MethodName = null;
				string TagID = null;
				stringBuilderLegacyController ToolsPanel = null;
				string OptionsPanel = string.Empty;
				stringBuilderLegacyController LinkPanel = null;
				string LoginPanel = string.Empty;
				bool iValueBoolean = false;
				string WorkingQueryString = null;
				string BubbleCopy = null;
				stringBuilderLegacyController AnotherPanel = null;
				adminUIController Adminui = new adminUIController(cpCore);
				bool ShowLegacyToolsPanel = false;
				string QS = null;
				//
				MethodName = "main_GetToolsPanel";
				//
				if (cpCore.doc.authContext.user.AllowToolsPanel)
				{
					ShowLegacyToolsPanel = cpCore.siteProperties.getBoolean("AllowLegacyToolsPanel", true);
					//
					// --- Link Panel - used for both Legacy Tools Panel, and without it
					//
					LinkPanel = new stringBuilderLegacyController();
					LinkPanel.Add(SpanClassAdminSmall);
					LinkPanel.Add("Contensive " + cpCore.codeVersion() + " | ");
					LinkPanel.Add(cpCore.doc.profileStartTime.ToString("G") + " | ");
					LinkPanel.Add("<a class=\"ccAdminLink\" target=\"_blank\" href=\"http://support.Contensive.com/\">Support</A> | ");
					LinkPanel.Add("<a class=\"ccAdminLink\" href=\"" + genericController.encodeHTML("/" + cpCore.serverConfig.appConfig.adminRoute) + "\">Admin Home</A> | ");
					LinkPanel.Add("<a class=\"ccAdminLink\" href=\"" + genericController.encodeHTML("http://" + cpCore.webServer.requestDomain) + "\">Public Home</A> | ");
					LinkPanel.Add("<a class=\"ccAdminLink\" target=\"_blank\" href=\"" + genericController.encodeHTML("/" + cpCore.serverConfig.appConfig.adminRoute + "?" + RequestNameHardCodedPage + "=" + HardCodedPageMyProfile) + "\">My Profile</A> | ");
					if (cpCore.siteProperties.getBoolean("AllowMobileTemplates", false))
					{
						if (cpCore.doc.authContext.visit.Mobile)
						{
							QS = cpCore.doc.refreshQueryString;
							QS = genericController.ModifyQueryString(QS, "method", "forcenonmobile");
							LinkPanel.Add("<a class=\"ccAdminLink\" href=\"?" + QS + "\">Non-Mobile Version</A> | ");
						}
						else
						{
							QS = cpCore.doc.refreshQueryString;
							QS = genericController.ModifyQueryString(QS, "method", "forcemobile");
							LinkPanel.Add("<a class=\"ccAdminLink\" href=\"?" + QS + "\">Mobile Version</A> | ");
						}
					}
					LinkPanel.Add("</span>");
					//
					if (ShowLegacyToolsPanel)
					{
						ToolsPanel = new stringBuilderLegacyController();
						WorkingQueryString = genericController.ModifyQueryString(cpCore.doc.refreshQueryString, "ma", "", false);
						//
						// ----- Tools Panel Caption
						//
						string helpLink = "";
						//helpLink = main_GetHelpLink("2", "Contensive Tools Panel", BubbleCopy)
						BubbleCopy = "Use the Tools Panel to enable features such as editing and debugging tools. It also includes links to the admin site, the support site and the My Profile page.";
						result = result + main_GetPanelHeader("Contensive Tools Panel" + helpLink);
						//
						ToolsPanel.Add(cpCore.html.html_GetFormStart(WorkingQueryString));
						ToolsPanel.Add(cpCore.html.html_GetFormInputHidden("Type", FormTypeToolsPanel));
						//
						if (true)
						{
							//
							// ----- Create the Options Panel
							//
							//PathsContentID = main_GetContentID("Paths")
							//                '
							//                ' Allow Help Links
							//                '
							//                iValueBoolean = visitProperty.getboolean("AllowHelpIcon")
							//                TagID =  "AllowHelpIcon"
							//                OptionsPanel = OptionsPanel & "" _
							//                    & CR & "<div class=""ccAdminSmall"">" _
							//                    & cr2 & "<LABEL for=""" & TagID & """>" & main_GetFormInputCheckBox2(TagID, iValueBoolean, TagID) & "&nbsp;Help</LABEL>" _
							//                    & CR & "</div>"
							//
							EditTagID = "AllowEditing";
							QuickEditTagID = "AllowQuickEditor";
							AdvancedEditTagID = "AllowAdvancedEditor";
							WorkflowTagID = "AllowWorkflowRendering";
							//
							// Edit
							//
							helpLink = "";
							//helpLink = main_GetHelpLink(7, "Enable Editing", "Display the edit tools for basic content, such as pages, copy and sections. ")
							iValueBoolean = cpCore.visitProperty.getBoolean("AllowEditing");
							Tag = cpCore.html.html_GetFormInputCheckBox2(EditTagID, iValueBoolean, EditTagID);
							Tag = genericController.vbReplace(Tag, ">", " onClick=\"document.getElementById('" + QuickEditTagID + "').checked=false;document.getElementById('" + AdvancedEditTagID + "').checked=false;\">");
							OptionsPanel = OptionsPanel + "\r" + "<div class=\"ccAdminSmall\">"
							+ cr2 + "<LABEL for=\"" + EditTagID + "\">" + Tag + "&nbsp;Edit</LABEL>" + helpLink + "\r" + "</div>";
							//
							// Quick Edit
							//
							helpLink = "";
							//helpLink = main_GetHelpLink(8, "Enable Quick Edit", "Display the quick editor to edit the main page content.")
							iValueBoolean = cpCore.visitProperty.getBoolean("AllowQuickEditor");
							Tag = cpCore.html.html_GetFormInputCheckBox2(QuickEditTagID, iValueBoolean, QuickEditTagID);
							Tag = genericController.vbReplace(Tag, ">", " onClick=\"document.getElementById('" + EditTagID + "').checked=false;document.getElementById('" + AdvancedEditTagID + "').checked=false;\">");
							OptionsPanel = OptionsPanel + "\r" + "<div class=\"ccAdminSmall\">"
							+ cr2 + "<LABEL for=\"" + QuickEditTagID + "\">" + Tag + "&nbsp;Quick Edit</LABEL>" + helpLink + "\r" + "</div>";
							//
							// Advanced Edit
							//
							helpLink = "";
							//helpLink = main_GetHelpLink(0, "Enable Advanced Edit", "Display the edit tools for advanced content, such as templates and add-ons. Basic content edit tools are also displayed.")
							iValueBoolean = cpCore.visitProperty.getBoolean("AllowAdvancedEditor");
							Tag = cpCore.html.html_GetFormInputCheckBox2(AdvancedEditTagID, iValueBoolean, AdvancedEditTagID);
							Tag = genericController.vbReplace(Tag, ">", " onClick=\"document.getElementById('" + QuickEditTagID + "').checked=false;document.getElementById('" + EditTagID + "').checked=false;\">");
							OptionsPanel = OptionsPanel + "\r" + "<div class=\"ccAdminSmall\">"
							+ cr2 + "<LABEL for=\"" + AdvancedEditTagID + "\">" + Tag + "&nbsp;Advanced Edit</LABEL>" + helpLink + "\r" + "</div>";
							//
							// Workflow Authoring Render Mode
							//
							helpLink = "";
							//helpLink = main_GetHelpLink(9, "Enable Workflow Rendering", "Control the display of workflow rendering. With workflow rendering enabled, any changes saved to content records that have not been published will be visible for your review.")
							//If cpCore.siteProperties.allowWorkflowAuthoring Then
							//    iValueBoolean = cpCore.visitProperty.getBoolean("AllowWorkflowRendering")
							//    Tag = cpCore.html.html_GetFormInputCheckBox2(WorkflowTagID, iValueBoolean, WorkflowTagID)
							//    OptionsPanel = OptionsPanel _
							//    & cr & "<div class=""ccAdminSmall"">" _
							//    & cr2 & "<LABEL for=""" & WorkflowTagID & """>" & Tag & "&nbsp;Render Workflow Authoring Changes</LABEL>" & helpLink _
							//    & cr & "</div>"
							//End If
							helpLink = "";
							iValueBoolean = cpCore.visitProperty.getBoolean("AllowDebugging");
							TagID = "AllowDebugging";
							Tag = cpCore.html.html_GetFormInputCheckBox2(TagID, iValueBoolean, TagID);
							OptionsPanel = OptionsPanel + "\r" + "<div class=\"ccAdminSmall\">"
							+ cr2 + "<LABEL for=\"" + TagID + "\">" + Tag + "&nbsp;Debug</LABEL>" + helpLink + "\r" + "</div>";
							//'
							//' Create Path Block Row
							//'
							//If cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore) Then
							//    TagID = "CreatePathBlock"
							//    If cpCore.siteProperties.allowPathBlocking Then
							//        '
							//        ' Path blocking allowed
							//        '
							//        'OptionsPanel = OptionsPanel & SpanClassAdminSmall & "<LABEL for=""" & TagID & """>"
							//        CS = cpCore.db.cs_open("Paths", "name=" & cpCore.db.encodeSQLText(cpCore.webServer.requestPath), , , , , , "ID")
							//        If cpCore.db.cs_ok(CS) Then
							//            PathID = (cpCore.db.cs_getInteger(CS, "ID"))
							//        End If
							//        Call cpCore.db.cs_Close(CS)
							//        If PathID <> 0 Then
							//            '
							//            ' Path is blocked
							//            '
							//            Tag = cpCore.html.html_GetFormInputCheckBox2(TagID, True, TagID) & "&nbsp;Path is blocked [" & cpCore.webServer.requestPath & "] [<a href=""" & genericController.encodeHTML("/" & cpCore.serverconfig.appconfig.adminRoute & "?af=" & AdminFormEdit & "&id=" & PathID & "&cid=" & models.complex.cdefmodel.getcontentid(cpcore,"paths") & "&ad=1") & """ target=""_blank"">edit</a>]</LABEL>"
							//        Else
							//            '
							//            ' Path is not blocked
							//            '
							//            Tag = cpCore.html.html_GetFormInputCheckBox2(TagID, False, TagID) & "&nbsp;Block this path [" & cpCore.webServer.requestPath & "]</LABEL>"
							//        End If
							//        helpLink = ""
							//        'helpLink = main_GetHelpLink(10, "Enable Debugging", "Debugging is a developer only debugging tool. With Debugging enabled, ccLib.TestPoints(...) will print, ErrorTrapping will be displayed, redirections are blocked, and more.")
							//        OptionsPanel = OptionsPanel _
							//        & cr & "<div class=""ccAdminSmall"">" _
							//        & cr2 & "<LABEL for=""" & TagID & """>" & Tag & "</LABEL>" & helpLink _
							//        & cr & "</div>"
							//    End If
							//End If
							//
							// Buttons
							//
							OptionsPanel = OptionsPanel + ""
							+ "\r" + "<div class=\"ccButtonCon\">"
							+ cr2 + "<input type=submit name=" + "mb value=\"" + ButtonApply + "\">"
							+ "\r" + "</div>"
							+ "";
						}
						//
						// ----- Create the Login Panel
						//
						if (string.IsNullOrEmpty(cpCore.doc.authContext.user.name.Trim(' ')))
						{
							Copy = "You are logged in as member #" + cpCore.doc.authContext.user.id + ".";
						}
						else
						{
							Copy = "You are logged in as " + cpCore.doc.authContext.user.name + ".";
						}
						LoginPanel = LoginPanel + ""
						+ "\r" + "<div class=\"ccAdminSmall\">"
						+ cr2 + Copy + ""
						+ "\r" + "</div>";
						//
						// Username
						//
						string Caption = null;
						if (cpCore.siteProperties.getBoolean("allowEmailLogin", false))
						{
							Caption = "Username&nbsp;or&nbsp;Email";
						}
						else
						{
							Caption = "Username";
						}
						TagID = "Username";
						LoginPanel = LoginPanel + ""
						+ "\r" + "<div class=\"ccAdminSmall\">"
						+ cr2 + "<LABEL for=\"" + TagID + "\">" + cpCore.html.html_GetFormInputText2(TagID, "", 1, 30, TagID, false) + "&nbsp;" + Caption + "</LABEL>"
						+ "\r" + "</div>";
						//
						// Username
						//
						if (cpCore.siteProperties.getBoolean("allownopasswordLogin", false))
						{
							Caption = "Password&nbsp;(optional)";
						}
						else
						{
							Caption = "Password";
						}
						TagID = "Password";
						LoginPanel = LoginPanel + ""
						+ "\r" + "<div class=\"ccAdminSmall\">"
						+ cr2 + "<LABEL for=\"" + TagID + "\">" + cpCore.html.html_GetFormInputText2(TagID, "", 1, 30, TagID, true) + "&nbsp;" + Caption + "</LABEL>"
						+ "\r" + "</div>";
						//
						// Autologin checkbox
						//
						if (cpCore.siteProperties.getBoolean("AllowAutoLogin", false))
						{
							if (cpCore.doc.authContext.visit.CookieSupport)
							{
								TagID = "autologin";
								LoginPanel = LoginPanel + ""
								+ "\r" + "<div class=\"ccAdminSmall\">"
								+ cr2 + "<LABEL for=\"" + TagID + "\">" + cpCore.html.html_GetFormInputCheckBox2(TagID, true, TagID) + "&nbsp;Login automatically from this computer</LABEL>"
								+ "\r" + "</div>";
							}
						}
						//
						// Buttons
						//
						LoginPanel = LoginPanel + Adminui.GetButtonBar(Adminui.GetButtonsFromList(ButtonLogin + "," + ButtonLogout, true, true, "mb"), "");
						//
						// ----- assemble tools panel
						//
						Copy = ""
						+ "\r" + "<td width=\"50%\" class=\"ccPanelInput\" style=\"vertical-align:bottom;\">"
						+ genericController.htmlIndent(LoginPanel) + "\r" + "</td>"
						+ "\r" + "<td width=\"50%\" class=\"ccPanelInput\" style=\"vertical-align:bottom;\">"
						+ genericController.htmlIndent(OptionsPanel) + "\r" + "</td>";
						Copy = ""
						+ "\r" + "<tr>"
						+ genericController.htmlIndent(Copy) + "\r" + "</tr>"
						+ "";
						Copy = ""
						+ "\r" + "<table border=\"0\" cellpadding=\"3\" cellspacing=\"0\" width=\"100%\">"
						+ genericController.htmlIndent(Copy) + "\r" + "</table>";
						ToolsPanel.Add(main_GetPanelInput(Copy));
						ToolsPanel.Add(cpCore.html.html_GetFormEnd);
						result = result + main_GetPanel(ToolsPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5);
						//
						result = result + main_GetPanel(LinkPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5);
						//
						LinkPanel = null;
						ToolsPanel = null;
						AnotherPanel = null;
					}
					//
					// --- Developer Debug Panel
					//
					if (cpCore.visitProperty.getBoolean("AllowDebugging"))
					{
						//
						// --- Debug Panel Header
						//
						LinkPanel = new stringBuilderLegacyController();
						LinkPanel.Add(SpanClassAdminSmall);
						//LinkPanel.Add( "WebClient " & main_WebClientVersion & " | "
						LinkPanel.Add("Contensive " + cpCore.codeVersion() + " | ");
						LinkPanel.Add(cpCore.doc.profileStartTime.ToString("G") + " | ");
						LinkPanel.Add("<a class=\"ccAdminLink\" target=\"_blank\" href=\"http: //support.Contensive.com/\">Support</A> | ");
						LinkPanel.Add("<a class=\"ccAdminLink\" href=\"" + genericController.encodeHTML("/" + cpCore.serverConfig.appConfig.adminRoute) + "\">Admin Home</A> | ");
						LinkPanel.Add("<a class=\"ccAdminLink\" href=\"" + genericController.encodeHTML("http://" + cpCore.webServer.requestDomain) + "\">Public Home</A> | ");
						LinkPanel.Add("<a class=\"ccAdminLink\" target=\"_blank\" href=\"" + genericController.encodeHTML("/" + cpCore.serverConfig.appConfig.adminRoute + "?" + RequestNameHardCodedPage + "=" + HardCodedPageMyProfile) + "\">My Profile</A> | ");
						LinkPanel.Add("</span>");
						//
						//
						//
						//DebugPanel = DebugPanel & main_GetPanel(LinkPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", "5")
						//
						DebugPanel = DebugPanel + "\r" + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">"
						+ cr2 + "<tr>"
						+ cr3 + "<td width=\"100\" class=\"ccPanel\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100\" height=\"1\" ></td>"
						+ cr3 + "<td width=\"100%\" class=\"ccPanel\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"1\" height=\"1\" ></td>"
						+ cr2 + "</tr>";
						//
						DebugPanel = DebugPanel + getDebugPanelRow("DOM", "<a class=\"ccAdminLink\" href=\"/ccLib/clientside/DOMViewer.htm\" target=\"_blank\">Click</A>");
						DebugPanel = DebugPanel + getDebugPanelRow("Trap Errors", genericController.encodeHTML(cpCore.siteProperties.trapErrors.ToString()));
						DebugPanel = DebugPanel + getDebugPanelRow("Trap Email", genericController.encodeHTML(cpCore.siteProperties.getText("TrapEmail")));
						DebugPanel = DebugPanel + getDebugPanelRow("main_ServerLink", genericController.encodeHTML(cpCore.webServer.requestUrl));
						DebugPanel = DebugPanel + getDebugPanelRow("main_ServerDomain", genericController.encodeHTML(cpCore.webServer.requestDomain));
						DebugPanel = DebugPanel + getDebugPanelRow("main_ServerProtocol", genericController.encodeHTML(cpCore.webServer.requestProtocol));
						DebugPanel = DebugPanel + getDebugPanelRow("main_ServerHost", genericController.encodeHTML(cpCore.webServer.requestDomain));
						DebugPanel = DebugPanel + getDebugPanelRow("main_ServerPath", genericController.encodeHTML(cpCore.webServer.requestPath));
						DebugPanel = DebugPanel + getDebugPanelRow("main_ServerPage", genericController.encodeHTML(cpCore.webServer.requestPage));
						Copy = "";
						if (cpCore.webServer.requestQueryString != "")
						{
							CopySplit = cpCore.webServer.requestQueryString.Split('&');
							for (Ptr = 0; Ptr <= CopySplit.GetUpperBound(0); Ptr++)
							{
								copyNameValue = CopySplit[Ptr];
								if (!string.IsNullOrEmpty(copyNameValue))
								{
									copyNameValueSplit = copyNameValue.Split('=');
									CopyName = genericController.DecodeResponseVariable(copyNameValueSplit[0]);
									copyValue = "";
									if (copyNameValueSplit.GetUpperBound(0) > 0)
									{
										copyValue = genericController.DecodeResponseVariable(copyNameValueSplit[1]);
									}
									Copy = Copy + "\r" + "<br>" + genericController.encodeHTML(CopyName + "=" + copyValue);
								}
							}
							Copy = Copy.Substring(7);
						}
						DebugPanel = DebugPanel + getDebugPanelRow("main_ServerQueryString", Copy);
						Copy = "";
						foreach (string key in cpCore.docProperties.getKeyList())
						{
							docPropertiesClass docProperty = cpCore.docProperties.getProperty(key);
							if (docProperty.IsForm)
							{
								Copy = Copy + "\r" + "<br>" + genericController.encodeHTML(docProperty.NameValue);
							}
						}
						DebugPanel = DebugPanel + getDebugPanelRow("Render Time &gt;= ", ((cpCore.doc.appStopWatch.ElapsedMilliseconds) / 1000).ToString("0.000") + " sec");
						if (true)
						{
							VisitHrs = Convert.ToInt32(cpCore.doc.authContext.visit.TimeToLastHit / 3600);
							VisitMin = Convert.ToInt32(cpCore.doc.authContext.visit.TimeToLastHit / 60) - (60 * VisitHrs);
							VisitSec = cpCore.doc.authContext.visit.TimeToLastHit % 60;
							DebugPanel = DebugPanel + getDebugPanelRow("Visit Length", Convert.ToString(cpCore.doc.authContext.visit.TimeToLastHit) + " sec, (" + VisitHrs + " hrs " + VisitMin + " mins " + VisitSec + " secs)");
							//DebugPanel = DebugPanel & main_DebugPanelRow("Visit Length", CStr(main_VisitTimeToLastHit) & " sec, (" & Int(main_VisitTimeToLastHit / 60) & " min " & (main_VisitTimeToLastHit Mod 60) & " sec)")
						}
						DebugPanel = DebugPanel + getDebugPanelRow("Addon Profile", "<hr><ul class=\"ccPanel\">" + "<li>tbd</li>" + "\r" + "</ul>");
						//
						DebugPanel = DebugPanel + "</table>";
						//
						if (ShowLegacyToolsPanel)
						{
							//
							// Debug Panel as part of legacy tools panel
							//
							result = result + main_GetPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5);
						}
						else
						{
							//
							// Debug Panel without Legacy Tools panel
							//
							result = result + main_GetPanelHeader("Debug Panel") + main_GetPanel(LinkPanel.Text) + main_GetPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5);
						}
					}
					result = "\r" + "<div class=\"ccCon\">" + genericController.htmlIndent(result) + "\r" + "</div>";
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//
		//
		//
		private string getDebugPanelRow(string Label, string Value)
		{
			return cr2 + "<tr><td valign=\"top\" class=\"ccPanel ccAdminSmall\">" + Label + "</td><td valign=\"top\" class=\"ccPanel ccAdminSmall\">" + Value + "</td></tr>";
		}

		//
		//=================================================================================================================
		//   csv_GetAddonOptionStringValue
		//
		//   gets the value from a list matching the name
		//
		//   InstanceOptionstring is an "AddonEncoded" name=AddonEncodedValue[selector]descriptor&name=value string
		//=================================================================================================================
		//
		public static string getAddonOptionStringValue(string OptionName, string addonOptionString)
		{
			string result = genericController.getSimpleNameValue(OptionName, addonOptionString, "", "&");
			int Pos = genericController.vbInstr(1, result, "[");
			if (Pos > 0)
			{
				result = result.Substring(0, Pos - 1);
			}
			return Convert.ToString(genericController.decodeNvaArgument(result)).Trim(' ');
		}
		//
		//====================================================================================================
		/// <summary>
		/// Create the full html doc from the accumulated elements
		/// </summary>
		/// <param name="htmlBody"></param>
		/// <param name="htmlBodyTag"></param>
		/// <param name="allowLogin"></param>
		/// <param name="allowTools"></param>
		/// <param name="blockNonContentExtras"></param>
		/// <param name="isAdminSite"></param>
		/// <returns></returns>
		public string getHtmlDoc(string htmlBody, string htmlBodyTag, bool allowLogin = true, bool allowTools = true)
		{
			string result = "";
			try
			{
				string htmlHead = getHtmlHead();
				string htmlBeforeEndOfBody = getHtmlDoc_beforeEndOfBodyHtml(allowLogin, allowTools);

				result = ""
					+ cpCore.siteProperties.docTypeDeclaration + Environment.NewLine + "<html>"
					+ Environment.NewLine + "<head>"
					+ htmlHead + Environment.NewLine + "</head>"
					+ Environment.NewLine + htmlBodyTag + htmlBody + htmlBeforeEndOfBody + Environment.NewLine + "</body>"
					+ Environment.NewLine + "</html>"
					+ "";
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//'
		//' assemble all the html parts
		//'
		//Public Function assembleHtmlDoc(ByVal head As String, ByVal bodyTag As String, ByVal Body As String) As String
		//    Return "" _
		//        & cpCore.siteProperties.docTypeDeclarationAdmin _
		//        & cr & "<html>" _
		//        & cr2 & "<head>" _
		//        & genericController.htmlIndent(head) _
		//        & cr2 & "</head>" _
		//        & cr2 & bodyTag _
		//        & genericController.htmlIndent(Body) _
		//        & cr2 & "</body>" _
		//        & cr & "</html>"
		//End Function
		//'
		//'========================================================================
		//' ----- Starts an HTML page (for an admin page -- not a public page)
		//'========================================================================
		//'
		//Public Function getHtmlDoc_beforeBodyHtml(Optional ByVal Title As String = "", Optional ByVal PageMargin As Integer = 0) As String
		//    If Title <> "" Then
		//        Call main_AddPagetitle(Title)
		//    End If
		//    If main_MetaContent_Title = "" Then
		//        Call main_AddPagetitle("Admin-" & cpCore.webServer.webServerIO_requestDomain)
		//    End If
		//    cpCore.webServer.webServerIO_response_NoFollow = True
		//    Call main_SetMetaContent(0, 0)
		//    '
		//    Return "" _
		//        & cpCore.siteProperties.docTypeDeclarationAdmin _
		//        & vbCrLf & "<html>" _
		//        & vbCrLf & "<head>" _
		//        & getHTMLInternalHead(True) _
		//        & vbCrLf & "</head>" _
		//        & vbCrLf & "<body class=""ccBodyAdmin ccCon"">"
		//End Function

		//
		//====================================================================================================
		/// <summary>
		/// legacy compatibility
		/// </summary>
		/// <param name="cpCore"></param>
		/// <param name="ButtonList"></param>
		/// <returns></returns>
		public static string legacy_closeFormTable(coreClass cpCore, string ButtonList)
		{
			string templegacy_closeFormTable = null;
			if (!string.IsNullOrEmpty(ButtonList))
			{
				templegacy_closeFormTable = "</td></tr></TABLE>" + cpCore.html.main_GetPanelButtons(ButtonList, "Button") + "</form>";
			}
			else
			{
				templegacy_closeFormTable = "</td></tr></TABLE></form>";
			}
			return templegacy_closeFormTable;
		}
		//
		//====================================================================================================
		/// <summary>
		/// legacy compatibility
		/// </summary>
		/// <param name="cpCore"></param>
		/// <param name="ButtonList"></param>
		/// <returns></returns>
		public static string legacy_openFormTable(coreClass cpCore, string ButtonList)
		{
			string result = "";
			try
			{
				result = cpCore.html.html_GetFormStart();
				if (!string.IsNullOrEmpty(ButtonList))
				{
					result = result + cpCore.html.main_GetPanelButtons(ButtonList, "Button");
				}
				result = result + "<table border=\"0\" cellpadding=\"10\" cellspacing=\"0\" width=\"100%\"><tr><TD>";
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//
		//====================================================================================================
		//
		public string getHtmlHead()
		{
			List<string> headList = new List<string>();
			try
			{
				//
				// -- meta content
				if (cpCore.doc.htmlMetaContent_TitleList.Count > 0)
				{
					string content = "";
					foreach (var asset in cpCore.doc.htmlMetaContent_TitleList)
					{
						if ((cpCore.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage)))
						{
							headList.Add("<!-- added by " + asset.addedByMessage + " -->");
						}
						content += " | " + asset.content;
					}
					headList.Add("<title>" + encodeHTML(content.Substring(3)) + "</title>");
				}
				if (cpCore.doc.htmlMetaContent_KeyWordList.Count > 0)
				{
					string content = "";
					foreach (var asset in cpCore.doc.htmlMetaContent_KeyWordList.FindAll((a) => (!string.IsNullOrEmpty(a.content))))
					{
						if ((cpCore.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage)))
						{
							headList.Add("<!-- '" + encodeHTML(asset.content + "' added by " + asset.addedByMessage) + " -->");
						}
						content += "," + asset.content;
					}
					if (!string.IsNullOrEmpty(content))
					{
						headList.Add("<meta name=\"keywords\" content=\"" + encodeHTML(content.Substring(1)) + "\" >");
					}
				}
				if (cpCore.doc.htmlMetaContent_Description.Count > 0)
				{
					string content = "";
					foreach (var asset in cpCore.doc.htmlMetaContent_Description)
					{
						if ((cpCore.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage)))
						{
							headList.Add("<!-- '" + encodeHTML(asset.content + "' added by " + asset.addedByMessage) + " -->");
						}
						content += "," + asset.content;
					}
					headList.Add("<meta name=\"description\" content=\"" + encodeHTML(content.Substring(1)) + "\" >");
				}
				//
				// -- favicon
				string VirtualFilename = cpCore.siteProperties.getText("faviconfilename");
				switch (IO.Path.GetExtension(VirtualFilename).ToLower)
				{
					case ".ico":
						headList.Add("<link rel=\"icon\" type=\"image/vnd.microsoft.icon\" href=\"" + genericController.getCdnFileLink(cpCore, VirtualFilename) + "\" >");
						break;
					case ".png":
						headList.Add("<link rel=\"icon\" type=\"image/png\" href=\"" + genericController.getCdnFileLink(cpCore, VirtualFilename) + "\" >");
						break;
					case ".gif":
						headList.Add("<link rel=\"icon\" type=\"image/gif\" href=\"" + genericController.getCdnFileLink(cpCore, VirtualFilename) + "\" >");
						break;
					case ".jpg":
						headList.Add("<link rel=\"icon\" type=\"image/jpg\" href=\"" + genericController.getCdnFileLink(cpCore, VirtualFilename) + "\" >");
						break;
				}
				//
				// -- misc caching, etc
				string encoding = genericController.encodeHTML(cpCore.siteProperties.getText("Site Character Encoding", "utf-8"));
				headList.Add("<meta http-equiv=\"content-type\" content=\"text/html; charset=" + encoding + "\">");
				headList.Add("<meta http-equiv=\"content-language\" content=\"en-us\">");
				headList.Add("<meta http-equiv=\"cache-control\" content=\"no-cache\">");
				headList.Add("<meta http-equiv=\"expires\" content=\"-1\">");
				headList.Add("<meta http-equiv=\"pragma\" content=\"no-cache\">");
				headList.Add("<meta name=\"generator\" content=\"Contensive\">");
				//
				// -- no-follow
				if (cpCore.webServer.response_NoFollow)
				{
					headList.Add("<meta name=\"robots\" content=\"nofollow\" >");
					headList.Add("<meta name=\"mssmarttagspreventparsing\" content=\"true\" >");
				}
				//
				// -- base is needed for Link Alias case where a slash is in the URL (page named 1/2/3/4/5)
				if (!string.IsNullOrEmpty(cpCore.webServer.serverFormActionURL))
				{
					string BaseHref = cpCore.webServer.serverFormActionURL;
					if (!string.IsNullOrEmpty(cpCore.doc.refreshQueryString))
					{
						BaseHref += "?" + cpCore.doc.refreshQueryString;
					}
					headList.Add("<base href=\"" + BaseHref + "\" >");
				}
				//
				if (cpCore.doc.htmlAssetList.Count > 0)
				{
					List<string> scriptList = new List<string>();
					List<string> styleList = new List<string>();
					foreach (var asset in cpCore.doc.htmlAssetList.FindAll((htmlAssetClass item) => (item.inHead)))
					{
						if (cpCore.doc.allowDebugLog)
						{
							if ((cpCore.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage)))
							{
								headList.Add("<!-- '" + encodeHTML(asset.content + "' added by " + asset.addedByMessage) + " -->");
							}
						}
						if (asset.assetType.Equals(htmlAssetTypeEnum.style))
						{
							if (asset.isLink)
							{
								styleList.Add("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + asset.content + "\" >");
							}
							else
							{
								styleList.Add("<style>" + asset.content + "</style>");
							}
						}
						else if (asset.assetType.Equals(htmlAssetTypeEnum.script))
						{

							if (asset.isLink)
							{
								scriptList.Add("<script type=\"text/javascript\" src=\"" + asset.content + "\"></script>");
							}
							else
							{
								scriptList.Add("<script type=\"text/javascript\">" + asset.content + "</script>");
							}
						}
					}
					headList.AddRange(styleList);
					headList.AddRange(scriptList);
				}
				//
				// -- other head tags - always last
				foreach (var asset in cpCore.doc.htmlMetaContent_OtherTags.FindAll((a) => (!string.IsNullOrEmpty(a.content))))
				{
					if (cpCore.doc.allowDebugLog)
					{
						if ((cpCore.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage)))
						{
							headList.Add("<!-- '" + encodeHTML(asset.content + "' added by " + asset.addedByMessage) + " -->");
						}
					}
					headList.Add(asset.content);
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return string.Join("\r", headList);
		}

	}
}
