
Option Explicit On
Option Strict On
'
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Imports System.Xml
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
'
Namespace Contensive.Addons.AdminSite
    Partial Public Class getAdminSiteClass
        Inherits Contensive.BaseClasses.AddonBaseClass
        '
        '========================================================================
        '   Print the index form, values and all
        '       creates a sql with leftjoins, and renames lookups as TableLookupxName
        '       where x is the TarGetFieldPtr of the field that is FieldTypeLookup
        '
        '   Input:
        '       AdminContent.contenttablename is required
        '       OrderByFieldPtr
        '       OrderByDirection
        '       RecordTop
        '       RecordsPerPage
        '       Findstring( ColumnPointer )
        '========================================================================
        '
        Private Function GetForm_Index(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, ByVal IsEmailContent As Boolean) As String
            Dim returnForm As String = ""
            Try
                Const FilterClosedLabel = "<div style=""font-size:9px;text-align:center;"">&nbsp;<br>F<br>i<br>l<br>t<br>e<br>r<br>s</div>"
                '
                Dim Copy As String = ""
                Dim RightCopy As String
                Dim TitleRows As Integer
                ' refactor -- is was using page manager code, and it only detected if the page is the current domain's 
                'Dim LandingPageID As Integer
                'Dim IsPageContent As Boolean
                'Dim IsLandingPage As Boolean
                Dim PageCount As Integer
                Dim AllowAdd As Boolean
                Dim AllowDelete As Boolean
                Dim recordCnt As Integer
                Dim AllowAccessToContent As Boolean
                Dim ContentName As String
                Dim ContentAccessLimitMessage As String = ""
                Dim IsLimitedToSubContent As Boolean
                Dim GroupList As String = ""
                Dim Groups() As String
                Dim FieldCaption As String
                Dim SubTitle As String
                Dim SubTitlePart As String
                Dim Title As String
                Dim AjaxQS As String
                Dim FilterColumn As String = ""
                Dim DataColumn As String
                Dim DataTable_DataRows As String = ""
                Dim FilterDataTable As String = ""
                Dim DataTable_FindRow As String = ""
                Dim DataTable As String
                Dim DataTable_HdrRow As String = ""
                Dim IndexFilterContent As String = ""
                Dim IndexFilterHead As String = ""
                Dim IndexFilterJS As String = ""
                Dim IndexFilterOpen As Boolean
                Dim IndexConfig As indexConfigClass
                Dim Ptr As Integer
                Dim SortTitle As String
                Dim HeaderDescription As String = ""
                Dim AllowFilterNav As Boolean
                Dim ColumnPointer As Integer
                Dim WhereCount As Integer
                Dim sqlWhere As String = ""
                Dim sqlOrderBy As String = ""
                Dim sqlFieldList As String = ""
                Dim sqlFrom As String = ""
                Dim CS As Integer
                Dim SQL As String
                Dim RowColor As String = ""
                Dim RecordPointer As Integer
                Dim RecordLast As Integer
                Dim RecordTop_NextPage As Integer
                Dim RecordTop_PreviousPage As Integer
                Dim ColumnWidth As Integer
                Dim ButtonBar As String
                Dim TitleBar As String
                Dim FindWordValue As String
                Dim ButtonObject As String
                Dim ButtonFace As String
                Dim ButtonHref As String
                Dim URI As String
                'Dim DataSourceName As String
                'Dim DataSourceType As Integer
                Dim FieldName As String
                Dim FieldUsedInColumns As New Dictionary(Of String, Boolean)                 ' used to prevent select SQL from being sorted by a field that does not appear
                Dim ColumnWidthTotal As Integer
                Dim SubForm As Integer
                Dim Stream As New stringBuilderLegacyController
                Dim RecordID As Integer
                Dim RecordName As String
                Dim LeftButtons As String = ""
                Dim RightButtons As String = ""
                Dim Adminui As New adminUIController(cpCore)
                Dim IsLookupFieldValid As New Dictionary(Of String, Boolean)
                Dim allowCMEdit As Boolean
                Dim allowCMAdd As Boolean
                Dim allowCMDelete As Boolean
                '
                ' --- make sure required fields are present
                '
                If adminContent.Id = 0 Then
                    '
                    ' Bad content id
                    '
                    Stream.Add(GetForm_Error(
                        "This form requires a valid content definition, and one was not found for content ID [" & adminContent.Id & "]." _
                        , "No content definition was specified [ContentID=0]. Please contact your application developer for more assistance."
                        ))
                ElseIf adminContent.Name = "" Then
                    '
                    ' Bad content name
                    '
                    Stream.Add(GetForm_Error(
                        "No content definition could be found for ContentID [" & adminContent.Id & "]. This could be a menu error. Please contact your application developer for more assistance." _
                        , "No content definition for ContentID [" & adminContent.Id & "] could be found."
                        ))
                ElseIf adminContent.ContentTableName = "" Then
                    '
                    ' No tablename
                    '
                    Stream.Add(GetForm_Error(
                        "The content definition [" & adminContent.Name & "] is not associated with a valid database table. Please contact your application developer for more assistance." _
                        , "Content [" & adminContent.Name & "] ContentTablename is empty."
                        ))
                ElseIf adminContent.fields.Count = 0 Then
                    '
                    ' No Fields
                    '
                    Stream.Add(GetForm_Error(
                        "This content [" & adminContent.Name & "] cannot be accessed because it has no fields. Please contact your application developer for more assistance." _
                        , "Content [" & adminContent.Name & "] has no field records."
                        ))
                ElseIf (adminContent.DeveloperOnly And (Not cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore))) Then
                    '
                    ' Developer Content and not developer
                    '
                    Stream.Add(GetForm_Error(
                        "Access to this content [" & adminContent.Name & "] requires developer permissions. Please contact your application developer for more assistance." _
                        , "Content [" & adminContent.Name & "] has no field records."
                        ))
                Else
                    Dim datasource As Models.Entity.dataSourceModel = Models.Entity.dataSourceModel.create(cpCore, adminContent.dataSourceId, New List(Of String))
                    '
                    ' get access rights
                    '
                    Call cpCore.doc.authContext.getContentAccessRights(cpCore, adminContent.Name, allowCMEdit, allowCMAdd, allowCMDelete)
                    '
                    ' detemine which subform to disaply
                    '
                    SubForm = cpCore.docProperties.getInteger(RequestNameAdminSubForm)
                    If SubForm <> 0 Then
                        Select Case SubForm
                            Case AdminFormIndex_SubFormExport
                                Copy = GetForm_Index_Export(adminContent, editRecord)
                            Case AdminFormIndex_SubFormSetColumns
                                Copy = GetForm_Index_SetColumns(adminContent, editRecord)
                            Case AdminFormIndex_SubFormAdvancedSearch
                                Copy = GetForm_Index_AdvancedSearch(adminContent, editRecord)
                        End Select
                    End If
                    Call Stream.Add(Copy)
                    If Copy = "" Then
                        '
                        ' If subforms return empty, go to parent form
                        '
                        AllowFilterNav = True
                        '
                        ' -- Load Index page customizations
                        IndexConfig = LoadIndexConfig(adminContent)
                        Call SetIndexSQL_ProcessIndexConfigRequests(adminContent, editRecord, IndexConfig)
                        Call SetIndexSQL_SaveIndexConfig(IndexConfig)
                        '
                        ' Get the SQL parts
                        '
                        Call SetIndexSQL(adminContent, editRecord, IndexConfig, AllowAccessToContent, sqlFieldList, sqlFrom, sqlWhere, sqlOrderBy, IsLimitedToSubContent, ContentAccessLimitMessage, FieldUsedInColumns, IsLookupFieldValid)
                        If (Not allowCMEdit) Or (Not AllowAccessToContent) Then
                            '
                            ' two conditions should be the same -- but not time to check - This user does not have access to this content
                            '
                            Call errorController.error_AddUserError(cpCore, "Your account does not have access to any records in '" & adminContent.Name & "'.")
                        Else
                            '
                            ' Get the total record count
                            '
                            SQL = "select count(" & adminContent.ContentTableName & ".ID) as cnt from " & sqlFrom
                            If sqlWhere <> "" Then
                                SQL &= " where " & sqlWhere
                            End If
                            CS = cpCore.db.csOpenSql_rev(datasource.Name, SQL)
                            If cpCore.db.csOk(CS) Then
                                recordCnt = cpCore.db.csGetInteger(CS, "cnt")
                            End If
                            Call cpCore.db.csClose(CS)
                            '
                            ' Assumble the SQL
                            '
                            SQL = "select"
                            If datasource.type <> DataSourceTypeODBCMySQL Then
                                SQL &= " Top " & (IndexConfig.RecordTop + IndexConfig.RecordsPerPage)
                            End If
                            SQL &= " " & sqlFieldList & " From " & sqlFrom
                            If sqlWhere <> "" Then
                                SQL &= " WHERE " & sqlWhere
                            End If
                            If sqlOrderBy <> "" Then
                                SQL &= " Order By" & sqlOrderBy
                            End If
                            If datasource.type = DataSourceTypeODBCMySQL Then
                                SQL &= " Limit " & (IndexConfig.RecordTop + IndexConfig.RecordsPerPage)
                            End If
                            '
                            ' Refresh Query String
                            '
                            Call cpCore.doc.addRefreshQueryString("tr", IndexConfig.RecordTop.ToString())
                            Call cpCore.doc.addRefreshQueryString("asf", AdminForm.ToString())
                            Call cpCore.doc.addRefreshQueryString("cid", adminContent.Id.ToString())
                            Call cpCore.doc.addRefreshQueryString(RequestNameTitleExtension, genericController.EncodeRequestVariable(TitleExtension))
                            If WherePairCount > 0 Then
                                For WhereCount = 0 To WherePairCount - 1
                                    Call cpCore.doc.addRefreshQueryString("wl" & WhereCount, WherePair(0, WhereCount))
                                    Call cpCore.doc.addRefreshQueryString("wr" & WhereCount, WherePair(1, WhereCount))
                                Next
                            End If
                            '
                            ' ----- ButtonBar
                            '
                            AllowAdd = adminContent.AllowAdd And (Not IsLimitedToSubContent) And (allowCMAdd)
                            If MenuDepth > 0 Then
                                LeftButtons = LeftButtons & cpCore.html.html_GetFormButton(ButtonClose, , , "window.close();")
                            Else
                                LeftButtons = LeftButtons & cpCore.html.html_GetFormButton(ButtonCancel)
                                'LeftButtons = LeftButtons & cpCore.main_GetFormButton(ButtonCancel, , , "return processSubmit(this)")
                            End If
                            If AllowAdd Then
                                LeftButtons = LeftButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonAdd & """>"
                                'LeftButtons = LeftButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonAdd & """ onClick=""return processSubmit(this);"">"
                            Else
                                LeftButtons = LeftButtons & "<input TYPE=SUBMIT NAME=BUTTON DISABLED VALUE=""" & ButtonAdd & """>"
                                'LeftButtons = LeftButtons & "<input TYPE=SUBMIT NAME=BUTTON DISABLED VALUE=""" & ButtonAdd & """ onClick=""return processSubmit(this);"">"
                            End If
                            AllowDelete = (adminContent.AllowDelete) And (allowCMDelete)
                            If AllowDelete Then
                                LeftButtons = LeftButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonDelete & """ onClick=""if(!DeleteCheck())return false;"">"
                            Else
                                LeftButtons = LeftButtons & "<input TYPE=SUBMIT NAME=BUTTON DISABLED VALUE=""" & ButtonDelete & """ onClick=""if(!DeleteCheck())return false;"">"
                            End If
                            If IndexConfig.PageNumber = 1 Then
                                RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonFirst & """ DISABLED>"
                                RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonPrevious & """ DISABLED>"
                            Else
                                RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonFirst & """>"
                                'RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonFirst & """ onClick=""return processSubmit(this);"">"
                                RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonPrevious & """>"
                                'RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonPrevious & """ onClick=""return processSubmit(this);"">"
                            End If
                            'RightButtons = RightButtons & cpCore.main_GetFormButton(ButtonFirst)
                            'RightButtons = RightButtons & cpCore.main_GetFormButton(ButtonPrevious)
                            If recordCnt > (IndexConfig.PageNumber * IndexConfig.RecordsPerPage) Then
                                RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonNext & """>"
                                'RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonNext & """ onClick=""return processSubmit(this);"">"
                            Else
                                RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonNext & """ DISABLED>"
                            End If
                            RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonRefresh & """>"
                            If recordCnt <= 1 Then
                                PageCount = 1
                            Else
                                PageCount = CInt(1 + Int((recordCnt - 1) / IndexConfig.RecordsPerPage))
                            End If
                            ButtonBar = Adminui.GetButtonBarForIndex(LeftButtons, RightButtons, IndexConfig.PageNumber, IndexConfig.RecordsPerPage, PageCount)
                            'ButtonBar = AdminUI.GetButtonBar(LeftButtons, RightButtons)
                            '
                            ' ----- TitleBar
                            '
                            Title = ""
                            SubTitle = ""
                            SubTitlePart = ""
                            With IndexConfig
                                If .ActiveOnly Then
                                    SubTitle = SubTitle & ", active records"
                                End If
                                SubTitlePart = ""
                                If .LastEditedByMe Then
                                    SubTitlePart = SubTitlePart & " by " & cpCore.doc.authContext.user.name
                                End If
                                If .LastEditedPast30Days Then
                                    SubTitlePart = SubTitlePart & " in the past 30 days"
                                End If
                                If .LastEditedPast7Days Then
                                    SubTitlePart = SubTitlePart & " in the week"
                                End If
                                If .LastEditedToday Then
                                    SubTitlePart = SubTitlePart & " today"
                                End If
                                If SubTitlePart <> "" Then
                                    SubTitle = SubTitle & ", last edited" & SubTitlePart
                                End If
                                For Each kvp In .FindWords
                                    Dim findWord As indexConfigFindWordClass = kvp.Value
                                    If Not String.IsNullOrEmpty(findWord.Name) Then
                                        FieldCaption = genericController.encodeText(Models.Complex.cdefModel.GetContentFieldProperty(cpCore, adminContent.Name, findWord.Name, "caption"))
                                        Select Case findWord.MatchOption
                                            Case FindWordMatchEnum.MatchEmpty
                                                SubTitle = SubTitle & ", " & FieldCaption & " is empty"
                                            Case FindWordMatchEnum.MatchEquals
                                                SubTitle = SubTitle & ", " & FieldCaption & " = '" & findWord.Value & "'"
                                            Case FindWordMatchEnum.MatchFalse
                                                SubTitle = SubTitle & ", " & FieldCaption & " is false"
                                            Case FindWordMatchEnum.MatchGreaterThan
                                                SubTitle = SubTitle & ", " & FieldCaption & " &gt; '" & findWord.Value & "'"
                                            Case FindWordMatchEnum.matchincludes
                                                SubTitle = SubTitle & ", " & FieldCaption & " includes '" & findWord.Value & "'"
                                            Case FindWordMatchEnum.MatchLessThan
                                                SubTitle = SubTitle & ", " & FieldCaption & " &lt; '" & findWord.Value & "'"
                                            Case FindWordMatchEnum.MatchNotEmpty
                                                SubTitle = SubTitle & ", " & FieldCaption & " is not empty"
                                            Case FindWordMatchEnum.MatchTrue
                                                SubTitle = SubTitle & ", " & FieldCaption & " is true"
                                        End Select

                                    End If
                                Next
                                If .SubCDefID > 0 Then
                                    ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, .SubCDefID)
                                    If ContentName <> "" Then
                                        SubTitle = SubTitle & ", in Sub-content '" & ContentName & "'"
                                    End If
                                End If
                                '
                                ' add groups to caption
                                '
                                If (LCase(adminContent.ContentTableName) = "ccmembers") And (.GroupListCnt > 0) Then
                                    'If (LCase(AdminContent.ContentTableName) = "ccmembers") And (.GroupListCnt > 0) Then
                                    SubTitlePart = ""
                                    For Ptr = 0 To .GroupListCnt - 1
                                        If .GroupList(Ptr) <> "" Then
                                            GroupList = GroupList & vbTab & .GroupList(Ptr)
                                        End If
                                    Next
                                    If GroupList <> "" Then
                                        Groups = Split(Mid(GroupList, 2), vbTab)
                                        If UBound(Groups) = 0 Then
                                            SubTitle = SubTitle & ", in group '" & Groups(0) & "'"
                                        ElseIf UBound(Groups) = 1 Then
                                            SubTitle = SubTitle & ", in groups '" & Groups(0) & "' and '" & Groups(1) & "'"
                                        Else
                                            For Ptr = 0 To UBound(Groups) - 1
                                                SubTitlePart = SubTitlePart & ", '" & Groups(Ptr) & "'"
                                            Next
                                            SubTitle = SubTitle & ", in groups" & Mid(SubTitlePart, 2) & " and '" & Groups(Ptr) & "'"
                                        End If

                                    End If
                                End If
                                '
                                ' add sort details to caption
                                '
                                SubTitlePart = ""
                                For Each kvp In .Sorts
                                    Dim sort As indexConfigSortClass = kvp.Value
                                    If (sort.direction > 0) Then
                                        SubTitlePart = SubTitlePart & " and " & adminContent.fields(sort.fieldName).caption
                                        If (sort.direction > 1) Then
                                            SubTitlePart &= " reverse"
                                        End If
                                    End If
                                Next
                                If SubTitlePart <> "" Then
                                    SubTitle &= ", sorted by" & Mid(SubTitlePart, 5)
                                End If
                            End With
                            '
                            Title = adminContent.Name
                            If TitleExtension <> "" Then
                                Title = Title & " " & TitleExtension
                            End If
                            Select Case recordCnt
                                Case 0
                                    RightCopy = "no records found"
                                Case 1
                                    RightCopy = "1 record found"
                                Case Else
                                    RightCopy = recordCnt & " records found"
                            End Select
                            RightCopy = RightCopy & ", page " & IndexConfig.PageNumber
                            Title = "<div>" _
                                & "<span style=""float:left;""><strong>" & Title & "</strong></span>" _
                                & "<span style=""float:right;"">" & RightCopy & "</span>" _
                                & "</div>"
                            TitleRows = 0
                            If SubTitle <> "" Then
                                Title = Title & "<div style=""clear:both"">Filter: " & genericController.encodeHTML(Mid(SubTitle, 3)) & "</div>"
                                TitleRows = TitleRows + 1
                            End If
                            If ContentAccessLimitMessage <> "" Then
                                Title = Title & "<div style=""clear:both"">" & ContentAccessLimitMessage & "</div>"
                                TitleRows = TitleRows + 1
                            End If
                            If TitleRows = 0 Then
                                Title = Title & "<div style=""clear:both"">&nbsp;</div>"
                            End If
                            '
                            TitleBar = SpanClassAdminNormal & Title & "</span>"
                            'TitleBar = TitleBar & cpCore.main_GetHelpLink(46, "Using the Admin Index Page", BubbleCopy_AdminIndexPage)
                            '
                            ' ----- Filter Data Table
                            '
                            If AllowFilterNav Then
                                '
                                ' Filter Nav - if enabled, just add another cell to the row
                                '
                                IndexFilterOpen = cpCore.visitProperty.getBoolean("IndexFilterOpen", False)
                                If IndexFilterOpen Then
                                    '
                                    ' Ajax Filter Open
                                    '
                                    IndexFilterHead = "" _
                                        & vbCrLf & "<div class=""ccHeaderCon"">" _
                                        & vbCrLf & "<div id=""IndexFilterHeCursorTypeEnum.ADOPENed"" class=""opened"">" _
                                        & cr & "<table border=0 cellpadding=0 cellspacing=0 width=""100%""><tr>" _
                                        & cr & "<td valign=Middle class=""left"">Filters</td>" _
                                        & cr & "<td valign=Middle class=""right""><a href=""#"" onClick=""CloseIndexFilter();return false""><img alt=""Close Filters"" title=""Close Filters"" src=""/ccLib/images/ClosexRev1313.gif"" width=13 height=13 border=0></a></td>" _
                                        & cr & "</tr></table>" _
                                        & vbCrLf & "</div>" _
                                        & vbCrLf & "<div id=""IndexFilterHeadClosed"" class=""closed"" style=""display:none;"">" _
                                        & cr & "<a href=""#"" onClick=""OpenIndexFilter();return false""><img title=""Open Navigator"" alt=""Open Filter"" src=""/ccLib/images/OpenRightRev1313.gif"" width=13 height=13 border=0 style=""text-align:right;""></a>" _
                                        & vbCrLf & "</div>" _
                                        & vbCrLf & "</div>" _
                                        & ""
                                    IndexFilterContent = "" _
                                        & vbCrLf & "<div class=""ccContentCon"">" _
                                        & vbCrLf & "<div id=""IndexFilterContentOpened"" class=""opened"">" & GetForm_IndexFilterContent(adminContent) & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""200"" height=""1"" style=""clear:both""></div>" _
                                        & vbCrLf & "<div id=""IndexFilterContentClosed"" class=""closed"" style=""display:none;"">" & FilterClosedLabel & "</div>" _
                                        & vbCrLf & "</div>"
                                    IndexFilterJS = "" _
                                        & vbCrLf & "<script Language=""JavaScript"" type=""text/javascript"">" _
                                        & vbCrLf & "function CloseIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','none');SetDisplay('IndexFilterContentOpened','none');SetDisplay('IndexFilterHeadClosed','block');SetDisplay('IndexFilterContentClosed','block');cj.ajax.qs('" & RequestNameAjaxFunction & "=" & AjaxCloseIndexFilter & "','','')}" _
                                        & vbCrLf & "function OpenIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','block');SetDisplay('IndexFilterContentOpened','block');SetDisplay('IndexFilterHeadClosed','none');SetDisplay('IndexFilterContentClosed','none');cj.ajax.qs('" & RequestNameAjaxFunction & "=" & AjaxOpenIndexFilter & "','','')}" _
                                        & vbCrLf & "</script>"
                                Else
                                    '
                                    ' Ajax Filter Closed
                                    '
                                    IndexFilterHead = "" _
                                        & vbCrLf & "<div class=""ccHeaderCon"">" _
                                        & vbCrLf & "<div id=""IndexFilterHeCursorTypeEnum.ADOPENed"" class=""opened"" style=""display:none;"">" _
                                        & cr & "<table border=0 cellpadding=0 cellspacing=0 width=""100%""><tr>" _
                                        & cr & "<td valign=Middle class=""left"">Filter</td>" _
                                        & cr & "<td valign=Middle class=""right""><a href=""#"" onClick=""CloseIndexFilter();return false""><img alt=""Close Filter"" title=""Close Navigator"" src=""/ccLib/images/ClosexRev1313.gif"" width=13 height=13 border=0></a></td>" _
                                        & cr & "</tr></table>" _
                                        & vbCrLf & "</div>" _
                                        & vbCrLf & "<div id=""IndexFilterHeadClosed"" class=""closed"">" _
                                        & cr & "<a href=""#"" onClick=""OpenIndexFilter();return false""><img title=""Open Navigator"" alt=""Open Navigator"" src=""/ccLib/images/OpenRightRev1313.gif"" width=13 height=13 border=0 style=""text-align:right;""></a>" _
                                        & vbCrLf & "</div>" _
                                        & vbCrLf & "</div>" _
                                        & ""
                                    IndexFilterContent = "" _
                                        & vbCrLf & "<div class=""ccContentCon"">" _
                                        & vbCrLf & "<div id=""IndexFilterContentOpened"" class=""opened"" style=""display:none;""><div style=""text-align:center;""><img src=""/ccLib/images/ajax-loader-small.gif"" width=16 height=16></div></div>" _
                                        & vbCrLf & "<div id=""IndexFilterContentClosed"" class=""closed"">" & FilterClosedLabel & "</div>" _
                                        & vbCrLf & "<div id=""IndexFilterContentMinWidth"" style=""display:none;""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""200"" height=""1"" style=""clear:both""></div>" _
                                        & vbCrLf & "</div>"
                                    AjaxQS = cpCore.doc.refreshQueryString
                                    AjaxQS = genericController.ModifyQueryString(AjaxQS, RequestNameAjaxFunction, AjaxOpenIndexFilterGetContent)
                                    IndexFilterJS = "" _
                                        & vbCrLf & "<script Language=""JavaScript"" type=""text/javascript"">" _
                                        & vbCrLf & "var IndexFilterPop=false;" _
                                        & vbCrLf & "function CloseIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','none');SetDisplay('IndexFilterHeadClosed','block');SetDisplay('IndexFilterContentOpened','none');SetDisplay('IndexFilterContentMinWidth','none');SetDisplay('IndexFilterContentClosed','block');cj.ajax.qs('" & RequestNameAjaxFunction & "=" & AjaxCloseIndexFilter & "','','')}" _
                                        & vbCrLf & "function OpenIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','block');SetDisplay('IndexFilterHeadClosed','none');SetDisplay('IndexFilterContentOpened','block');SetDisplay('IndexFilterContentMinWidth','block');SetDisplay('IndexFilterContentClosed','none');if(!IndexFilterPop){cj.ajax.qs('" & AjaxQS & "','','IndexFilterContentOpened');IndexFilterPop=true;}else{cj.ajax.qs('" & RequestNameAjaxFunction & "=" & AjaxOpenIndexFilter & "','','');}}" _
                                        & vbCrLf & "</script>"
                                End If
                            End If
                            '
                            ' Dual Window Right - Data
                            '
                            FilterDataTable &= "<td valign=top class=""ccPanel"">"
                            '
                            DataTable_HdrRow &= "<tr>"
                            '
                            ' Row Number Column
                            '
                            DataTable_HdrRow &= "<td width=20 align=center valign=bottom class=""ccAdminListCaption"">Row</td>"
                            '
                            ' Delete Select Box Columns
                            '
                            If Not AllowDelete Then
                                DataTable_HdrRow &= "<td width=20 align=center valign=bottom class=""ccAdminListCaption""><input TYPE=CheckBox disabled=""disabled""></td>"
                            Else
                                DataTable_HdrRow &= "<td width=20 align=center valign=bottom class=""ccAdminListCaption""><input TYPE=CheckBox OnClick=""CheckInputs('DelCheck',this.checked);""></td>"
                            End If
                            '
                            ' Calculate total width
                            '
                            ColumnWidthTotal = 0
                            For Each kvp In IndexConfig.Columns
                                Dim column As indexConfigColumnClass = kvp.Value
                                If column.Width < 1 Then
                                    column.Width = 1
                                End If
                                ColumnWidthTotal = ColumnWidthTotal + column.Width
                            Next
                            '
                            ' Edit Column
                            '
                            DataTable_HdrRow &= "<td width=20 align=center valign=bottom class=""ccAdminListCaption"">Edit</td>"
                            For Each kvp In IndexConfig.Columns
                                Dim column As indexConfigColumnClass = kvp.Value
                                '
                                ' ----- print column headers - anchored so they sort columns
                                '
                                ColumnWidth = CInt((100 * column.Width) / ColumnWidthTotal)
                                'fieldId = column.FieldId
                                FieldName = column.Name
                                '
                                'if this is a current sort ,add the reverse flag
                                '
                                ButtonHref = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & RequestNameAdminForm & "=" & AdminFormIndex & "&SetSortField=" & FieldName & "&RT=0&" & RequestNameTitleExtension & "=" & genericController.EncodeRequestVariable(TitleExtension) & "&cid=" & adminContent.Id & "&ad=" & MenuDepth
                                For Each sortKvp In IndexConfig.Sorts
                                    Dim sort As indexConfigSortClass = sortKvp.Value

                                Next
                                If Not IndexConfig.Sorts.ContainsKey(FieldName) Then
                                    ButtonHref &= "&SetSortDirection=1"
                                Else
                                    Select Case IndexConfig.Sorts(FieldName).direction
                                        Case 1
                                            ButtonHref &= "&SetSortDirection=2"
                                        Case 2
                                            ButtonHref &= "&SetSortDirection=0"
                                        Case Else
                                    End Select
                                End If
                                '
                                '----- column header includes WherePairCount
                                '
                                If WherePairCount > 0 Then
                                    For WhereCount = 0 To WherePairCount - 1
                                        If WherePair(0, WhereCount) <> "" Then
                                            ButtonHref &= "&wl" & WhereCount & "=" & genericController.EncodeRequestVariable(WherePair(0, WhereCount))
                                            ButtonHref &= "&wr" & WhereCount & "=" & genericController.EncodeRequestVariable(WherePair(1, WhereCount))
                                        End If
                                    Next
                                End If
                                ButtonFace = adminContent.fields(FieldName.ToLower()).caption
                                ButtonFace = genericController.vbReplace(ButtonFace, " ", "&nbsp;")
                                SortTitle = "Sort A-Z"
                                '
                                If IndexConfig.Sorts.ContainsKey(FieldName) Then
                                    Select Case IndexConfig.Sorts(FieldName).direction
                                        Case 1
                                            ButtonFace = ButtonFace & "<img src=""/ccLib/images/arrowdown.gif"" width=8 height=8 border=0>"
                                            SortTitle = "Sort Z-A"
                                        Case 2
                                            ButtonFace = ButtonFace & "<img src=""/ccLib/images/arrowup.gif"" width=8 height=8 border=0>"
                                            SortTitle = "Remove Sort"
                                        Case Else
                                    End Select
                                End If
                                ButtonObject = "Button" & ButtonObjectCount
                                ButtonObjectCount = ButtonObjectCount + 1
                                DataTable_HdrRow &= "<td width=""" & ColumnWidth & "%"" valign=bottom align=left class=""ccAdminListCaption"">"
                                DataTable_HdrRow &= ("<a title=""" & SortTitle & """ href=""" & genericController.encodeHTML(ButtonHref) & """ class=""ccAdminListCaption"">" & ButtonFace & "</A>")
                                DataTable_HdrRow &= ("</td>")
                            Next
                            DataTable_HdrRow &= ("</tr>")
                            '
                            '   select and print Records
                            '
                            'DataSourceName = cpCore.db.getDataSourceNameByID(adminContent.dataSourceId)
                            CS = cpCore.db.csOpenSql(SQL, datasource.Name, IndexConfig.RecordsPerPage, IndexConfig.PageNumber)
                            If cpCore.db.csOk(CS) Then
                                RowColor = ""
                                RecordPointer = IndexConfig.RecordTop
                                RecordLast = IndexConfig.RecordTop + IndexConfig.RecordsPerPage
                                '
                                ' --- Print out the records
                                '
                                'IsPageContent = (LCase(adminContent.ContentTableName) = "ccpagecontent")
                                'If IsPageContent Then
                                '    LandingPageID = cpCore.main_GetLandingPageID
                                'End If
                                Do While ((cpCore.db.csOk(CS)) And (RecordPointer < RecordLast))
                                    RecordID = cpCore.db.csGetInteger(CS, "ID")
                                    RecordName = cpCore.db.csGetText(CS, "name")
                                    'IsLandingPage = IsPageContent And (RecordID = LandingPageID)
                                    If RowColor = "class=""ccAdminListRowOdd""" Then
                                        RowColor = "class=""ccAdminListRowEven"""
                                    Else
                                        RowColor = "class=""ccAdminListRowOdd"""
                                    End If
                                    DataTable_DataRows &= vbCrLf & "<tr>"
                                    '
                                    ' --- Record Number column
                                    '
                                    DataTable_DataRows &= "<td align=right " & RowColor & ">" & SpanClassAdminSmall & "[" & RecordPointer + 1 & "]</span></td>"
                                    '
                                    ' --- Delete Checkbox Columns
                                    '
                                    If AllowDelete Then
                                        'If AllowDelete And Not IsLandingPage Then
                                        'If AdminContent.AllowDelete And Not IsLandingPage Then
                                        DataTable_DataRows &= "<td align=center " & RowColor & "><input TYPE=CheckBox NAME=row" & RecordPointer & " VALUE=1 ID=""DelCheck""><input type=hidden name=rowid" & RecordPointer & " VALUE=" & RecordID & "></span></td>"
                                    Else
                                        DataTable_DataRows &= "<td align=center " & RowColor & "><input TYPE=CheckBox disabled=""disabled"" NAME=row" & RecordPointer & " VALUE=1><input type=hidden name=rowid" & RecordPointer & " VALUE=" & RecordID & "></span></td>"
                                    End If
                                    '
                                    ' --- Edit button column
                                    '
                                    DataTable_DataRows &= "<td align=center " & RowColor & ">"
                                    URI = "\" & cpCore.serverConfig.appConfig.adminRoute _
                                        & "?" & RequestNameAdminAction & "=" & AdminActionNop _
                                        & "&cid=" & adminContent.Id _
                                        & "&id=" & RecordID _
                                        & "&" & RequestNameTitleExtension & "=" & genericController.EncodeRequestVariable(TitleExtension) _
                                        & "&ad=" & MenuDepth _
                                        & "&" & RequestNameAdminSourceForm & "=" & AdminForm _
                                        & "&" & RequestNameAdminForm & "=" & AdminFormEdit
                                    If WherePairCount > 0 Then
                                        For WhereCount = 0 To WherePairCount - 1
                                            URI = URI & "&wl" & WhereCount & "=" & genericController.EncodeRequestVariable(WherePair(0, WhereCount)) & "&wr" & WhereCount & "=" & genericController.EncodeRequestVariable(WherePair(1, WhereCount))
                                        Next
                                    End If
                                    DataTable_DataRows &= ("<a href=""" & genericController.encodeHTML(URI) & """><img src=""/ccLib/images/IconContentEdit.gif"" border=""0""></a>")
                                    DataTable_DataRows &= ("</td>")
                                    '
                                    ' --- field columns
                                    '
                                    For Each columnKvp In IndexConfig.Columns
                                        Dim column As indexConfigColumnClass = columnKvp.Value
                                        Dim columnNameLc As String = column.Name.ToLower()
                                        If FieldUsedInColumns.ContainsKey(columnNameLc) Then
                                            If FieldUsedInColumns.Item(columnNameLc) Then
                                                DataTable_DataRows &= (vbCrLf & "<td valign=""middle"" " & RowColor & " align=""left"">" & SpanClassAdminNormal)
                                                DataTable_DataRows &= GetForm_Index_GetCell(adminContent, editRecord, column.Name, CS, IsLookupFieldValid(columnNameLc), genericController.vbLCase(adminContent.ContentTableName) = "ccemail")
                                                DataTable_DataRows &= ("&nbsp;</span></td>")
                                            End If
                                        End If
                                    Next
                                    DataTable_DataRows &= (vbLf & "    </tr>")
                                    Call cpCore.db.csGoNext(CS)
                                    RecordPointer = RecordPointer + 1
                                Loop
                                DataTable_DataRows &= "<input type=hidden name=rowcnt value=" & RecordPointer & ">"
                                '
                                ' --- print out the stuff at the bottom
                                '
                                RecordTop_NextPage = IndexConfig.RecordTop
                                If cpCore.db.csOk(CS) Then
                                    RecordTop_NextPage = RecordPointer
                                End If
                                RecordTop_PreviousPage = IndexConfig.RecordTop - IndexConfig.RecordsPerPage
                                If RecordTop_PreviousPage < 0 Then
                                    RecordTop_PreviousPage = 0
                                End If
                            End If
                            Call cpCore.db.csClose(CS)
                            '
                            ' Header at bottom
                            '
                            If RowColor = "class=""ccAdminListRowOdd""" Then
                                RowColor = "class=""ccAdminListRowEven"""
                            Else
                                RowColor = "class=""ccAdminListRowOdd"""
                            End If
                            If (RecordPointer = 0) Then
                                '
                                ' No records found
                                '
                                DataTable_DataRows &= ("<tr>" _
                                    & "<td " & RowColor & " align=center>-</td>" _
                                    & "<td " & RowColor & " align=center>-</td>" _
                                    & "<td " & RowColor & " align=center>-</td>" _
                                    & "<td colspan=" & IndexConfig.Columns.Count & " " & RowColor & " style=""text-align:left ! important;"">no records were found</td>" _
                                    & "</tr>")
                            Else
                                If (RecordPointer < RecordLast) Then
                                    '
                                    ' End of list
                                    '
                                    DataTable_DataRows &= ("<tr>" _
                                        & "<td " & RowColor & " align=center>-</td>" _
                                        & "<td " & RowColor & " align=center>-</td>" _
                                        & "<td " & RowColor & " align=center>-</td>" _
                                        & "<td colspan=" & IndexConfig.Columns.Count & " " & RowColor & " style=""text-align:left ! important;"">----- end of list</td>" _
                                        & "</tr>")
                                End If
                                '
                                ' Add another header to the data rows
                                '
                                DataTable_DataRows &= DataTable_HdrRow
                            End If
                            ''
                            '' ----- DataTable_FindRow
                            ''
                            'ReDim Findstring(IndexConfig.Columns.Count)
                            'For ColumnPointer = 0 To IndexConfig.Columns.Count - 1
                            '    FieldName = IndexConfig.Columns(ColumnPointer).Name
                            '    If genericController.vbLCase(FieldName) = FindWordName Then
                            '        Findstring(ColumnPointer) = FindWordValue
                            '    End If
                            'Next
                            '        ReDim Findstring(CustomAdminColumnCount)
                            '        For ColumnPointer = 0 To CustomAdminColumnCount - 1
                            '            FieldPtr = CustomAdminColumn(ColumnPointer).FieldPointer
                            '            With AdminContent.fields(FieldPtr)
                            '                If genericController.vbLCase(.Name) = FindWordName Then
                            '                    Findstring(ColumnPointer) = FindWordValue
                            '                End If
                            '            End With
                            '        Next
                            '
                            DataTable_FindRow = DataTable_FindRow & "<tr><td colspan=" & (3 + IndexConfig.Columns.Count) & " style=""background-color:black;height:1;""></td></tr>"
                            'DataTable_FindRow = DataTable_FindRow & "<tr><td colspan=" & (3 + CustomAdminColumnCount) & " style=""background-color:black;height:1;""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td></tr>"
                            DataTable_FindRow = DataTable_FindRow & "<tr>"
                            DataTable_FindRow = DataTable_FindRow & "<td colspan=3 width=""60"" class=""ccPanel"" align=center style=""text-align:center ! important;"">"
                            DataTable_FindRow = DataTable_FindRow _
                                & vbCrLf & "<script language=""javascript"" type=""text/javascript"">" _
                                & vbCrLf & "function KeyCheck(e){" _
                                & vbCrLf & "  var code = e.keyCode;" _
                                & vbCrLf & "  if(code==13){" _
                                & vbCrLf & "    document.getElementById('FindButton').focus();" _
                                & vbCrLf & "    document.getElementById('FindButton').click();" _
                                & vbCrLf & "    return false;" _
                                & vbCrLf & "  }" _
                                & vbCrLf & "} " _
                                & vbCrLf & "</script>"
                            DataTable_FindRow = DataTable_FindRow & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""60"" height=""1"" ><br >" & cpCore.html.html_GetFormButton(ButtonFind, , "FindButton") & "</td>"
                            ColumnPointer = 0
                            For Each kvp In IndexConfig.Columns
                                Dim column As indexConfigColumnClass = kvp.Value
                                'For ColumnPointer = 0 To CustomAdminColumnCount - 1
                                With column
                                    ColumnWidth = .Width
                                    'fieldId = .FieldId
                                    FieldName = genericController.vbLCase(.Name)
                                End With
                                FindWordValue = ""
                                If IndexConfig.FindWords.ContainsKey(FieldName) Then
                                    With IndexConfig.FindWords(FieldName)
                                        If (.MatchOption = FindWordMatchEnum.matchincludes) Or (.MatchOption = FindWordMatchEnum.MatchEquals) Then
                                            FindWordValue = .Value
                                        End If
                                    End With
                                End If
                                DataTable_FindRow = DataTable_FindRow _
                                    & vbCrLf _
                                    & "<td valign=""top"" align=""center"" class=""ccPanel3DReverse"" style=""padding-top:2px;padding-bottom:2px;"">" _
                                    & "<input type=hidden name=""FindName" & ColumnPointer & """ value=""" & FieldName & """>" _
                                    & "<input onkeypress=""KeyCheck(event);""  type=text id=""F" & ColumnPointer & """ name=""FindValue" & ColumnPointer & """ value=""" & FindWordValue & """ style=""width:98%"">" _
                                    & "</td>"
                                ColumnPointer += 1
                            Next
                            DataTable_FindRow = DataTable_FindRow & "</tr>"
                            '
                            ' Assemble DataTable
                            '
                            DataTable = "" _
                                & "<table ID=""DataTable"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""Background-Color:white;"">" _
                                & DataTable_HdrRow _
                                & DataTable_DataRows _
                                & DataTable_FindRow _
                                & "</table>"
                            'DataTable = GetForm_Index_AdvancedSearch()
                            '
                            ' Assemble DataFilterTable
                            '
                            If IndexFilterContent <> "" Then
                                FilterColumn = "<td valign=top style=""border-right:1px solid black;"" class=""ccToolsCon"">" & IndexFilterJS & IndexFilterHead & IndexFilterContent & "</td>"
                                'FilterColumn = "<td valign=top class=""ccPanel3DReverse ccAdminEditBody"" style=""border-right:1px solid black;"">" & IndexFilterJS & IndexFilterHead & IndexFilterContent & "</td>"
                            End If
                            DataColumn = "<td width=""99%"" valign=top>" & DataTable & "</td>"
                            FilterDataTable = "" _
                                & "<table ID=""DataFilterTable"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""Background-Color:white;"">" _
                                & "<tr>" _
                                & FilterColumn _
                                & DataColumn _
                                & "</tr>" _
                                & "</table>"
                            '
                            ' Assemble LiveWindowTable
                            '
                            ' Stream.Add( OpenLiveWindowTable)
                            Stream.Add(vbCrLf & cpCore.html.html_GetFormStart(, "adminForm"))
                            Stream.Add("<input type=""hidden"" name=""indexGoToPage"" value="""">")
                            Stream.Add(ButtonBar)
                            Stream.Add(Adminui.GetTitleBar(TitleBar, HeaderDescription))
                            Stream.Add(FilterDataTable)
                            Stream.Add(ButtonBar)
                            Stream.Add(cpCore.html.main_GetPanel("<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"", height=""10"" >"))
                            Stream.Add("<input type=hidden name=Columncnt VALUE=" & IndexConfig.Columns.Count & ">")
                            Stream.Add("</form>")
                            '  Stream.Add( CloseLiveWindowTable)
                            Call cpCore.html.addTitle(adminContent.Name)
                        End If
                    End If
                    'End If
                    '
                End If
                returnForm = Stream.Text
                '
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnForm
        End Function
        '
        '========================================================================
        ' ----- Get an XML nodes attribute based on its name
        '========================================================================
        '
        Private Function GetXMLAttribute(ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As String) As String
            On Error GoTo ErrorTrap
            '
            Dim NodeAttribute As XmlAttribute
            Dim ResultNode As XmlNode
            Dim UcaseName As String
            '
            Found = False
            ResultNode = Node.Attributes.GetNamedItem(Name)
            If (ResultNode Is Nothing) Then
                UcaseName = genericController.vbUCase(Name)
                For Each NodeAttribute In Node.Attributes
                    If genericController.vbUCase(NodeAttribute.Name) = UcaseName Then
                        GetXMLAttribute = NodeAttribute.Value
                        Found = True
                        Exit For
                    End If
                Next
            Else
                GetXMLAttribute = ResultNode.Value
                Found = True
            End If
            If Not Found Then
                GetXMLAttribute = DefaultIfNotFound
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetXMLAttribute")
        End Function
        '
        ' REFACTOR -- THIS SHOULD BE A REMOTE METHOD AND NOT CALLED FROM CPCORE.
        '==========================================================================================================================================
        ''' <summary>
        ''' Get index view filter content - remote method
        ''' </summary>
        ''' <param name="adminContent"></param>
        ''' <returns></returns>
        Public Function GetForm_IndexFilterContent(adminContent As Models.Complex.cdefModel) As String
            Dim returnContent As String = ""
            Try
                Dim RecordID As Integer
                Dim Name As String
                Dim TableName As String
                Dim FieldCaption As String
                Dim ContentName As String
                Dim CS As Integer
                Dim SQL As String
                Dim Caption As String
                Dim Link As String
                Dim IsAuthoringMode As Boolean
                Dim FirstCaption As String = ""
                Dim RQS As String
                Dim QS As String
                Dim Ptr As Integer
                Dim SubFilterList As String
                Dim IndexConfig As indexConfigClass
                Dim list As String
                Dim ListSplit() As String
                Dim Cnt As Integer
                Dim Pos As Integer
                Dim subContentID As Integer
                '
                IndexConfig = LoadIndexConfig(adminContent)
                With IndexConfig
                    '
                    ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, adminContent.Id)
                    IsAuthoringMode = True
                    RQS = "cid=" & adminContent.Id & "&af=1"
                    '
                    '-------------------------------------------------------------------------------------
                    ' Remove filters
                    '-------------------------------------------------------------------------------------
                    '
                    If (.SubCDefID > 0) Or (.GroupListCnt <> 0) Or (.FindWords.Count <> 0) Or .ActiveOnly Or .LastEditedByMe Or .LastEditedToday Or .LastEditedPast7Days Or .LastEditedPast30Days Then
                        '
                        ' Remove Filters
                        '
                        returnContent &= "<div class=""ccFilterHead"">Remove&nbsp;Filters</div>"
                        QS = RQS
                        QS = genericController.ModifyQueryString(QS, "IndexFilterRemoveAll", "1")
                        Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                        returnContent &= "<div class=""ccFilterSubHead""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;Remove All</a></div>"
                        '
                        ' Last Edited Edited by me
                        '
                        SubFilterList = ""
                        If .LastEditedByMe Then
                            QS = RQS
                            QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedByMe", CStr(0), True)
                            Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                            SubFilterList = SubFilterList & "<div class=""ccFilterIndent ccFilterList""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">By&nbsp;Me</a></div>"
                        End If
                        If .LastEditedToday Then
                            QS = RQS
                            QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedToday", CStr(0), True)
                            Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                            SubFilterList = SubFilterList & "<div class=""ccFilterIndent ccFilterList""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">Today</a></div>"
                        End If
                        If .LastEditedPast7Days Then
                            QS = RQS
                            QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedPast7Days", CStr(0), True)
                            Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                            SubFilterList = SubFilterList & "<div class=""ccFilterIndent ccFilterList""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">Past Week</a></div>"
                        End If
                        If .LastEditedPast30Days Then
                            QS = RQS
                            QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedPast30Days", CStr(0), True)
                            Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                            SubFilterList = SubFilterList & "<div class=""ccFilterIndent ccFilterList""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">Past 30 Days</a></div>"
                        End If
                        If SubFilterList <> "" Then
                            returnContent &= "<div class=""ccFilterSubHead"">Last&nbsp;Edited</div>" & SubFilterList
                        End If
                        '
                        ' Sub Content definitions
                        '
                        Dim SubContentName As String
                        SubFilterList = ""
                        If .SubCDefID > 0 Then
                            SubContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, .SubCDefID)
                            QS = RQS
                            QS = genericController.ModifyQueryString(QS, "IndexFilterRemoveCDef", CStr(.SubCDefID))
                            Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                            SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">" & SubContentName & "</a></div>"
                        End If
                        If SubFilterList <> "" Then
                            returnContent &= "<div class=""ccFilterSubHead"">In Sub-content</div>" & SubFilterList
                        End If
                        '
                        ' Group Filter List
                        '
                        Dim GroupName As String
                        SubFilterList = ""
                        If .GroupListCnt > 0 Then
                            For Ptr = 0 To .GroupListCnt - 1
                                GroupName = .GroupList(Ptr)
                                If .GroupList(Ptr) <> "" Then
                                    If Len(GroupName) > 30 Then
                                        GroupName = Left(GroupName, 15) & "..." & Right(GroupName, 15)
                                    End If
                                    QS = RQS
                                    QS = genericController.ModifyQueryString(QS, "IndexFilterRemoveGroup", .GroupList(Ptr))
                                    Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                                    SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">" & GroupName & "</a></div>"
                                End If
                            Next
                        End If
                        If SubFilterList <> "" Then
                            returnContent &= "<div class=""ccFilterSubHead"">In Group(s)</div>" & SubFilterList
                        End If
                        '
                        ' Other Filter List
                        '
                        SubFilterList = ""
                        If .ActiveOnly Then
                            QS = RQS
                            QS = genericController.ModifyQueryString(QS, "IndexFilterActiveOnly", CStr(0), True)
                            Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                            SubFilterList = SubFilterList & "<div class=""ccFilterIndent ccFilterList""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">Active&nbsp;Only</a></div>"
                        End If
                        If SubFilterList <> "" Then
                            returnContent &= "<div class=""ccFilterSubHead"">Other</div>" & SubFilterList
                        End If
                        '
                        ' FindWords
                        '
                        For Each findWordKvp In .FindWords
                            Dim findWord As indexConfigFindWordClass = findWordKvp.Value
                            FieldCaption = genericController.encodeText(Models.Complex.cdefModel.GetContentFieldProperty(cpCore, ContentName, findWord.Name, "caption"))
                            QS = RQS
                            QS = genericController.ModifyQueryString(QS, "IndexFilterRemoveFind", findWord.Name)
                            Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                            Select Case findWord.MatchOption
                                Case FindWordMatchEnum.matchincludes
                                    returnContent &= "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;" & FieldCaption & "&nbsp;includes&nbsp;'" & findWord.Value & "'</a></div>"
                                Case FindWordMatchEnum.MatchEmpty
                                    returnContent &= "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;" & FieldCaption & "&nbsp;is&nbsp;empty</a></div>"
                                Case FindWordMatchEnum.MatchEquals
                                    returnContent &= "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;" & FieldCaption & "&nbsp;=&nbsp;'" & findWord.Value & "'</a></div>"
                                Case FindWordMatchEnum.MatchFalse
                                    returnContent &= "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;" & FieldCaption & "&nbsp;is&nbsp;false</a></div>"
                                Case FindWordMatchEnum.MatchGreaterThan
                                    returnContent &= "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;" & FieldCaption & "&nbsp;&gt;&nbsp;'" & findWord.Value & "'</a></div>"
                                Case FindWordMatchEnum.MatchLessThan
                                    returnContent &= "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;" & FieldCaption & "&nbsp;&lt;&nbsp;'" & findWord.Value & "'</a></div>"
                                Case FindWordMatchEnum.MatchNotEmpty
                                    returnContent &= "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;" & FieldCaption & "&nbsp;is&nbsp;not&nbsp;empty</a></div>"
                                Case FindWordMatchEnum.MatchTrue
                                    returnContent &= "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """><img src=""/ccLib/images/delete1313.gif"" width=13 height=13 border=0 style=""vertical-align:middle;"">&nbsp;" & FieldCaption & "&nbsp;is&nbsp;true</a></div>"
                            End Select
                        Next
                        '
                        returnContent &= "<div style=""border-bottom:1px dotted #808080;"">&nbsp;</div>"
                    End If
                    '
                    '-------------------------------------------------------------------------------------
                    ' Add filters
                    '-------------------------------------------------------------------------------------
                    '
                    returnContent &= "<div class=""ccFilterHead"">Add&nbsp;Filters</div>"
                    '
                    ' Last Edited
                    '
                    SubFilterList = ""
                    If Not .LastEditedByMe Then
                        QS = RQS
                        QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedByMe", "1", True)
                        Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                        SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """>By&nbsp;Me</a></div>"
                    End If
                    If Not .LastEditedToday Then
                        QS = RQS
                        QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedToday", "1", True)
                        Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                        SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """>Today</a></div>"
                    End If
                    If Not .LastEditedPast7Days Then
                        QS = RQS
                        QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedPast7Days", "1", True)
                        Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                        SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """>Past Week</a></div>"
                    End If
                    If Not .LastEditedPast30Days Then
                        QS = RQS
                        QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedPast30Days", "1", True)
                        Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                        SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """>Past 30 Days</a></div>"
                    End If
                    If SubFilterList <> "" Then
                        returnContent &= "<div class=""ccFilterSubHead"">Last&nbsp;Edited</div>" & SubFilterList
                    End If
                    '
                    ' Sub Content Definitions
                    '
                    SubFilterList = ""
                    list = Models.Complex.cdefModel.getContentControlCriteria(cpCore, ContentName)
                    If list <> "" Then
                        ListSplit = Split(list, "=")
                        Cnt = UBound(ListSplit) + 1
                        If Cnt > 0 Then
                            For Ptr = 0 To Cnt - 1
                                Pos = genericController.vbInstr(1, ListSplit(Ptr), ")")
                                If Pos > 0 Then
                                    subContentID = genericController.EncodeInteger(Mid(ListSplit(Ptr), 1, Pos - 1))
                                    If subContentID > 0 And (subContentID <> adminContent.Id) And (subContentID <> .SubCDefID) Then
                                        Caption = "<span style=""white-space:nowrap;"">" & Models.Complex.cdefModel.getContentNameByID(cpCore, subContentID) & "</span>"
                                        QS = RQS
                                        QS = genericController.ModifyQueryString(QS, "IndexFilterAddCDef", CStr(subContentID), True)
                                        Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                                        SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """>" & Caption & "</a></div>"
                                    End If
                                End If
                            Next
                        End If
                    End If
                    If SubFilterList <> "" Then
                        returnContent &= "<div class=""ccFilterSubHead"">In Sub-content</div>" & SubFilterList
                    End If
                    '
                    ' people filters
                    '
                    TableName = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName)
                    SubFilterList = ""
                    If genericController.vbLCase(TableName) = genericController.vbLCase("ccMembers") Then
                        SQL = cpCore.db.GetSQLSelect("default", "ccGroups", "ID,Caption,Name", "(active<>0)", "Caption,Name")
                        CS = cpCore.db.csOpenSql_rev("default", SQL)
                        Do While cpCore.db.csOk(CS)
                            Name = cpCore.db.csGetText(CS, "Name")
                            Ptr = 0
                            If .GroupListCnt > 0 Then
                                For Ptr = 0 To .GroupListCnt - 1
                                    If Name = .GroupList(Ptr) Then
                                        Exit For
                                    End If
                                Next
                            End If
                            If Ptr = .GroupListCnt Then
                                RecordID = cpCore.db.csGetInteger(CS, "ID")
                                Caption = cpCore.db.csGetText(CS, "Caption")
                                If Caption = "" Then
                                    Caption = Name
                                    If Caption = "" Then
                                        Caption = "Group " & RecordID
                                    End If
                                End If
                                If Len(Caption) > 30 Then
                                    Caption = Left(Caption, 15) & "..." & Right(Caption, 15)
                                End If
                                Caption = "<span style=""white-space:nowrap;"">" & Caption & "</span>"
                                QS = RQS
                                If Trim(Name) <> "" Then
                                    QS = genericController.ModifyQueryString(QS, "IndexFilterAddGroup", Name, True)
                                Else
                                    QS = genericController.ModifyQueryString(QS, "IndexFilterAddGroup", CStr(RecordID), True)
                                End If
                                Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                                SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """>" & Caption & "</a></div>"
                            End If
                            cpCore.db.csGoNext(CS)
                        Loop
                    End If
                    If SubFilterList <> "" Then
                        returnContent &= "<div class=""ccFilterSubHead"">In Group(s)</div>" & SubFilterList
                    End If
                    '
                    ' Active Only
                    '
                    SubFilterList = ""
                    If Not .ActiveOnly Then
                        QS = RQS
                        QS = genericController.ModifyQueryString(QS, "IndexFilterActiveOnly", "1", True)
                        Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                        SubFilterList = SubFilterList & "<div class=""ccFilterIndent""><a class=""ccFilterLink"" href=""" & Link & """>Active&nbsp;Only</a></div>"
                    End If
                    If SubFilterList <> "" Then
                        returnContent &= "<div class=""ccFilterSubHead"">Other</div>" & SubFilterList
                    End If
                    '
                    returnContent &= "<div style=""border-bottom:1px dotted #808080;"">&nbsp;</div>"
                    '
                    ' Advanced Search Link
                    '
                    QS = RQS
                    QS = genericController.ModifyQueryString(QS, RequestNameAdminSubForm, AdminFormIndex_SubFormAdvancedSearch, True)
                    Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                    returnContent &= "<div class=""ccFilterHead""><a class=""ccFilterLink"" href=""" & Link & """>Advanced&nbsp;Search</a></div>"
                    '
                    returnContent &= "<div style=""border-bottom:1px dotted #808080;"">&nbsp;</div>"
                    '
                    ' Set Column Link
                    '
                    QS = RQS
                    QS = genericController.ModifyQueryString(QS, RequestNameAdminSubForm, AdminFormIndex_SubFormSetColumns, True)
                    Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                    returnContent &= "<div class=""ccFilterHead""><a class=""ccFilterLink"" href=""" & Link & """>Set&nbsp;Columns</a></div>"
                    '
                    returnContent &= "<div style=""border-bottom:1px dotted #808080;"">&nbsp;</div>"
                    '
                    ' Import Link
                    '
                    QS = RQS
                    QS = genericController.ModifyQueryString(QS, RequestNameAdminForm, AdminFormImportWizard, True)
                    Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                    returnContent &= "<div class=""ccFilterHead""><a class=""ccFilterLink"" href=""" & Link & """>Import</a></div>"
                    '
                    returnContent &= "<div style=""border-bottom:1px dotted #808080;"">&nbsp;</div>"
                    '
                    ' Export Link
                    '
                    QS = RQS
                    QS = genericController.ModifyQueryString(QS, RequestNameAdminSubForm, AdminFormIndex_SubFormExport, True)
                    Link = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" & QS
                    returnContent &= "<div class=""ccFilterHead""><a class=""ccFilterLink"" href=""" & Link & """>Export</a></div>"
                    '
                    returnContent &= "<div style=""border-bottom:1px dotted #808080;"">&nbsp;</div>"
                    '
                    returnContent = "<div style=""padding-left:10px;padding-right:10px;"">" & returnContent & "</div>"
                End With
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnContent
        End Function
        '
    End Class
End Namespace
