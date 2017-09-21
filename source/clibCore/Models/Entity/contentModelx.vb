
'Option Explicit On
'Option Strict On

'Imports System
'Imports System.Collections.Generic
'Imports System.Text
'Imports Contensive.BaseClasses
'Imports Contensive.Core.Controllers
'Imports Newtonsoft.Json

'Namespace Contensive.Core.Models.Entity
'    Public Class contentModel
'        '
'        '-- const
'        Public Const primaryContentName As String = "content" '<------ set content name
'        Private Const primaryContentTableName As String = "cccontent" '<------ set to tablename for the primary content (used for cache names)
'        Private Const primaryContentDataSource As String = "default" '<----- set to datasource if not default
'        '
'        ' -- instance properties
'        Public Property AdminOnly As Boolean
'        Public Property AllowAdd As Boolean
'        Public Property AllowContentChildTool As Boolean
'        Public Property AllowContentTracking As Boolean
'        Public Property AllowDelete As Boolean
'        Public Property AllowTopicRules As Boolean
'        Public Property AllowWorkflowAuthoring As Boolean
'        Public Property AuthoringTableID As Integer
'        Public Property ContentTableID As Integer
'        Public Property DefaultSortMethodID As Integer
'        Public Property DeveloperOnly As Boolean
'        Public Property DropDownFieldList As String
'        
'        
'        Public Property EditorGroupID As Integer
'        
'        Public Property IconHeight As Integer
'        Public Property IconLink As String
'        Public Property IconSprites As Integer
'        Public Property IconWidth As Integer
'        Public Property InstalledByCollectionID As Integer
'        Public Property IsBaseContent As Boolean
'        Public Property ParentID As Integer
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' Create an empty object. needed for deserialization
'        ''' </summary>
'        Public Sub New()
'            '
'        End Sub
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' add a new recod to the db and open it. Starting a new model with this method will use the default
'        ''' values in Contensive metadata (active, contentcontrolid, etc)
'        ''' </summary>
'        ''' <param name="cpCore"></param>
'        ''' <param name="callersCacheNameList"></param>
'        ''' <returns></returns>
'        Public Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As contentModel
'            Dim result As contentModel = Nothing
'            Try
'                result = create(cpCore, cpCore.db.metaData_InsertContentRecordGetID(primaryContentName, cpCore.authContext.user.id), callersCacheNameList)
'            Catch ex As Exception
'                cpCore.handleException(ex) : Throw
'                Throw
'            End Try
'            Return result
'        End Function
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="recordId">The id of the record to be read into the new object</param>
'        ''' <param name="callersCacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
'        Public Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As contentModel
'            Dim result As contentModel = Nothing
'            Try
'                If recordId > 0 Then
'                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", recordId.ToString())
'                    result = cpCore.cache.getObject(Of contentModel)(cacheName)
'                    If (result Is Nothing) Then
'                        Using cs As New csController(cpCore)
'                            If cs.open(primaryContentName, "(id=" & recordId.ToString() & ")") Then
'                                result = loadRecord(cpCore, cs, callersCacheNameList)
'                            End If
'                        End Using
'                    End If
'                End If
'            Catch ex As Exception
'                cpCore.handleException(ex) : Throw
'                Throw
'            End Try
'            Return result
'        End Function
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' open an existing object
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="recordGuid"></param>
'        Public Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As contentModel
'            Dim result As contentModel = Nothing
'            Try
'                If Not String.IsNullOrEmpty(recordGuid) Then
'                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "ccguid", recordGuid)
'                    result = cpCore.cache.getObject(Of contentModel)(cacheName)
'                    If (result Is Nothing) Then
'                        Using cs As New csController(cpCore)
'                            If cs.open(primaryContentName, "(ccGuid=" & cpCore.db.encodeSQLText(recordGuid) & ")") Then
'                                result = loadRecord(cpCore, cs, callersCacheNameList)
'                            End If
'                        End Using
'                    End If
'                End If
'            Catch ex As Exception
'                cpCore.handleException(ex) : Throw
'                Throw
'            End Try
'            Return result
'        End Function
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' template for open an existing object with multiple keys (like a rule)
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="foreignKey1Id"></param>
'        Public Shared Function create(cpCore As coreClass, foreignKey1Id As Integer, foreignKey2Id As Integer, ByRef callersCacheNameList As List(Of String)) As contentModel
'            Dim result As contentModel = Nothing
'            Try
'                If ((foreignKey1Id > 0) And (foreignKey2Id > 0)) Then
'                    result = cpCore.cache.getObject(Of contentModel)(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "foreignKey1", foreignKey1Id.ToString(), "foreignKey2", foreignKey2Id.ToString()))
'                    If (result Is Nothing) Then
'                        Using cs As New csController(cpCore)
'                            If cs.open(primaryContentName, "(foreignKey1=" & foreignKey1Id.ToString() & ")and(foreignKey1=" & foreignKey1Id.ToString() & ")") Then
'                                result = loadRecord(cpCore, cs, callersCacheNameList)
'                            End If
'                        End Using
'                    End If
'                End If
'            Catch ex As Exception
'                cpCore.handleException(ex) : Throw
'                Throw
'            End Try
'            Return result
'        End Function
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' open an existing object
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="sqlCriteria"></param>
'        Private Shared Function loadRecord(cpCore As coreClass, cs As csController, ByRef callersCacheNameList As List(Of String)) As contentModel
'            Dim result As contentModel = Nothing
'            Try
'                If cs.ok() Then
'                    result = New contentModel
'                    With result
'                        '
'                        ' -- populate result model
'                        .ID = cs.getInteger("ID")
'                        .Active = cs.getBoolean("Active")
'                        .AdminOnly = cs.getBoolean("AdminOnly")
'                        .AllowAdd = cs.getBoolean("AllowAdd")
'                        .AllowContentChildTool = cs.getBoolean("AllowContentChildTool")
'                        .AllowContentTracking = cs.getBoolean("AllowContentTracking")
'                        .AllowDelete = cs.getBoolean("AllowDelete")
'                        .AllowMetaContent = cs.getBoolean("AllowMetaContent")
'                        .AllowTopicRules = cs.getBoolean("AllowTopicRules")
'                        .AllowWorkflowAuthoring = cs.getBoolean("AllowWorkflowAuthoring")
'                        .AuthoringTableID = cs.getInteger("AuthoringTableID")
'                        .ccGuid = cs.getText("ccGuid")
'                        '
'                        .ContentControlID = cs.getInteger("ContentControlID")
'                        .ContentTableID = cs.getInteger("ContentTableID")
'                        .CreatedBy = cs.getInteger("CreatedBy")
'                        .CreateKey = cs.getInteger("CreateKey")
'                        .DateAdded = cs.getDate("DateAdded")
'                        .DefaultSortMethodID = cs.getInteger("DefaultSortMethodID")
'                        .DeveloperOnly = cs.getBoolean("DeveloperOnly")
'                        .DropDownFieldList = cs.getText("DropDownFieldList")
'                        '
'                        '
'                        .EditorGroupID = cs.getInteger("EditorGroupID")
'                        '
'                        .IconHeight = cs.getInteger("IconHeight")
'                        .IconLink = cs.getText("IconLink")
'                        .IconSprites = cs.getInteger("IconSprites")
'                        .IconWidth = cs.getInteger("IconWidth")
'                        .InstalledByCollectionID = cs.getInteger("InstalledByCollectionID")
'                        .IsBaseContent = cs.getBoolean("IsBaseContent")
'                        .ModifiedBy = cs.getInteger("ModifiedBy")
'                        .ModifiedDate = cs.getDate("ModifiedDate")
'                        .Name = cs.getText("Name")
'                        .ParentID = cs.getInteger("ParentID")
'                        .SortOrder = cs.getText("SortOrder")
'                    End With
'                    If (result IsNot Nothing) Then
'                        '
'                        ' -- set primary cache to the object created
'                        ' -- set secondary caches to the primary cache
'                        ' -- add all cachenames to the injected cachenamelist
'                        Dim cacheName0 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", result.id.ToString())
'                        callersCacheNameList.Add(cacheName0)
'                        cpCore.cache.setObject(cacheName0, result)
'                        '
'                        Dim cacheName1 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "ccguid", result.ccguid)
'                        callersCacheNameList.Add(cacheName1)
'                        cpCore.cache.setSecondaryObject(cacheName1, cacheName0)
'                    End If
'                End If
'            Catch ex As Exception
'                cpCore.handleException(ex) : Throw
'                Throw
'            End Try
'            Return result
'        End Function
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' save the instance properties to a record with matching id. If id is not provided, a new record is created.
'        ''' </summary>
'        ''' <param name="cpCore"></param>
'        ''' <returns></returns>
'        Public Function save(cpCore As coreClass) As Integer
'            Try
'                Dim cs As New csController(cpCore)
'                If (id > 0) Then
'                    If Not cs.open(primaryContentName, "id=" & id) Then
'                        Dim message As String = "Unable to open record in content [" & primaryContentName & "], with id [" & id & "]"
'                        cs.Close()
'                        id = 0
'                        Throw New ApplicationException(message)
'                    End If
'                Else
'                    If Not cs.Insert(primaryContentName) Then
'                        cs.Close()
'                        id = 0
'                        Throw New ApplicationException("Unable to insert record in content [" & primaryContentName & "]")
'                    End If
'                End If
'                If cs.ok() Then
'                    id = cs.getInteger("id")
'                    cs.setField("Active", Active.ToString())
'                    cs.setField("AdminOnly", AdminOnly.ToString())
'                    cs.setField("AllowAdd", AllowAdd.ToString())
'                    cs.setField("AllowContentChildTool", AllowContentChildTool.ToString())
'                    cs.setField("AllowContentTracking", AllowContentTracking.ToString())
'                    cs.setField("AllowDelete", AllowDelete.ToString())
'                    cs.setField("AllowMetaContent", AllowMetaContent.ToString())
'                    cs.setField("AllowTopicRules", AllowTopicRules.ToString())
'                    cs.setField("AllowWorkflowAuthoring", AllowWorkflowAuthoring.ToString())
'                    cs.setField("AuthoringTableID", AuthoringTableID.ToString())
'                    cs.setField("ccGuid", ccGuid)
'                    '
'                    cs.setField("ContentControlID", ContentControlID.ToString())
'                    cs.setField("ContentTableID", ContentTableID.ToString())
'                    cs.setField("CreatedBy", CreatedBy.ToString())
'                    cs.setField("CreateKey", CreateKey.ToString())
'                    cs.setField("DateAdded", DateAdded.ToString())
'                    cs.setField("DefaultSortMethodID", DefaultSortMethodID.ToString())
'                    cs.setField("DeveloperOnly", DeveloperOnly.ToString())
'                    cs.setField("DropDownFieldList", DropDownFieldList)
'                    '
'                    '
'                    cs.setField("EditorGroupID", EditorGroupID.ToString())
'                    '
'                    cs.setField("IconHeight", IconHeight.ToString())
'                    cs.setField("IconLink", IconLink)
'                    cs.setField("IconSprites", IconSprites.ToString())
'                    cs.setField("IconWidth", IconWidth.ToString())
'                    cs.setField("InstalledByCollectionID", InstalledByCollectionID.ToString())
'                    cs.setField("IsBaseContent", IsBaseContent.ToString())
'                    cs.setField("ModifiedBy", ModifiedBy.ToString())
'                    cs.setField("ModifiedDate", ModifiedDate.ToString())
'                    cs.setField("Name", Name)
'                    cs.setField("ParentID", ParentID.ToString())
'                    cs.setField("SortOrder", SortOrder)
'                    If (String.IsNullOrEmpty(ccGuid)) Then ccGuid = Controllers.genericController.getGUID()
'                End If
'                Call cs.Close()
'                '
'                ' -- invalidate objects
'                ' -- no, the primary is invalidated by the cs.save()
'                'cpCore.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"id", id.ToString))
'                ' -- no, the secondary points to the pirmary, which is invalidated. Dont waste resources invalidating
'                'cpCore.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"ccguid", ccguid))
'                '
'                ' -- object is here, but the cache was invalidated, setting
'                cpCore.cache.setObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", Me.id.ToString()), Me)
'            Catch ex As Exception
'                cpCore.handleException(ex) : Throw
'                Throw
'            End Try
'            Return id
'        End Function
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' delete an existing database record by id
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="recordId"></param>
'        Public Shared Sub delete(cpCore As coreClass, recordId As Integer)
'            Try
'                If (recordId > 0) Then
'                    cpCore.db.deleteContentRecords(primaryContentName, "id=" & recordId.ToString)
'                    cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", recordId.ToString))
'                End If
'            Catch ex As Exception
'                cpCore.handleException(ex) : Throw
'                Throw
'            End Try
'        End Sub
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' delete an existing database record by guid
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="ccguid"></param>
'        Public Shared Sub delete(cpCore As coreClass, ccguid As String)
'            Try
'                If (Not String.IsNullOrEmpty(ccguid)) Then
'                    Dim instance As contentModel = create(cpCore, ccguid, New List(Of String))
'                    If (instance IsNot Nothing) Then
'                        invalidatePrimaryCache(cpCore, instance.id)
'                        cpCore.db.deleteContentRecords(primaryContentName, "(ccguid=" & cpCore.db.encodeSQLText(ccguid) & ")")
'                    End If
'                End If
'            Catch ex As Exception
'                cpCore.handleException(ex) : Throw
'                Throw
'            End Try
'        End Sub
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' pattern to delete an existing object based on multiple criteria (like a rule record)
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="foreignKey1Id"></param>
'        ''' <param name="foreignKey2Id"></param>
'        Public Shared Sub delete(cpCore As coreClass, foreignKey1Id As Integer, foreignKey2Id As Integer)
'            Try
'                If (foreignKey2Id > 0) And (foreignKey1Id > 0) Then
'                    Dim instance As contentModel = create(cpCore, foreignKey1Id, foreignKey2Id, New List(Of String))
'                    If (instance IsNot Nothing) Then
'                        invalidatePrimaryCache(cpCore, instance.id)
'                        cpCore.db.deleteTableRecord(primaryContentTableName, instance.id, primaryContentDataSource)
'                    End If
'                End If
'            Catch ex As Exception
'                cpCore.handleException(ex) : Throw
'                Throw
'            End Try
'        End Sub
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' pattern get a list of objects from this model
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="someCriteria"></param>
'        ''' <returns></returns>
'        Public Shared Function createDict(cpCore As coreClass, callersCacheNameList As List(Of String)) As Dictionary(Of Integer, contentModel)
'            Dim result As New Dictionary(Of Integer, contentModel)
'            Try
'                Dim cs As New csController(cpCore)
'                Dim ignoreCacheNames As New List(Of String)
'                '
'                ' -- use sql because this content is needed to create contentcontrolcriteria, so it recurses to neverland
'                If (cs.openSQL("select * from " & primaryContentTableName & " where (active>0)")) Then
'                    Dim instance As contentModel
'                    Do
'                        instance = contentModel.loadRecord(cpCore, cs, callersCacheNameList)
'                        If (instance IsNot Nothing) Then
'                            result.Add(instance.ID, instance)
'                        End If
'                        cs.goNext()
'                    Loop While cs.ok()
'                End If
'                cs.Close()
'            Catch ex As Exception
'                cpCore.handleException(ex) : Throw
'            End Try
'            Return result
'        End Function
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' invalidate the primary key (which depends on all secondary keys)
'        ''' </summary>
'        ''' <param name="cpCore"></param>
'        ''' <param name="recordId"></param>
'        Public Shared Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
'            cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", recordId.ToString))
'            '
'            ' -- the zero record cache means any record was updated. Can be used to invalidate arbitraty lists of records in the table
'            cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", "0"))
'        End Sub
'        ''
'        ''====================================================================================================
'        '''' <summary>
'        '''' produce a standard format cachename for this model
'        '''' </summary>
'        '''' <param name="fieldName"></param>
'        '''' <param name="fieldValue"></param>
'        '''' <returns></returns>
'        'Private Shared Function Controllers.cacheController.getModelCacheName( primaryContentTableName,fieldName As String, fieldValue As String) As String
'        '    Return (primaryContentTableName & "-" & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
'        'End Function
'        ''
'        'Private Shared Function Controllers.cacheController.getModelCacheName( primaryContentTableName,field1Name As String, field1Value As String, field2Name As String, field2Value As String) As String
'        '    Return (primaryContentTableName & "-" & field1Name & "." & field1Value & "-" & field2Name & "." & field2Value).ToLower().Replace(" ", "_")
'        'End Function
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' get the name of the record by it's id
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="recordId"></param>record
'        ''' <returns></returns>
'        Public Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
'            Return contentModel.create(cpcore, recordId, New List(Of String)).name
'        End Function
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' get the name of the record by it's guid 
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="ccGuid"></param>record
'        ''' <returns></returns>
'        Public Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
'            Return contentModel.create(cpcore, ccGuid, New List(Of String)).name
'        End Function
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' get the id of the record by it's guid 
'        ''' </summary>
'        ''' <param name="cp"></param>
'        ''' <param name="ccGuid"></param>record
'        ''' <returns></returns>
'        Public Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
'            Return contentModel.create(cpcore, ccGuid, New List(Of String)).id
'        End Function
'        '
'        '====================================================================================================
'        '
'        Public Shared Function createDefault(cpcore As coreClass) As contentModel
'            Dim instance As New contentModel
'            Try
'                Dim CDef As cdefModel = cpcore.metaData.getCdef(primaryContentName)
'                If (CDef Is Nothing) Then
'                    Throw New ApplicationException("content [" & primaryContentName & "] could Not be found.")
'                ElseIf (CDef.Id <= 0) Then
'                    Throw New ApplicationException("content [" & primaryContentName & "] could Not be found.")
'                Else
'                    With CDef
'                        instance.Active = genericController.EncodeBoolean(.fields("Active").defaultValue)
'                        instance.AdminOnly = genericController.EncodeBoolean(.fields("AdminOnly").defaultValue)
'                        instance.AllowAdd = genericController.EncodeBoolean(.fields("AllowAdd").defaultValue)
'                        instance.AllowContentChildTool = genericController.EncodeBoolean(.fields("AllowContentChildTool").defaultValue)
'                        instance.AllowContentTracking = genericController.EncodeBoolean(.fields("AllowContentTracking").defaultValue)
'                        instance.AllowDelete = genericController.EncodeBoolean(.fields("AllowDelete").defaultValue)
'                        instance.AllowMetaContent = genericController.EncodeBoolean(.fields("AllowMetaContent").defaultValue)
'                        instance.AllowTopicRules = genericController.EncodeBoolean(.fields("AllowTopicRules").defaultValue)
'                        instance.AllowWorkflowAuthoring = genericController.EncodeBoolean(.fields("AllowWorkflowAuthoring").defaultValue)
'                        instance.AuthoringTableID = genericController.EncodeInteger(.fields("AuthoringTableID").defaultValue)
'                        instance.ccGuid = genericController.encodeText(.fields("ccGuid").defaultValue)

'                        instance.ContentControlID = CDef.Id
'                        instance.ContentTableID = genericController.EncodeInteger(.fields("ContentTableID").defaultValue)
'                        instance.CreatedBy = genericController.EncodeInteger(.fields("CreatedBy").defaultValue)
'                        instance.CreateKey = genericController.EncodeInteger(.fields("CreateKey").defaultValue)
'                        instance.DateAdded = genericController.EncodeDate(.fields("DateAdded").defaultValue)
'                        instance.DefaultSortMethodID = genericController.EncodeInteger(.fields("DefaultSortMethodID").defaultValue)
'                        instance.DeveloperOnly = genericController.EncodeBoolean(.fields("DeveloperOnly").defaultValue)
'                        instance.DropDownFieldList = genericController.encodeText(.fields("DropDownFieldList").defaultValue)


'                        instance.EditorGroupID = genericController.EncodeInteger(.fields("EditorGroupID").defaultValue)

'                        instance.IconHeight = genericController.EncodeInteger(.fields("IconHeight").defaultValue)
'                        instance.IconLink = genericController.encodeText(.fields("IconLink").defaultValue)
'                        instance.IconSprites = genericController.EncodeInteger(.fields("IconSprites").defaultValue)
'                        instance.IconWidth = genericController.EncodeInteger(.fields("IconWidth").defaultValue)
'                        instance.InstalledByCollectionID = genericController.EncodeInteger(.fields("InstalledByCollectionID").defaultValue)
'                        instance.IsBaseContent = genericController.EncodeBoolean(.fields("IsBaseContent").defaultValue)
'                        instance.ModifiedBy = genericController.EncodeInteger(.fields("ModifiedBy").defaultValue)
'                        instance.ModifiedDate = genericController.EncodeDate(.fields("ModifiedDate").defaultValue)
'                        instance.Name = genericController.encodeText(.fields("Name").defaultValue)
'                        instance.ParentID = genericController.EncodeInteger(.fields("ParentID").defaultValue)
'                        instance.SortOrder = genericController.encodeText(.fields("SortOrder").defaultValue)
'                    End With
'                End If
'            Catch ex As Exception
'                cpcore.handleException(ex)
'            End Try
'            Return instance
'        End Function
'    End Class
'End Namespace
