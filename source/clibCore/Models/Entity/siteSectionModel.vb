
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class siteSectionModel
        '
        '-- const
        Public Const primaryContentName As String = "site sections"
        Private Const primaryContentTableName As String = "ccsitesections"
        Private Const primaryContentDataSource As String = "default" '<----- set to datasource if not default
        '
        ' -- instance properties
        Public ID As Integer
        Public Active As Boolean
        Public BlockSection As Boolean
        Public Caption As String
        Public ccGuid As String
        Public ContentCategoryID As Integer
        Public ContentControlID As Integer
        Public ContentID As Integer
        Public CreatedBy As Integer
        Public CreateKey As Integer
        Public DateAdded As Date
        Public EditArchive As Boolean
        Public EditBlank As Boolean
        Public EditSourceID As Integer
        Public HideMenu As Boolean
        Public JSEndBody As String
        Public JSFilename As String
        Public JSHead As String
        Public JSOnLoad As String
        'Public MenuImageDownFilename As String
        'Public menuImageDownOverFilename As String
        'Public MenuImageFilename As String
        'Public MenuImageOverFilename As String
        Public ModifiedBy As Integer
        Public ModifiedDate As Date
        Public Name As String
        Public RootPageID As Integer
        Public SortOrder As String
        Public TemplateID As Integer
        '
        ' -- publics not exposed to the UI (test/internal data)
        '<JsonIgnore> Public createKey As Integer
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
        ''' return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId">The id of the record to be read into the new object</param>
        ''' <param name="cacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
        Public Shared Function create(cpCore As coreClass, recordId As Integer, ByRef cacheNameList As List(Of String)) As siteSectionModel
            Dim result As siteSectionModel = Nothing
            Try
                If recordId > 0 Then
                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", recordId.ToString())
                    result = cpCore.cache.getObject(Of siteSectionModel)(cacheName)
                    If (result Is Nothing) Then
                        result = loadObject(cpCore, "id=" & recordId.ToString(), cacheNameList)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Shared Function create(cpCore As coreClass, recordGuid As String, ByRef cacheNameList As List(Of String)) As siteSectionModel
            Dim result As siteSectionModel = Nothing
            Try
                If Not String.IsNullOrEmpty(recordGuid) Then
                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "ccguid", recordGuid)
                    result = cpCore.cache.getObject(Of siteSectionModel)(cacheName)
                    If (result Is Nothing) Then
                        result = loadObject(cpCore, "ccGuid=" & cpCore.db.encodeSQLText(recordGuid), cacheNameList)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Private Shared Function loadObject(cpCore As coreClass, sqlCriteria As String, ByRef cacheNameList As List(Of String)) As siteSectionModel
            Dim result As siteSectionModel = Nothing
            Try
                Dim cs As New csController(cpCore)
                If cs.open(primaryContentName, sqlCriteria) Then
                    result = New siteSectionModel
                    With result
                        '
                        ' -- populate result model
                        .ID = cs.getInteger("ID")
                        .Active = cs.getBoolean("Active")
                        .BlockSection = cs.getBoolean("BlockSection")
                        .Caption = cs.getText("Caption")
                        .ccGuid = cs.getText("ccGuid")
                        .ContentCategoryID = cs.getInteger("ContentCategoryID")
                        .ContentControlID = cs.getInteger("ContentControlID")
                        .ContentID = cs.getInteger("ContentID")
                        .CreatedBy = cs.getInteger("CreatedBy")
                        .CreateKey = cs.getInteger("CreateKey")
                        .DateAdded = cs.getDate("DateAdded")
                        .EditArchive = cs.getBoolean("EditArchive")
                        .EditBlank = cs.getBoolean("EditBlank")
                        .EditSourceID = cs.getInteger("EditSourceID")
                        .HideMenu = cs.getBoolean("HideMenu")
                        .JSEndBody = cs.getText("JSEndBody")
                        .JSFilename = cs.getText("JSFilename")
                        .JSHead = cs.getText("JSHead")
                        .JSOnLoad = cs.getText("JSOnLoad")
                        '.MenuImageDownFilename = cs.getText("MenuImageDownFilename")
                        '.menuImageDownOverFilename = cs.getText("menuImageDownOverFilename")
                        '.MenuImageFilename = cs.getText("MenuImageFilename")
                        '.MenuImageOverFilename = cs.getText("MenuImageOverFilename")
                        .ModifiedBy = cs.getInteger("ModifiedBy")
                        .ModifiedDate = cs.getDate("ModifiedDate")
                        .Name = cs.getText("Name")
                        .RootPageID = cs.getInteger("RootPageID")
                        .SortOrder = cs.getText("SortOrder")
                        .TemplateID = cs.getInteger("TemplateID")
                        If (String.IsNullOrEmpty(.ccGuid)) Then .ccGuid = Controllers.genericController.getGUID()
                    End With
                    If (result IsNot Nothing) Then
                        '
                        ' -- set primary and secondary caches
                        ' -- add all cachenames to the injected cachenamelist
                        Dim cacheName0 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", result.ID.ToString())
                        cacheNameList.Add(cacheName0)
                        cpCore.cache.setObject(cacheName0, result)
                        '
                        Dim cacheName1 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "ccguid", result.ccGuid)
                        cacheNameList.Add(cacheName1)
                        cpCore.cache.setSecondaryObject(cacheName1, cacheName0)
                    End If
                End If
                Call cs.Close()
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                    If (String.IsNullOrEmpty(ccGuid)) Then
                        ccGuid = Controllers.genericController.getGUID()
                    End If
                    cs.setField("Active", Active.ToString())
                    cs.SetField("BlockSection", BlockSection.ToString())
                    cs.SetField("Caption", Caption)
                    cs.SetField("ccGuid", ccGuid)
                    cs.SetField("ContentCategoryID", ContentCategoryID.ToString())
                    cs.SetField("ContentControlID", ContentControlID.ToString())
                    cs.SetField("ContentID", ContentID.ToString())
                    cs.SetField("CreatedBy", CreatedBy.ToString())
                    cs.setField("CreateKey", CreateKey.ToString())
                    cs.SetField("DateAdded", DateAdded.ToString())
                    cs.SetField("EditArchive", EditArchive.ToString())
                    cs.SetField("EditBlank", EditBlank.ToString())
                    cs.SetField("EditSourceID", EditSourceID.ToString())
                    cs.SetField("HideMenu", HideMenu.ToString())
                    cs.SetField("JSEndBody", JSEndBody)
                    cs.SetField("JSFilename", JSFilename)
                    cs.SetField("JSHead", JSHead)
                    cs.SetField("JSOnLoad", JSOnLoad)
                    'cs.SetField("MenuImageDownFilename", MenuImageDownFilename)
                    'cs.SetField("menuImageDownOverFilename", menuImageDownOverFilename)
                    'cs.SetField("MenuImageFilename", MenuImageFilename)
                    'cs.SetField("MenuImageOverFilename", MenuImageOverFilename)
                    cs.SetField("ModifiedBy", ModifiedBy.ToString())
                    cs.SetField("ModifiedDate", ModifiedDate.ToString())
                    cs.SetField("Name", Name)
                    cs.SetField("RootPageID", RootPageID.ToString())
                    cs.SetField("SortOrder", SortOrder)
                    cs.SetField("TemplateID", TemplateID.ToString())
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Shared Function getObjectList(cpCore As coreClass, someCriteria As Integer) As List(Of siteSectionModel)
            Dim result As New List(Of siteSectionModel)
            Try
                Dim cs As New csController(cpCore)
                Dim ignoreCacheNames As New List(Of String)
                If (cs.open(primaryContentName, "(someCriteria=" & someCriteria & ")", "name", True, "id")) Then
                    Dim instance As siteSectionModel
                    Do
                        instance = siteSectionModel.create(cpCore, cs.getInteger("id"), ignoreCacheNames)
                        If (instance IsNot Nothing) Then
                            result.Add(instance)
                        End If
                        cs.goNext()
                    Loop While cs.ok()
                End If
                cs.Close()
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get a dictionary of sectionId by rootPageId
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="someCriteria"></param>
        ''' <returns></returns>
        Public Shared Function getRootPageIdIndex(cpCore As coreClass) As Dictionary(Of Integer, Integer)
            Dim result As New Dictionary(Of Integer, Integer)
            Try
                Dim cs As New csController(cpCore)
                Dim ignoreCacheNames As New List(Of String)
                If (cs.open(primaryContentName, "", "id", True, "rootpageid,id")) Then
                    Do
                        result.Add(cs.getInteger("rootpageid"), cs.getInteger("id"))
                        cs.goNext()
                    Loop While cs.ok()
                End If
                cs.Close()
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
            cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", recordId.ToString))
            '
            ' -- always clear the cache with the content name
            '?? cpCore.cache.invalidateObject(primaryContentName)
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' invalidate a secondary key (ccGuid field).
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="guid"></param>
        Public Shared Sub invalidateGuidCache(cpCore As coreClass, guid As String)
            cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "ccguid", guid))
            '
            ' -- always clear the cache with the content name
            '?? cpCore.cache.invalidateObject(primaryContentName)
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
        '    Return (primaryContentTableName & "." & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
        'End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' add a new recod to the db and open it. Starting a new model with this method will use the default
        ''' values in Contensive metadata (active, contentcontrolid, etc)
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="cacheNameList"></param>
        ''' <returns></returns>
        Public Shared Function add(cpCore As coreClass, ByRef cacheNameList As List(Of String)) As siteSectionModel
            Dim result As siteSectionModel = Nothing
            Try
                result = create(cpCore, cpCore.db.metaData_InsertContentRecordGetID(primaryContentName, 0), cacheNameList)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
                Throw
            End Try
            Return result
        End Function
    End Class
End Namespace
