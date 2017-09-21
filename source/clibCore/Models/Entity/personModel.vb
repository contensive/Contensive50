
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class personModel
        '
        '-- const
        Public Const primaryContentName As String = "people" '<------ set content name
        Private Const primaryContentTableName As String = "ccmembers" '<------ set to tablename for the primary content (used for cache names)
        Private Const primaryContentDataSource As String = "default" '<----- set to datasource if not default
        '
        ' -- instance properties
        Public id As Integer
        Public Active As Boolean
        Public Address As String
        Public Address2 As String
        Public Admin As Boolean
        Public AdminMenuModeID As Integer
        Public AllowBulkEmail As Boolean
        Public AllowToolsPanel As Boolean
        Public AutoLogin As Boolean
        Public BillAddress As String
        Public BillAddress2 As String
        Public BillCity As String
        Public BillCompany As String
        Public BillCountry As String
        Public BillEmail As String
        Public BillFax As String
        Public BillName As String
        Public BillPhone As String
        Public BillState As String
        Public BillZip As String
        Public BirthdayDay As Integer
        Public BirthdayMonth As Integer
        Public BirthdayYear As Integer
        Public ccGuid As String
        Public City As String
        Public Company As String
        
        Public ContentControlID As Integer
        Public Country As String
        Public CreatedBy As Integer
        Public CreatedByVisit As Boolean
        Public CreateKey As Integer
        Public DateAdded As Date
        Public DateExpires As Date
        Public Developer As Boolean
        
        
        
        Public Email As String
        Public ExcludeFromAnalytics As Boolean
        Public Fax As String
        Public FirstName As String
        Public ImageFilename As String
        Public LanguageID As Integer
        Public LastName As String
        Public LastVisit As Date
        Public ModifiedBy As Integer
        Public ModifiedDate As Date
        Public Name As String
        Public nickName As String
        Public NotesFilename As String
        Public OrganizationID As Integer
        Public Password As String
        Public Phone As String
        Public ResumeFilename As String
        Public ShipAddress As String
        Public ShipAddress2 As String
        Public ShipCity As String
        Public ShipCompany As String
        Public ShipCountry As String
        Public ShipName As String
        Public ShipPhone As String
        Public ShipState As String
        Public ShipZip As String
        Public SortOrder As String
        Public State As String
        'Public StyleFilename As String
        Public ThumbnailFilename As String
        Public Title As String
        Public Username As String
        Public Visits As Integer
        Public Zip As String
        '
        Public language As String ' REFACTOR - remove if this is not used
        '
        '====================================================================================================
        ''' <summary>
        ''' Create an empty object. needed for deserialization
        ''' </summary>
        Public Sub New()
            '
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' add a new recod to the db and open it. Starting a new model with this method will use the default
        ''' values in Contensive metadata (active, contentcontrolid, etc)
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="cacheNameList"></param>
        ''' <returns></returns>
        Public Shared Function add(cpCore As coreClass, ByRef cacheNameList As List(Of String)) As personModel
            Dim result As personModel = Nothing
            Try
                result = create(cpCore, cpCore.db.metaData_InsertContentRecordGetID(primaryContentName, 0), cacheNameList)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId">The id of the record to be read into the new object</param>
        ''' <param name="cacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
        Public Shared Function create(cpCore As coreClass, recordId As Integer, ByRef cacheNameList As List(Of String)) As personModel
            Dim result As personModel = Nothing
            Try
                If recordId > 0 Then
                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", recordId.ToString())
                    result = cpCore.cache.getObject(Of personModel)(cacheName)
                    If (result Is Nothing) Then
                        result = loadObject(cpCore, "id=" & recordId.ToString(), cacheNameList)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' open an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordGuid"></param>
        Public Shared Function create(cpCore As coreClass, recordGuid As String, ByRef cacheNameList As List(Of String)) As personModel
            Dim result As personModel = Nothing
            Try
                If Not String.IsNullOrEmpty(recordGuid) Then
                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "ccguid", recordGuid)
                    result = cpCore.cache.getObject(Of personModel)(cacheName)
                    If (result Is Nothing) Then
                        result = loadObject(cpCore, "ccGuid=" & cpCore.db.encodeSQLText(recordGuid), cacheNameList)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' open an existing object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="sqlCriteria"></param>
        Private Shared Function loadObject(cpCore As coreClass, sqlCriteria As String, ByRef cacheNameList As List(Of String)) As personModel
            Dim result As personModel = Nothing
            Try
                Dim cs As New csController(cpCore)
                If cs.open(primaryContentName, sqlCriteria) Then
                    result = New personModel
                    With result
                        '
                        ' -- populate result model
                        .id = cs.getInteger("ID")
                        .Active = cs.getBoolean("Active")
                        .Address = cs.getText("Address")
                        .Address2 = cs.getText("Address2")
                        .Admin = cs.getBoolean("Admin")
                        .AdminMenuModeID = cs.getInteger("AdminMenuModeID")
                        .AllowBulkEmail = cs.getBoolean("AllowBulkEmail")
                        .AllowToolsPanel = cs.getBoolean("AllowToolsPanel")
                        .AutoLogin = cs.getBoolean("AutoLogin")
                        .BillAddress = cs.getText("BillAddress")
                        .BillAddress2 = cs.getText("BillAddress2")
                        .BillCity = cs.getText("BillCity")
                        .BillCompany = cs.getText("BillCompany")
                        .BillCountry = cs.getText("BillCountry")
                        .BillEmail = cs.getText("BillEmail")
                        .BillFax = cs.getText("BillFax")
                        .BillName = cs.getText("BillName")
                        .BillPhone = cs.getText("BillPhone")
                        .BillState = cs.getText("BillState")
                        .BillZip = cs.getText("BillZip")
                        .BirthdayDay = cs.getInteger("BirthdayDay")
                        .BirthdayMonth = cs.getInteger("BirthdayMonth")
                        .BirthdayYear = cs.getInteger("BirthdayYear")
                        .ccGuid = cs.getText("ccGuid")
                        .City = cs.getText("City")
                        .Company = cs.getText("Company")
                        '
                        .ContentControlID = cs.getInteger("ContentControlID")
                        .Country = cs.getText("Country")
                        .CreatedBy = cs.getInteger("CreatedBy")
                        .CreatedByVisit = cs.getBoolean("CreatedByVisit")
                        .CreateKey = cs.getInteger("CreateKey")
                        .DateAdded = cs.getDate("DateAdded")
                        .DateExpires = cs.getDate("DateExpires")
                        .Developer = cs.getBoolean("Developer")
                        '
                        '
                        '
                        .Email = cs.getText("Email")
                        .ExcludeFromAnalytics = cs.getBoolean("ExcludeFromAnalytics")
                        .Fax = cs.getText("Fax")
                        .FirstName = cs.getText("FirstName")
                        .ImageFilename = cs.getText("ImageFilename")
                        .LanguageID = cs.getInteger("LanguageID")
                        .LastName = cs.getText("LastName")
                        .LastVisit = cs.getDate("LastVisit")
                        .ModifiedBy = cs.getInteger("ModifiedBy")
                        .ModifiedDate = cs.getDate("ModifiedDate")
                        .Name = cs.getText("Name")
                        .nickName = cs.getText("nickName")
                        .NotesFilename = cs.getText("NotesFilename")
                        .OrganizationID = cs.getInteger("OrganizationID")
                        .Password = cs.getText("Password")
                        .Phone = cs.getText("Phone")
                        .ResumeFilename = cs.getText("ResumeFilename")
                        .ShipAddress = cs.getText("ShipAddress")
                        .ShipAddress2 = cs.getText("ShipAddress2")
                        .ShipCity = cs.getText("ShipCity")
                        .ShipCompany = cs.getText("ShipCompany")
                        .ShipCountry = cs.getText("ShipCountry")
                        .ShipName = cs.getText("ShipName")
                        .ShipPhone = cs.getText("ShipPhone")
                        .ShipState = cs.getText("ShipState")
                        .ShipZip = cs.getText("ShipZip")
                        .SortOrder = cs.getText("SortOrder")
                        .State = cs.getText("State")
                        '.StyleFilename = cs.getText("StyleFilename")
                        .ThumbnailFilename = cs.getText("ThumbnailFilename")
                        .Title = cs.getText("Title")
                        .Username = cs.getText("Username")
                        .Visits = cs.getInteger("Visits")
                        .Zip = cs.getText("Zip")
                        If (String.IsNullOrEmpty(.ccGuid)) Then .ccGuid = Controllers.genericController.getGUID() End With
                    If (result IsNot Nothing) Then
                        '
                        ' -- set primary and secondary caches
                        ' -- add all cachenames to the injected cachenamelist
                        Dim cacheName0 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", result.id.ToString())
                        cacheNameList.Add(cacheName0)
                        cpCore.cache.setObject(cacheName0, result)
                        '
                        Dim cacheName1 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "ccguid", result.ccGuid)
                        cacheNameList.Add(cacheName1)
                        cpCore.cache.setSecondaryObject(cacheName1, cacheName0)
                    End If
                End If
                Call cs.Close()
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' save the instance properties to a record with matching id. If id is not provided, a new record is created.
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <returns></returns>
        Public Function saveObject(cpCore As coreClass) As Integer
            Try
                Dim cs As New csController(cpCore)
                If (id > 0) Then
                    If Not cs.open(primaryContentName, "id=" & id) Then
                        id = 0
                        cs.Close()
                        Throw New ApplicationException("Unable to open record in content [" & primaryContentName & "], with id [" & id & "]")
                    End If
                Else
                    If Not cs.Insert(primaryContentName) Then
                        cs.Close()
                        id = 0
                        Throw New ApplicationException("Unable to insert record in content [" & primaryContentName & "]")
                    End If
                End If
                If cs.ok() Then
                    id = cs.getInteger("id")
                    If (String.IsNullOrEmpty(ccGuid)) Then
                        ccGuid = Controllers.genericController.getGUID()
                    End If
                    cs.setField("Active", Active.ToString())
                    cs.SetField("Address", Address)
                    cs.SetField("Address2", Address2)
                    cs.SetField("Admin", Admin.ToString())
                    cs.SetField("AdminMenuModeID", AdminMenuModeID.ToString())
                    cs.SetField("AllowBulkEmail", AllowBulkEmail.ToString())
                    cs.SetField("AllowToolsPanel", AllowToolsPanel.ToString())
                    cs.SetField("AutoLogin", AutoLogin.ToString())
                    cs.SetField("BillAddress", BillAddress)
                    cs.SetField("BillAddress2", BillAddress2)
                    cs.SetField("BillCity", BillCity)
                    cs.SetField("BillCompany", BillCompany)
                    cs.SetField("BillCountry", BillCountry)
                    cs.SetField("BillEmail", BillEmail)
                    cs.SetField("BillFax", BillFax)
                    cs.SetField("BillName", BillName)
                    cs.SetField("BillPhone", BillPhone)
                    cs.SetField("BillState", BillState)
                    cs.SetField("BillZip", BillZip)
                    cs.SetField("BirthdayDay", BirthdayDay.ToString())
                    cs.SetField("BirthdayMonth", BirthdayMonth.ToString())
                    cs.SetField("BirthdayYear", BirthdayYear.ToString())
                    cs.SetField("ccGuid", ccGuid)
                    cs.SetField("City", City)
                    cs.SetField("Company", Company)
                    '
                    cs.setField("ContentControlID", ContentControlID.ToString())
                    cs.setField("Country", Country)
                    cs.setField("CreatedBy", CreatedBy.ToString())
                    cs.setField("CreatedByVisit", CreatedByVisit.ToString())
                    cs.setField("CreateKey", CreateKey.ToString())
                    cs.setField("DateAdded", DateAdded.ToString())
                    cs.setField("DateExpires", DateExpires.ToString())
                    cs.setField("Developer", Developer.ToString())
                    '
                    '
                    '
                    cs.SetField("Email", Email)
                    cs.SetField("ExcludeFromAnalytics", ExcludeFromAnalytics.ToString())
                    cs.SetField("Fax", Fax)
                    cs.SetField("FirstName", FirstName)
                    cs.SetField("ImageFilename", ImageFilename)
                    cs.SetField("LanguageID", LanguageID.ToString())
                    cs.SetField("LastName", LastName)
                    cs.SetField("LastVisit", LastVisit.ToString())
                    cs.SetField("ModifiedBy", ModifiedBy.ToString())
                    cs.SetField("ModifiedDate", ModifiedDate.ToString())
                    cs.SetField("Name", Name)
                    cs.SetField("nickName", nickName)
                    cs.SetField("NotesFilename", NotesFilename)
                    cs.SetField("OrganizationID", OrganizationID.ToString())
                    cs.SetField("Password", Password)
                    cs.SetField("Phone", Phone)
                    cs.SetField("ResumeFilename", ResumeFilename)
                    cs.SetField("ShipAddress", ShipAddress)
                    cs.SetField("ShipAddress2", ShipAddress2)
                    cs.SetField("ShipCity", ShipCity)
                    cs.SetField("ShipCompany", ShipCompany)
                    cs.SetField("ShipCountry", ShipCountry)
                    cs.SetField("ShipName", ShipName)
                    cs.SetField("ShipPhone", ShipPhone)
                    cs.SetField("ShipState", ShipState)
                    cs.SetField("ShipZip", ShipZip)
                    cs.SetField("SortOrder", SortOrder)
                    cs.SetField("State", State)
                    'cs.SetField("StyleFilename", StyleFilename)
                    cs.SetField("ThumbnailFilename", ThumbnailFilename)
                    cs.SetField("Title", Title)
                    cs.SetField("Username", Username)
                    cs.SetField("Visits", Visits.ToString())
                    cs.SetField("Zip", Zip)
                End If
                Call cs.Close()
                '
                ' -- invalidate objects
                ' -- no, the primary is invalidated by the cs.save()
                'cpCore.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"id", id.ToString))
                ' -- no, the secondary points to the pirmary, which is invalidated. Dont waste resources invalidating
                'cpCore.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"ccguid", ccguid))
                '
                ' -- object is here, but the cache was invalidated, setting
                cpCore.cache.setObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", Me.id.ToString()), Me)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw
            End Try
            Return id
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' delete an existing database record
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>
        Public Shared Sub delete(cpCore As coreClass, recordId As Integer)
            Try
                If (recordId > 0) Then
                    cpCore.db.deleteContentRecords(primaryContentName, "id=" & recordId.ToString)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' delete an existing database record
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>
        Public Shared Sub delete(cpCore As coreClass, guid As String)
            Try
                If (Not String.IsNullOrEmpty(guid)) Then
                    cpCore.db.deleteContentRecords(primaryContentName, "(ccguid=" & cpCore.db.encodeSQLText(guid) & ")")
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' get a list of objects from this model
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="someCriteria"></param>
        ''' <returns></returns>
        Public Shared Function getObjectList(cpCore As coreClass, someCriteria As Integer) As List(Of personModel)
            Dim result As New List(Of personModel)
            Try
                Dim cs As New csController(cpCore)
                Dim ignoreCacheNames As New List(Of String)
                If (cs.open(primaryContentName, "(someCriteria=" & someCriteria & ")", "name", True, "id")) Then
                    Dim instance As personModel
                    Do
                        instance = personModel.create(cpCore, cs.getInteger("id"), ignoreCacheNames)
                        If (instance IsNot Nothing) Then
                            result.Add(instance)
                        End If
                        cs.goNext()
                    Loop While cs.ok()
                End If
                cs.Close()
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' invalidate the primary key (which depends on all secondary keys)
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="recordId"></param>
        Public Shared Sub invalidateIdCache(cpCore As coreClass, recordId As Integer)
            cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "id", recordId.ToString))
            '
            ' -- always clear the cache with the content name
            '?? cpCore.cache.invalidateObject(primaryContentName)
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' invalidate a secondary key (ccGuid field).
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="guid"></param>
        Public Shared Sub invalidateGuidCache(cpCore As coreClass, guid As String)
            cpCore.cache.invalidateObject(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "ccguid", guid))
            '
            ' -- always clear the cache with the content name
            '?? cpCore.cache.invalidateObject(primaryContentName)
        End Sub
        ''
        ''====================================================================================================
        '''' <summary>
        '''' produce a standard format cachename for this model
        '''' </summary>
        '''' <param name="fieldName"></param>
        '''' <param name="fieldValue"></param>
        '''' <returns></returns>
        'Private Shared Function Controllers.cacheController.getModelCacheName( primaryContentTableName,fieldName As String, fieldValue As String) As String
        '    Return (primaryContentTableName & "." & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
        'End Function
    End Class
End Namespace
