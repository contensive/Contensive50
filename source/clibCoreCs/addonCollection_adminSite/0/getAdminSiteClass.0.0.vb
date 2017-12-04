
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
