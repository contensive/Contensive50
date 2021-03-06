Imports System.Data
'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    ''' <summary>
    ''' CP.Db - This object references the database directly
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class CPDbBaseClass
        Public MustOverride Sub Delete(ByVal DataSourcename As String, ByVal TableName As String, ByVal RecordId As Integer)
        Public MustOverride Function GetConnectionString(ByVal DataSourcename As String) As String
        Public MustOverride Function GetDataSourceType(ByVal DataSourcename As String) As Integer
        Public MustOverride Function GetTableID(ByVal TableName As String) As Integer
        Public MustOverride Function IsTable(ByVal DataSourcename As String, ByVal TableName As String) As Boolean
        Public MustOverride Function IsTableField(ByVal DataSourcename As String, ByVal TableName As String, ByVal FieldName As String) As Boolean
        Public MustOverride Function EncodeSQLBoolean(ByVal SourceBoolean As Boolean) As String
        Public MustOverride Function EncodeSQLDate(ByVal SourceDate As Date) As String
        Public MustOverride Function EncodeSQLNumber(ByVal SourceNumber As Double) As String
        Public MustOverride Function EncodeSQLText(ByVal SourceText As String) As String
        Public MustOverride Function ExecuteSQL(ByVal SQL As String, Optional ByVal DataSourcename As String = "Default", Optional ByVal Retries As String = "0", Optional ByVal PageSize As String = "10", Optional ByVal PageNumber As String = "1") As Object
        '
        ' need: executeQuery() returns dataTable
        ' need: executeNonQuery() returns recordsAffected
        ' need: executeNonQueryAsync() returns nothing
        '
        'Public MustOverride Function ExecuteSQL_GetRecordSet(ByVal SQL As String) As ADODB.Recordset
        'Public MustOverride Function ExecuteSQL_GetRecordSet(ByVal SQL As String, ByVal MaxRows As Integer) As ADODB.Recordset
        'Public MustOverride Function ExecuteSQL_GetRecordSet(ByVal SQL As String, ByVal MaxRows As Integer, ByVal PageSize As Integer, ByVal PageNumber As Integer) As ADODB.Recordset
        'Public MustOverride Function ExecuteSQL_GetRecordSet(ByVal SQL As String, ByVal DataSourcename As String) As ADODB.Recordset
        'Public MustOverride Function ExecuteSQL_GetRecordSet(ByVal SQL As String, ByVal DataSourcename As String, ByVal MaxRows As Integer, ByVal PageSize As Integer, ByVal PageNumber As Integer) As ADODB.Recordset
        'Public MustOverride Function ExecuteSQL_GetDataTable(ByVal SQL As String) As DataTable
        'Public MustOverride Function ExecuteSQL_GetDataTable(ByVal SQL As String, ByVal MaxRows As Integer) As DataTable
        'Public MustOverride Function ExecuteSQL_GetDataTable(ByVal SQL As String, ByVal MaxRows As Integer, PageSize As Integer, ByVal PageNumber As Integer) As DataTable
        'Public MustOverride Function ExecuteSQL_GetDataTable(ByVal SQL As String, ByVal DataSourcename As String) As DataTable
        'Public MustOverride Function ExecuteSQL_GetDataTable(ByVal SQL As String, ByVal DataSourcename As String, ByVal MaxRows As Integer, ByVal PageSize As Integer, ByVal PageNumber As Integer) As DataTable
        Public MustOverride Property SQLTimeout() As Integer
        Public MustOverride Function GetRemoteQueryKey(ByVal sql As String, Optional ByVal DataSourceName As String = "Default", Optional ByVal pageSize As Integer = 100) As String
        '
        ' deprecated
        '
        Public MustOverride Function DbGetConnectionString(ByVal DataSourcename As String) As String
        Public MustOverride Function DbGetDataSourceType(ByVal DataSourcename As String) As Integer
        Public MustOverride Function DbGetTableID(ByVal TableName As String) As Integer
        Public MustOverride Function DbIsTable(ByVal DataSourcename As String, ByVal TableName As String) As Boolean
        Public MustOverride Function DbIsTableField(ByVal DataSourcename As String, ByVal TableName As String, ByVal FieldName As String) As Boolean
    End Class

End Namespace

