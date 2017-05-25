
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class groupModel
        '
        '-- const
        Public Const primaryContentName As String = "groups" '<------ set content name
        Private Const primaryContentTableName As String = "ccgroups" '<------ set to tablename for the primary content (used for cache names)
        Private Const primaryContentDataSource As String = "default" '<----- set to datasource if not default
        '
        ' -- instance properties
        Public ID As Integer
        Public Active As Boolean
        Public AllowBulkEmail As Boolean
        Public Caption As String
        Public ccGuid As String
        Public ContentCategoryID As Integer
        Public ContentControlID As Integer
        Public CopyFilename As String
        Public CreatedBy As Integer
        Public CreateKey As Integer
        Public DateAdded As Date
        Public EditArchive As Boolean
        Public EditBlank As Boolean
        Public EditSourceID As Integer
        Public ModifiedBy As Integer
        Public ModifiedDate As Date
        Public Name As String
        Public PublicJoin As Boolean
        Public SortOrder As String
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
        Public Shared Function add(cpCore As coreClass, ByRef cacheNameList As List(Of String)) As groupModel
            Dim result As groupModel = Nothing
            Try
                result = create(cpCore, cpCore.db.metaData_InsertContentRecordGetID(primaryContentName, 0), cacheNameList)
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
        Public Shared Function create(cpCore As coreClass, recordId As Integer, ByRef cacheNameList As List(Of String)) As groupModel
            Dim result As groupModel = Nothing
            Try
                If recordId > 0 Then
                    Dim cacheName As String = GetType(groupModel).FullName & getCacheName("id", recordId.ToString())
                    result = cpCore.cache.getObject(Of groupModel)(cacheName)
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
        Public Shared Function create(cpCore As coreClass, recordGuid As String, ByRef cacheNameList As List(Of String)) As groupModel
            Dim result As groupModel = Nothing
            Try
                If Not String.IsNullOrEmpty(recordGuid) Then
                    Dim cacheName As String = GetType(groupModel).FullName & getCacheName("ccguid", recordGuid)
                    result = cpCore.cache.getObject(Of groupModel)(cacheName)
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
        ''' template for open an existing object with multiple keys (like a rule)
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="foreignKey1Id"></param>
        Public Shared Function create(cpCore As coreClass, foreignKey1Id As Integer, foreignKey2Id As Integer, ByRef cacheNameList As List(Of String)) As groupModel
            Dim result As groupModel = Nothing
            Try
                If ((foreignKey1Id > 0) And (foreignKey2Id > 0)) Then
                    result = cpCore.cache.getObject(Of groupModel)(getCacheName("foreignKey1", foreignKey1Id.ToString(), "foreignKey2", foreignKey2Id.ToString()))
                    If (result Is Nothing) Then
                        result = loadObject(cpCore, "(foreignKey1=" & foreignKey1Id.ToString() & ")and(foreignKey1=" & foreignKey1Id.ToString() & ")", cacheNameList)
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
        Private Shared Function loadObject(cpCore As coreClass, sqlCriteria As String, ByRef callersCacheNameList As List(Of String)) As groupModel
            Dim result As groupModel = Nothing
            Try
                Dim cs As New csController(cpCore)
                If cs.open(primaryContentName, sqlCriteria) Then
                    result = New groupModel
                    With result
                        '
                        ' -- populate result model
                        .ID = cs.getInteger("ID")
                        .Active = cs.getBoolean("Active")
                        .AllowBulkEmail = cs.getBoolean("AllowBulkEmail")
                        .Caption = cs.getText("Caption")
                        .ccGuid = cs.getText("ccGuid")
                        .ContentCategoryID = cs.getInteger("ContentCategoryID")
                        .ContentControlID = cs.getInteger("ContentControlID")
                        .CopyFilename = cs.getText("CopyFilename")
                        .CreatedBy = cs.getInteger("CreatedBy")
                        .CreateKey = cs.getInteger("CreateKey")
                        .DateAdded = cs.getDate("DateAdded")
                        .EditArchive = cs.getBoolean("EditArchive")
                        .EditBlank = cs.getBoolean("EditBlank")
                        .EditSourceID = cs.getInteger("EditSourceID")
                        .ModifiedBy = cs.getInteger("ModifiedBy")
                        .ModifiedDate = cs.getDate("ModifiedDate")
                        .Name = cs.getText("Name")
                        .PublicJoin = cs.getBoolean("PublicJoin")
                        .SortOrder = cs.getText("SortOrder")
                    End With
                    If (result IsNot Nothing) Then
                        '
                        ' -- set primary cache to the object created
                        ' -- set secondary caches to the primary cache
                        ' -- add all cachenames to the injected cachenamelist
                        Dim cacheName0 As String = getCacheName("id", result.id.ToString())
                        callersCacheNameList.Add(cacheName0)
                        cpCore.cache.setObject(cacheName0, result)
                        '
                        Dim cacheName1 As String = getCacheName("ccguid", result.ccguid)
                        callersCacheNameList.Add(cacheName1)
                            cpCore.cache.setSecondaryObject(cacheName1, cacheName0)
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
                    cs.setField("Active", Active.ToString())
                    cs.setField("AllowBulkEmail", AllowBulkEmail.ToString())
                    cs.setField("Caption", Caption)
                    cs.setField("ccGuid", ccGuid)
                    cs.setField("ContentCategoryID", ContentCategoryID.ToString())
                    cs.setField("ContentControlID", ContentControlID.ToString())
                    cs.setField("CopyFilename", CopyFilename)
                    cs.setField("CreatedBy", CreatedBy.ToString())
                    cs.setField("CreateKey", CreateKey.ToString())
                    cs.setField("DateAdded", DateAdded.ToString())
                    cs.setField("EditArchive", EditArchive.ToString())
                    cs.setField("EditBlank", EditBlank.ToString())
                    cs.setField("EditSourceID", EditSourceID.ToString())
                    cs.setField("ModifiedBy", ModifiedBy.ToString())
                    cs.setField("ModifiedDate", ModifiedDate.ToString())
                    cs.setField("Name", Name)
                    cs.setField("PublicJoin", PublicJoin.ToString())
                    cs.setField("SortOrder", SortOrder)
                End If
                Call cs.Close()
                '
                ' -- invalidate objects
                cpCore.cache.invalidateObject(getCacheName("id", id.ToString))
                cpCore.cache.invalidateObject(getCacheName("ccguid", ccguid))
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return id
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' delete an existing database record by id
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>
        Public Shared Sub delete(cpCore As coreClass, recordId As Integer)
            Try
                If (recordId > 0) Then
                    cpCore.db.deleteContentRecords(primaryContentName, "id=" & recordId.ToString)
                    cpCore.cache.invalidateObject(getCacheName("id", recordId.ToString))
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' delete an existing database record by guid
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>
        Public Shared Sub delete(cpCore As coreClass, ccguid As String)
            Try
                If (Not String.IsNullOrEmpty(ccguid)) Then
                    cpCore.db.deleteContentRecords(primaryContentName, "(ccguid=" & cpCore.db.encodeSQLText(ccguid) & ")")
                    cpCore.cache.invalidateObject(getCacheName("ccguid", ccguid))
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' pattern to delete an existing object based on multiple criteria (like a rule record)
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="foreignKey1Id"></param>
        ''' <param name="foreignKey2Id"></param>
        Public Shared Sub delete(cpCore As coreClass, foreignKey1Id As Integer, foreignKey2Id As Integer)
            Try
                If (foreignKey2Id > 0) And (foreignKey1Id > 0) Then
                    Dim rule As groupModel = create(cpCore, foreignKey1Id, foreignKey2Id, New List(Of String))
                    If (rule IsNot Nothing) Then
                        cpCore.cache.invalidateObject(GetType(groupModel).FullName & getCacheName("foreignKey1", foreignKey1Id.ToString()) & getCacheName("foreignKey2", foreignKey1Id.ToString()))
                        cpCore.db.deleteTableRecord(primaryContentTableName, rule.id, primaryContentDataSource)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' pattern get a list of objects from this model
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="someCriteria"></param>
        ''' <returns></returns>
        Public Shared Function getObjectList(cpCore As coreClass, someCriteria As Integer) As List(Of groupModel)
            Dim result As New List(Of groupModel)
            Try
                Dim cs As New csController(cpCore)
                Dim ignoreCacheNames As New List(Of String)
                If (cs.open(primaryContentName, "(someCriteria=" & someCriteria & ")", "name", True, "id")) Then
                    Dim instance As groupModel
                    Do
                        instance = groupModel.create(cpCore, cs.getInteger("id"), ignoreCacheNames)
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
        Public Shared Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
            cpCore.cache.invalidateObject(getCacheName("id", recordId.ToString))
            '
            ' -- the zero record cache means any record was updated. Can be used to invalidate arbitraty lists of records in the table
            cpCore.cache.invalidateObject(getCacheName("id", "0"))
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
            Return (primaryContentTableName & "-" & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
        End Function
        Private Shared Function getCacheName(field1Name As String, field1Value As String, field2Name As String, field2Value As String) As String
            Return (primaryContentTableName & "-" & field1Name & "." & field1Value & "-" & field2Name & "." & field2Value).ToLower().Replace(" ", "_")
        End Function
    End Class
End Namespace
