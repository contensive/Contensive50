
Option Strict On
Option Explicit On

Imports Contensive.BaseClasses
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Text
'
Namespace Contensive.Core.Controllers
    '
    '====================================================================================================
    ''' <summary>
    ''' controller for shared non-specific tasks
    ''' </summary>
    Public Class genericController
        '
        '
        '====================================================================================================
        ''' <summary>
        ''' return a normalized guid in registry format
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <param name="registryFormat"></param>
        ''' <returns></returns>
        Public Shared Function getGUID(Optional ByRef registryFormat As Boolean = False) As String
            Dim result As String = ""
            Dim g As Guid = Guid.NewGuid
            If g <> Guid.Empty Then
                result = g.ToString
                '
                If result <> "" Then
                    result = If(registryFormat, result, "{" & result & "}")
                End If
            End If
            Return result
        End Function
        '    '
        '    '==========================================================================
        '    '   Convert a variant to an long (long)
        '    '   returns 0 if the input is not an integer
        '    '   if float, rounds to integer
        '    '==========================================================================
        '    '
        '    Public shared Function EncodeInteger(ExpressionVariant As Object) As Integer
        '        ' 7/14/2009 - cover the overflow case, return 0
        '        On Error Resume Next
        '        '
        '        If Not IsArray(ExpressionVariant) Then
        '            If Not IsMissing(ExpressionVariant) Then
        '                If Not IsNull(ExpressionVariant) Then
        '                    If ExpressionVariant <> "" Then
        '                        If vbIsNumeric(ExpressionVariant) Then
        '                            encodeInteger = CLng(ExpressionVariant)
        '                        End If
        '                    End If
        '                End If
        '            End If
        '        End If
        '        '
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '        encodeInteger = 0
        '    End Function
        '    '
        '    '==========================================================================
        '    '   Convert a variant to a number (double)
        '    '   returns 0 if the input is not a number
        '    '==========================================================================
        '    '
        '    Public shared Function encodeNumber(ExpressionVariant As Object) As Double
        '        On Error GoTo ErrorTrap
        '        '
        '        'encodeNumber = 0
        '        If Not IsMissing(ExpressionVariant) Then
        '            If Not IsNull(ExpressionVariant) Then
        '                If ExpressionVariant <> "" Then
        '                    If vbIsNumeric(ExpressionVariant) Then
        '                        encodeNumber = ExpressionVariant
        '                    End If
        '                End If
        '            End If
        '        End If
        '        '
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '        encodeNumber = 0
        '    End Function
        '    '
        '    '==========================================================================
        '    '   Convert a variant to a date
        '    '   returns 0 if the input is not a number
        '    '==========================================================================
        '    '
        '    Public shared Function  EncodeDate(ExpressionVariant As Object) As Date
        '        On Error GoTo ErrorTrap
        '        '
        '        '    encodeDate = CDate(ExpressionVariant)
        '        '    encodeDate = CDate("1/1/1980")
        '        'encodeDate = Date.MinValue
        '        If Not IsMissing(ExpressionVariant) Then
        '            If Not IsNull(ExpressionVariant) Then
        '                If ExpressionVariant <> "" Then
        '                    If IsDate(ExpressionVariant) Then
        '                        encodeDate = ExpressionVariant
        '                    End If
        '                End If
        '            End If
        '        End If
        '        '
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '        encodeDate = Date.MinValue
        '    End Function
        '    '
        '    '==========================================================================
        '    '   Convert a variant to a boolean
        '    '   Returns true if input is not false, else false
        '    '==========================================================================
        '    '
        '    Public shared Function EncodeBoolean(ExpressionVariant As Object) As Boolean
        '        On Error GoTo ErrorTrap
        '        '
        '        'encodeBoolean = False
        '        If Not IsMissing(ExpressionVariant) Then
        '            If Not IsNull(ExpressionVariant) Then
        '                If ExpressionVariant <> "" Then
        '                    If vbIsNumeric(ExpressionVariant) Then
        '                        If ExpressionVariant <> "0" Then
        '                            If ExpressionVariant <> 0 Then
        '                                encodeBoolean = True
        '                            End If
        '                        End If
        '                    ElseIf vbUCase(ExpressionVariant) = "ON" Then
        '                        encodeBoolean = True
        '                    ElseIf vbUCase(ExpressionVariant) = "YES" Then
        '                        encodeBoolean = True
        '                    ElseIf vbUCase(ExpressionVariant) = "TRUE" Then
        '                        encodeBoolean = True
        '                    Else
        '                        encodeBoolean = False
        '                    End If
        '                End If
        '            End If
        '        End If
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '        encodeBoolean = False
        '    End Function
        '    '
        '    '==========================================================================
        '    '   Convert a variant into 0 or 1
        '    '   Returns 1 if input is not false, else 0
        '    '==========================================================================
        '    '
        '    Public shared Function encodeBit(ExpressionVariant As Object) As Integer
        '        On Error GoTo ErrorTrap
        '        '
        '        'encodeBit = 0
        '        If EncodeBoolean(ExpressionVariant) Then
        '            encodeBit = 1
        '        End If
        '        '
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '        encodeBit = 0
        '    End Function
        '    '
        '    '==========================================================================
        '    '   Convert a variant to a string
        '    '   returns emptystring if the input is not a string
        '    '==========================================================================
        '    '
        '    Public shared Function encodeText(ExpressionVariant As Object) As String
        '        On Error GoTo ErrorTrap
        '        '
        '        'encodeText = ""
        '        If Not IsMissing(ExpressionVariant) Then
        '            If Not IsNull(ExpressionVariant) Then
        '                encodeText = CStr(ExpressionVariant)
        '            End If
        '        End If
        '        '
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '        encodeText = ""
        '    End Function
        '    '
        '    '==========================================================================
        '    '   Converts a possibly missing value to variant
        '    '==========================================================================
        '    '
        '    Public shared Function encodeMissingText(ExpressionVariant As Object, DefaultVariant As Object) As Object
        '        'On Error GoTo ErrorTrap
        '        '
        '        If IsMissing(ExpressionVariant) Then
        '            encodeMissing = DefaultVariant
        '        Else
        '            encodeMissing = ExpressionVariant
        '        End If
        '        '
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '    End Function
        '
        '
        '
        Public Shared Function encodeEmptyText(ByVal sourceText As String, ByVal DefaultText As String) As String
            Dim returnText As String = sourceText
            If (returnText = "") Then
                returnText = DefaultText
            End If
            Return returnText
        End Function
        '
        '
        '
        Public Shared Function encodeEmptyInteger(ByVal sourceText As String, ByVal DefaultInteger As Integer) As Integer
            Return EncodeInteger(encodeEmptyText(sourceText, DefaultInteger.ToString))
        End Function
        '
        '
        '
        Public Shared Function encodeEmptyDate(ByVal sourceText As String, ByVal DefaultDate As Date) As Date
            Return EncodeDate(encodeEmptyText(sourceText, DefaultDate.ToString))
        End Function
        '
        '
        '
        Public Shared Function encodeEmptyNumber(ByVal sourceText As String, ByVal DefaultNumber As Double) As Double
            Return EncodeNumber(encodeEmptyText(sourceText, DefaultNumber.ToString))
        End Function
        '
        '
        '
        Public Shared Function encodeEmptyBoolean(ByVal sourceText As String, ByVal DefaultState As Boolean) As Boolean
            Return EncodeBoolean(encodeEmptyText(sourceText, DefaultState.ToString))
        End Function
        '    '
        '    '================================================================================================================
        '    '   Separate a URL into its host, path, page parts
        '    '================================================================================================================
        '    '
        '    Public shared sub SeparateURL(ByVal SourceURL As String, ByRef Protocol As String, ByRef Host As String, ByRef Path As String, ByRef Page As String, ByRef QueryString As String)
        '        'On Error GoTo ErrorTrap
        '        '
        '        '   Divide the URL into URLHost, URLPath, and URLPage
        '        '
        '        Dim WorkingURL As String
        '        Dim Position As Integer
        '        '
        '        ' Get Protocol (before the first :)
        '        '
        '        WorkingURL = SourceURL
        '        Position = vbInstr(1, WorkingURL, ":")
        '        'Position = vbInstr(1, WorkingURL, "://")
        '        If Position <> 0 Then
        '            Protocol = Mid(WorkingURL, 1, Position + 2)
        '            WorkingURL = Mid(WorkingURL, Position + 3)
        '        End If
        '        '
        '        ' compatibility fix
        '        '
        '        If vbInstr(1, WorkingURL, "//") = 1 Then
        '            If Protocol = "" Then
        '                Protocol = "http:"
        '            End If
        '            Protocol = Protocol & "//"
        '            WorkingURL = Mid(WorkingURL, 3)
        '        End If
        '        '
        '        ' Get QueryString
        '        '
        '        Position = vbInstr(1, WorkingURL, "?")
        '        If Position > 0 Then
        '            QueryString = Mid(WorkingURL, Position)
        '            WorkingURL = Mid(WorkingURL, 1, Position - 1)
        '        End If
        '        '
        '        ' separate host from pathpage
        '        '
        '        'iURLHost = WorkingURL
        '        Position = vbInstr(WorkingURL, "/")
        '        If (Position = 0) And (Protocol = "") Then
        '            '
        '            ' Page without path or host
        '            '
        '            Page = WorkingURL
        '            Path = ""
        '            Host = ""
        '        ElseIf (Position = 0) Then
        '            '
        '            ' host, without path or page
        '            '
        '            Page = ""
        '            Path = "/"
        '            Host = WorkingURL
        '        Else
        '            '
        '            ' host with a path (at least)
        '            '
        '            Path = Mid(WorkingURL, Position)
        '            Host = Mid(WorkingURL, 1, Position - 1)
        '            '
        '            ' separate page from path
        '            '
        '            Position = InStrRev(Path, "/")
        '            If Position = 0 Then
        '                '
        '                ' no path, just a page
        '                '
        '                Page = Path
        '                Path = "/"
        '            Else
        '                Page = Mid(Path, Position + 1)
        '                Path = Mid(Path, 1, Position)
        '            End If
        '        End If
        '        Exit Sub
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '    End Sub
        '    '
        '    '================================================================================================================
        '    '   Separate a URL into its host, path, page parts
        '    '================================================================================================================
        '    '
        '    Public shared sub ParseURL(ByVal SourceURL As String, ByRef Protocol As String, ByRef Host As String, ByRef Port As String, ByRef Path As String, ByRef Page As String, ByRef QueryString As String)
        '        'On Error GoTo ErrorTrap
        '        '
        '        '   Divide the URL into URLHost, URLPath, and URLPage
        '        '
        '        Dim iURLWorking As String               ' internal storage for GetURL functions
        '        Dim iURLProtocol As String
        '        Dim iURLHost As String
        '        Dim iURLPort As String
        '        Dim iURLPath As String
        '        Dim iURLPage As String
        '        Dim iURLQueryString As String
        '        Dim Position As Integer
        '        '
        '        iURLWorking = SourceURL
        '        Position = vbInstr(1, iURLWorking, "://")
        '        If Position <> 0 Then
        '            iURLProtocol = Mid(iURLWorking, 1, Position + 2)
        '            iURLWorking = Mid(iURLWorking, Position + 3)
        '        End If
        '        '
        '        ' separate Host:Port from pathpage
        '        '
        '        iURLHost = iURLWorking
        '        Position = vbInstr(iURLHost, "/")
        '        If Position = 0 Then
        '            '
        '            ' just host, no path or page
        '            '
        '            iURLPath = "/"
        '            iURLPage = ""
        '        Else
        '            iURLPath = Mid(iURLHost, Position)
        '            iURLHost = Mid(iURLHost, 1, Position - 1)
        '            '
        '            ' separate page from path
        '            '
        '            Position = InStrRev(iURLPath, "/")
        '            If Position = 0 Then
        '                '
        '                ' no path, just a page
        '                '
        '                iURLPage = iURLPath
        '                iURLPath = "/"
        '            Else
        '                iURLPage = Mid(iURLPath, Position + 1)
        '                iURLPath = Mid(iURLPath, 1, Position)
        '            End If
        '        End If
        '        '
        '        ' Divide Host from Port
        '        '
        '        Position = vbInstr(iURLHost, ":")
        '        If Position = 0 Then
        '            '
        '            ' host not given, take a guess
        '            '
        '            Select Case vbUCase(iURLProtocol)
        '                Case "FTP://"
        '                    iURLPort = "21"
        '                Case "HTTP://", "HTTPS://"
        '                    iURLPort = "80"
        '                Case Else
        '                    iURLPort = "80"
        '            End Select
        '        Else
        '            iURLPort = Mid(iURLHost, Position + 1)
        '            iURLHost = Mid(iURLHost, 1, Position - 1)
        '        End If
        '        Position = vbInstr(1, iURLPage, "?")
        '        If Position > 0 Then
        '            iURLQueryString = Mid(iURLPage, Position)
        '            iURLPage = Mid(iURLPage, 1, Position - 1)
        '        End If
        '        Protocol = iURLProtocol
        '        Host = iURLHost
        '        Port = iURLPort
        '        Path = iURLPath
        '        Page = iURLPage
        '        QueryString = iURLQueryString
        '        Exit Sub
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '    End Sub
        '    '
        '    '
        '    '
        '    Function DecodeGMTDate(GMTDate As String) As Date
        '        'On Error GoTo ErrorTrap
        '        '
        '        Dim WorkString As String
        '        DecodeGMTDate = 0
        '        If GMTDate <> "" Then
        '            WorkString = Mid(GMTDate, 6, 11)
        '            If IsDate(WorkString) Then
        '                DecodeGMTDate = CDate(WorkString)
        '                WorkString = Mid(GMTDate, 18, 8)
        '                If IsDate(WorkString) Then
        '                    DecodeGMTDate = DecodeGMTDate + CDate(WorkString) + 4 / 24
        '                End If
        '            End If
        '        End If
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '    End Function
        '    '
        '    '
        '    '
        '    Function EncodeGMTDate(MSDate As Date) As String
        '        'On Error GoTo ErrorTrap
        '        '
        '        Dim WorkString As String
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '    End Function
        '    '
        '    '=================================================================================
        '    '   Renamed to catch all the cases that used it in addons
        '    '
        '    '   Do not use this routine in Addons to get the addon option string value
        '    '   to get the value in an option string, use cmc.csv_getAddonOption("name")
        '    '
        '    ' Get the value of a name in a string of name value pairs parsed with vrlf and =
        '    '   the legacy line delimiter was a '&' -> name1=value1&name2=value2"
        '    '   new format is "name1=value1 crlf name2=value2 crlf ..."
        '    '   There can be no extra spaces between the delimiter, the name and the "="
        '    '=================================================================================
        '    '
        '    Function getSimpleNameValue(Name As String, ArgumentString As String, DefaultValue As String, Delimiter As String) As String
        '        'Function getArgument(Name As String, ArgumentString As String, Optional DefaultValue as object, Optional Delimiter As String) As String
        '        '
        '        Dim WorkingString As String
        '        Dim iDefaultValue As String
        '        Dim NameLength As Integer
        '        Dim ValueStart As Integer
        '        Dim ValueEnd As Integer
        '        Dim IsQuoted As Boolean
        '        '
        '        ' determine delimiter
        '        '
        '        If Delimiter = "" Then
        '            '
        '            ' If not explicit
        '            '
        '            If vbInstr(1, ArgumentString, vbCrLf) <> 0 Then
        '                '
        '                ' crlf can only be here if it is the delimiter
        '                '
        '                Delimiter = vbCrLf
        '            Else
        '                '
        '                ' either only one option, or it is the legacy '&' delimit
        '                '
        '                Delimiter = "&"
        '            End If
        '        End If
        '        iDefaultValue = encodeMissingText(DefaultValue, "")
        '        WorkingString = ArgumentString
        '        getSimpleNameValue = iDefaultValue
        '        If WorkingString <> "" Then
        '            WorkingString = Delimiter & WorkingString & Delimiter
        '            ValueStart = vbInstr(1, WorkingString, Delimiter & Name & "=", vbTextCompare)
        '            If ValueStart <> 0 Then
        '                NameLength = Len(Name)
        '                ValueStart = ValueStart + Len(Delimiter) + NameLength + 1
        '                If Mid(WorkingString, ValueStart, 1) = """" Then
        '                    IsQuoted = True
        '                    ValueStart = ValueStart + 1
        '                End If
        '                If IsQuoted Then
        '                    ValueEnd = vbInstr(ValueStart, WorkingString, """" & Delimiter)
        '                Else
        '                    ValueEnd = vbInstr(ValueStart, WorkingString, Delimiter)
        '                End If
        '                If ValueEnd = 0 Then
        '                    getSimpleNameValue = Mid(WorkingString, ValueStart)
        '                Else
        '                    getSimpleNameValue = Mid(WorkingString, ValueStart, ValueEnd - ValueStart)
        '                End If
        '            End If
        '        End If
        '        '

        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '    End Function
        '    '
        '    '=================================================================================
        '    '   Do not use this code
        '    '
        '    '   To retrieve a value from an option string, use cmc.csv_getAddonOption("name")
        '    '
        '    '   This was left here to work through any code issues that might arrise during
        '    '   the conversion.
        '    '
        '    '   Return the value from a name value pair, parsed with =,&[|].
        '    '   For example:
        '    '       name=Jay[Jay|Josh|Dwayne]
        '    '       the answer is Jay. If a select box is displayed, it is a dropdown of all three
        '    '=================================================================================
        '    '
        '    Public shared Function GetAggrOption_old(Name As String, SegmentCMDArgs As String) As String
        '        '
        '        Dim Pos As Integer
        '        '
        '        GetAggrOption_old = getSimpleNameValue(Name, SegmentCMDArgs, "", vbCrLf)
        '        '
        '        ' remove the manual select list syntax "answer[choice1|choice2]"
        '        '
        '        Pos = vbInstr(1, GetAggrOption_old, "[")
        '        If Pos <> 0 Then
        '            GetAggrOption_old = Left(GetAggrOption_old, Pos - 1)
        '        End If
        '        '
        '        ' remove any function syntax "answer{selectcontentname RSS Feeds}"
        '        '
        '        Pos = vbInstr(1, GetAggrOption_old, "{")
        '        If Pos <> 0 Then
        '            GetAggrOption_old = Left(GetAggrOption_old, Pos - 1)
        '        End If
        '        '
        '    End Function
        '    '
        '    '=================================================================================
        '    '   Do not use this code
        '    '
        '    '   To retrieve a value from an option string, use cmc.csv_getAddonOption("name")
        '    '
        '    '   This was left here to work through any code issues that might arrise during
        '    '   Compatibility for GetArgument
        '    '=================================================================================
        '    '
        'Function getNameValue_old(Name As String, ArgumentString As String, Optional DefaultValue as string = "") As String
        '        getNameValue_old = getSimpleNameValue(Name, ArgumentString, DefaultValue, vbCrLf)
        '    End Function
        '    '
        '    '========================================================================
        '    '   encodeSQLText
        '    '========================================================================
        '    '
        '    Public shared Function encodeSQLText(ExpressionVariant As Object) As String
        '        'On Error GoTo ErrorTrap
        '        '
        '        'Dim MethodName As String
        '        '
        '        'MethodName = "encodeSQLText"
        '        '
        '        If IsNull(ExpressionVariant) Then
        '            encodeSQLText = "null"
        '        ElseIf IsMissing(ExpressionVariant) Then
        '            encodeSQLText = "null"
        '        ElseIf ExpressionVariant = "" Then
        '            encodeSQLText = "null"
        '        Else
        '            encodeSQLText = CStr(ExpressionVariant)
        '            ' ??? this should not be here -- to correct a field used in a CDef, truncate in SaveCS by fieldtype
        '            'encodeSQLText = Left(ExpressionVariant, 255)
        '            'remove-can not find a case where | is not allowed to be saved.
        '            'encodeSQLText = vbReplace(encodeSQLText, "|", "_")
        '            encodeSQLText = "'" & vbReplace(encodeSQLText, "'", "''") & "'"
        '        End If
        '        Exit Function
        '        '
        '        ' ----- Error Trap
        '        '
        'ErrorTrap:
        '    End Function
        '    '
        '    '========================================================================
        '    '   encodeSQLLongText
        '    '========================================================================
        '    '
        '    Public shared Function encodeSQLLongText(ExpressionVariant As Object) As String
        '        'On Error GoTo ErrorTrap
        '        '
        '        'Dim MethodName As String
        '        '
        '        'MethodName = "encodeSQLLongText"
        '        '
        '        If IsNull(ExpressionVariant) Then
        '            encodeSQLLongText = "null"
        '        ElseIf IsMissing(ExpressionVariant) Then
        '            encodeSQLLongText = "null"
        '        ElseIf ExpressionVariant = "" Then
        '            encodeSQLLongText = "null"
        '        Else
        '            encodeSQLLongText = ExpressionVariant
        '            'encodeSQLLongText = vbReplace(ExpressionVariant, "|", "_")
        '            encodeSQLLongText = "'" & vbReplace(encodeSQLLongText, "'", "''") & "'"
        '        End If
        '        Exit Function
        '        '
        '        ' ----- Error Trap
        '        '
        'ErrorTrap:
        '    End Function
        '    '
        '    '========================================================================
        '    '   encodeSQLDate
        '    '       encode a date variable to go in an sql expression
        '    '========================================================================
        '    '
        '    Public shared Function encodeSQLDate(ExpressionVariant As Object) As String
        '        'On Error GoTo ErrorTrap
        '        '
        '        Dim TimeVar As Date
        '        Dim TimeValuething As Single
        '        Dim TimeHours As Integer
        '        Dim TimeMinutes As Integer
        '        Dim TimeSeconds As Integer
        '        'Dim MethodName As String
        '        ''
        '        'MethodName = "encodeSQLDate"
        '        '
        '        If IsNull(ExpressionVariant) Then
        '            encodeSQLDate = "null"
        '        ElseIf IsMissing(ExpressionVariant) Then
        '            encodeSQLDate = "null"
        '        ElseIf ExpressionVariant = "" Then
        '            encodeSQLDate = "null"
        '        ElseIf IsDate(ExpressionVariant) Then
        '            TimeVar = CDate(ExpressionVariant)
        '            If TimeVar = 0 Then
        '                encodeSQLDate = "null"
        '            Else
        '                TimeValuething = 86400.0! * (TimeVar - Int(TimeVar + 0.000011!))
        '                TimeHours = Int(TimeValuething / 3600.0!)
        '                If TimeHours >= 24 Then
        '                    TimeHours = 23
        '                End If
        '                TimeMinutes = Int(TimeValuething / 60.0!) - (TimeHours * 60)
        '                If TimeMinutes >= 60 Then
        '                    TimeMinutes = 59
        '                End If
        '                TimeSeconds = TimeValuething - (TimeHours * 3600.0!) - (TimeMinutes * 60.0!)
        '                If TimeSeconds >= 60 Then
        '                    TimeSeconds = 59
        '                End If
        '                encodeSQLDate = "{ts '" & Year(ExpressionVariant) & "-" & Right("0" & Month(ExpressionVariant), 2) & "-" & Right("0" & Day(ExpressionVariant), 2) & " " & Right("0" & TimeHours, 2) & ":" & Right("0" & TimeMinutes, 2) & ":" & Right("0" & TimeSeconds, 2) & "'}"
        '            End If
        '        Else
        '            encodeSQLDate = "null"
        '        End If
        '        Exit Function
        '        '
        '        ' ----- Error Trap
        '        '
        'ErrorTrap:
        '    End Function
        '    '
        '    '========================================================================
        '    '   encodeSQLNumber
        '    '       encode a number variable to go in an sql expression
        '    '========================================================================
        '    '
        '    Function encodeSQLNumber(ExpressionVariant As Object) As String
        '        'On Error GoTo ErrorTrap
        '        '
        '        'Dim MethodName As String
        '        ''
        '        'MethodName = "encodeSQLNumber"
        '        '
        '        If IsNull(ExpressionVariant) Then
        '            encodeSQLNumber = "null"
        '        ElseIf IsMissing(ExpressionVariant) Then
        '            encodeSQLNumber = "null"
        '        ElseIf ExpressionVariant = "" Then
        '            encodeSQLNumber = "null"
        '        ElseIf vbIsNumeric(ExpressionVariant) Then
        '            Select Case VarType(ExpressionVariant)
        '                Case vbBoolean
        '                    If ExpressionVariant Then
        '                        encodeSQLNumber = SQLTrue
        '                    Else
        '                        encodeSQLNumber = SQLFalse
        '                    End If
        '                Case Else
        '                    encodeSQLNumber = ExpressionVariant
        '            End Select
        '        Else
        '            encodeSQLNumber = "null"
        '        End If
        '        Exit Function
        '        '
        '        ' ----- Error Trap
        '        '
        'ErrorTrap:
        '    End Function
        '    '
        '    '========================================================================
        '    '   encodeSQLBoolean
        '    '       encode a boolean variable to go in an sql expression
        '    '========================================================================
        '    '
        '    Public shared Function encodeSQLBoolean(ExpressionVariant As Object) As String
        '        '
        '        Dim src As String
        '        '
        '        encodeSQLBoolean = SQLFalse
        '        If EncodeBoolean(ExpressionVariant) Then
        '            encodeSQLBoolean = SQLTrue
        '        End If
        '        '    If Not IsNull(ExpressionVariant) Then
        '        '        If Not IsMissing(ExpressionVariant) Then
        '        '            If ExpressionVariant <> False Then
        '        '                    encodeSQLBoolean = SQLTrue
        '        '                End If
        '        '            End If
        '        '        End If
        '        '    End If
        '        '
        '    End Function
        '    '
        '    '========================================================================
        '    '   Gets the next line from a string, and removes the line
        '    '========================================================================
        '    '
        '    Public shared Function getLine(Body As String) As String
        '        Dim EOL As String
        '        Dim NextCR As Integer
        '        Dim NextLF As Integer
        '        Dim BOL As Integer
        '        '
        '        NextCR = vbInstr(1, Body, vbCr)
        '        NextLF = vbInstr(1, Body, vbLf)

        '        If NextCR <> 0 Or NextLF <> 0 Then
        '            If NextCR <> 0 Then
        '                If NextLF <> 0 Then
        '                    If NextCR < NextLF Then
        '                        EOL = NextCR - 1
        '                        If NextLF = NextCR + 1 Then
        '                            BOL = NextLF + 1
        '                        Else
        '                            BOL = NextCR + 1
        '                        End If

        '                    Else
        '                        EOL = NextLF - 1
        '                        BOL = NextLF + 1
        '                    End If
        '                Else
        '                    EOL = NextCR - 1
        '                    BOL = NextCR + 1
        '                End If
        '            Else
        '                EOL = NextLF - 1
        '                BOL = NextLF + 1
        '            End If
        '            getLine = Mid(Body, 1, EOL)
        '            Body = Mid(Body, BOL)
        '        Else
        '            getLine = Body
        '            Body = ""
        '        End If

        '        'EOL = vbInstr(1, Body, vbCrLf)

        '        'If EOL <> 0 Then
        '        '    getLine = Mid(Body, 1, EOL - 1)
        '        '    Body = Mid(Body, EOL + 2)
        '        '    End If
        '        '
        '    End Function
        '    '
        '    '=================================================================================
        '    '   Get a Random Long Value
        '    '=================================================================================
        '    '
        '    Public shared Function GetRandomInteger() As Integer
        '        '
        '        Dim RandomBase As Integer
        '        Dim RandomLimit As Integer
        '        '
        '        RandomBase =Threading.Thread.CurrentThread.ManagedThreadId
        '        RandomBase = RandomBase And ((2 ^ 30) - 1)
        '        RandomLimit = (2 ^ 31) - RandomBase - 1
        '        Randomize()
        '        GetRandomInteger = RandomBase + (Rnd * RandomLimit)
        '        '
        '    End Function
        '    '
        '    '=================================================================================
        '    '
        '    '=================================================================================
        '    '
        '    Public shared Function isDataTableOk(RS As Object) As Boolean
        '        isDataTableOk = False
        '        If (isDataTableOk(rs)) Then
        '            If true Then
        '                If Not rs.rows.count=0 Then
        '                    isDataTableOk = True
        '                End If
        '            End If
        '        End If
        '    End Function
        '    '
        '    '=================================================================================
        '    '
        '    '=================================================================================
        '    '
        '    Public shared sub closeDataTable(RS As Object)
        '        If (isDataTableOk(rs)) Then
        '            If true Then
        '                Call 'RS.Close()
        '            End If
        '        End If
        '    End Sub
        '
        '=============================================================================
        ' Create the part of the sql where clause that is modified by the user
        '   WorkingQuery is the original querystring to change
        '   QueryName is the name part of the name pair to change
        '   If the QueryName is not found in the string, ignore call
        '=============================================================================
        '
        Public Shared Function ModifyQueryString(ByVal WorkingQuery As String, ByVal QueryName As String, ByVal QueryValue As String, Optional ByVal AddIfMissing As Boolean = True) As String
            '
            If WorkingQuery.IndexOf("?") > 0 Then
                ModifyQueryString = modifyLinkQuery(WorkingQuery, QueryName, QueryValue, AddIfMissing)
            Else
                ModifyQueryString = Mid(modifyLinkQuery("?" & WorkingQuery, QueryName, QueryValue, AddIfMissing), 2)
            End If
        End Function
        '
        Public Shared Function ModifyQueryString(ByVal WorkingQuery As String, ByVal QueryName As String, ByVal QueryValue As Integer, Optional ByVal AddIfMissing As Boolean = True) As String
            Return ModifyQueryString(WorkingQuery, QueryName, QueryValue.ToString, AddIfMissing)
        End Function
        '
        Public Shared Function ModifyQueryString(ByVal WorkingQuery As String, ByVal QueryName As String, ByVal QueryValue As Boolean, Optional ByVal AddIfMissing As Boolean = True) As String
            Return ModifyQueryString(WorkingQuery, QueryName, QueryValue.ToString, AddIfMissing)
        End Function
        '
        '=============================================================================
        ''' <summary>
        ''' Modify the querystring at the end of a link. If there is no, question mark, the link argument is assumed to be a link, not the querysting
        ''' </summary>
        ''' <param name="Link"></param>
        ''' <param name="QueryName"></param>
        ''' <param name="QueryValue"></param>
        ''' <param name="AddIfMissing"></param>
        ''' <returns></returns>
        Public Shared Function modifyLinkQuery(ByVal Link As String, ByVal QueryName As String, ByVal QueryValue As String, Optional ByVal AddIfMissing As Boolean = True) As String
            Try
                Dim Element() As String = {}
                Dim ElementCount As Integer
                Dim ElementPointer As Integer
                Dim NameValue() As String
                Dim UcaseQueryName As String
                Dim ElementFound As Boolean
                Dim iAddIfMissing As Boolean
                Dim QueryString As String
                '
                iAddIfMissing = AddIfMissing
                If vbInstr(1, Link, "?") <> 0 Then
                    modifyLinkQuery = Mid(Link, 1, vbInstr(1, Link, "?") - 1)
                    QueryString = Mid(Link, Len(modifyLinkQuery) + 2)
                Else
                    modifyLinkQuery = Link
                    QueryString = ""
                End If
                UcaseQueryName = vbUCase(EncodeRequestVariable(QueryName))
                If QueryString <> "" Then
                    Element = Split(QueryString, "&")
                    ElementCount = UBound(Element) + 1
                    For ElementPointer = 0 To ElementCount - 1
                        NameValue = Split(Element(ElementPointer), "=")
                        If UBound(NameValue) = 1 Then
                            If vbUCase(NameValue(0)) = UcaseQueryName Then
                                If QueryValue = "" Then
                                    Element(ElementPointer) = ""
                                Else
                                    Element(ElementPointer) = QueryName & "=" & QueryValue
                                End If
                                ElementFound = True
                                Exit For
                            End If
                        End If
                    Next
                End If
                If Not ElementFound And (QueryValue <> "") Then
                    '
                    ' element not found, it needs to be added
                    '
                    If iAddIfMissing Then
                        If QueryString = "" Then
                            QueryString = EncodeRequestVariable(QueryName) & "=" & EncodeRequestVariable(QueryValue)
                        Else
                            QueryString = QueryString & "&" & EncodeRequestVariable(QueryName) & "=" & EncodeRequestVariable(QueryValue)
                        End If
                    End If
                Else
                    '
                    ' element found
                    '
                    QueryString = Join(Element, "&")
                    If (QueryString <> "") And (QueryValue = "") Then
                        '
                        ' element found and needs to be removed
                        '
                        QueryString = vbReplace(QueryString, "&&", "&")
                        If Left(QueryString, 1) = "&" Then
                            QueryString = Mid(QueryString, 2)
                        End If
                        If Right(QueryString, 1) = "&" Then
                            QueryString = Mid(QueryString, 1, Len(QueryString) - 1)
                        End If
                    End If
                End If
                If (QueryString <> "") Then
                    modifyLinkQuery = modifyLinkQuery & "?" & QueryString
                End If
            Catch ex As Exception
                Throw New ApplicationException("Exception in modifyLinkQuery", ex)
            End Try
            '
        End Function
        '    '
        '    '=================================================================================
        '    '
        '    '=================================================================================
        '    '
        '    Public shared Function GetIntegerString(Value As Integer, DigitCount As Integer) As String
        '        If Len(Value) <= DigitCount Then
        '        GetIntegerString = String(DigitCount - Len(CStr(Value)), "0") & CStr(Value)
        '        Else
        '            GetIntegerString = CStr(Value)
        '        End If
        '    End Function
        '    '
        '    '==========================================================================================
        '    '   the current process to a high priority
        '    '       Should be called once from the objects parent when it is first created.
        '    '
        '    '   taken from an example labeled
        '    '       KPD-Team 2000
        '    '       URL: http://www.allapi.net/
        '    '       Email: KPDTeam@Allapi.net
        '    '==========================================================================================
        '    '
        '    Public shared sub SetProcessHighPriority()
        '        Dim hProcess As Integer
        '        '
        '        'set the new priority class
        '        '
        '        hProcess = GetCurrentProcess
        '        Call SetPriorityClass(hProcess, HIGH_PRIORITY_CLASS)
        '        '
        '    End Sub
        ''
        ''==========================================================================================
        ''   Format the current error object into a standard string
        ''==========================================================================================
        ''
        'Public shared Function GetErrString(Optional ErrorObject As Object) As String
        '    Dim Copy As String
        '    If ErrorObject Is Nothing Then
        '        If Err.Number = 0 Then
        '            GetErrString = "[no error]"
        '        Else
        '            Copy = Err.Description
        '            Copy = vbReplace(Copy, vbCrLf, "-")
        '            Copy = vbReplace(Copy, vbLf, "-")
        '            Copy = vbReplace(Copy, vbCrLf, "")
        '            GetErrString = "[" & Err.Source & " #" & Err.Number & ", " & Copy & "]"
        '        End If
        '    Else
        '        If ErrorObject.Number = 0 Then
        '            GetErrString = "[no error]"
        '        Else
        '            Copy = ErrorObject.Description
        '            Copy = vbReplace(Copy, vbCrLf, "-")
        '            Copy = vbReplace(Copy, vbLf, "-")
        '            Copy = vbReplace(Copy, vbCrLf, "")
        '            GetErrString = "[" & ErrorObject.Source & " #" & ErrorObject.Number & ", " & Copy & "]"
        '        End If
        '    End If
        '    '
        'End Function
        '    '
        '    '==========================================================================================
        '    '   Format the current error object into a standard string
        '    '==========================================================================================
        '    '
        '    Public shared Function GetProcessID() As Integer
        '        GetProcessID = GetCurrentProcessId
        '    End Function
        '    '
        '    '==========================================================================================
        '    '   Test if a test string is in a delimited string
        '    '==========================================================================================
        '    '
        '    Public shared Function genericController.IsInDelimitedString(DelimitedString As String, TestString As String, Delimiter As String) As Boolean
        '        IsInDelimitedString = (0 <> vbInstr(1, Delimiter & DelimitedString & Delimiter, Delimiter & TestString & Delimiter, vbTextCompare))
        '    End Function
        '    '
        '    '========================================================================
        '    ' encodeURL
        '    '
        '    '   Encodes only what is to the left of the first ?
        '    '   All URL path characters are assumed to be correct (/:#)
        '    '========================================================================
        '    '
        '    Function encodeURL(Source As String) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim URLSplit() As String
        '        Dim LeftSide As String
        '        Dim RightSide As String
        '        '
        '        encodeURL = Source
        '        If Source <> "" Then
        '            URLSplit = Split(Source, "?")
        '            encodeURL = URLSplit(0)
        '            encodeURL = vbReplace(encodeURL, "%", "%25")
        '            '
        '            encodeURL = vbReplace(encodeURL, """", "%22")
        '            encodeURL = vbReplace(encodeURL, " ", "%20")
        '            encodeURL = vbReplace(encodeURL, "$", "%24")
        '            encodeURL = vbReplace(encodeURL, "+", "%2B")
        '            encodeURL = vbReplace(encodeURL, ",", "%2C")
        '            encodeURL = vbReplace(encodeURL, ";", "%3B")
        '            encodeURL = vbReplace(encodeURL, "<", "%3C")
        '            encodeURL = vbReplace(encodeURL, "=", "%3D")
        '            encodeURL = vbReplace(encodeURL, ">", "%3E")
        '            encodeURL = vbReplace(encodeURL, "@", "%40")
        '            If UBound(URLSplit) > 0 Then
        '                encodeURL = encodeURL & "?" & encodeQueryString(URLSplit(1))
        '            End If
        '        End If
        '        '
        '    End Function
        '    '
        '    '========================================================================
        '    ' encodeQueryString
        '    '
        '    '   This routine encodes the URL QueryString to conform to rules
        '    '========================================================================
        '    '
        '    Function encodeQueryString(Source As String) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim QSSplit() As String
        '        Dim QSPointer As Integer
        '        Dim NVSplit() As String
        '        Dim NV As String
        '        '
        '        encodeQueryString = ""
        '        If Source <> "" Then
        '            QSSplit = Split(Source, "&")
        '            For QSPointer = 0 To UBound(QSSplit)
        '                NV = QSSplit(QSPointer)
        '                If NV <> "" Then
        '                    NVSplit = Split(NV, "=")
        '                    If UBound(NVSplit) = 0 Then
        '                        NVSplit(0) = encodeRequestVariable(NVSplit(0))
        '                        encodeQueryString = encodeQueryString & "&" & NVSplit(0)
        '                    Else
        '                        NVSplit(0) = encodeRequestVariable(NVSplit(0))
        '                        NVSplit(1) = encodeRequestVariable(NVSplit(1))
        '                        encodeQueryString = encodeQueryString & "&" & NVSplit(0) & "=" & NVSplit(1)
        '                    End If
        '                End If
        '            Next
        '            If encodeQueryString <> "" Then
        '                encodeQueryString = Mid(encodeQueryString, 2)
        '            End If
        '        End If
        '        '
        '    End Function
        '    '
        '    '========================================================================
        '    ' encodeRequestVariable
        '    '
        '    '   This routine encodes a request variable for a URL Query String
        '    '       ...can be the requestname or the requestvalue
        '    '========================================================================
        '    '
        '    Function encodeRequestVariable(Source As String) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim SourcePointer As Integer
        '        Dim Character As String
        '        Dim LocalSource As String
        '        '
        '        If Source <> "" Then
        '            LocalSource = Source
        '            ' "+" is an allowed character for filenames. If you add it, the wrong file will be looked up
        '            'LocalSource = vbReplace(LocalSource, " ", "+")
        '            For SourcePointer = 1 To Len(LocalSource)
        '                Character = Mid(LocalSource, SourcePointer, 1)
        '                ' "%" added so if this is called twice, it will not destroy "%20" values
        '                'If Character = " " Then
        '                '    encodeRequestVariable = encodeRequestVariable & "+"
        '                If vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_!*()", Character, vbTextCompare) <> 0 Then
        '                    'ElseIf vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./:-_!*()", Character, vbTextCompare) <> 0 Then
        '                    'ElseIf vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./:?#-_!~*'()%", Character, vbTextCompare) <> 0 Then
        '                    encodeRequestVariable = encodeRequestVariable & Character
        '                Else
        '                    encodeRequestVariable = encodeRequestVariable & "%" & Hex(Asc(Character))
        '                End If
        '            Next
        '        End If
        '        '
        '    End Function
        '    '
        '    '========================================================================
        '    ' encodeHTML
        '    '
        '    '   Convert all characters that are not allowed in HTML to their Text equivalent
        '    '   in preperation for use on an HTML page
        '    '========================================================================
        '    '
        '    Function encodeHTML(Source As String) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        encodeHTML = Source
        '        encodeHTML = vbReplace(encodeHTML, "&", "&amp;")
        '        encodeHTML = vbReplace(encodeHTML, "<", "&lt;")
        '        encodeHTML = vbReplace(encodeHTML, ">", "&gt;")
        '        encodeHTML = vbReplace(encodeHTML, """", "&quot;")
        '        encodeHTML = vbReplace(encodeHTML, "'", "&#39;")
        '        'encodeHTML = vbReplace(encodeHTML, "'", "&apos;")
        '        '
        '    End Function
        '
        '========================================================================
        ' decodeHtml
        '
        '   Convert HTML equivalent characters to their equivalents
        '========================================================================
        '
        Public Shared Function decodeHtml(ByVal Source As String) As String
            ' ##### removed to catch err<>0 problem on error resume next
            '
            Dim Pos As Integer
            Dim s As String
            Dim CharCodeString As String
            Dim CharCode As Integer
            Dim posEnd As Integer
            '
            ' 11/26/2009 - basically re-wrote it, I commented the old one out below
            '
            decodeHtml = ""
            If Source <> "" Then
                s = Source
                '
                Pos = Len(s)
                Pos = InStrRev(s, "&#", Pos)
                Do While Pos <> 0
                    CharCodeString = ""
                    If Mid(s, Pos + 3, 1) = ";" Then
                        CharCodeString = Mid(s, Pos + 2, 1)
                        posEnd = Pos + 4
                    ElseIf Mid(s, Pos + 4, 1) = ";" Then
                        CharCodeString = Mid(s, Pos + 2, 2)
                        posEnd = Pos + 5
                    ElseIf Mid(s, Pos + 5, 1) = ";" Then
                        CharCodeString = Mid(s, Pos + 2, 3)
                        posEnd = Pos + 6
                    End If
                    If CharCodeString <> "" Then
                        If vbIsNumeric(CharCodeString) Then
                            CharCode = EncodeInteger(CharCodeString)
                            s = Mid(s, 1, Pos - 1) & Chr(CharCode) & Mid(s, posEnd)
                        End If
                    End If
                    '
                    Pos = InStrRev(s, "&#", Pos)
                Loop
                '
                ' replace out all common names (at least the most common for now)
                '
                s = vbReplace(s, "&lt;", "<")
                s = vbReplace(s, "&gt;", ">")
                s = vbReplace(s, "&quot;", """")
                s = vbReplace(s, "&apos;", "'")
                '
                ' Always replace the amp last
                '
                s = vbReplace(s, "&amp;", "&")
                '
                decodeHtml = s
            End If
            ' pre-11/26/2009
            'decodeHtml = Source
            'decodeHtml = vbReplace(decodeHtml, "&amp;", "&")
            'decodeHtml = vbReplace(decodeHtml, "&lt;", "<")
            'decodeHtml = vbReplace(decodeHtml, "&gt;", ">")
            'decodeHtml = vbReplace(decodeHtml, "&quot;", """")
            'decodeHtml = vbReplace(decodeHtml, "&nbsp;", " ")
            '
        End Function
        '    '
        '    '   Indent every line by 1 tab
        '    '
        Public Shared Function htmlIndent(ByVal Source As String, Optional depth As Integer = 1) As String
            Dim posStart As Integer
            Dim posEnd As Integer
            Dim pre As String
            Dim post As String
            Dim target As String
            '
            posStart = vbInstr(1, Source, "<![CDATA[", CompareMethod.Text)
            If posStart = 0 Then
                '
                ' no cdata
                '
                posStart = vbInstr(1, Source, "<textarea", CompareMethod.Text)
                If posStart = 0 Then
                    '
                    ' no textarea
                    '
                    Dim replaceText As String = vbCrLf & New String(CChar(vbTab), (depth + 1))
                    htmlIndent = vbReplace(Source, vbCrLf & vbTab, replaceText)
                Else
                    '
                    ' text area found, isolate it and indent before and after
                    '
                    posEnd = vbInstr(posStart, Source, "</textarea>", CompareMethod.Text)
                    pre = Mid(Source, 1, posStart - 1)
                    If posEnd = 0 Then
                        target = Mid(Source, posStart)
                        post = ""
                    Else
                        target = Mid(Source, posStart, posEnd - posStart + Len("</textarea>"))
                        post = Mid(Source, posEnd + Len("</textarea>"))
                    End If
                    htmlIndent = htmlIndent(pre) & target & htmlIndent(post)
                End If
            Else
                '
                ' cdata found, isolate it and indent before and after
                '
                posEnd = vbInstr(posStart, Source, "]]>", CompareMethod.Text)
                pre = Mid(Source, 1, posStart - 1)
                If posEnd = 0 Then
                    target = Mid(Source, posStart)
                    post = ""
                Else
                    target = Mid(Source, posStart, posEnd - posStart + Len("]]>"))
                    post = Mid(Source, posEnd + 3)
                End If
                htmlIndent = htmlIndent(pre) & target & htmlIndent(post)
            End If
            '    kmaIndent = Source
            '    If vbInstr(1, kmaIndent, "<textarea", vbTextCompare) = 0 Then
            '        kmaIndent = vbReplace(Source, vbCrLf & vbTab, vbCrLf & vbTab & vbTab)
            '    End If
        End Function
        '
        '========================================================================================================
        'Place code in a form module
        'Add a Command button.
        '========================================================================================================
        '
        Public Shared Function kmaByteArrayToString(ByVal Bytes() As Byte) As String
            Return System.Text.UTF8Encoding.ASCII.GetString(Bytes)

            'Dim iUnicode As Integer, i As Integer, j As Integer

            'On Error Resume Next
            'i = UBound(Bytes)

            'If (i < 1) Then
            '    'ANSI, just convert to unicode and return
            '    kmaByteArrayToString = StrConv(Bytes, VbStrConv.vbUnicode)
            '    Exit Function
            'End If
            'i = i + 1

            ''Examine the first two bytes
            'CopyMemory(iUnicode, Bytes(0), 2)

            'If iUnicode = Bytes(0) Then 'Unicode
            '    'Account for terminating null
            '    If (i Mod 2) Then i = i - 1
            '    'Set up a buffer to recieve the string
            '    kmaByteArrayToString = String$(i / 2, 0)
            '    'Copy to string
            '    CopyMemory ByVal StrPtr(kmaByteArrayToString), Bytes(0), i
            'Else 'ANSI
            '    kmaByteArrayToString = StrConv(Bytes, vbUnicode)
            'End If

        End Function
        '    '
        '    '========================================================================================================
        '    '
        '    '========================================================================================================
        '    '
        '    Public shared Function kmaStringToByteArray(strInput As String, _
        '                                    Optional bReturnAsUnicode As Boolean = True, _
        '                                    Optional bAddNullTerminator As Boolean = False) As Byte()

        '        Dim lRet As Integer
        '        Dim bytBuffer() As Byte
        '        Dim lLenB As Integer

        '        If bReturnAsUnicode Then
        '            'Number of bytes
        '            lLenB = LenB(strInput)
        '            'Resize buffer, do we want terminating null?
        '            If bAddNullTerminator Then
        '                ReDim bytBuffer(lLenB)
        '            Else
        '                ReDim bytBuffer(lLenB - 1)
        '            End If
        '            'Copy characters from string to byte array
        '        CopyMemory bytBuffer(0), ByVal StrPtr(strInput), lLenB
        '        Else
        '            'METHOD ONE
        '            '        'Get rid of embedded nulls
        '            '        strRet = StrConv(strInput, vbFromUnicode)
        '            '        lLenB = LenB(strRet)
        '            '        If bAddNullTerminator Then
        '            '            ReDim bytBuffer(lLenB)
        '            '        Else
        '            '            ReDim bytBuffer(lLenB - 1)
        '            '        End If
        '            '        CopyMemory bytBuffer(0), ByVal StrPtr(strInput), lLenB

        '            'METHOD TWO
        '            'Num of characters
        '            lLenB = Len(strInput)
        '            If bAddNullTerminator Then
        '                ReDim bytBuffer(lLenB)
        '            Else
        '                ReDim bytBuffer(lLenB - 1)
        '            End If
        '        lRet = WideCharToMultiByte(CP_ACP, 0&, ByVal StrPtr(strInput), -1, ByVal VarPtr(bytBuffer(0)), lLenB, 0&, 0&)
        '        End If

        '        kmaStringToByteArray = bytBuffer

        '    End Function
        '    '
        '    '========================================================================================================
        '    '   Sample kmaStringToByteArray
        '    '========================================================================================================
        '    '
        '    Private Sub SampleStringToByteArray()
        '        Dim bAnsi() As Byte
        '        Dim bUni() As Byte
        '        Dim str As String
        '        Dim i As Integer
        '        '
        '        str = "Convert"
        '        bAnsi = kmaStringToByteArray(str, False)
        '        bUni = kmaStringToByteArray(str)
        '        '
        '        For i = 0 To UBound(bAnsi)
        '            Debug.Print("=" & bAnsi(i))
        '        Next
        '        '
        '        Debug.Print("========")
        '        '
        '        For i = 0 To UBound(bUni)
        '            Debug.Print("=" & bUni(i))
        '        Next
        '        '
        '        Debug.Print("ANSI= " & kmaByteArrayToString(bAnsi))
        '        Debug.Print("UNICODE= " & kmaByteArrayToString(bUni))
        '        'Using StrConv to convert a Unicode character array directly
        '        'will cause the resultant string to have extra embedded nulls
        '        'reason, StrConv does not know the difference between Unicode and ANSI
        '        Debug.Print("Resull= " & StrConv(bUni, vbUnicode))
        '    End Sub

        '    '======================================================================================
        '    '
        '    '======================================================================================
        '    '
        '    Public shared sub StartDebugTimer(Enabled As Boolean, Label As String)
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        If Enabled Then
        '            If TimerStackCount < TimerStackMax Then
        '                TimerStack(TimerStackCount).Label = Label
        '                TimerStack(TimerStackCount).StartTicks = GetTickCount
        '            Else
        '                Call AppendLogFile("dll" & ".?.StartDebugTimer, " & "Timer Stack overflow, attempting push # [" & TimerStackCount & "], but max = [" & TimerStackMax & "]")
        '            End If
        '            TimerStackCount = TimerStackCount + 1
        '        End If
        '    End Sub
        '    '
        '    Public shared sub StopDebugTimer(Enabled As Boolean, Label As String)
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        If Enabled Then
        '            If TimerStackCount <= 0 Then
        '                Call AppendLogFile("dll" & ".?.StopDebugTimer, " & "Timer Error, attempting to Pop, but the stack is empty")
        '            Else
        '                If TimerStackCount <= TimerStackMax Then
        '                    If TimerStack(TimerStackCount - 1).Label = Label Then
        '                    Call AppendLogFile("dll" & ".?.StopDebugTimer, " & "Timer [" & String(2 * TimerStackCount, ".") & Label & "] took " & (GetTickCount - TimerStack(TimerStackCount - 1).StartTicks) & " msec")
        '                    Else
        '                        Call AppendLogFile("dll" & ".?.StopDebugTimer, " & "Timer Error, [" & Label & "] was popped, but [" & TimerStack(TimerStackCount).Label & "] was on the top of the stack")
        '                    End If
        '                End If
        '                TimerStackCount = TimerStackCount - 1
        '            End If
        '        End If
        '    End Sub
        '    '
        '    '
        '    '
        '    Public shared Function PayString(Index) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        Select Case Index
        '            Case PayTypeCreditCardOnline
        '                PayString = "Credit Card"
        '            Case PayTypeCreditCardByPhone
        '                PayString = "Credit Card by phone"
        '            Case PayTypeCreditCardByFax
        '                PayString = "Credit Card by fax"
        '            Case PayTypeCHECK
        '                PayString = "Personal Check"
        '            Case PayTypeCHECKCOMPANY
        '                PayString = "Company Check"
        '            Case PayTypeCREDIT
        '                PayString = "You will be billed"
        '            Case PayTypeNetTerms
        '                PayString = "Net Terms (Approved customers only)"
        '            Case PayTypeCODCompanyCheck
        '                PayString = "COD- Pre-Approved Only"
        '            Case PayTypeCODCertifiedFunds
        '                PayString = "COD- Certified Funds"
        '            Case PayTypePAYPAL
        '                PayString = "PayPal"
        '            Case Else
        '                ' Case PayTypeNONE
        '                PayString = "No payment required"
        '        End Select
        '    End Function
        '    '
        '    '
        '    '
        '    Public shared Function CCString(Index) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        Select Case Index
        '            Case CCTYPEVISA
        '                CCString = "Visa"
        '            Case CCTYPEMC
        '                CCString = "MasterCard"
        '            Case CCTYPEAMEX
        '                CCString = "American Express"
        '            Case CCTYPEDISCOVER
        '                CCString = "Discover"
        '            Case Else
        '                ' Case CCTYPENOVUS
        '                CCString = "Novus Card"
        '        End Select
        '    End Function
        '    '
        '    '========================================================================
        '    ' Get a Long from a CommandPacket
        '    '   position+0, 4 byte value
        '    '========================================================================
        '    '
        '    Public shared Function GetLongFromByteArray(ByteArray() As Byte, Position As Integer) As Integer
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        GetLongFromByteArray = ByteArray(Position + 3)
        '        GetLongFromByteArray = ByteArray(Position + 2) + (256 * GetLongFromByteArray)
        '        GetLongFromByteArray = ByteArray(Position + 1) + (256 * GetLongFromByteArray)
        '        GetLongFromByteArray = ByteArray(Position + 0) + (256 * GetLongFromByteArray)
        '        Position = Position + 4
        '        '
        '    End Function
        '    '
        '    '========================================================================
        '    ' Get a Long from a byte array
        '    '   position+0, 4 byte size of the number
        '    '   position+3, start of the number
        '    '========================================================================
        '    '
        '    Public shared Function GetNumberFromByteArray(ByteArray() As Byte, Position As Integer) As Integer
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim ArgumentCount As Integer
        '        Dim ArgumentLength As Integer
        '        '
        '        ArgumentLength = GetLongFromByteArray(ByteArray(), Position)
        '        '
        '        If ArgumentLength > 0 Then
        '            GetNumberFromByteArray = 0
        '            For ArgumentCount = ArgumentLength - 1 To 0 Step -1
        '                GetNumberFromByteArray = ByteArray(Position + ArgumentCount) + (256 * GetNumberFromByteArray)
        '            Next
        '        End If
        '        Position = Position + ArgumentLength
        '        '
        '    End Function
        '    '
        '    '========================================================================
        '    ' Get a String a byte array
        '    '   position+0, 4 byte length of the string
        '    '   position+3, start of the string
        '    '========================================================================
        '    '
        '    Public shared Function GetStringFromByteArray(ByteArray() As Byte, Position As Integer) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim Pointer As Integer
        '        Dim ArgumentLength As Integer
        '        '
        '        ArgumentLength = GetLongFromByteArray(ByteArray(), Position)
        '        '
        '        GetStringFromByteArray = ""
        '        If ArgumentLength > 0 Then
        '            For Pointer = 0 To ArgumentLength - 1
        '                GetStringFromByteArray = GetStringFromByteArray & chr(ByteArray(Position + Pointer))
        '            Next
        '        End If
        '        Position = Position + ArgumentLength
        '        '
        '    End Function
        '    '
        '    '========================================================================
        '    ' Get a Long from a byte array
        '    '========================================================================
        '    '
        '    Public shared sub SetLongByteArray(ByRef ByteArray() As Byte, Position As Integer, LongValue As Integer)
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        ByteArray(Position + 0) = LongValue And (&HFF)
        '        ByteArray(Position + 1) = Int(LongValue / 256) And (&HFF)
        '        ByteArray(Position + 2) = Int(LongValue / (256 ^ 2)) And (&HFF)
        '        ByteArray(Position + 3) = Int(LongValue / (256 ^ 3)) And (&HFF)
        '        Position = Position + 4
        '        '
        '    End Sub
        '    '
        '    '========================================================================
        '    ' Set a string in a byte array
        '    '========================================================================
        '    '
        '    Public shared sub SetStringByteArray(ByRef ByteArray() As Byte, Position As Integer, StringValue As String)
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim Pointer As Integer
        '        Dim LenStringValue As Integer
        '        '
        '        LenStringValue = Len(StringValue)
        '        If LenStringValue > 0 Then
        '            For Pointer = 0 To LenStringValue - 1
        '                ByteArray(Position + Pointer) = Asc(Mid(StringValue, Pointer + 1, 1)) And (&HFF)
        '            Next
        '            Position = Position + LenStringValue
        '        End If
        '        '
        '    End Sub

        '    '
        '    '========================================================================
        '    '   a Long long on the end of a RMB (Remote Method Block)
        '    '       You determine the position, or it will add it to the end
        '    '========================================================================
        '    '
        'Public shared sub SetRMBLong(ByRef ByteArray() As Byte, LongValue As Integer, Optional Position)
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim Temp As Integer
        '        Dim MyPosition As Integer
        '        Dim ByteArraySize As Integer
        '        '
        '        ' ----- determine the position
        '        '
        '        If Not IsMissing(Position) Then
        '            MyPosition = Position
        '        Else
        '            '
        '            ' ----- Add it to the end, determine length
        '            '
        '            MyPosition = ByteArray(RMBPositionLength + 3)
        '            MyPosition = ByteArray(RMBPositionLength + 2) + (256 * MyPosition)
        '            MyPosition = ByteArray(RMBPositionLength + 1) + (256 * MyPosition)
        '            MyPosition = ByteArray(RMBPositionLength + 0) + (256 * MyPosition)
        '            '
        '            ' ----- adjust size of array if necessary
        '            '
        '            ByteArraySize = UBound(ByteArray)
        '            If ByteArraySize < (MyPosition + 8) Then
        '                ReDim Preserve ByteArray(ByteArraySize + 8)
        '            End If
        '        End If
        '        '
        '        ' ----- set the length
        '        '
        '        'ByteArray(MyPosition + 0) = 4
        '        'ByteArray(MyPosition + 1) = 0
        '        'ByteArray(MyPosition + 2) = 0
        '        'ByteArray(MyPosition + 3) = 0
        '        'MyPosition = MyPosition + 4
        '        '
        '        ' ----- set the value
        '        '
        '        ByteArray(MyPosition + 0) = LongValue And (&HFF)
        '        ByteArray(MyPosition + 1) = Int(LongValue / 256) And (&HFF)
        '        ByteArray(MyPosition + 2) = Int(LongValue / (256 ^ 2)) And (&HFF)
        '        ByteArray(MyPosition + 3) = Int(LongValue / (256 ^ 3)) And (&HFF)
        '        MyPosition = MyPosition + 4
        '        '
        '        If IsMissing(Position) Then
        '            '
        '            ' ----- Adjust the RMB length if length not given
        '            '
        '            ByteArray(RMBPositionLength + 0) = MyPosition And (&HFF)
        '            ByteArray(RMBPositionLength + 1) = Int(MyPosition / 256) And (&HFF)
        '            ByteArray(RMBPositionLength + 2) = Int(MyPosition / (256 ^ 2)) And (&HFF)
        '            ByteArray(RMBPositionLength + 3) = Int(MyPosition / (256 ^ 3)) And (&HFF)
        '        End If
        '        '
        '    End Sub
        '    '
        '    '========================================================================
        '    '   a Long long on the end of a RMB (Remote Method Block)
        '    '       You determine the position, or it will add it to the end
        '    '========================================================================
        '    '
        'Public shared sub SetRMBString(ByRef ByteArray() As Byte, StringValue As String, Optional Position)
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim Temp As Integer
        '        Dim MyPosition As Integer
        '        Dim ByteArraySize As Integer
        '        '
        '        ' ----- determine the position
        '        '
        '        If Not IsMissing(Position) Then
        '            MyPosition = Position
        '        Else
        '            '
        '            ' ----- Add it to the end, determine length
        '            '
        '            MyPosition = ByteArray(RMBPositionLength + 3)
        '            MyPosition = ByteArray(RMBPositionLength + 2) + (256 * MyPosition)
        '            MyPosition = ByteArray(RMBPositionLength + 1) + (256 * MyPosition)
        '            MyPosition = ByteArray(RMBPositionLength + 0) + (256 * MyPosition)
        '            '
        '            ' ----- adjust size of array if necessary
        '            '
        '            ByteArraySize = UBound(ByteArray)
        '            If ByteArraySize < (MyPosition + 8) Then
        '                ReDim Preserve ByteArray(ByteArraySize + 4 + Len(StringValue))
        '            End If
        '        End If
        '        '
        '        ' ----- set the value
        '        '

        '        '
        '        Dim Pointer As Integer
        '        Dim LenStringValue As Integer
        '        '
        '        LenStringValue = Len(StringValue)
        '        If LenStringValue > 0 Then
        '            For Pointer = 0 To LenStringValue - 1
        '                ByteArray(MyPosition + Pointer) = Asc(Mid(StringValue, Pointer + 1, 1)) And (&HFF)
        '            Next
        '            MyPosition = MyPosition + LenStringValue
        '        End If
        '        '
        '        If IsMissing(Position) Then
        '            '
        '            ' ----- Adjust the RMB length if length not given
        '            '
        '            ByteArray(RMBPositionLength + 0) = MyPosition And (&HFF)
        '            ByteArray(RMBPositionLength + 1) = Int(MyPosition / 256) And (&HFF)
        '            ByteArray(RMBPositionLength + 2) = Int(MyPosition / (256 ^ 2)) And (&HFF)
        '            ByteArray(RMBPositionLength + 3) = Int(MyPosition / (256 ^ 3)) And (&HFF)
        '        End If
        '        '
        '    End Sub
        '    '
        '    '========================================================================
        '    '   IsTrue
        '    '       returns true or false depending on the state of the variant input
        '    '========================================================================
        '    '
        '    Function IsTrue(ValueVariant) As Boolean
        '        IsTrue = EncodeBoolean(ValueVariant)
        '    End Function
        '    '
        '    '========================================================================
        '    ' EncodeXML
        '    '
        '    '========================================================================
        '    '
        '    Function EncodeXML(ValueVariant As Object, fieldType As Integer) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim TimeValuething As Single
        '        Dim TimeHours As Integer
        '        Dim TimeMinutes As Integer
        '        Dim TimeSeconds As Integer
        '        '
        '        Select Case fieldType
        '            Case FieldTypeInteger, FieldTypeLookup, FieldTypeRedirect, FieldTypeManyToMany, FieldTypeMemberSelect
        '                If IsNull(ValueVariant) Then
        '                    EncodeXML = "null"
        '                ElseIf ValueVariant = "" Then
        '                    EncodeXML = "null"
        '                ElseIf vbIsNumeric(ValueVariant) Then
        '                    EncodeXML = Int(ValueVariant)
        '                Else
        '                    EncodeXML = "null"
        '                End If
        '            Case FieldTypeBoolean
        '                If IsNull(ValueVariant) Then
        '                    EncodeXML = "0"
        '                ElseIf ValueVariant <> False Then
        '                    EncodeXML = "1"
        '                Else
        '                    EncodeXML = "0"
        '                End If
        '            Case FieldTypeCurrency
        '                If IsNull(ValueVariant) Then
        '                    EncodeXML = "null"
        '                ElseIf ValueVariant = "" Then
        '                    EncodeXML = "null"
        '                ElseIf vbIsNumeric(ValueVariant) Then
        '                    EncodeXML = ValueVariant
        '                Else
        '                    EncodeXML = "null"
        '                End If
        '            Case FieldTypeFloat
        '                If IsNull(ValueVariant) Then
        '                    EncodeXML = "null"
        '                ElseIf ValueVariant = "" Then
        '                    EncodeXML = "null"
        '                ElseIf vbIsNumeric(ValueVariant) Then
        '                    EncodeXML = ValueVariant
        '                Else
        '                    EncodeXML = "null"
        '                End If
        '            Case FieldTypeDate
        '                If IsNull(ValueVariant) Then
        '                    EncodeXML = "null"
        '                ElseIf ValueVariant = "" Then
        '                    EncodeXML = "null"
        '                ElseIf IsDate(ValueVariant) Then
        '                    'TimeVar = CDate(ValueVariant)
        '                    'TimeValuething = 86400! * (TimeVar - Int(TimeVar))
        '                    'TimeHours = Int(TimeValuething / 3600!)
        '                    'TimeMinutes = Int(TimeValuething / 60!) - (TimeHours * 60)
        '                    'TimeSeconds = TimeValuething - (TimeHours * 3600!) - (TimeMinutes * 60!)
        '                    'EncodeXML = Year(ValueVariant) & "-" & Right("0" & Month(ValueVariant), 2) & "-" & Right("0" & Day(ValueVariant), 2) & " " & Right("0" & TimeHours, 2) & ":" & Right("0" & TimeMinutes, 2) & ":" & Right("0" & TimeSeconds, 2)
        '                    EncodeXML = encodeText(ValueVariant)
        '                End If
        '            Case Else
        '                '
        '                ' ----- FieldTypeText
        '                ' ----- FieldTypeLongText
        '                ' ----- FieldTypeFile
        '                ' ----- FieldTypeImage
        '                ' ----- FieldTypeTextFile
        '                ' ----- FieldTypeCSSFile
        '                ' ----- FieldTypeXMLFile
        '                ' ----- FieldTypeJavascriptFile
        '                ' ----- FieldTypeLink
        '                ' ----- FieldTypeResourceLink
        '                ' ----- FieldTypeHTML
        '                ' ----- FieldTypeHTMLFile
        '                '
        '                If IsNull(ValueVariant) Then
        '                    EncodeXML = "null"
        '                ElseIf ValueVariant = "" Then
        '                    EncodeXML = ""
        '                Else
        '                    'EncodeXML = ASPServer.HTMLEncode(ValueVariant)
        '                    'EncodeXML = vbReplace(ValueVariant, "&", "&lt;")
        '                    'EncodeXML = vbReplace(ValueVariant, "<", "&lt;")
        '                    'EncodeXML = vbReplace(EncodeXML, ">", "&gt;")
        '                End If
        '        End Select
        '        '
        '    End Function
        '
        '========================================================================
        ' EncodeFilename
        '
        '========================================================================
        '
        Public Shared Function encodeFilename(ByVal Source As String) As String
            Dim allowed As String
            Dim chr As String
            Dim Ptr As Integer
            Dim Cnt As Integer
            Dim returnString As String
            '
            returnString = ""
            Cnt = Len(Source)
            If Cnt > 254 Then
                Cnt = 254
            End If
            allowed = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ^&'@{}[],$-#()%.+~_"
            For Ptr = 1 To Cnt
                chr = Mid(Source, Ptr, 1)
                If (InStr(1, allowed, chr, vbBinaryCompare) >= 0) Then
                    returnString = returnString & chr
                Else
                    returnString &= "_"
                End If
            Next
            encodeFilename = returnString
        End Function
        '    '
        '    'Function encodeFilename(Filename As String) As String
        '    '    ' ##### removed to catch err<>0 problem on error resume next
        '    '    '
        '    '    Dim Source() as object
        '    '    Dim Replacement() as object
        '    '    '
        '    '    Source = Array("""", "*", "/", ":", "<", ">", "?", "\", "|", "=")
        '    '    Replacement = Array("_", "_", "_", "_", "_", "_", "_", "_", "_", "_")
        '    '    '
        '    '    encodeFilename = ReplaceMany(Filename, Source, Replacement)
        '    '    If Len(encodeFilename) > 254 Then
        '    '        encodeFilename = Left(encodeFilename, 254)
        '    '    End If
        '    '    encodeFilename = vbReplace(encodeFilename, vbCr, "_")
        '    '    encodeFilename = vbReplace(encodeFilename, vbLf, "_")
        '    '    '
        '    '    End Function
        '    '
        '    '
        '    '

        '    '
        '    '========================================================================
        '    ' DecodeHTML
        '    '
        '    '========================================================================
        '    '
        '    Function DecodeHTML(Source As String) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        DecodeHTML = decodeHtml(Source)
        '        'Dim SourceChr() as object
        '        'Dim ReplacementChr() as object
        '        ''
        '        'SourceChr = Array("&gt;", "&lt;", "&nbsp;", "&amp;")
        '        'ReplacementChr = Array(">", "<", " ", "&")
        '        ''
        '        'DecodeHTML = ReplaceMany(Source, SourceChr, ReplacementChr)
        '        '
        '    End Function
        '    '
        '    '========================================================================
        '    ' EncodeFilename
        '    '
        '    '========================================================================
        '    '
        '    Function ReplaceMany(Source As String, ArrayOfSource() As Object, ArrayOfReplacement() As Object) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim Count As Integer
        '        Dim Pointer As Integer
        '        '
        '        Count = UBound(ArrayOfSource) + 1
        '        ReplaceMany = Source
        '        For Pointer = 0 To Count - 1
        '            ReplaceMany = vbReplace(ReplaceMany, ArrayOfSource(Pointer), ArrayOfReplacement(Pointer))
        '        Next
        '        '
        '    End Function
        '    '
        '    '
        '    '
        '    Public shared Function GetURIHost(URI) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        '   Divide the URI into URIHost, URIPath, and URIPage
        '        '
        '        Dim URIWorking As String
        '        Dim Slash As Integer
        '        Dim LastSlash As Integer
        '        Dim URIHost As String
        '        Dim URIPath As String
        '        Dim URIPage As String
        '        URIWorking = URI
        '        If Mid(vbUCase(URIWorking), 1, 4) = "HTTP" Then
        '            URIWorking = Mid(URIWorking, vbInstr(1, URIWorking, "//") + 2)
        '        End If
        '        URIHost = URIWorking
        '        Slash = vbInstr(1, URIHost, "/")
        '        If Slash = 0 Then
        '            URIPath = "/"
        '            URIPage = ""
        '        Else
        '            URIPath = Mid(URIHost, Slash)
        '            URIHost = Mid(URIHost, 1, Slash - 1)
        '            Slash = vbInstr(1, URIPath, "/")
        '            Do While Slash <> 0
        '                LastSlash = Slash
        '                Slash = vbInstr(LastSlash + 1, URIPath, "/")
        '                '''DoEvents()
        '            Loop
        '            URIPage = Mid(URIPath, LastSlash + 1)
        '            URIPath = Mid(URIPath, 1, LastSlash)
        '        End If
        '        GetURIHost = URIHost
        '        '
        '    End Function
        '    '
        '    '
        '    '
        '    Public shared Function GetURIPage(URI) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        '   Divide the URI into URIHost, URIPath, and URIPage
        '        '
        '        Dim Slash As Integer
        '        Dim LastSlash As Integer
        '        Dim URIHost As String
        '        Dim URIPath As String
        '        Dim URIPage As String
        '        Dim URIWorking As String
        '        URIWorking = URI
        '        If Mid(vbUCase(URIWorking), 1, 4) = "HTTP" Then
        '            URIWorking = Mid(URIWorking, vbInstr(1, URIWorking, "//") + 2)
        '        End If
        '        URIHost = URIWorking
        '        Slash = vbInstr(1, URIHost, "/")
        '        If Slash = 0 Then
        '            URIPath = "/"
        '            URIPage = ""
        '        Else
        '            URIPath = Mid(URIHost, Slash)
        '            URIHost = Mid(URIHost, 1, Slash - 1)
        '            Slash = vbInstr(1, URIPath, "/")
        '            Do While Slash <> 0
        '                LastSlash = Slash
        '                Slash = vbInstr(LastSlash + 1, URIPath, "/")
        '                '''DoEvents()
        '            Loop
        '            URIPage = Mid(URIPath, LastSlash + 1)
        '            URIPath = Mid(URIPath, 1, LastSlash)
        '        End If
        '        GetURIPage = URIPage
        '        '
        '    End Function
        '    '
        '    '
        '    '
        '    Function GetDateFromGMT(GMTDate As String) As Date
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim WorkString As String
        '        GetDateFromGMT = 0
        '        If GMTDate <> "" Then
        '            WorkString = Mid(GMTDate, 6, 11)
        '            If IsDate(WorkString) Then
        '                GetDateFromGMT = CDate(WorkString)
        '                WorkString = Mid(GMTDate, 18, 8)
        '                If IsDate(WorkString) Then
        '                    GetDateFromGMT = GetDateFromGMT + CDate(WorkString) + 4 / 24
        '                End If
        '            End If
        '        End If
        '        '
        '    End Function
        '
        ' Wdy, DD-Mon-YYYY HH:MM:SS GMT
        '
        Public Shared Function GetGMTFromDate(ByVal DateValue As Date) As String
            Dim WorkLong As Integer
            '
            GetGMTFromDate = ""
            If IsDate(DateValue) Then
                Select Case Weekday(DateValue)
                    Case vbSunday
                        GetGMTFromDate = "Sun, "
                    Case vbMonday
                        GetGMTFromDate = "Mon, "
                    Case vbTuesday
                        GetGMTFromDate = "Tue, "
                    Case vbWednesday
                        GetGMTFromDate = "Wed, "
                    Case vbThursday
                        GetGMTFromDate = "Thu, "
                    Case vbFriday
                        GetGMTFromDate = "Fri, "
                    Case vbSaturday
                        GetGMTFromDate = "Sat, "
                End Select
                '
                WorkLong = Day(DateValue)
                If WorkLong < 10 Then
                    GetGMTFromDate = GetGMTFromDate & "0" & CStr(WorkLong) & " "
                Else
                    GetGMTFromDate = GetGMTFromDate & CStr(WorkLong) & " "
                End If
                '
                Select Case Month(DateValue)
                    Case 1
                        GetGMTFromDate = GetGMTFromDate & "Jan "
                    Case 2
                        GetGMTFromDate = GetGMTFromDate & "Feb "
                    Case 3
                        GetGMTFromDate = GetGMTFromDate & "Mar "
                    Case 4
                        GetGMTFromDate = GetGMTFromDate & "Apr "
                    Case 5
                        GetGMTFromDate = GetGMTFromDate & "May "
                    Case 6
                        GetGMTFromDate = GetGMTFromDate & "Jun "
                    Case 7
                        GetGMTFromDate = GetGMTFromDate & "Jul "
                    Case 8
                        GetGMTFromDate = GetGMTFromDate & "Aug "
                    Case 9
                        GetGMTFromDate = GetGMTFromDate & "Sep "
                    Case 10
                        GetGMTFromDate = GetGMTFromDate & "Oct "
                    Case 11
                        GetGMTFromDate = GetGMTFromDate & "Nov "
                    Case 12
                        GetGMTFromDate = GetGMTFromDate & "Dec "
                End Select
                '
                GetGMTFromDate = GetGMTFromDate & CStr(Year(DateValue)) & " "
                '
                WorkLong = Hour(DateValue)
                If WorkLong < 10 Then
                    GetGMTFromDate = GetGMTFromDate & "0" & CStr(WorkLong) & ":"
                Else
                    GetGMTFromDate = GetGMTFromDate & CStr(WorkLong) & ":"
                End If
                '
                WorkLong = Minute(DateValue)
                If WorkLong < 10 Then
                    GetGMTFromDate = GetGMTFromDate & "0" & CStr(WorkLong) & ":"
                Else
                    GetGMTFromDate = GetGMTFromDate & CStr(WorkLong) & ":"
                End If
                '
                WorkLong = Second(DateValue)
                If WorkLong < 10 Then
                    GetGMTFromDate = GetGMTFromDate & "0" & CStr(WorkLong)
                Else
                    GetGMTFromDate = GetGMTFromDate & CStr(WorkLong)
                End If
                '
                GetGMTFromDate = GetGMTFromDate & " GMT"
            End If
            '
        End Function
        ''    '
        ''    '========================================================================
        ''    '   EncodeSQL
        ''    '       encode a variable to go in an sql expression
        ''    '       NOT supported
        ''    '========================================================================
        ''    '
        'Public shared Function EncodeSQL(ByVal expression As Object, Optional ByVal fieldType As Integer = FieldTypeIdText) As String
        '    ' ##### removed to catch err<>0 problem on error resume next
        '    '
        '    Dim iFieldType As Integer
        '    Dim MethodName As String
        '    '
        '    MethodName = "EncodeSQL"
        '    '
        '    iFieldType = fieldType
        '    Select Case iFieldType
        '        Case FieldTypeIdBoolean
        '            EncodeSQL = app.EncodeSQLBoolean(expression)
        '        Case FieldTypeIdCurrency, FieldTypeIdAutoIdIncrement, FieldTypeIdFloat, FieldTypeIdInteger, FieldTypeIdLookup, FieldTypeIdMemberSelect
        '            EncodeSQL = app.EncodeSQLNumber(expression)
        '        Case FieldTypeIdDate
        '            EncodeSQL = app.EncodeSQLDate(expression)
        '        Case FieldTypeIdLongText, FieldTypeIdHTML
        '            EncodeSQL = app.EncodeSQLText(expression)
        '        Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdRedirect, FieldTypeIdManyToMany, FieldTypeIdText, FieldTypeIdFileTextPrivate, FieldTypeIdFileJavascript, FieldTypeIdFileXML, FieldTypeIdFileCSS, FieldTypeIdFileHTMLPrivate
        '            EncodeSQL = app.EncodeSQLText(expression)
        '        Case Else
        '            EncodeSQL = app.EncodeSQLText(expression)
        '            On Error GoTo 0
        '            fixme-- cpCore.handleException(New ApplicationException("")) ' -----ignoreInteger, "dll", "Unknown Field Type [" & fieldType & "] used FieldTypeText.")
        '    End Select
        '    '
        'End Function
        ''
        ''=====================================================================================================
        ''   a value in a name/value pair
        ''=====================================================================================================
        ''
        'Public shared sub SetNameValueArrays(ByVal InputName As String, ByVal InputValue As String, ByRef SQLName() As String, ByRef SQLValue() As String, ByRef Index As Integer)
        '    ' ##### removed to catch err<>0 problem on error resume next
        '    '
        '    SQLName(Index) = InputName
        '    SQLValue(Index) = InputValue
        '    Index = Index + 1
        '    '
        'End Sub
        '    '
        '    '
        '    '
        Public Shared Function GetApplicationStatusMessage(ByVal ApplicationStatus As Models.Entity.serverConfigModel.appStatusEnum) As String

            Select Case ApplicationStatus
                'Case Models.Entity.serverConfigModel.appStatusEnum.notFound
                '    GetApplicationStatusMessage = "Application not found"
                'Case Models.Entity.serverConfigModel.appStatusEnum.notEnabled
                '    GetApplicationStatusMessage = "Application not enabled"
                'Case Models.Entity.serverConfigModel.appStatusEnum.errorDbBad
                '    GetApplicationStatusMessage = "Error verifying core database records"
                'Case Models.Entity.serverConfigModel.appStatusEnum.errorDbNotFound
                '    GetApplicationStatusMessage = "Error opening application database"
                'Case Models.Entity.serverConfigModel.appStatusEnum.errorDbFoundButContentMetaMissing
                '    GetApplicationStatusMessage = "The database for this application was found, but content meta table could not be read."
                'Case Models.Entity.serverConfigModel.appStatusEnum.errorAppConfigNotValid
                '    GetApplicationStatusMessage = "The application configuration file on this front-end server is not valid."
                'Case Models.Entity.serverConfigModel.appStatusEnum.errorAppConfigNotFound
                '    GetApplicationStatusMessage = "The application configuration file was not be found on this front-end server."
                'Case Models.Entity.serverConfigModel.appStatusEnum.errorNoHostService
                '    GetApplicationStatusMessage = "Contensive server not running"
                'Case Models.Entity.serverConfigModel.appStatusEnum.errorKernelFailure
                '    GetApplicationStatusMessage = "Error contacting Contensive kernel services"
                'Case Models.Entity.serverConfigModel.appStatusEnum.errorLicenseFailure
                '    GetApplicationStatusMessage = "Error verifying Contensive site license, see Http://www.Contensive.com/License"
                'Case Models.Entity.serverConfigModel.appStatusEnum.errorConnectionObjectFailure
                '    GetApplicationStatusMessage = "Error creating ODBC Connection object"
                'Case Models.Entity.serverConfigModel.appStatusEnum.errorConnectionStringFailure
                '    GetApplicationStatusMessage = "ODBC Data Source connection failed"
                'Case Models.Entity.serverConfigModel.appStatusEnum.errorDataSourceFailure
                '    GetApplicationStatusMessage = "Error opening default data source"
                'Case Models.Entity.serverConfigModel.appStatusEnum.errorDuplicateDomains
                '    GetApplicationStatusMessage = "Can not determine application because there are multiple applications with domain names that match this site's domain (See Application Manager)"
                'Case Models.Entity.serverConfigModel.appStatusEnum.errorFailedToInitialize
                '    GetApplicationStatusMessage = "Application failed to initialize, see trace log for details"
                '    'Case Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusPaused
                '    '    GetApplicationStatusMessage = "Contensive application paused"
                Case Models.Entity.serverConfigModel.appStatusEnum.OK
                    GetApplicationStatusMessage = "Application OK"
                Case Models.Entity.serverConfigModel.appStatusEnum.building
                    GetApplicationStatusMessage = "Application building"
                Case Else
                    GetApplicationStatusMessage = "Unknown status code [" & ApplicationStatus & "], see trace log for details"
            End Select
        End Function
        '    '
        '    '
        '    '
        '    Public shared Function GetFormInputSelectNameValue(SelectName As String, NameValueArray() As NameValuePairType) As String
        '        Dim Pointer As Integer
        '        Dim Source() As NameValuePairType
        '        '
        '        Source = NameValueArray
        '        GetFormInputSelectNameValue = "<SELECT name=""" & SelectName & """ Size=""1"">"
        '        For Pointer = 0 To UBound(NameValueArray)
        '            GetFormInputSelectNameValue = GetFormInputSelectNameValue & "<OPTION value=""" & Source(Pointer).Value & """>" & Source(Pointer).Name & "</OPTION>"
        '        Next
        '        GetFormInputSelectNameValue = GetFormInputSelectNameValue & "</SELECT>"
        '    End Function
        '
        '
        '
        Public Shared Function getSpacer(ByVal Width As Integer, ByVal Height As Integer) As String
            getSpacer = "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""" & Width & """ height=""" & Height & """ border=""0"">"
        End Function
        '
        '
        '
        Public Shared Function processReplacement(ByVal NameValueLines As Object, ByVal Source As Object) As String
            '
            Dim iNameValueLines As String
            Dim Lines() As String
            Dim LinePtr As Integer
            Dim Names() As String = {}
            Dim Values() As String = {}
            Dim PairPtr As Integer
            Dim PairCnt As Integer
            Dim Splits() As String
            '
            ' ----- read pairs in from NameValueLines
            '
            iNameValueLines = encodeText(NameValueLines)
            If vbInstr(1, iNameValueLines, "=") <> 0 Then
                PairCnt = 0
                Lines = SplitCRLF(iNameValueLines)
                For LinePtr = 0 To UBound(Lines)
                    If vbInstr(1, Lines(LinePtr), "=") <> 0 Then
                        Splits = Split(Lines(LinePtr), "=")
                        ReDim Preserve Names(PairCnt)
                        ReDim Preserve Names(PairCnt)
                        ReDim Preserve Values(PairCnt)
                        Names(PairCnt) = Trim(Splits(0))
                        Names(PairCnt) = vbReplace(Names(PairCnt), vbTab, "")
                        Splits(0) = ""
                        Values(PairCnt) = Trim(Splits(1))
                        PairCnt = PairCnt + 1
                    End If
                Next
            End If
            '
            ' ----- Process replacements on Source
            '
            processReplacement = encodeText(Source)
            If PairCnt > 0 Then
                For PairPtr = 0 To PairCnt - 1
                    processReplacement = vbReplace(processReplacement, Names(PairPtr), Values(PairPtr), 1, 99, vbTextCompare)
                Next
            End If
            '
        End Function
        Public Shared Function ConvertLinksToAbsolute(ByVal Source As String, ByVal RootLink As String) As String
            Dim result As String = Source
            Try
                result = vbReplace(result, " href=""", " href=""/", 1, 99, vbTextCompare)
                result = vbReplace(result, " href=""/http", " href=""http", 1, 99, vbTextCompare)
                result = vbReplace(result, " href=""/mailto", " href=""mailto", 1, 99, vbTextCompare)
                result = vbReplace(result, " href=""//", " href=""" & RootLink, 1, 99, vbTextCompare)
                result = vbReplace(result, " href=""/?", " href=""" & RootLink & "?", 1, 99, vbTextCompare)
                result = vbReplace(result, " href=""/", " href=""" & RootLink, 1, 99, vbTextCompare)
                '
                result = vbReplace(result, " href=", " href=/", 1, 99, vbTextCompare)
                result = vbReplace(result, " href=/""", " href=""", 1, 99, vbTextCompare)
                result = vbReplace(result, " href=/http", " href=http", 1, 99, vbTextCompare)
                result = vbReplace(result, " href=//", " href=" & RootLink, 1, 99, vbTextCompare)
                result = vbReplace(result, " href=/?", " href=" & RootLink & "?", 1, 99, vbTextCompare)
                result = vbReplace(result, " href=/", " href=" & RootLink, 1, 99, vbTextCompare)
                '
                result = vbReplace(result, " src=""", " src=""/", 1, 99, vbTextCompare)
                result = vbReplace(result, " src=""/http", " src=""http", 1, 99, vbTextCompare)
                result = vbReplace(result, " src=""//", " src=""" & RootLink, 1, 99, vbTextCompare)
                result = vbReplace(result, " src=""/?", " src=""" & RootLink & "?", 1, 99, vbTextCompare)
                result = vbReplace(result, " src=""/", " src=""" & RootLink, 1, 99, vbTextCompare)
                '
                result = vbReplace(result, " src=", " src=/", 1, 99, vbTextCompare)
                result = vbReplace(result, " src=/""", " src=""", 1, 99, vbTextCompare)
                result = vbReplace(result, " src=/http", " src=http", 1, 99, vbTextCompare)
                result = vbReplace(result, " src=//", " src=" & RootLink, 1, 99, vbTextCompare)
                result = vbReplace(result, " src=/?", " src=" & RootLink & "?", 1, 99, vbTextCompare)
                result = vbReplace(result, " src=/", " src=" & RootLink, 1, 99, vbTextCompare)
            Catch ex As Exception
                Throw New ApplicationException("Error in ConvertLinksToAbsolute")
            End Try
            Return result
        End Function
        ''
        ''
        ''
        'Public shared Function GetAddonRootPath() As String
        '    Dim testPath As String
        '    '
        '    GetAddonRootPath = getAppPath & "\addons"
        '    If vbInstr(1, GetAddonRootPath, "\github\", vbTextCompare) <> 0 Then
        '        '
        '        ' debugging - change program path to dummy path so addon builds all copy to
        '        '
        '        testPath = Environ$("programfiles(x86)")
        '        If testPath = "" Then
        '            testPath = Environ$("programfiles")
        '        End If
        '        GetAddonRootPath = testPath & "\kma\contensive\addons"
        '    End If
        'End Function
        '
        '
        '
        Public Shared Function GetHTMLComment(ByVal Comment As String) As String
            GetHTMLComment = "<!-- " & Comment & " -->"
        End Function
        '
        '
        '
        Public Shared Function SplitCRLF(ByVal Expression As String) As String()
            '
            If vbInstr(1, Expression, vbCrLf) <> 0 Then
                SplitCRLF = Split(Expression, vbCrLf, , vbTextCompare)
            ElseIf vbInstr(1, Expression, vbCr) <> 0 Then
                SplitCRLF = Split(Expression, vbCr, , vbTextCompare)
            ElseIf vbInstr(1, Expression, vbLf) <> 0 Then
                SplitCRLF = Split(Expression, vbLf, , vbTextCompare)
            Else
                ReDim SplitCRLF(0)
                SplitCRLF = Split(Expression, vbCrLf)
            End If
        End Function
        '    '
        '    '
        '    '
        'Public shared sub runProcess(cp.core,Cmd As String, Optional ByVal eWindowStyle As VBA.VbAppWinStyle = vbHide, Optional WaitForReturn As Boolean)
        '        On Error GoTo ErrorTrap
        '        '
        '        Dim ShellObj As Object
        '        '
        '        ShellObj = CreateObject("WScript.Shell")
        '        If Not (ShellObj Is Nothing) Then
        '            Call ShellObj.Run(Cmd, 0, WaitForReturn)
        '        End If
        '        ShellObj = Nothing
        '        Exit Sub
        '        '
        'ErrorTrap:
        '        Call AppendLogFile("ErrorTrap, runProcess running command [" & Cmd & "], WaitForReturn=" & WaitForReturn & ", err=" & GetErrString(Err))
        '    End Sub
        '
        '------------------------------------------------------------------------------------------------------------
        '   Encodes an argument in an Addon OptionString (QueryString) for all non-allowed characters
        '       call this before parsing them together
        '       call decodeAddonConstructorArgument after parsing them apart
        '
        '       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=VAlue[Option1|Option2]
        '
        '       This routine is needed for all Arg, Name, Value, Option values
        '
        '------------------------------------------------------------------------------------------------------------
        '
        Public Shared Function EncodeAddonConstructorArgument(ByVal Arg As String) As String
            Dim a As String = Arg
            a = vbReplace(a, "\", "\\")
            a = vbReplace(a, vbCrLf, "\n")
            a = vbReplace(a, vbTab, "\t")
            a = vbReplace(a, "&", "\&")
            a = vbReplace(a, "=", "\=")
            a = vbReplace(a, ",", "\,")
            a = vbReplace(a, """", "\""")
            a = vbReplace(a, "'", "\'")
            a = vbReplace(a, "|", "\|")
            a = vbReplace(a, "[", "\[")
            a = vbReplace(a, "]", "\]")
            a = vbReplace(a, ":", "\:")
            Return a
        End Function
        '
        '------------------------------------------------------------------------------------------------------------
        '   Decodes an argument parsed from an AddonConstructorString for all non-allowed characters
        '       AddonConstructorString is a & delimited string of name=value[selector]descriptor
        '
        '       to get a value from an AddonConstructorString, first use getargument() to get the correct value[selector]descriptor
        '       then remove everything to the right of any '['
        '
        '       call encodeAddonConstructorargument before parsing them together
        '       call decodeAddonConstructorArgument after parsing them apart
        '
        '       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=VAlue[Option1|Option2]
        '
        '       This routine is needed for all Arg, Name, Value, Option values
        '
        '------------------------------------------------------------------------------------------------------------
        '
        Public Shared Function DecodeAddonConstructorArgument(ByVal EncodedArg As String) As String
            Dim a As String
            '
            a = EncodedArg
            a = vbReplace(a, "\:", ":")
            a = vbReplace(a, "\]", "]")
            a = vbReplace(a, "\[", "[")
            a = vbReplace(a, "\|", "|")
            a = vbReplace(a, "\'", "'")
            a = vbReplace(a, "\""", """")
            a = vbReplace(a, "\,", ",")
            a = vbReplace(a, "\=", "=")
            a = vbReplace(a, "\&", "&")
            a = vbReplace(a, "\t", vbTab)
            a = vbReplace(a, "\n", vbCrLf)
            a = vbReplace(a, "\\", "\")
            Return a
        End Function
        '    '
        '    ' returns true of the link is a valid link on the source host
        '    '
        Public Shared Function IsLinkToThisHost(ByVal Host As String, ByVal Link As String) As Boolean
            Dim result As Boolean = False
            Try
                Dim LinkHost As String
                Dim Pos As Integer
                '
                If Trim(Link) = "" Then
                    '
                    ' Blank is not a link
                    '
                    IsLinkToThisHost = False
                ElseIf vbInstr(1, Link, "://") <> 0 Then
                    '
                    ' includes protocol, may be link to another site
                    '
                    LinkHost = vbLCase(Link)
                    Pos = 1
                    Pos = vbInstr(Pos, LinkHost, "://")
                    If Pos > 0 Then
                        Pos = vbInstr(Pos + 3, LinkHost, "/")
                        If Pos > 0 Then
                            LinkHost = Mid(LinkHost, 1, Pos - 1)
                        End If
                        IsLinkToThisHost = (vbLCase(Host) = LinkHost)
                        If Not IsLinkToThisHost Then
                            '
                            ' try combinations including/excluding www.
                            '
                            If vbInstr(1, LinkHost, "www.", vbTextCompare) <> 0 Then
                                '
                                ' remove it
                                '
                                LinkHost = vbReplace(LinkHost, "www.", "", 1, 99, vbTextCompare)
                                IsLinkToThisHost = (vbLCase(Host) = LinkHost)
                            Else
                                '
                                ' add it
                                '
                                LinkHost = vbReplace(LinkHost, "://", "://www.", 1, 99, vbTextCompare)
                                IsLinkToThisHost = (vbLCase(Host) = LinkHost)
                            End If
                        End If
                    End If
                ElseIf vbInstr(1, Link, "#") = 1 Then
                    '
                    ' Is a bookmark, not a link
                    '
                    IsLinkToThisHost = False
                Else
                    '
                    ' all others are links on the source
                    '
                    IsLinkToThisHost = True
                End If
                If Not IsLinkToThisHost Then
                    Link = Link
                End If
            Catch ex As Exception
                Throw
            End Try
            Return result
        End Function
        '
        '========================================================================================================
        '   ConvertLinkToRootRelative
        '
        '   /images/logo-cmc.main_jpg with any Basepath to /images/logo-cmc.main_jpg
        '   http://gcm.brandeveolve.com/images/logo-cmc.main_jpg with any BasePath  to /images/logo-cmc.main_jpg
        '   images/logo-cmc.main_jpg with Basepath '/' to /images/logo-cmc.main_jpg
        '   logo-cmc.main_jpg with Basepath '/images/' to /images/logo-cmc.main_jpg
        '
        '========================================================================================================
        '
        Public Shared Function ConvertLinkToRootRelative(ByVal Link As String, ByVal BasePath As String) As String
            '
            Dim Pos As Integer
            '
            ConvertLinkToRootRelative = Link
            If vbInstr(1, Link, "/") = 1 Then
                '
                '   case /images/logo-cmc.main_jpg with any Basepath to /images/logo-cmc.main_jpg
                '
            ElseIf vbInstr(1, Link, "://") <> 0 Then
                '
                '   case http://gcm.brandeveolve.com/images/logo-cmc.main_jpg with any BasePath  to /images/logo-cmc.main_jpg
                '
                Pos = vbInstr(1, Link, "://")
                If Pos > 0 Then
                    Pos = vbInstr(Pos + 3, Link, "/")
                    If Pos > 0 Then
                        ConvertLinkToRootRelative = Mid(Link, Pos)
                    Else
                        '
                        ' This is just the domain name, RootRelative is the root
                        '
                        ConvertLinkToRootRelative = "/"
                    End If
                End If
            Else
                '
                '   case images/logo-cmc.main_jpg with Basepath '/' to /images/logo-cmc.main_jpg
                '   case logo-cmc.main_jpg with Basepath '/images/' to /images/logo-cmc.main_jpg
                '
                ConvertLinkToRootRelative = BasePath & Link
            End If
            '
        End Function
        '
        '
        '
        Public Shared Function GetAddonIconImg(ByVal AdminURL As String, ByVal IconWidth As Integer, ByVal IconHeight As Integer, ByVal IconSprites As Integer, ByVal IconIsInline As Boolean, ByVal IconImgID As String, ByVal IconFilename As String, ByVal serverFilePath As String, ByVal IconAlt As String, ByVal IconTitle As String, ByVal ACInstanceID As String, ByVal IconSpriteColumn As Integer) As String
            '
            If IconAlt = "" Then
                IconAlt = "Add-on"
            End If
            If IconTitle = "" Then
                IconTitle = "Rendered as Add-on"
            End If
            If IconFilename = "" Then
                '
                ' No icon given, use the default
                '
                If IconIsInline Then
                    IconFilename = "/ccLib/images/IconAddonInlineDefault.png"
                    IconWidth = 62
                    IconHeight = 17
                    IconSprites = 0
                Else
                    IconFilename = "/ccLib/images/IconAddonBlockDefault.png"
                    IconWidth = 57
                    IconHeight = 59
                    IconSprites = 4
                End If
            ElseIf vbInstr(1, IconFilename, "://") <> 0 Then
                '
                ' icon is an Absolute URL - leave it
                '
            ElseIf Left(IconFilename, 1) = "/" Then
                '
                ' icon is Root Relative, leave it
                '
            Else
                '
                ' icon is a virtual file, add the serverfilepath
                '
                IconFilename = serverFilePath & IconFilename
            End If
            'IconFilename = encodeJavascript(IconFilename)
            If (IconWidth = 0) Or (IconHeight = 0) Then
                IconSprites = 0
            End If

            If IconSprites = 0 Then
                '
                ' just the icon
                '
                GetAddonIconImg = "<img" _
                    & " border=0" _
                    & " id=""" & IconImgID & """" _
                    & " onDblClick=""window.parent.OpenAddonPropertyWindow(this,'" & AdminURL & "');""" _
                    & " alt=""" & IconAlt & """" _
                    & " title=""" & IconTitle & """" _
                    & " src=""" & IconFilename & """"
                'GetAddonIconImg = "<img" _
                '    & " id=""AC,AGGREGATEFUNCTION,0," & FieldName & "," & ArgumentList & """" _
                '    & " onDblClick=""window.parent.OpenAddonPropertyWindow(this);""" _
                '    & " alt=""" & IconAlt & """" _
                '    & " title=""" & IconTitle & """" _
                '    & " src=""" & IconFilename & """"
                If IconWidth <> 0 Then
                    GetAddonIconImg = GetAddonIconImg & " width=""" & IconWidth & "px"""
                End If
                If IconHeight <> 0 Then
                    GetAddonIconImg = GetAddonIconImg & " height=""" & IconHeight & "px"""
                End If
                If IconIsInline Then
                    GetAddonIconImg = GetAddonIconImg & " style=""vertical-align:middle;display:inline;"" "
                Else
                    GetAddonIconImg = GetAddonIconImg & " style=""display:block"" "
                End If
                If ACInstanceID <> "" Then
                    GetAddonIconImg = GetAddonIconImg & " ACInstanceID=""" & ACInstanceID & """"
                End If
                GetAddonIconImg = GetAddonIconImg & ">"
            Else
                '
                ' Sprite Icon
                '
                GetAddonIconImg = GetIconSprite(IconImgID, IconSpriteColumn, IconFilename, IconWidth, IconHeight, IconAlt, IconTitle, "window.parent.OpenAddonPropertyWindow(this,'" & AdminURL & "');", IconIsInline, ACInstanceID)
            End If
        End Function
        '
        '
        '
        Public Shared Function ConvertRSTypeToGoogleType(ByVal RecordFieldType As Integer) As String
            Select Case RecordFieldType
                Case 2, 3, 4, 5, 6, 14, 16, 17, 18, 19, 20, 21, 131
                    ConvertRSTypeToGoogleType = "number"
                Case Else
                    ConvertRSTypeToGoogleType = "string"
            End Select
        End Function
        '    '
        '    '
        '    '
        Public Shared Function GetIconSprite(ByVal TagID As String, ByVal SpriteColumn As Integer, ByVal IconSrc As String, ByVal IconWidth As Integer, ByVal IconHeight As Integer, ByVal IconAlt As String, ByVal IconTitle As String, ByVal onDblClick As String, ByVal IconIsInline As Boolean, ByVal ACInstanceID As String) As String
            '
            Dim ImgStyle As String
            '
            GetIconSprite = "<img" _
                & " border=0" _
                & " id=""" & TagID & """" _
                & " onMouseOver=""this.style.backgroundPosition='" & (-1 * SpriteColumn * IconWidth) & "px -" & (2 * IconHeight) & "px';""" _
                & " onMouseOut=""this.style.backgroundPosition='" & (-1 * SpriteColumn * IconWidth) & "px 0px'""" _
                & " onDblClick=""" & onDblClick & """" _
                & " alt=""" & IconAlt & """" _
                & " title=""" & IconTitle & """" _
                & " src=""/ccLib/images/spacer.gif"""
            ImgStyle = "background:url(" & IconSrc & ") " & (-1 * SpriteColumn * IconWidth) & "px 0px no-repeat;"
            ImgStyle = ImgStyle & "width:" & IconWidth & "px;"
            ImgStyle = ImgStyle & "height:" & IconHeight & "px;"
            If IconIsInline Then
                ImgStyle = ImgStyle & "vertical-align:middle;display:inline;"
            Else
                ImgStyle = ImgStyle & "display:block;"
            End If
            If ACInstanceID <> "" Then
                GetIconSprite = GetIconSprite & " ACInstanceID=""" & ACInstanceID & """"
            End If
            GetIconSprite = GetIconSprite & " style=""" & ImgStyle & """>"
        End Function
        '
        '================================================================================================================
        '   Separate a URL into its host, path, page parts
        '================================================================================================================
        '
        Public Shared Sub SeparateURL(ByVal SourceURL As String, ByRef Protocol As String, ByRef Host As String, ByRef Path As String, ByRef Page As String, ByRef QueryString As String)
            '
            '   Divide the URL into URLHost, URLPath, and URLPage
            '
            Dim WorkingURL As String
            Dim Position As Integer
            '
            ' Get Protocol (before the first :)
            '
            WorkingURL = SourceURL
            Position = vbInstr(1, WorkingURL, ":")
            'Position = vbInstr(1, WorkingURL, "://")
            If Position <> 0 Then
                Protocol = Mid(WorkingURL, 1, Position + 2)
                WorkingURL = Mid(WorkingURL, Position + 3)
            End If
            '
            ' compatibility fix
            '
            If vbInstr(1, WorkingURL, "//") = 1 Then
                If Protocol = "" Then
                    Protocol = "http:"
                End If
                Protocol = Protocol & "//"
                WorkingURL = Mid(WorkingURL, 3)
            End If
            '
            ' Get QueryString
            '
            Position = vbInstr(1, WorkingURL, "?")
            If Position > 0 Then
                QueryString = Mid(WorkingURL, Position)
                WorkingURL = Mid(WorkingURL, 1, Position - 1)
            End If
            '
            ' separate host from pathpage
            '
            'iURLHost = WorkingURL
            Position = vbInstr(WorkingURL, "/")
            If (Position = 0) And (Protocol = "") Then
                '
                ' Page without path or host
                '
                Page = WorkingURL
                Path = ""
                Host = ""
            ElseIf (Position = 0) Then
                '
                ' host, without path or page
                '
                Page = ""
                Path = "/"
                Host = WorkingURL
            Else
                '
                ' host with a path (at least)
                '
                Path = Mid(WorkingURL, Position)
                Host = Mid(WorkingURL, 1, Position - 1)
                '
                ' separate page from path
                '
                Position = InStrRev(Path, "/")
                If Position = 0 Then
                    '
                    ' no path, just a page
                    '
                    Page = Path
                    Path = "/"
                Else
                    Page = Mid(Path, Position + 1)
                    Path = Mid(Path, 1, Position)
                End If
            End If
        End Sub
        '
        '================================================================================================================
        '   Separate a URL into its host, path, page parts
        '================================================================================================================
        '
        Public Shared Sub ParseURL(ByVal SourceURL As String, ByRef Protocol As String, ByRef Host As String, ByRef Port As String, ByRef Path As String, ByRef Page As String, ByRef QueryString As String)
            '
            '   Divide the URL into URLHost, URLPath, and URLPage
            '
            Dim iURLWorking As String = ""
            Dim iURLProtocol As String = ""
            Dim iURLHost As String = ""
            Dim iURLPort As String = ""
            Dim iURLPath As String = ""
            Dim iURLPage As String = ""
            Dim iURLQueryString As String = ""
            Dim Position As Integer = 0
            '
            iURLWorking = SourceURL
            Position = vbInstr(1, iURLWorking, "://")
            If Position <> 0 Then
                iURLProtocol = Mid(iURLWorking, 1, Position + 2)
                iURLWorking = Mid(iURLWorking, Position + 3)
            End If
            '
            ' separate Host:Port from pathpage
            '
            iURLHost = iURLWorking
            Position = vbInstr(iURLHost, "/")
            If Position = 0 Then
                '
                ' just host, no path or page
                '
                iURLPath = "/"
                iURLPage = ""
            Else
                iURLPath = Mid(iURLHost, Position)
                iURLHost = Mid(iURLHost, 1, Position - 1)
                '
                ' separate page from path
                '
                Position = InStrRev(iURLPath, "/")
                If Position = 0 Then
                    '
                    ' no path, just a page
                    '
                    iURLPage = iURLPath
                    iURLPath = "/"
                Else
                    iURLPage = Mid(iURLPath, Position + 1)
                    iURLPath = Mid(iURLPath, 1, Position)
                End If
            End If
            '
            ' Divide Host from Port
            '
            Position = vbInstr(iURLHost, ":")
            If Position = 0 Then
                '
                ' host not given, take a guess
                '
                Select Case vbUCase(iURLProtocol)
                    Case "FTP://"
                        iURLPort = "21"
                    Case "HTTP://", "HTTPS://"
                        iURLPort = "80"
                    Case Else
                        iURLPort = "80"
                End Select
            Else
                iURLPort = Mid(iURLHost, Position + 1)
                iURLHost = Mid(iURLHost, 1, Position - 1)
            End If
            Position = vbInstr(1, iURLPage, "?")
            If Position > 0 Then
                iURLQueryString = Mid(iURLPage, Position)
                iURLPage = Mid(iURLPage, 1, Position - 1)
            End If
            Protocol = iURLProtocol
            Host = iURLHost
            Port = iURLPort
            Path = iURLPath
            Page = iURLPage
            QueryString = iURLQueryString
        End Sub
        '
        '
        '
        Public Shared Function DecodeGMTDate(ByVal GMTDate As String) As Date
            '
            Dim YearPart As Double
            Dim HourPart As Double
            '
            DecodeGMTDate = #12:00:00 AM#
            If GMTDate <> "" Then
                HourPart = EncodeNumber(Mid(GMTDate, 6, 11))
                If IsDate(HourPart) Then
                    YearPart = EncodeNumber(Mid(GMTDate, 18, 8))
                    If IsDate(YearPart) Then
                        DecodeGMTDate = Date.FromOADate(YearPart + (HourPart + 4) / 24)
                    End If
                End If
            End If
        End Function
        ''
        ''
        ''
        'Public shared Function  EncodeGMTDate(ByVal MSDate As Date) As String
        '    EncodeGMTDate = ""
        'End Function
        ''
        '=================================================================================
        ' Get the value of a name in a string of name value pairs parsed with vrlf and =
        '   the legacy line delimiter was a '&' -> name1=value1&name2=value2"
        '   new format is "name1=value1 crlf name2=value2 crlf ..."
        '   There can be no extra spaces between the delimiter, the name and the "="
        '=================================================================================
        '
        Public Shared Function GetArgument(ByVal Name As String, ByVal ArgumentString As String, Optional ByVal DefaultValue As String = "", Optional ByVal Delimiter As String = "") As String
            '
            Dim WorkingString As String
            Dim iDefaultValue As String
            Dim NameLength As Integer
            Dim ValueStart As Integer
            Dim ValueEnd As Integer
            Dim IsQuoted As Boolean
            '
            ' determine delimiter
            '
            If Delimiter = "" Then
                '
                ' If not explicit
                '
                If vbInstr(1, ArgumentString, vbCrLf) <> 0 Then
                    '
                    ' crlf can only be here if it is the delimiter
                    '
                    Delimiter = vbCrLf
                Else
                    '
                    ' either only one option, or it is the legacy '&' delimit
                    '
                    Delimiter = "&"
                End If
            End If
            iDefaultValue = DefaultValue
            WorkingString = ArgumentString
            GetArgument = iDefaultValue
            If WorkingString <> "" Then
                WorkingString = Delimiter & WorkingString & Delimiter
                ValueStart = vbInstr(1, WorkingString, Delimiter & Name & "=", vbTextCompare)
                If ValueStart <> 0 Then
                    NameLength = Len(Name)
                    ValueStart = ValueStart + Len(Delimiter) + NameLength + 1
                    If Mid(WorkingString, ValueStart, 1) = """" Then
                        IsQuoted = True
                        ValueStart = ValueStart + 1
                    End If
                    If IsQuoted Then
                        ValueEnd = vbInstr(ValueStart, WorkingString, """" & Delimiter)
                    Else
                        ValueEnd = vbInstr(ValueStart, WorkingString, Delimiter)
                    End If
                    If ValueEnd = 0 Then
                        GetArgument = Mid(WorkingString, ValueStart)
                    Else
                        GetArgument = Mid(WorkingString, ValueStart, ValueEnd - ValueStart)
                    End If
                End If
            End If
            '

            Exit Function
            '
            ' ----- ErrorTrap
            '
ErrorTrap:
        End Function
        ''
        ''=================================================================================
        ''   Return the value from a name value pair, parsed with =,&[|].
        ''   For example:
        ''       name=Jay[Jay|Josh|Dwayne]
        ''       the answer is Jay. If a select box is displayed, it is a dropdown of all three
        ''=================================================================================
        ''
        'Public shared Function  GetAggrOption(ByVal Name As String, ByVal SegmentCMDArgs As String) As String
        '    '
        '    Dim Pos As Integer
        '    '
        '    GetAggrOption = GetArgument(Name, SegmentCMDArgs)
        '    '
        '    ' remove the manual select list syntax "answer[choice1|choice2]"
        '    '
        '    Pos = vbInstr(1, GetAggrOption, "[")
        '    If Pos <> 0 Then
        '        GetAggrOption = Left(GetAggrOption, Pos - 1)
        '    End If
        '    '
        '    ' remove any function syntax "answer{selectcontentname RSS Feeds}"
        '    '
        '    Pos = vbInstr(1, GetAggrOption, "{")
        '    If Pos <> 0 Then
        '        GetAggrOption = Left(GetAggrOption, Pos - 1)
        '    End If
        '    '
        'End Function
        ''
        ''=================================================================================
        ''   Compatibility for GetArgument
        ''=================================================================================
        ''
        'Public shared Function  GetNameValue(ByVal Name As String, ByVal ArgumentString As String, Optional ByVal DefaultValue As String = "") As String
        '    getNameValue = GetArgument(Name, ArgumentString, DefaultValue)
        'End Function
        ''
        ''========================================================================
        ''   Gets the next line from a string, and removes the line
        ''========================================================================
        ''
        'Public shared Function GetLine(ByVal Body As String) As String
        '    Dim EOL As String
        '    Dim NextCR As Integer
        '    Dim NextLF As Integer
        '    Dim BOL As Integer
        '    '
        '    NextCR = vbInstr(1, Body, vbCr)
        '    NextLF = vbInstr(1, Body, vbLf)

        '    If NextCR <> 0 Or NextLF <> 0 Then
        '        If NextCR <> 0 Then
        '            If NextLF <> 0 Then
        '                If NextCR < NextLF Then
        '                    EOL = NextCR - 1
        '                    If NextLF = NextCR + 1 Then
        '                        BOL = NextLF + 1
        '                    Else
        '                        BOL = NextCR + 1
        '                    End If

        '                Else
        '                    EOL = NextLF - 1
        '                    BOL = NextLF + 1
        '                End If
        '            Else
        '                EOL = NextCR - 1
        '                BOL = NextCR + 1
        '            End If
        '        Else
        '            EOL = NextLF - 1
        '            BOL = NextLF + 1
        '        End If
        '        GetLine = Mid(Body, 1, EOL)
        '        Body = Mid(Body, BOL)
        '    Else
        '        GetLine = Body
        '        Body = ""
        '    End If
        'End Function
        '
        '=================================================================================
        '   Get a Random Long Value
        '=================================================================================
        '
        Public Shared Function GetRandomInteger() As Integer
            '
            Dim RandomBase As Integer
            Dim RandomLimit As Integer
            '
            RandomBase = CInt((2 ^ 30) - 1)
            RandomLimit = CInt((2 ^ 31) - RandomBase - 1)
            Randomize()
            GetRandomInteger = CInt(RandomBase + (Rnd() * RandomLimit))
            '
        End Function
        '
        '=================================================================================
        ' fix for isDataTableOk
        '=================================================================================
        '
        Public Shared Function isDataTableOk(ByVal dt As DataTable) As Boolean
            Return (dt.Rows.Count > 0)
        End Function
        '
        '=================================================================================
        ' fix for closeRS
        '=================================================================================
        '
        Public Shared Sub closeDataTable(ByVal dt As DataTable)
            ' nothing needed
            'dt.Clear()
            dt.Dispose()
        End Sub
        ''
        ''=============================================================================
        '' Create the part of the sql where clause that is modified by the user
        ''   WorkingQuery is the original querystring to change
        ''   QueryName is the name part of the name pair to change
        ''   If the QueryName is not found in the string, ignore call
        ''=============================================================================
        ''
        'Public shared Function ModifyQueryString(ByVal WorkingQuery As String, ByVal QueryName As String, ByVal QueryValue As String, Optional ByVal AddIfMissing As Boolean = True) As String
        '    '
        '    If vbInstr(1, WorkingQuery, "?") Then
        '        ModifyQueryString = ModifyLinkQueryString(WorkingQuery, QueryName, QueryValue, AddIfMissing)
        '    Else
        '        ModifyQueryString = Mid(ModifyLinkQueryString("?" & WorkingQuery, QueryName, QueryValue, AddIfMissing), 2)
        '    End If
        'End Function
        '
        '=============================================================================
        '   Modify a querystring name/value pair in a Link
        '=============================================================================
        '
        Public Shared Function ModifyLinkQueryString(ByVal Link As String, ByVal QueryName As String, ByVal QueryValue As String, Optional ByVal AddIfMissing As Boolean = True) As String
            '
            Dim Element() As String = Split("", ",")
            Dim ElementCount As Integer
            Dim ElementPointer As Integer
            Dim NameValue() As String
            Dim UcaseQueryName As String
            Dim ElementFound As Boolean
            Dim QueryString As String
            '
            If vbInstr(1, Link, "?") <> 0 Then
                ModifyLinkQueryString = Mid(Link, 1, vbInstr(1, Link, "?") - 1)
                QueryString = Mid(Link, Len(ModifyLinkQueryString) + 2)
            Else
                ModifyLinkQueryString = Link
                QueryString = ""
            End If
            UcaseQueryName = vbUCase(EncodeRequestVariable(QueryName))
            If QueryString <> "" Then
                Element = Split(QueryString, "&")
                ElementCount = UBound(Element) + 1
                For ElementPointer = 0 To ElementCount - 1
                    NameValue = Split(Element(ElementPointer), "=")
                    If UBound(NameValue) = 1 Then
                        If vbUCase(NameValue(0)) = UcaseQueryName Then
                            If QueryValue = "" Then
                                Element(ElementPointer) = ""
                            Else
                                Element(ElementPointer) = QueryName & "=" & QueryValue
                            End If
                            ElementFound = True
                            Exit For
                        End If
                    End If
                Next
            End If
            If Not ElementFound And (QueryValue <> "") Then
                '
                ' element not found, it needs to be added
                '
                If AddIfMissing Then
                    If QueryString = "" Then
                        QueryString = EncodeRequestVariable(QueryName) & "=" & EncodeRequestVariable(QueryValue)
                    Else
                        QueryString = QueryString & "&" & EncodeRequestVariable(QueryName) & "=" & EncodeRequestVariable(QueryValue)
                    End If
                End If
            Else
                '
                ' element found
                '
                QueryString = Join(Element, "&")
                If (QueryString <> "") And (QueryValue = "") Then
                    '
                    ' element found and needs to be removed
                    '
                    QueryString = vbReplace(QueryString, "&&", "&")
                    If Left(QueryString, 1) = "&" Then
                        QueryString = Mid(QueryString, 2)
                    End If
                    If Right(QueryString, 1) = "&" Then
                        QueryString = Mid(QueryString, 1, Len(QueryString) - 1)
                    End If
                End If
            End If
            If (QueryString <> "") Then
                ModifyLinkQueryString = ModifyLinkQueryString & "?" & QueryString
            End If
        End Function
        '
        '=================================================================================
        '
        '=================================================================================
        '
        Public Shared Function GetIntegerString(ByVal Value As Integer, ByVal DigitCount As Integer) As String
            If Len(Value) <= DigitCount Then
                GetIntegerString = New String("0"c, DigitCount - Len(CStr(Value))) & CStr(Value)
            Else
                GetIntegerString = CStr(Value)
            End If
        End Function
        ''
        ''==========================================================================================
        ''   the current process to a high priority
        ''       Should be called once from the objects parent when it is first created.
        ''
        ''   taken from an example labeled
        ''       KPD-Team 2000
        ''       URL: http://www.allapi.net/
        ''       Email: KPDTeam@Allapi.net
        ''==========================================================================================
        ''
        'Public shared sub SetProcessHighPriority()
        '    Dim hProcess As Integer
        '    '
        '    'set the new priority class
        '    '
        '    hProcess = GetCurrentProcess
        '    Call SetPriorityClass(hProcess, HIGH_PRIORITY_CLASS)
        '    '
        'End Sub
        ''
        '==========================================================================================
        '   Format the current error object into a standard string
        '==========================================================================================
        '
        Public Shared Function GetErrString(Optional ByVal ErrorObject As ErrObject = Nothing) As String
            Dim Copy As String
            If ErrorObject Is Nothing Then
                If Err.Number = 0 Then
                    GetErrString = "[no error]"
                Else
                    Copy = Err.Description
                    Copy = vbReplace(Copy, vbCrLf, "-")
                    Copy = vbReplace(Copy, vbLf, "-")
                    Copy = vbReplace(Copy, vbCrLf, "")
                    GetErrString = "[" & Err.Source & " #" & Err.Number & ", " & Copy & "]"
                End If
            Else
                If ErrorObject.Number = 0 Then
                    GetErrString = "[no error]"
                Else
                    Copy = ErrorObject.Description
                    Copy = vbReplace(Copy, vbCrLf, "-")
                    Copy = vbReplace(Copy, vbLf, "-")
                    Copy = vbReplace(Copy, vbCrLf, "")
                    GetErrString = "[" & ErrorObject.Source & " #" & ErrorObject.Number & ", " & Copy & "]"
                End If
            End If
            '
        End Function
        '
        '==========================================================================================
        '   Format the current error object into a standard string
        '==========================================================================================
        '
        Public Shared Function GetProcessID() As Integer
            Dim Instance As Process = Process.GetCurrentProcess()
            '
            GetProcessID = Instance.Id
        End Function
        '
        '==========================================================================================
        '   Test if a test string is in a delimited string
        '==========================================================================================
        '
        Public Shared Function IsInDelimitedString(ByVal DelimitedString As String, ByVal TestString As String, ByVal Delimiter As String) As Boolean
            IsInDelimitedString = (0 <> vbInstr(1, Delimiter & DelimitedString & Delimiter, Delimiter & TestString & Delimiter, vbTextCompare))
        End Function
        '
        '========================================================================
        ' EncodeURL
        '
        '   Encodes only what is to the left of the first ?
        '   All URL path characters are assumed to be correct (/:#)
        '========================================================================
        '
        Public Shared Function EncodeURL(ByVal Source As String) As String
            Return WebUtility.UrlEncode(Source)
            '' ##### removed to catch err<>0 problem on error resume next
            ''
            'Dim URLSplit() As String
            ''Dim LeftSide As String
            ''Dim RightSide As String
            ''
            'EncodeURL = Source
            'If Source <> "" Then
            '    URLSplit = Split(Source, "?")
            '    EncodeURL = URLSplit(0)
            '    EncodeURL = vbReplace(EncodeURL, "%", "%25")
            '    '
            '    EncodeURL = vbReplace(EncodeURL, """", "%22")
            '    EncodeURL = vbReplace(EncodeURL, " ", "%20")
            '    EncodeURL = vbReplace(EncodeURL, "$", "%24")
            '    EncodeURL = vbReplace(EncodeURL, "+", "%2B")
            '    EncodeURL = vbReplace(EncodeURL, ",", "%2C")
            '    EncodeURL = vbReplace(EncodeURL, ";", "%3B")
            '    EncodeURL = vbReplace(EncodeURL, "<", "%3C")
            '    EncodeURL = vbReplace(EncodeURL, "=", "%3D")
            '    EncodeURL = vbReplace(EncodeURL, ">", "%3E")
            '    EncodeURL = vbReplace(EncodeURL, "@", "%40")
            '    If UBound(URLSplit) > 0 Then
            '        EncodeURL = EncodeURL & "?" & EncodeQueryString(URLSplit(1))
            '    End If
            'End If
            '
        End Function
        '
        '========================================================================
        ' EncodeQueryString
        '
        '   This routine encodes the URL QueryString to conform to rules
        '========================================================================
        '
        Public Shared Function EncodeQueryString(ByVal Source As String) As String
            ' ##### removed to catch err<>0 problem on error resume next
            '
            Dim QSSplit() As String
            Dim QSPointer As Integer
            Dim NVSplit() As String
            Dim NV As String
            '
            EncodeQueryString = ""
            If Source <> "" Then
                QSSplit = Split(Source, "&")
                For QSPointer = 0 To UBound(QSSplit)
                    NV = QSSplit(QSPointer)
                    If NV <> "" Then
                        NVSplit = Split(NV, "=")
                        If UBound(NVSplit) = 0 Then
                            NVSplit(0) = EncodeRequestVariable(NVSplit(0))
                            EncodeQueryString = EncodeQueryString & "&" & NVSplit(0)
                        Else
                            NVSplit(0) = EncodeRequestVariable(NVSplit(0))
                            NVSplit(1) = EncodeRequestVariable(NVSplit(1))
                            EncodeQueryString = EncodeQueryString & "&" & NVSplit(0) & "=" & NVSplit(1)
                        End If
                    End If
                Next
                If EncodeQueryString <> "" Then
                    EncodeQueryString = Mid(EncodeQueryString, 2)
                End If
            End If
            '
        End Function
        '
        '========================================================================
        ' EncodeRequestVariable
        '
        '   This routine encodes a request variable for a URL Query String
        '       ...can be the requestname or the requestvalue
        '========================================================================
        '
        Public Shared Function EncodeRequestVariable(ByVal Source As String) As String
            If (Source Is Nothing) Then
                Return ""
            Else
                Return System.Uri.EscapeDataString(Source)
            End If
            '' ##### removed to catch err<>0 problem on error resume next
            ''
            'Dim SourcePointer As Integer
            'Dim Character As String
            'Dim LocalSource As String
            ''
            'EncodeRequestVariable = ""
            'If Source <> "" Then
            '    LocalSource = Source
            '    ' "+" is an allowed character for filenames. If you add it, the wrong file will be looked up
            '    'LocalSource = vbReplace(LocalSource, " ", "+")
            '    For SourcePointer = 1 To Len(LocalSource)
            '        Character = Mid(LocalSource, SourcePointer, 1)
            '        ' "%" added so if this is called twice, it will not destroy "%20" values
            '        If False Then
            '            'End If
            '            'If Character = " " Then
            '            EncodeRequestVariable += "+"
            '        ElseIf vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./:-_!*()", Character, vbTextCompare) <> 0 Then
            '            'ElseIf vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./:?#-_!~*'()%", Character, vbTextCompare) <> 0 Then
            '            EncodeRequestVariable += Character
            '        Else
            '            EncodeRequestVariable += "%" & Hex(Asc(Character))
            '        End If
            '    Next
            'End If
            '
        End Function
        ''
        ''========================================================================
        '' DecodeHTML
        ''
        ''   Convert HTML equivalent characters to their equivalents
        ''========================================================================
        ''
        'Public shared Function DecodeHTML(ByVal Source As String) As String
        '    ' ##### removed to catch err<>0 problem on error resume next
        '    '
        '    Dim Pos As Integer
        '    Dim s As String
        '    Dim CharCodeString As String
        '    Dim CharCode As Integer
        '    Dim PosEnd As Integer
        '    '
        '    ' 11/26/2009 - basically re-wrote it, I commented the old one out below
        '    '
        '    s = Source
        '    '
        '    ' numeric entities
        '    '
        '    Pos = Len(s)
        '    Pos = InStrRev(s, "&#", Pos)
        '    Do While Pos <> 0
        '        CharCodeString = ""
        '        If Mid(s, Pos + 3, 1) = ";" Then
        '            CharCodeString = Mid(s, Pos + 2, 1)
        '            PosEnd = Pos + 4
        '        ElseIf Mid(s, Pos + 4, 1) = ";" Then
        '            CharCodeString = Mid(s, Pos + 2, 2)
        '            PosEnd = Pos + 5
        '        ElseIf Mid(s, Pos + 5, 1) = ";" Then
        '            CharCodeString = Mid(s, Pos + 2, 3)
        '            PosEnd = Pos + 6
        '        End If
        '        If CharCodeString <> "" Then
        '            If vbIsNumeric(CharCodeString) Then
        '                CharCode = CLng(CharCodeString)
        '                s = Mid(s, 1, Pos - 1) & Chr(CharCode) & Mid(s, PosEnd)
        '            End If
        '        End If
        '        '
        '        Pos = InStrRev(s, "&#", Pos)
        '    Loop
        '    '
        '    ' character entities (at least the most common )
        '    '
        '    s = vbReplace(s, "&lt;", "<")
        '    s = vbReplace(s, "&gt;", ">")
        '    s = vbReplace(s, "&quot;", """")
        '    s = vbReplace(s, "&apos;", "'")
        '    '
        '    ' always last
        '    '
        '    s = vbReplace(s, "&amp;", "&")
        '    '
        '    DecodeHTML = s
        '    '
        'End Function
        '
        '========================================================================
        ' AddSpanClass
        '
        '   Adds a span around the copy with the class name provided
        '========================================================================
        '
        Public Shared Function AddSpan(ByVal Copy As String, ByVal ClassName As String) As String
            '
            AddSpan = "<SPAN Class=""" & ClassName & """>" & Copy & "</SPAN>"
            '
        End Function
        '
        '========================================================================
        ' DecodeResponseVariable
        '
        '   Converts a querystring name or value back into the characters it represents
        '   This is the same code as the decodeurl
        '========================================================================
        '
        Public Shared Function DecodeResponseVariable(ByVal Source As String) As String
            '
            Dim Position As Integer
            Dim ESCString As String
            Dim ESCValue As Integer
            Dim Digit0 As String
            Dim Digit1 As String
            'Dim iURL As String
            '
            'iURL = Source
            ' plus to space only applies for query component of a URL, but %99 encoding works for both
            'DecodeResponseVariable = vbReplace(iURL, "+", " ")
            DecodeResponseVariable = Source
            Position = vbInstr(1, DecodeResponseVariable, "%")
            Do While Position <> 0
                ESCString = Mid(DecodeResponseVariable, Position, 3)
                Digit0 = vbUCase(Mid(ESCString, 2, 1))
                Digit1 = vbUCase(Mid(ESCString, 3, 1))
                If ((Digit0 >= "0") And (Digit0 <= "9")) Or ((Digit0 >= "A") And (Digit0 <= "F")) Then
                    If ((Digit1 >= "0") And (Digit1 <= "9")) Or ((Digit1 >= "A") And (Digit1 <= "F")) Then
                        ESCValue = CInt("&H" & Mid(ESCString, 2))
                        DecodeResponseVariable = Mid(DecodeResponseVariable, 1, Position - 1) & Chr(ESCValue) & Mid(DecodeResponseVariable, Position + 3)
                        '  & vbReplace(DecodeResponseVariable, ESCString, Chr(ESCValue), Position, 1)
                    End If
                End If
                Position = vbInstr(Position + 1, DecodeResponseVariable, "%")
            Loop
            '
        End Function
        '
        '========================================================================
        ' DecodeURL
        '   Converts a querystring from an Encoded URL (with %20 and +), to non incoded (with spaced)
        '========================================================================
        '
        Public Shared Function DecodeURL(ByVal Source As String) As String
            ' ##### removed to catch err<>0 problem on error resume next
            '
            Dim Position As Integer
            Dim ESCString As String
            Dim ESCValue As Integer
            Dim Digit0 As String
            Dim Digit1 As String
            'Dim iURL As String
            '
            'iURL = Source
            ' plus to space only applies for query component of a URL, but %99 encoding works for both
            'DecodeURL = vbReplace(iURL, "+", " ")
            DecodeURL = Source
            Position = vbInstr(1, DecodeURL, "%")
            Do While Position <> 0
                ESCString = Mid(DecodeURL, Position, 3)
                Digit0 = vbUCase(Mid(ESCString, 2, 1))
                Digit1 = vbUCase(Mid(ESCString, 3, 1))
                If ((Digit0 >= "0") And (Digit0 <= "9")) Or ((Digit0 >= "A") And (Digit0 <= "F")) Then
                    If ((Digit1 >= "0") And (Digit1 <= "9")) Or ((Digit1 >= "A") And (Digit1 <= "F")) Then
                        ESCValue = CInt("&H" & Mid(ESCString, 2))
                        DecodeURL = vbReplace(DecodeURL, ESCString, Chr(ESCValue))
                    End If
                End If
                Position = vbInstr(Position + 1, DecodeURL, "%")
            Loop
            '
        End Function
        '
        '========================================================================
        ' GetFirstNonZeroDate
        '
        '   Converts a querystring name or value back into the characters it represents
        '========================================================================
        '
        Public Shared Function GetFirstNonZeroDate(ByVal Date0 As Date, ByVal Date1 As Date) As Date
            ' ##### removed to catch err<>0 problem on error resume next
            '
            Dim NullDate As Date
            '
            NullDate = Date.MinValue
            If Date0 = NullDate Then
                If Date1 = NullDate Then
                    '
                    ' Both 0, return 0
                    '
                    GetFirstNonZeroDate = NullDate
                Else
                    '
                    ' Date0 is NullDate, return Date1
                    '
                    GetFirstNonZeroDate = Date1
                End If
            Else
                If Date1 = NullDate Then
                    '
                    ' Date1 is nulldate, return Date0
                    '
                    GetFirstNonZeroDate = Date0
                ElseIf Date0 < Date1 Then
                    '
                    ' Date0 is first
                    '
                    GetFirstNonZeroDate = Date0
                Else
                    '
                    ' Date1 is first
                    '
                    GetFirstNonZeroDate = Date1
                End If
            End If
            '
        End Function
        '
        '========================================================================
        ' getFirstposition
        '
        '   returns 0 if both are zero
        '   returns 1 if the first integer is non-zero and less then the second
        '   returns 2 if the second integer is non-zero and less then the first
        '========================================================================
        '
        Public Shared Function GetFirstNonZeroInteger(ByVal Integer1 As Integer, ByVal Integer2 As Integer) As Integer
            ' ##### removed to catch err<>0 problem on error resume next
            '
            If Integer1 = 0 Then
                If Integer2 = 0 Then
                    '
                    ' Both 0, return 0
                    '
                    GetFirstNonZeroInteger = 0
                Else
                    '
                    ' Integer1 is 0, return Integer2
                    '
                    GetFirstNonZeroInteger = 2
                End If
            Else
                If Integer2 = 0 Then
                    '
                    ' Integer2 is 0, return Integer1
                    '
                    GetFirstNonZeroInteger = 1
                ElseIf Integer1 < Integer2 Then
                    '
                    ' Integer1 is first
                    '
                    GetFirstNonZeroInteger = 1
                Else
                    '
                    ' Integer2 is first
                    '
                    GetFirstNonZeroInteger = 2
                End If
            End If
            '
        End Function
        '
        '========================================================================
        ' splitDelimited
        '   returns the result of a Split, except it honors quoted text
        '   if a quote is found, it is assumed to also be a delimiter ( 'this"that"theother' = 'this "that" theother' )
        '========================================================================
        '
        Public Shared Function SplitDelimited(ByVal WordList As String, ByVal Delimiter As String) As String()
            ' ##### removed to catch err<>0 problem on error resume next
            '
            Dim QuoteSplit() As String
            Dim QuoteSplitCount As Integer
            Dim QuoteSplitPointer As Integer
            Dim InQuote As Boolean
            Dim Out() As String
            Dim OutPointer As Integer
            Dim OutSize As Integer
            Dim SpaceSplit() As String
            Dim SpaceSplitCount As Integer
            Dim SpaceSplitPointer As Integer
            Dim Fragment As String
            '
            OutPointer = 0
            ReDim Out(0)
            OutSize = 1
            If WordList <> "" Then
                QuoteSplit = Split(WordList, """")
                QuoteSplitCount = UBound(QuoteSplit) + 1
                InQuote = (Mid(WordList, 1, 1) = "")
                For QuoteSplitPointer = 0 To QuoteSplitCount - 1
                    Fragment = QuoteSplit(QuoteSplitPointer)
                    If Fragment = "" Then
                        '
                        ' empty fragment
                        ' this is a quote at the end, or two quotes together
                        ' do not skip to the next out pointer
                        '
                        If OutPointer >= OutSize Then
                            OutSize = OutSize + 10
                            ReDim Preserve Out(OutSize)
                        End If
                        'OutPointer = OutPointer + 1
                    Else
                        If Not InQuote Then
                            SpaceSplit = Split(Fragment, Delimiter)
                            SpaceSplitCount = UBound(SpaceSplit) + 1
                            For SpaceSplitPointer = 0 To SpaceSplitCount - 1
                                If OutPointer >= OutSize Then
                                    OutSize = OutSize + 10
                                    ReDim Preserve Out(OutSize)
                                End If
                                Out(OutPointer) = Out(OutPointer) & SpaceSplit(SpaceSplitPointer)
                                If (SpaceSplitPointer <> (SpaceSplitCount - 1)) Then
                                    '
                                    ' divide output between splits
                                    '
                                    OutPointer = OutPointer + 1
                                    If OutPointer >= OutSize Then
                                        OutSize = OutSize + 10
                                        ReDim Preserve Out(OutSize)
                                    End If
                                End If
                            Next
                        Else
                            Out(OutPointer) = Out(OutPointer) & """" & Fragment & """"
                        End If
                    End If
                    InQuote = Not InQuote
                Next
            End If
            ReDim Preserve Out(OutPointer)
            '
            '
            SplitDelimited = Out
            '
        End Function
        '
        '
        '
        Public Shared Function GetYesNo(ByVal Key As Boolean) As String
            If Key Then
                GetYesNo = "Yes"
            Else
                GetYesNo = "No"
            End If
        End Function
        ''
        ''
        ''
        'Public shared Function GetFilename(ByVal PathFilename As String) As String
        '    Dim Position As Integer
        '    '
        '    GetFilename = PathFilename
        '    Position = InStrRev(GetFilename, "/")
        '    If Position <> 0 Then
        '        GetFilename = Mid(GetFilename, Position + 1)
        '    End If
        'End Function
        '        '
        '        '
        '        '
        Public Shared Function StartTable(ByVal Padding As Integer, ByVal Spacing As Integer, ByVal Border As Integer, Optional ByVal ClassStyle As String = "") As String
            StartTable = "<table border=""" & Border & """ cellpadding=""" & Padding & """ cellspacing=""" & Spacing & """ class=""" & ClassStyle & """ width=""100%"">"
        End Function
        '        '
        '        '
        '        '
        Public Shared Function StartTableRow() As String
            StartTableRow = "<tr>"
        End Function
        '        '
        '        '
        '        '
        Public Shared Function StartTableCell(Optional ByVal Width As String = "", Optional ByVal ColSpan As Integer = 0, Optional ByVal EvenRow As Boolean = False, Optional ByVal Align As String = "", Optional ByVal BGColor As String = "") As String
            StartTableCell = ""
            If Width <> "" Then
                StartTableCell = " width=""" & Width & """"
            End If
            If BGColor <> "" Then
                StartTableCell = StartTableCell & " bgcolor=""" & BGColor & """"
            ElseIf EvenRow Then
                StartTableCell = StartTableCell & " class=""ccPanelRowEven"""
            Else
                StartTableCell = StartTableCell & " class=""ccPanelRowOdd"""
            End If
            If ColSpan <> 0 Then
                StartTableCell = StartTableCell & " colspan=""" & ColSpan & """"
            End If
            If Align <> "" Then
                StartTableCell = StartTableCell & " align=""" & Align & """"
            End If
            StartTableCell = "<TD" & StartTableCell & ">"
        End Function
        '        '
        '        '
        '        '
        Public Shared Function GetTableCell(ByVal Copy As String, Optional ByVal Width As String = "", Optional ByVal ColSpan As Integer = 0, Optional ByVal EvenRow As Boolean = False, Optional ByVal Align As String = "", Optional ByVal BGColor As String = "") As String
            GetTableCell = StartTableCell(Width, ColSpan, EvenRow, Align, BGColor) & Copy & kmaEndTableCell
        End Function
        '        '
        '        '
        '        '
        Public Shared Function GetTableRow(ByVal Cell As String, Optional ByVal ColSpan As Integer = 0, Optional ByVal EvenRow As Boolean = False) As String
            GetTableRow = StartTableRow() & GetTableCell(Cell, "100%", ColSpan, EvenRow) & kmaEndTableRow
        End Function
        '
        ' remove the host and approotpath, leaving the "active" path and all else
        '
        Public Shared Function ConvertShortLinkToLink(ByVal URL As String, ByVal PathPagePrefix As String) As String
            ConvertShortLinkToLink = URL
            If URL <> "" And PathPagePrefix <> "" Then
                If vbInstr(1, ConvertShortLinkToLink, PathPagePrefix, vbTextCompare) = 1 Then
                    ConvertShortLinkToLink = Mid(ConvertShortLinkToLink, Len(PathPagePrefix) + 1)
                End If
            End If
        End Function
        '
        ' ------------------------------------------------------------------------------------------------------
        '   Preserve URLs that do not start HTTP or HTTPS
        '   Preserve URLs from other sites (offsite)
        '   Preserve HTTP://ServerHost/ServerVirtualPath/Files/ in all cases
        '   Convert HTTP://ServerHost/ServerVirtualPath/folder/page -> /folder/page
        '   Convert HTTP://ServerHost/folder/page -> /folder/page
        ' ------------------------------------------------------------------------------------------------------
        '
        Public Shared Function ConvertLinkToShortLink(ByVal URL As String, ByVal ServerHost As String, ByVal ServerVirtualPath As String) As String
            '
            Dim BadString As String = ""
            Dim GoodString As String = ""
            Dim Protocol As String = ""
            Dim WorkingLink As String = ""
            '
            WorkingLink = URL
            '
            ' ----- Determine Protocol
            '
            If vbInstr(1, WorkingLink, "HTTP://", vbTextCompare) = 1 Then
                '
                ' HTTP
                '
                Protocol = Mid(WorkingLink, 1, 7)
            ElseIf vbInstr(1, WorkingLink, "HTTPS://", vbTextCompare) = 1 Then
                '
                ' HTTPS
                '
                ' try this -- a ssl link can not be shortened
                ConvertLinkToShortLink = WorkingLink
                Exit Function
                Protocol = Mid(WorkingLink, 1, 8)
            End If
            If Protocol <> "" Then
                '
                ' ----- Protcol found, determine if is local
                '
                GoodString = Protocol & ServerHost
                If (InStr(1, WorkingLink, GoodString, vbTextCompare) <> 0) Then
                    '
                    ' URL starts with Protocol ServerHost
                    '
                    GoodString = Protocol & ServerHost & ServerVirtualPath & "/files/"
                    If (InStr(1, WorkingLink, GoodString, vbTextCompare) <> 0) Then
                        '
                        ' URL is in the virtual files directory
                        '
                        BadString = GoodString
                        GoodString = ServerVirtualPath & "/files/"
                        WorkingLink = vbReplace(WorkingLink, BadString, GoodString, 1, 99, vbTextCompare)
                    Else
                        '
                        ' URL is not in files virtual directory
                        '
                        BadString = Protocol & ServerHost & ServerVirtualPath & "/"
                        GoodString = "/"
                        WorkingLink = vbReplace(WorkingLink, BadString, GoodString, 1, 99, vbTextCompare)
                        '
                        BadString = Protocol & ServerHost & "/"
                        GoodString = "/"
                        WorkingLink = vbReplace(WorkingLink, BadString, GoodString, 1, 99, vbTextCompare)
                    End If
                End If
            End If
            ConvertLinkToShortLink = WorkingLink
        End Function
        '
        ' Correct the link for the virtual path, either add it or remove it
        '
        Public Shared Function EncodeAppRootPath(ByVal Link As String, ByVal VirtualPath As String, ByVal AppRootPath As String, ByVal ServerHost As String) As String
            '
            Dim Protocol As String = ""
            Dim Host As String = ""
            Dim Path As String = ""
            Dim Page As String = ""
            Dim QueryString As String = ""
            Dim VirtualHosted As Boolean = False
            '
            EncodeAppRootPath = Link
            If (InStr(1, EncodeAppRootPath, ServerHost, vbTextCompare) <> 0) Or (InStr(1, Link, "/") = 1) Then
                'If (InStr(1, EncodeAppRootPath, ServerHost, vbTextCompare) <> 0) And (InStr(1, Link, "/") <> 0) Then
                '
                ' This link is onsite and has a path
                '
                VirtualHosted = (InStr(1, AppRootPath, VirtualPath, vbTextCompare) <> 0)
                If VirtualHosted And (InStr(1, Link, AppRootPath, vbTextCompare) = 1) Then
                    '
                    ' quick - virtual hosted and link starts at AppRootPath
                    '
                ElseIf (Not VirtualHosted) And (Mid(Link, 1, 1) = "/") And (InStr(1, Link, AppRootPath, vbTextCompare) = 1) Then
                    '
                    ' quick - not virtual hosted and link starts at Root
                    '
                Else
                    Call SeparateURL(Link, Protocol, Host, Path, Page, QueryString)
                    If VirtualHosted Then
                        '
                        ' Virtual hosted site, add VirualPath if it is not there
                        '
                        If vbInstr(1, Path, AppRootPath, vbTextCompare) = 0 Then
                            If Path = "/" Then
                                Path = AppRootPath
                            Else
                                Path = AppRootPath & Mid(Path, 2)
                            End If
                        End If
                    Else
                        '
                        ' Root hosted site, remove virtual path if it is there
                        '
                        If vbInstr(1, Path, AppRootPath, vbTextCompare) <> 0 Then
                            Path = vbReplace(Path, AppRootPath, "/")
                        End If
                    End If
                    EncodeAppRootPath = Protocol & Host & Path & Page & QueryString
                End If
            End If
        End Function
        '
        ' Return just the tablename from a tablename reference (database.object.tablename->tablename)
        '
        Public Shared Function GetDbObjectTableName(ByVal DbObject As String) As String
            Dim Position As Integer
            '
            GetDbObjectTableName = DbObject
            Position = InStrRev(GetDbObjectTableName, ".")
            If Position > 0 Then
                GetDbObjectTableName = Mid(GetDbObjectTableName, Position + 1)
            End If
        End Function
        '
        '
        '
        Public Shared Function GetLinkedText(ByVal AnchorTag As String, ByVal AnchorText As String) As String
            '
            Dim UcaseAnchorText As String
            Dim LinkPosition As Integer
            Dim MethodName As String
            Dim iAnchorTag As String
            Dim iAnchorText As String
            '
            MethodName = "GetLinkedText"
            '
            GetLinkedText = ""
            iAnchorTag = AnchorTag
            iAnchorText = AnchorText
            UcaseAnchorText = vbUCase(iAnchorText)
            If (iAnchorTag <> "") And (iAnchorText <> "") Then
                LinkPosition = InStrRev(UcaseAnchorText, "<LINK>", -1)
                If LinkPosition = 0 Then
                    GetLinkedText = iAnchorTag & iAnchorText & "</A>"
                Else
                    GetLinkedText = iAnchorText
                    LinkPosition = InStrRev(UcaseAnchorText, "</LINK>", -1)
                    Do While LinkPosition > 1
                        GetLinkedText = Mid(GetLinkedText, 1, LinkPosition - 1) & "</A>" & Mid(GetLinkedText, LinkPosition + 7)
                        LinkPosition = InStrRev(UcaseAnchorText, "<LINK>", LinkPosition - 1)
                        If LinkPosition <> 0 Then
                            GetLinkedText = Mid(GetLinkedText, 1, LinkPosition - 1) & iAnchorTag & Mid(GetLinkedText, LinkPosition + 6)
                        End If
                        LinkPosition = InStrRev(UcaseAnchorText, "</LINK>", LinkPosition)
                    Loop
                End If
            End If
            '
        End Function
        '
        Public Shared Function EncodeInitialCaps(ByVal Source As String) As String
            Dim SegSplit() As String
            Dim SegPtr As Integer
            Dim SegMax As Integer
            '
            EncodeInitialCaps = ""
            If Source <> "" Then
                SegSplit = Split(Source, " ")
                SegMax = UBound(SegSplit)
                If SegMax >= 0 Then
                    For SegPtr = 0 To SegMax
                        SegSplit(SegPtr) = vbUCase(Left(SegSplit(SegPtr), 1)) & vbLCase(Mid(SegSplit(SegPtr), 2))
                    Next
                End If
                EncodeInitialCaps = Join(SegSplit, " ")
            End If
        End Function
        '
        '
        '
        Public Shared Function RemoveTag(ByVal Source As String, ByVal TagName As String) As String
            Dim Pos As Integer
            Dim PosEnd As Integer
            RemoveTag = Source
            Pos = vbInstr(1, Source, "<" & TagName, vbTextCompare)
            If Pos <> 0 Then
                PosEnd = vbInstr(Pos, Source, ">")
                If PosEnd > 0 Then
                    RemoveTag = Left(Source, Pos - 1) & Mid(Source, PosEnd + 1)
                End If
            End If
        End Function
        '
        '
        '
        Public Shared Function RemoveStyleTags(ByVal Source As String) As String
            RemoveStyleTags = Source
            Do While vbInstr(1, RemoveStyleTags, "<style", vbTextCompare) <> 0
                RemoveStyleTags = RemoveTag(RemoveStyleTags, "style")
            Loop
            Do While vbInstr(1, RemoveStyleTags, "</style", vbTextCompare) <> 0
                RemoveStyleTags = RemoveTag(RemoveStyleTags, "/style")
            Loop
        End Function
        '
        '
        '
        Public Shared Function GetSingular(ByVal PluralSource As String) As String
            '
            Dim UpperCase As Boolean
            Dim LastCharacter As String
            '
            GetSingular = PluralSource
            If Len(GetSingular) > 1 Then
                LastCharacter = Right(GetSingular, 1)
                If LastCharacter <> vbUCase(LastCharacter) Then
                    UpperCase = True
                End If
                If vbUCase(Right(GetSingular, 3)) = "IES" Then
                    If UpperCase Then
                        GetSingular = Mid(GetSingular, 1, Len(GetSingular) - 3) & "Y"
                    Else
                        GetSingular = Mid(GetSingular, 1, Len(GetSingular) - 3) & "y"
                    End If
                ElseIf vbUCase(Right(GetSingular, 2)) = "SS" Then
                    ' nothing
                ElseIf vbUCase(Right(GetSingular, 1)) = "S" Then
                    GetSingular = Mid(GetSingular, 1, Len(GetSingular) - 1)
                Else
                    ' nothing
                End If
            End If
        End Function
        '
        '
        '
        Public Shared Function EncodeJavascript(ByVal Source As String) As String
            '
            EncodeJavascript = Source
            EncodeJavascript = vbReplace(EncodeJavascript, "\", "\\")
            EncodeJavascript = vbReplace(EncodeJavascript, "'", "\'")
            'EncodeJavascript = vbReplace(EncodeJavascript, "'", "'+""'""+'")
            EncodeJavascript = vbReplace(EncodeJavascript, vbCrLf, "\n")
            EncodeJavascript = vbReplace(EncodeJavascript, vbCr, "\n")
            EncodeJavascript = vbReplace(EncodeJavascript, vbLf, "\n")
            '
        End Function
        ''' <summary>
        ''' returns a 1-based index into the comma seperated ListOfItems where Item is found
        ''' </summary>
        ''' <param name="Item"></param>
        ''' <param name="ListOfItems"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetListIndex(ByVal Item As String, ByVal ListOfItems As String) As Integer
            '
            Dim Items() As String
            Dim LcaseItem As String
            Dim LcaseList As String
            Dim Ptr As Integer
            '
            GetListIndex = 0
            If ListOfItems <> "" Then
                LcaseItem = vbLCase(Item)
                LcaseList = vbLCase(ListOfItems)
                Items = SplitDelimited(LcaseList, ",")
                For Ptr = 0 To UBound(Items)
                    If Items(Ptr) = LcaseItem Then
                        GetListIndex = Ptr + 1
                        Exit For
                    End If
                Next
            End If
            '
        End Function
        '
        '
        '
        Public Shared Function EncodeInteger(ByVal Expression As Object) As Integer
            '
            EncodeInteger = 0
            If vbIsNumeric(Expression) Then
                EncodeInteger = CInt(Expression)
            ElseIf TypeOf Expression Is Boolean Then
                If DirectCast(Expression, Boolean) Then
                    EncodeInteger = 1
                End If
            End If
        End Function
        '
        '
        '
        Public Shared Function EncodeNumber(ByVal Expression As Object) As Double
            EncodeNumber = 0
            If vbIsNumeric(Expression) Then
                EncodeNumber = CDbl(Expression)
            ElseIf TypeOf Expression Is Boolean Then
                If DirectCast(Expression, Boolean) Then
                    EncodeNumber = 1
                End If
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function encodeText(ByVal Expression As Object) As String
            If Not (TypeOf Expression Is DBNull) Then
                If (Expression IsNot Nothing) Then
                    Return Expression.ToString()
                End If
            End If
            Return String.Empty
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function EncodeBoolean(ByVal Expression As Object) As Boolean
            EncodeBoolean = False
            If TypeOf Expression Is Boolean Then
                EncodeBoolean = DirectCast(Expression, Boolean)
            ElseIf vbIsNumeric(Expression) Then
                EncodeBoolean = (CStr(Expression) <> "0")
            ElseIf TypeOf Expression Is String Then
                Select Case Expression.ToString.ToLower.Trim
                    Case "on", "yes", "true"
                        EncodeBoolean = True
                End Select
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function EncodeDate(ByVal Expression As Object) As Date
            EncodeDate = Date.MinValue
            If IsDate(Expression) Then
                EncodeDate = CDate(Expression)
                'If EncodeDate < #1/1/1990# Then
                '    EncodeDate = Date.MinValue
                'End If
            End If
        End Function
        '
        '========================================================================
        '   Gets the next line from a string, and removes the line
        '========================================================================
        '
        Public Shared Function getLine(ByRef Body As String) As String
            Dim returnFirstLine As String = Body
            Try
                Dim EOL As Integer
                Dim NextCR As Integer
                Dim NextLF As Integer
                Dim BOL As Integer
                '
                NextCR = vbInstr(1, Body, vbCr)
                NextLF = vbInstr(1, Body, vbLf)

                If NextCR <> 0 Or NextLF <> 0 Then
                    If NextCR <> 0 Then
                        If NextLF <> 0 Then
                            If NextCR < NextLF Then
                                EOL = NextCR - 1
                                If NextLF = NextCR + 1 Then
                                    BOL = NextLF + 1
                                Else
                                    BOL = NextCR + 1
                                End If

                            Else
                                EOL = NextLF - 1
                                BOL = NextLF + 1
                            End If
                        Else
                            EOL = NextCR - 1
                            BOL = NextCR + 1
                        End If
                    Else
                        EOL = NextLF - 1
                        BOL = NextLF + 1
                    End If
                    returnFirstLine = Mid(Body, 1, EOL)
                    Body = Mid(Body, BOL)
                Else
                    returnFirstLine = Body
                    Body = ""
                End If
            Catch ex As Exception
                '
                '
                '
            End Try
            Return returnFirstLine
        End Function
        '
        '
        '
        Public Shared Function runProcess(cpCore As coreClass, ByVal Cmd As String, Optional ByVal Arguments As String = "", Optional ByVal WaitForReturn As Boolean = False) As String
            Dim returnResult As String = ""
            Dim p As Process = New Process()
            '
            logController.appendLog(cpCore, "ccCommonModule.runProcess, cmd=[" & Cmd & "], Arguments=[" & Arguments & "], WaitForReturn=[" & WaitForReturn & "]")
            '
            p.StartInfo.FileName = Cmd
            p.StartInfo.Arguments = Arguments
            p.StartInfo.UseShellExecute = False
            p.StartInfo.CreateNoWindow = True
            p.StartInfo.ErrorDialog = False
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
            If WaitForReturn Then
                p.StartInfo.RedirectStandardOutput = True
            End If
            p.Start()
            If WaitForReturn Then
                p.WaitForExit(1000 * 60 * 5)
                returnResult = p.StandardOutput.ReadToEnd()
            End If

            p.Dispose()
            '
            Return returnResult
        End Function
        ''
        'Public shared sub runProcess(cp.core,cmd As String, Optional ignore As Object = "", Optional waitforreturn As Boolean = False)
        '    Call runProcess(cp.core,cmd, waitforreturn)
        '    'Dim ShellObj As Object
        '    'ShellObj = CreateObject("WScript.Shell")
        '    'If Not (ShellObj Is Nothing) Then
        '    '    Call ShellObj.Run(Cmd, 0, WaitForReturn)
        '    'End If
        '    'ShellObj = Nothing
        'End Sub
        '
        '------------------------------------------------------------------------------------------------------------
        '   use only internally
        '
        '   encode an argument to be used in a name=value& (N-V-A) string
        '
        '   an argument can be any one of these is this format:
        '       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=Value[Option1|Option2]descriptor
        '
        '   to create an nva string
        '       string = encodeNvaArgument( name ) & "=" & encodeNvaArgument( value ) & "&"
        '
        '   to decode an nva string
        '       split on ampersand then on equal, and genericController.decodeNvaArgument() each part
        '
        '------------------------------------------------------------------------------------------------------------
        '
        Public Shared Function encodeNvaArgument(ByVal Arg As String) As String
            Dim a As String
            a = Arg
            If a <> "" Then
                a = vbReplace(a, vbCrLf, "#0013#")
                a = vbReplace(a, vbLf, "#0013#")
                a = vbReplace(a, vbCr, "#0013#")
                a = vbReplace(a, "&", "#0038#")
                a = vbReplace(a, "=", "#0061#")
                a = vbReplace(a, ",", "#0044#")
                a = vbReplace(a, """", "#0034#")
                a = vbReplace(a, "'", "#0039#")
                a = vbReplace(a, "|", "#0124#")
                a = vbReplace(a, "[", "#0091#")
                a = vbReplace(a, "]", "#0093#")
                a = vbReplace(a, ":", "#0058#")
            End If
            encodeNvaArgument = a
        End Function
        '
        '------------------------------------------------------------------------------------------------------------
        '   use only internally
        '       decode an argument removed from a name=value& string
        '       see encodeNvaArgument for details on how to use this
        '------------------------------------------------------------------------------------------------------------
        '
        Public Shared Function decodeNvaArgument(ByVal EncodedArg As String) As String
            Dim a As String
            '
            a = EncodedArg
            a = vbReplace(a, "#0058#", ":")
            a = vbReplace(a, "#0093#", "]")
            a = vbReplace(a, "#0091#", "[")
            a = vbReplace(a, "#0124#", "|")
            a = vbReplace(a, "#0039#", "'")
            a = vbReplace(a, "#0034#", """")
            a = vbReplace(a, "#0044#", ",")
            a = vbReplace(a, "#0061#", "=")
            a = vbReplace(a, "#0038#", "&")
            a = vbReplace(a, "#0013#", vbCrLf)
            decodeNvaArgument = a
        End Function
        '        '
        '        '========================================================================
        '        '   encodeSQLText
        '        '========================================================================
        '        '
        '        Public shared Function app.EncodeSQLText(ByVal expression As Object) As String
        '            Dim returnString As String = ""
        '            If expression Is Nothing Then
        '                returnString = "null"
        '            Else
        '                returnString = encodeText(expression)
        '                If returnString = "" Then
        '                    returnString = "null"
        '                Else
        '                    returnString = "'" & vbReplace(returnString, "'", "''") & "'"
        '                End If
        '            End If
        '            Return returnString
        '        End Function
        '        '
        '        '========================================================================
        '        '   encodeSQLLongText
        '        '========================================================================
        '        '
        '        Public shared Function app.EncodeSQLText(ByVal expression As Object) As String
        '            Dim returnString As String = ""
        '            If expression Is Nothing Then
        '                returnString = "null"
        '            Else
        '                returnString = encodeText(expression)
        '                If returnString = "" Then
        '                    returnString = "null"
        '                Else
        '                    returnString = "'" & vbReplace(returnString, "'", "''") & "'"
        '                End If
        '            End If
        '            Return returnString
        '        End Function
        '        '
        '        '========================================================================
        '        '   encodeSQLDate
        '        '       encode a date variable to go in an sql expression
        '        '========================================================================
        '        '
        '        Public shared Function app.EncodeSQLDate(ByVal expression As Object) As String
        '            Dim returnString As String = ""
        '            Dim expressionDate As Date = Date.MinValue
        '            If expression Is Nothing Then
        '                returnString = "null"
        '            ElseIf Not IsDate(expression) Then
        '                returnString = "null"
        '            Else
        '                If IsDBNull(expression) Then
        '                    returnString = "null"
        '                Else
        '                    expressionDate =  EncodeDate(expression)
        '                    If (expressionDate = Date.MinValue) Then
        '                        returnString = "null"
        '                    Else
        '                        returnString = "'" & Year(expressionDate) & Right("0" & Month(expressionDate), 2) & Right("0" & Day(expressionDate), 2) & " " & Right("0" & expressionDate.Hour, 2) & ":" & Right("0" & expressionDate.Minute, 2) & ":" & Right("0" & expressionDate.Second, 2) & ":" & Right("00" & expressionDate.Millisecond, 3) & "'"
        '                    End If
        '                End If
        '            End If
        '            Return returnString
        '        End Function
        '        '
        '        '========================================================================
        '        '   encodeSQLNumber
        '        '       encode a number variable to go in an sql expression
        '        '========================================================================
        '        '
        '        Public shared Function app.EncodeSQLNumber(ByVal expression As Object) As String
        '            Dim returnString As String = ""
        '            Dim expressionNumber As Double = 0
        '            If expression Is Nothing Then
        '                returnString = "null"
        '            ElseIf VarType(expression) = vbBoolean Then
        '                If expression Then
        '                    returnString = SQLTrue
        '                Else
        '                    returnString = SQLFalse
        '                End If
        '            ElseIf Not vbIsNumeric(expression) Then
        '                returnString = "null"
        '            Else
        '                returnString = expression.ToString
        '            End If
        '            Return returnString
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '        End Function
        '        '
        '        '========================================================================
        '        '   encodeSQLBoolean
        '        '       encode a boolean variable to go in an sql expression
        '        '========================================================================
        '        '
        '        Public shared Function app.EncodeSQLBoolean(ByVal ExpressionVariant As Object) As String
        '            '
        '            'Dim src As String
        '            '
        '            app.EncodeSQLBoolean = SQLFalse
        '            If EncodeBoolean(ExpressionVariant) Then
        '                app.EncodeSQLBoolean = SQLTrue
        '            End If
        '        End Function
        '
        '=================================================================================
        '   Renamed to catch all the cases that used it in addons
        '
        '   Do not use this routine in Addons to get the addon option string value
        '   to get the value in an option string, use cmc.csv_getAddonOption("name")
        '
        ' Get the value of a name in a string of name value pairs parsed with vrlf and =
        '   the legacy line delimiter was a '&' -> name1=value1&name2=value2"
        '   new format is "name1=value1 crlf name2=value2 crlf ..."
        '   There can be no extra spaces between the delimiter, the name and the "="
        '=================================================================================
        '
        Public Shared Function getSimpleNameValue(ByVal Name As String, ByVal ArgumentString As String, ByVal DefaultValue As String, ByVal Delimiter As String) As String
            '
            Dim WorkingString As String
            Dim iDefaultValue As String
            Dim NameLength As Integer
            Dim ValueStart As Integer
            Dim ValueEnd As Integer
            Dim IsQuoted As Boolean
            '
            ' determine delimiter
            '
            If Delimiter = "" Then
                '
                ' If not explicit
                '
                If vbInstr(1, ArgumentString, vbCrLf) <> 0 Then
                    '
                    ' crlf can only be here if it is the delimiter
                    '
                    Delimiter = vbCrLf
                Else
                    '
                    ' either only one option, or it is the legacy '&' delimit
                    '
                    Delimiter = "&"
                End If
            End If
            iDefaultValue = DefaultValue
            WorkingString = ArgumentString
            getSimpleNameValue = iDefaultValue
            If WorkingString <> "" Then
                WorkingString = Delimiter & WorkingString & Delimiter
                ValueStart = vbInstr(1, WorkingString, Delimiter & Name & "=", vbTextCompare)
                If ValueStart <> 0 Then
                    NameLength = Len(Name)
                    ValueStart = ValueStart + Len(Delimiter) + NameLength + 1
                    If Mid(WorkingString, ValueStart, 1) = """" Then
                        IsQuoted = True
                        ValueStart = ValueStart + 1
                    End If
                    If IsQuoted Then
                        ValueEnd = vbInstr(ValueStart, WorkingString, """" & Delimiter)
                    Else
                        ValueEnd = vbInstr(ValueStart, WorkingString, Delimiter)
                    End If
                    If ValueEnd = 0 Then
                        getSimpleNameValue = Mid(WorkingString, ValueStart)
                    Else
                        getSimpleNameValue = Mid(WorkingString, ValueStart, ValueEnd - ValueStart)
                    End If
                End If
            End If
            '

            Exit Function
            '
            ' ----- ErrorTrap
            '
ErrorTrap:
        End Function
        '==========================================================================================================================
        '   To convert from site license to server licenses, we still need the URLEncoder in the site license
        '   This routine generates a site license that is just the URL encoder.
        '==========================================================================================================================
        '
        Public Shared Function GetURLEncoder() As String
            Randomize()
            GetURLEncoder = CStr(Int(1 + (Rnd() * 8))) & CStr(Int(1 + (Rnd() * 8))) & CStr(Int(1000000000 + (Rnd() * 899999999)))
        End Function
        '
        '
        '
        Public Shared Function getIpAddressList() As String
            Dim ipAddressList As String = ""
            Dim host As IPHostEntry
            '
            host = Dns.GetHostEntry(Dns.GetHostName())
            For Each ip As IPAddress In host.AddressList
                If (ip.AddressFamily = Sockets.AddressFamily.InterNetwork) Then
                    ipAddressList &= "," & ip.ToString
                End If
            Next
            If ipAddressList <> "" Then
                ipAddressList = ipAddressList.Substring(1)
            End If
            Return ipAddressList
        End Function
        ''
        ''
        ''
        'Public shared Function GetAddonRootPath() As String
        '    Dim testPath As String
        '    '
        '    GetAddonRootPath = "addons"
        '    If vbInstr(1, GetAddonRootPath, "\github\", vbTextCompare) <> 0 Then
        '        '
        '        ' debugging - change program path to dummy path so addon builds all copy to
        '        '
        '        testPath = Environ$("programfiles(x86)")
        '        If testPath = "" Then
        '            testPath = Environ$("programfiles")
        '        End If
        '        GetAddonRootPath = testPath & "\kma\contensive\addons"
        '    End If
        'End Function
        '
        '
        '
        Public Shared Function IsNull(ByVal source As Object) As Boolean
            Return (source Is Nothing)
        End Function
        ''
        ''
        ''
        'Public shared Function isNothing(ByVal source As Object) As Boolean
        '    Return IsNothing(source)
        '    'Dim returnIsEmpty As Boolean = True
        '    'Try
        '    '    If Not IsNothing(source) Then

        '    '    End If
        '    'Catch ex As Exception
        '    '    '
        '    'End Try
        '    'Return returnIsEmpty
        'End Function
        '
        '
        '
        Public Shared Function isMissing(ByVal source As Object) As Boolean
            Return False
        End Function
        '
        ' convert date to number of seconds since 1/1/1990
        '
        Public Shared Function dateToSeconds(ByVal sourceDate As Date) As Integer
            Dim returnSeconds As Integer
            Dim oldDate = New Date(1900, 1, 1)
            If sourceDate.CompareTo(oldDate) > 0 Then
                returnSeconds = CInt(sourceDate.Subtract(oldDate).TotalSeconds)
            End If
            Return returnSeconds
        End Function
        '
        ' ==============================================================================
        ' true if there is a previous instance of this application running
        ' ==============================================================================
        '
        Public Shared Function PrevInstance() As Boolean
            If UBound(Diagnostics.Process.GetProcessesByName _
               (Diagnostics.Process.GetCurrentProcess.ProcessName)) _
               > 0 Then
                Return True
            Else
                Return False
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Encode a date to minvalue, if date is < minVAlue,m set it to minvalue, if date < 1/1/1990 (the beginning of time), it returns date.minvalue
        ''' </summary>
        ''' <param name="sourceDate"></param>
        ''' <returns></returns>
        Public Shared Function encodeDateMinValue(ByVal sourceDate As Date) As Date
            If sourceDate <= #1/1/1000# Then
                Return Date.MinValue
            End If
            Return sourceDate
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Return true if a date is the mindate, else return false 
        ''' </summary>
        ''' <param name="sourceDate"></param>
        ''' <returns></returns>
        Public Shared Function isMinDate(sourceDate As Date) As Boolean
            Return encodeDateMinValue(sourceDate) = Date.MinValue
        End Function
        ''
        'Public Shared Function getVirtualTableFieldPath(ByVal TableName As String, ByVal FieldName As String) As String
        '    Dim result As String = TableName & "/" & FieldName & "/"
        '    Return result.Replace(" ", "_").Replace(".", "_")
        'End Function
        'Public Shared Function getVirtualTableFieldIdPath(ByVal TableName As String, ByVal FieldName As String, ByVal RecordID As Integer) As String
        '    Return getVirtualTableFieldPath(TableName, FieldName) & RecordID.ToString().PadLeft(12, "0"c) & "/"
        'End Function
        ''
        ''========================================================================
        '' ----- Create a filename for the Virtual Directory
        ''   Do not allow spaces.
        ''   If the content supports authoring, the filename returned will be for the
        ''   current authoring record.
        ''========================================================================
        ''
        'Public Shared Function getVirtualRecordPathFilename(ByVal TableName As String, ByVal FieldName As String, ByVal RecordID As Integer, ByVal OriginalFilename As String, ByVal fieldType As Integer) As String
        '    Dim result As String = ""
        '    '
        '    Dim iOriginalFilename As String = OriginalFilename.Replace(" ", "_").Replace(".", "_")
        '    If OriginalFilename <> "" Then
        '        result = getVirtualTableFieldIdPath(TableName, FieldName, RecordID) & OriginalFilename
        '    Else
        '        Dim IdFilename As String = CStr(RecordID)
        '        If RecordID = 0 Then
        '            IdFilename = getGUID().Replace("{", "").Replace("}", "").Replace("-", "")
        '        Else
        '            IdFilename = RecordID.ToString().PadLeft(12, "0"c)
        '        End If
        '        Select Case fieldType
        '            Case FieldTypeIdFileCSS
        '                result = getVirtualTableFieldPath(TableName, FieldName) & IdFilename & ".css"
        '            Case FieldTypeIdFileXML
        '                result = getVirtualTableFieldPath(TableName, FieldName) & IdFilename & ".xml"
        '            Case FieldTypeIdFileJavascript
        '                result = getVirtualTableFieldPath(TableName, FieldName) & IdFilename & ".js"
        '            Case FieldTypeIdFileHTML
        '                result = getVirtualTableFieldPath(TableName, FieldName) & IdFilename & ".html"
        '            Case Else
        '                result = getVirtualTableFieldPath(TableName, FieldName) & IdFilename & ".txt"
        '        End Select
        '    End If
        '    Return result
        'End Function
        '
        '====================================================================================================
        ' the the name of the current executable
        '====================================================================================================
        '
        Public Shared Function getAppExeName() As String
            Return System.IO.Path.GetFileName(System.Windows.Forms.Application.ExecutablePath)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' convert a dtaTable to list of string 
        ''' </summary>
        ''' <param name="dt"></param>
        ''' <returns></returns>
        Public Shared Function convertDataTableColumntoItemList(dt As DataTable) As List(Of String)
            Dim returnString As New List(Of String)
            For Each dr As DataRow In dt.Rows
                returnString.Add(dr.Item(0).ToString.ToLower())
            Next
            Return returnString
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' convert a dtaTable to a comma delimited list of column 0
        ''' </summary>
        ''' <param name="dt"></param>
        ''' <returns></returns>
        Public Shared Function convertDataTableColumntoItemCommaList(dt As DataTable) As String
            Dim returnString As String = ""
            For Each dr As DataRow In dt.Rows
                returnString &= "," & dr.Item(0).ToString
            Next
            If returnString.Length > 0 Then
                returnString = returnString.Substring(1)
            End If
            Return returnString
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' returns true or false if a string is located within another string. Similar to indexof but returns true/false 
        ''' </summary>
        ''' <param name="start"></param>
        ''' <param name="haystack"></param>
        ''' <param name="needle"></param>
        ''' <param name="ignore"></param>
        ''' <returns></returns>
        Public Shared Function isInStr(start As Integer, haystack As String, needle As String, Optional ignore As CompareMethod = CompareMethod.Text) As Boolean
            Return (InStr(start, haystack, needle, vbTextCompare) >= 0)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Convert a route to the anticipated format (lowercase,no leading /, no trailing /)
        ''' </summary>
        ''' <param name="route"></param>
        ''' <returns></returns>
        Public Shared Function normalizeRoute(route As String) As String
            Dim normalizedRoute As String = route.ToLower().Trim()
            Try
                If String.IsNullOrEmpty(normalizedRoute) Then
                    normalizedRoute = String.Empty
                Else
                    normalizedRoute = genericController.convertToUnixSlash(normalizedRoute)
                    Do While normalizedRoute.IndexOf("//") >= 0
                        normalizedRoute = normalizedRoute.Replace("//", "/")
                    Loop
                    If (normalizedRoute.Substring(0, 1).Equals("/")) Then
                        normalizedRoute = normalizedRoute.Substring(1)
                    End If
                    If (normalizedRoute.Substring(normalizedRoute.Length - 1, 1) = "/") Then
                        normalizedRoute = normalizedRoute.Substring(0, normalizedRoute.Length - 1)
                    End If
                End If
            Catch ex As Exception
                Throw New ApplicationException("Unexpected exception in normalizeRoute(route=[" & route & "])", ex)
            End Try
            Return normalizedRoute
        End Function
        '
        '========================================================================
        '   converts a virtual file into a filename
        '       - in local mode, the cdnFiles can be mapped to a virtual folder in appRoot
        '           -- see appConfig.cdnFilesVirtualFolder
        '       convert all / to \
        '       if it includes "://", it is a root file
        '       if it starts with "/", it is already root relative
        '       else (if it start with a file or a path), add the publicFileContentPathPrefix
        '========================================================================
        '
        Public Shared Function convertCdnUrlToCdnPathFilename(ByVal cdnUrl As String) As String
            '
            ' this routine was originally written to handle modes that were not adopted (content file absolute and relative URLs)
            ' leave it here as a simple slash converter in case other conversions are needed later
            '
            Return vbReplace(cdnUrl, "/", "\")
        End Function
        '
        '==============================================================================
        Public Shared Function isGuid(Source As String) As Boolean
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
                Throw New ApplicationException("Exception in isGuid", ex)
            End Try
            Return returnValue
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function vbInstr(string1 As String, string2 As String, compare As CompareMethod) As Integer
            Return vbInstr(1, string1, string2, compare)
        End Function
        '
        Public Shared Function vbInstr(string1 As String, string2 As String) As Integer
            Return vbInstr(1, string1, string2, CompareMethod.Binary)
        End Function
        '
        Public Shared Function vbInstr(start As Integer, string1 As String, string2 As String) As Integer
            Return vbInstr(start, string1, string2, CompareMethod.Binary)
        End Function
        '
        Public Shared Function vbInstr(start As Integer, string1 As String, string2 As String, compare As CompareMethod) As Integer
            If (String.IsNullOrEmpty(string1)) Then
                Return 0
            Else
                If (start < 1) Then
                    Throw New ArgumentException("Instr() start must be > 0.")
                Else
                    If (compare = CompareMethod.Binary) Then
                        Return string1.IndexOf(string2, start - 1, StringComparison.CurrentCulture) + 1
                    Else
                        Return string1.IndexOf(string2, start - 1, StringComparison.CurrentCultureIgnoreCase) + 1
                    End If
                End If
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function vbReplace(expression As String, find As String, replacement As Integer) As String
            Return vbReplace(expression, find, replacement.ToString(), 1, 9999, CompareMethod.Binary)
        End Function
        '
        Public Shared Function vbReplace(expression As String, find As String, replacement As String) As String
            Return vbReplace(expression, find, replacement, 1, 9999, CompareMethod.Binary)
        End Function
        '
        Public Shared Function vbReplace(expression As String, oldValue As String, replacement As String, startIgnore As Integer, countIgnore As Integer, compare As CompareMethod) As String
            If String.IsNullOrEmpty(expression) Then
                Return expression
            ElseIf String.IsNullOrEmpty(oldValue) Then
                Return expression
            Else
                If compare = CompareMethod.Binary Then
                    Return expression.Replace(oldValue, replacement)
                Else

                    Dim sb As New StringBuilder()
                    Dim previousIndex As Integer = 0
                    Dim Index As Integer = expression.IndexOf(oldValue, StringComparison.CurrentCultureIgnoreCase)
                    Do While (Index <> -1)
                        sb.Append(expression.Substring(previousIndex, Index - previousIndex))
                        sb.Append(replacement)
                        Index += oldValue.Length
                        previousIndex = Index
                        Index = expression.IndexOf(oldValue, Index, StringComparison.CurrentCultureIgnoreCase)
                    Loop
                    sb.Append(expression.Substring(previousIndex))
                    Return sb.ToString()

                    '    ElseIf String.IsNullOrEmpty(replacement) Then
                    '    Return Regex.Replace(expression, find, "", RegexOptions.IgnoreCase)
                    'Else
                    '    Return Regex.Replace(expression, find, replacement, RegexOptions.IgnoreCase)
                End If
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Visual Basic UCase
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Public Shared Function vbUCase(source As String) As String
            If (String.IsNullOrEmpty(source)) Then
                Return ""
            Else
                Return source.ToUpper
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Visual Basic LCase
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Public Shared Function vbLCase(source As String) As String
            If (String.IsNullOrEmpty(source)) Then
                Return ""
            Else
                Return source.ToLower
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' visual basic Left()
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="length"></param>
        ''' <returns></returns>
        Public Shared Function vbLeft(source As String, length As Integer) As String
            If (String.IsNullOrEmpty(source)) Then
                Return ""
            Else
                Return source.Substring(length)
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Visual Basic Right()
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="length"></param>
        ''' <returns></returns>
        Public Shared Function vbRight(source As String, length As Integer) As String
            If (String.IsNullOrEmpty(source)) Then
                Return ""
            Else
                Return source.Substring(source.Length - length)
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Visual Basic Len()
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Public Shared Function vbLen(source As String) As Integer
            If (String.IsNullOrEmpty(source)) Then
                Return 0
            Else
                Return source.Length
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Visual Basic Mid()
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="startIndex"></param>
        ''' <returns></returns>
        Public Shared Function vbMid(source As String, startIndex As Integer) As String
            If (String.IsNullOrEmpty(source)) Then
                Return ""
            Else
                Return source.Substring(startIndex)
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Visual Basic Mid()
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="startIndex"></param>
        ''' <param name="length"></param>
        ''' <returns></returns>
        Public Shared Function vbMid(source As String, startIndex As Integer, length As Integer) As String
            If (String.IsNullOrEmpty(source)) Then
                Return ""
            Else
                Return source.Substring(startIndex, length)
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' replacement for visual basic isNumeric
        ''' </summary>
        ''' <param name="Expression"></param>
        ''' <returns></returns>
        Public Shared Function vbIsNumeric(Expression As Object) As Boolean
            If (TypeOf Expression Is DateTime) Then
                Return False
            ElseIf (Expression Is Nothing) Then
                Return False
            ElseIf (TypeOf Expression Is Integer) Or (TypeOf Expression Is Int16) Or (TypeOf Expression Is Int32) Or (TypeOf Expression Is Int64) Or (TypeOf Expression Is Decimal) Or (TypeOf Expression Is Single) Or (TypeOf Expression Is Double) Or (TypeOf Expression Is Boolean) Then
                Return True
            ElseIf (TypeOf Expression Is String) Then
                Dim output As Double
                Return Double.TryParse(DirectCast(Expression, String), output)
            Else
                Return False
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' convert a date to the number of days since date.min
        ''' </summary>
        ''' <param name="srcDate"></param>
        ''' <returns></returns>
        Public Shared Function convertDateToDayPtr(srcDate As Date) As Integer
            Return CInt(DateDiff(DateInterval.Day, srcDate, Date.MinValue))
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Encodes an argument in an Addon OptionString (QueryString) for all non-allowed characters
        ''' Arg0,Arg1,Arg2,Arg3,Name=Value&Name=VAlue[Option1|Option2]
        ''' call this before parsing them together
        ''' call decodeAddonOptionArgument after parsing them apart
        ''' </summary>
        ''' <param name="Arg"></param>
        ''' <returns></returns>
        '------------------------------------------------------------------------------------------------------------
        '
        Friend Shared Function encodeLegacyOptionStringArgument(ByVal Arg As String) As String
            Dim a As String = ""
            If Not String.IsNullOrEmpty(Arg) Then
                a = Arg
                a = genericController.vbReplace(a, vbCrLf, "#0013#")
                a = genericController.vbReplace(a, vbLf, "#0013#")
                a = genericController.vbReplace(a, vbCr, "#0013#")
                a = genericController.vbReplace(a, "&", "#0038#")
                a = genericController.vbReplace(a, "=", "#0061#")
                a = genericController.vbReplace(a, ",", "#0044#")
                a = genericController.vbReplace(a, """", "#0034#")
                a = genericController.vbReplace(a, "'", "#0039#")
                a = genericController.vbReplace(a, "|", "#0124#")
                a = genericController.vbReplace(a, "[", "#0091#")
                a = genericController.vbReplace(a, "]", "#0093#")
                a = genericController.vbReplace(a, ":", "#0058#")
            End If
            Return a
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function createGuid() As String
            Return "{" & Guid.NewGuid().ToString & "}"
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Returns true if the argument is a string in guid compatible format
        ''' </summary>
        ''' <param name="guid"></param>
        ''' <returns></returns>
        Public Shared Function common_isGuid(guid As String) As Boolean
            Dim returnIsGuid As Boolean = False
            Try
                returnIsGuid = (Len(guid) = 38) And (Left(guid, 1) = "{") And (Right(guid, 1) = "}")
            Catch ex As Exception
                Throw (ex)
            End Try
            Return returnIsGuid
        End Function
        '
        '========================================================================
        ' main_encodeCookieName
        '   replace invalid cookie characters with %hex
        '========================================================================
        '
        Public Shared Function main_encodeCookieName(ByVal Source As String) As String
            Dim result As String = ""

            Dim SourcePointer As Integer
            Dim Character As String
            Dim localSource As String
            '
            If Source <> "" Then
                localSource = Source
                For SourcePointer = 1 To Len(localSource)
                    Character = Mid(localSource, SourcePointer, 1)
                    If genericController.vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_!*()", Character, vbTextCompare) <> 0 Then
                        result = result & Character
                    Else
                        result = result & "%" & Hex(Asc(Character))
                    End If
                Next
            End If
            '
            Return result
        End Function
        Public Shared Function main_GetYesNo(ByVal InputValue As Boolean) As String
            If InputValue Then
                Return "Yes"
            Else
                Return "No"
            End If
        End Function
        '
        '
        '=============================================================================
        ' ----- Return the value associated with the name given
        '   NameValueString is a string of Name=Value pairs, separated by spaces or "&"
        '   If Name is not given, returns ""
        '   If Name present but no value, returns true (as if Name=true)
        '   If Name = Value, it returns value
        '=============================================================================
        '
        Public Shared Function main_GetNameValue_Internal(cpcore As coreClass, NameValueString As String, Name As String) As String
            Dim result As String = ""
            '
            Dim NameValueStringWorking As String = NameValueString
            Dim UcaseNameValueStringWorking As String = NameValueString.ToUpper()
            Dim pairs() As String
            Dim PairCount As Integer
            Dim PairPointer As Integer
            Dim PairSplit() As String
            '
            If ((NameValueString <> "") And (Name <> "")) Then
                Do While genericController.vbInstr(1, NameValueStringWorking, " =") <> 0
                    NameValueStringWorking = genericController.vbReplace(NameValueStringWorking, " =", "=")
                Loop
                Do While genericController.vbInstr(1, NameValueStringWorking, "= ") <> 0
                    NameValueStringWorking = genericController.vbReplace(NameValueStringWorking, "= ", "=")
                Loop
                Do While genericController.vbInstr(1, NameValueStringWorking, "& ") <> 0
                    NameValueStringWorking = genericController.vbReplace(NameValueStringWorking, "& ", "&")
                Loop
                Do While genericController.vbInstr(1, NameValueStringWorking, " &") <> 0
                    NameValueStringWorking = genericController.vbReplace(NameValueStringWorking, " &", "&")
                Loop
                NameValueStringWorking = NameValueString & "&"
                UcaseNameValueStringWorking = genericController.vbUCase(NameValueStringWorking)
                '
                result = ""
                If NameValueStringWorking <> "" Then
                    pairs = Split(NameValueStringWorking, "&")
                    PairCount = UBound(pairs) + 1
                    For PairPointer = 0 To PairCount - 1
                        PairSplit = Split(pairs(PairPointer), "=")
                        If genericController.vbUCase(PairSplit(0)) = genericController.vbUCase(Name) Then
                            If UBound(PairSplit) > 0 Then
                                result = PairSplit(1)
                            End If
                            Exit For
                        End If
                    Next
                End If
            End If
            Return result
        End Function
        '
        '=============================================================================
        ' Cleans a text file of control characters, allowing only vblf
        '=============================================================================
        '
        Public Shared Function main_RemoveControlCharacters(ByVal DirtyText As String) As String
            Dim result As String = DirtyText
            Dim Pointer As Integer
            Dim ChrTest As Integer
            Dim iDirtyText As String
            '
            iDirtyText = encodeText(DirtyText)
            result = ""
            If (iDirtyText <> "") Then
                result = ""
                For Pointer = 1 To Len(iDirtyText)
                    ChrTest = Asc(Mid(iDirtyText, Pointer, 1))
                    If ChrTest >= 32 And ChrTest < 128 Then
                        result = result & Chr(ChrTest)
                    Else
                        Select Case ChrTest
                            Case 9
                                result = result & " "
                            Case 10
                                result = result & vbLf
                        End Select
                    End If
                Next
                '
                ' limit CRLF to 2
                '
                Do While vbInstr(result, vbLf & vbLf & vbLf) <> 0
                    result = vbReplace(result, vbLf & vbLf & vbLf, vbLf & vbLf)
                Loop
                '
                ' limit spaces to 1
                '
                Do While vbInstr(result, "  ") <> 0
                    result = vbReplace(result, "  ", " ")
                Loop
            End If
            Return result
        End Function
        '
        '========================================================================
        '   convert a virtual file into a Link usable on the website:
        '       convert all \ to /
        '       if it includes "://", leave it along
        '       if it starts with "/", it is already root relative, leave it alone
        '       else (if it start with a file or a path), add the serverFilePath
        '========================================================================
        '
        Public Shared Function getCdnFileLink(cpcore As coreClass, ByVal virtualFile As String) As String
            Dim returnLink As String
            '
            returnLink = virtualFile
            returnLink = genericController.vbReplace(returnLink, "\", "/")
            If genericController.vbInstr(1, returnLink, "://") <> 0 Then
                '
                ' icon is an Absolute URL - leave it
                '
                Return returnLink
            ElseIf Left(returnLink, 1) = "/" Then
                '
                ' icon is Root Relative, leave it
                '
                Return returnLink
            Else
                '
                ' icon is a virtual file, add the serverfilepath
                '
                Return cpcore.serverConfig.appConfig.cdnFilesNetprefix & returnLink
            End If
        End Function
        '
        '
        '
        Public Shared Function csv_GetLinkedText(ByVal AnchorTag As String, ByVal AnchorText As String) As String
            Dim result As String = ""
            Dim UcaseAnchorText As String
            Dim LinkPosition As Integer
            Dim iAnchorTag As String
            Dim iAnchorText As String
            '
            iAnchorTag = genericController.encodeText(AnchorTag)
            iAnchorText = genericController.encodeText(AnchorText)
            UcaseAnchorText = genericController.vbUCase(iAnchorText)
            If (iAnchorTag <> "") And (iAnchorText <> "") Then
                LinkPosition = InStrRev(UcaseAnchorText, "<LINK>", -1)
                If LinkPosition = 0 Then
                    result = iAnchorTag & iAnchorText & "</a>"
                Else
                    result = iAnchorText
                    LinkPosition = InStrRev(UcaseAnchorText, "</LINK>", -1)
                    Do While LinkPosition > 1
                        result = Mid(result, 1, LinkPosition - 1) & "</a>" & Mid(result, LinkPosition + 7)
                        LinkPosition = InStrRev(UcaseAnchorText, "<LINK>", LinkPosition - 1)
                        If LinkPosition <> 0 Then
                            result = Mid(result, 1, LinkPosition - 1) & iAnchorTag & Mid(result, LinkPosition + 6)
                        End If
                        LinkPosition = InStrRev(UcaseAnchorText, "</LINK>", LinkPosition)
                    Loop
                End If
            End If
            Return result
        End Function
        '
        Friend Shared Function convertNameValueDictToREquestString(nameValueDict As Dictionary(Of String, String)) As String
            Dim requestFormSerialized As String = ""
            If nameValueDict.Count > 0 Then
                For Each kvp As KeyValuePair(Of String, String) In nameValueDict
                    requestFormSerialized &= "&" & EncodeURL(Left(kvp.Key, 255)) & "=" & EncodeURL(Left(kvp.Value, 255))
                    If (requestFormSerialized.Length > 255) Then Exit For
                Next
            End If
            Return requestFormSerialized
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' if date is invalid, set to minValue
        ''' </summary>
        ''' <param name="srcDate"></param>
        ''' <returns></returns>
        Public Shared Function encodeMinDate(srcDate As DateTime) As DateTime
            Dim returnDate As DateTime = srcDate
            If srcDate < New DateTime(1900, 1, 1) Then
                returnDate = DateTime.MinValue
            End If
            Return returnDate
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' if valid date, return the short date, else return blank string 
        ''' </summary>
        ''' <param name="srcDate"></param>
        ''' <returns></returns>
        Public Shared Function getShortDateString(srcDate As DateTime) As String
            Dim returnString As String = ""
            Dim workingDate As DateTime = encodeMinDate(srcDate)
            If Not isDateEmpty(srcDate) Then
                returnString = workingDate.ToShortDateString()
            End If
            Return returnString
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function isDateEmpty(srcDate As DateTime) As Boolean
            Return (srcDate < New DateTime(1900, 1, 1))
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function urlEncodePath(ByVal path As String) As String
            Return Uri.EscapeUriString(convertToUnixSlash(path))
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function convertToDosSlash(ByVal path As String) As String
            Return path.Replace("/", "\")
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function convertToUnixSlash(ByVal path As String) As String
            Return path.Replace("\", "/")
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function getPath(ByVal PathFilename As String) As String
            Dim result As String = PathFilename
            If (Not String.IsNullOrEmpty(result)) Then
                Dim slashpos As Integer = PathFilename.Replace("/", "\").LastIndexOf("\")
                If (slashpos >= 0) And (slashpos < PathFilename.Length) Then
                    result = PathFilename.Substring(0, slashpos + 1)
                End If
            End If
            Return result
        End Function
        '
        '========================================================================
        ' EncodeHTML
        '
        '   Convert all characters that are not allowed in HTML to their Text equivalent
        '   in preperation for use on an HTML page
        '========================================================================
        '
        Public Shared Function encodeHTML(ByVal Source As String) As String
            ' ##### removed to catch err<>0 problem on error resume next
            '
            encodeHTML = Source
            encodeHTML = genericController.vbReplace(encodeHTML, "&", "&amp;")
            encodeHTML = genericController.vbReplace(encodeHTML, "<", "&lt;")
            encodeHTML = genericController.vbReplace(encodeHTML, ">", "&gt;")
            encodeHTML = genericController.vbReplace(encodeHTML, """", "&quot;")
            encodeHTML = genericController.vbReplace(encodeHTML, "'", "&apos;")
            '
        End Function
        '
        '======================================================================================
        ''' <summary>
        ''' Convert addon argument list to a doc property compatible dictionary of strings
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="SrcOptionList"></param>
        ''' <returns></returns>
        Public Shared Function convertAddonArgumentstoDocPropertiesList(cpCore As coreClass, SrcOptionList As String) As Dictionary(Of String, String)
            Dim returnList As New Dictionary(Of String, String)
            Try
                Dim SrcOptions As String()
                Dim key As String
                Dim value As String
                Dim Pos As Integer
                '
                If Not String.IsNullOrEmpty(SrcOptionList) Then
                    SrcOptions = Split(SrcOptionList.Replace(vbCrLf, vbCr).Replace(vbLf, vbCr), vbCr)
                    For Ptr = 0 To UBound(SrcOptions)
                        key = SrcOptions(Ptr).Replace(vbTab, "")
                        If Not String.IsNullOrEmpty(key) Then
                            value = ""
                            Pos = genericController.vbInstr(1, key, "=")
                            If Pos > 0 Then
                                value = Mid(key, Pos + 1)
                                key = Mid(key, 1, Pos - 1)
                            End If
                            returnList.Add(key, value)
                        End If
                    Next
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnList
        End Function
    End Class
End Namespace
