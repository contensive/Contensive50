

using Controllers;

// 
//  findReplace as integer to as integer
//  just the document -- replace out 
//  if 'Imports Interop.adodb, replace in ObjectStateEnum.adState...
//  findreplace encode to encode
//  findreplace ''DoEvents to '''DoEvents
//  runProcess becomes runProcess
//  Sleep becomes Threading.Thread.Sleep(
//  as object to as object
// 

namespace Controllers {
    
    public class htmlParserController {
        
        // 
        // ============================================================================
        // 
        //  open source html parser to try
        //  ************* NUGET html parser
        // 
        // 
        // 
        // 
        //  Parse HTML
        // 
        //    This class parses an HTML document into Nodes. A node may be text, and it
        //    may be a tag. Use the IsTag method to detect
        // 
        //    It makes no attempt to create a document structer. The advantage is that
        //    it can parse through, and make available poor HTML structures
        // 
        //    If Element.IsTag
        //        Element.text = the tag, including <>
        //    otherwise
        //        Element.text = the string
        // ============================================================================
        // 
        //    Internal Storage
        // 
        private coreClass cpCore;
        
        // 
        private Element[] LocalElements;
        
        private int LocalElementSize;
        
        private int LocalElementCount;
        
        private string[] SplitStore;
        
        private int SplitStoreCnt;
        
        private string[] Blobs;
        
        private int BlobCnt;
        
        private string BlobSN;
        
        // 
        //    Internal HTML Element Attribute structure
        // 
        private struct ElementAttributeStructure {
            
            private string Name;
            
            private string UcaseName;
            
            private string Value;
        }
        
        // 
        //    Internal HTML Element (tag) structure
        // 
        private struct Element {
            
            private bool IsTag;
            
            private string TagName;
            
            private string Text;
            
            private int Position;
            
            private int AttributeCount;
            
            private int AttributeSize;
            
            private ElementAttributeStructure[] Attributes;
            
            private bool Loaded;
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' constructor
        // '' </summary>
        // '' <param name="cp"></param>
        // '' <remarks></remarks>
        public htmlParserController(coreClass cpCore) {
            this.cpCore = cpCore;
        }
        
