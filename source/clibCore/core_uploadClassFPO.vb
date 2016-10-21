

Imports Contensive.Core.ccCommonModule
'Imports Contensive.Core.cpCommonUtilsClass
'Imports Interop.adodb

'
' findReplace as integer to as integer
' just the document -- replace out 
' if 'Imports Interop.adodb, replace in ObjectStateEnum.adState...
' findreplace encode to encode
' findreplace ''DoEvents to '''DoEvents
' runProcess becomes runProcess
' Sleep becomes Threading.Thread.Sleep(
' as object to as object
' convert types to structures (private structure, public structure, End structure)
'
Namespace Contensive.Core
    Public Class uploadClassFPO
        Friend Property binaryHeader As Byte()
        'Friend Property Count As Integer


        Private BinaryHeaderLocal As Object                'the HTTP header. Read in as Binary

        '
        'Private MyScriptingContext  As ASPTypeLibrary.ScriptingContext
        'Private MyRequest           As ASPTypeLibrary.Request
        Private ItemStorageCollection As Collection
        Private ItemNames() As String
        Private ItemCount as integer
        ''
        ''
        ''
        'Public Sub OnStartPage(PassedScriptingContext As ScriptingContext)
        '    On Error GoTo ErrorTrap
        '    '
        '    Dim HeaderLength as integer  'the number of bytes sent
        '    '
        '    MyScriptingContext = PassedScriptingContext
        '    MyRequest = MyScriptingContext.Request
        '
        '    ' then build the collection...
        '    HeaderLength = MyRequest.TotalBytes
        '    If HeaderLength > 0 Then
        '        BinaryHeaderLocal = MyRequest.BinaryRead(HeaderLength - 1)
        '        ItemStorageCollection = BuildForm
        '        End If
        '    '
        '    Exit Sub
        '    '
        'ErrorTrap:
        'End Sub
        '
        '
        '
        Public ReadOnly Property Form(ByVal Key As Object) As itemStorageClass
            Get
                If Not ItemStorageCollection Is Nothing Then
                    If ItemStorageCollection.Count > 0 Then
                        If FieldExists(Key) Then
                            Form = ItemStorageCollection(Key)
                        Else
                            Form = ItemStorageCollection("EMPTY")
                        End If
                    End If
                End If
            End Get
        End Property
        '
        ' Get the Count of Form collection
        '
        Public ReadOnly Property Count() as integer
            Get
                If ItemStorageCollection Is Nothing Then
                    Count = 0
                Else
                    Count = ItemCount
                    'Count = ItemStorageCollection.Count
                End If

            End Get
        End Property
        '
        ' Populate the Form Collection Property from the Request object
        '
        Private Function BuildForm() As Collection
            '            On Error GoTo ErrorTrap
            '            '
            '            Dim HeaderLength As Object  'the number of bytes sent
            '            Dim HeaderSectionDelimiter As Object  'the delimiter that divides the fields
            '            Dim HeaderSectionEnd as integer
            '            Dim HeaderLine As String
            '            Dim HeaderLineDelimiter As Object
            '            Dim HeaderLineBreak as integer
            '            Dim HeaderLineStart as integer
            '            Dim HeaderLineEnd as integer
            '            '
            '            Dim ItemStorageName As String
            '            Dim ItemStorage As ItemStorageClass
            '            Dim ItemTemp As ItemStorageClass
            '            '
            '            Dim ErrMessage As String
            '            '
            '            Dim Filename As Object
            '            '
            '            Dim FieldStart as integer
            '            Dim FieldEnd as integer
            '            Dim LoopCount as integer
            '            Dim UnicodeHeader As Object
            '            '
            '            'Initialize collection
            '            '
            '            BuildForm = New Collection
            '            '
            '            ' Populate the empty entry, used if a bad key is requested
            '            '
            '            ItemStorage = New ItemStorageClass
            '            ItemStorage.Value = ""
            '            ItemStorage.FileSize = 0
            '            ItemStorage.ContentType = ""
            '            ItemStorage.Filename = ""
            '            BuildForm.Add(ItemStorage, "EMPTY")
            '            '
            '            ItemCount = 0
            '            '
            '            UnicodeHeader = StrConv(BinaryHeaderLocal, vbUnicode)
            '            HeaderLength = LenB(UnicodeHeader)
            '            If HeaderLength <> 0 Then
            '                '
            '                ' Delimeter string, the encode type is multi-part
            '                '
            '                HeaderLineDelimiter = vbCrLf
            '                HeaderLineBreak = InStrB(1, UnicodeHeader, HeaderLineDelimiter)
            '                HeaderSectionDelimiter = LeftB(UnicodeHeader, HeaderLineBreak - 1)
            '                HeaderSectionEnd = InStrB(HeaderLineBreak, UnicodeHeader, HeaderSectionDelimiter)
            '                If HeaderSectionEnd = 0 Then
            '                    HeaderSectionEnd = HeaderLength
            '                End If
            '                Do While (HeaderLineBreak <> 0) And (HeaderLineBreak < HeaderLength) And (HeaderSectionEnd > 0)
            '                    '
            '                    ' loop through all the lines of this section creating the itemstorage object
            '                    '
            '                    ItemStorage = New ItemStorageClass
            '                    ItemStorageName = ""
            '                    Do While (HeaderLineBreak <> 0) And (HeaderLineBreak < HeaderSectionEnd)
            '                        HeaderLineStart = HeaderLineBreak + 4
            '                        '
            '                        ' process this line
            '                        '
            '                        HeaderLineBreak = InStrB(HeaderLineStart, UnicodeHeader, HeaderLineDelimiter)
            '                        If HeaderLineBreak >= HeaderLineStart Then
            '                            HeaderLine = MidB(UnicodeHeader, HeaderLineStart, HeaderLineBreak - HeaderLineStart)
            '                            If HeaderLine = "" Then
            '                                '
            '                                ' Blank line - entity follows
            '                                '
            '                                HeaderLineStart = HeaderLineStart + 4
            '                                ItemStorage.Value = MidB(UnicodeHeader, HeaderLineStart, HeaderSectionEnd - HeaderLineStart - 4)
            '                                HeaderLineBreak = HeaderSectionEnd
            '                                HeaderLineStart = HeaderSectionEnd
            '                            ElseIf InStrB(1, HeaderLine, ":") <> 0 Then
            '                                '
            '                                ' Read Commands from the line
            '                                '
            '                                Dim HeaderCommand As Object
            '                                HeaderCommand = MidB(HeaderLine, 1, InStrB(1, HeaderLine, ":") - 1)
            '                                Select Case HeaderCommand
            '                                    Case "Content-Disposition"
            '                                        '
            '                                        ' Read in the name of the item
            '                                        '
            '                                        FieldStart = InStrB(1, HeaderLine, " name=", vbTextCompare)
            '                                        If FieldStart <> 0 Then
            '                                            FieldStart = FieldStart + 14
            '                                            FieldEnd = InStrB(FieldStart, HeaderLine, """")
            '                                            ItemStorageName = MidB(HeaderLine, FieldStart, FieldEnd - FieldStart)
            '                                        End If
            '                                        '
            '                                        ' Read in the filename of the item
            '                                        '
            '                                        FieldStart = InStrB(1, HeaderLine, " filename=", vbTextCompare)
            '                                        If FieldStart <> 0 Then
            '                                            FieldStart = FieldStart + 22
            '                                            FieldEnd = InStrB(FieldStart, HeaderLine, """")
            '                                            Filename = MidB(HeaderLine, FieldStart, FieldEnd - FieldStart)
            '                                            Filename = Replace(Filename, "/", "\")
            '                                            FieldStart = InStrB(1, Filename, "\")
            '                                            LoopCount = 0
            '                                            Do While FieldStart <> 0 And LoopCount < 100
            '                                                Filename = MidB(Filename, FieldStart + 2)
            '                                                FieldStart = InStrB(1, Filename, "\")
            '                                                LoopCount = LoopCount + 1
            '                                            Loop
            '                                            ItemStorage.Filename = Filename
            '                                        End If
            '                                    Case "Content-Type"
            '                                        '
            '                                        ' Read in the Content Type
            '                                        '
            '                                        'FieldStart = InStrB(1, HeaderLine, ":") + 1
            '                                        ItemStorage.ContentType = MidB(HeaderLine, 29)
            '                                End Select
            '                            End If
            '                        End If
            '                    Loop
            '                    If ItemStorageName <> "" Then
            '                        If FieldExists(ItemStorageName) Then
            '                            ItemTemp = BuildForm.Item(ItemStorageName)
            '                            ItemTemp.Value = ItemTemp.Value & "," & ItemStorage.Value
            '                        Else
            '                            '
            '                            ' ----- Assign formfieldnames and formfieldvalues to collection
            '                            '
            '                            BuildForm.Add(ItemStorage, ItemStorageName)
            '                            '
            '                            ' ----- Store the field name in the ItemNames array so we can check them later
            '                            '
            '                            ItemCount = ItemCount + 1
            '                            ReDim Preserve ItemNames(ItemCount)
            '                            ItemNames(ItemCount - 1) = UCase(ItemStorageName)
            '                        End If
            '                    End If
            '                    '
            '                    ' ----- destroy form field object
            '                    '
            '                    ItemStorage = Nothing
            '                    '
            '                    ' ----- Find the next section
            '                    '
            '                    HeaderSectionEnd = InStrB(HeaderSectionEnd + 1, UnicodeHeader, HeaderSectionDelimiter)
            '                Loop
            '            End If
            '            Exit Function
            '            '
            'ErrorTrap:
        End Function
        '
        ' Test if a key exists in the form collection
        '
        Public Function FieldExists(ByVal Key As Object) As Boolean
            On Error GoTo ErrorTrap
            '
            Dim ItemPointer as integer
            Dim UcaseKey As String
            Dim ErrMessage As String
            '
            FieldExists = False
            If (ItemCount > 0) And Not IsNull(Key) Then
                UcaseKey = UCase(Key)
                For ItemPointer = 0 To ItemCount - 1
                    If ItemNames(ItemPointer) = UcaseKey Then
                        FieldExists = True
                        Exit For
                    End If
                Next
            End If
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError("FieldExists", Err.Number, Err.Source, Err.Description)
        End Function


        Public Function Key(ByVal Index as integer) As String
            On Error GoTo ErrorTrap
            '
            If Index < ItemCount Then
                Key = ItemNames(Index)
            End If
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError("FieldExists", Err.Number, Err.Source, Err.Description)
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' handle legacy errors in this class
        ''' </summary>
        ''' <param name="MethodName"></param>
        ''' <param name="ErrNumber"></param>
        ''' <param name="ErrSource"></param>
        ''' <param name="ErrDescription"></param>
        ''' <remarks></remarks>
        Private Sub handleLegacyClassError(ByVal MethodName As String, ByVal ErrNumber as integer, ByVal ErrSource As String, ByVal ErrDescription As String)
            '
            On Error GoTo 0
            Call Err.Raise(ErrNumber, ErrSource, "App.EXEName" & ".Upload." & MethodName & " encountered and error: " & ErrDescription)
            '
        End Sub
    End Class
End Namespace
