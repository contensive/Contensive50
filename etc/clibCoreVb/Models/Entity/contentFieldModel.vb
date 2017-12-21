
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class contentFieldModel
        Inherits baseModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "content fields"
        Public Const contentTableName As String = "ccfields"
        Private Shadows Const contentDataSource As String = "default"
        '
        '====================================================================================================
        ' -- instance properties
        Public Property AdminOnly As Boolean
        Public Property Authorable As Boolean
        Public Property Caption As String
        Public Property ContentID As Integer
        Public Property createResourceFilesOnRoot As Boolean
        Public Property DefaultValue As String
        Public Property DeveloperOnly As Boolean
        Public Property editorAddonID As Integer
        Public Property EditSortPriority As Integer
        Public Property EditTab As String
        Public Property HTMLContent As Boolean
        Public Property IndexColumn As Integer
        Public Property IndexSortDirection As Integer
        Public Property IndexSortPriority As Integer
        Public Property IndexWidth As String
        Public Property InstalledByCollectionID As Integer
        'Public Property IsBaseField As Boolean
        Public Property LookupContentID As Integer
        Public Property LookupList As String
        Public Property ManyToManyContentID As Integer
        Public Property ManyToManyRuleContentID As Integer
        Public Property ManyToManyRulePrimaryField As String
        Public Property ManyToManyRuleSecondaryField As String
        Public Property MemberSelectGroupID As Integer
        Public Property NotEditable As Boolean
        Public Property Password As Boolean
        Public Property prefixForRootResourceFiles As String
        Public Property [ReadOnly] As Boolean
        Public Property RedirectContentID As Integer
        Public Property RedirectID As String
        Public Property RedirectPath As String
        Public Property Required As Boolean
        Public Property RSSDescriptionField As Boolean
        Public Property RSSTitleField As Boolean
        Public Property Scramble As Boolean
        Public Property TextBuffered As Boolean
        Public Property Type As Integer
        Public Property UniqueName As Boolean
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass) As contentFieldModel
            Return add(Of contentFieldModel)(cpCore)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As contentFieldModel
            Return add(Of contentFieldModel)(cpCore, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer) As contentFieldModel
            Return create(Of contentFieldModel)(cpCore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As contentFieldModel
            Return create(Of contentFieldModel)(cpCore, recordId, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String) As contentFieldModel
            Return create(Of contentFieldModel)(cpCore, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As contentFieldModel
            Return create(Of contentFieldModel)(cpCore, recordGuid, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String) As contentFieldModel
            Return createByName(Of contentFieldModel)(cpCore, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As contentFieldModel
            Return createByName(Of contentFieldModel)(cpCore, recordName, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cpCore As coreClass)
            MyBase.save(cpCore)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, recordId As Integer)
            delete(Of contentFieldModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, ccGuid As String)
            delete(Of contentFieldModel)(cpCore, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String, callersCacheNameList As List(Of String)) As List(Of contentFieldModel)
            Return createList(Of contentFieldModel)(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String) As List(Of contentFieldModel)
            Return createList(Of contentFieldModel)(cpCore, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String) As List(Of contentFieldModel)
            Return createList(Of contentFieldModel)(cpCore, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
            invalidateCacheSingleRecord(Of contentFieldModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return baseModel.getRecordName(Of contentFieldModel)(cpcore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of contentFieldModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
            Return baseModel.getRecordId(Of contentFieldModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createDefault(cpcore As coreClass) As contentFieldModel
            Return createDefault(Of contentFieldModel)(cpcore)
        End Function
    End Class
End Namespace
