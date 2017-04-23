
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Contensive.Core.Models.Entity
    '
    '====================================================================================================
    ' cached entity model pattern
    '   factory pattern creator, constructor is a shared method that returns a loaded object
    '   new() - to allow deserialization (so all methods must pass in cp)
    '   shared getObject( cp, id ) - returns loaded model
    '   saveObject( cp ) - saves instance properties, returns the record id
    '
    Public Class dataSourceModel
        '
        ' -- public properties
        Public id As Integer = 0
        Public name As String = String.Empty
        Public connStringOLEDB As String = String.Empty
        Public dataSourceType As dataSourceTypeEnum
        Public endPoint As String
        Public username As String
        Public password As String
        '
        ' -- list of tag names that will flush the cache
        Public Shared ReadOnly Property cacheTagList As String = cnDataSources & ","
        '
        Public Enum dataSourceTypeEnum
            sqlServerOdbc = 1
            sqlServerNative = 2
            mySqlNative = 3
        End Enum
        '
        '====================================================================================================
        ''' <summary>
        ''' Create an empty object. needed for deserialization. Use newModel() method as constructor, includes cache
        ''' </summary>
        Public Sub New()
            '
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Open existing
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>
        Public Shared Function getObject(cp As CPBaseClass, recordId As Integer, ByRef adminMessageList As List(Of String)) As Models.Entity.dataSourceModel
            Dim returnModel As Models.Entity.dataSourceModel = Nothing
            Try
                Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer
                Dim recordCacheName As String = cnDataSources & "CachedModelRecordId" & recordId
                Dim recordCache As String = cp.Cache.Read(recordCacheName)
                Dim loadDbModel As Boolean = True
                If Not String.IsNullOrEmpty(recordCache) Then
                    Try
                        returnModel = json_serializer.Deserialize(Of dataSourceModel)(recordCache)
                        '
                        ' -- if model exposes any objects, verify they are created
                        'If (returnModel.meetingItemList Is Nothing) Then
                        '    returnModel.meetingItemList = New List(Of meetingItemModel)
                        'End If
                        loadDbModel = False
                    Catch ex As Exception
                        'ignore error - just roll through to rebuild model and save new cache
                    End Try
                End If
                If loadDbModel Or (returnModel Is Nothing) Then
                    returnModel = getObjectNoCache(cp, recordId)
                    Call cp.Cache.Save(recordCacheName, json_serializer.Serialize(returnModel), cacheTagList)
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return returnModel
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' called only from getObject. Load the model from the Db without cache. If there are any properties or objects that cannot be used from cache, do not include them here either, load in getObject()
        ''' </summary>
        ''' <param name="recordId"></param>
        Private Shared Function getObjectNoCache(cp As CPBaseClass, recordId As Integer) As Models.Entity.dataSourceModel
            Dim returnNewModel As New dataSourceModel()
            Try
                Dim cs As CPCSBaseClass = cp.CSNew()
                returnNewModel.id = 0
                If recordId <> 0 Then
                    cs.Open(cnDataSources, "(ID=" & recordId & ")", "name", True, "name,connString")
                    If cs.OK() Then
                        returnNewModel.id = recordId
                        returnNewModel.name = normalizeDataSourceName(cs.GetText("Name"))
                        returnNewModel.connStringOLEDB = cs.GetText("connString")
                    End If
                    Call cs.Close()
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return returnNewModel
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Save the object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Function saveObject(cp As CPBaseClass) As Integer
            Try
                Dim cs As CPCSBaseClass = cp.CSNew()
                If (id > 0) Then
                    If Not cs.Open(cnDataSources, "id=" & id) Then
                        id = 0
                        cs.Close()
                        Throw New ApplicationException("Unable to open record [" & id & "]")
                    End If
                Else
                    If Not cs.Insert(cnDataSources) Then
                        cs.Close()
                        id = 0
                        Throw New ApplicationException("Unable to insert record")
                    End If
                End If
                If cs.OK() Then
                    id = cs.GetInteger("id")
                    Call cs.SetField("name", normalizeDataSourceName(name))
                    Call cs.SetField("connString", connStringOLEDB)
                End If
                Call cs.Close()
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Throw
            End Try
            Return id
        End Function
        '
        Public Shared Function getDictionary(cpcore As coreClass) As Dictionary(Of String, dataSourceModel)
            Dim result As New Dictionary(Of String, dataSourceModel)
            Try
                Dim sql As String = "select id,name,connString from ccDataSources order by id"
                Using dt As DataTable = cpcore.db.executeSql(sql, "", 0, 1)
                    If (dt Is Nothing) Then
                        'Throw New ApplicationException("dataSourceName [" & dataSourceName & "] is not valid.")
                    ElseIf (dt.Rows.Count = 0) Then
                        'Throw New ApplicationException("dataSourceName [" & dataSourceName & "] is not valid.")
                    Else
                        Dim dataSource As New dataSourceModel
                        Dim normalizedDataSourceName As String = normalizeDataSourceName(dt.Rows(0).Field(Of String)("name"))
                        dataSource.id = dt.Rows(0).Field(Of Integer)("id")
                        dataSource.name = normalizedDataSourceName
                        dataSource.connStringOLEDB = dt.Rows(0).Field(Of String)("connString")
                        result.Add(normalizedDataSourceName, dataSource)
                    End If
                End Using
            Catch ex As Exception
                cpcore.handleExceptionAndRethrow(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' convert a datasource name into the key value used by the datasourcedictionary cache
        ''' </summary>
        ''' <param name="DataSourceName"></param>
        ''' <returns></returns>
        Public Shared Function normalizeDataSourceName(DataSourceName As String) As String
            If Not String.IsNullOrEmpty(DataSourceName) Then
                Return DataSourceName.Trim().ToLower()
            End If
            Return String.Empty
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get the name of the meeting
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>record
        ''' <returns></returns>
        Public Shared Function getRecordName(cp As CPBaseClass, recordId As Integer) As String
            Return cp.Content.GetRecordName(cnDataSources, recordId)
        End Function
    End Class
End Namespace

