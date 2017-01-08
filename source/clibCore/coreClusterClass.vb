
Option Explicit On
Option Strict On

Imports System.Data.SqlClient

Namespace Contensive.Core
    '
    '====================================================================================================
    ''' <summary>
    ''' cluster srervices - properties and methods to maintain the cluster. Applications do not have access to this. 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class coreClusterClass
        Implements IDisposable
        '
        ' ----- objects passed in constructor, do not dispose
        '
        Private cpCore As coreClass
        '
        '========================================================================
        ''' <summary>
        ''' Constructor builds data. read from cache and deserialize, if not in cache, build it from scratch, eventually, setup public properties as indivisual lazyCache
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As coreClass)
            '
            ' called during core constructor - so cp.core is not valid
            '
            MyBase.New()
            Me.cpCore = cpCore
            Try
                _ok = True
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '===================================================================================================
        ''' <summary>
        ''' file object pointed to the cluster folder in the serverconfig file. Used initially to boot the cluster.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property localClusterFiles As coreFileSystemClass
            Get
                If (_localClusterFiles Is Nothing) Then
                    localClusterFiles = New coreFileSystemClass(cpCore, cpCore.clusterConfig.isLocal, coreFileSystemClass.fileSyncModeEnum.activeSync, cpCore.serverConfig.clusterPath)
                End If
                Return _localClusterFiles
            End Get
        End Property
        Private _localClusterFiles As coreFileSystemClass
        '
        '====================================================================================================
        ''' <summary>
        ''' ok - means the class has initialized and methods can be used to maintain the cluser
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ok As Boolean
            Get
                Return _ok
            End Get
        End Property
        Private _ok As Boolean = False
        '
        '====================================================================================================
        ''' <summary>
        ''' physical path to the head of the local data storage
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property localDataPath As String
            Get
                If (cpCore.clusterConfig Is Nothing) Then
                    Return ""
                Else
                    Return cpCore.serverConfig.clusterPath
                End If
            End Get
        End Property
        '
        Public ReadOnly Property localAppsPath As String
            Get
                If (cpCore.clusterConfig Is Nothing) Then
                    Return ""
                Else
                    Return cpCore.serverConfig.clusterPath & "apps\"
                End If
            End Get
        End Property
        '
        '====================================================================================================
        ''' <summary>
        ''' return the correctly formated connection string for this datasource
        ''' </summary>
        ''' <returns></returns>
        Public Function getConnectionString(catalogName As String) As String
            Dim returnString As String = ""
            Try
                Dim dataSourceUrl As String
                dataSourceUrl = cpCore.clusterConfig.defaultDataSourceAddress
                If (dataSourceUrl.IndexOf(":") > 0) Then
                    dataSourceUrl = dataSourceUrl.Substring(0, dataSourceUrl.IndexOf(":"))
                End If
                '
                returnString = "" _
                    & "data source=" & dataSourceUrl & ";" _
                    & "UID=" & cpCore.clusterConfig.defaultDataSourceUsername & ";" _
                    & "PWD=" & cpCore.clusterConfig.defaultDataSourcePassword & ";" _
                    & ""
                If Not String.IsNullOrEmpty(catalogName) Then
                    returnString &= "initial catalog=" & catalogName & ";"
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnString
        End Function
        '
        ' execute sql on default connection and return datatable
        '
        Public Function db_executeSql(ByVal sql As String, Optional ByVal startRecord As Integer = 0, Optional ByVal maxRecords As Integer = 9999) As DataTable
            Dim returnData As New DataTable
            Try
                Dim connString As String = getConnectionString("")
                Using connSQL As New SqlConnection(connString)
                    connSQL.Open()
                    Using cmdSQL As New SqlCommand()
                        cmdSQL.CommandType = Data.CommandType.Text
                        cmdSQL.CommandText = sql
                        cmdSQL.Connection = connSQL
                        Using adptSQL = New SqlClient.SqlDataAdapter(cmdSQL)
                            adptSQL.Fill(startRecord, maxRecords, returnData)
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnData
        End Function
        '
        ' execute sql on default connection and return datatable
        '
        Public Sub db_executeSqlAsync(ByVal sql As String)
            Try
                Dim connString As String = getConnectionString("")
                Using connSQL As New SqlConnection(connString)
                    connSQL.Open()
                    Using cmdSQL As New SqlCommand()
                        cmdSQL.CommandType = Data.CommandType.Text
                        cmdSQL.CommandText = sql
                        cmdSQL.Connection = connSQL
                        cmdSQL.BeginExecuteNonQuery()
                    End Using
                End Using
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        ' verify database exists
        '
        Public Function checkDatabaseExists(databaseName As String) As Boolean
            Dim returnOk As Boolean = False
            Try
                Dim sql As String
                Dim databaseId As Integer = 0
                Dim dt As DataTable
                '
                sql = String.Format("SELECT database_id FROM sys.databases WHERE Name = '{0}'", databaseName)
                dt = db_executeSql(sql)
                returnOk = (dt.Rows.Count > 0)
                dt.Dispose()
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnOk
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' save config changes to the clusterConfig.json file
        ''' </summary>
        Public Sub saveConfig()
            Dim jsonTemp As String = cpCore.json.Serialize(cpCore.clusterConfig)
            localClusterFiles.saveFile("clusterConfig.json", jsonTemp)
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' handle exceptions in this class
        ''' </summary>
        ''' <param name="ex"></param>
        ''' <param name="methodName"></param>
        ''' <param name="Cause"></param>
        ''' <remarks></remarks>
        Private Sub handleClassException(ByVal ex As Exception, ByVal methodName As String, ByVal Cause As String)
            cpCore.handleExceptionAndRethrow(ex, "Unexpected exception in clusterServicesClass." & methodName & ", cause=[" & Cause & "]")
        End Sub



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
                    '
                    ' call .dispose for managed objects
                    '
                    'If Not (AddonObj Is Nothing) Then AddonObj.Dispose()
                End If
                '
                ' cleanup non-managed objects
                '
            End If
        End Sub
#End Region
    End Class
End Namespace
