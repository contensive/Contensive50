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
		//
		public string main_GetEditorAddonListJSON(csv_contentTypeEnum ContentType)
		{
			string result = string.Empty;
			try
			{
				string AddonName = null;
				string LastAddonName = string.Empty;
				int CSAddons = 0;
				string DefaultAddonOption_String = null;
				bool UseAjaxDefaultAddonOptions = false;
				int PtrTest = 0;
				string s = null;
				int IconWidth = 0;
				int IconHeight = 0;
				int IconSprites = 0;
				bool IsInline = false;
				string AddonGuid = null;
				string IconIDControlString = null;
				string IconImg = null;
				string AddonContentName = null;
				string ObjectProgramID2 = null;
				int LoopPtr = 0;
				string FieldCaption = null;
				string SelectList = null;
				string IconFilename = null;
				string FieldName = null;
				string ArgumentList = null;
				keyPtrController Index = null;
				string[] Items = null;
				int ItemsSize = 0;
				int ItemsCnt = 0;
				int ItemsPtr = 0;
				string Criteria = null;
				int CSLists = 0;
				string FieldList = null;
				string cacheKey;
				//
				// can not save this because there are multiple main_versions
				//
				cacheKey = "editorAddonList:" + ContentType;
				result = cpCore.docProperties.getText(cacheKey);
				if (string.IsNullOrEmpty(result))
				{
					//
					// ----- AC Tags, Would like to replace these with Add-ons eventually
					//
					ItemsSize = 100;
					Items = new string[101];
					ItemsCnt = 0;
					Index = new keyPtrController();
					//Set main_cmc = main_cs_getv()
					//
					// AC StartBlockText
					//
					IconIDControlString = "AC," + ACTypeAggregateFunction + ",0,Block Text,";
					IconImg = genericController.GetAddonIconImg("/" + cpCore.serverConfig.appConfig.adminRoute, 0, 0, 0, true, IconIDControlString, "", cpCore.serverConfig.appConfig.cdnFilesNetprefix, "Text Block Start", "Block text to all except selected groups starting at this point", "", 0);
					IconImg = genericController.EncodeJavascript(IconImg);
					Items[ItemsCnt] = "['Block Text','" + IconImg + "']";
					Index.setPtr("Block Text", ItemsCnt);
					ItemsCnt = ItemsCnt + 1;
					//
					// AC EndBlockText
					//
					IconIDControlString = "AC," + ACTypeAggregateFunction + ",0,Block Text End,";
					IconImg = genericController.GetAddonIconImg("/" + cpCore.serverConfig.appConfig.adminRoute, 0, 0, 0, true, IconIDControlString, "", cpCore.serverConfig.appConfig.cdnFilesNetprefix, "Text Block End", "End of text block", "", 0);
					IconImg = genericController.EncodeJavascript(IconImg);
					Items[ItemsCnt] = "['Block Text End','" + IconImg + "']";
					Index.setPtr("Block Text", ItemsCnt);
					ItemsCnt = ItemsCnt + 1;
					//
					if ((ContentType == csv_contentTypeEnum.contentTypeEmail) || (ContentType == csv_contentTypeEnum.contentTypeEmailTemplate))
					{
						//
						// ----- Email Only AC tags
						//
						// Editing Email Body or Templates - Since Email can not process Add-ons, it main_Gets the legacy AC tags for now
						//
						// Personalization Tag
						//
						FieldList = Models.Complex.cdefModel.GetContentProperty(cpCore, "people", "SelectFieldList");
						FieldList = genericController.vbReplace(FieldList, ",", "|");
						IconIDControlString = "AC,PERSONALIZATION,0,Personalization,field=[" + FieldList + "]";
						IconImg = genericController.GetAddonIconImg("/" + cpCore.serverConfig.appConfig.adminRoute, 0, 0, 0, true, IconIDControlString, "", cpCore.serverConfig.appConfig.cdnFilesNetprefix, "Any Personalization Field", "Renders as any Personalization Field", "", 0);
						IconImg = genericController.EncodeJavascript(IconImg);
						Items[ItemsCnt] = "['Personalization','" + IconImg + "']";
						Index.setPtr("Personalization", ItemsCnt);
						ItemsCnt = ItemsCnt + 1;
						//
						if (ContentType == csv_contentTypeEnum.contentTypeEmailTemplate)
						{
							//
							// Editing Email Templates
							//   This is a special case
							//   Email content processing can not process add-ons, and PageContentBox and TextBox are needed
							//   So I added the old AC Tag into the menu for this case
							//   Need a more consistant solution later
							//
							IconIDControlString = "AC," + ACTypeTemplateContent + ",0,Template Content,";
							IconImg = genericController.GetAddonIconImg("/" + cpCore.serverConfig.appConfig.adminRoute, 52, 64, 0, false, IconIDControlString, "/ccLib/images/ACTemplateContentIcon.gif", cpCore.serverConfig.appConfig.cdnFilesNetprefix, "Content Box", "Renders as the content for a template", "", 0);
							IconImg = genericController.EncodeJavascript(IconImg);
							Items[ItemsCnt] = "['Content Box','" + IconImg + "']";
							//Items(ItemsCnt) = "['Template Content','<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Content"" id=""AC," & ACTypeTemplateContent & ",0,Template Content,"" src=""/ccLib/images/ACTemplateContentIcon.gif"" WIDTH=52 HEIGHT=64>']"
							Index.setPtr("Content Box", ItemsCnt);
							ItemsCnt = ItemsCnt + 1;
							//
							IconIDControlString = "AC," + ACTypeTemplateText + ",0,Template Text,Name=Default";
							IconImg = genericController.GetAddonIconImg("/" + cpCore.serverConfig.appConfig.adminRoute, 52, 52, 0, false, IconIDControlString, "/ccLib/images/ACTemplateTextIcon.gif", cpCore.serverConfig.appConfig.cdnFilesNetprefix, "Template Text", "Renders as a template text block", "", 0);
							IconImg = genericController.EncodeJavascript(IconImg);
							Items[ItemsCnt] = "['Template Text','" + IconImg + "']";
							//Items(ItemsCnt) = "['Template Text','<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Text"" id=""AC," & ACTypeTemplateText & ",0,Template Text,Name=Default"" src=""/ccLib/images/ACTemplateTextIcon.gif"" WIDTH=52 HEIGHT=52>']"
							Index.setPtr("Template Text", ItemsCnt);
							ItemsCnt = ItemsCnt + 1;
						}
					}
					else
					{
						//
						// ----- Web Only AC Tags
						//
						// Watch Lists
						//
						CSLists = cpCore.db.csOpen("Content Watch Lists",, "Name,ID",,,,, "Name,ID", 20, 1);
						if (cpCore.db.csOk(CSLists))
						{
							while (cpCore.db.csOk(CSLists))
							{
								FieldName = Convert.ToString(cpCore.db.csGetText(CSLists, "name")).Trim(' ');
								if (!string.IsNullOrEmpty(FieldName))
								{
									FieldCaption = "Watch List [" + FieldName + "]";
									IconIDControlString = "AC,WATCHLIST,0," + FieldName + ",ListName=" + FieldName + "&SortField=[DateAdded|Link|LinkLabel|Clicks|WhatsNewDateExpires]&SortDirection=Z-A[A-Z|Z-A]";
									IconImg = genericController.GetAddonIconImg("/" + cpCore.serverConfig.appConfig.adminRoute, 0, 0, 0, true, IconIDControlString, "", cpCore.serverConfig.appConfig.cdnFilesNetprefix, FieldCaption, "Rendered as the " + FieldCaption, "", 0);
									IconImg = genericController.EncodeJavascript(IconImg);
									FieldCaption = genericController.EncodeJavascript(FieldCaption);
									Items[ItemsCnt] = "['" + FieldCaption + "','" + IconImg + "']";
									//Items(ItemsCnt) = "['" & FieldCaption & "','<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the " & FieldCaption & """ id=""AC,WATCHLIST,0," & FieldName & ",ListName=" & FieldName & "&SortField=[DateAdded|Link|LinkLabel|Clicks|WhatsNewDateExpires]&SortDirection=Z-A[A-Z|Z-A]"" src=""/ccLib/images/ACWatchList.GIF"">']"
									Index.setPtr(FieldCaption, ItemsCnt);
									ItemsCnt = ItemsCnt + 1;
									if (ItemsCnt >= ItemsSize)
									{
										ItemsSize = ItemsSize + 100;
										Array.Resize(ref Items, ItemsSize + 1);
									}
								}
								cpCore.db.csGoNext(CSLists);
							}
						}
						cpCore.db.csClose(CSLists);
					}
					//
					// ----- Add-ons (AC Aggregate Functions)
					//
					if ((false) && (ContentType == csv_contentTypeEnum.contentTypeEmail))
					{
						//
						// Email did not support add-ons
						//
					}
					else
					{
						//
						// Either non-email or > 4.0.325
						//
						Criteria = "(1=1)";
						if (ContentType == csv_contentTypeEnum.contentTypeEmail)
						{
							//
							// select only addons with email placement (dont need to check main_version bc if email, must be >4.0.325
							//
							Criteria = Criteria + "and(email<>0)";
						}
						else
						{
							if (true)
							{
								if (ContentType == csv_contentTypeEnum.contentTypeWeb)
								{
									//
									// Non Templates
									//
									Criteria = Criteria + "and(content<>0)";
								}
								else
								{
									//
									// Templates
									//
									Criteria = Criteria + "and(template<>0)";
								}
							}
						}
						AddonContentName = cnAddons;
						SelectList = "Name,Link,ID,ArgumentList,ObjectProgramID,IconFilename,IconWidth,IconHeight,IconSprites,IsInline,ccguid";
						CSAddons = cpCore.db.csOpen(AddonContentName, Criteria, "Name,ID",,,,, SelectList);
						if (cpCore.db.csOk(CSAddons))
						{
							while (cpCore.db.csOk(CSAddons))
							{
								AddonGuid = cpCore.db.csGetText(CSAddons, "ccguid");
								ObjectProgramID2 = cpCore.db.csGetText(CSAddons, "ObjectProgramID");
								if ((ContentType == csv_contentTypeEnum.contentTypeEmail) && (!string.IsNullOrEmpty(ObjectProgramID2)))
								{
									//
									// Block activex addons from email
									//
									ObjectProgramID2 = ObjectProgramID2;
								}
								else
								{
									AddonName = Convert.ToString(cpCore.db.csGet(CSAddons, "name")).Trim(' ');
									if (!string.IsNullOrEmpty(AddonName) & (AddonName != LastAddonName))
									{
										//
										// Icon (fieldtyperesourcelink)
										//
										IsInline = cpCore.db.csGetBoolean(CSAddons, "IsInline");
										IconFilename = cpCore.db.csGet(CSAddons, "Iconfilename");
										if (string.IsNullOrEmpty(IconFilename))
										{
											IconWidth = 0;
											IconHeight = 0;
											IconSprites = 0;
										}
										else
										{
											IconWidth = cpCore.db.csGetInteger(CSAddons, "IconWidth");
											IconHeight = cpCore.db.csGetInteger(CSAddons, "IconHeight");
											IconSprites = cpCore.db.csGetInteger(CSAddons, "IconSprites");
										}
										//
										// Calculate DefaultAddonOption_String
										//
										UseAjaxDefaultAddonOptions = true;
										if (UseAjaxDefaultAddonOptions)
										{
											DefaultAddonOption_String = "";
										}
										else
										{
											ArgumentList = Convert.ToString(cpCore.db.csGet(CSAddons, "ArgumentList")).Trim(' ');
											DefaultAddonOption_String = addonController.main_GetDefaultAddonOption_String(cpCore, ArgumentList, AddonGuid, IsInline);
											DefaultAddonOption_String = main_encodeHTML(DefaultAddonOption_String);
										}
										//
										// Changes necessary to support commas in AddonName and OptionString
										//   Remove commas in Field Name
										//   Then in Javascript, when spliting on comma, anything past position 4, put back onto 4
										//
										LastAddonName = AddonName;
										IconIDControlString = "AC,AGGREGATEFUNCTION,0," + AddonName + "," + DefaultAddonOption_String + "," + AddonGuid;
										IconImg = genericController.GetAddonIconImg("/" + cpCore.serverConfig.appConfig.adminRoute, IconWidth, IconHeight, IconSprites, IsInline, IconIDControlString, IconFilename, cpCore.serverConfig.appConfig.cdnFilesNetprefix, AddonName, "Rendered as the Add-on [" + AddonName + "]", "", 0);
										Items[ItemsCnt] = "['" + genericController.EncodeJavascript(AddonName) + "','" + genericController.EncodeJavascript(IconImg) + "']";
										Index.setPtr(AddonName, ItemsCnt);
										ItemsCnt = ItemsCnt + 1;
										if (ItemsCnt >= ItemsSize)
										{
											ItemsSize = ItemsSize + 100;
											Array.Resize(ref Items, ItemsSize + 1);
										}
									}
								}
								cpCore.db.csGoNext(CSAddons);
							}
						}
						cpCore.db.csClose(CSAddons);
					}
					//
					// Build output sting in alphabetical order by name
					//
					s = "";
					ItemsPtr = Index.getFirstPtr;
					while (ItemsPtr >= 0 && LoopPtr < ItemsCnt)
					{
						s = s + Environment.NewLine + "," + Items[ItemsPtr];
						PtrTest = Index.getNextPtr;
						if (PtrTest < 0)
						{
							break;
						}
						else
						{
							ItemsPtr = PtrTest;
						}
						LoopPtr = LoopPtr + 1;
					}
					if (!string.IsNullOrEmpty(s))
					{
						s = "[" + s.Substring(3) + "]";
					}
					//
					result = s;
					cpCore.docProperties.setProperty(cacheKey, result, false);
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//'
		//'========================================================================
		//'   deprecated - see csv_EncodeActiveContent_Internal
		//'========================================================================
		//'
		//Public Function html_EncodeActiveContent4(ByVal Source As String, ByVal PeopleID As Integer, ByVal ContextContentName As String, ByVal ContextRecordID As Integer, ByVal ContextContactPeopleID As Integer, ByVal AddLinkEID As Boolean, ByVal EncodeCachableTags As Boolean, ByVal EncodeImages As Boolean, ByVal EncodeEditIcons As Boolean, ByVal EncodeNonCachableTags As Boolean, ByVal AddAnchorQuery As String, ByVal ProtocolHostString As String, ByVal IsEmailContent As Boolean, ByVal AdminURL As String) As String
		//    html_EncodeActiveContent4 = html_EncodeActiveContent_Internal(Source, PeopleID, ContextContentName, ContextRecordID, ContextContactPeopleID, AddLinkEID, EncodeCachableTags, EncodeImages, EncodeEditIcons, EncodeNonCachableTags, AddAnchorQuery, ProtocolHostString, IsEmailContent, AdminURL, cpCore.doc.authContext.isAuthenticated)
		//End Function
		//'
		//'========================================================================
		//'   see csv_EncodeActiveContent_Internal
		//'========================================================================
		//'
		//Public Function html_EncodeActiveContent5(ByVal Source As String, ByVal PeopleID As Integer, ByVal ContextContentName As String, ByVal ContextRecordID As Integer, ByVal ContextContactPeopleID As Integer, ByVal AddLinkEID As Boolean, ByVal EncodeCachableTags As Boolean, ByVal EncodeImages As Boolean, ByVal EncodeEditIcons As Boolean, ByVal EncodeNonCachableTags As Boolean, ByVal AddAnchorQuery As String, ByVal ProtocolHostString As String, ByVal IsEmailContent As Boolean, ByVal AdminURL As String, ByVal personalizationIsAuthenticated As Boolean, ByVal Context As CPUtilsBaseClass.addonContext) As String
		//    html_EncodeActiveContent5 = html_EncodeActiveContent_Internal(Source, PeopleID, ContextContentName, ContextRecordID, ContextContactPeopleID, AddLinkEID, EncodeCachableTags, EncodeImages, EncodeEditIcons, EncodeNonCachableTags, AddAnchorQuery, ProtocolHostString, IsEmailContent, AdminURL, cpCore.doc.authContext.isAuthenticated, Context)
		//End Function
		//
		//========================================================================
		//   encode (execute) all {% -- %} commands
		//========================================================================
		//
		public string executeContentCommands(object nothingObject, string Source, CPUtilsBaseClass.addonContext Context, int personalizationPeopleId, bool personalizationIsAuthenticated, ref string Return_ErrorMessage)
		{
			string returnValue = "";
			try
			{
				int LoopPtr = 0;
				contentCmdController contentCmd = new contentCmdController(cpCore);
				//
				returnValue = Source;
				LoopPtr = 0;
				while ((LoopPtr < 10) && ((returnValue.IndexOf(contentReplaceEscapeStart) + 1 != 0)))
				{
					returnValue = contentCmd.ExecuteCmd(returnValue, Context, personalizationPeopleId, personalizationIsAuthenticated);
					LoopPtr = LoopPtr + 1;
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return returnValue;
		}
		//
		//========================================================================
		// csv_EncodeActiveContent_Internal
		//       ...
		//       AllowLinkEID    Boolean, if yes, the EID=000... string is added to all links in the content
		//                       Use this for email so links will include the members longin.
		//
		//       Some Active elements can not be replaced here because they incorporate content from  the wbeclient.
		//       For instance the Aggregate Function Objects. These elements create
		//       <!-- FPO1 --> placeholders in the content, and commented instructions, one per line, at the top of the content
		//       Replacement instructions
		//       <!-- Replace FPO1,AFObject,ObjectName,OptionString -->
		//           Aggregate Function Object, ProgramID=ObjectName, Optionstring=Optionstring
		//       <!-- Replace FPO1,AFObject,ObjectName,OptionString -->
		//           Aggregate Function Object, ProgramID=ObjectName, Optionstring=Optionstring
		//
		// Tag descriptions:
		//
		//   primary methods
		//
		//   <Ac Type="Date">
		//   <Ac Type="Member" Field="Name">
		//   <Ac Type="Organization" Field="Name">
		//   <Ac Type="Visitor" Field="Name">
		//   <Ac Type="Visit" Field="Name">
		//   <Ac Type="Contact" Member="PeopleID">
		//       displays a For More info block of copy
		//   <Ac Type="Feedback" Member="PeopleID">
		//       displays a feedback note block
		//   <Ac Type="ChildList" Name="ChildListName">
		//       displays a list of child blocks that reference this CHildList Element
		//   <Ac Type="Language" Name="All|English|Spanish|etc.">
		//       blocks content to next language tag to eveyone without this PeopleLanguage
		//   <Ac Type="Image" Record="" Width="" Height="" Alt="" Align="">
		//   <AC Type="Download" Record="" Alt="">
		//       renders as an anchored download icon, with the alt tag
		//       the rendered anchor points back to the root/index, which increments the resource's download count
		//
		//   During Editing, AC tags are converted (Encoded) to EditIcons
		//       these are image tags with AC information stored in the ID attribute
		//       except AC-Image, which are converted into the actual image for editing
		//       during the edit save, the EditIcons are converted (Decoded) back
		//
		//   Remarks
		//
		//   First <Member.FieldName> encountered opens the Members Table, etc.
		//       ( does <OpenTable name="Member" Tablename="ccMembers" ID=(current PeopleID)> )
		//   The copy is divided into Blocks, starting at every tag and running to the next tag.
		//   BlockTag()  The tag without the braces found
		//   BlockCopy() The copy following the tag up to the next tag
		//   BlockLabel()    the string identifier for the block
		//   BlockCount  the total blocks in the message
		//   BlockPointer    the current block being examined
		//========================================================================
		//
		private string convertActiveContent_Internal_activeParts(string Source, int personalizationPeopleId, string ContextContentName, int ContextRecordID, int moreInfoPeopleId, bool AddLinkEID, bool EncodeCachableTags, bool EncodeImages, bool EncodeEditIcons, bool EncodeNonCachableTags, string AddAnchorQuery, string ProtocolHostLink, bool IsEmailContent, string AdminURL, bool personalizationIsAuthenticated, CPUtilsBaseClass.addonContext context = CPUtilsBaseClass.addonContext.ContextPage)
		{
			string result = "";
			try
			{
				string ACGuid = null;
				bool AddonFound = false;
				string ACNameCaption = null;
				string GroupIDList = null;
				string IDControlString = null;
				string IconIDControlString = null;
				string Criteria = null;
				string AddonContentName = null;
				string SelectList = "";
				int IconWidth = 0;
				int IconHeight = 0;
				int IconSprites = 0;
				bool AddonIsInline = false;
				string IconAlt = "";
				string IconTitle = "";
				string IconImg = null;
				string TextName = null;
				string ListName = null;
				string SrcOptionSelector = null;
				string ResultOptionSelector = null;
				string SrcOptionList = null;
				int Pos = 0;
				string REsultOptionValue = null;
				string SrcOptionValueSelector = null;
				string InstanceOptionValue = null;
				string ResultOptionListHTMLEncoded = null;
				string UCaseACName = null;
				string IconFilename = null;
				string FieldName = null;
				int Ptr = 0;
				int ElementPointer = 0;
				int ListCount = 0;
				int CSVisitor = 0;
				int CSVisit = 0;
				bool CSVisitorSet = false;
				bool CSVisitSet = false;
				string ElementTag = null;
				string ACType = null;
				string ACField = null;
				string ACName = "";
				string Copy = null;
				htmlParserController KmaHTML = null;
				int AttributeCount = 0;
				int AttributePointer = 0;
				string Name = null;
				string Value = null;
				int CS = 0;
				int ACAttrRecordID = 0;
				int ACAttrWidth = 0;
				int ACAttrHeight = 0;
				string ACAttrAlt = null;
				int ACAttrBorder = 0;
				int ACAttrLoop = 0;
				int ACAttrVSpace = 0;
				int ACAttrHSpace = 0;
				string Filename = "";
				string ACAttrAlign = null;
				bool ProcessAnchorTags = false;
				bool ProcessACTags = false;
				string ACLanguageName = null;
				stringBuilderLegacyController Stream = new stringBuilderLegacyController();
				string AnchorQuery = "";
				int CSOrganization = 0;
				bool CSOrganizationSet = false;
				int CSPeople = 0;
				bool CSPeopleSet = false;
				int CSlanguage = 0;
				bool PeopleLanguageSet = false;
				string PeopleLanguage = "";
				string UcasePeopleLanguage = null;
				string serverFilePath = "";
				string ReplaceInstructions = string.Empty;
				string Link = null;
				int NotUsedID = 0;
				string addonOptionString = null;
				string AddonOptionStringHTMLEncoded = null;
				string[] SrcOptions = null;
				string SrcOptionName = null;
				int FormCount = 0;
				int FormInputCount = 0;
				string ACInstanceID = null;
				int PosStart = 0;
				int PosEnd = 0;
				string AllowGroups = null;
				string workingContent = null;
				string NewName = null;
				//
				workingContent = Source;
				//
				// Fixup Anchor Query (additional AddonOptionString pairs to add to the end)
				//
				if (AddLinkEID && (personalizationPeopleId != 0))
				{
					AnchorQuery = AnchorQuery + "&EID=" + cpCore.security.encodeToken(genericController.EncodeInteger(personalizationPeopleId), DateTime.Now);
				}
				//
				if (!string.IsNullOrEmpty(AddAnchorQuery))
				{
					AnchorQuery = AnchorQuery + "&" + AddAnchorQuery;
				}
				//
				if (!string.IsNullOrEmpty(AnchorQuery))
				{
					AnchorQuery = AnchorQuery.Substring(1);
				}
				//
				// ----- xml contensive process instruction
				//
				//TemplateBodyContent
				//Pos = genericController.vbInstr(1, TemplateBodyContent, "<?contensive", vbTextCompare)
				//If Pos > 0 Then
				//    '
				//    ' convert template body if provided - this is the content that replaces the content box addon
				//    '
				//    TemplateBodyContent = Mid(TemplateBodyContent, Pos)
				//    LayoutEngineOptionString = "data=" & encodeNvaArgument(TemplateBodyContent)
				//    TemplateBodyContent = csv_ExecuteActiveX("aoPrimitives.StructuredDataClass", "Structured Data Engine", nothing, LayoutEngineOptionString, "data=(structured data)", LayoutErrorMessage)
				//End If
				Pos = genericController.vbInstr(1, workingContent, "<?contensive", Microsoft.VisualBasic.Constants.vbTextCompare);
				if (Pos > 0)
				{
					throw new ApplicationException("Structured xml data commands are no longer supported");
					//'
					//' convert content if provided
					//'
					//workingContent = Mid(workingContent, Pos)
					//LayoutEngineOptionString = "data=" & encodeNvaArgument(workingContent)
					//Dim structuredData As New core_primitivesStructuredDataClass(Me)
					//workingContent = structuredData.execute()
					//workingContent = csv_ExecuteActiveX("aoPrimitives.StructuredDataClass", "Structured Data Engine", LayoutEngineOptionString, "data=(structured data)", LayoutErrorMessage)
				}
				//
				// Special Case
				// Convert <!-- STARTGROUPACCESS 10,11,12 --> format to <AC type=GROUPACCESS AllowGroups="10,11,12">
				// Convert <!-- ENDGROUPACCESS --> format to <AC type=GROUPACCESSEND>
				//
				PosStart = genericController.vbInstr(1, workingContent, "<!-- STARTGROUPACCESS ", Microsoft.VisualBasic.Constants.vbTextCompare);
				if (PosStart > 0)
				{
					PosEnd = genericController.vbInstr(PosStart, workingContent, "-->");
					if (PosEnd > 0)
					{
						AllowGroups = workingContent.Substring(PosStart + 21, PosEnd - PosStart - 23);
						workingContent = workingContent.Substring(0, PosStart - 1) + "<AC type=\"" + ACTypeAggregateFunction + "\" name=\"block text\" querystring=\"allowgroups=" + AllowGroups + "\">" + workingContent.Substring(PosEnd + 2);
					}
				}
				//
				PosStart = genericController.vbInstr(1, workingContent, "<!-- ENDGROUPACCESS ", Microsoft.VisualBasic.Constants.vbTextCompare);
				if (PosStart > 0)
				{
					PosEnd = genericController.vbInstr(PosStart, workingContent, "-->");
					if (PosEnd > 0)
					{
						workingContent = workingContent.Substring(0, PosStart - 1) + "<AC type=\"" + ACTypeAggregateFunction + "\" name=\"block text end\" >" + workingContent.Substring(PosEnd + 2);
					}
				}
				//
				// ----- Special case -- if any of these are in the source, this is legacy. Convert them to icons,
				//       and they will be converted to AC tags when the icons are saved
				//
				if (EncodeEditIcons)
				{
					//
					IconIDControlString = "AC," + ACTypeTemplateContent + "," + NotUsedID + "," + ACName + ",";
					IconImg = genericController.GetAddonIconImg(AdminURL, 52, 64, 0, false, IconIDControlString, "/ccLib/images/ACTemplateContentIcon.gif", serverFilePath, "Template Page Content", "Renders as [Template Page Content]", "", 0);
					workingContent = genericController.vbReplace(workingContent, "{{content}}", IconImg, 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
					//WorkingContent = genericController.vbReplace(WorkingContent, "{{content}}", "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Page Content"" id=""AC," & ACTypeTemplateContent & "," & NotUsedID & "," & ACName & ","" src=""/ccLib/images/ACTemplateContentIcon.gif"" WIDTH=52 HEIGHT=64>", 1, -1, vbTextCompare)
					//'
					//' replace all other {{...}}
					//'
					//LoopPtr = 0
					//Pos = 1
					//Do While Pos > 0 And LoopPtr < 100
					//    Pos = genericController.vbInstr(Pos, workingContent, "{{" & ACTypeDynamicMenu, vbTextCompare)
					//    If Pos > 0 Then
					//        addonOptionString = ""
					//        PosStart = Pos
					//        If PosStart <> 0 Then
					//            'PosStart = PosStart + 2 + Len(ACTypeDynamicMenu)
					//            PosEnd = genericController.vbInstr(PosStart, workingContent, "}}", vbTextCompare)
					//            If PosEnd <> 0 Then
					//                Cmd = Mid(workingContent, PosStart + 2, PosEnd - PosStart - 2)
					//                Pos = genericController.vbInstr(1, Cmd, "?")
					//                If Pos <> 0 Then
					//                    addonOptionString = genericController.decodeHtml(Mid(Cmd, Pos + 1))
					//                End If
					//                TextName = cpCore.csv_GetAddonOptionStringValue("menu", addonOptionString)
					//                '
					//                addonOptionString = "Menu=" & TextName & "[" & cpCore.csv_GetDynamicMenuACSelect() & "]&NewMenu="
					//                AddonOptionStringHTMLEncoded = html_EncodeHTML("Menu=" & TextName & "[" & cpCore.csv_GetDynamicMenuACSelect() & "]&NewMenu=")
					//                '
					//                IconIDControlString = "AC," & ACTypeDynamicMenu & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded
					//                IconImg = genericController.GetAddonIconImg(AdminURL, 52, 52, 0, False, IconIDControlString, "/ccLib/images/ACDynamicMenuIcon.gif", serverFilePath, "Dynamic Menu", "Renders as [Dynamic Menu]", "", 0)
					//                workingContent = Mid(workingContent, 1, PosStart - 1) & IconImg & Mid(workingContent, PosEnd + 2)
					//            End If
					//        End If
					//    End If
					//Loop
				}
				//
				// Test early if this needs to run at all
				//
				ProcessACTags = (((EncodeCachableTags || EncodeNonCachableTags || EncodeImages || EncodeEditIcons)) & (workingContent.IndexOf("<AC ", System.StringComparison.OrdinalIgnoreCase) + 1 != 0)) != 0;
				ProcessAnchorTags = (!string.IsNullOrEmpty(AnchorQuery)) & (workingContent.IndexOf("<A ", System.StringComparison.OrdinalIgnoreCase) + 1 != 0);
				if ((!string.IsNullOrEmpty(workingContent)) & (ProcessAnchorTags || ProcessACTags))
				{
					//
					// ----- Load the Active Elements
					//
					KmaHTML = new htmlParserController(cpCore);
					KmaHTML.Load(workingContent);
					//
					// ----- Execute and output elements
					//
					ElementPointer = 0;
					if (KmaHTML.ElementCount > 0)
					{
						ElementPointer = 0;
						workingContent = "";
						serverFilePath = ProtocolHostLink + "/" + cpCore.serverConfig.appConfig.name + "/files/";
						Stream = new stringBuilderLegacyController();
						while (ElementPointer < KmaHTML.ElementCount)
						{
							Copy = KmaHTML.Text(ElementPointer).ToString();
							if (KmaHTML.IsTag(ElementPointer))
							{
								ElementTag = genericController.vbUCase(KmaHTML.TagName(ElementPointer));
								ACName = KmaHTML.ElementAttribute(ElementPointer, "NAME");
								UCaseACName = genericController.vbUCase(ACName);
								switch (ElementTag)
								{
									case "FORM":
										//
										// Form created in content
										// EncodeEditIcons -> remove the
										//
										if (EncodeNonCachableTags)
										{
											FormCount = FormCount + 1;
											//
											// 5/14/2009 - DM said it is OK to remove UserResponseForm Processing
											// however, leave this one because it is needed to make current forms work.
											//
											if ((Copy.IndexOf("contensiveuserform=1", System.StringComparison.OrdinalIgnoreCase) + 1 != 0) | (Copy.IndexOf("contensiveuserform=\"1\"", System.StringComparison.OrdinalIgnoreCase) + 1 != 0))
											{
												//
												// if it has "contensiveuserform=1" in the form tag, remove it from the form and add the hidden that makes it work
												//
												Copy = genericController.vbReplace(Copy, "ContensiveUserForm=1", "", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
												Copy = genericController.vbReplace(Copy, "ContensiveUserForm=\"1\"", "", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
												if (!EncodeEditIcons)
												{
													Copy = Copy + "<input type=hidden name=ContensiveUserForm value=1>";
												}
											}
										}
										break;
									case "INPUT":
										if (EncodeNonCachableTags)
										{
											FormInputCount = FormInputCount + 1;
										}
										break;
									case "A":
										if (!string.IsNullOrEmpty(AnchorQuery))
										{
											//
											// ----- Add ?eid=0000 to all anchors back to the same site so emails
											//       can be sent that will automatically log the person in when they
											//       arrive.
											//
											AttributeCount = KmaHTML.ElementAttributeCount(ElementPointer);
											if (AttributeCount > 0)
											{
												Copy = "<A ";
												for (AttributePointer = 0; AttributePointer < AttributeCount; AttributePointer++)
												{
													Name = KmaHTML.ElementAttributeName(ElementPointer, AttributePointer);
													Value = KmaHTML.ElementAttributeValue(ElementPointer, AttributePointer);
													if (genericController.vbUCase(Name) == "HREF")
													{
														Link = Value;
														Pos = genericController.vbInstr(1, Link, "://");
														if (Pos > 0)
														{
															Link = Link.Substring(Pos + 2);
															Pos = genericController.vbInstr(1, Link, "/");
															if (Pos > 0)
															{
																Link = Link.Substring(0, Pos - 1);
															}
														}
														if ((string.IsNullOrEmpty(Link)) || ("," + cpCore.serverConfig.appConfig.domainList(0) + ",".IndexOf("," + Link + ",", System.StringComparison.OrdinalIgnoreCase) + 1 != 0))
														{
															//
															// ----- link is for this site
															//
															if (Value.Substring(Value.Length - 1) == "?")
															{
																//
																// Ends in a questionmark, must be Dwayne (?)
																//
																Value = Value + AnchorQuery;
															}
															else if (genericController.vbInstr(1, Value, "mailto:", Microsoft.VisualBasic.Constants.vbTextCompare) != 0)
															{
																//
																// catch mailto
																//
																//Value = Value & AnchorQuery
															}
															else if (genericController.vbInstr(1, Value, "?") == 0)
															{
																//
																// No questionmark there, add it
																//
																Value = Value + "?" + AnchorQuery;
															}
															else
															{
																//
																// Questionmark somewhere, add new value with amp;
																//
																Value = Value + "&" + AnchorQuery;
															}
															//    End If
														}
													}
													Copy = Copy + " " + Name + "=\"" + Value + "\"";
												}
												Copy = Copy + ">";
											}
										}
										break;
									case "AC":
										//
										// ----- decode all AC tags
										//
										ListCount = 0;
										ACType = KmaHTML.ElementAttribute(ElementPointer, "TYPE");
										// if ACInstanceID=0, it can not create settings link in edit mode. ACInstanceID is added during edit save.
										ACInstanceID = KmaHTML.ElementAttribute(ElementPointer, "ACINSTANCEID");
										ACGuid = KmaHTML.ElementAttribute(ElementPointer, "GUID");
										switch (genericController.vbUCase(ACType))
										{
											case ACTypeEnd:
											{
												//
												// End Tag - Personalization
												//       This tag causes an end to the all tags, like Language
												//       It is removed by with EncodeEditIcons (on the way to the editor)
												//       It is added to the end of the content with Decode(activecontent)
												//
												if (EncodeEditIcons)
												{
													Copy = "";
												}
												else if (EncodeNonCachableTags)
												{
													Copy = "<!-- Language ANY -->";
												}
												break;
											}
											case ACTypeDate:
											{
												//
												// Date Tag
												//
												if (EncodeEditIcons)
												{
													IconIDControlString = "AC," + ACTypeDate;
													IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "Current Date", "Renders as [Current Date]", ACInstanceID, 0);
													Copy = IconImg;
													//Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as the current date"" ID=""AC," & ACTypeDate & """ src=""/ccLib/images/ACDate.GIF"">"
												}
												else if (EncodeNonCachableTags)
												{
													Copy = DateTime.Now.ToString();
												}
												break;
											}
											case ACTypeMember:
											case ACTypePersonalization:
											{
												//
												// Member Tag works regardless of authentication
												// cm must be sure not to reveal anything
												//
												ACField = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "FIELD"));
												if (string.IsNullOrEmpty(ACField))
												{
													// compatibility for old personalization type
													ACField = htmlController.getAddonOptionStringValue("FIELD", KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING"));
												}
												FieldName = genericController.EncodeInitialCaps(ACField);
												if (string.IsNullOrEmpty(FieldName))
												{
													FieldName = "Name";
												}
												if (EncodeEditIcons)
												{
													switch (genericController.vbUCase(FieldName))
													{
														case "FIRSTNAME":
															//
															IconIDControlString = "AC," + ACType + "," + FieldName;
															IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "User's First Name", "Renders as [User's First Name]", ACInstanceID, 0);
															Copy = IconImg;
														//
															break;
														case "LASTNAME":
															//
															IconIDControlString = "AC," + ACType + "," + FieldName;
															IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "User's Last Name", "Renders as [User's Last Name]", ACInstanceID, 0);
															Copy = IconImg;
															//
															break;
														default:
															//
															IconIDControlString = "AC," + ACType + "," + FieldName;
															IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "User's " + FieldName, "Renders as [User's " + FieldName + "]", ACInstanceID, 0);
															Copy = IconImg;
															//
															break;
													}
												}
												else if (EncodeNonCachableTags)
												{
													if (personalizationPeopleId != 0)
													{
														if (genericController.vbUCase(FieldName) == "EID")
														{
															Copy = cpCore.security.encodeToken(personalizationPeopleId, DateTime.Now);
														}
														else
														{
															if (!CSPeopleSet)
															{
																CSPeople = cpCore.db.cs_openContentRecord("People", personalizationPeopleId);
																CSPeopleSet = true;
															}
															if ((cpCore.db.csOk(CSPeople) & cpCore.db.cs_isFieldSupported(CSPeople, FieldName)) != 0)
															{
																Copy = cpCore.db.csGetLookup(CSPeople, FieldName);
															}
														}
													}
												}
												break;
											}
											case ACTypeChildList:
											{
												//
												// Child List
												//
												ListName = genericController.encodeText((KmaHTML.ElementAttribute(ElementPointer, "name")));

												if (EncodeEditIcons)
												{
													IconIDControlString = "AC," + ACType + ",," + ACName;
													IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "List of Child Pages", "Renders as [List of Child Pages]", ACInstanceID, 0);
													Copy = IconImg;
												}
												else if (EncodeCachableTags)
												{
													//
													// Handle in webclient
													//
													// removed sort method because all child pages are read in together in the order set by the parent - improve this later
													Copy = "{{" + ACTypeChildList + "?name=" + genericController.encodeNvaArgument(ListName) + "}}";
												}
												break;
											}
											case ACTypeContact:
											{
												//
												// Formatting Tag
												//
												if (EncodeEditIcons)
												{
													//
													IconIDControlString = "AC," + ACType;
													IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "Contact Information Line", "Renders as [Contact Information Line]", ACInstanceID, 0);
													Copy = IconImg;
													//
													//Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as a line of text with contact information for this record's primary contact"" id=""AC," & ACType & """ src=""/ccLib/images/ACContact.GIF"">"
												}
												else if (EncodeCachableTags)
												{
													if (moreInfoPeopleId != 0)
													{
														Copy = pageContentController.getMoreInfoHtml(cpCore, moreInfoPeopleId);
													}
												}
												break;
											}
											case ACTypeFeedback:
											{
												//
												// Formatting tag - change from information to be included after submission
												//
												if (EncodeEditIcons)
												{
													//
													IconIDControlString = "AC," + ACType;
													IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, false, IconIDControlString, "", serverFilePath, "Feedback Form", "Renders as [Feedback Form]", ACInstanceID, 0);
													Copy = IconImg;
													//
													//Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as a feedback form, sent to this record's primary contact."" id=""AC," & ACType & """ src=""/ccLib/images/ACFeedBack.GIF"">"
												}
												else if (EncodeNonCachableTags)
												{
													if ((moreInfoPeopleId != 0) & (!string.IsNullOrEmpty(ContextContentName)) & (ContextRecordID != 0))
													{
														Copy = FeedbackFormNotSupportedComment;
													}
												}
												break;
											}
											case ACTypeLanguage:
											{
												//
												// Personalization Tag - block languages not from the visitor
												//
												ACLanguageName = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "NAME"));
												if (EncodeEditIcons)
												{
													switch (genericController.vbUCase(ACLanguageName))
													{
														case "ANY":
															//
															IconIDControlString = "AC," + ACType + ",," + ACLanguageName;
															IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "All copy following this point is rendered, regardless of the member's language setting", "Renders as [Begin Rendering All Languages]", ACInstanceID, 0);
															Copy = IconImg;
															//
															//Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""All copy following this point is rendered, regardless of the member's language setting"" id=""AC," & ACType & ",," & ACLanguageName & """ src=""/ccLib/images/ACLanguageAny.GIF"">"
															//Case "ENGLISH", "FRENCH", "GERMAN", "PORTUGEUESE", "ITALIAN", "SPANISH", "CHINESE", "HINDI"
															//   Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""All copy following this point is rendered if the member's language setting matchs [" & ACLanguageName & "]"" id=""AC," & ACType & ",," & ACLanguageName & """ src=""/ccLib/images/ACLanguage" & ACLanguageName & ".GIF"">"
															break;
														default:
															//
															IconIDControlString = "AC," + ACType + ",," + ACLanguageName;
															IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "All copy following this point is rendered if the member's language setting matchs [" + ACLanguageName + "]", "Begin Rendering for language [" + ACLanguageName + "]", ACInstanceID, 0);
															Copy = IconImg;
															//
															//Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""All copy following this point is rendered if the member's language setting matchs [" & ACLanguageName & "]"" id=""AC," & ACType & ",," & ACLanguageName & """ src=""/ccLib/images/ACLanguageOther.GIF"">"
															break;
													}
												}
												else if (EncodeNonCachableTags)
												{
													if (personalizationPeopleId == 0)
													{
														PeopleLanguage = "any";
													}
													else
													{
														if (!PeopleLanguageSet)
														{
															if (!CSPeopleSet)
															{
																CSPeople = cpCore.db.cs_openContentRecord("people", personalizationPeopleId);
																CSPeopleSet = true;
															}
															CSlanguage = cpCore.db.cs_openContentRecord("Languages", cpCore.db.csGetInteger(CSPeople, "LanguageID"),,,, "Name");
															if (cpCore.db.csOk(CSlanguage))
															{
																PeopleLanguage = cpCore.db.csGetText(CSlanguage, "name");
															}
															cpCore.db.csClose(CSlanguage);
															PeopleLanguageSet = true;
														}
													}
													UcasePeopleLanguage = genericController.vbUCase(PeopleLanguage);
													if (UcasePeopleLanguage == "ANY")
													{
														//
														// This person wants all the languages, put in language marker and continue
														//
														Copy = "<!-- Language " + ACLanguageName + " -->";
													}
													else if ((ACLanguageName != UcasePeopleLanguage) & (ACLanguageName != "ANY"))
													{
														//
														// Wrong language, remove tag, skip to the end, or to the next language tag
														//
														Copy = "";
														ElementPointer = ElementPointer + 1;
														while (ElementPointer < KmaHTML.ElementCount)
														{
															ElementTag = genericController.vbUCase(KmaHTML.TagName(ElementPointer));
															if (ElementTag == "AC")
															{
																ACType = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "TYPE"));
																if (ACType == ACTypeLanguage)
																{
																	ElementPointer = ElementPointer - 1;
																	break;
																}
																else if (ACType == ACTypeEnd)
																{
																	break;
																}
															}
															ElementPointer = ElementPointer + 1;
														}
													}
													else
													{
														//
														// Right Language, remove tag
														//
														Copy = "";
													}
												}
												break;
											}
											case ACTypeAggregateFunction:
											{
												//
												// ----- Add-on
												//
												NotUsedID = 0;
												AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING");
												addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded);
												if (IsEmailContent)
												{
													//
													// Addon - for email
													//
													if (EncodeNonCachableTags)
													{
														//
														// Only hardcoded Add-ons can run in Emails
														//
														switch (genericController.vbLCase(ACName))
														{
															case "block text":
																//
																// Email is always considered authenticated bc they need their login credentials to get the email.
																// Allowed to see the content that follows if you are authenticated, admin, or in the group list
																// This must be done out on the page because the csv does not know about authenticated
																//
																Copy = "";
																GroupIDList = htmlController.getAddonOptionStringValue("AllowGroups", addonOptionString);
																if (!cpCore.doc.authContext.isMemberOfGroupIdList(cpCore, personalizationPeopleId, true, GroupIDList, true))
																{
																	//
																	// Block content if not allowed
																	//
																	ElementPointer = ElementPointer + 1;
																	while (ElementPointer < KmaHTML.ElementCount)
																	{
																		ElementTag = genericController.vbUCase(KmaHTML.TagName(ElementPointer));
																		if (ElementTag == "AC")
																		{
																			ACType = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "TYPE"));
																			if (ACType == ACTypeAggregateFunction)
																			{
																				if (genericController.vbLCase(KmaHTML.ElementAttribute(ElementPointer, "name")) == "block text end")
																				{
																					break;
																				}
																			}
																		}
																		ElementPointer = ElementPointer + 1;
																	}
																}
																break;
															case "block text end":
																//
																// always remove end tags because the block text did not remove it
																//
																Copy = "";
																break;
															default:
																//
																// all other add-ons, pass out to cpCoreClass to process
																CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext()
																{
																	addonType = CPUtilsBaseClass.addonContext.ContextEmail,
																	cssContainerClass = "",
																	cssContainerId = "",
																	hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext()
																	{
																		contentName = ContextContentName,
																		fieldName = "",
																		recordId = ContextRecordID
																	},
																	personalizationAuthenticated = personalizationIsAuthenticated,
																	personalizationPeopleId = personalizationPeopleId,
																	instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, AddonOptionStringHTMLEncoded),
																	instanceGuid = ACInstanceID
																};
																Models.Entity.addonModel addon = Models.Entity.addonModel.createByName(cpCore, ACName);
																Copy = cpCore.addon.execute(addon, executeContext);
																//Copy = cpCore.addon.execute_legacy6(0, ACName, AddonOptionStringHTMLEncoded, CPUtilsBaseClass.addonContext.ContextEmail, "", 0, "", ACInstanceID, False, 0, "", True, Nothing, "", Nothing, "", personalizationPeopleId, personalizationIsAuthenticated)
																break;
														}
													}
												}
												else
												{
													//
													// Addon - for web
													//

													if (EncodeEditIcons)
													{
														//
														// Get IconFilename, update the optionstring, and execute optionstring replacement functions
														//
														AddonContentName = cnAddons;
														if (true)
														{
															SelectList = "Name,Link,ID,ArgumentList,ObjectProgramID,IconFilename,IconWidth,IconHeight,IconSprites,IsInline,ccGuid";
														}
														if (!string.IsNullOrEmpty(ACGuid))
														{
															Criteria = "ccguid=" + cpCore.db.encodeSQLText(ACGuid);
														}
														else
														{
															Criteria = "name=" + cpCore.db.encodeSQLText(UCaseACName);
														}
														CS = cpCore.db.csOpen(AddonContentName, Criteria, "Name,ID",,,,, SelectList);
														if (cpCore.db.csOk(CS))
														{
															AddonFound = true;
															// ArgumentList comes in already encoded
															IconFilename = cpCore.db.csGet(CS, "IconFilename");
															SrcOptionList = cpCore.db.csGet(CS, "ArgumentList");
															IconWidth = cpCore.db.csGetInteger(CS, "IconWidth");
															IconHeight = cpCore.db.csGetInteger(CS, "IconHeight");
															IconSprites = cpCore.db.csGetInteger(CS, "IconSprites");
															AddonIsInline = cpCore.db.csGetBoolean(CS, "IsInline");
															ACGuid = cpCore.db.csGetText(CS, "ccGuid");
															IconAlt = ACName;
															IconTitle = "Rendered as the Add-on [" + ACName + "]";
														}
														else
														{
															switch (genericController.vbLCase(ACName))
															{
																case "block text":
																	IconFilename = "";
																	SrcOptionList = AddonOptionConstructor_ForBlockText;
																	IconWidth = 0;
																	IconHeight = 0;
																	IconSprites = 0;
																	AddonIsInline = true;
																	ACGuid = "";
																	break;
																case "block text end":
																	IconFilename = "";
																	SrcOptionList = "";
																	IconWidth = 0;
																	IconHeight = 0;
																	IconSprites = 0;
																	AddonIsInline = true;
																	ACGuid = "";
																	break;
																default:
																	IconFilename = "";
																	SrcOptionList = "";
																	IconWidth = 0;
																	IconHeight = 0;
																	IconSprites = 0;
																	AddonIsInline = false;
																	IconAlt = "Unknown Add-on [" + ACName + "]";
																	IconTitle = "Unknown Add-on [" + ACName + "]";
																	ACGuid = "";
																	break;
															}
														}
														cpCore.db.csClose(CS);
														//
														// Build AddonOptionStringHTMLEncoded from SrcOptionList (for names), itself (for current settings), and SrcOptionList (for select options)
														//
														if (SrcOptionList.IndexOf("wrapper", System.StringComparison.OrdinalIgnoreCase) + 1 == 0)
														{
															if (AddonIsInline)
															{
																SrcOptionList = SrcOptionList + Environment.NewLine + AddonOptionConstructor_Inline;
															}
															else
															{
																SrcOptionList = SrcOptionList + Environment.NewLine + AddonOptionConstructor_Block;
															}
														}
														if (string.IsNullOrEmpty(SrcOptionList))
														{
															ResultOptionListHTMLEncoded = "";
														}
														else
														{
															ResultOptionListHTMLEncoded = "";
															REsultOptionValue = "";
															SrcOptionList = genericController.vbReplace(SrcOptionList, Environment.NewLine, "\r");
															SrcOptionList = genericController.vbReplace(SrcOptionList, "\n", "\r");
															SrcOptions = Microsoft.VisualBasic.Strings.Split(SrcOptionList, "\r", -1, Microsoft.VisualBasic.CompareMethod.Binary);
															for (Ptr = 0; Ptr <= SrcOptions.GetUpperBound(0); Ptr++)
															{
																SrcOptionName = SrcOptions[Ptr];
																int LoopPtr2 = 0;

																while ((SrcOptionName.Length > 1) && (SrcOptionName.Substring(0, 1) == "\t") && (LoopPtr2 < 100))
																{
																	SrcOptionName = SrcOptionName.Substring(1);
																	LoopPtr2 = LoopPtr2 + 1;
																}
																SrcOptionValueSelector = "";
																SrcOptionSelector = "";
																Pos = genericController.vbInstr(1, SrcOptionName, "=");
																if (Pos > 0)
																{
																	SrcOptionValueSelector = SrcOptionName.Substring(Pos);
																	SrcOptionName = SrcOptionName.Substring(0, Pos - 1);
																	SrcOptionSelector = "";
																	Pos = genericController.vbInstr(1, SrcOptionValueSelector, "[");
																	if (Pos != 0)
																	{
																		SrcOptionSelector = SrcOptionValueSelector.Substring(Pos - 1);
																	}
																}
																// all Src and Instance vars are already encoded correctly
																if (!string.IsNullOrEmpty(SrcOptionName))
																{
																	// since AddonOptionString is encoded, InstanceOptionValue will be also
																	InstanceOptionValue = htmlController.getAddonOptionStringValue(SrcOptionName, addonOptionString);
																	//InstanceOptionValue = cpcore.csv_GetAddonOption(SrcOptionName, AddonOptionString)
																	ResultOptionSelector = getAddonSelector(SrcOptionName, genericController.encodeNvaArgument(InstanceOptionValue), SrcOptionSelector);
																	//ResultOptionSelector = csv_GetAddonSelector(SrcOptionName, InstanceOptionValue, SrcOptionValueSelector)
																	ResultOptionListHTMLEncoded = ResultOptionListHTMLEncoded + "&" + ResultOptionSelector;
																}
															}
															if (!string.IsNullOrEmpty(ResultOptionListHTMLEncoded))
															{
																ResultOptionListHTMLEncoded = ResultOptionListHTMLEncoded.Substring(1);
															}
														}
														ACNameCaption = genericController.vbReplace(ACName, "\"", "");
														ACNameCaption = encodeHTML(ACNameCaption);
														IDControlString = "AC," + ACType + "," + NotUsedID + "," + genericController.encodeNvaArgument(ACName) + "," + ResultOptionListHTMLEncoded + "," + ACGuid;
														Copy = genericController.GetAddonIconImg(AdminURL, IconWidth, IconHeight, IconSprites, AddonIsInline, IDControlString, IconFilename, serverFilePath, IconAlt, IconTitle, ACInstanceID, 0);
													}
													else if (EncodeNonCachableTags)
													{
														//
														// Add-on Experiment - move all processing to the Webclient
														// just pass the name and arguments back in th FPO
														// HTML encode and quote the name and AddonOptionString
														//
														Copy = ""
														+ ""
														+ "<!-- ADDON "
														+ "\"" + ACName + "\""
														+ ",\"" + AddonOptionStringHTMLEncoded + "\""
														+ ",\"" + ACInstanceID + "\""
														+ ",\"" + ACGuid + "\""
														+ " -->"
														+ "";
													}
													//
												}
												break;
											}
											case ACTypeImage:
											{
												//
												// ----- Image Tag, substitute image placeholder with the link from the REsource Library Record
												//
												if (EncodeImages)
												{
													Copy = "";
													ACAttrRecordID = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "RECORDID"));
													ACAttrWidth = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "WIDTH"));
													ACAttrHeight = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "HEIGHT"));
													ACAttrAlt = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "ALT"));
													ACAttrBorder = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "BORDER"));
													ACAttrLoop = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "LOOP"));
													ACAttrVSpace = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "VSPACE"));
													ACAttrHSpace = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "HSPACE"));
													ACAttrAlign = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "ALIGN"));
													//
													Models.Entity.libraryFilesModel file = Models.Entity.libraryFilesModel.create(cpCore, ACAttrRecordID);
													if (file != null)
													{
														Filename = file.Filename;
														Filename = genericController.vbReplace(Filename, "\\", "/");
														Filename = genericController.EncodeURL(Filename);
														Copy = Copy + "<img ID=\"AC,IMAGE,," + ACAttrRecordID + "\" src=\"" + genericController.getCdnFileLink(cpCore, Filename) + "\"";
														//
														if (ACAttrWidth == 0)
														{
															ACAttrWidth = file.pxWidth;
														}
														if (ACAttrWidth != 0)
														{
															Copy = Copy + " width=\"" + ACAttrWidth + "\"";
														}
														//
														if (ACAttrHeight == 0)
														{
															ACAttrHeight = file.pxHeight;
														}
														if (ACAttrHeight != 0)
														{
															Copy = Copy + " height=\"" + ACAttrHeight + "\"";
														}
														//
														if (ACAttrVSpace != 0)
														{
															Copy = Copy + " vspace=\"" + ACAttrVSpace + "\"";
														}
														//
														if (ACAttrHSpace != 0)
														{
															Copy = Copy + " hspace=\"" + ACAttrHSpace + "\"";
														}
														//
														if (!string.IsNullOrEmpty(ACAttrAlt))
														{
															Copy = Copy + " alt=\"" + ACAttrAlt + "\"";
														}
														//
														if (!string.IsNullOrEmpty(ACAttrAlign))
														{
															Copy = Copy + " align=\"" + ACAttrAlign + "\"";
														}
														//
														// no, 0 is an important value
														//If ACAttrBorder <> 0 Then
														Copy = Copy + " border=\"" + ACAttrBorder + "\"";
														//    End If
														//
														if (ACAttrLoop != 0)
														{
															Copy = Copy + " loop=\"" + ACAttrLoop + "\"";
														}
														//
														string attr = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "STYLE"));
														if (!string.IsNullOrEmpty(attr))
														{
															Copy = Copy + " style=\"" + attr + "\"";
														}
														//
														attr = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "CLASS"));
														if (!string.IsNullOrEmpty(attr))
														{
															Copy = Copy + " class=\"" + attr + "\"";
														}
														//
														Copy = Copy + ">";
													}
												}
											//
											//
												break;
											}
											case ACTypeDownload:
											{
												//
												// ----- substitute and anchored download image for the AC-Download tag
												//
												ACAttrRecordID = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "RECORDID"));
												ACAttrAlt = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "ALT"));
												//
												if (EncodeEditIcons)
												{
													//
													// Encoding the edit icons for the active editor form
													//
													IconIDControlString = "AC," + ACTypeDownload + ",," + ACAttrRecordID;
													IconImg = genericController.GetAddonIconImg(AdminURL, 16, 16, 0, true, IconIDControlString, "/ccLib/images/IconDownload3.gif", serverFilePath, "Download Icon with a link to a resource", "Renders as [Download Icon with a link to a resource]", ACInstanceID, 0);
													Copy = IconImg;
													//
													//Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Renders as a download icon"" id=""AC," & ACTypeDownload & ",," & ACAttrRecordID & """ src=""/ccLib/images/IconDownload3.GIF"">"
												}
												else if (EncodeImages)
												{
													//
													Models.Entity.libraryFilesModel file = Models.Entity.libraryFilesModel.create(cpCore, ACAttrRecordID);
													if (file != null)
													{
														if (string.IsNullOrEmpty(ACAttrAlt))
														{
															ACAttrAlt = genericController.encodeText(file.AltText);
														}
														Copy = "<a href=\"" + ProtocolHostLink + requestAppRootPath + cpCore.siteProperties.serverPageDefault + "?" + RequestNameDownloadID + "=" + ACAttrRecordID + "\" target=\"_blank\"><img src=\"" + ProtocolHostLink + "/ccLib/images/IconDownload3.gif\" width=\"16\" height=\"16\" border=\"0\" alt=\"" + ACAttrAlt + "\"></a>";
													}
												}
												break;
											}
											case ACTypeTemplateContent:
											{
												//
												// ----- Create Template Content
												//
												//ACName = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "NAME"))
												AddonOptionStringHTMLEncoded = "";
												addonOptionString = "";
												NotUsedID = 0;
												if (EncodeEditIcons)
												{
													//
													IconIDControlString = "AC," + ACType + "," + NotUsedID + "," + ACName + "," + AddonOptionStringHTMLEncoded;
													IconImg = genericController.GetAddonIconImg(AdminURL, 52, 64, 0, false, IconIDControlString, "/ccLib/images/ACTemplateContentIcon.gif", serverFilePath, "Template Page Content", "Renders as the Template Page Content", ACInstanceID, 0);
													Copy = IconImg;
													//
													//Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Page Content"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACTemplateContentIcon.gif"" WIDTH=52 HEIGHT=64>"
												}
												else if (EncodeNonCachableTags)
												{
													//
													// Add in the Content
													//
													Copy = fpoContentBox;
													//Copy = TemplateBodyContent
													//Copy = "{{" & ACTypeTemplateContent & "}}"
												}
												break;
											}
											case ACTypeTemplateText:
											{
												//
												// ----- Create Template Content
												//
												//ACName = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "NAME"))
												AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING");
												addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded);
												NotUsedID = 0;
												if (EncodeEditIcons)
												{
													//
													IconIDControlString = "AC," + ACType + "," + NotUsedID + "," + ACName + "," + AddonOptionStringHTMLEncoded;
													IconImg = genericController.GetAddonIconImg(AdminURL, 52, 52, 0, false, IconIDControlString, "/ccLib/images/ACTemplateTextIcon.gif", serverFilePath, "Template Text", "Renders as a Template Text Box", ACInstanceID, 0);
													Copy = IconImg;
													//
													//Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as Template Text"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACTemplateTextIcon.gif"" WIDTH=52 HEIGHT=52>"
												}
												else if (EncodeNonCachableTags)
												{
													//
													// Add in the Content Page
													//
													//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
													//test - encoding changed
													NewName = htmlController.getAddonOptionStringValue("new", addonOptionString);
													//NewName =  genericController.DecodeResponseVariable(getSimpleNameValue("new", AddonOptionString, "", "&"))
													TextName = htmlController.getAddonOptionStringValue("name", addonOptionString);
													//TextName = getSimpleNameValue("name", AddonOptionString)
													if (string.IsNullOrEmpty(TextName))
													{
														TextName = "Default";
													}
													Copy = "{{" + ACTypeTemplateText + "?name=" + genericController.encodeNvaArgument(TextName) + "&new=" + genericController.encodeNvaArgument(NewName) + "}}";
													// ***** can not add it here, if a web hit, it must be encoded from the web client for aggr objects
													//Copy = csv_GetContentCopy(TextName, "Copy Content", "", personalizationpeopleId)
												}
											//Case ACTypeDynamicMenu
											//    '
											//    ' ----- Create Template Menu
											//    '
											//    'ACName = KmaHTML.ElementAttribute(ElementPointer, "NAME")
											//    AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING")
											//    addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded)
											//    '
											//    ' test for illegal characters (temporary patch to get around not addonencoding during the addon replacement
											//    '
											//    Pos = genericController.vbInstr(1, addonOptionString, "menunew=", vbTextCompare)
											//    If Pos > 0 Then
											//        NewName = Mid(addonOptionString, Pos + 8)
											//        Dim IsOK As Boolean
											//        IsOK = (NewName = genericController.encodeNvaArgument(NewName))
											//        If Not IsOK Then
											//            addonOptionString = Left(addonOptionString, Pos - 1) & "MenuNew=" & genericController.encodeNvaArgument(NewName)
											//        End If
											//    End If
											//    NotUsedID = 0
											//    If EncodeEditIcons Then
											//        If genericController.vbInstr(1, AddonOptionStringHTMLEncoded, "menu=", vbTextCompare) <> 0 Then
											//            '
											//            ' Dynamic Menu
											//            '
											//            '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
											//            ' test - encoding changed
											//            TextName = cpCore.csv_GetAddonOptionStringValue("menu", addonOptionString)
											//            'TextName = cpcore.csv_GetAddonOption("Menu", AddonOptionString)
											//            '
											//            IconIDControlString = "AC," & ACType & "," & NotUsedID & "," & ACName & ",Menu=" & TextName & "[" & cpCore.csv_GetDynamicMenuACSelect() & "]&NewMenu="
											//            IconImg = genericController.GetAddonIconImg(AdminURL, 52, 52, 0, False, IconIDControlString, "/ccLib/images/ACDynamicMenuIcon.gif", serverFilePath, "Dynamic Menu", "Renders as a Dynamic Menu", ACInstanceID, 0)
											//            Copy = IconImg
											//            '
											//            'Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as a Dynamic Menu"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & ",Menu=" & TextName & "[" & csv_GetDynamicMenuACSelect & "]&NewMenu="" src=""/ccLib/images/ACDynamicMenuIcon.gif"" WIDTH=52 HEIGHT=52>"
											//        Else
											//            '
											//            ' Old Dynamic Menu - values are stored in the icon
											//            '
											//            IconIDControlString = "AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded
											//            IconImg = genericController.GetAddonIconImg(AdminURL, 52, 52, 0, False, IconIDControlString, "/ccLib/images/ACDynamicMenuIcon.gif", serverFilePath, "Dynamic Menu", "Renders as a Dynamic Menu", ACInstanceID, 0)
											//            Copy = IconImg
											//            '
											//            'Copy = "<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as a Dynamic Menu"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACDynamicMenuIcon.gif"" WIDTH=52 HEIGHT=52>"
											//        End If
											//    ElseIf EncodeNonCachableTags Then
											//        '
											//        ' Add in the Content Pag
											//        '
											//        Copy = "{{" & ACTypeDynamicMenu & "?" & addonOptionString & "}}"
											//    End If
												break;
											}
											case ACTypeWatchList:
											{
												//
												// ----- Formatting Tag
												//
												//
												// Content Watch replacement
												//   served by the web client because the
												//
												//UCaseACName = genericController.vbUCase(Trim(KmaHTML.ElementAttribute(ElementPointer, "NAME")))
												//ACName = encodeInitialCaps(UCaseACName)
												AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING");
												addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded);
												if (EncodeEditIcons)
												{
													//
													IconIDControlString = "AC," + ACType + "," + NotUsedID + "," + ACName + "," + AddonOptionStringHTMLEncoded;
													IconImg = genericController.GetAddonIconImg(AdminURL, 109, 10, 0, true, IconIDControlString, "/ccLib/images/ACWatchList.gif", serverFilePath, "Watch List", "Renders as the Watch List [" + ACName + "]", ACInstanceID, 0);
													Copy = IconImg;
													//
													//Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Watch List [" & ACName & "]"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACWatchList.GIF"">"
												}
												else if (EncodeNonCachableTags)
												{
													//
													Copy = "{{" + ACTypeWatchList + "?" + addonOptionString + "}}";
												}
												break;
											}
										}
										break;
								}
							}
							//
							// ----- Output the results
							//
							Stream.Add(Copy);
							ElementPointer = ElementPointer + 1;
						}
					}
					workingContent = Stream.Text;
					//
					// Add Contensive User Form if needed
					//
					if (FormCount == 0 && FormInputCount > 0)
					{
					}
					workingContent = ReplaceInstructions + workingContent;
					if (CSPeopleSet)
					{
						cpCore.db.csClose(CSPeople);
					}
					if (CSOrganizationSet)
					{
						cpCore.db.csClose(CSOrganization);
					}
					if (CSVisitorSet)
					{
						cpCore.db.csClose(CSVisitor);
					}
					if (CSVisitSet)
					{
						cpCore.db.csClose(CSVisit);
					}
					KmaHTML = null;
				}
				result = workingContent;
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return result;
		}



	}
}
