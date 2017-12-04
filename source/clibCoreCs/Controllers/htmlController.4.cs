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
																					if ((genericController.vbIsNumeric(SizeTest[0]) & genericController.vbIsNumeric(SizeTest[1])) != 0)
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
		//
		//========================================================================
		//   Encodes characters to be compatibile with HTML
		//   i.e. it converts the equation 5 > 6 to th sequence "5 &gt; 6"
		//
		//   convert carriage returns ( 0x10 ) to <br >
		//   remove linefeeds ( 0x13 )
		//========================================================================
		//
		public string main_encodeHTML(object Source)
		{
			try
			{
				//
				return encodeHTML(genericController.encodeText(Source));
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("EncodeHTML")
		}
		//
		//========================================================================
		//   Convert an HTML source to a text equivelent
		//
		//       converts CRLF to <br>
		//       encodes reserved HTML characters to their equivalent
		//========================================================================
		//
		public string convertTextToHTML(string Source)
		{
			return convertCRLFToHtmlBreak(encodeHTML(Source));
		}
		//
		//========================================================================
		// ----- Encode Active Content AI
		//========================================================================
		//
		public string convertHTMLToText(string Source)
		{
			try
			{
				htmlToTextControllers Decoder = new htmlToTextControllers(cpCore);
				return Decoder.convert(Source);
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Unexpected exception");
			}
		}
		//
		//===============================================================================================================================
		//   Get Addon Selector
		//
		//   The addon selector is the string sent out with the content in edit-mode. In the editor, it is converted by javascript
		//   to the popup window that selects instance options. It is in this format:
		//
		//   Select (creates a list of names in a select box, returns the selected name)
		//       name=currentvalue[optionname0:optionvalue0|optionname1:optionvalue1|...]
		//   CheckBox (creates a list of names in checkboxes, and returns the selected names)
		//===============================================================================================================================
		//
		public string getAddonSelector(string SrcOptionName, string InstanceOptionValue_AddonEncoded, string SrcOptionValueSelector)
		{
			string result = "";
			try
			{
				//
				const string ACFunctionList = "List";
				const string ACFunctionList1 = "selectname";
				const string ACFunctionList2 = "listname";
				const string ACFunctionList3 = "selectcontentname";
				const string ACFunctionListID = "ListID";
				const string ACFunctionListFields = "ListFields";
				//
				int CID = 0;
				bool IsContentList = false;
				bool IsListField = false;
				string Choice = null;
				string[] Choices = null;
				int ChoiceCnt = 0;
				int Ptr = 0;
				bool IncludeID = false;
				int FnLen = 0;
				int RecordID = 0;
				int CS = 0;
				string ContentName = null;
				int Pos = 0;
				string list = null;
				string FnArgList = null;
				string[] FnArgs = null;
				int FnArgCnt = 0;
				string ContentCriteria = null;
				string RecordName = null;
				string SrcSelectorInner = null;
				string SrcSelectorSuffix = string.Empty;
				object[,] Cell = null;
				int RowCnt = 0;
				int RowPtr = 0;
				string SrcSelector = SrcOptionValueSelector.Trim(' ');
				//
				SrcSelectorInner = SrcSelector;
				int PosLeft = genericController.vbInstr(1, SrcSelector, "[");
				if (PosLeft != 0)
				{
					int PosRight = genericController.vbInstr(1, SrcSelector, "]");
					if (PosRight != 0)
					{
						if (PosRight < SrcSelector.Length)
						{
							SrcSelectorSuffix = SrcSelector.Substring(PosRight);
						}
						SrcSelector = (SrcSelector.Substring(PosLeft - 1, PosRight - PosLeft + 1)).Trim(' ');
						SrcSelectorInner = (SrcSelector.Substring(1, SrcSelector.Length - 2)).Trim(' ');
					}
				}
				list = "";
				//
				// Break SrcSelectorInner up into individual choices to detect functions
				//
				if (!string.IsNullOrEmpty(SrcSelectorInner))
				{
					Choices = SrcSelectorInner.Split('|');
					ChoiceCnt = Choices.GetUpperBound(0) + 1;
					for (Ptr = 0; Ptr < ChoiceCnt; Ptr++)
					{
						Choice = Choices[Ptr];
						IsContentList = false;
						IsListField = false;
						//
						// List Function (and all the indecision that went along with it)
						//
						Pos = 0;
						if (Pos == 0)
						{
							Pos = genericController.vbInstr(1, Choice, ACFunctionList1 + "(", Microsoft.VisualBasic.Constants.vbTextCompare);
							if (Pos > 0)
							{
								IsContentList = true;
								IncludeID = false;
								FnLen = ACFunctionList1.Length;
							}
						}
						if (Pos == 0)
						{
							Pos = genericController.vbInstr(1, Choice, ACFunctionList2 + "(", Microsoft.VisualBasic.Constants.vbTextCompare);
							if (Pos > 0)
							{
								IsContentList = true;
								IncludeID = false;
								FnLen = ACFunctionList2.Length;
							}
						}
						if (Pos == 0)
						{
							Pos = genericController.vbInstr(1, Choice, ACFunctionList3 + "(", Microsoft.VisualBasic.Constants.vbTextCompare);
							if (Pos > 0)
							{
								IsContentList = true;
								IncludeID = false;
								FnLen = ACFunctionList3.Length;
							}
						}
						if (Pos == 0)
						{
							Pos = genericController.vbInstr(1, Choice, ACFunctionListID + "(", Microsoft.VisualBasic.Constants.vbTextCompare);
							if (Pos > 0)
							{
								IsContentList = true;
								IncludeID = true;
								FnLen = ACFunctionListID.Length;
							}
						}
						if (Pos == 0)
						{
							Pos = genericController.vbInstr(1, Choice, ACFunctionList + "(", Microsoft.VisualBasic.Constants.vbTextCompare);
							if (Pos > 0)
							{
								IsContentList = true;
								IncludeID = false;
								FnLen = ACFunctionList.Length;
							}
						}
						if (Pos == 0)
						{
							Pos = genericController.vbInstr(1, Choice, ACFunctionListFields + "(", Microsoft.VisualBasic.Constants.vbTextCompare);
							if (Pos > 0)
							{
								IsListField = true;
								IncludeID = false;
								FnLen = ACFunctionListFields.Length;
							}
						}
						//
						if (Pos > 0)
						{
							//
							FnArgList = (Choice.Substring((Pos + FnLen) - 1)).Trim(' ');
							ContentName = "";
							ContentCriteria = "";
							if ((FnArgList.Substring(0, 1) == "(") && (FnArgList.Substring(FnArgList.Length - 1) == ")"))
							{
								//
								// set ContentName and ContentCriteria from argument list
								//
								FnArgList = FnArgList.Substring(1, FnArgList.Length - 2);
								FnArgs = genericController.SplitDelimited(FnArgList, ",");
								FnArgCnt = FnArgs.GetUpperBound(0) + 1;
								if (FnArgCnt > 0)
								{
									ContentName = FnArgs[0].Trim(' ');
									if ((ContentName.Substring(0, 1) == "\"") && (ContentName.Substring(ContentName.Length - 1) == "\""))
									{
										ContentName = (ContentName.Substring(1, ContentName.Length - 2)).Trim(' ');
									}
									else if ((ContentName.Substring(0, 1) == "'") && (ContentName.Substring(ContentName.Length - 1) == "'"))
									{
										ContentName = (ContentName.Substring(1, ContentName.Length - 2)).Trim(' ');
									}
								}
								if (FnArgCnt > 1)
								{
									ContentCriteria = FnArgs[1].Trim(' ');
									if ((ContentCriteria.Substring(0, 1) == "\"") && (ContentCriteria.Substring(ContentCriteria.Length - 1) == "\""))
									{
										ContentCriteria = (ContentCriteria.Substring(1, ContentCriteria.Length - 2)).Trim(' ');
									}
									else if ((ContentCriteria.Substring(0, 1) == "'") && (ContentCriteria.Substring(ContentCriteria.Length - 1) == "'"))
									{
										ContentCriteria = (ContentCriteria.Substring(1, ContentCriteria.Length - 2)).Trim(' ');
									}
								}
							}
							CS = -1;
							if (IsContentList)
							{
								//
								// ContentList - Open the Content and build the options from the names
								//
								if (!string.IsNullOrEmpty(ContentCriteria))
								{
									CS = cpCore.db.csOpen(ContentName, ContentCriteria, "name",,,,, "ID,Name");
								}
								else
								{
									CS = cpCore.db.csOpen(ContentName,, "name",,,,, "ID,Name");
								}
							}
							else if (IsListField)
							{
								//
								// ListField
								//
								CID = Models.Complex.cdefModel.getContentId(cpCore, ContentName);
								if (CID > 0)
								{
									CS = cpCore.db.csOpen("Content Fields", "Contentid=" + CID, "name",,,,, "ID,Name");
								}
							}

							if (cpCore.db.csOk(CS))
							{
								Cell = cpCore.db.cs_getRows(CS);
								RowCnt = Cell.GetUpperBound(1) + 1;
								for (RowPtr = 0; RowPtr < RowCnt; RowPtr++)
								{
									//
									RecordName = genericController.encodeText(Cell[1, RowPtr]);
									RecordName = genericController.vbReplace(RecordName, Environment.NewLine, " ");
									RecordID = genericController.EncodeInteger(Cell[0, RowPtr]);
									if (string.IsNullOrEmpty(RecordName))
									{
										RecordName = "record " + RecordID;
									}
									else if (RecordName.Length > 50)
									{
										RecordName = RecordName.Substring(0, 50) + "...";
									}
									RecordName = genericController.encodeNvaArgument(RecordName);
									list = list + "|" + RecordName;
									if (IncludeID)
									{
										list = list + ":" + RecordID;
									}
								}
							}
							cpCore.db.csClose(CS);
						}
						else
						{
							//
							// choice is not a function, just add the choice back to the list
							//
							list = list + "|" + Choices[Ptr];
						}
					}
					if (!string.IsNullOrEmpty(list))
					{
						list = list.Substring(1);
					}
				}
				//
				// Build output string
				//
				//csv_result = encodeNvaArgument(SrcOptionName)
				result = encodeHTML(genericController.encodeNvaArgument(SrcOptionName)) + "=";
				if (!string.IsNullOrEmpty(InstanceOptionValue_AddonEncoded))
				{
					result = result + encodeHTML(InstanceOptionValue_AddonEncoded);
				}
				if (string.IsNullOrEmpty(SrcSelectorSuffix) && string.IsNullOrEmpty(list))
				{
					//
					// empty list with no suffix, return with name=value
					//
				}
				else if (genericController.vbLCase(SrcSelectorSuffix) == "resourcelink")
				{
					//
					// resource link, exit with empty list
					//
					result = result + "[]ResourceLink";
				}
				else
				{
					//
					//
					//
					result = result + "[" + list + "]" + SrcSelectorSuffix;
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//
		//========================================================================
		// main_Get an HTML Form text input (or text area)
		//
		public string getFormInputHTML(string htmlName, string DefaultValue = "", string styleHeight = "", string styleWidth = "", bool readOnlyfield = false, bool allowActiveContent = false, string addonListJSON = "", string styleList = "", string styleOptionList = "", bool allowResourceLibrary = false)
		{
			string returnHtml = "";
			try
			{
				string FieldTypeDefaultEditorAddonIdList = editorController.getFieldTypeDefaultEditorAddonIdList(cpCore);
				string[] FieldTypeDefaultEditorAddonIds = FieldTypeDefaultEditorAddonIdList.Split(',');
				int FieldTypeDefaultEditorAddonId = genericController.EncodeInteger(FieldTypeDefaultEditorAddonIds[FieldTypeIdHTML]);
				if (FieldTypeDefaultEditorAddonId == 0)
				{
					//
					//    use default wysiwyg
					returnHtml = html_GetFormInputTextExpandable2(htmlName, DefaultValue);
				}
				else
				{
					//
					// use addon editor
					Dictionary<string, string> arguments = new Dictionary<string, string>();
					arguments.Add("editorName", htmlName);
					arguments.Add("editorValue", DefaultValue);
					arguments.Add("editorFieldType", FieldTypeIdHTML.ToString());
					arguments.Add("editorReadOnly", readOnlyfield.ToString());
					arguments.Add("editorWidth", styleWidth);
					arguments.Add("editorHeight", styleHeight);
					arguments.Add("editorAllowResourceLibrary", allowResourceLibrary.ToString());
					arguments.Add("editorAllowActiveContent", allowActiveContent.ToString());
					arguments.Add("editorAddonList", addonListJSON);
					arguments.Add("editorStyles", styleList);
					arguments.Add("editorStyleOptions", styleOptionList);
					returnHtml = cpCore.addon.execute(addonModel.create(cpCore, FieldTypeDefaultEditorAddonId), new CPUtilsBaseClass.addonExecuteContext()
					{
						addonType = CPUtilsBaseClass.addonContext.ContextEditor,
						instanceArguments = arguments
					});
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
		//========================================================================
		// ----- Process the reply from the Tools Panel form
		//========================================================================
		//
		public void processFormToolsPanel(string legacyFormSn = "")
		{
			try
			{
				string Button = null;
				string username = null;
				//
				// ----- Read in and save the Member profile values from the tools panel
				//
				if (cpCore.doc.authContext.user.id > 0)
				{
					if (!(cpCore.doc.debug_iUserError != ""))
					{
						Button = cpCore.docProperties.getText(legacyFormSn + "mb");
						switch (Convert.ToInt32(Button))
						{
							case ButtonLogout:
								//
								// Logout - This can only come from the Horizonal Tool Bar
								//
								cpCore.doc.authContext.logout(cpCore);
								break;
							case ButtonLogin:
								//
								// Login - This can only come from the Horizonal Tool Bar
								//
								Controllers.loginController.processFormLoginDefault(cpCore);
								break;
							case ButtonApply:
								//
								// Apply
								//
								username = cpCore.docProperties.getText(legacyFormSn + "username");
								if (!string.IsNullOrEmpty(username))
								{
									Controllers.loginController.processFormLoginDefault(cpCore);
								}
								//
								// ----- AllowAdminLinks
								//
								cpCore.visitProperty.setProperty("AllowEditing", genericController.encodeText(cpCore.docProperties.getBoolean(legacyFormSn + "AllowEditing")));
								//
								// ----- Quick Editor
								//
								cpCore.visitProperty.setProperty("AllowQuickEditor", genericController.encodeText(cpCore.docProperties.getBoolean(legacyFormSn + "AllowQuickEditor")));
								//
								// ----- Advanced Editor
								//
								cpCore.visitProperty.setProperty("AllowAdvancedEditor", genericController.encodeText(cpCore.docProperties.getBoolean(legacyFormSn + "AllowAdvancedEditor")));
								//
								// ----- Allow Workflow authoring Render Mode - Visit Property
								//
								cpCore.visitProperty.setProperty("AllowWorkflowRendering", genericController.encodeText(cpCore.docProperties.getBoolean(legacyFormSn + "AllowWorkflowRendering")));
								//
								// ----- developer Only parts
								//
								cpCore.visitProperty.setProperty("AllowDebugging", genericController.encodeText(cpCore.docProperties.getBoolean(legacyFormSn + "AllowDebugging")));
								break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
		}
		//
		//========================================================================
		// -----
		//========================================================================
		//
		public void processAddonSettingsEditor()
		{
			//
			string constructor = null;
			bool ParseOK = false;
			int PosNameStart = 0;
			int PosNameEnd = 0;
			string AddonName = null;
			//Dim CSAddon As Integer
			int OptionPtr = 0;
			string ArgValueAddonEncoded = null;
			int OptionCnt = 0;
			bool needToClearCache = false;
			string[] ConstructorSplit = null;
			int Ptr = 0;
			string[] Arg = null;
			string ArgName = null;
			string ArgValue = null;
			string AddonOptionConstructor = string.Empty;
			string addonOption_String = string.Empty;
			int fieldType = 0;
			string Copy = string.Empty;
			string MethodName = null;
			int RecordID = 0;
			string FieldName = null;
			string ACInstanceID = null;
			string ContentName = null;
			int CS = 0;
			int PosACInstanceID = 0;
			int PosStart = 0;
			int PosIDStart = 0;
			int PosIDEnd = 0;
			//
			MethodName = "main_ProcessAddonSettingsEditor()";
			//
			ContentName = cpCore.docProperties.getText("ContentName");
			RecordID = cpCore.docProperties.getInteger("RecordID");
			FieldName = cpCore.docProperties.getText("FieldName");
			ACInstanceID = cpCore.docProperties.getText("ACInstanceID");
			bool FoundAddon = false;
			if (ACInstanceID == PageChildListInstanceID)
			{
				//
				// ----- Page Content Child List Add-on
				//
				if (RecordID != 0)
				{
					addonModel addon = cpCore.addonCache.getAddonById(cpCore.siteProperties.childListAddonID);
					if (addon != null)
					{
						FoundAddon = true;
						AddonOptionConstructor = addon.ArgumentList;
						AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, Environment.NewLine, "\r");
						AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\n", "\r");
						AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\r", Environment.NewLine);
						if (true)
						{
							if (!string.IsNullOrEmpty(AddonOptionConstructor))
							{
								AddonOptionConstructor = AddonOptionConstructor + Environment.NewLine;
							}
							if (addon.IsInline)
							{
								AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Inline;
							}
							else
							{
								AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Block;
							}
						}

						ConstructorSplit = Microsoft.VisualBasic.Strings.Split(AddonOptionConstructor, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
						AddonOptionConstructor = "";
						//
						// main_Get all responses from current Argument List and build new addonOption_String
						//
						for (Ptr = 0; Ptr <= ConstructorSplit.GetUpperBound(0); Ptr++)
						{
							Arg = ConstructorSplit[Ptr].Split('=');
							ArgName = Arg[0];
							OptionCnt = cpCore.docProperties.getInteger(ArgName + "CheckBoxCnt");
							if (OptionCnt > 0)
							{
								ArgValueAddonEncoded = "";
								for (OptionPtr = 0; OptionPtr < OptionCnt; OptionPtr++)
								{
									ArgValue = cpCore.docProperties.getText(ArgName + OptionPtr);
									if (!string.IsNullOrEmpty(ArgValue))
									{
										ArgValueAddonEncoded = ArgValueAddonEncoded + "," + genericController.encodeNvaArgument(ArgValue);
									}
								}
								if (!string.IsNullOrEmpty(ArgValueAddonEncoded))
								{
									ArgValueAddonEncoded = ArgValueAddonEncoded.Substring(1);
								}
							}
							else
							{
								ArgValue = cpCore.docProperties.getText(ArgName);
								ArgValueAddonEncoded = genericController.encodeNvaArgument(ArgValue);
							}
							addonOption_String = addonOption_String + "&" + genericController.encodeNvaArgument(ArgName) + "=" + ArgValueAddonEncoded;
						}
						if (!string.IsNullOrEmpty(addonOption_String))
						{
							addonOption_String = addonOption_String.Substring(1);
						}

					}
					cpCore.db.executeQuery("update ccpagecontent set ChildListInstanceOptions=" + cpCore.db.encodeSQLText(addonOption_String) + " where id=" + RecordID);
					needToClearCache = true;
					//CS = main_OpenCSContentRecord("page content", RecordID)
					//If app.csv_IsCSOK(CS) Then
					//    Call app.SetCS(CS, "ChildListInstanceOptions", addonOption_String)
					//    needToClearCache = True
					//End If
					//Call app.closeCS(CS)
				}
			}
			else if ((ACInstanceID == "-2") && (!string.IsNullOrEmpty(FieldName)))
			{
				//
				// ----- Admin Addon, ACInstanceID=-2, FieldName=AddonName
				//
				AddonName = FieldName;
				FoundAddon = false;
				addonModel addon = cpCore.addonCache.getAddonByName(AddonName);
				if (addon != null)
				{
					FoundAddon = true;
					AddonOptionConstructor = addon.ArgumentList;
					AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, Environment.NewLine, "\r");
					AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\n", "\r");
					AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\r", Environment.NewLine);
					if (!string.IsNullOrEmpty(AddonOptionConstructor))
					{
						AddonOptionConstructor = AddonOptionConstructor + Environment.NewLine;
					}
					if (genericController.EncodeBoolean(addon.IsInline))
					{
						AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Inline;
					}
					else
					{
						AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Block;
					}
				}
				if (!FoundAddon)
				{
					//
					// Hardcoded Addons
					//
					switch (genericController.vbLCase(AddonName))
					{
						case "block text":
							FoundAddon = true;
							AddonOptionConstructor = AddonOptionConstructor_ForBlockText;
							break;
						case "":
						break;
					}
				}
				if (FoundAddon)
				{
					ConstructorSplit = Microsoft.VisualBasic.Strings.Split(AddonOptionConstructor, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
					addonOption_String = "";
					//
					// main_Get all responses from current Argument List
					//
					for (Ptr = 0; Ptr <= ConstructorSplit.GetUpperBound(0); Ptr++)
					{
						string nvp = ConstructorSplit[Ptr].Trim(' ');
						if (!string.IsNullOrEmpty(nvp))
						{
							Arg = ConstructorSplit[Ptr].Split('=');
							ArgName = Arg[0];
							OptionCnt = cpCore.docProperties.getInteger(ArgName + "CheckBoxCnt");
							if (OptionCnt > 0)
							{
								ArgValueAddonEncoded = "";
								for (OptionPtr = 0; OptionPtr < OptionCnt; OptionPtr++)
								{
									ArgValue = cpCore.docProperties.getText(ArgName + OptionPtr);
									if (!string.IsNullOrEmpty(ArgValue))
									{
										ArgValueAddonEncoded = ArgValueAddonEncoded + "," + genericController.encodeNvaArgument(ArgValue);
									}
								}
								if (!string.IsNullOrEmpty(ArgValueAddonEncoded))
								{
									ArgValueAddonEncoded = ArgValueAddonEncoded.Substring(1);
								}
							}
							else
							{
								ArgValue = cpCore.docProperties.getText(ArgName);
								ArgValueAddonEncoded = genericController.encodeNvaArgument(ArgValue);
							}
							addonOption_String = addonOption_String + "&" + genericController.encodeNvaArgument(ArgName) + "=" + ArgValueAddonEncoded;
						}
					}
					if (!string.IsNullOrEmpty(addonOption_String))
					{
						addonOption_String = addonOption_String.Substring(1);
					}
					cpCore.userProperty.setProperty("Addon [" + AddonName + "] Options", addonOption_String);
					needToClearCache = true;
				}
			}
			else if (string.IsNullOrEmpty(ContentName) || RecordID == 0)
			{
				//
				// ----- Public Site call, must have contentname and recordid
				//
				cpCore.handleException(new Exception("invalid content [" + ContentName + "], RecordID [" + RecordID + "]"));
			}
			else
			{
				//
				// ----- Normal Content Edit - find instance in the content
				//
				CS = cpCore.db.csOpenRecord(ContentName, RecordID);
				if (!cpCore.db.csOk(CS))
				{
					cpCore.handleException(new Exception("No record found with content [" + ContentName + "] and RecordID [" + RecordID + "]"));
				}
				else
				{
					if (!string.IsNullOrEmpty(FieldName))
					{
						//
						// Field is given, find the position
						//
						Copy = cpCore.db.csGet(CS, FieldName);
						PosACInstanceID = genericController.vbInstr(1, Copy, "=\"" + ACInstanceID + "\" ", Microsoft.VisualBasic.Constants.vbTextCompare);
					}
					else
					{
						//
						// Find the field, then find the position
						//
						FieldName = cpCore.db.cs_getFirstFieldName(CS);
						while (!string.IsNullOrEmpty(FieldName))
						{
							fieldType = cpCore.db.cs_getFieldTypeId(CS, FieldName);
							switch (fieldType)
							{
								case FieldTypeIdLongText:
								case FieldTypeIdText:
								case FieldTypeIdFileText:
								case FieldTypeIdFileCSS:
								case FieldTypeIdFileXML:
								case FieldTypeIdFileJavascript:
								case FieldTypeIdHTML:
								case FieldTypeIdFileHTML:
									Copy = cpCore.db.csGet(CS, FieldName);
									PosACInstanceID = genericController.vbInstr(1, Copy, "ACInstanceID=\"" + ACInstanceID + "\"", Microsoft.VisualBasic.Constants.vbTextCompare);
									if (PosACInstanceID != 0)
									{
										//
										// found the instance
										//
										PosACInstanceID = PosACInstanceID + 13;
//INSTANT C# WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
//ORIGINAL LINE: Exit Do
										goto ExitLabel1;
									}
									break;
							}
							FieldName = cpCore.db.cs_getNextFieldName(CS);
						}
						ExitLabel1: ;
					}
					//
					// Parse out the Addon Name
					//
					if (PosACInstanceID == 0)
					{
						cpCore.handleException(new Exception("AC Instance [" + ACInstanceID + "] not found in record with content [" + ContentName + "] and RecordID [" + RecordID + "]"));
					}
					else
					{
						Copy = upgradeActiveContent(Copy);
						ParseOK = false;
						PosStart = Copy.LastIndexOf("<ac ", PosACInstanceID - 1, System.StringComparison.OrdinalIgnoreCase) + 1;
						if (PosStart != 0)
						{
							//
							// main_Get Addon Name to lookup Addon and main_Get most recent Argument List
							//
							PosNameStart = genericController.vbInstr(PosStart, Copy, " name=", Microsoft.VisualBasic.Constants.vbTextCompare);
							if (PosNameStart != 0)
							{
								PosNameStart = PosNameStart + 7;
								PosNameEnd = genericController.vbInstr(PosNameStart, Copy, "\"");
								if (PosNameEnd != 0)
								{
									AddonName = Copy.Substring(PosNameStart - 1, PosNameEnd - PosNameStart);
									//????? test this
									FoundAddon = false;
									addonModel embeddedAddon = cpCore.addonCache.getAddonByName(AddonName);
									if (embeddedAddon != null)
									{
										FoundAddon = true;
										AddonOptionConstructor = genericController.encodeText(embeddedAddon.ArgumentList);
										AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, Environment.NewLine, "\r");
										AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\n", "\r");
										AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\r", Environment.NewLine);
										if (!string.IsNullOrEmpty(AddonOptionConstructor))
										{
											AddonOptionConstructor = AddonOptionConstructor + Environment.NewLine;
										}
										if (genericController.EncodeBoolean(embeddedAddon.IsInline))
										{
											AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Inline;
										}
										else
										{
											AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Block;
										}
									}
									else
									{
										//
										// -- Hardcoded Addons
										switch (genericController.vbLCase(AddonName))
										{
											case "block text":
												FoundAddon = true;
												AddonOptionConstructor = AddonOptionConstructor_ForBlockText;
												break;
											case "":
											break;
										}
									}
									if (FoundAddon)
									{
										ConstructorSplit = Microsoft.VisualBasic.Strings.Split(AddonOptionConstructor, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
										addonOption_String = "";
										//
										// main_Get all responses from current Argument List
										//
										for (Ptr = 0; Ptr <= ConstructorSplit.GetUpperBound(0); Ptr++)
										{
											constructor = ConstructorSplit[Ptr];
											if (!string.IsNullOrEmpty(constructor))
											{
												Arg = constructor.Split('=');
												ArgName = Arg[0];
												OptionCnt = cpCore.docProperties.getInteger(ArgName + "CheckBoxCnt");
												if (OptionCnt > 0)
												{
													ArgValueAddonEncoded = "";
													for (OptionPtr = 0; OptionPtr < OptionCnt; OptionPtr++)
													{
														ArgValue = cpCore.docProperties.getText(ArgName + OptionPtr);
														if (!string.IsNullOrEmpty(ArgValue))
														{
															ArgValueAddonEncoded = ArgValueAddonEncoded + "," + genericController.encodeNvaArgument(ArgValue);
														}
													}
													if (!string.IsNullOrEmpty(ArgValueAddonEncoded))
													{
														ArgValueAddonEncoded = ArgValueAddonEncoded.Substring(1);
													}
												}
												else
												{
													ArgValue = cpCore.docProperties.getText(ArgName);
													ArgValueAddonEncoded = genericController.encodeNvaArgument(ArgValue);
												}

												addonOption_String = addonOption_String + "&" + genericController.encodeNvaArgument(ArgName) + "=" + ArgValueAddonEncoded;
											}
										}
										if (!string.IsNullOrEmpty(addonOption_String))
										{
											addonOption_String = addonOption_String.Substring(1);
										}
									}
								}
							}
							//
							// Replace the new querystring into the AC tag in the content
							//
							PosIDStart = genericController.vbInstr(PosStart, Copy, " querystring=", Microsoft.VisualBasic.Constants.vbTextCompare);
							if (PosIDStart != 0)
							{
								PosIDStart = PosIDStart + 14;
								if (PosIDStart != 0)
								{
									PosIDEnd = genericController.vbInstr(PosIDStart, Copy, "\"");
									if (PosIDEnd != 0)
									{
										ParseOK = true;
										Copy = Copy.Substring(0, PosIDStart - 1) + encodeHTML(addonOption_String) + Copy.Substring(PosIDEnd - 1);
										cpCore.db.csSet(CS, FieldName, Copy);
										needToClearCache = true;
									}
								}
							}
						}
						if (!ParseOK)
						{
							cpCore.handleException(new Exception("There was a problem parsing AC Instance [" + ACInstanceID + "] record with content [" + ContentName + "] and RecordID [" + RecordID + "]"));
						}
					}
				}
				cpCore.db.csClose(CS);
			}
			if (needToClearCache)
			{
				//
				// Clear Caches
				//
				if (!string.IsNullOrEmpty(ContentName))
				{
					cpCore.cache.invalidateAllObjectsInContent(ContentName);
				}
			}
		}
		//
		//========================================================================
		// ----- Process the little edit form in the help bubble
		//========================================================================
		//
		public void processHelpBubbleEditor()
		{
			//
			string SQL = null;
			string MethodName = null;
			string HelpBubbleID = null;
			string[] IDSplit = null;
			int RecordID = 0;
			string HelpCaption = null;
			string HelpMessage = null;
			//
			MethodName = "main_ProcessHelpBubbleEditor()";
			//
			HelpBubbleID = cpCore.docProperties.getText("HelpBubbleID");
			IDSplit = HelpBubbleID.Split('-');
			switch (genericController.vbLCase(IDSplit[0]))
			{
				case "userfield":
					//
					// main_Get the id of the field, and save the input as the caption and help
					//
					if (IDSplit.GetUpperBound(0) > 0)
					{
						RecordID = genericController.EncodeInteger(IDSplit[1]);
						if (RecordID > 0)
						{
							HelpCaption = cpCore.docProperties.getText("helpcaption");
							HelpMessage = cpCore.docProperties.getText("helptext");
							SQL = "update ccfields set caption=" + cpCore.db.encodeSQLText(HelpCaption) + ",HelpMessage=" + cpCore.db.encodeSQLText(HelpMessage) + " where id=" + RecordID;
							cpCore.db.executeQuery(SQL);
							cpCore.cache.invalidateAll();
							cpCore.doc.clearMetaData();
						}
					}
					break;
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// Convert an active content field (html data stored with <ac></ac> html tags) to a wysiwyg editor request (html with edit icon <img> for <ac></ac>)
		/// </summary>
		/// <param name="editorValue"></param>
		/// <returns></returns>
		public string convertActiveContentToHtmlForWysiwygEditor(string editorValue)
		{
			return cpCore.html.convertActiveContent_internal(editorValue, 0, "", 0, 0, false, false, false, true, true, false, "", "", false, 0, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple, false, null, false);
		}
		//
		//====================================================================================================
		//
		public string convertActiveContentToJsonForRemoteMethod(string Source, string ContextContentName, int ContextRecordID, int ContextContactPeopleID, string ProtocolHostString, int DefaultWrapperID, string ignore_TemplateCaseOnly_Content, CPUtilsBaseClass.addonContext addonContext)
		{
			return convertActiveContent_internal(Source, cpCore.doc.authContext.user.id, ContextContentName, ContextRecordID, ContextContactPeopleID, false, false, true, true, false, true, "", ProtocolHostString, false, DefaultWrapperID, ignore_TemplateCaseOnly_Content, addonContext, cpCore.doc.authContext.isAuthenticated, null, cpCore.doc.authContext.isEditingAnything());
			//False, False, True, True, False, True, ""
		}
		//
		//====================================================================================================
		//
		public string convertActiveContentToHtmlForWebRender(string Source, string ContextContentName, int ContextRecordID, int ContextContactPeopleID, string ProtocolHostString, int DefaultWrapperID, CPUtilsBaseClass.addonContext addonContext)
		{
			return convertActiveContent_internal(Source, cpCore.doc.authContext.user.id, ContextContentName, ContextRecordID, ContextContactPeopleID, false, false, true, true, false, true, "", ProtocolHostString, false, DefaultWrapperID, "", addonContext, cpCore.doc.authContext.isAuthenticated, null, cpCore.doc.authContext.isEditingAnything());
			//False, False, True, True, False, True, ""
		}
		//
		//====================================================================================================
		//
		public string convertActiveContentToHtmlForEmailSend(string Source, int personalizationPeopleID, string queryStringForLinkAppend)
		{
			return convertActiveContent_internal(Source, personalizationPeopleID, "", 0, 0, false, true, true, true, false, true, queryStringForLinkAppend, "", true, 0, "", CPUtilsBaseClass.addonContext.ContextEmail, true, null, false);
			//False, False, True, True, False, True, ""
		}

		//
		//========================================================================
		// Print the Member Edit form
		//
		//   For instance, list out a checklist of all public groups, with the ones checked that this member belongs to
		//       PrimaryContentName = "People"
		//       PrimaryRecordID = MemberID
		//       SecondaryContentName = "Groups"
		//       SecondaryContentSelectCriteria = "ccGroups.PublicJoin<>0"
		//       RulesContentName = "Member Rules"
		//       RulesPrimaryFieldName = "MemberID"
		//       RulesSecondaryFieldName = "GroupID"
		//========================================================================
		//
		public string getCheckList2(string TagName, string PrimaryContentName, int PrimaryRecordID, string SecondaryContentName, string RulesContentName, string RulesPrimaryFieldname, string RulesSecondaryFieldName, string SecondaryContentSelectCriteria = "", string CaptionFieldName = "", bool readOnlyfield = false)
		{
			return getCheckList(TagName, PrimaryContentName, PrimaryRecordID, SecondaryContentName, RulesContentName, RulesPrimaryFieldname, RulesSecondaryFieldName, SecondaryContentSelectCriteria, genericController.encodeText(CaptionFieldName), readOnlyfield, false, "");
		}
		//
		//========================================================================
		//   main_Get a list of checkbox options based on a standard set of rules
		//
		//   IncludeContentFolderDivs
		//       When true, the list of options (checkboxes) are grouped by ContentFolder and wrapped in a Div with ID="ContentFolder99"
		//
		//   For instance, list out a options of all public groups, with the ones checked that this member belongs to
		//       PrimaryContentName = "People"
		//       PrimaryRecordID = MemberID
		//       SecondaryContentName = "Groups"
		//       SecondaryContentSelectCriteria = "ccGroups.PublicJoin<>0"
		//       RulesContentName = "Member Rules"
		//       RulesPrimaryFieldName = "MemberID"
		//       RulesSecondaryFieldName = "GroupID"
		//========================================================================
		//
		public string getCheckList(string TagName, string PrimaryContentName, int PrimaryRecordID, string SecondaryContentName, string RulesContentName, string RulesPrimaryFieldname, string RulesSecondaryFieldName, string SecondaryContentSelectCriteria = "", string CaptionFieldName = "", bool readOnlyfield = false, bool IncludeContentFolderDivs = false, string DefaultSecondaryIDList = "")
		{
			string returnHtml = "";
			try
			{
				string[] main_MemberShipText = null;
				int Ptr = 0;
				int main_MemberShipID = 0;
				string javaScriptRequired = "";
				string DivName = null;
				int DivCnt = 0;
				string OldFolderVar = null;
				string EndDiv = null;
				int OpenFolderID = 0;
				string RuleCopyCaption = null;
				string RuleCopy = null;
				string SQL = null;
				int CS = 0;
				int main_MemberShipCount = 0;
				int main_MemberShipSize = 0;
				int main_MemberShipPointer = 0;
				string SectionName = null;
				int CheckBoxCnt = 0;
				int DivCheckBoxCnt = 0;
				int[] main_MemberShip = {};
				string[] main_MemberShipRuleCopy = {};
				int PrimaryContentID = 0;
				string SecondaryTablename = null;
				int SecondaryContentID = 0;
				string rulesTablename = null;
				string OptionName = null;
				string OptionCaption = null;
				string optionCaptionHtmlEncoded = null;
				bool CanSeeHiddenFields = false;
				Models.Complex.cdefModel SecondaryCDef = null;
				List<int> ContentIDList = new List<int>();
				bool Found = false;
				int RecordID = 0;
				string SingularPrefixHtmlEncoded = null;
				bool IsRuleCopySupported = false;
				bool AllowRuleCopy = false;
				//
				// IsRuleCopySupported - if true, the rule records include an allow button, and copy
				//   This is for a checkbox like [ ] Other [enter other copy here]
				//
				IsRuleCopySupported = Models.Complex.cdefModel.isContentFieldSupported(cpCore, RulesContentName, "RuleCopy");
				if (IsRuleCopySupported)
				{
					IsRuleCopySupported = IsRuleCopySupported && Models.Complex.cdefModel.isContentFieldSupported(cpCore, SecondaryContentName, "AllowRuleCopy");
					if (IsRuleCopySupported)
					{
						IsRuleCopySupported = IsRuleCopySupported && Models.Complex.cdefModel.isContentFieldSupported(cpCore, SecondaryContentName, "RuleCopyCaption");
					}
				}
				if (string.IsNullOrEmpty(CaptionFieldName))
				{
					CaptionFieldName = "name";
				}
				CaptionFieldName = genericController.encodeEmptyText(CaptionFieldName, "name");
				if (string.IsNullOrEmpty(PrimaryContentName) || string.IsNullOrEmpty(SecondaryContentName) || string.IsNullOrEmpty(RulesContentName) || string.IsNullOrEmpty(RulesPrimaryFieldname) || string.IsNullOrEmpty(RulesSecondaryFieldName))
				{
					returnHtml = "[Checklist not configured]";
					cpCore.handleException(new Exception("Creating checklist, all required fields were not supplied, Caption=[" + CaptionFieldName + "], PrimaryContentName=[" + PrimaryContentName + "], SecondaryContentName=[" + SecondaryContentName + "], RulesContentName=[" + RulesContentName + "], RulesPrimaryFieldName=[" + RulesPrimaryFieldname + "], RulesSecondaryFieldName=[" + RulesSecondaryFieldName + "]"));
				}
				else
				{
					//
					// ----- Gather all the SecondaryContent that associates to the PrimaryContent
					//
					PrimaryContentID = Models.Complex.cdefModel.getContentId(cpCore, PrimaryContentName);
					SecondaryCDef = Models.Complex.cdefModel.getCdef(cpCore, SecondaryContentName);
					SecondaryTablename = SecondaryCDef.ContentTableName;
					SecondaryContentID = SecondaryCDef.Id;
					ContentIDList.Add(SecondaryContentID);
					ContentIDList.AddRange(SecondaryCDef.childIdList(cpCore));
					//
					//
					//
					rulesTablename = Models.Complex.cdefModel.getContentTablename(cpCore, RulesContentName);
					SingularPrefixHtmlEncoded = encodeHTML(genericController.GetSingular(SecondaryContentName)) + "&nbsp;";
					//
					main_MemberShipCount = 0;
					main_MemberShipSize = 0;
					returnHtml = "";
					if ((!string.IsNullOrEmpty(SecondaryTablename)) & (!string.IsNullOrEmpty(rulesTablename)))
					{
						OldFolderVar = "OldFolder" + cpCore.doc.checkListCnt;
						javaScriptRequired += "var " + OldFolderVar + ";";
						if (PrimaryRecordID == 0)
						{
							//
							// New record, use the DefaultSecondaryIDList
							//
							if (!string.IsNullOrEmpty(DefaultSecondaryIDList))
							{

								main_MemberShipText = DefaultSecondaryIDList.Split(',');
								for (Ptr = 0; Ptr <= main_MemberShipText.GetUpperBound(0); Ptr++)
								{
									main_MemberShipID = genericController.EncodeInteger(main_MemberShipText[Ptr]);
									if (main_MemberShipID != 0)
									{
										Array.Resize(ref main_MemberShip, Ptr + 1);
										main_MemberShip[Ptr] = main_MemberShipID;
										main_MemberShipCount = Ptr + 1;
									}
								}
								if (main_MemberShipCount > 0)
								{
									main_MemberShipRuleCopy = new string[main_MemberShipCount];
								}
								//main_MemberShipCount = UBound(main_MemberShip) + 1
								main_MemberShipSize = main_MemberShipCount;
							}
						}
						else
						{
							//
							// ----- Determine main_MemberShip (which secondary records are associated by a rule)
							// ----- (exclude new record issue ID=0)
							//
							if (IsRuleCopySupported)
							{
								SQL = "SELECT " + SecondaryTablename + ".ID AS ID," + rulesTablename + ".RuleCopy";
							}
							else
							{
								SQL = "SELECT " + SecondaryTablename + ".ID AS ID,'' as RuleCopy";
							}
							SQL += ""
							+ " FROM " + SecondaryTablename + " LEFT JOIN"
							+ " " + rulesTablename + " ON " + SecondaryTablename + ".ID = " + rulesTablename + "." + RulesSecondaryFieldName + " WHERE "
							+ " (" + rulesTablename + "." + RulesPrimaryFieldname + "=" + PrimaryRecordID + ")"
							+ " AND (" + rulesTablename + ".Active<>0)"
							+ " AND (" + SecondaryTablename + ".Active<>0)"
							+ " And (" + SecondaryTablename + ".ContentControlID IN (" + string.Join(",", ContentIDList) + "))";
							if (!string.IsNullOrEmpty(SecondaryContentSelectCriteria))
							{
								SQL += "AND(" + SecondaryContentSelectCriteria + ")";
							}
							CS = cpCore.db.csOpenSql(SQL);
							if (cpCore.db.csOk(CS))
							{
								if (true)
								{
									main_MemberShipSize = 10;
									main_MemberShip = new int[main_MemberShipSize + 1];
									main_MemberShipRuleCopy = new string[main_MemberShipSize + 1];
									while (cpCore.db.csOk(CS))
									{
										if (main_MemberShipCount >= main_MemberShipSize)
										{
											main_MemberShipSize = main_MemberShipSize + 10;
											Array.Resize(ref main_MemberShip, main_MemberShipSize + 1);
											Array.Resize(ref main_MemberShipRuleCopy, main_MemberShipSize + 1);
										}
										main_MemberShip[main_MemberShipCount] = cpCore.db.csGetInteger(CS, "ID");
										main_MemberShipRuleCopy[main_MemberShipCount] = cpCore.db.csGetText(CS, "RuleCopy");
										main_MemberShipCount = main_MemberShipCount + 1;
										cpCore.db.csGoNext(CS);
									}
								}
							}
							cpCore.db.csClose(CS);
						}
						//
						// ----- Gather all the Secondary Records, sorted by ContentName
						//
						SQL = "SELECT " + SecondaryTablename + ".ID AS ID, " + SecondaryTablename + "." + CaptionFieldName + " AS OptionCaption, " + SecondaryTablename + ".name AS OptionName, " + SecondaryTablename + ".SortOrder";
						if (IsRuleCopySupported)
						{
							SQL += "," + SecondaryTablename + ".AllowRuleCopy," + SecondaryTablename + ".RuleCopyCaption";
						}
						else
						{
							SQL += ",0 as AllowRuleCopy,'' as RuleCopyCaption";
						}
						SQL += " from " + SecondaryTablename + " where (1=1)";
						if (!string.IsNullOrEmpty(SecondaryContentSelectCriteria))
						{
							SQL += "AND(" + SecondaryContentSelectCriteria + ")";
						}
						SQL += " GROUP BY " + SecondaryTablename + ".ID, " + SecondaryTablename + "." + CaptionFieldName + ", " + SecondaryTablename + ".name, " + SecondaryTablename + ".SortOrder";
						if (IsRuleCopySupported)
						{
							SQL += ", " + SecondaryTablename + ".AllowRuleCopy," + SecondaryTablename + ".RuleCopyCaption";
						}
						SQL += " ORDER BY ";
						SQL += SecondaryTablename + "." + CaptionFieldName;
						CS = cpCore.db.csOpenSql(SQL);
						if (!cpCore.db.csOk(CS))
						{
							returnHtml = "(No choices are available.)";
						}
						else
						{
							if (true)
							{
								OpenFolderID = -1;
								EndDiv = "";
								SectionName = "";
								CheckBoxCnt = 0;
								DivCheckBoxCnt = 0;
								DivCnt = 0;
								CanSeeHiddenFields = cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore);
								DivName = TagName + ".All";
								while (cpCore.db.csOk(CS))
								{
									OptionName = cpCore.db.csGetText(CS, "OptionName");
									if ((OptionName.Substring(0, 1) != "_") || CanSeeHiddenFields)
									{
										//
										// Current checkbox is visible
										//
										RecordID = cpCore.db.csGetInteger(CS, "ID");
										AllowRuleCopy = cpCore.db.csGetBoolean(CS, "AllowRuleCopy");
										RuleCopyCaption = cpCore.db.csGetText(CS, "RuleCopyCaption");
										OptionCaption = cpCore.db.csGetText(CS, "OptionCaption");
										if (string.IsNullOrEmpty(OptionCaption))
										{
											OptionCaption = OptionName;
										}
										if (string.IsNullOrEmpty(OptionCaption))
										{
											optionCaptionHtmlEncoded = SingularPrefixHtmlEncoded + RecordID;
										}
										else
										{
											optionCaptionHtmlEncoded = encodeHTML(OptionCaption);
										}
										if (DivCheckBoxCnt != 0)
										{
											// leave this between checkboxes - it is searched in the admin page
											returnHtml += "<br >" + Environment.NewLine;
										}
										RuleCopy = "";
										if (false)
										{
											Found = false;
											//s = s & "<input type=""checkbox"" name=""" & TagName & "." & CheckBoxCnt & """ "
											if (main_MemberShipCount != 0)
											{
												for (main_MemberShipPointer = 0; main_MemberShipPointer < main_MemberShipCount; main_MemberShipPointer++)
												{
													if (main_MemberShip[main_MemberShipPointer] == (RecordID))
													{
														RuleCopy = main_MemberShipRuleCopy[main_MemberShipPointer];
														returnHtml += html_GetFormInputHidden(TagName + "." + CheckBoxCnt, true);
														Found = true;
														break;
													}
												}
											}
											returnHtml += genericController.main_GetYesNo(Found) + "&nbsp;-&nbsp;";
										}
										else
										{
											Found = false;
											if (main_MemberShipCount != 0)
											{
												for (main_MemberShipPointer = 0; main_MemberShipPointer < main_MemberShipCount; main_MemberShipPointer++)
												{
													if (main_MemberShip[main_MemberShipPointer] == (RecordID))
													{
														//s = s & main_GetFormInputHidden(TagName & "." & CheckBoxCnt, True)
														RuleCopy = main_MemberShipRuleCopy[main_MemberShipPointer];
														Found = true;
														break;
													}
												}
											}
											// must leave the first hidden with the value in this form - it is searched in the admin pge
											returnHtml += Environment.NewLine;
											returnHtml += "<table><tr><td style=\"vertical-align:top;margin-top:0;width:20px;\">";
											returnHtml += "<input type=hidden name=\"" + TagName + "." + CheckBoxCnt + ".ID\" value=" + RecordID + ">";
											if (readOnlyfield && !Found)
											{
												returnHtml += "<input type=checkbox disabled>";
											}
											else if (readOnlyfield)
											{
												returnHtml += "<input type=checkbox disabled checked>";
												returnHtml += "<input type=\"hidden\" name=\"" + TagName + "." + CheckBoxCnt + ".ID\" value=" + RecordID + ">";
											}
											else if (Found)
											{
												returnHtml += "<input type=checkbox name=\"" + TagName + "." + CheckBoxCnt + "\" checked>";
											}
											else
											{
												returnHtml += "<input type=checkbox name=\"" + TagName + "." + CheckBoxCnt + "\">";
											}
											returnHtml += "</td><td style=\"vertical-align:top;padding-top:4px;\">";
											returnHtml += SpanClassAdminNormal + optionCaptionHtmlEncoded;
											if (AllowRuleCopy)
											{
												returnHtml += ", " + RuleCopyCaption + "&nbsp;" + html_GetFormInputText2(TagName + "." + CheckBoxCnt + ".RuleCopy", RuleCopy, 1, 20);
											}
											returnHtml += "</td></tr></table>";
										}
										CheckBoxCnt = CheckBoxCnt + 1;
										DivCheckBoxCnt = DivCheckBoxCnt + 1;
									}
									cpCore.db.csGoNext(CS);
								}
								returnHtml += EndDiv;
								returnHtml += "<input type=\"hidden\" name=\"" + TagName + ".RowCount\" value=\"" + CheckBoxCnt + "\">" + Environment.NewLine;
							}
						}
						cpCore.db.csClose(CS);
						addScriptCode_head(javaScriptRequired, "CheckList Categories");
					}
					//End If
					cpCore.doc.checkListCnt = cpCore.doc.checkListCnt + 1;
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return returnHtml;
		}

	}
}
