
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class contentModel
        Inherits baseModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "content"
        Public Const contentTableName As String = "cccontent"
        Private Shadows Const contentDataSource As String = "default"
        '
        '====================================================================================================
        ' -- instance properties
        Public Property AdminOnly As Boolean
        Public Property AllowAdd As Boolean
        Public Property AllowContentChildTool As Boolean
        Public Property AllowContentTracking As Boolean
        Public Property AllowDelete As Boolean
        Public Property AllowTopicRules As Boolean
        Public Property AllowWorkflowAuthoring As Boolean
        Public Property AuthoringTableID As Integer
        Public Property ContentTableID As Integer
        Public Property DefaultSortMethodID As Integer
        Public Property DeveloperOnly As Boolean
        Public Property DropDownFieldList As String
        Public Property EditArchive As Boolean
        Public Property EditBlank As Boolean
        Public Property EditorGroupID As Integer
        Public Property EditSourceID As Integer
        Public Property IconHeight As Integer
        Public Property IconLink As String
        Public Property IconSprites As Integer
        Public Property IconWidth As Integer
        Public Property InstalledByCollectionID As Integer
        Public Property IsBaseContent As Boolean
        Public Property ParentID As Integer
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass) As contentModel
            Return add(Of contentModel)(cpCore)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As contentModel
            Return add(Of contentModel)(cpCore, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer) As contentModel
            Return create(Of contentModel)(cpCore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As contentModel
            Return create(Of contentModel)(cpCore, recordId, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String) As contentModel
            Return create(Of contentModel)(cpCore, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As contentModel
            Return create(Of contentModel)(cpCore, recordGuid, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String) As contentModel
            Return createByName(Of contentModel)(cpCore, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As contentModel
            Return createByName(Of contentModel)(cpCore, recordName, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cpCore As coreClass)
            MyBase.save(cpCore)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, recordId As Integer)
            delete(Of contentModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, ccGuid As String)
            delete(Of contentModel)(cpCore, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String, callersCacheNameList As List(Of String)) As List(Of contentModel)
            Return createList(Of contentModel)(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String) As List(Of contentModel)
            Return createList(Of contentModel)(cpCore, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String) As List(Of contentModel)
            Return createList(Of contentModel)(cpCore, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
            invalidateCacheSingleRecord(Of contentModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return baseModel.getRecordName(Of contentModel)(cpcore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of contentModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
            Return baseModel.getRecordId(Of contentModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createDefault(cpcore As coreClass) As contentModel
            Return createDefault(Of contentModel)(cpcore)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' pattern get a list of objects from this model
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="someCriteria"></param>
        ''' <returns></returns>
        Public Shared Function createDict(cpCore As coreClass, callersCacheNameList As List(Of String)) As Dictionary(Of Integer, contentModel)
            Dim result As New Dictionary(Of Integer, contentModel)
            Try
                For Each content As contentModel In createList(cpCore, "")
                    result.Add(content.id, content)
                Next
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
    End Class
End Namespace
