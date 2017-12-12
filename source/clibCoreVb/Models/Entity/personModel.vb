
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class personModel
        Inherits baseModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "people"
        Public Const contentTableName As String = "ccmembers"
        Private Shadows Const contentDataSource As String = "default"
        '
        '====================================================================================================
        ' -- instance properties
        Public Property Address As String
        Public Property Address2 As String
        Public Property Admin As Boolean
        Public Property AdminMenuModeID As Integer
        Public Property AllowBulkEmail As Boolean
        Public Property AllowToolsPanel As Boolean
        Public Property AutoLogin As Boolean
        Public Property BillAddress As String
        Public Property BillAddress2 As String
        Public Property BillCity As String
        Public Property BillCompany As String
        Public Property BillCountry As String
        Public Property BillEmail As String
        Public Property BillFax As String
        Public Property BillName As String
        Public Property BillPhone As String
        Public Property BillState As String
        Public Property BillZip As String
        Public Property BirthdayDay As Integer
        Public Property BirthdayMonth As Integer
        Public Property BirthdayYear As Integer
        Public Property City As String
        Public Property Company As String
        Public Property Country As String
        Public Property CreatedByVisit As Boolean
        Public Property DateExpires As Date
        Public Property Developer As Boolean
        Public Property Email As String
        Public Property ExcludeFromAnalytics As Boolean
        Public Property Fax As String
        Public Property FirstName As String
        Public Property ImageFilename As String
        Public Property LanguageID As Integer
        Public Property LastName As String
        Public Property LastVisit As Date
        Public Property nickName As String
        Public Property NotesFilename As String
        Public Property OrganizationID As Integer
        Public Property Password As String
        Public Property Phone As String
        Public Property ResumeFilename As String
        Public Property ShipAddress As String
        Public Property ShipAddress2 As String
        Public Property ShipCity As String
        Public Property ShipCompany As String
        Public Property ShipCountry As String
        Public Property ShipName As String
        Public Property ShipPhone As String
        Public Property ShipState As String
        Public Property ShipZip As String
        Public Property State As String
        Public Property ThumbnailFilename As String
        Public Property Title As String
        Public Property Username As String
        Public Property Visits As Integer
        Public Property Zip As String
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass) As personModel
            Return add(Of personModel)(cpCore)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As personModel
            Return add(Of personModel)(cpCore, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer) As personModel
            Return create(Of personModel)(cpCore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As personModel
            Return create(Of personModel)(cpCore, recordId, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String) As personModel
            Return create(Of personModel)(cpCore, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As personModel
            Return create(Of personModel)(cpCore, recordGuid, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String) As personModel
            Return createByName(Of personModel)(cpCore, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As personModel
            Return createByName(Of personModel)(cpCore, recordName, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cpCore As coreClass)
            MyBase.save(cpCore)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, recordId As Integer)
            delete(Of personModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, ccGuid As String)
            delete(Of personModel)(cpCore, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String, callersCacheNameList As List(Of String)) As List(Of personModel)
            Return createList(Of personModel)(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String) As List(Of personModel)
            Return createList(Of personModel)(cpCore, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String) As List(Of personModel)
            Return createList(Of personModel)(cpCore, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
            invalidateCacheSingleRecord(Of personModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return baseModel.getRecordName(Of personModel)(cpcore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of personModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
            Return baseModel.getRecordId(Of personModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createDefault(cpcore As coreClass) As personModel
            Return createDefault(Of personModel)(cpcore)
        End Function
    End Class
End Namespace
