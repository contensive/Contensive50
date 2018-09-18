


// 

namespace Controllers {
    
    public class htmlToTextControllers {
        
        // 
        private coreClass cpCore;
        
        // 
        public bool ConvertLinksToText;
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' constructor
        // '' </summary>
        // '' <param name="cpCore"></param>
        // '' <remarks></remarks>
        public htmlToTextControllers(coreClass cpCore) {
            this.cpCore = cpCore;
        }
        
        // 
        // ========================================================================
        //  Decode an HTML document back into plain text
        // ========================================================================
        // 
        public string convert(string Body) {
            string result = Body;
            try {
                string TextTest;
                bool BlockOpen;
                bool BlockOpenLast;
                bool BlockClose;
                bool BlockCloseLast;
                htmlParserController Parse;
                int ElementCount;
                int ElementPointer;
                string ElementText;
                string iBody;
                int LoopCount;
                string LastHRef = "";
                int AttrCount;
                int AttrPointer;
                // 
                result = "";
                if (!IsNull(Body)) {
                    iBody = Body;
                    // 
                    //  ----- Remove HTML whitespace
                    // 
                    iBody = iBody.Replace("\n", " ");
                    iBody = iBody.Replace("\r", " ");
                    iBody = iBody.Replace('\t', " ");
                    LoopCount = 0;
                    while (((iBody.IndexOf("  ") >= 0) 
                                && (LoopCount < 1000))) {
                        iBody = iBody.Replace("  ", " ");
                        LoopCount++;
                    }
                    
                    // 
                    //  ----- Remove HTML tags
                    // 
                    Parse = new htmlParserController(cpCore);
                    if (!(Parse == null)) {
                        Parse.Load(iBody);
                        ElementCount = Parse.ElementCount;
                        ElementPointer = 0;
                        if ((ElementCount > 0)) {
                            LoopCount = 0;
                            BlockOpen = false;
                            BlockClose = false;
                            while (((ElementPointer < ElementCount) 
                                        && (LoopCount < 100000))) {
                                if (!Parse.IsTag(ElementPointer)) {
                                    ElementText = Parse.Text(ElementPointer);
                                    TextTest = ElementText;
                                    TextTest = genericController.vbReplace(TextTest, " ", "");
                                    TextTest = genericController.vbReplace(TextTest, "\r", "");
                                    TextTest = genericController.vbReplace(TextTest, "\n", "");
                                    TextTest = genericController.vbReplace(TextTest, '\t', "");
                                    if ((TextTest != "")) {
                                        // 
                                        //  if there is non-white space between tags, last element was no longer blockopen or closed
                                        // 
                                        BlockOpen = false;
                                        BlockClose = false;
                                    }
                                    
                                }
                                else {
                                    ElementText = "";
                                    BlockOpenLast = BlockOpen;
                                    BlockCloseLast = BlockClose;
                                    BlockOpen = false;
                                    BlockClose = false;
                                    switch (genericController.vbUCase(Parse.TagName(ElementPointer))) {
                                        case "BR":
                                            ElementText = "\r\n";
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
                                            BlockOpen = true;
                                            if (BlockOpenLast) {
                                                // 
                                                //  embedded block open, do nothing
                                                // 
                                            }
                                            else if (BlockCloseLast) {
                                                // 
                                                //  block close did the crlf, do nothing
                                                // 
                                            }
                                            else {
                                                // 
                                                //  new line
                                                // 
                                                ElementText = "\r\n";
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
                                            BlockClose = true;
                                            if (BlockCloseLast) {
                                                // 
                                                //  embedded block close, do nothing
                                                // 
                                            }
                                            else {
                                                // 
                                                //  new line
                                                // 
                                                ElementText = "\r\n";
                                            }
                                            
                                            // Case "/OL", "/UL"
                                            //     '
                                            //     ' ----- Special cases, go to new line
                                            //     '
                                            //     ElementText = vbCrLf
                                            // Case "P"
                                            //     '
                                            //     ' ----- paragraph start, skip a line
                                            //     '
                                            //     ElementText = vbCrLf & vbCrLf
                                            break;
                                        case "/A":
                                            ElementText = "";
                                            if ((ConvertLinksToText 
                                                        && (LastHRef != ""))) {
                                                ElementText = (" (" 
                                                            + (LastHRef + ") "));
                                            }
                                            
                                            LastHRef = "";
                                            break;
                                        case "A":
                                            AttrCount = Parse.ElementAttributeCount(ElementPointer);
                                            if ((AttrCount > 0)) {
                                                for (AttrPointer = 0; (AttrPointer 
                                                            <= (AttrCount - 1)); AttrPointer++) {
                                                    if ((genericController.vbUCase(Parse.ElementAttributeName(ElementPointer, AttrPointer)) == "HREF")) {
                                                        LastHRef = Parse.ElementAttributeValue(ElementPointer, AttrPointer);
                                                        break;
                                                    }
                                                    
                                                }
                                                
                                            }
                                            
                                            ElementText = "";
                                            break;
                                        case "SCRIPT":
                                            while ((ElementPointer < ElementCount)) {
                                                if (Parse.IsTag(ElementPointer)) {
                                                    if ((genericController.vbUCase(Parse.TagName(ElementPointer)) == "/SCRIPT")) {
                                                        break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                                                    }
                                                    
                                                }
                                                
                                                ElementPointer = (ElementPointer + 1);
                                            }
                                            
                                            ElementText = "";
                                            break;
                                        case "STYLE":
                                            while ((ElementPointer < ElementCount)) {
                                                if (Parse.IsTag(ElementPointer)) {
                                                    if ((genericController.vbUCase(Parse.TagName(ElementPointer)) == "/STYLE")) {
                                                        break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                                                    }
                                                    
                                                }
                                                
                                                ElementPointer = (ElementPointer + 1);
                                            }
                                            
                                            ElementText = "";
                                            break;
                                        case "HEAD":
                                            while ((ElementPointer < ElementCount)) {
                                                if (Parse.IsTag(ElementPointer)) {
                                                    if ((genericController.vbUCase(Parse.TagName(ElementPointer)) == "/HEAD")) {
                                                        break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                                                    }
                                                    
                                                }
                                                
                                                ElementPointer = (ElementPointer + 1);
                                            }
                                            
                                            ElementText = "";
                                            break;
                                        default:
                                            ElementText = "";
                                            break;
                                    }
                                }
                                
                                result = (result + ElementText);
                                ElementPointer = (ElementPointer + 1);
                                LoopCount++;
                            }
                            
                        }
                        
                    }
                    
                    Parse = null;
                    result = genericController.vbReplace(result, """, "\"");
                    result = genericController.vbReplace(result, " ", " ");
                    result = genericController.vbReplace(result, "<", "<");
                    result = genericController.vbReplace(result, ">", ">");
                    result = genericController.vbReplace(result, "&", "&");
                    // 
                    //  remove duplicate spaces
                    // 
                    LoopCount = 0;
                    while ((((result.IndexOf("  ", 0) + 1) 
                                != 0) 
                                && (LoopCount < 1000))) {
                        result = genericController.vbReplace(result, "  ", " ");
                        LoopCount++;
                    }
                    
                    // 
                    //  Remove lines that are just spaces
                    // 
                    LoopCount = 0;
                    while ((((result.IndexOf(("\r\n" + " "), 0) + 1) 
                                != 0) 
                                && (LoopCount < 1000))) {
                        result = genericController.vbReplace(result, ("\r\n" + " "), "\r\n");
                        LoopCount++;
                    }
                    
                    // 
                    //  remove long sets of extra line feeds
                    // 
                    LoopCount = 0;
                    while ((((result.IndexOf(("\r\n" + ("\r\n" + "\r\n")), 0) + 1) 
                                != 0) 
                                && (LoopCount < 1000))) {
                        result = genericController.vbReplace(result, ("\r\n" + ("\r\n" + "\r\n")), ("\r\n" + "\r\n"));
                        LoopCount++;
                    }
                    
                    // 
                    //  Trim CR from the start
                    // 
                    LoopCount = 0;
                    while ((((result.IndexOf("\r\n", 0) + 1) 
                                == 1) 
                                && (LoopCount < 1000))) {
                        result = result.Substring(2);
                        LoopCount++;
                    }
                    
                    // 
                }
                
            }
            catch (Exception ex) {
                throw new ApplicationException("Exception during convertHtmltoText", ex);
            }
            
            return result;
        }
        
        //         '
        //         ' Returns a string with only the text part of the body
        //         '
        //         Private Function DecodeHTML_RemoveHTMLTags(ByVal Body As String) As String
        //             On Error GoTo ErrorTrap
        //             '
        //             Dim UcaseBody As String
        //             Dim Body2 As String
        //             Dim TagStart As Integer
        //             Dim TagEnd As Integer
        //             Dim TagString As String
        //             Dim ReplaceChar As String
        //             '
        //             ' Remove all html tags
        //             '
        //             UcaseBody = genericController.vbUCase(Body)
        //             Body2 = ""
        //             TagStart = genericController.vbInstr(1, UcaseBody, "<")
        //             TagEnd = 0
        //             ReplaceChar = ""
        //             Do While TagStart <> 0
        //                 Body2 = Body2 & ReplaceChar & Mid(Body, TagEnd + 1, TagStart - TagEnd - 1)
        //                 '
        //                 ' Find the TagEnd
        //                 '
        //                 TagEnd = genericController.vbInstr(TagStart, UcaseBody, ">")
        //                 ReplaceChar = ""
        //                 If Mid(UcaseBody, TagStart, 4) = "<!--" Then
        //                     '
        //                     ' tag is a comment, skip over it
        //                     '
        //                     TagEnd = genericController.vbInstr(TagStart, UcaseBody, "-->")
        //                     If TagEnd <> 0 Then
        //                         TagEnd = TagEnd + 2
        //                     End If
        //                 ElseIf Mid(UcaseBody, TagStart, 7) = "<SCRIPT" Then
        //                     '
        //                     ' tag is a comment, skip over it
        //                     '
        //                     TagEnd = genericController.vbInstr(TagStart, UcaseBody, "/SCRIPT>")
        //                     If TagEnd <> 0 Then
        //                         TagEnd = TagEnd + 7
        //                     End If
        //                     '
        //                     ' Tags used that divide words (not within words)
        //                     '
        //                 ElseIf Mid(UcaseBody, TagStart, 3) = "<TD" Then
        //                     ReplaceChar = vbCrLf
        //                 ElseIf Mid(UcaseBody, TagStart, 3) = "</TD" Then
        //                     ReplaceChar = vbCrLf
        //                 ElseIf Mid(UcaseBody, TagStart, 3) = "<BR" Then
        //                     ReplaceChar = vbCrLf
        //                 ElseIf Mid(UcaseBody, TagStart, 3) = "<P>" Then
        //                     ReplaceChar = vbCrLf
        //                 ElseIf Mid(UcaseBody, TagStart, 3) = "</P>" Then
        //                     ReplaceChar = vbCrLf
        //                 ElseIf Mid(UcaseBody, TagStart, 3) = "<P " Then
        //                     ReplaceChar = vbCrLf
        //                 ElseIf Mid(UcaseBody, TagStart, 3) = "</P " Then
        //                     ReplaceChar = vbCrLf
        //                 End If
        //                 '
        //                 If TagEnd = 0 Then
        //                     ' Call LogError(Doc, 20, "", TagStart, UcaseBody)
        //                     TagEnd = TagStart + 1
        //                 End If
        //                 '
        //                 TagString = Mid(UcaseBody, TagStart, TagEnd - TagStart + 1)
        //                 TagStart = genericController.vbInstr(TagEnd, UcaseBody, "<")
        //             Loop
        //             Body2 = Body2 & ReplaceChar & Mid(Body, TagEnd + 1)
        //             DecodeHTML_RemoveHTMLTags = Body2
        //             Exit Function
        //             '
        //             ' ----- Error Trap
        //             '
        // ErrorTrap:
        //             Err.Clear()
        //             Resume Next
        //         End Function
        // 
        // =============================================================================
        //  Remove all but a..z, A..Z
        // =============================================================================
        // 
        private string DecodeHTML_RemoveWhiteSpace(string DirtyText) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            string WorkingText;
            int Pointer;
            int SpaceCounter;
            string ChrTest;
            string ChrAllowed;
            int AscTest;
            string BodyBuffer;
            // 
            ChrAllowed = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ,.<>;:[{]}\\|`~!@#$%^&*()-_=+/?\"\'";
            DecodeHTML_RemoveWhiteSpace = "";
            if (!IsNull(DirtyText)) {
                if ((DirtyText != "")) {
                    BodyBuffer = genericController.vbReplace(DirtyText, "\r\n", "\r");
                    BodyBuffer = genericController.vbReplace(BodyBuffer, "\n", "\r");
                    for (Pointer = 1; (Pointer <= BodyBuffer.Length); Pointer++) {
                        ChrTest = BodyBuffer.Substring((Pointer - 1), 1);
                        AscTest = Asc(ChrTest);
                        if (isInStr(1, ChrAllowed, ChrTest)) {
                            DecodeHTML_RemoveWhiteSpace = (DecodeHTML_RemoveWhiteSpace + ChrTest);
                            SpaceCounter = 0;
                        }
                        else if ((AscTest == Asc("\r"))) {
                            DecodeHTML_RemoveWhiteSpace = (DecodeHTML_RemoveWhiteSpace + "\r\n");
                            SpaceCounter = 0;
                        }
                        else if ((ChrTest == " ")) {
                            if ((SpaceCounter == 0)) {
                                SpaceCounter = 1;
                                DecodeHTML_RemoveWhiteSpace = (DecodeHTML_RemoveWhiteSpace + " ");
                            }
                            
                        }
                        else {
                            SpaceCounter = 0;
                        }
                        
                    }
                    
                }
                
            }
            
            return DecodeHTML_RemoveWhiteSpace.Trim();
            
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            Err.Clear();
        }
    }
}