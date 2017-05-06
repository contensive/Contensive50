
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Contensive.Core.Models.Entity
    '
    '====================================================================================================
    ' cached entity model pattern
    '   factory pattern creator, constructor is a shared method that returns a loaded object
    '   new() - to allow deserialization (so all methods must pass in cp)
    '   shared getObject( cp, id ) - returns loaded model
    '   saveObject( cp ) - saves instance properties, returns the record id
    '
    Public Class addonModel
        '
        ' -- public properties
        '
        Public id As Integer = 0
        Public name As String = String.Empty
        Public active As Boolean = False
        Public sortorder As String = String.Empty
        Public dateadded As Date = Date.MinValue
        Public createdby As Integer = 0
        Public modifieddate As Date = Date.MinValue
        Public modifiedby As Integer = 0
        Public ContentControlID As Integer = 0
        Public CreateKey As Integer = 0
        Public EditSourceID As Integer = 0
        Public EditArchive As Boolean = False
        Public EditBlank As Boolean = False
        Public ContentCategoryID As Integer = 0
        Public ccGuid As String = String.Empty
        Public onpagestartevent As Boolean = False
        Public blockdefaultstyles As Boolean = False
        Public iconheight As Integer = 0
        Public iconwidth As Integer = 0
        Public customstylesfilename As String = String.Empty
        Public iconsprites As Integer = 0
        Public javascriptbodyend As String = String.Empty
        Public onnewvisitevent As Boolean = False
        Public remotemethod As Boolean = False
        Public copytext As String = String.Empty
        Public sharedstyles As String = String.Empty
        Public stylesfilename As String = String.Empty
        Public otherheadtags As String = String.Empty
        Public metakeywordlist As String = String.Empty
        Public onpageendevent As Boolean = False
        Public pagetitle As String = String.Empty
        Public iconfilename As String = String.Empty
        Public javascriptonload As String = String.Empty
        Public admin As Boolean = False
        Public content As Boolean = False
        Public template As Boolean = False
        Public email As Boolean = False
        Public helplink As String = String.Empty
        Public navtypeid As Integer = 0
        Public help As String = String.Empty
        Public metadescription As String = String.Empty
        Public scriptingcode As String = String.Empty
        Public scriptingtimeout As String = String.Empty
        Public inlinescript As String = String.Empty
        Public collectionid As Integer = 0
        Public onbodystart As Boolean = False
        Public fieldtypeeditor As String = String.Empty
        Public isinline As Boolean = False
        Public dotnetclass As String = String.Empty
        Public copy As String = String.Empty
        Public argumentlist As String = String.Empty
        Public link As String = String.Empty
        Public onbodyend As Boolean = False
        Public objectprogramid As String = String.Empty
        Public jsfilename As String = String.Empty
        Public robotstxt As String = String.Empty
        Public includedaddons As String = String.Empty
        Public formxml As String = String.Empty
        Public inframe As Boolean = False
        Public filter As Boolean = False
        Public scriptinglanguageid As Integer = 0
        Public remoteassetlink As String = String.Empty
        Public blockedittools As Boolean = False
        Public asajax As Boolean = False
        Public processserverkey As String = String.Empty
        Public processinterval As Integer = 0
        Public processrunonce As Boolean = False
        Public processnextrun As Date = Date.MinValue
        Public processcontenttriggers As String = String.Empty
        Public scriptingentrypoint As String = String.Empty
        Public scriptingmodules As String = String.Empty
        Public events As String = String.Empty
        '
        ' -- list of tag names that will flush the cache
        Public Shared ReadOnly Property cacheTagList As String = "Aggregate Functions"
        '
        '====================================================================================================
        ''' <summary>
        ''' Create an empty object. needed for deserialization. Use newModel() method as constructor, includes cache
        ''' </summary>
        Public Sub New()
            '
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Open existing
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>
        Public Shared Function getObject(cpcore As coreClass, recordId As Integer, ByRef adminMessageList As List(Of String)) As addonModel
            Dim returnModel As addonModel = Nothing
            Try
                Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer
                Dim recordCacheName As String = cnAddons & "CachedModelRecordId" & recordId
                Dim recordCache As String = cpcore.cache.getString(recordCacheName)
                Dim loadDbModel As Boolean = True
                If Not String.IsNullOrEmpty(recordCache) Then
                    Try
                        returnModel = json_serializer.Deserialize(Of addonModel)(recordCache)
                        '
                        ' -- if model exposes any objects, verify they are created
                        'If (returnModel.meetingItemList Is Nothing) Then
                        '    returnModel.meetingItemList = New List(Of meetingItemModel)
                        'End If
                        loadDbModel = False
                    Catch ex As Exception
                        'ignore error - just roll through to rebuild model and save new cache
                    End Try
                End If
                If loadDbModel Or (returnModel Is Nothing) Then
                    returnModel = getObjectNoCache(cpcore, recordId)
                    Call cpcore.cache.setKey(recordCacheName, json_serializer.Serialize(returnModel), cacheTagList)
                End If
            Catch ex As Exception
                cpcore.handleExceptionAndRethrow(ex)
            End Try
            Return returnModel
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' called only from getObject. Load the model from the Db without cache. If there are any properties or objects that cannot be used from cache, do not include them here either, load in getObject()
        ''' </summary>
        ''' <param name="recordId"></param>
        Private Shared Function getObjectNoCache(cpcore As coreClass, recordId As Integer) As addonModel
            Dim returnNewModel As New addonModel()
            Try
                Dim cs As New csController(cpcore)
                returnNewModel.id = 0
                If recordId <> 0 Then
                    cs.open(cnAddons, "(ID=" & recordId & ")")
                    If cs.ok() Then
                        With returnNewModel
                            .id = recordId
                            .name = cs.getText("Name")
                            .active = cs.getBoolean("active")
                            .sortorder = cs.getText("sortorder")
                            .dateadded = cs.getDate("dateadded")
                            .createdby = cs.getInteger("createdby")
                            .modifieddate = cs.getDate("modifieddate")
                            .modifiedby = cs.getInteger("modifiedby")
                            .ContentControlID = cs.getInteger("ContentControlID")
                            .CreateKey = cs.getInteger("CreateKey")
                            .EditSourceID = cs.getInteger("EditSourceID")
                            .EditArchive = cs.getBoolean("EditArchive")
                            .EditBlank = cs.getBoolean("EditBlank")
                            .ContentCategoryID = cs.getInteger("ContentCategoryID")
                            .ccGuid = cs.getText("ccGuid")
                            .onpagestartevent = cs.getBoolean("onpagestartevent")
                            .blockdefaultstyles = cs.getBoolean("blockdefaultstyles")
                            .iconheight = cs.getInteger("iconheight")
                            .iconwidth = cs.getInteger("iconwidth")
                            .customstylesfilename = cs.getText("customstylesfilename")
                            .iconsprites = cs.getInteger("iconsprites")
                            .javascriptbodyend = cs.getText("javascriptbodyend")
                            .onnewvisitevent = cs.getBoolean("onnewvisitevent")
                            .remotemethod = cs.getBoolean("remotemethod")
                            .copytext = cs.getText("copytext")
                            .sharedstyles = cs.getText("sharedstyles")
                            .stylesfilename = cs.getText("stylesfilename")
                            .otherheadtags = cs.getText("otherheadtags")
                            .metakeywordlist = cs.getText("metakeywordlist")
                            .onpageendevent = cs.getBoolean("onpageendevent")
                            .pagetitle = cs.getText("pagetitle")
                            .iconfilename = cs.getText("iconfilename")
                            .javascriptonload = cs.getText("javascriptonload")
                            .admin = cs.getBoolean("admin")
                            .content = cs.getBoolean("content")
                            .template = cs.getBoolean("template")
                            .email = cs.getBoolean("email")
                            .helplink = cs.getText("helplink")
                            .navtypeid = cs.getInteger("navtypeid")
                            .help = cs.getText("help")
                            .metadescription = cs.getText("metadescription")
                            .scriptingcode = cs.getText("scriptingcode")
                            .scriptingtimeout = cs.getText("scriptingtimeout")
                            .inlinescript = cs.getText("inlinescript")
                            .collectionid = cs.getInteger("collectionid")
                            .onbodystart = cs.getBoolean("onbodystart")
                            .fieldtypeeditor = cs.getText("fieldtypeeditor")
                            .isinline = cs.getBoolean("isinline")
                            .dotnetclass = cs.getText("dotnetclass")
                            .copy = cs.getText("copy")
                            .argumentlist = cs.getText("argumentlist")
                            .link = cs.getText("link")
                            .onbodyend = cs.getBoolean("onbodyend")
                            .objectprogramid = cs.getText("objectprogramid")
                            .jsfilename = cs.getText("jsfilename")
                            .robotstxt = cs.getText("robotstxt")
                            .includedaddons = cs.getText("includedaddons")
                            .formxml = cs.getText("formxml")
                            .inframe = cs.getBoolean("inframe")
                            .filter = cs.getBoolean("filter")
                            .scriptinglanguageid = cs.getInteger("scriptinglanguageid")
                            .remoteassetlink = cs.getText("remoteassetlink")
                            .blockedittools = cs.getBoolean("blockedittools")
                            .asajax = cs.getBoolean("asajax")
                            .processserverkey = cs.getText("processserverkey")
                            .processinterval = cs.getInteger("processinterval")
                            .processrunonce = cs.getBoolean("processrunonce")
                            .processnextrun = cs.getDate("processnextrun")
                            .processcontenttriggers = cs.getText("processcontenttriggers")
                            .scriptingentrypoint = cs.getText("scriptingentrypoint")
                            .scriptingmodules = cs.getText("scriptingmodules")
                            .events = cs.getText("events")
                        End With
                    End If
                    Call cs.Close()
                End If
            Catch ex As Exception
                cpcore.handleExceptionAndRethrow(ex)
            End Try
            Return returnNewModel
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Save the object
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Function saveObject(cpcore As coreClass) As Integer
            Try
                Dim cs As New csController(cpcore)
                If (id > 0) Then
                    If Not cs.open(cnAddons, "id=" & id) Then
                        id = 0
                        cs.Close()
                        Throw New ApplicationException("Unable to open record [" & id & "]")
                    End If
                Else
                    If Not cs.Insert(cnAddons) Then
                        cs.Close()
                        id = 0
                        Throw New ApplicationException("Unable to insert record")
                    End If
                End If
                If cs.ok() Then
                    id = cs.getInteger("id")
                    cs.SetField("Name", name)
                    cs.setField("active", active)
                    cs.SetField("sortorder", sortorder)
                    cs.setField("dateadded", dateadded)
                    cs.setField("createdby", createdby)
                    cs.setField("modifieddate", modifieddate)
                    cs.setField("modifiedby", modifiedby)
                    cs.setField("ContentControlID", ContentControlID)
                    cs.setField("CreateKey", CreateKey)
                    cs.setField("EditSourceID", EditSourceID)
                    cs.setField("EditArchive", EditArchive)
                    cs.setField("EditBlank", EditBlank)
                    cs.setField("ContentCategoryID", ContentCategoryID)
                    cs.SetField("ccGuid", ccGuid)
                    cs.setField("onpagestartevent", onpagestartevent)
                    cs.setField("blockdefaultstyles", blockdefaultstyles)
                    cs.setField("iconheight", iconheight)
                    cs.setField("iconwidth", iconwidth)
                    cs.SetField("customstylesfilename", customstylesfilename)
                    cs.setField("iconsprites", iconsprites)
                    cs.SetField("javascriptbodyend", javascriptbodyend)
                    cs.setField("onnewvisitevent", onnewvisitevent)
                    cs.setField("remotemethod", remotemethod)
                    cs.SetField("copytext", copytext)
                    cs.SetField("sharedstyles", sharedstyles)
                    cs.SetField("stylesfilename", stylesfilename)
                    cs.SetField("otherheadtags", otherheadtags)
                    cs.SetField("metakeywordlist", metakeywordlist)
                    cs.setField("onpageendevent", onpageendevent)
                    cs.SetField("pagetitle", pagetitle)
                    cs.SetField("iconfilename", iconfilename)
                    cs.SetField("javascriptonload", javascriptonload)
                    cs.setField("admin", admin)
                    cs.setField("content", content)
                    cs.setField("template", template)
                    cs.setField("email", email)
                    cs.SetField("helplink", helplink)
                    cs.setField("navtypeid", navtypeid)
                    cs.SetField("help", help)
                    cs.SetField("metadescription", metadescription)
                    cs.SetField("scriptingcode", scriptingcode)
                    cs.SetField("scriptingtimeout", scriptingtimeout)
                    cs.SetField("inlinescript", inlinescript)
                    cs.setField("collectionid", collectionid)
                    cs.setField("onbodystart", onbodystart)
                    cs.SetField("fieldtypeeditor", fieldtypeeditor)
                    cs.setField("isinline", isinline)
                    cs.SetField("dotnetclass", dotnetclass)
                    cs.SetField("copy", copy)
                    cs.SetField("argumentlist", argumentlist)
                    cs.SetField("link", link)
                    cs.setField("onbodyend", onbodyend)
                    cs.SetField("objectprogramid", objectprogramid)
                    cs.SetField("jsfilename", jsfilename)
                    cs.SetField("robotstxt", robotstxt)
                    cs.SetField("includedaddons", includedaddons)
                    cs.SetField("formxml", formxml)
                    cs.setField("inframe", inframe)
                    cs.setField("filter", filter)
                    cs.setField("scriptinglanguageid", scriptinglanguageid)
                    cs.SetField("remoteassetlink", remoteassetlink)
                    cs.setField("blockedittools", blockedittools)
                    cs.setField("asajax", asajax)
                    cs.SetField("processserverkey", processserverkey)
                    cs.setField("processinterval", processinterval)
                    cs.setField("processrunonce", processrunonce)
                    cs.setField("processnextrun", processnextrun)
                    cs.SetField("processcontenttriggers", processcontenttriggers)
                    cs.SetField("scriptingentrypoint", scriptingentrypoint)
                    cs.SetField("scriptingmodules", scriptingmodules)
                    cs.SetField("events", events)
                End If
                Call cs.Close()
            Catch ex As Exception
                cpcore.handleExceptionAndRethrow(ex)
                Throw
            End Try
            Return id
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return a list of remote methods
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Shared Function getRemoteMethods(cpcore As coreClass) As List(Of addonModel)
            Dim result As List(Of addonModel) = Nothing
            Try
                Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer
                Dim recordCacheName As String = cnAddons & "CachedRemoteMethods"
                Dim recordCache As String = cpcore.cache.getString(recordCacheName)
                Dim loadDbModel As Boolean = True
                If Not String.IsNullOrEmpty(recordCache) Then
                    Try
                        result = json_serializer.Deserialize(Of List(Of addonModel))(recordCache)
                        '
                        ' -- if model exposes any objects, verify they are created
                        'If (returnModel.meetingItemList Is Nothing) Then
                        '    returnModel.meetingItemList = New List(Of meetingItemModel)
                        'End If
                        loadDbModel = False
                    Catch ex As Exception
                        'ignore error - just roll through to rebuild model and save new cache
                    End Try
                End If
                If loadDbModel Or (result Is Nothing) Then
                    result = getRemoteMethodsNoCache(cpcore)
                    Call cpcore.cache.setKey(recordCacheName, json_serializer.Serialize(result), cacheTagList)
                End If
            Catch ex As Exception
                cpcore.handleExceptionAndRethrow(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return a list of remote methods without cache
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Shared Function getRemoteMethodsNoCache(cpcore As coreClass) As List(Of addonModel)
            Dim result As New List(Of addonModel)
            Try
                Dim cs As New csController(cpcore)
                If (cs.open(cnAddons, "(remoteMethod=1)", "name", True, "id")) Then
                    Do
                        result.Add(getObject(cpcore, cs.getInteger("id"), New List(Of String)))
                        cs.goNext()
                    Loop While cs.ok()
                End If

            Catch ex As Exception
                cpcore.handleExceptionAndRethrow(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get the name of the meeting
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="recordId"></param>record
        ''' <returns></returns>
        Public Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return cpcore.content_GetRecordName(cnAddons, recordId)
        End Function
    End Class
End Namespace

