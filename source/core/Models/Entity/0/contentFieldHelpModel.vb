
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class ContentFieldHelpModel
        Inherits baseModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "content field help"
        Public Const contentTableName As String = "ccFieldHelp"
        Private Shadows Const contentDataSource As String = "default"
        '
        '====================================================================================================
        ' -- instance properties
        Public Property FieldID As Integer
        Public Property HelpCustom As String
        Public Property HelpDefault As String
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass) As ContentFieldHelpModel
            Return add(Of ContentFieldHelpModel)(cpCore)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As ContentFieldHelpModel
            Return add(Of ContentFieldHelpModel)(cpCore, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer) As ContentFieldHelpModel
            Return create(Of ContentFieldHelpModel)(cpCore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As ContentFieldHelpModel
            Return create(Of ContentFieldHelpModel)(cpCore, recordId, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String) As ContentFieldHelpModel
            Return create(Of ContentFieldHelpModel)(cpCore, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As ContentFieldHelpModel
            Return create(Of ContentFieldHelpModel)(cpCore, recordGuid, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String) As ContentFieldHelpModel
            Return createByName(Of ContentFieldHelpModel)(cpCore, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As ContentFieldHelpModel
            Return createByName(Of ContentFieldHelpModel)(cpCore, recordName, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cpCore As coreClass)
            MyBase.save(cpCore)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, recordId As Integer)
            delete(Of ContentFieldHelpModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, ccGuid As String)
            delete(Of ContentFieldHelpModel)(cpCore, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String, callersCacheNameList As List(Of String)) As List(Of ContentFieldHelpModel)
            Return createList(Of ContentFieldHelpModel)(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String) As List(Of ContentFieldHelpModel)
            Return createList(Of ContentFieldHelpModel)(cpCore, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String) As List(Of ContentFieldHelpModel)
            Return createList(Of ContentFieldHelpModel)(cpCore, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
            invalidateCacheSingleRecord(Of ContentFieldHelpModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return baseModel.getRecordName(Of ContentFieldHelpModel)(cpcore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of ContentFieldHelpModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
            Return baseModel.getRecordId(Of ContentFieldHelpModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createDefault(cpcore As coreClass) As ContentFieldHelpModel
            Return createDefault(Of ContentFieldHelpModel)(cpcore)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get the first field help for a field, digard the rest
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="fieldId"></param>
        ''' <returns></returns>
        Public Shared Function createByFieldId(cpCore As coreClass, fieldId As Integer) As ContentFieldHelpModel
            Dim helpList As List(Of ContentFieldHelpModel) = createList(cpCore, "(fieldId=" & fieldId & ")", "id")
            If (helpList.Count = 0) Then
                Return Nothing
            Else
                Return helpList.First
            End If
        End Function
    End Class
End Namespace
