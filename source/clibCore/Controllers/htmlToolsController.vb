﻿
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
    Public Class htmlToolsController
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


    End Class
End Namespace