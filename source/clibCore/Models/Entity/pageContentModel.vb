
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    '
    Public Class pageContentModel
        '
        '-- const
        Public Const primaryContentName As String = "page content"
        Private Const primaryContentTableName As String = "ccpagecontent"
        Private Const primaryContentDataSource As String = "default" '<----- set to datasource if not default
        '
        ' -- instance properties
        Public ID As Integer
        Public Active As Boolean
        Public AllowBrief As Boolean
        Public AllowChildListDisplay As Boolean
        Public AllowEmailPage As Boolean
        Public AllowFeedback As Boolean
        Public AllowHitNotification As Boolean
        Public AllowInChildLists As Boolean
        Public AllowInMenus As Boolean
        Public AllowLastModifiedFooter As Boolean
        Public AllowMessageFooter As Boolean
        Public AllowMetaContentNoFollow As Boolean
        Public AllowMoreInfo As Boolean
        Public AllowPrinterVersion As Boolean
        Public AllowReturnLinkDisplay As Boolean
        Public AllowReviewedFooter As Boolean
        Public AllowSeeAlso As Boolean
        Public AlternateContentID As Integer
        Public AlternateContentLink As String
        Public ArchiveParentID As Integer
        Public BlockContent As Boolean
        Public BlockPage As Boolean
        Public BlockSourceID As Integer
        Public BriefFilename As String
        Public ccGuid As String
        Public ChildListInstanceOptions As String
        Public ChildListSortMethodID As Integer
        Public ChildPagesFound As Boolean
        Public Clicks As Integer
        Public ContactMemberID As Integer
        Public ContentCategoryID As Integer
        Public ContentControlID As Integer
        Public ContentPadding As Integer
        Public Copyfilename As String
        Public CreatedBy As Integer
        Public CreateKey As Integer
        Public CustomBlockMessage As String
        Public DateAdded As Date
        Public DateArchive As Date
        Public DateExpires As Date
        Public DateReviewed As Date
        Public DocFilename As String
        Public DocLabel As String
        Public EditArchive As Boolean
        Public EditBlank As Boolean
        Public EditSourceID As Integer
        Public Headline As String
        Public ImageFilename As String
        Public IsSecure As Boolean
        Public JSEndBody As String
        Public JSFilename As String
        Public JSHead As String
        Public JSOnLoad As String
        Public Link As String
        Public LinkAlias As String
        Public LinkLabel As String
        Public Marquee As String
        Public MenuHeadline As String
        Public MenuImageFileName As String
        Public ModifiedBy As Integer
        Public ModifiedDate As Date
        Public Name As String
        Public OrganizationID As Integer
        Public PageLink As String
        Public ParentID As Integer
        Public ParentListName As String
        Public PodcastMediaLink As String
        Public PodcastSize As Integer
        Public PubDate As Date
        Public RegistrationGroupID As Integer
        Public RegistrationRequired As Boolean
        Public ReviewedBy As Integer
        Public RSSDateExpire As Date
        Public RSSDatePublish As Date
        Public RSSDescription As String
        Public RSSLink As String
        Public RSSTitle As String
        Public SortOrder As String
        Public TemplateID As Integer
        Public TriggerAddGroupID As Integer
        Public TriggerConditionGroupID As Integer
        Public TriggerConditionID As Integer
        Public TriggerRemoveGroupID As Integer
        Public TriggerSendSystemEmailID As Integer
        Public Viewings As Integer
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
        Public Shared Function create(cpCore As coreClass, recordId As Integer, ByRef cacheNameList As List(Of String)) As pageContentModel
            Dim result As pageContentModel = Nothing
            Try
                If recordId > 0 Then
                    Dim cacheName As String = GetType(pageContentModel).FullName & getCacheName("id", recordId.ToString())
                    result = cpCore.cache.getObject(Of pageContentModel)(cacheName)
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
        Public Shared Function create(cpCore As coreClass, recordGuid As String, ByRef cacheNameList As List(Of String)) As pageContentModel
            Dim result As pageContentModel = Nothing
            Try
                If Not String.IsNullOrEmpty(recordGuid) Then
                    Dim cacheName As String = GetType(pageContentModel).FullName & getCacheName("ccguid", recordGuid)
                    result = cpCore.cache.getObject(Of pageContentModel)(cacheName)
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
        Private Shared Function loadObject(cpCore As coreClass, sqlCriteria As String, ByRef cacheNameList As List(Of String)) As pageContentModel
            Dim result As pageContentModel = Nothing
            Try
                Dim cs As New csController(cpCore)
                If cs.open(primaryContentName, sqlCriteria) Then
                    result = New pageContentModel
                    With result
                        .ID = cs.getInteger("ID")
                        .Active = cs.getBoolean("Active")
                        .AllowBrief = cs.getBoolean("AllowBrief")
                        .AllowChildListDisplay = cs.getBoolean("AllowChildListDisplay")
                        .AllowEmailPage = cs.getBoolean("AllowEmailPage")
                        .AllowFeedback = cs.getBoolean("AllowFeedback")
                        .AllowHitNotification = cs.getBoolean("AllowHitNotification")
                        .AllowInChildLists = cs.getBoolean("AllowInChildLists")
                        .AllowInMenus = cs.getBoolean("AllowInMenus")
                        .AllowLastModifiedFooter = cs.getBoolean("AllowLastModifiedFooter")
                        .AllowMessageFooter = cs.getBoolean("AllowMessageFooter")
                        .AllowMetaContentNoFollow = cs.getBoolean("AllowMetaContentNoFollow")
                        .AllowMoreInfo = cs.getBoolean("AllowMoreInfo")
                        .AllowPrinterVersion = cs.getBoolean("AllowPrinterVersion")
                        .AllowReturnLinkDisplay = cs.getBoolean("AllowReturnLinkDisplay")
                        .AllowReviewedFooter = cs.getBoolean("AllowReviewedFooter")
                        .AllowSeeAlso = cs.getBoolean("AllowSeeAlso")
                        .AlternateContentID = cs.getInteger("AlternateContentID")
                        .AlternateContentLink = cs.getText("AlternateContentLink")
                        .ArchiveParentID = cs.getInteger("ArchiveParentID")
                        .BlockContent = cs.getBoolean("BlockContent")
                        .BlockPage = cs.getBoolean("BlockPage")
                        .BlockSourceID = cs.getInteger("BlockSourceID")
                        .BriefFilename = cs.getText("BriefFilename")
                        .ccGuid = cs.getText("ccGuid")
                        .ChildListInstanceOptions = cs.getText("ChildListInstanceOptions")
                        .ChildListSortMethodID = cs.getInteger("ChildListSortMethodID")
                        .ChildPagesFound = cs.getBoolean("ChildPagesFound")
                        .Clicks = cs.getInteger("Clicks")
                        .ContactMemberID = cs.getInteger("ContactMemberID")
                        .ContentCategoryID = cs.getInteger("ContentCategoryID")
                        .ContentControlID = cs.getInteger("ContentControlID")
                        .ContentPadding = cs.getInteger("ContentPadding")
                        .Copyfilename = cs.getText("Copyfilename")
                        .CreatedBy = cs.getInteger("CreatedBy")
                        .CreateKey = cs.getInteger("CreateKey")
                        .CustomBlockMessage = cs.getText("CustomBlockMessage")
                        .DateAdded = cs.getDate("DateAdded")
                        .DateArchive = cs.getDate("DateArchive")
                        .DateExpires = cs.getDate("DateExpires")
                        .DateReviewed = cs.getDate("DateReviewed")
                        .DocFilename = cs.getText("DocFilename")
                        .DocLabel = cs.getText("DocLabel")
                        .EditArchive = cs.getBoolean("EditArchive")
                        .EditBlank = cs.getBoolean("EditBlank")
                        .EditSourceID = cs.getInteger("EditSourceID")
                        .Headline = cs.getText("Headline")
                        .ImageFilename = cs.getText("ImageFilename")
                        .IsSecure = cs.getBoolean("IsSecure")
                        .JSEndBody = cs.getText("JSEndBody")
                        .JSFilename = cs.getText("JSFilename")
                        .JSHead = cs.getText("JSHead")
                        .JSOnLoad = cs.getText("JSOnLoad")
                        .Link = cs.getText("Link")
                        .LinkAlias = cs.getText("LinkAlias")
                        .LinkLabel = cs.getText("LinkLabel")
                        .Marquee = cs.getText("Marquee")
                        .MenuHeadline = cs.getText("MenuHeadline")
                        .MenuImageFileName = cs.getText("MenuImageFileName")
                        .ModifiedBy = cs.getInteger("ModifiedBy")
                        .ModifiedDate = cs.getDate("ModifiedDate")
                        .Name = cs.getText("Name")
                        .OrganizationID = cs.getInteger("OrganizationID")
                        .PageLink = cs.getText("PageLink")
                        .ParentID = cs.getInteger("ParentID")
                        .ParentListName = cs.getText("ParentListName")
                        .PodcastMediaLink = cs.getText("PodcastMediaLink")
                        .PodcastSize = cs.getInteger("PodcastSize")
                        .PubDate = cs.getDate("PubDate")
                        .RegistrationGroupID = cs.getInteger("RegistrationGroupID")
                        .RegistrationRequired = cs.getBoolean("RegistrationRequired")
                        .ReviewedBy = cs.getInteger("ReviewedBy")
                        .RSSDateExpire = cs.getDate("RSSDateExpire")
                        .RSSDatePublish = cs.getDate("RSSDatePublish")
                        .RSSDescription = cs.getText("RSSDescription")
                        .RSSLink = cs.getText("RSSLink")
                        .RSSTitle = cs.getText("RSSTitle")
                        .SortOrder = cs.getText("SortOrder")
                        .TemplateID = cs.getInteger("TemplateID")
                        .TriggerAddGroupID = cs.getInteger("TriggerAddGroupID")
                        .TriggerConditionGroupID = cs.getInteger("TriggerConditionGroupID")
                        .TriggerConditionID = cs.getInteger("TriggerConditionID")
                        .TriggerRemoveGroupID = cs.getInteger("TriggerRemoveGroupID")
                        .TriggerSendSystemEmailID = cs.getInteger("TriggerSendSystemEmailID")
                        .Viewings = cs.getInteger("Viewings")
                        If (String.IsNullOrEmpty(.ccGuid)) Then .ccGuid = Controllers.genericController.getGUID() End With
                    If (result IsNot Nothing) Then
                        '
                        ' -- set primary and secondary caches
                        ' -- add all cachenames to the injected cachenamelist
                        Dim cacheName0 As String = getCacheName("id", result.id.ToString())
                        cacheNameList.Add(cacheName0)
                        cpCore.cache.setObject(cacheName0, result)
                        '
                        Dim cacheName1 As String = getCacheName("ccguid", result.ccGuid)
                        cacheNameList.Add(cacheName1)
                        cpCore.cache.setObject(cacheName1, Nothing, cacheName0)
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
                    If (String.IsNullOrEmpty(ccGuid)) Then
                        ccGuid = Controllers.genericController.getGUID()
                    End If
                    cs.setField("Active", Active.ToString())
                    cs.SetField("AllowBrief", AllowBrief.ToString())
                    cs.SetField("AllowChildListDisplay", AllowChildListDisplay.ToString())
                    cs.SetField("AllowEmailPage", AllowEmailPage.ToString())
                    cs.SetField("AllowFeedback", AllowFeedback.ToString())
                    cs.SetField("AllowHitNotification", AllowHitNotification.ToString())
                    cs.SetField("AllowInChildLists", AllowInChildLists.ToString())
                    cs.SetField("AllowInMenus", AllowInMenus.ToString())
                    cs.SetField("AllowLastModifiedFooter", AllowLastModifiedFooter.ToString())
                    cs.SetField("AllowMessageFooter", AllowMessageFooter.ToString())
                    cs.SetField("AllowMetaContentNoFollow", AllowMetaContentNoFollow.ToString())
                    cs.SetField("AllowMoreInfo", AllowMoreInfo.ToString())
                    cs.SetField("AllowPrinterVersion", AllowPrinterVersion.ToString())
                    cs.SetField("AllowReturnLinkDisplay", AllowReturnLinkDisplay.ToString())
                    cs.SetField("AllowReviewedFooter", AllowReviewedFooter.ToString())
                    cs.SetField("AllowSeeAlso", AllowSeeAlso.ToString())
                    cs.SetField("AlternateContentID", AlternateContentID.ToString())
                    cs.SetField("AlternateContentLink", AlternateContentLink)
                    cs.SetField("ArchiveParentID", ArchiveParentID.ToString())
                    cs.SetField("BlockContent", BlockContent.ToString())
                    cs.SetField("BlockPage", BlockPage.ToString())
                    cs.SetField("BlockSourceID", BlockSourceID.ToString())
                    cs.SetField("BriefFilename", BriefFilename)
                    cs.SetField("ccGuid", ccGuid)
                    cs.SetField("ChildListInstanceOptions", ChildListInstanceOptions)
                    cs.SetField("ChildListSortMethodID", ChildListSortMethodID.ToString())
                    cs.SetField("ChildPagesFound", ChildPagesFound.ToString())
                    cs.SetField("Clicks", Clicks.ToString())
                    cs.SetField("ContactMemberID", ContactMemberID.ToString())
                    cs.SetField("ContentCategoryID", ContentCategoryID.ToString())
                    cs.SetField("ContentControlID", ContentControlID.ToString())
                    cs.SetField("ContentPadding", ContentPadding.ToString())
                    cs.SetField("Copyfilename", Copyfilename)
                    cs.SetField("CreatedBy", CreatedBy.ToString())
                    cs.SetField("CreateKey", CreateKey.ToString())
                    cs.SetField("CustomBlockMessage", CustomBlockMessage)
                    cs.SetField("DateAdded", DateAdded.ToString())
                    cs.SetField("DateArchive", DateArchive.ToString())
                    cs.SetField("DateExpires", DateExpires.ToString())
                    cs.SetField("DateReviewed", DateReviewed.ToString())
                    cs.SetField("DocFilename", DocFilename)
                    cs.SetField("DocLabel", DocLabel)
                    cs.SetField("EditArchive", EditArchive.ToString())
                    cs.SetField("EditBlank", EditBlank.ToString())
                    cs.SetField("EditSourceID", EditSourceID.ToString())
                    cs.SetField("Headline", Headline)
                    cs.SetField("ImageFilename", ImageFilename)
                    cs.SetField("IsSecure", IsSecure.ToString())
                    cs.SetField("JSEndBody", JSEndBody)
                    cs.SetField("JSFilename", JSFilename)
                    cs.SetField("JSHead", JSHead)
                    cs.SetField("JSOnLoad", JSOnLoad)
                    cs.SetField("Link", Link)
                    cs.SetField("LinkAlias", LinkAlias)
                    cs.SetField("LinkLabel", LinkLabel)
                    cs.SetField("Marquee", Marquee)
                    cs.SetField("MenuHeadline", MenuHeadline)
                    cs.SetField("MenuImageFileName", MenuImageFileName)
                    cs.SetField("ModifiedBy", ModifiedBy.ToString())
                    cs.SetField("ModifiedDate", ModifiedDate.ToString())
                    cs.SetField("Name", Name)
                    cs.SetField("OrganizationID", OrganizationID.ToString())
                    cs.SetField("PageLink", PageLink)
                    cs.SetField("ParentID", ParentID.ToString())
                    cs.SetField("ParentListName", ParentListName)
                    cs.SetField("PodcastMediaLink", PodcastMediaLink)
                    cs.SetField("PodcastSize", PodcastSize.ToString())
                    cs.SetField("PubDate", PubDate.ToString())
                    cs.SetField("RegistrationGroupID", RegistrationGroupID.ToString())
                    cs.SetField("RegistrationRequired", RegistrationRequired.ToString())
                    cs.SetField("ReviewedBy", ReviewedBy.ToString())
                    cs.SetField("RSSDateExpire", RSSDateExpire.ToString())
                    cs.SetField("RSSDatePublish", RSSDatePublish.ToString())
                    cs.SetField("RSSDescription", RSSDescription)
                    cs.SetField("RSSLink", RSSLink)
                    cs.SetField("RSSTitle", RSSTitle)
                    cs.SetField("SortOrder", SortOrder)
                    cs.SetField("TemplateID", TemplateID.ToString())
                    cs.SetField("TriggerAddGroupID", TriggerAddGroupID.ToString())
                    cs.SetField("TriggerConditionGroupID", TriggerConditionGroupID.ToString())
                    cs.SetField("TriggerConditionID", TriggerConditionID.ToString())
                    cs.SetField("TriggerRemoveGroupID", TriggerRemoveGroupID.ToString())
                    cs.SetField("TriggerSendSystemEmailID", TriggerSendSystemEmailID.ToString())
                    cs.SetField("Viewings", Viewings.ToString())
                End If
                Call cs.Close()
                '
                ' -- invalidate objects
                cpCore.cache.invalidateObject(getCacheName("id", id.ToString))
                cpCore.cache.invalidateObject(getCacheName("ccguid", ccGuid))
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
        Public Shared Function getObjectList(cpCore As coreClass, someCriteria As Integer) As List(Of pageContentModel)
            Dim result As New List(Of pageContentModel)
            Try
                Dim cs As New csController(cpCore)
                Dim ignoreCacheNames As New List(Of String)
                If (cs.open(primaryContentName, "(someCriteria=" & someCriteria & ")", "name", True, "id")) Then
                    Dim instance As pageContentModel
                    Do
                        instance = pageContentModel.create(cpCore, cs.getInteger("id"), ignoreCacheNames)
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
            cpCore.cache.invalidateObject(getCacheName("ccguid", guid))
            '
            ' -- always clear the cache with the content name
            '?? cpCore.cache.invalidateObject(primaryContentName)
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
            'Return (GetType(pageContentModel).FullName & "." & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' add a new recod to the db and open it. Starting a new model with this method will use the default
        ''' values in Contensive metadata (active, contentcontrolid, etc)
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="cacheNameList"></param>
        ''' <returns></returns>
        Public Shared Function add(cpCore As coreClass, ByRef cacheNameList As List(Of String)) As pageContentModel
            Dim result As pageContentModel = Nothing
            Try
                result = create(cpCore, cpCore.db.metaData_InsertContentRecordGetID(primaryContentName, 0), cacheNameList)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return result
        End Function
    End Class
End Namespace
