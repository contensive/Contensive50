
Option Explicit On
Option Strict On

Imports System.Data.SqlClient
Imports System.Text.RegularExpressions

Namespace Contensive.Core.Controllers
    Public Class dbEngineController
        Implements IDisposable
        '
        ' objects passed in that are not disposed
        '
        Private cpCore As coreClass
        '
        ' internal storage
        '
        Private dbEnabled As Boolean = True                                    ' set true when configured and tested - else db calls are skipped
        '
        '==========================================================================================
        ''' <summary>
        ''' app services constructor
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Try
                Me.cpCore = cpCore
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' return the correctly formated connection string for this datasource. Called only from within this class
        ''' </summary>
        ''' <returns>
        ''' </returns>
        Public Function getConnectionStringADONET() As String
            '
            ' (OLEDB) OLE DB Provider for SQL Server > "Provider=sqloledb;Data Source=MyServerName;Initial Catalog=MyDatabaseName;User Id=MyUsername;Password=MyPassword;"
            '     https://www.codeproject.com/Articles/2304/ADO-Connection-Strings#OLE%20DB%20SqlServer
            '
            ' (OLEDB) Microsoft OLE DB Provider for SQL Server connection strings > "Provider=sqloledb;Data Source=myServerAddress;Initial Catalog=myDataBase;User Id = myUsername;Password=myPassword;"
            '     https://www.connectionstrings.com/microsoft-ole-db-provider-for-sql-server-sqloledb/
            '
            ' (ADONET) .NET Framework Data Provider for SQL Server > Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password = myPassword;
            '     https://www.connectionstrings.com/sql-server/
            '
            Dim returnConnString As String = ""
            Try
                Dim serverUrl As String = cpCore.serverConfig.defaultDataSourceAddress
                If (serverUrl.IndexOf(":") > 0) Then
                    serverUrl = serverUrl.Substring(0, serverUrl.IndexOf(":"))
                End If
                returnConnString &= "" _
                    & "server=" & serverUrl & ";" _
                    & "User Id=" & cpCore.serverConfig.defaultDataSourceUsername & ";" _
                    & "Password=" & cpCore.serverConfig.defaultDataSourcePassword & ";" _
                    & ""
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnConnString
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Create a new catalog in the database
        ''' </summary>
        ''' <param name="catalogName"></param>
        Public Sub createCatalog(catalogName As String)
            Try
                executeSql("create database " + catalogName)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Check if the database exists
        ''' </summary>
        ''' <param name="catalog"></param>
        ''' <returns></returns>
        Public Function checkCatalogExists(catalog As String) As Boolean
            Dim returnOk As Boolean = False
            Try
                Dim sql As String
                Dim databaseId As Integer = 0
                Dim dt As DataTable
                '
                sql = String.Format("SELECT database_id FROM sys.databases WHERE Name = '{0}'", catalog)
                dt = executeSql(sql)
                returnOk = (dt.Rows.Count > 0)
                dt.Dispose()
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnOk
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Execute a command (sql statemwent) and return a dataTable object
        ''' </summary>
        ''' <param name="sql"></param>
        ''' <param name="dataSourceName"></param>
        ''' <param name="startRecord"></param>
        ''' <param name="maxRecords"></param>
        ''' <returns></returns>
        Private Function executeSql(ByVal sql As String) As DataTable
            Dim returnData As New DataTable
            Try
                Dim connString As String = getConnectionStringADONET()
                If dbEnabled Then
                    Using connSQL As New SqlConnection(connString)
                        connSQL.Open()
                        Using cmdSQL As New SqlCommand()
                            cmdSQL.CommandType = Data.CommandType.Text
                            cmdSQL.CommandText = sql
                            cmdSQL.Connection = connSQL
                            Using adptSQL = New SqlClient.SqlDataAdapter(cmdSQL)
                                adptSQL.Fill(returnData)
                            End Using
                        End Using
                    End Using
                End If
            Catch ex As Exception
                Dim newEx As New ApplicationException("Exception [" & ex.Message & "] executing master sql [" & sql & "]", ex)
                cpCore.handleExceptionAndContinue(newEx)
            End Try
            Return returnData
        End Function
#Region " IDisposable Support "
        Protected disposed As Boolean = False
        '
        '==========================================================================================
        ''' <summary>
        ''' dispose
        ''' </summary>
        ''' <param name="disposing"></param>
        ''' <remarks></remarks>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    '
                    ' ----- call .dispose for managed objects
                    '
                    '
                    ' ----- Close all open csv_ContentSets, and make sure the RS is killed
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

