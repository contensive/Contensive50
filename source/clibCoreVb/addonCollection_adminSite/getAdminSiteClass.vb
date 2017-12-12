
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
    Public Class getAdminSiteClass
        Inherits Contensive.BaseClasses.AddonBaseClass
        '
        '=================================================================================
        '   Load the index configig
        '       if it is empty, setup defaults
        '=================================================================================
        '
        Private Function LoadIndexConfig(adminContent As Models.Complex.cdefModel) As indexConfigClass
            Dim returnIndexConfig As New indexConfigClass
            Try
                '
                Dim ConfigListLine As String
                Dim Line As String
                Dim Ptr As Integer
                Dim ConfigList As String
                Dim ConfigListLines() As String
                Dim LineSplit() As String
                '
                With returnIndexConfig
                    '
                    ' Setup defaults
                    '
                    .ContentID = adminContent.Id
                    .ActiveOnly = False
                    .LastEditedByMe = False
                    .LastEditedToday = False
                    .LastEditedPast7Days = False
                    .LastEditedPast30Days = False
                    .Loaded = True
                    .Open = False
                    .PageNumber = 1
                    .RecordsPerPage = RecordsPerPageDefault
                    .RecordTop = 0
                    '
                    ' Setup Member Properties
                    '
                    ConfigList = cpCore.userProperty.getText(IndexConfigPrefix & CStr(adminContent.Id), "")
                    If ConfigList <> "" Then
                        '
                        ' load values
                        '
                        ConfigList = ConfigList & vbCrLf
                        ConfigListLines = genericController.SplitCRLF(ConfigList)
                        Ptr = 0
                        Do While Ptr < UBound(ConfigListLines)
                            '
                            ' check next line
                            '
                            ConfigListLine = genericController.vbLCase(ConfigListLines(Ptr))
                            If ConfigListLine <> "" Then
                                Select Case ConfigListLine
                                    Case "columns"
                                        Ptr = Ptr + 1
                                        Do While ConfigListLines(Ptr) <> ""
                                            Line = ConfigListLines(Ptr)
                                            LineSplit = Split(Line, vbTab, 2)
                                            If UBound(LineSplit) > 0 Then
                                                Dim column As New indexConfigColumnClass
                                                column.Name = LineSplit(0).Trim()
                                                column.Width = genericController.EncodeInteger(LineSplit(1))
                                                .Columns.Add(column.Name.ToLower(), column)
                                            End If
                                            Ptr = Ptr + 1
                                        Loop
                                    Case "sorts"
                                        Ptr = Ptr + 1
                                        Do While ConfigListLines(Ptr) <> ""
                                            'ReDim Preserve .Sorts(.SortCnt)
                                            Line = ConfigListLines(Ptr)
                                            LineSplit = Split(Line, vbTab, 2)
                                            If UBound(LineSplit) = 1 Then
                                                .Sorts.Add(LineSplit(0).ToLower, New indexConfigSortClass With {
                                                    .fieldName = LineSplit(0),
                                                    .direction = If(genericController.EncodeBoolean(LineSplit(1)), 1, 2)
                                                })
                                            End If
                                            Ptr = Ptr + 1
                                        Loop
                                End Select
                            End If
                            Ptr = Ptr + 1
                        Loop
                        If .RecordsPerPage <= 0 Then
                            .RecordsPerPage = RecordsPerPageDefault
                        End If
                        '.PageNumber = 1 + Int(.RecordTop / .RecordsPerPage)
                    End If
                    '
                    ' Setup Visit Properties
                    '
                    ConfigList = cpCore.visitProperty.getText(IndexConfigPrefix & CStr(adminContent.Id), "")
                    If ConfigList <> "" Then
                        '
                        ' load values
                        '
                        ConfigList = ConfigList & vbCrLf
                        ConfigListLines = genericController.SplitCRLF(ConfigList)
                        Ptr = 0
                        Do While Ptr < UBound(ConfigListLines)
                            '
                            ' check next line
                            '
                            ConfigListLine = genericController.vbLCase(ConfigListLines(Ptr))
                            If ConfigListLine <> "" Then
                                Select Case ConfigListLine
                                    Case "findwordlist"
                                        Ptr = Ptr + 1
                                        Do While ConfigListLines(Ptr) <> ""
                                            'ReDim Preserve .FindWords(.FindWords.Count)
                                            Line = ConfigListLines(Ptr)
                                            LineSplit = Split(Line, vbTab)
                                            If UBound(LineSplit) > 1 Then
                                                Dim findWord As New indexConfigFindWordClass
                                                findWord.Name = LineSplit(0)
                                                findWord.Value = LineSplit(1)
                                                findWord.MatchOption = DirectCast(genericController.EncodeInteger(LineSplit(2)), FindWordMatchEnum)
                                                .FindWords.Add(findWord.Name, findWord)
                                            End If
                                            Ptr = Ptr + 1
                                        Loop
                                    Case "grouplist"
                                        Ptr = Ptr + 1
                                        Do While ConfigListLines(Ptr) <> ""
                                            ReDim Preserve .GroupList(.GroupListCnt)
                                            .GroupList(.GroupListCnt) = ConfigListLines(Ptr)
                                            .GroupListCnt = .GroupListCnt + 1
                                            Ptr = Ptr + 1
                                        Loop
                                    Case "cdeflist"
                                        Ptr = Ptr + 1
                                        .SubCDefID = genericController.EncodeInteger(ConfigListLines(Ptr))
                                    Case "indexfiltercategoryid"
                                        ' -- remove deprecated value
                                        Ptr = Ptr + 1
                                        Dim ignore As Integer = genericController.EncodeInteger(ConfigListLines(Ptr))
                                    Case "indexfilteractiveonly"
                                        .ActiveOnly = True
                                    Case "indexfilterlasteditedbyme"
                                        .LastEditedByMe = True
                                    Case "indexfilterlasteditedtoday"
                                        .LastEditedToday = True
                                    Case "indexfilterlasteditedpast7days"
                                        .LastEditedPast7Days = True
                                    Case "indexfilterlasteditedpast30days"
                                        .LastEditedPast30Days = True
                                    Case "indexfilteropen"
                                        .Open = True
                                    Case "recordsperpage"
                                        Ptr = Ptr + 1
                                        .RecordsPerPage = genericController.EncodeInteger(ConfigListLines(Ptr))
                                        If .RecordsPerPage <= 0 Then
                                            .RecordsPerPage = 50
                                        End If
                                        .RecordTop = ((.PageNumber - 1) * .RecordsPerPage)
                                    Case "pagenumber"
                                        Ptr = Ptr + 1
                                        .PageNumber = genericController.EncodeInteger(ConfigListLines(Ptr))
                                        If .PageNumber <= 0 Then
                                            .PageNumber = 1
                                        End If
                                        .RecordTop = ((.PageNumber - 1) * .RecordsPerPage)
                                End Select
                            End If
                            Ptr = Ptr + 1
                        Loop
                        If .RecordsPerPage <= 0 Then
                            .RecordsPerPage = RecordsPerPageDefault
                        End If
                        '.PageNumber = 1 + Int(.RecordTop / .RecordsPerPage)
                    End If
                    '
                    ' Setup defaults if not loaded
                    '
                    If (.Columns.Count = 0) And (adminContent.adminColumns.Count > 0) Then
                        '.Columns.Count = adminContent.adminColumns.Count
                        'ReDim .Columns(.Columns.Count - 1)
                        'Ptr = 0
                        For Each keyValuepair In adminContent.adminColumns
                            Dim cdefAdminColumn As Models.Complex.cdefModel.CDefAdminColumnClass = keyValuepair.Value
                            Dim column As New indexConfigColumnClass
                            column.Name = cdefAdminColumn.Name
                            column.Width = cdefAdminColumn.Width
                            returnIndexConfig.Columns.Add(column.Name.ToLower(), column)
                        Next
                    End If
                    '
                    ' Set field pointers for columns and sorts
                    '
                    ' dont knwo what this does
                    'For Each keyValuePair As KeyValuePair(Of String, appServices_metaDataClass.CDefFieldClass) In adminContent.fields
                    '    Dim field As appServices_metaDataClass.CDefFieldClass = keyValuePair.Value
                    '    If .Columns.Count > 0 Then
                    '        For Ptr = 0 To .Columns.Count - 1
                    '            With .Columns(Ptr)
                    '                If genericController.vbLCase(.Name) = field.Name.ToLower Then
                    '                    .FieldId = SrcPtr
                    '                    Exit For
                    '                End If
                    '            End With
                    '        Next
                    '    End If
                    '    '
                    '    If .SortCnt > 0 Then
                    '        For Ptr = 0 To .SortCnt - 1
                    '            With .Sorts(Ptr)
                    '                If genericController.vbLCase(.FieldName) = field.Name Then
                    '                    .FieldPtr = SrcPtr
                    '                    Exit For
                    '                End If
                    '            End With
                    '        Next
                    '    End If
                    'Next
                    '        '
                    '        ' set Column Field Ptr for later
                    '        '
                    '        If .columns.count > 0 Then
                    '            For Ptr = 0 To .columns.count - 1
                    '                With .Columns(Ptr)
                    '                    For SrcPtr = 0 To AdminContent.fields.count - 1
                    '                        If .Name = AdminContent.fields(SrcPtr).Name Then
                    '                            .FieldPointer = SrcPtr
                    '                            Exit For
                    '                        End If
                    '                    Next
                    '                End With
                    '            Next
                    '        End If
                    '        '
                    '        ' set Sort Field Ptr for later
                    '        '
                    '        If .SortCnt > 0 Then
                    '            For Ptr = 0 To .SortCnt - 1
                    '                With .Sorts(Ptr)
                    '                    For SrcPtr = 0 To AdminContent.fields.count - 1
                    '                        If genericController.vbLCase(.FieldName) = genericController.vbLCase(AdminContent.fields(SrcPtr).Name) Then
                    '                            .FieldPtr = SrcPtr
                    '                            Exit For
                    '                        End If
                    '                    Next
                    '                End With
                    '            Next
                    '        End If
                End With
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnIndexConfig
        End Function
        '
        '========================================================================================
        '   Process request input on the IndexConfig
        '========================================================================================
        '
        Private Sub SetIndexSQL_ProcessIndexConfigRequests(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, ByRef IndexConfig As indexConfigClass)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("ProcessIndexConfigRequests")
            '
            Dim TestInteger As Integer
            Dim MatchOption As Integer
            Dim FindWordPtr As Integer
            Dim FormFieldCnt As Integer
            Dim FormFieldPtr As Integer
            Dim ContentFields() As indexConfigFindWordClass
            Dim NumericOption As String
            Dim fieldType As Integer
            Dim FieldValue As String
            Dim FieldName As String

            Dim CS As Integer
            Dim Criteria As String
            Dim VarText As String
            Dim FindName As String
            Dim FindValue As String
            Dim Ptr As Integer
            Dim ColumnCnt As Integer
            Dim ColumnPtr As Integer
            Dim Button As String
            ''Dim arrayOfFields() As appServices_metaDataClass.CDefFieldClass
            '
            'arrayOfFields = adminContent.fields
            With IndexConfig
                If Not .Loaded Then
                    IndexConfig = LoadIndexConfig(adminContent)
                End If
                '
                ' ----- Page number
                '
                VarText = cpCore.docProperties.getText("rt")
                If VarText <> "" Then
                    .RecordTop = genericController.EncodeInteger(VarText)
                End If
                '
                VarText = cpCore.docProperties.getText("RS")
                If VarText <> "" Then
                    .RecordsPerPage = genericController.EncodeInteger(VarText)
                End If
                If .RecordsPerPage <= 0 Then
                    .RecordsPerPage = RecordsPerPageDefault
                End If
                .PageNumber = CInt(1 + Int(.RecordTop / .RecordsPerPage))
                '
                ' ----- Process indexGoToPage value
                '
                TestInteger = cpCore.docProperties.getInteger("indexGoToPage")
                If TestInteger > 0 Then
                    .PageNumber = TestInteger
                    .RecordTop = ((.PageNumber - 1) * .RecordsPerPage)
                Else
                    '
                    ' ----- Read filter changes and First/Next/Previous from form
                    '
                    Button = cpCore.docProperties.getText(RequestNameButton)
                    If Button <> "" Then
                        Select Case AdminButton
                            Case ButtonFirst
                                '
                                ' Force to first page
                                '
                                .PageNumber = 1
                                .RecordTop = ((.PageNumber - 1) * .RecordsPerPage)
                            Case ButtonNext
                                '
                                ' Go to next page
                                '
                                .PageNumber = .PageNumber + 1
                                .RecordTop = ((.PageNumber - 1) * .RecordsPerPage)
                            Case ButtonPrevious
                                '
                                ' Go to previous page
                                '
                                .PageNumber = .PageNumber - 1
                                If .PageNumber <= 0 Then
                                    .PageNumber = 1
                                End If
                                .RecordTop = ((.PageNumber - 1) * .RecordsPerPage)
                            Case ButtonFind
                                '
                                ' Find (change search criteria and go to first page)
                                '
                                .PageNumber = 1
                                .RecordTop = ((.PageNumber - 1) * .RecordsPerPage)
                                ColumnCnt = cpCore.docProperties.getInteger("ColumnCnt")
                                If (ColumnCnt > 0) Then
                                    For ColumnPtr = 0 To ColumnCnt - 1
                                        FindName = cpCore.docProperties.getText("FindName" & ColumnPtr).ToLower
                                        If (Not String.IsNullOrEmpty(FindName)) Then
                                            If (adminContent.fields.ContainsKey(FindName.ToLower)) Then
                                                FindValue = Trim(cpCore.docProperties.getText("FindValue" & ColumnPtr))
                                                If (String.IsNullOrEmpty(FindValue)) Then
                                                    '
                                                    ' -- find blank, if name in list, remove it
                                                    If (.FindWords.ContainsKey(FindName)) Then
                                                        .FindWords.Remove(FindName)
                                                    End If
                                                Else
                                                    '
                                                    ' -- nonblank find, store it
                                                    If (.FindWords.ContainsKey(FindName)) Then
                                                        .FindWords.Item(FindName).Value = FindValue
                                                    Else
                                                        Dim field As Models.Complex.CDefFieldModel = adminContent.fields(FindName.ToLower)
                                                        Dim findWord As New indexConfigFindWordClass
                                                        findWord.Name = FindName
                                                        findWord.Value = FindValue
                                                        Select Case field.fieldTypeId
                                                            Case FieldTypeIdAutoIdIncrement, FieldTypeIdCurrency, FieldTypeIdFloat, FieldTypeIdInteger, FieldTypeIdLookup, FieldTypeIdMemberSelect
                                                                findWord.MatchOption = FindWordMatchEnum.MatchEquals
                                                            Case FieldTypeIdDate
                                                                findWord.MatchOption = FindWordMatchEnum.MatchEquals
                                                            Case FieldTypeIdBoolean
                                                                findWord.MatchOption = FindWordMatchEnum.MatchEquals
                                                            Case Else
                                                                findWord.MatchOption = FindWordMatchEnum.matchincludes
                                                        End Select
                                                        .FindWords.Add(FindName, findWord)
                                                    End If
                                                End If
                                            End If
                                        End If
                                    Next
                                End If
                        End Select
                    End If
                End If
                '
                ' Process Filter form
                '
                If cpCore.docProperties.getBoolean("IndexFilterRemoveAll") Then
                    '
                    ' Remove all filters
                    '
                    .FindWords = New Dictionary(Of String, indexConfigFindWordClass)
                    .GroupListCnt = 0
                    .SubCDefID = 0
                    .ActiveOnly = False
                    .LastEditedByMe = False
                    .LastEditedToday = False
                    .LastEditedPast7Days = False
                    .LastEditedPast30Days = False
                Else
                    Dim VarInteger As Integer
                    '
                    ' Add CDef
                    '
                    VarInteger = cpCore.docProperties.getInteger("IndexFilterAddCDef")
                    If VarInteger <> 0 Then
                        .SubCDefID = VarInteger
                        .PageNumber = 1
                        '                If .SubCDefCnt > 0 Then
                        '                    For Ptr = 0 To .SubCDefCnt - 1
                        '                        If VarInteger = .SubCDefs(Ptr) Then
                        '                            Exit For
                        '                        End If
                        '                    Next
                        '                End If
                        '                If Ptr = .SubCDefCnt Then
                        '                    ReDim Preserve .SubCDefs(.SubCDefCnt)
                        '                    .SubCDefs(.SubCDefCnt) = VarInteger
                        '                    .SubCDefCnt = .SubCDefCnt + 1
                        '                    .PageNumber = 1
                        '                End If
                    End If
                    '
                    ' Remove CDef
                    '
                    VarInteger = cpCore.docProperties.getInteger("IndexFilterRemoveCDef")
                    If VarInteger <> 0 Then
                        .SubCDefID = 0
                        .PageNumber = 1
                        '                If .SubCDefCnt > 0 Then
                        '                    For Ptr = 0 To .SubCDefCnt - 1
                        '                        If .SubCDefs(Ptr) = VarInteger Then
                        '                            .SubCDefs(Ptr) = 0
                        '                            .PageNumber = 1
                        '                            Exit For
                        '                        End If
                        '                    Next
                        '                End If
                    End If
                    '
                    ' Add Groups
                    '
                    VarText = cpCore.docProperties.getText("IndexFilterAddGroup").ToLower()
                    If VarText <> "" Then
                        If .GroupListCnt > 0 Then
                            For Ptr = 0 To .GroupListCnt - 1
                                If VarText = .GroupList(Ptr) Then
                                    Exit For
                                End If
                            Next
                        End If
                        If Ptr = .GroupListCnt Then
                            ReDim Preserve .GroupList(.GroupListCnt)
                            .GroupList(.GroupListCnt) = VarText
                            .GroupListCnt = .GroupListCnt + 1
                            .PageNumber = 1
                        End If
                    End If
                    '
                    ' Remove Groups
                    '
                    VarText = cpCore.docProperties.getText("IndexFilterRemoveGroup").ToLower()
                    If VarText <> "" Then
                        If .GroupListCnt > 0 Then
                            For Ptr = 0 To .GroupListCnt - 1
                                If .GroupList(Ptr) = VarText Then
                                    .GroupList(Ptr) = ""
                                    .PageNumber = 1
                                    Exit For
                                End If
                            Next
                        End If
                    End If
                    '
                    ' Remove FindWords
                    '
                    VarText = cpCore.docProperties.getText("IndexFilterRemoveFind").ToLower()
                    If VarText <> "" Then
                        If .FindWords.ContainsKey(VarText) Then
                            .FindWords.Remove(VarText)
                        End If
                        'If .FindWords.Count > 0 Then
                        '    For Ptr = 0 To .FindWords.Count - 1
                        '        If .FindWords(Ptr).Name = VarText Then
                        '            .FindWords(Ptr).MatchOption = FindWordMatchEnum.MatchIgnore
                        '            .FindWords(Ptr).Value = ""
                        '            .PageNumber = 1
                        '            Exit For
                        '        End If
                        '    Next
                        'End If
                    End If
                    '
                    ' Read ActiveOnly
                    '
                    VarText = cpCore.docProperties.getText("IndexFilterActiveOnly")
                    If VarText <> "" Then
                        .ActiveOnly = genericController.EncodeBoolean(VarText)
                        .PageNumber = 1
                    End If
                    '
                    ' Read LastEditedByMe
                    '
                    VarText = cpCore.docProperties.getText("IndexFilterLastEditedByMe")
                    If VarText <> "" Then
                        .LastEditedByMe = genericController.EncodeBoolean(VarText)
                        .PageNumber = 1
                    End If
                    '
                    ' Last Edited Past 30 Days
                    '
                    VarText = cpCore.docProperties.getText("IndexFilterLastEditedPast30Days")
                    If VarText <> "" Then
                        .LastEditedPast30Days = genericController.EncodeBoolean(VarText)
                        .LastEditedPast7Days = False
                        .LastEditedToday = False
                        .PageNumber = 1
                    Else
                        '
                        ' Past 7 Days
                        '
                        VarText = cpCore.docProperties.getText("IndexFilterLastEditedPast7Days")
                        If VarText <> "" Then
                            .LastEditedPast30Days = False
                            .LastEditedPast7Days = genericController.EncodeBoolean(VarText)
                            .LastEditedToday = False
                            .PageNumber = 1
                        Else
                            '
                            ' Read LastEditedToday
                            '
                            VarText = cpCore.docProperties.getText("IndexFilterLastEditedToday")
                            If VarText <> "" Then
                                .LastEditedPast30Days = False
                                .LastEditedPast7Days = False
                                .LastEditedToday = genericController.EncodeBoolean(VarText)
                                .PageNumber = 1
                            End If
                        End If
                    End If
                    '
                    ' Read IndexFilterOpen
                    '
                    VarText = cpCore.docProperties.getText("IndexFilterOpen")
                    If VarText <> "" Then
                        .Open = genericController.EncodeBoolean(VarText)
                        .PageNumber = 1
                    End If
                    '
                    ' SortField
                    '
                    VarText = cpCore.docProperties.getText("SetSortField").ToLower()
                    If VarText <> "" Then
                        If .Sorts.ContainsKey(VarText) Then
                            .Sorts.Remove(VarText)
                        End If
                        Dim sortDirection As Integer = cpCore.docProperties.getInteger("SetSortDirection")
                        If (sortDirection > 0) Then
                            .Sorts.Add(VarText, New indexConfigSortClass With {.fieldName = VarText, .direction = sortDirection})
                        End If
                    End If
                    '
                    ' Build FindWordList
                    '
                    '.FindWordList = ""
                    'If .findwords.count > 0 Then
                    '    For Ptr = 0 To .findwords.count - 1
                    '        If .FindWords(Ptr).Value <> "" Then
                    '            .FindWordList = .FindWordList & vbCrLf & .FindWords(Ptr).Name & "=" & .FindWords(Ptr).Value
                    '        End If
                    '    Next
                    'End If
                End If
                '            Criteria = "(active<>0)and(ContentID=" & cpCore.main_GetContentID("people") & ")and(authorable<>0)"
                '            CS = cpCore.app.csOpen("Content Fields", Criteria, "EditSortPriority")
                '            Do While cpCore.app.csv_IsCSOK(CS)
                '                FieldName = cpCore.db.cs_getText(CS, "name")
                '                FieldValue = cpCore.main_GetStreamText2(FieldName)
                '                FieldType = cpCore.app.cs_getInteger(CS, "Type")
                '                Select Case FieldType
                '                    Case FieldTypeCurrency, FieldTypeFloat, FieldTypeInteger
                '                        NumericOption = cpCore.main_GetStreamText2(FieldName & "_N")
                '                        If NumericOption <> "" Then
                '                            '.FindWords(0).MatchOption = 1
                '                            ContactSearchCriteria = ContactSearchCriteria _
                '                                & vbCrLf _
                '                                & FieldName & vbTab _
                '                                & FieldType & vbTab _
                '                                & FieldValue & vbTab _
                '                                & NumericOption
                '                        End If
                '                    Case FieldTypeBoolean
                '                        If FieldValue <> "" Then
                '                            ContactSearchCriteria = ContactSearchCriteria _
                '                                & vbCrLf _
                '                                & FieldName & vbTab _
                '                                & FieldType & vbTab _
                '                                & FieldValue & vbTab _
                '                                & ""
                '                        End If
                '                    Case FieldTypeText
                '                        TextOption = cpCore.main_GetStreamText2(FieldName & "_T")
                '                        If TextOption <> "" Then
                '                            ContactSearchCriteria = ContactSearchCriteria _
                '                                & vbCrLf _
                '                                & FieldName & vbTab _
                '                                & CStr(FieldType) & vbTab _
                '                                & FieldValue & vbTab _
                '                                & TextOption
                '                        End If
                '                    Case FieldTypeLookup
                '                        If FieldValue <> "" Then
                '                            ContactSearchCriteria = ContactSearchCriteria _
                '                                & vbCrLf _
                '                                & FieldName & vbTab _
                '                                & FieldType & vbTab _
                '                                & FieldValue & vbTab _
                '                                & ""
                '                        End If
                '                End Select
                '                Call cpCore.app.nextCSRecord(CS)
                '            Loop
                '            Call cpCore.app.closeCS(CS)
                '            Call cpCore.main_SetMemberProperty("ContactSearchCriteria", ContactSearchCriteria)
                '        End If


                '
                ' Set field pointers for columns and sorts
                '
                'Dim SrcPtr As Integer
                'If .Columns.Count > 0 Or .SortCnt > 0 Then
                '    For Each keyValuePair As KeyValuePair(Of String, appServices_metaDataClass.CDefFieldClass) In adminContent.fields
                '        Dim field As appServices_metaDataClass.CDefFieldClass = keyValuePair.Value
                '        If .Columns.Count > 0 Then
                '            For Ptr = 0 To .Columns.Count - 1
                '                With .Columns(Ptr)
                '                    If genericController.vbLCase(.Name) = field.Name Then
                '                        .FieldId = SrcPtr
                '                        Exit For
                '                    End If
                '                End With
                '            Next
                '        End If
                '        '
                '        If .SortCnt > 0 Then
                '            For Ptr = 0 To .SortCnt - 1
                '                With .Sorts(Ptr)
                '                    If genericController.vbLCase(.FieldName) = field.Name Then
                '                        .FieldPtr = SrcPtr
                '                        Exit For
                '                    End If
                '                End With
                '            Next
                '        End If
                '    Next
                'End If
            End With
            '
            Exit Sub
