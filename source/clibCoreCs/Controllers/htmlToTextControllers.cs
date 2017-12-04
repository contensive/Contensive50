using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.Core.Controllers.genericController;

//
namespace Contensive.Core.Controllers
{
	public class htmlToTextControllers
	{
		//
		private coreClass cpCore;
		//
		public bool ConvertLinksToText;
		//
		//====================================================================================================
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="cpCore"></param>
		/// <remarks></remarks>
		public htmlToTextControllers(coreClass cpCore)
		{
			this.cpCore = cpCore;
		}
		//
		//========================================================================
		// Decode an HTML document back into plain text
		//========================================================================
		//
		public string convert(string Body)
		{
			string result = Body;
			try
			{
				string TextTest = null;
				bool BlockOpen = false;
				bool BlockOpenLast = false;
				bool BlockClose = false;
				bool BlockCloseLast = false;
				htmlParserController Parse = null;
				int ElementCount = 0;
				int ElementPointer = 0;
				string ElementText = null;
				string iBody = null;
				int LoopCount = 0;
				string LastHRef = "";
				int AttrCount = 0;
				int AttrPointer = 0;
				//
				result = "";
				if (!IsNull(Body))
				{
					iBody = Body;
					//
					// ----- Remove HTML whitespace
					//
					iBody = iBody.Replace("\n", " ");
					iBody = iBody.Replace("\r", " ");
					iBody = iBody.Replace("\t", " ");
					LoopCount = 0;
					while ((iBody.IndexOf("  ") >= 0) && (LoopCount < 1000))
					{
						iBody = iBody.Replace("  ", " ");
						LoopCount += 1;
					}
					//
					// ----- Remove HTML tags
					//
					Parse = new htmlParserController(cpCore);
					if (Parse != null)
					{
						Parse.Load(iBody);
						ElementCount = Parse.ElementCount;
						ElementPointer = 0;
						if (ElementCount > 0)
						{
							LoopCount = 0;
							BlockOpen = false;
							BlockClose = false;
							while ((ElementPointer < ElementCount) && (LoopCount < 100000))
							{
								if (!Parse.IsTag(ElementPointer))
								{
									ElementText = Parse.Text(ElementPointer).ToString();
									TextTest = ElementText;
									TextTest = genericController.vbReplace(TextTest, " ", "");
									TextTest = genericController.vbReplace(TextTest, "\r", "");
									TextTest = genericController.vbReplace(TextTest, "\n", "");
									TextTest = genericController.vbReplace(TextTest, "\t", "");
									if (!string.IsNullOrEmpty(TextTest))
									{
										//
										// if there is non-white space between tags, last element was no longer blockopen or closed
										//
										BlockOpen = false;
										BlockClose = false;
									}
								}
								else
								{
									ElementText = "";
									BlockOpenLast = BlockOpen;
									BlockCloseLast = BlockClose;
									BlockOpen = false;
									BlockClose = false;
									switch (genericController.vbUCase(Parse.TagName(ElementPointer)))
									{
										case "BR":
											//
											// ----- break
											//
											ElementText = Environment.NewLine;
											break;
										case "DIV":
										case "TD":
										case "LI":
										case "OL":
										case "UL":
										case "P":
										case "H1":
										case "H2":
										case "H3":
										case "H4":
										case "H5":
										case "H6":
											//
											// ----- Block tag open
											//
											BlockOpen = true;
											if (BlockOpenLast)
											{
												//
												// embedded block open, do nothing
												//
											}
											else if (BlockCloseLast)
											{
												//
												// block close did the crlf, do nothing
												//
											}
											else
											{
												//
												// new line
												//
												ElementText = Environment.NewLine;
											}
											break;
										case "/DIV":
										case "/TD":
										case "/LI":
										case "/OL":
										case "/UL":
										case "/P":
										case "/H1":
										case "/H2":
										case "/H3":
										case "/H4":
										case "/H5":
										case "/H6":
											//
											// ----- Block tag close
											//
											BlockClose = true;
											if (BlockCloseLast)
											{
												//
												// embedded block close, do nothing
												//
											}
											else
											{
												//
												// new line
												//
												ElementText = Environment.NewLine;
											}
										//Case "/OL", "/UL"
										//    '
										//    ' ----- Special cases, go to new line
										//    '
										//    ElementText = vbCrLf
										//Case "P"
										//    '
										//    ' ----- paragraph start, skip a line
										//    '
										//    ElementText = vbCrLf & vbCrLf
											break;
										case "/A":
											//
											// ----- end anchor, put the URL in parantheses
											//
											ElementText = "";
											if (ConvertLinksToText && (!string.IsNullOrEmpty(LastHRef)))
											{
												ElementText = " (" + LastHRef + ") ";
											}
											LastHRef = "";
											break;
										case "A":
											//
											// ----- paragraph start, skip a line
											//
											AttrCount = Parse.ElementAttributeCount(ElementPointer);
											if (AttrCount > 0)
											{
												for (AttrPointer = 0; AttrPointer < AttrCount; AttrPointer++)
												{
													if (genericController.vbUCase(Parse.ElementAttributeName(ElementPointer, AttrPointer)) == "HREF")
													{
														LastHRef = Parse.ElementAttributeValue(ElementPointer, AttrPointer);
														break;
													}
												}
											}
											ElementText = "";
										//LastHRef = Parse.ElementAttributeValue(ElementPointer, 1)
										//ElementText = vbCrLf & vbCrLf
											break;
										case "SCRIPT":
											//
											// ----- Script, skip to end of script
											//
											while (ElementPointer < ElementCount)
											{
												if (Parse.IsTag(ElementPointer))
												{
													if (genericController.vbUCase(Parse.TagName(ElementPointer)) == "/SCRIPT")
													{
														break;
													}
												}
												ElementPointer = ElementPointer + 1;
											}
											ElementText = "";
											break;
										case "STYLE":
											//
											// ----- style, skip to end of style
											//
											while (ElementPointer < ElementCount)
											{
												if (Parse.IsTag(ElementPointer))
												{
													if (genericController.vbUCase(Parse.TagName(ElementPointer)) == "/STYLE")
													{
														break;
													}
												}
												ElementPointer = ElementPointer + 1;
											}
											ElementText = "";
											break;
										case "HEAD":
											//
											// ----- head, skip to end of head
											//
											while (ElementPointer < ElementCount)
											{
												if (Parse.IsTag(ElementPointer))
												{
													if (genericController.vbUCase(Parse.TagName(ElementPointer)) == "/HEAD")
													{
														break;
													}
												}
												ElementPointer = ElementPointer + 1;
											}
											ElementText = "";
											break;
										default:
											//
											// ----- by default, just skip tags
											//
											ElementText = "";
											break;
									}
								}
								result = result + ElementText;
								ElementPointer = ElementPointer + 1;
								LoopCount += 1;
							}
						}
					}
					Parse = null;
					//
					// do HTML character substitutions
					//
					result = genericController.vbReplace(result, "&quot;", "\"");
					result = genericController.vbReplace(result, "&nbsp;", " ");
					result = genericController.vbReplace(result, "&lt;", "<");
					result = genericController.vbReplace(result, "&gt;", ">");
					result = genericController.vbReplace(result, "&amp;", "&");
					//
					// remove duplicate spaces
					//
					LoopCount = 0;
					while ((result.IndexOf("  ") + 1 != 0) && (LoopCount < 1000))
					{
						result = genericController.vbReplace(result, "  ", " ");
						LoopCount += 1;
					}
					//
					// Remove lines that are just spaces
					//
					LoopCount = 0;
					while ((result.IndexOf(Environment.NewLine + " ") + 1 != 0) && (LoopCount < 1000))
					{
						result = genericController.vbReplace(result, Environment.NewLine + " ", Environment.NewLine);
						LoopCount += 1;
					}
					//
					// remove long sets of extra line feeds
					//
					LoopCount = 0;
					while ((result.IndexOf(Environment.NewLine + Environment.NewLine + Environment.NewLine) + 1 != 0) && (LoopCount < 1000))
					{
						result = genericController.vbReplace(result, Environment.NewLine + Environment.NewLine + Environment.NewLine, Environment.NewLine + Environment.NewLine);
						LoopCount += 1;
					}
					//
					// Trim CR from the start
					//
					LoopCount = 0;
					while ((result.IndexOf(Environment.NewLine) + 1 == 1) && (LoopCount < 1000))
					{
						result = result.Substring(2);
						LoopCount += 1;
					}
					//
				}
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Exception during convertHtmltoText", ex);
			}
			return result;
		}
		//        '
		//        ' Returns a string with only the text part of the body
		//        '
		//        Private Function DecodeHTML_RemoveHTMLTags(ByVal Body As String) As String
		//            On Error GoTo ErrorTrap
		//            '
		//            Dim UcaseBody As String
		//            Dim Body2 As String
		//            Dim TagStart As Integer
		//            Dim TagEnd As Integer
		//            Dim TagString As String
		//            Dim ReplaceChar As String
		//            '
		//            ' Remove all html tags
		//            '
		//            UcaseBody = genericController.vbUCase(Body)
		//            Body2 = ""
		//            TagStart = genericController.vbInstr(1, UcaseBody, "<")
		//            TagEnd = 0
		//            ReplaceChar = ""
		//            Do While TagStart <> 0
		//                Body2 = Body2 & ReplaceChar & Mid(Body, TagEnd + 1, TagStart - TagEnd - 1)
		//                '
		//                ' Find the TagEnd
		//                '
		//                TagEnd = genericController.vbInstr(TagStart, UcaseBody, ">")
		//                ReplaceChar = ""
		//                If Mid(UcaseBody, TagStart, 4) = "<!--" Then
		//                    '
		//                    ' tag is a comment, skip over it
		//                    '
		//                    TagEnd = genericController.vbInstr(TagStart, UcaseBody, "-->")
		//                    If TagEnd <> 0 Then
		//                        TagEnd = TagEnd + 2
		//                    End If
		//                ElseIf Mid(UcaseBody, TagStart, 7) = "<SCRIPT" Then
		//                    '
		//                    ' tag is a comment, skip over it
		//                    '
		//                    TagEnd = genericController.vbInstr(TagStart, UcaseBody, "/SCRIPT>")
		//                    If TagEnd <> 0 Then
		//                        TagEnd = TagEnd + 7
		//                    End If
		//                    '
		//                    ' Tags used that divide words (not within words)
		//                    '
		//                ElseIf Mid(UcaseBody, TagStart, 3) = "<TD" Then
		//                    ReplaceChar = vbCrLf
		//                ElseIf Mid(UcaseBody, TagStart, 3) = "</TD" Then
		//                    ReplaceChar = vbCrLf
		//                ElseIf Mid(UcaseBody, TagStart, 3) = "<BR" Then
		//                    ReplaceChar = vbCrLf
		//                ElseIf Mid(UcaseBody, TagStart, 3) = "<P>" Then
		//                    ReplaceChar = vbCrLf
		//                ElseIf Mid(UcaseBody, TagStart, 3) = "</P>" Then
		//                    ReplaceChar = vbCrLf
		//                ElseIf Mid(UcaseBody, TagStart, 3) = "<P " Then
		//                    ReplaceChar = vbCrLf
		//                ElseIf Mid(UcaseBody, TagStart, 3) = "</P " Then
		//                    ReplaceChar = vbCrLf
		//                End If
		//                '
		//                If TagEnd = 0 Then
		//                    ' Call LogError(Doc, 20, "", TagStart, UcaseBody)
		//                    TagEnd = TagStart + 1
		//                End If
		//                '
		//                TagString = Mid(UcaseBody, TagStart, TagEnd - TagStart + 1)
		//                TagStart = genericController.vbInstr(TagEnd, UcaseBody, "<")
		//            Loop
		//            Body2 = Body2 & ReplaceChar & Mid(Body, TagEnd + 1)
		//            DecodeHTML_RemoveHTMLTags = Body2
		//            Exit Function
		//            '
		//            ' ----- Error Trap
		//            '
		//ErrorTrap:
		//            Err.Clear()
		//            Resume Next
		//        End Function
		//
		//=============================================================================
		// Remove all but a..z, A..Z
		//=============================================================================
		//
		private string DecodeHTML_RemoveWhiteSpace(string DirtyText)
		{
				string tempDecodeHTML_RemoveWhiteSpace = null;
			try
			{
				//
				string WorkingText = null;
				int Pointer = 0;
				int SpaceCounter = 0;
				string ChrTest = null;
				string ChrAllowed = null;
				int AscTest = 0;
				string BodyBuffer = null;
				//
				ChrAllowed = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ,.<>;:[{]}\\|`~!@#$%^&*()-_=+/?\"'";
				tempDecodeHTML_RemoveWhiteSpace = "";
				if (!IsNull(DirtyText))
				{
					if (!string.IsNullOrEmpty(DirtyText))
					{
						BodyBuffer = genericController.vbReplace(DirtyText, Environment.NewLine, "\r");
						BodyBuffer = genericController.vbReplace(BodyBuffer, "\n", "\r");
//INSTANT C# NOTE: The ending condition of VB 'For' loops is tested only on entry to the loop. Instant C# has created a temporary variable in order to use the initial value of Len(BodyBuffer) for every iteration:
						int tempVar = BodyBuffer.Length;
						for (Pointer = 1; Pointer <= tempVar; Pointer++)
						{
							ChrTest = BodyBuffer.Substring(Pointer - 1, 1);
							AscTest = Microsoft.VisualBasic.Strings.Asc(ChrTest);
							if (isInStr(1, ChrAllowed, ChrTest))
							{
								tempDecodeHTML_RemoveWhiteSpace = tempDecodeHTML_RemoveWhiteSpace + ChrTest;
								SpaceCounter = 0;
							}
							else
							{
								if (AscTest == Microsoft.VisualBasic.Strings.Asc("\r"))
								{
									tempDecodeHTML_RemoveWhiteSpace = tempDecodeHTML_RemoveWhiteSpace + Environment.NewLine;
									SpaceCounter = 0;
								}
								else if (ChrTest == " ")
								{
									if (SpaceCounter == 0)
									{
										SpaceCounter = 1;
										tempDecodeHTML_RemoveWhiteSpace = tempDecodeHTML_RemoveWhiteSpace + " ";
									}
								}
								else
								{
									SpaceCounter = 0;
								}
							}
						}
					}
				}
				return tempDecodeHTML_RemoveWhiteSpace.Trim(' ');
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
			Microsoft.VisualBasic.Information.Err().Clear();
//INSTANT C# TODO TASK: The 'Resume Next' statement is not converted by Instant C#:
			Resume Next
			return tempDecodeHTML_RemoveWhiteSpace;
		}
	}
}
