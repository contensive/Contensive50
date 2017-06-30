
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class linkAliasModel
        Inherits baseModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "link aliases"
        Public Const contentTableName As String = "cclinkaliases"
        Private Shadows Const contentDataSource As String = "default"
        '
        '====================================================================================================
        ' -- instance properties
        Public Property Link As String
        Public Property PageID As Integer
        Public Property QueryStringSuffix As String
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass) As linkAliasModel
            Return add(Of linkAliasModel)(cpCore)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As linkAliasModel
            Return add(Of linkAliasModel)(cpCore, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer) As linkAliasModel
            Return create(Of linkAliasModel)(cpCore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As linkAliasModel
            Return create(Of linkAliasModel)(cpCore, recordId, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String) As linkAliasModel
            Return create(Of linkAliasModel)(cpCore, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As linkAliasModel
            Return create(Of linkAliasModel)(cpCore, recordGuid, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String) As linkAliasModel
            Return createByName(Of linkAliasModel)(cpCore, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As linkAliasModel
            Return createByName(Of linkAliasModel)(cpCore, recordName, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cpCore As coreClass)
            MyBase.save(cpCore)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, recordId As Integer)
            delete(Of linkAliasModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, ccGuid As String)
            delete(Of linkAliasModel)(cpCore, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String, callersCacheNameList As List(Of String)) As List(Of linkAliasModel)
            Return createList(Of linkAliasModel)(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String) As List(Of linkAliasModel)
            Return createList(Of linkAliasModel)(cpCore, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String) As List(Of linkAliasModel)
            Return createList(Of linkAliasModel)(cpCore, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
            invalidateCacheSingleRecord(Of linkAliasModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return baseModel.getRecordName(Of linkAliasModel)(cpcore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of linkAliasModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
            Return baseModel.getRecordId(Of linkAliasModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createDefault(cpcore As coreClass) As linkAliasModel
            Return createDefault(Of linkAliasModel)(cpcore)
        End Function
    End Class
End Namespace
