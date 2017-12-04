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
		//   Legacy
		//========================================================================
		//
		public string getInputSelectList(string MenuName, string CurrentValue, string SelectList, string NoneCaption = "", string htmlId = "")
		{
			return getInputSelectList2(genericController.encodeText(MenuName), genericController.EncodeInteger(CurrentValue), genericController.encodeText(SelectList), genericController.encodeText(NoneCaption), genericController.encodeText(htmlId));
		}
		//
		//========================================================================
		//   Create a select list from a comma separated list
		//       returns an index into the list list, starting at 1
		//       if an element is blank (,) no option is created
		//========================================================================
		//
		public string getInputSelectList2(string MenuName, int CurrentValue, string SelectList, string NoneCaption, string htmlId, string HtmlClass = "")
		{
			try
			{
				//
				stringBuilderLegacyController FastString = new stringBuilderLegacyController();
				string[] lookups = null;
				string iSelectList = null;
				int Ptr = 0;
				int RecordID = 0;
				//Dim SelectedFound As Integer
				string Copy = null;
				string TagID = null;
				int SelectFieldWidthLimit;
				//
				SelectFieldWidthLimit = cpCore.siteProperties.selectFieldWidthLimit;
				if (SelectFieldWidthLimit == 0)
				{
					SelectFieldWidthLimit = 256;
				}
				//
				//iSelectList = genericController.encodeText(SelectList)
				//
				// ----- Start select box
				//
				FastString.Add("<select id=\"" + htmlId + "\" class=\"" + HtmlClass + "\" size=\"1\" name=\"" + MenuName + "\">");
				if (!string.IsNullOrEmpty(NoneCaption))
				{
					FastString.Add("<option value=\"\">" + NoneCaption + "</option>");
				}
				else
				{
					FastString.Add("<option value=\"\">Select One</option>");
				}
				//
				// ----- select values
				//
				lookups = SelectList.Split(',');
				for (Ptr = 0; Ptr <= lookups.GetUpperBound(0); Ptr++)
				{
					RecordID = Ptr + 1;
					Copy = lookups[Ptr];
					if (!string.IsNullOrEmpty(Copy))
					{
						FastString.Add(Environment.NewLine + "<option value=\"" + RecordID + "\" ");
						if (RecordID == CurrentValue)
						{
							FastString.Add("selected");
							//SelectedFound = True
						}
						if (Copy.Length > SelectFieldWidthLimit)
						{
							Copy = Copy.Substring(0, SelectFieldWidthLimit) + "...+";
						}
						FastString.Add(">" + Copy + "</option>");
					}
				}
				FastString.Add("</select>");
				return FastString.Text;
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
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError13("main_GetFormInputSelectList2")
		}
		//
		//========================================================================
		//   Display an icon with a link to the login form/cclib.net/admin area
		//========================================================================
		//
		public string main_GetLoginLink()
		{
			string result = string.Empty;
			try
			{
				//
				//If Not (true) Then Exit Function
				//
				string Link = null;
				string IconFilename = null;
				//
				if (cpCore.siteProperties.getBoolean("AllowLoginIcon", true))
				{
					result = result + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">";
					result = result + "<tr><td align=\"right\">";
					if (cpCore.doc.authContext.isAuthenticatedContentManager(cpCore))
					{
						result = result + "<a href=\"" + encodeHTML("/" + cpCore.serverConfig.appConfig.adminRoute) + "\" target=\"_blank\">";
					}
					else
					{
						Link = cpCore.webServer.requestPage + "?" + cpCore.doc.refreshQueryString;
						Link = genericController.modifyLinkQuery(Link, RequestNameHardCodedPage, HardCodedPageLogin, true);
						//Link = genericController.modifyLinkQuery(Link, RequestNameInterceptpage, LegacyInterceptPageSNLogin, True)
						result = result + "<a href=\"" + encodeHTML(Link) + "\" >";
					}
					IconFilename = cpCore.siteProperties.LoginIconFilename;
					if (genericController.vbLCase(IconFilename.Substring(0, 7)) != "/ccLib/")
					{
						IconFilename = genericController.getCdnFileLink(cpCore, IconFilename);
					}
					result = result + "<img alt=\"Login\" src=\"" + IconFilename + "\" border=\"0\" >";
					result = result + "</A>";
					result = result + "</td></tr></table>";
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
		//'   legacy
		//'========================================================================
		//'
		//Public Function main_GetClosePage(Optional ByVal AllowLogin As Boolean = True, Optional ByVal AllowTools As Boolean = True) As String
		//    main_GetClosePage = main_GetClosePage3(AllowLogin, AllowTools, False, False)
		//End Function
		//'
		//'========================================================================
		//'   legacy
		//'========================================================================
		//'
		//Public Function main_GetClosePage2(AllowLogin As Boolean, AllowTools As Boolean, BlockNonContentExtras As Boolean) As String
		//    Try
		//        main_GetClosePage2 = main_GetClosePage3(AllowLogin, AllowTools, False, False)
		//    Catch ex As Exception
		//        cpCore.handleExceptionAndContinue(ex) : Throw
		//    End Try
		//End Function
		//'
		//'========================================================================
		//'   main_GetClosePage3
		//'       Public interface to end the page call
		//'       Must be called last on every public page
		//'       internally, you can NOT writeAltBuffer( main_GetClosePage3 ) because the stream is closed
		//'       call main_GetEndOfBody - main_Gets toolspanel and all html,menuing,etc needed to finish page
		//'       optionally calls main_dispose
		//'========================================================================
		//'
		//Public Function main_GetClosePage3(AllowLogin As Boolean, AllowTools As Boolean, BlockNonContentExtras As Boolean, doNotDisposeOnExit As Boolean) As String
		//    Try
		//        Return getBeforeEndOfBodyHtml(AllowLogin, AllowTools, BlockNonContentExtras, False)
		//    Catch ex As Exception
		//        cpCore.handleExceptionAndContinue(ex) : Throw
		//    End Try
		//End Function
		//        '
		//        '========================================================================
		//        '   Write to the HTML stream
		//        '========================================================================
		//        ' refactor -- if this conversion goes correctly, all writeStream will mvoe to teh executeRoute which returns the string 
		//        Public Sub writeAltBuffer(ByVal Message As Object)
		//            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("WriteStream")
		//            '
		//            If cpCore.doc.continueProcessing Then
		//                Select Case cpCore.webServer.outStreamDevice
		//                    Case htmlDoc_OutStreamJavaScript
		//                        Call webServerIO_JavaStream_Add(genericController.encodeText(Message))
		//                    Case Else

		//                        If (cpCore.webServer.iisContext IsNot Nothing) Then
		//                            cpCore.doc.isStreamWritten = True
		//                            Call cpCore.webServer.iisContext.Response.Write(genericController.encodeText(Message))
		//                        Else
		//                            cpCore.doc.docBuffer = cpCore.doc.docBuffer & genericController.encodeText(Message)
		//                        End If
		//                End Select
		//            End If
		//            '
		//            Exit Sub
		//ErrorTrap:
		//            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("writeAltBuffer")
		//        End Sub

		//        '
		//        '
		//        Private Sub webServerIO_JavaStream_Add(ByVal NewString As String)
		//            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00375")
		//            '
		//            If cpCore.doc.javascriptStreamCount >= cpCore.doc.javascriptStreamSize Then
		//                cpCore.doc.javascriptStreamSize = cpCore.doc.javascriptStreamSize + htmlDoc_JavaStreamChunk
		//                ReDim Preserve cpCore.doc.javascriptStreamHolder(cpCore.doc.javascriptStreamSize)
		//            End If
		//            cpCore.doc.javascriptStreamHolder(cpCore.doc.javascriptStreamCount) = NewString
		//            cpCore.doc.javascriptStreamCount = cpCore.doc.javascriptStreamCount + 1
		//            Exit Sub
		//            '
		//ErrorTrap:
		//            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_JavaStream_Add")
		//        End Sub



		//Public ReadOnly Property webServerIO_JavaStream_Text() As String
		//    Get
		//        Dim MsgLabel As String

		//        MsgLabel = "Msg" & genericController.encodeText(genericController.GetRandomInteger)

		//        webServerIO_JavaStream_Text = Join(cpCore.doc.javascriptStreamHolder, "")
		//        webServerIO_JavaStream_Text = genericController.vbReplace(webServerIO_JavaStream_Text, "'", "'+""'""+'")
		//        webServerIO_JavaStream_Text = genericController.vbReplace(webServerIO_JavaStream_Text, vbCrLf, "\n")
		//        webServerIO_JavaStream_Text = genericController.vbReplace(webServerIO_JavaStream_Text, vbCr, "\n")
		//        webServerIO_JavaStream_Text = genericController.vbReplace(webServerIO_JavaStream_Text, vbLf, "\n")
		//        webServerIO_JavaStream_Text = "var " & MsgLabel & " = '" & webServerIO_JavaStream_Text & "'; document.write( " & MsgLabel & " ); " & vbCrLf

		//    End Get
		//End Property
		//'
		//'
		//'
		//Public Sub webServerIO_addRefreshQueryString(ByVal Name As String, Optional ByVal Value As String = "")
		//    Try
		//        Dim temp() As String
		//        '
		//        If (InStr(1, Name, "=") > 0) Then
		//            temp = Split(Name, "=")
		//            cpCore.doc.refreshQueryString = genericController.ModifyQueryString(cpCore.doc.refreshQueryString, temp(0), temp(1), True)
		//        Else
		//            cpCore.doc.refreshQueryString = genericController.ModifyQueryString(cpCore.doc.refreshQueryString, Name, Value, True)
		//        End If
		//    Catch ex As Exception
		//        cpCore.handleExceptionAndContinue(ex) : Throw
		//    End Try
		//
		//  End Sub
		//
		public string html_GetLegacySiteStyles()
		{
				string temphtml_GetLegacySiteStyles = null;
			try
			{
				//
				if (!cpCore.doc.legacySiteStyles_Loaded)
				{
					cpCore.doc.legacySiteStyles_Loaded = true;
					//
					// compatibility with old sites - if they do not main_Get the default style sheet, put it in here
					//
					if (false)
					{
						temphtml_GetLegacySiteStyles = ""
							+ "\r" + "<!-- compatibility with legacy framework --><style type=text/css>"
							+ "\r" + " .ccEditWrapper {border-top:1px solid #6a6;border-left:1px solid #6a6;border-bottom:1px solid #cec;border-right:1px solid #cec;}"
							+ "\r" + " .ccEditWrapperInner {border-top:1px solid #cec;border-left:1px solid #cec;border-bottom:1px solid #6a6;border-right:1px solid #6a6;}"
							+ "\r" + " .ccEditWrapperCaption {text-align:left;border-bottom:1px solid #888;padding:4px;background-color:#40C040;color:black;}"
							+ "\r" + " .ccEditWrapperContent{padding:4px;}"
							+ "\r" + " .ccHintWrapper {border:1px dashed #888;margin-bottom:10px}"
							+ "\r" + " .ccHintWrapperContent{padding:10px;background-color:#80E080;color:black;}"
							+ "</style>";
					}
					else
					{
						temphtml_GetLegacySiteStyles = ""
							+ "\r" + "<!-- compatibility with legacy framework --><style type=text/css>"
							+ "\r" + " .ccEditWrapper {border:1px dashed #808080;}"
							+ "\r" + " .ccEditWrapperCaption {text-align:left;border-bottom:1px solid #808080;padding:4px;background-color:#40C040;color:black;}"
							+ "\r" + " .ccEditWrapperContent{padding:4px;}"
							+ "\r" + " .ccHintWrapper {border:1px dashed #808080;margin-bottom:10px}"
							+ "\r" + " .ccHintWrapperContent{padding:10px;background-color:#80E080;color:black;}"
							+ "</style>";
					}
				}
				//
				return temphtml_GetLegacySiteStyles;
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError13("main_GetLegacySiteStyles")
			return temphtml_GetLegacySiteStyles;
		}
		//
		//===================================================================================================
		//   Wrap the content in a common wrapper if authoring is enabled
		//===================================================================================================
		//
		public string html_GetAdminHintWrapper(string Content)
		{
				string temphtml_GetAdminHintWrapper = null;
			try
			{
				//
				//If Not (true) Then Exit Function
				//
				temphtml_GetAdminHintWrapper = "";
				if (cpCore.doc.authContext.isEditing("") | cpCore.doc.authContext.isAuthenticatedAdmin(cpCore))
				{
					temphtml_GetAdminHintWrapper = temphtml_GetAdminHintWrapper + html_GetLegacySiteStyles();
					temphtml_GetAdminHintWrapper = temphtml_GetAdminHintWrapper + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccHintWrapper\">"
							+ "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccHintWrapperContent\">"
							+ "<b>Administrator</b>"
							+ "<br>"
							+ "<br>" + genericController.encodeText(Content) + "</td></tr></table>"
						+ "</td></tr></table>";
				}

				return temphtml_GetAdminHintWrapper;
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_GetAdminHintWrapper")
			return temphtml_GetAdminHintWrapper;
		}
		//
		//
		//
		public void enableOutputBuffer(bool BufferOn)
		{
			try
			{
				if (cpCore.doc.outputBufferEnabled)
				{
					//
					// ----- once on, can not be turned off Response Object
					//
					cpCore.doc.outputBufferEnabled = BufferOn;
				}
				else
				{
					//
					// ----- StreamBuffer off, allow on and off
					//
					cpCore.doc.outputBufferEnabled = BufferOn;
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}

		//
		//========================================================================
		// ----- Starts an HTML form for uploads
		//       Should be closed with main_GetUploadFormEnd
		//========================================================================
		//
		public string html_GetUploadFormStart(string ActionQueryString = null)
		{
			try
			{
				if (ActionQueryString == null)
				{
					ActionQueryString = cpCore.doc.refreshQueryString;
				}
				//
				string iActionQueryString;
				//
				iActionQueryString = genericController.ModifyQueryString(ActionQueryString, RequestNameRequestBinary, true, true);
				//
				return "<form action=\"" + cpCore.webServer.serverFormActionURL + "?" + iActionQueryString + "\" ENCTYPE=\"MULTIPART/FORM-DATA\" METHOD=\"POST\"  style=\"display: inline;\" >";
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_GetUploadFormStart")
		}
		//
		//========================================================================
		// ----- Closes an HTML form for uploads
		//========================================================================
		//
		public string html_GetUploadFormEnd()
		{
			return html_GetFormEnd();
		}
		//
		//========================================================================
		// ----- Starts an HTML form
		//       Should be closed with PrintFormEnd
		//========================================================================
		//
		public string html_GetFormStart(string ActionQueryString = null, string htmlName = "", string htmlId = "", string htmlMethod = "")
		{
				string temphtml_GetFormStart = null;
			try
			{
				//
				//If Not (true) Then Exit Function
				//
				int Ptr = 0;
				string MethodName = null;
				string ActionQS = null;
				string iMethod = null;
				string[] ActionParts = null;
				string Action = null;
				string[] QSParts = null;
				string[] QSNameValues = null;
				string QSName = null;
				string QSValue = null;
				string RefreshHiddens = null;
				//
				MethodName = "main_GetFormStart3";
				//
				if (ActionQueryString == null)
				{
					ActionQS = cpCore.doc.refreshQueryString;
				}
				else
				{
					ActionQS = ActionQueryString;
				}
				iMethod = genericController.vbLCase(htmlMethod);
				if (string.IsNullOrEmpty(iMethod))
				{
					iMethod = "post";
				}
				RefreshHiddens = "";
				Action = cpCore.webServer.serverFormActionURL;
				//
				if (!string.IsNullOrEmpty(ActionQS))
				{
					if (iMethod != "main_Get")
					{
						//
						// non-main_Get, put Action QS on end of Action
						//
						Action = Action + "?" + ActionQS;
					}
					else
					{
						//
						// main_Get method, build hiddens for actionQS
						//
						QSParts = ActionQS.Split('&');
						for (Ptr = 0; Ptr <= QSParts.GetUpperBound(0); Ptr++)
						{
							QSNameValues = QSParts[Ptr].Split('=');
							if (QSNameValues.GetUpperBound(0) == 0)
							{
								QSName = genericController.DecodeResponseVariable(QSNameValues[0]);
							}
							else
							{
								QSName = genericController.DecodeResponseVariable(QSNameValues[0]);
								QSValue = genericController.DecodeResponseVariable(QSNameValues[1]);
								RefreshHiddens = RefreshHiddens + "\r" + "<input type=\"hidden\" name=\"" + encodeHTML(QSName) + "\" value=\"" + encodeHTML(QSValue) + "\">";
							}
						}
					}
				}
				//
				temphtml_GetFormStart = ""
					+ "\r" + "<form name=\"" + htmlName + "\" id=\"" + htmlId + "\" action=\"" + Action + "\" method=\"" + iMethod + "\" style=\"display: inline;\" >"
					+ RefreshHiddens + "";
				//
				return temphtml_GetFormStart;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18(MethodName)
			//
			return temphtml_GetFormStart;
		}
		//
		//========================================================================
		// ----- Ends an HTML form
		//========================================================================
		//
		public string html_GetFormEnd()
		{
			//
			return "</form>";
			//
		}
		//
		//
		//
		public string html_GetFormInputText(string TagName, string DefaultValue = "", string Height = "", string Width = "", string Id = "", bool PasswordField = false)
		{
			return html_GetFormInputText2(genericController.encodeText(TagName), genericController.encodeText(DefaultValue), genericController.EncodeInteger(Height), genericController.EncodeInteger(Width), genericController.encodeText(Id), PasswordField, false);
		}
		//
		//
		//
		public string html_GetFormInputText2(string htmlName, string DefaultValue = "", int Height = -1, int Width = -1, string HtmlId = "", bool PasswordField = false, bool Disabled = false, string HtmlClass = "")
		{
				string temphtml_GetFormInputText2 = null;
			try
			{
				//
				string iDefaultValue = null;
				int iWidth = 0;
				int iHeight = 0;
				string TagID = null;
				string TagDisabled = string.Empty;
				//
				if (true)
				{
					TagID = "";
					//
					iDefaultValue = encodeHTML(DefaultValue);
					if (!string.IsNullOrEmpty(HtmlId))
					{
						TagID = TagID + " id=\"" + genericController.encodeEmptyText(HtmlId, "") + "\"";
					}
					//
					if (!string.IsNullOrEmpty(HtmlClass))
					{
						TagID = TagID + " class=\"" + HtmlClass + "\"";
					}
					//
					iWidth = Width;
					if (iWidth <= 0)
					{
						iWidth = cpCore.siteProperties.defaultFormInputWidth;
					}
					//
					iHeight = Height;
					if (iHeight <= 0)
					{
						iHeight = cpCore.siteProperties.defaultFormInputTextHeight;
					}
					//
					if (Disabled)
					{
						TagDisabled = " disabled=\"disabled\"";
					}
					//
					if (PasswordField)
					{
						temphtml_GetFormInputText2 = "<input TYPE=\"password\" NAME=\"" + htmlName + "\" SIZE=\"" + iWidth + "\" VALUE=\"" + iDefaultValue + "\"" + TagID + TagDisabled + ">";
					}
					else if ((iHeight == 1) && (iDefaultValue.IndexOf("\"") + 1 == 0))
					{
						temphtml_GetFormInputText2 = "<input TYPE=\"Text\" NAME=\"" + htmlName + "\" SIZE=\"" + iWidth.ToString() + "\" VALUE=\"" + iDefaultValue + "\"" + TagID + TagDisabled + ">";
					}
					else
					{
						temphtml_GetFormInputText2 = "<textarea NAME=\"" + htmlName + "\" ROWS=\"" + iHeight.ToString() + "\" COLS=\"" + iWidth.ToString() + "\"" + TagID + TagDisabled + ">" + iDefaultValue + "</TEXTAREA>";
					}
					cpCore.doc.formInputTextCnt = cpCore.doc.formInputTextCnt + 1;
				}
				//
				return temphtml_GetFormInputText2;
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_GetFormInputText2")
			return temphtml_GetFormInputText2;
		}
		//
		//========================================================================
		// ----- main_Get an HTML Form text input (or text area)
		//========================================================================
		//
		public string html_GetFormInputTextExpandable(string TagName, string Value = "", int Rows = 0, string styleWidth = "100%", string Id = "", bool PasswordField = false)
		{
			if (Rows == 0)
			{
				Rows = cpCore.siteProperties.defaultFormInputTextHeight;
			}
			return html_GetFormInputTextExpandable2(TagName, Value, Rows, styleWidth, Id, PasswordField, false, "");
		}
		//
		//========================================================================
		// ----- main_Get an HTML Form text input (or text area)
		//   added disabled case
		//========================================================================
		//
		public string html_GetFormInputTextExpandable2(string TagName, string Value = "", int Rows = 0, string styleWidth = "100%", string Id = "", bool PasswordField = false, bool Disabled = false, string HtmlClass = "")
		{
				string temphtml_GetFormInputTextExpandable2 = null;
			try
			{
				string Tn = "cpCoreClass.GetFormInputTextExpandable2";
				//
				//If Not (true) Then Exit Function
				//
				string AttrDisabled = string.Empty;
				string Value_Local = null;
				string StyleWidth_Local = null;
				int Rows_Local = 0;
				string IDRoot = null;
				string EditorClosed = null;
				string EditorOpened = null;
				//
				Value_Local = encodeHTML(Value);
				IDRoot = Id;
				if (string.IsNullOrEmpty(IDRoot))
				{
					IDRoot = "TextArea" + cpCore.doc.formInputTextCnt;
				}
				//
				StyleWidth_Local = styleWidth;
				if (string.IsNullOrEmpty(StyleWidth_Local))
				{
					StyleWidth_Local = "100%";
				}
				//
				Rows_Local = Rows;
				if (Rows_Local == 0)
				{
					//
					// need a default for this -- it should be different from a text, it should be for a textarea -- bnecause it is used differently
					//
					//Rows_Local = app.SiteProperty_DefaultFormInputTextHeight
					if (Rows_Local == 0)
					{
						Rows_Local = 10;
					}
				}
				if (Disabled)
				{
					AttrDisabled = " disabled=\"disabled\"";
				}
				//
				EditorClosed = ""
					+ "\r" + "<div class=\"ccTextAreaHead\" ID=\"" + IDRoot + "Head\">"
					+ cr2 + "<a href=\"#\" onClick=\"OpenTextArea('" + IDRoot + "');return false\"><img src=\"/ccLib/images/OpenUpRev1313.gif\" width=13 height=13 border=0>&nbsp;Full Screen</a>"
					+ "\r" + "</div>"
					+ "\r" + "<div class=\"ccTextArea\">"
					+ cr2 + "<textarea ID=\"" + IDRoot + "\" NAME=\"" + TagName + "\" ROWS=\"" + Rows_Local + "\" Style=\"width:" + StyleWidth_Local + ";\"" + AttrDisabled + " onkeydown=\"return cj.encodeTextAreaKey(this, event);\">" + Value_Local + "</TEXTAREA>"
					+ "\r" + "</div>"
					+ "";
				//
				EditorOpened = ""
					+ "\r" + "<div class=\"ccTextAreaHeCursorTypeEnum.ADOPENed\" style=\"display:none;\" ID=\"" + IDRoot + "HeCursorTypeEnum.ADOPENed\">"
					+ "\r" + "<a href=\"#\" onClick=\"CloseTextArea('" + IDRoot + "');return false\"><img src=\"/ccLib/images/OpenDownRev1313.gif\" width=13 height=13 border=0>&nbsp;Full Screen</a>"
					+ cr2 + "</div>"
					+ "\r" + "<textarea class=\"ccTextAreaOpened\" style=\"display:none;\" ID=\"" + IDRoot + "Opened\" NAME=\"" + IDRoot + "Opened\"" + AttrDisabled + " onkeydown=\"return cj.encodeTextAreaKey(this, event);\"></TEXTAREA>";
				//
				temphtml_GetFormInputTextExpandable2 = ""
					+ "<div class=\"" + HtmlClass + "\">"
					+ genericController.htmlIndent(EditorClosed) + genericController.htmlIndent(EditorOpened) + "</div>";
				cpCore.doc.formInputTextCnt = cpCore.doc.formInputTextCnt + 1;
				return temphtml_GetFormInputTextExpandable2;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			cpCore.handleException(new Exception("Unexpected exception"));
			//
			return temphtml_GetFormInputTextExpandable2;
		}
		//
		//
		//
		public string html_GetFormInputDate(string TagName, string DefaultValue = "", string Width = "", string Id = "")
		{
			string result = string.Empty;
			try
			{
				string HeadJS = null;
				string DateString = string.Empty;
				DateTime DateValue = default(DateTime);
				string iDefaultValue = null;
				int iWidth = 0;
				string MethodName = null;
				string iTagName = null;
				string TagID = null;
				string CalendarObjName = null;
				string AnchorName = null;
				//
				MethodName = "main_GetFormInputDate";
				//
				iTagName = genericController.encodeText(TagName);
				iDefaultValue = genericController.encodeEmptyText(DefaultValue, "");
				if ((iDefaultValue == "0") || (iDefaultValue == "12:00:00 AM"))
				{
					iDefaultValue = "";
				}
				else
				{
					iDefaultValue = encodeHTML(iDefaultValue);
				}
				if (genericController.encodeEmptyText(Id, "") != "")
				{
					TagID = " ID=\"" + genericController.encodeEmptyText(Id, "") + "\"";
				}
				//
				iWidth = genericController.encodeEmptyInteger(Width, 20);
				if (iWidth == 0)
				{
					iWidth = 20;
				}
				//
				CalendarObjName = "Cal" + cpCore.doc.inputDateCnt;
				AnchorName = "ACal" + cpCore.doc.inputDateCnt;

				if (cpCore.doc.inputDateCnt == 0)
				{
					HeadJS = ""
					+ Environment.NewLine + "<SCRIPT LANGUAGE=\"JavaScript\" SRC=\"/ccLib/mktree/CalendarPopup.js\"></SCRIPT>"
					+ Environment.NewLine + "<SCRIPT LANGUAGE=\"JavaScript\">"
					+ Environment.NewLine + "var cal = new CalendarPopup();"
					+ Environment.NewLine + "cal.showNavigationDropdowns();"
					+ Environment.NewLine + "</SCRIPT>";
					addScriptLink_Head("/ccLib/mktree/CalendarPopup.js", "Calendar Popup");
					addScriptCode_head("var cal=new CalendarPopup();cal.showNavigationDropdowns();", "Calendar Popup");
				}

				if (DateHelper.IsDate(iDefaultValue))
				{
					DateValue = genericController.EncodeDate(iDefaultValue);
					if (DateValue.Month < 10)
					{
						DateString = DateString + "0";
					}
					DateString = DateString + DateValue.Month + "/";
					if (DateValue.Day < 10)
					{
						DateString = DateString + "0";
					}
					DateString = DateString + DateValue.Day + "/" + DateValue.Year;
				}


				result = result + Environment.NewLine + "<input TYPE=\"text\" NAME=\"" + iTagName + "\" ID=\"" + iTagName + "\" VALUE=\"" + iDefaultValue + "\"  SIZE=\"" + iWidth + "\">"
				+ Environment.NewLine + "<a HREF=\"#\" Onclick = \"cal.select(document.getElementById('" + iTagName + "'),'" + AnchorName + "','MM/dd/yyyy','" + DateString + "'); return false;\" NAME=\"" + AnchorName + "\" ID=\"" + AnchorName + "\"><img title=\"Select a date\" alt=\"Select a date\" src=\"/ccLib/images/table.jpg\" width=12 height=10 border=0></A>"
				+ Environment.NewLine + "";

				cpCore.doc.inputDateCnt = cpCore.doc.inputDateCnt + 1;
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//
		//========================================================================
		// ----- main_Get an HTML Form file upload input
		//========================================================================
		//
		public string html_GetFormInputFile2(string TagName, string htmlId = "", string HtmlClass = "")
		{
			//
			return "<input TYPE=\"file\" name=\"" + TagName + "\" id=\"" + htmlId + "\" class=\"" + HtmlClass + "\">";
			//
		}
		//
		// ----- main_Get an HTML Form file upload input
		//
		public string html_GetFormInputFile(string TagName, string htmlId = "")
		{
			//
			return html_GetFormInputFile2(TagName, htmlId);
			//
		}
		//
		//========================================================================
		// ----- main_Get an HTML Form input
		//========================================================================
		//
		public string html_GetFormInputRadioBox(string TagName, string TagValue, string CurrentValue, string htmlId = "")
		{
				string temphtml_GetFormInputRadioBox = null;
			try
			{
				//
				//If Not (true) Then Exit Function
				//
				string MethodName = null;
				string iTagName = null;
				string iTagValue = null;
				string iCurrentValue = null;
				string ihtmlId = null;
				string TagID = string.Empty;
				//
				iTagName = genericController.encodeText(TagName);
				iTagValue = genericController.encodeText(TagValue);
				iCurrentValue = genericController.encodeText(CurrentValue);
				ihtmlId = genericController.encodeEmptyText(htmlId, "");
				if (!string.IsNullOrEmpty(ihtmlId))
				{
					TagID = " ID=\"" + ihtmlId + "\"";
				}
				//
				MethodName = "main_GetFormInputRadioBox";
				//
				if (iTagValue == iCurrentValue)
				{
					temphtml_GetFormInputRadioBox = "<input TYPE=\"Radio\" NAME=\"" + iTagName + "\" VALUE=\"" + iTagValue + "\" checked" + TagID + ">";
				}
				else
				{
					temphtml_GetFormInputRadioBox = "<input TYPE=\"Radio\" NAME=\"" + iTagName + "\" VALUE=\"" + iTagValue + "\"" + TagID + ">";
				}
				//
				return temphtml_GetFormInputRadioBox;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18(MethodName)
			//
			return temphtml_GetFormInputRadioBox;
		}
		//
		//========================================================================
		//   Legacy
		//========================================================================
		//
		public string html_GetFormInputCheckBox(string TagName, string DefaultValue = "", string htmlId = "")
		{
			return html_GetFormInputCheckBox2(genericController.encodeText(TagName), genericController.EncodeBoolean(DefaultValue), genericController.encodeText(htmlId));
		}
		//
		//========================================================================
		//
		//========================================================================
		//
		public string html_GetFormInputCheckBox2(string TagName, bool DefaultValue = false, string HtmlId = "", bool Disabled = false, string HtmlClass = "")
		{
				string temphtml_GetFormInputCheckBox2 = null;
			try
			{
				//
				temphtml_GetFormInputCheckBox2 = "<input TYPE=\"CheckBox\" NAME=\"" + TagName + "\" VALUE=\"1\"";
				if (!string.IsNullOrEmpty(HtmlId))
				{
					temphtml_GetFormInputCheckBox2 = temphtml_GetFormInputCheckBox2 + " id=\"" + HtmlId + "\"";
				}
				if (!string.IsNullOrEmpty(HtmlClass))
				{
					temphtml_GetFormInputCheckBox2 = temphtml_GetFormInputCheckBox2 + " class=\"" + HtmlClass + "\"";
				}
				if (DefaultValue)
				{
					temphtml_GetFormInputCheckBox2 = temphtml_GetFormInputCheckBox2 + " checked=\"checked\"";
				}
				if (Disabled)
				{
					temphtml_GetFormInputCheckBox2 = temphtml_GetFormInputCheckBox2 + " disabled=\"disabled\"";
				}
				return temphtml_GetFormInputCheckBox2 + ">";
				//
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_GetFormInputCheckBox2")
			return temphtml_GetFormInputCheckBox2;
		}
		//
		//========================================================================
		//   Create a List of Checkboxes based on a contentname and a list of IDs that should be checked
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
		public string html_GetFormInputCheckListByIDList(string TagName, string SecondaryContentName, string CheckedIDList, string CaptionFieldName = "", bool readOnlyField = false)
		{
			try
			{
				//
				//If Not (true) Then Exit Function
				//
				string SQL = null;
				int CS = 0;
				int main_MemberShipCount = 0;
				int main_MemberShipSize = 0;
				int main_MemberShipPointer = 0;
				string SectionName = null;
				int GroupCount = 0;
				int[] main_MemberShip = null;
				string SecondaryTablename = null;
				int SecondaryContentID = 0;
				string rulesTablename = null;
				string Result = string.Empty;
				string MethodName = null;
				string iCaptionFieldName = null;
				string GroupName = null;
				string GroupCaption = null;
				bool CanSeeHiddenFields = false;
				Models.Complex.cdefModel SecondaryCDef = null;
				string ContentIDList = string.Empty;
				bool Found = false;
				int RecordID = 0;
				string SingularPrefix = null;
				//
				MethodName = "main_GetFormInputCheckListByIDList";
				//
				iCaptionFieldName = genericController.encodeEmptyText(CaptionFieldName, "name");
				//
				// ----- Gather all the SecondaryContent that associates to the PrimaryContent
				//
				SecondaryCDef = Models.Complex.cdefModel.getCdef(cpCore, SecondaryContentName);
				SecondaryTablename = SecondaryCDef.ContentTableName;
				SecondaryContentID = SecondaryCDef.Id;
				SecondaryCDef.childIdList(cpCore).Add(SecondaryContentID);
				SingularPrefix = genericController.GetSingular(SecondaryContentName) + "&nbsp;";
				//
				// ----- Gather all the records, sorted by ContentName
				//
				SQL = "SELECT " + SecondaryTablename + ".ID AS ID, ccContent.Name AS SectionName, " + SecondaryTablename + "." + iCaptionFieldName + " AS GroupCaption, " + SecondaryTablename + ".name AS GroupName, " + SecondaryTablename + ".SortOrder"
				+ " FROM " + SecondaryTablename + " LEFT JOIN ccContent ON " + SecondaryTablename + ".ContentControlID = ccContent.ID"
				+ " Where (" + SecondaryTablename + ".Active<>" + SQLFalse + ")"
				+ " And (ccContent.Active<>" + SQLFalse + ")"
				+ " And (" + SecondaryTablename + ".ContentControlID IN (" + ContentIDList + "))";
				SQL += ""
					+ " GROUP BY " + SecondaryTablename + ".ID, ccContent.Name, " + SecondaryTablename + "." + iCaptionFieldName + ", " + SecondaryTablename + ".name, " + SecondaryTablename + ".SortOrder"
					+ " ORDER BY ccContent.Name, " + SecondaryTablename + "." + iCaptionFieldName;
				CS = cpCore.db.csOpenSql(SQL);
				if (cpCore.db.csOk(CS))
				{
					SectionName = "";
					GroupCount = 0;
					CanSeeHiddenFields = cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore);
					while (cpCore.db.csOk(CS))
					{
						GroupName = cpCore.db.csGetText(CS, "GroupName");
						if ((GroupName.Substring(0, 1) != "_") || CanSeeHiddenFields)
						{
							RecordID = cpCore.db.csGetInteger(CS, "ID");
							GroupCaption = cpCore.db.csGetText(CS, "GroupCaption");
							if (string.IsNullOrEmpty(GroupCaption))
							{
								GroupCaption = GroupName;
							}
							if (string.IsNullOrEmpty(GroupCaption))
							{
								GroupCaption = SingularPrefix + RecordID;
							}
							if (GroupCount != 0)
							{
								// leave this between checkboxes - it is searched in the admin page
								Result = Result + "<br >" + Environment.NewLine;
							}
							if (genericController.IsInDelimitedString(CheckedIDList, RecordID.ToString(), ","))
							{
								Found = true;
							}
							else
							{
								Found = false;
							}
							// must leave the first hidden with the value in this form - it is searched in the admin pge
							Result = Result + "<input type=hidden name=\"" + TagName + "." + GroupCount + ".ID\" value=" + RecordID + ">";
							if (readOnlyField && !Found)
							{
								Result = Result + "<input type=checkbox disabled>";
							}
							else if (readOnlyField)
							{
								Result = Result + "<input type=checkbox disabled checked>";
								Result = Result + "<input type=\"hidden\" name=\"" + TagName + "." + GroupCount + ".ID\" value=" + RecordID + ">";
							}
							else if (Found)
							{
								Result = Result + "<input type=checkbox name=\"" + TagName + "." + GroupCount + "\" checked>";
							}
							else
							{
								Result = Result + "<input type=checkbox name=\"" + TagName + "." + GroupCount + "\">";
							}
							Result = Result + SpanClassAdminNormal + GroupCaption;
							GroupCount = GroupCount + 1;
						}
						cpCore.db.csGoNext(CS);
					}
					Result = Result + "<input type=\"hidden\" name=\"" + TagName + ".RowCount\" value=\"" + GroupCount + "\">" + Environment.NewLine;
				}
				cpCore.db.csClose(CS);
				return Result;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18(MethodName)
			//
		}
		//
		// -----
		//
		public string html_GetFormInputCS(int CSPointer, string ContentName, string FieldName, int Height = 1, int Width = 40, string htmlId = "")
		{
			string returnResult = string.Empty;
			try
			{
				bool IsEmptyList = false;
				string Stream = null;
				string MethodName = null;
				string FieldCaption = null;
				string FieldValueVariant = string.Empty;
				string FieldValueText = null;
				int FieldValueInteger = 0;
				int fieldTypeId = 0;
				bool FieldReadOnly = false;
				bool FieldPassword = false;
				bool fieldFound = false;
				int FieldLookupContentID = 0;
				int FieldMemberSelectGroupID = 0;
				string FieldLookupContentName = null;
				Models.Complex.cdefModel Contentdefinition = null;
				bool FieldHTMLContent = false;
				int CSLookup = 0;
				string FieldLookupList = string.Empty;
				//
				MethodName = "main_GetFormInputCS";
				//
				Stream = "";
				if (true)
				{
					fieldFound = false;
					Contentdefinition = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
					foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in Contentdefinition.fields)
					{
						Models.Complex.CDefFieldModel field = keyValuePair.Value;
						if (genericController.vbUCase(field.nameLc) == genericController.vbUCase(FieldName))
						{
							FieldValueVariant = field.defaultValue;
							fieldTypeId = field.fieldTypeId;
							FieldReadOnly = field.ReadOnly;
							FieldCaption = field.caption;
							FieldPassword = field.Password;
							FieldHTMLContent = field.htmlContent;
							FieldLookupContentID = field.lookupContentID;
							FieldLookupList = field.lookupList;
							FieldMemberSelectGroupID = field.MemberSelectGroupID;
							fieldFound = true;
						}
					}
					if (!fieldFound)
					{
						cpCore.handleException(new Exception("Field [" + FieldName + "] was not found in Content Definition [" + ContentName + "]"));
					}
					else
					{
						//
						// main_Get the current value if the record was found
						//
						if (cpCore.db.csOk(CSPointer))
						{
							FieldValueVariant = cpCore.db.cs_getValue(CSPointer, FieldName);
						}
						//
						if (FieldPassword)
						{
							//
							// Handle Password Fields
							//
							FieldValueText = genericController.encodeText(FieldValueVariant);
							returnResult = html_GetFormInputText2(FieldName, FieldValueText, Height, Width, "", true);
						}
						else
						{
							//
							// Non Password field by fieldtype
							//
							switch (fieldTypeId)
							{
							//
							//
							//
								case FieldTypeIdHTML:
									FieldValueText = genericController.encodeText(FieldValueVariant);
									if (FieldReadOnly)
									{
										returnResult = FieldValueText;
									}
									else
									{
										returnResult = getFormInputHTML(FieldName, FieldValueText, "", Width.ToString());
									}
								//
								// html files, read from cdnFiles and use html editor
								//
									break;
								case FieldTypeIdFileHTML:
									FieldValueText = genericController.encodeText(FieldValueVariant);
									if (!string.IsNullOrEmpty(FieldValueText))
									{
										FieldValueText = cpCore.cdnFiles.readFile(FieldValueText);
									}
									if (FieldReadOnly)
									{
										returnResult = FieldValueText;
									}
									else
									{
										//Height = encodeEmptyInteger(Height, 4)
										returnResult = getFormInputHTML(FieldName, FieldValueText, "", Width.ToString());
									}
								//
								// text cdnFiles files, read from cdnFiles and use text editor
								//
									break;
								case FieldTypeIdFileText:
									FieldValueText = genericController.encodeText(FieldValueVariant);
									if (!string.IsNullOrEmpty(FieldValueText))
									{
										FieldValueText = cpCore.cdnFiles.readFile(FieldValueText);
									}
									if (FieldReadOnly)
									{
										returnResult = FieldValueText;
									}
									else
									{
										//Height = encodeEmptyInteger(Height, 4)
										returnResult = html_GetFormInputText2(FieldName, FieldValueText, Height, Width);
									}
								//
								// text public files, read from cpcore.cdnFiles and use text editor
								//
									break;
								case FieldTypeIdFileCSS:
								case FieldTypeIdFileXML:
								case FieldTypeIdFileJavascript:
									FieldValueText = genericController.encodeText(FieldValueVariant);
									if (!string.IsNullOrEmpty(FieldValueText))
									{
										FieldValueText = cpCore.cdnFiles.readFile(FieldValueText);
									}
									if (FieldReadOnly)
									{
										returnResult = FieldValueText;
									}
									else
									{
										//Height = encodeEmptyInteger(Height, 4)
										returnResult = html_GetFormInputText2(FieldName, FieldValueText, Height, Width);
									}
								//
								//
								//
									break;
								case FieldTypeIdBoolean:
									if (FieldReadOnly)
									{
										returnResult = genericController.encodeText(genericController.EncodeBoolean(FieldValueVariant));
									}
									else
									{
										returnResult = html_GetFormInputCheckBox2(FieldName, genericController.EncodeBoolean(FieldValueVariant));
									}
								//
								//
								//
									break;
								case FieldTypeIdAutoIdIncrement:
									returnResult = genericController.encodeText(genericController.EncodeNumber(FieldValueVariant));
								//
								//
								//
									break;
								case FieldTypeIdFloat:
								case FieldTypeIdCurrency:
								case FieldTypeIdInteger:
									FieldValueVariant = genericController.EncodeNumber(FieldValueVariant).ToString();
									if (FieldReadOnly)
									{
										returnResult = genericController.encodeText(FieldValueVariant);
									}
									else
									{
										returnResult = html_GetFormInputText2(FieldName, genericController.encodeText(FieldValueVariant), Height, Width);
									}
								//
								//
								//
									break;
								case FieldTypeIdFile:
									FieldValueText = genericController.encodeText(FieldValueVariant);
									if (FieldReadOnly)
									{
										returnResult = FieldValueText;
									}
									else
									{
										returnResult = FieldValueText + "<BR >change: " + html_GetFormInputFile(FieldName, genericController.encodeText(FieldValueVariant));
									}
								//
								//
								//
									break;
								case FieldTypeIdFileImage:
									FieldValueText = genericController.encodeText(FieldValueVariant);
									if (FieldReadOnly)
									{
										returnResult = FieldValueText;
									}
									else
									{
										returnResult = "<img src=\"" + genericController.getCdnFileLink(cpCore, FieldValueText) + "\"><BR >change: " + html_GetFormInputFile(FieldName, genericController.encodeText(FieldValueVariant));
									}
								//
								//
								//
									break;
								case FieldTypeIdLookup:
									FieldValueInteger = genericController.EncodeInteger(FieldValueVariant);
									FieldLookupContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, FieldLookupContentID);
									if (!string.IsNullOrEmpty(FieldLookupContentName))
									{
										//
										// Lookup into Content
										//
										if (FieldReadOnly)
										{
											CSPointer = cpCore.db.cs_open2(FieldLookupContentName, FieldValueInteger);
											if (cpCore.db.csOk(CSLookup))
											{
												returnResult = csController.getTextEncoded(cpCore, CSLookup, "name");
											}
											cpCore.db.csClose(CSLookup);
										}
										else
										{
											returnResult = main_GetFormInputSelect2(FieldName, FieldValueInteger, FieldLookupContentName, "", "", "", IsEmptyList);
										}
									}
									else if (!string.IsNullOrEmpty(FieldLookupList))
									{
										//
										// Lookup into LookupList
										//
										returnResult = getInputSelectList2(FieldName, FieldValueInteger, FieldLookupList, "", "");
									}
									else
									{
										//
										// Just call it text
										//
										returnResult = html_GetFormInputText2(FieldName, FieldValueInteger.ToString(), Height, Width);
									}
								//
								//
								//
									break;
								case FieldTypeIdMemberSelect:
									FieldValueInteger = genericController.EncodeInteger(FieldValueVariant);
									returnResult = getInputMemberSelect(FieldName, FieldValueInteger, FieldMemberSelectGroupID);
									//
									//
									//
									break;
								default:
									FieldValueText = genericController.encodeText(FieldValueVariant);
									if (FieldReadOnly)
									{
										returnResult = FieldValueText;
									}
									else
									{
										if (FieldHTMLContent)
										{
											returnResult = getFormInputHTML(FieldName, FieldValueText, Height.ToString(), Width.ToString(), FieldReadOnly, false);
											//main_GetFormInputCS = main_GetFormInputActiveContent(fieldname, FieldValueText, height, width)
										}
										else
										{
											returnResult = html_GetFormInputText2(FieldName, FieldValueText, Height, Width);
										}
									}
									break;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return returnResult;
		}
		//
		//========================================================================
		// ----- Print an HTML Form Button element named BUTTON
		//========================================================================
		//
		public string html_GetFormButton(string ButtonLabel, string Name = "", string htmlId = "", string OnClick = "")
		{
			return html_GetFormButton2(ButtonLabel, Name, htmlId, OnClick, false);
		}
		//
		//========================================================================
		// ----- Print an HTML Form Button element named BUTTON
		//========================================================================
		//
		public string html_GetFormButton2(string ButtonLabel, string Name = "button", string htmlId = "", string OnClick = "", bool Disabled = false)
		{
			try
			{
				//
				//If Not (true) Then Exit Function
				//
				string MethodName = null;
				string iOnClick = null;
				string TagID = null;
				string s = null;
				//
				MethodName = "main_GetFormButton2";
				//
				s = "<input TYPE=\"SUBMIT\""
					+ " NAME=\"" + genericController.encodeEmptyText(Name, "button") + "\""
					+ " VALUE=\"" + genericController.encodeText(ButtonLabel) + "\""
					+ " OnClick=\"" + genericController.encodeEmptyText(OnClick, "") + "\""
					+ " ID=\"" + genericController.encodeEmptyText(htmlId, "") + "\"";
				if (Disabled)
				{
					s = s + " disabled=\"disabled\"";
				}
				return s + ">";
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18(MethodName)
			//
		}
		//
		//========================================================================
		// main_Gets a value in a hidden form field
		//   Handles name and value encoding
		//========================================================================
		//
		public string html_GetFormInputHidden(string TagName, string TagValue, string htmlId = "")
		{
			try
			{
				//
				//If Not (true) Then Exit Function
				//
				string iTagValue = null;
				string ihtmlId = null;
				string s;
				//
				s = "\r" + "<input type=\"hidden\" NAME=\"" + encodeHTML(genericController.encodeText(TagName)) + "\"";
				//
				iTagValue = encodeHTML(genericController.encodeText(TagValue));
				if (!string.IsNullOrEmpty(iTagValue))
				{
					s = s + " VALUE=\"" + iTagValue + "\"";
				}
				//
				ihtmlId = genericController.encodeText(htmlId);
				if (!string.IsNullOrEmpty(ihtmlId))
				{
					s = s + " ID=\"" + encodeHTML(ihtmlId) + "\"";
				}
				//
				s = s + ">";
				//
				return s;
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_GetFormInputHidden")
		}
		//
		public string html_GetFormInputHidden(string TagName, bool TagValue, string htmlId = "")
		{
			return html_GetFormInputHidden(TagName, TagValue.ToString(), htmlId);
		}
		//
		public string html_GetFormInputHidden(string TagName, int TagValue, string htmlId = "")
		{
			return html_GetFormInputHidden(TagName, TagValue.ToString(), htmlId);
		}
		//
		// Popup a separate window with the contents of a file
		//
		public string html_GetWindowOpenJScript(string URI, string WindowWidth = "", string WindowHeight = "", string WindowScrollBars = "", bool WindowResizable = true, string WindowName = "_blank")
		{
				string temphtml_GetWindowOpenJScript = null;
			try
			{
				//
				//If Not (true) Then Exit Function
				//
				string Delimiter = null;
				string MethodName = null;
				//
				temphtml_GetWindowOpenJScript = "";
				WindowName = genericController.encodeEmptyText(WindowName, "_blank");
				//
				MethodName = "main_GetWindowOpenJScript()";
				//
				// Added addl options from huhcorp.com sample
				//
				temphtml_GetWindowOpenJScript = temphtml_GetWindowOpenJScript + "window.open('" + URI + "', '" + WindowName + "'";
				temphtml_GetWindowOpenJScript = temphtml_GetWindowOpenJScript + ",'menubar=no,toolbar=no,location=no,status=no";
				Delimiter = ",";
				if (!genericController.isMissing(WindowWidth))
				{
					if (!string.IsNullOrEmpty(WindowWidth))
					{
						temphtml_GetWindowOpenJScript = temphtml_GetWindowOpenJScript + Delimiter + "width=" + WindowWidth;
						Delimiter = ",";
					}
				}
				if (!genericController.isMissing(WindowHeight))
				{
					if (!string.IsNullOrEmpty(WindowHeight))
					{
						temphtml_GetWindowOpenJScript = temphtml_GetWindowOpenJScript + Delimiter + "height=" + WindowHeight;
						Delimiter = ",";
					}
				}
				if (!genericController.isMissing(WindowScrollBars))
				{
					if (!string.IsNullOrEmpty(WindowScrollBars))
					{
						temphtml_GetWindowOpenJScript = temphtml_GetWindowOpenJScript + Delimiter + "scrollbars=" + WindowScrollBars;
						Delimiter = ",";
					}
				}
				if (WindowResizable)
				{
					temphtml_GetWindowOpenJScript = temphtml_GetWindowOpenJScript + Delimiter + "resizable";
					Delimiter = ",";
				}
				return temphtml_GetWindowOpenJScript + "')";
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18(MethodName)
			//
			return temphtml_GetWindowOpenJScript;
		}
		//
		// Popup a separate window with the contents of a file
		//
		public string html_GetWindowDialogJScript(string URI, string WindowWidth = "", string WindowHeight = "", bool WindowScrollBars = false, bool WindowResizable = false, string WindowName = "")
		{
				string temphtml_GetWindowDialogJScript = null;
			try
			{
				//
				//If Not (true) Then Exit Function
				//
				string Delimiter = null;
				string iWindowName = null;
				string MethodName = null;
				//
				iWindowName = genericController.encodeEmptyText(WindowName, "_blank");
				//
				MethodName = "main_GetWindowDialogJScript()";
				//
				// Added addl options from huhcorp.com sample
				//
				temphtml_GetWindowDialogJScript = "";
				temphtml_GetWindowDialogJScript = temphtml_GetWindowDialogJScript + "showModalDialog('" + URI + "', '" + iWindowName + "'";
				temphtml_GetWindowDialogJScript = temphtml_GetWindowDialogJScript + ",'status:false";
				if (!genericController.isMissing(WindowWidth))
				{
					if (!string.IsNullOrEmpty(WindowWidth))
					{
						temphtml_GetWindowDialogJScript = temphtml_GetWindowDialogJScript + ";dialogWidth:" + WindowWidth + "px";
					}
				}
				if (!genericController.isMissing(WindowHeight))
				{
					if (!string.IsNullOrEmpty(WindowHeight))
					{
						temphtml_GetWindowDialogJScript = temphtml_GetWindowDialogJScript + ";dialogHeight:" + WindowHeight + "px";
					}
				}
				if (WindowScrollBars)
				{
					temphtml_GetWindowDialogJScript = temphtml_GetWindowDialogJScript + ";scroll:yes";
				}
				if (WindowResizable)
				{
					temphtml_GetWindowDialogJScript = temphtml_GetWindowDialogJScript + ";resizable:yes";
				}
				return temphtml_GetWindowDialogJScript + "')";
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18(MethodName)
			//
			return temphtml_GetWindowDialogJScript;
		}
		//
		//=========================================================================================
		//
		//=========================================================================================
		//
		public void html_AddEvent(string HtmlId, string DOMEvent, string Javascript)
		{
			string JSCodeAsString = Javascript;
			JSCodeAsString = genericController.vbReplace(JSCodeAsString, "'", "'+\"'\"+'");
			JSCodeAsString = genericController.vbReplace(JSCodeAsString, Environment.NewLine, "\\n");
			JSCodeAsString = genericController.vbReplace(JSCodeAsString, "\r", "\\n");
			JSCodeAsString = genericController.vbReplace(JSCodeAsString, "\n", "\\n");
			JSCodeAsString = "'" + JSCodeAsString + "'";
			addScriptCode_onLoad("" + "cj.addListener(" + "document.getElementById('" + HtmlId + "')" + ",'" + DOMEvent + "'" + ",function(){eval(" + JSCodeAsString + ")}" + ")", "");
		}
		//
		//
		//
		public string html_GetFormInputField(string ContentName, string FieldName, string htmlName = "", string HtmlValue = "", string HtmlClass = "", string HtmlId = "", string HtmlStyle = "", int ManyToManySourceRecordID = 0)
		{
			string result = string.Empty;
			try
			{
				bool IgnoreBoolean = false;
				string LookupContentName = null;
				int fieldType = 0;
				string InputName = null;
				int GroupID = 0;
				Models.Complex.cdefModel CDef = null;
				string MTMContent0 = null;
				string MTMContent1 = null;
				string MTMRuleContent = null;
				string MTMRuleField0 = null;
				string MTMRuleField1 = null;
				//
				InputName = htmlName;
				if (string.IsNullOrEmpty(InputName))
				{
					InputName = FieldName;
				}
				//
				fieldType = genericController.EncodeInteger(Models.Complex.cdefModel.GetContentFieldProperty(cpCore, ContentName, FieldName, "type"));
				switch (fieldType)
				{
					case FieldTypeIdBoolean:
					{
						//
						//
						//
						result = html_GetFormInputCheckBox2(InputName, genericController.EncodeBoolean(HtmlValue) == true, HtmlId, false, HtmlClass);
						if (!string.IsNullOrEmpty(HtmlStyle))
						{
							result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
						}
						break;
					}
					case FieldTypeIdFileCSS:
					{
						//
						//
						//
						result = html_GetFormInputTextExpandable2(InputName, HtmlValue, 0, "100%", HtmlId, false, false, HtmlClass);
						if (!string.IsNullOrEmpty(HtmlStyle))
						{
							result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
						}
						break;
					}
					case FieldTypeIdCurrency:
					{
						//
						//
						//
						result = html_GetFormInputText2(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
						if (!string.IsNullOrEmpty(HtmlStyle))
						{
							result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
						}
						break;
					}
					case FieldTypeIdDate:
					{
						//
						//
						//
						result = html_GetFormInputDate(InputName, HtmlValue, "", HtmlId);
						if (!string.IsNullOrEmpty(HtmlClass))
						{
							result = genericController.vbReplace(result, ">", " class=\"" + HtmlClass + "\">");
						}
						if (!string.IsNullOrEmpty(HtmlStyle))
						{
							result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
						}
						break;
					}
					case FieldTypeIdFile:
					{
						//
						//
						//
						if (string.IsNullOrEmpty(HtmlValue))
						{
							result = html_GetFormInputFile2(InputName, HtmlId, HtmlClass);
						}
						else
						{

							string FieldValuefilename = "";
							string FieldValuePath = "";
							cpCore.cdnFiles.splitPathFilename(HtmlValue, FieldValuePath, FieldValuefilename);
							result = result + "<a href=\"http://" + genericController.EncodeURL(cpCore.webServer.requestDomain + genericController.getCdnFileLink(cpCore, HtmlValue)) + "\" target=\"_blank\">" + SpanClassAdminSmall + "[" + FieldValuefilename + "]</A>";
							result = result + "&nbsp;&nbsp;&nbsp;Delete:&nbsp;" + html_GetFormInputCheckBox2(InputName + ".Delete", false);
							result = result + "&nbsp;&nbsp;&nbsp;Change:&nbsp;" + html_GetFormInputFile2(InputName, HtmlId, HtmlClass);
						}
						if (!string.IsNullOrEmpty(HtmlStyle))
						{
							result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
						}
						break;
					}
					case FieldTypeIdFloat:
					{
						//
						//
						//
						result = html_GetFormInputText2(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
						if (!string.IsNullOrEmpty(HtmlStyle))
						{
							result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
						}
						break;
					}
					case FieldTypeIdFileImage:
					{
						//
						//
						//
						if (string.IsNullOrEmpty(HtmlValue))
						{
							result = html_GetFormInputFile2(InputName, HtmlId, HtmlClass);
						}
						else
						{
							string FieldValuefilename = "";
							string FieldValuePath = "";
							cpCore.cdnFiles.splitPathFilename(HtmlValue, FieldValuePath, FieldValuefilename);
							result = result + "<a href=\"http://" + genericController.EncodeURL(cpCore.webServer.requestDomain + genericController.getCdnFileLink(cpCore, HtmlValue)) + "\" target=\"_blank\">" + SpanClassAdminSmall + "[" + FieldValuefilename + "]</A>";
							result = result + "&nbsp;&nbsp;&nbsp;Delete:&nbsp;" + html_GetFormInputCheckBox2(InputName + ".Delete", false);
							result = result + "&nbsp;&nbsp;&nbsp;Change:&nbsp;" + html_GetFormInputFile2(InputName, HtmlId, HtmlClass);
						}
						if (!string.IsNullOrEmpty(HtmlStyle))
						{
							result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
						}
						break;
					}
					case FieldTypeIdInteger:
					{
						//
						//
						//
						result = html_GetFormInputText2(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
						if (!string.IsNullOrEmpty(HtmlStyle))
						{
							result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
						}
						break;
					}
					case FieldTypeIdFileJavascript:
					{
						//
						//
						//
						result = html_GetFormInputTextExpandable2(InputName, HtmlValue, 0, "100%", HtmlId, false, false, HtmlClass);
						if (!string.IsNullOrEmpty(HtmlStyle))
						{
							result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
						}
						break;
					}
					case FieldTypeIdLink:
					{
						//
						//
						//
						result = html_GetFormInputText2(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
						if (!string.IsNullOrEmpty(HtmlStyle))
						{
							result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
						}
						break;
					}
					case FieldTypeIdLookup:
					{
						//
						//
						//
						CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
						LookupContentName = "";
						foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in CDef.fields)
						{
							Models.Complex.CDefFieldModel field = keyValuePair.Value;
							if (genericController.vbUCase(field.nameLc) == genericController.vbUCase(FieldName))
							{
								if (field.lookupContentID != 0)
								{
									LookupContentName = genericController.encodeText(Models.Complex.cdefModel.getContentNameByID(cpCore, field.lookupContentID));
								}
								if (!string.IsNullOrEmpty(LookupContentName))
								{
									result = main_GetFormInputSelect2(InputName, genericController.EncodeInteger(HtmlValue), LookupContentName, "", "Select One", HtmlId, IgnoreBoolean, HtmlClass);
								}
								else if (field.lookupList != "")
								{
									result = getInputSelectList2(InputName, genericController.EncodeInteger(HtmlValue), field.lookupList, "Select One", HtmlId, HtmlClass);
								}
								if (!string.IsNullOrEmpty(HtmlStyle))
								{
									result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
								}
								break;
							}
						}
						break;
					}
					case FieldTypeIdManyToMany:
					{
						//
						//
						//
						CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
						var tempVar = CDef.fields(FieldName.ToLower());
						MTMContent0 = Models.Complex.cdefModel.getContentNameByID(cpCore, tempVar.contentId);
						MTMContent1 = Models.Complex.cdefModel.getContentNameByID(cpCore, tempVar.manyToManyContentID);
						MTMRuleContent = Models.Complex.cdefModel.getContentNameByID(cpCore, tempVar.manyToManyRuleContentID);
						MTMRuleField0 = tempVar.ManyToManyRulePrimaryField;
						MTMRuleField1 = tempVar.ManyToManyRuleSecondaryField;
						result = getCheckList(InputName, MTMContent0, ManyToManySourceRecordID, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, "", "", false);
						//result = getInputCheckListCategories(InputName, MTMContent0, ManyToManySourceRecordID, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, , , False, MTMContent1, HtmlValue)
						break;
					}
					case FieldTypeIdMemberSelect:
					{
						//
						//
						//
						GroupID = genericController.EncodeInteger(Models.Complex.cdefModel.GetContentFieldProperty(cpCore, ContentName, FieldName, "memberselectgroupid"));
						result = getInputMemberSelect(InputName, genericController.EncodeInteger(HtmlValue), GroupID,,, HtmlId);
						if (!string.IsNullOrEmpty(HtmlClass))
						{
							result = genericController.vbReplace(result, ">", " class=\"" + HtmlClass + "\">");
						}
						if (!string.IsNullOrEmpty(HtmlStyle))
						{
							result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
						}
						break;
					}
					case FieldTypeIdResourceLink:
					{
						//
						//
						//
						result = html_GetFormInputText2(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
						if (!string.IsNullOrEmpty(HtmlStyle))
						{
							result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
						}
						break;
					}
					case FieldTypeIdText:
					{
						//
						//
						//
						result = html_GetFormInputText2(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
						if (!string.IsNullOrEmpty(HtmlStyle))
						{
							result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
						}
						break;
					}
					case FieldTypeIdLongText:
					case FieldTypeIdFileText:
					{
						//
						//
						//
						result = html_GetFormInputTextExpandable2(InputName, HtmlValue, 0, "100%", HtmlId, false, false, HtmlClass);
						if (!string.IsNullOrEmpty(HtmlStyle))
						{
							result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
						}
						break;
					}
					case FieldTypeIdFileXML:
					{
						//
						//
						//
						result = html_GetFormInputTextExpandable2(InputName, HtmlValue, 0, "100%", HtmlId, false, false, HtmlClass);
						if (!string.IsNullOrEmpty(HtmlStyle))
						{
							result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
						}
						break;
					}
					case FieldTypeIdHTML:
					case FieldTypeIdFileHTML:
					{
						//
						//
						//
						result = getFormInputHTML(InputName, HtmlValue);
						if (!string.IsNullOrEmpty(HtmlStyle))
						{
							result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
						}
						if (!string.IsNullOrEmpty(HtmlClass))
						{
							result = genericController.vbReplace(result, ">", " class=\"" + HtmlClass + "\">");
						}
						break;
					}
					default:
					{
						//
						// unsupported field type
						//
					break;
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//'
		//'   renamed to AllowDebugging
		//'
		//Public ReadOnly Property visitProperty_AllowVerboseReporting() As Boolean
		//    Get
		//        Return visitProperty.getBoolean("AllowDebugging")
		//    End Get
		//End Property
		//        '
		//        '
		//        '
		//        Public Function main_parseJSON(ByVal Source As String) As Object
		//            On Error GoTo ErrorTrap 'Const Tn = "parseJSON" : ''Dim th as integer : th = profileLogMethodEnter(Tn)    '
		//            '
		//            main_parseJSON = common_jsonDeserialize(Source)
		//            '
		//            Exit Function
		//            '
		//            ' ----- Error Trap
		//            '
		//ErrorTrap:
		//            cpCore.handleExceptionAndContinue(New Exception("Unexpected exception"))
		//            '
		//        End Function
		//'
		//'
		//'
		//Public Function main_GetStyleSheet2(ByVal ContentType As csv_contentTypeEnum, Optional ByVal templateId As Integer = 0, Optional ByVal EmailID As Integer = 0) As String
		//    main_GetStyleSheet2 = html_getStyleSheet2(ContentType, templateId, EmailID)
		//End Function
		//
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


	}
}
