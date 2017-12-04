using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

//
using Contensive.BaseClasses;
using Contensive.Core.Models.Entity;
//
namespace Contensive.Core.Controllers
{
	//
	//====================================================================================================
	/// <summary>
	/// build page content system. Persistence is the docController.
	/// </summary>
	public class pageContentController : IDisposable
	{
		//
		//
		//
		//
		internal static string getFormPage(coreClass cpcore, string FormPageName, int GroupIDToJoinOnSuccess)
		{
				string tempgetFormPage = null;
			try
			{
				//
				string RepeatBody = null;
				int PtrFront = 0;
				int PtrBack = 0;
				string[] i = null;
				int IPtr = 0;
				int IStart = 0;
				string[] IArgs = null;
				int IArgPtr = 0;
				int CSPeople = 0;
				string Body = null;
				string Instruction = null;
				string Formhtml = string.Empty;
				string FormInstructions = string.Empty;
				int CS = 0;
				bool HasRequiredFields = false;
				string ArgCaption = null;
				int ArgType = 0;
				bool ArgRequired = false;
				string GroupName = null;
				bool GroupValue = false;
				int GroupRowPtr = 0;
				int FormPageID = 0;
				main_FormPagetype f = null;
				bool IsRetry = false;
				string CaptionSpan = null;
				string Caption = null;
				bool IsRequiredByCDef = false;
				Models.Complex.cdefModel PeopleCDef = null;
				//
				IsRetry = (cpcore.docProperties.getInteger("ContensiveFormPageID") != 0);
				//
				CS = cpcore.db.csOpen("Form Pages", "name=" + cpcore.db.encodeSQLText(FormPageName));
				if (cpcore.db.csOk(CS))
				{
					FormPageID = cpcore.db.csGetInteger(CS, "ID");
					Formhtml = cpcore.db.csGetText(CS, "Body");
					FormInstructions = cpcore.db.csGetText(CS, "Instructions");
				}
				cpcore.db.csClose(CS);
				f = loadFormPageInstructions(cpcore, FormInstructions, Formhtml);
				//
				//
				//
				RepeatBody = "";
				CSPeople = -1;
				for (IPtr = 0; IPtr <= f.Inst.GetUpperBound(0); IPtr++)
				{
					var tempVar = f.Inst(IPtr);
					switch (tempVar.Type)
					{
						case 1:
							//
							// People Record
							//
							if (IsRetry && cpcore.docProperties.getText(tempVar.PeopleField) == "")
							{
								CaptionSpan = "<span class=\"ccError\">";
							}
							else
							{
								CaptionSpan = "<span>";
							}
							if (!cpcore.db.csOk(CSPeople))
							{
								CSPeople = cpcore.db.csOpenRecord("people", cpcore.doc.authContext.user.id);
							}
							Caption = tempVar.Caption;
							if (tempVar.REquired | genericController.EncodeBoolean(Models.Complex.cdefModel.GetContentFieldProperty(cpcore, "People", tempVar.PeopleField, "Required")))
							{
								Caption = "*" + Caption;
							}
							if (cpcore.db.csOk(CSPeople))
							{
								Body = f.RepeatCell;
								Body = genericController.vbReplace(Body, "{{CAPTION}}", CaptionSpan + Caption + "</span>", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
								Body = genericController.vbReplace(Body, "{{FIELD}}", cpcore.html.html_GetFormInputCS(CSPeople, "People", tempVar.PeopleField), 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
								RepeatBody = RepeatBody + Body;
								HasRequiredFields = HasRequiredFields || tempVar.REquired;
							}
							break;
						case 2:
							//
							// Group main_MemberShip
							//
							GroupValue = cpcore.doc.authContext.IsMemberOfGroup2(cpcore, tempVar.GroupName);
							Body = f.RepeatCell;
							Body = genericController.vbReplace(Body, "{{CAPTION}}", cpcore.html.html_GetFormInputCheckBox2("Group" + tempVar.GroupName, GroupValue), 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
							Body = genericController.vbReplace(Body, "{{FIELD}}", tempVar.Caption);
							RepeatBody = RepeatBody + Body;
							GroupRowPtr = GroupRowPtr + 1;
							HasRequiredFields = HasRequiredFields || tempVar.REquired;
							break;
					}
				}
				cpcore.db.csClose(CSPeople);
				if (HasRequiredFields)
				{
					Body = f.RepeatCell;
					Body = genericController.vbReplace(Body, "{{CAPTION}}", "&nbsp;", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
					Body = genericController.vbReplace(Body, "{{FIELD}}", "*&nbsp;Required Fields");
					RepeatBody = RepeatBody + Body;
				}
				//
				tempgetFormPage = ""
				+ errorController.error_GetUserError(cpcore) + cpcore.html.html_GetUploadFormStart() + cpcore.html.html_GetFormInputHidden("ContensiveFormPageID", FormPageID) + cpcore.html.html_GetFormInputHidden("SuccessID", cpcore.security.encodeToken(GroupIDToJoinOnSuccess, cpcore.doc.profileStartTime)) + f.PreRepeat + RepeatBody + f.PostRepeat + cpcore.html.html_GetUploadFormEnd();
				//
				return tempgetFormPage;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError13("main_GetFormPage")
			return tempgetFormPage;
		}
		//
		//=============================================================================
		//   getContentBox
		//
		//   PageID is the page to display. If it is 0, the root page is displayed
		//   RootPageID has to be the ID of the root page for PageID
		//=============================================================================
		//
		public static string getContentBox(coreClass cpCore, string OrderByClause, bool AllowChildPageList, bool AllowReturnLink, bool ArchivePages, int ignoreme, bool UseContentWatchLink, bool allowPageWithoutSectionDisplay)
		{
			string returnHtml = "";
			try
			{
				DateTime DateModified = default(DateTime);
				int PageRecordID = 0;
				string PageName = null;
				int CS = 0;
				string SQL = null;
				bool ContentBlocked = false;
				bool NewPageCreated = false;
				int SystemEMailID = 0;
				int ConditionID = 0;
				int ConditionGroupID = 0;
				int main_AddGroupID = 0;
				int RemoveGroupID = 0;
				int RegistrationGroupID = 0;
				string[] BlockedPages = null;
				int BlockedPageRecordID = 0;
				string BlockCopy = null;
				int pageViewings = 0;
				string layoutError = "";
				//
				cpCore.html.addHeadTag("<meta name=\"contentId\" content=\"" + cpCore.doc.page.id + "\" >", "page content");
				//
				returnHtml = getContentBox_content(cpCore, OrderByClause, AllowChildPageList, AllowReturnLink, ArchivePages, ignoreme, UseContentWatchLink, allowPageWithoutSectionDisplay);
				//
				// ----- If Link field populated, do redirect
				if (cpCore.doc.page.PageLink != "")
				{
					cpCore.doc.page.Clicks += 1;
					cpCore.doc.page.save(cpCore);
					cpCore.doc.redirectLink = cpCore.doc.page.PageLink;
					cpCore.doc.redirectReason = "Redirect required because this page (PageRecordID=" + cpCore.doc.page.id + ") has a Link Override [" + cpCore.doc.page.PageLink + "].";
					return "";
				}
				//
				// -- build list of blocked pages
				string BlockedRecordIDList = "";
				if ((!string.IsNullOrEmpty(returnHtml)) && (cpCore.doc.redirectLink == ""))
				{
					NewPageCreated = true;
					foreach (pageContentModel testPage in cpCore.doc.pageToRootList)
					{
						if (testPage.BlockContent | testPage.BlockPage)
						{
							BlockedRecordIDList = BlockedRecordIDList + "," + testPage.id;
						}
					}
					if (!string.IsNullOrEmpty(BlockedRecordIDList))
					{
						BlockedRecordIDList = BlockedRecordIDList.Substring(1);
					}
				}
				//
				// ----- Content Blocking
				if (!string.IsNullOrEmpty(BlockedRecordIDList))
				{
					if (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore))
					{
						//
						// Administrators are never blocked
						//
					}
					else if (!cpCore.doc.authContext.isAuthenticated())
					{
						//
						// non-authenticated are always blocked
						//
						ContentBlocked = true;
					}
					else
					{
						//
						// Check Access Groups, if in access groups, remove group from BlockedRecordIDList
						//
						SQL = "SELECT DISTINCT ccPageContentBlockRules.RecordID"
							+ " FROM (ccPageContentBlockRules"
							+ " LEFT JOIN ccgroups ON ccPageContentBlockRules.GroupID = ccgroups.ID)"
							+ " LEFT JOIN ccMemberRules ON ccgroups.ID = ccMemberRules.GroupID"
							+ " WHERE (((ccMemberRules.MemberID)=" + cpCore.db.encodeSQLNumber(cpCore.doc.authContext.user.id) + ")"
							+ " AND ((ccPageContentBlockRules.RecordID) In (" + BlockedRecordIDList + "))"
							+ " AND ((ccPageContentBlockRules.Active)<>0)"
							+ " AND ((ccgroups.Active)<>0)"
							+ " AND ((ccMemberRules.Active)<>0)"
							+ " AND ((ccMemberRules.DateExpires) Is Null Or (ccMemberRules.DateExpires)>" + cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime) + "));";
						CS = cpCore.db.csOpenSql(SQL);
						BlockedRecordIDList = "," + BlockedRecordIDList;
						while (cpCore.db.csOk(CS))
						{
							BlockedRecordIDList = genericController.vbReplace(BlockedRecordIDList, "," + cpCore.db.csGetText(CS, "RecordID"), "");
							cpCore.db.csGoNext(CS);
						}
						cpCore.db.csClose(CS);
						if (!string.IsNullOrEmpty(BlockedRecordIDList))
						{
							//
							// ##### remove the leading comma
							BlockedRecordIDList = BlockedRecordIDList.Substring(1);
							// Check the remaining blocked records against the members Content Management
							// ##### removed hardcoded mistakes from the sql
							SQL = "SELECT DISTINCT ccPageContent.ID as RecordID"
								+ " FROM ((ccPageContent"
								+ " LEFT JOIN ccGroupRules ON ccPageContent.ContentControlID = ccGroupRules.ContentID)"
								+ " LEFT JOIN ccgroups AS ManagementGroups ON ccGroupRules.GroupID = ManagementGroups.ID)"
								+ " LEFT JOIN ccMemberRules AS ManagementMemberRules ON ManagementGroups.ID = ManagementMemberRules.GroupID"
								+ " WHERE (((ccPageContent.ID) In (" + BlockedRecordIDList + "))"
								+ " AND ((ccGroupRules.Active)<>0)"
								+ " AND ((ManagementGroups.Active)<>0)"
								+ " AND ((ManagementMemberRules.Active)<>0)"
								+ " AND ((ManagementMemberRules.DateExpires) Is Null Or (ManagementMemberRules.DateExpires)>" + cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime) + ")"
								+ " AND ((ManagementMemberRules.MemberID)=" + cpCore.doc.authContext.user.id + " ));";
							CS = cpCore.db.csOpenSql(SQL);
							while (cpCore.db.csOk(CS))
							{
								BlockedRecordIDList = genericController.vbReplace(BlockedRecordIDList, "," + cpCore.db.csGetText(CS, "RecordID"), "");
								cpCore.db.csGoNext(CS);
							}
							cpCore.db.csClose(CS);
						}
						if (!string.IsNullOrEmpty(BlockedRecordIDList))
						{
							ContentBlocked = true;
						}
						cpCore.db.csClose(CS);
					}
				}
				//
				//
				//
				if (ContentBlocked)
				{
					string CustomBlockMessageFilename = "";
					int BlockSourceID = main_BlockSourceDefaultMessage;
					int ContentPadding = 20;
					BlockedPages = BlockedRecordIDList.Split(',');
					BlockedPageRecordID = genericController.EncodeInteger(BlockedPages[BlockedPages.GetUpperBound(0)]);
					if (BlockedPageRecordID != 0)
					{
						CS = cpCore.db.csOpenRecord("Page Content", BlockedPageRecordID,,, "CustomBlockMessage,BlockSourceID,RegistrationGroupID,ContentPadding");
						if (cpCore.db.csOk(CS))
						{
							BlockSourceID = cpCore.db.csGetInteger(CS, "BlockSourceID");
							ContentPadding = cpCore.db.csGetInteger(CS, "ContentPadding");
							CustomBlockMessageFilename = cpCore.db.csGetText(CS, "CustomBlockMessage");
							RegistrationGroupID = cpCore.db.csGetInteger(CS, "RegistrationGroupID");
						}
						cpCore.db.csClose(CS);
					}
					//
					// Block Appropriately
					//
					switch (BlockSourceID)
					{
						case main_BlockSourceCustomMessage:
						{
							//
							// ----- Custom Message
							//
							returnHtml = cpCore.cdnFiles.readFile(CustomBlockMessageFilename);
							break;
						}
						case main_BlockSourceLogin:
						{
							//
							// ----- Login page
							//
							string BlockForm = "";
							if (!cpCore.doc.authContext.isAuthenticated())
							{
								if (!cpCore.doc.authContext.isRecognized(cpCore))
								{
									//
									// -- not recognized
									BlockForm = ""
										+ "<p>This content has limited access. If you have an account, please login using this form.</p>"
										+ cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginForm), new CPUtilsBaseClass.addonExecuteContext {addonType = CPUtilsBaseClass.addonContext.ContextPage}) + "";
								}
								else
								{
									//
									// -- recognized, not authenticated
									BlockForm = ""
										+ "<p>This content has limited access. You were recognized as \"<b>" + cpCore.doc.authContext.user.name + "</b>\", but you need to login to continue. To login to this account or another, please use this form.</p>"
										+ cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginForm), new CPUtilsBaseClass.addonExecuteContext {addonType = CPUtilsBaseClass.addonContext.ContextPage}) + "";
								}
							}
							else
							{
								//
								// -- authenticated
								BlockForm = ""
									+ "<p>You are currently logged in as \"<b>" + cpCore.doc.authContext.user.name + "</b>\". If this is not you, please <a href=\"?" + cpCore.doc.refreshQueryString + "&method=logout\" rel=\"nofollow\">Click Here</a>.</p>"
									+ "<p>This account does not have access to this content. If you want to login with a different account, please use this form.</p>"
									+ cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginForm), new CPUtilsBaseClass.addonExecuteContext {addonType = CPUtilsBaseClass.addonContext.ContextPage}) + "";
							}
							returnHtml = ""
								+ "<div style=\"margin: 100px, auto, auto, auto;text-align:left;\">"
								+ errorController.error_GetUserError(cpCore) + BlockForm + "</div>";
								break;
						}
						case main_BlockSourceRegistration:
						{
							//
							// ----- Registration
							//
							string BlockForm = "";
							if (cpCore.docProperties.getInteger("subform") == main_BlockSourceLogin)
							{
								//
								// login subform form
								BlockForm = ""
									+ "<p>This content has limited access. If you have an account, please login using this form.</p>"
									+ "<p>If you do not have an account, <a href=?" + cpCore.doc.refreshQueryString + "&subform=0>click here to register</a>.</p>"
									+ cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginForm), new CPUtilsBaseClass.addonExecuteContext {addonType = CPUtilsBaseClass.addonContext.ContextPage}) + "";
							}
							else
							{
								//
								// Register Form
								//
								if (!cpCore.doc.authContext.isAuthenticated() & cpCore.doc.authContext.isRecognized(cpCore))
								{
									//
									// -- Can not take the chance, if you go to a registration page, and you are recognized but not auth -- logout first
									cpCore.doc.authContext.logout(cpCore);
								}
								if (!cpCore.doc.authContext.isAuthenticated())
								{
									//
									// -- Not Authenticated
									cpCore.doc.verifyRegistrationFormPage(cpCore);
									BlockForm = ""
										+ "<p>This content has limited access. If you have an account, <a href=?" + cpCore.doc.refreshQueryString + "&subform=" + main_BlockSourceLogin + ">Click Here to login</a>.</p>"
										+ "<p>To view this content, please complete this form.</p>"
										+ getFormPage(cpCore, "Registration Form", RegistrationGroupID) + "";
								}
								else
								{
									//
									// -- Authenticated
									cpCore.doc.verifyRegistrationFormPage(cpCore);
									BlockCopy = ""
										+ "<p>You are currently logged in as \"<b>" + cpCore.doc.authContext.user.name + "</b>\". If this is not you, please <a href=\"?" + cpCore.doc.refreshQueryString + "&method=logout\" rel=\"nofollow\">Click Here</a>.</p>"
										+ "<p>This account does not have access to this content. To view this content, please complete this form.</p>"
										+ getFormPage(cpCore, "Registration Form", RegistrationGroupID) + "";
								}
							}
							returnHtml = ""
								+ "<div style=\"margin: 100px, auto, auto, auto;text-align:left;\">"
								+ errorController.error_GetUserError(cpCore) + BlockForm + "</div>";
								break;
						}
						default:
						{
							//
							// ----- Content as blocked - convert from site property to content page
							//
							returnHtml = getDefaultBlockMessage(cpCore, UseContentWatchLink);
							break;
						}
					}
					//
					// If the output is blank, put default message in
					//
					if (string.IsNullOrEmpty(returnHtml))
					{
						returnHtml = getDefaultBlockMessage(cpCore, UseContentWatchLink);
					}
					//
					// Encode the copy
					//
					returnHtml = cpCore.html.executeContentCommands(null, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpCore.doc.authContext.user.id, cpCore.doc.authContext.isAuthenticated, layoutError);
					returnHtml = cpCore.html.convertActiveContentToHtmlForWebRender(returnHtml, pageContentModel.contentName, PageRecordID, cpCore.doc.page.ContactMemberID, "http://" + cpCore.webServer.requestDomain, cpCore.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage);
					if (cpCore.doc.refreshQueryString != "")
					{
						returnHtml = genericController.vbReplace(returnHtml, "?method=login", "?method=Login&" + cpCore.doc.refreshQueryString, 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
					}
					//
					// Add in content padding required for integration with the template
					returnHtml = getContentBoxWrapper(cpCore, returnHtml, ContentPadding);
				}
				//
				// ----- Encoding, Tracking and Triggers
				if (!ContentBlocked)
				{
					if (cpCore.visitProperty.getBoolean("AllowQuickEditor"))
					{
						//
						// Quick Editor, no encoding or tracking
						//
					}
					else
					{
						pageViewings = cpCore.doc.page.Viewings;
						if (cpCore.doc.authContext.isEditing(pageContentModel.contentName) | cpCore.visitProperty.getBoolean("AllowWorkflowRendering"))
						{
							//
							// Link authoring, workflow rendering -> do encoding, but no tracking
							//
							returnHtml = cpCore.html.executeContentCommands(null, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpCore.doc.authContext.user.id, cpCore.doc.authContext.isAuthenticated, layoutError);
							returnHtml = cpCore.html.convertActiveContentToHtmlForWebRender(returnHtml, pageContentModel.contentName, PageRecordID, cpCore.doc.page.ContactMemberID, "http://" + cpCore.webServer.requestDomain, cpCore.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage);
						}
						else
						{
							//
							// Live content
							returnHtml = cpCore.html.executeContentCommands(null, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpCore.doc.authContext.user.id, cpCore.doc.authContext.isAuthenticated, layoutError);
							returnHtml = cpCore.html.convertActiveContentToHtmlForWebRender(returnHtml, pageContentModel.contentName, PageRecordID, cpCore.doc.page.ContactMemberID, "http://" + cpCore.webServer.requestDomain, cpCore.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage);
							cpCore.db.executeQuery("update ccpagecontent set viewings=" + (pageViewings + 1) + " where id=" + cpCore.doc.page.id);
						}
						//
						// Page Hit Notification
						//
						if ((!cpCore.doc.authContext.visit.ExcludeFromAnalytics) & (cpCore.doc.page.ContactMemberID != 0) && (cpCore.webServer.requestBrowser.IndexOf("kmahttp", System.StringComparison.OrdinalIgnoreCase) + 1 == 0))
						{
							if (cpCore.doc.page.AllowHitNotification)
							{
								PageName = cpCore.doc.page.name;
								if (string.IsNullOrEmpty(PageName))
								{
									PageName = cpCore.doc.page.MenuHeadline;
									if (string.IsNullOrEmpty(PageName))
									{
										PageName = cpCore.doc.page.Headline;
										if (string.IsNullOrEmpty(PageName))
										{
											PageName = "[no name]";
										}
									}
								}
								string Body = "";
								Body = Body + "<p><b>Page Hit Notification.</b></p>";
								Body = Body + "<p>This email was sent to you by the Contensive Server as a notification of the following content viewing details.</p>";
								Body = Body + genericController.StartTable(4, 1, 1);
								Body = Body + "<tr><td align=\"right\" width=\"150\" Class=\"ccPanelHeader\">Description<br><img alt=\"image\" src=\"http://" + cpCore.webServer.requestDomain + "/ccLib/images/spacer.gif\" width=\"150\" height=\"1\"></td><td align=\"left\" width=\"100%\" Class=\"ccPanelHeader\">Value</td></tr>";
								Body = Body + getTableRow("Domain", cpCore.webServer.requestDomain, true);
								Body = Body + getTableRow("Link", cpCore.webServer.requestUrl, false);
								Body = Body + getTableRow("Page Name", PageName, true);
								Body = Body + getTableRow("Member Name", cpCore.doc.authContext.user.name, false);
								Body = Body + getTableRow("Member #", Convert.ToString(cpCore.doc.authContext.user.id), true);
								Body = Body + getTableRow("Visit Start Time", Convert.ToString(cpCore.doc.authContext.visit.StartTime), false);
								Body = Body + getTableRow("Visit #", Convert.ToString(cpCore.doc.authContext.visit.id), true);
								Body = Body + getTableRow("Visit IP", cpCore.webServer.requestRemoteIP, false);
								Body = Body + getTableRow("Browser ", cpCore.webServer.requestBrowser, true);
								Body = Body + getTableRow("Visitor #", Convert.ToString(cpCore.doc.authContext.visitor.ID), false);
								Body = Body + getTableRow("Visit Authenticated", Convert.ToString(cpCore.doc.authContext.visit.VisitAuthenticated), true);
								Body = Body + getTableRow("Visit Referrer", cpCore.doc.authContext.visit.HTTP_REFERER, false);
								Body = Body + kmaEndTable;
								cpCore.email.sendPerson(cpCore.doc.page.ContactMemberID, cpCore.siteProperties.getText("EmailFromAddress", "info@" + cpCore.webServer.requestDomain), "Page Hit Notification", Body, false, true, 0, "", false);
							}
						}
						//
						// -- Process Trigger Conditions
						ConditionID = cpCore.doc.page.TriggerConditionID;
						ConditionGroupID = cpCore.doc.page.TriggerConditionGroupID;
						main_AddGroupID = cpCore.doc.page.TriggerAddGroupID;
						RemoveGroupID = cpCore.doc.page.TriggerRemoveGroupID;
						SystemEMailID = cpCore.doc.page.TriggerSendSystemEmailID;
						switch (ConditionID)
						{
							case 1:
								//
								// Always
								//
								if (SystemEMailID != 0)
								{
									cpCore.email.sendSystem_Legacy(cpCore.db.getRecordName("System Email", SystemEMailID), "", cpCore.doc.authContext.user.id);
								}
								if (main_AddGroupID != 0)
								{
									groupController.group_AddGroupMember(cpCore, groupController.group_GetGroupName(cpCore, main_AddGroupID));
								}
								if (RemoveGroupID != 0)
								{
									groupController.group_DeleteGroupMember(cpCore, groupController.group_GetGroupName(cpCore, RemoveGroupID));
								}
								break;
							case 2:
								//
								// If in Condition Group
								//
								if (ConditionGroupID != 0)
								{
									if (cpCore.doc.authContext.IsMemberOfGroup2(cpCore, groupController.group_GetGroupName(cpCore, ConditionGroupID)))
									{
										if (SystemEMailID != 0)
										{
											cpCore.email.sendSystem_Legacy(cpCore.db.getRecordName("System Email", SystemEMailID), "", cpCore.doc.authContext.user.id);
										}
										if (main_AddGroupID != 0)
										{
											groupController.group_AddGroupMember(cpCore, groupController.group_GetGroupName(cpCore, main_AddGroupID));
										}
										if (RemoveGroupID != 0)
										{
											groupController.group_DeleteGroupMember(cpCore, groupController.group_GetGroupName(cpCore, RemoveGroupID));
										}
									}
								}
								break;
							case 3:
								//
								// If not in Condition Group
								//
								if (ConditionGroupID != 0)
								{
									if (!cpCore.doc.authContext.IsMemberOfGroup2(cpCore, groupController.group_GetGroupName(cpCore, ConditionGroupID)))
									{
										if (main_AddGroupID != 0)
										{
											groupController.group_AddGroupMember(cpCore, groupController.group_GetGroupName(cpCore, main_AddGroupID));
										}
										if (RemoveGroupID != 0)
										{
											groupController.group_DeleteGroupMember(cpCore, groupController.group_GetGroupName(cpCore, RemoveGroupID));
										}
										if (SystemEMailID != 0)
										{
											cpCore.email.sendSystem_Legacy(cpCore.db.getRecordName("System Email", SystemEMailID), "", cpCore.doc.authContext.user.id);
										}
									}
								}
								break;
						}
						//End If
						//Call app.closeCS(CS)
					}
					//
					//---------------------------------------------------------------------------------
					// ----- Add in ContentPadding (a table around content with the appropriate padding added)
					//---------------------------------------------------------------------------------
					//
					returnHtml = getContentBoxWrapper(cpCore, returnHtml, cpCore.doc.page.ContentPadding);
					//
					//---------------------------------------------------------------------------------
					// ----- Set Headers
					//---------------------------------------------------------------------------------
					//
					if (DateModified != DateTime.MinValue)
					{
						cpCore.webServer.addResponseHeader("LAST-MODIFIED", genericController.GetGMTFromDate(DateModified));
					}
					//
					//---------------------------------------------------------------------------------
					// ----- Store page javascript
					//---------------------------------------------------------------------------------
					//
					cpCore.html.addScriptCode_onLoad(cpCore.doc.page.JSOnLoad, "page content");
					cpCore.html.addScriptCode_head(cpCore.doc.page.JSHead, "page content");
					if (cpCore.doc.page.JSFilename != "")
					{
						cpCore.html.addScriptLink_Head(genericController.getCdnFileLink(cpCore, cpCore.doc.page.JSFilename), "page content");
					}
					cpCore.html.addScriptCode_body(cpCore.doc.page.JSEndBody, "page content");
					//
					//---------------------------------------------------------------------------------
					// Set the Meta Content flag
					//---------------------------------------------------------------------------------
					//
					cpCore.html.addTitle(genericController.encodeHTML(cpCore.doc.page.pageTitle), "page content");
					cpCore.html.addMetaDescription(genericController.encodeHTML(cpCore.doc.page.metaDescription), "page content");
					cpCore.html.addHeadTag(cpCore.doc.page.OtherHeadTags, "page content");
					cpCore.html.addMetaKeywordList(cpCore.doc.page.MetaKeywordList, "page content");
					//
					Dictionary<string, string> instanceArguments = new Dictionary<string, string>();
					instanceArguments.Add("CSPage", "-1");
					CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext()
					{
						instanceGuid = "-1",
						instanceArguments = instanceArguments
					};
					//
					// -- OnPageStartEvent
					cpCore.doc.bodyContent = returnHtml;
					executeContext.addonType = CPUtilsBaseClass.addonContext.ContextOnPageStart;
					List<addonModel> addonList = Models.Entity.addonModel.createList_OnPageStartEvent(cpCore, new List<string>());
					foreach (Models.Entity.addonModel addon in addonList)
					{
						cpCore.doc.bodyContent = cpCore.addon.execute(addon, executeContext) + cpCore.doc.bodyContent;
						//AddonContent = cpCore.addon.execute_legacy5(addon.id, addon.name, "CSPage=-1", CPUtilsBaseClass.addonContext.ContextOnPageStart, "", 0, "", -1)
					}
					returnHtml = cpCore.doc.bodyContent;
					//
					// -- OnPageEndEvent / filter
					cpCore.doc.bodyContent = returnHtml;
					executeContext.addonType = CPUtilsBaseClass.addonContext.ContextOnPageEnd;
					foreach (addonModel addon in cpCore.addonCache.getOnPageEndAddonList)
					{
						cpCore.doc.bodyContent += cpCore.addon.execute(addon, executeContext);
						//cpCore.doc.bodyContent &= cpCore.addon.execute_legacy5(addon.id, addon.name, "CSPage=-1", CPUtilsBaseClass.addonContext.ContextOnPageStart, "", 0, "", -1)
					}
					returnHtml = cpCore.doc.bodyContent;
					//
				}
				//
				// -- title
				cpCore.html.addTitle(cpCore.doc.page.name);
				//
				// -- add contentid and sectionid
				cpCore.html.addHeadTag("<meta name=\"contentId\" content=\"" + cpCore.doc.page.id + "\" >", "page content");
				//
				// Display Admin Warnings with Edits for record errors
				//
				if (cpCore.doc.adminWarning != "")
				{
					//
					if (cpCore.doc.adminWarningPageID != 0)
					{
						cpCore.doc.adminWarning = cpCore.doc.adminWarning + "</p>" + cpCore.html.main_GetRecordEditLink2("Page Content", cpCore.doc.adminWarningPageID, true, "Page " + cpCore.doc.adminWarningPageID, cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) + "&nbsp;Edit the page<p>";
						cpCore.doc.adminWarningPageID = 0;
					}
					returnHtml = ""
					+ cpCore.html.html_GetAdminHintWrapper(cpCore.doc.adminWarning) + returnHtml + "";
					cpCore.doc.adminWarning = "";
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return returnHtml;
		}
		//
		//====================================================================================================
		/// <summary>
		/// GetHtmlBody_GetSection_GetContentBox
		/// </summary>
		/// <param name="PageID"></param>
		/// <param name="rootPageId"></param>
		/// <param name="RootPageContentName"></param>
		/// <param name="OrderByClause"></param>
		/// <param name="AllowChildPageList"></param>
		/// <param name="AllowReturnLink"></param>
		/// <param name="ArchivePages"></param>
		/// <param name="ignoreMe"></param>
		/// <param name="UseContentWatchLink"></param>
		/// <param name="allowPageWithoutSectionDisplay"></param>
		/// <returns></returns>
		//
		internal static string getContentBox_content(coreClass cpcore, string OrderByClause, bool AllowChildPageList, bool AllowReturnLink, bool ArchivePages, int ignoreMe, bool UseContentWatchLink, bool allowPageWithoutSectionDisplay)
		{
			string result = "";
			try
			{
				bool isEditing = false;
				string LiveBody = null;
				//
				if (cpcore.doc.continueProcessing)
				{
					if (cpcore.doc.redirectLink == "")
					{
						isEditing = cpcore.doc.authContext.isEditing(pageContentModel.contentName);
						//
						// ----- Render the Body
						LiveBody = getContentBox_content_Body(cpcore, OrderByClause, AllowChildPageList, false, cpcore.doc.pageToRootList.Last.id, AllowReturnLink, pageContentModel.contentName, ArchivePages);
						bool isRootPage = (cpcore.doc.pageToRootList.Count == 1);
						if (cpcore.doc.authContext.isAdvancedEditing(cpcore, ""))
						{
							result = result + cpcore.html.main_GetRecordEditLink(pageContentModel.contentName, cpcore.doc.page.id, (!isRootPage)) + LiveBody;
						}
						else if (isEditing)
						{
							result = result + cpcore.html.getEditWrapper("", cpcore.html.main_GetRecordEditLink(pageContentModel.contentName, cpcore.doc.page.id, (!isRootPage)) + LiveBody);
						}
						else
						{
							result = result + LiveBody;
						}
					}
				}
				//
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
			}
			return result;
		}
		//
		//====================================================================================================
		/// <summary>
		/// render the page content
		/// </summary>
		/// <param name="ContentName"></param>
		/// <param name="ContentID"></param>
		/// <param name="OrderByClause"></param>
		/// <param name="AllowChildList"></param>
		/// <param name="Authoring"></param>
		/// <param name="rootPageId"></param>
		/// <param name="AllowReturnLink"></param>
		/// <param name="RootPageContentName"></param>
		/// <param name="ArchivePage"></param>
		/// <returns></returns>

		internal static string getContentBox_content_Body(coreClass cpcore, string OrderByClause, bool AllowChildList, bool Authoring, int rootPageId, bool AllowReturnLink, string RootPageContentName, bool ArchivePage)
		{
			string result = "";
			try
			{
				bool allowChildListComposite = AllowChildList && cpcore.doc.page.AllowChildListDisplay;
				bool allowReturnLinkComposite = AllowReturnLink && cpcore.doc.page.AllowReturnLinkDisplay;
				string bodyCopy = cpcore.doc.page.Copyfilename.content;
				string breadCrumb = "";
				string BreadCrumbDelimiter = null;
				string BreadCrumbPrefix = null;
				bool isRootPage = cpcore.doc.pageToRootList.Count.Equals(1);
				//
				if (allowReturnLinkComposite && (!isRootPage))
				{
					//
					// ----- Print Heading if not at root Page
					//
					BreadCrumbPrefix = cpcore.siteProperties.getText("BreadCrumbPrefix", "Return to");
					BreadCrumbDelimiter = cpcore.siteProperties.getText("BreadCrumbDelimiter", " &gt; ");
					breadCrumb = cpcore.doc.getReturnBreadcrumb(RootPageContentName, cpcore.doc.page.ParentID, rootPageId, "", ArchivePage, BreadCrumbDelimiter);
					if (!string.IsNullOrEmpty(breadCrumb))
					{
						breadCrumb = "\r" + "<p class=\"ccPageListNavigation\">" + BreadCrumbPrefix + " " + breadCrumb + "</p>";
					}
				}
				result = result + breadCrumb;
				//
				if (true)
				{
					string IconRow = "";
					if ((!cpcore.doc.authContext.visit.Bot) & (cpcore.doc.page.AllowPrinterVersion | cpcore.doc.page.AllowEmailPage))
					{
						//
						// not a bot, and either print or email allowed
						//
						if (cpcore.doc.page.AllowPrinterVersion)
						{
							string QueryString = cpcore.doc.refreshQueryString;
							QueryString = genericController.ModifyQueryString(QueryString, rnPageId, genericController.encodeText(cpcore.doc.page.id), true);
							QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, HardCodedPagePrinterVersion, true);
							string Caption = cpcore.siteProperties.getText("PagePrinterVersionCaption", "Printer Version");
							Caption = genericController.vbReplace(Caption, " ", "&nbsp;");
							IconRow = IconRow + "\r" + "&nbsp;&nbsp;<a href=\"" + genericController.encodeHTML(cpcore.webServer.requestPage + "?" + QueryString) + "\" target=\"_blank\"><img alt=\"image\" src=\"/ccLib/images/IconSmallPrinter.gif\" width=\"13\" height=\"13\" border=\"0\" align=\"absmiddle\"></a>&nbsp<a href=\"" + genericController.encodeHTML(cpcore.webServer.requestPage + "?" + QueryString) + "\" target=\"_blank\" style=\"text-decoration:none! important;font-family:sanserif,verdana,helvetica;font-size:11px;\">" + Caption + "</a>";
						}
						if (cpcore.doc.page.AllowEmailPage)
						{
							string QueryString = cpcore.doc.refreshQueryString;
							if (!string.IsNullOrEmpty(QueryString))
							{
								QueryString = "?" + QueryString;
							}
							string EmailBody = cpcore.webServer.requestProtocol + cpcore.webServer.requestDomain + cpcore.webServer.requestPathPage + QueryString;
							string Caption = cpcore.siteProperties.getText("PageAllowEmailCaption", "Email This Page");
							Caption = genericController.vbReplace(Caption, " ", "&nbsp;");
							IconRow = IconRow + "\r" + "&nbsp;&nbsp;<a HREF=\"mailto:?SUBJECT=You might be interested in this&amp;BODY=" + EmailBody + "\"><img alt=\"image\" src=\"/ccLib/images/IconSmallEmail.gif\" width=\"13\" height=\"13\" border=\"0\" align=\"absmiddle\"></a>&nbsp;<a HREF=\"mailto:?SUBJECT=You might be interested in this&amp;BODY=" + EmailBody + "\" style=\"text-decoration:none! important;font-family:sanserif,verdana,helvetica;font-size:11px;\">" + Caption + "</a>";
						}
					}
					if (!string.IsNullOrEmpty(IconRow))
					{
						result = result + "\r" + "<div style=\"text-align:right;\">"
						+ genericController.htmlIndent(IconRow) + "\r" + "</div>";
					}
				}
				//
				// ----- Start Text Search
				//
				string Cell = "";
				if (cpcore.doc.authContext.isQuickEditing(cpcore, pageContentModel.contentName))
				{
					Cell = Cell + cpcore.doc.getQuickEditing(rootPageId, OrderByClause, AllowChildList, AllowReturnLink, ArchivePage, cpcore.doc.page.ContactMemberID, cpcore.doc.page.ChildListSortMethodID, allowChildListComposite, ArchivePage);
				}
				else
				{
					//
					// ----- Headline
					//
					if (cpcore.doc.page.Headline != "")
					{
						string headline = cpcore.html.main_encodeHTML(cpcore.doc.page.Headline);
						Cell = Cell + "\r" + "<h1>" + headline + "</h1>";
						//
						// Add AC end here to force the end of any left over AC tags (like language)
						Cell = Cell + ACTagEnd;
					}
					//
					// ----- Page Copy
					if (string.IsNullOrEmpty(bodyCopy))
					{
						//
						// Page copy is empty if  Links Enabled put in a blank line to separate edit from add tag
						if (cpcore.doc.authContext.isEditing(pageContentModel.contentName))
						{
							bodyCopy = "\r" + "<p><!-- Empty Content Placeholder --></p>";
						}
					}
					else
					{
						bodyCopy = bodyCopy + "\r" + ACTagEnd;
					}
					//
					// ----- Wrap content body
					Cell = Cell + "\r" + "<!-- ContentBoxBodyStart -->"
						+ genericController.htmlIndent(bodyCopy) + "\r" + "<!-- ContentBoxBodyEnd -->";
					//
					// ----- Child pages
					if (allowChildListComposite || cpcore.doc.authContext.isEditingAnything())
					{
						if (!allowChildListComposite)
						{
							Cell = Cell + cpcore.html.html_GetAdminHintWrapper("Automatic Child List display is disabled for this page. It is displayed here because you are in editing mode. To enable automatic child list display, see the features tab for this page.");
						}
						bool AddonStatusOK = false;
						Models.Entity.addonModel addon = Models.Entity.addonModel.create(cpcore, cpcore.siteProperties.childListAddonID);
						CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext()
						{
							addonType = CPUtilsBaseClass.addonContext.ContextPage,
							hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext()
							{
								contentName = Models.Entity.pageContentModel.contentName,
								fieldName = "",
								recordId = cpcore.doc.page.id
							},
							instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpcore, cpcore.doc.page.ChildListInstanceOptions),
							instanceGuid = PageChildListInstanceID,
							wrapperID = cpcore.siteProperties.defaultWrapperID
						};
						Cell += cpcore.addon.execute(addon, executeContext);
						//Cell = Cell & cpcore.addon.execute_legacy2(cpcore.siteProperties.childListAddonID, "", cpcore.doc.page.ChildListInstanceOptions, CPUtilsBaseClass.addonContext.ContextPage, Models.Entity.pageContentModel.contentName, cpcore.doc.page.id, "", PageChildListInstanceID, False, cpcore.siteProperties.defaultWrapperID, "", AddonStatusOK, Nothing)
					}
				}
				//
				// ----- End Text Search
				result = result + "\r" + "<!-- TextSearchStart -->"
					+ genericController.htmlIndent(Cell) + "\r" + "<!-- TextSearchEnd -->";
				//
				// ----- Page See Also
				if (cpcore.doc.page.AllowSeeAlso)
				{
					result = result + "\r" + "<div>"
						+ genericController.htmlIndent(getSeeAlso(cpcore, pageContentModel.contentName, cpcore.doc.page.id)) + "\r" + "</div>";
				}
				//
				// ----- Allow More Info
				if ((cpcore.doc.page.ContactMemberID != 0) & cpcore.doc.page.AllowMoreInfo)
				{
					result = result + "\r" + "<ac TYPE=\"" + ACTypeContact + "\">";
				}
				//
				// ----- Feedback
				if ((cpcore.doc.page.ContactMemberID != 0) & cpcore.doc.page.AllowFeedback)
				{
					result = result + "\r" + "<ac TYPE=\"" + ACTypeFeedback + "\">";
				}
				//
				// ----- Last Modified line
				if ((cpcore.doc.page.ModifiedDate != DateTime.MinValue) & cpcore.doc.page.AllowLastModifiedFooter)
				{
					result = result + "\r" + "<p>This page was last modified " + cpcore.doc.page.ModifiedDate.ToString("G");
					if (cpcore.doc.authContext.isAuthenticatedAdmin(cpcore))
					{
						if (cpcore.doc.page.ModifiedBy == 0)
						{
							result = result + " (admin only: modified by unknown)";
						}
						else
						{
							string personName = cpcore.db.getRecordName("people", cpcore.doc.page.ModifiedBy);
							if (string.IsNullOrEmpty(personName))
							{
								result = result + " (admin only: modified by person with unnamed or deleted record #" + cpcore.doc.page.ModifiedBy + ")";
							}
							else
							{
								result = result + " (admin only: modified by " + personName + ")";
							}
						}
					}
					result = result + "</p>";
				}
				//
				// ----- Last Reviewed line
				if ((cpcore.doc.page.DateReviewed != DateTime.MinValue) & cpcore.doc.page.AllowReviewedFooter)
				{
					result = result + "\r" + "<p>This page was last reviewed " + cpcore.doc.page.DateReviewed.ToString("");
					if (cpcore.doc.authContext.isAuthenticatedAdmin(cpcore))
					{
						if (cpcore.doc.page.ReviewedBy == 0)
						{
							result = result + " (by unknown)";
						}
						else
						{
							string personName = cpcore.db.getRecordName("people", cpcore.doc.page.ReviewedBy);
							if (string.IsNullOrEmpty(personName))
							{
								result = result + " (by person with unnamed or deleted record #" + cpcore.doc.page.ReviewedBy + ")";
							}
							else
							{
								result = result + " (by " + personName + ")";
							}
						}
						result = result + ".</p>";
					}
				}
				//
				// ----- Page Content Message Footer
				if (cpcore.doc.page.AllowMessageFooter)
				{
					string pageContentMessageFooter = cpcore.siteProperties.getText("PageContentMessageFooter", "");
					if (!string.IsNullOrEmpty(pageContentMessageFooter))
					{
						result = result + "\r" + "<p>" + pageContentMessageFooter + "</p>";
					}
				}
				//Call cpcore.db.cs_Close(CS)
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
			}
			return result;
		}
		//
		//=============================================================================
		// Print the See Also listing
		//   ContentName is the name of the parent table
		//   RecordID is the parent RecordID
		//=============================================================================
		//
		public static string getSeeAlso(coreClass cpcore, string ContentName, int RecordID)
		{
			string result = "";
			try
			{
				int CS = 0;
				string SeeAlsoLink = null;
				int ContentID = 0;
				int SeeAlsoCount = 0;
				string Copy = null;
				string MethodName = null;
				string iContentName = null;
				int iRecordID = 0;
				bool IsEditingLocal = false;
				//
				iContentName = genericController.encodeText(ContentName);
				iRecordID = genericController.EncodeInteger(RecordID);
				//
				MethodName = "result";
				//
				SeeAlsoCount = 0;
				if (iRecordID > 0)
				{
					ContentID = models.complex.cdefmodel.getcontentid(cpcore, iContentName);
					if (ContentID > 0)
					{
						//
						// ----- Set authoring only for valid ContentName
						//
						IsEditingLocal = cpcore.doc.authContext.isEditing(iContentName);
					}
					else
					{
						//
						// ----- if iContentName was bad, maybe they put table in, no authoring
						//
						ContentID = Models.Complex.cdefModel.getContentIDByTablename(cpcore, iContentName);
					}
					if (ContentID > 0)
					{
						//
						CS = cpcore.db.csOpen("See Also", "((active<>0)AND(ContentID=" + ContentID + ")AND(RecordID=" + iRecordID + "))");
						while (cpcore.db.csOk(CS))
						{
							SeeAlsoLink = (cpcore.db.csGetText(CS, "Link"));
							if (!string.IsNullOrEmpty(SeeAlsoLink))
							{
								result = result + "\r" + "<li class=\"ccListItem\">";
								if (genericController.vbInstr(1, SeeAlsoLink, "://") == 0)
								{
									SeeAlsoLink = cpcore.webServer.requestProtocol + SeeAlsoLink;
								}
								if (IsEditingLocal)
								{
									result = result + cpcore.html.main_GetRecordEditLink2("See Also", (cpcore.db.csGetInteger(CS, "ID")), false, "", cpcore.doc.authContext.isEditing("See Also"));
								}
								result = result + "<a href=\"" + genericController.encodeHTML(SeeAlsoLink) + "\" target=\"_blank\">" + (cpcore.db.csGetText(CS, "Name")) + "</A>";
								Copy = (cpcore.db.csGetText(CS, "Brief"));
								if (!string.IsNullOrEmpty(Copy))
								{
									result = result + "<br >" + genericController.AddSpan(Copy, "ccListCopy");
								}
								SeeAlsoCount = SeeAlsoCount + 1;
								result = result + "</li>";
							}
							cpcore.db.csGoNext(CS);
						}
						cpcore.db.csClose(CS);
						//
						if (IsEditingLocal)
						{
							SeeAlsoCount = SeeAlsoCount + 1;
							result = result + "\r" + "<li class=\"ccListItem\">" + cpcore.html.main_GetRecordAddLink("See Also", "RecordID=" + iRecordID + ",ContentID=" + ContentID) + "</LI>";
						}
					}
					//
					if (SeeAlsoCount == 0)
					{
						result = "";
					}
					else
					{
						result = "<p>See Also" + "\r" + "<ul class=\"ccList\">" + genericController.htmlIndent(result) + "\r" + "</ul></p>";
					}
				}
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
			}
			return result;
		}
		//
		//========================================================================
		// Print the "for more information, please contact" line
		//
		//========================================================================
		//
		public string main_GetMoreInfo(coreClass cpcore, int contactMemberID)
		{
			string tempmain_GetMoreInfo = null;
			string result = "";
			try
			{
				tempmain_GetMoreInfo = getMoreInfoHtml(cpcore, genericController.EncodeInteger(contactMemberID));
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
			}
			return result;
		}
		//
		//========================================================================
		// ----- prints a link to the feedback popup form
		//
		//   Creates a sub-form that when submitted, is logged by the notes
		//   system (in MembersLib right now). When submitted, it prints a thank you
		//   message.
		//
		//========================================================================
		//
		public static string main_GetFeedbackForm(coreClass cpcore, string ContentName, int RecordID, int ToMemberID, string headline = "")
		{
			string result = "";
			try
			{
				string Panel = null;
				string Copy = null;
				string FeedbackButton = null;
				string NoteCopy = string.Empty;
				string NoteFromEmail = null;
				string NoteFromName = null;
				int CS = 0;
				string iContentName = null;
				int iRecordID = 0;
				int iToMemberID = 0;
				string iHeadline = null;
				//
				iContentName = genericController.encodeText(ContentName);
				iRecordID = genericController.EncodeInteger(RecordID);
				iToMemberID = genericController.EncodeInteger(ToMemberID);
				iHeadline = genericController.encodeEmptyText(headline, "");
				//
				const string FeedbackButtonSubmit = "Submit";
				//
				FeedbackButton = cpcore.docProperties.getText("fbb");
				switch (FeedbackButton)
				{
					case FeedbackButtonSubmit:
						//
						// ----- form was submitted, save the note, send it and say thanks
						//
						NoteFromName = cpcore.docProperties.getText("NoteFromName");
						NoteFromEmail = cpcore.docProperties.getText("NoteFromEmail");
						//
						NoteCopy = NoteCopy + "Feedback Submitted" + BR;
						NoteCopy = NoteCopy + "From " + NoteFromName + " at " + NoteFromEmail + BR;
						NoteCopy = NoteCopy + "Replying to:" + BR;
						if (!string.IsNullOrEmpty(iHeadline))
						{
							NoteCopy = NoteCopy + "    Article titled [" + iHeadline + "]" + BR;
						}
						NoteCopy = NoteCopy + "    Record [" + iRecordID + "] in Content Definition [" + iContentName + "]" + BR;
						NoteCopy = NoteCopy + BR;
						NoteCopy = NoteCopy + "<b>Comments</b>" + BR;
						//
						Copy = cpcore.docProperties.getText("NoteCopy");
						if (string.IsNullOrEmpty(Copy))
						{
							NoteCopy = NoteCopy + "[no comments entered]" + BR;
						}
						else
						{
							NoteCopy = NoteCopy + cpcore.html.convertCRLFToHtmlBreak(Copy) + BR;
						}
						//
						NoteCopy = NoteCopy + BR;
						NoteCopy = NoteCopy + "<b>Content on which the comments are based</b>" + BR;
						//
						CS = cpcore.db.csOpen(iContentName, "ID=" + iRecordID);
						Copy = "[the content of this page is not available]" + BR;
						if (cpcore.db.csOk(CS))
						{
							Copy = (cpcore.db.csGet(CS, "copyFilename"));
							//Copy = main_EncodeContent5(Copy, c.authcontext.user.userid, iContentName, iRecordID, 0, False, False, True, True, False, True, "", "", False, 0)
						}
						NoteCopy = NoteCopy + Copy + BR;
						cpcore.db.csClose(CS);
						//
						cpcore.email.sendPerson(iToMemberID, NoteFromEmail, "Feedback Form Submitted", NoteCopy, false, true, 0, "", false);
						//
						// ----- Note sent, say thanks
						//
						result = result + "<p>Thank you. Your feedback was received.</p>";
						break;
					default:
						//
						// ----- print the feedback submit form
						//
						Panel = "<form Action=\"" + cpcore.webServer.serverFormActionURL + "?" + cpcore.doc.refreshQueryString + "\" Method=\"post\">";
						Panel = Panel + "<table border=\"0\" cellpadding=\"4\" cellspacing=\"0\" width=\"100%\">";
						Panel = Panel + "<tr>";
						Panel = Panel + "<td colspan=\"2\"><p>Your feedback is welcome</p></td>";
						Panel = Panel + "</tr><tr>";
						//
						// ----- From Name
						//
						Copy = cpcore.doc.authContext.user.name;
						Panel = Panel + "<td align=\"right\" width=\"100\"><p>Your Name</p></td>";
						Panel = Panel + "<td align=\"left\"><input type=\"text\" name=\"NoteFromName\" value=\"" + genericController.encodeHTML(Copy) + "\"></span></td>";
						Panel = Panel + "</tr><tr>";
						//
						// ----- From Email address
						//
						Copy = cpcore.doc.authContext.user.Email;
						Panel = Panel + "<td align=\"right\" width=\"100\"><p>Your Email</p></td>";
						Panel = Panel + "<td align=\"left\"><input type=\"text\" name=\"NoteFromEmail\" value=\"" + genericController.encodeHTML(Copy) + "\"></span></td>";
						Panel = Panel + "</tr><tr>";
						//
						// ----- Message
						//
						Copy = "";
						Panel = Panel + "<td align=\"right\" width=\"100\" valign=\"top\"><p>Feedback</p></td>";
						Panel = Panel + "<td>" + cpcore.html.html_GetFormInputText2("NoteCopy", Copy, 4, 40, "TextArea", false) + "</td>";
						//Panel = Panel & "<td><textarea ID=""TextArea"" rows=""4"" cols=""40"" name=""NoteCopy"">" & Copy & "</textarea></td>"
						Panel = Panel + "</tr><tr>";
						//
						// ----- submit button
						//
						Panel = Panel + "<td>&nbsp;</td>";
						Panel = Panel + "<td><input type=\"submit\" name=\"fbb\" value=\"" + FeedbackButtonSubmit + "\"></td>";
						Panel = Panel + "</tr></table>";
						Panel = Panel + "</form>";
						//
						result = Panel;
						break;
				}
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
			}
			return result;
		}



	}
}