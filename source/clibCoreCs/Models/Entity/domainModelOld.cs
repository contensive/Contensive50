using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

//Option Explicit On
//Option Strict On

//Imports System
//Imports System.Collections.Generic
//Imports System.Text
//Imports Contensive.BaseClasses
//Imports Contensive.Core.Controllers
//Imports Newtonsoft.Json

//namespace Contensive.Core.Models.Entity
//    Public Class domainModel
//        '
//        '-- const
//        Public Const primaryContentName As String = "domains"
//        Private Const primaryContentTableName As String = "ccdomains"
//        Private Const primaryContentDataSource As String = "default"
//        '
//        ' -- instance properties
//        Public ID As Integer
//        Public Active As Boolean
//        Public ccGuid As String
//        'Public ContentControlID As Integer
//        Public CreatedBy As Integer
//        Public CreateKey As Integer
//        Public DateAdded As Date
//        Public DefaultTemplateId As Integer
//        Public forwardDomainId As Integer
//        Public ForwardURL As String
//        Public ModifiedBy As Integer
//        Public ModifiedDate As Date
//        Public Name As String
//        Public NoFollow As Boolean
//        Public PageNotFoundPageID As Integer
//        Public RootPageID As Integer
//        Public SortOrder As String
//        Public TypeID As Integer
//        Public Visited As Boolean
//        '
//        Public Enum domainTypeEnum
//            Normal = 1
//            ForwardToUrl = 2
//            ForwardToReplacementDomain = 3
//        End Enum

