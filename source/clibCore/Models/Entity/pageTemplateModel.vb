
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class pageTemplateModel
        '
        '-- const
        Public Const primaryContentName As String = "page templates"
        Private Const primaryContentTableName As String = "ccpagetemplates"
        Private Const primaryContentDataSource As String = "default"
        '
        ' -- instance properties
        Public Property ID As Integer
        Public Property Active As Boolean
        Public Property BodyHTML As String
        ' Public Property BodyTag As String
        Public Property ccGuid As String
        Public Property ContentControlID As Integer
        Public Property CreatedBy As Integer
        Public Property CreateKey As Integer
        Public Property DateAdded As Date
        Public Property IsSecure As Boolean
        ' Public Property JSEndBody As String
        ' Public Property JSFilename As String
        ' Public Property JSHead As String
        ' Public Property JSOnLoad As String
        ' Public Property Link As String
        ' Public Property MobileBodyHTML As String
        Public Property ModifiedBy As Integer
        Public Property ModifiedDate As Date
        Public Property Name As String
        ' Public Property OtherHeadTags As String
        Public Property SortOrder As String
        ' Public Property Source As String
        ' Public Property StylesFilename As String
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
        ''' <param name="callersCacheNameList"></param>
        ''' <returns></returns>
        Public Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As pageTemplateModel
            Dim result As pageTemplateModel = Nothing
            Try
                result = create(cpCore, cpCore.db.metaData_InsertContentRecordGetID(primaryContentName, cpCore.authContext.user.id), callersCacheNameList)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
        ''' <param name="callersCacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
        Public Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As pageTemplateModel
            Dim result As pageTemplateModel = Nothing
            Try
                If recordId > 0 Then
                    Dim cacheName As String = Controllers.cacheController.getCacheName_Entity(primaryContentTableName, recordId)
                    result = cpCore.cache.getObject(Of pageTemplateModel)(cacheName)
                    If (result Is Nothing) Then
                        Using cs As New csController(cpCore)
                            If cs.open(primaryContentName, "(id=" & recordId.ToString() & ")") Then
                                result = loadRecord(cpCore, cs, callersCacheNameList)
                            End If
                        End Using
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
        Public Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As pageTemplateModel
            Dim result As pageTemplateModel = Nothing
            Try
                If Not String.IsNullOrEmpty(recordGuid) Then
                    Dim cacheName As String = Controllers.cacheController.getCacheName_Entity(primaryContentTableName, "ccguid", recordGuid)
                    result = cpCore.cache.getObject(Of pageTemplateModel)(cacheName)
                    If (result Is Nothing) Then
                        Using cs As New csController(cpCore)
                            If cs.open(primaryContentName, "(ccGuid=" & cpCore.db.encodeSQLText(recordGuid) & ")") Then
                                result = loadRecord(cpCore, cs, callersCacheNameList)
                            End If
                        End Using
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
        Public Shared Function create(cpCore As coreClass, foreignKey1Id As Integer, foreignKey2Id As Integer, ByRef callersCacheNameList As List(Of String)) As pageTemplateModel
            Dim result As pageTemplateModel = Nothing
            Try
                If ((foreignKey1Id > 0) And (foreignKey2Id > 0)) Then
                    result = cpCore.cache.getObject(Of pageTemplateModel)(Controllers.cacheController.getCacheName_Entity(primaryContentTableName, "foreignKey1", foreignKey1Id.ToString(), "foreignKey2", foreignKey2Id.ToString()))
                    If (result Is Nothing) Then
                        Using cs As New csController(cpCore)
                            If cs.open(primaryContentName, "(foreignKey1=" & foreignKey1Id.ToString() & ")and(foreignKey1=" & foreignKey1Id.ToString() & ")") Then
                                result = loadRecord(cpCore, cs, callersCacheNameList)
                            End If
                        End Using
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
        ''' <param name="recordName"></param>
        Public Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As pageTemplateModel
            Dim result As pageTemplateModel = Nothing
            Try
                If Not String.IsNullOrEmpty(recordName) Then
                    Dim cacheName As String = Controllers.cacheController.getCacheName_Entity(primaryContentTableName, "name", recordName)
                    result = cpCore.cache.getObject(Of pageTemplateModel)(cacheName)
                    If (result Is Nothing) Then
                        Using cs As New csController(cpCore)
                            If cs.open(primaryContentName, "(name=" & cpCore.db.encodeSQLText(recordName) & ")", "id") Then
                                result = loadRecord(cpCore, cs, callersCacheNameList)
                            End If
                        End Using
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
        Private Shared Function loadRecord(cpCore As coreClass, cs As csController, ByRef callersCacheNameList As List(Of String)) As pageTemplateModel
            Dim result As pageTemplateModel = Nothing
            Try
                If cs.ok() Then
                    result = New pageTemplateModel
                    With result
                        '
                        ' -- populate result model
                        .ID = cs.getInteger("ID")
                        .Active = cs.getBoolean("Active")
                        .BodyHTML = cs.getText("BodyHTML")
                        ' .BodyTag = cs.getText("BodyTag")
                        .ccGuid = cs.getText("ccGuid")
                        ''
                        .ContentControlID = cs.getInteger("ContentControlID")
                        .CreatedBy = cs.getInteger("CreatedBy")
                        .CreateKey = cs.getInteger("CreateKey")
                        .DateAdded = cs.getDate("DateAdded")
                        ''
                        ''
                        ''
                        .IsSecure = cs.getBoolean("IsSecure")
                        ' .JSEndBody = cs.getText("JSEndBody")
                        ' .JSFilename = cs.getText("JSFilename")
                        ' .JSHead = cs.getText("JSHead")
                        ' .JSOnLoad = cs.getText("JSOnLoad")
                        ' .Link = cs.getText("Link")
                        ' .MobileBodyHTML = cs.getText("MobileBodyHTML")
                        .ModifiedBy = cs.getInteger("ModifiedBy")
                        .ModifiedDate = cs.getDate("ModifiedDate")
                        .Name = cs.getText("Name")
                        ' .OtherHeadTags = cs.getText("OtherHeadTags")
                        .SortOrder = cs.getText("SortOrder")
                        ' .Source = cs.getText("Source")
                        ' .StylesFilename = cs.getText("StylesFilename")
                    End With
                    If (result IsNot Nothing) Then
                        '
                        ' -- set primary cache to the object created
                        ' -- set secondary caches to the primary cache
                        ' -- add all cachenames to the injected cachenamelist
                        Dim cacheName0 As String = Controllers.cacheController.getCacheName_Entity(primaryContentTableName, "id", result.ID.ToString())
                        callersCacheNameList.Add(cacheName0)
                        cpCore.cache.setObject(cacheName0, result)
                        '
                        Dim cacheName1 As String = Controllers.cacheController.getCacheName_Entity(primaryContentTableName, "ccguid", result.ccGuid)
                        callersCacheNameList.Add(cacheName1)
                        cpCore.cache.setSecondaryObject(cacheName1, cacheName0)
                        '
                        'Dim cacheName2 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "foreignKey1", result.foreignKey1Id.ToString(), "foreignKey2", result.foreignKey2Id.ToString())
                        'callersCacheNameList.Add(cacheName2)
                        'cpCore.cache.setSecondaryObject(cacheName2, cacheName0)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
        Public Function save(cpCore As coreClass) As Integer
            Try
                Dim cs As New csController(cpCore)
                If (ID > 0) Then
                    If Not cs.open(primaryContentName, "id=" & ID) Then
                        Dim message As String = "Unable to open record in content [" & primaryContentName & "], with id [" & ID & "]"
                        cs.Close()
                        ID = 0
                        Throw New ApplicationException(message)
                    End If
                Else
                    If Not cs.Insert(primaryContentName) Then
                        cs.Close()
                        ID = 0
                        Throw New ApplicationException("Unable to insert record in content [" & primaryContentName & "]")
                    End If
                End If
                If cs.ok() Then
                    ID = cs.getInteger("id")
                    cs.setField("Active", Active.ToString())
                    cs.setField("BodyHTML", BodyHTML)
                    ' cs.setField("BodyTag", BodyTag)
                    cs.setField("ccGuid", ccGuid)
                    '
                    cs.setField("ContentControlID", ContentControlID.ToString())
                    cs.setField("CreatedBy", CreatedBy.ToString())
                    cs.setField("CreateKey", CreateKey.ToString())
                    cs.setField("DateAdded", DateAdded.ToString())
                    ''''
                    ''
                    ''
                    cs.setField("IsSecure", IsSecure.ToString())
                    ' cs.setField("JSEndBody", JSEndBody)
                    ' cs.setField("JSFilename", JSFilename)
                    ' cs.setField("JSHead", JSHead)
                    ' cs.setField("JSOnLoad", JSOnLoad)
                    ' cs.setField("Link", Link)
                    ' cs.setField("MobileBodyHTML", MobileBodyHTML)
                    cs.setField("ModifiedBy", ModifiedBy.ToString())
                    cs.setField("ModifiedDate", ModifiedDate.ToString())
                    cs.setField("Name", Name)
                    ' cs.setField("OtherHeadTags", OtherHeadTags)
                    cs.setField("SortOrder", SortOrder)
                    ' cs.setField("Source", Source)
                    ' cs.setField("StylesFilename", StylesFilename)
                    If (String.IsNullOrEmpty(ccGuid)) Then ccGuid = Controllers.genericController.getGUID()
                End If
                Call cs.Close()
                '
                ' -- invalidate objects
                ' -- no, the primary is invalidated by the cs.save()
                'cpCore.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"id", id.ToString))
                ' -- no, the secondary points to the pirmary, which is invalidated. Dont waste resources invalidating
                'cpCore.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"ccguid", ccguid))
                '
                ' -- object is here, but the cache was invalidated, setting
                cpCore.cache.setObject(Controllers.cacheController.getCacheName_Entity(primaryContentTableName, "id", Me.ID.ToString()), Me)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw
            End Try
            Return ID
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
                    cpCore.cache.invalidateObject(Controllers.cacheController.getCacheName_Entity(primaryContentTableName, recordId))
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' delete an existing database record by guid
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ccguid"></param>
        Public Shared Sub delete(cpCore As coreClass, ccguid As String)
            Try
                If (Not String.IsNullOrEmpty(ccguid)) Then
                    Dim instance As pageTemplateModel = create(cpCore, ccguid, New List(Of String))
                    If (instance IsNot Nothing) Then
                        invalidatePrimaryCache(cpCore, instance.ID)
                        cpCore.db.deleteContentRecords(primaryContentName, "(ccguid=" & cpCore.db.encodeSQLText(ccguid) & ")")
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
                    Dim instance As pageTemplateModel = create(cpCore, foreignKey1Id, foreignKey2Id, New List(Of String))
                    If (instance IsNot Nothing) Then
                        invalidatePrimaryCache(cpCore, instance.ID)
                        cpCore.db.deleteTableRecord(primaryContentTableName, instance.ID, primaryContentDataSource)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
        Public Shared Function createList_criteria(cpCore As coreClass, someCriteria As Integer, callersCacheNameList As List(Of String)) As List(Of pageTemplateModel)
            Dim result As New List(Of pageTemplateModel)
            Try
                Dim cs As New csController(cpCore)
                Dim ignoreCacheNames As New List(Of String)
                If (cs.open(primaryContentName, "(someCriteria=" & someCriteria & ")", "id")) Then
                    Dim instance As pageTemplateModel
                    Do
                        instance = pageTemplateModel.loadRecord(cpCore, cs, callersCacheNameList)
                        If (instance IsNot Nothing) Then
                            result.Add(instance)
                        End If
                        cs.goNext()
                    Loop While cs.ok()
                End If
                cs.Close()
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
            cpCore.cache.invalidateObject(Controllers.cacheController.getCacheName_Entity(primaryContentTableName, recordId))
            '
            ' -- the zero record cache means any record was updated. Can be used to invalidate arbitraty lists of records in the table
            cpCore.cache.invalidateObject(Controllers.cacheController.getCacheName_Entity(primaryContentTableName, "id", "0"))
        End Sub
        ''
        ''====================================================================================================
        '''' <summary>
        '''' produce a standard format cachename for this model
        '''' </summary>
        '''' <param name="fieldName"></param>
        '''' <param name="fieldValue"></param>
        '''' <returns></returns>
        'Private Shared Function Controllers.cacheController.getModelCacheName( primaryContentTableName,fieldName As String, fieldValue As String) As String
        '    Return (primaryContentTableName & "-" & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
        'End Function
        ''
        'Private Shared Function Controllers.cacheController.getModelCacheName( primaryContentTableName,field1Name As String, field1Value As String, field2Name As String, field2Value As String) As String
        '    Return (primaryContentTableName & "-" & field1Name & "." & field1Value & "-" & field2Name & "." & field2Value).ToLower().Replace(" ", "_")
        'End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get the name of the record by it's id
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>record
        ''' <returns></returns>
        Public Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return pageTemplateModel.create(cpcore, recordId, New List(Of String)).name
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get the name of the record by it's guid 
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ccGuid"></param>record
        ''' <returns></returns>
        Public Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
            Return pageTemplateModel.create(cpcore, ccGuid, New List(Of String)).name
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get the id of the record by it's guid 
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ccGuid"></param>record
        ''' <returns></returns>
        Public Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
            Return pageTemplateModel.create(cpcore, ccGuid, New List(Of String)).id
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function createDefault(cpcore As coreClass) As pageTemplateModel
            Dim instance As New pageTemplateModel
            Try
                Dim CDef As cdefModel = cpcore.metaData.getCdef(primaryContentName)
                If (CDef Is Nothing) Then
                    Throw New ApplicationException("content [" & primaryContentName & "] could Not be found.")
                ElseIf (CDef.Id <= 0) Then
                    Throw New ApplicationException("content [" & primaryContentName & "] could Not be found.")
                Else
                    With CDef
                        instance.Active = genericController.EncodeBoolean(.fields("Active").defaultValue)
                        instance.BodyHTML = genericController.encodeText(.fields("BodyHTML").defaultValue)
                        ' instance.BodyTag = genericController.encodeText(.fields("BodyTag").defaultValue)
                        instance.ccGuid = genericController.encodeText(.fields("ccGuid").defaultValue)
                        instance.ContentControlID = CDef.Id
                        instance.CreatedBy = genericController.EncodeInteger(.fields("CreatedBy").defaultValue)
                        instance.CreateKey = genericController.EncodeInteger(.fields("CreateKey").defaultValue)
                        instance.DateAdded = genericController.EncodeDate(.fields("DateAdded").defaultValue)
                        instance.IsSecure = genericController.EncodeBoolean(.fields("IsSecure").defaultValue)
                        ' instance.JSEndBody = genericController.encodeText(.fields("JSEndBody").defaultValue)
                        ' instance.JSFilename = genericController.encodeText(.fields("JSFilename").defaultValue)
                        ' instance.JSHead = genericController.encodeText(.fields("JSHead").defaultValue)
                        ' instance.JSOnLoad = genericController.encodeText(.fields("JSOnLoad").defaultValue)
                        ' instance.Link = genericController.encodeText(.fields("Link").defaultValue)
                        ' instance.MobileBodyHTML = genericController.encodeText(.fields("MobileBodyHTML").defaultValue)
                        instance.ModifiedBy = genericController.EncodeInteger(.fields("ModifiedBy").defaultValue)
                        instance.ModifiedDate = genericController.EncodeDate(.fields("ModifiedDate").defaultValue)
                        instance.Name = genericController.encodeText(.fields("Name").defaultValue)
                        ' instance.OtherHeadTags = genericController.encodeText(.fields("OtherHeadTags").defaultValue)
                        instance.SortOrder = genericController.encodeText(.fields("SortOrder").defaultValue)
                        ' instance.Source = genericController.encodeText(.fields("Source").defaultValue)
                        ' instance.StylesFilename = genericController.encodeText(.fields("StylesFilename").defaultValue)
                    End With
                End If
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
            Return instance
        End Function
    End Class
End Namespace
