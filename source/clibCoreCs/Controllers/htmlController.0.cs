﻿
using System;
using Contensive.BaseClasses;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
using Contensive.Core.Models.Entity;

namespace Contensive.Core.Controllers {
    /// <summary>
    /// Tools used to assemble html document elements. This is not a storage for assembling a document (see docController)
    /// </summary>
    public class htmlController {
        //
        private coreClass cpCore;
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cpCore"></param>
        /// <remarks></remarks>
        public htmlController(coreClass cpCore) {
            this.cpCore = cpCore;
        }
        //
        //====================================================================================================
        /// <summary>
        /// setOuter
        /// </summary>
        /// <param name="ignore"></param>
        /// <param name="layout"></param>
        /// <param name="Key"></param>
        /// <param name="textToInsert"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string insertOuterHTML(object ignore, string layout, string Key, string textToInsert) {
            string returnValue = "";
            try {
                if (string.IsNullOrEmpty(Key)) {
                    returnValue = textToInsert;
                } else {
                    returnValue = layout;
                    int posStart = getTagStartPos2(ignore, layout, 1, Key);
                    if (posStart != 0) {
                        int posEnd = getTagEndPos(ignore, layout, posStart);
                        if (posEnd > 0) {
                            //
                            // seems like these are the correct positions here.
                            //
                            returnValue = layout.Substring(0, posStart - 1) + textToInsert + layout.Substring(posEnd - 1);
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnValue;
        }
        //
        //====================================================================================================
        public string insertInnerHTML(object ignore, string layout, string Key, string textToInsert) {
            string returnValue = "";
            try {
                int posStart = 0;
                int posEnd = 0;
                //
                // short-cut for now, get the outerhtml, find the position, then remove the wrapping tags
                //
                if (string.IsNullOrEmpty(Key)) {
                    returnValue = textToInsert;
                } else {
                    returnValue = layout;
                    posStart = getTagStartPos2(ignore, layout, 1, Key);
                    //outerHTML = getOuterHTML(ignore, layout, Key, PosStart)
                    if (posStart != 0) {
                        posEnd = getTagEndPos(ignore, layout, posStart);
                        if (posEnd > 0) {
                            posStart = genericController.vbInstr(posStart + 1, layout, ">");
                            if (posStart != 0) {
                                posStart = posStart + 1;
                                posEnd = layout.LastIndexOf("<", posEnd - 2) + 1;
                                if (posEnd != 0) {
                                    returnValue = layout.Substring(0, posStart - 1) + textToInsert + layout.Substring(posEnd - 1);
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnValue;
        }
        //
        //====================================================================================================
        /// <summary>
        /// getInnerHTML
        /// </summary>
        /// <param name="ignore"></param>
        /// <param name="layout"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string getInnerHTML(object ignore, string layout, string Key) {
            string returnValue = "";
            try {
                int posStart = 0;
                int posEnd = 0;
                //
                // short-cut for now, get the outerhtml, find the position, then remove the wrapping tags
                //
                if (string.IsNullOrEmpty(Key)) {
                    //
                    // inner of nothing is nothing
                    //
                } else {
                    returnValue = layout;
                    posStart = getTagStartPos2(ignore, layout, 1, Key);
                    if (posStart != 0) {
                        posEnd = getTagEndPos(ignore, layout, posStart);
                        if (posEnd > 0) {
                            posStart = genericController.vbInstr(posStart + 1, layout, ">");
                            if (posStart != 0) {
                                posStart = posStart + 1;
                                posEnd = layout.LastIndexOf("<", posEnd - 2) + 1;
                                if (posEnd != 0) {
                                    //
                                    // now move the end forward to skip trailing whitespace
                                    //
                                    do {
                                        posEnd = posEnd + 1;
                                    } while ((posEnd < layout.Length) && ("\t" + "\r" + "\n" + "\t" + " ".IndexOf(layout.Substring(posEnd - 1, 1)) + 1 != 0));
                                    posEnd = posEnd - 1;
                                    returnValue = layout.Substring(posStart - 1, (posEnd - posStart));
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnValue;
        }
        //
        //====================================================================================================
        /// <summary>
        /// getOuterHTML
        /// </summary>
        /// <param name="ignore"></param>
        /// <param name="layout"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string getOuterHTML(object ignore, string layout, string Key) {
            string returnValue = "";
            try {
                int posStart = 0;
                int posEnd = 0;
                string s;
                //
                s = layout;
                if (!string.IsNullOrEmpty(s)) {
                    posStart = getTagStartPos2(ignore, s, 1, Key);
                    if (posStart > 0) {
                        //
                        // now backtrack to include the leading whitespace
                        //
                        while ((posStart > 0) && ("\t" + "\r" + "\n" + "\t" + " ".IndexOf(s.Substring(posStart - 1, 1)) + 1 != 0)) {
                            posStart = posStart - 1;
                        }
                        //posStart = posStart + 1
                        s = s.Substring(posStart - 1);
                        posEnd = getTagEndPos(ignore, s, 1);
                        if (posEnd > 0) {
                            s = s.Substring(0, posEnd - 1);
                            returnValue = s;
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnValue;
        }
        //
        //====================================================================================================
        private bool tagMatch(string layout, int posStartTag, string searchId, string searchClass) {
            bool returnValue = false;
            try {
                const string attrAllowedChars = "abcdefghijklmnopqrstuvwzyz-_";
                string Tag = null;
                string tagLower = null;
                int Pos = 0;
                string Delimiter = null;
                string testChar = null;
                int tagLength = 0;
                int posValueStart = 0;
                string testValue = null;
                string[] testValues = null;
                int testCnt = 0;
                int Ptr = 0;
                //
                returnValue = false;
                Pos = genericController.vbInstr(posStartTag, layout, ">");
                if (Pos > 0) {
                    returnValue = true;
                    Tag = layout.Substring(posStartTag - 1, Pos - posStartTag + 1);
                    tagLower = genericController.vbLCase(Tag);
                    tagLength = Tag.Length;
                    //
                    // check searchId
                    //
                    if (returnValue && (!string.IsNullOrEmpty(searchId))) {
                        Pos = genericController.vbInstr(1, tagLower, " id=", Microsoft.VisualBasic.Constants.vbTextCompare);
                        if (Pos <= 0) {
                            //
                            // id required but this tag has no id attr
                            //
                            returnValue = false;
                        } else {
                            //
                            // test if the id attr value matches the searchClass
                            //
                            Pos = Pos + 4;
                            Delimiter = tagLower.Substring(Pos - 1, 1);
                            testValue = "";
                            if ((Delimiter == "\"") || (Delimiter == "'")) {
                                //
                                // search for end of delimited attribute value
                                //
                                posValueStart = Pos + 1;
                                do {
                                    Pos = Pos + 1;
                                    testChar = tagLower.Substring(Pos - 1, 1);
                                } while ((Pos < tagLength) && (testChar != Delimiter));
                                if (Pos >= tagLength) {
                                    //
                                    // delimiter not found, html error
                                    //
                                    returnValue = false;
                                } else {
                                    testValue = Tag.Substring(posValueStart - 1, Pos - posValueStart);
                                }
                            } else {
                                //
                                // search for end of non-delimited attribute value
                                //
                                posValueStart = Pos;
                                while ((Pos < tagLength) && (isInStr(1, attrAllowedChars, tagLower.Substring(Pos - 1, 1), Microsoft.VisualBasic.Constants.vbTextCompare))) {
                                    Pos = Pos + 1;
                                }
                                if (Pos >= tagLength) {
                                    //
                                    // delimiter not found, html error
                                    //
                                    returnValue = false;
                                } else {
                                    testValue = Tag.Substring(posValueStart - 1, Pos - posValueStart);
                                }
                            }
                            if (returnValue && (!string.IsNullOrEmpty(testValue))) {
                                //
                                //
                                //
                                if (searchId != testValue) {
                                    //
                                    // there can only be one id, and this does not match
                                    //
                                    returnValue = false;
                                }
                            }
                        }
                    }
                    //
                    // check searchClass
                    //
                    if (returnValue && (!string.IsNullOrEmpty(searchClass))) {
                        Pos = genericController.vbInstr(1, tagLower, " class=", Microsoft.VisualBasic.Constants.vbTextCompare);
                        if (Pos <= 0) {
                            //
                            // class required but this tag has no class attr
                            //
                            returnValue = false;
                        } else {
                            //
                            // test if the class attr value matches the searchClass
                            //
                            Pos = Pos + 7;
                            Delimiter = tagLower.Substring(Pos - 1, 1);
                            testValue = "";
                            if ((Delimiter == "\"") || (Delimiter == "'")) {
                                //
                                // search for end of delimited attribute value
                                //
                                posValueStart = Pos + 1;
                                do {
                                    Pos = Pos + 1;
                                    testChar = tagLower.Substring(Pos - 1, 1);
                                } while ((Pos < tagLength) && (testChar != Delimiter));
                                if (Pos >= tagLength) {
                                    //
                                    // delimiter not found, html error
                                    //
                                    returnValue = false;
                                } else {
                                    testValue = Tag.Substring(posValueStart - 1, Pos - posValueStart);
                                }
                            } else {
                                //
                                // search for end of non-delimited attribute value
                                //
                                posValueStart = Pos;
                                while ((Pos < tagLength) && (isInStr(1, attrAllowedChars, tagLower.Substring(Pos - 1, 1), Microsoft.VisualBasic.Constants.vbTextCompare))) {
                                    Pos = Pos + 1;
                                }
                                if (Pos >= tagLength) {
                                    //
                                    // delimiter not found, html error
                                    //
                                    returnValue = false;
                                } else {
                                    testValue = Tag.Substring(posValueStart - 1, Pos - posValueStart);
                                }
                            }
                            if (returnValue && (!string.IsNullOrEmpty(testValue))) {
                                //
                                //
                                //
                                testValues = testValue.Split(' ');
                                testCnt = testValues.GetUpperBound(0) + 1;
                                for (Ptr = 0; Ptr < testCnt; Ptr++) {
                                    if (searchClass == testValues[Ptr]) {
                                        break;
                                    }
                                }
                                if (Ptr >= testCnt) {
                                    returnValue = false;
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnValue;
        }
        //
        //====================================================================================================
        public int getTagStartPos2(object ignore, string layout, int layoutStartPos, string Key) {
            int returnValue = 0;
            try {
                int returnPos = 0;
                int SegmentStart = 0;
                int Pos = 0;
                int LoopPtr = 0;
                string searchKey = null;
                int lenSearchKey = 0;
                int Ptr = 0;
                string workingKey = null;
                string[] workingKeys = null;
                string searchClass = null;
                string searchId = null;
                string searchTag = null;
                int posStartTag = 0;
                //
                returnPos = 0;
                workingKey = Key;
                if (genericController.vbInstr(1, workingKey, ">") != 0) {
                    //
                    // does not support > yet.
                    //
                    workingKey = genericController.vbReplace(workingKey, ">", " ");
                }
                //
                // eliminate whitespace
                //
                while (genericController.vbInstr(1, workingKey, "\t") != 0) {
                    workingKey = genericController.vbReplace(workingKey, "\t", " ");
                }
                //
                while (genericController.vbInstr(1, workingKey, "\r") != 0) {
                    workingKey = genericController.vbReplace(workingKey, "\r", " ");
                }
                //
                while (genericController.vbInstr(1, workingKey, "\n") != 0) {
                    workingKey = genericController.vbReplace(workingKey, "\n", " ");
                }
                //
                while (genericController.vbInstr(1, workingKey, "  ") != 0) {
                    workingKey = genericController.vbReplace(workingKey, "  ", " ");
                }
                //
                workingKey = workingKey.Trim(' ');
                //
                if (genericController.vbInstr(1, workingKey, " ") != 0) {
                    //
                    // if there are spaces, do them sequentially
                    //
                    workingKeys = workingKey.Split(' ');
                    SegmentStart = 1;
                    while ((!string.IsNullOrEmpty(layout)) & (SegmentStart != 0) && (Ptr <= workingKeys.GetUpperBound(0))) {
                        SegmentStart = getTagStartPos2(null, layout, SegmentStart, workingKeys[Ptr]);
                        Ptr = Ptr + 1;
                    }
                    returnPos = SegmentStart;
                } else {
                    //
                    // find this single key and get the outerHTML
                    //   at this point, the key can be
                    //       a class = .xxxx
                    //       an id = #xxxx
                    //       a tag = xxxx
                    //       a compound in either form, xxxx.xxxx or xxxx#xxxx
                    //
                    //   searchKey = the search pattern to start
                    //
                    if (workingKey.Substring(0, 1) == ".") {
                        //
                        // search for a class
                        //
                        searchClass = workingKey.Substring(1);
                        searchTag = "";
                        searchId = "";
                        Pos = genericController.vbInstr(1, searchClass, "#");
                        if (Pos != 0) {
                            searchId = searchClass.Substring(Pos - 1);
                            searchClass = searchClass.Substring(0, Pos - 1);
                        }
                        //
                        //workingKey = Mid(workingKey, 2)
                        searchKey = "<";
                    } else if (workingKey.Substring(0, 1) == "#") {
                        //
                        // search for an ID
                        //
                        searchClass = "";
                        searchTag = "";
                        searchId = workingKey.Substring(1);
                        Pos = genericController.vbInstr(1, searchId, ".");
                        if (Pos != 0) {
                            searchClass = searchId.Substring(Pos - 1);
                            searchId = searchId.Substring(0, Pos - 1);
                        }
                        //
                        //workingKey = Mid(workingKey, 2)
                        searchKey = "<";
                    } else {
                        //
                        // search for a tagname
                        //
                        searchClass = "";
                        searchTag = workingKey;
                        searchId = "";
                        //
                        Pos = genericController.vbInstr(1, searchTag, "#");
                        if (Pos != 0) {
                            searchId = searchTag.Substring(Pos);
                            searchTag = searchTag.Substring(0, Pos - 1);
                            Pos = genericController.vbInstr(1, searchId, ".");
                            if (Pos != 0) {
                                searchClass = searchId.Substring(Pos - 1);
                                searchId = searchId.Substring(0, Pos - 1);
                            }
                        }
                        Pos = genericController.vbInstr(1, searchTag, ".");
                        if (Pos != 0) {
                            searchClass = searchTag.Substring(Pos);
                            searchTag = searchTag.Substring(0, Pos - 1);
                            Pos = genericController.vbInstr(1, searchClass, "#");
                            if (Pos != 0) {
                                searchId = searchClass.Substring(Pos - 1);
                                searchClass = searchClass.Substring(0, Pos - 1);
                            }
                        }
                        //
                        searchKey = "<" + searchTag;
                    }
                    lenSearchKey = searchKey.Length;
                    Pos = layoutStartPos;
                    //posMatch = genericController.vbInstr(layoutStartPos, layout, searchKey)
                    //pos = posMatch
                    //searchIsOver = False
                    do {
                        Pos = genericController.vbInstr(Pos, layout, searchKey);
                        if (Pos == 0) {
                            //
                            // not found, return empty
                            //
                            //s = ""
                            break;
                        } else {
                            //
                            // string found - go to the start of the tag
                            //
                            posStartTag = layout.LastIndexOf("<", Pos) + 1;
                            if (posStartTag <= 0) {
                                //
                                // bad html, no start tag found
                                //
                                Pos = 0;
                                returnPos = 0;
                            } else if (layout.Substring(posStartTag - 1, 2) == "</") {
                                //
                                // this is an end tag, skip it
                                //
                                Pos = Pos + 1;
                            } else if (tagMatch(layout, posStartTag, searchId, searchClass)) {
                                //
                                // match, return with this position
                                //
                                returnPos = Pos;
                                break;
                            } else {
                                //
                                // no match, skip this and go to the next
                                //
                                Pos = Pos + 1;
                            }
                        }
                        LoopPtr = LoopPtr + 1;
                    } while (LoopPtr < 1000);
                    //
                    //
                    //
                    if (LoopPtr >= 10000) {
                        cpCore.handleException(new ApplicationException("Tag limit of 10000 tags per block reached."));
                    }
                }
                //
                returnValue = returnPos;
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnValue;
        }
        //
        //====================================================================================================
        public int getTagStartPos(object ignore, string layout, int layoutStartPos, string Key) {
            int returnValue = 0;
            try {
                returnValue = getTagStartPos2(ignore, layout, layoutStartPos, Key);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnValue;
        }
        //
        //=================================================================================================
        //   return the position following the tag which closes the tag that starts the string
        //       starting postion<div><div><p>this and that</p><!-- </div> --></div></div>And a whole lot more
        //       returns the position of the "A" following the last /div
        //       string 123<img>456 returns pointing to "4"
        //       string 123456 returns pointing to "6"
        //       returns 0 if the end was not found
        //=================================================================================================
        //
        public int getTagEndPos(object ignore, string Source, int startPos) {
            int returnValue = 0;
            try {
                int Pos = 0;
                string TagName = null;
                string endTag = null;
                string startTag = null;
                int posNest = 0;
                int posEnd = 0;
                int posComment = 0;
                string c = null;
                //
                Pos = genericController.vbInstr(startPos, Source, "<");
                TagName = "";
                returnValue = 0;
                if (Pos != 0) {
                    Pos = Pos + 1;
                    while (Pos < Source.Length) {
                        c = genericController.vbLCase(Source.Substring(Pos - 1, 1));
                        if ((string.CompareOrdinal(c, "a") >= 0) && (string.CompareOrdinal(c, "z") <= 0)) {
                            TagName = TagName + c;
                        } else {
                            break;
                        }
                        Pos = Pos + 1;
                    }
                    if (!string.IsNullOrEmpty(TagName)) {
                        endTag = "</" + TagName;
                        startTag = "<" + TagName;
                        while (Pos != 0) {
                            posEnd = genericController.vbInstr(Pos + 1, Source, endTag, Microsoft.VisualBasic.Constants.vbTextCompare);
                            if (posEnd == 0) {
                                //
                                // no end was found, return the tag or rest of the string
                                //
                                returnValue = genericController.vbInstr(Pos + 1, Source, ">") + 1;
                                if (posEnd == 1) {
                                    returnValue = Source.Length;
                                }
                                break;
                            } else {
                                posNest = genericController.vbInstr(Pos + 1, Source, startTag, Microsoft.VisualBasic.Constants.vbTextCompare);
                                if (posNest == 0) {
                                    //
                                    // no nest found, set to end
                                    //
                                    posNest = Source.Length;
                                }
                                posComment = genericController.vbInstr(Pos + 1, Source, "<!--");
                                if (posComment == 0) {
                                    //
                                    // no comment found, set to end
                                    //
                                    posComment = Source.Length;
                                }
                                if ((posNest < posEnd) && (posNest < posComment)) {
                                    //
                                    // ----- the tag is nested, find the end of the nest
                                    //
                                    Pos = getTagEndPos(ignore, Source, posNest);
                                    // 8/28/2012, if there is a nested tag right before the correct end tag, it skips the end:
                                    // <div class=a>a<div class=b>b</div></div>
                                    // the second /div is missed because returnValue returns one past the >, then the
                                    // next search starts +1 that position
                                    if (Pos > 0) {
                                        Pos = Pos - 1;
                                    }
                                } else if (posComment < posEnd) {
                                    //
                                    // ----- there is a comment between the tag and the first tagend, skip it
                                    //
                                    Pos = genericController.vbInstr(posComment, Source, "-->");
                                    if (Pos == 0) {
                                        //
                                        // start comment with no end, exit now
                                        //
                                        returnValue = Source.Length;
                                        break;
                                    }
                                } else {
                                    //
                                    // ----- end position is here, go to the end of it and exit
                                    //
                                    Pos = genericController.vbInstr(posEnd, Source, ">");
                                    if (Pos == 0) {
                                        //
                                        // no end was found, just exit
                                        //
                                        returnValue = Source.Length;
                                        break;
                                    } else {
                                        //
                                        // ----- end was found
                                        //
                                        returnValue = Pos + 1;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnValue;
        }
        //
        //========================================================================================================
        //
        // Finds all tags matching the input, and concatinates them into the output
        // does NOT account for nested tags, use for body, script, style
        //
        // ReturnAll - if true, it returns all the occurances, back-to-back
        //
        //========================================================================================================
        //
        public static string getTagInnerHTML(string PageSource, string Tag, bool ReturnAll) {
            string tempgetTagInnerHTML = null;
            try {
                //
                int TagStart = 0;
                int TagEnd = 0;
                int LoopCnt = 0;
                string WB = null;
                int Pos = 0;
                int PosEnd = 0;
                int CommentPos = 0;
                int ScriptPos = 0;
                //
                tempgetTagInnerHTML = "";
                Pos = 1;
                while ((Pos > 0) && (LoopCnt < 100)) {
                    TagStart = genericController.vbInstr(Pos, PageSource, "<" + Tag, Microsoft.VisualBasic.Constants.vbTextCompare);
                    if (TagStart == 0) {
                        Pos = 0;
                    } else {
                        //
                        // tag found, skip any comments that start between current position and the tag
                        //
                        CommentPos = genericController.vbInstr(Pos, PageSource, "<!--");
                        if ((CommentPos != 0) && (CommentPos < TagStart)) {
                            //
                            // skip comment and start again
                            //
                            Pos = genericController.vbInstr(CommentPos, PageSource, "-->");
                        } else {
                            ScriptPos = genericController.vbInstr(Pos, PageSource, "<script");
                            if ((ScriptPos != 0) && (ScriptPos < TagStart)) {
                                //
                                // skip comment and start again
                                //
                                Pos = genericController.vbInstr(ScriptPos, PageSource, "</script");
                            } else {
                                //
                                // Get the tags innerHTML
                                //
                                TagStart = genericController.vbInstr(TagStart, PageSource, ">", Microsoft.VisualBasic.Constants.vbTextCompare);
                                Pos = TagStart;
                                if (TagStart != 0) {
                                    TagStart = TagStart + 1;
                                    TagEnd = genericController.vbInstr(TagStart, PageSource, "</" + Tag, Microsoft.VisualBasic.Constants.vbTextCompare);
                                    if (TagEnd != 0) {
                                        tempgetTagInnerHTML += PageSource.Substring(TagStart - 1, TagEnd - TagStart);
                                    }
                                }
                            }
                        }
                        LoopCnt = LoopCnt + 1;
                        if (ReturnAll) {
                            TagStart = genericController.vbInstr(TagEnd, PageSource, "<" + Tag, Microsoft.VisualBasic.Constants.vbTextCompare);
                        } else {
                            TagStart = 0;
                        }
                    }
                }
                //
                return tempgetTagInnerHTML;
                //
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            return tempgetTagInnerHTML;
        }
        //
        //====================================================================================================
        //
        public string getHtmlDoc_beforeEndOfBodyHtml(bool AllowLogin, bool AllowTools) {
            List<string> result = new List<string>();
            try {
                List<string> bodyScript = new List<string>();
                //
                // -- content extras like tool panel
                if ((cpCore.doc.authContext.isAuthenticatedContentManager(cpCore) & cpCore.doc.authContext.user.AllowToolsPanel) != 0) {
                    if (AllowTools) {
                        result.Add(cpCore.html.main_GetToolsPanel());
                    }
                } else {
                    if (AllowLogin) {
                        result.Add(main_GetLoginLink());
                    }
                }
                //
                // -- Include any other close page
                if (cpCore.doc.htmlForEndOfBody != "") {
                    result.Add(cpCore.doc.htmlForEndOfBody);
                }
                if (cpCore.doc.testPointMessage != "") {
                    result.Add("<div class=\"ccTestPointMessageCon\">" + cpCore.doc.testPointMessage + "</div>");
                }
                //
                // TODO -- closing the menu attaches the flyout panels -- should be done when the menu is returned, not at page end
                // -- output the menu system
                if (cpCore.menuFlyout != null) {
                    result.Add(cpCore.menuFlyout.menu_GetClose());
                }
                //
                // -- Add onload javascript
                foreach (htmlAssetClass asset in cpCore.doc.htmlAssetList.FindAll((a) => (a.assetType == htmlAssetTypeEnum.OnLoadScript) && (!string.IsNullOrEmpty(a.content)))) {
                    result.Add("<script Language=\"JavaScript\" type=\"text/javascript\">window.addEventListener('load', function(){" + asset.content + "});</script>");
                }
                //
                // -- body Javascript
                bool allowDebugging = cpCore.visitProperty.getBoolean("AllowDebugging");
                foreach (var jsBody in cpCore.doc.htmlAssetList.FindAll((a) => (a.assetType == htmlAssetTypeEnum.script) & (!a.inHead) && (!string.IsNullOrEmpty(a.content)))) {
                    if ((jsBody.addedByMessage != "") && allowDebugging) {
                        result.Add("<!-- from " + jsBody.addedByMessage + " -->");
                    }
                    if (!jsBody.isLink) {
                        result.Add("<script Language=\"JavaScript\" type=\"text/javascript\">" + jsBody.content + "</script>");
                    } else {
                        result.Add("<script type=\"text/javascript\" src=\"" + jsBody.content + "\"></script>");
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return string.Join("\r", result);
        }
        //
        //========================================================================
        // main_Get a string with a Drop Down Select Box, see PrintFormInputSelect
        //========================================================================
        //
        public string main_GetFormInputSelect(string MenuName, int CurrentValue, string ContentName, string Criteria = "", string NoneCaption = "", string htmlId = "") {
            bool tempVar = false;
            return main_GetFormInputSelect2(MenuName, CurrentValue, ContentName, Criteria, NoneCaption, htmlId, ref tempVar, "");
        }
        //
        //
        //
        public string main_GetFormInputSelect2(string MenuName, int CurrentValue, string ContentName, string Criteria, string NoneCaption, string htmlId, ref bool return_IsEmptyList, string HtmlClass = "") {
            string result = string.Empty;
            try {
                const string MenuNameFPO = "<MenuName>";
                const string NoneCaptionFPO = "<NoneCaption>";
                Models.Complex.cdefModel CDef = null;
                string ContentControlCriteria = null;
                string LcaseCriteria = null;
                int CSPointer = 0;
                bool SelectedFound = false;
                int RecordID = 0;
                string Copy = null;
                string MethodName = null;
                string DropDownFieldList = null;
                string[] DropDownFieldName = { };
                string[] DropDownDelimiter = { };
                int DropDownFieldCount = 0;
                string DropDownPreField = string.Empty;
                int DropDownFieldListLength = 0;
                string FieldName = string.Empty;
                string CharAllowed = null;
                string CharTest = null;
                int CharPointer = 0;
                int IDFieldPointer = 0;
                stringBuilderLegacyController FastString = new stringBuilderLegacyController();
                string[,] RowsArray = null;
                string[] RowFieldArray = null;
                int RowCnt = 0;
                int RowMax = 0;
                int ColumnMax = 0;
                int RowPointer = 0;
                int ColumnPointer = 0;
                int[] DropDownFieldPointer = null;
                string UcaseFieldName = null;
                string SortFieldList = string.Empty;
                string SQL = null;
                string TableName = null;
                string DataSource = null;
                string SelectFields = null;
                int Ptr = 0;
                string SelectRaw = string.Empty;
                int CachePtr = 0;
                string TagID = null;
                string CurrentValueText = null;
                //
                MethodName = "main_GetFormInputSelect2";
                //
                LcaseCriteria = genericController.vbLCase(Criteria);
                return_IsEmptyList = true;
                //
                CurrentValueText = CurrentValue.ToString();
                foreach (constants.main_InputSelectCacheType inputSelect in cpCore.doc.inputSelectCache) {
                    if ((inputSelect.ContentName == ContentName) && (inputSelect.Criteria == LcaseCriteria) && (inputSelect.CurrentValue == CurrentValueText)) {
                        SelectRaw = inputSelect.SelectRaw;
                        return_IsEmptyList = false;
                        break;
                    }
                }
                //
                //
                //
                if (string.IsNullOrEmpty(SelectRaw)) {
                    //
                    // Build the SelectRaw
                    // Test selection size
                    //
                    // This was commented out -- I really do not know why -- seems like the best way
                    //
                    CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
                    TableName = CDef.ContentTableName;
                    DataSource = CDef.ContentDataSourceName;
                    ContentControlCriteria = CDef.ContentControlCriteria;
                    //
                    // This is what was there
                    //
                    //        TableName = main_GetContentProperty(ContentName, "ContentTableName")
                    //        DataSource = main_GetContentProperty(ContentName, "ContentDataSourceName")
                    //        ContentControlCriteria = main_GetContentProperty(ContentName, "ContentControlCriteria")
                    //
                    SQL = "select count(*) as cnt from " + TableName + " where " + ContentControlCriteria;
                    if (!string.IsNullOrEmpty(LcaseCriteria)) {
                        SQL += " and " + LcaseCriteria;
                    }
                    DataTable dt = cpCore.db.executeQuery(SQL);
                    if (dt.Rows.Count > 0) {
                        RowCnt = genericController.EncodeInteger(dt.Rows[0].Item("cnt"));
                    }
                    if (RowCnt == 0) {
                        RowMax = -1;
                    } else {
                        return_IsEmptyList = false;
                        RowMax = RowCnt - 1;
                    }
                    //
                    if (RowCnt > cpCore.siteProperties.selectFieldLimit) {
                        //
                        // Selection is too big
                        //
                        errorController.error_AddUserError(cpCore, "The drop down list for " + ContentName + " called " + MenuName + " is too long to display. The site administrator has been notified and the problem will be resolved shortly. To fix this issue temporarily, go to the admin tab of the Preferences page and set the Select Field Limit larger than " + RowCnt + ".");
                        //                    cpcore.handleException(New Exception("Legacy error, MethodName=[" & MethodName & "], cause=[" & Cause & "] #" & Err.Number & "," & Err.Source & "," & Err.Description & ""), Cause, 2)

                        cpCore.handleException(new Exception("Error creating select list from content [" + ContentName + "] called [" + MenuName + "]. Selection of [" + RowCnt + "] records exceeds [" + cpCore.siteProperties.selectFieldLimit + "], the current Site Property SelectFieldLimit."));
                        result = result + html_GetFormInputHidden(MenuNameFPO, CurrentValue);
                        if (CurrentValue == 0) {
                            result = html_GetFormInputText2(MenuName, "0");
                        } else {
                            CSPointer = cpCore.db.csOpenRecord(ContentName, CurrentValue);
                            if (cpCore.db.csOk(CSPointer)) {
                                result = cpCore.db.csGetText(CSPointer, "name") + "&nbsp;";
                            }
                            cpCore.db.csClose(ref CSPointer);
                        }
                        result = result + "(Selection is too large to display option list)";
                    } else {
                        //
                        // ----- Generate Drop Down Field Names
                        //
                        DropDownFieldList = CDef.DropDownFieldList;
                        //DropDownFieldList = main_GetContentProperty(ContentName, "DropDownFieldList")
                        if (string.IsNullOrEmpty(DropDownFieldList)) {
                            DropDownFieldList = "NAME";
                        }
                        DropDownFieldCount = 0;
                        CharAllowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                        DropDownFieldListLength = DropDownFieldList.Length;
                        for (CharPointer = 1; CharPointer <= DropDownFieldListLength; CharPointer++) {
                            CharTest = DropDownFieldList.Substring(CharPointer - 1, 1);
                            if (genericController.vbInstr(1, CharAllowed, CharTest) == 0) {
                                //
                                // Character not allowed, delimit Field name here
                                //
                                if (!string.IsNullOrEmpty(FieldName)) {
                                    //
                                    // ----- main_Get new Field Name and save it
                                    //
                                    if (string.IsNullOrEmpty(SortFieldList)) {
                                        SortFieldList = FieldName;
                                    }
                                    Array.Resize(ref DropDownFieldName, DropDownFieldCount + 1);
                                    Array.Resize(ref DropDownDelimiter, DropDownFieldCount + 1);
                                    DropDownFieldName[DropDownFieldCount] = FieldName;
                                    DropDownDelimiter[DropDownFieldCount] = CharTest;
                                    DropDownFieldCount = DropDownFieldCount + 1;
                                    FieldName = "";
                                } else {
                                    //
                                    // ----- Save Field Delimiter
                                    //
                                    if (DropDownFieldCount == 0) {
                                        //
                                        // ----- Before any field, add to DropDownPreField
                                        //
                                        DropDownPreField = DropDownPreField + CharTest;
                                    } else {
                                        //
                                        // ----- after a field, add to last DropDownDelimiter
                                        //
                                        DropDownDelimiter[DropDownFieldCount - 1] = DropDownDelimiter[DropDownFieldCount - 1] + CharTest;
                                    }
                                }
                            } else {
                                //
                                // Character Allowed, Put character into fieldname and continue
                                //
                                FieldName = FieldName + CharTest;
                            }
                        }
                        if (!string.IsNullOrEmpty(FieldName)) {
                            if (string.IsNullOrEmpty(SortFieldList)) {
                                SortFieldList = FieldName;
                            }
                            Array.Resize(ref DropDownFieldName, DropDownFieldCount + 1);
                            Array.Resize(ref DropDownDelimiter, DropDownFieldCount + 1);
                            DropDownFieldName[DropDownFieldCount] = FieldName;
                            DropDownDelimiter[DropDownFieldCount] = "";
                            DropDownFieldCount = DropDownFieldCount + 1;
                        }
                        if (DropDownFieldCount == 0) {
                            cpCore.handleException(new Exception("No drop down field names found for content [" + ContentName + "]."));
                        } else {
                            DropDownFieldPointer = new int[DropDownFieldCount];
                            SelectFields = "ID";
                            for (Ptr = 0; Ptr < DropDownFieldCount; Ptr++) {
                                SelectFields = SelectFields + "," + DropDownFieldName[Ptr];
                            }
                            //
                            // ----- Start select box
                            //
                            TagID = "";
                            if (!string.IsNullOrEmpty(htmlId)) {
                                TagID = " ID=\"" + htmlId + "\"";
                            }
                            FastString.Add("<select size=\"1\" name=\"" + MenuNameFPO + "\"" + TagID + ">");
                            FastString.Add("<option value=\"\">" + NoneCaptionFPO + "</option>");
                            //
                            // ----- select values
                            //
                            CSPointer = cpCore.db.csOpen(ContentName, Criteria, SortFieldList,,,,, SelectFields);
                            if (cpCore.db.csOk(CSPointer)) {
                                RowsArray = cpCore.db.cs_getRows(CSPointer);
                                RowFieldArray = cpCore.db.cs_getSelectFieldList(CSPointer).Split(',');
                                ColumnMax = RowsArray.GetUpperBound(0);
                                RowMax = RowsArray.GetUpperBound(1);
                                //
                                // -- setup IDFieldPointer
                                UcaseFieldName = "ID";
                                for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                    if (UcaseFieldName == genericController.vbUCase(RowFieldArray[ColumnPointer])) {
                                        IDFieldPointer = ColumnPointer;
                                        break;
                                    }
                                }
                                //
                                // setup DropDownFieldPointer()
                                //
                                for (var FieldPointer = 0; FieldPointer < DropDownFieldCount; FieldPointer++) {
                                    UcaseFieldName = genericController.vbUCase(DropDownFieldName[FieldPointer]);
                                    for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                        if (UcaseFieldName == genericController.vbUCase(RowFieldArray[ColumnPointer])) {
                                            DropDownFieldPointer[FieldPointer] = ColumnPointer;
                                            break;
                                        }
                                    }
                                }
                                //
                                // output select
                                //
                                SelectedFound = false;
                                for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                                    RecordID = genericController.EncodeInteger(RowsArray[IDFieldPointer, RowPointer]);
                                    Copy = DropDownPreField;
                                    for (var FieldPointer = 0; FieldPointer < DropDownFieldCount; FieldPointer++) {
                                        Copy = Copy + RowsArray[DropDownFieldPointer[FieldPointer], RowPointer] + DropDownDelimiter[FieldPointer];
                                    }
                                    if (string.IsNullOrEmpty(Copy)) {
                                        Copy = "no name";
                                    }
                                    FastString.Add(Environment.NewLine + "<option value=\"" + RecordID + "\" ");
                                    if (RecordID == CurrentValue) {
                                        FastString.Add("selected");
                                        SelectedFound = true;
                                    }
                                    if (cpCore.siteProperties.selectFieldWidthLimit != 0) {
                                        if (Copy.Length > cpCore.siteProperties.selectFieldWidthLimit) {
                                            Copy = Copy.Substring(0, cpCore.siteProperties.selectFieldWidthLimit) + "...+";
                                        }
                                    }
                                    FastString.Add(">" + encodeHTML(Copy) + "</option>");
                                }
                                if (!SelectedFound && (CurrentValue != 0)) {
                                    cpCore.db.csClose(ref CSPointer);
                                    if (!string.IsNullOrEmpty(Criteria)) {
                                        Criteria = Criteria + "and";
                                    }
                                    Criteria = Criteria + "(id=" + genericController.EncodeInteger(CurrentValue) + ")";
                                    CSPointer = cpCore.db.csOpen(ContentName, Criteria, SortFieldList, false,,,, SelectFields);
                                    if (cpCore.db.csOk(CSPointer)) {
                                        RowsArray = cpCore.db.cs_getRows(CSPointer);
                                        RowFieldArray = cpCore.db.cs_getSelectFieldList(CSPointer).Split(',');
                                        RowMax = RowsArray.GetUpperBound(1);
                                        ColumnMax = RowsArray.GetUpperBound(0);
                                        RecordID = genericController.EncodeInteger(RowsArray[IDFieldPointer, 0]);
                                        Copy = DropDownPreField;
                                        for (var FieldPointer = 0; FieldPointer < DropDownFieldCount; FieldPointer++) {
                                            Copy = Copy + RowsArray[DropDownFieldPointer[FieldPointer], 0] + DropDownDelimiter[FieldPointer];
                                        }
                                        if (string.IsNullOrEmpty(Copy)) {
                                            Copy = "no name";
                                        }
                                        FastString.Add(Environment.NewLine + "<option value=\"" + RecordID + "\" selected");
                                        SelectedFound = true;
                                        if (cpCore.siteProperties.selectFieldWidthLimit != 0) {
                                            if (Copy.Length > cpCore.siteProperties.selectFieldWidthLimit) {
                                                Copy = Copy.Substring(0, cpCore.siteProperties.selectFieldWidthLimit) + "...+";
                                            }
                                        }
                                        FastString.Add(">" + encodeHTML(Copy) + "</option>");
                                    }
                                }
                            }
                            FastString.Add("</select>");
                            cpCore.db.csClose(ref CSPointer);
                            SelectRaw = FastString.Text;
                        }
                    }
                    //
                    // Save the SelectRaw
                    //
                    if (!return_IsEmptyList) {
                        cpCore.doc.inputSelectCache.Add(new constants.main_InputSelectCacheType() {
                            ContentName = ContentName,
                            Criteria = Criteria,
                            CurrentValue = CurrentValue.ToString(),
                            SelectRaw = SelectRaw
                        });
                    }
                }
                //
                SelectRaw = genericController.vbReplace(SelectRaw, MenuNameFPO, MenuName);
                SelectRaw = genericController.vbReplace(SelectRaw, NoneCaptionFPO, NoneCaption);
                if (!string.IsNullOrEmpty(HtmlClass)) {
                    SelectRaw = genericController.vbReplace(SelectRaw, "<select ", "<select class=\"" + HtmlClass + "\"");
                }
                result = SelectRaw;
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        public string getInputMemberSelect(string MenuName, int CurrentValue, int GroupID, string ignore = "", string NoneCaption = "", string htmlId = "") {
            return html_GetFormInputMemberSelect2(MenuName, CurrentValue, GroupID, "", NoneCaption, htmlId);
        }
        //
        public string html_GetFormInputMemberSelect2(string MenuName, int CurrentValue, int GroupID, string ignore = "", string NoneCaption = "", string HtmlId = "", string HtmlClass = "") {
            string result = string.Empty;
            try {
                int LastRecordID = 0;
                string MemberRulesTableName = null;
                string iMenuName = null;
                int iCurrentValue = 0;
                string iNoneCaption = null;
                int CSPointer = 0;
                bool SelectedFound = false;
                int RecordID = 0;
                string Copy = null;
                string MethodName = null;
                string DropDownFieldList = null;
                string[] DropDownFieldName = { };
                string[] DropDownDelimiter = { };
                int DropDownFieldCount = 0;
                // converted array to dictionary - Dim FieldPointer As Integer
                string DropDownPreField = string.Empty;
                int DropDownFieldListLength = 0;
                string FieldName = string.Empty;
                string CharAllowed = null;
                string CharTest = null;
                int CharPointer = 0;
                int IDFieldPointer = 0;
                stringBuilderLegacyController FastString = new stringBuilderLegacyController();
                //
                string[,] RowsArray = null;
                string[] RowFieldArray = null;
                int RowMax = 0;
                int ColumnMax = 0;
                int RowPointer = 0;
                int ColumnPointer = 0;
                int[] DropDownFieldPointer = null;
                string UcaseFieldName = null;
                string SortFieldList = string.Empty;
                string SQL = null;
                string PeopleTableName = null;
                string PeopleDataSource = null;
                string iCriteria = string.Empty;
                string SelectFields = null;
                int Ptr = 0;
                string SelectRaw = string.Empty;
                int CachePtr = 0;
                string TagID = null;
                string TagClass = null;
                //
                const string MenuNameFPO = "<MenuName>";
                const string NoneCaptionFPO = "<NoneCaption>";
                //
                MethodName = "main_GetFormInputMemberSelect2";
                //
                iMenuName = genericController.encodeText(MenuName);
                iCurrentValue = genericController.EncodeInteger(CurrentValue);
                iNoneCaption = genericController.encodeEmptyText(NoneCaption, "Select One");
                //iCriteria = genericController.vbLCase(encodeMissingText(Criteria, ""))
                //
                foreach (constants.main_InputSelectCacheType tempVar in cpCore.doc.inputSelectCache) {
                    if ((tempVar.ContentName == "Group:" + GroupID) && (tempVar.Criteria == iCriteria) && (genericController.EncodeInteger(tempVar.CurrentValue) == iCurrentValue)) {
                        SelectRaw = tempVar.SelectRaw;
                        break;
                    }
                }
                //
                //
                //
                if (string.IsNullOrEmpty(SelectRaw)) {
                    //
                    // Build the SelectRaw
                    // Test selection size
                    //
                    PeopleTableName = Models.Complex.cdefModel.getContentTablename(cpCore, "people");
                    PeopleDataSource = Models.Complex.cdefModel.getContentDataSource(cpCore, "People");
                    MemberRulesTableName = Models.Complex.cdefModel.getContentTablename(cpCore, "Member Rules");
                    //
                    RowMax = 0;
                    SQL = "select count(*) as cnt"
                    + " from ccMemberRules R"
                    + " inner join ccMembers P on R.MemberID=P.ID"
                    + " where (P.active<>0)"
                    + " and (R.GroupID=" + GroupID + ")";
                    CSPointer = cpCore.db.csOpenSql_rev(PeopleDataSource, SQL);
                    if (cpCore.db.csOk(CSPointer)) {
                        RowMax = RowMax + cpCore.db.csGetInteger(CSPointer, "cnt");
                    }
                    cpCore.db.csClose(ref CSPointer);
                    //
                    //        SQL = " select count(*) as cnt" _
                    //            & " from ccMembers P" _
                    //            & " where (active<>0)" _
                    //            & " and(( P.admin<>0 )or( P.developer<>0 ))"
                    //        CSPointer = app.csv_OpenCSSQL(PeopleDataSource, SQL, memberID)
                    //        If app.csv_IsCSOK(CSPointer) Then
                    //            RowMax = RowMax + app.csv_cs_getInteger(CSPointer, "cnt")
                    //        End If
                    //        Call app.closeCS(CSPointer)
                    //
                    if (RowMax > cpCore.siteProperties.selectFieldLimit) {
                        //
                        // Selection is too big
                        //
                        cpCore.handleException(new Exception("While building a group members list for group [" + groupController.group_GetGroupName(cpCore, GroupID) + "], too many rows were selected. [" + RowMax + "] records exceeds [" + cpCore.siteProperties.selectFieldLimit + "], the current Site Property app.SiteProperty_SelectFieldLimit."));
                        result = result + html_GetFormInputHidden(MenuNameFPO, iCurrentValue);
                        if (iCurrentValue != 0) {
                            CSPointer = cpCore.db.csOpenRecord("people", iCurrentValue);
                            if (cpCore.db.csOk(CSPointer)) {
                                result = cpCore.db.csGetText(CSPointer, "name") + "&nbsp;";
                            }
                            cpCore.db.csClose(ref CSPointer);
                        }
                        result = result + "(Selection is too large to display)";
                    } else {
                        //
                        // ----- Generate Drop Down Field Names
                        //
                        DropDownFieldList = Models.Complex.cdefModel.GetContentProperty(cpCore, "people", "DropDownFieldList");
                        if (string.IsNullOrEmpty(DropDownFieldList)) {
                            DropDownFieldList = "NAME";
                        }
                        DropDownFieldCount = 0;
                        CharAllowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                        DropDownFieldListLength = DropDownFieldList.Length;
                        for (CharPointer = 1; CharPointer <= DropDownFieldListLength; CharPointer++) {
                            CharTest = DropDownFieldList.Substring(CharPointer - 1, 1);
                            if (genericController.vbInstr(1, CharAllowed, CharTest) == 0) {
                                //
                                // Character not allowed, delimit Field name here
                                //
                                if (!string.IsNullOrEmpty(FieldName)) {
                                    //
                                    // ----- main_Get new Field Name and save it
                                    //
                                    if (string.IsNullOrEmpty(SortFieldList)) {
                                        SortFieldList = FieldName;
                                    }
                                    Array.Resize(ref DropDownFieldName, DropDownFieldCount + 1);
                                    Array.Resize(ref DropDownDelimiter, DropDownFieldCount + 1);
                                    DropDownFieldName[DropDownFieldCount] = FieldName;
                                    DropDownDelimiter[DropDownFieldCount] = CharTest;
                                    DropDownFieldCount = DropDownFieldCount + 1;
                                    FieldName = "";
                                } else {
                                    //
                                    // ----- Save Field Delimiter
                                    //
                                    if (DropDownFieldCount == 0) {
                                        //
                                        // ----- Before any field, add to DropDownPreField
                                        //
                                        DropDownPreField = DropDownPreField + CharTest;
                                    } else {
                                        //
                                        // ----- after a field, add to last DropDownDelimiter
                                        //
                                        DropDownDelimiter[DropDownFieldCount - 1] = DropDownDelimiter[DropDownFieldCount - 1] + CharTest;
                                    }
                                }
                            } else {
                                //
                                // Character Allowed, Put character into fieldname and continue
                                //
                                FieldName = FieldName + CharTest;
                            }
                        }
                        if (!string.IsNullOrEmpty(FieldName)) {
                            if (string.IsNullOrEmpty(SortFieldList)) {
                                SortFieldList = FieldName;
                            }
                            Array.Resize(ref DropDownFieldName, DropDownFieldCount + 1);
                            Array.Resize(ref DropDownDelimiter, DropDownFieldCount + 1);
                            DropDownFieldName[DropDownFieldCount] = FieldName;
                            DropDownDelimiter[DropDownFieldCount] = "";
                            DropDownFieldCount = DropDownFieldCount + 1;
                        }
                        if (DropDownFieldCount == 0) {
                            cpCore.handleException(new Exception("No drop down field names found for content [" + GroupID + "]."));
                        } else {
                            DropDownFieldPointer = new int[DropDownFieldCount];
                            SelectFields = "P.ID";
                            for (Ptr = 0; Ptr < DropDownFieldCount; Ptr++) {
                                SelectFields = SelectFields + ",P." + DropDownFieldName[Ptr];
                            }
                            //
                            // ----- Start select box
                            //
                            TagClass = "";
                            if (genericController.encodeEmptyText(HtmlClass, "") != "") {
                                TagClass = " Class=\"" + genericController.encodeEmptyText(HtmlClass, "") + "\"";
                            }
                            //
                            TagID = "";
                            if (genericController.encodeEmptyText(HtmlId, "") != "") {
                                TagID = " ID=\"" + genericController.encodeEmptyText(HtmlId, "") + "\"";
                            }
                            //
                            FastString.Add("<select size=\"1\" name=\"" + MenuNameFPO + "\"" + TagID + TagClass + ">");
                            FastString.Add("<option value=\"\">" + NoneCaptionFPO + "</option>");
                            //
                            // ----- select values
                            //
                            if (string.IsNullOrEmpty(SortFieldList)) {
                                SortFieldList = "name";
                            }
                            SQL = "select " + SelectFields + " from ccMemberRules R"
                            + " inner join ccMembers P on R.MemberID=P.ID"
                            + " where (R.GroupID=" + GroupID + ")"
                            + " and((R.DateExpires is null)or(R.DateExpires>" + cpCore.db.encodeSQLDate(DateTime.Now) + "))"
                            + " and(P.active<>0)"
                            + " order by P." + SortFieldList;
                            //                SQL = "select " & SelectFields _
                            //                    & " from ccMemberRules R" _
                            //                    & " inner join ccMembers P on R.MemberID=P.ID" _
                            //                    & " where (R.GroupID=" & GroupID & ")" _
                            //                    & " and((R.DateExpires is null)or(R.DateExpires>" & encodeSQLDate(Now) & "))" _
                            //                    & " and(P.active<>0)" _
                            //                    & " union" _
                            //                    & " select P.ID,P.NAME" _
                            //                    & " from ccMembers P" _
                            //                    & " where (active<>0)" _
                            //                    & " and(( P.admin<>0 )or( P.developer<>0 ))" _
                            //                    & " order by P." & SortFieldList
                            CSPointer = cpCore.db.csOpenSql_rev(PeopleDataSource, SQL);
                            if (cpCore.db.csOk(CSPointer)) {
                                RowsArray = cpCore.db.cs_getRows(CSPointer);
                                //RowFieldArray = app.csv_cs_getRowFields(CSPointer)
                                RowFieldArray = cpCore.db.cs_getSelectFieldList(CSPointer).Split(',');
                                RowMax = RowsArray.GetUpperBound(1);
                                ColumnMax = RowsArray.GetUpperBound(0);
                                //
                                // setup IDFieldPointer
                                //
                                UcaseFieldName = "ID";
                                for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                    if (UcaseFieldName == genericController.vbUCase(RowFieldArray[ColumnPointer])) {
                                        IDFieldPointer = ColumnPointer;
                                        break;
                                    }
                                }
                                //
                                // setup DropDownFieldPointer()
                                //
                                for (var FieldPointer = 0; FieldPointer < DropDownFieldCount; FieldPointer++) {
                                    UcaseFieldName = genericController.vbUCase(DropDownFieldName[FieldPointer]);
                                    for (ColumnPointer = 0; ColumnPointer <= ColumnMax; ColumnPointer++) {
                                        if (UcaseFieldName == genericController.vbUCase(RowFieldArray[ColumnPointer])) {
                                            DropDownFieldPointer[FieldPointer] = ColumnPointer;
                                            break;
                                        }
                                    }
                                }
                                //
                                // output select
                                //
                                SelectedFound = false;
                                LastRecordID = -1;
                                for (RowPointer = 0; RowPointer <= RowMax; RowPointer++) {
                                    RecordID = genericController.EncodeInteger(RowsArray[IDFieldPointer, RowPointer]);
                                    if (RecordID != LastRecordID) {
                                        Copy = DropDownPreField;
                                        for (var FieldPointer = 0; FieldPointer < DropDownFieldCount; FieldPointer++) {
                                            Copy = Copy + RowsArray[DropDownFieldPointer[FieldPointer], RowPointer] + DropDownDelimiter[FieldPointer];
                                        }
                                        if (string.IsNullOrEmpty(Copy)) {
                                            Copy = "no name";
                                        }
                                        FastString.Add(Environment.NewLine + "<option value=\"" + RecordID + "\" ");
                                        if (RecordID == iCurrentValue) {
                                            FastString.Add("selected");
                                            SelectedFound = true;
                                        }
                                        if (cpCore.siteProperties.selectFieldWidthLimit != 0) {
                                            if (Copy.Length > cpCore.siteProperties.selectFieldWidthLimit) {
                                                Copy = Copy.Substring(0, cpCore.siteProperties.selectFieldWidthLimit) + "...+";
                                            }
                                        }
                                        FastString.Add(">" + Copy + "</option>");
                                        LastRecordID = RecordID;
                                    }
                                }
                            }
                            FastString.Add("</select>");
                            cpCore.db.csClose(ref CSPointer);
                            SelectRaw = FastString.Text;
                        }
                    }
                    //
                    // Save the SelectRaw
                    //
                    cpCore.doc.inputSelectCache.Add(new constants.main_InputSelectCacheType() {
                        ContentName = "Group:" + GroupID,
                        Criteria = iCriteria,
                        CurrentValue = iCurrentValue.ToString(),
                        SelectRaw = SelectRaw
                    });
                }
                //
                SelectRaw = genericController.vbReplace(SelectRaw, MenuNameFPO, iMenuName);
                SelectRaw = genericController.vbReplace(SelectRaw, NoneCaptionFPO, iNoneCaption);
                result = SelectRaw;
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        //   Legacy
        //========================================================================
        //
        public string getInputSelectList(string MenuName, string CurrentValue, string SelectList, string NoneCaption = "", string htmlId = "") {
            return getInputSelectList2(genericController.encodeText(MenuName), genericController.EncodeInteger(CurrentValue), genericController.encodeText(SelectList), genericController.encodeText(NoneCaption), genericController.encodeText(htmlId));
        }
        //
        //========================================================================
        //   Create a select list from a comma separated list
        //       returns an index into the list list, starting at 1
        //       if an element is blank (,) no option is created
        //========================================================================
        //
        public string getInputSelectList2(string MenuName, int CurrentValue, string SelectList, string NoneCaption, string htmlId, string HtmlClass = "") {
            try {
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
                if (SelectFieldWidthLimit == 0) {
                    SelectFieldWidthLimit = 256;
                }
                //
                //iSelectList = genericController.encodeText(SelectList)
                //
                // ----- Start select box
                //
                FastString.Add("<select id=\"" + htmlId + "\" class=\"" + HtmlClass + "\" size=\"1\" name=\"" + MenuName + "\">");
                if (!string.IsNullOrEmpty(NoneCaption)) {
                    FastString.Add("<option value=\"\">" + NoneCaption + "</option>");
                } else {
                    FastString.Add("<option value=\"\">Select One</option>");
                }
                //
                // ----- select values
                //
                lookups = SelectList.Split(',');
                for (Ptr = 0; Ptr <= lookups.GetUpperBound(0); Ptr++) {
                    RecordID = Ptr + 1;
                    Copy = lookups[Ptr];
                    if (!string.IsNullOrEmpty(Copy)) {
                        FastString.Add(Environment.NewLine + "<option value=\"" + RecordID + "\" ");
                        if (RecordID == CurrentValue) {
                            FastString.Add("selected");
                            //SelectedFound = True
                        }
                        if (Copy.Length > SelectFieldWidthLimit) {
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
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError13("main_GetFormInputSelectList2")
        }
        //
        //========================================================================
        //   Display an icon with a link to the login form/cclib.net/admin area
        //========================================================================
        //
        public string main_GetLoginLink() {
            string result = string.Empty;
            try {
                //
                //If Not (true) Then Exit Function
                //
                string Link = null;
                string IconFilename = null;
                //
                if (cpCore.siteProperties.getBoolean("AllowLoginIcon", true)) {
                    result = result + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">";
                    result = result + "<tr><td align=\"right\">";
                    if (cpCore.doc.authContext.isAuthenticatedContentManager(cpCore)) {
                        result = result + "<a href=\"" + encodeHTML("/" + cpCore.serverConfig.appConfig.adminRoute) + "\" target=\"_blank\">";
                    } else {
                        Link = cpCore.webServer.requestPage + "?" + cpCore.doc.refreshQueryString;
                        Link = genericController.modifyLinkQuery(Link, RequestNameHardCodedPage, HardCodedPageLogin, true);
                        //Link = genericController.modifyLinkQuery(Link, RequestNameInterceptpage, LegacyInterceptPageSNLogin, True)
                        result = result + "<a href=\"" + encodeHTML(Link) + "\" >";
                    }
                    IconFilename = cpCore.siteProperties.LoginIconFilename;
                    if (genericController.vbLCase(IconFilename.Substring(0, 7)) != "/ccLib/") {
                        IconFilename = genericController.getCdnFileLink(cpCore, IconFilename);
                    }
                    result = result + "<img alt=\"Login\" src=\"" + IconFilename + "\" border=\"0\" >";
                    result = result + "</A>";
                    result = result + "</td></tr></table>";
                }
            } catch (Exception ex) {
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
        ////ErrorTrap:
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
        ////ErrorTrap:
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
        public string html_GetLegacySiteStyles() {
            string temphtml_GetLegacySiteStyles = null;
            try {
                //
                if (!cpCore.doc.legacySiteStyles_Loaded) {
                    cpCore.doc.legacySiteStyles_Loaded = true;
                    //
                    // compatibility with old sites - if they do not main_Get the default style sheet, put it in here
                    //
                    if (false) {
                        temphtml_GetLegacySiteStyles = ""
                            + "\r" + "<!-- compatibility with legacy framework --><style type=text/css>"
                            + "\r" + " .ccEditWrapper {border-top:1px solid #6a6;border-left:1px solid #6a6;border-bottom:1px solid #cec;border-right:1px solid #cec;}"
                            + "\r" + " .ccEditWrapperInner {border-top:1px solid #cec;border-left:1px solid #cec;border-bottom:1px solid #6a6;border-right:1px solid #6a6;}"
                            + "\r" + " .ccEditWrapperCaption {text-align:left;border-bottom:1px solid #888;padding:4px;background-color:#40C040;color:black;}"
                            + "\r" + " .ccEditWrapperContent{padding:4px;}"
                            + "\r" + " .ccHintWrapper {border:1px dashed #888;margin-bottom:10px}"
                            + "\r" + " .ccHintWrapperContent{padding:10px;background-color:#80E080;color:black;}"
                            + "</style>";
                    } else {
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
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError13("main_GetLegacySiteStyles")
            return temphtml_GetLegacySiteStyles;
        }
        //
        //===================================================================================================
        //   Wrap the content in a common wrapper if authoring is enabled
        //===================================================================================================
        //
        public string html_GetAdminHintWrapper(string Content) {
            string temphtml_GetAdminHintWrapper = null;
            try {
                //
                //If Not (true) Then Exit Function
                //
                temphtml_GetAdminHintWrapper = "";
                if ((cpCore.doc.authContext.isEditing("") | cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) != 0) {
                    temphtml_GetAdminHintWrapper = temphtml_GetAdminHintWrapper + html_GetLegacySiteStyles();
                    temphtml_GetAdminHintWrapper = temphtml_GetAdminHintWrapper + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccHintWrapper\">"
                            + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccHintWrapperContent\">"
                            + "<b>Administrator</b>"
                            + "<br>"
                            + "<br>" + genericController.encodeText(Content) + "</td></tr></table>"
                        + "</td></tr></table>";
                }

                return temphtml_GetAdminHintWrapper;
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_GetAdminHintWrapper")
            return temphtml_GetAdminHintWrapper;
        }
        //
        //
        //
        public void enableOutputBuffer(bool BufferOn) {
            try {
                if (cpCore.doc.outputBufferEnabled) {
                    //
                    // ----- once on, can not be turned off Response Object
                    //
                    cpCore.doc.outputBufferEnabled = BufferOn;
                } else {
                    //
                    // ----- StreamBuffer off, allow on and off
                    //
                    cpCore.doc.outputBufferEnabled = BufferOn;
                }
            } catch (Exception ex) {
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
        public string html_GetUploadFormStart(string ActionQueryString = null) {
            try {
                if (ActionQueryString == null) {
                    ActionQueryString = cpCore.doc.refreshQueryString;
                }
                //
                string iActionQueryString;
                //
                iActionQueryString = genericController.ModifyQueryString(ActionQueryString, RequestNameRequestBinary, true, true);
                //
                return "<form action=\"" + cpCore.webServer.serverFormActionURL + "?" + iActionQueryString + "\" ENCTYPE=\"MULTIPART/FORM-DATA\" METHOD=\"POST\"  style=\"display: inline;\" >";
                //
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_GetUploadFormStart")
        }
        //
        //========================================================================
        // ----- Closes an HTML form for uploads
        //========================================================================
        //
        public string html_GetUploadFormEnd() {
            return html_GetFormEnd();
        }
        //
        //========================================================================
        // ----- Starts an HTML form
        //       Should be closed with PrintFormEnd
        //========================================================================
        //
        public string html_GetFormStart(string ActionQueryString = null, string htmlName = "", string htmlId = "", string htmlMethod = "") {
            string temphtml_GetFormStart = null;
            try {
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
                if (ActionQueryString == null) {
                    ActionQS = cpCore.doc.refreshQueryString;
                } else {
                    ActionQS = ActionQueryString;
                }
                iMethod = genericController.vbLCase(htmlMethod);
                if (string.IsNullOrEmpty(iMethod)) {
                    iMethod = "post";
                }
                RefreshHiddens = "";
                Action = cpCore.webServer.serverFormActionURL;
                //
                if (!string.IsNullOrEmpty(ActionQS)) {
                    if (iMethod != "main_Get") {
                        //
                        // non-main_Get, put Action QS on end of Action
                        //
                        Action = Action + "?" + ActionQS;
                    } else {
                        //
                        // main_Get method, build hiddens for actionQS
                        //
                        QSParts = ActionQS.Split('&');
                        for (Ptr = 0; Ptr <= QSParts.GetUpperBound(0); Ptr++) {
                            QSNameValues = QSParts[Ptr].Split('=');
                            if (QSNameValues.GetUpperBound(0) == 0) {
                                QSName = genericController.DecodeResponseVariable(QSNameValues[0]);
                            } else {
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
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18(MethodName)
                                                                    //
            return temphtml_GetFormStart;
        }
        //
        //========================================================================
        // ----- Ends an HTML form
        //========================================================================
        //
        public string html_GetFormEnd() {
            //
            return "</form>";
            //
        }
        //
        //
        //
        public string html_GetFormInputText(string TagName, string DefaultValue = "", string Height = "", string Width = "", string Id = "", bool PasswordField = false) {
            return html_GetFormInputText2(genericController.encodeText(TagName), genericController.encodeText(DefaultValue), genericController.EncodeInteger(Height), genericController.EncodeInteger(Width), genericController.encodeText(Id), PasswordField, false);
        }
        //
        //
        //
        public string html_GetFormInputText2(string htmlName, string DefaultValue = "", int Height = -1, int Width = -1, string HtmlId = "", bool PasswordField = false, bool Disabled = false, string HtmlClass = "") {
            string temphtml_GetFormInputText2 = null;
            try {
                //
                string iDefaultValue = null;
                int iWidth = 0;
                int iHeight = 0;
                string TagID = null;
                string TagDisabled = string.Empty;
                //
                if (true) {
                    TagID = "";
                    //
                    iDefaultValue = encodeHTML(DefaultValue);
                    if (!string.IsNullOrEmpty(HtmlId)) {
                        TagID = TagID + " id=\"" + genericController.encodeEmptyText(HtmlId, "") + "\"";
                    }
                    //
                    if (!string.IsNullOrEmpty(HtmlClass)) {
                        TagID = TagID + " class=\"" + HtmlClass + "\"";
                    }
                    //
                    iWidth = Width;
                    if (iWidth <= 0) {
                        iWidth = cpCore.siteProperties.defaultFormInputWidth;
                    }
                    //
                    iHeight = Height;
                    if (iHeight <= 0) {
                        iHeight = cpCore.siteProperties.defaultFormInputTextHeight;
                    }
                    //
                    if (Disabled) {
                        TagDisabled = " disabled=\"disabled\"";
                    }
                    //
                    if (PasswordField) {
                        temphtml_GetFormInputText2 = "<input TYPE=\"password\" NAME=\"" + htmlName + "\" SIZE=\"" + iWidth + "\" VALUE=\"" + iDefaultValue + "\"" + TagID + TagDisabled + ">";
                    } else if ((iHeight == 1) && (iDefaultValue.IndexOf("\"") + 1 == 0)) {
                        temphtml_GetFormInputText2 = "<input TYPE=\"Text\" NAME=\"" + htmlName + "\" SIZE=\"" + iWidth.ToString() + "\" VALUE=\"" + iDefaultValue + "\"" + TagID + TagDisabled + ">";
                    } else {
                        temphtml_GetFormInputText2 = "<textarea NAME=\"" + htmlName + "\" ROWS=\"" + iHeight.ToString() + "\" COLS=\"" + iWidth.ToString() + "\"" + TagID + TagDisabled + ">" + iDefaultValue + "</TEXTAREA>";
                    }
                    cpCore.doc.formInputTextCnt = cpCore.doc.formInputTextCnt + 1;
                }
                //
                return temphtml_GetFormInputText2;
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_GetFormInputText2")
            return temphtml_GetFormInputText2;
        }
        //
        //========================================================================
        // ----- main_Get an HTML Form text input (or text area)
        //========================================================================
        //
        public string html_GetFormInputTextExpandable(string TagName, string Value = "", int Rows = 0, string styleWidth = "100%", string Id = "", bool PasswordField = false) {
            if (Rows == 0) {
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
        public string html_GetFormInputTextExpandable2(string TagName, string Value = "", int Rows = 0, string styleWidth = "100%", string Id = "", bool PasswordField = false, bool Disabled = false, string HtmlClass = "") {
            string temphtml_GetFormInputTextExpandable2 = null;
            try {
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
                if (string.IsNullOrEmpty(IDRoot)) {
                    IDRoot = "TextArea" + cpCore.doc.formInputTextCnt;
                }
                //
                StyleWidth_Local = styleWidth;
                if (string.IsNullOrEmpty(StyleWidth_Local)) {
                    StyleWidth_Local = "100%";
                }
                //
                Rows_Local = Rows;
                if (Rows_Local == 0) {
                    //
                    // need a default for this -- it should be different from a text, it should be for a textarea -- bnecause it is used differently
                    //
                    //Rows_Local = app.SiteProperty_DefaultFormInputTextHeight
                    if (Rows_Local == 0) {
                        Rows_Local = 10;
                    }
                }
                if (Disabled) {
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
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            cpCore.handleException(new Exception("Unexpected exception"));
            //
            return temphtml_GetFormInputTextExpandable2;
        }
        //
        //
        //
        public string html_GetFormInputDate(string TagName, string DefaultValue = "", string Width = "", string Id = "") {
            string result = string.Empty;
            try {
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
                if ((iDefaultValue == "0") || (iDefaultValue == "12:00:00 AM")) {
                    iDefaultValue = "";
                } else {
                    iDefaultValue = encodeHTML(iDefaultValue);
                }
                if (genericController.encodeEmptyText(Id, "") != "") {
                    TagID = " ID=\"" + genericController.encodeEmptyText(Id, "") + "\"";
                }
                //
                iWidth = genericController.encodeEmptyInteger(Width, 20);
                if (iWidth == 0) {
                    iWidth = 20;
                }
                //
                CalendarObjName = "Cal" + cpCore.doc.inputDateCnt;
                AnchorName = "ACal" + cpCore.doc.inputDateCnt;

                if (cpCore.doc.inputDateCnt == 0) {
                    HeadJS = ""
                    + Environment.NewLine + "<SCRIPT LANGUAGE=\"JavaScript\" SRC=\"/ccLib/mktree/CalendarPopup.js\"></SCRIPT>"
                    + Environment.NewLine + "<SCRIPT LANGUAGE=\"JavaScript\">"
                    + Environment.NewLine + "var cal = new CalendarPopup();"
                    + Environment.NewLine + "cal.showNavigationDropdowns();"
                    + Environment.NewLine + "</SCRIPT>";
                    addScriptLink_Head("/ccLib/mktree/CalendarPopup.js", "Calendar Popup");
                    addScriptCode_head("var cal=new CalendarPopup();cal.showNavigationDropdowns();", "Calendar Popup");
                }

                if (DateHelper.IsDate(iDefaultValue)) {
                    DateValue = genericController.EncodeDate(iDefaultValue);
                    if (DateValue.Month < 10) {
                        DateString = DateString + "0";
                    }
                    DateString = DateString + DateValue.Month + "/";
                    if (DateValue.Day < 10) {
                        DateString = DateString + "0";
                    }
                    DateString = DateString + DateValue.Day + "/" + DateValue.Year;
                }


                result = result + Environment.NewLine + "<input TYPE=\"text\" NAME=\"" + iTagName + "\" ID=\"" + iTagName + "\" VALUE=\"" + iDefaultValue + "\"  SIZE=\"" + iWidth + "\">"
                + Environment.NewLine + "<a HREF=\"#\" Onclick = \"cal.select(document.getElementById('" + iTagName + "'),'" + AnchorName + "','MM/dd/yyyy','" + DateString + "'); return false;\" NAME=\"" + AnchorName + "\" ID=\"" + AnchorName + "\"><img title=\"Select a date\" alt=\"Select a date\" src=\"/ccLib/images/table.jpg\" width=12 height=10 border=0></A>"
                + Environment.NewLine + "";

                cpCore.doc.inputDateCnt = cpCore.doc.inputDateCnt + 1;
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        // ----- main_Get an HTML Form file upload input
        //========================================================================
        //
        public string html_GetFormInputFile2(string TagName, string htmlId = "", string HtmlClass = "") {
            //
            return "<input TYPE=\"file\" name=\"" + TagName + "\" id=\"" + htmlId + "\" class=\"" + HtmlClass + "\">";
            //
        }
        //
        // ----- main_Get an HTML Form file upload input
        //
        public string html_GetFormInputFile(string TagName, string htmlId = "") {
            //
            return html_GetFormInputFile2(TagName, htmlId);
            //
        }
        //
        //========================================================================
        // ----- main_Get an HTML Form input
        //========================================================================
        //
        public string html_GetFormInputRadioBox(string TagName, string TagValue, string CurrentValue, string htmlId = "") {
            string temphtml_GetFormInputRadioBox = null;
            try {
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
                if (!string.IsNullOrEmpty(ihtmlId)) {
                    TagID = " ID=\"" + ihtmlId + "\"";
                }
                //
                MethodName = "main_GetFormInputRadioBox";
                //
                if (iTagValue == iCurrentValue) {
                    temphtml_GetFormInputRadioBox = "<input TYPE=\"Radio\" NAME=\"" + iTagName + "\" VALUE=\"" + iTagValue + "\" checked" + TagID + ">";
                } else {
                    temphtml_GetFormInputRadioBox = "<input TYPE=\"Radio\" NAME=\"" + iTagName + "\" VALUE=\"" + iTagValue + "\"" + TagID + ">";
                }
                //
                return temphtml_GetFormInputRadioBox;
                //
                // ----- Error Trap
                //
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18(MethodName)
                                                                    //
            return temphtml_GetFormInputRadioBox;
        }
        //
        //========================================================================
        //   Legacy
        //========================================================================
        //
        public string html_GetFormInputCheckBox(string TagName, string DefaultValue = "", string htmlId = "") {
            return html_GetFormInputCheckBox2(genericController.encodeText(TagName), genericController.EncodeBoolean(DefaultValue), genericController.encodeText(htmlId));
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        public string html_GetFormInputCheckBox2(string TagName, bool DefaultValue = false, string HtmlId = "", bool Disabled = false, string HtmlClass = "") {
            string temphtml_GetFormInputCheckBox2 = null;
            try {
                //
                temphtml_GetFormInputCheckBox2 = "<input TYPE=\"CheckBox\" NAME=\"" + TagName + "\" VALUE=\"1\"";
                if (!string.IsNullOrEmpty(HtmlId)) {
                    temphtml_GetFormInputCheckBox2 = temphtml_GetFormInputCheckBox2 + " id=\"" + HtmlId + "\"";
                }
                if (!string.IsNullOrEmpty(HtmlClass)) {
                    temphtml_GetFormInputCheckBox2 = temphtml_GetFormInputCheckBox2 + " class=\"" + HtmlClass + "\"";
                }
                if (DefaultValue) {
                    temphtml_GetFormInputCheckBox2 = temphtml_GetFormInputCheckBox2 + " checked=\"checked\"";
                }
                if (Disabled) {
                    temphtml_GetFormInputCheckBox2 = temphtml_GetFormInputCheckBox2 + " disabled=\"disabled\"";
                }
                return temphtml_GetFormInputCheckBox2 + ">";
                //
                //
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
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
        public string html_GetFormInputCheckListByIDList(string TagName, string SecondaryContentName, string CheckedIDList, string CaptionFieldName = "", bool readOnlyField = false) {
            try {
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
                if (cpCore.db.csOk(CS)) {
                    SectionName = "";
                    GroupCount = 0;
                    CanSeeHiddenFields = cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore);
                    while (cpCore.db.csOk(CS)) {
                        GroupName = cpCore.db.csGetText(CS, "GroupName");
                        if ((GroupName.Substring(0, 1) != "_") || CanSeeHiddenFields) {
                            RecordID = cpCore.db.csGetInteger(CS, "ID");
                            GroupCaption = cpCore.db.csGetText(CS, "GroupCaption");
                            if (string.IsNullOrEmpty(GroupCaption)) {
                                GroupCaption = GroupName;
                            }
                            if (string.IsNullOrEmpty(GroupCaption)) {
                                GroupCaption = SingularPrefix + RecordID;
                            }
                            if (GroupCount != 0) {
                                // leave this between checkboxes - it is searched in the admin page
                                Result = Result + "<br >" + Environment.NewLine;
                            }
                            if (genericController.IsInDelimitedString(CheckedIDList, RecordID.ToString(), ",")) {
                                Found = true;
                            } else {
                                Found = false;
                            }
                            // must leave the first hidden with the value in this form - it is searched in the admin pge
                            Result = Result + "<input type=hidden name=\"" + TagName + "." + GroupCount + ".ID\" value=" + RecordID + ">";
                            if (readOnlyField && !Found) {
                                Result = Result + "<input type=checkbox disabled>";
                            } else if (readOnlyField) {
                                Result = Result + "<input type=checkbox disabled checked>";
                                Result = Result + "<input type=\"hidden\" name=\"" + TagName + "." + GroupCount + ".ID\" value=" + RecordID + ">";
                            } else if (Found) {
                                Result = Result + "<input type=checkbox name=\"" + TagName + "." + GroupCount + "\" checked>";
                            } else {
                                Result = Result + "<input type=checkbox name=\"" + TagName + "." + GroupCount + "\">";
                            }
                            Result = Result + SpanClassAdminNormal + GroupCaption;
                            GroupCount = GroupCount + 1;
                        }
                        cpCore.db.csGoNext(CS);
                    }
                    Result = Result + "<input type=\"hidden\" name=\"" + TagName + ".RowCount\" value=\"" + GroupCount + "\">" + Environment.NewLine;
                }
                cpCore.db.csClose(ref CS);
                return Result;
                //
                // ----- Error Trap
                //
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18(MethodName)
                                                                    //
        }
        //
        // -----
        //
        public string html_GetFormInputCS(int CSPointer, string ContentName, string FieldName, int Height = 1, int Width = 40, string htmlId = "") {
            string returnResult = string.Empty;
            try {
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
                if (true) {
                    fieldFound = false;
                    Contentdefinition = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
                    foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in Contentdefinition.fields) {
                        Models.Complex.CDefFieldModel field = keyValuePair.Value;
                        if (genericController.vbUCase(field.nameLc) == genericController.vbUCase(FieldName)) {
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
                    if (!fieldFound) {
                        cpCore.handleException(new Exception("Field [" + FieldName + "] was not found in Content Definition [" + ContentName + "]"));
                    } else {
                        //
                        // main_Get the current value if the record was found
                        //
                        if (cpCore.db.csOk(CSPointer)) {
                            FieldValueVariant = cpCore.db.cs_getValue(CSPointer, FieldName);
                        }
                        //
                        if (FieldPassword) {
                            //
                            // Handle Password Fields
                            //
                            FieldValueText = genericController.encodeText(FieldValueVariant);
                            returnResult = html_GetFormInputText2(FieldName, FieldValueText, Height, Width, "", true);
                        } else {
                            //
                            // Non Password field by fieldtype
                            //
                            switch (fieldTypeId) {
                                //
                                //
                                //
                                case FieldTypeIdHTML:
                                    FieldValueText = genericController.encodeText(FieldValueVariant);
                                    if (FieldReadOnly) {
                                        returnResult = FieldValueText;
                                    } else {
                                        returnResult = getFormInputHTML(FieldName, FieldValueText,, Width.ToString());
                                    }
                                    //
                                    // html files, read from cdnFiles and use html editor
                                    //
                                    break;
                                case FieldTypeIdFileHTML:
                                    FieldValueText = genericController.encodeText(FieldValueVariant);
                                    if (!string.IsNullOrEmpty(FieldValueText)) {
                                        FieldValueText = cpCore.cdnFiles.readFile(FieldValueText);
                                    }
                                    if (FieldReadOnly) {
                                        returnResult = FieldValueText;
                                    } else {
                                        //Height = encodeEmptyInteger(Height, 4)
                                        returnResult = getFormInputHTML(FieldName, FieldValueText,, Width.ToString());
                                    }
                                    //
                                    // text cdnFiles files, read from cdnFiles and use text editor
                                    //
                                    break;
                                case FieldTypeIdFileText:
                                    FieldValueText = genericController.encodeText(FieldValueVariant);
                                    if (!string.IsNullOrEmpty(FieldValueText)) {
                                        FieldValueText = cpCore.cdnFiles.readFile(FieldValueText);
                                    }
                                    if (FieldReadOnly) {
                                        returnResult = FieldValueText;
                                    } else {
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
                                    if (!string.IsNullOrEmpty(FieldValueText)) {
                                        FieldValueText = cpCore.cdnFiles.readFile(FieldValueText);
                                    }
                                    if (FieldReadOnly) {
                                        returnResult = FieldValueText;
                                    } else {
                                        //Height = encodeEmptyInteger(Height, 4)
                                        returnResult = html_GetFormInputText2(FieldName, FieldValueText, Height, Width);
                                    }
                                    //
                                    //
                                    //
                                    break;
                                case FieldTypeIdBoolean:
                                    if (FieldReadOnly) {
                                        returnResult = genericController.encodeText(genericController.EncodeBoolean(FieldValueVariant));
                                    } else {
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
                                    if (FieldReadOnly) {
                                        returnResult = genericController.encodeText(FieldValueVariant);
                                    } else {
                                        returnResult = html_GetFormInputText2(FieldName, genericController.encodeText(FieldValueVariant), Height, Width);
                                    }
                                    //
                                    //
                                    //
                                    break;
                                case FieldTypeIdFile:
                                    FieldValueText = genericController.encodeText(FieldValueVariant);
                                    if (FieldReadOnly) {
                                        returnResult = FieldValueText;
                                    } else {
                                        returnResult = FieldValueText + "<BR >change: " + html_GetFormInputFile(FieldName, genericController.encodeText(FieldValueVariant));
                                    }
                                    //
                                    //
                                    //
                                    break;
                                case FieldTypeIdFileImage:
                                    FieldValueText = genericController.encodeText(FieldValueVariant);
                                    if (FieldReadOnly) {
                                        returnResult = FieldValueText;
                                    } else {
                                        returnResult = "<img src=\"" + genericController.getCdnFileLink(cpCore, FieldValueText) + "\"><BR >change: " + html_GetFormInputFile(FieldName, genericController.encodeText(FieldValueVariant));
                                    }
                                    //
                                    //
                                    //
                                    break;
                                case FieldTypeIdLookup:
                                    FieldValueInteger = genericController.EncodeInteger(FieldValueVariant);
                                    FieldLookupContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, FieldLookupContentID);
                                    if (!string.IsNullOrEmpty(FieldLookupContentName)) {
                                        //
                                        // Lookup into Content
                                        //
                                        if (FieldReadOnly) {
                                            CSPointer = cpCore.db.cs_open2(FieldLookupContentName, FieldValueInteger);
                                            if (cpCore.db.csOk(CSLookup)) {
                                                returnResult = csController.getTextEncoded(cpCore, CSLookup, "name");
                                            }
                                            cpCore.db.csClose(ref CSLookup);
                                        } else {
                                            returnResult = main_GetFormInputSelect2(FieldName, FieldValueInteger, FieldLookupContentName, "", "", "", ref IsEmptyList);
                                        }
                                    } else if (!string.IsNullOrEmpty(FieldLookupList)) {
                                        //
                                        // Lookup into LookupList
                                        //
                                        returnResult = getInputSelectList2(FieldName, FieldValueInteger, FieldLookupList, "", "");
                                    } else {
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
                                    if (FieldReadOnly) {
                                        returnResult = FieldValueText;
                                    } else {
                                        if (FieldHTMLContent) {
                                            returnResult = getFormInputHTML(FieldName, FieldValueText, Height.ToString(), Width.ToString(), FieldReadOnly, false);
                                            //main_GetFormInputCS = main_GetFormInputActiveContent(fieldname, FieldValueText, height, width)
                                        } else {
                                            returnResult = html_GetFormInputText2(FieldName, FieldValueText, Height, Width);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
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
        public string html_GetFormButton(string ButtonLabel, string Name = "", string htmlId = "", string OnClick = "") {
            return html_GetFormButton2(ButtonLabel, Name, htmlId, OnClick, false);
        }
        //
        //========================================================================
        // ----- Print an HTML Form Button element named BUTTON
        //========================================================================
        //
        public string html_GetFormButton2(string ButtonLabel, string Name = "button", string htmlId = "", string OnClick = "", bool Disabled = false) {
            try {
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
                if (Disabled) {
                    s = s + " disabled=\"disabled\"";
                }
                return s + ">";
                //
                // ----- Error Trap
                //
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18(MethodName)
                                                                    //
        }
        //
        //========================================================================
        // main_Gets a value in a hidden form field
        //   Handles name and value encoding
        //========================================================================
        //
        public string html_GetFormInputHidden(string TagName, string TagValue, string htmlId = "") {
            try {
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
                if (!string.IsNullOrEmpty(iTagValue)) {
                    s = s + " VALUE=\"" + iTagValue + "\"";
                }
                //
                ihtmlId = genericController.encodeText(htmlId);
                if (!string.IsNullOrEmpty(ihtmlId)) {
                    s = s + " ID=\"" + encodeHTML(ihtmlId) + "\"";
                }
                //
                s = s + ">";
                //
                return s;
                //
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_GetFormInputHidden")
        }
        //
        public string html_GetFormInputHidden(string TagName, bool TagValue, string htmlId = "") {
            return html_GetFormInputHidden(TagName, TagValue.ToString(), htmlId);
        }
        //
        public string html_GetFormInputHidden(string TagName, int TagValue, string htmlId = "") {
            return html_GetFormInputHidden(TagName, TagValue.ToString(), htmlId);
        }
        //
        // Popup a separate window with the contents of a file
        //
        public string html_GetWindowOpenJScript(string URI, string WindowWidth = "", string WindowHeight = "", string WindowScrollBars = "", bool WindowResizable = true, string WindowName = "_blank") {
            string temphtml_GetWindowOpenJScript = null;
            try {
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
                if (!genericController.isMissing(WindowWidth)) {
                    if (!string.IsNullOrEmpty(WindowWidth)) {
                        temphtml_GetWindowOpenJScript = temphtml_GetWindowOpenJScript + Delimiter + "width=" + WindowWidth;
                        Delimiter = ",";
                    }
                }
                if (!genericController.isMissing(WindowHeight)) {
                    if (!string.IsNullOrEmpty(WindowHeight)) {
                        temphtml_GetWindowOpenJScript = temphtml_GetWindowOpenJScript + Delimiter + "height=" + WindowHeight;
                        Delimiter = ",";
                    }
                }
                if (!genericController.isMissing(WindowScrollBars)) {
                    if (!string.IsNullOrEmpty(WindowScrollBars)) {
                        temphtml_GetWindowOpenJScript = temphtml_GetWindowOpenJScript + Delimiter + "scrollbars=" + WindowScrollBars;
                        Delimiter = ",";
                    }
                }
                if (WindowResizable) {
                    temphtml_GetWindowOpenJScript = temphtml_GetWindowOpenJScript + Delimiter + "resizable";
                    Delimiter = ",";
                }
                return temphtml_GetWindowOpenJScript + "')";
                //
                // ----- Error Trap
                //
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18(MethodName)
                                                                    //
            return temphtml_GetWindowOpenJScript;
        }
        //
        // Popup a separate window with the contents of a file
        //
        public string html_GetWindowDialogJScript(string URI, string WindowWidth = "", string WindowHeight = "", bool WindowScrollBars = false, bool WindowResizable = false, string WindowName = "") {
            string temphtml_GetWindowDialogJScript = null;
            try {
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
                if (!genericController.isMissing(WindowWidth)) {
                    if (!string.IsNullOrEmpty(WindowWidth)) {
                        temphtml_GetWindowDialogJScript = temphtml_GetWindowDialogJScript + ";dialogWidth:" + WindowWidth + "px";
                    }
                }
                if (!genericController.isMissing(WindowHeight)) {
                    if (!string.IsNullOrEmpty(WindowHeight)) {
                        temphtml_GetWindowDialogJScript = temphtml_GetWindowDialogJScript + ";dialogHeight:" + WindowHeight + "px";
                    }
                }
                if (WindowScrollBars) {
                    temphtml_GetWindowDialogJScript = temphtml_GetWindowDialogJScript + ";scroll:yes";
                }
                if (WindowResizable) {
                    temphtml_GetWindowDialogJScript = temphtml_GetWindowDialogJScript + ";resizable:yes";
                }
                return temphtml_GetWindowDialogJScript + "')";
                //
                // ----- Error Trap
                //
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18(MethodName)
                                                                    //
            return temphtml_GetWindowDialogJScript;
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        public void html_AddEvent(string HtmlId, string DOMEvent, string Javascript) {
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
        public string html_GetFormInputField(string ContentName, string FieldName, string htmlName = "", string HtmlValue = "", string HtmlClass = "", string HtmlId = "", string HtmlStyle = "", int ManyToManySourceRecordID = 0) {
            string result = string.Empty;
            try {
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
                if (string.IsNullOrEmpty(InputName)) {
                    InputName = FieldName;
                }
                //
                fieldType = genericController.EncodeInteger(Models.Complex.cdefModel.GetContentFieldProperty(cpCore, ContentName, FieldName, "type"));
                switch (fieldType) {
                    case FieldTypeIdBoolean: {
                            //
                            //
                            //
                            result = html_GetFormInputCheckBox2(InputName, genericController.EncodeBoolean(HtmlValue) == true, HtmlId, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdFileCSS: {
                            //
                            //
                            //
                            result = html_GetFormInputTextExpandable2(InputName, HtmlValue, 0, "100%", HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdCurrency: {
                            //
                            //
                            //
                            result = html_GetFormInputText2(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdDate: {
                            //
                            //
                            //
                            result = html_GetFormInputDate(InputName, HtmlValue, "", HtmlId);
                            if (!string.IsNullOrEmpty(HtmlClass)) {
                                result = genericController.vbReplace(result, ">", " class=\"" + HtmlClass + "\">");
                            }
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdFile: {
                            //
                            //
                            //
                            if (string.IsNullOrEmpty(HtmlValue)) {
                                result = html_GetFormInputFile2(InputName, HtmlId, HtmlClass);
                            } else {

                                string FieldValuefilename = "";
                                string FieldValuePath = "";
                                cpCore.cdnFiles.splitPathFilename(HtmlValue, ref FieldValuePath, ref FieldValuefilename);
                                result = result + "<a href=\"http://" + genericController.EncodeURL(cpCore.webServer.requestDomain + genericController.getCdnFileLink(cpCore, HtmlValue)) + "\" target=\"_blank\">" + SpanClassAdminSmall + "[" + FieldValuefilename + "]</A>";
                                result = result + "&nbsp;&nbsp;&nbsp;Delete:&nbsp;" + html_GetFormInputCheckBox2(InputName + ".Delete", false);
                                result = result + "&nbsp;&nbsp;&nbsp;Change:&nbsp;" + html_GetFormInputFile2(InputName, HtmlId, HtmlClass);
                            }
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdFloat: {
                            //
                            //
                            //
                            result = html_GetFormInputText2(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdFileImage: {
                            //
                            //
                            //
                            if (string.IsNullOrEmpty(HtmlValue)) {
                                result = html_GetFormInputFile2(InputName, HtmlId, HtmlClass);
                            } else {
                                string FieldValuefilename = "";
                                string FieldValuePath = "";
                                cpCore.cdnFiles.splitPathFilename(HtmlValue, ref FieldValuePath, ref FieldValuefilename);
                                result = result + "<a href=\"http://" + genericController.EncodeURL(cpCore.webServer.requestDomain + genericController.getCdnFileLink(cpCore, HtmlValue)) + "\" target=\"_blank\">" + SpanClassAdminSmall + "[" + FieldValuefilename + "]</A>";
                                result = result + "&nbsp;&nbsp;&nbsp;Delete:&nbsp;" + html_GetFormInputCheckBox2(InputName + ".Delete", false);
                                result = result + "&nbsp;&nbsp;&nbsp;Change:&nbsp;" + html_GetFormInputFile2(InputName, HtmlId, HtmlClass);
                            }
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdInteger: {
                            //
                            //
                            //
                            result = html_GetFormInputText2(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdFileJavascript: {
                            //
                            //
                            //
                            result = html_GetFormInputTextExpandable2(InputName, HtmlValue, 0, "100%", HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdLink: {
                            //
                            //
                            //
                            result = html_GetFormInputText2(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdLookup: {
                            //
                            //
                            //
                            CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
                            LookupContentName = "";
                            foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in CDef.fields) {
                                Models.Complex.CDefFieldModel field = keyValuePair.Value;
                                if (genericController.vbUCase(field.nameLc) == genericController.vbUCase(FieldName)) {
                                    if (field.lookupContentID != 0) {
                                        LookupContentName = genericController.encodeText(Models.Complex.cdefModel.getContentNameByID(cpCore, field.lookupContentID));
                                    }
                                    if (!string.IsNullOrEmpty(LookupContentName)) {
                                        result = main_GetFormInputSelect2(InputName, genericController.EncodeInteger(HtmlValue), LookupContentName, "", "Select One", HtmlId, ref IgnoreBoolean, HtmlClass);
                                    } else if (field.lookupList != "") {
                                        result = getInputSelectList2(InputName, genericController.EncodeInteger(HtmlValue), field.lookupList, "Select One", HtmlId, HtmlClass);
                                    }
                                    if (!string.IsNullOrEmpty(HtmlStyle)) {
                                        result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    case FieldTypeIdManyToMany: {
                            //
                            //
                            //
                            CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
                            var tempVar = CDef.fields[FieldName.ToLower()];
                            MTMContent0 = Models.Complex.cdefModel.getContentNameByID(cpCore, tempVar.contentId);
                            MTMContent1 = Models.Complex.cdefModel.getContentNameByID(cpCore, tempVar.manyToManyContentID);
                            MTMRuleContent = Models.Complex.cdefModel.getContentNameByID(cpCore, tempVar.manyToManyRuleContentID);
                            MTMRuleField0 = tempVar.ManyToManyRulePrimaryField;
                            MTMRuleField1 = tempVar.ManyToManyRuleSecondaryField;
                            result = getCheckList(InputName, MTMContent0, ManyToManySourceRecordID, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1,,, false);
                            //result = getInputCheckListCategories(InputName, MTMContent0, ManyToManySourceRecordID, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, , , False, MTMContent1, HtmlValue)
                            break;
                        }
                    case FieldTypeIdMemberSelect: {
                            //
                            //
                            //
                            GroupID = genericController.EncodeInteger(Models.Complex.cdefModel.GetContentFieldProperty(cpCore, ContentName, FieldName, "memberselectgroupid"));
                            result = getInputMemberSelect(InputName, genericController.EncodeInteger(HtmlValue), GroupID,,, HtmlId);
                            if (!string.IsNullOrEmpty(HtmlClass)) {
                                result = genericController.vbReplace(result, ">", " class=\"" + HtmlClass + "\">");
                            }
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdResourceLink: {
                            //
                            //
                            //
                            result = html_GetFormInputText2(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdText: {
                            //
                            //
                            //
                            result = html_GetFormInputText2(InputName, HtmlValue, -1, -1, HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdLongText:
                    case FieldTypeIdFileText: {
                            //
                            //
                            //
                            result = html_GetFormInputTextExpandable2(InputName, HtmlValue, 0, "100%", HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdFileXML: {
                            //
                            //
                            //
                            result = html_GetFormInputTextExpandable2(InputName, HtmlValue, 0, "100%", HtmlId, false, false, HtmlClass);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            break;
                        }
                    case FieldTypeIdHTML:
                    case FieldTypeIdFileHTML: {
                            //
                            //
                            //
                            result = getFormInputHTML(InputName, HtmlValue);
                            if (!string.IsNullOrEmpty(HtmlStyle)) {
                                result = genericController.vbReplace(result, ">", " style=\"" + HtmlStyle + "\">");
                            }
                            if (!string.IsNullOrEmpty(HtmlClass)) {
                                result = genericController.vbReplace(result, ">", " class=\"" + HtmlClass + "\">");
                            }
                            break;
                        }
                    default: {
                            //
                            // unsupported field type
                            //
                            break;
                        }
                }
            } catch (Exception ex) {
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
        ////ErrorTrap:
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
        public string main_GetEditorAddonListJSON(csv_contentTypeEnum ContentType) {
            string result = string.Empty;
            try {
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
                if (string.IsNullOrEmpty(result)) {
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
                    if ((ContentType == csv_contentTypeEnum.contentTypeEmail) || (ContentType == csv_contentTypeEnum.contentTypeEmailTemplate)) {
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
                        if (ContentType == csv_contentTypeEnum.contentTypeEmailTemplate) {
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
                    } else {
                        //
                        // ----- Web Only AC Tags
                        //
                        // Watch Lists
                        //
                        CSLists = cpCore.db.csOpen("Content Watch Lists",, "Name,ID",,,,, "Name,ID", 20, 1);
                        if (cpCore.db.csOk(CSLists)) {
                            while (cpCore.db.csOk(CSLists)) {
                                FieldName = Convert.ToString(cpCore.db.csGetText(CSLists, "name")).Trim(' ');
                                if (!string.IsNullOrEmpty(FieldName)) {
                                    FieldCaption = "Watch List [" + FieldName + "]";
                                    IconIDControlString = "AC,WATCHLIST,0," + FieldName + ",ListName=" + FieldName + "&SortField=[DateAdded|Link|LinkLabel|Clicks|WhatsNewDateExpires]&SortDirection=Z-A[A-Z|Z-A]";
                                    IconImg = genericController.GetAddonIconImg("/" + cpCore.serverConfig.appConfig.adminRoute, 0, 0, 0, true, IconIDControlString, "", cpCore.serverConfig.appConfig.cdnFilesNetprefix, FieldCaption, "Rendered as the " + FieldCaption, "", 0);
                                    IconImg = genericController.EncodeJavascript(IconImg);
                                    FieldCaption = genericController.EncodeJavascript(FieldCaption);
                                    Items[ItemsCnt] = "['" + FieldCaption + "','" + IconImg + "']";
                                    //Items(ItemsCnt) = "['" & FieldCaption & "','<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the " & FieldCaption & """ id=""AC,WATCHLIST,0," & FieldName & ",ListName=" & FieldName & "&SortField=[DateAdded|Link|LinkLabel|Clicks|WhatsNewDateExpires]&SortDirection=Z-A[A-Z|Z-A]"" src=""/ccLib/images/ACWatchList.GIF"">']"
                                    Index.setPtr(FieldCaption, ItemsCnt);
                                    ItemsCnt = ItemsCnt + 1;
                                    if (ItemsCnt >= ItemsSize) {
                                        ItemsSize = ItemsSize + 100;
                                        Array.Resize(ref Items, ItemsSize + 1);
                                    }
                                }
                                cpCore.db.csGoNext(CSLists);
                            }
                        }
                        cpCore.db.csClose(ref CSLists);
                    }
                    //
                    // ----- Add-ons (AC Aggregate Functions)
                    //
                    if ((false) && (ContentType == csv_contentTypeEnum.contentTypeEmail)) {
                        //
                        // Email did not support add-ons
                        //
                    } else {
                        //
                        // Either non-email or > 4.0.325
                        //
                        Criteria = "(1=1)";
                        if (ContentType == csv_contentTypeEnum.contentTypeEmail) {
                            //
                            // select only addons with email placement (dont need to check main_version bc if email, must be >4.0.325
                            //
                            Criteria = Criteria + "and(email<>0)";
                        } else {
                            if (true) {
                                if (ContentType == csv_contentTypeEnum.contentTypeWeb) {
                                    //
                                    // Non Templates
                                    //
                                    Criteria = Criteria + "and(content<>0)";
                                } else {
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
                        if (cpCore.db.csOk(CSAddons)) {
                            while (cpCore.db.csOk(CSAddons)) {
                                AddonGuid = cpCore.db.csGetText(CSAddons, "ccguid");
                                ObjectProgramID2 = cpCore.db.csGetText(CSAddons, "ObjectProgramID");
                                if ((ContentType == csv_contentTypeEnum.contentTypeEmail) && (!string.IsNullOrEmpty(ObjectProgramID2))) {
                                    //
                                    // Block activex addons from email
                                    //
                                    ObjectProgramID2 = ObjectProgramID2;
                                } else {
                                    AddonName = Convert.ToString(cpCore.db.csGet(CSAddons, "name")).Trim(' ');
                                    if (!string.IsNullOrEmpty(AddonName) & (AddonName != LastAddonName)) {
                                        //
                                        // Icon (fieldtyperesourcelink)
                                        //
                                        IsInline = cpCore.db.csGetBoolean(CSAddons, "IsInline");
                                        IconFilename = cpCore.db.csGet(CSAddons, "Iconfilename");
                                        if (string.IsNullOrEmpty(IconFilename)) {
                                            IconWidth = 0;
                                            IconHeight = 0;
                                            IconSprites = 0;
                                        } else {
                                            IconWidth = cpCore.db.csGetInteger(CSAddons, "IconWidth");
                                            IconHeight = cpCore.db.csGetInteger(CSAddons, "IconHeight");
                                            IconSprites = cpCore.db.csGetInteger(CSAddons, "IconSprites");
                                        }
                                        //
                                        // Calculate DefaultAddonOption_String
                                        //
                                        UseAjaxDefaultAddonOptions = true;
                                        if (UseAjaxDefaultAddonOptions) {
                                            DefaultAddonOption_String = "";
                                        } else {
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
                                        if (ItemsCnt >= ItemsSize) {
                                            ItemsSize = ItemsSize + 100;
                                            Array.Resize(ref Items, ItemsSize + 1);
                                        }
                                    }
                                }
                                cpCore.db.csGoNext(CSAddons);
                            }
                        }
                        cpCore.db.csClose(ref CSAddons);
                    }
                    //
                    // Build output sting in alphabetical order by name
                    //
                    s = "";
                    ItemsPtr = Index.getFirstPtr;
                    while (ItemsPtr >= 0 && LoopPtr < ItemsCnt) {
                        s = s + Environment.NewLine + "," + Items[ItemsPtr];
                        PtrTest = Index.getNextPtr;
                        if (PtrTest < 0) {
                            break;
                        } else {
                            ItemsPtr = PtrTest;
                        }
                        LoopPtr = LoopPtr + 1;
                    }
                    if (!string.IsNullOrEmpty(s)) {
                        s = "[" + s.Substring(3) + "]";
                    }
                    //
                    result = s;
                    cpCore.docProperties.setProperty(cacheKey, result, false);
                }
            } catch (Exception ex) {
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
        public string executeContentCommands(object nothingObject, string Source, CPUtilsBaseClass.addonContext Context, int personalizationPeopleId, bool personalizationIsAuthenticated, ref string Return_ErrorMessage) {
            string returnValue = "";
            try {
                int LoopPtr = 0;
                contentCmdController contentCmd = new contentCmdController(cpCore);
                //
                returnValue = Source;
                LoopPtr = 0;
                while ((LoopPtr < 10) && ((returnValue.IndexOf(contentReplaceEscapeStart) + 1 != 0))) {
                    returnValue = contentCmd.ExecuteCmd(returnValue, Context, personalizationPeopleId, personalizationIsAuthenticated);
                    LoopPtr = LoopPtr + 1;
                }
            } catch (Exception ex) {
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
        private string convertActiveContent_Internal_activeParts(string Source, int personalizationPeopleId, string ContextContentName, int ContextRecordID, int moreInfoPeopleId, bool AddLinkEID, bool EncodeCachableTags, bool EncodeImages, bool EncodeEditIcons, bool EncodeNonCachableTags, string AddAnchorQuery, string ProtocolHostLink, bool IsEmailContent, string AdminURL, bool personalizationIsAuthenticated, CPUtilsBaseClass.addonContext context = CPUtilsBaseClass.addonContext.ContextPage) {
            string result = "";
            try {
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
                if (AddLinkEID && (personalizationPeopleId != 0)) {
                    AnchorQuery = AnchorQuery + "&EID=" + cpCore.security.encodeToken(genericController.EncodeInteger(personalizationPeopleId), DateTime.Now);
                }
                //
                if (!string.IsNullOrEmpty(AddAnchorQuery)) {
                    AnchorQuery = AnchorQuery + "&" + AddAnchorQuery;
                }
                //
                if (!string.IsNullOrEmpty(AnchorQuery)) {
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
                if (Pos > 0) {
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
                if (PosStart > 0) {
                    PosEnd = genericController.vbInstr(PosStart, workingContent, "-->");
                    if (PosEnd > 0) {
                        AllowGroups = workingContent.Substring(PosStart + 21, PosEnd - PosStart - 23);
                        workingContent = workingContent.Substring(0, PosStart - 1) + "<AC type=\"" + ACTypeAggregateFunction + "\" name=\"block text\" querystring=\"allowgroups=" + AllowGroups + "\">" + workingContent.Substring(PosEnd + 2);
                    }
                }
                //
                PosStart = genericController.vbInstr(1, workingContent, "<!-- ENDGROUPACCESS ", Microsoft.VisualBasic.Constants.vbTextCompare);
                if (PosStart > 0) {
                    PosEnd = genericController.vbInstr(PosStart, workingContent, "-->");
                    if (PosEnd > 0) {
                        workingContent = workingContent.Substring(0, PosStart - 1) + "<AC type=\"" + ACTypeAggregateFunction + "\" name=\"block text end\" >" + workingContent.Substring(PosEnd + 2);
                    }
                }
                //
                // ----- Special case -- if any of these are in the source, this is legacy. Convert them to icons,
                //       and they will be converted to AC tags when the icons are saved
                //
                if (EncodeEditIcons) {
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
                if ((!string.IsNullOrEmpty(workingContent)) & (ProcessAnchorTags || ProcessACTags)) {
                    //
                    // ----- Load the Active Elements
                    //
                    KmaHTML = new htmlParserController(cpCore);
                    KmaHTML.Load(workingContent);
                    //
                    // ----- Execute and output elements
                    //
                    ElementPointer = 0;
                    if (KmaHTML.ElementCount > 0) {
                        ElementPointer = 0;
                        workingContent = "";
                        serverFilePath = ProtocolHostLink + "/" + cpCore.serverConfig.appConfig.name + "/files/";
                        Stream = new stringBuilderLegacyController();
                        while (ElementPointer < KmaHTML.ElementCount) {
                            Copy = KmaHTML.Text(ElementPointer).ToString();
                            if (KmaHTML.IsTag(ElementPointer)) {
                                ElementTag = genericController.vbUCase(KmaHTML.TagName(ElementPointer));
                                ACName = KmaHTML.ElementAttribute(ElementPointer, "NAME");
                                UCaseACName = genericController.vbUCase(ACName);
                                switch (ElementTag) {
                                    case "FORM":
                                        //
                                        // Form created in content
                                        // EncodeEditIcons -> remove the
                                        //
                                        if (EncodeNonCachableTags) {
                                            FormCount = FormCount + 1;
                                            //
                                            // 5/14/2009 - DM said it is OK to remove UserResponseForm Processing
                                            // however, leave this one because it is needed to make current forms work.
                                            //
                                            if ((Copy.IndexOf("contensiveuserform=1", System.StringComparison.OrdinalIgnoreCase) + 1 != 0) | (Copy.IndexOf("contensiveuserform=\"1\"", System.StringComparison.OrdinalIgnoreCase) + 1 != 0)) {
                                                //
                                                // if it has "contensiveuserform=1" in the form tag, remove it from the form and add the hidden that makes it work
                                                //
                                                Copy = genericController.vbReplace(Copy, "ContensiveUserForm=1", "", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
                                                Copy = genericController.vbReplace(Copy, "ContensiveUserForm=\"1\"", "", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
                                                if (!EncodeEditIcons) {
                                                    Copy = Copy + "<input type=hidden name=ContensiveUserForm value=1>";
                                                }
                                            }
                                        }
                                        break;
                                    case "INPUT":
                                        if (EncodeNonCachableTags) {
                                            FormInputCount = FormInputCount + 1;
                                        }
                                        break;
                                    case "A":
                                        if (!string.IsNullOrEmpty(AnchorQuery)) {
                                            //
                                            // ----- Add ?eid=0000 to all anchors back to the same site so emails
                                            //       can be sent that will automatically log the person in when they
                                            //       arrive.
                                            //
                                            AttributeCount = KmaHTML.ElementAttributeCount(ElementPointer);
                                            if (AttributeCount > 0) {
                                                Copy = "<A ";
                                                for (AttributePointer = 0; AttributePointer < AttributeCount; AttributePointer++) {
                                                    Name = KmaHTML.ElementAttributeName(ElementPointer, AttributePointer);
                                                    Value = KmaHTML.ElementAttributeValue(ElementPointer, AttributePointer);
                                                    if (genericController.vbUCase(Name) == "HREF") {
                                                        Link = Value;
                                                        Pos = genericController.vbInstr(1, Link, "://");
                                                        if (Pos > 0) {
                                                            Link = Link.Substring(Pos + 2);
                                                            Pos = genericController.vbInstr(1, Link, "/");
                                                            if (Pos > 0) {
                                                                Link = Link.Substring(0, Pos - 1);
                                                            }
                                                        }
                                                        if ((string.IsNullOrEmpty(Link)) || ("," + cpCore.serverConfig.appConfig.domainList[0] + ",".IndexOf("," + Link + ",", System.StringComparison.OrdinalIgnoreCase) + 1 != 0)) {
                                                            //
                                                            // ----- link is for this site
                                                            //
                                                            if (Value.Substring(Value.Length - 1) == "?") {
                                                                //
                                                                // Ends in a questionmark, must be Dwayne (?)
                                                                //
                                                                Value = Value + AnchorQuery;
                                                            } else if (genericController.vbInstr(1, Value, "mailto:", Microsoft.VisualBasic.Constants.vbTextCompare) != 0) {
                                                                //
                                                                // catch mailto
                                                                //
                                                                //Value = Value & AnchorQuery
                                                            } else if (genericController.vbInstr(1, Value, "?") == 0) {
                                                                //
                                                                // No questionmark there, add it
                                                                //
                                                                Value = Value + "?" + AnchorQuery;
                                                            } else {
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
                                        switch (genericController.vbUCase(ACType)) {
                                            case ACTypeEnd: {
                                                    //
                                                    // End Tag - Personalization
                                                    //       This tag causes an end to the all tags, like Language
                                                    //       It is removed by with EncodeEditIcons (on the way to the editor)
                                                    //       It is added to the end of the content with Decode(activecontent)
                                                    //
                                                    if (EncodeEditIcons) {
                                                        Copy = "";
                                                    } else if (EncodeNonCachableTags) {
                                                        Copy = "<!-- Language ANY -->";
                                                    }
                                                    break;
                                                }
                                            case ACTypeDate: {
                                                    //
                                                    // Date Tag
                                                    //
                                                    if (EncodeEditIcons) {
                                                        IconIDControlString = "AC," + ACTypeDate;
                                                        IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "Current Date", "Renders as [Current Date]", ACInstanceID, 0);
                                                        Copy = IconImg;
                                                        //Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as the current date"" ID=""AC," & ACTypeDate & """ src=""/ccLib/images/ACDate.GIF"">"
                                                    } else if (EncodeNonCachableTags) {
                                                        Copy = DateTime.Now.ToString();
                                                    }
                                                    break;
                                                }
                                            case ACTypeMember:
                                            case ACTypePersonalization: {
                                                    //
                                                    // Member Tag works regardless of authentication
                                                    // cm must be sure not to reveal anything
                                                    //
                                                    ACField = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "FIELD"));
                                                    if (string.IsNullOrEmpty(ACField)) {
                                                        // compatibility for old personalization type
                                                        ACField = htmlController.getAddonOptionStringValue("FIELD", KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING"));
                                                    }
                                                    FieldName = genericController.EncodeInitialCaps(ACField);
                                                    if (string.IsNullOrEmpty(FieldName)) {
                                                        FieldName = "Name";
                                                    }
                                                    if (EncodeEditIcons) {
                                                        switch (genericController.vbUCase(FieldName)) {
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
                                                    } else if (EncodeNonCachableTags) {
                                                        if (personalizationPeopleId != 0) {
                                                            if (genericController.vbUCase(FieldName) == "EID") {
                                                                Copy = cpCore.security.encodeToken(personalizationPeopleId, DateTime.Now);
                                                            } else {
                                                                if (!CSPeopleSet) {
                                                                    CSPeople = cpCore.db.cs_openContentRecord("People", personalizationPeopleId);
                                                                    CSPeopleSet = true;
                                                                }
                                                                if ((cpCore.db.csOk(CSPeople) & cpCore.db.cs_isFieldSupported(CSPeople, FieldName)) != 0) {
                                                                    Copy = cpCore.db.csGetLookup(CSPeople, FieldName);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    break;
                                                }
                                            case ACTypeChildList: {
                                                    //
                                                    // Child List
                                                    //
                                                    ListName = genericController.encodeText((KmaHTML.ElementAttribute(ElementPointer, "name")));

                                                    if (EncodeEditIcons) {
                                                        IconIDControlString = "AC," + ACType + ",," + ACName;
                                                        IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "List of Child Pages", "Renders as [List of Child Pages]", ACInstanceID, 0);
                                                        Copy = IconImg;
                                                    } else if (EncodeCachableTags) {
                                                        //
                                                        // Handle in webclient
                                                        //
                                                        // removed sort method because all child pages are read in together in the order set by the parent - improve this later
                                                        Copy = "{{" + ACTypeChildList + "?name=" + genericController.encodeNvaArgument(ListName) + "}}";
                                                    }
                                                    break;
                                                }
                                            case ACTypeContact: {
                                                    //
                                                    // Formatting Tag
                                                    //
                                                    if (EncodeEditIcons) {
                                                        //
                                                        IconIDControlString = "AC," + ACType;
                                                        IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, true, IconIDControlString, "", serverFilePath, "Contact Information Line", "Renders as [Contact Information Line]", ACInstanceID, 0);
                                                        Copy = IconImg;
                                                        //
                                                        //Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as a line of text with contact information for this record's primary contact"" id=""AC," & ACType & """ src=""/ccLib/images/ACContact.GIF"">"
                                                    } else if (EncodeCachableTags) {
                                                        if (moreInfoPeopleId != 0) {
                                                            Copy = pageContentController.getMoreInfoHtml(cpCore, moreInfoPeopleId);
                                                        }
                                                    }
                                                    break;
                                                }
                                            case ACTypeFeedback: {
                                                    //
                                                    // Formatting tag - change from information to be included after submission
                                                    //
                                                    if (EncodeEditIcons) {
                                                        //
                                                        IconIDControlString = "AC," + ACType;
                                                        IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, false, IconIDControlString, "", serverFilePath, "Feedback Form", "Renders as [Feedback Form]", ACInstanceID, 0);
                                                        Copy = IconImg;
                                                        //
                                                        //Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as a feedback form, sent to this record's primary contact."" id=""AC," & ACType & """ src=""/ccLib/images/ACFeedBack.GIF"">"
                                                    } else if (EncodeNonCachableTags) {
                                                        if ((moreInfoPeopleId != 0) & (!string.IsNullOrEmpty(ContextContentName)) & (ContextRecordID != 0)) {
                                                            Copy = FeedbackFormNotSupportedComment;
                                                        }
                                                    }
                                                    break;
                                                }
                                            case ACTypeLanguage: {
                                                    //
                                                    // Personalization Tag - block languages not from the visitor
                                                    //
                                                    ACLanguageName = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "NAME"));
                                                    if (EncodeEditIcons) {
                                                        switch (genericController.vbUCase(ACLanguageName)) {
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
                                                    } else if (EncodeNonCachableTags) {
                                                        if (personalizationPeopleId == 0) {
                                                            PeopleLanguage = "any";
                                                        } else {
                                                            if (!PeopleLanguageSet) {
                                                                if (!CSPeopleSet) {
                                                                    CSPeople = cpCore.db.cs_openContentRecord("people", personalizationPeopleId);
                                                                    CSPeopleSet = true;
                                                                }
                                                                CSlanguage = cpCore.db.cs_openContentRecord("Languages", cpCore.db.csGetInteger(CSPeople, "LanguageID"),,,, "Name");
                                                                if (cpCore.db.csOk(CSlanguage)) {
                                                                    PeopleLanguage = cpCore.db.csGetText(CSlanguage, "name");
                                                                }
                                                                cpCore.db.csClose(ref CSlanguage);
                                                                PeopleLanguageSet = true;
                                                            }
                                                        }
                                                        UcasePeopleLanguage = genericController.vbUCase(PeopleLanguage);
                                                        if (UcasePeopleLanguage == "ANY") {
                                                            //
                                                            // This person wants all the languages, put in language marker and continue
                                                            //
                                                            Copy = "<!-- Language " + ACLanguageName + " -->";
                                                        } else if ((ACLanguageName != UcasePeopleLanguage) & (ACLanguageName != "ANY")) {
                                                            //
                                                            // Wrong language, remove tag, skip to the end, or to the next language tag
                                                            //
                                                            Copy = "";
                                                            ElementPointer = ElementPointer + 1;
                                                            while (ElementPointer < KmaHTML.ElementCount) {
                                                                ElementTag = genericController.vbUCase(KmaHTML.TagName(ElementPointer));
                                                                if (ElementTag == "AC") {
                                                                    ACType = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "TYPE"));
                                                                    if (ACType == ACTypeLanguage) {
                                                                        ElementPointer = ElementPointer - 1;
                                                                        break;
                                                                    } else if (ACType == ACTypeEnd) {
                                                                        break;
                                                                    }
                                                                }
                                                                ElementPointer = ElementPointer + 1;
                                                            }
                                                        } else {
                                                            //
                                                            // Right Language, remove tag
                                                            //
                                                            Copy = "";
                                                        }
                                                    }
                                                    break;
                                                }
                                            case ACTypeAggregateFunction: {
                                                    //
                                                    // ----- Add-on
                                                    //
                                                    NotUsedID = 0;
                                                    AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING");
                                                    addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded);
                                                    if (IsEmailContent) {
                                                        //
                                                        // Addon - for email
                                                        //
                                                        if (EncodeNonCachableTags) {
                                                            //
                                                            // Only hardcoded Add-ons can run in Emails
                                                            //
                                                            switch (genericController.vbLCase(ACName)) {
                                                                case "block text":
                                                                    //
                                                                    // Email is always considered authenticated bc they need their login credentials to get the email.
                                                                    // Allowed to see the content that follows if you are authenticated, admin, or in the group list
                                                                    // This must be done out on the page because the csv does not know about authenticated
                                                                    //
                                                                    Copy = "";
                                                                    GroupIDList = htmlController.getAddonOptionStringValue("AllowGroups", addonOptionString);
                                                                    if (!cpCore.doc.authContext.isMemberOfGroupIdList(cpCore, personalizationPeopleId, true, GroupIDList, true)) {
                                                                        //
                                                                        // Block content if not allowed
                                                                        //
                                                                        ElementPointer = ElementPointer + 1;
                                                                        while (ElementPointer < KmaHTML.ElementCount) {
                                                                            ElementTag = genericController.vbUCase(KmaHTML.TagName(ElementPointer));
                                                                            if (ElementTag == "AC") {
                                                                                ACType = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "TYPE"));
                                                                                if (ACType == ACTypeAggregateFunction) {
                                                                                    if (genericController.vbLCase(KmaHTML.ElementAttribute(ElementPointer, "name")) == "block text end") {
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
                                                                    CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                                                                        addonType = CPUtilsBaseClass.addonContext.ContextEmail,
                                                                        cssContainerClass = "",
                                                                        cssContainerId = "",
                                                                        hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
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
                                                    } else {
                                                        //
                                                        // Addon - for web
                                                        //

                                                        if (EncodeEditIcons) {
                                                            //
                                                            // Get IconFilename, update the optionstring, and execute optionstring replacement functions
                                                            //
                                                            AddonContentName = cnAddons;
                                                            if (true) {
                                                                SelectList = "Name,Link,ID,ArgumentList,ObjectProgramID,IconFilename,IconWidth,IconHeight,IconSprites,IsInline,ccGuid";
                                                            }
                                                            if (!string.IsNullOrEmpty(ACGuid)) {
                                                                Criteria = "ccguid=" + cpCore.db.encodeSQLText(ACGuid);
                                                            } else {
                                                                Criteria = "name=" + cpCore.db.encodeSQLText(UCaseACName);
                                                            }
                                                            CS = cpCore.db.csOpen(AddonContentName, Criteria, "Name,ID",,,,, SelectList);
                                                            if (cpCore.db.csOk(CS)) {
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
                                                            } else {
                                                                switch (genericController.vbLCase(ACName)) {
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
                                                            cpCore.db.csClose(ref CS);
                                                            //
                                                            // Build AddonOptionStringHTMLEncoded from SrcOptionList (for names), itself (for current settings), and SrcOptionList (for select options)
                                                            //
                                                            if (SrcOptionList.IndexOf("wrapper", System.StringComparison.OrdinalIgnoreCase) + 1 == 0) {
                                                                if (AddonIsInline) {
                                                                    SrcOptionList = SrcOptionList + Environment.NewLine + AddonOptionConstructor_Inline;
                                                                } else {
                                                                    SrcOptionList = SrcOptionList + Environment.NewLine + AddonOptionConstructor_Block;
                                                                }
                                                            }
                                                            if (string.IsNullOrEmpty(SrcOptionList)) {
                                                                ResultOptionListHTMLEncoded = "";
                                                            } else {
                                                                ResultOptionListHTMLEncoded = "";
                                                                REsultOptionValue = "";
                                                                SrcOptionList = genericController.vbReplace(SrcOptionList, Environment.NewLine, "\r");
                                                                SrcOptionList = genericController.vbReplace(SrcOptionList, "\n", "\r");
                                                                SrcOptions = Microsoft.VisualBasic.Strings.Split(SrcOptionList, "\r", -1, Microsoft.VisualBasic.CompareMethod.Binary);
                                                                for (Ptr = 0; Ptr <= SrcOptions.GetUpperBound(0); Ptr++) {
                                                                    SrcOptionName = SrcOptions[Ptr];
                                                                    int LoopPtr2 = 0;

                                                                    while ((SrcOptionName.Length > 1) && (SrcOptionName.Substring(0, 1) == "\t") && (LoopPtr2 < 100)) {
                                                                        SrcOptionName = SrcOptionName.Substring(1);
                                                                        LoopPtr2 = LoopPtr2 + 1;
                                                                    }
                                                                    SrcOptionValueSelector = "";
                                                                    SrcOptionSelector = "";
                                                                    Pos = genericController.vbInstr(1, SrcOptionName, "=");
                                                                    if (Pos > 0) {
                                                                        SrcOptionValueSelector = SrcOptionName.Substring(Pos);
                                                                        SrcOptionName = SrcOptionName.Substring(0, Pos - 1);
                                                                        SrcOptionSelector = "";
                                                                        Pos = genericController.vbInstr(1, SrcOptionValueSelector, "[");
                                                                        if (Pos != 0) {
                                                                            SrcOptionSelector = SrcOptionValueSelector.Substring(Pos - 1);
                                                                        }
                                                                    }
                                                                    // all Src and Instance vars are already encoded correctly
                                                                    if (!string.IsNullOrEmpty(SrcOptionName)) {
                                                                        // since AddonOptionString is encoded, InstanceOptionValue will be also
                                                                        InstanceOptionValue = htmlController.getAddonOptionStringValue(SrcOptionName, addonOptionString);
                                                                        //InstanceOptionValue = cpcore.csv_GetAddonOption(SrcOptionName, AddonOptionString)
                                                                        ResultOptionSelector = getAddonSelector(SrcOptionName, genericController.encodeNvaArgument(InstanceOptionValue), SrcOptionSelector);
                                                                        //ResultOptionSelector = csv_GetAddonSelector(SrcOptionName, InstanceOptionValue, SrcOptionValueSelector)
                                                                        ResultOptionListHTMLEncoded = ResultOptionListHTMLEncoded + "&" + ResultOptionSelector;
                                                                    }
                                                                }
                                                                if (!string.IsNullOrEmpty(ResultOptionListHTMLEncoded)) {
                                                                    ResultOptionListHTMLEncoded = ResultOptionListHTMLEncoded.Substring(1);
                                                                }
                                                            }
                                                            ACNameCaption = genericController.vbReplace(ACName, "\"", "");
                                                            ACNameCaption = encodeHTML(ACNameCaption);
                                                            IDControlString = "AC," + ACType + "," + NotUsedID + "," + genericController.encodeNvaArgument(ACName) + "," + ResultOptionListHTMLEncoded + "," + ACGuid;
                                                            Copy = genericController.GetAddonIconImg(AdminURL, IconWidth, IconHeight, IconSprites, AddonIsInline, IDControlString, IconFilename, serverFilePath, IconAlt, IconTitle, ACInstanceID, 0);
                                                        } else if (EncodeNonCachableTags) {
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
                                            case ACTypeImage: {
                                                    //
                                                    // ----- Image Tag, substitute image placeholder with the link from the REsource Library Record
                                                    //
                                                    if (EncodeImages) {
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
                                                        if (file != null) {
                                                            Filename = file.Filename;
                                                            Filename = genericController.vbReplace(Filename, "\\", "/");
                                                            Filename = genericController.EncodeURL(Filename);
                                                            Copy = Copy + "<img ID=\"AC,IMAGE,," + ACAttrRecordID + "\" src=\"" + genericController.getCdnFileLink(cpCore, Filename) + "\"";
                                                            //
                                                            if (ACAttrWidth == 0) {
                                                                ACAttrWidth = file.pxWidth;
                                                            }
                                                            if (ACAttrWidth != 0) {
                                                                Copy = Copy + " width=\"" + ACAttrWidth + "\"";
                                                            }
                                                            //
                                                            if (ACAttrHeight == 0) {
                                                                ACAttrHeight = file.pxHeight;
                                                            }
                                                            if (ACAttrHeight != 0) {
                                                                Copy = Copy + " height=\"" + ACAttrHeight + "\"";
                                                            }
                                                            //
                                                            if (ACAttrVSpace != 0) {
                                                                Copy = Copy + " vspace=\"" + ACAttrVSpace + "\"";
                                                            }
                                                            //
                                                            if (ACAttrHSpace != 0) {
                                                                Copy = Copy + " hspace=\"" + ACAttrHSpace + "\"";
                                                            }
                                                            //
                                                            if (!string.IsNullOrEmpty(ACAttrAlt)) {
                                                                Copy = Copy + " alt=\"" + ACAttrAlt + "\"";
                                                            }
                                                            //
                                                            if (!string.IsNullOrEmpty(ACAttrAlign)) {
                                                                Copy = Copy + " align=\"" + ACAttrAlign + "\"";
                                                            }
                                                            //
                                                            // no, 0 is an important value
                                                            //If ACAttrBorder <> 0 Then
                                                            Copy = Copy + " border=\"" + ACAttrBorder + "\"";
                                                            //    End If
                                                            //
                                                            if (ACAttrLoop != 0) {
                                                                Copy = Copy + " loop=\"" + ACAttrLoop + "\"";
                                                            }
                                                            //
                                                            string attr = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "STYLE"));
                                                            if (!string.IsNullOrEmpty(attr)) {
                                                                Copy = Copy + " style=\"" + attr + "\"";
                                                            }
                                                            //
                                                            attr = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "CLASS"));
                                                            if (!string.IsNullOrEmpty(attr)) {
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
                                            case ACTypeDownload: {
                                                    //
                                                    // ----- substitute and anchored download image for the AC-Download tag
                                                    //
                                                    ACAttrRecordID = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "RECORDID"));
                                                    ACAttrAlt = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "ALT"));
                                                    //
                                                    if (EncodeEditIcons) {
                                                        //
                                                        // Encoding the edit icons for the active editor form
                                                        //
                                                        IconIDControlString = "AC," + ACTypeDownload + ",," + ACAttrRecordID;
                                                        IconImg = genericController.GetAddonIconImg(AdminURL, 16, 16, 0, true, IconIDControlString, "/ccLib/images/IconDownload3.gif", serverFilePath, "Download Icon with a link to a resource", "Renders as [Download Icon with a link to a resource]", ACInstanceID, 0);
                                                        Copy = IconImg;
                                                        //
                                                        //Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Renders as a download icon"" id=""AC," & ACTypeDownload & ",," & ACAttrRecordID & """ src=""/ccLib/images/IconDownload3.GIF"">"
                                                    } else if (EncodeImages) {
                                                        //
                                                        Models.Entity.libraryFilesModel file = Models.Entity.libraryFilesModel.create(cpCore, ACAttrRecordID);
                                                        if (file != null) {
                                                            if (string.IsNullOrEmpty(ACAttrAlt)) {
                                                                ACAttrAlt = genericController.encodeText(file.AltText);
                                                            }
                                                            Copy = "<a href=\"" + ProtocolHostLink + requestAppRootPath + cpCore.siteProperties.serverPageDefault + "?" + RequestNameDownloadID + "=" + ACAttrRecordID + "\" target=\"_blank\"><img src=\"" + ProtocolHostLink + "/ccLib/images/IconDownload3.gif\" width=\"16\" height=\"16\" border=\"0\" alt=\"" + ACAttrAlt + "\"></a>";
                                                        }
                                                    }
                                                    break;
                                                }
                                            case ACTypeTemplateContent: {
                                                    //
                                                    // ----- Create Template Content
                                                    //
                                                    //ACName = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "NAME"))
                                                    AddonOptionStringHTMLEncoded = "";
                                                    addonOptionString = "";
                                                    NotUsedID = 0;
                                                    if (EncodeEditIcons) {
                                                        //
                                                        IconIDControlString = "AC," + ACType + "," + NotUsedID + "," + ACName + "," + AddonOptionStringHTMLEncoded;
                                                        IconImg = genericController.GetAddonIconImg(AdminURL, 52, 64, 0, false, IconIDControlString, "/ccLib/images/ACTemplateContentIcon.gif", serverFilePath, "Template Page Content", "Renders as the Template Page Content", ACInstanceID, 0);
                                                        Copy = IconImg;
                                                        //
                                                        //Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Page Content"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACTemplateContentIcon.gif"" WIDTH=52 HEIGHT=64>"
                                                    } else if (EncodeNonCachableTags) {
                                                        //
                                                        // Add in the Content
                                                        //
                                                        Copy = fpoContentBox;
                                                        //Copy = TemplateBodyContent
                                                        //Copy = "{{" & ACTypeTemplateContent & "}}"
                                                    }
                                                    break;
                                                }
                                            case ACTypeTemplateText: {
                                                    //
                                                    // ----- Create Template Content
                                                    //
                                                    //ACName = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "NAME"))
                                                    AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING");
                                                    addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded);
                                                    NotUsedID = 0;
                                                    if (EncodeEditIcons) {
                                                        //
                                                        IconIDControlString = "AC," + ACType + "," + NotUsedID + "," + ACName + "," + AddonOptionStringHTMLEncoded;
                                                        IconImg = genericController.GetAddonIconImg(AdminURL, 52, 52, 0, false, IconIDControlString, "/ccLib/images/ACTemplateTextIcon.gif", serverFilePath, "Template Text", "Renders as a Template Text Box", ACInstanceID, 0);
                                                        Copy = IconImg;
                                                        //
                                                        //Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as Template Text"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACTemplateTextIcon.gif"" WIDTH=52 HEIGHT=52>"
                                                    } else if (EncodeNonCachableTags) {
                                                        //
                                                        // Add in the Content Page
                                                        //
                                                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                                                        //test - encoding changed
                                                        NewName = htmlController.getAddonOptionStringValue("new", addonOptionString);
                                                        //NewName =  genericController.DecodeResponseVariable(getSimpleNameValue("new", AddonOptionString, "", "&"))
                                                        TextName = htmlController.getAddonOptionStringValue("name", addonOptionString);
                                                        //TextName = getSimpleNameValue("name", AddonOptionString)
                                                        if (string.IsNullOrEmpty(TextName)) {
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
                                            case ACTypeWatchList: {
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
                                                    if (EncodeEditIcons) {
                                                        //
                                                        IconIDControlString = "AC," + ACType + "," + NotUsedID + "," + ACName + "," + AddonOptionStringHTMLEncoded;
                                                        IconImg = genericController.GetAddonIconImg(AdminURL, 109, 10, 0, true, IconIDControlString, "/ccLib/images/ACWatchList.gif", serverFilePath, "Watch List", "Renders as the Watch List [" + ACName + "]", ACInstanceID, 0);
                                                        Copy = IconImg;
                                                        //
                                                        //Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Watch List [" & ACName & "]"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACWatchList.GIF"">"
                                                    } else if (EncodeNonCachableTags) {
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
                    if (FormCount == 0 && FormInputCount > 0) {
                    }
                    workingContent = ReplaceInstructions + workingContent;
                    if (CSPeopleSet) {
                        cpCore.db.csClose(ref CSPeople);
                    }
                    if (CSOrganizationSet) {
                        cpCore.db.csClose(ref CSOrganization);
                    }
                    if (CSVisitorSet) {
                        cpCore.db.csClose(ref CSVisitor);
                    }
                    if (CSVisitSet) {
                        cpCore.db.csClose(ref CSVisit);
                    }
                    KmaHTML = null;
                }
                result = workingContent;
            } catch (Exception ex) {
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
        public string convertEditorResponseToActiveContent(string SourceCopy) {
            string result = "";
            try {
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
                string[] ImageIDArray = { };
                int ImageIDArrayCount = 0;
                string QueryString = null;
                string[] QSSplit = null;
                int QSPtr = 0;
                string serverFilePath = null;
                bool ImageAllowSFResize = false;
                imageEditController sf = null;
                //
                result = SourceCopy;
                if (!string.IsNullOrEmpty(result)) {
                    //
                    // leave this in to make sure old <acform tags are converted back
                    // new editor deals with <form, so no more converting
                    //
                    result = genericController.vbReplace(result, "<ACFORM>", "<FORM>");
                    result = genericController.vbReplace(result, "<ACFORM ", "<FORM ");
                    result = genericController.vbReplace(result, "</ACFORM>", "</form>");
                    result = genericController.vbReplace(result, "</ACFORM ", "</FORM ");
                    if (DHTML.Load(result)) {
                        result = "";
                        ElementCount = DHTML.ElementCount;
                        if (ElementCount > 0) {
                            //
                            // ----- Locate and replace IMG Edit icons with AC tags
                            //
                            Stream = new stringBuilderLegacyController();
                            for (ElementPointer = 0; ElementPointer < ElementCount; ElementPointer++) {
                                ElementText = DHTML.Text(ElementPointer).ToString();
                                if (DHTML.IsTag(ElementPointer)) {
                                    switch (genericController.vbUCase(DHTML.TagName(ElementPointer))) {
                                        case "FORM":
                                            //
                                            // User created form - add the attribute "Contensive=1"
                                            //
                                            // 5/14/2009 - DM said it is OK to remove UserResponseForm Processing
                                            //ElementText = genericController.vbReplace(ElementText, "<FORM", "<FORM ContensiveUserForm=1 ", vbTextCompare)
                                            break;
                                        case "IMG":
                                            AttributeCount = DHTML.ElementAttributeCount(ElementPointer);

                                            if (AttributeCount > 0) {
                                                ImageID = DHTML.ElementAttribute(ElementPointer, "id");
                                                ImageSrcOriginal = DHTML.ElementAttribute(ElementPointer, "src");
                                                VirtualFilePathBad = cpCore.serverConfig.appConfig.name + "/files/";
                                                serverFilePath = "/" + VirtualFilePathBad;
                                                if (ImageSrcOriginal.ToLower().Substring(0, VirtualFilePathBad.Length) == genericController.vbLCase(VirtualFilePathBad)) {
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
                                                if (0 != genericController.vbInstr(1, ImageID, ",")) {
                                                    ImageIDArray = ImageID.Split(',');
                                                    ImageIDArrayCount = ImageIDArray.GetUpperBound(0) + 1;
                                                    if (ImageIDArrayCount > 5) {
                                                        for (Ptr = 5; Ptr < ImageIDArrayCount; Ptr++) {
                                                            ACGuid = ImageIDArray[Ptr];
                                                            if ((ACGuid.Substring(0, 1) == "{") && (ACGuid.Substring(ACGuid.Length - 1) == "}")) {
                                                                //
                                                                // this element is the guid, go with it
                                                                //
                                                                break;
                                                            } else if ((string.IsNullOrEmpty(ACGuid)) && (Ptr == (ImageIDArrayCount - 1))) {
                                                                //
                                                                // this is the last element, leave it as the guid
                                                                //
                                                                break;
                                                            } else {
                                                                //
                                                                // not a valid guid, add it to element 4 and try the next
                                                                //
                                                                ImageIDArray[4] = ImageIDArray[4] + "," + ACGuid;
                                                                ACGuid = "";
                                                            }
                                                        }
                                                    }
                                                    if (ImageIDArrayCount > 1) {
                                                        ACIdentifier = genericController.vbUCase(ImageIDArray[0]);
                                                        ACType = ImageIDArray[1];
                                                        if (ImageIDArrayCount > 2) {
                                                            ACFieldName = ImageIDArray[2];
                                                            if (ImageIDArrayCount > 3) {
                                                                ACInstanceName = ImageIDArray[3];
                                                                if (ImageIDArrayCount > 4) {
                                                                    ACQueryString = ImageIDArray[4];
                                                                    //If ImageIDArrayCount > 5 Then
                                                                    //    ACGuid = ImageIDArray(5)
                                                                    //End If
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                if (ACIdentifier == "AC") {
                                                    if (true) {
                                                        if (true) {
                                                            //
                                                            // ----- Process AC Tag
                                                            //
                                                            ACInstanceID = DHTML.ElementAttribute(ElementPointer, "ACINSTANCEID");
                                                            if (string.IsNullOrEmpty(ACInstanceID)) {
                                                                //GUIDGenerator = New guidClass
                                                                ACInstanceID = Guid.NewGuid().ToString();
                                                                //ACInstanceID = Guid.NewGuid.ToString()
                                                            }
                                                            ElementText = "";
                                                            //----------------------------- change to ACType
                                                            switch (genericController.vbUCase(ACType)) {
                                                                case "IMAGE":
                                                                    //
                                                                    // ----- AC Image, Decode Active Images to Resource Library references
                                                                    //
                                                                    if (ImageIDArrayCount >= 4) {
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

                                                                        if (!string.IsNullOrEmpty(ImageStyle)) {
                                                                            //
                                                                            // ----- Process styles, which override attributes
                                                                            //
                                                                            IMageStyleArray = ImageStyle.Split(';');
                                                                            ImageStyleArrayCount = IMageStyleArray.GetUpperBound(0) + 1;
                                                                            for (ImageStyleArrayPointer = 0; ImageStyleArrayPointer < ImageStyleArrayCount; ImageStyleArrayPointer++) {
                                                                                ImageStylePair = IMageStyleArray[ImageStyleArrayPointer].Trim(' ');
                                                                                PositionColon = genericController.vbInstr(1, ImageStylePair, ":");
                                                                                if (PositionColon > 1) {
                                                                                    ImageStylePairName = (ImageStylePair.Substring(0, PositionColon - 1)).Trim(' ');
                                                                                    ImageStylePairValue = (ImageStylePair.Substring(PositionColon)).Trim(' ');
                                                                                    switch (genericController.vbUCase(ImageStylePairName)) {
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
                                                                    if (ImageIDArrayCount >= 4) {
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
                                                                    if (ACInstanceName == "0") {
                                                                        ACInstanceName = genericController.GetRandomInteger().ToString();
                                                                    }
                                                                    ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\">";
                                                                    break;
                                                                case ACTypeAggregateFunction:
                                                                    //
                                                                    // Function
                                                                    //
                                                                    QueryString = "";
                                                                    if (!string.IsNullOrEmpty(ACQueryString)) {
                                                                        // I added this because single stepping through it I found it split on the & in &amp;
                                                                        // I had added an Add-on and was saving
                                                                        // I find it VERY odd that this could be the case
                                                                        //
                                                                        QSHTMLEncoded = genericController.encodeText(ACQueryString);
                                                                        QueryString = genericController.decodeHtml(QSHTMLEncoded);
                                                                        QSSplit = QueryString.Split('&');
                                                                        for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++) {
                                                                            Pos = genericController.vbInstr(1, QSSplit[QSPtr], "[");
                                                                            if (Pos > 0) {
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
                                                                    if (ImageIDArrayCount > 4) {
                                                                        QueryString = genericController.encodeText(ImageIDArray[4]);
                                                                        QSSplit = QueryString.Split('&');
                                                                        for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++) {
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
                                                                    if (ImageIDArrayCount > 4) {
                                                                        QueryString = genericController.encodeText(ImageIDArray[4]);
                                                                        QueryString = genericController.decodeHtml(QueryString);
                                                                        QSSplit = QueryString.Split('&');
                                                                        for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++) {
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
                                                                    if (ImageIDArrayCount > 4) {
                                                                        QueryString = genericController.encodeText(ImageIDArray[4]);
                                                                        QueryString = genericController.decodeHtml(QueryString);
                                                                        QSSplit = QueryString.Split('&');
                                                                        for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++) {
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
                                                                    if (ImageIDArrayCount > 4) {
                                                                        QueryString = genericController.encodeText(ImageIDArray[4]);
                                                                        QueryString = genericController.decodeHtml(QueryString);
                                                                        QSSplit = QueryString.Split('&');
                                                                        for (QSPtr = 0; QSPtr <= QSSplit.GetUpperBound(0); QSPtr++) {
                                                                            QSSplit[QSPtr] = encodeHTML(QSSplit[QSPtr]);
                                                                        }
                                                                        QueryString = string.Join("&", QSSplit);
                                                                    }
                                                                    ElementText = "<AC type=\"" + ACType + "\" name=\"" + ACInstanceName + "\" ACInstanceID=\"" + ACInstanceID + "\" field=\"" + ACFieldName + "\" querystring=\"" + QueryString + "\">";
                                                                    break;
                                                            }
                                                        }
                                                    }
                                                } else if (genericController.vbInstr(1, ImageSrc, "cclibraryfiles", Microsoft.VisualBasic.Constants.vbTextCompare) != 0) {
                                                    ImageAllowSFResize = cpCore.siteProperties.getBoolean("ImageAllowSFResize", true);
                                                    if (ImageAllowSFResize && true) {
                                                        //
                                                        // if it is a real image, check for resize
                                                        //
                                                        Pos = genericController.vbInstr(1, ImageSrc, "cclibraryfiles", Microsoft.VisualBasic.Constants.vbTextCompare);
                                                        if (Pos != 0) {
                                                            ImageVirtualFilename = ImageSrc.Substring(Pos - 1);
                                                            Paths = ImageVirtualFilename.Split('/');
                                                            if (Paths.GetUpperBound(0) > 2) {
                                                                if (genericController.vbLCase(Paths[1]) == "filename") {
                                                                    RecordID = genericController.EncodeInteger(Paths[2]);
                                                                    if (RecordID != 0) {
                                                                        ImageFilename = Paths[3];
                                                                        ImageVirtualFilePath = genericController.vbReplace(ImageVirtualFilename, ImageFilename, "");
                                                                        Pos = ImageFilename.LastIndexOf(".") + 1;
                                                                        if (Pos > 0) {
                                                                            string ImageFilenameAltSize = "";
                                                                            ImageFilenameExt = ImageFilename.Substring(Pos);
                                                                            ImageFilenameNoExt = ImageFilename.Substring(0, Pos - 1);
                                                                            Pos = ImageFilenameNoExt.LastIndexOf("-") + 1;
                                                                            if (Pos > 0) {
                                                                                //
                                                                                // ImageAltSize should be set from the width and height of the img tag,
                                                                                // NOT from the actual width and height of the image file
                                                                                // NOT from the suffix of the image filename
                                                                                // ImageFilenameAltSize is used when the image has been resized, then 'reset' was hit
                                                                                //  on the properties dialog before the save. The width and height come from this suffix
                                                                                //
                                                                                ImageFilenameAltSize = ImageFilenameNoExt.Substring(Pos);
                                                                                SizeTest = ImageFilenameAltSize.Split('x');
                                                                                if (SizeTest.GetUpperBound(0) != 1) {
                                                                                    ImageFilenameAltSize = "";
                                                                                } else {
                                                                                    if ((genericController.vbIsNumeric(SizeTest[0]) & genericController.vbIsNumeric(SizeTest[1])) != 0) {
                                                                                        ImageFilenameNoExt = ImageFilenameNoExt.Substring(0, Pos - 1);
                                                                                        //RecordVirtualFilenameNoExt = Mid(RecordVirtualFilename, 1, Pos - 1)
                                                                                    } else {
                                                                                        ImageFilenameAltSize = "";
                                                                                    }
                                                                                }
                                                                                //ImageFilenameNoExt = Mid(ImageFilenameNoExt, 1, Pos - 1)
                                                                            }
                                                                            if (genericController.vbInstr(1, sfImageExtList, ImageFilenameExt, Microsoft.VisualBasic.Constants.vbTextCompare) != 0) {
                                                                                //
                                                                                // Determine ImageWidth and ImageHeight
                                                                                //
                                                                                ImageStyle = DHTML.ElementAttribute(ElementPointer, "style");
                                                                                ImageWidth = genericController.EncodeInteger(DHTML.ElementAttribute(ElementPointer, "width"));
                                                                                ImageHeight = genericController.EncodeInteger(DHTML.ElementAttribute(ElementPointer, "height"));
                                                                                if (!string.IsNullOrEmpty(ImageStyle)) {
                                                                                    Styles = ImageStyle.Split(';');
                                                                                    for (Ptr = 0; Ptr <= Styles.GetUpperBound(0); Ptr++) {
                                                                                        Style = Styles[Ptr].Split(':');
                                                                                        if (Style.GetUpperBound(0) > 0) {
                                                                                            StyleName = genericController.vbLCase(Style[0].Trim(' '));
                                                                                            if (StyleName == "width") {
                                                                                                StyleValue = genericController.vbLCase(Style[1].Trim(' '));
                                                                                                StyleValue = genericController.vbReplace(StyleValue, "px", "");
                                                                                                StyleValueInt = genericController.EncodeInteger(StyleValue);
                                                                                                if (StyleValueInt > 0) {
                                                                                                    ImageWidth = StyleValueInt;
                                                                                                }
                                                                                            } else if (StyleName == "height") {
                                                                                                StyleValue = genericController.vbLCase(Style[1].Trim(' '));
                                                                                                StyleValue = genericController.vbReplace(StyleValue, "px", "");
                                                                                                StyleValueInt = genericController.EncodeInteger(StyleValue);
                                                                                                if (StyleValueInt > 0) {
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
                                                                                if (file != null) {
                                                                                    RecordVirtualFilename = file.Filename;
                                                                                    RecordWidth = file.pxWidth;
                                                                                    RecordHeight = file.pxHeight;
                                                                                    RecordAltSizeList = file.AltSizeList;
                                                                                    RecordFilename = RecordVirtualFilename;
                                                                                    Pos = RecordVirtualFilename.LastIndexOf("/") + 1;
                                                                                    if (Pos > 0) {
                                                                                        RecordFilename = RecordVirtualFilename.Substring(Pos);
                                                                                    }
                                                                                    RecordFilenameExt = "";
                                                                                    RecordFilenameNoExt = RecordFilename;
                                                                                    Pos = RecordFilenameNoExt.LastIndexOf(".") + 1;
                                                                                    if (Pos > 0) {
                                                                                        RecordFilenameExt = RecordFilenameNoExt.Substring(Pos);
                                                                                        RecordFilenameNoExt = RecordFilenameNoExt.Substring(0, Pos - 1);
                                                                                    }
                                                                                    //
                                                                                    // if recordwidth or height are missing, get them from the file
                                                                                    //
                                                                                    if (RecordWidth == 0 || RecordHeight == 0) {
                                                                                        sf = new imageEditController();
                                                                                        if (sf.load(genericController.convertCdnUrlToCdnPathFilename(ImageVirtualFilename))) {
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
                                                                                    if (RecordWidth != 0 & RecordHeight != 0) {
                                                                                        //
                                                                                        // set ImageWidth and ImageHeight if one of them is missing
                                                                                        //
                                                                                        if ((ImageWidth == RecordWidth) && (ImageHeight == 0)) {
                                                                                            //
                                                                                            // Image only included width, set default height
                                                                                            //
                                                                                            ImageHeight = RecordHeight;
                                                                                        } else if ((ImageHeight == RecordHeight) && (ImageWidth == 0)) {
                                                                                            //
                                                                                            // Image only included height, set default width
                                                                                            //
                                                                                            ImageWidth = RecordWidth;
                                                                                        } else if ((ImageHeight == 0) && (ImageWidth == 0)) {
                                                                                            //
                                                                                            // Image has no width or height, default both
                                                                                            // This happens when you hit 'reset' on the image properties dialog
                                                                                            //
                                                                                            sf = new imageEditController();
                                                                                            if (sf.load(genericController.convertCdnUrlToCdnPathFilename(ImageVirtualFilename))) {
                                                                                                ImageWidth = sf.width;
                                                                                                ImageHeight = sf.height;
                                                                                            }
                                                                                            sf.Dispose();
                                                                                            sf = null;
                                                                                            if ((ImageHeight == 0) && (ImageWidth == 0) && (!string.IsNullOrEmpty(ImageFilenameAltSize))) {
                                                                                                Pos = genericController.vbInstr(1, ImageFilenameAltSize, "x");
                                                                                                if (Pos != 0) {
                                                                                                    ImageWidth = genericController.EncodeInteger(ImageFilenameAltSize.Substring(0, Pos - 1));
                                                                                                    ImageHeight = genericController.EncodeInteger(ImageFilenameAltSize.Substring(Pos));
                                                                                                }
                                                                                            }
                                                                                            if (ImageHeight == 0 && ImageWidth == 0) {
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
                                                                                        if ((RecordVirtualFilename == (ImageVirtualFilePath + ImageFilename)) && ((RecordWidth == ImageWidth) || (RecordHeight == ImageHeight))) {
                                                                                            //
                                                                                            // OK
                                                                                            // this is the raw image
                                                                                            // image matches record, and the sizes are the same
                                                                                            //
                                                                                            RecordVirtualFilename = RecordVirtualFilename;
                                                                                        } else if ((RecordVirtualFilename == ImageVirtualFilePath + ImageFilenameNoExt + "." + ImageFilenameExt) && (RecordAltSizeList.IndexOf(ImageAltSize, System.StringComparison.OrdinalIgnoreCase) + 1 != 0)) {
                                                                                            //
                                                                                            // OK
                                                                                            // resized image, and altsize is in the list - go with resized image name
                                                                                            //
                                                                                            NewImageFilename = ImageFilenameNoExt + "-" + ImageAltSize + "." + ImageFilenameExt;
                                                                                            // images included in email have spaces that must be converted to "%20" or they 404
                                                                                            imageNewLink = genericController.EncodeURL(genericController.getCdnFileLink(cpCore, ImageVirtualFilePath) + NewImageFilename);
                                                                                            ElementText = genericController.vbReplace(ElementText, ImageSrcOriginal, encodeHTML(imageNewLink));
                                                                                        } else if ((RecordWidth < ImageWidth) || (RecordHeight < ImageHeight)) {
                                                                                            //
                                                                                            // OK
                                                                                            // reize image larger then original - go with it as is
                                                                                            //
                                                                                            // images included in email have spaces that must be converted to "%20" or they 404
                                                                                            ElementText = genericController.vbReplace(ElementText, ImageSrcOriginal, encodeHTML(genericController.EncodeURL(genericController.getCdnFileLink(cpCore, RecordVirtualFilename))));
                                                                                        } else {
                                                                                            //
                                                                                            // resized image - create NewImageFilename (and add new alt size to the record)
                                                                                            //
                                                                                            if (RecordWidth == ImageWidth && RecordHeight == ImageHeight) {
                                                                                                //
                                                                                                // set back to Raw image untouched, use the record image filename
                                                                                                //
                                                                                                ElementText = ElementText;
                                                                                                //ElementText = genericController.vbReplace(ElementText, ImageVirtualFilename, RecordVirtualFilename)
                                                                                            } else {
                                                                                                //
                                                                                                // Raw image filename in content, but it is resized, switch to an alternate size
                                                                                                //
                                                                                                NewImageFilename = RecordFilename;
                                                                                                if ((ImageWidth == 0) || (ImageHeight == 0) || (Environment.NewLine + RecordAltSizeList + Environment.NewLine.IndexOf(Environment.NewLine + ImageAltSize + Environment.NewLine) + 1 == 0)) {
                                                                                                    //
                                                                                                    // Alt image has not been built
                                                                                                    //
                                                                                                    sf = new imageEditController();
                                                                                                    if (!sf.load(genericController.convertCdnUrlToCdnPathFilename(RecordVirtualFilename))) {
                                                                                                        //
                                                                                                        // image load failed, use raw filename
                                                                                                        //
                                                                                                        throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "Error while loading image to resize, [" & RecordVirtualFilename & "]", "dll", "cpCoreClass", "DecodeAciveContent", Err.Number, Err.Source, Err.Description, False, True, "")
                                                                                                                                                                  //INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
                                                                                                        //Microsoft.VisualBasic.Information.Err().Clear();
                                                                                                        NewImageFilename = ImageFilename;
                                                                                                    } else {
                                                                                                        //
                                                                                                        //
                                                                                                        //
                                                                                                        RecordWidth = sf.width;
                                                                                                        RecordHeight = sf.height;
                                                                                                        if (ImageWidth == 0) {
                                                                                                            //
                                                                                                            //
                                                                                                            //
                                                                                                            sf.height = ImageHeight;
                                                                                                        } else if (ImageHeight == 0) {
                                                                                                            //
                                                                                                            //
                                                                                                            //
                                                                                                            sf.width = ImageWidth;
                                                                                                        } else if (RecordHeight == ImageHeight) {
                                                                                                            //
                                                                                                            // change the width
                                                                                                            //
                                                                                                            sf.width = ImageWidth;
                                                                                                        } else {
                                                                                                            //
                                                                                                            // change the height
                                                                                                            //
                                                                                                            sf.height = ImageHeight;
                                                                                                        }
                                                                                                        //
                                                                                                        // if resized only width or height, set the other
                                                                                                        //
                                                                                                        if (ImageWidth == 0) {
                                                                                                            ImageWidth = sf.width;
                                                                                                            ImageAltSize = ImageWidth.ToString() + "x" + ImageHeight.ToString();
                                                                                                        }
                                                                                                        if (ImageHeight == 0) {
                                                                                                            ImageHeight = sf.height;
                                                                                                            ImageAltSize = ImageWidth.ToString() + "x" + ImageHeight.ToString();
                                                                                                        }
                                                                                                        //
                                                                                                        // set HTML attributes so image properties will display
                                                                                                        //
                                                                                                        if (genericController.vbInstr(1, ElementText, "height=", Microsoft.VisualBasic.Constants.vbTextCompare) == 0) {
                                                                                                            ElementText = genericController.vbReplace(ElementText, ">", " height=\"" + ImageHeight + "\">");
                                                                                                        }
                                                                                                        if (genericController.vbInstr(1, ElementText, "width=", Microsoft.VisualBasic.Constants.vbTextCompare) == 0) {
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
            } catch (Exception ex) {
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
        public string convertCRLFToHtmlBreak(object Source) {
            string tempconvertCRLFToHtmlBreak = null;
            try {
                //
                string iSource;
                //
                iSource = genericController.encodeText(Source);
                tempconvertCRLFToHtmlBreak = "";
                if (!string.IsNullOrEmpty(iSource)) {
                    tempconvertCRLFToHtmlBreak = iSource;
                    tempconvertCRLFToHtmlBreak = genericController.vbReplace(tempconvertCRLFToHtmlBreak, "\r", "");
                    tempconvertCRLFToHtmlBreak = genericController.vbReplace(tempconvertCRLFToHtmlBreak, "\n", "<br >");
                }
                return tempconvertCRLFToHtmlBreak;
                //
                // ----- Error Trap
                //
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
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
        public string main_encodeHTML(object Source) {
            try {
                //
                return encodeHTML(genericController.encodeText(Source));
                //
                // ----- Error Trap
                //
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
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
        public string convertTextToHTML(string Source) {
            return convertCRLFToHtmlBreak(encodeHTML(Source));
        }
        //
        //========================================================================
        // ----- Encode Active Content AI
        //========================================================================
        //
        public string convertHTMLToText(string Source) {
            try {
                htmlToTextControllers Decoder = new htmlToTextControllers(cpCore);
                return Decoder.convert(Source);
            } catch (Exception ex) {
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
        public string getAddonSelector(string SrcOptionName, string InstanceOptionValue_AddonEncoded, string SrcOptionValueSelector) {
            string result = "";
            try {
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
                if (PosLeft != 0) {
                    int PosRight = genericController.vbInstr(1, SrcSelector, "]");
                    if (PosRight != 0) {
                        if (PosRight < SrcSelector.Length) {
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
                if (!string.IsNullOrEmpty(SrcSelectorInner)) {
                    Choices = SrcSelectorInner.Split('|');
                    ChoiceCnt = Choices.GetUpperBound(0) + 1;
                    for (Ptr = 0; Ptr < ChoiceCnt; Ptr++) {
                        Choice = Choices[Ptr];
                        IsContentList = false;
                        IsListField = false;
                        //
                        // List Function (and all the indecision that went along with it)
                        //
                        Pos = 0;
                        if (Pos == 0) {
                            Pos = genericController.vbInstr(1, Choice, ACFunctionList1 + "(", Microsoft.VisualBasic.Constants.vbTextCompare);
                            if (Pos > 0) {
                                IsContentList = true;
                                IncludeID = false;
                                FnLen = ACFunctionList1.Length;
                            }
                        }
                        if (Pos == 0) {
                            Pos = genericController.vbInstr(1, Choice, ACFunctionList2 + "(", Microsoft.VisualBasic.Constants.vbTextCompare);
                            if (Pos > 0) {
                                IsContentList = true;
                                IncludeID = false;
                                FnLen = ACFunctionList2.Length;
                            }
                        }
                        if (Pos == 0) {
                            Pos = genericController.vbInstr(1, Choice, ACFunctionList3 + "(", Microsoft.VisualBasic.Constants.vbTextCompare);
                            if (Pos > 0) {
                                IsContentList = true;
                                IncludeID = false;
                                FnLen = ACFunctionList3.Length;
                            }
                        }
                        if (Pos == 0) {
                            Pos = genericController.vbInstr(1, Choice, ACFunctionListID + "(", Microsoft.VisualBasic.Constants.vbTextCompare);
                            if (Pos > 0) {
                                IsContentList = true;
                                IncludeID = true;
                                FnLen = ACFunctionListID.Length;
                            }
                        }
                        if (Pos == 0) {
                            Pos = genericController.vbInstr(1, Choice, ACFunctionList + "(", Microsoft.VisualBasic.Constants.vbTextCompare);
                            if (Pos > 0) {
                                IsContentList = true;
                                IncludeID = false;
                                FnLen = ACFunctionList.Length;
                            }
                        }
                        if (Pos == 0) {
                            Pos = genericController.vbInstr(1, Choice, ACFunctionListFields + "(", Microsoft.VisualBasic.Constants.vbTextCompare);
                            if (Pos > 0) {
                                IsListField = true;
                                IncludeID = false;
                                FnLen = ACFunctionListFields.Length;
                            }
                        }
                        //
                        if (Pos > 0) {
                            //
                            FnArgList = (Choice.Substring((Pos + FnLen) - 1)).Trim(' ');
                            ContentName = "";
                            ContentCriteria = "";
                            if ((FnArgList.Substring(0, 1) == "(") && (FnArgList.Substring(FnArgList.Length - 1) == ")")) {
                                //
                                // set ContentName and ContentCriteria from argument list
                                //
                                FnArgList = FnArgList.Substring(1, FnArgList.Length - 2);
                                FnArgs = genericController.SplitDelimited(FnArgList, ",");
                                FnArgCnt = FnArgs.GetUpperBound(0) + 1;
                                if (FnArgCnt > 0) {
                                    ContentName = FnArgs[0].Trim(' ');
                                    if ((ContentName.Substring(0, 1) == "\"") && (ContentName.Substring(ContentName.Length - 1) == "\"")) {
                                        ContentName = (ContentName.Substring(1, ContentName.Length - 2)).Trim(' ');
                                    } else if ((ContentName.Substring(0, 1) == "'") && (ContentName.Substring(ContentName.Length - 1) == "'")) {
                                        ContentName = (ContentName.Substring(1, ContentName.Length - 2)).Trim(' ');
                                    }
                                }
                                if (FnArgCnt > 1) {
                                    ContentCriteria = FnArgs[1].Trim(' ');
                                    if ((ContentCriteria.Substring(0, 1) == "\"") && (ContentCriteria.Substring(ContentCriteria.Length - 1) == "\"")) {
                                        ContentCriteria = (ContentCriteria.Substring(1, ContentCriteria.Length - 2)).Trim(' ');
                                    } else if ((ContentCriteria.Substring(0, 1) == "'") && (ContentCriteria.Substring(ContentCriteria.Length - 1) == "'")) {
                                        ContentCriteria = (ContentCriteria.Substring(1, ContentCriteria.Length - 2)).Trim(' ');
                                    }
                                }
                            }
                            CS = -1;
                            if (IsContentList) {
                                //
                                // ContentList - Open the Content and build the options from the names
                                //
                                if (!string.IsNullOrEmpty(ContentCriteria)) {
                                    CS = cpCore.db.csOpen(ContentName, ContentCriteria, "name",,,,, "ID,Name");
                                } else {
                                    CS = cpCore.db.csOpen(ContentName,, "name",,,,, "ID,Name");
                                }
                            } else if (IsListField) {
                                //
                                // ListField
                                //
                                CID = Models.Complex.cdefModel.getContentId(cpCore, ContentName);
                                if (CID > 0) {
                                    CS = cpCore.db.csOpen("Content Fields", "Contentid=" + CID, "name",,,,, "ID,Name");
                                }
                            }

                            if (cpCore.db.csOk(CS)) {
                                Cell = cpCore.db.cs_getRows(CS);
                                RowCnt = Cell.GetUpperBound(1) + 1;
                                for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                                    //
                                    RecordName = genericController.encodeText(Cell[1, RowPtr]);
                                    RecordName = genericController.vbReplace(RecordName, Environment.NewLine, " ");
                                    RecordID = genericController.EncodeInteger(Cell[0, RowPtr]);
                                    if (string.IsNullOrEmpty(RecordName)) {
                                        RecordName = "record " + RecordID;
                                    } else if (RecordName.Length > 50) {
                                        RecordName = RecordName.Substring(0, 50) + "...";
                                    }
                                    RecordName = genericController.encodeNvaArgument(RecordName);
                                    list = list + "|" + RecordName;
                                    if (IncludeID) {
                                        list = list + ":" + RecordID;
                                    }
                                }
                            }
                            cpCore.db.csClose(ref CS);
                        } else {
                            //
                            // choice is not a function, just add the choice back to the list
                            //
                            list = list + "|" + Choices[Ptr];
                        }
                    }
                    if (!string.IsNullOrEmpty(list)) {
                        list = list.Substring(1);
                    }
                }
                //
                // Build output string
                //
                //csv_result = encodeNvaArgument(SrcOptionName)
                result = encodeHTML(genericController.encodeNvaArgument(SrcOptionName)) + "=";
                if (!string.IsNullOrEmpty(InstanceOptionValue_AddonEncoded)) {
                    result = result + encodeHTML(InstanceOptionValue_AddonEncoded);
                }
                if (string.IsNullOrEmpty(SrcSelectorSuffix) && string.IsNullOrEmpty(list)) {
                    //
                    // empty list with no suffix, return with name=value
                    //
                } else if (genericController.vbLCase(SrcSelectorSuffix) == "resourcelink") {
                    //
                    // resource link, exit with empty list
                    //
                    result = result + "[]ResourceLink";
                } else {
                    //
                    //
                    //
                    result = result + "[" + list + "]" + SrcSelectorSuffix;
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        // main_Get an HTML Form text input (or text area)
        //
        public string getFormInputHTML(string htmlName, string DefaultValue = "", string styleHeight = "", string styleWidth = "", bool readOnlyfield = false, bool allowActiveContent = false, string addonListJSON = "", string styleList = "", string styleOptionList = "", bool allowResourceLibrary = false) {
            string returnHtml = "";
            try {
                string FieldTypeDefaultEditorAddonIdList = editorController.getFieldTypeDefaultEditorAddonIdList(cpCore);
                string[] FieldTypeDefaultEditorAddonIds = FieldTypeDefaultEditorAddonIdList.Split(',');
                int FieldTypeDefaultEditorAddonId = genericController.EncodeInteger(FieldTypeDefaultEditorAddonIds[FieldTypeIdHTML]);
                if (FieldTypeDefaultEditorAddonId == 0) {
                    //
                    //    use default wysiwyg
                    returnHtml = html_GetFormInputTextExpandable2(htmlName, DefaultValue);
                } else {
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
                    returnHtml = cpCore.addon.execute(addonModel.create(cpCore, FieldTypeDefaultEditorAddonId), new CPUtilsBaseClass.addonExecuteContext() {
                        addonType = CPUtilsBaseClass.addonContext.ContextEditor,
                        instanceArguments = arguments
                    });
                }
            } catch (Exception ex) {
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
        public void processFormToolsPanel(string legacyFormSn = "") {
            try {
                string Button = null;
                string username = null;
                //
                // ----- Read in and save the Member profile values from the tools panel
                //
                if (cpCore.doc.authContext.user.id > 0) {
                    if (!(cpCore.doc.debug_iUserError != "")) {
                        Button = cpCore.docProperties.getText(legacyFormSn + "mb");
                        switch (Button) {
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
                                if (!string.IsNullOrEmpty(username)) {
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
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
        }
        //
        //========================================================================
        // -----
        //========================================================================
        //
        public void processAddonSettingsEditor() {
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
            if (ACInstanceID == PageChildListInstanceID) {
                //
                // ----- Page Content Child List Add-on
                //
                if (RecordID != 0) {
                    addonModel addon = cpCore.addonCache.getAddonById(cpCore.siteProperties.childListAddonID);
                    if (addon != null) {
                        FoundAddon = true;
                        AddonOptionConstructor = addon.ArgumentList;
                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, Environment.NewLine, "\r");
                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\n", "\r");
                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\r", Environment.NewLine);
                        if (true) {
                            if (!string.IsNullOrEmpty(AddonOptionConstructor)) {
                                AddonOptionConstructor = AddonOptionConstructor + Environment.NewLine;
                            }
                            if (addon.IsInline) {
                                AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Inline;
                            } else {
                                AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Block;
                            }
                        }

                        ConstructorSplit = Microsoft.VisualBasic.Strings.Split(AddonOptionConstructor, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
                        AddonOptionConstructor = "";
                        //
                        // main_Get all responses from current Argument List and build new addonOption_String
                        //
                        for (Ptr = 0; Ptr <= ConstructorSplit.GetUpperBound(0); Ptr++) {
                            Arg = ConstructorSplit[Ptr].Split('=');
                            ArgName = Arg[0];
                            OptionCnt = cpCore.docProperties.getInteger(ArgName + "CheckBoxCnt");
                            if (OptionCnt > 0) {
                                ArgValueAddonEncoded = "";
                                for (OptionPtr = 0; OptionPtr < OptionCnt; OptionPtr++) {
                                    ArgValue = cpCore.docProperties.getText(ArgName + OptionPtr);
                                    if (!string.IsNullOrEmpty(ArgValue)) {
                                        ArgValueAddonEncoded = ArgValueAddonEncoded + "," + genericController.encodeNvaArgument(ArgValue);
                                    }
                                }
                                if (!string.IsNullOrEmpty(ArgValueAddonEncoded)) {
                                    ArgValueAddonEncoded = ArgValueAddonEncoded.Substring(1);
                                }
                            } else {
                                ArgValue = cpCore.docProperties.getText(ArgName);
                                ArgValueAddonEncoded = genericController.encodeNvaArgument(ArgValue);
                            }
                            addonOption_String = addonOption_String + "&" + genericController.encodeNvaArgument(ArgName) + "=" + ArgValueAddonEncoded;
                        }
                        if (!string.IsNullOrEmpty(addonOption_String)) {
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
            } else if ((ACInstanceID == "-2") && (!string.IsNullOrEmpty(FieldName))) {
                //
                // ----- Admin Addon, ACInstanceID=-2, FieldName=AddonName
                //
                AddonName = FieldName;
                FoundAddon = false;
                addonModel addon = cpCore.addonCache.getAddonByName(AddonName);
                if (addon != null) {
                    FoundAddon = true;
                    AddonOptionConstructor = addon.ArgumentList;
                    AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, Environment.NewLine, "\r");
                    AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\n", "\r");
                    AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\r", Environment.NewLine);
                    if (!string.IsNullOrEmpty(AddonOptionConstructor)) {
                        AddonOptionConstructor = AddonOptionConstructor + Environment.NewLine;
                    }
                    if (genericController.EncodeBoolean(addon.IsInline)) {
                        AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Inline;
                    } else {
                        AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Block;
                    }
                }
                if (!FoundAddon) {
                    //
                    // Hardcoded Addons
                    //
                    switch (genericController.vbLCase(AddonName)) {
                        case "block text":
                            FoundAddon = true;
                            AddonOptionConstructor = AddonOptionConstructor_ForBlockText;
                            break;
                        case "":
                            break;
                    }
                }
                if (FoundAddon) {
                    ConstructorSplit = Microsoft.VisualBasic.Strings.Split(AddonOptionConstructor, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
                    addonOption_String = "";
                    //
                    // main_Get all responses from current Argument List
                    //
                    for (Ptr = 0; Ptr <= ConstructorSplit.GetUpperBound(0); Ptr++) {
                        string nvp = ConstructorSplit[Ptr].Trim(' ');
                        if (!string.IsNullOrEmpty(nvp)) {
                            Arg = ConstructorSplit[Ptr].Split('=');
                            ArgName = Arg[0];
                            OptionCnt = cpCore.docProperties.getInteger(ArgName + "CheckBoxCnt");
                            if (OptionCnt > 0) {
                                ArgValueAddonEncoded = "";
                                for (OptionPtr = 0; OptionPtr < OptionCnt; OptionPtr++) {
                                    ArgValue = cpCore.docProperties.getText(ArgName + OptionPtr);
                                    if (!string.IsNullOrEmpty(ArgValue)) {
                                        ArgValueAddonEncoded = ArgValueAddonEncoded + "," + genericController.encodeNvaArgument(ArgValue);
                                    }
                                }
                                if (!string.IsNullOrEmpty(ArgValueAddonEncoded)) {
                                    ArgValueAddonEncoded = ArgValueAddonEncoded.Substring(1);
                                }
                            } else {
                                ArgValue = cpCore.docProperties.getText(ArgName);
                                ArgValueAddonEncoded = genericController.encodeNvaArgument(ArgValue);
                            }
                            addonOption_String = addonOption_String + "&" + genericController.encodeNvaArgument(ArgName) + "=" + ArgValueAddonEncoded;
                        }
                    }
                    if (!string.IsNullOrEmpty(addonOption_String)) {
                        addonOption_String = addonOption_String.Substring(1);
                    }
                    cpCore.userProperty.setProperty("Addon [" + AddonName + "] Options", addonOption_String);
                    needToClearCache = true;
                }
            } else if (string.IsNullOrEmpty(ContentName) || RecordID == 0) {
                //
                // ----- Public Site call, must have contentname and recordid
                //
                cpCore.handleException(new Exception("invalid content [" + ContentName + "], RecordID [" + RecordID + "]"));
            } else {
                //
                // ----- Normal Content Edit - find instance in the content
                //
                CS = cpCore.db.csOpenRecord(ContentName, RecordID);
                if (!cpCore.db.csOk(CS)) {
                    cpCore.handleException(new Exception("No record found with content [" + ContentName + "] and RecordID [" + RecordID + "]"));
                } else {
                    if (!string.IsNullOrEmpty(FieldName)) {
                        //
                        // Field is given, find the position
                        //
                        Copy = cpCore.db.csGet(CS, FieldName);
                        PosACInstanceID = genericController.vbInstr(1, Copy, "=\"" + ACInstanceID + "\" ", Microsoft.VisualBasic.Constants.vbTextCompare);
                    } else {
                        //
                        // Find the field, then find the position
                        //
                        FieldName = cpCore.db.cs_getFirstFieldName(CS);
                        while (!string.IsNullOrEmpty(FieldName)) {
                            fieldType = cpCore.db.cs_getFieldTypeId(CS, FieldName);
                            switch (fieldType) {
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
                                    if (PosACInstanceID != 0) {
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
                        ExitLabel1:;
                    }
                    //
                    // Parse out the Addon Name
                    //
                    if (PosACInstanceID == 0) {
                        cpCore.handleException(new Exception("AC Instance [" + ACInstanceID + "] not found in record with content [" + ContentName + "] and RecordID [" + RecordID + "]"));
                    } else {
                        Copy = upgradeActiveContent(Copy);
                        ParseOK = false;
                        PosStart = Copy.LastIndexOf("<ac ", PosACInstanceID - 1, System.StringComparison.OrdinalIgnoreCase) + 1;
                        if (PosStart != 0) {
                            //
                            // main_Get Addon Name to lookup Addon and main_Get most recent Argument List
                            //
                            PosNameStart = genericController.vbInstr(PosStart, Copy, " name=", Microsoft.VisualBasic.Constants.vbTextCompare);
                            if (PosNameStart != 0) {
                                PosNameStart = PosNameStart + 7;
                                PosNameEnd = genericController.vbInstr(PosNameStart, Copy, "\"");
                                if (PosNameEnd != 0) {
                                    AddonName = Copy.Substring(PosNameStart - 1, PosNameEnd - PosNameStart);
                                    //????? test this
                                    FoundAddon = false;
                                    addonModel embeddedAddon = cpCore.addonCache.getAddonByName(AddonName);
                                    if (embeddedAddon != null) {
                                        FoundAddon = true;
                                        AddonOptionConstructor = genericController.encodeText(embeddedAddon.ArgumentList);
                                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, Environment.NewLine, "\r");
                                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\n", "\r");
                                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, "\r", Environment.NewLine);
                                        if (!string.IsNullOrEmpty(AddonOptionConstructor)) {
                                            AddonOptionConstructor = AddonOptionConstructor + Environment.NewLine;
                                        }
                                        if (genericController.EncodeBoolean(embeddedAddon.IsInline)) {
                                            AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Inline;
                                        } else {
                                            AddonOptionConstructor = AddonOptionConstructor + AddonOptionConstructor_Block;
                                        }
                                    } else {
                                        //
                                        // -- Hardcoded Addons
                                        switch (genericController.vbLCase(AddonName)) {
                                            case "block text":
                                                FoundAddon = true;
                                                AddonOptionConstructor = AddonOptionConstructor_ForBlockText;
                                                break;
                                            case "":
                                                break;
                                        }
                                    }
                                    if (FoundAddon) {
                                        ConstructorSplit = Microsoft.VisualBasic.Strings.Split(AddonOptionConstructor, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
                                        addonOption_String = "";
                                        //
                                        // main_Get all responses from current Argument List
                                        //
                                        for (Ptr = 0; Ptr <= ConstructorSplit.GetUpperBound(0); Ptr++) {
                                            constructor = ConstructorSplit[Ptr];
                                            if (!string.IsNullOrEmpty(constructor)) {
                                                Arg = constructor.Split('=');
                                                ArgName = Arg[0];
                                                OptionCnt = cpCore.docProperties.getInteger(ArgName + "CheckBoxCnt");
                                                if (OptionCnt > 0) {
                                                    ArgValueAddonEncoded = "";
                                                    for (OptionPtr = 0; OptionPtr < OptionCnt; OptionPtr++) {
                                                        ArgValue = cpCore.docProperties.getText(ArgName + OptionPtr);
                                                        if (!string.IsNullOrEmpty(ArgValue)) {
                                                            ArgValueAddonEncoded = ArgValueAddonEncoded + "," + genericController.encodeNvaArgument(ArgValue);
                                                        }
                                                    }
                                                    if (!string.IsNullOrEmpty(ArgValueAddonEncoded)) {
                                                        ArgValueAddonEncoded = ArgValueAddonEncoded.Substring(1);
                                                    }
                                                } else {
                                                    ArgValue = cpCore.docProperties.getText(ArgName);
                                                    ArgValueAddonEncoded = genericController.encodeNvaArgument(ArgValue);
                                                }

                                                addonOption_String = addonOption_String + "&" + genericController.encodeNvaArgument(ArgName) + "=" + ArgValueAddonEncoded;
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(addonOption_String)) {
                                            addonOption_String = addonOption_String.Substring(1);
                                        }
                                    }
                                }
                            }
                            //
                            // Replace the new querystring into the AC tag in the content
                            //
                            PosIDStart = genericController.vbInstr(PosStart, Copy, " querystring=", Microsoft.VisualBasic.Constants.vbTextCompare);
                            if (PosIDStart != 0) {
                                PosIDStart = PosIDStart + 14;
                                if (PosIDStart != 0) {
                                    PosIDEnd = genericController.vbInstr(PosIDStart, Copy, "\"");
                                    if (PosIDEnd != 0) {
                                        ParseOK = true;
                                        Copy = Copy.Substring(0, PosIDStart - 1) + encodeHTML(addonOption_String) + Copy.Substring(PosIDEnd - 1);
                                        cpCore.db.csSet(CS, FieldName, Copy);
                                        needToClearCache = true;
                                    }
                                }
                            }
                        }
                        if (!ParseOK) {
                            cpCore.handleException(new Exception("There was a problem parsing AC Instance [" + ACInstanceID + "] record with content [" + ContentName + "] and RecordID [" + RecordID + "]"));
                        }
                    }
                }
                cpCore.db.csClose(ref CS);
            }
            if (needToClearCache) {
                //
                // Clear Caches
                //
                if (!string.IsNullOrEmpty(ContentName)) {
                    cpCore.cache.invalidateAllObjectsInContent(ContentName);
                }
            }
        }
        //
        //========================================================================
        // ----- Process the little edit form in the help bubble
        //========================================================================
        //
        public void processHelpBubbleEditor() {
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
            switch (genericController.vbLCase(IDSplit[0])) {
                case "userfield":
                    //
                    // main_Get the id of the field, and save the input as the caption and help
                    //
                    if (IDSplit.GetUpperBound(0) > 0) {
                        RecordID = genericController.EncodeInteger(IDSplit[1]);
                        if (RecordID > 0) {
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
        public string convertActiveContentToHtmlForWysiwygEditor(string editorValue) {
            return cpCore.html.convertActiveContent_internal(editorValue, 0, "", 0, 0, false, false, false, true, true, false, "", "", false, 0, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple, false, null, false);
        }
        //
        //====================================================================================================
        //
        public string convertActiveContentToJsonForRemoteMethod(string Source, string ContextContentName, int ContextRecordID, int ContextContactPeopleID, string ProtocolHostString, int DefaultWrapperID, string ignore_TemplateCaseOnly_Content, CPUtilsBaseClass.addonContext addonContext) {
            return convertActiveContent_internal(Source, cpCore.doc.authContext.user.id, ContextContentName, ContextRecordID, ContextContactPeopleID, false, false, true, true, false, true, "", ProtocolHostString, false, DefaultWrapperID, ignore_TemplateCaseOnly_Content, addonContext, cpCore.doc.authContext.isAuthenticated, null, cpCore.doc.authContext.isEditingAnything());
            //False, False, True, True, False, True, ""
        }
        //
        //====================================================================================================
        //
        public string convertActiveContentToHtmlForWebRender(string Source, string ContextContentName, int ContextRecordID, int ContextContactPeopleID, string ProtocolHostString, int DefaultWrapperID, CPUtilsBaseClass.addonContext addonContext) {
            return convertActiveContent_internal(Source, cpCore.doc.authContext.user.id, ContextContentName, ContextRecordID, ContextContactPeopleID, false, false, true, true, false, true, "", ProtocolHostString, false, DefaultWrapperID, "", addonContext, cpCore.doc.authContext.isAuthenticated, null, cpCore.doc.authContext.isEditingAnything());
            //False, False, True, True, False, True, ""
        }
        //
        //====================================================================================================
        //
        public string convertActiveContentToHtmlForEmailSend(string Source, int personalizationPeopleID, string queryStringForLinkAppend) {
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
        public string getCheckList2(string TagName, string PrimaryContentName, int PrimaryRecordID, string SecondaryContentName, string RulesContentName, string RulesPrimaryFieldname, string RulesSecondaryFieldName, string SecondaryContentSelectCriteria = "", string CaptionFieldName = "", bool readOnlyfield = false) {
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
        public string getCheckList(string TagName, string PrimaryContentName, int PrimaryRecordID, string SecondaryContentName, string RulesContentName, string RulesPrimaryFieldname, string RulesSecondaryFieldName, string SecondaryContentSelectCriteria = "", string CaptionFieldName = "", bool readOnlyfield = false, bool IncludeContentFolderDivs = false, string DefaultSecondaryIDList = "") {
            string returnHtml = "";
            try {
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
                int[] main_MemberShip = { };
                string[] main_MemberShipRuleCopy = { };
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
                if (IsRuleCopySupported) {
                    IsRuleCopySupported = IsRuleCopySupported && Models.Complex.cdefModel.isContentFieldSupported(cpCore, SecondaryContentName, "AllowRuleCopy");
                    if (IsRuleCopySupported) {
                        IsRuleCopySupported = IsRuleCopySupported && Models.Complex.cdefModel.isContentFieldSupported(cpCore, SecondaryContentName, "RuleCopyCaption");
                    }
                }
                if (string.IsNullOrEmpty(CaptionFieldName)) {
                    CaptionFieldName = "name";
                }
                CaptionFieldName = genericController.encodeEmptyText(CaptionFieldName, "name");
                if (string.IsNullOrEmpty(PrimaryContentName) || string.IsNullOrEmpty(SecondaryContentName) || string.IsNullOrEmpty(RulesContentName) || string.IsNullOrEmpty(RulesPrimaryFieldname) || string.IsNullOrEmpty(RulesSecondaryFieldName)) {
                    returnHtml = "[Checklist not configured]";
                    cpCore.handleException(new Exception("Creating checklist, all required fields were not supplied, Caption=[" + CaptionFieldName + "], PrimaryContentName=[" + PrimaryContentName + "], SecondaryContentName=[" + SecondaryContentName + "], RulesContentName=[" + RulesContentName + "], RulesPrimaryFieldName=[" + RulesPrimaryFieldname + "], RulesSecondaryFieldName=[" + RulesSecondaryFieldName + "]"));
                } else {
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
                    if ((!string.IsNullOrEmpty(SecondaryTablename)) & (!string.IsNullOrEmpty(rulesTablename))) {
                        OldFolderVar = "OldFolder" + cpCore.doc.checkListCnt;
                        javaScriptRequired += "var " + OldFolderVar + ";";
                        if (PrimaryRecordID == 0) {
                            //
                            // New record, use the DefaultSecondaryIDList
                            //
                            if (!string.IsNullOrEmpty(DefaultSecondaryIDList)) {

                                main_MemberShipText = DefaultSecondaryIDList.Split(',');
                                for (Ptr = 0; Ptr <= main_MemberShipText.GetUpperBound(0); Ptr++) {
                                    main_MemberShipID = genericController.EncodeInteger(main_MemberShipText[Ptr]);
                                    if (main_MemberShipID != 0) {
                                        Array.Resize(ref main_MemberShip, Ptr + 1);
                                        main_MemberShip[Ptr] = main_MemberShipID;
                                        main_MemberShipCount = Ptr + 1;
                                    }
                                }
                                if (main_MemberShipCount > 0) {
                                    main_MemberShipRuleCopy = new string[main_MemberShipCount];
                                }
                                //main_MemberShipCount = UBound(main_MemberShip) + 1
                                main_MemberShipSize = main_MemberShipCount;
                            }
                        } else {
                            //
                            // ----- Determine main_MemberShip (which secondary records are associated by a rule)
                            // ----- (exclude new record issue ID=0)
                            //
                            if (IsRuleCopySupported) {
                                SQL = "SELECT " + SecondaryTablename + ".ID AS ID," + rulesTablename + ".RuleCopy";
                            } else {
                                SQL = "SELECT " + SecondaryTablename + ".ID AS ID,'' as RuleCopy";
                            }
                            SQL += ""
                            + " FROM " + SecondaryTablename + " LEFT JOIN"
                            + " " + rulesTablename + " ON " + SecondaryTablename + ".ID = " + rulesTablename + "." + RulesSecondaryFieldName + " WHERE "
                            + " (" + rulesTablename + "." + RulesPrimaryFieldname + "=" + PrimaryRecordID + ")"
                            + " AND (" + rulesTablename + ".Active<>0)"
                            + " AND (" + SecondaryTablename + ".Active<>0)"
                            + " And (" + SecondaryTablename + ".ContentControlID IN (" + string.Join(",", ContentIDList) + "))";
                            if (!string.IsNullOrEmpty(SecondaryContentSelectCriteria)) {
                                SQL += "AND(" + SecondaryContentSelectCriteria + ")";
                            }
                            CS = cpCore.db.csOpenSql(SQL);
                            if (cpCore.db.csOk(CS)) {
                                if (true) {
                                    main_MemberShipSize = 10;
                                    main_MemberShip = new int[main_MemberShipSize + 1];
                                    main_MemberShipRuleCopy = new string[main_MemberShipSize + 1];
                                    while (cpCore.db.csOk(CS)) {
                                        if (main_MemberShipCount >= main_MemberShipSize) {
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
                            cpCore.db.csClose(ref CS);
                        }
                        //
                        // ----- Gather all the Secondary Records, sorted by ContentName
                        //
                        SQL = "SELECT " + SecondaryTablename + ".ID AS ID, " + SecondaryTablename + "." + CaptionFieldName + " AS OptionCaption, " + SecondaryTablename + ".name AS OptionName, " + SecondaryTablename + ".SortOrder";
                        if (IsRuleCopySupported) {
                            SQL += "," + SecondaryTablename + ".AllowRuleCopy," + SecondaryTablename + ".RuleCopyCaption";
                        } else {
                            SQL += ",0 as AllowRuleCopy,'' as RuleCopyCaption";
                        }
                        SQL += " from " + SecondaryTablename + " where (1=1)";
                        if (!string.IsNullOrEmpty(SecondaryContentSelectCriteria)) {
                            SQL += "AND(" + SecondaryContentSelectCriteria + ")";
                        }
                        SQL += " GROUP BY " + SecondaryTablename + ".ID, " + SecondaryTablename + "." + CaptionFieldName + ", " + SecondaryTablename + ".name, " + SecondaryTablename + ".SortOrder";
                        if (IsRuleCopySupported) {
                            SQL += ", " + SecondaryTablename + ".AllowRuleCopy," + SecondaryTablename + ".RuleCopyCaption";
                        }
                        SQL += " ORDER BY ";
                        SQL += SecondaryTablename + "." + CaptionFieldName;
                        CS = cpCore.db.csOpenSql(SQL);
                        if (!cpCore.db.csOk(CS)) {
                            returnHtml = "(No choices are available.)";
                        } else {
                            if (true) {
                                OpenFolderID = -1;
                                EndDiv = "";
                                SectionName = "";
                                CheckBoxCnt = 0;
                                DivCheckBoxCnt = 0;
                                DivCnt = 0;
                                CanSeeHiddenFields = cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore);
                                DivName = TagName + ".All";
                                while (cpCore.db.csOk(CS)) {
                                    OptionName = cpCore.db.csGetText(CS, "OptionName");
                                    if ((OptionName.Substring(0, 1) != "_") || CanSeeHiddenFields) {
                                        //
                                        // Current checkbox is visible
                                        //
                                        RecordID = cpCore.db.csGetInteger(CS, "ID");
                                        AllowRuleCopy = cpCore.db.csGetBoolean(CS, "AllowRuleCopy");
                                        RuleCopyCaption = cpCore.db.csGetText(CS, "RuleCopyCaption");
                                        OptionCaption = cpCore.db.csGetText(CS, "OptionCaption");
                                        if (string.IsNullOrEmpty(OptionCaption)) {
                                            OptionCaption = OptionName;
                                        }
                                        if (string.IsNullOrEmpty(OptionCaption)) {
                                            optionCaptionHtmlEncoded = SingularPrefixHtmlEncoded + RecordID;
                                        } else {
                                            optionCaptionHtmlEncoded = encodeHTML(OptionCaption);
                                        }
                                        if (DivCheckBoxCnt != 0) {
                                            // leave this between checkboxes - it is searched in the admin page
                                            returnHtml += "<br >" + Environment.NewLine;
                                        }
                                        RuleCopy = "";
                                        if (false) {
                                            Found = false;
                                            //s = s & "<input type=""checkbox"" name=""" & TagName & "." & CheckBoxCnt & """ "
                                            if (main_MemberShipCount != 0) {
                                                for (main_MemberShipPointer = 0; main_MemberShipPointer < main_MemberShipCount; main_MemberShipPointer++) {
                                                    if (main_MemberShip[main_MemberShipPointer] == (RecordID)) {
                                                        RuleCopy = main_MemberShipRuleCopy[main_MemberShipPointer];
                                                        returnHtml += html_GetFormInputHidden(TagName + "." + CheckBoxCnt, true);
                                                        Found = true;
                                                        break;
                                                    }
                                                }
                                            }
                                            returnHtml += genericController.main_GetYesNo(Found) + "&nbsp;-&nbsp;";
                                        } else {
                                            Found = false;
                                            if (main_MemberShipCount != 0) {
                                                for (main_MemberShipPointer = 0; main_MemberShipPointer < main_MemberShipCount; main_MemberShipPointer++) {
                                                    if (main_MemberShip[main_MemberShipPointer] == (RecordID)) {
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
                                            if (readOnlyfield && !Found) {
                                                returnHtml += "<input type=checkbox disabled>";
                                            } else if (readOnlyfield) {
                                                returnHtml += "<input type=checkbox disabled checked>";
                                                returnHtml += "<input type=\"hidden\" name=\"" + TagName + "." + CheckBoxCnt + ".ID\" value=" + RecordID + ">";
                                            } else if (Found) {
                                                returnHtml += "<input type=checkbox name=\"" + TagName + "." + CheckBoxCnt + "\" checked>";
                                            } else {
                                                returnHtml += "<input type=checkbox name=\"" + TagName + "." + CheckBoxCnt + "\">";
                                            }
                                            returnHtml += "</td><td style=\"vertical-align:top;padding-top:4px;\">";
                                            returnHtml += SpanClassAdminNormal + optionCaptionHtmlEncoded;
                                            if (AllowRuleCopy) {
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
                        cpCore.db.csClose(ref CS);
                        addScriptCode_head(javaScriptRequired, "CheckList Categories");
                    }
                    //End If
                    cpCore.doc.checkListCnt = cpCore.doc.checkListCnt + 1;
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnHtml;
        }
        //
        //========================================================================
        // main_GetRecordEditLink2( iContentName, iRecordID, AllowCut, RecordName )
        //
        //   ContentName The content for this link
        //   RecordID    The ID of the record in the Table
        //   AllowCut
        //   RecordName
        //   IsEditing
        //========================================================================
        //
        public string main_GetRecordEditLink2(string ContentName, int RecordID, bool AllowCut, string RecordName, bool IsEditing) {
            string tempmain_GetRecordEditLink2 = null;
            try {
                //
                //If Not (true) Then Exit Function
                //
                int CS = 0;
                string SQL = null;
                int ContentID = 0;
                string Link = null;
                string MethodName = null;
                string iContentName = null;
                int iRecordID = 0;
                string RootEntryName = null;
                string ClipBoard = null;
                string WorkingLink = null;
                bool iAllowCut = false;
                string Icon = null;
                string ContentCaption = null;
                //
                iContentName = genericController.encodeText(ContentName);
                iRecordID = genericController.EncodeInteger(RecordID);
                iAllowCut = genericController.EncodeBoolean(AllowCut);
                ContentCaption = genericController.encodeHTML(iContentName);
                if (genericController.vbLCase(ContentCaption) == "aggregate functions") {
                    ContentCaption = "Add-on";
                }
                if (genericController.vbLCase(ContentCaption) == "aggregate function objects") {
                    ContentCaption = "Add-on";
                }
                ContentCaption = ContentCaption + " record";
                if (!string.IsNullOrEmpty(RecordName)) {
                    ContentCaption = ContentCaption + ", named '" + RecordName + "'";
                }
                //
                MethodName = "main_GetRecordEditLink2";
                //
                tempmain_GetRecordEditLink2 = "";
                if (string.IsNullOrEmpty(iContentName)) {
                    throw (new ApplicationException("ContentName [" + ContentName + "] is invalid")); // handleLegacyError14(MethodName, "")
                } else {
                    if (iRecordID < 1) {
                        throw (new ApplicationException("RecordID [" + RecordID + "] is invalid")); // handleLegacyError14(MethodName, "")
                    } else {
                        if (IsEditing) {
                            //
                            // Edit link, main_Get the CID
                            //
                            ContentID = Models.Complex.cdefModel.getContentId(cpCore, iContentName);
                            //
                            tempmain_GetRecordEditLink2 = tempmain_GetRecordEditLink2 + "<a"
                                + " class=\"ccRecordEditLink\" "
                                + " TabIndex=-1"
                                + " href=\"" + genericController.encodeHTML("/" + cpCore.serverConfig.appConfig.adminRoute + "?cid=" + ContentID + "&id=" + iRecordID + "&af=4&aa=2&ad=1") + "\"";
                            tempmain_GetRecordEditLink2 = tempmain_GetRecordEditLink2 + "><img"
                                + " src=\"/ccLib/images/IconContentEdit.gif\""
                                + " border=\"0\""
                                + " alt=\"Edit this " + genericController.encodeHTML(ContentCaption) + "\""
                                + " title=\"Edit this " + genericController.encodeHTML(ContentCaption) + "\""
                                + " align=\"absmiddle\""
                                + "></a>";
                            //
                            // Cut Link if enabled
                            //
                            if (iAllowCut) {
                                WorkingLink = genericController.modifyLinkQuery(cpCore.webServer.requestPage + "?" + cpCore.doc.refreshQueryString, RequestNameCut, genericController.encodeText(ContentID) + "." + genericController.encodeText(RecordID), true);
                                tempmain_GetRecordEditLink2 = ""
                                    + tempmain_GetRecordEditLink2 + "<a class=\"ccRecordCutLink\" TabIndex=\"-1\" href=\"" + genericController.encodeHTML(WorkingLink) + "\"><img src=\"/ccLib/images/Contentcut.gif\" border=\"0\" alt=\"Cut this " + ContentCaption + " to clipboard\" title=\"Cut this " + ContentCaption + " to clipboard\" align=\"absmiddle\"></a>";
                            }
                            //
                            // Help link if enabled
                            //
                            string helpLink = "";
                            //helpLink = main_GetHelpLink(5, "Editing " & ContentCaption, "Turn on Edit icons by checking 'Edit' in the tools panel, and click apply.<br><br><img src=""/ccLib/images/IconContentEdit.gif"" style=""vertical-align:middle""> Edit-Content icon<br><br>Edit-Content icons appear in your content. Click them to edit your content.")
                            tempmain_GetRecordEditLink2 = ""
                                + tempmain_GetRecordEditLink2 + helpLink;
                            //
                            tempmain_GetRecordEditLink2 = "<span class=\"ccRecordLinkCon\" style=\"white-space:nowrap;\">" + tempmain_GetRecordEditLink2 + "</span>";
                            //'
                            //main_GetRecordEditLink2 = "" _
                            //    & cr & "<div style=""position:absolute;"">" _
                            //    & genericController.kmaIndent(main_GetRecordEditLink2) _
                            //    & cr & "</div>"
                            //
                            //main_GetRecordEditLink2 = "" _
                            //    & cr & "<div style=""position:relative;display:inline;"">" _
                            //    & genericController.kmaIndent(main_GetRecordEditLink2) _
                            //    & cr & "</div>"
                        }

                    }
                }
                //
                return tempmain_GetRecordEditLink2;
                //
                // ----- Error Trap
                //
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError18(MethodName)
                                                                    //
            return tempmain_GetRecordEditLink2;
        }
        //
        //========================================================================
        // Print an add link for the current ContentSet
        //   iCSPointer is the content set to be added to
        //   PresetNameValueList is a name=value pair to force in the added record
        //========================================================================
        //
        public string main_cs_getRecordAddLink(int CSPointer, string PresetNameValueList = "", bool AllowPaste = false) {
            string tempmain_cs_getRecordAddLink = null;
            try {
                //
                //If Not (true) Then Exit Function
                //
                string ContentName = null;
                string iPresetNameValueList = null;
                string MethodName = null;
                int iCSPointer;
                //
                iCSPointer = genericController.EncodeInteger(CSPointer);
                iPresetNameValueList = genericController.encodeEmptyText(PresetNameValueList, "");
                //
                MethodName = "main_cs_getRecordAddLink";
                //
                if (iCSPointer < 0) {
                    throw (new ApplicationException("invalid ContentSet pointer [" + iCSPointer + "]")); // handleLegacyError14(MethodName, "main_cs_getRecordAddLink was called with ")
                } else {
                    //
                    // Print an add tag to the iCSPointers Content
                    //
                    ContentName = cpCore.db.cs_getContentName(iCSPointer);
                    if (string.IsNullOrEmpty(ContentName)) {
                        throw (new ApplicationException("main_cs_getRecordAddLink was called with a ContentSet that was created with an SQL statement. The function requires a ContentSet opened with an OpenCSContent.")); // handleLegacyError14(MethodName, "")
                    } else {
                        tempmain_cs_getRecordAddLink = main_GetRecordAddLink(ContentName, iPresetNameValueList, AllowPaste);
                    }
                }
                return tempmain_cs_getRecordAddLink;
                //
                // ----- Error Trap
                //
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError18(MethodName)
                                                                    //
            return tempmain_cs_getRecordAddLink;
        }
        //
        //========================================================================
        // main_GetRecordAddLink( iContentName, iPresetNameValueList )
        //
        //   Returns a string of add tags for the Content Definition included, and all
        //   child contents of that area.
        //
        //   iContentName The content for this link
        //   iPresetNameValueList The sql equivalent used to select the record.
        //           translates to name0=value0,name1=value1.. pairs separated by ,
        //
        //   LowestRootMenu - The Menu in the flyout structure that is the furthest down
        //   in the chain that the user has content access to. This is so a content manager
        //   does not have to navigate deep into a structure to main_Get to content he can
        //   edit.
        //   Basically, the entire menu is created down from the MenuName, and populated
        //   with all the entiries this user has access to. The LowestRequiredMenuName is
        //   is returned from the _branch routine, and that is to root on-which the
        //   main_GetMenu uses
        //========================================================================
        //
        public string main_GetRecordAddLink(string ContentName, string PresetNameValueList, bool AllowPaste = false) {
            return main_GetRecordAddLink2(genericController.encodeText(ContentName), genericController.encodeText(PresetNameValueList), AllowPaste, cpCore.doc.authContext.isEditing(ContentName));
        }
        //
        //========================================================================
        // main_GetRecordAddLink2
        //
        //   Returns a string of add tags for the Content Definition included, and all
        //   child contents of that area.
        //
        //   iContentName The content for this link
        //   iPresetNameValueList The sql equivalent used to select the record.
        //           translates to name0=value0,name1=value1.. pairs separated by ,
        //
        //   LowestRootMenu - The Menu in the flyout structure that is the furthest down
        //   in the chain that the user has content access to. This is so a content manager
        //   does not have to navigate deep into a structure to main_Get to content he can
        //   edit.
        //   Basically, the entire menu is created down from the MenuName, and populated
        //   with all the entiries this user has access to. The LowestRequiredMenuName is
        //   is returned from the _branch routine, and that is to root on-which the
        //   main_GetMenu uses
        //========================================================================
        //
        public string main_GetRecordAddLink2(string ContentName, string PresetNameValueList, bool AllowPaste, bool IsEditing) {
            string tempmain_GetRecordAddLink2 = null;
            try {
                //
                //If Not (true) Then Exit Function
                //
                int ParentID = 0;
                string BufferString = null;
                string MethodName = null;
                string iContentName = null;
                int iContentID = 0;
                string iPresetNameValueList = null;
                string MenuName = null;
                bool MenuHasBranches = false;
                string LowestRequiredMenuName = string.Empty;
                string ClipBoard = null;
                string PasteLink = string.Empty;
                int Position = 0;
                string[] ClipBoardArray = null;
                int ClipboardContentID = 0;
                int ClipChildRecordID = 0;
                bool iAllowPaste = false;
                bool useFlyout = false;
                int csChildContent = 0;
                string Link = null;
                //
                MethodName = "main_GetRecordAddLink";
                //
                tempmain_GetRecordAddLink2 = "";
                if (IsEditing) {
                    iContentName = genericController.encodeText(ContentName);
                    iPresetNameValueList = genericController.encodeText(PresetNameValueList);
                    iPresetNameValueList = genericController.vbReplace(iPresetNameValueList, "&", ",");
                    iAllowPaste = genericController.EncodeBoolean(AllowPaste);

                    if (string.IsNullOrEmpty(iContentName)) {
                        throw (new ApplicationException("Method called with blank ContentName")); // handleLegacyError14(MethodName, "")
                    } else {
                        iContentID = Models.Complex.cdefModel.getContentId(cpCore, iContentName);
                        csChildContent = cpCore.db.csOpen("Content", "ParentID=" + iContentID,,,,,, "id");
                        useFlyout = cpCore.db.csOk(csChildContent);
                        cpCore.db.csClose(ref csChildContent);
                        //
                        if (!useFlyout) {
                            Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?cid=" + iContentID + "&af=4&aa=2&ad=1";
                            if (!string.IsNullOrEmpty(PresetNameValueList)) {
                                Link = Link + "&wc=" + genericController.EncodeRequestVariable(PresetNameValueList);
                            }
                            tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + "<a"
                                + " TabIndex=-1"
                                + " href=\"" + genericController.encodeHTML(Link) + "\"";
                            tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + "><img"
                                + " src=\"/ccLib/images/IconContentAdd.gif\""
                                + " border=\"0\""
                                + " alt=\"Add record\""
                                + " title=\"Add record\""
                                + " align=\"absmiddle\""
                                + "></a>";
                        } else {
                            //
                            MenuName = genericController.GetRandomInteger().ToString();
                            cpCore.menuFlyout.menu_AddEntry(MenuName,, "/ccLib/images/IconContentAdd.gif",,,, "stylesheet", "stylesheethover");
                            LowestRequiredMenuName = main_GetRecordAddLink_AddMenuEntry(iContentName, iPresetNameValueList, "", MenuName, MenuName);
                        }
                        //
                        // Add in the paste entry, if needed
                        //
                        if (iAllowPaste) {
                            ClipBoard = cpCore.visitProperty.getText("Clipboard", "");
                            if (!string.IsNullOrEmpty(ClipBoard)) {
                                Position = genericController.vbInstr(1, ClipBoard, ".");
                                if (Position != 0) {
                                    ClipBoardArray = ClipBoard.Split('.');
                                    if (ClipBoardArray.GetUpperBound(0) > 0) {
                                        ClipboardContentID = genericController.EncodeInteger(ClipBoardArray[0]);
                                        ClipChildRecordID = genericController.EncodeInteger(ClipBoardArray[1]);
                                        //iContentID = main_GetContentID(iContentName)
                                        if (Models.Complex.cdefModel.isWithinContent(cpCore, ClipboardContentID, iContentID)) {
                                            if (genericController.vbInstr(1, iPresetNameValueList, "PARENTID=", Microsoft.VisualBasic.Constants.vbTextCompare) != 0) {
                                                //
                                                // must test for main_IsChildRecord
                                                //
                                                BufferString = iPresetNameValueList;
                                                BufferString = genericController.vbReplace(BufferString, "(", "");
                                                BufferString = genericController.vbReplace(BufferString, ")", "");
                                                BufferString = genericController.vbReplace(BufferString, ",", "&");
                                                ParentID = genericController.EncodeInteger(genericController.main_GetNameValue_Internal(cpCore, BufferString, "Parentid"));
                                            }


                                            if ((ParentID != 0) & (!pageContentController.isChildRecord(cpCore, iContentName, ParentID, ClipChildRecordID))) {
                                                //
                                                // Can not paste as child of itself
                                                //
                                                PasteLink = cpCore.webServer.requestPage + "?" + cpCore.doc.refreshQueryString;
                                                PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePaste, "1", true);
                                                PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteParentContentID, iContentID.ToString(), true);
                                                PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteParentRecordID, ParentID.ToString(), true);
                                                PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteFieldList, iPresetNameValueList, true);
                                                tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + "<a class=\"ccRecordCutLink\" TabIndex=\"-1\" href=\"" + genericController.encodeHTML(PasteLink) + "\"><img src=\"/ccLib/images/ContentPaste.gif\" border=\"0\" alt=\"Paste record in clipboard here\" title=\"Paste record in clipboard here\" align=\"absmiddle\"></a>";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //
                        // Add in the available flyout Navigator Entries
                        //
                        if (!string.IsNullOrEmpty(LowestRequiredMenuName)) {
                            tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + cpCore.menuFlyout.getMenu(LowestRequiredMenuName, 0);
                            tempmain_GetRecordAddLink2 = genericController.vbReplace(tempmain_GetRecordAddLink2, "class=\"ccFlyoutButton\" ", "", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
                            if (!string.IsNullOrEmpty(PasteLink)) {
                                tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + "<a TabIndex=-1 href=\"" + genericController.encodeHTML(PasteLink) + "\"><img src=\"/ccLib/images/ContentPaste.gif\" border=\"0\" alt=\"Paste content from clipboard\" align=\"absmiddle\"></a>";
                            }
                        }
                        //
                        // Help link if enabled
                        //
                        string helpLink = "";
                        //helpLink = main_GetHelpLink(6, "Adding " & iContentName, "Turn on Edit icons by checking 'Edit' in the tools panel, and click apply.<br><br><img src=""/ccLib/images/IconContentAdd.gif"" " & IconWidthHeight & " style=""vertical-align:middle""> Add-Content icon<br><br>Add-Content icons appear in your content. Click them to add content.")
                        tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + helpLink;
                        if (!string.IsNullOrEmpty(tempmain_GetRecordAddLink2)) {
                            tempmain_GetRecordAddLink2 = ""
                                + Environment.NewLine + "\t" + "<div style=\"display:inline;\">"
                                + genericController.htmlIndent(tempmain_GetRecordAddLink2) + Environment.NewLine + "\t" + "</div>";
                        }
                        //
                        // ----- Add the flyout panels to the content to return
                        //       This must be here so if the call is made after main_ClosePage, the panels will still deliver
                        //
                        if (!string.IsNullOrEmpty(LowestRequiredMenuName)) {
                            tempmain_GetRecordAddLink2 = tempmain_GetRecordAddLink2 + cpCore.menuFlyout.menu_GetClose();
                            if (genericController.vbInstr(1, tempmain_GetRecordAddLink2, "IconContentAdd.gif", Microsoft.VisualBasic.Constants.vbTextCompare) != 0) {
                                tempmain_GetRecordAddLink2 = genericController.vbReplace(tempmain_GetRecordAddLink2, "IconContentAdd.gif\" ", "IconContentAdd.gif\" align=\"absmiddle\" ");
                            }
                        }
                        tempmain_GetRecordAddLink2 = genericController.vbReplace(tempmain_GetRecordAddLink2, "target=", "xtarget=", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
                    }
                }
                //
                return tempmain_GetRecordAddLink2;
                //
                // ----- Error Trap
                //
            } catch {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError18(MethodName)
                                                                    //
            return tempmain_GetRecordAddLink2;
        }
        //
        //========================================================================
        // main_GetRecordAddLink_AddMenuEntry( ContentName, PresetNameValueList, ContentNameList, MenuName )
        //
        //   adds an add entry for the content name, and all the child content
        //   returns the MenuName of the lowest branch that has valid
        //   Navigator Entries.
        //
        //   ContentName The content for this link
        //   PresetNameValueList The sql equivalent used to select the record.
        //           translates to (name0=value0)&(name1=value1).. pairs separated by &
        //   ContentNameList is a comma separated list of names of the content included so far
        //   MenuName is the name of the root branch, for flyout menu
        //
        //   IsMember(), main_IsAuthenticated() And Member_AllowLinkAuthoring must already be checked
        //========================================================================
        //
        private string main_GetRecordAddLink_AddMenuEntry(string ContentName, string PresetNameValueList, string ContentNameList, string MenuName, string ParentMenuName) {
            string result = "";
            string Copy = null;
            int CS = 0;
            string SQL = null;
            int csChildContent = 0;
            int ContentID = 0;
            string Link = null;
            string MyContentNameList = null;
            string ButtonCaption = null;
            bool ContentRecordFound = false;
            bool ContentAllowAdd = false;
            bool GroupRulesAllowAdd = false;
            DateTime MemberRulesDateExpires = default(DateTime);
            bool MemberRulesAllow = false;
            int ChildMenuButtonCount = 0;
            string ChildMenuName = null;
            string ChildContentName = null;
            //
            Link = "";
            MyContentNameList = ContentNameList;
            if (string.IsNullOrEmpty(ContentName)) {
                throw (new ApplicationException("main_GetRecordAddLink, ContentName is empty")); // handleLegacyError14(MethodName, "")
            } else {
                if (MyContentNameList.IndexOf("," + genericController.vbUCase(ContentName) + ",") + 1 >= 0) {
                    throw (new ApplicationException("result , Content Child [" + ContentName + "] is one of its own parents")); // handleLegacyError14(MethodName, "")
                } else {
                    MyContentNameList = MyContentNameList + "," + genericController.vbUCase(ContentName) + ",";
                    //
                    // ----- Select the Content Record for the Menu Entry selected
                    //
                    ContentRecordFound = false;
                    if (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                        //
                        // ----- admin member, they have access, main_Get ContentID and set markers true
                        //
                        SQL = "SELECT ID as ContentID, AllowAdd as ContentAllowAdd, 1 as GroupRulesAllowAdd, null as MemberRulesDateExpires"
                            + " FROM ccContent"
                            + " WHERE ("
                            + " (ccContent.Name=" + cpCore.db.encodeSQLText(ContentName) + ")"
                            + " AND(ccContent.active<>0)"
                            + " );";
                        CS = cpCore.db.csOpenSql(SQL);
                        if (cpCore.db.csOk(CS)) {
                            //
                            // Entry was found
                            //
                            ContentRecordFound = true;
                            ContentID = cpCore.db.csGetInteger(CS, "ContentID");
                            ContentAllowAdd = cpCore.db.csGetBoolean(CS, "ContentAllowAdd");
                            GroupRulesAllowAdd = true;
                            MemberRulesDateExpires = DateTime.MinValue;
                            MemberRulesAllow = true;
                        }
                        cpCore.db.csClose(ref CS);
                    } else {
                        //
                        // non-admin member, first check if they have access and main_Get true markers
                        //
                        SQL = "SELECT ccContent.ID as ContentID, ccContent.AllowAdd as ContentAllowAdd, ccGroupRules.AllowAdd as GroupRulesAllowAdd, ccMemberRules.DateExpires as MemberRulesDateExpires"
                            + " FROM (((ccContent"
                                + " LEFT JOIN ccGroupRules ON ccGroupRules.ContentID=ccContent.ID)"
                                + " LEFT JOIN ccgroups ON ccGroupRules.GroupID=ccgroups.ID)"
                                + " LEFT JOIN ccMemberRules ON ccgroups.ID=ccMemberRules.GroupID)"
                                + " LEFT JOIN ccMembers ON ccMemberRules.MemberID=ccMembers.ID"
                            + " WHERE ("
                            + " (ccContent.Name=" + cpCore.db.encodeSQLText(ContentName) + ")"
                            + " AND(ccContent.active<>0)"
                            + " AND(ccGroupRules.active<>0)"
                            + " AND(ccMemberRules.active<>0)"
                            + " AND((ccMemberRules.DateExpires is Null)or(ccMemberRules.DateExpires>" + cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime) + "))"
                            + " AND(ccgroups.active<>0)"
                            + " AND(ccMembers.active<>0)"
                            + " AND(ccMembers.ID=" + cpCore.doc.authContext.user.id + ")"
                            + " );";
                        CS = cpCore.db.csOpenSql(SQL);
                        if (cpCore.db.csOk(CS)) {
                            //
                            // ----- Entry was found, member has some kind of access
                            //
                            ContentRecordFound = true;
                            ContentID = cpCore.db.csGetInteger(CS, "ContentID");
                            ContentAllowAdd = cpCore.db.csGetBoolean(CS, "ContentAllowAdd");
                            GroupRulesAllowAdd = cpCore.db.csGetBoolean(CS, "GroupRulesAllowAdd");
                            MemberRulesDateExpires = cpCore.db.csGetDate(CS, "MemberRulesDateExpires");
                            MemberRulesAllow = false;
                            if (MemberRulesDateExpires == DateTime.MinValue) {
                                MemberRulesAllow = true;
                            } else if (MemberRulesDateExpires > cpCore.doc.profileStartTime) {
                                MemberRulesAllow = true;
                            }
                        } else {
                            //
                            // ----- No entry found, this member does not have access, just main_Get ContentID
                            //
                            ContentRecordFound = true;
                            ContentID = Models.Complex.cdefModel.getContentId(cpCore, ContentName);
                            ContentAllowAdd = false;
                            GroupRulesAllowAdd = false;
                            MemberRulesAllow = false;
                        }
                        cpCore.db.csClose(ref CS);
                    }
                    if (ContentRecordFound) {
                        //
                        // Add the Menu Entry* to the current menu (MenuName)
                        //
                        Link = "";
                        ButtonCaption = ContentName;
                        result = MenuName;
                        if (ContentAllowAdd && GroupRulesAllowAdd && MemberRulesAllow) {
                            Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?cid=" + ContentID + "&af=4&aa=2&ad=1";
                            if (!string.IsNullOrEmpty(PresetNameValueList)) {
                                string NameValueList = PresetNameValueList;
                                Link = Link + "&wc=" + genericController.EncodeRequestVariable(PresetNameValueList);
                            }
                        }
                        cpCore.menuFlyout.menu_AddEntry(MenuName + ":" + ContentName, ParentMenuName,,, Link, ButtonCaption, "", "", true);
                        //
                        // Create child submenu if Child Entries found
                        //
                        csChildContent = cpCore.db.csOpen("Content", "ParentID=" + ContentID,,,,,, "name");
                        if (!cpCore.db.csOk(csChildContent)) {
                            //
                            // No child menu
                            //
                        } else {
                            //
                            // Add the child menu
                            //
                            ChildMenuName = MenuName + ":" + ContentName;
                            ChildMenuButtonCount = 0;
                            //
                            // ----- Create the ChildPanel with all Children found
                            //
                            while (cpCore.db.csOk(csChildContent)) {
                                ChildContentName = cpCore.db.csGetText(csChildContent, "name");
                                Copy = main_GetRecordAddLink_AddMenuEntry(ChildContentName, PresetNameValueList, MyContentNameList, MenuName, ParentMenuName);
                                if (!string.IsNullOrEmpty(Copy)) {
                                    ChildMenuButtonCount = ChildMenuButtonCount + 1;
                                }
                                if ((string.IsNullOrEmpty(result)) && (!string.IsNullOrEmpty(Copy))) {
                                    result = Copy;
                                }
                                cpCore.db.csGoNext(csChildContent);
                            }
                        }
                    }
                }
                cpCore.db.csClose(ref csChildContent);
            }
            return result;
        }
        //
        //========================================================================
        //   main_GetPanel( Panel, Optional StylePanel, Optional StyleHilite, Optional StyleShadow, Optional Width, Optional Padding, Optional HeightMin) As String
        // Return a panel with the input as center
        //========================================================================
        //
        public string main_GetPanel(string Panel, string StylePanel = "", string StyleHilite = "ccPanelHilite", string StyleShadow = "ccPanelShadow", string Width = "100%", int Padding = 5, int HeightMin = 1) {
            string tempmain_GetPanel = null;
            string ContentPanelWidth = null;
            string MethodName = null;
            string MyStylePanel = null;
            string MyStyleHilite = null;
            string MyStyleShadow = null;
            string MyWidth = null;
            string MyPadding = null;
            string MyHeightMin = null;
            string s0 = null;
            string s1 = null;
            string s2 = null;
            string s3 = null;
            string s4 = null;
            string contentPanelWidthStyle = null;
            //
            MethodName = "main_GetPanelTop";
            //
            MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel");
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite");
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow");
            MyWidth = genericController.encodeEmptyText(Width, "100%");
            MyPadding = Padding.ToString();
            MyHeightMin = HeightMin.ToString();
            //
            if (genericController.vbIsNumeric(MyWidth)) {
                ContentPanelWidth = (int.Parse(MyWidth) - 2).ToString();
                contentPanelWidthStyle = ContentPanelWidth + "px";
            } else {
                ContentPanelWidth = "100%";
                contentPanelWidthStyle = ContentPanelWidth;
            }
            //
            //
            //
            s0 = ""
                + "\r" + "<td style=\"padding:" + MyPadding + "px;vertical-align:top\" class=\"" + MyStylePanel + "\">"
                + genericController.htmlIndent(genericController.encodeText(Panel)) + "\r" + "</td>"
                + "";
            //
            s1 = ""
                + "\r" + "<tr>"
                + genericController.htmlIndent(s0) + "\r" + "</tr>"
                + "";
            s2 = ""
                + "\r" + "<table style=\"width:" + contentPanelWidthStyle + ";border:0px;\" class=\"" + MyStylePanel + "\" cellspacing=\"0\">"
                + genericController.htmlIndent(s1) + "\r" + "</table>"
                + "";
            s3 = ""
                + "\r" + "<td width=\"1\" height=\"" + MyHeightMin + "\" class=\"" + MyStyleHilite + "\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"" + MyHeightMin + "\" width=\"1\" ></td>"
                + "\r" + "<td width=\"" + ContentPanelWidth + "\" valign=\"top\" align=\"left\" class=\"" + MyStylePanel + "\">"
                + genericController.htmlIndent(s2) + "\r" + "</td>"
                + "\r" + "<td width=\"1\" class=\"" + MyStyleShadow + "\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"1\" ></td>"
                + "";
            s4 = ""
                + "\r" + "<tr>"
                + cr2 + "<td colspan=\"3\" class=\"" + MyStyleHilite + "\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"" + MyWidth + "\" ></td>"
                + "\r" + "</tr>"
                + "\r" + "<tr>"
                + genericController.htmlIndent(s3) + "\r" + "</tr>"
                + "\r" + "<tr>"
                + cr2 + "<td colspan=\"3\" class=\"" + MyStyleShadow + "\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"" + MyWidth + "\" ></td>"
                + "\r" + "</tr>"
                + "";
            tempmain_GetPanel = ""
                + "\r" + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"" + MyWidth + "\" class=\"" + MyStylePanel + "\">"
                + genericController.htmlIndent(s4) + "\r" + "</table>"
                + "";
            return tempmain_GetPanel;
        }
        //
        //========================================================================
        //   main_GetPanel( Panel, Optional StylePanel, Optional StyleHilite, Optional StyleShadow, Optional Width, Optional Padding, Optional HeightMin) As String
        // Return a panel with the input as center
        //========================================================================
        //
        public string main_GetReversePanel(string Panel, string StylePanel = "", string StyleHilite = "ccPanelShadow", string StyleShadow = "ccPanelHilite", string Width = "", string Padding = "", string HeightMin = "") {
            string MyStyleHilite = null;
            string MyStyleShadow = null;
            //
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelShadow");
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelHilite");

            return main_GetPanelTop(StylePanel, MyStyleHilite, MyStyleShadow, Width, Padding, HeightMin) + genericController.encodeText(Panel) + main_GetPanelBottom(StylePanel, MyStyleHilite, MyStyleShadow, Width, Padding);
        }
        //
        //========================================================================
        // Return a panel header with the header message reversed out of the left
        //========================================================================
        //
        public string main_GetPanelHeader(string HeaderMessage, string RightSideMessage = "") {
            string iHeaderMessage = null;
            string iRightSideMessage = null;
            adminUIController Adminui = new adminUIController(cpCore);
            //
            //If Not (true) Then Exit Function
            //
            iHeaderMessage = genericController.encodeText(HeaderMessage);
            iRightSideMessage = genericController.encodeEmptyText(RightSideMessage, cpCore.doc.profileStartTime.ToString("G"));
            return Adminui.GetHeader(iHeaderMessage, iRightSideMessage);
        }

        //
        //========================================================================
        // Prints the top of display panel
        //   Must be closed with PrintPanelBottom
        //========================================================================
        //
        public string main_GetPanelTop(string StylePanel = "", string StyleHilite = "", string StyleShadow = "", string Width = "", string Padding = "", string HeightMin = "") {
            string tempmain_GetPanelTop = null;
            string ContentPanelWidth = null;
            string MethodName = null;
            string MyStylePanel = null;
            string MyStyleHilite = null;
            string MyStyleShadow = null;
            string MyWidth = null;
            string MyPadding = null;
            string MyHeightMin = null;
            //
            tempmain_GetPanelTop = "";
            MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel");
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite");
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow");
            MyWidth = genericController.encodeEmptyText(Width, "100%");
            MyPadding = genericController.encodeEmptyText(Padding, "5");
            MyHeightMin = genericController.encodeEmptyText(HeightMin, "1");
            MethodName = "main_GetPanelTop";
            if (genericController.vbIsNumeric(MyWidth)) {
                ContentPanelWidth = (int.Parse(MyWidth) - 2).ToString();
            } else {
                ContentPanelWidth = "100%";
            }
            tempmain_GetPanelTop = tempmain_GetPanelTop + "\r" + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"" + MyWidth + "\" class=\"" + MyStylePanel + "\">";
            //
            // --- top hilite row
            //
            tempmain_GetPanelTop = tempmain_GetPanelTop + cr2 + "<tr>"
                + cr3 + "<td colspan=\"3\" class=\"" + MyStyleHilite + "\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"" + MyWidth + "\" ></td>"
                + cr2 + "</tr>";
            //
            // --- center row with Panel
            //
            tempmain_GetPanelTop = tempmain_GetPanelTop + cr2 + "<tr>"
                + cr3 + "<td width=\"1\" height=\"" + MyHeightMin + "\" class=\"" + MyStyleHilite + "\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"" + MyHeightMin + "\" width=\"1\" ></td>"
                + cr3 + "<td width=\"" + ContentPanelWidth + "\" valign=\"top\" align=\"left\" class=\"" + MyStylePanel + "\">"
                + cr4 + "<table border=\"0\" cellpadding=\"" + MyPadding + "\" cellspacing=\"0\" width=\"" + ContentPanelWidth + "\" class=\"" + MyStylePanel + "\">"
                + cr5 + "<tr>"
                + cr6 + "<td valign=\"top\" class=\"" + MyStylePanel + "\"><Span class=\"" + MyStylePanel + "\">";
            return tempmain_GetPanelTop;
        }
        //
        //========================================================================
        // Return a panel with the input as center
        //========================================================================
        //
        public string main_GetPanelBottom(string StylePanel = "", string StyleHilite = "", string StyleShadow = "", string Width = "", string Padding = "") {
            string result = string.Empty;
            try {
                //Dim MyStylePanel As String
                //Dim MyStyleHilite As String
                string MyStyleShadow = null;
                string MyWidth = null;
                //Dim MyPadding As String
                //
                //MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel")
                //MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite")
                MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow");
                MyWidth = genericController.encodeEmptyText(Width, "100%");
                //MyPadding = genericController.encodeEmptyText(Padding, "5")
                //
                result = result + cr6 + "</span></td>"
                    + cr5 + "</tr>"
                    + cr4 + "</table>"
                    + cr3 + "</td>"
                    + cr3 + "<td width=\"1\" class=\"" + MyStyleShadow + "\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"1\" ></td>"
                    + cr2 + "</tr>"
                    + cr2 + "<tr>"
                    + cr3 + "<td colspan=\"3\" class=\"" + MyStyleShadow + "\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" height=\"1\" width=\"" + MyWidth + "\" ></td>"
                    + cr2 + "</tr>"
                    + "\r" + "</table>";
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        public string main_GetPanelButtons(string ButtonValueList, string ButtonName, string PanelWidth = "", string PanelHeightMin = "") {
            adminUIController Adminui = new adminUIController(cpCore);
            return Adminui.GetButtonBar(Adminui.GetButtonsFromList(ButtonValueList, true, true, ButtonName), "");
        }
        //
        //
        //
        public string main_GetPanelRev(string PanelContent, string PanelWidth = "", string PanelHeightMin = "") {
            return main_GetPanel(PanelContent, "ccPanel", "ccPanelShadow", "ccPanelHilite", PanelWidth, 2, genericController.EncodeInteger(PanelHeightMin));
        }
        //
        //
        //
        public string main_GetPanelInput(string PanelContent, string PanelWidth = "", string PanelHeightMin = "1") {
            return main_GetPanel(PanelContent, "ccPanelInput", "ccPanelShadow", "ccPanelHilite", PanelWidth, 2, genericController.EncodeInteger(PanelHeightMin));
        }
        //
        //========================================================================
        // Print the tools panel at the bottom of the page
        //========================================================================
        //
        public string main_GetToolsPanel() {
            string result = string.Empty;
            try {
                string copyNameValue = null;
                string CopyName = null;
                string copyValue = null;
                string[] copyNameValueSplit = null;
                int VisitMin = 0;
                int VisitHrs = 0;
                int VisitSec = 0;
                string DebugPanel = string.Empty;
                string Copy = null;
                string[] CopySplit = null;
                int Ptr = 0;
                string EditTagID = null;
                string QuickEditTagID = null;
                string AdvancedEditTagID = null;
                string WorkflowTagID = null;
                string Tag = null;
                string MethodName = null;
                string TagID = null;
                stringBuilderLegacyController ToolsPanel = null;
                string OptionsPanel = string.Empty;
                stringBuilderLegacyController LinkPanel = null;
                string LoginPanel = string.Empty;
                bool iValueBoolean = false;
                string WorkingQueryString = null;
                string BubbleCopy = null;
                stringBuilderLegacyController AnotherPanel = null;
                adminUIController Adminui = new adminUIController(cpCore);
                bool ShowLegacyToolsPanel = false;
                string QS = null;
                //
                MethodName = "main_GetToolsPanel";
                //
                if (cpCore.doc.authContext.user.AllowToolsPanel) {
                    ShowLegacyToolsPanel = cpCore.siteProperties.getBoolean("AllowLegacyToolsPanel", true);
                    //
                    // --- Link Panel - used for both Legacy Tools Panel, and without it
                    //
                    LinkPanel = new stringBuilderLegacyController();
                    LinkPanel.Add(SpanClassAdminSmall);
                    LinkPanel.Add("Contensive " + cpCore.codeVersion() + " | ");
                    LinkPanel.Add(cpCore.doc.profileStartTime.ToString("G") + " | ");
                    LinkPanel.Add("<a class=\"ccAdminLink\" target=\"_blank\" href=\"http://support.Contensive.com/\">Support</A> | ");
                    LinkPanel.Add("<a class=\"ccAdminLink\" href=\"" + genericController.encodeHTML("/" + cpCore.serverConfig.appConfig.adminRoute) + "\">Admin Home</A> | ");
                    LinkPanel.Add("<a class=\"ccAdminLink\" href=\"" + genericController.encodeHTML("http://" + cpCore.webServer.requestDomain) + "\">Public Home</A> | ");
                    LinkPanel.Add("<a class=\"ccAdminLink\" target=\"_blank\" href=\"" + genericController.encodeHTML("/" + cpCore.serverConfig.appConfig.adminRoute + "?" + RequestNameHardCodedPage + "=" + HardCodedPageMyProfile) + "\">My Profile</A> | ");
                    if (cpCore.siteProperties.getBoolean("AllowMobileTemplates", false)) {
                        if (cpCore.doc.authContext.visit.Mobile) {
                            QS = cpCore.doc.refreshQueryString;
                            QS = genericController.ModifyQueryString(QS, "method", "forcenonmobile");
                            LinkPanel.Add("<a class=\"ccAdminLink\" href=\"?" + QS + "\">Non-Mobile Version</A> | ");
                        } else {
                            QS = cpCore.doc.refreshQueryString;
                            QS = genericController.ModifyQueryString(QS, "method", "forcemobile");
                            LinkPanel.Add("<a class=\"ccAdminLink\" href=\"?" + QS + "\">Mobile Version</A> | ");
                        }
                    }
                    LinkPanel.Add("</span>");
                    //
                    if (ShowLegacyToolsPanel) {
                        ToolsPanel = new stringBuilderLegacyController();
                        WorkingQueryString = genericController.ModifyQueryString(cpCore.doc.refreshQueryString, "ma", "", false);
                        //
                        // ----- Tools Panel Caption
                        //
                        string helpLink = "";
                        //helpLink = main_GetHelpLink("2", "Contensive Tools Panel", BubbleCopy)
                        BubbleCopy = "Use the Tools Panel to enable features such as editing and debugging tools. It also includes links to the admin site, the support site and the My Profile page.";
                        result = result + main_GetPanelHeader("Contensive Tools Panel" + helpLink);
                        //
                        ToolsPanel.Add(cpCore.html.html_GetFormStart(WorkingQueryString));
                        ToolsPanel.Add(cpCore.html.html_GetFormInputHidden("Type", FormTypeToolsPanel));
                        //
                        if (true) {
                            //
                            // ----- Create the Options Panel
                            //
                            //PathsContentID = main_GetContentID("Paths")
                            //                '
                            //                ' Allow Help Links
                            //                '
                            //                iValueBoolean = visitProperty.getboolean("AllowHelpIcon")
                            //                TagID =  "AllowHelpIcon"
                            //                OptionsPanel = OptionsPanel & "" _
                            //                    & CR & "<div class=""ccAdminSmall"">" _
                            //                    & cr2 & "<LABEL for=""" & TagID & """>" & main_GetFormInputCheckBox2(TagID, iValueBoolean, TagID) & "&nbsp;Help</LABEL>" _
                            //                    & CR & "</div>"
                            //
                            EditTagID = "AllowEditing";
                            QuickEditTagID = "AllowQuickEditor";
                            AdvancedEditTagID = "AllowAdvancedEditor";
                            WorkflowTagID = "AllowWorkflowRendering";
                            //
                            // Edit
                            //
                            helpLink = "";
                            //helpLink = main_GetHelpLink(7, "Enable Editing", "Display the edit tools for basic content, such as pages, copy and sections. ")
                            iValueBoolean = cpCore.visitProperty.getBoolean("AllowEditing");
                            Tag = cpCore.html.html_GetFormInputCheckBox2(EditTagID, iValueBoolean, EditTagID);
                            Tag = genericController.vbReplace(Tag, ">", " onClick=\"document.getElementById('" + QuickEditTagID + "').checked=false;document.getElementById('" + AdvancedEditTagID + "').checked=false;\">");
                            OptionsPanel = OptionsPanel + "\r" + "<div class=\"ccAdminSmall\">"
                            + cr2 + "<LABEL for=\"" + EditTagID + "\">" + Tag + "&nbsp;Edit</LABEL>" + helpLink + "\r" + "</div>";
                            //
                            // Quick Edit
                            //
                            helpLink = "";
                            //helpLink = main_GetHelpLink(8, "Enable Quick Edit", "Display the quick editor to edit the main page content.")
                            iValueBoolean = cpCore.visitProperty.getBoolean("AllowQuickEditor");
                            Tag = cpCore.html.html_GetFormInputCheckBox2(QuickEditTagID, iValueBoolean, QuickEditTagID);
                            Tag = genericController.vbReplace(Tag, ">", " onClick=\"document.getElementById('" + EditTagID + "').checked=false;document.getElementById('" + AdvancedEditTagID + "').checked=false;\">");
                            OptionsPanel = OptionsPanel + "\r" + "<div class=\"ccAdminSmall\">"
                            + cr2 + "<LABEL for=\"" + QuickEditTagID + "\">" + Tag + "&nbsp;Quick Edit</LABEL>" + helpLink + "\r" + "</div>";
                            //
                            // Advanced Edit
                            //
                            helpLink = "";
                            //helpLink = main_GetHelpLink(0, "Enable Advanced Edit", "Display the edit tools for advanced content, such as templates and add-ons. Basic content edit tools are also displayed.")
                            iValueBoolean = cpCore.visitProperty.getBoolean("AllowAdvancedEditor");
                            Tag = cpCore.html.html_GetFormInputCheckBox2(AdvancedEditTagID, iValueBoolean, AdvancedEditTagID);
                            Tag = genericController.vbReplace(Tag, ">", " onClick=\"document.getElementById('" + QuickEditTagID + "').checked=false;document.getElementById('" + EditTagID + "').checked=false;\">");
                            OptionsPanel = OptionsPanel + "\r" + "<div class=\"ccAdminSmall\">"
                            + cr2 + "<LABEL for=\"" + AdvancedEditTagID + "\">" + Tag + "&nbsp;Advanced Edit</LABEL>" + helpLink + "\r" + "</div>";
                            //
                            // Workflow Authoring Render Mode
                            //
                            helpLink = "";
                            //helpLink = main_GetHelpLink(9, "Enable Workflow Rendering", "Control the display of workflow rendering. With workflow rendering enabled, any changes saved to content records that have not been published will be visible for your review.")
                            //If cpCore.siteProperties.allowWorkflowAuthoring Then
                            //    iValueBoolean = cpCore.visitProperty.getBoolean("AllowWorkflowRendering")
                            //    Tag = cpCore.html.html_GetFormInputCheckBox2(WorkflowTagID, iValueBoolean, WorkflowTagID)
                            //    OptionsPanel = OptionsPanel _
                            //    & cr & "<div class=""ccAdminSmall"">" _
                            //    & cr2 & "<LABEL for=""" & WorkflowTagID & """>" & Tag & "&nbsp;Render Workflow Authoring Changes</LABEL>" & helpLink _
                            //    & cr & "</div>"
                            //End If
                            helpLink = "";
                            iValueBoolean = cpCore.visitProperty.getBoolean("AllowDebugging");
                            TagID = "AllowDebugging";
                            Tag = cpCore.html.html_GetFormInputCheckBox2(TagID, iValueBoolean, TagID);
                            OptionsPanel = OptionsPanel + "\r" + "<div class=\"ccAdminSmall\">"
                            + cr2 + "<LABEL for=\"" + TagID + "\">" + Tag + "&nbsp;Debug</LABEL>" + helpLink + "\r" + "</div>";
                            //'
                            //' Create Path Block Row
                            //'
                            //If cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore) Then
                            //    TagID = "CreatePathBlock"
                            //    If cpCore.siteProperties.allowPathBlocking Then
                            //        '
                            //        ' Path blocking allowed
                            //        '
                            //        'OptionsPanel = OptionsPanel & SpanClassAdminSmall & "<LABEL for=""" & TagID & """>"
                            //        CS = cpCore.db.cs_open("Paths", "name=" & cpCore.db.encodeSQLText(cpCore.webServer.requestPath), , , , , , "ID")
                            //        If cpCore.db.cs_ok(CS) Then
                            //            PathID = (cpCore.db.cs_getInteger(CS, "ID"))
                            //        End If
                            //        Call cpCore.db.cs_Close(CS)
                            //        If PathID <> 0 Then
                            //            '
                            //            ' Path is blocked
                            //            '
                            //            Tag = cpCore.html.html_GetFormInputCheckBox2(TagID, True, TagID) & "&nbsp;Path is blocked [" & cpCore.webServer.requestPath & "] [<a href=""" & genericController.encodeHTML("/" & cpCore.serverconfig.appconfig.adminRoute & "?af=" & AdminFormEdit & "&id=" & PathID & "&cid=" & models.complex.cdefmodel.getcontentid(cpcore,"paths") & "&ad=1") & """ target=""_blank"">edit</a>]</LABEL>"
                            //        Else
                            //            '
                            //            ' Path is not blocked
                            //            '
                            //            Tag = cpCore.html.html_GetFormInputCheckBox2(TagID, False, TagID) & "&nbsp;Block this path [" & cpCore.webServer.requestPath & "]</LABEL>"
                            //        End If
                            //        helpLink = ""
                            //        'helpLink = main_GetHelpLink(10, "Enable Debugging", "Debugging is a developer only debugging tool. With Debugging enabled, ccLib.TestPoints(...) will print, ErrorTrapping will be displayed, redirections are blocked, and more.")
                            //        OptionsPanel = OptionsPanel _
                            //        & cr & "<div class=""ccAdminSmall"">" _
                            //        & cr2 & "<LABEL for=""" & TagID & """>" & Tag & "</LABEL>" & helpLink _
                            //        & cr & "</div>"
                            //    End If
                            //End If
                            //
                            // Buttons
                            //
                            OptionsPanel = OptionsPanel + ""
                            + "\r" + "<div class=\"ccButtonCon\">"
                            + cr2 + "<input type=submit name=" + "mb value=\"" + ButtonApply + "\">"
                            + "\r" + "</div>"
                            + "";
                        }
                        //
                        // ----- Create the Login Panel
                        //
                        if (string.IsNullOrEmpty(cpCore.doc.authContext.user.name.Trim(' '))) {
                            Copy = "You are logged in as member #" + cpCore.doc.authContext.user.id + ".";
                        } else {
                            Copy = "You are logged in as " + cpCore.doc.authContext.user.name + ".";
                        }
                        LoginPanel = LoginPanel + ""
                        + "\r" + "<div class=\"ccAdminSmall\">"
                        + cr2 + Copy + ""
                        + "\r" + "</div>";
                        //
                        // Username
                        //
                        string Caption = null;
                        if (cpCore.siteProperties.getBoolean("allowEmailLogin", false)) {
                            Caption = "Username&nbsp;or&nbsp;Email";
                        } else {
                            Caption = "Username";
                        }
                        TagID = "Username";
                        LoginPanel = LoginPanel + ""
                        + "\r" + "<div class=\"ccAdminSmall\">"
                        + cr2 + "<LABEL for=\"" + TagID + "\">" + cpCore.html.html_GetFormInputText2(TagID, "", 1, 30, TagID, false) + "&nbsp;" + Caption + "</LABEL>"
                        + "\r" + "</div>";
                        //
                        // Username
                        //
                        if (cpCore.siteProperties.getBoolean("allownopasswordLogin", false)) {
                            Caption = "Password&nbsp;(optional)";
                        } else {
                            Caption = "Password";
                        }
                        TagID = "Password";
                        LoginPanel = LoginPanel + ""
                        + "\r" + "<div class=\"ccAdminSmall\">"
                        + cr2 + "<LABEL for=\"" + TagID + "\">" + cpCore.html.html_GetFormInputText2(TagID, "", 1, 30, TagID, true) + "&nbsp;" + Caption + "</LABEL>"
                        + "\r" + "</div>";
                        //
                        // Autologin checkbox
                        //
                        if (cpCore.siteProperties.getBoolean("AllowAutoLogin", false)) {
                            if (cpCore.doc.authContext.visit.CookieSupport) {
                                TagID = "autologin";
                                LoginPanel = LoginPanel + ""
                                + "\r" + "<div class=\"ccAdminSmall\">"
                                + cr2 + "<LABEL for=\"" + TagID + "\">" + cpCore.html.html_GetFormInputCheckBox2(TagID, true, TagID) + "&nbsp;Login automatically from this computer</LABEL>"
                                + "\r" + "</div>";
                            }
                        }
                        //
                        // Buttons
                        //
                        LoginPanel = LoginPanel + Adminui.GetButtonBar(Adminui.GetButtonsFromList(ButtonLogin + "," + ButtonLogout, true, true, "mb"), "");
                        //
                        // ----- assemble tools panel
                        //
                        Copy = ""
                        + "\r" + "<td width=\"50%\" class=\"ccPanelInput\" style=\"vertical-align:bottom;\">"
                        + genericController.htmlIndent(LoginPanel) + "\r" + "</td>"
                        + "\r" + "<td width=\"50%\" class=\"ccPanelInput\" style=\"vertical-align:bottom;\">"
                        + genericController.htmlIndent(OptionsPanel) + "\r" + "</td>";
                        Copy = ""
                        + "\r" + "<tr>"
                        + genericController.htmlIndent(Copy) + "\r" + "</tr>"
                        + "";
                        Copy = ""
                        + "\r" + "<table border=\"0\" cellpadding=\"3\" cellspacing=\"0\" width=\"100%\">"
                        + genericController.htmlIndent(Copy) + "\r" + "</table>";
                        ToolsPanel.Add(main_GetPanelInput(Copy));
                        ToolsPanel.Add(cpCore.html.html_GetFormEnd);
                        result = result + main_GetPanel(ToolsPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5);
                        //
                        result = result + main_GetPanel(LinkPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5);
                        //
                        LinkPanel = null;
                        ToolsPanel = null;
                        AnotherPanel = null;
                    }
                    //
                    // --- Developer Debug Panel
                    //
                    if (cpCore.visitProperty.getBoolean("AllowDebugging")) {
                        //
                        // --- Debug Panel Header
                        //
                        LinkPanel = new stringBuilderLegacyController();
                        LinkPanel.Add(SpanClassAdminSmall);
                        //LinkPanel.Add( "WebClient " & main_WebClientVersion & " | "
                        LinkPanel.Add("Contensive " + cpCore.codeVersion() + " | ");
                        LinkPanel.Add(cpCore.doc.profileStartTime.ToString("G") + " | ");
                        LinkPanel.Add("<a class=\"ccAdminLink\" target=\"_blank\" href=\"http: //support.Contensive.com/\">Support</A> | ");
                        LinkPanel.Add("<a class=\"ccAdminLink\" href=\"" + genericController.encodeHTML("/" + cpCore.serverConfig.appConfig.adminRoute) + "\">Admin Home</A> | ");
                        LinkPanel.Add("<a class=\"ccAdminLink\" href=\"" + genericController.encodeHTML("http://" + cpCore.webServer.requestDomain) + "\">Public Home</A> | ");
                        LinkPanel.Add("<a class=\"ccAdminLink\" target=\"_blank\" href=\"" + genericController.encodeHTML("/" + cpCore.serverConfig.appConfig.adminRoute + "?" + RequestNameHardCodedPage + "=" + HardCodedPageMyProfile) + "\">My Profile</A> | ");
                        LinkPanel.Add("</span>");
                        //
                        //
                        //
                        //DebugPanel = DebugPanel & main_GetPanel(LinkPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", "5")
                        //
                        DebugPanel = DebugPanel + "\r" + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">"
                        + cr2 + "<tr>"
                        + cr3 + "<td width=\"100\" class=\"ccPanel\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100\" height=\"1\" ></td>"
                        + cr3 + "<td width=\"100%\" class=\"ccPanel\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"1\" height=\"1\" ></td>"
                        + cr2 + "</tr>";
                        //
                        DebugPanel = DebugPanel + getDebugPanelRow("DOM", "<a class=\"ccAdminLink\" href=\"/ccLib/clientside/DOMViewer.htm\" target=\"_blank\">Click</A>");
                        DebugPanel = DebugPanel + getDebugPanelRow("Trap Errors", genericController.encodeHTML(cpCore.siteProperties.trapErrors.ToString()));
                        DebugPanel = DebugPanel + getDebugPanelRow("Trap Email", genericController.encodeHTML(cpCore.siteProperties.getText("TrapEmail")));
                        DebugPanel = DebugPanel + getDebugPanelRow("main_ServerLink", genericController.encodeHTML(cpCore.webServer.requestUrl));
                        DebugPanel = DebugPanel + getDebugPanelRow("main_ServerDomain", genericController.encodeHTML(cpCore.webServer.requestDomain));
                        DebugPanel = DebugPanel + getDebugPanelRow("main_ServerProtocol", genericController.encodeHTML(cpCore.webServer.requestProtocol));
                        DebugPanel = DebugPanel + getDebugPanelRow("main_ServerHost", genericController.encodeHTML(cpCore.webServer.requestDomain));
                        DebugPanel = DebugPanel + getDebugPanelRow("main_ServerPath", genericController.encodeHTML(cpCore.webServer.requestPath));
                        DebugPanel = DebugPanel + getDebugPanelRow("main_ServerPage", genericController.encodeHTML(cpCore.webServer.requestPage));
                        Copy = "";
                        if (cpCore.webServer.requestQueryString != "") {
                            CopySplit = cpCore.webServer.requestQueryString.Split('&');
                            for (Ptr = 0; Ptr <= CopySplit.GetUpperBound(0); Ptr++) {
                                copyNameValue = CopySplit[Ptr];
                                if (!string.IsNullOrEmpty(copyNameValue)) {
                                    copyNameValueSplit = copyNameValue.Split('=');
                                    CopyName = genericController.DecodeResponseVariable(copyNameValueSplit[0]);
                                    copyValue = "";
                                    if (copyNameValueSplit.GetUpperBound(0) > 0) {
                                        copyValue = genericController.DecodeResponseVariable(copyNameValueSplit[1]);
                                    }
                                    Copy = Copy + "\r" + "<br>" + genericController.encodeHTML(CopyName + "=" + copyValue);
                                }
                            }
                            Copy = Copy.Substring(7);
                        }
                        DebugPanel = DebugPanel + getDebugPanelRow("main_ServerQueryString", Copy);
                        Copy = "";
                        foreach (string key in cpCore.docProperties.getKeyList()) {
                            docPropertiesClass docProperty = cpCore.docProperties.getProperty(key);
                            if (docProperty.IsForm) {
                                Copy = Copy + "\r" + "<br>" + genericController.encodeHTML(docProperty.NameValue);
                            }
                        }
                        DebugPanel = DebugPanel + getDebugPanelRow("Render Time &gt;= ", ((cpCore.doc.appStopWatch.ElapsedMilliseconds) / 1000).ToString("0.000") + " sec");
                        if (true) {
                            VisitHrs = Convert.ToInt32(cpCore.doc.authContext.visit.TimeToLastHit / 3600);
                            VisitMin = Convert.ToInt32(cpCore.doc.authContext.visit.TimeToLastHit / 60) - (60 * VisitHrs);
                            VisitSec = cpCore.doc.authContext.visit.TimeToLastHit % 60;
                            DebugPanel = DebugPanel + getDebugPanelRow("Visit Length", Convert.ToString(cpCore.doc.authContext.visit.TimeToLastHit) + " sec, (" + VisitHrs + " hrs " + VisitMin + " mins " + VisitSec + " secs)");
                            //DebugPanel = DebugPanel & main_DebugPanelRow("Visit Length", CStr(main_VisitTimeToLastHit) & " sec, (" & Int(main_VisitTimeToLastHit / 60) & " min " & (main_VisitTimeToLastHit Mod 60) & " sec)")
                        }
                        DebugPanel = DebugPanel + getDebugPanelRow("Addon Profile", "<hr><ul class=\"ccPanel\">" + "<li>tbd</li>" + "\r" + "</ul>");
                        //
                        DebugPanel = DebugPanel + "</table>";
                        //
                        if (ShowLegacyToolsPanel) {
                            //
                            // Debug Panel as part of legacy tools panel
                            //
                            result = result + main_GetPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5);
                        } else {
                            //
                            // Debug Panel without Legacy Tools panel
                            //
                            result = result + main_GetPanelHeader("Debug Panel") + main_GetPanel(LinkPanel.Text) + main_GetPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5);
                        }
                    }
                    result = "\r" + "<div class=\"ccCon\">" + genericController.htmlIndent(result) + "\r" + "</div>";
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //
        //
        private string getDebugPanelRow(string Label, string Value) {
            return cr2 + "<tr><td valign=\"top\" class=\"ccPanel ccAdminSmall\">" + Label + "</td><td valign=\"top\" class=\"ccPanel ccAdminSmall\">" + Value + "</td></tr>";
        }

        //
        //=================================================================================================================
        //   csv_GetAddonOptionStringValue
        //
        //   gets the value from a list matching the name
        //
        //   InstanceOptionstring is an "AddonEncoded" name=AddonEncodedValue[selector]descriptor&name=value string
        //=================================================================================================================
        //
        public static string getAddonOptionStringValue(string OptionName, string addonOptionString) {
            string result = genericController.getSimpleNameValue(OptionName, addonOptionString, "", "&");
            int Pos = genericController.vbInstr(1, result, "[");
            if (Pos > 0) {
                result = result.Substring(0, Pos - 1);
            }
            return Convert.ToString(genericController.decodeNvaArgument(result)).Trim(' ');
        }
        //
        //====================================================================================================
        /// <summary>
        /// Create the full html doc from the accumulated elements
        /// </summary>
        /// <param name="htmlBody"></param>
        /// <param name="htmlBodyTag"></param>
        /// <param name="allowLogin"></param>
        /// <param name="allowTools"></param>
        /// <param name="blockNonContentExtras"></param>
        /// <param name="isAdminSite"></param>
        /// <returns></returns>
        public string getHtmlDoc(string htmlBody, string htmlBodyTag, bool allowLogin = true, bool allowTools = true) {
            string result = "";
            try {
                string htmlHead = getHtmlHead();
                string htmlBeforeEndOfBody = getHtmlDoc_beforeEndOfBodyHtml(allowLogin, allowTools);

                result = ""
                    + cpCore.siteProperties.docTypeDeclaration + Environment.NewLine + "<html>"
                    + Environment.NewLine + "<head>"
                    + htmlHead + Environment.NewLine + "</head>"
                    + Environment.NewLine + htmlBodyTag + htmlBody + htmlBeforeEndOfBody + Environment.NewLine + "</body>"
                    + Environment.NewLine + "</html>"
                    + "";
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //'
        //' assemble all the html parts
        //'
        //Public Function assembleHtmlDoc(ByVal head As String, ByVal bodyTag As String, ByVal Body As String) As String
        //    Return "" _
        //        & cpCore.siteProperties.docTypeDeclarationAdmin _
        //        & cr & "<html>" _
        //        & cr2 & "<head>" _
        //        & genericController.htmlIndent(head) _
        //        & cr2 & "</head>" _
        //        & cr2 & bodyTag _
        //        & genericController.htmlIndent(Body) _
        //        & cr2 & "</body>" _
        //        & cr & "</html>"
        //End Function
        //'
        //'========================================================================
        //' ----- Starts an HTML page (for an admin page -- not a public page)
        //'========================================================================
        //'
        //Public Function getHtmlDoc_beforeBodyHtml(Optional ByVal Title As String = "", Optional ByVal PageMargin As Integer = 0) As String
        //    If Title <> "" Then
        //        Call main_AddPagetitle(Title)
        //    End If
        //    If main_MetaContent_Title = "" Then
        //        Call main_AddPagetitle("Admin-" & cpCore.webServer.webServerIO_requestDomain)
        //    End If
        //    cpCore.webServer.webServerIO_response_NoFollow = True
        //    Call main_SetMetaContent(0, 0)
        //    '
        //    Return "" _
        //        & cpCore.siteProperties.docTypeDeclarationAdmin _
        //        & vbCrLf & "<html>" _
        //        & vbCrLf & "<head>" _
        //        & getHTMLInternalHead(True) _
        //        & vbCrLf & "</head>" _
        //        & vbCrLf & "<body class=""ccBodyAdmin ccCon"">"
        //End Function

        //
        //====================================================================================================
        /// <summary>
        /// legacy compatibility
        /// </summary>
        /// <param name="cpCore"></param>
        /// <param name="ButtonList"></param>
        /// <returns></returns>
        public static string legacy_closeFormTable(coreClass cpCore, string ButtonList) {
            string templegacy_closeFormTable = null;
            if (!string.IsNullOrEmpty(ButtonList)) {
                templegacy_closeFormTable = "</td></tr></TABLE>" + cpCore.html.main_GetPanelButtons(ButtonList, "Button") + "</form>";
            } else {
                templegacy_closeFormTable = "</td></tr></TABLE></form>";
            }
            return templegacy_closeFormTable;
        }
        //
        //====================================================================================================
        /// <summary>
        /// legacy compatibility
        /// </summary>
        /// <param name="cpCore"></param>
        /// <param name="ButtonList"></param>
        /// <returns></returns>
        public static string legacy_openFormTable(coreClass cpCore, string ButtonList) {
            string result = "";
            try {
                result = cpCore.html.html_GetFormStart();
                if (!string.IsNullOrEmpty(ButtonList)) {
                    result = result + cpCore.html.main_GetPanelButtons(ButtonList, "Button");
                }
                result = result + "<table border=\"0\" cellpadding=\"10\" cellspacing=\"0\" width=\"100%\"><tr><TD>";
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public string getHtmlHead() {
            List<string> headList = new List<string>();
            try {
                //
                // -- meta content
                if (cpCore.doc.htmlMetaContent_TitleList.Count > 0) {
                    string content = "";
                    foreach (var asset in cpCore.doc.htmlMetaContent_TitleList) {
                        if ((cpCore.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage))) {
                            headList.Add("<!-- added by " + asset.addedByMessage + " -->");
                        }
                        content += " | " + asset.content;
                    }
                    headList.Add("<title>" + encodeHTML(content.Substring(3)) + "</title>");
                }
                if (cpCore.doc.htmlMetaContent_KeyWordList.Count > 0) {
                    string content = "";
                    foreach (var asset in cpCore.doc.htmlMetaContent_KeyWordList.FindAll((a) => (!string.IsNullOrEmpty(a.content)))) {
                        if ((cpCore.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage))) {
                            headList.Add("<!-- '" + encodeHTML(asset.content + "' added by " + asset.addedByMessage) + " -->");
                        }
                        content += "," + asset.content;
                    }
                    if (!string.IsNullOrEmpty(content)) {
                        headList.Add("<meta name=\"keywords\" content=\"" + encodeHTML(content.Substring(1)) + "\" >");
                    }
                }
                if (cpCore.doc.htmlMetaContent_Description.Count > 0) {
                    string content = "";
                    foreach (var asset in cpCore.doc.htmlMetaContent_Description) {
                        if ((cpCore.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage))) {
                            headList.Add("<!-- '" + encodeHTML(asset.content + "' added by " + asset.addedByMessage) + " -->");
                        }
                        content += "," + asset.content;
                    }
                    headList.Add("<meta name=\"description\" content=\"" + encodeHTML(content.Substring(1)) + "\" >");
                }
                //
                // -- favicon
                string VirtualFilename = cpCore.siteProperties.getText("faviconfilename");
                switch (IO.Path.GetExtension(VirtualFilename).ToLower()) {
                    case ".ico":
                        headList.Add("<link rel=\"icon\" type=\"image/vnd.microsoft.icon\" href=\"" + genericController.getCdnFileLink(cpCore, VirtualFilename) + "\" >");
                        break;
                    case ".png":
                        headList.Add("<link rel=\"icon\" type=\"image/png\" href=\"" + genericController.getCdnFileLink(cpCore, VirtualFilename) + "\" >");
                        break;
                    case ".gif":
                        headList.Add("<link rel=\"icon\" type=\"image/gif\" href=\"" + genericController.getCdnFileLink(cpCore, VirtualFilename) + "\" >");
                        break;
                    case ".jpg":
                        headList.Add("<link rel=\"icon\" type=\"image/jpg\" href=\"" + genericController.getCdnFileLink(cpCore, VirtualFilename) + "\" >");
                        break;
                }
                //
                // -- misc caching, etc
                string encoding = genericController.encodeHTML(cpCore.siteProperties.getText("Site Character Encoding", "utf-8"));
                headList.Add("<meta http-equiv=\"content-type\" content=\"text/html; charset=" + encoding + "\">");
                headList.Add("<meta http-equiv=\"content-language\" content=\"en-us\">");
                headList.Add("<meta http-equiv=\"cache-control\" content=\"no-cache\">");
                headList.Add("<meta http-equiv=\"expires\" content=\"-1\">");
                headList.Add("<meta http-equiv=\"pragma\" content=\"no-cache\">");
                headList.Add("<meta name=\"generator\" content=\"Contensive\">");
                //
                // -- no-follow
                if (cpCore.webServer.response_NoFollow) {
                    headList.Add("<meta name=\"robots\" content=\"nofollow\" >");
                    headList.Add("<meta name=\"mssmarttagspreventparsing\" content=\"true\" >");
                }
                //
                // -- base is needed for Link Alias case where a slash is in the URL (page named 1/2/3/4/5)
                if (!string.IsNullOrEmpty(cpCore.webServer.serverFormActionURL)) {
                    string BaseHref = cpCore.webServer.serverFormActionURL;
                    if (!string.IsNullOrEmpty(cpCore.doc.refreshQueryString)) {
                        BaseHref += "?" + cpCore.doc.refreshQueryString;
                    }
                    headList.Add("<base href=\"" + BaseHref + "\" >");
                }
                //
                if (cpCore.doc.htmlAssetList.Count > 0) {
                    List<string> scriptList = new List<string>();
                    List<string> styleList = new List<string>();
                    foreach (var asset in cpCore.doc.htmlAssetList.FindAll((htmlAssetClass item) => (item.inHead))) {
                        if (cpCore.doc.allowDebugLog) {
                            if ((cpCore.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage))) {
                                headList.Add("<!-- '" + encodeHTML(asset.content + "' added by " + asset.addedByMessage) + " -->");
                            }
                        }
                        if (asset.assetType.Equals(htmlAssetTypeEnum.style)) {
                            if (asset.isLink) {
                                styleList.Add("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + asset.content + "\" >");
                            } else {
                                styleList.Add("<style>" + asset.content + "</style>");
                            }
                        } else if (asset.assetType.Equals(htmlAssetTypeEnum.script)) {

                            if (asset.isLink) {
                                scriptList.Add("<script type=\"text/javascript\" src=\"" + asset.content + "\"></script>");
                            } else {
                                scriptList.Add("<script type=\"text/javascript\">" + asset.content + "</script>");
                            }
                        }
                    }
                    headList.AddRange(styleList);
                    headList.AddRange(scriptList);
                }
                //
                // -- other head tags - always last
                foreach (var asset in cpCore.doc.htmlMetaContent_OtherTags.FindAll((a) => (!string.IsNullOrEmpty(a.content)))) {
                    if (cpCore.doc.allowDebugLog) {
                        if ((cpCore.doc.visitPropertyAllowDebugging) && (!string.IsNullOrEmpty(asset.addedByMessage))) {
                            headList.Add("<!-- '" + encodeHTML(asset.content + "' added by " + asset.addedByMessage) + " -->");
                        }
                    }
                    headList.Add(asset.content);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return string.Join("\r", headList);
        }

        //
        //====================================================================================================
        //
        public void addScriptCode_onLoad(string code, string addedByMessage) {
            try {
                if (!string.IsNullOrEmpty(code)) {
                    cpCore.doc.htmlAssetList.Add(new htmlAssetClass() {
                        assetType = htmlAssetTypeEnum.OnLoadScript,
                        addedByMessage = addedByMessage,
                        isLink = false,
                        content = code
                    });
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
        }
        //
        //====================================================================================================
        //
        public void addScriptCode_body(string code, string addedByMessage) {
            try {
                if (!string.IsNullOrEmpty(code)) {
                    cpCore.doc.htmlAssetList.Add(new htmlAssetClass() {
                        assetType = htmlAssetTypeEnum.script,
                        addedByMessage = addedByMessage,
                        isLink = false,
                        content = genericController.removeScriptTag(code)
                    });
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
        }
        //
        //====================================================================================================
        //
        public void addScriptCode_head(string code, string addedByMessage) {
            try {
                if (!string.IsNullOrEmpty(code)) {
                    cpCore.doc.htmlAssetList.Add(new htmlAssetClass() {
                        assetType = htmlAssetTypeEnum.script,
                        inHead = true,
                        addedByMessage = addedByMessage,
                        isLink = false,
                        content = genericController.removeScriptTag(code)
                    });
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
        }
        //
        //=========================================================================================================
        //
        public void addScriptLink_Head(string Filename, string addedByMessage) {
            try {
                if (!string.IsNullOrEmpty(Filename)) {
                    cpCore.doc.htmlAssetList.Add(new htmlAssetClass {
                        assetType = htmlAssetTypeEnum.script,
                        addedByMessage = addedByMessage,
                        isLink = true,
                        inHead = true,
                        content = Filename
                    });
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
        }
        //
        //=========================================================================================================
        //
        public void addScriptLink_Body(string Filename, string addedByMessage) {
            try {
                if (!string.IsNullOrEmpty(Filename)) {
                    cpCore.doc.htmlAssetList.Add(new htmlAssetClass {
                        assetType = htmlAssetTypeEnum.script,
                        addedByMessage = addedByMessage,
                        isLink = true,
                        content = Filename
                    });
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
        }
        //
        //=========================================================================================================
        //
        public void addTitle(string pageTitle, string addedByMessage = "") {
            try {
                if (!string.IsNullOrEmpty(pageTitle.Trim())) {
                    cpCore.doc.htmlMetaContent_TitleList.Add(new htmlMetaClass() {
                        addedByMessage = addedByMessage,
                        content = pageTitle
                    });
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
        }
        //
        //=========================================================================================================
        //
        public void addMetaDescription(string MetaDescription, string addedByMessage = "") {
            try {
                if (!string.IsNullOrEmpty(MetaDescription.Trim())) {
                    cpCore.doc.htmlMetaContent_Description.Add(new htmlMetaClass() {
                        addedByMessage = addedByMessage,
                        content = MetaDescription
                    });
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
        }
        //
        //=========================================================================================================
        //
        public void addStyleLink(string StyleSheetLink, string addedByMessage) {
            try {
                if (!string.IsNullOrEmpty(StyleSheetLink.Trim())) {
                    cpCore.doc.htmlAssetList.Add(new htmlAssetClass() {
                        addedByMessage = addedByMessage,
                        assetType = htmlAssetTypeEnum.style,
                        inHead = true,
                        isLink = true,
                        content = StyleSheetLink
                    });
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
        }
        //
        //=========================================================================================================
        //
        public void addStyleCode(string code, string addedByMessage = "") {
            try {
                if (!string.IsNullOrEmpty(code.Trim())) {
                    cpCore.doc.htmlAssetList.Add(new htmlAssetClass() {
                        addedByMessage = addedByMessage,
                        assetType = htmlAssetTypeEnum.style,
                        inHead = true,
                        isLink = false,
                        content = code
                    });
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
        }
        //
        //=========================================================================================================
        //
        public void addMetaKeywordList(string MetaKeywordList, string addedByMessage = "") {
            try {
                foreach (string keyword in MetaKeywordList.Split(',')) {
                    if (!string.IsNullOrEmpty(keyword)) {
                        cpCore.doc.htmlMetaContent_KeyWordList.Add(new htmlMetaClass() {
                            addedByMessage = addedByMessage,
                            content = keyword
                        });
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
        }
        //
        //=========================================================================================================
        //
        public void addHeadTag(string HeadTag, string addedByMessage = "") {
            try {
                cpCore.doc.htmlMetaContent_OtherTags.Add(new htmlMetaClass() {
                    addedByMessage = addedByMessage,
                    content = HeadTag
                });
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
        }
        //
        //===================================================================================================
        //
        public string getEditWrapper(string Caption, string Content) {
            string result = Content;
            try {
                if (cpCore.doc.authContext.isEditingAnything()) {
                    result = html_GetLegacySiteStyles() + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccEditWrapper\">";
                    if (!string.IsNullOrEmpty(Caption)) {
                        result += ""
                                + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccEditWrapperCaption\">"
                                + genericController.encodeText(Caption) + "<!-- <img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=1 height=22 align=absmiddle> -->"
                                + "</td></tr></table>";
                    }
                    result += ""
                            + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0><tr><td class=\"ccEditWrapperContent\" id=\"editWrapper" + cpCore.doc.editWrapperCnt + "\">"
                            + genericController.encodeText(Content) + "</td></tr></table>"
                            + "</td></tr></table>";
                    cpCore.doc.editWrapperCnt = cpCore.doc.editWrapperCnt + 1;
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //===================================================================================================
        // To support the special case when the template calls this to encode itself, and the page content has already been rendered.
        //
        private string convertActiveContent_internal(string Source, int personalizationPeopleId, string ContextContentName, int ContextRecordID, int ContextContactPeopleID, bool PlainText, bool AddLinkEID, bool EncodeActiveFormatting, bool EncodeActiveImages, bool EncodeActiveEditIcons, bool EncodeActivePersonalization, string queryStringForLinkAppend, string ProtocolHostLink, bool IsEmailContent, int ignore_DefaultWrapperID, string ignore_TemplateCaseOnly_Content, CPUtilsBaseClass.addonContext Context, bool personalizationIsAuthenticated, object nothingObject, bool isEditingAnything) {
            string result = Source;
            try {
                //
                const string StartFlag = "<!-- ADDON";
                const string EndFlag = " -->";
                //
                bool DoAnotherPass = false;
                int ArgCnt = 0;
                string AddonGuid = null;
                string ACInstanceID = null;
                string[] ArgSplit = null;
                string AddonName = null;
                string addonOptionString = null;
                int LineStart = 0;
                int LineEnd = 0;
                string Copy = null;
                string[] Wrapper = null;
                string[] SegmentSplit = null;
                string AcCmd = null;
                string SegmentSuffix = null;
                string[] AcCmdSplit = null;
                string ACType = null;
                string[] ContentSplit = null;
                int ContentSplitCnt = 0;
                string Segment = null;
                int Ptr = 0;
                string CopyName = null;
                string ListName = null;
                string SortField = null;
                bool SortReverse = false;
                string AdminURL = null;
                //
                htmlToTextControllers converthtmlToText = null;
                //
                int iPersonalizationPeopleId = personalizationPeopleId;
                if (iPersonalizationPeopleId == 0) {
                    iPersonalizationPeopleId = cpCore.doc.authContext.user.id;
                }
                //

                //hint = "csv_EncodeContent9 enter"
                if (!string.IsNullOrEmpty(result)) {
                    AdminURL = "/" + cpCore.serverConfig.appConfig.adminRoute;
                    //
                    //--------
                    // cut-paste from csv_EncodeContent8
                    //--------
                    //
                    // ----- Do EncodeCRLF Conversion
                    //
                    //hint = hint & ",010"
                    if (cpCore.siteProperties.getBoolean("ConvertContentCRLF2BR", false) && (!PlainText)) {
                        result = genericController.vbReplace(result, "\r", "");
                        result = genericController.vbReplace(result, "\n", "<br>");
                    }
                    //
                    // ----- Do upgrade conversions (upgrade legacy objects and upgrade old images)
                    //
                    //hint = hint & ",020"
                    result = upgradeActiveContent(result);
                    //
                    // ----- Do Active Content Conversion
                    //
                    //hint = hint & ",030"
                    if (AddLinkEID || EncodeActiveFormatting || EncodeActiveImages || EncodeActiveEditIcons) {
                        result = convertActiveContent_Internal_activeParts(result, iPersonalizationPeopleId, ContextContentName, ContextRecordID, ContextContactPeopleID, AddLinkEID, EncodeActiveFormatting, EncodeActiveImages, EncodeActiveEditIcons, EncodeActivePersonalization, queryStringForLinkAppend, ProtocolHostLink, IsEmailContent, AdminURL, personalizationIsAuthenticated, Context);
                    }
                    //
                    // ----- Do Plain Text Conversion
                    //
                    //hint = hint & ",040"
                    if (PlainText) {
                        converthtmlToText = new htmlToTextControllers(cpCore);
                        result = converthtmlToText.convert(result);
                        converthtmlToText = null;
                    }
                    //
                    // Process Active Content that must be run here to access webclass objects
                    //     parse as {{functionname?querystring}}
                    //
                    //hint = hint & ",110"
                    if ((!EncodeActiveEditIcons) && (result.IndexOf("{{") + 1 != 0)) {
                        ContentSplit = Microsoft.VisualBasic.Strings.Split(result, "{{", -1, Microsoft.VisualBasic.CompareMethod.Binary);
                        result = "";
                        ContentSplitCnt = ContentSplit.GetUpperBound(0) + 1;
                        Ptr = 0;
                        while (Ptr < ContentSplitCnt) {
                            //hint = hint & ",200"
                            Segment = ContentSplit[Ptr];
                            if (Ptr == 0) {
                                //
                                // Add in the non-command text that is before the first command
                                //
                                result = result + Segment;
                            } else if (!string.IsNullOrEmpty(Segment)) {
                                if (genericController.vbInstr(1, Segment, "}}") == 0) {
                                    //
                                    // No command found, return the marker and deliver the Segment
                                    //
                                    //hint = hint & ",210"
                                    result = result + "{{" + Segment;
                                } else {
                                    //
                                    // isolate the command
                                    //
                                    //hint = hint & ",220"
                                    SegmentSplit = Microsoft.VisualBasic.Strings.Split(Segment, "}}", -1, Microsoft.VisualBasic.CompareMethod.Binary);
                                    AcCmd = SegmentSplit[0];
                                    SegmentSplit[0] = "";
                                    SegmentSuffix = string.Join("}}", SegmentSplit).Substring(2);
                                    if (!string.IsNullOrEmpty(AcCmd.Trim(' '))) {
                                        //
                                        // isolate the arguments
                                        //
                                        //hint = hint & ",230"
                                        AcCmdSplit = AcCmd.Split('?');
                                        ACType = AcCmdSplit[0].Trim(' ');
                                        if (AcCmdSplit.GetUpperBound(0) == 0) {
                                            addonOptionString = "";
                                        } else {
                                            addonOptionString = AcCmdSplit[1];
                                            addonOptionString = genericController.decodeHtml(addonOptionString);
                                        }
                                        //
                                        // execute the command
                                        //
                                        switch (genericController.vbUCase(ACType)) {
                                            case ACTypeDynamicForm:
                                                //
                                                // Dynamic Form - run the core addon replacement instead
                                                CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                                                    addonType = CPUtilsBaseClass.addonContext.ContextPage,
                                                    cssContainerClass = "",
                                                    cssContainerId = "",
                                                    hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                                                        contentName = ContextContentName,
                                                        fieldName = "",
                                                        recordId = ContextRecordID
                                                    },
                                                    personalizationAuthenticated = personalizationIsAuthenticated,
                                                    personalizationPeopleId = iPersonalizationPeopleId,
                                                    instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, addonOptionString)
                                                };
                                                Models.Entity.addonModel addon = Models.Entity.addonModel.create(cpCore, addonGuidDynamicForm);
                                                result += cpCore.addon.execute(addon, executeContext);
                                                break;
                                            case ACTypeChildList:
                                                //
                                                // Child Page List
                                                //
                                                //hint = hint & ",320"
                                                ListName = addonController.getAddonOption("name", addonOptionString);
                                                result = result + cpCore.doc.getChildPageList(ListName, ContextContentName, ContextRecordID, true);
                                                break;
                                            case ACTypeTemplateText:
                                                //
                                                // Text Box = copied here from gethtmlbody
                                                //
                                                CopyName = addonController.getAddonOption("new", addonOptionString);
                                                if (string.IsNullOrEmpty(CopyName)) {
                                                    CopyName = addonController.getAddonOption("name", addonOptionString);
                                                    if (string.IsNullOrEmpty(CopyName)) {
                                                        CopyName = "Default";
                                                    }
                                                }
                                                result = result + html_GetContentCopy(CopyName, "", iPersonalizationPeopleId, false, personalizationIsAuthenticated);
                                                break;
                                            case ACTypeWatchList:
                                                //
                                                // Watch List
                                                //
                                                //hint = hint & ",330"
                                                ListName = addonController.getAddonOption("LISTNAME", addonOptionString);
                                                SortField = addonController.getAddonOption("SORTFIELD", addonOptionString);
                                                SortReverse = genericController.EncodeBoolean(addonController.getAddonOption("SORTDIRECTION", addonOptionString));
                                                result = result + cpCore.doc.main_GetWatchList(cpCore, ListName, SortField, SortReverse);
                                                break;
                                            default:
                                                //
                                                // Unrecognized command - put all the syntax back in
                                                //
                                                //hint = hint & ",340"
                                                result = result + "{{" + AcCmd + "}}";
                                                break;
                                        }
                                    }
                                    //
                                    // add the SegmentSuffix back on
                                    //
                                    result = result + SegmentSuffix;
                                }
                            }
                            //
                            // Encode into Javascript if required
                            //
                            Ptr = Ptr + 1;
                        }
                    }
                    //
                    // Process Addons
                    //   parse as <!-- Addon "Addon Name","OptionString" -->
                    //   They are handled here because Addons are written against cpCoreClass, not the Content Server class
                    //   ...so Group Email can not process addons 8(
                    //   Later, remove the csv routine that translates <ac to this, and process it directly right here
                    //   Later, rewrite so addons call csv, not cpCoreClass, so email processing can include addons
                    // (2/16/2010) - move csv_EncodeContent to csv, or wait and move it all to CP
                    //    eventually, everything should migrate to csv and/or cp to eliminate the cpCoreClass dependancy
                    //    and all add-ons run as processes the same as they run on pages, or as remote methods
                    // (2/16/2010) - if <!-- AC --> has four arguments, the fourth is the addon guid
                    //
                    if (result.IndexOf(StartFlag) + 1 != 0) {
                        while (result.IndexOf(StartFlag) + 1 != 0) {
                            LineStart = genericController.vbInstr(1, result, StartFlag);
                            LineEnd = genericController.vbInstr(LineStart, result, EndFlag);
                            if (LineEnd == 0) {
                                logController.appendLog(cpCore, "csv_EncodeContent9, Addon could not be inserted into content because the HTML comment holding the position is not formated correctly");
                                break;
                            } else {
                                AddonName = "";
                                addonOptionString = "";
                                ACInstanceID = "";
                                AddonGuid = "";
                                Copy = result.Substring(LineStart + 10, LineEnd - LineStart - 11);
                                ArgSplit = genericController.SplitDelimited(Copy, ",");
                                ArgCnt = ArgSplit.GetUpperBound(0) + 1;
                                if (!string.IsNullOrEmpty(ArgSplit[0])) {
                                    AddonName = ArgSplit[0].Substring(1, ArgSplit[0].Length - 2);
                                    if (ArgCnt > 1) {
                                        if (!string.IsNullOrEmpty(ArgSplit[1])) {
                                            addonOptionString = ArgSplit[1].Substring(1, ArgSplit[1].Length - 2);
                                            addonOptionString = genericController.decodeHtml(addonOptionString.Trim(' '));
                                        }
                                        if (ArgCnt > 2) {
                                            if (!string.IsNullOrEmpty(ArgSplit[2])) {
                                                ACInstanceID = ArgSplit[2].Substring(1, ArgSplit[2].Length - 2);
                                            }
                                            if (ArgCnt > 3) {
                                                if (!string.IsNullOrEmpty(ArgSplit[3])) {
                                                    AddonGuid = ArgSplit[3].Substring(1, ArgSplit[3].Length - 2);
                                                }
                                            }
                                        }
                                    }
                                    // dont have any way of getting fieldname yet

                                    CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext() {
                                        addonType = CPUtilsBaseClass.addonContext.ContextPage,
                                        cssContainerClass = "",
                                        cssContainerId = "",
                                        hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                                            contentName = ContextContentName,
                                            fieldName = "",
                                            recordId = ContextRecordID
                                        },
                                        personalizationAuthenticated = personalizationIsAuthenticated,
                                        personalizationPeopleId = iPersonalizationPeopleId,
                                        instanceGuid = ACInstanceID,
                                        instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, addonOptionString)
                                    };
                                    if (!string.IsNullOrEmpty(AddonGuid)) {
                                        Copy = cpCore.addon.execute(Models.Entity.addonModel.create(cpCore, AddonGuid), executeContext);
                                        //Copy = cpCore.addon.execute_legacy6(0, AddonGuid, addonOptionString, CPUtilsBaseClass.addonContext.ContextPage, ContextContentName, ContextRecordID, "", ACInstanceID, False, ignore_DefaultWrapperID, ignore_TemplateCaseOnly_Content, False, Nothing, "", Nothing, "", iPersonalizationPeopleId, personalizationIsAuthenticated)
                                    } else {
                                        Copy = cpCore.addon.execute(Models.Entity.addonModel.createByName(cpCore, AddonName), executeContext);
                                        //Copy = cpCore.addon.execute_legacy6(0, AddonName, addonOptionString, CPUtilsBaseClass.addonContext.ContextPage, ContextContentName, ContextRecordID, "", ACInstanceID, False, ignore_DefaultWrapperID, ignore_TemplateCaseOnly_Content, False, Nothing, "", Nothing, "", iPersonalizationPeopleId, personalizationIsAuthenticated)
                                    }
                                }
                            }
                            result = result.Substring(0, LineStart - 1) + Copy + result.Substring(LineEnd + 3);
                        }
                    }
                    //
                    // process out text block comments inserted by addons
                    // remove all content between BlockTextStartMarker and the next BlockTextEndMarker, or end of copy
                    // exception made for the content with just the startmarker because when the AC tag is replaced with
                    // with the marker, encode content is called with the result, which is just the marker, and this
                    // section will remove it
                    //
                    if ((!isEditingAnything) && (result != BlockTextStartMarker)) {
                        DoAnotherPass = true;
                        while ((result.IndexOf(BlockTextStartMarker, System.StringComparison.OrdinalIgnoreCase) + 1 != 0) && DoAnotherPass) {
                            LineStart = genericController.vbInstr(1, result, BlockTextStartMarker, Microsoft.VisualBasic.Constants.vbTextCompare);
                            if (LineStart == 0) {
                                DoAnotherPass = false;
                            } else {
                                LineEnd = genericController.vbInstr(LineStart, result, BlockTextEndMarker, Microsoft.VisualBasic.Constants.vbTextCompare);
                                if (LineEnd <= 0) {
                                    DoAnotherPass = false;
                                    result = result.Substring(0, LineStart - 1);
                                } else {
                                    LineEnd = genericController.vbInstr(LineEnd, result, " -->");
                                    if (LineEnd <= 0) {
                                        DoAnotherPass = false;
                                    } else {
                                        result = result.Substring(0, LineStart - 1) + result.Substring(LineEnd + 3);
                                        //returnValue = Mid(returnValue, 1, LineStart - 1) & Copy & Mid(returnValue, LineEnd + 4)
                                    }
                                }
                            }
                        }
                    }
                    //
                    // only valid for a webpage
                    //
                    if (true) {
                        //
                        // Add in EditWrappers for Aggregate scripts and replacements
                        //   This is also old -- used here because csv encode content can create replacements and links, but can not
                        //   insert wrappers. This is all done in GetAddonContents() now. This routine is left only to
                        //   handle old style calls in cache.
                        //
                        //hint = hint & ",500, Adding edit wrappers"
                        if (isEditingAnything) {
                            if (result.IndexOf("<!-- AFScript -->", System.StringComparison.OrdinalIgnoreCase) + 1 != 0) {
                                throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError7("returnValue", "AFScript Style edit wrappers are not supported")
                                Copy = getEditWrapper("Aggregate Script", "##MARKER##");
                                Wrapper = Microsoft.VisualBasic.Strings.Split(Copy, "##MARKER##", -1, Microsoft.VisualBasic.CompareMethod.Binary);
                                result = genericController.vbReplace(result, "<!-- AFScript -->", Wrapper[0], 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
                                result = genericController.vbReplace(result, "<!-- /AFScript -->", Wrapper[1], 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
                            }
                            if (result.IndexOf("<!-- AFReplacement -->", System.StringComparison.OrdinalIgnoreCase) + 1 != 0) {
                                throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError7("returnValue", "AFReplacement Style edit wrappers are not supported")
                                Copy = getEditWrapper("Aggregate Replacement", "##MARKER##");
                                Wrapper = Microsoft.VisualBasic.Strings.Split(Copy, "##MARKER##", -1, Microsoft.VisualBasic.CompareMethod.Binary);
                                result = genericController.vbReplace(result, "<!-- AFReplacement -->", Wrapper[0], 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
                                result = genericController.vbReplace(result, "<!-- /AFReplacement -->", Wrapper[1], 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
                            }
                        }
                        //
                        // Process Feedback form
                        //
                        //hint = hint & ",600, Handle webclient features"
                        if (genericController.vbInstr(1, result, FeedbackFormNotSupportedComment, Microsoft.VisualBasic.Constants.vbTextCompare) != 0) {
                            result = genericController.vbReplace(result, FeedbackFormNotSupportedComment, pageContentController.main_GetFeedbackForm(cpCore, ContextContentName, ContextRecordID, ContextContactPeopleID), 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
                        }
                        //'
                        //' If any javascript or styles were added during encode, pick them up now
                        //'
                        //Copy = cpCore.doc.getNextJavascriptBodyEnd()
                        //Do While Copy <> ""
                        //    Call addScriptCode_body(Copy, "embedded content")
                        //    Copy = cpCore.doc.getNextJavascriptBodyEnd()
                        //Loop
                        //'
                        //' current
                        //'
                        //Copy = cpCore.doc.getNextJSFilename()
                        //Do While Copy <> ""
                        //    If genericController.vbInstr(1, Copy, "://") <> 0 Then
                        //    ElseIf Left(Copy, 1) = "/" Then
                        //    Else
                        //        Copy = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, Copy)
                        //    End If
                        //    Call addScriptLink_Head(Copy, "embedded content")
                        //    Copy = cpCore.doc.getNextJSFilename()
                        //Loop
                        //'
                        //Copy = cpCore.doc.getJavascriptOnLoad()
                        //Do While Copy <> ""
                        //    Call addOnLoadJs(Copy, "")
                        //    Copy = cpCore.doc.getJavascriptOnLoad()
                        //Loop
                        //
                        //Copy = cpCore.doc.getNextStyleFilenames()
                        //Do While Copy <> ""
                        //    If genericController.vbInstr(1, Copy, "://") <> 0 Then
                        //    ElseIf Left(Copy, 1) = "/" Then
                        //    Else
                        //        Copy = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, Copy)
                        //    End If
                        //    Call addStyleLink(Copy, "")
                        //    Copy = cpCore.doc.getNextStyleFilenames()
                        //Loop
                    }
                }
                //
                result = result;
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        // ================================================================================================================
        //   Upgrade old objects in content, and update changed resource library images
        // ================================================================================================================
        //
        public string upgradeActiveContent(string Source) {
            string result = Source;
            try {
                string RecordVirtualPath = string.Empty;
                string RecordVirtualFilename = null;
                string RecordFilename = null;
                string RecordFilenameNoExt = null;
                string RecordFilenameExt = string.Empty;
                string[] SizeTest = null;
                string RecordAltSizeList = null;
                int TagPosEnd = 0;
                int TagPosStart = 0;
                bool InTag = false;
                int Pos = 0;
                string FilenameSegment = null;
                int EndPos1 = 0;
                int EndPos2 = 0;
                string[] LinkSplit = null;
                int LinkCnt = 0;
                int LinkPtr = 0;
                string[] TableSplit = null;
                string TableName = null;
                string FieldName = null;
                int RecordID = 0;
                bool SaveChanges = false;
                int EndPos = 0;
                int Ptr = 0;
                string FilePrefixSegment = null;
                bool ImageAllowUpdate = false;
                string ContentFilesLinkPrefix = null;
                string ResourceLibraryLinkPrefix = null;
                string TestChr = null;
                bool ParseError = false;
                result = Source;
                //
                ContentFilesLinkPrefix = "/" + cpCore.serverConfig.appConfig.name + "/files/";
                ResourceLibraryLinkPrefix = ContentFilesLinkPrefix + "ccLibraryFiles/";
                ImageAllowUpdate = cpCore.siteProperties.getBoolean("ImageAllowUpdate", true);
                ImageAllowUpdate = ImageAllowUpdate && (Source.IndexOf(ResourceLibraryLinkPrefix, System.StringComparison.OrdinalIgnoreCase) + 1 != 0);
                if (ImageAllowUpdate) {
                    //
                    // ----- Process Resource Library Images (swap in most current file)
                    //
                    //   There is a better way:
                    //   problem with replacing the images is the problem with parsing - too much work to find it
                    //   instead, use new replacement tags <ac type=image src="cclibraryfiles/filename/00001" width=0 height=0>
                    //
                    //'hint = hint & ",010"
                    ParseError = false;
                    LinkSplit = Microsoft.VisualBasic.Strings.Split(Source, ContentFilesLinkPrefix, -1, Microsoft.VisualBasic.Constants.vbTextCompare);
                    LinkCnt = LinkSplit.GetUpperBound(0) + 1;
                    for (LinkPtr = 1; LinkPtr < LinkCnt; LinkPtr++) {
                        //
                        // Each LinkSplit(1...) is a segment that would have started with '/appname/files/'
                        // Next job is to determine if this sement is in a tag (<img src="...">) or in content (&quot...&quote)
                        // For now, skip the ones in content
                        //
                        //'hint = hint & ",020"
                        TagPosEnd = genericController.vbInstr(1, LinkSplit[LinkPtr], ">");
                        TagPosStart = genericController.vbInstr(1, LinkSplit[LinkPtr], "<");
                        if (TagPosEnd == 0 && TagPosStart == 0) {
                            //
                            // no tags found, skip it
                            //
                            InTag = false;
                        } else if (TagPosEnd == 0) {
                            //
                            // no end tag, but found a start tag -> in content
                            //
                            InTag = false;
                        } else if (TagPosEnd < TagPosStart) {
                            //
                            // Found end before start - > in tag
                            //
                            InTag = true;
                        } else {
                            //
                            // Found start before end -> in content
                            //
                            InTag = false;
                        }
                        if (InTag) {
                            //'hint = hint & ",030"
                            TableSplit = LinkSplit[LinkPtr].Split('/');
                            if (TableSplit.GetUpperBound(0) > 2) {
                                TableName = TableSplit[0];
                                FieldName = TableSplit[1];
                                RecordID = genericController.EncodeInteger(TableSplit[2]);
                                FilenameSegment = TableSplit[3];
                                if ((TableName.ToLower() == "cclibraryfiles") && (FieldName.ToLower() == "filename") && (RecordID != 0)) {
                                    Models.Entity.libraryFilesModel file = Models.Entity.libraryFilesModel.create(cpCore, RecordID);
                                    if (file != null) {
                                        //'hint = hint & ",060"
                                        FieldName = "filename";
                                        //SQL = "select filename,altsizelist from " & TableName & " where id=" & RecordID
                                        //CS = app.csv_OpenCSSQL("default", SQL)
                                        //If app.csv_IsCSOK(CS) Then
                                        if (true) {
                                            //
                                            // now figure out how the link is delimited by how it starts
                                            //   go to the left and look for:
                                            //   ' ' - ignore spaces, continue forward until we find one of these
                                            //   '=' - means space delimited (src=/image.jpg), ends in ' ' or '>'
                                            //   '"' - means quote delimited (src="/image.jpg"), ends in '"'
                                            //   '>' - means this is not in an HTML tag - skip it (<B>image.jpg</b>)
                                            //   '<' - means god knows what, but its wrong, skip it
                                            //   '(' - means it is a URL(/image.jpg), go to ')'
                                            //
                                            // odd cases:
                                            //   URL( /image.jpg) -
                                            //
                                            RecordVirtualFilename = file.Filename;
                                            RecordAltSizeList = file.AltSizeList;
                                            if (RecordVirtualFilename == genericController.EncodeJavascript(RecordVirtualFilename)) {
                                                //
                                                // The javascript version of the filename must match the filename, since we have no way
                                                // of differentiating a ligitimate file, from a block of javascript. If the file
                                                // contains an apostrophe, the original code could have encoded it, but we can not here
                                                // so the best plan is to skip it
                                                //
                                                // example:
                                                // RecordVirtualFilename = "jay/files/cclibraryfiles/filename/000005/test.png"
                                                //
                                                // RecordFilename = "test.png"
                                                // RecordFilenameAltSize = "" (does not exist - the record has the raw filename in it)
                                                // RecordFilenameExt = "png"
                                                // RecordFilenameNoExt = "test"
                                                //
                                                // RecordVirtualFilename = "jay/files/cclibraryfiles/filename/000005/test-100x200.png"
                                                // this is a specail case - most cases to not have the alt size format saved in the filename
                                                // RecordFilename = "test-100x200.png"
                                                // RecordFilenameAltSize (does not exist - the record has the raw filename in it)
                                                // RecordFilenameExt = "png"
                                                // RecordFilenameNoExt = "test-100x200"
                                                // this is wrong
                                                //   xRecordFilenameAltSize = "100x200"
                                                //   xRecordFilenameExt = "png"
                                                //   xRecordFilenameNoExt = "test"
                                                //
                                                //'hint = hint & ",080"
                                                Pos = RecordVirtualFilename.LastIndexOf("/") + 1;
                                                RecordFilename = "";
                                                if (Pos > 0) {
                                                    RecordVirtualPath = RecordVirtualFilename.Substring(0, Pos);
                                                    RecordFilename = RecordVirtualFilename.Substring(Pos);
                                                }
                                                Pos = RecordFilename.LastIndexOf(".") + 1;
                                                RecordFilenameNoExt = "";
                                                if (Pos > 0) {
                                                    RecordFilenameExt = genericController.vbLCase(RecordFilename.Substring(Pos));
                                                    RecordFilenameNoExt = genericController.vbLCase(RecordFilename.Substring(0, Pos - 1));
                                                }
                                                FilePrefixSegment = LinkSplit[LinkPtr - 1];
                                                if (FilePrefixSegment.Length > 1) {
                                                    //
                                                    // Look into FilePrefixSegment and see if we are in the querystring attribute of an <AC tag
                                                    //   if so, the filename may be AddonEncoded and delimited with & (so skip it)
                                                    Pos = FilePrefixSegment.LastIndexOf("<") + 1;
                                                    if (Pos > 0) {
                                                        if (genericController.vbLCase(FilePrefixSegment.Substring(Pos, 3)) != "ac ") {
                                                            //
                                                            // look back in the FilePrefixSegment to find the character before the link
                                                            //
                                                            EndPos = 0;
                                                            for (Ptr = FilePrefixSegment.Length; Ptr >= 1; Ptr--) {
                                                                TestChr = FilePrefixSegment.Substring(Ptr - 1, 1);
                                                                switch (TestChr) {
                                                                    case "=":
                                                                        //
                                                                        // Ends in ' ' or '>', find the first
                                                                        //
                                                                        EndPos1 = genericController.vbInstr(1, FilenameSegment, " ");
                                                                        EndPos2 = genericController.vbInstr(1, FilenameSegment, ">");
                                                                        if (EndPos1 != 0 & EndPos2 != 0) {
                                                                            if (EndPos1 < EndPos2) {
                                                                                EndPos = EndPos1;
                                                                            } else {
                                                                                EndPos = EndPos2;
                                                                            }
                                                                        } else if (EndPos1 != 0) {
                                                                            EndPos = EndPos1;
                                                                        } else if (EndPos2 != 0) {
                                                                            EndPos = EndPos2;
                                                                        } else {
                                                                            EndPos = 0;
                                                                        }
                                                                        //INSTANT C# WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                                        //ORIGINAL LINE: Exit For
                                                                        goto ExitLabel1;
                                                                    case "\"":
                                                                        //
                                                                        // Quoted, ends is '"'
                                                                        //
                                                                        EndPos = genericController.vbInstr(1, FilenameSegment, "\"");
                                                                        //INSTANT C# WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                                        //ORIGINAL LINE: Exit For
                                                                        goto ExitLabel1;
                                                                    case "(":
                                                                        //
                                                                        // url() style, ends in ')' or a ' '
                                                                        //
                                                                        if (genericController.vbLCase(FilePrefixSegment.Substring(Ptr - 1, 7)) == "(&quot;") {
                                                                            EndPos = genericController.vbInstr(1, FilenameSegment, "&quot;)");
                                                                        } else if (genericController.vbLCase(FilePrefixSegment.Substring(Ptr - 1, 2)) == "('") {
                                                                            EndPos = genericController.vbInstr(1, FilenameSegment, "')");
                                                                        } else if (genericController.vbLCase(FilePrefixSegment.Substring(Ptr - 1, 2)) == "(\"") {
                                                                            EndPos = genericController.vbInstr(1, FilenameSegment, "\")");
                                                                        } else {
                                                                            EndPos = genericController.vbInstr(1, FilenameSegment, ")");
                                                                        }
                                                                        //INSTANT C# WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                                        //ORIGINAL LINE: Exit For
                                                                        goto ExitLabel1;
                                                                    case "'":
                                                                        //
                                                                        // Delimited within a javascript pair of apostophys
                                                                        //
                                                                        EndPos = genericController.vbInstr(1, FilenameSegment, "'");
                                                                        //INSTANT C# WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                                        //ORIGINAL LINE: Exit For
                                                                        goto ExitLabel1;
                                                                    case ">":
                                                                    case "<":
                                                                        //
                                                                        // Skip this link
                                                                        //
                                                                        ParseError = true;
                                                                        //INSTANT C# WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                                        //ORIGINAL LINE: Exit For
                                                                        goto ExitLabel1;
                                                                }
                                                            }
                                                            ExitLabel1:
                                                            //
                                                            // check link
                                                            //
                                                            if (EndPos == 0) {
                                                                ParseError = true;
                                                                break;
                                                            } else {
                                                                string ImageFilename = null;
                                                                string SegmentAfterImage = null;

                                                                string ImageFilenameNoExt = null;
                                                                string ImageFilenameExt = null;
                                                                string ImageAltSize = null;

                                                                //'hint = hint & ",120"
                                                                SegmentAfterImage = FilenameSegment.Substring(EndPos - 1);
                                                                ImageFilename = genericController.DecodeResponseVariable(FilenameSegment.Substring(0, EndPos - 1));
                                                                ImageFilenameNoExt = ImageFilename;
                                                                ImageFilenameExt = "";
                                                                Pos = ImageFilename.LastIndexOf(".") + 1;
                                                                if (Pos > 0) {
                                                                    ImageFilenameNoExt = genericController.vbLCase(ImageFilename.Substring(0, Pos - 1));
                                                                    ImageFilenameExt = genericController.vbLCase(ImageFilename.Substring(Pos));
                                                                }
                                                                //
                                                                // Get ImageAltSize
                                                                //
                                                                //'hint = hint & ",130"
                                                                ImageAltSize = "";
                                                                if (ImageFilenameNoExt == RecordFilenameNoExt) {
                                                                    //
                                                                    // Exact match
                                                                    //
                                                                } else if (genericController.vbInstr(1, ImageFilenameNoExt, RecordFilenameNoExt, Microsoft.VisualBasic.Constants.vbTextCompare) != 1) {
                                                                    //
                                                                    // There was a change and the recordfilename is not part of the imagefilename
                                                                    //
                                                                } else {
                                                                    //
                                                                    // the recordfilename is the first part of the imagefilename - Get ImageAltSize
                                                                    //
                                                                    ImageAltSize = ImageFilenameNoExt.Substring(RecordFilenameNoExt.Length);
                                                                    if (ImageAltSize.Substring(0, 1) != "-") {
                                                                        ImageAltSize = "";
                                                                    } else {
                                                                        ImageAltSize = ImageAltSize.Substring(1);
                                                                        SizeTest = ImageAltSize.Split('x');
                                                                        if (SizeTest.GetUpperBound(0) != 1) {
                                                                            ImageAltSize = "";
                                                                        } else {
                                                                            if (genericController.vbIsNumeric(SizeTest[0]) & genericController.vbIsNumeric(SizeTest[1])) {
                                                                                ImageFilenameNoExt = RecordFilenameNoExt;
                                                                                //ImageFilenameNoExt = Mid(ImageFilenameNoExt, 1, Pos - 1)
                                                                                //RecordFilenameNoExt = Mid(RecordFilename, 1, Pos - 1)
                                                                            } else {
                                                                                ImageAltSize = "";
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                //
                                                                // problem - in the case where the recordfilename = img-100x200, the imagefilenamenoext is img
                                                                //
                                                                //'hint = hint & ",140"
                                                                if ((RecordFilenameNoExt != ImageFilenameNoExt) | (RecordFilenameExt != ImageFilenameExt)) {
                                                                    //
                                                                    // There has been a change
                                                                    //
                                                                    string NewRecordFilename = null;
                                                                    int ImageHeight = 0;
                                                                    int ImageWidth = 0;
                                                                    NewRecordFilename = RecordVirtualPath + RecordFilenameNoExt + "." + RecordFilenameExt;
                                                                    //
                                                                    // realtime image updates replace without creating new size - that is for the edit interface
                                                                    //
                                                                    // put the New file back into the tablesplit in case there are more then 4 splits
                                                                    //
                                                                    TableSplit[0] = "";
                                                                    TableSplit[1] = "";
                                                                    TableSplit[2] = "";
                                                                    TableSplit[3] = SegmentAfterImage;
                                                                    NewRecordFilename = genericController.EncodeURL(NewRecordFilename) + ((string)(string.Join("/", TableSplit))).Substring(3);
                                                                    LinkSplit[LinkPtr] = NewRecordFilename;
                                                                    SaveChanges = true;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (ParseError) {
                            break;
                        }
                    }
                    //'hint = hint & ",910"
                    if (SaveChanges && (!ParseError)) {
                        result = string.Join(ContentFilesLinkPrefix, LinkSplit);
                    }
                }
                //'hint = hint & ",920"
                if (!ParseError) {
                    //
                    // Convert ACTypeDynamicForm to Add-on
                    //
                    if (genericController.vbInstr(1, result, "<ac type=\"" + ACTypeDynamicForm, Microsoft.VisualBasic.Constants.vbTextCompare) != 0) {
                        result = genericController.vbReplace(result, "type=\"DYNAMICFORM\"", "TYPE=\"aggregatefunction\"", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
                        result = genericController.vbReplace(result, "name=\"DYNAMICFORM\"", "name=\"DYNAMIC FORM\"", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
                    }
                }
                //'hint = hint & ",930"
                if (ParseError) {
                    result = ""
                    + Environment.NewLine + "<!-- warning: parsing aborted on ccLibraryFile replacement -->"
                    + Environment.NewLine + result + Environment.NewLine + "<!-- /warning: parsing aborted on ccLibraryFile replacement -->";
                }
                //
                // {{content}} should be <ac type="templatecontent" etc>
                // the merge is now handled in csv_EncodeActiveContent, but some sites have hand {{content}} tags entered
                //
                //'hint = hint & ",940"
                if (genericController.vbInstr(1, result, "{{content}}", Microsoft.VisualBasic.Constants.vbTextCompare) != 0) {
                    result = genericController.vbReplace(result, "{{content}}", "<AC type=\"" + ACTypeTemplateContent + "\">", 1, 99, Microsoft.VisualBasic.Constants.vbTextCompare);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            return result;
        }
        //
        //============================================================================
        //   csv_GetContentCopy3
        //       To get them, cp.content.getCopy must call the cpCoreClass version, which calls this for the content
        //============================================================================
        //
        public string html_GetContentCopy(string CopyName, string DefaultContent, int personalizationPeopleId, bool AllowEditWrapper, bool personalizationIsAuthenticated) {
            string returnCopy = "";
            try {
                //
                int CS = 0;
                int RecordID = 0;
                int contactPeopleId = 0;
                string Return_ErrorMessage = "";
                //
                // honestly, not sure what to do with 'return_ErrorMessage'
                //
                CS = cpCore.db.csOpen("copy content", "Name=" + cpCore.db.encodeSQLText(CopyName), "ID",, 0,,, "Name,ID,Copy,modifiedBy");
                if (!cpCore.db.csOk(CS)) {
                    cpCore.db.csClose(ref CS);
                    CS = cpCore.db.csInsertRecord("copy content", 0);
                    if (cpCore.db.csOk(CS)) {
                        RecordID = cpCore.db.csGetInteger(CS, "ID");
                        cpCore.db.csSet(CS, "name", CopyName);
                        cpCore.db.csSet(CS, "copy", genericController.encodeText(DefaultContent));
                        cpCore.db.csSave2(CS);
                        //   Call cpCore.workflow.publishEdit("copy content", RecordID)
                    }
                }
                if (cpCore.db.csOk(CS)) {
                    RecordID = cpCore.db.csGetInteger(CS, "ID");
                    contactPeopleId = cpCore.db.csGetInteger(CS, "modifiedBy");
                    returnCopy = cpCore.db.csGet(CS, "Copy");
                    returnCopy = executeContentCommands(null, returnCopy, CPUtilsBaseClass.addonContext.ContextPage, personalizationPeopleId, personalizationIsAuthenticated, ref Return_ErrorMessage);
                    returnCopy = convertActiveContentToHtmlForWebRender(returnCopy, "copy content", RecordID, personalizationPeopleId, "", 0, CPUtilsBaseClass.addonContext.ContextPage);
                    //returnCopy = convertActiveContent_internal(returnCopy, personalizationPeopleId, "copy content", RecordID, contactPeopleId, False, False, True, True, False, True, "", "", False, 0, "", CPUtilsBaseClass.addonContext.ContextPage, False, Nothing, False)
                    //
                    if (true) {
                        if (cpCore.doc.authContext.isEditingAnything()) {
                            returnCopy = cpCore.db.csGetRecordEditLink(CS, false) + returnCopy;
                            if (AllowEditWrapper) {
                                returnCopy = getEditWrapper("copy content", returnCopy);
                            }
                        }
                    }
                }
                cpCore.db.csClose(ref CS);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnCopy;
        }
        //
        //
        //
        public void main_AddTabEntry(string Caption, string Link, bool IsHit, string StylePrefix = "", string LiveBody = "") {
            try {
                //
                // should use the ccNav object, no the ccCommon module for this code
                //
                cpCore.menuTab.AddEntry(genericController.encodeText(Caption), genericController.encodeText(Link), genericController.EncodeBoolean(IsHit), genericController.encodeText(StylePrefix));

                //Call ccAddTabEntry(genericController.encodeText(Caption), genericController.encodeText(Link), genericController.EncodeBoolean(IsHit), genericController.encodeText(StylePrefix), genericController.encodeText(LiveBody))
                //
                return;
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_AddTabEntry")
        }
        //        '
        //        '
        //        '
        //        Public Function main_GetTabs() As String
        //            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetTabs")
        //            '
        //            ' should use the ccNav object, no the ccCommon module for this code
        //            '
        //            '
        //            main_GetTabs = menuTab.GetTabs()
        //            '    main_GetTabs = ccGetTabs()
        //            '
        //            Exit Function
        ////ErrorTrap:
        //            throw new applicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetTabs")
        //        End Function
        //
        //
        //
        public void main_AddLiveTabEntry(string Caption, string LiveBody, string StylePrefix = "") {
            try {
                //
                // should use the ccNav object, no the ccCommon module for this code
                //
                if (cpCore.doc.menuLiveTab == null) {
                    cpCore.doc.menuLiveTab = new menuLiveTabController();
                }
                cpCore.doc.menuLiveTab.AddEntry(genericController.encodeText(Caption), genericController.encodeText(LiveBody), genericController.encodeText(StylePrefix));
                //
                return;
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_AddLiveTabEntry")
        }
        //
        //
        //
        public string main_GetLiveTabs() {
            try {
                //
                // should use the ccNav object, no the ccCommon module for this code
                //
                if (cpCore.doc.menuLiveTab == null) {
                    cpCore.doc.menuLiveTab = new menuLiveTabController();
                }
                return cpCore.doc.menuLiveTab.GetTabs();
                //
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_GetLiveTabs")
        }
        //
        //
        //
        public void menu_AddComboTabEntry(string Caption, string Link, string AjaxLink, string LiveBody, bool IsHit, string ContainerClass) {
            try {
                //
                // should use the ccNav object, no the ccCommon module for this code
                //
                if (cpCore.doc.menuComboTab == null) {
                    cpCore.doc.menuComboTab = new menuComboTabController();
                }
                cpCore.doc.menuComboTab.AddEntry(Caption, Link, AjaxLink, LiveBody, IsHit, ContainerClass);
                //
                return;
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_AddComboTabEntry")
        }
        //
        //
        //
        public string menu_GetComboTabs() {
            try {
                //
                // should use the ccNav object, no the ccCommon module for this code
                //
                if (cpCore.doc.menuComboTab == null) {
                    cpCore.doc.menuComboTab = new menuComboTabController();
                }
                return cpCore.doc.menuComboTab.GetTabs();
                //
            } catch (Exception ex) {
                cpCore.handleException(ex);
            }
            //ErrorTrap:
            throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("main_GetComboTabs")
        }
        //'
        //'================================================================================================================
        //'   main_Get SharedStyleFilelist
        //'
        //'   SharedStyleFilelist is a list of filenames (with conditional comments) that should be included on pages
        //'   that call out the SharedFileIDList
        //'
        //'   Suffix and Prefix are for Conditional Comments around the style tag
        //'
        //'   SharedStyleFileList is
        //'       crlf filename < Prefix< Suffix
        //'       crlf filename < Prefix< Suffix
        //'       ...
        //'       Prefix and Suffix are htmlencoded
        //'
        //'   SharedStyleMap file
        //'       crlf StyleID tab StyleFilename < Prefix < Suffix, IncludedStyleFilename < Prefix < Suffix, ...
        //'       crlf StyleID tab StyleFilename < Prefix < Suffix, IncludedStyleFilename < Prefix < Suffix, ...
        //'       ...
        //'       StyleID is 0 if Always include is set
        //'       The Prefix and Suffix have had crlf removed, and comma replaced with &#44;
        //'================================================================================================================
        //'
        //Friend Shared Function main_GetSharedStyleFileList(cpCore As coreClass, SharedStyleIDList As String, main_IsAdminSite As Boolean) As String
        //    Dim result As String = ""
        //    '
        //    Dim Prefix As String
        //    Dim Suffix As String
        //    Dim Files() As String
        //    Dim Pos As Integer
        //    Dim SrcID As Integer
        //    Dim Srcs() As String
        //    Dim SrcCnt As Integer
        //    Dim IncludedStyleFilename As String
        //    Dim styleId As Integer
        //    Dim LastStyleID As Integer
        //    Dim CS As Integer
        //    Dim Ptr As Integer
        //    Dim MapList As String
        //    Dim Map() As String
        //    Dim MapCnt As Integer
        //    Dim MapRow As Integer
        //    Dim Filename As String
        //    Dim FileList As String
        //    Dim SQL As String = String.Empty
        //    Dim BakeName As String
        //    '
        //    If main_IsAdminSite Then
        //        BakeName = "SharedStyleMap-Admin"
        //    Else
        //        BakeName = "SharedStyleMap-Public"
        //    End If
        //    MapList = genericController.encodeText(cpCore.cache.getObject(Of String)(BakeName))
        //    If MapList = "" Then
        //        '
        //        ' BuildMap
        //        '
        //        MapList = ""
        //        If True Then
        //            '
        //            ' add prefix and suffix conditional comments
        //            '
        //            SQL = "select s.ID,s.Stylefilename,s.Prefix,s.Suffix,i.StyleFilename as iStylefilename,s.AlwaysInclude,i.Prefix as iPrefix,i.Suffix as iSuffix" _
        //                & " from ((ccSharedStyles s" _
        //                & " left join ccSharedStylesIncludeRules r on r.StyleID=s.id)" _
        //                & " left join ccSharedStyles i on i.id=r.IncludedStyleID)" _
        //                & " where ( s.active<>0 )and((i.active is null)or(i.active<>0))"
        //        End If
        //        CS = cpCore.db.cs_openSql(SQL)
        //        LastStyleID = 0
        //        Do While cpCore.db.cs_ok(CS)
        //            styleId = cpCore.db.cs_getInteger(CS, "ID")
        //            If styleId <> LastStyleID Then
        //                Filename = cpCore.db.cs_get(CS, "StyleFilename")
        //                Prefix = genericController.vbReplace(cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "Prefix")), ",", "&#44;")
        //                Suffix = genericController.vbReplace(cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "Suffix")), ",", "&#44;")
        //                If (Not main_IsAdminSite) And cpCore.db.cs_getBoolean(CS, "alwaysinclude") Then
        //                    MapList = MapList & vbCrLf & "0" & vbTab & Filename & "<" & Prefix & "<" & Suffix
        //                Else
        //                    MapList = MapList & vbCrLf & styleId & vbTab & Filename & "<" & Prefix & "<" & Suffix
        //                End If
        //            End If
        //            IncludedStyleFilename = cpCore.db.cs_getText(CS, "iStylefilename")
        //            Prefix = cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "iPrefix"))
        //            Suffix = cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "iSuffix"))
        //            If IncludedStyleFilename <> "" Then
        //                MapList = MapList & "," & IncludedStyleFilename & "<" & Prefix & "<" & Suffix
        //            End If
        //            Call cpCore.db.cs_goNext(CS)
        //        Loop
        //        If MapList = "" Then
        //            MapList = ","
        //        End If
        //        Call cpCore.cache.setObject(BakeName, MapList, "Shared Styles")
        //    End If
        //    If (MapList <> "") And (MapList <> ",") Then
        //        Srcs = Split(SharedStyleIDList, ",")
        //        SrcCnt = UBound(Srcs) + 1
        //        Map = Split(MapList, vbCrLf)
        //        MapCnt = UBound(Map) + 1
        //        '
        //        ' Add stylesheets with AlwaysInclude set (ID is saved as 0 in Map)
        //        '
        //        FileList = ""
        //        For MapRow = 0 To MapCnt - 1
        //            If genericController.vbInstr(1, Map(MapRow), "0" & vbTab) = 1 Then
        //                Pos = genericController.vbInstr(1, Map(MapRow), vbTab)
        //                If Pos > 0 Then
        //                    FileList = FileList & "," & Mid(Map(MapRow), Pos + 1)
        //                End If
        //            End If
        //        Next
        //        '
        //        ' create a filelist of everything that is needed, might be duplicates
        //        '
        //        For Ptr = 0 To SrcCnt - 1
        //            SrcID = genericController.EncodeInteger(Srcs[Ptr])
        //            If SrcID <> 0 Then
        //                For MapRow = 0 To MapCnt - 1
        //                    If genericController.vbInstr(1, Map(MapRow), SrcID & vbTab) <> 0 Then
        //                        Pos = genericController.vbInstr(1, Map(MapRow), vbTab)
        //                        If Pos > 0 Then
        //                            FileList = FileList & "," & Mid(Map(MapRow), Pos + 1)
        //                        End If
        //                    End If
        //                Next
        //            End If
        //        Next
        //        '
        //        ' dedup the filelist and convert it to crlf delimited
        //        '
        //        If FileList <> "" Then
        //            Files = Split(FileList, ",")
        //            For Ptr = 0 To UBound(Files)
        //                Filename = Files[Ptr]
        //                If genericController.vbInstr(1, result, Filename, vbTextCompare) = 0 Then
        //                    result = result & vbCrLf & Filename
        //                End If
        //            Next
        //        End If
        //    End If
        //    Return result
        //End Function

        //
        //
        //
        public string main_GetResourceLibrary2(string RootFolderName, bool AllowSelectResource, string SelectResourceEditorName, string SelectLinkObjectName, bool AllowGroupAdd) {
            string addonGuidResourceLibrary = "{564EF3F5-9673-4212-A692-0942DD51FF1A}";
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            arguments.Add("RootFolderName", RootFolderName);
            arguments.Add("AllowSelectResource", AllowSelectResource.ToString());
            arguments.Add("SelectResourceEditorName", SelectResourceEditorName);
            arguments.Add("SelectLinkObjectName", SelectLinkObjectName);
            arguments.Add("AllowGroupAdd", AllowGroupAdd.ToString());
            return cpCore.addon.execute(addonModel.create(cpCore, addonGuidResourceLibrary), new CPUtilsBaseClass.addonExecuteContext() {
                addonType = CPUtilsBaseClass.addonContext.ContextAdmin,
                instanceArguments = arguments
            });
            //Dim Option_String As String
            //Option_String = "" _
            //    & "RootFolderName=" & RootFolderName _
            //    & "&AllowSelectResource=" & AllowSelectResource _
            //    & "&SelectResourceEditorName=" & SelectResourceEditorName _
            //    & "&SelectLinkObjectName=" & SelectLinkObjectName _
            //    & "&AllowGroupAdd=" & AllowGroupAdd _
            //    & ""

            //Return cpCore.addon.execute_legacy4(addonGuidResourceLibrary, Option_String, CPUtilsBaseClass.addonContext.ContextAdmin)
        }
        //
        //========================================================================
        // Read and save a main_GetFormInputCheckList
        //   see main_GetFormInputCheckList for an explaination of the input
        //========================================================================
        //
        public void main_ProcessCheckList(string TagName, string PrimaryContentName, string PrimaryRecordID, string SecondaryContentName, string RulesContentName, string RulesPrimaryFieldname, string RulesSecondaryFieldName) {
            //
            string rulesTablename = null;
            string SQL = null;
            DataTable currentRules = null;
            int currentRulesCnt = 0;
            bool RuleFound = false;
            int RuleId = 0;
            int Ptr = 0;
            int TestRecordIDLast = 0;
            int TestRecordID = 0;
            string dupRuleIdList = null;
            int GroupCnt = 0;
            int GroupPtr = 0;
            string MethodName = null;
            int SecondaryRecordID = 0;
            bool RuleNeeded = false;
            int CSRule = 0;
            bool RuleContentChanged = false;
            bool SupportRuleCopy = false;
            string RuleCopy = null;
            //
            MethodName = "ProcessCheckList";
            //
            // --- create Rule records for all selected
            //
            GroupCnt = cpCore.docProperties.getInteger(TagName + ".RowCount");
            if (GroupCnt > 0) {
                //
                // Test if RuleCopy is supported
                //
                SupportRuleCopy = Models.Complex.cdefModel.isContentFieldSupported(cpCore, RulesContentName, "RuleCopy");
                if (SupportRuleCopy) {
                    SupportRuleCopy = SupportRuleCopy && Models.Complex.cdefModel.isContentFieldSupported(cpCore, SecondaryContentName, "AllowRuleCopy");
                    if (SupportRuleCopy) {
                        SupportRuleCopy = SupportRuleCopy && Models.Complex.cdefModel.isContentFieldSupported(cpCore, SecondaryContentName, "RuleCopyCaption");
                    }
                }
                //
                // Go through each checkbox and check for a rule
                //
                //
                // try
                //
                currentRulesCnt = 0;
                dupRuleIdList = "";
                rulesTablename = Models.Complex.cdefModel.getContentTablename(cpCore, RulesContentName);
                SQL = "select " + RulesSecondaryFieldName + ",id from " + rulesTablename + " where (" + RulesPrimaryFieldname + "=" + PrimaryRecordID + ")and(active<>0) order by " + RulesSecondaryFieldName;
                currentRulesCnt = 0;
                currentRules = cpCore.db.executeQuery(SQL);
                currentRulesCnt = currentRules.Rows.Count;
                for (GroupPtr = 0; GroupPtr < GroupCnt; GroupPtr++) {
                    //
                    // ----- Read Response
                    //
                    SecondaryRecordID = cpCore.docProperties.getInteger(TagName + "." + GroupPtr + ".ID");
                    RuleCopy = cpCore.docProperties.getText(TagName + "." + GroupPtr + ".RuleCopy");
                    RuleNeeded = cpCore.docProperties.getBoolean(TagName + "." + GroupPtr);
                    //
                    // ----- Update Record
                    //
                    RuleFound = false;
                    RuleId = 0;
                    TestRecordIDLast = 0;
                    for (Ptr = 0; Ptr < currentRulesCnt; Ptr++) {
                        TestRecordID = genericController.EncodeInteger(currentRules.Rows[Ptr].Item(0));
                        if (TestRecordID == 0) {
                            //
                            // skip
                            //
                        } else if (TestRecordID == SecondaryRecordID) {
                            //
                            // hit
                            //
                            RuleFound = true;
                            RuleId = genericController.EncodeInteger(currentRules.Rows[Ptr].Item(1));
                            break;
                        } else if (TestRecordID == TestRecordIDLast) {
                            //
                            // dup
                            //
                            dupRuleIdList = dupRuleIdList + "," + genericController.EncodeInteger(currentRules.Rows[Ptr].Item(1));
                            currentRules.Rows[Ptr].Item(0) = 0;
                        }
                        TestRecordIDLast = TestRecordID;
                    }
                    if (SupportRuleCopy && RuleNeeded && (RuleFound)) {
                        //
                        // Record exists and is needed, update the rule copy
                        //
                        SQL = "update " + rulesTablename + " set rulecopy=" + cpCore.db.encodeSQLText(RuleCopy) + " where id=" + RuleId;
                        cpCore.db.executeQuery(SQL);
                    } else if (RuleNeeded && (!RuleFound)) {
                        //
                        // No record exists, and one is needed
                        //
                        CSRule = cpCore.db.csInsertRecord(RulesContentName);
                        if (cpCore.db.csOk(CSRule)) {
                            cpCore.db.csSet(CSRule, "Active", RuleNeeded);
                            cpCore.db.csSet(CSRule, RulesPrimaryFieldname, PrimaryRecordID);
                            cpCore.db.csSet(CSRule, RulesSecondaryFieldName, SecondaryRecordID);
                            if (SupportRuleCopy) {
                                cpCore.db.csSet(CSRule, "RuleCopy", RuleCopy);
                            }
                        }
                        cpCore.db.csClose(ref CSRule);
                        RuleContentChanged = true;
                    } else if ((!RuleNeeded) && RuleFound) {
                        //
                        // Record exists and it is not needed
                        //
                        SQL = "delete from " + rulesTablename + " where id=" + RuleId;
                        cpCore.db.executeQuery(SQL);
                        RuleContentChanged = true;
                    }
                }
                //
                // delete dups
                //
                if (!string.IsNullOrEmpty(dupRuleIdList)) {
                    SQL = "delete from " + rulesTablename + " where id in (" + dupRuleIdList.Substring(1) + ")";
                    cpCore.db.executeQuery(SQL);
                    RuleContentChanged = true;
                }
            }
            if (RuleContentChanged) {
                cpCore.cache.invalidateAllObjectsInContent(RulesContentName);
            }
        }

        //'
        //'========================================================================
        //' ----- Ends an HTML page
        //'========================================================================
        //'
        //Public Function getHtmlDoc_afterBodyHtml() As String
        //    Return "" _
        //        & cr & "</body>" _
        //        & vbCrLf & "</html>"
        //End Function
        //
        //========================================================================
        // main_GetRecordEditLink( iContentName, iRecordID )
        //
        //   iContentName The content for this link
        //   iRecordID    The ID of the record in the Table
        //========================================================================
        //
        public string main_GetRecordEditLink(string ContentName, int RecordID, bool AllowCut = false) {
            return main_GetRecordEditLink2(ContentName, RecordID, genericController.EncodeBoolean(AllowCut), "", cpCore.doc.authContext.isEditing(ContentName));
        }
    }
}