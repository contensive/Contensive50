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
//    Public Class sitePropertyModel
//        '
//        '-- const
//        Public Const primaryContentName As String = "site properties"
//        Private Const primaryContentTableName As String = "ccsetup"
//        Private Const primaryContentDataSource As String = "default"
//        '
//        ' -- instance properties
//        Public ID As Integer
//        'Public Active As Boolean
//        'Public ccGuid As String
//        'Public ContentControlID As Integer
//        'Public CreatedBy As Integer
//        'Public CreateKey As Integer
//        'Public DateAdded As Date
//        Public FieldValue As String
//        'Public ModifiedBy As Integer
//        'Public ModifiedDate As Date
//        Public Name As String
//        'Public SortOrder As String
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
//        Public Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As sitePropertyModel
//            Dim result As sitePropertyModel = Nothing
//            Try
//                result = create(cpCore, cpCore.db.metaData_InsertContentRecordGetID(primaryContentName, cpCore.doc.authContext.user.ID), callersCacheNameList)
//            Catch ex As Exception
//                cpCore.handleExceptionAndContinue(ex) : Throw
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
//        Public Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As sitePropertyModel
//            Dim result As sitePropertyModel = Nothing
//            Try
//                If recordId > 0 Then
//                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, recordId)
//                    result = cpCore.cache.getObject(Of sitePropertyModel)(cacheName)
//                    If (result Is Nothing) Then
//                        result = loadObject(cpCore, "id=" & recordId.ToString(), callersCacheNameList)
//                    End If
//                End If
//            Catch ex As Exception
//                cpCore.handleExceptionAndContinue(ex) : Throw
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
//        Public Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As sitePropertyModel
//            Dim result As sitePropertyModel = Nothing
//            Try
//                If Not String.IsNullOrEmpty(recordGuid) Then
//                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "ccguid", recordGuid)
//                    result = cpCore.cache.getObject(Of sitePropertyModel)(cacheName)
//                    If (result Is Nothing) Then
//                        result = loadObject(cpCore, "ccGuid=" & cpCore.db.encodeSQLText(recordGuid), callersCacheNameList)
//                    End If
//                End If
//            Catch ex As Exception
//                cpCore.handleExceptionAndContinue(ex) : Throw
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
//        Public Shared Function create(cpCore As coreClass, foreignKey1Id As Integer, foreignKey2Id As Integer, ByRef callersCacheNameList As List(Of String)) As sitePropertyModel
//            Dim result As sitePropertyModel = Nothing
//            Try
//                If ((foreignKey1Id > 0) And (foreignKey2Id > 0)) Then
//                    result = cpCore.cache.getObject(Of sitePropertyModel)(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "foreignKey1", foreignKey1Id.ToString(), "foreignKey2", foreignKey2Id.ToString()))
//                    If (result Is Nothing) Then
//                        result = loadObject(cpCore, "(foreignKey1=" & foreignKey1Id.ToString() & ")and(foreignKey1=" & foreignKey1Id.ToString() & ")", callersCacheNameList)
//                    End If
//                End If
//            Catch ex As Exception
//                cpCore.handleExceptionAndContinue(ex) : Throw
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
//        Private Shared Function loadObject(cpCore As coreClass, sqlCriteria As String, ByRef callersCacheNameList As List(Of String)) As sitePropertyModel
//            Dim result As sitePropertyModel = Nothing
//            Try
//                Dim cs As New csController(cpCore)
//                If cs.open(primaryContentName, sqlCriteria) Then
//                    result = New sitePropertyModel
//                    With result
//                        '
//                        ' -- populate result model

