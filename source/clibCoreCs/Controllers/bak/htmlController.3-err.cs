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


	}
}
