using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.BaseClasses;
using Controllers;

using Models.Entity;

namespace Controllers
{
	/// <summary>
	/// Tools used to assemble html document elements. This is not a storage for assembling a document (see docController)
	/// </summary>
	public partial class htmlController
	{

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
				ProcessACTags = ((EncodeCachableTags || EncodeNonCachableTags || EncodeImages || EncodeEditIcons)) & (workingContent.IndexOf("<AC ", System.StringComparison.OrdinalIgnoreCase) + 1 != 0);
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
															if (cpCore.db.csOk(CSPeople) & cpCore.db.cs_isFieldSupported(CSPeople, FieldName))
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
		//
		//========================================================================
		//   Decodes ActiveContent and EditIcons into <AC tags
		//       Detect IMG tags
		//           If IMG ID attribute is "AC,IMAGE,recordid", convert to AC Image tag
		//           If IMG ID attribute is "AC,DOWNLOAD,recordid", convert to AC Download tag
		//           If IMG ID attribute is "AC,ACType,ACFieldName,ACInstanceName,QueryStringArguments,AddonGuid", convert it to generic AC tag
		//   ACInstanceID - used to identify an AC tag on a page. Each instance of an AC tag must havea unique ACinstanceID
		//========================================================================
		//
		public string convertEditorResponseToActiveContent(string SourceCopy)
		{
			string result = "";
			try
			{
				string imageNewLink = null;
				string ACQueryString = "";
				string ACGuid = null;
				string ACIdentifier = null;
				string RecordFilename = null;
				string RecordFilenameNoExt = null;
				string RecordFilenameExt = null;
				int Ptr = 0;
				string ACInstanceID = null;
				string QSHTMLEncoded = null;
				int Pos = 0;
				string ImageSrcOriginal = null;
				string VirtualFilePathBad = null;
				string[] Paths = null;
				string ImageVirtualFilename = null;
				string ImageFilename = null;
				string ImageFilenameExt = null;
				string ImageFilenameNoExt = null;
				string[] SizeTest = null;
				string[] Styles = null;
				string StyleName = null;
				string StyleValue = null;
				int StyleValueInt = 0;
				string[] Style = null;
				string ImageVirtualFilePath = null;
				string RecordVirtualFilename = null;
				int RecordWidth = 0;
				int RecordHeight = 0;
				string RecordAltSizeList = null;
				string ImageAltSize = null;
				string NewImageFilename = null;
				htmlParserController DHTML = new htmlParserController(cpCore);
				int ElementPointer = 0;
				int ElementCount = 0;
				int AttributeCount = 0;
				string ACType = null;
				string ACFieldName = null;
				string ACInstanceName = null;
				int RecordID = 0;
				string ImageWidthText = null;
				string ImageHeightText = null;
				int ImageWidth = 0;
				int ImageHeight = 0;
				string ElementText = null;
				string ImageID = null;
				string ImageSrc = null;
				string ImageAlt = null;
				int ImageVSpace = 0;
				int ImageHSpace = 0;
				string ImageAlign = null;
				string ImageBorder = null;
				string ImageLoop = null;
				string ImageStyle = null;
				string[] IMageStyleArray = null;
				int ImageStyleArrayCount = 0;
				int ImageStyleArrayPointer = 0;
				string ImageStylePair = null;
				int PositionColon = 0;
				string ImageStylePairName = null;
				string ImageStylePairValue = null;
				stringBuilderLegacyController Stream = new stringBuilderLegacyController();
				string[] ImageIDArray = {};
				int ImageIDArrayCount = 0;
				string QueryString = null;
				string[] QSSplit = null;
				int QSPtr = 0;
				string serverFilePath = null;
				bool ImageAllowSFResize = false;
				imageEditController sf = null;
				//
				result = SourceCopy;
				if (!string.IsNullOrEmpty(result))
				{
					//
					// leave this in to make sure old <acform tags are converted back
					// new editor deals with <form, so no more converting
					//
					result = genericController.vbReplace(result, "<ACFORM>", "<FORM>");
					result = genericController.vbReplace(result, "<ACFORM ", "<FORM ");
					result = genericController.vbReplace(result, "</ACFORM>", "</form>");
					result = genericController.vbReplace(result, "</ACFORM ", "</FORM ");
					if (DHTML.Load(result))
					{
						result = "";
						ElementCount = DHTML.ElementCount;
						if (ElementCount > 0)
						{
							//
							// ----- Locate and replace IMG Edit icons with AC tags
							//
							Stream = new stringBuilderLegacyController();
							for (ElementPointer = 0; ElementPointer < ElementCount; ElementPointer++)
							{
								ElementText = DHTML.Text(ElementPointer).ToString();
								if (DHTML.IsTag(ElementPointer))
								{
									switch (genericController.vbUCase(DHTML.TagName(ElementPointer)))
									{
										case "FORM":
										//
										// User created form - add the attribute "Contensive=1"
										//
										// 5/14/2009 - DM said it is OK to remove UserResponseForm Processing
										//ElementText = genericController.vbReplace(ElementText, "<FORM", "<FORM ContensiveUserForm=1 ", vbTextCompare)
										break;
										case "IMG":
											AttributeCount = DHTML.ElementAttributeCount(ElementPointer);

											if (AttributeCount > 0)
											{
												ImageID = DHTML.ElementAttribute(ElementPointer, "id");
												ImageSrcOriginal = DHTML.ElementAttribute(ElementPointer, "src");
												VirtualFilePathBad = cpCore.serverConfig.appConfig.name + "/files/";
												serverFilePath = "/" + VirtualFilePathBad;
												if (ImageSrcOriginal.ToLower().Substring(0, VirtualFilePathBad.Length) == genericController.vbLCase(VirtualFilePathBad))
												{
													//
													// if the image is from the virtual file path, but the editor did not include the root path, add it
													//
													ElementText = genericController.vbReplace(ElementText, VirtualFilePathBad, "/" + VirtualFilePathBad, 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
													ImageSrcOriginal = genericController.vbReplace(ImageSrcOriginal, VirtualFilePathBad, "/" + VirtualFilePathBad, 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
												}
												ImageSrc = genericController.decodeHtml(ImageSrcOriginal);
												ImageSrc = DecodeURL(ImageSrc);
												//
												// problem with this case is if the addon icon image is from another site.
												// not sure how it happened, but I do not think the src of an addon edit icon
												// should be able to prevent the addon from executing.
												//
												ACIdentifier = "";
												ACType = "";
												ACFieldName = "";
												ACInstanceName = "";
												ACGuid = "";
												ImageIDArrayCount = 0;
												if (0 != genericController.vbInstr(1, ImageID, ","))
												{
													ImageIDArray = ImageID.Split(',');
													ImageIDArrayCount = ImageIDArray.GetUpperBound(0) + 1;
													if (ImageIDArrayCount > 5)
													{
														for (Ptr = 5; Ptr < ImageIDArrayCount; Ptr++)
														{
															ACGuid = ImageIDArray[Ptr];
															if ((ACGuid.Substring(0, 1) == "{") && (ACGuid.Substring(ACGuid.Length - 1) == "}"))
															{
																//
																// this element is the guid, go with it
																//
																break;
															}
															else if ((string.IsNullOrEmpty(ACGuid)) && (Ptr == (ImageIDArrayCount - 1)))
															{
																//
																// this is the last element, leave it as the guid
																//
																break;
															}
															else
															{
																//
																// not a valid guid, add it to element 4 and try the next
																//
																ImageIDArray[4] = ImageIDArray[4] + "," + ACGuid;
																ACGuid = "";
															}
														}
													}
													if (ImageIDArrayCount > 1)
													{
														ACIdentifier = genericController.vbUCase(ImageIDArray[0]);
														ACType = ImageIDArray[1];
														if (ImageIDArrayCount > 2)
														{
															ACFieldName = ImageIDArray[2];
															if (ImageIDArrayCount > 3)
															{
																ACInstanceName = ImageIDArray[3];
																if (ImageIDArrayCount > 4)
																{
																	ACQueryString = ImageIDArray[4];
																	//If ImageIDArrayCount > 5 Then
																	//    ACGuid = ImageIDArray(5)
																	//End If
																}
															}
														}
													}
												}
												if (ACIdentifier == "AC")
												{
													if (true)
													{
														if (true)
														{
															//
															// ----- Process AC Tag
															//
															ACInstanceID = DHTML.ElementAttribute(ElementPointer, "ACINSTANCEID");
															if (string.IsNullOrEmpty(ACInstanceID))
															{
																//GUIDGenerator = New guidClass
																ACInstanceID = Guid.NewGuid().ToString();
																//ACInstanceID = Guid.NewGuid.ToString()
															}
															ElementText = "";
															//----------------------------- change to ACType
															switch (genericController.vbUCase(ACType))
															{
																case "IMAGE":
																	//
																	// ----- AC Image, Decode Active Images to Resource Library references
																	//
																	if (ImageIDArrayCount >= 4)
																	{
																		RecordID = genericController.EncodeInteger(ACInstanceName);
																		ImageWidthText = DHTML.ElementAttribute(ElementPointer, "WIDTH");
																		ImageHeightText = DHTML.ElementAttribute(ElementPointer, "HEIGHT");
																		ImageAlt = encodeHTML(DHTML.ElementAttribute(ElementPointer, "Alt"));
																		ImageVSpace = genericController.EncodeInteger(DHTML.ElementAttribute(ElementPointer, "vspace"));
																		ImageHSpace = genericController.EncodeInteger(DHTML.ElementAttribute(ElementPointer, "hspace"));
																		ImageAlign = DHTML.ElementAttribute(ElementPointer, "Align");
																		ImageBorder = DHTML.ElementAttribute(ElementPointer, "BORDER");
																		ImageLoop = DHTML.ElementAttribute(ElementPointer, "LOOP");
																		ImageStyle = DHTML.ElementAttribute(ElementPointer, "STYLE");

																		if (!string.IsNullOrEmpty(ImageStyle))
																		{
																			//
																			// ----- Process styles, which override attributes
																			//
																			IMageStyleArray = ImageStyle.Split(';');
																			ImageStyleArrayCount = IMageStyleArray.GetUpperBound(0) + 1;
																			for (ImageStyleArrayPointer = 0; ImageStyleArrayPointer < ImageStyleArrayCount; ImageStyleArrayPointer++)
																			{
																				ImageStylePair = IMageStyleArray[ImageStyleArrayPointer].Trim(' ');
																				PositionColon = genericController.vbInstr(1, ImageStylePair, ":");
																				if (PositionColon > 1)
																				{
																					ImageStylePairName = (ImageStylePair.Substring(0, PositionColon - 1)).Trim(' ');
																					ImageStylePairValue = (ImageStylePair.Substring(PositionColon)).Trim(' ');
																					switch (genericController.vbUCase(ImageStylePairName))
																					{
																						case "WIDTH":
																							ImageStylePairValue = genericController.vbReplace(ImageStylePairValue, "px", "");
																							ImageWidthText = ImageStylePairValue;
																							break;
																						case "HEIGHT":
																							ImageStylePairValue = genericController.vbReplace(ImageStylePairValue, "px", "");
																							ImageHeightText = ImageStylePairValue;
																							break;
																					}
																					//If genericController.vbInstr(1, ImageStylePair, "WIDTH", vbTextCompare) = 1 Then
																					//    End If
																				}
																			}
																		}
																		ElementText = "<AC type=\"IMAGE\" ACInstanceID=\"" + ACInstanceID + "\" RecordID=\"" + RecordID + "\" Style=\"" + ImageStyle + "\" Width=\"" + ImageWidthText + "\" Height=\"" + ImageHeightText + "\" VSpace=\"" + ImageVSpace + "\" HSpace=\"" + ImageHSpace + "\" Alt=\"" + ImageAlt + "\" Align=\"" + ImageAlign + "\" Border=\"" + ImageBorder + "\" Loop=\"" + ImageLoop + "\">";
																	}
																	break;
																case ACTypeDownload:
																	//
																	// AC Download
																	//
																	if (ImageIDArrayCount >= 4)
																	{
																		RecordID = genericController.EncodeInteger(ACInstanceName);
																		ElementText = "<AC type=\"DOWNLOAD\" ACInstanceID=\"" + ACInstanceID + "\" RecordID=\"" + RecordID + "\">";
																	}
																	break;
																case ACTypeDate:
																	//
																	// Date
																	//
																	ElementText = "<AC type=\"" + ACTypeDate + "\">";
																	break;
																case ACTypeVisit:
																case ACTypeVisitor:
																case ACTypeMember:
																case ACTypeOrganization:
																case ACTypePersonalization:
																	//
																	// Visit, etc
																	//
																	ElementText = "<AC type=\"" + ACType + "\" ACInstanceID=\"" + ACInstanceID + "\" field=\"" + ACFieldName + "\">";
																	break;
																case ACTypeChildList:
																case ACTypeLanguage:
																	//
																	// ChildList, Language
																	//
																	if (ACInstanceName == "0")
																	{
																		ACInstanceName = genericController.GetRandomInteger().ToString();
																	}
																	ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\">";
																	break;
																case ACTypeAggregateFunction:
																	//
																	// Function
																	//
																	QueryString = "";
																	if (!string.IsNullOrEmpty(ACQueryString))
																	{
																		// I added this because single stepping through it I found it split on the & in &amp;
																		// I had added an Add-on and was saving
																		// I find it VERY odd that this could be the case
																		//
																		QSHTMLEncoded = genericController.encodeText(ACQueryString);
																		QueryString = genericController.decodeHtml(QSHTMLEncoded);
																		QSSplit = QueryString.Split('&');
																		for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++)
																		{
																			Pos = genericController.vbInstr(1, QSSplit[QSPtr], "[");
																			if (Pos > 0)
																			{
																				QSSplit[QSPtr] = QSSplit[QSPtr].Substring(0, Pos - 1);
																			}
																			QSSplit[QSPtr] = encodeHTML(QSSplit[QSPtr]);
																		}
																		QueryString = string.Join("&", QSSplit);
																	}
																	ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\" querystring=\"" + QueryString + "\" guid=\"" + ACGuid + "\">";
																	break;
																case ACTypeContact:
																case ACTypeFeedback:
																	//
																	// Contact and Feedback
																	//
																	ElementText = "<AC type=\"" + ACType + "\" ACInstanceID=\"" + ACInstanceID + "\">";
																	break;
																case ACTypeTemplateContent:
																case ACTypeTemplateText:
																	//
																	//
																	//
																	QueryString = "";
																	if (ImageIDArrayCount > 4)
																	{
																		QueryString = genericController.encodeText(ImageIDArray[4]);
																		QSSplit = QueryString.Split('&');
																		for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++)
																		{
																			QSSplit[QSPtr] = encodeHTML(QSSplit[QSPtr]);
																		}
																		QueryString = string.Join("&", QSSplit);

																	}
																	ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\" querystring=\"" + QueryString + "\">";
																	break;
																case ACTypeWatchList:
																	//
																	// Watch List
																	//
																	QueryString = "";
																	if (ImageIDArrayCount > 4)
																	{
																		QueryString = genericController.encodeText(ImageIDArray[4]);
																		QueryString = genericController.decodeHtml(QueryString);
																		QSSplit = QueryString.Split('&');
																		for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++)
																		{
																			QSSplit[QSPtr] = encodeHTML(QSSplit[QSPtr]);
																		}
																		QueryString = string.Join("&", QSSplit);
																	}
																	ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\" querystring=\"" + QueryString + "\">";
																	break;
																case ACTypeRSSLink:
																	//
																	// RSS Link
																	//
																	QueryString = "";
																	if (ImageIDArrayCount > 4)
																	{
																		QueryString = genericController.encodeText(ImageIDArray[4]);
																		QueryString = genericController.decodeHtml(QueryString);
																		QSSplit = QueryString.Split('&');
																		for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++)
																		{
																			QSSplit[QSPtr] = encodeHTML(QSSplit[QSPtr]);
																		}
																		QueryString = string.Join("&", QSSplit);
																	}
																	ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\" querystring=\"" + QueryString + "\">";
																	break;
																default:
																	//
																	// All others -- added querystring from element(4) to all others to cover the group access AC object
																	//
																	QueryString = "";
																	if (ImageIDArrayCount > 4)
																	{
																		QueryString = genericController.encodeText(ImageIDArray[4]);
																		QueryString = genericController.decodeHtml(QueryString);
																		QSSplit = QueryString.Split('&');
																		for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++)
																		{
																			QSSplit[QSPtr] = encodeHTML(QSSplit[QSPtr]);
																		}
																		QueryString = string.Join("&", QSSplit);
																	}
																	ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\" field=\"" + ACFieldName + "\" querystring=\"" + QueryString + "\">";
																	break;
															}
														}
													}
												}
												else if (genericController.vbInstr(1, ImageSrc, "cclibraryfiles", Microsoft.VisualBasic.Constants.vbTextCompare) != 0)
												{
													ImageAllowSFResize = cpCore.siteProperties.getBoolean("ImageAllowSFResize", true);
													if (ImageAllowSFResize && true)
													{
														//
														// if it is a real image, check for resize
														//
														Pos = genericController.vbInstr(1, ImageSrc, "cclibraryfiles", Microsoft.VisualBasic.Constants.vbTextCompare);
														if (Pos != 0)
														{
															ImageVirtualFilename = ImageSrc.Substring(Pos - 1);
															Paths = ImageVirtualFilename.Split('/');
															if (Paths.GetUpperBound(0) > 2)
															{
																if (genericController.vbLCase(Paths[1]) == "filename")
																{
																	RecordID = genericController.EncodeInteger(Paths[2]);
																	if (RecordID != 0)
																	{
																		ImageFilename = Paths[3];
																		ImageVirtualFilePath = genericController.vbReplace(ImageVirtualFilename, ImageFilename, "");
																		Pos = ImageFilename.LastIndexOf(".") + 1;
																		if (Pos > 0)
																		{
																			string ImageFilenameAltSize = "";
																			ImageFilenameExt = ImageFilename.Substring(Pos);
																			ImageFilenameNoExt = ImageFilename.Substring(0, Pos - 1);
																			Pos = ImageFilenameNoExt.LastIndexOf("-") + 1;
																			if (Pos > 0)
																			{
																				//
																				// ImageAltSize should be set from the width and height of the img tag,
																				// NOT from the actual width and height of the image file
																				// NOT from the suffix of the image filename
																				// ImageFilenameAltSize is used when the image has been resized, then 'reset' was hit
																				//  on the properties dialog before the save. The width and height come from this suffix
																				//
																				ImageFilenameAltSize = ImageFilenameNoExt.Substring(Pos);
																				SizeTest = ImageFilenameAltSize.Split('x');
																				if (SizeTest.GetUpperBound(0) != 1)
																				{
																					ImageFilenameAltSize = "";
																				}
																				else
																				{
																					if (genericController.vbIsNumeric(SizeTest[0]) & genericController.vbIsNumeric(SizeTest[1]))
																					{
																						ImageFilenameNoExt = ImageFilenameNoExt.Substring(0, Pos - 1);
																						//RecordVirtualFilenameNoExt = Mid(RecordVirtualFilename, 1, Pos - 1)
																					}
																					else
																					{
																						ImageFilenameAltSize = "";
																					}
																				}
																				//ImageFilenameNoExt = Mid(ImageFilenameNoExt, 1, Pos - 1)
																			}
																			if (genericController.vbInstr(1, sfImageExtList, ImageFilenameExt, Microsoft.VisualBasic.Constants.vbTextCompare) != 0)
																			{
																				//
																				// Determine ImageWidth and ImageHeight
																				//
																				ImageStyle = DHTML.ElementAttribute(ElementPointer, "style");
																				ImageWidth = genericController.EncodeInteger(DHTML.ElementAttribute(ElementPointer, "width"));
																				ImageHeight = genericController.EncodeInteger(DHTML.ElementAttribute(ElementPointer, "height"));
																				if (!string.IsNullOrEmpty(ImageStyle))
																				{
																					Styles = ImageStyle.Split(';');
																					for (Ptr = 0; Ptr <= Styles.GetUpperBound(0); Ptr++)
																					{
																						Style = Styles[Ptr].Split(':');
																						if (Style.GetUpperBound(0) > 0)
																						{
																							StyleName = genericController.vbLCase(Style[0].Trim(' '));
																							if (StyleName == "width")
																							{
																								StyleValue = genericController.vbLCase(Style[1].Trim(' '));
																								StyleValue = genericController.vbReplace(StyleValue, "px", "");
																								StyleValueInt = genericController.EncodeInteger(StyleValue);
																								if (StyleValueInt > 0)
																								{
																									ImageWidth = StyleValueInt;
																								}
																							}
																							else if (StyleName == "height")
																							{
																								StyleValue = genericController.vbLCase(Style[1].Trim(' '));
																								StyleValue = genericController.vbReplace(StyleValue, "px", "");
																								StyleValueInt = genericController.EncodeInteger(StyleValue);
																								if (StyleValueInt > 0)
																								{
																									ImageHeight = StyleValueInt;
																								}
																							}
																						}
																					}
																				}
																				//
																				// Get the record values
																				//
																				Models.Entity.libraryFilesModel file = Models.Entity.libraryFilesModel.create(cpCore, RecordID);
																				if (file != null)
																				{
																					RecordVirtualFilename = file.Filename;
																					RecordWidth = file.pxWidth;
																					RecordHeight = file.pxHeight;
																					RecordAltSizeList = file.AltSizeList;
																					RecordFilename = RecordVirtualFilename;
																					Pos = RecordVirtualFilename.LastIndexOf("/") + 1;
																					if (Pos > 0)
																					{
																						RecordFilename = RecordVirtualFilename.Substring(Pos);
																					}
																					RecordFilenameExt = "";
																					RecordFilenameNoExt = RecordFilename;
																					Pos = RecordFilenameNoExt.LastIndexOf(".") + 1;
																					if (Pos > 0)
																					{
																						RecordFilenameExt = RecordFilenameNoExt.Substring(Pos);
																						RecordFilenameNoExt = RecordFilenameNoExt.Substring(0, Pos - 1);
																					}
																					//
																					// if recordwidth or height are missing, get them from the file
																					//
																					if (RecordWidth == 0 || RecordHeight == 0)
																					{
																						sf = new imageEditController();
																						if (sf.load(genericController.convertCdnUrlToCdnPathFilename(ImageVirtualFilename)))
																						{
																							file.pxWidth = sf.width;
																							file.pxHeight = sf.height;
																							file.save(cpCore);
																						}
																						sf.Dispose();
																						sf = null;
																					}
																					//
																					// continue only if we have record width and height
																					//
																					if (RecordWidth != 0 & RecordHeight != 0)
																					{
																						//
																						// set ImageWidth and ImageHeight if one of them is missing
																						//
																						if ((ImageWidth == RecordWidth) && (ImageHeight == 0))
																						{
																							//
																							// Image only included width, set default height
																							//
																							ImageHeight = RecordHeight;
																						}
																						else if ((ImageHeight == RecordHeight) && (ImageWidth == 0))
																						{
																							//
																							// Image only included height, set default width
																							//
																							ImageWidth = RecordWidth;
																						}
																						else if ((ImageHeight == 0) && (ImageWidth == 0))
																						{
																							//
																							// Image has no width or height, default both
																							// This happens when you hit 'reset' on the image properties dialog
																							//
																							sf = new imageEditController();
																							if (sf.load(genericController.convertCdnUrlToCdnPathFilename(ImageVirtualFilename)))
																							{
																								ImageWidth = sf.width;
																								ImageHeight = sf.height;
																							}
																							sf.Dispose();
																							sf = null;
																							if ((ImageHeight == 0) && (ImageWidth == 0) && (!string.IsNullOrEmpty(ImageFilenameAltSize)))
																							{
																								Pos = genericController.vbInstr(1, ImageFilenameAltSize, "x");
																								if (Pos != 0)
																								{
																									ImageWidth = genericController.EncodeInteger(ImageFilenameAltSize.Substring(0, Pos - 1));
																									ImageHeight = genericController.EncodeInteger(ImageFilenameAltSize.Substring(Pos));
																								}
																							}
																							if (ImageHeight == 0 && ImageWidth == 0)
																							{
																								ImageHeight = RecordHeight;
																								ImageWidth = RecordWidth;
																							}
																						}
																						//
																						// Set the ImageAltSize to what was requested from the img tag
																						// if the actual image is a few rounding-error pixels off does not matter
																						// if either is 0, let altsize be 0, set real value for image height/width
																						//
																						ImageAltSize = ImageWidth.ToString() + "x" + ImageHeight.ToString();
																						//
																						// determine if we are OK, or need to rebuild
																						//
																						if ((RecordVirtualFilename == (ImageVirtualFilePath + ImageFilename)) && ((RecordWidth == ImageWidth) || (RecordHeight == ImageHeight)))
																						{
																							//
																							// OK
																							// this is the raw image
																							// image matches record, and the sizes are the same
																							//
																							RecordVirtualFilename = RecordVirtualFilename;
																						}
																						else if ((RecordVirtualFilename == ImageVirtualFilePath + ImageFilenameNoExt + "." + ImageFilenameExt) && (RecordAltSizeList.IndexOf(ImageAltSize, System.StringComparison.OrdinalIgnoreCase) + 1 != 0))
																						{
																							//
																							// OK
																							// resized image, and altsize is in the list - go with resized image name
																							//
																							NewImageFilename = ImageFilenameNoExt + "-" + ImageAltSize + "." + ImageFilenameExt;
																							// images included in email have spaces that must be converted to "%20" or they 404
																							imageNewLink = genericController.EncodeURL(genericController.getCdnFileLink(cpCore, ImageVirtualFilePath) + NewImageFilename);
																							ElementText = genericController.vbReplace(ElementText, ImageSrcOriginal, encodeHTML(imageNewLink));
																						}
																						else if ((RecordWidth < ImageWidth) || (RecordHeight < ImageHeight))
																						{
																							//
																							// OK
																							// reize image larger then original - go with it as is
																							//
																							// images included in email have spaces that must be converted to "%20" or they 404
																							ElementText = genericController.vbReplace(ElementText, ImageSrcOriginal, encodeHTML(genericController.EncodeURL(genericController.getCdnFileLink(cpCore, RecordVirtualFilename))));
																						}
																						else
																						{
																							//
																							// resized image - create NewImageFilename (and add new alt size to the record)
																							//
																							if (RecordWidth == ImageWidth && RecordHeight == ImageHeight)
																							{
																								//
																								// set back to Raw image untouched, use the record image filename
																								//
																								ElementText = ElementText;
																								//ElementText = genericController.vbReplace(ElementText, ImageVirtualFilename, RecordVirtualFilename)
																							}
																							else
																							{
																								//
																								// Raw image filename in content, but it is resized, switch to an alternate size
																								//
																								NewImageFilename = RecordFilename;
																								if ((ImageWidth == 0) || (ImageHeight == 0) || (Environment.NewLine + RecordAltSizeList + Environment.NewLine.IndexOf(Environment.NewLine + ImageAltSize + Environment.NewLine) + 1 == 0))
																								{
																									//
																									// Alt image has not been built
																									//
																									sf = new imageEditController();
																									if (!sf.load(genericController.convertCdnUrlToCdnPathFilename(RecordVirtualFilename)))
																									{
																										//
																										// image load failed, use raw filename
																										//
																										throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "Error while loading image to resize, [" & RecordVirtualFilename & "]", "dll", "cpCoreClass", "DecodeAciveContent", Err.Number, Err.Source, Err.Description, False, True, "")
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
																										Microsoft.VisualBasic.Information.Err().Clear();
																										NewImageFilename = ImageFilename;
																									}
																									else
																									{
																										//
																										//
																										//
																										RecordWidth = sf.width;
																										RecordHeight = sf.height;
																										if (ImageWidth == 0)
																										{
																											//
																											//
																											//
																											sf.height = ImageHeight;
																										}
																										else if (ImageHeight == 0)
																										{
																											//
																											//
																											//
																											sf.width = ImageWidth;
																										}
																										else if (RecordHeight == ImageHeight)
																										{
																											//
																											// change the width
																											//
																											sf.width = ImageWidth;
																										}
																										else
																										{
																											//
																											// change the height
																											//
																											sf.height = ImageHeight;
																										}
																										//
																										// if resized only width or height, set the other
																										//
																										if (ImageWidth == 0)
																										{
																											ImageWidth = sf.width;
																											ImageAltSize = ImageWidth.ToString() + "x" + ImageHeight.ToString();
																										}
																										if (ImageHeight == 0)
																										{
																											ImageHeight = sf.height;
																											ImageAltSize = ImageWidth.ToString() + "x" + ImageHeight.ToString();
																										}
																										//
																										// set HTML attributes so image properties will display
																										//
																										if (genericController.vbInstr(1, ElementText, "height=", Microsoft.VisualBasic.Constants.vbTextCompare) == 0)
																										{
																											ElementText = genericController.vbReplace(ElementText, ">", " height=\"" + ImageHeight + "\">");
																										}
																										if (genericController.vbInstr(1, ElementText, "width=", Microsoft.VisualBasic.Constants.vbTextCompare) == 0)
																										{
																											ElementText = genericController.vbReplace(ElementText, ">", " width=\"" + ImageWidth + "\">");
																										}
																										//
																										// Save new file
																										//
																										NewImageFilename = RecordFilenameNoExt + "-" + ImageAltSize + "." + RecordFilenameExt;
																										sf.save(genericController.convertCdnUrlToCdnPathFilename(ImageVirtualFilePath + NewImageFilename));
																										//
																										// Update image record
																										//
																										RecordAltSizeList = RecordAltSizeList + Environment.NewLine + ImageAltSize;
																									}
																								}
																								//
																								// Change the image src to the AltSize
																								ElementText = genericController.vbReplace(ElementText, ImageSrcOriginal, encodeHTML(genericController.EncodeURL(genericController.getCdnFileLink(cpCore, ImageVirtualFilePath) + NewImageFilename)));
																							}
																						}
																					}
																				}
																				file.save(cpCore);
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
											break;
									}
								}
								Stream.Add(ElementText);
							}
						}
						result = Stream.Text;
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
			//
		}
		//
		//========================================================================
		// Modify a string to be printed through the HTML stream
		//   convert carriage returns ( 0x10 ) to <br>
		//   remove linefeeds ( 0x13 )
		//========================================================================
		//
		public string convertCRLFToHtmlBreak(object Source)
		{
				string tempconvertCRLFToHtmlBreak = null;
			try
			{
				//
				string iSource;
				//
				iSource = genericController.encodeText(Source);
				tempconvertCRLFToHtmlBreak = "";
				if (!string.IsNullOrEmpty(iSource))
				{
					tempconvertCRLFToHtmlBreak = iSource;
					tempconvertCRLFToHtmlBreak = genericController.vbReplace(tempconvertCRLFToHtmlBreak, "\r", "");
					tempconvertCRLFToHtmlBreak = genericController.vbReplace(tempconvertCRLFToHtmlBreak, "\n", "<br >");
				}
				return tempconvertCRLFToHtmlBreak;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_EncodeCRLF")
			return tempconvertCRLFToHtmlBreak;
		}


	}
}
