
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class addonIncludeRuleModel
        Inherits baseModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Add-on Include Rules"
        Public Const contentTableName As String = "ccAddonIncludeRules"
        Private Shadows Const contentDataSource As String = "default"
        '
        '====================================================================================================
        ' -- instance properties
        Public Property AddonID As Integer
        Public Property IncludedAddonID As Integer
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass) As addonIncludeRuleModel
            Return add(Of addonIncludeRuleModel)(cpCore)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As addonIncludeRuleModel
            Return add(Of addonIncludeRuleModel)(cpCore, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer) As addonIncludeRuleModel
            Return create(Of addonIncludeRuleModel)(cpCore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As addonIncludeRuleModel
            Return create(Of addonIncludeRuleModel)(cpCore, recordId, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String) As addonIncludeRuleModel
            Return create(Of addonIncludeRuleModel)(cpCore, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As addonIncludeRuleModel
            Return create(Of addonIncludeRuleModel)(cpCore, recordGuid, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String) As addonIncludeRuleModel
            Return createByName(Of addonIncludeRuleModel)(cpCore, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As addonIncludeRuleModel
            Return createByName(Of addonIncludeRuleModel)(cpCore, recordName, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cpCore As coreClass)
            MyBase.save(cpCore)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, recordId As Integer)
            delete(Of addonIncludeRuleModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, ccGuid As String)
            delete(Of addonIncludeRuleModel)(cpCore, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String, callersCacheNameList As List(Of String)) As List(Of addonIncludeRuleModel)
            Return createList(Of addonIncludeRuleModel)(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String) As List(Of addonIncludeRuleModel)
            Return createList(Of addonIncludeRuleModel)(cpCore, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String) As List(Of addonIncludeRuleModel)
            Return createList(Of addonIncludeRuleModel)(cpCore, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
            invalidateCacheSingleRecord(Of addonIncludeRuleModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return baseModel.getRecordName(Of addonIncludeRuleModel)(cpcore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of addonIncludeRuleModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
            Return baseModel.getRecordId(Of addonIncludeRuleModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createDefault(cpcore As coreClass) As addonIncludeRuleModel
            Return createDefault(Of addonIncludeRuleModel)(cpcore)
        End Function
    End Class
End Namespace
