Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices
Imports ADODB

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)>
    <ComClass(CPDbClass.ClassId, CPDbClass.InterfaceId, CPDbClass.EventsId)>
    Public Class CPDbClass
        Inherits BaseClasses.CPDbBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "13B707F9-190A-4ECD-8A53-49FE1D8CAE54"
        Public Const InterfaceId As String = "A20CF53D-1FB7-41C4-8493-3D5AC3671A5E"
        Public Const EventsId As String = "F68D64F0-595F-4CA2-9EC6-6DBA9A7E458B"
#End Region
        '
        Private cp As CPClass
        '
        Public Enum remoteQueryType
            sql = 1
            openContent = 2
            updateContent = 3
            insertContent = 4
        End Enum
        '
        '====================================================================================================
        ''' <summary>
        ''' Construct
        ''' </summary>
        ''' <param name="cpParent"></param>
        Public Sub New(ByRef cpParent As CPClass)
            MyBase.New()
            cp = cpParent
        End Sub
        '
        '====================================================================================================
        '
        Private Sub reportClassError(ByVal ex As Exception, ByVal methodName As String)
            Try
                Call cp.core.handleExceptionAndRethrow(ex, "Unexpected Trap Error in CP.DB." & methodName)
            Catch exInner As Exception
                '
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub Delete(ByVal DataSourcename As String, ByVal TableName As String, ByVal RecordId As Integer)
            Call cp.core.db.deleteTableRecord(TableName, RecordId, DataSourcename)
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Function GetConnectionString(ByVal DataSourcename As String) As String
            Return cp.core.db.getConnectionStringADONET(cp.core.appConfig.name, DataSourcename)
        End Function
        '
        '====================================================================================================
        ' deprecated
        '
        <Obsolete("Use GetConnectionString( dataSourceName )")>
        Public Overrides Function DbGetConnectionString(ByVal DataSourcename As String) As String
            Return GetConnectionString(DataSourcename)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetDataSourceType(ByVal DataSourcename As String) As Integer
            Return cp.core.db.getDataSourceType(DataSourcename)
        End Function
        '
        '====================================================================================================
        ' deprecated
        '
        <Obsolete("Use GetDataSourceType( dataSourceName )")>
        Public Overrides Function DbGetDataSourceType(ByVal DataSourcename As String) As Integer
            DbGetDataSourceType = GetDataSourceType(DataSourcename)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetTableID(ByVal TableName As String) As Integer
            Return cp.core.GetTableID(TableName)
        End Function
        '
        '====================================================================================================
        ' deprecated 
        '
        <Obsolete("Use GetDataSourceType( dataSourceName )")>
        Public Overrides Function dbGetTableID(ByVal TableName As String) As Integer
            Return GetTableID(TableName)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function IsTable(ByVal DataSourcename As String, ByVal TableName As String) As Boolean
            Return cp.core.IsSQLTable(DataSourcename, TableName)
        End Function
        '
        '====================================================================================================
        ' deprecated
        '
        Public Overrides Function DbIsTable(ByVal DataSourcename As String, ByVal TableName As String) As Boolean
            Return IsTable(DataSourcename, TableName)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function IsTableField(ByVal DataSourcename As String, ByVal TableName As String, ByVal FieldName As String) As Boolean
            Return cp.core.IsSQLTableField(DataSourcename, TableName, FieldName)
        End Function
        '
        '====================================================================================================
        ' deprecated 
        '
        Public Overrides Function DbIsTableField(ByVal DataSourcename As String, ByVal TableName As String, ByVal FieldName As String) As Boolean
            Return IsTableField(DataSourcename, TableName, FieldName)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function EncodeSQLBoolean(ByVal SourceBoolean As Boolean) As String
            Return cp.core.db.encodeSQLBoolean(SourceBoolean)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function EncodeSQLDate(ByVal SourceDate As Date) As String
            Return cp.core.db.encodeSQLDate(SourceDate)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function EncodeSQLNumber(ByVal SourceNumber As Double) As String
            Return cp.core.db.encodeSQLNumber(SourceNumber)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function EncodeSQLText(ByVal SourceText As String) As String
            Return cp.core.db.encodeSQLText(SourceText)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' process a sql query to be executed remotely. Returns a key that is used in the future.
        ''' </summary>
        ''' <param name="sql"></param>
        ''' <param name="DataSourceName"></param>
        ''' <param name="pageSize"></param>
        ''' <returns></returns>
        Public Overrides Function GetRemoteQueryKey(ByVal sql As String, Optional ByVal DataSourceName As String = "Default", Optional ByVal pageSize As Integer = 100) As String
            Dim returnKey As String = ""
            Try
                Dim cs As Contensive.BaseClasses.CPCSBaseClass = cp.CSNew
                Dim dataSourceID As Integer
                '
                If pageSize = 0 Then
                    pageSize = 9999
                End If
                If cs.Insert("Remote Queries") Then
                    returnKey = Guid.NewGuid().ToString()
                    dataSourceID = cp.Content.GetRecordID("Data Sources", DataSourceName)
                    cs.SetField("remotekey", returnKey)
                    cs.SetField("datasourceid", dataSourceID.ToString)
                    cs.SetField("sqlquery", sql)
                    cs.SetField("maxRows", pageSize.ToString)
                    cs.SetField("dateexpires", Now.AddDays(1).ToString)
                    cs.SetField("QueryTypeID", remoteQueryType.sql.ToString)
                    cs.SetField("VisitID", cp.Visit.Id.ToString)
                End If
                Call cs.Close()
                '
            Catch ex As Exception
                Call cp.core.handleExceptionAndRethrow(ex)
            End Try
            Return returnKey
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' execute a sql query and return a recordset
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <param name="DataSourcename"></param>
        ''' <param name="Retries"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        <Obsolete("deprecated. Use ExecuteSql_GetRecordSet() or ExecuteSql_GetDataTable()", False)>
        Public Overrides Function ExecuteSQL(ByVal SQL As String, Optional ByVal DataSourcename As String = "Default", Optional ByVal Retries As String = "0", Optional ByVal PageSize As String = "10", Optional ByVal PageNumber As String = "1") As Object
            Return cp.core.db.executeSql_getRecordSet(SQL, DataSourcename, PageNumber * PageSize, PageSize)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Execute sql or command on the default datasource and return all rows in a datatable.
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <returns></returns>
        Public Overrides Function ExecuteSQL_GetDataTable(SQL As String) As DataTable
            Return cp.core.db.executeSql(SQL)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Execute sql or command on the default datasource and return MaxRows in a datatable.
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <param name="MaxRows"></param>
        ''' <returns></returns>
        Public Overrides Function ExecuteSQL_GetDataTable(SQL As String, MaxRows As Integer) As DataTable
            Return cp.core.db.executeSql(SQL, "", 0, MaxRows)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Execute sql or command on a specific datasource and return all rows in a datatable. The default datasource is either "", or "default"
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <param name="DataSourcename"></param>
        ''' <returns></returns>
        Public Overrides Function ExecuteSQL_GetDataTable(SQL As String, DataSourcename As String) As DataTable
            Return cp.core.db.executeSql(SQL, DataSourcename)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Execute sql or command on a specific datasource and return MaxRows starting on page PageNumber (0 based) as a datatable.
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <param name="DataSourcename"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        Public Overrides Function ExecuteSQL_GetDataTable(SQL As String, DataSourcename As String, MaxRows As Integer, PageSize As Integer, PageNumber As Integer) As DataTable
            Return cp.core.db.executeSql(SQL, "", (PageSize * PageNumber), MaxRows)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Execute sql or command on the default datasource and return all rows started on page pageNumber (0 based) as a datatable. Limit the number of rows within the query. The default datasource is either "", or "default"
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        Public Overrides Function ExecuteSQL_GetDataTable(SQL As String, MaxRows As Integer, PageSize As Integer, PageNumber As Integer) As DataTable
            Throw New NotImplementedException()
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Execute sql or command on the default datasource and return all rows a recordset (ADODB).
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <returns></returns>
        Public Overrides Function ExecuteSQL_GetRecordSet(SQL As String) As Recordset
            Throw New NotImplementedException()
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Execute sql or command on the default datasource and return MaxRows in a recordset (ADODB)
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <param name="MaxRows"></param>
        ''' <returns></returns>
        Public Overrides Function ExecuteSQL_GetRecordSet(SQL As String, MaxRows As Integer) As Recordset
            Throw New NotImplementedException()
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Execute sql or command on a sepcific datasource and a recordset (ADODB). The default datasource is either "", or "default"
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <param name="DataSourcename"></param>
        ''' <returns></returns>
        Public Overrides Function ExecuteSQL_GetRecordSet(SQL As String, DataSourcename As String) As Recordset
            Return cp.core.db.executeSql_getRecordSet(SQL, DataSourcename, 0, -1)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Execute sql or command on the default datasource and return all rows started on page pageNumber (0 based) as a datatable. Limit the number of rows within the query. The default datasource is either "", or "default"
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <param name="MaxRows"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        Public Overrides Function ExecuteSQL_GetRecordSet(SQL As String, MaxRows As Integer, PageSize As Integer, PageNumber As Integer) As Recordset
            Return cp.core.db.executeSql_getRecordSet(SQL, "", (PageSize * PageNumber), MaxRows)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Execute sql or command on a specific datasource and return all rows started on page pageNumber (0 based) as a datatable. Limit the number of rows within the query. The default datasource is either "", or "default"
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <param name="DataSourcename"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        Public Overrides Function ExecuteSQL_GetRecordSet(SQL As String, DataSourcename As String, MaxRows As Integer, PageSize As Integer, PageNumber As Integer) As Recordset
            Return cp.core.db.executeSql_getRecordSet(SQL, DataSourcename, (PageSize * PageNumber), MaxRows)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Property SQLTimeout() As Integer
            Get
                Return cp.core.db.sqlCommandTimeout
            End Get
            Set(ByVal value As Integer)
                cp.core.db.sqlCommandTimeout = value
            End Set
        End Property
        '
        '====================================================================================================
        '
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDbDebug.log", Now & " - cp.db, " & copy & vbCrLf, True)
        End Sub
        '
        '
        '
#Region " IDisposable Support "
        '
        Protected disposed As Boolean = False
        '====================================================================================================
        ''' <summary>
        ''' Dispose
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Call appendDebugLog(".dispose, dereference main, csv")
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub
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