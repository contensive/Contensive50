
'Option Explicit On
'Option Strict On

'Imports System
'Imports System.Collections.Generic
'Imports System.Text
'Imports Contensive.BaseClasses
'Imports Contensive.Core.Controllers
'Imports Newtonsoft.Json

'Namespace Contensive.Core.Models.Entity
'    Public Class pageContentModel
'        '
'        '-- const
'        Public Const contentName As String = "page content"
'        Private Const tableName As String = "ccpagecontent"
'        Private Const dataSource As String = "default"
'        '
'        ' -- instance properties
'        Public ID As Integer
'        Public Active As Boolean
'        Public AllowBrief As Boolean
'        Public AllowChildListDisplay As Boolean
'        Public AllowEmailPage As Boolean
'        Public AllowFeedback As Boolean
'        Public AllowHitNotification As Boolean
'        Public AllowInChildLists As Boolean
'        Public AllowInMenus As Boolean
'        Public AllowLastModifiedFooter As Boolean
'        Public AllowMessageFooter As Boolean
'        Public AllowMetaContentNoFollow As Boolean
'        Public AllowMoreInfo As Boolean
'        Public AllowPrinterVersion As Boolean
'        Public AllowReturnLinkDisplay As Boolean
'        Public AllowReviewedFooter As Boolean
'        Public AllowSeeAlso As Boolean
'        Public ArchiveParentID As Integer
'        Public BlockContent As Boolean
'        Public BlockPage As Boolean
'        Public BlockSourceID As Integer
'        Public BriefFilename As String
'        Public ccGuid As String
'        Public ChildListInstanceOptions As String
'        Public ChildListSortMethodID As Integer
'        Public ChildPagesFound As Boolean
'        Public Clicks As Integer
'        Public ContactMemberID As Integer
'        Public ContentCategoryID As Integer
'        Public ContentControlID As Integer
'        Public ContentPadding As Integer
'        Public Copyfilename As String
'        Public CreatedBy As Integer
'        Public CreateKey As Integer
'        Public CustomBlockMessage As String
'        Public DateAdded As Date
'        Public DateArchive As Date
'        Public DateExpires As Date
'        Public DateReviewed As Date
'        Public EditArchive As Boolean
'        Public EditBlank As Boolean
'        Public EditSourceID As Integer
'        Public Headline As String
'        'Public ImageFilename As String
'        Public IsSecure As Boolean
'        Public JSEndBody As String
'        Public JSFilename As String
'        Public JSHead As String
'        Public JSOnLoad As String
'        Public LinkAlias As String
'        'Public Marquee As String
'        Public MenuHeadline As String
'        'Public MenuImageFileName As String
'        Public ModifiedBy As Integer
'        Public ModifiedDate As Date
'        Public Name As String
'        'Public OrganizationID As Integer
'        Public PageLink As String
'        Public ParentID As Integer
'        Public ParentListName As String
'        'Public PodcastMediaLink As String
'        ' Public PodcastSize As Integer
'        Public PubDate As Date
'        Public RegistrationGroupID As Integer
'        'Public RegistrationRequired As Boolean
'        Public ReviewedBy As Integer
'        'Public RSSDateExpire As Date
'        'Public RSSDatePublish As Date
'        'Public RSSDescription As String
'        'Public RSSLink As String
'        'Public RSSTitle As String
'        Public SortOrder As String
'        Public TemplateID As Integer
'        Public TriggerAddGroupID As Integer
'        Public TriggerConditionGroupID As Integer
'        Public TriggerConditionID As Integer
'        Public TriggerRemoveGroupID As Integer
'        Public TriggerSendSystemEmailID As Integer
'        Public Viewings As Integer
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
'        Public Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As pageContentModel
'            Dim result As pageContentModel = Nothing
'            Try
'                result = create(cpCore, cpCore.db.metaData_InsertContentRecordGetID(contentName, cpCore.authContext.user.id), callersCacheNameList)
'            Catch ex As Exception
'                cpCore.handleExceptionAndContinue(ex) : Throw
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
'        Public Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As pageContentModel
'            Dim result As pageContentModel = Nothing
'            Try
'                If recordId > 0 Then
'                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(tableName, "id", recordId.ToString())
'                    result = cpCore.cache.getObject(Of pageContentModel)(cacheName)
'                    If (result Is Nothing) Then
'                        Using cs As New csController(cpCore)
'                            If cs.open(contentName, "(id=" & recordId.ToString() & ")") Then
'                                result = loadRecord(cpCore, cs, callersCacheNameList)
'                            End If
'                        End Using
'                    End If
'                End If
'            Catch ex As Exception
'                cpCore.handleExceptionAndContinue(ex) : Throw
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
'        Public Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As pageContentModel
'            Dim result As pageContentModel = Nothing
'            Try
'                If Not String.IsNullOrEmpty(recordGuid) Then
'                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(tableName, "ccguid", recordGuid)
'                    result = cpCore.cache.getObject(Of pageContentModel)(cacheName)
'                    If (result Is Nothing) Then
'                        Using cs As New csController(cpCore)
'                            If cs.open(contentName, "(ccGuid=" & cpCore.db.encodeSQLText(recordGuid) & ")") Then
'                                result = loadRecord(cpCore, cs, callersCacheNameList)
'                            End If
'                        End Using
'                    End If
'                End If
'            Catch ex As Exception
'                cpCore.handleExceptionAndContinue(ex) : Throw
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
'        Public Shared Function create(cpCore As coreClass, foreignKey1Id As Integer, foreignKey2Id As Integer, ByRef callersCacheNameList As List(Of String)) As pageContentModel
'            Dim result As pageContentModel = Nothing
'            Try
'                If ((foreignKey1Id > 0) And (foreignKey2Id > 0)) Then
'                    result = cpCore.cache.getObject(Of pageContentModel)(Controllers.cacheController.getDbRecordCacheName(tableName, "foreignKey1", foreignKey1Id.ToString(), "foreignKey2", foreignKey2Id.ToString()))
'                    If (result Is Nothing) Then
'                        Using cs As New csController(cpCore)
'                            If cs.open(contentName, "(foreignKey1=" & foreignKey1Id.ToString() & ")and(foreignKey1=" & foreignKey1Id.ToString() & ")") Then
'                                result = loadRecord(cpCore, cs, callersCacheNameList)
'                            End If
'                        End Using
'                    End If
'                End If
'            Catch ex As Exception
'                cpCore.handleExceptionAndContinue(ex) : Throw
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
'        ''' <param name="recordName"></param>
'        Public Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As pageContentModel
'            Dim result As pageContentModel = Nothing
'            Try
'                If Not String.IsNullOrEmpty(recordName) Then
'                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(tableName, "name", recordName)
'                    result = cpCore.cache.getObject(Of pageContentModel)(cacheName)
'                    If (result Is Nothing) Then
'                        Using cs As New csController(cpCore)
'                            If cs.open(contentName, "(name=" & cpCore.db.encodeSQLText(recordName) & ")", "id") Then
'                                result = loadRecord(cpCore, cs, callersCacheNameList)
'                            End If
'                        End Using
'                    End If
'                End If
'            Catch ex As Exception
'                cpCore.handleExceptionAndContinue(ex) : Throw
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
'        Private Shared Function loadRecord(cpCore As coreClass, cs As csController, ByRef callersCacheNameList As List(Of String)) As pageContentModel
'            Dim result As pageContentModel = Nothing
'            Try
'                If cs.ok() Then
'                    result = New pageContentModel
'                    With result
'                        '
'                        ' -- populate result model
'                        .ID = cs.getInteger("ID")
'                        .Active = cs.getBoolean("Active")
'                        .AllowBrief = cs.getBoolean("AllowBrief")
'                        .AllowChildListDisplay = cs.getBoolean("AllowChildListDisplay")
'                        .AllowEmailPage = cs.getBoolean("AllowEmailPage")
'                        .AllowFeedback = cs.getBoolean("AllowFeedback")
'                        .AllowHitNotification = cs.getBoolean("AllowHitNotification")
'                        .AllowInChildLists = cs.getBoolean("AllowInChildLists")
'                        .AllowInMenus = cs.getBoolean("AllowInMenus")
'                        .AllowLastModifiedFooter = cs.getBoolean("AllowLastModifiedFooter")
'                        .AllowMessageFooter = cs.getBoolean("AllowMessageFooter")
'                        .AllowMetaContentNoFollow = cs.getBoolean("AllowMetaContentNoFollow")
'                        .AllowMoreInfo = cs.getBoolean("AllowMoreInfo")
'                        .AllowPrinterVersion = cs.getBoolean("AllowPrinterVersion")
'                        .AllowReturnLinkDisplay = cs.getBoolean("AllowReturnLinkDisplay")
'                        .AllowReviewedFooter = cs.getBoolean("AllowReviewedFooter")
'                        .AllowSeeAlso = cs.getBoolean("AllowSeeAlso")
'                        .ArchiveParentID = cs.getInteger("ArchiveParentID")
'                        .BlockContent = cs.getBoolean("BlockContent")
'                        .BlockPage = cs.getBoolean("BlockPage")
'                        .BlockSourceID = cs.getInteger("BlockSourceID")
'                        .BriefFilename = cs.getText("BriefFilename")
'                        .ccGuid = cs.getText("ccGuid")
'                        .ChildListInstanceOptions = cs.getText("ChildListInstanceOptions")
'                        .ChildListSortMethodID = cs.getInteger("ChildListSortMethodID")
'                        .ChildPagesFound = cs.getBoolean("ChildPagesFound")
'                        .Clicks = cs.getInteger("Clicks")
'                        .ContactMemberID = cs.getInteger("ContactMemberID")
'                        .ContentCategoryID = cs.getInteger("ContentCategoryID")
'                        .ContentControlID = cs.getInteger("ContentControlID")
'                        .ContentPadding = cs.getInteger("ContentPadding")
'                        .Copyfilename = cs.getText("Copyfilename")
'                        .CreatedBy = cs.getInteger("CreatedBy")
'                        .CreateKey = cs.getInteger("CreateKey")
'                        .CustomBlockMessage = cs.getText("CustomBlockMessage")
'                        .DateAdded = cs.getDate("DateAdded")
'                        .DateArchive = cs.getDate("DateArchive")
'                        .DateExpires = cs.getDate("DateExpires")
'                        .DateReviewed = cs.getDate("DateReviewed")
'                        .EditArchive = cs.getBoolean("EditArchive")
'                        .EditBlank = cs.getBoolean("EditBlank")
'                        .EditSourceID = cs.getInteger("EditSourceID")
'                        .Headline = cs.getText("Headline")
'                        '.ImageFilename = cs.getText("ImageFilename")
'                        .IsSecure = cs.getBoolean("IsSecure")
'                        .JSEndBody = cs.getText("JSEndBody")
'                        .JSFilename = cs.getText("JSFilename")
'                        .JSHead = cs.getText("JSHead")
'                        .JSOnLoad = cs.getText("JSOnLoad")
'                        .LinkAlias = cs.getText("LinkAlias")
'                        '.Marquee = cs.getText("Marquee")
'                        .MenuHeadline = cs.getText("MenuHeadline")
'                        '.MenuImageFileName = cs.getText("MenuImageFileName")
'                        .ModifiedBy = cs.getInteger("ModifiedBy")
'                        .ModifiedDate = cs.getDate("ModifiedDate")
'                        .Name = cs.getText("Name")
'                        '.OrganizationID = cs.getInteger("OrganizationID")
'                        .PageLink = cs.getText("PageLink")
'                        .ParentID = cs.getInteger("ParentID")
'                        .ParentListName = cs.getText("ParentListName")
'                        '.PodcastMediaLink = cs.getText("PodcastMediaLink")
'                        '.PodcastSize = cs.getInteger("PodcastSize")
'                        .PubDate = cs.getDate("PubDate")
'                        .RegistrationGroupID = cs.getInteger("RegistrationGroupID")
'                        '.RegistrationRequired = cs.getBoolean("RegistrationRequired")
'                        .ReviewedBy = cs.getInteger("ReviewedBy")
'                        '.RSSDateExpire = cs.getDate("RSSDateExpire")
'                        '.RSSDatePublish = cs.getDate("RSSDatePublish")
'                        '.RSSDescription = cs.getText("RSSDescription")
'                        '.RSSLink = cs.getText("RSSLink")
'                        '.RSSTitle = cs.getText("RSSTitle")
'                        .SortOrder = cs.getText("SortOrder")
'                        .TemplateID = cs.getInteger("TemplateID")
'                        .TriggerAddGroupID = cs.getInteger("TriggerAddGroupID")
'                        .TriggerConditionGroupID = cs.getInteger("TriggerConditionGroupID")
'                        .TriggerConditionID = cs.getInteger("TriggerConditionID")
'                        .TriggerRemoveGroupID = cs.getInteger("TriggerRemoveGroupID")
'                        .TriggerSendSystemEmailID = cs.getInteger("TriggerSendSystemEmailID")
'                        .Viewings = cs.getInteger("Viewings")
'                    End With
'                    If (result IsNot Nothing) Then
'                        '
'                        ' -- set primary cache to the object created
'                        ' -- set secondary caches to the primary cache
'                        ' -- add all cachenames to the injected cachenamelist
'                        Dim cacheName0 As String = Controllers.cacheController.getDbRecordCacheName(tableName, "id", result.ID.ToString())
'                        callersCacheNameList.Add(cacheName0)
'                        cpCore.cache.setObject(cacheName0, result)
'                        '
'                        Dim cacheName1 As String = Controllers.cacheController.getDbRecordCacheName(tableName, "ccguid", result.ccGuid)
'                        callersCacheNameList.Add(cacheName1)
'                        cpCore.cache.setSecondaryObject(cacheName1, cacheName0)
'                        '
'                        'Dim cacheName2 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "foreignKey1", result.foreignKey1Id.ToString(), "foreignKey2", result.foreignKey2Id.ToString())
'                        'callersCacheNameList.Add(cacheName2)
'                        'cpCore.cache.setSecondaryObject(cacheName2, cacheName0)
'                    End If
'                End If
'            Catch ex As Exception
'                cpCore.handleExceptionAndContinue(ex) : Throw
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
'                If (ID > 0) Then
'                    If Not cs.open(contentName, "id=" & ID) Then
'                        Dim message As String = "Unable to open record in content [" & contentName & "], with id [" & ID & "]"
'                        cs.Close()
'                        ID = 0
'                        Throw New ApplicationException(message)
'                    End If
'                Else
'                    If Not cs.Insert(contentName) Then
'                        cs.Close()
'                        ID = 0
'                        Throw New ApplicationException("Unable to insert record in content [" & contentName & "]")
'                    End If
'                End If
'                If cs.ok() Then
'                    ID = cs.getInteger("id")
'                    cs.setField("Active", Active.ToString())
'                    cs.setField("AllowBrief", AllowBrief.ToString())
'                    cs.setField("AllowChildListDisplay", AllowChildListDisplay.ToString())
'                    cs.setField("AllowEmailPage", AllowEmailPage.ToString())
'                    cs.setField("AllowFeedback", AllowFeedback.ToString())
'                    cs.setField("AllowHitNotification", AllowHitNotification.ToString())
'                    cs.setField("AllowInChildLists", AllowInChildLists.ToString())
'                    cs.setField("AllowInMenus", AllowInMenus.ToString())
'                    cs.setField("AllowLastModifiedFooter", AllowLastModifiedFooter.ToString())
'                    cs.setField("AllowMessageFooter", AllowMessageFooter.ToString())
'                    cs.setField("AllowMetaContentNoFollow", AllowMetaContentNoFollow.ToString())
'                    cs.setField("AllowMoreInfo", AllowMoreInfo.ToString())
'                    cs.setField("AllowPrinterVersion", AllowPrinterVersion.ToString())
'                    cs.setField("AllowReturnLinkDisplay", AllowReturnLinkDisplay.ToString())
'                    cs.setField("AllowReviewedFooter", AllowReviewedFooter.ToString())
'                    cs.setField("AllowSeeAlso", AllowSeeAlso.ToString())
'                    cs.setField("ArchiveParentID", ArchiveParentID.ToString())
'                    cs.setField("BlockContent", BlockContent.ToString())
'                    cs.setField("BlockPage", BlockPage.ToString())
'                    cs.setField("BlockSourceID", BlockSourceID.ToString())
'                    cs.setField("BriefFilename", BriefFilename)
'                    cs.setField("ccGuid", ccGuid)
'                    cs.setField("ChildListInstanceOptions", ChildListInstanceOptions)
'                    cs.setField("ChildListSortMethodID", ChildListSortMethodID.ToString())
'                    cs.setField("ChildPagesFound", ChildPagesFound.ToString())
'                    cs.setField("Clicks", Clicks.ToString())
'                    cs.setField("ContactMemberID", ContactMemberID.ToString())
'                    cs.setField("ContentCategoryID", ContentCategoryID.ToString())
'                    cs.setField("ContentControlID", ContentControlID.ToString())
'                    cs.setField("ContentPadding", ContentPadding.ToString())
'                    cs.setField("Copyfilename", Copyfilename)
'                    cs.setField("CreatedBy", CreatedBy.ToString())
'                    cs.setField("CreateKey", CreateKey.ToString())
'                    cs.setField("CustomBlockMessage", CustomBlockMessage)
'                    cs.setField("DateAdded", DateAdded.ToString())
'                    cs.setField("DateArchive", DateArchive.ToString())
'                    cs.setField("DateExpires", DateExpires.ToString())
'                    cs.setField("DateReviewed", DateReviewed.ToString())
'                    cs.setField("EditArchive", EditArchive.ToString())
'                    cs.setField("EditBlank", EditBlank.ToString())
'                    cs.setField("EditSourceID", EditSourceID.ToString())
'                    cs.setField("Headline", Headline)
'                    'cs.setField("ImageFilename", ImageFilename)
'                    cs.setField("IsSecure", IsSecure.ToString())
'                    cs.setField("JSEndBody", JSEndBody)
'                    cs.setField("JSFilename", JSFilename)
'                    cs.setField("JSHead", JSHead)
'                    cs.setField("JSOnLoad", JSOnLoad)
'                    cs.setField("LinkAlias", LinkAlias)
'                    'cs.setField("Marquee", Marquee)
'                    cs.setField("MenuHeadline", MenuHeadline)
'                    'cs.setField("MenuImageFileName", MenuImageFileName)
'                    cs.setField("ModifiedBy", ModifiedBy.ToString())
'                    cs.setField("ModifiedDate", ModifiedDate.ToString())
'                    cs.setField("Name", Name)
'                    'cs.setField("OrganizationID", OrganizationID.ToString())
'                    cs.setField("PageLink", PageLink)
'                    cs.setField("ParentID", ParentID.ToString())
'                    cs.setField("ParentListName", ParentListName)
'                    'cs.setField("PodcastMediaLink", PodcastMediaLink)
'                    'cs.setField("PodcastSize", PodcastSize.ToString())
'                    cs.setField("PubDate", PubDate.ToString())
'                    cs.setField("RegistrationGroupID", RegistrationGroupID.ToString())
'                    'cs.setField("RegistrationRequired", RegistrationRequired.ToString())
'                    cs.setField("ReviewedBy", ReviewedBy.ToString())
'                    'cs.setField("RSSDateExpire", RSSDateExpire.ToString())
'                    ' cs.setField("RSSDatePublish", RSSDatePublish.ToString())
'                    'cs.setField("RSSDescription", RSSDescription)
'                    'cs.setField("RSSLink", RSSLink)
'                    'cs.setField("RSSTitle", RSSTitle)
'                    cs.setField("SortOrder", SortOrder)
'                    cs.setField("TemplateID", TemplateID.ToString())
'                    cs.setField("TriggerAddGroupID", TriggerAddGroupID.ToString())
'                    cs.setField("TriggerConditionGroupID", TriggerConditionGroupID.ToString())
'                    cs.setField("TriggerConditionID", TriggerConditionID.ToString())
'                    cs.setField("TriggerRemoveGroupID", TriggerRemoveGroupID.ToString())
'                    cs.setField("TriggerSendSystemEmailID", TriggerSendSystemEmailID.ToString())
'                    cs.setField("Viewings", Viewings.ToString())
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
'                cpCore.cache.setObject(Controllers.cacheController.getDbRecordCacheName(tableName, "id", Me.ID.ToString()), Me)
'            Catch ex As Exception
'                cpCore.handleExceptionAndContinue(ex) : Throw
'                Throw
'            End Try
'            Return ID
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
'                    cpCore.db.deleteContentRecords(contentName, "id=" & recordId.ToString)
'                    cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(tableName, "id", recordId.ToString))
'                End If
'            Catch ex As Exception
'                cpCore.handleExceptionAndContinue(ex) : Throw
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
'                    Dim instance As pageContentModel = create(cpCore, ccguid, New List(Of String))
'                    If (instance IsNot Nothing) Then
'                        invalidatePrimaryCache(cpCore, instance.ID)
'                        cpCore.db.deleteContentRecords(contentName, "(ccguid=" & cpCore.db.encodeSQLText(ccguid) & ")")
'                    End If
'                End If
'            Catch ex As Exception
'                cpCore.handleExceptionAndContinue(ex) : Throw
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
'                    Dim instance As pageContentModel = create(cpCore, foreignKey1Id, foreignKey2Id, New List(Of String))
'                    If (instance IsNot Nothing) Then
'                        invalidatePrimaryCache(cpCore, instance.ID)
'                        cpCore.db.deleteTableRecord(tableName, instance.ID, dataSource)
'                    End If
'                End If
'            Catch ex As Exception
'                cpCore.handleExceptionAndContinue(ex) : Throw
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
'        Public Shared Function createList_criteria(cpCore As coreClass, someCriteria As Integer, callersCacheNameList As List(Of String)) As List(Of pageContentModel)
'            Dim result As New List(Of pageContentModel)
'            Try
'                Dim cs As New csController(cpCore)
'                Dim ignoreCacheNames As New List(Of String)
'                If (cs.open(contentName, "(someCriteria=" & someCriteria & ")", "id")) Then
'                    Dim instance As pageContentModel
'                    Do
'                        instance = pageContentModel.loadRecord(cpCore, cs, callersCacheNameList)
'                        If (instance IsNot Nothing) Then
'                            result.Add(instance)
'                        End If
'                        cs.goNext()
'                    Loop While cs.ok()
'                End If
'                cs.Close()
'            Catch ex As Exception
'                cpCore.handleExceptionAndContinue(ex) : Throw
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
'            cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(tableName, "id", recordId.ToString))
'            '
'            ' -- the zero record cache means any record was updated. Can be used to invalidate arbitraty lists of records in the table
'            cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(tableName, "id", "0"))
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
'            Return pageContentModel.create(cpcore, recordId, New List(Of String)).Name
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
'            Return pageContentModel.create(cpcore, ccGuid, New List(Of String)).Name
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
'            Return pageContentModel.create(cpcore, ccGuid, New List(Of String)).ID
'        End Function
'        '
'        '====================================================================================================
'        '
'        Public Shared Function createDefault(cpcore As coreClass) As pageContentModel
'            Dim instance As New pageContentModel
'            Try
'                Dim CDef As cdefModel = cpcore.metaData.getCdef(contentName)
'                If (CDef Is Nothing) Then
'                    Throw New ApplicationException("content [" & contentName & "] could Not be found.")
'                ElseIf (CDef.Id <= 0) Then
'                    Throw New ApplicationException("content [" & contentName & "] could Not be found.")
'                Else
'                    With CDef
'                        instance.Active = genericController.EncodeBoolean(.fields("Active").defaultValue)
'                        instance.AllowBrief = genericController.EncodeBoolean(.fields("AllowBrief").defaultValue)
'                        instance.AllowChildListDisplay = genericController.EncodeBoolean(.fields("AllowChildListDisplay").defaultValue)
'                        instance.AllowEmailPage = genericController.EncodeBoolean(.fields("AllowEmailPage").defaultValue)
'                        instance.AllowFeedback = genericController.EncodeBoolean(.fields("AllowFeedback").defaultValue)
'                        instance.AllowHitNotification = genericController.EncodeBoolean(.fields("AllowHitNotification").defaultValue)
'                        instance.AllowInChildLists = genericController.EncodeBoolean(.fields("AllowInChildLists").defaultValue)
'                        instance.AllowInMenus = genericController.EncodeBoolean(.fields("AllowInMenus").defaultValue)
'                        instance.AllowLastModifiedFooter = genericController.EncodeBoolean(.fields("AllowLastModifiedFooter").defaultValue)
'                        instance.AllowMessageFooter = genericController.EncodeBoolean(.fields("AllowMessageFooter").defaultValue)
'                        instance.AllowMetaContentNoFollow = genericController.EncodeBoolean(.fields("AllowMetaContentNoFollow").defaultValue)
'                        instance.AllowMoreInfo = genericController.EncodeBoolean(.fields("AllowMoreInfo").defaultValue)
'                        instance.AllowPrinterVersion = genericController.EncodeBoolean(.fields("AllowPrinterVersion").defaultValue)
'                        instance.AllowReturnLinkDisplay = genericController.EncodeBoolean(.fields("AllowReturnLinkDisplay").defaultValue)
'                        instance.AllowReviewedFooter = genericController.EncodeBoolean(.fields("AllowReviewedFooter").defaultValue)
'                        instance.AllowSeeAlso = genericController.EncodeBoolean(.fields("AllowSeeAlso").defaultValue)
'                        instance.ArchiveParentID = genericController.EncodeInteger(.fields("ArchiveParentID").defaultValue)
'                        instance.BlockContent = genericController.EncodeBoolean(.fields("BlockContent").defaultValue)
'                        instance.BlockPage = genericController.EncodeBoolean(.fields("BlockPage").defaultValue)
'                        instance.BlockSourceID = genericController.EncodeInteger(.fields("BlockSourceID").defaultValue)
'                        instance.BriefFilename = genericController.encodeText(.fields("BriefFilename").defaultValue)
'                        instance.ccGuid = genericController.encodeText(.fields("ccGuid").defaultValue)
'                        instance.ChildListInstanceOptions = genericController.encodeText(.fields("ChildListInstanceOptions").defaultValue)
'                        instance.ChildListSortMethodID = genericController.EncodeInteger(.fields("ChildListSortMethodID").defaultValue)
'                        instance.ChildPagesFound = genericController.EncodeBoolean(.fields("ChildPagesFound").defaultValue)
'                        instance.Clicks = genericController.EncodeInteger(.fields("Clicks").defaultValue)
'                        instance.ContactMemberID = genericController.EncodeInteger(.fields("ContactMemberID").defaultValue)
'                        instance.ContentCategoryID = genericController.EncodeInteger(.fields("ContentCategoryID").defaultValue)
'                        instance.ContentControlID = CDef.Id
'                        instance.ContentPadding = genericController.EncodeInteger(.fields("ContentPadding").defaultValue)
'                        instance.Copyfilename = genericController.encodeText(.fields("Copyfilename").defaultValue)
'                        instance.CreatedBy = genericController.EncodeInteger(.fields("CreatedBy").defaultValue)
'                        instance.CreateKey = genericController.EncodeInteger(.fields("CreateKey").defaultValue)
'                        instance.CustomBlockMessage = genericController.encodeText(.fields("CustomBlockMessage").defaultValue)
'                        instance.DateAdded = genericController.EncodeDate(.fields("DateAdded").defaultValue)
'                        instance.DateArchive = genericController.EncodeDate(.fields("DateArchive").defaultValue)
'                        instance.DateExpires = genericController.EncodeDate(.fields("DateExpires").defaultValue)
'                        instance.DateReviewed = genericController.EncodeDate(.fields("DateReviewed").defaultValue)
'                        instance.EditArchive = genericController.EncodeBoolean(.fields("EditArchive").defaultValue)
'                        instance.EditBlank = genericController.EncodeBoolean(.fields("EditBlank").defaultValue)
'                        instance.EditSourceID = genericController.EncodeInteger(.fields("EditSourceID").defaultValue)
'                        instance.Headline = genericController.encodeText(.fields("Headline").defaultValue)
'                        'instance.ImageFilename = genericController.encodeText(.fields("ImageFilename").defaultValue)
'                        instance.IsSecure = genericController.EncodeBoolean(.fields("IsSecure").defaultValue)
'                        instance.JSEndBody = genericController.encodeText(.fields("JSEndBody").defaultValue)
'                        instance.JSFilename = genericController.encodeText(.fields("JSFilename").defaultValue)
'                        instance.JSHead = genericController.encodeText(.fields("JSHead").defaultValue)
'                        instance.JSOnLoad = genericController.encodeText(.fields("JSOnLoad").defaultValue)
'                        instance.LinkAlias = genericController.encodeText(.fields("LinkAlias").defaultValue)
'                        'instance.Marquee = genericController.encodeText(.fields("Marquee").defaultValue)
'                        instance.MenuHeadline = genericController.encodeText(.fields("MenuHeadline").defaultValue)
'                        'instance.MenuImageFileName = genericController.encodeText(.fields("MenuImageFileName").defaultValue)
'                        instance.ModifiedBy = genericController.EncodeInteger(.fields("ModifiedBy").defaultValue)
'                        instance.ModifiedDate = genericController.EncodeDate(.fields("ModifiedDate").defaultValue)
'                        instance.Name = genericController.encodeText(.fields("Name").defaultValue)
'                        'instance.OrganizationID = genericController.EncodeInteger(.fields("OrganizationID").defaultValue)
'                        instance.PageLink = genericController.encodeText(.fields("PageLink").defaultValue)
'                        instance.ParentID = genericController.EncodeInteger(.fields("ParentID").defaultValue)
'                        instance.ParentListName = genericController.encodeText(.fields("ParentListName").defaultValue)
'                        'instance.PodcastMediaLink = genericController.encodeText(.fields("PodcastMediaLink").defaultValue)
'                        'instance.PodcastSize = genericController.EncodeInteger(.fields("PodcastSize").defaultValue)
'                        instance.PubDate = genericController.EncodeDate(.fields("PubDate").defaultValue)
'                        instance.RegistrationGroupID = genericController.EncodeInteger(.fields("RegistrationGroupID").defaultValue)
'                        'instance.RegistrationRequired = genericController.EncodeBoolean(.fields("RegistrationRequired").defaultValue)
'                        instance.ReviewedBy = genericController.EncodeInteger(.fields("ReviewedBy").defaultValue)
'                        'instance.RSSDateExpire = genericController.EncodeDate(.fields("RSSDateExpire").defaultValue)
'                        'instance.RSSDatePublish = genericController.EncodeDate(.fields("RSSDatePublish").defaultValue)
'                        'instance.RSSDescription = genericController.encodeText(.fields("RSSDescription").defaultValue)
'                        'instance.RSSLink = genericController.encodeText(.fields("RSSLink").defaultValue)
'                        'instance.RSSTitle = genericController.encodeText(.fields("RSSTitle").defaultValue)
'                        instance.SortOrder = genericController.encodeText(.fields("SortOrder").defaultValue)
'                        instance.TemplateID = genericController.EncodeInteger(.fields("TemplateID").defaultValue)
'                        instance.TriggerAddGroupID = genericController.EncodeInteger(.fields("TriggerAddGroupID").defaultValue)
'                        instance.TriggerConditionGroupID = genericController.EncodeInteger(.fields("TriggerConditionGroupID").defaultValue)
'                        instance.TriggerConditionID = genericController.EncodeInteger(.fields("TriggerConditionID").defaultValue)
'                        instance.TriggerRemoveGroupID = genericController.EncodeInteger(.fields("TriggerRemoveGroupID").defaultValue)
'                        instance.TriggerSendSystemEmailID = genericController.EncodeInteger(.fields("TriggerSendSystemEmailID").defaultValue)
'                        instance.Viewings = genericController.EncodeInteger(.fields("Viewings").defaultValue)
'                    End With
'                End If
'            Catch ex As Exception
'                cpcore.handleExceptionAndContinue(ex)
'            End Try
'            Return instance
'        End Function
'    End Class
'End Namespace
