
Option Explicit On
Option Strict On

Imports System.Xml

Namespace Contensive.Core
    Public Class coreXmlToolsClass
        '
        '========================================================================
        ' This page and its contents are copyright by Kidwell McGowan Associates.
        '========================================================================
        '
        ' ----- global scope variables
        '
        Private iAbort As Boolean
        Private iBusy As Integer
        Private iTaskCount As Integer
        Const ApplicationNameLocal = "unknown"
        Private cpCore As cpCoreClass
        '
        '====================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As cpCoreClass)
            Me.cpCore = cpCore
        End Sub
        '
        '========================================================================
        ' ----- Save all content to an XML Stream
        '   4/28/08 - changed so content is read from Db using RS/Conn, not cache version
        '========================================================================
        '
        Public Function GetXMLContentDefinition2(Optional ByVal ContentName As String = "") As String
            GetXMLContentDefinition2 = GetXMLContentDefinition3(ContentName, False)
        End Function
        '
        '========================================================================
        ' ----- Save all content to an XML Stream
        '   4/28/08 - changed so content is read from Db using RS/Conn, not cache version
        '   2/20/2010 - changed to include includebasefield
        '========================================================================
        '
        Public Function GetXMLContentDefinition3(Optional ByVal ContentName As String = "", Optional ByVal IncludeBaseFields As Boolean = False) As String
            On Error GoTo ErrorTrap
            '
            Const ContentSelectList = "" _
                & " id,name,active,adminonly,allowadd" _
                & ",allowcalendarevents,allowcontentchildtool,allowcontenttracking,allowdelete,allowmetacontent" _
                & ",allowtopicrules,AllowWorkflowAuthoring,AuthoringTableID" _
                & ",ContentTableID,DefaultSortMethodID,DeveloperOnly,DropDownFieldList" _
                & ",EditorGroupID,ParentID,ccGuid,IsBaseContent" _
                & ",IconLink,IconHeight,IconWidth,IconSprites"

            Const FieldSelectList = "" _
                & "f.ID,f.Name,f.contentid,f.Active,f.AdminOnly,f.Authorable,f.Caption,f.DeveloperOnly,f.EditSortPriority,f.Type,f.HTMLContent" _
                & ",f.IndexColumn,f.IndexSortDirection,f.IndexSortPriority,f.RedirectID,f.RedirectPath,f.Required" _
                & ",f.TextBuffered,f.UniqueName,f.DefaultValue,f.RSSTitleField,f.RSSDescriptionField,f.MemberSelectGroupID" _
                & ",f.EditTab,f.Scramble,f.LookupList,f.NotEditable,f.Password,f.readonly,f.ManyToManyRulePrimaryField" _
                & ",f.ManyToManyRuleSecondaryField,'' as HelpMessageDeprecated,f.ModifiedBy,f.IsBaseField,f.LookupContentID" _
                & ",f.RedirectContentID,f.ManyToManyContentID,f.ManyToManyRuleContentID" _
                & ",h.helpdefault,h.helpcustom,f.IndexWidth"

            Const f_ID = 0
            Const f_Name = 1
            Const f_contentid = 2
            Const f_Active = 3
            Const f_AdminOnly = 4
            Const f_Authorable = 5
            Const f_Caption = 6
            Const f_DeveloperOnly = 7
            Const f_EditSortPriority = 8
            Const f_Type = 9
            Const f_HTMLContent = 10
            Const f_IndexColumn = 11
            Const f_IndexSortDirection = 12
            Const f_IndexSortPriority = 13
            Const f_RedirectID = 14
            Const f_RedirectPath = 15
            Const f_Required = 16
            Const f_TextBuffered = 17
            Const f_UniqueName = 18
            Const f_DefaultValue = 19
            Const f_RSSTitleField = 20
            Const f_RSSDescriptionField = 21
            Const f_MemberSelectGroupID = 22
            Const f_EditTab = 23
            Const f_Scramble = 24
            Const f_LookupList = 25
            Const f_NotEditable = 26
            Const f_Password = 27
            Const f_ReadOnly = 28
            Const f_ManyToManyRulePrimaryField = 29
            Const f_ManyToManyRuleSecondaryField = 30
            Const f_HelpMessageDeprecated = 31
            Const f_ModifiedBy = 32
            Const f_IsBaseField = 33
            Const f_LookupContentID = 34
            Const f_RedirectContentID = 35
            Const f_ManyToManyContentID = 36
            Const f_ManyToManyRuleContentID = 37
            Const f_helpdefault = 38
            Const f_helpcustom = 39
            Const f_IndexWidth = 40
            '
            Dim IsBaseContent As Boolean
            Dim FieldCnt As Integer
            Dim FieldName As String
            Dim FieldContentID As Integer
            Dim LastFieldID As Integer
            Dim RecordID As Integer
            Dim RecordName As String
            Dim AuthoringTableID As Integer
            Dim HelpDefault As String
            Dim HelpCustom As String
            Dim HelpCnt As Integer
            Dim fieldId As Integer
            Dim fieldType As String
            Dim TableID As Integer
            Dim ContentTableID As Integer
            Dim TableName As String
            Dim DataSourceID As Integer
            Dim DataSourceName As String
            'Dim RSTable as datatable
            Dim DefaultSortMethodID As Integer
            Dim DefaultSortMethod As String
            Dim EditorGroupID As Integer
            Dim EditorGroupName As String
            Dim ParentID As Integer
            Dim ParentName As String
            Dim CSContent As Integer
            Dim ContentID As Integer
            Dim CSDataSources As Integer
            Dim sb As New System.Text.StringBuilder
            Dim CDef As coreMetaDataClass.CDefClass
            Dim CDefPointer As Integer
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim iContentName As String
            Dim CDefPointerMin As Integer
            Dim CDefPointerMax As Integer
            Dim CDefArray() As coreMetaDataClass.CDefClass
            Dim CDefArrayCount As Integer
            Dim AllowContentChildTool As Boolean
            Dim CSField As Integer
            Dim RS As DataTable
            Dim RSF As DataTable
            Dim RSH As DataTable
            Dim SQL As String
            Dim FoundMenuTable As Boolean
            'Dim FoundAFTable As Boolean
            Dim tickStart As Integer
            Dim Ptr As Integer
            Dim Tables As String(,)
            Dim TableCnt As Integer
            Dim Sorts As String(,)
            Dim SortCnt As Integer
            Dim Groups As String(,)
            Dim GroupCnt As Integer
            Dim Contents As String(,)
            Dim ContentCnt As Integer
            'Dim ContentSrc as object
            'Dim ContentSrcCnt as integer
            'Dim ContentSrcPtr as integer
            Dim CFields As String(,)
            Dim CFieldCnt As Integer
            Dim CFieldPtr As Integer
            Dim appName As String
            'Dim sb As System.Text.StringBuilder

            '
            'cpCore.AppendLog("getXmlContentDefinition, entry")
            tickStart = GetTickCount
            appName = cpCore.app.config.name
            iContentName = ContentName
            If iContentName <> "" Then
                SQL = "select id from cccontent where name=" & cpCore.app.db_EncodeSQLText(iContentName)
                RS = cpCore.app.executeSql(SQL)
                If RS.Rows.Count > 0 Then
                    ContentID = EncodeInteger(RS.Rows(0).Item("id"))
                End If
            End If
            If iContentName <> "" And (ContentID = 0) Then
                '
                ' export requested for content name that does not exist - return blank
                '
            Else
                '
                ' Build table lookup
                '
                SQL = "select T.ID,T.Name as TableName,D.Name as DataSourceName from ccTables T Left Join ccDataSources D on D.ID=T.DataSourceID"
                RS = cpCore.app.executeSql(SQL)
                Tables = convertDataTabletoArray(RS)
                If Tables Is Nothing Then
                    TableCnt = 0
                Else
                    TableCnt = UBound(Tables, 2) + 1
                End If
                '
                ' Build SortMethod lookup
                '
                SQL = "select ID,Name from ccSortMethods"
                RS = cpCore.app.executeSql(SQL)
                Sorts = convertDataTabletoArray(RS)
                If Sorts Is Nothing Then
                    SortCnt = 0
                Else
                    SortCnt = UBound(Sorts, 2) + 1
                End If
                '
                ' Build SortMethod lookup
                '
                SQL = "select ID,Name from ccGroups"
                RS = cpCore.app.executeSql(SQL)
                Groups = convertDataTabletoArray(RS)
                If Groups Is Nothing Then
                    GroupCnt = 0
                Else
                    GroupCnt = UBound(Groups, 2) + 1
                End If
                '
                ' Build Content lookup
                '
                SQL = "select id,name from ccContent"
                RS = cpCore.app.executeSql(SQL)
                Contents = convertDataTabletoArray(RS)
                If Contents Is Nothing Then
                    ContentCnt = 0
                Else
                    ContentCnt = UBound(Contents, 2) + 1
                End If
                '
                ' select all the fields
                '
                If ContentID <> 0 Then
                    SQL = "select " & FieldSelectList & "" _
                        & " from ccfields f left join ccfieldhelp h on h.fieldid=f.id" _
                        & " where (f.Type<>0)and(f.contentid=" & ContentID & ")" _
                        & ""
                Else
                    SQL = "select " & FieldSelectList & "" _
                        & " from ccfields f left join ccfieldhelp h on h.fieldid=f.id" _
                        & " where (f.Type<>0)" _
                        & ""
                End If
                If Not IncludeBaseFields Then
                    SQL &= " and ((f.IsBaseField is null)or(f.IsBaseField=0))"
                End If
                SQL &= " order by f.contentid,f.id,h.id desc"
                RS = cpCore.app.executeSql(SQL)
                CFields = convertDataTabletoArray(RS)
                CFieldCnt = UBound(CFields, 2) + 1
                '
                ' select the content
                '
                If ContentID <> 0 Then
                    SQL = "select " & ContentSelectList & " from ccContent where (id=" & ContentID & ")and(contenttableid is not null)and(contentcontrolid is not null) order by id"
                Else
                    SQL = "select " & ContentSelectList & " from ccContent where (name<>'')and(name is not null)and(contenttableid is not null)and(contentcontrolid is not null) order by id"
                End If
                RS = cpCore.app.executeSql(SQL)
                '
                ' create output
                '
                CFieldPtr = 0
                For Each dr As DataRow In RS.Rows
                    '
                    ' ----- <cdef>
                    '
                    IsBaseContent = EncodeBoolean(dr("isBaseContent"))
                    iContentName = GetRSXMLAttribute(appName, dr, "Name")
                    If InStr(1, iContentName, "data sources", vbTextCompare) = 1 Then
                        iContentName = iContentName
                    End If
                    ContentID = EncodeInteger(dr("ID"))
                    sb.Append(vbCrLf & vbTab & "<CDef")
                    sb.Append(" Name=""" & iContentName & """")
                    If (Not IsBaseContent) Or IncludeBaseFields Then
                        sb.Append(" Active=""" & GetRSXMLAttribute(appName, dr, "Active") & """")
                        sb.Append(" AdminOnly=""" & GetRSXMLAttribute(appName, dr, "AdminOnly") & """")
                        'sb.Append( " AliasID=""" & GetRSXMLAttribute( appname,RS, "AliasID") & """")
                        'sb.Append( " AliasName=""" & GetRSXMLAttribute( appname,RS, "AliasName") & """")
                        sb.Append(" AllowAdd=""" & GetRSXMLAttribute(appName, dr, "AllowAdd") & """")
                        sb.Append(" AllowCalendarEvents=""" & GetRSXMLAttribute(appName, dr, "AllowCalendarEvents") & """")
                        sb.Append(" AllowContentChildTool=""" & GetRSXMLAttribute(appName, dr, "AllowContentChildTool") & """")
                        sb.Append(" AllowContentTracking=""" & GetRSXMLAttribute(appName, dr, "AllowContentTracking") & """")
                        sb.Append(" AllowDelete=""" & GetRSXMLAttribute(appName, dr, "AllowDelete") & """")
                        sb.Append(" AllowMetaContent=""" & GetRSXMLAttribute(appName, dr, "AllowMetaContent") & """")
                        sb.Append(" AllowTopicRules=""" & GetRSXMLAttribute(appName, dr, "AllowTopicRules") & """")
                        sb.Append(" AllowWorkflowAuthoring=""" & GetRSXMLAttribute(appName, dr, "AllowWorkflowAuthoring") & """")
                        '
                        AuthoringTableID = EncodeInteger(dr("AuthoringTableID"))
                        TableName = ""
                        DataSourceName = ""
                        If AuthoringTableID <> 0 Then
                            For Ptr = 0 To TableCnt - 1
                                If EncodeInteger(Tables(0, Ptr)) = AuthoringTableID Then
                                    TableName = EncodeText(Tables(1, Ptr))
                                    DataSourceName = EncodeText(Tables(2, Ptr))
                                    Exit For
                                End If
                            Next
                        End If
                        If DataSourceName = "" Then
                            DataSourceName = "Default"
                        End If
                        If UCase(TableName) = "CCMENUENTRIES" Then
                            FoundMenuTable = True
                        End If
                        sb.Append(" AuthoringDataSourceName=""" & EncodeXMLattribute(DataSourceName) & """")
                        sb.Append(" AuthoringTableName=""" & EncodeXMLattribute(TableName) & """")
                        '
                        ContentTableID = EncodeInteger(dr("ContentTableID"))
                        If ContentTableID <> AuthoringTableID Then
                            If ContentTableID <> 0 Then
                                TableName = ""
                                DataSourceName = ""
                                For Ptr = 0 To TableCnt - 1
                                    If EncodeInteger(Tables(0, Ptr)) = ContentTableID Then
                                        TableName = EncodeText(Tables(1, Ptr))
                                        DataSourceName = EncodeText(Tables(2, Ptr))
                                        Exit For
                                    End If
                                Next
                                If DataSourceName = "" Then
                                    DataSourceName = "Default"
                                End If
                            End If
                        End If
                        sb.Append(" ContentDataSourceName=""" & EncodeXMLattribute(DataSourceName) & """")
                        sb.Append(" ContentTableName=""" & EncodeXMLattribute(TableName) & """")
                        '
                        DefaultSortMethodID = EncodeInteger(dr("DefaultSortMethodID"))
                        DefaultSortMethod = CacheLookup(DefaultSortMethodID, Sorts)
                        sb.Append(" DefaultSortMethod=""" & EncodeXMLattribute(DefaultSortMethod) & """")
                        '
                        sb.Append(" DeveloperOnly=""" & GetRSXMLAttribute(appName, dr, "DeveloperOnly") & """")
                        sb.Append(" DropDownFieldList=""" & GetRSXMLAttribute(appName, dr, "DropDownFieldList") & """")
                        '
                        EditorGroupID = EncodeInteger(dr("EditorGroupID"))
                        EditorGroupName = CacheLookup(EditorGroupID, Groups)
                        sb.Append(" EditorGroupName=""" & EncodeXMLattribute(EditorGroupName) & """")
                        '
                        ParentID = EncodeInteger(dr("ParentID"))
                        ParentName = CacheLookup(ParentID, Contents)
                        sb.Append(" Parent=""" & EncodeXMLattribute(ParentName) & """")
                        '
                        sb.Append(" IconLink=""" & GetRSXMLAttribute(appName, dr, "IconLink") & """")
                        sb.Append(" IconHeight=""" & GetRSXMLAttribute(appName, dr, "IconHeight") & """")
                        sb.Append(" IconWidth=""" & GetRSXMLAttribute(appName, dr, "IconWidth") & """")
                        sb.Append(" IconSprites=""" & GetRSXMLAttribute(appName, dr, "IconSprites") & """")
                        '
                        '
                        If True Then
                            '
                            ' Add IsBaseContent
                            '
                            sb.Append(" isbasecontent=""" & GetRSXMLAttribute(appName, dr, "IsBaseContent") & """")
                        End If
                    End If
                    '
                    If True Then
                        '
                        ' Add guid
                        '
                        sb.Append(" guid=""" & GetRSXMLAttribute(appName, dr, "ccGuid") & """")
                    End If
                    sb.Append(" >")
                    '
                    ' ----- <field>
                    '
                    FieldCnt = 0
                    fieldId = 0
                    Do While (CFieldPtr < CFieldCnt)
                        LastFieldID = fieldId
                        fieldId = EncodeInteger(CFields(f_ID, CFieldPtr))
                        FieldName = EncodeText(CFields(f_Name, CFieldPtr))
                        FieldContentID = EncodeInteger(CFields(f_contentid, CFieldPtr))
                        If FieldContentID > ContentID Then
                            Exit Do
                        ElseIf (FieldContentID = ContentID) And (fieldId <> LastFieldID) Then
                            If IncludeBaseFields Or (InStr(1, ",id,ContentCategoryID,dateadded,createdby,modifiedby,EditBlank,EditArchive,EditSourceID,ContentControlID,CreateKey,ModifiedDate,ccguid,", "," & FieldName & ",", vbTextCompare) = 0) Then
                                sb.Append(vbCrLf & vbTab & vbTab & "<Field")
                                fieldType = cpCore.app.getFieldTypeNameFromFieldTypeId(EncodeInteger(CFields(f_Type, CFieldPtr)))
                                sb.Append(" Name=""" & xaT(FieldName) & """")
                                sb.Append(" active=""" & xaB(CFields(f_Active, CFieldPtr)) & """")
                                sb.Append(" AdminOnly=""" & xaB(CFields(f_AdminOnly, CFieldPtr)) & """")
                                sb.Append(" Authorable=""" & xaB(CFields(f_Authorable, CFieldPtr)) & """")
                                sb.Append(" Caption=""" & xaT(CFields(f_Caption, CFieldPtr)) & """")
                                sb.Append(" DeveloperOnly=""" & xaB(CFields(f_DeveloperOnly, CFieldPtr)) & """")
                                sb.Append(" EditSortPriority=""" & xaT(CFields(f_EditSortPriority, CFieldPtr)) & """")
                                sb.Append(" FieldType=""" & fieldType & """")
                                sb.Append(" HTMLContent=""" & xaB(CFields(f_HTMLContent, CFieldPtr)) & """")
                                sb.Append(" IndexColumn=""" & xaT(CFields(f_IndexColumn, CFieldPtr)) & """")
                                sb.Append(" IndexSortDirection=""" & xaT(CFields(f_IndexSortDirection, CFieldPtr)) & """")
                                sb.Append(" IndexSortOrder=""" & xaT(CFields(f_IndexSortPriority, CFieldPtr)) & """")
                                sb.Append(" IndexWidth=""" & xaT(CFields(f_IndexWidth, CFieldPtr)) & """")
                                sb.Append(" RedirectID=""" & xaT(CFields(f_RedirectID, CFieldPtr)) & """")
                                sb.Append(" RedirectPath=""" & xaT(CFields(f_RedirectPath, CFieldPtr)) & """")
                                sb.Append(" Required=""" & xaB(CFields(f_Required, CFieldPtr)) & """")
                                sb.Append(" TextBuffered=""" & xaB(CFields(f_TextBuffered, CFieldPtr)) & """")
                                sb.Append(" UniqueName=""" & xaB(CFields(f_UniqueName, CFieldPtr)) & """")
                                sb.Append(" DefaultValue=""" & xaT(CFields(f_DefaultValue, CFieldPtr)) & """")
                                sb.Append(" RSSTitle=""" & xaB(CFields(f_RSSTitleField, CFieldPtr)) & """")
                                sb.Append(" RSSDescription=""" & xaB(CFields(f_RSSDescriptionField, CFieldPtr)) & """")
                                sb.Append(" MemberSelectGroupID=""" & xaT(CFields(f_MemberSelectGroupID, CFieldPtr)) & """")
                                sb.Append(" EditTab=""" & xaT(CFields(f_EditTab, CFieldPtr)) & """")
                                sb.Append(" Scramble=""" & xaB(CFields(f_Scramble, CFieldPtr)) & """")
                                sb.Append(" LookupList=""" & xaT(CFields(f_LookupList, CFieldPtr)) & """")
                                sb.Append(" NotEditable=""" & xaB(CFields(f_NotEditable, CFieldPtr)) & """")
                                sb.Append(" Password=""" & xaB(CFields(f_Password, CFieldPtr)) & """")
                                sb.Append(" ReadOnly=""" & xaB(CFields(f_ReadOnly, CFieldPtr)) & """")
                                sb.Append(" ManyToManyRulePrimaryField=""" & xaT(CFields(f_ManyToManyRulePrimaryField, CFieldPtr)) & """")
                                sb.Append(" ManyToManyRuleSecondaryField=""" & xaT(CFields(f_ManyToManyRuleSecondaryField, CFieldPtr)) & """")
                                sb.Append(" IsModified=""" & (EncodeInteger(CFields(f_ModifiedBy, CFieldPtr)) <> 0) & """")
                                If True Then
                                    sb.Append(" IsBaseField=""" & xaB(CFields(f_IsBaseField, CFieldPtr)) & """")
                                End If
                                '
                                RecordID = EncodeInteger(CFields(f_LookupContentID, CFieldPtr))
                                RecordName = CacheLookup(RecordID, Contents)
                                sb.Append(" LookupContent=""" & EncodeHTML(RecordName) & """")
                                '
                                RecordID = EncodeInteger(CFields(f_RedirectContentID, CFieldPtr))
                                RecordName = CacheLookup(RecordID, Contents)
                                sb.Append(" RedirectContent=""" & EncodeHTML(RecordName) & """")
                                '
                                RecordID = EncodeInteger(CFields(f_ManyToManyContentID, CFieldPtr))
                                RecordName = CacheLookup(RecordID, Contents)
                                sb.Append(" ManyToManyContent=""" & EncodeHTML(RecordName) & """")
                                '
                                RecordID = EncodeInteger(CFields(f_ManyToManyRuleContentID, CFieldPtr))
                                RecordName = CacheLookup(RecordID, Contents)
                                sb.Append(" ManyToManyRuleContent=""" & EncodeHTML(RecordName) & """")
                                '
                                sb.Append(" >")
                                '
                                HelpCnt = 0
                                '                    HelpDefault = xaT(CFields(f_helpdefault, CFieldPtr))
                                '                    If HelpDefault <> "" Then
                                '                        sb.Append( vbCrLf & vbTab & vbTab & vbTab & "<HelpDefault>" & HelpDefault & "</HelpDefault>")
                                '                        HelpCnt = HelpCnt + 1
                                '                    End If
                                HelpDefault = xaT(CFields(f_helpcustom, CFieldPtr))
                                If HelpDefault = "" Then
                                    HelpDefault = xaT(CFields(f_helpdefault, CFieldPtr))
                                End If
                                If HelpDefault <> "" Then
                                    sb.Append(vbCrLf & vbTab & vbTab & vbTab & "<HelpDefault>" & HelpDefault & "</HelpDefault>")
                                    HelpCnt = HelpCnt + 1
                                End If
                                '                            HelpCustom = xaT(CFields(f_helpcustom, CFieldPtr))
                                '                            If HelpCustom <> "" Then
                                '                                sb.Append( vbCrLf & vbTab & vbTab & vbTab & "<HelpCustom>" & HelpCustom & "</HelpCustom>")
                                '                                HelpCnt = HelpCnt + 1
                                '                            End If
                                If HelpCnt > 0 Then
                                    sb.Append(vbCrLf & vbTab & vbTab)
                                End If
                                sb.Append("</Field>")
                            End If
                            FieldCnt = FieldCnt + 1
                        End If
                        CFieldPtr = CFieldPtr + 1
                    Loop
                    '
                    If FieldCnt > 0 Then
                        sb.Append(vbCrLf & vbTab)
                    End If
                    sb.Append("</CDef>")
                Next
                If ContentName = "" Then
                    '
                    ' Add other areas of the CDef file
                    '
                    sb.Append(GetXMLContentDefinition_SQLIndexes())
                    If FoundMenuTable Then
                        sb.Append(GetXMLContentDefinition_AdminMenus())
                    End If
                    '
                    ' These are not needed anymore - later add "ImportCollection" entries for all collections installed
                    '
                    '        If FoundAFTable Then
                    '            sb.Append( GetXMLContentDefinition_AggregateFunctions()
                    '        End If
                End If
                GetXMLContentDefinition3 = "<" & CollectionFileRootNode & " name=""Application"" guid=""" & ApplicationCollectionGuid & """>" & sb.ToString & vbCrLf & "</" & CollectionFileRootNode & ">"
            End If
            '
            'cpCore.AppendLog("getXmlContentDefinition, exit")
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call HandleClassErrorAndBubble(appName, "GetXMLContentDefinition3")
        End Function
        '
        '========================================================================
        ' ----- Save all content to an XML Stream
        '   4/28/08 - changed so content is read from Db using RS/Conn, not cache version
        '========================================================================
        '
        Public Function GetXMLContentDefinition(Optional ByVal ContentName As String = "") As String
            '
            Dim appName As String
            '
            appName = cpCore.app.config.name
            GetXMLContentDefinition = GetXMLContentDefinition3(ContentName, False)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call HandleClassErrorAndBubble(appName, "GetXMLContentDefinition")
        End Function
        ''
        ''========================================================================
        '' ----- Save all content to an XML Stream
        ''========================================================================
        ''
        'Private Function GetXMLContent(cmc as appServicesClass, ContentName As String) As String
        '    On Error GoTo ErrorTrap
        '    '
        '    Dim CS as integer
        '    Dim sb as new system.text.stringBuilder
        '    Dim CDefPointer as integer
        '    Dim CDefArrayCount as integer
        '    Dim CSRows as object
        '    Dim CSRowCaptions as object
        '    Dim RowCount as integer
        '    Dim RowPointer as integer
        '    Dim ColumnCount as integer
        '    Dim ColumnPointer as integer
        '    '
        '    sb.append( "<ContensiveContent>" & vbCrLf)
        '    If ContentName <> "" Then
        '        Call sb.append("<CDef Name=""" & ContentName & """>" & vbCrLf)
        '        CS = cpCore.db_csOpen(ContentName)
        '        CSRows = cpCore.Csv_GetCSRows(CS)
        '        RowCount = UBound(CSRows, 2)
        '        CSRowCaptions = cpCore.Csv_GetCSRowFields(CS)
        '        ColumnCount = UBound(CSRowCaptions)
        '        For RowPointer = 0 To RowCount - 1
        '            sb.append( "<CR>")
        '            For ColumnPointer = 0 To ColumnCount - 1
        '                sb.append( "<CC Name=""" & CSRowCaptions(ColumnPointer) & """>")
        '                sb.append( CSRows(RowPointer, ColumnPointer))
        '                sb.append( "</CC>")
        '                Next
        '            sb.append( "</CR>" & vbCrLf)
        '            Next
        '        sb.append( "</CDef>" & vbCrLf)
        '        End If
        '    sb.append( "</ContensiveContent>" & vbCrLf)
        '    GetXMLContent = sb.tostring
        '    '
        '    Exit Function
        '    '
        '    ' ----- Error Trap
        '    '
        'ErrorTrap:
        '    Call HandleClassErrorAndBubble(appname,"GetXMLContent")
        'End Function
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
            GetXMLAttribute = ""
            Found = False
            'Set REsultNode = Node.Attributes.getNamedItem(Name)
            'If Not (REsultNode Is Nothing) Then
            '    GetXMLAttribute = REsultNode.Value
            '    Found = True
            'End If
            'If Not Found Then
            '    GetXMLAttribute = DefaultIfNotFound
            'End If
            'Exit Function
            'If Not (Node.Attributes Is Nothing) Then
            '    REsultNode = Node.Attributes.getNamedItem(Name)
            '    If (REsultNode Is Nothing) Then
            UcaseName = UCase(Name)
            For Each NodeAttribute In Node.Attributes
                If UCase(NodeAttribute.Name) = UcaseName Then
                    GetXMLAttribute = NodeAttribute.Value
                    Found = True
                    Exit For
                End If
            Next
            If Not Found Then
                GetXMLAttribute = DefaultIfNotFound
            End If
            '    Else
            '        GetXMLAttribute = REsultNode.Value
            '        Found = True
            '    End If
            'End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call HandleClassErrorAndBubble("unknown", "GetXMLAttribute")
            Resume Next
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Function GetXMLAttributeNumber(ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As Double) As Double
            GetXMLAttributeNumber = EncodeNumber(GetXMLAttribute(Found, Node, Name, CStr(DefaultIfNotFound)))
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Function GetXMLAttributeBoolean(ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As Boolean) As Boolean
            GetXMLAttributeBoolean = EncodeBoolean(GetXMLAttribute(Found, Node, Name, CStr(DefaultIfNotFound)))
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Function GetXMLAttributeInteger(ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As Integer) As Integer
            GetXMLAttributeInteger = EncodeInteger(GetXMLAttribute(Found, Node, Name, CStr(DefaultIfNotFound)))
        End Function
        ''
        ''========================================================================
        '' ----- Get an XML nodes attribute based on its name
        ''========================================================================
        ''
        'Private Function GetXMLAttribute(NodeName As XmlNode, Name As String) As String
        '    On Error GoTo ErrorTrap
        '    '
        '    Dim NodeAttribute As xmlattribute
        '    Dim MethodName As String
        '    '
        '    MethodName = "XMLClass.GetXMLAttribute"
        '    '
        '    For Each NodeAttribute In NodeName.Attributes
        '        If UCase(NodeAttribute.Name) = UCase(Name) Then
        '            GetXMLAttribute = NodeAttribute.nodeValue
        '            End If
        '        Next
        '    '
        '    Exit Function
        '    '
        '    ' ----- Error Trap
        '    '
        'ErrorTrap:
        '    Call HandleClassErrorAndBubble(appname,"GetXMLAttribute")
        'End Function
        ''
        ''
        ''
        'Private Function GetContentNameByID(cmc As appServicesClass, ContentID as integer) As String
        '    On Error GoTo ErrorTrap
        '    '
        '    dim dt as datatable
        '    Dim appName As String
        '    '
        '    appName = cpCore.appEnvironment.name
        '    GetContentNameByID = ""
        '    RS = cpCore.app.executeSql("Default", "Select Name from ccContent where ID=" & encodeSQLNumber(ContentID))
        '    If isDataTableOk(RS) Then
        '        GetContentNameByID = cpCore.getDataRowColumnName(RS.rows(0), "Name")
        '        End If
        '    Call closeDataTable(RS)
        '    If (isDataTableOk(rs)) Then
        '        If false Then
        '            RS.Close
        '        End If
        '        'RS = Nothing
        '    End If
        '    '
        '    Exit Function
        '    '
        '    ' ----- Error Trap
        '    '
        'ErrorTrap:
        '    Call HandleClassErrorAndBubble(appName, "GetContentNameByID")
        'End Function
        '
        '========================================================================
        ' ----- Save the admin menus to CDef AdminMenu tags
        '========================================================================
        '
        Private Function GetXMLContentDefinition_SQLIndexes() As String
            On Error GoTo ErrorTrap
            '
            Dim DataSourceName As String
            Dim TableName As String
            '
            Dim IndexFields As String
            Dim IndexList As String
            Dim IndexName As String
            Dim ListRows() As String
            Dim ListRow As String
            Dim ListRowSplit() As String
            Dim SQL As String
            Dim CS As Integer
            Dim Ptr As Integer
            Dim sb As New System.Text.StringBuilder
            Dim appName As String
            '
            appName = cpCore.app.config.name
            SQL = "select D.name as DataSourceName,T.name as TableName" _
                & " from cctables T left join ccDataSources d on D.ID=T.DataSourceID" _
                & " where t.active<>0"
            CS = cpCore.app.db_openCsSql_rev("default", SQL)
            Do While cpCore.app.db_csOk(CS)
                DataSourceName = cpCore.app.db_GetCSText(CS, "DataSourceName")
                TableName = cpCore.app.db_GetCSText(CS, "TableName")
                IndexList = cpCore.app.csv_GetSQLIndexList(DataSourceName, TableName)
                '
                ' name1,index1
                ' name1,index2
                ' name2,field3
                ' name3,field4
                '
                '
                If IndexList <> "" Then
                    ListRows = Split(IndexList, vbCrLf)
                    IndexName = ""
                    For Ptr = 0 To UBound(ListRows) + 1
                        If Ptr <= UBound(ListRows) Then
                            '
                            ' ListRowSplit has the indexname and field for this index
                            '
                            ListRowSplit = Split(ListRows(Ptr), ",")
                        Else
                            '
                            ' one past the last row, ListRowSplit gets a dummy entry to force the output of the last line
                            '
                            ListRowSplit = Split("-,-", ",")
                        End If
                        If UBound(ListRowSplit) > 0 Then
                            If ListRowSplit(0) <> "" Then
                                If IndexName = "" Then
                                    '
                                    ' first line of the first index description
                                    '
                                    IndexName = ListRowSplit(0)
                                    IndexFields = ListRowSplit(1)
                                ElseIf IndexName = ListRowSplit(0) Then
                                    '
                                    ' next line of the index description
                                    '
                                    IndexFields = IndexFields & "," & ListRowSplit(1)
                                Else
                                    '
                                    ' first line of a new index description
                                    ' save previous line
                                    '
                                    If IndexName <> "" And IndexFields <> "" Then
                                        Call sb.Append("<SQLIndex")
                                        Call sb.Append(" Indexname=""" & EncodeXMLattribute(IndexName) & """")
                                        Call sb.Append(" DataSourceName=""" & EncodeXMLattribute(DataSourceName) & """")
                                        Call sb.Append(" TableName=""" & EncodeXMLattribute(TableName) & """")
                                        Call sb.Append(" FieldNameList=""" & EncodeXMLattribute(IndexFields) & """")
                                        Call sb.Append("></SQLIndex>" & vbCrLf)
                                    End If
                                    '
                                    IndexName = ListRowSplit(0)
                                    IndexFields = ListRowSplit(1)
                                End If
                            End If
                        End If
                    Next
                End If
                cpCore.app.db_csGoNext(CS)
            Loop
            Call cpCore.app.db_csClose(CS)
            GetXMLContentDefinition_SQLIndexes = sb.ToString
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call HandleClassErrorAndBubble(appName, "GetXMLContentDefinition_SQLIndexes")
        End Function
        '
        '========================================================================
        ' ----- Save the admin menus to CDef AdminMenu tags
        '========================================================================
        '
        Private Function GetXMLContentDefinition_AdminMenus() As String
            On Error GoTo ErrorTrap
            '
            Dim s As String = ""
            Dim ContentID As Integer
            Dim appName As String
            '
            appName = cpCore.app.config.name
            s = s & GetXMLContentDefinition_AdminMenus_MenuEntries()
            s = s & GetXMLContentDefinition_AdminMenus_NavigatorEntries()
            '
            GetXMLContentDefinition_AdminMenus = s
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call HandleClassErrorAndBubble(appName, "GetXMLContentDefinition_AdminMenus")
        End Function
        '
        '========================================================================
        ' ----- Save the admin menus to CDef AdminMenu tags
        '========================================================================
        '
        Private Function GetXMLContentDefinition_AdminMenus_NavigatorEntries() As String
            On Error GoTo ErrorTrap
            '
            Dim NavIconType As Integer
            Dim NavIconTitle As String
            Dim CSPointer As Integer
            Dim sb As New System.Text.StringBuilder
            Dim dt As DataTable
            Dim ContentID As Integer
            Dim menuNameSpace As String
            Dim RecordName As String
            Dim ParentID As Integer
            Dim MenuContentID As Integer
            Dim SplitArray() As String
            Dim SplitIndex As Integer
            Dim appName As String

            '
            ' ****************************** if cdef not loaded, this fails
            '
            appName = cpCore.app.config.name
            MenuContentID = cpCore.csv_GetRecordID("Content", "Navigator Entries")
            dt = cpCore.app.executeSql("select * from ccMenuEntries where (contentcontrolid=" & MenuContentID & ")and(name<>'')")
            If dt.Rows.Count > 0 Then
                NavIconType = 0
                NavIconTitle = ""
                For Each rsDr As DataRow In dt.Rows
                    RecordName = EncodeText(rsDr("Name"))
                    If RecordName = "Advanced" Then
                        RecordName = RecordName
                    End If
                    ParentID = EncodeInteger(rsDr("ParentID"))
                    menuNameSpace = getMenuNameSpace(ParentID, "")
                    Call sb.Append("<NavigatorEntry Name=""" & EncodeXMLattribute(RecordName) & """")
                    Call sb.Append(" NameSpace=""" & menuNameSpace & """")
                    Call sb.Append(" LinkPage=""" & GetRSXMLAttribute(appName, rsDr, "LinkPage") & """")
                    Call sb.Append(" ContentName=""" & GetRSXMLLookupAttribute(appName, rsDr, "ContentID", "ccContent") & """")
                    Call sb.Append(" AdminOnly=""" & GetRSXMLAttribute(appName, rsDr, "AdminOnly") & """")
                    Call sb.Append(" DeveloperOnly=""" & GetRSXMLAttribute(appName, rsDr, "DeveloperOnly") & """")
                    Call sb.Append(" NewWindow=""" & GetRSXMLAttribute(appName, rsDr, "NewWindow") & """")
                    Call sb.Append(" Active=""" & GetRSXMLAttribute(appName, rsDr, "Active") & """")
                    Call sb.Append(" AddonName=""" & GetRSXMLLookupAttribute(appName, rsDr, "AddonID", "ccAggregateFunctions") & """")
                    Call sb.Append(" SortOrder=""" & GetRSXMLAttribute(appName, rsDr, "SortOrder") & """")
                    NavIconType = EncodeInteger(GetRSXMLAttribute(appName, rsDr, "NavIconType"))
                    NavIconTitle = GetRSXMLAttribute(appName, rsDr, "NavIconTitle")
                    Call sb.Append(" NavIconTitle=""" & NavIconTitle & """")
                    SplitArray = Split(NavIconTypeList & ",help", ",")
                    SplitIndex = NavIconType - 1
                    If (SplitIndex >= 0) And (SplitIndex <= UBound(SplitArray)) Then
                        Call sb.Append(" NavIconType=""" & SplitArray(SplitIndex) & """")
                    Else
                        SplitIndex = SplitIndex
                    End If
                    '
                    If True Then
                        Call sb.Append(" guid=""" & GetRSXMLAttribute(appName, rsDr, "ccGuid") & """")
                    ElseIf True Then
                        Call sb.Append(" guid=""" & GetRSXMLAttribute(appName, rsDr, "NavGuid") & """")
                    End If
                    '
                    Call sb.Append("></NavigatorEntry>" & vbCrLf)
                Next
            End If
            GetXMLContentDefinition_AdminMenus_NavigatorEntries = sb.ToString
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call HandleClassErrorAndBubble(appName, "GetXMLContentDefinition_NavigatorEntries")
        End Function
        '
        '========================================================================
        ' ----- Save the admin menus to CDef AdminMenu tags
        '========================================================================
        '
        Private Function GetXMLContentDefinition_AdminMenus_MenuEntries() As String
            On Error GoTo ErrorTrap
            '
            Dim CSPointer As Integer
            Dim sb As New System.Text.StringBuilder
            Dim rs As DataTable
            Dim ContentID As Integer
            'dim buildversion As String
            Dim menuNameSpace As String
            Dim RecordName As String
            Dim ParentID As Integer
            Dim MenuContentID As Integer
            Dim appName As String
            '
            appName = cpCore.app.config.name
            ' BuildVersion = cpCore.app.getSiteProperty("BuildVersion", "0.0.000", SystemMemberID)
            '
            ' ****************************** if cdef not loaded, this fails
            '
            MenuContentID = cpCore.csv_GetRecordID("Content", "Menu Entries")
            rs = cpCore.app.executeSql("select * from ccMenuEntries where (contentcontrolid=" & MenuContentID & ")and(name<>'')")
            If (isDataTableOk(rs)) Then
                If True Then
                    For Each dr As DataRow In rs.Rows
                        RecordName = EncodeText(dr("Name"))
                        Call sb.Append("<MenuEntry Name=""" & EncodeXMLattribute(RecordName) & """")
                        Call sb.Append(" ParentName=""" & GetRSXMLLookupAttribute(appName, dr, "ParentID", "ccMenuEntries") & """")
                        Call sb.Append(" LinkPage=""" & GetRSXMLAttribute(appName, dr, "LinkPage") & """")
                        Call sb.Append(" ContentName=""" & GetRSXMLLookupAttribute(appName, dr, "ContentID", "ccContent") & """")
                        Call sb.Append(" AdminOnly=""" & GetRSXMLAttribute(appName, dr, "AdminOnly") & """")
                        Call sb.Append(" DeveloperOnly=""" & GetRSXMLAttribute(appName, dr, "DeveloperOnly") & """")
                        Call sb.Append(" NewWindow=""" & GetRSXMLAttribute(appName, dr, "NewWindow") & """")
                        Call sb.Append(" Active=""" & GetRSXMLAttribute(appName, dr, "Active") & """")
                        If True Then
                            Call sb.Append(" AddonName=""" & GetRSXMLLookupAttribute(appName, dr, "AddonID", "ccAggregateFunctions") & """")
                        End If
                        Call sb.Append("/>" & vbCrLf)
                    Next
                End If
            End If
            GetXMLContentDefinition_AdminMenus_MenuEntries = sb.ToString
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call HandleClassErrorAndBubble(appName, "GetXMLContentDefinition_NavigatorEntries")
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Function GetXMLContentDefinition_AggregateFunctions() As String
            On Error GoTo ErrorTrap
            '
            Dim rs As DataTable
            Dim sb As New System.Text.StringBuilder
            Dim appName As String
            '
            appName = cpCore.app.config.name
            rs = cpCore.app.executeSql("select * from ccAggregateFunctions")
            If (isDataTableOk(rs)) Then
                If True Then
                    For Each rsdr As DataRow In rs.Rows
                        Call sb.Append("<Addon Name=""" & GetRSXMLAttribute(appName, rsdr, "Name") & """")
                        Call sb.Append(" Link=""" & GetRSXMLAttribute(appName, rsdr, "Link") & """")
                        Call sb.Append(" ObjectProgramID=""" & GetRSXMLAttribute(appName, rsdr, "ObjectProgramID") & """")
                        Call sb.Append(" ArgumentList=""" & GetRSXMLAttribute(appName, rsdr, "ArgumentList") & """")
                        Call sb.Append(" SortOrder=""" & GetRSXMLAttribute(appName, rsdr, "SortOrder") & """")
                        Call sb.Append(" >")
                        Call sb.Append(GetRSXMLAttribute(appName, rsdr, "Copy"))
                        Call sb.Append("</Addon>" & vbCrLf)
                    Next
                End If
            End If
            GetXMLContentDefinition_AggregateFunctions = sb.ToString
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call HandleClassErrorAndBubble(appName, "GetXMLContentDefinition_AggregateFunctions")
        End Function
        '
        '
        '
        Private Function EncodeXMLattribute(ByVal Source As String) As String
            EncodeXMLattribute = EncodeHTML(Source)
            EncodeXMLattribute = Replace(EncodeXMLattribute, vbCrLf, " ")
            EncodeXMLattribute = Replace(EncodeXMLattribute, vbCr, "")
            EncodeXMLattribute = Replace(EncodeXMLattribute, vbLf, "")
        End Function
        '
        '
        '
        Private Function GetTableRecordName(ByVal TableName As String, ByVal RecordID As Integer) As String
            On Error GoTo ErrorTrap
            '
            Dim dt As DataTable
            Dim appName As String
            '
            appName = cpCore.app.config.name
            If RecordID <> 0 And TableName <> "" Then
                dt = cpCore.app.executeSql("select Name from " & TableName & " where ID=" & RecordID)
                If dt.Rows.Count > 0 Then
                    GetTableRecordName = dt.Rows(0).Item(0).ToString
                End If
            End If
            '
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call HandleClassErrorAndBubble(appName, "GetTableRecordName")
        End Function
        '
        '
        '
        Private Function GetRSXMLAttribute(ByVal appName As String, ByVal dr As DataRow, ByVal FieldName As String) As String
            On Error GoTo ErrorTrap
            '
            GetRSXMLAttribute = EncodeXMLattribute(EncodeText(dr(FieldName)))
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call HandleClassErrorAndBubble(appName, "GetRSXML")
        End Function
        '
        '
        '
        Private Function GetRSXMLLookupAttribute(ByVal appName As String, ByVal dr As DataRow, ByVal FieldName As String, ByVal TableName As String) As String
            On Error GoTo ErrorTrap
            '
            GetRSXMLLookupAttribute = EncodeXMLattribute(GetTableRecordName(TableName, EncodeInteger(dr(FieldName))))
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call HandleClassErrorAndBubble(appName, "GetRSXMLLookupAttribute")
        End Function
        '
        '
        '
        Private Function getMenuNameSpace(ByVal RecordID As Integer, ByVal UsedIDString As String) As String
            On Error GoTo ErrorTrap
            '
            Dim rs As DataTable
            Dim ParentID As Integer
            Dim RecordName As String
            Dim ParentSpace As String
            Dim appName As String
            '
            appName = cpCore.app.config.name
            If RecordID <> 0 Then
                If InStr(1, "," & UsedIDString & ",", "," & RecordID & ",", vbTextCompare) <> 0 Then
                    Call HandleClassErrorAndResume(appName, "getMenuNameSpace", "Circular reference found in UsedIDString [" & UsedIDString & "] getting ccMenuEntries namespace for recordid [" & RecordID & "]")
                    getMenuNameSpace = ""
                Else
                    UsedIDString = UsedIDString & "," & RecordID
                    ParentID = 0
                    If RecordID <> 0 Then
                        rs = cpCore.app.executeSql("select Name,ParentID from ccMenuEntries where ID=" & RecordID)
                        If (isDataTableOk(rs)) Then
                            ParentID = EncodeInteger(rs.Rows(0).Item("ParentID"))
                            RecordName = EncodeText(rs.Rows(0).Item("Name"))
                        End If
                        If (isDataTableOk(rs)) Then
                            If False Then
                                'RS.Close()
                            End If
                            'RS = Nothing
                        End If
                    End If
                    If RecordName <> "" Then
                        If ParentID = RecordID Then
                            '
                            ' circular reference
                            '
                            Call HandleClassErrorAndResume(appName, "getMenuNameSpace", "Circular reference found (ParentID=RecordID) getting ccMenuEntries namespace for recordid [" & RecordID & "]")
                            getMenuNameSpace = ""
                        Else
                            If ParentID <> 0 Then
                                '
                                ' get next parent
                                '
                                ParentSpace = getMenuNameSpace(ParentID, UsedIDString)
                            End If
                            If ParentSpace <> "" Then
                                getMenuNameSpace = ParentSpace & "." & RecordName
                            Else
                                getMenuNameSpace = RecordName
                            End If
                        End If
                    Else
                        getMenuNameSpace = ""
                    End If
                End If
            End If
            Exit Function
            '
ErrorTrap:
            Call HandleClassErrorAndBubble(appName, "getMenuNameSpace")
        End Function
        '
        '
        '
        Private Function CacheLookup(ByVal RecordID As Integer, ByVal Cache As Object(,)) As String
            '
            Dim Ptr As Integer
            '
            CacheLookup = ""
            If RecordID <> 0 Then
                For Ptr = 0 To UBound(Cache, 2)
                    If EncodeInteger(Cache(0, Ptr)) = RecordID Then
                        CacheLookup = EncodeText(Cache(1, Ptr))
                        Exit For
                    End If
                Next
            End If
        End Function
        '
        '
        '
        Private Function xaT(ByVal Source As Object) As String
            xaT = EncodeXMLattribute(EncodeText(Source))
        End Function
        '
        '
        '
        Private Function xaB(ByVal Source As Object) As String
            xaB = CStr(EncodeBoolean(EncodeText(Source)))
        End Function
        '
        '===========================================================================
        '   Error handler
        '===========================================================================
        '
        Private Sub HandleClassErrorAndBubble(ByVal appName As String, ByVal MethodName As String, Optional ByVal Cause As String = "unknown")
            '
            cpCore.handleLegacyError3(appName, Cause, "dll", "XMLToolsClass", MethodName, Err.Number, Err.Source, Err.Description, False, False, "")
            '
        End Sub
        '
        '===========================================================================
        '   Error handler
        '===========================================================================
        '
        Private Sub HandleClassErrorAndResume(ByVal appName As String, ByVal MethodName As String, ByVal Cause As String)
            '
            cpCore.handleLegacyError3(appName, Cause, "dll", "XMLToolsClass", MethodName, Err.Number, Err.Source, Err.Description, False, True, "")
            '
        End Sub



    End Class
End Namespace