//                        .ID = cs.getInteger("ID")
//                        '.Active = cs.getBoolean("Active")
//                        '.ccGuid = cs.getText("ccGuid")
//                        ''
//                        '.ContentControlID = cs.getInteger("ContentControlID")
//                        '.CreatedBy = cs.getInteger("CreatedBy")
//                        '.CreateKey = cs.getInteger("CreateKey")
//                        '.DateAdded = cs.getDate("DateAdded")
//                        ''
//                        ''
//                        ''
//                        .FieldValue = cs.getText("FieldValue")
//                        '.ModifiedBy = cs.getInteger("ModifiedBy")
//                        '.ModifiedDate = cs.getDate("ModifiedDate")
//                        .Name = cs.getText("Name")
//                        '.SortOrder = cs.getText("SortOrder")
//                    End With
//                    If (result IsNot Nothing) Then
//                        '
//                        ' -- set primary cache to the object created
//                        ' -- set secondary caches to the primary cache
//                        ' -- add all cachenames to the injected cachenamelist
//                        'Dim cacheName0 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", result.id.ToString())
//                        'callersCacheNameList.Add(cacheName0)
//                        'cpCore.cache.setObject(cacheName0, result)
//                        ''
//                        'Dim cacheName1 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "ccguid", result.ccguid)
//                        'callersCacheNameList.Add(cacheName1)
//                        'cpCore.cache.setSecondaryObject(cacheName1, cacheName0)
//                        ''
//                        'Dim cacheName2 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "foreignKey1", result.foreignKey1Id.ToString(), "foreignKey2", result.foreignKey2Id.ToString())
//                        'callersCacheNameList.Add(cacheName2)
//                        'cpCore.cache.setSecondaryObject(cacheName2, cacheName0)
//                    End If
//                End If
//                Call cs.Close()
//            Catch ex As Exception
//                cpCore.handleExceptionAndContinue(ex) : Throw
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
//        Public Function saveObject(cpCore As coreClass) As Integer
//            Try
//                Dim cs As New csController(cpCore)
//                If (id > 0) Then
//                    If Not cs.open(primaryContentName, "id=" & id) Then
//                        Dim message As String = "Unable to open record in content [" & primaryContentName & "], with id [" & id & "]"
//                        cs.Close()
//                        id = 0
//                        Throw New ApplicationException(message)
//                    End If
//                Else
//                    If Not cs.Insert(primaryContentName) Then
//                        cs.Close()
//                        id = 0
//                        Throw New ApplicationException("Unable to insert record in content [" & primaryContentName & "]")
//                    End If
//                End If
//                If cs.ok() Then
//                    id = cs.getInteger("id")
//                    'cs.setField("Active", Active.ToString())
//                    'cs.setField("ccGuid", ccGuid)
//                    ''
//                    'cs.setField("ContentControlID", ContentControlID.ToString())
//                    'cs.setField("CreatedBy", CreatedBy.ToString())
//                    'cs.setField("CreateKey", CreateKey.ToString())
//                    'cs.setField("DateAdded", DateAdded.ToString())
//                    ''
//                    ';'
//                    ''
//                    cs.setField("FieldValue", FieldValue)
//                    'cs.setField("ModifiedBy", ModifiedBy.ToString())
//                    'cs.setField("ModifiedDate", ModifiedDate.ToString())
//                    cs.setField("Name", Name)
//                    'cs.setField("SortOrder", SortOrder)
//                    'If (String.IsNullOrEmpty(ccGuid)) Then ccGuid = Controllers.genericController.getGUID()
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
//                'cpCore.cache.setObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", Me.id.ToString()), Me)
//            Catch ex As Exception
//                cpCore.handleExceptionAndContinue(ex) : Throw
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
//                cpCore.handleExceptionAndContinue(ex) : Throw
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
//                    Dim instance As sitePropertyModel = create(cpCore, ccguid, New List(Of String))
//                    If (instance IsNot Nothing) Then
//                        invalidatePrimaryCache(cpCore, instance.id)
//                        cpCore.db.deleteContentRecords(primaryContentName, "(ccguid=" & cpCore.db.encodeSQLText(ccguid) & ")")
//                    End If
//                End If
//            Catch ex As Exception
//                cpCore.handleExceptionAndContinue(ex) : Throw
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
//                    Dim instance As sitePropertyModel = create(cpCore, foreignKey1Id, foreignKey2Id, New List(Of String))
//                    If (instance IsNot Nothing) Then
//                        invalidatePrimaryCache(cpCore, instance.id)
//                        cpCore.db.deleteTableRecord(primaryContentTableName, instance.id, primaryContentDataSource)
//                    End If
//                End If
//            Catch ex As Exception
//                cpCore.handleExceptionAndContinue(ex) : Throw
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
//        Public Shared Function getNameValueDict(cpCore As coreClass) As Dictionary(Of String, String)
//            Dim result As New Dictionary(Of String, String)
//            Try
//                Dim cs As New csController(cpCore)
//                Dim ignoreCacheNames As New List(Of String)
//                If (cs.open(primaryContentName, "", "name", True, "id,name,FieldValue")) Then
//                    Do
//                        Dim instance As New sitePropertyModel
//                        instance.ID = cs.getInteger("id")
//                        instance.Name = cs.getText("name")
//                        instance.FieldValue = cs.getText("FieldValue")
//                        If (instance IsNot Nothing) Then
//                            result.Add(cs.getText("name"), cs.getText("FieldValue"))
//                        End If
//                        cs.goNext()
//                    Loop While cs.ok()
//                End If
//                cs.Close()
//            Catch ex As Exception
//                cpCore.handleExceptionAndContinue(ex) : Throw
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
//            Return sitePropertyModel.create(cpcore, recordId, New List(Of String)).name
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
//            Return sitePropertyModel.create(cpcore, ccGuid, New List(Of String)).name
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
//            Return sitePropertyModel.create(cpcore, ccGuid, New List(Of String)).id
//        End Function
//        '
//        '====================================================================================================
//        '
//        Public Shared Function getDefault(cpcore As coreClass) As sitePropertyModel
//            Dim instance As New sitePropertyModel
//            Try
//                Dim CDef As coreMetaDataClass.CDefClass = models.complex.cdefmodel.getcdef(cpcore,primaryContentName)
//                If (CDef Is Nothing) Then
//                    Throw New ApplicationException("content [" & primaryContentName & "] could Not be found.")
//                ElseIf (CDef.Id <= 0) Then
//                    Throw New ApplicationException("content [" & primaryContentName & "] could Not be found.")
//                Else
//                    With CDef
//                        'instance.Active = genericController.EncodeBoolean(.fields("Active").defaultValue)
//                        'instance.ccGuid = genericController.encodeText(.fields("ccGuid").defaultValue)
//                        'instance.ContentControlID = CDef.Id
//                        'instance.CreatedBy = genericController.EncodeInteger(.fields("CreatedBy").defaultValue)
//                        'instance.CreateKey = genericController.EncodeInteger(.fields("CreateKey").defaultValue)
//                        'instance.DateAdded = genericController.EncodeDate(.fields("DateAdded").defaultValue)
//                        instance.FieldValue = genericController.encodeText(.fields("FieldValue").defaultValue)
//                        'instance.ModifiedBy = genericController.EncodeInteger(.fields("ModifiedBy").defaultValue)
//                        'instance.ModifiedDate = genericController.EncodeDate(.fields("ModifiedDate").defaultValue)
//                        instance.Name = genericController.encodeText(.fields("Name").defaultValue)
//                        'instance.SortOrder = genericController.encodeText(.fields("SortOrder").defaultValue)
//                    End With
//                End If
//            Catch ex As Exception
//                cpcore.handleExceptionAndContinue(ex)
//            End Try
//            Return instance
//        End Function
//    End Class
//End Namespace
