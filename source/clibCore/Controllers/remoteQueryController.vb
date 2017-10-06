
Option Explicit On
Option Strict On
'
Imports System.Text.RegularExpressions
'
Namespace Contensive.Core.Controllers
    '
    '====================================================================================================
    ''' <summary>
    ''' static class controller
    ''' </summary>
    Public Class remoteQueryController
        Implements IDisposable
        '
        '
        '
        '
        Public Shared Function main_GetRemoteQueryKey(cpCore As coreClass, ByVal SQL As String, Optional ByVal DataSourceName As String = "", Optional ByVal maxRows As Integer = 1000) As String
            '
            Dim CS As Integer
            Dim RemoteKey As String = ""
            Dim DataSourceID As Integer
            '
            If maxRows = 0 Then
                maxRows = 1000
            End If
            CS = cpCore.db.csInsertRecord("Remote Queries")
            If cpCore.db.csOk(CS) Then
                RemoteKey = Guid.NewGuid.ToString()
                DataSourceID = cpCore.db.getRecordID("Data Sources", DataSourceName)
                Call cpCore.db.csSet(CS, "remotekey", RemoteKey)
                Call cpCore.db.csSet(CS, "datasourceid", DataSourceID)
                Call cpCore.db.csSet(CS, "sqlquery", SQL)
                Call cpCore.db.csSet(CS, "maxRows", maxRows)
                Call cpCore.db.csSet(CS, "dateexpires", cpCore.db.encodeSQLDate(cpCore.profileStartTime.AddDays(1)))
                Call cpCore.db.csSet(CS, "QueryTypeID", QueryTypeSQL)
                Call cpCore.db.csSet(CS, "VisitId", cpCore.authContext.visit.id)
            End If
            Call cpCore.db.csClose(CS)
            '
            Return RemoteKey
        End Function
        '
        '
        '
        Public Shared Function main_FormatRemoteQueryOutput(cpCore As coreClass, gd As GoogleDataType, RemoteFormat As RemoteFormatEnum) As String
            '
            Dim s As stringBuilderLegacyController
            Dim ColDelim As String
            Dim RowDelim As String
            Dim ColPtr As Integer
            Dim RowPtr As Integer
            '
            ' Select output format
            '
            s = New stringBuilderLegacyController
            Select Case RemoteFormat
                Case RemoteFormatEnum.RemoteFormatJsonNameValue
                    '
                    '
                    '
                    Call s.Add("{")
                    If Not gd.IsEmpty Then
                        ColDelim = ""
                        For ColPtr = 0 To UBound(gd.col)
                            Call s.Add(ColDelim & gd.col(ColPtr).Id & ":'" & gd.row(0).Cell(ColPtr).v & "'")
                            ColDelim = ","
                        Next
                    End If
                    Call s.Add("}")
                Case RemoteFormatEnum.RemoteFormatJsonNameArray
                    '
                    '
                    '
                    Call s.Add("{")
                    If Not gd.IsEmpty Then
                        ColDelim = ""
                        For ColPtr = 0 To UBound(gd.col)
                            Call s.Add(ColDelim & gd.col(ColPtr).Id & ":[")
                            ColDelim = ","
                            RowDelim = ""
                            For RowPtr = 0 To UBound(gd.row)
                                With gd.row(RowPtr).Cell(ColPtr)
                                    s.Add(RowDelim & "'" & .v & "'")
                                    RowDelim = ","
                                End With
                            Next
                            Call s.Add("]")
                        Next
                    End If
                    Call s.Add("}")
                Case RemoteFormatEnum.RemoteFormatJsonTable
                    '
                    '
                    '
                    Call s.Add("{")
                    If Not gd.IsEmpty Then
                        Call s.Add("cols: [")
                        ColDelim = ""
                        For ColPtr = 0 To UBound(gd.col)
                            With gd.col(ColPtr)
                                Call s.Add(ColDelim & "{id: '" & genericController.EncodeJavascript(.Id) & "', label: '" & genericController.EncodeJavascript(.Label) & "', type: '" & genericController.EncodeJavascript(.Type) & "'}")
                                ColDelim = ","
                            End With
                        Next
                        Call s.Add("],rows:[")
                        RowDelim = ""
                        For RowPtr = 0 To UBound(gd.row)
                            s.Add(RowDelim & "{c:[")
                            RowDelim = ","
                            ColDelim = ""
                            For ColPtr = 0 To UBound(gd.col)
                                With gd.row(RowPtr).Cell(ColPtr)
                                    Call s.Add(ColDelim & "{v: '" & genericController.EncodeJavascript(.v) & "'}")
                                    ColDelim = ","
                                End With
                            Next
                            s.Add("]}")
                        Next
                        Call s.Add("]")
                    End If
                    Call s.Add("}")
            End Select
            main_FormatRemoteQueryOutput = s.Text
            '
        End Function
        '
        '====================================================================================================
#Region " IDisposable Support "
        '
        ' this class must implement System.IDisposable
        ' never throw an exception in dispose
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        '====================================================================================================
        '
        Protected disposed As Boolean = False
        '
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        '
        Protected Overrides Sub Finalize()
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(False)
            MyBase.Finalize()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' dispose.
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Me.disposed = True
                If disposing Then
                    'If (cacheClient IsNot Nothing) Then
                    '    cacheClient.Dispose()
                    'End If
                End If
                '
                ' cleanup non-managed objects
                '
            End If
        End Sub
#End Region
    End Class
    '
End Namespace