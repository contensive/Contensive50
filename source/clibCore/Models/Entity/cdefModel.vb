

Option Explicit On
Option Strict On

Imports Contensive.Core.Models
Imports Contensive.Core.Models.Context
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers

Namespace Contensive.Core.Models.Entity



    '
    ' ---------------------------------------------------------------------------------------------------
    ' ----- CDefType
    '       class not structure because it has to marshall to vb6
    ' ---------------------------------------------------------------------------------------------------
    '
    <Serializable>
    Public Class cdefModel
        '
        Public Id As Integer                           ' index in content table
        Public Name As String                       ' Name of Content
        Public ContentTableName As String           ' the name of the content table
        Public ContentDataSourceName As String      '
        Public AuthoringTableName As String         ' the name of the authoring table
        Public AuthoringDataSourceName As String    '
        Public AllowAdd As Boolean                  ' Allow adding records
        Public AllowDelete As Boolean               ' Allow deleting records
        Public WhereClause As String                ' Used to filter records in the admin area
        Public DefaultSortMethod As String          ' FieldName Direction, ....
        Public ActiveOnly As Boolean                ' When true
        Public AdminOnly As Boolean                 ' Only allow administrators to modify content
        Public DeveloperOnly As Boolean             ' Only allow developers to modify content
        Public AllowWorkflowAuthoring As Boolean    ' if true, treat this content with authoring proceses
        Public DropDownFieldList As String          ' String used to populate select boxes
        Public EditorGroupName As String            ' Group of members who administer Workflow Authoring
        Public dataSourceId As Integer
        Private _dataSourceName As String = ""
        '
        Public IgnoreContentControl As Boolean     ' if true, all records in the source are in this content
        Public AliasName As String                 ' Field Name of the required "name" field
        Public AliasID As String                   ' Field Name of the required "id" field
        '
        Public AllowTopicRules As Boolean          ' For admin edit page
        Public AllowContentTracking As Boolean     ' For admin edit page
        Public AllowCalendarEvents As Boolean      ' For admin edit page
        Public AllowMetaContent As Boolean         ' For admin edit page - Adds the Meta Content Section
        '
        Public dataChanged As Boolean
        Public includesAFieldChange As Boolean                     ' if any fields().changed, this is set true to
        Public Active As Boolean
        Public AllowContentChildTool As Boolean
        Public IsModifiedSinceInstalled As Boolean
        Public IconLink As String
        Public IconWidth As Integer
        Public IconHeight As Integer
        Public IconSprites As Integer
        Public guid As String
        Public IsBaseContent As Boolean
        Public installedByCollectionGuid As String
        '
        ' fields stored differently in xml collection files
        '   name is loaded from xml collection files 
        '   id is created during the cacheLoad process when loading from Db (and used in metaData)
        '
        Public parentID As Integer                  ' read from Db, if not IgnoreContentControl, the ID of the parent content
        Public parentName As String                 ' read from xml, used to set parentId
        '
        ' calculated after load
        '
        Public TimeStamp As String                 ' string that changes if any record in Content Definition changes, in memory only
        Public fields As New Dictionary(Of String, CDefFieldModel)
        ' -- !!!!! changed to string because dotnet json cannot serialize an integer key
        Public adminColumns As New SortedList(Of String, CDefAdminColumnClass)
        Public ContentControlCriteria As String     ' String created from ParentIDs used to select records
        Public selectList As New List(Of String)
        Public SelectCommaList As String            ' Field list used in OpenCSContent calls (all active field definitions)
        'Public childIdList As New List(Of Integer)      ' Comma separated list of child content definition IDs
        '
        Public Property childIdList(cpCore As coreClass) As List(Of Integer)
            Get
                If (_childIdList Is Nothing) Then
                    Dim Sql As String = "select id from cccontent where parentid=" & Id
                    Dim dt As DataTable = cpCore.db.executeSql(Sql)
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
        ' ---------------------------------------------------------------------------------------------------
        ' ----- CDefAdminColumnType
        ' ---------------------------------------------------------------------------------------------------
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
