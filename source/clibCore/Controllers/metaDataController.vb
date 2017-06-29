
Option Explicit On
Option Strict On

Imports Contensive.Core.Models
Imports Contensive.Core.Models.Context
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers.genericController

Namespace Contensive.Core.Controllers
    Public Class metaDataController
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
        Private cdefList As Dictionary(Of String, cdefModel)
        Private tableSchemaList As Dictionary(Of String, tableSchemaModel)
        '
        Private ReadOnly Property contentNameIdDictionary As Dictionary(Of String, Integer)
            Get
                If (_contentNameIdDictionary Is Nothing) Then
                    _contentNameIdDictionary = New Dictionary(Of String, Integer)
                    For Each kvp As KeyValuePair(Of Integer, contentModel) In contentIdDict
                        Dim key As String = kvp.Value.Name.Trim().ToLower()
                        If Not String.IsNullOrEmpty(key) Then
                            If (Not _contentNameIdDictionary.ContainsKey(key)) Then
                                _contentNameIdDictionary.Add(key, kvp.Value.ID)
                            End If
                        End If
                    Next
                End If
                Return _contentNameIdDictionary
            End Get
        End Property
        Private _contentNameIdDictionary As Dictionary(Of String, Integer) = Nothing

        Private ReadOnly Property contentIdDict As Dictionary(Of Integer, contentModel)
            Get
                If (_contentIdDict Is Nothing) Then
                    _contentIdDict = contentModel.createDict(cpCore, New List(Of String))
                End If
                Return _contentIdDict
            End Get
        End Property
        Private _contentIdDict As Dictionary(Of Integer, contentModel) = Nothing
        '
        '==========================================================================================
        ''' <summary>
        ''' initialize the cdef cache. All loads are on-demand so just setup. constructed during appServices constructor so it cannot use core.app yet
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="cluster"></param>
        ''' <param name="appName"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpCore = cpCore
            '
            ' reset metaData
            '
            cdefList = New Dictionary(Of String, cdefModel)
            'contentNameIdDictionary = Nothing
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
                If (contentNameIdDictionary.ContainsKey(contentName.ToLower)) Then
                    returnId = contentNameIdDictionary(contentName.ToLower)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Function getCdef(contentName As String) As cdefModel
            Dim returnCdef As cdefModel = Nothing
            Try
                Dim ContentId As Integer
                ContentId = getContentId(contentName)
                If (ContentId < 0) Then
                    Throw New ApplicationException("No metadata was found for content name [" & contentName & "]")
                Else
                    returnCdef = getCdef(ContentId)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnCdef
        End Function
        '        
        '====================================================================================================
        ''' <summary>
        ''' return a cdef class from content id
        ''' </summary>
        ''' <param name="contentId"></param>
        ''' <returns></returns>
        Public Function getCdef(contentId As Integer, Optional forceDbLoad As Boolean = False, Optional loadInvalidFields As Boolean = False) As cdefModel
            Dim returnCdef As cdefModel = Nothing
            Try
                Dim sql As String
                Dim dt As DataTable
                Dim contentName As String
                Dim contentTablename As String
                Dim field As CDefFieldModel
                Dim row As DataRow
                Dim fieldId As Integer
                Dim fieldTypeId As Integer
                Dim fieldHtmlContent As Boolean
                Dim fieldName As String
                Dim parentCdef As cdefModel
                Dim contentIdKey As String = contentId.ToString
                '
                If (contentId <= 0) Then
                    '
                    ' -- invalid id                    
                ElseIf (Not forceDbLoad) And (cdefList.ContainsKey(contentIdKey)) Then
                    '
                    ' -- already loaded and no force re-load, just return the current cdef                    
                    returnCdef = cdefList.Item(contentIdKey)
                Else
                    If (cdefList.ContainsKey(contentIdKey)) Then
                        '
                        ' -- key is already there, remove it first                        
                        cdefList.Remove(contentIdKey)
                    End If
                    '
                    ' load cache version
                    '
                    Dim cacheName As String = Controllers.cacheController.getDbRecordCacheName("cccontent", "id", contentId.ToString)
                    Dim dependantCacheNameList As New List(Of String)
                    If (Not forceDbLoad) Then
                        Try
                            returnCdef = cpCore.cache.getObject(Of cdefModel)(cacheName)
                        Catch ex As Exception
                            cpCore.handleExceptionAndContinue(ex)
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
                            returnCdef = New cdefModel
                            With returnCdef
                                .fields = New Dictionary(Of String, CDefFieldModel)
                                .childIdList(cpCore) = New List(Of Integer)
                                .selectList = New List(Of String)
                                ' -- !!!!! changed to string because dotnet json cannot serialize an integer key
                                .adminColumns = New SortedList(Of String, cdefModel.CDefAdminColumnClass)
                                '
                                ' ----- save values in definition
                                '
                                row = dt.Rows(0)
                                contentName = Trim(genericController.encodeText(row.Item(1)))
                                'contentId = genericController.EncodeInteger(row.Item(0))
                                contentTablename = genericController.encodeText(row.Item(10))
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
                                .AuthoringDataSourceName = "default"
                                .AuthoringTableName = genericController.encodeText(row.Item(12))
                                .AllowWorkflowAuthoring = genericController.EncodeBoolean(row.Item(14))
                                .AllowCalendarEvents = genericController.EncodeBoolean(row.Item(15))
                                .DefaultSortMethod = genericController.encodeText(row.Item(17))
                                If .DefaultSortMethod = "" Then
                                    .DefaultSortMethod = "name"
                                End If
                                .EditorGroupName = genericController.encodeText(row.Item(18))
                                .AllowContentTracking = genericController.EncodeBoolean(row.Item(19))
                                .AllowTopicRules = genericController.EncodeBoolean(row.Item(20))
                                .AllowMetaContent = genericController.EncodeBoolean(row.Item(21))
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
                                        Dim parentField As CDefFieldModel = keyvaluepair.Value
                                        Dim childField As New CDefFieldModel
                                        childField = DirectCast(parentField.Clone, CDefFieldModel)
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
                                'sql = "select id from cccontent where parentid=" & .Id
                                'dt = cpCore.db.executeSql(sql)
                                'If dt.Rows.Count = 0 Then
                                '    For Each parentrow As DataRow In dt.Rows
                                '        .childIdList.Add(EncodeInteger(parentrow.Item(0)))
                                '    Next
                                'End If
                                'dt.Dispose()
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
                                        fieldName = genericController.encodeText(row.Item(13))
                                        fieldId = genericController.EncodeInteger(row.Item(12))
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
                                            field = New CDefFieldModel
                                            With field
                                                Dim fieldIndexColumn As Integer
                                                fieldTypeId = genericController.EncodeInteger(row.Item(15))
                                                If (genericController.encodeText(row.Item(4)) = "") Then
                                                    fieldIndexColumn = -1
                                                Else
                                                    fieldIndexColumn = genericController.EncodeInteger(row.Item(4))
                                                End If
                                                '
                                                ' translate htmlContent to fieldtypehtml
                                                '   this is also converted in upgrade, daily housekeep, addon install
                                                '
                                                fieldHtmlContent = genericController.EncodeBoolean(row.Item(25))
                                                If fieldHtmlContent Then
                                                    If fieldTypeId = FieldTypeIdLongText Then
                                                        fieldTypeId = FieldTypeIdHTML
                                                    ElseIf fieldTypeId = FieldTypeIdFileTextPrivate Then
                                                        fieldTypeId = FieldTypeIdFileHTMLPrivate
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
                                                .indexWidth = genericController.encodeText(EncodeInteger(Replace(genericController.encodeText(row.Item(5)), "%", "")))
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
                                                'If .lookupContentID > 0 Then
                                                '    dt = cpCore.db.executeSql("select name from cccontent where id=" & .lookupContentID)
                                                '    If dt.Rows.Count > 0 Then
                                                '        .lookupContentName(cpCore) = genericController.encodeText(dt.Rows(0).Item(0))
                                                '    End If
                                                'End If
                                                'If .manyToManyContentID > 0 Then
                                                '    dt = cpCore.db.executeSql("select name from cccontent where id=" & .manyToManyContentID)
                                                '    If dt.Rows.Count > 0 Then
                                                '        .ManyToManyContentName = genericController.encodeText(dt.Rows(0).Item(0))
                                                '    End If
                                                'End If
                                                'If .manyToManyRuleContentID > 0 Then
                                                '    dt = cpCore.db.executeSql("select name from cccontent where id=" & .manyToManyRuleContentID)
                                                '    If dt.Rows.Count > 0 Then
                                                '        .ManyToManyRuleContentName = genericController.encodeText(dt.Rows(0).Item(0))
                                                '    End If
                                                'End If
                                                'If .MemberSelectGroupID > 0 Then
                                                '    dt = cpCore.db.executeSql("select name from ccgroups where id=" & .MemberSelectGroupID)
                                                '    If dt.Rows.Count > 0 Then
                                                '        .MemberSelectGroupName(cpCore) = genericController.encodeText(dt.Rows(0).Item(0))
                                                '    End If
                                                'End If
                                                'If .RedirectContentID > 0 Then
                                                '    dt = cpCore.db.executeSql("select name from cccontent where id=" & .RedirectContentID)
                                                '    If dt.Rows.Count > 0 Then
                                                '        .RedirectContentName = genericController.encodeText(dt.Rows(0).Item(0))
                                                '    End If
                                                'End If
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
                            Call cpCore.cache.setObject(cacheName, returnCdef, dependantCacheNameList)
                        Catch ex As Exception
                            cpCore.handleExceptionAndContinue(ex)
                        End Try
                    End If
                    cdefList.Add(contentIdKey, returnCdef)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Private Function getContentControlCriteria(ByVal contentId As Integer, contentTableName As String, contentDAtaSourceName As String, ByVal parentIdList As List(Of Integer)) As String
            Dim returnCriteria As String = ""
            Try
                '
                returnCriteria = "(1=0)"
                If (contentId >= 0) Then
                    If Not parentIdList.Contains(contentId) Then
                        parentIdList.Add(contentId)
                        returnCriteria = "(" & contentTableName & ".contentcontrolId=" & contentId & ")"
                        For Each kvp As KeyValuePair(Of Integer, contentModel) In contentIdDict
                            If (kvp.Value.ParentID = contentId) Then
                                returnCriteria &= "OR" & getContentControlCriteria(kvp.Value.ID, contentTableName, contentDAtaSourceName, parentIdList)
                            End If
                        Next
                        parentIdList.Remove(contentId)
                        returnCriteria = "(" & returnCriteria & ")"
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Function isWithinContent(ByVal ChildContentID As Integer, ByVal ParentContentID As Integer) As Boolean
            Dim returnOK As Boolean = False
            Try
                Dim cdef As cdefModel
                If (ChildContentID = ParentContentID) Then
                    returnOK = True
                Else
                    cdef = getCdef(ParentContentID)
                    If Not (cdef Is Nothing) Then
                        If cdef.childIdList(cpCore).Count > 0 Then
                            returnOK = cdef.childIdList(cpCore).Contains(ChildContentID)
                            If Not returnOK Then
                                For Each contentId As Integer In cdef.childIdList(cpCore)
                                    returnOK = isWithinContent(contentId, ParentContentID)
                                    If returnOK Then Exit For
                                Next
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnOK
        End Function
        ''
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
        Public Sub getCdef_SetAdminColumns(cdef As cdefModel)
            Try
                '
                'Dim DestPtr As Integer
                ' converted array to dictionary - Dim FieldPointer As Integer
                Dim FieldActive As Boolean
                Dim FieldWidth As Integer
                Dim FieldWidthTotal As Integer
                Dim adminColumn As cdefModel.CDefAdminColumnClass
                '
                With cdef
                    If .Id > 0 Then
                        Dim cnt As Integer = 0
                        For Each keyValuePair As KeyValuePair(Of String, CDefFieldModel) In cdef.fields
                            Dim field As CDefFieldModel = keyValuePair.Value
                            FieldActive = field.active
                            FieldWidth = genericController.EncodeInteger(field.indexWidth)
                            If FieldActive And (FieldWidth > 0) Then
                                FieldWidthTotal = FieldWidthTotal + FieldWidth
                                adminColumn = New cdefModel.CDefAdminColumnClass
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
                                    adminColumn = New cdefModel.CDefAdminColumnClass
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
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        ''
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
            Dim Copy As String = ""
            Dim rowPtr As Integer
            '
            MethodName = "new_loadCdefCache_loadContentEngineContentEngine_VerifyAuthoring"
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
                    Dim cdef As cdefModel = getCdef(EncodeInteger(row("id")))
                    'If genericController.vbUCase(cdef.Name) = "PAGE CONTENT" Then
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
                                        EditRecordID = cpCore.db.insertTableRecordGetId(AuthoringDataSourceName, AuthoringTableName, SystemMemberID)
                                        LiveRecordID = genericController.EncodeInteger(dtContent.Rows(rowPtr).Item("ID"))
                                        If LiveRecordID = 159 Then
                                            LiveRecordID = LiveRecordID
                                        End If
                                        For Each keyValuePair In cdef.fields
                                            Dim field As CDefFieldModel = keyValuePair.Value
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
                                                                LiveFilename = genericController.encodeText(FieldValueVariant)
                                                                If LiveFilename <> "" Then
                                                                    EditFilename = csv_GetVirtualFilenameByTable(AuthoringTableName, field.nameLc, EditRecordID, "", field.fieldTypeId)
                                                                    FieldValueVariant = EditFilename
                                                                    If EditFilename <> "" Then
                                                                        Copy = cpCore.privateFiles.readFile(convertCdnUrlToCdnPathFilename(EditFilename))
                                                                    End If
                                                                    'Copy = contentFiles.ReadFile(LiveFilename)
                                                                    If Copy <> "" Then
                                                                        Call cpCore.privateFiles.saveFile(convertCdnUrlToCdnPathFilename(EditFilename), Copy)
                                                                        'Call publicFiles.SaveFile(EditFilename, Copy)
                                                                    End If
                                                                End If
                                                            End If
                                                            If (field.fieldTypeId = FieldTypeIdFileCSS) Or (field.fieldTypeId = FieldTypeIdFileXML) Or (field.fieldTypeId = FieldTypeIdFileJavascript) Then
                                                                '
                                                                ' create new text file and copy field - public files
                                                                '
                                                                EditFilename = ""
                                                                LiveFilename = genericController.encodeText(FieldValueVariant)
                                                                If LiveFilename <> "" Then
                                                                    EditFilename = csv_GetVirtualFilenameByTable(AuthoringTableName, field.nameLc, EditRecordID, "", field.fieldTypeId)
                                                                    FieldValueVariant = EditFilename
                                                                    If EditFilename <> "" Then
                                                                        Copy = cpCore.cdnFiles.readFile(convertCdnUrlToCdnPathFilename(EditFilename))
                                                                    End If
                                                                    'Copy = contentFiles.ReadFile(LiveFilename)
                                                                    If Copy <> "" Then
                                                                        Call cpCore.cdnFiles.saveFile(convertCdnUrlToCdnPathFilename(EditFilename), Copy)
                                                                        'Call publicFiles.SaveFile(EditFilename, Copy)
                                                                    End If
                                                                End If
                                                            End If
                                                            'SQLNames(FieldPointer) = field.Name
                                                            sqlFieldList.add(field.nameLc, cpCore.db.EncodeSQL(FieldValueVariant, field.fieldTypeId))
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
                                        Call cpCore.db.updateTableRecord(AuthoringDataSourceName, AuthoringTableName, "ID=" & EditRecordID, sqlFieldList)
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

        '
        '===========================================================================
        '   main_Get Authoring List
        '       returns a comma delimited list of ContentIDs that the Member can author
        '===========================================================================
        '
        Public Function getEditableCdefIdList() As List(Of Integer)
            Dim returnList As New List(Of Integer)
            Try
                Dim SQL As String
                Dim cidDataTable As DataTable
                Dim CIDCount As Integer
                Dim CIDPointer As Integer
                Dim CDef As cdefModel
                Dim ContentID As Integer
                '
                SQL = "Select ccGroupRules.ContentID as ID" _
                & " FROM ((ccmembersrules" _
                & " Left Join ccGroupRules on ccMemberRules.GroupID=ccGroupRules.GroupID)" _
                & " Left Join ccContent on ccGroupRules.ContentID=ccContent.ID)" _
                & " WHERE" _
                    & " (ccMemberRules.MemberID=" & cpCore.authContext.user.ID & ")" _
                    & " AND(ccGroupRules.Active<>0)" _
                    & " AND(ccContent.Active<>0)" _
                    & " AND(ccMemberRules.Active<>0)"
                cidDataTable = cpCore.db.executeSql(SQL)
                CIDCount = cidDataTable.Rows.Count
                For CIDPointer = 0 To CIDCount - 1
                    ContentID = genericController.EncodeInteger(cidDataTable.Rows(CIDPointer).Item(0))
                    returnList.Add(ContentID)
                    CDef = getCdef(ContentID)
                    If Not (CDef Is Nothing) Then
                        returnList.AddRange(CDef.childIdList(cpCore))
                    End If
                    ''main_GetContentManagementList = main_GetContentManagementList & "," & CStr(ContentID)
                    'ContentName = cpcore.metaData.getContentNameByID(ContentID)
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                _contentNameIdDictionary = Nothing
                _contentIdDict = Nothing
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '=================================================================================
        ' Returns a pointer into the cdefCache.tableSchema() array for the table that matches
        '   returns -1 if the table is not found
        '=================================================================================
        '
        Public Function getTableSchema(ByVal TableName As String, ByVal DataSourceName As String) As tableSchemaModel
            Dim tableSchema As tableSchemaModel = Nothing
            Try
                Dim dt As DataTable
                Dim isInCache As Boolean = False
                Dim isInDb As Boolean = False
                'Dim readFromDb As Boolean
                Dim lowerTablename As String
                Dim buildCache As Boolean
                '
                If (DataSourceName <> "") And (DataSourceName <> "-1") And (DataSourceName.ToLower <> "default") Then
                    Throw New NotImplementedException("alternate datasources not supported yet")
                Else
                    If TableName <> "" Then
                        lowerTablename = TableName.ToLower
                        If (tableSchemaList) Is Nothing Then
                            tableSchemaList = New Dictionary(Of String, tableSchemaModel)
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
                                tableSchema = New tableSchemaModel
                                tableSchema.columns = New List(Of String)
                                tableSchema.indexes = New List(Of String)
                                tableSchema.TableName = lowerTablename
                                '
                                ' load columns
                                '
                                dt = cpCore.db.getColumnSchemaData(TableName)
                                If dt.Rows.Count > 0 Then
                                    For Each row As DataRow In dt.Rows
                                        tableSchema.columns.Add(genericController.encodeText(row("COLUMN_NAME")).ToLower)
                                    Next
                                End If
                                '
                                ' Load the index schema
                                '
                                dt = cpCore.db.getIndexSchemaData(TableName)
                                If dt.Rows.Count > 0 Then
                                    For Each row As DataRow In dt.Rows
                                        tableSchema.indexes.Add(genericController.encodeText(row("INDEX_NAME")).ToLower)
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        ''' <param name="ContentName"></param>
        Public Sub deleteContent(ContentName As String)
            Try
                Models.Entity.contentModel.delete(cpCore, cpCore.metaData.getContentId(ContentName))
                clear()
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.cache.invalidateObject("content")
                clear()
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Sub createContentChild(ByVal ChildContentName As String, ByVal ParentContentName As String, ByVal MemberID As Integer)
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
                    ChildContentID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows(0), "ID"))
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
                        ParentContentID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows(0), "ID"))
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
                        Throw (New ApplicationException("Can not create Child Content [" & ChildContentName & "] because the Parent Content [" & ParentContentName & "] was not found."))
                    Else
                        '
                        ' ----- create child content record, let the csv_ExecuteSQL reload CDef
                        '
                        DataSourceName = "Default"
                        CSContent = cpCore.db.cs_openContentRecord("Content", ParentContentID)
                        If Not cpCore.db.cs_ok(CSContent) Then
                            Throw (New ApplicationException("Can not create Child Content [" & ChildContentName & "] because the Parent Content [" & ParentContentName & "] was not found."))
                        Else
                            SelectFieldList = cpCore.db.cs_getSelectFieldList(CSContent)
                            If SelectFieldList = "" Then
                                Throw (New ApplicationException("Can not create Child Content [" & ChildContentName & "] because the Parent Content [" & ParentContentName & "] record has not fields."))
                            Else
                                CSNew = cpCore.db.cs_insertRecord("Content", 0)
                                If Not cpCore.db.cs_ok(CSNew) Then
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
                                                Call cpCore.db.cs_set(CSNew, FieldName, ChildContentName)
                                            Case "PARENTID"
                                                Call cpCore.db.cs_set(CSNew, FieldName, cpCore.db.cs_getText(CSContent, "ID"))
                                            Case "CREATEDBY", "MODIFIEDBY"
                                                Call cpCore.db.cs_set(CSNew, FieldName, MemberID)
                                            Case "DATEADDED", "MODIFIEDDATE"
                                                Call cpCore.db.cs_set(CSNew, FieldName, DateNow)
                                            Case "CCGUID"

                                                '
                                                ' new, non-blank guid so if this cdef is exported, it will be updateable
                                                '
                                                Call cpCore.db.cs_set(CSNew, FieldName, Guid.NewGuid.ToString())
                                            Case Else
                                                Call cpCore.db.cs_set(CSNew, FieldName, cpCore.db.cs_getText(CSContent, FieldName))
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
                Throw (New ApplicationException("Exception in CreateContentChild"))
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
                Dim CPtr As Integer
                Dim C As String
                Dim CValue As Integer
                Dim crc As Integer
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Dim CDef As cdefModel
                '
                CDef = getCdef(ContentName)
                If (CDef IsNot Nothing) Then
                    returnTableName = CDef.ContentTableName
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Dim CDef As cdefModel
                '
                CDef = getCdef(ContentName)
                If (CDef Is Nothing) Then
                    '
                Else
                    returnDataSource = CDef.ContentDataSourceName
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
                Dim cdef As cdefModel
                '
                cdef = getCdef(ContentID)
                If cdef IsNot Nothing Then
                    returnName = cdef.Name
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnName
        End Function

        '========================================================================
        '   Create a content definition
        '       called from upgrade and DeveloperTools
        '========================================================================
        '
        Public Function createContent(ByVal Active As Boolean, datasource As dataSourceModel, ByVal TableName As String, ByVal contentName As String, Optional ByVal AdminOnly As Boolean = False, Optional ByVal DeveloperOnly As Boolean = False, Optional ByVal AllowAdd As Boolean = True, Optional ByVal AllowDelete As Boolean = True, Optional ByVal ParentName As String = "", Optional ByVal DefaultSortMethod As String = "", Optional ByVal DropDownFieldList As String = "", Optional ByVal AllowWorkflowAuthoring As Boolean = False, Optional ByVal AllowCalendarEvents As Boolean = False, Optional ByVal AllowContentTracking As Boolean = False, Optional ByVal AllowTopicRules As Boolean = False, Optional ByVal AllowContentChildTool As Boolean = False, Optional ByVal AllowMetaContent As Boolean = False, Optional ByVal IconLink As String = "", Optional ByVal IconWidth As Integer = 0, Optional ByVal IconHeight As Integer = 0, Optional ByVal IconSprites As Integer = 0, Optional ByVal ccGuid As String = "", Optional ByVal IsBaseContent As Boolean = False, Optional ByVal installedByCollectionGuid As String = "", Optional clearMetaCache As Boolean = False) As Integer
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
                'Dim DataSourceID As Integer
                Dim iDefaultSortMethod As String
                Dim DefaultSortMethodID As Integer
                Dim CDefFound As Boolean
                Dim InstalledByCollectionID As Integer
                Dim sqlList As sqlFieldListClass
                Dim field As CDefFieldModel
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
                        Call cpCore.db.createSQLTable(datasource.Name, TableName)
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
                        SQL = "select ID,ccguid,IsBaseContent from ccContent where (name=" & cpCore.db.encodeSQLText(contentName) & ") order by id;"
                        dt = cpCore.db.executeSql(SQL)
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
                            dt = cpCore.db.executeSql(SQL)
                            If dt.Rows.Count > 0 Then
                                ContentIDofContent = genericController.EncodeInteger(dt.Rows(0).Item("ID"))
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
                                parentId = genericController.EncodeInteger(dt.Rows(0).Item(0))
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
                                returnContentId = cpCore.db.insertTableRecordGetId("Default", "ccContent", SystemMemberID)
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
                                TableID = cpCore.db.insertTableRecordGetId("Default", "ccTables", SystemMemberID)
                                '
                                sqlList = New sqlFieldListClass
                                sqlList.add("name", cpCore.db.encodeSQLText(TableName))
                                sqlList.add("active", SQLTrue)
                                sqlList.add("DATASOURCEID", cpCore.db.encodeSQLNumber(datasource.ID))
                                sqlList.add("CONTENTCONTROLID", cpCore.db.encodeSQLNumber(cpCore.db.getContentId("Tables")))
                                '
                                Call cpCore.db.updateTableRecord("Default", "ccTables", "ID=" & TableID, sqlList)
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
                                dt = cpCore.db.openTable("Default", "ccSortMethods", "(name=" & cpCore.db.encodeSQLText(iDefaultSortMethod) & ")and(active<>0)", "ID", "ID", 1, 1)
                                If dt.Rows.Count > 0 Then
                                    DefaultSortMethodID = genericController.EncodeInteger(dt.Rows(0).Item("ID"))
                                End If
                            End If
                            If DefaultSortMethodID = 0 Then
                                '
                                ' fallback - maybe they put the orderbyclause in (common mistake)
                                '
                                dt = cpCore.db.openTable("Default", "ccSortMethods", "(OrderByClause=" & cpCore.db.encodeSQLText(iDefaultSortMethod) & ")and(active<>0)", "ID", "ID", 1, 1)
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
                            Call sqlList.add("name", cpCore.db.encodeSQLText(contentName))
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
                                ElseIf (NewGuid <> "") And (LcContentGuid <> genericController.vbLCase(NewGuid)) Then
                                    '
                                    ' new guid does not match current guid
                                    '
                                    'cpCore.AppendLog("upgrading cdef [" & ContentName & "], the guid was not updated because the current guid [" & LcContentGuid & "] is not empty, and it did not match the new guid [" & genericController.vbLCase(NewGuid) & "]")
                                    'Call AppendLog2(cpCore,appEnvironment.name, "upgrading cdef [" & ContentName & "], the guid was not updated because the current guid [" & LcContentGuid & "] is not empty, and it did not match the new guid [" & genericController.vbLCase(NewGuid) & "]", "dll", "cpCoreClass", "csv_CreateContent3", 0, "", "", False, True, "", "", "")
                                End If
                            End If
                            If returnContentId = 54 Then
                                returnContentId = returnContentId
                            End If
                            Call cpCore.db.updateTableRecord("Default", "ccContent", "ID=" & returnContentId, sqlList)
                            '
                            '-----------------------------------------------------------------------------------------------
                            ' Verify Core Content Definition Fields
                            '-----------------------------------------------------------------------------------------------
                            '
                            If parentId < 1 Then
                                '
                                ' CDef does not inherit its fields, create what is needed for a non-inherited CDef
                                '
                                If Not cpCore.db.isCdefField(returnContentId, "ID") Then
                                    field = New CDefFieldModel
                                    field.nameLc = "id"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdAutoIdIncrement
                                    field.editSortPriority = 100
                                    field.authorable = False
                                    field.caption = "ID"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(contentName, field)
                                End If
                                '
                                If Not cpCore.db.isCdefField(returnContentId, "name") Then
                                    field = New CDefFieldModel
                                    field.nameLc = "name"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdText
                                    field.editSortPriority = 110
                                    field.authorable = True
                                    field.caption = "Name"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(contentName, field)
                                End If
                                '
                                If Not cpCore.db.isCdefField(returnContentId, "active") Then
                                    field = New CDefFieldModel
                                    field.nameLc = "active"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdBoolean
                                    field.editSortPriority = 200
                                    field.authorable = True
                                    field.caption = "Active"
                                    field.defaultValue = "1"
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(contentName, field)
                                End If
                                '
                                If Not cpCore.db.isCdefField(returnContentId, "sortorder") Then
                                    field = New CDefFieldModel
                                    field.nameLc = "sortorder"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdText
                                    field.editSortPriority = 2000
                                    field.authorable = False
                                    field.caption = "Alpha Sort Order"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(contentName, field)
                                End If
                                '
                                If Not cpCore.db.isCdefField(returnContentId, "dateadded") Then
                                    field = New CDefFieldModel
                                    field.nameLc = "dateadded"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdDate
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Date Added"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(contentName, field)
                                End If
                                If Not cpCore.db.isCdefField(returnContentId, "createdby") Then
                                    field = New CDefFieldModel
                                    field.nameLc = "createdby"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdLookup
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Created By"
                                    field.lookupContentName(cpCore) = "People"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(contentName, field)
                                End If
                                If Not cpCore.db.isCdefField(returnContentId, "modifieddate") Then
                                    field = New CDefFieldModel
                                    field.nameLc = "modifieddate"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdDate
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Date Modified"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(contentName, field)
                                End If
                                If Not cpCore.db.isCdefField(returnContentId, "modifiedby") Then
                                    field = New CDefFieldModel
                                    field.nameLc = "modifiedby"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdLookup
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Modified By"
                                    field.lookupContentName(cpCore) = "People"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(contentName, field)
                                End If
                                If Not cpCore.db.isCdefField(returnContentId, "ContentControlID") Then
                                    field = New CDefFieldModel
                                    field.nameLc = "ContentControlID"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdLookup
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Controlling Content"
                                    field.lookupContentName(cpCore) = "Content"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(contentName, field)
                                End If
                                If Not cpCore.db.isCdefField(returnContentId, "CreateKey") Then
                                    field = New CDefFieldModel
                                    field.nameLc = "CreateKey"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdInteger
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Create Key"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(contentName, field)
                                End If
                                '
                                ' REFACTOR - these fieldsonly apply to page content
                                '
                                If Not cpCore.db.isCdefField(returnContentId, "EditSourceID") Then
                                    field = New CDefFieldModel
                                    field.nameLc = "EditSourceID"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdInteger
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Edit Source ID"
                                    field.lookupContentName(cpCore) = ""
                                    field.defaultValue = "null"
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(contentName, field)
                                End If
                                If Not cpCore.db.isCdefField(returnContentId, "EditArchive") Then
                                    field = New CDefFieldModel
                                    field.nameLc = "EditArchive"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdBoolean
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Edit Archive"
                                    field.lookupContentName(cpCore) = ""
                                    field.defaultValue = "0"
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(contentName, field)
                                End If
                                If Not cpCore.db.isCdefField(returnContentId, "EditBlank") Then
                                    field = New CDefFieldModel
                                    field.nameLc = "EditBlank"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdBoolean
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Edit Blank"
                                    field.lookupContentName(cpCore) = ""
                                    field.defaultValue = "0"
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(contentName, field)
                                End If
                                If Not cpCore.db.isCdefField(returnContentId, "ContentCategoryID") Then
                                    field = New CDefFieldModel
                                    field.nameLc = "ContentCategoryID"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdLookup
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Content Category"
                                    field.lookupContentName(cpCore) = "Content Categories"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(contentName, field)
                                End If
                                If Not cpCore.db.isCdefField(returnContentId, "ccGuid") Then
                                    field = New CDefFieldModel
                                    field.nameLc = "ccGuid"
                                    field.active = True
                                    field.fieldTypeId = FieldTypeIdText
                                    field.editSortPriority = 9999
                                    field.authorable = False
                                    field.caption = "Guid"
                                    field.defaultValue = ""
                                    field.isBaseField = IsBaseContent
                                    Call verifyCDefField_ReturnID(contentName, field)
                                End If
                            End If
                            '
                            ' ----- Load CDef
                            '
                            If clearMetaCache Then
                                cpCore.cache.invalidateContent("content")
                                cpCore.cache.invalidateContent("content fields")
                                cpCore.metaData.clear()
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Function verifyCDefField_ReturnID(ByVal ContentName As String, field As CDefFieldModel) As Integer ' , ByVal FieldName As String, ByVal Args As String, ByVal Delimiter As String) As Integer
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
                    ContentID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows(0), "ID"))
                    TableID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows(0), "ContentTableID"))
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
                    RecordID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows(0), "ID"))
                    RecordIsBaseField = genericController.EncodeBoolean(cpCore.db.getDataRowColumnName(rs.Rows(0), "IsBaseField"))
                End If
                '
                ' check if this is a non-base field updating a base field
                '
                IsBaseField = field.isBaseField
                If (Not IsBaseField) And (RecordIsBaseField) Then
                    '
                    ' This update is not allowed
                    '
                    Throw (New ApplicationException("Unexpected exception")) '  cpCore.handleLegacyError2("cpCoreClass", "csv_VerifyCDefField_ReturnID", cpCore.serverConfig.appConfig.name & ", Warning, a Base field Is being updated To non-base. This should only happen When a base field Is removed from the base collection. Content [" & ContentName & "], field [" & field.nameLc & "].")
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
                    DefaultValue = genericController.encodeText(field.defaultValue)
                    NotEditable = field.NotEditable
                    LookupContentName = field.lookupContentName(cpCore)
                    'field.indexColumn = field.indexColumn
                    AdminIndexWidth = field.indexWidth
                    AdminIndexSort = field.indexSortOrder
                    RedirectContentName = field.RedirectContentName(cpCore)
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
                    ManyToManyContent = field.ManyToManyContentName(cpCore)
                    ManyToManyRuleContent = field.ManyToManyRuleContentName(cpCore)
                    ManyToManyRulePrimaryField = field.ManyToManyRulePrimaryField
                    ManyToManyRuleSecondaryField = field.ManyToManyRuleSecondaryField
                    '
                    If RedirectContentName <> "" Then
                        RedirectContentID = cpCore.db.getContentId(RedirectContentName)
                        If RedirectContentID <= 0 Then
                            Throw (New Exception("Could Not create redirect For field [" & field.nameLc & "] For Content Definition [" & ContentName & "] because no Content Definition was found For RedirectContentName [" & RedirectContentName & "]."))
                        End If
                    End If
                    '
                    If LookupContentName <> "" Then
                        LookupContentID = cpCore.db.getContentId(LookupContentName)
                        If LookupContentID <= 0 Then
                            Throw (New Exception("Could Not create lookup For field [" & field.nameLc & "] For Content Definition [" & ContentName & "] because no Content Definition was found For [" & LookupContentName & "]."))
                        End If
                    End If
                    '
                    If ManyToManyContent <> "" Then
                        ManyToManyContentID = cpCore.db.getContentId(ManyToManyContent)
                        If ManyToManyContentID <= 0 Then
                            Throw (New ApplicationException("Could Not create many To many For field [" & field.nameLc & "] For Content Definition [" & ContentName & "] because no Content Definition was found For ManyToManyContent [" & ManyToManyContent & "]."))
                        End If
                    End If
                    '
                    If ManyToManyRuleContent <> "" Then
                        ManyToManyRuleContentID = cpCore.db.getContentId(ManyToManyRuleContent)
                        If ManyToManyRuleContentID <= 0 Then
                            Throw (New ApplicationException("Could Not create many To many For field [" & field.nameLc & "] For Content Definition [" & ContentName & "] because no Content Definition was found For ManyToManyRuleContent [" & ManyToManyRuleContent & "]."))
                        End If
                    End If
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
                        rs = cpCore.db.executeSql("Select Name, DataSourceID from ccTables where ID=" & cpCore.db.encodeSQLNumber(TableID) & ";")
                        If Not isDataTableOk(rs) Then
                            Throw (New ApplicationException("Could Not create Field [" & field.nameLc & "] because table For tableID [" & TableID & "] was Not found."))
                        Else
                            DataSourceID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows(0), "DataSourceID"))
                            TableName = genericController.encodeText(cpCore.db.getDataRowColumnName(rs.Rows(0), "Name"))
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
                                    DataSourceName = genericController.encodeText(cpCore.db.getDataRowColumnName(rs.Rows(0), "Name"))
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
                                    InstalledByCollectionID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows(0), "Id"))
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
                                Call cpCore.db.createSQLTableField(DataSourceName, TableName, field.nameLc, fieldTypeId)
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
                            Call sqlList.add("CONTENTCONTROLID", cpCore.db.encodeSQLNumber(cpCore.db.getContentId("Content Fields"))) ' Pointer)
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
                                RecordID = cpCore.db.insertTableRecordGetId("Default", "ccFields")
                            End If
                            If RecordID = 0 Then
                                Throw (New ApplicationException("Could Not create Field [" & field.nameLc & "] because insert into ccfields failed."))
                            Else
                                Call cpCore.db.updateTableRecord("Default", "ccFields", "ID=" & RecordID, sqlList)
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
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnId
        End Function
        '
        '=============================================================
        '
        '=============================================================
        '
        Public Function isContentFieldSupported(ByVal ContentName As String, ByVal FieldName As String) As Boolean
            Dim returnOk As Boolean = False
            Try
                Dim cdef As cdefModel
                '
                cdef = getCdef(ContentName)
                If (cdef IsNot Nothing) Then
                    returnOk = cdef.fields.ContainsKey(FieldName.ToLower)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnOk
        End Function
        '
        '========================================================================
        ' Get a tables first ContentID from Tablename
        '========================================================================
        '
        Public Function GetContentIDByTablename(TableName As String) As Integer
            '
            Dim SQL As String
            Dim CS As Integer
            '
            GetContentIDByTablename = -1
            If TableName <> "" Then
                SQL = "select ContentControlID from " & TableName & " where contentcontrolid is not null order by contentcontrolid;"
                CS = cpCore.db.cs_openCsSql_rev("Default", SQL, 1, 1)
                If cpCore.db.cs_ok(CS) Then
                    GetContentIDByTablename = cpCore.db.cs_getInteger(CS, "ContentControlID")
                End If
                Call cpCore.db.cs_Close(CS)
            End If
        End Function
        '
        '========================================================================
        '
        Public Function content_getContentControlCriteria(ByVal ContentName As String) As String
            Return getCdef(ContentName).ContentControlCriteria
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