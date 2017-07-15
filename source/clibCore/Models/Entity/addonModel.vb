
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
        Public Property BlockDefaultStyles As Boolean
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
        Public Property CustomStylesFilename As String
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
        Public Property inlineScript As String
        Public Property IsInline As Boolean
        Public Property JavaScriptBodyEnd As String
        Public Property JavaScriptOnLoad As String
        Public Property JSFilename As String
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
        Public Property StylesFilename As String
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
    End Class
End Namespace