//        '
//        '====================================================================================================
//        ''' <summary>
//        ''' Create an empty object. needed for deserialization
//        ''' </summary>
//        Public Sub New()
//            '
//        End Sub
//        '
//        '====================================================================================================
//        ''' <summary>
//        ''' add a new recod to the db and open it. Starting a new model with this method will use the default
//        ''' values in Contensive metadata (active, contentcontrolid, etc)
//        ''' </summary>
//        ''' <param name="cpCore"></param>
//        ''' <param name="callersCacheNameList"></param>
//        ''' <returns></returns>
//        Public Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As domainModel
//            Dim result As domainModel = Nothing
//            Try
//                result = create(cpCore, cpCore.db.metaData_InsertContentRecordGetID(primaryContentName, cpCore.doc.authContext.user.id), callersCacheNameList)
//            Catch ex As Exception
//                cpCore.handleException(ex); : Throw
//                Throw
//            End Try
//            Return result
//        End Function
//        '
//        '====================================================================================================
//        ''' <summary>
//        ''' return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
//        ''' </summary>
//        ''' <param name="cp"></param>
//        ''' <param name="recordId">The id of the record to be read into the new object</param>
//        ''' <param name="callersCacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
//        Public Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As domainModel
//            Dim result As domainModel = Nothing
//            Try
//                If recordId > 0 Then
//                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, recordId)
//                    result = cpCore.cache.getObject(Of domainModel)(cacheName)
//                    If (result Is Nothing) Then
//                        Using cs As New csController(cpCore)
//                            If cs.open(primaryContentName, "(id=" & recordId.ToString() & ")") Then
//                                result = loadRecord(cpCore, cs, callersCacheNameList)
//                            End If
//                        End Using
//                    End If
//                End If
//            Catch ex As Exception
//                cpCore.handleException(ex); : Throw
//                Throw
//            End Try
//            Return result
//        End Function
//        '
//        '====================================================================================================
//        ''' <summary>
//        ''' open an existing object
//        ''' </summary>
//        ''' <param name="cp"></param>
//        ''' <param name="recordGuid"></param>
//        Public Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As domainModel
//            Dim result As domainModel = Nothing
//            Try
//                If Not String.IsNullOrEmpty(recordGuid) Then
//                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "ccguid", recordGuid)
//                    result = cpCore.cache.getObject(Of domainModel)(cacheName)
//                    If (result Is Nothing) Then
//                        Using cs As New csController(cpCore)
//                            If cs.open(primaryContentName, "(ccGuid=" & cpCore.db.encodeSQLText(recordGuid) & ")") Then
//                                result = loadRecord(cpCore, cs, callersCacheNameList)
//                            End If
//                        End Using
//                    End If
//                End If
//            Catch ex As Exception
//                cpCore.handleException(ex); : Throw
//                Throw
//            End Try
//            Return result
//        End Function
//        '
//        '====================================================================================================
//        ''' <summary>
//        ''' open an existing object
//        ''' </summary>
//        ''' <param name="cp"></param>
//        ''' <param name="recordName"></param>
//        Public Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As domainModel
//            Dim result As domainModel = Nothing
//            Try
//                If Not String.IsNullOrEmpty(recordName) Then
//                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "name", recordName)
//                    result = cpCore.cache.getObject(Of domainModel)(cacheName)
//                    If (result Is Nothing) Then
//                        Using cs As New csController(cpCore)
//                            If cs.open(primaryContentName, "(name=" & cpCore.db.encodeSQLText(recordName) & ")", "id") Then
//                                result = loadRecord(cpCore, cs, callersCacheNameList)
//                            End If
//                        End Using
//                    End If
//                End If
//            Catch ex As Exception
//                cpCore.handleException(ex); : Throw
//                Throw
//            End Try
//            Return result
//        End Function
//        '
//        '====================================================================================================
//        ''' <summary>
//        ''' template for open an existing object with multiple keys (like a rule)
//        ''' </summary>
//        ''' <param name="cp"></param>
//        ''' <param name="foreignKey1Id"></param>
//        Public Shared Function create(cpCore As coreClass, foreignKey1Id As Integer, foreignKey2Id As Integer, ByRef callersCacheNameList As List(Of String)) As domainModel
//            Dim result As domainModel = Nothing
//            Try
//                If ((foreignKey1Id > 0) And (foreignKey2Id > 0)) Then
//                    result = cpCore.cache.getObject(Of domainModel)(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "foreignKey1", foreignKey1Id.ToString(), "foreignKey2", foreignKey2Id.ToString()))
//                    If (result Is Nothing) Then
//                        Using cs As New csController(cpCore)
//                            If cs.open(primaryContentName, "(foreignKey1=" & foreignKey1Id.ToString() & ")and(foreignKey1=" & foreignKey1Id.ToString() & ")") Then
//                                result = loadRecord(cpCore, cs, callersCacheNameList)
//                            End If
//                        End Using
//                    End If
//                End If
//            Catch ex As Exception
//                cpCore.handleException(ex); : Throw
//                Throw
//            End Try
//            Return result
//        End Function
//        '
//        '====================================================================================================
//        ''' <summary>
//        ''' open an existing object
//        ''' </summary>
//        ''' <param name="cp"></param>
//        ''' <param name="sqlCriteria"></param>
//        Private Shared Function loadRecord(cpCore As coreClass, cs As csController, ByRef callersCacheNameList As List(Of String)) As domainModel
//            Dim result As domainModel = Nothing
//            Try
//                If cs.ok() Then
//                    result = New domainModel
//                    With result
//                        '
//                        ' -- populate result model
//                        .ID = cs.getInteger("ID")
//                        .Active = cs.getBoolean("Active")
//                        .ccGuid = cs.getText("ccGuid")
//                        ''
//                        '.ContentControlID = cs.getInteger("ContentControlID")
//                        .CreatedBy = cs.getInteger("CreatedBy")
//                        .CreateKey = cs.getInteger("CreateKey")
//                        .DateAdded = cs.getDate("DateAdded")
//                        .DefaultTemplateId = cs.getInteger("DefaultTemplateId")
//                        ''
//                        ''
//                        ''
//                        .forwardDomainId = cs.getInteger("forwardDomainId")
//                        .ForwardURL = cs.getText("ForwardURL")
//                        .ModifiedBy = cs.getInteger("ModifiedBy")
//                        .ModifiedDate = cs.getDate("ModifiedDate")
//                        .Name = cs.getText("Name")
//                        .NoFollow = cs.getBoolean("NoFollow")
//                        .PageNotFoundPageID = cs.getInteger("PageNotFoundPageID")
//                        .RootPageID = cs.getInteger("RootPageID")
//                        .SortOrder = cs.getText("SortOrder")
//                        .TypeID = cs.getInteger("TypeID")
//                        .Visited = cs.getBoolean("Visited")
//                    End With
//                    If (result IsNot Nothing) Then
//                        '
//                        ' -- set primary cache to the object created
//                        ' -- set secondary caches to the primary cache
//                        ' -- add all cachenames to the injected cachenamelist
//                        Dim cacheName0 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", result.ID.ToString())
//                        callersCacheNameList.Add(cacheName0)
//                        cpCore.cache.setObject(cacheName0, result)
//                        '
//                        Dim cacheName1 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "ccguid", result.ccGuid)
//                        callersCacheNameList.Add(cacheName1)
//                        cpCore.cache.setSecondaryObject(cacheName1, cacheName0)
//                        '
//                        Dim cacheName2 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "name", result.Name)
//                        callersCacheNameList.Add(cacheName2)
//                        cpCore.cache.setSecondaryObject(cacheName2, cacheName0)
//                    End If
//                End If
//            Catch ex As Exception
//                cpCore.handleException(ex); : Throw
//                Throw
//            End Try
//            Return result
//        End Function
//        '
//        '====================================================================================================
//        ''' <summary>
//        ''' save the instance properties to a record with matching id. If id is not provided, a new record is created.
//        ''' </summary>
//        ''' <param name="cpCore"></param>
//        ''' <returns></returns>
//        Public Function save(cpCore As coreClass) As Integer
//            Try
//                Dim cs As New csController(cpCore)
//                If (ID > 0) Then
//                    If Not cs.open(primaryContentName, "id=" & ID) Then
//                        Dim message As String = "Unable to open record in content [" & primaryContentName & "], with id [" & ID & "]"
//                        cs.Close()
//                        ID = 0
//                        Throw New ApplicationException(message)
//                    End If
//                Else
//                    If Not cs.Insert(primaryContentName) Then
//                        cs.Close()
//                        ID = 0
//                        Throw New ApplicationException("Unable to insert record in content [" & primaryContentName & "]")
//                    End If
//                End If
//                If cs.ok() Then
//                    ID = cs.getInteger("id")
//                    cs.setField("Active", Active.ToString())
//                    cs.setField("ccGuid", ccGuid)
//                    ''
//                    'cs.setField("ContentControlID", ContentControlID.ToString())
//                    cs.setField("CreatedBy", CreatedBy.ToString())
//                    cs.setField("CreateKey", CreateKey.ToString())
//                    cs.setField("DateAdded", DateAdded.ToString())
//                    cs.setField("DefaultTemplateId", DefaultTemplateId.ToString())
//                    ''
//                    ''
//                    ''
//                    cs.setField("forwardDomainId", forwardDomainId.ToString())
//                    cs.setField("ForwardURL", ForwardURL)
//                    cs.setField("ModifiedBy", ModifiedBy.ToString())
//                    cs.setField("ModifiedDate", ModifiedDate.ToString())
//                    cs.setField("Name", Name)
//                    cs.setField("NoFollow", NoFollow.ToString())
//                    cs.setField("PageNotFoundPageID", PageNotFoundPageID.ToString())
//                    cs.setField("RootPageID", RootPageID.ToString())
//                    cs.setField("SortOrder", SortOrder)
//                    cs.setField("TypeID", TypeID.ToString())
//                    cs.setField("Visited", Visited.ToString())
//                    If (String.IsNullOrEmpty(ccGuid)) Then ccGuid = Controllers.genericController.getGUID()
//                End If
//                Call cs.Close()
//                '
//                ' -- invalidate objects
//                ' -- no, the primary is invalidated by the cs.save()
//                'cpCore.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"id", id.ToString))
//                ' -- no, the secondary points to the pirmary, which is invalidated. Dont waste resources invalidating
//                'cpCore.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"ccguid", ccguid))
//                '
//                ' -- object is here, but the cache was invalidated, setting
//                cpCore.cache.setObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", Me.id.ToString()), Me)
//            Catch ex As Exception
//                cpCore.handleException(ex); : Throw
//                Throw
//            End Try
//            Return id
//        End Function
//        '
//        '====================================================================================================
//        ''' <summary>
//        ''' delete an existing database record by id
//        ''' </summary>
//        ''' <param name="cp"></param>
//        ''' <param name="recordId"></param>
//        Public Shared Sub delete(cpCore As coreClass, recordId As Integer)
//            Try
//                If (recordId > 0) Then
//                    cpCore.db.deleteContentRecords(primaryContentName, "id=" & recordId.ToString)
//                    cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, recordId))
//                End If
//            Catch ex As Exception
//                cpCore.handleException(ex); : Throw
//                Throw
//            End Try
//        End Sub
//        '
//        '====================================================================================================
//        ''' <summary>
//        ''' delete an existing database record by guid
//        ''' </summary>
//        ''' <param name="cp"></param>
//        ''' <param name="ccguid"></param>
//        Public Shared Sub delete(cpCore As coreClass, ccguid As String)
//            Try
//                If (Not String.IsNullOrEmpty(ccguid)) Then
//                    Dim instance As domainModel = create(cpCore, ccguid, New List(Of String))
//                    If (instance IsNot Nothing) Then
//                        invalidatePrimaryCache(cpCore, instance.id)
//                        cpCore.db.deleteContentRecords(primaryContentName, "(ccguid=" & cpCore.db.encodeSQLText(ccguid) & ")")
//                    End If
//                End If
//            Catch ex As Exception
//                cpCore.handleException(ex); : Throw
//                Throw
//            End Try
//        End Sub
//        '
//        '====================================================================================================
//        ''' <summary>
//        ''' pattern to delete an existing object based on multiple criteria (like a rule record)
//        ''' </summary>
//        ''' <param name="cp"></param>
//        ''' <param name="foreignKey1Id"></param>
//        ''' <param name="foreignKey2Id"></param>
//        Public Shared Sub delete(cpCore As coreClass, foreignKey1Id As Integer, foreignKey2Id As Integer)
//            Try
//                If (foreignKey2Id > 0) And (foreignKey1Id > 0) Then
//                    Dim instance As domainModel = create(cpCore, foreignKey1Id, foreignKey2Id, New List(Of String))
//                    If (instance IsNot Nothing) Then
//                        invalidatePrimaryCache(cpCore, instance.id)
//                        cpCore.db.deleteTableRecord(primaryContentTableName, instance.id, primaryContentDataSource)
//                    End If
//                End If
//            Catch ex As Exception
//                cpCore.handleException(ex); : Throw
//                Throw
//            End Try
//        End Sub
//        '
//        '====================================================================================================
//        ''' <summary>
//        ''' pattern get a list of objects from this model
//        ''' </summary>
//        ''' <param name="cp"></param>
//        ''' <param name="someCriteria"></param>
//        ''' <returns></returns>
//        Public Shared Function createList_criteria(cpCore As coreClass, someCriteria As Integer, callersCacheNameList As List(Of String)) As List(Of domainModel)
//            Dim result As New List(Of domainModel)
//            Try
//                Dim cs As New csController(cpCore)
//                Dim ignoreCacheNames As New List(Of String)
//                If (cs.open(primaryContentName, "(someCriteria=" & someCriteria & ")", "id")) Then
//                    Dim instance As domainModel
//                    Do
//                        instance = domainModel.loadRecord(cpCore, cs, callersCacheNameList)
//                        If (instance IsNot Nothing) Then
//                            result.Add(instance)
//                        End If
//                        cs.goNext()
//                    Loop While cs.ok()
//                End If
//                cs.Close()
//            Catch ex As Exception
//                cpCore.handleException(ex); : Throw
//            End Try
//            Return result
//        End Function
//        '
//        '====================================================================================================
//        ''' <summary>
//        ''' invalidate the primary key (which depends on all secondary keys)
//        ''' </summary>
//        ''' <param name="cpCore"></param>
//        ''' <param name="recordId"></param>
//        Public Shared Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
//            cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, recordId))
//            '
//            ' -- the zero record cache means any record was updated. Can be used to invalidate arbitraty lists of records in the table
//            cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", "0"))
//        End Sub
//        ''
//        ''====================================================================================================
//        '''' <summary>
//        '''' produce a standard format cachename for this model
//        '''' </summary>
//        '''' <param name="fieldName"></param>
//        '''' <param name="fieldValue"></param>
//        '''' <returns></returns>
//        'Private Shared Function Controllers.cacheController.getModelCacheName( primaryContentTableName,fieldName As String, fieldValue As String) As String
//        '    Return (primaryContentTableName & "-" & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
//        'End Function
//        ''
//        'Private Shared Function Controllers.cacheController.getModelCacheName( primaryContentTableName,field1Name As String, field1Value As String, field2Name As String, field2Value As String) As String
//        '    Return (primaryContentTableName & "-" & field1Name & "." & field1Value & "-" & field2Name & "." & field2Value).ToLower().Replace(" ", "_")
//        'End Function
//        '
//        '====================================================================================================
//        ''' <summary>
//        ''' get the name of the record by it's id
//        ''' </summary>
//        ''' <param name="cp"></param>
//        ''' <param name="recordId"></param>record
//        ''' <returns></returns>
//        Public Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
//            Return domainModel.create(cpcore, recordId, New List(Of String)).name
//        End Function
//        '
//        '====================================================================================================
//        ''' <summary>
//        ''' get the name of the record by it's guid 
//        ''' </summary>
//        ''' <param name="cp"></param>
//        ''' <param name="ccGuid"></param>record
//        ''' <returns></returns>
//        Public Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
//            Return domainModel.create(cpcore, ccGuid, New List(Of String)).name
//        End Function
//        '
//        '====================================================================================================
//        ''' <summary>
//        ''' get the id of the record by it's guid 
//        ''' </summary>
//        ''' <param name="cp"></param>
//        ''' <param name="ccGuid"></param>record
//        ''' <returns></returns>
//        Public Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
//            Return domainModel.create(cpcore, ccGuid, New List(Of String)).id
//        End Function
//        '
//        '====================================================================================================
//        '
//        Public Shared Function createDefault(cpcore As coreClass) As domainModel
//            Dim instance As New domainModel
//            Try
//                Dim CDef As Models.Complex.cdefModel = models.complex.cdefmodel.getcdef(cpcore,primaryContentName)
//                If (CDef Is Nothing) Then
//                    Throw New ApplicationException("content [" & primaryContentName & "] could Not be found.")
//                ElseIf (CDef.Id <= 0) Then
//                    Throw New ApplicationException("content [" & primaryContentName & "] could Not be found.")
//                Else
//                    With CDef
//                        instance.Active = genericController.EncodeBoolean(.fields("Active").defaultValue)
//                        instance.ccGuid = genericController.encodeText(.fields("ccGuid").defaultValue)
//                        'instance.ContentControlID = CDef.Id
//                        instance.CreatedBy = genericController.EncodeInteger(.fields("CreatedBy").defaultValue)
//                        instance.CreateKey = genericController.EncodeInteger(.fields("CreateKey").defaultValue)
//                        instance.DateAdded = genericController.EncodeDate(.fields("DateAdded").defaultValue)
//                        instance.DefaultTemplateId = genericController.EncodeInteger(.fields("DefaultTemplateId").defaultValue)
//                        instance.forwardDomainId = genericController.EncodeInteger(.fields("forwardDomainId").defaultValue)
//                        instance.ForwardURL = genericController.encodeText(.fields("ForwardURL").defaultValue)
//                        instance.ModifiedBy = genericController.EncodeInteger(.fields("ModifiedBy").defaultValue)
//                        instance.ModifiedDate = genericController.EncodeDate(.fields("ModifiedDate").defaultValue)
//                        instance.Name = genericController.encodeText(.fields("Name").defaultValue)
//                        instance.NoFollow = genericController.EncodeBoolean(.fields("NoFollow").defaultValue)
//                        instance.PageNotFoundPageID = genericController.EncodeInteger(.fields("PageNotFoundPageID").defaultValue)
//                        instance.RootPageID = genericController.EncodeInteger(.fields("RootPageID").defaultValue)
//                        instance.SortOrder = genericController.encodeText(.fields("SortOrder").defaultValue)
//                        instance.TypeID = genericController.EncodeInteger(.fields("TypeID").defaultValue)
//                        instance.Visited = genericController.EncodeBoolean(.fields("Visited").defaultValue)
//                    End With
//                End If
//            Catch ex As Exception
//                cpcore.handleException(ex)
//            End Try
//            Return instance
//        End Function
//    End Class
//End Namespace
