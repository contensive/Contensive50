
Option Explicit On
Option Strict On


Imports Contensive.BaseClasses
Imports Contensive.Core.coreCommonModule
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

'
' findReplace as integer to as integer
' just the document -- replace out 
' if 'Imports Interop.adodb, replace in ObjectStateEnum.adState...
' findreplace encode to encode
' findreplace ''DoEvents to '''DoEvents
' runProcess becomes runProcess
' Sleep becomes Threading.Thread.Sleep(
' as object to as object

Namespace Contensive.Core.Controllers
    Public Class htmlDocController
        '
        Private cpCore As coreClass
        '
        Public html_quickEdit_copy As String = ""
        '
        ' local cache for loading editor addon json list
        '
        Public html_Private_FieldEditorList As String = ""
        Public html_Private_FieldEditorList_Loaded As Boolean = False
        '
        Public html_ServerDomainCrossList As String = ""
        Public html_ServerDomainCrossList_Loaded As Boolean = False
        '
        ' Public view bubble editors
        '
        Public html_HelpViewerButtonID As String = ""
        Public html_EditWrapperCnt As Integer = 0
        '
        Public html_DocBodyFilter As String = ""
        '
        '
        Public html_LegacySiteStyles_Loaded As Boolean = False
        '
        Private menu_MenuSystemCloseCount As Integer = 0
        '
        ' -- Help Subsystem
        Friend htmlDoc_HelpCodeCount As Integer = 0
        Friend htmlDoc_HelpCodeSize As Integer = 0
        Friend htmlDoc_HelpCodes As String()
        Friend htmlDoc_HelpCaptions As String()
        Friend htmlDoc_HelpDialogCnt As Integer = 0
        '
        Public htmlForEndOfBody As String = ""             ' Anything that needs to be written to the Page during main_GetClosePage
        '
        Public pageManager_printVersion As Boolean = False
        '
        Public Const htmlDoc_JavaStreamChunk = 100
        Public Const htmlDoc_OutStreamStandard = 0
        Public Const htmlDoc_OutStreamJavaScript = 1
        '
        Public htmlDoc_RefreshQueryString As String = ""      ' the querystring required to return to the current state (perform a refresh)
        Public htmlDoc_RedirectContentID As Integer = 0
        Public htmlDoc_RedirectRecordID As Integer = 0
        Public htmlDoc_JavaStreamHolder() As String
        Public htmlDoc_JavaStreamSize As Integer = 0
        Public htmlDoc_JavaStreamCount As Integer = 0
        Public htmlDoc_IsStreamWritten As Boolean = False       ' true when anything has been writeAltBuffered.
        '
        Public docBufferEnabled As Boolean = True          ' when true (default), stream is buffered until page is done
        Public docBuffer As String = ""                   ' if any method calls writeAltBuffer, string concatinates here. If this is not empty at exit, it is used instead of returned string
        '
        Public main_MetaContent_Title As String = ""
        Public main_MetaContent_Description As String = ""
        Public main_MetaContent_OtherHeadTags As String = ""
        Public main_MetaContent_KeyWordList As String = ""
        Public main_MetaContent_StyleSheetTags As String = ""
        Public main_MetaContent_TemplateStyleSheetTag As String = ""
        Public main_MetaContent_SharedStyleIDList As String = ""
        '
        '====================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As coreClass)
            Me.cpCore = cpCore
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' setOuter
        ''' </summary>
        ''' <param name="ignore"></param>
        ''' <param name="layout"></param>
        ''' <param name="Key"></param>
        ''' <param name="textToInsert"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function insertOuterHTML(ignore As Object, layout As String, Key As String, textToInsert As String) As String
            Dim returnValue As String = ""
            Try
                Dim posStart As Integer
                Dim posEnd As Integer
                '
                ' short-cut for now, get the outerhtml, find the position, then remove the wrapping tags
                '
                If Key = "" Then
                    returnValue = textToInsert
                Else
                    returnValue = layout
                    posStart = getTagStartPos2(ignore, layout, 1, Key)
                    If posStart <> 0 Then
                        posEnd = getTagEndPos(ignore, layout, posStart)
                        If posEnd > 0 Then
                            '
                            ' seems like these are the correct positions here.
                            '
                            returnValue = Left(layout, posStart - 1) & textToInsert & Mid(layout, posEnd)
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnValue
        End Function
        '
        '====================================================================================================
        Public Function insertInnerHTML(ignore As Object, layout As String, Key As String, textToInsert As String) As String
            Dim returnValue As String = ""
            Try
                Dim posStart As Integer
                Dim posEnd As Integer
                '
                ' short-cut for now, get the outerhtml, find the position, then remove the wrapping tags
                '
                If Key = "" Then
                    returnValue = textToInsert
                Else
                    returnValue = layout
                    posStart = getTagStartPos2(ignore, layout, 1, Key)
                    'outerHTML = getOuterHTML(ignore, layout, Key, PosStart)
                    If posStart <> 0 Then
                        posEnd = getTagEndPos(ignore, layout, posStart)
                        If posEnd > 0 Then
                            posStart = genericController.vbInstr(posStart + 1, layout, ">")
                            If posStart <> 0 Then
                                posStart = posStart + 1
                                posEnd = InStrRev(layout, "<", posEnd - 1)
                                If posEnd <> 0 Then
                                    returnValue = Left(layout, posStart - 1) & textToInsert & Mid(layout, posEnd)
                                End If
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnValue
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' getInnerHTML
        ''' </summary>
        ''' <param name="ignore"></param>
        ''' <param name="layout"></param>
        ''' <param name="Key"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getInnerHTML(ignore As Object, layout As String, Key As String) As String
            Dim returnValue As String = ""
            Try
                Dim posStart As Integer
                Dim posEnd As Integer
                '
                ' short-cut for now, get the outerhtml, find the position, then remove the wrapping tags
                '
                If Key = "" Then
                    '
                    ' inner of nothing is nothing
                    '
                Else
                    returnValue = layout
                    posStart = getTagStartPos2(ignore, layout, 1, Key)
                    If posStart <> 0 Then
                        posEnd = getTagEndPos(ignore, layout, posStart)
                        If posEnd > 0 Then
                            posStart = genericController.vbInstr(posStart + 1, layout, ">")
                            If posStart <> 0 Then
                                posStart = posStart + 1
                                posEnd = InStrRev(layout, "<", posEnd - 1)
                                If posEnd <> 0 Then
                                    '
                                    ' now move the end forward to skip trailing whitespace
                                    '
                                    Do
                                        posEnd = posEnd + 1
                                    Loop While (posEnd < Len(layout)) And (InStr(1, vbTab & vbCr & vbLf & vbTab & " ", Mid(layout, posEnd, 1)) <> 0)
                                    posEnd = posEnd - 1
                                    returnValue = Mid(layout, posStart, (posEnd - posStart))
                                End If
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnValue
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' getOuterHTML
        ''' </summary>
        ''' <param name="ignore"></param>
        ''' <param name="layout"></param>
        ''' <param name="Key"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getOuterHTML(ignore As Object, layout As String, Key As String) As String
            Dim returnValue As String = ""
            Try
                Dim posStart As Integer
                Dim posEnd As Integer
                Dim s As String
                '
                s = layout
                If s <> "" Then
                    posStart = getTagStartPos2(ignore, s, 1, Key)
                    If posStart > 0 Then
                        '
                        ' now backtrack to include the leading whitespace
                        '
                        Do While (posStart > 0) And (InStr(1, vbTab & vbCr & vbLf & vbTab & " ", Mid(s, posStart, 1)) <> 0)
                            posStart = posStart - 1
                        Loop
                        'posStart = posStart + 1
                        s = Mid(s, posStart)
                        posEnd = getTagEndPos(ignore, s, 1)
                        If posEnd > 0 Then
                            s = Left(s, posEnd - 1)
                            returnValue = s
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnValue
        End Function
        '
        '====================================================================================================
        Private Function tagMatch(layout As String, posStartTag As Integer, searchId As String, searchClass As String) As Boolean
            Dim returnValue As Boolean = False
            Try
                Const attrAllowedChars = "abcdefghijklmnopqrstuvwzyz-_"
                Dim Tag As String
                Dim tagLower As String
                Dim Pos As Integer
                Dim Delimiter As String
                Dim testChar As String
                Dim tagLength As Integer
                Dim posValueStart As Integer
                Dim testValue As String
                Dim testValues() As String
                Dim testCnt As Integer
                Dim Ptr As Integer
                '
                returnValue = False
                Pos = genericController.vbInstr(posStartTag, layout, ">")
                If Pos > 0 Then
                    returnValue = True
                    Tag = Mid(layout, posStartTag, Pos - posStartTag + 1)
                    tagLower = genericController.vbLCase(Tag)
                    tagLength = Len(Tag)
                    '
                    ' check searchId
                    '
                    If returnValue And (searchId <> "") Then
                        Pos = genericController.vbInstr(1, tagLower, " id=", vbTextCompare)
                        If Pos <= 0 Then
                            '
                            ' id required but this tag has no id attr
                            '
                            returnValue = False
                        Else
                            '
                            ' test if the id attr value matches the searchClass
                            '
                            Pos = Pos + 4
                            Delimiter = Mid(tagLower, Pos, 1)
                            testValue = ""
                            If (Delimiter = """") Or (Delimiter = "'") Then
                                '
                                ' search for end of delimited attribute value
                                '
                                posValueStart = Pos + 1
                                Do
                                    Pos = Pos + 1
                                    testChar = Mid(tagLower, Pos, 1)
                                Loop While (Pos < tagLength) And (testChar <> Delimiter)
                                If Pos >= tagLength Then
                                    '
                                    ' delimiter not found, html error
                                    '
                                    returnValue = False
                                Else
                                    testValue = Mid(Tag, posValueStart, Pos - posValueStart)
                                End If
                            Else
                                '
                                ' search for end of non-delimited attribute value
                                '
                                posValueStart = Pos
                                Do While (Pos < tagLength) And (isInStr(1, attrAllowedChars, Mid(tagLower, Pos, 1), vbTextCompare))
                                    Pos = Pos + 1
                                Loop
                                If Pos >= tagLength Then
                                    '
                                    ' delimiter not found, html error
                                    '
                                    returnValue = False
                                Else
                                    testValue = Mid(Tag, posValueStart, Pos - posValueStart)
                                End If
                            End If
                            If returnValue And (testValue <> "") Then
                                '
                                '
                                '
                                If searchId <> testValue Then
                                    '
                                    ' there can only be one id, and this does not match
                                    '
                                    returnValue = False
                                End If
                            End If
                        End If
                    End If
                    '
                    ' check searchClass
                    '
                    If returnValue And (searchClass <> "") Then
                        Pos = genericController.vbInstr(1, tagLower, " class=", vbTextCompare)
                        If Pos <= 0 Then
                            '
                            ' class required but this tag has no class attr
                            '
                            returnValue = False
                        Else
                            '
                            ' test if the class attr value matches the searchClass
                            '
                            Pos = Pos + 7
                            Delimiter = Mid(tagLower, Pos, 1)
                            testValue = ""
                            If (Delimiter = """") Or (Delimiter = "'") Then
                                '
                                ' search for end of delimited attribute value
                                '
                                posValueStart = Pos + 1
                                Do
                                    Pos = Pos + 1
                                    testChar = Mid(tagLower, Pos, 1)
                                Loop While (Pos < tagLength) And (testChar <> Delimiter)
                                If Pos >= tagLength Then
                                    '
                                    ' delimiter not found, html error
                                    '
                                    returnValue = False
                                Else
                                    testValue = Mid(Tag, posValueStart, Pos - posValueStart)
                                End If
                            Else
                                '
                                ' search for end of non-delimited attribute value
                                '
                                posValueStart = Pos
                                Do While (Pos < tagLength) And (isInStr(1, attrAllowedChars, Mid(tagLower, Pos, 1), vbTextCompare))
                                    Pos = Pos + 1
                                Loop
                                If Pos >= tagLength Then
                                    '
                                    ' delimiter not found, html error
                                    '
                                    returnValue = False
                                Else
                                    testValue = Mid(Tag, posValueStart, Pos - posValueStart)
                                End If
                            End If
                            If returnValue And (testValue <> "") Then
                                '
                                '
                                '
                                testValues = Split(testValue, " ")
                                testCnt = UBound(testValues) + 1
                                For Ptr = 0 To testCnt - 1
                                    If searchClass = testValues(Ptr) Then
                                        Exit For
                                    End If
                                Next
                                If Ptr >= testCnt Then
                                    returnValue = False
                                End If
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnValue
        End Function
        '
        '====================================================================================================
        Public Function getTagStartPos2(ignore As Object, layout As String, layoutStartPos As Integer, Key As String) As Integer
            Dim returnValue As Integer = 0
            Try
                Dim returnPos As Integer
                Dim SegmentStart As Integer
                Dim Pos As Integer
                Dim LoopPtr As Integer
                Dim searchKey As String
                Dim lenSearchKey As Integer
                Dim Ptr As Integer
                Dim workingKey As String
                Dim workingKeys() As String
                Dim searchClass As String
                Dim searchId As String
                Dim searchTag As String
                Dim posStartTag As Integer
                '
                returnPos = 0
                workingKey = Key
                If genericController.vbInstr(1, workingKey, ">") <> 0 Then
                    '
                    ' does not support > yet.
                    '
                    workingKey = genericController.vbReplace(workingKey, ">", " ")
                End If
                '
                ' eliminate whitespace
                '
                Do While genericController.vbInstr(1, workingKey, vbTab) <> 0
                    workingKey = genericController.vbReplace(workingKey, vbTab, " ")
                Loop
                '
                Do While genericController.vbInstr(1, workingKey, vbCr) <> 0
                    workingKey = genericController.vbReplace(workingKey, vbCr, " ")
                Loop
                '
                Do While genericController.vbInstr(1, workingKey, vbLf) <> 0
                    workingKey = genericController.vbReplace(workingKey, vbLf, " ")
                Loop
                '
                Do While genericController.vbInstr(1, workingKey, "  ") <> 0
                    workingKey = genericController.vbReplace(workingKey, "  ", " ")
                Loop
                '
                workingKey = Trim(workingKey)
                '
                If genericController.vbInstr(1, workingKey, " ") <> 0 Then
                    '
                    ' if there are spaces, do them sequentially
                    '
                    workingKeys = Split(workingKey, " ")
                    SegmentStart = 1
                    Do While (layout <> "") And (SegmentStart <> 0) And (Ptr <= UBound(workingKeys))
                        SegmentStart = getTagStartPos2(Nothing, layout, SegmentStart, workingKeys(Ptr))
                        Ptr = Ptr + 1
                    Loop
                    returnPos = SegmentStart
                Else
                    '
                    ' find this single key and get the outerHTML
                    '   at this point, the key can be
                    '       a class = .xxxx
                    '       an id = #xxxx
                    '       a tag = xxxx
                    '       a compound in either form, xxxx.xxxx or xxxx#xxxx
                    '
                    '   searchKey = the search pattern to start
                    '
                    If Left(workingKey, 1) = "." Then
                        '
                        ' search for a class
                        '
                        searchClass = Mid(workingKey, 2)
                        searchTag = ""
                        searchId = ""
                        Pos = genericController.vbInstr(1, searchClass, "#")
                        If Pos <> 0 Then
                            searchId = Mid(searchClass, Pos)
                            searchClass = Mid(searchClass, 1, Pos - 1)
                        End If
                        '
                        'workingKey = Mid(workingKey, 2)
                        searchKey = "<"
                    ElseIf Left(workingKey, 1) = "#" Then
                        '
                        ' search for an ID
                        '
                        searchClass = ""
                        searchTag = ""
                        searchId = Mid(workingKey, 2)
                        Pos = genericController.vbInstr(1, searchId, ".")
                        If Pos <> 0 Then
                            searchClass = Mid(searchId, Pos)
                            searchId = Mid(searchId, 1, Pos - 1)
                        End If
                        '
                        'workingKey = Mid(workingKey, 2)
                        searchKey = "<"
                    Else
                        '
                        ' search for a tagname
                        '
                        searchClass = ""
                        searchTag = workingKey
                        searchId = ""
                        '
                        Pos = genericController.vbInstr(1, searchTag, "#")
                        If Pos <> 0 Then
                            searchId = Mid(searchTag, Pos + 1)
                            searchTag = Mid(searchTag, 1, Pos - 1)
                            Pos = genericController.vbInstr(1, searchId, ".")
                            If Pos <> 0 Then
                                searchClass = Mid(searchId, Pos)
                                searchId = Mid(searchId, 1, Pos - 1)
                            End If
                        End If
                        Pos = genericController.vbInstr(1, searchTag, ".")
                        If Pos <> 0 Then
                            searchClass = Mid(searchTag, Pos + 1)
                            searchTag = Mid(searchTag, 1, Pos - 1)
                            Pos = genericController.vbInstr(1, searchClass, "#")
                            If Pos <> 0 Then
                                searchId = Mid(searchClass, Pos)
                                searchClass = Mid(searchClass, 1, Pos - 1)
                            End If
                        End If
                        '
                        searchKey = "<" & searchTag
                    End If
                    lenSearchKey = Len(searchKey)
                    Pos = layoutStartPos
                    'posMatch = genericController.vbInstr(layoutStartPos, layout, searchKey)
                    'pos = posMatch
                    'searchIsOver = False
                    Do
                        Pos = genericController.vbInstr(Pos, layout, searchKey)
                        If Pos = 0 Then
                            '
                            ' not found, return empty
                            '
                            's = ""
                            Exit Do
                        Else
                            '
                            ' string found - go to the start of the tag
                            '
                            posStartTag = InStrRev(layout, "<", Pos + 1)
                            If posStartTag <= 0 Then
                                '
                                ' bad html, no start tag found
                                '
                                Pos = 0
                                returnPos = 0
                            ElseIf Mid(layout, posStartTag, 2) = "</" Then
                                '
                                ' this is an end tag, skip it
                                '
                                Pos = Pos + 1
                            ElseIf tagMatch(layout, posStartTag, searchId, searchClass) Then
                                '
                                ' match, return with this position
                                '
                                returnPos = Pos
                                Exit Do
                            Else
                                '
                                ' no match, skip this and go to the next
                                '
                                Pos = Pos + 1
                            End If
                        End If
                        LoopPtr = LoopPtr + 1
                    Loop While LoopPtr < 1000
                    '
                    '
                    '
                    If LoopPtr >= 1000 Then
                        Call Err.Raise(ignoreInteger, "Controllers.htmlToolsController.getTagStartPos2", "Tag limit of 1000 tags per block reached.")
                    End If
                End If
                '
                returnValue = returnPos
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnValue
        End Function
        '
        '====================================================================================================
        Public Function getTagStartPos(ignore As Object, layout As String, layoutStartPos As Integer, Key As String) As Integer
            Dim returnValue As Integer = 0
            Try
                returnValue = getTagStartPos2(ignore, layout, layoutStartPos, Key)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnValue
        End Function
        '
        '=================================================================================================
        '   return the position following the tag which closes the tag that starts the string
        '       starting postion<div><div><p>this and that</p><!-- </div> --></div></div>And a whole lot more
        '       returns the position of the "A" following the last /div
        '       string 123<img>456 returns pointing to "4"
        '       string 123456 returns pointing to "6"
        '       returns 0 if the end was not found
        '=================================================================================================
        '
        Public Function getTagEndPos(ignore As Object, Source As String, startPos As Integer) As Integer
            Dim returnValue As Integer = 0
            Try
                Dim Pos As Integer
                Dim TagName As String
                Dim endTag As String
                Dim startTag As String
                Dim posNest As Integer
                Dim posEnd As Integer
                Dim posComment As Integer
                Dim c As String
                '
                Pos = genericController.vbInstr(startPos, Source, "<")
                TagName = ""
                returnValue = 0
                If Pos <> 0 Then
                    Pos = Pos + 1
                    Do While Pos < Len(Source)
                        c = genericController.vbLCase(Mid(Source, Pos, 1))
                        If (c >= "a") And (c <= "z") Then
                            TagName = TagName & c
                        Else
                            Exit Do
                        End If
                        Pos = Pos + 1
                    Loop
                    If TagName <> "" Then
                        endTag = "</" & TagName
                        startTag = "<" & TagName
                        Do While (Pos <> 0)
                            posEnd = genericController.vbInstr(Pos + 1, Source, endTag, vbTextCompare)
                            If posEnd = 0 Then
                                '
                                ' no end was found, return the tag or rest of the string
                                '
                                returnValue = genericController.vbInstr(Pos + 1, Source, ">") + 1
                                If posEnd = 1 Then
                                    returnValue = Len(Source)
                                End If
                                Exit Do
                            Else
                                posNest = genericController.vbInstr(Pos + 1, Source, startTag, vbTextCompare)
                                If posNest = 0 Then
                                    '
                                    ' no nest found, set to end
                                    '
                                    posNest = Len(Source)
                                End If
                                posComment = genericController.vbInstr(Pos + 1, Source, "<!--")
                                If posComment = 0 Then
                                    '
                                    ' no comment found, set to end
                                    '
                                    posComment = Len(Source)
                                End If
                                If (posNest < posEnd) And (posNest < posComment) Then
                                    '
                                    ' ----- the tag is nested, find the end of the nest
                                    '
                                    Pos = getTagEndPos(ignore, Source, posNest)
                                    ' 8/28/2012, if there is a nested tag right before the correct end tag, it skips the end:
                                    ' <div class=a>a<div class=b>b</div></div>
                                    ' the second /div is missed because returnValue returns one past the >, then the
                                    ' next search starts +1 that position
                                    If (Pos > 0) Then
                                        Pos = Pos - 1
                                    End If
                                ElseIf (posComment < posEnd) Then
                                    '
                                    ' ----- there is a comment between the tag and the first tagend, skip it
                                    '
                                    Pos = genericController.vbInstr(posComment, Source, "-->")
                                    If Pos = 0 Then
                                        '
                                        ' start comment with no end, exit now
                                        '
                                        returnValue = Len(Source)
                                        Exit Do
                                    End If
                                Else
                                    '
                                    ' ----- end position is here, go to the end of it and exit
                                    '
                                    Pos = genericController.vbInstr(posEnd, Source, ">")
                                    If Pos = 0 Then
                                        '
                                        ' no end was found, just exit
                                        '
                                        returnValue = Len(Source)
                                        Exit Do
                                    Else
                                        '
                                        ' ----- end was found
                                        '
                                        returnValue = Pos + 1
                                        Exit Do
                                    End If
                                End If
                            End If
                        Loop
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnValue
        End Function
        '
        '========================================================================
        ' EncodeHTML
        '
        '   Convert all characters that are not allowed in HTML to their Text equivalent
        '   in preperation for use on an HTML page
        '========================================================================
        '
        Public Function html_EncodeHTML(ByVal Source As String) As String
            ' ##### removed to catch err<>0 problem on error resume next
            '
            html_EncodeHTML = Source
            html_EncodeHTML = genericController.vbReplace(html_EncodeHTML, "&", "&amp;")
            html_EncodeHTML = genericController.vbReplace(html_EncodeHTML, "<", "&lt;")
            html_EncodeHTML = genericController.vbReplace(html_EncodeHTML, ">", "&gt;")
            html_EncodeHTML = genericController.vbReplace(html_EncodeHTML, """", "&quot;")
            html_EncodeHTML = genericController.vbReplace(html_EncodeHTML, "'", "&apos;")
            '
        End Function
        '
        '========================================================================================================
        '
        ' Finds all tags matching the input, and concatinates them into the output
        ' does NOT account for nested tags, use for body, script, style
        '
        ' ReturnAll - if true, it returns all the occurances, back-to-back
        '
        '========================================================================================================
        '
        Public Shared Function getTagInnerHTML(ByVal PageSource As String, ByVal Tag As String, ByVal ReturnAll As Boolean) As String
            On Error GoTo ErrorTrap
            '
            Dim TagStart As Integer
            Dim TagEnd As Integer
            Dim LoopCnt As Integer
            Dim WB As String
            Dim Pos As Integer
            Dim PosEnd As Integer
            Dim CommentPos As Integer
            Dim ScriptPos As Integer
            '
            getTagInnerHTML = ""
            Pos = 1
            Do While (Pos > 0) And (LoopCnt < 100)
                TagStart = genericController.vbInstr(Pos, PageSource, "<" & Tag, vbTextCompare)
                If TagStart = 0 Then
                    Pos = 0
                Else
                    '
                    ' tag found, skip any comments that start between current position and the tag
                    '
                    CommentPos = genericController.vbInstr(Pos, PageSource, "<!--")
                    If (CommentPos <> 0) And (CommentPos < TagStart) Then
                        '
                        ' skip comment and start again
                        '
                        Pos = genericController.vbInstr(CommentPos, PageSource, "-->")
                    Else
                        ScriptPos = genericController.vbInstr(Pos, PageSource, "<script")
                        If (ScriptPos <> 0) And (ScriptPos < TagStart) Then
                            '
                            ' skip comment and start again
                            '
                            Pos = genericController.vbInstr(ScriptPos, PageSource, "</script")
                        Else
                            '
                            ' Get the tags innerHTML
                            '
                            TagStart = genericController.vbInstr(TagStart, PageSource, ">", vbTextCompare)
                            Pos = TagStart
                            If TagStart <> 0 Then
                                TagStart = TagStart + 1
                                TagEnd = genericController.vbInstr(TagStart, PageSource, "</" & Tag, vbTextCompare)
                                If TagEnd <> 0 Then
                                    getTagInnerHTML &= Mid(PageSource, TagStart, TagEnd - TagStart)
                                End If
                            End If
                        End If
                    End If
                    LoopCnt = LoopCnt + 1
                    If ReturnAll Then
                        TagStart = genericController.vbInstr(TagEnd, PageSource, "<" & Tag, vbTextCompare)
                    Else
                        TagStart = 0
                    End If
                End If
            Loop
            '
            Exit Function
            '
ErrorTrap:
        End Function

        '
        '========================================================================
        ' ----- Add a new DHTML menu entry
        '========================================================================
        '
        Public Sub menu_AddEntry(ByVal Name As String, Optional ByVal ParentName As String = "", Optional ByVal ImageLink As String = "", Optional ByVal ImageOverLink As String = "", Optional ByVal Link As String = "", Optional ByVal Caption As String = "", Optional ByVal StyleSheet As String = "", Optional ByVal StyleSheetHover As String = "", Optional ByVal NewWindow As Boolean = False)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("AddMenuEntry")
            '
            'If Not (true) Then Exit Sub
            Dim MethodName As String
            Dim Image As String
            Dim ImageOver As String
            '
            MethodName = "AddMenu()"
            '
            Image = genericController.encodeText(ImageLink)
            If Image <> "" Then
                ImageOver = genericController.encodeText(ImageOverLink)
                If Image = ImageOver Then
                    ImageOver = ""
                End If
            End If
            Call cpCore.menuFlyout.AddEntry(genericController.encodeText(Name), ParentName, Image, ImageOver, Link, Caption, , NewWindow)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleLegacyError18(MethodName)
            '
        End Sub
        '
        '========================================================================
        ' ----- main_Get all the menu close scripts
        '
        '   call this at the end of the page
        '========================================================================
        '
        Public Function menu_GetClose() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00214")
            '
            'If Not (true) Then Exit Function
            Dim MethodName As String
            Dim MenuFlyoutIcon As String
            '
            MethodName = "main_GetMenuClose()"
            '
            If Not (cpCore.menuFlyout Is Nothing) Then
                menu_MenuSystemCloseCount = menu_MenuSystemCloseCount + 1
                menu_GetClose = menu_GetClose & cpCore.menuFlyout.GetMenuClose()
                MenuFlyoutIcon = cpCore.siteProperties.getText("MenuFlyoutIcon", "&#187;")
                If MenuFlyoutIcon <> "&#187;" Then
                    menu_GetClose = genericController.vbReplace(menu_GetClose, "&#187;</a>", MenuFlyoutIcon & "</a>")
                End If
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleLegacyError18(MethodName)
            '
        End Function
        '
        '====================================================================================================
        '
        Public Function html_GetEndOfBody(AllowLogin As Boolean, AllowTools As Boolean, BlockNonContentExtras As Boolean, main_IsAdminSite As Boolean) As String
            Dim s As String = ""
            Try
                Dim AllowPopupErrors As Boolean
                Dim AllowPopupTraps As Boolean
                Dim Ptr As Integer
                Dim JSCodeAsString As String
                Dim Copy As String
                Dim JS As String
                Dim autoPrintText As String
                Dim RenderTimeString As String = Format((GetTickCount - cpCore.app_startTickCount) / 1000, "0.000")
                '
                ' ----- Developer debug counters
                '
                cpCore.main_ClosePageCounter = cpCore.main_ClosePageCounter + 1
                If cpCore.webServer.webServerIO_InitCounter = 0 Then
                    cpCore.handleExceptionAndRethrow(New Exception("Page was not initialized properly. Init(...) call may be missing."))
                End If
                If cpCore.webServer.webServerIO_InitCounter > 1 Then
                    cpCore.handleExceptionAndRethrow(New Exception("Page was not initialized properly. Init(...) was called multiple times."))
                End If
                If cpCore.main_ClosePageCounter > 1 Then
                    cpCore.handleExceptionAndRethrow(New Exception("Page was not closed properly. main_GetEndOfBody was called multiple times."))
                End If
                '
                ' ----- add window.print if this is the Printerversion
                '
                If cpCore.htmlDoc.pageManager_printVersion Then
                    autoPrintText = cpCore.docProperties.getText("AutoPrint")
                    If autoPrintText = "" Then
                        autoPrintText = cpCore.siteProperties.getText("AllowAutoPrintDialog", "1")
                    End If
                    If genericController.EncodeBoolean(autoPrintText) Then
                        Call cpCore.main_AddOnLoadJavascript2("window.print(); window.close()", "Print Page")
                    End If
                End If
                '
                ' -- print what is needed
                '
                If (Not BlockNonContentExtras) And (Not cpCore.htmlDoc.pageManager_printVersion) Then
                    If cpCore.user.isAuthenticatedContentManager() And cpCore.user.allowToolsPanel Then
                        If AllowTools Then
                            s = s & cpCore.main_GetToolsPanel()
                        End If
                    Else
                        If AllowLogin Then
                            s = s & cpCore.htmlDoc.main_GetLoginLink()
                        End If
                    End If
                End If
                '
                ' ----- output the menu system
                '
                If Not (cpCore.menuFlyout Is Nothing) Then
                    s = s & cpCore.htmlDoc.menu_GetClose()
                End If
                '
                ' ----- Popup USER errors
                '
                If (Not BlockNonContentExtras) And cpCore.error_IsUserError() Then
                    AllowPopupErrors = cpCore.siteProperties.getBoolean("AllowPopupErrors", True)
                    If AllowPopupErrors Then
                        's = s & main_GetPopupMessage("<div style=""margin:20px;"">" & main_GetUserError() & "</div>", 300, 200, False)
                    End If
                End If
                '
                If Not cpCore.webServer.webServerIO_BlockClosePageCopyright Then
                    s = s & vbCrLf & vbTab & "<!--" & vbCrLf & vbCrLf & vbTab & "Contensive Framework/" & cpCore.codeVersion & ", copyright 1999-2012 Contensive, www.Contensive.com, " & RenderTimeString & vbCrLf & vbCrLf & vbTab & "-->"
                End If
                '
                If Not cpCore.html_BlockClosePageLink Then
                    s = s & vbCrLf & vbTab & "<a href=""http://www.contensive.com""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" border=""0""></a>"
                End If
                '
                ' ----- popup error if this is a developer
                '
                If (Not BlockNonContentExtras) And cpCore.user.isAuthenticatedDeveloper() Then
                    AllowPopupTraps = cpCore.siteProperties.getBoolean("AllowPopupTraps", True)
                    If AllowPopupTraps Then
                        If cpCore.html_PageErrorWithoutCsv Then
                            cpCore.main_TrapLogMessage = "" _
                                    & "<div style=""padding: 20px;"">" _
                                    & genericController.vbReplace(cpCore.main_TrapLogMessage, vbCrLf, "<br>") _
                                    & "</div>"
                            '  s = s & main_GetPopupMessage(main_TrapLogMessage, 600, 400, True, True)
                        ElseIf (cpCore.app_errorCount > 0) Then
                            '  s = s & main_GetPopupMessage(main_TrapLogMessage, "", "", "yes")
                        End If
                    End If
                End If
                '
                ' ----- Include any other close page
                '
                If Not BlockNonContentExtras Then
                    If cpCore.htmlDoc.htmlForEndOfBody <> "" Then
                        s = s & cpCore.htmlDoc.htmlForEndOfBody
                    End If
                    If cpCore.main_testPointMessage <> "" Then
                        s = s & "<div class=""ccTestPointMessageCon"">" & cpCore.main_testPointMessage & "</div>"
                    End If
                End If
                '
                ' ----- Check for javascipt setup, but the appropriate calls are not in this site
                '
                JS = ""
                '
                ' Add Script Code to Head
                '
                If cpCore.main_HeadScriptCnt > 0 Then
                    For Ptr = 0 To cpCore.main_HeadScriptCnt - 1
                        With cpCore.main_HeadScripts(Ptr)
                            If .addedByMessage <> "" Then
                                JS = JS & vbCrLf & "/* from " & .addedByMessage & " */ "
                            End If
                            If Not .IsLink Then
                                JSCodeAsString = .Text
                                JSCodeAsString = genericController.vbReplace(JSCodeAsString, "'", "'+""'""+'")
                                JSCodeAsString = genericController.vbReplace(JSCodeAsString, vbCrLf, "\n")
                                JSCodeAsString = genericController.vbReplace(JSCodeAsString, vbCr, "\n")
                                JSCodeAsString = genericController.vbReplace(JSCodeAsString, vbLf, "\n")
                                JS = JS & vbCrLf & "cj.addHeadScriptCode('" & JSCodeAsString & "');"
                                'JS = JS & vbCrLf & "cjAddHeadScriptCode('" & JSCodeAsString & "');"
                            Else
                                JS = JS & vbCrLf & "cj.addHeadScriptLink('" & .Text & "');"
                                'JS = JS & vbCrLf & "cjAddHeadScriptLink('" & .Text & "');"
                            End If
                        End With
                    Next
                    cpCore.main_HeadScriptCnt = 0
                End If
                '
                ' ----- Add onload javascript
                '
                If (cpCore.main_OnLoadJavascript <> "") Then
                    JS = JS & vbCrLf & vbTab & "cj.addLoadEvent(function(){" & cpCore.main_OnLoadJavascript & "});"
                End If
                '
                ' ----- Add any left over style links
                '
                Dim headTags As String
                headTags = ""
                '
                If (main_MetaContent_StyleSheetTags <> "") Then
                    headTags = headTags & cr & main_MetaContent_StyleSheetTags
                    'JS = JS & vbCrLf & vbTab & "cjAddHeadTag('" & genericController.EncodeJavascript(main_MetaContent_StyleSheetTags) & "');"
                    main_MetaContent_StyleSheetTags = ""
                End If
                '
                ' ----- Add any left over shared styles
                '
                Dim FileList As String
                Dim Files() As String
                Dim Parts() As String
                If (main_MetaContent_SharedStyleIDList <> "") Then
                    FileList = cpCore.main_GetSharedStyleFileList(main_MetaContent_SharedStyleIDList, main_IsAdminSite)
                    main_MetaContent_SharedStyleIDList = ""
                    If FileList <> "" Then
                        Files = Split(FileList, vbCrLf)
                        For Ptr = 0 To UBound(Files)
                            If Files(Ptr) <> "" Then
                                Parts = Split(Files(Ptr) & "<<", "<")
                                If Parts(1) <> "" Then
                                    headTags = headTags & cr & genericController.decodeHtml(Parts(1))
                                End If
                                headTags = headTags & cr & "<link rel=""stylesheet"" type=""text/css"" href=""" & cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, Parts(0)) & """ >"
                                If Parts(2) <> "" Then
                                    headTags = headTags & cr & genericController.decodeHtml(Parts(2))
                                End If
                                'End If
                            End If
                        Next
                    End If
                End If
                '
                ' ----- Add Member Stylesheet if left over
                '
                If cpCore.user.styleFilename <> "" Then
                    Copy = cpCore.cdnFiles.readFile(cpCore.user.styleFilename)
                    headTags = headTags & cr & "<style type=""text/css"">" & Copy & "</style>"
                    'JS = JS & vbCrLf & vbTab & "cjAddHeadTag('<style type=""text/css"">" & Copy & "</style>');"
                    cpCore.user.styleFilename = ""
                End If
                '
                ' ----- Add any left over head tags
                '
                If (main_MetaContent_OtherHeadTags <> "") Then
                    headTags = headTags & cr & main_MetaContent_OtherHeadTags
                    'JS = JS & vbCrLf & vbTab & "cjAddHeadTag('" & genericController.EncodeJavascript(main_MetaContent_OtherHeadTags) & "');"
                    main_MetaContent_OtherHeadTags = ""
                End If
                If (headTags <> "") Then
                    JS = JS & vbCrLf & vbTab & "cj.addHeadTag('" & genericController.EncodeJavascript(headTags) & "');"
                    'JS = JS & vbCrLf & vbTab & "cjAddHeadTag('" & genericController.EncodeJavascript(headTags) & "');"
                End If
                '
                ' ----- Add end of body javascript
                '
                If (cpCore.main_endOfBodyJavascript <> "") Then
                    JS = JS & vbCrLf & cpCore.main_endOfBodyJavascript
                    cpCore.main_endOfBodyJavascript = ""
                End If
                '
                ' ----- If javascript stream, output it all now
                '
                If (cpCore.webServer.webServerIO_OutStreamDevice = htmlDoc_OutStreamJavaScript) Then
                    '
                    ' This is a js output stream from a <script src=url></script>
                    ' process everything into a var=msg;document.write(var)
                    ' any js from the page should be added to this group
                    '
                    Call writeAltBuffer(s)
                    cpCore.webServer.webServerIO_OutStreamDevice = htmlDoc_OutStreamStandard
                    s = webServerIO_JavaStream_Text
                    If JS <> "" Then
                        s = s & vbCrLf & JS
                        JS = ""
                    End If
                Else
                    '
                    ' This is a standard html write
                    ' any javascript collected should go in a <script tag
                    '
                    If JS <> "" Then
                        s = s _
                                & vbCrLf & "<script Language=""javascript"" type=""text/javascript"">" _
                                & vbCrLf & "if(cj){" _
                                & JS _
                                & vbCrLf & "}" _
                                & vbCrLf & "</script>"
                        JS = ""
                    End If
                End If
                '
                ' end-of-body string -- include it without csv because it may have error strings
                '
                If (Not BlockNonContentExtras) And (cpCore.main_endOfBodyString <> "") Then
                    s = s & cpCore.main_endOfBodyString
                End If
            Catch ex As Exception
                Call cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return s
        End Function
        '
        '========================================================================
        ' main_Get a string with a Drop Down Select Box, see PrintFormInputSelect
        '========================================================================
        '
        Public Function main_GetFormInputSelect(ByVal MenuName As String, ByVal CurrentValue As Integer, ByVal ContentName As String, Optional ByVal Criteria As String = "", Optional ByVal NoneCaption As String = "", Optional ByVal htmlId As String = "") As String
            main_GetFormInputSelect = main_GetFormInputSelect2(MenuName, CurrentValue, ContentName, Criteria, NoneCaption, htmlId, False, "")
        End Function
        '
        '
        '
        Public Function main_GetFormInputSelect2(ByVal MenuName As String, ByVal CurrentValue As Integer, ByVal ContentName As String, ByVal Criteria As String, ByVal NoneCaption As String, ByVal htmlId As String, ByRef return_IsEmptyList As Boolean, Optional ByVal HtmlClass As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFormInputSelect2")
            '
            'If Not (true) Then Exit Function
            '
            Const MenuNameFPO = "<MenuName>"
            Const NoneCaptionFPO = "<NoneCaption>"
            '
            Dim CDef As coreMetaDataClass.CDefClass
            'dim dt as datatable
            Dim ContentControlCriteria As String
            Dim LcaseCriteria As String
            Dim CSPointer As Integer
            Dim SelectedFound As Boolean
            Dim RecordID As Integer
            Dim Copy As String
            Dim MethodName As String
            Dim PositionPointer As Integer
            Dim DropDownFieldList As String
            Dim DropDownFieldName() As String
            Dim DropDownDelimiter() As String
            Dim DropDownFieldCount As Integer
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim DropDownPreField As String
            Dim DropDownFieldListLength As Integer
            Dim FieldName As String
            Dim CharAllowed As String
            Dim CharTest As String
            Dim CharPointer As Integer
            Dim AllowedCharacters As String
            'Dim CSContent as integer
            Dim IDFieldPointer As Integer
            Dim FastString As New coreFastStringClass
            '
            Dim RowsArray(,) As String
            Dim RowFieldArray() As String
            Dim RowCnt As Integer
            Dim RowMax As Integer
            Dim ColumnMax As Integer
            Dim RowPointer As Integer
            Dim ColumnPointer As Integer
            Dim DropDownFieldPointer() As Integer
            Dim UcaseFieldName As String
            Dim SortFieldList As String
            Dim SelectListCount As Integer
            Dim SQL As String
            Dim TableName As String
            Dim DataSource As String
            Dim SelectFields As String
            Dim Ptr As Integer
            Dim SelectRaw As String
            Dim CachePtr As Integer
            Dim TagID As String
            Dim CurrentValueText As String
            '
            MethodName = "main_GetFormInputSelect2"
            '
            LcaseCriteria = genericController.vbLCase(Criteria)
            return_IsEmptyList = True
            '
            CurrentValueText = CStr(CurrentValue)
            If cpCore.main_InputSelectCacheCnt > 0 Then
                For CachePtr = 0 To cpCore.main_InputSelectCacheCnt - 1
                    With cpCore.main_InputSelectCache(CachePtr)
                        If (.ContentName = ContentName) And (.Criteria = LcaseCriteria) And (.CurrentValue = CurrentValueText) Then
                            SelectRaw = .SelectRaw
                            return_IsEmptyList = False
                            Exit For
                        End If
                    End With
                Next
            End If
            '
            '
            '
            If SelectRaw = "" Then
                '
                ' Build the SelectRaw
                ' Test selection size
                '
                ' This was commented out -- I really do not know why -- seems like the best way
                '
                CDef = cpCore.metaData.getCdef(ContentName)
                TableName = CDef.ContentTableName
                DataSource = CDef.ContentDataSourceName
                ContentControlCriteria = CDef.ContentControlCriteria
                '
                ' This is what was there
                '
                '        TableName = main_GetContentProperty(ContentName, "ContentTableName")
                '        DataSource = main_GetContentProperty(ContentName, "ContentDataSourceName")
                '        ContentControlCriteria = main_GetContentProperty(ContentName, "ContentControlCriteria")
                '
                SQL = "select count(*) as cnt from " & TableName & " where " & ContentControlCriteria & " AND(editsourceid is null)"
                If LcaseCriteria <> "" Then
                    SQL &= " and " & LcaseCriteria
                End If
                Dim dt As DataTable
                dt = cpCore.db.executeSql(SQL)
                If dt.Rows.Count > 0 Then
                    RowCnt = genericController.EncodeInteger(dt.Rows(0).Item("cnt"))
                End If
                If RowCnt = 0 Then
                    RowMax = -1
                Else
                    return_IsEmptyList = False
                    RowMax = RowCnt - 1
                End If
                '
                If RowCnt > cpCore.siteProperties.selectFieldLimit Then
                    '
                    ' Selection is too big
                    '
                    Call cpCore.error_AddUserError("The drop down list for " & ContentName & " called " & MenuName & " is too long to display. The site administrator has been notified and the problem will be resolved shortly. To fix this issue temporarily, go to the admin tab of the Preferences page and set the Select Field Limit larger than " & RowCnt & ".")
                    '                    cpcore.handleException(New Exception("Legacy error, MethodName=[" & MethodName & "], cause=[" & Cause & "] #" & Err.Number & "," & Err.Source & "," & Err.Description & ""), Cause, 2)

                    cpCore.handleExceptionAndRethrow(New Exception("Error creating select list from content [" & ContentName & "] called [" & MenuName & "]. Selection of [" & RowCnt & "] records exceeds [" & cpCore.siteProperties.selectFieldLimit & "], the current Site Property SelectFieldLimit."))
                    main_GetFormInputSelect2 = main_GetFormInputSelect2 & cpCore.html_GetFormInputHidden(MenuNameFPO, CurrentValue)
                    If CurrentValue = 0 Then
                        main_GetFormInputSelect2 = cpCore.html_GetFormInputText2(MenuName, "0")
                    Else
                        CSPointer = cpCore.csOpen(ContentName, CurrentValue)
                        If cpCore.db.cs_ok(CSPointer) Then
                            main_GetFormInputSelect2 = cpCore.db.cs_getText(CSPointer, "name") & "&nbsp;"
                        End If
                        Call cpCore.db.cs_Close(CSPointer)
                    End If
                    main_GetFormInputSelect2 = main_GetFormInputSelect2 & "(Selection is too large to display option list)"
                Else
                    '
                    ' ----- Generate Drop Down Field Names
                    '
                    DropDownFieldList = CDef.DropDownFieldList
                    'DropDownFieldList = main_GetContentProperty(ContentName, "DropDownFieldList")
                    If DropDownFieldList = "" Then
                        DropDownFieldList = "NAME"
                    End If
                    DropDownFieldCount = 0
                    CharAllowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
                    DropDownFieldListLength = Len(DropDownFieldList)
                    For CharPointer = 1 To DropDownFieldListLength
                        CharTest = Mid(DropDownFieldList, CharPointer, 1)
                        If genericController.vbInstr(1, CharAllowed, CharTest) = 0 Then
                            '
                            ' Character not allowed, delimit Field name here
                            '
                            If (FieldName <> "") Then
                                '
                                ' ----- main_Get new Field Name and save it
                                '
                                If SortFieldList = "" Then
                                    SortFieldList = FieldName
                                End If
                                ReDim Preserve DropDownFieldName(DropDownFieldCount)
                                ReDim Preserve DropDownDelimiter(DropDownFieldCount)
                                DropDownFieldName(DropDownFieldCount) = FieldName
                                DropDownDelimiter(DropDownFieldCount) = CharTest
                                DropDownFieldCount = DropDownFieldCount + 1
                                FieldName = ""
                            Else
                                '
                                ' ----- Save Field Delimiter
                                '
                                If DropDownFieldCount = 0 Then
                                    '
                                    ' ----- Before any field, add to DropDownPreField
                                    '
                                    DropDownPreField = DropDownPreField & CharTest
                                Else
                                    '
                                    ' ----- after a field, add to last DropDownDelimiter
                                    '
                                    DropDownDelimiter(DropDownFieldCount - 1) = DropDownDelimiter(DropDownFieldCount - 1) & CharTest
                                End If
                            End If
                        Else
                            '
                            ' Character Allowed, Put character into fieldname and continue
                            '
                            FieldName = FieldName & CharTest
                        End If
                    Next
                    If FieldName <> "" Then
                        If SortFieldList = "" Then
                            SortFieldList = FieldName
                        End If
                        ReDim Preserve DropDownFieldName(DropDownFieldCount)
                        ReDim Preserve DropDownDelimiter(DropDownFieldCount)
                        DropDownFieldName(DropDownFieldCount) = FieldName
                        DropDownDelimiter(DropDownFieldCount) = ""
                        DropDownFieldCount = DropDownFieldCount + 1
                    End If
                    If DropDownFieldCount = 0 Then
                        cpCore.handleExceptionAndRethrow(New Exception("No drop down field names found for content [" & ContentName & "]."))
                    Else
                        ReDim DropDownFieldPointer(DropDownFieldCount - 1)
                        SelectFields = "ID"
                        For Ptr = 0 To DropDownFieldCount - 1
                            SelectFields = SelectFields & "," & DropDownFieldName(Ptr)
                        Next
                        '
                        ' ----- Start select box
                        '
                        TagID = ""
                        If htmlId <> "" Then
                            TagID = " ID=""" & htmlId & """"
                        End If
                        Call FastString.Add("<select size=""1"" name=""" & MenuNameFPO & """" & TagID & ">")
                        Call FastString.Add("<option value="""">" & NoneCaptionFPO & "</option>")
                        '
                        ' ----- select values
                        '
                        CSPointer = cpCore.db.cs_open(ContentName, Criteria, SortFieldList, , , , , SelectFields)
                        If cpCore.db.cs_ok(CSPointer) Then
                            Call cpCore.debug_testPoint("main_GetFormInputSelect2, 10 ContentName=[" & ContentName & "] Criteria=[" & Criteria & "] ")
                            RowsArray = cpCore.db.cs_getRows(CSPointer)
                            Call cpCore.debug_testPoint("main_GetFormInputSelect2, 20")
                            'RowFieldArray = app.csv_cs_getRowFields(CSPointer)
                            RowFieldArray = Split(cpCore.db.cs_getSelectFieldList(CSPointer), ",")
                            Call cpCore.debug_testPoint("main_GetFormInputSelect2, 30")
                            ColumnMax = UBound(RowsArray, 1)
                            Call cpCore.debug_testPoint("main_GetFormInputSelect2, 40")

                            RowMax = UBound(RowsArray, 2)
                            Call cpCore.debug_testPoint("main_GetFormInputSelect2, 50")
                            '
                            ' setup IDFieldPointer
                            '
                            UcaseFieldName = "ID"
                            For ColumnPointer = 0 To ColumnMax
                                If UcaseFieldName = genericController.vbUCase(RowFieldArray(ColumnPointer)) Then
                                    IDFieldPointer = ColumnPointer
                                    Exit For
                                End If
                            Next
                            '
                            ' setup DropDownFieldPointer()
                            '
                            For FieldPointer = 0 To DropDownFieldCount - 1
                                UcaseFieldName = genericController.vbUCase(DropDownFieldName(FieldPointer))
                                For ColumnPointer = 0 To ColumnMax
                                    If UcaseFieldName = genericController.vbUCase(RowFieldArray(ColumnPointer)) Then
                                        DropDownFieldPointer(FieldPointer) = ColumnPointer
                                        Exit For
                                    End If
                                Next
                            Next
                            '
                            ' output select
                            '
                            SelectedFound = False
                            For RowPointer = 0 To RowMax
                                RecordID = genericController.EncodeInteger(RowsArray(IDFieldPointer, RowPointer))
                                Copy = DropDownPreField
                                For FieldPointer = 0 To DropDownFieldCount - 1
                                    Copy = Copy & RowsArray(DropDownFieldPointer(FieldPointer), RowPointer) & DropDownDelimiter(FieldPointer)
                                Next
                                If Copy = "" Then
                                    Copy = "no name"
                                End If
                                Call FastString.Add(vbCrLf & "<option value=""" & RecordID & """ ")
                                If RecordID = CurrentValue Then
                                    Call FastString.Add("selected")
                                    SelectedFound = True
                                End If
                                If cpCore.siteProperties.selectFieldWidthLimit <> 0 Then
                                    If Len(Copy) > cpCore.siteProperties.selectFieldWidthLimit Then
                                        Copy = Left(Copy, cpCore.siteProperties.selectFieldWidthLimit) & "...+"
                                    End If
                                End If
                                Call FastString.Add(">" & cpCore.htmlDoc.html_EncodeHTML(Copy) & "</option>")
                            Next
                            If Not SelectedFound And (CurrentValue <> 0) Then
                                Call cpCore.db.cs_Close(CSPointer)
                                If Criteria <> "" Then
                                    Criteria = Criteria & "and"
                                End If
                                Criteria = Criteria & "(id=" & genericController.EncodeInteger(CurrentValue) & ")"
                                CSPointer = cpCore.db.cs_open(ContentName, Criteria, SortFieldList, False, , , , SelectFields)
                                If cpCore.db.cs_ok(CSPointer) Then
                                    Call cpCore.debug_testPoint("main_GetFormInputSelect2, 110")
                                    RowsArray = cpCore.db.cs_getRows(CSPointer)
                                    Call cpCore.debug_testPoint("main_GetFormInputSelect2, 120")
                                    RowFieldArray = Split(cpCore.db.cs_getSelectFieldList(CSPointer), ",")
                                    Call cpCore.debug_testPoint("main_GetFormInputSelect2, 130")
                                    RowMax = UBound(RowsArray, 2)
                                    Call cpCore.debug_testPoint("main_GetFormInputSelect2, 140")
                                    ColumnMax = UBound(RowsArray, 1)
                                    Call cpCore.debug_testPoint("main_GetFormInputSelect2, 150")
                                    RecordID = genericController.EncodeInteger(RowsArray(IDFieldPointer, 0))
                                    Call cpCore.debug_testPoint("main_GetFormInputSelect2, 160")
                                    Copy = DropDownPreField
                                    For FieldPointer = 0 To DropDownFieldCount - 1
                                        Copy = Copy & RowsArray(DropDownFieldPointer(FieldPointer), 0) & DropDownDelimiter(FieldPointer)
                                    Next
                                    If Copy = "" Then
                                        Copy = "no name"
                                    End If
                                    Call FastString.Add(vbCrLf & "<option value=""" & RecordID & """ selected")
                                    SelectedFound = True
                                    If cpCore.siteProperties.selectFieldWidthLimit <> 0 Then
                                        If Len(Copy) > cpCore.siteProperties.selectFieldWidthLimit Then
                                            Copy = Left(Copy, cpCore.siteProperties.selectFieldWidthLimit) & "...+"
                                        End If
                                    End If
                                    Call FastString.Add(">" & cpCore.htmlDoc.html_EncodeHTML(Copy) & "</option>")
                                End If
                            End If
                        End If
                        Call FastString.Add("</select>")
                        Call cpCore.db.cs_Close(CSPointer)
                        SelectRaw = FastString.Text
                    End If
                End If
                '
                ' Save the SelectRaw
                '
                If Not return_IsEmptyList Then
                    CachePtr = cpCore.main_InputSelectCacheCnt
                    cpCore.main_InputSelectCacheCnt = cpCore.main_InputSelectCacheCnt + 1
                    ReDim Preserve cpCore.main_InputSelectCache(Ptr)
                    ReDim Preserve cpCore.main_InputSelectCache(CachePtr)
                    cpCore.main_InputSelectCache(CachePtr).ContentName = ContentName
                    cpCore.main_InputSelectCache(CachePtr).Criteria = LcaseCriteria
                    cpCore.main_InputSelectCache(CachePtr).CurrentValue = CurrentValue.ToString
                    cpCore.main_InputSelectCache(CachePtr).SelectRaw = SelectRaw
                End If
            End If
            '
            SelectRaw = genericController.vbReplace(SelectRaw, MenuNameFPO, MenuName)
            SelectRaw = genericController.vbReplace(SelectRaw, NoneCaptionFPO, NoneCaption)
            If HtmlClass <> "" Then
                SelectRaw = genericController.vbReplace(SelectRaw, "<select ", "<select class=""" & HtmlClass & """")
            End If
            main_GetFormInputSelect2 = SelectRaw
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleLegacyError13(MethodName)
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function html_GetFormInputMemberSelect(ByVal MenuName As String, ByVal CurrentValue As Integer, ByVal GroupID As Integer, Optional ByVal ignore As String = "", Optional ByVal NoneCaption As String = "", Optional ByVal htmlId As String = "") As String
            html_GetFormInputMemberSelect = html_GetFormInputMemberSelect2(MenuName, CurrentValue, GroupID, , NoneCaption, htmlId)
        End Function
        '
        Public Function html_GetFormInputMemberSelect2(ByVal MenuName As String, ByVal CurrentValue As Integer, ByVal GroupID As Integer, Optional ByVal ignore As String = "", Optional ByVal NoneCaption As String = "", Optional ByVal HtmlId As String = "", Optional ByVal HtmlClass As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFormInputMemberSelect2")
            '
            'If Not (true) Then Exit Function
            '
            Dim LastRecordID As Integer
            Dim MemberRulesTableName As String
            Dim iMenuName As String
            Dim iCurrentValue As Integer
            Dim iNoneCaption As String
            Dim CSPointer As Integer
            Dim SelectedFound As Boolean
            Dim RecordID As Integer
            Dim Copy As String
            Dim MethodName As String
            Dim PositionPointer As Integer
            Dim DropDownFieldList As String
            Dim DropDownFieldName() As String
            Dim DropDownDelimiter() As String
            Dim DropDownFieldCount As Integer
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim DropDownPreField As String
            Dim DropDownFieldListLength As Integer
            Dim FieldName As String
            Dim CharAllowed As String
            Dim CharTest As String
            Dim CharPointer As Integer
            Dim AllowedCharacters As String
            'Dim CSContent as integer
            Dim IDFieldPointer As Integer
            Dim FastString As New coreFastStringClass
            '
            Dim RowsArray As String(,)
            Dim RowFieldArray() As String
            Dim RowMax As Integer
            Dim ColumnMax As Integer
            Dim RowPointer As Integer
            Dim ColumnPointer As Integer
            Dim DropDownFieldPointer() As Integer
            Dim UcaseFieldName As String
            Dim SortFieldList As String
            Dim SelectListCount As Integer
            Dim SQL As String
            Dim PeopleTableName As String
            Dim PeopleDataSource As String
            Dim iCriteria As String
            Dim SelectFields As String
            Dim Ptr As Integer
            Dim SelectRaw As String
            Dim CachePtr As Integer
            Dim TagID As String
            Dim TagClass As String
            Dim OrderByField As String
            '
            Const MenuNameFPO = "<MenuName>"
            Const NoneCaptionFPO = "<NoneCaption>"
            '
            MethodName = "main_GetFormInputMemberSelect2"
            '
            iMenuName = genericController.encodeText(MenuName)
            iCurrentValue = genericController.EncodeInteger(CurrentValue)
            iNoneCaption = genericController.encodeEmptyText(NoneCaption, "Select One")
            'iCriteria = genericController.vbLCase(encodeMissingText(Criteria, ""))
            '
            If cpCore.main_InputSelectCacheCnt > 0 Then
                For CachePtr = 0 To cpCore.main_InputSelectCacheCnt - 1
                    With cpCore.main_InputSelectCache(CachePtr)
                        If (.ContentName = "Group:" & GroupID) And (.Criteria = iCriteria) And (genericController.EncodeInteger(.CurrentValue) = iCurrentValue) Then
                            SelectRaw = .SelectRaw
                            Exit For
                        End If
                    End With
                Next
            End If
            '
            '
            '
            If SelectRaw = "" Then
                '
                ' Build the SelectRaw
                ' Test selection size
                '
                PeopleTableName = cpCore.GetContentTablename("people")
                PeopleDataSource = cpCore.main_GetContentDataSource("People")
                MemberRulesTableName = cpCore.GetContentTablename("Member Rules")
                '
                RowMax = 0
                SQL = "select count(*) as cnt" _
                    & " from ccMemberRules R" _
                    & " inner join ccMembers P on R.MemberID=P.ID" _
                    & " where (P.active<>0)" _
                    & " and (R.GroupID=" & GroupID & ")"
                CSPointer = cpCore.db.cs_openCsSql_rev(PeopleDataSource, SQL)
                If cpCore.db.cs_ok(CSPointer) Then
                    RowMax = RowMax + cpCore.db.cs_getInteger(CSPointer, "cnt")
                End If
                Call cpCore.db.cs_Close(CSPointer)
                '
                '        SQL = " select count(*) as cnt" _
                '            & " from ccMembers P" _
                '            & " where (active<>0)" _
                '            & " and(( P.admin<>0 )or( P.developer<>0 ))"
                '        CSPointer = app.csv_OpenCSSQL(PeopleDataSource, SQL, memberID)
                '        If app.csv_IsCSOK(CSPointer) Then
                '            RowMax = RowMax + app.csv_cs_getInteger(CSPointer, "cnt")
                '        End If
                '        Call app.closeCS(CSPointer)
                '
                If RowMax > cpCore.siteProperties.selectFieldLimit Then
                    '
                    ' Selection is too big
                    '
                    cpCore.handleExceptionAndRethrow(New Exception("While building a group members list for group [" & cpCore.group_GetGroupName(GroupID) & "], too many rows were selected. [" & RowMax & "] records exceeds [" & cpCore.siteProperties.selectFieldLimit & "], the current Site Property app.SiteProperty_SelectFieldLimit."))
                    html_GetFormInputMemberSelect2 = html_GetFormInputMemberSelect2 & cpCore.html_GetFormInputHidden(MenuNameFPO, iCurrentValue)
                    If iCurrentValue <> 0 Then
                        CSPointer = cpCore.csOpen("people", iCurrentValue)
                        If cpCore.db.cs_ok(CSPointer) Then
                            html_GetFormInputMemberSelect2 = cpCore.db.cs_getText(CSPointer, "name") & "&nbsp;"
                        End If
                        Call cpCore.db.cs_Close(CSPointer)
                    End If
                    html_GetFormInputMemberSelect2 = html_GetFormInputMemberSelect2 & "(Selection is too large to display)"
                Else
                    '
                    ' ----- Generate Drop Down Field Names
                    '
                    DropDownFieldList = cpCore.GetContentProperty("people", "DropDownFieldList")
                    If DropDownFieldList = "" Then
                        DropDownFieldList = "NAME"
                    End If
                    DropDownFieldCount = 0
                    CharAllowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
                    DropDownFieldListLength = Len(DropDownFieldList)
                    For CharPointer = 1 To DropDownFieldListLength
                        CharTest = Mid(DropDownFieldList, CharPointer, 1)
                        If genericController.vbInstr(1, CharAllowed, CharTest) = 0 Then
                            '
                            ' Character not allowed, delimit Field name here
                            '
                            If (FieldName <> "") Then
                                '
                                ' ----- main_Get new Field Name and save it
                                '
                                If SortFieldList = "" Then
                                    SortFieldList = FieldName
                                End If
                                ReDim Preserve DropDownFieldName(DropDownFieldCount)
                                ReDim Preserve DropDownDelimiter(DropDownFieldCount)
                                DropDownFieldName(DropDownFieldCount) = FieldName
                                DropDownDelimiter(DropDownFieldCount) = CharTest
                                DropDownFieldCount = DropDownFieldCount + 1
                                FieldName = ""
                            Else
                                '
                                ' ----- Save Field Delimiter
                                '
                                If DropDownFieldCount = 0 Then
                                    '
                                    ' ----- Before any field, add to DropDownPreField
                                    '
                                    DropDownPreField = DropDownPreField & CharTest
                                Else
                                    '
                                    ' ----- after a field, add to last DropDownDelimiter
                                    '
                                    DropDownDelimiter(DropDownFieldCount - 1) = DropDownDelimiter(DropDownFieldCount - 1) & CharTest
                                End If
                            End If
                        Else
                            '
                            ' Character Allowed, Put character into fieldname and continue
                            '
                            FieldName = FieldName & CharTest
                        End If
                    Next
                    If FieldName <> "" Then
                        If SortFieldList = "" Then
                            SortFieldList = FieldName
                        End If
                        ReDim Preserve DropDownFieldName(DropDownFieldCount)
                        ReDim Preserve DropDownDelimiter(DropDownFieldCount)
                        DropDownFieldName(DropDownFieldCount) = FieldName
                        DropDownDelimiter(DropDownFieldCount) = ""
                        DropDownFieldCount = DropDownFieldCount + 1
                    End If
                    If DropDownFieldCount = 0 Then
                        cpCore.handleExceptionAndRethrow(New Exception("No drop down field names found for content [" & GroupID & "]."))
                    Else
                        ReDim DropDownFieldPointer(DropDownFieldCount - 1)
                        SelectFields = "P.ID"
                        For Ptr = 0 To DropDownFieldCount - 1
                            SelectFields = SelectFields & ",P." & DropDownFieldName(Ptr)
                        Next
                        '
                        ' ----- Start select box
                        '
                        TagClass = ""
                        If genericController.encodeEmptyText(HtmlClass, "") <> "" Then
                            TagClass = " Class=""" & genericController.encodeEmptyText(HtmlClass, "") & """"
                        End If
                        '
                        TagID = ""
                        If genericController.encodeEmptyText(HtmlId, "") <> "" Then
                            TagID = " ID=""" & genericController.encodeEmptyText(HtmlId, "") & """"
                        End If
                        '
                        Call FastString.Add("<select size=""1"" name=""" & MenuNameFPO & """" & TagID & TagClass & ">")
                        Call FastString.Add("<option value="""">" & NoneCaptionFPO & "</option>")
                        '
                        ' ----- select values
                        '
                        If SortFieldList = "" Then
                            SortFieldList = "name"
                        End If
                        SQL = "select " & SelectFields _
                            & " from ccMemberRules R" _
                            & " inner join ccMembers P on R.MemberID=P.ID" _
                            & " where (R.GroupID=" & GroupID & ")" _
                            & " and((R.DateExpires is null)or(R.DateExpires>" & cpCore.db.encodeSQLDate(Now) & "))" _
                            & " and(P.active<>0)" _
                            & " order by P." & SortFieldList
                        '                SQL = "select " & SelectFields _
                        '                    & " from ccMemberRules R" _
                        '                    & " inner join ccMembers P on R.MemberID=P.ID" _
                        '                    & " where (R.GroupID=" & GroupID & ")" _
                        '                    & " and((R.DateExpires is null)or(R.DateExpires>" & encodeSQLDate(Now) & "))" _
                        '                    & " and(P.active<>0)" _
                        '                    & " union" _
                        '                    & " select P.ID,P.NAME" _
                        '                    & " from ccMembers P" _
                        '                    & " where (active<>0)" _
                        '                    & " and(( P.admin<>0 )or( P.developer<>0 ))" _
                        '                    & " order by P." & SortFieldList
                        CSPointer = cpCore.db.cs_openCsSql_rev(PeopleDataSource, SQL)
                        If cpCore.db.cs_ok(CSPointer) Then
                            RowsArray = cpCore.db.cs_getRows(CSPointer)
                            'RowFieldArray = app.csv_cs_getRowFields(CSPointer)
                            RowFieldArray = Split(cpCore.db.cs_getSelectFieldList(CSPointer), ",")
                            RowMax = UBound(RowsArray, 2)
                            ColumnMax = UBound(RowsArray, 1)
                            '
                            ' setup IDFieldPointer
                            '
                            UcaseFieldName = "ID"
                            For ColumnPointer = 0 To ColumnMax
                                If UcaseFieldName = genericController.vbUCase(RowFieldArray(ColumnPointer)) Then
                                    IDFieldPointer = ColumnPointer
                                    Exit For
                                End If
                            Next
                            '
                            ' setup DropDownFieldPointer()
                            '
                            For FieldPointer = 0 To DropDownFieldCount - 1
                                UcaseFieldName = genericController.vbUCase(DropDownFieldName(FieldPointer))
                                For ColumnPointer = 0 To ColumnMax
                                    If UcaseFieldName = genericController.vbUCase(RowFieldArray(ColumnPointer)) Then
                                        DropDownFieldPointer(FieldPointer) = ColumnPointer
                                        Exit For
                                    End If
                                Next
                            Next
                            '
                            ' output select
                            '
                            SelectedFound = False
                            LastRecordID = -1
                            For RowPointer = 0 To RowMax
                                RecordID = genericController.EncodeInteger(RowsArray(IDFieldPointer, RowPointer))
                                If RecordID <> LastRecordID Then
                                    Copy = DropDownPreField
                                    For FieldPointer = 0 To DropDownFieldCount - 1
                                        Copy = Copy & RowsArray(DropDownFieldPointer(FieldPointer), RowPointer) & DropDownDelimiter(FieldPointer)
                                    Next
                                    If Copy = "" Then
                                        Copy = "no name"
                                    End If
                                    Call FastString.Add(vbCrLf & "<option value=""" & RecordID & """ ")
                                    If RecordID = iCurrentValue Then
                                        Call FastString.Add("selected")
                                        SelectedFound = True
                                    End If
                                    If cpCore.siteProperties.selectFieldWidthLimit <> 0 Then
                                        If Len(Copy) > cpCore.siteProperties.selectFieldWidthLimit Then
                                            Copy = Left(Copy, cpCore.siteProperties.selectFieldWidthLimit) & "...+"
                                        End If
                                    End If
                                    Call FastString.Add(">" & Copy & "</option>")
                                    LastRecordID = RecordID
                                End If
                            Next
                        End If
                        Call FastString.Add("</select>")
                        Call cpCore.db.cs_Close(CSPointer)
                        SelectRaw = FastString.Text
                    End If
                End If
                '
                ' Save the SelectRaw
                '
                CachePtr = cpCore.main_InputSelectCacheCnt
                cpCore.main_InputSelectCacheCnt = cpCore.main_InputSelectCacheCnt + 1
                ReDim Preserve cpCore.main_InputSelectCache(Ptr)
                ReDim Preserve cpCore.main_InputSelectCache(CachePtr)
                cpCore.main_InputSelectCache(CachePtr).ContentName = "Group:" & GroupID
                cpCore.main_InputSelectCache(CachePtr).Criteria = iCriteria
                cpCore.main_InputSelectCache(CachePtr).CurrentValue = iCurrentValue.ToString
                cpCore.main_InputSelectCache(CachePtr).SelectRaw = SelectRaw
            End If
            '
            SelectRaw = genericController.vbReplace(SelectRaw, MenuNameFPO, iMenuName)
            SelectRaw = genericController.vbReplace(SelectRaw, NoneCaptionFPO, iNoneCaption)
            html_GetFormInputMemberSelect2 = SelectRaw
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleLegacyError13(MethodName)
        End Function
        '
        '========================================================================
        '   Legacy
        '========================================================================
        '
        Public Function main_GetFormInputSelectList(ByVal MenuName As String, ByVal CurrentValue As String, ByVal SelectList As String, Optional ByVal NoneCaption As String = "", Optional ByVal htmlId As String = "") As String
            main_GetFormInputSelectList = main_GetFormInputSelectList2(genericController.encodeText(MenuName), genericController.EncodeInteger(CurrentValue), genericController.encodeText(SelectList), genericController.encodeText(NoneCaption), genericController.encodeText(htmlId))
        End Function
        '
        '========================================================================
        '   Create a select list from a comma separated list
        '       returns an index into the list list, starting at 1
        '       if an element is blank (,) no option is created
        '========================================================================
        '
        Public Function main_GetFormInputSelectList2(ByVal MenuName As String, ByVal CurrentValue As Integer, ByVal SelectList As String, ByVal NoneCaption As String, ByVal htmlId As String, Optional ByVal HtmlClass As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFormInputSelectList2")
            '
            Dim FastString As New coreFastStringClass
            Dim lookups() As String
            Dim iSelectList As String
            Dim Ptr As Integer
            Dim RecordID As Integer
            'Dim SelectedFound As Integer
            Dim Copy As String
            Dim TagID As String
            Dim SelectFieldWidthLimit As Integer
            '
            SelectFieldWidthLimit = cpCore.siteProperties.selectFieldWidthLimit
            If SelectFieldWidthLimit = 0 Then
                SelectFieldWidthLimit = 256
            End If
            '
            'iSelectList = genericController.encodeText(SelectList)
            '
            ' ----- Start select box
            '
            Call FastString.Add("<select id=""" & htmlId & """ class=""" & HtmlClass & """ size=""1"" name=""" & MenuName & """>")
            If NoneCaption <> "" Then
                Call FastString.Add("<option value="""">" & NoneCaption & "</option>")
            Else
                Call FastString.Add("<option value="""">Select One</option>")
            End If
            '
            ' ----- select values
            '
            lookups = Split(SelectList, ",")
            For Ptr = 0 To UBound(lookups)
                RecordID = Ptr + 1
                Copy = lookups(Ptr)
                If Copy <> "" Then
                    Call FastString.Add(vbCrLf & "<option value=""" & RecordID & """ ")
                    If RecordID = CurrentValue Then
                        Call FastString.Add("selected")
                        'SelectedFound = True
                    End If
                    If Len(Copy) > SelectFieldWidthLimit Then
                        Copy = Left(Copy, SelectFieldWidthLimit) & "...+"
                    End If
                    Call FastString.Add(">" & Copy & "</option>")
                End If
            Next
            Call FastString.Add("</select>")
            main_GetFormInputSelectList2 = FastString.Text
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleLegacyError13("main_GetFormInputSelectList2")
        End Function
        '
        '========================================================================
        '   Display an icon with a link to the login form/cclib.net/admin area
        '========================================================================
        '
        Public Function main_GetLoginLink() As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetLoginLink")
            '
            'If Not (true) Then Exit Function
            '
            Dim Link As String
            Dim IconFilename As String
            '
            If cpCore.siteProperties.getBoolean("AllowLoginIcon", True) Then
                main_GetLoginLink = main_GetLoginLink & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">"
                main_GetLoginLink = main_GetLoginLink & "<tr><td align=""right"">"
                If cpCore.user.isAuthenticatedContentManager() Then
                    main_GetLoginLink = main_GetLoginLink & "<a href=""" & cpCore.htmlDoc.html_EncodeHTML(cpCore.siteProperties.adminURL) & """ target=""_blank"">"
                Else
                    Link = cpCore.webServer.webServerIO_requestPage & "?" & cpCore.web_RefreshQueryString
                    Link = genericController.modifyLinkQuery(Link, RequestNameHardCodedPage, HardCodedPageLogin, True)
                    'Link = genericController.modifyLinkQuery(Link, RequestNameInterceptpage, LegacyInterceptPageSNLogin, True)
                    main_GetLoginLink = main_GetLoginLink & "<a href=""" & cpCore.htmlDoc.html_EncodeHTML(Link) & """ >"
                End If
                IconFilename = cpCore.siteProperties.LoginIconFilename
                If genericController.vbLCase(Mid(IconFilename, 1, 7)) <> "/ccLib/" Then
                    IconFilename = cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, IconFilename)
                End If
                main_GetLoginLink = main_GetLoginLink & "<img alt=""Login"" src=""" & IconFilename & """ border=""0"" >"
                main_GetLoginLink = main_GetLoginLink & "</A>"
                main_GetLoginLink = main_GetLoginLink & "</td></tr></table>"
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleLegacyError18("main_GetLoginLink")
        End Function
        '
        '========================================================================
        '   legacy
        '========================================================================
        '
        Public Function main_GetClosePage(Optional ByVal AllowLogin As Boolean = True, Optional ByVal AllowTools As Boolean = True) As String
            main_GetClosePage = main_GetClosePage3(AllowLogin, AllowTools, False, False)
        End Function
        '
        '========================================================================
        '   legacy
        '========================================================================
        '
        Public Function main_GetClosePage2(AllowLogin As Boolean, AllowTools As Boolean, BlockNonContentExtras As Boolean) As String
            Try
                main_GetClosePage2 = main_GetClosePage3(AllowLogin, AllowTools, False, False)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Function
        '
        '========================================================================
        '   main_GetClosePage3
        '       Public interface to end the page call
        '       Must be called last on every public page
        '       internally, you can NOT writeAltBuffer( main_GetClosePage3 ) because the stream is closed
        '       call main_GetEndOfBody - main_Gets toolspanel and all html,menuing,etc needed to finish page
        '       optionally calls main_dispose
        '========================================================================
        '
        Public Function main_GetClosePage3(AllowLogin As Boolean, AllowTools As Boolean, BlockNonContentExtras As Boolean, doNotDisposeOnExit As Boolean) As String
            Try
                main_GetClosePage3 = html_GetEndOfBody(AllowLogin, AllowTools, BlockNonContentExtras, False)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Function
        '
        '========================================================================
        '   Write to the HTML stream
        '========================================================================
        ' refactor -- if this conversion goes correctly, all writeStream will mvoe to teh executeRoute which returns the string 
        Public Sub writeAltBuffer(ByVal Message As Object)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("WriteStream")
            '
            If cpCore.docOpen Then
                Select Case cpCore.webServer.webServerIO_OutStreamDevice
                    Case htmlDoc_OutStreamJavaScript
                        Call webServerIO_JavaStream_Add(genericController.encodeText(Message))
                    Case Else

                        If (cpCore.webServer.iisContext IsNot Nothing) Then
                            htmlDoc_IsStreamWritten = True
                            Call cpCore.webServer.iisContext.Response.Write(genericController.encodeText(Message))
                        Else
                            docBuffer = docBuffer & genericController.encodeText(Message)
                        End If
                End Select
            End If
            '
            Exit Sub
ErrorTrap:
            Call cpCore.handleLegacyError18("writeAltBuffer")
        End Sub

        '
        '
        Private Sub webServerIO_JavaStream_Add(ByVal NewString As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00375")
            '
            If htmlDoc_JavaStreamCount >= htmlDoc_JavaStreamSize Then
                htmlDoc_JavaStreamSize = htmlDoc_JavaStreamSize + htmlDoc_JavaStreamChunk
                ReDim Preserve htmlDoc_JavaStreamHolder(htmlDoc_JavaStreamSize)
            End If
            htmlDoc_JavaStreamHolder(htmlDoc_JavaStreamCount) = NewString
            htmlDoc_JavaStreamCount = htmlDoc_JavaStreamCount + 1
            Exit Sub
            '
ErrorTrap:
            Call cpCore.handleLegacyError13("main_JavaStream_Add")
        End Sub
        '
        '
        '
        Public ReadOnly Property webServerIO_JavaStream_Text() As String
            Get
                Dim MsgLabel As String
                '
                MsgLabel = "Msg" & genericController.encodeText(genericController.GetRandomInteger)
                '
                webServerIO_JavaStream_Text = Join(htmlDoc_JavaStreamHolder, "")
                webServerIO_JavaStream_Text = genericController.vbReplace(webServerIO_JavaStream_Text, "'", "'+""'""+'")
                webServerIO_JavaStream_Text = genericController.vbReplace(webServerIO_JavaStream_Text, vbCrLf, "\n")
                webServerIO_JavaStream_Text = genericController.vbReplace(webServerIO_JavaStream_Text, vbCr, "\n")
                webServerIO_JavaStream_Text = genericController.vbReplace(webServerIO_JavaStream_Text, vbLf, "\n")
                webServerIO_JavaStream_Text = "var " & MsgLabel & " = '" & webServerIO_JavaStream_Text & "'; document.write( " & MsgLabel & " ); " & vbCrLf

            End Get
        End Property
        '
        '
        '
        Public Sub webServerIO_addRefreshQueryString(ByVal Name As String, Optional ByVal Value As String = "")
            Try
                Dim temp() As String
                '
                If (InStr(1, Name, "=") > 0) Then
                    temp = Split(Name, "=")
                    htmlDoc_RefreshQueryString = genericController.ModifyQueryString(htmlDoc_RefreshQueryString, temp(0), temp(1), True)
                Else
                    htmlDoc_RefreshQueryString = genericController.ModifyQueryString(htmlDoc_RefreshQueryString, Name, Value, True)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            '
        End Sub
    End Class
End Namespace
