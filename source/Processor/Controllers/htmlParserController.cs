
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
namespace Contensive.Processor.Controllers {
    public class HtmlParserController {
        //
        //====================================================================================================
        // open source html parser to try
        // ************* NUGET html parser
        //
        //
        //
        //
        // Parse HTML
        //
        //   This class parses an HTML document into Nodes. A node may be text, and it
        //   may be a tag. Use the IsTag method to detect
        //
        //   It makes no attempt to create a document structer. The advantage is that
        //   it can parse through, and make available poor HTML structures
        //
        //   If Element.IsTag
        //       Element.text = the tag, including <>
        //   otherwise
        //       Element.text = the string
        //============================================================================
        //
        //   Internal Storage
        //
        private CoreController core;
        //
        private const bool NewWay = true;
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
        //   Internal HTML Element Attribute structure
        //
        private struct ElementAttributeStructure {
            public string Name;
            public string UcaseName;
            public string Value;
        }
        //
        //   Internal HTML Element (tag) structure
        //
        private struct Element {
            public bool IsTag;
            public string TagName;
            public string Text;
            public int Position;
            public int AttributeCount;
            public int AttributeSize;
            public ElementAttributeStructure[] Attributes;
            public bool Loaded;
        }
        //
        //====================================================================================================
        //
        public HtmlParserController(CoreController core) {
            this.core = core;
        }
        //
        //====================================================================================================
        //   Parses the string, returns true if loaded OK
        //
        public bool Load(string HTMLSource) {
            bool tempLoad = false;
            try {
                //
                string WorkingSrc = null;
                string[] splittest = null;
                int Ptr = 0;
                int Cnt = 0;
                int PosScriptEnd = 0;
                int PosEndScript = 0;
                //
                // ----- initialize internal storage
                //
                WorkingSrc = HTMLSource;
                LocalElementCount = 0;
                LocalElementSize = 0;
                LocalElements = new Contensive.Processor.Controllers.HtmlParserController.Element[LocalElementSize + 1];
                tempLoad = true;
                Ptr = 0;
                //
                // get a unique signature
                //
                do {
                    BlobSN = "/blob" + encodeText(GenericController.GetRandomInteger(core)) + ":";
                    Ptr = Ptr + 1;
                } while ((WorkingSrc.IndexOf(BlobSN, System.StringComparison.OrdinalIgnoreCase) != -1) && (Ptr < 10));
                //
                // remove all scripting
                //
                splittest = WorkingSrc.Split(new[] { "<script" }, StringSplitOptions.None);
                //  Regex.Split( WorkingSrc, "<script");
                Cnt = splittest.GetUpperBound(0) + 1;
                if (Cnt > 1) {
                    for (Ptr = 1; Ptr < Cnt; Ptr++) {
                        PosScriptEnd = GenericController.vbInstr(1, splittest[Ptr], ">");
                        if (PosScriptEnd > 0) {
                            PosEndScript = GenericController.vbInstr(PosScriptEnd, splittest[Ptr], "</script");
                            if (PosEndScript > 0) {
                                Array.Resize(ref Blobs, BlobCnt + 1);
                                Blobs[BlobCnt] = splittest[Ptr].Substring(PosScriptEnd, (PosEndScript - 1) - (PosScriptEnd + 1) + 1);
                                splittest[Ptr] = splittest[Ptr].Left(PosScriptEnd) + BlobSN + BlobCnt + "/" + splittest[Ptr].Substring(PosEndScript - 1);
                                BlobCnt = BlobCnt + 1;
                            }
                        }
                    }
                    WorkingSrc = string.Join("<script", splittest);
                }
                //
                // remove all styles
                //
                splittest = GenericController.stringSplit(WorkingSrc, "<style");
                Cnt = splittest.GetUpperBound(0) + 1;
                if (Cnt > 1) {
                    for (Ptr = 1; Ptr < Cnt; Ptr++) {
                        PosScriptEnd = GenericController.vbInstr(1, splittest[Ptr], ">");
                        if (PosScriptEnd > 0) {
                            PosEndScript = GenericController.vbInstr(PosScriptEnd, splittest[Ptr], "</style", 1);
                            if (PosEndScript > 0) {
                                Array.Resize(ref Blobs, BlobCnt + 1);
                                Blobs[BlobCnt] = splittest[Ptr].Substring(PosScriptEnd, (PosEndScript - 1) - (PosScriptEnd + 1) + 1);
                                splittest[Ptr] = splittest[Ptr].Left(PosScriptEnd) + BlobSN + BlobCnt + "/" + splittest[Ptr].Substring(PosEndScript - 1);
                                BlobCnt = BlobCnt + 1;
                            }
                        }
                    }
                    WorkingSrc = string.Join("<style", splittest);
                }
                //
                // remove comments
                //
                splittest = GenericController.stringSplit(WorkingSrc, "<!--");
                Cnt = splittest.GetUpperBound(0) + 1;
                if (Cnt > 1) {
                    for (Ptr = 1; Ptr < Cnt; Ptr++) {
                        PosScriptEnd = GenericController.vbInstr(1, splittest[Ptr], "-->");
                        if (PosScriptEnd > 0) {
                            Array.Resize(ref Blobs, BlobCnt + 1);
                            Blobs[BlobCnt] = splittest[Ptr].Left(PosScriptEnd - 1);
                            splittest[Ptr] = BlobSN + BlobCnt + "/" + splittest[Ptr].Substring(PosScriptEnd - 1);
                            BlobCnt = BlobCnt + 1;
                        }
                    }
                    WorkingSrc = string.Join("<!--", splittest);
                }
                //
                // Split the html on <
                //
                SplitStore = WorkingSrc.Split('<');
                SplitStoreCnt = SplitStore.GetUpperBound(0) + 1;
                LocalElementCount = (SplitStoreCnt * 2);
                LocalElements = new Contensive.Processor.Controllers.HtmlParserController.Element[LocalElementCount + 1];
                return tempLoad;
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
            }
            //ErrorTrap:
            LogController.handleError( core,new Exception("unexpected exception"));
            return tempLoad;
        }
        //
        //====================================================================================================
        //   Get the element count
        //
        public int ElementCount {
            get {
                return LocalElementCount;
            }
        }
        //
        //====================================================================================================
        //   is the specified element a tag (or text)
        //
        public bool IsTag(int ElementPointer) {
            bool result = false;
            try {
                LoadElement(ElementPointer);
                if (ElementPointer < LocalElementCount) {
                    result = LocalElements[ElementPointer].IsTag;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //   Get the LocalElements value
        //
        public string Text(int ElementPointer) {
            string tempText = null;
            try {
                //
                tempText = "";
                LoadElement(ElementPointer);
                if (ElementPointer < LocalElementCount) {
                    tempText = LocalElements[ElementPointer].Text;
                }
                //
                return tempText;
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
            }
            //ErrorTrap:
            LogController.handleError( core,new Exception("unexpected exception"));
            return tempText;
        }
        //
        //====================================================================================================
        //   Get the LocalElements value
        //
        public string TagName(int ElementPointer) {
            string tempTagName = null;
            try {
                //
                tempTagName = "";
                LoadElement(ElementPointer);
                if (ElementPointer < LocalElementCount) {
                    tempTagName = LocalElements[ElementPointer].TagName;
                }
                //
                return tempTagName;
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
            }
            //ErrorTrap:
            LogController.handleError( core,new Exception("unexpected exception"));
            return tempTagName;
        }
        //
        //====================================================================================================
        //   Get the LocalElements value
        //
        public int Position(int ElementPointer) {
            int tempPosition = 0;
            try {
                //
                tempPosition = 0;
                LoadElement(ElementPointer);
                if (ElementPointer < LocalElementCount) {
                    tempPosition = LocalElements[ElementPointer].Position;
                }
                //
                return tempPosition;
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
            }
            //ErrorTrap:
            LogController.handleError( core,new Exception("unexpected exception"));
            return tempPosition;
        }
        //
        //====================================================================================================
        //   Get an LocalElements attribute count
        //
        public int ElementAttributeCount(int ElementPointer) {
            int tempElementAttributeCount = 0;
            try {
                //
                tempElementAttributeCount = 0;
                LoadElement(ElementPointer);
                if (ElementPointer < LocalElementCount) {
                    tempElementAttributeCount = LocalElements[ElementPointer].AttributeCount;
                }
                //
                return tempElementAttributeCount;
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
            }
            //ErrorTrap:
            LogController.handleError( core,new Exception("unexpected exception"));
            return tempElementAttributeCount;
        }
        //
        //====================================================================================================
        //   Get an LocalElements attribute name
        //
        public string ElementAttributeName(int ElementPointer, int AttributePointer) {
            string tempElementAttributeName = null;
            try {
                //
                tempElementAttributeName = "";
                LoadElement(ElementPointer);
                if (ElementPointer < LocalElementCount) {
                    if (AttributePointer < LocalElements[ElementPointer].AttributeCount) {
                        tempElementAttributeName = LocalElements[ElementPointer].Attributes[AttributePointer].Name;
                    }
                }
                //
                return tempElementAttributeName;
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
            }
            //ErrorTrap:
            LogController.handleError( core,new Exception("unexpected exception"));
            return tempElementAttributeName;
        }
        //
        //====================================================================================================
        //   Get an LocalElements attribute value
        //
        public string ElementAttributeValue(int ElementPointer, int AttributePointer) {
            string tempElementAttributeValue = null;
            try {
                //
                tempElementAttributeValue = "";
                LoadElement(ElementPointer);
                if (ElementPointer < LocalElementCount) {
                    if (AttributePointer < LocalElements[ElementPointer].AttributeCount) {
                        tempElementAttributeValue = LocalElements[ElementPointer].Attributes[AttributePointer].Value;
                    }
                }
                //
                return tempElementAttributeValue;
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
            }
            //ErrorTrap:
            LogController.handleError( core,new Exception("unexpected exception"));
            return tempElementAttributeValue;
        }
        //
        //====================================================================================================
        //   Get an LocalElements attribute value
        //
        public string ElementAttribute(int ElementPointer, string Name) {
            string tempElementAttribute = null;
            try {
                //
                int AttributePointer = 0;
                string UcaseName = null;
                //
                tempElementAttribute = "";
                LoadElement(ElementPointer);
                if (ElementPointer < LocalElementCount) {
                    if (LocalElements[ElementPointer].AttributeCount > 0) {
                        UcaseName = GenericController.vbUCase(Name);
                        //todo  NOTE: The ending condition of VB 'For' loops is tested only on entry to the loop. Instant C# has created a temporary variable in order to use the initial value of LocalElements(ElementPointer).AttributeCount for every iteration:
                        int tempVar = LocalElements[ElementPointer].AttributeCount;
                        for (AttributePointer = 0; AttributePointer < tempVar; AttributePointer++) {
                            if (LocalElements[ElementPointer].Attributes[AttributePointer].UcaseName == UcaseName) {
                                tempElementAttribute = LocalElements[ElementPointer].Attributes[AttributePointer].Value;
                                break;
                            }
                        }
                    }
                }
                //
                return tempElementAttribute;
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
            }
            //ErrorTrap:
            LogController.handleError( core,new Exception("unexpected exception"));
            return tempElementAttribute;
        }
        //
        //====================================================================================================
        //   Parse a Tag element into its attributes
        //
        private void ParseTag(int ElementPointer) {
            try {
                string TagString = null;
                string[] AttrSplit = null;
                int AttrCount = 0;
                int AttrPointer = 0;
                string AttrName = null;
                string AttrValue = null;
                int AttrValueLen = 0;
                //
                TagString = LocalElements[ElementPointer].Text.Substring(1, LocalElements[ElementPointer].Text.Length - 2);
                if (TagString.Substring(TagString.Length - 1) == "/") {
                    TagString = TagString.Left( TagString.Length - 1);
                }
                //TagString = genericController.vbReplace(TagString, ">", " ") & " "
                TagString = GenericController.vbReplace(TagString, "\r", " ");
                TagString = GenericController.vbReplace(TagString, "\n", " ");
                TagString = GenericController.vbReplace(TagString, "  ", " ");
                //TagString = genericController.vbReplace(TagString, " =", "=")
                //TagString = genericController.vbReplace(TagString, "= ", "=")
                //TagString = genericController.vbReplace(TagString, "'", """")
                LocalElements[ElementPointer].AttributeCount = 0;
                LocalElements[ElementPointer].AttributeSize = 1;
                LocalElements[ElementPointer].Attributes = new Contensive.Processor.Controllers.HtmlParserController.ElementAttributeStructure[1]; // allocates the first
                                                                                                                                              //ClosePosition = Len(TagString)
                                                                                                                                              //If ClosePosition <= 2 Then
                                                                                                                                              //    '
                                                                                                                                              //    ' ----- there is nothing in the <>, skip element
                                                                                                                                              //    '
                                                                                                                                              //Else
                                                                                                                                              //
                                                                                                                                              // ----- Get the tag name
                                                                                                                                              //
                if (!string.IsNullOrEmpty(TagString)) {
                    AttrSplit = SplitDelimited(TagString, " ");
                    AttrCount = AttrSplit.GetUpperBound(0) + 1;
                    if (AttrCount > 0) {
                        LocalElements[ElementPointer].TagName = AttrSplit[0];
                        if (LocalElements[ElementPointer].TagName == "!--") {
                            //
                            // Skip comment tags, ignore the attributes
                            //
                        } else {
                            //
                            // Process the tag
                            //
                            if (AttrCount > 1) {
                                for (AttrPointer = 1; AttrPointer < AttrCount; AttrPointer++) {
                                    AttrName = AttrSplit[AttrPointer];
                                    if (!string.IsNullOrEmpty(AttrName)) {
                                        if (LocalElements[ElementPointer].AttributeCount >= LocalElements[ElementPointer].AttributeSize) {
                                            LocalElements[ElementPointer].AttributeSize = LocalElements[ElementPointer].AttributeSize + 5;
                                            Array.Resize(ref LocalElements[ElementPointer].Attributes, (LocalElements[ElementPointer].AttributeSize) + 1);
                                        }
                                        int EqualPosition = GenericController.vbInstr(1, AttrName, "=");
                                        if (EqualPosition == 0) {
                                            LocalElements[ElementPointer].Attributes[LocalElements[ElementPointer].AttributeCount].Name = AttrName;
                                            LocalElements[ElementPointer].Attributes[LocalElements[ElementPointer].AttributeCount].UcaseName = GenericController.vbUCase(AttrName);
                                            LocalElements[ElementPointer].Attributes[LocalElements[ElementPointer].AttributeCount].Value = AttrName;
                                        } else {
                                            AttrValue = AttrName.Substring(EqualPosition);
                                            AttrValueLen = AttrValue.Length;
                                            if (AttrValueLen > 1) {
                                                if ((AttrValue.Left( 1) == "\"") && (AttrValue.Substring(AttrValueLen - 1, 1) == "\"")) {
                                                    AttrValue = AttrValue.Substring(1, AttrValueLen - 2);
                                                }
                                            }
                                            AttrName = AttrName.Left( EqualPosition - 1);
                                            LocalElements[ElementPointer].Attributes[LocalElements[ElementPointer].AttributeCount].Name = AttrName;
                                            LocalElements[ElementPointer].Attributes[LocalElements[ElementPointer].AttributeCount].UcaseName = GenericController.vbUCase(AttrName);
                                            LocalElements[ElementPointer].Attributes[LocalElements[ElementPointer].AttributeCount].Value = AttrValue;
                                        }
                                        LocalElements[ElementPointer].AttributeCount = LocalElements[ElementPointer].AttributeCount + 1;
                                    }
                                }
                            }
                        }
                    }
                }
                //
                //            CursorPosition = 1
                //            SpacePosition = GetLesserNonZero(InStr(CursorPosition, TagString, " "), ClosePosition)
                //            .TagName = Mid(TagString, 2, SpacePosition - 2)
                //            CursorPosition = SpacePosition + 1
                //            Do While (CursorPosition < ClosePosition) And (CursorPosition <> 0)
                //                SpacePosition = GetLesserNonZero(InStr(CursorPosition, TagString, " "), ClosePosition + 1)
                //                QuotePosition = GetLesserNonZero(InStr(CursorPosition, TagString, """"), ClosePosition + 1)
                //                EqualPosition = GetLesserNonZero(InStr(CursorPosition, TagString, "="), ClosePosition + 1)
                //                '
                //                If .AttributeCount >= .AttributeSize Then
                //                    .AttributeSize = .AttributeSize + 1
                //                    ReDim Preserve .Attributes(.AttributeSize)
                //                    End If
                //                If SpacePosition < EqualPosition Then
                //                    '
                //                    ' ----- Case 1, attribute without a value
                //                    '
                //                    Name = Mid(TagString, CursorPosition, SpacePosition - CursorPosition)
                //                    .Attributes(.AttributeCount).Name = Name
                //                    .Attributes(.AttributeCount).UcaseName = genericController.vbUCase(Name)
                //                    .Attributes(.AttributeCount).Value = Name
                //                    CursorPosition = SpacePosition
                //                ElseIf QuotePosition < SpacePosition Then
                //                    '
                //                    ' ----- Case 2, quoted value
                //                    '
                //                    CloseQuotePosition = GetLesserNonZero(InStr(QuotePosition + 1, TagString, """"), ClosePosition)
                //                    Name = Mid(TagString, CursorPosition, EqualPosition - CursorPosition)
                //                    .Attributes(.AttributeCount).Name = Name
                //                    .Attributes(.AttributeCount).UcaseName = genericController.vbUCase(Name)
                //                    .Attributes(.AttributeCount).Value = Mid(TagString, QuotePosition + 1, CloseQuotePosition - QuotePosition - 1)
                //                    CursorPosition = CloseQuotePosition
                //                Else
                //                    '
                //                    ' ----- Case 2, unquoted value
                //                    '
                //                    Name = Mid(TagString, CursorPosition, EqualPosition - CursorPosition)
                //                    .Attributes(.AttributeCount).Name = Name
                //                    .Attributes(.AttributeCount).UcaseName = genericController.vbUCase(Name)
                //                    .Attributes(.AttributeCount).Value = Mid(TagString, EqualPosition + 1, SpacePosition - EqualPosition - 1)
                //                    CursorPosition = SpacePosition
                //                    End If
                //                If CursorPosition <> 0 Then
                //                    CursorPosition = PassWhiteSpace(CursorPosition + 1, TagString)
                //                    End If
                //                .AttributeCount = .AttributeCount + 1
                //                Loop
                //End If
                //
                return;
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
            }
            //ErrorTrap:
            LogController.handleError( core,new Exception("unexpected exception"));
        }
        //
        //====================================================================================================
        //
        private int GetLesserNonZero(int value0, int value1) {
            int tempGetLesserNonZero = 0;
            try {
                //
                if (value0 == 0) {
                    tempGetLesserNonZero = value1;
                } else {
                    if (value1 == 0) {
                        tempGetLesserNonZero = value0;
                    } else {
                        if (value0 < value1) {
                            tempGetLesserNonZero = value0;
                        } else {
                            tempGetLesserNonZero = value1;
                        }
                    }
                }
                //
                return tempGetLesserNonZero;
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
            }
            //ErrorTrap:
            LogController.handleError( core,new Exception("unexpected exception"));
            return tempGetLesserNonZero;
        }
        //
        //====================================================================================================
        // Pass spaces at the current cursor position
        //
        private int PassWhiteSpace(int CursorPosition, string TagString) {
            int tempPassWhiteSpace = 0;
            try {
                //
                tempPassWhiteSpace = CursorPosition;
                while ((TagString.Substring(tempPassWhiteSpace - 1, 1) == " ") && (tempPassWhiteSpace < TagString.Length)) {
                    tempPassWhiteSpace = tempPassWhiteSpace + 1;
                }
                //
                return tempPassWhiteSpace;
            } catch( Exception ex ) {
                LogController.handleError( core,ex);
            }
            //ErrorTrap:
            LogController.handleError( core,new Exception("unexpected exception"));
            return tempPassWhiteSpace;
        }
        //
        //====================================================================================================
        //
        private void LoadElement(int ElementPtr) {
            int SplitPtr = 0;
            string SplitSrc = null;
            int ElementBasePtr = 0;
            int Ptr = 0;
            string SrcTag = null;
            string SrcBody = null;
            //
            if (NewWay) {
                if (!(LocalElements[ElementPtr].Loaded)) {
                    SplitPtr = encodeInteger(ElementPtr / 2.0);
                    ElementBasePtr = SplitPtr * 2;
                    SplitSrc = SplitStore[SplitPtr];
                    Ptr = GenericController.vbInstr(1, SplitSrc, ">");
                    //
                    // replace blobs
                    //
                    if (Ptr == 0) {
                        SrcTag = "";
                        SrcBody = ReplaceBlob(SplitSrc);
                    } else {
                        SrcTag = ReplaceBlob(SplitSrc.Left( Ptr));
                        SrcBody = ReplaceBlob(SplitSrc.Substring(Ptr));
                    }
                    if (Ptr == 0) {
                        if (ElementPtr == 0) {
                            //
                            // no close tag, elementptr=0 then First entry is empty, second is body
                            //
                            LocalElements[ElementBasePtr].AttributeCount = 0;
                            LocalElements[ElementBasePtr].IsTag = false;
                            LocalElements[ElementBasePtr].Loaded = true;
                            LocalElements[ElementBasePtr].Position = 0;
                            LocalElements[ElementBasePtr].Text = "";
                            //
                            LocalElements[ElementBasePtr + 1].AttributeCount = 0;
                            LocalElements[ElementBasePtr + 1].IsTag = false;
                            LocalElements[ElementBasePtr + 1].Loaded = true;
                            LocalElements[ElementBasePtr + 1].Position = 0;
                            LocalElements[ElementBasePtr + 1].Text = SplitSrc;
                        } else {
                            //
                            // no close tag, elementptr>0 then First entry is '<', second is body
                            //
                            LocalElements[ElementBasePtr].AttributeCount = 0;
                            LocalElements[ElementBasePtr].IsTag = false;
                            LocalElements[ElementBasePtr].Loaded = true;
                            LocalElements[ElementBasePtr].Position = 0;
                            LocalElements[ElementBasePtr].Text = "<";
                            //
                            LocalElements[ElementBasePtr + 1].AttributeCount = 0;
                            LocalElements[ElementBasePtr + 1].IsTag = false;
                            LocalElements[ElementBasePtr + 1].Loaded = true;
                            LocalElements[ElementBasePtr + 1].Position = 0;
                            LocalElements[ElementBasePtr + 1].Text = SplitSrc;
                        }
                    } else {
                        //
                        // close tag found, first entry is tag text, second entry is body
                        //
                        LocalElements[ElementBasePtr].Text = "<" + SrcTag;
                        LocalElements[ElementBasePtr].IsTag = true;
                        ParseTag(ElementBasePtr);
                        LocalElements[ElementBasePtr].Loaded = true;
                        //
                        LocalElements[ElementBasePtr + 1].AttributeCount = 0;
                        LocalElements[ElementBasePtr + 1].IsTag = false;
                        LocalElements[ElementBasePtr + 1].Loaded = true;
                        LocalElements[ElementBasePtr + 1].Position = 0;
                        LocalElements[ElementBasePtr + 1].Text = SrcBody;
                    }
                }
            }
        }
        //
        //====================================================================================================
        //
        private string ReplaceBlob(string Src) {
            string tempReplaceBlob = null;
            int Pos = 0;
            int PosEnd = 0;
            int PosNum = 0;
            string PtrText = null;
            int Ptr = 0;
            string Blob = "";
            //
            tempReplaceBlob = Src;
            Pos = GenericController.vbInstr(1, Src, BlobSN);
            if (Pos != 0) {
                PosEnd = GenericController.vbInstr(Pos + 1, Src, "/");
                if (PosEnd > 0) {
                    PosNum = GenericController.vbInstr(Pos + 1, Src, ":");
                    if (PosNum > 0) {
                        PtrText = Src.Substring(PosNum, PosEnd - PosNum - 1);
                        if (PtrText.IsNumeric()) {
                            Ptr = int.Parse(PtrText);
                            if (Ptr < BlobCnt) {
                                Blob = Blobs[Ptr];
                            }
                            tempReplaceBlob = Src.Left( Pos - 1) + Blob + Src.Substring(PosEnd);
                        }
                    }
                }
            }

            return tempReplaceBlob;
        }
    }
}
