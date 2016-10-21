
Imports Contensive.Core.ccCommonModule
'
Namespace Contensive.Core
    Public Class importClass
        '
        Private CPcore As cpCoreClass

        '        '
        '        '=================================================================================
        '        '
        '        '=================================================================================
        '        '
        '        Public Sub CompactXML(ByVal DataSource As String, ByVal TableName As String, ByVal FilePath As String)
        '            On Error GoTo ErrorTrap
        '            '
        '            Dim SQL As String
        '            dim dt as datatable
        '            Dim Columns As String
        '            Dim Rows As String
        '            Dim Field As Field
        '            Dim Filename As String
        '            Dim DT As New dataTreeClass
        '            Dim NodePtr As Integer
        '            Dim Column() As String
        '            Dim ColumnCnt As Integer
        '            Dim Row() As String
        '            Dim ForceID As Boolean
        '            Dim ColumnPtr As Integer
        '            Dim RecordID As Integer
        '            Dim SelectSQL As String
        '            Dim UpdateSQL As String
        '            Dim InsertSQL As String
        '            Dim InsertSQLNameList As String
        '            Dim InsertSQLValueList As String
        '            Dim KeyField As String
        '            Dim RecordKey As String
        '            '
        '            If TableName <> "" Then
        '                If DataSource = "" Then
        '                    DataSource = "DEFAULT"
        '                End If
        '                If FilePath = "" Then
        '                    Filename = cmc.asv.config.physicalFilePath & "\DataArchive\Archive-" & DataSource & "-" & TableName & ".xml"
        '                ElseIf Right(FilePath, 1) = "\" Then
        '                    Filename = FilePath & "DataArchive\Archive-" & DataSource & "-" & TableName & ".xml"
        '                Else
        '                    Filename = FilePath & "\DataArchive\Archive-" & DataSource & "-" & TableName & ".xml"
        '                End If
        '            End If
        '            Call DT.LoadFile(Filename)
        '            If UCase(DT.nodename) = "CONTENSIVEARCHIVE" Then

        '                ForceID = EncodeBoolean(DT.GetAttr("ForceID"))
        '                KeyField = UCase(EncodeText(DT.GetAttr("KeyField")))
        '                If KeyField = "" Then
        '                    KeyField = "ID"
        '                End If
        '                Call DT.GoFirstChild()
        '                Do While DT.IsNodeOK
        '                    Select Case UCase(DT.nodename)
        '                        Case "COLUMNS"
        '                            Column = Split(DT.Text, vbTab)
        '                            ColumnCnt = UBound(Column) + 1
        '                            InsertSQLNameList = "," & Join(Column, ",") & ","
        '                            InsertSQLNameList = Replace(InsertSQLNameList, "," & KeyField & ",", ",", 1, -1, vbTextCompare)
        '                            InsertSQLNameList = Replace(InsertSQLNameList, ",,", ",")
        '                            InsertSQLNameList = Mid(InsertSQLNameList, 2, Len(InsertSQLNameList) - 2)
        '                        Case "ROW"
        '                            Row = Split(DT.Text, vbTab)
        '                            RecordKey = ""
        '                            For ColumnPtr = 0 To ColumnCnt - 1
        '                                If UCase(Column(ColumnPtr)) = KeyField Then
        '                                    '
        '                                    ' Key field
        '                                    '
        '                                    RecordKey = Row(ColumnPtr)
        '                                Else
        '                                    '
        '                                    ' non-Key field
        '                                    '
        '                                    UpdateSQL = UpdateSQL & "," & Column(ColumnPtr) & "=" & EncodeSQLText(Row(ColumnPtr))
        '                                    InsertSQLValueList = InsertSQLValueList & "," & EncodeSQLText(Row(ColumnPtr))
        '                                End If
        '                            Next
        '                            If UpdateSQL <> "" Then
        '                                InsertSQLValueList = Mid(InsertSQLValueList, 2)
        '                                UpdateSQL = Mid(UpdateSQL, 2)
        '                            End If
        '                            If RecordKey <> "" Then
        '                                '
        '                                ' Key field available
        '                                '
        '                                SelectSQL = "select ID from " & TableName & " where " & KeyField & "=" & EncodeSQLText(RecordKey)
        '                                InsertSQL = "insert into " & TableName & "(" & InsertSQLNameList & ")values(" & InsertSQLValueList & ")"
        '                                RS = cmc.asv.csv_ExecuteSQL(DataSource, SelectSQL)
        '                                If isDataTableOk(RS) Then
        '                                    '
        '                                    ' Record found, run update
        '                                    '
        '                                    Call cmc.asv.csv_ExecuteSQL(DataSource, UpdateSQL)
        '                                Else
        '                                    '
        '                                    ' run insert
        '                                    '
        '                                    Call cmc.asv.csv_ExecuteSQL(DataSource, InsertSQL)
        '                                End If
        '                                'RS = Nothing
        '                            Else
        '                                '
        '                                ' no key field found, just do insert
        '                                '
        '                                Call cmc.asv.csv_ExecuteSQL(DataSource, InsertSQL)
        '                            End If
        '                    End Select
        '                    Call DT.GoNext()
        '                Loop

        '            End If
        '            '
        '            Exit Sub
        'ErrorTrap:
        '        End Sub
        Public Sub New(cpCore As cpCoreClass)
            MyBase.New()
            Me.CPcore = cpCore
        End Sub
    End Class
End Namespace
