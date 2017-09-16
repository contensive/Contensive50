
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports Contensive.Core.Models.Entity

Namespace Contensive.Core.Controllers
    ''' <summary>
    ''' Tools used to assemble html document elements. This is not a storage for assembling a document (see docController)
    ''' </summary>
    Public Class htmlController

        '
        Private cpCore As coreClass
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
                cpCore.handleException(ex) : Throw
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
                cpCore.handleException(ex) : Throw
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
                cpCore.handleException(ex) : Throw
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
                cpCore.handleException(ex) : Throw
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
                cpCore.handleException(ex) : Throw
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
                    If LoopPtr >= 10000 Then
                        cpCore.handleException(New ApplicationException("Tag limit of 10000 tags per block reached."))
                    End If
                End If
                '
                returnValue = returnPos
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
                cpCore.handleException(ex) : Throw
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
                cpCore.handleException(ex) : Throw
            End Try
            Return returnValue
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
        '====================================================================================================
        '
        Public Function getHtmlDoc_beforeEndOfBodyHtml(AllowLogin As Boolean, AllowTools As Boolean, BlockNonContentExtras As Boolean, main_IsAdminSite As Boolean) As String
            Dim result As String = ""
            Try
                Dim Ptr As Integer
                Dim JSCodeAsString As String
                Dim JS As String
                Dim RenderTimeString As String = Format((GetTickCount - cpCore.profileStartTickCount) / 1000, "0.000")
                '
                ' -- content extras like tool panel
                If (Not BlockNonContentExtras) Then
                    If cpCore.authContext.isAuthenticatedContentManager(cpCore) And cpCore.authContext.user.AllowToolsPanel Then
                        If AllowTools Then
                            result &= cpCore.html.main_GetToolsPanel()
                        End If
                    Else
                        If AllowLogin Then
                            result &= main_GetLoginLink()
                        End If
                    End If
                    '
                    ' -- Include any other close page
                    If cpCore.doc.htmlForEndOfBody <> "" Then
                        result = result & cpCore.doc.htmlForEndOfBody
                    End If
                    If cpCore.testPointMessage <> "" Then
                        result = result & "<div class=""ccTestPointMessageCon"">" & cpCore.testPointMessage & "</div>"
                    End If
                End If
                '
                ' TODO -- closing the menu attaches the flyout panels -- should be done when the menu is returned, not at page end
                ' -- output the menu system
                If Not (cpCore.menuFlyout Is Nothing) Then
                    result &= cpCore.menuFlyout.menu_GetClose()
                End If
                '
                ' -- Add Script Code to Head
                JS = ""
                If cpCore.doc.headScripts.Count > 0 Then
                    For Ptr = 0 To cpCore.doc.headScripts.Count - 1
                        With cpCore.doc.headScripts(Ptr)
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
                    cpCore.doc.headScripts = {}
                End If
                '
                ' ----- Add onload javascript
                '
                If (cpCore.doc.onLoadJavascript <> "") Then
                    JS = JS & vbCrLf & vbTab & "cj.addLoadEvent(function(){" & cpCore.doc.onLoadJavascript & "});"
                End If
                '
                ' ----- Add any left over style links
                '
                Dim headTags As String
                headTags = ""
                '
                If (cpCore.doc.metaContent_StyleSheetTags <> "") Then
                    headTags = headTags & cr & cpCore.doc.metaContent_StyleSheetTags
                    'JS = JS & vbCrLf & vbTab & "cjAddHeadTag('" & genericController.EncodeJavascript(main_MetaContent_StyleSheetTags) & "');"
                    cpCore.doc.metaContent_StyleSheetTags = ""
                End If
                '
                ' -- Add any left over head tags
                If (cpCore.doc.metaContent_OtherHeadTags <> "") Then
                    headTags = headTags & cr & cpCore.doc.metaContent_OtherHeadTags
                    'JS = JS & vbCrLf & vbTab & "cjAddHeadTag('" & genericController.EncodeJavascript(main_MetaContent_OtherHeadTags) & "');"
                    cpCore.doc.metaContent_OtherHeadTags = ""
                End If
                If (headTags <> "") Then
                    JS = JS & vbCrLf & vbTab & "cj.addHeadTag('" & genericController.EncodeJavascript(headTags) & "');"
                    'JS = JS & vbCrLf & vbTab & "cjAddHeadTag('" & genericController.EncodeJavascript(headTags) & "');"
                End If
                '
                ' -- Add end of body javascript
                If (cpCore.doc.endOfBodyJavascript <> "") Then
                    JS = JS & vbCrLf & cpCore.doc.endOfBodyJavascript
                    cpCore.doc.endOfBodyJavascript = ""
                End If
                '
                ' -- If javascript stream, output it all now
                If (cpCore.webServer.outStreamDevice = htmlDoc_OutStreamJavaScript) Then
                    '
                    ' This is a js output stream from a <script src=url></script>
                    ' process everything into a var=msg;document.write(var)
                    ' any js from the page should be added to this group
                    '
                    Call writeAltBuffer(result)
                    cpCore.webServer.outStreamDevice = htmlDoc_OutStreamStandard
                    result = webServerIO_JavaStream_Text
                    If JS <> "" Then
                        result = result & vbCrLf & JS
                        JS = ""
                    End If
                Else
                    '
                    ' This is a standard html write
                    ' any javascript collected should go in a <script tag
                    '
                    If JS <> "" Then
                        result = result _
                            & vbCrLf & "<script Language=""javascript"" type=""text/javascript"">" _
                            & vbCrLf & "if(cj){" _
                            & JS _
                            & vbCrLf & "}" _
                            & vbCrLf & "</script>"
                        JS = ""
                    End If
                End If
                '
                ' -- end-of-body string -- include it without csv because it may have error strings
                If (Not BlockNonContentExtras) And (cpCore.doc.endOfBodyString <> "") Then
                    result = result & cpCore.doc.endOfBodyString
                End If
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return result
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
            Dim result As String = String.Empty
            Try
                Const MenuNameFPO = "<MenuName>"
                Const NoneCaptionFPO = "<NoneCaption>"
                Dim CDef As cdefModel
                Dim ContentControlCriteria As String
                Dim LcaseCriteria As String
                Dim CSPointer As Integer
                Dim SelectedFound As Boolean
                Dim RecordID As Integer
                Dim Copy As String
                Dim MethodName As String
                Dim DropDownFieldList As String
                Dim DropDownFieldName As String() = {}
                Dim DropDownDelimiter As String() = {}
                Dim DropDownFieldCount As Integer
                Dim DropDownPreField As String = String.Empty
                Dim DropDownFieldListLength As Integer
                Dim FieldName As String = String.Empty
                Dim CharAllowed As String
                Dim CharTest As String
                Dim CharPointer As Integer
                Dim IDFieldPointer As Integer
                Dim FastString As New stringBuilderLegacyController
                Dim RowsArray(,) As String
                Dim RowFieldArray() As String
                Dim RowCnt As Integer
                Dim RowMax As Integer
                Dim ColumnMax As Integer
                Dim RowPointer As Integer
                Dim ColumnPointer As Integer
                Dim DropDownFieldPointer() As Integer
                Dim UcaseFieldName As String
                Dim SortFieldList As String = String.Empty
                Dim SQL As String
                Dim TableName As String
                Dim DataSource As String
                Dim SelectFields As String
                Dim Ptr As Integer
                Dim SelectRaw As String = String.Empty
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
                If cpCore.doc.inputSelectCacheCnt > 0 Then
                    For CachePtr = 0 To cpCore.doc.inputSelectCacheCnt - 1
                        With cpCore.doc.inputSelectCache(CachePtr)
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
                        Call errorController.error_AddUserError(cpCore, "The drop down list for " & ContentName & " called " & MenuName & " is too long to display. The site administrator has been notified and the problem will be resolved shortly. To fix this issue temporarily, go to the admin tab of the Preferences page and set the Select Field Limit larger than " & RowCnt & ".")
                        '                    cpcore.handleException(New Exception("Legacy error, MethodName=[" & MethodName & "], cause=[" & Cause & "] #" & Err.Number & "," & Err.Source & "," & Err.Description & ""), Cause, 2)

                        cpCore.handleException(New Exception("Error creating select list from content [" & ContentName & "] called [" & MenuName & "]. Selection of [" & RowCnt & "] records exceeds [" & cpCore.siteProperties.selectFieldLimit & "], the current Site Property SelectFieldLimit."))
                        result = result & html_GetFormInputHidden(MenuNameFPO, CurrentValue)
                        If CurrentValue = 0 Then
                            result = html_GetFormInputText2(MenuName, "0")
                        Else
                            CSPointer = cpCore.db.csOpenRecord(ContentName, CurrentValue)
                            If cpCore.db.cs_ok(CSPointer) Then
                                result = cpCore.db.cs_getText(CSPointer, "name") & "&nbsp;"
                            End If
                            Call cpCore.db.cs_Close(CSPointer)
                        End If
                        result = result & "(Selection is too large to display option list)"
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
                            cpCore.handleException(New Exception("No drop down field names found for content [" & ContentName & "]."))
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
                                RowsArray = cpCore.db.cs_getRows(CSPointer)
                                RowFieldArray = Split(cpCore.db.cs_getSelectFieldList(CSPointer), ",")
                                ColumnMax = UBound(RowsArray, 1)
                                RowMax = UBound(RowsArray, 2)
                                '
                                ' -- setup IDFieldPointer
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
                                    Call FastString.Add(">" & encodeHTML(Copy) & "</option>")
                                Next
                                If Not SelectedFound And (CurrentValue <> 0) Then
                                    Call cpCore.db.cs_Close(CSPointer)
                                    If Criteria <> "" Then
                                        Criteria = Criteria & "and"
                                    End If
                                    Criteria = Criteria & "(id=" & genericController.EncodeInteger(CurrentValue) & ")"
                                    CSPointer = cpCore.db.cs_open(ContentName, Criteria, SortFieldList, False, , , , SelectFields)
                                    If cpCore.db.cs_ok(CSPointer) Then
                                        RowsArray = cpCore.db.cs_getRows(CSPointer)
                                        RowFieldArray = Split(cpCore.db.cs_getSelectFieldList(CSPointer), ",")
                                        RowMax = UBound(RowsArray, 2)
                                        ColumnMax = UBound(RowsArray, 1)
                                        RecordID = genericController.EncodeInteger(RowsArray(IDFieldPointer, 0))
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
                                        Call FastString.Add(">" & encodeHTML(Copy) & "</option>")
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
                        CachePtr = cpCore.doc.inputSelectCacheCnt
                        cpCore.doc.inputSelectCacheCnt = cpCore.doc.inputSelectCacheCnt + 1
                        ReDim Preserve cpCore.doc.inputSelectCache(Ptr)
                        ReDim Preserve cpCore.doc.inputSelectCache(CachePtr)
                        cpCore.doc.inputSelectCache(CachePtr).ContentName = ContentName
                        cpCore.doc.inputSelectCache(CachePtr).Criteria = LcaseCriteria
                        cpCore.doc.inputSelectCache(CachePtr).CurrentValue = CurrentValue.ToString
                        cpCore.doc.inputSelectCache(CachePtr).SelectRaw = SelectRaw
                    End If
                End If
                '
                SelectRaw = genericController.vbReplace(SelectRaw, MenuNameFPO, MenuName)
                SelectRaw = genericController.vbReplace(SelectRaw, NoneCaptionFPO, NoneCaption)
                If HtmlClass <> "" Then
                    SelectRaw = genericController.vbReplace(SelectRaw, "<select ", "<select class=""" & HtmlClass & """")
                End If
                result = SelectRaw
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function getInputMemberSelect(ByVal MenuName As String, ByVal CurrentValue As Integer, ByVal GroupID As Integer, Optional ByVal ignore As String = "", Optional ByVal NoneCaption As String = "", Optional ByVal htmlId As String = "") As String
            getInputMemberSelect = html_GetFormInputMemberSelect2(MenuName, CurrentValue, GroupID, , NoneCaption, htmlId)
        End Function
        '
        Public Function html_GetFormInputMemberSelect2(ByVal MenuName As String, ByVal CurrentValue As Integer, ByVal GroupID As Integer, Optional ByVal ignore As String = "", Optional ByVal NoneCaption As String = "", Optional ByVal HtmlId As String = "", Optional ByVal HtmlClass As String = "") As String
            Dim result As String = String.Empty
            Try
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
                Dim DropDownFieldList As String
                Dim DropDownFieldName As String() = {}
                Dim DropDownDelimiter As String() = {}
                Dim DropDownFieldCount As Integer
                ' converted array to dictionary - Dim FieldPointer As Integer
                Dim DropDownPreField As String = String.Empty
                Dim DropDownFieldListLength As Integer
                Dim FieldName As String = String.Empty
                Dim CharAllowed As String
                Dim CharTest As String
                Dim CharPointer As Integer
                Dim IDFieldPointer As Integer
                Dim FastString As New stringBuilderLegacyController
                '
                Dim RowsArray As String(,)
                Dim RowFieldArray() As String
                Dim RowMax As Integer
                Dim ColumnMax As Integer
                Dim RowPointer As Integer
                Dim ColumnPointer As Integer
                Dim DropDownFieldPointer() As Integer
                Dim UcaseFieldName As String
                Dim SortFieldList As String = String.Empty
                Dim SQL As String
                Dim PeopleTableName As String
                Dim PeopleDataSource As String
                Dim iCriteria As String = String.Empty
                Dim SelectFields As String
                Dim Ptr As Integer
                Dim SelectRaw As String = String.Empty
                Dim CachePtr As Integer
                Dim TagID As String
                Dim TagClass As String
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
                If cpCore.doc.inputSelectCacheCnt > 0 Then
                    For CachePtr = 0 To cpCore.doc.inputSelectCacheCnt - 1
                        With cpCore.doc.inputSelectCache(CachePtr)
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
                    PeopleTableName = cpCore.metaData.getContentTablename("people")
                    PeopleDataSource = cpCore.metaData.getContentDataSource("People")
                    MemberRulesTableName = cpCore.metaData.getContentTablename("Member Rules")
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
                        cpCore.handleException(New Exception("While building a group members list for group [" & groupController.group_GetGroupName(cpCore, GroupID) & "], too many rows were selected. [" & RowMax & "] records exceeds [" & cpCore.siteProperties.selectFieldLimit & "], the current Site Property app.SiteProperty_SelectFieldLimit."))
                        result = result & html_GetFormInputHidden(MenuNameFPO, iCurrentValue)
                        If iCurrentValue <> 0 Then
                            CSPointer = cpCore.db.csOpenRecord("people", iCurrentValue)
                            If cpCore.db.cs_ok(CSPointer) Then
                                result = cpCore.db.cs_getText(CSPointer, "name") & "&nbsp;"
                            End If
                            Call cpCore.db.cs_Close(CSPointer)
                        End If
                        result = result & "(Selection is too large to display)"
                    Else
                        '
                        ' ----- Generate Drop Down Field Names
                        '
                        DropDownFieldList = cpCore.metaData.GetContentProperty("people", "DropDownFieldList")
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
                            cpCore.handleException(New Exception("No drop down field names found for content [" & GroupID & "]."))
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
                    CachePtr = cpCore.doc.inputSelectCacheCnt
                    cpCore.doc.inputSelectCacheCnt = cpCore.doc.inputSelectCacheCnt + 1
                    ReDim Preserve cpCore.doc.inputSelectCache(Ptr)
                    ReDim Preserve cpCore.doc.inputSelectCache(CachePtr)
                    cpCore.doc.inputSelectCache(CachePtr).ContentName = "Group:" & GroupID
                    cpCore.doc.inputSelectCache(CachePtr).Criteria = iCriteria
                    cpCore.doc.inputSelectCache(CachePtr).CurrentValue = iCurrentValue.ToString
                    cpCore.doc.inputSelectCache(CachePtr).SelectRaw = SelectRaw
                End If
                '
                SelectRaw = genericController.vbReplace(SelectRaw, MenuNameFPO, iMenuName)
                SelectRaw = genericController.vbReplace(SelectRaw, NoneCaptionFPO, iNoneCaption)
                result = SelectRaw
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        '   Legacy
        '========================================================================
        '
        Public Function getInputSelectList(ByVal MenuName As String, ByVal CurrentValue As String, ByVal SelectList As String, Optional ByVal NoneCaption As String = "", Optional ByVal htmlId As String = "") As String
            getInputSelectList = getInputSelectList2(genericController.encodeText(MenuName), genericController.EncodeInteger(CurrentValue), genericController.encodeText(SelectList), genericController.encodeText(NoneCaption), genericController.encodeText(htmlId))
        End Function
        '
        '========================================================================
        '   Create a select list from a comma separated list
        '       returns an index into the list list, starting at 1
        '       if an element is blank (,) no option is created
        '========================================================================
        '
        Public Function getInputSelectList2(ByVal MenuName As String, ByVal CurrentValue As Integer, ByVal SelectList As String, ByVal NoneCaption As String, ByVal htmlId As String, Optional ByVal HtmlClass As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFormInputSelectList2")
            '
            Dim FastString As New stringBuilderLegacyController
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
            getInputSelectList2 = FastString.Text
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_GetFormInputSelectList2")
        End Function
        '
        '========================================================================
        '   Display an icon with a link to the login form/cclib.net/admin area
        '========================================================================
        '
        Public Function main_GetLoginLink() As String
            Dim result As String = String.Empty
            Try
                '
                'If Not (true) Then Exit Function
                '
                Dim Link As String
                Dim IconFilename As String
                '
                If cpCore.siteProperties.getBoolean("AllowLoginIcon", True) Then
                    result = result & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">"
                    result = result & "<tr><td align=""right"">"
                    If cpCore.authContext.isAuthenticatedContentManager(cpCore) Then
                        result = result & "<a href=""" & encodeHTML(cpCore.siteProperties.adminURL) & """ target=""_blank"">"
                    Else
                        Link = cpCore.webServer.requestPage & "?" & cpCore.doc.refreshQueryString
                        Link = genericController.modifyLinkQuery(Link, RequestNameHardCodedPage, HardCodedPageLogin, True)
                        'Link = genericController.modifyLinkQuery(Link, RequestNameInterceptpage, LegacyInterceptPageSNLogin, True)
                        result = result & "<a href=""" & encodeHTML(Link) & """ >"
                    End If
                    IconFilename = cpCore.siteProperties.LoginIconFilename
                    If genericController.vbLCase(Mid(IconFilename, 1, 7)) <> "/ccLib/" Then
                        IconFilename = genericController.getCdnFileLink(cpCore, IconFilename)
                    End If
                    result = result & "<img alt=""Login"" src=""" & IconFilename & """ border=""0"" >"
                    result = result & "</A>"
                    result = result & "</td></tr></table>"
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        ''
        ''========================================================================
        ''   legacy
        ''========================================================================
        ''
        'Public Function main_GetClosePage(Optional ByVal AllowLogin As Boolean = True, Optional ByVal AllowTools As Boolean = True) As String
        '    main_GetClosePage = main_GetClosePage3(AllowLogin, AllowTools, False, False)
        'End Function
        ''
        ''========================================================================
        ''   legacy
        ''========================================================================
        ''
        'Public Function main_GetClosePage2(AllowLogin As Boolean, AllowTools As Boolean, BlockNonContentExtras As Boolean) As String
        '    Try
        '        main_GetClosePage2 = main_GetClosePage3(AllowLogin, AllowTools, False, False)
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex) : Throw
        '    End Try
        'End Function
        ''
        ''========================================================================
        ''   main_GetClosePage3
        ''       Public interface to end the page call
        ''       Must be called last on every public page
        ''       internally, you can NOT writeAltBuffer( main_GetClosePage3 ) because the stream is closed
        ''       call main_GetEndOfBody - main_Gets toolspanel and all html,menuing,etc needed to finish page
        ''       optionally calls main_dispose
        ''========================================================================
        ''
        'Public Function main_GetClosePage3(AllowLogin As Boolean, AllowTools As Boolean, BlockNonContentExtras As Boolean, doNotDisposeOnExit As Boolean) As String
        '    Try
        '        Return getBeforeEndOfBodyHtml(AllowLogin, AllowTools, BlockNonContentExtras, False)
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex) : Throw
        '    End Try
        'End Function
        '
        '========================================================================
        '   Write to the HTML stream
        '========================================================================
        ' refactor -- if this conversion goes correctly, all writeStream will mvoe to teh executeRoute which returns the string 
        Public Sub writeAltBuffer(ByVal Message As Object)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("WriteStream")
            '
            If cpCore.continueProcessing Then
                Select Case cpCore.webServer.outStreamDevice
                    Case htmlDoc_OutStreamJavaScript
                        Call webServerIO_JavaStream_Add(genericController.encodeText(Message))
                    Case Else

                        If (cpCore.webServer.iisContext IsNot Nothing) Then
                            cpCore.doc.isStreamWritten = True
                            Call cpCore.webServer.iisContext.Response.Write(genericController.encodeText(Message))
                        Else
                            cpCore.doc.docBuffer = cpCore.doc.docBuffer & genericController.encodeText(Message)
                        End If
                End Select
            End If
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("writeAltBuffer")
        End Sub

        '
        '
        Private Sub webServerIO_JavaStream_Add(ByVal NewString As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00375")
            '
            If cpCore.doc.javascriptStreamCount >= cpCore.doc.javascriptStreamSize Then
                cpCore.doc.javascriptStreamSize = cpCore.doc.javascriptStreamSize + htmlDoc_JavaStreamChunk
                ReDim Preserve cpCore.doc.javascriptStreamHolder(cpCore.doc.javascriptStreamSize)
            End If
            cpCore.doc.javascriptStreamHolder(cpCore.doc.javascriptStreamCount) = NewString
            cpCore.doc.javascriptStreamCount = cpCore.doc.javascriptStreamCount + 1
            Exit Sub
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_JavaStream_Add")
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
                webServerIO_JavaStream_Text = Join(cpCore.doc.javascriptStreamHolder, "")
                webServerIO_JavaStream_Text = genericController.vbReplace(webServerIO_JavaStream_Text, "'", "'+""'""+'")
                webServerIO_JavaStream_Text = genericController.vbReplace(webServerIO_JavaStream_Text, vbCrLf, "\n")
                webServerIO_JavaStream_Text = genericController.vbReplace(webServerIO_JavaStream_Text, vbCr, "\n")
                webServerIO_JavaStream_Text = genericController.vbReplace(webServerIO_JavaStream_Text, vbLf, "\n")
                webServerIO_JavaStream_Text = "var " & MsgLabel & " = '" & webServerIO_JavaStream_Text & "'; document.write( " & MsgLabel & " ); " & vbCrLf

            End Get
        End Property
        ''
        ''
        ''
        'Public Sub webServerIO_addRefreshQueryString(ByVal Name As String, Optional ByVal Value As String = "")
        '    Try
        '        Dim temp() As String
        '        '
        '        If (InStr(1, Name, "=") > 0) Then
        '            temp = Split(Name, "=")
        '            cpCore.doc.refreshQueryString = genericController.ModifyQueryString(cpCore.doc.refreshQueryString, temp(0), temp(1), True)
        '        Else
        '            cpCore.doc.refreshQueryString = genericController.ModifyQueryString(cpCore.doc.refreshQueryString, Name, Value, True)
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex) : Throw
        '    End Try
        '
        '  End Sub
        '
        Public Function html_GetLegacySiteStyles() As String
            On Error GoTo ErrorTrap
            '
            If Not cpCore.doc.legacySiteStyles_Loaded Then
                cpCore.doc.legacySiteStyles_Loaded = True
                '
                ' compatibility with old sites - if they do not main_Get the default style sheet, put it in here
                '
                If False Then
                    html_GetLegacySiteStyles = "" _
                        & cr & "<!-- compatibility with legacy framework --><style type=text/css>" _
                        & cr & " .ccEditWrapper {border-top:1px solid #6a6;border-left:1px solid #6a6;border-bottom:1px solid #cec;border-right:1px solid #cec;}" _
                        & cr & " .ccEditWrapperInner {border-top:1px solid #cec;border-left:1px solid #cec;border-bottom:1px solid #6a6;border-right:1px solid #6a6;}" _
                        & cr & " .ccEditWrapperCaption {text-align:left;border-bottom:1px solid #888;padding:4px;background-color:#40C040;color:black;}" _
                        & cr & " .ccEditWrapperContent{padding:4px;}" _
                        & cr & " .ccHintWrapper {border:1px dashed #888;margin-bottom:10px}" _
                        & cr & " .ccHintWrapperContent{padding:10px;background-color:#80E080;color:black;}" _
                        & "</style>"
                Else
                    html_GetLegacySiteStyles = "" _
                        & cr & "<!-- compatibility with legacy framework --><style type=text/css>" _
                        & cr & " .ccEditWrapper {border:1px dashed #808080;}" _
                        & cr & " .ccEditWrapperCaption {text-align:left;border-bottom:1px solid #808080;padding:4px;background-color:#40C040;color:black;}" _
                        & cr & " .ccEditWrapperContent{padding:4px;}" _
                        & cr & " .ccHintWrapper {border:1px dashed #808080;margin-bottom:10px}" _
                        & cr & " .ccHintWrapperContent{padding:10px;background-color:#80E080;color:black;}" _
                        & "</style>"
                End If
            End If
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_GetLegacySiteStyles")
        End Function
        '
        '===================================================================================================
        '   Wrap the content in a common wrapper if authoring is enabled
        '===================================================================================================
        '
        Public Function html_GetAdminHintWrapper(ByVal Content As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetAdminHintWrapper")
            '
            'If Not (true) Then Exit Function
            '
            html_GetAdminHintWrapper = ""
            If cpCore.authContext.isEditing("") Or cpCore.authContext.isAuthenticatedAdmin(cpCore) Then
                html_GetAdminHintWrapper = html_GetAdminHintWrapper & html_GetLegacySiteStyles()
                html_GetAdminHintWrapper = html_GetAdminHintWrapper _
                    & "<table border=0 width=""100%"" cellspacing=0 cellpadding=0><tr><td class=""ccHintWrapper"">" _
                        & "<table border=0 width=""100%"" cellspacing=0 cellpadding=0><tr><td class=""ccHintWrapperContent"">" _
                        & "<b>Administrator</b>" _
                        & "<br>" _
                        & "<br>" & genericController.encodeText(Content) _
                        & "</td></tr></table>" _
                    & "</td></tr></table>"
            End If

            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetAdminHintWrapper")
        End Function
        '
        '
        '
        Public Sub enableOutputBuffer(BufferOn As Boolean)
            Try
                If cpCore.doc.outputBufferEnabled Then
                    '
                    ' ----- once on, can not be turned off Response Object
                    '
                    cpCore.doc.outputBufferEnabled = BufferOn
                Else
                    '
                    ' ----- StreamBuffer off, allow on and off
                    '
                    cpCore.doc.outputBufferEnabled = BufferOn
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub

        '
        '========================================================================
        ' ----- Starts an HTML form for uploads
        '       Should be closed with main_GetUploadFormEnd
        '========================================================================
        '
        Public Function html_GetUploadFormStart(Optional ByVal ActionQueryString As String = Nothing) As String

            If ActionQueryString Is Nothing Then
                ActionQueryString = cpCore.doc.refreshQueryString
            End If
            On Error GoTo ErrorTrap
            '
            Dim iActionQueryString As String
            '
            iActionQueryString = genericController.ModifyQueryString(ActionQueryString, RequestNameRequestBinary, True, True)
            '
            html_GetUploadFormStart = "<form action=""" & cpCore.webServer.serverFormActionURL & "?" & iActionQueryString & """ ENCTYPE=""MULTIPART/FORM-DATA"" METHOD=""POST""  style=""display: inline;"" >"
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetUploadFormStart")
        End Function
        '
        '========================================================================
        ' ----- Closes an HTML form for uploads
        '========================================================================
        '
        Public Function html_GetUploadFormEnd() As String
            html_GetUploadFormEnd = html_GetFormEnd()
        End Function
        '
        '========================================================================
        ' ----- Starts an HTML form
        '       Should be closed with PrintFormEnd
        '========================================================================
        '
        Public Function html_GetFormStart(Optional ByVal ActionQueryString As String = Nothing, Optional ByVal htmlName As String = "", Optional ByVal htmlId As String = "", Optional ByVal htmlMethod As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFormStart3")
            '
            'If Not (true) Then Exit Function
            '
            Dim Ptr As Integer
            Dim MethodName As String
            Dim ActionQS As String
            Dim iMethod As String
            Dim ActionParts() As String
            Dim Action As String
            Dim QSParts() As String
            Dim QSNameValues() As String
            Dim QSName As String
            Dim QSValue As String
            Dim RefreshHiddens As String
            '
            MethodName = "main_GetFormStart3"
            '
            If ActionQueryString Is Nothing Then
                ActionQS = cpCore.doc.refreshQueryString
            Else
                ActionQS = ActionQueryString
            End If
            iMethod = genericController.vbLCase(htmlMethod)
            If iMethod = "" Then
                iMethod = "post"
            End If
            RefreshHiddens = ""
            Action = cpCore.webServer.serverFormActionURL
            '
            If (ActionQS <> "") Then
                If (iMethod <> "main_Get") Then
                    '
                    ' non-main_Get, put Action QS on end of Action
                    '
                    Action = Action & "?" & ActionQS
                Else
                    '
                    ' main_Get method, build hiddens for actionQS
                    '
                    QSParts = Split(ActionQS, "&")
                    For Ptr = 0 To UBound(QSParts)
                        QSNameValues = Split(QSParts(Ptr), "=")
                        If UBound(QSNameValues) = 0 Then
                            QSName = genericController.DecodeResponseVariable(QSNameValues(0))
                        Else
                            QSName = genericController.DecodeResponseVariable(QSNameValues(0))
                            QSValue = genericController.DecodeResponseVariable(QSNameValues(1))
                            RefreshHiddens = RefreshHiddens & cr & "<input type=""hidden"" name=""" & encodeHTML(QSName) & """ value=""" & encodeHTML(QSValue) & """>"
                        End If
                    Next
                End If
            End If
            '
            html_GetFormStart = "" _
                & cr & "<form name=""" & htmlName & """ id=""" & htmlId & """ action=""" & Action & """ method=""" & iMethod & """ style=""display: inline;"" >" _
                & RefreshHiddens _
                & ""
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' ----- Ends an HTML form
        '========================================================================
        '
        Public Function html_GetFormEnd() As String
            '
            html_GetFormEnd = "</form>"
            '
        End Function
        '
        '
        '
        Public Function html_GetFormInputText(ByVal TagName As String, Optional ByVal DefaultValue As String = "", Optional ByVal Height As String = "", Optional ByVal Width As String = "", Optional ByVal Id As String = "", Optional ByVal PasswordField As Boolean = False) As String
            html_GetFormInputText = html_GetFormInputText2(genericController.encodeText(TagName), genericController.encodeText(DefaultValue), genericController.EncodeInteger(Height), genericController.EncodeInteger(Width), genericController.encodeText(Id), PasswordField, False)
        End Function
        '
        '
        '
        Public Function html_GetFormInputText2(ByVal htmlName As String, Optional ByVal DefaultValue As String = "", Optional ByVal Height As Integer = -1, Optional ByVal Width As Integer = -1, Optional ByVal HtmlId As String = "", Optional ByVal PasswordField As Boolean = False, Optional ByVal Disabled As Boolean = False, Optional ByVal HtmlClass As String = "") As String
            On Error GoTo ErrorTrap
            '
            Dim iDefaultValue As String
            Dim iWidth As Integer
            Dim iHeight As Integer
            Dim TagID As String
            Dim TagDisabled As String = String.Empty
            '
            If True Then
                TagID = ""
                '
                iDefaultValue = encodeHTML(DefaultValue)
                If HtmlId <> "" Then
                    TagID = TagID & " id=""" & genericController.encodeEmptyText(HtmlId, "") & """"
                End If
                '
                If HtmlClass <> "" Then
                    TagID = TagID & " class=""" & HtmlClass & """"
                End If
                '
                iWidth = Width
                If (iWidth <= 0) Then
                    iWidth = cpCore.siteProperties.defaultFormInputWidth
                End If
                '
                iHeight = Height
                If (iHeight <= 0) Then
                    iHeight = cpCore.siteProperties.defaultFormInputTextHeight
                End If
                '
                If Disabled Then
                    TagDisabled = " disabled=""disabled"""
                End If
                '
                If PasswordField Then
                    html_GetFormInputText2 = "<input TYPE=""password"" NAME=""" & htmlName & """ SIZE=""" & iWidth & """ VALUE=""" & iDefaultValue & """" & TagID & TagDisabled & ">"
                ElseIf (iHeight = 1) And (InStr(1, iDefaultValue, """") = 0) Then
                    html_GetFormInputText2 = "<input TYPE=""Text"" NAME=""" & htmlName & """ SIZE=""" & iWidth.ToString & """ VALUE=""" & iDefaultValue & """" & TagID & TagDisabled & ">"
                Else
                    html_GetFormInputText2 = "<textarea NAME=""" & htmlName & """ ROWS=""" & iHeight.ToString & """ COLS=""" & iWidth.ToString & """" & TagID & TagDisabled & ">" & iDefaultValue & "</TEXTAREA>"
                End If
                cpCore.doc.formInputTextCnt = cpCore.doc.formInputTextCnt + 1
            End If
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetFormInputText2")
        End Function
        '
        '========================================================================
        ' ----- main_Get an HTML Form text input (or text area)
        '========================================================================
        '
        Public Function html_GetFormInputTextExpandable(ByVal TagName As String, Optional ByVal Value As String = "", Optional ByVal Rows As Integer = 0, Optional ByVal styleWidth As String = "100%", Optional ByVal Id As String = "", Optional ByVal PasswordField As Boolean = False) As String
            If Rows = 0 Then
                Rows = cpCore.siteProperties.defaultFormInputTextHeight
            End If
            html_GetFormInputTextExpandable = html_GetFormInputTextExpandable2(TagName, Value, Rows, styleWidth, Id, PasswordField, False, "")
        End Function
        '
        '========================================================================
        ' ----- main_Get an HTML Form text input (or text area)
        '   added disabled case
        '========================================================================
        '
        Public Function html_GetFormInputTextExpandable2(ByVal TagName As String, Optional ByVal Value As String = "", Optional ByVal Rows As Integer = 0, Optional ByVal styleWidth As String = "100%", Optional ByVal Id As String = "", Optional ByVal PasswordField As Boolean = False, Optional ByVal Disabled As Boolean = False, Optional ByVal HtmlClass As String = "") As String
            On Error GoTo ErrorTrap : Dim Tn As String : Tn = "cpCoreClass.GetFormInputTextExpandable2" ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            'If Not (true) Then Exit Function
            '
            Dim AttrDisabled As String = String.Empty
            Dim Value_Local As String
            Dim StyleWidth_Local As String
            Dim Rows_Local As Integer
            Dim IDRoot As String
            Dim EditorClosed As String
            Dim EditorOpened As String
            '
            Value_Local = encodeHTML(Value)
            IDRoot = Id
            If IDRoot = "" Then
                IDRoot = "TextArea" & cpCore.doc.formInputTextCnt
            End If
            '
            StyleWidth_Local = styleWidth
            If StyleWidth_Local = "" Then
                StyleWidth_Local = "100%"
            End If
            '
            Rows_Local = Rows
            If Rows_Local = 0 Then
                '
                ' need a default for this -- it should be different from a text, it should be for a textarea -- bnecause it is used differently
                '
                'Rows_Local = app.SiteProperty_DefaultFormInputTextHeight
                If Rows_Local = 0 Then
                    Rows_Local = 10
                End If
            End If
            If Disabled Then
                AttrDisabled = " disabled=""disabled"""
            End If
            '
            EditorClosed = "" _
                & cr & "<div class=""ccTextAreaHead"" ID=""" & IDRoot & "Head"">" _
                & cr2 & "<a href=""#"" onClick=""OpenTextArea('" & IDRoot & "');return false""><img src=""/ccLib/images/OpenUpRev1313.gif"" width=13 height=13 border=0>&nbsp;Full Screen</a>" _
                & cr & "</div>" _
                & cr & "<div class=""ccTextArea"">" _
                & cr2 & "<textarea ID=""" & IDRoot & """ NAME=""" & TagName & """ ROWS=""" & Rows_Local & """ Style=""width:" & StyleWidth_Local & ";""" & AttrDisabled & " onkeydown=""return cj.encodeTextAreaKey(this, event);"">" & Value_Local & "</TEXTAREA>" _
                & cr & "</div>" _
                & ""
            '
            EditorOpened = "" _
                & cr & "<div class=""ccTextAreaHeCursorTypeEnum.ADOPENed"" style=""display:none;"" ID=""" & IDRoot & "HeCursorTypeEnum.ADOPENed"">" _
                & cr & "<a href=""#"" onClick=""CloseTextArea('" & IDRoot & "');return false""><img src=""/ccLib/images/OpenDownRev1313.gif"" width=13 height=13 border=0>&nbsp;Full Screen</a>" _
                & cr2 & "</div>" _
                & cr & "<textarea class=""ccTextAreaOpened"" style=""display:none;"" ID=""" & IDRoot & "Opened"" NAME=""" & IDRoot & "Opened""" & AttrDisabled & " onkeydown=""return cj.encodeTextAreaKey(this, event);""></TEXTAREA>"
            '
            html_GetFormInputTextExpandable2 = "" _
                & "<div class=""" & HtmlClass & """>" _
                & genericController.htmlIndent(EditorClosed) _
                & genericController.htmlIndent(EditorOpened) _
                & "</div>"
            cpCore.doc.formInputTextCnt = cpCore.doc.formInputTextCnt + 1
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            cpCore.handleException(New Exception("Unexpected exception"))
            '
        End Function
        '
        '
        '
        Public Function html_GetFormInputDate(ByVal TagName As String, Optional ByVal DefaultValue As String = "", Optional ByVal Width As String = "", Optional ByVal Id As String = "") As String
            Dim result As String = String.Empty
            Try
                Dim HeadJS As String
                Dim DateString As String = String.Empty
                Dim DateValue As Date
                Dim iDefaultValue As String
                Dim iWidth As Integer
                Dim MethodName As String
                Dim iTagName As String
                Dim TagID As String
                Dim CalendarObjName As String
                Dim AnchorName As String
                '
                MethodName = "main_GetFormInputDate"
                '
                iTagName = genericController.encodeText(TagName)
                iDefaultValue = genericController.encodeEmptyText(DefaultValue, "")
                If (iDefaultValue = "0") Or (iDefaultValue = "12:00:00 AM") Then
                    iDefaultValue = ""
                Else
                    iDefaultValue = encodeHTML(iDefaultValue)
                End If
                If genericController.encodeEmptyText(Id, "") <> "" Then
                    TagID = " ID=""" & genericController.encodeEmptyText(Id, "") & """"
                End If
                '
                iWidth = genericController.encodeEmptyInteger(Width, 20)
                If iWidth = 0 Then
                    iWidth = 20
                End If
                '
                CalendarObjName = "Cal" & cpCore.doc.inputDateCnt
                AnchorName = "ACal" & cpCore.doc.inputDateCnt

                If cpCore.doc.inputDateCnt = 0 Then
                    HeadJS = "" _
                    & vbCrLf & "<SCRIPT LANGUAGE=""JavaScript"" SRC=""/ccLib/mktree/CalendarPopup.js""></SCRIPT>" _
                    & vbCrLf & "<SCRIPT LANGUAGE=""JavaScript"">" _
                    & vbCrLf & "var cal = new CalendarPopup();" _
                    & vbCrLf & "cal.showNavigationDropdowns();" _
                    & vbCrLf & "</SCRIPT>"
                    Call addJavaScriptLinkHead("/ccLib/mktree/CalendarPopup.js", "Calendar Popup")
                    Call addHeadJavascriptCode("var cal=new CalendarPopup();cal.showNavigationDropdowns();", "Calendar Popup")
                End If

                If IsDate(iDefaultValue) Then
                    DateValue = genericController.EncodeDate(iDefaultValue)
                    If Month(DateValue) < 10 Then
                        DateString = DateString & "0"
                    End If
                    DateString = DateString & Month(DateValue) & "/"
                    If Day(DateValue) < 10 Then
                        DateString = DateString & "0"
                    End If
                    DateString = DateString & Day(DateValue) & "/" & Year(DateValue)
                End If


                result = result _
                & vbCrLf & "<input TYPE=""text"" NAME=""" & iTagName & """ ID=""" & iTagName & """ VALUE=""" & iDefaultValue & """  SIZE=""" & iWidth & """>" _
                & vbCrLf & "<a HREF=""#"" Onclick = ""cal.select(document.getElementById('" & iTagName & "'),'" & AnchorName & "','MM/dd/yyyy','" & DateString & "'); return false;"" NAME=""" & AnchorName & """ ID=""" & AnchorName & """><img title=""Select a date"" alt=""Select a date"" src=""/ccLib/images/table.jpg"" width=12 height=10 border=0></A>" _
                & vbCrLf & ""

                cpCore.doc.inputDateCnt = cpCore.doc.inputDateCnt + 1
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        ' ----- main_Get an HTML Form file upload input
        '========================================================================
        '
        Public Function html_GetFormInputFile2(ByVal TagName As String, Optional ByVal htmlId As String = "", Optional ByVal HtmlClass As String = "") As String
            '
            html_GetFormInputFile2 = "<input TYPE=""file"" name=""" & TagName & """ id=""" & htmlId & """ class=""" & HtmlClass & """>"
            '
        End Function
        '
        ' ----- main_Get an HTML Form file upload input
        '
        Public Function html_GetFormInputFile(ByVal TagName As String, Optional ByVal htmlId As String = "") As String
            '
            html_GetFormInputFile = html_GetFormInputFile2(TagName, htmlId)
            '
        End Function
        '
        '========================================================================
        ' ----- main_Get an HTML Form input
        '========================================================================
        '
        Public Function html_GetFormInputRadioBox(ByVal TagName As String, ByVal TagValue As String, ByVal CurrentValue As String, Optional ByVal htmlId As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFormInputRadioBox")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim iTagName As String
            Dim iTagValue As String
            Dim iCurrentValue As String
            Dim ihtmlId As String
            Dim TagID As String = String.Empty
            '
            iTagName = genericController.encodeText(TagName)
            iTagValue = genericController.encodeText(TagValue)
            iCurrentValue = genericController.encodeText(CurrentValue)
            ihtmlId = genericController.encodeEmptyText(htmlId, "")
            If ihtmlId <> "" Then
                TagID = " ID=""" & ihtmlId & """"
            End If
            '
            MethodName = "main_GetFormInputRadioBox"
            '
            If iTagValue = iCurrentValue Then
                html_GetFormInputRadioBox = "<input TYPE=""Radio"" NAME=""" & iTagName & """ VALUE=""" & iTagValue & """ checked" & TagID & ">"
            Else
                html_GetFormInputRadioBox = "<input TYPE=""Radio"" NAME=""" & iTagName & """ VALUE=""" & iTagValue & """" & TagID & ">"
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        '   Legacy
        '========================================================================
        '
        Public Function html_GetFormInputCheckBox(ByVal TagName As String, Optional ByVal DefaultValue As String = "", Optional ByVal htmlId As String = "") As String
            html_GetFormInputCheckBox = html_GetFormInputCheckBox2(genericController.encodeText(TagName), genericController.EncodeBoolean(DefaultValue), genericController.encodeText(htmlId))
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function html_GetFormInputCheckBox2(ByVal TagName As String, Optional ByVal DefaultValue As Boolean = False, Optional ByVal HtmlId As String = "", Optional ByVal Disabled As Boolean = False, Optional ByVal HtmlClass As String = "") As String
            On Error GoTo ErrorTrap
            '
            html_GetFormInputCheckBox2 = "<input TYPE=""CheckBox"" NAME=""" & TagName & """ VALUE=""1"""
            If HtmlId <> "" Then
                html_GetFormInputCheckBox2 = html_GetFormInputCheckBox2 & " id=""" & HtmlId & """"
            End If
            If HtmlClass <> "" Then
                html_GetFormInputCheckBox2 = html_GetFormInputCheckBox2 & " class=""" & HtmlClass & """"
            End If
            If DefaultValue Then
                html_GetFormInputCheckBox2 = html_GetFormInputCheckBox2 & " checked=""checked"""
            End If
            If Disabled Then
                html_GetFormInputCheckBox2 = html_GetFormInputCheckBox2 & " disabled=""disabled"""
            End If
            html_GetFormInputCheckBox2 = html_GetFormInputCheckBox2 & ">"
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetFormInputCheckBox2")
        End Function
        '
        '========================================================================
        '   Create a List of Checkboxes based on a contentname and a list of IDs that should be checked
        '
        '   For instance, list out a checklist of all public groups, with the ones checked that this member belongs to
        '       PrimaryContentName = "People"
        '       PrimaryRecordID = MemberID
        '       SecondaryContentName = "Groups"
        '       SecondaryContentSelectCriteria = "ccGroups.PublicJoin<>0"
        '       RulesContentName = "Member Rules"
        '       RulesPrimaryFieldName = "MemberID"
        '       RulesSecondaryFieldName = "GroupID"
        '========================================================================
        '
        Public Function html_GetFormInputCheckListByIDList(ByVal TagName As String, ByVal SecondaryContentName As String, ByVal CheckedIDList As String, Optional ByVal CaptionFieldName As String = "", Optional ByVal readOnlyField As Boolean = False) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFormInputCheckListByIDList")
            '
            'If Not (true) Then Exit Function
            '
            Dim SQL As String
            Dim CS As Integer
            Dim main_MemberShipCount As Integer
            Dim main_MemberShipSize As Integer
            Dim main_MemberShipPointer As Integer
            Dim SectionName As String
            Dim GroupCount As Integer
            Dim main_MemberShip() As Integer
            Dim SecondaryTablename As String
            Dim SecondaryContentID As Integer
            Dim rulesTablename As String
            Dim Result As String = String.Empty
            Dim MethodName As String
            Dim iCaptionFieldName As String
            Dim GroupName As String
            Dim GroupCaption As String
            Dim CanSeeHiddenFields As Boolean
            Dim SecondaryCDef As cdefModel
            Dim ContentIDList As String = String.Empty
            Dim Found As Boolean
            Dim RecordID As Integer
            Dim SingularPrefix As String
            '
            MethodName = "main_GetFormInputCheckListByIDList"
            '
            iCaptionFieldName = genericController.encodeEmptyText(CaptionFieldName, "name")
            '
            ' ----- Gather all the SecondaryContent that associates to the PrimaryContent
            '
            SecondaryCDef = cpCore.metaData.getCdef(SecondaryContentName)
            SecondaryTablename = SecondaryCDef.ContentTableName
            SecondaryContentID = SecondaryCDef.Id
            SecondaryCDef.childIdList(cpCore).Add(SecondaryContentID)
            SingularPrefix = genericController.GetSingular(SecondaryContentName) & "&nbsp;"
            '
            ' ----- Gather all the records, sorted by ContentName
            '
            SQL = "SELECT " & SecondaryTablename & ".ID AS ID, ccContent.Name AS SectionName, " & SecondaryTablename & "." & iCaptionFieldName & " AS GroupCaption, " & SecondaryTablename & ".name AS GroupName, " & SecondaryTablename & ".SortOrder" _
            & " FROM " & SecondaryTablename & " LEFT JOIN ccContent ON " & SecondaryTablename & ".ContentControlID = ccContent.ID" _
            & " Where (" & SecondaryTablename & ".Active<>" & SQLFalse & ")" _
            & " And (ccContent.Active<>" & SQLFalse & ")" _
            & " And (" & SecondaryTablename & ".ContentControlID IN (" & ContentIDList & "))"
            SQL &= "" _
                & " GROUP BY " & SecondaryTablename & ".ID, ccContent.Name, " & SecondaryTablename & "." & iCaptionFieldName & ", " & SecondaryTablename & ".name, " & SecondaryTablename & ".SortOrder" _
                & " ORDER BY ccContent.Name, " & SecondaryTablename & "." & iCaptionFieldName
            CS = cpCore.db.cs_openSql(SQL)
            If cpCore.db.cs_ok(CS) Then
                SectionName = ""
                GroupCount = 0
                CanSeeHiddenFields = cpCore.authContext.isAuthenticatedDeveloper(cpCore)
                Do While cpCore.db.cs_ok(CS)
                    GroupName = cpCore.db.cs_getText(CS, "GroupName")
                    If (Mid(GroupName, 1, 1) <> "_") Or CanSeeHiddenFields Then
                        RecordID = cpCore.db.cs_getInteger(CS, "ID")
                        GroupCaption = cpCore.db.cs_getText(CS, "GroupCaption")
                        If GroupCaption = "" Then
                            GroupCaption = GroupName
                        End If
                        If GroupCaption = "" Then
                            GroupCaption = SingularPrefix & RecordID
                        End If
                        If GroupCount <> 0 Then
                            ' leave this between checkboxes - it is searched in the admin page
                            Result = Result & "<br >" & vbCrLf
                        End If
                        If genericController.IsInDelimitedString(CheckedIDList, CStr(RecordID), ",") Then
                            Found = True
                        Else
                            Found = False
                        End If
                        ' must leave the first hidden with the value in this form - it is searched in the admin pge
                        Result = Result & "<input type=hidden name=""" & TagName & "." & GroupCount & ".ID"" value=" & RecordID & ">"
                        If readOnlyField And Not Found Then
                            Result = Result & "<input type=checkbox disabled>"
                        ElseIf readOnlyField Then
                            Result = Result & "<input type=checkbox disabled checked>"
                            Result = Result & "<input type=""hidden"" name=""" & TagName & "." & GroupCount & ".ID"" value=" & RecordID & ">"
                        ElseIf Found Then
                            Result = Result & "<input type=checkbox name=""" & TagName & "." & GroupCount & """ checked>"
                        Else
                            Result = Result & "<input type=checkbox name=""" & TagName & "." & GroupCount & """>"
                        End If
                        Result = Result & SpanClassAdminNormal & GroupCaption
                        GroupCount = GroupCount + 1
                    End If
                    cpCore.db.cs_goNext(CS)
                Loop
                Result = Result & "<input type=""hidden"" name=""" & TagName & ".RowCount"" value=""" & GroupCount & """>" & vbCrLf
            End If
            cpCore.db.cs_Close(CS)
            html_GetFormInputCheckListByIDList = Result
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18(MethodName)
            '
        End Function
        '
        ' -----
        '
        Public Function html_GetFormInputCS(ByVal CSPointer As Integer, ByVal ContentName As String, ByVal FieldName As String, Optional ByVal Height As Integer = 1, Optional ByVal Width As Integer = 40, Optional ByVal htmlId As String = "") As String
            Dim returnResult As String = String.Empty
            Try
                Dim IsEmptyList As Boolean
                Dim Stream As String
                Dim MethodName As String
                Dim FieldCaption As String
                Dim FieldValueVariant As String = String.Empty
                Dim FieldValueText As String
                Dim FieldValueInteger As Integer
                Dim fieldTypeId As Integer
                Dim FieldReadOnly As Boolean
                Dim FieldPassword As Boolean
                Dim fieldFound As Boolean
                Dim FieldLookupContentID As Integer
                Dim FieldMemberSelectGroupID As Integer
                Dim FieldLookupContentName As String
                Dim Contentdefinition As cdefModel
                Dim FieldHTMLContent As Boolean
                Dim CSLookup As Integer
                Dim FieldLookupList As String = String.Empty
                '
                MethodName = "main_GetFormInputCS"
                '
                Stream = ""
                If True Then
                    fieldFound = False
                    Contentdefinition = cpCore.metaData.getCdef(ContentName)
                    For Each keyValuePair As KeyValuePair(Of String, CDefFieldModel) In Contentdefinition.fields
                        Dim field As CDefFieldModel = keyValuePair.Value
                        With field
                            If genericController.vbUCase(.nameLc) = genericController.vbUCase(FieldName) Then
                                FieldValueVariant = .defaultValue
                                fieldTypeId = .fieldTypeId
                                FieldReadOnly = .ReadOnly
                                FieldCaption = .caption
                                FieldPassword = .Password
                                FieldHTMLContent = .htmlContent
                                FieldLookupContentID = .lookupContentID
                                FieldLookupList = .lookupList
                                FieldMemberSelectGroupID = .MemberSelectGroupID
                                fieldFound = True
                            End If
                        End With
                    Next
                    If Not fieldFound Then
                        cpCore.handleException(New Exception("Field [" & FieldName & "] was not found in Content Definition [" & ContentName & "]"))
                    Else
                        '
                        ' main_Get the current value if the record was found
                        '
                        If cpCore.db.cs_ok(CSPointer) Then
                            FieldValueVariant = cpCore.db.cs_getField(CSPointer, FieldName)
                        End If
                        '
                        If FieldPassword Then
                            '
                            ' Handle Password Fields
                            '
                            FieldValueText = genericController.encodeText(FieldValueVariant)
                            returnResult = html_GetFormInputText2(FieldName, FieldValueText, Height, Width, , True)
                        Else
                            '
                            ' Non Password field by fieldtype
                            '
                            Select Case fieldTypeId
                            '
                            '
                            '
                                Case FieldTypeIdHTML
                                    FieldValueText = genericController.encodeText(FieldValueVariant)
                                    If FieldReadOnly Then
                                        returnResult = FieldValueText
                                    Else
                                        returnResult = html_GetFormInputHTML(FieldName, FieldValueText, , Width.ToString)
                                    End If
                                '
                                ' html private files, read from privatefiles and use html editor
                                '
                                Case FieldTypeIdFileHTMLPrivate
                                    FieldValueText = genericController.encodeText(FieldValueVariant)
                                    If FieldValueText <> "" Then
                                        FieldValueText = cpCore.privateFiles.readFile(FieldValueText)
                                    End If
                                    If FieldReadOnly Then
                                        returnResult = FieldValueText
                                    Else
                                        'Height = encodeEmptyInteger(Height, 4)
                                        returnResult = html_GetFormInputHTML(FieldName, FieldValueText, , Width.ToString)
                                    End If
                                '
                                ' text private files, read from privatefiles and use text editor
                                '
                                Case FieldTypeIdFileTextPrivate
                                    FieldValueText = genericController.encodeText(FieldValueVariant)
                                    If FieldValueText <> "" Then
                                        FieldValueText = cpCore.privateFiles.readFile(FieldValueText)
                                    End If
                                    If FieldReadOnly Then
                                        returnResult = FieldValueText
                                    Else
                                        'Height = encodeEmptyInteger(Height, 4)
                                        returnResult = html_GetFormInputText2(FieldName, FieldValueText, Height, Width)
                                    End If
                                '
                                ' text public files, read from cpcore.cdnFiles and use text editor
                                '
                                Case FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript
                                    FieldValueText = genericController.encodeText(FieldValueVariant)
                                    If FieldValueText <> "" Then
                                        FieldValueText = cpCore.cdnFiles.readFile(FieldValueText)
                                    End If
                                    If FieldReadOnly Then
                                        returnResult = FieldValueText
                                    Else
                                        'Height = encodeEmptyInteger(Height, 4)
                                        returnResult = html_GetFormInputText2(FieldName, FieldValueText, Height, Width)
                                    End If
                                '
                                '
                                '
                                Case FieldTypeIdBoolean
                                    If FieldReadOnly Then
                                        returnResult = genericController.encodeText(genericController.EncodeBoolean(FieldValueVariant))
                                    Else
                                        returnResult = html_GetFormInputCheckBox2(FieldName, genericController.EncodeBoolean(FieldValueVariant))
                                    End If
                                '
                                '
                                '
                                Case FieldTypeIdAutoIdIncrement
                                    returnResult = genericController.encodeText(genericController.EncodeNumber(FieldValueVariant))
                                '
                                '
                                '
                                Case FieldTypeIdFloat, FieldTypeIdCurrency, FieldTypeIdInteger
                                    FieldValueVariant = genericController.EncodeNumber(FieldValueVariant).ToString()
                                    If FieldReadOnly Then
                                        returnResult = genericController.encodeText(FieldValueVariant)
                                    Else
                                        returnResult = html_GetFormInputText2(FieldName, genericController.encodeText(FieldValueVariant), Height, Width)
                                    End If
                                '
                                '
                                '
                                Case FieldTypeIdFile
                                    FieldValueText = genericController.encodeText(FieldValueVariant)
                                    If FieldReadOnly Then
                                        returnResult = FieldValueText
                                    Else
                                        returnResult = FieldValueText & "<BR >change: " & html_GetFormInputFile(FieldName, genericController.encodeText(FieldValueVariant))
                                    End If
                                '
                                '
                                '
                                Case FieldTypeIdFileImage
                                    FieldValueText = genericController.encodeText(FieldValueVariant)
                                    If FieldReadOnly Then
                                        returnResult = FieldValueText
                                    Else
                                        returnResult = "<img src=""" & genericController.getCdnFileLink(cpCore, FieldValueText) & """><BR >change: " & html_GetFormInputFile(FieldName, genericController.encodeText(FieldValueVariant))
                                    End If
                                '
                                '
                                '
                                Case FieldTypeIdLookup
                                    FieldValueInteger = genericController.EncodeInteger(FieldValueVariant)
                                    FieldLookupContentName = cpCore.metaData.getContentNameByID(FieldLookupContentID)
                                    If FieldLookupContentName <> "" Then
                                        '
                                        ' Lookup into Content
                                        '
                                        If FieldReadOnly Then
                                            CSPointer = cpCore.db.cs_open2(FieldLookupContentName, FieldValueInteger)
                                            If cpCore.db.cs_ok(CSLookup) Then
                                                returnResult = csController.main_cs_getEncodedField(cpCore, CSLookup, "name")
                                            End If
                                            Call cpCore.db.cs_Close(CSLookup)
                                        Else
                                            returnResult = main_GetFormInputSelect2(FieldName, FieldValueInteger, FieldLookupContentName, "", "", "", IsEmptyList)
                                        End If
                                    ElseIf FieldLookupList <> "" Then
                                        '
                                        ' Lookup into LookupList
                                        '
                                        returnResult = getInputSelectList2(FieldName, FieldValueInteger, FieldLookupList, "", "")
                                    Else
                                        '
                                        ' Just call it text
                                        '
                                        returnResult = html_GetFormInputText2(FieldName, CStr(FieldValueInteger), Height, Width)
                                    End If
                                '
                                '
                                '
                                Case FieldTypeIdMemberSelect
                                    FieldValueInteger = genericController.EncodeInteger(FieldValueVariant)
                                    returnResult = getInputMemberSelect(FieldName, FieldValueInteger, FieldMemberSelectGroupID)
                                    '
                                    '
                                    '
                                Case Else
                                    FieldValueText = genericController.encodeText(FieldValueVariant)
                                    If FieldReadOnly Then
                                        returnResult = FieldValueText
                                    Else
                                        If FieldHTMLContent Then
                                            returnResult = html_GetFormInputHTML3(FieldName, FieldValueText, CStr(Height), CStr(Width), FieldReadOnly, False)
                                            'main_GetFormInputCS = main_GetFormInputActiveContent(fieldname, FieldValueText, height, width)
                                        Else
                                            returnResult = html_GetFormInputText2(FieldName, FieldValueText, Height, Width)
                                        End If
                                    End If
                            End Select
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ' ----- Print an HTML Form Button element named BUTTON
        '========================================================================
        '
        Public Function html_GetFormButton(ByVal ButtonLabel As String, Optional ByVal Name As String = "", Optional ByVal htmlId As String = "", Optional ByVal OnClick As String = "") As String
            html_GetFormButton = html_GetFormButton2(ButtonLabel, Name, htmlId, OnClick, False)
        End Function
        '
        '========================================================================
        ' ----- Print an HTML Form Button element named BUTTON
        '========================================================================
        '
        Public Function html_GetFormButton2(ByVal ButtonLabel As String, Optional ByVal Name As String = "button", Optional ByVal htmlId As String = "", Optional ByVal OnClick As String = "", Optional ByVal Disabled As Boolean = False) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFormButton2")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim iOnClick As String
            Dim TagID As String
            Dim s As String
            '
            MethodName = "main_GetFormButton2"
            '
            s = "<input TYPE=""SUBMIT""" _
                & " NAME=""" & genericController.encodeEmptyText(Name, "button") & """" _
                & " VALUE=""" & genericController.encodeText(ButtonLabel) & """" _
                & " OnClick=""" & genericController.encodeEmptyText(OnClick, "") & """" _
                & " ID=""" & genericController.encodeEmptyText(htmlId, "") & """"
            If Disabled Then
                s = s & " disabled=""disabled"""
            End If
            html_GetFormButton2 = s & ">"
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' main_Gets a value in a hidden form field
        '   Handles name and value encoding
        '========================================================================
        '
        Public Function html_GetFormInputHidden(ByVal TagName As String, ByVal TagValue As String, Optional ByVal htmlId As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFormInputHidden")
            '
            'If Not (true) Then Exit Function
            '
            Dim iTagValue As String
            Dim ihtmlId As String
            Dim s As String
            '
            s = cr & "<input type=""hidden"" NAME=""" & encodeHTML(genericController.encodeText(TagName)) & """"
            '
            iTagValue = encodeHTML(genericController.encodeText(TagValue))
            If iTagValue <> "" Then
                s = s & " VALUE=""" & iTagValue & """"
            End If
            '
            ihtmlId = genericController.encodeText(htmlId)
            If ihtmlId <> "" Then
                s = s & " ID=""" & encodeHTML(ihtmlId) & """"
            End If
            '
            s = s & ">"
            '
            html_GetFormInputHidden = s
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetFormInputHidden")
        End Function
        '
        Public Function html_GetFormInputHidden(ByVal TagName As String, ByVal TagValue As Boolean, Optional ByVal htmlId As String = "") As String
            Return html_GetFormInputHidden(TagName, TagValue.ToString, htmlId)
        End Function
        '
        Public Function html_GetFormInputHidden(ByVal TagName As String, ByVal TagValue As Integer, Optional ByVal htmlId As String = "") As String
            Return html_GetFormInputHidden(TagName, TagValue.ToString, htmlId)
        End Function
        '
        ' Popup a separate window with the contents of a file
        '
        Public Function html_GetWindowOpenJScript(ByVal URI As String, Optional ByVal WindowWidth As String = "", Optional ByVal WindowHeight As String = "", Optional ByVal WindowScrollBars As String = "", Optional ByVal WindowResizable As Boolean = True, Optional ByVal WindowName As String = "_blank") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetWindowOpenJScript")
            '
            'If Not (true) Then Exit Function
            '
            Dim Delimiter As String
            Dim MethodName As String
            '
            html_GetWindowOpenJScript = ""
            WindowName = genericController.encodeEmptyText(WindowName, "_blank")
            '
            MethodName = "main_GetWindowOpenJScript()"
            '
            ' Added addl options from huhcorp.com sample
            '
            html_GetWindowOpenJScript = html_GetWindowOpenJScript & "window.open('" & URI & "', '" & WindowName & "'"
            html_GetWindowOpenJScript = html_GetWindowOpenJScript & ",'menubar=no,toolbar=no,location=no,status=no"
            Delimiter = ","
            If Not genericController.isMissing(WindowWidth) Then
                If WindowWidth <> "" Then
                    html_GetWindowOpenJScript = html_GetWindowOpenJScript & Delimiter & "width=" & WindowWidth
                    Delimiter = ","
                End If
            End If
            If Not genericController.isMissing(WindowHeight) Then
                If WindowHeight <> "" Then
                    html_GetWindowOpenJScript = html_GetWindowOpenJScript & Delimiter & "height=" & WindowHeight
                    Delimiter = ","
                End If
            End If
            If Not genericController.isMissing(WindowScrollBars) Then
                If WindowScrollBars <> "" Then
                    html_GetWindowOpenJScript = html_GetWindowOpenJScript & Delimiter & "scrollbars=" & WindowScrollBars
                    Delimiter = ","
                End If
            End If
            If WindowResizable Then
                html_GetWindowOpenJScript = html_GetWindowOpenJScript & Delimiter & "resizable"
                Delimiter = ","
            End If
            html_GetWindowOpenJScript = html_GetWindowOpenJScript & "')"
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18(MethodName)
            '
        End Function
        '
        ' Popup a separate window with the contents of a file
        '
        Public Function html_GetWindowDialogJScript(ByVal URI As String, Optional ByVal WindowWidth As String = "", Optional ByVal WindowHeight As String = "", Optional ByVal WindowScrollBars As Boolean = False, Optional ByVal WindowResizable As Boolean = False, Optional ByVal WindowName As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetWindowDialogJScript")
            '
            'If Not (true) Then Exit Function
            '
            Dim Delimiter As String
            Dim iWindowName As String
            Dim MethodName As String
            '
            iWindowName = genericController.encodeEmptyText(WindowName, "_blank")
            '
            MethodName = "main_GetWindowDialogJScript()"
            '
            ' Added addl options from huhcorp.com sample
            '
            html_GetWindowDialogJScript = ""
            html_GetWindowDialogJScript = html_GetWindowDialogJScript & "showModalDialog('" & URI & "', '" & iWindowName & "'"
            html_GetWindowDialogJScript = html_GetWindowDialogJScript & ",'status:false"
            If Not genericController.isMissing(WindowWidth) Then
                If WindowWidth <> "" Then
                    html_GetWindowDialogJScript = html_GetWindowDialogJScript & ";dialogWidth:" & WindowWidth & "px"
                End If
            End If
            If Not genericController.isMissing(WindowHeight) Then
                If WindowHeight <> "" Then
                    html_GetWindowDialogJScript = html_GetWindowDialogJScript & ";dialogHeight:" & WindowHeight & "px"
                End If
            End If
            If WindowScrollBars Then
                html_GetWindowDialogJScript = html_GetWindowDialogJScript & ";scroll:yes"
            End If
            If WindowResizable Then
                html_GetWindowDialogJScript = html_GetWindowDialogJScript & ";resizable:yes"
            End If
            html_GetWindowDialogJScript = html_GetWindowDialogJScript & "')"
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18(MethodName)
            '
        End Function
        '
        '=========================================================================================
        '
        '=========================================================================================
        '
        Public Sub html_AddEvent(ByVal HtmlId As String, ByVal DOMEvent As String, ByVal Javascript As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("AddEvent")
            '
            Dim JSCodeAsString As String
            '
            JSCodeAsString = Javascript
            JSCodeAsString = genericController.vbReplace(JSCodeAsString, "'", "'+""'""+'")
            JSCodeAsString = genericController.vbReplace(JSCodeAsString, vbCrLf, "\n")
            JSCodeAsString = genericController.vbReplace(JSCodeAsString, vbCr, "\n")
            JSCodeAsString = genericController.vbReplace(JSCodeAsString, vbLf, "\n")
            JSCodeAsString = "'" & JSCodeAsString & "'"
            '
            Call addOnLoadJavascript("" _
                & "cj.addListener(" _
                    & "document.getElementById('" & HtmlId & "')" _
                    & ",'" & DOMEvent & "'" _
                    & ",function(){eval(" & JSCodeAsString & ")}" _
                & ")", "")
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("AddEvent")
        End Sub
        '
        '
        '
        Public Function html_GetFormInputField(ByVal ContentName As String, ByVal FieldName As String, Optional ByVal htmlName As String = "", Optional ByVal HtmlValue As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "", Optional ByVal HtmlStyle As String = "", Optional ByVal ManyToManySourceRecordID As Integer = 0) As String
            Dim result As String = String.Empty
            Try
                Dim IgnoreBoolean As Boolean
                Dim LookupContentName As String
                Dim fieldType As Integer
                Dim InputName As String
                Dim GroupID As Integer
                Dim CDef As cdefModel
                Dim MTMContent0 As String
                Dim MTMContent1 As String
                Dim MTMRuleContent As String
                Dim MTMRuleField0 As String
                Dim MTMRuleField1 As String
                '
                InputName = htmlName
                If InputName = "" Then
                    InputName = FieldName
                End If
                '
                fieldType = genericController.EncodeInteger(cpCore.metaData.GetContentFieldProperty(ContentName, FieldName, "type"))
                Select Case fieldType
                    Case FieldTypeIdBoolean
                        '
                        '
                        '
                        result = html_GetFormInputCheckBox2(InputName, genericController.EncodeBoolean(HtmlValue) = True, HtmlId, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdFileCSS
                        '
                        '
                        '
                        result = html_GetFormInputTextExpandable2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdCurrency
                        '
                        '
                        '
                        result = html_GetFormInputText2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdDate
                        '
                        '
                        '
                        result = html_GetFormInputDate(InputName, HtmlValue, , HtmlId)
                        If HtmlClass <> "" Then
                            result = genericController.vbReplace(result, ">", " class=""" & HtmlClass & """>")
                        End If
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdFile
                        '
                        '
                        '
                        If HtmlValue = "" Then
                            result = html_GetFormInputFile2(InputName, HtmlId, HtmlClass)
                        Else

                            Dim FieldValuefilename As String = ""
                            Dim FieldValuePath As String = ""
                            cpCore.privateFiles.splitPathFilename(HtmlValue, FieldValuePath, FieldValuefilename)
                            result = result & "<a href=""http://" & genericController.EncodeURL(cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, HtmlValue)) & """ target=""_blank"">" & SpanClassAdminSmall & "[" & FieldValuefilename & "]</A>"
                            result = result & "&nbsp;&nbsp;&nbsp;Delete:&nbsp;" & html_GetFormInputCheckBox2(InputName & ".Delete", False)
                            result = result & "&nbsp;&nbsp;&nbsp;Change:&nbsp;" & html_GetFormInputFile2(InputName, HtmlId, HtmlClass)
                        End If
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdFloat
                        '
                        '
                        '
                        result = html_GetFormInputText2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdFileImage
                        '
                        '
                        '
                        If HtmlValue = "" Then
                            result = html_GetFormInputFile2(InputName, HtmlId, HtmlClass)
                        Else
                            Dim FieldValuefilename As String = ""
                            Dim FieldValuePath As String = ""
                            cpCore.privateFiles.splitPathFilename(HtmlValue, FieldValuePath, FieldValuefilename)
                            result = result & "<a href=""http://" & genericController.EncodeURL(cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, HtmlValue)) & """ target=""_blank"">" & SpanClassAdminSmall & "[" & FieldValuefilename & "]</A>"
                            result = result & "&nbsp;&nbsp;&nbsp;Delete:&nbsp;" & html_GetFormInputCheckBox2(InputName & ".Delete", False)
                            result = result & "&nbsp;&nbsp;&nbsp;Change:&nbsp;" & html_GetFormInputFile2(InputName, HtmlId, HtmlClass)
                        End If
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdInteger
                        '
                        '
                        '
                        result = html_GetFormInputText2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdFileJavascript
                        '
                        '
                        '
                        result = html_GetFormInputTextExpandable2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdLink
                        '
                        '
                        '
                        result = html_GetFormInputText2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdLookup
                        '
                        '
                        '
                        CDef = cpCore.metaData.getCdef(ContentName)
                        LookupContentName = ""
                        With CDef
                            For Each keyValuePair As KeyValuePair(Of String, CDefFieldModel) In CDef.fields
                                Dim field As CDefFieldModel = keyValuePair.Value
                                With field
                                    If genericController.vbUCase(.nameLc) = genericController.vbUCase(FieldName) Then
                                        If .lookupContentID <> 0 Then
                                            LookupContentName = genericController.encodeText(cpCore.metaData.getContentNameByID(.lookupContentID))
                                        End If
                                        If LookupContentName <> "" Then
                                            result = main_GetFormInputSelect2(InputName, genericController.EncodeInteger(HtmlValue), LookupContentName, "", "Select One", HtmlId, IgnoreBoolean, HtmlClass)
                                        ElseIf .lookupList <> "" Then
                                            result = getInputSelectList2(InputName, genericController.EncodeInteger(HtmlValue), .lookupList, "Select One", HtmlId, HtmlClass)
                                        End If
                                        If HtmlStyle <> "" Then
                                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                                        End If
                                        Exit For
                                    End If
                                End With
                            Next
                        End With
                    Case FieldTypeIdManyToMany
                        '
                        '
                        '
                        CDef = cpCore.metaData.getCdef(ContentName)
                        With CDef.fields(FieldName.ToLower())
                            MTMContent0 = cpCore.metaData.getContentNameByID(.contentId)
                            MTMContent1 = cpCore.metaData.getContentNameByID(.manyToManyContentID)
                            MTMRuleContent = cpCore.metaData.getContentNameByID(.manyToManyRuleContentID)
                            MTMRuleField0 = .ManyToManyRulePrimaryField
                            MTMRuleField1 = .ManyToManyRuleSecondaryField
                        End With
                        result = getInputCheckList(InputName, MTMContent0, ManyToManySourceRecordID, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, , , False)
                        'result = getInputCheckListCategories(InputName, MTMContent0, ManyToManySourceRecordID, MTMContent1, MTMRuleContent, MTMRuleField0, MTMRuleField1, , , False, MTMContent1, HtmlValue)
                    Case FieldTypeIdMemberSelect
                        '
                        '
                        '
                        GroupID = genericController.EncodeInteger(cpCore.metaData.GetContentFieldProperty(ContentName, FieldName, "memberselectgroupid"))
                        result = getInputMemberSelect(InputName, genericController.EncodeInteger(HtmlValue), GroupID, , , HtmlId)
                        If HtmlClass <> "" Then
                            result = genericController.vbReplace(result, ">", " class=""" & HtmlClass & """>")
                        End If
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdResourceLink
                        '
                        '
                        '
                        result = html_GetFormInputText2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdText
                        '
                        '
                        '
                        result = html_GetFormInputText2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdLongText, FieldTypeIdFileTextPrivate
                        '
                        '
                        '
                        result = html_GetFormInputTextExpandable2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdFileXML
                        '
                        '
                        '
                        result = html_GetFormInputTextExpandable2(InputName, HtmlValue, , , HtmlId, False, False, HtmlClass)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                    Case FieldTypeIdHTML, FieldTypeIdFileHTMLPrivate
                        '
                        '
                        '
                        result = html_GetFormInputHTML(InputName, HtmlValue)
                        If HtmlStyle <> "" Then
                            result = genericController.vbReplace(result, ">", " style=""" & HtmlStyle & """>")
                        End If
                        If HtmlClass <> "" Then
                            result = genericController.vbReplace(result, ">", " class=""" & HtmlClass & """>")
                        End If
                    Case Else
                        '
                        ' unsupported field type
                        '
                End Select
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        ''
        ''   renamed to AllowDebugging
        ''
        'Public ReadOnly Property visitProperty_AllowVerboseReporting() As Boolean
        '    Get
        '        Return visitProperty.getBoolean("AllowDebugging")
        '    End Get
        'End Property
        '        '
        '        '
        '        '
        '        Public Function main_parseJSON(ByVal Source As String) As Object
        '            On Error GoTo ErrorTrap 'Const Tn = "parseJSON" : ''Dim th as integer : th = profileLogMethodEnter(Tn)    '
        '            '
        '            main_parseJSON = common_jsonDeserialize(Source)
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            cpCore.handleExceptionAndContinue(New Exception("Unexpected exception"))
        '            '
        '        End Function
        ''
        ''
        ''
        'Public Function main_GetStyleSheet2(ByVal ContentType As csv_contentTypeEnum, Optional ByVal templateId As Integer = 0, Optional ByVal EmailID As Integer = 0) As String
        '    main_GetStyleSheet2 = html_getStyleSheet2(ContentType, templateId, EmailID)
        'End Function
        '
        '
        '
        Public Function main_GetEditorAddonListJSON(ByVal ContentType As csv_contentTypeEnum) As String
            Dim result As String = String.Empty
            Try
                Dim AddonName As String
                Dim LastAddonName As String = String.Empty
                Dim CSAddons As Integer
                Dim DefaultAddonOption_String As String
                Dim UseAjaxDefaultAddonOptions As Boolean
                Dim PtrTest As Integer
                Dim s As String
                Dim IconWidth As Integer
                Dim IconHeight As Integer
                Dim IconSprites As Integer
                Dim IsInline As Boolean
                Dim AddonGuid As String
                Dim IconIDControlString As String
                Dim IconImg As String
                Dim AddonContentName As String
                Dim ObjectProgramID2 As String
                Dim LoopPtr As Integer
                Dim FieldCaption As String
                Dim SelectList As String
                Dim IconFilename As String
                Dim FieldName As String
                Dim ArgumentList As String
                Dim Index As keyPtrController
                Dim Items() As String
                Dim ItemsSize As Integer
                Dim ItemsCnt As Integer
                Dim ItemsPtr As Integer
                Dim Criteria As String
                Dim CSLists As Integer
                Dim FieldList As String
                Dim cacheKey As String
                '
                ' can not save this because there are multiple main_versions
                '
                cacheKey = "editorAddonList:" & ContentType
                result = cpCore.docProperties.getText(cacheKey)
                If (result = "") Then
                    '
                    ' ----- AC Tags, Would like to replace these with Add-ons eventually
                    '
                    ItemsSize = 100
                    ReDim Items(100)
                    ItemsCnt = 0
                    Index = New keyPtrController
                    'Set main_cmc = main_cs_getv()
                    '
                    ' AC StartBlockText
                    '
                    IconIDControlString = "AC," & ACTypeAggregateFunction & ",0,Block Text,"
                    IconImg = genericController.GetAddonIconImg(cpCore.siteProperties.adminURL, 0, 0, 0, True, IconIDControlString, "", cpCore.serverConfig.appConfig.cdnFilesNetprefix, "Text Block Start", "Block text to all except selected groups starting at this point", "", 0)
                    IconImg = genericController.EncodeJavascript(IconImg)
                    Items(ItemsCnt) = "['Block Text','" & IconImg & "']"
                    Call Index.setPtr("Block Text", ItemsCnt)
                    ItemsCnt = ItemsCnt + 1
                    '
                    ' AC EndBlockText
                    '
                    IconIDControlString = "AC," & ACTypeAggregateFunction & ",0,Block Text End,"
                    IconImg = genericController.GetAddonIconImg(cpCore.siteProperties.adminURL, 0, 0, 0, True, IconIDControlString, "", cpCore.serverConfig.appConfig.cdnFilesNetprefix, "Text Block End", "End of text block", "", 0)
                    IconImg = genericController.EncodeJavascript(IconImg)
                    Items(ItemsCnt) = "['Block Text End','" & IconImg & "']"
                    Call Index.setPtr("Block Text", ItemsCnt)
                    ItemsCnt = ItemsCnt + 1
                    '
                    If (ContentType = csv_contentTypeEnum.contentTypeEmail) Or (ContentType = csv_contentTypeEnum.contentTypeEmailTemplate) Then
                        '
                        ' ----- Email Only AC tags
                        '
                        ' Editing Email Body or Templates - Since Email can not process Add-ons, it main_Gets the legacy AC tags for now
                        '
                        ' Personalization Tag
                        '
                        FieldList = cpCore.metaData.GetContentProperty("people", "SelectFieldList")
                        FieldList = genericController.vbReplace(FieldList, ",", "|")
                        IconIDControlString = "AC,PERSONALIZATION,0,Personalization,field=[" & FieldList & "]"
                        IconImg = genericController.GetAddonIconImg(cpCore.siteProperties.adminURL, 0, 0, 0, True, IconIDControlString, "", cpCore.serverConfig.appConfig.cdnFilesNetprefix, "Any Personalization Field", "Renders as any Personalization Field", "", 0)
                        IconImg = genericController.EncodeJavascript(IconImg)
                        Items(ItemsCnt) = "['Personalization','" & IconImg & "']"
                        Call Index.setPtr("Personalization", ItemsCnt)
                        ItemsCnt = ItemsCnt + 1
                        '
                        If (ContentType = csv_contentTypeEnum.contentTypeEmailTemplate) Then
                            '
                            ' Editing Email Templates
                            '   This is a special case
                            '   Email content processing can not process add-ons, and PageContentBox and TextBox are needed
                            '   So I added the old AC Tag into the menu for this case
                            '   Need a more consistant solution later
                            '
                            IconIDControlString = "AC," & ACTypeTemplateContent & ",0,Template Content,"
                            IconImg = genericController.GetAddonIconImg(cpCore.siteProperties.adminURL, 52, 64, 0, False, IconIDControlString, "/ccLib/images/ACTemplateContentIcon.gif", cpCore.serverConfig.appConfig.cdnFilesNetprefix, "Content Box", "Renders as the content for a template", "", 0)
                            IconImg = genericController.EncodeJavascript(IconImg)
                            Items(ItemsCnt) = "['Content Box','" & IconImg & "']"
                            'Items(ItemsCnt) = "['Template Content','<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Content"" id=""AC," & ACTypeTemplateContent & ",0,Template Content,"" src=""/ccLib/images/ACTemplateContentIcon.gif"" WIDTH=52 HEIGHT=64>']"
                            Call Index.setPtr("Content Box", ItemsCnt)
                            ItemsCnt = ItemsCnt + 1
                            '
                            IconIDControlString = "AC," & ACTypeTemplateText & ",0,Template Text,Name=Default"
                            IconImg = genericController.GetAddonIconImg(cpCore.siteProperties.adminURL, 52, 52, 0, False, IconIDControlString, "/ccLib/images/ACTemplateTextIcon.gif", cpCore.serverConfig.appConfig.cdnFilesNetprefix, "Template Text", "Renders as a template text block", "", 0)
                            IconImg = genericController.EncodeJavascript(IconImg)
                            Items(ItemsCnt) = "['Template Text','" & IconImg & "']"
                            'Items(ItemsCnt) = "['Template Text','<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Text"" id=""AC," & ACTypeTemplateText & ",0,Template Text,Name=Default"" src=""/ccLib/images/ACTemplateTextIcon.gif"" WIDTH=52 HEIGHT=52>']"
                            Call Index.setPtr("Template Text", ItemsCnt)
                            ItemsCnt = ItemsCnt + 1
                        End If
                    Else
                        '
                        ' ----- Web Only AC Tags
                        '
                        ' Watch Lists
                        '
                        CSLists = cpCore.db.cs_open("Content Watch Lists", , "Name,ID", , , , , "Name,ID", 20, 1)
                        If cpCore.db.cs_ok(CSLists) Then
                            Do While cpCore.db.cs_ok(CSLists)
                                FieldName = Trim(cpCore.db.cs_getText(CSLists, "name"))
                                If FieldName <> "" Then
                                    FieldCaption = "Watch List [" & FieldName & "]"
                                    IconIDControlString = "AC,WATCHLIST,0," & FieldName & ",ListName=" & FieldName & "&SortField=[DateAdded|Link|LinkLabel|Clicks|WhatsNewDateExpires]&SortDirection=Z-A[A-Z|Z-A]"
                                    IconImg = genericController.GetAddonIconImg(cpCore.siteProperties.adminURL, 0, 0, 0, True, IconIDControlString, "", cpCore.serverConfig.appConfig.cdnFilesNetprefix, FieldCaption, "Rendered as the " & FieldCaption, "", 0)
                                    IconImg = genericController.EncodeJavascript(IconImg)
                                    FieldCaption = genericController.EncodeJavascript(FieldCaption)
                                    Items(ItemsCnt) = "['" & FieldCaption & "','" & IconImg & "']"
                                    'Items(ItemsCnt) = "['" & FieldCaption & "','<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the " & FieldCaption & """ id=""AC,WATCHLIST,0," & FieldName & ",ListName=" & FieldName & "&SortField=[DateAdded|Link|LinkLabel|Clicks|WhatsNewDateExpires]&SortDirection=Z-A[A-Z|Z-A]"" src=""/ccLib/images/ACWatchList.GIF"">']"
                                    Call Index.setPtr(FieldCaption, ItemsCnt)
                                    ItemsCnt = ItemsCnt + 1
                                    If ItemsCnt >= ItemsSize Then
                                        ItemsSize = ItemsSize + 100
                                        ReDim Preserve Items(ItemsSize)
                                    End If
                                End If
                                cpCore.db.cs_goNext(CSLists)
                            Loop
                        End If
                        Call cpCore.db.cs_Close(CSLists)
                    End If
                    '
                    ' ----- Add-ons (AC Aggregate Functions)
                    '
                    If (False) And (ContentType = csv_contentTypeEnum.contentTypeEmail) Then
                        '
                        ' Email did not support add-ons
                        '
                    Else
                        '
                        ' Either non-email or > 4.0.325
                        '
                        Criteria = "(1=1)"
                        If (ContentType = csv_contentTypeEnum.contentTypeEmail) Then
                            '
                            ' select only addons with email placement (dont need to check main_version bc if email, must be >4.0.325
                            '
                            Criteria = Criteria & "and(email<>0)"
                        Else
                            If True Then
                                If (ContentType = csv_contentTypeEnum.contentTypeWeb) Then
                                    '
                                    ' Non Templates
                                    '
                                    Criteria = Criteria & "and(content<>0)"
                                Else
                                    '
                                    ' Templates
                                    '
                                    Criteria = Criteria & "and(template<>0)"
                                End If
                            End If
                        End If
                        AddonContentName = cnAddons
                        SelectList = "Name,Link,ID,ArgumentList,ObjectProgramID,IconFilename,IconWidth,IconHeight,IconSprites,IsInline,ccguid"
                        CSAddons = cpCore.db.cs_open(AddonContentName, Criteria, "Name,ID", , , , , SelectList)
                        If cpCore.db.cs_ok(CSAddons) Then
                            Do While cpCore.db.cs_ok(CSAddons)
                                AddonGuid = cpCore.db.cs_getText(CSAddons, "ccguid")
                                ObjectProgramID2 = cpCore.db.cs_getText(CSAddons, "ObjectProgramID")
                                If (ContentType = csv_contentTypeEnum.contentTypeEmail) And (ObjectProgramID2 <> "") Then
                                    '
                                    ' Block activex addons from email
                                    '
                                    ObjectProgramID2 = ObjectProgramID2
                                Else
                                    AddonName = Trim(cpCore.db.cs_get(CSAddons, "name"))
                                    If AddonName <> "" And (AddonName <> LastAddonName) Then
                                        '
                                        ' Icon (fieldtyperesourcelink)
                                        '
                                        IsInline = cpCore.db.cs_getBoolean(CSAddons, "IsInline")
                                        IconFilename = cpCore.db.cs_get(CSAddons, "Iconfilename")
                                        If IconFilename = "" Then
                                            IconWidth = 0
                                            IconHeight = 0
                                            IconSprites = 0
                                        Else
                                            IconWidth = cpCore.db.cs_getInteger(CSAddons, "IconWidth")
                                            IconHeight = cpCore.db.cs_getInteger(CSAddons, "IconHeight")
                                            IconSprites = cpCore.db.cs_getInteger(CSAddons, "IconSprites")
                                        End If
                                        '
                                        ' Calculate DefaultAddonOption_String
                                        '
                                        UseAjaxDefaultAddonOptions = True
                                        If UseAjaxDefaultAddonOptions Then
                                            DefaultAddonOption_String = ""
                                        Else
                                            ArgumentList = Trim(cpCore.db.cs_get(CSAddons, "ArgumentList"))
                                            DefaultAddonOption_String = addonController.main_GetDefaultAddonOption_String(cpCore, ArgumentList, AddonGuid, IsInline)
                                            DefaultAddonOption_String = main_encodeHTML(DefaultAddonOption_String)
                                        End If
                                        '
                                        ' Changes necessary to support commas in AddonName and OptionString
                                        '   Remove commas in Field Name
                                        '   Then in Javascript, when spliting on comma, anything past position 4, put back onto 4
                                        '
                                        LastAddonName = AddonName
                                        IconIDControlString = "AC,AGGREGATEFUNCTION,0," & AddonName & "," & DefaultAddonOption_String & "," & AddonGuid
                                        IconImg = genericController.GetAddonIconImg(cpCore.siteProperties.adminURL, IconWidth, IconHeight, IconSprites, IsInline, IconIDControlString, IconFilename, cpCore.serverConfig.appConfig.cdnFilesNetprefix, AddonName, "Rendered as the Add-on [" & AddonName & "]", "", 0)
                                        Items(ItemsCnt) = "['" & genericController.EncodeJavascript(AddonName) & "','" & genericController.EncodeJavascript(IconImg) & "']"
                                        Call Index.setPtr(AddonName, ItemsCnt)
                                        ItemsCnt = ItemsCnt + 1
                                        If ItemsCnt >= ItemsSize Then
                                            ItemsSize = ItemsSize + 100
                                            ReDim Preserve Items(ItemsSize)
                                        End If
                                    End If
                                End If
                                cpCore.db.cs_goNext(CSAddons)
                            Loop
                        End If
                        Call cpCore.db.cs_Close(CSAddons)
                    End If
                    '
                    ' Build output sting in alphabetical order by name
                    '
                    s = ""
                    ItemsPtr = Index.getFirstPtr
                    Do While ItemsPtr >= 0 And LoopPtr < ItemsCnt
                        s = s & vbCrLf & "," & Items(ItemsPtr)
                        PtrTest = Index.getNextPtr
                        If PtrTest < 0 Then
                            Exit Do
                        Else
                            ItemsPtr = PtrTest
                        End If
                        LoopPtr = LoopPtr + 1
                    Loop
                    If s <> "" Then
                        s = "[" & Mid(s, 4) & "]"
                    End If
                    '
                    result = s
                    Call cpCore.docProperties.setProperty(cacheKey, result, False)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        ''
        ''========================================================================
        ''   deprecated - see csv_EncodeActiveContent_Internal
        ''========================================================================
        ''
        'Public Function html_EncodeActiveContent4(ByVal Source As String, ByVal PeopleID As Integer, ByVal ContextContentName As String, ByVal ContextRecordID As Integer, ByVal ContextContactPeopleID As Integer, ByVal AddLinkEID As Boolean, ByVal EncodeCachableTags As Boolean, ByVal EncodeImages As Boolean, ByVal EncodeEditIcons As Boolean, ByVal EncodeNonCachableTags As Boolean, ByVal AddAnchorQuery As String, ByVal ProtocolHostString As String, ByVal IsEmailContent As Boolean, ByVal AdminURL As String) As String
        '    html_EncodeActiveContent4 = html_EncodeActiveContent_Internal(Source, PeopleID, ContextContentName, ContextRecordID, ContextContactPeopleID, AddLinkEID, EncodeCachableTags, EncodeImages, EncodeEditIcons, EncodeNonCachableTags, AddAnchorQuery, ProtocolHostString, IsEmailContent, AdminURL, cpCore.authContext.isAuthenticated)
        'End Function
        ''
        ''========================================================================
        ''   see csv_EncodeActiveContent_Internal
        ''========================================================================
        ''
        'Public Function html_EncodeActiveContent5(ByVal Source As String, ByVal PeopleID As Integer, ByVal ContextContentName As String, ByVal ContextRecordID As Integer, ByVal ContextContactPeopleID As Integer, ByVal AddLinkEID As Boolean, ByVal EncodeCachableTags As Boolean, ByVal EncodeImages As Boolean, ByVal EncodeEditIcons As Boolean, ByVal EncodeNonCachableTags As Boolean, ByVal AddAnchorQuery As String, ByVal ProtocolHostString As String, ByVal IsEmailContent As Boolean, ByVal AdminURL As String, ByVal personalizationIsAuthenticated As Boolean, ByVal Context As CPUtilsBaseClass.addonContext) As String
        '    html_EncodeActiveContent5 = html_EncodeActiveContent_Internal(Source, PeopleID, ContextContentName, ContextRecordID, ContextContactPeopleID, AddLinkEID, EncodeCachableTags, EncodeImages, EncodeEditIcons, EncodeNonCachableTags, AddAnchorQuery, ProtocolHostString, IsEmailContent, AdminURL, cpCore.authContext.isAuthenticated, Context)
        'End Function
        '
        '========================================================================
        '   encode (execute) all {% -- %} commands
        '========================================================================
        '
        Public Function html_executeContentCommands(ByVal nothingObject As Object, ByVal Source As String, ByVal Context As CPUtilsBaseClass.addonContext, ByVal personalizationPeopleId As Integer, ByVal personalizationIsAuthenticated As Boolean, ByRef Return_ErrorMessage As String) As String
            Dim returnValue As String = ""
            Try
                Dim LoopPtr As Integer
                Dim contentCmd As New contentCmdController(cpCore)
                '
                returnValue = Source
                LoopPtr = 0
                Do While (LoopPtr < 10) And ((InStr(1, returnValue, contentReplaceEscapeStart) <> 0))
                    returnValue = contentCmd.ExecuteCmd(returnValue, Context, personalizationPeopleId, personalizationIsAuthenticated)
                    LoopPtr = LoopPtr + 1
                Loop
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnValue
        End Function
        '
        '========================================================================
        ' csv_EncodeActiveContent_Internal
        '       ...
        '       AllowLinkEID    Boolean, if yes, the EID=000... string is added to all links in the content
        '                       Use this for email so links will include the members longin.
        '
        '       Some Active elements can not be replaced here because they incorporate content from  the wbeclient.
        '       For instance the Aggregate Function Objects. These elements create
        '       <!-- FPO1 --> placeholders in the content, and commented instructions, one per line, at the top of the content
        '       Replacement instructions
        '       <!-- Replace FPO1,AFObject,ObjectName,OptionString -->
        '           Aggregate Function Object, ProgramID=ObjectName, Optionstring=Optionstring
        '       <!-- Replace FPO1,AFObject,ObjectName,OptionString -->
        '           Aggregate Function Object, ProgramID=ObjectName, Optionstring=Optionstring
        '
        ' Tag descriptions:
        '
        '   primary methods
        '
        '   <Ac Type="Date">
        '   <Ac Type="Member" Field="Name">
        '   <Ac Type="Organization" Field="Name">
        '   <Ac Type="Visitor" Field="Name">
        '   <Ac Type="Visit" Field="Name">
        '   <Ac Type="Contact" Member="PeopleID">
        '       displays a For More info block of copy
        '   <Ac Type="Feedback" Member="PeopleID">
        '       displays a feedback note block
        '   <Ac Type="ChildList" Name="ChildListName">
        '       displays a list of child blocks that reference this CHildList Element
        '   <Ac Type="Language" Name="All|English|Spanish|etc.">
        '       blocks content to next language tag to eveyone without this PeopleLanguage
        '   <Ac Type="Image" Record="" Width="" Height="" Alt="" Align="">
        '   <AC Type="Download" Record="" Alt="">
        '       renders as an anchored download icon, with the alt tag
        '       the rendered anchor points back to the root/index, which increments the resource's download count
        '
        '   During Editing, AC tags are converted (Encoded) to EditIcons
        '       these are image tags with AC information stored in the ID attribute
        '       except AC-Image, which are converted into the actual image for editing
        '       during the edit save, the EditIcons are converted (Decoded) back
        '
        '   Remarks
        '
        '   First <Member.FieldName> encountered opens the Members Table, etc.
        '       ( does <OpenTable name="Member" Tablename="ccMembers" ID=(current PeopleID)> )
        '   The copy is divided into Blocks, starting at every tag and running to the next tag.
        '   BlockTag()  The tag without the braces found
        '   BlockCopy() The copy following the tag up to the next tag
        '   BlockLabel()    the string identifier for the block
        '   BlockCount  the total blocks in the message
        '   BlockPointer    the current block being examined
        '========================================================================
        '
        Public Function html_EncodeActiveContent_Internal(ByVal Source As String, ByVal personalizationPeopleId As Integer, ByVal ContextContentName As String, ByVal ContextRecordID As Integer, ByVal moreInfoPeopleId As Integer, ByVal AddLinkEID As Boolean, ByVal EncodeCachableTags As Boolean, ByVal EncodeImages As Boolean, ByVal EncodeEditIcons As Boolean, ByVal EncodeNonCachableTags As Boolean, ByVal AddAnchorQuery As String, ByVal ProtocolHostString As String, ByVal IsEmailContent As Boolean, ByVal AdminURL As String, ByVal personalizationIsAuthenticated As Boolean, Optional ByVal context As CPUtilsBaseClass.addonContext = CPUtilsBaseClass.addonContext.ContextPage) As String
            Dim result As String = ""
            Try
                Dim ACGuid As String
                Dim AddonFound As Boolean
                Dim ACNameCaption As String
                Dim GroupIDList As String
                Dim IDControlString As String
                Dim IconIDControlString As String
                Dim Criteria As String
                Dim AddonContentName As String
                Dim SelectList As String = ""
                Dim IconWidth As Integer
                Dim IconHeight As Integer
                Dim IconSprites As Integer
                Dim AddonIsInline As Boolean
                Dim IconAlt As String = ""
                Dim IconTitle As String = ""
                Dim IconImg As String
                Dim TextName As String
                Dim ListName As String
                Dim SrcOptionSelector As String
                Dim ResultOptionSelector As String
                Dim SrcOptionList As String
                Dim Pos As Integer
                Dim REsultOptionValue As String
                Dim SrcOptionValueSelector As String
                Dim InstanceOptionValue As String
                Dim ResultOptionListHTMLEncoded As String
                Dim UCaseACName As String
                Dim IconFilename As String
                Dim FieldName As String
                Dim Ptr As Integer
                Dim ElementPointer As Integer
                Dim ListCount As Integer
                Dim CSVisitor As Integer
                Dim CSVisit As Integer
                Dim CSVisitorSet As Boolean
                Dim CSVisitSet As Boolean
                Dim ElementTag As String
                Dim ACType As String
                Dim ACField As String
                Dim ACName As String = ""
                Dim Copy As String
                Dim KmaHTML As htmlParserController
                Dim AttributeCount As Integer
                Dim AttributePointer As Integer
                Dim Name As String
                Dim Value As String
                Dim CS As Integer
                Dim ACAttrRecordID As Integer
                Dim ACAttrWidth As Integer
                Dim ACAttrHeight As Integer
                Dim ACAttrAlt As String
                Dim ACAttrBorder As Integer
                Dim ACAttrLoop As Integer
                Dim ACAttrVSpace As Integer
                Dim ACAttrHSpace As Integer
                Dim Filename As String = ""
                Dim ACAttrAlign As String
                Dim ProcessAnchorTags As Boolean
                Dim ProcessACTags As Boolean
                Dim ACLanguageName As String
                Dim Stream As New stringBuilderLegacyController
                Dim AnchorQuery As String = ""
                Dim CSOrganization As Integer
                Dim CSOrganizationSet As Boolean
                Dim CSPeople As Integer
                Dim CSPeopleSet As Boolean
                Dim CSlanguage As Integer
                Dim PeopleLanguageSet As Boolean
                Dim PeopleLanguage As String = ""
                Dim UcasePeopleLanguage As String
                Dim serverFilePath As String = ""
                Dim ReplaceInstructions As String = String.Empty
                Dim Link As String
                Dim NotUsedID As Integer
                Dim addonOptionString As String
                Dim AddonOptionStringHTMLEncoded As String
                Dim SrcOptions() As String
                Dim SrcOptionName As String
                Dim FormCount As Integer
                Dim FormInputCount As Integer
                Dim ACInstanceID As String
                Dim PosStart As Integer
                Dim PosEnd As Integer
                Dim AllowGroups As String
                Dim workingContent As String
                Dim NewName As String
                '
                workingContent = Source
                '
                ' Fixup Anchor Query (additional AddonOptionString pairs to add to the end)
                '
                If AddLinkEID And (personalizationPeopleId <> 0) Then
                    AnchorQuery = AnchorQuery & "&EID=" & cpCore.security.encodeToken(genericController.EncodeInteger(personalizationPeopleId), Now())
                End If
                '
                If AddAnchorQuery <> "" Then
                    AnchorQuery = AnchorQuery & "&" & AddAnchorQuery
                End If
                '
                If AnchorQuery <> "" Then
                    AnchorQuery = Mid(AnchorQuery, 2)
                End If
                '
                ' ----- xml contensive process instruction
                '
                'TemplateBodyContent
                'Pos = genericController.vbInstr(1, TemplateBodyContent, "<?contensive", vbTextCompare)
                'If Pos > 0 Then
                '    '
                '    ' convert template body if provided - this is the content that replaces the content box addon
                '    '
                '    TemplateBodyContent = Mid(TemplateBodyContent, Pos)
                '    LayoutEngineOptionString = "data=" & encodeNvaArgument(TemplateBodyContent)
                '    TemplateBodyContent = csv_ExecuteActiveX("aoPrimitives.StructuredDataClass", "Structured Data Engine", nothing, LayoutEngineOptionString, "data=(structured data)", LayoutErrorMessage)
                'End If
                Pos = genericController.vbInstr(1, workingContent, "<?contensive", vbTextCompare)
                If Pos > 0 Then
                    Throw New ApplicationException("Structured xml data commands are no longer supported")
                    ''
                    '' convert content if provided
                    ''
                    'workingContent = Mid(workingContent, Pos)
                    'LayoutEngineOptionString = "data=" & encodeNvaArgument(workingContent)
                    'Dim structuredData As New core_primitivesStructuredDataClass(Me)
                    'workingContent = structuredData.execute()
                    'workingContent = csv_ExecuteActiveX("aoPrimitives.StructuredDataClass", "Structured Data Engine", LayoutEngineOptionString, "data=(structured data)", LayoutErrorMessage)
                End If
                '
                ' Special Case
                ' Convert <!-- STARTGROUPACCESS 10,11,12 --> format to <AC type=GROUPACCESS AllowGroups="10,11,12">
                ' Convert <!-- ENDGROUPACCESS --> format to <AC type=GROUPACCESSEND>
                '
                PosStart = genericController.vbInstr(1, workingContent, "<!-- STARTGROUPACCESS ", vbTextCompare)
                If PosStart > 0 Then
                    PosEnd = genericController.vbInstr(PosStart, workingContent, "-->")
                    If PosEnd > 0 Then
                        AllowGroups = Mid(workingContent, PosStart + 22, PosEnd - PosStart - 23)
                        workingContent = Mid(workingContent, 1, PosStart - 1) & "<AC type=""" & ACTypeAggregateFunction & """ name=""block text"" querystring=""allowgroups=" & AllowGroups & """>" & Mid(workingContent, PosEnd + 3)
                    End If
                End If
                '
                PosStart = genericController.vbInstr(1, workingContent, "<!-- ENDGROUPACCESS ", vbTextCompare)
                If PosStart > 0 Then
                    PosEnd = genericController.vbInstr(PosStart, workingContent, "-->")
                    If PosEnd > 0 Then
                        workingContent = Mid(workingContent, 1, PosStart - 1) & "<AC type=""" & ACTypeAggregateFunction & """ name=""block text end"" >" & Mid(workingContent, PosEnd + 3)
                    End If
                End If
                '
                ' ----- Special case -- if any of these are in the source, this is legacy. Convert them to icons,
                '       and they will be converted to AC tags when the icons are saved
                '
                If EncodeEditIcons Then
                    '
                    IconIDControlString = "AC," & ACTypeTemplateContent & "," & NotUsedID & "," & ACName & ","
                    IconImg = genericController.GetAddonIconImg(AdminURL, 52, 64, 0, False, IconIDControlString, "/ccLib/images/ACTemplateContentIcon.gif", serverFilePath, "Template Page Content", "Renders as [Template Page Content]", "", 0)
                    workingContent = genericController.vbReplace(workingContent, "{{content}}", IconImg, 1, 99, vbTextCompare)
                    'WorkingContent = genericController.vbReplace(WorkingContent, "{{content}}", "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Page Content"" id=""AC," & ACTypeTemplateContent & "," & NotUsedID & "," & ACName & ","" src=""/ccLib/images/ACTemplateContentIcon.gif"" WIDTH=52 HEIGHT=64>", 1, -1, vbTextCompare)
                    ''
                    '' replace all other {{...}}
                    ''
                    'LoopPtr = 0
                    'Pos = 1
                    'Do While Pos > 0 And LoopPtr < 100
                    '    Pos = genericController.vbInstr(Pos, workingContent, "{{" & ACTypeDynamicMenu, vbTextCompare)
                    '    If Pos > 0 Then
                    '        addonOptionString = ""
                    '        PosStart = Pos
                    '        If PosStart <> 0 Then
                    '            'PosStart = PosStart + 2 + Len(ACTypeDynamicMenu)
                    '            PosEnd = genericController.vbInstr(PosStart, workingContent, "}}", vbTextCompare)
                    '            If PosEnd <> 0 Then
                    '                Cmd = Mid(workingContent, PosStart + 2, PosEnd - PosStart - 2)
                    '                Pos = genericController.vbInstr(1, Cmd, "?")
                    '                If Pos <> 0 Then
                    '                    addonOptionString = genericController.decodeHtml(Mid(Cmd, Pos + 1))
                    '                End If
                    '                TextName = cpCore.csv_GetAddonOptionStringValue("menu", addonOptionString)
                    '                '
                    '                addonOptionString = "Menu=" & TextName & "[" & cpCore.csv_GetDynamicMenuACSelect() & "]&NewMenu="
                    '                AddonOptionStringHTMLEncoded = html_EncodeHTML("Menu=" & TextName & "[" & cpCore.csv_GetDynamicMenuACSelect() & "]&NewMenu=")
                    '                '
                    '                IconIDControlString = "AC," & ACTypeDynamicMenu & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded
                    '                IconImg = genericController.GetAddonIconImg(AdminURL, 52, 52, 0, False, IconIDControlString, "/ccLib/images/ACDynamicMenuIcon.gif", serverFilePath, "Dynamic Menu", "Renders as [Dynamic Menu]", "", 0)
                    '                workingContent = Mid(workingContent, 1, PosStart - 1) & IconImg & Mid(workingContent, PosEnd + 2)
                    '            End If
                    '        End If
                    '    End If
                    'Loop
                End If
                '
                ' Test early if this needs to run at all
                '
                ProcessACTags = ((EncodeCachableTags Or EncodeNonCachableTags Or EncodeImages Or EncodeEditIcons)) And (InStr(1, workingContent, "<AC ", vbTextCompare) <> 0)
                ProcessAnchorTags = (AnchorQuery <> "") And (InStr(1, workingContent, "<A ", vbTextCompare) <> 0)
                If (workingContent <> "") And (ProcessAnchorTags Or ProcessACTags) Then
                    '
                    ' ----- Load the Active Elements
                    '
                    KmaHTML = New htmlParserController(cpCore)
                    Call KmaHTML.Load(workingContent)
                    '
                    ' ----- Execute and output elements
                    '
                    ElementPointer = 0
                    If KmaHTML.ElementCount > 0 Then
                        ElementPointer = 0
                        workingContent = ""
                        serverFilePath = ProtocolHostString & "/" & cpCore.serverConfig.appConfig.name & "/files/"
                        Stream = New stringBuilderLegacyController
                        Do While ElementPointer < KmaHTML.ElementCount
                            Copy = KmaHTML.Text(ElementPointer)
                            If KmaHTML.IsTag(ElementPointer) Then
                                ElementTag = genericController.vbUCase(KmaHTML.TagName(ElementPointer))
                                ACName = KmaHTML.ElementAttribute(ElementPointer, "NAME")
                                UCaseACName = genericController.vbUCase(ACName)
                                Select Case ElementTag
                                    Case "FORM"
                                        '
                                        ' Form created in content
                                        ' EncodeEditIcons -> remove the
                                        '
                                        If EncodeNonCachableTags Then
                                            FormCount = FormCount + 1
                                            '
                                            ' 5/14/2009 - DM said it is OK to remove UserResponseForm Processing
                                            ' however, leave this one because it is needed to make current forms work.
                                            '
                                            If (InStr(1, Copy, "contensiveuserform=1", vbTextCompare) <> 0) Or (InStr(1, Copy, "contensiveuserform=""1""", vbTextCompare) <> 0) Then
                                                '
                                                ' if it has "contensiveuserform=1" in the form tag, remove it from the form and add the hidden that makes it work
                                                '
                                                Copy = genericController.vbReplace(Copy, "ContensiveUserForm=1", "", 1, 99, vbTextCompare)
                                                Copy = genericController.vbReplace(Copy, "ContensiveUserForm=""1""", "", 1, 99, vbTextCompare)
                                                If Not EncodeEditIcons Then
                                                    Copy = Copy & "<input type=hidden name=ContensiveUserForm value=1>"
                                                End If
                                            End If
                                        End If
                                    Case "INPUT"
                                        If EncodeNonCachableTags Then
                                            FormInputCount = FormInputCount + 1
                                        End If
                                    Case "A"
                                        If (AnchorQuery <> "") Then
                                            '
                                            ' ----- Add ?eid=0000 to all anchors back to the same site so emails
                                            '       can be sent that will automatically log the person in when they
                                            '       arrive.
                                            '
                                            AttributeCount = KmaHTML.ElementAttributeCount(ElementPointer)
                                            If AttributeCount > 0 Then
                                                Copy = "<A "
                                                For AttributePointer = 0 To AttributeCount - 1
                                                    Name = KmaHTML.ElementAttributeName(ElementPointer, AttributePointer)
                                                    Value = KmaHTML.ElementAttributeValue(ElementPointer, AttributePointer)
                                                    If genericController.vbUCase(Name) = "HREF" Then
                                                        Link = Value
                                                        Pos = genericController.vbInstr(1, Link, "://")
                                                        If Pos > 0 Then
                                                            Link = Mid(Link, Pos + 3)
                                                            Pos = genericController.vbInstr(1, Link, "/")
                                                            If Pos > 0 Then
                                                                Link = Left(Link, Pos - 1)
                                                            End If
                                                        End If
                                                        If (Link = "") Or (InStr(1, "," & cpCore.serverConfig.appConfig.domainList(0) & ",", "," & Link & ",", vbTextCompare) <> 0) Then
                                                            '
                                                            ' ----- link is for this site
                                                            '
                                                            If Right(Value, 1) = "?" Then
                                                                '
                                                                ' Ends in a questionmark, must be Dwayne (?)
                                                                '
                                                                Value = Value & AnchorQuery
                                                            ElseIf genericController.vbInstr(1, Value, "mailto:", vbTextCompare) <> 0 Then
                                                                '
                                                                ' catch mailto
                                                                '
                                                                'Value = Value & AnchorQuery
                                                            ElseIf genericController.vbInstr(1, Value, "?") = 0 Then
                                                                '
                                                                ' No questionmark there, add it
                                                                '
                                                                Value = Value & "?" & AnchorQuery
                                                            Else
                                                                '
                                                                ' Questionmark somewhere, add new value with amp;
                                                                '
                                                                Value = Value & "&" & AnchorQuery
                                                            End If
                                                            '    End If
                                                        End If
                                                    End If
                                                    Copy = Copy & " " & Name & "=""" & Value & """"
                                                Next
                                                Copy = Copy & ">"
                                            End If
                                        End If
                                    Case "AC"
                                        '
                                        ' ----- decode all AC tags
                                        '
                                        ListCount = 0
                                        ACType = KmaHTML.ElementAttribute(ElementPointer, "TYPE")
                                        ' if ACInstanceID=0, it can not create settings link in edit mode. ACInstanceID is added during edit save.
                                        ACInstanceID = KmaHTML.ElementAttribute(ElementPointer, "ACINSTANCEID")
                                        ACGuid = KmaHTML.ElementAttribute(ElementPointer, "GUID")
                                        Select Case genericController.vbUCase(ACType)
                                            Case ACTypeEnd
                                                '
                                                ' End Tag - Personalization
                                                '       This tag causes an end to the all tags, like Language
                                                '       It is removed by with EncodeEditIcons (on the way to the editor)
                                                '       It is added to the end of the content with Decode(activecontent)
                                                '
                                                If EncodeEditIcons Then
                                                    Copy = ""
                                                ElseIf EncodeNonCachableTags Then
                                                    Copy = "<!-- Language ANY -->"
                                                End If
                                            Case ACTypeDate
                                                '
                                                ' Date Tag
                                                '
                                                If EncodeEditIcons Then
                                                    IconIDControlString = "AC," & ACTypeDate
                                                    IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, True, IconIDControlString, "", serverFilePath, "Current Date", "Renders as [Current Date]", ACInstanceID, 0)
                                                    Copy = IconImg
                                                    'Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as the current date"" ID=""AC," & ACTypeDate & """ src=""/ccLib/images/ACDate.GIF"">"
                                                ElseIf EncodeNonCachableTags Then
                                                    Copy = DateTime.Now.ToString
                                                End If
                                            Case ACTypeMember, ACTypePersonalization
                                                '
                                                ' Member Tag works regardless of authentication
                                                ' cm must be sure not to reveal anything
                                                '
                                                ACField = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "FIELD"))
                                                If ACField = "" Then
                                                    ' compatibility for old personalization type
                                                    ACField = htmlController.csv_GetAddonOptionStringValue("FIELD", KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING"))
                                                End If
                                                FieldName = genericController.EncodeInitialCaps(ACField)
                                                If (FieldName = "") Then
                                                    FieldName = "Name"
                                                End If
                                                If EncodeEditIcons Then
                                                    Select Case genericController.vbUCase(FieldName)
                                                        Case "FIRSTNAME"
                                                            '
                                                            IconIDControlString = "AC," & ACType & "," & FieldName
                                                            IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, True, IconIDControlString, "", serverFilePath, "User's First Name", "Renders as [User's First Name]", ACInstanceID, 0)
                                                            Copy = IconImg
                                                        '
                                                        Case "LASTNAME"
                                                            '
                                                            IconIDControlString = "AC," & ACType & "," & FieldName
                                                            IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, True, IconIDControlString, "", serverFilePath, "User's Last Name", "Renders as [User's Last Name]", ACInstanceID, 0)
                                                            Copy = IconImg
                                                            '
                                                        Case Else
                                                            '
                                                            IconIDControlString = "AC," & ACType & "," & FieldName
                                                            IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, True, IconIDControlString, "", serverFilePath, "User's " & FieldName, "Renders as [User's " & FieldName & "]", ACInstanceID, 0)
                                                            Copy = IconImg
                                                            '
                                                    End Select
                                                ElseIf EncodeNonCachableTags Then
                                                    If personalizationPeopleId <> 0 Then
                                                        If genericController.vbUCase(FieldName) = "EID" Then
                                                            Copy = cpCore.security.encodeToken(personalizationPeopleId, Now())
                                                        Else
                                                            If Not CSPeopleSet Then
                                                                CSPeople = cpCore.db.cs_openContentRecord("People", personalizationPeopleId)
                                                                CSPeopleSet = True
                                                            End If
                                                            If cpCore.db.cs_ok(CSPeople) And cpCore.db.cs_isFieldSupported(CSPeople, FieldName) Then
                                                                Copy = cpCore.db.cs_getLookup(CSPeople, FieldName)
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            Case ACTypeChildList
                                                '
                                                ' Child List
                                                '
                                                ListName = genericController.encodeText((KmaHTML.ElementAttribute(ElementPointer, "name")))

                                                If EncodeEditIcons Then
                                                    IconIDControlString = "AC," & ACType & ",," & ACName
                                                    IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, True, IconIDControlString, "", serverFilePath, "List of Child Pages", "Renders as [List of Child Pages]", ACInstanceID, 0)
                                                    Copy = IconImg
                                                ElseIf EncodeCachableTags Then
                                                    '
                                                    ' Handle in webclient
                                                    '
                                                    ' removed sort method because all child pages are read in together in the order set by the parent - improve this later
                                                    Copy = "{{" & ACTypeChildList & "?name=" & genericController.encodeNvaArgument(ListName) & "}}"
                                                End If
                                            Case ACTypeContact
                                                '
                                                ' Formatting Tag
                                                '
                                                If EncodeEditIcons Then
                                                    '
                                                    IconIDControlString = "AC," & ACType
                                                    IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, True, IconIDControlString, "", serverFilePath, "Contact Information Line", "Renders as [Contact Information Line]", ACInstanceID, 0)
                                                    Copy = IconImg
                                                    '
                                                    'Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as a line of text with contact information for this record's primary contact"" id=""AC," & ACType & """ src=""/ccLib/images/ACContact.GIF"">"
                                                ElseIf EncodeCachableTags Then
                                                    If moreInfoPeopleId <> 0 Then
                                                        Copy = pageContentController.getMoreInfoHtml(cpCore, moreInfoPeopleId)
                                                    End If
                                                End If
                                            Case ACTypeFeedback
                                                '
                                                ' Formatting tag - change from information to be included after submission
                                                '
                                                If EncodeEditIcons Then
                                                    '
                                                    IconIDControlString = "AC," & ACType
                                                    IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, False, IconIDControlString, "", serverFilePath, "Feedback Form", "Renders as [Feedback Form]", ACInstanceID, 0)
                                                    Copy = IconImg
                                                    '
                                                    'Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as a feedback form, sent to this record's primary contact."" id=""AC," & ACType & """ src=""/ccLib/images/ACFeedBack.GIF"">"
                                                ElseIf EncodeNonCachableTags Then
                                                    If (moreInfoPeopleId <> 0) And (ContextContentName <> "") And (ContextRecordID <> 0) Then
                                                        Copy = FeedbackFormNotSupportedComment
                                                    End If
                                                End If
                                            Case ACTypeLanguage
                                                '
                                                ' Personalization Tag - block languages not from the visitor
                                                '
                                                ACLanguageName = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "NAME"))
                                                If EncodeEditIcons Then
                                                    Select Case genericController.vbUCase(ACLanguageName)
                                                        Case "ANY"
                                                            '
                                                            IconIDControlString = "AC," & ACType & ",," & ACLanguageName
                                                            IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, True, IconIDControlString, "", serverFilePath, "All copy following this point is rendered, regardless of the member's language setting", "Renders as [Begin Rendering All Languages]", ACInstanceID, 0)
                                                            Copy = IconImg
                                                            '
                                                            'Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""All copy following this point is rendered, regardless of the member's language setting"" id=""AC," & ACType & ",," & ACLanguageName & """ src=""/ccLib/images/ACLanguageAny.GIF"">"
                                                            'Case "ENGLISH", "FRENCH", "GERMAN", "PORTUGEUESE", "ITALIAN", "SPANISH", "CHINESE", "HINDI"
                                                            '   Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""All copy following this point is rendered if the member's language setting matchs [" & ACLanguageName & "]"" id=""AC," & ACType & ",," & ACLanguageName & """ src=""/ccLib/images/ACLanguage" & ACLanguageName & ".GIF"">"
                                                        Case Else
                                                            '
                                                            IconIDControlString = "AC," & ACType & ",," & ACLanguageName
                                                            IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, True, IconIDControlString, "", serverFilePath, "All copy following this point is rendered if the member's language setting matchs [" & ACLanguageName & "]", "Begin Rendering for language [" & ACLanguageName & "]", ACInstanceID, 0)
                                                            Copy = IconImg
                                                            '
                                                            'Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""All copy following this point is rendered if the member's language setting matchs [" & ACLanguageName & "]"" id=""AC," & ACType & ",," & ACLanguageName & """ src=""/ccLib/images/ACLanguageOther.GIF"">"
                                                    End Select
                                                ElseIf EncodeNonCachableTags Then
                                                    If personalizationPeopleId = 0 Then
                                                        PeopleLanguage = "any"
                                                    Else
                                                        If Not PeopleLanguageSet Then
                                                            If Not CSPeopleSet Then
                                                                CSPeople = cpCore.db.cs_openContentRecord("people", personalizationPeopleId)
                                                                CSPeopleSet = True
                                                            End If
                                                            CSlanguage = cpCore.db.cs_openContentRecord("Languages", cpCore.db.cs_getInteger(CSPeople, "LanguageID"), , , , "Name")
                                                            If cpCore.db.cs_ok(CSlanguage) Then
                                                                PeopleLanguage = cpCore.db.cs_getText(CSlanguage, "name")
                                                            End If
                                                            Call cpCore.db.cs_Close(CSlanguage)
                                                            PeopleLanguageSet = True
                                                        End If
                                                    End If
                                                    UcasePeopleLanguage = genericController.vbUCase(PeopleLanguage)
                                                    If UcasePeopleLanguage = "ANY" Then
                                                        '
                                                        ' This person wants all the languages, put in language marker and continue
                                                        '
                                                        Copy = "<!-- Language " & ACLanguageName & " -->"
                                                    ElseIf (ACLanguageName <> UcasePeopleLanguage) And (ACLanguageName <> "ANY") Then
                                                        '
                                                        ' Wrong language, remove tag, skip to the end, or to the next language tag
                                                        '
                                                        Copy = ""
                                                        ElementPointer = ElementPointer + 1
                                                        Do While ElementPointer < KmaHTML.ElementCount
                                                            ElementTag = genericController.vbUCase(KmaHTML.TagName(ElementPointer))
                                                            If (ElementTag = "AC") Then
                                                                ACType = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "TYPE"))
                                                                If (ACType = ACTypeLanguage) Then
                                                                    ElementPointer = ElementPointer - 1
                                                                    Exit Do
                                                                ElseIf (ACType = ACTypeEnd) Then
                                                                    Exit Do
                                                                End If
                                                            End If
                                                            ElementPointer = ElementPointer + 1
                                                        Loop
                                                    Else
                                                        '
                                                        ' Right Language, remove tag
                                                        '
                                                        Copy = ""
                                                    End If
                                                End If
                                            Case ACTypeAggregateFunction
                                                '
                                                ' ----- Add-on
                                                '
                                                NotUsedID = 0
                                                AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING")
                                                addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded)
                                                If IsEmailContent Then
                                                    '
                                                    ' Addon - for email
                                                    '
                                                    If EncodeNonCachableTags Then
                                                        '
                                                        ' Only hardcoded Add-ons can run in Emails
                                                        '
                                                        Select Case genericController.vbLCase(ACName)
                                                            Case "block text"
                                                                '
                                                                ' Email is always considered authenticated bc they need their login credentials to get the email.
                                                                ' Allowed to see the content that follows if you are authenticated, admin, or in the group list
                                                                ' This must be done out on the page because the csv does not know about authenticated
                                                                '
                                                                Copy = ""
                                                                GroupIDList = htmlController.csv_GetAddonOptionStringValue("AllowGroups", addonOptionString)
                                                                If (Not cpCore.authContext.isMemberOfGroupIdList(cpCore, personalizationPeopleId, True, GroupIDList, True)) Then
                                                                    '
                                                                    ' Block content if not allowed
                                                                    '
                                                                    ElementPointer = ElementPointer + 1
                                                                    Do While ElementPointer < KmaHTML.ElementCount
                                                                        ElementTag = genericController.vbUCase(KmaHTML.TagName(ElementPointer))
                                                                        If (ElementTag = "AC") Then
                                                                            ACType = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "TYPE"))
                                                                            If (ACType = ACTypeAggregateFunction) Then
                                                                                If genericController.vbLCase(KmaHTML.ElementAttribute(ElementPointer, "name")) = "block text end" Then
                                                                                    Exit Do
                                                                                End If
                                                                            End If
                                                                        End If
                                                                        ElementPointer = ElementPointer + 1
                                                                    Loop
                                                                End If
                                                            Case "block text end"
                                                                '
                                                                ' always remove end tags because the block text did not remove it
                                                                '
                                                                Copy = ""
                                                            Case Else
                                                                '
                                                                ' all other add-ons, pass out to cpCoreClass to process
                                                                '
                                                                Copy = cpCore.addon.execute(0, ACName, AddonOptionStringHTMLEncoded, CPUtilsBaseClass.addonContext.ContextEmail, "", 0, "", ACInstanceID, False, 0, "", True, Nothing, "", Nothing, "", personalizationPeopleId, personalizationIsAuthenticated)
                                                                'Copy = "" _
                                                                '    & "" _
                                                                '    & "<!-- ADDON " _
                                                                '    & """" & ACName & """" _
                                                                '    & ",""" & AddonOptionStringHTMLEncoded & """" _
                                                                '    & ",""" & ACInstanceID & """" _
                                                                '    & ",""" & ACGuid & """" _
                                                                '    & " -->" _
                                                                '    & ""
                                                        End Select
                                                    End If
                                                Else
                                                    '
                                                    ' Addon - for web
                                                    '

                                                    If EncodeEditIcons Then
                                                        '
                                                        ' Get IconFilename, update the optionstring, and execute optionstring replacement functions
                                                        '
                                                        AddonContentName = cnAddons
                                                        If True Then
                                                            SelectList = "Name,Link,ID,ArgumentList,ObjectProgramID,IconFilename,IconWidth,IconHeight,IconSprites,IsInline,ccGuid"
                                                        End If
                                                        If ACGuid <> "" Then
                                                            Criteria = "ccguid=" & cpCore.db.encodeSQLText(ACGuid)
                                                        Else
                                                            Criteria = "name=" & cpCore.db.encodeSQLText(UCaseACName)
                                                        End If
                                                        CS = cpCore.db.cs_open(AddonContentName, Criteria, "Name,ID", , , , , SelectList)
                                                        If cpCore.db.cs_ok(CS) Then
                                                            AddonFound = True
                                                            ' ArgumentList comes in already encoded
                                                            IconFilename = cpCore.db.cs_get(CS, "IconFilename")
                                                            SrcOptionList = cpCore.db.cs_get(CS, "ArgumentList")
                                                            IconWidth = cpCore.db.cs_getInteger(CS, "IconWidth")
                                                            IconHeight = cpCore.db.cs_getInteger(CS, "IconHeight")
                                                            IconSprites = cpCore.db.cs_getInteger(CS, "IconSprites")
                                                            AddonIsInline = cpCore.db.cs_getBoolean(CS, "IsInline")
                                                            ACGuid = cpCore.db.cs_getText(CS, "ccGuid")
                                                            IconAlt = ACName
                                                            IconTitle = "Rendered as the Add-on [" & ACName & "]"
                                                        Else
                                                            Select Case genericController.vbLCase(ACName)
                                                                Case "block text"
                                                                    IconFilename = ""
                                                                    SrcOptionList = AddonOptionConstructor_ForBlockText
                                                                    IconWidth = 0
                                                                    IconHeight = 0
                                                                    IconSprites = 0
                                                                    AddonIsInline = True
                                                                    ACGuid = ""
                                                                Case "block text end"
                                                                    IconFilename = ""
                                                                    SrcOptionList = ""
                                                                    IconWidth = 0
                                                                    IconHeight = 0
                                                                    IconSprites = 0
                                                                    AddonIsInline = True
                                                                    ACGuid = ""
                                                                Case Else
                                                                    IconFilename = ""
                                                                    SrcOptionList = ""
                                                                    IconWidth = 0
                                                                    IconHeight = 0
                                                                    IconSprites = 0
                                                                    AddonIsInline = False
                                                                    IconAlt = "Unknown Add-on [" & ACName & "]"
                                                                    IconTitle = "Unknown Add-on [" & ACName & "]"
                                                                    ACGuid = ""
                                                            End Select
                                                        End If
                                                        Call cpCore.db.cs_Close(CS)
                                                        '
                                                        ' Build AddonOptionStringHTMLEncoded from SrcOptionList (for names), itself (for current settings), and SrcOptionList (for select options)
                                                        '
                                                        If (InStr(1, SrcOptionList, "wrapper", vbTextCompare) = 0) Then
                                                            If AddonIsInline Then
                                                                SrcOptionList = SrcOptionList & vbCrLf & AddonOptionConstructor_Inline
                                                            Else
                                                                SrcOptionList = SrcOptionList & vbCrLf & AddonOptionConstructor_Block
                                                            End If
                                                        End If
                                                        If SrcOptionList = "" Then
                                                            ResultOptionListHTMLEncoded = ""
                                                        Else
                                                            ResultOptionListHTMLEncoded = ""
                                                            REsultOptionValue = ""
                                                            SrcOptionList = genericController.vbReplace(SrcOptionList, vbCrLf, vbCr)
                                                            SrcOptionList = genericController.vbReplace(SrcOptionList, vbLf, vbCr)
                                                            SrcOptions = Split(SrcOptionList, vbCr)
                                                            For Ptr = 0 To UBound(SrcOptions)
                                                                SrcOptionName = SrcOptions(Ptr)
                                                                Dim LoopPtr2 As Integer

                                                                LoopPtr2 = 0
                                                                Do While (Len(SrcOptionName) > 1) And (Mid(SrcOptionName, 1, 1) = vbTab) And (LoopPtr2 < 100)
                                                                    SrcOptionName = Mid(SrcOptionName, 2)
                                                                    LoopPtr2 = LoopPtr2 + 1
                                                                Loop
                                                                SrcOptionValueSelector = ""
                                                                SrcOptionSelector = ""
                                                                Pos = genericController.vbInstr(1, SrcOptionName, "=")
                                                                If Pos > 0 Then
                                                                    SrcOptionValueSelector = Mid(SrcOptionName, Pos + 1)
                                                                    SrcOptionName = Mid(SrcOptionName, 1, Pos - 1)
                                                                    SrcOptionSelector = ""
                                                                    Pos = genericController.vbInstr(1, SrcOptionValueSelector, "[")
                                                                    If Pos <> 0 Then
                                                                        SrcOptionSelector = Mid(SrcOptionValueSelector, Pos)
                                                                    End If
                                                                End If
                                                                ' all Src and Instance vars are already encoded correctly
                                                                If SrcOptionName <> "" Then
                                                                    ' since AddonOptionString is encoded, InstanceOptionValue will be also
                                                                    InstanceOptionValue = htmlController.csv_GetAddonOptionStringValue(SrcOptionName, addonOptionString)
                                                                    'InstanceOptionValue = cpcore.csv_GetAddonOption(SrcOptionName, AddonOptionString)
                                                                    ResultOptionSelector = getAddonSelector(SrcOptionName, genericController.encodeNvaArgument(InstanceOptionValue), SrcOptionSelector)
                                                                    'ResultOptionSelector = csv_GetAddonSelector(SrcOptionName, InstanceOptionValue, SrcOptionValueSelector)
                                                                    ResultOptionListHTMLEncoded = ResultOptionListHTMLEncoded & "&" & ResultOptionSelector
                                                                End If
                                                            Next
                                                            If ResultOptionListHTMLEncoded <> "" Then
                                                                ResultOptionListHTMLEncoded = Mid(ResultOptionListHTMLEncoded, 2)
                                                            End If
                                                        End If
                                                        ACNameCaption = genericController.vbReplace(ACName, """", "")
                                                        ACNameCaption = encodeHTML(ACNameCaption)
                                                        IDControlString = "AC," & ACType & "," & NotUsedID & "," & genericController.encodeNvaArgument(ACName) & "," & ResultOptionListHTMLEncoded & "," & ACGuid
                                                        Copy = genericController.GetAddonIconImg(AdminURL, IconWidth, IconHeight, IconSprites, AddonIsInline, IDControlString, IconFilename, serverFilePath, IconAlt, IconTitle, ACInstanceID, 0)
                                                    ElseIf EncodeNonCachableTags Then
                                                        '
                                                        ' Add-on Experiment - move all processing to the Webclient
                                                        ' just pass the name and arguments back in th FPO
                                                        ' HTML encode and quote the name and AddonOptionString
                                                        '
                                                        Copy = "" _
                                                        & "" _
                                                        & "<!-- ADDON " _
                                                        & """" & ACName & """" _
                                                        & ",""" & AddonOptionStringHTMLEncoded & """" _
                                                        & ",""" & ACInstanceID & """" _
                                                        & ",""" & ACGuid & """" _
                                                        & " -->" _
                                                        & ""
                                                    End If
                                                    '
                                                End If
                                            Case ACTypeImage
                                                '
                                                ' ----- Image Tag, substitute image placeholder with the link from the REsource Library Record
                                                '
                                                If EncodeImages Then
                                                    Copy = ""
                                                    ACAttrRecordID = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "RECORDID"))
                                                    ACAttrWidth = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "WIDTH"))
                                                    ACAttrHeight = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "HEIGHT"))
                                                    ACAttrAlt = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "ALT"))
                                                    ACAttrBorder = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "BORDER"))
                                                    ACAttrLoop = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "LOOP"))
                                                    ACAttrVSpace = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "VSPACE"))
                                                    ACAttrHSpace = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "HSPACE"))
                                                    ACAttrAlign = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "ALIGN"))
                                                    '
                                                    Dim file As Models.Entity.libraryFilesModel = Models.Entity.libraryFilesModel.create(cpCore, ACAttrRecordID)
                                                    If (file IsNot Nothing) Then
                                                        Filename = file.Filename
                                                        Filename = genericController.vbReplace(Filename, "\", "/")
                                                        Filename = genericController.EncodeURL(Filename)
                                                        Copy = Copy & "<img ID=""AC,IMAGE,," & ACAttrRecordID & """ src=""" & genericController.getCdnFileLink(cpCore, Filename) & """"
                                                        '
                                                        If ACAttrWidth = 0 Then
                                                            ACAttrWidth = file.pxWidth
                                                        End If
                                                        If ACAttrWidth <> 0 Then
                                                            Copy = Copy & " width=""" & ACAttrWidth & """"
                                                        End If
                                                        '
                                                        If ACAttrHeight = 0 Then
                                                            ACAttrHeight = file.pxHeight
                                                        End If
                                                        If ACAttrHeight <> 0 Then
                                                            Copy = Copy & " height=""" & ACAttrHeight & """"
                                                        End If
                                                        '
                                                        If ACAttrVSpace <> 0 Then
                                                            Copy = Copy & " vspace=""" & ACAttrVSpace & """"
                                                        End If
                                                        '
                                                        If ACAttrHSpace <> 0 Then
                                                            Copy = Copy & " hspace=""" & ACAttrHSpace & """"
                                                        End If
                                                        '
                                                        If ACAttrAlt <> "" Then
                                                            Copy = Copy & " alt=""" & ACAttrAlt & """"
                                                        End If
                                                        '
                                                        If ACAttrAlign <> "" Then
                                                            Copy = Copy & " align=""" & ACAttrAlign & """"
                                                        End If
                                                        '
                                                        ' no, 0 is an important value
                                                        'If ACAttrBorder <> 0 Then
                                                        Copy = Copy & " border=""" & ACAttrBorder & """"
                                                        '    End If
                                                        '
                                                        If ACAttrLoop <> 0 Then
                                                            Copy = Copy & " loop=""" & ACAttrLoop & """"
                                                        End If
                                                        '
                                                        Dim attr As String
                                                        attr = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "STYLE"))
                                                        If attr <> "" Then
                                                            Copy = Copy & " style=""" & attr & """"
                                                        End If
                                                        '
                                                        attr = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "CLASS"))
                                                        If attr <> "" Then
                                                            Copy = Copy & " class=""" & attr & """"
                                                        End If
                                                        '
                                                        Copy = Copy & ">"
                                                    End If
                                                End If
                                            '
                                            '
                                            Case ACTypeDownload
                                                '
                                                ' ----- substitute and anchored download image for the AC-Download tag
                                                '
                                                ACAttrRecordID = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "RECORDID"))
                                                ACAttrAlt = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "ALT"))
                                                '
                                                If EncodeEditIcons Then
                                                    '
                                                    ' Encoding the edit icons for the active editor form
                                                    '
                                                    IconIDControlString = "AC," & ACTypeDownload & ",," & ACAttrRecordID
                                                    IconImg = genericController.GetAddonIconImg(AdminURL, 16, 16, 0, True, IconIDControlString, "/ccLib/images/IconDownload3.gif", serverFilePath, "Download Icon with a link to a resource", "Renders as [Download Icon with a link to a resource]", ACInstanceID, 0)
                                                    Copy = IconImg
                                                    '
                                                    'Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Renders as a download icon"" id=""AC," & ACTypeDownload & ",," & ACAttrRecordID & """ src=""/ccLib/images/IconDownload3.GIF"">"
                                                ElseIf EncodeImages Then
                                                    '
                                                    Dim file As Models.Entity.libraryFilesModel = Models.Entity.libraryFilesModel.create(cpCore, ACAttrRecordID)
                                                    If (file IsNot Nothing) Then
                                                        If ACAttrAlt = "" Then
                                                            ACAttrAlt = genericController.encodeText(file.AltText)
                                                        End If
                                                        Copy = "<a href=""" & ProtocolHostString & requestAppRootPath & cpCore.siteProperties.serverPageDefault & "?" & RequestNameDownloadID & "=" & ACAttrRecordID & """ target=""_blank""><img src=""" & ProtocolHostString & "/ccLib/images/IconDownload3.gif"" width=""16"" height=""16"" border=""0"" alt=""" & ACAttrAlt & """></a>"
                                                    End If
                                                End If
                                            Case ACTypeTemplateContent
                                                '
                                                ' ----- Create Template Content
                                                '
                                                'ACName = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "NAME"))
                                                AddonOptionStringHTMLEncoded = ""
                                                addonOptionString = ""
                                                NotUsedID = 0
                                                If EncodeEditIcons Then
                                                    '
                                                    IconIDControlString = "AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded
                                                    IconImg = genericController.GetAddonIconImg(AdminURL, 52, 64, 0, False, IconIDControlString, "/ccLib/images/ACTemplateContentIcon.gif", serverFilePath, "Template Page Content", "Renders as the Template Page Content", ACInstanceID, 0)
                                                    Copy = IconImg
                                                    '
                                                    'Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Page Content"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACTemplateContentIcon.gif"" WIDTH=52 HEIGHT=64>"
                                                ElseIf EncodeNonCachableTags Then
                                                    '
                                                    ' Add in the Content
                                                    '
                                                    Copy = fpoContentBox
                                                    'Copy = TemplateBodyContent
                                                    'Copy = "{{" & ACTypeTemplateContent & "}}"
                                                End If
                                            Case ACTypeTemplateText
                                                '
                                                ' ----- Create Template Content
                                                '
                                                'ACName = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "NAME"))
                                                AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING")
                                                addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded)
                                                NotUsedID = 0
                                                If EncodeEditIcons Then
                                                    '
                                                    IconIDControlString = "AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded
                                                    IconImg = genericController.GetAddonIconImg(AdminURL, 52, 52, 0, False, IconIDControlString, "/ccLib/images/ACTemplateTextIcon.gif", serverFilePath, "Template Text", "Renders as a Template Text Box", ACInstanceID, 0)
                                                    Copy = IconImg
                                                    '
                                                    'Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as Template Text"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACTemplateTextIcon.gif"" WIDTH=52 HEIGHT=52>"
                                                ElseIf EncodeNonCachableTags Then
                                                    '
                                                    ' Add in the Content Page
                                                    '
                                                    '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                                                    'test - encoding changed
                                                    NewName = htmlController.csv_GetAddonOptionStringValue("new", addonOptionString)
                                                    'NewName =  genericController.DecodeResponseVariable(getSimpleNameValue("new", AddonOptionString, "", "&"))
                                                    TextName = htmlController.csv_GetAddonOptionStringValue("name", addonOptionString)
                                                    'TextName = getSimpleNameValue("name", AddonOptionString)
                                                    If TextName = "" Then
                                                        TextName = "Default"
                                                    End If
                                                    Copy = "{{" & ACTypeTemplateText & "?name=" & genericController.encodeNvaArgument(TextName) & "&new=" & genericController.encodeNvaArgument(NewName) & "}}"
                                                    ' ***** can not add it here, if a web hit, it must be encoded from the web client for aggr objects
                                                    'Copy = csv_GetContentCopy(TextName, "Copy Content", "", personalizationpeopleId)
                                                End If
                                            'Case ACTypeDynamicMenu
                                            '    '
                                            '    ' ----- Create Template Menu
                                            '    '
                                            '    'ACName = KmaHTML.ElementAttribute(ElementPointer, "NAME")
                                            '    AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING")
                                            '    addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded)
                                            '    '
                                            '    ' test for illegal characters (temporary patch to get around not addonencoding during the addon replacement
                                            '    '
                                            '    Pos = genericController.vbInstr(1, addonOptionString, "menunew=", vbTextCompare)
                                            '    If Pos > 0 Then
                                            '        NewName = Mid(addonOptionString, Pos + 8)
                                            '        Dim IsOK As Boolean
                                            '        IsOK = (NewName = genericController.encodeNvaArgument(NewName))
                                            '        If Not IsOK Then
                                            '            addonOptionString = Left(addonOptionString, Pos - 1) & "MenuNew=" & genericController.encodeNvaArgument(NewName)
                                            '        End If
                                            '    End If
                                            '    NotUsedID = 0
                                            '    If EncodeEditIcons Then
                                            '        If genericController.vbInstr(1, AddonOptionStringHTMLEncoded, "menu=", vbTextCompare) <> 0 Then
                                            '            '
                                            '            ' Dynamic Menu
                                            '            '
                                            '            '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                                            '            ' test - encoding changed
                                            '            TextName = cpCore.csv_GetAddonOptionStringValue("menu", addonOptionString)
                                            '            'TextName = cpcore.csv_GetAddonOption("Menu", AddonOptionString)
                                            '            '
                                            '            IconIDControlString = "AC," & ACType & "," & NotUsedID & "," & ACName & ",Menu=" & TextName & "[" & cpCore.csv_GetDynamicMenuACSelect() & "]&NewMenu="
                                            '            IconImg = genericController.GetAddonIconImg(AdminURL, 52, 52, 0, False, IconIDControlString, "/ccLib/images/ACDynamicMenuIcon.gif", serverFilePath, "Dynamic Menu", "Renders as a Dynamic Menu", ACInstanceID, 0)
                                            '            Copy = IconImg
                                            '            '
                                            '            'Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as a Dynamic Menu"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & ",Menu=" & TextName & "[" & csv_GetDynamicMenuACSelect & "]&NewMenu="" src=""/ccLib/images/ACDynamicMenuIcon.gif"" WIDTH=52 HEIGHT=52>"
                                            '        Else
                                            '            '
                                            '            ' Old Dynamic Menu - values are stored in the icon
                                            '            '
                                            '            IconIDControlString = "AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded
                                            '            IconImg = genericController.GetAddonIconImg(AdminURL, 52, 52, 0, False, IconIDControlString, "/ccLib/images/ACDynamicMenuIcon.gif", serverFilePath, "Dynamic Menu", "Renders as a Dynamic Menu", ACInstanceID, 0)
                                            '            Copy = IconImg
                                            '            '
                                            '            'Copy = "<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as a Dynamic Menu"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACDynamicMenuIcon.gif"" WIDTH=52 HEIGHT=52>"
                                            '        End If
                                            '    ElseIf EncodeNonCachableTags Then
                                            '        '
                                            '        ' Add in the Content Pag
                                            '        '
                                            '        Copy = "{{" & ACTypeDynamicMenu & "?" & addonOptionString & "}}"
                                            '    End If
                                            Case ACTypeWatchList
                                                '
                                                ' ----- Formatting Tag
                                                '
                                                '
                                                ' Content Watch replacement
                                                '   served by the web client because the
                                                '
                                                'UCaseACName = genericController.vbUCase(Trim(KmaHTML.ElementAttribute(ElementPointer, "NAME")))
                                                'ACName = encodeInitialCaps(UCaseACName)
                                                AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING")
                                                addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded)
                                                If EncodeEditIcons Then
                                                    '
                                                    IconIDControlString = "AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded
                                                    IconImg = genericController.GetAddonIconImg(AdminURL, 109, 10, 0, True, IconIDControlString, "/ccLib/images/ACWatchList.gif", serverFilePath, "Watch List", "Renders as the Watch List [" & ACName & "]", ACInstanceID, 0)
                                                    Copy = IconImg
                                                    '
                                                    'Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Watch List [" & ACName & "]"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACWatchList.GIF"">"
                                                ElseIf EncodeNonCachableTags Then
                                                    '
                                                    Copy = "{{" & ACTypeWatchList & "?" & addonOptionString & "}}"
                                                End If
                                        End Select
                                End Select
                            End If
                            '
                            ' ----- Output the results
                            '
                            Stream.Add(Copy)
                            ElementPointer = ElementPointer + 1
                        Loop
                    End If
                    workingContent = Stream.Text
                    '
                    ' Add Contensive User Form if needed
                    '
                    If FormCount = 0 And FormInputCount > 0 Then
                    End If
                    workingContent = ReplaceInstructions & workingContent
                    If CSPeopleSet Then
                        Call cpCore.db.cs_Close(CSPeople)
                    End If
                    If CSOrganizationSet Then
                        Call cpCore.db.cs_Close(CSOrganization)
                    End If
                    If CSVisitorSet Then
                        Call cpCore.db.cs_Close(CSVisitor)
                    End If
                    If CSVisitSet Then
                        Call cpCore.db.cs_Close(CSVisit)
                    End If
                    KmaHTML = Nothing
                End If
                result = workingContent
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        '========================================================================
        '   Decodes ActiveContent and EditIcons into <AC tags
        '       Detect IMG tags
        '           If IMG ID attribute is "AC,IMAGE,recordid", convert to AC Image tag
        '           If IMG ID attribute is "AC,DOWNLOAD,recordid", convert to AC Download tag
        '           If IMG ID attribute is "AC,ACType,ACFieldName,ACInstanceName,QueryStringArguments,AddonGuid", convert it to generic AC tag
        '   ACInstanceID - used to identify an AC tag on a page. Each instance of an AC tag must havea unique ACinstanceID
        '========================================================================
        '
        Public Function html_DecodeActiveContent(ByVal SourceCopy As String) As String
            Dim result As String = ""
            Try
                Dim imageNewLink As String
                Dim ACQueryString As String = ""
                Dim ACGuid As String
                Dim ACIdentifier As String
                Dim RecordFilename As String
                Dim RecordFilenameNoExt As String
                Dim RecordFilenameExt As String
                Dim Ptr As Integer
                Dim ACInstanceID As String
                Dim QSHTMLEncoded As String
                Dim Pos As Integer
                Dim ImageSrcOriginal As String
                Dim VirtualFilePathBad As String
                Dim Paths() As String
                Dim ImageVirtualFilename As String
                Dim ImageFilename As String
                Dim ImageFilenameExt As String
                Dim ImageFilenameNoExt As String
                Dim SizeTest() As String
                Dim Styles() As String
                Dim StyleName As String
                Dim StyleValue As String
                Dim StyleValueInt As Integer
                Dim Style() As String
                Dim ImageVirtualFilePath As String
                Dim RecordVirtualFilename As String
                Dim RecordWidth As Integer
                Dim RecordHeight As Integer
                Dim RecordAltSizeList As String
                Dim ImageAltSize As String
                Dim NewImageFilename As String
                Dim DHTML As New htmlParserController(cpCore)
                Dim ElementPointer As Integer
                Dim ElementCount As Integer
                Dim AttributeCount As Integer
                Dim ACType As String
                Dim ACFieldName As String
                Dim ACInstanceName As String
                Dim RecordID As Integer
                Dim ImageWidthText As String
                Dim ImageHeightText As String
                Dim ImageWidth As Integer
                Dim ImageHeight As Integer
                Dim ElementText As String
                Dim ImageID As String
                Dim ImageSrc As String
                Dim ImageAlt As String
                Dim ImageVSpace As Integer
                Dim ImageHSpace As Integer
                Dim ImageAlign As String
                Dim ImageBorder As String
                Dim ImageLoop As String
                Dim ImageStyle As String
                Dim IMageStyleArray As String()
                Dim ImageStyleArrayCount As Integer
                Dim ImageStyleArrayPointer As Integer
                Dim ImageStylePair As String
                Dim PositionColon As Integer
                Dim ImageStylePairName As String
                Dim ImageStylePairValue As String
                Dim Stream As New stringBuilderLegacyController
                Dim ImageIDArray As String() = {}
                Dim ImageIDArrayCount As Integer
                Dim QueryString As String
                Dim QSSplit() As String
                Dim QSPtr As Integer
                Dim serverFilePath As String
                Dim ImageAllowSFResize As Boolean
                Dim sf As imageEditController
                '
                result = SourceCopy
                If result <> "" Then
                    '
                    ' leave this in to make sure old <acform tags are converted back
                    ' new editor deals with <form, so no more converting
                    '
                    result = genericController.vbReplace(result, "<ACFORM>", "<FORM>")
                    result = genericController.vbReplace(result, "<ACFORM ", "<FORM ")
                    result = genericController.vbReplace(result, "</ACFORM>", "</form>")
                    result = genericController.vbReplace(result, "</ACFORM ", "</FORM ")
                    If DHTML.Load(result) Then
                        result = ""
                        ElementCount = DHTML.ElementCount
                        If ElementCount > 0 Then
                            '
                            ' ----- Locate and replace IMG Edit icons with AC tags
                            '
                            Stream = New stringBuilderLegacyController
                            For ElementPointer = 0 To ElementCount - 1
                                ElementText = DHTML.Text(ElementPointer)
                                If DHTML.IsTag(ElementPointer) Then
                                    Select Case genericController.vbUCase(DHTML.TagName(ElementPointer))
                                        Case "FORM"
                                        '
                                        ' User created form - add the attribute "Contensive=1"
                                        '
                                        ' 5/14/2009 - DM said it is OK to remove UserResponseForm Processing
                                        'ElementText = genericController.vbReplace(ElementText, "<FORM", "<FORM ContensiveUserForm=1 ", vbTextCompare)
                                        Case "IMG"
                                            AttributeCount = DHTML.ElementAttributeCount(ElementPointer)

                                            If AttributeCount > 0 Then
                                                ImageID = DHTML.ElementAttribute(ElementPointer, "id")
                                                ImageSrcOriginal = DHTML.ElementAttribute(ElementPointer, "src")
                                                VirtualFilePathBad = cpCore.serverConfig.appConfig.name & "/files/"
                                                serverFilePath = "/" & VirtualFilePathBad
                                                If Left(LCase(ImageSrcOriginal), Len(VirtualFilePathBad)) = genericController.vbLCase(VirtualFilePathBad) Then
                                                    '
                                                    ' if the image is from the virtual file path, but the editor did not include the root path, add it
                                                    '
                                                    ElementText = genericController.vbReplace(ElementText, VirtualFilePathBad, "/" & VirtualFilePathBad, 1, 99, vbTextCompare)
                                                    ImageSrcOriginal = genericController.vbReplace(ImageSrcOriginal, VirtualFilePathBad, "/" & VirtualFilePathBad, 1, 99, vbTextCompare)
                                                End If
                                                ImageSrc = genericController.decodeHtml(ImageSrcOriginal)
                                                ImageSrc = DecodeURL(ImageSrc)
                                                '
                                                ' problem with this case is if the addon icon image is from another site.
                                                ' not sure how it happened, but I do not think the src of an addon edit icon
                                                ' should be able to prevent the addon from executing.
                                                '
                                                ACIdentifier = ""
                                                ACType = ""
                                                ACFieldName = ""
                                                ACInstanceName = ""
                                                ACGuid = ""
                                                ImageIDArrayCount = 0
                                                If 0 <> genericController.vbInstr(1, ImageID, ",") Then
                                                    ImageIDArray = Split(ImageID, ",")
                                                    ImageIDArrayCount = UBound(ImageIDArray) + 1
                                                    If ImageIDArrayCount > 5 Then
                                                        For Ptr = 5 To ImageIDArrayCount - 1
                                                            ACGuid = ImageIDArray(Ptr)
                                                            If (Left(ACGuid, 1) = "{") And (Right(ACGuid, 1) = "}") Then
                                                                '
                                                                ' this element is the guid, go with it
                                                                '
                                                                Exit For
                                                            ElseIf (ACGuid = "") And (Ptr = (ImageIDArrayCount - 1)) Then
                                                                '
                                                                ' this is the last element, leave it as the guid
                                                                '
                                                                Exit For
                                                            Else
                                                                '
                                                                ' not a valid guid, add it to element 4 and try the next
                                                                '
                                                                ImageIDArray(4) = ImageIDArray(4) & "," & ACGuid
                                                                ACGuid = ""
                                                            End If
                                                        Next
                                                    End If
                                                    If (ImageIDArrayCount > 1) Then
                                                        ACIdentifier = genericController.vbUCase(ImageIDArray(0))
                                                        ACType = ImageIDArray(1)
                                                        If ImageIDArrayCount > 2 Then
                                                            ACFieldName = ImageIDArray(2)
                                                            If ImageIDArrayCount > 3 Then
                                                                ACInstanceName = ImageIDArray(3)
                                                                If ImageIDArrayCount > 4 Then
                                                                    ACQueryString = ImageIDArray(4)
                                                                    'If ImageIDArrayCount > 5 Then
                                                                    '    ACGuid = ImageIDArray(5)
                                                                    'End If
                                                                End If
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                                If ACIdentifier = "AC" Then
                                                    If True Then
                                                        If True Then
                                                            '
                                                            ' ----- Process AC Tag
                                                            '
                                                            ACInstanceID = DHTML.ElementAttribute(ElementPointer, "ACINSTANCEID")
                                                            If ACInstanceID = "" Then
                                                                'GUIDGenerator = New guidClass
                                                                ACInstanceID = Guid.NewGuid().ToString
                                                                'ACInstanceID = Guid.NewGuid.ToString()
                                                            End If
                                                            ElementText = ""
                                                            '----------------------------- change to ACType
                                                            Select Case genericController.vbUCase(ACType)
                                                                Case "IMAGE"
                                                                    '
                                                                    ' ----- AC Image, Decode Active Images to Resource Library references
                                                                    '
                                                                    If ImageIDArrayCount >= 4 Then
                                                                        RecordID = genericController.EncodeInteger(ACInstanceName)
                                                                        ImageWidthText = DHTML.ElementAttribute(ElementPointer, "WIDTH")
                                                                        ImageHeightText = DHTML.ElementAttribute(ElementPointer, "HEIGHT")
                                                                        ImageAlt = encodeHTML(DHTML.ElementAttribute(ElementPointer, "Alt"))
                                                                        ImageVSpace = genericController.EncodeInteger(DHTML.ElementAttribute(ElementPointer, "vspace"))
                                                                        ImageHSpace = genericController.EncodeInteger(DHTML.ElementAttribute(ElementPointer, "hspace"))
                                                                        ImageAlign = DHTML.ElementAttribute(ElementPointer, "Align")
                                                                        ImageBorder = DHTML.ElementAttribute(ElementPointer, "BORDER")
                                                                        ImageLoop = DHTML.ElementAttribute(ElementPointer, "LOOP")
                                                                        ImageStyle = DHTML.ElementAttribute(ElementPointer, "STYLE")

                                                                        If ImageStyle <> "" Then
                                                                            '
                                                                            ' ----- Process styles, which override attributes
                                                                            '
                                                                            IMageStyleArray = Split(ImageStyle, ";")
                                                                            ImageStyleArrayCount = UBound(IMageStyleArray) + 1
                                                                            For ImageStyleArrayPointer = 0 To ImageStyleArrayCount - 1
                                                                                ImageStylePair = Trim(IMageStyleArray(ImageStyleArrayPointer))
                                                                                PositionColon = genericController.vbInstr(1, ImageStylePair, ":")
                                                                                If PositionColon > 1 Then
                                                                                    ImageStylePairName = Trim(Mid(ImageStylePair, 1, PositionColon - 1))
                                                                                    ImageStylePairValue = Trim(Mid(ImageStylePair, PositionColon + 1))
                                                                                    Select Case genericController.vbUCase(ImageStylePairName)
                                                                                        Case "WIDTH"
                                                                                            ImageStylePairValue = genericController.vbReplace(ImageStylePairValue, "px", "")
                                                                                            ImageWidthText = ImageStylePairValue
                                                                                        Case "HEIGHT"
                                                                                            ImageStylePairValue = genericController.vbReplace(ImageStylePairValue, "px", "")
                                                                                            ImageHeightText = ImageStylePairValue
                                                                                    End Select
                                                                                    'If genericController.vbInstr(1, ImageStylePair, "WIDTH", vbTextCompare) = 1 Then
                                                                                    '    End If
                                                                                End If
                                                                            Next
                                                                        End If
                                                                        ElementText = "<AC type=""IMAGE"" ACInstanceID=""" & ACInstanceID & """ RecordID=""" & RecordID & """ Style=""" & ImageStyle & """ Width=""" & ImageWidthText & """ Height=""" & ImageHeightText & """ VSpace=""" & ImageVSpace & """ HSpace=""" & ImageHSpace & """ Alt=""" & ImageAlt & """ Align=""" & ImageAlign & """ Border=""" & ImageBorder & """ Loop=""" & ImageLoop & """>"
                                                                    End If
                                                                Case ACTypeDownload
                                                                    '
                                                                    ' AC Download
                                                                    '
                                                                    If ImageIDArrayCount >= 4 Then
                                                                        RecordID = genericController.EncodeInteger(ACInstanceName)
                                                                        ElementText = "<AC type=""DOWNLOAD"" ACInstanceID=""" & ACInstanceID & """ RecordID=""" & RecordID & """>"
                                                                    End If
                                                                Case ACTypeDate
                                                                    '
                                                                    ' Date
                                                                    '
                                                                    ElementText = "<AC type=""" & ACTypeDate & """>"
                                                                Case ACTypeVisit, ACTypeVisitor, ACTypeMember, ACTypeOrganization, ACTypePersonalization
                                                                    '
                                                                    ' Visit, etc
                                                                    '
                                                                    ElementText = "<AC type=""" & ACType & """ ACInstanceID=""" & ACInstanceID & """ field=""" & ACFieldName & """>"
                                                                Case ACTypeChildList, ACTypeLanguage
                                                                    '
                                                                    ' ChildList, Language
                                                                    '
                                                                    If ACInstanceName = "0" Then
                                                                        ACInstanceName = genericController.GetRandomInteger().ToString()
                                                                    End If
                                                                    ElementText = "<AC type=""" & ACType & """ name=""" & ACInstanceName & """ ACInstanceID=""" & ACInstanceID & """>"
                                                                Case ACTypeAggregateFunction
                                                                    '
                                                                    ' Function
                                                                    '
                                                                    QueryString = ""
                                                                    If ACQueryString <> "" Then
                                                                        ' I added this because single stepping through it I found it split on the & in &amp;
                                                                        ' I had added an Add-on and was saving
                                                                        ' I find it VERY odd that this could be the case
                                                                        '
                                                                        QSHTMLEncoded = genericController.encodeText(ACQueryString)
                                                                        QueryString = genericController.decodeHtml(QSHTMLEncoded)
                                                                        QSSplit = Split(QueryString, "&")
                                                                        For QSPtr = 0 To UBound(QSSplit)
                                                                            Pos = genericController.vbInstr(1, QSSplit(QSPtr), "[")
                                                                            If Pos > 0 Then
                                                                                QSSplit(QSPtr) = Mid(QSSplit(QSPtr), 1, Pos - 1)
                                                                            End If
                                                                            QSSplit(QSPtr) = encodeHTML(QSSplit(QSPtr))
                                                                        Next
                                                                        QueryString = Join(QSSplit, "&")
                                                                    End If
                                                                    ElementText = "<AC type=""" & ACType & """ name=""" & ACInstanceName & """ ACInstanceID=""" & ACInstanceID & """ querystring=""" & QueryString & """ guid=""" & ACGuid & """>"
                                                                Case ACTypeContact, ACTypeFeedback
                                                                    '
                                                                    ' Contact and Feedback
                                                                    '
                                                                    ElementText = "<AC type=""" & ACType & """ ACInstanceID=""" & ACInstanceID & """>"
                                                                Case ACTypeTemplateContent, ACTypeTemplateText
                                                                    '
                                                                    '
                                                                    '
                                                                    QueryString = ""
                                                                    If ImageIDArrayCount > 4 Then
                                                                        QueryString = genericController.encodeText(ImageIDArray(4))
                                                                        QSSplit = Split(QueryString, "&")
                                                                        For QSPtr = 0 To UBound(QSSplit)
                                                                            QSSplit(QSPtr) = encodeHTML(QSSplit(QSPtr))
                                                                        Next
                                                                        QueryString = Join(QSSplit, "&")

                                                                    End If
                                                                    ElementText = "<AC type=""" & ACType & """ name=""" & ACInstanceName & """ ACInstanceID=""" & ACInstanceID & """ querystring=""" & QueryString & """>"
                                                                Case ACTypeWatchList
                                                                    '
                                                                    ' Watch List
                                                                    '
                                                                    QueryString = ""
                                                                    If ImageIDArrayCount > 4 Then
                                                                        QueryString = genericController.encodeText(ImageIDArray(4))
                                                                        QueryString = genericController.decodeHtml(QueryString)
                                                                        QSSplit = Split(QueryString, "&")
                                                                        For QSPtr = 0 To UBound(QSSplit)
                                                                            QSSplit(QSPtr) = encodeHTML(QSSplit(QSPtr))
                                                                        Next
                                                                        QueryString = Join(QSSplit, "&")
                                                                    End If
                                                                    ElementText = "<AC type=""" & ACType & """ name=""" & ACInstanceName & """ ACInstanceID=""" & ACInstanceID & """ querystring=""" & QueryString & """>"
                                                                Case ACTypeRSSLink
                                                                    '
                                                                    ' RSS Link
                                                                    '
                                                                    QueryString = ""
                                                                    If ImageIDArrayCount > 4 Then
                                                                        QueryString = genericController.encodeText(ImageIDArray(4))
                                                                        QueryString = genericController.decodeHtml(QueryString)
                                                                        QSSplit = Split(QueryString, "&")
                                                                        For QSPtr = 0 To UBound(QSSplit)
                                                                            QSSplit(QSPtr) = encodeHTML(QSSplit(QSPtr))
                                                                        Next
                                                                        QueryString = Join(QSSplit, "&")
                                                                    End If
                                                                    ElementText = "<AC type=""" & ACType & """ name=""" & ACInstanceName & """ ACInstanceID=""" & ACInstanceID & """ querystring=""" & QueryString & """>"
                                                                Case Else
                                                                    '
                                                                    ' All others -- added querystring from element(4) to all others to cover the group access AC object
                                                                    '
                                                                    QueryString = ""
                                                                    If ImageIDArrayCount > 4 Then
                                                                        QueryString = genericController.encodeText(ImageIDArray(4))
                                                                        QueryString = genericController.decodeHtml(QueryString)
                                                                        QSSplit = Split(QueryString, "&")
                                                                        For QSPtr = 0 To UBound(QSSplit)
                                                                            QSSplit(QSPtr) = encodeHTML(QSSplit(QSPtr))
                                                                        Next
                                                                        QueryString = Join(QSSplit, "&")
                                                                    End If
                                                                    ElementText = "<AC type=""" & ACType & """ name=""" & ACInstanceName & """ ACInstanceID=""" & ACInstanceID & """ field=""" & ACFieldName & """ querystring=""" & QueryString & """>"
                                                            End Select
                                                        End If
                                                    End If
                                                ElseIf genericController.vbInstr(1, ImageSrc, "cclibraryfiles", vbTextCompare) <> 0 Then
                                                    ImageAllowSFResize = cpCore.siteProperties.getBoolean("ImageAllowSFResize", True)
                                                    If ImageAllowSFResize And True Then
                                                        '
                                                        ' if it is a real image, check for resize
                                                        '
                                                        Pos = genericController.vbInstr(1, ImageSrc, "cclibraryfiles", vbTextCompare)
                                                        If Pos <> 0 Then
                                                            ImageVirtualFilename = Mid(ImageSrc, Pos)
                                                            Paths = Split(ImageVirtualFilename, "/")
                                                            If UBound(Paths) > 2 Then
                                                                If genericController.vbLCase(Paths(1)) = "filename" Then
                                                                    RecordID = genericController.EncodeInteger(Paths(2))
                                                                    If RecordID <> 0 Then
                                                                        ImageFilename = Paths(3)
                                                                        ImageVirtualFilePath = genericController.vbReplace(ImageVirtualFilename, ImageFilename, "")
                                                                        Pos = InStrRev(ImageFilename, ".")
                                                                        If Pos > 0 Then
                                                                            Dim ImageFilenameAltSize As String = ""
                                                                            ImageFilenameExt = Mid(ImageFilename, Pos + 1)
                                                                            ImageFilenameNoExt = Mid(ImageFilename, 1, Pos - 1)
                                                                            Pos = InStrRev(ImageFilenameNoExt, "-")
                                                                            If Pos > 0 Then
                                                                                '
                                                                                ' ImageAltSize should be set from the width and height of the img tag,
                                                                                ' NOT from the actual width and height of the image file
                                                                                ' NOT from the suffix of the image filename
                                                                                ' ImageFilenameAltSize is used when the image has been resized, then 'reset' was hit
                                                                                '  on the properties dialog before the save. The width and height come from this suffix
                                                                                '
                                                                                ImageFilenameAltSize = Mid(ImageFilenameNoExt, Pos + 1)
                                                                                SizeTest = Split(ImageFilenameAltSize, "x")
                                                                                If UBound(SizeTest) <> 1 Then
                                                                                    ImageFilenameAltSize = ""
                                                                                Else
                                                                                    If genericController.vbIsNumeric(SizeTest(0)) And genericController.vbIsNumeric(SizeTest(1)) Then
                                                                                        ImageFilenameNoExt = Mid(ImageFilenameNoExt, 1, Pos - 1)
                                                                                        'RecordVirtualFilenameNoExt = Mid(RecordVirtualFilename, 1, Pos - 1)
                                                                                    Else
                                                                                        ImageFilenameAltSize = ""
                                                                                    End If
                                                                                End If
                                                                                'ImageFilenameNoExt = Mid(ImageFilenameNoExt, 1, Pos - 1)
                                                                            End If
                                                                            If genericController.vbInstr(1, sfImageExtList, ImageFilenameExt, vbTextCompare) <> 0 Then
                                                                                '
                                                                                ' Determine ImageWidth and ImageHeight
                                                                                '
                                                                                ImageStyle = DHTML.ElementAttribute(ElementPointer, "style")
                                                                                ImageWidth = genericController.EncodeInteger(DHTML.ElementAttribute(ElementPointer, "width"))
                                                                                ImageHeight = genericController.EncodeInteger(DHTML.ElementAttribute(ElementPointer, "height"))
                                                                                If ImageStyle <> "" Then
                                                                                    Styles = Split(ImageStyle, ";")
                                                                                    For Ptr = 0 To UBound(Styles)
                                                                                        Style = Split(Styles(Ptr), ":")
                                                                                        If UBound(Style) > 0 Then
                                                                                            StyleName = genericController.vbLCase(Trim(Style(0)))
                                                                                            If StyleName = "width" Then
                                                                                                StyleValue = genericController.vbLCase(Trim(Style(1)))
                                                                                                StyleValue = genericController.vbReplace(StyleValue, "px", "")
                                                                                                StyleValueInt = genericController.EncodeInteger(StyleValue)
                                                                                                If StyleValueInt > 0 Then
                                                                                                    ImageWidth = StyleValueInt
                                                                                                End If
                                                                                            ElseIf StyleName = "height" Then
                                                                                                StyleValue = genericController.vbLCase(Trim(Style(1)))
                                                                                                StyleValue = genericController.vbReplace(StyleValue, "px", "")
                                                                                                StyleValueInt = genericController.EncodeInteger(StyleValue)
                                                                                                If StyleValueInt > 0 Then
                                                                                                    ImageHeight = StyleValueInt
                                                                                                End If
                                                                                            End If
                                                                                        End If
                                                                                    Next
                                                                                End If
                                                                                '
                                                                                ' Get the record values
                                                                                '
                                                                                Dim file As Models.Entity.libraryFilesModel = Models.Entity.libraryFilesModel.create(cpCore, RecordID)
                                                                                If (file IsNot Nothing) Then
                                                                                    RecordVirtualFilename = file.Filename
                                                                                    RecordWidth = file.pxWidth
                                                                                    RecordHeight = file.pxHeight
                                                                                    RecordAltSizeList = file.AltSizeList
                                                                                    RecordFilename = RecordVirtualFilename
                                                                                    Pos = InStrRev(RecordVirtualFilename, "/")
                                                                                    If Pos > 0 Then
                                                                                        RecordFilename = Mid(RecordVirtualFilename, Pos + 1)
                                                                                    End If
                                                                                    RecordFilenameExt = ""
                                                                                    RecordFilenameNoExt = RecordFilename
                                                                                    Pos = InStrRev(RecordFilenameNoExt, ".")
                                                                                    If Pos > 0 Then
                                                                                        RecordFilenameExt = Mid(RecordFilenameNoExt, Pos + 1)
                                                                                        RecordFilenameNoExt = Mid(RecordFilenameNoExt, 1, Pos - 1)
                                                                                    End If
                                                                                    '
                                                                                    ' if recordwidth or height are missing, get them from the file
                                                                                    '
                                                                                    If RecordWidth = 0 Or RecordHeight = 0 Then
                                                                                        sf = New imageEditController
                                                                                        If sf.load(genericController.convertCdnUrlToCdnPathFilename(ImageVirtualFilename)) Then
                                                                                            file.pxWidth = sf.width
                                                                                            file.pxHeight = sf.height
                                                                                            file.save(cpCore)
                                                                                        End If
                                                                                        Call sf.Dispose()
                                                                                        sf = Nothing
                                                                                    End If
                                                                                    '
                                                                                    ' continue only if we have record width and height
                                                                                    '
                                                                                    If RecordWidth <> 0 And RecordHeight <> 0 Then
                                                                                        '
                                                                                        ' set ImageWidth and ImageHeight if one of them is missing
                                                                                        '
                                                                                        If (ImageWidth = RecordWidth) And (ImageHeight = 0) Then
                                                                                            '
                                                                                            ' Image only included width, set default height
                                                                                            '
                                                                                            ImageHeight = RecordHeight
                                                                                        ElseIf (ImageHeight = RecordHeight) And (ImageWidth = 0) Then
                                                                                            '
                                                                                            ' Image only included height, set default width
                                                                                            '
                                                                                            ImageWidth = RecordWidth
                                                                                        ElseIf (ImageHeight = 0) And (ImageWidth = 0) Then
                                                                                            '
                                                                                            ' Image has no width or height, default both
                                                                                            ' This happens when you hit 'reset' on the image properties dialog
                                                                                            '
                                                                                            sf = New imageEditController
                                                                                            If sf.load(genericController.convertCdnUrlToCdnPathFilename(ImageVirtualFilename)) Then
                                                                                                ImageWidth = sf.width
                                                                                                ImageHeight = sf.height
                                                                                            End If
                                                                                            Call sf.Dispose()
                                                                                            sf = Nothing
                                                                                            If (ImageHeight = 0) And (ImageWidth = 0) And (Not String.IsNullOrEmpty(ImageFilenameAltSize)) Then
                                                                                                Pos = genericController.vbInstr(1, ImageFilenameAltSize, "x")
                                                                                                If Pos <> 0 Then
                                                                                                    ImageWidth = genericController.EncodeInteger(Mid(ImageFilenameAltSize, 1, Pos - 1))
                                                                                                    ImageHeight = genericController.EncodeInteger(Mid(ImageFilenameAltSize, Pos + 1))
                                                                                                End If
                                                                                            End If
                                                                                            If ImageHeight = 0 And ImageWidth = 0 Then
                                                                                                ImageHeight = RecordHeight
                                                                                                ImageWidth = RecordWidth
                                                                                            End If
                                                                                        End If
                                                                                        '
                                                                                        ' Set the ImageAltSize to what was requested from the img tag
                                                                                        ' if the actual image is a few rounding-error pixels off does not matter
                                                                                        ' if either is 0, let altsize be 0, set real value for image height/width
                                                                                        '
                                                                                        ImageAltSize = CStr(ImageWidth) & "x" & CStr(ImageHeight)
                                                                                        '
                                                                                        ' determine if we are OK, or need to rebuild
                                                                                        '
                                                                                        If (RecordVirtualFilename = (ImageVirtualFilePath & ImageFilename)) And ((RecordWidth = ImageWidth) Or (RecordHeight = ImageHeight)) Then
                                                                                            '
                                                                                            ' OK
                                                                                            ' this is the raw image
                                                                                            ' image matches record, and the sizes are the same
                                                                                            '
                                                                                            RecordVirtualFilename = RecordVirtualFilename
                                                                                        ElseIf (RecordVirtualFilename = ImageVirtualFilePath & ImageFilenameNoExt & "." & ImageFilenameExt) And (InStr(1, RecordAltSizeList, ImageAltSize, vbTextCompare) <> 0) Then
                                                                                            '
                                                                                            ' OK
                                                                                            ' resized image, and altsize is in the list - go with resized image name
                                                                                            '
                                                                                            NewImageFilename = ImageFilenameNoExt & "-" & ImageAltSize & "." & ImageFilenameExt
                                                                                            ' images included in email have spaces that must be converted to "%20" or they 404
                                                                                            imageNewLink = genericController.EncodeURL(genericController.getCdnFileLink(cpCore, ImageVirtualFilePath) & NewImageFilename)
                                                                                            ElementText = genericController.vbReplace(ElementText, ImageSrcOriginal, encodeHTML(imageNewLink))
                                                                                        ElseIf (RecordWidth < ImageWidth) Or (RecordHeight < ImageHeight) Then
                                                                                            '
                                                                                            ' OK
                                                                                            ' reize image larger then original - go with it as is
                                                                                            '
                                                                                            ' images included in email have spaces that must be converted to "%20" or they 404
                                                                                            ElementText = genericController.vbReplace(ElementText, ImageSrcOriginal, encodeHTML(genericController.EncodeURL(genericController.getCdnFileLink(cpCore, RecordVirtualFilename))))
                                                                                        Else
                                                                                            '
                                                                                            ' resized image - create NewImageFilename (and add new alt size to the record)
                                                                                            '
                                                                                            If RecordWidth = ImageWidth And RecordHeight = ImageHeight Then
                                                                                                '
                                                                                                ' set back to Raw image untouched, use the record image filename
                                                                                                '
                                                                                                ElementText = ElementText
                                                                                                'ElementText = genericController.vbReplace(ElementText, ImageVirtualFilename, RecordVirtualFilename)
                                                                                            Else
                                                                                                '
                                                                                                ' Raw image filename in content, but it is resized, switch to an alternate size
                                                                                                '
                                                                                                NewImageFilename = RecordFilename
                                                                                                If (ImageWidth = 0) Or (ImageHeight = 0) Or (InStr(1, vbCrLf & RecordAltSizeList & vbCrLf, vbCrLf & ImageAltSize & vbCrLf) = 0) Then
                                                                                                    '
                                                                                                    ' Alt image has not been built
                                                                                                    '
                                                                                                    sf = New imageEditController
                                                                                                    If Not sf.load(genericController.convertCdnUrlToCdnPathFilename(RecordVirtualFilename)) Then
                                                                                                        '
                                                                                                        ' image load failed, use raw filename
                                                                                                        '
                                                                                                        Throw (New ApplicationException("Unexpected exception")) 'cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "Error while loading image to resize, [" & RecordVirtualFilename & "]", "dll", "cpCoreClass", "DecodeAciveContent", Err.Number, Err.Source, Err.Description, False, True, "")
                                                                                                        Err.Clear()
                                                                                                        NewImageFilename = ImageFilename
                                                                                                    Else
                                                                                                        '
                                                                                                        '
                                                                                                        '
                                                                                                        RecordWidth = sf.width
                                                                                                        RecordHeight = sf.height
                                                                                                        If ImageWidth = 0 Then
                                                                                                            '
                                                                                                            '
                                                                                                            '
                                                                                                            sf.height = ImageHeight
                                                                                                        ElseIf ImageHeight = 0 Then
                                                                                                            '
                                                                                                            '
                                                                                                            '
                                                                                                            sf.width = ImageWidth
                                                                                                        ElseIf RecordHeight = ImageHeight Then
                                                                                                            '
                                                                                                            ' change the width
                                                                                                            '
                                                                                                            sf.width = ImageWidth
                                                                                                        Else
                                                                                                            '
                                                                                                            ' change the height
                                                                                                            '
                                                                                                            sf.height = ImageHeight
                                                                                                        End If
                                                                                                        '
                                                                                                        ' if resized only width or height, set the other
                                                                                                        '
                                                                                                        If ImageWidth = 0 Then
                                                                                                            ImageWidth = sf.width
                                                                                                            ImageAltSize = CStr(ImageWidth) & "x" & CStr(ImageHeight)
                                                                                                        End If
                                                                                                        If ImageHeight = 0 Then
                                                                                                            ImageHeight = sf.height
                                                                                                            ImageAltSize = CStr(ImageWidth) & "x" & CStr(ImageHeight)
                                                                                                        End If
                                                                                                        '
                                                                                                        ' set HTML attributes so image properties will display
                                                                                                        '
                                                                                                        If genericController.vbInstr(1, ElementText, "height=", vbTextCompare) = 0 Then
                                                                                                            ElementText = genericController.vbReplace(ElementText, ">", " height=""" & ImageHeight & """>")
                                                                                                        End If
                                                                                                        If genericController.vbInstr(1, ElementText, "width=", vbTextCompare) = 0 Then
                                                                                                            ElementText = genericController.vbReplace(ElementText, ">", " width=""" & ImageWidth & """>")
                                                                                                        End If
                                                                                                        '
                                                                                                        ' Save new file
                                                                                                        '
                                                                                                        NewImageFilename = RecordFilenameNoExt & "-" & ImageAltSize & "." & RecordFilenameExt
                                                                                                        Call sf.save(genericController.convertCdnUrlToCdnPathFilename(ImageVirtualFilePath & NewImageFilename))
                                                                                                        '
                                                                                                        ' Update image record
                                                                                                        '
                                                                                                        RecordAltSizeList = RecordAltSizeList & vbCrLf & ImageAltSize
                                                                                                    End If
                                                                                                End If
                                                                                                '
                                                                                                ' Change the image src to the AltSize
                                                                                                ElementText = genericController.vbReplace(ElementText, ImageSrcOriginal, encodeHTML(genericController.EncodeURL(genericController.getCdnFileLink(cpCore, ImageVirtualFilePath) & NewImageFilename)))
                                                                                            End If
                                                                                        End If
                                                                                    End If
                                                                                End If
                                                                                file.save(cpCore)
                                                                            End If
                                                                        End If
                                                                    End If
                                                                End If
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            End If
                                    End Select
                                End If
                                Stream.Add(ElementText)
                            Next
                        End If
                        result = Stream.Text
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
            '
        End Function
        '
        '========================================================================
        ' ----- Decode Content
        '========================================================================
        '
        Public Function renderActiveContent(ByVal Source As String) As String
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-186" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            ' ----- Do Active Content Conversion
            '
            renderActiveContent = Source
            If (renderActiveContent <> "") Then
                renderActiveContent = html_DecodeActiveContent(Source)
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError4(Err.Number, Err.Source, Err.Description, "csv_DecodeContent", True)
        End Function
        '
        '========================================================================
        ' Modify a string to be printed through the HTML stream
        '   convert carriage returns ( 0x10 ) to <br>
        '   remove linefeeds ( 0x13 )
        '========================================================================
        '
        Public Function main_EncodeCRLF(ByVal Source As Object) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("EncodeCRLF")
            '
            Dim iSource As String
            '
            iSource = genericController.encodeText(Source)
            main_EncodeCRLF = ""
            If (iSource <> "") Then
                main_EncodeCRLF = iSource
                main_EncodeCRLF = genericController.vbReplace(main_EncodeCRLF, vbCr, "")
                main_EncodeCRLF = genericController.vbReplace(main_EncodeCRLF, vbLf, "<br >")
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_EncodeCRLF")
        End Function
        '
        '========================================================================
        '   Encodes characters to be compatibile with HTML
        '   i.e. it converts the equation 5 > 6 to th sequence "5 &gt; 6"
        '
        '   convert carriage returns ( 0x10 ) to <br >
        '   remove linefeeds ( 0x13 )
        '========================================================================
        '
        Public Function main_encodeHTML(ByVal Source As Object) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("encodeHTML")
            '
            main_encodeHTML = encodeHTML(genericController.encodeText(Source))
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("EncodeHTML")
        End Function
        '
        '========================================================================
        '   Convert an HTML source to a text equivelent
        '
        '       converts CRLF to <br>
        '       encodes reserved HTML characters to their equivalent
        '========================================================================
        '
        Public Function html_convertText2HTML(ByVal Source As Object) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("ConvertText2HTML")
            '
            html_convertText2HTML = encodeHTML(genericController.encodeText(Source))
            html_convertText2HTML = main_EncodeCRLF(html_convertText2HTML)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_ConvertText2HTML")
        End Function
        '
        '========================================================================
        '   11/26/2009 - changed to 'undo' what encodehtml does
        '
        '   it converts the html equivlent "5 &gt; 6" to the he equation 5>6
        '========================================================================
        '
        Public Function main_DecodeHTML(ByVal Source As Object) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("DecodeHTML")
            '
            'If Not (true) Then Exit Function
            '
            main_DecodeHTML = genericController.decodeHtml(genericController.encodeText(Source))
            '
            '    '
            '    Dim Decoder As htmlDecodeClass
            '    Dim iSource As String
            '    '
            '    iSource = genericController.encodeText(Source)
            '    '
            '    Decoder = New htmlDecodeClass
            '    main_DecodeHTML = Decoder.Decode(iSource)
            '    Decoder = Nothing
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            'Set Decoder = Nothing
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_DecodeHTML")
        End Function
        '
        '========================================================================
        ' ----- Encode Active Content AI
        '========================================================================
        '
        Public Function main_ConvertHTML2Text(ByVal Source As String) As String
            Try
                Dim Decoder As New htmlToTextControllers(cpCore)
                Return Decoder.convert(Source)
            Catch ex As Exception
                Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_ConvertHTML2Text")
            End Try
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function main_EncodeRequestVariable(Source As String) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("EncodeRequestVariable")
            '
            main_EncodeRequestVariable = genericController.EncodeRequestVariable(genericController.encodeText(Source))
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_EncodeRequestVariable")
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function main_EncodeURL(Source As String) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("EncodeURL")
            '
            'If Not (true) Then Exit Function
            '
            main_EncodeURL = genericController.EncodeURL(genericController.encodeText(Source))
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_EncodeURL")
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function main_DecodeUrl(ByVal sUrl As String) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("DecodeUrl")
            '
            main_DecodeUrl = genericController.DecodeResponseVariable(genericController.encodeText(sUrl))
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("DecodeUrl")
        End Function
        '        '
        '        '
        '        '
        '        Private Function html_DecodeActiveContent_ProcessDynamicMenu(ByVal QueryString As String) As String
        '            On Error GoTo ErrorTrap 'Const Tn = "csv_DecodeActiveContent_ProcessDynamicMenu" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim EditTabCaption As String
        '            Dim ACTags() As String
        '            Dim TagPtr As Integer
        '            Dim QSPos As Integer
        '            Dim QSPosEnd As Integer
        '            Dim QS As String
        '            Dim MenuName As String
        '            Dim StylePrefix As String
        '            Dim CS As Integer
        '            Dim IsFound As Boolean
        '            Dim StyleSheet As String
        '            'Dim DefaultStyles As String
        '            'Dim DynamicStyles As String
        '            'Dim AddStyles As String
        '            'Dim StyleSplit() As String
        '            'Dim StylePtr as integer
        '            'Dim StyleLine As String
        '            Dim Filename As String
        '            'Dim NewStyleLine As String
        '            Dim Menu As String
        '            Dim MenuNew As String
        '            '
        '            QS = QueryString
        '            If True Then
        '                If genericController.vbInstr(1, QS, "Menu=", vbTextCompare) <> 0 Then
        '                    '
        '                    ' New menu
        '                    '
        '                    Menu = cpCore.csv_GetAddonOption("Menu", QS)
        '                    MenuNew = cpCore.csv_GetAddonOption("NewMenu", QS)
        '                    If MenuNew <> "" Then
        '                        '
        '                        ' Add a new Menu
        '                        '
        '                        Menu = MenuNew
        '                        Call cpCore.csv_VerifyDynamicMenu(Menu)
        '                    End If
        '                    '
        '                    ' fixup the tag so next encode it pulls a new list of Dynamic Menus
        '                    '
        '                    QS = "Menu=" & Menu
        '                ElseIf genericController.vbInstr(1, QS, "MenuName=", vbTextCompare) <> 0 Then
        '                    '
        '                    ' Old Style Menu Icon
        '                    '
        '                    MenuName = cpCore.csv_GetAddonOption("MenuName", QS)
        '                    Call cpCore.csv_VerifyDynamicMenu(MenuName)
        '                End If
        '            End If
        '            html_DecodeActiveContent_ProcessDynamicMenu = QS
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError4(Err.Number, Err.Source, Err.Description, "csv_DecodeActiveContent_ProcessDynamicMenu", True, True)
        '        End Function
        '        '
        '        '=======================================================================================================
        '        '   return the entire stylesheet for the given templateID and/or EmailID
        '        '=======================================================================================================
        '        '
        '        Public Function html_getStyleSheet2(ByVal ContentType As csv_contentTypeEnum, ByVal templateId As Integer, Optional ByVal EmailID As Integer = 0) As String
        '            On Error GoTo ErrorTrap 'Const Tn = "getStyleSheet2" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim blockStyles As Boolean
        '            Dim usedSharedStyleList As String = String.Empty
        '            Dim EMailTemplateID As Integer
        '            Dim SQL As String
        '            Dim CS As Integer
        '            Dim Filename As String
        '            'dim dt as datatable
        '            Dim StyleName As String
        '            Dim styleId As Integer
        '            Dim Ptr As Integer
        '            Dim DefaultStyles As String
        '            Dim SiteStyles As String
        '            Dim sharedStyles As String = String.Empty
        '            Dim templateStyles As String = String.Empty
        '            Dim emailstyles As String = String.Empty
        '            '
        '            For Ptr = 0 To cpCore.stylesheetCacheCnt - 1
        '                If (cpCore.stylesheetCache(Ptr).EmailID = EmailID) And (cpCore.stylesheetCache(Ptr).EmailID = EmailID) Then
        '                    html_getStyleSheet2 = cpCore.stylesheetCache(Ptr).StyleSheet
        '                    Exit For
        '                End If
        '            Next
        '            If Ptr >= cpCore.stylesheetCacheCnt Then
        '                blockStyles = False
        '                DefaultStyles = getStyleSheetDefault()
        '                SiteStyles = "" _
        '                    & vbCrLf & "/*" _
        '                    & vbCrLf & "Site Styles" _
        '                    & vbCrLf & "*/" _
        '                    & vbCrLf & genericController.RemoveStyleTags(cpCore.cdnFiles.readFile("templates\styles.css"))
        '                '
        '                ' shared styles marked AlwaysInclude
        '                '
        '                SQL = "select s.name,s.id,s.StyleFilename from ccSharedStyles s where (s.active<>0)and(s.AlwaysInclude<>0)"
        '                Dim dt As DataTable
        '                dt = cpCore.db.executeSql(SQL)
        '                If dt.Rows.Count > 0 Then
        '                    For Each row As DataRow In dt.Rows
        '                        styleId = genericController.EncodeInteger(row("id"))
        '                        StyleName = genericController.encodeText(row("name"))
        '                        StyleName = genericController.vbReplace(StyleName, "*/", "*-/")
        '                        If (InStr(1, usedSharedStyleList & ",", "," & styleId & ",") = 0) Then
        '                            usedSharedStyleList = usedSharedStyleList & "," & styleId
        '                            Filename = genericController.encodeText(row("stylefilename"))
        '                            If Filename <> "" Then
        '                                sharedStyles = sharedStyles _
        '                                    & vbCrLf & "/*" _
        '                                    & vbCrLf & "Shared Style " & StyleName & " marked always include" _
        '                                    & vbCrLf & "*/" _
        '                                    & vbCrLf & genericController.RemoveStyleTags(cpCore.cdnFiles.readFile(Filename))
        '                            End If
        '                        End If
        '                    Next
        '                End If
        '                '
        '                If templateId <> 0 Then
        '                    '
        '                    ' template exclusive styles
        '                    '
        '                    SQL = "select name,stylesFilename from cctemplates where (id=" & templateId & ")and(stylesFilename is not null)"
        '                    'Dim dt As DataTable
        '                    dt = cpCore.db.executeSql(SQL)
        '                    If dt.Rows.Count > 0 Then
        '                        For Each dr As DataRow In dt.Rows
        '                            Filename = genericController.encodeText(dr("stylesfilename"))
        '                            StyleName = genericController.encodeText(dr("name"))
        '                            StyleName = genericController.vbReplace(StyleName, "*/", "*-/")
        '                            If Filename <> "" Then
        '                                templateStyles = templateStyles _
        '                                    & vbCrLf & "/*" _
        '                                    & vbCrLf & "Template Styles" _
        '                                    & vbCrLf & "*/" _
        '                                    & vbCrLf & genericController.RemoveStyleTags(cpCore.cdnFiles.readFile(Filename))
        '                            End If

        '                        Next
        '                    End If
        '                    '
        '                    ' template shared styles
        '                    '
        '                    Dim rs As DataTable

        '                    SQL = "select s.name,s.id,s.StyleFilename from ccSharedStyles s left join ccSharedStylesTemplateRules r on s.id=r.styleid where (s.active<>0)and(r.templateid=" & templateId & ")and((s.AlwaysInclude=0)or(s.AlwaysInclude is null))"
        '                    rs = cpCore.db.executeSql(SQL)
        '                    If rs.Rows.Count > 0 Then
        '                        styleId = genericController.EncodeInteger(rs.Rows(0).Item("id"))
        '                        StyleName = genericController.encodeText(rs.Rows(0).Item("name"))
        '                        StyleName = genericController.vbReplace(StyleName, "*/", "*-/")
        '                        If (InStr(1, usedSharedStyleList & ",", "," & styleId & ",") = 0) Then
        '                            usedSharedStyleList = usedSharedStyleList & "," & styleId
        '                            Filename = genericController.encodeText(rs.Rows(0).Item("stylefilename"))
        '                            If Filename <> "" Then
        '                                sharedStyles = sharedStyles _
        '                                    & vbCrLf & "/*" _
        '                                    & vbCrLf & "Shared Style " & StyleName & " included in template" _
        '                                    & vbCrLf & "*/" _
        '                                    & vbCrLf & genericController.RemoveStyleTags(cpCore.cdnFiles.readFile(Filename))
        '                            End If
        '                        End If
        '                    End If
        '                End If
        '                '
        '                If EmailID <> 0 Then
        '                    '
        '                    ' email exclusive styles
        '                    '
        '                    SQL = "select name,blockSiteStyles,stylesFilename,emailTemplateID from ccemail where id=" & EmailID
        '                    'Dim dt As DataTable

        '                    dt = cpCore.db.executeSql(SQL)
        '                    If dt.Rows.Count > 0 Then
        '                        For Each rsDr As DataRow In dt.Rows
        '                            blockStyles = genericController.EncodeBoolean(rsDr("blockSiteStyles"))
        '                            If Not blockStyles Then
        '                                EMailTemplateID = genericController.EncodeInteger("EmailTemplateID")
        '                                Filename = genericController.encodeText(rsDr("stylesFilename"))
        '                                StyleName = genericController.encodeText(rsDr("name"))
        '                                StyleName = genericController.vbReplace(StyleName, "*/", "*-/")
        '                                If Filename <> "" Then
        '                                    emailstyles = emailstyles _
        '                                        & vbCrLf & "/*" _
        '                                        & vbCrLf & "Email Styles" _
        '                                        & vbCrLf & "*/" _
        '                                        & vbCrLf & genericController.RemoveStyleTags(cpCore.cdnFiles.readFile(Filename))
        '                                End If
        '                            End If
        '                        Next
        '                    End If
        '                    '
        '                    ' email shared styles
        '                    '
        '                    SQL = "select s.name,s.id,s.StyleFilename from ccSharedStyles s left join ccEmailStyleRules r on s.id=r.sharedstylesid where (s.active<>0)and(r.emailid=" & EmailID & ")and((s.AlwaysInclude=0)or(s.AlwaysInclude is null))"
        '                    dt = cpCore.db.executeSql(SQL)
        '                    For Each rsDr As DataRow In dt.Rows
        '                        styleId = genericController.EncodeInteger(rsDr("id"))
        '                        StyleName = genericController.encodeText(rsDr("name"))
        '                        StyleName = genericController.vbReplace(StyleName, "*/", "*-/")
        '                        If (InStr(1, usedSharedStyleList & ",", "," & styleId & ",") = 0) Then
        '                            usedSharedStyleList = usedSharedStyleList & "," & styleId
        '                            Filename = genericController.encodeText(rsDr("stylefilename"))
        '                            If Filename <> "" Then
        '                                sharedStyles = sharedStyles _
        '                                    & vbCrLf & "/*" _
        '                                    & vbCrLf & "Shared Styles included in email" _
        '                                    & vbCrLf & "*/" _
        '                                    & vbCrLf & genericController.RemoveStyleTags(cpCore.cdnFiles.readFile(Filename))
        '                            End If
        '                        End If
        '                    Next
        '                    '
        '                    If EMailTemplateID <> 0 Then
        '                        '
        '                        ' email templates do not have styles (yet, or not at all)
        '                        '
        '                    End If
        '                End If
        '                '
        '                ' assemble styles
        '                '
        '                If blockStyles Then
        '                    html_getStyleSheet2 = ""
        '                Else
        '                    html_getStyleSheet2 = "" _
        '                        & DefaultStyles _
        '                        & SiteStyles _
        '                        & sharedStyles _
        '                        & templateStyles _
        '                        & emailstyles
        '                    '
        '                    ' convert ccBodyWeb and ccBodyEmail to body tag on contentType
        '                    '
        '                End If
        '                '
        '                ' save it in cache in case there are >1 call on this page
        '                '
        '                Ptr = cpCore.stylesheetCacheCnt
        '                ReDim Preserve cpCore.stylesheetCache(Ptr)
        '                cpCore.stylesheetCacheCnt = cpCore.stylesheetCacheCnt + 1
        '                With cpCore.stylesheetCache(Ptr)
        '                    .EmailID = EmailID
        '                    .templateId = templateId
        '                    .StyleSheet = html_getStyleSheet2
        '                End With
        '            End If
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError4(Err.Number, Err.Source, Err.Description, "csv_getStyleSheet2", True, False)
        '        End Function
        ''
        ''
        ''
        'Public Function getStyleSheetDefault() As String
        '    '
        '    getStyleSheetDefault = ""
        '    If cpCore.siteProperties.getBoolean("Allow CSS Reset") Then
        '        getStyleSheetDefault = getStyleSheetDefault _
        '            & vbCrLf & "/*" _
        '            & vbCrLf & "Reset Styles" _
        '            & vbCrLf & "*/" _
        '            & vbCrLf & genericController.RemoveStyleTags(cpCore.appRootFiles.readFile("\cclib\styles\ccreset.css"))
        '    End If
        '    getStyleSheetDefault = getStyleSheetDefault _
        '        & vbCrLf & "/*" _
        '        & vbCrLf & "Contensive Styles" _
        '        & vbCrLf & "*/" _
        '        & vbCrLf & genericController.RemoveStyleTags(cpCore.appRootFiles.readFile("\cclib\styles\" & defaultStyleFilename))
        'End Function
        ''
        '
        '===============================================================================================================================
        '   Get Addon Selector
        '
        '   The addon selector is the string sent out with the content in edit-mode. In the editor, it is converted by javascript
        '   to the popup window that selects instance options. It is in this format:
        '
        '   Select (creates a list of names in a select box, returns the selected name)
        '       name=currentvalue[optionname0:optionvalue0|optionname1:optionvalue1|...]
        '   CheckBox (creates a list of names in checkboxes, and returns the selected names)
        '===============================================================================================================================
        '
        Public Function getAddonSelector(ByVal SrcOptionName As String, ByVal InstanceOptionValue_AddonEncoded As String, ByVal SrcOptionValueSelector As String) As String
            On Error GoTo ErrorTrap 'Const Tn = "GetAddonSelector" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            'ResultOptionSelector = csv_GetAddonSelector(SrcOptionName, InstanceOptionValue_AddonEncoded, SrcOptionValueSelector)
            '
            Const ACFunctionList = "List"
            Const ACFunctionList1 = "selectname"
            Const ACFunctionList2 = "listname"
            Const ACFunctionList3 = "selectcontentname"
            Const ACFunctionListID = "ListID"
            Const ACFunctionListFields = "ListFields"

            '
            Dim CID As Integer
            Dim IsContentList As Boolean
            Dim IsListField As Boolean
            Dim Choice As String
            Dim Choices() As String
            Dim ChoiceCnt As Integer
            Dim Ptr As Integer
            Dim IncludeID As Boolean
            Dim FnLen As Integer
            Dim RecordID As Integer
            Dim CS As Integer
            Dim ContentName As String
            Dim SrcOptionList As String
            Dim Pos As Integer
            Dim list As String
            Dim FnArgList As String
            Dim FnArgs() As String
            Dim FnArgCnt As Integer
            Dim ContentCriteria As String
            Dim RecordName As String
            Dim REsultOptionValue As String
            Dim ResultOptionListHTMLEncoded As String
            Dim SrcSelectorInner As String
            Dim FunctionListNames As String
            Dim SrcSelectorSuffix As String = String.Empty
            Dim Cell(,) As Object
            Dim RowCnt As Integer
            Dim RowPtr As Integer
            'Dim Ptr as integer
            '
            Dim SrcSelector As String
            SrcSelector = Trim(SrcOptionValueSelector)
            '
            SrcSelectorInner = SrcSelector
            Dim PosLeft As Integer
            Dim PosRight As Integer
            PosLeft = genericController.vbInstr(1, SrcSelector, "[")
            If PosLeft <> 0 Then
                PosRight = genericController.vbInstr(1, SrcSelector, "]")
                If PosRight <> 0 Then
                    If (PosRight < Len(SrcSelector)) Then
                        SrcSelectorSuffix = Mid(SrcSelector, PosRight + 1)
                    End If
                    SrcSelector = Trim(Mid(SrcSelector, PosLeft, PosRight - PosLeft + 1))
                    SrcSelectorInner = Trim(Mid(SrcSelector, 2, Len(SrcSelector) - 2))
                End If
            End If
            list = ""
            '
            ' Break SrcSelectorInner up into individual choices to detect functions
            '

            If SrcSelectorInner <> "" Then
                Choices = Split(SrcSelectorInner, "|")
                ChoiceCnt = UBound(Choices) + 1
                For Ptr = 0 To ChoiceCnt - 1
                    Choice = Choices(Ptr)
                    IsContentList = False
                    IsListField = False
                    '
                    ' List Function (and all the indecision that went along with it)
                    '
                    Pos = 0
                    If Pos = 0 Then
                        Pos = genericController.vbInstr(1, Choice, ACFunctionList1 & "(", vbTextCompare)
                        If Pos > 0 Then
                            IsContentList = True
                            IncludeID = False
                            FnLen = Len(ACFunctionList1)
                        End If
                    End If
                    If Pos = 0 Then
                        Pos = genericController.vbInstr(1, Choice, ACFunctionList2 & "(", vbTextCompare)
                        If Pos > 0 Then
                            IsContentList = True
                            IncludeID = False
                            FnLen = Len(ACFunctionList2)
                        End If
                    End If
                    If Pos = 0 Then
                        Pos = genericController.vbInstr(1, Choice, ACFunctionList3 & "(", vbTextCompare)
                        If Pos > 0 Then
                            IsContentList = True
                            IncludeID = False
                            FnLen = Len(ACFunctionList3)
                        End If
                    End If
                    If Pos = 0 Then
                        Pos = genericController.vbInstr(1, Choice, ACFunctionListID & "(", vbTextCompare)
                        If Pos > 0 Then
                            IsContentList = True
                            IncludeID = True
                            FnLen = Len(ACFunctionListID)
                        End If
                    End If
                    If Pos = 0 Then
                        Pos = genericController.vbInstr(1, Choice, ACFunctionList & "(", vbTextCompare)
                        If Pos > 0 Then
                            IsContentList = True
                            IncludeID = False
                            FnLen = Len(ACFunctionList)
                        End If
                    End If
                    If Pos = 0 Then
                        Pos = genericController.vbInstr(1, Choice, ACFunctionListFields & "(", vbTextCompare)
                        If Pos > 0 Then
                            IsListField = True
                            IncludeID = False
                            FnLen = Len(ACFunctionListFields)
                        End If
                    End If
                    '
                    If Pos > 0 Then
                        '
                        FnArgList = Trim(Mid(Choice, Pos + FnLen))
                        ContentName = ""
                        ContentCriteria = ""
                        If (Left(FnArgList, 1) = "(") And (Right(FnArgList, 1) = ")") Then
                            '
                            ' set ContentName and ContentCriteria from argument list
                            '
                            FnArgList = Mid(FnArgList, 2, Len(FnArgList) - 2)
                            FnArgs = genericController.SplitDelimited(FnArgList, ",")
                            FnArgCnt = UBound(FnArgs) + 1
                            If FnArgCnt > 0 Then
                                ContentName = Trim(FnArgs(0))
                                If (Left(ContentName, 1) = """") And (Right(ContentName, 1) = """") Then
                                    ContentName = Trim(Mid(ContentName, 2, Len(ContentName) - 2))
                                ElseIf (Left(ContentName, 1) = "'") And (Right(ContentName, 1) = "'") Then
                                    ContentName = Trim(Mid(ContentName, 2, Len(ContentName) - 2))
                                End If
                            End If
                            If FnArgCnt > 1 Then
                                ContentCriteria = Trim(FnArgs(1))
                                If (Left(ContentCriteria, 1) = """") And (Right(ContentCriteria, 1) = """") Then
                                    ContentCriteria = Trim(Mid(ContentCriteria, 2, Len(ContentCriteria) - 2))
                                ElseIf (Left(ContentCriteria, 1) = "'") And (Right(ContentCriteria, 1) = "'") Then
                                    ContentCriteria = Trim(Mid(ContentCriteria, 2, Len(ContentCriteria) - 2))
                                End If
                            End If
                        End If
                        CS = -1
                        If IsContentList Then
                            '
                            ' ContentList - Open the Content and build the options from the names
                            '
                            If ContentCriteria <> "" Then
                                CS = cpCore.db.cs_open(ContentName, ContentCriteria, "name", , , , , "ID,Name")
                            Else
                                CS = cpCore.db.cs_open(ContentName, , "name", , , , , "ID,Name")
                            End If
                        ElseIf IsListField Then
                            '
                            ' ListField
                            '
                            CID = cpCore.metaData.getContentId(ContentName)
                            If CID > 0 Then
                                CS = cpCore.db.cs_open("Content Fields", "Contentid=" & CID, "name", , , , , "ID,Name")
                            End If
                        End If

                        If cpCore.db.cs_ok(CS) Then
                            Cell = cpCore.db.cs_getRows(CS)
                            RowCnt = UBound(Cell, 2) + 1
                            For RowPtr = 0 To RowCnt - 1
                                '
                                RecordName = genericController.encodeText(Cell(1, RowPtr))
                                RecordName = genericController.vbReplace(RecordName, vbCrLf, " ")
                                RecordID = genericController.EncodeInteger(Cell(0, RowPtr))
                                If RecordName = "" Then
                                    RecordName = "record " & RecordID
                                ElseIf Len(RecordName) > 50 Then
                                    RecordName = Left(RecordName, 50) & "..."
                                End If
                                RecordName = genericController.encodeNvaArgument(RecordName)
                                list = list & "|" & RecordName
                                If IncludeID Then
                                    list = list & ":" & RecordID
                                End If
                            Next
                        End If
                        Call cpCore.db.cs_Close(CS)
                    Else
                        '
                        ' choice is not a function, just add the choice back to the list
                        '
                        list = list & "|" & Choices(Ptr)
                    End If
                Next
                If list <> "" Then
                    list = Mid(list, 2)
                End If
            End If
            '
            ' Build output string
            '
            'csv_GetAddonSelector = encodeNvaArgument(SrcOptionName)
            getAddonSelector = encodeHTML(genericController.encodeNvaArgument(SrcOptionName)) & "="
            If InstanceOptionValue_AddonEncoded <> "" Then
                getAddonSelector = getAddonSelector & encodeHTML(InstanceOptionValue_AddonEncoded)
            End If
            If SrcSelectorSuffix = "" And list = "" Then
                '
                ' empty list with no suffix, return with name=value
                '
            ElseIf genericController.vbLCase(SrcSelectorSuffix) = "resourcelink" Then
                '
                ' resource link, exit with empty list
                '
                getAddonSelector = getAddonSelector & "[]ResourceLink"
            Else
                '
                '
                '
                getAddonSelector = getAddonSelector & "[" & list & "]" & SrcSelectorSuffix
            End If
            '
            Exit Function
            '
ErrorTrap:
            cpCore.handleException(New Exception("Unexpected exception"))
        End Function
        '
        '========================================================================
        '   Compatibility
        '========================================================================
        '
        Public Function html_GetFormInputHTML(ByVal TagName As String, Optional ByVal DefaultValue As String = "", Optional ByVal Height As String = "", Optional ByVal Width As String = "") As String
            html_GetFormInputHTML = html_GetFormInputHTML3(genericController.encodeText(TagName), genericController.encodeText(DefaultValue), genericController.encodeText(Height), genericController.encodeText(Width))
        End Function
        '
        '========================================================================
        ' ----- main_Get an HTML Form text input (or text area)
        '========================================================================
        '
        Public Function html_GetFormInputHTML3(ByVal htmlName As String, Optional ByVal DefaultValue As String = "", Optional ByVal styleHeight As String = "", Optional ByVal styleWidth As String = "", Optional ByVal readOnlyfield As Boolean = False, Optional ByVal allowActiveContent As Boolean = False, Optional ByVal addonListJSON As String = "", Optional ByVal styleList As String = "", Optional ByVal styleOptionList As String = "", Optional ByVal allowResourceLibrary As Boolean = False) As String
            Dim returnHtml As String = ""
            Try
                Dim FieldTypeDefaultEditorAddonIdList As String
                Dim FieldTypeDefaultEditorAddonIds() As String
                Dim addonOption_String As String
                Dim FieldTypeDefaultEditorAddonId As Integer
                '
                FieldTypeDefaultEditorAddonIdList = editorController.getFieldTypeDefaultEditorAddonIdList(cpCore)
                FieldTypeDefaultEditorAddonIds = Split(FieldTypeDefaultEditorAddonIdList, ",")
                FieldTypeDefaultEditorAddonId = genericController.EncodeInteger(FieldTypeDefaultEditorAddonIds(FieldTypeIdHTML))

                If FieldTypeDefaultEditorAddonId = 0 Then
                    '
                    '    use default wysiwyg
                    '
                    returnHtml = html_GetFormInputTextExpandable2(htmlName, DefaultValue)
                Else
                    '
                    ' use addon editor
                    '
                    addonOption_String = "" _
                        & "editorName=" & genericController.encodeNvaArgument(htmlName) _
                        & "&editorValue=" & genericController.encodeNvaArgument(DefaultValue) _
                        & "&editorFieldType=" & FieldTypeIdHTML _
                        & "&editorReadOnly=" & readOnlyfield _
                        & "&editorWidth=" & styleWidth _
                        & "&editorHeight=" & styleHeight _
                        & ""
                    addonOption_String = addonOption_String _
                        & "&editorAllowResourceLibrary=" & genericController.encodeNvaArgument(CStr(allowResourceLibrary)) _
                        & "&editorAllowActiveContent=" & genericController.encodeNvaArgument(CStr(allowActiveContent)) _
                        & "&editorAddonList=" & genericController.encodeNvaArgument(addonListJSON) _
                        & "&editorStyles=" & genericController.encodeNvaArgument(styleList) _
                        & "&editorStyleOptions=" & genericController.encodeNvaArgument(styleOptionList) _
                        & ""
                    returnHtml = cpCore.addon.execute_legacy4(FieldTypeDefaultEditorAddonId.ToString, addonOption_String, CPUtilsBaseClass.addonContext.ContextEditor)
                End If

            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return returnHtml
        End Function
        '
        '========================================================================
        ' ----- Process the reply from the Tools Panel form
        '========================================================================
        '
        Public Sub processFormToolsPanel(Optional legacyFormSn As String = "")
            Try
                Dim Button As String
                Dim username As String
                '
                ' ----- Read in and save the Member profile values from the tools panel
                '
                If (cpCore.authContext.user.id > 0) Then
                    If Not (cpCore.debug_iUserError <> "") Then
                        Button = cpCore.docProperties.getText(legacyFormSn & "mb")
                        Select Case Button
                            Case ButtonLogout
                                '
                                ' Logout - This can only come from the Horizonal Tool Bar
                                '
                                Call cpCore.authContext.logout(cpCore)
                            Case ButtonLogin
                                '
                                ' Login - This can only come from the Horizonal Tool Bar
                                '
                                Dim loginAddon As New Addons.loginPageClass(cpCore)
                                Call loginAddon.processFormLoginDefault()
                            Case ButtonApply
                                '
                                ' Apply
                                '
                                username = cpCore.docProperties.getText(legacyFormSn & "username")
                                If username <> "" Then
                                    Dim loginAddon As New Addons.loginPageClass(cpCore)
                                    Call loginAddon.processFormLoginDefault()
                                End If
                                '
                                ' ----- AllowAdminLinks
                                '
                                Call cpCore.visitProperty.setProperty("AllowEditing", genericController.encodeText(cpCore.docProperties.getBoolean(legacyFormSn & "AllowEditing")))
                                '
                                ' ----- Quick Editor
                                '
                                Call cpCore.visitProperty.setProperty("AllowQuickEditor", genericController.encodeText(cpCore.docProperties.getBoolean(legacyFormSn & "AllowQuickEditor")))
                                '
                                ' ----- Advanced Editor
                                '
                                Call cpCore.visitProperty.setProperty("AllowAdvancedEditor", genericController.encodeText(cpCore.docProperties.getBoolean(legacyFormSn & "AllowAdvancedEditor")))
                                '
                                ' ----- Allow Workflow authoring Render Mode - Visit Property
                                '
                                Call cpCore.visitProperty.setProperty("AllowWorkflowRendering", genericController.encodeText(cpCore.docProperties.getBoolean(legacyFormSn & "AllowWorkflowRendering")))
                                '
                                ' ----- developer Only parts
                                '
                                Call cpCore.visitProperty.setProperty("AllowDebugging", genericController.encodeText(cpCore.docProperties.getBoolean(legacyFormSn & "AllowDebugging")))
                        End Select
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '========================================================================
        ' -----
        '========================================================================
        '
        Public Sub processAddonSettingsEditor()
            '
            Dim constructor As String
            Dim ParseOK As Boolean
            Dim PosNameStart As Integer
            Dim PosNameEnd As Integer
            Dim AddonName As String
            'Dim CSAddon As Integer
            Dim OptionPtr As Integer
            Dim ArgValueAddonEncoded As String
            Dim OptionCnt As Integer
            Dim needToClearCache As Boolean
            Dim ConstructorSplit() As String
            Dim Ptr As Integer
            Dim Arg() As String
            Dim ArgName As String
            Dim ArgValue As String
            Dim AddonOptionConstructor As String = String.Empty
            Dim addonOption_String As String = String.Empty
            Dim fieldType As Integer
            Dim Copy As String = String.Empty
            Dim MethodName As String
            Dim RecordID As Integer
            Dim FieldName As String
            Dim ACInstanceID As String
            Dim ContentName As String
            Dim CS As Integer
            Dim PosACInstanceID As Integer
            Dim PosStart As Integer
            Dim PosIDStart As Integer
            Dim PosIDEnd As Integer
            '
            MethodName = "main_ProcessAddonSettingsEditor()"
            '
            ContentName = cpCore.docProperties.getText("ContentName")
            RecordID = cpCore.docProperties.getInteger("RecordID")
            FieldName = cpCore.docProperties.getText("FieldName")
            ACInstanceID = cpCore.docProperties.getText("ACInstanceID")
            Dim FoundAddon As Boolean = False
            If (ACInstanceID = PageChildListInstanceID) Then
                '
                ' ----- Page Content Child List Add-on
                '
                If (RecordID <> 0) Then
                    Dim addon As addonModel = cpCore.addonCache.getAddonById(cpCore.siteProperties.childListAddonID)
                    If (addon IsNot Nothing) Then
                        FoundAddon = True
                        AddonOptionConstructor = addon.ArgumentList
                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCrLf, vbCr)
                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbLf, vbCr)
                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCr, vbCrLf)
                        If True Then
                            If AddonOptionConstructor <> "" Then
                                AddonOptionConstructor = AddonOptionConstructor & vbCrLf
                            End If
                            If addon.IsInline Then
                                AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_Inline
                            Else
                                AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_Block
                            End If
                        End If

                        ConstructorSplit = Split(AddonOptionConstructor, vbCrLf)
                        AddonOptionConstructor = ""
                        '
                        ' main_Get all responses from current Argument List and build new addonOption_String
                        '
                        For Ptr = 0 To UBound(ConstructorSplit)
                            Arg = Split(ConstructorSplit(Ptr), "=")
                            ArgName = Arg(0)
                            OptionCnt = cpCore.docProperties.getInteger(ArgName & "CheckBoxCnt")
                            If OptionCnt > 0 Then
                                ArgValueAddonEncoded = ""
                                For OptionPtr = 0 To OptionCnt - 1
                                    ArgValue = cpCore.docProperties.getText(ArgName & OptionPtr)
                                    If ArgValue <> "" Then
                                        ArgValueAddonEncoded = ArgValueAddonEncoded & "," & genericController.encodeNvaArgument(ArgValue)
                                    End If
                                Next
                                If ArgValueAddonEncoded <> "" Then
                                    ArgValueAddonEncoded = Mid(ArgValueAddonEncoded, 2)
                                End If
                            Else
                                ArgValue = cpCore.docProperties.getText(ArgName)
                                ArgValueAddonEncoded = genericController.encodeNvaArgument(ArgValue)
                            End If
                            addonOption_String = addonOption_String & "&" & genericController.encodeNvaArgument(ArgName) & "=" & ArgValueAddonEncoded
                        Next
                        If addonOption_String <> "" Then
                            addonOption_String = Mid(addonOption_String, 2)
                        End If

                    End If
                    Call cpCore.db.executeSql("update ccpagecontent set ChildListInstanceOptions=" & cpCore.db.encodeSQLText(addonOption_String) & " where id=" & RecordID)
                    needToClearCache = True
                    'CS = main_OpenCSContentRecord("page content", RecordID)
                    'If app.csv_IsCSOK(CS) Then
                    '    Call app.SetCS(CS, "ChildListInstanceOptions", addonOption_String)
                    '    needToClearCache = True
                    'End If
                    'Call app.closeCS(CS)
                End If
            ElseIf (ACInstanceID = "-2") And (FieldName <> "") Then
                '
                ' ----- Admin Addon, ACInstanceID=-2, FieldName=AddonName
                '
                AddonName = FieldName
                FoundAddon = False
                Dim addon As addonModel = cpCore.addonCache.getAddonByName(AddonName)
                If (addon IsNot Nothing) Then
                    FoundAddon = True
                    AddonOptionConstructor = addon.ArgumentList
                    AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCrLf, vbCr)
                    AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbLf, vbCr)
                    AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCr, vbCrLf)
                    If AddonOptionConstructor <> "" Then
                        AddonOptionConstructor = AddonOptionConstructor & vbCrLf
                    End If
                    If genericController.EncodeBoolean(addon.IsInline) Then
                        AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_Inline
                    Else
                        AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_Block
                    End If
                End If
                If Not FoundAddon Then
                    '
                    ' Hardcoded Addons
                    '
                    Select Case genericController.vbLCase(AddonName)
                        Case "block text"
                            FoundAddon = True
                            AddonOptionConstructor = AddonOptionConstructor_ForBlockText
                        Case ""
                    End Select
                End If
                If FoundAddon Then
                    ConstructorSplit = Split(AddonOptionConstructor, vbCrLf)
                    addonOption_String = ""
                    '
                    ' main_Get all responses from current Argument List
                    '
                    For Ptr = 0 To UBound(ConstructorSplit)
                        Dim nvp As String
                        nvp = Trim(ConstructorSplit(Ptr))
                        If nvp <> "" Then
                            Arg = Split(ConstructorSplit(Ptr), "=")
                            ArgName = Arg(0)
                            OptionCnt = cpCore.docProperties.getInteger(ArgName & "CheckBoxCnt")
                            If OptionCnt > 0 Then
                                ArgValueAddonEncoded = ""
                                For OptionPtr = 0 To OptionCnt - 1
                                    ArgValue = cpCore.docProperties.getText(ArgName & OptionPtr)
                                    If ArgValue <> "" Then
                                        ArgValueAddonEncoded = ArgValueAddonEncoded & "," & genericController.encodeNvaArgument(ArgValue)
                                    End If
                                Next
                                If ArgValueAddonEncoded <> "" Then
                                    ArgValueAddonEncoded = Mid(ArgValueAddonEncoded, 2)
                                End If
                            Else
                                ArgValue = cpCore.docProperties.getText(ArgName)
                                ArgValueAddonEncoded = genericController.encodeNvaArgument(ArgValue)
                            End If
                            addonOption_String = addonOption_String & "&" & genericController.encodeNvaArgument(ArgName) & "=" & ArgValueAddonEncoded
                        End If
                    Next
                    If addonOption_String <> "" Then
                        addonOption_String = Mid(addonOption_String, 2)
                    End If
                    Call cpCore.userProperty.setProperty("Addon [" & AddonName & "] Options", addonOption_String)
                    needToClearCache = True
                End If
            ElseIf ContentName = "" Or RecordID = 0 Then
                '
                ' ----- Public Site call, must have contentname and recordid
                '
                cpCore.handleException(New Exception("invalid content [" & ContentName & "], RecordID [" & RecordID & "]"))
            Else
                '
                ' ----- Normal Content Edit - find instance in the content
                '
                CS = cpCore.db.csOpenRecord(ContentName, RecordID)
                If Not cpCore.db.cs_ok(CS) Then
                    cpCore.handleException(New Exception("No record found with content [" & ContentName & "] and RecordID [" & RecordID & "]"))
                Else
                    If FieldName <> "" Then
                        '
                        ' Field is given, find the position
                        '
                        Copy = cpCore.db.cs_get(CS, FieldName)
                        PosACInstanceID = genericController.vbInstr(1, Copy, "=""" & ACInstanceID & """ ", vbTextCompare)
                    Else
                        '
                        ' Find the field, then find the position
                        '
                        FieldName = cpCore.db.cs_getFirstFieldName(CS)
                        Do While FieldName <> ""
                            fieldType = cpCore.db.cs_getFieldTypeId(CS, FieldName)
                            Select Case fieldType
                                Case FieldTypeIdLongText, FieldTypeIdText, FieldTypeIdFileTextPrivate, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript, FieldTypeIdHTML, FieldTypeIdFileHTMLPrivate
                                    Copy = cpCore.db.cs_get(CS, FieldName)
                                    PosACInstanceID = genericController.vbInstr(1, Copy, "ACInstanceID=""" & ACInstanceID & """", vbTextCompare)
                                    If PosACInstanceID <> 0 Then
                                        '
                                        ' found the instance
                                        '
                                        PosACInstanceID = PosACInstanceID + 13
                                        Exit Do
                                    End If
                            End Select
                            FieldName = cpCore.db.cs_getNextFieldName(CS)
                        Loop
                    End If
                    '
                    ' Parse out the Addon Name
                    '
                    If PosACInstanceID = 0 Then
                        cpCore.handleException(New Exception("AC Instance [" & ACInstanceID & "] not found in record with content [" & ContentName & "] and RecordID [" & RecordID & "]"))
                    Else
                        Copy = html_EncodeContentUpgrades(Copy)
                        ParseOK = False
                        PosStart = InStrRev(Copy, "<ac ", PosACInstanceID, vbTextCompare)
                        If PosStart <> 0 Then
                            '
                            ' main_Get Addon Name to lookup Addon and main_Get most recent Argument List
                            '
                            PosNameStart = genericController.vbInstr(PosStart, Copy, " name=", vbTextCompare)
                            If PosNameStart <> 0 Then
                                PosNameStart = PosNameStart + 7
                                PosNameEnd = genericController.vbInstr(PosNameStart, Copy, """")
                                If PosNameEnd <> 0 Then
                                    AddonName = Mid(Copy, PosNameStart, PosNameEnd - PosNameStart)
                                    '????? test this
                                    FoundAddon = False
                                    Dim embeddedAddon As addonModel = cpCore.addonCache.getAddonByName(AddonName)
                                    If (embeddedAddon IsNot Nothing) Then
                                        FoundAddon = True
                                        AddonOptionConstructor = genericController.encodeText(embeddedAddon.ArgumentList)
                                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCrLf, vbCr)
                                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbLf, vbCr)
                                        AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCr, vbCrLf)
                                        If AddonOptionConstructor <> "" Then
                                            AddonOptionConstructor = AddonOptionConstructor & vbCrLf
                                        End If
                                        If genericController.EncodeBoolean(embeddedAddon.IsInline) Then
                                            AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_Inline
                                        Else
                                            AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_Block
                                        End If
                                    Else
                                        '
                                        ' -- Hardcoded Addons
                                        Select Case genericController.vbLCase(AddonName)
                                            Case "block text"
                                                FoundAddon = True
                                                AddonOptionConstructor = AddonOptionConstructor_ForBlockText
                                            Case ""
                                        End Select
                                    End If
                                    If FoundAddon Then
                                        ConstructorSplit = Split(AddonOptionConstructor, vbCrLf)
                                        addonOption_String = ""
                                        '
                                        ' main_Get all responses from current Argument List
                                        '
                                        For Ptr = 0 To UBound(ConstructorSplit)
                                            constructor = ConstructorSplit(Ptr)
                                            If constructor <> "" Then
                                                Arg = Split(constructor, "=")
                                                ArgName = Arg(0)
                                                OptionCnt = cpCore.docProperties.getInteger(ArgName & "CheckBoxCnt")
                                                If OptionCnt > 0 Then
                                                    ArgValueAddonEncoded = ""
                                                    For OptionPtr = 0 To OptionCnt - 1
                                                        ArgValue = cpCore.docProperties.getText(ArgName & OptionPtr)
                                                        If ArgValue <> "" Then
                                                            ArgValueAddonEncoded = ArgValueAddonEncoded & "," & genericController.encodeNvaArgument(ArgValue)
                                                        End If
                                                    Next
                                                    If ArgValueAddonEncoded <> "" Then
                                                        ArgValueAddonEncoded = Mid(ArgValueAddonEncoded, 2)
                                                    End If
                                                Else
                                                    ArgValue = cpCore.docProperties.getText(ArgName)
                                                    ArgValueAddonEncoded = genericController.encodeNvaArgument(ArgValue)
                                                End If

                                                addonOption_String = addonOption_String & "&" & genericController.encodeNvaArgument(ArgName) & "=" & ArgValueAddonEncoded
                                            End If
                                        Next
                                        If addonOption_String <> "" Then
                                            addonOption_String = Mid(addonOption_String, 2)
                                        End If
                                    End If
                                End If
                            End If
                            '
                            ' Replace the new querystring into the AC tag in the content
                            '
                            PosIDStart = genericController.vbInstr(PosStart, Copy, " querystring=", vbTextCompare)
                            If PosIDStart <> 0 Then
                                PosIDStart = PosIDStart + 14
                                If PosIDStart <> 0 Then
                                    PosIDEnd = genericController.vbInstr(PosIDStart, Copy, """")
                                    If PosIDEnd <> 0 Then
                                        ParseOK = True
                                        Copy = Mid(Copy, 1, PosIDStart - 1) & encodeHTML(addonOption_String) & Mid(Copy, PosIDEnd)
                                        Call cpCore.db.cs_set(CS, FieldName, Copy)
                                        needToClearCache = True
                                    End If
                                End If
                            End If
                        End If
                        If Not ParseOK Then
                            cpCore.handleException(New Exception("There was a problem parsing AC Instance [" & ACInstanceID & "] record with content [" & ContentName & "] and RecordID [" & RecordID & "]"))
                        End If
                    End If
                End If
                Call cpCore.db.cs_Close(CS)
            End If
            If needToClearCache Then
                '
                ' Clear Caches
                '
                If ContentName <> "" Then
                    Call cpCore.cache.invalidateContent(ContentName)
                End If
            End If
        End Sub
        '
        '========================================================================
        ' ----- Process the little edit form in the help bubble
        '========================================================================
        '
        Public Sub processHelpBubbleEditor()
            '
            Dim SQL As String
            Dim MethodName As String
            Dim HelpBubbleID As String
            Dim IDSplit() As String
            Dim RecordID As Integer
            Dim HelpCaption As String
            Dim HelpMessage As String
            '
            MethodName = "main_ProcessHelpBubbleEditor()"
            '
            HelpBubbleID = cpCore.docProperties.getText("HelpBubbleID")
            IDSplit = Split(HelpBubbleID, "-")
            Select Case genericController.vbLCase(IDSplit(0))
                Case "userfield"
                    '
                    ' main_Get the id of the field, and save the input as the caption and help
                    '
                    If UBound(IDSplit) > 0 Then
                        RecordID = genericController.EncodeInteger(IDSplit(1))
                        If RecordID > 0 Then
                            HelpCaption = cpCore.docProperties.getText("helpcaption")
                            HelpMessage = cpCore.docProperties.getText("helptext")
                            SQL = "update ccfields set caption=" & cpCore.db.encodeSQLText(HelpCaption) & ",HelpMessage=" & cpCore.db.encodeSQLText(HelpMessage) & " where id=" & RecordID
                            Call cpCore.db.executeSql(SQL)
                            cpCore.cache.invalidateAll()
                            cpCore.metaData.clear()
                        End If
                    End If
            End Select
        End Sub
        '
        '====================================================================================================
        '   encode content moved to csv so support cp.content.GetCopy()
        '====================================================================================================
        '
        Public Function encodeContent9(Source As String, personalizationPeopleId As Integer, ContextContentName As String, ContextRecordID As Integer, ContextContactPeopleID As Integer, PlainText As Boolean, AddLinkEID As Boolean, EncodeActiveFormatting As Boolean, EncodeActiveImages As Boolean, EncodeActiveEditIcons As Boolean, EncodeActivePersonalization As Boolean, AddAnchorQuery As String, ProtocolHostString As String, IsEmailContent As Boolean, DefaultWrapperID As Integer, ignore_TemplateCaseOnly_Content As String, addonContext As CPUtilsBaseClass.addonContext) As String
            Return encodeContent10(Source, personalizationPeopleId, ContextContentName, ContextRecordID, ContextContactPeopleID, PlainText, AddLinkEID, EncodeActiveFormatting, EncodeActiveImages, EncodeActiveEditIcons, EncodeActivePersonalization, AddAnchorQuery, ProtocolHostString, IsEmailContent, DefaultWrapperID, ignore_TemplateCaseOnly_Content, addonContext, cpCore.authContext.isAuthenticated, Nothing, cpCore.authContext.isEditingAnything())
        End Function
        '
        '========================================================================
        ' ----- Encode Active Content AI
        '========================================================================
        '
        Public Function decodeContent(ByVal Source As String) As String
            Return renderActiveContent(genericController.encodeText(Source))
        End Function
        '
        '==========================================================================================================================================
        '   Encode Content call for preparing content for display on the web page
        '       BasePath is for a future addition:
        '           Each page will have its own URLName. The URLName is the part of the URL that points to this page.
        '           If an aggr object is on a page, and it offers variations depending on a QS value, the BasePath is like
        '           the main_RefreshQueryString -- it is the path that main_Gets you back here. This needs to be passed into the objects
        '           so an object can call encodecontentforweb.
        '==========================================================================================================================================
        '
        Public Function encodeContentForWeb(Source As String, ContextContentName As String, ContextRecordID As Integer, Ignore_BasePath As String, WrapperID As Integer) As String
            Return encodeContent9(Source, cpCore.authContext.user.id, ContextContentName, ContextRecordID, 0, False, False, True, True, False, True, "", "", False, WrapperID, "", CPUtilsBaseClass.addonContext.ContextPage)
        End Function
        '
        '========================================================================
        ' Print the Member Edit form
        '
        '   For instance, list out a checklist of all public groups, with the ones checked that this member belongs to
        '       PrimaryContentName = "People"
        '       PrimaryRecordID = MemberID
        '       SecondaryContentName = "Groups"
        '       SecondaryContentSelectCriteria = "ccGroups.PublicJoin<>0"
        '       RulesContentName = "Member Rules"
        '       RulesPrimaryFieldName = "MemberID"
        '       RulesSecondaryFieldName = "GroupID"
        '========================================================================
        '
        Public Function getINputChecList2(ByVal TagName As String, ByVal PrimaryContentName As String, ByVal PrimaryRecordID As Integer, ByVal SecondaryContentName As String, ByVal RulesContentName As String, ByVal RulesPrimaryFieldname As String, ByVal RulesSecondaryFieldName As String, Optional ByVal SecondaryContentSelectCriteria As String = "", Optional ByVal CaptionFieldName As String = "", Optional ByVal readOnlyfield As Boolean = False) As String
            getINputChecList2 = getInputCheckList(TagName, PrimaryContentName, PrimaryRecordID, SecondaryContentName, RulesContentName, RulesPrimaryFieldname, RulesSecondaryFieldName, SecondaryContentSelectCriteria, genericController.encodeText(CaptionFieldName), readOnlyfield, False, "")
        End Function
        '
        '========================================================================
        '   main_Get a list of checkbox options based on a standard set of rules
        '
        '   IncludeContentFolderDivs
        '       When true, the list of options (checkboxes) are grouped by ContentFolder and wrapped in a Div with ID="ContentFolder99"
        '
        '   For instance, list out a options of all public groups, with the ones checked that this member belongs to
        '       PrimaryContentName = "People"
        '       PrimaryRecordID = MemberID
        '       SecondaryContentName = "Groups"
        '       SecondaryContentSelectCriteria = "ccGroups.PublicJoin<>0"
        '       RulesContentName = "Member Rules"
        '       RulesPrimaryFieldName = "MemberID"
        '       RulesSecondaryFieldName = "GroupID"
        '========================================================================
        '
        Public Function getInputCheckList(ByVal TagName As String, ByVal PrimaryContentName As String, ByVal PrimaryRecordID As Integer, ByVal SecondaryContentName As String, ByVal RulesContentName As String, ByVal RulesPrimaryFieldname As String, ByVal RulesSecondaryFieldName As String, Optional ByVal SecondaryContentSelectCriteria As String = "", Optional ByVal CaptionFieldName As String = "", Optional ByVal readOnlyfield As Boolean = False, Optional ByVal IncludeContentFolderDivs As Boolean = False, Optional ByVal DefaultSecondaryIDList As String = "") As String
            Dim returnHtml As String = ""
            Try
                Dim main_MemberShipText() As String
                Dim Ptr As Integer
                Dim main_MemberShipID As Integer
                Dim javaScriptRequired As String = ""
                Dim ContentFolderName As String
                Dim DivName As String
                Dim DivCnt As Integer
                Dim DivID As String
                Dim OldFolderVar As String
                Dim EndDiv As String
                Dim OpenFolderID As Integer
                Dim CurrentFolderID As Integer
                Dim IsContentCategoriesSupported As Boolean
                Dim RuleCopyCaption As String
                Dim RuleCopy As String
                Dim SQL As String
                Dim CS As Integer
                Dim main_MemberShipCount As Integer
                Dim main_MemberShipSize As Integer
                Dim main_MemberShipPointer As Integer
                Dim SectionName As String
                Dim CheckBoxCnt As Integer
                Dim DivCheckBoxCnt As Integer
                Dim main_MemberShip As Integer() = {}
                Dim main_MemberShipRuleCopy As String() = {}
                Dim PrimaryContentID As Integer
                Dim SecondaryTablename As String
                Dim SecondaryContentID As Integer
                Dim rulesTablename As String
                Dim OptionName As String
                Dim OptionCaption As String
                Dim optionCaptionHtmlEncoded As String
                Dim CanSeeHiddenFields As Boolean
                Dim SecondaryCDef As cdefModel
                Dim ContentIDList As New List(Of Integer)
                Dim Found As Boolean
                Dim RecordID As Integer
                Dim SingularPrefixHtmlEncoded As String
                Dim IsRuleCopySupported As Boolean
                Dim AllowRuleCopy As Boolean
                '
                ' IsContentCategoriesSupported - if true, break checkboxes out into divs for each Content Category
                '
                IsContentCategoriesSupported = IncludeContentFolderDivs
                If IsContentCategoriesSupported Then
                    IsContentCategoriesSupported = cpCore.metaData.isContentFieldSupported(SecondaryContentName, "ContentCategoryID")
                End If
                '
                ' IsRuleCopySupported - if true, the rule records include an allow button, and copy
                '   This is for a checkbox like [ ] Other [enter other copy here]
                '
                IsRuleCopySupported = cpCore.metaData.isContentFieldSupported(RulesContentName, "RuleCopy")
                If IsRuleCopySupported Then
                    IsRuleCopySupported = IsRuleCopySupported And cpCore.metaData.isContentFieldSupported(SecondaryContentName, "AllowRuleCopy")
                    If IsRuleCopySupported Then
                        IsRuleCopySupported = IsRuleCopySupported And cpCore.metaData.isContentFieldSupported(SecondaryContentName, "RuleCopyCaption")
                    End If
                End If
                If CaptionFieldName = "" Then
                    CaptionFieldName = "name"
                End If
                CaptionFieldName = genericController.encodeEmptyText(CaptionFieldName, "name")
                If PrimaryContentName = "" Or SecondaryContentName = "" Or RulesContentName = "" Or RulesPrimaryFieldname = "" Or RulesSecondaryFieldName = "" Then
                    returnHtml = "[Checklist not configured]"
                    cpCore.handleException(New Exception("Creating checklist, all required fields were not supplied, Caption=[" & CaptionFieldName & "], PrimaryContentName=[" & PrimaryContentName & "], SecondaryContentName=[" & SecondaryContentName & "], RulesContentName=[" & RulesContentName & "], RulesPrimaryFieldName=[" & RulesPrimaryFieldname & "], RulesSecondaryFieldName=[" & RulesSecondaryFieldName & "]"))
                Else
                    '
                    ' ----- Gather all the SecondaryContent that associates to the PrimaryContent
                    '
                    PrimaryContentID = cpCore.metaData.getContentId(PrimaryContentName)
                    SecondaryCDef = cpCore.metaData.getCdef(SecondaryContentName)
                    SecondaryTablename = SecondaryCDef.ContentTableName
                    SecondaryContentID = SecondaryCDef.Id
                    ContentIDList.Add(SecondaryContentID)
                    ContentIDList.AddRange(SecondaryCDef.childIdList(cpCore))
                    '
                    '
                    '
                    rulesTablename = cpCore.metaData.getContentTablename(RulesContentName)
                    SingularPrefixHtmlEncoded = encodeHTML(genericController.GetSingular(SecondaryContentName)) & "&nbsp;"
                    '
                    main_MemberShipCount = 0
                    main_MemberShipSize = 0
                    returnHtml = ""
                    If (SecondaryTablename <> "") And (rulesTablename <> "") Then
                        OldFolderVar = "OldFolder" & cpCore.doc.checkListCnt
                        javaScriptRequired &= "var " & OldFolderVar & ";"
                        If PrimaryRecordID = 0 Then
                            '
                            ' New record, use the DefaultSecondaryIDList
                            '
                            If DefaultSecondaryIDList <> "" Then

                                main_MemberShipText = Split(DefaultSecondaryIDList, ",")
                                For Ptr = 0 To UBound(main_MemberShipText)
                                    main_MemberShipID = genericController.EncodeInteger(main_MemberShipText(Ptr))
                                    If main_MemberShipID <> 0 Then
                                        ReDim Preserve main_MemberShip(Ptr)
                                        main_MemberShip(Ptr) = main_MemberShipID
                                        main_MemberShipCount = Ptr + 1
                                    End If
                                Next
                                If main_MemberShipCount > 0 Then
                                    ReDim main_MemberShipRuleCopy(main_MemberShipCount - 1)
                                End If
                                'main_MemberShipCount = UBound(main_MemberShip) + 1
                                main_MemberShipSize = main_MemberShipCount
                            End If
                        Else
                            '
                            ' ----- Determine main_MemberShip (which secondary records are associated by a rule)
                            ' ----- (exclude new record issue ID=0)
                            '
                            If IsRuleCopySupported Then
                                SQL = "SELECT " & SecondaryTablename & ".ID AS ID," & rulesTablename & ".RuleCopy"
                            Else
                                SQL = "SELECT " & SecondaryTablename & ".ID AS ID,'' as RuleCopy"
                            End If
                            SQL &= "" _
                            & " FROM " & SecondaryTablename & " LEFT JOIN" _
                            & " " & rulesTablename & " ON " & SecondaryTablename & ".ID = " & rulesTablename & "." & RulesSecondaryFieldName _
                            & " WHERE " _
                            & " (" & rulesTablename & "." & RulesPrimaryFieldname & "=" & PrimaryRecordID & ")" _
                            & " AND (" & rulesTablename & ".Active<>0)" _
                            & " AND (" & SecondaryTablename & ".Active<>0)" _
                            & " And (" & SecondaryTablename & ".ContentControlID IN (" & String.Join(",", ContentIDList) & "))"
                            If SecondaryContentSelectCriteria <> "" Then
                                SQL &= "AND(" & SecondaryContentSelectCriteria & ")"
                            End If
                            If SecondaryCDef.AllowWorkflowAuthoring And cpCore.siteProperties.allowWorkflowAuthoring Then
                                SQL &= "and(" & SecondaryTablename & ".editsourceid is null)"
                            End If
                            CS = cpCore.db.cs_openSql(SQL)
                            If cpCore.db.cs_ok(CS) Then
                                If True Then
                                    main_MemberShipSize = 10
                                    ReDim main_MemberShip(main_MemberShipSize)
                                    ReDim main_MemberShipRuleCopy(main_MemberShipSize)
                                    Do While cpCore.db.cs_ok(CS)
                                        If main_MemberShipCount >= main_MemberShipSize Then
                                            main_MemberShipSize = main_MemberShipSize + 10
                                            ReDim Preserve main_MemberShip(main_MemberShipSize)
                                            ReDim Preserve main_MemberShipRuleCopy(main_MemberShipSize)
                                        End If
                                        main_MemberShip(main_MemberShipCount) = cpCore.db.cs_getInteger(CS, "ID")
                                        main_MemberShipRuleCopy(main_MemberShipCount) = cpCore.db.cs_getText(CS, "RuleCopy")
                                        main_MemberShipCount = main_MemberShipCount + 1
                                        cpCore.db.cs_goNext(CS)
                                    Loop
                                End If
                            End If
                            cpCore.db.cs_Close(CS)
                        End If
                        '
                        ' ----- Gather all the Secondary Records, sorted by ContentName
                        '
                        SQL = "SELECT " & SecondaryTablename & ".ID AS ID, AllowedContent.Name AS SectionName, " & SecondaryTablename & "." & CaptionFieldName & " AS OptionCaption, " & SecondaryTablename & ".name AS OptionName, " & SecondaryTablename & ".SortOrder"
                        If IsRuleCopySupported Then
                            SQL &= "," & SecondaryTablename & ".AllowRuleCopy," & SecondaryTablename & ".RuleCopyCaption"
                        Else
                            SQL &= ",0 as AllowRuleCopy,'' as RuleCopyCaption"
                        End If
                        If IsContentCategoriesSupported Then
                            SQL &= "" _
                                & ",Folders.ID as ContentCategoryID,Folders.Name as ContentFolderName" _
                                & " FROM ((" & SecondaryTablename _
                                & " LEFT JOIN ccContent AllowedContent ON " & SecondaryTablename & ".ContentControlID = AllowedContent.ID)" _
                                & " LEFT JOIN ccContentCategories Folders ON " & SecondaryTablename & ".ContentCategoryID = Folders.ID)" _
                                & " Where (" & SecondaryTablename & ".Active<>" & SQLFalse & ")" _
                                & " And (AllowedContent.Active<>" & SQLFalse & ")" _
                                & " And (" & SecondaryTablename & ".ContentControlID IN (" & String.Join(",", ContentIDList) & "))"
                        Else
                            SQL &= "" _
                                & ",0 as ContentCategoryID,'' as ContentFolderName" _
                                & " FROM (" & SecondaryTablename _
                                & " LEFT JOIN ccContent AllowedContent ON " & SecondaryTablename & ".ContentControlID = AllowedContent.ID)" _
                                & " Where (" & SecondaryTablename & ".Active<>" & SQLFalse & ")" _
                                & " And (AllowedContent.Active<>" & SQLFalse & ")" _
                                & " And (" & SecondaryTablename & ".ContentControlID IN (" & String.Join(",", ContentIDList) & "))"
                        End If
                        If SecondaryCDef.AllowWorkflowAuthoring And cpCore.siteProperties.allowWorkflowAuthoring Then
                            SQL &= "and(" & SecondaryTablename & ".editsourceid is null)"
                        End If
                        If SecondaryContentSelectCriteria <> "" Then
                            SQL &= "AND(" & SecondaryContentSelectCriteria & ")"
                        End If
                        SQL &= " GROUP BY " & SecondaryTablename & ".ID, AllowedContent.Name, " & SecondaryTablename & "." & CaptionFieldName & ", " & SecondaryTablename & ".name, " & SecondaryTablename & ".SortOrder"
                        If IsRuleCopySupported Then
                            SQL &= ", " & SecondaryTablename & ".AllowRuleCopy," & SecondaryTablename & ".RuleCopyCaption"
                        End If
                        If IsContentCategoriesSupported Then
                            SQL &= ",Folders.Name,Folders.ID"
                        End If
                        SQL &= " ORDER BY "
                        If IsContentCategoriesSupported Then
                            SQL &= "Folders.Name,Folders.ID,"
                        End If
                        SQL &= SecondaryTablename & "." & CaptionFieldName
                        CS = cpCore.db.cs_openSql(SQL)
                        If Not cpCore.db.cs_ok(CS) Then
                            returnHtml = "(No choices are available.)"
                        Else
                            If True Then
                                OpenFolderID = -1
                                EndDiv = ""
                                SectionName = ""
                                CheckBoxCnt = 0
                                DivCheckBoxCnt = 0
                                DivCnt = 0
                                CanSeeHiddenFields = cpCore.authContext.isAuthenticatedDeveloper(cpCore)
                                DivName = TagName & ".All"
                                Do While cpCore.db.cs_ok(CS)
                                    OptionName = cpCore.db.cs_getText(CS, "OptionName")
                                    If (Mid(OptionName, 1, 1) <> "_") Or CanSeeHiddenFields Then
                                        '
                                        ' Current checkbox is visible
                                        '
                                        CurrentFolderID = cpCore.db.cs_getInteger(CS, "ContentCategoryID")
                                        If IsContentCategoriesSupported And (CurrentFolderID <> OpenFolderID) Then
                                            '
                                            ' Content Category mode, new folderID - end the previous div and start a new one
                                            '
                                            OpenFolderID = CurrentFolderID
                                            ContentFolderName = cpCore.db.cs_getText(CS, "ContentFolderName")
                                            DivID = TagName & ".ContentCategoryID" & CurrentFolderID
                                            If DivCnt = 0 Then
                                                '
                                                ' First div - Add in javascript needed to store current visible div
                                                '
                                                javaScriptRequired &= OldFolderVar & "='" & DivID & "';"
                                                's = s & cr & "<script type=""text/javascript"">" & OldFolderVar & "='" & DivID & "'</script>" & vbCrLf
                                                'OldFolderVar = "OldFolder" & main_CheckListCnt
                                                's = s & cr & "<script type=""text/javascript"">var " & OldFolderVar & "='" & DivID & "'</script>" & vbCrLf
                                                '
                                                ' Add in the empty DIV - shows when the folder is empty
                                                '
                                                returnHtml &= cr & "<div id=""" & TagName & ".empty"" style=""display:none;"">This category is empty</div>"
                                            End If

                                            returnHtml &= EndDiv
                                            returnHtml &= cr & "<div ID=""" & DivID & """ style=""display:none;""><input type=hidden name=""" & DivName & """>"
                                            If ContentFolderName <> "" Then
                                                returnHtml &= cr & "<div>" & ContentFolderName & "</div>"
                                                returnHtml &= cr & "<div style=""padding-left:10px;padding-bottom:10px;"">"
                                            Else
                                                returnHtml &= cr & "<div>Uncategorized</div>"
                                                returnHtml &= cr & "<div style=""padding-left:10px;padding-bottom:10px;"">"
                                            End If
                                            EndDiv = cr & "</div></div>"
                                            DivCnt = DivCnt + 1
                                            DivCheckBoxCnt = 0
                                        End If

                                        RecordID = cpCore.db.cs_getInteger(CS, "ID")
                                        AllowRuleCopy = cpCore.db.cs_getBoolean(CS, "AllowRuleCopy")
                                        RuleCopyCaption = cpCore.db.cs_getText(CS, "RuleCopyCaption")
                                        OptionCaption = cpCore.db.cs_getText(CS, "OptionCaption")
                                        If OptionCaption = "" Then
                                            OptionCaption = OptionName
                                        End If
                                        If OptionCaption = "" Then
                                            optionCaptionHtmlEncoded = SingularPrefixHtmlEncoded & RecordID
                                        Else
                                            optionCaptionHtmlEncoded = encodeHTML(OptionCaption)
                                        End If
                                        If DivCheckBoxCnt <> 0 Then
                                            ' leave this between checkboxes - it is searched in the admin page
                                            returnHtml &= "<br >" & vbCrLf
                                        End If
                                        RuleCopy = ""
                                        If False Then
                                            Found = False
                                            's = s & "<input type=""checkbox"" name=""" & TagName & "." & CheckBoxCnt & """ "
                                            If main_MemberShipCount <> 0 Then
                                                For main_MemberShipPointer = 0 To main_MemberShipCount - 1
                                                    If main_MemberShip(main_MemberShipPointer) = (RecordID) Then
                                                        RuleCopy = main_MemberShipRuleCopy(main_MemberShipPointer)
                                                        returnHtml &= html_GetFormInputHidden(TagName & "." & CheckBoxCnt, True)
                                                        Found = True
                                                        Exit For
                                                    End If
                                                Next
                                            End If
                                            returnHtml &= genericController.main_GetYesNo(Found) & "&nbsp;-&nbsp;"
                                        Else
                                            Found = False
                                            If main_MemberShipCount <> 0 Then
                                                For main_MemberShipPointer = 0 To main_MemberShipCount - 1
                                                    If main_MemberShip(main_MemberShipPointer) = (RecordID) Then
                                                        's = s & main_GetFormInputHidden(TagName & "." & CheckBoxCnt, True)
                                                        RuleCopy = main_MemberShipRuleCopy(main_MemberShipPointer)
                                                        Found = True
                                                        Exit For
                                                    End If
                                                Next
                                            End If
                                            ' must leave the first hidden with the value in this form - it is searched in the admin pge
                                            returnHtml &= vbCrLf
                                            returnHtml &= "<table><tr><td style=""vertical-align:top;margin-top:0;width:20px;"">"
                                            returnHtml &= "<input type=hidden name=""" & TagName & "." & CheckBoxCnt & ".ID"" value=" & RecordID & ">"
                                            If readOnlyfield And Not Found Then
                                                returnHtml &= "<input type=checkbox disabled>"
                                            ElseIf readOnlyfield Then
                                                returnHtml &= "<input type=checkbox disabled checked>"
                                                returnHtml &= "<input type=""hidden"" name=""" & TagName & "." & CheckBoxCnt & ".ID"" value=" & RecordID & ">"
                                            ElseIf Found Then
                                                returnHtml &= "<input type=checkbox name=""" & TagName & "." & CheckBoxCnt & """ checked>"
                                            Else
                                                returnHtml &= "<input type=checkbox name=""" & TagName & "." & CheckBoxCnt & """>"
                                            End If
                                            returnHtml &= "</td><td style=""vertical-align:top;padding-top:4px;"">"
                                            returnHtml &= SpanClassAdminNormal & optionCaptionHtmlEncoded
                                            If AllowRuleCopy Then
                                                returnHtml &= ", " & RuleCopyCaption & "&nbsp;" & html_GetFormInputText2(TagName & "." & CheckBoxCnt & ".RuleCopy", RuleCopy, 1, 20)
                                            End If
                                            returnHtml &= "</td></tr></table>"
                                        End If
                                        CheckBoxCnt = CheckBoxCnt + 1
                                        DivCheckBoxCnt = DivCheckBoxCnt + 1
                                    End If
                                    cpCore.db.cs_goNext(CS)
                                Loop
                                returnHtml &= EndDiv
                                returnHtml &= "<input type=""hidden"" name=""" & TagName & ".RowCount"" value=""" & CheckBoxCnt & """>" & vbCrLf
                            End If
                        End If
                        cpCore.db.cs_Close(CS)
                        Call addHeadJavascriptCode(javaScriptRequired, "CheckList Categories")
                    End If
                    'End If
                    cpCore.doc.checkListCnt = cpCore.doc.checkListCnt + 1
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnHtml
        End Function
        '
        '==========================================================================================================================================
        '   main_GetFormInputCheckList - with two panes
        '       Content Category view on the left
        '       Checkboxes for content on the right that match the left folder
        '==========================================================================================================================================
        '
        Public Function getInputCheckListCategories(ByVal TagName As String, ByVal PrimaryContentName As String, ByVal PrimaryRecordID As Integer, ByVal SecondaryContentName As String, ByVal RulesContentName As String, ByVal RulesPrimaryFieldname As String, ByVal RulesSecondaryFieldName As String, Optional ByVal SecondaryContentSelectCriteria As String = "", Optional ByVal CaptionFieldName As String = "", Optional ByVal readOnlyField As Boolean = False, Optional ByVal RightSideHeader As String = "", Optional ByVal DefaultSecondaryIDList As String = "") As String
            Dim result As String = ""
            Try
                Dim AllNode As String
                Dim LeftPane As String
                Dim RightPane As String
                Dim CS As Integer
                Dim SQL As String
                Dim BakeName As String
                Dim Caption As String
                Dim Id As Integer
                Dim CurrentFolderID As Integer
                Dim IsAuthoringMode As Boolean
                Dim LinkBase As String
                Dim JSCaption As String
                Dim JSSwitch As String
                Dim JSSwitchFirst As String = String.Empty
                Dim FirstCaption As String = String.Empty
                Dim EmptyDivID As String
                Dim JSSwitchAll As String
                Dim IsContentCategoriesSupported As Boolean
                '
                IsContentCategoriesSupported = cpCore.metaData.isContentFieldSupported(SecondaryContentName, "ContentCategoryID")
                If Not IsContentCategoriesSupported Then
                    result = getInputCheckList(TagName, PrimaryContentName, PrimaryRecordID, SecondaryContentName, RulesContentName, RulesPrimaryFieldname, RulesSecondaryFieldName, SecondaryContentSelectCriteria, CaptionFieldName, readOnlyField, False, "")
                Else
                    IsAuthoringMode = True
                    LinkBase = cpCore.doc.refreshQueryString
                    BakeName = "ContentFolderNav"
                    If Not IsAuthoringMode Then
                        '          main_GetFormInputCheckListCategories = cache.cache_readBake(BakeName)
                    End If

                    Dim s As String = String.Empty

                    If result = "" Then
                        EmptyDivID = TagName & ".empty"
                        SQL = cpCore.db.GetSQLSelect("", "ccContentCategories", "ID,ContentCategoryID,Name", , "Name")
                        CS = cpCore.db.cs_openSql(SQL)
                        Do While cpCore.db.cs_ok(CS)
                            Caption = cpCore.db.cs_getText(CS, "name")
                            Id = cpCore.db.cs_getInteger(CS, "ID")
                            CurrentFolderID = cpCore.db.cs_getInteger(CS, "ContentCategoryID")
                            '
                            Caption = genericController.vbReplace(Caption, " ", "&nbsp;")
                            If FirstCaption = "" Then
                                FirstCaption = Caption
                            End If
                            JSCaption = genericController.EncodeJavascript(Caption)
                            JSSwitch = "switchContentFolderDiv( '" & TagName & ".ContentCategoryID" & Id & "',OldFolder" & cpCore.doc.checkListCnt & ",'" & TagName & ".ContentCaption','" & JSCaption & "','" & EmptyDivID & "'); OldFolder" & cpCore.doc.checkListCnt & "='" & TagName & ".ContentCategoryID" & Id & "';return false;"
                            If JSSwitchFirst = "" Then
                                JSSwitchFirst = JSSwitch
                            End If
                            s = s & cr & "<li class=""ccAdminSmall ccPanel""><a href=""#"" onclick=""" & JSSwitch & """>" & Caption & "</a></li>"
                            'Call Tree.AddEntry(CStr(Id), CStr(CurrentFolderID), , , Link, Caption, JSSwitch)
                            'Call Tree.AddEntry(CStr(ID), CStr(CurrentFolderID), , , Link, Caption, "switchContentFolderDiv( '" & TagName & ".ContentCategoryID" & ID & "', OldFolder" & main_CheckListCnt & ",'" & TagName & ".ContentCaption'," & JSCaption & "); OldFolder" & main_CheckListCnt & "='" & TagName & ".ContentCategoryID" & ID & "';")
                            Call cpCore.db.cs_goNext(CS)
                        Loop
                        Call cpCore.db.cs_Close(CS)
                        LeftPane = cr & "<ul>" & genericController.htmlIndent(s) & cr & "</ul>"
                        'LeftPane = Tree.GetTree(CStr(0), OpenMenuName)
                        '
                        ' Add the top 'All' node
                        '
                        JSCaption = "All"
                        JSSwitchAll = "switchContentFolderDiv( '" & TagName & ".All',  OldFolder" & cpCore.doc.checkListCnt & ",'" & TagName & ".ContentCaption','" & JSCaption & "','" & EmptyDivID & "'); OldFolder" & cpCore.doc.checkListCnt & "='" & TagName & ".All';"
                        If genericController.vbInstr(1, LeftPane, "<LI", vbTextCompare) = 0 Then
                            AllNode = "<div class=""caption""><a href=""#"" onClick=""" & JSSwitchAll & ";return false;"">Show all</a></div>"
                            LeftPane = cr & AllNode & LeftPane
                        Else
                            AllNode = "<div class=""caption""><a href=""#"" onClick=""" & JSSwitchAll & ";return false;"">Show all</a></div>"
                            LeftPane = cr & AllNode & LeftPane
                        End If
                        '
                        ' + Add Category
                        '
                        If cpCore.authContext.isAuthenticatedContentManager(cpCore, "Content Categories") Then
                            LeftPane = LeftPane & cr & "<div class=""caption""><a href=""" & cpCore.siteProperties.adminURL & "?editreferer=" & genericController.EncodeRequestVariable("?" & cpCore.doc.refreshQueryString) & "&cid=" & cpCore.metaData.getContentId("Content Categories") & "&af=4&aa=2"">+&nbsp;Add&nbsp;Category</a></div>"
                        End If
                        '
                        LeftPane = cr & "<div class=""ccCategoryListCon"">" & genericController.htmlIndent(LeftPane) & cr & "</div>"
                        '
                        ' open the current node
                        '
                        RightPane = getInputCheckList(TagName, PrimaryContentName, PrimaryRecordID, SecondaryContentName, RulesContentName, RulesPrimaryFieldname, RulesSecondaryFieldName, SecondaryContentSelectCriteria, CaptionFieldName, readOnlyField, True, DefaultSecondaryIDList)
                        '
                        result = "" _
                        & "<div style=""border:1px solid #A0A0A0;"">" _
                        & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%;"">" _
                        & "<tr>" _
                        & "<td class=""ccAdminTab"" style=""width:100px;padding:5px;text-align:left"">Categories<br ><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=90 height=1></td>" _
                        & "<td class=""ccAdminTab"" style=""width:1px;""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=1 height=1></td>" _
                        & "<td class=""ccAdminTab"" style=""width:90%;padding:5px;text-align:left"" ID=""" & TagName & ".ContentCaption"">" & genericController.encodeEmptyText(RightSideHeader, "&nbsp;") & "</td>" _
                        & "</td></tr>" _
                        & "<tr>" _
                        & "<td style=""width:100px;padding:10px;Background-color:white;border:0px solid #808080;vertical-align:top;text-align:left"">" & LeftPane & "</td>" _
                        & "<td class=""ccAdminTab"" style=""width:1px;""></td>" _
                        & "<td style=""width:90%;padding:10px;Background-color:white;border:0px solid #808080;vertical-align:top;text-align:left"">" & RightPane & "</td>" _
                        & "</td></tr>" _
                        & "</table>" _
                        & "</div>"
                        If Not IsAuthoringMode Then
                            Call cpCore.cache.setObject(BakeName, result, "Content Categories," & PrimaryContentName & "," & SecondaryContentName & "," & RulesContentName)
                        End If
                        '
                        ' initialize with all open
                        '
                        Call addOnLoadJavascript(JSSwitchAll, "Checklist Categories")
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function

        '
        '
        '
        Public Sub addOnLoadJavascript(NewCode As String, addedByMessage As String)
            On Error GoTo ErrorTrap
            '
            Dim s As String
            '
            s = cpCore.doc.onLoadJavascript
            If NewCode <> "" And genericController.vbInstr(1, s, NewCode, vbTextCompare) = 0 Then
                If s <> "" Then
                    s = s & ";"
                End If
                If (addedByMessage <> "") And cpCore.visitProperty.getBoolean("AllowDebugging") Then
                    s = s & " /* from " & addedByMessage & " */ "
                End If
                s = s & NewCode
                cpCore.doc.onLoadJavascript = s
            End If
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_AddOnLoadJavascript2")
        End Sub
        '
        '
        '
        Public Sub addBodyJavascriptCode(NewCode As String, addedByMessage As String)
            On Error GoTo ErrorTrap
            '
            Dim s As String
            '
            s = ""
            If NewCode <> "" And genericController.vbInstr(1, cpCore.doc.endOfBodyJavascript, NewCode, vbTextCompare) = 0 Then
                's = s & vbCrLf
                If (addedByMessage <> "") And cpCore.visitProperty.getBoolean("AllowDebugging") Then
                    s = s & "/* from " & addedByMessage & "*/"
                End If
                s = s & NewCode
                cpCore.doc.endOfBodyJavascript = cpCore.doc.endOfBodyJavascript & vbCrLf & s
            End If
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_AddEndOfBodyJavascript2")
        End Sub

        '
        '
        '
        Public Sub addHeadJavascriptCode(NewCode As String, addedByMessage As String)
            On Error GoTo ErrorTrap
            '
            Dim s As String
            Dim StartPos As Integer
            Dim EndPos As Integer
            '
            If NewCode <> "" Then
                s = NewCode
                StartPos = genericController.vbInstr(1, s, "<script", vbTextCompare)
                If StartPos <> 0 Then
                    EndPos = genericController.vbInstr(StartPos, s, "</script", vbTextCompare)
                    If EndPos <> 0 Then
                        EndPos = genericController.vbInstr(EndPos, s, ">", vbTextCompare)
                        If EndPos <> 0 Then
                            s = Left(s, StartPos - 1) & Mid(s, EndPos + 1)
                        End If
                    End If
                End If
                '
                ' I am going to regret this...
                '
                Do While genericController.vbInstr(1, NewCode, vbTab & vbTab) <> 0
                    NewCode = genericController.vbReplace(NewCode, vbTab & vbTab, vbTab)
                Loop
                Do While genericController.vbInstr(1, NewCode, cr) <> 0
                    NewCode = genericController.vbReplace(NewCode, cr, vbCrLf)
                Loop
                NewCode = genericController.vbReplace(NewCode, vbCrLf & vbCrLf, vbCrLf)
                NewCode = genericController.vbReplace(NewCode, vbCrLf & vbCrLf, vbCrLf)
                NewCode = genericController.vbReplace(NewCode, vbCrLf, cr2)
                ReDim Preserve cpCore.doc.headScripts(cpCore.doc.headScripts.Count)
                cpCore.doc.headScripts(cpCore.doc.headScripts.Count - 1).IsLink = False
                cpCore.doc.headScripts(cpCore.doc.headScripts.Count - 1).Text = NewCode
                cpCore.doc.headScripts(cpCore.doc.headScripts.Count - 1).addedByMessage = genericController.vbLCase(addedByMessage)
                'cpCore.doc.headScriptCnt = cpCore.doc.headScriptCnt + 1
            End If
            '    If NewCode <> "" And genericController.vbInstr(1, main_HeadScriptCode, NewCode, vbTextCompare) = 0 Then
            '        s = NewCode
            '        StartPos = genericController.vbInstr(1, s, "<script", vbTextCompare)
            '        If StartPos <> 0 Then
            '            EndPos = genericController.vbInstr(StartPos, s, "</script", vbTextCompare)
            '            If EndPos <> 0 Then
            '                EndPos = genericController.vbInstr(EndPos, s, ">", vbTextCompare)
            '                If EndPos <> 0 Then
            '                    s = Left(s, StartPos - 1) & Mid(s, EndPos + 1)
            '                End If
            '            End If
            '        End If
            '        main_HeadScriptCode = main_HeadScriptCode & cr
            '        If AddedByMessage <> "" Then
            '            main_HeadScriptCode = main_HeadScriptCode & "/* from " & AddedByMessage & " */" & cr
            '        End If
            '        main_HeadScriptCode = main_HeadScriptCode & s
            ''        If Pos = 0 Then
            ''            main_HeadScriptCode = main_HeadScriptCode & "<script Language=""JavaScript"" type=""text/javascript"">" & NewCode & "</script>"
            ''        Else
            ''            main_HeadScriptCode = main_HeadScriptCode & NewCode
            ''        End If
            '    End If
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_AddHeadScriptCode")
        End Sub
        '
        '
        '
        Public Sub addJavaScriptLinkHead(Filename As String, addedByMessage As String)
            On Error GoTo ErrorTrap
            '
            Dim s As String
            '
            If Filename <> "" Then
                ReDim Preserve cpCore.doc.headScripts(cpCore.doc.headScripts.Count)
                cpCore.doc.headScripts(cpCore.doc.headScripts.Count - 1).IsLink = True
                cpCore.doc.headScripts(cpCore.doc.headScripts.Count - 1).Text = Filename
                cpCore.doc.headScripts(cpCore.doc.headScripts.Count - 1).addedByMessage = addedByMessage
                'cpCore.doc.headScriptCnt = cpCore.doc.headScriptCnt + 1
            End If
            '    If Filename <> "" And genericController.vbInstr(1, s, Filename, vbTextCompare) = 0 Then
            '
            '        main_HeadScriptLinkList = main_HeadScriptLinkList & vbCrLf & Filename & vbTab & AddedByMessage
            '    End If
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_AddHeadScriptLink")
        End Sub
        '
        '=========================================================================================================
        '
        '=========================================================================================================
        '
        Public Sub main_AddPagetitle(PageTitle As String)
            Call doc_AddPagetitle2(PageTitle, "")
        End Sub
        '
        '
        '
        Public Sub doc_AddPagetitle2(PageTitle As String, addedByMessage As String)
            On Error GoTo ErrorTrap
            '
            If PageTitle <> "" And genericController.vbInstr(1, cpCore.doc.metaContent_Title, PageTitle, vbTextCompare) = 0 Then
                If (addedByMessage <> "") And cpCore.visitProperty.getBoolean("AllowDebugging") Then
                    Call doc_AddHeadTag2("<!-- title from " & addedByMessage & " -->", "")
                End If
                If cpCore.doc.metaContent_Title <> "" Then
                    cpCore.doc.metaContent_Title = cpCore.doc.metaContent_Title & ", "
                End If
                cpCore.doc.metaContent_Title = cpCore.doc.metaContent_Title & PageTitle
                'main_MetaContent_Title_ToBeAdded = True
            End If
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_AddPagetitle")
        End Sub
        '
        '=========================================================================================================
        '
        '=========================================================================================================
        '
        Public Sub main_addMetaDescription(MetaDescription As String)
            Call doc_addMetaDescription2(MetaDescription, "")
        End Sub
        '
        '
        '
        Public Sub doc_addMetaDescription2(MetaDescription As String, addedByMessage As String)
            On Error GoTo ErrorTrap
            '
            If MetaDescription <> "" And genericController.vbInstr(1, cpCore.doc.metaContent_Description, MetaDescription, vbTextCompare) = 0 Then
                If (addedByMessage <> "") And cpCore.visitProperty.getBoolean("AllowDebugging") Then
                    Call doc_AddHeadTag2("<!-- meta description from " & addedByMessage & " -->", "")
                End If
                If cpCore.doc.metaContent_Description <> "" Then
                    cpCore.doc.metaContent_Description = cpCore.doc.metaContent_Description & ", "
                End If
                cpCore.doc.metaContent_Description = cpCore.doc.metaContent_Description & MetaDescription
            End If
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_addMetaDescription2")
        End Sub

        '
        '
        '
        Public Sub addStyleLink(StyleSheetLink As String, addedByMessage As String)
            Try
                If StyleSheetLink <> "" Then
                    cpCore.doc.metaContent_StyleSheetTags = cpCore.doc.metaContent_StyleSheetTags & cr
                    If (addedByMessage <> "") And cpCore.visitProperty.getBoolean("AllowDebugging") Then
                        cpCore.doc.metaContent_StyleSheetTags = cpCore.doc.metaContent_StyleSheetTags & "<!-- from " & addedByMessage & " -->"
                    End If
                    cpCore.doc.metaContent_StyleSheetTags = cpCore.doc.metaContent_StyleSheetTags & "<link rel=""stylesheet"" type=""text/css"" href=""" & StyleSheetLink & """>"
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub

        '
        '
        '
        Public Sub addSharedStyleID2(ByVal styleId As Integer, Optional ByVal addedByMessage As String = "")
            On Error GoTo ErrorTrap
            '
            If genericController.vbInstr(1, cpCore.doc.metaContent_SharedStyleIDList & ",", "," & styleId & ",") = 0 Then
                If (addedByMessage <> "") And cpCore.visitProperty.getBoolean("AllowDebugging") Then
                    Call doc_AddHeadTag2("<!-- shared style " & styleId & " from " & addedByMessage & " -->", "")
                End If
                cpCore.doc.metaContent_SharedStyleIDList = cpCore.doc.metaContent_SharedStyleIDList & "," & styleId
            End If
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_AddSharedStyleID2")
        End Sub
        '
        '=========================================================================================================
        '
        '=========================================================================================================
        '
        Public Sub main_addMetaKeywordList(MetaKeywordList As String)
            Call doc_addMetaKeywordList2(MetaKeywordList, "")
        End Sub
        '
        '
        '
        Public Sub doc_addMetaKeywordList2(MetaKeywordList As String, addedByMessage As String)
            On Error GoTo ErrorTrap
            '
            If MetaKeywordList <> "" And genericController.vbInstr(1, cpCore.doc.metaContent_KeyWordList, MetaKeywordList, vbTextCompare) = 0 Then
                If (addedByMessage <> "") And cpCore.visitProperty.getBoolean("AllowDebugging") Then
                    Call doc_AddHeadTag2("<!-- meta keyword list from " & addedByMessage & " -->", "")
                End If
                If cpCore.doc.metaContent_KeyWordList <> "" Then
                    cpCore.doc.metaContent_KeyWordList = cpCore.doc.metaContent_KeyWordList & ", "
                End If
                cpCore.doc.metaContent_KeyWordList = cpCore.doc.metaContent_KeyWordList & MetaKeywordList
            End If
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_addMetaKeywordList2")
        End Sub
        '
        '=========================================================================================================
        '
        '=========================================================================================================
        '
        Public Sub addHeadTag(HeadTag As String)
            Call doc_AddHeadTag2(HeadTag, "")
        End Sub
        '
        '
        '
        Public Sub doc_AddHeadTag2(HeadTag As String, addedByMessage As String)
            On Error GoTo ErrorTrap
            '
            If HeadTag <> "" And genericController.vbInstr(1, cpCore.doc.metaContent_OtherHeadTags, HeadTag, vbTextCompare) = 0 Then
                If cpCore.doc.metaContent_OtherHeadTags <> "" Then
                    cpCore.doc.metaContent_OtherHeadTags = cpCore.doc.metaContent_OtherHeadTags & vbCrLf
                End If
                If (addedByMessage <> "") And cpCore.visitProperty.getBoolean("AllowDebugging") Then
                    cpCore.doc.metaContent_OtherHeadTags = cpCore.doc.metaContent_OtherHeadTags & "<!-- from " & addedByMessage & " -->" & vbCrLf
                End If
                cpCore.doc.metaContent_OtherHeadTags = cpCore.doc.metaContent_OtherHeadTags & HeadTag
            End If
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_AddHeadTag")
        End Sub
        '
        '
        '
        Public Sub main_addMeta(metaName As String, metaContent As String, addedByMessage As String)
            Call doc_AddHeadTag2("<meta name=""" & encodeHTML(metaName) & """ content=""" & encodeHTML(metaContent) & """>", addedByMessage)
        End Sub
        '
        '
        '
        Public Sub main_addMetaProperty(metaProperty As String, metaContent As String, addedByMessage As String)
            Call doc_AddHeadTag2("<meta property=""" & encodeHTML(metaProperty) & """ content=""" & encodeHTML(metaContent) & """>", addedByMessage)
        End Sub
        '
        Friend ReadOnly Property main_ReturnAfterEdit() As Boolean
            Get
                Return True
            End Get
        End Property
        '
        '===================================================================================================
        '   Wrap the content in a common wrapper if authoring is enabled
        '===================================================================================================
        '
        Public Function main_GetEditWrapper(ByVal Caption As String, ByVal Content As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetEditWrapper")
            '
            'If Not (true) Then Exit Function
            '
            Dim IsAuthoring As Boolean
            '
            IsAuthoring = cpCore.authContext.isEditingAnything()
            If Not IsAuthoring Then
                main_GetEditWrapper = Content
            Else
                main_GetEditWrapper = html_GetLegacySiteStyles()
                If False Then
                    main_GetEditWrapper = main_GetEditWrapper _
                        & "<table border=0 width=""100%"" cellspacing=0 cellpadding=0><tr><td class=""ccEditWrapper"">" _
                        & "<table border=0 width=""100%"" cellspacing=0 cellpadding=0><tr><td class=""ccEditWrapperInner"">" _
                            & "<table border=0 width=""100%"" cellspacing=0 cellpadding=0><tr><td class=""ccEditWrapperCaption"">" _
                            & genericController.encodeText(Caption) _
                            & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=1 height=22 align=absmiddle>" _
                            & "</td></tr></table>" _
                            & "<table border=0 width=""100%"" cellspacing=0 cellpadding=0><tr><td class=""ccEditWrapperContent"" id=""editWrapper" & cpCore.doc.editWrapperCnt & """>" _
                            & genericController.encodeText(Content) _
                            & "</td></tr></table>" _
                        & "</td></tr></table>" _
                        & "</td></tr></table>"
                Else
                    main_GetEditWrapper = main_GetEditWrapper _
                        & "<table border=0 width=""100%"" cellspacing=0 cellpadding=0><tr><td class=""ccEditWrapper"">"
                    If Caption <> "" Then
                        main_GetEditWrapper = main_GetEditWrapper _
                                & "<table border=0 width=""100%"" cellspacing=0 cellpadding=0><tr><td class=""ccEditWrapperCaption"">" _
                                & genericController.encodeText(Caption) _
                                & "<!-- <img alt=""space"" src=""/ccLib/images/spacer.gif"" width=1 height=22 align=absmiddle> -->" _
                                & "</td></tr></table>"
                    End If
                    main_GetEditWrapper = main_GetEditWrapper _
                            & "<table border=0 width=""100%"" cellspacing=0 cellpadding=0><tr><td class=""ccEditWrapperContent"" id=""editWrapper" & cpCore.doc.editWrapperCnt & """>" _
                            & genericController.encodeText(Content) _
                            & "</td></tr></table>" _
                        & "</td></tr></table>"
                End If
                cpCore.doc.editWrapperCnt = cpCore.doc.editWrapperCnt + 1
            End If

            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetEditWrapper")
        End Function
        '
        ' To support the special case when the template calls this to encode itself, and the page content has already been rendered.
        '
        Public Function encodeContent10(Source As String, personalizationPeopleId As Integer, ContextContentName As String, ContextRecordID As Integer, ContextContactPeopleID As Integer, PlainText As Boolean, AddLinkEID As Boolean, EncodeActiveFormatting As Boolean, EncodeActiveImages As Boolean, EncodeActiveEditIcons As Boolean, EncodeActivePersonalization As Boolean, queryStringForLinkAppend As String, ProtocolHostString As String, IsEmailContent As Boolean, ignore_DefaultWrapperID As Integer, ignore_TemplateCaseOnly_Content As String, Context As CPUtilsBaseClass.addonContext, personalizationIsAuthenticated As Boolean, nothingObject As Object, isEditingAnything As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("EncodeContent9")
            '
            Const StartFlag = "<!-- ADDON"
            Const EndFlag = " -->"
            '
            Dim DoAnotherPass As Boolean
            Dim returnValue As String
            Dim ArgCnt As Integer
            Dim AddonGuid As String
            Dim AddonStatusOK As Boolean
            Dim SortMethodID As Integer
            Dim SortMethod As String
            Dim ACInstanceID As String
            Dim ArgSplit() As String
            Dim AddonName As String
            Dim addonOptionString As String
            Dim LineStart As Integer
            Dim FormSetName As String
            'Dim RecordID as integer
            Dim ContentID As Integer
            'Dim ContentName As String
            'Dim ContactMemberID as integer
            Dim Instruction As String
            Dim LineEnd As Integer
            Dim InstrSplit As Object
            Dim AggrObject As Object
            Dim Copy As String
            Dim OptionString As String
            Dim Wrapper() As String
            Dim Stream As String
            Dim SegmentSplit() As String
            Dim AcCmd As String
            Dim SegmentPrefix As String
            Dim SegmentSuffix As String
            Dim AcCmdSplit() As String
            Dim ACType As String
            'Dim AcCmdArgs As String
            Dim ContentSplit() As String
            Dim ContentSplitCnt As Integer
            Dim Segment As String
            Dim Ptr As Integer
            'Dim ContentFound as integer
            Dim CopyName As String
            'Dim CSContext as integer
            Dim AggregateObjectName As String
            Dim ListName As String
            Dim SortField As String
            Dim SortReverse As Boolean
            Dim ACName As String
            Dim Obj As Object
            Dim hint As String
            Dim AdminURL As String
            '
            Dim converthtmlToText As htmlToTextControllers
            Dim Pos As Integer
            Dim LayoutEngineOptionString As String
            Dim LayoutErrorMessage As String
            '
            ' if personalizationPeopleId is 0, set it to the current user
            '
            Dim iPersonalizationPeopleId As Integer
            iPersonalizationPeopleId = personalizationPeopleId
            If iPersonalizationPeopleId = 0 Then
                iPersonalizationPeopleId = cpCore.authContext.user.id
            End If
            '
            returnValue = Source
            'hint = "csv_EncodeContent9 enter"
            If returnValue <> "" Then
                AdminURL = cpCore.siteProperties.adminURL
                '
                '--------
                ' cut-paste from csv_EncodeContent8
                '--------
                '
                ' ----- Do EncodeCRLF Conversion
                '
                'hint = hint & ",010"
                If cpCore.siteProperties.getBoolean("ConvertContentCRLF2BR", False) And (Not PlainText) Then
                    returnValue = genericController.vbReplace(returnValue, vbCr, "")
                    returnValue = genericController.vbReplace(returnValue, vbLf, "<br>")
                End If
                '
                ' ----- Do upgrade conversions (upgrade legacy objects and upgrade old images)
                '
                'hint = hint & ",020"
                returnValue = html_EncodeContentUpgrades(returnValue)
                '
                ' ----- Do Active Content Conversion
                '
                'hint = hint & ",030"
                If (AddLinkEID Or EncodeActiveFormatting Or EncodeActiveImages Or EncodeActiveEditIcons) Then
                    returnValue = html_EncodeActiveContent_Internal(returnValue, iPersonalizationPeopleId, ContextContentName, ContextRecordID, ContextContactPeopleID, AddLinkEID, EncodeActiveFormatting, EncodeActiveImages, EncodeActiveEditIcons, EncodeActivePersonalization, queryStringForLinkAppend, ProtocolHostString, IsEmailContent, AdminURL, personalizationIsAuthenticated, Context)
                End If
                '
                ' ----- Do Plain Text Conversion
                '
                'hint = hint & ",040"
                If PlainText Then
                    converthtmlToText = New htmlToTextControllers(cpCore)
                    returnValue = converthtmlToText.convert(returnValue)
                    converthtmlToText = Nothing
                End If
                '
                ' Process Active Content that must be run here to access webclass objects
                '     parse as {{functionname?querystring}}
                '
                'hint = hint & ",110"
                If (Not EncodeActiveEditIcons) And (InStr(1, returnValue, "{{") <> 0) Then
                    ContentSplit = Split(returnValue, "{{")
                    returnValue = ""
                    ContentSplitCnt = UBound(ContentSplit) + 1
                    Ptr = 0
                    Do While Ptr < ContentSplitCnt
                        'hint = hint & ",200"
                        Segment = ContentSplit(Ptr)
                        If Ptr = 0 Then
                            '
                            ' Add in the non-command text that is before the first command
                            '
                            returnValue = returnValue & Segment
                        ElseIf (Segment <> "") Then
                            If genericController.vbInstr(1, Segment, "}}") = 0 Then
                                '
                                ' No command found, return the marker and deliver the Segment
                                '
                                'hint = hint & ",210"
                                returnValue = returnValue & "{{" & Segment
                            Else
                                '
                                ' isolate the command
                                '
                                'hint = hint & ",220"
                                SegmentSplit = Split(Segment, "}}")
                                AcCmd = SegmentSplit(0)
                                SegmentSplit(0) = ""
                                SegmentSuffix = Mid(Join(SegmentSplit, "}}"), 3)
                                If Trim(AcCmd) <> "" Then
                                    '
                                    ' isolate the arguments
                                    '
                                    'hint = hint & ",230"
                                    AcCmdSplit = Split(AcCmd, "?")
                                    ACType = Trim(AcCmdSplit(0))
                                    If UBound(AcCmdSplit) = 0 Then
                                        addonOptionString = ""
                                    Else
                                        addonOptionString = AcCmdSplit(1)
                                        addonOptionString = genericController.decodeHtml(addonOptionString)
                                    End If
                                    '
                                    ' execute the command
                                    '
                                    Select Case genericController.vbUCase(ACType)
                                        Case ACTypeDynamicForm
                                            '
                                            ' Dynamic Form - run the core addon replacement instead
                                            '
                                            'hint = hint & ",310"
                                            returnValue = returnValue & cpCore.addon.execute(0, DynamicFormGuid, addonOptionString, CPUtilsBaseClass.addonContext.ContextPage, ContextContentName, ContextRecordID, "", "", False, ignore_DefaultWrapperID, "", AddonStatusOK, Nothing, "", Nothing, "", iPersonalizationPeopleId, personalizationIsAuthenticated)
                                        Case ACTypeChildList
                                            '
                                            ' Child Page List
                                            '
                                            'hint = hint & ",320"
                                            ListName = addonController.getAddonOption("name", addonOptionString)
                                            returnValue = returnValue & cpCore.doc.getChildPageList(ListName, ContextContentName, ContextRecordID, True)
                                        Case ACTypeTemplateText
                                            '
                                            ' Text Box = copied here from gethtmlbody
                                            '
                                            CopyName = addonController.getAddonOption("new", addonOptionString)
                                            If CopyName = "" Then
                                                CopyName = addonController.getAddonOption("name", addonOptionString)
                                                If CopyName = "" Then
                                                    CopyName = "Default"
                                                End If
                                            End If
                                            returnValue = returnValue & html_GetContentCopy(CopyName, "", iPersonalizationPeopleId, False, personalizationIsAuthenticated)
                                        Case ACTypeWatchList
                                            '
                                            ' Watch List
                                            '
                                            'hint = hint & ",330"
                                            ListName = addonController.getAddonOption("LISTNAME", addonOptionString)
                                            SortField = addonController.getAddonOption("SORTFIELD", addonOptionString)
                                            SortReverse = genericController.EncodeBoolean(addonController.getAddonOption("SORTDIRECTION", addonOptionString))
                                            returnValue = returnValue & cpCore.doc.main_GetWatchList(cpCore, ListName, SortField, SortReverse)
                                        Case Else
                                            '
                                            ' Unrecognized command - put all the syntax back in
                                            '
                                            'hint = hint & ",340"
                                            returnValue = returnValue & "{{" & AcCmd & "}}"
                                    End Select
                                End If
                                '
                                ' add the SegmentSuffix back on
                                '
                                returnValue = returnValue & SegmentSuffix
                            End If
                        End If
                        '
                        ' Encode into Javascript if required
                        '
                        Ptr = Ptr + 1
                    Loop
                End If
                '
                ' Process Addons
                '   parse as <!-- Addon "Addon Name","OptionString" -->
                '   They are handled here because Addons are written against cpCoreClass, not the Content Server class
                '   ...so Group Email can not process addons 8(
                '   Later, remove the csv routine that translates <ac to this, and process it directly right here
                '   Later, rewrite so addons call csv, not cpCoreClass, so email processing can include addons
                ' (2/16/2010) - move csv_EncodeContent to csv, or wait and move it all to CP
                '    eventually, everything should migrate to csv and/or cp to eliminate the cpCoreClass dependancy
                '    and all add-ons run as processes the same as they run on pages, or as remote methods
                ' (2/16/2010) - if <!-- AC --> has four arguments, the fourth is the addon guid
                '
                If (InStr(1, returnValue, StartFlag) <> 0) Then
                    Do While (InStr(1, returnValue, StartFlag) <> 0)
                        LineStart = genericController.vbInstr(1, returnValue, StartFlag)
                        LineEnd = genericController.vbInstr(LineStart, returnValue, EndFlag)
                        If LineEnd = 0 Then
                            logController.appendLog(cpCore, "csv_EncodeContent9, Addon could not be inserted into content because the HTML comment holding the position is not formated correctly")
                            Exit Do
                        Else
                            AddonName = ""
                            addonOptionString = ""
                            ACInstanceID = ""
                            AddonGuid = ""
                            Copy = Mid(returnValue, LineStart + 11, LineEnd - LineStart - 11)
                            ArgSplit = genericController.SplitDelimited(Copy, ",")
                            ArgCnt = UBound(ArgSplit) + 1
                            If ArgSplit(0) <> "" Then
                                AddonName = Mid(ArgSplit(0), 2, Len(ArgSplit(0)) - 2)
                                If ArgCnt > 1 Then
                                    If ArgSplit(1) <> "" Then
                                        addonOptionString = Mid(ArgSplit(1), 2, Len(ArgSplit(1)) - 2)
                                        addonOptionString = genericController.decodeHtml(Trim(addonOptionString))
                                    End If
                                    If ArgCnt > 2 Then
                                        If ArgSplit(2) <> "" Then
                                            ACInstanceID = Mid(ArgSplit(2), 2, Len(ArgSplit(2)) - 2)
                                        End If
                                        If ArgCnt > 3 Then
                                            If ArgSplit(3) <> "" Then
                                                AddonGuid = Mid(ArgSplit(3), 2, Len(ArgSplit(3)) - 2)
                                            End If
                                        End If
                                    End If
                                End If
                                ' dont have any way of getting fieldname yet
                                If AddonGuid <> "" Then
                                    Copy = cpCore.addon.execute(0, AddonGuid, addonOptionString, CPUtilsBaseClass.addonContext.ContextPage, ContextContentName, ContextRecordID, "", ACInstanceID, False, ignore_DefaultWrapperID, ignore_TemplateCaseOnly_Content, AddonStatusOK, Nothing, "", Nothing, "", iPersonalizationPeopleId, personalizationIsAuthenticated)
                                Else
                                    Copy = cpCore.addon.execute(0, AddonName, addonOptionString, CPUtilsBaseClass.addonContext.ContextPage, ContextContentName, ContextRecordID, "", ACInstanceID, False, ignore_DefaultWrapperID, ignore_TemplateCaseOnly_Content, AddonStatusOK, Nothing, "", Nothing, "", iPersonalizationPeopleId, personalizationIsAuthenticated)
                                End If
                            End If
                        End If
                        returnValue = Mid(returnValue, 1, LineStart - 1) & Copy & Mid(returnValue, LineEnd + 4)
                    Loop
                End If
                '
                ' process out text block comments inserted by addons
                ' remove all content between BlockTextStartMarker and the next BlockTextEndMarker, or end of copy
                ' exception made for the content with just the startmarker because when the AC tag is replaced with
                ' with the marker, encode content is called with the result, which is just the marker, and this
                ' section will remove it
                '
                If (Not isEditingAnything) And (returnValue <> BlockTextStartMarker) Then
                    DoAnotherPass = True
                    Do While (InStr(1, returnValue, BlockTextStartMarker, vbTextCompare) <> 0) And DoAnotherPass
                        LineStart = genericController.vbInstr(1, returnValue, BlockTextStartMarker, vbTextCompare)
                        If LineStart = 0 Then
                            DoAnotherPass = False
                        Else
                            LineEnd = genericController.vbInstr(LineStart, returnValue, BlockTextEndMarker, vbTextCompare)
                            If LineEnd <= 0 Then
                                DoAnotherPass = False
                                returnValue = Mid(returnValue, 1, LineStart - 1)
                            Else
                                LineEnd = genericController.vbInstr(LineEnd, returnValue, " -->")
                                If LineEnd <= 0 Then
                                    DoAnotherPass = False
                                Else
                                    returnValue = Mid(returnValue, 1, LineStart - 1) & Mid(returnValue, LineEnd + 4)
                                    'returnValue = Mid(returnValue, 1, LineStart - 1) & Copy & Mid(returnValue, LineEnd + 4)
                                End If
                            End If
                        End If
                    Loop
                End If
                '
                ' only valid for a webpage
                '
                If True Then
                    '
                    ' Add in EditWrappers for Aggregate scripts and replacements
                    '   This is also old -- used here because csv encode content can create replacements and links, but can not
                    '   insert wrappers. This is all done in GetAddonContents() now. This routine is left only to
                    '   handle old style calls in cache.
                    '
                    'hint = hint & ",500, Adding edit wrappers"
                    If isEditingAnything Then
                        If (InStr(1, returnValue, "<!-- AFScript -->", vbTextCompare) <> 0) Then
                            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError7("returnValue", "AFScript Style edit wrappers are not supported")
                            Copy = main_GetEditWrapper("Aggregate Script", "##MARKER##")
                            Wrapper = Split(Copy, "##MARKER##")
                            returnValue = genericController.vbReplace(returnValue, "<!-- AFScript -->", Wrapper(0), 1, 99, vbTextCompare)
                            returnValue = genericController.vbReplace(returnValue, "<!-- /AFScript -->", Wrapper(1), 1, 99, vbTextCompare)
                        End If
                        If (InStr(1, returnValue, "<!-- AFReplacement -->", vbTextCompare) <> 0) Then
                            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError7("returnValue", "AFReplacement Style edit wrappers are not supported")
                            Copy = main_GetEditWrapper("Aggregate Replacement", "##MARKER##")
                            Wrapper = Split(Copy, "##MARKER##")
                            returnValue = genericController.vbReplace(returnValue, "<!-- AFReplacement -->", Wrapper(0), 1, 99, vbTextCompare)
                            returnValue = genericController.vbReplace(returnValue, "<!-- /AFReplacement -->", Wrapper(1), 1, 99, vbTextCompare)
                        End If
                    End If
                    '
                    ' Process Feedback form
                    '
                    'hint = hint & ",600, Handle webclient features"
                    If genericController.vbInstr(1, returnValue, FeedbackFormNotSupportedComment, vbTextCompare) <> 0 Then
                        returnValue = genericController.vbReplace(returnValue, FeedbackFormNotSupportedComment, pageContentController.main_GetFeedbackForm(cpCore, ContextContentName, ContextRecordID, ContextContactPeopleID), 1, 99, vbTextCompare)
                    End If
                    '
                    ' if call from webpage, push addon js and css out to cpCoreClass
                    '
                    Copy = cpCore.doc.headTags
                    If Copy <> "" Then
                        '
                        ' headtags generated from csv_EncodeContent
                        '
                        Call doc_AddHeadTag2(Copy, "embedded content")
                    End If
                    '
                    ' If any javascript or styles were added during encode, pick them up now
                    '
                    Copy = cpCore.doc.getNextJavascriptBodyEnd()
                    Do While Copy <> ""
                        Call addBodyJavascriptCode(Copy, "embedded content")
                        Copy = cpCore.doc.getNextJavascriptBodyEnd()
                    Loop
                    '
                    ' current
                    '
                    Copy = cpCore.doc.getNextJSFilename()
                    Do While Copy <> ""
                        If genericController.vbInstr(1, Copy, "://") <> 0 Then
                        ElseIf Left(Copy, 1) = "/" Then
                        Else
                            Copy = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, Copy)
                        End If
                        Call addJavaScriptLinkHead(Copy, "embedded content")
                        Copy = cpCore.doc.getNextJSFilename()
                    Loop
                    '
                    Copy = cpCore.doc.getJavascriptOnLoad()
                    Do While Copy <> ""
                        Call addOnLoadJavascript(Copy, "")
                        Copy = cpCore.doc.getJavascriptOnLoad()
                    Loop
                    '
                    Copy = cpCore.doc.getNextStyleFilenames()
                    Do While Copy <> ""
                        If genericController.vbInstr(1, Copy, "://") <> 0 Then
                        ElseIf Left(Copy, 1) = "/" Then
                        Else
                            Copy = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, Copy)
                        End If
                        Call addStyleLink(Copy, "")
                        Copy = cpCore.doc.getNextStyleFilenames()
                    Loop
                End If
            End If
            '
            encodeContent10 = returnValue
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            If hint <> "" Then
                Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError7("csv_EncodeContent9-" & hint, "Unexpected Trap Error")
            Else
                Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError7("csv_EncodeContent9", "Unexpected Trap Error")
            End If
        End Function
        '
        ' ================================================================================================================
        '   Upgrade old objects in content, and update changed resource library images
        ' ================================================================================================================
        '
        Public Function html_EncodeContentUpgrades(Source As String) As String
            On Error GoTo ErrorTrap 'Const Tn = "EncodeContentUpgrades": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            'Dim hint As String
            Dim RecordVirtualPath As String = String.Empty
            Dim RecordVirtualFilename As String
            Dim RecordFilename As String
            Dim RecordFilenameNoExt As String
            Dim RecordFilenameExt As String = String.Empty
            Dim RecordFilenameAltSize As String
            Dim SizeTest() As String
            Dim RecordAltSizeList As String
            Dim TagPosEnd As Integer
            Dim TagPosStart As Integer
            Dim InTag As Boolean
            Dim Wrapper As String
            Dim AddonFound As Boolean
            Dim ACNameCaption As String
            Dim GroupIDList As String
            Dim IDControlString As String
            Dim IconIDControlString As String
            Dim Criteria As String
            Dim AddonContentName As String
            Dim SelectList As String
            Dim CSFields As Integer
            Dim IconWidth As Integer
            Dim IconHeight As Integer
            Dim IconSprites As Integer
            Dim IconIsInline As Boolean
            Dim IconAlt As String
            Dim IconTitle As String
            Dim IconImg As String
            'dim buildversion As String
            Dim Cmd As String
            Dim TextName As String
            Dim ACInstanceOptionList As String
            Dim SortFieldList As String
            Dim ListName As String
            Dim SortDirection As String
            Dim AllowGroupAccess As Boolean
            Dim LoopPtr As Integer
            Dim SrcOptionSelector As String
            Dim ResultOptionSelector As String
            Dim ContentName As String
            Dim SrcOptionList As String
            Dim Pos As Integer
            Dim list As String
            Dim FnArgList As String
            Dim FnArgs() As String
            Dim FnArgCnt As Integer
            Dim ContentCriteria As String
            Dim RecordName As String
            Dim REsultOptionValue As String
            Dim SrcOptionValueSelector As String
            Dim InstanceOptionValue As String
            Dim ResultOptionListHTMLEncoded As String
            Dim UCaseACName As String
            Dim IconFilename As String
            Dim FilenameSegment As String
            Dim EndPos1 As Integer
            Dim EndPos2 As Integer
            Dim LinkSplit() As String
            Dim LinkCnt As Integer
            Dim LinkPtr As Integer
            Dim TableSplit() As String
            Dim TableName As String
            Dim FieldName As String
            Dim RecordID As Integer
            Dim SQL As String
            Dim SaveChanges As Boolean
            Dim NextPosSpace As Integer
            Dim NextPosQuote As Integer
            Dim LinkEndPos As Integer
            Dim EndPos As Integer
            Dim Ptr As Integer
            Dim FilePrefixSegment As String
            Dim ElementPointer As Integer
            Dim ListCount As Integer
            Dim CSVisitor As Integer
            Dim CSVisit As Integer
            Dim CSChildList As Integer
            Dim CSVisitorSet As Boolean
            Dim CSVisitSet As Boolean
            'Dim ContextContentName As String
            'Dim ContextRecordID as integer
            Dim ElementTag As String
            Dim ElementName As String
            Dim ACType As String
            Dim ACField As String
            Dim ACName As String
            Dim ACBullet As Boolean
            'Dim ContextContactPeopleID as integer
            Dim Copy As String
            Dim LinkLabel As String
            Dim Overview As String
            'Dim CSContext as integer
            Dim KmaHTML As htmlParserController
            Dim AttributeCount As Integer
            Dim AttributePointer As Integer
            Dim Name As String
            Dim Value As String
            Dim JoinCriteria As String
            Dim CS As Integer
            'Dim FormattingContentID as integer
            Dim ACAttrRecordID As Integer
            Dim ACAttrWidth As Integer
            Dim ACAttrHeight As Integer
            Dim ACAttrAlt As String
            Dim ACAttrBorder As Integer
            Dim ACAttrLoop As Integer
            Dim ACAttrVSpace As Integer
            Dim ACAttrHSpace As Integer
            Dim Filename As String
            Dim ACAttrAlign As String
            Dim ProcessAnchorTags As Boolean
            Dim ProcessACTags As Boolean
            Dim SelectFieldList As String
            Dim ACLanguageName As String
            Dim Stream As stringBuilderLegacyController
            Dim AnchorQuery As String
            '
            Dim CSOrganization As Integer
            Dim CSOrganizationSet As Boolean
            '
            Dim CSPeople As Integer
            Dim CSPeopleSet As Boolean
            '
            Dim CSlanguage As Integer
            Dim PeopleLanguageSet As Boolean
            Dim PeopleLanguage As String
            Dim UcasePeopleLanguage As String
            '
            Dim serverFilePath As String
            Dim ReplaceCount As Integer
            Dim ReplaceInstructions As String
            '
            Dim Link As String
            Dim NotUsedID As Integer
            Dim CSInstance As Integer
            Dim InstanceOptionList As String
            Dim InstanceOptionListHTMLEncoded As String
            Dim ProgramID As String
            Dim AggrObject As Object
            Dim OptionString As String
            Dim Options() As String
            Dim OptionCnt As Integer
            Dim OptionPtr As Integer
            Dim OptionPair() As String
            Dim OptionName As String
            Dim OptionValue As String
            Dim SrcOptions() As String
            Dim SrcOptionCnt As Integer
            Dim SrcOptionPtr As Integer
            Dim SrcOptionPair() As String
            Dim SrcOptionName As String
            Dim SrcOptionValue As String

            Dim FormCount As Integer
            Dim FormInputCount As Integer
            Dim CDef As cdefModel
            Dim FieldList As String
            Dim ImageAllowUpdate As Boolean
            Dim ContentFilesLinkPrefix As String
            Dim ResourceLibraryLinkPrefix As String
            Dim TestChr As String
            Dim ACInstanceID As String
            Dim ParseError As Boolean
            Dim PosStart As Integer
            Dim PosEnd As Integer
            Dim AllowGroups As String
            '
            ''hint = "csv_EncodeContentUpgrades enter"
            html_EncodeContentUpgrades = Source
            '
            ContentFilesLinkPrefix = "/" & cpCore.serverConfig.appConfig.name & "/files/"
            ResourceLibraryLinkPrefix = ContentFilesLinkPrefix & "ccLibraryFiles/"
            ImageAllowUpdate = cpCore.siteProperties.getBoolean("ImageAllowUpdate", True)
            ImageAllowUpdate = ImageAllowUpdate And (InStr(1, Source, ResourceLibraryLinkPrefix, vbTextCompare) <> 0)
            If ImageAllowUpdate Then
                '
                ' ----- Process Resource Library Images (swap in most current file)
                '
                '   There is a better way:
                '   problem with replacing the images is the problem with parsing - too much work to find it
                '   instead, use new replacement tags <ac type=image src="cclibraryfiles/filename/00001" width=0 height=0>
                '
                ''hint = hint & ",010"
                ParseError = False
                LinkSplit = Split(Source, ContentFilesLinkPrefix, , vbTextCompare)
                LinkCnt = UBound(LinkSplit) + 1
                For LinkPtr = 1 To LinkCnt - 1
                    '
                    ' Each LinkSplit(1...) is a segment that would have started with '/appname/files/'
                    ' Next job is to determine if this sement is in a tag (<img src="...">) or in content (&quot...&quote)
                    ' For now, skip the ones in content
                    '
                    ''hint = hint & ",020"
                    TagPosEnd = genericController.vbInstr(1, LinkSplit(LinkPtr), ">")
                    TagPosStart = genericController.vbInstr(1, LinkSplit(LinkPtr), "<")
                    If TagPosEnd = 0 And TagPosStart = 0 Then
                        '
                        ' no tags found, skip it
                        '
                        InTag = False
                    ElseIf TagPosEnd = 0 Then
                        '
                        ' no end tag, but found a start tag -> in content
                        '
                        InTag = False
                    ElseIf TagPosEnd < TagPosStart Then
                        '
                        ' Found end before start - > in tag
                        '
                        InTag = True
                    Else
                        '
                        ' Found start before end -> in content
                        '
                        InTag = False
                    End If
                    If InTag Then
                        ''hint = hint & ",030"
                        TableSplit = Split(LinkSplit(LinkPtr), "/")
                        If UBound(TableSplit) > 2 Then
                            TableName = TableSplit(0)
                            FieldName = TableSplit(1)
                            RecordID = genericController.EncodeInteger(TableSplit(2))
                            FilenameSegment = TableSplit(3)
                            If (LCase(TableName) = "cclibraryfiles") And (LCase(FieldName) = "filename") And (RecordID <> 0) Then
                                Dim file As Models.Entity.libraryFilesModel = Models.Entity.libraryFilesModel.create(cpCore, RecordID)
                                If (file IsNot Nothing) Then
                                    ''hint = hint & ",060"
                                    FieldName = "filename"
                                    'SQL = "select filename,altsizelist from " & TableName & " where id=" & RecordID
                                    'CS = app.csv_OpenCSSQL("default", SQL)
                                    'If app.csv_IsCSOK(CS) Then
                                    If True Then
                                        '
                                        ' now figure out how the link is delimited by how it starts
                                        '   go to the left and look for:
                                        '   ' ' - ignore spaces, continue forward until we find one of these
                                        '   '=' - means space delimited (src=/image.jpg), ends in ' ' or '>'
                                        '   '"' - means quote delimited (src="/image.jpg"), ends in '"'
                                        '   '>' - means this is not in an HTML tag - skip it (<B>image.jpg</b>)
                                        '   '<' - means god knows what, but its wrong, skip it
                                        '   '(' - means it is a URL(/image.jpg), go to ')'
                                        '
                                        ' odd cases:
                                        '   URL( /image.jpg) -
                                        '
                                        RecordVirtualFilename = file.Filename
                                        RecordAltSizeList = file.AltSizeList
                                        If RecordVirtualFilename = genericController.EncodeJavascript(RecordVirtualFilename) Then
                                            '
                                            ' The javascript version of the filename must match the filename, since we have no way
                                            ' of differentiating a ligitimate file, from a block of javascript. If the file
                                            ' contains an apostrophe, the original code could have encoded it, but we can not here
                                            ' so the best plan is to skip it
                                            '
                                            ' example:
                                            ' RecordVirtualFilename = "jay/files/cclibraryfiles/filename/000005/test.png"
                                            '
                                            ' RecordFilename = "test.png"
                                            ' RecordFilenameAltSize = "" (does not exist - the record has the raw filename in it)
                                            ' RecordFilenameExt = "png"
                                            ' RecordFilenameNoExt = "test"
                                            '
                                            ' RecordVirtualFilename = "jay/files/cclibraryfiles/filename/000005/test-100x200.png"
                                            ' this is a specail case - most cases to not have the alt size format saved in the filename
                                            ' RecordFilename = "test-100x200.png"
                                            ' RecordFilenameAltSize (does not exist - the record has the raw filename in it)
                                            ' RecordFilenameExt = "png"
                                            ' RecordFilenameNoExt = "test-100x200"
                                            ' this is wrong
                                            '   xRecordFilenameAltSize = "100x200"
                                            '   xRecordFilenameExt = "png"
                                            '   xRecordFilenameNoExt = "test"
                                            '
                                            ''hint = hint & ",080"
                                            Pos = InStrRev(RecordVirtualFilename, "/")
                                            RecordFilename = ""
                                            If Pos > 0 Then
                                                RecordVirtualPath = Mid(RecordVirtualFilename, 1, Pos)
                                                RecordFilename = Mid(RecordVirtualFilename, Pos + 1)
                                            End If
                                            Pos = InStrRev(RecordFilename, ".")
                                            RecordFilenameNoExt = ""
                                            If Pos > 0 Then
                                                RecordFilenameExt = genericController.vbLCase(Mid(RecordFilename, Pos + 1))
                                                RecordFilenameNoExt = genericController.vbLCase(Mid(RecordFilename, 1, Pos - 1))
                                            End If
                                            FilePrefixSegment = LinkSplit(LinkPtr - 1)
                                            If Len(FilePrefixSegment) > 1 Then
                                                '
                                                ' Look into FilePrefixSegment and see if we are in the querystring attribute of an <AC tag
                                                '   if so, the filename may be AddonEncoded and delimited with & (so skip it)
                                                Pos = InStrRev(FilePrefixSegment, "<")
                                                If Pos > 0 Then
                                                    If genericController.vbLCase(Mid(FilePrefixSegment, Pos + 1, 3)) <> "ac " Then
                                                        '
                                                        ' look back in the FilePrefixSegment to find the character before the link
                                                        '
                                                        EndPos = 0
                                                        For Ptr = Len(FilePrefixSegment) To 1 Step -1
                                                            TestChr = Mid(FilePrefixSegment, Ptr, 1)
                                                            Select Case TestChr
                                                                Case "="
                                                                    '
                                                                    ' Ends in ' ' or '>', find the first
                                                                    '
                                                                    EndPos1 = genericController.vbInstr(1, FilenameSegment, " ")
                                                                    EndPos2 = genericController.vbInstr(1, FilenameSegment, ">")
                                                                    If EndPos1 <> 0 And EndPos2 <> 0 Then
                                                                        If EndPos1 < EndPos2 Then
                                                                            EndPos = EndPos1
                                                                        Else
                                                                            EndPos = EndPos2
                                                                        End If
                                                                    ElseIf EndPos1 <> 0 Then
                                                                        EndPos = EndPos1
                                                                    ElseIf EndPos2 <> 0 Then
                                                                        EndPos = EndPos2
                                                                    Else
                                                                        EndPos = 0
                                                                    End If
                                                                    Exit For
                                                                Case """"
                                                                    '
                                                                    ' Quoted, ends is '"'
                                                                    '
                                                                    EndPos = genericController.vbInstr(1, FilenameSegment, """")
                                                                    Exit For
                                                                Case "("
                                                                    '
                                                                    ' url() style, ends in ')' or a ' '
                                                                    '
                                                                    If genericController.vbLCase(Mid(FilePrefixSegment, Ptr, 7)) = "(&quot;" Then
                                                                        EndPos = genericController.vbInstr(1, FilenameSegment, "&quot;)")
                                                                    ElseIf genericController.vbLCase(Mid(FilePrefixSegment, Ptr, 2)) = "('" Then
                                                                        EndPos = genericController.vbInstr(1, FilenameSegment, "')")
                                                                    ElseIf genericController.vbLCase(Mid(FilePrefixSegment, Ptr, 2)) = "(""" Then
                                                                        EndPos = genericController.vbInstr(1, FilenameSegment, """)")
                                                                    Else
                                                                        EndPos = genericController.vbInstr(1, FilenameSegment, ")")
                                                                    End If
                                                                    Exit For
                                                                Case "'"
                                                                    '
                                                                    ' Delimited within a javascript pair of apostophys
                                                                    '
                                                                    EndPos = genericController.vbInstr(1, FilenameSegment, "'")
                                                                    Exit For
                                                                Case ">", "<"
                                                                    '
                                                                    ' Skip this link
                                                                    '
                                                                    ParseError = True
                                                                    Exit For
                                                            End Select
                                                        Next
                                                        '
                                                        ' check link
                                                        '
                                                        If EndPos = 0 Then
                                                            ParseError = True
                                                            Exit For
                                                        Else
                                                            Dim ImageFilename As String
                                                            Dim SegmentAfterImage As String

                                                            Dim ImageFilenameNoExt As String
                                                            Dim ImageFilenameExt As String
                                                            Dim ImageAltSize As String

                                                            ''hint = hint & ",120"
                                                            SegmentAfterImage = Mid(FilenameSegment, EndPos)
                                                            ImageFilename = genericController.DecodeResponseVariable(Mid(FilenameSegment, 1, EndPos - 1))
                                                            ImageFilenameNoExt = ImageFilename
                                                            ImageFilenameExt = ""
                                                            Pos = InStrRev(ImageFilename, ".")
                                                            If Pos > 0 Then
                                                                ImageFilenameNoExt = genericController.vbLCase(Mid(ImageFilename, 1, Pos - 1))
                                                                ImageFilenameExt = genericController.vbLCase(Mid(ImageFilename, Pos + 1))
                                                            End If
                                                            '
                                                            ' Get ImageAltSize
                                                            '
                                                            ''hint = hint & ",130"
                                                            ImageAltSize = ""
                                                            If ImageFilenameNoExt = RecordFilenameNoExt Then
                                                                '
                                                                ' Exact match
                                                                '
                                                            ElseIf genericController.vbInstr(1, ImageFilenameNoExt, RecordFilenameNoExt, vbTextCompare) <> 1 Then
                                                                '
                                                                ' There was a change and the recordfilename is not part of the imagefilename
                                                                '
                                                            Else
                                                                '
                                                                ' the recordfilename is the first part of the imagefilename - Get ImageAltSize
                                                                '
                                                                ImageAltSize = Mid(ImageFilenameNoExt, Len(RecordFilenameNoExt) + 1)
                                                                If Left(ImageAltSize, 1) <> "-" Then
                                                                    ImageAltSize = ""
                                                                Else
                                                                    ImageAltSize = Mid(ImageAltSize, 2)
                                                                    SizeTest = Split(ImageAltSize, "x")
                                                                    If UBound(SizeTest) <> 1 Then
                                                                        ImageAltSize = ""
                                                                    Else
                                                                        If genericController.vbIsNumeric(SizeTest(0)) And genericController.vbIsNumeric(SizeTest(1)) Then
                                                                            ImageFilenameNoExt = RecordFilenameNoExt
                                                                            'ImageFilenameNoExt = Mid(ImageFilenameNoExt, 1, Pos - 1)
                                                                            'RecordFilenameNoExt = Mid(RecordFilename, 1, Pos - 1)
                                                                        Else
                                                                            ImageAltSize = ""
                                                                        End If
                                                                    End If
                                                                End If
                                                            End If
                                                            '
                                                            ' problem - in the case where the recordfilename = img-100x200, the imagefilenamenoext is img
                                                            '
                                                            ''hint = hint & ",140"
                                                            If (RecordFilenameNoExt <> ImageFilenameNoExt) Or (RecordFilenameExt <> ImageFilenameExt) Then
                                                                '
                                                                ' There has been a change
                                                                '
                                                                Dim NewRecordFilename As String
                                                                Dim ImageHeight As Integer
                                                                Dim ImageWidth As Integer
                                                                NewRecordFilename = RecordVirtualPath & RecordFilenameNoExt & "." & RecordFilenameExt
                                                                '
                                                                ' realtime image updates replace without creating new size - that is for the edit interface
                                                                '
                                                                ' put the New file back into the tablesplit in case there are more then 4 splits
                                                                '
                                                                TableSplit(0) = ""
                                                                TableSplit(1) = ""
                                                                TableSplit(2) = ""
                                                                TableSplit(3) = SegmentAfterImage
                                                                NewRecordFilename = genericController.EncodeURL(NewRecordFilename) & Mid(Join(TableSplit, "/"), 4)
                                                                LinkSplit(LinkPtr) = NewRecordFilename
                                                                SaveChanges = True
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                    If ParseError Then
                        Exit For
                    End If
                Next
                ''hint = hint & ",910"
                If SaveChanges And (Not ParseError) Then
                    html_EncodeContentUpgrades = Join(LinkSplit, ContentFilesLinkPrefix)
                End If
            End If
            ''hint = hint & ",920"
            If Not ParseError Then
                '
                ' Convert ACTypeDynamicForm to Add-on
                '
                If genericController.vbInstr(1, html_EncodeContentUpgrades, "<ac type=""" & ACTypeDynamicForm, vbTextCompare) <> 0 Then
                    html_EncodeContentUpgrades = genericController.vbReplace(html_EncodeContentUpgrades, "type=""DYNAMICFORM""", "TYPE=""aggregatefunction""", 1, 99, vbTextCompare)
                    html_EncodeContentUpgrades = genericController.vbReplace(html_EncodeContentUpgrades, "name=""DYNAMICFORM""", "name=""DYNAMIC FORM""", 1, 99, vbTextCompare)
                End If
            End If
            ''hint = hint & ",930"
            If ParseError Then
                html_EncodeContentUpgrades = "" _
                    & vbCrLf & "<!-- warning: parsing aborted on ccLibraryFile replacement -->" _
                    & vbCrLf & html_EncodeContentUpgrades _
                    & vbCrLf & "<!-- /warning: parsing aborted on ccLibraryFile replacement -->"
            End If
            '
            ' {{content}} should be <ac type="templatecontent" etc>
            ' the merge is now handled in csv_EncodeActiveContent, but some sites have hand {{content}} tags entered
            '
            ''hint = hint & ",940"
            If genericController.vbInstr(1, html_EncodeContentUpgrades, "{{content}}", vbTextCompare) <> 0 Then
                html_EncodeContentUpgrades = genericController.vbReplace(html_EncodeContentUpgrades, "{{content}}", "<AC type=""" & ACTypeTemplateContent & """>", 1, 99, vbTextCompare)
            End If
            '
            'Call main_testPoint(hint)
            '
            Exit Function
ErrorTrap:
            cpCore.handleException(New Exception("Unexpected exception"))
            'Call csv_HandleClassTrapError(Err.Number, Err.Source, Err.Description, "unknownMethodNameLegacyCall" & ", hint=[" & hint & "]", True)
        End Function
        '
        '============================================================================
        '   csv_GetContentCopy3
        '       To get them, cp.content.getCopy must call the cpCoreClass version, which calls this for the content
        '============================================================================
        '
        Public Function html_GetContentCopy(ByVal CopyName As String, ByVal DefaultContent As String, ByVal personalizationPeopleId As Integer, ByVal AllowEditWrapper As Boolean, ByVal personalizationIsAuthenticated As Boolean) As String
            Dim returnCopy As String = ""
            Try
                '
                Dim CS As Integer
                Dim RecordID As Integer
                Dim contactPeopleId As Integer
                Dim Return_ErrorMessage As String = ""
                '
                ' honestly, not sure what to do with 'return_ErrorMessage'
                '
                CS = cpCore.db.cs_open("copy content", "Name=" & cpCore.db.encodeSQLText(CopyName), "ID", , 0, , , "Name,ID,Copy,modifiedBy")
                If Not cpCore.db.cs_ok(CS) Then
                    Call cpCore.db.cs_Close(CS)
                    CS = cpCore.db.cs_insertRecord("copy content", 0)
                    If cpCore.db.cs_ok(CS) Then
                        RecordID = cpCore.db.cs_getInteger(CS, "ID")
                        Call cpCore.db.cs_set(CS, "name", CopyName)
                        Call cpCore.db.cs_set(CS, "copy", genericController.encodeText(DefaultContent))
                        Call cpCore.db.cs_save2(CS)
                        Call cpCore.workflow.publishEdit("copy content", RecordID)
                    End If
                End If
                If cpCore.db.cs_ok(CS) Then
                    RecordID = cpCore.db.cs_getInteger(CS, "ID")
                    contactPeopleId = cpCore.db.cs_getInteger(CS, "modifiedBy")
                    returnCopy = cpCore.db.cs_get(CS, "Copy")
                    returnCopy = html_executeContentCommands(Nothing, returnCopy, CPUtilsBaseClass.addonContext.ContextPage, personalizationPeopleId, personalizationIsAuthenticated, Return_ErrorMessage)
                    returnCopy = encodeContent10(returnCopy, personalizationPeopleId, "copy content", RecordID, contactPeopleId, False, False, True, True, False, True, "", "", False, 0, "", CPUtilsBaseClass.addonContext.ContextPage, False, Nothing, False)
                    '
                    If True Then
                        If cpCore.authContext.isEditingAnything() Then
                            returnCopy = cs_cs_getRecordEditLink(CS, False) & returnCopy
                            If AllowEditWrapper Then
                                returnCopy = main_GetEditWrapper("copy content", returnCopy)
                            End If
                        End If
                    End If
                End If
                Call cpCore.db.cs_Close(CS)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnCopy
        End Function
        '
        '
        '
        Public Sub main_AddTabEntry(ByVal Caption As String, ByVal Link As String, ByVal IsHit As Boolean, Optional ByVal StylePrefix As String = "", Optional ByVal LiveBody As String = "")
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("AddTabEntry")
            '
            ' should use the ccNav object, no the ccCommon module for this code
            '
            Call cpCore.menuTab.AddEntry(genericController.encodeText(Caption), genericController.encodeText(Link), genericController.EncodeBoolean(IsHit), genericController.encodeText(StylePrefix))

            'Call ccAddTabEntry(genericController.encodeText(Caption), genericController.encodeText(Link), genericController.EncodeBoolean(IsHit), genericController.encodeText(StylePrefix), genericController.encodeText(LiveBody))
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_AddTabEntry")
        End Sub
        '        '
        '        '
        '        '
        '        Public Function main_GetTabs() As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetTabs")
        '            '
        '            ' should use the ccNav object, no the ccCommon module for this code
        '            '
        '            '
        '            main_GetTabs = menuTab.GetTabs()
        '            '    main_GetTabs = ccGetTabs()
        '            '
        '            Exit Function
        'ErrorTrap:
        '            throw new applicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetTabs")
        '        End Function
        '
        '
        '
        Public Sub main_AddLiveTabEntry(ByVal Caption As String, ByVal LiveBody As String, Optional ByVal StylePrefix As String = "")
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("AddLiveTabEntry")
            '
            ' should use the ccNav object, no the ccCommon module for this code
            '
            If (cpCore.doc.menuLiveTab Is Nothing) Then
                cpCore.doc.menuLiveTab = New menuLiveTabController
            End If
            Call cpCore.doc.menuLiveTab.AddEntry(genericController.encodeText(Caption), genericController.encodeText(LiveBody), genericController.encodeText(StylePrefix))
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_AddLiveTabEntry")
        End Sub
        '
        '
        '
        Public Function main_GetLiveTabs() As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetLiveTabs")
            '
            ' should use the ccNav object, no the ccCommon module for this code
            '
            If (cpCore.doc.menuLiveTab Is Nothing) Then
                cpCore.doc.menuLiveTab = New menuLiveTabController
            End If
            main_GetLiveTabs = cpCore.doc.menuLiveTab.GetTabs()
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetLiveTabs")
        End Function
        '
        '
        '
        Public Sub menu_AddComboTabEntry(Caption As String, Link As String, AjaxLink As String, LiveBody As String, IsHit As Boolean, ContainerClass As String)
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("AddComboTabEntry")
            '
            ' should use the ccNav object, no the ccCommon module for this code
            '
            If (cpCore.doc.menuComboTab Is Nothing) Then
                cpCore.doc.menuComboTab = New menuComboTabController
            End If
            Call cpCore.doc.menuComboTab.AddEntry(Caption, Link, AjaxLink, LiveBody, IsHit, ContainerClass)
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_AddComboTabEntry")
        End Sub
        '
        '
        '
        Public Function menu_GetComboTabs() As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetComboTabs")
            '
            ' should use the ccNav object, no the ccCommon module for this code
            '
            If (cpCore.doc.menuComboTab Is Nothing) Then
                cpCore.doc.menuComboTab = New menuComboTabController
            End If
            menu_GetComboTabs = cpCore.doc.menuComboTab.GetTabs()
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetComboTabs")
        End Function
        ''
        ''================================================================================================================
        ''   main_Get SharedStyleFilelist
        ''
        ''   SharedStyleFilelist is a list of filenames (with conditional comments) that should be included on pages
        ''   that call out the SharedFileIDList
        ''
        ''   Suffix and Prefix are for Conditional Comments around the style tag
        ''
        ''   SharedStyleFileList is
        ''       crlf filename < Prefix< Suffix
        ''       crlf filename < Prefix< Suffix
        ''       ...
        ''       Prefix and Suffix are htmlencoded
        ''
        ''   SharedStyleMap file
        ''       crlf StyleID tab StyleFilename < Prefix < Suffix, IncludedStyleFilename < Prefix < Suffix, ...
        ''       crlf StyleID tab StyleFilename < Prefix < Suffix, IncludedStyleFilename < Prefix < Suffix, ...
        ''       ...
        ''       StyleID is 0 if Always include is set
        ''       The Prefix and Suffix have had crlf removed, and comma replaced with &#44;
        ''================================================================================================================
        ''
        'Friend Shared Function main_GetSharedStyleFileList(cpCore As coreClass, SharedStyleIDList As String, main_IsAdminSite As Boolean) As String
        '    Dim result As String = ""
        '    '
        '    Dim Prefix As String
        '    Dim Suffix As String
        '    Dim Files() As String
        '    Dim Pos As Integer
        '    Dim SrcID As Integer
        '    Dim Srcs() As String
        '    Dim SrcCnt As Integer
        '    Dim IncludedStyleFilename As String
        '    Dim styleId As Integer
        '    Dim LastStyleID As Integer
        '    Dim CS As Integer
        '    Dim Ptr As Integer
        '    Dim MapList As String
        '    Dim Map() As String
        '    Dim MapCnt As Integer
        '    Dim MapRow As Integer
        '    Dim Filename As String
        '    Dim FileList As String
        '    Dim SQL As String = String.Empty
        '    Dim BakeName As String
        '    '
        '    If main_IsAdminSite Then
        '        BakeName = "SharedStyleMap-Admin"
        '    Else
        '        BakeName = "SharedStyleMap-Public"
        '    End If
        '    MapList = genericController.encodeText(cpCore.cache.getObject(Of String)(BakeName))
        '    If MapList = "" Then
        '        '
        '        ' BuildMap
        '        '
        '        MapList = ""
        '        If True Then
        '            '
        '            ' add prefix and suffix conditional comments
        '            '
        '            SQL = "select s.ID,s.Stylefilename,s.Prefix,s.Suffix,i.StyleFilename as iStylefilename,s.AlwaysInclude,i.Prefix as iPrefix,i.Suffix as iSuffix" _
        '                & " from ((ccSharedStyles s" _
        '                & " left join ccSharedStylesIncludeRules r on r.StyleID=s.id)" _
        '                & " left join ccSharedStyles i on i.id=r.IncludedStyleID)" _
        '                & " where ( s.active<>0 )and((i.active is null)or(i.active<>0))"
        '        End If
        '        CS = cpCore.db.cs_openSql(SQL)
        '        LastStyleID = 0
        '        Do While cpCore.db.cs_ok(CS)
        '            styleId = cpCore.db.cs_getInteger(CS, "ID")
        '            If styleId <> LastStyleID Then
        '                Filename = cpCore.db.cs_get(CS, "StyleFilename")
        '                Prefix = genericController.vbReplace(cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "Prefix")), ",", "&#44;")
        '                Suffix = genericController.vbReplace(cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "Suffix")), ",", "&#44;")
        '                If (Not main_IsAdminSite) And cpCore.db.cs_getBoolean(CS, "alwaysinclude") Then
        '                    MapList = MapList & vbCrLf & "0" & vbTab & Filename & "<" & Prefix & "<" & Suffix
        '                Else
        '                    MapList = MapList & vbCrLf & styleId & vbTab & Filename & "<" & Prefix & "<" & Suffix
        '                End If
        '            End If
        '            IncludedStyleFilename = cpCore.db.cs_getText(CS, "iStylefilename")
        '            Prefix = cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "iPrefix"))
        '            Suffix = cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "iSuffix"))
        '            If IncludedStyleFilename <> "" Then
        '                MapList = MapList & "," & IncludedStyleFilename & "<" & Prefix & "<" & Suffix
        '            End If
        '            Call cpCore.db.cs_goNext(CS)
        '        Loop
        '        If MapList = "" Then
        '            MapList = ","
        '        End If
        '        Call cpCore.cache.setObject(BakeName, MapList, "Shared Styles")
        '    End If
        '    If (MapList <> "") And (MapList <> ",") Then
        '        Srcs = Split(SharedStyleIDList, ",")
        '        SrcCnt = UBound(Srcs) + 1
        '        Map = Split(MapList, vbCrLf)
        '        MapCnt = UBound(Map) + 1
        '        '
        '        ' Add stylesheets with AlwaysInclude set (ID is saved as 0 in Map)
        '        '
        '        FileList = ""
        '        For MapRow = 0 To MapCnt - 1
        '            If genericController.vbInstr(1, Map(MapRow), "0" & vbTab) = 1 Then
        '                Pos = genericController.vbInstr(1, Map(MapRow), vbTab)
        '                If Pos > 0 Then
        '                    FileList = FileList & "," & Mid(Map(MapRow), Pos + 1)
        '                End If
        '            End If
        '        Next
        '        '
        '        ' create a filelist of everything that is needed, might be duplicates
        '        '
        '        For Ptr = 0 To SrcCnt - 1
        '            SrcID = genericController.EncodeInteger(Srcs(Ptr))
        '            If SrcID <> 0 Then
        '                For MapRow = 0 To MapCnt - 1
        '                    If genericController.vbInstr(1, Map(MapRow), SrcID & vbTab) <> 0 Then
        '                        Pos = genericController.vbInstr(1, Map(MapRow), vbTab)
        '                        If Pos > 0 Then
        '                            FileList = FileList & "," & Mid(Map(MapRow), Pos + 1)
        '                        End If
        '                    End If
        '                Next
        '            End If
        '        Next
        '        '
        '        ' dedup the filelist and convert it to crlf delimited
        '        '
        '        If FileList <> "" Then
        '            Files = Split(FileList, ",")
        '            For Ptr = 0 To UBound(Files)
        '                Filename = Files(Ptr)
        '                If genericController.vbInstr(1, result, Filename, vbTextCompare) = 0 Then
        '                    result = result & vbCrLf & Filename
        '                End If
        '            Next
        '        End If
        '    End If
        '    Return result
        'End Function

        '
        '
        '
        Public Function main_GetResourceLibrary2(ByVal RootFolderName As String, ByVal AllowSelectResource As Boolean, ByVal SelectResourceEditorName As String, ByVal SelectLinkObjectName As String, ByVal AllowGroupAdd As Boolean) As String
            Dim Option_String As String
            '
            Option_String = "" _
                & "RootFolderName=" & RootFolderName _
                & "&AllowSelectResource=" & AllowSelectResource _
                & "&SelectResourceEditorName=" & SelectResourceEditorName _
                & "&SelectLinkObjectName=" & SelectLinkObjectName _
                & "&AllowGroupAdd=" & AllowGroupAdd _
                & ""
            Return cpCore.addon.execute_legacy4("{564EF3F5-9673-4212-A692-0942DD51FF1A}", Option_String, CPUtilsBaseClass.addonContext.ContextAdmin)
        End Function
        '
        '========================================================================
        ' Read and save a main_GetFormInputCheckList
        '   see main_GetFormInputCheckList for an explaination of the input
        '========================================================================
        '
        Public Sub main_ProcessCheckList(ByVal TagName As String, ByVal PrimaryContentName As String, ByVal PrimaryRecordID As String, ByVal SecondaryContentName As String, ByVal RulesContentName As String, ByVal RulesPrimaryFieldname As String, ByVal RulesSecondaryFieldName As String)
            '
            Dim rulesTablename As String
            Dim SQL As String
            Dim currentRules As DataTable
            Dim currentRulesCnt As Integer
            Dim RuleFound As Boolean
            Dim RuleId As Integer
            Dim Ptr As Integer
            Dim TestRecordIDLast As Integer
            Dim TestRecordID As Integer
            Dim dupRuleIdList As String
            Dim GroupCnt As Integer
            Dim GroupPtr As Integer
            Dim MethodName As String
            Dim SecondaryRecordID As Integer
            Dim RuleNeeded As Boolean
            Dim CSRule As Integer
            Dim RuleContentChanged As Boolean
            Dim SupportRuleCopy As Boolean
            Dim RuleCopy As String
            '
            MethodName = "ProcessCheckList"
            '
            ' --- create Rule records for all selected
            '
            GroupCnt = cpCore.docProperties.getInteger(TagName & ".RowCount")
            If GroupCnt > 0 Then
                '
                ' Test if RuleCopy is supported
                '
                SupportRuleCopy = cpCore.metaData.isContentFieldSupported(RulesContentName, "RuleCopy")
                If SupportRuleCopy Then
                    SupportRuleCopy = SupportRuleCopy And cpCore.metaData.isContentFieldSupported(SecondaryContentName, "AllowRuleCopy")
                    If SupportRuleCopy Then
                        SupportRuleCopy = SupportRuleCopy And cpCore.metaData.isContentFieldSupported(SecondaryContentName, "RuleCopyCaption")
                    End If
                End If
                '
                ' Go through each checkbox and check for a rule
                '
                '
                ' try
                '
                currentRulesCnt = 0
                dupRuleIdList = ""
                rulesTablename = cpCore.metaData.getContentTablename(RulesContentName)
                SQL = "select " & RulesSecondaryFieldName & ",id from " & rulesTablename & " where (" & RulesPrimaryFieldname & "=" & PrimaryRecordID & ")and(active<>0) order by " & RulesSecondaryFieldName
                currentRulesCnt = 0
                currentRules = cpCore.db.executeSql(SQL)
                currentRulesCnt = currentRules.Rows.Count
                For GroupPtr = 0 To GroupCnt - 1
                    '
                    ' ----- Read Response
                    '
                    SecondaryRecordID = cpCore.docProperties.getInteger(TagName & "." & GroupPtr & ".ID")
                    RuleCopy = cpCore.docProperties.getText(TagName & "." & GroupPtr & ".RuleCopy")
                    RuleNeeded = cpCore.docProperties.getBoolean(TagName & "." & GroupPtr)
                    '
                    ' ----- Update Record
                    '
                    RuleFound = False
                    RuleId = 0
                    TestRecordIDLast = 0
                    For Ptr = 0 To currentRulesCnt - 1
                        TestRecordID = genericController.EncodeInteger(currentRules.Rows(Ptr).Item(0))
                        If TestRecordID = 0 Then
                            '
                            ' skip
                            '
                        ElseIf TestRecordID = SecondaryRecordID Then
                            '
                            ' hit
                            '
                            RuleFound = True
                            RuleId = genericController.EncodeInteger(currentRules.Rows(Ptr).Item(1))
                            Exit For
                        ElseIf TestRecordID = TestRecordIDLast Then
                            '
                            ' dup
                            '
                            dupRuleIdList = dupRuleIdList & "," & genericController.EncodeInteger(currentRules.Rows(Ptr).Item(1))
                            currentRules.Rows(Ptr).Item(0) = 0
                        End If
                        TestRecordIDLast = TestRecordID
                    Next
                    If SupportRuleCopy And RuleNeeded And (RuleFound) Then
                        '
                        ' Record exists and is needed, update the rule copy
                        '
                        SQL = "update " & rulesTablename & " set rulecopy=" & cpCore.db.encodeSQLText(RuleCopy) & " where id=" & RuleId
                        Call cpCore.db.executeSql(SQL)
                    ElseIf RuleNeeded And (Not RuleFound) Then
                        '
                        ' No record exists, and one is needed
                        '
                        CSRule = cpCore.db.cs_insertRecord(RulesContentName)
                        If cpCore.db.cs_ok(CSRule) Then
                            Call cpCore.db.cs_set(CSRule, "Active", RuleNeeded)
                            Call cpCore.db.cs_set(CSRule, RulesPrimaryFieldname, PrimaryRecordID)
                            Call cpCore.db.cs_set(CSRule, RulesSecondaryFieldName, SecondaryRecordID)
                            If SupportRuleCopy Then
                                Call cpCore.db.cs_set(CSRule, "RuleCopy", RuleCopy)
                            End If
                        End If
                        Call cpCore.db.cs_Close(CSRule)
                        RuleContentChanged = True
                    ElseIf (Not RuleNeeded) And RuleFound Then
                        '
                        ' Record exists and it is not needed
                        '
                        SQL = "delete from " & rulesTablename & " where id=" & RuleId
                        Call cpCore.db.executeSql(SQL)
                        RuleContentChanged = True
                    End If
                Next
                '
                ' delete dups
                '
                If dupRuleIdList <> "" Then
                    SQL = "delete from " & rulesTablename & " where id in (" & Mid(dupRuleIdList, 2) & ")"
                    Call cpCore.db.executeSql(SQL)
                    RuleContentChanged = True
                End If
            End If
            If RuleContentChanged Then
                Call cpCore.cache.invalidateContent(RulesContentName)
            End If
        End Sub

        ''
        ''========================================================================
        '' ----- Ends an HTML page
        ''========================================================================
        ''
        'Public Function getHtmlDoc_afterBodyHtml() As String
        '    Return "" _
        '        & cr & "</body>" _
        '        & vbCrLf & "</html>"
        'End Function
        '
        '========================================================================
        ' main_GetRecordEditLink( iContentName, iRecordID )
        '
        '   iContentName The content for this link
        '   iRecordID    The ID of the record in the Table
        '========================================================================
        '
        Public Function main_GetRecordEditLink(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal AllowCut As Boolean = False) As String
            main_GetRecordEditLink = main_GetRecordEditLink2(ContentName, RecordID, genericController.EncodeBoolean(AllowCut), "", cpCore.authContext.isEditing(ContentName))
        End Function
        '
        '========================================================================
        ' main_GetRecordEditLink2( iContentName, iRecordID, AllowCut, RecordName )
        '
        '   ContentName The content for this link
        '   RecordID    The ID of the record in the Table
        '   AllowCut
        '   RecordName
        '   IsEditing
        '========================================================================
        '
        Public Function main_GetRecordEditLink2(ByVal ContentName As String, ByVal RecordID As Integer, ByVal AllowCut As Boolean, ByVal RecordName As String, ByVal IsEditing As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetRecordEditLink2")
            '
            'If Not (true) Then Exit Function
            '
            Dim CS As Integer
            Dim SQL As String
            Dim ContentID As Integer
            Dim Link As String
            Dim MethodName As String
            Dim iContentName As String
            Dim iRecordID As Integer
            Dim RootEntryName As String
            Dim ClipBoard As String
            Dim WorkingLink As String
            Dim iAllowCut As Boolean
            Dim Icon As String
            Dim ContentCaption As String
            '
            iContentName = genericController.encodeText(ContentName)
            iRecordID = genericController.EncodeInteger(RecordID)
            iAllowCut = genericController.EncodeBoolean(AllowCut)
            ContentCaption = genericController.encodeHTML(iContentName)
            If genericController.vbLCase(ContentCaption) = "aggregate functions" Then
                ContentCaption = "Add-on"
            End If
            If genericController.vbLCase(ContentCaption) = "aggregate function objects" Then
                ContentCaption = "Add-on"
            End If
            ContentCaption = ContentCaption & " record"
            If RecordName <> "" Then
                ContentCaption = ContentCaption & ", named '" & RecordName & "'"
            End If
            '
            MethodName = "main_GetRecordEditLink2"
            '
            main_GetRecordEditLink2 = ""
            If (iContentName = "") Then
                Throw (New ApplicationException("ContentName [" & ContentName & "] is invalid")) ' handleLegacyError14(MethodName, "")
            Else
                If (iRecordID < 1) Then
                    Throw (New ApplicationException("RecordID [" & RecordID & "] is invalid")) ' handleLegacyError14(MethodName, "")
                Else
                    If IsEditing Then
                        '
                        ' Edit link, main_Get the CID
                        '
                        ContentID = cpCore.metaData.getContentId(iContentName)
                        '
                        main_GetRecordEditLink2 = main_GetRecordEditLink2 _
                            & "<a" _
                            & " class=""ccRecordEditLink"" " _
                            & " TabIndex=-1" _
                            & " href=""" & genericController.encodeHTML(cpCore.siteProperties.adminURL & "?cid=" & ContentID & "&id=" & iRecordID & "&af=4&aa=2&ad=1") & """"
                        If Not cpCore.html.main_ReturnAfterEdit Then
                            main_GetRecordEditLink2 = main_GetRecordEditLink2 & " target=""_blank"""
                        End If
                        main_GetRecordEditLink2 = main_GetRecordEditLink2 _
                            & "><img" _
                            & " src=""/ccLib/images/IconContentEdit.gif""" _
                            & " border=""0""" _
                            & " alt=""Edit this " & genericController.encodeHTML(ContentCaption) & """" _
                            & " title=""Edit this " & genericController.encodeHTML(ContentCaption) & """" _
                            & " align=""absmiddle""" _
                            & "></a>"
                        '
                        ' Cut Link if enabled
                        '
                        If iAllowCut Then
                            WorkingLink = genericController.modifyLinkQuery(cpCore.webServer.requestPage & "?" & cpCore.doc.refreshQueryString, RequestNameCut, genericController.encodeText(ContentID) & "." & genericController.encodeText(RecordID), True)
                            main_GetRecordEditLink2 = "" _
                                & main_GetRecordEditLink2 _
                                & "<a class=""ccRecordCutLink"" TabIndex=""-1"" href=""" & genericController.encodeHTML(WorkingLink) & """><img src=""/ccLib/images/Contentcut.gif"" border=""0"" alt=""Cut this " & ContentCaption & " to clipboard"" title=""Cut this " & ContentCaption & " to clipboard"" align=""absmiddle""></a>"
                        End If
                        '
                        ' Help link if enabled
                        '
                        Dim helpLink As String
                        helpLink = ""
                        'helpLink = main_GetHelpLink(5, "Editing " & ContentCaption, "Turn on Edit icons by checking 'Edit' in the tools panel, and click apply.<br><br><img src=""/ccLib/images/IconContentEdit.gif"" style=""vertical-align:middle""> Edit-Content icon<br><br>Edit-Content icons appear in your content. Click them to edit your content.")
                        main_GetRecordEditLink2 = "" _
                            & main_GetRecordEditLink2 _
                            & helpLink
                        '
                        main_GetRecordEditLink2 = "<span class=""ccRecordLinkCon"" style=""white-space:nowrap;"">" & main_GetRecordEditLink2 & "</span>"
                        ''
                        'main_GetRecordEditLink2 = "" _
                        '    & cr & "<div style=""position:absolute;"">" _
                        '    & genericController.kmaIndent(main_GetRecordEditLink2) _
                        '    & cr & "</div>"
                        '
                        'main_GetRecordEditLink2 = "" _
                        '    & cr & "<div style=""position:relative;display:inline;"">" _
                        '    & genericController.kmaIndent(main_GetRecordEditLink2) _
                        '    & cr & "</div>"
                    End If

                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' Print an add link for the current ContentSet
        '   iCSPointer is the content set to be added to
        '   PresetNameValueList is a name=value pair to force in the added record
        '========================================================================
        '
        Public Function main_cs_getRecordAddLink(ByVal CSPointer As Integer, Optional ByVal PresetNameValueList As String = "", Optional ByVal AllowPaste As Boolean = False) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("cs_getRecordAddLink")
            '
            'If Not (true) Then Exit Function
            '
            Dim ContentName As String
            Dim iPresetNameValueList As String
            Dim MethodName As String
            Dim iCSPointer As Integer
            '
            iCSPointer = genericController.EncodeInteger(CSPointer)
            iPresetNameValueList = genericController.encodeEmptyText(PresetNameValueList, "")
            '
            MethodName = "main_cs_getRecordAddLink"
            '
            If iCSPointer < 0 Then
                Throw (New ApplicationException("invalid ContentSet pointer [" & iCSPointer & "]")) ' handleLegacyError14(MethodName, "main_cs_getRecordAddLink was called with ")
            Else
                '
                ' Print an add tag to the iCSPointers Content
                '
                ContentName = cpCore.db.cs_getContentName(iCSPointer)
                If ContentName = "" Then
                    Throw (New ApplicationException("main_cs_getRecordAddLink was called with a ContentSet that was created with an SQL statement. The function requires a ContentSet opened with an OpenCSContent.")) ' handleLegacyError14(MethodName, "")
                Else
                    main_cs_getRecordAddLink = main_GetRecordAddLink(ContentName, iPresetNameValueList, AllowPaste)
                End If
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' main_GetRecordAddLink( iContentName, iPresetNameValueList )
        '
        '   Returns a string of add tags for the Content Definition included, and all
        '   child contents of that area.
        '
        '   iContentName The content for this link
        '   iPresetNameValueList The sql equivalent used to select the record.
        '           translates to name0=value0,name1=value1.. pairs separated by ,
        '
        '   LowestRootMenu - The Menu in the flyout structure that is the furthest down
        '   in the chain that the user has content access to. This is so a content manager
        '   does not have to navigate deep into a structure to main_Get to content he can
        '   edit.
        '   Basically, the entire menu is created down from the MenuName, and populated
        '   with all the entiries this user has access to. The LowestRequiredMenuName is
        '   is returned from the _branch routine, and that is to root on-which the
        '   main_GetMenu uses
        '========================================================================
        '
        Public Function main_GetRecordAddLink(ByVal ContentName As String, ByVal PresetNameValueList As String, Optional ByVal AllowPaste As Boolean = False) As String
            main_GetRecordAddLink = main_GetRecordAddLink2(genericController.encodeText(ContentName), genericController.encodeText(PresetNameValueList), AllowPaste, cpCore.authContext.isEditing(ContentName))
        End Function
        '
        '========================================================================
        ' main_GetRecordAddLink2
        '
        '   Returns a string of add tags for the Content Definition included, and all
        '   child contents of that area.
        '
        '   iContentName The content for this link
        '   iPresetNameValueList The sql equivalent used to select the record.
        '           translates to name0=value0,name1=value1.. pairs separated by ,
        '
        '   LowestRootMenu - The Menu in the flyout structure that is the furthest down
        '   in the chain that the user has content access to. This is so a content manager
        '   does not have to navigate deep into a structure to main_Get to content he can
        '   edit.
        '   Basically, the entire menu is created down from the MenuName, and populated
        '   with all the entiries this user has access to. The LowestRequiredMenuName is
        '   is returned from the _branch routine, and that is to root on-which the
        '   main_GetMenu uses
        '========================================================================
        '
        Public Function main_GetRecordAddLink2(ByVal ContentName As String, ByVal PresetNameValueList As String, ByVal AllowPaste As Boolean, ByVal IsEditing As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetRecordAddLink")
            '
            'If Not (true) Then Exit Function
            '
            Dim ParentID As Integer
            Dim BufferString As String
            Dim MethodName As String
            Dim iContentName As String
            Dim iContentID As Integer
            Dim iPresetNameValueList As String
            Dim MenuName As String
            Dim MenuHasBranches As Boolean
            Dim LowestRequiredMenuName As String = String.Empty
            Dim ClipBoard As String
            Dim PasteLink As String = String.Empty
            Dim Position As Integer
            Dim ClipBoardArray As String()
            Dim ClipboardContentID As Integer
            Dim ClipChildRecordID As Integer
            Dim iAllowPaste As Boolean
            Dim useFlyout As Boolean
            Dim csChildContent As Integer
            Dim Link As String
            '
            MethodName = "main_GetRecordAddLink"
            '
            main_GetRecordAddLink2 = ""
            If IsEditing Then
                iContentName = genericController.encodeText(ContentName)
                iPresetNameValueList = genericController.encodeText(PresetNameValueList)
                iPresetNameValueList = genericController.vbReplace(iPresetNameValueList, "&", ",")
                iAllowPaste = genericController.EncodeBoolean(AllowPaste)

                If iContentName = "" Then
                    Throw (New ApplicationException("Method called with blank ContentName")) ' handleLegacyError14(MethodName, "")
                Else
                    iContentID = cpCore.metaData.getContentId(iContentName)
                    csChildContent = cpCore.db.cs_open("Content", "ParentID=" & iContentID, , , , , , "id")
                    useFlyout = cpCore.db.cs_ok(csChildContent)
                    Call cpCore.db.cs_Close(csChildContent)
                    '
                    If Not useFlyout Then
                        Link = cpCore.siteProperties.adminURL & "?cid=" & iContentID & "&af=4&aa=2&ad=1"
                        If PresetNameValueList <> "" Then
                            Link = Link & "&wc=" & cpCore.html.main_EncodeRequestVariable(PresetNameValueList)
                        End If
                        main_GetRecordAddLink2 = main_GetRecordAddLink2 _
                            & "<a" _
                            & " TabIndex=-1" _
                            & " href=""" & genericController.encodeHTML(Link) & """"
                        If Not cpCore.html.main_ReturnAfterEdit Then
                            main_GetRecordAddLink2 = main_GetRecordAddLink2 & " target=""_blank"""
                        End If
                        main_GetRecordAddLink2 = main_GetRecordAddLink2 _
                            & "><img" _
                            & " src=""/ccLib/images/IconContentAdd.gif""" _
                            & " border=""0""" _
                            & " alt=""Add record""" _
                            & " title=""Add record""" _
                            & " align=""absmiddle""" _
                            & "></a>"
                    Else
                        '
                        MenuName = genericController.GetRandomInteger().ToString
                        Call cpCore.menuFlyout.menu_AddEntry(MenuName, , "/ccLib/images/IconContentAdd.gif", , , , "stylesheet", "stylesheethover")
                        LowestRequiredMenuName = main_GetRecordAddLink_AddMenuEntry(iContentName, iPresetNameValueList, "", MenuName, MenuName)
                    End If
                    '
                    ' Add in the paste entry, if needed
                    '
                    If iAllowPaste Then
                        ClipBoard = cpCore.visitProperty.getText("Clipboard", "")
                        If ClipBoard <> "" Then
                            Position = genericController.vbInstr(1, ClipBoard, ".")
                            If Position <> 0 Then
                                ClipBoardArray = Split(ClipBoard, ".")
                                If UBound(ClipBoardArray) > 0 Then
                                    ClipboardContentID = genericController.EncodeInteger(ClipBoardArray(0))
                                    ClipChildRecordID = genericController.EncodeInteger(ClipBoardArray(1))
                                    'iContentID = main_GetContentID(iContentName)
                                    If cpCore.metaData.isWithinContent(ClipboardContentID, iContentID) Then
                                        If genericController.vbInstr(1, iPresetNameValueList, "PARENTID=", vbTextCompare) <> 0 Then
                                            '
                                            ' must test for main_IsChildRecord
                                            '
                                            BufferString = iPresetNameValueList
                                            BufferString = genericController.vbReplace(BufferString, "(", "")
                                            BufferString = genericController.vbReplace(BufferString, ")", "")
                                            BufferString = genericController.vbReplace(BufferString, ",", "&")
                                            ParentID = genericController.EncodeInteger(genericController.main_GetNameValue_Internal(cpCore, BufferString, "Parentid"))
                                        End If


                                        If (ParentID <> 0) And (Not cpCore.doc.main_IsChildRecord(iContentName, ParentID, ClipChildRecordID)) Then
                                            '
                                            ' Can not paste as child of itself
                                            '
                                            PasteLink = cpCore.webServer.requestPage & "?" & cpCore.doc.refreshQueryString
                                            PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePaste, "1", True)
                                            PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteParentContentID, CStr(iContentID), True)
                                            PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteParentRecordID, CStr(ParentID), True)
                                            PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteFieldList, iPresetNameValueList, True)
                                            main_GetRecordAddLink2 = main_GetRecordAddLink2 _
                                                & "<a class=""ccRecordCutLink"" TabIndex=""-1"" href=""" & genericController.encodeHTML(PasteLink) & """><img src=""/ccLib/images/ContentPaste.gif"" border=""0"" alt=""Paste record in clipboard here"" title=""Paste record in clipboard here"" align=""absmiddle""></a>"
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                    '
                    ' Add in the available flyout Navigator Entries
                    '
                    If LowestRequiredMenuName <> "" Then
                        main_GetRecordAddLink2 = main_GetRecordAddLink2 & cpCore.menuFlyout.getMenu(LowestRequiredMenuName, 0)
                        main_GetRecordAddLink2 = genericController.vbReplace(main_GetRecordAddLink2, "class=""ccFlyoutButton"" ", "", 1, 99, vbTextCompare)
                        If PasteLink <> "" Then
                            main_GetRecordAddLink2 = main_GetRecordAddLink2 & "<a TabIndex=-1 href=""" & genericController.encodeHTML(PasteLink) & """><img src=""/ccLib/images/ContentPaste.gif"" border=""0"" alt=""Paste content from clipboard"" align=""absmiddle""></a>"
                        End If
                    End If
                    '
                    ' Help link if enabled
                    '
                    Dim helpLink As String
                    helpLink = ""
                    'helpLink = main_GetHelpLink(6, "Adding " & iContentName, "Turn on Edit icons by checking 'Edit' in the tools panel, and click apply.<br><br><img src=""/ccLib/images/IconContentAdd.gif"" " & IconWidthHeight & " style=""vertical-align:middle""> Add-Content icon<br><br>Add-Content icons appear in your content. Click them to add content.")
                    main_GetRecordAddLink2 = main_GetRecordAddLink2 & helpLink                '
                    If main_GetRecordAddLink2 <> "" Then
                        main_GetRecordAddLink2 = "" _
                            & vbCrLf & vbTab & "<div style=""display:inline;"">" _
                            & genericController.htmlIndent(main_GetRecordAddLink2) _
                            & vbCrLf & vbTab & "</div>"
                    End If
                    '
                    ' ----- Add the flyout panels to the content to return
                    '       This must be here so if the call is made after main_ClosePage, the panels will still deliver
                    '
                    If LowestRequiredMenuName <> "" Then
                        main_GetRecordAddLink2 = main_GetRecordAddLink2 & cpCore.menuFlyout.menu_GetClose()
                        If genericController.vbInstr(1, main_GetRecordAddLink2, "IconContentAdd.gif", vbTextCompare) <> 0 Then
                            main_GetRecordAddLink2 = genericController.vbReplace(main_GetRecordAddLink2, "IconContentAdd.gif"" ", "IconContentAdd.gif"" align=""absmiddle"" ")
                        End If
                    End If
                    If cpCore.html.main_ReturnAfterEdit Then
                        main_GetRecordAddLink2 = genericController.vbReplace(main_GetRecordAddLink2, "target=", "xtarget=", 1, 99, vbTextCompare)
                    End If
                    'End If
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' main_GetRecordAddLink_AddMenuEntry( ContentName, PresetNameValueList, ContentNameList, MenuName )
        '
        '   adds an add entry for the content name, and all the child content
        '   returns the MenuName of the lowest branch that has valid
        '   Navigator Entries.
        '
        '   ContentName The content for this link
        '   PresetNameValueList The sql equivalent used to select the record.
        '           translates to (name0=value0)&(name1=value1).. pairs separated by &
        '   ContentNameList is a comma separated list of names of the content included so far
        '   MenuName is the name of the root branch, for flyout menu
        '
        '   IsMember(), main_IsAuthenticated() And Member_AllowLinkAuthoring must already be checked
        '========================================================================
        '
        Private Function main_GetRecordAddLink_AddMenuEntry(ByVal ContentName As String, ByVal PresetNameValueList As String, ByVal ContentNameList As String, ByVal MenuName As String, ByVal ParentMenuName As String) As String
            Dim result As String = ""
            Dim Copy As String
            Dim CS As Integer
            Dim SQL As String
            Dim csChildContent As Integer
            Dim ContentID As Integer
            Dim Link As String
            Dim MyContentNameList As String
            Dim ButtonCaption As String
            Dim ContentRecordFound As Boolean
            Dim ContentAllowAdd As Boolean
            Dim GroupRulesAllowAdd As Boolean
            Dim MemberRulesDateExpires As Date
            Dim MemberRulesAllow As Boolean
            Dim ChildMenuButtonCount As Integer
            Dim ChildMenuName As String
            Dim ChildContentName As String
            '
            Link = ""
            MyContentNameList = ContentNameList
            If (ContentName = "") Then
                Throw (New ApplicationException("main_GetRecordAddLink, ContentName is empty")) ' handleLegacyError14(MethodName, "")
            Else
                If (InStr(1, MyContentNameList, "," & genericController.vbUCase(ContentName) & ",") >= 0) Then
                    Throw (New ApplicationException("result , Content Child [" & ContentName & "] is one of its own parents")) ' handleLegacyError14(MethodName, "")
                Else
                    MyContentNameList = MyContentNameList & "," & genericController.vbUCase(ContentName) & ","
                    '
                    ' ----- Select the Content Record for the Menu Entry selected
                    '
                    ContentRecordFound = False
                    If cpCore.authContext.isAuthenticatedAdmin(cpCore) Then
                        '
                        ' ----- admin member, they have access, main_Get ContentID and set markers true
                        '
                        SQL = "SELECT ID as ContentID, AllowAdd as ContentAllowAdd, 1 as GroupRulesAllowAdd, null as MemberRulesDateExpires" _
                            & " FROM ccContent" _
                            & " WHERE (" _
                            & " (ccContent.Name=" & cpCore.db.encodeSQLText(ContentName) & ")" _
                            & " AND(ccContent.active<>0)" _
                            & " );"
                        CS = cpCore.db.cs_openSql(SQL)
                        If cpCore.db.cs_ok(CS) Then
                            '
                            ' Entry was found
                            '
                            ContentRecordFound = True
                            ContentID = cpCore.db.cs_getInteger(CS, "ContentID")
                            ContentAllowAdd = cpCore.db.cs_getBoolean(CS, "ContentAllowAdd")
                            GroupRulesAllowAdd = True
                            MemberRulesDateExpires = Date.MinValue
                            MemberRulesAllow = True
                        End If
                        Call cpCore.db.cs_Close(CS)
                    Else
                        '
                        ' non-admin member, first check if they have access and main_Get true markers
                        '
                        SQL = "SELECT ccContent.ID as ContentID, ccContent.AllowAdd as ContentAllowAdd, ccGroupRules.AllowAdd as GroupRulesAllowAdd, ccMemberRules.DateExpires as MemberRulesDateExpires" _
                            & " FROM (((ccContent" _
                                & " LEFT JOIN ccGroupRules ON ccGroupRules.ContentID=ccContent.ID)" _
                                & " LEFT JOIN ccgroups ON ccGroupRules.GroupID=ccgroups.ID)" _
                                & " LEFT JOIN ccMemberRules ON ccgroups.ID=ccMemberRules.GroupID)" _
                                & " LEFT JOIN ccMembers ON ccMemberRules.MemberID=ccMembers.ID" _
                            & " WHERE (" _
                            & " (ccContent.Name=" & cpCore.db.encodeSQLText(ContentName) & ")" _
                            & " AND(ccContent.active<>0)" _
                            & " AND(ccGroupRules.active<>0)" _
                            & " AND(ccMemberRules.active<>0)" _
                            & " AND((ccMemberRules.DateExpires is Null)or(ccMemberRules.DateExpires>" & cpCore.db.encodeSQLDate(cpCore.profileStartTime) & "))" _
                            & " AND(ccgroups.active<>0)" _
                            & " AND(ccMembers.active<>0)" _
                            & " AND(ccMembers.ID=" & cpCore.authContext.user.id & ")" _
                            & " );"
                        CS = cpCore.db.cs_openSql(SQL)
                        If cpCore.db.cs_ok(CS) Then
                            '
                            ' ----- Entry was found, member has some kind of access
                            '
                            ContentRecordFound = True
                            ContentID = cpCore.db.cs_getInteger(CS, "ContentID")
                            ContentAllowAdd = cpCore.db.cs_getBoolean(CS, "ContentAllowAdd")
                            GroupRulesAllowAdd = cpCore.db.cs_getBoolean(CS, "GroupRulesAllowAdd")
                            MemberRulesDateExpires = cpCore.db.cs_getDate(CS, "MemberRulesDateExpires")
                            MemberRulesAllow = False
                            If MemberRulesDateExpires = Date.MinValue Then
                                MemberRulesAllow = True
                            ElseIf (MemberRulesDateExpires > cpCore.profileStartTime) Then
                                MemberRulesAllow = True
                            End If
                        Else
                            '
                            ' ----- No entry found, this member does not have access, just main_Get ContentID
                            '
                            ContentRecordFound = True
                            ContentID = cpCore.metaData.getContentId(ContentName)
                            ContentAllowAdd = False
                            GroupRulesAllowAdd = False
                            MemberRulesAllow = False
                        End If
                        Call cpCore.db.cs_Close(CS)
                    End If
                    If ContentRecordFound Then
                        '
                        ' Add the Menu Entry* to the current menu (MenuName)
                        '
                        Link = ""
                        ButtonCaption = ContentName
                        result = MenuName
                        If ContentAllowAdd And GroupRulesAllowAdd And MemberRulesAllow Then
                            Link = cpCore.siteProperties.adminURL & "?cid=" & ContentID & "&af=4&aa=2&ad=1"
                            If PresetNameValueList <> "" Then
                                Dim NameValueList As String
                                NameValueList = PresetNameValueList
                                Link = Link & "&wc=" & cpCore.html.main_EncodeRequestVariable(PresetNameValueList)
                            End If
                        End If
                        Call cpCore.menuFlyout.menu_AddEntry(MenuName & ":" & ContentName, ParentMenuName, , , Link, ButtonCaption, "", "", True)
                        '
                        ' Create child submenu if Child Entries found
                        '
                        csChildContent = cpCore.db.cs_open("Content", "ParentID=" & ContentID, , , , , , "name")
                        If Not cpCore.db.cs_ok(csChildContent) Then
                            '
                            ' No child menu
                            '
                        Else
                            '
                            ' Add the child menu
                            '
                            ChildMenuName = MenuName & ":" & ContentName
                            ChildMenuButtonCount = 0
                            '
                            ' ----- Create the ChildPanel with all Children found
                            '
                            Do While cpCore.db.cs_ok(csChildContent)
                                ChildContentName = cpCore.db.cs_getText(csChildContent, "name")
                                Copy = main_GetRecordAddLink_AddMenuEntry(ChildContentName, PresetNameValueList, MyContentNameList, MenuName, ParentMenuName)
                                If Copy <> "" Then
                                    ChildMenuButtonCount = ChildMenuButtonCount + 1
                                End If
                                If (result = "") And (Copy <> "") Then
                                    result = Copy
                                End If
                                cpCore.db.cs_goNext(csChildContent)
                            Loop
                        End If
                    End If
                End If
                Call cpCore.db.cs_Close(csChildContent)
            End If
            Return result
        End Function
        Public Sub main_ClearStream()
            cpCore.doc.docBuffer = ""
            cpCore.webServer.bufferRedirect = ""
            cpCore.webServer.bufferResponseHeader = ""
        End Sub
        Public Function cs_cs_getRecordEditLink(ByVal CSPointer As Integer, Optional ByVal AllowCut As Object = False) As String
            Dim result As String = ""

            Dim RecordName As String
            Dim ContentName As String
            Dim RecordID As Integer
            Dim ContentControlID As Integer
            Dim iCSPointer As Integer
            '
            iCSPointer = genericController.EncodeInteger(CSPointer)
            If iCSPointer = -1 Then
                Throw (New ApplicationException("main_cs_getRecordEditLink called with invalid iCSPointer")) ' handleLegacyError14(MethodName, "")
            Else
                If Not cpCore.db.cs_ok(iCSPointer) Then
                    Throw (New ApplicationException("main_cs_getRecordEditLink called with Not main_CSOK")) ' handleLegacyError14(MethodName, "")
                Else
                    '
                    ' Print an edit link for the records Content (may not be iCSPointer content)
                    '
                    RecordID = (cpCore.db.cs_getInteger(iCSPointer, "ID"))
                    RecordName = cpCore.db.cs_getText(iCSPointer, "Name")
                    ContentControlID = (cpCore.db.cs_getInteger(iCSPointer, "contentcontrolid"))
                    ContentName = cpCore.metaData.getContentNameByID(ContentControlID)
                    If ContentName <> "" Then
                        result = cpCore.html.main_GetRecordEditLink2(ContentName, RecordID, genericController.EncodeBoolean(AllowCut), RecordName, cpCore.authContext.isEditing(ContentName))
                    End If
                End If
            End If
            Return result
        End Function
        '
        '========================================================================
        '   main_GetPanel( Panel, Optional StylePanel, Optional StyleHilite, Optional StyleShadow, Optional Width, Optional Padding, Optional HeightMin) As String
        ' Return a panel with the input as center
        '========================================================================
        '
        Public Function main_GetPanel(ByVal Panel As String, Optional ByVal StylePanel As String = "", Optional ByVal StyleHilite As String = "ccPanelHilite", Optional ByVal StyleShadow As String = "ccPanelShadow", Optional ByVal Width As String = "100%", Optional ByVal Padding As Integer = 5, Optional ByVal HeightMin As Integer = 1) As String
            Dim ContentPanelWidth As String
            Dim MethodName As String
            Dim MyStylePanel As String
            Dim MyStyleHilite As String
            Dim MyStyleShadow As String
            Dim MyWidth As String
            Dim MyPadding As String
            Dim MyHeightMin As String
            Dim s0 As String
            Dim s1 As String
            Dim s2 As String
            Dim s3 As String
            Dim s4 As String
            Dim contentPanelWidthStyle As String
            '
            MethodName = "main_GetPanelTop"
            '
            MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel")
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite")
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow")
            MyWidth = genericController.encodeEmptyText(Width, "100%")
            MyPadding = Padding.ToString
            MyHeightMin = HeightMin.ToString
            '
            If genericController.vbIsNumeric(MyWidth) Then
                ContentPanelWidth = (CInt(MyWidth) - 2).ToString
                contentPanelWidthStyle = ContentPanelWidth & "px"
            Else
                ContentPanelWidth = "100%"
                contentPanelWidthStyle = ContentPanelWidth
            End If
            '
            '
            '
            s0 = "" _
                & cr & "<td style=""padding:" & MyPadding & "px;vertical-align:top"" class=""" & MyStylePanel & """>" _
                & genericController.htmlIndent(genericController.encodeText(Panel)) _
                & cr & "</td>" _
                & ""
            '
            s1 = "" _
                & cr & "<tr>" _
                & genericController.htmlIndent(s0) _
                & cr & "</tr>" _
                & ""
            s2 = "" _
                & cr & "<table style=""width:" & contentPanelWidthStyle & ";border:0px;"" class=""" & MyStylePanel & """ cellspacing=""0"">" _
                & genericController.htmlIndent(s1) _
                & cr & "</table>" _
                & ""
            s3 = "" _
                & cr & "<td width=""1"" height=""" & MyHeightMin & """ class=""" & MyStyleHilite & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""" & MyHeightMin & """ width=""1"" ></td>" _
                & cr & "<td width=""" & ContentPanelWidth & """ valign=""top"" align=""left"" class=""" & MyStylePanel & """>" _
                & genericController.htmlIndent(s2) _
                & cr & "</td>" _
                & cr & "<td width=""1"" class=""" & MyStyleShadow & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""1"" ></td>" _
                & ""
            s4 = "" _
                & cr & "<tr>" _
                & cr2 & "<td colspan=""3"" class=""" & MyStyleHilite & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""" & MyWidth & """ ></td>" _
                & cr & "</tr>" _
                & cr & "<tr>" _
                & genericController.htmlIndent(s3) _
                & cr & "</tr>" _
                & cr & "<tr>" _
                & cr2 & "<td colspan=""3"" class=""" & MyStyleShadow & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""" & MyWidth & """ ></td>" _
                & cr & "</tr>" _
                & ""
            main_GetPanel = "" _
                & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""" & MyWidth & """ class=""" & MyStylePanel & """>" _
                & genericController.htmlIndent(s4) _
                & cr & "</table>" _
                & ""
        End Function
        '
        '========================================================================
        '   main_GetPanel( Panel, Optional StylePanel, Optional StyleHilite, Optional StyleShadow, Optional Width, Optional Padding, Optional HeightMin) As String
        ' Return a panel with the input as center
        '========================================================================
        '
        Public Function main_GetReversePanel(ByVal Panel As String, Optional ByVal StylePanel As String = "", Optional ByVal StyleHilite As String = "ccPanelShadow", Optional ByVal StyleShadow As String = "ccPanelHilite", Optional ByVal Width As String = "", Optional ByVal Padding As String = "", Optional ByVal HeightMin As String = "") As String
            Dim MyStyleHilite As String
            Dim MyStyleShadow As String
            '
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelShadow")
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelHilite")

            main_GetReversePanel = main_GetPanelTop(StylePanel, MyStyleHilite, MyStyleShadow, Width, Padding, HeightMin) _
                & genericController.encodeText(Panel) _
                & main_GetPanelBottom(StylePanel, MyStyleHilite, MyStyleShadow, Width, Padding)
        End Function
        '
        '========================================================================
        ' Return a panel header with the header message reversed out of the left
        '========================================================================
        '
        Public Function main_GetPanelHeader(ByVal HeaderMessage As String, Optional ByVal RightSideMessage As String = "") As String
            Dim iHeaderMessage As String
            Dim iRightSideMessage As String
            Dim Adminui As New adminUIController(cpCore)
            '
            'If Not (true) Then Exit Function
            '
            iHeaderMessage = genericController.encodeText(HeaderMessage)
            iRightSideMessage = genericController.encodeEmptyText(RightSideMessage, FormatDateTime(cpCore.profileStartTime))
            main_GetPanelHeader = Adminui.GetHeader(iHeaderMessage, iRightSideMessage)
        End Function

        '
        '========================================================================
        ' Prints the top of display panel
        '   Must be closed with PrintPanelBottom
        '========================================================================
        '
        Public Function main_GetPanelTop(Optional ByVal StylePanel As String = "", Optional ByVal StyleHilite As String = "", Optional ByVal StyleShadow As String = "", Optional ByVal Width As String = "", Optional ByVal Padding As String = "", Optional ByVal HeightMin As String = "") As String
            Dim ContentPanelWidth As String
            Dim MethodName As String
            Dim MyStylePanel As String
            Dim MyStyleHilite As String
            Dim MyStyleShadow As String
            Dim MyWidth As String
            Dim MyPadding As String
            Dim MyHeightMin As String
            '
            main_GetPanelTop = ""
            MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel")
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite")
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow")
            MyWidth = genericController.encodeEmptyText(Width, "100%")
            MyPadding = genericController.encodeEmptyText(Padding, "5")
            MyHeightMin = genericController.encodeEmptyText(HeightMin, "1")
            MethodName = "main_GetPanelTop"
            If genericController.vbIsNumeric(MyWidth) Then
                ContentPanelWidth = (CInt(MyWidth) - 2).ToString
            Else
                ContentPanelWidth = "100%"
            End If
            main_GetPanelTop = main_GetPanelTop _
                & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""" & MyWidth & """ class=""" & MyStylePanel & """>"
            '
            ' --- top hilite row
            '
            main_GetPanelTop = main_GetPanelTop _
                & cr2 & "<tr>" _
                & cr3 & "<td colspan=""3"" class=""" & MyStyleHilite & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""" & MyWidth & """ ></td>" _
                & cr2 & "</tr>"
            '
            ' --- center row with Panel
            '
            main_GetPanelTop = main_GetPanelTop _
                & cr2 & "<tr>" _
                & cr3 & "<td width=""1"" height=""" & MyHeightMin & """ class=""" & MyStyleHilite & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""" & MyHeightMin & """ width=""1"" ></td>" _
                & cr3 & "<td width=""" & ContentPanelWidth & """ valign=""top"" align=""left"" class=""" & MyStylePanel & """>" _
                & cr4 & "<table border=""0"" cellpadding=""" & MyPadding & """ cellspacing=""0"" width=""" & ContentPanelWidth & """ class=""" & MyStylePanel & """>" _
                & cr5 & "<tr>" _
                & cr6 & "<td valign=""top"" class=""" & MyStylePanel & """><Span class=""" & MyStylePanel & """>"
        End Function
        '
        '========================================================================
        ' Return a panel with the input as center
        '========================================================================
        '
        Public Function main_GetPanelBottom(Optional ByVal StylePanel As String = "", Optional ByVal StyleHilite As String = "", Optional ByVal StyleShadow As String = "", Optional ByVal Width As String = "", Optional ByVal Padding As String = "") As String
            Dim result As String = String.Empty
            Try
                'Dim MyStylePanel As String
                'Dim MyStyleHilite As String
                Dim MyStyleShadow As String
                Dim MyWidth As String
                'Dim MyPadding As String
                '
                'MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel")
                'MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite")
                MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow")
                MyWidth = genericController.encodeEmptyText(Width, "100%")
                'MyPadding = genericController.encodeEmptyText(Padding, "5")
                '
                result = result _
                    & cr6 & "</span></td>" _
                    & cr5 & "</tr>" _
                    & cr4 & "</table>" _
                    & cr3 & "</td>" _
                    & cr3 & "<td width=""1"" class=""" & MyStyleShadow & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""1"" ></td>" _
                    & cr2 & "</tr>" _
                    & cr2 & "<tr>" _
                    & cr3 & "<td colspan=""3"" class=""" & MyStyleShadow & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""" & MyWidth & """ ></td>" _
                    & cr2 & "</tr>" _
                    & cr & "</table>"
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function main_GetPanelButtons(ByVal ButtonValueList As String, ByVal ButtonName As String, Optional ByVal PanelWidth As String = "", Optional ByVal PanelHeightMin As String = "") As String
            Dim Adminui As New adminUIController(cpCore)
            main_GetPanelButtons = Adminui.GetButtonBar(Adminui.GetButtonsFromList(ButtonValueList, True, True, ButtonName), "")
        End Function
        '
        '
        '
        Public Function main_GetPanelRev(ByVal PanelContent As String, Optional ByVal PanelWidth As String = "", Optional ByVal PanelHeightMin As String = "") As String
            main_GetPanelRev = main_GetPanel(PanelContent, "ccPanel", "ccPanelShadow", "ccPanelHilite", PanelWidth, 2, genericController.EncodeInteger(PanelHeightMin))
        End Function
        '
        '
        '
        Public Function main_GetPanelInput(ByVal PanelContent As String, Optional ByVal PanelWidth As String = "", Optional ByVal PanelHeightMin As String = "1") As String
            main_GetPanelInput = main_GetPanel(PanelContent, "ccPanelInput", "ccPanelShadow", "ccPanelHilite", PanelWidth, 2, genericController.EncodeInteger(PanelHeightMin))
        End Function
        '
        '========================================================================
        ' Print the tools panel at the bottom of the page
        '========================================================================
        '
        Public Function main_GetToolsPanel() As String
            Dim result As String = String.Empty
            Try
                Dim copyNameValue As String
                Dim CopyName As String
                Dim copyValue As String
                Dim copyNameValueSplit() As String
                Dim VisitMin As Integer
                Dim VisitHrs As Integer
                Dim VisitSec As Integer
                Dim DebugPanel As String = String.Empty
                Dim Copy As String
                Dim CopySplit() As String
                Dim Ptr As Integer
                Dim EditTagID As String
                Dim QuickEditTagID As String
                Dim AdvancedEditTagID As String
                Dim WorkflowTagID As String
                Dim Tag As String
                Dim MethodName As String
                Dim TagID As String
                Dim ToolsPanel As stringBuilderLegacyController
                Dim OptionsPanel As String = String.Empty
                Dim LinkPanel As stringBuilderLegacyController
                Dim LoginPanel As String = String.Empty
                Dim iValueBoolean As Boolean
                Dim WorkingQueryString As String
                Dim BubbleCopy As String
                Dim AnotherPanel As stringBuilderLegacyController
                Dim Adminui As New adminUIController(cpCore)
                Dim ShowLegacyToolsPanel As Boolean
                Dim QS As String
                '
                MethodName = "main_GetToolsPanel"
                '
                If cpCore.authContext.user.AllowToolsPanel Then
                    ShowLegacyToolsPanel = cpCore.siteProperties.getBoolean("AllowLegacyToolsPanel", True)
                    '
                    ' --- Link Panel - used for both Legacy Tools Panel, and without it
                    '
                    LinkPanel = New stringBuilderLegacyController
                    LinkPanel.Add(SpanClassAdminSmall)
                    LinkPanel.Add("Contensive " & cpCore.codeVersion() & " | ")
                    LinkPanel.Add(FormatDateTime(cpCore.profileStartTime) & " | ")
                    LinkPanel.Add("<a class=""ccAdminLink"" target=""_blank"" href=""http://support.Contensive.com/"">Support</A> | ")
                    LinkPanel.Add("<a class=""ccAdminLink"" href=""" & genericController.encodeHTML(cpCore.siteProperties.adminURL) & """>Admin Home</A> | ")
                    LinkPanel.Add("<a class=""ccAdminLink"" href=""" & genericController.encodeHTML("http://" & cpCore.webServer.requestDomain) & """>Public Home</A> | ")
                    LinkPanel.Add("<a class=""ccAdminLink"" target=""_blank"" href=""" & genericController.encodeHTML(cpCore.siteProperties.adminURL & "?" & RequestNameHardCodedPage & "=" & HardCodedPageMyProfile) & """>My Profile</A> | ")
                    If cpCore.siteProperties.getBoolean("AllowMobileTemplates", False) Then
                        If cpCore.authContext.visit.Mobile Then
                            QS = cpCore.doc.refreshQueryString
                            QS = genericController.ModifyQueryString(QS, "method", "forcenonmobile")
                            LinkPanel.Add("<a class=""ccAdminLink"" href=""?" & QS & """>Non-Mobile Version</A> | ")
                        Else
                            QS = cpCore.doc.refreshQueryString
                            QS = genericController.ModifyQueryString(QS, "method", "forcemobile")
                            LinkPanel.Add("<a class=""ccAdminLink"" href=""?" & QS & """>Mobile Version</A> | ")
                        End If
                    End If
                    LinkPanel.Add("</span>")
                    '
                    If ShowLegacyToolsPanel Then
                        ToolsPanel = New stringBuilderLegacyController
                        WorkingQueryString = genericController.ModifyQueryString(cpCore.doc.refreshQueryString, "ma", "", False)
                        '
                        ' ----- Tools Panel Caption
                        '
                        Dim helpLink As String
                        helpLink = ""
                        'helpLink = main_GetHelpLink("2", "Contensive Tools Panel", BubbleCopy)
                        BubbleCopy = "Use the Tools Panel to enable features such as editing and debugging tools. It also includes links to the admin site, the support site and the My Profile page."
                        result = result & main_GetPanelHeader("Contensive Tools Panel" & helpLink)
                        '
                        ToolsPanel.Add(cpCore.html.html_GetFormStart(WorkingQueryString))
                        ToolsPanel.Add(cpCore.html.html_GetFormInputHidden("Type", FormTypeToolsPanel))
                        '
                        If True Then
                            '
                            ' ----- Create the Options Panel
                            '
                            'PathsContentID = main_GetContentID("Paths")
                            '                '
                            '                ' Allow Help Links
                            '                '
                            '                iValueBoolean = visitProperty.getboolean("AllowHelpIcon")
                            '                TagID =  "AllowHelpIcon"
                            '                OptionsPanel = OptionsPanel & "" _
                            '                    & CR & "<div class=""ccAdminSmall"">" _
                            '                    & cr2 & "<LABEL for=""" & TagID & """>" & main_GetFormInputCheckBox2(TagID, iValueBoolean, TagID) & "&nbsp;Help</LABEL>" _
                            '                    & CR & "</div>"
                            '
                            EditTagID = "AllowEditing"
                            QuickEditTagID = "AllowQuickEditor"
                            AdvancedEditTagID = "AllowAdvancedEditor"
                            WorkflowTagID = "AllowWorkflowRendering"
                            '
                            ' Edit
                            '
                            helpLink = ""
                            'helpLink = main_GetHelpLink(7, "Enable Editing", "Display the edit tools for basic content, such as pages, copy and sections. ")
                            iValueBoolean = cpCore.visitProperty.getBoolean("AllowEditing")
                            Tag = cpCore.html.html_GetFormInputCheckBox2(EditTagID, iValueBoolean, EditTagID)
                            Tag = genericController.vbReplace(Tag, ">", " onClick=""document.getElementById('" & QuickEditTagID & "').checked=false;document.getElementById('" & AdvancedEditTagID & "').checked=false;"">")
                            OptionsPanel = OptionsPanel _
                            & cr & "<div class=""ccAdminSmall"">" _
                            & cr2 & "<LABEL for=""" & EditTagID & """>" & Tag & "&nbsp;Edit</LABEL>" & helpLink _
                            & cr & "</div>"
                            '
                            ' Quick Edit
                            '
                            helpLink = ""
                            'helpLink = main_GetHelpLink(8, "Enable Quick Edit", "Display the quick editor to edit the main page content.")
                            iValueBoolean = cpCore.visitProperty.getBoolean("AllowQuickEditor")
                            Tag = cpCore.html.html_GetFormInputCheckBox2(QuickEditTagID, iValueBoolean, QuickEditTagID)
                            Tag = genericController.vbReplace(Tag, ">", " onClick=""document.getElementById('" & EditTagID & "').checked=false;document.getElementById('" & AdvancedEditTagID & "').checked=false;"">")
                            OptionsPanel = OptionsPanel _
                            & cr & "<div class=""ccAdminSmall"">" _
                            & cr2 & "<LABEL for=""" & QuickEditTagID & """>" & Tag & "&nbsp;Quick Edit</LABEL>" & helpLink _
                            & cr & "</div>"
                            '
                            ' Advanced Edit
                            '
                            helpLink = ""
                            'helpLink = main_GetHelpLink(0, "Enable Advanced Edit", "Display the edit tools for advanced content, such as templates and add-ons. Basic content edit tools are also displayed.")
                            iValueBoolean = cpCore.visitProperty.getBoolean("AllowAdvancedEditor")
                            Tag = cpCore.html.html_GetFormInputCheckBox2(AdvancedEditTagID, iValueBoolean, AdvancedEditTagID)
                            Tag = genericController.vbReplace(Tag, ">", " onClick=""document.getElementById('" & QuickEditTagID & "').checked=false;document.getElementById('" & EditTagID & "').checked=false;"">")
                            OptionsPanel = OptionsPanel _
                            & cr & "<div class=""ccAdminSmall"">" _
                            & cr2 & "<LABEL for=""" & AdvancedEditTagID & """>" & Tag & "&nbsp;Advanced Edit</LABEL>" & helpLink _
                            & cr & "</div>"
                            '
                            ' Workflow Authoring Render Mode
                            '
                            helpLink = ""
                            'helpLink = main_GetHelpLink(9, "Enable Workflow Rendering", "Control the display of workflow rendering. With workflow rendering enabled, any changes saved to content records that have not been published will be visible for your review.")
                            If cpCore.siteProperties.allowWorkflowAuthoring Then
                                iValueBoolean = cpCore.visitProperty.getBoolean("AllowWorkflowRendering")
                                Tag = cpCore.html.html_GetFormInputCheckBox2(WorkflowTagID, iValueBoolean, WorkflowTagID)
                                OptionsPanel = OptionsPanel _
                                & cr & "<div class=""ccAdminSmall"">" _
                                & cr2 & "<LABEL for=""" & WorkflowTagID & """>" & Tag & "&nbsp;Render Workflow Authoring Changes</LABEL>" & helpLink _
                                & cr & "</div>"
                            End If
                            helpLink = ""
                            iValueBoolean = cpCore.visitProperty.getBoolean("AllowDebugging")
                            TagID = "AllowDebugging"
                            Tag = cpCore.html.html_GetFormInputCheckBox2(TagID, iValueBoolean, TagID)
                            OptionsPanel = OptionsPanel _
                            & cr & "<div class=""ccAdminSmall"">" _
                            & cr2 & "<LABEL for=""" & TagID & """>" & Tag & "&nbsp;Debug</LABEL>" & helpLink _
                            & cr & "</div>"
                            ''
                            '' Create Path Block Row
                            ''
                            'If cpCore.authContext.isAuthenticatedDeveloper(cpCore) Then
                            '    TagID = "CreatePathBlock"
                            '    If cpCore.siteProperties.allowPathBlocking Then
                            '        '
                            '        ' Path blocking allowed
                            '        '
                            '        'OptionsPanel = OptionsPanel & SpanClassAdminSmall & "<LABEL for=""" & TagID & """>"
                            '        CS = cpCore.db.cs_open("Paths", "name=" & cpCore.db.encodeSQLText(cpCore.webServer.requestPath), , , , , , "ID")
                            '        If cpCore.db.cs_ok(CS) Then
                            '            PathID = (cpCore.db.cs_getInteger(CS, "ID"))
                            '        End If
                            '        Call cpCore.db.cs_Close(CS)
                            '        If PathID <> 0 Then
                            '            '
                            '            ' Path is blocked
                            '            '
                            '            Tag = cpCore.html.html_GetFormInputCheckBox2(TagID, True, TagID) & "&nbsp;Path is blocked [" & cpCore.webServer.requestPath & "] [<a href=""" & genericController.encodeHTML(cpCore.siteProperties.adminURL & "?af=" & AdminFormEdit & "&id=" & PathID & "&cid=" & cpCore.metaData.getContentId("paths") & "&ad=1") & """ target=""_blank"">edit</a>]</LABEL>"
                            '        Else
                            '            '
                            '            ' Path is not blocked
                            '            '
                            '            Tag = cpCore.html.html_GetFormInputCheckBox2(TagID, False, TagID) & "&nbsp;Block this path [" & cpCore.webServer.requestPath & "]</LABEL>"
                            '        End If
                            '        helpLink = ""
                            '        'helpLink = main_GetHelpLink(10, "Enable Debugging", "Debugging is a developer only debugging tool. With Debugging enabled, ccLib.TestPoints(...) will print, ErrorTrapping will be displayed, redirections are blocked, and more.")
                            '        OptionsPanel = OptionsPanel _
                            '        & cr & "<div class=""ccAdminSmall"">" _
                            '        & cr2 & "<LABEL for=""" & TagID & """>" & Tag & "</LABEL>" & helpLink _
                            '        & cr & "</div>"
                            '    End If
                            'End If
                            '
                            ' Buttons
                            '
                            OptionsPanel = OptionsPanel & "" _
                            & cr & "<div class=""ccButtonCon"">" _
                            & cr2 & "<input type=submit name=" & "mb value=""" & ButtonApply & """>" _
                            & cr & "</div>" _
                            & ""
                        End If
                        '
                        ' ----- Create the Login Panel
                        '
                        If Trim(cpCore.authContext.user.Name) = "" Then
                            Copy = "You are logged in as member #" & cpCore.authContext.user.id & "."
                        Else
                            Copy = "You are logged in as " & cpCore.authContext.user.Name & "."
                        End If
                        LoginPanel = LoginPanel & "" _
                        & cr & "<div class=""ccAdminSmall"">" _
                        & cr2 & Copy & "" _
                        & cr & "</div>"
                        '
                        ' Username
                        '
                        Dim Caption As String
                        If cpCore.siteProperties.getBoolean("allowEmailLogin", False) Then
                            Caption = "Username&nbsp;or&nbsp;Email"
                        Else
                            Caption = "Username"
                        End If
                        TagID = "Username"
                        LoginPanel = LoginPanel & "" _
                        & cr & "<div class=""ccAdminSmall"">" _
                        & cr2 & "<LABEL for=""" & TagID & """>" & cpCore.html.html_GetFormInputText2(TagID, "", 1, 30, TagID, False) & "&nbsp;" & Caption & "</LABEL>" _
                        & cr & "</div>"
                        '
                        ' Username
                        '
                        If cpCore.siteProperties.getBoolean("allownopasswordLogin", False) Then
                            Caption = "Password&nbsp;(optional)"
                        Else
                            Caption = "Password"
                        End If
                        TagID = "Password"
                        LoginPanel = LoginPanel & "" _
                        & cr & "<div class=""ccAdminSmall"">" _
                        & cr2 & "<LABEL for=""" & TagID & """>" & cpCore.html.html_GetFormInputText2(TagID, "", 1, 30, TagID, True) & "&nbsp;" & Caption & "</LABEL>" _
                        & cr & "</div>"
                        '
                        ' Autologin checkbox
                        '
                        If cpCore.siteProperties.getBoolean("AllowAutoLogin", False) Then
                            If cpCore.authContext.visit.CookieSupport Then
                                TagID = "autologin"
                                LoginPanel = LoginPanel & "" _
                                & cr & "<div class=""ccAdminSmall"">" _
                                & cr2 & "<LABEL for=""" & TagID & """>" & cpCore.html.html_GetFormInputCheckBox2(TagID, True, TagID) & "&nbsp;Login automatically from this computer</LABEL>" _
                                & cr & "</div>"
                            End If
                        End If
                        '
                        ' Buttons
                        '
                        LoginPanel = LoginPanel & Adminui.GetButtonBar(Adminui.GetButtonsFromList(ButtonLogin & "," & ButtonLogout, True, True, "mb"), "")
                        '
                        ' ----- assemble tools panel
                        '
                        Copy = "" _
                        & cr & "<td width=""50%"" class=""ccPanelInput"" style=""vertical-align:bottom;"">" _
                        & genericController.htmlIndent(LoginPanel) _
                        & cr & "</td>" _
                        & cr & "<td width=""50%"" class=""ccPanelInput"" style=""vertical-align:bottom;"">" _
                        & genericController.htmlIndent(OptionsPanel) _
                        & cr & "</td>"
                        Copy = "" _
                        & cr & "<tr>" _
                        & genericController.htmlIndent(Copy) _
                        & cr & "</tr>" _
                        & ""
                        Copy = "" _
                        & cr & "<table border=""0"" cellpadding=""3"" cellspacing=""0"" width=""100%"">" _
                        & genericController.htmlIndent(Copy) _
                        & cr & "</table>"
                        ToolsPanel.Add(main_GetPanelInput(Copy))
                        ToolsPanel.Add(cpCore.html.html_GetFormEnd)
                        result = result & main_GetPanel(ToolsPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5)
                        '
                        result = result & main_GetPanel(LinkPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5)
                        '
                        LinkPanel = Nothing
                        ToolsPanel = Nothing
                        AnotherPanel = Nothing
                    End If
                    '
                    ' --- Developer Debug Panel
                    '
                    If cpCore.visitProperty.getBoolean("AllowDebugging") Then
                        '
                        ' --- Debug Panel Header
                        '
                        LinkPanel = New stringBuilderLegacyController
                        LinkPanel.Add(SpanClassAdminSmall)
                        'LinkPanel.Add( "WebClient " & main_WebClientVersion & " | "
                        LinkPanel.Add("Contensive " & cpCore.codeVersion() & " | ")
                        LinkPanel.Add(FormatDateTime(cpCore.profileStartTime) & " | ")
                        LinkPanel.Add("<a class=""ccAdminLink"" target=""_blank"" href=""http: //support.Contensive.com/"">Support</A> | ")
                        LinkPanel.Add("<a class=""ccAdminLink"" href=""" & genericController.encodeHTML(cpCore.siteProperties.adminURL) & """>Admin Home</A> | ")
                        LinkPanel.Add("<a class=""ccAdminLink"" href=""" & genericController.encodeHTML("http://" & cpCore.webServer.requestDomain) & """>Public Home</A> | ")
                        LinkPanel.Add("<a class=""ccAdminLink"" target=""_blank"" href=""" & genericController.encodeHTML(cpCore.siteProperties.adminURL & "?" & RequestNameHardCodedPage & "=" & HardCodedPageMyProfile) & """>My Profile</A> | ")
                        LinkPanel.Add("</span>")
                        '
                        '
                        '
                        'DebugPanel = DebugPanel & main_GetPanel(LinkPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", "5")
                        '
                        DebugPanel = DebugPanel _
                        & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">" _
                        & cr2 & "<tr>" _
                        & cr3 & "<td width=""100"" class=""ccPanel""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100"" height=""1"" ></td>" _
                        & cr3 & "<td width=""100%"" class=""ccPanel""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td>" _
                        & cr2 & "</tr>"
                        '
                        DebugPanel = DebugPanel & main_DebugPanelRow("DOM", "<a class=""ccAdminLink"" href=""/ccLib/clientside/DOMViewer.htm"" target=""_blank"">Click</A>")
                        DebugPanel = DebugPanel & main_DebugPanelRow("Trap Errors", genericController.encodeHTML(cpCore.siteProperties.trapErrors.ToString))
                        DebugPanel = DebugPanel & main_DebugPanelRow("Trap Email", genericController.encodeHTML(cpCore.siteProperties.getText("TrapEmail")))
                        DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerLink", genericController.encodeHTML(cpCore.webServer.requestUrl))
                        DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerDomain", genericController.encodeHTML(cpCore.webServer.requestDomain))
                        DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerProtocol", genericController.encodeHTML(cpCore.webServer.requestProtocol))
                        DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerHost", genericController.encodeHTML(cpCore.webServer.requestDomain))
                        DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerPath", genericController.encodeHTML(cpCore.webServer.requestPath))
                        DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerPage", genericController.encodeHTML(cpCore.webServer.requestPage))
                        Copy = ""
                        If cpCore.webServer.requestQueryString <> "" Then
                            CopySplit = Split(cpCore.webServer.requestQueryString, "&")
                            For Ptr = 0 To UBound(CopySplit)
                                copyNameValue = CopySplit(Ptr)
                                If copyNameValue <> "" Then
                                    copyNameValueSplit = Split(copyNameValue, "=")
                                    CopyName = genericController.DecodeResponseVariable(copyNameValueSplit(0))
                                    copyValue = ""
                                    If UBound(copyNameValueSplit) > 0 Then
                                        copyValue = genericController.DecodeResponseVariable(copyNameValueSplit(1))
                                    End If
                                    Copy = Copy & cr & "<br>" & genericController.encodeHTML(CopyName & "=" & copyValue)
                                End If
                            Next
                            Copy = Mid(Copy, 8)
                        End If
                        DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerQueryString", Copy)
                        Copy = ""
                        For Each key As String In cpCore.docProperties.getKeyList()
                            Dim docProperty As docPropertiesClass = cpCore.docProperties.getProperty(key)
                            If docProperty.IsForm Then
                                Copy = Copy & cr & "<br>" & genericController.encodeHTML(docProperty.NameValue)
                            End If
                        Next
                        DebugPanel = DebugPanel & main_DebugPanelRow("Render Time &gt;= ", Format((GetTickCount - cpCore.profileStartTickCount) / 1000, "0.000") & " sec")
                        If True Then
                            VisitHrs = CInt(cpCore.authContext.visit.TimeToLastHit / 3600)
                            VisitMin = CInt(cpCore.authContext.visit.TimeToLastHit / 60) - (60 * VisitHrs)
                            VisitSec = cpCore.authContext.visit.TimeToLastHit Mod 60
                            DebugPanel = DebugPanel & main_DebugPanelRow("Visit Length", CStr(cpCore.authContext.visit.TimeToLastHit) & " sec, (" & VisitHrs & " hrs " & VisitMin & " mins " & VisitSec & " secs)")
                            'DebugPanel = DebugPanel & main_DebugPanelRow("Visit Length", CStr(main_VisitTimeToLastHit) & " sec, (" & Int(main_VisitTimeToLastHit / 60) & " min " & (main_VisitTimeToLastHit Mod 60) & " sec)")
                        End If
                        DebugPanel = DebugPanel & main_DebugPanelRow("Addon Profile", "<hr><ul class=""ccPanel"">" & "<li>tbd</li>" & cr & "</ul>")
                        '
                        DebugPanel = DebugPanel & "</table>"
                        '
                        If ShowLegacyToolsPanel Then
                            '
                            ' Debug Panel as part of legacy tools panel
                            '
                            result = result _
                            & main_GetPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5)
                        Else
                            '
                            ' Debug Panel without Legacy Tools panel
                            '
                            result = result _
                            & main_GetPanelHeader("Debug Panel") _
                            & main_GetPanel(LinkPanel.Text) _
                            & main_GetPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5)
                        End If
                    End If
                    result = cr & "<div class=""ccCon"">" & genericController.htmlIndent(result) & cr & "</div>"
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '
        '
        Private Function main_DebugPanelRow(Label As String, Value As String) As String
            Return cr2 & "<tr><td valign=""top"" class=""ccPanel ccAdminSmall"">" & Label & "</td><td valign=""top"" class=""ccPanel ccAdminSmall"">" & Value & "</td></tr>"
        End Function

        '
        '=================================================================================================================
        '   csv_GetAddonOptionStringValue
        '
        '   gets the value from a list matching the name
        '
        '   InstanceOptionstring is an "AddonEncoded" name=AddonEncodedValue[selector]descriptor&name=value string
        '=================================================================================================================
        '
        Public Shared Function csv_GetAddonOptionStringValue(OptionName As String, addonOptionString As String) As String
            Dim Pos As Integer
            Dim s As String
            '
            s = genericController.getSimpleNameValue(OptionName, addonOptionString, "", "&")
            Pos = genericController.vbInstr(1, s, "[")
            If Pos > 0 Then
                s = Left(s, Pos - 1)
            End If
            s = genericController.decodeNvaArgument(s)
            '
            csv_GetAddonOptionStringValue = Trim(s)
        End Function
        '
        '====================================================================================================
        Public Function getHtmlDoc(htmlBody As String, htmlBodyTag As String, Optional allowLogin As Boolean = True, Optional allowTools As Boolean = True, Optional blockNonContentExtras As Boolean = False, Optional isAdminSite As Boolean = True) As String
            Dim result As String = ""
            Try
                Dim htmlHead As String = cpCore.doc.getHtmlHead(isAdminSite)
                Dim htmlBeforeEndOfBody As String = getHtmlDoc_beforeEndOfBodyHtml(allowLogin, allowTools, blockNonContentExtras, isAdminSite)

                result = "" _
                    & cpCore.siteProperties.docTypeDeclarationAdmin _
                    & vbCrLf & "<html>" _
                    & vbCrLf & "<head>" _
                    & htmlHead _
                    & vbCrLf & "</head>" _
                    & vbCrLf & htmlBodyTag _
                    & htmlBody _
                    & htmlBeforeEndOfBody _
                    & vbCrLf & "</body>" _
                    & vbCrLf & "</html>" _
                    & ""
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        ''
        '' assemble all the html parts
        ''
        'Public Function assembleHtmlDoc(ByVal head As String, ByVal bodyTag As String, ByVal Body As String) As String
        '    Return "" _
        '        & cpCore.siteProperties.docTypeDeclarationAdmin _
        '        & cr & "<html>" _
        '        & cr2 & "<head>" _
        '        & genericController.htmlIndent(head) _
        '        & cr2 & "</head>" _
        '        & cr2 & bodyTag _
        '        & genericController.htmlIndent(Body) _
        '        & cr2 & "</body>" _
        '        & cr & "</html>"
        'End Function
        ''
        ''========================================================================
        '' ----- Starts an HTML page (for an admin page -- not a public page)
        ''========================================================================
        ''
        'Public Function getHtmlDoc_beforeBodyHtml(Optional ByVal Title As String = "", Optional ByVal PageMargin As Integer = 0) As String
        '    If Title <> "" Then
        '        Call main_AddPagetitle(Title)
        '    End If
        '    If main_MetaContent_Title = "" Then
        '        Call main_AddPagetitle("Admin-" & cpCore.webServer.webServerIO_requestDomain)
        '    End If
        '    cpCore.webServer.webServerIO_response_NoFollow = True
        '    Call main_SetMetaContent(0, 0)
        '    '
        '    Return "" _
        '        & cpCore.siteProperties.docTypeDeclarationAdmin _
        '        & vbCrLf & "<html>" _
        '        & vbCrLf & "<head>" _
        '        & getHTMLInternalHead(True) _
        '        & vbCrLf & "</head>" _
        '        & vbCrLf & "<body class=""ccBodyAdmin ccCon"">"
        'End Function

        '
        '====================================================================================================
        ''' <summary>
        ''' legacy compatibility
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="ButtonList"></param>
        ''' <returns></returns>
        Public Shared Function CloseFormTable(cpCore As coreClass, ByVal ButtonList As String) As String
            If ButtonList <> "" Then
                CloseFormTable = "</td></tr></TABLE>" & cpCore.html.main_GetPanelButtons(ButtonList, "Button") & "</form>"
            Else
                CloseFormTable = "</td></tr></TABLE></form>"
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' legacy compatibility
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="ButtonList"></param>
        ''' <returns></returns>
        Public Shared Function OpenFormTable(cpCore As coreClass, ByVal ButtonList As String) As String
            Dim result As String = ""
            Try
                result = cpCore.html.html_GetFormStart()
                If ButtonList <> "" Then
                    result = result & cpCore.html.main_GetPanelButtons(ButtonList, "Button")
                End If
                result = result & "<table border=""0"" cellpadding=""10"" cellspacing=""0"" width=""100%""><tr><TD>"
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
