using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

//
using Controllers;


using System.Xml;
using Contensive.Core;
using Models.Entity;
//
namespace Contensive.Addons.AdminSite
{
	public partial class getAdminSiteClass : Contensive.BaseClasses.AddonBaseClass
	{

		//
		//
		//=============================================================================================
		//   Create a duplicate record
		//=============================================================================================
		//
		private void ProcessActionDuplicate(Models.Complex.cdefModel adminContent, editRecordClass editRecord)
		{
			try
			{
				// converted array to dictionary - Dim FieldPointer As Integer
				//
				if (!(cpCore.doc.debug_iUserError != ""))
				{
					switch (genericController.vbUCase(adminContent.ContentTableName))
					{
						case "CCEMAIL":
							//
							// --- preload array with values that may not come back in response
							//
							LoadEditRecord(adminContent, editRecord);
							LoadEditRecord_Request(adminContent, editRecord);
							//
							if (!(cpCore.doc.debug_iUserError != ""))
							{
								//
								// ----- Convert this to the Duplicate
								//
								if (adminContent.fields.ContainsKey("submitted"))
								{
									editRecord.fieldsLc("submitted").value = false;
								}
								if (adminContent.fields.ContainsKey("sent"))
								{
									editRecord.fieldsLc("sent").value = false;
								}
								//
								editRecord.id = 0;
								cpCore.doc.addRefreshQueryString("id", genericController.encodeText(editRecord.id));
							}
							break;
						default:
							//
							//
							//
							//
							// --- preload array with values that may not come back in response
							//
							LoadEditRecord(adminContent, editRecord);
							LoadEditRecord_Request(adminContent, editRecord);
							//
							if (!(cpCore.doc.debug_iUserError != ""))
							{
								//
								// ----- Convert this to the Duplicate
								//
								editRecord.id = 0;
								//
								// block fields that should not duplicate
								//
								if (editRecord.fieldsLc.ContainsKey("ccguid"))
								{
									editRecord.fieldsLc("ccguid").value = "";
								}
								//
								if (editRecord.fieldsLc.ContainsKey("dateadded"))
								{
									editRecord.fieldsLc("dateadded").value = DateTime.MinValue;
								}
								//
								if (editRecord.fieldsLc.ContainsKey("modifieddate"))
								{
									editRecord.fieldsLc("modifieddate").value = DateTime.MinValue;
								}
								//
								if (editRecord.fieldsLc.ContainsKey("modifiedby"))
								{
									editRecord.fieldsLc("modifiedby").value = 0;
								}
								//
								// block fields that must be unique
								//
								foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in adminContent.fields)
								{
									Models.Complex.CDefFieldModel field = keyValuePair.Value;
									if (genericController.vbLCase(field.nameLc) == "email")
									{
										if ((adminContent.ContentTableName.ToLower() == "ccmembers") && (genericController.EncodeBoolean(cpCore.siteProperties.getBoolean("allowemaillogin", false))))
										{
											editRecord.fieldsLc(field.nameLc).value = "";
										}
									}
									if (field.UniqueName)
									{
										editRecord.fieldsLc(field.nameLc).value = "";
									}
								}
								//
								cpCore.doc.addRefreshQueryString("id", genericController.encodeText(editRecord.id));
							}
							//Call cpCore.htmldoc.main_AddUserError("The create duplicate action is not supported for this content.")
							break;
					}
					AdminForm = AdminSourceForm;
					AdminAction = AdminActionNop; // convert so action can be used in as a refresh
				}
				//
				return;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			handleLegacyClassError3("ProcessActionDuplicate");
			//
		}
		//
		//========================================================================
		// PrintMenuTop()
		//   Prints the menu section of the admin page
		//========================================================================
		//
		private string GetMenuTopMode()
		{
				string tempGetMenuTopMode = null;
			try
			{
				//
				//Const MenuEntryContentName = cnNavigatorEntries
				//
				int CSMenus = 0;
				string Name = null;
				int Id = 0;
				int ParentID = 0;
				bool NewWindow = false;
				string Link = null;
				string LinkLabel = null;
				int LinkCID = 0;
				int MenuPointer = 0;
				string StyleSheet = "";
				string StyleSheetHover = "";
				string ImageLink = null;
				string ImageOverLink = null;
				DateTime MenuDate = default(DateTime);
				DateTime BakeDate = default(DateTime);
				string BakeName = null;
				string MenuHeader = null;
				List<int> editableCdefIdList = null;
				int ContentID = 0;
				bool IsAdminLocal = false;
				string MenuClose = null;
				int MenuDelimiterPosition = 0;
				bool AccessOK = false;
				//
				const string MenuDelimiter = "\r\n" + "<!-- Menus -->" + "\r\n";
				//
				// Create the menu
				//
				if (AdminMenuModeID == AdminMenuModeTop)
				{
					//
					// ----- Get the baked version
					//
					BakeName = "AdminMenu" + cpCore.doc.authContext.user.id.ToString("00000000");
					tempGetMenuTopMode = genericController.encodeText(cpCore.cache.getObject<string>(BakeName));
					MenuDelimiterPosition = genericController.vbInstr(1, tempGetMenuTopMode, MenuDelimiter, Microsoft.VisualBasic.Constants.vbTextCompare);
					if (MenuDelimiterPosition > 1)
					{
						MenuClose = tempGetMenuTopMode.Substring((MenuDelimiterPosition + MenuDelimiter.Length) - 1);
						tempGetMenuTopMode = tempGetMenuTopMode.Substring(0, MenuDelimiterPosition - 1);
					}
					else
					{
						//If GetMenuTopMode = "" Then
						//
						// ----- Bake the menu
						//
						CSMenus = GetMenuCSPointer("");
						//CSMenus = cpCore.app_openCsSql_Rev_Internal("default", GetMenuSQLNew())
						if (cpCore.db.csOk(CSMenus))
						{
							//
							// There are menu items to bake
							//
							IsAdminLocal = cpCore.doc.authContext.isAuthenticatedAdmin(cpCore);
							if (!IsAdminLocal)
							{
								//
								// content managers, need the ContentManagementList
								//
								editableCdefIdList = Models.Complex.cdefModel.getEditableCdefIdList(cpCore);
							}
							else
							{
								editableCdefIdList = new List<int>();
							}
							ImageLink = "";
							ImageOverLink = "";
							while (cpCore.db.csOk(CSMenus))
							{
								ContentID = cpCore.db.csGetInteger(CSMenus, "ContentID");
								if (IsAdminLocal || ContentID == 0)
								{
									AccessOK = true;
								}
								else if (editableCdefIdList.Contains(ContentID))
								{
									AccessOK = true;
								}
								else
								{
									AccessOK = false;
								}
								Id = cpCore.db.csGetInteger(CSMenus, "ID");
								ParentID = cpCore.db.csGetInteger(CSMenus, "ParentID");
								if (AccessOK)
								{
									Link = GetMenuLink(cpCore.db.csGet(CSMenus, "LinkPage"), ContentID);
									if (genericController.vbInstr(1, Link, "?") == 1)
									{
										Link = cpCore.serverConfig.appConfig.adminRoute + Link;
									}
								}
								else
								{
									Link = "";
								}
								LinkLabel = cpCore.db.csGet(CSMenus, "Name");
								//If LinkLabel = "Calendar" Then
								//    Link = Link
								//    End If
								NewWindow = cpCore.db.csGetBoolean(CSMenus, "NewWindow");
								cpCore.menuFlyout.menu_AddEntry(genericController.encodeText(Id), ParentID.ToString(), ImageLink, ImageOverLink, Link, LinkLabel, StyleSheet, StyleSheetHover, NewWindow);

								cpCore.db.csGoNext(CSMenus);
							}
						}
						cpCore.db.csClose(CSMenus);
						//            '
						//            ' Add in top level node for "switch to navigator"
						//            '
						//            Call cpCore.htmldoc.main_AddMenuEntry("GoToNav", 0, "?" & cpCore.main_RefreshQueryString & "&mm=1", "", "", "Switch To Navigator", StyleSheet, StyleSheetHover, False)
						//
						// Create menus
						//
						int ButtonCnt = 0;
						CSMenus = GetMenuCSPointer("(ParentID is null)or(ParentID=0)");
						if (cpCore.db.csOk(CSMenus))
						{
							tempGetMenuTopMode = "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"><tr>";
							ButtonCnt = 0;
							while (cpCore.db.csOk(CSMenus))
							{
								Name = cpCore.db.csGet(CSMenus, "Name");
								Id = cpCore.db.csGetInteger(CSMenus, "ID");
								NewWindow = cpCore.db.csGetBoolean(CSMenus, "NewWindow");
								MenuHeader = cpCore.menuFlyout.getMenu(genericController.encodeText(Id), 0);
								if (!string.IsNullOrEmpty(MenuHeader))
								{
									if (ButtonCnt > 0)
									{
										tempGetMenuTopMode = tempGetMenuTopMode + "<td class=\"ccFlyoutDelimiter\">|</td>";
									}
									//GetMenuTopMode = GetMenuTopMode & "<td width=""1"" class=""ccPanelShadow""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td>"
									//GetMenuTopMode = GetMenuTopMode & "<td width=""1"" class=""ccPanelHilite""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td>"
									//
									// --- Add New GetMenuTopMode Button and leave the column open
									//
									Link = "";
									tempGetMenuTopMode = tempGetMenuTopMode + "<td class=\"ccFlyoutButton\">" + MenuHeader + "</td>";
									// GetMenuTopMode = GetMenuTopMode & "<td><nobr>&nbsp;" & MenuHeader & "&nbsp;</nobr></td>"
								}
								ButtonCnt = ButtonCnt + 1;
								cpCore.db.csGoNext(CSMenus);
							}
							tempGetMenuTopMode = tempGetMenuTopMode + "</tr></table>";
							tempGetMenuTopMode = cpCore.html.main_GetPanel(tempGetMenuTopMode, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 1);
						}
						cpCore.db.csClose(CSMenus);
						//
						// Save the Baked Menu
						//
						MenuClose = cpCore.menuFlyout.menu_GetClose();
						//GetMenuTopMode = GetMenuTopMode & cpCore.main_GetMenuClose
						cpCore.cache.setContent(BakeName, tempGetMenuTopMode + MenuDelimiter + MenuClose, "Navigator Entries,People,Content,Groups,Group Rules");
					}
					cpCore.doc.htmlForEndOfBody = cpCore.doc.htmlForEndOfBody + MenuClose;
				}
				return tempGetMenuTopMode;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			handleLegacyClassError3("GetMenuTopMode");
			//
			return tempGetMenuTopMode;
		}
		//
		//========================================================================
		// Read and save a GetForm_InputCheckList
		//   see GetForm_InputCheckList for an explaination of the input
		//========================================================================
		//
		private void SaveMemberRules(int PeopleID)
		{
			try
			{
				//
				int GroupCount = 0;
				int GroupPointer = 0;
				int CSPointer = 0;
				string MethodName = null;
				int GroupID = 0;
				bool RuleNeeded = false;
				int CSRule = 0;
				DateTime DateExpires = default(DateTime);
				object DateExpiresVariant = null;
				bool RuleActive = false;
				DateTime RuleDateExpires = default(DateTime);
				int MemberRuleID = 0;
				//
				MethodName = "SaveMemberRules";
				//
				// --- create MemberRule records for all selected
				//
				GroupCount = cpCore.docProperties.getInteger("MemberRules.RowCount");
				if (GroupCount > 0)
				{
					for (GroupPointer = 0; GroupPointer < GroupCount; GroupPointer++)
					{
						//
						// ----- Read Response
						//
						GroupID = cpCore.docProperties.getInteger("MemberRules." + GroupPointer + ".ID");
						RuleNeeded = cpCore.docProperties.getBoolean("MemberRules." + GroupPointer);
						DateExpires = cpCore.docProperties.getDate("MemberRules." + GroupPointer + ".DateExpires");
						if (DateExpires == DateTime.MinValue)
						{
							DateExpiresVariant = DBNull.Value;
						}
						else
						{
							DateExpiresVariant = DateExpires;
						}
						//
						// ----- Update Record
						//
						CSRule = cpCore.db.csOpen("Member Rules", "(MemberID=" + PeopleID + ")and(GroupID=" + GroupID + ")",, false,,,, "Active,MemberID,GroupID,DateExpires");
						if (!cpCore.db.csOk(CSRule))
						{
							//
							// No record exists
							//
							if (RuleNeeded)
							{
								//
								// No record, Rule needed, add it
								//
								cpCore.db.csClose(CSRule);
								CSRule = cpCore.db.csInsertRecord("Member Rules");
								if (cpCore.db.csOk(CSRule))
								{
									cpCore.db.csSet(CSRule, "Active", true);
									cpCore.db.csSet(CSRule, "MemberID", PeopleID);
									cpCore.db.csSet(CSRule, "GroupID", GroupID);
									cpCore.db.csSet(CSRule, "DateExpires", DateExpires);
								}
								cpCore.db.csClose(CSRule);
							}
							else
							{
								//
								// No record, no Rule needed, ignore it
								//
								cpCore.db.csClose(CSRule);
							}
						}
						else
						{
							//
							// Record exists
							//
							if (RuleNeeded)
							{
								//
								// record exists, and it is needed, update the DateExpires if changed
								//
								RuleActive = cpCore.db.csGetBoolean(CSRule, "active");
								RuleDateExpires = cpCore.db.csGetDate(CSRule, "DateExpires");
								if ((!RuleActive) || (RuleDateExpires != DateExpires))
								{
									cpCore.db.csSet(CSRule, "Active", true);
									cpCore.db.csSet(CSRule, "DateExpires", DateExpires);
								}
								cpCore.db.csClose(CSRule);
							}
							else
							{
								//
								// record exists and it is not needed, delete it
								//
								MemberRuleID = cpCore.db.csGetInteger(CSRule, "ID");
								cpCore.db.csClose(CSRule);
								cpCore.db.deleteTableRecord("ccMemberRules", MemberRuleID, "Default");
							}
						}
					}
				}
				return;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			handleLegacyClassError3("SaveMemberRules");
		}
		//
		//===========================================================================
		//
		//===========================================================================
		//
		private string GetForm_Error(string UserError, string DeveloperError)
		{
				string tempGetForm_Error = null;
			try
			{
				//
				if (!string.IsNullOrEmpty(DeveloperError))
				{
					throw (new Exception("error [" + DeveloperError + "], user error [" + UserError + "]"));
				}
				if (!string.IsNullOrEmpty(UserError))
				{
					errorController.error_AddUserError(cpCore, UserError);
					tempGetForm_Error = AdminFormErrorOpen + errorController.error_GetUserError(cpCore) + AdminFormErrorClose;
				}
				//
				return tempGetForm_Error;
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			handleLegacyClassError3("GetForm_Error");
			return tempGetForm_Error;
		}
		//
		//=============================================================================
		// Create a child content
		//=============================================================================
		//
		private string GetContentChildTool()
		{
			try
			{
				//
				bool IsEmptyList = false;
				//Dim cmc As cpCoreClass
				//Dim GUIDGenerator As guidClass
				string ccGuid = null;
				bool SupportAddonID = false;
				bool SupportGuid = false;
				string MenuContentName = null;
				int ParentID = 0;
				string ParentName = null;
				int CSEntry = 0;
				int ParentContentID = 0;
				string ParentContentName = null;
				string ChildContentName = "";
				int ChildContentID = 0;
				bool AddAdminMenuEntry = false;
				int CS = 0;
				string MenuName = null;
				bool AdminOnly = false;
				bool DeveloperOnly = false;
				stringBuilderLegacyController Content = new stringBuilderLegacyController();
				string FieldValue = null;
				bool NewGroup = false;
				int GroupID = 0;
				string NewGroupName = "";
				string ButtonBar = null;
				string Button = null;
				adminUIController Adminui = new adminUIController(cpCore);
				string Caption = null;
				string Description = "";
				string ButtonList = "";
				bool BlockForm = false;
				//
				Button = cpCore.docProperties.getText(RequestNameButton);
				if (Button == ButtonCancel)
				{
					//
					//
					//
					return cpCore.webServer.redirect("/" + cpCore.serverConfig.appConfig.adminRoute, "GetContentChildTool, Cancel Button Pressed");
				}
				else if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore))
				{
					//
					//
					//
					ButtonList = ButtonCancel;
					Content.Add(Adminui.GetFormBodyAdminOnly());
				}
				else
				{
					//
					if (Button != ButtonOK)
					{
						//
						// Load defaults
						//
						ParentContentID = cpCore.docProperties.getInteger("ParentContentID");
						if (ParentContentID == 0)
						{
							ParentContentID = Models.Complex.cdefModel.getContentId(cpCore, "Page Content");
						}
						AddAdminMenuEntry = true;
						GroupID = 0;
					}
					else
					{
						//
						// Process input
						//
						ParentContentID = cpCore.docProperties.getInteger("ParentContentID");
						ParentContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ParentContentID);
						ChildContentName = cpCore.docProperties.getText("ChildContentName");
						AddAdminMenuEntry = cpCore.docProperties.getBoolean("AddAdminMenuEntry");
						GroupID = cpCore.docProperties.getInteger("GroupID");
						NewGroup = cpCore.docProperties.getBoolean("NewGroup");
						NewGroupName = cpCore.docProperties.getText("NewGroupName");
						//
						if ((string.IsNullOrEmpty(ParentContentName)) || (string.IsNullOrEmpty(ChildContentName)))
						{
							errorController.error_AddUserError(cpCore, "You must select a parent and provide a child name.");
						}
						else
						{
							//
							// Create Definition
							//
							Description = Description + "<div>&nbsp;</div>"
								+ "<div>Creating content [" + ChildContentName + "] from [" + ParentContentName + "]</div>";
							Models.Complex.cdefModel.createContentChild(cpCore, ChildContentName, ParentContentName, cpCore.doc.authContext.user.id);
							ChildContentID = Models.Complex.cdefModel.getContentId(cpCore, ChildContentName);
							//
							// Create Group and Rule
							//
							if (NewGroup && (!string.IsNullOrEmpty(NewGroupName)))
							{
								CS = cpCore.db.csOpen("Groups", "name=" + cpCore.db.encodeSQLText(NewGroupName));
								if (cpCore.db.csOk(CS))
								{
									Description = Description + "<div>Group [" + NewGroupName + "] already exists, using existing group.</div>";
									GroupID = cpCore.db.csGetInteger(CS, "ID");
								}
								else
								{
									Description = Description + "<div>Creating new group [" + NewGroupName + "]</div>";
									cpCore.db.csClose(CS);
									CS = cpCore.db.csInsertRecord("Groups");
									if (cpCore.db.csOk(CS))
									{
										GroupID = cpCore.db.csGetInteger(CS, "ID");
										cpCore.db.csSet(CS, "Name", NewGroupName);
										cpCore.db.csSet(CS, "Caption", NewGroupName);
									}
								}
								cpCore.db.csClose(CS);
							}
							if (GroupID != 0)
							{
								CS = cpCore.db.csInsertRecord("Group Rules");
								if (cpCore.db.csOk(CS))
								{
									Description = Description + "<div>Assigning group [" + cpCore.db.getRecordName("Groups", GroupID) + "] to edit content [" + ChildContentName + "].</div>";
									cpCore.db.csSet(CS, "GroupID", GroupID);
									cpCore.db.csSet(CS, "ContentID", ChildContentID);
								}
								cpCore.db.csClose(CS);
							}
							//
							// Add Admin Menu Entry
							//
							if (AddAdminMenuEntry)
							{
							}
							//
							Description = Description + "<div>&nbsp;</div>"
								+ "<div>Your new content is ready. <a href=\"?" + RequestNameAdminForm + "=22\">Click here</a> to create another Content Definition, or hit [Cancel] to return to the main menu.</div>";
							ButtonList = ButtonCancel;
							BlockForm = true;
						}
						cpCore.doc.clearMetaData();
						cpCore.cache.invalidateAll();
					}
					//
					// Get the form
					//
					if (!BlockForm)
					{
						Content.Add(Adminui.EditTableOpen);
						//
						FieldValue = "<select size=\"1\" name=\"ParentContentID\" ID=\"\"><option value=\"\">Select One</option>";
						FieldValue = FieldValue + GetContentChildTool_Options(0, ParentContentID);
						FieldValue = FieldValue + "</select>";
						//FieldValue = cpCore.htmldoc.main_GetFormInputSelect2("ParentContentID", CStr(ParentContentID), "Content", "(AllowContentChildTool<>0)")

						Content.Add(Adminui.GetEditRow(FieldValue, "Parent Content Name", "", false, false, ""));
						//
						FieldValue = cpCore.html.html_GetFormInputText2("ChildContentName", ChildContentName, 1, 40);
						Content.Add(Adminui.GetEditRow(FieldValue, "New Child Content Name", "", false, false, ""));
						//
						FieldValue = cpCore.html.html_GetFormInputRadioBox("NewGroup", false.ToString(), NewGroup.ToString()) + cpCore.html.main_GetFormInputSelect2("GroupID", GroupID, "Groups", "", "", "", IsEmptyList) + "(Select a current group)"
							+ "<br>" + cpCore.html.html_GetFormInputRadioBox("NewGroup", true.ToString(), NewGroup.ToString()) + cpCore.html.html_GetFormInputText2("NewGroupName", NewGroupName) + "(Create a new group)";
						Content.Add(Adminui.GetEditRow(FieldValue, "Content Manager Group", "", false, false, ""));
						//            '
						//            FieldValue = cpCore.main_GetFormInputCheckBox2("AddAdminMenuEntry", AddAdminMenuEntry) & "(Add Navigator Entry under Manager Site Content &gt; Advanced)"
						//            Call Content.Add(AdminUI.GetEditRow( FieldValue, "Add Menu Entry", "", False, False, ""))
						//
						Content.Add(Adminui.EditTableClose);
						Content.Add("</td></tr>" + kmaEndTable);
						//
						ButtonList = ButtonOK + "," + ButtonCancel;
					}
					Content.Add(cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormContentChildTool));
				}
				//
				Caption = "Create Content Definition";
				Description = "<div>This tool is used to create content definitions that help segregate your content into authorable segments.</div>" + Description;
				return Adminui.GetBody(Caption, ButtonList, "", false, false, Description, "", 0, Content.Text);
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			handleLegacyClassError3("GetContentChildTool");
		}
		//
		//=============================================================================
		// Create a child content
		//=============================================================================
		//
		private string GetContentChildTool_Options(int ParentID, int DefaultValue)
		{
			string returnOptions = "";
			try
			{
				//
				string SQL = null;
				int CS = 0;
				int RecordID = 0;
				string RecordName = null;
				//
				if (ParentID == 0)
				{
					SQL = "select Name, ID from ccContent where ((ParentID<1)or(Parentid is null)) and (AllowContentChildTool<>0);";
				}
				else
				{
					SQL = "select Name, ID from ccContent where ParentID=" + ParentID + " and (AllowContentChildTool<>0) and not (allowcontentchildtool is null);";
				}
				CS = cpCore.db.csOpenSql_rev("Default", SQL);
				while (cpCore.db.csOk(CS))
				{
					RecordName = cpCore.db.csGet(CS, "Name");
					RecordID = cpCore.db.csGetInteger(CS, "ID");
					if (RecordID == DefaultValue)
					{
						returnOptions = returnOptions + "<option value=\"" + RecordID + "\" selected>" + cpCore.db.csGet(CS, "name") + "</option>";
					}
					else
					{
						returnOptions = returnOptions + "<option value=\"" + RecordID + "\" >" + cpCore.db.csGet(CS, "name") + "</option>";
					}
					returnOptions = returnOptions + GetContentChildTool_Options(RecordID, DefaultValue);
					cpCore.db.csGoNext(CS);
				}
				cpCore.db.csClose(CS);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return returnOptions;
		}
		//
		//========================================================================
		//
		//========================================================================
		//
		private string GetForm_HouseKeepingControl()
		{
				string tempGetForm_HouseKeepingControl = null;
			try
			{
				//
				int WhereCount = 0;
				stringBuilderLegacyController Content = new stringBuilderLegacyController();
				bool AllowContentSpider = false;
				string status = null;
				string TargetDomain = null;
				bool EDGPublishToProduction = false;
				int CSServers = 0;
				string Copy = null;
				string StagingServer = null;
				int PagesFound = 0;
				int PagesComplete = 0;
				string SQL = null;
				string Button = null;
				string SpiderAuthUsername = null;
				string SpiderAuthPassword = null;
				string SpiderAppRootPath = null;
				string SpiderPassword = null;
				string SpiderUsername = null;
				string SPIDERQUERYSTRINGEXCLUDELIST = null;
				string SPIDERQUERYSTRINGIGNORELIST = null;
				//Dim SPIDERREADTIME as integer
				//Dim SpiderURLIgnoreList As String
				string QueryString = null;
				int Result = 0;
				int PagesTotal = 0;
				DateTime LastCheckDate = default(DateTime);
				DateTime FirstCheckDate = default(DateTime);
				string Caption = null;
				string SpiderFontsAllowed = null;
				string SpiderPDFBodyText = null;
				string ProgressMessage = null;
				DateTime DateValue = default(DateTime);
				string AgeInDays = null;
				int ArchiveRecordAgeDays = 0;
				string ArchiveTimeOfDay = null;
				bool ArchiveAllowFileClean = false;
				adminUIController Adminui = new adminUIController(cpCore);
				string ButtonList = "";
				string Description = null;
				//
				//
				Button = cpCore.docProperties.getText(RequestNameButton);
				if (Button == ButtonCancel)
				{
					//
					//
					//
					return cpCore.webServer.redirect("/" + cpCore.serverConfig.appConfig.adminRoute, "HouseKeepingControl, Cancel Button Pressed");
				}
				else if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore))
				{
					//
					//
					//
					ButtonList = ButtonCancel;
					Content.Add(Adminui.GetFormBodyAdminOnly());
				}
				else
				{
					//
					Content.Add(Adminui.EditTableOpen);
					//
					// Set defaults
					//
					ArchiveRecordAgeDays = (cpCore.siteProperties.getinteger("ArchiveRecordAgeDays", 0));
					ArchiveTimeOfDay = cpCore.siteProperties.getText("ArchiveTimeOfDay", "12:00:00 AM");
					ArchiveAllowFileClean = (cpCore.siteProperties.getBoolean("ArchiveAllowFileClean", false));
					//ArchiveAllowLogClean = genericController.EncodeBoolean(cpCore.main_GetSiteProperty2("ArchiveAllowLogClean", False))

					//
					// Process Requests
					//
					switch (Convert.ToInt32(Button))
					{
						case ButtonOK:
						case ButtonSave:
							//
							ArchiveRecordAgeDays = cpCore.docProperties.getInteger("ArchiveRecordAgeDays");
							cpCore.siteProperties.setProperty("ArchiveRecordAgeDays", genericController.encodeText(ArchiveRecordAgeDays));
							//
							ArchiveTimeOfDay = cpCore.docProperties.getText("ArchiveTimeOfDay");
							cpCore.siteProperties.setProperty("ArchiveTimeOfDay", ArchiveTimeOfDay);
							//
							ArchiveAllowFileClean = cpCore.docProperties.getBoolean("ArchiveAllowFileClean");
							cpCore.siteProperties.setProperty("ArchiveAllowFileClean", genericController.encodeText(ArchiveAllowFileClean));
							break;
					}
					//
					if (Button == ButtonOK)
					{
						return cpCore.webServer.redirect("/" + cpCore.serverConfig.appConfig.adminRoute, "StaticPublishControl, OK Button Pressed");
					}
					//
					// ----- Status
					//
					Content.Add(genericController.StartTableRow() + "<td colspan=\"3\" class=\"ccPanel3D ccAdminEditSubHeader\"><b>Status</b>" + kmaEndTableCell + kmaEndTableRow);
					//
					// ----- Visits Found
					//
					PagesTotal = 0;
					SQL = "SELECT Count(ID) as Result FROM ccVisits;";
					CSServers = cpCore.db.csOpenSql_rev("Default", SQL);
					if (cpCore.db.csOk(CSServers))
					{
						PagesTotal = cpCore.db.csGetInteger(CSServers, "Result");
					}
					cpCore.db.csClose(CSServers);
					Content.Add(Adminui.GetEditRow(SpanClassAdminNormal + PagesTotal, "Visits Found", "", false, false, ""));
					//
					// ----- Oldest Visit
					//
					Copy = "unknown";
					AgeInDays = "unknown";
					SQL = cpCore.db.GetSQLSelect("default", "ccVisits", "DateAdded",, "ID",, 1);
					CSServers = cpCore.db.csOpenSql_rev("Default", SQL);
					//SQL = "SELECT Top 1 DateAdded FROM ccVisits order by ID;"
					//CSServers = cpCore.app_openCsSql_Rev_Internal("Default", SQL)
					if (cpCore.db.csOk(CSServers))
					{
						DateValue = cpCore.db.csGetDate(CSServers, "DateAdded");
						if (DateValue != DateTime.MinValue)
						{
							Copy = genericController.encodeText(DateValue);
							AgeInDays = genericController.encodeText(Convert.ToInt32(Math.Floor(Convert.ToDouble(cpCore.doc.profileStartTime - DateValue))));
						}
					}
					cpCore.db.csClose(CSServers);
					Content.Add(Adminui.GetEditRow(SpanClassAdminNormal + Copy + " (" + AgeInDays + " days)", "Oldest Visit", "", false, false, ""));
					//
					// ----- Viewings Found
					//
					PagesTotal = 0;
					SQL = "SELECT Count(ID) as result  FROM ccViewings;";
					CSServers = cpCore.db.csOpenSql_rev("Default", SQL);
					if (cpCore.db.csOk(CSServers))
					{
						PagesTotal = cpCore.db.csGetInteger(CSServers, "Result");
					}
					cpCore.db.csClose(CSServers);
					Content.Add(Adminui.GetEditRow(SpanClassAdminNormal + PagesTotal, "Viewings Found", "", false, false, ""));
					//
					Content.Add(genericController.StartTableRow() + "<td colspan=\"3\" class=\"ccPanel3D ccAdminEditSubHeader\"><b>Options</b>" + kmaEndTableCell + kmaEndTableRow);
					//
					Caption = "Archive Age";
					Copy = cpCore.html.html_GetFormInputText2("ArchiveRecordAgeDays", ArchiveRecordAgeDays.ToString(),, 20) + "&nbsp;Number of days to keep visit records. 0 disables housekeeping.";
					Content.Add(Adminui.GetEditRow(Copy, Caption));
					//
					Caption = "Housekeeping Time";
					Copy = cpCore.html.html_GetFormInputText2("ArchiveTimeOfDay", ArchiveTimeOfDay,, 20) + "&nbsp;The time of day when record deleting should start.";
					Content.Add(Adminui.GetEditRow(Copy, Caption));
					//
					Caption = "Purge Content Files";
					Copy = cpCore.html.html_GetFormInputCheckBox2("ArchiveAllowFileClean", ArchiveAllowFileClean) + "&nbsp;Delete Contensive content files with no associated database record.";
					Content.Add(Adminui.GetEditRow(Copy, Caption));
					//
					Content.Add(Adminui.EditTableClose);
					Content.Add(cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminformHousekeepingControl));
					ButtonList = ButtonCancel + ",Refresh," + ButtonSave + "," + ButtonOK;
				}
				//
				Caption = "Data Housekeeping Control";
				Description = "This tool is used to control the database record housekeeping process. This process deletes visit history records, so care should be taken before making any changes.";
				tempGetForm_HouseKeepingControl = Adminui.GetBody(Caption, ButtonList, "", false, false, Description, "", 0, Content.Text);
				//
				cpCore.html.addTitle(Caption);
				return tempGetForm_HouseKeepingControl;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			handleLegacyClassError3("GetForm_HouseKeepingControl");
			//
			return tempGetForm_HouseKeepingControl;
		}
		//
		//
		//
		private string GetPropertyHTMLControl(string Name, bool ProcessRequest, string DefaultValue, bool readOnlyField)
		{
				string tempGetPropertyHTMLControl = null;
			try
			{
				//
				string CurrentValue = null;
				//
				if (readOnlyField)
				{
					tempGetPropertyHTMLControl = "<div style=\"border:1px solid #808080; padding:20px;\">" + genericController.decodeHtml(cpCore.siteProperties.getText(Name, DefaultValue)) + "</div>";
				}
				else if (ProcessRequest)
				{
					CurrentValue = cpCore.docProperties.getText(Name);
					cpCore.siteProperties.setProperty(Name, CurrentValue);
					tempGetPropertyHTMLControl = cpCore.html.getFormInputHTML(Name, CurrentValue);
				}
				else
				{
					CurrentValue = cpCore.siteProperties.getText(Name, DefaultValue);
					tempGetPropertyHTMLControl = cpCore.html.getFormInputHTML(Name, CurrentValue);
				}
				return tempGetPropertyHTMLControl;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			handleLegacyClassError3("GetPropertyControl");
			return tempGetPropertyHTMLControl;
		}
		//
		//========================================================================
		//
		//========================================================================
		//
		private string admin_GetForm_StyleEditor()
		{
			return "<div><p>Site Styles are not longer supported. Instead add your styles to addons and add them with template dependencies.</p></div>";
		}
		//
		//
		//
		private string GetForm_ControlPage_CopyContent(string Caption, string CopyName)
		{
				string tempGetForm_ControlPage_CopyContent = null;
			try
			{
				//
				int CS = 0;
				int RecordID = 0;
				string EditIcon = null;
				string Copy = "";
				//
				const string ContentName = "Copy Content";
				//
				CS = cpCore.db.csOpen(ContentName, "Name=" + cpCore.db.encodeSQLText(CopyName));
				if (cpCore.db.csOk(CS))
				{
					RecordID = cpCore.db.csGetInteger(CS, "ID");
					Copy = cpCore.db.csGetText(CS, "copy");
				}
				cpCore.db.csClose(CS);
				//
				if (RecordID != 0)
				{
					EditIcon = "<a href=\"?cid=" + Models.Complex.cdefModel.getContentId(cpCore, ContentName) + "&id=" + RecordID + "&" + RequestNameAdminForm + "=4\" target=_blank><img src=\"/ccLib/images/IconContentEdit.gif\" border=0 alt=\"Edit content\" valign=absmiddle></a>";
				}
				else
				{
					EditIcon = "<a href=\"?cid=" + Models.Complex.cdefModel.getContentId(cpCore, ContentName) + "&" + RequestNameAdminForm + "=4&" + RequestNameAdminAction + "=2&ad=1&wc=" + genericController.EncodeURL("name=" + CopyName) + "\" target=_blank><img src=\"/ccLib/images/IconContentEdit.gif\" border=0 alt=\"Edit content\" valign=absmiddle></a>";
				}
				if (string.IsNullOrEmpty(Copy))
				{
					Copy = "&nbsp;";
				}
				//
				tempGetForm_ControlPage_CopyContent = ""
					+ genericController.StartTable(4, 0, 1) + "<tr>"
					+ "<td width=150 align=right>" + Caption + "<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=150 height=1></td>"
					+ "<td width=20 align=center>" + EditIcon + "</td>"
					+ "<td>" + Copy + "&nbsp;</td>"
					+ "</tr></table>";
				//
				return tempGetForm_ControlPage_CopyContent;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			handleLegacyClassError3("GetForm_ControlPage_CopyContent");
			//
			return tempGetForm_ControlPage_CopyContent;
		}
		//
		//========================================================================
		//
		//========================================================================
		//
		private string GetForm_Downloads()
		{
				string tempGetForm_Downloads = null;
			try
			{
				//
				bool IsEmptyList = false;
				string ResultMessage = null;
				string LinkPrefix = null;
				string LinkSuffix = null;
				string RemoteKey = null;
				string Copy = null;
				string Button = null;
				string ButtonPanel = null;
				bool SaveAction = false;
				string helpCopy = null;
				string FieldValue = null;
				int PaymentProcessMethod = 0;
				string Argument1 = null;
				int CS = 0;
				string ContactGroupCriteria = null;
				int GroupCount = 0;
				int GroupPointer = 0;
				bool GroupChecked = false;
				string RecordName = null;
				string ContentName = null;
				int RecordID = 0;
				bool RowEven = false;
				string SQL = null;
				string RQS = null;
				int SubTab = 0;
				bool FormSave = false;
				bool FormClear = false;
				int ContactContentID = 0;
				string Criteria = null;
				string ContentGorupCriteria = null;
				string ContactSearchCriteria = null;
				string[] FieldParms = null;
				object CriteriaValues = null;
				int CriteriaCount = 0;
				int CriteriaPointer = 0;
				int PageSize = 0;
				int PageNumber = 0;
				int TopCount = 0;
				int RowPointer = 0;
				int DataRowCount = 0;
				string PreTableCopy = "";
				string PostTableCopy = "";
				int ColumnPtr = 0;
				string[] ColCaption = null;
				string[] ColAlign = null;
				string[] ColWidth = null;
				string[,] Cells = null;
				int GroupID = 0;
				int GroupToolAction = 0;
				string ActionPanel = null;
				int RowCount = 0;
				string GroupName = null;
				int MemberID = 0;
				string QS = null;
				string VisitsCell = null;
				int VisitCount = 0;
				string AdminURL = null;
				int CCID = 0;
				string SQLValue = null;
				string DefaultName = null;
				string SearchCaption = null;
				string BlankPanel = null;
				string RowPageSize = null;
				string RowGroups = null;
				string[] GroupIDs = null;
				int GroupPtr = 0;
				string GroupDelimiter = null;
				DateTime DateCompleted = default(DateTime);
				int RowCnt = 0;
				int RowPtr = 0;
				int ContentID = 0;
				string Format = null;
				string TableName = null;
				string Filename = null;
				string Name = null;
				string Caption = null;
				string Description = "";
				string ButtonListLeft = null;
				string ButtonListRight = null;
				int ContentPadding = 0;
				string ContentSummary = "";
				stringBuilderLegacyController Tab0 = new stringBuilderLegacyController();
				stringBuilderLegacyController Tab1 = new stringBuilderLegacyController();
				string Content = "";
				string Cell = null;
				adminUIController Adminui = new adminUIController(cpCore);
				string SQLFieldName = null;
				//
				const int ColumnCnt = 5;
				//
				Button = cpCore.docProperties.getText(RequestNameButton);
				if (Button == ButtonCancel)
				{
					return cpCore.webServer.redirect("/" + cpCore.serverConfig.appConfig.adminRoute, "Downloads, Cancel Button Pressed");
				}
				//
				if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore))
				{
					//
					// Must be a developer
					//
					ButtonListLeft = ButtonCancel;
					ButtonListRight = "";
					Content = Content + Adminui.GetFormBodyAdminOnly();
				}
				else
				{
					ContentID = cpCore.docProperties.getInteger("ContentID");
					Format = cpCore.docProperties.getText("Format");
					if (false)
					{
						SQLFieldName = "SQL";
					}
					else
					{
						SQLFieldName = "SQLQuery";
					}
					//
					// Process Requests
					//
					if (!string.IsNullOrEmpty(Button))
					{
						switch (Convert.ToInt32(Button))
						{
							case ButtonDelete:
								RowCnt = cpCore.docProperties.getInteger("RowCnt");
								if (RowCnt > 0)
								{
									for (RowPtr = 0; RowPtr < RowCnt; RowPtr++)
									{
										if (cpCore.docProperties.getBoolean("Row" + RowPtr))
										{
											cpCore.db.deleteContentRecord("Tasks", cpCore.docProperties.getInteger("RowID" + RowPtr));
										}
									}
								}
								break;
							case ButtonRequestDownload:
								//
								// Request the download again
								//
								RowCnt = cpCore.docProperties.getInteger("RowCnt");
								if (RowCnt > 0)
								{
									for (RowPtr = 0; RowPtr < RowCnt; RowPtr++)
									{
										if (cpCore.docProperties.getBoolean("Row" + RowPtr))
										{
											int CSSrc = 0;
											int CSDst = 0;

											CSSrc = cpCore.db.csOpenRecord("Tasks", cpCore.docProperties.getInteger("RowID" + RowPtr));
											if (cpCore.db.csOk(CSSrc))
											{
												CSDst = cpCore.db.csInsertRecord("Tasks");
												if (cpCore.db.csOk(CSDst))
												{
													cpCore.db.csSet(CSDst, "Name", cpCore.db.csGetText(CSSrc, "name"));
													cpCore.db.csSet(CSDst, SQLFieldName, cpCore.db.csGetText(CSSrc, SQLFieldName));
													if (genericController.vbLCase(cpCore.db.csGetText(CSSrc, "command")) == "xml")
													{
														cpCore.db.csSet(CSDst, "Filename", "DupDownload_" + Convert.ToString(genericController.dateToSeconds(cpCore.doc.profileStartTime)) + Convert.ToString(genericController.GetRandomInteger()) + ".xml");
														cpCore.db.csSet(CSDst, "Command", "BUILDXML");
													}
													else
													{
														cpCore.db.csSet(CSDst, "Filename", "DupDownload_" + Convert.ToString(genericController.dateToSeconds(cpCore.doc.profileStartTime)) + Convert.ToString(genericController.GetRandomInteger()) + ".csv");
														cpCore.db.csSet(CSDst, "Command", "BUILDCSV");
													}
												}
												cpCore.db.csClose(CSDst);
											}
											cpCore.db.csClose(CSSrc);
										}
									}
								}
								//
								//
								//
								if ((!string.IsNullOrEmpty(Format)) && (ContentID == 0))
								{
									Description = Description + "<p>Please select a Content before requesting a download</p>";
								}
								else if ((string.IsNullOrEmpty(Format)) && (ContentID != 0))
								{
									Description = Description + "<p>Please select a Format before requesting a download</p>";
								}
								else if (Format == "CSV")
								{
									CS = cpCore.db.csInsertRecord("Tasks");
									if (cpCore.db.csOk(CS))
									{
										ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID);
										TableName = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName);
										Criteria = Models.Complex.cdefModel.getContentControlCriteria(cpCore, ContentName);
										Name = "CSV Download, " + ContentName;
										Filename = genericController.vbReplace(ContentName, " ", "") + "_" + Convert.ToString(genericController.dateToSeconds(cpCore.doc.profileStartTime)) + Convert.ToString(genericController.GetRandomInteger()) + ".csv";
										cpCore.db.csSet(CS, "Name", Name);
										cpCore.db.csSet(CS, "Filename", Filename);
										cpCore.db.csSet(CS, "Command", "BUILDCSV");
										cpCore.db.csSet(CS, SQLFieldName, "SELECT * from " + TableName + " where " + Criteria);
										Description = Description + "<p>Your CSV Download has been requested.</p>";
									}
									cpCore.db.csClose(CS);
									Format = "";
									ContentID = 0;
								}
								else if (Format == "XML")
								{
									CS = cpCore.db.csInsertRecord("Tasks");
									if (cpCore.db.csOk(CS))
									{
										ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID);
										TableName = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName);
										Criteria = Models.Complex.cdefModel.getContentControlCriteria(cpCore, ContentName);
										Name = "XML Download, " + ContentName;
										Filename = genericController.vbReplace(ContentName, " ", "") + "_" + Convert.ToString(genericController.dateToSeconds(cpCore.doc.profileStartTime)) + Convert.ToString(genericController.GetRandomInteger()) + ".xml";
										cpCore.db.csSet(CS, "Name", Name);
										cpCore.db.csSet(CS, "Filename", Filename);
										cpCore.db.csSet(CS, "Command", "BUILDXML");
										cpCore.db.csSet(CS, SQLFieldName, "SELECT * from " + TableName + " where " + Criteria);
										Description = Description + "<p>Your XML Download has been requested.</p>";
									}
									cpCore.db.csClose(CS);
									Format = "";
									ContentID = 0;
								}
								break;
						}
					}
					//
					// Build Tab0
					//
					//Tab0.Add( "<p>The following is a list of available downloads</p>")
					//'
					RQS = cpCore.doc.refreshQueryString;
					PageSize = cpCore.docProperties.getInteger(RequestNamePageSize);
					if (PageSize == 0)
					{
						PageSize = 50;
					}
					PageNumber = cpCore.docProperties.getInteger(RequestNamePageNumber);
					if (PageNumber == 0)
					{
						PageNumber = 1;
					}
					AdminURL = "/" + cpCore.serverConfig.appConfig.adminRoute;
					TopCount = PageNumber * PageSize;
					//
					// Setup Headings
					//
					ColCaption = new string[ColumnCnt + 1];
					ColAlign = new string[ColumnCnt + 1];
					ColWidth = new string[ColumnCnt + 1];
					Cells = new string[PageSize + 1, ColumnCnt + 1];
					//
					ColCaption[ColumnPtr] = "Select<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=10 height=1>";
					ColAlign[ColumnPtr] = "center";
					ColWidth[ColumnPtr] = "10";
					ColumnPtr = ColumnPtr + 1;
					//
					ColCaption[ColumnPtr] = "Name";
					ColAlign[ColumnPtr] = "left";
					ColWidth[ColumnPtr] = "100%";
					ColumnPtr = ColumnPtr + 1;
					//
					ColCaption[ColumnPtr] = "For<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=100 height=1>";
					ColAlign[ColumnPtr] = "left";
					ColWidth[ColumnPtr] = "100";
					ColumnPtr = ColumnPtr + 1;
					//
					ColCaption[ColumnPtr] = "Requested<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=150 height=1>";
					ColAlign[ColumnPtr] = "left";
					ColWidth[ColumnPtr] = "150";
					ColumnPtr = ColumnPtr + 1;
					//
					ColCaption[ColumnPtr] = "File<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=100 height=1>";
					ColAlign[ColumnPtr] = "Left";
					ColWidth[ColumnPtr] = "100";
					ColumnPtr = ColumnPtr + 1;
					//
					//   Get Data
					//
					SQL = "select M.Name as CreatedByName, T.* from ccTasks T left join ccMembers M on M.ID=T.CreatedBy where (T.Command='BuildCSV')or(T.Command='BuildXML') order by T.DateAdded Desc";
					//Call cpCore.main_TestPoint("Selection SQL=" & SQL)
					CS = cpCore.db.csOpenSql_rev("default", SQL, PageSize, PageNumber);
					RowPointer = 0;
					if (!cpCore.db.csOk(CS))
					{
						Cells[0, 1] = "There are no download requests";
						RowPointer = 1;
					}
					else
					{
						DataRowCount = cpCore.db.csGetRowCount(CS);
						LinkPrefix = "<a href=\"" + cpCore.serverConfig.appConfig.cdnFilesNetprefix;
						LinkSuffix = "\" target=_blank>Available</a>";
						while (cpCore.db.csOk(CS) && (RowPointer < PageSize))
						{
							RecordID = cpCore.db.csGetInteger(CS, "ID");
							DateCompleted = cpCore.db.csGetDate(CS, "DateCompleted");
							ResultMessage = cpCore.db.csGetText(CS, "ResultMessage");
							Cells[RowPointer, 0] = cpCore.html.html_GetFormInputCheckBox2("Row" + RowPointer) + cpCore.html.html_GetFormInputHidden("RowID" + RowPointer, RecordID);
							Cells[RowPointer, 1] = cpCore.db.csGetText(CS, "name");
							Cells[RowPointer, 2] = cpCore.db.csGetText(CS, "CreatedByName");
							Cells[RowPointer, 3] = cpCore.db.csGetDate(CS, "DateAdded").ToShortDateString;
							if (DateCompleted == DateTime.MinValue)
							{
								RemoteKey = remoteQueryController.main_GetRemoteQueryKey(cpCore, "select DateCompleted,filename,resultMessage from cctasks where id=" + RecordID, "default", 1);
								Cell = "";
								Cell = Cell + Environment.NewLine + "<div id=\"pending" + RowPointer + "\">Pending <img src=\"/ccLib/images/ajax-loader-small.gif\" width=16 height=16></div>";
								//
								Cell = Cell + Environment.NewLine + "<script>";
								Cell = Cell + Environment.NewLine + "function statusHandler" + RowPointer + "(results) {";
								Cell = Cell + Environment.NewLine + " var jo,isDone=false;";
								Cell = Cell + Environment.NewLine + " eval('jo='+results);";
								Cell = Cell + Environment.NewLine + " if (jo){";
								Cell = Cell + Environment.NewLine + "  if(jo.DateCompleted) {";
								Cell = Cell + Environment.NewLine + "    var dst=document.getElementById('pending" + RowPointer + "');";
								Cell = Cell + Environment.NewLine + "    isDone=true;";
								Cell = Cell + Environment.NewLine + "    if(jo.resultMessage=='OK') {";
								Cell = Cell + Environment.NewLine + "      dst.innerHTML='" + LinkPrefix + "'+jo.filename+'" + LinkSuffix + "';";
								Cell = Cell + Environment.NewLine + "    }else{";
								Cell = Cell + Environment.NewLine + "      dst.innerHTML='error';";
								Cell = Cell + Environment.NewLine + "    }";
								Cell = Cell + Environment.NewLine + "  }";
								Cell = Cell + Environment.NewLine + " }";
								Cell = Cell + Environment.NewLine + " if(!isDone) setTimeout(\"requestStatus" + RowPointer + "()\",5000)";
								Cell = Cell + Environment.NewLine + "}";
								//
								Cell = Cell + Environment.NewLine + "function requestStatus" + RowPointer + "() {";
								Cell = Cell + Environment.NewLine + "  cj.ajax.getNameValue(statusHandler" + RowPointer + ",'" + RemoteKey + "');";
								Cell = Cell + Environment.NewLine + "}";
								Cell = Cell + Environment.NewLine + "requestStatus" + RowPointer + "();";
								Cell = Cell + Environment.NewLine + "</script>";
								//
								Cells[RowPointer, 4] = Cell;
							}
							else if (ResultMessage == "ok")
							{
								Cells[RowPointer, 4] = "<div id=\"pending" + RowPointer + "\">" + LinkPrefix + cpCore.db.csGetText(CS, "filename") + LinkSuffix + "</div>";
							}
							else
							{
								Cells[RowPointer, 4] = "<div id=\"pending" + RowPointer + "\"><a href=\"javascript:alert('" + genericController.EncodeJavascript(ResultMessage) + ";return false');\">error</a></div>";
							}
							RowPointer = RowPointer + 1;
							cpCore.db.csGoNext(CS);
						}
					}
					cpCore.db.csClose(CS);
					Tab0.Add(cpCore.html.html_GetFormInputHidden("RowCnt", RowPointer));
					Cell = Adminui.GetReport(RowPointer, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, "ccPanel");
					Tab0.Add(Cell);
					//Tab0.Add( "<div style=""height:200px;"">" & Cell & "</div>"
					//        '
					//        ' Build RequestContent Form
					//        '
					//        Tab1.Add( "<p>Use this form to request a download. Select the criteria for the download and click the [Request Download] button. The request should then appear on the requested download list in the other tab. When the download has been created, it will be become available.</p>")
					//        '
					//        Tab1.Add( "<table border=""0"" cellpadding=""3"" cellspacing=""0"" width=""100%"">")
					//        '
					//        Call Tab1.Add("<tr>")
					//        Call Tab1.Add("<td align=right>Content</td>")
					//        Call Tab1.Add("<td>" & cpCore.htmldoc.main_GetFormInputSelect2("ContentID", ContentID, "Content", "", "", "", IsEmptyList) & "</td>")
					//        Call Tab1.Add("</tr>")
					//        '
					//        Call Tab1.Add("<tr>")
					//        Call Tab1.Add("<td align=right>Format</td>")
					//        Call Tab1.Add("<td><select name=Format value=""" & Format & """><option value=CSV>CSV</option><option name=XML value=XML>XML</option></select></td>")
					//        Call Tab1.Add("</tr>")
					//        '
					//        Call Tab1.Add("" _
					//            & "<tr>" _
					//            & "<td width=""120""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""120"" height=""1""></td>" _
					//            & "<td width=""100%"">&nbsp;</td>" _
					//            & "</tr>" _
					//            & "</table>")
					//        '
					//        ' Build and add tabs
					//        '
					//        Call cpCore.htmldoc.main_AddLiveTabEntry("Current&nbsp;Downloads", Tab0.Text, "ccAdminTab")
					//        Call cpCore.htmldoc.main_AddLiveTabEntry("Request&nbsp;New&nbsp;Download", Tab1.Text, "ccAdminTab")
					//        Content = cpCore.htmldoc.main_GetLiveTabs()
					Content = Tab0.Text;
					//
					ButtonListLeft = ButtonCancel + "," + ButtonRefresh + "," + ButtonDelete;
					//ButtonListLeft = ButtonCancel & "," & ButtonRefresh & "," & ButtonDelete & "," & ButtonRequestDownload
					ButtonListRight = "";
					Content = Content + cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormDownloads);
				}
				//
				Caption = "Download Manager";
				Description = ""
					+ "<p>The Download Manager holds all downloads requested from anywhere on the website. It also provides tools to request downloads from any Content.</p>"
					+ "<p>To add a new download of any content in Contensive, click Export on the filter tab of the content listing page. To add a new download from a SQL statement, use Custom Reports under Reports on the Navigator.</p>";
				ContentPadding = 0;
				tempGetForm_Downloads = Adminui.GetBody(Caption, ButtonListLeft, ButtonListRight, true, true, Description, ContentSummary, ContentPadding, Content);
				//
				cpCore.html.addTitle(Caption);
				return tempGetForm_Downloads;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			handleLegacyClassError3("GetForm_Downloads");
			return tempGetForm_Downloads;
		}
		//
		//========================================================================
		//   Display field in the admin/edit
		//========================================================================
		//
		private string GetForm_Edit_AddTab(string Caption, string Content, bool AllowAdminTabs)
		{
				string tempGetForm_Edit_AddTab = null;
			try
			{
				//
				if (!string.IsNullOrEmpty(Content))
				{
					if (!AllowAdminTabs)
					{
						tempGetForm_Edit_AddTab = Content;
					}
					else
					{
						cpCore.html.menu_AddComboTabEntry(Caption.Replace(" ", "&nbsp;"), "", "", Content, false, "ccAdminTab");
						//Call cpCore.htmldoc.main_AddLiveTabEntry(Replace(Caption, " ", "&nbsp;"), Content, "ccAdminTab")
					}
				}
				//
				return tempGetForm_Edit_AddTab;
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			handleLegacyClassError3("GetForm_Edit_AddTab");
			return tempGetForm_Edit_AddTab;
		}
		//
		//========================================================================
		//   Creates Tabbed content that is either Live (all content on page) or Ajax (click and ajax in the content)
		//========================================================================
		//
		private string GetForm_Edit_AddTab2(string Caption, string Content, bool AllowAdminTabs, string AjaxLink)
		{
				string tempGetForm_Edit_AddTab2 = null;
			try
			{
				//
				if (!AllowAdminTabs)
				{
					//
					// non-tab mode
					//
					tempGetForm_Edit_AddTab2 = Content;
				}
				else if (!string.IsNullOrEmpty(AjaxLink))
				{
					//
					// Ajax Tab
					//
					cpCore.html.menu_AddComboTabEntry(Caption.Replace(" ", "&nbsp;"), "", AjaxLink, "", false, "ccAdminTab");
				}
				else
				{
					//
					// Live Tab
					//
					cpCore.html.menu_AddComboTabEntry(Caption.Replace(" ", "&nbsp;"), "", "", Content, false, "ccAdminTab");
				}
				//
				return tempGetForm_Edit_AddTab2;
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			handleLegacyClassError3("GetForm_Edit_AddTab2");
			return tempGetForm_Edit_AddTab2;
		}
		//
		//=============================================================================
		// Create a child content
		//=============================================================================
		//
		private string GetForm_PageContentMap()
		{
			try
			{
				//
				return "<p>The Page Content Map has been replaced with the Site Explorer, available as an Add-on through the Add-on Manager.</p>";
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			handleLegacyClassError3("GetForm_PageContentMap");
		}
		//
		//
		//
		private string GetForm_Edit_Tabs(Models.Complex.cdefModel adminContent, editRecordClass editRecord, bool readOnlyField, bool IsLandingPage, bool IsRootPage, csv_contentTypeEnum EditorContext, bool allowAjaxTabs, int TemplateIDForStyles, string[] fieldTypeDefaultEditors, string fieldEditorPreferenceList, string styleList, string styleOptionList, int emailIdForStyles, bool IsTemplateTable, string editorAddonListJSON)
		{
			string returnHtml = "";
			try
			{
				//
				string tabContent = null;
				string AjaxLink = null;
				List<string> TabsFound = new List<string>();
				string editTabCaption = null;
				string NewFormFieldList = null;
				string FormFieldList = null;
				bool AllowHelpMsgCustom = false;
				string IDList = null;
				DataTable dt = null;
				string[,] TempVar = null;
				int HelpCnt = 0;
				int fieldId = 0;
				int LastFieldID = 0;
				int HelpPtr = 0;
				int[] HelpIDCache = {};
				string[] helpDefaultCache = {};
				string[] HelpCustomCache = {};
				keyPtrController helpIdIndex = new keyPtrController();
				string fieldNameLc = null;
				//
				// ----- read in help
				//
				IDList = "";
				foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in adminContent.fields)
				{
					Models.Complex.CDefFieldModel field = keyValuePair.Value;
					IDList = IDList + "," + field.id;
				}
				if (!string.IsNullOrEmpty(IDList))
				{
					IDList = IDList.Substring(1);
				}
				//
				dt = cpCore.db.executeQuery("select fieldid,helpdefault,helpcustom from ccfieldhelp where fieldid in (" + IDList + ") order by fieldid,id");
				TempVar = cpCore.db.convertDataTabletoArray(dt);
				if (TempVar.GetLength(0) > 0)
				{
					HelpCnt = TempVar.GetUpperBound(1) + 1;
					HelpIDCache = new int[HelpCnt + 1];
					helpDefaultCache = new string[HelpCnt + 1];
					HelpCustomCache = new string[HelpCnt + 1];
					fieldId = -1;
					for (HelpPtr = 0; HelpPtr < HelpCnt; HelpPtr++)
					{
						fieldId = genericController.EncodeInteger(TempVar[0, HelpPtr]);
						if (fieldId != LastFieldID)
						{
							LastFieldID = fieldId;
							HelpIDCache[HelpPtr] = fieldId;
							helpIdIndex.setPtr(fieldId.ToString(), HelpPtr);
							helpDefaultCache[HelpPtr] = genericController.encodeText(TempVar[1, HelpPtr]);
							HelpCustomCache[HelpPtr] = genericController.encodeText(TempVar[2, HelpPtr]);
						}
					}
					AllowHelpMsgCustom = true;
				}
				//
				FormFieldList = ",";
				foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in adminContent.fields)
				{
					Models.Complex.CDefFieldModel field = keyValuePair.Value;
					if ((field.authorable) & (field.active) && (!TabsFound.Contains(field.editTabName.ToLower())))
					{
						TabsFound.Add(field.editTabName.ToLower());
						fieldNameLc = field.nameLc;
						editTabCaption = field.editTabName;
						if (string.IsNullOrEmpty(editTabCaption))
						{
							editTabCaption = "Details";
						}
						NewFormFieldList = "";
						if ((!allowAdminTabs) | (!allowAjaxTabs) || (editTabCaption.ToLower() == "details"))
						{
							//
							// Live Tab (non-tab mode, non-ajax mode, or details tab
							//
							tabContent = GetForm_Edit_Tab(adminContent, editRecord, editRecord.id, adminContent.Id, readOnlyField, IsLandingPage, IsRootPage, field.editTabName, EditorContext, NewFormFieldList, TemplateIDForStyles, HelpCnt, HelpIDCache, helpDefaultCache, HelpCustomCache, AllowHelpMsgCustom, helpIdIndex, fieldTypeDefaultEditors, fieldEditorPreferenceList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON);
							if (!string.IsNullOrEmpty(tabContent))
							{
								returnHtml += GetForm_Edit_AddTab2(editTabCaption, tabContent, allowAdminTabs, "");
							}
						}
						else
						{
							//
							// Ajax Tab
							//
							//AjaxLink = "/admin/index.asp?"
							AjaxLink = "/" + cpCore.serverConfig.appConfig.adminRoute + "?"
							+ RequestNameAjaxFunction + "=" + AjaxGetFormEditTabContent + "&ID=" + editRecord.id + "&CID=" + adminContent.Id + "&ReadOnly=" + readOnlyField + "&IsLandingPage=" + IsLandingPage + "&IsRootPage=" + IsRootPage + "&EditTab=" + genericController.EncodeRequestVariable(field.editTabName) + "&EditorContext=" + EditorContext + "&NewFormFieldList=" + genericController.EncodeRequestVariable(NewFormFieldList);
							returnHtml += GetForm_Edit_AddTab2(editTabCaption, "", true, AjaxLink);
						}
						if (!string.IsNullOrEmpty(NewFormFieldList))
						{
							FormFieldList = NewFormFieldList + FormFieldList;
						}
					}
				}
				//
				// ----- add the FormFieldList hidden - used on read to make sure all fields are returned
				//       this may not be needed, but we are having a problem with forms coming back without values
				//
				//
				// moved this to GetEditTabContent - so one is added for each tab.
				//
				returnHtml += cpCore.html.html_GetFormInputHidden("FormFieldList", FormFieldList);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return returnHtml;
		}
		//        '
		//        ' Delete this when I can verify the Csvr patch to the instream process works
		//        '
		//        Private Sub VerifyDynamicMenuStyleSheet(ByVal MenuID As Integer)
		//            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogAdminMethodEnter("VerifyDynamicMenuStyleSheet")
		//            '
		//            Dim StyleSN As String
		//            Dim EditTabCaption As String
		//            Dim ACTags() As String
		//            Dim TagPtr As Integer
		//            Dim QSPos As Integer
		//            Dim QSPosEnd As Integer
		//            Dim QS As String
		//            Dim MenuName As String
		//            Dim StylePrefix As String
		//            Dim CS As Integer
		//            Dim IsFound As Boolean
		//            Dim StyleSheet As String
		//            Dim DefaultStyles As String
		//            Dim DynamicStyles As String
		//            Dim AddStyles As String
		//            Dim StyleSplit() As String
		//            Dim StylePtr As Integer
		//            Dim StyleLine As String
		//            Dim Filename As String
		//            Dim NewStyleLine As String
		//            Dim TestSTyles As String

		//            '
		//            CS = cpCore.main_OpenCSContentRecord("Dynamic Menus", MenuID)
		//            If cpCore.app.IsCSOK(CS) Then
		//                StylePrefix = cpCore.db.cs_getText(CS, "StylePrefix")
		//                If StylePrefix <> "" And genericController.vbUCase(StylePrefix) <> "CCFLYOUT" Then
		//                    if true then ' 3.3.951" Then
		//                        TestSTyles = cpCore.app.cs_get(CS, "StylesFilename")
		//                    Else
		//                        TestSTyles = cpCore.main_GetStyleSheet
		//                    End If
		//                    If genericController.vbInstr(1, TestSTyles, "." & StylePrefix, vbTextCompare) = 0 Then
		//                        '
		//                        ' style not found, get the default ccFlyout styles
		//                        '
		//                        DefaultStyles = RemoveStyleTags(cpCore.cluster.programDataFiles.ReadFile("ccLib\" & "Styles\" & defaultStyleFilename))
		//                        'DefaultStyles = genericController.vbReplace(DefaultStyles, vbCrLf, " ")
		//                        Do While genericController.vbInstr(1, DefaultStyles, "  ") <> 0
		//                            DefaultStyles = genericController.vbReplace(DefaultStyles, "  ", " ")
		//                        Loop
		//                        StyleSplit = Split(DefaultStyles, "}")
		//                        For StylePtr = 0 To UBound(StyleSplit)
		//                            StyleLine = StyleSplit(StylePtr)
		//                            If StyleLine <> "" Then
		//                                If genericController.vbInstr(1, StyleLine, ".ccflyout", vbTextCompare) <> 0 Then
		//                                    StyleLine = genericController.vbReplace(StyleLine, vbCrLf, " ")
		//                                    StyleLine = genericController.vbReplace(StyleLine, ".ccflyout", "." & StylePrefix, vbTextCompare)
		//                                    Do While Left(StyleLine, 1) = " "
		//                                        StyleLine = Mid(StyleLine, 2)
		//                                    Loop
		//                                    AddStyles = AddStyles & StyleLine & "}" & vbCrLf
		//                                End If
		//                            End If
		//                        Next
		//                        If AddStyles <> "" Then
		//                            '
		//                            '
		//                            '
		//                            if true then ' 3.3.951" Then
		//                                '
		//                                ' Add new styles to the StylesFilename field
		//                                '
		//                                DynamicStyles = "" _
		//                                    & cpCore.app.cs_get(CS, "StylesFilename") _
		//                                    & vbCrLf & "" _
		//                                    & vbCrLf & "/* Menu Styles for Style Prefix [" & StylePrefix & "] created " & nt(cpCore.main_PageStartTime.toshortdateString & " */" _
		//                                    & vbCrLf & "" _
		//                                    & vbCrLf & AddStyles _
		//                                    & ""
		//                                Call cpCore.app.SetCS(CS, "StylesFilename", DynamicStyles)
		//                            Else
		//                                '
		//                                ' Legacy - add styles to the site stylesheet
		//                                '
		//                                Filename = cpCore.app.confxxxig.physicalFilePath & DynamicStylesFilename
		//                                DynamicStyles = RemoveStyleTags(cpCore.app.publicFiles.ReadFile(Filename)) & vbCrLf & AddStyles
		//                                Call cpCore.app.publicFiles.SaveFile(Filename, DynamicStyles)
		//                                '
		//                                ' Now create admin and public stylesheets from the styles.css styles
		//                                '
		//                                StyleSN = (cpCore.app.siteProperty_getInteger("StylesheetSerialNumber", "0"))
		//                                If StyleSN <> 0 Then
		//                                    ' mark to rebuild next fetch
		//                                    Call cpCore.app.siteProperty_set("StylesheetSerialNumber", "-1")
		//                                    '' Linked Styles
		//                                    '' Bump the Style Serial Number so next fetch is not cached
		//                                    ''
		//                                    'StyleSN = StyleSN + 1
		//                                    'Call cpCore.app.setSiteProperty("StylesheetSerialNumber", StyleSN)
		//                                    ''
		//                                    '' Save new public stylesheet
		//                                    ''
		//                                    '' 11/24/2009 - style sheet processing deprecated
		//                                    'Call cpCore.app.publicFiles.SaveFile("templates\Public" & StyleSN & ".css", cpCore.main_GetStyleSheet)
		//                                    ''Call cpCore.app.publicFiles.SaveFile("templates\Public" & StyleSN & ".css", cpCore.main_GetStyleSheetProcessed)
		//                                    'Call cpCore.app.publicFiles.SaveFile("templates\Admin" & StyleSN & ".css", cpCore.main_GetStyleSheetDefault)
		//                                End If
		//                            End If
		//                        End If
		//                    End If
		//                End If
		//            End If
		//            Call cpCore.app.closeCS(CS)
		//            '
		//            Exit Sub
		//            '
		//            ' ----- Error Trap
		//            '
		//ErrorTrap:
		//            Call handleLegacyClassError3("GetForm_Edit_UserFieldTabs")
		//        End Sub
		//
		//========================================================================
		//
		//========================================================================
		//
		private string GetForm_CustomReports()
		{
				string tempGetForm_CustomReports = null;
			try
			{
				//
				string Copy = null;
				string Button = null;
				string ButtonPanel = null;
				bool SaveAction = false;
				string helpCopy = null;
				string FieldValue = null;
				int PaymentProcessMethod = 0;
				string Argument1 = null;
				int CS = 0;
				string ContactGroupCriteria = null;
				int GroupCount = 0;
				int GroupPointer = 0;
				bool GroupChecked = false;
				string RecordName = null;
				string ContentName = null;
				int RecordID = 0;
				bool RowEven = false;
				string SQL = null;
				string RQS = null;
				int SubTab = 0;
				bool FormSave = false;
				bool FormClear = false;
				int ContactContentID = 0;
				string Criteria = null;
				string ContentGorupCriteria = null;
				string ContactSearchCriteria = null;
				string[] FieldParms = null;
				object CriteriaValues = null;
				int CriteriaCount = 0;
				int CriteriaPointer = 0;
				int PageSize = 0;
				int PageNumber = 0;
				int TopCount = 0;
				int RowPointer = 0;
				int DataRowCount = 0;
				string PreTableCopy = "";
				string PostTableCopy = "";
				int ColumnPtr = 0;
				string[] ColCaption = null;
				string[] ColAlign = null;
				string[] ColWidth = null;
				string[,] Cells = null;
				int GroupID = 0;
				int GroupToolAction = 0;
				string ActionPanel = null;
				int RowCount = 0;
				string GroupName = null;
				int MemberID = 0;
				string QS = null;
				string VisitsCell = null;
				int VisitCount = 0;
				string AdminURL = null;
				int CCID = 0;
				string SQLValue = null;
				string DefaultName = null;
				string SearchCaption = null;
				string BlankPanel = null;
				string RowPageSize = null;
				string RowGroups = null;
				string[] GroupIDs = null;
				int GroupPtr = 0;
				string GroupDelimiter = null;
				DateTime DateCompleted = default(DateTime);
				int RowCnt = 0;
				int RowPtr = 0;
				int ContentID = 0;
				string Format = null;
				string TableName = null;
				string Filename = null;
				string Name = null;
				string Caption = null;
				string Description = null;
				string ButtonListLeft = null;
				string ButtonListRight = null;
				int ContentPadding = 0;
				string ContentSummary = "";
				stringBuilderLegacyController Tab0 = new stringBuilderLegacyController();
				stringBuilderLegacyController Tab1 = new stringBuilderLegacyController();
				string Content = "";
				string SQLFieldName = null;
				//
				const int ColumnCnt = 4;
				//
				Button = cpCore.docProperties.getText(RequestNameButton);
				ContentID = cpCore.docProperties.getInteger("ContentID");
				Format = cpCore.docProperties.getText("Format");
				//
				Caption = "Custom Report Manager";
				Description = "Custom Reports are a way for you to create a snapshot of data to view or download. To request a report, select the Custom Reports tab, check the report(s) you want, and click the [Request Download] Button. When your report is ready, it will be available in the <a href=\"?" + RequestNameAdminForm + "=30\">Download Manager</a>. To create a new custom report, select the Request New Report tab, enter a name and SQL statement, and click the Apply button.";
				ContentPadding = 0;
				ButtonListLeft = ButtonCancel + "," + ButtonDelete + "," + ButtonRequestDownload;
				//ButtonListLeft = ButtonCancel & "," & ButtonDelete & "," & ButtonRequestDownload & "," & ButtonApply
				ButtonListRight = "";
				if (false)
				{
					SQLFieldName = "SQL";
				}
				else
				{
					SQLFieldName = "SQLQuery";
				}
				//
				if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore))
				{
					//
					// Must be a developer
					//
					Description = Description + "You can not access the Custom Report Manager because your account is not configured as an administrator.";
				}
				else
				{
					//
					// Process Requests
					//
					if (!string.IsNullOrEmpty(Button))
					{
						switch (Convert.ToInt32(Button))
						{
							case ButtonCancel:
								return cpCore.webServer.redirect("/" + cpCore.serverConfig.appConfig.adminRoute, "CustomReports, Cancel Button Pressed");
								//Call cpCore.main_Redirect2(encodeAppRootPath(cpCore.main_GetSiteProperty2("AdminURL"), cpCore.main_ServerVirtualPath, cpCore.app.RootPath, cpCore.main_ServerHost))
							case ButtonDelete:
								RowCnt = cpCore.docProperties.getInteger("RowCnt");
								if (RowCnt > 0)
								{
									for (RowPtr = 0; RowPtr < RowCnt; RowPtr++)
									{
										if (cpCore.docProperties.getBoolean("Row" + RowPtr))
										{
											cpCore.db.deleteContentRecord("Custom Reports", cpCore.docProperties.getInteger("RowID" + RowPtr));
										}
									}
								}
								break;
							case ButtonRequestDownload:
							case ButtonApply:
								//
								Name = cpCore.docProperties.getText("name");
								SQL = cpCore.docProperties.getText(SQLFieldName);
								if (!string.IsNullOrEmpty(Name) | !string.IsNullOrEmpty(SQL))
								{
									if ((string.IsNullOrEmpty(Name)) || (string.IsNullOrEmpty(SQL)))
									{
										errorController.error_AddUserError(cpCore, "A name and SQL Query are required to save a new custom report.");
									}
									else
									{
										CS = cpCore.db.csInsertRecord("Custom Reports");
										if (cpCore.db.csOk(CS))
										{
											cpCore.db.csSet(CS, "Name", Name);
											cpCore.db.csSet(CS, SQLFieldName, SQL);
										}
										cpCore.db.csClose(CS);
									}
								}
								//
								RowCnt = cpCore.docProperties.getInteger("RowCnt");
								if (RowCnt > 0)
								{
									for (RowPtr = 0; RowPtr < RowCnt; RowPtr++)
									{
										if (cpCore.docProperties.getBoolean("Row" + RowPtr))
										{
											RecordID = cpCore.docProperties.getInteger("RowID" + RowPtr);
											CS = cpCore.db.csOpenRecord("Custom Reports", RecordID);
											if (cpCore.db.csOk(CS))
											{
												SQL = cpCore.db.csGetText(CS, SQLFieldName);
												Name = cpCore.db.csGetText(CS, "Name");
											}
											cpCore.db.csClose(CS);
											//
											CS = cpCore.db.csInsertRecord("Tasks");
											if (cpCore.db.csOk(CS))
											{
												RecordName = "CSV Download, Custom Report [" + Name + "]";
												Filename = "CustomReport_" + Convert.ToString(genericController.dateToSeconds(cpCore.doc.profileStartTime)) + Convert.ToString(genericController.GetRandomInteger()) + ".csv";
												cpCore.db.csSet(CS, "Name", RecordName);
												cpCore.db.csSet(CS, "Filename", Filename);
												if (Format == "XML")
												{
													cpCore.db.csSet(CS, "Command", "BUILDXML");
												}
												else
												{
													cpCore.db.csSet(CS, "Command", "BUILDCSV");
												}
												cpCore.db.csSet(CS, SQLFieldName, SQL);
												Description = Description + "<p>Your Download [" + Name + "] has been requested, and will be available in the <a href=\"?" + RequestNameAdminForm + "=30\">Download Manager</a> when it is complete. This may take a few minutes depending on the size of the report.</p>";
											}
											cpCore.db.csClose(CS);
										}
									}
								}
								break;
						}
					}
					//
					// Build Tab0
					//
					Tab0.Add("<p>The following is a list of available custom reports.</p>");
					//
					RQS = cpCore.doc.refreshQueryString;
					PageSize = cpCore.docProperties.getInteger(RequestNamePageSize);
					if (PageSize == 0)
					{
						PageSize = 50;
					}
					PageNumber = cpCore.docProperties.getInteger(RequestNamePageNumber);
					if (PageNumber == 0)
					{
						PageNumber = 1;
					}
					AdminURL = "/" + cpCore.serverConfig.appConfig.adminRoute;
					TopCount = PageNumber * PageSize;
					//
					// Setup Headings
					//
					ColCaption = new string[ColumnCnt + 1];
					ColAlign = new string[ColumnCnt + 1];
					ColWidth = new string[ColumnCnt + 1];
					Cells = new string[PageSize + 1, ColumnCnt + 1];
					//
					ColCaption[ColumnPtr] = "Select<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=10 height=1>";
					ColAlign[ColumnPtr] = "center";
					ColWidth[ColumnPtr] = "10";
					ColumnPtr = ColumnPtr + 1;
					//
					ColCaption[ColumnPtr] = "Name";
					ColAlign[ColumnPtr] = "left";
					ColWidth[ColumnPtr] = "100%";
					ColumnPtr = ColumnPtr + 1;
					//
					ColCaption[ColumnPtr] = "Created By<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=100 height=1>";
					ColAlign[ColumnPtr] = "left";
					ColWidth[ColumnPtr] = "100";
					ColumnPtr = ColumnPtr + 1;
					//
					ColCaption[ColumnPtr] = "Date Created<br><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=150 height=1>";
					ColAlign[ColumnPtr] = "left";
					ColWidth[ColumnPtr] = "150";
					ColumnPtr = ColumnPtr + 1;
					//'
					//ColCaption(ColumnPtr) = "?<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=100 height=1>"
					//ColAlign(ColumnPtr) = "Left"
					//ColWidth(ColumnPtr) = "100"
					//ColumnPtr = ColumnPtr + 1
					//
					//   Get Data
					//
					CS = cpCore.db.csOpen("Custom Reports");
					RowPointer = 0;
					if (!cpCore.db.csOk(CS))
					{
						Cells[0, 1] = "There are no custom reports defined";
						RowPointer = 1;
					}
					else
					{
						DataRowCount = cpCore.db.csGetRowCount(CS);
						while (cpCore.db.csOk(CS) && (RowPointer < PageSize))
						{
							RecordID = cpCore.db.csGetInteger(CS, "ID");
							//DateCompleted = cpCore.db.cs_getDate(CS, "DateCompleted")
							Cells[RowPointer, 0] = cpCore.html.html_GetFormInputCheckBox2("Row" + RowPointer) + cpCore.html.html_GetFormInputHidden("RowID" + RowPointer, RecordID);
							Cells[RowPointer, 1] = cpCore.db.csGetText(CS, "name");
							Cells[RowPointer, 2] = cpCore.db.csGet(CS, "CreatedBy");
							Cells[RowPointer, 3] = cpCore.db.csGetDate(CS, "DateAdded").ToShortDateString;
							//Cells(RowPointer, 4) = "&nbsp;"
							RowPointer = RowPointer + 1;
							cpCore.db.csGoNext(CS);
						}
					}
					cpCore.db.csClose(CS);
					string Cell = null;
					Tab0.Add(cpCore.html.html_GetFormInputHidden("RowCnt", RowPointer));
					adminUIController Adminui = new adminUIController(cpCore);
					Cell = Adminui.GetReport(RowPointer, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, "ccPanel");
					Tab0.Add("<div>" + Cell + "</div>");
					//
					// Build RequestContent Form
					//
					Tab1.Add("<p>Use this form to create a new custom report. Enter the SQL Query for the report, and a name that will be used as a caption.</p>");
					//
					Tab1.Add("<table border=\"0\" cellpadding=\"3\" cellspacing=\"0\" width=\"100%\">");
					//
					Tab1.Add("<tr>");
					Tab1.Add("<td align=right>Name</td>");
					Tab1.Add("<td>" + cpCore.html.html_GetFormInputText2("Name", "", 1, 40) + "</td>");
					Tab1.Add("</tr>");
					//
					Tab1.Add("<tr>");
					Tab1.Add("<td align=right>SQL Query</td>");
					Tab1.Add("<td>" + cpCore.html.html_GetFormInputText2(SQLFieldName, "", 8, 40) + "</td>");
					Tab1.Add("</tr>");
					//
					Tab1.Add("" + "<tr>" + "<td width=\"120\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"120\" height=\"1\"></td>" + "<td width=\"100%\">&nbsp;</td>" + "</tr>" + "</table>");
					//
					// Build and add tabs
					//
					cpCore.html.main_AddLiveTabEntry("Custom&nbsp;Reports", Tab0.Text, "ccAdminTab");
					cpCore.html.main_AddLiveTabEntry("Request&nbsp;New&nbsp;Report", Tab1.Text, "ccAdminTab");
					Content = cpCore.html.main_GetLiveTabs();
					//
				}
				//
				tempGetForm_CustomReports = admin_GetAdminFormBody(Caption, ButtonListLeft, ButtonListRight, true, true, Description, ContentSummary, ContentPadding, Content);
				//
				cpCore.html.addTitle("Custom Reports");
				return tempGetForm_CustomReports;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			handleLegacyClassError3("GetForm_CustomReports");
			return tempGetForm_CustomReports;
		}






	}
}
