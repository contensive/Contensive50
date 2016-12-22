
Option Explicit On
Option Strict On


Imports Contensive.BaseClasses
Imports Contensive.Core.coreCommonModule

'
' findReplace as integer to as integer
' just the document -- replace out 
' if 'Imports Interop.adodb, replace in ObjectStateEnum.adState...
' findreplace encode to encode
' findreplace ''DoEvents to '''DoEvents
' runProcess becomes runProcess
' Sleep becomes Threading.Thread.Sleep(
' as object to as object

Namespace Contensive.Core
    Public Class coreContentCmdClass
        '
        Private cpCore As cpCoreClass
        '
        '====================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As cpCoreClass)
            Me.cpCore = cpCore
        End Sub
        ''
        ''=================================================================================
        'Public Function execute(CsvObject As Object, mainObject As Object, optionString As String, filterInput As String) As String
        '    Dim returnValue As String = ""
        '    Try
        '        Dim src As String
        '        Dim Context As Integer
        '        Dim personalizationPeopleId As Integer
        '        Dim personalizationIsAuthenticated As Boolean
        '        '
        '        src = CsvObject.GetAddonOption("data", optionString)
        '        Context = encodeInteger(CsvObject.GetAddonOption("context", optionString))
        '        personalizationPeopleId = EncodeInteger(CsvObject.GetAddonOption("personalizationPeopleId", optionString))
        '        personalizationIsAuthenticated = EncodeBoolean(CsvObject.GetAddonOption("personalizationIsAuthenticated", optionString))
        '        '
        '        ' compatibility with old Contensive
        '        '
        '        If (personalizationPeopleId = 0) And (Not (mainObject Is Nothing)) Then
        '            personalizationPeopleId = mainObject.MemberID
        '            personalizationIsAuthenticated = mainObject.isAuthenticated
        '        End If
        '        If src <> "" Then
        '            '
        '            ' test for Contensive processign instruction
        '            '
        '            execute = ExecuteCmd(src, Context, personalizationPeopleId, personalizationIsAuthenticated)
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleException(ex)
        '    End Try
        '    Return returnValue
        'End Function
        '
        '============================================================================================
        '
        '   Content Replacements
        '
        '   A list of commands that create, modify and return strings
        '   the start and end with escape sequences contentReplaceEscapeStart/contentReplaceEscapeEnd
        '       {{ and }} previously
        '       {% and %} right now
        '
        '   format:
        '       {% commands %}
        '
        '    commands
        '       a single command or a JSON array of commands.
        '       if a command has arguments, the command should be a JSON object
        '           openLayout layoutName
        '
        '       one command, no arguments -- non JSON
        '               {% user %}
        '       one command, one argument -- non JSON
        '               {% user "firstname" %}
        '
        '       one command, no arguments -- JSON command array of one
        '               {% [ "user" ] %}
        '               cmdList[0] = "user"
        '
        '       two commands, no arguments -- JSON command array
        '               {% [
        '                       "user",
        '                       "user"
        '                   ] %}
        '               cmdList[0] = "user"
        '               cmdList[1] = "user"
        '
        '       one command, one argument -- JSON object for command
        '               {% [
        '                       {
        '                           "cmd": "layout",
        '                           "arg": "mylayout"
        '                       }
        '                   ] %}
        '               cmdList[0].cmd = layout
        '               cmdList[0].arg = "mylayout"
        '
        '       one command, two arguments
        '               {% [
        '                       {
        '                           "cmd": "set",
        '                           "arg": {
        '                               "find":"$fpo$",
        '                               "replace":"Some Content"
        '                       }
        '                   ] %}
        '               cmdList[0].cmd = "replace"
        '               cmdList[0].arg.find = "$fpo$"
        '               cmdList[0].arg.replace = "Some Content"
        '
        '       two commands, two arguments
        '               {% [
        '                       {
        '                           "cmd": "import",
        '                           "arg": "myTemplate.html"
        '                       },
        '                       {
        '                           "cmd": "setInner",
        '                           "arg": {
        '                               "find":".contentBoxClass",
        '                               "replace":"{% addon contentBox %}"
        '                       }
        '                   ] %}
        '               cmdList[0].cmd = "import"
        '               cmdList[0].arg = "myTemplate.html"
        '               cmdList[1].cmd = "setInner"
        '               cmdList[0].arg.find = ".contntBoxClass"
        '               cmdList[0].arg.replace = "{% addon contentBox %}"
        '
        '           import htmlFile
        '           importVirtual htmlFile
        '           open textFile
        '           openVirtual webfilename
        '           addon contentbox( JSON-Object-optionstring-list )
        '           set find replace
        '           setInner findLocation replace
        '           setOuter findLocation replace
        '           user firstname
        '           site propertyname
        '
        Friend Function ExecuteCmd(src As String, Context As cpCoreClass.addonContextEnum, personalizationPeopleId As Integer, personalizationIsAuthenticated As Boolean) As String
            Dim returnValue As String = ""
            Try
                Dim badCmd As Boolean
                Dim notFound As Boolean
                Dim AddonOptionString As String
                Dim pairNames As Object
                Dim i As Integer
                Dim pairName As String
                Dim pairValue As String
                Dim ValueVariant As Object
                Dim posNextStart As Integer
                Dim posArgOpen As Integer
                Dim posArgClose As Integer
                Dim posOpen As Integer
                Dim posClose As Integer
                Dim Cmd As String
                Dim cmdArgs As String
                Dim cmdResult As String
                '
                Dim ACGuid As String
                Dim addonName As String
                Dim ACInstanceID As String
                Dim AddonOptionStringHTMLEncoded As String
                Dim CSPeople As CPCSBaseClass
                Dim CSPeopleSet As Boolean
                Dim cmdArgJSON As Object
                'Dim peopleId as integer
                Dim isJSON As Boolean
                '
                Dim posDq As Integer
                Dim posSq As Integer
                Dim Ptr As Integer
                Dim ptrLast As Integer
                Dim dst As String
                Dim escape As String
                '
                dst = ""
                ptrLast = 1
                Do
                    Cmd = ""
                    posOpen = InStr(ptrLast, src, contentReplaceEscapeStart)
                    Ptr = posOpen
                    If Ptr = 0 Then
                        '
                        ' not found, copy the rest of src to dst
                        '
                    Else
                        '
                        ' scan until we have passed all double and single quotes that are before the next
                        '
                        notFound = True
                        Do
                            posClose = InStr(Ptr, src, contentReplaceEscapeEnd)
                            If posClose = 0 Then
                                '
                                ' brace opened but no close, forget the open and exit
                                '
                                posOpen = 0
                                notFound = False
                            Else
                                posDq = Ptr
                                Do
                                    posDq = InStr(posDq + 1, src, """")
                                    escape = ""
                                    If posDq > 0 Then
                                        escape = Mid(src, posDq - 1, 1)
                                    End If
                                Loop While (escape = "\")
                                posSq = Ptr
                                Do
                                    posSq = InStr(posSq + 1, src, "'")
                                    escape = ""
                                    If posSq > 0 Then
                                        escape = Mid(src, posSq - 1, 1)
                                    End If
                                Loop While (escape = "\")
                                Select Case GetFirstNonZeroInteger(posSq, posDq)
                                    Case 0
                                        '
                                        ' both 0, posClose is OK as-is
                                        '
                                        notFound = False
                                    Case 1
                                        '
                                        ' posSq is before posDq
                                        '
                                        If posSq > posClose Then
                                            notFound = False
                                        Else
                                            '
                                            ' skip forward to the next non-escaped sq
                                            '
                                            Do
                                                posSq = InStr(posSq + 1, src, "'")
                                                escape = ""
                                                If posSq > 0 Then
                                                    escape = Mid(src, posSq - 1, 1)
                                                End If
                                            Loop While (escape = "\")
                                            Ptr = posSq + 1
                                            'notFound = False
                                        End If
                                    Case Else
                                        '
                                        ' posDq is before posSq
                                        '
                                        If posDq > posClose Then
                                            notFound = False
                                        Else
                                            '
                                            ' skip forward to the next non-escaped dq
                                            '
                                            Do
                                                'Ptr = posDq + 1
                                                posDq = InStr(posDq + 1, src, """")
                                                escape = ""
                                                If posDq > 0 Then
                                                    escape = Mid(src, posDq - 1, 1)
                                                End If
                                            Loop While (escape = "\")
                                            Ptr = posDq + 1
                                            'notFound = False
                                        End If
                                End Select
                            End If
                        Loop While notFound
                    End If
                    If posOpen <= 0 Then
                        '
                        ' no cmd found, add from the last ptr to the end
                        '
                        dst = dst & Mid(src, ptrLast)
                        Ptr = -1
                    Else
                        '
                        ' cmd found, process it and add the results to the dst
                        '
                        Cmd = Mid(src, posOpen + 2, (posClose - posOpen - 2))
                        cmdResult = ExecuteAllCmdLists_Execute(Cmd, badCmd, Context, personalizationPeopleId, personalizationIsAuthenticated)
                        If badCmd Then
                            '
                            ' the command was bad, put it back in place (?) in case it was not a command
                            '
                            cmdResult = contentReplaceEscapeStart & Cmd & contentReplaceEscapeEnd
                        End If
                        dst = dst & Mid(src, ptrLast, posOpen - ptrLast) & cmdResult
                        Ptr = posClose + 2
                    End If
                    ptrLast = Ptr
                Loop While (Ptr > 1)
                '
                returnValue = dst
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnValue
        End Function
        '
        '=================================================================================================================
        '   EncodeActiveContent - Execute Content Command Source
        '=================================================================================================================
        '
        Private Function ExecuteAllCmdLists_Execute(cmdSrc As String, return_BadCmd As Boolean, Context As cpCoreClass.addonContextEnum, personalizationPeopleId As Integer, personalizationIsAuthenticated As Boolean) As String
            Dim returnValue As String = ""
            Try
                '
                ' accumulator gets the result of each cmd, then is passed to the next command to filter
                '
                Dim CmdAccumulator As String
                Dim CS As CPCSBaseClass
                Dim htmlTools As coreHtmlToolsClass
                Dim argField As String
                Dim importHead As String
                Dim argFind As String
                Dim argReplace As String
                Dim ArgName As String
                Dim ArgInstanceId As String
                Dim ArgGuid As String
                Dim ArgOptionString As String = ""
                Dim Cmd As String = ""
                Dim cmdArg As String
                Dim Pos As Integer
                Dim addonName As String
                Dim CSPeople As CPCSBaseClass
                Dim CSPeopleSet As Boolean = False
                Dim Ptr As Integer
                Dim cmdStringOrDictionary As Object
                Dim cmdDictionaryOrCollection As Object
                Dim cmdDictionary As New Dictionary(Of String, Object)
                Dim cmdCollection As Collection
                Dim cmdDef As Dictionary(Of String, Object)
                Dim cmdArgDef As New Dictionary(Of String, Object)
                Dim leftChr As String
                Dim rightChr As String
                Dim trimLen As Integer
                Dim whiteChrs As String
                Dim trimming As Boolean
                Dim addonStatusOK As Boolean
                '
                htmlTools = New coreHtmlToolsClass(cpCore)
                '
                cmdSrc = Trim(cmdSrc)
                whiteChrs = vbCr & vbLf & vbTab & " "
                Do
                    trimming = False
                    trimLen = Len(cmdSrc)
                    If trimLen > 0 Then
                        leftChr = Left(cmdSrc, 1)
                        rightChr = Right(cmdSrc, 1)
                        If InStr(1, whiteChrs, leftChr) <> 0 Then
                            cmdSrc = Mid(cmdSrc, 2)
                            trimming = True
                        End If
                        If InStr(1, whiteChrs, rightChr) <> 0 Then
                            cmdSrc = Mid(cmdSrc, 1, Len(cmdSrc) - 1)
                            trimming = True
                        End If
                    End If
                Loop While trimming
                CmdAccumulator = ""
                If cmdSrc <> "" Then
                    '
                    ' convert cmdSrc to cmdCollection
                    '   cmdCollection is a collection of
                    '       1) dictionary objects
                    '       2) strings
                    '
                    ' the cmdSrc can be one of three things:
                    '   - [a,b,c,d] a JSON array - parseJSON returns a collection
                    '       - leave as collection
                    '   - {a:b,c:d} a JSON object - parseJSON returns a dictionary
                    '       - convert to a collection of each dictionaries
                    '   - a "b" - do not use the parseJSON
                    '       - just make a collection
                    '
                    Dim dictionaryKeys As Dictionary(Of String, Object).KeyCollection
                    Dim Key As String
                    Dim itemObject As Object
                    Dim itemVariant As Object
                    Dim cmdObject As Dictionary(Of String, Object)
                    '
                    cmdCollection = New Collection
                    If (Left(cmdSrc, 1) = "{") And (Right(cmdSrc, 1) = "}") Then
                        '
                        ' JSON is a single command in the form of an object, like:
                        '   { "import": "test.html" }
                        '
                        Try
                            cmdDictionary = DirectCast(cpCore.main_parseJSON(cmdSrc), Dictionary(Of String, Object))
                        Catch ex As Exception
                            cpCore.handleException(ex, "Error parsing JSON command list [" & GetErrString() & "]")
                        End Try
                        If True Then
                            'End If
                            'If (LCase(TypeName(cmdDictionaryOrCollection)) <> "dictionary") Then
                            '    Throw New ApplicationException("Error parsing JSON command list, expected a single command, command list [" & cmdSrc & "]")
                            'Else
                            '
                            '   Top leve should be an array of commands (or objects) [a,b,c,d]
                            '       if it is an object {a:b,c:d,}, convert to arry [{a:b},{c:d}]
                            ' convert dictionary of objects to a collection of objects
                            '
                            '
                            dictionaryKeys = cmdDictionary.Keys
                            For Each Key In dictionaryKeys
                                If Not (cmdDictionary.Item(Key) Is Nothing) Then
                                    cmdObject = New Dictionary(Of String, Object)
                                    itemObject = cmdDictionary.Item(Key)
                                    Call cmdObject.Add(Key, itemObject)
                                    Call cmdCollection.Add(cmdObject)
                                Else
                                    cmdObject = New Dictionary(Of String, Object)
                                    itemVariant = cmdDictionary.Item(Key)
                                    Call cmdObject.Add(Key, itemVariant)
                                    Call cmdCollection.Add(cmdObject)
                                End If
                            Next
                            '                If (cmdDictionary.Count = 1) Then
                            '                    Set cmdCollection = New Collection
                            '                    Call cmdCollection.Add(cmdDictionary.Item(0), cmdDictionary.key(0))
                            '                Else
                            '                    For Each cmdString In cmdDictionary
                            '                        cmdCollection.Add (cmdString)
                            '                    Next
                            '                End If
                        End If
                    ElseIf (Left(cmdSrc, 1) = "[") And (Right(cmdSrc, 1) = "]") Then
                        '
                        ' JSON is a command list in the form of an array, like:
                        '   [ "clear" , { "import": "test.html" },{ "open" : "myfile.txt" }]
                        '
                        cmdCollection = DirectCast(cpCore.main_parseJSON(cmdSrc), Collection)
                        'If True Then
                        'End If
                        'If (LCase(TypeName(cmdDictionaryOrCollection)) <> "collection") Then
                        '    Throw New ApplicationException("Error parsing JSON command list, expected a command list but parser did not return list, command list [" & cmdSrc & "]")
                        '    Exit Function
                        'Else
                        '    '
                        '    ' assign command array
                        '    '
                        '    cmdCollection = cmdDictionaryOrCollection
                        'End If
                    Else
                        '
                        ' a single text command without JSON wrapper, like
                        '   open myfile.html
                        '   open "myfile.html"
                        '   "open" "myfile.html"
                        '   "content box"
                        '   all other posibilities are syntax errors
                        '
                        Cmd = Trim(cmdSrc)
                        cmdArg = ""
                        If Mid(Cmd, 1, 1) = """" Then
                            '
                            'cmd is quoted
                            '   "open"
                            '   "Open" file
                            '   "Open" "file"
                            '
                            Pos = InStr(2, Cmd, """")
                            If Pos <= 1 Then
                                Throw New ApplicationException("Error parsing content command [" & cmdSrc & "], expected a close quote around position " & Pos)
                            Else
                                If Pos = Len(Cmd) Then
                                    '
                                    ' cmd like "open"
                                    '
                                    cmdArg = ""
                                    Cmd = Mid(Cmd, 2, Pos - 2)
                                ElseIf Mid(Cmd, Pos + 1, 1) <> " " Then
                                    '
                                    ' syntax error, must be a space between cmd and argument
                                    '
                                    Throw New ApplicationException("Error parsing content command [" & cmdSrc & "], expected a space between command and argument around position " & Pos)
                                    Exit Function
                                Else
                                    cmdArg = Trim(Mid(Cmd, Pos + 1))
                                    Cmd = Mid(Cmd, 2, Pos - 2)
                                End If
                            End If

                        Else
                            '
                            ' no quotes, can be
                            '   open
                            '   open file
                            '
                            Pos = InStr(1, Cmd, " ")
                            If Pos > 0 Then
                                cmdArg = Mid(cmdSrc, Pos + 1)
                                Cmd = Trim(Mid(cmdSrc, 1, Pos - 1))
                            End If
                        End If
                        If Mid(cmdArg, 1, 1) = """" Then
                            '
                            'cmdarg is quoted
                            '
                            Pos = InStr(2, cmdArg, """")
                            If Pos <= 1 Then
                                Throw New ApplicationException("Error parsing JSON command list, expected a quoted command argument, command list [" & cmdSrc & "]")
                            Else
                                cmdArg = Mid(cmdArg, 2, Pos - 2)
                            End If
                        End If
                        If (Left(cmdArg, 1) = "{") And (Right(cmdArg, 1) = "}") Then
                            '
                            ' argument is in the form of an object, like:
                            '   { "text name": "my text" }
                            '
                            cmdDictionaryOrCollection = cpCore.main_parseJSON(cmdArg)
                            If (LCase(TypeName(cmdDictionaryOrCollection)) <> "dictionary") Then
                                Throw New ApplicationException("Error parsing JSON command argument list, expected a single command, command list [" & cmdSrc & "]")
                                Exit Function
                            Else
                                '
                                ' create command array of one command
                                '
                                Call cmdCollection.Add(cmdDictionaryOrCollection)
                            End If
                            cmdDef = New Dictionary(Of String, Object)
                            Call cmdDef.Add(Cmd, cmdDictionaryOrCollection)
                            cmdCollection = New Collection
                            Call cmdCollection.Add(cmdDef)
                        Else
                            '
                            ' command and arguments are strings
                            '
                            cmdDef = New Dictionary(Of String, Object)
                            Call cmdDef.Add(Cmd, cmdArg)
                            cmdCollection = New Collection
                            Call cmdCollection.Add(cmdDef)
                        End If
                    End If
                    '
                    ' execute the commands in the JSON cmdCollection
                    '
                    'Dim cmdVariant As Variant

                    For Each cmdStringOrDictionary In cmdCollection
                        '
                        ' repeat for all commands in the collection:
                        ' convert each command in the command array to a cmd string, and a cmdArgDef dictionary
                        ' each cmdStringOrDictionary is a command. It may be:
                        '   A - "command"
                        '   B - { "command" }
                        '   C - { "command" : "single-default-argument" }
                        '   D - { "command" : { "name" : "The Name"} }
                        '   E - { "command" : { "name" : "The Name" , "secondArgument" : "secondValue" } }
                        '
                        If LCase(TypeName(cmdStringOrDictionary)) = "string" Then
                            '
                            ' case A & B, the cmdDef is a string
                            '
                            Cmd = DirectCast(cmdStringOrDictionary, String)
                            cmdArgDef = New Dictionary(Of String, Object)
                        ElseIf LCase(TypeName(cmdStringOrDictionary)) = "dictionary" Then
                            '
                            ' cases C-E, (0).key=cmd, (0).value = argument (might be string or object)
                            '
                            cmdDef = DirectCast(cmdStringOrDictionary, Dictionary(Of String, Object))
                            If cmdDef.Count <> 1 Then
                                '
                                ' syntax error
                                '
                            Else
                                Cmd = cmdDef.Keys(0)
                                If LCase(TypeName(cmdDef.Item(Cmd))) = "string" Then
                                    '
                                    ' command definition with default argument
                                    '
                                    cmdArgDef = New Dictionary(Of String, Object)
                                    Call cmdArgDef.Add("default", cmdDef.Item(Cmd))
                                ElseIf LCase(TypeName(cmdDef.Item(Cmd))) = "dictionary" Then
                                    cmdArgDef = DirectCast(cmdDef.Item(Cmd), Dictionary(Of String, Object))
                                Else
                                    '
                                    ' syntax error, bad command
                                    '
                                    Throw New ApplicationException("Error parsing JSON command list, , command list [" & cmdSrc & "]")
                                    Err.Clear()
                                    Exit Function
                                End If
                            End If
                        Else
                            '
                            ' syntax error
                            '
                            Throw New ApplicationException("Error parsing JSON command list, , command list [" & cmdSrc & "]")
                            Err.Clear()
                            Exit Function
                        End If
                        '
                        ' execute the cmd with cmdArgDef dictionary
                        '
                        Select Case LCase(Cmd)
                            Case "textbox"
                                '
                                ' Opens a textbox addon (patch for text box name being "text name" so it requies json)copy content record
                                '
                                ' arguments
                                '   name: copy content record
                                ' default
                                '   name
                                '
                                CmdAccumulator = ""
                                ArgName = ""
                                For Each kvp As KeyValuePair(Of String, Object) In cmdArgDef
                                    Select Case kvp.Key.ToLower()
                                        Case "name", "default"
                                            ArgName = DirectCast(kvp.Value, String)
                                    End Select
                                Next
                                If ArgName <> "" Then
                                    CmdAccumulator = cpCore.main_GetContentCopy(ArgName, "copy content")
                                End If
                            Case "opencopy"
                                '
                                ' Opens a copy content record
                                '
                                ' arguments
                                '   name: layout record name
                                ' default
                                '   name
                                '
                                CmdAccumulator = ""
                                ArgName = ""
                                For Each kvp As KeyValuePair(Of String, Object) In cmdArgDef
                                    Select Case kvp.Key.ToLower()
                                        Case "name", "default"
                                            ArgName = DirectCast(kvp.Value, String)
                                    End Select
                                Next
                                If ArgName <> "" Then
                                    CmdAccumulator = cpCore.main_GetContentCopy(ArgName, "copy content")
                                    'Dim dt As DataTable = cpCore.app.executeSql("select copy from ccCopyContent where name=" & EncodeSQLText(ArgName))
                                    'If Not (dt Is Nothing) Then
                                    '    CmdAccumulator = EncodeText(dt.Rows(0).Item("copy"))
                                    'End If
                                    'dt.Dispose()
                                End If
                            Case "openlayout"
                                '
                                ' Opens a layout record
                                '
                                ' arguments
                                '   name: layout record name
                                ' default
                                '   name
                                '
                                CmdAccumulator = ""
                                ArgName = ""
                                For Each kvp As KeyValuePair(Of String, Object) In cmdArgDef
                                    Select Case kvp.Key.ToLower()
                                        Case "name", "default"
                                            ArgName = DirectCast(kvp.Value, String)
                                    End Select
                                Next
                                If ArgName <> "" Then
                                    'CmdAccumulator = cpCore.main_GetContentCopy(ArgName, "copy content")
                                    Dim dt As DataTable = cpCore.app.executeSql("select layout from ccLayouts where name=" & cpCore.app.db_EncodeSQLText(ArgName))
                                    If Not (dt Is Nothing) Then
                                        CmdAccumulator = EncodeText(dt.Rows(0).Item("layout"))
                                    End If
                                    dt.Dispose()
                                End If
                            Case "open"
                                '
                                ' Opens a file in the wwwPath
                                '
                                ' arguments
                                '   name: filename
                                ' default
                                '   name
                                '
                                CmdAccumulator = ""
                                ArgName = ""
                                For Each kvp As KeyValuePair(Of String, Object) In cmdArgDef
                                    Select Case kvp.Key.ToLower()
                                        Case "name", "default"
                                            ArgName = DirectCast(kvp.Value, String)
                                    End Select
                                Next
                                If ArgName <> "" Then
                                    CmdAccumulator = cpCore.app.appRootFiles.ReadFile(ArgName)
                                End If
                            Case "import"
                                '
                                ' Opens an html file in the wwwPath and imports the head and returns the body
                                '
                                ' arguments
                                '   name: filename
                                ' default argument
                                '   name
                                '
                                CmdAccumulator = ""
                                ArgName = ""
                                For Each kvp As KeyValuePair(Of String, Object) In cmdArgDef
                                    Select Case kvp.Key.ToLower()
                                        Case "name", "default"
                                            ArgName = DirectCast(kvp.Value, String)
                                    End Select
                                Next
                                If ArgName <> "" Then
                                    CmdAccumulator = cpCore.app.appRootFiles.ReadFile(ArgName)
                                    If CmdAccumulator <> "" Then
                                        importHead = GetTagInnerHTML(CmdAccumulator, "head", False)
                                        If importHead <> "" Then
                                            ' try this, but it may not be implemented yet
                                            Call cpCore.csv_addHeadTags(importHead)
                                        End If
                                        CmdAccumulator = GetTagInnerHTML(CmdAccumulator, "body", False)
                                    End If
                                End If
                            Case "user"
                                Throw New NotImplementedException("user contentCmd")
                                '
                                ' returns the value of the current users field
                                '
                                ' arguments
                                '   field: fieldName
                                ' default
                                '   field
                                '
                                'CmdAccumulator = ""
                                'argField = ""
                                'For Each kvp As KeyValuePair(Of String, Object) In cmdArgDef
                                '    Select Case kvp.Key.ToLower()
                                '        Case "field", "default"
                                '            argField = DirectCast(kvp.Value, String)
                                '    End Select
                                'Next
                                'If argField = "" Then
                                '    argField = "name"
                                'End If
                                'If Not CSPeopleSet Then
                                '    CSPeople.Open("People", "id=" & personalizationPeopleId)
                                '    CSPeopleSet = True
                                'End If
                                'If CSPeople.OK() And CSPeople.FieldOK(argField) Then
                                '    CmdAccumulator = CSPeople.GetText(argField)
                                'End If
                            Case "site"
                                Throw New NotImplementedException("site contentCmd")
                                ''
                                '' returns a site property
                                ''
                                '' arguments
                                ''   name: the site property name
                                '' default argument
                                ''   name
                                ''
                                'CmdAccumulator = ""
                                'ArgName = ""
                                'For Ptr = 0 To cmdArgDef.Count - 1
                                '    Select Case LCase(cmdArgDef.Keys(Ptr))
                                '        Case "name", "default"
                                '            ArgName = cmdArgDef.Item(Ptr)
                                '    End Select
                                'Next
                                'If ArgName <> "" Then
                                '    CmdAccumulator = cpCore.app.siteProperty_getText(ArgName, "")
                                'End If
                            Case "set"
                                Throw New NotImplementedException("set contentCmd")
                                ''
                                '' does a find and replace
                                ''
                                '' arguments
                                ''   find: what to search for in teh accumulator
                                ''   replace: what to replace it with
                                '' default argument
                                ''   find
                                ''
                                ''CmdAccumulator = ""
                                'ArgName = ""
                                'For Ptr = 0 To cmdArgDef.Count - 1
                                '    Select Case LCase(cmdArgDef.Keys(Ptr))
                                '        Case "find"
                                '            argFind = cmdArgDef.Item(Ptr)
                                '        Case "replace"
                                '            argReplace = cmdArgDef.Item(Ptr)
                                '    End Select
                                'Next
                                'If argFind <> "" Then
                                '    CmdAccumulator = Replace(CmdAccumulator, argFind, argReplace, , , vbTextCompare)
                                'End If
                            Case "setinner"
                                Throw New NotImplementedException("setInner contentCmd")
                                ''
                                '' does a find and replace on the inner HTML of an element identified by its class selector
                                ''
                                '' arguments
                                ''   find: what to search for in teh accumulator
                                ''   replace: what to replace it with
                                '' default argument
                                ''   find
                                ''
                                'ArgName = ""
                                'For Ptr = 0 To cmdArgDef.Count - 1
                                '    Select Case LCase(cmdArgDef.Keys(Ptr))
                                '        Case "find"
                                '            argFind = cmdArgDef.Item(Ptr)
                                '        Case "replace"
                                '            argReplace = cmdArgDef.Item(Ptr)
                                '    End Select
                                'Next
                                'If argFind <> "" Then
                                '    CmdAccumulator = htmlTools.insertInnerHTML(Nothing, CmdAccumulator, argFind, argReplace)
                                'End If
                            Case "getinner"
                                Throw New NotImplementedException("getInner contentCmd")
                                ''
                                '' returns the inner HTML of an element identified by its class selector
                                ''
                                '' arguments
                                ''   find: what to search for in teh accumulator
                                '' default argument
                                ''   find
                                ''
                                'ArgName = ""
                                'For Ptr = 0 To cmdArgDef.Count - 1
                                '    Select Case LCase(cmdArgDef.Keys(Ptr))
                                '        Case "find"
                                '            argFind = cmdArgDef.Item(Ptr)
                                '        Case "replace"
                                '            argReplace = cmdArgDef.Item(Ptr)
                                '    End Select
                                'Next
                                'If argFind <> "" Then
                                '    CmdAccumulator = htmlTools.getInnerHTML(Nothing, CmdAccumulator, argFind)
                                'End If
                            Case "setouter"
                                Throw New NotImplementedException("setOuter contentCmd")
                                ''
                                '' does a find and replace on the outer HTML of an element identified by its class selector
                                ''
                                '' arguments
                                ''   find: what to search for in teh accumulator
                                ''   replace: what to replace it with
                                '' default argument
                                ''   find
                                ''
                                'ArgName = ""
                                'For Ptr = 0 To cmdArgDef.Count - 1
                                '    Select Case LCase(cmdArgDef.Keys(Ptr))
                                '        Case "find"
                                '            argFind = cmdArgDef.Item(Ptr)
                                '        Case "replace"
                                '            argReplace = cmdArgDef.Item(Ptr)
                                '    End Select
                                'Next
                                'If argFind <> "" Then
                                '    CmdAccumulator = htmlTools.insertOuterHTML(Nothing, CmdAccumulator, argFind, argReplace)
                                'End If
                            Case "getouter"
                                Throw New NotImplementedException("getouter contentCmd")
                                ''
                                '' returns the outer HTML of an element identified by its class selector
                                ''
                                '' arguments
                                ''   find: what to search for in teh accumulator
                                '' default argument
                                ''   find
                                ''
                                'ArgName = ""
                                'For Ptr = 0 To cmdArgDef.Count - 1
                                '    Select Case LCase(cmdArgDef.Keys(Ptr))
                                '        Case "find"
                                '            argFind = cmdArgDef.Item(Ptr)
                                '        Case "replace"
                                '            argReplace = cmdArgDef.Item(Ptr)
                                '    End Select
                                'Next
                                'If argFind <> "" Then
                                '    CmdAccumulator = htmlTools.getOuterHTML(Nothing, CmdAccumulator, argFind)
                                'End If
                            Case "runaddon", "executeaddon", "addon"
                                '
                                ' execute an add-on
                                '
                                addonName = ""
                                ArgInstanceId = ""
                                ArgGuid = ""
                                For Each kvp As KeyValuePair(Of String, Object) In cmdArgDef
                                    Select Case kvp.Key.ToLower()
                                        Case "addon"
                                            addonName = kvp.Value.ToString()
                                        Case "instanceid"
                                            ArgInstanceId = kvp.Value.ToString()
                                        Case "guid"
                                            ArgGuid = kvp.Value.ToString()
                                        Case Else
                                            ArgOptionString &= "&" & cpCore.csv_EncodeAddonOptionArgument(EncodeText(kvp.Key.ToString())) & "=" & cpCore.csv_EncodeAddonOptionArgument(EncodeText(kvp.Value.ToString()))
                                    End Select
                                Next
                                ArgOptionString &= "&cmdAccumulator=" & cpCore.csv_EncodeAddonOptionArgument(CmdAccumulator)
                                ArgOptionString = Mid(ArgOptionString, 2)
                                CmdAccumulator = cpCore.executeAddon(0, addonName, ArgOptionString, Context, "", 0, "", "", False, 0, "", addonStatusOK, Nothing, "", Nothing, "", personalizationPeopleId, personalizationIsAuthenticated)
                            Case Else
                                '
                                ' attempts to execute an add-on with the command name
                                '
                                addonName = Cmd
                                ArgInstanceId = ""
                                ArgGuid = ""
                                For Each kvp As KeyValuePair(Of String, Object) In cmdArgDef
                                    Select Case kvp.Key.ToLower()
                                        Case "instanceid"
                                            ArgInstanceId = kvp.Value.ToString()
                                        Case "guid"
                                            ArgGuid = kvp.Value.ToString()
                                        Case Else
                                            ArgOptionString &= "&" & cpCore.csv_EncodeAddonOptionArgument(EncodeText(kvp.Key)) & "=" & cpCore.csv_EncodeAddonOptionArgument(EncodeText(kvp.Value.ToString()))
                                    End Select
                                Next
                                ArgOptionString = ArgOptionString & "&cmdAccumulator=" & cpCore.csv_EncodeAddonOptionArgument(CmdAccumulator)
                                ArgOptionString = Mid(ArgOptionString, 2)
                                CmdAccumulator = cpCore.executeAddon(0, addonName, ArgOptionString, Context, "", 0, "", "", False, 0, "", addonStatusOK, Nothing, "", Nothing, "", personalizationPeopleId, personalizationIsAuthenticated)
                                'CmdAccumulator = mainOrNothing.ExecuteAddon3(addonName, ArgOptionString, Context)
                        End Select
                    Next
                End If
                '
                ExecuteAllCmdLists_Execute = CmdAccumulator
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnValue
        End Function
    End Class
End Namespace

