
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class linkForwardModel
        Inherits baseModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "link forwards"
        Public Const contentTableName As String = "cclinkforwards"
        Private Shadows Const contentDataSource As String = "default"
        '
        '====================================================================================================
        ' -- instance properties
        Public DestinationLink As String
        Public GroupID As Integer
        Public SourceLink As String
        Public Viewings As Integer
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass) As linkForwardModel
            Return add(Of linkForwardModel)(cpCore)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As linkForwardModel
            Return add(Of linkForwardModel)(cpCore, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer) As linkForwardModel
            Return create(Of linkForwardModel)(cpCore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As linkForwardModel
            Return create(Of linkForwardModel)(cpCore, recordId, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String) As linkForwardModel
            Return create(Of linkForwardModel)(cpCore, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As linkForwardModel
            Return create(Of linkForwardModel)(cpCore, recordGuid, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String) As linkForwardModel
            Return createByName(Of linkForwardModel)(cpCore, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As linkForwardModel
            Return createByName(Of linkForwardModel)(cpCore, recordName, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cpCore As coreClass)
            MyBase.save(cpCore)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, recordId As Integer)
            delete(Of linkForwardModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, ccGuid As String)
            delete(Of linkForwardModel)(cpCore, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String, callersCacheNameList As List(Of String)) As List(Of linkForwardModel)
            Return createList(Of linkForwardModel)(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String) As List(Of linkForwardModel)
            Return createList(Of linkForwardModel)(cpCore, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String) As List(Of linkForwardModel)
            Return createList(Of linkForwardModel)(cpCore, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
            invalidateCacheSingleRecord(Of linkForwardModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return baseModel.getRecordName(Of linkForwardModel)(cpcore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of linkForwardModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
            Return baseModel.getRecordId(Of linkForwardModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createDefault(cpcore As coreClass) As linkForwardModel
            Return createDefault(Of linkForwardModel)(cpcore)
        End Function
    End Class
End Namespace
