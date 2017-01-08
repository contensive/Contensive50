
Option Explicit On
Option Strict On

Namespace Contensive.Core
    Public Class coreMetaDataClass
        Implements IDisposable
        '
        '------------------------------------------------------------------------------------------------------------------------
        ' objects passed in that are not disposed
        '------------------------------------------------------------------------------------------------------------------------
        '
        Private cpCore As coreClass
        '
        '------------------------------------------------------------------------------------------------------------------------
        ' objects to destruct during dispose
        '------------------------------------------------------------------------------------------------------------------------
        '
        Private cdefList As Dictionary(Of Integer, CDefClass)
        Private cdefNameIdXref As Dictionary(Of String, Integer)
        Private tableSchemaList As Dictionary(Of String, tableSchemaClass)
        '
        '====================================================================================================
        ''' <summary>
        ''' Serializable classes that define the collection file structure. Originally in XML, now converting to json.
        ''' </summary>
        <Serializable>
        Public Class collectionFileWishListClass
            Public name As String
            Public guid As String
            Public system As Boolean
            Public Class addonClass
                Public copy As String
                Public copyText As String
                Public activeXProgramId As String
                Public dotNetClass As String
                Public argumentList As String
                Public asAjax As Boolean
                Public filter As Boolean
                Public help As String
                Public helpLink As String
                Public iconLInk As String
                Public icons As List(Of iconClass)
                Public inIFrame As Boolean
                Public blockEditTools As Boolean
                Public formXml As String
                Public isInLine As Boolean
                Public javascriptOnLoad As String
                Public javascriptINHead As String
                Public javascriptBodyEnd As String
                Public metaDescription As String
                Public otherHeadTags As String
                Public content As Boolean
                Public template As Boolean
                Public email As Boolean
                Public admin As Boolean
                Public onPageEndEvent As Boolean
                Public onPageStartEvent As Boolean
                Public onBodyStart As Boolean
                Public onBodyEnd As Boolean
                Public remoteMethod As Boolean
                Public processRunOnce As Boolean
                Public processInterval As Boolean
                Public pageTitle As String
                Public remoteAssetLink As String
                Public styles As String
                Public Class scriptingClass
                    Public language As String
                    Public entryPoint As String
                    Public timeout As Integer
                End Class
                Public Scriptings As List(Of scriptingClass)
            End Class            '
            Public Class iconClass
                Public link As String
                Public width As Integer
                Public height As Integer
                Public sprits As Integer
            End Class            '
            Public Class cdefClass
                Public Name As String
                Public ActiveOnly As Boolean
                Public AliasID As Integer
                Public AliasName As String
                Public AllowAdd As Boolean
                Public AllowContentTracking As Boolean
                Public AllowDelete As Boolean
                Public AllowTopicRules As Boolean
                Public AllowWorkflowAuthoring As Boolean
                Public AuthoringDataSourceName As String
                Public AuthoringTableName As String
                Public ContentDataSourceName As String
                Public ContentTableName As String
                Public ccDataSources As String
                Public DefaultSortMethod As String
                Public DeveloperOnly As String
                Public DropDownFieldList As String
                Public EditorGroupName As String
                Public IgnoreContentControl As Boolean
                Public Parent As String
                Public AllowMetaContent As Boolean
                Public AllowContentChildTool As Boolean
                Public NavIconType As String
                Public icon As iconClass
                Public guid As String
                Public Class cdefFieldClass
                    Public Name As String
                    Public active As Boolean
                    Public AdminOnly As Boolean
                    Public Authorable As Boolean
                    Public Caption As String
                    Public DeveloperOnly As Boolean
                    Public EditSortPriority As String
                    Public FieldType As String
                    Public HTMLContent As Boolean
                    Public IndexColumn As Integer
                    Public IndexSortDirection As Boolean
                    Public IndexSortOrder As String
                    Public IndexWidth As String
                    Public LookupContent As String
                    Public NotEditable As Boolean
                    Public Password As Boolean
                    Public ReadOnlyField As Boolean
                    Public RedirectContent As String
                    Public RedirectID As Integer
                    Public RedirectPath As String
                    Public Required As Boolean
                    Public TextBuffered As Boolean
                    Public UniqueName As Boolean
                    Public DefaultValue As String
                    Public RSSTitle As String
                    Public RSSDescription As String
                    Public MemberSelectGroupID As Integer
                    Public EditTab As String
                    Public Scramble As Boolean
                    Public LookupList As String
                    Public ManyToManyContent As String
                    Public ManyToManyRuleContent As String
                    Public ManyToManyRulePrimaryField As String
                    Public ManyToManyRuleSecondaryField As String
                End Class
                Public fields As List(Of cdefFieldClass)
                Public Class sqlIndexFlass
                    Public indexName As String
                    Public dataSourceName As String
                    Public tableName As String
                    Public filenameList As String
                End Class
                Public sqlIndexes As List(Of sqlIndexFlass)
            End Class
            Public addons As List(Of addonClass)
            Public Class navigatorClass
                Public Name As String
                Public NavigatorNamespace As String
                Public navIconTitle As String
                Public NavIconType As String
                Public LinkPage As String
                Public ContentName As String
                Public AdminOnly As Boolean
                Public DeveloperOnly As Boolean
                Public NewWindow As Boolean
                Public Active As Boolean
                Public AddonName As String
                Public guid As String
            End Class
            Public NavigatorEntries As List(Of navigatorClass)
            Public Class recordClass
                Public content As String
                Public guid As String
                Public name As String
                Public Class recordFieldClass
                    Public name As String
                    Public value As String
                End Class
                Public fields As List(Of recordFieldClass)
            End Class
            Public data As List(Of recordClass)
            Public Class importCollectionClass
                Public name As String
                Public value As String
            End Class
            Public ImportCollections As List(Of importCollectionClass)
        End Class
        ''
        'Structure NavEntryType
        '    Dim NavigatorID As Integer
        '    Dim ContentName As String
        'End Structure
        ''
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
        '
        ' ---------------------------------------------------------------------------------------------------
        ' ----- CDefFieldClass
        '       class not structure because it has to marshall to vb6
        ' ---------------------------------------------------------------------------------------------------
        '
        <Serializable>
        Public Class CDefFieldClass
            Implements ICloneable
            Implements IComparable

            Public nameLc As String                      ' The name of the field
            'Public ValueVariant As Object             ' The value carried to and from the database
            Public id As Integer                          ' the ID in the ccContentFields Table that this came from
            Public active As Boolean                   ' if the field is available in the admin area
            Public fieldTypeId As Integer                   ' The type of data the field holds
            Public caption As String                   ' The caption for displaying the field
            Public [ReadOnly] As Boolean            ' was ReadOnly -- If true, this field can not be written back to the database
            Public NotEditable As Boolean              ' if true, you can only edit new records
            Public Required As Boolean                 ' if true, this field must be entered
            Public defaultValue As String         ' default value on a new record
            Public HelpMessage As String               ' explaination of this field
            Public UniqueName As Boolean               '
            Public TextBuffered As Boolean             ' if true, the input is run through RemoveControlCharacters()
            Public Password As Boolean                 ' for text boxes, sets the password attribute
            Public RedirectID As String                ' If TYPEREDIRECT, this is the field that must match ID of this record
            Public RedirectPath As String              ' New Field, If TYPEREDIRECT, this is the path to the next page (if blank, current page is used)
            Public indexColumn As Integer                 ' the column desired in the admin index form
            Public indexWidth As String                ' either number or percentage, blank if not included 
            Public indexSortOrder As Integer            ' alpha sort on index page
            Public indexSortDirection As Integer          ' 1 sorts forward, -1 backward
            'Public Changed As Boolean                  ' if true, field value needs to be saved to database
            Public adminOnly As Boolean                ' This field is only available to administrators
            Public developerOnly As Boolean            ' This field is only available to administrators
            Public blockAccess As Boolean              ' ***** Field Reused to keep binary compatiblity - "IsBaseField" - if true this is a CDefBase field
            Public htmlContent As Boolean              ' if true, the HTML editor (active edit) can be used
            Public authorable As Boolean               ' true if it can be seen in the admin form
            Public inherited As Boolean                ' if true, this field takes its values from a parent, see ContentID
            Public contentId As Integer                   ' This is the ID of the Content Def that defines these properties
            Public editSortPriority As Integer            ' The Admin Edit Sort Order
            Public ManyToManyRulePrimaryField As String     ' Rule Field Name for Primary Table
            Public ManyToManyRuleSecondaryField As String   ' Rule Field Name for Secondary Table
            Public RSSTitleField As Boolean             ' When creating RSS fields from this content, this is the title
            Public RSSDescriptionField As Boolean       ' When creating RSS fields from this content, this is the description
            Public editTabName As String                   ' Editing group - used for the tabs
            Public Scramble As Boolean                 ' save the field scrambled in the Db
            Public lookupList As String                ' If TYPELOOKUP, and LookupContentID is null, this is a comma separated list of choices
            'Public AllowContentTracking As Boolean      ' tmp 
            '
            Public dataChanged As Boolean
            Public isBaseField As Boolean
            Public isModifiedSinceInstalled As Boolean
            Public installedByCollectionGuid As String
            Public HelpDefault As String
            Public HelpCustom As String
            Public HelpChanged As Boolean
            '
            ' fields stored differently in xml collection files
            '   name is loaded from xml collection files 
            '   id is created during the cacheLoad process when loading from Db (and used in metaData)
            '
            Public lookupContentName As String
            Public lookupContentID As Integer             ' If TYPELOOKUP, (for Db controled sites) this is the content ID of the source table
            Public RedirectContentName As String
            Public RedirectContentID As Integer           ' If TYPEREDIRECT, this is new contentID
            Public ManyToManyContentName As String
            Public manyToManyContentID As Integer         ' Content containing Secondary Records
            Public ManyToManyRuleContentName As String
            Public manyToManyRuleContentID As Integer     ' Content with rules between Primary and Secondary
            Public MemberSelectGroupName As String ' If the Type is TypeMemberSelect, this is the group that the member will be selected from
            Public MemberSelectGroupID As Integer
            '
            Public Function Clone() As Object Implements ICloneable.Clone
                Return Me.MemberwiseClone
            End Function
            '
            Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
                Dim c As CDefFieldClass = CType(obj, CDefFieldClass)
                Return String.Compare(Me.nameLc.ToLower, c.nameLc.ToLower)
            End Function
        End Class
        '
        ' ---------------------------------------------------------------------------------------------------
        ' ----- CDefType
        '       class not structure because it has to marshall to vb6
        ' ---------------------------------------------------------------------------------------------------
        '
        <Serializable>
        Public Class CDefClass
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
            Public fields As New Dictionary(Of String, CDefFieldClass)
            Public adminColumns As New SortedList(Of Integer, CDefAdminColumnClass)
            Public ContentControlCriteria As String     ' String created from ParentIDs used to select records
            Public selectList As New List(Of String)
            Public SelectCommaList As String            ' Field list used in OpenCSContent calls (all active field definitions)
            Public childIdList As New List(Of Integer)      ' Comma separated list of child content definition IDs
        End Class
        '
        ' ----- Table Schema caching to speed up update
        '
        Public Class tableSchemaClass
            Public TableName As String
            Public Dirty As Boolean
            Public columns As List(Of String)
            ' list of all indexes, with the field it covers
            Public indexes As List(Of String)
        End Class
        '
        '==========================================================================================
        ''' <summary>
        ''' initialize the cdef cache. All loads are on-demand so just setup. constructed during appServices constructor so it cannot use core.app yet
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="cluster"></param>
        ''' <param name="appName"></param>
        ''' <remarks></remarks>
        public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpCore = cpCore
            '
            ' reset metaData
            '
            cdefList = New Dictionary(Of Integer, CDefClass)
            cdefNameIdXref = Nothing
            tableSchemaList = Nothing
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' get content id from content name
        ''' </summary>
        ''' <param name="contentName"></param>
        ''' <returns></returns>
        Public Function getContentId(contentName As String) As Integer
            Dim returnId As Integer = 0
            Try
                If (cdefNameIdXref Is Nothing) Then
                    Call cdefNameIdXref_load()
                End If
                If (cdefNameIdXref.ContainsKey(contentName.ToLower)) Then
                    returnId = cdefNameIdXref(contentName.ToLower)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnId
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' get Cdef from content name. If the cdef is not found, return nothing.
        ''' </summary>
        ''' <param name="contentName"></param>
        ''' <returns></returns>
        Public Function getCdef(contentName As String) As CDefClass
            Dim returnCdef As CDefClass = Nothing
            Try
                Dim ContentId As Integer
                ContentId = getContentId(contentName)
                If (ContentId < 0) Then
                    Throw New ApplicationException("No metadata was found for content name [" & contentName & "]")
                Else
                    returnCdef = getCdef(ContentId)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnCdef
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' load cdefNameIdXref from cache and/or Db
        ''' </summary>
        Public Sub cdefNameIdXref_load()
            Try
                Dim dt As DataTable
                Dim recordName As String
                '
                ' load xref from cache
                '
                Try
                    '
                    ' catch cache issues so cdef will load
                    '
                    cdefNameIdXref = cpCore.cache.getObject(Of Dictionary(Of String, Integer))("cdefNameIdXref")
                    'cdefNameIdXref = DirectCast(cpCore.cache.getObject(Of Dictionary(Of String, Integer))("cdefNameIdXref"), Dictionary(Of String, Integer))
                Catch ex As Exception
                    cpCore.handleExceptionAndNoThrow(ex)
                End Try
                If (cdefNameIdXref Is Nothing) OrElse (cdefNameIdXref.Count = 0) Then
                    '
                    ' load xref from Db
                    '
                    cdefNameIdXref = New Dictionary(Of String, Integer)
                    dt = cpCore.db.executeSql("select id,name from ccContent where (active<>0)")
                    If dt.Rows.Count > 0 Then
                        For Each row As DataRow In dt.Rows
                            recordName = EncodeText(row.Item("name")).ToLower
                            If Not cdefNameIdXref.ContainsKey(recordName) Then
                                cdefNameIdXref.Add(recordName, EncodeInteger(row.Item("id")))
                            End If
                        Next
                    End If
                    dt.Dispose()
                    Try
                        Call cpCore.cache.setKey("cdefNameIdXref", cdefNameIdXref, "content")
                    Catch ex As Exception
                        cpCore.handleExceptionAndNoThrow(ex)
                    End Try
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' return a cdef class from content id
        ''' </summary>
        ''' <param name="contentId"></param>
        ''' <returns></returns>
        Public Function getCdef(contentId As Integer, Optional forceDbLoad As Boolean = False, Optional loadInvalidFields As Boolean = False) As CDefClass
            Dim returnCdef As CDefClass = Nothing
            Try
                Dim sql As String
                Dim dt As DataTable
                Dim contentName As String
                'Dim contentId As Integer
                Dim contentTablename As String
                Dim field As CDefFieldClass
                Dim row As DataRow
                Dim fieldId As Integer
                Dim fieldTypeId As Integer
                Dim fieldHtmlContent As Boolean
                'Dim fieldActive As Boolean
                Dim fieldName As String
                Dim parentCdef As CDefClass
                '
                If contentId = 38 Then
                    contentId = contentId
                End If
                If (contentId <= 0) Then
                    '
                    ' invalid id
                    '
                ElseIf (Not forceDbLoad) And (cdefList.ContainsKey(contentId)) Then
                    '
                    ' already loaded and no force re-load, just return the current cdef
                    '
                    returnCdef = cdefList.Item(contentId)
                Else
                    If (cdefList.ContainsKey(contentId)) Then
                        '
                        ' key is already there, remove it first
                        '
                        cdefList.Remove(contentId)
                    End If
                    '
                    ' load cache version
                    '
                    '
                    If (Not forceDbLoad) Then
                        Try
                            returnCdef = cpCore.cache.getObject(Of CDefClass)("cdefId" & contentId.ToString)
                        Catch ex As Exception
                            cpCore.handleExceptionAndNoThrow(ex)
                        End Try
                    End If
                    If returnCdef Is Nothing Then
                        '
                        ' load Db version
                        '
                        sql = "SELECT " _
                                & "c.ID" _
                                & ", c.Name" _
                                & ", c.name" _
                                & ", c.AllowAdd" _
                                & ", c.DeveloperOnly" _
                                & ", c.AdminOnly" _
                                & ", c.AllowDelete" _
                                & ", c.ParentID" _
                                & ", c.DefaultSortMethodID" _
                                & ", c.DropDownFieldList" _
                                & ", ContentTable.Name AS ContentTableName" _
                                & ", ContentDataSource.Name AS ContentDataSourceName" _
                                & ", AuthoringTable.Name AS AuthoringTableName" _
                                & ", AuthoringDataSource.Name AS AuthoringDataSourceName" _
                                & ", c.AllowWorkflowAuthoring AS AllowWorkflowAuthoring" _
                                & ", c.AllowCalendarEvents as AllowCalendarEvents" _
                                & ", ContentTable.DataSourceID" _
                                & ", ccSortMethods.OrderByClause as DefaultSortMethod" _
                                & ", ccGroups.Name as EditorGroupName" _
                                & ", c.AllowContentTracking as AllowContentTracking" _
                                & ", c.AllowTopicRules as AllowTopicRules" _
                                & ", c.AllowContentTracking as AllowContentTracking" _
                                & " from (((((ccContent c" _
                                & " left join ccTables AS ContentTable ON c.ContentTableID = ContentTable.ID)" _
                                & " left join ccTables AS AuthoringTable ON c.AuthoringTableID = AuthoringTable.ID)" _
                                & " left join ccDataSources AS ContentDataSource ON ContentTable.DataSourceID = ContentDataSource.ID)" _
                                & " left join ccDataSources AS AuthoringDataSource ON AuthoringTable.DataSourceID = AuthoringDataSource.ID)" _
                                & " left join ccSortMethods ON c.DefaultSortMethodID = ccSortMethods.ID)" _
                                & " left join ccGroups ON c.EditorGroupID = ccGroups.ID" _
                                & " where (c.Active<>0)" _
                                & " and(c.id=" & contentId.ToString & ")"
                        dt = cpCore.db.executeSql(sql)
                        If dt.Rows.Count = 0 Then
                            '
                            ' cdef not found
                            '
                        Else
                            returnCdef = New CDefClass
                            With returnCdef
                                .fields = New Dictionary(Of String, CDefFieldClass)
                                .childIdList = New List(Of Integer)
                                .selectList = New List(Of String)
                                .adminColumns = New SortedList(Of Integer, CDefAdminColumnClass)
                                '
                                ' ----- save values in definition
                                '
                                row = dt.Rows(0)
                                contentName = Trim(EncodeText(row.Item(1)))
                                'contentId = EncodeInteger(row.Item(0))
                                contentTablename = EncodeText(row.Item(10))
                                .Name = contentName
                                .Id = contentId
                                .AllowAdd = EncodeBoolean(row.Item(3))
                                .DeveloperOnly = EncodeBoolean(row.Item(4))
                                .AdminOnly = EncodeBoolean(row.Item(5))
                                .AllowDelete = EncodeBoolean(row.Item(6))
                                .parentID = EncodeInteger(row.Item(7))
                                .DropDownFieldList = vbUCase(EncodeText(row.Item(9)))
                                .ContentTableName = EncodeText(contentTablename)
                                .ContentDataSourceName = "default"
                                .AuthoringDataSourceName = "default"
                                .AuthoringTableName = EncodeText(row.Item(12))
                                .AllowWorkflowAuthoring = EncodeBoolean(row.Item(14))
                                .AllowCalendarEvents = EncodeBoolean(row.Item(15))
                                .DefaultSortMethod = EncodeText(row.Item(17))
                                If .DefaultSortMethod = "" Then
                                    .DefaultSortMethod = "name"
                                End If
                                .EditorGroupName = EncodeText(row.Item(18))
                                .AllowContentTracking = EncodeBoolean(row.Item(19))
                                .AllowTopicRules = EncodeBoolean(row.Item(20))
                                .AllowMetaContent = EncodeBoolean(row.Item(21))
                                '
                                .ActiveOnly = True
                                .AliasID = "ID"
                                .AliasName = "NAME"
                                .IgnoreContentControl = False
                                '
                                ' load parent cdef fields first so we can overlay the current cdef field
                                '
                                If .parentID = 0 Then
                                    .parentID = -1
                                Else
                                    parentCdef = getCdef(.parentID, forceDbLoad)
                                    For Each keyvaluepair In parentCdef.fields
                                        Dim parentField As CDefFieldClass = keyvaluepair.Value
                                        Dim childField As New CDefFieldClass
                                        childField = DirectCast(parentField.Clone, CDefFieldClass)
                                        childField.inherited = True
                                        .fields.Add(childField.nameLc.ToLower, childField)
                                        If Not ((parentField.fieldTypeId = FieldTypeIdManyToMany) Or (parentField.fieldTypeId = FieldTypeIdRedirect)) Then
                                            If Not .selectList.Contains(parentField.nameLc) Then
                                                .selectList.Add(parentField.nameLc)
                                            End If
                                        End If
                                    Next
                                End If
                                '
                                ' append child cdef
                                '
                                sql = "select id from cccontent where parentid=" & .Id
                                dt = cpCore.db.executeSql(sql)
                                If dt.Rows.Count = 0 Then
                                    For Each parentrow As DataRow In dt.Rows
                                        .childIdList.Add(EncodeInteger(parentrow.Item(0)))
                                    Next
                                End If
                                dt.Dispose()
                                '
                                ' ----- now load all the Content Definition Fields
                                '
                                sql = "SELECT" _
                                    & " f.DeveloperOnly" _
                                    & ",f.UniqueName" _
                                    & ",f.TextBuffered" _
                                    & ",f.Password" _
                                    & ",f.IndexColumn" _
                                    & ",f.IndexWidth" _
                                    & ",f.IndexSortPriority" _
                                    & ",f.IndexSortDirection" _
                                    & ",f.AdminOnly" _
                                    & ",f.SortOrder" _
                                    & ",f.EditSortPriority" _
                                    & ",f.ContentID" _
                                    & ",f.ID" _
                                    & ",f.Name" _
                                    & ",f.Required" _
                                    & ",f.Type" _
                                    & ",f.Caption" _
                                    & ",f.readonly" _
                                    & ",f.LookupContentID" _
                                    & ",f.RedirectContentID" _
                                    & ",f.RedirectPath" _
                                    & ",f.RedirectID" _
                                    & ",f.DefaultValue" _
                                    & ",'' as HelpMessageDeprecated" _
                                    & ",f.Active" _
                                    & ",f.HTMLContent" _
                                    & ",f.NotEditable" _
                                    & ",f.authorable" _
                                    & ",f.ManyToManyContentID" _
                                    & ",f.ManyToManyRuleContentID" _
                                    & ",f.ManyToManyRulePrimaryField" _
                                    & ",f.ManyToManyRuleSecondaryField" _
                                    & ",f.RSSTitleField" _
                                    & ",f.RSSDescriptionField" _
                                    & ",f.EditTab" _
                                    & ",f.Scramble" _
                                    & ",f.MemberSelectGroupID" _
                                    & ",f.LookupList" _
                                    & ",f.IsBaseField" _
                                    & ",f.InstalledByCollectionID" _
                                    & ",h.helpDefault" _
                                    & ",h.helpCustom" _
                                    & "" _
                                    & " from ((ccFields f" _
                                    & " left join ccContent c ON f.ContentID = c.ID)" _
                                    & " left join ccfieldHelp h on h.fieldid=f.id)" _
                                    & "" _
                                    & " where" _
                                    & " (c.ID Is not Null)" _
                                    & " and(c.Active<>0)" _
                                    & " and(c.ID=" & contentId & ")" _
                                    & "" _
                                    & ""
                                '
                                If Not loadInvalidFields Then
                                    sql &= "" _
                                        & " and(f.active<>0)" _
                                        & " and(f.Type<>0)" _
                                        & " and(f.name <>'')" _
                                        & ""
                                End If
                                sql &= "" _
                                    & " order by" _
                                    & " f.ContentID,f.EditTab,f.EditSortPriority" _
                                    & ""
                                dt = cpCore.db.executeSql(sql)
                                If dt.Rows.Count = 0 Then
                                    '
                                Else
                                    Dim usedFields As New List(Of String)
                                    For Each row In dt.Rows
                                        Dim skipDuplicateField As Boolean = False
                                        fieldName = EncodeText(row.Item(13))
                                        fieldId = EncodeInteger(row.Item(12))
                                        Dim fieldNameLower As String = fieldName.ToLower()
                                        If usedFields.Contains(fieldNameLower) Then
                                            '
                                            ' this is a dup field for this content (not accounting for possibleinherited field) - keep the one with the lowest id
                                            '
                                            If .fields(fieldNameLower).id < fieldId Then
                                                '
                                                ' this new field has a higher id, skip it
                                                '
                                                skipDuplicateField = True
                                            Else
                                                '
                                                ' this new field has a lower id, remove the other one
                                                '
                                                .fields.Remove(fieldNameLower)
                                            End If
                                        End If
                                        If Not skipDuplicateField Then
                                            '
                                            ' only add the first field found, ordered by id
                                            '
                                            If (.fields.ContainsKey(fieldNameLower)) Then
                                                '
                                                ' remove inherited field and replace it with field from this table
                                                '
                                                .fields.Remove(fieldNameLower)
                                            End If
                                            field = New CDefFieldClass
                                            With field
                                                Dim fieldIndexColumn As Integer
                                                fieldTypeId = EncodeInteger(row.Item(15))
                                                If (EncodeText(row.Item(4)) = "") Then
                                                    fieldIndexColumn = -1
                                                Else
                                                    fieldIndexColumn = EncodeInteger(row.Item(4))
                                                End If
                                                '
                                                ' translate htmlContent to fieldtypehtml
                                                '   this is also converted in upgrade, daily housekeep, addon install
                                                '
                                                fieldHtmlContent = EncodeBoolean(row.Item(25))
                                                If fieldHtmlContent Then
                                                    If fieldTypeId = FieldTypeIdLongText Then
                                                        fieldTypeId = FieldTypeIdHTML
                                                    ElseIf fieldTypeId = FieldTypeIdFileTextPrivate Then
                                                        fieldTypeId = FieldTypeIdFileHTMLPrivate
                                                    End If
                                                End If
                                                .active = EncodeBoolean(row.Item(24))
                                                .adminOnly = EncodeBoolean(row.Item(8))
                                                .authorable = EncodeBoolean(row.Item(27))
                                                .blockAccess = EncodeBoolean(row.Item(38))
                                                .caption = EncodeText(row.Item(16))
                                                .dataChanged = False
                                                '.Changed
                                                .contentId = contentId
                                                .defaultValue = EncodeText(row.Item(22))
                                                .developerOnly = EncodeBoolean(row.Item(0))
                                                .editSortPriority = EncodeInteger(row.Item(10))
                                                .editTabName = EncodeText(row.Item(34))
                                                .fieldTypeId = fieldTypeId
                                                .htmlContent = fieldHtmlContent
                                                .id = fieldId
                                                .indexColumn = fieldIndexColumn
                                                .indexSortDirection = EncodeInteger(row.Item(7))
                                                .indexSortOrder = EncodeInteger(row.Item(6))
                                                .indexWidth = EncodeText(EncodeInteger(Replace(EncodeText(row.Item(5)), "%", "")))
                                                .inherited = False
                                                .installedByCollectionGuid = EncodeText(row.Item(39))
                                                .isBaseField = EncodeBoolean(row.Item(38))
                                                .isModifiedSinceInstalled = False
                                                .lookupContentID = EncodeInteger(row.Item(18))
                                                .lookupContentName = ""
                                                .lookupList = EncodeText(row.Item(37))
                                                .manyToManyContentID = EncodeInteger(row.Item(28))
                                                .manyToManyRuleContentID = EncodeInteger(row.Item(29))
                                                .ManyToManyRulePrimaryField = EncodeText(row.Item(30))
                                                .ManyToManyRuleSecondaryField = EncodeText(row.Item(31))
                                                .ManyToManyContentName = ""
                                                .ManyToManyRuleContentName = ""
                                                .MemberSelectGroupID = EncodeInteger(row.Item(36))
                                                .MemberSelectGroupName = ""
                                                .nameLc = fieldNameLower
                                                .NotEditable = EncodeBoolean(row.Item(26))
                                                .Password = EncodeBoolean(row.Item(3))
                                                .ReadOnly = EncodeBoolean(row.Item(17))
                                                .RedirectContentID = EncodeInteger(row.Item(19))
                                                .RedirectContentName = ""
                                                .RedirectID = EncodeText(row.Item(21))
                                                .RedirectPath = EncodeText(row.Item(20))
                                                .Required = EncodeBoolean(row.Item(14))
                                                .RSSTitleField = EncodeBoolean(row.Item(32))
                                                .RSSDescriptionField = EncodeBoolean(row.Item(33))
                                                .Scramble = EncodeBoolean(row.Item(35))
                                                .TextBuffered = EncodeBoolean(row.Item(2))
                                                .UniqueName = EncodeBoolean(row.Item(1))
                                                '.ValueVariant
                                                '
                                                .HelpCustom = EncodeText(row.Item(41))
                                                .HelpDefault = EncodeText(row.Item(40))
                                                If .HelpCustom = "" Then
                                                    .HelpMessage = .HelpDefault
                                                Else
                                                    .HelpMessage = .HelpCustom
                                                End If
                                                .HelpChanged = False
                                                If .lookupContentID > 0 Then
                                                    dt = cpCore.db.executeSql("select name from cccontent where id=" & .lookupContentID)
                                                    If dt.Rows.Count > 0 Then
                                                        .lookupContentName = EncodeText(dt.Rows(0).Item(0))
                                                    End If
                                                End If
                                                If .manyToManyContentID > 0 Then
                                                    dt = cpCore.db.executeSql("select name from cccontent where id=" & .manyToManyContentID)
                                                    If dt.Rows.Count > 0 Then
                                                        .ManyToManyContentName = EncodeText(dt.Rows(0).Item(0))
                                                    End If
                                                End If
                                                If .manyToManyRuleContentID > 0 Then
                                                    dt = cpCore.db.executeSql("select name from cccontent where id=" & .manyToManyRuleContentID)
                                                    If dt.Rows.Count > 0 Then
                                                        .ManyToManyRuleContentName = EncodeText(dt.Rows(0).Item(0))
                                                    End If
                                                End If
                                                If .MemberSelectGroupID > 0 Then
                                                    dt = cpCore.db.executeSql("select name from ccgroups where id=" & .MemberSelectGroupID)
                                                    If dt.Rows.Count > 0 Then
                                                        .MemberSelectGroupName = EncodeText(dt.Rows(0).Item(0))
                                                    End If
                                                End If
                                                If .RedirectContentID > 0 Then
                                                    dt = cpCore.db.executeSql("select name from cccontent where id=" & .RedirectContentID)
                                                    If dt.Rows.Count > 0 Then
                                                        .RedirectContentName = EncodeText(dt.Rows(0).Item(0))
                                                    End If
                                                End If
                                                dt.Dispose()
                                            End With
                                            .fields.Add(fieldNameLower, field)
                                            'REFACTOR
                                            If (contentName.ToLower() = "system email") And (field.nameLc = "sharedstylesid") Then
                                                contentName = contentName
                                            End If
                                            If ((field.fieldTypeId <> FieldTypeIdManyToMany) And (field.fieldTypeId <> FieldTypeIdRedirect) And (Not .selectList.Contains(fieldNameLower))) Then
                                                '
                                                ' add only fields that can be selected
                                                '
                                                .selectList.Add(fieldNameLower)
                                            End If
                                        End If
                                    Next
                                    .SelectCommaList = String.Join(",", .selectList)
                                End If
                                '
                                ' ----- Apply WorkflowAuthoring Rule that any definition must match its parent
                                '
                                If .parentID > 0 Then
                                    parentCdef = getCdef(.parentID)
                                    If Not (parentCdef Is Nothing) Then
                                        .AllowWorkflowAuthoring = parentCdef.AllowWorkflowAuthoring
                                    End If
                                End If
                                '
                                ' ----- Create the ContentControlCriteria
                                '
                                .ContentControlCriteria = getContentControlCriteria(.Id, .ContentTableName, .ContentDataSourceName, New List(Of Integer))
                                '
                            End With
                            getCdef_SetAdminColumns(returnCdef)
                        End If
                        Try
                            Call cpCore.cache.setKey("cdefId" & contentId.ToString, returnCdef, "content,content fields")
                        Catch ex As Exception
                            cpCore.handleExceptionAndNoThrow(ex)
                        End Try
                    End If
                    cdefList.Add(contentId, returnCdef)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnCdef
        End Function
        '''
        '''========================================================================
        '''   Returns a comma separated list of ContentPointers whos parent is the ContentPointer
        '''========================================================================
        '''
        ''Private Function GetChildPointerList(ByVal ContentPointer As Integer, Optional ByVal ParentContentPointerList As String = "") As String
        ''    Dim returnList As String = ""
        ''    Try
        ''        Dim ContentID As Integer
        ''        Dim ChildContentPointer As Integer
        ''        Dim ChildPointerList As String = ""
        ''        Dim GrandChildPointerList As String
        ''        Dim ParentContentPointerListLocal As String
        ''        '
        ''        returnList = ""
        ''        If (ContentPointer >= 0) And (InStr(1, "," & ContentPointer & ",", "," & ParentContentPointerList & ",") = 0) Then
        ''            '
        ''            ' ----- ContentPointer is good, and it is not in the parent list
        ''            '
        ''            ParentContentPointerListLocal = ParentContentPointerList & "," & ContentPointer
        ''            returnList = metaCache.cdef(ContentPointer).ChildPointerList
        ''            If returnList = "" Then
        ''                '
        ''                ' ----- List needs to be created
        ''                '
        ''                ContentID = metaCache.cdef(ContentPointer).Id
        ''                If ContentID = 106 Then
        ''                    ContentID = ContentID
        ''                End If
        ''                If ContentID > 0 Then
        ''                    For ChildContentPointer = 0 To metaCache.cdefCount - 1
        ''                        If metaCache.cdef(ChildContentPointer).ParentID = ContentID Then
        ''                            '
        ''                            ' ----- child content found
        ''                            '
        ''                            If ChildPointerList <> "" Then
        ''                                ChildPointerList = ChildPointerList & ","
        ''                            End If
        ''                            ChildPointerList = ChildPointerList & ChildContentPointer
        ''                            GrandChildPointerList = GetChildPointerList(ChildContentPointer, ParentContentPointerListLocal)
        ''                            If GrandChildPointerList <> "" Then
        ''                                ChildPointerList = ChildPointerList & "," & GrandChildPointerList
        ''                            End If
        ''                        End If
        ''                    Next
        ''                End If
        ''                returnList = ChildPointerList
        ''                metaCache.cdef(ContentPointer).ChildPointerList = ChildPointerList
        ''            End If
        ''        End If
        ''    Catch ex As Exception
        ''        cpCore.handleException(ex)
        ''    End Try
        ''    Return returnList
        'End Function
        '
        '========================================================================
        '   Get Child Criteria
        '
        '   Dig into Content Definition Records and create an SQL Criteria statement
        '   for parent-child relationships.
        '
        '   for instance, for a ContentControlCriteria, call this with:
        '       CriteriaFieldName = "ContentID"
        '       ContentName = "Content"
        '
        '   Results in (ContentID=5)or(ContentID=6)or(ContentID=10)
        '
        ' Get a string that can be used in the where criteria of a SQL statement
        ' opening the content pointed to by the content pointer. This criteria
        ' will include both the content, and its child contents.
        '========================================================================
        '
        Private Function getContentControlCriteria(ByVal contentId As Integer, contentTableName As String, contentDAtaSourceName As String, ByVal parentIdList As List(Of Integer)) As String
            Dim returnCriteria As String = ""
            Try
                Dim dt As DataTable
                Dim childContentId As Integer
                '
                returnCriteria = "(1=0)"
                If (contentId >= 0) Then
                    If Not parentIdList.Contains(contentId) Then
                        parentIdList.Add(contentId)
                        returnCriteria = "((" & contentTableName & ".contentcontrolId=" & contentId & ")"
                        dt = cpCore.db.executeSql("select id from cccontent where parentid=" & contentId, contentDAtaSourceName)
                        For Each datarow As DataRow In dt.Rows
                            childContentId = EncodeInteger(datarow.Item(0))
                            returnCriteria &= "OR" & getContentControlCriteria(childContentId, contentTableName, contentDAtaSourceName, parentIdList)
                        Next
                        dt.Dispose()
                        parentIdList.Remove(contentId)
                        returnCriteria = returnCriteria & ")"
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnCriteria
        End Function
        ''
        ''========================================================================
        '' Load the fields for a content provided
        ''
        ''   Definitions that are in the cache before the load, and are not reloaded, are
        ''   cleared by altering their names and IDs
        ''========================================================================
        ''
        'Public Sub StoreCDef(ByVal dt As DataTable)
        '    Try
        '        '
        '        Dim CSPointer As Integer
        '        Dim SQL As String
        '        Dim SortMethodCount As Integer
        '        Dim SortMethodSize As Integer
        '        Dim SortMethodPointer As Integer
        '        Dim DefaultSortMethodID As Integer
        '        Dim ContentPointer As Integer
        '        Dim TablePointer As Integer
        '        Dim ContentID As Integer
        '        Dim ContentTableName As String
        '        Dim RowMax As Integer
        '        Dim RowPointer As Integer
        '        Dim ColumnMax As Integer
        '        Dim ColumnPointer As Integer
        '        Dim NamePointer As Integer
        '        Dim IDPointer As Integer
        '        Dim DataSourceName As String
        '        Dim activepointer As Integer
        '        Dim UsedNames As String
        '        Dim WorkingPointer As Integer
        '        Dim ParentID As Integer
        '        Dim ParentName As String
        '        Dim ParentPointer As Integer
        '        Dim CDef As appServices_metaDataClass.CDefClass
        '        Dim ContentName As String
        '        Dim UcaseContentName As String
        '        '
        '        Dim AuthoringDataSourceNamePointer As Integer
        '        Dim AppSupportsWorkFlowAuthoring As Boolean
        '        '
        '        Dim x As Integer
        '        '
        '        '   clear all indexes for a full reload
        '        '
        '        'metaCache.cdefContentNameIndex = New keyPtrIndexClass
        '        'metaCache.cdefContentIDIndex = New keyPtrIndexClass
        '        'metaCache.cdefContentTableNameIndex = New keyPtrIndexClass
        '        '
        '        ' set count to 0 to start re-using the array
        '        '
        '        'metaCache.cdefCount = 0
        '        'metaCache.cdefSize = 0
        '        'ReDim metaCache.cdef(metaCache.cdefSize)
        '        ''
        '        ''   safe method, preload the content definitions with not 'touched'
        '        ''
        '        'If CDefCount > 0 Then
        '        '    For RowPointer = 0 To CDefCount - 1
        '        '        metaCache.cdef(RowPointer).SingleRecord = False
        '        '        Next
        '        '    End If
        '        '
        '        '   load content definition from array
        '        '
        '        RowMax = dt.Rows.Count - 1
        '        For RowPointer = 0 To RowMax
        '            ContentName = Trim(EncodeText(dt.Rows(RowPointer).Item(1)))
        '            UcaseContentName = vbUCase(ContentName)
        '            ContentID = EncodeInteger(dt.Rows(RowPointer).Item(0))
        '            ContentTableName = EncodeText(dt.Rows(RowPointer).Item(10))
        '            '
        '            ContentPointer = metaCache.cdefContentIDIndex.getPtr(CStr(ContentID))
        '            If ContentPointer = -1 Then
        '                '
        '                ' CDef not found, add it
        '                '
        '                ContentPointer = AddCDef(ContentName, ContentID, ContentTableName)
        '            End If
        '            With metaCache.cdef(ContentPointer)
        '                '
        '                ' ----- save values in definition
        '                '
        '                '.SingleRecord = True ' marked this record touched
        '                '.WhereClause = encodeText(RSRows(2, RowPointer))
        '                .AllowAdd = EncodeBoolean(dt.Rows(RowPointer).Item(3))
        '                .DeveloperOnly = EncodeBoolean(dt.Rows(RowPointer).Item(4))
        '                .AdminOnly = EncodeBoolean(dt.Rows(RowPointer).Item(5))
        '                .AllowDelete = EncodeBoolean(dt.Rows(RowPointer).Item(6))
        '                .ParentID = EncodeInteger(dt.Rows(RowPointer).Item(7))
        '                .DropDownFieldList = vbUCase(EncodeText(dt.Rows(RowPointer).Item(9)))
        '                '
        '                .ContentTableName = EncodeText(ContentTableName)
        '                '.TableName = .ContentTableName
        '                '
        '                DataSourceName = EncodeText(dt.Rows(RowPointer).Item(11))
        '                If DataSourceName = "" Then
        '                    DataSourceName = "DEFAULT"
        '                End If
        '                .ContentDataSourceName = DataSourceName
        '                '
        '                .AuthoringTableName = EncodeText(dt.Rows(RowPointer).Item(12))
        '                '
        '                DataSourceName = EncodeText(dt.Rows(RowPointer).Item(13))
        '                If DataSourceName = "" Then
        '                    DataSourceName = "DEFAULT"
        '                End If
        '                .AuthoringDataSourceName = DataSourceName
        '                .AllowWorkflowAuthoring = EncodeBoolean(dt.Rows(RowPointer).Item(14))
        '                .AllowCalendarEvents = EncodeBoolean(dt.Rows(RowPointer).Item(15))
        '                .dataSourceId = EncodeInteger(dt.Rows(RowPointer).Item(16))
        '                .DefaultSortMethod = EncodeText(dt.Rows(RowPointer).Item(17))
        '                If .DefaultSortMethod = "" Then
        '                    .DefaultSortMethod = "Name"
        '                End If
        '                .EditorGroupName = EncodeText(dt.Rows(RowPointer).Item(18))
        '                '
        '                .AllowContentTracking = EncodeBoolean(dt.Rows(RowPointer).Item(19))
        '                .AllowTopicRules = EncodeBoolean(dt.Rows(RowPointer).Item(20))
        '                .AllowMetaContent = EncodeBoolean(dt.Rows(RowPointer).Item(21))
        '                '
        '                ' future
        '                '
        '                .ActiveOnly = True
        '                .AliasID = "ID"
        '                .AliasName = "NAME"
        '                .IgnoreContentControl = False
        '                '
        '                ' Special cases
        '                '
        '                If (.AuthoringDataSourceName = "") Or (.AuthoringTableName = "") Then
        '                    .AuthoringDataSourceName = .ContentDataSourceName
        '                    .AuthoringTableName = .ContentTableName
        '                End If
        '                If .dataSourceId = 0 Then
        '                    .dataSourceId = -1
        '                End If
        '                If .ParentID = 0 Then
        '                    .ParentID = -1
        '                End If
        '                ' 20151114 - test removing these. They should be added when fields are added, and these block child content .field() records from being added
        '                .SelectFieldList = ""
        '                '.SelectFieldList = "id,name,dateadded,modifieddate,createdby,modifiedby,active,sortorder,contentcategoryid"
        '            End With
        '            '''DoEvents()
        '        Next
        '        '
        '        ' Force all child content definitions to use their parent's tables
        '        '
        '        Dim RootPointer As Integer
        '        Dim CDefPointer As Integer
        '        For CDefPointer = 0 To metaCache.cdefCount - 1
        '            If metaCache.cdef(CDefPointer).ParentID > 0 Then
        '                RootPointer = GetCDefRootPointer(CDefPointer)
        '                If RootPointer >= 0 Then
        '                    metaCache.cdef(CDefPointer).ContentTableName = metaCache.cdef(RootPointer).ContentTableName
        '                    metaCache.cdef(CDefPointer).AuthoringTableName = metaCache.cdef(RootPointer).AuthoringTableName
        '                    metaCache.cdef(CDefPointer).ContentDataSourceName = metaCache.cdef(RootPointer).ContentDataSourceName
        '                    metaCache.cdef(CDefPointer).AuthoringDataSourceName = metaCache.cdef(RootPointer).AuthoringDataSourceName
        '                    metaCache.cdef(CDefPointer).AllowWorkflowAuthoring = metaCache.cdef(RootPointer).AllowWorkflowAuthoring
        '                End If
        '            End If
        '            '''DoEvents()
        '        Next
        '    Catch ex As Exception
        '        cpCore.handleException(ex)
        '    End Try
        'End Sub
        ''
        ''========================================================================
        '' Load the fields for a content provided
        ''
        ''   All Content Definitions must be loaded first
        ''========================================================================
        ''
        'Public Sub StoreCDefFields(ByVal dt As DataTable)
        '    Try
        '        '
        '        Dim ChildArray() As String
        '        Dim Pointer As Integer
        '        Dim ChildIDList As String
        '        ' converted array to dictionary - Dim FieldPointer As Integer
        '        Dim FieldName As String
        '        Dim fieldType As Integer
        '        Dim fieldId As Integer
        '        Dim FieldContentID As Integer
        '        Dim LastFieldContentID As Integer
        '        'Dim ContentPointer As Integer
        '        Dim rowMax As Integer
        '        Dim RowPointer As Integer
        '        Dim ContentName As String
        '        Dim UsedNames As String
        '        Dim WorkingPointer As Integer
        '        Dim ParentID As Integer
        '        Dim ParentName As String
        '        Dim x As Integer
        '        Dim FieldEditSortPriority As Integer
        '        Dim IsSelectField As Boolean
        '        Dim fieldHtmlContent As Boolean
        '        Dim fieldActive As Boolean
        '        Dim cdef As CDefClass
        '        '
        '        rowMax = dt.Rows.Count - 1
        '        LastFieldContentID = -1
        '        For RowPointer = 0 To rowMax
        '            FieldName = EncodeText(dt.Rows(RowPointer).Item(13)).ToLower
        '            If Not String.IsNullOrEmpty(FieldName) Then
        '                IsSelectField = True
        '                FieldEditSortPriority = EncodeInteger(dt.Rows(RowPointer).Item(10))
        '                FieldContentID = EncodeInteger(dt.Rows(RowPointer).Item(11))
        '                fieldId = EncodeInteger(dt.Rows(RowPointer).Item(12))
        '                fieldType = EncodeInteger(dt.Rows(RowPointer).Item(15))
        '                fieldHtmlContent = EncodeBoolean(dt.Rows(RowPointer).Item(25))
        '                fieldActive = EncodeBoolean(dt.Rows(RowPointer).Item(24))
        '                '
        '                ' translate htmlContent to fieldtypehtml
        '                '   this is also converted in upgrade, daily housekeep, addon install
        '                '
        '                If fieldHtmlContent Then
        '                    If fieldType = FieldTypeLongText Then
        '                        fieldType = FieldTypeHTML
        '                    ElseIf fieldType = FieldTypeTextFile Then
        '                        fieldType = FieldTypeHTMLFile
        '                    End If
        '                End If
        '                '
        '                ' Determine if it is a select field or not (actual field in the select list)
        '                '
        '                If (Not fieldActive) Or (fieldType = FieldTypeRedirect) Or (fieldType = FieldTypeManyToMany) Then
        '                    '
        '                    ' Special exception, Redirect fields may not have field names
        '                    '
        '                    IsSelectField = False
        '                ElseIf (InStr(1, FieldName, "unnamedfield", vbTextCompare) <> 0) Then
        '                    '
        '                    ' Special exception, fields named "UnnamedField..." fields may not have field names
        '                    '
        '                    IsSelectField = False
        '                End If
        '                '
        '                ' Check for next content definition
        '                '
        '                If FieldContentID <> LastFieldContentID Then
        '                    '
        '                    ' New Content Definition, get new CDef object
        '                    '
        '                    cdef = getCdef(FieldContentID)
        '                    'ContentPointer = metaCache.cdefContentIDIndex.getPtr(CStr(FieldContentID))
        '                    ContentName = cdef.Name
        '                    LastFieldContentID = FieldContentID
        '                End If
        '                '
        '                ' Get Field Pointer within CDef
        '                '
        '                Dim field As CDefFieldClass
        '                If Not cdef.fields.ContainsKey(FieldName) Then
        '                    field = New CDefFieldClass()
        '                    cdef.fields.Add(FieldName, field)
        '                Else
        '                    field = cdef.fields(fieldName.ToLower())
        '                End If

        '                With field
        '                    .DeveloperOnly = EncodeBoolean(dt.Rows(RowPointer).Item(0))
        '                    .UniqueName = EncodeBoolean(dt.Rows(RowPointer).Item(1))
        '                    .TextBuffered = EncodeBoolean(dt.Rows(RowPointer).Item(2))
        '                    .Password = EncodeBoolean(dt.Rows(RowPointer).Item(3))
        '                    If (EncodeText(dt.Rows(RowPointer).Item(4)) = "") Then
        '                        .IndexColumn = -1
        '                    Else
        '                        .IndexColumn = EncodeInteger(dt.Rows(RowPointer).Item(4))
        '                    End If
        '                    .IndexWidth = EncodeText(dt.Rows(RowPointer).Item(5))
        '                    .IndexWidth = EncodeText(EncodeInteger(Replace(.IndexWidth, "%", "")))
        '                    .IndexSortOrder = EncodeText(dt.Rows(RowPointer).Item(6))
        '                    .IndexSortDirection = EncodeInteger(dt.Rows(RowPointer).Item(7))
        '                    .AdminOnly = EncodeBoolean(dt.Rows(RowPointer).Item(8))
        '                    ' sortorder
        '                    .EditSortPriority = FieldEditSortPriority
        '                    .ContentID = FieldContentID
        '                    .Id = fieldId
        '                    ' name
        '                    .Required = EncodeBoolean(dt.Rows(RowPointer).Item(14))
        '                    .fieldType = fieldType
        '                    .Caption = EncodeText(dt.Rows(RowPointer).Item(16))
        '                    .ReadOnlyField = EncodeBoolean(dt.Rows(RowPointer).Item(17))
        '                    .LookupContentID = EncodeInteger(dt.Rows(RowPointer).Item(18))
        '                    .RedirectContentID = EncodeInteger(dt.Rows(RowPointer).Item(19))
        '                    .RedirectPath = EncodeText(dt.Rows(RowPointer).Item(20))
        '                    .RedirectID = EncodeText(dt.Rows(RowPointer).Item(21))
        '                    .DefaultValueObject = dt.Rows(RowPointer).Item(22)
        '                    .HelpMessage = "deprecated"
        '                    .active = fieldActive
        '                    .htmlContent = fieldHtmlContent
        '                    .NotEditable = EncodeBoolean(dt.Rows(RowPointer).Item(26))
        '                    .Authorable = EncodeBoolean(dt.Rows(RowPointer).Item(27))
        '                    '
        '                    .ManyToManyContentID = EncodeInteger(dt.Rows(RowPointer).Item(28))
        '                    .ManyToManyRuleContentID = EncodeInteger(dt.Rows(RowPointer).Item(29))
        '                    .ManyToManyRulePrimaryField = EncodeText(dt.Rows(RowPointer).Item(30))
        '                    .ManyToManyRuleSecondaryField = EncodeText(dt.Rows(RowPointer).Item(31))
        '                    '
        '                    .RSSTitleField = EncodeBoolean(dt.Rows(RowPointer).Item(32))
        '                    .RSSDescriptionField = EncodeBoolean(dt.Rows(RowPointer).Item(33))
        '                    .EditTab = EncodeText(dt.Rows(RowPointer).Item(34))
        '                    .Scramble = EncodeBoolean(dt.Rows(RowPointer).Item(35))
        '                    .MemberSelectGroupID = EncodeInteger(dt.Rows(RowPointer).Item(36))
        '                    .LookupList = EncodeText(dt.Rows(RowPointer).Item(37))
        '                    '
        '                    .Inherited = False
        '                    Dim testFieldList As String
        '                    testFieldList = cdef.SelectFieldList
        '                    If String.IsNullOrEmpty(testFieldList) Then
        '                        cdef.SelectFieldList = .Name
        '                    ElseIf IsSelectField And (InStr(1, "," & testFieldList & ",", "," & .Name & ",", vbTextCompare) = 0) Then
        '                        '
        '                        ' Create SelectFieldList if active, non-redirect, non-empty and not already there
        '                        '
        '                        cdef.SelectFieldList &= "," & .Name
        '                    End If
        '                    '
        '                    ' IsBaseField - true this is a custom field, false is it a cdef field
        '                    '
        '                    .BlockAccess = EncodeBoolean(dt.Rows(RowPointer).Item(38))
        '                End With
        '            End If
        '        Next
        '        '
        '        ' ----- now do custom fields
        '        '
        '        For ContentPointer = 0 To metaCache.cdefCount - 1
        '            '
        '            ' ----- Apply WorkflowAuthoring Rule to prevent cyclic ParentIDs
        '            '
        '            UsedNames = ""
        '            ContentName = metaCache.cdef(ContentPointer).Name
        '            ParentName = ContentName
        '            WorkingPointer = metaCache.cdefContentNameIndex.getPtr(Trim(ParentName))
        '            Do While (WorkingPointer <> -1) And (InStr(1, UsedNames & ".", "." & ParentName & ".") = 0)
        '                UsedNames = UsedNames & "." & ParentName
        '                ParentID = metaCache.cdef(metaCache.cdefContentNameIndex.getPtr(Trim(ParentName))).ParentID
        '                If ParentID = -1 Then
        '                    WorkingPointer = -1
        '                Else
        '                    WorkingPointer = metaCache.cdefContentIDIndex.getPtr(CStr(ParentID))
        '                    ParentName = metaCache.cdef(WorkingPointer).Name
        '                End If
        '            Loop
        '            If WorkingPointer <> -1 Then
        '                metaCache.cdef(ContentPointer).ParentID = 0
        '            End If
        '            '
        '            ' ----- Apply WorkflowAuthoring Rule that any definition must match its parent
        '            '
        '            If ParentName <> ContentName Then
        '                metaCache.cdef(ContentPointer).AllowWorkflowAuthoring = metaCache.cdef(metaCache.cdefContentNameIndex.getPtr(Trim(ParentName))).AllowWorkflowAuthoring
        '            End If
        '            '
        '            ' ----- Create the ContentControlCriteria
        '            '
        '            metaCache.cdef(ContentPointer).ContentControlCriteria = GetChildCriteria(ContentName, "ContentControlID")
        '            '
        '            ' ----- Create the ChildPointerList
        '            '
        '            metaCache.cdef(ContentPointer).ChildPointerList = GetChildPointerList(ContentPointer)
        '            '
        '            ' ----- Create the ChildIDList from the ChildPointerList
        '            '
        '            If metaCache.cdef(ContentPointer).ChildPointerList <> "" Then
        '                ChildArray = Split(metaCache.cdef(ContentPointer).ChildPointerList, ",")
        '                ChildIDList = ""
        '                For Pointer = 0 To UBound(ChildArray)
        '                    ChildIDList = ChildIDList & "," & metaCache.cdef(EncodeInteger(ChildArray(Pointer))).Id
        '                Next
        '                metaCache.cdef(ContentPointer).ChildIDList = Mid(ChildIDList, 2)
        '            End If
        '            '''DoEvents()
        '        Next
        '        '
        '        ' ----- populate inherated fields
        '        '
        '        For ContentPointer = 0 To metaCache.cdefCount - 1
        '            If (metaCache.cdef(ContentPointer).ParentID = -1) And (metaCache.cdef(ContentPointer).ChildPointerList <> "") Then
        '                '
        '                ' ----- Start with root pages (no parent) that have children
        '                '
        '                'Call StoreCDefFields_ShareWithChildren(ContentPointer)
        '            End If
        '        Next
        '        '
        '        ' ----- Precalculate Admin Field structure
        '        '
        '        For Each cdefWorking As CDefClass In metaCache.cdef
        '            Call getCdef_SetAdminColumns(cdefWorking)
        '        Next
        '    Catch ex As Exception
        '        cpCore.handleException(ex)
        '    End Try
        'End Sub
        ''
        ''========================================================================
        ''   Share inheritable fields with children
        ''========================================================================
        ''
        'Private Sub StoreCDefFields_ShareWithChildren(ByVal ParentCDefPointer As Integer)
        '    Try
        '        '
        '        Dim ChildCDefPointerList As String
        '        Dim ChildCDefPointerArray As String()
        '        Dim ChildCDefPointerArray_Pointer As Integer
        '        Dim ChildCDefPointer As Integer
        '        Dim ChildFieldPointer As Integer
        '        Dim ParentFieldPointer As Integer
        '        Dim FieldName As String
        '        Dim FieldEditSortPriority As Integer
        '        Dim ChildFieldList As String
        '        Dim lcaseName As String
        '        Dim SelectFieldList As String
        '        '
        '        If metaCache.cdef(ParentCDefPointer).Id > 0 Then
        '            ChildCDefPointerList = metaCache.cdef(ParentCDefPointer).ChildPointerList
        '            If ChildCDefPointerList <> "" Then
        '                ChildCDefPointerArray = Split(ChildCDefPointerList, ",")
        '                For ChildCDefPointerArray_Pointer = 0 To UBound(ChildCDefPointerArray)
        '                    '
        '                    ' inherit the parent definition fields to this child definition
        '                    '
        '                    ChildCDefPointer = EncodeInteger(ChildCDefPointerArray(ChildCDefPointerArray_Pointer))
        '                    '
        '                    ' Create a string of Child FieldNames to search
        '                    '
        '                    '20151113 - list should include common fields so child does not inherit 'sortorder' and create a duplicate
        '                    ChildFieldList = "," & metaCache.cdef(ChildCDefPointer).SelectFieldList & ","
        '                    SelectFieldList = ""
        '                    For ParentFieldPointer = 0 To metaCache.cdef(ParentCDefPointer).fields.Count - 1
        '                        '
        '                        ' check childfieldlist for parent field
        '                        '
        '                        If (InStr(1, ChildFieldList, "," & metaCache.cdef(ParentCDefPointer).fields(ParentFieldPointer).Name & ",", vbTextCompare) = 0) Then
        '                            '
        '                            ' Add inherited Field needs to be inherited because a child field was not found
        '                            '
        '                            With metaCache.cdef(ParentCDefPointer).fields(ParentFieldPointer)
        '                                FieldName = .Name
        '                                FieldEditSortPriority = .EditSortPriority
        '                            End With
        '                            ChildFieldPointer = AddField(ChildCDefPointer, FieldName)
        '                            metaCache.cdef(ChildCDefPointer).fields(ChildFieldPointer) = metaCache.cdef(ParentCDefPointer).fields(ParentFieldPointer)
        '                            With metaCache.cdef(ChildCDefPointer).fields(ChildFieldPointer)
        '                                .Inherited = True
        '                                lcaseName = vbLCase(.Name)
        '                                ' 20151114 - dont understand why id is excluded
        '                                If .active And (.fieldType <> FieldTypeRedirect) And (.fieldType <> FieldTypeManyToMany) And (.Name <> "") Then
        '                                    'If .active And (.fieldType <> FieldTypeRedirect) And (.fieldType <> FieldTypeManyToMany) And (.Name <> "") And (lcaseName <> "id") Then
        '                                    'If .active And (.FieldType <> FieldTypeRedirect) And (.FieldType <> FieldTypeManyToMany) And (.Name <> "") And (lcaseName <> "id") And (lcaseName <> "ccguid") Then
        '                                    '
        '                                    ' can not select fields with no name, redirects, inactives and "ID" was added when fields was added
        '                                    '
        '                                    SelectFieldList = SelectFieldList & "," & FieldName
        '                                End If
        '                            End With
        '                        End If
        '                        '''DoEvents()
        '                    Next
        '                    metaCache.cdef(ChildCDefPointer).SelectFieldList &= SelectFieldList
        '                    '
        '                    If metaCache.cdef(ChildCDefPointer).ChildPointerList <> "" Then
        '                        '
        '                        ' inherit the child definition fields to the childs children
        '                        '
        '                        Call StoreCDefFields_ShareWithChildren(ChildCDefPointer)
        '                    End If
        '                    '''DoEvents()
        '                Next
        '            End If
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleException(ex)
        '    End Try
        'End Sub
        ''
        '' Add a Field to the CDef structure
        ''
        ''
        'Private Function AddField(ByVal ContentPointer As Integer, ByVal FieldName As String) As Integer
        '    Try
        '        '
        '        With metaCache.cdef(ContentPointer)
        '            If .fields.Count >= .FieldSize Then
        '                .FieldSize = .FieldSize + 100
        '                ReDim Preserve .fields(.FieldSize)
        '                'Call .redimFields(.FieldSize)
        '            End If
        '            AddField = .fields.Count
        '            .fields.Count = .fields.Count + 1
        '            .fields(AddField).Name = Trim(FieldName)
        '        End With
        '    Catch ex As Exception
        '        cpCore.handleException(ex)
        '    End Try
        'End Function
        ''
        ''
        ''
        'Private Function GetFieldPointer(ByVal ContentPointer As Integer, ByVal FieldName As String) As Integer
        '    Dim returnPtr As Integer = -1
        '    Try
        '        '
        '        Dim UcaseFieldName As String
        '        '
        '        returnPtr = -1
        '        With metaCache.cdef(ContentPointer)
        '            If .fields.Count > 0 Then
        '                UcaseFieldName = vbUCase(FieldName)
        '                For returnPtr = 0 To .fields.Count - 1
        '                    If vbUCase(.fields(returnPtr).Name) = UcaseFieldName Then
        '                        Exit For
        '                    End If
        '                    '''DoEvents()
        '                Next
        '            End If
        '            If returnPtr >= .fields.Count Then
        '                returnPtr = -1
        '            End If
        '        End With
        '    Catch ex As Exception
        '        cpCore.handleException(ex)
        '    End Try
        '    Return returnPtr
        'End Function
        ''
        '' ----- Add a new Content Definition
        ''
        'Private Function AddCDef(ByVal ContentName As String, ByVal ContentID As Integer, ByVal TableName As String) As Integer
        '    Dim returnPtr As Integer
        '    Try
        '        '
        '        If metaCache.cdefCount >= metaCache.cdefSize Then
        '            metaCache.cdefSize = metaCache.cdefSize + 10
        '            ReDim Preserve metaCache.cdef(metaCache.cdefSize)
        '            ReDim Preserve CDefName(metaCache.cdefSize)
        '            ReDim Preserve CDefID(metaCache.cdefSize)
        '        End If
        '        metaCache.cdef(metaCache.cdefCount).Name = ContentName
        '        metaCache.cdef(metaCache.cdefCount).Id = ContentID
        '        metaCache.cdef(metaCache.cdefCount).ContentTableName = TableName
        '        metaCache.cdef(metaCache.cdefCount).SelectFieldList = "ID"
        '        '
        '        CDefName(metaCache.cdefCount) = ContentName
        '        CDefID(metaCache.cdefCount) = ContentID
        '        '
        '        'Call metaCache.cdefContentNameIndex.setPtr(ContentName, metaCache.cdefCount)
        '        'Call metaCache.cdefContentIDIndex.setPtr(CStr(ContentID), metaCache.cdefCount)
        '        'Call metaCache.cdefContentTableNameIndex.setPtr(TableName, metaCache.cdefCount)
        '        returnPtr = metaCache.cdefCount
        '        metaCache.cdefCount = metaCache.cdefCount + 1
        '    Catch ex As Exception
        '        cpCore.handleException(ex)
        '    End Try
        '    Return returnPtr
        'End Function
        ''
        '' ----- GetContentNameByID
        ''
        'Public Function getContentNameByID(ByVal ContentID As Integer) As String
        '    Dim returnName As String = ""
        '    Try
        '        Dim ContentPointer As Integer
        '        '
        '        ContentPointer = metaCache.cdefContentIDIndex.getPtr(CStr(ContentID))
        '        If ContentPointer >= 0 Then
        '            returnName = metaCache.cdef(ContentPointer).Name
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleException(ex)
        '    End Try
        '    Return returnName
        'End Function
        '
        '========================================================================
        '   IsWithinContent( ChildContentID, ParentContentID )
        '
        '       Returns true if ChildContentID is in ParentContentID
        '========================================================================
        '
        Function isWithinContent(ByVal ChildContentID As Integer, ByVal ParentContentID As Integer) As Boolean
            Dim returnOK As Boolean = False
            Try
                Dim cdef As CDefClass
                If (ChildContentID = ParentContentID) Then
                    returnOK = True
                Else
                    cdef = getCdef(ParentContentID)
                    If Not (cdef Is Nothing) Then
                        If cdef.childIdList.Count > 0 Then
                            returnOK = cdef.childIdList.Contains(ChildContentID)
                            If Not returnOK Then
                                For Each contentId As Integer In cdef.childIdList
                                    returnOK = isWithinContent(contentId, ParentContentID)
                                    If returnOK Then Exit For
                                Next
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnOK
        End Function
        ''
        ''========================================================================
        ''   Get the CDef Count
        ''========================================================================
        ''
        'Public Function GetCDefCount() As Integer
        '    Return metaCache.cdefCount
        'End Function
        ''
        ''========================================================================
        ''   Get the CDefName array
        ''========================================================================
        ''
        'Public Function GetCDefNames() As String()
        '    GetCDefNames = CDefName
        'End Function
        ''
        ''========================================================================
        ''   Get the CDefID array
        ''========================================================================
        ''
        'Public Function GetCDefIDs() As Integer()
        '    GetCDefIDs = CDefID
        'End Function
        ''
        ''=================================================================
        ''   Returns a pointer to the topmost CDef entry -- the one that holds the table entries
        ''       (if there is not a valid parent, the current entry is the top-most)
        ''=================================================================
        ''
        'Private Function GetCDefRootPointer(ByVal CDefPointer As Integer) As Integer
        '    Dim returnPtr As Integer = -1
        '    Try
        '        Dim ParentPointer As Integer
        '        Dim ParentID As Integer
        '        '
        '        GetCDefRootPointer = CDefPointer
        '        If CDefPointer >= 0 Then
        '            ParentID = metaCache.cdef(CDefPointer).ParentID
        '            If ParentID > 0 Then
        '                ParentPointer = metaCache.cdefContentIDIndex.getPtr(CStr(ParentID))
        '                If ParentPointer >= 0 Then
        '                    GetCDefRootPointer = GetCDefRootPointer(ParentPointer)
        '                End If
        '            End If
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleException(ex)
        '    End Try
        '    Return returnPtr
        'End Function
        '
        '=================================================================================
        '
        ' From Admin code in ccWeb3
        '   Put it here so the same data will be used for both the admin site and the tool page
        '   change this so it reads from the CDef, not the database
        '
        '
        '=================================================================================
        '
        Public Sub getCdef_SetAdminColumns(cdef As CDefClass)
            Try
                '
                'Dim DestPtr As Integer
                ' converted array to dictionary - Dim FieldPointer As Integer
                Dim FieldActive As Boolean
                Dim FieldWidth As Integer
                Dim FieldWidthTotal As Integer
                Dim adminColumn As CDefAdminColumnClass
                '
                With cdef
                    If .Id > 0 Then
                        Dim cnt As Integer = 0
                        For Each keyValuePair As KeyValuePair(Of String, coreMetaDataClass.CDefFieldClass) In cdef.fields
                            Dim field As coreMetaDataClass.CDefFieldClass = keyValuePair.Value
                            FieldActive = field.active
                            FieldWidth = EncodeInteger(field.indexWidth)
                            If FieldActive And (FieldWidth > 0) Then
                                FieldWidthTotal = FieldWidthTotal + FieldWidth
                                adminColumn = New CDefAdminColumnClass
                                With adminColumn
                                    .Name = field.nameLc
                                    .SortDirection = field.indexSortDirection
                                    .SortPriority = EncodeInteger(field.indexSortOrder)
                                    .Width = FieldWidth
                                    FieldWidthTotal = FieldWidthTotal + .Width
                                End With
                                .adminColumns.Add((cnt + (adminColumn.SortPriority * 1000)), adminColumn)
                            End If
                            cnt += 1
                        Next
                        '
                        ' Force the Name field as the only column
                        '
                        If .fields.Count > 0 Then
                            If .adminColumns.Count = 0 Then
                                '
                                ' Force the Name field as the only column
                                '
                                If .fields.ContainsKey("name") Then
                                    adminColumn = New CDefAdminColumnClass
                                    With adminColumn
                                        .Name = "Name"
                                        .SortDirection = 1
                                        .SortPriority = 1
                                        .Width = 100
                                        FieldWidthTotal = FieldWidthTotal + .Width
                                    End With
                                    .adminColumns.Add(1, adminColumn)
                                End If
                            End If
                            '
                            ' Normalize the column widths
                            '
                            For Each keyvaluepair In .adminColumns
                                adminColumn = keyvaluepair.Value
                                adminColumn.Width = CInt(100 * (CDbl(adminColumn.Width) / CDbl(FieldWidthTotal)))
                            Next
                        End If
                    End If
                End With
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        ''
        ''========================================================================
        ''   Load the Content Definitions
        ''
        ''   Reloads or appends a content definition from the table
        ''   Call executesqlTrapless because executesql calls this routine
        ''========================================================================
        ''
        'Public Sub loadMetaCache_cdef()
        '    Try
        '        Dim dt As DataTable
        '        Dim SQL As String
        '        Dim SQLMeta As String
        '        Dim SQLOrderBy As String
        '        '
        '        ' Clear the local cache
        '        '
        '        metaCache = New metaCacheClass()
        '        'metaCache.cdefCount = 0
        '        '
        '        ' ---- Setup Content Definition loads for older data versions
        '        '
        '        SQLMeta = "ccContent.AllowMetaContent as AllowMetaContent"
        '        '
        '        ' ----- Disable Duplicates which may prevent proper reload
        '        '
        '        If cpCore.cluster.config.defaultDataSourceType = dataSourceTypeEnum.mySqlNative Then
        '            '
        '            ' MySQL - need a dedup solution ******************************************************
        '            '
        '        Else
        '            SQL = "update cccontent set Active = 0 where ID IN ("
        '            sql &=  " select C2.ID"
        '            sql &=  " from ccContent AS C1 LEFT JOIN ccContent AS C2 ON C1.Name = C2.Name"
        '            sql &=  " WHERE (C2.Active<>0) AND (C1.Active<>0) AND (C1.ID<C2.ID)"
        '            sql &=  ")"
        '            Call cpCore.app.executeSql(SQL)
        '            '
        '            SQL = "update ccFields set Active = 0 where ID IN ("
        '            sql &=  " select F2.ID"
        '            sql &=  " from ccFields AS F1 LEFT JOIN ccFields AS F2 ON F1.Name = F2.Name"
        '            sql &=  " WHERE (F1.contentid=F2.contentid) and (F2.Active<>0) AND (F1.Active<>0) AND (F1.ID<F2.ID)"
        '            sql &=  ")"
        '            Call cpCore.app.executeSql(SQL)
        '        End If
        '        '
        '        ' ----- Convert all boolean defaults from null to 0
        '        '
        '        SQL = "update ccfields set defaultValue='0' where type=4 and defaultValue is null"
        '        Call cpCore.app.executeSql(SQL)
        '        '
        '        ' ----- load all content records
        '        '
        '        SQL = "SELECT " _
        '        & "ccContent.ID" _
        '        & ", ccContent.Name" _
        '        & ", ccContent.name" _
        '        & ", ccContent.AllowAdd" _
        '        & ", ccContent.DeveloperOnly" _
        '        & ", ccContent.AdminOnly" _
        '        & ", ccContent.AllowDelete" _
        '        & ", ccContent.ParentID" _
        '        & ", ccContent.DefaultSortMethodID" _
        '        & ", ccContent.DropDownFieldList" _
        '        & ", ContentTable.Name AS ContentTableName" _
        '        & ", ContentDataSource.Name AS ContentDataSourceName" _
        '        & ", AuthoringTable.Name AS AuthoringTableName" _
        '        & ", AuthoringDataSource.Name AS AuthoringDataSourceName" _
        '        & ", ccContent.AllowWorkflowAuthoring AS AllowWorkflowAuthoring" _
        '        & ", ccContent.AllowCalendarEvents as AllowCalendarEvents" _
        '        & ", ContentTable.DataSourceID" _
        '        & ", ccSortMethods.OrderByClause as DefaultSortMethod" _
        '        & ", ccGroups.Name as EditorGroupName" _
        '        & ", ccContent.AllowContentTracking as AllowContentTracking" _
        '        & ", ccContent.AllowTopicRules as AllowTopicRules" _
        '        & ", ccContent.AllowMetaContent as AllowMetaContent" _
        '        & " FROM (((((ccContent LEFT JOIN ccTables AS ContentTable ON ccContent.ContentTableID = ContentTable.ID) LEFT JOIN ccTables AS AuthoringTable ON ccContent.AuthoringTableID = AuthoringTable.ID) LEFT JOIN ccDataSources AS ContentDataSource ON ContentTable.DataSourceID = ContentDataSource.ID) LEFT JOIN ccDataSources AS AuthoringDataSource ON AuthoringTable.DataSourceID = AuthoringDataSource.ID) LEFT JOIN ccSortMethods ON ccContent.DefaultSortMethodID = ccSortMethods.ID) LEFT JOIN ccGroups ON ccContent.EditorGroupID = ccGroups.ID" _
        '        & " WHERE (ccContent.Active<>0)"
        '        dt = cpCore.app.executeSql(SQL)
        '        If dt.Rows.Count = 0 Then
        '            '
        '        Else
        '            Call StoreCDef(dt)
        '            '
        '            ' ----- now load all the Content Definition Fields
        '            '
        '            SQL = "SELECT" _
        '            & "  ccFields.DeveloperOnly" _
        '            & ", ccFields.UniqueName" _
        '            & ", ccFields.TextBuffered" _
        '            & ", ccFields.Password" _
        '            & ", ccFields.IndexColumn" _
        '            & ", ccFields.IndexWidth" _
        '            & ", ccFields.IndexSortPriority" _
        '            & ", ccFields.IndexSortDirection" _
        '            & ", ccFields.AdminOnly" _
        '            & ", ccFields.SortOrder" _
        '            & ", ccFields.EditSortPriority" _
        '            & ", ccFields.ContentID" _
        '            & ", ccFields.ID" _
        '            & ", ccFields.Name" _
        '            & ", ccFields.Required" _
        '            & ", ccFields.Type" _
        '            & ", ccFields.Caption" _
        '            & ", ccFields.readonly" _
        '            & ", ccFields.LookupContentID" _
        '            & ", ccFields.RedirectContentID" _
        '            & ", ccFields.RedirectPath" _
        '            & ", ccFields.RedirectID" _
        '            & ", ccFields.DefaultValue"
        '            sql &=  ", '' as HelpMessageDeprecated" _
        '            & ", ccFields.Active" _
        '            & ", ccFields.HTMLContent" _
        '            & ", ccFields.NotEditable" _
        '            & ", ccFields.authorable"
        '            SQLOrderBy = "ccFields.ContentID,ccFields.EditTab,ccFields.EditSortPriority;"
        '            sql &= "" _
        '            & ", ccFields.ManyToManyContentID" _
        '            & ",ccFields.ManyToManyRuleContentID" _
        '            & ",ccFields.ManyToManyRulePrimaryField" _
        '            & ",ccFields.ManyToManyRuleSecondaryField" _
        '            & ",ccFields.RSSTitleField" _
        '            & ",ccFields.RSSDescriptionField" _
        '            & ",ccFields.EditTab" _
        '            & ",ccFields.Scramble" _
        '            & ",ccFields.MemberSelectGroupID" _
        '            & ",ccFields.LookupList" _
        '            & ",ccFields.IsBaseField"
        '            sql &=  " FROM ccFields LEFT JOIN ccContent ON ccFields.ContentID = ccContent.ID"
        '            sql &=  " WHERE (ccContent.ID Is not Null)AND(ccContent.Active<>0)and(ccFields.active<>0)and(ccFields.Type<>0)"
        '            sql &=  " AND(ccfields.name<>'')"
        '            sql &=  " ORDER BY " & SQLOrderBy
        '            '
        '            dt = cpCore.app.executeSql(SQL)
        '            If dt.Rows.Count = 0 Then
        '                '
        '            Else
        '                Call StoreCDefFields(dt)
        '            End If
        '        End If
        '        '
        '        ' setup local storage for this cdef
        '        '
        '        Dim pointer As Integer
        '        ReDim Preserve metaCache.cdefContentFieldIndex(metaCache.cdefCount)
        '        ReDim Preserve metaCache.cdefContentFieldIndexPopulated(metaCache.cdefCount)
        '        For pointer = 0 To metaCache.cdefCount - 1
        '            '
        '            ' setup local storage for this cdef
        '            '
        '            Dim lcaseContentName As String
        '            lcaseContentName = metaCache.cdef(pointer).Name.ToLower
        '            Call metaCache.cdefContentNameIndex.setPtr(lcaseContentName, pointer)
        '            metaCache.cdefContentFieldIndex(pointer) = New keyPtrIndexClass
        '            metaCache.cdefContentFieldIndexPopulated(pointer) = False
        '        Next
        '        '
        '        metaCache.cdefCount = GetCDefCount()
        '        metaCache.cdefName = GetCDefNames()
        '        metaCache.cdefID = GetCDefIDs()
        '    Catch ex As Exception
        '        cpCore.handleException(ex)
        '    End Try
        'End Sub
        '
        '=============================================================================================
        ' ----- Load appServices DataSource values (but do not open until they are needed)
        '       Note: Default DataSource must be valid
        '=============================================================================================
        '
        Public Sub loadMetaCache_DataSources()
            Try
                Dim dt As DataTable
                Dim ptr As Integer = 0
                '
                dt = cpCore.db.executeSql("select id,name,typeId,address,username,password,ConnString from ccDataSources")
                If dt.Rows.Count > 0 Then
                    ReDim cpCore.db.dataSources(cpCore.db.dataSources.Length - 1)
                    For Each row As DataRow In dt.Rows
                        cpCore.db.dataSources(ptr).Id = EncodeInteger(row(0))
                        cpCore.db.dataSources(ptr).NameLower = EncodeText(row(1)).ToLower
                        cpCore.db.dataSources(ptr).dataSourceType = DirectCast(EncodeInteger(row(2)), dataSourceTypeEnum)
                        cpCore.db.dataSources(ptr).endPoint = EncodeText(row(3))
                        cpCore.db.dataSources(ptr).username = EncodeText(row(4))
                        cpCore.db.dataSources(ptr).password = EncodeText(row(5))
                        cpCore.db.dataSources(ptr).odbcConnectionString = EncodeText(row(6))
                        ptr += 1
                    Next
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '

        '========================================================================
        '   Verify AuthoringTable is complete
        '       find all Live Records without Authoring Records
        '       create an authoring record for each one found missing
        '           EditSourceID = Live.ID
        '           create new TextFile for each that needs it
        '========================================================================
        '
        Private Sub loadMetaCache_VerifyAuthoring()
            On Error GoTo ErrorTrap
            '
            Dim MethodName As String
            'Dim ContentPointer As Integer
            Dim ContentID As Integer
            Dim dtContent As DataTable
            'Dim dtAuthoring as datatable
            Dim ContentTableName As String
            Dim ContentDataSourceName As String
            Dim AuthoringTableName As String
            Dim AuthoringDataSourceName As String
            Dim FieldPointer As Integer
            Dim FieldCount As Integer
            'Dim Field.Name As String
            Dim FieldValueVariant As Object
            Dim SQLNames() As String
            Dim sqlFieldList As New sqlFieldListClass
            'Dim fieldType As Integer
            Dim SQL As String
            '
            Dim JoinCount As Integer                 ' number of joins for this content definition
            Dim JoinFieldPointer() As Integer        ' Field which contains join
            Dim JoinContent() As String           ' Content Definition of joined table
            '
            Dim EditFilename As String
            Dim EditRecordID As Integer
            '
            Dim LiveFilename As String
            Dim LiveRecordID As Integer
            '
            Dim ContentName As String
            '
            Dim RSJoin As DataTable
            Dim JoinPointer As Integer
            Dim JoinContentName As String
            Dim JoinContentPointer As Integer
            Dim iJoinFieldPointer As Integer
            Dim JoinFieldName As String
            Dim JoinFieldValue As Integer
            Dim JoinContentAuthoringTablename As String
            Dim JoinContentAuthoringDataSourceName As String
            Dim JoinRecordID As Integer
            '
            Dim Copy As String
            'Dim CDefArray() As appServices_metaDataClass.CDefClass
            'Dim CDefArrayCount As Integer
            Dim rowPtr As Integer
            'Dim kmafs As New fileSystemClass
            '
            MethodName = "new_loadCdefCache_loadContentEngineContentEngine_VerifyAuthoring"
            '
            ' 3/10/2009 - set Allow Workflow only on with the Request Workflow during startup
            '   RequestWorkflow is in the Preferences Settings Page, AllowWorkflow is read only
            '   The site should NOT use the AllowWorkflow site property, but the value of it during startup
            '
            If False Then
                '
                ' Database needs to be updated - Until this upgrade, Request Follow Allow
                ' After upgrade, Allow Follows Request
                '
                Call cpCore.siteProperties.setProperty("RequestWorkflowAuthoring", cpCore.siteProperties.getText("AllowWorkflowAuthoring", "0"))
            Else
                '
                ' Database is up-to-date, Set Allow from state of Request
                '
                Call cpCore.siteProperties.setProperty("AllowWorkflowAuthoring", cpCore.siteProperties.getText("RequestWorkflowAuthoring", "0"))
            End If
            '
            Dim dt As DataTable
            dt = cpCore.db.executeSql("select id,name from ccContent where active<>0")
            If Not (dt Is Nothing) Then
                For Each row As DataRow In dt.Rows
                    Dim cdef As CDefClass = getCdef(EncodeInteger(row("id")))
                    'If vbUCase(cdef.Name) = "PAGE CONTENT" Then
                    '    Copy = Copy
                    '    End If
                    ContentName = cdef.Name
                    With cdef
                        ContentTableName = .ContentTableName
                        ContentDataSourceName = .ContentDataSourceName
                        AuthoringTableName = .AuthoringTableName
                        AuthoringDataSourceName = .AuthoringDataSourceName
                        FieldCount = .fields.Count
                        ContentID = .Id
                    End With
                    If (ContentName <> "") And (ContentTableName <> "") And (ContentDataSourceName <> "") Then
                        If Not ((cpCore.siteProperties.allowWorkflowAuthoring) And (cdef.AllowWorkflowAuthoring)) Then
                            '
                            ' Not Authoring - delete edit and archive records
                            '
                            If (LCase(ContentTableName) = "ccpagecontent") Or (LCase(ContentTableName) = "cccopycontent") Then
                                '
                                ' for now, limit the housekeep to just the tables that are allowed to have workflow
                                ' to stop timeouts on members/visits/etc.
                                '
                                SQL = "delete from " & ContentTableName & " where (editsourceid>0)"
                                Call cpCore.db.executeSql(SQL, ContentDataSourceName)
                            End If
                        Else
                            '
                            ' Authoring - verify edit record
                            '
                            If FieldCount > 0 Then
                                '
                                ' mark all records with a 0 createkey so we can tell which ones are changed
                                ' NO -- THIS IS NOT USED IN THE REST OF THE PROC, AND IT IS SLOW
                                'SQL = "UPDATE " & AuthoringTableName & " Set CreateKey=0"
                                'Call executesql(SQL,ContentDataSourceName)
                                '
                                ' Select all records that need to be copied
                                '
                                SQL = "SELECT ContentTable.*" _
                                    & " FROM " & ContentTableName & " AS ContentTable LEFT JOIN " & AuthoringTableName & " AS AuthoringTable ON ContentTable.ID = AuthoringTable.EditSourceID" _
                                    & " WHERE ((ContentTable.ContentControlID=" & cpCore.db.encodeSQLNumber(ContentID) & ")AND(ContentTable.EditSourceID Is Null)AND(AuthoringTable.ID Is Null));"
                                dtContent = cpCore.db.executeSql(SQL)
                                If dtContent.Rows.Count > 0 Then
                                    Do While rowPtr < dtContent.Rows.Count

                                        '
                                        ' clear arrays
                                        '
                                        'ReDim SQLNames(FieldCount + 1)
                                        'ReDim sqlFieldList(FieldCount + 1)
                                        '
                                        ' Create sqlFieldList array for each live record
                                        '
                                        EditRecordID = cpCore.db.db_InsertTableRecordGetID(AuthoringDataSourceName, AuthoringTableName, SystemMemberID)
                                        LiveRecordID = EncodeInteger(dtContent.Rows(rowPtr).Item("ID"))
                                        If LiveRecordID = 159 Then
                                            LiveRecordID = LiveRecordID
                                        End If
                                        For Each keyValuePair In cdef.fields
                                            Dim field As CDefFieldClass = keyValuePair.Value
                                            Select Case field.fieldTypeId
                                                Case FieldTypeIdManyToMany, FieldTypeIdRedirect
                                                    '
                                                    ' These content field types have no Db field
                                                    '
                                                Case Else
                                                    '
                                                    ' reproduce the field types
                                                    '
                                                    FieldValueVariant = dtContent.Rows(rowPtr).Item(field.nameLc)
                                                    Select Case field.nameLc
                                                        Case "ID", "EDITSOURCEID", "EDITARCHIVE", ""
                                                        Case Else
                                                            If (field.fieldTypeId = FieldTypeIdFileTextPrivate) Or (field.fieldTypeId = FieldTypeIdFileHTMLPrivate) Then
                                                                '
                                                                ' create new text file and copy field - private files
                                                                '
                                                                EditFilename = ""
                                                                LiveFilename = EncodeText(FieldValueVariant)
                                                                If LiveFilename <> "" Then
                                                                    EditFilename = csv_GetVirtualFilenameByTable(AuthoringTableName, field.nameLc, EditRecordID, "", field.fieldTypeId)
                                                                    FieldValueVariant = EditFilename
                                                                    If EditFilename <> "" Then
                                                                        Copy = cpCore.privateFiles.ReadFile(convertCdnUrlToCdnPathFilename(EditFilename))
                                                                    End If
                                                                    'Copy = contentFiles.ReadFile(LiveFilename)
                                                                    If Copy <> "" Then
                                                                        Call cpCore.privateFiles.SaveFile(convertCdnUrlToCdnPathFilename(EditFilename), Copy)
                                                                        'Call publicFiles.SaveFile(EditFilename, Copy)
                                                                    End If
                                                                End If
                                                            End If
                                                            If (field.fieldTypeId = FieldTypeIdFileCSS) Or (field.fieldTypeId = FieldTypeIdFileXML) Or (field.fieldTypeId = FieldTypeIdFileJavascript) Then
                                                                '
                                                                ' create new text file and copy field - public files
                                                                '
                                                                EditFilename = ""
                                                                LiveFilename = EncodeText(FieldValueVariant)
                                                                If LiveFilename <> "" Then
                                                                    EditFilename = csv_GetVirtualFilenameByTable(AuthoringTableName, field.nameLc, EditRecordID, "", field.fieldTypeId)
                                                                    FieldValueVariant = EditFilename
                                                                    If EditFilename <> "" Then
                                                                        Copy = cpCore.cdnFiles.ReadFile(convertCdnUrlToCdnPathFilename(EditFilename))
                                                                    End If
                                                                    'Copy = contentFiles.ReadFile(LiveFilename)
                                                                    If Copy <> "" Then
                                                                        Call cpCore.cdnFiles.SaveFile(convertCdnUrlToCdnPathFilename(EditFilename), Copy)
                                                                        'Call publicFiles.SaveFile(EditFilename, Copy)
                                                                    End If
                                                                End If
                                                            End If
                                                            'SQLNames(FieldPointer) = field.Name
                                                            sqlFieldList.add(field.nameLc, cpCore.db.db_EncodeSQL(FieldValueVariant, field.fieldTypeId))
                                                            'end case
                                                    End Select
                                                    'end case
                                            End Select
                                        Next
                                        '
                                        ' EditSourceID
                                        '
                                        'SQLNames(FieldPointer) = "EDITSOURCEID"
                                        sqlFieldList.add("editsourceid", cpCore.db.encodeSQLNumber(LiveRecordID))
                                        FieldPointer = FieldPointer + 1
                                        '
                                        ' EditArchive
                                        '
                                        'SQLNames(FieldPointer) = "EDITARCHIVE"
                                        sqlFieldList.add("EDITARCHIVE", SQLFalse)
                                        FieldPointer = FieldPointer + 1
                                        '
                                        Call cpCore.db.db_UpdateTableRecord(AuthoringDataSourceName, AuthoringTableName, "ID=" & EditRecordID, sqlFieldList)
                                    Loop
                                    '
                                    ' ----- Verify good DateAdded
                                    '
                                    Dim testDate As Date = Date.MinValue
                                    SQL = "UPDATE " & ContentTableName & " set DateAdded=" & cpCore.db.encodeSQLDate(testDate) & " where dateadded is null;"
                                    Call cpCore.db.executeSql(SQL, ContentDataSourceName)
                                    If ContentTableName <> AuthoringTableName Then
                                        SQL = "UPDATE " & AuthoringTableName & " set DateAdded=" & cpCore.db.encodeSQLDate(testDate) & " where dateadded is null;"
                                        Call cpCore.db.executeSql(SQL, AuthoringDataSourceName)
                                    End If
                                    '
                                    ' ----- Verify good CreatedBy
                                    '
                                    SQL = "UPDATE " & ContentTableName & " set CreatedBy=" & cpCore.db.encodeSQLNumber(SystemMemberID) & " where CreatedBy is null;"
                                    Call cpCore.db.executeSql(SQL, ContentDataSourceName)
                                    If ContentTableName <> AuthoringTableName Then
                                        SQL = "UPDATE " & AuthoringTableName & " set CreatedBy=" & cpCore.db.encodeSQLNumber(SystemMemberID) & " where CreatedBy is null;"
                                        Call cpCore.db.executeSql(SQL, AuthoringDataSourceName)
                                    End If
                                    '
                                    ' ----- Verify good ModifiedDate
                                    '
                                    SQL = "UPDATE " & ContentTableName & " set ModifiedDate=DateAdded where ModifiedDate is null;"
                                    Call cpCore.db.executeSql(SQL, ContentDataSourceName)
                                    If ContentTableName <> AuthoringTableName Then
                                        SQL = "UPDATE " & AuthoringTableName & " set ModifiedDate=DateAdded where ModifiedDate is null;"
                                        Call cpCore.db.executeSql(SQL, AuthoringDataSourceName)
                                    End If
                                    '
                                    ' ----- Verify good ModifiedBy
                                    '
                                    SQL = "UPDATE " & ContentTableName & " set ModifiedBy=" & cpCore.db.encodeSQLNumber(SystemMemberID) & " where ModifiedBy is null;"
                                    Call cpCore.db.executeSql(SQL, ContentDataSourceName)
                                    If ContentTableName <> AuthoringTableName Then
                                        SQL = "UPDATE " & AuthoringTableName & " set ModifiedBy=" & cpCore.db.encodeSQLNumber(SystemMemberID) & " where ModifiedBy is null;"
                                        Call cpCore.db.executeSql(SQL, AuthoringDataSourceName)
                                    End If
                                    '
                                    ' ----- Verify good EditArchive in authoring table
                                    '
                                    SQL = "UPDATE " & AuthoringTableName & " set EditArchive=" & SQLFalse & " where EditArchive is null;"
                                    Call cpCore.db.executeSql(SQL, AuthoringDataSourceName)
                                    '
                                    ' ----- Verify good EditBlank
                                    '
                                    SQL = "UPDATE " & AuthoringTableName & " set EditBlank=" & SQLFalse & " where EditBlank is null;"
                                    Call cpCore.db.executeSql(SQL, AuthoringDataSourceName)
                                    If ContentTableName <> AuthoringTableName Then
                                        SQL = "UPDATE " & ContentTableName & " set EditBlank=" & SQLFalse & " where EditBlank is null;"
                                        Call cpCore.db.executeSql(SQL, ContentDataSourceName)
                                    End If
                                End If
                                dtContent = Nothing
                            End If
                        End If
                    End If
                Next
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            'Call handleLegacyClassError1(MethodName, "trap")
        End Sub
        ''
        ''===============================================================================
        ''   Load the records needed to run the Content Server
        ''===============================================================================
        ''
        'Public Function loadMetaCache() As Boolean
        '    Dim returnOk As Boolean = False
        '    Try
        '        Dim needToLoadCdefCache As Boolean = True
        '        Dim jsonMetaTemp As String
        '        Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer
        '        Const cacheName As String = "metaData"
        '        '
        '        ' legacy -- convert all to lazy cache and remove from the metaCache class -- too slow to load all at once
        '        '
        '        metaCache = New appServices_metaDataClass.metaCacheClass()
        '        metaCache.tableSchema = New Dictionary(Of String, appServices_metaDataClass.tableSchemaClass)
        '        'metaCache.cdefContentNameIndex = New keyPtrIndexClass
        '        'metaCache.cdefContentIDIndex = New keyPtrIndexClass
        '        'metaCache.cdefContentTableNameIndex = New keyPtrIndexClass
        '        'ReDim metaCache.cdefContentFieldIndex(0)
        '        'metaCache.cdefContentFieldIndex(0) = New keyPtrIndexClass
        '        '
        '        metaCache = DirectCast(cpCore.cache.cache_read(Of metaCacheClass)(cacheName), metaCacheClass)
        '        If metaCache Is Nothing Then
        '            needToLoadCdefCache = True
        '        Else
        '            With metaCache
        '                If (.contentNameIndexBag <> "") And (.contentIdIndexBag <> "") And (.contentTableNameIndexBag <> "") Then
        '                    '.cdefContentNameIndex.importPropertyBag(.contentNameIndexBag)
        '                    '.cdefContentIDIndex.importPropertyBag(.contentIdIndexBag)
        '                    .cdefContentTableNameIndex.importPropertyBag(.contentTableNameIndexBag)
        '                    needToLoadCdefCache = False
        '                    returnOk = True
        '                End If
        '            End With
        '        End If
        '        If needToLoadCdefCache Then
        '            Call loadMetaCache_DataSources()
        '            Call loadMetaCache_cdef()
        '            Call loadMetaCache_VerifyAuthoring()
        '            '
        '            With metaCache
        '                .contentNameIndexBag = .cdefContentNameIndex.exportPropertyBag()
        '                .contentIdIndexBag = .cdefContentIDIndex.exportPropertyBag()
        '                .contentTableNameIndexBag = .cdefContentTableNameIndex.exportPropertyBag()
        '                jsonMetaTemp = json_serializer.Serialize(metaCache)
        '            End With
        '            Call cpCore.cache.cache_save(cacheName, metaCache, "cdef")
        '            'Call cpCore.app.privateFiles.SaveFile("metaCache.json", jsonMetaTemp)
        '            returnOk = True
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleException(ex)
        '    End Try
        '    Return returnOk
        'End Function
        '
        '===========================================================================
        '   main_Get Authoring List
        '       returns a comma delimited list of ContentIDs that the Member can author
        '===========================================================================
        '
        Public Function getEditableCdefIdList() As List(Of Integer)
            Dim returnList As New List(Of Integer)
            Try
                '
                Dim SQL As String
                'Dim RS As DataTable
                Dim cidDataTable As DataTable
                Dim CIDCount As Integer
                Dim CIDPointer As Integer
                Dim CDef As coreMetaDataClass.CDefClass
                Dim ContentName As String
                Dim ContentID As Integer
                '
                SQL = "Select ccGroupRules.ContentID as ID" _
                & " FROM ((ccmembersrules" _
                & " Left Join ccGroupRules on ccMemberRules.GroupID=ccGroupRules.GroupID)" _
                & " Left Join ccContent on ccGroupRules.ContentID=ccContent.ID)" _
                & " WHERE" _
                    & " (ccMemberRules.MemberID=" & cpCore.user.id & ")" _
                    & " AND(ccGroupRules.Active<>0)" _
                    & " AND(ccContent.Active<>0)" _
                    & " AND(ccMemberRules.Active<>0)"
                cidDataTable = cpCore.db.executeSql(SQL)
                CIDCount = cidDataTable.Rows.Count
                For CIDPointer = 0 To CIDCount - 1
                    ContentID = EncodeInteger(cidDataTable.Rows(CIDPointer).Item(0))
                    returnList.Add(ContentID)
                    CDef = getCdef(ContentID)
                    If Not (CDef Is Nothing) Then
                        returnList.AddRange(CDef.childIdList)
                    End If
                    ''main_GetContentManagementList = main_GetContentManagementList & "," & CStr(ContentID)
                    'ContentName = metaData.getContentNameByID(ContentID)
                    'If ContentName <> "" Then
                    '    CDef = getCdef(ContentName)
                    '    If CDef.ChildIDList.Count > 0 Then
                    '        returnList.Add(CDef.ChildIDList)
                    '        'main_GetContentManagementList = main_GetContentManagementList & "," & CDef.ChildIDList
                    '    End If
                    'End If
                Next
                'If Len(main_GetContentManagementList) > 1 Then
                '    main_GetContentManagementList = Mid(main_GetContentManagementList, 2)
                'End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnList
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Clear all data from the metaData current instance. Next request will load from cache.
        ''' </summary>
        Public Sub clear()
            Try
                If (Not cdefList Is Nothing) Then
                    cdefList.Clear()
                End If
                If (Not tableSchemaList Is Nothing) Then
                    tableSchemaList.Clear()
                End If
                If (Not cdefNameIdXref Is Nothing) Then
                    cdefNameIdXref = Nothing
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '=================================================================================
        ' Returns a pointer into the cdefCache.tableSchema() array for the table that matches
        '   returns -1 if the table is not found
        '=================================================================================
        '
        public Function getTableSchema(ByVal TableName As String, ByVal DataSourceName As String) As coreMetaDataClass.tableSchemaClass
            Dim tableSchema As coreMetaDataClass.tableSchemaClass = Nothing
            Try
                Dim dt As DataTable
                Dim isInCache As Boolean = False
                Dim isInDb As Boolean = False
                'Dim readFromDb As Boolean
                Dim lowerTablename As String
                Dim buildCache As Boolean
                '
                If (DataSourceName <> "") And (DataSourceName <> "-1") And (DataSourceName.ToLower <> "default") Then
                    cpCore.handleExceptionAndRethrow(New NotImplementedException("alternate datasources not supported yet"))
                Else
                    If TableName <> "" Then
                        lowerTablename = TableName.ToLower
                        If (tableSchemaList) Is Nothing Then
                            tableSchemaList = New Dictionary(Of String, tableSchemaClass)
                        Else
                            isInCache = tableSchemaList.TryGetValue(lowerTablename, tableSchema)
                        End If
                        buildCache = Not isInCache
                        If isInCache Then
                            buildCache = tableSchema.Dirty
                        End If
                        If buildCache Then
                            '
                            ' cache needs to be built
                            '
                            dt = cpCore.db.getTableSchemaData(TableName)
                            If dt.Rows.Count <= 0 Then
                                tableSchema = Nothing
                            Else
                                isInDb = True
                                tableSchema = New coreMetaDataClass.tableSchemaClass
                                tableSchema.columns = New List(Of String)
                                tableSchema.indexes = New List(Of String)
                                tableSchema.TableName = lowerTablename
                                '
                                ' load columns
                                '
                                dt = cpCore.db.getColumnSchemaData(TableName)
                                If dt.Rows.Count > 0 Then
                                    For Each row As DataRow In dt.Rows
                                        tableSchema.columns.Add(EncodeText(row("COLUMN_NAME")).ToLower)
                                    Next
                                End If
                                '
                                ' Load the index schema
                                '
                                dt = cpCore.db.getIndexSchemaData(TableName)
                                If dt.Rows.Count > 0 Then
                                    For Each row As DataRow In dt.Rows
                                        tableSchema.indexes.Add(EncodeText(row("INDEX_NAME")).ToLower)
                                    Next
                                End If
                            End If
                            If Not isInDb And isInCache Then
                                tableSchemaList.Remove(lowerTablename)
                            ElseIf isInDb And (Not isInCache) Then
                                tableSchemaList.Add(lowerTablename, tableSchema)
                            ElseIf isInDb And isInCache Then
                                tableSchemaList(lowerTablename) = tableSchema
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return tableSchema
        End Function
        '
        '====================================================================================================
        Public Sub tableSchemaListClear()
            tableSchemaList.Clear()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' delete a content based on name or guid, without removing the table
        ''' </summary>
        ''' <param name="contentNameOrGuid"></param>
        Public Sub deleteContent(contentNameOrGuid As String)
            Try
                cpCore.db.executeSql("delete from cccontent where (name=" & cpCore.db.encodeSQLText(contentNameOrGuid) & ")or(ccguid=" & cpCore.db.encodeSQLText(contentNameOrGuid) & ")")
                cpCore.cache.invalidateTag("content")
                clear()
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' delete a content based on name or guid, without removing the table
        ''' </summary>
        ''' <param name="contentNameOrGuid"></param>
        Public Sub deleteContent(contentid As Integer)
            Try
                cpCore.db.executeSql("delete from cccontent where (id=" & cpCore.db.encodeSQLNumber(contentid) & ")")
                cpCore.cache.invalidateTag("content")
                clear()
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '=============================================================================
        ' Create a child content from a parent content
        '
        '   If child does not exist, copy everything from the parent
        '   If child already exists, add any missing fields from parent
        '=============================================================================
        '
        Public Sub CreateContentChild(ByVal ChildContentName As String, ByVal ParentContentName As String, ByVal MemberID As Integer)
            Try
                Dim DataSourceName As String = ""
                Dim MethodName As String
                Dim SQL As String
                Dim rs As DataTable
                Dim ChildContentID As Integer
                Dim ParentContentID As Integer
                'Dim StateOfAllowContentAutoLoad As Boolean
                Dim CSContent As Integer
                Dim CSNew As Integer
                Dim SelectFieldList As String
                Dim Fields() As String
                ' converted array to dictionary - Dim FieldPointer As Integer
                Dim FieldName As String
                Dim DateNow As Date
                '
                DateNow = Date.MinValue
                '
                MethodName = "csv_CreateContentChild"
                '
                ' ----- Prevent StateOfAllowContentAutoLoad
                '
                'StateOfAllowContentAutoLoad = AllowContentAutoLoad
                'AllowContentAutoLoad = False
                '
                ' ----- check if child already exists
                '
                SQL = "select ID from ccContent where name=" & cpCore.db.encodeSQLText(ChildContentName) & ";"
                rs = cpCore.db.executeSql(SQL)
                If isDataTableOk(rs) Then
                    ChildContentID = EncodeInteger(cpCore.db.db_getDataRowColumnName(rs.Rows(0), "ID"))
                    '
                    ' mark the record touched so upgrade will not delete it
                    '
                    Call cpCore.db.executeSql("update ccContent set CreateKey=0 where ID=" & ChildContentID)
                End If
                Call closeDataTable(rs)
                If (isDataTableOk(rs)) Then
                    If False Then
                        'RS.Dispose)
                    End If
                    'RS = Nothing
                End If
                If ChildContentID = 0 Then
                    '
                    ' Get ContentID of parent
                    '
                    SQL = "select ID from ccContent where name=" & cpCore.db.encodeSQLText(ParentContentName) & ";"
                    rs = cpCore.db.executeSql(SQL, DataSourceName)
                    If isDataTableOk(rs) Then
                        ParentContentID = EncodeInteger(cpCore.db.db_getDataRowColumnName(rs.Rows(0), "ID"))
                        '
                        ' mark the record touched so upgrade will not delete it
                        '
                        Call cpCore.db.executeSql("update ccContent set CreateKey=0 where ID=" & ParentContentID)
                    End If
                    Call closeDataTable(rs)
                    If (isDataTableOk(rs)) Then
                        If False Then
                            'RS.Close()
                        End If
                        'RS = Nothing
                    End If
                    '
                    If ParentContentID = 0 Then
                        cpCore.handleExceptionAndRethrow(New ApplicationException("Can not create Child Content [" & ChildContentName & "] because the Parent Content [" & ParentContentName & "] was not found."))
                    Else
                        '
                        ' ----- create child content record, let the csv_ExecuteSQL reload CDef
                        '
                        DataSourceName = "Default"
                        CSContent = cpCore.db.db_OpenCSContentRecord("Content", ParentContentID)
                        If Not cpCore.db.cs_Ok(CSContent) Then
                            cpCore.handleExceptionAndRethrow(New ApplicationException("Can not create Child Content [" & ChildContentName & "] because the Parent Content [" & ParentContentName & "] was not found."))
                        Else
                            SelectFieldList = cpCore.db.db_GetCSSelectFieldList(CSContent)
                            If SelectFieldList = "" Then
                                cpCore.handleExceptionAndRethrow(New ApplicationException("Can not create Child Content [" & ChildContentName & "] because the Parent Content [" & ParentContentName & "] record has not fields."))
                            Else
                                CSNew = cpCore.db.cs_insertRecord("Content", 0)
                                If Not cpCore.db.cs_Ok(CSNew) Then
                                    Call cpCore.handleExceptionAndRethrow(New ApplicationException("Can not create Child Content [" & ChildContentName & "] because there was an error creating a new record in ccContent."))
                                Else
                                    Fields = Split(SelectFieldList, ",")
                                    DateNow = DateTime.Now()
                                    For FieldPointer = 0 To UBound(Fields)
                                        FieldName = Fields(FieldPointer)
                                        Select Case vbUCase(FieldName)
                                            Case "ID"
                                            ' do nothing
                                            Case "NAME"
                                                Call cpCore.db.db_SetCSField(CSNew, FieldName, ChildContentName)
                                            Case "PARENTID"
                                                Call cpCore.db.db_SetCSField(CSNew, FieldName, cpCore.db.cs_getText(CSContent, "ID"))
                                            Case "CREATEDBY", "MODIFIEDBY"
                                                Call cpCore.db.db_SetCSField(CSNew, FieldName, MemberID)
                                            Case "DATEADDED", "MODIFIEDDATE"
                                                Call cpCore.db.db_SetCSField(CSNew, FieldName, DateNow)
                                            Case "CCGUID"

                                                '
                                                ' new, non-blank guid so if this cdef is exported, it will be updateable
                                                '
                                                Call cpCore.db.db_SetCSField(CSNew, FieldName, Guid.NewGuid.ToString())
                                            Case Else
                                                Call cpCore.db.db_SetCSField(CSNew, FieldName, cpCore.db.cs_getText(CSContent, FieldName))
                                        End Select
                                    Next
                                End If
                                Call cpCore.db.cs_Close(CSNew)
                            End If
                        End If
                        Call cpCore.db.cs_Close(CSContent)
                        'SQL = "INSERT INTO ccContent ( Name, Active, DateAdded, CreatedBy, ModifiedBy, ModifiedDate, AllowAdd, DeveloperOnly, AdminOnly, CreateKey, SortOrder, ContentControlID, AllowDelete, ParentID, EditSourceID, EditArchive, EditBlank, ContentTableID, AuthoringTableID, AllowWorkflowAuthoring, DefaultSortMethodID, DropDownFieldList, EditorGroupID )" _
                        '    & " SELECT " & encodeSQLText(ChildContentName) & " AS Name, ccContent.Active, ccContent.DateAdded, ccContent.CreatedBy, ccContent.ModifiedBy, ccContent.ModifiedDate, ccContent.AllowAdd, ccContent.DeveloperOnly, ccContent.AdminOnly, ccContent.CreateKey, ccContent.SortOrder, ccContent.ContentControlID, ccContent.AllowDelete, ccContent.ID, ccContent.EditSourceID, ccContent.EditArchive, ccContent.EditBlank, ccContent.ContentTableID, ccContent.AuthoringTableID, ccContent.AllowWorkflowAuthoring, ccContent.DefaultSortMethodID, ccContent.DropDownFieldList, ccContent.EditorGroupID" _
                        '    & " From ccContent" _
                        '    & " WHERE (((ccContent.ID)=" & encodeSQLNumber(ParentContentID) & "));"
                        'Call csv_ExecuteSQL(sql,DataSourceName)
                    End If
                End If
                '
                ' ----- Load CDef
                '
                cpCore.cache.invalidateAll()
                clear()
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in CreateContentChild"))
            End Try
        End Sub
        '
        '=============================================================================
        '   Return just the copy from a content page
        '=============================================================================
        '
        Public Function TextDeScramble(ByVal Copy As String) As String
            Dim returnCopy As String = ""
            Try
                Dim CS As Integer
                Dim CPtr As Integer
                Dim C As String
                Dim CValue As Integer
                Dim crc As Integer
                Dim ModAnswer As String
                Dim Source As String
                Dim Base As Integer
                Const CMin = 32
                Const CMax = 126
                '
                ' assume this one is not converted
                '
                Source = Copy
                Base = 50
                '
                ' First characger must be _
                ' Second character is the scramble version 'a' is the starting system
                '
                If Mid(Source, 1, 2) <> "_a" Then
                    returnCopy = Copy
                Else
                    Source = Mid(Source, 3)
                    '
                    ' cycle through all characters
                    '
                    For CPtr = Len(Source) - 1 To 1 Step -1
                        C = Mid(Source, CPtr, 1)
                        CValue = Asc(C)
                        crc = crc + CValue
                        If (CValue < CMin) Or (CValue > CMax) Then
                            '
                            ' if out of ascii bounds, just leave it in place
                            '
                        Else
                            CValue = CValue - Base
                            If CValue < CMin Then
                                CValue = CValue + CMax - CMin + 1
                            End If
                        End If
                        returnCopy = returnCopy & Chr(CValue)
                    Next
                    '
                    ' Test mod
                    '
                    If CStr(crc Mod 9) <> Mid(Source, Len(Source), 1) Then
                        '
                        ' Nope - set it back to the input
                        '
                        returnCopy = Copy
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnCopy
        End Function

        '
        '=============================================================================
        '   Return just the copy from a content page
        '=============================================================================
        '
        Public Function TextScramble(ByVal Copy As String) As String
            Dim returnCopy As String = ""
            Try
                Dim CS As Integer
                Dim CPtr As Integer
                Dim C As String
                Dim CValue As Integer
                Dim crc As Integer
                Dim Base As Integer
                Const CMin = 32
                Const CMax = 126
                '
                ' scrambled starts with _
                '
                Base = 50
                For CPtr = 1 To Len(Copy)
                    C = Mid(Copy, CPtr, 1)
                    CValue = Asc(C)
                    If (CValue < CMin) Or (CValue > CMax) Then
                        '
                        ' if out of ascii bounds, just leave it in place
                        '
                    Else
                        CValue = CValue + Base
                        If CValue > CMax Then
                            CValue = CValue - CMax + CMin - 1
                        End If
                    End If
                    '
                    ' CRC is addition of all scrambled characters
                    '
                    crc = crc + CValue
                    '
                    ' put together backwards
                    '
                    returnCopy = Chr(CValue) & returnCopy
                Next
                '
                ' Ends with the mod of the CRC and 13
                '
                returnCopy = "_a" & returnCopy & CStr(crc Mod 9)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnCopy
        End Function
        '
        '========================================================================
        ' Get a Contents Tablename from the ContentPointer
        '========================================================================
        '
        Public Function getContentTablename(ByVal ContentName As String) As String
            Dim returnTableName As String = ""
            Try
                Dim CDef As coreMetaDataClass.CDefClass
                '
                CDef = getCdef(ContentName)
                If (CDef IsNot Nothing) Then
                    returnTableName = CDef.ContentTableName
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnTableName
        End Function
        '
        '========================================================================
        ' ----- Get a DataSource Name from its ContentName
        '
        Public Function getContentDataSource(ContentName As String) As String
            Dim returnDataSource As String = ""
            Try
                Dim CDef As coreMetaDataClass.CDefClass
                '
                CDef = getCdef(ContentName)
                If (CDef Is Nothing) Then
                    '
                Else
                    returnDataSource = CDef.ContentDataSourceName
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnDataSource
        End Function
        '
        '========================================================================
        ' Get a Contents Name from the ContentID
        '   Bad ContentID returns blank
        '========================================================================
        '
        Public Function getContentNameByID(ByVal ContentID As Integer) As String
            Dim returnName As String = ""
            Try
                Dim cdef As coreMetaDataClass.CDefClass
                '
                cdef = getCdef(ContentID)
                If cdef IsNot Nothing Then
                    returnName = cdef.Name
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnName
        End Function

        '========================================================================
        '   Create a content definition
        '       called from upgrade and DeveloperTools
        '========================================================================
        '
        Public Function metaData_CreateContent4(ByVal Active As Boolean, ByVal DataSourceName As String, ByVal TableName As String, ByVal ContentName As String, Optional ByVal AdminOnly As Boolean = False, Optional ByVal DeveloperOnly As Boolean = False, Optional ByVal AllowAdd As Boolean = True, Optional ByVal AllowDelete As Boolean = True, Optional ByVal ParentName As String = "", Optional ByVal DefaultSortMethod As String = "", Optional ByVal DropDownFieldList As String = "", Optional ByVal AllowWorkflowAuthoring As Boolean = False, Optional ByVal AllowCalendarEvents As Boolean = False, Optional ByVal AllowContentTracking As Boolean = False, Optional ByVal AllowTopicRules As Boolean = False, Optional ByVal AllowContentChildTool As Boolean = False, Optional ByVal AllowMetaContent As Boolean = False, Optional ByVal IconLink As String = "", Optional ByVal IconWidth As Integer = 0, Optional ByVal IconHeight As Integer = 0, Optional ByVal IconSprites As Integer = 0, Optional ByVal ccGuid As String = "", Optional ByVal IsBaseContent As Boolean = False, Optional ByVal installedByCollectionGuid As String = "", Optional clearMetaCache As Boolean = False) As Integer
            Dim returnContentId As Integer = 0
            Try
                '
                Dim ContentIsBaseContent As Boolean
                Dim NewGuid As String
                Dim SupportsGuid As Boolean
                Dim LcContentGuid As String
                Dim SQL As String
                Dim parentId As Integer
                Dim dt As DataTable
                Dim TableID As Integer
                Dim DataSourceID As Integer
                Dim iDefaultSortMethod As String
                Dim DefaultSortMethodID As Integer
                Dim CDefFound As Boolean
                Dim InstalledByCollectionID As Integer
                Dim sqlList As sqlFieldListClass
                Dim field As coreMetaDataClass.CDefFieldClass
                Dim ContentIDofContent As Integer
                '
                If ContentName = "" Then
                    cpCore.handleExceptionAndRethrow(New ApplicationException("ContentName can not be blank"))
                Else
                    '
                    If TableName = "" Then
                        cpCore.handleExceptionAndRethrow(New ApplicationException("Tablename can not be blanl"))
                    Else
                        '
                        ' Create the SQL table
                        '
                        Call cpCore.db.db_CreateSQLTable(DataSourceName, TableName)
                        '
                        ' Check for a Content Definition
                        '
                        returnContentId = 0
                        LcContentGuid = ""
                        ContentIsBaseContent = False
                        NewGuid = encodeEmptyText(ccGuid, "")
                        '
                        ' get contentId, guid, IsBaseContent
                        '
                        SQL = "select ID,ccguid,IsBaseContent  from ccContent where (name=" & cpCore.db.encodeSQLText(ContentName) & ") order by id;"
                        dt = cpCore.db.executeSql(SQL)
                        If dt.Rows.Count > 0 Then
                            returnContentId = EncodeInteger(dt.Rows(0).Item("ID"))
                            LcContentGuid = vbLCase(EncodeText(dt.Rows(0).Item("ccguid")))
                            ContentIsBaseContent = EncodeBoolean(dt.Rows(0).Item("IsBaseContent"))
                        End If
                        dt.Dispose()
                        '
                        ' get contentid of content
                        '
                        ContentIDofContent = 0
                        If ContentName.ToLower() = "content" Then
                            ContentIDofContent = returnContentId
                        Else
                            SQL = "select ID from ccContent where (name='content') order by id;"
                            dt = cpCore.db.executeSql(SQL)
                            If dt.Rows.Count > 0 Then
                                ContentIDofContent = EncodeInteger(dt.Rows(0).Item("ID"))
                            End If
                            dt.Dispose()
                        End If
                        '
                        ' get parentId
                        '
                        If Not String.IsNullOrEmpty(ParentName) Then
                            SQL = "select id from ccContent where (name=" & cpCore.db.encodeSQLText(ParentName) & ") order by id;"
                            dt = cpCore.db.executeSql(SQL)
                            If dt.Rows.Count > 0 Then
                                parentId = EncodeInteger(dt.Rows(0).Item(0))
                            End If
                            dt.Dispose()
                        End If
                        '
                        ' get InstalledByCollectionID
                        '
                        InstalledByCollectionID = 0
                        If (installedByCollectionGuid <> "") Then
                            SQL = "select id from ccAddonCollections where ccGuid=" & cpCore.db.encodeSQLText(installedByCollectionGuid)
                            dt = cpCore.db.executeSql(SQL)
                            If dt.Rows.Count > 0 Then
                                InstalledByCollectionID = EncodeInteger(dt.Rows(0).Item("ID"))
                            End If
                        End If
                        '
                        ' Block non-base update of a base field
                        '
                        If ContentIsBaseContent And Not IsBaseContent Then
                            '
                            '
                            '
                            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Attempt to update a Base Content Definition [" & ContentName & "] as non-base. This is not allowed."))
                        Else
                            CDefFound = (returnContentId <> 0)
                            If Not CDefFound Then
                                '
                                ' ----- Create a new empty Content Record (to get ContentID)
                                '
                                returnContentId = cpCore.db.db_InsertTableRecordGetID("Default", "ccContent", SystemMemberID)
                            End If
                            '
                            ' ----- Get the Table Definition ID, create one if missing
                            '
                            SQL = "SELECT ID from ccTables where (active<>0) and (name=" & cpCore.db.encodeSQLText(TableName) & ");"
                            dt = cpCore.db.executeSql(SQL)
                            If dt.Rows.Count <= 0 Then
                                '
                                ' ----- no table definition found, create one
                                '
                                If vbUCase(DataSourceName) = "DEFAULT" Then
                                    DataSourceID = -1
                                ElseIf DataSourceName = "" Then
                                    DataSourceID = -1
                                Else
                                    DataSourceID = cpCore.db.db_GetDataSourceID(DataSourceName)
                                    If DataSourceID = -1 Then
                                        Call cpCore.handleExceptionAndRethrow(New ApplicationException("Could not find DataSource [" & DataSourceName & "] for table [" & TableName & "]"))
                                    End If
                                End If
                                TableID = cpCore.db.db_InsertTableRecordGetID("Default", "ccTables", SystemMemberID)
                                '
                                sqlList = New sqlFieldListClass
                                sqlList.add("name", cpCore.db.encodeSQLText(TableName))
                                sqlList.add("active", SQLTrue)
                                sqlList.add("DATASOURCEID", cpCore.db.encodeSQLNumber(DataSourceID))
                                sqlList.add("CONTENTCONTROLID", cpCore.db.encodeSQLNumber(cpCore.db.db_GetContentID("Tables")))
                                '
                                Call cpCore.db.db_UpdateTableRecord("Default", "ccTables", "ID=" & TableID, sqlList)
                            Else
                                TableID = EncodeInteger(dt.Rows(0).Item("ID"))
                            End If
                            '
                            ' ----- Get Sort Method ID from SortMethod
                            iDefaultSortMethod = encodeEmptyText(DefaultSortMethod, "")
                            DefaultSortMethodID = 0
                            '
                            ' First - try lookup by name
                            '
                            If iDefaultSortMethod = "" Then
                                DefaultSortMethodID = 0
                            Else
                                dt = cpCore.db.db_openTable("Default", "ccSortMethods", "(name=" & cpCore.db.encodeSQLText(iDefaultSortMethod) & ")and(active<>0)", "ID", "ID", 1, 1)
                                If dt.Rows.Count > 0 Then
                                    DefaultSortMethodID = EncodeInteger(dt.Rows(0).Item("ID"))
                                End If
                            End If
                            If DefaultSortMethodID = 0 Then
                                '
                                ' fallback - maybe they put the orderbyclause in (common mistake)
                                '
                                dt = cpCore.db.db_openTable("Default", "ccSortMethods", "(OrderByClause=" & cpCore.db.encodeSQLText(iDefaultSortMethod) & ")and(active<>0)", "ID", "ID", 1, 1)
                                If dt.Rows.Count > 0 Then
                                    DefaultSortMethodID = EncodeInteger(dt.Rows(0).Item("ID"))
                                End If
                            End If
                            '
                            ' determine parentId from parentName
                            '

                            '
                            ' ----- update record
                            '
                            sqlList = New sqlFieldListClass
                            Call sqlList.add("name", cpCore.db.encodeSQLText(ContentName))
                            Call sqlList.add("CREATEKEY", "0")
                            Call sqlList.add("active", cpCore.db.encodeSQLBoolean(Active))
                            Call sqlList.add("ContentControlID", cpCore.db.encodeSQLNumber(ContentIDofContent))
                            Call sqlList.add("AllowAdd", cpCore.db.encodeSQLBoolean(AllowAdd))
                            Call sqlList.add("AllowDelete", cpCore.db.encodeSQLBoolean(AllowDelete))
                            Call sqlList.add("AllowWorkflowAuthoring", cpCore.db.encodeSQLBoolean(AllowWorkflowAuthoring))
                            Call sqlList.add("DeveloperOnly", cpCore.db.encodeSQLBoolean(DeveloperOnly))
                            Call sqlList.add("AdminOnly", cpCore.db.encodeSQLBoolean(AdminOnly))
                            Call sqlList.add("ParentID", cpCore.db.encodeSQLNumber(parentId))
                            Call sqlList.add("DefaultSortMethodID", cpCore.db.encodeSQLNumber(DefaultSortMethodID))
                            Call sqlList.add("DropDownFieldList", cpCore.db.encodeSQLText(encodeEmptyText(DropDownFieldList, "Name")))
                            Call sqlList.add("ContentTableID", cpCore.db.encodeSQLNumber(TableID))
                            Call sqlList.add("AuthoringTableID", cpCore.db.encodeSQLNumber(TableID))
                            Call sqlList.add("ModifiedDate", cpCore.db.encodeSQLDate(Now))
                            Call sqlList.add("CreatedBy", cpCore.db.encodeSQLNumber(SystemMemberID))
                            Call sqlList.add("ModifiedBy", cpCore.db.encodeSQLNumber(SystemMemberID))
                            Call sqlList.add("AllowCalendarEvents", cpCore.db.encodeSQLBoolean(AllowCalendarEvents))
                            Call sqlList.add("AllowContentTracking", cpCore.db.encodeSQLBoolean(AllowContentTracking))
                            Call sqlList.add("AllowTopicRules", cpCore.db.encodeSQLBoolean(AllowTopicRules))
                            Call sqlList.add("AllowContentChildTool", cpCore.db.encodeSQLBoolean(AllowContentChildTool))
                            Call sqlList.add("AllowMetaContent", cpCore.db.encodeSQLBoolean(AllowMetaContent))
                            Call sqlList.add("IconLink", cpCore.db.encodeSQLText(encodeEmptyText(IconLink, "")))
                            Call sqlList.add("IconHeight", cpCore.db.encodeSQLNumber(IconHeight))
                            Call sqlList.add("IconWidth", cpCore.db.encodeSQLNumber(IconWidth))
                            Call sqlList.add("IconSprites", cpCore.db.encodeSQLNumber(IconSprites))
                            Call sqlList.add("installedByCollectionid", cpCore.db.encodeSQLNumber(InstalledByCollectionID))
                            If SupportsGuid Then
                                If (LcContentGuid = "") And (NewGuid <> "") Then
                                    '
                                    ' hard one - only update guid if the tables supports it, and it the new guid is not blank
                                    ' if the new guid does no match te old guid
                                    '
                                    Call sqlList.add("ccGuid", cpCore.db.encodeSQLText(NewGuid))
                                ElseIf (NewGuid <> "") And (LcContentGuid <> vbLCase(NewGuid)) Then
                                    '
                                    ' new guid does not match current guid
                                    '
                                    'cpCore.AppendLog("upgrading cdef [" & ContentName & "], the guid was not updated because the current guid [" & LcContentGuid & "] is not empty, and it did not match the new guid [" & vbLCase(NewGuid) & "]")
                                    'Call AppendLog2(cpCore,appEnvironment.name, "upgrading cdef [" & ContentName & "], the guid was not updated because the current guid [" & LcContentGuid & "] is not empty, and it did not match the new guid [" & vbLCase(NewGuid) & "]", "dll", "cpCoreClass", "csv_CreateContent3", 0, "", "", False, True, "", "", "")
                                End If
                            End If
                            If returnContentId = 54 Then
                                returnContentId = returnContentId
                            End If
                            Call cpCore.db.db_UpdateTableRecord("Default", "ccContent", "ID=" & returnContentId, sqlList)
                            '
                            '-----------------------------------------------------------------------------------------------
                            ' Verify Core Content Definition Fields
                            '-----------------------------------------------------------------------------------------------
                            '
                            If parentId < 1 Then
                                '
                                ' CDef does not inherit its fields, create what is needed for a non-inherited CDef
                                '
                                If Not cpCore.db.db_isCdefField(returnContentId, "ID") Then
                                    field = New coreMetaDataClass.CDefFieldClass
                                    field.nameLc = "id"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdAutoIdIncrement
                                    field.editSortPriority = 100
                                    field.authorable = False
                                    field.caption = "ID"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call metaData_VerifyCDefField_ReturnID(ContentName, field)
                                End If
                                '
                                If Not cpCore.db.db_isCdefField(returnContentId, "name") Then
                                    field = New coreMetaDataClass.CDefFieldClass
                                    field.nameLc = "name"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdText
                                    field.editSortPriority = 110
                                    field.authorable = True
                                    field.caption = "Name"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call metaData_VerifyCDefField_ReturnID(ContentName, field)
                                End If
                                '
                                If Not cpCore.db.db_isCdefField(returnContentId, "active") Then
                                    field = New coreMetaDataClass.CDefFieldClass
                                    field.nameLc = "active"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdBoolean
                                    field.editSortPriority = 200
                                    field.authorable = True
                                    field.caption = "Active"
                                    field.defaultValue = "1"
                                    field.isBaseField = IsBaseContent
                                    Call metaData_VerifyCDefField_ReturnID(ContentName, field)
                                End If
                                '
                                If Not cpCore.db.db_isCdefField(returnContentId, "sortorder") Then
                                    field = New coreMetaDataClass.CDefFieldClass
                                    field.nameLc = "sortorder"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdText
                                    field.editSortPriority = 2000
                                    field.authorable = False
                                    field.caption = "Alpha Sort Order"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call metaData_VerifyCDefField_ReturnID(ContentName, field)
                                End If
                                '
                                If Not cpCore.db.db_isCdefField(returnContentId, "dateadded") Then
                                    field = New coreMetaDataClass.CDefFieldClass
                                    field.nameLc = "dateadded"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdDate
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Date Added"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call metaData_VerifyCDefField_ReturnID(ContentName, field)
                                End If
                                If Not cpCore.db.db_isCdefField(returnContentId, "createdby") Then
                                    field = New coreMetaDataClass.CDefFieldClass
                                    field.nameLc = "createdby"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdLookup
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Created By"
                                    field.lookupContentName = "People"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call metaData_VerifyCDefField_ReturnID(ContentName, field)
                                End If
                                If Not cpCore.db.db_isCdefField(returnContentId, "modifieddate") Then
                                    field = New coreMetaDataClass.CDefFieldClass
                                    field.nameLc = "modifieddate"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdDate
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Date Modified"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call metaData_VerifyCDefField_ReturnID(ContentName, field)
                                End If
                                If Not cpCore.db.db_isCdefField(returnContentId, "modifiedby") Then
                                    field = New coreMetaDataClass.CDefFieldClass
                                    field.nameLc = "modifiedby"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdLookup
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Modified By"
                                    field.lookupContentName = "People"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call metaData_VerifyCDefField_ReturnID(ContentName, field)
                                End If
                                If Not cpCore.db.db_isCdefField(returnContentId, "ContentControlID") Then
                                    field = New coreMetaDataClass.CDefFieldClass
                                    field.nameLc = "ContentControlID"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdLookup
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Controlling Content"
                                    field.lookupContentName = "Content"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call metaData_VerifyCDefField_ReturnID(ContentName, field)
                                End If
                                If Not cpCore.db.db_isCdefField(returnContentId, "CreateKey") Then
                                    field = New coreMetaDataClass.CDefFieldClass
                                    field.nameLc = "CreateKey"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdInteger
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Create Key"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call metaData_VerifyCDefField_ReturnID(ContentName, field)
                                End If
                                '
                                ' REFACTOR - these fieldsonly apply to page content
                                '
                                If Not cpCore.db.db_isCdefField(returnContentId, "EditSourceID") Then
                                    field = New coreMetaDataClass.CDefFieldClass
                                    field.nameLc = "EditSourceID"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdInteger
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Edit Source ID"
                                    field.lookupContentName = ""
                                    field.defaultValue = "null"
                                    field.isBaseField = IsBaseContent
                                    Call metaData_VerifyCDefField_ReturnID(ContentName, field)
                                End If
                                If Not cpCore.db.db_isCdefField(returnContentId, "EditArchive") Then
                                    field = New coreMetaDataClass.CDefFieldClass
                                    field.nameLc = "EditArchive"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdBoolean
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Edit Archive"
                                    field.lookupContentName = ""
                                    field.defaultValue = "0"
                                    field.isBaseField = IsBaseContent
                                    Call metaData_VerifyCDefField_ReturnID(ContentName, field)
                                End If
                                If Not cpCore.db.db_isCdefField(returnContentId, "EditBlank") Then
                                    field = New coreMetaDataClass.CDefFieldClass
                                    field.nameLc = "EditBlank"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdBoolean
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Edit Blank"
                                    field.lookupContentName = ""
                                    field.defaultValue = "0"
                                    field.isBaseField = IsBaseContent
                                    Call metaData_VerifyCDefField_ReturnID(ContentName, field)
                                End If
                                If Not cpCore.db.db_isCdefField(returnContentId, "ContentCategoryID") Then
                                    field = New coreMetaDataClass.CDefFieldClass
                                    field.nameLc = "ContentCategoryID"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdLookup
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Content Category"
                                    field.lookupContentName = "Content Categories"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call metaData_VerifyCDefField_ReturnID(ContentName, field)
                                End If
                                If Not cpCore.db.db_isCdefField(returnContentId, "ccGuid") Then
                                    field = New coreMetaDataClass.CDefFieldClass
                                    field.nameLc = "ccGuid"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdText
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Guid"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call metaData_VerifyCDefField_ReturnID(ContentName, field)
                                End If
                            End If
                            '
                            ' ----- Load CDef
                            '
                            If clearMetaCache Then
                                cpCore.cache.invalidateTagCommaList("content,content fields")
                                cpCore.metaData.clear()
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnContentId
        End Function
        '
        ' ====================================================================================================================
        '   Verify a CDef field and return the recordid
        '       same a old csv_CreateContentField
        '      args is a delimited name=value pair sring: a=1,b=2,c=3 where delimiter = ","
        '
        ' ***** add optional argument, doNotOverWrite -- called true from csv_CreateContent3 so if the cdef is there, it's fields will not be crushed.
        '
        ' ====================================================================================================================
        '
        Public Function metaData_VerifyCDefField_ReturnID(ByVal ContentName As String, field As coreMetaDataClass.CDefFieldClass) As Integer ' , ByVal FieldName As String, ByVal Args As String, ByVal Delimiter As String) As Integer
            Dim returnId As Integer = 0
            Try
                '
                Dim RecordIsBaseField As Boolean
                Dim IsBaseField As Boolean
                Dim SQL As String
                Dim dt As DataTable
                Dim ContentID As Integer
                Dim Pointer As Integer
                Dim SQLName(100) As String
                Dim SQLValue(100) As String
                Dim MethodName As String
                Dim LookupContentID As Integer
                Dim RecordID As Integer
                Dim TableID As Integer
                Dim TableName As String
                Dim DataSourceID As Integer
                Dim DataSourceName As String

                Dim FieldReadOnly As Boolean
                Dim FieldActive As Boolean
                Dim fieldTypeId As Integer
                Dim FieldCaption As String
                'Dim FieldSortOrder As Integer
                Dim FieldAuthorable As Boolean
                Dim LookupContentName As String
                Dim DefaultValue As String
                Dim NotEditable As Boolean
                'Dim field.indexColumn As Integer
                Dim AdminIndexWidth As String
                Dim AdminIndexSort As Integer
                Dim RedirectContentName As String
                Dim RedirectIDField As String
                Dim RedirectPath As String
                Dim HTMLContent As Boolean
                Dim UniqueName As Boolean
                Dim Password As Boolean
                Dim RedirectContentID As Integer
                Dim FieldRequired As Boolean
                Dim StateOfAllowContentAutoLoad As Boolean
                Dim RSSTitle As Boolean
                Dim RSSDescription As Boolean
                'Dim FieldAdminOnly As Boolean
                Dim FieldDeveloperOnly As Boolean
                Dim MemberSelectGroupID As Integer
                Dim installedByCollectionGuid As String
                Dim InstalledByCollectionID As Integer
                Dim EditTab As String
                Dim Scramble As Boolean
                Dim LookupList As String
                Dim ManyToManyContent As String
                Dim ManyToManyContentID As Integer
                Dim ManyToManyRuleContent As String
                Dim ManyToManyRuleContentID As Integer
                Dim ManyToManyRulePrimaryField As String
                Dim ManyToManyRuleSecondaryField As String
                Dim rs As DataTable
                Dim isNewFieldRecord As Boolean = True
                '
                MethodName = "csv_VerifyCDefField_ReturnID(" & ContentName & "," & field.nameLc & ")"
                '
                '
                If (UCase(ContentName) = "PAGE CONTENT") And (UCase(field.nameLc) = "ACTIVE") Then
                    field.nameLc = field.nameLc
                End If
                '
                ' Prevent load during the changes
                '
                'StateOfAllowContentAutoLoad = AllowContentAutoLoad
                'AllowContentAutoLoad = False
                '
                ' determine contentid and tableid
                '
                ContentID = -1
                TableID = 0
                SQL = "select ID,ContentTableID from ccContent where name=" & cpCore.db.encodeSQLText(ContentName) & ";"
                rs = cpCore.db.executeSql(SQL)
                If isDataTableOk(rs) Then
                    ContentID = EncodeInteger(cpCore.db.db_getDataRowColumnName(rs.Rows(0), "ID"))
                    TableID = EncodeInteger(cpCore.db.db_getDataRowColumnName(rs.Rows(0), "ContentTableID"))
                End If
                '
                ' test if field definition found or not
                '
                RecordID = 0
                RecordIsBaseField = False
                SQL = "select ID,IsBaseField from ccFields where (ContentID=" & cpCore.db.encodeSQLNumber(ContentID) & ")and(name=" & cpCore.db.encodeSQLText(field.nameLc) & ");"
                rs = cpCore.db.executeSql(SQL)
                If isDataTableOk(rs) Then
                    isNewFieldRecord = False
                    RecordID = EncodeInteger(cpCore.db.db_getDataRowColumnName(rs.Rows(0), "ID"))
                    RecordIsBaseField = EncodeBoolean(cpCore.db.db_getDataRowColumnName(rs.Rows(0), "IsBaseField"))
                End If
                '
                ' check if this is a non-base field updating a base field
                '
                IsBaseField = field.isBaseField
                If (Not IsBaseField) And (RecordIsBaseField) Then
                    '
                    ' This update is not allowed
                    '
                    cpCore.handleLegacyError2("cpCoreClass", "csv_VerifyCDefField_ReturnID", cpCore.appConfig.name & ", Warning, a Base field Is being updated To non-base. This should only happen When a base field Is removed from the base collection. Content [" & ContentName & "], field [" & field.nameLc & "].")
                End If
                If True Then
                    'FieldAdminOnly = field.adminOnly
                    FieldDeveloperOnly = field.developerOnly
                    FieldActive = field.active
                    FieldCaption = field.caption
                    FieldReadOnly = field.ReadOnly
                    fieldTypeId = field.fieldTypeId
                    'FieldSortOrder = field.indexSortOrder
                    FieldAuthorable = field.authorable
                    DefaultValue = EncodeText(field.defaultValue)
                    NotEditable = field.NotEditable
                    LookupContentName = field.lookupContentName
                    'field.indexColumn = field.indexColumn
                    AdminIndexWidth = field.indexWidth
                    AdminIndexSort = field.indexSortOrder
                    RedirectContentName = field.RedirectContentName
                    RedirectIDField = field.RedirectID
                    RedirectPath = field.RedirectPath
                    HTMLContent = field.htmlContent
                    UniqueName = field.UniqueName
                    Password = field.Password
                    FieldRequired = field.Required
                    RSSTitle = field.RSSTitleField
                    RSSDescription = field.RSSDescriptionField
                    MemberSelectGroupID = field.MemberSelectGroupID
                    installedByCollectionGuid = field.installedByCollectionGuid
                    EditTab = field.editTabName
                    Scramble = field.Scramble
                    LookupList = field.lookupList
                    ManyToManyContent = field.ManyToManyContentName
                    ManyToManyRuleContent = field.ManyToManyRuleContentName
                    ManyToManyRulePrimaryField = field.ManyToManyRulePrimaryField
                    ManyToManyRuleSecondaryField = field.ManyToManyRuleSecondaryField
                    '
                    If RedirectContentName <> "" Then
                        RedirectContentID = cpCore.db.db_GetContentID(RedirectContentName)
                        If RedirectContentID <= 0 Then
                            Call cpCore.handleExceptionAndRethrow(New Exception("Could Not create redirect For field [" & field.nameLc & "] For Content Definition [" & ContentName & "] because no Content Definition was found For RedirectContentName [" & RedirectContentName & "]."))
                        End If
                    End If
                    '
                    If LookupContentName <> "" Then
                        LookupContentID = cpCore.db.db_GetContentID(LookupContentName)
                        If LookupContentID <= 0 Then
                            Call cpCore.handleExceptionAndRethrow(New Exception("Could Not create lookup For field [" & field.nameLc & "] For Content Definition [" & ContentName & "] because no Content Definition was found For [" & LookupContentName & "]."))
                        End If
                    End If
                    '
                    If ManyToManyContent <> "" Then
                        ManyToManyContentID = cpCore.db.db_GetContentID(ManyToManyContent)
                        If ManyToManyContentID <= 0 Then
                            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Could Not create many To many For field [" & field.nameLc & "] For Content Definition [" & ContentName & "] because no Content Definition was found For ManyToManyContent [" & ManyToManyContent & "]."))
                        End If
                    End If
                    '
                    If ManyToManyRuleContent <> "" Then
                        ManyToManyRuleContentID = cpCore.db.db_GetContentID(ManyToManyRuleContent)
                        If ManyToManyRuleContentID <= 0 Then
                            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Could Not create many To many For field [" & field.nameLc & "] For Content Definition [" & ContentName & "] because no Content Definition was found For ManyToManyRuleContent [" & ManyToManyRuleContent & "]."))
                        End If
                    End If
                    '
                    ' ----- Check error conditions before starting
                    '
                    If ContentID = -1 Then
                        '
                        ' Content Definition not found
                        '
                        Call cpCore.handleExceptionAndRethrow(New ApplicationException("Could Not create Field [" & field.nameLc & "] because Content Definition [" & ContentName & "] was Not found In ccContent Table."))
                    ElseIf TableID <= 0 Then
                        '
                        ' Content Definition not found
                        '
                        Call cpCore.handleExceptionAndRethrow(New ApplicationException("Could Not create Field [" & field.nameLc & "] because Content Definition [" & ContentName & "] has no associated Content Table."))
                    ElseIf fieldTypeId <= 0 Then
                        '
                        ' invalid field type
                        '
                        Call cpCore.handleExceptionAndRethrow(New ApplicationException("Could Not create Field [" & field.nameLc & "] because the field type [" & fieldTypeId & "] Is Not valid."))
                    Else
                        '
                        ' Get the TableName and DataSourceID
                        '
                        TableName = ""
                        rs = cpCore.db.executeSql("Select Name, DataSourceID from ccTables where ID=" & cpCore.db.encodeSQLNumber(TableID) & ";")
                        If Not isDataTableOk(rs) Then
                            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Could Not create Field [" & field.nameLc & "] because table For tableID [" & TableID & "] was Not found."))
                        Else
                            DataSourceID = EncodeInteger(cpCore.db.db_getDataRowColumnName(rs.Rows(0), "DataSourceID"))
                            TableName = EncodeText(cpCore.db.db_getDataRowColumnName(rs.Rows(0), "Name"))
                        End If
                        rs.Dispose()
                        If (TableName <> "") Then
                            '
                            ' Get the DataSourceName
                            '
                            If (DataSourceID < 1) Then
                                DataSourceName = "Default"
                            Else
                                rs = cpCore.db.executeSql("Select Name from ccDataSources where ID=" & cpCore.db.encodeSQLNumber(DataSourceID) & ";")
                                If Not isDataTableOk(rs) Then

                                    DataSourceName = "Default"
                                    ' change condition to successful -- the goal is 1) deliver pages 2) report problems
                                    ' this problem, if translated to default, is really no longer a problem, unless the
                                    ' resulting datasource does not have this data, then other errors will be generated anyway.
                                    'Call csv_HandleClassInternalError(MethodName, "Could Not create Field [" & field.name & "] because datasource For ID [" & DataSourceID & "] was Not found.")
                                Else
                                    DataSourceName = EncodeText(cpCore.db.db_getDataRowColumnName(rs.Rows(0), "Name"))
                                End If
                                rs.Dispose()
                            End If
                            '
                            ' Get the installedByCollectionId
                            '
                            InstalledByCollectionID = 0
                            If (installedByCollectionGuid <> "") Then
                                rs = cpCore.db.executeSql("Select id from ccAddonCollections where ccguid=" & cpCore.db.encodeSQLText(installedByCollectionGuid) & ";")
                                If isDataTableOk(rs) Then
                                    InstalledByCollectionID = EncodeInteger(cpCore.db.db_getDataRowColumnName(rs.Rows(0), "Id"))
                                End If
                                rs.Dispose()
                            End If
                            '
                            ' Create or update the Table Field
                            '
                            If (fieldTypeId = FieldTypeIdRedirect) Then
                                '
                                ' Redirect Field
                                '
                            ElseIf (fieldTypeId = FieldTypeIdManyToMany) Then
                                '
                                ' ManyToMany Field
                                '
                            Else
                                '
                                ' All other fields
                                '
                                Call cpCore.db.db_CreateSQLTableField(DataSourceName, TableName, field.nameLc, fieldTypeId)
                            End If
                            '
                            ' create or update the field
                            '
                            Dim sqlList As New sqlFieldListClass
                            Pointer = 0
                            Call sqlList.add("ACTIVE", cpCore.db.encodeSQLBoolean(field.active)) ' Pointer)
                            Call sqlList.add("MODIFIEDBY", cpCore.db.encodeSQLNumber(SystemMemberID)) ' Pointer)
                            Call sqlList.add("MODIFIEDDATE", cpCore.db.encodeSQLDate(Now)) ' Pointer)
                            Call sqlList.add("TYPE", cpCore.db.encodeSQLNumber(fieldTypeId)) ' Pointer)
                            Call sqlList.add("CAPTION", cpCore.db.encodeSQLText(FieldCaption)) ' Pointer)
                            Call sqlList.add("ReadOnly", cpCore.db.encodeSQLBoolean(FieldReadOnly)) ' Pointer)
                            Call sqlList.add("LOOKUPCONTENTID", cpCore.db.encodeSQLNumber(LookupContentID)) ' Pointer)
                            Call sqlList.add("REQUIRED", cpCore.db.encodeSQLBoolean(FieldRequired)) ' Pointer)
                            Call sqlList.add("TEXTBUFFERED", SQLFalse) ' Pointer)
                            Call sqlList.add("PASSWORD", cpCore.db.encodeSQLBoolean(Password)) ' Pointer)
                            Call sqlList.add("EDITSORTPRIORITY", cpCore.db.encodeSQLNumber(field.editSortPriority)) ' Pointer)
                            Call sqlList.add("ADMINONLY", cpCore.db.encodeSQLBoolean(field.adminOnly)) ' Pointer)
                            Call sqlList.add("DEVELOPERONLY", cpCore.db.encodeSQLBoolean(FieldDeveloperOnly)) ' Pointer)
                            Call sqlList.add("CONTENTCONTROLID", cpCore.db.encodeSQLNumber(cpCore.db.db_GetContentID("Content Fields"))) ' Pointer)
                            Call sqlList.add("DefaultValue", cpCore.db.encodeSQLText(DefaultValue)) ' Pointer)
                            Call sqlList.add("HTMLCONTENT", cpCore.db.encodeSQLBoolean(HTMLContent)) ' Pointer)
                            Call sqlList.add("NOTEDITABLE", cpCore.db.encodeSQLBoolean(NotEditable)) ' Pointer)
                            Call sqlList.add("AUTHORABLE", cpCore.db.encodeSQLBoolean(FieldAuthorable)) ' Pointer)
                            Call sqlList.add("EDITARCHIVE", SQLFalse) ' Pointer)
                            Call sqlList.add("EDITBLANK", SQLFalse) ' Pointer)
                            Call sqlList.add("INDEXCOLUMN", cpCore.db.encodeSQLNumber(field.indexColumn)) ' Pointer)
                            Call sqlList.add("INDEXWIDTH", cpCore.db.encodeSQLText(AdminIndexWidth)) ' Pointer)
                            Call sqlList.add("INDEXSORTPRIORITY", cpCore.db.encodeSQLNumber(AdminIndexSort)) ' Pointer)
                            Call sqlList.add("REDIRECTCONTENTID", cpCore.db.encodeSQLNumber(RedirectContentID)) ' Pointer)
                            Call sqlList.add("REDIRECTID", cpCore.db.encodeSQLText(RedirectIDField)) ' Pointer)
                            Call sqlList.add("REDIRECTPATH", cpCore.db.encodeSQLText(RedirectPath)) ' Pointer)
                            Call sqlList.add("UNIQUENAME", cpCore.db.encodeSQLBoolean(UniqueName)) ' Pointer)
                            Call sqlList.add("RSSTITLEFIELD", cpCore.db.encodeSQLBoolean(RSSTitle)) ' Pointer)
                            Call sqlList.add("RSSDESCRIPTIONFIELD", cpCore.db.encodeSQLBoolean(RSSDescription)) ' Pointer)
                            Call sqlList.add("MEMBERSELECTGROUPID", cpCore.db.encodeSQLNumber(MemberSelectGroupID)) ' Pointer)
                            Call sqlList.add("installedByCollectionId", cpCore.db.encodeSQLNumber(InstalledByCollectionID)) ' Pointer)
                            Call sqlList.add("EDITTAB", cpCore.db.encodeSQLText(EditTab)) ' Pointer)
                            Call sqlList.add("SCRAMBLE", cpCore.db.encodeSQLBoolean(Scramble)) ' Pointer)
                            Call sqlList.add("LOOKUPLIST", cpCore.db.encodeSQLText(LookupList)) ' Pointer)
                            Call sqlList.add("MANYTOMANYCONTENTID", cpCore.db.encodeSQLNumber(ManyToManyContentID)) ' Pointer)
                            Call sqlList.add("MANYTOMANYRULECONTENTID", cpCore.db.encodeSQLNumber(ManyToManyRuleContentID)) ' Pointer)
                            Call sqlList.add("MANYTOMANYRULEPRIMARYFIELD", cpCore.db.encodeSQLText(ManyToManyRulePrimaryField)) ' Pointer)
                            Call sqlList.add("MANYTOMANYRULESECONDARYFIELD", cpCore.db.encodeSQLText(ManyToManyRuleSecondaryField)) ' Pointer)
                            Call sqlList.add("ISBASEFIELD", cpCore.db.encodeSQLBoolean(IsBaseField)) ' Pointer)
                            '
                            If RecordID = 0 Then
                                Call sqlList.add("NAME", cpCore.db.encodeSQLText(field.nameLc)) ' Pointer)
                                Call sqlList.add("CONTENTID", cpCore.db.encodeSQLNumber(ContentID)) ' Pointer)
                                Call sqlList.add("CREATEKEY", "0") ' Pointer)
                                Call sqlList.add("DATEADDED", cpCore.db.encodeSQLDate(Now)) ' Pointer)
                                Call sqlList.add("CREATEDBY", cpCore.db.encodeSQLNumber(SystemMemberID)) ' Pointer)
                                '
                                RecordID = cpCore.db.db_InsertTableRecordGetID("Default", "ccFields")
                            End If
                            If RecordID = 0 Then
                                Call cpCore.handleExceptionAndRethrow(New ApplicationException("Could Not create Field [" & field.nameLc & "] because insert into ccfields failed."))
                            Else
                                Call cpCore.db.db_UpdateTableRecord("Default", "ccFields", "ID=" & RecordID, sqlList)
                            End If
                            '
                        End If
                    End If
                End If
                '
                If Not isNewFieldRecord Then
                    cpCore.cache.invalidateAll()
                    cpCore.metaData.clear()
                End If
                '
                returnId = RecordID
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnId
        End Function


#Region " IDisposable Support "
        Protected disposed As Boolean = False
        '
        '==========================================================================================
        ''' <summary>
        ''' dispose
        ''' </summary>
        ''' <param name="disposing"></param>
        ''' <remarks></remarks>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    '
                    ' ----- call .dispose for managed objects
                    '
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region
    End Class
End Namespace