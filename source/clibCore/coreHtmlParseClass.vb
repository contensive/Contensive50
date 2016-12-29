
Option Explicit On
Option Strict On

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
'
Namespace Contensive.Core
    Public Class coreHtmlParseClass

        '
        '============================================================================
        '
        ' open source html parser to try
        ' ************* NUGET html parser
        '
        '
        '
        '
        ' Parse HTML
        '
        '   This class parses an HTML document into Nodes. A node may be text, and it
        '   may be a tag. Use the IsTag method to detect
        '
        '   It makes no attempt to create a document structer. The advantage is that
        '   it can parse through, and make available poor HTML structures
        '
        '   If Element.IsTag
        '       Element.text = the tag, including <>
        '   otherwise
        '       Element.text = the string
        '============================================================================
        '
        '   Internal Storage
        '
        Private cpCore As cpCoreClass
        '
        Const NewWay = True
        '
        Private LocalElements() As Element
        Private LocalElementSize As Integer
        Private LocalElementCount As Integer
        Private SplitStore() As String
        Private SplitStoreCnt As Integer
        Private Blobs() As String
        Private BlobCnt As Integer
        Private BlobSN As String
        '
        '   Internal HTML Element Attribute structure
        '
        Private Structure ElementAttributeStructure
            Dim Name As String
            Dim UcaseName As String
            Dim Value As String
        End Structure
        '
        '   Internal HTML Element (tag) structure
        '
        Private Structure Element
            Dim IsTag As Boolean
            Dim TagName As String
            Dim Text As String
            Dim Position As Integer
            Dim AttributeCount As Integer
            Dim AttributeSize As Integer
            Dim Attributes() As ElementAttributeStructure
            Dim Loaded As Boolean
        End Structure
        '
        '====================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As cpCoreClass)
            Me.cpCore = cpCore
        End Sub
        '
        '========================================================================
        '   Parses the string
        '   returns true if loaded OK
        '========================================================================
        '
        Public Function Load(ByVal HTMLSource As String) As Boolean
            On Error GoTo ErrorTrap
            '
            Const Chunk = 1000
            '
            Dim WorkingSrc As String
            Dim TagStart As Integer
            Dim TagEnd As Integer
            Dim TagLength As Integer
            Dim TagStartString As String
            Dim splittest() As String
            Dim Ptr As Integer
            Dim Cnt As Integer
            Dim Pos As Integer
            Dim testing As Boolean
            Dim PosScriptEnd As Integer
            Dim PosEndScript As Integer
            Dim PosEndScriptEnd As Integer
            '
            ' ----- initialize internal storage
            '
            WorkingSrc = HTMLSource
            LocalElementCount = 0
            LocalElementSize = 0
            ReDim LocalElements(LocalElementSize)
            '
            If NewWay Then
                '
                '--------------------------------------------------------------------------------
                ' New Way
                '--------------------------------------------------------------------------------
                '
                Load = True
                Ptr = 0
                '
                ' get a unique signature
                '
                Do
                    BlobSN = "/blob" & CStr(GetRandomInteger()) & ":"
                    Ptr = Ptr + 1
                Loop While ((InStr(1, WorkingSrc, BlobSN, vbTextCompare) <> 0) And (Ptr < 10))
                '
                ' remove all scripting
                '
                splittest = Split(WorkingSrc, "<script")
                Cnt = UBound(splittest) + 1
                If Cnt > 1 Then
                    For Ptr = 1 To Cnt - 1
                        PosScriptEnd = InStr(1, splittest(Ptr), ">")
                        If PosScriptEnd > 0 Then
                            PosEndScript = InStr(PosScriptEnd, splittest(Ptr), "</script", vbTextCompare)
                            If PosEndScript > 0 Then
                                ReDim Preserve Blobs(BlobCnt)
                                Blobs(BlobCnt) = Mid(splittest(Ptr), PosScriptEnd + 1, (PosEndScript - 1) - (PosScriptEnd + 1) + 1)
                                splittest(Ptr) = Mid(splittest(Ptr), 1, PosScriptEnd) & BlobSN & BlobCnt & "/" & Mid(splittest(Ptr), PosEndScript)
                                BlobCnt = BlobCnt + 1
                            End If
                        End If
                    Next
                    WorkingSrc = Join(splittest, "<script")
                End If
                '
                ' remove all styles
                '
                splittest = Split(WorkingSrc, "<style")
                Cnt = UBound(splittest) + 1
                If Cnt > 1 Then
                    For Ptr = 1 To Cnt - 1
                        PosScriptEnd = InStr(1, splittest(Ptr), ">")
                        If PosScriptEnd > 0 Then
                            PosEndScript = InStr(PosScriptEnd, splittest(Ptr), "</style", vbTextCompare)
                            If PosEndScript > 0 Then
                                ReDim Preserve Blobs(BlobCnt)
                                Blobs(BlobCnt) = Mid(splittest(Ptr), PosScriptEnd + 1, (PosEndScript - 1) - (PosScriptEnd + 1) + 1)
                                splittest(Ptr) = Mid(splittest(Ptr), 1, PosScriptEnd) & BlobSN & BlobCnt & "/" & Mid(splittest(Ptr), PosEndScript)
                                BlobCnt = BlobCnt + 1
                            End If
                        End If
                    Next
                    WorkingSrc = Join(splittest, "<style")
                End If
                '
                ' remove comments
                '
                splittest = Split(WorkingSrc, "<!--")
                Cnt = UBound(splittest) + 1
                If Cnt > 1 Then
                    For Ptr = 1 To Cnt - 1
                        PosScriptEnd = InStr(1, splittest(Ptr), "-->")
                        If PosScriptEnd > 0 Then
                            ReDim Preserve Blobs(BlobCnt)
                            Blobs(BlobCnt) = Mid(splittest(Ptr), 1, PosScriptEnd - 1)
                            splittest(Ptr) = BlobSN & BlobCnt & "/" & Mid(splittest(Ptr), PosScriptEnd)
                            BlobCnt = BlobCnt + 1
                        End If
                    Next
                    WorkingSrc = Join(splittest, "<!--")
                End If
                '
                ' Split the html on <
                '
                SplitStore = Split(WorkingSrc, "<")
                SplitStoreCnt = UBound(SplitStore) + 1
                LocalElementCount = (SplitStoreCnt * 2)
                ReDim LocalElements(LocalElementCount)
                '
            Else
                '
                '--------------------------------------------------------------------------------
                ' Old way
                '--------------------------------------------------------------------------------
                '
                Load = True
                If Not IsNull(WorkingSrc) Then
                    TagEnd = 0
                    TagStartString = "<"
                    TagStart = InStr(1, WorkingSrc, TagStartString)
                    Do While TagStart <> 0
                        If (LocalElementCount / 1000) = Int(LocalElementCount / 1000) Then
                            LocalElementCount = LocalElementCount
                        End If
                        TagStartString = "<"
                        '
                        ' ----- create a non-tag element if last end is not this start
                        '
                        If TagStart > (TagEnd + 1) Then
                            If LocalElementCount >= LocalElementSize Then
                                LocalElementSize = LocalElementSize + Chunk
                                ReDim Preserve LocalElements(LocalElementSize)
                            End If
                            LocalElements(LocalElementCount).IsTag = False
                            LocalElements(LocalElementCount).Text = Mid(WorkingSrc, TagEnd + 1, TagStart - 1 - TagEnd)
                            LocalElements(LocalElementCount).Position = TagEnd + 1
                            LocalElements(LocalElementCount).Loaded = True
                            LocalElementCount = LocalElementCount + 1
                        End If
                        '
                        ' ----- create a tag element
                        '
                        If LocalElementCount >= LocalElementSize Then
                            LocalElementSize = LocalElementSize + Chunk
                            ReDim Preserve LocalElements(LocalElementSize)
                        End If
                        LocalElements(LocalElementCount).Position = TagStart
                        LocalElements(LocalElementCount).IsTag = True
                        '
                        If Mid(WorkingSrc, TagStart, 4) = "<!--" Then
                            '
                            ' Comment Tag
                            '
                            TagEnd = InStr(TagStart, WorkingSrc, "-->")
                            If TagEnd = 0 Then
                                LocalElements(LocalElementCount).Text = Mid(WorkingSrc, TagStart)
                            Else
                                TagEnd = TagEnd + 2
                                LocalElements(LocalElementCount).Text = Mid(WorkingSrc, TagStart, TagEnd - TagStart + 1)
                            End If
                            LocalElements(LocalElementCount).TagName = "!--"
                            LocalElements(LocalElementCount).Loaded = True
                            TagStartString = "<"
                        ElseIf LCase(Mid(WorkingSrc, TagStart, 7)) = "<script" Then
                            '
                            ' Script tag - include everything up to the </script> in the next non-tag
                            '
                            TagEnd = InStr(TagStart, WorkingSrc, ">")
                            If TagEnd = 0 Then
                                LocalElements(LocalElementCount).Text = Mid(WorkingSrc, TagStart)
                            Else
                                LocalElements(LocalElementCount).Text = Mid(WorkingSrc, TagStart, TagEnd - TagStart + 1)
                            End If
                            Call ParseTag(LocalElementCount)
                            LocalElements(LocalElementCount).Loaded = True
                            TagStartString = "</script"
                        Else
                            '
                            ' All other tags
                            '
                            TagEnd = InStr(TagStart, WorkingSrc, ">")
                            If TagEnd = 0 Then
                                LocalElements(LocalElementCount).Text = Mid(WorkingSrc, TagStart)
                            Else
                                LocalElements(LocalElementCount).Text = Mid(WorkingSrc, TagStart, TagEnd - TagStart + 1)
                            End If
                            Call ParseTag(LocalElementCount)
                            LocalElements(LocalElementCount).Loaded = True
                            TagStartString = "<"
                        End If

                        LocalElementCount = LocalElementCount + 1
                        If TagEnd = 0 Then
                            TagStart = 0
                        Else
                            TagStart = InStr(TagEnd, WorkingSrc, TagStartString, vbTextCompare)
                        End If
                        Do While TagStart <> 0 And (Mid(WorkingSrc, TagStart + 1, 1) = " ")
                            TagStart = InStr(TagStart + 1, WorkingSrc, TagStartString, vbTextCompare)
                        Loop
                    Loop
                    '
                    ' ----- if there is anything left in the WorkingSrc, make an element out of it
                    '
                    If TagEnd < Len(WorkingSrc) Then
                        If LocalElementCount >= LocalElementSize Then
                            LocalElementSize = LocalElementSize + Chunk
                            ReDim Preserve LocalElements(LocalElementSize)
                        End If
                        LocalElements(LocalElementCount).IsTag = False
                        LocalElements(LocalElementCount).Text = Mid(WorkingSrc, TagEnd + 1)
                        LocalElementCount = LocalElementCount + 1
                    End If
                End If
            End If
            '
            Exit Function
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("unexpected exception"))
        End Function
        '
        '========================================================================
        '   Get the element count
        '========================================================================
        '
        Public ReadOnly Property ElementCount As Integer
            Get
                ElementCount = LocalElementCount
            End Get
        End Property
        '
        '========================================================================
        '   is the specified element a tag (or text)
        '========================================================================
        '
        Public Function IsTag(ByVal ElementPointer As Integer) As Boolean
            On Error GoTo ErrorTrap
            '
            Dim Copy As String
            '
            IsTag = False
            Call LoadElement(ElementPointer)
            'If Not LocalElements(ElementPointer).Loaded Then
            '    Call LoadElement(ElementPointer)
            'End If
            If ElementPointer < LocalElementCount Then
                IsTag = LocalElements(ElementPointer).IsTag
            End If
            Exit Function
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("unexpected exception"))
        End Function
        '
        '========================================================================
        '   Get the LocalElements value
        '========================================================================
        '
        Public Function Text(ByVal ElementPointer As Integer) As String
            On Error GoTo ErrorTrap
            '
            Text = ""
            Call LoadElement(ElementPointer)
            If ElementPointer < LocalElementCount Then
                Text = LocalElements(ElementPointer).Text
            End If
            '
            Exit Function
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("unexpected exception"))
        End Function
        '
        '========================================================================
        '   Get the LocalElements value
        '========================================================================
        '
        Public Function TagName(ByVal ElementPointer As Integer) As String
            On Error GoTo ErrorTrap
            '
            TagName = ""
            Call LoadElement(ElementPointer)
            If ElementPointer < LocalElementCount Then
                TagName = LocalElements(ElementPointer).TagName
            End If
            '
            Exit Function
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("unexpected exception"))
        End Function
        '
        '========================================================================
        '   Get the LocalElements value
        '========================================================================
        '
        Public Function Position(ByVal ElementPointer As Integer) As Integer
            On Error GoTo ErrorTrap
            '
            Position = 0
            Call LoadElement(ElementPointer)
            If ElementPointer < LocalElementCount Then
                Position = LocalElements(ElementPointer).Position
            End If
            '
            Exit Function
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("unexpected exception"))
        End Function
        '
        '========================================================================
        '   Get an LocalElements attribute count
        '========================================================================
        '
        Public Function ElementAttributeCount(ByVal ElementPointer As Integer) As Integer
            On Error GoTo ErrorTrap
            '
            ElementAttributeCount = 0
            Call LoadElement(ElementPointer)
            If ElementPointer < LocalElementCount Then
                ElementAttributeCount = LocalElements(ElementPointer).AttributeCount
            End If
            '
            Exit Function
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("unexpected exception"))
        End Function
        '
        '========================================================================
        '   Get an LocalElements attribute name
        '========================================================================
        '
        Public Function ElementAttributeName(ByVal ElementPointer As Integer, ByVal AttributePointer As Integer) As String
            On Error GoTo ErrorTrap
            '
            ElementAttributeName = ""
            Call LoadElement(ElementPointer)
            If ElementPointer < LocalElementCount Then
                If AttributePointer < LocalElements(ElementPointer).AttributeCount Then
                    ElementAttributeName = LocalElements(ElementPointer).Attributes(AttributePointer).Name
                End If
            End If
            '
            Exit Function
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("unexpected exception"))
        End Function
        '
        '========================================================================
        '   Get an LocalElements attribute value
        '========================================================================
        '
        Public Function ElementAttributeValue(ByVal ElementPointer As Integer, ByVal AttributePointer As Integer) As String
            On Error GoTo ErrorTrap
            '
            ElementAttributeValue = ""
            Call LoadElement(ElementPointer)
            If ElementPointer < LocalElementCount Then
                If AttributePointer < LocalElements(ElementPointer).AttributeCount Then
                    ElementAttributeValue = LocalElements(ElementPointer).Attributes(AttributePointer).Value
                End If
            End If
            '
            Exit Function
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("unexpected exception"))
        End Function
        '
        '========================================================================
        '   Get an LocalElements attribute value
        '========================================================================
        '
        Public Function ElementAttribute(ByVal ElementPointer As Integer, ByVal Name As String) As String
            On Error GoTo ErrorTrap
            '
            Dim AttributePointer As Integer
            Dim UcaseName As String
            '
            ElementAttribute = ""
            Call LoadElement(ElementPointer)
            If ElementPointer < LocalElementCount Then
                With LocalElements(ElementPointer)
                    If .AttributeCount > 0 Then
                        UcaseName = UCase(Name)
                        For AttributePointer = 0 To .AttributeCount - 1
                            If .Attributes(AttributePointer).UcaseName = UcaseName Then
                                ElementAttribute = .Attributes(AttributePointer).Value
                                Exit For
                            End If
                        Next
                    End If
                End With
            End If
            '
            Exit Function
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("unexpected exception"))
        End Function
        '
        '========================================================================
        '   Parse a Tag element into its attributes
        '========================================================================
        '
        Private Sub ParseTag(ByVal ElementPointer As Integer)
            'Exit Sub
            On Error GoTo ErrorTrap
            '
            Dim Copy As String
            Dim CursorPosition As Integer
            Dim SpacePosition As Integer
            'Dim ClosePosition as integer
            Dim AttributeDelimiterPosition As Integer
            Dim TagString As String
            '
            Dim QuotePosition As Integer
            Dim CloseQuotePosition As Integer
            Dim EqualPosition As Integer
            Dim TestPosition As Integer
            Dim TestValue As String
            Dim Name As String
            '
            Dim AttrSplit() As String
            Dim AttrCount As Integer
            Dim AttrPointer As Integer
            Dim AttrName As String
            Dim AttrValue As String
            Dim AttrValueLen As Integer
            '
            With LocalElements(ElementPointer)
                TagString = Mid(.Text, 2, Len(.Text) - 2)
                If Right(TagString, 1) = "/" Then
                    TagString = Mid(TagString, 1, Len(TagString) - 1)
                End If
                'TagString = Replace(TagString, ">", " ") & " "
                TagString = Replace(TagString, vbCr, " ")
                TagString = Replace(TagString, vbLf, " ")
                TagString = Replace(TagString, "  ", " ")
                'TagString = Replace(TagString, " =", "=")
                'TagString = Replace(TagString, "= ", "=")
                'TagString = Replace(TagString, "'", """")
                .AttributeCount = 0
                .AttributeSize = 1
                ReDim .Attributes(0)  ' allocates the first
                'ClosePosition = Len(TagString)
                'If ClosePosition <= 2 Then
                '    '
                '    ' ----- there is nothing in the <>, skip element
                '    '
                'Else
                '
                ' ----- Get the tag name
                '
                If TagString <> "" Then
                    AttrSplit = SplitDelimited(TagString, " ")
                    AttrCount = UBound(AttrSplit) + 1
                    If AttrCount > 0 Then
                        .TagName = AttrSplit(0)
                        If .TagName = "!--" Then
                            '
                            ' Skip comment tags, ignore the attributes
                            '
                        Else
                            '
                            ' Process the tag
                            '
                            If AttrCount > 1 Then
                                For AttrPointer = 1 To AttrCount - 1
                                    AttrName = AttrSplit(AttrPointer)
                                    If AttrName <> "" Then
                                        If .AttributeCount >= .AttributeSize Then
                                            .AttributeSize = .AttributeSize + 5
                                            ReDim Preserve .Attributes(.AttributeSize)
                                        End If
                                        EqualPosition = InStr(1, AttrName, "=")
                                        If EqualPosition = 0 Then
                                            .Attributes(.AttributeCount).Name = AttrName
                                            .Attributes(.AttributeCount).UcaseName = UCase(AttrName)
                                            .Attributes(.AttributeCount).Value = AttrName
                                        Else
                                            AttrValue = Mid(AttrName, EqualPosition + 1)
                                            AttrValueLen = Len(AttrValue)
                                            If (AttrValueLen > 1) Then
                                                If (Mid(AttrValue, 1, 1) = """") And (Mid(AttrValue, AttrValueLen, 1) = """") Then
                                                    AttrValue = Mid(AttrValue, 2, AttrValueLen - 2)
                                                End If
                                            End If
                                            AttrName = Mid(AttrName, 1, EqualPosition - 1)
                                            .Attributes(.AttributeCount).Name = AttrName
                                            .Attributes(.AttributeCount).UcaseName = UCase(AttrName)
                                            .Attributes(.AttributeCount).Value = AttrValue
                                        End If
                                        .AttributeCount = .AttributeCount + 1
                                    End If
                                Next
                            End If
                        End If
                    End If
                End If
                '
                '            CursorPosition = 1
                '            SpacePosition = GetLesserNonZero(InStr(CursorPosition, TagString, " "), ClosePosition)
                '            .TagName = Mid(TagString, 2, SpacePosition - 2)
                '            CursorPosition = SpacePosition + 1
                '            Do While (CursorPosition < ClosePosition) And (CursorPosition <> 0)
                '                SpacePosition = GetLesserNonZero(InStr(CursorPosition, TagString, " "), ClosePosition + 1)
                '                QuotePosition = GetLesserNonZero(InStr(CursorPosition, TagString, """"), ClosePosition + 1)
                '                EqualPosition = GetLesserNonZero(InStr(CursorPosition, TagString, "="), ClosePosition + 1)
                '                '
                '                If .AttributeCount >= .AttributeSize Then
                '                    .AttributeSize = .AttributeSize + 1
                '                    ReDim Preserve .Attributes(.AttributeSize)
                '                    End If
                '                If SpacePosition < EqualPosition Then
                '                    '
                '                    ' ----- Case 1, attribute without a value
                '                    '
                '                    Name = Mid(TagString, CursorPosition, SpacePosition - CursorPosition)
                '                    .Attributes(.AttributeCount).Name = Name
                '                    .Attributes(.AttributeCount).UcaseName = UCase(Name)
                '                    .Attributes(.AttributeCount).Value = Name
                '                    CursorPosition = SpacePosition
                '                ElseIf QuotePosition < SpacePosition Then
                '                    '
                '                    ' ----- Case 2, quoted value
                '                    '
                '                    CloseQuotePosition = GetLesserNonZero(InStr(QuotePosition + 1, TagString, """"), ClosePosition)
                '                    Name = Mid(TagString, CursorPosition, EqualPosition - CursorPosition)
                '                    .Attributes(.AttributeCount).Name = Name
                '                    .Attributes(.AttributeCount).UcaseName = UCase(Name)
                '                    .Attributes(.AttributeCount).Value = Mid(TagString, QuotePosition + 1, CloseQuotePosition - QuotePosition - 1)
                '                    CursorPosition = CloseQuotePosition
                '                Else
                '                    '
                '                    ' ----- Case 2, unquoted value
                '                    '
                '                    Name = Mid(TagString, CursorPosition, EqualPosition - CursorPosition)
                '                    .Attributes(.AttributeCount).Name = Name
                '                    .Attributes(.AttributeCount).UcaseName = UCase(Name)
                '                    .Attributes(.AttributeCount).Value = Mid(TagString, EqualPosition + 1, SpacePosition - EqualPosition - 1)
                '                    CursorPosition = SpacePosition
                '                    End If
                '                If CursorPosition <> 0 Then
                '                    CursorPosition = PassWhiteSpace(CursorPosition + 1, TagString)
                '                    End If
                '                .AttributeCount = .AttributeCount + 1
                '                Loop
                'End If
            End With
            '
            Exit Sub
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("unexpected exception"))
        End Sub
        ''
        ''   CursorPosition points to the first character of an attribute name
        ''   ElementValue has no spaces before and after '=', and no double spaces anywhere
        ''   ElementValue whiteSpace has been converted to " "
        ''
        'Private Function GetAttributeDelimiterPosition(CursorPosition as integer, ElementValue As String) as integer
        '    '
        '    Dim SpacePosition as integer
        '    Dim QuotePosition as integer
        '    Dim EndPosition as integer
        '    Dim EqualPosition as integer
        '    Dim TestPosition as integer
        '    Dim TestValue As String
        '    '
        '    CursorPosition = PassWhiteSpace(CursorPosition, ElementValue)
        '
        '    GetAttributeDelimiterPosition = 0
        '    EndPosition = Len(ElementValue)
        '    SpacePosition = GetLesserNonZero(InStr(CursorPosition, ElementValue, " "), EndPosition + 1)
        '    QuotePosition = GetLesserNonZero(InStr(CursorPosition, ElementValue, """"), EndPosition + 1)
        '    EqualPosition = GetLesserNonZero(InStr(CursorPosition, ElementValue, "="), EndPosition + 1)
        '    '
        '    If SpacePosition < EqualPosition Then
        '        '
        '        ' ----- Case 1, attribute without a value
        '        '
        '
        '        End If
        'End Function
        '
        '
        '
        Private Function GetLesserNonZero(ByVal value0 As Integer, ByVal value1 As Integer) As Integer
            On Error GoTo ErrorTrap
            '
            If value0 = 0 Then
                GetLesserNonZero = value1
            Else
                If value1 = 0 Then
                    GetLesserNonZero = value0
                Else
                    If value0 < value1 Then
                        GetLesserNonZero = value0
                    Else
                        GetLesserNonZero = value1
                    End If
                End If
            End If
            '
            Exit Function
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("unexpected exception"))
        End Function
        '
        ' Pass spaces at the current cursor position
        '
        Private Function PassWhiteSpace(ByVal CursorPosition As Integer, ByVal TagString As String) As Integer
            On Error GoTo ErrorTrap
            '
            PassWhiteSpace = CursorPosition
            Do While (Mid(TagString, PassWhiteSpace, 1) = " ") And (PassWhiteSpace < Len(TagString))
                PassWhiteSpace = PassWhiteSpace + 1
            Loop
            '
            Exit Function
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("unexpected exception"))
        End Function
        '        '
        '        ' Create the full URI from a possible relative URI
        '        '   URIBase is the URI of the page that contains this URI
        '        '       blank if the URI is not from a link
        '        '       it can also be from the base tag
        '        '
        '        Private Function GetAbsoluteURL(ByVal URIWorking As String, ByVal URIBase As String)
        '            On Error GoTo ErrorTrap
        '            '
        '            Dim RightSide As String
        '            Dim LeftSide As String
        '            Dim QueryString As String
        '            Dim Position As Integer
        '            Dim BaseProtocol As String
        '            Dim BaseHost As String
        '            Dim BasePath As String
        '            Dim BasePage As String
        '            Dim BaseQueryString As String
        '            If (Left(UCase(URIWorking), 5) <> "HTTP:") Then
        '                '
        '                ' path is relative, construct from base
        '                '
        '                If URIBase = "" Then
        '                    '
        '                    ' URI base is not given, use the working URI instead
        '                    '
        '                    URIBase = URIWorking
        '                End If
        '                '
        '                ' make sure base does not have anchors or querystrings
        '                '
        '                Position = InStr(1, URIBase, "#")
        '                If Position <> 0 Then
        '                    URIBase = Mid(URIBase, 1, Position - 1)
        '                End If
        '                Position = InStr(1, URIBase, "?")
        '                If Position <> 0 Then
        '                    URIBase = Mid(URIBase, 1, Position - 1)
        '                End If
        '                '
        '                ' save base host, path and page
        '                '
        '                If Mid(URIWorking, 1, 1) = "#" Then
        '                    URIWorking = URIWorking
        '                End If
        '                Call SeparateURL(URIBase, BaseProtocol, BaseHost, BasePath, BasePage, BaseQueryString)
        '                '
        '                ' if URI is only an anchor or a querysting, use base plus URI
        '                '
        '                If Mid(URIWorking, 1, 1) = "?" Then
        '                    URIWorking = BasePath & BasePage & URIWorking
        '                End If
        '                If Mid(URIWorking, 1, 1) = "#" Then
        '                    URIWorking = BasePath & BasePage & URIWorking
        '                End If
        '                '
        '                ' if path does not go to root directory, stick on base path
        '                '
        '                If Mid(URIWorking, 1, 1) <> "/" Then
        '                    URIWorking = BasePath & URIWorking
        '                End If
        '                Position = InStr(1, URIWorking, "../")
        '                Do Until Position = 0
        '                    '
        '                    ' if path contains directory changes, do the move
        '                    '
        '                    RightSide = Mid(URIWorking, Position + 3)
        '                    LeftSide = Mid(URIWorking, 1, Position - 1)
        '                    If Len(LeftSide) > 1 Then LeftSide = Mid(LeftSide, 1, Len(LeftSide) - 1)
        '                    Do While Len(LeftSide) > 1 And Mid(LeftSide, Len(LeftSide), 1) <> "/"
        '                        LeftSide = Mid(LeftSide, 1, Len(LeftSide) - 1)
        '                        'DoEvents()
        '                    Loop
        '                    URIWorking = LeftSide + RightSide
        '                    Position = InStr(1, URIWorking, "../")
        '                    'DoEvents()
        '                Loop
        '                Position = InStr(1, URIWorking, "./")
        '                Do Until Position = 0
        '                    '
        '                    ' if path contains directory marks, remove them
        '                    '
        '                    RightSide = Mid(URIWorking, Position + 2)
        '                    LeftSide = Mid(URIWorking, 1, Position - 1)
        '                    Do While Len(LeftSide) > 1 And Mid(LeftSide, Len(LeftSide), 1) <> "/"
        '                        LeftSide = Mid(LeftSide, 1, Len(LeftSide) - 1)
        '                        'DoEvents()
        '                    Loop
        '                    URIWorking = LeftSide + RightSide
        '                    Position = InStr(1, URIWorking, "./")
        '                    'DoEvents()
        '                Loop
        '                '
        '                ' add the protocol and host
        '                '
        '                URIWorking = "http://" & BaseHost & URIWorking
        '            End If
        '            GetAbsoluteURL = URIWorking
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            cpCore.handleException(New Exception("unexpected exception"))
        '        End Function
        '
        '========================================================================
        '   Get all the text and tags between this tag and its close
        '
        '   If it does not close correctly, return "<ERROR0>"
        '========================================================================
        '
        Public Function TagInnerText(ByVal ElementPointer As Integer) As String
            On Error GoTo ErrorTrap
            '
            Dim iElementPointer As Integer
            Dim iElementStart As Integer
            Dim iElementCount As Integer
            Dim TagName As String
            Dim TagNameEnd As String
            Dim TagCount As Integer
            '
            Call LoadElement(ElementPointer)
            If ElementPointer >= 0 Then
                If LocalElements(ElementPointer).IsTag Then
                    iElementPointer = ElementPointer + 1

                    TagName = UCase(LocalElements(ElementPointer).TagName)
                    TagNameEnd = "/" & TagName
                    TagCount = 1
                    Do While TagCount <> 0 And iElementPointer < LocalElementCount
                        Call LoadElement(iElementPointer)
                        With LocalElements(iElementPointer)
                            If Not .IsTag Then
                                TagInnerText = TagInnerText & .Text
                            Else
                                Select Case UCase(.TagName)
                                    Case TagName
                                        TagCount = TagCount + 1
                                        TagInnerText = TagInnerText & .Text
                                    Case TagNameEnd
                                        TagCount = TagCount - 1
                                        If TagCount <> 0 Then
                                            TagInnerText = TagInnerText & .Text
                                        End If
                                    Case Else
                                        TagInnerText = TagInnerText & .Text
                                End Select
                            End If
                        End With
                        iElementPointer = iElementPointer + 1
                    Loop
                    If iElementPointer >= LocalElementCount Then
                        TagInnerText = "<ERROR0>"
                    End If
                End If
            End If
            '
            Exit Function
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("unexpected exception"))
        End Function
        '
        '
        '
        Private Sub LoadElement(ByVal ElementPtr As Integer)
            Dim SplitPtr As Integer
            Dim SplitSrc As String
            Dim TagPtr As Integer
            Dim BodyPtr As Integer
            Dim ElementBasePtr As Integer
            Dim Ptr As Integer
            Dim SrcTag As String
            Dim SrcBody As String
            '
            If NewWay Then
                If Not LocalElements(ElementPtr).Loaded Then
                    SplitPtr = CInt(ElementPtr / 2)
                    ElementBasePtr = SplitPtr * 2
                    SplitSrc = SplitStore(SplitPtr)
                    Ptr = InStr(1, SplitSrc, ">")
                    '
                    ' replace blobs
                    '
                    If Ptr = 0 Then
                        SrcTag = ""
                        SrcBody = ReplaceBlob(SplitSrc)
                    Else
                        SrcTag = ReplaceBlob(Mid(SplitSrc, 1, Ptr))
                        SrcBody = ReplaceBlob(Mid(SplitSrc, Ptr + 1))
                    End If
                    If Ptr = 0 Then
                        If ElementPtr = 0 Then
                            '
                            ' no close tag, elementptr=0 then First entry is empty, second is body
                            '
                            With LocalElements(ElementBasePtr)
                                .AttributeCount = 0
                                .IsTag = False
                                .Loaded = True
                                .Position = 0
                                .Text = ""
                            End With
                            '
                            With LocalElements(ElementBasePtr + 1)
                                .AttributeCount = 0
                                .IsTag = False
                                .Loaded = True
                                .Position = 0
                                .Text = SplitSrc
                            End With
                        Else
                            '
                            ' no close tag, elementptr>0 then First entry is '<', second is body
                            '
                            With LocalElements(ElementBasePtr)
                                .AttributeCount = 0
                                .IsTag = False
                                .Loaded = True
                                .Position = 0
                                .Text = "<"
                            End With
                            '
                            With LocalElements(ElementBasePtr + 1)
                                .AttributeCount = 0
                                .IsTag = False
                                .Loaded = True
                                .Position = 0
                                .Text = SplitSrc
                            End With
                        End If
                    Else
                        '
                        ' close tag found, first entry is tag text, second entry is body
                        '
                        With LocalElements(ElementBasePtr)
                            .Text = "<" & SrcTag
                            .IsTag = True
                            Call ParseTag(ElementBasePtr)
                            .Loaded = True
                        End With
                        '
                        With LocalElements(ElementBasePtr + 1)
                            .AttributeCount = 0
                            .IsTag = False
                            .Loaded = True
                            .Position = 0
                            .Text = SrcBody
                        End With
                    End If
                End If
            End If
        End Sub
        '
        '
        '
        Private Function ReplaceBlob(ByVal Src As String) As String
            Dim Pos As Integer
            Dim PosEnd As Integer
            Dim PosNum As Integer
            Dim PtrText As String
            Dim Ptr As Integer
            Dim Blob As String
            '
            ReplaceBlob = Src
            Pos = InStr(1, Src, BlobSN)
            If Pos <> 0 Then
                PosEnd = InStr(Pos + 1, Src, "/")
                If PosEnd > 0 Then
                    PosNum = InStr(Pos + 1, Src, ":")
                    If PosNum > 0 Then
                        PtrText = Mid(Src, PosNum + 1, PosEnd - PosNum - 1)
                        If IsNumeric(PtrText) Then
                            Ptr = CInt(PtrText)
                            If Ptr < BlobCnt Then
                                Blob = Blobs(Ptr)
                            End If
                            ReplaceBlob = Mid(Src, 1, Pos - 1) & Blob & Mid(Src, PosEnd + 1)
                        End If
                    End If
                End If
            End If

        End Function
        '
        '========================================================================
        ''' <summary>
        ''' handle legacy class errors
        ''' </summary>
        ''' <param name="MethodName"></param>
        ''' <param name="ErrNumber"></param>
        ''' <param name="ErrSource"></param>
        ''' <param name="ErrDescription"></param>
        ''' <remarks></remarks>
        Private Sub handleLegacyClassError(ByVal MethodName As String, ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String)
            '
            cpCore.handleExceptionAndRethrow(New Exception("unexpected exception in method [" & MethodName & "], ErrDescription [" & ErrDescription & "]"))
            '
        End Sub
    End Class
End Namespace
