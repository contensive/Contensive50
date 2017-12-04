

using Models.Entity;

using Controllers;
using Contensive.BaseClasses;

namespace Controllers {
    
    public class htmlController {
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' setOuter
        // '' </summary>
        // '' <param name="ignore"></param>
        // '' <param name="layout"></param>
        // '' <param name="Key"></param>
        // '' <param name="textToInsert"></param>
        // '' <returns></returns>
        // '' <remarks></remarks>
        public string insertOuterHTML(object ignore, string layout, string Key, string textToInsert) {
            string returnValue = "";
            try {
                if (string.IsNullOrEmpty(Key)) {
                    returnValue = textToInsert;
                }
                else {
                    returnValue = layout;
                    int posStart = this.getTagStartPos2(ignore, layout, 1, Key);
                    if ((posStart != 0)) {
                        int posEnd = this.getTagEndPos(ignore, layout, posStart);
                        if ((posEnd > 0)) {
                            // 
                            //  seems like these are the correct positions here.
                            // 
                            returnValue = (layout.Substring(0, (posStart - 1)) 
                                        + (textToInsert + layout.Substring((posEnd - 1))));
                        }
                        
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnValue;
        }
        
        // 
        // ====================================================================================================
        public string insertInnerHTML(object ignore, string layout, string Key, string textToInsert) {
            string returnValue = "";
            try {
                int posStart;
                int posEnd;
                // 
                //  short-cut for now, get the outerhtml, find the position, then remove the wrapping tags
                // 
                if ((Key == "")) {
                    returnValue = textToInsert;
                }
                else {
                    returnValue = layout;
                    posStart = this.getTagStartPos2(ignore, layout, 1, Key);
                    // outerHTML = getOuterHTML(ignore, layout, Key, PosStart)
                    if ((posStart != 0)) {
                        posEnd = this.getTagEndPos(ignore, layout, posStart);
                        if ((posEnd > 0)) {
                            posStart = genericController.vbInstr((posStart + 1), layout, ">");
                            if ((posStart != 0)) {
                                posStart = (posStart + 1);
                                posEnd = InStrRev(layout, "<", (posEnd - 1));
                                if ((posEnd != 0)) {
                                    returnValue = (layout.Substring(0, (posStart - 1)) 
                                                + (textToInsert + layout.Substring((posEnd - 1))));
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnValue;
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' getInnerHTML
        // '' </summary>
        // '' <param name="ignore"></param>
        // '' <param name="layout"></param>
        // '' <param name="Key"></param>
        // '' <returns></returns>
        // '' <remarks></remarks>
        public string getInnerHTML(object ignore, string layout, string Key) {
            string returnValue = "";
            try {
                int posStart;
                int posEnd;
                // 
                //  short-cut for now, get the outerhtml, find the position, then remove the wrapping tags
                // 
                if ((Key == "")) {
                    // 
                    //  inner of nothing is nothing
                    // 
                }
                else {
                    returnValue = layout;
                    posStart = this.getTagStartPos2(ignore, layout, 1, Key);
                    if ((posStart != 0)) {
                        posEnd = this.getTagEndPos(ignore, layout, posStart);
                        if ((posEnd > 0)) {
                            posStart = genericController.vbInstr((posStart + 1), layout, ">");
                            if ((posStart != 0)) {
                                posStart = (posStart + 1);
                                posEnd = InStrRev(layout, "<", (posEnd - 1));
                                if ((posEnd != 0)) {
                                    // 
                                    //  now move the end forward to skip trailing whitespace
                                    // 
                                    for (
                                    ; ((posEnd < layout.Length) 
                                                && ((('\t' + ("\r" + ("\n" + ('\t' + " ")))).IndexOf(layout.Substring((posEnd - 1), 1), 0) + 1) 
                                                != 0)); 
                                    ) {
                                        posEnd = (posEnd + 1);
                                    }
                                    
                                    posEnd = (posEnd - 1);
                                    returnValue = layout.Substring((posStart - 1), (posEnd - posStart));
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnValue;
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' getOuterHTML
        // '' </summary>
        // '' <param name="ignore"></param>
        // '' <param name="layout"></param>
        // '' <param name="Key"></param>
        // '' <returns></returns>
        // '' <remarks></remarks>
        public string getOuterHTML(object ignore, string layout, string Key) {
            string returnValue = "";
            try {
                int posStart;
                int posEnd;
                string s;
                // 
                s = layout;
                if ((s != "")) {
                    posStart = this.getTagStartPos2(ignore, s, 1, Key);
                    if ((posStart > 0)) {
                        // 
                        //  now backtrack to include the leading whitespace
                        // 
                        while (((posStart > 0) 
                                    && ((('\t' + ("\r" + ("\n" + ('\t' + " ")))).IndexOf(s.Substring((posStart - 1), 1), 0) + 1) 
                                    != 0))) {
                            posStart = (posStart - 1);
                        }
                        
                        // posStart = posStart + 1
                        s = s.Substring((posStart - 1));
                        posEnd = this.getTagEndPos(ignore, s, 1);
                        if ((posEnd > 0)) {
                            s = s.Substring(0, (posEnd - 1));
                            returnValue = s;
                        }
                        
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnValue;
        }
        
        // 
        // ====================================================================================================
        private bool tagMatch(string layout, int posStartTag, string searchId, string searchClass) {
            bool returnValue = false;
            try {
                const object attrAllowedChars = "abcdefghijklmnopqrstuvwzyz-_";
                string Tag;
                string tagLower;
                int Pos;
                string Delimiter;
                string testChar;
                int tagLength;
                int posValueStart;
                string testValue;
                string[] testValues;
                int testCnt;
                int Ptr;
                // 
                returnValue = false;
                Pos = genericController.vbInstr(posStartTag, layout, ">");
                if ((Pos > 0)) {
                    returnValue = true;
                    Tag = layout.Substring((posStartTag - 1), ((Pos - posStartTag) 
                                    + 1));
                    tagLower = genericController.vbLCase(Tag);
                    tagLength = Tag.Length;
                    // 
                    //  check searchId
                    // 
                    if ((returnValue 
                                && (searchId != ""))) {
                        Pos = genericController.vbInstr(1, tagLower, " id=", vbTextCompare);
                        if ((Pos <= 0)) {
                            // 
                            //  id required but this tag has no id attr
                            // 
                            returnValue = false;
                        }
                        else {
                            // 
                            //  test if the id attr value matches the searchClass
                            // 
                            Pos = (Pos + 4);
                            Delimiter = tagLower.Substring((Pos - 1), 1);
                            testValue = "";
                            if (((Delimiter == "\"") 
                                        || (Delimiter == "\'"))) {
                                // 
                                //  search for end of delimited attribute value
                                // 
                                posValueStart = (Pos + 1);
                                for (
                                ; ((Pos < tagLength) 
                                            && (testChar != Delimiter)); 
                                ) {
                                    Pos = (Pos + 1);
                                    testChar = tagLower.Substring((Pos - 1), 1);
                                }
                                
                                if ((Pos >= tagLength)) {
                                    // 
                                    //  delimiter not found, html error
                                    // 
                                    returnValue = false;
                                }
                                else {
                                    testValue = Tag.Substring((posValueStart - 1), (Pos - posValueStart));
                                }
                                
                            }
                            else {
                                // 
                                //  search for end of non-delimited attribute value
                                // 
                                posValueStart = Pos;
                                while (((Pos < tagLength) 
                                            && isInStr(1, attrAllowedChars, tagLower.Substring((Pos - 1), 1), vbTextCompare))) {
                                    Pos = (Pos + 1);
                                }
                                
                                if ((Pos >= tagLength)) {
                                    // 
                                    //  delimiter not found, html error
                                    // 
                                    returnValue = false;
                                }
                                else {
                                    testValue = Tag.Substring((posValueStart - 1), (Pos - posValueStart));
                                }
                                
                            }
                            
                            if ((returnValue 
                                        && (testValue != ""))) {
                                // 
                                // 
                                // 
                                if ((searchId != testValue)) {
                                    // 
                                    //  there can only be one id, and this does not match
                                    // 
                                    returnValue = false;
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                    // 
                    //  check searchClass
                    // 
                    if ((returnValue 
                                && (searchClass != ""))) {
                        Pos = genericController.vbInstr(1, tagLower, " class=", vbTextCompare);
                        if ((Pos <= 0)) {
                            // 
                            //  class required but this tag has no class attr
                            // 
                            returnValue = false;
                        }
                        else {
                            // 
                            //  test if the class attr value matches the searchClass
                            // 
                            Pos = (Pos + 7);
                            Delimiter = tagLower.Substring((Pos - 1), 1);
                            testValue = "";
                            if (((Delimiter == "\"") 
                                        || (Delimiter == "\'"))) {
                                // 
                                //  search for end of delimited attribute value
                                // 
                                posValueStart = (Pos + 1);
                                for (
                                ; ((Pos < tagLength) 
                                            && (testChar != Delimiter)); 
                                ) {
                                    Pos = (Pos + 1);
                                    testChar = tagLower.Substring((Pos - 1), 1);
                                }
                                
                                if ((Pos >= tagLength)) {
                                    // 
                                    //  delimiter not found, html error
                                    // 
                                    returnValue = false;
                                }
                                else {
                                    testValue = Tag.Substring((posValueStart - 1), (Pos - posValueStart));
                                }
                                
                            }
                            else {
                                // 
                                //  search for end of non-delimited attribute value
                                // 
                                posValueStart = Pos;
                                while (((Pos < tagLength) 
                                            && isInStr(1, attrAllowedChars, tagLower.Substring((Pos - 1), 1), vbTextCompare))) {
                                    Pos = (Pos + 1);
                                }
                                
                                if ((Pos >= tagLength)) {
                                    // 
                                    //  delimiter not found, html error
                                    // 
                                    returnValue = false;
                                }
                                else {
                                    testValue = Tag.Substring((posValueStart - 1), (Pos - posValueStart));
                                }
                                
                            }
                            
                            if ((returnValue 
                                        && (testValue != ""))) {
                                // 
                                // 
                                // 
                                testValues = testValue.Split(" ");
                                testCnt = (UBound(testValues) + 1);
                                for (Ptr = 0; (Ptr 
                                            <= (testCnt - 1)); Ptr++) {
                                    if ((searchClass == testValues[Ptr])) {
                                        break;
                                    }
                                    
                                }
                                
                                if ((Ptr >= testCnt)) {
                                    returnValue = false;
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnValue;
        }
        
        // 
        // ====================================================================================================
        public int getTagStartPos2(object ignore, string layout, int layoutStartPos, string Key) {
            int returnValue = 0;
            try {
                int returnPos;
                int SegmentStart;
                int Pos;
                int LoopPtr;
                string searchKey;
                int lenSearchKey;
                int Ptr;
                string workingKey;
                string[] workingKeys;
                string searchClass;
                string searchId;
                string searchTag;
                int posStartTag;
                // 
                returnPos = 0;
                workingKey = Key;
                if ((genericController.vbInstr(1, workingKey, ">") != 0)) {
                    // 
                    //  does not support > yet.
                    // 
                    workingKey = genericController.vbReplace(workingKey, ">", " ");
                }
                
                // 
                //  eliminate whitespace
                // 
                while ((genericController.vbInstr(1, workingKey, '\t') != 0)) {
                    workingKey = genericController.vbReplace(workingKey, '\t', " ");
                }
                
                // 
                while ((genericController.vbInstr(1, workingKey, "\r") != 0)) {
                    workingKey = genericController.vbReplace(workingKey, "\r", " ");
                }
                
                // 
                while ((genericController.vbInstr(1, workingKey, "\n") != 0)) {
                    workingKey = genericController.vbReplace(workingKey, "\n", " ");
                }
                
                // 
                while ((genericController.vbInstr(1, workingKey, "  ") != 0)) {
                    workingKey = genericController.vbReplace(workingKey, "  ", " ");
                }
                
                // 
                workingKey = workingKey.Trim();
                // 
                if ((genericController.vbInstr(1, workingKey, " ") != 0)) {
                    // 
                    //  if there are spaces, do them sequentially
                    // 
                    workingKeys = workingKey.Split(" ");
                    SegmentStart = 1;
                    while (((layout != "") 
                                && ((SegmentStart != 0) 
                                && (Ptr <= UBound(workingKeys))))) {
                        SegmentStart = this.getTagStartPos2(null, layout, SegmentStart, workingKeys[Ptr]);
                        Ptr = (Ptr + 1);
                    }
                    
                    returnPos = SegmentStart;
                }
                else {
                    // 
                    //  find this single key and get the outerHTML
                    //    at this point, the key can be
                    //        a class = .xxxx
                    //        an id = #xxxx
                    //        a tag = xxxx
                    //        a compound in either form, xxxx.xxxx or xxxx#xxxx
                    // 
                    //    searchKey = the search pattern to start
                    // 
                    if ((workingKey.Substring(0, 1) == ".")) {
                        // 
                        //  search for a class
                        // 
                        searchClass = workingKey.Substring(1);
                        searchTag = "";
                        searchId = "";
                        Pos = genericController.vbInstr(1, searchClass, "#");
                        if ((Pos != 0)) {
                            searchId = searchClass.Substring((Pos - 1));
                            searchClass = searchClass.Substring(0, (Pos - 1));
                        }
                        
                        // 
                        // workingKey = Mid(workingKey, 2)
                        searchKey = "<";
                    }
                    else if ((workingKey.Substring(0, 1) == "#")) {
                        // 
                        //  search for an ID
                        // 
                        searchClass = "";
                        searchTag = "";
                        searchId = workingKey.Substring(1);
                        Pos = genericController.vbInstr(1, searchId, ".");
                        if ((Pos != 0)) {
                            searchClass = searchId.Substring((Pos - 1));
                            searchId = searchId.Substring(0, (Pos - 1));
                        }
                        
                        // 
                        // workingKey = Mid(workingKey, 2)
                        searchKey = "<";
                    }
                    else {
                        // 
                        //  search for a tagname
                        // 
                        searchClass = "";
                        searchTag = workingKey;
                        searchId = "";
                        Pos = genericController.vbInstr(1, searchTag, "#");
                        if ((Pos != 0)) {
                            searchId = searchTag.Substring(Pos);
                            searchTag = searchTag.Substring(0, (Pos - 1));
                            Pos = genericController.vbInstr(1, searchId, ".");
                            if ((Pos != 0)) {
                                searchClass = searchId.Substring((Pos - 1));
                                searchId = searchId.Substring(0, (Pos - 1));
                            }
                            
                        }
                        
                        Pos = genericController.vbInstr(1, searchTag, ".");
                        if ((Pos != 0)) {
                            searchClass = searchTag.Substring(Pos);
                            searchTag = searchTag.Substring(0, (Pos - 1));
                            Pos = genericController.vbInstr(1, searchClass, "#");
                            if ((Pos != 0)) {
                                searchId = searchClass.Substring((Pos - 1));
                                searchClass = searchClass.Substring(0, (Pos - 1));
                            }
                            
                        }
                        
                        // 
                        searchKey = ("<" + searchTag);
                    }
                    
                    lenSearchKey = searchKey.Length;
                    Pos = layoutStartPos;
                    // posMatch = genericController.vbInstr(layoutStartPos, layout, searchKey)
                    // pos = posMatch
                    // searchIsOver = False
                    for (
                    ; (LoopPtr < 1000); 
                    ) {
                        Pos = genericController.vbInstr(Pos, layout, searchKey);
                        if ((Pos == 0)) {
                            // 
                            //  not found, return empty
                            // 
                            // s = ""
                            break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                        }
                        else {
                            // 
                            //  string found - go to the start of the tag
                            // 
                            posStartTag = InStrRev(layout, "<", (Pos + 1));
                            if ((posStartTag <= 0)) {
                                // 
                                //  bad html, no start tag found
                                // 
                                Pos = 0;
                                returnPos = 0;
                            }
                            else if ((layout.Substring((posStartTag - 1), 2) == "</")) {
                                // 
                                //  this is an end tag, skip it
                                // 
                                Pos = (Pos + 1);
                            }
                            else if (this.tagMatch(layout, posStartTag, searchId, searchClass)) {
                                // 
                                //  match, return with this position
                                // 
                                returnPos = Pos;
                                break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                            }
                            else {
                                // 
                                //  no match, skip this and go to the next
                                // 
                                Pos = (Pos + 1);
                            }
                            
                        }
                        
                        LoopPtr = (LoopPtr + 1);
                    }
                    
                    // 
                    // 
                    // 
                    if ((LoopPtr >= 10000)) {
                        cpCore.handleException(new ApplicationException("Tag limit of 10000 tags per block reached."));
                    }
                    
                }
                
                // 
                returnValue = returnPos;
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnValue;
        }
        
        // 
        // ====================================================================================================
        public int getTagStartPos(object ignore, string layout, int layoutStartPos, string Key) {
            int returnValue = 0;
            try {
                returnValue = this.getTagStartPos2(ignore, layout, layoutStartPos, Key);
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnValue;
        }
        
        // 
        // =================================================================================================
        //    return the position following the tag which closes the tag that starts the string
        //        starting postion<div><div><p>this and that</p><!-- </div> --></div></div>And a whole lot more
        //        returns the position of the "A" following the last /div
        //        string 123<img>456 returns pointing to "4"
        //        string 123456 returns pointing to "6"
        //        returns 0 if the end was not found
        // =================================================================================================
        // 
        public int getTagEndPos(object ignore, string Source, int startPos) {
            int returnValue = 0;
            try {
                int Pos;
                string TagName;
                string endTag;
                string startTag;
                int posNest;
                int posEnd;
                int posComment;
                string c;
                // 
                Pos = genericController.vbInstr(startPos, Source, "<");
                TagName = "";
                returnValue = 0;
                if ((Pos != 0)) {
                    Pos = (Pos + 1);
                    while ((Pos < Source.Length)) {
                        c = genericController.vbLCase(Source.Substring((Pos - 1), 1));
                        if (((c >= "a") 
                                    && (c <= "z"))) {
                            TagName = (TagName + c);
                        }
                        else {
                            break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                        }
                        
                        Pos = (Pos + 1);
                    }
                    
                    if ((TagName != "")) {
                        endTag = ("</" + TagName);
                        startTag = ("<" + TagName);
                        while ((Pos != 0)) {
                            posEnd = genericController.vbInstr((Pos + 1), Source, endTag, vbTextCompare);
                            if ((posEnd == 0)) {
                                // 
                                //  no end was found, return the tag or rest of the string
                                // 
                                returnValue = (genericController.vbInstr((Pos + 1), Source, ">") + 1);
                                if ((posEnd == 1)) {
                                    returnValue = Source.Length;
                                }
                                
                                break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                            }
                            else {
                                posNest = genericController.vbInstr((Pos + 1), Source, startTag, vbTextCompare);
                                if ((posNest == 0)) {
                                    // 
                                    //  no nest found, set to end
                                    // 
                                    posNest = Source.Length;
                                }
                                
                                posComment = genericController.vbInstr((Pos + 1), Source, "<!--");
                                if ((posComment == 0)) {
                                    // 
                                    //  no comment found, set to end
                                    // 
                                    posComment = Source.Length;
                                }
                                
                                if (((posNest < posEnd) 
                                            && (posNest < posComment))) {
                                    // 
                                    //  ----- the tag is nested, find the end of the nest
                                    // 
                                    Pos = this.getTagEndPos(ignore, Source, posNest);
                                    //  8/28/2012, if there is a nested tag right before the correct end tag, it skips the end:
                                    //  <div class=a>a<div class=b>b</div></div>
                                    //  the second /div is missed because returnValue returns one past the >, then the
                                    //  next search starts +1 that position
                                    if ((Pos > 0)) {
                                        Pos = (Pos - 1);
                                    }
                                    
                                }
                                else if ((posComment < posEnd)) {
                                    // 
                                    //  ----- there is a comment between the tag and the first tagend, skip it
                                    // 
                                    Pos = genericController.vbInstr(posComment, Source, "-->");
                                    if ((Pos == 0)) {
                                        // 
                                        //  start comment with no end, exit now
                                        // 
                                        returnValue = Source.Length;
                                        break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                                    }
                                    
                                }
                                else {
                                    // 
                                    //  ----- end position is here, go to the end of it and exit
                                    // 
                                    Pos = genericController.vbInstr(posEnd, Source, ">");
                                    if ((Pos == 0)) {
                                        // 
                                        //  no end was found, just exit
                                        // 
                                        returnValue = Source.Length;
                                        break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                                    }
                                    else {
                                        // 
                                        //  ----- end was found
                                        // 
                                        returnValue = (Pos + 1);
                                        break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                                    }
                                    
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnValue;
        }
        
        // 
        // ========================================================================================================
        // 
        //  Finds all tags matching the input, and concatinates them into the output
        //  does NOT account for nested tags, use for body, script, style
        // 
        //  ReturnAll - if true, it returns all the occurances, back-to-back
        // 
        // ========================================================================================================
        // 
        public static string getTagInnerHTML(string PageSource, string Tag, bool ReturnAll) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 
            int TagStart;
            int TagEnd;
            int LoopCnt;
            string WB;
            int Pos;
            int PosEnd;
            int CommentPos;
            int ScriptPos;
            // 
            getTagInnerHTML = "";
            Pos = 1;
            while (((Pos > 0) 
                        && (LoopCnt < 100))) {
                TagStart = genericController.vbInstr(Pos, PageSource, ("<" + Tag), vbTextCompare);
                if ((TagStart == 0)) {
                    Pos = 0;
                }
                else {
                    // 
                    //  tag found, skip any comments that start between current position and the tag
                    // 
                    CommentPos = genericController.vbInstr(Pos, PageSource, "<!--");
                    if (((CommentPos != 0) 
                                && (CommentPos < TagStart))) {
                        // 
                        //  skip comment and start again
                        // 
                        Pos = genericController.vbInstr(CommentPos, PageSource, "-->");
                    }
                    else {
                        ScriptPos = genericController.vbInstr(Pos, PageSource, "<script");
                        if (((ScriptPos != 0) 
                                    && (ScriptPos < TagStart))) {
                            // 
                            //  skip comment and start again
                            // 
                            Pos = genericController.vbInstr(ScriptPos, PageSource, "</script");
                        }
                        else {
                            // 
                            //  Get the tags innerHTML
                            // 
                            TagStart = genericController.vbInstr(TagStart, PageSource, ">", vbTextCompare);
                            Pos = TagStart;
                            if ((TagStart != 0)) {
                                TagStart = (TagStart + 1);
                                TagEnd = genericController.vbInstr(TagStart, PageSource, ("</" + Tag), vbTextCompare);
                                if ((TagEnd != 0)) {
                                    PageSource.Substring((TagStart - 1), (TagEnd - TagStart));
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                    LoopCnt = (LoopCnt + 1);
                    if (ReturnAll) {
                        TagStart = genericController.vbInstr(TagEnd, PageSource, ("<" + Tag), vbTextCompare);
                    }
                    else {
                        TagStart = 0;
                    }
                    
                }
                
            }
            
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
        ErrorTrap:
        }
        
        // 
        // ====================================================================================================
        // 
        public string getHtmlDoc_beforeEndOfBodyHtml(bool AllowLogin, bool AllowTools) {
            List<string> result = new List<string>();
            try {
                List<string> bodyScript = new List<string>();
                if ((cpCore.doc.authContext.isAuthenticatedContentManager(cpCore) && cpCore.doc.authContext.user.AllowToolsPanel)) {
                    if (AllowTools) {
                        result.Add(cpCore.html.main_GetToolsPanel());
                    }
                    
                }
                else if (AllowLogin) {
                    result.Add(main_GetLoginLink());
                }
                
                // 
                //  -- Include any other close page
                if ((cpCore.doc.htmlForEndOfBody != "")) {
                    result.Add(cpCore.doc.htmlForEndOfBody);
                }
                
                if ((cpCore.doc.testPointMessage != "")) {
                    result.Add(("<div class=\"ccTestPointMessageCon\">" 
                                    + (cpCore.doc.testPointMessage + "</div>")));
                }
                
                // 
                //  TODO -- closing the menu attaches the flyout panels -- should be done when the menu is returned, not at page end
                //  -- output the menu system
                if (!(cpCore.menuFlyout == null)) {
                    result.Add(cpCore.menuFlyout.menu_GetClose());
                }
                
                // 
                //  -- Add onload javascript
                foreach (htmlAssetClass asset in cpCore.doc.htmlAssetList.FindAll(Function, (a[a.assetType=htmlAssetTypeEnum.OnLoadScript] 
                                & !string.IsNullOrEmpty(a.content)))) {
                    result.Add(("<script Language=\"JavaScript\" type=\"text/javascript\">window.addEventListener(\'load\', function(){" 
                                    + (asset.content + "});</script>")));
                }
                
                // 
                //  -- body Javascript
                bool allowDebugging = cpCore.visitProperty.getBoolean("AllowDebugging");
                foreach (jsBody in cpCore.doc.htmlAssetList.FindAll(Function, (a[a.assetType=htmlAssetTypeEnum.script] 
                                & (!a.inHead 
                                & !string.IsNullOrEmpty(a.content))))) {
                    if (((jsBody.addedByMessage != "") 
                                && allowDebugging)) {
                        result.Add(("<!-- from " 
                                        + (jsBody.addedByMessage + " -->")));
                    }
                    
                    if (!jsBody.isLink) {
                        result.Add(("<script Language=\"JavaScript\" type=\"text/javascript\">" 
                                        + (jsBody.content + "</script>")));
                    }
                    else {
                        result.Add(("<script type=\"text/javascript\" src=\"" 
                                        + (jsBody.content + "\"></script>")));
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return string.Join(cr, result);
        }
        
        // 
        // ========================================================================
        //  main_Get a string with a Drop Down Select Box, see PrintFormInputSelect
        // ========================================================================
        // 
        public string main_GetFormInputSelect(string MenuName, int CurrentValue, string ContentName, string Criteria, void =, void , string NoneCaption, void =, void , string htmlId, void =, void ) {
            main_GetFormInputSelect = this.main_GetFormInputSelect2(MenuName, CurrentValue, ContentName, Criteria, NoneCaption, htmlId, false, "");
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
        }
        
        // 
        // 
        // 
        public string main_GetFormInputSelect2(string MenuName, int CurrentValue, string ContentName, string Criteria, string NoneCaption, string htmlId, ref bool return_IsEmptyList, string HtmlClass, void =, void ) {
            string result = String.Empty;
            // Warning!!! Optional parameters not supported
            try {
                const object MenuNameFPO = "<MenuName>";
                const object NoneCaptionFPO = "<NoneCaption>";
                Models.Complex.cdefModel CDef;
                string ContentControlCriteria;
                string LcaseCriteria;
                int CSPointer;
                bool SelectedFound;
                int RecordID;
                string Copy;
                string MethodName;
                string DropDownFieldList;
                string[] DropDownFieldName;
                string[] DropDownDelimiter;
                int DropDownFieldCount;
                string DropDownPreField = String.Empty;
                int DropDownFieldListLength;
                string FieldName = String.Empty;
                string CharAllowed;
                string CharTest;
                int CharPointer;
                int IDFieldPointer;
                stringBuilderLegacyController FastString = new stringBuilderLegacyController();
                string[,] RowsArray;
                string[] RowFieldArray;
                int RowCnt;
                int RowMax;
                int ColumnMax;
                int RowPointer;
                int ColumnPointer;
                int[] DropDownFieldPointer;
                string UcaseFieldName;
                string SortFieldList = String.Empty;
                string SQL;
                string TableName;
                string DataSource;
                string SelectFields;
                int Ptr;
                string SelectRaw = String.Empty;
                int CachePtr;
                string TagID;
                string CurrentValueText;
                // 
                MethodName = "main_GetFormInputSelect2";
                LcaseCriteria = genericController.vbLCase(Criteria);
                return_IsEmptyList = true;
                CurrentValueText = CurrentValue.ToString();
                if ((cpCore.doc.inputSelectCacheCnt > 0)) {
                    for (CachePtr = 0; (CachePtr 
                                <= (cpCore.doc.inputSelectCacheCnt - 1)); CachePtr++) {
                        // With...
                        if (((cpCore.doc.inputSelectCache(CachePtr).ContentName == ContentName) 
                                    && ((cpCore.doc.inputSelectCache(CachePtr).Criteria == LcaseCriteria) 
                                    && (cpCore.doc.inputSelectCache(CachePtr).CurrentValue == CurrentValueText)))) {
                            SelectRaw = cpCore.doc.inputSelectCache(CachePtr).SelectRaw;
                            return_IsEmptyList = false;
                            break;
                        }
                        
                    }
                    
                }
                
                // 
                // 
                // 
                if ((SelectRaw == "")) {
                    // 
                    //  Build the SelectRaw
                    //  Test selection size
                    // 
                    //  This was commented out -- I really do not know why -- seems like the best way
                    // 
                    CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
                    TableName = CDef.ContentTableName;
                    DataSource = CDef.ContentDataSourceName;
                    ContentControlCriteria = CDef.ContentControlCriteria;
                    // 
                    //  This is what was there
                    // 
                    //         TableName = main_GetContentProperty(ContentName, "ContentTableName")
                    //         DataSource = main_GetContentProperty(ContentName, "ContentDataSourceName")
                    //         ContentControlCriteria = main_GetContentProperty(ContentName, "ContentControlCriteria")
                    // 
                    SQL = ("select count(*) as cnt from " 
                                + (TableName + (" where " + ContentControlCriteria)));
                    if ((LcaseCriteria != "")) {
                        (" and " + LcaseCriteria);
                    }
                    
                    DataTable dt;
                    dt = cpCore.db.executeQuery(SQL);
                    if ((dt.Rows.Count > 0)) {
                        RowCnt = genericController.EncodeInteger(dt.Rows[0].Item["cnt"]);
                    }
                    
                    if ((RowCnt == 0)) {
                        RowMax = -1;
                    }
                    else {
                        return_IsEmptyList = false;
                        RowMax = (RowCnt - 1);
                    }
                    
                    // 
                    if ((RowCnt > cpCore.siteProperties.selectFieldLimit)) {
                        // 
                        //  Selection is too big
                        // 
                        errorController.error_AddUserError(cpCore, ("The drop down list for " 
                                        + (ContentName + (" called " 
                                        + (MenuName + (" is too long to display. The site administrator has been notified and the problem will be resolved sh" +
                                        "ortly. To fix this issue temporarily, go to the admin tab of the Preferences page and set the Select" +
                                        " Field Limit larger than " 
                                        + (RowCnt + ".")))))));
                        //                     cpcore.handleException(New Exception("Legacy error, MethodName=[" & MethodName & "], cause=[" & Cause & "] #" & Err.Number & "," & Err.Source & "," & Err.Description & ""), Cause, 2)
                        cpCore.handleException(new Exception(("Error creating select list from content [" 
                                            + (ContentName + ("] called [" 
                                            + (MenuName + ("]. Selection of [" 
                                            + (RowCnt + ("] records exceeds [" 
                                            + (cpCore.siteProperties.selectFieldLimit + "], the current Site Property SelectFieldLimit."))))))))));
                        result = (result + html_GetFormInputHidden(MenuNameFPO, CurrentValue));
                        if ((CurrentValue == 0)) {
                            result = html_GetFormInputText2(MenuName, "0");
                        }
                        else {
                            CSPointer = cpCore.db.csOpenRecord(ContentName, CurrentValue);
                            if (cpCore.db.csOk(CSPointer)) {
                                result = (cpCore.db.csGetText(CSPointer, "name") + " ");
                            }
                            
                            cpCore.db.csClose(CSPointer);
                        }
                        
                        result = (result + "(Selection is too large to display option list)");
                    }
                    else {
                        // 
                        //  ----- Generate Drop Down Field Names
                        // 
                        DropDownFieldList = CDef.DropDownFieldList;
                        // DropDownFieldList = main_GetContentProperty(ContentName, "DropDownFieldList")
                        if ((DropDownFieldList == "")) {
                            DropDownFieldList = "NAME";
                        }
                        
                        DropDownFieldCount = 0;
                        CharAllowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                        DropDownFieldListLength = DropDownFieldList.Length;
                        for (CharPointer = 1; (CharPointer <= DropDownFieldListLength); CharPointer++) {
                            CharTest = DropDownFieldList.Substring((CharPointer - 1), 1);
                            if ((genericController.vbInstr(1, CharAllowed, CharTest) == 0)) {
                                // 
                                //  Character not allowed, delimit Field name here
                                // 
                                if ((FieldName != "")) {
                                    // 
                                    //  ----- main_Get new Field Name and save it
                                    // 
                                    if ((SortFieldList == "")) {
                                        SortFieldList = FieldName;
                                    }
                                    
                                    object Preserve;
                                    DropDownFieldName[DropDownFieldCount];
                                    object Preserve;
                                    DropDownDelimiter[DropDownFieldCount];
                                    DropDownFieldName[DropDownFieldCount] = FieldName;
                                    DropDownDelimiter[DropDownFieldCount] = CharTest;
                                    DropDownFieldCount = (DropDownFieldCount + 1);
                                    FieldName = "";
                                }
                                else {
                                    // 
                                    //  ----- Save Field Delimiter
                                    // 
                                    if ((DropDownFieldCount == 0)) {
                                        // 
                                        //  ----- Before any field, add to DropDownPreField
                                        // 
                                        DropDownPreField = (DropDownPreField + CharTest);
                                    }
                                    else {
                                        // 
                                        //  ----- after a field, add to last DropDownDelimiter
                                        // 
                                        DropDownDelimiter[(DropDownFieldCount - 1)] = (DropDownDelimiter[(DropDownFieldCount - 1)] + CharTest);
                                    }
                                    
                                }
                                
                            }
                            else {
                                // 
                                //  Character Allowed, Put character into fieldname and continue
                                // 
                                FieldName = (FieldName + CharTest);
                            }
                            
                        }
                        
                        if ((FieldName != "")) {
                            if ((SortFieldList == "")) {
                                SortFieldList = FieldName;
                            }
                            
                            object Preserve;
                            DropDownFieldName[DropDownFieldCount];
                            object Preserve;
                            DropDownDelimiter[DropDownFieldCount];
                            DropDownFieldName[DropDownFieldCount] = FieldName;
                            DropDownDelimiter[DropDownFieldCount] = "";
                            DropDownFieldCount = (DropDownFieldCount + 1);
                        }
                        
                        if ((DropDownFieldCount == 0)) {
                            cpCore.handleException(new Exception(("No drop down field names found for content [" 
                                                + (ContentName + "]."))));
                        }
                        else {
                            object DropDownFieldPointer;
                            SelectFields = "ID";
                            for (Ptr = 0; (Ptr 
                                        <= (DropDownFieldCount - 1)); Ptr++) {
                                SelectFields = (SelectFields + ("," + DropDownFieldName[Ptr]));
                            }
                            
                            // 
                            //  ----- Start select box
                            // 
                            TagID = "";
                            if ((htmlId != "")) {
                                TagID = (" ID=\"" 
                                            + (htmlId + "\""));
                            }
                            
                            FastString.Add(("<select size=\"1\" name=\"" 
                                            + (MenuNameFPO + ("\"" 
                                            + (TagID + ">")))));
                            FastString.Add(("<option value=\"\">" 
                                            + (NoneCaptionFPO + "</option>")));
                            // 
                            //  ----- select values
                            // 
                            CSPointer = cpCore.db.csOpen(ContentName, Criteria, SortFieldList, ,, ,, SelectFields);
                            if (cpCore.db.csOk(CSPointer)) {
                                RowsArray = cpCore.db.cs_getRows(CSPointer);
                                RowFieldArray = cpCore.db.cs_getSelectFieldList(CSPointer).Split(",");
                                ColumnMax = UBound(RowsArray, 1);
                                RowMax = UBound(RowsArray, 2);
                                // 
                                //  -- setup IDFieldPointer
                                UcaseFieldName = "ID";
                                for (ColumnPointer = 0; (ColumnPointer <= ColumnMax); ColumnPointer++) {
                                    if ((UcaseFieldName == genericController.vbUCase(RowFieldArray[ColumnPointer]))) {
                                        IDFieldPointer = ColumnPointer;
                                        break;
                                    }
                                    
                                }
                                
                                // 
                                //  setup DropDownFieldPointer()
                                // 
                                for (FieldPointer = 0; (FieldPointer 
                                            <= (DropDownFieldCount - 1)); FieldPointer++) {
                                    UcaseFieldName = genericController.vbUCase(DropDownFieldName[FieldPointer]);
                                    for (ColumnPointer = 0; (ColumnPointer <= ColumnMax); ColumnPointer++) {
                                        if ((UcaseFieldName == genericController.vbUCase(RowFieldArray[ColumnPointer]))) {
                                            DropDownFieldPointer[FieldPointer] = ColumnPointer;
                                            break;
                                        }
                                        
                                    }
                                    
                                }
                                
                                // 
                                //  output select
                                // 
                                SelectedFound = false;
                                for (RowPointer = 0; (RowPointer <= RowMax); RowPointer++) {
                                    RecordID = genericController.EncodeInteger(RowsArray[IDFieldPointer, RowPointer]);
                                    Copy = DropDownPreField;
                                    for (FieldPointer = 0; (FieldPointer 
                                                <= (DropDownFieldCount - 1)); FieldPointer++) {
                                        Copy = (Copy 
                                                    + (RowsArray[DropDownFieldPointer[FieldPointer], RowPointer] + DropDownDelimiter[FieldPointer]));
                                    }
                                    
                                    if ((Copy == "")) {
                                        Copy = "no name";
                                    }
                                    
                                    FastString.Add(("\r\n" + ("<option value=\"" 
                                                    + (RecordID + "\" "))));
                                    if ((RecordID == CurrentValue)) {
                                        FastString.Add("selected");
                                        SelectedFound = true;
                                    }
                                    
                                    if ((cpCore.siteProperties.selectFieldWidthLimit != 0)) {
                                        if ((Copy.Length > cpCore.siteProperties.selectFieldWidthLimit)) {
                                            Copy = (Copy.Substring(0, cpCore.siteProperties.selectFieldWidthLimit) + "...+");
                                        }
                                        
                                    }
                                    
                                    FastString.Add((">" 
                                                    + (encodeHTML(Copy) + "</option>")));
                                }
                                
                                if ((!SelectedFound 
                                            && (CurrentValue != 0))) {
                                    cpCore.db.csClose(CSPointer);
                                    if ((Criteria != "")) {
                                        Criteria = (Criteria + "and");
                                    }
                                    
                                    Criteria = (Criteria + ("(id=" 
                                                + (genericController.EncodeInteger(CurrentValue) + ")")));
                                    CSPointer = cpCore.db.csOpen(ContentName, Criteria, SortFieldList, false, ,, ,, SelectFields);
                                    if (cpCore.db.csOk(CSPointer)) {
                                        RowsArray = cpCore.db.cs_getRows(CSPointer);
                                        RowFieldArray = cpCore.db.cs_getSelectFieldList(CSPointer).Split(",");
                                        RowMax = UBound(RowsArray, 2);
                                        ColumnMax = UBound(RowsArray, 1);
                                        RecordID = genericController.EncodeInteger(RowsArray[IDFieldPointer, 0]);
                                        Copy = DropDownPreField;
                                        for (FieldPointer = 0; (FieldPointer 
                                                    <= (DropDownFieldCount - 1)); FieldPointer++) {
                                            Copy = (Copy 
                                                        + (RowsArray[DropDownFieldPointer[FieldPointer], 0] + DropDownDelimiter[FieldPointer]));
                                        }
                                        
                                        if ((Copy == "")) {
                                            Copy = "no name";
                                        }
                                        
                                        FastString.Add(("\r\n" + ("<option value=\"" 
                                                        + (RecordID + "\" selected"))));
                                        SelectedFound = true;
                                        if ((cpCore.siteProperties.selectFieldWidthLimit != 0)) {
                                            if ((Copy.Length > cpCore.siteProperties.selectFieldWidthLimit)) {
                                                Copy = (Copy.Substring(0, cpCore.siteProperties.selectFieldWidthLimit) + "...+");
                                            }
                                            
                                        }
                                        
                                        FastString.Add((">" 
                                                        + (encodeHTML(Copy) + "</option>")));
                                    }
                                    
                                }
                                
                            }
                            
                            FastString.Add("</select>");
                            cpCore.db.csClose(CSPointer);
                            SelectRaw = FastString.Text;
                        }
                        
                    }
                    
                    // 
                    //  Save the SelectRaw
                    // 
                    if (!return_IsEmptyList) {
                        CachePtr = cpCore.doc.inputSelectCacheCnt;
                        cpCore.doc.inputSelectCacheCnt = (cpCore.doc.inputSelectCacheCnt + 1);
                        object Preserve;
                        cpCore.doc.inputSelectCache(Ptr);
                        object Preserve;
                        cpCore.doc.inputSelectCache(CachePtr);
                        cpCore.doc.inputSelectCache(CachePtr).ContentName = ContentName;
                        cpCore.doc.inputSelectCache(CachePtr).Criteria = LcaseCriteria;
                        cpCore.doc.inputSelectCache(CachePtr).CurrentValue = CurrentValue.ToString;
                        cpCore.doc.inputSelectCache(CachePtr).SelectRaw = SelectRaw;
                    }
                    
                }
                
                // 
                SelectRaw = genericController.vbReplace(SelectRaw, MenuNameFPO, MenuName);
                SelectRaw = genericController.vbReplace(SelectRaw, NoneCaptionFPO, NoneCaption);
                if ((HtmlClass != "")) {
                    SelectRaw = genericController.vbReplace(SelectRaw, "<select ", ("<select class=\"" 
                                    + (HtmlClass + "\"")));
                }
                
                result = SelectRaw;
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
            return result;
        }
        
        // 
        // ========================================================================
        // 
        // ========================================================================
        // 
        public string getInputMemberSelect(string MenuName, int CurrentValue, int GroupID, string ignore, void =, void , string NoneCaption, void =, void , string htmlId, void =, void ) {
            getInputMemberSelect = this.html_GetFormInputMemberSelect2(MenuName, CurrentValue, GroupID, ,, NoneCaption, htmlId);
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
        }
        
        // 
        public string html_GetFormInputMemberSelect2(string MenuName, int CurrentValue, int GroupID, string ignore, void =, void , string NoneCaption, void =, void , string HtmlId, void =, void , string HtmlClass, void =, void ) {
            string result = String.Empty;
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            try {
                int LastRecordID;
                string MemberRulesTableName;
                string iMenuName;
                int iCurrentValue;
                string iNoneCaption;
                int CSPointer;
                bool SelectedFound;
                int RecordID;
                string Copy;
                string MethodName;
                string DropDownFieldList;
                string[] DropDownFieldName;
                string[] DropDownDelimiter;
                int DropDownFieldCount;
                //  converted array to dictionary - Dim FieldPointer As Integer
                string DropDownPreField = String.Empty;
                int DropDownFieldListLength;
                string FieldName = String.Empty;
                string CharAllowed;
                string CharTest;
                int CharPointer;
                int IDFieldPointer;
                stringBuilderLegacyController FastString = new stringBuilderLegacyController();
                // 
                string[,] RowsArray;
                string[] RowFieldArray;
                int RowMax;
                int ColumnMax;
                int RowPointer;
                int ColumnPointer;
                int[] DropDownFieldPointer;
                string UcaseFieldName;
                string SortFieldList = String.Empty;
                string SQL;
                string PeopleTableName;
                string PeopleDataSource;
                string iCriteria = String.Empty;
                string SelectFields;
                int Ptr;
                string SelectRaw = String.Empty;
                int CachePtr;
                string TagID;
                string TagClass;
                // 
                const object MenuNameFPO = "<MenuName>";
                const object NoneCaptionFPO = "<NoneCaption>";
                MethodName = "main_GetFormInputMemberSelect2";
                iMenuName = genericController.encodeText(MenuName);
                iCurrentValue = genericController.EncodeInteger(CurrentValue);
                iNoneCaption = genericController.encodeEmptyText(NoneCaption, "Select One");
                // iCriteria = genericController.vbLCase(encodeMissingText(Criteria, ""))
                // 
                if ((cpCore.doc.inputSelectCacheCnt > 0)) {
                    for (CachePtr = 0; (CachePtr 
                                <= (cpCore.doc.inputSelectCacheCnt - 1)); CachePtr++) {
                        // With...
                        if ((((cpCore.doc.inputSelectCache(CachePtr).ContentName == "Group:") 
                                    + GroupID) 
                                    && ((cpCore.doc.inputSelectCache(CachePtr).Criteria == iCriteria) 
                                    && (genericController.EncodeInteger(cpCore.doc.inputSelectCache(CachePtr).CurrentValue) == iCurrentValue)))) {
                            SelectRaw = cpCore.doc.inputSelectCache(CachePtr).SelectRaw;
                            break;
                        }
                        
                    }
                    
                }
                
                // 
                // 
                // 
                if ((SelectRaw == "")) {
                    // 
                    //  Build the SelectRaw
                    //  Test selection size
                    // 
                    PeopleTableName = Models.Complex.cdefModel.getContentTablename(cpCore, "people");
                    PeopleDataSource = Models.Complex.cdefModel.getContentDataSource(cpCore, "People");
                    MemberRulesTableName = Models.Complex.cdefModel.getContentTablename(cpCore, "Member Rules");
                    // 
                    RowMax = 0;
                    SQL = ("select count(*) as cnt" + (" from ccMemberRules R" + (" inner join ccMembers P on R.MemberID=P.ID" + (" where (P.active<>0)" + (" and (R.GroupID=" 
                                + (GroupID + ")"))))));
                    CSPointer = cpCore.db.csOpenSql_rev(PeopleDataSource, SQL);
                    if (cpCore.db.csOk(CSPointer)) {
                        RowMax = (RowMax + cpCore.db.csGetInteger(CSPointer, "cnt"));
                    }
                    
                    cpCore.db.csClose(CSPointer);
                    // 
                    //         SQL = " select count(*) as cnt" _
                    //             & " from ccMembers P" _
                    //             & " where (active<>0)" _
                    //             & " and(( P.admin<>0 )or( P.developer<>0 ))"
                    //         CSPointer = app.csv_OpenCSSQL(PeopleDataSource, SQL, memberID)
                    //         If app.csv_IsCSOK(CSPointer) Then
                    //             RowMax = RowMax + app.csv_cs_getInteger(CSPointer, "cnt")
                    //         End If
                    //         Call app.closeCS(CSPointer)
                    // 
                    if ((RowMax > cpCore.siteProperties.selectFieldLimit)) {
                        // 
                        //  Selection is too big
                        // 
                        cpCore.handleException(new Exception(("While building a group members list for group [" 
                                            + (groupController.group_GetGroupName(cpCore, GroupID) + ("], too many rows were selected. [" 
                                            + (RowMax + ("] records exceeds [" 
                                            + (cpCore.siteProperties.selectFieldLimit + "], the current Site Property app.SiteProperty_SelectFieldLimit."))))))));
                        result = (result + html_GetFormInputHidden(MenuNameFPO, iCurrentValue));
                        if ((iCurrentValue != 0)) {
                            CSPointer = cpCore.db.csOpenRecord("people", iCurrentValue);
                            if (cpCore.db.csOk(CSPointer)) {
                                result = (cpCore.db.csGetText(CSPointer, "name") + " ");
                            }
                            
                            cpCore.db.csClose(CSPointer);
                        }
                        
                        result = (result + "(Selection is too large to display)");
                    }
                    else {
                        // 
                        //  ----- Generate Drop Down Field Names
                        // 
                        DropDownFieldList = Models.Complex.cdefModel.GetContentProperty(cpCore, "people", "DropDownFieldList");
                        if ((DropDownFieldList == "")) {
                            DropDownFieldList = "NAME";
                        }
                        
                        DropDownFieldCount = 0;
                        CharAllowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                        DropDownFieldListLength = DropDownFieldList.Length;
                        for (CharPointer = 1; (CharPointer <= DropDownFieldListLength); CharPointer++) {
                            CharTest = DropDownFieldList.Substring((CharPointer - 1), 1);
                            if ((genericController.vbInstr(1, CharAllowed, CharTest) == 0)) {
                                // 
                                //  Character not allowed, delimit Field name here
                                // 
                                if ((FieldName != "")) {
                                    // 
                                    //  ----- main_Get new Field Name and save it
                                    // 
                                    if ((SortFieldList == "")) {
                                        SortFieldList = FieldName;
                                    }
                                    
                                    object Preserve;
                                    DropDownFieldName[DropDownFieldCount];
                                    object Preserve;
                                    DropDownDelimiter[DropDownFieldCount];
                                    DropDownFieldName[DropDownFieldCount] = FieldName;
                                    DropDownDelimiter[DropDownFieldCount] = CharTest;
                                    DropDownFieldCount = (DropDownFieldCount + 1);
                                    FieldName = "";
                                }
                                else {
                                    // 
                                    //  ----- Save Field Delimiter
                                    // 
                                    if ((DropDownFieldCount == 0)) {
                                        // 
                                        //  ----- Before any field, add to DropDownPreField
                                        // 
                                        DropDownPreField = (DropDownPreField + CharTest);
                                    }
                                    else {
                                        // 
                                        //  ----- after a field, add to last DropDownDelimiter
                                        // 
                                        DropDownDelimiter[(DropDownFieldCount - 1)] = (DropDownDelimiter[(DropDownFieldCount - 1)] + CharTest);
                                    }
                                    
                                }
                                
                            }
                            else {
                                // 
                                //  Character Allowed, Put character into fieldname and continue
                                // 
                                FieldName = (FieldName + CharTest);
                            }
                            
                        }
                        
                        if ((FieldName != "")) {
                            if ((SortFieldList == "")) {
                                SortFieldList = FieldName;
                            }
                            
                            object Preserve;
                            DropDownFieldName[DropDownFieldCount];
                            object Preserve;
                            DropDownDelimiter[DropDownFieldCount];
                            DropDownFieldName[DropDownFieldCount] = FieldName;
                            DropDownDelimiter[DropDownFieldCount] = "";
                            DropDownFieldCount = (DropDownFieldCount + 1);
                        }
                        
                        if ((DropDownFieldCount == 0)) {
                            cpCore.handleException(new Exception(("No drop down field names found for content [" 
                                                + (GroupID + "]."))));
                        }
                        else {
                            object DropDownFieldPointer;
                            SelectFields = "P.ID";
                            for (Ptr = 0; (Ptr 
                                        <= (DropDownFieldCount - 1)); Ptr++) {
                                SelectFields = (SelectFields + (",P." + DropDownFieldName[Ptr]));
                            }
                            
                            // 
                            //  ----- Start select box
                            // 
                            TagClass = "";
                            if ((genericController.encodeEmptyText(HtmlClass, "") != "")) {
                                TagClass = (" Class=\"" 
                                            + (genericController.encodeEmptyText(HtmlClass, "") + "\""));
                            }
                            
                            // 
                            TagID = "";
                            if ((genericController.encodeEmptyText(HtmlId, "") != "")) {
                                TagID = (" ID=\"" 
                                            + (genericController.encodeEmptyText(HtmlId, "") + "\""));
                            }
                            
                            // 
                            FastString.Add(("<select size=\"1\" name=\"" 
                                            + (MenuNameFPO + ("\"" 
                                            + (TagID 
                                            + (TagClass + ">"))))));
                            FastString.Add(("<option value=\"\">" 
                                            + (NoneCaptionFPO + "</option>")));
                            // 
                            //  ----- select values
                            // 
                            if ((SortFieldList == "")) {
                                SortFieldList = "name";
                            }
                            
                            SQL = ("select " 
                                        + (SelectFields + (" from ccMemberRules R" + (" inner join ccMembers P on R.MemberID=P.ID" + (" where (R.GroupID=" 
                                        + (GroupID + (")" + (" and((R.DateExpires is null)or(R.DateExpires>" 
                                        + (cpCore.db.encodeSQLDate(Now) + ("))" + (" and(P.active<>0)" + (" order by P." + SortFieldList))))))))))));
                            //                 SQL = "select " & SelectFields _
                            //                     & " from ccMemberRules R" _
                            //                     & " inner join ccMembers P on R.MemberID=P.ID" _
                            //                     & " where (R.GroupID=" & GroupID & ")" _
                            //                     & " and((R.DateExpires is null)or(R.DateExpires>" & encodeSQLDate(Now) & "))" _
                            //                     & " and(P.active<>0)" _
                            //                     & " union" _
                            //                     & " select P.ID,P.NAME" _
                            //                     & " from ccMembers P" _
                            //                     & " where (active<>0)" _
                            //                     & " and(( P.admin<>0 )or( P.developer<>0 ))" _
                            //                     & " order by P." & SortFieldList
                            CSPointer = cpCore.db.csOpenSql_rev(PeopleDataSource, SQL);
                            if (cpCore.db.csOk(CSPointer)) {
                                RowsArray = cpCore.db.cs_getRows(CSPointer);
                                // RowFieldArray = app.csv_cs_getRowFields(CSPointer)
                                RowFieldArray = cpCore.db.cs_getSelectFieldList(CSPointer).Split(",");
                                RowMax = UBound(RowsArray, 2);
                                ColumnMax = UBound(RowsArray, 1);
                                // 
                                //  setup IDFieldPointer
                                // 
                                UcaseFieldName = "ID";
                                for (ColumnPointer = 0; (ColumnPointer <= ColumnMax); ColumnPointer++) {
                                    if ((UcaseFieldName == genericController.vbUCase(RowFieldArray[ColumnPointer]))) {
                                        IDFieldPointer = ColumnPointer;
                                        break;
                                    }
                                    
                                }
                                
                                // 
                                //  setup DropDownFieldPointer()
                                // 
                                for (FieldPointer = 0; (FieldPointer 
                                            <= (DropDownFieldCount - 1)); FieldPointer++) {
                                    UcaseFieldName = genericController.vbUCase(DropDownFieldName[FieldPointer]);
                                    for (ColumnPointer = 0; (ColumnPointer <= ColumnMax); ColumnPointer++) {
                                        if ((UcaseFieldName == genericController.vbUCase(RowFieldArray[ColumnPointer]))) {
                                            DropDownFieldPointer[FieldPointer] = ColumnPointer;
                                            break;
                                        }
                                        
                                    }
                                    
                                }
                                
                                // 
                                //  output select
                                // 
                                SelectedFound = false;
                                LastRecordID = -1;
                                for (RowPointer = 0; (RowPointer <= RowMax); RowPointer++) {
                                    RecordID = genericController.EncodeInteger(RowsArray[IDFieldPointer, RowPointer]);
                                    if ((RecordID != LastRecordID)) {
                                        Copy = DropDownPreField;
                                        for (FieldPointer = 0; (FieldPointer 
                                                    <= (DropDownFieldCount - 1)); FieldPointer++) {
                                            Copy = (Copy 
                                                        + (RowsArray[DropDownFieldPointer[FieldPointer], RowPointer] + DropDownDelimiter[FieldPointer]));
                                        }
                                        
                                        if ((Copy == "")) {
                                            Copy = "no name";
                                        }
                                        
                                        FastString.Add(("\r\n" + ("<option value=\"" 
                                                        + (RecordID + "\" "))));
                                        if ((RecordID == iCurrentValue)) {
                                            FastString.Add("selected");
                                            SelectedFound = true;
                                        }
                                        
                                        if ((cpCore.siteProperties.selectFieldWidthLimit != 0)) {
                                            if ((Copy.Length > cpCore.siteProperties.selectFieldWidthLimit)) {
                                                Copy = (Copy.Substring(0, cpCore.siteProperties.selectFieldWidthLimit) + "...+");
                                            }
                                            
                                        }
                                        
                                        FastString.Add((">" 
                                                        + (Copy + "</option>")));
                                        LastRecordID = RecordID;
                                    }
                                    
                                }
                                
                            }
                            
                            FastString.Add("</select>");
                            cpCore.db.csClose(CSPointer);
                            SelectRaw = FastString.Text;
                        }
                        
                    }
                    
                    // 
                    //  Save the SelectRaw
                    // 
                    CachePtr = cpCore.doc.inputSelectCacheCnt;
                    cpCore.doc.inputSelectCacheCnt = (cpCore.doc.inputSelectCacheCnt + 1);
                    object Preserve;
                    cpCore.doc.inputSelectCache(Ptr);
                    object Preserve;
                    cpCore.doc.inputSelectCache(CachePtr);
                    cpCore.doc.inputSelectCache(CachePtr).ContentName = ("Group:" + GroupID);
                    cpCore.doc.inputSelectCache(CachePtr).Criteria = iCriteria;
                    cpCore.doc.inputSelectCache(CachePtr).CurrentValue = iCurrentValue.ToString;
                    cpCore.doc.inputSelectCache(CachePtr).SelectRaw = SelectRaw;
                }
                
                // 
                SelectRaw = genericController.vbReplace(SelectRaw, MenuNameFPO, iMenuName);
                SelectRaw = genericController.vbReplace(SelectRaw, NoneCaptionFPO, iNoneCaption);
                result = SelectRaw;
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
            return result;
        }
    }
}