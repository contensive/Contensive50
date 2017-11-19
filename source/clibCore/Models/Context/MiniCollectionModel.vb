
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Newtonsoft.Json

Namespace Contensive.Core.Models.Entity
    '
    '
    '====================================================================================================
    ''' <summary>
    ''' miniCollection - This is an old collection object used in part to load the cdef part xml files. REFACTOR this into CollectionWantList and werialization into jscon
    ''' </summary>
    Public Class miniCollectionModel
        Implements ICloneable
        '
        ' Content Definitions (some data in CDef, some in the CDef extension)
        '
        Public name As String
        Public isBaseCollection As Boolean                     ' true only for the one collection created from the base file. This property does not transfer during addSrcToDst
        Public CDef As New Dictionary(Of String, Models.Complex.cdefModel)
        Public SQLIndexCnt As Integer
        Public SQLIndexes() As SQLIndexType
        Public Structure SQLIndexType
            Dim DataSourceName As String
            Dim TableName As String
            Dim IndexName As String
            Dim FieldNameList As String
            Dim dataChanged As Boolean
        End Structure
        Public MenuCnt As Integer
        Public Menus() As MenusType
        Public Structure MenusType
            Dim Name As String
            Dim IsNavigator As Boolean
            Dim menuNameSpace As String
            Dim ParentName As String
            Dim ContentName As String
            Dim LinkPage As String
            Dim SortOrder As String
            Dim AdminOnly As Boolean
            Dim DeveloperOnly As Boolean
            Dim NewWindow As Boolean
            Dim Active As Boolean
            Dim AddonName As String
            Dim dataChanged As Boolean
            Dim Guid As String
            Dim NavIconType As String
            Dim NavIconTitle As String
            Dim Key As String
        End Structure
        Public AddOns() As AddOnType
        Public Structure AddOnType
            Dim Name As String
            Dim Link As String
            Dim ObjectProgramID As String
            Dim ArgumentList As String
            Dim SortOrder As String
            Dim Copy As String
            Dim dataChanged As Boolean
        End Structure
        Public AddOnCnt As Integer
        Public Styles() As StyleType
        Public Structure StyleType
            Dim Name As String
            Dim Overwrite As Boolean
            Dim Copy As String
            Dim dataChanged As Boolean
        End Structure
        Public StyleCnt As Integer
        Public StyleSheet As String
        Public ImportCnt As Integer
        Public collectionImports() As ImportCollectionType
        Public Structure ImportCollectionType
            Dim Name As String
            Dim Guid As String
        End Structure
        Public PageTemplateCnt As Integer
        Public PageTemplates() As PageTemplateType
        '   Page Template - started, but CDef2 and LoadDataToCDef are all that is done do far
        Public Structure PageTemplateType
            Dim Name As String
            Dim Copy As String
            Dim Guid As String
            Dim Style As String
        End Structure
        Public Function Clone() As Object Implements ICloneable.Clone
            Return Me.MemberwiseClone
        End Function
    End Class
End Namespace