        // 
        // ========================================================================
        //    Parses the string
        //    returns true if loaded OK
        // ========================================================================
        // 
        public bool Load(string HTMLSource) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            const object Chunk = 1000;
            // 
            string WorkingSrc;
            int TagStart;
            int TagEnd;
            int TagLength;
            string TagStartString;
            string[] splittest;
            int Ptr;
            int Cnt;
            int Pos;
            bool testing;
            int PosScriptEnd;
            int PosEndScript;
            int PosEndScriptEnd;
            // 
            //  ----- initialize internal storage
            // 
            WorkingSrc = HTMLSource;
            LocalElementCount = 0;
            LocalElementSize = 0;
            object LocalElements;
            if (NewWay) {
                // 
                // --------------------------------------------------------------------------------
                //  New Way
                // --------------------------------------------------------------------------------
                // 
                Load = true;
                Ptr = 0;
                // 
                //  get a unique signature
                // 
                for (
                ; (((WorkingSrc.IndexOf(BlobSN, 0, System.StringComparison.OrdinalIgnoreCase) + 1) 
                            != 0) 
                            && (Ptr < 10)); 
                ) {
                    BlobSN = ("/blob" 
                                + (genericController.GetRandomInteger().ToString() + ":"));
                    Ptr = (Ptr + 1);
                }
                
                splittest = WorkingSrc.Split("<script");
                Cnt = (UBound(splittest) + 1);
                if ((Cnt > 1)) {
                    for (Ptr = 1; (Ptr 
                                <= (Cnt - 1)); Ptr++) {
                        PosScriptEnd = genericController.vbInstr(1, splittest[Ptr], ">");
                        if ((PosScriptEnd > 0)) {
                            PosEndScript = genericController.vbInstr(PosScriptEnd, splittest[Ptr], "</script", vbTextCompare);
                            if ((PosEndScript > 0)) {
                                object Preserve;
                                Blobs(BlobCnt);
                                Blobs(BlobCnt) = splittest[Ptr].Substring(PosScriptEnd, (((PosEndScript - 1) 
                                                - (PosScriptEnd + 1)) 
                                                + 1));
                                splittest[Ptr] = (splittest[Ptr].Substring(0, PosScriptEnd) 
                                            + (BlobSN 
                                            + (BlobCnt + ("/" + splittest[Ptr].Substring((PosEndScript - 1))))));
                                BlobCnt = (BlobCnt + 1);
                            }
                            
                        }
                        
                    }
                    
                    WorkingSrc = Join(splittest, "<script");
                }
                
                // 
                //  remove all styles
                // 
                splittest = WorkingSrc.Split("<style");
                Cnt = (UBound(splittest) + 1);
                if ((Cnt > 1)) {
                    for (Ptr = 1; (Ptr 
                                <= (Cnt - 1)); Ptr++) {
                        PosScriptEnd = genericController.vbInstr(1, splittest[Ptr], ">");
                        if ((PosScriptEnd > 0)) {
                            PosEndScript = genericController.vbInstr(PosScriptEnd, splittest[Ptr], "</style", vbTextCompare);
                            if ((PosEndScript > 0)) {
                                object Preserve;
                                Blobs(BlobCnt);
                                Blobs(BlobCnt) = splittest[Ptr].Substring(PosScriptEnd, (((PosEndScript - 1) 
                                                - (PosScriptEnd + 1)) 
                                                + 1));
                                splittest[Ptr] = (splittest[Ptr].Substring(0, PosScriptEnd) 
                                            + (BlobSN 
                                            + (BlobCnt + ("/" + splittest[Ptr].Substring((PosEndScript - 1))))));
                                BlobCnt = (BlobCnt + 1);
                            }
                            
                        }
                        
                    }
                    
                    WorkingSrc = Join(splittest, "<style");
                }
                
                // 
                //  remove comments
                // 
                splittest = WorkingSrc.Split("<!--");
                Cnt = (UBound(splittest) + 1);
                if ((Cnt > 1)) {
                    for (Ptr = 1; (Ptr 
                                <= (Cnt - 1)); Ptr++) {
                        PosScriptEnd = genericController.vbInstr(1, splittest[Ptr], "-->");
                        if ((PosScriptEnd > 0)) {
                            object Preserve;
                            Blobs(BlobCnt);
                            Blobs(BlobCnt) = splittest[Ptr].Substring(0, (PosScriptEnd - 1));
                            splittest[Ptr] = (BlobSN 
                                        + (BlobCnt + ("/" + splittest[Ptr].Substring((PosScriptEnd - 1)))));
                            BlobCnt = (BlobCnt + 1);
                        }
                        
                    }
                    
                    WorkingSrc = Join(splittest, "<!--");
                }
                
                // 
                //  Split the html on <
                // 
                SplitStore = WorkingSrc.Split("<");
                SplitStoreCnt = (UBound(SplitStore) + 1);
                LocalElementCount = (SplitStoreCnt * 2);
                object LocalElements;
            }
            else {
                // 
                // --------------------------------------------------------------------------------
                //  Old way
                // --------------------------------------------------------------------------------
                // 
                Load = true;
                if (!IsNull(WorkingSrc)) {
                    TagEnd = 0;
                    TagStartString = "<";
                    TagStart = genericController.vbInstr(1, WorkingSrc, TagStartString);
                    while ((TagStart != 0)) {
                        if (((LocalElementCount / 1000) 
                                    == Int((LocalElementCount / 1000)))) {
                            LocalElementCount = LocalElementCount;
                        }
                        
                        TagStartString = "<";
                        if ((TagStart 
                                    > (TagEnd + 1))) {
                            if ((LocalElementCount >= LocalElementSize)) {
                                LocalElementSize = (LocalElementSize + Chunk);
                                object Preserve;
                                LocalElements[LocalElementSize];
                            }
                            
                            LocalElements[LocalElementCount].IsTag = false;
                            LocalElements[LocalElementCount].Text = WorkingSrc.Substring(TagEnd, (TagStart - (1 - TagEnd)));
                            LocalElements[LocalElementCount].Position = (TagEnd + 1);
                            LocalElements[LocalElementCount].Loaded = true;
                            LocalElementCount = (LocalElementCount + 1);
                        }
                        
                        // 
                        //  ----- create a tag element
                        // 
                        if ((LocalElementCount >= LocalElementSize)) {
                            LocalElementSize = (LocalElementSize + Chunk);
                            object Preserve;
                            LocalElements[LocalElementSize];
                        }
                        
                        LocalElements[LocalElementCount].Position = TagStart;
                        LocalElements[LocalElementCount].IsTag = true;
                        if ((WorkingSrc.Substring((TagStart - 1), 4) == "<!--")) {
                            // 
                            //  Comment Tag
                            // 
                            TagEnd = genericController.vbInstr(TagStart, WorkingSrc, "-->");
                            if ((TagEnd == 0)) {
                                LocalElements[LocalElementCount].Text = WorkingSrc.Substring((TagStart - 1));
                            }
                            else {
                                TagEnd = (TagEnd + 2);
                                LocalElements[LocalElementCount].Text = WorkingSrc.Substring((TagStart - 1), ((TagEnd - TagStart) 
                                                + 1));
                            }
                            
                            LocalElements[LocalElementCount].TagName = "!--";
                            LocalElements[LocalElementCount].Loaded = true;
                            TagStartString = "<";
                        }
                        else if ((genericController.vbLCase(WorkingSrc.Substring((TagStart - 1), 7)) == "<script")) {
                            // 
                            //  Script tag - include everything up to the </script> in the next non-tag
                            // 
                            TagEnd = genericController.vbInstr(TagStart, WorkingSrc, ">");
                            if ((TagEnd == 0)) {
                                LocalElements[LocalElementCount].Text = WorkingSrc.Substring((TagStart - 1));
                            }
                            else {
                                LocalElements[LocalElementCount].Text = WorkingSrc.Substring((TagStart - 1), ((TagEnd - TagStart) 
                                                + 1));
                            }
                            
                            this.ParseTag(LocalElementCount);
                            LocalElements[LocalElementCount].Loaded = true;
                            TagStartString = "</script";
                        }
                        else {
                            // 
                            //  All other tags
                            // 
                            TagEnd = genericController.vbInstr(TagStart, WorkingSrc, ">");
                            if ((TagEnd == 0)) {
                                LocalElements[LocalElementCount].Text = WorkingSrc.Substring((TagStart - 1));
                            }
                            else {
                                LocalElements[LocalElementCount].Text = WorkingSrc.Substring((TagStart - 1), ((TagEnd - TagStart) 
                                                + 1));
                            }
                            
                            this.ParseTag(LocalElementCount);
                            LocalElements[LocalElementCount].Loaded = true;
                            TagStartString = "<";
                        }
                        
                        LocalElementCount = (LocalElementCount + 1);
                        if ((TagEnd == 0)) {
                            TagStart = 0;
                        }
                        else {
                            TagStart = genericController.vbInstr(TagEnd, WorkingSrc, TagStartString, vbTextCompare);
                        }
                        
                        while (((TagStart != 0) 
                                    && (WorkingSrc.Substring(TagStart, 1) == " "))) {
                            TagStart = genericController.vbInstr((TagStart + 1), WorkingSrc, TagStartString, vbTextCompare);
                        }
                        
                    }
                    
                    // 
                    //  ----- if there is anything left in the WorkingSrc, make an element out of it
                    // 
                    if ((TagEnd < WorkingSrc.Length)) {
                        if ((LocalElementCount >= LocalElementSize)) {
                            LocalElementSize = (LocalElementSize + Chunk);
                            object Preserve;
                            LocalElements[LocalElementSize];
                        }
                        
                        LocalElements[LocalElementCount].IsTag = false;
                        LocalElements[LocalElementCount].Text = WorkingSrc.Substring(TagEnd);
                        LocalElementCount = (LocalElementCount + 1);
                    }
                    
                }
                
            }
            
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
        ErrorTrap:
            cpCore.handleException(new Exception("unexpected exception"));
        }
        
