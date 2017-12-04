using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

//Option Explicit On
//Option Strict On

//Imports System
//Imports System.Collections.Generic
//Imports System.Text
//Imports Contensive.BaseClasses
//Imports Contensive.Core.Controllers
//Imports Newtonsoft.Json

//namespace Contensive.Core.Models.Entity
//    Public Class xSharedStylesTemplateRuleModel
//        Inherits baseModel
//        '
//        '====================================================================================================
//        '-- const
//        Public Const contentName As String = "Shared Styles Template Rules"
//        Public Const contentTableName As String = "ccSharedStylesTemplateRules"
//        Private Shadows Const contentDataSource As String = "default"
//        '
//        '====================================================================================================
//        ' -- instance properties
//        Public Property StyleID As Integer
//        Public Property TemplateID As Integer
//        '
//        '====================================================================================================
//        Public Overloads Shared Function add(cpCore As coreClass) As SharedStylesTemplateRuleModel
//            Return add(Of SharedStylesTemplateRuleModel)(cpCore)
//        End Function
//        '
//        '====================================================================================================
//        Public Overloads Shared Function add(cpCore As coreClass, ByRef callersCacheNameList As List(Of String)) As SharedStylesTemplateRuleModel
//            Return add(Of SharedStylesTemplateRuleModel)(cpCore, callersCacheNameList)
//        End Function
//        '
//        '====================================================================================================
//        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer) As SharedStylesTemplateRuleModel
//            Return create(Of SharedStylesTemplateRuleModel)(cpCore, recordId)
//        End Function
//        '
//        '====================================================================================================
//        Public Overloads Shared Function create(cpCore As coreClass, recordId As Integer, ByRef callersCacheNameList As List(Of String)) As SharedStylesTemplateRuleModel
//            Return create(Of SharedStylesTemplateRuleModel)(cpCore, recordId, callersCacheNameList)
//        End Function
//        '
//        '====================================================================================================
//        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String) As SharedStylesTemplateRuleModel
//            Return create(Of SharedStylesTemplateRuleModel)(cpCore, recordGuid)
//        End Function
//        '
//        '====================================================================================================
//        Public Overloads Shared Function create(cpCore As coreClass, recordGuid As String, ByRef callersCacheNameList As List(Of String)) As SharedStylesTemplateRuleModel
//            Return create(Of SharedStylesTemplateRuleModel)(cpCore, recordGuid, callersCacheNameList)
//        End Function
//        '
//        '====================================================================================================
//        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String) As SharedStylesTemplateRuleModel
//            Return createByName(Of SharedStylesTemplateRuleModel)(cpCore, recordName)
//        End Function
//        '
//        '====================================================================================================
//        Public Overloads Shared Function createByName(cpCore As coreClass, recordName As String, ByRef callersCacheNameList As List(Of String)) As SharedStylesTemplateRuleModel
//            Return createByName(Of SharedStylesTemplateRuleModel)(cpCore, recordName, callersCacheNameList)
//        End Function
//        '
//        '====================================================================================================
//        Public Overloads Sub save(cpCore As coreClass)
//            MyBase.save(cpCore)
//        End Sub
//        '
//        '====================================================================================================
//        Public Overloads Shared Sub delete(cpCore As coreClass, recordId As Integer)
//            delete(Of SharedStylesTemplateRuleModel)(cpCore, recordId)
//        End Sub
//        '
//        '====================================================================================================
//        Public Overloads Shared Sub delete(cpCore As coreClass, ccGuid As String)
//            delete(Of SharedStylesTemplateRuleModel)(cpCore, ccGuid)
//        End Sub
//        '
//        '====================================================================================================
//        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String, callersCacheNameList As List(Of String)) As List(Of SharedStylesTemplateRuleModel)
//            Return createList(Of SharedStylesTemplateRuleModel)(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList)
//        End Function
//        '
//        '====================================================================================================
//        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String, sqlOrderBy As String) As List(Of SharedStylesTemplateRuleModel)
//            Return createList(Of SharedStylesTemplateRuleModel)(cpCore, sqlCriteria, sqlOrderBy)
//        End Function
//        '
//        '====================================================================================================
//        Public Overloads Shared Function createList(cpCore As coreClass, sqlCriteria As String) As List(Of SharedStylesTemplateRuleModel)
//            Return createList(Of SharedStylesTemplateRuleModel)(cpCore, sqlCriteria)
//        End Function
//        '
//        '====================================================================================================
//        Public Overloads Sub invalidatePrimaryCache(cpCore As coreClass, recordId As Integer)
//            invalidateCacheSingleRecord(Of SharedStylesTemplateRuleModel)(cpCore, recordId)
//        End Sub
//        '
//        '====================================================================================================
//        Public Overloads Shared Function getRecordName(cpcore As coreClass, recordId As Integer) As String
//            Return baseModel.getRecordName(Of SharedStylesTemplateRuleModel)(cpcore, recordId)
//        End Function
//        '
//        '====================================================================================================
//        Public Overloads Shared Function getRecordName(cpcore As coreClass, ccGuid As String) As String
//            Return baseModel.getRecordName(Of SharedStylesTemplateRuleModel)(cpcore, ccGuid)
//        End Function
//        '
//        '====================================================================================================
//        Public Overloads Shared Function getRecordId(cpcore As coreClass, ccGuid As String) As Integer
//            Return baseModel.getRecordId(Of SharedStylesTemplateRuleModel)(cpcore, ccGuid)
//        End Function
//        '
//        '====================================================================================================
//        Public Overloads Shared Function createDefault(cpcore As coreClass) As SharedStylesTemplateRuleModel
//            Return createDefault(Of SharedStylesTemplateRuleModel)(cpcore)
//        End Function
//    End Class
//End Namespace
