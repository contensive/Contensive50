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
		//========================================================================
		//   Returns the entire HTML page based on the bid/sid stream values
		//
		//   This code is based on the GoMethod site script
		//========================================================================
		//
		public static string getHtmlBody(coreClass cpCore)
		{
			string returnHtml = "";
			try
			{
				int downloadId = 0;
				int Pos = 0;
				string htmlDocBody = null;
				string Clip = null;
				int ClipParentRecordID = 0;
				int ClipParentContentID = 0;
				string ClipParentContentName = null;
				int ClipChildContentID = 0;
				string ClipChildContentName = null;
				int ClipChildRecordID = 0;
				string ClipChildRecordName = null;
				int CSClip = 0;
				string[] ClipBoardArray = null;
				string ClipBoard = null;
				string ClipParentFieldList = null;
				string[] Fields = null;
				int FieldCount = 0;
				string[] NameValues = null;
				string RedirectLink = "";
				string RedirectReason = "";
				string PageNotFoundReason = "";
				string PageNotFoundSource = "";
				bool IsPageNotFound = false;
				//
				if (cpCore.doc.continueProcessing)
				{
					//
					// -- setup domain
					string domainTest = cpCore.webServer.requestDomain.Trim().ToLower().Replace("..", ".");
					cpCore.doc.domain = null;
					if (!string.IsNullOrEmpty(domainTest))
					{
						int posDot = 0;
						int loopCnt = 10;
						do
						{
							cpCore.doc.domain = Models.Entity.domainModel.createByName(cpCore, domainTest, new List<string>());
							posDot = domainTest.IndexOf('.');
							if ((posDot >= 0) && (domainTest.Length > 1))
							{
								domainTest = domainTest.Substring(posDot + 1);
							}
							loopCnt -= 1;
						} while ((cpCore.doc.domain == null) && (posDot >= 0) && (loopCnt > 0));
					}


					if (cpCore.doc.domain == null)
					{

					}
					//
					// -- load requested page/template
					pageContentController.loadPage(cpCore, cpCore.docProperties.getInteger(rnPageId), cpCore.doc.domain);
					//
					// -- execute context for included addons
					CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext()
					{
						addonType = CPUtilsBaseClass.addonContext.ContextSimple,
						cssContainerClass = "",
						cssContainerId = "",
						hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext()
						{
							contentName = pageContentModel.contentName,
							fieldName = "copyfilename",
							recordId = cpCore.doc.page.id
						},
						isIncludeAddon = false,
						personalizationAuthenticated = cpCore.doc.authContext.visit.VisitAuthenticated,
						personalizationPeopleId = cpCore.doc.authContext.user.id
					};

					//
					// -- execute template Dependencies
					List<Models.Entity.addonModel> templateAddonList = addonModel.createList_templateDependencies(cpCore, cpCore.doc.template.ID);
					foreach (addonModel addon in templateAddonList)
					{
						bool AddonStatusOK = true;
						returnHtml += cpCore.addon.executeDependency(addon, executeContext);
						//returnHtml &= cpCore.addon.executeDependency(addon.id, CPUtilsBaseClass.addonContext.ContextSimple, pageContentModel.contentName, cpCore.doc.page.id, "copyFilename", "", 0, AddonStatusOK, cpCore.doc.authContext.user.id, cpCore.doc.authContext.visit.VisitAuthenticated)
					}
					//
					// -- execute page Dependencies
					List<Models.Entity.addonModel> pageAddonList = addonModel.createList_pageDependencies(cpCore, cpCore.doc.page.id);
					foreach (addonModel addon in pageAddonList)
					{
						bool AddonStatusOK = true;
						returnHtml += cpCore.addon.executeDependency(addon, executeContext);
						//returnHtml &= cpCore.addon.executeDependency(addon.id, CPUtilsBaseClass.addonContext.ContextSimple, pageContentModel.contentName, cpCore.doc.page.id, "copyFilename", "", 0, AddonStatusOK, cpCore.doc.authContext.user.id, cpCore.doc.authContext.visit.VisitAuthenticated)
					}
					//
					cpCore.doc.adminWarning = cpCore.docProperties.getText("main_AdminWarningMsg");
					cpCore.doc.adminWarningPageID = cpCore.docProperties.getInteger("main_AdminWarningPageID");
					//
					// todo move cookie test to htmlDoc controller
					// -- Add cookie test
					bool AllowCookieTest = cpCore.siteProperties.allowVisitTracking && (cpCore.doc.authContext.visit.PageVisits == 1);
					if (AllowCookieTest)
					{
						cpCore.html.addScriptCode_onLoad("if (document.cookie && document.cookie != null){cj.ajax.qs('f92vo2a8d=" + cpCore.security.encodeToken(cpCore.doc.authContext.visit.id, cpCore.doc.profileStartTime) + "')};", "Cookie Test");
					}
					//
					//--------------------------------------------------------------------------
					//   User form processing
					//       if a form is created in the editor, process it by emailing and saving to the User Form Response content
					//--------------------------------------------------------------------------
					//
					if (cpCore.docProperties.getInteger("ContensiveUserForm") == 1)
					{
						string FromAddress = cpCore.siteProperties.getText("EmailFromAddress", "info@" + cpCore.webServer.requestDomain);
						cpCore.email.sendForm(cpCore.siteProperties.emailAdmin, FromAddress, "Form Submitted on " + cpCore.webServer.requestReferer);
						int cs = cpCore.db.csInsertRecord("User Form Response");
						if (cpCore.db.csOk(cs))
						{
							cpCore.db.csSet(cs, "name", "Form " + cpCore.webServer.requestReferrer);
							string Copy = "";

							foreach (string key in cpCore.docProperties.getKeyList())
							{
								docPropertiesClass docProperty = cpCore.docProperties.getProperty(key);
								if (key.ToLower() != "contensiveuserform")
								{
									Copy += docProperty.Name + "=" + docProperty.Value + Environment.NewLine;
								}
							}
							cpCore.db.csSet(cs, "copy", Copy);
							cpCore.db.csSet(cs, "VisitId", cpCore.doc.authContext.visit.id);
						}
						cpCore.db.csClose(cs);
					}
					//
					//--------------------------------------------------------------------------
					//   Contensive Form Page Processing
					//--------------------------------------------------------------------------
					//
					if (cpCore.docProperties.getInteger("ContensiveFormPageID") != 0)
					{
						processForm(cpCore, cpCore.docProperties.getInteger("ContensiveFormPageID"));
					}
					//
					//--------------------------------------------------------------------------
					// ----- Automatic Redirect to a full URL
					//   If the link field of the record is an absolution address
					//       rc = redirect contentID
					//       ri = redirect content recordid
					//--------------------------------------------------------------------------
					//
					cpCore.doc.redirectContentID = (cpCore.docProperties.getInteger(rnRedirectContentId));
					if (cpCore.doc.redirectContentID != 0)
					{
						cpCore.doc.redirectRecordID = (cpCore.docProperties.getInteger(rnRedirectRecordId));
						if (cpCore.doc.redirectRecordID != 0)
						{
							string contentName = Models.Complex.cdefModel.getContentNameByID(cpCore, cpCore.doc.redirectContentID);
							if (!string.IsNullOrEmpty(contentName))
							{
								if (iisController.main_RedirectByRecord_ReturnStatus(cpCore, contentName, cpCore.doc.redirectRecordID))
								{
									//
									//Call AppendLog("main_init(), 3210 - exit for rc/ri redirect ")
									//
									cpCore.doc.continueProcessing = false; //--- should be disposed by caller --- Call dispose
									return string.Empty;
								}
								else
								{
									cpCore.doc.adminWarning = "<p>The site attempted to automatically jump to another page, but there was a problem with the page that included the link.<p>";
									cpCore.doc.adminWarningPageID = cpCore.doc.redirectRecordID;
								}
							}
						}
					}
					//
					//--------------------------------------------------------------------------
					// ----- Active Download hook
					string RecordEID = cpCore.docProperties.getText(RequestNameLibraryFileID);
					if (!string.IsNullOrEmpty(RecordEID))
					{
						DateTime tokenDate = default(DateTime);
						cpCore.security.decodeToken(RecordEID, downloadId, tokenDate);
						if (downloadId != 0)
						{
							//
							// -- lookup record and set clicks
							Models.Entity.libraryFilesModel file = Models.Entity.libraryFilesModel.create(cpCore, downloadId);
							if (file != null)
							{
								file.Clicks += 1;
								file.save(cpCore);
								if (file.Filename != "")
								{
									//
									// -- create log entry
									Models.Entity.libraryFileLogModel log = Models.Entity.libraryFileLogModel.add(cpCore);
									if (log != null)
									{
										log.FileID = file.id;
										log.VisitID = cpCore.doc.authContext.visit.id;
										log.MemberID = cpCore.doc.authContext.user.id;
									}
									//
									// -- and go
									string link = cpCore.webServer.requestProtocol + cpCore.webServer.requestDomain + genericController.getCdnFileLink(cpCore, file.Filename);
									return cpCore.webServer.redirect(link, "Redirecting because the active download request variable is set to a valid Library Files record. Library File Log has been appended.");
								}
							}
							//
						}
					}
					//
					//--------------------------------------------------------------------------
					//   Process clipboard cut/paste
					//--------------------------------------------------------------------------
					//
					Clip = cpCore.docProperties.getText(RequestNameCut);
					if (!string.IsNullOrEmpty(Clip))
					{
						//
						// if a cut, load the clipboard
						//
						cpCore.visitProperty.setProperty("Clipboard", Clip);
						genericController.modifyLinkQuery(cpCore.doc.refreshQueryString, RequestNameCut, "");
					}
					ClipParentContentID = cpCore.docProperties.getInteger(RequestNamePasteParentContentID);
					ClipParentRecordID = cpCore.docProperties.getInteger(RequestNamePasteParentRecordID);
					ClipParentFieldList = cpCore.docProperties.getText(RequestNamePasteFieldList);
					if ((ClipParentContentID != 0) & (ClipParentRecordID != 0))
					{
						//
						// Request for a paste, clear the cliboard
						//
						ClipBoard = cpCore.visitProperty.getText("Clipboard", "");
						cpCore.visitProperty.setProperty("Clipboard", "");
						genericController.ModifyQueryString(cpCore.doc.refreshQueryString, RequestNamePasteParentContentID, "");
						genericController.ModifyQueryString(cpCore.doc.refreshQueryString, RequestNamePasteParentRecordID, "");
						ClipParentContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ClipParentContentID);
						if (string.IsNullOrEmpty(ClipParentContentName))
						{
							// state not working...
						}
						else if (string.IsNullOrEmpty(ClipBoard))
						{
							// state not working...
						}
						else
						{
							if (!cpCore.doc.authContext.isAuthenticatedContentManager(cpCore, ClipParentContentName))
							{
								errorController.error_AddUserError(cpCore, "The paste operation failed because you are not a content manager of the Clip Parent");
							}
							else
							{
								//
								// Current identity is a content manager for this content
								//
								int Position = genericController.vbInstr(1, ClipBoard, ".");
								if (Position == 0)
								{
									errorController.error_AddUserError(cpCore, "The paste operation failed because the clipboard data is configured incorrectly.");
								}
								else
								{
									ClipBoardArray = ClipBoard.Split('.');
									if (ClipBoardArray.GetUpperBound(0) == 0)
									{
										errorController.error_AddUserError(cpCore, "The paste operation failed because the clipboard data is configured incorrectly.");
									}
									else
									{
										ClipChildContentID = genericController.EncodeInteger(ClipBoardArray[0]);
										ClipChildRecordID = genericController.EncodeInteger(ClipBoardArray[1]);
										if (!Models.Complex.cdefModel.isWithinContent(cpCore, ClipChildContentID, ClipParentContentID))
										{
											errorController.error_AddUserError(cpCore, "The paste operation failed because the destination location is not compatible with the clipboard data.");
										}
										else
										{
											//
											// the content definition relationship is OK between the child and parent record
											//
											ClipChildContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ClipChildContentID);
											if (!(!string.IsNullOrEmpty(ClipChildContentName)))
											{
												errorController.error_AddUserError(cpCore, "The paste operation failed because the clipboard data content is undefined.");
											}
											else
											{
												if (ClipParentRecordID == 0)
												{
													errorController.error_AddUserError(cpCore, "The paste operation failed because the clipboard data record is undefined.");
												}
												else if (pageContentController.isChildRecord(cpCore, ClipChildContentName, ClipParentRecordID, ClipChildRecordID))
												{
													errorController.error_AddUserError(cpCore, "The paste operation failed because the destination location is a child of the clipboard data record.");
												}
												else
												{
													//
													// the parent record is not a child of the child record (circular check)
													//
													ClipChildRecordName = "record " + ClipChildRecordID;
													CSClip = cpCore.db.cs_open2(ClipChildContentName, ClipChildRecordID, true, true);
													if (!cpCore.db.csOk(CSClip))
													{
														errorController.error_AddUserError(cpCore, "The paste operation failed because the data record referenced by the clipboard could not found.");
													}
													else
													{
														//
														// Paste the edit record record
														//
														ClipChildRecordName = cpCore.db.csGetText(CSClip, "name");
														if (string.IsNullOrEmpty(ClipParentFieldList))
														{
															//
															// Legacy paste - go right to the parent id
															//
															if (!cpCore.db.cs_isFieldSupported(CSClip, "ParentID"))
															{
																errorController.error_AddUserError(cpCore, "The paste operation failed because the record you are pasting does not   support the necessary parenting feature.");
															}
															else
															{
																cpCore.db.csSet(CSClip, "ParentID", ClipParentRecordID);
															}
														}
														else
														{
															//
															// Fill in the Field List name values
															//
															Fields = ClipParentFieldList.Split(',');
															FieldCount = Fields.GetUpperBound(0) + 1;
															for (var FieldPointer = 0; FieldPointer < FieldCount; FieldPointer++)
															{
																string Pair = Fields[FieldPointer];
																if (Pair.Substring(0, 1) == "(" && Pair.Substring(Pair.Length - 1, 1) == ")")
																{
																	Pair = Pair.Substring(1, Pair.Length - 2);
																}
																NameValues = Pair.Split('=');
																if (NameValues.GetUpperBound(0) == 0)
																{
																	errorController.error_AddUserError(cpCore, "The paste operation failed because the clipboard data Field List is not configured correctly.");
																}
																else
																{
																	if (!cpCore.db.cs_isFieldSupported(CSClip, Convert.ToString(NameValues[0])))
																	{
																		errorController.error_AddUserError(cpCore, "The paste operation failed because the clipboard data Field [" + Convert.ToString(NameValues[0]) + "] is not supported by the location data.");
																	}
																	else
																	{
																		cpCore.db.csSet(CSClip, Convert.ToString(NameValues[0]), Convert.ToString(NameValues[1]));
																	}
																}
															}
														}
														//'
														//' Fixup Content Watch
														//'
														//ShortLink = main_ServerPathPage
														//ShortLink = ConvertLinkToShortLink(ShortLink, main_ServerHost, main_ServerVirtualPath)
														//ShortLink = genericController.modifyLinkQuery(ShortLink, rnPageId, CStr(ClipChildRecordID), True)
														//Call main_TrackContentSet(CSClip, ShortLink)
													}
													cpCore.db.csClose(CSClip);
													//
													// Set Child Pages Found and clear caches
													//
													CSClip = cpCore.db.csOpenRecord(ClipParentContentName, ClipParentRecordID,,, "ChildPagesFound");
													if (cpCore.db.csOk(CSClip))
													{
														cpCore.db.csSet(CSClip, "ChildPagesFound", true.ToString());
													}
													cpCore.db.csClose(CSClip);
													//
													// Live Editing
													//
													cpCore.cache.invalidateAllObjectsInContent(ClipChildContentName);
													cpCore.cache.invalidateAllObjectsInContent(ClipParentContentName);
												}
											}
										}
									}
								}
							}
						}
					}
					Clip = cpCore.docProperties.getText(RequestNameCutClear);
					if (!string.IsNullOrEmpty(Clip))
					{
						//
						// if a cut clear, clear the clipboard
						//
						cpCore.visitProperty.setProperty("Clipboard", "");
						Clip = cpCore.visitProperty.getText("Clipboard", "");
						genericController.modifyLinkQuery(cpCore.doc.refreshQueryString, RequestNameCutClear, "");
					}
					//
					// link alias and link forward
					//
					string LinkAliasCriteria = "";
					string linkAliasTest1 = "";
					string linkAliasTest2 = null;
					string LinkNoProtocol = "";
					string linkDomain = "";
					string LinkFullPath = "";
					string LinkFullPathNoSlash = "";
					bool isLinkForward = false;
					string LinkForwardCriteria = "";
					string Sql = "";
					int CSPointer = -1;
					bool IsInLinkForwardTable = false;
					int Viewings = 0;
					string[] LinkSplit = null;
					bool IsLinkAlias = false;
					//
					//--------------------------------------------------------------------------
					// try link alias
					//--------------------------------------------------------------------------
					//
					LinkAliasCriteria = "";
					linkAliasTest1 = cpCore.webServer.requestPathPage;
					if (linkAliasTest1.Substring(0, 1) == "/")
					{
						linkAliasTest1 = linkAliasTest1.Substring(1);
					}
					if (linkAliasTest1.Length > 0)
					{
						if (linkAliasTest1.Substring(linkAliasTest1.Length - 1, 1) == "/")
						{
							linkAliasTest1 = linkAliasTest1.Substring(0, linkAliasTest1.Length - 1);
						}
					}

					linkAliasTest2 = linkAliasTest1 + "/";
					if ((!IsPageNotFound) && (cpCore.webServer.requestPathPage != ""))
					{
						//
						// build link variations needed later
						//
						//
						Pos = genericController.vbInstr(1, cpCore.webServer.requestPathPage, "://", Microsoft.VisualBasic.Constants.vbTextCompare);
						if (Pos != 0)
						{
							LinkNoProtocol = cpCore.webServer.requestPathPage.Substring(Pos + 2);
							Pos = genericController.vbInstr(Pos + 3, cpCore.webServer.requestPathPage, "/", Microsoft.VisualBasic.Constants.vbBinaryCompare);
							if (Pos != 0)
							{
								linkDomain = cpCore.webServer.requestPathPage.Substring(0, Pos - 1);
								LinkFullPath = cpCore.webServer.requestPathPage.Substring(Pos - 1);
								//
								// strip off leading or trailing slashes, and return only the string between the leading and secton slash
								//
								if (genericController.vbInstr(1, LinkFullPath, "/") != 0)
								{
									LinkSplit = LinkFullPath.Split('/');
									LinkFullPathNoSlash = LinkSplit[0];
									if (string.IsNullOrEmpty(LinkFullPathNoSlash))
									{
										if (LinkSplit.GetUpperBound(0) > 0)
										{
											LinkFullPathNoSlash = LinkSplit[1];
										}
									}
								}
								linkAliasTest1 = LinkFullPath;
								linkAliasTest2 = LinkFullPathNoSlash;
							}
						}
						//
						//   if this has not already been recognized as a pagenot found, and the custom404source is present, try all these
						//   Build LinkForwardCritia and LinkAliasCriteria
						//   sample: http://www.a.com/kb/test
						//   LinkForwardCriteria = (Sourcelink='http://www.a.com/kb/test')or(Sourcelink='http://www.a.com/kb/test/')
						//
						LinkForwardCriteria = ""
							+ "(active<>0)"
							+ "and("
							+ "(SourceLink=" + cpCore.db.encodeSQLText(cpCore.webServer.requestPathPage) + ")"
							+ "or(SourceLink=" + cpCore.db.encodeSQLText(LinkNoProtocol) + ")"
							+ "or(SourceLink=" + cpCore.db.encodeSQLText(LinkFullPath) + ")"
							+ "or(SourceLink=" + cpCore.db.encodeSQLText(LinkFullPathNoSlash) + ")"
							+ ")";
						isLinkForward = false;
						Sql = cpCore.db.GetSQLSelect("", "ccLinkForwards", "ID,DestinationLink,Viewings,GroupID", LinkForwardCriteria, "ID",, 1);
						CSPointer = cpCore.db.csOpenSql(Sql);
						if (cpCore.db.csOk(CSPointer))
						{
							//
							// Link Forward found - update count
							//
							string tmpLink = null;
							int GroupID = 0;
							string groupName = null;
							//
							IsInLinkForwardTable = true;
							Viewings = cpCore.db.csGetInteger(CSPointer, "Viewings") + 1;
							Sql = "update ccLinkForwards set Viewings=" + Viewings + " where ID=" + cpCore.db.csGetInteger(CSPointer, "ID");
							cpCore.db.executeQuery(Sql);
							tmpLink = cpCore.db.csGetText(CSPointer, "DestinationLink");
							if (!string.IsNullOrEmpty(tmpLink))
							{
								//
								// Valid Link Forward (without link it is just a record created by the autocreate function
								//
								isLinkForward = true;
								tmpLink = cpCore.db.csGetText(CSPointer, "DestinationLink");
								GroupID = cpCore.db.csGetInteger(CSPointer, "GroupID");
								if (GroupID != 0)
								{
									groupName = groupController.group_GetGroupName(cpCore, GroupID);
									if (!string.IsNullOrEmpty(groupName))
									{
										groupController.group_AddGroupMember(cpCore, groupName);
									}
								}
								if (!string.IsNullOrEmpty(tmpLink))
								{
									RedirectLink = tmpLink;
									RedirectReason = "Redirecting because the URL Is a valid Link Forward entry.";
								}
							}
						}
						cpCore.db.csClose(CSPointer);
						//
						if ((string.IsNullOrEmpty(RedirectLink)) && !isLinkForward)
						{
							//
							// Test for Link Alias
							//
							if (!string.IsNullOrEmpty(linkAliasTest1 + linkAliasTest2))
							{
								string sqlLinkAliasCriteria = "(name=" + cpCore.db.encodeSQLText(linkAliasTest1) + ")or(name=" + cpCore.db.encodeSQLText(linkAliasTest2) + ")";
								List<Models.Entity.linkAliasModel> linkAliasList = Models.Entity.linkAliasModel.createList(cpCore, sqlLinkAliasCriteria, "id desc");
								if (linkAliasList.Count > 0)
								{
									Models.Entity.linkAliasModel linkAlias = linkAliasList.First;
									string LinkQueryString = rnPageId + "=" + linkAlias.PageID + "&" + linkAlias.QueryStringSuffix;
									cpCore.docProperties.setProperty(rnPageId, linkAlias.PageID.ToString(), false);
									string[] nameValuePairs = linkAlias.QueryStringSuffix.Split('&');
									//Dim nameValuePairs As String() = Split(cpCore.cache_linkAlias(linkAliasCache_queryStringSuffix, Ptr), "&")
									foreach (string nameValuePair in nameValuePairs)
									{
										string[] nameValueThing = nameValuePair.Split('=');
										if (nameValueThing.GetUpperBound(0) == 0)
										{
											cpCore.docProperties.setProperty(nameValueThing[0], "", false);
										}
										else
										{
											cpCore.docProperties.setProperty(nameValueThing[0], nameValueThing[1], false);
										}
									}
								}
							}
							//
							if (!IsLinkAlias)
							{
								//
								// No Link Forward, no Link Alias, no RemoteMethodFromPage, not Robots.txt
								//
								if ((cpCore.doc.errorCount == 0) && cpCore.siteProperties.getBoolean("LinkForwardAutoInsert") && (!IsInLinkForwardTable))
								{
									//
									// Add a new Link Forward entry
									//
									CSPointer = cpCore.db.csInsertRecord("Link Forwards");
									if (cpCore.db.csOk(CSPointer))
									{
										cpCore.db.csSet(CSPointer, "Name", cpCore.webServer.requestPathPage);
										cpCore.db.csSet(CSPointer, "sourcelink", cpCore.webServer.requestPathPage);
										cpCore.db.csSet(CSPointer, "Viewings", 1);
									}
									cpCore.db.csClose(CSPointer);
								}
								//'
								//' real 404
								//'
								//IsPageNotFound = True
								//PageNotFoundSource = cpCore.webServer.requestPathPage
								//PageNotFoundReason = "The page could Not be displayed because the URL Is Not a valid page, Link Forward, Link Alias Or RemoteMethod."
							}
						}
					}
					//
					// ----- do anonymous access blocking
					//
					if (!cpCore.doc.authContext.isAuthenticated())
					{
						if ((cpCore.webServer.requestPath != "/") & genericController.vbInstr(1, "/" + cpCore.serverConfig.appConfig.adminRoute, cpCore.webServer.requestPath, Microsoft.VisualBasic.Constants.vbTextCompare) != 0)
						{
							//
							// admin page is excluded from custom blocking
							//
						}
						else
						{
							int AnonymousUserResponseID = genericController.EncodeInteger(cpCore.siteProperties.getText("AnonymousUserResponseID", "0"));
							switch (AnonymousUserResponseID)
							{
								case 1:
									//
									// -- block with login
									cpCore.doc.continueProcessing = false;
									return cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginPage), new CPUtilsBaseClass.addonExecuteContext() {addonType = CPUtilsBaseClass.addonContext.ContextPage});
								case 2:
									//
									// -- block with custom content
									cpCore.doc.continueProcessing = false;
									cpCore.doc.setMetaContent(0, 0);
									cpCore.html.addScriptCode_onLoad("document.body.style.overflow='scroll'", "Anonymous User Block");
									return cpCore.html.getHtmlDoc('\r' + cpCore.html.html_GetContentCopy("AnonymousUserResponseCopy", "<p style=\"width:250px;margin:100px auto auto auto;\">The site is currently not available for anonymous access.</p>", cpCore.doc.authContext.user.id, true, cpCore.doc.authContext.isAuthenticated), TemplateDefaultBodyTag, true, true);
							}
						}
					}
					//
					// -- build document
					htmlDocBody = getHtmlBodyTemplate(cpCore);
					//
					// -- check secure certificate required
					bool SecureLink_Template_Required = cpCore.doc.template.IsSecure;
					bool SecureLink_Page_Required = false;
					foreach (Models.Entity.pageContentModel page in cpCore.doc.pageToRootList)
					{
						if (cpCore.doc.page.IsSecure)
						{
							SecureLink_Page_Required = true;
							break;
						}
					}
					bool SecureLink_Required = SecureLink_Template_Required || SecureLink_Page_Required;
					bool SecureLink_CurrentURL = (cpCore.webServer.requestUrl.ToLower().Substring(0, 8) == "https://");
					if (SecureLink_CurrentURL && (!SecureLink_Required))
					{
						//
						// -- redirect to non-secure
						RedirectLink = genericController.vbReplace(cpCore.webServer.requestUrl, "https://", "http://");
						cpCore.doc.redirectReason = "Redirecting because neither the page or the template requires a secure link.";
						return "";
					}
					else if ((!SecureLink_CurrentURL) && SecureLink_Required)
					{
						//
						// -- redirect to secure
						RedirectLink = genericController.vbReplace(cpCore.webServer.requestUrl, "http://", "https://");
						if (SecureLink_Page_Required)
						{
							cpCore.doc.redirectReason = "Redirecting because this page [" + cpCore.doc.pageToRootList(0).name + "] requires a secure link.";
						}
						else
						{
							cpCore.doc.redirectReason = "Redirecting because this template [" + cpCore.doc.template.Name + "] requires a secure link.";
						}
						return "";
					}
					//
					// -- check that this template exists on this domain
					// -- if endpoint is just domain -> the template is automatically compatible by default (domain determined the landing page)
					// -- if endpoint is domain + route (link alias), the route determines the page, which may determine the cpCore.doc.template. If this template is not allowed for this domain, redirect to the domain's landingcpCore.doc.page.
					//
					Sql = "(domainId=" + cpCore.doc.domain.id + ")";
					List<Models.Entity.TemplateDomainRuleModel> allowTemplateRuleList = Models.Entity.TemplateDomainRuleModel.createList(cpCore, Sql);
					if (allowTemplateRuleList.Count == 0)
					{
						//
						// -- current template has no domain preference, use current
					}
					else
					{
						bool allowTemplate = false;
						foreach (TemplateDomainRuleModel rule in allowTemplateRuleList)
						{
							if (rule.templateId == cpCore.doc.template.ID)
							{
								allowTemplate = true;
								break;
							}
						}
						if (!allowTemplate)
						{
							//
							// -- must redirect to a domain's landing page
							RedirectLink = cpCore.webServer.requestProtocol + cpCore.doc.domain.name;
							cpCore.doc.redirectBecausePageNotFound = false;
							cpCore.doc.redirectReason = "Redirecting because this domain has template requiements set, and this template is not configured [" + cpCore.doc.template.Name + "].";
							return "";
						}
					}
					returnHtml += htmlDocBody;
				}
				//
				// all other routes should be handled here.
				//   - this code is in initApp right now but should be migrated here.
				//   - if all other routes fail, use the defaultRoute (page manager at first)
				//
				if (true)
				{
					// --- not reall sure what to do with this - was in appInit() and I am just sure it does not go there.
					//
					//--------------------------------------------------------------------------
					// ----- check if the custom404pathpage matches the defaultdoc
					//       in this case, the 404 hit is a direct result of a 404 I justreturned to IIS
					//       currently, I am redirecting to the page-not-found page with a 404 - wrong
					//       I should realize here that this is a 404 caused by the page in the 404 custom string
					//           and display the 404 page. Even if all I can say is "the page was not found"
					//
					//--------------------------------------------------------------------------
					//
					if (genericController.vbLCase(cpCore.webServer.requestPathPage) == genericController.vbLCase(requestAppRootPath + cpCore.siteProperties.serverPageDefault))
					{
						//
						// This is a 404 caused by Contensive returning a 404
						//   possibly because the pageid was not found or was inactive.
						//   contensive returned a 404 error, and the IIS custom error handler is hitting now
						//   what we returned as an error cause is lost
						//   ( because the Custom404Source page is the default page )
						//   send it to the 404 page
						//
						cpCore.webServer.requestPathPage = cpCore.webServer.requestPathPage;
						IsPageNotFound = true;
						PageNotFoundReason = "The page could not be displayed. The record may have been deleted, marked inactive. The page's parent pages or section may be invalid.";
					}
				}
				if (false)
				{
					//
					//todo consider if we will keep this. It is not straightforward, and and more straightforward method may exist
					//
					// Determine where to go next
					//   If the current page is not the referring page, redirect to the referring page
					//   Because...
					//   - the page with the form (the referrer) was a link alias page. You can not post to a link alias, so internally we post to the default page, and redirect back.
					//   - This only acts on internal Contensive forms, so developer pages are not effected
					//   - This way, if the form post comes from a main_GetJSPage Remote Method, it posts to the Content Server,
					//       then redirects back to the static site (with the new changed content)
					//
					if (cpCore.webServer.requestReferrer != "")
					{
						string main_ServerReferrerURL = null;
						string main_ServerReferrerQs = null;
						int Position = 0;
						main_ServerReferrerURL = cpCore.webServer.requestReferrer;
						main_ServerReferrerQs = "";
						Position = genericController.vbInstr(1, main_ServerReferrerURL, "?");
						if (Position != 0)
						{
							main_ServerReferrerQs = main_ServerReferrerURL.Substring(Position);
							main_ServerReferrerURL = main_ServerReferrerURL.Substring(0, Position - 1);
						}
						if (main_ServerReferrerURL.Substring(main_ServerReferrerURL.Length - 1) == "/")
						{
							//
							// Referer had no page, figure out what it should have been
							//
							if (cpCore.webServer.requestPage != "")
							{
								//
								// If the referer had no page, and there is one here now, it must have been from an IIS redirect, use the current page as the default page
								//
								main_ServerReferrerURL = main_ServerReferrerURL + cpCore.webServer.requestPage;
							}
							else
							{
								main_ServerReferrerURL = main_ServerReferrerURL + cpCore.siteProperties.serverPageDefault;
							}
						}
						string linkDst = null;
						//main_ServerPage = main_ServerPage
						if (main_ServerReferrerURL != cpCore.webServer.serverFormActionURL)
						{
							//
							// remove any methods from referrer
							//
							string Copy = "Redirecting because a Contensive Form was detected, source URL [" + main_ServerReferrerURL + "] does not equal the current URL [" + cpCore.webServer.serverFormActionURL + "]. This may be from a Contensive Add-on that now needs to redirect back to the host page.";
							linkDst = cpCore.webServer.requestReferer;
							if (!string.IsNullOrEmpty(main_ServerReferrerQs))
							{
								linkDst = main_ServerReferrerURL;
								main_ServerReferrerQs = genericController.ModifyQueryString(main_ServerReferrerQs, "method", "");
								if (!string.IsNullOrEmpty(main_ServerReferrerQs))
								{
									linkDst = linkDst + "?" + main_ServerReferrerQs;
								}
							}
							return cpCore.webServer.redirect(linkDst, Copy);
							cpCore.doc.continueProcessing = false; //--- should be disposed by caller --- Call dispose
						}
					}
				}
				if (true)
				{
					// - same here, this was in appInit() to prcess the pagenotfounds - maybe here (at the end, maybe in page Manager)
					//--------------------------------------------------------------------------
					// ----- Process Early page-not-found
					//--------------------------------------------------------------------------
					//
					if (IsPageNotFound)
					{
						if (true)
						{
							//
							// new way -- if a (real) 404 page is received, just convert this hit to the page-not-found page, do not redirect to it
							//
							logController.log_appendLogPageNotFound(cpCore, cpCore.webServer.requestUrlSource);
							cpCore.webServer.setResponseStatus("404 Not Found");
							cpCore.docProperties.setProperty(rnPageId, getPageNotFoundPageId(cpCore));
							//Call main_mergeInStream(rnPageId & "=" & main_GetPageNotFoundPageId())
							if (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore))
							{
								cpCore.doc.adminWarning = PageNotFoundReason;
								cpCore.doc.adminWarningPageID = 0;
							}
						}
						else
						{
							//
							// old way -- if a (real) 404 page is received, redirect to it to the 404 page with content
							//
							RedirectReason = PageNotFoundReason;
							RedirectLink = pageContentController.main_ProcessPageNotFound_GetLink(cpCore, PageNotFoundReason, "", PageNotFoundSource);
						}
					}
				}
				//
				// add exception list header
				//
				returnHtml = errorController.getDocExceptionHtmlList(cpCore) + returnHtml;
				//
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_GetHTMLDoc2")
			}
			return returnHtml;
		}
		//
		//
		//
		private static void processForm(coreClass cpcore, int FormPageID)
		{
			try
			{
				//
				int CS = 0;
				string SQL = null;
				string Formhtml = string.Empty;
				string FormInstructions = string.Empty;
				main_FormPagetype f = null;
				int Ptr = 0;
				int CSPeople = 0;
				bool IsInGroup = false;
				bool WasInGroup = false;
				string FormValue = null;
				bool Success = false;
				string PeopleFirstName = string.Empty;
				string PeopleLastName = string.Empty;
				string PeopleUsername = null;
				string PeoplePassword = null;
				string PeopleName = string.Empty;
				string PeopleEmail = string.Empty;
				string[] Groups = null;
				string GroupName = null;
				int GroupIDToJoinOnSuccess = 0;
				//
				// main_Get the instructions from the record
				//
				CS = cpcore.db.csOpenRecord("Form Pages", FormPageID);
				if (cpcore.db.csOk(CS))
				{
					Formhtml = cpcore.db.csGetText(CS, "Body");
					FormInstructions = cpcore.db.csGetText(CS, "Instructions");
				}
				cpcore.db.csClose(CS);
				if (!string.IsNullOrEmpty(FormInstructions))
				{
					//
					// Load the instructions
					//
					f = loadFormPageInstructions(cpcore, FormInstructions, Formhtml);
					if (f.AuthenticateOnFormProcess & !cpcore.doc.authContext.isAuthenticated() & cpcore.doc.authContext.isRecognized(cpcore))
					{
						//
						// If this form will authenticate when done, and their is a current, non-authenticated account -- logout first
						//
						cpcore.doc.authContext.logout(cpcore);
					}
					CSPeople = -1;
					Success = true;
					for (Ptr = 0; Ptr <= f.Inst.GetUpperBound(0); Ptr++)
					{
						var tempVar = f.Inst(Ptr);
						switch (tempVar.Type)
						{
							case 1:
								//
								// People Record
								//
								FormValue = cpcore.docProperties.getText(tempVar.PeopleField);
								if ((!string.IsNullOrEmpty(FormValue)) & genericController.EncodeBoolean(Models.Complex.cdefModel.GetContentFieldProperty(cpcore, "people", tempVar.PeopleField, "uniquename")))
								{
									SQL = "select count(*) from ccMembers where " + tempVar.PeopleField + "=" + cpcore.db.encodeSQLText(FormValue);
									CS = cpcore.db.csOpenSql(SQL);
									if (cpcore.db.csOk(CS))
									{
										Success = cpcore.db.csGetInteger(CS, "cnt") == 0;
									}
									cpcore.db.csClose(CS);
									if (!Success)
									{
										errorController.error_AddUserError(cpcore, "The field [" + tempVar.Caption + "] must be unique, and the value [" + genericController.encodeHTML(FormValue) + "] has already been used.");
									}
								}
								if ((tempVar.REquired | genericController.EncodeBoolean(Models.Complex.cdefModel.GetContentFieldProperty(cpcore, "people", tempVar.PeopleField, "required"))) && string.IsNullOrEmpty(FormValue))
								{
									Success = false;
									errorController.error_AddUserError(cpcore, "The field [" + genericController.encodeHTML(tempVar.Caption) + "] is required.");
								}
								else
								{
									if (!cpcore.db.csOk(CSPeople))
									{
										CSPeople = cpcore.db.csOpenRecord("people", cpcore.doc.authContext.user.id);
									}
									if (cpcore.db.csOk(CSPeople))
									{
										switch (genericController.vbUCase(tempVar.PeopleField))
										{
											case "NAME":
												PeopleName = FormValue;
												cpcore.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
												break;
											case "FIRSTNAME":
												PeopleFirstName = FormValue;
												cpcore.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
												break;
											case "LASTNAME":
												PeopleLastName = FormValue;
												cpcore.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
												break;
											case "EMAIL":
												PeopleEmail = FormValue;
												cpcore.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
												break;
											case "USERNAME":
												PeopleUsername = FormValue;
												cpcore.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
												break;
											case "PASSWORD":
												PeoplePassword = FormValue;
												cpcore.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
												break;
											default:
												cpcore.db.csSet(CSPeople, tempVar.PeopleField, FormValue);
												break;
										}
									}
								}
								break;
							case 2:
								//
								// Group main_MemberShip
								//
								IsInGroup = cpcore.docProperties.getBoolean("Group" + tempVar.GroupName);
								WasInGroup = cpcore.doc.authContext.IsMemberOfGroup2(cpcore, tempVar.GroupName);
								if (WasInGroup && !IsInGroup)
								{
									groupController.group_DeleteGroupMember(cpcore, tempVar.GroupName);
								}
								else if (IsInGroup && !WasInGroup)
								{
									groupController.group_AddGroupMember(cpcore, tempVar.GroupName);
								}
								break;
						}
					}
					//
					// Create People Name
					//
					if (string.IsNullOrEmpty(PeopleName) && !string.IsNullOrEmpty(PeopleFirstName) & !string.IsNullOrEmpty(PeopleLastName))
					{
						if (cpcore.db.csOk(CSPeople))
						{
							cpcore.db.csSet(CSPeople, "name", PeopleFirstName + " " + PeopleLastName);
						}
					}
					cpcore.db.csClose(CSPeople);
					//
					// AuthenticationOnFormProcess requires Username/Password and must be valid
					//
					if (Success)
					{
						//
						// Authenticate
						//
						if (f.AuthenticateOnFormProcess)
						{
							cpcore.doc.authContext.authenticateById(cpcore, cpcore.doc.authContext.user.id, cpcore.doc.authContext);
						}
						//
						// Join Group requested by page that created form
						//
						DateTime tokenDate = default(DateTime);
						cpcore.security.decodeToken(cpcore.docProperties.getText("SuccessID"), GroupIDToJoinOnSuccess, tokenDate);
						//GroupIDToJoinOnSuccess = main_DecodeKeyNumber(main_GetStreamText2("SuccessID"))
						if (GroupIDToJoinOnSuccess != 0)
						{
							groupController.group_AddGroupMember(cpcore, groupController.group_GetGroupName(cpcore, GroupIDToJoinOnSuccess));
						}
						//
						// Join Groups requested by pageform
						//
						if (f.AddGroupNameList != "")
						{
							Groups = (Convert.ToString(f.AddGroupNameList).Trim(' ')).Split(',');
							for (Ptr = 0; Ptr <= Groups.GetUpperBound(0); Ptr++)
							{
								GroupName = Groups[Ptr].Trim(' ');
								if (!string.IsNullOrEmpty(GroupName))
								{
									groupController.group_AddGroupMember(cpcore, GroupName);
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
				throw;
			}
		}
		//
		// Instruction Format
		//   Line 1 - Version FormBuilderVersion
		//   Line 2+ instruction lines
		//   blank line
		//   FormHTML
		//
		// Instruction Line Format
		//   Type,Caption,Required,arguments
		//
		//   Types
		//       1 = People Content field
		//           arguments = FieldName
		//       2 = Group main_MemberShip
		//           arguments = GroupName
		//
		// FormHTML
		//   All HTML with the following:
		//   ##REPEAT></REPEAT> tags -- repeated for each instruction line
		//   {{CAPTION}} tags -- main_Gets the caption for each instruction line
		//   {{FIELD}} tags -- main_Gets the form field for each instruction line
		//
		internal static main_FormPagetype loadFormPageInstructions(coreClass cpcore, string FormInstructions, string Formhtml)
		{
			main_FormPagetype result = new main_FormPagetype();
			try
			{
				string RepeatBody = null;
				int PtrFront = 0;
				int PtrBack = 0;
				string[] i = null;
				int IPtr = 0;
				int IStart = 0;
				string[] IArgs = null;
				int CSPeople = 0;
				//
				if (true)
				{
					PtrFront = genericController.vbInstr(1, Formhtml, "{{REPEATSTART", Microsoft.VisualBasic.Constants.vbTextCompare);
					if (PtrFront > 0)
					{
						PtrBack = genericController.vbInstr(PtrFront, Formhtml, "}}");
						if (PtrBack > 0)
						{
							result.PreRepeat = Formhtml.Substring(0, PtrFront - 1);
							PtrFront = genericController.vbInstr(PtrBack, Formhtml, "{{REPEATEND", Microsoft.VisualBasic.Constants.vbTextCompare);
							if (PtrFront > 0)
							{
								result.RepeatCell = Formhtml.Substring(PtrBack + 1, PtrFront - PtrBack - 2);
								PtrBack = genericController.vbInstr(PtrFront, Formhtml, "}}");
								if (PtrBack > 0)
								{
									result.PostRepeat = Formhtml.Substring(PtrBack + 1);
									//
									// Decode instructions and build output
									//
									i = genericController.SplitCRLF(FormInstructions);
									if (i.GetUpperBound(0) > 0)
									{
										if (string.CompareOrdinal(i[0].Trim(' '), "1") >= 0)
										{
											//
											// decode Version 1 arguments, then start instructions line at line 1
											//
											result.AddGroupNameList = genericController.encodeText(i[1]);
											result.AuthenticateOnFormProcess = genericController.EncodeBoolean(i[2]);
											IStart = 3;
										}
										//
										// read in and compose the repeat lines
										//

										RepeatBody = "";
										CSPeople = -1;
//INSTANT C# TODO TASK: The following 'ReDim' could not be resolved. A possible reason may be that the object of the ReDim was not declared as an array:
										ReDim result.Inst(i.GetUpperBound(0));
										for (IPtr = 0; IPtr <= i.GetUpperBound(0) - IStart; IPtr++)
										{
											var tempVar = result.Inst(IPtr);
											IArgs = i[IPtr + IStart].Split(',');
											if (IArgs.GetUpperBound(0) >= main_IPosMax)
											{
												tempVar.Caption = IArgs[main_IPosCaption];
												tempVar.Type = genericController.EncodeInteger(IArgs[main_IPosType]);
												tempVar.REquired = genericController.EncodeBoolean(IArgs[main_IPosRequired]);
												switch (tempVar.Type)
												{
													case 1:
														//
														// People Record
														//
														tempVar.PeopleField = IArgs[main_IPosPeopleField];
														break;
													case 2:
														//
														// Group main_MemberShip
														//
														tempVar.GroupName = IArgs[main_IPosGroupName];
														break;
												}
											}
										}
									}
								}
							}
						}
					}
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