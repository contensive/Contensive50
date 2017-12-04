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
		//====================================================================================================
		//
		public void addScriptCode_onLoad(string code, string addedByMessage)
		{
			try
			{
				if (!string.IsNullOrEmpty(code))
				{
					cpCore.doc.htmlAssetList.Add(new htmlAssetClass()
					{
						assetType = htmlAssetTypeEnum.OnLoadScript,
						addedByMessage = addedByMessage,
						isLink = false,
						content = code
					});
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
		}
		//
		//====================================================================================================
		//
		public void addScriptCode_body(string code, string addedByMessage)
		{
			try
			{
				if (!string.IsNullOrEmpty(code))
				{
					cpCore.doc.htmlAssetList.Add(new htmlAssetClass()
					{
						assetType = htmlAssetTypeEnum.script,
						addedByMessage = addedByMessage,
						isLink = false,
						content = genericController.removeScriptTag(code)
					});
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
		}
		//
		//====================================================================================================
		//
		public void addScriptCode_head(string code, string addedByMessage)
		{
			try
			{
				if (!string.IsNullOrEmpty(code))
				{
					cpCore.doc.htmlAssetList.Add(new htmlAssetClass()
					{
						assetType = htmlAssetTypeEnum.script,
						inHead = true,
						addedByMessage = addedByMessage,
						isLink = false,
						content = genericController.removeScriptTag(code)
					});
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
		}
		//
		//=========================================================================================================
		//
		public void addScriptLink_Head(string Filename, string addedByMessage)
		{
			try
			{
				if (!string.IsNullOrEmpty(Filename))
				{
					cpCore.doc.htmlAssetList.Add(new htmlAssetClass
					{
						assetType = htmlAssetTypeEnum.script,
						addedByMessage = addedByMessage,
						isLink = true,
						inHead = true,
						content = Filename
					});
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
		}
		//
		//=========================================================================================================
		//
		public void addScriptLink_Body(string Filename, string addedByMessage)
		{
			try
			{
				if (!string.IsNullOrEmpty(Filename))
				{
					cpCore.doc.htmlAssetList.Add(new htmlAssetClass
					{
						assetType = htmlAssetTypeEnum.script,
						addedByMessage = addedByMessage,
						isLink = true,
						content = Filename
					});
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
		}
		//
		//=========================================================================================================
		//
		public void addTitle(string pageTitle, string addedByMessage = "")
		{
			try
			{
				if (!string.IsNullOrEmpty(pageTitle.Trim()))
				{
					cpCore.doc.htmlMetaContent_TitleList.Add(new htmlMetaClass()
					{
						addedByMessage = addedByMessage,
						content = pageTitle
					});
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
		}
		//
		//=========================================================================================================
		//
		public void addMetaDescription(string MetaDescription, string addedByMessage = "")
		{
			try
			{
				if (!string.IsNullOrEmpty(MetaDescription.Trim()))
				{
					cpCore.doc.htmlMetaContent_Description.Add(new htmlMetaClass()
					{
						addedByMessage = addedByMessage,
						content = MetaDescription
					});
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
		}
		//
		//=========================================================================================================
		//
		public void addStyleLink(string StyleSheetLink, string addedByMessage)
		{
			try
			{
				if (!string.IsNullOrEmpty(StyleSheetLink.Trim()))
				{
					cpCore.doc.htmlAssetList.Add(new htmlAssetClass()
					{
						addedByMessage = addedByMessage,
						assetType = htmlAssetTypeEnum.style,
						inHead = true,
						isLink = true,
						content = StyleSheetLink
					});
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
		}
		//
		//=========================================================================================================
		//
		public void addStyleCode(string code, string addedByMessage = "")
		{
			try
			{
				if (!string.IsNullOrEmpty(code.Trim()))
				{
					cpCore.doc.htmlAssetList.Add(new htmlAssetClass()
					{
						addedByMessage = addedByMessage,
						assetType = htmlAssetTypeEnum.style,
						inHead = true,
						isLink = false,
						content = code
					});
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
		}
		//
		//=========================================================================================================
		//
		public void addMetaKeywordList(string MetaKeywordList, string addedByMessage = "")
		{
			try
			{
				foreach (string keyword in MetaKeywordList.Split(','))
				{
					if (!string.IsNullOrEmpty(keyword))
					{
						cpCore.doc.htmlMetaContent_KeyWordList.Add(new htmlMetaClass()
						{
							addedByMessage = addedByMessage,
							content = keyword
						});
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
		}
		//
		//=========================================================================================================
		//
		public void addHeadTag(string HeadTag, string addedByMessage = "")
		{
			try
			{
				cpCore.doc.htmlMetaContent_OtherTags.Add(new htmlMetaClass()
				{
					addedByMessage = addedByMessage,
					content = HeadTag
				});
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
		}
		//
		//===================================================================================================
		//
		public string getEditWrapper(string Caption, string Content)
		{
			string result = Content;
			try
			{
				if (cpCore.doc.authContext.isEditingAnything())
				{
					result = html_GetLegacySiteStyles() + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccEditWrapper\">";
					if (!string.IsNullOrEmpty(Caption))
					{
						result += ""
								+ "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccEditWrapperCaption\">"
								+ genericController.encodeText(Caption) + "<!-- <img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=1 height=22 align=absmiddle> -->"
								+ "</td></tr></table>";
					}
					result += ""
							+ "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccEditWrapperContent\" id=\"editWrapper" + cpCore.doc.editWrapperCnt + "\">"
							+ genericController.encodeText(Content) + "</td></tr></table>"
							+ "</td></tr></table>";
					cpCore.doc.editWrapperCnt = cpCore.doc.editWrapperCnt + 1;
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//
		//===================================================================================================
		// To support the special case when the template calls this to encode itself, and the page content has already been rendered.
		//
		private string convertActiveContent_internal(string Source, int personalizationPeopleId, string ContextContentName, int ContextRecordID, int ContextContactPeopleID, bool PlainText, bool AddLinkEID, bool EncodeActiveFormatting, bool EncodeActiveImages, bool EncodeActiveEditIcons, bool EncodeActivePersonalization, string queryStringForLinkAppend, string ProtocolHostLink, bool IsEmailContent, int ignore_DefaultWrapperID, string ignore_TemplateCaseOnly_Content, CPUtilsBaseClass.addonContext Context, bool personalizationIsAuthenticated, object nothingObject, bool isEditingAnything)
		{
			string result = Source;
			try
			{
				//
				const string StartFlag = "<!-- ADDON";
				const string EndFlag = " -->";
				//
				bool DoAnotherPass = false;
				int ArgCnt = 0;
				string AddonGuid = null;
				string ACInstanceID = null;
				string[] ArgSplit = null;
				string AddonName = null;
				string addonOptionString = null;
				int LineStart = 0;
				int LineEnd = 0;
				string Copy = null;
				string[] Wrapper = null;
				string[] SegmentSplit = null;
				string AcCmd = null;
				string SegmentSuffix = null;
				string[] AcCmdSplit = null;
				string ACType = null;
				string[] ContentSplit = null;
				int ContentSplitCnt = 0;
				string Segment = null;
				int Ptr = 0;
				string CopyName = null;
				string ListName = null;
				string SortField = null;
				bool SortReverse = false;
				string AdminURL = null;
				//
				htmlToTextControllers converthtmlToText = null;
				//
				int iPersonalizationPeopleId = personalizationPeopleId;
				if (iPersonalizationPeopleId == 0)
				{
					iPersonalizationPeopleId = cpCore.doc.authContext.user.id;
				}
				//

				//hint = "csv_EncodeContent9 enter"
				if (!string.IsNullOrEmpty(result))
				{
					AdminURL = "/" + cpCore.serverConfig.appConfig.adminRoute;
					//
					//--------
					// cut-paste from csv_EncodeContent8
					//--------
					//
					// ----- Do EncodeCRLF Conversion
					//
					//hint = hint & ",010"
					if (cpCore.siteProperties.getBoolean("ConvertContentCRLF2BR", false) && (!PlainText))
					{
						result = genericController.vbReplace(result, "\r", "");
						result = genericController.vbReplace(result, "\n", "<br>");
					}
					//
					// ----- Do upgrade conversions (upgrade legacy objects and upgrade old images)
					//
					//hint = hint & ",020"
					result = upgradeActiveContent(result);
					//
					// ----- Do Active Content Conversion
					//
					//hint = hint & ",030"
					if (AddLinkEID || EncodeActiveFormatting || EncodeActiveImages || EncodeActiveEditIcons)
					{
						result = convertActiveContent_Internal_activeParts(result, iPersonalizationPeopleId, ContextContentName, ContextRecordID, ContextContactPeopleID, AddLinkEID, EncodeActiveFormatting, EncodeActiveImages, EncodeActiveEditIcons, EncodeActivePersonalization, queryStringForLinkAppend, ProtocolHostLink, IsEmailContent, AdminURL, personalizationIsAuthenticated, Context);
					}
					//
					// ----- Do Plain Text Conversion
					//
					//hint = hint & ",040"
					if (PlainText)
					{
						converthtmlToText = new htmlToTextControllers(cpCore);
						result = converthtmlToText.convert(result);
						converthtmlToText = null;
					}
					//
					// Process Active Content that must be run here to access webclass objects
					//     parse as {{functionname?querystring}}
					//
					//hint = hint & ",110"
					if ((!EncodeActiveEditIcons) && (result.IndexOf("{{") + 1 != 0))
					{
						ContentSplit = Microsoft.VisualBasic.Strings.Split(result, "{{", -1, Microsoft.VisualBasic.CompareMethod.Binary);
						result = "";
						ContentSplitCnt = ContentSplit.GetUpperBound(0) + 1;
						Ptr = 0;
						while (Ptr < ContentSplitCnt)
						{
							//hint = hint & ",200"
							Segment = ContentSplit[Ptr];
							if (Ptr == 0)
							{
								//
								// Add in the non-command text that is before the first command
								//
								result = result + Segment;
							}
							else if (!string.IsNullOrEmpty(Segment))
							{
								if (genericController.vbInstr(1, Segment, "}}") == 0)
								{
									//
									// No command found, return the marker and deliver the Segment
									//
									//hint = hint & ",210"
									result = result + "{{" + Segment;
								}
								else
								{
									//
									// isolate the command
									//
									//hint = hint & ",220"
									SegmentSplit = Microsoft.VisualBasic.Strings.Split(Segment, "}}", -1, Microsoft.VisualBasic.CompareMethod.Binary);
									AcCmd = SegmentSplit[0];
									SegmentSplit[0] = "";
									SegmentSuffix = string.Join("}}", SegmentSplit).Substring(2);
									if (!string.IsNullOrEmpty(AcCmd.Trim(' ')))
									{
										//
										// isolate the arguments
										//
										//hint = hint & ",230"
										AcCmdSplit = AcCmd.Split('?');
										ACType = AcCmdSplit[0].Trim(' ');
										if (AcCmdSplit.GetUpperBound(0) == 0)
										{
											addonOptionString = "";
										}
										else
										{
											addonOptionString = AcCmdSplit[1];
											addonOptionString = genericController.decodeHtml(addonOptionString);
										}
										//
										// execute the command
										//
										switch (genericController.vbUCase(ACType))
										{
											case ACTypeDynamicForm:
												//
												// Dynamic Form - run the core addon replacement instead
												CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext()
												{
													addonType = CPUtilsBaseClass.addonContext.ContextPage,
													cssContainerClass = "",
													cssContainerId = "",
													hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext()
													{
														contentName = ContextContentName,
														fieldName = "",
														recordId = ContextRecordID
													},
													personalizationAuthenticated = personalizationIsAuthenticated,
													personalizationPeopleId = iPersonalizationPeopleId,
													instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, addonOptionString)
												};
												Models.Entity.addonModel addon = Models.Entity.addonModel.create(cpCore, addonGuidDynamicForm);
												result += cpCore.addon.execute(addon, executeContext);
												break;
											case ACTypeChildList:
												//
												// Child Page List
												//
												//hint = hint & ",320"
												ListName = addonController.getAddonOption("name", addonOptionString);
												result = result + cpCore.doc.getChildPageList(ListName, ContextContentName, ContextRecordID, true);
												break;
											case ACTypeTemplateText:
												//
												// Text Box = copied here from gethtmlbody
												//
												CopyName = addonController.getAddonOption("new", addonOptionString);
												if (string.IsNullOrEmpty(CopyName))
												{
													CopyName = addonController.getAddonOption("name", addonOptionString);
													if (string.IsNullOrEmpty(CopyName))
													{
														CopyName = "Default";
													}
												}
												result = result + html_GetContentCopy(CopyName, "", iPersonalizationPeopleId, false, personalizationIsAuthenticated);
												break;
											case ACTypeWatchList:
												//
												// Watch List
												//
												//hint = hint & ",330"
												ListName = addonController.getAddonOption("LISTNAME", addonOptionString);
												SortField = addonController.getAddonOption("SORTFIELD", addonOptionString);
												SortReverse = genericController.EncodeBoolean(addonController.getAddonOption("SORTDIRECTION", addonOptionString));
												result = result + cpCore.doc.main_GetWatchList(cpCore, ListName, SortField, SortReverse);
												break;
											default:
												//
												// Unrecognized command - put all the syntax back in
												//
												//hint = hint & ",340"
												result = result + "{{" + AcCmd + "}}";
												break;
										}
									}
									//
									// add the SegmentSuffix back on
									//
									result = result + SegmentSuffix;
								}
							}
							//
							// Encode into Javascript if required
							//
							Ptr = Ptr + 1;
						}
					}
					//
					// Process Addons
					//   parse as <!-- Addon "Addon Name","OptionString" -->
					//   They are handled here because Addons are written against cpCoreClass, not the Content Server class
					//   ...so Group Email can not process addons 8(
					//   Later, remove the csv routine that translates <ac to this, and process it directly right here
					//   Later, rewrite so addons call csv, not cpCoreClass, so email processing can include addons
					// (2/16/2010) - move csv_EncodeContent to csv, or wait and move it all to CP
					//    eventually, everything should migrate to csv and/or cp to eliminate the cpCoreClass dependancy
					//    and all add-ons run as processes the same as they run on pages, or as remote methods
					// (2/16/2010) - if <!-- AC --> has four arguments, the fourth is the addon guid
					//
					if (result.IndexOf(StartFlag) + 1 != 0)
					{
						while (result.IndexOf(StartFlag) + 1 != 0)
						{
							LineStart = genericController.vbInstr(1, result, StartFlag);
							LineEnd = genericController.vbInstr(LineStart, result, EndFlag);
							if (LineEnd == 0)
							{
								logController.appendLog(cpCore, "csv_EncodeContent9, Addon could not be inserted into content because the HTML comment holding the position is not formated correctly");
								break;
							}
							else
							{
								AddonName = "";
								addonOptionString = "";
								ACInstanceID = "";
								AddonGuid = "";
								Copy = result.Substring(LineStart + 10, LineEnd - LineStart - 11);
								ArgSplit = genericController.SplitDelimited(Copy, ",");
								ArgCnt = ArgSplit.GetUpperBound(0) + 1;
								if (!string.IsNullOrEmpty(ArgSplit[0]))
								{
									AddonName = ArgSplit[0].Substring(1, ArgSplit[0].Length - 2);
									if (ArgCnt > 1)
									{
										if (!string.IsNullOrEmpty(ArgSplit[1]))
										{
											addonOptionString = ArgSplit[1].Substring(1, ArgSplit[1].Length - 2);
											addonOptionString = genericController.decodeHtml(addonOptionString.Trim(' '));
										}
										if (ArgCnt > 2)
										{
											if (!string.IsNullOrEmpty(ArgSplit[2]))
											{
												ACInstanceID = ArgSplit[2].Substring(1, ArgSplit[2].Length - 2);
											}
											if (ArgCnt > 3)
											{
												if (!string.IsNullOrEmpty(ArgSplit[3]))
												{
													AddonGuid = ArgSplit[3].Substring(1, ArgSplit[3].Length - 2);
												}
											}
										}
									}
									// dont have any way of getting fieldname yet

									CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext()
									{
										addonType = CPUtilsBaseClass.addonContext.ContextPage,
										cssContainerClass = "",
										cssContainerId = "",
										hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext()
										{
											contentName = ContextContentName,
											fieldName = "",
											recordId = ContextRecordID
										},
										personalizationAuthenticated = personalizationIsAuthenticated,
										personalizationPeopleId = iPersonalizationPeopleId,
										instanceGuid = ACInstanceID,
										instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, addonOptionString)
									};
									if (!string.IsNullOrEmpty(AddonGuid))
									{
										Copy = cpCore.addon.execute(Models.Entity.addonModel.create(cpCore, AddonGuid), executeContext);
										//Copy = cpCore.addon.execute_legacy6(0, AddonGuid, addonOptionString, CPUtilsBaseClass.addonContext.ContextPage, ContextContentName, ContextRecordID, "", ACInstanceID, False, ignore_DefaultWrapperID, ignore_TemplateCaseOnly_Content, False, Nothing, "", Nothing, "", iPersonalizationPeopleId, personalizationIsAuthenticated)
									}
									else
									{
										Copy = cpCore.addon.execute(Models.Entity.addonModel.createByName(cpCore, AddonName), executeContext);
										//Copy = cpCore.addon.execute_legacy6(0, AddonName, addonOptionString, CPUtilsBaseClass.addonContext.ContextPage, ContextContentName, ContextRecordID, "", ACInstanceID, False, ignore_DefaultWrapperID, ignore_TemplateCaseOnly_Content, False, Nothing, "", Nothing, "", iPersonalizationPeopleId, personalizationIsAuthenticated)
									}
								}
							}
							result = result.Substring(0, LineStart - 1) + Copy + result.Substring(LineEnd + 3);
						}
					}
					//
					// process out text block comments inserted by addons
					// remove all content between BlockTextStartMarker and the next BlockTextEndMarker, or end of copy
					// exception made for the content with just the startmarker because when the AC tag is replaced with
					// with the marker, encode content is called with the result, which is just the marker, and this
					// section will remove it
					//
					if ((!isEditingAnything) && (result != BlockTextStartMarker))
					{
						DoAnotherPass = true;
						while ((result.IndexOf(BlockTextStartMarker, System.StringComparison.OrdinalIgnoreCase) + 1 != 0) && DoAnotherPass)
						{
							LineStart = genericController.vbInstr(1, result, BlockTextStartMarker, Microsoft.VisualBasic.Constants.vbTextCompare);
							if (LineStart == 0)
							{
								DoAnotherPass = false;
							}
							else
							{
								LineEnd = genericController.vbInstr(LineStart, result, BlockTextEndMarker, Microsoft.VisualBasic.Constants.vbTextCompare);
								if (LineEnd <= 0)
								{
									DoAnotherPass = false;
									result = result.Substring(0, LineStart - 1);
								}
								else
								{
									LineEnd = genericController.vbInstr(LineEnd, result, " -->");
									if (LineEnd <= 0)
									{
										DoAnotherPass = false;
									}
									else
									{
										result = result.Substring(0, LineStart - 1) + result.Substring(LineEnd + 3);
										//returnValue = Mid(returnValue, 1, LineStart - 1) & Copy & Mid(returnValue, LineEnd + 4)
									}
								}
							}
						}
					}
					//
					// only valid for a webpage
					//
					if (true)
					{
						//
						// Add in EditWrappers for Aggregate scripts and replacements
						//   This is also old -- used here because csv encode content can create replacements and links, but can not
						//   insert wrappers. This is all done in GetAddonContents() now. This routine is left only to
						//   handle old style calls in cache.
						//
						//hint = hint & ",500, Adding edit wrappers"
						if (isEditingAnything)
						{
							if (result.IndexOf("<!-- AFScript -->", System.StringComparison.OrdinalIgnoreCase) + 1 != 0)
							{
								throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError7("returnValue", "AFScript Style edit wrappers are not supported")
								Copy = getEditWrapper("Aggregate Script", "##MARKER##");
								Wrapper = Microsoft.VisualBasic.Strings.Split(Copy, "##MARKER##", -1, Microsoft.VisualBasic.CompareMethod.Binary);
								result = genericController.vbReplace(result, "<!-- AFScript -->", Wrapper[0], 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
								result = genericController.vbReplace(result, "<!-- /AFScript -->", Wrapper[1], 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
							}
							if (result.IndexOf("<!-- AFReplacement -->", System.StringComparison.OrdinalIgnoreCase) + 1 != 0)
							{
								throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError7("returnValue", "AFReplacement Style edit wrappers are not supported")
								Copy = getEditWrapper("Aggregate Replacement", "##MARKER##");
								Wrapper = Microsoft.VisualBasic.Strings.Split(Copy, "##MARKER##", -1, Microsoft.VisualBasic.CompareMethod.Binary);
								result = genericController.vbReplace(result, "<!-- AFReplacement -->", Wrapper[0], 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
								result = genericController.vbReplace(result, "<!-- /AFReplacement -->", Wrapper[1], 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
							}
						}
						//
						// Process Feedback form
						//
						//hint = hint & ",600, Handle webclient features"
						if (genericController.vbInstr(1, result, FeedbackFormNotSupportedComment, Microsoft.VisualBasic.Constants.vbTextCompare) != 0)
						{
							result = genericController.vbReplace(result, FeedbackFormNotSupportedComment, pageContentController.main_GetFeedbackForm(cpCore, ContextContentName, ContextRecordID, ContextContactPeopleID), 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
						}
						//'
						//' If any javascript or styles were added during encode, pick them up now
						//'
						//Copy = cpCore.doc.getNextJavascriptBodyEnd()
						//Do While Copy <> ""
						//    Call addScriptCode_body(Copy, "embedded content")
						//    Copy = cpCore.doc.getNextJavascriptBodyEnd()
						//Loop
						//'
						//' current
						//'
						//Copy = cpCore.doc.getNextJSFilename()
						//Do While Copy <> ""
						//    If genericController.vbInstr(1, Copy, "://") <> 0 Then
						//    ElseIf Left(Copy, 1) = "/" Then
						//    Else
						//        Copy = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, Copy)
						//    End If
						//    Call addScriptLink_Head(Copy, "embedded content")
						//    Copy = cpCore.doc.getNextJSFilename()
						//Loop
						//'
						//Copy = cpCore.doc.getJavascriptOnLoad()
						//Do While Copy <> ""
						//    Call addOnLoadJs(Copy, "")
						//    Copy = cpCore.doc.getJavascriptOnLoad()
						//Loop
						//
						//Copy = cpCore.doc.getNextStyleFilenames()
						//Do While Copy <> ""
						//    If genericController.vbInstr(1, Copy, "://") <> 0 Then
						//    ElseIf Left(Copy, 1) = "/" Then
						//    Else
						//        Copy = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, Copy)
						//    End If
						//    Call addStyleLink(Copy, "")
						//    Copy = cpCore.doc.getNextStyleFilenames()
						//Loop
					}
				}
				//
				result = result;
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//
		// ================================================================================================================
		//   Upgrade old objects in content, and update changed resource library images
		// ================================================================================================================
		//
		public string upgradeActiveContent(string Source)
		{
			string result = Source;
			try
			{
				string RecordVirtualPath = string.Empty;
				string RecordVirtualFilename = null;
				string RecordFilename = null;
				string RecordFilenameNoExt = null;
				string RecordFilenameExt = string.Empty;
				string[] SizeTest = null;
				string RecordAltSizeList = null;
				int TagPosEnd = 0;
				int TagPosStart = 0;
				bool InTag = false;
				int Pos = 0;
				string FilenameSegment = null;
				int EndPos1 = 0;
				int EndPos2 = 0;
				string[] LinkSplit = null;
				int LinkCnt = 0;
				int LinkPtr = 0;
				string[] TableSplit = null;
				string TableName = null;
				string FieldName = null;
				int RecordID = 0;
				bool SaveChanges = false;
				int EndPos = 0;
				int Ptr = 0;
				string FilePrefixSegment = null;
				bool ImageAllowUpdate = false;
				string ContentFilesLinkPrefix = null;
				string ResourceLibraryLinkPrefix = null;
				string TestChr = null;
				bool ParseError = false;
				result = Source;
				//
				ContentFilesLinkPrefix = "/" + cpCore.serverConfig.appConfig.name + "/files/";
				ResourceLibraryLinkPrefix = ContentFilesLinkPrefix + "ccLibraryFiles/";
				ImageAllowUpdate = cpCore.siteProperties.getBoolean("ImageAllowUpdate", true);
				ImageAllowUpdate = ImageAllowUpdate && (Source.IndexOf(ResourceLibraryLinkPrefix, System.StringComparison.OrdinalIgnoreCase) + 1 != 0);
				if (ImageAllowUpdate)
				{
					//
					// ----- Process Resource Library Images (swap in most current file)
					//
					//   There is a better way:
					//   problem with replacing the images is the problem with parsing - too much work to find it
					//   instead, use new replacement tags <ac type=image src="cclibraryfiles/filename/00001" width=0 height=0>
					//
					//'hint = hint & ",010"
					ParseError = false;
					LinkSplit = Microsoft.VisualBasic.Strings.Split(Source, ContentFilesLinkPrefix, -1, Microsoft.VisualBasic.Constants.vbTextCompare);
					LinkCnt = LinkSplit.GetUpperBound(0) + 1;
					for (LinkPtr = 1; LinkPtr < LinkCnt; LinkPtr++)
					{
						//
						// Each LinkSplit(1...) is a segment that would have started with '/appname/files/'
						// Next job is to determine if this sement is in a tag (<img src="...">) or in content (&quot...&quote)
						// For now, skip the ones in content
						//
						//'hint = hint & ",020"
						TagPosEnd = genericController.vbInstr(1, LinkSplit[LinkPtr], ">");
						TagPosStart = genericController.vbInstr(1, LinkSplit[LinkPtr], "<");
						if (TagPosEnd == 0 && TagPosStart == 0)
						{
							//
							// no tags found, skip it
							//
							InTag = false;
						}
						else if (TagPosEnd == 0)
						{
							//
							// no end tag, but found a start tag -> in content
							//
							InTag = false;
						}
						else if (TagPosEnd < TagPosStart)
						{
							//
							// Found end before start - > in tag
							//
							InTag = true;
						}
						else
						{
							//
							// Found start before end -> in content
							//
							InTag = false;
						}
						if (InTag)
						{
							//'hint = hint & ",030"
							TableSplit = LinkSplit[LinkPtr].Split('/');
							if (TableSplit.GetUpperBound(0) > 2)
							{
								TableName = TableSplit[0];
								FieldName = TableSplit[1];
								RecordID = genericController.EncodeInteger(TableSplit[2]);
								FilenameSegment = TableSplit[3];
								if ((TableName.ToLower() == "cclibraryfiles") && (FieldName.ToLower() == "filename") && (RecordID != 0))
								{
									Models.Entity.libraryFilesModel file = Models.Entity.libraryFilesModel.create(cpCore, RecordID);
									if (file != null)
									{
										//'hint = hint & ",060"
										FieldName = "filename";
										//SQL = "select filename,altsizelist from " & TableName & " where id=" & RecordID
										//CS = app.csv_OpenCSSQL("default", SQL)
										//If app.csv_IsCSOK(CS) Then
										if (true)
										{
											//
											// now figure out how the link is delimited by how it starts
											//   go to the left and look for:
											//   ' ' - ignore spaces, continue forward until we find one of these
											//   '=' - means space delimited (src=/image.jpg), ends in ' ' or '>'
											//   '"' - means quote delimited (src="/image.jpg"), ends in '"'
											//   '>' - means this is not in an HTML tag - skip it (<B>image.jpg</b>)
											//   '<' - means god knows what, but its wrong, skip it
											//   '(' - means it is a URL(/image.jpg), go to ')'
											//
											// odd cases:
											//   URL( /image.jpg) -
											//
											RecordVirtualFilename = file.Filename;
											RecordAltSizeList = file.AltSizeList;
											if (RecordVirtualFilename == genericController.EncodeJavascript(RecordVirtualFilename))
											{
												//
												// The javascript version of the filename must match the filename, since we have no way
												// of differentiating a ligitimate file, from a block of javascript. If the file
												// contains an apostrophe, the original code could have encoded it, but we can not here
												// so the best plan is to skip it
												//
												// example:
												// RecordVirtualFilename = "jay/files/cclibraryfiles/filename/000005/test.png"
												//
												// RecordFilename = "test.png"
												// RecordFilenameAltSize = "" (does not exist - the record has the raw filename in it)
												// RecordFilenameExt = "png"
												// RecordFilenameNoExt = "test"
												//
												// RecordVirtualFilename = "jay/files/cclibraryfiles/filename/000005/test-100x200.png"
												// this is a specail case - most cases to not have the alt size format saved in the filename
												// RecordFilename = "test-100x200.png"
												// RecordFilenameAltSize (does not exist - the record has the raw filename in it)
												// RecordFilenameExt = "png"
												// RecordFilenameNoExt = "test-100x200"
												// this is wrong
												//   xRecordFilenameAltSize = "100x200"
												//   xRecordFilenameExt = "png"
												//   xRecordFilenameNoExt = "test"
												//
												//'hint = hint & ",080"
												Pos = RecordVirtualFilename.LastIndexOf("/") + 1;
												RecordFilename = "";
												if (Pos > 0)
												{
													RecordVirtualPath = RecordVirtualFilename.Substring(0, Pos);
													RecordFilename = RecordVirtualFilename.Substring(Pos);
												}
												Pos = RecordFilename.LastIndexOf(".") + 1;
												RecordFilenameNoExt = "";
												if (Pos > 0)
												{
													RecordFilenameExt = genericController.vbLCase(RecordFilename.Substring(Pos));
													RecordFilenameNoExt = genericController.vbLCase(RecordFilename.Substring(0, Pos - 1));
												}
												FilePrefixSegment = LinkSplit[LinkPtr - 1];
												if (FilePrefixSegment.Length > 1)
												{
													//
													// Look into FilePrefixSegment and see if we are in the querystring attribute of an <AC tag
													//   if so, the filename may be AddonEncoded and delimited with & (so skip it)
													Pos = FilePrefixSegment.LastIndexOf("<") + 1;
													if (Pos > 0)
													{
														if (genericController.vbLCase(FilePrefixSegment.Substring(Pos, 3)) != "ac ")
														{
															//
															// look back in the FilePrefixSegment to find the character before the link
															//
															EndPos = 0;
															for (Ptr = FilePrefixSegment.Length; Ptr >= 1; Ptr--)
															{
																TestChr = FilePrefixSegment.Substring(Ptr - 1, 1);
																switch (TestChr)
																{
																	case "=":
																		//
																		// Ends in ' ' or '>', find the first
																		//
																		EndPos1 = genericController.vbInstr(1, FilenameSegment, " ");
																		EndPos2 = genericController.vbInstr(1, FilenameSegment, ">");
																		if (EndPos1 != 0 & EndPos2 != 0)
																		{
																			if (EndPos1 < EndPos2)
																			{
																				EndPos = EndPos1;
																			}
																			else
																			{
																				EndPos = EndPos2;
																			}
																		}
																		else if (EndPos1 != 0)
																		{
																			EndPos = EndPos1;
																		}
																		else if (EndPos2 != 0)
																		{
																			EndPos = EndPos2;
																		}
																		else
																		{
																			EndPos = 0;
																		}
//INSTANT C# WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
//ORIGINAL LINE: Exit For
																		goto ExitLabel1;
																	case "\"":
																		//
																		// Quoted, ends is '"'
																		//
																		EndPos = genericController.vbInstr(1, FilenameSegment, "\"");
//INSTANT C# WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
//ORIGINAL LINE: Exit For
																		goto ExitLabel1;
																	case "(":
																		//
																		// url() style, ends in ')' or a ' '
																		//
																		if (genericController.vbLCase(FilePrefixSegment.Substring(Ptr - 1, 7)) == "(&quot;")
																		{
																			EndPos = genericController.vbInstr(1, FilenameSegment, "&quot;)");
																		}
																		else if (genericController.vbLCase(FilePrefixSegment.Substring(Ptr - 1, 2)) == "('")
																		{
																			EndPos = genericController.vbInstr(1, FilenameSegment, "')");
																		}
																		else if (genericController.vbLCase(FilePrefixSegment.Substring(Ptr - 1, 2)) == "(\"")
																		{
																			EndPos = genericController.vbInstr(1, FilenameSegment, "\")");
																		}
																		else
																		{
																			EndPos = genericController.vbInstr(1, FilenameSegment, ")");
																		}
//INSTANT C# WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
//ORIGINAL LINE: Exit For
																		goto ExitLabel1;
																	case "'":
																		//
																		// Delimited within a javascript pair of apostophys
																		//
																		EndPos = genericController.vbInstr(1, FilenameSegment, "'");
//INSTANT C# WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
//ORIGINAL LINE: Exit For
																		goto ExitLabel1;
																	case ">":
																	case "<":
																		//
																		// Skip this link
																		//
																		ParseError = true;
//INSTANT C# WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
//ORIGINAL LINE: Exit For
																		goto ExitLabel1;
																}
															}
															ExitLabel1:
															//
															// check link
															//
															if (EndPos == 0)
															{
																ParseError = true;
																break;
															}
															else
															{
																string ImageFilename = null;
																string SegmentAfterImage = null;

																string ImageFilenameNoExt = null;
																string ImageFilenameExt = null;
																string ImageAltSize = null;

																//'hint = hint & ",120"
																SegmentAfterImage = FilenameSegment.Substring(EndPos - 1);
																ImageFilename = genericController.DecodeResponseVariable(FilenameSegment.Substring(0, EndPos - 1));
																ImageFilenameNoExt = ImageFilename;
																ImageFilenameExt = "";
																Pos = ImageFilename.LastIndexOf(".") + 1;
																if (Pos > 0)
																{
																	ImageFilenameNoExt = genericController.vbLCase(ImageFilename.Substring(0, Pos - 1));
																	ImageFilenameExt = genericController.vbLCase(ImageFilename.Substring(Pos));
																}
																//
																// Get ImageAltSize
																//
																//'hint = hint & ",130"
																ImageAltSize = "";
																if (ImageFilenameNoExt == RecordFilenameNoExt)
																{
																	//
																	// Exact match
																	//
																}
																else if (genericController.vbInstr(1, ImageFilenameNoExt, RecordFilenameNoExt, Microsoft.VisualBasic.Constants.vbTextCompare) != 1)
																{
																	//
																	// There was a change and the recordfilename is not part of the imagefilename
																	//
																}
																else
																{
																	//
																	// the recordfilename is the first part of the imagefilename - Get ImageAltSize
																	//
																	ImageAltSize = ImageFilenameNoExt.Substring(RecordFilenameNoExt.Length);
																	if (ImageAltSize.Substring(0, 1) != "-")
																	{
																		ImageAltSize = "";
																	}
																	else
																	{
																		ImageAltSize = ImageAltSize.Substring(1);
																		SizeTest = ImageAltSize.Split('x');
																		if (SizeTest.GetUpperBound(0) != 1)
																		{
																			ImageAltSize = "";
																		}
																		else
																		{
																			if (genericController.vbIsNumeric(SizeTest[0]) & genericController.vbIsNumeric(SizeTest[1]))
																			{
																				ImageFilenameNoExt = RecordFilenameNoExt;
																				//ImageFilenameNoExt = Mid(ImageFilenameNoExt, 1, Pos - 1)
																				//RecordFilenameNoExt = Mid(RecordFilename, 1, Pos - 1)
																			}
																			else
																			{
																				ImageAltSize = "";
																			}
																		}
																	}
																}
																//
																// problem - in the case where the recordfilename = img-100x200, the imagefilenamenoext is img
																//
																//'hint = hint & ",140"
																if ((RecordFilenameNoExt != ImageFilenameNoExt) | (RecordFilenameExt != ImageFilenameExt))
																{
																	//
																	// There has been a change
																	//
																	string NewRecordFilename = null;
																	int ImageHeight = 0;
																	int ImageWidth = 0;
																	NewRecordFilename = RecordVirtualPath + RecordFilenameNoExt + "." + RecordFilenameExt;
																	//
																	// realtime image updates replace without creating new size - that is for the edit interface
																	//
																	// put the New file back into the tablesplit in case there are more then 4 splits
																	//
																	TableSplit[0] = "";
																	TableSplit[1] = "";
																	TableSplit[2] = "";
																	TableSplit[3] = SegmentAfterImage;
																	NewRecordFilename = genericController.EncodeURL(NewRecordFilename) + ((string)(string.Join("/", TableSplit))).Substring(3);
																	LinkSplit[LinkPtr] = NewRecordFilename;
																	SaveChanges = true;
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
						}
						if (ParseError)
						{
							break;
						}
					}
					//'hint = hint & ",910"
					if (SaveChanges && (!ParseError))
					{
						result = string.Join(ContentFilesLinkPrefix, LinkSplit);
					}
				}
				//'hint = hint & ",920"
				if (!ParseError)
				{
					//
					// Convert ACTypeDynamicForm to Add-on
					//
					if (genericController.vbInstr(1, result, "<ac type=\"" + ACTypeDynamicForm, Microsoft.VisualBasic.Constants.vbTextCompare) != 0)
					{
						result = genericController.vbReplace(result, "type=\"DYNAMICFORM\"", "TYPE=\"aggregatefunction\"", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
						result = genericController.vbReplace(result, "name=\"DYNAMICFORM\"", "name=\"DYNAMIC FORM\"", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
					}
				}
				//'hint = hint & ",930"
				if (ParseError)
				{
					result = ""
					+ Environment.NewLine + "<!-- warning: parsing aborted on ccLibraryFile replacement -->"
					+ Environment.NewLine + result + Environment.NewLine + "<!-- /warning: parsing aborted on ccLibraryFile replacement -->";
				}
				//
				// {{content}} should be <ac type="templatecontent" etc>
				// the merge is now handled in csv_EncodeActiveContent, but some sites have hand {{content}} tags entered
				//
				//'hint = hint & ",940"
				if (genericController.vbInstr(1, result, "{{content}}", Microsoft.VisualBasic.Constants.vbTextCompare) != 0)
				{
					result = genericController.vbReplace(result, "{{content}}", "<AC type=\"" + ACTypeTemplateContent + "\">", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//
		//============================================================================
		//   csv_GetContentCopy3
		//       To get them, cp.content.getCopy must call the cpCoreClass version, which calls this for the content
		//============================================================================
		//
		public string html_GetContentCopy(string CopyName, string DefaultContent, int personalizationPeopleId, bool AllowEditWrapper, bool personalizationIsAuthenticated)
		{
			string returnCopy = "";
			try
			{
				//
				int CS = 0;
				int RecordID = 0;
				int contactPeopleId = 0;
				string Return_ErrorMessage = "";
				//
				// honestly, not sure what to do with 'return_ErrorMessage'
				//
				CS = cpCore.db.csOpen("copy content", "Name=" + cpCore.db.encodeSQLText(CopyName), "ID",, 0,,, "Name,ID,Copy,modifiedBy");
				if (!cpCore.db.csOk(CS))
				{
					cpCore.db.csClose(CS);
					CS = cpCore.db.csInsertRecord("copy content", 0);
					if (cpCore.db.csOk(CS))
					{
						RecordID = cpCore.db.csGetInteger(CS, "ID");
						cpCore.db.csSet(CS, "name", CopyName);
						cpCore.db.csSet(CS, "copy", genericController.encodeText(DefaultContent));
						cpCore.db.csSave2(CS);
						//   Call cpCore.workflow.publishEdit("copy content", RecordID)
					}
				}
				if (cpCore.db.csOk(CS))
				{
					RecordID = cpCore.db.csGetInteger(CS, "ID");
					contactPeopleId = cpCore.db.csGetInteger(CS, "modifiedBy");
					returnCopy = cpCore.db.csGet(CS, "Copy");
					returnCopy = executeContentCommands(null, returnCopy, CPUtilsBaseClass.addonContext.ContextPage, personalizationPeopleId, personalizationIsAuthenticated, Return_ErrorMessage);
					returnCopy = convertActiveContentToHtmlForWebRender(returnCopy, "copy content", RecordID, personalizationPeopleId, "", 0, CPUtilsBaseClass.addonContext.ContextPage);
					//returnCopy = convertActiveContent_internal(returnCopy, personalizationPeopleId, "copy content", RecordID, contactPeopleId, False, False, True, True, False, True, "", "", False, 0, "", CPUtilsBaseClass.addonContext.ContextPage, False, Nothing, False)
					//
					if (true)
					{
						if (cpCore.doc.authContext.isEditingAnything())
						{
							returnCopy = cpCore.db.csGetRecordEditLink(CS, false) + returnCopy;
							if (AllowEditWrapper)
							{
								returnCopy = getEditWrapper("copy content", returnCopy);
							}
						}
					}
				}
				cpCore.db.csClose(CS);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return returnCopy;
		}
		//
		//
		//
		public void main_AddTabEntry(string Caption, string Link, bool IsHit, string StylePrefix = "", string LiveBody = "")
		{
			try
			{
				//
				// should use the ccNav object, no the ccCommon module for this code
				//
				cpCore.menuTab.AddEntry(genericController.encodeText(Caption), genericController.encodeText(Link), genericController.EncodeBoolean(IsHit), genericController.encodeText(StylePrefix));

				//Call ccAddTabEntry(genericController.encodeText(Caption), genericController.encodeText(Link), genericController.EncodeBoolean(IsHit), genericController.encodeText(StylePrefix), genericController.encodeText(LiveBody))
				//
				return;
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_AddTabEntry")
		}
		//        '
		//        '
		//        '
		//        Public Function main_GetTabs() As String
		//            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetTabs")
		//            '
		//            ' should use the ccNav object, no the ccCommon module for this code
		//            '
		//            '
		//            main_GetTabs = menuTab.GetTabs()
		//            '    main_GetTabs = ccGetTabs()
		//            '
		//            Exit Function
		//ErrorTrap:
		//            throw new applicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetTabs")
		//        End Function
		//
		//
		//
		public void main_AddLiveTabEntry(string Caption, string LiveBody, string StylePrefix = "")
		{
			try
			{
				//
				// should use the ccNav object, no the ccCommon module for this code
				//
				if (cpCore.doc.menuLiveTab == null)
				{
					cpCore.doc.menuLiveTab = new menuLiveTabController();
				}
				cpCore.doc.menuLiveTab.AddEntry(genericController.encodeText(Caption), genericController.encodeText(LiveBody), genericController.encodeText(StylePrefix));
				//
				return;
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_AddLiveTabEntry")
		}
		//
		//
		//
		public string main_GetLiveTabs()
		{
			try
			{
				//
				// should use the ccNav object, no the ccCommon module for this code
				//
				if (cpCore.doc.menuLiveTab == null)
				{
					cpCore.doc.menuLiveTab = new menuLiveTabController();
				}
				return cpCore.doc.menuLiveTab.GetTabs();
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_GetLiveTabs")
		}
		//
		//
		//
		public void menu_AddComboTabEntry(string Caption, string Link, string AjaxLink, string LiveBody, bool IsHit, string ContainerClass)
		{
			try
			{
				//
				// should use the ccNav object, no the ccCommon module for this code
				//
				if (cpCore.doc.menuComboTab == null)
				{
					cpCore.doc.menuComboTab = new menuComboTabController();
				}
				cpCore.doc.menuComboTab.AddEntry(Caption, Link, AjaxLink, LiveBody, IsHit, ContainerClass);
				//
				return;
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_AddComboTabEntry")
		}
		//
		//
		//
		public string menu_GetComboTabs()
		{
			try
			{
				//
				// should use the ccNav object, no the ccCommon module for this code
				//
				if (cpCore.doc.menuComboTab == null)
				{
					cpCore.doc.menuComboTab = new menuComboTabController();
				}
				return cpCore.doc.menuComboTab.GetTabs();
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_GetComboTabs")
		}
		//'
		//'================================================================================================================
		//'   main_Get SharedStyleFilelist
		//'
		//'   SharedStyleFilelist is a list of filenames (with conditional comments) that should be included on pages
		//'   that call out the SharedFileIDList
		//'
		//'   Suffix and Prefix are for Conditional Comments around the style tag
		//'
		//'   SharedStyleFileList is
		//'       crlf filename < Prefix< Suffix
		//'       crlf filename < Prefix< Suffix
		//'       ...
		//'       Prefix and Suffix are htmlencoded
		//'
		//'   SharedStyleMap file
		//'       crlf StyleID tab StyleFilename < Prefix < Suffix, IncludedStyleFilename < Prefix < Suffix, ...
		//'       crlf StyleID tab StyleFilename < Prefix < Suffix, IncludedStyleFilename < Prefix < Suffix, ...
		//'       ...
		//'       StyleID is 0 if Always include is set
		//'       The Prefix and Suffix have had crlf removed, and comma replaced with &#44;
		//'================================================================================================================
		//'
		//Friend Shared Function main_GetSharedStyleFileList(cpCore As coreClass, SharedStyleIDList As String, main_IsAdminSite As Boolean) As String
		//    Dim result As String = ""
		//    '
		//    Dim Prefix As String
		//    Dim Suffix As String
		//    Dim Files() As String
		//    Dim Pos As Integer
		//    Dim SrcID As Integer
		//    Dim Srcs() As String
		//    Dim SrcCnt As Integer
		//    Dim IncludedStyleFilename As String
		//    Dim styleId As Integer
		//    Dim LastStyleID As Integer
		//    Dim CS As Integer
		//    Dim Ptr As Integer
		//    Dim MapList As String
		//    Dim Map() As String
		//    Dim MapCnt As Integer
		//    Dim MapRow As Integer
		//    Dim Filename As String
		//    Dim FileList As String
		//    Dim SQL As String = String.Empty
		//    Dim BakeName As String
		//    '
		//    If main_IsAdminSite Then
		//        BakeName = "SharedStyleMap-Admin"
		//    Else
		//        BakeName = "SharedStyleMap-Public"
		//    End If
		//    MapList = genericController.encodeText(cpCore.cache.getObject(Of String)(BakeName))
		//    If MapList = "" Then
		//        '
		//        ' BuildMap
		//        '
		//        MapList = ""
		//        If True Then
		//            '
		//            ' add prefix and suffix conditional comments
		//            '
		//            SQL = "select s.ID,s.Stylefilename,s.Prefix,s.Suffix,i.StyleFilename as iStylefilename,s.AlwaysInclude,i.Prefix as iPrefix,i.Suffix as iSuffix" _
		//                & " from ((ccSharedStyles s" _
		//                & " left join ccSharedStylesIncludeRules r on r.StyleID=s.id)" _
		//                & " left join ccSharedStyles i on i.id=r.IncludedStyleID)" _
		//                & " where ( s.active<>0 )and((i.active is null)or(i.active<>0))"
		//        End If
		//        CS = cpCore.db.cs_openSql(SQL)
		//        LastStyleID = 0
		//        Do While cpCore.db.cs_ok(CS)
		//            styleId = cpCore.db.cs_getInteger(CS, "ID")
		//            If styleId <> LastStyleID Then
		//                Filename = cpCore.db.cs_get(CS, "StyleFilename")
		//                Prefix = genericController.vbReplace(cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "Prefix")), ",", "&#44;")
		//                Suffix = genericController.vbReplace(cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "Suffix")), ",", "&#44;")
		//                If (Not main_IsAdminSite) And cpCore.db.cs_getBoolean(CS, "alwaysinclude") Then
		//                    MapList = MapList & vbCrLf & "0" & vbTab & Filename & "<" & Prefix & "<" & Suffix
		//                Else
		//                    MapList = MapList & vbCrLf & styleId & vbTab & Filename & "<" & Prefix & "<" & Suffix
		//                End If
		//            End If
		//            IncludedStyleFilename = cpCore.db.cs_getText(CS, "iStylefilename")
		//            Prefix = cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "iPrefix"))
		//            Suffix = cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "iSuffix"))
		//            If IncludedStyleFilename <> "" Then
		//                MapList = MapList & "," & IncludedStyleFilename & "<" & Prefix & "<" & Suffix
		//            End If
		//            Call cpCore.db.cs_goNext(CS)
		//        Loop
		//        If MapList = "" Then
		//            MapList = ","
		//        End If
		//        Call cpCore.cache.setObject(BakeName, MapList, "Shared Styles")
		//    End If
		//    If (MapList <> "") And (MapList <> ",") Then
		//        Srcs = Split(SharedStyleIDList, ",")
		//        SrcCnt = UBound(Srcs) + 1
		//        Map = Split(MapList, vbCrLf)
		//        MapCnt = UBound(Map) + 1
		//        '
		//        ' Add stylesheets with AlwaysInclude set (ID is saved as 0 in Map)
		//        '
		//        FileList = ""
		//        For MapRow = 0 To MapCnt - 1
		//            If genericController.vbInstr(1, Map(MapRow), "0" & vbTab) = 1 Then
		//                Pos = genericController.vbInstr(1, Map(MapRow), vbTab)
		//                If Pos > 0 Then
		//                    FileList = FileList & "," & Mid(Map(MapRow), Pos + 1)
		//                End If
		//            End If
		//        Next
		//        '
		//        ' create a filelist of everything that is needed, might be duplicates
		//        '
		//        For Ptr = 0 To SrcCnt - 1
		//            SrcID = genericController.EncodeInteger(Srcs(Ptr))
		//            If SrcID <> 0 Then
		//                For MapRow = 0 To MapCnt - 1
		//                    If genericController.vbInstr(1, Map(MapRow), SrcID & vbTab) <> 0 Then
		//                        Pos = genericController.vbInstr(1, Map(MapRow), vbTab)
		//                        If Pos > 0 Then
		//                            FileList = FileList & "," & Mid(Map(MapRow), Pos + 1)
		//                        End If
		//                    End If
		//                Next
		//            End If
		//        Next
		//        '
		//        ' dedup the filelist and convert it to crlf delimited
		//        '
		//        If FileList <> "" Then
		//            Files = Split(FileList, ",")
		//            For Ptr = 0 To UBound(Files)
		//                Filename = Files(Ptr)
		//                If genericController.vbInstr(1, result, Filename, vbTextCompare) = 0 Then
		//                    result = result & vbCrLf & Filename
		//                End If
		//            Next
		//        End If
		//    End If
		//    Return result
		//End Function

		//
		//
		//
		public string main_GetResourceLibrary2(string RootFolderName, bool AllowSelectResource, string SelectResourceEditorName, string SelectLinkObjectName, bool AllowGroupAdd)
		{
			string addonGuidResourceLibrary = "{564EF3F5-9673-4212-A692-0942DD51FF1A}";
			Dictionary<string, string> arguments = new Dictionary<string, string>();
			arguments.Add("RootFolderName", RootFolderName);
			arguments.Add("AllowSelectResource", AllowSelectResource.ToString());
			arguments.Add("SelectResourceEditorName", SelectResourceEditorName);
			arguments.Add("SelectLinkObjectName", SelectLinkObjectName);
			arguments.Add("AllowGroupAdd", AllowGroupAdd.ToString());
			return cpCore.addon.execute(addonModel.create(cpCore, addonGuidResourceLibrary), new CPUtilsBaseClass.addonExecuteContext()
			{
				addonType = CPUtilsBaseClass.addonContext.ContextAdmin,
				instanceArguments = arguments
			});
			//Dim Option_String As String
			//Option_String = "" _
			//    & "RootFolderName=" & RootFolderName _
			//    & "&AllowSelectResource=" & AllowSelectResource _
			//    & "&SelectResourceEditorName=" & SelectResourceEditorName _
			//    & "&SelectLinkObjectName=" & SelectLinkObjectName _
			//    & "&AllowGroupAdd=" & AllowGroupAdd _
			//    & ""

			//Return cpCore.addon.execute_legacy4(addonGuidResourceLibrary, Option_String, CPUtilsBaseClass.addonContext.ContextAdmin)
		}
		//
		//========================================================================
		// Read and save a main_GetFormInputCheckList
		//   see main_GetFormInputCheckList for an explaination of the input
		//========================================================================
		//
		public void main_ProcessCheckList(string TagName, string PrimaryContentName, string PrimaryRecordID, string SecondaryContentName, string RulesContentName, string RulesPrimaryFieldname, string RulesSecondaryFieldName)
		{
			//
			string rulesTablename = null;
			string SQL = null;
			DataTable currentRules = null;
			int currentRulesCnt = 0;
			bool RuleFound = false;
			int RuleId = 0;
			int Ptr = 0;
			int TestRecordIDLast = 0;
			int TestRecordID = 0;
			string dupRuleIdList = null;
			int GroupCnt = 0;
			int GroupPtr = 0;
			string MethodName = null;
			int SecondaryRecordID = 0;
			bool RuleNeeded = false;
			int CSRule = 0;
			bool RuleContentChanged = false;
			bool SupportRuleCopy = false;
			string RuleCopy = null;
			//
			MethodName = "ProcessCheckList";
			//
			// --- create Rule records for all selected
			//
			GroupCnt = cpCore.docProperties.getInteger(TagName + ".RowCount");
			if (GroupCnt > 0)
			{
				//
				// Test if RuleCopy is supported
				//
				SupportRuleCopy = Models.Complex.cdefModel.isContentFieldSupported(cpCore, RulesContentName, "RuleCopy");
				if (SupportRuleCopy)
				{
					SupportRuleCopy = SupportRuleCopy && Models.Complex.cdefModel.isContentFieldSupported(cpCore, SecondaryContentName, "AllowRuleCopy");
					if (SupportRuleCopy)
					{
						SupportRuleCopy = SupportRuleCopy && Models.Complex.cdefModel.isContentFieldSupported(cpCore, SecondaryContentName, "RuleCopyCaption");
					}
				}
				//
				// Go through each checkbox and check for a rule
				//
				//
				// try
				//
				currentRulesCnt = 0;
				dupRuleIdList = "";
				rulesTablename = Models.Complex.cdefModel.getContentTablename(cpCore, RulesContentName);
				SQL = "select " + RulesSecondaryFieldName + ",id from " + rulesTablename + " where (" + RulesPrimaryFieldname + "=" + PrimaryRecordID + ")and(active<>0) order by " + RulesSecondaryFieldName;
				currentRulesCnt = 0;
				currentRules = cpCore.db.executeQuery(SQL);
				currentRulesCnt = currentRules.Rows.Count;
				for (GroupPtr = 0; GroupPtr < GroupCnt; GroupPtr++)
				{
					//
					// ----- Read Response
					//
					SecondaryRecordID = cpCore.docProperties.getInteger(TagName + "." + GroupPtr + ".ID");
					RuleCopy = cpCore.docProperties.getText(TagName + "." + GroupPtr + ".RuleCopy");
					RuleNeeded = cpCore.docProperties.getBoolean(TagName + "." + GroupPtr);
					//
					// ----- Update Record
					//
					RuleFound = false;
					RuleId = 0;
					TestRecordIDLast = 0;
					for (Ptr = 0; Ptr < currentRulesCnt; Ptr++)
					{
						TestRecordID = genericController.EncodeInteger(currentRules.Rows(Ptr).Item(0));
						if (TestRecordID == 0)
						{
							//
							// skip
							//
						}
						else if (TestRecordID == SecondaryRecordID)
						{
							//
							// hit
							//
							RuleFound = true;
							RuleId = genericController.EncodeInteger(currentRules.Rows(Ptr).Item(1));
							break;
						}
						else if (TestRecordID == TestRecordIDLast)
						{
							//
							// dup
							//
							dupRuleIdList = dupRuleIdList + "," + genericController.EncodeInteger(currentRules.Rows(Ptr).Item(1));
							currentRules.Rows(Ptr).Item(0) = 0;
						}
						TestRecordIDLast = TestRecordID;
					}
					if (SupportRuleCopy && RuleNeeded && (RuleFound))
					{
						//
						// Record exists and is needed, update the rule copy
						//
						SQL = "update " + rulesTablename + " set rulecopy=" + cpCore.db.encodeSQLText(RuleCopy) + " where id=" + RuleId;
						cpCore.db.executeQuery(SQL);
					}
					else if (RuleNeeded && (!RuleFound))
					{
						//
						// No record exists, and one is needed
						//
						CSRule = cpCore.db.csInsertRecord(RulesContentName);
						if (cpCore.db.csOk(CSRule))
						{
							cpCore.db.csSet(CSRule, "Active", RuleNeeded);
							cpCore.db.csSet(CSRule, RulesPrimaryFieldname, PrimaryRecordID);
							cpCore.db.csSet(CSRule, RulesSecondaryFieldName, SecondaryRecordID);
							if (SupportRuleCopy)
							{
								cpCore.db.csSet(CSRule, "RuleCopy", RuleCopy);
							}
						}
						cpCore.db.csClose(CSRule);
						RuleContentChanged = true;
					}
					else if ((!RuleNeeded) && RuleFound)
					{
						//
						// Record exists and it is not needed
						//
						SQL = "delete from " + rulesTablename + " where id=" + RuleId;
						cpCore.db.executeQuery(SQL);
						RuleContentChanged = true;
					}
				}
				//
				// delete dups
				//
				if (!string.IsNullOrEmpty(dupRuleIdList))
				{
					SQL = "delete from " + rulesTablename + " where id in (" + dupRuleIdList.Substring(1) + ")";
					cpCore.db.executeQuery(SQL);
					RuleContentChanged = true;
				}
			}
			if (RuleContentChanged)
			{
				cpCore.cache.invalidateAllObjectsInContent(RulesContentName);
			}
		}

		//'
		//'========================================================================
		//' ----- Ends an HTML page
		//'========================================================================
		//'
		//Public Function getHtmlDoc_afterBodyHtml() As String
		//    Return "" _
		//        & cr & "</body>" _
		//        & vbCrLf & "</html>"
		//End Function
		//
		//========================================================================
		// main_GetRecordEditLink( iContentName, iRecordID )
		//
		//   iContentName The content for this link
		//   iRecordID    The ID of the record in the Table
		//========================================================================
		//
		public string main_GetRecordEditLink(string ContentName, int RecordID, bool AllowCut = false)
		{
			return main_GetRecordEditLink2(ContentName, RecordID, genericController.EncodeBoolean(AllowCut), "", cpCore.doc.authContext.isEditing(ContentName));
		}
	}
}
