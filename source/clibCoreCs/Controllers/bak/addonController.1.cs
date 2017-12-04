using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using System.Reflection;
using Contensive.BaseClasses;
using Contensive.Core.Controllers.genericController;

using System.Xml;
using Contensive.Core.Models.Entity;

namespace Contensive.Core.Controllers
{
	//
	//====================================================================================================
	/// <summary>
	/// classSummary
	/// - first routine should be constructor
	/// - disposable region at end
	/// - if disposable is not needed add: not IDisposable - not contained classes that need to be disposed
	/// </summary>
	public partial class addonController : IDisposable
	{
				//
		//===============================================================================================================================================
		//   cpcore.main_Get inner HTML viewer Bubble
		//===============================================================================================================================================
		//
		public string getHTMLViewerBubble(int addonId, string HTMLSourceID, ref string return_DialogList)
		{
				string tempgetHTMLViewerBubble = null;
			try
			{
				//
				string DefaultStylesheet = null;
				string StyleSheet = null;
				string OptionDefault = null;
				string OptionSuffix = null;
				int OptionCnt = 0;
				string OptionValue_AddonEncoded = null;
				string OptionValue = null;
				string OptionCaption = null;
				string LCaseOptionDefault = null;
				string[] OptionValues = null;
				string FormInput = null;
				int OptionPtr = 0;
				string QueryString = null;
				string LocalCode = string.Empty;
				string CopyHeader = string.Empty;
				string CopyContent = null;
				string BubbleJS = null;
				string[] OptionSplit = null;
				string OptionName = null;
				string OptionSelector = null;
				int Ptr = 0;
				int Pos = 0;
				int CS = 0;
				string AddonName = string.Empty;
				int StyleSN = 0;
				string HTMLViewerBubbleID = null;
				//
				if (cpCore.doc.authContext.isAuthenticated())
				{
					if (cpCore.doc.authContext.isEditingAnything())
					{
						StyleSN = genericController.EncodeInteger(cpCore.siteProperties.getText("StylesheetSerialNumber", "0"));
						HTMLViewerBubbleID = "HelpBubble" + cpCore.doc.helpCodeCount;
						//
						CopyHeader = CopyHeader + "<div class=\"ccHeaderCon\">"
							+ "<table border=0 cellpadding=0 cellspacing=0 width=\"100%\">"
							+ "<tr>"
							+ "<td align=left class=\"bbLeft\">HTML viewer</td>"
							+ "<td align=right class=\"bbRight\"><a href=\"#\" onClick=\"HelpBubbleOff('" + HTMLViewerBubbleID + "');return false;\"><img alt=\"close\" src=\"/ccLib/images/ClosexRev1313.gif\" width=13 height=13 border=0></A></td>"
							+ "</tr>"
							+ "</table>"
							+ "</div>";
						CopyContent = ""
							+ "<table border=0 cellpadding=5 cellspacing=0 width=\"100%\">"
							+ "<tr><td style=\"width:400px;background-color:transparent;\" class=\"ccAdminSmall\">This is the HTML produced by this add-on. Carrage returns and tabs have been added or modified to enhance readability.</td></tr>"
							+ "<tr><td style=\"width:400px;background-color:transparent;\" class=\"ccAdminSmall\">" + cpCore.html.html_GetFormInputTextExpandable2("DefaultStyles", "", 10, "400px", HTMLViewerBubbleID + "_dst",, false) + "</td></tr>"
							+ "</tr>"
							+ "</table>"
							+ "";
						//
						QueryString = cpCore.doc.refreshQueryString;
						QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, "", false);
						//QueryString = genericController.ModifyQueryString(QueryString, RequestNameInterceptpage, "", False)
						return_DialogList = return_DialogList + "<div class=\"ccCon helpDialogCon\">"
							+ "<table border=0 cellpadding=0 cellspacing=0 class=\"ccBubbleCon\" id=\"" + HTMLViewerBubbleID + "\" style=\"display:none;visibility:hidden;\">"
							+ "<tr><td class=\"ccHeaderCon\">" + CopyHeader + "</td></tr>"
							+ "<tr><td class=\"ccContentCon\">" + CopyContent + "</td></tr>"
							+ "</table>"
							+ "</div>";
						BubbleJS = " onClick=\"var d=document.getElementById('" + HTMLViewerBubbleID + "_dst');if(d){var s=document.getElementById('" + HTMLSourceID + "');if(s){d.value=s.innerHTML;HelpBubbleOn( '" + HTMLViewerBubbleID + "',this)}};return false;\" ";
						if (cpCore.doc.helpCodeCount >= cpCore.doc.helpCodeSize)
						{
							cpCore.doc.helpCodeSize = cpCore.doc.helpCodeSize + 10;
//INSTANT C# TODO TASK: The following 'ReDim' could not be resolved. A possible reason may be that the object of the ReDim was not declared as an array:
							ReDim Preserve cpCore.doc.helpCodes(cpCore.doc.helpCodeSize);
//INSTANT C# TODO TASK: The following 'ReDim' could not be resolved. A possible reason may be that the object of the ReDim was not declared as an array:
							ReDim Preserve cpCore.doc.helpCaptions(cpCore.doc.helpCodeSize);
						}
						cpCore.doc.helpCodes(cpCore.doc.helpCodeCount) = LocalCode;
						cpCore.doc.helpCaptions(cpCore.doc.helpCodeCount) = AddonName;
						cpCore.doc.helpCodeCount = cpCore.doc.helpCodeCount + 1;
						//SiteStylesBubbleCache = "x"
						//
						if (cpCore.doc.helpDialogCnt == 0)
						{
							cpCore.html.addScriptCode_onLoad("jQuery(function(){jQuery('.helpDialogCon').draggable()})", "draggable dialogs");
						}
						cpCore.doc.helpDialogCnt = cpCore.doc.helpDialogCnt + 1;
						tempgetHTMLViewerBubble = ""
							+ "&nbsp;<a href=\"#\" tabindex=-1 target=\"_blank\"" + BubbleJS + " >"
							+ GetIconSprite("", 0, "/ccLib/images/toolhtml.png", 22, 22, "View the source HTML produced by this Add-on", "View the source HTML produced by this Add-on", "", true, "") + "</A>";
					}
				}
				//
				return tempgetHTMLViewerBubble;
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("addon_execute_GetHTMLViewerBubble")
			return tempgetHTMLViewerBubble;
		}
		//
		//
		//
		private string getFormContent(string FormXML, ref bool return_ExitRequest)
		{
			string tempgetFormContent = null;
			string result = "";
			try
			{
				int FieldCount = 0;
				int RowMax = 0;
				int ColumnMax = 0;
				int SQLPageSize = 0;
				int ErrorNumber = 0;
				string ErrorDescription = null;
				string[,] dataArray = null;
				int RecordID = 0;
				string fieldfilename = null;
				string FieldDataSource = null;
				string FieldSQL = null;
				stringBuilderLegacyController Content = new stringBuilderLegacyController();
				string Copy = null;
				string Button = null;
				adminUIController Adminui = new adminUIController(cpCore);
				string ButtonList = string.Empty;
				string Filename = null;
				string NonEncodedLink = null;
				string EncodedLink = null;
				string VirtualFilePath = null;
				string TabName = null;
				string TabDescription = null;
				string TabHeading = null;
				int TabCnt = 0;
				stringBuilderLegacyController TabCell = null;
				string FieldValue = string.Empty;
				string FieldDescription = null;
				string FieldDefaultValue = null;
				bool IsFound = false;
				string Name = string.Empty;
				string Description = string.Empty;
				XmlDocument Doc = new XmlDocument();
//INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
//				XmlNode TabNode = null;
//INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
//				XmlNode SettingNode = null;
				int CS = 0;
				string FieldName = null;
				string FieldCaption = null;
				string FieldAddon = null;
				bool FieldReadOnly = false;
				bool FieldHTML = false;
				string fieldType = null;
				string FieldSelector = null;
				string DefaultFilename = null;
				//
				Button = cpCore.docProperties.getText(RequestNameButton);
				if (Button == ButtonCancel)
				{
					//
					// Cancel just exits with no content
					//
					return_ExitRequest = true;
					return string.Empty;
				}
				else if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore))
				{
					//
					// Not Admin Error
					//
					ButtonList = ButtonCancel;
					Content.Add(Adminui.GetFormBodyAdminOnly());
				}
				else
				{
					if (true)
					{
						bool loadOK = true;
						try
						{

							Doc.LoadXml(FormXML);
						}
						catch (Exception ex)
						{
							// error
							//
							ButtonList = ButtonCancel;
							Content.Add("<div class=\"ccError\" style=\"margin:10px;padding:10px;background-color:white;\">There was a problem with the Setting Page you requested.</div>");
							loadOK = false;
						}
						//        CS = cpcore.main_OpenCSContentRecord("Setting Pages", SettingPageID)
						//        If Not app.csv_IsCSOK(CS) Then
						//            '
						//            ' Setting Page was not found
						//            '
						//            ButtonList = ButtonCancel
						//            Content.Add( "<div class=""ccError"" style=""margin:10px;padding:10px;background-color:white;"">The Setting Page you requested could not be found.</div>"
						//        Else
						//            XMLFile = app.cs_get(CS, "xmlfile")
						//            Doc = New XmlDocument
						//Doc.loadXML (XMLFile)
						if (loadOK)
						{
						}
						else
						{
							//
							// data is OK
							//
							if (genericController.vbLCase(Doc.DocumentElement.Name) != "form")
							{
								//
								// error - Need a way to reach the user that submitted the file
								//
								ButtonList = ButtonCancel;
								Content.Add("<div class=\"ccError\" style=\"margin:10px;padding:10px;background-color:white;\">There was a problem with the Setting Page you requested.</div>");
							}
							else
							{
								//
								// ----- Process Requests
								//
								if ((Button == ButtonSave) || (Button == ButtonOK))
								{
									foreach (XmlNode SettingNode in Doc.DocumentElement.ChildNodes)
									{
										switch (genericController.vbLCase(SettingNode.Name))
										{
											case "tab":
												foreach (XmlNode TabNode in SettingNode.ChildNodes)
												{
													switch (genericController.vbLCase(TabNode.Name))
													{
														case "siteproperty":
															//
															FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
															FieldValue = cpCore.docProperties.getText(FieldName);
															fieldType = xml_GetAttribute(IsFound, TabNode, "type", "");
															switch (genericController.vbLCase(fieldType))
															{
																case "integer":
																	//
																	if (!string.IsNullOrEmpty(FieldValue))
																	{
																		FieldValue = genericController.EncodeInteger(FieldValue).ToString();
																	}
																	cpCore.siteProperties.setProperty(FieldName, FieldValue);
																	break;
																case "boolean":
																	//
																	if (!string.IsNullOrEmpty(FieldValue))
																	{
																		FieldValue = genericController.EncodeBoolean(FieldValue).ToString();
																	}
																	cpCore.siteProperties.setProperty(FieldName, FieldValue);
																	break;
																case "float":
																	//
																	if (!string.IsNullOrEmpty(FieldValue))
																	{
																		FieldValue = EncodeNumber(FieldValue).ToString();
																	}
																	cpCore.siteProperties.setProperty(FieldName, FieldValue);
																	break;
																case "date":
																	//
																	if (!string.IsNullOrEmpty(FieldValue))
																	{
																		FieldValue = genericController.EncodeDate(FieldValue).ToString();
																	}
																	cpCore.siteProperties.setProperty(FieldName, FieldValue);
																	break;
																case "file":
																case "imagefile":
																	//
																	if (cpCore.docProperties.getBoolean(FieldName + ".DeleteFlag"))
																	{
																		cpCore.siteProperties.setProperty(FieldName, "");
																	}
																	if (!string.IsNullOrEmpty(FieldValue))
																	{
																		Filename = FieldValue;
																		VirtualFilePath = "Settings/" + FieldName + "/";
																		cpCore.cdnFiles.upload(FieldName, VirtualFilePath, Filename);
																		cpCore.siteProperties.setProperty(FieldName, VirtualFilePath + "/" + Filename);
																	}
																	break;
																case "textfile":
																	//
																	DefaultFilename = "Settings/" + FieldName + ".txt";
																	Filename = cpCore.siteProperties.getText(FieldName, DefaultFilename);
																	if (string.IsNullOrEmpty(Filename))
																	{
																		Filename = DefaultFilename;
																		cpCore.siteProperties.setProperty(FieldName, DefaultFilename);
																	}
																	cpCore.appRootFiles.saveFile(Filename, FieldValue);
																	break;
																case "cssfile":
																	//
																	DefaultFilename = "Settings/" + FieldName + ".css";
																	Filename = cpCore.siteProperties.getText(FieldName, DefaultFilename);
																	if (string.IsNullOrEmpty(Filename))
																	{
																		Filename = DefaultFilename;
																		cpCore.siteProperties.setProperty(FieldName, DefaultFilename);
																	}
																	cpCore.appRootFiles.saveFile(Filename, FieldValue);
																	break;
																case "xmlfile":
																	//
																	DefaultFilename = "Settings/" + FieldName + ".xml";
																	Filename = cpCore.siteProperties.getText(FieldName, DefaultFilename);
																	if (string.IsNullOrEmpty(Filename))
																	{
																		Filename = DefaultFilename;
																		cpCore.siteProperties.setProperty(FieldName, DefaultFilename);
																	}
																	cpCore.appRootFiles.saveFile(Filename, FieldValue);
																	break;
																case "currency":
																	//
																	if (!string.IsNullOrEmpty(FieldValue))
																	{
																		FieldValue = EncodeNumber(FieldValue).ToString();
																		FieldValue = Microsoft.VisualBasic.Strings.FormatCurrency(FieldValue, -1, Microsoft.VisualBasic.TriState.UseDefault, Microsoft.VisualBasic.TriState.UseDefault, Microsoft.VisualBasic.TriState.UseDefault);
																	}
																	cpCore.siteProperties.setProperty(FieldName, FieldValue);
																	break;
																case "link":
																	cpCore.siteProperties.setProperty(FieldName, FieldValue);
																	break;
																default:
																	cpCore.siteProperties.setProperty(FieldName, FieldValue);
																	break;
															}
															break;
														case "copycontent":
															//
															// A Copy Content block
															//
															FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
															if (!FieldReadOnly)
															{
																FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
																FieldHTML = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "html", "false"));
																if (FieldHTML)
																{
																	//
																	// treat html as active content for now.
																	//
																	FieldValue = cpCore.docProperties.getRenderedActiveContent(FieldName);
																}
																else
																{
																	FieldValue = cpCore.docProperties.getText(FieldName);
																}

																CS = cpCore.db.csOpen("Copy Content", "name=" + cpCore.db.encodeSQLText(FieldName), "ID");
																if (!cpCore.db.csOk(CS))
																{
																	cpCore.db.csClose(CS);
																	CS = cpCore.db.csInsertRecord("Copy Content");
																}
																if (cpCore.db.csOk(CS))
																{
																	cpCore.db.csSet(CS, "name", FieldName);
																	//
																	// Set copy
																	//
																	cpCore.db.csSet(CS, "copy", FieldValue);
																	//
																	// delete duplicates
																	//
																	cpCore.db.csGoNext(CS);
																	while (cpCore.db.csOk(CS))
																	{
																		cpCore.db.csDeleteRecord(CS);
																		cpCore.db.csGoNext(CS);
																	}
																}
																cpCore.db.csClose(CS);
															}

															break;
														case "filecontent":
															//
															// A File Content block
															//
															FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
															if (!FieldReadOnly)
															{
																FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
																fieldfilename = xml_GetAttribute(IsFound, TabNode, "filename", "");
																FieldValue = cpCore.docProperties.getText(FieldName);
																cpCore.appRootFiles.saveFile(fieldfilename, FieldValue);
															}
															break;
														case "dbquery":
															//
															// dbquery has no results to process
															//
														break;
													}
												}
												break;
											default:
											break;
										}
									}
								}
								if (Button == ButtonOK)
								{
									//
									// Exit on OK or cancel
									//
									return_ExitRequest = true;
									return string.Empty;
								}
								//
								// ----- Display Form
								//
								Content.Add(Adminui.EditTableOpen);
								Name = xml_GetAttribute(IsFound, Doc.DocumentElement, "name", "");
								foreach (XmlNode SettingNode in Doc.DocumentElement.ChildNodes)
								{
									switch (genericController.vbLCase(SettingNode.Name))
									{
										case "description":
											Description = SettingNode.InnerText;
											break;
										case "tab":
											TabCnt = TabCnt + 1;
											TabName = xml_GetAttribute(IsFound, SettingNode, "name", "");
											TabDescription = xml_GetAttribute(IsFound, SettingNode, "description", "");
											TabHeading = xml_GetAttribute(IsFound, SettingNode, "heading", "");
											TabCell = new stringBuilderLegacyController();
											foreach (XmlNode TabNode in SettingNode.ChildNodes)
											{
												switch (genericController.vbLCase(TabNode.Name))
												{
													case "heading":
														//
														// Heading
														//
														FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "");
														TabCell.Add(Adminui.GetEditSubheadRow(FieldCaption));
														break;
													case "siteproperty":
														//
														// Site property
														//
														FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
														if (!string.IsNullOrEmpty(FieldName))
														{
															FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "");
															if (string.IsNullOrEmpty(FieldCaption))
															{
																FieldCaption = FieldName;
															}
															FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
															FieldHTML = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "html", ""));
															fieldType = xml_GetAttribute(IsFound, TabNode, "type", "");
															FieldSelector = xml_GetAttribute(IsFound, TabNode, "selector", "");
															FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "");
															FieldAddon = xml_GetAttribute(IsFound, TabNode, "EditorAddon", "");
															FieldDefaultValue = TabNode.InnerText;
															FieldValue = cpCore.siteProperties.getText(FieldName, FieldDefaultValue);
															//                                                    If FieldReadOnly Then
															//                                                        '
															//                                                        ' Read only = no editor
															//                                                        '
															//                                                        Copy = FieldValue & cpcore.main_GetFormInputHidden( FieldName, FieldValue)
															//
															//                                                    ElseIf FieldAddon <> "" Then
															if (!string.IsNullOrEmpty(FieldAddon))
															{
																//
																// Use Editor Addon
																//
																Dictionary<string, string> arguments = new Dictionary<string, string>();
																arguments.Add("FieldName", FieldName);
																arguments.Add("FieldValue", cpCore.siteProperties.getText(FieldName, FieldDefaultValue));
																CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {addonType = CPUtilsBaseClass.addonContext.ContextAdmin};
																addonModel addon = addonModel.createByName(cpCore, FieldAddon);
																Copy = cpCore.addon.execute(addon, executeContext);
																//Option_String = "FieldName=" & FieldName & "&FieldValue=" & encodeNvaArgument(cpCore.siteProperties.getText(FieldName, FieldDefaultValue))
																//Copy = execute_legacy5(0, FieldAddon, Option_String, CPUtilsBaseClass.addonContext.ContextAdmin, "", 0, "", 0)
															}
															else if (!string.IsNullOrEmpty(FieldSelector))
															{
																//
																// Use Selector
																//
																Copy = getFormContent_decodeSelector(FieldName, FieldValue, FieldSelector);
															}
															else
															{
																//
																// Use default editor for each field type
																//
																switch (genericController.vbLCase(fieldType))
																{
																	case "integer":
																		//
																		if (FieldReadOnly)
																		{
																			Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
																		}
																		else
																		{
																			Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue);
																		}
																		break;
																	case "boolean":
																		if (FieldReadOnly)
																		{
																			Copy = cpCore.html.html_GetFormInputCheckBox2(FieldName, genericController.EncodeBoolean(FieldValue));
																			Copy = genericController.vbReplace(Copy, ">", " disabled>");
																			Copy = Copy + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
																		}
																		else
																		{
																			Copy = cpCore.html.html_GetFormInputCheckBox2(FieldName, genericController.EncodeBoolean(FieldValue));
																		}
																		break;
																	case "float":
																		if (FieldReadOnly)
																		{
																			Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
																		}
																		else
																		{
																			Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue);
																		}
																		break;
																	case "date":
																		if (FieldReadOnly)
																		{
																			Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
																		}
																		else
																		{
																			Copy = cpCore.html.html_GetFormInputDate(FieldName, FieldValue);
																		}
																		break;
																	case "file":
																	case "imagefile":
																		//
																		if (FieldReadOnly)
																		{
																			Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
																		}
																		else
																		{
																			if (string.IsNullOrEmpty(FieldValue))
																			{
																				Copy = cpCore.html.html_GetFormInputFile(FieldName);
																			}
																			else
																			{
																				NonEncodedLink = cpCore.webServer.requestDomain + genericController.getCdnFileLink(cpCore, FieldValue);
																				EncodedLink = EncodeURL(NonEncodedLink);
																				string FieldValuefilename = "";
																				string FieldValuePath = "";
																				cpCore.privateFiles.splitPathFilename(FieldValue, FieldValuePath, FieldValuefilename);
																				Copy = ""
																				+ "<a href=\"http://" + EncodedLink + "\" target=\"_blank\">[" + FieldValuefilename + "]</A>"
																				+ "&nbsp;&nbsp;&nbsp;Delete:&nbsp;" + cpCore.html.html_GetFormInputCheckBox2(FieldName + ".DeleteFlag", false) + "&nbsp;&nbsp;&nbsp;Change:&nbsp;" + cpCore.html.html_GetFormInputFile(FieldName);
																			}
																		}
																	//Call s.Add("&nbsp;</span></nobr></td>")
																		break;
																	case "currency":
																		//
																		if (FieldReadOnly)
																		{
																			Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
																		}
																		else
																		{
																			if (!string.IsNullOrEmpty(FieldValue))
																			{
																				FieldValue = Microsoft.VisualBasic.Strings.FormatCurrency(FieldValue, -1, Microsoft.VisualBasic.TriState.UseDefault, Microsoft.VisualBasic.TriState.UseDefault, Microsoft.VisualBasic.TriState.UseDefault);
																			}
																			Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue);
																		}
																		break;
																	case "textfile":
																		//
																		if (FieldReadOnly)
																		{
																			Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
																		}
																		else
																		{
																			FieldValue = cpCore.cdnFiles.readFile(FieldValue);
																			if (FieldHTML)
																			{
																				Copy = cpCore.html.getFormInputHTML(FieldName, FieldValue);
																			}
																			else
																			{
																				Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, FieldValue, 5);
																			}
																		}
																		break;
																	case "cssfile":
																		//
																		if (FieldReadOnly)
																		{
																			Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
																		}
																		else
																		{
																			Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, FieldValue, 5);
																		}
																		break;
																	case "xmlfile":
																		//
																		if (FieldReadOnly)
																		{
																			Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
																		}
																		else
																		{
																			Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, FieldValue, 5);
																		}
																		break;
																	case "link":
																		//
																		if (FieldReadOnly)
																		{
																			Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
																		}
																		else
																		{
																			Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue);
																		}
																		break;
																	default:
																		//
																		// text
																		//
																		if (FieldReadOnly)
																		{
																			Copy = FieldValue + cpCore.html.html_GetFormInputHidden(FieldName, FieldValue);
																		}
																		else
																		{
																			if (FieldHTML)
																			{
																				Copy = cpCore.html.getFormInputHTML(FieldName, FieldValue);
																			}
																			else
																			{
																				Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue);
																			}
																		}
																		break;
																}
															}
															TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, false, false, ""));
														}
														break;
													case "copycontent":
														//
														// Content Copy field
														//
														FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
														if (!string.IsNullOrEmpty(FieldName))
														{
															FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "");
															if (string.IsNullOrEmpty(FieldCaption))
															{
																FieldCaption = FieldName;
															}
															FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
															FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "");
															FieldHTML = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "html", ""));
															//
															CS = cpCore.db.csOpen("Copy Content", "Name=" + cpCore.db.encodeSQLText(FieldName), "ID",,,,, "Copy");
															if (!cpCore.db.csOk(CS))
															{
																cpCore.db.csClose(CS);
																CS = cpCore.db.csInsertRecord("Copy Content");
																if (cpCore.db.csOk(CS))
																{
																	RecordID = cpCore.db.csGetInteger(CS, "ID");
																	cpCore.db.csSet(CS, "name", FieldName);
																	cpCore.db.csSet(CS, "copy", genericController.encodeText(TabNode.InnerText));
																	cpCore.db.csSave2(CS);
																	//   Call cpCore.workflow.publishEdit("Copy Content", RecordID)
																}
															}
															if (cpCore.db.csOk(CS))
															{
																FieldValue = cpCore.db.csGetText(CS, "copy");
															}
															if (FieldReadOnly)
															{
																//
																// Read only
																//
																Copy = FieldValue;
															}
															else if (FieldHTML)
															{
																//
																// HTML
																//
																Copy = cpCore.html.getFormInputHTML(FieldName, FieldValue);
																//Copy = cpcore.main_GetFormInputActiveContent( FieldName, FieldValue)
															}
															else
															{
																//
																// Text edit
																//
																Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, FieldValue);
															}
															TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, false, false, ""));
														}
														break;
													case "filecontent":
														//
														// Content from a flat file
														//
														FieldName = xml_GetAttribute(IsFound, TabNode, "name", "");
														FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "");
														fieldfilename = xml_GetAttribute(IsFound, TabNode, "filename", "");
														FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""));
														FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "");
														FieldDefaultValue = TabNode.InnerText;
														Copy = "";
														if (!string.IsNullOrEmpty(fieldfilename))
														{
															if (cpCore.appRootFiles.fileExists(fieldfilename))
															{
																Copy = FieldDefaultValue;
															}
															else
															{
																Copy = cpCore.cdnFiles.readFile(fieldfilename);
															}
															if (!FieldReadOnly)
															{
																Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, Copy, 10);
															}
														}
														TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, false, false, ""));
														break;
													case "dbquery":
													case "querydb":
													case "query":
													case "db":
														//
														// Display the output of a query
														//
														Copy = "";
														FieldDataSource = xml_GetAttribute(IsFound, TabNode, "DataSourceName", "");
														FieldSQL = TabNode.InnerText;
														FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "");
														FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "");
														SQLPageSize = genericController.EncodeInteger(xml_GetAttribute(IsFound, TabNode, "rowmax", ""));
														if (SQLPageSize == 0)
														{
															SQLPageSize = 100;
														}
														//
														// Run the SQL
														//
														DataTable dt = null;

														if (!string.IsNullOrEmpty(FieldSQL))
														{
															try
															{
																dt = cpCore.db.executeQuery(FieldSQL, FieldDataSource,, SQLPageSize);
															}
															catch (Exception ex)
															{
																ErrorDescription = ex.ToString();
																loadOK = false;
															}
														}
														if (dt != null)
														{
															if (string.IsNullOrEmpty(FieldSQL))
															{
																//
																// ----- Error
																//
																Copy = "No Result";
															}
															else if (ErrorNumber != 0)
															{
																//
																// ----- Error
																//
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
																Copy = "Error: " + Microsoft.VisualBasic.Information.Err().Description;
															}
															else if (dt.Rows.Count <= 0)
															{
																//
																// ----- no result
																//
																Copy = "No Results";
															}
															else
															{
																//
																// ----- print results
																//
																//PageSize = RS.PageSize
																//
																// --- Create the Fields for the new table
																//
																//
																//Dim dtOk As Boolean = True
																dataArray = cpCore.db.convertDataTabletoArray(dt);
																//
																RowMax = dataArray.GetUpperBound(1);
																ColumnMax = dataArray.GetUpperBound(0);
																if (RowMax == 0 && ColumnMax == 0)
																{
																	//
																	// Single result, display with no table
																	//
																	Copy = cpCore.html.html_GetFormInputText2("result", genericController.encodeText(dataArray[0, 0]),,,,, true);
																}
																else
																{
																	//
																	// Build headers
																	//
																	FieldCount = dt.Columns.Count;
																	Copy = Copy + ("\r" + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-bottom:1px solid #444;border-right:1px solid #444;background-color:white;color:#444;\">");
																	Copy = Copy + (cr2 + "<tr>");
																	foreach (DataColumn dc in dt.Columns)
																	{
																		Copy = Copy + (cr2 + "\t" + "<td class=\"ccadminsmall\" style=\"border-top:1px solid #444;border-left:1px solid #444;color:black;padding:2px;padding-top:4px;padding-bottom:4px;\">" + dc.ColumnName + "</td>");
																	}
																	Copy = Copy + (cr2 + "</tr>");
																	//
																	// Build output table
																	//
																	string RowStart = null;
																	string RowEnd = null;
																	string ColumnStart = null;
																	string ColumnEnd = null;
																	RowStart = cr2 + "<tr>";
																	RowEnd = cr2 + "</tr>";
																	ColumnStart = cr2 + "\t" + "<td class=\"ccadminnormal\" style=\"border-top:1px solid #444;border-left:1px solid #444;background-color:white;color:#444;padding:2px\">";
																	ColumnEnd = "</td>";
																	int RowPointer = 0;
																	for (RowPointer = 0; RowPointer <= RowMax; RowPointer++)
																	{
																		Copy = Copy + (RowStart);
																		int ColumnPointer = 0;
																		for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++)
																		{
																			object CellData = dataArray[ColumnPointer, RowPointer];
																			if (IsNull(CellData))
																			{
																				Copy = Copy + (ColumnStart + "[null]" + ColumnEnd);
																			}
																			else if ((CellData == null))
																			{
																				Copy = Copy + (ColumnStart + "[empty]" + ColumnEnd);
																			}
																			else if (Microsoft.VisualBasic.Information.IsArray(CellData))
																			{
																				Copy = Copy + ColumnStart + "[array]";
																			}
																			else if (genericController.encodeText(CellData) == "")
																			{
																				Copy = Copy + (ColumnStart + "[empty]" + ColumnEnd);
																			}
																			else
																			{
																				Copy = Copy + (ColumnStart + genericController.encodeHTML(genericController.encodeText(CellData)) + ColumnEnd);
																			}
																		}
																		Copy = Copy + (RowEnd);
																	}
																	Copy = Copy + ("\r" + "</table>");
																}
															}
														}
														TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, false, false, ""));
														break;
												}
											}
											Copy = Adminui.GetEditPanel(true, TabHeading, TabDescription, Adminui.EditTableOpen + TabCell.Text + Adminui.EditTableClose);
											if (!string.IsNullOrEmpty(Copy))
											{
												cpCore.html.main_AddLiveTabEntry(TabName.Replace(" ", "&nbsp;"), Copy, "ccAdminTab");
											}
											//Content.Add( cpcore.main_GetForm_Edit_AddTab(TabName, Copy, True))
											TabCell = null;
											break;
										default:
										break;
									}
								}
								//
								// Buttons
								//
								ButtonList = ButtonCancel + "," + ButtonSave + "," + ButtonOK;
								//
								// Close Tables
								//
								if (TabCnt > 0)
								{
									Content.Add(cpCore.html.main_GetLiveTabs());
								}
							}
						}
					}
				}
				//
				tempgetFormContent = Adminui.GetBody(Name, ButtonList, "", true, true, Description, "", 0, Content.Text);
				Content = null;
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//
		//========================================================================
		//   Display field in the admin/edit
		//========================================================================
		//
		private string getFormContent_decodeSelector(string SitePropertyName, string SitePropertyValue, string selector)
		{
				string tempgetFormContent_decodeSelector = null;
			try
			{
				//
				string ExpandedSelector = string.Empty;
				Dictionary<string, string> addonInstanceProperties = new Dictionary<string, string>();
				string OptionCaption = null;
				string OptionValue = null;
				string OptionValue_AddonEncoded = null;
				int OptionPtr = 0;
				int OptionCnt = 0;
				string[] OptionValues = null;
				string OptionSuffix = string.Empty;
				string LCaseOptionDefault = null;
				int Pos = 0;
				bool Checked = false;
				int ParentID = 0;
				int ParentCID = 0;
				string Criteria = null;
				int RootCID = 0;
				string SQL = null;
				int TableID = 0;
				int TableName = 0;
				int ChildCID = 0;
				string CIDList = null;
				string TableName2 = null;
				string RecordContentName = null;
				bool HasParentID = false;
				int CS = 0;
				// converted array to dictionary - Dim FieldPointer As Integer
				int CSPointer = 0;
				int RecordID = 0;
				stringBuilderLegacyController FastString = null;
				int FieldValueInteger = 0;
				bool FieldRequired = false;
				string FieldHelp = null;
				string AuthoringStatusMessage = null;
				string Delimiter = null;
				string Copy = string.Empty;
				adminUIController Adminui = new adminUIController(cpCore);
				//
				string FieldName = null;
				//
				FastString = new stringBuilderLegacyController();
				//
				Dictionary<string, string> instanceOptions = new Dictionary<string, string>();
				instanceOptions.Add(SitePropertyName, SitePropertyValue);
				buildAddonOptionLists(ref addonInstanceProperties, ref ExpandedSelector, SitePropertyName + "=" + selector, instanceOptions, "0", true);
				Pos = genericController.vbInstr(1, ExpandedSelector, "[");
				if (Pos != 0)
				{
					//
					// List of Options, might be select, radio or checkbox
					//
					LCaseOptionDefault = genericController.vbLCase(ExpandedSelector.Substring(0, Pos - 1));
					int PosEqual = genericController.vbInstr(1, LCaseOptionDefault, "=");

					if (PosEqual > 0)
					{
						LCaseOptionDefault = LCaseOptionDefault.Substring(PosEqual);
					}

					LCaseOptionDefault = genericController.decodeNvaArgument(LCaseOptionDefault);
					ExpandedSelector = ExpandedSelector.Substring(Pos);
					Pos = genericController.vbInstr(1, ExpandedSelector, "]");
					if (Pos > 0)
					{
						if (Pos < ExpandedSelector.Length)
						{
							OptionSuffix = genericController.vbLCase((ExpandedSelector.Substring(Pos)).Trim(' '));
						}
						ExpandedSelector = ExpandedSelector.Substring(0, Pos - 1);
					}
					OptionValues = ExpandedSelector.Split('|');
					tempgetFormContent_decodeSelector = "";
					OptionCnt = OptionValues.GetUpperBound(0) + 1;
					for (OptionPtr = 0; OptionPtr < OptionCnt; OptionPtr++)
					{
						OptionValue_AddonEncoded = OptionValues[OptionPtr].Trim(' ');
						if (!string.IsNullOrEmpty(OptionValue_AddonEncoded))
						{
							Pos = genericController.vbInstr(1, OptionValue_AddonEncoded, ":");
							if (Pos == 0)
							{
								OptionValue = genericController.decodeNvaArgument(OptionValue_AddonEncoded);
								OptionCaption = OptionValue;
							}
							else
							{
								OptionCaption = genericController.decodeNvaArgument(OptionValue_AddonEncoded.Substring(0, Pos - 1));
								OptionValue = genericController.decodeNvaArgument(OptionValue_AddonEncoded.Substring(Pos));
							}
							switch (OptionSuffix)
							{
								case "checkbox":
									//
									// Create checkbox
									//
									if (genericController.vbInstr(1, "," + LCaseOptionDefault + ",", "," + genericController.vbLCase(OptionValue) + ",") != 0)
									{
										tempgetFormContent_decodeSelector = tempgetFormContent_decodeSelector + "<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" + SitePropertyName + OptionPtr + "\" value=\"" + OptionValue + "\" checked=\"checked\">" + OptionCaption + "</div>";
									}
									else
									{
										tempgetFormContent_decodeSelector = tempgetFormContent_decodeSelector + "<div style=\"white-space:nowrap\"><input type=\"checkbox\" name=\"" + SitePropertyName + OptionPtr + "\" value=\"" + OptionValue + "\" >" + OptionCaption + "</div>";
									}
									break;
								case "radio":
									//
									// Create Radio
									//
									if (genericController.vbLCase(OptionValue) == LCaseOptionDefault)
									{
										tempgetFormContent_decodeSelector = tempgetFormContent_decodeSelector + "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + SitePropertyName + "\" value=\"" + OptionValue + "\" checked=\"checked\" >" + OptionCaption + "</div>";
									}
									else
									{
										tempgetFormContent_decodeSelector = tempgetFormContent_decodeSelector + "<div style=\"white-space:nowrap\"><input type=\"radio\" name=\"" + SitePropertyName + "\" value=\"" + OptionValue + "\" >" + OptionCaption + "</div>";
									}
									break;
								default:
									//
									// Create select 
									//
									if (genericController.vbLCase(OptionValue) == LCaseOptionDefault)
									{
										tempgetFormContent_decodeSelector = tempgetFormContent_decodeSelector + "<option value=\"" + OptionValue + "\" selected>" + OptionCaption + "</option>";
									}
									else
									{
										tempgetFormContent_decodeSelector = tempgetFormContent_decodeSelector + "<option value=\"" + OptionValue + "\">" + OptionCaption + "</option>";
									}
									break;
							}
						}
					}
					switch (OptionSuffix)
					{
						case "checkbox":
							//
							//
							Copy = Copy + "<input type=\"hidden\" name=\"" + SitePropertyName + "CheckBoxCnt\" value=\"" + OptionCnt + "\" >";
							break;
						case "radio":
							//
							// Create Radio 
							//
							//cpCore.htmldoc.main_Addon_execute_GetFormContent_decodeSelector = "<div>" & genericController.vbReplace(cpCore.htmldoc.main_Addon_execute_GetFormContent_decodeSelector, "><", "></div><div><") & "</div>"
						break;
						default:
							//
							// Create select 
							//
							tempgetFormContent_decodeSelector = "<select name=\"" + SitePropertyName + "\">" + tempgetFormContent_decodeSelector + "</select>";
							break;
					}
				}
				else
				{
					//
					// Create Text addon_execute_GetFormContent_decodeSelector
					//

					selector = genericController.decodeNvaArgument(selector);
					tempgetFormContent_decodeSelector = cpCore.html.html_GetFormInputText2(SitePropertyName, selector, 1, 20);
				}

				FastString = null;
				return tempgetFormContent_decodeSelector;
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			FastString = null;
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("addon_execute_GetFormContent_decodeSelector")
			return tempgetFormContent_decodeSelector;
		}
		//
		//===================================================================================================
		//   Build AddonOptionLists
		//
		//   On entry:
		//       AddonOptionConstructor = the addon-encoded version of the list that comes from the Addon Record
		//           It is crlf delimited and all escape characters converted
		//       AddonOptionString = addonencoded version of the list that comes from the HTML AC tag
		//           that means & delimited
		//
		//   On Exit:
		//       OptionString_ForObjectCall
		//               pass this string to the addon when it is run, crlf delimited name=value pair.
		//               This should include just the name=values pairs, with no selectors
		//               it should include names from both Addon and Instance
		//               If the Instance has a value, include it. Otherwise include Addon value
		//       AddonOptionExpandedConstructor = pass this to the bubble editor to create the the selectr
		//===================================================================================================
		//
		public void buildAddonOptionLists2(ref Dictionary<string, string> addonInstanceProperties, ref string addonArgumentListPassToBubbleEditor, string addonArgumentListFromRecord, Dictionary<string, string> instanceOptions, string InstanceID, bool IncludeSettingsBubbleOptions)
		{
			try
			{
				//
				int SavePtr = 0;
				string[] ConstructorTypes = null;
				string ConstructorType = null;
				string ConstructorValue = null;
				string ConstructorSelector = null;
				string ConstructorName = null;
				int ConstructorPtr = 0;
				int Pos = 0;
				int InstanceCnt = 0;
				string InstanceName = null;
				string InstanceValue = null;
				//
				string[] ConstructorNameValues = {};
				string[] ConstructorNames = {};
				string[] ConstructorSelectors = {};
				string[] ConstructorValues = {};
				//
				int ConstructorCnt = 0;


				if (!string.IsNullOrEmpty(addonArgumentListFromRecord))
				{
					//
					// Initially Build Constructor from AddonOptions
					//
					ConstructorNameValues = Microsoft.VisualBasic.Strings.Split(addonArgumentListFromRecord, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
					ConstructorCnt = ConstructorNameValues.GetUpperBound(0) + 1;
					ConstructorNames = new string[ConstructorCnt + 1];
					ConstructorSelectors = new string[ConstructorCnt + 1];
					ConstructorValues = new string[ConstructorCnt + 1];
					ConstructorTypes = new string[ConstructorCnt + 1];
					SavePtr = 0;
					for (ConstructorPtr = 0; ConstructorPtr < ConstructorCnt; ConstructorPtr++)
					{
						ConstructorName = ConstructorNameValues[ConstructorPtr];
						ConstructorSelector = "";
						ConstructorValue = "";
						ConstructorType = "text";
						Pos = genericController.vbInstr(1, ConstructorName, "=");
						if (Pos > 1)
						{
							ConstructorValue = ConstructorName.Substring(Pos);
							ConstructorName = (ConstructorName.Substring(0, Pos - 1)).Trim(' ');
							Pos = genericController.vbInstr(1, ConstructorValue, "[");
							if (Pos > 0)
							{
								ConstructorSelector = ConstructorValue.Substring(Pos - 1);
								ConstructorValue = ConstructorValue.Substring(0, Pos - 1);
							}
						}
						if (!string.IsNullOrEmpty(ConstructorName))
						{
							//Pos = genericController.vbInstr(1, ConstructorName, ",")
							//If Pos > 1 Then
							//    ConstructorType = Mid(ConstructorName, Pos + 1)
							//    ConstructorName = Left(ConstructorName, Pos - 1)
							//End If

							ConstructorNames[SavePtr] = ConstructorName;
							ConstructorValues[SavePtr] = ConstructorValue;
							ConstructorSelectors[SavePtr] = ConstructorSelector;
							//ConstructorTypes(ConstructorPtr) = ConstructorType
							SavePtr = SavePtr + 1;
						}
					}
					ConstructorCnt = SavePtr;
				}
				InstanceCnt = 0;
				//
				// Now update the values with Instance - if a name is not found, add it
				//
				foreach (var kvp in instanceOptions)
				{
					InstanceName = kvp.Key;
					InstanceValue = kvp.Value;
					if (!string.IsNullOrEmpty(InstanceName))
					{
						//
						// if the name is not in the Constructor, add it
						if (ConstructorCnt > 0)
						{
							for (ConstructorPtr = 0; ConstructorPtr < ConstructorCnt; ConstructorPtr++)
							{
								if (genericController.vbLCase(InstanceName) == genericController.vbLCase(ConstructorNames[ConstructorPtr]))
								{
									break;
								}
							}
						}
						if (ConstructorPtr >= ConstructorCnt)
						{
							//
							// not found, add this instance name and value to the Constructor values
							//
							Array.Resize(ref ConstructorNames, ConstructorCnt + 1);
							Array.Resize(ref ConstructorValues, ConstructorCnt + 1);
							Array.Resize(ref ConstructorSelectors, ConstructorCnt + 1);
							ConstructorNames[ConstructorCnt] = InstanceName;
							ConstructorValues[ConstructorCnt] = InstanceValue;
							ConstructorCnt = ConstructorCnt + 1;
						}
						else
						{
							//
							// found, set the ConstructorValue to the instance value
							//
							ConstructorValues[ConstructorPtr] = InstanceValue;
						}
						SavePtr = SavePtr + 1;
					}
				}
				addonArgumentListPassToBubbleEditor = "";
				//
				// Build output strings from name and value found
				//
				for (ConstructorPtr = 0; ConstructorPtr < ConstructorCnt; ConstructorPtr++)
				{
					ConstructorName = ConstructorNames[ConstructorPtr];
					ConstructorValue = ConstructorValues[ConstructorPtr];
					ConstructorSelector = ConstructorSelectors[ConstructorPtr];
					// here goes nothing!!
					addonInstanceProperties.Add(ConstructorName, ConstructorValue);
					//OptionString_ForObjectCall = OptionString_ForObjectCall & csv_DecodeAddonOptionArgument(ConstructorName) & "=" & csv_DecodeAddonOptionArgument(ConstructorValue) & vbCrLf
					if (IncludeSettingsBubbleOptions)
					{
						addonArgumentListPassToBubbleEditor = addonArgumentListPassToBubbleEditor + Environment.NewLine + cpCore.html.getAddonSelector(ConstructorName, ConstructorValue, ConstructorSelector);
					}
				}
				addonInstanceProperties.Add("InstanceID", InstanceID);
				//If OptionString_ForObjectCall <> "" Then
				//    OptionString_ForObjectCall = Mid(OptionString_ForObjectCall, 1, Len(OptionString_ForObjectCall) - 1)
				//    'OptionString_ForObjectCall = Mid(OptionString_ForObjectCall, 1, Len(OptionString_ForObjectCall) - 2)
				//End If
				if (!string.IsNullOrEmpty(addonArgumentListPassToBubbleEditor))
				{
					addonArgumentListPassToBubbleEditor = addonArgumentListPassToBubbleEditor.Substring(2);
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
		}
		//
		//===================================================================================================
		//   Build AddonOptionLists
		//
		//   On entry:
		//       AddonOptionConstructor = the addon-encoded Version of the list that comes from the Addon Record
		//           It is line-delimited with &, and all escape characters converted
		//       InstanceOptionList = addonencoded Version of the list that comes from the HTML AC tag
		//           that means crlf line-delimited
		//
		//   On Exit:
		//       AddonOptionNameValueList
		//               pass this string to the addon when it is run, crlf delimited name=value pair.
		//               This should include just the name=values pairs, with no selectors
		//               it should include names from both Addon and Instance
		//               If the Instance has a value, include it. Otherwise include Addon value
		//       AddonOptionExpandedConstructor = pass this to the bubble editor to create the the selectr
		//===================================================================================================
		//
		public void buildAddonOptionLists(ref Dictionary<string, string> addonInstanceProperties, ref string addonArgumentListPassToBubbleEditor, string addonArgumentListFromRecord, Dictionary<string, string> InstanceOptionList, string InstanceID, bool IncludeEditWrapper)
		{
			buildAddonOptionLists2(ref addonInstanceProperties, ref addonArgumentListPassToBubbleEditor, addonArgumentListFromRecord, InstanceOptionList, InstanceID, IncludeEditWrapper);
		}
		//
		//
		//
		public string getPrivateFilesAddonPath()
		{
			return "addons\\";
		}
		//
		//========================================================================
		//   Apply a wrapper to content
		//========================================================================
		//
		private string addWrapperToResult(string Content, int WrapperID, string WrapperSourceForComment = "")
		{
			try
			{
				//
				int Pos = 0;
				int CS = 0;
				string JSFilename = null;
				string Copy = null;
				string s = null;
				string SelectFieldList = null;
				string Wrapper = null;
				string wrapperName = null;
				string SourceComment = null;
				string TargetString = null;
				//
				s = Content;
				SelectFieldList = "name,copytext,javascriptonload,javascriptbodyend,stylesfilename,otherheadtags,JSFilename,targetString";
				CS = cpCore.db.csOpenRecord("Wrappers", WrapperID,,, SelectFieldList);
				if (cpCore.db.csOk(CS))
				{
					Wrapper = cpCore.db.csGetText(CS, "copytext");
					wrapperName = cpCore.db.csGetText(CS, "name");
					TargetString = cpCore.db.csGetText(CS, "targetString");
					//
					SourceComment = "wrapper " + wrapperName;
					if (!string.IsNullOrEmpty(WrapperSourceForComment))
					{
						SourceComment = SourceComment + " for " + WrapperSourceForComment;
					}
					cpCore.html.addScriptCode_onLoad(cpCore.db.csGetText(CS, "javascriptonload"), SourceComment);
					cpCore.html.addScriptCode_body(cpCore.db.csGetText(CS, "javascriptbodyend"), SourceComment);
					cpCore.html.addHeadTag(cpCore.db.csGetText(CS, "OtherHeadTags"), SourceComment);
					//
					JSFilename = cpCore.db.csGetText(CS, "jsfilename");
					if (!string.IsNullOrEmpty(JSFilename))
					{
						JSFilename = cpCore.webServer.requestProtocol + cpCore.webServer.requestDomain + genericController.getCdnFileLink(cpCore, JSFilename);
						cpCore.html.addScriptLink_Head(JSFilename, SourceComment);
					}
					Copy = cpCore.db.csGetText(CS, "stylesfilename");
					if (!string.IsNullOrEmpty(Copy))
					{
						if (genericController.vbInstr(1, Copy, "://") != 0)
						{
						}
						else if (Copy.Substring(0, 1) == "/")
						{
						}
						else
						{
							Copy = cpCore.webServer.requestProtocol + cpCore.webServer.requestDomain + genericController.getCdnFileLink(cpCore, Copy);
						}
						cpCore.html.addStyleLink(Copy, SourceComment);
					}
					//
					if (!string.IsNullOrEmpty(Wrapper))
					{
						Pos = genericController.vbInstr(1, Wrapper, TargetString, Microsoft.VisualBasic.Constants.vbTextCompare);
						if (Pos != 0)
						{
							s = genericController.vbReplace(Wrapper, TargetString, s, 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
						}
						else
						{
							s = ""
								+ "<!-- the selected wrapper does not include the Target String marker to locate the position of the content. -->"
								+ Wrapper + s;
						}
					}
				}
				cpCore.db.csClose(CS);
				//
				return s;
				//
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("WrapContent")
		}
		//
		//========================================================================
		// ----- main_Get an XML nodes attribute based on its name
		//========================================================================
		//
		public string xml_GetAttribute(bool Found, XmlNode Node, string Name, string DefaultIfNotFound)
		{
			string result = string.Empty;
			try
			{
				//
//INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
//				XmlAttribute NodeAttribute = null;
				XmlNode ResultNode = null;
				string UcaseName = null;
				//
				Found = false;
				ResultNode = Node.Attributes.GetNamedItem(Name);
				if (ResultNode == null)
				{
					UcaseName = genericController.vbUCase(Name);
					foreach (XmlAttribute NodeAttribute in Node.Attributes)
					{
						if (genericController.vbUCase(NodeAttribute.Name) == UcaseName)
						{
							result = NodeAttribute.Value;
							Found = true;
							break;
						}
					}
				}
				else
				{
					result = ResultNode.Value;
					Found = true;
				}
				if (!Found)
				{
					result = DefaultIfNotFound;
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
		public static string main_GetDefaultAddonOption_String(coreClass cpCore, string ArgumentList, string AddonGuid, bool IsInline)
		{
			string result = "";
			//
			string NameValuePair = null;
			int Pos = 0;
			string OptionName = null;
			string OptionValue = null;
			string OptionSelector = null;
			string[] QuerySplit = null;
			string NameValue = null;
			int Ptr = 0;
			//
			ArgumentList = genericController.vbReplace(ArgumentList, Environment.NewLine, "\r");
			ArgumentList = genericController.vbReplace(ArgumentList, "\n", "\r");
			ArgumentList = genericController.vbReplace(ArgumentList, "\r", Environment.NewLine);
			if (ArgumentList.IndexOf("wrapper", System.StringComparison.OrdinalIgnoreCase) + 1 == 0)
			{
				//
				// Add in default constructors, like wrapper
				//
				if (!string.IsNullOrEmpty(ArgumentList))
				{
					ArgumentList = ArgumentList + Environment.NewLine;
				}
				if (genericController.vbLCase(AddonGuid) == genericController.vbLCase(addonGuidContentBox))
				{
					ArgumentList = ArgumentList + AddonOptionConstructor_BlockNoAjax;
				}
				else if (IsInline)
				{
					ArgumentList = ArgumentList + AddonOptionConstructor_Inline;
				}
				else
				{
					ArgumentList = ArgumentList + AddonOptionConstructor_Block;
				}
			}
			if (!string.IsNullOrEmpty(ArgumentList))
			{
				//
				// Argument list is present, translate from AddonConstructor to AddonOption format (see main_executeAddon for details)
				//
				QuerySplit = genericController.SplitCRLF(ArgumentList);
				result = "";
				for (Ptr = 0; Ptr <= QuerySplit.GetUpperBound(0); Ptr++)
				{
					NameValue = QuerySplit[Ptr];
					if (!string.IsNullOrEmpty(NameValue))
					{
						//
						// Execute list functions
						//
						OptionName = "";
						OptionValue = "";
						OptionSelector = "";
						//
						// split on equal
						//
						NameValue = genericController.vbReplace(NameValue, "\\=", Environment.NewLine);
						Pos = genericController.vbInstr(1, NameValue, "=");
						if (Pos == 0)
						{
							OptionName = NameValue;
						}
						else
						{
							OptionName = NameValue.Substring(0, Pos - 1);
							OptionValue = NameValue.Substring(Pos);
						}
						OptionName = genericController.vbReplace(OptionName, Environment.NewLine, "\\=");
						OptionValue = genericController.vbReplace(OptionValue, Environment.NewLine, "\\=");
						//
						// split optionvalue on [
						//
						OptionValue = genericController.vbReplace(OptionValue, "\\[", Environment.NewLine);
						Pos = genericController.vbInstr(1, OptionValue, "[");
						if (Pos != 0)
						{
							OptionSelector = OptionValue.Substring(Pos - 1);
							OptionValue = OptionValue.Substring(0, Pos - 1);
						}
						OptionValue = genericController.vbReplace(OptionValue, Environment.NewLine, "\\[");
						OptionSelector = genericController.vbReplace(OptionSelector, Environment.NewLine, "\\[");
						//
						// Decode AddonConstructor format
						//
						OptionName = genericController.DecodeAddonConstructorArgument(OptionName);
						OptionValue = genericController.DecodeAddonConstructorArgument(OptionValue);
						//
						// Encode AddonOption format
						//
						//main_GetAddonSelector expects value to be encoded, but not name
						//OptionName = encodeNvaArgument(OptionName)
						OptionValue = genericController.encodeNvaArgument(OptionValue);
						//
						// rejoin
						//
						NameValuePair = cpCore.html.getAddonSelector(OptionName, OptionValue, OptionSelector);
						NameValuePair = genericController.EncodeJavascript(NameValuePair);
						result = result + "&" + NameValuePair;
						if (genericController.vbInstr(1, NameValuePair, "=") == 0)
						{
							result = result + "=";
						}
					}
				}
				if (!string.IsNullOrEmpty(result))
				{
					// remove leading "&"
					result = result.Substring(1);
				}
			}
			return result;
		}
		//
		//=================================================================================================================
		//   csv_GetAddonOption
		//
		//   returns the value matching a given name in an AddonOptionConstructor
		//
		//   AddonOptionConstructor is a crlf delimited name=value[selector]descriptor list
		//
		//   See cpCoreClass.ExecuteAddon for a full description of:
		//       AddonOptionString
		//       AddonOptionConstructor
		//       AddonOptionNameValueList
		//       AddonOptionExpandedConstructor
		//=================================================================================================================
		//
		public static string getAddonOption(string OptionName, string OptionString)
		{
			string result = "";
			string WorkingString = null;
			string[] Options = null;
			int Ptr = 0;
			int Pos = 0;
			string TestName = null;
			string TargetName = null;
			//
			WorkingString = OptionString;
			result = "";
			if (!string.IsNullOrEmpty(WorkingString))
			{
				TargetName = genericController.vbLCase(OptionName);
				Options = OptionString.Split('&');
				for (Ptr = 0; Ptr <= Options.GetUpperBound(0); Ptr++)
				{
					Pos = genericController.vbInstr(1, Options[Ptr], "=");
					if (Pos > 0)
					{
						TestName = genericController.vbLCase((Options[Ptr].Substring(0, Pos - 1)).Trim(' '));
						while ((!string.IsNullOrEmpty(TestName)) && (TestName.Substring(0, 1) == "\t"))
						{
							TestName = TestName.Substring(1).Trim(' ');
						}
						while ((!string.IsNullOrEmpty(TestName)) && (TestName.Substring(TestName.Length - 1) == "\t"))
						{
							TestName = (TestName.Substring(0, TestName.Length - 1)).Trim(' ');
						}
						if (TestName == TargetName)
						{
							result = genericController.decodeNvaArgument((Options[Ptr].Substring(Pos)).Trim(' '));
							break;
						}
					}
				}
			}
			return result;
		}
		//
		private string getAddonDescription(coreClass cpcore, Models.Entity.addonModel addon)
		{
			string addonDescription = "[invalid addon]";
			if (addon != null)
			{
				string collectionName = "invalid collection or collection not set";
				Models.Entity.AddonCollectionModel collection = Models.Entity.AddonCollectionModel.create(cpcore, addon.CollectionID);
				if (collection != null)
				{
					collectionName = collection.name;
				}
				addonDescription = "[#" + addon.id.ToString() + ", " + addon.name + "], collection [" + collectionName + "]";
			}
			return addonDescription;
		}
		//
		//====================================================================================================
		/// <summary>
		/// Special case addon as it is a required core service. This method attempts the addon call and it if fails, calls the safe-mode version, tested for this build
		/// </summary>
		/// <returns></returns>
		public static string GetAddonManager(coreClass cpCore)
		{
			string addonManager = "";
			try
			{
				bool AddonStatusOK = true;
				try
				{
					addonModel addon = addonModel.create(cpCore, addonGuidAddonManager);
					addonManager = cpCore.addon.execute(addon, new CPUtilsBaseClass.addonExecuteContext() {addonType = CPUtilsBaseClass.addonContext.ContextAdmin});
					//addonManager = cpCore.addon.execute_legacy2(0, addonGuidAddonManager, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, "", 0, "", "0", False, -1, "", AddonStatusOK, Nothing)
				}
				catch (Exception ex)
				{
					cpCore.handleException(new Exception("Error calling ExecuteAddon with AddonManagerGuid, will attempt Safe Mode Addon Manager. Exception=[" + ex.ToString() + "]"));
					AddonStatusOK = false;
				}
				if (string.IsNullOrEmpty(addonManager))
				{
					cpCore.handleException(new Exception("AddonManager returned blank, calling Safe Mode Addon Manager."));
					AddonStatusOK = false;
				}
				if (!AddonStatusOK)
				{
					addon_AddonMngrSafeClass AddonMan = new addon_AddonMngrSafeClass(cpCore);
					addonManager = AddonMan.GetForm_SafeModeAddonManager();
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return addonManager;
		}
		//
		//====================================================================================================
		/// <summary>
		/// If an addon assembly references a system assembly that is not in the gac (system.io.compression.filesystem), it does not look in the folder I did the loadfrom.
		/// Problem is knowing where to look. No argument to pass a path...
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static Assembly myAssemblyResolve(object sender, ResolveEventArgs args)
		{
			string sample_folderPath = IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

			string assemblyPath = IO.Path.Combine(sample_folderPath, (new AssemblyName(args.Name)).Name + ".dll");
			if (!IO.File.Exists(assemblyPath))
			{
				return null;
			}
			Assembly assembly = Assembly.LoadFrom(assemblyPath);
			return assembly;
		}




	}
}