
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    Public Class addonModel
        Inherits baseModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "add-ons" '<------ set content name
        Public Const contentTableName As String = "ccaggregatefunctions" '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default" '<----- set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties
        '
        'Public Property ID As Integer
        'Public Property Active As Boolean
        Public Property Admin As Boolean
        Public Property ArgumentList As String
        Public Property AsAjax As Boolean
        'Public Property BlockDefaultStyles As Boolean
        Public Property BlockEditTools As Boolean
        'Public Property ccGuid As String
        Public Property CollectionID As Integer
        Public Property Content As Boolean
        Public Property ContentCategoryID As Integer
        'Public Property ContentControlID As Integer
        Public Property Copy As String
        Public Property CopyText As String
        'Public Property CreatedBy As Integer
        'Public Property CreateKey As Integer
        'Public Property CustomStylesFilename As String
        'Public Property DateAdded As Date
        Public Property DotNetClass As String
        Public Property EditArchive As Boolean
        Public Property EditBlank As Boolean
        Public Property EditSourceID As Integer
        Public Property Email As Boolean
        Public Property Filter As Boolean
        Public Property FormXML As String
        Public Property Help As String
        Public Property HelpLink As String
        Public Property IconFilename As String
        Public Property IconHeight As Integer
        Public Property IconSprites As Integer
        Public Property IconWidth As Integer
        Public Property InFrame As Boolean
        Public Property IsInline As Boolean
        Public Property JavaScriptBodyEnd As String
        Public Property JavaScriptOnLoad As String
        Public Property JSFilename As fieldTypeTextFile
        Public Property Link As String
        Public Property MetaDescription As String
        Public Property MetaKeywordList As String
        'Public Property ModifiedBy As Integer
        'Public Property ModifiedDate As Date
        'Public Property Name As String
        Public Property NavTypeID As Integer
        Public Property ObjectProgramID As String
        Public Property OnBodyEnd As Boolean
        Public Property OnBodyStart As Boolean
        Public Property OnNewVisitEvent As Boolean
        Public Property OnPageEndEvent As Boolean
        Public Property OnPageStartEvent As Boolean
        Public Property OtherHeadTags As String
        Public Property PageTitle As String
        Public Property ProcessInterval As Integer
        Public Property ProcessNextRun As Date
        Public Property ProcessRunOnce As Boolean
        Public Property ProcessServerKey As String
        Public Property RemoteAssetLink As String
        Public Property RemoteMethod As Boolean
        Public Property RobotsTxt As String
        Public Property ScriptingCode As String
        Public Property ScriptingEntryPoint As String
        Public Property ScriptingLanguageID As Integer
        Public Property ScriptingTimeout As String
        'Public Property SortOrder As String
        Public Property StylesFilename As fieldTypeTextFile
        Public Property Template As Boolean        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass) As addonModel
            Return add(Of addonModel)(cpCore)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As addonModel
            Return add(Of addonModel)(cpCore, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer) As addonModel
            Return create(Of addonModel)(cpCore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As addonModel
            Return create(Of addonModel)(cpCore, recordId, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String) As addonModel
            Return create(Of addonModel)(cpCore, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As addonModel
            Return create(Of addonModel)(cpCore, recordGuid, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String) As addonModel
            Return createByName(Of addonModel)(cpCore, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As addonModel
            Return createByName(Of addonModel)(cpCore, recordName, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cpCore As coreClass)
            MyBase.save(cpCore)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, recordId As Integer)
            delete(Of addonModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cpCore As coreClass, ccGuid As String)
            delete(Of addonModel)(cpCore, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String, callersCacheNameList As List(Of String)) As List(Of addonModel)
            Return createList(Of addonModel)(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String) As List(Of addonModel)
            Return createList(Of addonModel)(cpCore, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String) As List(Of addonModel)
            Return createList(Of addonModel)(cpCore, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
            invalidateCacheSingleRecord(Of addonModel)(cpCore, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
            Return baseModel.getRecordName(Of addonModel)(cpcore, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
            Return baseModel.getRecordName(Of addonModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
            Return baseModel.getRecordId(Of addonModel)(cpcore, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createDefault(cpcore As coreClass) As addonModel
            Return createDefault(Of addonModel)(cpcore)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Get a list of addons to run on new visit
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="someCriteria"></param>
        ''' <returns></returns>
        Public Shared Function createList_OnNewVisitEvent(cpCore As coreClass, callersCacheNameList As List(Of String)) As List(Of addonModel)
            Dim result As New List(Of addonModel)
            Try
                result = createList(cpCore, "(OnNewVisitEvent<>0)")
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Get list of addons that run on page start event
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="someCriteria"></param>
        ''' <returns></returns>
        Public Shared Function createList_OnPageStartEvent(cpCore As coreClass, callersCacheNameList As List(Of String)) As List(Of addonModel)
            Dim result As New List(Of addonModel)
            Try
                result = createList(cpCore, "(OnPageStartEvent<>0)")
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' pattern get a list of objects from this model
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="someCriteria"></param>
        ''' <returns></returns>
        Public Shared Function createList_RemoteMethods(cpCore As coreClass, callersCacheNameList As List(Of String)) As List(Of addonModel)
            Dim result As New List(Of addonModel)
            Try
                result = createList(cpCore, "(remoteMethod=1)")
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        Public Shared Function createList_pageDependencies(cpCore As coreClass, pageId As Integer) As List(Of addonModel)
            Dim result As New List(Of addonModel)
            Try
                result = createList(cpCore, "(id in (select addonId from ccAddonPageRules where (pageId=" & pageId & ")))")
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        Public Shared Function createList_templateDependencies(cpCore As coreClass, templateId As Integer) As List(Of addonModel)
            Dim result As New List(Of addonModel)
            Try
                result = createList(cpCore, "(id in (select addonId from ccAddonTemplateRules where (templateId=" & templateId & ")))")
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        Public Class addonCacheClass
            Private dictIdAddon As New Dictionary(Of Integer, addonModel)
            Private dictGuidId As New Dictionary(Of String, Integer)
            Private dictNameId As New Dictionary(Of String, Integer)
            '
            Private onBodyEndIdList As New List(Of Integer)
            Private onBodyStartIdList As New List(Of Integer)
            Private onNewVisitIdList As New List(Of Integer)
            Private OnPageEndIdList As New List(Of Integer)
            Private OnPageStartIdList As New List(Of Integer)
            Public robotsTxt As String = ""
            '
            Public Sub add(addon As addonModel)
                If (Not dictIdAddon.ContainsKey(addon.id)) Then
                    If (Not dictGuidId.ContainsKey(addon.ccguid)) Then
                        If (Not dictNameId.ContainsKey(addon.name)) Then
                            dictIdAddon.Add(addon.id, addon)
                            dictGuidId.Add(addon.ccguid.ToLower(), addon.id)
                            dictNameId.Add(addon.name.ToLower(), addon.id)
                        End If
                    End If
                End If
                If (addon.OnBodyEnd) And (Not onBodyEndIdList.Contains(addon.id)) Then
                    onBodyEndIdList.Add(addon.id)
                End If
                If (addon.OnBodyStart) And (Not onBodyStartIdList.Contains(addon.id)) Then
                    onBodyStartIdList.Add(addon.id)
                End If
                If (addon.OnNewVisitEvent) And (Not onNewVisitIdList.Contains(addon.id)) Then
                    onNewVisitIdList.Add(addon.id)
                End If
                If (addon.OnPageEndEvent) And (Not OnPageEndIdList.Contains(addon.id)) Then
                    OnPageEndIdList.Add(addon.id)
                End If
                If (addon.OnPageStartEvent) And (Not OnPageStartIdList.Contains(addon.id)) Then
                    OnPageStartIdList.Add(addon.id)
                End If
                robotsTxt &= vbCrLf & addon.RobotsTxt
            End Sub
            Public Function getAddonByGuid(guid As String) As addonModel
                If (Me.dictGuidId.ContainsKey(guid.ToLower())) Then
                    Return getAddonById(Me.dictGuidId.Item(guid.ToLower()))
                End If
                Return Nothing
            End Function
            Public Function getAddonByName(name As String) As addonModel
                If (Me.dictNameId.ContainsKey(name.ToLower())) Then
                    Return getAddonById(Me.dictNameId.Item(name.ToLower()))
                End If
                Return Nothing
            End Function
            Public Function getAddonById(addonId As Integer) As addonModel
                If (Me.dictIdAddon.ContainsKey(addonId)) Then
                    Return Me.dictIdAddon.Item(addonId)
                End If
                Return Nothing
            End Function
            '
            Private Function getAddonList(addonIdList As List(Of Integer)) As List(Of addonModel)
                Dim result As New List(Of addonModel)
                For Each addonId As Integer In addonIdList
                    result.Add(getAddonById(addonId))
                Next
                Return result
            End Function
            '
            Public Function getOnBodyEndAddonList() As List(Of addonModel)
                Return getAddonList(onBodyEndIdList)
            End Function
            '
            Public Function getOnBodyStartAddonList() As List(Of addonModel)
                Return getAddonList(onBodyStartIdList)
            End Function
            '
            Public Function getOnNewVisitAddonList() As List(Of addonModel)
                Return getAddonList(onNewVisitIdList)
            End Function
            '
            Public Function getOnPageEndAddonList() As List(Of addonModel)
                Return getAddonList(OnPageEndIdList)
            End Function
            '
            Public Function getOnPageStartAddonList() As List(Of addonModel)
                Return getAddonList(OnPageStartIdList)
            End Function
        End Class
    End Class
End Namespace
