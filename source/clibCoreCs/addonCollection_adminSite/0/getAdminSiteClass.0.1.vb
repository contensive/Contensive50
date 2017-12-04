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



    End Class
End Namespace
