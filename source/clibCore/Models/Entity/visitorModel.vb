
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
    ' entity model pattern
    '   factory pattern load because if a record is not found, must rturn nothing
    '   new() - empty constructor to allow deserialization
    '   saveObject() - saves instance properties (nonstatic method)
    '   create() - loads instance properties and returns a model 
    '   delete() - deletes the record that matches the argument
    '   getObjectList() - a pattern for creating model lists.
    '   invalidateFIELDNAMEcache() - method to invalide the model cache. One per cache
    '
    '	1) set the primary content name in const cnPrimaryContent. avoid constants Like cnAddons used outside model
    '	2) find-And-replace "visitorModel" with the name for this model
    '	3) when adding model fields, add in three places: the Public Property, the saveObject(), the loadObject()
    '	4) when adding create() methods to support other fields/combinations of fields, 
    '       - add a secondary cache For that new create method argument in loadObjec()
    '       - add it to the injected cachename list in loadObject()
    '       - add an invalidate
    '
    ' Model Caching
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
    '   cacheNames spaces are replaced with underscores, so "addon collections" should be addon_collections
    '
    '   cacheNames that match content names are treated as caches of "any" record in the content, so invalidating "people" can be used to invalidate
    '       any non-specific cache in the people table, by including "people" as a dependant cachename. the "people" cachename should not clear
    '       specific people caches, like people.id.99, but can be used to clear lists of records like "staff_list_group"
    '       - this can be used as a fallback strategy to cache record lists: a remote method list can be cached with a dependancy on "add-ons".
    '       - models should always clear this content name cache entry on all cache clears
    '
    '   when a model is created, the code first attempts to read the model's cacheobject. if it fails, it builds it and saves the cache object and tags
    '       - when building the model, is writes object to the primary cacheobject, and writes all the secondaries to be used
    '       - when building the model, if a database record is opened, a dependantObject Tag is created for the tablename+'id'+id
    '       - when building the model, if another model is added, that model returns its cachenames in the cacheNameList to be added as dependentObjects
    '
    '
    Public Class visitorModel
        '
        '-- const
        Public Const primaryContentName As String = "visitors" '<------ set content name
        Private Const primaryContentTableName As String = "ccvisitors" '<------ set to tablename for the primary content (used for cache names)
        '
        ' -- instance properties
        Public id As Integer
        Public name As String
        Public guid As String
        '
        Public memberID As Integer = 0              ' the last member account this visitor used (memberid=0 means untracked guest)
        Public orderID As Integer = 0               ' the current shopping cart (non-complete order)
        Public newVisitor As Boolean = False               ' stored in visit record - Is this the first visit for this visitor
        Public forceBrowserMobile As Integer = 0           ' 0 = not set -- use Browser detect each time, 1 = Force Mobile, 2 = Force not Mobile
        '
        ' -- publics not exposed to the UI (test/internal data)
        <JsonIgnore> Public createKey As Integer
        '
        '====================================================================================================
        ''' <summary>
        ''' Create an empty object. needed for deserialization
        ''' </summary>
        Public Sub New()
            '
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' add a new recod to the db and open it. Starting a new model with this method will use the default
        ''' values in Contensive metadata (active, contentcontrolid, etc)
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="cacheNameList"></param>
        ''' <returns></returns>
        Public Shared Function add(cpCore As coreClass, ByRef cacheNameList As List(Of String)) As visitorModel
            Dim result As visitorModel = Nothing
            Try
                result = create(cpCore, cpCore.db.metaData_InsertContentRecordGetID(primaryContentName, cpCore.authContext.user.ID), cacheNameList)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId">The id of the record to be read into the new object</param>
        ''' <param name="cacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
        Public Shared Function create(cpCore As coreClass, recordId As Integer, ByRef cacheNameList As List(Of String)) As visitorModel
            Dim result As visitorModel = Nothing
            Try
                If recordId > 0 Then
                    Dim cacheName As String = GetType(visitorModel).FullName & getCacheName("id", recordId.ToString())
                    result = cpCore.cache.getObject(Of visitorModel)(cacheName)
                    If (result Is Nothing) Then
                        result = loadObject(cpCore, "id=" & recordId.ToString(), cacheNameList)
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
        Public Shared Function create(cpCore As coreClass, recordGuid As String, ByRef cacheNameList As List(Of String)) As visitorModel
            Dim result As visitorModel = Nothing
            Try
                If Not String.IsNullOrEmpty(recordGuid) Then
                    Dim cacheName As String = GetType(visitorModel).FullName & getCacheName("ccguid", recordGuid)
                    result = cpCore.cache.getObject(Of visitorModel)(cacheName)
                    If (result Is Nothing) Then
                        result = loadObject(cpCore, "ccGuid=" & cpCore.db.encodeSQLText(recordGuid), cacheNameList)
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
        Private Shared Function loadObject(cpCore As coreClass, sqlCriteria As String, ByRef cacheNameList As List(Of String)) As visitorModel
            Dim result As visitorModel = Nothing
            Try
                Dim cs As New csController(cpCore)
                If cs.open(primaryContentName, sqlCriteria) Then
                    result = New visitorModel
                    With result
                        '
                        ' -- populate result model
                        .id = cs.getInteger("id")
                        .name = cs.getText("name")
                        .guid = cs.getText("ccGuid")
                        .createKey = cs.getInteger("createKey")
                    End With
                    If (result IsNot Nothing) Then
                        '
                        ' -- set primary and secondary caches
                        ' -- add all cachenames to the injected cachenamelist
                        Dim cacheName0 As String = getCacheName("id", result.id.ToString())
                        cacheNameList.Add(cacheName0)
                        cpCore.cache.setObject(cacheName0, result)
                        '
                        Dim cacheName1 As String = getCacheName("ccguid", result.guid)
                        cacheNameList.Add(cacheName1)
                        cpCore.cache.setObject(cacheName1, Nothing, cacheName1)
                    End If
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
        ''' <summary>
        ''' save the instance properties to a record with matching id. If id is not provided, a new record is created.
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <returns></returns>
        Public Function saveObject(cpCore As coreClass) As Integer
            Try
                Dim cs As New csController(cpCore)
                If (id > 0) Then
                    If Not cs.open(primaryContentName, "id=" & id) Then
                        id = 0
                        cs.Close()
                        Throw New ApplicationException("Unable to open record in content [" & primaryContentName & "], with id [" & id & "]")
                    End If
                Else
                    If Not cs.Insert(primaryContentName) Then
                        cs.Close()
                        id = 0
                        Throw New ApplicationException("Unable to insert record in content [" & primaryContentName & "]")
                    End If
                End If
                If cs.ok() Then
                    id = cs.getInteger("id")
                    Call cs.SetField("name", name)
                    Call cs.SetField("ccGuid", guid)
                    Call cs.SetField("createKey", createKey.ToString())
                End If
                Call cs.Close()
                '
                ' -- invalidate objects
                cpCore.cache.invalidateObject(getCacheName("id", id.ToString))
                cpCore.cache.invalidateObject(getCacheName("ccguid", guid))
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return id
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' delete an existing database record
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>
        Public Shared Sub delete(cpCore As coreClass, recordId As Integer)
            Try
                If (recordId > 0) Then
                    cpCore.db.deleteContentRecords(primaryContentName, "id=" & recordId.ToString)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' delete an existing database record
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>
        Public Shared Sub delete(cpCore As coreClass, guid As String)
            Try
                If (Not String.IsNullOrEmpty(guid)) Then
                    cpCore.db.deleteContentRecords(primaryContentName, "(ccguid=" & cpCore.db.encodeSQLText(guid) & ")")
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
        Public Shared Function getObjectList(cpCore As coreClass, someCriteria As Integer) As List(Of visitorModel)
            Dim result As New List(Of visitorModel)
            Try
                Dim cs As New csController(cpCore)
                Dim ignoreCacheNames As New List(Of String)
                If (cs.open(primaryContentName, "(someCriteria=" & someCriteria & ")", "name", True, "id")) Then
                    Dim instance As visitorModel
                    Do
                        instance = visitorModel.create(cpCore, cs.getInteger("id"), ignoreCacheNames)
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
        '
        '====================================================================================================
        ''' <summary>
        ''' invalidate the primary key (which depends on all secondary keys)
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="recordId"></param>
        Public Shared Sub invalidateIdCache(cpCore As coreClass, recordId As Integer)
            cpCore.cache.invalidateObject(getCacheName("id", recordId.ToString))
            '
            ' -- always clear the cache with the content name
            cpCore.cache.invalidateObject(primaryContentName)
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' invalidate a secondary key (ccGuid field).
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="guid"></param>
        Public Shared Sub invalidateGuidCache(cpCore As coreClass, guid As String)
            cpCore.cache.invalidateObject(getCacheName("ccguid", guid))
            '
            ' -- always clear the cache with the content name
            cpCore.cache.invalidateObject(primaryContentName)
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' produce a standard format cachename for this model
        ''' </summary>
        ''' <param name="fieldName"></param>
        ''' <param name="fieldValue"></param>
        ''' <returns></returns>
        Private Shared Function getCacheName(fieldName As String, fieldValue As String) As String
            Return (primaryContentTableName & "." & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
            'Return (GetType(visitorModel).FullName & "." & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
        End Function
        '        '
        '        ' LEGACY CODE =============================================================================
        '        '   Save Visitor
        '        '
        '        '   Saves changes to the visitor record back to the database. Should be called
        '        '   before exit of anypage if anything here changes
        '        '=============================================================================
        '        '
        '        Public Sub saveObject(cpcore As coreClass)
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SaveVisitor")
        '            '
        '            'If Not (true) Then Exit Sub
        '            '
        '            Dim SQL As String
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_SaveVisitor"
        '            '
        '            If cpcore.visit.visit_initialized Then
        '                If True Then
        '                    SQL = "UPDATE ccVisitors SET " _
        '                        & " Name = " & cpcore.db.encodeSQLText(name) _
        '                        & ",MemberID = " & cpcore.db.encodeSQLNumber(memberID) _
        '                        & ",OrderID = " & cpcore.db.encodeSQLNumber(orderID) _
        '                        & ",ForceBrowserMobile = " & cpcore.db.encodeSQLNumber(forceBrowserMobile) _
        '                        & " WHERE ID=" & id & ";"
        '                Else
        '                    SQL = "UPDATE ccVisitors SET " _
        '                        & " Name = " & cpcore.db.encodeSQLText(name) _
        '                        & ",MemberID = " & cpcore.db.encodeSQLNumber(memberID) _
        '                        & ",OrderID = " & cpcore.db.encodeSQLNumber(orderID) _
        '                        & " WHERE ID=" & id & ";"
        '                End If
        '                Call cpcore.db.executeSql(SQL)
        '            End If
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call cpcore.handleLegacyError18(MethodName)
        '            '
        '        End Sub
    End Class
End Namespace
