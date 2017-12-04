
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class addonContentFieldTypeRulesModel
        Inherits baseModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "add-on Content Field Type Rules"
        Public Const contentTableName As String = "ccAddonContentFieldTypeRules"
        Private Shadows Const contentDataSource As String = "default"
        '
        '====================================================================================================
        ' -- instance properties
        Public Property addonID As Integer
        Public Property contentFieldTypeID As Integer
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass) As addonContentFieldTypeRulesModel
            Return add(Of addonContentFieldTypeRulesModel)(cpCore)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As addonContentFieldTypeRulesModel
            Return add(Of addonContentFieldTypeRulesModel)(cpCore, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer) As addonContentFieldTypeRulesModel
            Return create(Of addonContentFieldTypeRulesModel)(cpCore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As addonContentFieldTypeRulesModel
            Return create(Of addonContentFieldTypeRulesModel)(cpCore, recordId, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String) As addonContentFieldTypeRulesModel
            Return create(Of addonContentFieldTypeRulesModel)(cpCore, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As addonContentFieldTypeRulesModel
            Return create(Of addonContentFieldTypeRulesModel)(cpCore, recordGuid, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String) As addonContentFieldTypeRulesModel
            Return createByName(Of addonContentFieldTypeRulesModel)(cpCore, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As addonContentFieldTypeRulesModel
            Return createByName(Of addonContentFieldTypeRulesModel)(cpCore, recordName, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cpCore As coreClass)
            MyBase.save(cpCore)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, recordId As Integer)
            delete(Of addonContentFieldTypeRulesModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, ccGuid As String)
            delete(Of addonContentFieldTypeRulesModel)(cpCore, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String, callersCacheNameList As List(Of String)) As List(Of addonContentFieldTypeRulesModel)
            Return createList(Of addonContentFieldTypeRulesModel)(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String) As List(Of addonContentFieldTypeRulesModel)
            Return createList(Of addonContentFieldTypeRulesModel)(cpCore, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String) As List(Of addonContentFieldTypeRulesModel)
            Return createList(Of addonContentFieldTypeRulesModel)(cpCore, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
            invalidateCacheSingleRecord(Of addonContentFieldTypeRulesModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return baseModel.getRecordName(Of addonContentFieldTypeRulesModel)(cpcore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of addonContentFieldTypeRulesModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
            Return baseModel.getRecordId(Of addonContentFieldTypeRulesModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createDefault(cpcore As coreClass) As addonContentFieldTypeRulesModel
            Return createDefault(Of addonContentFieldTypeRulesModel)(cpcore)
        End Function
    End Class
End Namespace
