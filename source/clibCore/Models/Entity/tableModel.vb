
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class tableModel
        Inherits baseModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "tables" '<------ set content name
        Public Const contentTableName As String = "ccTables" '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default" '<----- set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties
        Public Property DataSourceID As Integer
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass) As tableModel
            Return add(Of tableModel)(cpCore)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As tableModel
            Return add(Of tableModel)(cpCore, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer) As tableModel
            Return create(Of tableModel)(cpCore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As tableModel
            Return create(Of tableModel)(cpCore, recordId, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Function create(cpCore As coreClass, recordGuid As String) As tableModel
            Return MyBase.create(Of tableModel)(cpCore, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As tableModel
            Return MyBase.create(Of tableModel)(cpCore, recordGuid, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String) As tableModel
            Return createByName(Of tableModel)(cpCore, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As tableModel
            Return createByName(Of tableModel)(cpCore, recordName, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cpCore As coreClass)
            MyBase.save(cpCore)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, recordId As Integer)
            delete(Of tableModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, ccGuid As String)
            delete(Of tableModel)(cpCore, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, callersCacheNameList As List(Of String)) As List(Of tableModel)
            Return createList(Of tableModel)(cpCore, sqlCriteria, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String) As List(Of tableModel)
            Return createList(Of tableModel)(cpCore, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
            invalidatePrimaryCache(Of tableModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return baseModel.getRecordName(Of tableModel)(cpcore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of tableModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
            Return baseModel.getRecordId(Of tableModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createDefault(cpcore As coreClass) As tableModel
            Return createDefault(Of tableModel)(cpcore)
        End Function
    End Class
End Namespace
