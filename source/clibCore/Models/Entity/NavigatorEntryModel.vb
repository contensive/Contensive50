
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class NavigatorEntryModel
        Inherits baseModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Navigator Entries"
        Public Const contentTableName As String = "ccMenuEntries"
        Private Shadows Const contentDataSource As String = "default"
        '
        '====================================================================================================
        ' -- instance properties
        Public Property ParentID As Integer
        Public Property NavIconTitle As String
        Public Property NavIconType As Integer
        Public Property AddonID As Integer
        Public Property AdminOnly As Boolean
        Public Property ContentID As Integer
        Public Property DeveloperOnly As Boolean
        Public Property EditArchive As Boolean
        Public Property EditBlank As Boolean
        Public Property EditSourceID As Integer
        Public Property HelpAddonID As Integer
        Public Property HelpCollectionID As Integer
        Public Property InstalledByCollectionID As Integer
        Public Property LinkPage As String
        Public Property NewWindow As Boolean
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass) As NavigatorEntryModel
            Return add(Of NavigatorEntryModel)(cpCore)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As NavigatorEntryModel
            Return add(Of NavigatorEntryModel)(cpCore, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer) As NavigatorEntryModel
            Return create(Of NavigatorEntryModel)(cpCore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As NavigatorEntryModel
            Return create(Of NavigatorEntryModel)(cpCore, recordId, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String) As NavigatorEntryModel
            Return create(Of NavigatorEntryModel)(cpCore, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As NavigatorEntryModel
            Return create(Of NavigatorEntryModel)(cpCore, recordGuid, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String) As NavigatorEntryModel
            Return createByName(Of NavigatorEntryModel)(cpCore, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As NavigatorEntryModel
            Return createByName(Of NavigatorEntryModel)(cpCore, recordName, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cpCore As coreClass)
            MyBase.save(cpCore)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, recordId As Integer)
            delete(Of NavigatorEntryModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, ccGuid As String)
            delete(Of NavigatorEntryModel)(cpCore, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String, callersCacheNameList As List(Of String)) As List(Of NavigatorEntryModel)
            Return createList(Of NavigatorEntryModel)(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String) As List(Of NavigatorEntryModel)
            Return createList(Of NavigatorEntryModel)(cpCore, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String) As List(Of NavigatorEntryModel)
            Return createList(Of NavigatorEntryModel)(cpCore, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
            invalidateCacheSingleRecord(Of NavigatorEntryModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return baseModel.getRecordName(Of NavigatorEntryModel)(cpcore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of NavigatorEntryModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
            Return baseModel.getRecordId(Of NavigatorEntryModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createDefault(cpcore As coreClass) As NavigatorEntryModel
            Return createDefault(Of NavigatorEntryModel)(cpcore)
        End Function
    End Class
End Namespace
