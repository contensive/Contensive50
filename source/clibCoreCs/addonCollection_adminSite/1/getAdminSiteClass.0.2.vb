'
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Imports System.Xml
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
'
Namespace Contensive.Addons.AdminSite
    Public Class getAdminSiteClass
        Inherits Contensive.BaseClasses.AddonBaseClass

        '
        '=============================================================================
        '   Export the Admin List form results
        '=============================================================================
        '
        Private Function GetForm_Index_Export(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
            On Error GoTo ErrorTrap
            '
            Dim AllowContentAccess As Boolean
            Dim ButtonList As String = ""
            Dim ExportName As String
            Dim Adminui As New adminUIController(cpCore)
            Dim Description As String
            Dim Content As String = ""
            Dim ExportType As Integer
            Dim Button As String
            Dim RecordLimit As Integer
            Dim recordCnt As Integer
            'Dim DataSourceName As String
            'Dim DataSourceType As Integer
            Dim sqlFieldList As String = ""
            Dim SQLFrom As String = ""
            Dim SQLWhere As String = ""
            Dim SQLOrderBy As String = ""
            Dim IsLimitedToSubContent As Boolean
            Dim ContentAccessLimitMessage As String = ""
            Dim FieldUsedInColumns As New Dictionary(Of String, Boolean)
            Dim IsLookupFieldValid As New Dictionary(Of String, Boolean)
            Dim IndexConfig As indexConfigClass
            Dim SQL As String
            Dim CS As Integer
            'Dim RecordTop As Integer
            'Dim RecordsPerPage As Integer
            Dim IsRecordLimitSet As Boolean
            Dim RecordLimitText As String
            Dim allowContentEdit As Boolean
            Dim allowContentAdd As Boolean
            Dim allowContentDelete As Boolean
            Dim datasource As Models.Entity.dataSourceModel = Models.Entity.dataSourceModel.create(cpCore, adminContent.dataSourceId, New List(Of String))
            '
            ' ----- Process Input
            '
            Button = cpCore.docProperties.getText("Button")
            If Button = ButtonCancelAll Then
                '
                ' Cancel out to the main page
                '
                Return cpCore.webServer.redirect("?", "CancelAll button pressed on Index Export")
            ElseIf Button <> ButtonCancel Then
                '
                ' get content access rights
                '
                Call cpCore.doc.authContext.getContentAccessRights(cpCore, adminContent.Name, allowContentEdit, allowContentAdd, allowContentDelete)
                If Not allowContentEdit Then
                    'If Not cpCore.doc.authContext.user.main_IsContentManager2(AdminContent.Name) Then
                    '
                    ' You must be a content manager of this content to use this tool
                    '
                    Content = "" _
                        & "<p>You must be a content manager of " & adminContent.Name & " to use this tool. Hit Cancel to return to main admin page.</p>" _
                        & cpCore.html.html_GetFormInputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormExport) _
                        & ""
                    ButtonList = ButtonCancelAll
                Else
                    IsRecordLimitSet = False
                    If Button = "" Then
                        '
                        ' Set Defaults
                        '
                        ExportName = ""
                        ExportType = 1
                        RecordLimit = 0
                        RecordLimitText = ""
                    Else
                        ExportName = cpCore.docProperties.getText("ExportName")
                        ExportType = cpCore.docProperties.getInteger("ExportType")
                        RecordLimitText = cpCore.docProperties.getText("RecordLimit")
                        If RecordLimitText <> "" Then
                            IsRecordLimitSet = True
                            RecordLimit = genericController.EncodeInteger(RecordLimitText)
                        End If
                    End If
                    If ExportName = "" Then
                        ExportName = adminContent.Name & " export for " & cpCore.doc.authContext.user.name
                    End If
                    '
                    ' Get the SQL parts
                    '
                    'DataSourceName = cpCore.db.getDataSourceNameByID(adminContent.dataSourceId)
                    'DataSourceType = cpCore.db.getDataSourceType(DataSourceName)
                    IndexConfig = LoadIndexConfig(adminContent)
                    'RecordTop = IndexConfig.RecordTop
                    'RecordsPerPage = IndexConfig.RecordsPerPage
                    Call SetIndexSQL(adminContent, editRecord, IndexConfig, AllowContentAccess, sqlFieldList, SQLFrom, SQLWhere, SQLOrderBy, IsLimitedToSubContent, ContentAccessLimitMessage, FieldUsedInColumns, IsLookupFieldValid)
                    If Not AllowContentAccess Then
                        '
                        ' This should be caught with check earlier, but since I added this, and I never make mistakes, I will leave this in case there is a mistake in the earlier code
                        '
                        Call errorController.error_AddUserError(cpCore, "Your account does not have access to any records in '" & adminContent.Name & "'.")
                    Else
                        '
                        ' Get the total record count
                        '
                        SQL = "select count(" & adminContent.ContentTableName & ".ID) as cnt from " & SQLFrom & " where " & SQLWhere
                        CS = cpCore.db.csOpenSql_rev(datasource.Name, SQL)
                        If cpCore.db.csOk(CS) Then
                            recordCnt = cpCore.db.csGetInteger(CS, "cnt")
                        End If
                        Call cpCore.db.csClose(CS)
                        '
                        ' Build the SQL
                        '
                        SQL = "select"
                        If IsRecordLimitSet And (datasource.type <> DataSourceTypeODBCMySQL) Then
                            SQL &= " Top " & RecordLimit
                        End If
                        SQL &= " " & adminContent.ContentTableName & ".* From " & SQLFrom & " WHERE " & SQLWhere
                        If SQLOrderBy <> "" Then
                            SQL &= " Order By" & SQLOrderBy
                        End If
                        If IsRecordLimitSet And (datasource.type = DataSourceTypeODBCMySQL) Then
                            SQL &= " Limit " & RecordLimit
                        End If
                        '
                        ' Assumble the SQL
                        '
                        If recordCnt = 0 Then
                            '
                            ' There are no records to request
                            '
                            Content = "" _
                                & "<p>This selection has no records.. Hit Cancel to return to the " & adminContent.Name & " list page.</p>" _
                                & cpCore.html.html_GetFormInputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormExport) _
                                & ""
                            ButtonList = ButtonCancel
                        ElseIf Button = ButtonRequestDownload Then
                            '
                            ' Request the download
                            '
                            Select Case ExportType
                                Case 1
                                    Call taskSchedulerController.main_RequestTask(cpCore, "BuildCSV", SQL, ExportName, "Export-" & CStr(genericController.GetRandomInteger) & ".csv")
                                Case Else
                                    Call taskSchedulerController.main_RequestTask(cpCore, "BuildXML", SQL, ExportName, "Export-" & CStr(genericController.GetRandomInteger) & ".xml")
                            End Select
                            '
                            Content = "" _
                                & "<p>Your export has been requested and will be available shortly in the <a href=""?" & RequestNameAdminForm & "=" & AdminFormDownloads & """>Download Manager</a>. Hit Cancel to return to the " & adminContent.Name & " list page.</p>" _
                                & cpCore.html.html_GetFormInputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormExport) _
                                & ""
                            '
                            ButtonList = ButtonCancel
                        Else
                            '
                            ' no button or refresh button, Ask are you sure
                            '
                            Content = Content _
                                & cr & "<tr>" _
                                & cr2 & "<td class=""exportTblCaption"">Export Name</td>" _
                                & cr2 & "<td class=""exportTblInput"">" & cpCore.html.html_GetFormInputText2("ExportName", ExportName) & "</td>" _
                                & cr & "</tr>"
                            Content = Content _
                                & cr & "<tr>" _
                                & cr2 & "<td class=""exportTblCaption"">Export Format</td>" _
                                & cr2 & "<td class=""exportTblInput"">" & cpCore.html.getInputSelectList2("ExportType", ExportType, "Comma Delimited,XML", "", "") & "</td>" _
                                & cr & "</tr>"
                            Content = Content _
                                & cr & "<tr>" _
                                & cr2 & "<td class=""exportTblCaption"">Records Found</td>" _
                                & cr2 & "<td class=""exportTblInput"">" & cpCore.html.html_GetFormInputText2("RecordCnt", CStr(recordCnt), , , , , True) & "</td>" _
                                & cr & "</tr>"
                            Content = Content _
                                & cr & "<tr>" _
                                & cr2 & "<td class=""exportTblCaption"">Record Limit</td>" _
                                & cr2 & "<td class=""exportTblInput"">" & cpCore.html.html_GetFormInputText2("RecordLimit", RecordLimitText) & "</td>" _
                                & cr & "</tr>"
                            If cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore) Then
                                Content = Content _
                                    & cr & "<tr>" _
                                    & cr2 & "<td class=""exportTblCaption"">Results SQL</td>" _
                                    & cr2 & "<td class=""exportTblInput""><div style=""border:1px dashed #ccc;background-color:#fff;padding:10px;"">" & SQL & "</div></td>" _
                                    & cr & "</tr>" _
                                    & ""
                            End If
                            '
                            Content = "" _
                                & cr & "<table>" _
                                & genericController.htmlIndent(Content) _
                                & cr & "</table>" _
                                & ""
                            '
                            Content = "" _
                                & cr & "<style>" _
                                & cr2 & ".exportTblCaption {width:100px;}" _
                                & cr2 & ".exportTblInput {}" _
                                & cr & "</style>" _
                                & Content _
                                & cpCore.html.html_GetFormInputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormExport) _
                                & ""
                            ButtonList = ButtonCancel & "," & ButtonRequestDownload
                            If cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore) Then
                                ButtonList = ButtonList & "," & ButtonRefresh
                            End If
                        End If
                    End If
                End If
                '
                Description = "<p>This tool creates an export of the current admin list page results. If you would like to download the current results, select a format and press OK. Your search results will be submitted for export. Your download will be ready shortly in the download manager. To exit without requesting an output, hit Cancel.</p>"
                GetForm_Index_Export = "" _
                    & Adminui.GetBody(adminContent.Name & " Export", ButtonList, "", False, False, Description, "", 10, Content)
            End If
            '
            Exit Function
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Index_Export")
        End Function
        '
        '=============================================================================
        '   Print the Configure Index Form
        '=============================================================================
        '
        Private Function GetForm_Index_SetColumns(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
            On Error GoTo ErrorTrap
            '
            Dim Button As String
            Dim Ptr As Integer
            Dim Description As String

            Dim NeedToReloadCDef As Boolean
            Dim Title As String
            Dim TitleBar As String
            Dim Content As String
            Dim ButtonBar As String
            Dim Adminui As New adminUIController(cpCore)
            Dim SQL As String
            Dim MenuHeader As String
            Dim ColumnPtr As Integer
            Dim ColumnWidth As Integer
            Dim FieldPtr As Integer
            Dim FieldName As String
            Dim FieldToAdd As Integer
            Dim AStart As String
            Dim CS As Integer
            Dim SetSort As Boolean
            Dim MenuEntryID As Integer
            Dim MenuHeaderID As Integer
            Dim MenuDirection As Integer
            Dim SourceID As Integer
            Dim PreviousID As Integer
            Dim SetID As Integer
            Dim NextSetID As Integer
            Dim SwapWithPrevious As Boolean
            Dim HitID As Integer
            Dim HitTable As String
            Dim SortPriorityLowest As Integer
            Dim TempColumn As String
            Dim Tempfield As String
            Dim TempWidth As String
            Dim TempSortPriority As Integer
            Dim TempSortDirection As Integer
            Dim CSPointer As Integer
            Dim RecordID As Integer
            Dim ContentID As Integer
            Dim CDef As Models.Complex.cdefModel
            'Dim AdminColumn As appServices_metaDataClass.CDefAdminColumnType
            Dim RowFieldID() As Integer
            Dim RowFieldWidth() As Integer
            Dim RowFieldCaption() As String
            'Dim RowFieldCount as integer
            Dim NonRowFieldID() As Integer
            Dim NonRowFieldCaption() As String
            Dim NonRowFieldCount As Integer
            Dim ContentName As String
            '
            Dim dt As DataTable
            Dim IndexWidth As Integer
            Dim CS1 As Integer
            Dim CS2 As Integer
            Dim FieldPtr1 As Integer
            Dim FieldPtr2 As Integer
            Dim NewRowFieldWidth As Integer
            Dim TargetFieldID As Integer
            Dim TargetFieldName As String
            '
            Dim ColumnWidthTotal As Integer
            '
            Dim ColumnPointer As Integer
            Dim CDefFieldCount As Integer
            Dim fieldId As Integer
            Dim FieldWidth As Integer
            Dim AllowContentAutoLoad As Boolean
            Dim TargetFieldPtr As Integer
            Dim MoveNextColumn As Boolean
            Dim FieldNameToAdd As String
            Dim FieldIDToAdd As Integer
            Dim CSSource As Integer
            Dim CSTarget As Integer
            Dim SourceContentID As Integer
            Dim SourceName As String
            Dim NeedToReloadConfig As Boolean
            Dim InheritedFieldCount As Integer
            Dim Caption As String
            'Dim ContentNameValues() As NameValuePrivateType
            Dim ContentCount As Integer
            Dim ContentSize As Integer
            Dim Stream As New stringBuilderLegacyController
            Dim ButtonList As String
            Dim FormPanel As String
            Dim ColumnWidthIncrease As Integer
            Dim ColumnWidthBalance As Integer
            Dim ToolsAction As Integer
            Dim IndexConfig As indexConfigClass
            ''Dim arrayOfFields() As appServices_metaDataClass.CDefFieldClass
            Dim FieldPointerTemp As Integer
            Dim NameTemp As String
            Dim WidthTemp As Integer
            '
            Const RequestNameAddField = "addfield"
            Const RequestNameAddFieldID = "addfieldID"
            '
            '
            '--------------------------------------------------------------------------------
            '   Process Button
            '--------------------------------------------------------------------------------
            '
            Button = cpCore.docProperties.getText(RequestNameButton)
            If Button = ButtonOK Then
                Exit Function
            End If
            '
            '--------------------------------------------------------------------------------
            '   Load Request
            '--------------------------------------------------------------------------------
            '
            ContentID = adminContent.Id
            ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID)
            If Button = ButtonReset Then
                Call cpCore.userProperty.setProperty(IndexConfigPrefix & CStr(ContentID), "")
            End If
            IndexConfig = LoadIndexConfig(adminContent)
            Title = adminContent.Name & " Columns"
            Description = "Use the icons to add, remove and modify your personal column prefernces for this content (" & ContentName & "). Hit OK when complete. Hit Reset to restore your column preferences for this content to the site's default column preferences."
            ToolsAction = cpCore.docProperties.getInteger("dta")
            TargetFieldID = cpCore.docProperties.getInteger("fi")
            TargetFieldName = cpCore.docProperties.getText("FieldName")
            ColumnPointer = cpCore.docProperties.getInteger("dtcn")
            FieldNameToAdd = genericController.vbUCase(cpCore.docProperties.getText(RequestNameAddField))
            FieldIDToAdd = cpCore.docProperties.getInteger(RequestNameAddFieldID)
            'ButtonList = ButtonCancel & "," & ButtonSelect
            NeedToReloadConfig = cpCore.docProperties.getBoolean("NeedToReloadConfig")
            '
            '--------------------------------------------------------------------------------
            ' Process actions
            '--------------------------------------------------------------------------------
            '
            If ContentID <> 0 Then
                CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName)
                If ToolsAction <> 0 Then
                    '
                    ' Block contentautoload, then force a load at the end
                    '
                    AllowContentAutoLoad = (cpCore.siteProperties.getBoolean("AllowContentAutoLoad", True))
                    Call cpCore.siteProperties.setProperty("AllowContentAutoLoad", False)
                    '
                    ' Make sure the FieldNameToAdd is not-inherited, if not, create new field
                    '
                    If (FieldIDToAdd <> 0) Then
                        For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In adminContent.fields
                            Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                            If field.id = FieldIDToAdd Then
                                'If CDef.fields(FieldPtr).Name = FieldNameToAdd Then
                                If field.inherited Then
                                    SourceContentID = field.contentId
                                    SourceName = field.nameLc
                                    CSSource = cpCore.db.csOpen("Content Fields", "(ContentID=" & SourceContentID & ")and(Name=" & cpCore.db.encodeSQLText(SourceName) & ")")
                                    If cpCore.db.csOk(CSSource) Then
                                        CSTarget = cpCore.db.csInsertRecord("Content Fields")
                                        If cpCore.db.csOk(CSTarget) Then
                                            Call cpCore.db.csCopyRecord(CSSource, CSTarget)
                                            Call cpCore.db.csSet(CSTarget, "ContentID", ContentID)
                                            NeedToReloadCDef = True
                                        End If
                                        Call cpCore.db.csClose(CSTarget)
                                    End If
                                    Call cpCore.db.csClose(CSSource)
                                End If
                                Exit For
                            End If
                        Next
                    End If
                    '
                    ' Make sure all fields are not-inherited, if not, create new fields
                    '
                    For Each kvp In IndexConfig.Columns
                        Dim column As indexConfigColumnClass = kvp.Value
                        Dim field As Models.Complex.CDefFieldModel = adminContent.fields(column.Name.ToLower())
                        If field.inherited Then
                            SourceContentID = field.contentId
                            SourceName = field.nameLc
                            CSSource = cpCore.db.csOpen("Content Fields", "(ContentID=" & SourceContentID & ")and(Name=" & cpCore.db.encodeSQLText(SourceName) & ")")
                            If cpCore.db.csOk(CSSource) Then
                                CSTarget = cpCore.db.csInsertRecord("Content Fields")
                                If cpCore.db.csOk(CSTarget) Then
                                    Call cpCore.db.csCopyRecord(CSSource, CSTarget)
                                    Call cpCore.db.csSet(CSTarget, "ContentID", ContentID)
                                    NeedToReloadCDef = True
                                End If
                                Call cpCore.db.csClose(CSTarget)
                            End If
                            Call cpCore.db.csClose(CSSource)
                        End If
                    Next
                    '
                    ' get current values for Processing
                    '
                    For Each kvp In IndexConfig.Columns
                        Dim column As indexConfigColumnClass = kvp.Value
                        ColumnWidthTotal += column.Width
                    Next
                    '
                    ' ----- Perform any actions first
                    '
                    Select Case ToolsAction
                        Case ToolsActionAddField
                            '
                            ' Add a field to the index form
                            '
                            If FieldIDToAdd <> 0 Then
                                Dim column As indexConfigColumnClass
                                For Each kvp In IndexConfig.Columns
                                    column = kvp.Value
                                    column.Width = CInt((column.Width * 80) / ColumnWidthTotal)
                                Next
                                column = New indexConfigColumnClass
                                CSPointer = cpCore.db.csOpenRecord("Content Fields", FieldIDToAdd, False, False)
                                If cpCore.db.csOk(CSPointer) Then
                                    column.Name = cpCore.db.csGet(CSPointer, "name")
                                    column.Width = 20
                                End If
                                Call cpCore.db.csClose(CSPointer)
                                IndexConfig.Columns.Add(column.Name.ToLower(), column)
                                NeedToReloadConfig = True
                            End If
                            '
                        Case ToolsActionRemoveField
                            '
                            ' Remove a field to the index form
                            '
                            Dim column As indexConfigColumnClass
                            If IndexConfig.Columns.ContainsKey(TargetFieldName.ToLower()) Then
                                column = IndexConfig.Columns(TargetFieldName.ToLower())
                                ColumnWidthTotal = ColumnWidthTotal + column.Width
                                IndexConfig.Columns.Remove(TargetFieldName.ToLower())
                                '
                                ' Normalize the widths of the remaining columns
                                '
                                For Each kvp In IndexConfig.Columns
                                    column = kvp.Value
                                    column.Width = CInt((1000 * column.Width) / ColumnWidthTotal)
                                Next
                                NeedToReloadConfig = True
                            End If
                        Case ToolsActionMoveFieldLeft
                            '
                            ' Move column field left
                            '
                            'If IndexConfig.Columns.Count > 1 Then
                            '    MoveNextColumn = False
                            '    For ColumnPointer = 1 To IndexConfig.Columns.Count - 1
                            '        If TargetFieldName = IndexConfig.Columns(ColumnPointer).Name Then
                            '            With IndexConfig.Columns(ColumnPointer)
                            '                FieldPointerTemp = .FieldId
                            '                NameTemp = .Name
                            '                WidthTemp = .Width
                            '                .FieldId = IndexConfig.Columns(ColumnPointer - 1).FieldId
                            '                .Name = IndexConfig.Columns(ColumnPointer - 1).Name
                            '                .Width = IndexConfig.Columns(ColumnPointer - 1).Width
                            '            End With
                            '            With IndexConfig.Columns(ColumnPointer - 1)
                            '                .FieldId = FieldPointerTemp
                            '                .Name = NameTemp
                            '                .Width = WidthTemp
                            '            End With
                            '        End If
                            '    Next
                            '    NeedToReloadConfig = True
                            'End If
                            ' end case
                        Case ToolsActionMoveFieldRight
                            '
                            ' Move Index column field right
                            '
                            'If IndexConfig.Columns.Count > 1 Then
                            '    MoveNextColumn = False
                            '    For ColumnPointer = 0 To IndexConfig.Columns.Count - 2
                            '        If TargetFieldName = IndexConfig.Columns(ColumnPointer).Name Then
                            '            With IndexConfig.Columns(ColumnPointer)
                            '                FieldPointerTemp = .FieldId
                            '                NameTemp = .Name
                            '                WidthTemp = .Width
                            '                .FieldId = IndexConfig.Columns(ColumnPointer + 1).FieldId
                            '                .Name = IndexConfig.Columns(ColumnPointer + 1).Name
                            '                .Width = IndexConfig.Columns(ColumnPointer + 1).Width
                            '            End With
                            '            With IndexConfig.Columns(ColumnPointer + 1)
                            '                .FieldId = FieldPointerTemp
                            '                .Name = NameTemp
                            '                .Width = WidthTemp
                            '            End With
                            '        End If
                            '    Next
                            '    NeedToReloadConfig = True
                            'End If
                            ' end case
                        Case ToolsActionExpand
                            '
                            ' Expand column
                            '
                            'ColumnWidthBalance = 0
                            'If IndexConfig.Columns.Count > 1 Then
                            '    '
                            '    ' Calculate the total width of the non-target columns
                            '    '
                            '    ColumnWidthIncrease = CInt(ColumnWidthTotal * 0.1)
                            '    For ColumnPointer = 0 To IndexConfig.Columns.Count - 1
                            '        If TargetFieldName <> IndexConfig.Columns(ColumnPointer).Name Then
                            '            ColumnWidthBalance = ColumnWidthBalance + IndexConfig.Columns(ColumnPointer).Width
                            '        End If
                            '    Next
                            '    '
                            '    ' Adjust all columns
                            '    '
                            '    If ColumnWidthBalance > 0 Then
                            '        For ColumnPointer = 0 To IndexConfig.Columns.Count - 1
                            '            With IndexConfig.Columns(ColumnPointer)
                            '                If TargetFieldName = .Name Then
                            '                    '
                            '                    ' Target gets 10% increase
                            '                    '
                            '                    .Width = Int(.Width + ColumnWidthIncrease)
                            '                Else
                            '                    '
                            '                    ' non-targets get their share of the shrinkage
                            '                    '
                            '                    .Width = CInt(.Width - ((ColumnWidthIncrease * .Width) / ColumnWidthBalance))
                            '                End If
                            '            End With
                            '        Next
                            '        NeedToReloadConfig = True
                            '    End If
                            'End If

                            ' end case
                        Case ToolsActionContract
                            '
                            ' Contract column
                            '
                            'ColumnWidthBalance = 0
                            'If IndexConfig.Columns.Count > 0 Then
                            '    '
                            '    ' Calculate the total width of the non-target columns
                            '    '
                            '    ColumnWidthIncrease = CInt(-(ColumnWidthTotal * 0.1))
                            '    For ColumnPointer = 0 To IndexConfig.Columns.Count - 1
                            '        With IndexConfig.Columns(ColumnPointer)
                            '            If TargetFieldName <> .Name Then
                            '                ColumnWidthBalance = ColumnWidthBalance + IndexConfig.Columns(ColumnPointer).Width
                            '            End If
                            '        End With
                            '    Next
                            '    '
                            '    ' Adjust all columns
                            '    '
                            '    If (ColumnWidthBalance > 0) And (ColumnWidthIncrease <> 0) Then
                            '        For ColumnPointer = 0 To IndexConfig.Columns.Count - 1
                            '            With IndexConfig.Columns(ColumnPointer)
                            '                If TargetFieldName = .Name Then
                            '                    '
                            '                    ' Target gets 10% increase
                            '                    '
                            '                    .Width = Int(.Width + ColumnWidthIncrease)
                            '                Else
                            '                    '
                            '                    ' non-targets get their share of the shrinkage
                            '                    '
                            '                    .Width = CInt(.Width - ((ColumnWidthIncrease * FieldWidth) / ColumnWidthBalance))
                            '                End If
                            '            End With
                            '        Next
                            '        NeedToReloadConfig = True
                            '    End If
                            'End If
                    End Select
                    '
                    ' Reload CDef if it changed
                    '
                    If NeedToReloadCDef Then
                        cpCore.doc.clearMetaData()
                        cpCore.cache.invalidateAll()
                        CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName)
                    End If
                    '
                    ' save indexconfig
                    '
                    If NeedToReloadConfig Then
                        Call SetIndexSQL_SaveIndexConfig(IndexConfig)
                        IndexConfig = LoadIndexConfig(adminContent)
                    End If
                End If
                '
                '--------------------------------------------------------------------------------
                '   Display the form
                '--------------------------------------------------------------------------------
                '
                Stream.Add("<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""99%""><tr>")
                Stream.Add("<td width=""5%"">&nbsp;</td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>10%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>20%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>30%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>40%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>50%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>60%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>70%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>80%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>90%</nobr></td>")
                Stream.Add("<td width=""9%"" align=""center"" class=""ccAdminSmall""><nobr>100%</nobr></td>")
                Stream.Add("<td width=""4%"" align=""center"">&nbsp;</td>")
                Stream.Add("</tr></table>")
                '
                Stream.Add("<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""99%""><tr>")
                Stream.Add("<td width=""9%""><nobr><img src=""/ccLib/images/black.gif"" width=""1"" height=""10"" ><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""10"" ></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><img src=""/ccLib/images/black.gif"" width=""1"" height=""10"" ><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""10"" ></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><img src=""/ccLib/images/black.gif"" width=""1"" height=""10"" ><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""10"" ></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><img src=""/ccLib/images/black.gif"" width=""1"" height=""10"" ><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""10"" ></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><img src=""/ccLib/images/black.gif"" width=""1"" height=""10"" ><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""10"" ></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><img src=""/ccLib/images/black.gif"" width=""1"" height=""10"" ><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""10"" ></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><img src=""/ccLib/images/black.gif"" width=""1"" height=""10"" ><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""10"" ></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><img src=""/ccLib/images/black.gif"" width=""1"" height=""10"" ><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""10"" ></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><img src=""/ccLib/images/black.gif"" width=""1"" height=""10"" ><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""10"" ></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><img src=""/ccLib/images/black.gif"" width=""1"" height=""10"" ><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""10"" ></nobr></td>")
                Stream.Add("<td width=""9%""><nobr><img src=""/ccLib/images/black.gif"" width=""1"" height=""10"" ><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""10"" ></nobr></td>")
                Stream.Add("</tr></table>")
                '
                ' print the column headers
                '
                ColumnWidthTotal = 0
                If IndexConfig.Columns.Count > 0 Then
                    '
                    ' Calc total width
                    '
                    For Each kvp In IndexConfig.Columns
                        Dim column As indexConfigColumnClass = kvp.Value
                        ColumnWidthTotal += column.Width
                    Next
                    If ColumnWidthTotal > 0 Then
                        Stream.Add("<table border=""0"" cellpadding=""5"" cellspacing=""0"" width=""90%"">")
                        For Each kvp In IndexConfig.Columns
                            Dim column As indexConfigColumnClass
                            column = kvp.Value
                            '
                            ' print column headers - anchored so they sort columns
                            '
                            ColumnWidth = CInt(100 * (column.Width / ColumnWidthTotal))
                            Dim field As Models.Complex.CDefFieldModel
                            field = adminContent.fields(column.Name.ToLower())
                            With field
                                fieldId = .id
                                Caption = .caption
                                If .inherited Then
                                    Caption = Caption & "*"
                                    InheritedFieldCount = InheritedFieldCount + 1
                                End If
                                AStart = "<a href=""?" & cpCore.doc.refreshQueryString & "&FieldName=" & genericController.encodeHTML(.nameLc) & "&fi=" & fieldId & "&dtcn=" & ColumnPtr & "&" & RequestNameAdminSubForm & "=" & AdminFormIndex_SubFormSetColumns
                                Call Stream.Add("<td width=""" & ColumnWidth & "%"" valign=""top"" align=""left"">" & SpanClassAdminNormal & Caption & "<br >")
                                Call Stream.Add("<img src=""/ccLib/images/black.GIF"" width=""100%"" height=""1"" >")
                                Call Stream.Add(AStart & "&dta=" & ToolsActionRemoveField & """><img src=""/ccLib/images/LibButtonDeleteUp.gif"" width=""50"" height=""15"" border=""0"" ></A><BR >")
                                Call Stream.Add(AStart & "&dta=" & ToolsActionMoveFieldRight & """><img src=""/ccLib/images/LibButtonMoveRightUp.gif"" width=""50"" height=""15"" border=""0"" ></A><BR >")
                                Call Stream.Add(AStart & "&dta=" & ToolsActionMoveFieldLeft & """><img src=""/ccLib/images/LibButtonMoveLeftUp.gif"" width=""50"" height=""15"" border=""0"" ></A><BR >")
                                'Call Stream.Add(AStart & "&dta=" & ToolsActionSetAZ & """><img src=""/ccLib/images/LibButtonSortazUp.gif"" width=""50"" height=""15"" border=""0"" ></A><BR >")
                                'Call Stream.Add(AStart & "&dta=" & ToolsActionSetZA & """><img src=""/ccLib/images/LibButtonSortzaUp.gif"" width=""50"" height=""15"" border=""0"" ></A><BR >")
                                Call Stream.Add(AStart & "&dta=" & ToolsActionExpand & """><img src=""/ccLib/images/LibButtonOpenUp.gif"" width=""50"" height=""15"" border=""0"" ></A><BR >")
                                Call Stream.Add(AStart & "&dta=" & ToolsActionContract & """><img src=""/ccLib/images/LibButtonCloseUp.gif"" width=""50"" height=""15"" border=""0"" ></A>")
                                Call Stream.Add("</span></td>")
                            End With
                        Next
                        Stream.Add("</tr>")
                        Stream.Add("</table>")
                    End If
                End If
                '
                ' ----- If anything was inherited, put up the message
                '
                If InheritedFieldCount > 0 Then
                    Call Stream.Add("<p class=""ccNormal"">* This field was inherited from the Content Definition's Parent. Inherited fields will automatically change when the field in the parent is changed. If you alter these settings, this connection will be broken, and the field will no longer inherit it's properties.</P class=""ccNormal"">")
                End If
                '
                ' ----- now output a list of fields to add
                '
                If CDef.fields.Count = 0 Then
                    Stream.Add(SpanClassAdminNormal & "This Content Definition has no fields</span><br>")
                Else
                    Stream.Add(SpanClassAdminNormal & "<br>")
                    For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In adminContent.fields
                        Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                        With field
                            '
                            ' display the column if it is not in use
                            '
                            If Not IndexConfig.Columns.ContainsKey(field.nameLc) Then
                                If False Then
                                    ' this causes more problems then it fixes
                                    'If Not .Authorable Then
                                    '
                                    ' not authorable
                                    '
                                    Stream.Add("<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""50"" height=""15"" border=""0"" > " & .caption & " (not authorable field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdFile) Then
                                    '
                                    ' file can not be search
                                    '
                                    Stream.Add("<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""50"" height=""15"" border=""0"" > " & .caption & " (file field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdFileText) Then
                                    '
                                    ' filename can not be search
                                    '
                                    Stream.Add("<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""50"" height=""15"" border=""0"" > " & .caption & " (text file field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdFileHTML) Then
                                    '
                                    ' filename can not be search
                                    '
                                    Stream.Add("<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""50"" height=""15"" border=""0"" > " & .caption & " (html file field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdFileCSS) Then
                                    '
                                    ' css filename can not be search
                                    '
                                    Stream.Add("<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""50"" height=""15"" border=""0"" > " & .caption & " (css file field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdFileXML) Then
                                    '
                                    ' xml filename can not be search
                                    '
                                    Stream.Add("<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""50"" height=""15"" border=""0"" > " & .caption & " (xml file field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdFileJavascript) Then
                                    '
                                    ' javascript filename can not be search
                                    '
                                    Stream.Add("<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""50"" height=""15"" border=""0"" > " & .caption & " (javascript file field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdLongText) Then
                                    '
                                    ' long text can not be search
                                    '
                                    Stream.Add("<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""50"" height=""15"" border=""0"" > " & .caption & " (long text field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdHTML) Then
                                    '
                                    ' long text can not be search
                                    '
                                    Stream.Add("<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""50"" height=""15"" border=""0"" > " & .caption & " (long text field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdFileImage) Then
                                    '
                                    ' long text can not be search
                                    '
                                    Stream.Add("<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""50"" height=""15"" border=""0"" > " & .caption & " (image field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdRedirect) Then
                                    '
                                    ' long text can not be search
                                    '
                                    Stream.Add("<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""50"" height=""15"" border=""0"" > " & .caption & " (redirect field)<br>")
                                ElseIf (.fieldTypeId = FieldTypeIdManyToMany) Then
                                    '
                                    ' many to many can not be search
                                    '
                                    Stream.Add("<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""50"" height=""15"" border=""0"" > " & .caption & " (many-to-many field)<br>")
                                Else
                                    '
                                    ' can be used as column header
                                    '
                                    Stream.Add("<a href=""?" & cpCore.doc.refreshQueryString & "&fi=" & .id & "&dta=" & ToolsActionAddField & "&" & RequestNameAddFieldID & "=" & .id & "&" & RequestNameAdminSubForm & "=" & AdminFormIndex_SubFormSetColumns & """><img src=""/ccLib/images/LibButtonAddUp.gif"" width=""50"" height=""15"" border=""0"" ></A> " & .caption & "<br>")
                                End If
                            End If
                        End With
                    Next
                End If
            End If
            '
            '--------------------------------------------------------------------------------
            ' print the content tables that have index forms to Configure
            '--------------------------------------------------------------------------------
            '
            'FormPanel = FormPanel & SpanClassAdminNormal & "Select a Content Definition to Configure its index form<br >"
            ''FormPanel = FormPanel & cpCore.main_GetFormInputHidden("af", AdminFormToolConfigureIndex)
            'FormPanel = FormPanel & cpCore.htmldoc.main_GetFormInputSelect2("ContentID", ContentID, "Content")
            'Call Stream.Add(cpcore.htmldoc.main_GetPanel(FormPanel))
            ''
            Call cpCore.siteProperties.setProperty("AllowContentAutoLoad", genericController.encodeText(AllowContentAutoLoad))
            'Stream.Add( cpCore.main_GetFormInputHidden("NeedToReloadConfig", NeedToReloadConfig))

            Content = "" _
                & Stream.Text _
                & cpCore.html.html_GetFormInputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormSetColumns) _
                & ""
            GetForm_Index_SetColumns = Adminui.GetBody(Title, ButtonOK & "," & ButtonReset, "", False, False, Description, "", 10, Content)
            '
            '
            '    ButtonBar = AdminUI.GetButtonsFromList( ButtonList, True, True, "button")
            '    ButtonBar = AdminUI.GetButtonBar(ButtonBar, "")
            '    Stream = New FastStringClass
            ''
            ''    GetForm_Index_SetColumns = "" _
            ''        & ButtonBar _
            ''        & AdminUI.EditTableOpen _
            ''        & Stream.Text _
            ''        & AdminUI.EditTableClose _
            ''        & ButtonBar _
            '    '
            '    '
            '    ' Assemble LiveWindowTable
            '    '
            '    Stream.Add( OpenLiveWindowTable)
            '    Stream.Add( vbCrLf & cpCore.main_GetFormStart()
            '    Stream.Add( ButtonBar)
            '    Stream.Add( TitleBar)
            '    Stream.Add( Content)
            '    Stream.Add( ButtonBar)
            '    Stream.Add( "<input type=hidden name=asf VALUE=" & AdminFormIndex_SubFormSetColumns & ">")
            '    Stream.Add( "</form>")
            '    Stream.Add( CloseLiveWindowTable)
            '    '
            '    GetForm_Index_SetColumns = Stream.Text
            Call cpCore.html.addTitle(Title)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Index_SetColumns")
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Sub TurnOnLinkAlias(ByVal UseContentWatchLink As Boolean)
            On Error GoTo ErrorTrap  ''Dim th as integer : th = profileLogAdminMethodEnter("TurnOnLinkAlias")
            '
            Dim CS As Integer
            Dim ErrorList As String
            Dim linkAlias As String
            '
            If (cpCore.doc.debug_iUserError <> "") Then
                Call errorController.error_AddUserError(cpCore, "Existing pages could not be checked for Link Alias names because there was another error on this page. Correct this error, and turn Link Alias on again to rerun the verification.")
            Else
                CS = cpCore.db.csOpen("Page Content")
                Do While cpCore.db.csOk(CS)
                    '
                    ' Add the link alias
                    '
                    linkAlias = cpCore.db.csGetText(CS, "LinkAlias")
                    If linkAlias <> "" Then
                        '
                        ' Add the link alias
                        '
                        Call docController.addLinkAlias(cpCore, linkAlias, cpCore.db.csGetInteger(CS, "ID"), "", False, True)
                    Else
                        '
                        ' Add the name
                        '
                        linkAlias = cpCore.db.csGetText(CS, "name")
                        If linkAlias <> "" Then
                            Call docController.addLinkAlias(cpCore, linkAlias, cpCore.db.csGetInteger(CS, "ID"), "", False, False)
                        End If
                    End If
                    '
                    Call cpCore.db.csGoNext(CS)
                Loop
                Call cpCore.db.csClose(CS)
                If (cpCore.doc.debug_iUserError <> "") Then
                    '
                    ' Throw out all the details of what happened, and add one simple error
                    '
                    ErrorList = errorController.error_GetUserError(cpCore)
                    ErrorList = genericController.vbReplace(ErrorList, UserErrorHeadline, "", 1, 99, vbTextCompare)
                    Call errorController.error_AddUserError(cpCore, "The following errors occurred while verifying Link Alias entries for your existing pages." & ErrorList)
                    'Call cpCore.htmldoc.main_AddUserError(ErrorList)
                End If
            End If
            '
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("TurnOnLinkAlias")
        End Sub
        '
        '========================================================================
        '   Editor features are stored in the \config\EditorFeatures.txt file
        '   This is a crlf delimited list, with each row including:
        '       admin:featurelist
        '       contentmanager:featurelist
        '       public:featurelist
        '========================================================================
        '
        Private Function GetForm_EditConfig() As String
            On Error GoTo ErrorTrap  ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_EditConfig")
            '
            Dim CS As Integer
            Dim EditorStyleRulesFilename As String
            Dim Pos As Integer
            Dim SrcPtr As Integer
            Dim FeatureDetails() As String
            Dim AllowAdmin As Boolean
            Dim AllowCM As Boolean
            Dim AllowPublic As Boolean
            Dim RowPtr As Integer
            Dim AdminList As String = ""
            Dim CMList As String = ""
            Dim PublicList As String = ""
            Dim TDLeft As String
            Dim TDCenter As String
            Dim Ptr As Integer
            Dim Content As New stringBuilderLegacyController
            Dim Button As String
            Dim Copy As String
            Dim ButtonList As String
            Dim Adminui As New adminUIController(cpCore)
            Dim Caption As String
            Dim Description As String
            Dim StyleSN As Integer
            Dim TBConfig As String
            Dim TBArray() As String
            Dim DefaultFeatures() As String
            Dim FeatureName As String
            Dim FeatureList As String
            Dim Features() As String
            '
            DefaultFeatures = Split(InnovaEditorFeatureList, ",")
            Description = "This tool is used to configure the wysiwyg content editor for different uses. Check the Administrator column if you want administrators to have access to this feature when editing a page. Check the Content Manager column to allow non-admins to have access to this feature. Check the Public column if you want those on the public site to have access to the feature when the editor is used for public forms."
            '
            Button = cpCore.docProperties.getText(RequestNameButton)
            If Button = ButtonCancel Then
                '
                ' Cancel button pressed, return with nothing goes to root form
                '
                'Call cpCore.main_Redirect2(cpCore.app.SiteProperty_AdminURL, "EditConfig, Cancel Button Pressed")
            Else
                '
                ' From here down will return a form
                '
                If Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                    '
                    ' Does not have permission
                    '
                    ButtonList = ButtonCancel
                    Content.Add(Adminui.GetFormBodyAdminOnly())
                    Call cpCore.html.addTitle("Style Editor")
                    GetForm_EditConfig = Adminui.GetBody("Site Styles", ButtonList, "", True, True, Description, "", 0, Content.Text)
                Else
                    '
                    ' OK to see and use this form
                    '
                    If Button = ButtonSave Or Button = ButtonOK Then
                        '
                        ' Save the Previous edits
                        '
                        Call cpCore.siteProperties.setProperty("Editor Background Color", cpCore.docProperties.getText("editorbackgroundcolor"))
                        '
                        For Ptr = 0 To UBound(DefaultFeatures)
                            FeatureName = DefaultFeatures(Ptr)
                            If genericController.vbLCase(FeatureName) = "styleandformatting" Then
                                '
                                ' must always be on or it throws js error (editor bug I guess)
                                '
                                AdminList = AdminList & "," & FeatureName
                                CMList = CMList & "," & FeatureName
                                PublicList = PublicList & "," & FeatureName
                            Else
                                If cpCore.docProperties.getBoolean(FeatureName & ".admin") Then
                                    AdminList = AdminList & "," & FeatureName
                                End If
                                If cpCore.docProperties.getBoolean(FeatureName & ".cm") Then
                                    CMList = CMList & "," & FeatureName
                                End If
                                If cpCore.docProperties.getBoolean(FeatureName & ".public") Then
                                    PublicList = PublicList & "," & FeatureName
                                End If
                            End If
                        Next
                        Call cpCore.privateFiles.saveFile(InnovaEditorFeaturefilename, "admin:" & AdminList & vbCrLf & "contentmanager:" & CMList & vbCrLf & "public:" & PublicList)
                        '
                        ' Clear the editor style rules template cache so next edit gets new background color
                        '
                        EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", "0", 1, 99, vbTextCompare)
                        Call cpCore.privateFiles.deleteFile(EditorStyleRulesFilename)
                        '
                        CS = cpCore.db.csOpenSql_rev("default", "select id from cctemplates")
                        Do While cpCore.db.csOk(CS)
                            EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", cpCore.db.csGet(CS, "ID"), 1, 99, vbTextCompare)
                            Call cpCore.privateFiles.deleteFile(EditorStyleRulesFilename)
                            Call cpCore.db.csGoNext(CS)
                        Loop
                        Call cpCore.db.csClose(CS)

                    End If
                    '
                    If Button = ButtonOK Then
                        '
                        ' exit with blank page
                        '
                    Else
                        '
                        ' Draw the form
                        '
                        FeatureList = cpCore.cdnFiles.readFile(InnovaEditorFeaturefilename)
                        'If FeatureList = "" Then
                        '    FeatureList = cpCore.cluster.localClusterFiles.readFile("ccLib\" & "Config\DefaultEditorConfig.txt")
                        '    Call cpCore.privateFiles.saveFile(InnovaEditorFeaturefilename, FeatureList)
                        'End If
                        If FeatureList = "" Then
                            FeatureList = "admin:" & InnovaEditorFeatureList & vbCrLf & "contentmanager:" & InnovaEditorFeatureList & vbCrLf & "public:" & InnovaEditorPublicFeatureList
                        End If
                        If FeatureList <> "" Then
                            Features = Split(FeatureList, vbCrLf)
                            AdminList = genericController.vbReplace(Features(0), "admin:", "", 1, 99, vbTextCompare)
                            If UBound(Features) > 0 Then
                                CMList = genericController.vbReplace(Features(1), "contentmanager:", "", 1, 99, vbTextCompare)
                                If UBound(Features) > 1 Then
                                    PublicList = genericController.vbReplace(Features(2), "public:", "", 1, 99, vbTextCompare)
                                End If
                            End If
                        End If
                        '
                        Copy = vbCrLf _
                            & "<tr class=""ccAdminListCaption"">" _
                            & "<td align=left style=""width:200;"">Feature</td>" _
                            & "<td align=center style=""width:100;"">Administrators</td>" _
                            & "<td align=center style=""width:100;"">Content&nbsp;Managers</td>" _
                            & "<td align=center style=""width:100;"">Public</td>" _
                            & "</tr>"
                        RowPtr = 0
                        For Ptr = 0 To UBound(DefaultFeatures)
                            FeatureName = DefaultFeatures(Ptr)
                            If genericController.vbLCase(FeatureName) = "styleandformatting" Then
                                '
                                ' hide and force on during process - editor bug I think.
                                '
                            Else
                                TDLeft = genericController.StartTableCell(, , CBool(RowPtr Mod 2), "left")
                                TDCenter = genericController.StartTableCell(, , CBool(RowPtr Mod 2), "center")
                                AllowAdmin = genericController.EncodeBoolean(InStr(1, "," & AdminList & ",", "," & FeatureName & ",", vbTextCompare))
                                AllowCM = genericController.EncodeBoolean(InStr(1, "," & CMList & ",", "," & FeatureName & ",", vbTextCompare))
                                AllowPublic = genericController.EncodeBoolean(InStr(1, "," & PublicList & ",", "," & FeatureName & ",", vbTextCompare))
                                Copy = Copy & vbCrLf _
                                    & "<tr>" _
                                    & TDLeft & FeatureName & "</td>" _
                                    & TDCenter & cpCore.html.html_GetFormInputCheckBox2(FeatureName & ".admin", AllowAdmin) & "</td>" _
                                    & TDCenter & cpCore.html.html_GetFormInputCheckBox2(FeatureName & ".cm", AllowCM) & "</td>" _
                                    & TDCenter & cpCore.html.html_GetFormInputCheckBox2(FeatureName & ".public", AllowPublic) & "</td>" _
                                    & "</tr>"
                                RowPtr = RowPtr + 1
                            End If
                        Next
                        Copy = "" _
                            & vbCrLf & "<div><b>body background style color</b> (default='white')</div>" _
                            & vbCrLf & "<div>" & cpCore.html.html_GetFormInputText2("editorbackgroundcolor", cpCore.siteProperties.getText("Editor Background Color", "white")) & "</div>" _
                            & vbCrLf & "<div>&nbsp;</div>" _
                            & vbCrLf & "<div><b>Toolbar features available</b></div>" _
                            & vbCrLf & "<table border=""0"" cellpadding=""4"" cellspacing=""0"" width=""500px"" align=left>" & genericController.htmlIndent(Copy) & vbCrLf & kmaEndTable
                        Copy = vbCrLf & genericController.StartTable(20, 0, 0) & "<tr><td>" & genericController.htmlIndent(Copy) & "</td></tr>" & vbCrLf & kmaEndTable
                        Content.Add(Copy)
                        ButtonList = ButtonCancel & "," & ButtonRefresh & "," & ButtonSave & "," & ButtonOK
                        Content.Add(cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormEditorConfig))
                        Call cpCore.html.addTitle("Editor Settings")
                        GetForm_EditConfig = Adminui.GetBody("Editor Configuration", ButtonList, "", True, True, Description, "", 0, Content.Text)
                    End If
                End If
                '
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_EditConfig")
            '
        End Function
        '
        '========================================================================
        ' Page Content Settings Page
        '========================================================================
        '
        Private Function GetForm_BuildCollection() As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogAdminMethodEnter( "GetForm_BuildCollection")
            '
            Dim Description As String
            Dim Content As New stringBuilderLegacyController
            Dim Button As String
            Dim Adminui As New adminUIController(cpCore)
            Dim ButtonList As String
            Dim AllowAutoLogin As Boolean
            Dim Copy As String
            '
            Button = cpCore.docProperties.getText(RequestNameButton)
            If Button = ButtonCancel Then
                '
                ' Cancel just exits with no content
                '
                Exit Function
            ElseIf Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                '
                ' Not Admin Error
                '
                ButtonList = ButtonCancel
                Content.Add(Adminui.GetFormBodyAdminOnly())
            Else
                Content.Add(Adminui.EditTableOpen)
                '
                ' Set defaults
                '
                AllowAutoLogin = (cpCore.siteProperties.getBoolean("AllowAutoLogin", True))
                '
                ' Process Requests
                '
                Select Case Button
                    Case ButtonSave, ButtonOK
                        '
                        '
                        '
                        AllowAutoLogin = cpCore.docProperties.getBoolean("AllowAutoLogin")
                        '
                        Call cpCore.siteProperties.setProperty("AllowAutoLogin", genericController.encodeText(AllowAutoLogin))
                End Select
                If (Button = ButtonOK) Then
                    '
                    ' Exit on OK or cancel
                    '
                    Exit Function
                End If
                '
                ' List Add-ons to include
                '

                Copy = cpCore.html.html_GetFormInputCheckBox2("AllowAutoLogin", AllowAutoLogin)
                Copy = Copy _
            & "<div>When checked, returning users are automatically logged-in, without requiring a username or password. This is very convenient, but creates a high security risk. Each time you login, you will be given the option to not allow Auto-Login from that computer.</div>"
                Call Content.Add(Adminui.GetEditRow(Copy, "Allow Auto Login", "", False, False, ""))
                '
                ' Buttons
                '
                ButtonList = ButtonCancel & "," & ButtonSave & "," & ButtonOK
                '
                ' Close Tables
                '
                Content.Add(Adminui.EditTableClose)
                Content.Add(cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormBuilderCollection))
            End If
            '
            Description = "Use this tool to modify the site security settings"
            GetForm_BuildCollection = Adminui.GetBody("Security Settings", ButtonList, "", True, True, Description, "", 0, Content.Text)
            Content = Nothing
            '
            '''Dim th as integer: Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Content = Nothing
            Call handleLegacyClassError3("GetForm_BuildCollection")
            '
        End Function



    End Class
End Namespace
