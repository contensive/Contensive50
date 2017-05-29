
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
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
    '	2) find-And-replace "addonModel" with the name for this model
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
    Public Class addonModel
        '
        '-- const
        Public Const primaryContentName As String = "add-ons" '<------ set content name
        Private Const primaryContentTableName As String = "ccaggregatefunctions" '<------ set to tablename for the primary content (used for cache names)
        Private Const primaryContentDataSource As String = "default" '<----- set to datasource if not default
        '
        Public ID As Integer
        Public Active As Boolean
        Public Admin As Boolean
        Public ArgumentList As String
        Public AsAjax As Boolean
        Public BlockDefaultStyles As Boolean
        Public BlockEditTools As Boolean
        Public ccGuid As String
        Public CollectionID As Integer
        Public Content As Boolean
        Public ContentCategoryID As Integer
        Public ContentControlID As Integer
        Public Copy As String
        Public CopyText As String
        Public CreatedBy As Integer
        Public CreateKey As Integer
        Public CustomStylesFilename As String
        Public DateAdded As Date
        Public DotNetClass As String
        Public EditArchive As Boolean
        Public EditBlank As Boolean
        Public EditSourceID As Integer
        Public Email As Boolean
        Public Filter As Boolean
        Public FormXML As String
        Public Help As String
        Public HelpLink As String
        Public IconFilename As String
        Public IconHeight As Integer
        Public IconSprites As Integer
        Public IconWidth As Integer
        Public InFrame As Boolean
        Public inlineScript As String
        Public IsInline As Boolean
        Public JavaScriptBodyEnd As String
        Public JavaScriptOnLoad As String
        Public JSFilename As String
        Public Link As String
        Public MetaDescription As String
        Public MetaKeywordList As String
        Public ModifiedBy As Integer
        Public ModifiedDate As Date
        Public Name As String
        Public NavTypeID As Integer
        Public ObjectProgramID As String
        Public OnBodyEnd As Boolean
        Public OnBodyStart As Boolean
        Public OnNewVisitEvent As Boolean
        Public OnPageEndEvent As Boolean
        Public OnPageStartEvent As Boolean
        Public OtherHeadTags As String
        Public PageTitle As String
        Public ProcessInterval As Integer
        Public ProcessNextRun As Date
        Public ProcessRunOnce As Boolean
        Public ProcessServerKey As String
        Public RemoteAssetLink As String
        Public RemoteMethod As Boolean
        Public RobotsTxt As String
        Public ScriptingCode As String
        Public ScriptingEntryPoint As String
        Public ScriptingLanguageID As Integer
        Public ScriptingTimeout As String
        Public SortOrder As String
        Public StylesFilename As String
        Public Template As Boolean
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
        Public Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As addonModel
            Dim result As addonModel = Nothing
            Try
                result = create(cpCore, cpCore.db.metaData_InsertContentRecordGetID(primaryContentName, cpCore.authContext.user.ID), callersCacheNameList)
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
        ''' <param name="callersCacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
        Public Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As addonModel
            Dim result As addonModel = Nothing
            Try
                If recordId > 0 Then
                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", recordId.ToString())
                    result = cpCore.cache.getObject(Of addonModel)(cacheName)
                    If (result Is Nothing) Then
                        Using cs As New csController(cpCore)
                            If cs.open(primaryContentName, "(id=" & recordId.ToString() & ")") Then
                                result = loadRecord(cpCore, cs, callersCacheNameList)
                            End If
                        End Using
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
        Public Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As addonModel
            Dim result As addonModel = Nothing
            Try
                If Not String.IsNullOrEmpty(recordGuid) Then
                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "ccguid", recordGuid)
                    result = cpCore.cache.getObject(Of addonModel)(cacheName)
                    If (result Is Nothing) Then
                        Using cs As New csController(cpCore)
                            If cs.open(primaryContentName, "(ccGuid=" & cpCore.db.encodeSQLText(recordGuid) & ")") Then
                                result = loadRecord(cpCore, cs, callersCacheNameList)
                            End If
                        End Using
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
        Public Shared Function create(cpCore As coreClass, foreignKey1Id As Integer, foreignKey2Id As Integer, ByRef callersCacheNameList As List(Of String)) As addonModel
            Dim result As addonModel = Nothing
            Try
                If ((foreignKey1Id > 0) And (foreignKey2Id > 0)) Then
                    result = cpCore.cache.getObject(Of addonModel)(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "foreignKey1", foreignKey1Id.ToString(), "foreignKey2", foreignKey2Id.ToString()))
                    If (result Is Nothing) Then
                        Using cs As New csController(cpCore)
                            If cs.open(primaryContentName, "(foreignKey1=" & foreignKey1Id.ToString() & ")and(foreignKey1=" & foreignKey1Id.ToString() & ")") Then
                                result = loadRecord(cpCore, cs, callersCacheNameList)
                            End If
                        End Using
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
        Private Shared Function loadRecord(cpCore As coreClass, cs As csController, ByRef callersCacheNameList As List(Of String)) As addonModel
            Dim result As addonModel = Nothing
            Try
                If cs.ok() Then
                    result = New addonModel
                    With result
                        '
                        ' -- populate result model
                        .ID = cs.getInteger("ID")
                        .Active = cs.getBoolean("Active")
                        .Admin = cs.getBoolean("Admin")
                        .ArgumentList = cs.getText("ArgumentList")
                        .AsAjax = cs.getBoolean("AsAjax")
                        .BlockDefaultStyles = cs.getBoolean("BlockDefaultStyles")
                        .BlockEditTools = cs.getBoolean("BlockEditTools")
                        .ccGuid = cs.getText("ccGuid")
                        .CollectionID = cs.getInteger("CollectionID")
                        .Content = cs.getBoolean("Content")
                        .ContentCategoryID = cs.getInteger("ContentCategoryID")
                        .ContentControlID = cs.getInteger("ContentControlID")
                        .Copy = cs.getText("Copy")
                        .CopyText = cs.getText("CopyText")
                        .CreatedBy = cs.getInteger("CreatedBy")
                        .CreateKey = cs.getInteger("CreateKey")
                        .CustomStylesFilename = cs.getText("CustomStylesFilename")
                        .DateAdded = cs.getDate("DateAdded")
                        .DotNetClass = cs.getText("DotNetClass")
                        .EditArchive = cs.getBoolean("EditArchive")
                        .EditBlank = cs.getBoolean("EditBlank")
                        .EditSourceID = cs.getInteger("EditSourceID")
                        .Email = cs.getBoolean("Email")
                        .Filter = cs.getBoolean("Filter")
                        .FormXML = cs.getText("FormXML")
                        .Help = cs.getText("Help")
                        .HelpLink = cs.getText("HelpLink")
                        .IconFilename = cs.getText("IconFilename")
                        .IconHeight = cs.getInteger("IconHeight")
                        .IconSprites = cs.getInteger("IconSprites")
                        .IconWidth = cs.getInteger("IconWidth")
                        .InFrame = cs.getBoolean("InFrame")
                        .inlineScript = cs.getText("inlineScript")
                        .IsInline = cs.getBoolean("IsInline")
                        .JavaScriptBodyEnd = cs.getText("JavaScriptBodyEnd")
                        .JavaScriptOnLoad = cs.getText("JavaScriptOnLoad")
                        .JSFilename = cs.getText("JSFilename")
                        .Link = cs.getText("Link")
                        .MetaDescription = cs.getText("MetaDescription")
                        .MetaKeywordList = cs.getText("MetaKeywordList")
                        .ModifiedBy = cs.getInteger("ModifiedBy")
                        .ModifiedDate = cs.getDate("ModifiedDate")
                        .Name = cs.getText("Name")
                        .NavTypeID = cs.getInteger("NavTypeID")
                        .ObjectProgramID = cs.getText("ObjectProgramID")
                        .OnBodyEnd = cs.getBoolean("OnBodyEnd")
                        .OnBodyStart = cs.getBoolean("OnBodyStart")
                        .OnNewVisitEvent = cs.getBoolean("OnNewVisitEvent")
                        .OnPageEndEvent = cs.getBoolean("OnPageEndEvent")
                        .OnPageStartEvent = cs.getBoolean("OnPageStartEvent")
                        .OtherHeadTags = cs.getText("OtherHeadTags")
                        .PageTitle = cs.getText("PageTitle")
                        .ProcessInterval = cs.getInteger("ProcessInterval")
                        .ProcessNextRun = cs.getDate("ProcessNextRun")
                        .ProcessRunOnce = cs.getBoolean("ProcessRunOnce")
                        .ProcessServerKey = cs.getText("ProcessServerKey")
                        .RemoteAssetLink = cs.getText("RemoteAssetLink")
                        .RemoteMethod = cs.getBoolean("RemoteMethod")
                        .RobotsTxt = cs.getText("RobotsTxt")
                        .ScriptingCode = cs.getText("ScriptingCode")
                        .ScriptingEntryPoint = cs.getText("ScriptingEntryPoint")
                        .ScriptingLanguageID = cs.getInteger("ScriptingLanguageID")
                        .ScriptingTimeout = cs.getText("ScriptingTimeout")
                        .SortOrder = cs.getText("SortOrder")
                        .StylesFilename = cs.getText("StylesFilename")
                        .Template = cs.getBoolean("Template")
                    End With
                    If (result IsNot Nothing) Then
                        '
                        ' -- set primary cache to the object created
                        ' -- set secondary caches to the primary cache
                        ' -- add all cachenames to the injected cachenamelist
                        Dim cacheName0 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", result.ID.ToString())
                        callersCacheNameList.Add(cacheName0)
                        cpCore.cache.setObject(cacheName0, result)
                        '
                        Dim cacheName1 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "ccguid", result.ccGuid)
                        callersCacheNameList.Add(cacheName1)
                        cpCore.cache.setSecondaryObject(cacheName1, cacheName0)
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
                    cs.setField("Admin", Admin.ToString())
                    cs.setField("ArgumentList", ArgumentList)
                    cs.setField("AsAjax", AsAjax.ToString())
                    cs.setField("BlockDefaultStyles", BlockDefaultStyles.ToString())
                    cs.setField("BlockEditTools", BlockEditTools.ToString())
                    cs.setField("ccGuid", ccGuid)
                    cs.setField("CollectionID", CollectionID.ToString())
                    cs.setField("Content", Content.ToString())
                    cs.setField("ContentCategoryID", ContentCategoryID.ToString())
                    cs.setField("ContentControlID", ContentControlID.ToString())
                    cs.setField("Copy", Copy)
                    cs.setField("CopyText", CopyText)
                    cs.setField("CreatedBy", CreatedBy.ToString())
                    cs.setField("CreateKey", CreateKey.ToString())
                    cs.setField("CustomStylesFilename", CustomStylesFilename)
                    cs.setField("DateAdded", DateAdded.ToString())
                    cs.setField("DotNetClass", DotNetClass)
                    cs.setField("EditArchive", EditArchive.ToString())
                    cs.setField("EditBlank", EditBlank.ToString())
                    cs.setField("EditSourceID", EditSourceID.ToString())
                    cs.setField("Email", Email.ToString())
                    cs.setField("Filter", Filter.ToString())
                    cs.setField("FormXML", FormXML)
                    cs.setField("Help", Help)
                    cs.setField("HelpLink", HelpLink)
                    cs.setField("IconFilename", IconFilename)
                    cs.setField("IconHeight", IconHeight.ToString())
                    cs.setField("IconSprites", IconSprites.ToString())
                    cs.setField("IconWidth", IconWidth.ToString())
                    cs.setField("InFrame", InFrame.ToString())
                    cs.setField("inlineScript", inlineScript)
                    cs.setField("IsInline", IsInline.ToString())
                    cs.setField("JavaScriptBodyEnd", JavaScriptBodyEnd)
                    cs.setField("JavaScriptOnLoad", JavaScriptOnLoad)
                    cs.setField("JSFilename", JSFilename)
                    cs.setField("Link", Link)
                    cs.setField("MetaDescription", MetaDescription)
                    cs.setField("MetaKeywordList", MetaKeywordList)
                    cs.setField("ModifiedBy", ModifiedBy.ToString())
                    cs.setField("ModifiedDate", ModifiedDate.ToString())
                    cs.setField("Name", Name)
                    cs.setField("NavTypeID", NavTypeID.ToString())
                    cs.setField("ObjectProgramID", ObjectProgramID)
                    cs.setField("OnBodyEnd", OnBodyEnd.ToString())
                    cs.setField("OnBodyStart", OnBodyStart.ToString())
                    cs.setField("OnNewVisitEvent", OnNewVisitEvent.ToString())
                    cs.setField("OnPageEndEvent", OnPageEndEvent.ToString())
                    cs.setField("OnPageStartEvent", OnPageStartEvent.ToString())
                    cs.setField("OtherHeadTags", OtherHeadTags)
                    cs.setField("PageTitle", PageTitle)
                    cs.setField("ProcessInterval", ProcessInterval.ToString())
                    cs.setField("ProcessNextRun", ProcessNextRun.ToString())
                    cs.setField("ProcessRunOnce", ProcessRunOnce.ToString())
                    cs.setField("ProcessServerKey", ProcessServerKey)
                    cs.setField("RemoteAssetLink", RemoteAssetLink)
                    cs.setField("RemoteMethod", RemoteMethod.ToString())
                    cs.setField("RobotsTxt", RobotsTxt)
                    cs.setField("ScriptingCode", ScriptingCode)
                    cs.setField("ScriptingEntryPoint", ScriptingEntryPoint)
                    cs.setField("ScriptingLanguageID", ScriptingLanguageID.ToString())
                    cs.setField("ScriptingTimeout", ScriptingTimeout)
                    cs.setField("SortOrder", SortOrder)
                    cs.setField("StylesFilename", StylesFilename)
                    cs.setField("Template", Template.ToString())
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
                cpCore.cache.setObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", Me.ID.ToString()), Me)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
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
                    cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", recordId.ToString))
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
        ''' <param name="ccguid"></param>
        Public Shared Sub delete(cpCore As coreClass, ccguid As String)
            Try
                If (Not String.IsNullOrEmpty(ccguid)) Then
                    Dim instance As addonModel = create(cpCore, ccguid, New List(Of String))
                    If (instance IsNot Nothing) Then
                        invalidatePrimaryCache(cpCore, instance.ID)
                        cpCore.db.deleteContentRecords(primaryContentName, "(ccguid=" & cpCore.db.encodeSQLText(ccguid) & ")")
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
        ''' pattern to delete an existing object based on multiple criteria (like a rule record)
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="foreignKey1Id"></param>
        ''' <param name="foreignKey2Id"></param>
        Public Shared Sub delete(cpCore As coreClass, foreignKey1Id As Integer, foreignKey2Id As Integer)
            Try
                If (foreignKey2Id > 0) And (foreignKey1Id > 0) Then
                    Dim instance As addonModel = create(cpCore, foreignKey1Id, foreignKey2Id, New List(Of String))
                    If (instance IsNot Nothing) Then
                        invalidatePrimaryCache(cpCore, instance.ID)
                        cpCore.db.deleteTableRecord(primaryContentTableName, instance.ID, primaryContentDataSource)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
        End Sub
        'getAddonList_OnNewVisitEvent
        '
        '====================================================================================================
        ''' <summary>
        ''' pattern get a list of objects from this model
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="someCriteria"></param>
        ''' <returns></returns>
        Public Shared Function createList_RemoteMethods(cpCore As coreClass, callersCacheNameList As List(Of String)) As List(Of addonModel)
            Dim result As New List(Of addonModel)
            Try
                Dim cs As New csController(cpCore)
                Dim ignoreCacheNames As New List(Of String)
                If (cs.open(primaryContentName, "(remoteMethod=1)", "name")) Then
                    Dim instance As addonModel
                    Do
                        instance = addonModel.loadRecord(cpCore, cs, callersCacheNameList)
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
            cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", recordId.ToString))
            '
            ' -- the zero record cache means any record was updated. Can be used to invalidate arbitraty lists of records in the table
            cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", "0"))
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' get the name of the record by it's id
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>record
        ''' <returns></returns>
        Public Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return addonModel.create(cpcore, recordId, New List(Of String)).Name
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
            Return addonModel.create(cpcore, ccGuid, New List(Of String)).Name
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
            Return addonModel.create(cpcore, ccGuid, New List(Of String)).ID
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function createDefault(cpcore As coreClass) As addonModel
            Dim instance As New addonModel
            Try
                Dim CDef As coreMetaDataClass.CDefClass = cpcore.metaData.getCdef(primaryContentName)
                If (CDef Is Nothing) Then
                    Throw New ApplicationException("content [" & primaryContentName & "] could Not be found.")
                ElseIf (CDef.Id <= 0) Then
                    Throw New ApplicationException("content [" & primaryContentName & "] could Not be found.")
                Else
                    With CDef
                        instance.Active = genericController.EncodeBoolean(.fields("Active").defaultValue)
                        instance.Admin = genericController.EncodeBoolean(.fields("Admin").defaultValue)
                        instance.ArgumentList = genericController.encodeText(.fields("ArgumentList").defaultValue)
                        instance.AsAjax = genericController.EncodeBoolean(.fields("AsAjax").defaultValue)
                        instance.BlockDefaultStyles = genericController.EncodeBoolean(.fields("BlockDefaultStyles").defaultValue)
                        instance.BlockEditTools = genericController.EncodeBoolean(.fields("BlockEditTools").defaultValue)
                        instance.ccGuid = genericController.encodeText(.fields("ccGuid").defaultValue)
                        instance.CollectionID = genericController.EncodeInteger(.fields("CollectionID").defaultValue)
                        instance.Content = genericController.EncodeBoolean(.fields("Content").defaultValue)
                        instance.ContentCategoryID = genericController.EncodeInteger(.fields("ContentCategoryID").defaultValue)
                        instance.ContentControlID = CDef.Id
                        instance.Copy = genericController.encodeText(.fields("Copy").defaultValue)
                        instance.CopyText = genericController.encodeText(.fields("CopyText").defaultValue)
                        instance.CreatedBy = genericController.EncodeInteger(.fields("CreatedBy").defaultValue)
                        instance.CreateKey = genericController.EncodeInteger(.fields("CreateKey").defaultValue)
                        instance.CustomStylesFilename = genericController.encodeText(.fields("CustomStylesFilename").defaultValue)
                        instance.DateAdded = genericController.EncodeDate(.fields("DateAdded").defaultValue)
                        instance.DotNetClass = genericController.encodeText(.fields("DotNetClass").defaultValue)
                        instance.EditArchive = genericController.EncodeBoolean(.fields("EditArchive").defaultValue)
                        instance.EditBlank = genericController.EncodeBoolean(.fields("EditBlank").defaultValue)
                        instance.EditSourceID = genericController.EncodeInteger(.fields("EditSourceID").defaultValue)
                        instance.Email = genericController.EncodeBoolean(.fields("Email").defaultValue)
                        instance.Filter = genericController.EncodeBoolean(.fields("Filter").defaultValue)
                        instance.FormXML = genericController.encodeText(.fields("FormXML").defaultValue)
                        instance.Help = genericController.encodeText(.fields("Help").defaultValue)
                        instance.HelpLink = genericController.encodeText(.fields("HelpLink").defaultValue)
                        instance.IconFilename = genericController.encodeText(.fields("IconFilename").defaultValue)
                        instance.IconHeight = genericController.EncodeInteger(.fields("IconHeight").defaultValue)
                        instance.IconSprites = genericController.EncodeInteger(.fields("IconSprites").defaultValue)
                        instance.IconWidth = genericController.EncodeInteger(.fields("IconWidth").defaultValue)
                        instance.InFrame = genericController.EncodeBoolean(.fields("InFrame").defaultValue)
                        instance.inlineScript = genericController.encodeText(.fields("inlineScript").defaultValue)
                        instance.IsInline = genericController.EncodeBoolean(.fields("IsInline").defaultValue)
                        instance.JavaScriptBodyEnd = genericController.encodeText(.fields("JavaScriptBodyEnd").defaultValue)
                        instance.JavaScriptOnLoad = genericController.encodeText(.fields("JavaScriptOnLoad").defaultValue)
                        instance.JSFilename = genericController.encodeText(.fields("JSFilename").defaultValue)
                        instance.Link = genericController.encodeText(.fields("Link").defaultValue)
                        instance.MetaDescription = genericController.encodeText(.fields("MetaDescription").defaultValue)
                        instance.MetaKeywordList = genericController.encodeText(.fields("MetaKeywordList").defaultValue)
                        instance.ModifiedBy = genericController.EncodeInteger(.fields("ModifiedBy").defaultValue)
                        instance.ModifiedDate = genericController.EncodeDate(.fields("ModifiedDate").defaultValue)
                        instance.Name = genericController.encodeText(.fields("Name").defaultValue)
                        instance.NavTypeID = genericController.EncodeInteger(.fields("NavTypeID").defaultValue)
                        instance.ObjectProgramID = genericController.encodeText(.fields("ObjectProgramID").defaultValue)
                        instance.OnBodyEnd = genericController.EncodeBoolean(.fields("OnBodyEnd").defaultValue)
                        instance.OnBodyStart = genericController.EncodeBoolean(.fields("OnBodyStart").defaultValue)
                        instance.OnNewVisitEvent = genericController.EncodeBoolean(.fields("OnNewVisitEvent").defaultValue)
                        instance.OnPageEndEvent = genericController.EncodeBoolean(.fields("OnPageEndEvent").defaultValue)
                        instance.OnPageStartEvent = genericController.EncodeBoolean(.fields("OnPageStartEvent").defaultValue)
                        instance.OtherHeadTags = genericController.encodeText(.fields("OtherHeadTags").defaultValue)
                        instance.PageTitle = genericController.encodeText(.fields("PageTitle").defaultValue)
                        instance.ProcessInterval = genericController.EncodeInteger(.fields("ProcessInterval").defaultValue)
                        instance.ProcessNextRun = genericController.EncodeDate(.fields("ProcessNextRun").defaultValue)
                        instance.ProcessRunOnce = genericController.EncodeBoolean(.fields("ProcessRunOnce").defaultValue)
                        instance.ProcessServerKey = genericController.encodeText(.fields("ProcessServerKey").defaultValue)
                        instance.RemoteAssetLink = genericController.encodeText(.fields("RemoteAssetLink").defaultValue)
                        instance.RemoteMethod = genericController.EncodeBoolean(.fields("RemoteMethod").defaultValue)
                        instance.RobotsTxt = genericController.encodeText(.fields("RobotsTxt").defaultValue)
                        instance.ScriptingCode = genericController.encodeText(.fields("ScriptingCode").defaultValue)
                        instance.ScriptingEntryPoint = genericController.encodeText(.fields("ScriptingEntryPoint").defaultValue)
                        instance.ScriptingLanguageID = genericController.EncodeInteger(.fields("ScriptingLanguageID").defaultValue)
                        instance.ScriptingTimeout = genericController.encodeText(.fields("ScriptingTimeout").defaultValue)
                        instance.SortOrder = genericController.encodeText(.fields("SortOrder").defaultValue)
                        instance.StylesFilename = genericController.encodeText(.fields("StylesFilename").defaultValue)
                        instance.Template = genericController.EncodeBoolean(.fields("Template").defaultValue)
                    End With
                End If
            Catch ex As Exception
                cpcore.handleExceptionAndContinue(ex)
            End Try
            Return instance
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' pattern get a list of objects from this model
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="someCriteria"></param>
        ''' <returns></returns>
        Public Shared Function createList_OnNewVisitEvent(cpCore As coreClass, callersCacheNameList As List(Of String)) As List(Of addonModel)
            Dim result As New List(Of addonModel)
            Try
                Dim cs As New csController(cpCore)
                Dim ignoreCacheNames As New List(Of String)
                If (cs.open(primaryContentName, "(OnNewVisitEvent<>0)")) Then
                    Dim instance As addonModel
                    Do
                        instance = addonModel.loadRecord(cpCore, cs, callersCacheNameList)
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