        // 
        // ========================================================================
        //    Get the element count
        // ========================================================================
        // 
        public int ElementCount {
            get {
                return LocalElementCount;
            }
        }
        
        public bool IsTag(int ElementPointer) {
            bool result = false;
            try {
                this.LoadElement(ElementPointer);
                if ((ElementPointer < LocalElementCount)) {
                    result = LocalElements(ElementPointer).IsTag;
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
            return result;
        }
        
        // 
        // ========================================================================
        //    Get the LocalElements value
        // ========================================================================
        // 
        public string Text(int ElementPointer) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            Text = "";
            this.LoadElement(ElementPointer);
            if ((ElementPointer < LocalElementCount)) {
                Text = LocalElements(ElementPointer).Text;
            }
            
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
        ErrorTrap:
            cpCore.handleException(new Exception("unexpected exception"));
        }
        
        // 
        // ========================================================================
        //    Get the LocalElements value
        // ========================================================================
        // 
        public string TagName(int ElementPointer) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            TagName = "";
            this.LoadElement(ElementPointer);
            if ((ElementPointer < LocalElementCount)) {
                TagName = LocalElements(ElementPointer).TagName;
            }
            
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
        ErrorTrap:
            cpCore.handleException(new Exception("unexpected exception"));
        }
        
        // 
        // ========================================================================
        //    Get the LocalElements value
        // ========================================================================
        // 
        public int Position(int ElementPointer) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            Position = 0;
            this.LoadElement(ElementPointer);
            if ((ElementPointer < LocalElementCount)) {
                Position = LocalElements(ElementPointer).Position;
            }
            
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
        ErrorTrap:
            cpCore.handleException(new Exception("unexpected exception"));
        }
        
