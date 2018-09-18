

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
                If String.IsNullOrEmpty(Key) Then
                    returnValue = textToInsert
                Else
                    returnValue = layout
                    Dim posStart As Integer = getTagStartPos2(ignore, layout, 1, Key)
                    If posStart <> 0 Then
                        Dim posEnd As Integer = getTagEndPos(ignore, layout, posStart)
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
        Public Function getHtmlDoc_beforeEndOfBodyHtml(AllowLogin As Boolean, AllowTools As Boolean) As String
            Dim result As New List(Of String)
            Try
                Dim bodyScript As New List(Of String)
                '
                ' -- content extras like tool panel
                If cpCore.doc.authContext.isAuthenticatedContentManager(cpCore) And cpCore.doc.authContext.user.AllowToolsPanel Then
                    If AllowTools Then
                        result.Add(cpCore.html.main_GetToolsPanel())
                    End If
                Else
                    If AllowLogin Then
                        result.Add(main_GetLoginLink())
                    End If
                End If
                '
                ' -- Include any other close page
                If cpCore.doc.htmlForEndOfBody <> "" Then
                    result.Add(cpCore.doc.htmlForEndOfBody)
                End If
                If cpCore.doc.testPointMessage <> "" Then
                    result.Add("<div class=""ccTestPointMessageCon"">" & cpCore.doc.testPointMessage & "</div>")
                End If
                '
                ' TODO -- closing the menu attaches the flyout panels -- should be done when the menu is returned, not at page end
                ' -- output the menu system
                If Not (cpCore.menuFlyout Is Nothing) Then
                    result.Add(cpCore.menuFlyout.menu_GetClose())
                End If
                '
                ' -- Add onload javascript
                For Each asset As htmlAssetClass In cpCore.doc.htmlAssetList.FindAll(Function(a) (a.assetType = htmlAssetTypeEnum.OnLoadScript) And (Not String.IsNullOrEmpty(a.content)))
                    result.Add("<script Language=""JavaScript"" type=""text/javascript"">window.addEventListener('load', function(){" & asset.content & "});</script>")
                Next
                '
                ' -- body Javascript
                Dim allowDebugging As Boolean = cpCore.visitProperty.getBoolean("AllowDebugging")
                For Each jsBody In cpCore.doc.htmlAssetList.FindAll(Function(a) (a.assetType = htmlAssetTypeEnum.script) And (Not a.inHead) And (Not String.IsNullOrEmpty(a.content)))
                    If (jsBody.addedByMessage <> "") And allowDebugging Then
                        result.Add("<!-- from " & jsBody.addedByMessage & " -->")
                    End If
                    If Not jsBody.isLink Then
                        result.Add("<script Language=""JavaScript"" type=""text/javascript"">" & jsBody.content & "</script>")
                    Else
                        result.Add("<script type=""text/javascript"" src=""" & jsBody.content & """></script>")
                    End If
                Next
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return String.Join(cr, result)
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
                Dim CDef As Models.Complex.cdefModel
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
                    CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName)
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
                    SQL = "select count(*) as cnt from " & TableName & " where " & ContentControlCriteria
                    If LcaseCriteria <> "" Then
                        SQL &= " and " & LcaseCriteria
                    End If
                    Dim dt As DataTable
                    dt = cpCore.db.executeQuery(SQL)
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
                            If cpCore.db.csOk(CSPointer) Then
                                result = cpCore.db.csGetText(CSPointer, "name") & "&nbsp;"
                            End If
                            Call cpCore.db.csClose(CSPointer)
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
                            CSPointer = cpCore.db.csOpen(ContentName, Criteria, SortFieldList, , , , , SelectFields)
                            If cpCore.db.csOk(CSPointer) Then
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
                                    Call cpCore.db.csClose(CSPointer)
                                    If Criteria <> "" Then
                                        Criteria = Criteria & "and"
                                    End If
                                    Criteria = Criteria & "(id=" & genericController.EncodeInteger(CurrentValue) & ")"
                                    CSPointer = cpCore.db.csOpen(ContentName, Criteria, SortFieldList, False, , , , SelectFields)
                                    If cpCore.db.csOk(CSPointer) Then
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
                            Call cpCore.db.csClose(CSPointer)
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
                    PeopleTableName = Models.Complex.cdefModel.getContentTablename(cpCore, "people")
                    PeopleDataSource = Models.Complex.cdefModel.getContentDataSource(cpCore, "People")
                    MemberRulesTableName = Models.Complex.cdefModel.getContentTablename(cpCore, "Member Rules")
                    '
                    RowMax = 0
                    SQL = "select count(*) as cnt" _
                    & " from ccMemberRules R" _
                    & " inner join ccMembers P on R.MemberID=P.ID" _
                    & " where (P.active<>0)" _
                    & " and (R.GroupID=" & GroupID & ")"
                    CSPointer = cpCore.db.csOpenSql_rev(PeopleDataSource, SQL)
                    If cpCore.db.csOk(CSPointer) Then
                        RowMax = RowMax + cpCore.db.csGetInteger(CSPointer, "cnt")
                    End If
                    Call cpCore.db.csClose(CSPointer)
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
                            If cpCore.db.csOk(CSPointer) Then
                                result = cpCore.db.csGetText(CSPointer, "name") & "&nbsp;"
                            End If
                            Call cpCore.db.csClose(CSPointer)
                        End If
                        result = result & "(Selection is too large to display)"
                    Else
                        '
                        ' ----- Generate Drop Down Field Names
                        '
                        DropDownFieldList = Models.Complex.cdefModel.GetContentProperty(cpCore, "people", "DropDownFieldList")
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
                            CSPointer = cpCore.db.csOpenSql_rev(PeopleDataSource, SQL)
                            If cpCore.db.csOk(CSPointer) Then
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
                            Call cpCore.db.csClose(CSPointer)
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

    End Class
End Namespace
