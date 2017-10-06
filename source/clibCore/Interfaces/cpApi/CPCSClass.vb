Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)>
    <ComClass(CPCSClass.ClassId, CPCSClass.InterfaceId, CPCSClass.EventsId)>
    Public Class CPCSClass
        Inherits CPCSBaseClass
        Implements IDisposable

        '
#Region "COM GUIDs"
        Public Const ClassId As String = "63745D9C-795E-4C01-BD6D-4BA35FC4A843"
        Public Const InterfaceId As String = "3F1E7D2E-D697-47A8-A0D3-B625A906BF6A"
        Public Const EventsId As String = "04B8E338-ABB7-44FE-A8DF-2681A36DCA46"
#End Region
        '
        Private cpCore As Contensive.Core.coreClass
        Private cs As Integer
        Private OpeningMemberID As Integer
        Private cp As CPClass
        Protected disposed As Boolean = False
        '
        ' Constructor - Initialize the Main and Csv objects
        '
        Public Sub New(ByRef cpParent As CPClass)
            MyBase.New()
            cp = cpParent
            cpCore = cp.core
            cs = -1
            OpeningMemberID = cpCore.authContext.user.id
        End Sub
        '
        ' dispose
        '
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    Try
                        If cs <> -1 And True Then
                            Call cpCore.db.csClose(cs)
                        End If
                        'If Not (False) Then
                        '    Call cmc.asv.csv_CloseCS(CSPointer)
                        'End If
                    Catch ex As Exception
                    End Try
                    cpCore = Nothing
                    cp = Nothing
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
                'csv = Nothing
            End If
            Me.disposed = True
        End Sub
        '
        ' Insert, called only from Processor41.CSInsert after initializing 
        '
        Public Overrides Function Insert(ByVal ContentName As String) As Boolean
            Dim success As Boolean = False
            '
            Try
                If cs <> -1 Then
                    Call cpCore.db.csClose(cs)
                End If
                cs = cpCore.db.csInsertRecord(ContentName, OpeningMemberID)
                success = cpCore.db.csOk(cs)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.insert")
            End Try
            Return success
        End Function
        '
        '
        '
        Public Overrides Function OpenRecord(ByVal ContentName As String, ByVal recordId As Integer, Optional ByVal SelectFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True) As Boolean
            Dim success As Boolean = False
            '
            Try
                If cs <> -1 Then
                    Call cpCore.db.csClose(cs)
                End If
                cs = cpCore.db.csOpen(ContentName, "id=" & recordId, , ActiveOnly, , , , SelectFieldList, 1, 1)
                success = cpCore.db.csOk(cs)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.OpenRecord")
            End Try
            Return success
        End Function
        '
        '
        '
        Public Overrides Function Open(ByVal ContentName As String, Optional ByVal SQLCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal SelectFieldList As String = "", Optional ByVal pageSize As Integer = 0, Optional ByVal PageNumber As Integer = 1) As Boolean
            'Public Overrides Function Open(ByVal ContentName As String, Optional ByVal SQLCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal SelectFieldList As String = "", Optional ByVal ignore As Integer = 10, Optional ByVal PageNumber As Integer = 1, Optional pageSize As Integer = 0) As Boolean
            Dim success As Boolean = False
            '
            Try
                If cs <> -1 Then
                    Call cpCore.db.csClose(cs)
                End If
                cs = cpCore.db.csOpen(ContentName, SQLCriteria, SortFieldList, ActiveOnly, , , , SelectFieldList, pageSize, PageNumber)
                success = cpCore.db.csOk(cs)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.Open") : Throw
            End Try
            Return success
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function OpenGroupUsers(ByVal GroupList As List(Of String), Optional ByVal SQLCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 10, Optional ByVal PageNumber As Integer = 1) As Boolean
            Dim success As Boolean = False
            '
            Try
                If cs <> -1 Then
                    Call cpCore.db.csClose(cs)
                End If
                cs = cpCore.db.csOpenGroupUsers(GroupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber)
                success = Ok()
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.OpenGroupUsers")
            End Try
            Return success
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function OpenGroupUsers(ByVal GroupName As String, Optional ByVal SQLCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 10, Optional ByVal PageNumber As Integer = 1) As Boolean
            Dim success As Boolean = False
            '
            Try
                Dim groupList As New List(Of String)
                groupList.Add(GroupName)
                If cs <> -1 Then
                    Call cpCore.db.csClose(cs)
                End If
                cs = cpCore.db.csOpenGroupUsers(groupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber)
                success = Ok()
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.OpenGroupUsers")
            End Try
            Return success
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function OpenGroupListUsers(ByVal GroupCommaList As String, Optional ByVal SQLCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 10, Optional ByVal PageNumber As Integer = 1) As Boolean
            Dim groupList As New List(Of String)
            '
            groupList.AddRange(GroupCommaList.Split(","))
            OpenGroupListUsers = OpenGroupUsers(groupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function OpenSQL(ByVal sql As String) As Boolean
            Dim success As Boolean = False
            '
            Try
                If cs <> -1 Then
                    Call cpCore.db.csClose(cs)
                End If
                cs = cpCore.db.csOpenSql_rev("default", sql)
                success = cpCore.db.csOk(cs)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.OpenSQL")
            End Try
            Return success
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function OpenSQL(ByVal sql As String, ByVal DataSourcename As String, Optional ByVal PageSize As Integer = 10, Optional ByVal PageNumber As Integer = 1) As Boolean
            Dim success As Boolean = False
            'Dim swap As String
            '
            Try
                If cs <> -1 Then
                    Call cpCore.db.csClose(cs)
                End If
                If ((sql = "") Or (sql.ToLower = "default")) And (DataSourcename <> "") And (DataSourcename.ToLower <> "default") Then
                    '
                    ' support legacy calls were the arguments were was backwards (datasourcename is sql and vise-versa)
                    '
                    cs = cpCore.db.csOpenSql(sql, DataSourcename, PageSize, PageNumber)
                Else
                    cs = cpCore.db.csOpenSql(sql, DataSourcename, PageSize, PageNumber)
                End If
                success = cpCore.db.csOk(cs)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.OpenSQL")
            End Try
            Return success
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function OpenSQL2(ByVal sql As String, Optional ByVal DataSourcename As String = "default", Optional ByVal PageSize As Integer = 10, Optional ByVal PageNumber As Integer = 1) As Boolean
            Dim success As Boolean = False
            'Dim swap As String
            '
            Try
                If cs <> -1 Then
                    Call cpCore.db.csClose(cs)
                End If
                If ((sql = "") Or (sql.ToLower = "default")) And (DataSourcename <> "") And (DataSourcename.ToLower <> "default") Then
                    '
                    ' support legacy calls were the arguments were was backwards (datasourcename is sql and vise-versa)
                    '
                    cs = cpCore.db.csOpenSql(sql, DataSourcename, PageSize, PageNumber)
                Else
                    cs = cpCore.db.csOpenSql(sql, DataSourcename, PageSize, PageNumber)
                End If
                success = cpCore.db.csOk(cs)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.OpenSQL")
            End Try
            Return success
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Sub Close()
            Try
                If cs <> -1 Then
                    Call cpCore.db.csClose(cs)
                    cs = -1
                End If
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.Close")
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Function GetFormInput(ByVal ContentName As String, ByVal FieldName As String, Optional ByVal Height As String = "", Optional ByVal Width As String = "", Optional ByVal HtmlId As String = "") As Object
            If True Then
                Return cpCore.html.html_GetFormInputCS(cs, ContentName, FieldName, Height, Width, HtmlId)
            Else
                Return ""
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Sub Delete()
            Try
                Call cpCore.db.csDeleteRecord(cs)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.Delete")
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Function FieldOK(ByVal FieldName As String) As Boolean
            Dim result As Boolean = False
            '
            Try
                result = cpCore.db.cs_isFieldSupported(cs, FieldName)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.FieldOK")
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Sub GoFirst()
            Try
                Call cpCore.db.cs_goFirst(cs, False)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.Delete")
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Function GetAddLink(Optional ByVal PresetNameValueList As String = "", Optional ByVal AllowPaste As Boolean = False) As String
            Dim result As Object
            '
            Try
                result = cpCore.html.main_cs_getRecordAddLink(cs, PresetNameValueList, AllowPaste)
                If result Is Nothing Then
                    result = String.Empty
                End If
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.GetAddLink")
                result = String.Empty
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetBoolean(ByVal FieldName As String) As Boolean
            Dim result As Boolean = False
            '
            Try
                result = cpCore.db.csGetBoolean(cs, FieldName)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.GetBoolean")
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetDate(ByVal FieldName As String) As Date
            Try
                Return cpCore.db.csGetDate(cs, FieldName)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return Date.MinValue
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetEditLink(Optional ByVal allowCut As Boolean = False) As String
            Try
                Return cpCore.db.csGetRecordEditLink(cs, allowCut)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return String.Empty
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetFilename(ByVal fieldName As String, Optional ByVal OriginalFilename As String = "", Optional ByVal ContentName As String = "") As String
            Try
                Return cpCore.db.csGetFilename(cs, fieldName, OriginalFilename, ContentName)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return String.Empty
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetInteger(ByVal FieldName As String) As Integer
            Try
                Return cpCore.db.csGetInteger(cs, FieldName)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return String.Empty
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetNumber(ByVal FieldName As String) As Double
            Try
                Return cpCore.db.csGetNumber(cs, FieldName)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return String.Empty
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetRowCount() As Integer
            Try
                Return cpCore.db.csGetRowCount(cs)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return String.Empty
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetSQL() As String
            Try
                Return cpCore.db.csGetSource(cs)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return String.Empty
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetText(ByVal FieldName As String) As String
            Try
                Return cpCore.db.csGet(cs, FieldName)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return String.Empty
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetHtml(ByVal FieldName As String) As String
            Try
                Return cpCore.db.csGet(cs, FieldName)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return String.Empty
        End Function
        '
        '====================================================================================================
        '
        <Obsolete("Use getText for copy. getFilename for filename", False)> Public Overrides Function GetTextFile(ByVal FieldName As String) As String
            Try
                Return cpCore.db.csGetText(cs, FieldName)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return String.Empty
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Sub GoNext()
            Try
                cpCore.db.csGoNext(cs)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Function NextOK() As Boolean
            Try
                Call cpCore.db.csGoNext(cs)
                Return cpCore.db.csOk(cs)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return False
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function Ok() As Boolean
            Try
                Return cpCore.db.csOk(cs)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return False
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Sub Save()
            Try
                cpCore.db.csSave2(cs)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub SetField(ByVal FieldName As String, ByVal FieldValue As Object)
            Try
                Call cpCore.db.csSet(cs, FieldName, CDate(FieldValue))
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.SetField")
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub SetField(ByVal FieldName As String, ByVal FieldValue As String)
            Try
                Call cpCore.db.csSet(cs, FieldName, FieldValue)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.SetField")
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub SetFile(ByVal FieldName As String, ByVal Copy As String, ByVal ContentName As String)
            Try
                Call cpCore.db.csSetTextFile(cs, FieldName, Copy, ContentName)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.SetFile")
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub SetFormInput(ByVal FieldName As String, Optional ByVal RequestName As String = "")
            Dim success As Boolean = False
            Try
                Call csController.cs_setFormInput(cpCore, cs, FieldName, RequestName)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw ' "Unexpected error in cs.SetFormInput")
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Return the value in the field
        ''' </summary>
        ''' <param name="fieldName"></param>
        ''' <returns></returns>
        Public Overrides Function GetValue(fieldName As String) As String
            Return cpCore.db.cs_getValue(cs, fieldName)
        End Function

#Region " IDisposable Support "
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region
    End Class

End Namespace

