
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class pageContentModel
        Inherits baseModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "page content"
        Public Const contentTableName As String = "ccpagecontent"
        Private Shadows Const contentDataSource As String = "default"
        '
        '====================================================================================================
        ' -- instance properties
        Public Property AllowBrief As Boolean
        Public Property AllowChildListDisplay As Boolean
        Public Property AllowEmailPage As Boolean
        Public Property AllowFeedback As Boolean
        Public Property AllowHitNotification As Boolean
        Public Property AllowInChildLists As Boolean
        Public Property AllowInMenus As Boolean
        Public Property AllowLastModifiedFooter As Boolean
        Public Property AllowMessageFooter As Boolean
        Public Property AllowMetaContentNoFollow As Boolean
        Public Property AllowMoreInfo As Boolean
        Public Property AllowPrinterVersion As Boolean
        Public Property AllowReturnLinkDisplay As Boolean
        Public Property AllowReviewedFooter As Boolean
        Public Property AllowSeeAlso As Boolean
        Public Property ArchiveParentID As Integer
        Public Property BlockContent As Boolean
        Public Property BlockPage As Boolean
        Public Property BlockSourceID As Integer
        Public Property BriefFilename As String
        Public Property ChildListInstanceOptions As String
        Public Property ChildListSortMethodID As Integer
        Public Property ChildPagesFound As Boolean
        Public Property Clicks As Integer
        Public Property ContactMemberID As Integer
        Public Property ContentPadding As Integer
        Public Property Copyfilename As New fieldTypeHTMLFile
        Public Property CustomBlockMessage As String
        Public Property DateArchive As Date
        Public Property DateExpires As Date
        Public Property DateReviewed As Date
        Public Property EditArchive As Boolean
        Public Property EditBlank As Boolean
        Public Property EditSourceID As Integer
        Public Property Headline As String
        Public Property imageFilename As String
        Public Property IsSecure As Boolean
        Public Property JSEndBody As String
        Public Property JSFilename As String
        Public Property JSHead As String
        Public Property JSOnLoad As String
        Public Property LinkAlias As String
        Public Property MenuHeadline As String
        Public Property metaDescription As String
        Public Property MetaKeywordList As String
        Public Property OtherHeadTags As String
        Public Property PageLink As String
        Public Property pageTitle As String
        Public Property ParentID As Integer
        Public Property ParentListName As String
        Public Property PubDate As Date
        Public Property RegistrationGroupID As Integer
        Public Property ReviewedBy As Integer
        Public Property TemplateID As Integer
        Public Property TriggerAddGroupID As Integer
        Public Property TriggerConditionGroupID As Integer
        Public Property TriggerConditionID As Integer
        Public Property TriggerRemoveGroupID As Integer
        Public Property TriggerSendSystemEmailID As Integer
        Public Property Viewings As Integer
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass) As pageContentModel
            Return add(Of pageContentModel)(cpCore)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As pageContentModel
            Return add(Of pageContentModel)(cpCore, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer) As pageContentModel
            Return create(Of pageContentModel)(cpCore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As pageContentModel
            Return create(Of pageContentModel)(cpCore, recordId, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String) As pageContentModel
            Return create(Of pageContentModel)(cpCore, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As pageContentModel
            Return create(Of pageContentModel)(cpCore, recordGuid, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String) As pageContentModel
            Return createByName(Of pageContentModel)(cpCore, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As pageContentModel
            Return createByName(Of pageContentModel)(cpCore, recordName, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cpCore As coreClass)
            MyBase.save(cpCore)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, recordId As Integer)
            delete(Of pageContentModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, ccGuid As String)
            delete(Of pageContentModel)(cpCore, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String, callersCacheNameList As List(Of String)) As List(Of pageContentModel)
            Return createList(Of pageContentModel)(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String) As List(Of pageContentModel)
            Return createList(Of pageContentModel)(cpCore, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String) As List(Of pageContentModel)
            Return createList(Of pageContentModel)(cpCore, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
            invalidateCacheSingleRecord(Of pageContentModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return baseModel.getRecordName(Of pageContentModel)(cpcore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of pageContentModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
            Return baseModel.getRecordId(Of pageContentModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createDefault(cpcore As coreClass) As pageContentModel
            Return createDefault(Of pageContentModel)(cpcore)
        End Function
    End Class
End Namespace
