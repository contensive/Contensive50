

Option Explicit On
Option Strict On

Imports Contensive.Core.Models
Imports Contensive.Core.Models.Context
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

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
        Private Const cacheNameInvalidateAll As String = "cdefInvalidateAll"
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
        '
        '====================================================================================================
        '
        Public Shared Function create(cpcore As coreClass, contentId As Integer, Optional loadInvalidFields As Boolean = False, Optional forceDbLoad As Boolean = False) As cdefModel
            Dim result As cdefModel = Nothing
            Try
                Dim dependantCacheNameList As New List(Of String)
                If (Not forceDbLoad) Then
                    result = getCache(cpcore, contentId)
                End If
                If result Is Nothing Then
                    '
                    ' load Db version
                    '
                    Dim sql As String = "SELECT " _
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
                        & ", '' AS AuthoringTableName" _
                        & ", '' AS AuthoringDataSourceName" _
                        & ", 0 AS AllowWorkflowAuthoring" _
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
                    Dim dt As DataTable = cpcore.db.executeQuery(sql)
                    If dt.Rows.Count = 0 Then
                        '
                        ' cdef not found
                        '
                    Else
                        result = New Models.Complex.cdefModel
                        With result
                            .fields = New Dictionary(Of String, Models.Complex.CDefFieldModel)
                            .childIdList(cpcore) = New List(Of Integer)
                            .selectList = New List(Of String)
                            ' -- !!!!! changed to string because dotnet json cannot serialize an integer key
                            .adminColumns = New SortedList(Of String, Models.Complex.cdefModel.CDefAdminColumnClass)
                            '
                            ' ----- save values in definition
                            '
                            Dim contentName As String
                            Dim row As DataRow = dt.Rows(0)
                            contentName = Trim(genericController.encodeText(row.Item(1)))
                            Dim contentTablename As String = genericController.encodeText(row.Item(10))
                            .Name = contentName
                            .Id = contentId
                            .AllowAdd = genericController.EncodeBoolean(row.Item(3))
                            .DeveloperOnly = genericController.EncodeBoolean(row.Item(4))
                            .AdminOnly = genericController.EncodeBoolean(row.Item(5))
                            .AllowDelete = genericController.EncodeBoolean(row.Item(6))
                            .parentID = genericController.EncodeInteger(row.Item(7))
                            .DropDownFieldList = genericController.vbUCase(genericController.encodeText(row.Item(9)))
                            .ContentTableName = genericController.encodeText(contentTablename)
                            .ContentDataSourceName = "default"
                            .AllowCalendarEvents = genericController.EncodeBoolean(row.Item(15))
                            .DefaultSortMethod = genericController.encodeText(row.Item(17))
                            If .DefaultSortMethod = "" Then
                                .DefaultSortMethod = "name"
                            End If
                            .EditorGroupName = genericController.encodeText(row.Item(18))
                            .AllowContentTracking = genericController.EncodeBoolean(row.Item(19))
                            .AllowTopicRules = genericController.EncodeBoolean(row.Item(20))
                            ' .AllowMetaContent = genericController.EncodeBoolean(row.Item(21))
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
                                Dim parentCdef As Models.Complex.cdefModel = create(cpcore, .parentID, loadInvalidFields, forceDbLoad)
                                For Each keyvaluepair In parentCdef.fields
                                    Dim parentField As Models.Complex.CDefFieldModel = keyvaluepair.Value
                                    Dim childField As New Models.Complex.CDefFieldModel
                                    childField = DirectCast(parentField.Clone, Models.Complex.CDefFieldModel)
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
                            dt = cpcore.db.executeQuery(sql)
                            If dt.Rows.Count = 0 Then
                                '
                            Else
                                Dim usedFields As New List(Of String)
                                For Each row In dt.Rows
                                    Dim fieldName As String = genericController.encodeText(row.Item(13))
                                    Dim fieldId As Integer = genericController.EncodeInteger(row.Item(12))
                                    Dim fieldNameLower As String = fieldName.ToLower()
                                    Dim skipDuplicateField As Boolean = False
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
                                        Dim field As Models.Complex.CDefFieldModel = New Models.Complex.CDefFieldModel
                                        With field
                                            Dim fieldIndexColumn As Integer = -1
                                            Dim fieldTypeId As Integer = genericController.EncodeInteger(row.Item(15))
                                            If (genericController.encodeText(row.Item(4)) <> "") Then
                                                fieldIndexColumn = genericController.EncodeInteger(row.Item(4))
                                            End If
                                            '
                                            ' translate htmlContent to fieldtypehtml
                                            '   this is also converted in upgrade, daily housekeep, addon install
                                            '
                                            Dim fieldHtmlContent As Boolean = genericController.EncodeBoolean(row.Item(25))
                                            If fieldHtmlContent Then
                                                If fieldTypeId = FieldTypeIdLongText Then
                                                    fieldTypeId = FieldTypeIdHTML
                                                ElseIf fieldTypeId = FieldTypeIdFileText Then
                                                    fieldTypeId = FieldTypeIdFileHTML
                                                End If
                                            End If
                                            .active = genericController.EncodeBoolean(row.Item(24))
                                            .adminOnly = genericController.EncodeBoolean(row.Item(8))
                                            .authorable = genericController.EncodeBoolean(row.Item(27))
                                            .blockAccess = genericController.EncodeBoolean(row.Item(38))
                                            .caption = genericController.encodeText(row.Item(16))
                                            .dataChanged = False
                                            '.Changed
                                            .contentId = contentId
                                            .defaultValue = genericController.encodeText(row.Item(22))
                                            .developerOnly = genericController.EncodeBoolean(row.Item(0))
                                            .editSortPriority = genericController.EncodeInteger(row.Item(10))
                                            .editTabName = genericController.encodeText(row.Item(34))
                                            .fieldTypeId = fieldTypeId
                                            .htmlContent = fieldHtmlContent
                                            .id = fieldId
                                            .indexColumn = fieldIndexColumn
                                            .indexSortDirection = genericController.EncodeInteger(row.Item(7))
                                            .indexSortOrder = genericController.EncodeInteger(row.Item(6))
                                            .indexWidth = genericController.encodeText(genericController.EncodeInteger(Replace(genericController.encodeText(row.Item(5)), "%", "")))
                                            .inherited = False
                                            .installedByCollectionGuid = genericController.encodeText(row.Item(39))
                                            .isBaseField = genericController.EncodeBoolean(row.Item(38))
                                            .isModifiedSinceInstalled = False
                                            .lookupContentID = genericController.EncodeInteger(row.Item(18))
                                            '.lookupContentName = ""
                                            .lookupList = genericController.encodeText(row.Item(37))
                                            .manyToManyContentID = genericController.EncodeInteger(row.Item(28))
                                            .manyToManyRuleContentID = genericController.EncodeInteger(row.Item(29))
                                            .ManyToManyRulePrimaryField = genericController.encodeText(row.Item(30))
                                            .ManyToManyRuleSecondaryField = genericController.encodeText(row.Item(31))
                                            '.ManyToManyContentName(cpCore) = ""
                                            '.ManyToManyRuleContentName(cpCore) = ""
                                            .MemberSelectGroupID = genericController.EncodeInteger(row.Item(36))
                                            '.MemberSelectGroupName(cpCore) = ""
                                            .nameLc = fieldNameLower
                                            .NotEditable = genericController.EncodeBoolean(row.Item(26))
                                            .Password = genericController.EncodeBoolean(row.Item(3))
                                            .ReadOnly = genericController.EncodeBoolean(row.Item(17))
                                            .RedirectContentID = genericController.EncodeInteger(row.Item(19))
                                            '.RedirectContentName(cpCore) = ""
                                            .RedirectID = genericController.encodeText(row.Item(21))
                                            .RedirectPath = genericController.encodeText(row.Item(20))
                                            .Required = genericController.EncodeBoolean(row.Item(14))
                                            .RSSTitleField = genericController.EncodeBoolean(row.Item(32))
                                            .RSSDescriptionField = genericController.EncodeBoolean(row.Item(33))
                                            .Scramble = genericController.EncodeBoolean(row.Item(35))
                                            .TextBuffered = genericController.EncodeBoolean(row.Item(2))
                                            .UniqueName = genericController.EncodeBoolean(row.Item(1))
                                            '.ValueVariant
                                            '
                                            .HelpCustom = genericController.encodeText(row.Item(41))
                                            .HelpDefault = genericController.encodeText(row.Item(40))
                                            If .HelpCustom = "" Then
                                                .HelpMessage = .HelpDefault
                                            Else
                                                .HelpMessage = .HelpCustom
                                            End If
                                            .HelpChanged = False
                                            dt.Dispose()
                                        End With
                                        .fields.Add(fieldNameLower, field)
                                        'REFACTOR
                                        If ((field.fieldTypeId <> FieldTypeIdManyToMany) And (field.fieldTypeId <> FieldTypeIdRedirect) And (Not .selectList.Contains(fieldNameLower))) Then
                                            '
                                            ' add only fields that can be selected
                                            .selectList.Add(fieldNameLower)
                                        End If
                                    End If
                                Next
                                .SelectCommaList = String.Join(",", .selectList)
                            End If
                            '
                            ' ----- Create the ContentControlCriteria
                            '
                            .ContentControlCriteria = Models.Complex.cdefModel.getContentControlCriteria(cpcore, .Id, .ContentTableName, .ContentDataSourceName, New List(Of Integer))
                            '
                        End With
                        getCdef_SetAdminColumns(cpcore, result)
                    End If
                    setCache(cpcore, contentId, result)
                End If
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        '
        Private Shared Sub getCdef_SetAdminColumns(cpcore As coreClass, cdef As Models.Complex.cdefModel)
            Try
                Dim FieldActive As Boolean
                Dim FieldWidth As Integer
                Dim FieldWidthTotal As Integer
                Dim adminColumn As Models.Complex.cdefModel.CDefAdminColumnClass
                '
                With cdef
                    If .Id > 0 Then
                        Dim cnt As Integer = 0
                        For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In cdef.fields
                            Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                            FieldActive = field.active
                            FieldWidth = genericController.EncodeInteger(field.indexWidth)
                            If FieldActive And (FieldWidth > 0) Then
                                FieldWidthTotal = FieldWidthTotal + FieldWidth
                                adminColumn = New Models.Complex.cdefModel.CDefAdminColumnClass
                                With adminColumn
                                    .Name = field.nameLc
                                    .SortDirection = field.indexSortDirection
                                    .SortPriority = genericController.EncodeInteger(field.indexSortOrder)
                                    .Width = FieldWidth
                                    FieldWidthTotal = FieldWidthTotal + .Width
                                End With
                                Dim key As String = (cnt + (adminColumn.SortPriority * 1000)).ToString().PadLeft(6, "0"c)
                                .adminColumns.Add(key, adminColumn)
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
                                    adminColumn = New Models.Complex.cdefModel.CDefAdminColumnClass
                                    With adminColumn
                                        .Name = "Name"
                                        .SortDirection = 1
                                        .SortPriority = 1
                                        .Width = 100
                                        FieldWidthTotal = FieldWidthTotal + .Width
                                    End With
                                    Dim key As String = ((1000)).ToString().PadLeft(6, "0"c)
                                    .adminColumns.Add(key, adminColumn)
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
                cpcore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' get content id from content name
        ''' </summary>
        ''' <param name="contentName"></param>
        ''' <returns></returns>
        Public Shared Function getContentId(cpcore As coreClass, contentName As String) As Integer
            Dim returnId As Integer = 0
            Try
                If (cpcore.doc.contentNameIdDictionary.ContainsKey(contentName.ToLower)) Then
                    returnId = cpcore.doc.contentNameIdDictionary(contentName.ToLower)
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
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
        Public Shared Function getCdef(cpcore As coreClass, contentName As String) As Models.Complex.cdefModel
            Dim returnCdef As Models.Complex.cdefModel = Nothing
            Try
                Dim ContentId As Integer = getContentId(cpcore, contentName)
                If (ContentId > 0) Then
                    returnCdef = getCdef(cpcore, ContentId)
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return returnCdef
        End Function
        '        
        '====================================================================================================
        ''' <summary>
        ''' return a cdef class from content id. Returns nothing if contentId is not valid
        ''' </summary>
        ''' <param name="contentId"></param>
        ''' <returns></returns>
        Public Shared Function getCdef(cpcore As coreClass, contentId As Integer, Optional forceDbLoad As Boolean = False, Optional loadInvalidFields As Boolean = False) As Models.Complex.cdefModel
            Dim returnCdef As Models.Complex.cdefModel = Nothing
            Try
                If (contentId <= 0) Then
                    '
                    ' -- invalid id                    
                ElseIf (Not forceDbLoad) And (cpcore.doc.cdefDictionary.ContainsKey(contentId.ToString)) Then
                    '
                    ' -- already loaded and no force re-load, just return the current cdef                    
                    returnCdef = cpcore.doc.cdefDictionary.Item(contentId.ToString)
                Else
                    If (cpcore.doc.cdefDictionary.ContainsKey(contentId.ToString)) Then
                        '
                        ' -- key is already there, remove it first                        
                        cpcore.doc.cdefDictionary.Remove(contentId.ToString)
                    End If
                    returnCdef = Models.Complex.cdefModel.create(cpcore, contentId, loadInvalidFields, forceDbLoad)
                    cpcore.doc.cdefDictionary.Add(contentId.ToString, returnCdef)
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return returnCdef
        End Function
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
        Friend Shared Function getContentControlCriteria(cpcore As coreClass, ByVal contentId As Integer, contentTableName As String, contentDAtaSourceName As String, ByVal parentIdList As List(Of Integer)) As String
            Dim returnCriteria As String = ""
            Try
                '
                returnCriteria = "(1=0)"
                If (contentId >= 0) Then
                    If Not parentIdList.Contains(contentId) Then
                        parentIdList.Add(contentId)
                        returnCriteria = "(" & contentTableName & ".contentcontrolId=" & contentId & ")"
                        For Each kvp As KeyValuePair(Of Integer, contentModel) In cpcore.doc.contentIdDict
                            If (kvp.Value.ParentID = contentId) Then
                                returnCriteria &= "OR" & getContentControlCriteria(cpcore, kvp.Value.id, contentTableName, contentDAtaSourceName, parentIdList)
                            End If
                        Next
                        parentIdList.Remove(contentId)
                        returnCriteria = "(" & returnCriteria & ")"
                    End If
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return returnCriteria
        End Function
        '
        '========================================================================
        '   IsWithinContent( ChildContentID, ParentContentID )
        '
        '       Returns true if ChildContentID is in ParentContentID
        '========================================================================
        '
        Public Shared Function isWithinContent(cpcore As coreClass, ByVal ChildContentID As Integer, ByVal ParentContentID As Integer) As Boolean
            Dim returnOK As Boolean = False
            Try
                Dim cdef As Models.Complex.cdefModel
                If (ChildContentID = ParentContentID) Then
                    returnOK = True
                Else
                    cdef = getCdef(cpcore, ParentContentID)
                    If Not (cdef Is Nothing) Then
                        If cdef.childIdList(cpcore).Count > 0 Then
                            returnOK = cdef.childIdList(cpcore).Contains(ChildContentID)
                            If Not returnOK Then
                                For Each contentId As Integer In cdef.childIdList(cpcore)
                                    returnOK = isWithinContent(cpcore, contentId, ParentContentID)
                                    If returnOK Then Exit For
                                Next
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return returnOK
        End Function
        '
        '===========================================================================
        '   main_Get Authoring List
        '       returns a comma delimited list of ContentIDs that the Member can author
        '===========================================================================
        '
        Public Shared Function getEditableCdefIdList(cpcore As coreClass) As List(Of Integer)
            Dim returnList As New List(Of Integer)
            Try
                Dim SQL As String
                Dim cidDataTable As DataTable
                Dim CIDCount As Integer
                Dim CIDPointer As Integer
                Dim CDef As Models.Complex.cdefModel
                Dim ContentID As Integer
                '
                SQL = "Select ccGroupRules.ContentID as ID" _
                & " FROM ((ccmembersrules" _
                & " Left Join ccGroupRules on ccMemberRules.GroupID=ccGroupRules.GroupID)" _
                & " Left Join ccContent on ccGroupRules.ContentID=ccContent.ID)" _
                & " WHERE" _
                    & " (ccMemberRules.MemberID=" & cpcore.doc.authContext.user.id & ")" _
                    & " AND(ccGroupRules.Active<>0)" _
                    & " AND(ccContent.Active<>0)" _
                    & " AND(ccMemberRules.Active<>0)"
                cidDataTable = cpcore.db.executeQuery(SQL)
                CIDCount = cidDataTable.Rows.Count
                For CIDPointer = 0 To CIDCount - 1
                    ContentID = genericController.EncodeInteger(cidDataTable.Rows(CIDPointer).Item(0))
                    returnList.Add(ContentID)
                    CDef = getCdef(cpcore, ContentID)
                    If Not (CDef Is Nothing) Then
                        returnList.AddRange(CDef.childIdList(cpcore))
                    End If
                Next
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return returnList
        End Function
        '
        '=============================================================================
        ' Create a child content from a parent content
        '
        '   If child does not exist, copy everything from the parent
        '   If child already exists, add any missing fields from parent
        '=============================================================================
        '
        Public Shared Sub createContentChild(cpcore As coreClass, ByVal ChildContentName As String, ByVal ParentContentName As String, ByVal MemberID As Integer)
            Try
                Dim DataSourceName As String = ""
                Dim MethodName As String
                Dim SQL As String
                Dim rs As DataTable
                Dim ChildContentID As Integer
                Dim ParentContentID As Integer
                Dim CSContent As Integer
                Dim CSNew As Integer
                Dim SelectFieldList As String
                Dim Fields() As String
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
                SQL = "select ID from ccContent where name=" & cpcore.db.encodeSQLText(ChildContentName) & ";"
                rs = cpcore.db.executeQuery(SQL)
                If isDataTableOk(rs) Then
                    ChildContentID = genericController.EncodeInteger(cpcore.db.getDataRowColumnName(rs.Rows(0), "ID"))
                    '
                    ' mark the record touched so upgrade will not delete it
                    '
                    Call cpcore.db.executeQuery("update ccContent set CreateKey=0 where ID=" & ChildContentID)
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
                    SQL = "select ID from ccContent where name=" & cpcore.db.encodeSQLText(ParentContentName) & ";"
                    rs = cpcore.db.executeQuery(SQL, DataSourceName)
                    If isDataTableOk(rs) Then
                        ParentContentID = genericController.EncodeInteger(cpcore.db.getDataRowColumnName(rs.Rows(0), "ID"))
                        '
                        ' mark the record touched so upgrade will not delete it
                        '
                        Call cpcore.db.executeQuery("update ccContent set CreateKey=0 where ID=" & ParentContentID)
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
                        Throw (New ApplicationException("Can not create Child Content [" & ChildContentName & "] because the Parent Content [" & ParentContentName & "] was not found."))
                    Else
                        '
                        ' ----- create child content record, let the csv_ExecuteSQL reload CDef
                        '
                        DataSourceName = "Default"
                        CSContent = cpcore.db.cs_openContentRecord("Content", ParentContentID)
                        If Not cpcore.db.csOk(CSContent) Then
                            Throw (New ApplicationException("Can not create Child Content [" & ChildContentName & "] because the Parent Content [" & ParentContentName & "] was not found."))
                        Else
                            SelectFieldList = cpcore.db.cs_getSelectFieldList(CSContent)
                            If SelectFieldList = "" Then
                                Throw (New ApplicationException("Can not create Child Content [" & ChildContentName & "] because the Parent Content [" & ParentContentName & "] record has not fields."))
                            Else
                                CSNew = cpcore.db.csInsertRecord("Content", 0)
                                If Not cpcore.db.csOk(CSNew) Then
                                    Throw (New ApplicationException("Can not create Child Content [" & ChildContentName & "] because there was an error creating a new record in ccContent."))
                                Else
                                    Fields = Split(SelectFieldList, ",")
                                    DateNow = DateTime.Now()
                                    For FieldPointer = 0 To UBound(Fields)
                                        FieldName = Fields(FieldPointer)
                                        Select Case genericController.vbUCase(FieldName)
                                            Case "ID"
                                            ' do nothing
                                            Case "NAME"
                                                Call cpcore.db.csSet(CSNew, FieldName, ChildContentName)
                                            Case "PARENTID"
                                                Call cpcore.db.csSet(CSNew, FieldName, cpcore.db.csGetText(CSContent, "ID"))
                                            Case "CREATEDBY", "MODIFIEDBY"
                                                Call cpcore.db.csSet(CSNew, FieldName, MemberID)
                                            Case "DATEADDED", "MODIFIEDDATE"
                                                Call cpcore.db.csSet(CSNew, FieldName, DateNow)
                                            Case "CCGUID"

                                                '
                                                ' new, non-blank guid so if this cdef is exported, it will be updateable
                                                '
                                                Call cpcore.db.csSet(CSNew, FieldName, createGuid())
                                            Case Else
                                                Call cpcore.db.csSet(CSNew, FieldName, cpcore.db.csGetText(CSContent, FieldName))
                                        End Select
                                    Next
                                End If
                                Call cpcore.db.csClose(CSNew)
                            End If
                        End If
                        Call cpcore.db.csClose(CSContent)
                    End If
                End If
                '
                ' ----- Load CDef
                '
                cpcore.cache.invalidateAll()
                cpcore.doc.clearMetaData()
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
        End Sub
        '
        '========================================================================
        ' Get a Contents Tablename from the ContentPointer
        '========================================================================
        '
        Public Shared Function getContentTablename(cpcore As coreClass, ByVal ContentName As String) As String
            Dim returnTableName As String = ""
            Try
                Dim CDef As Models.Complex.cdefModel
                '
                CDef = Models.Complex.cdefModel.getCdef(cpcore, ContentName)
                If (CDef IsNot Nothing) Then
                    returnTableName = CDef.ContentTableName
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return returnTableName
        End Function
        '
        '========================================================================
        ' ----- Get a DataSource Name from its ContentName
        '
        Public Shared Function getContentDataSource(cpcore As coreClass, ContentName As String) As String
            Dim returnDataSource As String = ""
            Try
                Dim CDef As Models.Complex.cdefModel
                '
                CDef = Models.Complex.cdefModel.getCdef(cpcore, ContentName)
                If (CDef Is Nothing) Then
                    '
                Else
                    returnDataSource = CDef.ContentDataSourceName
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return returnDataSource
        End Function
        '
        '========================================================================
        ' Get a Contents Name from the ContentID
        '   Bad ContentID returns blank
        '========================================================================
        '
        Public Shared Function getContentNameByID(cpcore As coreClass, ByVal ContentID As Integer) As String
            Dim returnName As String = ""
            Try
                Dim cdef As Models.Complex.cdefModel
                '
                cdef = Models.Complex.cdefModel.getCdef(cpcore, ContentID)
                If cdef IsNot Nothing Then
                    returnName = cdef.Name
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return returnName
        End Function

        '========================================================================
        '   Create a content definition
        '       called from upgrade and DeveloperTools
        '========================================================================
        '
        Public Shared Function addContent(cpcore As coreClass, ByVal Active As Boolean, datasource As dataSourceModel, ByVal TableName As String, ByVal contentName As String, Optional ByVal AdminOnly As Boolean = False, Optional ByVal DeveloperOnly As Boolean = False, Optional ByVal AllowAdd As Boolean = True, Optional ByVal AllowDelete As Boolean = True, Optional ByVal ParentName As String = "", Optional ByVal DefaultSortMethod As String = "", Optional ByVal DropDownFieldList As String = "", Optional ByVal AllowWorkflowAuthoring As Boolean = False, Optional ByVal AllowCalendarEvents As Boolean = False, Optional ByVal AllowContentTracking As Boolean = False, Optional ByVal AllowTopicRules As Boolean = False, Optional ByVal AllowContentChildTool As Boolean = False, Optional ByVal ignore1 As Boolean = False, Optional ByVal IconLink As String = "", Optional ByVal IconWidth As Integer = 0, Optional ByVal IconHeight As Integer = 0, Optional ByVal IconSprites As Integer = 0, Optional ByVal ccGuid As String = "", Optional ByVal IsBaseContent As Boolean = False, Optional ByVal installedByCollectionGuid As String = "", Optional clearMetaCache As Boolean = False) As Integer
            Dim returnContentId As Integer = 0
            Try
                '
                Dim ContentIsBaseContent As Boolean
                Dim NewGuid As String
                Dim LcContentGuid As String
                Dim SQL As String
                Dim parentId As Integer
                Dim dt As DataTable
                Dim TableID As Integer
                Dim iDefaultSortMethod As String
                Dim DefaultSortMethodID As Integer
                Dim CDefFound As Boolean
                Dim InstalledByCollectionID As Integer
                Dim sqlList As sqlFieldListClass
                Dim field As Models.Complex.CDefFieldModel
                Dim ContentIDofContent As Integer
                '
                If String.IsNullOrEmpty(contentName) Then
                    Throw New ApplicationException("contentName can not be blank")
                Else
                    '
                    If String.IsNullOrEmpty(TableName) Then
                        Throw New ApplicationException("Tablename can not be blank")
                    Else
                        '
                        ' Create the SQL table
                        '
                        Call cpcore.db.createSQLTable(datasource.Name, TableName)
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
                        SQL = "select ID,ccguid,IsBaseContent from ccContent where (name=" & cpcore.db.encodeSQLText(contentName) & ") order by id;"
                        dt = cpcore.db.executeQuery(SQL)
                        If dt.Rows.Count > 0 Then
                            returnContentId = genericController.EncodeInteger(dt.Rows(0).Item("ID"))
                            LcContentGuid = genericController.vbLCase(genericController.encodeText(dt.Rows(0).Item("ccguid")))
                            ContentIsBaseContent = genericController.EncodeBoolean(dt.Rows(0).Item("IsBaseContent"))
                        End If
                        dt.Dispose()
                        '
                        ' get contentid of content
                        '
                        ContentIDofContent = 0
                        If contentName.ToLower() = "content" Then
                            ContentIDofContent = returnContentId
                        Else
                            SQL = "select ID from ccContent where (name='content') order by id;"
                            dt = cpcore.db.executeQuery(SQL)
                            If dt.Rows.Count > 0 Then
                                ContentIDofContent = genericController.EncodeInteger(dt.Rows(0).Item("ID"))
                            End If
                            dt.Dispose()
                        End If
                        '
                        ' get parentId
                        '
                        If Not String.IsNullOrEmpty(ParentName) Then
                            SQL = "select id from ccContent where (name=" & cpcore.db.encodeSQLText(ParentName) & ") order by id;"
                            dt = cpcore.db.executeQuery(SQL)
                            If dt.Rows.Count > 0 Then
                                parentId = genericController.EncodeInteger(dt.Rows(0).Item(0))
                            End If
                            dt.Dispose()
                        End If
                        '
                        ' get InstalledByCollectionID
                        '
                        InstalledByCollectionID = 0
                        If (installedByCollectionGuid <> "") Then
                            SQL = "select id from ccAddonCollections where ccGuid=" & cpcore.db.encodeSQLText(installedByCollectionGuid)
                            dt = cpcore.db.executeQuery(SQL)
                            If dt.Rows.Count > 0 Then
                                InstalledByCollectionID = genericController.EncodeInteger(dt.Rows(0).Item("ID"))
                            End If
                        End If
                        '
                        ' Block non-base update of a base field
                        '
                        If ContentIsBaseContent And Not IsBaseContent Then
                            Throw New ApplicationException("Attempt to update a Base Content Definition [" & contentName & "] as non-base. This is not allowed.")
                        Else
                            CDefFound = (returnContentId <> 0)
                            If Not CDefFound Then
                                '
                                ' ----- Create a new empty Content Record (to get ContentID)
                                '
                                returnContentId = cpcore.db.insertTableRecordGetId("Default", "ccContent", SystemMemberID)
                            End If
                            '
                            ' ----- Get the Table Definition ID, create one if missing
                            '
                            SQL = "SELECT ID from ccTables where (active<>0) and (name=" & cpcore.db.encodeSQLText(TableName) & ");"
                            dt = cpcore.db.executeQuery(SQL)
                            If dt.Rows.Count <= 0 Then
                                '
                                ' ----- no table definition found, create one
                                '
                                'If genericController.vbUCase(DataSourceName) = "DEFAULT" Then
                                '    DataSourceID = -1
                                'ElseIf DataSourceName = "" Then
                                '    DataSourceID = -1
                                'Else
                                '    DataSourceID = cpCore.db.getDataSourceId(DataSourceName)
                                '    If DataSourceID = -1 Then
                                '        throw (New ApplicationException("Could not find DataSource [" & DataSourceName & "] for table [" & TableName & "]"))
                                '    End If
                                'End If
                                TableID = cpcore.db.insertTableRecordGetId("Default", "ccTables", SystemMemberID)
                                '
                                sqlList = New sqlFieldListClass
                                sqlList.add("name", cpcore.db.encodeSQLText(TableName))
                                sqlList.add("active", SQLTrue)
                                sqlList.add("DATASOURCEID", cpcore.db.encodeSQLNumber(datasource.ID))
                                sqlList.add("CONTENTCONTROLID", cpcore.db.encodeSQLNumber(Models.Complex.cdefModel.getContentId(cpcore, "Tables")))
                                '
                                Call cpcore.db.updateTableRecord("Default", "ccTables", "ID=" & TableID, sqlList)
                            Else
                                TableID = genericController.EncodeInteger(dt.Rows(0).Item("ID"))
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
                                dt = cpcore.db.openTable("Default", "ccSortMethods", "(name=" & cpcore.db.encodeSQLText(iDefaultSortMethod) & ")and(active<>0)", "ID", "ID", 1, 1)
                                If dt.Rows.Count > 0 Then
                                    DefaultSortMethodID = genericController.EncodeInteger(dt.Rows(0).Item("ID"))
                                End If
                            End If
                            If DefaultSortMethodID = 0 Then
                                '
                                ' fallback - maybe they put the orderbyclause in (common mistake)
                                '
                                dt = cpcore.db.openTable("Default", "ccSortMethods", "(OrderByClause=" & cpcore.db.encodeSQLText(iDefaultSortMethod) & ")and(active<>0)", "ID", "ID", 1, 1)
                                If dt.Rows.Count > 0 Then
                                    DefaultSortMethodID = genericController.EncodeInteger(dt.Rows(0).Item("ID"))
                                End If
                            End If
                            '
                            ' determine parentId from parentName
                            '

                            '
                            ' ----- update record
                            '
                            sqlList = New sqlFieldListClass
                            Call sqlList.add("name", cpcore.db.encodeSQLText(contentName))
                            Call sqlList.add("CREATEKEY", "0")
                            Call sqlList.add("active", cpcore.db.encodeSQLBoolean(Active))
                            Call sqlList.add("ContentControlID", cpcore.db.encodeSQLNumber(ContentIDofContent))
                            Call sqlList.add("AllowAdd", cpcore.db.encodeSQLBoolean(AllowAdd))
                            Call sqlList.add("AllowDelete", cpcore.db.encodeSQLBoolean(AllowDelete))
                            Call sqlList.add("AllowWorkflowAuthoring", cpcore.db.encodeSQLBoolean(AllowWorkflowAuthoring))
                            Call sqlList.add("DeveloperOnly", cpcore.db.encodeSQLBoolean(DeveloperOnly))
                            Call sqlList.add("AdminOnly", cpcore.db.encodeSQLBoolean(AdminOnly))
                            Call sqlList.add("ParentID", cpcore.db.encodeSQLNumber(parentId))
                            Call sqlList.add("DefaultSortMethodID", cpcore.db.encodeSQLNumber(DefaultSortMethodID))
                            Call sqlList.add("DropDownFieldList", cpcore.db.encodeSQLText(encodeEmptyText(DropDownFieldList, "Name")))
                            Call sqlList.add("ContentTableID", cpcore.db.encodeSQLNumber(TableID))
                            Call sqlList.add("AuthoringTableID", cpcore.db.encodeSQLNumber(TableID))
                            Call sqlList.add("ModifiedDate", cpcore.db.encodeSQLDate(Now))
                            Call sqlList.add("CreatedBy", cpcore.db.encodeSQLNumber(SystemMemberID))
                            Call sqlList.add("ModifiedBy", cpcore.db.encodeSQLNumber(SystemMemberID))
                            Call sqlList.add("AllowCalendarEvents", cpcore.db.encodeSQLBoolean(AllowCalendarEvents))
                            Call sqlList.add("AllowContentTracking", cpcore.db.encodeSQLBoolean(AllowContentTracking))
                            Call sqlList.add("AllowTopicRules", cpcore.db.encodeSQLBoolean(AllowTopicRules))
                            Call sqlList.add("AllowContentChildTool", cpcore.db.encodeSQLBoolean(AllowContentChildTool))
                            'Call sqlList.add("AllowMetaContent", cpCore.db.encodeSQLBoolean(ignore1))
                            Call sqlList.add("IconLink", cpcore.db.encodeSQLText(encodeEmptyText(IconLink, "")))
                            Call sqlList.add("IconHeight", cpcore.db.encodeSQLNumber(IconHeight))
                            Call sqlList.add("IconWidth", cpcore.db.encodeSQLNumber(IconWidth))
                            Call sqlList.add("IconSprites", cpcore.db.encodeSQLNumber(IconSprites))
                            Call sqlList.add("installedByCollectionid", cpcore.db.encodeSQLNumber(InstalledByCollectionID))
                            If (LcContentGuid = "") And (NewGuid <> "") Then
                                '
                                ' hard one - only update guid if the tables supports it, and it the new guid is not blank
                                ' if the new guid does no match te old guid
                                '
                                Call sqlList.add("ccGuid", cpcore.db.encodeSQLText(NewGuid))
                            ElseIf (NewGuid <> "") And (LcContentGuid <> genericController.vbLCase(NewGuid)) Then
                                '
                                ' installing content definition with matching name, but different guid -- this is an error that needs to be fixed
                                '
                                cpcore.handleException(New ApplicationException("createContent call, content.name match found but content.ccGuid did not, name [" & contentName & "], newGuid [" & NewGuid & "], installedGuid [" & LcContentGuid & "] "))
                            End If
                            Call cpcore.db.updateTableRecord("Default", "ccContent", "ID=" & returnContentId, sqlList)
                            '
                            '-----------------------------------------------------------------------------------------------
                            ' Verify Core Content Definition Fields
                            '-----------------------------------------------------------------------------------------------
                            '
                            If parentId < 1 Then
                                '
                                ' CDef does not inherit its fields, create what is needed for a non-inherited CDef
                                '
                                If Not cpcore.db.isCdefField(returnContentId, "ID") Then
                                    field = New Models.Complex.CDefFieldModel
                                    field.nameLc = "id"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdAutoIdIncrement
                                    field.editSortPriority = 100
                                    field.authorable = False
                                    field.caption = "ID"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(cpcore, contentName, field)
                                End If
                                '
                                If Not cpcore.db.isCdefField(returnContentId, "name") Then
                                    field = New Models.Complex.CDefFieldModel
                                    field.nameLc = "name"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdText
                                    field.editSortPriority = 110
                                    field.authorable = True
                                    field.caption = "Name"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(cpcore, contentName, field)
                                End If
                                '
                                If Not cpcore.db.isCdefField(returnContentId, "active") Then
                                    field = New Models.Complex.CDefFieldModel
                                    field.nameLc = "active"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdBoolean
                                    field.editSortPriority = 200
                                    field.authorable = True
                                    field.caption = "Active"
                                    field.defaultValue = "1"
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(cpcore, contentName, field)
                                End If
                                '
                                If Not cpcore.db.isCdefField(returnContentId, "sortorder") Then
                                    field = New Models.Complex.CDefFieldModel
                                    field.nameLc = "sortorder"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdText
                                    field.editSortPriority = 2000
                                    field.authorable = False
                                    field.caption = "Alpha Sort Order"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(cpcore, contentName, field)
                                End If
                                '
                                If Not cpcore.db.isCdefField(returnContentId, "dateadded") Then
                                    field = New Models.Complex.CDefFieldModel
                                    field.nameLc = "dateadded"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdDate
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Date Added"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(cpcore, contentName, field)
                                End If
                                If Not cpcore.db.isCdefField(returnContentId, "createdby") Then
                                    field = New Models.Complex.CDefFieldModel
                                    field.nameLc = "createdby"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdLookup
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Created By"
                                    field.lookupContentName(cpcore) = "People"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(cpcore, contentName, field)
                                End If
                                If Not cpcore.db.isCdefField(returnContentId, "modifieddate") Then
                                    field = New Models.Complex.CDefFieldModel
                                    field.nameLc = "modifieddate"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdDate
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Date Modified"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(cpcore, contentName, field)
                                End If
                                If Not cpcore.db.isCdefField(returnContentId, "modifiedby") Then
                                    field = New Models.Complex.CDefFieldModel
                                    field.nameLc = "modifiedby"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdLookup
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Modified By"
                                    field.lookupContentName(cpcore) = "People"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(cpcore, contentName, field)
                                End If
                                If Not cpcore.db.isCdefField(returnContentId, "ContentControlId") Then
                                    field = New Models.Complex.CDefFieldModel
                                    field.nameLc = "contentcontrolid"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdLookup
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Controlling Content"
                                    field.lookupContentName(cpcore) = "Content"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(cpcore, contentName, field)
                                End If
                                If Not cpcore.db.isCdefField(returnContentId, "CreateKey") Then
                                    field = New Models.Complex.CDefFieldModel
                                    field.nameLc = "createkey"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdInteger
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Create Key"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(cpcore, contentName, field)
                                End If
                                If Not cpcore.db.isCdefField(returnContentId, "ccGuid") Then
                                    field = New Models.Complex.CDefFieldModel
                                    field.nameLc = "ccguid"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdText
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Guid"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(cpcore, contentName, field)
                                End If
                                ' -- 20171029 - had to un-deprecate because compatibility issues are too timeconsuming
                                If Not cpcore.db.isCdefField(returnContentId, "ContentCategoryId") Then
                                    field = New Models.Complex.CDefFieldModel
                                    field.nameLc = "contentcategoryid"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdInteger
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Content Category"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(cpcore, contentName, field)
                                End If
                            End If
                            '
                            ' ----- Load CDef
                            '
                            If clearMetaCache Then
                                cpcore.cache.invalidateAllObjectsInContent(contentModel.contentName.ToLower())
                                cpcore.cache.invalidateAllObjectsInContent(contentFieldModel.contentName.ToLower())
                                cpcore.doc.clearMetaData()
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
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
        Public Shared Function verifyCDefField_ReturnID(cpcore As coreClass, ByVal ContentName As String, field As Models.Complex.CDefFieldModel) As Integer ' , ByVal FieldName As String, ByVal Args As String, ByVal Delimiter As String) As Integer
            Dim returnId As Integer = 0
            Try
                '
                Dim RecordIsBaseField As Boolean
                Dim IsBaseField As Boolean
                Dim SQL As String
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
                Dim FieldAuthorable As Boolean
                Dim LookupContentName As String
                Dim DefaultValue As String
                Dim NotEditable As Boolean
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
                Dim RSSTitle As Boolean
                Dim RSSDescription As Boolean
                Dim FieldDeveloperOnly As Boolean
                Dim MemberSelectGroupID As Integer
                Dim installedByCollectionGuid As String
                Dim InstalledByCollectionID As Integer
                Dim EditTab As String
                Dim Scramble As Boolean
                Dim LookupList As String
                Dim rs As DataTable
                Dim isNewFieldRecord As Boolean = True
                '
                MethodName = "csv_VerifyCDefField_ReturnID(" & ContentName & "," & field.nameLc & ")"
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
                SQL = "select ID,ContentTableID from ccContent where name=" & cpcore.db.encodeSQLText(ContentName) & ";"
                rs = cpcore.db.executeQuery(SQL)
                If isDataTableOk(rs) Then
                    ContentID = genericController.EncodeInteger(cpcore.db.getDataRowColumnName(rs.Rows(0), "ID"))
                    TableID = genericController.EncodeInteger(cpcore.db.getDataRowColumnName(rs.Rows(0), "ContentTableID"))
                End If
                '
                ' test if field definition found or not
                '
                RecordID = 0
                RecordIsBaseField = False
                SQL = "select ID,IsBaseField from ccFields where (ContentID=" & cpcore.db.encodeSQLNumber(ContentID) & ")and(name=" & cpcore.db.encodeSQLText(field.nameLc) & ");"
                rs = cpcore.db.executeQuery(SQL)
                If isDataTableOk(rs) Then
                    isNewFieldRecord = False
                    RecordID = genericController.EncodeInteger(cpcore.db.getDataRowColumnName(rs.Rows(0), "ID"))
                    RecordIsBaseField = genericController.EncodeBoolean(cpcore.db.getDataRowColumnName(rs.Rows(0), "IsBaseField"))
                End If
                '
                ' check if this is a non-base field updating a base field
                '
                IsBaseField = field.isBaseField
                If (Not IsBaseField) And (RecordIsBaseField) Then
                    '
                    ' This update is not allowed
                    '
                    cpcore.handleException(New ApplicationException("Warning, updating non-base field with base field, content [" & ContentName & "], field [" & field.nameLc & "]"))
                End If
                If True Then
                    'FieldAdminOnly = field.adminOnly
                    FieldDeveloperOnly = field.developerOnly
                    FieldActive = field.active
                    FieldCaption = field.caption
                    FieldReadOnly = field.ReadOnly
                    fieldTypeId = field.fieldTypeId
                    FieldAuthorable = field.authorable
                    DefaultValue = genericController.encodeText(field.defaultValue)
                    NotEditable = field.NotEditable
                    LookupContentName = field.lookupContentName(cpcore)
                    AdminIndexWidth = field.indexWidth
                    AdminIndexSort = field.indexSortOrder
                    RedirectContentName = field.RedirectContentName(cpcore)
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
                    '
                    ' ----- Check error conditions before starting
                    '
                    If ContentID = -1 Then
                        '
                        ' Content Definition not found
                        '
                        Throw (New ApplicationException("Could Not create Field [" & field.nameLc & "] because Content Definition [" & ContentName & "] was Not found In ccContent Table."))
                    ElseIf TableID <= 0 Then
                        '
                        ' Content Definition not found
                        '
                        Throw (New ApplicationException("Could Not create Field [" & field.nameLc & "] because Content Definition [" & ContentName & "] has no associated Content Table."))
                    ElseIf fieldTypeId <= 0 Then
                        '
                        ' invalid field type
                        '
                        Throw (New ApplicationException("Could Not create Field [" & field.nameLc & "] because the field type [" & fieldTypeId & "] Is Not valid."))
                    Else
                        '
                        ' Get the TableName and DataSourceID
                        '
                        TableName = ""
                        rs = cpcore.db.executeQuery("Select Name, DataSourceID from ccTables where ID=" & cpcore.db.encodeSQLNumber(TableID) & ";")
                        If Not isDataTableOk(rs) Then
                            Throw (New ApplicationException("Could Not create Field [" & field.nameLc & "] because table For tableID [" & TableID & "] was Not found."))
                        Else
                            DataSourceID = genericController.EncodeInteger(cpcore.db.getDataRowColumnName(rs.Rows(0), "DataSourceID"))
                            TableName = genericController.encodeText(cpcore.db.getDataRowColumnName(rs.Rows(0), "Name"))
                        End If
                        rs.Dispose()
                        If (TableName <> "") Then
                            '
                            ' Get the DataSourceName
                            '
                            If (DataSourceID < 1) Then
                                DataSourceName = "Default"
                            Else
                                rs = cpcore.db.executeQuery("Select Name from ccDataSources where ID=" & cpcore.db.encodeSQLNumber(DataSourceID) & ";")
                                If Not isDataTableOk(rs) Then

                                    DataSourceName = "Default"
                                    ' change condition to successful -- the goal is 1) deliver pages 2) report problems
                                    ' this problem, if translated to default, is really no longer a problem, unless the
                                    ' resulting datasource does not have this data, then other errors will be generated anyway.
                                    'Call csv_HandleClassInternalError(MethodName, "Could Not create Field [" & field.name & "] because datasource For ID [" & DataSourceID & "] was Not found.")
                                Else
                                    DataSourceName = genericController.encodeText(cpcore.db.getDataRowColumnName(rs.Rows(0), "Name"))
                                End If
                                rs.Dispose()
                            End If
                            '
                            ' Get the installedByCollectionId
                            '
                            InstalledByCollectionID = 0
                            If (installedByCollectionGuid <> "") Then
                                rs = cpcore.db.executeQuery("Select id from ccAddonCollections where ccguid=" & cpcore.db.encodeSQLText(installedByCollectionGuid) & ";")
                                If isDataTableOk(rs) Then
                                    InstalledByCollectionID = genericController.EncodeInteger(cpcore.db.getDataRowColumnName(rs.Rows(0), "Id"))
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
                                Call cpcore.db.createSQLTableField(DataSourceName, TableName, field.nameLc, fieldTypeId)
                            End If
                            '
                            ' create or update the field
                            '
                            Dim sqlList As New sqlFieldListClass
                            Pointer = 0
                            Call sqlList.add("ACTIVE", cpcore.db.encodeSQLBoolean(field.active)) ' Pointer)
                            Call sqlList.add("MODIFIEDBY", cpcore.db.encodeSQLNumber(SystemMemberID)) ' Pointer)
                            Call sqlList.add("MODIFIEDDATE", cpcore.db.encodeSQLDate(Now)) ' Pointer)
                            Call sqlList.add("TYPE", cpcore.db.encodeSQLNumber(fieldTypeId)) ' Pointer)
                            Call sqlList.add("CAPTION", cpcore.db.encodeSQLText(FieldCaption)) ' Pointer)
                            Call sqlList.add("ReadOnly", cpcore.db.encodeSQLBoolean(FieldReadOnly)) ' Pointer)
                            Call sqlList.add("REQUIRED", cpcore.db.encodeSQLBoolean(FieldRequired)) ' Pointer)
                            Call sqlList.add("TEXTBUFFERED", SQLFalse) ' Pointer)
                            Call sqlList.add("PASSWORD", cpcore.db.encodeSQLBoolean(Password)) ' Pointer)
                            Call sqlList.add("EDITSORTPRIORITY", cpcore.db.encodeSQLNumber(field.editSortPriority)) ' Pointer)
                            Call sqlList.add("ADMINONLY", cpcore.db.encodeSQLBoolean(field.adminOnly)) ' Pointer)
                            Call sqlList.add("DEVELOPERONLY", cpcore.db.encodeSQLBoolean(FieldDeveloperOnly)) ' Pointer)
                            Call sqlList.add("CONTENTCONTROLID", cpcore.db.encodeSQLNumber(Models.Complex.cdefModel.getContentId(cpcore, "Content Fields"))) ' Pointer)
                            Call sqlList.add("DefaultValue", cpcore.db.encodeSQLText(DefaultValue)) ' Pointer)
                            Call sqlList.add("HTMLCONTENT", cpcore.db.encodeSQLBoolean(HTMLContent)) ' Pointer)
                            Call sqlList.add("NOTEDITABLE", cpcore.db.encodeSQLBoolean(NotEditable)) ' Pointer)
                            Call sqlList.add("AUTHORABLE", cpcore.db.encodeSQLBoolean(FieldAuthorable)) ' Pointer)
                            Call sqlList.add("INDEXCOLUMN", cpcore.db.encodeSQLNumber(field.indexColumn)) ' Pointer)
                            Call sqlList.add("INDEXWIDTH", cpcore.db.encodeSQLText(AdminIndexWidth)) ' Pointer)
                            Call sqlList.add("INDEXSORTPRIORITY", cpcore.db.encodeSQLNumber(AdminIndexSort)) ' Pointer)
                            Call sqlList.add("REDIRECTID", cpcore.db.encodeSQLText(RedirectIDField)) ' Pointer)
                            Call sqlList.add("REDIRECTPATH", cpcore.db.encodeSQLText(RedirectPath)) ' Pointer)
                            Call sqlList.add("UNIQUENAME", cpcore.db.encodeSQLBoolean(UniqueName)) ' Pointer)
                            Call sqlList.add("RSSTITLEFIELD", cpcore.db.encodeSQLBoolean(RSSTitle)) ' Pointer)
                            Call sqlList.add("RSSDESCRIPTIONFIELD", cpcore.db.encodeSQLBoolean(RSSDescription)) ' Pointer)
                            Call sqlList.add("MEMBERSELECTGROUPID", cpcore.db.encodeSQLNumber(MemberSelectGroupID)) ' Pointer)
                            Call sqlList.add("installedByCollectionId", cpcore.db.encodeSQLNumber(InstalledByCollectionID)) ' Pointer)
                            Call sqlList.add("EDITTAB", cpcore.db.encodeSQLText(EditTab)) ' Pointer)
                            Call sqlList.add("SCRAMBLE", cpcore.db.encodeSQLBoolean(Scramble)) ' Pointer)
                            Call sqlList.add("ISBASEFIELD", cpcore.db.encodeSQLBoolean(IsBaseField)) ' Pointer)
                            Call sqlList.add("LOOKUPLIST", cpcore.db.encodeSQLText(LookupList))
                            '
                            ' -- conditional fields
                            Select Case fieldTypeId
                                Case FieldTypeIdLookup
                                    '
                                    ' -- lookup field
                                    '
                                    If LookupContentName <> "" Then
                                        LookupContentID = Models.Complex.cdefModel.getContentId(cpcore, LookupContentName)
                                        If LookupContentID <= 0 Then
                                            logController.appendLog(cpcore, "Could not create lookup field [" & field.nameLc & "] for content definition [" & ContentName & "] because no content definition was found For lookup-content [" & LookupContentName & "].")
                                        End If
                                    End If
                                    Call sqlList.add("LOOKUPCONTENTID", cpcore.db.encodeSQLNumber(LookupContentID)) ' Pointer)
                                Case FieldTypeIdManyToMany
                                    '
                                    ' -- many-to-many field
                                    '
                                    Dim ManyToManyContent As String = field.ManyToManyContentName(cpcore)
                                    If ManyToManyContent <> "" Then
                                        Dim ManyToManyContentID As Integer = Models.Complex.cdefModel.getContentId(cpcore, ManyToManyContent)
                                        If ManyToManyContentID <= 0 Then
                                            logController.appendLog(cpcore, "Could not create many-to-many field [" & field.nameLc & "] for [" & ContentName & "] because no content definition was found For many-to-many-content [" & ManyToManyContent & "].")
                                        End If
                                        Call sqlList.add("MANYTOMANYCONTENTID", cpcore.db.encodeSQLNumber(ManyToManyContentID))
                                    End If
                                    '
                                    Dim ManyToManyRuleContent As String = field.ManyToManyRuleContentName(cpcore)
                                    If ManyToManyRuleContent <> "" Then
                                        Dim ManyToManyRuleContentID As Integer = Models.Complex.cdefModel.getContentId(cpcore, ManyToManyRuleContent)
                                        If ManyToManyRuleContentID <= 0 Then
                                            logController.appendLog(cpcore, "Could not create many-to-many field [" & field.nameLc & "] for [" & ContentName & "] because no content definition was found For many-to-many-rule-content [" & ManyToManyRuleContent & "].")
                                        End If
                                        Call sqlList.add("MANYTOMANYRULECONTENTID", cpcore.db.encodeSQLNumber(ManyToManyRuleContentID))
                                    End If
                                    Call sqlList.add("MANYTOMANYRULEPRIMARYFIELD", cpcore.db.encodeSQLText(field.ManyToManyRulePrimaryField))
                                    Call sqlList.add("MANYTOMANYRULESECONDARYFIELD", cpcore.db.encodeSQLText(field.ManyToManyRuleSecondaryField))
                                Case FieldTypeIdRedirect
                                    '
                                    ' -- redirect field
                                    If RedirectContentName <> "" Then
                                        RedirectContentID = Models.Complex.cdefModel.getContentId(cpcore, RedirectContentName)
                                        If RedirectContentID <= 0 Then
                                            logController.appendLog(cpcore, "Could not create redirect field [" & field.nameLc & "] for Content Definition [" & ContentName & "] because no content definition was found For redirect-content [" & RedirectContentName & "].")
                                        End If
                                    End If
                                    Call sqlList.add("REDIRECTCONTENTID", cpcore.db.encodeSQLNumber(RedirectContentID)) ' Pointer)
                            End Select
                            '
                            If RecordID = 0 Then
                                Call sqlList.add("NAME", cpcore.db.encodeSQLText(field.nameLc)) ' Pointer)
                                Call sqlList.add("CONTENTID", cpcore.db.encodeSQLNumber(ContentID)) ' Pointer)
                                Call sqlList.add("CREATEKEY", "0") ' Pointer)
                                Call sqlList.add("DATEADDED", cpcore.db.encodeSQLDate(Now)) ' Pointer)
                                Call sqlList.add("CREATEDBY", cpcore.db.encodeSQLNumber(SystemMemberID)) ' Pointer)
                                '
                                RecordID = cpcore.db.insertTableRecordGetId("Default", "ccFields")
                            End If
                            If RecordID = 0 Then
                                Throw (New ApplicationException("Could Not create Field [" & field.nameLc & "] because insert into ccfields failed."))
                            Else
                                Call cpcore.db.updateTableRecord("Default", "ccFields", "ID=" & RecordID, sqlList)
                            End If
                            '
                        End If
                    End If
                End If
                '
                If Not isNewFieldRecord Then
                    cpcore.cache.invalidateAll()
                    cpcore.doc.clearMetaData()
                End If
                '
                returnId = RecordID
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return returnId
        End Function
        '
        '=============================================================
        '
        '=============================================================
        '
        Public Shared Function isContentFieldSupported(cpcore As coreClass, ByVal ContentName As String, ByVal FieldName As String) As Boolean
            Dim returnOk As Boolean = False
            Try
                Dim cdef As Models.Complex.cdefModel
                '
                cdef = Models.Complex.cdefModel.getCdef(cpcore, ContentName)
                If (cdef IsNot Nothing) Then
                    returnOk = cdef.fields.ContainsKey(FieldName.ToLower)
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return returnOk
        End Function
        '
        '========================================================================
        ' Get a tables first ContentID from Tablename
        '========================================================================
        '
        Public Shared Function getContentIDByTablename(cpcore As coreClass, TableName As String) As Integer
            '
            Dim SQL As String
            Dim CS As Integer
            '
            getContentIDByTablename = -1
            If TableName <> "" Then
                SQL = "select ContentControlID from " & TableName & " where contentcontrolid is not null order by contentcontrolid;"
                CS = cpcore.db.csOpenSql_rev("Default", SQL, 1, 1)
                If cpcore.db.csOk(CS) Then
                    getContentIDByTablename = cpcore.db.csGetInteger(CS, "ContentControlID")
                End If
                Call cpcore.db.csClose(CS)
            End If
        End Function
        '
        '========================================================================
        '
        Public Shared Function getContentControlCriteria(cpcore As coreClass, ByVal ContentName As String) As String
            Return Models.Complex.cdefModel.getCdef(cpcore, ContentName).ContentControlCriteria
        End Function
        '
        '============================================================================================================
        '   the content control Id for a record, all its edit and archive records, and all its child records
        '   returns records affected
        '   the contentname contains the record, but we do not know that this is the contentcontrol for the record,
        '   read it first to main_Get the correct contentid
        '============================================================================================================
        '
        Public Shared Sub setContentControlId(cpcore As coreClass, ByVal ContentID As Integer, ByVal RecordID As Integer, ByVal NewContentControlID As Integer, Optional ByVal UsedIDString As String = "")
            Dim result As Integer = 0
            Dim SQL As String
            Dim CS As Integer
            Dim RecordTableName As String
            Dim ContentName As String
            Dim HasParentID As Boolean
            Dim RecordContentID As Integer
            Dim RecordContentName As String = ""
            Dim DataSourceName As String
            '
            If Not genericController.IsInDelimitedString(UsedIDString, CStr(RecordID), ",") Then
                ContentName = getContentNameByID(cpcore, ContentID)
                CS = cpcore.db.csOpenRecord(ContentName, RecordID, False, False)
                If cpcore.db.csOk(CS) Then
                    HasParentID = cpcore.db.cs_isFieldSupported(CS, "ParentID")
                    RecordContentID = cpcore.db.csGetInteger(CS, "ContentControlID")
                    RecordContentName = getContentNameByID(cpcore, RecordContentID)
                End If
                Call cpcore.db.csClose(CS)
                If RecordContentName <> "" Then
                    '
                    '
                    '
                    DataSourceName = getContentDataSource(cpcore, RecordContentName)
                    RecordTableName = Models.Complex.cdefModel.getContentTablename(cpcore, RecordContentName)
                    '
                    ' either Workflow on non-workflow - it changes everything
                    '
                    SQL = "update " & RecordTableName & " set ContentControlID=" & NewContentControlID & " where ID=" & RecordID
                    Call cpcore.db.executeQuery(SQL, DataSourceName)
                    If HasParentID Then
                        SQL = "select contentcontrolid,ID from " & RecordTableName & " where ParentID=" & RecordID
                        CS = cpcore.db.csOpenSql_rev(DataSourceName, SQL)
                        Do While cpcore.db.csOk(CS)
                            Call setContentControlId(cpcore, cpcore.db.csGetInteger(CS, "contentcontrolid"), cpcore.db.csGetInteger(CS, "ID"), NewContentControlID, UsedIDString & "," & RecordID)
                            cpcore.db.csGoNext(CS)
                        Loop
                        Call cpcore.db.csClose(CS)
                    End If
                    '
                    ' fix content watch
                    '
                    SQL = "update ccContentWatch set ContentID=" & NewContentControlID & ", ContentRecordKey='" & NewContentControlID & "." & RecordID & "' where ContentID=" & ContentID & " and RecordID=" & RecordID
                    Call cpcore.db.executeQuery(SQL)
                End If
            End If
        End Sub
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Shared Function GetContentFieldProperty(cpcore As coreClass, ByVal ContentName As String, ByVal FieldName As String, ByVal PropertyName As String) As String
            Dim result As String = String.Empty
            Try
                Dim UcaseFieldName As String = genericController.vbUCase(genericController.encodeText(FieldName))
                Dim Contentdefinition As Models.Complex.cdefModel = Models.Complex.cdefModel.getCdef(cpcore, genericController.encodeText(ContentName))
                If (UcaseFieldName = "") Or (Contentdefinition.fields.Count < 1) Then
                    Throw (New ApplicationException("Content Name [" & genericController.encodeText(ContentName) & "] or FieldName [" & genericController.encodeText(FieldName) & "] was not valid")) ' handleLegacyError14(MethodName, "")
                Else
                    For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In Contentdefinition.fields
                        Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                        With field
                            If UcaseFieldName = genericController.vbUCase(.nameLc) Then
                                Select Case genericController.vbUCase(genericController.encodeText(PropertyName))
                                    Case "FIELDTYPE", "TYPE"
                                        result = .fieldTypeId.ToString()
                                    Case "HTMLCONTENT"
                                        result = .htmlContent.ToString()
                                    Case "ADMINONLY"
                                        result = .adminOnly.ToString()
                                    Case "AUTHORABLE"
                                        result = .authorable.ToString()
                                    Case "CAPTION"
                                        result = .caption
                                    Case "REQUIRED"
                                        result = .Required.ToString()
                                    Case "UNIQUENAME"
                                        result = .UniqueName.ToString()
                                    Case "UNIQUE"
                                        '
                                        ' fix for the uniquename screwup - it is not unique name, it is unique value
                                        '
                                        result = .UniqueName.ToString()
                                    Case "DEFAULT"
                                        result = genericController.encodeText(.defaultValue)
                                    Case "MEMBERSELECTGROUPID"
                                        result = genericController.encodeText(.MemberSelectGroupID)
                                    Case Else
                                        Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError14(MethodName, "Content Property [" & genericController.encodeText(PropertyName) & "] was not found in content [" & genericController.encodeText(ContentName) & "]")
                                End Select
                                Exit For
                            End If
                        End With
                    Next
                End If
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
            Return result


        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Shared Function GetContentProperty(cpcore As coreClass, ByVal ContentName As String, ByVal PropertyName As String) As String
            Dim result As String = ""
            Dim Contentdefinition As Models.Complex.cdefModel
            '
            Contentdefinition = Models.Complex.cdefModel.getCdef(cpcore, genericController.encodeText(ContentName))
            Select Case genericController.vbUCase(genericController.encodeText(PropertyName))
                Case "CONTENTCONTROLCRITERIA"
                    result = Contentdefinition.ContentControlCriteria
                Case "ACTIVEONLY"
                    result = Contentdefinition.ActiveOnly.ToString
                Case "ADMINONLY"
                    result = Contentdefinition.AdminOnly.ToString
                Case "ALIASID"
                    result = Contentdefinition.AliasID
                Case "ALIASNAME"
                    result = Contentdefinition.AliasName
                Case "ALLOWADD"
                    result = Contentdefinition.AllowAdd.ToString
                Case "ALLOWDELETE"
                    result = Contentdefinition.AllowDelete.ToString
                'Case "CHILDIDLIST"
                '    main_result = Contentdefinition.ChildIDList
                Case "DATASOURCEID"
                    result = Contentdefinition.dataSourceId.ToString
                Case "DEFAULTSORTMETHOD"
                    result = Contentdefinition.DefaultSortMethod
                Case "DEVELOPERONLY"
                    result = Contentdefinition.DeveloperOnly.ToString
                Case "FIELDCOUNT"
                    result = Contentdefinition.fields.Count.ToString
                'Case "FIELDPOINTER"
                '    main_result = Contentdefinition.FieldPointer
                Case "ID"
                    result = Contentdefinition.Id.ToString
                Case "IGNORECONTENTCONTROL"
                    result = Contentdefinition.IgnoreContentControl.ToString
                Case "NAME"
                    result = Contentdefinition.Name
                Case "PARENTID"
                    result = Contentdefinition.parentID.ToString
                'Case "SINGLERECORD"
                '    main_result = Contentdefinition.SingleRecord
                Case "CONTENTTABLENAME"
                    result = Contentdefinition.ContentTableName
                Case "CONTENTDATASOURCENAME"
                    result = Contentdefinition.ContentDataSourceName
                'Case "AUTHORINGTABLENAME"
                '    result = Contentdefinition.AuthoringTableName
                'Case "AUTHORINGDATASOURCENAME"
                '    result = Contentdefinition.AuthoringDataSourceName
                Case "WHERECLAUSE"
                    result = Contentdefinition.WhereClause
                'Case "ALLOWWORKFLOWAUTHORING"
                '    result = Contentdefinition.AllowWorkflowAuthoring.ToString
                Case "DROPDOWNFIELDLIST"
                    result = Contentdefinition.DropDownFieldList
                Case "SELECTFIELDLIST"
                    result = Contentdefinition.SelectCommaList
                Case Else
                    Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError14(MethodName, "Content Property [" & genericController.encodeText(PropertyName) & "] was not found in content [" & genericController.encodeText(ContentName) & "]")
            End Select
            Return result
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function getCache(cpcore As coreClass, contentId As Integer) As cdefModel
            Dim result As cdefModel = Nothing
            Try
                Try
                    Dim cacheName As String = Controllers.cacheController.getCacheKey_ComplexObject("cdef", contentId.ToString)
                    result = cpcore.cache.getObject(Of Models.Complex.cdefModel)(cacheName)
                Catch ex As Exception
                    cpcore.handleException(ex)
                End Try
            Catch ex As Exception

            End Try
            Return result
        End Function
        '
        '====================================================================================================
        '
        Public Shared Sub setCache(cpcore As coreClass, contentId As Integer, cdef As cdefModel)
            Dim cacheName As String = Controllers.cacheController.getCacheKey_ComplexObject("cdef", contentId.ToString)
            '
            ' -- make it dependant on cacheNameInvalidateAll. If invalidated, all cdef will invalidate
            Dim dependantList As New List(Of String)
            dependantList.Add(cacheNameInvalidateAll)
            Call cpcore.cache.setContent(cacheName, cdef, dependantList)
        End Sub
        '
        '====================================================================================================
        '
        Public Shared Sub invalidateCache(cpCore As coreClass, contentId As Integer)
            Dim cacheName As String = Controllers.cacheController.getCacheKey_ComplexObject("cdef", contentId.ToString)
            cpCore.cache.invalidateContent(cacheName)
        End Sub
        '
        '====================================================================================================
        '
        Public Shared Sub invalidateCacheAll(cpCore As coreClass)
            cpCore.cache.invalidateContent(cacheNameInvalidateAll)
        End Sub
    End Class
End Namespace
