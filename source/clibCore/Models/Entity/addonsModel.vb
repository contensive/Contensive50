
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
        Public Shared Function getObject(cp As CPBaseClass, recordId As Integer, ByRef adminMessageList As List(Of String)) As addonModel
            Dim returnModel As addonModel = Nothing
            Try
                Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer
                Dim recordCacheName As String = cnAddons & "CachedModelRecordId" & recordId
                Dim recordCache As String = cp.Cache.Read(recordCacheName)
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
                    returnModel = getObjectNoCache(cp, recordId)
                    Call cp.Cache.Save(recordCacheName, json_serializer.Serialize(returnModel), cacheTagList)
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return returnModel
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' called only from getObject. Load the model from the Db without cache. If there are any properties or objects that cannot be used from cache, do not include them here either, load in getObject()
        ''' </summary>
        ''' <param name="recordId"></param>
        Private Shared Function getObjectNoCache(cp As CPBaseClass, recordId As Integer) As addonModel
            Dim returnNewModel As New addonModel()
            Try
                Dim cs As CPCSBaseClass = cp.CSNew()
                returnNewModel.id = 0
                If recordId <> 0 Then
                    cs.Open(cnAddons, "(ID=" & recordId & ")")
                    If cs.OK() Then
                        With returnNewModel
                            .id = recordId
                            .name = cs.GetText("Name")
                            .active = cs.GetBoolean("active")
                            .sortorder = cs.GetText("sortorder")
                            .dateadded = cs.GetDate("dateadded")
                            .createdby = cs.GetInteger("createdby")
                            .modifieddate = cs.GetDate("modifieddate")
                            .modifiedby = cs.GetInteger("modifiedby")
                            .ContentControlID = cs.GetInteger("ContentControlID")
                            .CreateKey = cs.GetInteger("CreateKey")
                            .EditSourceID = cs.GetInteger("EditSourceID")
                            .EditArchive = cs.GetBoolean("EditArchive")
                            .EditBlank = cs.GetBoolean("EditBlank")
                            .ContentCategoryID = cs.GetInteger("ContentCategoryID")
                            .ccGuid = cs.GetText("ccGuid")
                            .onpagestartevent = cs.GetBoolean("onpagestartevent")
                            .blockdefaultstyles = cs.GetBoolean("blockdefaultstyles")
                            .iconheight = cs.GetInteger("iconheight")
                            .iconwidth = cs.GetInteger("iconwidth")
                            .customstylesfilename = cs.GetText("customstylesfilename")
                            .iconsprites = cs.GetInteger("iconsprites")
                            .javascriptbodyend = cs.GetText("javascriptbodyend")
                            .onnewvisitevent = cs.GetBoolean("onnewvisitevent")
                            .remotemethod = cs.GetBoolean("remotemethod")
                            .copytext = cs.GetText("copytext")
                            .sharedstyles = cs.GetText("sharedstyles")
                            .stylesfilename = cs.GetText("stylesfilename")
                            .otherheadtags = cs.GetText("otherheadtags")
                            .metakeywordlist = cs.GetText("metakeywordlist")
                            .onpageendevent = cs.GetBoolean("onpageendevent")
                            .pagetitle = cs.GetText("pagetitle")
                            .iconfilename = cs.GetText("iconfilename")
                            .javascriptonload = cs.GetText("javascriptonload")
                            .admin = cs.GetBoolean("admin")
                            .content = cs.GetBoolean("content")
                            .template = cs.GetBoolean("template")
                            .email = cs.GetBoolean("email")
                            .helplink = cs.GetText("helplink")
                            .navtypeid = cs.GetInteger("navtypeid")
                            .help = cs.GetText("help")
                            .metadescription = cs.GetText("metadescription")
                            .scriptingcode = cs.GetText("scriptingcode")
                            .scriptingtimeout = cs.GetText("scriptingtimeout")
                            .inlinescript = cs.GetText("inlinescript")
                            .collectionid = cs.GetInteger("collectionid")
                            .onbodystart = cs.GetBoolean("onbodystart")
                            .fieldtypeeditor = cs.GetText("fieldtypeeditor")
                            .isinline = cs.GetBoolean("isinline")
                            .dotnetclass = cs.GetText("dotnetclass")
                            .copy = cs.GetText("copy")
                            .argumentlist = cs.GetText("argumentlist")
                            .link = cs.GetText("link")
                            .onbodyend = cs.GetBoolean("onbodyend")
                            .objectprogramid = cs.GetText("objectprogramid")
                            .jsfilename = cs.GetText("jsfilename")
                            .robotstxt = cs.GetText("robotstxt")
                            .includedaddons = cs.GetText("includedaddons")
                            .formxml = cs.GetText("formxml")
                            .inframe = cs.GetBoolean("inframe")
                            .filter = cs.GetBoolean("filter")
                            .scriptinglanguageid = cs.GetInteger("scriptinglanguageid")
                            .remoteassetlink = cs.GetText("remoteassetlink")
                            .blockedittools = cs.GetBoolean("blockedittools")
                            .asajax = cs.GetBoolean("asajax")
                            .processserverkey = cs.GetText("processserverkey")
                            .processinterval = cs.GetInteger("processinterval")
                            .processrunonce = cs.GetBoolean("processrunonce")
                            .processnextrun = cs.GetDate("processnextrun")
                            .processcontenttriggers = cs.GetText("processcontenttriggers")
                            .scriptingentrypoint = cs.GetText("scriptingentrypoint")
                            .scriptingmodules = cs.GetText("scriptingmodules")
                            .events = cs.GetText("events")
                        End With
                    End If
                    Call cs.Close()
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
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
        Public Function saveObject(cp As CPBaseClass) As Integer
            Try
                Dim cs As CPCSBaseClass = cp.CSNew()
                If (id > 0) Then
                    If Not cs.Open(cnAddons, "id=" & id) Then
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
                If cs.OK() Then
                    id = cs.GetInteger("id")
                    cs.SetField("Name", name)
                    cs.SetField("active", active)
                    cs.SetField("sortorder", sortorder)
                    cs.SetField("dateadded", dateadded)
                    cs.SetField("createdby", createdby)
                    cs.SetField("modifieddate", modifieddate)
                    cs.SetField("modifiedby", modifiedby)
                    cs.SetField("ContentControlID", ContentControlID)
                    cs.SetField("CreateKey", CreateKey)
                    cs.SetField("EditSourceID", EditSourceID)
                    cs.SetField("EditArchive", EditArchive)
                    cs.SetField("EditBlank", EditBlank)
                    cs.SetField("ContentCategoryID", ContentCategoryID)
                    cs.SetField("ccGuid", ccGuid)
                    cs.SetField("onpagestartevent", onpagestartevent)
                    cs.SetField("blockdefaultstyles", blockdefaultstyles)
                    cs.SetField("iconheight", iconheight)
                    cs.SetField("iconwidth", iconwidth)
                    cs.SetField("customstylesfilename", customstylesfilename)
                    cs.SetField("iconsprites", iconsprites)
                    cs.SetField("javascriptbodyend", javascriptbodyend)
                    cs.SetField("onnewvisitevent", onnewvisitevent)
                    cs.SetField("remotemethod", remotemethod)
                    cs.SetField("copytext", copytext)
                    cs.SetField("sharedstyles", sharedstyles)
                    cs.SetField("stylesfilename", stylesfilename)
                    cs.SetField("otherheadtags", otherheadtags)
                    cs.SetField("metakeywordlist", metakeywordlist)
                    cs.SetField("onpageendevent", onpageendevent)
                    cs.SetField("pagetitle", pagetitle)
                    cs.SetField("iconfilename", iconfilename)
                    cs.SetField("javascriptonload", javascriptonload)
                    cs.SetField("admin", admin)
                    cs.SetField("content", content)
                    cs.SetField("template", template)
                    cs.SetField("email", email)
                    cs.SetField("helplink", helplink)
                    cs.SetField("navtypeid", navtypeid)
                    cs.SetField("help", help)
                    cs.SetField("metadescription", metadescription)
                    cs.SetField("scriptingcode", scriptingcode)
                    cs.SetField("scriptingtimeout", scriptingtimeout)
                    cs.SetField("inlinescript", inlinescript)
                    cs.SetField("collectionid", collectionid)
                    cs.SetField("onbodystart", onbodystart)
                    cs.SetField("fieldtypeeditor", fieldtypeeditor)
                    cs.SetField("isinline", isinline)
                    cs.SetField("dotnetclass", dotnetclass)
                    cs.SetField("copy", copy)
                    cs.SetField("argumentlist", argumentlist)
                    cs.SetField("link", link)
                    cs.SetField("onbodyend", onbodyend)
                    cs.SetField("objectprogramid", objectprogramid)
                    cs.SetField("jsfilename", jsfilename)
                    cs.SetField("robotstxt", robotstxt)
                    cs.SetField("includedaddons", includedaddons)
                    cs.SetField("formxml", formxml)
                    cs.SetField("inframe", inframe)
                    cs.SetField("filter", filter)
                    cs.SetField("scriptinglanguageid", scriptinglanguageid)
                    cs.SetField("remoteassetlink", remoteassetlink)
                    cs.SetField("blockedittools", blockedittools)
                    cs.SetField("asajax", asajax)
                    cs.SetField("processserverkey", processserverkey)
                    cs.SetField("processinterval", processinterval)
                    cs.SetField("processrunonce", processrunonce)
                    cs.SetField("processnextrun", processnextrun)
                    cs.SetField("processcontenttriggers", processcontenttriggers)
                    cs.SetField("scriptingentrypoint", scriptingentrypoint)
                    cs.SetField("scriptingmodules", scriptingmodules)
                    cs.SetField("events", events)
                End If
                Call cs.Close()
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
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
        Public Shared Function getRemoteMethods(cp As CPBaseClass) As List(Of addonModel)
            Dim result As List(Of addonModel) = Nothing
            Try
                Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer
                Dim recordCacheName As String = cnAddons & "CachedRemoteMethods"
                Dim recordCache As String = cp.Cache.Read(recordCacheName)
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
                    result = getRemoteMethodsNoCache(cp)
                    Call cp.Cache.Save(recordCacheName, json_serializer.Serialize(result), cacheTagList)
                End If
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
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
        Public Shared Function getRemoteMethodsNoCache(cp As CPBaseClass) As List(Of addonModel)
            Dim result As New List(Of addonModel)
            Try
                Dim cs As CPCSBaseClass = cp.CSNew()
                If (cs.Open(cnAddons, "(remoteMethod=1)", "name", True, "id")) Then
                    Do
                        result.Add(getObject(cp, cs.GetInteger("id"), New List(Of String)))
                        cs.GoNext()
                    Loop While cs.OK()
                End If

            Catch ex As Exception
                cp.Site.ErrorReport(ex)
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
        Public Shared Function getRecordName(cp As CPBaseClass, recordId As Integer) As String
            Return cp.Content.GetRecordName(cnAddons, recordId)
        End Function
    End Class
End Namespace

