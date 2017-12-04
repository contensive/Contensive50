


using Contensive.BaseClasses;
// 
//  findReplace as integer to as integer
//  just the document -- replace out 
//  if 'Imports Interop.adodb, replace in ObjectStateEnum.adState...
//  findreplace encode to encode
//  findreplace ''DoEvents to '''DoEvents
//  runProcess becomes runProcess
//  Sleep becomes Threading.Thread.Sleep(
//  as object to as object

namespace Controllers {
    
    public class contentCmdController {
        
        // 
        private coreClass cpCore;
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' constructor
        // '' </summary>
        // '' <param name="cpCore"></param>
        // '' <remarks></remarks>
        public contentCmdController(coreClass cpCore) {
            this.cpCore = cpCore;
        }
        
        // '
        // '=================================================================================
        // Public Function execute(CsvObject As Object, mainObject As Object, optionString As String, filterInput As String) As String
        //     Dim returnValue As String = ""
        //     Try
        //         Dim src As String
        //         Dim Context As Integer
        //         Dim personalizationPeopleId As Integer
        //         Dim personalizationIsAuthenticated As Boolean
        //         '
        //         src = CsvObject.GetAddonOption("data", optionString)
        //         Context = genericController.EncodeInteger(CsvObject.GetAddonOption("context", optionString))
        //         personalizationPeopleId = genericController.EncodeInteger(CsvObject.GetAddonOption("personalizationPeopleId", optionString))
        //         personalizationIsAuthenticated = genericController.EncodeBoolean(CsvObject.GetAddonOption("personalizationIsAuthenticated", optionString))
        //         '
        //         ' compatibility with old Contensive
        //         '
        //         If (personalizationPeopleId = 0) And (Not (mainObject Is Nothing)) Then
        //             personalizationPeopleId = mainObject.MemberID
        //             personalizationIsAuthenticated = mainObject.isAuthenticated
        //         End If
        //         If src <> "" Then
        //             '
        //             ' test for Contensive processign instruction
        //             '
        //             execute = ExecuteCmd(src, Context, personalizationPeopleId, personalizationIsAuthenticated)
        //         End If
        //     Catch ex As Exception
        //         cpCore.handleException(ex)
        //     End Try
        //     Return returnValue
        // End Function
        // 
        // ============================================================================================
        // 
        //    Content Replacements
        // 
        //    A list of commands that create, modify and return strings
        //    the start and end with escape sequences contentReplaceEscapeStart/contentReplaceEscapeEnd
        //        {{ and }} previously
        //        {% and %} right now
        // 
        //    format:
        //        {% commands %}
        // 
        //     commands
        //        a single command or a JSON array of commands.
        //        if a command has arguments, the command should be a JSON object
        //            openLayout layoutName
        // 
        //        one command, no arguments -- non JSON
        //                {% user %}
        //        one command, one argument -- non JSON
        //                {% user "firstname" %}
        // 
        //        one command, no arguments -- JSON command array of one
        //                {% [ "user" ] %}
        //                cmdList[0] = "user"
        // 
        //        two commands, no arguments -- JSON command array
        //                {% [
        //                        "user",
        //                        "user"
        //                    ] %}
        //                cmdList[0] = "user"
        //                cmdList[1] = "user"
        // 
        //        one command, one argument -- JSON object for command
        //                {% [
        //                        {
        //                            "cmd": "layout",
        //                            "arg": "mylayout"
        //                        }
        //                    ] %}
        //                cmdList[0].cmd = layout
        //                cmdList[0].arg = "mylayout"
        // 
        //        one command, two arguments
        //                {% [
        //                        {
        //                            "cmd": "set",
        //                            "arg": {
        //                                "find":"$fpo$",
        //                                "replace":"Some Content"
        //                        }
        //                    ] %}
        //                cmdList[0].cmd = "replace"
        //                cmdList[0].arg.find = "$fpo$"
        //                cmdList[0].arg.replace = "Some Content"
        // 
        //        two commands, two arguments
        //                {% [
        //                        {
        //                            "cmd": "import",
        //                            "arg": "myTemplate.html"
        //                        },
        //                        {
        //                            "cmd": "setInner",
        //                            "arg": {
        //                                "find":".contentBoxClass",
        //                                "replace":"{% addon contentBox %}"
        //                        }
        //                    ] %}
        //                cmdList[0].cmd = "import"
        //                cmdList[0].arg = "myTemplate.html"
        //                cmdList[1].cmd = "setInner"
        //                cmdList[0].arg.find = ".contntBoxClass"
        //                cmdList[0].arg.replace = "{% addon contentBox %}"
        // 
        //            import htmlFile
        //            importVirtual htmlFile
        //            open textFile
        //            openVirtual webfilename
        //            addon contentbox( JSON-Object-optionstring-list )
        //            set find replace
        //            setInner findLocation replace
        //            setOuter findLocation replace
        //            user firstname
        //            site propertyname
        // 
        public string ExecuteCmd(string src, Contensive.BaseClasses.CPUtilsBaseClass.addonContext Context, int personalizationPeopleId, bool personalizationIsAuthenticated) {
            string returnValue = "";
            try {
                bool badCmd;
                bool notFound;
                int posOpen;
                int posClose;
                string Cmd;
                string cmdResult;
                int posDq;
                int posSq;
                int Ptr;
                int ptrLast;
                string dst;
                string escape;
                // 
                dst = "";
                ptrLast = 1;
                for (
                ; (Ptr > 1); 
                ) {
                    Cmd = "";
                    posOpen = genericController.vbInstr(ptrLast, src, contentReplaceEscapeStart);
                    Ptr = posOpen;
                    if ((Ptr == 0)) {
                        // 
                        //  not found, copy the rest of src to dst
                        // 
                    }
                    else {
                        // 
                        //  scan until we have passed all double and single quotes that are before the next
                        // 
                        notFound = true;
                        for (
                        ; notFound; 
                        ) {
                            posClose = genericController.vbInstr(Ptr, src, contentReplaceEscapeEnd);
                            if ((posClose == 0)) {
                                // 
                                //  brace opened but no close, forget the open and exit
                                // 
                                posOpen = 0;
                                notFound = false;
                            }
                            else {
                                posDq = Ptr;
                                for (
                                ; (escape == "\\"); 
                                ) {
                                    posDq = genericController.vbInstr((posDq + 1), src, "\"");
                                    escape = "";
                                    if ((posDq > 0)) {
                                        escape = src.Substring(posDq, 1);
                                    }
                                    
                                }
                                
                                posSq = Ptr;
                                for (
                                ; (escape == "\\"); 
                                ) {
                                    posSq = genericController.vbInstr((posSq + 1), src, "\'");
                                    escape = "";
                                    if ((posSq > 0)) {
                                        escape = src.Substring(posSq, 1);
                                    }
                                    
                                }
                                
                                switch (GetFirstNonZeroInteger(posSq, posDq)) {
                                    case 0:
                                        // 
                                        //  both 0, posClose is OK as-is
                                        // 
                                        notFound = false;
                                        break;
                                    case 1:
                                        // 
                                        //  posSq is before posDq
                                        // 
                                        if ((posSq > posClose)) {
                                            notFound = false;
                                        }
                                        else {
                                            // 
                                            //  skip forward to the next non-escaped sq
                                            // 
                                            for (
                                            ; (escape == "\\"); 
                                            ) {
                                                posSq = genericController.vbInstr((posSq + 1), src, "\'");
                                                escape = "";
                                                if ((posSq > 0)) {
                                                    escape = src.Substring(posSq, 1);
                                                }
                                                
                                            }
                                            
                                            Ptr = (posSq + 1);
                                            // notFound = False
                                        }
                                        
                                        break;
                                        notFound = false;
                                        break;
                                    default:
                                        // 
                                        //  skip forward to the next non-escaped dq
                                        // 
                                        for (
                                        ; (escape == "\\"); 
                                        ) {
                                            posDq = genericController.vbInstr((posDq + 1), src, "\"");
                                            escape = "";
                                            if ((posDq > 0)) {
                                                escape = src.Substring(posDq, 1);
                                            }
                                            
                                        }
                                        
                                        Ptr = (posDq + 1);
                                        // notFound = False
                                        break;
                                }
                            }
                            
                        }
                        
                    }
                    
                    if ((posOpen <= 0)) {
                        // 
                        //  no cmd found, add from the last ptr to the end
                        // 
                        dst = (dst + src.Substring((ptrLast - 1)));
                        Ptr = -1;
                    }
                    else {
                        // 
                        //  cmd found, process it and add the results to the dst
                        // 
                        Cmd = src.Substring((posOpen + 1), (posClose 
                                        - (posOpen - 2)));
                        cmdResult = this.ExecuteAllCmdLists_Execute(Cmd, badCmd, Context, personalizationPeopleId, personalizationIsAuthenticated);
                        if (badCmd) {
                            // 
                            //  the command was bad, put it back in place (?) in case it was not a command
                            // 
                            cmdResult = (contentReplaceEscapeStart 
                                        + (Cmd + contentReplaceEscapeEnd));
                        }
                        
                        dst = (dst 
                                    + (src.Substring((ptrLast - 1), (posOpen - ptrLast)) + cmdResult));
                        Ptr = (posClose + 2);
                    }
                    
                    ptrLast = Ptr;
                }
                
                returnValue = dst;
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnValue;
        }
        
        // 
        // =================================================================================================================
        //    EncodeActiveContent - Execute Content Command Source
        //  refactor -- go through all the parsing sections and setup specific exceptions to help users get the syntax correct
        // =================================================================================================================
        // 
        private string ExecuteAllCmdLists_Execute(string cmdSrc, bool return_BadCmd, Contensive.BaseClasses.CPUtilsBaseClass.addonContext Context, int personalizationPeopleId, bool personalizationIsAuthenticated) {
            string returnValue = "";
            try {
                // 
                //  accumulator gets the result of each cmd, then is passed to the next command to filter
                // 
                string CmdAccumulator;
                Controllers.htmlController htmlDoc;
                string importHead;
                string ArgName;
                string ArgInstanceId;
                string ArgGuid;
                string ArgOptionString = "";
                string cmdText = "";
                string cmdArg;
                int Pos;
                string addonName;
                bool CSPeopleSet = false;
                object cmd;
                object cmdDictionaryOrCollection;
                Dictionary<string, object> cmdDictionary = new Dictionary<string, object>();
                Collection cmdCollection;
                Dictionary<string, object> cmdDef;
                Dictionary<string, object> cmdArgDef = new Dictionary<string, object>();
                string leftChr;
                string rightChr;
                int trimLen;
                string whiteChrs;
                bool trimming;
                // 
                htmlDoc = new Controllers.htmlController(cpCore);
                // 
                cmdSrc = cmdSrc.Trim();
                whiteChrs = ("\r" + ("\n" + ('\t' + " ")));
                for (
                ; trimming; 
                ) {
                    trimming = false;
                    trimLen = cmdSrc.Length;
                    if ((trimLen > 0)) {
                        leftChr = cmdSrc.Substring(0, 1);
                        rightChr = cmdSrc.Substring((cmdSrc.Length - 1));
                        if ((genericController.vbInstr(1, whiteChrs, leftChr) != 0)) {
                            cmdSrc = cmdSrc.Substring(1);
                            trimming = true;
                        }
                        
                        if ((genericController.vbInstr(1, whiteChrs, rightChr) != 0)) {
                            cmdSrc = cmdSrc.Substring(0, (cmdSrc.Length - 1));
                            trimming = true;
                        }
                        
                    }
                    
                }
                
                CmdAccumulator = "";
                if ((cmdSrc != "")) {
                    // 
                    //  convert cmdSrc to cmdCollection
                    //    cmdCollection is a collection of
                    //        1) dictionary objects
                    //        2) strings
                    // 
                    //  the cmdSrc can be one of three things:
                    //    - [a,b,c,d] a JSON array - parseJSON returns a collection
                    //        - leave as collection
                    //    - {a:b,c:d} a JSON object - parseJSON returns a dictionary
                    //        - convert to a collection of each dictionaries
                    //    - a "b" - do not use the parseJSON
                    //        - just make a collection
                    // 
                    Dictionary<string, object> dictionaryKeys;
                    KeyCollection;
                    string Key;
                    object itemObject;
                    object itemVariant;
                    Dictionary<string, object> cmdObject;
                    cmdCollection = new Collection();
                    if (((cmdSrc.Substring(0, 1) == "{") 
                                && (cmdSrc.Substring((cmdSrc.Length - 1)) == "}"))) {
                        // 
                        //  JSON is a single command in the form of an object, like:
                        //    { "import": "test.html" }
                        // 
                        try {
                            cmdDictionary = cpCore.json.Deserialize(Of, Dictionary(Of, String, Object))[cmdSrc];
                        }
                        catch (Exception ex) {
                            cpCore.handleException(ex, ("Error parsing JSON command list [" 
                                            + (GetErrString() + "]")));
                        }
                        
                        // 
                        dictionaryKeys = cmdDictionary.Keys;
                        foreach (Key in dictionaryKeys) {
                            if (!(cmdDictionary.Item[Key] == null)) {
                                cmdObject = new Dictionary<string, object>();
                                itemObject = cmdDictionary.Item[Key];
                                cmdObject.Add(Key, itemObject);
                                cmdCollection.Add(cmdObject);
                            }
                            else {
                                cmdObject = new Dictionary<string, object>();
                                itemVariant = cmdDictionary.Item[Key];
                                cmdObject.Add(Key, itemVariant);
                                cmdCollection.Add(cmdObject);
                            }
                            
                        }
                        
                    }
                    else if (((cmdSrc.Substring(0, 1) == "[") 
                                && (cmdSrc.Substring((cmdSrc.Length - 1)) == "]"))) {
                        // 
                        //  JSON is a command list in the form of an array, like:
                        //    [ "clear" , { "import": "test.html" },{ "open" : "myfile.txt" }]
                        // 
                        cmdCollection = cpCore.json.Deserialize(Of, Collection)[cmdSrc];
                        // If True Then
                        // End If
                        // If (LCase(TypeName(cmdDictionaryOrCollection)) <> "collection") Then
                        //     Throw New ApplicationException("Error parsing JSON command list, expected a command list but parser did not return list, command list [" & cmdSrc & "]")
                        //     Exit Function
                        // Else
                        //     '
                        //     ' assign command array
                        //     '
                        //     cmdCollection = cmdDictionaryOrCollection
                        // End If
                    }
                    else {
                        // 
                        //  a single text command without JSON wrapper, like
                        //    open myfile.html
                        //    open "myfile.html"
                        //    "open" "myfile.html"
                        //    "content box"
                        //    all other posibilities are syntax errors
                        // 
                        cmdText = cmdSrc.Trim();
                        cmdArg = "";
                        if ((cmdText.Substring(0, 1) == "\"")) {
                            // 
                            // cmd is quoted
                            //    "open"
                            //    "Open" file
                            //    "Open" "file"
                            // 
                            Pos = genericController.vbInstr(2, cmdText, "\"");
                            if ((Pos <= 1)) {
                                throw new ApplicationException(("Error parsing content command [" 
                                                + (cmdSrc + ("], expected a close quote around position " + Pos))));
                            }
                            else if ((Pos == cmdText.Length)) {
                                // 
                                //  cmd like "open"
                                // 
                                cmdArg = "";
                                cmdText = cmdText.Substring(1, (Pos - 2));
                            }
                            else if ((cmdText.Substring(Pos, 1) != " ")) {
                                // 
                                //  syntax error, must be a space between cmd and argument
                                // 
                                throw new ApplicationException(("Error parsing content command [" 
                                                + (cmdSrc + ("], expected a space between command and argument around position " + Pos))));
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                            }
                            else {
                                cmdArg = cmdText.Substring(Pos).Trim();
                                cmdText = cmdText.Substring(1, (Pos - 2));
                            }
                            
                        }
                        else {
                            // 
                            //  no quotes, can be
                            //    open
                            //    open file
                            // 
                            Pos = genericController.vbInstr(1, cmdText, " ");
                            if ((Pos > 0)) {
                                cmdArg = cmdSrc.Substring(Pos);
                                cmdText = cmdSrc.Substring(0, (Pos - 1)).Trim();
                            }
                            
                        }
                        
                        if ((cmdArg.Substring(0, 1) == "\"")) {
                            // 
                            // cmdarg is quoted
                            // 
                            Pos = genericController.vbInstr(2, cmdArg, "\"");
                            if ((Pos <= 1)) {
                                throw new ApplicationException(("Error parsing JSON command list, expected a quoted command argument, command list [" 
                                                + (cmdSrc + "]")));
                            }
                            else {
                                cmdArg = cmdArg.Substring(1, (Pos - 2));
                            }
                            
                        }
                        
                        if (((cmdArg.Substring(0, 1) == "{") 
                                    && (cmdArg.Substring((cmdArg.Length - 1)) == "}"))) {
                            // 
                            //  argument is in the form of an object, like:
                            //    { "text name": "my text" }
                            // 
                            cmdDictionaryOrCollection = cpCore.json.Deserialize(Of, Object)[cmdArg];
                            string cmdDictionaryOrCollectionTypeName = TypeName(cmdDictionaryOrCollection).ToLower();
                            if (((cmdDictionaryOrCollectionTypeName != "dictionary") 
                                        && (cmdDictionaryOrCollectionTypeName != "dictionary(of string,object)"))) {
                                throw new ApplicationException(("Error parsing JSON command argument list, expected a single command, command list [" 
                                                + (cmdSrc + "]")));
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                            }
                            else {
                                // 
                                //  create command array of one command
                                // 
                                cmdCollection.Add(cmdDictionaryOrCollection);
                            }
                            
                            cmdDef = new Dictionary<string, object>();
                            cmdDef.Add(cmdText, cmdDictionaryOrCollection);
                            cmdCollection = new Collection();
                            cmdCollection.Add(cmdDef);
                        }
                        else {
                            // 
                            //  command and arguments are strings
                            // 
                            cmdDef = new Dictionary<string, object>();
                            cmdDef.Add(cmdText, cmdArg);
                            cmdCollection = new Collection();
                            cmdCollection.Add(cmdDef);
                        }
                        
                    }
                    
                    // 
                    //  execute the commands in the JSON cmdCollection
                    // 
                    // Dim cmdVariant As Variant
                    foreach (cmd in cmdCollection) {
                        // 
                        //  repeat for all commands in the collection:
                        //  convert each command in the command array to a cmd string, and a cmdArgDef dictionary
                        //  each cmdStringOrDictionary is a command. It may be:
                        //    A - "command"
                        //    B - { "command" }
                        //    C - { "command" : "single-default-argument" }
                        //    D - { "command" : { "name" : "The Name"} }
                        //    E - { "command" : { "name" : "The Name" , "secondArgument" : "secondValue" } }
                        // 
                        string cmdTypeName = TypeName(cmd).ToLower();
                        if ((cmdTypeName == "string")) {
                            // 
                            //  case A & B, the cmdDef is a string
                            // 
                            cmd;
                            String;
                            cmdArgDef = new Dictionary<string, object>();
                        }
                        else if (((cmdTypeName == "dictionary") 
                                    || (cmdTypeName == "dictionary(of string,object)"))) {
                            // 
                            //  cases C-E, (0).key=cmd, (0).value = argument (might be string or object)
                            // 
                            cmd;
                            Dictionary(Of, String, Object);
                            if ((cmdDef.Count != 1)) {
                                // 
                                //  syntax error
                                // 
                            }
                            else {
                                string cmdDefKey = cmdDef.Keys[0];
                                string cmdDefValueTypeName = TypeName(cmdDef.Item[cmdDefKey]).ToLower();
                                // 
                                //  command is the key for these cases
                                // 
                                cmdText = cmdDefKey;
                                if ((cmdDefValueTypeName == "string")) {
                                    // 
                                    //  command definition with default argument
                                    // 
                                    cmdArgDef = new Dictionary<string, object>();
                                    cmdArgDef.Add("default", cmdDef.Item[cmdDefKey]);
                                }
                                else if (((cmdDefValueTypeName == "dictionary") 
                                            || (cmdDefValueTypeName == "dictionary(of string,object)"))) {
                                    cmdDef.Item[cmdDefKey];
                                    Dictionary(Of, String, Object);
                                }
                                else {
                                    // 
                                    //  syntax error, bad command
                                    // 
                                    throw new ApplicationException(("Error parsing JSON command list, , command list [" 
                                                    + (cmdSrc + "]")));
                                    Err.Clear();
                                    // TODO: Exit Function: Warning!!! Need to return the value
                                    return;
                                }
                                
                            }
                            
                        }
                        else {
                            // 
                            //  syntax error
                            // 
                            throw new ApplicationException(("Error parsing JSON command list, , command list [" 
                                            + (cmdSrc + "]")));
                            Err.Clear();
                            // TODO: Exit Function: Warning!!! Need to return the value
                            return;
                        }
                        
                        // 
                        //  execute the cmd with cmdArgDef dictionary
                        // 
                        switch (genericController.vbLCase(cmdText)) {
                            case "textbox":
                                CmdAccumulator = "";
                                ArgName = "";
                                foreach (KeyValuePair<string, object> kvp in cmdArgDef) {
                                    switch (kvp.Key.ToLower()) {
                                        case "name":
                                        case "default":
                                            kvp.Value;
                                            String;
                                            break;
                                    }
                                }
                                
                                if ((ArgName != "")) {
                                    CmdAccumulator = cpCore.html.html_GetContentCopy(ArgName, "copy content", cpCore.doc.authContext.user.id, true, cpCore.doc.authContext.isAuthenticated);
                                }
                                
                                break;
                            case "opencopy":
                                CmdAccumulator = "";
                                ArgName = "";
                                foreach (KeyValuePair<string, object> kvp in cmdArgDef) {
                                    switch (kvp.Key.ToLower()) {
                                        case "name":
                                        case "default":
                                            kvp.Value;
                                            String;
                                            break;
                                    }
                                }
                                
                                if ((ArgName != "")) {
                                    CmdAccumulator = cpCore.html.html_GetContentCopy(ArgName, "copy content", cpCore.doc.authContext.user.id, true, cpCore.doc.authContext.isAuthenticated);
                                }
                                
                                break;
                            case "openlayout":
                                CmdAccumulator = "";
                                ArgName = "";
                                foreach (KeyValuePair<string, object> kvp in cmdArgDef) {
                                    switch (kvp.Key.ToLower()) {
                                        case "name":
                                        case "default":
                                            kvp.Value;
                                            String;
                                            break;
                                    }
                                }
                                
                                if ((ArgName != "")) {
                                    // CmdAccumulator = cpCore.main_GetContentCopy(ArgName, "copy content")
                                    DataTable dt = cpCore.db.executeQuery(("select layout from ccLayouts where name=" + cpCore.db.encodeSQLText(ArgName)));
                                    if (!(dt == null)) {
                                        CmdAccumulator = genericController.encodeText(dt.Rows[0].Item["layout"]);
                                    }
                                    
                                    dt.Dispose();
                                }
                                
                                break;
                            case "open":
                                CmdAccumulator = "";
                                ArgName = "";
                                foreach (KeyValuePair<string, object> kvp in cmdArgDef) {
                                    switch (kvp.Key.ToLower()) {
                                        case "name":
                                        case "default":
                                            kvp.Value;
                                            String;
                                            break;
                                    }
                                }
                                
                                if ((ArgName != "")) {
                                    CmdAccumulator = cpCore.appRootFiles.readFile(ArgName);
                                }
                                
                                break;
                            case "import":
                                throw new NotImplementedException("import contentCmd");
                                break;
                            case "user":
                                throw new NotImplementedException("user contentCmd");
                                break;
                            case "site":
                                throw new NotImplementedException("site contentCmd");
                                // '
                                // ' returns a site property
                                // '
                                // ' arguments
                                // '   name: the site property name
                                // ' default argument
                                // '   name
                                // '
                                // CmdAccumulator = ""
                                // ArgName = ""
                                // For Ptr = 0 To cmdArgDef.Count - 1
                                //     Select Case genericController.vbLCase(cmdArgDef.Keys(Ptr))
                                //         Case "name", "default"
                                //             ArgName = cmdArgDef.Item(Ptr)
                                //     End Select
                                // Next
                                // If ArgName <> "" Then
                                //     CmdAccumulator = cpCore.app.siteProperty_getText(ArgName, "")
                                // End If
                                break;
                            case "set":
                                throw new NotImplementedException("set contentCmd");
                                // '
                                // ' does a find and replace
                                // '
                                // ' arguments
                                // '   find: what to search for in teh accumulator
                                // '   replace: what to replace it with
                                // ' default argument
                                // '   find
                                // '
                                // 'CmdAccumulator = ""
                                // ArgName = ""
                                // For Ptr = 0 To cmdArgDef.Count - 1
                                //     Select Case genericController.vbLCase(cmdArgDef.Keys(Ptr))
                                //         Case "find"
                                //             argFind = cmdArgDef.Item(Ptr)
                                //         Case "replace"
                                //             argReplace = cmdArgDef.Item(Ptr)
                                //     End Select
                                // Next
                                // If argFind <> "" Then
                                //     CmdAccumulator = genericController.vbReplace(CmdAccumulator, argFind, argReplace, vbTextCompare)
                                // End If
                                break;
                            case "setinner":
                                throw new NotImplementedException("setInner contentCmd");
                                // '
                                // ' does a find and replace on the inner HTML of an element identified by its class selector
                                // '
                                // ' arguments
                                // '   find: what to search for in teh accumulator
                                // '   replace: what to replace it with
                                // ' default argument
                                // '   find
                                // '
                                // ArgName = ""
                                // For Ptr = 0 To cmdArgDef.Count - 1
                                //     Select Case genericController.vbLCase(cmdArgDef.Keys(Ptr))
                                //         Case "find"
                                //             argFind = cmdArgDef.Item(Ptr)
                                //         Case "replace"
                                //             argReplace = cmdArgDef.Item(Ptr)
                                //     End Select
                                // Next
                                // If argFind <> "" Then
                                //     CmdAccumulator = htmlTools.insertInnerHTML(Nothing, CmdAccumulator, argFind, argReplace)
                                // End If
                                break;
                            case "getinner":
                                throw new NotImplementedException("getInner contentCmd");
                                // '
                                // ' returns the inner HTML of an element identified by its class selector
                                // '
                                // ' arguments
                                // '   find: what to search for in teh accumulator
                                // ' default argument
                                // '   find
                                // '
                                // ArgName = ""
                                // For Ptr = 0 To cmdArgDef.Count - 1
                                //     Select Case genericController.vbLCase(cmdArgDef.Keys(Ptr))
                                //         Case "find"
                                //             argFind = cmdArgDef.Item(Ptr)
                                //         Case "replace"
                                //             argReplace = cmdArgDef.Item(Ptr)
                                //     End Select
                                // Next
                                // If argFind <> "" Then
                                //     CmdAccumulator = htmlTools.getInnerHTML(Nothing, CmdAccumulator, argFind)
                                // End If
                                break;
                            case "setouter":
                                throw new NotImplementedException("setOuter contentCmd");
                                // '
                                // ' does a find and replace on the outer HTML of an element identified by its class selector
                                // '
                                // ' arguments
                                // '   find: what to search for in teh accumulator
                                // '   replace: what to replace it with
                                // ' default argument
                                // '   find
                                // '
                                // ArgName = ""
                                // For Ptr = 0 To cmdArgDef.Count - 1
                                //     Select Case genericController.vbLCase(cmdArgDef.Keys(Ptr))
                                //         Case "find"
                                //             argFind = cmdArgDef.Item(Ptr)
                                //         Case "replace"
                                //             argReplace = cmdArgDef.Item(Ptr)
                                //     End Select
                                // Next
                                // If argFind <> "" Then
                                //     CmdAccumulator = htmlTools.insertOuterHTML(Nothing, CmdAccumulator, argFind, argReplace)
                                // End If
                                break;
                            case "getouter":
                                throw new NotImplementedException("getouter contentCmd");
                                // '
                                // ' returns the outer HTML of an element identified by its class selector
                                // '
                                // ' arguments
                                // '   find: what to search for in teh accumulator
                                // ' default argument
                                // '   find
                                // '
                                // ArgName = ""
                                // For Ptr = 0 To cmdArgDef.Count - 1
                                //     Select Case genericController.vbLCase(cmdArgDef.Keys(Ptr))
                                //         Case "find"
                                //             argFind = cmdArgDef.Item(Ptr)
                                //         Case "replace"
                                //             argReplace = cmdArgDef.Item(Ptr)
                                //     End Select
                                // Next
                                // If argFind <> "" Then
                                //     CmdAccumulator = htmlTools.getOuterHTML(Nothing, CmdAccumulator, argFind)
                                // End If
                                break;
                            case "runaddon":
                            case "executeaddon":
                            case "addon":
                                addonName = "";
                                Dictionary<string, string> addonArgDict = new Dictionary<string, string>();
                                foreach (KeyValuePair<string, object> kvp in cmdArgDef) {
                                    switch (kvp.Key.ToLower()) {
                                        case "addon":
                                            addonName = kvp.Value.ToString();
                                            // Case "instanceid"
                                            //     ArgInstanceId = kvp.Value.ToString()
                                            // Case "guid"
                                            //     ArgGuid = kvp.Value.ToString()
                                            break;
                                        default:
                                            addonArgDict.Add(kvp.Key, kvp.Value.ToString());
                                            // ArgOptionString &= "&" & encodeNvaArgument(genericController.encodeText(kvp.Key.ToString())) & "=" & encodeNvaArgument(genericController.encodeText(kvp.Value.ToString()))
                                            break;
                                    }
                                }
                                
                                addonArgDict.Add("cmdAccumulator", CmdAccumulator);
                                // ArgOptionString &= "&cmdAccumulator=" & encodeNvaArgument(CmdAccumulator)
                                // ArgOptionString = Mid(ArgOptionString, 2)
                                CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext();
                                // With...
                                addonType = Context;
                                cssContainerClass = "";
                                cssContainerId = "";
                                hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext();
                                // With...
                                contentName = "";
                                fieldName = "";
                                recordId = 0;
                                personalizationAuthenticated = personalizationIsAuthenticated;
                                personalizationPeopleId = personalizationPeopleId;
                                instanceArguments = addonArgDict;
                                Models.Entity.addonModel addon = Models.Entity.addonModel.createByName(cpCore, addonName);
                                if ((addon == null)) {
                                    cpCore.handleException(new ApplicationException(("Add-on [" 
                                                        + (addonName + ("] could not be found executing command in content [" 
                                                        + (cmdSrc + "]"))))));
                                }
                                else {
                                    CmdAccumulator = cpCore.addon.execute(addon, executeContext);
                                }
                                
                                // 
                                //  execute an add-on
                                // 
                                addonName = cmdText;
                                // ArgInstanceId = ""
                                // ArgGuid = ""
                                Dictionary<string, string> addonArgDict = new Dictionary<string, string>();
                                foreach (KeyValuePair<string, object> kvp in cmdArgDef) {
                                    switch (kvp.Key.ToLower()) {
                                        default:
                                            addonArgDict.Add(kvp.Key, kvp.Value.ToString());
                                            // ArgOptionString &= "&" & encodeNvaArgument(genericController.encodeText(kvp.Key.ToString())) & "=" & encodeNvaArgument(genericController.encodeText(kvp.Value.ToString()))
                                            break;
                                    }
                                }
                                
                                addonArgDict.Add("cmdAccumulator", CmdAccumulator);
                                CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext();
                                // With...
                                addonType = Context;
                                cssContainerClass = "";
                                cssContainerId = "";
                                hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext();
                                // With...
                                contentName = "";
                                fieldName = "";
                                recordId = 0;
                                personalizationAuthenticated = personalizationIsAuthenticated;
                                personalizationPeopleId = personalizationPeopleId;
                                instanceArguments = addonArgDict;
                                Models.Entity.addonModel addon = Models.Entity.addonModel.createByName(cpCore, addonName);
                                CmdAccumulator = cpCore.addon.execute(addon, executeContext);
                                // '
                                // ' attempts to execute an add-on with the command name
                                // '
                                // addonName = cmdText
                                // ArgInstanceId = ""
                                // ArgGuid = ""
                                // For Each kvp As KeyValuePair(Of String, Object) In cmdArgDef
                                //     Select Case kvp.Key.ToLower()
                                //         Case "instanceid"
                                //             ArgInstanceId = kvp.Value.ToString()
                                //         Case "guid"
                                //             ArgGuid = kvp.Value.ToString()
                                //         Case Else
                                //             ArgOptionString &= "&" & encodeNvaArgument(genericController.encodeText(kvp.Key)) & "=" & encodeNvaArgument(genericController.encodeText(kvp.Value.ToString()))
                                //     End Select
                                // Next
                                // ArgOptionString = ArgOptionString & "&cmdAccumulator=" & encodeNvaArgument(CmdAccumulator)
                                // ArgOptionString = Mid(ArgOptionString, 2)
                                // Dim executeContext As New CPUtilsBaseClass.addonExecuteContext() With {
                                //     .addonType = Context,
                                //     .cssContainerClass = "",
                                //     .cssContainerId = "",
                                //     .hostRecord = New CPUtilsBaseClass.addonExecuteHostRecordContext() With {
                                //         .contentName = "",
                                //         .fieldName = "",
                                //         .recordId = 0
                                //     },
                                //     .personalizationAuthenticated = personalizationIsAuthenticated,
                                //     .personalizationPeopleId = personalizationPeopleId,
                                //     .instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, ArgOptionString)
                                // }
                                // Dim addon As Models.Entity.addonModel = Models.Entity.addonModel.createByName(cpCore, addonName)
                                // CmdAccumulator = cpCore.addon.execute(addon, executeContext)
                                // 'CmdAccumulator = cpCore.addon.execute_legacy6(0, addonName, ArgOptionString, Context, "", 0, "", "", False, 0, "", False, Nothing, "", Nothing, "", personalizationPeopleId, personalizationIsAuthenticated)
                                // 'CmdAccumulator = mainOrNothing.ExecuteAddon3(addonName, ArgOptionString, Context)
                                // 
                                returnValue = CmdAccumulator;
                                ((Exception)(ex));
                                cpCore.handleException(ex);
                                throw;
                                return returnValue;
                                break;
                        }
                    }
                    
                }
                
            }
            
        }
    }
}