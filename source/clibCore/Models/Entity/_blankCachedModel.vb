
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
    Public Class blankCachedModel
        '
        ' -- public properties
        '
        Public id As Integer = 0
        Public name As String = String.Empty
        '
        ' -- list of tag names that will flush the cache
        Public Shared ReadOnly Property cacheTagList As String = ""
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
        Public Shared Function getObject(cpcore As coreClass, recordId As Integer, ByRef adminMessageList As List(Of String)) As Models.Entity.blankCachedModel
            Dim returnModel As Models.Entity.blankCachedModel = Nothing
            Try
                Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer
                Dim recordCacheName As String = cnBlank & "CachedModelRecordId" & recordId
                Dim recordCache As String = cpcore.cache.getString(recordCacheName)
                Dim loadDbModel As Boolean = True
                If Not String.IsNullOrEmpty(recordCache) Then
                    Try
                        returnModel = json_serializer.Deserialize(Of blankCachedModel)(recordCache)
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
                    returnModel = getObjectNoCache(cpcore, recordId)
                    Call cpcore.cache.setKey(recordCacheName, json_serializer.Serialize(returnModel), cacheTagList)
                End If
            Catch ex As Exception
                cpcore.handleExceptionAndRethrow(ex)
            End Try
            Return returnModel
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' called only from getObject. Load the model from the Db without cache. If there are any properties or objects that cannot be used from cache, do not include them here either, load in getObject()
        ''' </summary>
        ''' <param name="recordId"></param>
        Private Shared Function getObjectNoCache(cpcore As coreClass, recordId As Integer) As Models.Entity.blankCachedModel
            Dim returnNewModel As New blankCachedModel()
            Try
                Dim cs As New csController(cpcore)
                returnNewModel.id = 0
                If recordId <> 0 Then
                    cs.open(cnBlank, "(ID=" & recordId & ")")
                    If cs.ok() Then
                        returnNewModel.id = recordId
                        returnNewModel.name = cs.getText("Name")
                    End If
                    Call cs.Close()
                End If
            Catch ex As Exception
                cpcore.handleExceptionAndRethrow(ex)
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
        Public Function saveObject(cpcore As coreClass) As Integer
            Try
                Dim cs As New csController(cpcore)
                If (id > 0) Then
                    If Not cs.open(cnBlank, "id=" & id) Then
                        id = 0
                        cs.Close()
                        Throw New ApplicationException("Unable to open record [" & id & "]")
                    End If
                Else
                    If Not cs.Insert(cnBlank) Then
                        cs.Close()
                        id = 0
                        Throw New ApplicationException("Unable to insert record")
                    End If
                End If
                If cs.ok() Then
                    id = cs.getInteger("id")
                    Call cs.SetField("name", name)
                End If
                Call cs.Close()
            Catch ex As Exception
                cpcore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return id
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get the name of the meeting
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>record
        ''' <returns></returns>
        Public Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return cpcore.content_GetRecordName(cnBlank, recordId)
        End Function
    End Class
End Namespace