        // 
        // ========================================================================
        //    Get an LocalElements attribute count
        // ========================================================================
        // 
        public int ElementAttributeCount(int ElementPointer) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            ElementAttributeCount = 0;
            this.LoadElement(ElementPointer);
            if ((ElementPointer < LocalElementCount)) {
                ElementAttributeCount = LocalElements(ElementPointer).AttributeCount;
            }
            
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
        ErrorTrap:
            cpCore.handleException(new Exception("unexpected exception"));
        }
        
        // 
        // ========================================================================
        //    Get an LocalElements attribute name
        // ========================================================================
        // 
        public string ElementAttributeName(int ElementPointer, int AttributePointer) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            ElementAttributeName = "";
            this.LoadElement(ElementPointer);
            if ((ElementPointer < LocalElementCount)) {
                if ((AttributePointer < LocalElements(ElementPointer).AttributeCount)) {
                    ElementAttributeName = LocalElements(ElementPointer).Attributes[AttributePointer].Name;
                }
                
            }
            
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
        ErrorTrap:
            cpCore.handleException(new Exception("unexpected exception"));
        }
        
        // 
        // ========================================================================
        //    Get an LocalElements attribute value
        // ========================================================================
        // 
        public string ElementAttributeValue(int ElementPointer, int AttributePointer) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            ElementAttributeValue = "";
            this.LoadElement(ElementPointer);
            if ((ElementPointer < LocalElementCount)) {
                if ((AttributePointer < LocalElements(ElementPointer).AttributeCount)) {
                    ElementAttributeValue = LocalElements(ElementPointer).Attributes[AttributePointer].Value;
                }
                
            }
            
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
        ErrorTrap:
            cpCore.handleException(new Exception("unexpected exception"));
        }
        
        // 
        // ========================================================================
        //    Get an LocalElements attribute value
        // ========================================================================
        // 
        public string ElementAttribute(int ElementPointer, string Name) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            int AttributePointer;
            string UcaseName;
            // 
            ElementAttribute = "";
            this.LoadElement(ElementPointer);
            if ((ElementPointer < LocalElementCount)) {
                // With...
                if ((LocalElements(ElementPointer).AttributeCount > 0)) {
                    UcaseName = genericController.vbUCase(Name);
                    for (AttributePointer = 0; (AttributePointer 
                                <= (LocalElements(ElementPointer).AttributeCount - 1)); AttributePointer++) {
                        if (LocalElements(ElementPointer).Attributes) {
                            AttributePointer.UcaseName = UcaseName;
                            ElementAttribute = LocalElements(ElementPointer).Attributes;
                            AttributePointer.Value;
                            break;
                        }
                        
                    }
                    
                }
                
            }
            
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
        ErrorTrap:
            cpCore.handleException(new Exception("unexpected exception"));
        }
        
        // 
        // ========================================================================
        //    Parse a Tag element into its attributes
        // ========================================================================
        // 
        private void ParseTag(int ElementPointer) {
            // Exit Sub
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            string Copy;
            int CursorPosition;
            int SpacePosition;
            // Dim ClosePosition as integer
            int AttributeDelimiterPosition;
            string TagString;
            // 
            int QuotePosition;
            int CloseQuotePosition;
            int EqualPosition;
            int TestPosition;
            string TestValue;
            string Name;
            // 
            string[] AttrSplit;
            int AttrCount;
            int AttrPointer;
            string AttrName;
            string AttrValue;
            int AttrValueLen;
            // 
            // With...
            TagString = LocalElements(ElementPointer).Text.Substring(1, (LocalElements(ElementPointer).Text.Length - 2));
            if ((TagString.Substring((TagString.Length - 1)) == "/")) {
                TagString = TagString.Substring(0, (TagString.Length - 1));
            }
            
            // TagString = genericController.vbReplace(TagString, ">", " ") & " "
            TagString = genericController.vbReplace(TagString, "\r", " ");
            TagString = genericController.vbReplace(TagString, "\n", " ");
            TagString = genericController.vbReplace(TagString, "  ", " ");
            // TagString = genericController.vbReplace(TagString, " =", "=")
            // TagString = genericController.vbReplace(TagString, "= ", "=")
            // TagString = genericController.vbReplace(TagString, "'", """")
            0.AttributeSize = 1;
            LocalElements(ElementPointer).AttributeCount = 1;
            object .;
            Attributes[0];
            //  allocates the first
            // ClosePosition = Len(TagString)
            // If ClosePosition <= 2 Then
            //     '
            //     ' ----- there is nothing in the <>, skip element
            //     '
            // Else
            // 
            //  ----- Get the tag name
            // 
            if ((TagString != "")) {
                AttrSplit = SplitDelimited(TagString, " ");
                AttrCount = (UBound(AttrSplit) + 1);
                if ((AttrCount > 0)) {
                    LocalElements(ElementPointer).TagName = AttrSplit[0];
                    if ((LocalElements(ElementPointer).TagName == "!--")) {
                        // 
                        //  Skip comment tags, ignore the attributes
                        // 
                    }
                    else {
                        // 
                        //  Process the tag
                        // 
                        if ((AttrCount > 1)) {
                            for (AttrPointer = 1; (AttrPointer 
                                        <= (AttrCount - 1)); AttrPointer++) {
                                AttrName = AttrSplit[AttrPointer];
                                if ((AttrName != "")) {
                                    if ((LocalElements(ElementPointer).AttributeCount >= LocalElements(ElementPointer).AttributeSize)) {
                                        LocalElements(ElementPointer).AttributeSize = (LocalElements(ElementPointer).AttributeSize + 5);
                                        object Preserve.Attributes;
                                    }
                                    
                                    EqualPosition = genericController.vbInstr(1, AttrName, "=");
                                    if ((EqualPosition == 0)) {
                                        LocalElements(ElementPointer).Attributes;
                                        genericController.vbUCase(AttrName).Attributes[LocalElements(ElementPointer).AttributeCount].Value = AttrName;
                                        AttrName.Attributes[LocalElements(ElementPointer).AttributeCount].UcaseName = AttrName;
                                        LocalElements(ElementPointer).AttributeCount.Name = AttrName;
                                    }
                                    else {
                                        AttrValue = AttrName.Substring(EqualPosition);
                                        AttrValueLen = AttrValue.Length;
                                        if ((AttrValueLen > 1)) {
                                            if (((AttrValue.Substring(0, 1) == "\"") 
                                                        && (AttrValue.Substring((AttrValueLen - 1), 1) == "\""))) {
                                                AttrValue = AttrValue.Substring(1, (AttrValueLen - 2));
                                            }
                                            
                                        }
                                        
                                        genericController.vbUCase(AttrName).Attributes[LocalElements(ElementPointer).AttributeCount].Value = AttrValue;
                                        AttrName.Attributes[LocalElements(ElementPointer).AttributeCount].UcaseName = AttrValue;
                                        AttrName.Substring(0, (EqualPosition - 1)).Attributes[LocalElements(ElementPointer).AttributeCount].Name = AttrValue;
                                        AttrName = AttrValue;
                                    }
                                    
                                    LocalElements(ElementPointer).AttributeCount = (LocalElements(ElementPointer).AttributeCount + 1);
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                }
                
            }
            
            // 
            //             CursorPosition = 1
            //             SpacePosition = GetLesserNonZero(InStr(CursorPosition, TagString, " "), ClosePosition)
            //             .TagName = Mid(TagString, 2, SpacePosition - 2)
            //             CursorPosition = SpacePosition + 1
            //             Do While (CursorPosition < ClosePosition) And (CursorPosition <> 0)
            //                 SpacePosition = GetLesserNonZero(InStr(CursorPosition, TagString, " "), ClosePosition + 1)
            //                 QuotePosition = GetLesserNonZero(InStr(CursorPosition, TagString, """"), ClosePosition + 1)
            //                 EqualPosition = GetLesserNonZero(InStr(CursorPosition, TagString, "="), ClosePosition + 1)
            //                 '
            //                 If .AttributeCount >= .AttributeSize Then
            //                     .AttributeSize = .AttributeSize + 1
            //                     ReDim Preserve .Attributes(.AttributeSize)
            //                     End If
            //                 If SpacePosition < EqualPosition Then
            //                     '
            //                     ' ----- Case 1, attribute without a value
            //                     '
            //                     Name = Mid(TagString, CursorPosition, SpacePosition - CursorPosition)
            //                     .Attributes(.AttributeCount).Name = Name
            //                     .Attributes(.AttributeCount).UcaseName = genericController.vbUCase(Name)
            //                     .Attributes(.AttributeCount).Value = Name
            //                     CursorPosition = SpacePosition
            //                 ElseIf QuotePosition < SpacePosition Then
            //                     '
            //                     ' ----- Case 2, quoted value
            //                     '
            //                     CloseQuotePosition = GetLesserNonZero(InStr(QuotePosition + 1, TagString, """"), ClosePosition)
            //                     Name = Mid(TagString, CursorPosition, EqualPosition - CursorPosition)
            //                     .Attributes(.AttributeCount).Name = Name
            //                     .Attributes(.AttributeCount).UcaseName = genericController.vbUCase(Name)
            //                     .Attributes(.AttributeCount).Value = Mid(TagString, QuotePosition + 1, CloseQuotePosition - QuotePosition - 1)
            //                     CursorPosition = CloseQuotePosition
            //                 Else
            //                     '
            //                     ' ----- Case 2, unquoted value
            //                     '
            //                     Name = Mid(TagString, CursorPosition, EqualPosition - CursorPosition)
            //                     .Attributes(.AttributeCount).Name = Name
            //                     .Attributes(.AttributeCount).UcaseName = genericController.vbUCase(Name)
            //                     .Attributes(.AttributeCount).Value = Mid(TagString, EqualPosition + 1, SpacePosition - EqualPosition - 1)
            //                     CursorPosition = SpacePosition
            //                     End If
            //                 If CursorPosition <> 0 Then
            //                     CursorPosition = PassWhiteSpace(CursorPosition + 1, TagString)
            //                     End If
            //                 .AttributeCount = .AttributeCount + 1
            //                 Loop
            // End If
            // 
            return;
        ErrorTrap:
            cpCore.handleException(new Exception("unexpected exception"));
        }
        
        // '
        // '   CursorPosition points to the first character of an attribute name
        // '   ElementValue has no spaces before and after '=', and no double spaces anywhere
        // '   ElementValue whiteSpace has been converted to " "
        // '
        // Private Function GetAttributeDelimiterPosition(CursorPosition as integer, ElementValue As String) as integer
        //     '
        //     Dim SpacePosition as integer
        //     Dim QuotePosition as integer
        //     Dim EndPosition as integer
        //     Dim EqualPosition as integer
        //     Dim TestPosition as integer
        //     Dim TestValue As String
        //     '
        //     CursorPosition = PassWhiteSpace(CursorPosition, ElementValue)
        // 
        //     GetAttributeDelimiterPosition = 0
        //     EndPosition = Len(ElementValue)
        //     SpacePosition = GetLesserNonZero(InStr(CursorPosition, ElementValue, " "), EndPosition + 1)
        //     QuotePosition = GetLesserNonZero(InStr(CursorPosition, ElementValue, """"), EndPosition + 1)
        //     EqualPosition = GetLesserNonZero(InStr(CursorPosition, ElementValue, "="), EndPosition + 1)
        //     '
        //     If SpacePosition < EqualPosition Then
        //         '
        //         ' ----- Case 1, attribute without a value
        //         '
        // 
        //         End If
        // End Function
        // 
        // 
        // 
        private int GetLesserNonZero(int value0, int value1) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            if ((value0 == 0)) {
                GetLesserNonZero = value1;
            }
            else if ((value1 == 0)) {
                GetLesserNonZero = value0;
            }
            else if ((value0 < value1)) {
                GetLesserNonZero = value0;
            }
            else {
                GetLesserNonZero = value1;
            }
            
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
        ErrorTrap:
            cpCore.handleException(new Exception("unexpected exception"));
        }
        
        // 
        //  Pass spaces at the current cursor position
        // 
        private int PassWhiteSpace(int CursorPosition, string TagString) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            PassWhiteSpace = CursorPosition;
            while (((TagString.Substring((PassWhiteSpace - 1), 1) == " ") 
                        && (PassWhiteSpace < TagString.Length))) {
                PassWhiteSpace = (PassWhiteSpace + 1);
            }
            
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
        ErrorTrap:
            cpCore.handleException(new Exception("unexpected exception"));
        }
        
        //         '
        //         ' Create the full URI from a possible relative URI
        //         '   URIBase is the URI of the page that contains this URI
        //         '       blank if the URI is not from a link
        //         '       it can also be from the base tag
        //         '
        //         Private Function GetAbsoluteURL(ByVal URIWorking As String, ByVal URIBase As String)
        //             On Error GoTo ErrorTrap
        //             '
        //             Dim RightSide As String
        //             Dim LeftSide As String
        //             Dim QueryString As String
        //             Dim Position As Integer
        //             Dim BaseProtocol As String
        //             Dim BaseHost As String
        //             Dim BasePath As String
        //             Dim BasePage As String
        //             Dim BaseQueryString As String
        //             If (Left(UCase(URIWorking), 5) <> "HTTP:") Then
        //                 '
        //                 ' path is relative, construct from base
        //                 '
        //                 If URIBase = "" Then
        //                     '
        //                     ' URI base is not given, use the working URI instead
        //                     '
        //                     URIBase = URIWorking
        //                 End If
        //                 '
        //                 ' make sure base does not have anchors or querystrings
        //                 '
        //                 Position = genericController.vbInstr(1, URIBase, "#")
        //                 If Position <> 0 Then
        //                     URIBase = Mid(URIBase, 1, Position - 1)
        //                 End If
        //                 Position = genericController.vbInstr(1, URIBase, "?")
        //                 If Position <> 0 Then
        //                     URIBase = Mid(URIBase, 1, Position - 1)
        //                 End If
        //                 '
        //                 ' save base host, path and page
        //                 '
        //                 If Mid(URIWorking, 1, 1) = "#" Then
        //                     URIWorking = URIWorking
        //                 End If
        //                 Call SeparateURL(URIBase, BaseProtocol, BaseHost, BasePath, BasePage, BaseQueryString)
        //                 '
        //                 ' if URI is only an anchor or a querysting, use base plus URI
        //                 '
        //                 If Mid(URIWorking, 1, 1) = "?" Then
        //                     URIWorking = BasePath & BasePage & URIWorking
        //                 End If
        //                 If Mid(URIWorking, 1, 1) = "#" Then
        //                     URIWorking = BasePath & BasePage & URIWorking
        //                 End If
        //                 '
        //                 ' if path does not go to root directory, stick on base path
        //                 '
        //                 If Mid(URIWorking, 1, 1) <> "/" Then
        //                     URIWorking = BasePath & URIWorking
        //                 End If
        //                 Position = genericController.vbInstr(1, URIWorking, "../")
        //                 Do Until Position = 0
        //                     '
        //                     ' if path contains directory changes, do the move
        //                     '
        //                     RightSide = Mid(URIWorking, Position + 3)
        //                     LeftSide = Mid(URIWorking, 1, Position - 1)
        //                     If Len(LeftSide) > 1 Then LeftSide = Mid(LeftSide, 1, Len(LeftSide) - 1)
        //                     Do While Len(LeftSide) > 1 And Mid(LeftSide, Len(LeftSide), 1) <> "/"
        //                         LeftSide = Mid(LeftSide, 1, Len(LeftSide) - 1)
        //                         'DoEvents()
        //                     Loop
        //                     URIWorking = LeftSide + RightSide
        //                     Position = genericController.vbInstr(1, URIWorking, "../")
        //                     'DoEvents()
        //                 Loop
        //                 Position = genericController.vbInstr(1, URIWorking, "./")
        //                 Do Until Position = 0
        //                     '
        //                     ' if path contains directory marks, remove them
        //                     '
        //                     RightSide = Mid(URIWorking, Position + 2)
        //                     LeftSide = Mid(URIWorking, 1, Position - 1)
        //                     Do While Len(LeftSide) > 1 And Mid(LeftSide, Len(LeftSide), 1) <> "/"
        //                         LeftSide = Mid(LeftSide, 1, Len(LeftSide) - 1)
        //                         'DoEvents()
        //                     Loop
        //                     URIWorking = LeftSide + RightSide
        //                     Position = genericController.vbInstr(1, URIWorking, "./")
        //                     'DoEvents()
        //                 Loop
        //                 '
        //                 ' add the protocol and host
        //                 '
        //                 URIWorking = "http://" & BaseHost & URIWorking
        //             End If
        //             GetAbsoluteURL = URIWorking
        //             Exit Function
        //             '
        //             ' ----- Error Trap
        //             '
        // ErrorTrap:
        //             cpCore.handleException(New Exception("unexpected exception"))
        //         End Function
        //         '
        //         '========================================================================
        //         '   Get all the text and tags between this tag and its close
        //         '
        //         '   If it does not close correctly, return "<ERROR0>"
        //         '========================================================================
        //         '
        //         Public Function TagInnerText(ByVal ElementPointer As Integer) As String
        //             On Error GoTo ErrorTrap
        //             '
        //             Dim iElementPointer As Integer
        //             Dim iElementStart As Integer
        //             Dim iElementCount As Integer
        //             Dim TagName As String
        //             Dim TagNameEnd As String
        //             Dim TagCount As Integer
        //             '
        //             Call LoadElement(ElementPointer)
        //             If ElementPointer >= 0 Then
        //                 If LocalElements(ElementPointer).IsTag Then
        //                     iElementPointer = ElementPointer + 1
        //                     TagName = genericController.vbUCase(LocalElements(ElementPointer).TagName)
        //                     TagNameEnd = "/" & TagName
        //                     TagCount = 1
        //                     Do While TagCount <> 0 And iElementPointer < LocalElementCount
        //                         Call LoadElement(iElementPointer)
        //                         With LocalElements(iElementPointer)
        //                             If Not .IsTag Then
        //                                 TagInnerText = TagInnerText & .Text
        //                             Else
        //                                 Select Case genericController.vbUCase(.TagName)
        //                                     Case TagName
        //                                         TagCount = TagCount + 1
        //                                         TagInnerText = TagInnerText & .Text
        //                                     Case TagNameEnd
        //                                         TagCount = TagCount - 1
        //                                         If TagCount <> 0 Then
        //                                             TagInnerText = TagInnerText & .Text
        //                                         End If
        //                                     Case Else
        //                                         TagInnerText = TagInnerText & .Text
        //                                 End Select
        //                             End If
        //                         End With
        //                         iElementPointer = iElementPointer + 1
        //                     Loop
        //                     If iElementPointer >= LocalElementCount Then
        //                         TagInnerText = "<ERROR0>"
        //                     End If
        //                 End If
        //             End If
        //             '
        //             Exit Function
        // ErrorTrap:
        //             cpCore.handleExceptionAndContinue(New Exception("unexpected exception"))
        //         End Function
        // 
        // 
        // 
        private void LoadElement(int ElementPtr) {
            int SplitPtr;
            string SplitSrc;
            int ElementBasePtr;
            int Ptr;
            string SrcTag;
            string SrcBody;
            // 
            if (NewWay) {
                if (!LocalElements(ElementPtr).Loaded) {
                    SplitPtr = int.Parse((ElementPtr / 2));
                    ElementBasePtr = (SplitPtr * 2);
                    SplitSrc = SplitStore(SplitPtr);
                    Ptr = genericController.vbInstr(1, SplitSrc, ">");
                    // 
                    //  replace blobs
                    // 
                    if ((Ptr == 0)) {
                        SrcTag = "";
                        SrcBody = this.ReplaceBlob(SplitSrc);
                    }
                    else {
                        SrcTag = this.ReplaceBlob(SplitSrc.Substring(0, Ptr));
                        SrcBody = this.ReplaceBlob(SplitSrc.Substring(Ptr));
                    }
                    
                    if ((Ptr == 0)) {
                        if ((ElementPtr == 0)) {
                            // 
                            //  no close tag, elementptr=0 then First entry is empty, second is body
                            // 
                            // With...
                            true.Position = "";
                            false.Loaded = "";
                            0.IsTag = "";
                            LocalElements(ElementBasePtr).AttributeCount = "";
                            // 
                            // With...
                            true.Position = SplitSrc;
                            false.Loaded = SplitSrc;
                            0.IsTag = SplitSrc;
                            LocalElements((ElementBasePtr + 1)).AttributeCount = SplitSrc;
                        }
                        else {
                            // 
                            //  no close tag, elementptr>0 then First entry is '<', second is body
                            // 
                            // With...
                            true.Position = "<";
                            false.Loaded = "<";
                            0.IsTag = "<";
                            LocalElements(ElementBasePtr).AttributeCount = "<";
                            // 
                            // With...
                            true.Position = SplitSrc;
                            false.Loaded = SplitSrc;
                            0.IsTag = SplitSrc;
                            LocalElements((ElementBasePtr + 1)).AttributeCount = SplitSrc;
                        }
                        
                    }
                    else {
                        // 
                        //  close tag found, first entry is tag text, second entry is body
                        // 
                        // With...
                        // TODO: Warning!!!! NULL EXPRESSION DETECTED...
                        ;
                        LocalElements(ElementBasePtr).Text = true;
                        // 
                        // With...
                        true.Position = SrcBody;
                        false.Loaded = SrcBody;
                        0.IsTag = SrcBody;
                        LocalElements((ElementBasePtr + 1)).AttributeCount = SrcBody;
                    }
                    
                }
                
            }
            
        }
        
        // 
        // 
        // 
        private string ReplaceBlob(string Src) {
            int Pos;
            int PosEnd;
            int PosNum;
            string PtrText;
            int Ptr;
            string Blob = "";
            ReplaceBlob = Src;
            Pos = genericController.vbInstr(1, Src, BlobSN);
            if ((Pos != 0)) {
                PosEnd = genericController.vbInstr((Pos + 1), Src, "/");
                if ((PosEnd > 0)) {
                    PosNum = genericController.vbInstr((Pos + 1), Src, ":");
                    if ((PosNum > 0)) {
                        PtrText = Src.Substring(PosNum, (PosEnd 
                                        - (PosNum - 1)));
                        if (genericController.vbIsNumeric(PtrText)) {
                            Ptr = int.Parse(PtrText);
                            if ((Ptr < BlobCnt)) {
                                Blob = Blobs(Ptr);
                            }
                            
                            ReplaceBlob = (Src.Substring(0, (Pos - 1)) 
                                        + (Blob + Src.Substring(PosEnd)));
                        }
                        
                    }
                    
                }
                
            }
            
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' handle legacy class errors
        // '' </summary>
        // '' <param name="MethodName"></param>
        // '' <param name="ErrNumber"></param>
        // '' <param name="ErrSource"></param>
        // '' <param name="ErrDescription"></param>
        // '' <remarks></remarks>
        private void handleLegacyClassError(string MethodName, int ErrNumber, string ErrSource, string ErrDescription) {
            // 
            cpCore.handleException(new Exception(("unexpected exception in method [" 
                                + (MethodName + ("], ErrDescription [" 
                                + (ErrDescription + "]"))))));
            // 
        }
    }
}