ErrorTrap:
            Call handleLegacyClassError3("ProcessIndexConfigRequests")
        End Sub
        '
        '=================================================================================
        '
        '=================================================================================
        '
        Private Sub SetIndexSQL_SaveIndexConfig(ByVal IndexConfig As indexConfigClass)
            '
            Dim FilterText As String
            Dim SubList As String
            Dim Ptr As Integer
            '
            ' ----- Save filter state to the visit property
            '
            With IndexConfig
                '
                ' -----------------------------------------------------------------------------------------------
                '   Visit Properties (non-persistant)
                ' -----------------------------------------------------------------------------------------------
                '
                FilterText = ""
                '
                ' Find words
                '
                SubList = ""
                For Each kvp In .FindWords
                    Dim findWord As indexConfigFindWordClass = kvp.Value
                    If (findWord.Name <> "") And (findWord.MatchOption <> FindWordMatchEnum.MatchIgnore) Then
                        SubList = SubList & vbCrLf & findWord.Name & vbTab & findWord.Value & vbTab & findWord.MatchOption
                    End If
                Next
                If SubList <> "" Then
                    FilterText = FilterText & vbCrLf & "FindWordList" & SubList & vbCrLf
                End If
                '
                ' CDef List
                '
                If .SubCDefID > 0 Then
                    FilterText = FilterText & vbCrLf & "CDefList" & vbCrLf & .SubCDefID & vbCrLf
                End If
                '        SubList = ""
                '        If .SubCDefCnt > 0 Then
                '            For Ptr = 0 To .SubCDefCnt - 1
                '                If .SubCDefs(Ptr) <> 0 Then
                '                    SubList = SubList & vbCrLf & .SubCDefs(Ptr)
                '                End If
                '            Next
                '        End If
                '        If SubList <> "" Then
                '            FilterText = FilterText & vbCrLf & "CDefList" & SubList & vbCrLf
                '        End If
                '
                ' Group List
                '
                SubList = ""
                If .GroupListCnt > 0 Then
                    For Ptr = 0 To .GroupListCnt - 1
                        If .GroupList(Ptr) <> "" Then
                            SubList = SubList & vbCrLf & .GroupList(Ptr)
                        End If
                    Next
                End If
                If SubList <> "" Then
                    FilterText = FilterText & vbCrLf & "GroupList" & SubList & vbCrLf
                End If
                '
                ' PageNumber and Records Per Page
                '
                FilterText = FilterText _
                    & vbCrLf & "" _
                    & vbCrLf & "pagenumber" _
                    & vbCrLf & .PageNumber
                FilterText = FilterText _
                    & vbCrLf & "" _
                    & vbCrLf & "recordsperpage" _
                    & vbCrLf & .RecordsPerPage
                '
                ' misc filters
                '
                If .ActiveOnly Then
                    FilterText = FilterText _
                        & vbCrLf & "" _
                        & vbCrLf & "IndexFilterActiveOnly"
                End If
                If .LastEditedByMe Then
                    FilterText = FilterText _
                        & vbCrLf & "" _
                        & vbCrLf & "IndexFilterLastEditedByMe"
                End If
                If .LastEditedToday Then
                    FilterText = FilterText _
                        & vbCrLf & "" _
                        & vbCrLf & "IndexFilterLastEditedToday"
                End If
                If .LastEditedPast7Days Then
                    FilterText = FilterText _
                        & vbCrLf & "" _
                        & vbCrLf & "IndexFilterLastEditedPast7Days"
                End If
                If .LastEditedPast30Days Then
                    FilterText = FilterText _
                        & vbCrLf & "" _
                        & vbCrLf & "IndexFilterLastEditedPast30Days"
                End If
                If .Open Then
                    FilterText = FilterText _
                        & vbCrLf & "" _
                        & vbCrLf & "IndexFilterOpen"
                End If
                '
                Call cpCore.visitProperty.setProperty(IndexConfigPrefix & CStr(.ContentID), FilterText)
                '
                ' -----------------------------------------------------------------------------------------------
                '   Member Properties (persistant)
                ' -----------------------------------------------------------------------------------------------
                '
                FilterText = ""
                '
                ' Save Admin Column
                '
                SubList = ""
                For Each kvp In .Columns
                    Dim column As indexConfigColumnClass = kvp.Value
                    If column.Name <> "" Then
                        SubList = SubList & vbCrLf & column.Name & vbTab & column.Width
                    End If
                Next
                If SubList <> "" Then
                    FilterText = FilterText & vbCrLf & "Columns" & SubList & vbCrLf
                End If
                '
                ' Sorts
                '
                SubList = ""
                For Each kvp In .Sorts
                    Dim sort As indexConfigSortClass = kvp.Value
                    If sort.fieldName <> "" Then
                        SubList = SubList & vbCrLf & sort.fieldName & vbTab & sort.direction
                    End If
                Next
                If SubList <> "" Then
                    FilterText = FilterText & vbCrLf & "Sorts" & SubList & vbCrLf
                End If
                Call cpCore.userProperty.setProperty(IndexConfigPrefix & CStr(.ContentID), FilterText)
            End With
            '

        End Sub
        '
        '
        '
        Private Function GetFormInputWithFocus2(ByVal ElementName As String, Optional ByVal CurrentValue As String = "", Optional ByVal Height As Integer = -1, Optional ByVal Width As Integer = -1, Optional ByVal ElementID As String = "", Optional ByVal OnFocusJavascript As String = "", Optional ByVal HtmlClass As String = "") As String
            GetFormInputWithFocus2 = cpCore.html.html_GetFormInputText2(ElementName, CurrentValue, Height, Width, ElementID)
            If OnFocusJavascript <> "" Then
                GetFormInputWithFocus2 = genericController.vbReplace(GetFormInputWithFocus2, ">", " onFocus=""" & OnFocusJavascript & """>")
            End If
            If HtmlClass <> "" Then
                GetFormInputWithFocus2 = genericController.vbReplace(GetFormInputWithFocus2, ">", " class=""" & HtmlClass & """>")
            End If
        End Function
        '
        '
        '
        Private Function GetFormInputWithFocus(ByVal ElementName As String, ByVal CurrentValue As String, ByVal Height As Integer, ByVal Width As Integer, ByVal ElementID As String, ByVal OnFocus As String) As String
            GetFormInputWithFocus = GetFormInputWithFocus2(ElementName, CurrentValue, Height, Width, ElementID, OnFocus)
        End Function
        '
        '
        '
        Private Function GetFormInputDateWithFocus2(ByVal ElementName As String, Optional ByVal CurrentValue As String = "", Optional ByVal Width As String = "", Optional ByVal ElementID As String = "", Optional ByVal OnFocusJavascript As String = "", Optional ByVal HtmlClass As String = "") As String
            GetFormInputDateWithFocus2 = cpCore.html.html_GetFormInputDate(ElementName, CurrentValue, Width, ElementID)
            If OnFocusJavascript <> "" Then
                GetFormInputDateWithFocus2 = genericController.vbReplace(GetFormInputDateWithFocus2, ">", " onFocus=""" & OnFocusJavascript & """>")
            End If
            If HtmlClass <> "" Then
                GetFormInputDateWithFocus2 = genericController.vbReplace(GetFormInputDateWithFocus2, ">", " class=""" & HtmlClass & """>")
            End If
        End Function
        '
        '
        '
        Private Function GetFormInputDateWithFocus(ByVal ElementName As String, ByVal CurrentValue As String, ByVal Width As String, ByVal ElementID As String, ByVal OnFocus As String) As String
            GetFormInputDateWithFocus = GetFormInputDateWithFocus2(ElementName, CurrentValue, Width, ElementID, OnFocus)
        End Function
        '
        '=================================================================================
        '
        '=================================================================================
        '
        Private Function GetForm_Index_AdvancedSearch(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
            Dim returnForm As String = ""
            Try
                '
                Dim SearchValue As String
                Dim MatchOption As FindWordMatchEnum
                Dim FormFieldPtr As Integer
                Dim FormFieldCnt As Integer
                Dim CDef As Models.Complex.cdefModel
                Dim FieldName As String
                Dim Stream As New stringBuilderLegacyController
                Dim FieldPtr As Integer
                Dim RowEven As Boolean
                Dim Button As String
                Dim RQS As String
                Dim FieldNames() As String
                Dim FieldCaption() As String
                Dim fieldId() As Integer
                Dim fieldTypeId() As Integer
                Dim FieldValue() As String
                Dim FieldMatchOptions() As Integer
                Dim FieldMatchOption As Integer
                Dim FieldLookupContentName() As String
                Dim FieldLookupList() As String
                Dim ContentID As Integer
                Dim FieldCnt As Integer
                Dim FieldSize As Integer
                Dim RowPointer As Integer
                Dim Adminui As New adminUIController(cpCore)
                Dim LeftButtons As String = ""
                Dim ButtonBar As String
                Dim Title As String
                Dim TitleBar As String
                Dim Content As String
                Dim TitleDescription As String
                Dim IndexConfig As indexConfigClass
                '
                If Not (False) Then
                    '
                    ' Process last form
                    '
                    Button = cpCore.docProperties.getText("button")
                    If Button <> "" Then
                        Select Case Button
                            Case ButtonSearch
                                IndexConfig = LoadIndexConfig(adminContent)
                                With IndexConfig
                                    FormFieldCnt = cpCore.docProperties.getInteger("fieldcnt")
                                    If FormFieldCnt > 0 Then
                                        For FormFieldPtr = 0 To FormFieldCnt - 1
                                            FieldName = genericController.vbLCase(cpCore.docProperties.getText("fieldname" & FormFieldPtr))
                                            MatchOption = DirectCast(cpCore.docProperties.getInteger("FieldMatch" & FormFieldPtr), FindWordMatchEnum)
                                            Select Case MatchOption
                                                Case FindWordMatchEnum.MatchEquals, FindWordMatchEnum.MatchGreaterThan, FindWordMatchEnum.matchincludes, FindWordMatchEnum.MatchLessThan
                                                    SearchValue = cpCore.docProperties.getText("FieldValue" & FormFieldPtr)
                                                Case Else
                                                    SearchValue = ""
                                            End Select
                                            If Not .FindWords.ContainsKey(FieldName) Then
                                                '
                                                ' fieldname not found, save if not FindWordMatchEnum.MatchIgnore
                                                '
                                                If MatchOption <> FindWordMatchEnum.MatchIgnore Then
                                                    Dim findWord As New indexConfigFindWordClass
                                                    findWord.Name = FieldName
                                                    findWord.MatchOption = MatchOption
                                                    findWord.Value = SearchValue
                                                    .FindWords.Add(FieldName, findWord)
                                                End If
                                            Else
                                                '
                                                ' fieldname was found
                                                '
                                                .FindWords.Item(FieldName).MatchOption = MatchOption
                                                .FindWords.Item(FieldName).Value = SearchValue
                                            End If
                                        Next
                                    End If
                                End With
                                Call SetIndexSQL_SaveIndexConfig(IndexConfig)
                                Return String.Empty
                            Case ButtonCancel
                                Return String.Empty
                        End Select
                    End If
                    IndexConfig = LoadIndexConfig(adminContent)
                    Button = "CriteriaSelect"
                    RQS = cpCore.doc.refreshQueryString
                    '
                    ' ----- ButtonBar
                    '
                    If MenuDepth > 0 Then
                        LeftButtons &= cpCore.html.html_GetFormButton(ButtonClose, , , "window.close();")
                    Else
                        LeftButtons &= cpCore.html.html_GetFormButton(ButtonCancel)
                        'LeftButtons &= cpCore.main_GetFormButton(ButtonCancel, , , "return processSubmit(this)")
                    End If
                    LeftButtons &= cpCore.html.html_GetFormButton(ButtonSearch)
                    'LeftButtons &= cpCore.main_GetFormButton(ButtonSearch, , , "return processSubmit(this)")
                    ButtonBar = Adminui.GetButtonBar(LeftButtons, "")
                    '
                    ' ----- TitleBar
                    '
                    Title = adminContent.Name
                    Title = Title & " Advanced Search"
                    Title = "<strong>" & Title & "</strong>"
                    Title = SpanClassAdminNormal & Title & "</span>"
                    'Title = Title & cpCore.main_GetHelpLink(46, "Using the Advanced Search Page", BubbleCopy_AdminIndexPage)
                    TitleDescription = "<div>Enter criteria for each field to identify and select your results. The results of a search will have to have all of the criteria you enter.</div>"
                    TitleBar = Adminui.GetTitleBar(Title, TitleDescription)
                    '
                    ' ----- List out all fields
                    '
                    CDef = Models.Complex.cdefModel.getCdef(cpCore, adminContent.Name)
                    FieldSize = 100
                    ReDim Preserve FieldNames(FieldSize)
                    ReDim Preserve FieldCaption(FieldSize)
                    ReDim Preserve fieldId(FieldSize)
                    ReDim Preserve fieldTypeId(FieldSize)
                    ReDim Preserve FieldValue(FieldSize)
                    ReDim Preserve FieldMatchOptions(FieldSize)
                    ReDim Preserve FieldLookupContentName(FieldSize)
                    ReDim Preserve FieldLookupList(FieldSize)
                    For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In adminContent.fields
                        Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                        If FieldPtr >= FieldSize Then
                            FieldSize = FieldSize + 100
                            ReDim Preserve FieldNames(FieldSize)
                            ReDim Preserve FieldCaption(FieldSize)
                            ReDim Preserve fieldId(FieldSize)
                            ReDim Preserve fieldTypeId(FieldSize)
                            ReDim Preserve FieldValue(FieldSize)
                            ReDim Preserve FieldMatchOptions(FieldSize)
                            ReDim Preserve FieldLookupContentName(FieldSize)
                            ReDim Preserve FieldLookupList(FieldSize)
                        End If
                        With field
                            FieldName = genericController.vbLCase(.nameLc)
                            FieldNames(FieldPtr) = FieldName
                            FieldCaption(FieldPtr) = .caption
                            fieldId(FieldPtr) = .id
                            fieldTypeId(FieldPtr) = .fieldTypeId
                            If fieldTypeId(FieldPtr) = FieldTypeIdLookup Then
                                ContentID = .lookupContentID
                                If ContentID > 0 Then
                                    FieldLookupContentName(FieldPtr) = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID)
                                End If
                                FieldLookupList(FieldPtr) = .lookupList
                            End If
                        End With
                        '
                        ' set prepoplate value from indexconfig
                        '
                        With IndexConfig
                            If .FindWords.ContainsKey(FieldName) Then
                                FieldValue(FieldPtr) = .FindWords(FieldName).Value
                                FieldMatchOptions(FieldPtr) = .FindWords(FieldName).MatchOption
                            End If
                        End With
                        FieldPtr += 1
                    Next
                    '        Criteria = "(active<>0)and(ContentID=" & adminContent.id & ")and(authorable<>0)"
                    '        CS = cpCore.app.csOpen("Content Fields", Criteria, "EditSortPriority")
                    '        FieldPtr = 0
                    '        Do While cpCore.app.csv_IsCSOK(CS)
                    '            If FieldPtr >= FieldSize Then
                    '                FieldSize = FieldSize + 100
                    '                ReDim Preserve FieldNames(FieldSize)
                    '                ReDim Preserve FieldCaption(FieldSize)
                    '                ReDim Preserve FieldID(FieldSize)
                    '                ReDim Preserve FieldType(FieldSize)
                    '                ReDim Preserve FieldValue(FieldSize)
                    '                ReDim Preserve FieldMatchOptions(FieldSize)
                    '                ReDim Preserve FieldLookupContentName(FieldSize)
                    '                ReDim Preserve FieldLookupList(FieldSize)
                    '            End If
                    '            FieldName = genericController.vbLCase(cpCore.db.cs_getText(CS, "name"))
                    '            FieldNames(FieldPtr) = FieldName
                    '            FieldCaption(FieldPtr) = cpCore.db.cs_getText(CS, "Caption")
                    '            FieldID(FieldPtr) = cpCore.app.cs_getInteger(CS, "ID")
                    '            FieldType(FieldPtr) = cpCore.app.cs_getInteger(CS, "Type")
                    '            If FieldType(FieldPtr) = 7 Then
                    '                ContentID = cpCore.app.cs_getInteger(CS, "LookupContentID")
                    '                If ContentID > 0 Then
                    '                    FieldLookupContentName(FieldPtr) = models.complex.cdefmodel.getContentNameByID(cpcore,ContentID)
                    '                End If
                    '                FieldLookupList(FieldPtr) = cpCore.db.cs_getText(CS, "LookupList")
                    '            End If
                    '            '
                    '            ' set prepoplate value from indexconfig
                    '            '
                    '            With IndexConfig
                    '                If .findwords.count > 0 Then
                    '                    For Ptr = 0 To .findwords.count - 1
                    '                        If .FindWords(Ptr).Name = FieldName Then
                    '                            FieldValue(FieldPtr) = .FindWords(Ptr).Value
                    '                            FieldMatchOptions(FieldPtr) = .FindWords(Ptr).MatchOption
                    '                            Exit For
                    '                        End If
                    '                    Next
                    '                End If
                    '            End With
                    ''            If CriteriaCount > 0 Then
                    ''                For CriteriaPointer = 0 To CriteriaCount - 1
                    ''                    FieldMatchOptions(FieldPtr) = 0
                    ''                    If genericController.vbInstr(1, CriteriaValues(CriteriaPointer), FieldNames(FieldPtr) & "=", vbTextCompare) = 1 Then
                    ''                        NameValues = Split(CriteriaValues(CriteriaPointer), "=")
                    ''                        FieldValue(FieldPtr) = NameValues(1)
                    ''                        FieldMatchOptions(FieldPtr) = 1
                    ''                    ElseIf genericController.vbInstr(1, CriteriaValues(CriteriaPointer), FieldNames(FieldPtr) & ">", vbTextCompare) = 1 Then
                    ''                        NameValues = Split(CriteriaValues(CriteriaPointer), ">")
                    ''                        FieldValue(FieldPtr) = NameValues(1)
                    ''                        FieldMatchOptions(FieldPtr) = 2
                    ''                    ElseIf genericController.vbInstr(1, CriteriaValues(CriteriaPointer), FieldNames(FieldPtr) & "<", vbTextCompare) = 1 Then
                    ''                        NameValues = Split(CriteriaValues(CriteriaPointer), "<")
                    ''                        FieldValue(FieldPtr) = NameValues(1)
                    ''                        FieldMatchOptions(FieldPtr) = 3
                    ''                    End If
                    ''                Next
                    ''            End If
                    '            FieldPtr = FieldPtr + 1
                    '            Call cpCore.app.nextCSRecord(CS)
                    '        Loop
                    '        Call cpCore.app.closeCS(CS)
                    FieldCnt = FieldPtr
                    '
                    ' Add headers to stream
                    '
                    returnForm = returnForm & "<table border=0 width=100% cellspacing=0 cellpadding=4>"
                    '
                    RowPointer = 0
                    For FieldPtr = 0 To FieldCnt - 1
                        returnForm = returnForm & cpCore.html.html_GetFormInputHidden("fieldname" & FieldPtr, FieldNames(FieldPtr))
                        RowEven = ((RowPointer Mod 2) = 0)
                        FieldMatchOption = FieldMatchOptions(FieldPtr)
                        Select Case fieldTypeId(FieldPtr)
                            Case FieldTypeIdDate
                                '
                                ' Date
                                '
                                returnForm = returnForm _
                                & "<tr>" _
                                & "<td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>" _
                                & "<td class=""ccAdminEditField"">" _
                                & "<div style=""display:block;float:left;width:800px;"">" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchIgnore).ToString, FieldMatchOption.ToString, "") & "ignore</div>" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchEmpty).ToString, FieldMatchOption.ToString, "") & "empty</div>" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchNotEmpty).ToString, FieldMatchOption.ToString, "") & "not&nbsp;empty</div>" _
                                & "<div style=""display:block;float:left;width:50px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchEquals).ToString, FieldMatchOption.ToString, "") & "=</div>" _
                                & "<div style=""display:block;float:left;width:50px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchGreaterThan).ToString, FieldMatchOption.ToString, "") & "&gt;</div>" _
                                & "<div style=""display:block;float:left;width:50px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchLessThan).ToString, FieldMatchOption.ToString, "") & "&lt;</div>" _
                                & "<div style=""display:block;float:left;width:300px;"">" & GetFormInputDateWithFocus2("fieldvalue" & FieldPtr, FieldValue(FieldPtr), "5", "", "", "ccAdvSearchText") & "</div>" _
                                & "</div>" _
                                & "</td>" _
                                & "</tr>"
                            Case FieldTypeIdCurrency, FieldTypeIdFloat, FieldTypeIdInteger, FieldTypeIdFloat, FieldTypeIdAutoIdIncrement
                                '
                                ' Numeric
                                '
                                ' changed FindWordMatchEnum.MatchEquals to MatchInclude to be compatible with Find Search
                                returnForm = returnForm _
                                & "<tr>" _
                                & "<td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>" _
                                & "<td class=""ccAdminEditField"">" _
                                & "<div style=""display:block;float:left;width:800px;"">" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchIgnore).ToString, FieldMatchOption.ToString, "") & "ignore</div>" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchEmpty).ToString, FieldMatchOption.ToString, "") & "empty</div>" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchNotEmpty).ToString, FieldMatchOption.ToString, "") & "not&nbsp;empty</div>" _
                                & "<div style=""display:block;float:left;width:50px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.matchincludes).ToString, FieldMatchOption.ToString, "n" & FieldPtr) & "=</div>" _
                                & "<div style=""display:block;float:left;width:50px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchGreaterThan).ToString, FieldMatchOption.ToString, "") & "&gt;</div>" _
                                & "<div style=""display:block;float:left;width:50px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchLessThan).ToString, FieldMatchOption.ToString, "") & "&lt;</div>" _
                                & "<div style=""display:block;float:left;width:300px;"">" & GetFormInputWithFocus2("fieldvalue" & FieldPtr, FieldValue(FieldPtr), 1, 5, "", "var e=getElementById('n" & FieldPtr & "');e.checked=1;", "ccAdvSearchText") & "</div>" _
                                & "</div>" _
                                & "</td>" _
                                & "</tr>"

                                '                    s = s _
                                '                        & "<tr>" _
                                '                        & "<td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>" _
                                '                        & "<td class=""ccAdminEditField"">" _
                                '                        & "<table border=0 width=100% cellspacing=0 cellpadding=0><tr>" _
                                '                            & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchIgnore, FieldMatchOption, "") & "</td><td align=left width=100>ignore</td>" _
                                '                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                '                            & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchEmpty, FieldMatchOption, "") & "</td><td align=left width=100>empty</td>" _
                                '                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                '                            & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchNotEmpty, FieldMatchOption, "") & "</td><td align=left width=100>not&nbsp;empty</td>" _
                                '                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                '                            & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchEquals, FieldMatchOption, "") & "</td><td align=left width=100>=</td>" _
                                '                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                '                            & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchGreaterThan, FieldMatchOption, "") & "</td><td align=left width=100>&gt;</td>" _
                                '                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                '                            & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchLessThan, FieldMatchOption, "") & "</td><td align=left width=100>&lt;</td>" _
                                '                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                '                            & "<td align=left width=99%>" & GetFormInputWithFocus("fieldvalue" & FieldPtr, FieldValue(FieldPtr), 1, 5, "", "") & "</td>" _
                                '                        & "</tr></table>" _
                                '                        & "</td>" _
                                '                        & "</tr>"

                                RowPointer = RowPointer + 1
                            Case FieldTypeIdFile, FieldTypeIdFileImage
                                '
                                ' File
                                '
                                returnForm = returnForm _
                                & "<tr>" _
                                & "<td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>" _
                                & "<td class=""ccAdminEditField"">" _
                                & "<div style=""display:block;float:left;width:800px;"">" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchIgnore).ToString, FieldMatchOption.ToString, "") & "ignore</div>" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchEmpty).ToString, FieldMatchOption.ToString, "") & "empty</div>" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchNotEmpty).ToString, FieldMatchOption.ToString, "") & "not&nbsp;empty</div>" _
                                & "</div>" _
                                & "</td>" _
                                & "</tr>"
                                's = s _
                                '    & "<tr>" _
                                '    & "<td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>" _
                                '    & "<td class=""ccAdminEditField"">" _
                                '    & "<table border=0 width=100% cellspacing=0 cellpadding=0><tr>" _
                                '        & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchIgnore, FieldMatchOption, "") & "</td><td align=left width=100>ignore</td>" _
                                '        & "<td width=10>&nbsp;&nbsp;</td>" _
                                '        & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchEmpty, FieldMatchOption, "") & "</td><td align=left width=100>empty</td>" _
                                '        & "<td width=10>&nbsp;&nbsp;</td>" _
                                '        & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchNotEmpty, FieldMatchOption, "") & "</td><td align=left width=100>not&nbsp;empty</td>" _
                                '        & "<td align=left width=99%>&nbsp;</td>" _
                                '    & "</tr></table>" _
                                '    & "</td>" _
                                '    & "</tr>"
                                RowPointer = RowPointer + 1
                            Case FieldTypeIdBoolean
                                '
                                ' Boolean
                                '
                                returnForm = returnForm _
                                & "<tr>" _
                                & "<td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>" _
                                & "<td class=""ccAdminEditField"">" _
                                & "<div style=""display:block;float:left;width:800px;"">" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchIgnore).ToString, FieldMatchOption.ToString, "") & "ignore</div>" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchTrue).ToString, FieldMatchOption.ToString, "") & "true</div>" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchFalse).ToString, FieldMatchOption.ToString, "") & "false</div>" _
                                & "</div>" _
                                & "</td>" _
                                & "</tr>"
                            '                    s = s _
                            '                        & "<tr>" _
                            '                        & "<td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>" _
                            '                        & "<td class=""ccAdminEditField"">" _
                            '                        & "<table border=0 width=100% cellspacing=0 cellpadding=0><tr>" _
                            '                            & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchIgnore, FieldMatchOption, "") & "</td><td align=left width=100>  ignore</td>" _
                            '                            & "<td width=10>&nbsp;&nbsp;</td>" _
                            '                            & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchTrue, FieldMatchOption, "") & "</td><td align=left width=100>true</td>" _
                            '                            & "<td width=10>&nbsp;&nbsp;</td>" _
                            '                            & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchFalse, FieldMatchOption, "") & "</td><td align=left width=100>false</td>" _
                            '                            & "<td width=99%>&nbsp;</td>" _
                            '                        & "</tr></table>" _
                            '                        & "</td>" _
                            '                        & "</tr>"
                            Case FieldTypeIdText, FieldTypeIdLongText, FieldTypeIdHTML, FieldTypeIdFileHTML, FieldTypeIdFileCSS, FieldTypeIdFileJavascript, FieldTypeIdFileXML
                                '
                                ' Text
                                '
                                returnForm = returnForm _
                                & "<tr>" _
                                & "<td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>" _
                                & "<td class=""ccAdminEditField"">" _
                                & "<div style=""display:block;float:left;width:800px;"">" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchIgnore).ToString, FieldMatchOption.ToString, "") & "ignore</div>" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchEmpty).ToString, FieldMatchOption.ToString, "") & "empty</div>" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchNotEmpty).ToString, FieldMatchOption.ToString, "") & "not&nbsp;empty</div>" _
                                & "<div style=""display:block;float:left;width:150px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.matchincludes).ToString, FieldMatchOption.ToString, "t" & FieldPtr) & "includes</div>" _
                                & "<div style=""display:block;float:left;width:300px;"">" & GetFormInputWithFocus2("fieldvalue" & FieldPtr, FieldValue(FieldPtr), 1, 5, "", "var e=getElementById('t" & FieldPtr & "');e.checked=1;", "ccAdvSearchText") & "</div>" _
                                & "</div>" _
                                & "</td>" _
                                & "</tr>"
                                '
                                '
                                '                    s = s _
                                '                        & "<tr>" _
                                '                        & "<td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>" _
                                '                        & "<td class=""ccAdminEditField"" valign=absmiddle>" _
                                '                        & "<table border=0 width=100% cellspacing=0 cellpadding=0><tr>" _
                                '                            & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchIgnore, FieldMatchOption, "") & "</td><td align=left width=100>&nbsp;&nbsp;ignore</td>" _
                                '                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                '                            & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchEmpty, FieldMatchOption, "") & "</td><td align=left width=100>empty</td>" _
                                '                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                '                            & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchNotEmpty, FieldMatchOption, "") & "</td><td align=left width=100>not&nbsp;empty</td>" _
                                '                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                '                            & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.matchincludes, FieldMatchOption, "t" & FieldPtr) & "</td><td align=center width=100>includes&nbsp;</td>" _
                                '                            & "<td align=left width=99%>" & GetFormInputWithFocus("FieldValue" & FieldPtr, FieldValue(FieldPtr), 1, 20, "", "var e=getElementById('t" & FieldPtr & "');e.checked=1;") & "</td>" _
                                '                        & "</tr></table>" _
                                '                        & "</td>" _
                                '                        & "</tr>"
                                RowPointer = RowPointer + 1
                            Case FieldTypeIdLookup, FieldTypeIdMemberSelect
                                '
                                ' Lookup
                                '
                                'Dim SelectOption As String
                                'Dim CurrentValue As String
                                'If FieldLookupContentName(FieldPtr) <> "" Then
                                '    ContentName = FieldLookupContentName(FieldPtr)
                                '    DataSourceName = models.complex.cdefmodel.getContentDataSource(cpcore,ContentName)
                                '    TableName = cpCore.main_GetContentTablename(ContentName)
                                '    SQL = "select distinct Name from " & TableName & " where (name is not null) order by name"
                                '    CS = cpCore.app.openCsSql_rev(DataSourceName, SQL)
                                '    If Not cpCore.app.csv_IsCSOK(CS) Then
                                '        selector = "no options"
                                '    Else
                                '        selector = vbCrLf & "<select name=""FieldValue" & FieldPtr & """ onFocus=""var e=getElementById('t" & FieldPtr & "');e.checked=1;"">"
                                '        CurrentValue = FieldValue(FieldPtr)
                                '        Do While cpCore.app.csv_IsCSOK(CS)
                                '            SelectOption = cpCore.db.cs_getText(CS, "name")
                                '            If CurrentValue = SelectOption Then
                                '                selector = selector & vbCrLf & "<option selected>" & SelectOption & "</option>"
                                '            Else
                                '                selector = selector & vbCrLf & "<option>" & SelectOption & "</option>"
                                '            End If
                                '            Call cpCore.app.nextCSRecord(CS)
                                '        Loop
                                '        selector = selector & vbCrLf & "</select>"
                                '    End If
                                '    Call cpCore.app.closeCS(CS)
                                '    'selector = cpCore.htmldoc.main_GetFormInputSelect2("FieldValue" & FieldPtr, FieldValue(FieldPtr), FieldLookupContentName(FieldPtr))
                                'Else
                                '    selector = cpCore.htmldoc.main_GetFormInputSelectList2("FieldValue" & FieldPtr, FieldValue(FieldPtr), FieldLookupList(FieldPtr))
                                'End If
                                'selector = genericController.vbReplace(selector, ">", "onFocus=""var e=getElementById('t" & FieldPtr & "');e.checked=1;"">")
                                returnForm = returnForm _
                                & "<tr>" _
                                & "<td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>" _
                                & "<td class=""ccAdminEditField"">" _
                                & "<div style=""display:block;float:left;width:800px;"">" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchIgnore).ToString, FieldMatchOption.ToString, "") & "ignore</div>" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchEmpty).ToString, FieldMatchOption.ToString, "") & "empty</div>" _
                                & "<div style=""display:block;float:left;width:100px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.MatchNotEmpty).ToString, FieldMatchOption.ToString, "") & "not&nbsp;empty</div>" _
                                & "<div style=""display:block;float:left;width:150px;"">" & cpCore.html.html_GetFormInputRadioBox("FieldMatch" & FieldPtr, CInt(FindWordMatchEnum.matchincludes).ToString, FieldMatchOption.ToString, "t" & FieldPtr) & "includes</div>" _
                                & "<div style=""display:block;float:left;width:300px;"">" & GetFormInputWithFocus2("fieldvalue" & FieldPtr, FieldValue(FieldPtr), 1, 5, "", "var e=getElementById('t" & FieldPtr & "');e.checked=1;", "ccAdvSearchText") & "</div>" _
                                & "</div>" _
                                & "</td>" _
                                & "</tr>"

                                '& "<div style=""display:block;float:left;width:150px;"">" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchEquals, FieldMatchOption, "t" & FieldPtr) & "=&nbsp;</div>" _
                                '& "<div style=""display:block;float:left;width:300px;"">" & selector & "</div>" _

                                '                    s = s _
                                '                        & "<tr>" _
                                '                        & "<td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>" _
                                '                        & "<td class=""ccAdminEditField"" valign=absmiddle>" _
                                '                        & "<table border=0 width=100% cellspacing=0 cellpadding=0><tr>" _
                                '                            & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchIgnore, FieldMatchOption, "") & "</td><td align=left width=100>&nbsp;&nbsp;ignore</td>" _
                                '                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                '                            & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchEmpty, FieldMatchOption, "") & "</td><td align=left width=100>empty</td>" _
                                '                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                '                            & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchNotEmpty, FieldMatchOption, "") & "</td><td align=left width=100>not&nbsp;empty</td>" _
                                '                            & "<td width=10>&nbsp;&nbsp;</td>" _
                                '                            & "<td width=10 align=right>" & cpCore.main_GetFormInputRadioBox("FieldMatch" & FieldPtr, FindWordMatchEnum.MatchEquals, FieldMatchOption, "t" & FieldPtr) & "</td><td align=center width=100>=&nbsp;</td>" _
                                '                            & "<td align=left width=99%>" & selector & "</td>" _
                                '                        & "</tr></table>" _
                                '                        & "</td>" _
                                '                        & "</tr>"



                                's = s & "<tr><td class=""ccAdminEditCaption"">" & FieldCaption(FieldPtr) & "</td>"
                                'If FieldLookupContentName(FieldPtr) <> "" Then
                                '    s = s _
                                '        & "<td class=""ccAdminEditField"">" _
                                '        & cpCore.htmldoc.main_GetFormInputSelect2(FieldNames(FieldPtr), FieldValue(FieldPtr), FieldLookupContentName(FieldPtr), , "Any") & "</td>"
                                'Else
                                '    s = s _
                                '        & "<td class=""ccAdminEditField"">" _
                                '        & cpCore.htmldoc.main_GetFormInputSelectList2(FieldNames(FieldPtr), FieldValue(FieldPtr), FieldLookupList(FieldPtr), , "Any") & "</td>"
                                'End If
                                's = s & "</tr>"
                                RowPointer = RowPointer + 1
                        End Select
                    Next
                    returnForm = returnForm & genericController.StartTableRow()
                    returnForm = returnForm & genericController.StartTableCell("120", 1, RowEven, "right") & "<img src=/ccLib/images/spacer.gif width=120 height=1></td>"
                    returnForm = returnForm & genericController.StartTableCell("99%", 1, RowEven, "left") & "<img src=/ccLib/images/spacer.gif width=1 height=1></td>"
                    returnForm = returnForm & kmaEndTableRow
                    returnForm = returnForm & "</table>"
                    Content = returnForm
                    '
                    ' Assemble LiveWindowTable
                    '
                    '        Stream.Add( OpenLiveWindowTable)
                    Stream.Add(vbCrLf & cpCore.html.html_GetFormStart())
                    Stream.Add(ButtonBar)
                    Stream.Add(TitleBar)
                    Stream.Add(Content)
                    Stream.Add(ButtonBar)
                    Stream.Add("<input type=hidden name=fieldcnt VALUE=" & FieldCnt & ">")
                    'Stream.Add( "<input type=hidden name=af VALUE=" & AdminFormIndex & ">")
                    Stream.Add("<input type=hidden name=" & RequestNameAdminSubForm & " VALUE=" & AdminFormIndex_SubFormAdvancedSearch & ">")
                    Stream.Add("</form>")
                    '        Stream.Add( CloseLiveWindowTable)
                    '
                    returnForm = Stream.Text
                    Call cpCore.html.addTitle(adminContent.Name & " Advanced Search")
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnForm
        End Function
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
            Dim datasource As dataSourceModel = dataSourceModel.create(cpCore, adminContent.dataSourceId, New List(Of String))
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
        ''
        ''========================================================================
        ''   Display field in the admin/edit
        ''========================================================================
        ''
        'Private Function GetForm_Edit_RSSFeeds(ContentName As String, ContentID as integer, RecordID as integer, PageLink As String) As String
        '    On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.GetForm_Edit_RSSFeeds")
        '    '
        '    Dim DateExpiresText As String
        '    Dim DatePublishText As String
        '    Dim FeedEditLink As String
        '    Dim RSSFeedCID as integer
        '    Dim Caption As String
        '    Dim AttachID as integer
        '    Dim AttachName As String
        '    Dim AttachLink As String
        '    Dim CS as integer
        '    Dim HTMLFieldString As String
        '    ' converted array to dictionary - Dim FieldPointer As Integer
        '    Dim CSPointer as integer
        '    Dim CSFeeds as integer
        '    Dim Cnt as integer
        '    Dim FeedID as integer
        '    Dim s As New fastStringClass
        '    Dim Copy As String
        '    Dim Adminui As New adminUIclass(cpcore)
        '    Dim FeedName As String
        '    Dim DefaultValue As Boolean
        '    Dim ItemID as integer
        '    Dim ItemName As String
        '    Dim ItemDescription As String
        '    Dim ItemLink As String
        '    Dim ItemDateExpires As Date
        '    Dim ItemDatePublish As Date
        '    '
        '    if true then ' 3.3.816" Then
        '        '
        '        ' Get the RSS Items (Name, etc)
        '        '
        '        CS = cpCore.app.csOpen("RSS Feed Items", "(ContentID=" & ContentID & ")and(RecordID=" & RecordID & ")", "ID")
        '        If Not cpCore.app.csv_IsCSOK(CS) Then
        '            '
        '            ' Default Value
        '            '
        '            ItemID = 0
        '            ItemName = ""
        '            ItemDescription = ""
        '            ItemLink = PageLink
        '            ItemDateExpires = Date.MinValue
        '            ItemDatePublish = Date.MinValue
        '        Else
        '            ItemID = cpCore.app.cs_getInteger(CS, "ID")
        '            ItemName = cpCore.db.cs_getText(CS, "Name")
        '            ItemDescription = cpCore.db.cs_getText(CS, "Description")
        '            ItemLink = cpCore.db.cs_getText(CS, "Link")
        '            ItemDateExpires = cpCore.db.cs_getDate(CS, "DateExpires")
        '            ItemDatePublish = cpCore.db.cs_getDate(CS, "DatePublish")
        '        End If
        '        Call cpCore.app.closeCS(CS)
        '        '
        '        ' List out the Feeds, lookup the rules top find a match between items and feeds
        '        '
        '        RSSFeedCID = cpCore.main_GetContentID("RSS Feeds")
        '        CSFeeds = cpCore.app.csOpen("RSS Feeds", , "name")
        '        If cpCore.app.csv_IsCSOK(CSFeeds) Then
        '            Cnt = 0
        '            Do While cpCore.app.csv_IsCSOK(CSFeeds)
        '                FeedID = cpCore.app.cs_getInteger(CSFeeds, "id")
        '                FeedName = cpCore.db.cs_getText(CSFeeds, "name")
        '                '
        '                DefaultValue = False
        '                If ItemID <> 0 Then
        '                    CS = cpCore.app.csOpen("RSS Feed Rules", "(RSSFeedID=" & FeedID & ")AND(RSSFeedItemID=" & ItemID & ")", , , True)
        '                    If cpCore.app.csv_IsCSOK(CS) Then
        '                        DefaultValue = True
        '                    End If
        '                    Call cpCore.app.closeCS(CS)
        '                End If
        '                '
        '                If Cnt = 0 Then
        '                    s.Add( "<tr><td class=""ccAdminEditCaption"">Include in RSS Feed</td>"
        '                Else
        '                    s.Add( "<tr><td class=""ccAdminEditCaption"">&nbsp;</td>"
        '                End If
        '                FeedEditLink = "[<a href=""?af=4&cid=" & RSSFeedCID & "&id=" & FeedID & """>Edit RSS Feed</a>]"
        '                s.Add( "<td class=""ccAdminEditField"">"
        '                    s.Add( "<table border=0 cellpadding=0 cellspacing=0 width=""100%"" ><tr>"
        '                    If editrecord.read_only Then
        '                        s.Add( "<td width=""50%"">" & genericController.encodeText(DefaultValue) & "&nbsp;" & FeedName & "</td>"
        '                    Else
        '                        s.Add( "<td width=""50%"">" & cpCore.main_GetFormInputHidden("RSSFeedWas." & Cnt, DefaultValue) & cpCore.main_GetFormInputHidden("RSSFeedID." & Cnt, FeedID) & cpCore.main_GetFormInputCheckBox2("RSSFeed." & Cnt, DefaultValue) & FeedName & "</td>"
        '                    End If
        '                    s.Add( "<td width=""50%"">" & FeedEditLink & "</td>"
        '                    s.Add( "</tr></table>"
        '                s.Add( "</td></tr>"
        '                Call cpCore.app.nextCSRecord(CSFeeds)
        '                Cnt = Cnt + 1
        '            Loop
        '            If Cnt = 0 Then
        '                s.Add( "<tr><td class=""ccAdminEditCaption"">Include in RSS Feed</td>"
        '            Else
        '                s.Add( "<tr><td class=""ccAdminEditCaption"">&nbsp;</td>"
        '            End If
        '            FeedEditLink = "[<a href=""?af=4&cid=" & RSSFeedCID & """>Add New RSS Feed</a>]&nbsp;[<a href=""?cid=" & RSSFeedCID & """>RSS Feeds</a>]"
        '            s.Add( "<td class=""ccAdminEditField"">"
        '                s.Add( "<table border=0 cellpadding=0 cellspacing=0 width=""100%"" ><tr>"
        '                s.Add( "<td width=""50%"">&nbsp;</td>"
        '                s.Add( "<td width=""50%"">" & FeedEditLink & "</td>"
        '                s.Add( "</tr></table>"
        '            s.Add( "</td></tr>"
        '
        '
        '        End If
        '        Call cpCore.app.closeCS(CSFeeds)
        '        s.Add( cpCore.main_GetFormInputHidden("RSSFeedCnt", Cnt)
        '        '
        '        ' ----- RSS Item fields
        '        '
        '        If ItemDateExpires = Date.MinValue Then
        '            DateExpiresText = ""
        '        Else
        '            DateExpiresText = CStr(ItemDateExpires)
        '        End If
        '        If ItemDatePublish = Date.MinValue Then
        '            DatePublishText = ""
        '        Else
        '            DatePublishText = CStr(ItemDatePublish)
        '        End If
        '        If editrecord.read_only Then
        '            s.Add( "<tr><td class=""ccAdminEditCaption"">Title</td><td class=""ccAdminEditField"">" & ItemName & "</td></tr>"
        '            s.Add( "<tr><td class=""ccAdminEditCaption"">Description</td><td class=""ccAdminEditField"">" & ItemDescription & "</td></tr>"
        '            s.Add( "<tr><td class=""ccAdminEditCaption"">Link</td><td class=""ccAdminEditField"">" & ItemLink & "</td></tr>"
        '            s.Add( "<tr><td class=""ccAdminEditCaption"">Publish</td><td class=""ccAdminEditField"">" & DatePublishText & "</td></tr>"
        '            s.Add( "<tr><td class=""ccAdminEditCaption"">Expire</td><td class=""ccAdminEditField"">" & DateExpiresText & "</td></tr>"
        '        Else
        '            s.Add( "<tr><td class=""ccAdminEditCaption"">Title*</td><td class=""ccAdminEditField"">" & cpCore.main_GetFormInputText2("RSSFeedItemName", ItemName, 1, 60) & "</td></tr>"
        '            s.Add( "<tr><td class=""ccAdminEditCaption"">Description*</td><td class=""ccAdminEditField"">" & cpCore.main_GetFormInputTextExpandable("RSSFeedItemDescription", ItemDescription, 5) & "</td></tr>"
        '            s.Add( "<tr><td class=""ccAdminEditCaption"">Link*</td><td class=""ccAdminEditField"">" & cpCore.main_GetFormInputText2("RSSFeedItemLink", ItemLink, 1, 60) & "</td></tr>"
        '            s.Add( "<tr><td class=""ccAdminEditCaption"">Publish</td><td class=""ccAdminEditField"">" & cpCore.main_GetFormInputDate("RSSFeedItemDatePublish", DatePublishText, 40) & "</td></tr>"
        '            s.Add( "<tr><td class=""ccAdminEditCaption"">Expire</td><td class=""ccAdminEditField"">" & cpCore.main_GetFormInputDate("RSSFeedItemDateExpires", DateExpiresText, 40) & "</td></tr>"
        '        End If
        '        '
        '        ' ----- Add Attachements to Feeds
        '        '
        '        Caption = "Add Podcast Media Link"
        '        Cnt = 0
        '        CS = cpCore.app.csOpen("Attachments", "(ContentID=" & ContentID & ")AND(RecordID=" & RecordID & ")", , , True)
        '        If cpCore.app.csv_IsCSOK(CS) Then
        '            '
        '            ' ----- List all Attachements
        '            '
        '            Cnt = 0
        '            Do While cpCore.app.csv_IsCSOK(CS)
        '
        '                AttachID = cpCore.app.cs_getInteger(CS, "id")
        '                AttachName = cpCore.db.cs_getText(CS, "name")
        '                AttachLink = cpCore.db.cs_getText(CS, "link")
        '                '
        '                s.Add( "<tr><td class=""ccAdminEditCaption"">" & Caption & "</td>"
        '                If Cnt = 0 Then
        '                    Caption = "&nbsp;"
        '                End If
        '                s.Add( "<td class=""ccAdminEditField"">"
        '                    s.Add( "<table border=0 cellpadding=0 cellspacing=0 width=""100%"" ><tr>"
        '                    If editrecord.read_only Then
        '                        s.Add( "<td>" & AttachLink & "</td>"
        '                        's.Add( "<td width=""30%"">Caption " & AttachName & "</td>"
        '                    Else
        '                        s.Add( "<td>" & cpCore.main_GetFormInputText2("AttachLink." & Cnt, AttachLink, 1, 60) & cpCore.main_GetFormInputHidden("AttachLinkID." & Cnt, AttachID) & "</td>"
        '                        's.Add( "<td width=""30%"">Caption " & cpCore.main_GetFormInputText2("AttachCaption." & Cnt, AttachName, 20) & "</td>"
        '                    End If
        '                    s.Add( "</tr></table>"
        '                s.Add( "<td width=""30%"">&nbsp;</td>"
        '                s.Add( "</td></tr>"
        '                Call cpCore.app.nextCSRecord(CS)
        '                Cnt = Cnt + 1
        '            Loop
        '            End If
        '        Call cpCore.app.closeCS(CS)
        '        '
        '        ' ----- Add Attachment link (only allow one for now)
        '        '
        '        If (Cnt = 0) And (Not editrecord.read_only) Then
        '            s.Add( "<tr><td class=""ccAdminEditCaption"">" & Caption & "</td>"
        '            s.Add( "<td class=""ccAdminEditField"">"
        '                s.Add( "<table border=0 cellpadding=0 cellspacing=0 width=""100%"" ><tr>"
        '                s.Add( "<td width=""70%"">" & cpCore.main_GetFormInputText2("AttachLink." & Cnt, AttachLink, 1, 60) & "</td>"
        '                s.Add( "<td width=""30%"">&nbsp;</td>"
        '                s.Add( "</tr></table>"
        '            s.Add( "</td></tr>"
        '            Cnt = Cnt + 1
        '        End If
        '        s.Add( cpCore.main_GetFormInputHidden("RSSAttachCnt", Cnt)
        '        '
        '        ' ----- add the *Required Fields footer
        '        '
        '        Call s.Add("" _
        '            & "<tr><td colspan=2 style=""padding-top:10px;font-size:70%"">" _
        '            & "<div>* Fields marked with an asterisk are required if any RSS Feed is selected.</div>" _
        '            & "</td></tr>")
        '        '
        '        ' ----- close the panel
        '        '
        '        GetForm_Edit_RSSFeeds = AdminUI.GetEditPanel( (Not AllowAdminTabs), "RSS Feeds / Podcasts", "Include in RSS Feeds / Podcasts", AdminUI.EditTableOpen & s.Text & AdminUI.EditTableClose)
        '        EditSectionPanelCount = EditSectionPanelCount + 1
        '        '
        '        s = Nothing
        '    End If
        '    '''Dim th as integer: Exit Function
        '    '
        'ErrorTrap:
        '    s = Nothing
        '    Call HandleClassTrapErrorBubble("GetForm_Edit_RSSFeeds")
        'End Function
        ''
        ''========================================================================
        ''   Load and Save RSS Feeds Tab
        ''========================================================================
        ''
        'Private Sub LoadAndSaveRSSFeeds(ContentName As String, ContentID as integer, RecordID as integer, ItemLink As String)
        '    On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.LoadAndSaveRSSFeeds")
        '    '
        '    Dim AttachID as integer
        '    Dim AttachLink As String
        '    Dim CS as integer
        '    Dim Cnt as integer
        '    Dim Ptr as integer
        '    Dim FeedChecked As Boolean
        '    Dim FeedWasChecked As Boolean
        '    Dim FeedID as integer
        '    Dim DateExpires As Date
        '    Dim RecordLink As String
        '    Dim ItemID as integer
        '    Dim ItemName As String
        '    Dim ItemDescription As String
        '    Dim ItemDateExpires As Date
        '    Dim ItemDatePublish As Date
        '    Dim FeedChanged As Boolean
        '    '
        '    ' Process Feeds
        '    '
        '    Cnt = cpCore.main_GetStreamInteger2("RSSFeedCnt")
        '    If Cnt > 0 Then
        '        '
        '        ' Test if any feed checked -- then check Feed Item fields for required
        '        '
        '        ItemName = cpCore.main_GetStreamText2("RSSFeedItemName")
        '        ItemDescription = cpCore.main_GetStreamText2("RSSFeedItemDescription")
        '        ItemLink = cpCore.main_GetStreamText2("RSSFeedItemLink")
        '        ItemDateExpires = cpCore.main_GetStreamDate("RSSFeedItemDateExpires")
        '        ItemDatePublish = cpCore.main_GetStreamDate("RSSFeedItemDatePublish")
        '        For Ptr = 0 To Cnt - 1
        '            FeedChecked = cpCore.main_GetStreamBoolean2("RSSFeed." & Ptr)
        '            If FeedChecked Then
        '                Exit For
        '            End If
        '        Next
        '        If FeedChecked Then
        '            '
        '            ' check required fields
        '            '
        '            If Trim(ItemName) = "" Then
        '                Call cpCore.htmldoc.main_AddUserError("In the RSS/Podcasts tab, a Title is required if any RSS Feed is checked.")
        '            End If
        '            If Trim(ItemDescription) = "" Then
        '                Call cpCore.htmldoc.main_AddUserError("In the RSS/Podcasts tab, a Description is required if any RSS Feed is checked.")
        '            End If
        '            If Trim(ItemLink) = "" Then
        '                Call cpCore.htmldoc.main_AddUserError("In the RSS/Podcasts tab, a Link is required if any RSS Feed is checked.")
        '            End If
        '        End If
        '        If FeedChecked Or (ItemName <> "") Or (ItemDescription <> "") Or (ItemLink <> "") Then
        '            '
        '            '
        '            '
        '            CS = cpCore.app.csOpen("RSS Feed Items", "(ContentID=" & ContentID & ")and(RecordID=" & RecordID & ")", "ID")
        '            If Not cpCore.app.csv_IsCSOK(CS) Then
        '                Call cpCore.app.closeCS(CS)
        '                CS = cpCore.main_InsertCSContent("RSS Feed Items")
        '            End If
        '            If ItemDatePublish = Date.MinValue Then
        '                ItemDatePublish = nt(cpCore.main_PageStartTime.toshortdateString
        '            End If
        '            If cpCore.app.csv_IsCSOK(CS) Then
        '                ItemID = cpCore.app.cs_getInteger(CS, "ID")
        '                Call cpCore.app.SetCS(CS, "ContentID", ContentID)
        '                Call cpCore.app.SetCS(CS, "RecordID", RecordID)
        '                Call cpCore.app.SetCS(CS, "Name", ItemName)
        '                Call cpCore.app.SetCS(CS, "Description", ItemDescription)
        '                Call cpCore.app.SetCS(CS, "Link", ItemLink)
        '                Call cpCore.app.SetCS(CS, "DateExpires", ItemDateExpires)
        '                Call cpCore.app.SetCS(CS, "DatePublish", ItemDatePublish)
        '            End If
        '            Call cpCore.app.closeCS(CS)
        '            FeedChanged = True
        '        End If
        '        '
        '        ' ----- Now process the RSS Feed checkboxes
        '        '
        '        For Ptr = 0 To Cnt - 1
        '            FeedChecked = cpCore.main_GetStreamBoolean2("RSSFeed." & Ptr)
        '            FeedWasChecked = cpCore.main_GetStreamBoolean2("RSSFeedWas." & Ptr)
        '            FeedID = cpCore.main_GetStreamInteger2("RSSFeedID." & Ptr)
        '            If FeedChecked And Not FeedWasChecked Then
        '                '
        '                ' Create rule
        '                '
        '                CS = cpCore.main_InsertCSContent("RSS Feed Rules")
        '                If cpCore.app.csv_IsCSOK(CS) Then
        '                    Call cpCore.app.SetCS(CS, "Name", "RSS Feed for " & EditRecord.Name)
        '                    Call cpCore.app.SetCS(CS, "RSSFeedID", FeedID)
        '                    Call cpCore.app.SetCS(CS, "RSSFeedItemID", ItemID)
        '                End If
        '                Call cpCore.app.closeCS(CS)
        '            ElseIf FeedWasChecked And Not FeedChecked Then
        '                '
        '                ' Delete Rule
        '                '
        '                FeedID = cpCore.main_GetStreamInteger2("RSSFeedID." & Ptr)
        '                Call cpCore.app.DeleteContentRecords("RSS Feed Rules", "(RSSFeedID=" & FeedID & ")and(ItemContentID=" & ContentID & ")and(RSSFeedItemID=" & ItemID & ")")
        '            End If
        '        Next
        '    End If
        '    '
        '    ' Attachments
        '    '
        '    Cnt = cpCore.main_GetStreamInteger2("RSSAttachCnt")
        '    If Cnt > 0 Then
        '        For Ptr = 0 To Cnt - 1
        '            AttachID = cpCore.main_GetStreamInteger2("AttachLinkID." & Ptr)
        '            AttachLink = cpCore.main_GetStreamText2("AttachLink." & Ptr)
        '            If AttachID <> 0 And AttachLink <> "" Then
        '                '
        '                ' Update Attachment
        '                '
        '                CS = cpCore.main_OpenCSContentRecord("Attachments", AttachID)
        '                If cpCore.app.csv_IsCSOK(CS) Then
        '                    Call cpCore.app.SetCS(CS, "Name", "Podcast attachment for " & EditRecord.Name)
        '                    Call cpCore.app.SetCS(CS, "Link", AttachLink)
        '                    Call cpCore.app.SetCS(CS, "ContentID", ContentID)
        '                    Call cpCore.app.SetCS(CS, "RecordID", RecordID)
        '                End If
        '                Call cpCore.app.closeCS(CS)
        '                FeedChanged = True
        '            ElseIf AttachID = 0 And AttachLink <> "" Then
        '                '
        '                ' Create Attachment
        '                '
        '                CS = cpCore.main_InsertCSContent("Attachments")
        '                If cpCore.app.csv_IsCSOK(CS) Then
        '                    Call cpCore.app.SetCS(CS, "Name", "Podcast attachment for " & EditRecord.Name)
        '                    Call cpCore.app.SetCS(CS, "Link", AttachLink)
        '                    Call cpCore.app.SetCS(CS, "AttachContentID", ContentID)
        '                    Call cpCore.app.SetCS(CS, "AttachRecordID", RecordID)
        '                End If
        '                Call cpCore.app.closeCS(CS)
        '                FeedChanged = True
        '            ElseIf AttachID <> 0 And AttachLink = "" Then
        '                '
        '                ' delete attachment
        '                '
        '                Call cpCore.app.DeleteContentRecords("Attachments", "(AttachContentID=" & ContentID & ")and(AttachRecordID=" & RecordID & ")")
        '                FeedChanged = True
        '            End If
        '        Next
        '    End If
        '    '
        '    '
        '    '
        '    If FeedChanged Then
        'Dim Cmd As String
        '        Cmd = getAppPath() & "\ccProcessRSS.exe"
        '        Call Shell(Cmd)
        '    End If
        '
        '    '
        '    '''Dim th as integer: Exit Sub
        '    '
        '    ' ----- Error Trap
        '    '
        'ErrorTrap:
        '    Call HandleClassTrapErrorBubble("LoadAndSaveRSSFeeds")
        '    '
        'End Sub
        '
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Function GetForm_ClearCache() As String
            Dim returnHtml As String = ""
            Try
                Dim Content As New stringBuilderLegacyController
                Dim Button As String
                Dim Adminui As New adminUIController(cpCore)
                Dim Description As String
                Dim ButtonList As String
                '
                Button = cpCore.docProperties.getText(RequestNameButton)
                If Button = ButtonCancel Then
                    '
                    ' Cancel just exits with no content
                    '
                    Return ""
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
                    '
                    ' Process Requests
                    '
                    Select Case Button
                        Case ButtonApply, ButtonOK
                            '
                            ' Clear the cache
                            '
                            Call cpCore.cache.invalidateAll()
                    End Select
                    If (Button = ButtonOK) Then
                        '
                        ' Exit on OK or cancel
                        '
                        Return ""
                    End If
                    '
                    ' Buttons
                    '
                    ButtonList = ButtonCancel & "," & ButtonApply & "," & ButtonOK
                    '
                    ' Close Tables
                    '
                    Content.Add(Adminui.EditTableClose)
                    Content.Add(cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormClearCache))
                End If
                '
                Description = "Hit Apply or OK to clear all current content caches"
                returnHtml = Adminui.GetBody("Clear Cache", ButtonList, "", True, True, Description, "", 0, Content.Text)
                Content = Nothing
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnHtml
        End Function
        '
        '========================================================================
        ' Tool to enter multiple Meta Keywords
        '========================================================================
        '
        Private Function GetForm_MetaKeywordTool() As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogAdminMethodEnter( "GetForm_MetaKeywordTool")
            '
            Const LoginMode_None = 1
            Const LoginMode_AutoRecognize = 2
            Const LoginMode_AutoLogin = 3
            '
            Dim LoginMode As Integer
            Dim Help As String
            Dim Content As New stringBuilderLegacyController
            Dim Copy As String
            Dim Button As String
            Dim PageNotFoundPageID As String
            Dim Adminui As New adminUIController(cpCore)
            Dim Description As String
            Dim ButtonList As String
            Dim AllowLinkAlias As Boolean
            'Dim AllowExternalLinksInChildList As Boolean
            Dim LinkForwardAutoInsert As Boolean
            Dim SectionLandingLink As String
            Dim ServerPageDefault As String
            Dim LandingPageID As String
            Dim DocTypeDeclaration As String
            Dim AllowAutoRecognize As Boolean
            Dim KeywordList As String
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
                ' Process Requests
                '
                Select Case Button
                    Case ButtonSave, ButtonOK
                        '
                        Dim Keywords() As String
                        Dim Keyword As String
                        Dim Cnt As Integer
                        Dim Ptr As Integer
                        Dim dt As DataTable
                        Dim CS As Integer
                        KeywordList = cpCore.docProperties.getText("KeywordList")
                        If KeywordList <> "" Then
                            KeywordList = genericController.vbReplace(KeywordList, vbCrLf, ",")
                            Keywords = Split(KeywordList, ",")
                            Cnt = UBound(Keywords) + 1
                            For Ptr = 0 To Cnt - 1
                                Keyword = Trim(Keywords(Ptr))
                                If Keyword <> "" Then
                                    'Dim dt As DataTable

                                    dt = cpCore.db.executeQuery("select top 1 ID from ccMetaKeywords where name=" & cpCore.db.encodeSQLText(Keyword))
                                    If dt.Rows.Count = 0 Then
                                        CS = cpCore.db.csInsertRecord("Meta Keywords")
                                        If cpCore.db.csOk(CS) Then
                                            Call cpCore.db.csSet(CS, "name", Keyword)
                                        End If
                                        Call cpCore.db.csClose(CS)
                                    End If
                                End If
                            Next
                        End If
                End Select
                If (Button = ButtonOK) Then
                    '
                    ' Exit on OK or cancel
                    '
                    Exit Function
                End If
                '
                ' KeywordList
                '
                Copy = cpCore.html.html_GetFormInputTextExpandable("KeywordList", , 10)
                Copy = Copy _
            & "<div>Paste your Meta Keywords into this text box, separated by either commas or enter keys. When you hit Save or OK, Meta Keyword records will be made out of each word. These can then be checked on any content page.</div>"
                Call Content.Add(Adminui.GetEditRow(Copy, "Paste Meta Keywords", "", False, False, ""))
                '
                ' Buttons
                '
                ButtonList = ButtonCancel & "," & ButtonSave & "," & ButtonOK
                '
                ' Close Tables
                '
                Content.Add(Adminui.EditTableClose)
                Content.Add(cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormSecurityControl))
            End If
            '
            Description = "Use this tool to enter multiple Meta Keywords"
            GetForm_MetaKeywordTool = Adminui.GetBody("Meta Keyword Entry Tool", ButtonList, "", True, True, Description, "", 0, Content.Text)
            Content = Nothing
            '
            '''Dim th as integer: Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Content = Nothing
            Call handleLegacyClassError3("GetForm_MetaKeywordTool")
            '
        End Function
        '
        '
        '
        Private Function AllowAdminFieldCheck() As Boolean
            If Not AllowAdminFieldCheck_LocalLoaded Then
                AllowAdminFieldCheck_LocalLoaded = True
                AllowAdminFieldCheck_Local = (cpCore.siteProperties.getBoolean("AllowAdminFieldCheck", True))
            End If
            AllowAdminFieldCheck = AllowAdminFieldCheck_Local
        End Function
        '
        '
        '
        Private Function GetAddonHelp(HelpAddonID As Integer, UsedIDString As String) As String
            Dim addonHelp As String = ""
            Try
                Dim IconFilename As String
                Dim IconWidth As Integer
                Dim IconHeight As Integer
                Dim IconSprites As Integer
                Dim IconIsInline As Boolean
                Dim CS As Integer
                Dim AddonName As String = ""
                Dim AddonHelpCopy As String = ""
                Dim AddonDateAdded As Date
                Dim AddonLastUpdated As Date
                Dim SQL As String
                Dim IncludeHelp As String = ""
                Dim IncludeID As Integer
                Dim IconImg As String = ""
                Dim helpLink As String = ""
                Dim FoundAddon As Boolean
                '
                If genericController.vbInstr(1, "," & UsedIDString & ",", "," & CStr(HelpAddonID) & ",") = 0 Then
                    CS = cpCore.db.csOpenRecord(cnAddons, HelpAddonID)
                    If cpCore.db.csOk(CS) Then
                        FoundAddon = True
                        AddonName = cpCore.db.csGet(CS, "Name")
                        AddonHelpCopy = cpCore.db.csGet(CS, "help")
                        AddonDateAdded = cpCore.db.csGetDate(CS, "dateadded")
                        If Models.Complex.cdefModel.isContentFieldSupported(cpCore, cnAddons, "lastupdated") Then
                            AddonLastUpdated = cpCore.db.csGetDate(CS, "lastupdated")
                        End If
                        If AddonLastUpdated = Date.MinValue Then
                            AddonLastUpdated = AddonDateAdded
                        End If
                        IconFilename = cpCore.db.csGet(CS, "Iconfilename")
                        IconWidth = cpCore.db.csGetInteger(CS, "IconWidth")
                        IconHeight = cpCore.db.csGetInteger(CS, "IconHeight")
                        IconSprites = cpCore.db.csGetInteger(CS, "IconSprites")
                        IconIsInline = cpCore.db.csGetBoolean(CS, "IsInline")
                        IconImg = genericController.GetAddonIconImg("/" & cpCore.serverConfig.appConfig.adminRoute, IconWidth, IconHeight, IconSprites, IconIsInline, "", IconFilename, cpCore.serverConfig.appConfig.cdnFilesNetprefix, AddonName, AddonName, "", 0)
                        helpLink = cpCore.db.csGet(CS, "helpLink")
                    End If
                    Call cpCore.db.csClose(CS)
                    '
                    If FoundAddon Then
                        '
                        ' Included Addons
                        '
                        SQL = "select IncludedAddonID from ccAddonIncludeRules where AddonID=" & HelpAddonID
                        CS = cpCore.db.csOpenSql_rev("default", SQL)
                        Do While cpCore.db.csOk(CS)
                            IncludeID = cpCore.db.csGetInteger(CS, "IncludedAddonID")
                            IncludeHelp = IncludeHelp & GetAddonHelp(IncludeID, HelpAddonID & "," & CStr(IncludeID))
                            Call cpCore.db.csGoNext(CS)
                        Loop
                        Call cpCore.db.csClose(CS)
                        '
                        If helpLink <> "" Then
                            If AddonHelpCopy <> "" Then
                                AddonHelpCopy = AddonHelpCopy & "<p>For additional help with this add-on, please visit <a href=""" & helpLink & """>" & helpLink & "</a>.</p>"
                            Else
                                AddonHelpCopy = AddonHelpCopy & "<p>For help with this add-on, please visit <a href=""" & helpLink & """>" & helpLink & "</a>.</p>"
                            End If
                        End If
                        If AddonHelpCopy = "" Then
                            AddonHelpCopy = AddonHelpCopy & "<p>Please refer to the help resources available for this collection. More information may also be available in the Contensive online Learning Center <a href=""http://support.contensive.com/Learning-Center"">http://support.contensive.com/Learning-Center</a> or contact Contensive Support support@contensive.com for more information.</p>"
                        End If
                        addonHelp = "" _
                            & "<div class=""ccHelpCon"">" _
                            & "<div class=""title""><div style=""float:right;""><a href=""?addonid=" & HelpAddonID & """>" & IconImg & "</a></div>" & AddonName & " Add-on</div>" _
                            & "<div class=""byline"">" _
                                & "<div>Installed " & AddonDateAdded & "</div>" _
                                & "<div>Last Updated " & AddonLastUpdated & "</div>" _
                            & "</div>" _
                            & "<div class=""body"" style=""clear:both;"">" & AddonHelpCopy & "</div>" _
                            & "</div>"
                        addonHelp = addonHelp & IncludeHelp
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return addonHelp
        End Function
        '
        '
        '
        Private Function GetCollectionHelp(HelpCollectionID As Integer, UsedIDString As String) As String
            Dim returnHelp As String = ""
            Try
                Dim CS As Integer
                Dim Collectionname As String = ""
                Dim CollectionHelpCopy As String = ""
                Dim CollectionHelpLink As String = ""
                Dim CollectionDateAdded As Date
                Dim CollectionLastUpdated As Date
                Dim SQL As String
                Dim IncludeHelp As String = ""
                Dim addonId As Integer
                '
                If genericController.vbInstr(1, "," & UsedIDString & ",", "," & CStr(HelpCollectionID) & ",") = 0 Then
                    CS = cpCore.db.csOpenRecord("Add-on Collections", HelpCollectionID)
                    If cpCore.db.csOk(CS) Then
                        Collectionname = cpCore.db.csGet(CS, "Name")
                        CollectionHelpCopy = cpCore.db.csGet(CS, "help")
                        CollectionDateAdded = cpCore.db.csGetDate(CS, "dateadded")
                        If Models.Complex.cdefModel.isContentFieldSupported(cpCore, "Add-on Collections", "lastupdated") Then
                            CollectionLastUpdated = cpCore.db.csGetDate(CS, "lastupdated")
                        End If
                        If Models.Complex.cdefModel.isContentFieldSupported(cpCore, "Add-on Collections", "helplink") Then
                            CollectionHelpLink = cpCore.db.csGet(CS, "helplink")
                        End If
                        If CollectionLastUpdated = Date.MinValue Then
                            CollectionLastUpdated = CollectionDateAdded
                        End If
                    End If
                    Call cpCore.db.csClose(CS)
                    '
                    ' Add-ons
                    '
                    If True Then ' 4.0.321" Then
                        '$$$$$ cache this
                        CS = cpCore.db.csOpen(cnAddons, "CollectionID=" & HelpCollectionID, "name", , , , , "ID")
                        Do While cpCore.db.csOk(CS)
                            IncludeHelp = IncludeHelp & "<div style=""clear:both;"">" & GetAddonHelp(cpCore.db.csGetInteger(CS, "ID"), "") & "</div>"
                            Call cpCore.db.csGoNext(CS)
                        Loop
                        Call cpCore.db.csClose(CS)
                    Else
                        ' addoncollectionrules deprecated for collectionid
                        SQL = "select AddonID from ccAddonCollectionRules where CollectionID=" & HelpCollectionID
                        CS = cpCore.db.csOpenSql_rev("default", SQL)
                        Do While cpCore.db.csOk(CS)
                            addonId = cpCore.db.csGetInteger(CS, "AddonID")
                            If addonId <> 0 Then
                                IncludeHelp = IncludeHelp & "<div style=""clear:both;"">" & GetAddonHelp(addonId, "") & "</div>"
                            End If
                            Call cpCore.db.csGoNext(CS)
                        Loop
                        Call cpCore.db.csClose(CS)
                    End If
                    '
                    If (CollectionHelpLink = "") And (CollectionHelpCopy = "") Then
                        CollectionHelpCopy = "<p>No help information could be found for this collection. Please use the online resources at <a href=""http://support.contensive.com/Learning-Center"">http://support.contensive.com/Learning-Center</a> or contact Contensive Support support@contensive.com by email.</p>"
                    ElseIf CollectionHelpLink <> "" Then
                        CollectionHelpCopy = "" _
                            & "<p>For information about this collection please visit <a href=""" & CollectionHelpLink & """>" & CollectionHelpLink & "</a>.</p>" _
                            & CollectionHelpCopy
                    End If
                    '
                    returnHelp = "" _
                        & "<div class=""ccHelpCon"">" _
                        & "<div class=""title"">" & Collectionname & " Collection</div>" _
                        & "<div class=""byline"">" _
                            & "<div>Installed " & CollectionDateAdded & "</div>" _
                            & "<div>Last Updated " & CollectionLastUpdated & "</div>" _
                        & "</div>" _
                        & "<div class=""body"">" & CollectionHelpCopy & "</div>"
                    If IncludeHelp <> "" Then
                        returnHelp = returnHelp & IncludeHelp
                    End If
                    returnHelp = returnHelp & "</div>"
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnHelp
        End Function
        '
        '
        '
        Private Sub SetIndexSQL(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, IndexConfig As indexConfigClass, ByRef Return_AllowAccess As Boolean, ByRef return_sqlFieldList As String, ByRef return_sqlFrom As String, ByRef return_SQLWhere As String, ByRef return_SQLOrderBy As String, ByRef return_IsLimitedToSubContent As Boolean, ByRef return_ContentAccessLimitMessage As String, ByRef FieldUsedInColumns As Dictionary(Of String, Boolean), IsLookupFieldValid As Dictionary(Of String, Boolean))
            Try
                Dim LookupQuery As String
                Dim ContentName As String
                Dim SortFieldName As String
                '
                Dim LookupPtr As Integer
                Dim lookups() As String
                Dim FindWordName As String
                Dim FindWordValue As String
                Dim FindMatchOption As Integer
                Dim WCount As Integer
                Dim SubContactList As String = ""
                Dim ContentID As Integer
                Dim Pos As Integer
                Dim Cnt As Integer
                Dim ListSplit() As String
                Dim SubContentCnt As Integer
                Dim list As String
                Dim SubQuery As String
                Dim GroupID As Integer
                Dim GroupName As String
                Dim JoinTablename As String
                'Dim FieldName As String
                Dim Ptr As Integer
                Dim IncludedInLeftJoin As Boolean
                '  Dim SupportWorkflowFields As Boolean
                Dim FieldPtr As Integer
                Dim IncludedInColumns As Boolean
                Dim LookupContentName As String
                ''Dim arrayOfFields() As appServices_metaDataClass.CDefFieldClass
                '
                Return_AllowAccess = True
                '
                ' ----- Workflow Fields
                '
                return_sqlFieldList = return_sqlFieldList & adminContent.ContentTableName & ".ID"
                '
                ' ----- From Clause - build joins for Lookup fields in columns, in the findwords, and in sorts
                '
                return_sqlFrom = adminContent.ContentTableName
                For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In adminContent.fields
                    Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                    With field
                        FieldPtr = .id ' quick fix for a replacement for the old fieldPtr (so multiple for loops will always use the same "table"+ptr string
                        IncludedInColumns = False
                        IncludedInLeftJoin = False
                        If Not IsLookupFieldValid.ContainsKey(.nameLc) Then
                            IsLookupFieldValid.Add(.nameLc, False)
                        End If
                        If Not FieldUsedInColumns.ContainsKey(.nameLc) Then
                            FieldUsedInColumns.Add(.nameLc, False)
                        End If
                        '
                        ' test if this field is one of the columns we are displaying
                        '
                        IncludedInColumns = IndexConfig.Columns.ContainsKey(field.nameLc)
                        '
                        ' disallow IncludedInColumns if a non-supported field type
                        '
                        Select Case .fieldTypeId
                            Case FieldTypeIdFileCSS, FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdFileJavascript, FieldTypeIdLongText, FieldTypeIdManyToMany, FieldTypeIdRedirect, FieldTypeIdFileText, FieldTypeIdFileXML, FieldTypeIdHTML, FieldTypeIdFileHTML
                                IncludedInColumns = False
                        End Select
                        'FieldName = genericController.vbLCase(.Name)
                        If (.fieldTypeId = FieldTypeIdMemberSelect) Or ((.fieldTypeId = FieldTypeIdLookup) And (.lookupContentID <> 0)) Then
                            '
                            ' This is a lookup field -- test if IncludedInLeftJoins
                            '
                            JoinTablename = ""
                            If .fieldTypeId = FieldTypeIdMemberSelect Then
                                LookupContentName = "people"
                            Else
                                LookupContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, .lookupContentID)
                            End If
                            If LookupContentName <> "" Then
                                JoinTablename = Models.Complex.cdefModel.getContentTablename(cpCore, LookupContentName)
                            End If
                            IncludedInLeftJoin = IncludedInColumns
                            If (IndexConfig.FindWords.Count > 0) Then
                                '
                                ' test findwords
                                '
                                If IndexConfig.FindWords.ContainsKey(.nameLc) Then
                                    If IndexConfig.FindWords(.nameLc).MatchOption <> FindWordMatchEnum.MatchIgnore Then
                                        IncludedInLeftJoin = True
                                    End If
                                End If
                            End If
                            If (Not IncludedInLeftJoin) And IndexConfig.Sorts.Count > 0 Then
                                '
                                ' test sorts
                                '
                                If IndexConfig.Sorts.ContainsKey(.nameLc.ToLower) Then
                                    IncludedInLeftJoin = True
                                End If
                            End If
                            If IncludedInLeftJoin Then
                                '
                                ' include this lookup field
                                '
                                FieldUsedInColumns.Item(.nameLc) = True
                                If JoinTablename <> "" Then
                                    IsLookupFieldValid(.nameLc) = True
                                    return_sqlFieldList = return_sqlFieldList & ", LookupTable" & FieldPtr & ".Name AS LookupTable" & FieldPtr & "Name"
                                    return_sqlFrom = "(" & return_sqlFrom & " LEFT JOIN " & JoinTablename & " AS LookupTable" & FieldPtr & " ON " & adminContent.ContentTableName & "." & .nameLc & " = LookupTable" & FieldPtr & ".ID)"
                                End If
                                'End If
                            End If
                        End If
                        If IncludedInColumns Then
                            '
                            ' This field is included in the columns, so include it in the select
                            '
                            return_sqlFieldList = return_sqlFieldList & " ," & adminContent.ContentTableName & "." & .nameLc
                            FieldUsedInColumns(.nameLc) = True
                        End If
                    End With
                Next
                '
                ' Sub CDef filter
                '
                With IndexConfig
                    If .SubCDefID > 0 Then
                        ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, .SubCDefID)
                        return_SQLWhere &= "AND(" & Models.Complex.cdefModel.getContentControlCriteria(cpCore, ContentName) & ")"
                    End If
                End With
                '
                ' Return_sqlFrom and Where Clause for Groups filter
                '
                Dim rightNow As Date = DateTime.Now()
                Dim sqlRightNow As String = cpCore.db.encodeSQLDate(rightNow)
                If adminContent.ContentTableName.ToLower = "ccmembers" Then
                    With IndexConfig
                        If .GroupListCnt > 0 Then
                            For Ptr = 0 To .GroupListCnt - 1
                                GroupName = .GroupList(Ptr)
                                If GroupName <> "" Then
                                    GroupID = cpCore.db.getRecordID("Groups", GroupName)
                                    If GroupID = 0 And genericController.vbIsNumeric(GroupName) Then
                                        GroupID = genericController.EncodeInteger(GroupName)
                                    End If
                                    Dim groupTableAlias As String = "GroupFilter" & Ptr
                                    return_SQLWhere &= "AND(" & groupTableAlias & ".GroupID=" & GroupID & ")and((" & groupTableAlias & ".dateExpires is null)or(" & groupTableAlias & ".dateExpires>" & sqlRightNow & "))"
                                    return_sqlFrom = "(" & return_sqlFrom & " INNER JOIN ccMemberRules AS GroupFilter" & Ptr & " ON GroupFilter" & Ptr & ".MemberID=ccMembers.ID)"
                                    'Return_sqlFrom = "(" & Return_sqlFrom & " INNER JOIN ccMemberRules AS GroupFilter" & Ptr & " ON GroupFilter" & Ptr & ".MemberID=ccmembers.ID)"
                                End If
                            Next
                        End If
                    End With
                End If
                '
                ' Add Name into Return_sqlFieldList
                '
                'If Not SQLSelectIncludesName Then
                ' SQLSelectIncludesName is declared, but not initialized
                return_sqlFieldList = return_sqlFieldList & " ," & adminContent.ContentTableName & ".Name"
                'End If
                '
                ' paste sections together and do where clause
                '
                If userHasContentAccess(adminContent.Id) Then
                    '
                    ' This person can see all the records
                    '
                    return_SQLWhere &= "AND(" & Models.Complex.cdefModel.getContentControlCriteria(cpCore, adminContent.Name) & ")"
                Else
                    '
                    ' Limit the Query to what they can see
                    '
                    return_IsLimitedToSubContent = True
                    SubQuery = ""
                    list = adminContent.ContentControlCriteria
                    adminContent.Id = adminContent.Id
                    SubContentCnt = 0
                    If list <> "" Then
                        Console.WriteLine("console - adminContent.contentControlCriteria=" & list)
                        Debug.WriteLine("debug - adminContent.contentControlCriteria=" & list)
                        logController.appendLog(cpCore, "appendlog - adminContent.contentControlCriteria=" & list)
                        ListSplit = Split(list, "=")
                        Cnt = UBound(ListSplit) + 1
                        If Cnt > 0 Then
                            For Ptr = 0 To Cnt - 1
                                Pos = genericController.vbInstr(1, ListSplit(Ptr), ")")
                                If Pos > 0 Then
                                    ContentID = genericController.EncodeInteger(Mid(ListSplit(Ptr), 1, Pos - 1))
                                    If ContentID > 0 And (ContentID <> adminContent.Id) And userHasContentAccess(ContentID) Then
                                        SubQuery = SubQuery & "OR(" & adminContent.ContentTableName & ".ContentControlID=" & ContentID & ")"
                                        return_ContentAccessLimitMessage = return_ContentAccessLimitMessage & ", '<a href=""?cid=" & ContentID & """>" & Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID) & "</a>'"
                                        SubContactList &= "," & ContentID
                                        SubContentCnt = SubContentCnt + 1
                                    End If
                                End If
                            Next
                        End If
                    End If
                    If SubQuery = "" Then
                        '
                        ' Person has no access
                        '
                        Return_AllowAccess = False
                        Exit Sub
                    Else
                        return_SQLWhere &= "AND(" & Mid(SubQuery, 3) & ")"
                        return_ContentAccessLimitMessage = "Your access to " & adminContent.Name & " is limited to Sub-content(s) " & Mid(return_ContentAccessLimitMessage, 3)
                    End If
                End If
                '
                ' Where Clause: Active Only
                '
                If IndexConfig.ActiveOnly Then
                    return_SQLWhere &= "AND(" & adminContent.ContentTableName & ".active<>0)"
                End If
                '
                ' Where Clause: edited by me
                '
                If IndexConfig.LastEditedByMe Then
                    return_SQLWhere &= "AND(" & adminContent.ContentTableName & ".ModifiedBy=" & cpCore.doc.authContext.user.id & ")"
                End If
                '
                ' Where Clause: edited today
                '
                If IndexConfig.LastEditedToday Then
                    return_SQLWhere &= "AND(" & adminContent.ContentTableName & ".ModifiedDate>=" & cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime.Date) & ")"
                End If
                '
                ' Where Clause: edited past week
                '
                If IndexConfig.LastEditedPast7Days Then
                    return_SQLWhere &= "AND(" & adminContent.ContentTableName & ".ModifiedDate>=" & cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime.Date.AddDays(-7)) & ")"
                End If
                '
                ' Where Clause: edited past month
                '
                If IndexConfig.LastEditedPast30Days Then
                    return_SQLWhere &= "AND(" & adminContent.ContentTableName & ".ModifiedDate>=" & cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime.Date.AddDays(-30)) & ")"
                End If
                '
                ' Where Clause: Where Pairs
                '
                For WCount = 0 To 9
                    If WherePair(1, WCount) <> "" Then
                        '
                        ' Verify that the fieldname called out is in this table
                        '
                        If adminContent.fields.Count > 0 Then
                            For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In adminContent.fields
                                Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                                With field
                                    If genericController.vbUCase(.nameLc) = genericController.vbUCase(WherePair(0, WCount)) Then
                                        '
                                        ' found it, add it in the sql
                                        '
                                        return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & WherePair(0, WCount) & "="
                                        If genericController.vbIsNumeric(WherePair(1, WCount)) Then
                                            return_SQLWhere &= WherePair(1, WCount) & ")"
                                        Else
                                            return_SQLWhere &= "'" & WherePair(1, WCount) & "')"
                                        End If
                                        Exit For
                                    End If
                                End With
                            Next
                        End If
                    End If
                Next
                '
                ' Where Clause: findwords
                '
                If IndexConfig.FindWords.Count > 0 Then
                    For Each kvp In IndexConfig.FindWords
                        Dim findword As indexConfigFindWordClass = kvp.Value
                        FindMatchOption = findword.MatchOption
                        If FindMatchOption <> FindWordMatchEnum.MatchIgnore Then
                            FindWordName = genericController.vbLCase(findword.Name)
                            FindWordValue = findword.Value
                            '
                            ' Get FieldType
                            '
                            If adminContent.fields.Count > 0 Then
                                For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In adminContent.fields
                                    Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                                    With field
                                        FieldPtr = .id ' quick fix for a replacement for the old fieldPtr (so multiple for loops will always use the same "table"+ptr string
                                        If genericController.vbLCase(.nameLc) = FindWordName Then
                                            Select Case .fieldTypeId
                                                Case FieldTypeIdAutoIdIncrement, FieldTypeIdInteger
                                                    '
                                                    ' integer
                                                    '
                                                    Dim FindWordValueInteger As Integer = genericController.EncodeInteger(FindWordValue)
                                                    Select Case FindMatchOption
                                                        Case FindWordMatchEnum.MatchEmpty
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & " is null)"
                                                        Case FindWordMatchEnum.MatchNotEmpty
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & " is not null)"
                                                        Case FindWordMatchEnum.MatchEquals, FindWordMatchEnum.matchincludes
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & "=" & cpCore.db.encodeSQLNumber(FindWordValueInteger) & ")"
                                                        Case FindWordMatchEnum.MatchGreaterThan
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & ">" & cpCore.db.encodeSQLNumber(FindWordValueInteger) & ")"
                                                        Case FindWordMatchEnum.MatchLessThan
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & "<" & cpCore.db.encodeSQLNumber(FindWordValueInteger) & ")"
                                                    End Select
                                                    Exit For

                                                Case FieldTypeIdCurrency, FieldTypeIdFloat
                                                    '
                                                    ' double
                                                    '
                                                    Dim FindWordValueDouble As Double = genericController.EncodeNumber(FindWordValue)
                                                    Select Case FindMatchOption
                                                        Case FindWordMatchEnum.MatchEmpty
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & " is null)"
                                                        Case FindWordMatchEnum.MatchNotEmpty
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & " is not null)"
                                                        Case FindWordMatchEnum.MatchEquals, FindWordMatchEnum.matchincludes
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & "=" & cpCore.db.encodeSQLNumber(FindWordValueDouble) & ")"
                                                        Case FindWordMatchEnum.MatchGreaterThan
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & ">" & cpCore.db.encodeSQLNumber(FindWordValueDouble) & ")"
                                                        Case FindWordMatchEnum.MatchLessThan
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & "<" & cpCore.db.encodeSQLNumber(FindWordValueDouble) & ")"
                                                    End Select
                                                    Exit For
                                                Case FieldTypeIdFile, FieldTypeIdFileImage
                                                    '
                                                    ' Date
                                                    '
                                                    Select Case FindMatchOption
                                                        Case FindWordMatchEnum.MatchEmpty
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & " is null)"
                                                        Case FindWordMatchEnum.MatchNotEmpty
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & " is not null)"
                                                    End Select
                                                    Exit For
                                                Case FieldTypeIdDate
                                                    '
                                                    ' Date
                                                    '
                                                    Dim findDate As Date = Date.MinValue
                                                    If IsDate(FindWordValue) Then
                                                        findDate = CDate(FindWordValue)
                                                    End If
                                                    Select Case FindMatchOption
                                                        Case FindWordMatchEnum.MatchEmpty
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & " is null)"
                                                        Case FindWordMatchEnum.MatchNotEmpty
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & " is not null)"
                                                        Case FindWordMatchEnum.MatchEquals, FindWordMatchEnum.matchincludes
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & "=" & cpCore.db.encodeSQLDate(findDate) & ")"
                                                        Case FindWordMatchEnum.MatchGreaterThan
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & ">" & cpCore.db.encodeSQLDate(findDate) & ")"
                                                        Case FindWordMatchEnum.MatchLessThan
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & "<" & cpCore.db.encodeSQLDate(findDate) & ")"
                                                    End Select
                                                    Exit For
                                                Case FieldTypeIdLookup, FieldTypeIdMemberSelect
                                                    '
                                                    ' Lookup
                                                    '
                                                    If IsLookupFieldValid(field.nameLc) Then
                                                        '
                                                        ' Content Lookup
                                                        '
                                                        Select Case FindMatchOption
                                                            Case FindWordMatchEnum.MatchEmpty
                                                                return_SQLWhere &= "AND(LookupTable" & FieldPtr & ".ID is null)"
                                                            Case FindWordMatchEnum.MatchNotEmpty
                                                                return_SQLWhere &= "AND(LookupTable" & FieldPtr & ".ID is not null)"
                                                            Case FindWordMatchEnum.MatchEquals
                                                                return_SQLWhere &= "AND(LookupTable" & FieldPtr & ".Name=" & cpCore.db.encodeSQLText(FindWordValue) & ")"
                                                            Case FindWordMatchEnum.matchincludes
                                                                return_SQLWhere &= "AND(LookupTable" & FieldPtr & ".Name LIKE " & cpCore.db.encodeSQLText("%" & FindWordValue & "%") & ")"
                                                        End Select
                                                    ElseIf .lookupList <> "" Then
                                                        '
                                                        ' LookupList
                                                        '
                                                        Select Case FindMatchOption
                                                            Case FindWordMatchEnum.MatchEmpty
                                                                return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & " is null)"
                                                            Case FindWordMatchEnum.MatchNotEmpty
                                                                return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & " is not null)"
                                                            Case FindWordMatchEnum.MatchEquals, FindWordMatchEnum.matchincludes
                                                                lookups = Split(.lookupList, ",")
                                                                LookupQuery = ""
                                                                For LookupPtr = 0 To UBound(lookups)
                                                                    If genericController.vbInstr(1, lookups(LookupPtr), FindWordValue, vbTextCompare) <> 0 Then
                                                                        LookupQuery = LookupQuery & "OR(" & adminContent.ContentTableName & "." & FindWordName & "=" & cpCore.db.encodeSQLNumber(LookupPtr + 1) & ")"
                                                                    End If
                                                                Next
                                                                If LookupQuery <> "" Then
                                                                    return_SQLWhere &= "AND(" & Mid(LookupQuery, 3) & ")"
                                                                End If
                                                        End Select
                                                    End If
                                                    Exit For
                                                Case FieldTypeIdBoolean
                                                    '
                                                    ' Boolean
                                                    '
                                                    Select Case FindMatchOption
                                                        Case FindWordMatchEnum.matchincludes
                                                            If genericController.EncodeBoolean(FindWordValue) Then
                                                                return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & "<>0)"
                                                            Else
                                                                return_SQLWhere &= "AND((" & adminContent.ContentTableName & "." & FindWordName & "=0)or(" & adminContent.ContentTableName & "." & FindWordName & " is null))"
                                                            End If
                                                        Case FindWordMatchEnum.MatchTrue
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & "<>0)"
                                                        Case FindWordMatchEnum.MatchFalse
                                                            return_SQLWhere &= "AND((" & adminContent.ContentTableName & "." & FindWordName & "=0)or(" & adminContent.ContentTableName & "." & FindWordName & " is null))"
                                                    End Select
                                                    Exit For
                                                Case Else
                                                    '
                                                    ' Text (and the rest)
                                                    '
                                                    Select Case FindMatchOption
                                                        Case FindWordMatchEnum.MatchEmpty
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & " is null)"
                                                        Case FindWordMatchEnum.MatchNotEmpty
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & " is not null)"
                                                        Case FindWordMatchEnum.matchincludes
                                                            FindWordValue = cpCore.db.encodeSQLText(FindWordValue)
                                                            FindWordValue = Mid(FindWordValue, 2, Len(FindWordValue) - 2)
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & " LIKE '%" & FindWordValue & "%')"
                                                        Case FindWordMatchEnum.MatchEquals
                                                            return_SQLWhere &= "AND(" & adminContent.ContentTableName & "." & FindWordName & "=" & cpCore.db.encodeSQLText(FindWordValue) & ")"
                                                    End Select
                                                    Exit For
                                            End Select
                                            Exit For
                                        End If
                                    End With
                                Next
                            End If
                        End If
                    Next
                End If
                return_SQLWhere = Mid(return_SQLWhere, 4)
                '
                ' SQL Order by
                '
                return_SQLOrderBy = ""
                Dim orderByDelim As String = " "
                For Each kvp In IndexConfig.Sorts
                    Dim sort As indexConfigSortClass = kvp.Value
                    SortFieldName = genericController.vbLCase(sort.fieldName)
                    '
                    ' Get FieldType
                    '
                    If adminContent.fields.ContainsKey(sort.fieldName) Then
                        With adminContent.fields(sort.fieldName)
                            FieldPtr = .id ' quick fix for a replacement for the old fieldPtr (so multiple for loops will always use the same "table"+ptr string
                            If (.fieldTypeId = FieldTypeIdLookup) And IsLookupFieldValid(sort.fieldName) Then
                                return_SQLOrderBy &= orderByDelim & "LookupTable" & FieldPtr & ".Name"
                            Else
                                return_SQLOrderBy &= orderByDelim & adminContent.ContentTableName & "." & SortFieldName
                            End If
                        End With
                    End If
                    If sort.direction > 1 Then
                        return_SQLOrderBy = return_SQLOrderBy & " Desc"
                    End If
                    orderByDelim = ","
                Next
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '==============================================================================================
        '   If this field has no help message, check the field with the same name from it's inherited parent
        '==============================================================================================
        '
        Private Sub getFieldHelpMsgs(ContentID As Integer, FieldName As String, ByRef return_Default As String, ByRef return_Custom As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "getFieldHelpMsgs")
            '
            Dim SQL As String
            Dim CS As Integer
            Dim Found As Boolean
            Dim ParentID As Integer
            '
            Found = False
            SQL = "select h.HelpDefault,h.HelpCustom from ccfieldhelp h left join ccfields f on f.id=h.fieldid where f.contentid=" & ContentID & " and f.name=" & cpCore.db.encodeSQLText(FieldName)
            CS = cpCore.db.csOpenSql(SQL)
            If cpCore.db.csOk(CS) Then
                Found = True
                return_Default = cpCore.db.csGetText(CS, "helpDefault")
                return_Custom = cpCore.db.csGetText(CS, "helpCustom")
            End If
            Call cpCore.db.csClose(CS)
            '
            If Not Found Then
                ParentID = 0
                SQL = "select parentid from cccontent where id=" & ContentID
                CS = cpCore.db.csOpenSql(SQL)
                If cpCore.db.csOk(CS) Then
                    ParentID = cpCore.db.csGetInteger(CS, "parentid")
                End If
                Call cpCore.db.csClose(CS)
                If ParentID <> 0 Then
                    Call getFieldHelpMsgs(ParentID, FieldName, return_Default, return_Custom)
                End If
            End If
            '
            Exit Sub
            '
ErrorTrap:
            Throw (New Exception("unexpected exception"))
        End Sub
        '
        '===========================================================================
        ''' <summary>
        ''' handle legacy errors in this class, v3
        ''' </summary>
        ''' <param name="MethodName"></param>
        ''' <param name="Context"></param>
        ''' <remarks></remarks>
        Private Sub handleLegacyClassError3(ByVal MethodName As String, Optional ByVal Context As String = "")
            '
            Throw (New Exception("error in method [" & MethodName & "], contect [" & Context & "]"))
            '
        End Sub
        '
        '===========================================================================
        ''' <summary>
        ''' handle legacy errors in this class, v2
        ''' </summary>
        ''' <param name="MethodName"></param>
        ''' <param name="Context"></param>
        ''' <remarks></remarks>
        Private Sub handleLegacyClassError2(ByVal MethodName As String, Optional ByVal Context As String = "")
            '
            Throw (New Exception("error in method [" & MethodName & "], Context [" & Context & "]"))
            Err.Clear()
            '
        End Sub
        '
        '===========================================================================
        ''' <summary>
        ''' handle legacy errors in this class, v1
        ''' </summary>
        ''' <param name="MethodName"></param>
        ''' <param name="ErrDescription"></param>
        ''' <remarks></remarks>
        Private Sub handleLegacyClassError(MethodName As String, ErrDescription As String)
            Throw (New Exception("error in method [" & MethodName & "], ErrDescription [" & ErrDescription & "]"))
        End Sub
        'Private Sub pattern1()
        '    Dim admincontent As coreMetaDataClass.CDefClass
        '    For Each keyValuePair As KeyValuePair(Of String, coreMetaDataClass.CDefFieldClass) In admincontent.fields
        '        Dim field As coreMetaDataClass.CDefFieldClass = keyValuePair.Value
        '        '
        '    Next
        'End Sub
        '
        '====================================================================================================
        ' properties
        '====================================================================================================
        '
        ' ----- ccGroupRules storage for list of Content that a group can author
        '
        Private Structure ContentGroupRuleType
            Dim ContentID As Integer
            Dim GroupID As Integer
            Dim AllowAdd As Boolean
            Dim AllowDelete As Boolean
        End Structure
        '
        ' ----- generic id/name dictionary
        '
        Private Structure StorageType
            Dim Id As Integer
            Dim Name As String
        End Structure
        '
        ' ----- Group Rules
        '
        Private Structure GroupRuleType
            Dim GroupID As Integer
            Dim AllowAdd As Boolean
            Dim AllowDelete As Boolean
        End Structure
        '
        ' ----- Used within Admin site to create fancyBox popups
        '
        Private includeFancyBox As Boolean
        Private fancyBoxPtr As Integer
        Private fancyBoxHeadJS As String
        Private ClassInitialized As Boolean        ' if true, the module has been
        Private Const allowSaveBeforeDuplicate = False
        '
        ' ----- To interigate Add-on Collections to check for re-use
        '
        Private Structure DeleteType
            Dim Name As String
            Dim ParentID As Integer
        End Structure
        Private Structure NavigatorType
            Dim Name As String
            Dim menuNameSpace As String
        End Structure
        Private Structure Collection2Type
            Dim AddOnCnt As Integer
            Dim AddonGuid() As String
            Dim AddonName() As String
            Dim MenuCnt As Integer
            Dim Menus() As String
            Dim NavigatorCnt As Integer
            Dim Navigators() As NavigatorType
        End Structure
        Private CollectionCnt As Integer
        Private Collections() As Collection2Type
        '
        ' ----- Target Data Storage
        '
        Private requestedContentId As Integer
        Private requestedRecordId As Integer
        'Private false As Boolean    ' set if content and site support workflow authoring
        Private BlockEditForm As Boolean                    ' true if there was an error loading the edit record - use to block the edit form
        '
        ' ----- Storage for current EditRecord, loaded in LoadEditRecord
        '
        Public Class editRecordFieldClass
            Public dbValue As Object
            Public value As Object
        End Class
        '
        Public Class editRecordClass
            Public fieldsLc As New Dictionary(Of String, editRecordFieldClass)
            Public id As Integer                            ' ID field of edit record (Record to be edited)
            Public parentID As Integer                      ' ParentID field of edit record (Record to be edited)
            Public nameLc As String                         ' name field of edit record
            Public active As Boolean                        ' active field of the edit record
            Public contentControlId As Integer              ' ContentControlID of the edit record
            Public contentControlId_Name As String          '
            Public menuHeadline As String                   ' Used for Content Watch Link Label if default
            Public modifiedDate As Date                     ' Used for control section display
            Public modifiedByMemberID As Integer            '   =
            Public dateAdded As Date                        '   =
            Public createByMemberId As Integer              '   =

            Public RootPageID As Integer
            Public SetPageNotFoundPageID As Boolean
            Public SetLandingPageID As Boolean

            '
            Public Loaded As Boolean            ' true/false - set true when the field array values are loaded
            Public Saved As Boolean              ' true if edit record was saved during this page
            Public Read_Only As Boolean           ' set if this record can not be edited, for various reasons
            '
            ' From cpCore.main_GetAuthoringStatus
            '
            Public IsDeleted As Boolean          ' true means the edit record has been deleted
            Public IsInserted As Boolean         ' set if Workflow authoring insert
            Public IsModified As Boolean         ' record has been modified since last published
            Public LockModifiedName As String        ' member who first edited the record
            Public LockModifiedDate As Date          ' Date when member modified record
            Public SubmitLock As Boolean         ' set if a submit Lock, even if the current user is admin
            Public SubmittedName As String       ' member who submitted the record
            Public SubmittedDate As Date         ' Date when record was submitted
            Public ApproveLock As Boolean        ' set if an approve Lock
            Public ApprovedName As String        ' member who approved the record
            Public ApprovedDate As Date          ' Date when record was approved
            '
            ' From cpCore.main_GetAuthoringPermissions
            '
            Public AllowInsert As Boolean
            Public AllowCancel As Boolean
            Public AllowSave As Boolean
            Public AllowDelete As Boolean
            Public AllowPublish As Boolean
            Public AllowAbort As Boolean
            Public AllowSubmit As Boolean
            Public AllowApprove As Boolean
            '
            ' From cpCore.main_GetEditLock
            '
            Public EditLock As Boolean           ' set if an edit Lock by anyone else besides the current user
            Public EditLockMemberID As Integer      ' Member who edit locked the record
            Public EditLockMemberName As String  ' Member who edit locked the record
            Public EditLockExpires As Date       ' Time when the edit lock expires

        End Class
        'Private EditRecordValuesObject() As Object      ' Storage for Edit Record values
        'Private EditRecordDbValues() As Object         ' Storage for last values read from Defaults+Db, added b/c file fields need Db value to display
        'Private EditRecord.ID As Integer                    ' ID field of edit record (Record to be edited)
        'Private EditRecord.ParentID As Integer              ' ParentID field of edit record (Record to be edited)
        'Private EditRecord.Name As String                ' name field of edit record
        'Private EditRecord.Active As Boolean             ' active field of the edit record
        'Private EditRecord.ContentID As Integer             ' ContentControlID of the edit record
        'Private EditRecord.ContentName As String         '
        'Private EditRecord.MenuHeadline As String        ' Used for Content Watch Link Label if default
        'Private EditRecord.ModifiedDate As Date          ' Used for control section display
        'Private EditRecord.ModifiedByMemberID As Integer    '   =
        'Private EditRecord.AddedDate As Date             '   =
        'Private EditRecord.AddedByMemberID As Integer       '   =
        'Private EditRecord.ContentCategoryID As Integer
        'Private EditRecordRootPageID As Integer
        'Private EditRecord.SetPageNotFoundPageID As Boolean
        'Private EditRecord.SetLandingPageID As Boolean

        ''
        'Private EditRecord.Loaded As Boolean            ' true/false - set true when the field array values are loaded
        'Private EditRecord.Saved As Boolean              ' true if edit record was saved during this page
        'Private editrecord.read_only As Boolean           ' set if this record can not be edited, for various reasons
        ''
        '' From cpCore.main_GetAuthoringStatus
        ''
        'Private EditRecord.IsDeleted As Boolean          ' true means the edit record has been deleted
        'Private EditRecord.IsInserted As Boolean         ' set if Workflow authoring insert
        'Private EditRecord.IsModified As Boolean         ' record has been modified since last published
        'Private EditRecord.LockModifiedName As String        ' member who first edited the record
        'Private EditRecord.LockModifiedDate As Date          ' Date when member modified record
        'Private EditRecord.SubmitLock As Boolean         ' set if a submit Lock, even if the current user is admin
        'Private EditRecord.SubmittedName As String       ' member who submitted the record
        'Private EditRecordSubmittedDate As Date         ' Date when record was submitted
        'Private EditRecord.ApproveLock As Boolean        ' set if an approve Lock
        'Private EditRecord.ApprovedName As String        ' member who approved the record
        'Private EditRecordApprovedDate As Date          ' Date when record was approved
        ''
        '' From cpCore.main_GetAuthoringPermissions
        ''
        'Private EditRecord.AllowInsert As Boolean
        'Private EditRecord.AllowCancel As Boolean
        'Private EditRecord.AllowSave As Boolean
        'Private EditRecord.AllowDelete As Boolean
        'Private EditRecord.AllowPublish As Boolean
        'Private EditRecord.AllowAbort As Boolean
        'Private EditRecord.AllowSubmit As Boolean
        'Private EditRecord.AllowApprove As Boolean
        ''
        '' From cpCore.main_GetEditLock
        ''
        'Private EditRecord.EditLock As Boolean           ' set if an edit Lock by anyone else besides the current user
        'Private EditRecord.EditLockMemberID As Integer      ' Member who edit locked the record
        'Private EditRecord.EditLockMemberName As String  ' Member who edit locked the record
        'Private EditRecord.EditLockExpires As Date       ' Time when the edit lock expires
        ''
        '
        '=============================================================================
        ' ----- Control Response
        '=============================================================================
        '
        Private AdminButton As String                ' Value returned from a submit button, process into action/form
        Private AdminAction As Integer                 ' The action to be performed before the next form
        Private AdminForm As Integer                   ' The next form to print
        Private AdminSourceForm As Integer             ' The form that submitted that the button to process
        Private WherePair(2, 10) As String                ' for passing where clause values from page to page
        Private WherePairCount As Integer                 ' the current number of WherePairCount in use
        'Private OrderByFieldPointer as integer
        Private Const OrderByFieldPointerDefault = -1
        'Private Direction as integer
        Private RecordTop As Integer
        Private RecordsPerPage As Integer
        Private Const RecordsPerPageDefault = 50
        'Private InputFieldName As String   ' Input FieldName used for DHTMLEdit

        Private MenuDepth As Integer                   ' The number of windows open (below this one)
        Private TitleExtension As String              ' String that adds on to the end of the title
        'Private Findstring(50) As String                ' Value to search for each index column
        '
        ' SpellCheck Features
        '
        Private SpellCheckSupported As Boolean      ' if true, spell checking is supported
        Private SpellCheckRequest As Boolean        ' If true, send the spell check form to the browser
        Private SpellCheckResponse As Boolean       ' if true, the user is sending the spell check back to process
        Private SpellCheckWhiteCharacterList As String
        Private SpellCheckDictionaryFilename As String  ' Full path to user dictionary
        Private SpellCheckIgnoreList As String      ' List of ignore words (used to verify the file is there)
        '
        '=============================================================================
        ' preferences
        '=============================================================================
        '
        Private AdminMenuModeID As Integer         ' Controls the menu mode, set from cpCore.main_MemberAdminMenuModeID
        Private allowAdminTabs As Boolean       ' true uses tab system
        Private fieldEditorPreference As String     ' this is a hidden on the edit form. The popup editor preferences sets this hidden and submits
        '
        '=============================================================================
        '   Content Tracking Editing
        '
        '   These values are read from Edit form response, and are used to populate then
        '   ContentWatch and ContentWatchListRules records.
        '
        '   They are read in before the current record is processed, then processed and
        '   Saved back to ContentWatch and ContentWatchRules after the current record is
        '   processed, so changes to the record can be reflected in the ContentWatch records.
        '   For instance, if the record is marked inactive, the ContentWatchLink is cleared
        '   and all ContentWatchListRules are deleted.
        '
        '=============================================================================
        '
        Private ContentWatchLoaded As Boolean               ' flag set that shows the rest are valid
        '
        Private ContentWatchRecordID As Integer
        Private ContentWatchLink As String
        Private ContentWatchClicks As Integer
        Private ContentWatchLinkLabel As String
        Private ContentWatchExpires As Date
        Private ContentWatchListID() As Integer            ' list of all ContentWatchLists for this Content, read from response, then later saved to Rules
        Private ContentWatchListIDSize As Integer          ' size of ContentWatchListID() array
        Private ContentWatchListIDCount As Integer         ' number of valid entries in ContentWatchListID()
        ''
        ''=============================================================================
        ''   Calendar Event Editing
        ''=============================================================================
        ''
        'Private CalendarEventName As String
        'Private CalendarEventStartDate As Date
        'Private CalendarEventEndDate As Date
        '
        '=============================================================================
        ' Other
        '=============================================================================
        '
        Private ObjectCount As Integer            ' Convert the following objects to this one
        Private ButtonObjectCount As Integer           ' Count of Buttons in use
        Private ImagePreloadCount As Integer           ' Number of images preloaded
        Private ImagePreloads(2, 100) As String       ' names of all gifs already preloaded
        '                       (0,x) = imagename
        '                       (1,x) = ImageObject name for the image
        Private JavaScriptString As String            ' Collected string of Javascript functions to print at end
        Private AdminFormBottom As String   ' the HTML needed to complete the Admin Form after contents
        Private UserAllowContentEdit As Boolean         ' set on load - checked within each edit/index page
        Private UserAllowContentAdd As Boolean
        Private UserAllowContentDelete As Boolean
        Private TabStopCount As Integer                ' used to generate TabStop values
        Private FormInputCount As Integer              ' used to generate labels for form input
        Private EditSectionPanelCount As Integer

        Const OpenLiveWindowTable = "<div ID=""LiveWindowTable"">"
        Const CloseLiveWindowTable = "</div>"
        'Const OpenLiveWindowTable = "<table ID=""LiveWindowTable"" border=0 cellpadding=0 cellspacing=0 width=""100%""><tr><td>"
        'Const CloseLiveWindowTable = "</td></tr></table>"
        '
        'Const adminui.EditTableClose = "<tr>" _
        '        & "<td width=20%><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""1"" ></td>" _
        '        & "<td width=""70%""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""1"" ></td>" _
        '        & "<td width=""10%""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""1"" ></td>" _
        '        & "</tr>" _
        '        & "</table>"
        Const AdminFormErrorOpen = "<table border=""0"" cellpadding=""20"" cellspacing=""0"" width=""100%""><tr><td align=""left"">"
        Const AdminFormErrorClose = "</td></tr></table>"
        '
        ' these were defined different in csv
        '
        'Private Const ContentTypeMember = 1
        'Private Const ContentTypePaths = 2
        'Private Const csv_contenttypeenum.contentTypeEmail = 3
        'Private Const ContentTypeContent = 4
        'Private Const ContentTypeSystem = 5
        'Private Const ContentTypeNormal = 6
        '
        '
        '
        Private Const RequestNameAdminDepth = "ad"
        Private Const RequestNameAdminForm = "af"
        Private Const RequestNameAdminSourceForm = "asf"
        Private Const RequestNameAdminAction = "aa"
        'Private Const RequestNameFieldName = "fn"
        Private Const RequestNameTitleExtension = "tx"
        '
        '
        ''
        ''Private AdminContentCellBackgroundColor As String
        ''
        Public Enum NodeTypeEnum
            NodeTypeEntry = 0
            NodeTypeCollection = 1
            NodeTypeAddon = 2
            NodeTypeContent = 3
        End Enum
        '
        Private Const IndexConfigPrefix = "IndexConfig:"
        '
        Public Enum FindWordMatchEnum
            MatchIgnore = 0
            MatchEmpty = 1
            MatchNotEmpty = 2
            MatchGreaterThan = 3
            MatchLessThan = 4
            matchincludes = 5
            MatchEquals = 6
            MatchTrue = 7
            MatchFalse = 8
        End Enum
        '
        '
        '
        Public Class indexConfigSortClass
            'Dim FieldPtr As Integer
            Public fieldName As String
            Public direction As Integer ' 1=forward, 2=reverse, 0=ignore/remove this sort
        End Class
        '
        Public Class indexConfigFindWordClass
            Public Name As String
            Public Value As String
            Public Type As Integer
            Public MatchOption As FindWordMatchEnum
        End Class
        '
        Public Class indexConfigColumnClass
            Public Name As String
            'Public FieldId As Integer
            Public Width As Integer
            Public SortPriority As Integer
            Public SortDirection As Integer
        End Class
        '
        Public Class indexConfigClass
            Public Loaded As Boolean
            Public ContentID As Integer
            Public PageNumber As Integer
            Public RecordsPerPage As Integer
            Public RecordTop As Integer

            'FindWordList As String
            Public FindWords As New Dictionary(Of String, indexConfigFindWordClass)
            'Public FindWordCnt As Integer
            Public ActiveOnly As Boolean
            Public LastEditedByMe As Boolean
            Public LastEditedToday As Boolean
            Public LastEditedPast7Days As Boolean
            Public LastEditedPast30Days As Boolean
            Public Open As Boolean
            'public SortCnt As Integer
            Public Sorts As New Dictionary(Of String, indexConfigSortClass)
            Public GroupListCnt As Integer
            Public GroupList() As String
            'public ColumnCnt As Integer
            Public Columns As New Dictionary(Of String, indexConfigColumnClass)
            'SubCDefs() as integer
            'SubCDefCnt as integer
            Public SubCDefID As Integer
        End Class
        '
        ' Temp
        '
        Const ToolsActionMenuMove = 1
        Const ToolsActionAddField = 2            ' Add a field to the Index page
        Const ToolsActionRemoveField = 3
        Const ToolsActionMoveFieldRight = 4
        Const ToolsActionMoveFieldLeft = 5
        Const ToolsActionSetAZ = 6
        Const ToolsActionSetZA = 7
        Const ToolsActionExpand = 8
        Const ToolsActionContract = 9
        Const ToolsActionEditMove = 10
        Const ToolsActionRunQuery = 11
        Const ToolsActionDuplicateDataSource = 12
        Const ToolsActionDefineContentFieldFromTableFieldsFromTable = 13
        Const ToolsActionFindAndReplace = 14
        '
        Private AllowAdminFieldCheck_Local As Boolean
        Private AllowAdminFieldCheck_LocalLoaded As Boolean
        '
        Private Const AddonGuidPreferences = "{D9C2D64E-9004-4DBE-806F-60635B9F52C8}"
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function admin_GetAdminFormBody(Caption As String, ButtonListLeft As String, ButtonListRight As String, AllowAdd As Boolean, AllowDelete As Boolean, Description As String, ContentSummary As String, ContentPadding As Integer, Content As String) As String
            Return New adminUIController(cpCore).GetBody(Caption, ButtonListLeft, ButtonListRight, AllowAdd, AllowDelete, Description, ContentSummary, ContentPadding, Content)
        End Function
        '
    End Class
End Namespace
