
'Option Explicit On
'Option Strict On

'Imports System
'Imports System.Collections.Generic
'Imports System.Text
'Imports Contensive.BaseClasses
'Imports Contensive.Core.Controllers
'Imports Newtonsoft.Json

'Namespace Contensive.Core.Models.Entity
'    Public Class xSharedStylesIncludeRuleModel
'        Inherits baseModel
'        '
'        '====================================================================================================
'        '-- const
'        Public Const contentName As String = "SharedStylesIncludeRules"
'        Public Const contentTableName As String = "ccSharedStylesIncludeRules"
'        Private Shadows Const contentDataSource As String = "default"
'        '
'        '====================================================================================================
'        ' -- instance properties
'        Public Property IncludedStyleID As Integer
'        Public Property StyleID As Integer
'        '
'        '====================================================================================================
'        Public Overloads Shared Function add(cpCore As coreClass) As _blankModel
'            Return add(Of _blankModel)(cpCore)
'        End Function
'        '
'        '====================================================================================================
'        Public Overloads Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As _blankModel
'            Return add(Of _blankModel)(cpCore, callersCacheNameList)
'        End Function
'        '
'        '====================================================================================================
'        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer) As _blankModel
'            Return create(Of _blankModel)(cpCore, recordId)
'        End Function
'        '
'        '====================================================================================================
'        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As _blankModel
'            Return create(Of _blankModel)(cpCore, recordId, callersCacheNameList)
'        End Function
'        '
'        '====================================================================================================
'        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String) As _blankModel
'            Return create(Of _blankModel)(cpCore, recordGuid)
'        End Function
'        '
'        '====================================================================================================
'        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As _blankModel
'            Return create(Of _blankModel)(cpCore, recordGuid, callersCacheNameList)
'        End Function
'        '
'        '====================================================================================================
'        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String) As _blankModel
'            Return createByName(Of _blankModel)(cpCore, recordName)
'        End Function
'        '
'        '====================================================================================================
'        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As _blankModel
'            Return createByName(Of _blankModel)(cpCore, recordName, callersCacheNameList)
'        End Function
'        '
'        '====================================================================================================
'        Public Overloads Sub save(cpCore As coreClass)
'            MyBase.save(cpCore)
'        End Sub
'        '
'        '====================================================================================================
'        Public Overloads Shared Sub delete(cpCore As coreClass, recordId As Integer)
'            delete(Of _blankModel)(cpCore, recordId)
'        End Sub
'        '
'        '====================================================================================================
'        Public Overloads Shared Sub delete(cpCore As coreClass, ccGuid As String)
'            delete(Of _blankModel)(cpCore, ccGuid)
'        End Sub
'        '
'        '====================================================================================================
'        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String, callersCacheNameList As List(Of String)) As List(Of _blankModel)
'            Return createList(Of _blankModel)(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList)
'        End Function
'        '
'        '====================================================================================================
'        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String) As List(Of _blankModel)
'            Return createList(Of _blankModel)(cpCore, sqlCriteria, sqlOrderBy)
'        End Function
'        '
'        '====================================================================================================
'        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String) As List(Of _blankModel)
'            Return createList(Of _blankModel)(cpCore, sqlCriteria)
'        End Function
'        '
'        '====================================================================================================
'        Public Overloads Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
'            invalidateCacheSingleRecord(Of _blankModel)(cpCore, recordId)
'        End Sub
'        '
'        '====================================================================================================
'        Public Overloads Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
'            Return baseModel.getRecordName(Of _blankModel)(cpcore, recordId)
'        End Function
'        '
'        '====================================================================================================
'        Public Overloads Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
'            Return baseModel.getRecordName(Of _blankModel)(cpcore, ccGuid)
'        End Function
'        '
'        '====================================================================================================
'        Public Overloads Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
'            Return baseModel.getRecordId(Of _blankModel)(cpcore, ccGuid)
'        End Function
'        '
'        '====================================================================================================
'        Public Overloads Shared Function createDefault(cpcore As coreClass) As _blankModel
'            Return createDefault(Of _blankModel)(cpcore)
'        End Function
'    End Class
'End Namespace
