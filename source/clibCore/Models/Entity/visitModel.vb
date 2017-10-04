
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class visitModel
        '
        '-- const
        Public Const primaryContentName As String = "visits"
        Private Const primaryContentTableName As String = "ccvisits"
        Private Const primaryContentDataSource As String = "default" '<----- set to datasource if not default
        '
        ' -- instance properties
        Public id As Integer
        Public Active As Boolean
        Public Bot As Boolean
        Public Browser As String
        Public ccGuid As String
        
        Public ContentControlID As Integer
        Public CookieSupport As Boolean
        Public CreatedBy As Integer
        Public CreateKey As Integer
        Public DateAdded As Date
        
        
        
        Public ExcludeFromAnalytics As Boolean
        Public HTTP_FROM As String
        Public HTTP_REFERER As String
        Public HTTP_VIA As String
        Public LastVisitTime As Date
        Public LoginAttempts As Integer
        Public MemberID As Integer
        Public MemberNew As Boolean
        Public Mobile As Boolean
        Public ModifiedBy As Integer
        Public ModifiedDate As Date
        Public Name As String
        Public PageVisits As Integer
        Public RefererPathPage As String
        Public REMOTE_ADDR As String
        Public RemoteName As String
        Public SortOrder As String
        Public StartDateValue As Integer
        Public StartTime As Date
        Public StopTime As Date
        Public TimeToLastHit As Integer
        Public VerboseReporting As Boolean
        Public VisitAuthenticated As Boolean
        Public VisitorID As Integer
        Public VisitorNew As Boolean
        '
        ' -- publics not exposed to the UI (test/internal data)
        '<JsonIgnore> Public createKey As Integer
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
        Public Shared Function add(cpCore As coreClass, ByRef cacheNameList As List(Of String)) As visitModel
            Dim result As visitModel = Nothing
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
        Public Shared Function create(cpCore As coreClass, recordId As Integer, ByRef cacheNameList As List(Of String)) As visitModel
            Dim result As visitModel = Nothing
            Try
                If recordId > 0 Then
                    Dim cacheName As String = Controllers.cacheController.getCacheName_Entity(primaryContentTableName, recordId)
                    result = cpCore.cache.getObject(Of visitModel)(cacheName)
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
        Public Shared Function create(cpCore As coreClass, recordGuid As String, ByRef cacheNameList As List(Of String)) As visitModel
            Dim result As visitModel = Nothing
            Try
                If Not String.IsNullOrEmpty(recordGuid) Then
                    Dim cacheName As String = Controllers.cacheController.getCacheName_Entity(primaryContentTableName, "ccguid", recordGuid)
                    result = cpCore.cache.getObject(Of visitModel)(cacheName)
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
        Private Shared Function loadObject(cpCore As coreClass, sqlCriteria As String, ByRef cacheNameList As List(Of String), Optional sortOrder As String = "id") As visitModel
            Dim result As visitModel = Nothing
            Try
                Dim cs As New csController(cpCore)
                If cs.open(primaryContentName, sqlCriteria, sortOrder) Then
                    result = New visitModel
                    With result
                        '
                        ' -- populate result model
                        .id = cs.getInteger("ID")
                        .Active = cs.getBoolean("Active")
                        .Bot = cs.getBoolean("Bot")
                        .Browser = cs.getText("Browser")
                        .ccGuid = cs.getText("ccGuid")
                        '
                        .ContentControlID = cs.getInteger("ContentControlID")
                        .CookieSupport = cs.getBoolean("CookieSupport")
                        .CreatedBy = cs.getInteger("CreatedBy")
                        .CreateKey = cs.getInteger("CreateKey")
                        .DateAdded = cs.getDate("DateAdded")
                        '
                        '
                        '
                        .ExcludeFromAnalytics = cs.getBoolean("ExcludeFromAnalytics")
                        .HTTP_FROM = cs.getText("HTTP_FROM")
                        .HTTP_REFERER = cs.getText("HTTP_REFERER")
                        .HTTP_VIA = cs.getText("HTTP_VIA")
                        .LastVisitTime = cs.getDate("LastVisitTime")
                        .LoginAttempts = cs.getInteger("LoginAttempts")
                        .MemberID = cs.getInteger("MemberID")
                        .MemberNew = cs.getBoolean("MemberNew")
                        .Mobile = cs.getBoolean("Mobile")
                        .ModifiedBy = cs.getInteger("ModifiedBy")
                        .ModifiedDate = cs.getDate("ModifiedDate")
                        .Name = cs.getText("Name")
                        .PageVisits = cs.getInteger("PageVisits")
                        .RefererPathPage = cs.getText("RefererPathPage")
                        .REMOTE_ADDR = cs.getText("REMOTE_ADDR")
                        .RemoteName = cs.getText("RemoteName")
                        .SortOrder = cs.getText("SortOrder")
                        .StartDateValue = cs.getInteger("StartDateValue")
                        .StartTime = cs.getDate("StartTime")
                        .StopTime = cs.getDate("StopTime")
                        .TimeToLastHit = cs.getInteger("TimeToLastHit")
                        .VerboseReporting = cs.getBoolean("VerboseReporting")
                        .VisitAuthenticated = cs.getBoolean("VisitAuthenticated")
                        .VisitorID = cs.getInteger("VisitorID")
                        .VisitorNew = cs.getBoolean("VisitorNew")
                        If (String.IsNullOrEmpty(.ccGuid)) Then .ccGuid = Controllers.genericController.getGUID()
                    End With
                    If (result IsNot Nothing) Then
                        '
                        ' -- set primary and secondary caches
                        ' -- add all cachenames to the injected cachenamelist
                        Dim cacheName0 As String = Controllers.cacheController.getCacheName_Entity(primaryContentTableName, "id", result.id.ToString())
                        cacheNameList.Add(cacheName0)
                        cpCore.cache.setObject(cacheName0, result)
                        '
                        Dim cacheName1 As String = Controllers.cacheController.getCacheName_Entity(primaryContentTableName, "ccguid", result.ccGuid)
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
                    If (String.IsNullOrEmpty(Name)) Then
                        Name = "Visit " & id.ToString()
                    End If
                    cs.setField("Active", Active.ToString())
                    cs.SetField("Bot", Bot.ToString())
                    cs.SetField("Browser", Browser)
                    cs.SetField("ccGuid", ccGuid)
                    '
                    cs.SetField("ContentControlID", ContentControlID.ToString())
                    cs.SetField("CookieSupport", CookieSupport.ToString())
                    cs.SetField("CreatedBy", CreatedBy.ToString())
                    cs.SetField("CreateKey", CreateKey.ToString())
                    cs.SetField("DateAdded", DateAdded.ToString())
                    '
                    '
                    '
                    cs.SetField("ExcludeFromAnalytics", ExcludeFromAnalytics.ToString())
                    cs.SetField("HTTP_FROM", HTTP_FROM)
                    cs.SetField("HTTP_REFERER", HTTP_REFERER)
                    cs.SetField("HTTP_VIA", HTTP_VIA)
                    cs.SetField("LastVisitTime", LastVisitTime.ToString())
                    cs.SetField("LoginAttempts", LoginAttempts.ToString())
                    cs.SetField("MemberID", MemberID.ToString())
                    cs.SetField("MemberNew", MemberNew.ToString())
                    cs.SetField("Mobile", Mobile.ToString())
                    cs.SetField("ModifiedBy", ModifiedBy.ToString())
                    cs.SetField("ModifiedDate", ModifiedDate.ToString())
                    cs.SetField("Name", Name)
                    cs.SetField("PageVisits", PageVisits.ToString())
                    cs.SetField("RefererPathPage", RefererPathPage)
                    cs.SetField("REMOTE_ADDR", REMOTE_ADDR)
                    cs.SetField("RemoteName", RemoteName)
                    cs.SetField("SortOrder", SortOrder)
                    cs.SetField("StartDateValue", StartDateValue.ToString())
                    cs.SetField("StartTime", StartTime.ToString())
                    cs.SetField("StopTime", StopTime.ToString())
                    cs.SetField("TimeToLastHit", TimeToLastHit.ToString())
                    cs.SetField("VerboseReporting", VerboseReporting.ToString())
                    cs.SetField("VisitAuthenticated", VisitAuthenticated.ToString())
                    cs.SetField("VisitorID", VisitorID.ToString())
                    cs.SetField("VisitorNew", VisitorNew.ToString())
                End If
                Call cs.Close()
                '
                ' -- object is here, but the cache was invalidated, setting
                cpCore.cache.setObject(Controllers.cacheController.getCacheName_Entity(primaryContentTableName, "id", Me.id.ToString()), Me)
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
        Public Shared Function getObjectList(cpCore As coreClass, someCriteria As Integer) As List(Of visitModel)
            Dim result As New List(Of visitModel)
            Try
                Dim cs As New csController(cpCore)
                Dim ignoreCacheNames As New List(Of String)
                If (cs.open(primaryContentName, "(someCriteria=" & someCriteria & ")", "name", True, "id")) Then
                    Dim instance As visitModel
                    Do
                        instance = visitModel.create(cpCore, cs.getInteger("id"), ignoreCacheNames)
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
            cpCore.cache.invalidateObject(Controllers.cacheController.getCacheName_Entity(primaryContentTableName, recordId))
            '
            ' -- always clear the cache with the content name
            '?? '?? cpCore.cache.invalidateObject(primaryContentName)
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' invalidate a secondary key (ccGuid field).
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="guid"></param>
        Public Shared Sub invalidateGuidCache(cpCore As coreClass, guid As String)
            cpCore.cache.invalidateObject(Controllers.cacheController.getCacheName_Entity(primaryContentTableName, "ccguid", guid))
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
        '
        '====================================================================================================
        ''' <summary>
        ''' return a visit object for the visitor's last visit before the provided id
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="visitId"></param>
        ''' <param name="visitorId"></param>
        ''' <returns></returns>
        Public Shared Function getLastVisitByVisitor(cpCore As coreClass, visitId As Integer, visitorId As Integer) As visitModel
            Dim result As visitModel = loadObject(cpCore, "(id<>" & visitId & ")and(VisitorID=" & visitorId & ")", New List(Of String), "id desc")
            Return result
        End Function
    End Class
End Namespace
