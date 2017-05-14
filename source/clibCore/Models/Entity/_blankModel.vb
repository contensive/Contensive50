
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    '
    '====================================================================================================
    ' simple entity model pattern
    '   factory pattern load because if a record is not found, must rturn nothing
    '   new() - to allow deserialization (so all methods must pass in cp)
    '   create( cp, id ) - to loads instance properties
    '   saveObject( cp ) - saves instance properties
    '
    Public Class _blankModel
        '
        '-- const
        Public Const cnPrimaryContent As String = "" '<------ set content name
        Private Const cacheNamePrefix As String = "" '<------ set to unique name, maybe db table name or if complex object a unique name for this model
        '
        ' cache
        '   caching applies to model objects only, not lists of models (for now)
        '       - this is because of the challenge of invalidating the list object when individual records are added or deleted
        '
        '   a model should have 1 primary cache object which stores the data and can have other secondary cacheObjects which do not hold data
        '    the cacheName of the 'primary' cacheObject for models and db records (cacheNamePrefix + ".id." + #id)
        '    'secondary' cacheName is (cacheNamePrefix + . + fieldName + . + #)
        '
        '   cacheobjects can be used to hold data (primary cacheobjects), or to hold only metadata (secondary cacheobjects)
        '       - primary cacheobjects are like 'personModel.id.99' that holds the model for id=99
        '           - it is primary because the .primaryobject is null
        '           - invalidationData. This cacheobject is invalid after this datetime
        '           - dependentobjectlist() - this object is invalid if any of those objects are invalid
        '       - secondary cachobjects are like 'person.ccguid.12345678'. It does not hold data, just a reference to the primary cacheobject
        '
        '   when a model is created, the code first attempts to read the model's cacheobject. if it fails, it builds it and saves the cache object and tags
        '       - when building the model, is writes object to the primary cacheobject, and writes all the secondaries to be used
        '       - when building the model, if a database record is opened, a dependantObject Tag is created for the tablename+'id'+id
        '       - when building the model, if another model is added, that models cacheObject name is added to the dependentObjectList
        '
        Private cacheName As String = ""
        '
        ' -- instance properties
        Public id As Integer
        Public name As String
        Public guid As String
        '
        ' -- publics not exposed to the UI (test/internal data)
        <JsonIgnore> Public createKey As Integer
        '
        ' -- when an object is created, this is the name of the cache entry it was saved to. The consuming object
        '    should use this in it's cache tag list so if another process modifies this data, the consuming object's
        '    cache will be invalidated.
        '   - There can be more than one cacheName (object-id, object-guid, etc). Add them all the parent's tag list.
        '   - 
        '
        '<JsonIgnore>
        'Public cacheNameList As List(Of String)
        '
        '====================================================================================================
        ''' <summary>
        ''' Create an empty object. needed for deserialization
        ''' </summary>
        Public Sub New()
            '
        End Sub
        '
        Private Shared Function getCacheKeySuffix(fieldName As String, fieldValue As String) As String
            Return ("." & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId">The id of the record to be read into the new object</param>
        ''' <param name="cacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
        Public Shared Function create(cpCore As coreClass, recordId As Integer, ByRef cacheNameList As List(Of String)) As _blankModel
            Dim result As _blankModel = Nothing
            Try
                If recordId > 0 Then
                    Dim cacheName As String = GetType(_blankModel).FullName & getCacheKeySuffix("id", recordId.ToString())
                    result = cpCore.cache.getObject(Of _blankModel)(cacheName)
                    If (result Is Nothing) Then
                        result = loadObject(cpCore, "id=" & recordId.ToString())
                    End If
                    If (result IsNot Nothing) Then
                        cacheNameList.Add(GetType(_blankModel).FullName & getCacheKeySuffix("id", result.id.ToString()))
                        cacheNameList.Add(GetType(_blankModel).FullName & getCacheKeySuffix("ccguid", result.guid))
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' open an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordGuid"></param>
        Public Shared Function create(cpCore As coreClass, recordGuid As String, ByRef cacheNameList As List(Of String)) As _blankModel
            Dim result As _blankModel = Nothing
            Try
                If Not String.IsNullOrEmpty(recordGuid) Then
                    Dim cacheName As String = GetType(_blankModel).FullName & getCacheKeySuffix("ccguid", recordGuid)
                    result = cpCore.cache.getObject(Of _blankModel)(cacheName)
                    If (result Is Nothing) Then
                        result = loadObject(cpCore, "ccGuid=" & cpCore.db.encodeSQLText(recordGuid))
                    End If
                    If (result IsNot Nothing) Then
                        cacheNameList.Add(GetType(_blankModel).FullName & getCacheKeySuffix("id", result.id.ToString()))
                        cacheNameList.Add(GetType(_blankModel).FullName & getCacheKeySuffix("ccguid", result.guid))
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' open an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="sqlCriteria"></param>
        Private Shared Function loadObject(cpCore As coreClass, sqlCriteria As String) As _blankModel
            Dim result As _blankModel = Nothing
            Try
                Dim cs As New csController(cpCore)
                If cs.open(cnPrimaryContent, sqlCriteria) Then
                    result = New _blankModel
                    With result
                        .id = cs.getInteger("id")
                        .name = cs.getText("name")
                        .guid = cs.getText("ccGuid")
                        .createKey = cs.getInteger("createKey")
                    End With
                End If
                Call cs.Close()
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        '
        Public Function saveObject(cpCore As coreClass) As Integer
            Try
                Dim cs As New csController(cpCore)
                If (id > 0) Then
                    If Not cs.open(cnPrimaryContent, "id=" & id) Then
                        id = 0
                        cs.Close()
                        Throw New ApplicationException("Unable to open record in content [" & cnPrimaryContent & "], with id [" & id & "]")
                    End If
                Else
                    If Not cs.Insert(cnPrimaryContent) Then
                        cs.Close()
                        id = 0
                        Throw New ApplicationException("Unable to insert record in content [" & cnPrimaryContent & "]")
                    End If
                End If
                If cs.ok() Then
                    id = cs.getInteger("id")
                    Call cs.SetField("name", name)
                    Call cs.SetField("ccGuid", guid)
                    Call cs.SetField("createKey", createKey.ToString())
                End If
                Call cs.Close()
                Dim primaryKey As String = GetType(_blankModel).FullName & getCacheKeySuffix("id", id.ToString)
                cpCore.cache.setObject(primaryKey, Me)

                Dim secondaryKey As String = GetType(_blankModel).FullName & getCacheKeySuffix("ccguid", guid)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return id
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' delete an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>
        Public Shared Sub delete(cpCore As coreClass, recordId As Integer)
            Try
                If (recordId > 0) Then
                    cpCore.db.deleteContentRecords(cnPrimaryContent, "id=" & recordId.ToString)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' delete an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>
        Public Shared Sub delete(cpCore As coreClass, guid As String)
            Try
                If (Not String.IsNullOrEmpty(guid)) Then
                    cpCore.db.deleteContentRecords(cnPrimaryContent, "(ccguid=" & cpCore.db.encodeSQLText(guid) & ")")
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' get a list of objects from this model
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="someCriteria"></param>
        ''' <returns></returns>
        Public Shared Function getObjectList(cpCore As coreClass, someCriteria As Integer) As List(Of _blankModel)
            Dim result As New List(Of _blankModel)
            Try
                Dim cs As New csController(cpCore)
                Dim ignoreCacheNames As New List(Of String)
                If (cs.open(cnPrimaryContent, "(someCriteria=" & someCriteria & ")", "name", True, "id")) Then
                    Dim instance As _blankModel
                    Do
                        instance = _blankModel.create(cpCore, cs.getInteger("id"), ignoreCacheNames)
                        If (instance IsNot Nothing) Then
                            result.Add(instance)
                        End If
                        cs.goNext()
                    Loop While cs.ok()
                End If
                cs.Close()
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
