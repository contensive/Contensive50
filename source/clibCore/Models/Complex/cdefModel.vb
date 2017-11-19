

Option Explicit On
Option Strict On

Imports Contensive.Core.Models
Imports Contensive.Core.Models.Context
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers

Namespace Contensive.Core.Models.Complex
    '
    ' ---------------------------------------------------------------------------------------------------
    ' ----- CDefType
    '       class not structure because it has to marshall to vb6
    ' ---------------------------------------------------------------------------------------------------
    '
    <Serializable>
    Public Class cdefModel
        '
        Public Property Id As Integer                           ' index in content table
        Public Property Name As String                       ' Name of Content
        Public Property ContentTableName As String           ' the name of the content table
        Public Property ContentDataSourceName As String      '
        Public Property AllowAdd As Boolean                  ' Allow adding records
        Public Property AllowDelete As Boolean               ' Allow deleting records
        Public Property WhereClause As String                ' Used to filter records in the admin area
        Public Property DefaultSortMethod As String          ' FieldName Direction, ....
        Public Property ActiveOnly As Boolean                ' When true
        Public Property AdminOnly As Boolean                 ' Only allow administrators to modify content
        Public Property DeveloperOnly As Boolean             ' Only allow developers to modify content
        Public Property DropDownFieldList As String          ' String used to populate select boxes
        Public Property EditorGroupName As String            ' Group of members who administer Workflow Authoring
        Public Property dataSourceId As Integer
        Private Property _dataSourceName As String = ""
        Public Property IgnoreContentControl As Boolean     ' if true, all records in the source are in this content
        Public Property AliasName As String                 ' Field Name of the required "name" field
        Public Property AliasID As String                   ' Field Name of the required "id" field
        Public Property AllowTopicRules As Boolean          ' For admin edit page
        Public Property AllowContentTracking As Boolean     ' For admin edit page
        Public Property AllowCalendarEvents As Boolean      ' For admin edit page
        Public Property dataChanged As Boolean
        Public Property includesAFieldChange As Boolean                     ' if any fields().changed, this is set true to
        Public Property Active As Boolean
        Public Property AllowContentChildTool As Boolean
        Public Property IsModifiedSinceInstalled As Boolean
        Public Property IconLink As String
        Public Property IconWidth As Integer
        Public Property IconHeight As Integer
        Public Property IconSprites As Integer
        Public Property guid As String
        Public Property IsBaseContent As Boolean
        Public Property installedByCollectionGuid As String
        Public Property parentID As Integer                  ' read from Db, if not IgnoreContentControl, the ID of the parent content
        Public Property parentName As String                 ' read from xml, used to set parentId
        Public Property TimeStamp As String                 ' string that changes if any record in Content Definition changes, in memory only
        Public Property fields As New Dictionary(Of String, Models.Complex.CDefFieldModel)
        Public Property adminColumns As New SortedList(Of String, CDefAdminColumnClass)
        Public Property ContentControlCriteria As String     ' String created from ParentIDs used to select records
        Public Property selectList As New List(Of String)
        Public Property SelectCommaList As String            ' Field list used in OpenCSContent calls (all active field definitions)
        '
        '====================================================================================================
        '
        Public Property childIdList(cpCore As coreClass) As List(Of Integer)
            Get
                If (_childIdList Is Nothing) Then
                    Dim Sql As String = "select id from cccontent where parentid=" & Id
                    Dim dt As DataTable = cpCore.db.executeQuery(Sql)
                    If dt.Rows.Count = 0 Then
                        _childIdList = New List(Of Integer)
                        For Each parentrow As DataRow In dt.Rows
                            _childIdList.Add(genericController.EncodeInteger(parentrow.Item(0)))
                        Next
                    End If
                    dt.Dispose()
                End If
                Return _childIdList
            End Get
            Set(value As List(Of Integer))
                _childIdList = value
            End Set
        End Property
        Private _childIdList As List(Of Integer) = Nothing
        '
        '====================================================================================================
        ' CDefAdminColumnType
        '
        <Serializable>
        Public Class CDefAdminColumnClass
            Public Name As String
            'Public FieldPointer As Integer
            Public Width As Integer
            Public SortPriority As Integer
            Public SortDirection As Integer
        End Class
    End Class
End Namespace
