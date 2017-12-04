
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class libraryFilesModel
        Inherits baseModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "library Files"
        Public Const contentTableName As String = "cclibraryFiles"
        Private Shadows Const contentDataSource As String = "default"
        '
        '====================================================================================================
        ' -- instance properties
        Public Property AltSizeList As String
        Public Property AltText As String
        Public Property Clicks As Integer
        Public Property Description As String
        Public Property Filename As String
        Public Property FileSize As Integer
        Public Property FileTypeID As Integer
        Public Property FolderID As Integer
        Public Property pxHeight As Integer
        Public Property pxWidth As Integer
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass) As libraryFilesModel
            Return add(Of libraryFilesModel)(cpCore)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As libraryFilesModel
            Return add(Of libraryFilesModel)(cpCore, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer) As libraryFilesModel
            Return create(Of libraryFilesModel)(cpCore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As libraryFilesModel
            Return create(Of libraryFilesModel)(cpCore, recordId, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String) As libraryFilesModel
            Return create(Of libraryFilesModel)(cpCore, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As libraryFilesModel
            Return create(Of libraryFilesModel)(cpCore, recordGuid, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String) As libraryFilesModel
            Return createByName(Of libraryFilesModel)(cpCore, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As libraryFilesModel
            Return createByName(Of libraryFilesModel)(cpCore, recordName, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cpCore As coreClass)
            MyBase.save(cpCore)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, recordId As Integer)
            delete(Of libraryFilesModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, ccGuid As String)
            delete(Of libraryFilesModel)(cpCore, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String, callersCacheNameList As List(Of String)) As List(Of libraryFilesModel)
            Return createList(Of libraryFilesModel)(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String) As List(Of libraryFilesModel)
            Return createList(Of libraryFilesModel)(cpCore, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String) As List(Of libraryFilesModel)
            Return createList(Of libraryFilesModel)(cpCore, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
            invalidateCacheSingleRecord(Of libraryFilesModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return baseModel.getRecordName(Of libraryFilesModel)(cpcore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of libraryFilesModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
            Return baseModel.getRecordId(Of libraryFilesModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createDefault(cpcore As coreClass) As libraryFilesModel
            Return createDefault(Of libraryFilesModel)(cpcore)
        End Function
    End Class
End Namespace
