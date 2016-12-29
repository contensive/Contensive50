
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
    Public Class coreHtmlToolsClass
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
        '
        '==============================================================================
        Public Function isGuid(ignore As Object, Source As String) As Boolean
            Dim returnValue As Boolean = False
            Try
                If (Len(Source) = 38) And (Left(Source, 1) = "{") And (Right(Source, 1) = "}") Then
                    '
                    ' Good to go
                    '
                    returnValue = True
                ElseIf (Len(Source) = 36) And (InStr(1, Source, " ") = 0) Then
                    '
                    ' might be valid with the brackets, add them
                    '
                    returnValue = True
                    'source = "{" & source & "}"
                ElseIf (Len(Source) = 32) Then
                    '
                    ' might be valid with the brackets and the dashes, add them
                    '
                    returnValue = True
                    'source = "{" & Mid(source, 1, 8) & "-" & Mid(source, 9, 4) & "-" & Mid(source, 13, 4) & "-" & Mid(source, 17, 4) & "-" & Mid(source, 21) & "}"
                Else
                    '
                    ' not valid
                    '
                    returnValue = False
                    '        source = ""
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnValue
        End Function
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
            Dim returnValue As String
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
                            posStart = InStr(posStart + 1, layout, ">")
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
                            posStart = InStr(posStart + 1, layout, ">")
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
                Dim idMatch As Boolean
                Dim classMatch As Boolean
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
                Pos = InStr(posStartTag, layout, ">")
                If Pos > 0 Then
                    returnValue = True
                    Tag = Mid(layout, posStartTag, Pos - posStartTag + 1)
                    tagLower = LCase(Tag)
                    tagLength = Len(Tag)
                    '
                    ' check searchId
                    '
                    If returnValue And (searchId <> "") Then
                        Pos = InStr(1, tagLower, " id=", vbTextCompare)
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
                        Pos = InStr(1, tagLower, " class=", vbTextCompare)
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
                '
                'Dim posMatch as integer
                Dim SegmentStart As Integer
                Dim Pos As Integer
                'Dim searchIsOver As Boolean
                Dim LoopPtr As Integer
                Dim searchKey As String
                Dim lenSearchKey As Integer
                'Dim s As String
                Dim c As String
                Dim Ptr As Integer
                Dim workingKey As String
                Dim workingKeys() As String
                Dim posEnd As Integer
                Dim searchClass As String
                Dim searchId As String
                Dim searchTag As String
                Dim posStartTag As Integer
                '
                's = layout
                returnPos = 0
                workingKey = Key
                If InStr(1, workingKey, ">") <> 0 Then
                    '
                    ' does not support > yet.
                    '
                    workingKey = Replace(workingKey, ">", " ")
                End If
                '
                ' eliminate whitespace
                '
                Do While InStr(1, workingKey, vbTab) <> 0
                    workingKey = Replace(workingKey, vbTab, " ")
                Loop
                '
                Do While InStr(1, workingKey, vbCr) <> 0
                    workingKey = Replace(workingKey, vbCr, " ")
                Loop
                '
                Do While InStr(1, workingKey, vbLf) <> 0
                    workingKey = Replace(workingKey, vbLf, " ")
                Loop
                '
                Do While InStr(1, workingKey, "  ") <> 0
                    workingKey = Replace(workingKey, "  ", " ")
                Loop
                '
                workingKey = Trim(workingKey)
                '
                If InStr(1, workingKey, " ") <> 0 Then
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
                        Pos = InStr(1, searchClass, "#")
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
                        Pos = InStr(1, searchId, ".")
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
                        Pos = InStr(1, searchTag, "#")
                        If Pos <> 0 Then
                            searchId = Mid(searchTag, Pos + 1)
                            searchTag = Mid(searchTag, 1, Pos - 1)
                            Pos = InStr(1, searchId, ".")
                            If Pos <> 0 Then
                                searchClass = Mid(searchId, Pos)
                                searchId = Mid(searchId, 1, Pos - 1)
                            End If
                        End If
                        Pos = InStr(1, searchTag, ".")
                        If Pos <> 0 Then
                            searchClass = Mid(searchTag, Pos + 1)
                            searchTag = Mid(searchTag, 1, Pos - 1)
                            Pos = InStr(1, searchClass, "#")
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
                    'posMatch = InStr(layoutStartPos, layout, searchKey)
                    'pos = posMatch
                    'searchIsOver = False
                    Do
                        Pos = InStr(Pos, layout, searchKey)
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
                        Call Err.Raise(KmaErrorInternal, "aoPrimitives.htmlToolsClass.getTagStartPos2", "Tag limit of 1000 tags per block reached.")
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
                Pos = InStr(startPos, Source, "<")
                TagName = ""
                returnValue = 0
                If Pos <> 0 Then
                    Pos = Pos + 1
                    Do While Pos < Len(Source)
                        c = LCase(Mid(Source, Pos, 1))
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
                            posEnd = InStr(Pos + 1, Source, endTag, vbTextCompare)
                            If posEnd = 0 Then
                                '
                                ' no end was found, return the tag or rest of the string
                                '
                                returnValue = InStr(Pos + 1, Source, ">") + 1
                                If posEnd = 1 Then
                                    returnValue = Len(Source)
                                End If
                                Exit Do
                            Else
                                posNest = InStr(Pos + 1, Source, startTag, vbTextCompare)
                                If posNest = 0 Then
                                    '
                                    ' no nest found, set to end
                                    '
                                    posNest = Len(Source)
                                End If
                                posComment = InStr(Pos + 1, Source, "<!--")
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
                                    Pos = InStr(posComment, Source, "-->")
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
                                    Pos = InStr(posEnd, Source, ">")
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

    End Class
End Namespace
