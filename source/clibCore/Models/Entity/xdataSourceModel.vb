
'Option Explicit On
'Option Strict On

'Imports System
'Imports System.Collections.Generic
'Imports System.Text
'Imports Contensive.BaseClasses

'Namespace Contensive.Core.Models.Entity
'    '
'    '====================================================================================================
'    ' cached entity model pattern
'    '   factory pattern creator, constructor is a shared method that returns a loaded object
'    '   new() - to allow deserialization (so all methods must pass in cp)
'    '   shared getObject( cp, id ) - returns loaded model
'    '   saveObject( cp ) - saves instance properties, returns the record id
'    '
'    Public Class xdataSourceModel
'        '
'        '-- const
'        Public Const primaryContentName As String = "data sources" '<------ set content name
'        Private Const primaryContentTableName As String = "ccDataSources" '<------ set to tablename for the primary content (used for cache names)
'        Private Const primaryContentDataSource As String = "default" '<----- set to datasource if not default
'        '
'        ' -- instance properties
'        Public id As Integer = 0
'        Public name As String = String.Empty
'        Public connStringOLEDB As String = String.Empty
'        Public dataSourceType As dataSourceTypeEnum
'        Public endPoint As String
'        Public username As String
'        Public password As String
'        '
'        ' -- list of tag names that will flush the cache
'        Public Shared ReadOnly Property cacheTagList As String = primaryContentName & ","
'        '
'        Public Enum dataSourceTypeEnum
'            sqlServerOdbc = 1
'            sqlServerNative = 2
'            mySqlNative = 3
'        End Enum
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' Create an empty object. needed for deserialization. Use newModel() method as constructor, includes cache
'        ''' </summary>
'        Public Sub New()
'            '
'        End Sub
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' Open existing
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="recordId"></param>
'        Public Shared Function getObject(cpcore As coreClass, recordId As Integer, ByRef adminMessageList As List(Of String)) As dataSourceModel
'            Dim returnModel As dataSourceModel = Nothing
'            Try
'                Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer
'                Dim recordCacheName As String = primaryContentName & "CachedModelRecordId" & recordId
'                Dim recordCache As String = cpcore.cache.getString(recordCacheName)
'                Dim loadDbModel As Boolean = True
'                If Not String.IsNullOrEmpty(recordCache) Then
'                    Try
'                        returnModel = json_serializer.Deserialize(Of dataSourceModel)(recordCache)
'                        '
'                        ' -- if model exposes any objects, verify they are created
'                        'If (returnModel.meetingItemList Is Nothing) Then
'                        '    returnModel.meetingItemList = New List(Of meetingItemModel)
'                        'End If
'                        loadDbModel = False
'                    Catch ex As Exception
'                        'ignore error - just roll through to rebuild model and save new cache
'                    End Try
'                End If
'                If loadDbModel Or (returnModel Is Nothing) Then
'                    returnModel = getObjectNoCache(cpcore, recordId)
'                    Call cpcore.cache.setObject(recordCacheName, json_serializer.Serialize(returnModel), cacheTagList)
'                End If
'            Catch ex As Exception
'                cpCore.handleExceptionAndContinue(ex) : Throw
'            End Try
'            Return returnModel
'        End Function
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' called only from getObject. Load the model from the Db without cache. If there are any properties or objects that cannot be used from cache, do not include them here either, load in getObject()
'        ''' </summary>
'        ''' <param name="recordId"></param>
'        Private Shared Function getObjectNoCache(cpcore As coreClass, recordId As Integer) As dataSourceModel
'            Dim returnNewModel As New dataSourceModel()
'            Try
'                Dim cs As New csController(cpcore)
'                returnNewModel.id = 0
'                If recordId <> 0 Then
'                    cs.open(primaryContentName, "(ID=" & recordId & ")", "name", True, "name,connString")
'                    If cs.ok() Then
'                        returnNewModel.id = recordId
'                        returnNewModel.name = normalizeDataSourceName(cs.getText("Name"))
'                        returnNewModel.connStringOLEDB = cs.getText("connString")
'                    End If
'                    Call cs.Close()
'                End If
'            Catch ex As Exception
'                cpCore.handleExceptionAndContinue(ex) : Throw
'            End Try
'            Return returnNewModel
'        End Function
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' Save the object
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <returns></returns>
'        Public Function saveObject(cpcore As coreClass) As Integer
'            Try
'                Dim cs As New csController(cpcore)
'                If (id > 0) Then
'                    If Not cs.open(primaryContentName, "id=" & id) Then
'                        id = 0
'                        cs.Close()
'                        Throw New ApplicationException("Unable to open record [" & id & "]")
'                    End If
'                Else
'                    If Not cs.Insert(primaryContentName) Then
'                        cs.Close()
'                        id = 0
'                        Throw New ApplicationException("Unable to insert record")
'                    End If
'                End If
'                If cs.ok() Then
'                    id = cs.getInteger("id")
'                    Call cs.setField("name", normalizeDataSourceName(name))
'                    Call cs.setField("connString", connStringOLEDB)
'                End If
'                Call cs.Close()
'            Catch ex As Exception
'                cpCore.handleExceptionAndContinue(ex) : Throw
'                Throw
'            End Try
'            Return id
'        End Function
'        '
'        Public Shared Function getDictionary(cpcore As coreClass) As Dictionary(Of String, dataSourceModel)
'            Dim result As New Dictionary(Of String, dataSourceModel)
'            Try
'                Dim sql As String = "select id,name,connString from ccDataSources order by id"
'                Using dt As DataTable = cpcore.db.executeSql(sql, "", 0, 1)
'                    If (dt Is Nothing) Then
'                        'Throw New ApplicationException("dataSourceName [" & dataSourceName & "] is not valid.")
'                    ElseIf (dt.Rows.Count = 0) Then
'                        'Throw New ApplicationException("dataSourceName [" & dataSourceName & "] is not valid.")
'                    Else
'                        Dim dataSource As New dataSourceModel
'                        Dim normalizedDataSourceName As String = normalizeDataSourceName(dt.Rows(0).Field(Of String)("name"))
'                        dataSource.id = dt.Rows(0).Field(Of Integer)("id")
'                        dataSource.name = normalizedDataSourceName
'                        dataSource.connStringOLEDB = dt.Rows(0).Field(Of String)("connString")
'                        result.Add(normalizedDataSourceName, dataSource)
'                    End If
'                End Using
'            Catch ex As Exception
'                cpCore.handleExceptionAndContinue(ex) : Throw
'            End Try
'            Return result
'        End Function
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' convert a datasource name into the key value used by the datasourcedictionary cache
'        ''' </summary>
'        ''' <param name="DataSourceName"></param>
'        ''' <returns></returns>
'        Public Shared Function normalizeDataSourceName(DataSourceName As String) As String
'            If Not String.IsNullOrEmpty(DataSourceName) Then
'                Return DataSourceName.Trim().ToLower()
'            End If
'            Return String.Empty
'        End Function
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' get the name of the meeting
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="recordId"></param>record
'        ''' <returns></returns>
'        Public Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
'            Return cpcore.db.getRecordName(primaryContentName, recordId)
'        End Function
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' add a new recod to the db and open it. Starting a new model with this method will use the default
'        ''' values in Contensive metadata (active, contentcontrolid, etc)
'        ''' </summary>
'        ''' <param name="cpCore"></param>
'        ''' <param name="cacheNameList"></param>
'        ''' <returns></returns>
'        Public Shared Function add(cpCore As coreClass, ByRef cacheNameList As List(Of String)) As dataSourceModel
'            Dim result As dataSourceModel = Nothing
'            Try
'                result = create(cpCore, cpCore.db.metaData_InsertContentRecordGetID(primaryContentName, 0), cacheNameList)
'            Catch ex As Exception
'                cpCore.handleExceptionAndContinue(ex) : Throw
'                Throw
'            End Try
'            Return result
'        End Function
'    End Class
'End Namespace

