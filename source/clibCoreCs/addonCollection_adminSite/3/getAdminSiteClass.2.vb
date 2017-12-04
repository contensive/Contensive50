
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
        '
        '========================================================================
        '   Print the root form
        '========================================================================
        '
        Private Function GetForm_Root() As String
            Dim returnHtml As String = ""
            Try
                Dim CS As Integer
                Dim Stream As New stringBuilderLegacyController
                Dim addonId As Integer
                Dim AddonIDText As String
                '
                ' This is really messy -- there must be a better way
                '
                addonId = 0
                If (cpCore.doc.authContext.visit.id = cpCore.docProperties.getInteger(RequestNameDashboardReset)) Then
                    '$$$$$ cache this
                    CS = cpCore.db.csOpen(cnAddons, "ccguid=" & cpCore.db.encodeSQLText(addonGuidDashboard))
                    If cpCore.db.csOk(CS) Then
                        addonId = cpCore.db.csGetInteger(CS, "id")
                        Call cpCore.siteProperties.setProperty("AdminRootAddonID", genericController.encodeText(addonId))
                    End If
                    Call cpCore.db.csClose(CS)
                End If
                If addonId = 0 Then
                    '
                    ' Get AdminRootAddon
                    '
                    AddonIDText = cpCore.siteProperties.getText("AdminRootAddonID", "")
                    If AddonIDText = "" Then
                        '
                        ' the desktop is likely unset, auto set it to dashboard
                        '
                        addonId = -1
                    ElseIf AddonIDText = "0" Then
                        '
                        ' the desktop has been set to none - go with default desktop
                        '
                        addonId = 0
                    ElseIf genericController.vbIsNumeric(AddonIDText) Then
                        '
                        ' it has been set to a non-zero number
                        '
                        addonId = genericController.EncodeInteger(AddonIDText)
                        '
                        ' Verify it so there is no error when it runs
                        '
                        CS = cpCore.db.csOpenRecord(cnAddons, addonId)
                        If Not cpCore.db.csOk(CS) Then
                            '
                            ' it was set, but the add-on is not available, auto set to dashboard
                            '
                            addonId = -1
                            Call cpCore.siteProperties.setProperty("AdminRootAddonID", "")
                        End If
                        Call cpCore.db.csClose(CS)
                    End If
                    If addonId = -1 Then
                        '
                        ' This has never been set, try to get the dashboard ID
                        '
                        '$$$$$ cache this
                        CS = cpCore.db.csOpen(cnAddons, "ccguid=" & cpCore.db.encodeSQLText(addonGuidDashboard))
                        If cpCore.db.csOk(CS) Then
                            addonId = cpCore.db.csGetInteger(CS, "id")
                            Call cpCore.siteProperties.setProperty("AdminRootAddonID", genericController.encodeText(addonId))
                        End If
                        Call cpCore.db.csClose(CS)
                    End If
                End If
                If addonId <> 0 Then
                    '
                    ' Display the Addon
                    '
                    If (cpCore.doc.debug_iUserError <> "") Then
                        returnHtml = returnHtml _
                        & "<div style=""clear:both;margin-top:20px;"">&nbsp;</div>" _
                        & "<div style=""clear:both;margin-top:20px;"">" & errorController.error_GetUserError(cpCore) & "</div>"
                    End If
                    returnHtml &= cpCore.addon.execute(addonModel.create(cpCore, addonId), New BaseClasses.CPUtilsBaseClass.addonExecuteContext() With {.addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, .errorCaption = "id:" & addonId})
                    'returnHtml = returnHtml & cpCore.addon.execute_legacy4(CStr(addonId), "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin)
                End If
                If returnHtml = "" Then
                    '
                    ' Nothing Displayed, show default root page
                    '
                    returnHtml = returnHtml _
                    & vbCrLf & "<div style=""padding:20px;height:450px"">" _
                    & vbCrLf & "<div><a href=http://www.Contensive.com target=_blank><img style=""border:1px solid #000;"" src=""/ccLib/images/ContensiveAdminLogo.GIF"" border=0 ></A></div>" _
                    & vbCrLf & "<div><strong>Contensive/" & cpCore.codeVersion & "</strong></div>" _
                    & vbCrLf & "<div style=""clear:both;height:18px;margin-top:10px""><div style=""float:left;width:200px;"">Domain Name</div><div style=""float:left;"">" & cpCore.webServer.requestDomain & "</div></div>" _
                    & vbCrLf & "<div style=""clear:both;height:18px;""><div style=""float:left;width:200px;"">Login Member Name</div><div style=""float:left;"">" & cpCore.doc.authContext.user.name & "</div></div>" _
                    & vbCrLf & "<div style=""clear:both;height:18px;""><div style=""float:left;width:200px;"">Quick Reports</div><div style=""float:left;""><a Href=""?" & RequestNameAdminForm & "=" & AdminFormQuickStats & """>Real-Time Activity</A></div></div>" _
                    & vbCrLf & "<div style=""clear:both;height:18px;""><div style=""float:left;width:200px;""><a Href=""?" & RequestNameDashboardReset & "=" & cpCore.doc.authContext.visit.id & """>Run Dashboard</A></div></div>" _
                    & vbCrLf & "<div style=""clear:both;height:18px;""><div style=""float:left;width:200px;""><a Href=""?addonguid=" & addonGuidAddonManager & """>Add-on Manager</A></div></div>"
                    '
                    If (cpCore.doc.debug_iUserError <> "") Then
                        returnHtml = returnHtml _
                        & "<div style=""clear:both;margin-top:20px;"">&nbsp;</div>" _
                        & "<div style=""clear:both;margin-top:20px;"">" & errorController.error_GetUserError(cpCore) & "</div>"
                    End If
                    '
                    returnHtml = returnHtml _
                    & vbCrLf & "</div>" _
                    & ""
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnHtml
        End Function
        '
        '========================================================================
        '   Print the root form
        '========================================================================
        '
        Private Function GetForm_QuickStats() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_QuickStats")
            '
            Dim SQL As String
            Dim CS As Integer
            Dim RowColor As String
            Dim Panel As String
            Dim VisitID As Integer
            Dim VisitCount As Integer
            Dim PageCount As Double
            Dim Stream As New stringBuilderLegacyController
            '
            ' --- Start a form to make a refresh button
            '
            Call Stream.Add(cpCore.html.html_GetFormStart)
            Call Stream.Add(cpCore.html.main_GetPanelButtons(ButtonCancel & "," & ButtonRefresh, "" & RequestNameButton & ""))
            Call Stream.Add("<input TYPE=""hidden"" NAME=""asf"" VALUE=""" & AdminFormQuickStats & """>")
            Call Stream.Add(cpCore.html.main_GetPanel(" "))
            '
            ' --- Indented part (Title Area plus page)
            '
            Stream.Add("<table border=""0"" cellpadding=""20"" cellspacing=""0"" width=""100%""><tr><td>" & SpanClassAdminNormal)
            Stream.Add("<h1>Real-Time Activity Report</h1>")
            '
            ' --- set column width
            '
            Stream.Add("<h2>Visits Today</h2>")
            Stream.Add("<table border=""0"" cellpadding=""3"" cellspacing=""0"" width=""100%"" style=""background-color:white;border-top:1px solid #888;"">")
            'Stream.Add( "<tr"">")
            'Stream.Add( "<td width=""150""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""140"" height=""1"" ></td>")
            'Stream.Add( "<td width=""150""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""140"" height=""1"" ></td>")
            'Stream.Add( "<td width=""100%""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""1"" ></td>")
            'Stream.Add( "</tr>")
            '
            ' ----- All Visits Today
            '
            SQL = "SELECT Count(ccVisits.ID) AS VisitCount, Avg(ccVisits.PageVisits) AS PageCount FROM ccVisits WHERE ((ccVisits.StartTime)>" & cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime.Date) & ");"
            CS = cpCore.db.csOpenSql(SQL)
            If cpCore.db.csOk(CS) Then
                VisitCount = cpCore.db.csGetInteger(CS, "VisitCount")
                PageCount = cpCore.db.csGetNumber(CS, "pageCount")
                Stream.Add("<tr>")
                Stream.Add("<td style=""border-bottom:1px solid #888;"" valign=top>" & SpanClassAdminNormal & "All Visits</span></td>")
                Stream.Add("<td style=""width:150px;border-bottom:1px solid #888;"" valign=top>" & SpanClassAdminNormal & "<a target=""_blank"" href=""/" & genericController.encodeHTML(cpCore.serverConfig.appConfig.adminRoute & "?" & RequestNameAdminForm & "=" & AdminFormReports & "&rid=3&DateFrom=" & cpCore.doc.profileStartTime & "&DateTo=" & cpCore.doc.profileStartTime.ToShortDateString) & """>" & VisitCount & "</A>, " & FormatNumber(PageCount, 2) & " pages/visit.</span></td>")
                Stream.Add("<td style=""border-bottom:1px solid #888;"" valign=top>" & SpanClassAdminNormal & "This includes all visitors to the website, including guests, bots and administrators. Pages/visit includes page hits and not ajax or remote method hits.</span></td>")
                Stream.Add("</tr>")
            End If
            Call cpCore.db.csClose(CS)
            '
            ' ----- Non-Bot Visits Today
            '
            SQL = "SELECT Count(ccVisits.ID) AS VisitCount, Avg(ccVisits.PageVisits) AS PageCount FROM ccVisits WHERE (ccVisits.CookieSupport=1)and((ccVisits.StartTime)>" & cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime.Date) & ");"
            CS = cpCore.db.csOpenSql(SQL)
            If cpCore.db.csOk(CS) Then
                VisitCount = cpCore.db.csGetInteger(CS, "VisitCount")
                PageCount = cpCore.db.csGetNumber(CS, "pageCount")
                Stream.Add("<tr>")
                Stream.Add("<td style=""border-bottom:1px solid #888;"" valign=top>" & SpanClassAdminNormal & "Non-bot Visits</span></td>")
                Stream.Add("<td style=""border-bottom:1px solid #888;"" valign=top>" & SpanClassAdminNormal & "<a target=""_blank"" href=""/" & genericController.encodeHTML(cpCore.serverConfig.appConfig.adminRoute & "?" & RequestNameAdminForm & "=" & AdminFormReports & "&rid=3&DateFrom=" & cpCore.doc.profileStartTime.ToShortDateString & "&DateTo=" & cpCore.doc.profileStartTime.ToShortDateString) & """>" & VisitCount & "</A>, " & FormatNumber(PageCount, 2) & " pages/visit.</span></td>")
                Stream.Add("<td style=""border-bottom:1px solid #888;"" valign=top>" & SpanClassAdminNormal & "This excludes hits from visitors identified as bots. Pages/visit includes page hits and not ajax or remote method hits.</span></td>")
                Stream.Add("</tr>")
            End If
            Call cpCore.db.csClose(CS)
            '
            ' ----- Visits Today by new visitors
            '
            SQL = "SELECT Count(ccVisits.ID) AS VisitCount, Avg(ccVisits.PageVisits) AS PageCount FROM ccVisits WHERE (ccVisits.CookieSupport=1)and(ccVisits.StartTime>" & cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime.Date) & ")AND(ccVisits.VisitorNew<>0);"
            CS = cpCore.db.csOpenSql(SQL)
            If cpCore.db.csOk(CS) Then
                VisitCount = cpCore.db.csGetInteger(CS, "VisitCount")
                PageCount = cpCore.db.csGetNumber(CS, "pageCount")
                Stream.Add("<tr>")
                Stream.Add("<td style=""border-bottom:1px solid #888;"" valign=top>" & SpanClassAdminNormal & "Visits by New Visitors</span></td>")
                Stream.Add("<td style=""border-bottom:1px solid #888;"" valign=top>" & SpanClassAdminNormal & "<a target=""_blank"" href=""/" & genericController.encodeHTML(cpCore.serverConfig.appConfig.adminRoute & "?" & RequestNameAdminForm & "=" & AdminFormReports & "&rid=3&ExcludeOldVisitors=1&DateFrom=" & cpCore.doc.profileStartTime.ToShortDateString & "&DateTo=" & cpCore.doc.profileStartTime.ToShortDateString) & """>" & VisitCount & "</A>, " & FormatNumber(PageCount, 2) & " pages/visit.</span></td>")
                Stream.Add("<td style=""border-bottom:1px solid #888;"" valign=top>" & SpanClassAdminNormal & "This includes only new visitors not identified as bots. Pages/visit includes page hits and not ajax or remote method hits.</span></td>")
                Stream.Add("</tr>")
            End If
            Call cpCore.db.csClose(CS)
            '
            Call Stream.Add("</table>")
            '
            ' ----- Visits currently online
            '
            If True Then
                Panel = ""
                Stream.Add("<h2>Current Visits</h2>")
                SQL = "SELECT ccVisits.HTTP_REFERER as referer,ccVisits.remote_addr as Remote_Addr, ccVisits.LastVisitTime as LastVisitTime, ccVisits.PageVisits as PageVisits, ccMembers.Name as MemberName, ccVisits.ID as VisitID, ccMembers.ID as MemberID" _
                    & " FROM ccVisits LEFT JOIN ccMembers ON ccVisits.MemberID = ccMembers.ID" _
                    & " WHERE (((ccVisits.LastVisitTime)>" & cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime.AddHours(-1)) & "))" _
                    & " ORDER BY ccVisits.LastVisitTime DESC;"
                CS = cpCore.db.csOpenSql(SQL)
                If cpCore.db.csOk(CS) Then
                    Panel = Panel & "<table width=""100%"" border=""0"" cellspacing=""1"" cellpadding=""2"">"
                    Panel = Panel & "<tr bgcolor=""#B0B0B0"">"
                    Panel = Panel & "<td width=""20%"" align=""left"">" & SpanClassAdminNormal & "User</td>"
                    Panel = Panel & "<td width=""20%"" align=""left"">" & SpanClassAdminNormal & "IP&nbsp;Address</td>"
                    Panel = Panel & "<td width=""20%"" align=""left"">" & SpanClassAdminNormal & "Last&nbsp;Page&nbsp;Hit</td>"
                    Panel = Panel & "<td width=""10%"" align=""right"">" & SpanClassAdminNormal & "Page&nbsp;Hits</td>"
                    Panel = Panel & "<td width=""10%"" align=""right"">" & SpanClassAdminNormal & "Visit</td>"
                    Panel = Panel & "<td width=""30%"" align=""left"">" & SpanClassAdminNormal & "Referer</td>"
                    Panel = Panel & "</tr>"
                    RowColor = "ccPanelRowEven"
                    Do While cpCore.db.csOk(CS)
                        VisitID = cpCore.db.csGetInteger(CS, "VisitID")
                        Panel = Panel & "<tr class=""" & RowColor & """>"
                        Panel = Panel & "<td align=""left"">" & SpanClassAdminNormal & "<a target=""_blank"" href=""/" & genericController.encodeHTML(cpCore.serverConfig.appConfig.adminRoute & "?" & RequestNameAdminForm & "=" & AdminFormReports & "&rid=16&MemberID=" & cpCore.db.csGetInteger(CS, "MemberID")) & """>" & cpCore.db.csGet(CS, "MemberName") & "</A></span></td>"
                        Panel = Panel & "<td align=""left"">" & SpanClassAdminNormal & cpCore.db.csGet(CS, "Remote_Addr") & "</span></td>"
                        Panel = Panel & "<td align=""left"">" & SpanClassAdminNormal & FormatDateTime(cpCore.db.csGetDate(CS, "LastVisitTime"), vbLongTime) & "</span></td>"
                        Panel = Panel & "<td align=""right"">" & SpanClassAdminNormal & "<a target=""_blank"" href=""/" & cpCore.serverConfig.appConfig.adminRoute & "?" & RequestNameAdminForm & "=" & AdminFormReports & "&rid=10&VisitID=" & VisitID & """>" & cpCore.db.csGet(CS, "PageVisits") & "</A></span></td>"
                        Panel = Panel & "<td align=""right"">" & SpanClassAdminNormal & "<a target=""_blank"" href=""/" & cpCore.serverConfig.appConfig.adminRoute & "?" & RequestNameAdminForm & "=" & AdminFormReports & "&rid=17&VisitID=" & VisitID & """>" & VisitID & "</A></span></td>"
                        Panel = Panel & "<td align=""left"">" & SpanClassAdminNormal & "&nbsp;" & cpCore.db.csGetText(CS, "referer") & "</span></td>"
                        Panel = Panel & "</tr>"
                        If RowColor = "ccPanelRowEven" Then
                            RowColor = "ccPanelRowOdd"
                        Else
                            RowColor = "ccPanelRowEven"
                        End If
                        Call cpCore.db.csGoNext(CS)
                    Loop
                    Panel = Panel & "</table>"
                End If
                Call cpCore.db.csClose(CS)
                Stream.Add(cpCore.html.main_GetPanel(Panel, "ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 0))
            End If
            Call Stream.Add("</td></tr></table>")
            Call Stream.Add(cpCore.html.html_GetFormEnd)
            '
            GetForm_QuickStats = Stream.Text
            Call cpCore.html.addTitle("Quick Stats")
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_QuickStats")
            '
        End Function
        ''
        ''========================================================================
        ''   Print the Topic Rules section of any edit form
        ''========================================================================
        ''
        'Private Function GetForm_Edit_TopicRules() As String
        '    On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.GetForm_Edit_TopicRules")
        '    '
        '    Dim SQL As String
        '    Dim CS as integer
        '    Dim MembershipCount as integer
        '    Dim MembershipSize as integer
        '    Dim MembershipPointer as integer
        '    Dim SectionName As String
        '    Dim TopicCount as integer
        '    Dim Membership() as integer
        '    Dim f As New fastStringClass
        '    Dim Checked As Boolean
        '    Dim TableID as integer
        '    Dim Adminui As New adminUIclass(cpcore)
        '    '
        '    If AdminContent.AllowTopicRules Then
        '        '
        '        ' ----- can not use common call
        '        '       problem, TopicRules has 2 primary content keys (ContentID and RecordID)
        '        '       if we changed it to only use ContentRecordKey, we could use that as the only primary key.
        '        '
        '        ' ----- Gather all the topics to which this member belongs
        '        '
        '        MembershipCount = 0
        '        MembershipSize = 0
        '        If EditRecord.ID <> 0 Then
        '            SQL = "SELECT ccTopicRules.TopicID AS TopicID FROM (ccContent LEFT JOIN ccTopicRules ON ccContent.ID = ccTopicRules.ContentID) LEFT JOIN ccTables ON ccContent.ContentTableID = ccTables.ID WHERE (((ccTables.Name)=" & encodeSQLText(AdminContent.ContentTableName) & ") AND ((ccTopicRules.RecordID)=" & EditRecord.ID & ") AND ((ccContent.Active)<>0) AND ((ccTopicRules.Active)<>0));"
        '
        '            'SQL = "SELECT ccTopicRules.TopicID as ID" _
        '             '   & " FROM ccContent LEFT JOIN ccTopicRules ON ccContent.ID = ccTopicRules.ContentID" _
        '              '  & " WHERE (((ccContent.ContentTablename)=" & encodeSQLText(AdminContent.ContentTableName) & ") AND ((ccTopicRules.RecordID)=" & EditRecord.ID & ") AND ((ccContent.Active)<>0) AND ((ccTopicRules.Active)<>0))"
        '            CS = cpCore.app_openCsSql_Rev_Internal("Default", SQL)
        '            If cpCore.app.csv_IsCSOK(CS) Then
        '                If True Then
        '                    MembershipSize = 10
        '                    ReDim Membership(MembershipSize)
        '                    Do While cpCore.app.csv_IsCSOK(CS)
        '                        If MembershipCount >= MembershipSize Then
        '                            MembershipSize = MembershipSize + 10
        '                            ReDim Preserve Membership(MembershipSize)
        '                            End If
        '                        Membership(MembershipCount) = cpCore.app.cs_getInteger(CS, "TopicID")
        '                        MembershipCount = MembershipCount + 1
        '                        Call cpCore.app.nextCSRecord(CS)
        '                        Loop
        '                    End If
        '                End If
        '            cpCore.main_CloseCS (CS)
        '            End If
        '        '
        '        ' ----- Gather all the topics, sorted by ContentName (no topics, skip section)
        '        '
        '        SQL = "SELECT ccTopics.ID AS ID, ccContent.Name AS SectionName, ccTopics.Name AS TopicName, ccTopics.SortOrder" _
        '            & " FROM ccTopics LEFT JOIN ccContent ON ccTopics.ContentControlID = ccContent.ID" _
        '            & " Where (((ccTopics.Active) <> " & SQLFalse & ") And ((ccContent.Active) <> " & SQLFalse & "))" _
        '            & " GROUP BY ccTopics.ID, ccContent.Name, ccTopics.Name, ccTopics.SortOrder" _
        '            & " ORDER BY ccContent.Name, ccTopics.SortOrder"
        '        CS = cpCore.app_openCsSql_Rev_Internal("Default", SQL)
        '        If cpCore.app.csv_IsCSOK(CS) Then
        '            If True Then
        '                '
        '                ' ----- Open the panel
        '                '
        '                Call f.Add(AdminUI.EditTableOpen)
        '                SectionName = ""
        '                TopicCount = 0
        '                Do While cpCore.app.csv_IsCSOK(CS)
        '                    f.Add( "<tr>"
        '                    If SectionName <> cpCore.app.cs_get(CS, "SectionName") Then
        '                        '
        '                        ' ----- create the next content Topic row
        '                        '
        '                        SectionName = cpCore.app.cs_get(CS, "SectionName")
        '                        Call f.Add("<td class=""ccAdminEditCaption"">" & SectionName & "</td>")
        '                    Else
        '                        Call f.Add("<td class=""ccAdminEditCaption"">&nbsp;</td>")
        '                    End If
        '                    Call f.Add("<td class=""ccAdminEditField"">")
        '                    Checked = False
        '                    If MembershipCount <> 0 Then
        '                        For MembershipPointer = 0 To MembershipCount - 1
        '                            If Membership(MembershipPointer) = cpCore.app.cs_getInteger(CS, "ID") Then
        '                                Checked = True
        '                                Exit For
        '                            End If
        '                        Next
        '                    End If
        '                    If editrecord.read_only And Not Checked Then
        '                        f.Add( "<input type=""checkbox"" disabled>"
        '                    ElseIf editrecord.read_only Then
        '                        f.Add( "<input type=""checkbox"" disabled checked>"
        '                        f.Add( "<input type=hidden name=""Topic" & TopicCount & """ value=1>"
        '                    ElseIf Checked Then
        '                        f.Add( "<input type=""checkbox"" name=""Topic" & TopicCount & """ checked>"
        '                    Else
        '                        f.Add( "<input type=""checkbox"" name=""Topic" & TopicCount & """>"
        '                    End If
        '                    f.Add( "<input type=""hidden"" name=""TopicID" & TopicCount & """ value=""" & cpCore.app.cs_get(CS, "ID") & """>"
        '                    f.Add( SpanClassAdminNormal & cpCore.app.cs_get(CS, "TopicName") & "</span></td>"
        '                    f.Add( "</tr>"
        '                    '
        '                    TopicCount = TopicCount + 1
        '                    Call cpCore.app.nextCSRecord(CS)
        '                Loop
        '                f.Add( vbCrLf & "<input type=""hidden"" name=""TopicCount"" value=""" & TopicCount & """>"
        '                f.Add( AdminUI.EditTableClose
        '                '
        '                ' ----- close the panel
        '                '
        '                GetForm_Edit_TopicRules = AdminUI.GetEditPanel( (Not AllowAdminTabs), "Topic Rules", "This content is associated with the following topics", f.Text)
        '                EditSectionPanelCount = EditSectionPanelCount + 1
        '                '
        '                End If
        '            End If
        '        Call cpCore.app.closeCS(CS)
        '    End If
        '    '''Dim th as integer: Exit Function
        '    '
        'ErrorTrap:
        '    Call HandleClassTrapErrorBubble("GetForm_Edit_TopicRules")
        'End Function
        '
        '========================================================================
        '   Print the Topic Rules section of any edit form
        '========================================================================
        '
        Private Function GetForm_Edit_LinkAliases(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, ByVal readOnlyField As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_LinkAliases")
            '
            Dim LinkCnt As Integer
            Dim LinkList As String = ""
            Dim f As New stringBuilderLegacyController
            Dim Adminui As New adminUIController(cpCore)
            Dim Ptr As Integer
            Dim linkAlias As String
            Dim AllowLinkAliasInTab As Boolean
            Dim Link As String
            Dim CS As Integer
            Dim tabContent As String
            Dim TabDescription As String
            '
            '
            ' Link Alias value from the admin data
            '
            TabDescription = "Link Aliases are URLs used for this content that are more friendly to users and search engines. If you set the Link Alias field, this name will be used on the URL for this page. If you leave the Link Alias blank, the page name will be used. Below is a list of names that have been used previously and are still active. All of these entries when used in the URL will resolve to this page. The first entry in this list will be used to create menus on the site. To move an entry to the top, type it into the Link Alias field and save."
            If Not cpCore.siteProperties.allowLinkAlias Then
                '
                ' Disabled
                '
                tabContent = "&nbsp;"
                TabDescription = "<p>The Link Alias feature is currently disabled. To enable Link Aliases, check the box marked 'Allow Link Alias' on the Page Settings page found on the Navigator under 'Settings'.</p><p>" & TabDescription & "</p>"
            Else
                '
                ' Link Alias Field
                '
                linkAlias = ""
                If adminContent.fields.ContainsKey("linkalias") Then
                    linkAlias = genericController.encodeText(editRecord.fieldsLc.Item("linkalias").value)
                End If
                Call f.Add("<tr><td class=""ccAdminEditCaption"">" & SpanClassAdminSmall & "Link Alias</td>")
                Call f.Add("<td class=""ccAdminEditField"" align=""left"" colspan=""2"">" & SpanClassAdminNormal)
                If readOnlyField Then
                    Call f.Add(linkAlias)
                Else
                    Call f.Add(cpCore.html.html_GetFormInputText2("LinkAlias", linkAlias))
                End If
                Call f.Add("</span></td></tr>")
                '
                ' Override Duplicates
                '
                Call f.Add("<tr><td class=""ccAdminEditCaption"">" & SpanClassAdminSmall & "Override Duplicates</td>")
                Call f.Add("<td class=""ccAdminEditField"" align=""left"" colspan=""2"">" & SpanClassAdminNormal)
                If readOnlyField Then
                    Call f.Add("No")
                Else
                    Call f.Add(cpCore.html.html_GetFormInputCheckBox2("OverrideDuplicate", False))
                End If
                Call f.Add("</span></td></tr>")
                '
                ' Table of old Link Aliases
                '
                Link = cpCore.doc.main_GetPageDynamicLink(editRecord.id, False)
                CS = cpCore.db.csOpen("Link Aliases", "pageid=" & editRecord.id, "ID Desc", , , , , "name")
                Do While cpCore.db.csOk(CS)
                    LinkList = LinkList & "<div style=""margin-left:4px;margin-bottom:4px;"">" & genericController.encodeHTML(cpCore.db.csGetText(CS, "name")) & "</div>"
                    LinkCnt = LinkCnt + 1
                    Call cpCore.db.csGoNext(CS)
                Loop
                Call cpCore.db.csClose(CS)
                If LinkCnt > 0 Then
                    Call f.Add("<tr><td class=""ccAdminEditCaption"">" & SpanClassAdminSmall & "Previous Link Alias List</td>")
                    Call f.Add("<td class=""ccAdminEditField"" align=""left"" colspan=""2"">" & SpanClassAdminNormal)
                    Call f.Add(LinkList)
                    Call f.Add("</span></td></tr>")
                End If
                tabContent = Adminui.EditTableOpen & f.Text & Adminui.EditTableClose
            End If
            '
            GetForm_Edit_LinkAliases = Adminui.GetEditPanel((Not allowAdminTabs), "Link Aliases", TabDescription, tabContent)
            EditSectionPanelCount = EditSectionPanelCount + 1
            '
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Edit_LinkAliases")
        End Function
        '        '
        '        '========================================================================
        '        '   Print the Topic Rules section of any edit form
        '        '========================================================================
        '        '
        '        Private Function GetForm_Edit_MetaContent(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, ByVal readOnlyField As Boolean) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_MetaContent")
        '            '
        '            Dim s As String
        '            Dim SQL As String
        '            Dim FastString As New stringBuilderLegacyController
        '            Dim Checked As Boolean
        '            Dim TableID As Integer
        '            Dim MetaContentID As Integer
        '            Dim CS As Integer
        '            Dim PageTitle As String = ""
        '            Dim MetaDescription As String = ""
        '            Dim MetaKeywordList As String = ""
        '            Dim OtherHeadTags As String = ""
        '            Dim Adminui As New adminUIController(cpCore)
        '            '
        '            If adminContent.AllowMetaContent Then
        '                CS = cpCore.db.cs_open("Meta Content", "(ContentID=" & editRecord.contentControlId & ")and(RecordID=" & editRecord.id & ")")
        '                If Not cpCore.db.cs_ok(CS) Then
        '                    CS = cpCore.db.cs_insertRecord("Meta Content")
        '                    Call cpCore.db.cs_set(CS, "ContentID", editRecord.contentControlId)
        '                    Call cpCore.db.cs_set(CS, "RecordID", editRecord.id)
        '                    Call cpCore.db.cs_save2(CS)
        '                End If
        '                If cpCore.db.cs_ok(CS) Then
        '                    MetaContentID = cpCore.db.cs_getInteger(CS, "ID")
        '                    PageTitle = cpCore.db.cs_get(CS, "Name")
        '                    MetaDescription = cpCore.db.cs_get(CS, "MetaDescription")
        '                    If True Then ' 3.3.930" Then
        '                        MetaKeywordList = cpCore.db.cs_get(CS, "MetaKeywordList")
        '                        OtherHeadTags = cpCore.db.cs_get(CS, "OtherHeadTags")
        '                    ElseIf cpCore.db.cs_isFieldSupported(CS, "OtherHeadTags") Then
        '                        OtherHeadTags = cpCore.db.cs_get(CS, "OtherHeadTags")
        '                    End If
        '                End If
        '                Call cpCore.db.cs_Close(CS)
        '                '
        '                'Call FastString.Add(cpCore.main_GetFormInputHidden("MetaContent.MetaContentID", MetaContentID))
        '                '
        '                ' Page Title
        '                '
        '                Call FastString.Add("<tr><td class=""ccAdminEditCaption"">" & SpanClassAdminSmall & "Page Title</td>")
        '                Call FastString.Add("<td class=""ccAdminEditField"" align=""left"" colspan=""2"">" & SpanClassAdminNormal)
        '                If readOnlyField Then
        '                    Call FastString.Add(PageTitle)
        '                Else
        '                    Call FastString.Add(cpCore.html.html_GetFormInputText2("MetaContent.PageTitle", PageTitle))
        '                End If
        '                Call FastString.Add("</span></td></tr>")
        '                '
        '                ' Meta Description
        '                '
        '                Call FastString.Add("<tr><td class=""ccAdminEditCaption"">" & SpanClassAdminSmall & "Meta Description</td>")
        '                Call FastString.Add("<td class=""ccAdminEditField"" align=""left"" colspan=""2"">" & SpanClassAdminNormal)
        '                If readOnlyField Then
        '                    Call FastString.Add(MetaDescription)
        '                Else
        '                    Call FastString.Add(cpCore.html.html_GetFormInputTextExpandable("MetaContent.MetaDescription", MetaDescription, 10))
        '                End If
        '                Call FastString.Add("</span></td></tr>")
        '                '
        '                ' Meta Keyword List
        '                '
        '                Call FastString.Add("<tr><td class=""ccAdminEditCaption"">" & SpanClassAdminSmall & "Meta Keyword List</td>")
        '                Call FastString.Add("<td class=""ccAdminEditField"" align=""left"" colspan=""2"">" & SpanClassAdminNormal)
        '                If readOnlyField Then
        '                    Call FastString.Add(MetaKeywordList)
        '                Else
        '                    Call FastString.Add(cpCore.html.html_GetFormInputTextExpandable("MetaContent.MetaKeywordList", MetaKeywordList, 10))
        '                End If
        '                Call FastString.Add("</span></td></tr>")
        '                '
        '                ' Meta Keywords, Shared
        '                '
        '                Call FastString.Add("<tr><td class=""ccAdminEditCaption"">" & SpanClassAdminSmall & "Shared Meta Keywords</td>")
        '                Call FastString.Add("<td class=""ccAdminEditField"" colspan=""2"">")
        '                Call FastString.Add(cpCore.html.getInputCheckList("MetaContent.KeywordList", "Meta Content", MetaContentID, "Meta Keywords", "Meta Keyword Rules", "MetaContentID", "MetaKeywordID", , "Name", readOnlyField))
        '                'Call FastString.Add(cpCore.html.getInputCheckListCategories("MetaContent.KeywordList", "Meta Content", MetaContentID, "Meta Keywords", "Meta Keyword Rules", "MetaContentID", "MetaKeywordID", , "Name", readOnlyField, "Meta Keywords"))
        '                Call FastString.Add("</td></tr>")
        '                '
        '                ' Other Head Tags
        '                '
        '                Call FastString.Add("<tr><td class=""ccAdminEditCaption"">" & SpanClassAdminSmall & "Other Head Tags</td>")
        '                Call FastString.Add("<td class=""ccAdminEditField"" colspan=""2"">" & SpanClassAdminNormal)
        '                If readOnlyField Then
        '                    Call FastString.Add(OtherHeadTags)
        '                Else
        '                    Call FastString.Add(cpCore.html.html_GetFormInputTextExpandable("MetaContent.OtherHeadTags", OtherHeadTags, 10))
        '                End If
        '                Call FastString.Add("</span></td></tr>")
        '                '
        '                s = "" _
        '                    & Adminui.EditTableOpen & FastString.Text & Adminui.EditTableClose _
        '                    & cpCore.html.html_GetFormInputHidden("MetaContent.MetaContentID", MetaContentID) _
        '                    & ""
        '                '
        '                GetForm_Edit_MetaContent = Adminui.GetEditPanel((Not allowAdminTabs), "Meta Content", "Meta Tags available for pages using this content", s)
        '                EditSectionPanelCount = EditSectionPanelCount + 1
        '                '
        '                FastString = Nothing
        '            End If
        '            '
        '            Exit Function
        '            '
        'ErrorTrap:
        '            FastString = Nothing
        '            Call handleLegacyClassError3("GetForm_Edit_MetaContent")
        '        End Function
        '
        '========================================================================
        ' Print the Email form Group associations
        '
        '   Content must conform to ccMember fields
        '========================================================================
        '
        Private Function GetForm_Edit_EmailRules(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, ByVal readOnlyField As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_EmailRules")
            '
            Dim f As New stringBuilderLegacyController
            Dim GroupList As String
            Dim GroupSplit() As String
            Dim Ptr As Integer
            Dim IDPtr As Integer
            Dim IDEndPtr As Integer
            Dim GroupID As Integer
            Dim ReportLink As String
            Dim Cnt As Integer
            Dim Adminui As New adminUIController(cpCore)
            Dim s As String
            '
            s = cpCore.html.getCheckList("EmailGroups", "Group Email", editRecord.id, "Groups", "Email Groups", "EmailID", "GroupID", , "Caption")
            's = cpCore.html.getInputCheckListCategories("EmailGroups", "Group Email", editRecord.id, "Groups", "Email Groups", "EmailID", "GroupID", , "Caption", readOnlyField, "Groups")
            s = "<tr>" _
                & "<td class=""ccAdminEditCaption"">Groups</td>" _
                & "<td class=""ccAdminEditField"" colspan=2>" & SpanClassAdminNormal & s & "</span></td>" _
                & "</tr><tr>" _
                & "<td class=""ccAdminEditCaption"">&nbsp;</td>" _
                & "<td class=""ccAdminEditField"" colspan=2>" & SpanClassAdminNormal & "[<a href=?cid=" & Models.Complex.cdefModel.getContentId(cpCore, "Groups") & " target=_blank>Manage Groups</a>]</span></td>" _
                & "</tr>"
            s = Adminui.EditTableOpen & s & Adminui.EditTableClose
            s = Adminui.GetEditPanel((Not allowAdminTabs), "Email Rules", "Send email to people in these groups", s)
            GetForm_Edit_EmailRules = s
            Exit Function
            'GroupList = cpCore.htmldoc.main_GetFormInputCheckList("EmailGroups", "Group Email", EditRecord.ID, "Groups", "Email Groups", "EmailID", "GroupID", , "Caption", readOnlyField)
            GroupSplit = Split(GroupList, "<br >", , vbTextCompare)
            Cnt = UBound(GroupSplit) + 1
            If Cnt = 0 Then
                f.Add("<tr>")
                f.Add("<td class=""ccAdminEditCaption"">&nbsp;</td>")
                f.Add("<td class=""ccAdminEditField"" colspan=2>" & SpanClassAdminNormal & "There are no currently no groups defined.</span></td>")
                f.Add("</tr>")
            Else
                For Ptr = 0 To Cnt - 1
                    GroupID = 0
                    Dim HiddenPos As Integer
                    HiddenPos = genericController.vbInstr(1, GroupSplit(Ptr), "hidden", vbTextCompare)
                    If HiddenPos > 0 Then
                        IDPtr = genericController.vbInstr(1, GroupSplit(Ptr), "value=", vbTextCompare)
                        'IDPtr = genericController.vbInstr(HiddenPos, GroupSplit(Ptr), "value=", vbTextCompare)
                        If IDPtr > 0 Then
                            IDEndPtr = genericController.vbInstr(IDPtr, GroupSplit(Ptr), ">")
                            If IDEndPtr > 0 Then
                                GroupID = genericController.EncodeInteger(Mid(GroupSplit(Ptr), IDPtr + 6, IDEndPtr - IDPtr - 6))
                            End If
                        End If
                        If GroupID > 0 Then
                            ReportLink = "[<a href=""?" & RequestNameAdminForm & "=12&rid=35&recordid=" & GroupID & """ target=_blank>Group&nbsp;Report</a>]"
                        Else
                            ReportLink = "&nbsp;"
                        End If
                    End If
                    f.Add("<tr>" _
                        & "<td class=""ccAdminEditCaption"">&nbsp;</td>" _
                        & "<td class=""ccAdminEditField"" colspan=2>" & SpanClassAdminNormal & GroupSplit(Ptr) & "&nbsp;" & ReportLink & "</span></td>" _
                        & "</tr>")
                Next
            End If
            f.Add("<tr>")
            f.Add("<td class=""ccAdminEditCaption"">&nbsp;</td>")
            f.Add("<td class=""ccAdminEditCaption"" colspan=2>" & SpanClassAdminNormal & "[<a href=?cid=" & Models.Complex.cdefModel.getContentId(cpCore, "Groups") & " target=_blank>Manage Groups</a>]</span></td>")
            f.Add("</tr>")
            GetForm_Edit_EmailRules = Adminui.GetEditPanel((Not allowAdminTabs), "Email Rules", "Send email to people in these groups", Adminui.EditTableOpen & f.Text & Adminui.EditTableClose)
            EditSectionPanelCount = EditSectionPanelCount + 1
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Edit_EmailRules")
        End Function
        '
        '========================================================================
        ' Print the Email for Topic associations
        '
        '   Content must conform to ccMember fields
        '========================================================================
        '
        Private Function GetForm_Edit_EmailTopics(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, ByVal readOnlyField As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_EmailTopics")
            '
            Dim f As New stringBuilderLegacyController
            Dim GroupList As String
            Dim GroupSplit() As String
            Dim Ptr As Integer
            Dim IDPtr As Integer
            Dim IDEndPtr As Integer
            Dim GroupID As Integer
            Dim ReportLink As String
            Dim Cnt As Integer
            Dim Adminui As New adminUIController(cpCore)
            '
            Dim s As String
            '
            s = cpCore.html.getCheckList("EmailTopics", "Group Email", editRecord.id, "Topics", "Email Topics", "EmailID", "TopicID", , "Name")
            's = cpCore.html.getInputCheckListCategories("EmailTopics", "Group Email", editRecord.id, "Topics", "Email Topics", "EmailID", "TopicID", , "Name", readOnlyField, "Topics")
            s = "<tr>" _
                & "<td class=""ccAdminEditCaption"">Topics</td>" _
                & "<td class=""ccAdminEditField"" colspan=2>" & SpanClassAdminNormal & s & "</span></td>" _
                & "</tr><tr>" _
                & "<td class=""ccAdminEditCaption"">&nbsp;</td>" _
                & "<td class=""ccAdminEditField"" colspan=2>" & SpanClassAdminNormal & "[<a href=?cid=" & Models.Complex.cdefModel.getContentId(cpCore, "Topics") & " target=_blank>Manage Topics</a>]</span></td>" _
                & "</tr>"
            s = Adminui.EditTableOpen & s & Adminui.EditTableClose
            s = Adminui.GetEditPanel((Not allowAdminTabs), "Email Rules", "Send email to people in these groups", s)
            GetForm_Edit_EmailTopics = s
            Exit Function

            'GroupList = cpCore.htmldoc.main_GetFormInputCheckList("EmailTopics", "Group Email", EditRecord.ID, "Topics", "Email Topics", "EmailID", "TopicID", , "Name", readOnlyField)
            'GroupSplit = Split(GroupList, "<br >", , vbTextCompare)
            'Cnt = UBound(GroupSplit) + 1
            'If Cnt = 0 Then
            '    f.Add("<tr>")
            '    f.Add("<td class=""ccAdminEditCaption"">&nbsp;</td>")
            '    f.Add("<td class=""ccAdminEditField"" colspan=2>" & SpanClassAdminNormal & "There are no currently no topics defined.</span></td>")
            '    f.Add("</tr>")
            'Else
            '    For Ptr = 0 To UBound(GroupSplit)
            '        GroupID = 0
            '        IDPtr = genericController.vbInstr(1, GroupSplit(Ptr), "value=", vbTextCompare)
            '        If IDPtr > 0 Then
            '            IDEndPtr = genericController.vbInstr(IDPtr, GroupSplit(Ptr), ">")
            '            If IDEndPtr > 0 Then
            '                GroupID = genericController.EncodeInteger(Mid(GroupSplit(Ptr), IDPtr + 6, IDEndPtr - IDPtr - 6))
            '            End If
            '        End If
            '        If GroupID > 0 Then
            '            ReportLink = "&nbsp;"
            '            'ReportLink = "<a href=""?" & RequestNameAdminForm & "=12&rid=35&recordid=" & GroupID & """ target=_blank>Group&nbsp;Report</a>"
            '        Else
            '            ReportLink = "&nbsp;"
            '        End If
            '        f.Add("<tr>" _
            '            & "<td class=""ccAdminEditCaption"">&nbsp;</td>" _
            '            & "<td class=""ccAdminEditField"" colspan=2>" & SpanClassAdminNormal & GroupSplit(Ptr) & ReportLink & "</span></td>" _
            '            & "</tr>")
            '    Next
            'End If
            'f.Add("<tr>")
            'f.Add("<td class=""ccAdminEditCaption"">&nbsp;</td>")
            'f.Add("<td class=""ccAdminEditField"" colspan=2>" & SpanClassAdminNormal & "[<a href=?cid=" & cpCore.main_GetContentID("Topics") & " target=_blank>Manage Topics</a>]</span></td>")
            'f.Add("</tr>")
            'GetForm_Edit_EmailTopics = Adminui.GetEditPanel((Not AllowAdminTabs), "Email Topics", "Send email to people who are associated with these topics", Adminui.EditTableOpen & f.Text & Adminui.EditTableClose)
            'EditSectionPanelCount = EditSectionPanelCount + 1
            'Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Edit_EmailTopics")
            '
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Function GetForm_Edit_EmailBounceStatus() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_EmailBounceStatus")
            '
            Dim f As New stringBuilderLegacyController
            Dim Copy As String
            Dim Adminui As New adminUIController(cpCore)
            '
            f.Add(Adminui.GetEditRow("<a href=?" & RequestNameAdminForm & "=28 target=_blank>Open in New Window</a>", "Email Control", "The settings in this section can be modified with the Email Control page."))
            f.Add(Adminui.GetEditRow(cpCore.siteProperties.getText("EmailBounceAddress", ""), "Bounce Email Address", "All bounced emails will be sent to this address automatically. This must be a valid email account, and you should either use Contensive Bounce processing to capture the emails, or manually remove them from the account yourself."))
            f.Add(Adminui.GetEditRow(genericController.GetYesNo(genericController.EncodeBoolean(cpCore.siteProperties.getBoolean("AllowEmailBounceProcessing", False))), "Allow Bounce Email Processing", "If checked, Contensive will periodically retrieve all the email from the POP email account and take action on the membefr account that sent the email."))
            Select Case cpCore.siteProperties.getText("EMAILBOUNCEPROCESSACTION", "0")
                Case "1"
                    Copy = "Clear the Allow Group Email field for all members with a matching Email address"
                Case "2"
                    Copy = "Clear all member Email addresses that match the Email address"
                Case "3"
                    Copy = "Delete all Members with a matching Email address"
                Case Else
                    Copy = "Do Nothing"
            End Select
            f.Add(Adminui.GetEditRow(Copy, "Bounce Email Action", "When an email is determined to be a bounce, this action will taken against member with that email address."))
            f.Add(Adminui.GetEditRow(cpCore.siteProperties.getText("POPServerStatus"), "Last Email Retrieve Status", "This is the status of the last POP email retrieval attempted."))
            '
            GetForm_Edit_EmailBounceStatus = Adminui.GetEditPanel((Not allowAdminTabs), "Bounced Email Handling", "", Adminui.EditTableOpen & f.Text & Adminui.EditTableClose)
            EditSectionPanelCount = EditSectionPanelCount + 1
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Edit_EmailBounceStatus")
            '
        End Function
        '
        '========================================================================
        ' Print the Member Edit form
        '
        '   Content must conform to ccMember fields
        '========================================================================
        '
        Private Function GetForm_Edit_MemberGroups(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_MemberGroups")
            '
            Dim f As New stringBuilderLegacyController
            Dim Copy As String
            Dim SQL As String
            Dim CS As Integer
            Dim MembershipCount As Integer
            Dim MembershipSize As Integer
            Dim MembershipPointer As Integer
            Dim SectionName As String
            Dim PeopleContentID As Integer
            Dim GroupContentID As Integer
            Dim CanSeeHiddenGroups As Boolean
            Dim DateExpireValue As String
            Dim GroupCount As Integer
            Dim GroupID As Integer
            Dim GroupName As String
            Dim GroupCaption As String
            Dim GroupActive As Boolean
            Dim Membership() As Integer = {}
            Dim DateExpires() As Date = {}
            Dim Active() As Boolean = {}
            Dim Caption As String
            Dim MethodName As String
            Dim ReportLink As String
            Dim Adminui As New adminUIController(cpCore)
            '
            MethodName = "GetForm_Edit_MemberGroups"
            '
            ' ----- Gather all the SecondaryContent that associates to the PrimaryContent
            '
            PeopleContentID = Models.Complex.cdefModel.getContentId(cpCore, "People")
            GroupContentID = Models.Complex.cdefModel.getContentId(cpCore, "Groups")
            '
            MembershipCount = 0
            MembershipSize = 0
            If True Then
                'If EditRecord.ID <> 0 Then
                '
                ' ----- read in the groups that this member has subscribed (exclude new member records)
                '
                If editRecord.id <> 0 Then
                    SQL = "SELECT Active,GroupID,DateExpires" _
                        & " FROM ccMemberRules" _
                        & " WHERE MemberID=" & editRecord.id
                    CS = cpCore.db.csOpenSql_rev("Default", SQL)
                    Do While cpCore.db.csOk(CS)
                        If MembershipCount >= MembershipSize Then
                            MembershipSize = MembershipSize + 10
                            ReDim Preserve Membership(MembershipSize)
                            ReDim Preserve Active(MembershipSize)
                            ReDim Preserve DateExpires(MembershipSize)
                        End If
                        Membership(MembershipCount) = cpCore.db.csGetInteger(CS, "GroupID")
                        DateExpires(MembershipCount) = cpCore.db.csGetDate(CS, "DateExpires")
                        Active(MembershipCount) = cpCore.db.csGetBoolean(CS, "Active")
                        MembershipCount = MembershipCount + 1
                        cpCore.db.csGoNext(CS)
                    Loop
                    Call cpCore.db.csClose(CS)
                End If
                '
                ' ----- read in all the groups, sorted by ContentName
                '
                SQL = "SELECT ccGroups.ID AS ID, ccContent.Name AS SectionName, ccGroups.Caption AS GroupCaption, ccGroups.name AS GroupName, ccGroups.SortOrder" _
                    & " FROM ccGroups LEFT JOIN ccContent ON ccGroups.ContentControlID = ccContent.ID" _
                    & " Where (((ccGroups.Active) <> " & SQLFalse & ") And ((ccContent.Active) <> " & SQLFalse & "))"
                SQL &= "" _
                    & " GROUP BY ccGroups.ID, ccContent.Name, ccGroups.Caption, ccGroups.name, ccGroups.SortOrder" _
                    & " ORDER BY ccGroups.Caption"
                'sql &= "" _
                '    & " GROUP BY ccGroups.ID, ccContent.Name, ccGroups.Caption, ccGroups.name, ccGroups.SortOrder" _
                '    & " ORDER BY ccContent.Name, ccGroups.Caption"
                CS = cpCore.db.csOpenSql_rev("Default", SQL)
                '
                ' Output all the groups, with the active and dateexpires from those joined
                '
                f.Add(Adminui.EditTableOpen)
                SectionName = ""
                GroupCount = 0
                CanSeeHiddenGroups = cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore)
                Do While cpCore.db.csOk(CS)
                    GroupName = cpCore.db.csGet(CS, "GroupName")
                    If (Mid(GroupName, 1, 1) <> "_") Or CanSeeHiddenGroups Then
                        GroupCaption = cpCore.db.csGet(CS, "GroupCaption")
                        GroupID = cpCore.db.csGetInteger(CS, "ID")
                        If GroupCaption = "" Then
                            GroupCaption = GroupName
                            If GroupCaption = "" Then
                                GroupCaption = "Group&nbsp;" & GroupID
                            End If
                        End If
                        GroupActive = False
                        DateExpireValue = ""
                        If MembershipCount <> 0 Then
                            For MembershipPointer = 0 To MembershipCount - 1
                                If Membership(MembershipPointer) = GroupID Then
                                    GroupActive = Active(MembershipPointer)
                                    If DateExpires(MembershipPointer) > Date.MinValue Then
                                        DateExpireValue = genericController.encodeText(DateExpires(MembershipPointer))
                                    End If
                                    Exit For
                                End If
                            Next
                        End If
                        ReportLink = ""
                        ReportLink = ReportLink & "[<a href=""?af=4&cid=" & GroupContentID & "&id=" & GroupID & """>Edit&nbsp;Group</a>]"
                        If GroupID > 0 Then
                            ReportLink = ReportLink & "&nbsp;[<a href=""?" & RequestNameAdminForm & "=12&rid=35&recordid=" & GroupID & """>Group&nbsp;Report</a>]"
                        End If
                        '
                        If GroupCount = 0 Then
                            Caption = SpanClassAdminSmall & "Groups</span>"
                        Else
                            Caption = "&nbsp;"
                        End If
                        f.Add("<tr><td class=""ccAdminEditCaption"">" & Caption & "</td>")
                        f.Add("<td class=""ccAdminEditField"">")
                        f.Add("<table border=0 cellpadding=0 cellspacing=0 width=""100%"" ><tr>")
                        f.Add("<td width=""40%"">" & cpCore.html.html_GetFormInputHidden("Memberrules." & GroupCount & ".ID", GroupID) & cpCore.html.html_GetFormInputCheckBox2("MemberRules." & GroupCount, GroupActive) & GroupCaption & "</td>")
                        f.Add("<td width=""30%""> Expires " & cpCore.html.html_GetFormInputText2("MemberRules." & GroupCount & ".DateExpires", DateExpireValue, 1, 20) & "</td>")
                        f.Add("<td width=""30%"">" & ReportLink & "</td>")
                        f.Add("</tr></table>")
                        f.Add("</td></tr>")
                        GroupCount = GroupCount + 1
                    End If
                    cpCore.db.csGoNext(CS)
                Loop
                cpCore.db.csClose(CS)
            End If
            If GroupCount = 0 Then
                'If EditRecord.ID = 0 Then
                '    F.Add( "<tr>" _
                '        & "<td valign=middle align=right>" & SpanClassAdminSmall & "Groups</span></td>" _
                '        & "<td>" & SpanClassAdminNormal & "Groups will be available after this record is saved</span></td>" _
                '        & "</tr>"
                'ElseIf GroupCount = 0 Then
                f.Add("<tr>" _
                    & "<td valign=middle align=right>" & SpanClassAdminSmall & "Groups</span></td>" _
                    & "<td>" & SpanClassAdminNormal & "There are currently no groups defined</span></td>" _
                    & "</tr>")
            Else
                f.Add("<input type=""hidden"" name=""MemberRules.RowCount"" value=""" & GroupCount & """>")
            End If
            f.Add("<tr>")
            f.Add("<td class=""ccAdminEditCaption"">&nbsp;</td>")
            f.Add("<td class=""ccAdminEditField"">" & SpanClassAdminNormal & "[<a href=?cid=" & Models.Complex.cdefModel.getContentId(cpCore, "Groups") & " target=_blank>Manage Groups</a>]</span></td>")
            f.Add("</tr>")

            GetForm_Edit_MemberGroups = Adminui.GetEditPanel((Not allowAdminTabs), "Group Membership", "This person is a member of these groups", Adminui.EditTableOpen & f.Text & Adminui.EditTableClose)
            EditSectionPanelCount = EditSectionPanelCount + 1
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Edit_MemberGroups")
        End Function
        '
        '========================================================================
        '   Special case tab for Layout records
        '========================================================================
        '
        Private Function GetForm_Edit_LayoutReports(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_LayoutReports")
            '
            Dim FastString As stringBuilderLegacyController
            Dim Adminui As New adminUIController(cpCore)
            '
            FastString = New stringBuilderLegacyController
            Call FastString.Add("<tr>")
            Call FastString.Add("<td valign=""top"" align=""right"">&nbsp;</td>")
            Call FastString.Add("<td colspan=""2"" class=""ccAdminEditField"" align=""left"">" & SpanClassAdminNormal)
            Call FastString.Add("<ul class=""ccList"">")
            Call FastString.Add("<li class=""ccListItem""><a target=""_blank"" href=""/preview?layout=" & editRecord.id & """>Preview this layout</A></LI>")
            Call FastString.Add("</ul>")
            Call FastString.Add("</span></td></tr>")
            GetForm_Edit_LayoutReports = Adminui.GetEditPanel((Not allowAdminTabs), "Contensive Reporting", "", Adminui.EditTableOpen & FastString.Text & Adminui.EditTableClose)
            EditSectionPanelCount = EditSectionPanelCount + 1
            FastString = Nothing
            Exit Function
            '
ErrorTrap:
            FastString = Nothing
            Call handleLegacyClassError3("GetForm_Edit_LayoutReports")
        End Function
        '
        '========================================================================
        '   Special case tab for People records
        '========================================================================
        '
        Private Function GetForm_Edit_MemberReports(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_MemberReports")
            '
            Dim FastString As stringBuilderLegacyController
            Dim Adminui As New adminUIController(cpCore)
            '
            FastString = New stringBuilderLegacyController
            Call FastString.Add("<tr>")
            Call FastString.Add("<td valign=""top"" align=""right"">&nbsp;</td>")
            Call FastString.Add("<td colspan=""2"" class=""ccAdminEditField"" align=""left"">" & SpanClassAdminNormal)
            Call FastString.Add("<ul class=""ccList"">")
            Call FastString.Add("<li class=""ccListItem""><a target=""_blank"" href=""/" & cpCore.serverConfig.appConfig.adminRoute & "?" & RequestNameAdminForm & "=" & AdminFormReports & "&rid=3&MemberID=" & editRecord.id & "&DateTo=" & Int(cpCore.doc.profileStartTime.ToOADate) & "&DateFrom=" & Int(cpCore.doc.profileStartTime.ToOADate) - 365 & """>All visits from this person</A></LI>")
            Call FastString.Add("<li class=""ccListItem""><a target=""_blank"" href=""/" & cpCore.serverConfig.appConfig.adminRoute & "?" & RequestNameAdminForm & "=" & AdminFormReports & "&rid=13&MemberID=" & editRecord.id & "&DateTo=" & Int(CDbl(cpCore.doc.profileStartTime.ToOADate)) & "&DateFrom=" & Int(CDbl(cpCore.doc.profileStartTime.ToOADate) - 365) & """>All orders from this person</A></LI>")
            Call FastString.Add("</ul>")
            Call FastString.Add("</span></td></tr>")
            GetForm_Edit_MemberReports = Adminui.GetEditPanel((Not allowAdminTabs), "Contensive Reporting", "", Adminui.EditTableOpen & FastString.Text & Adminui.EditTableClose)
            EditSectionPanelCount = EditSectionPanelCount + 1
            FastString = Nothing
            Exit Function
            '
ErrorTrap:
            FastString = Nothing
            Call handleLegacyClassError3("GetForm_Edit_MemberReports")
        End Function
        '        '
        '        '========================================================================
        '        '   Print the path Rules section of the path edit form
        '        '========================================================================
        '        '
        '        Private Function GetForm_Edit_PathRules(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_PathRules")
        '            '
        '            Dim FastString As stringBuilderLegacyController
        '            Dim Adminui As New adminUIController(cpCore)
        '            '
        '            FastString = New stringBuilderLegacyController
        '            Call FastString.Add("<tr>")
        '            Call FastString.Add("<td valign=""top"" align=""right"">" & SpanClassAdminSmall & "Groups</td>")
        '            Call FastString.Add("<td colspan=""2"" class=""ccAdminEditField"" align=""left"">" & SpanClassAdminNormal & cpCore.html.getINputChecList2("PathRules", "Paths", editRecord.id, "Groups", "Path Rules", "PathID", "GroupID", , "Caption") & "</span></td>")
        '            Call FastString.Add("</tr>")
        '            'Call FastString.Add(adminui.EditTableClose)
        '            GetForm_Edit_PathRules = Adminui.GetEditPanel((Not allowAdminTabs), "Path Permissions", "Groups that have access to this path", Adminui.EditTableOpen & FastString.Text & Adminui.EditTableClose)
        '            EditSectionPanelCount = EditSectionPanelCount + 1
        '            FastString = Nothing
        '            Exit Function
        '            '
        'ErrorTrap:
        '            FastString = Nothing
        '            Call handleLegacyClassError3("GetForm_Edit_PathRules")
        '        End Function
        '
        '========================================================================
        '   Print the path Rules section of the path edit form
        '========================================================================
        '
        Private Function GetForm_Edit_PageContentBlockRules(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_PageContentBlockRules")
            '
            Dim f As New stringBuilderLegacyController
            Dim GroupList As String
            Dim GroupSplit() As String
            Dim Ptr As Integer
            Dim IDPtr As Integer
            Dim IDEndPtr As Integer
            Dim GroupID As Integer
            Dim ReportLink As String
            Dim Adminui As New adminUIController(cpCore)
            '
            GroupList = cpCore.html.getCheckList2("PageContentBlockRules", adminContent.Name, editRecord.id, "Groups", "Page Content Block Rules", "RecordID", "GroupID", , "Caption", False)
            GroupSplit = Split(GroupList, "<br >", , vbTextCompare)
            For Ptr = 0 To UBound(GroupSplit)
                GroupID = 0
                IDPtr = genericController.vbInstr(1, GroupSplit(Ptr), "value=", vbTextCompare)
                If IDPtr > 0 Then
                    IDEndPtr = genericController.vbInstr(IDPtr, GroupSplit(Ptr), ">")
                    If IDEndPtr > 0 Then
                        GroupID = genericController.EncodeInteger(Mid(GroupSplit(Ptr), IDPtr + 6, IDEndPtr - IDPtr - 6))
                    End If
                End If
                If GroupID > 0 Then
                    ReportLink = "[<a href=""?" & RequestNameAdminForm & "=12&rid=35&recordid=" & GroupID & """ target=_blank>Group&nbsp;Report</a>]"
                Else
                    ReportLink = "&nbsp;"
                End If
                f.Add("<tr>" _
                    & "<td>&nbsp;</td>" _
                    & "<td class=""ccAdminEditField"" align=left>" & SpanClassAdminNormal & GroupSplit(Ptr) & "</span></td>" _
                    & "<td class=""ccAdminEditField"" align=center>" & ReportLink & "</td>" _
                    & "</tr>")
            Next
            GetForm_Edit_PageContentBlockRules = Adminui.GetEditPanel((Not allowAdminTabs), "Content Blocking", "If content is blocked, select groups that have access to this content", Adminui.EditTableOpen & f.Text & Adminui.EditTableClose)
            EditSectionPanelCount = EditSectionPanelCount + 1
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Edit_PageContentBlockRules")
        End Function
        '
        '========================================================================
        '   Print the path Rules section of the path edit form
        '========================================================================
        '
        Private Function GetForm_Edit_LibraryFolderRules(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_LibraryFolderRules")
            '
            Dim Copy As String
            Dim f As New stringBuilderLegacyController
            Dim GroupList As String
            Dim GroupSplit() As String
            Dim Ptr As Integer
            Dim IDPtr As Integer
            Dim IDEndPtr As Integer
            Dim GroupID As Integer
            Dim ReportLink As String
            Dim Adminui As New adminUIController(cpCore)
            '
            GroupList = cpCore.html.getCheckList2("LibraryFolderRules", adminContent.Name, editRecord.id, "Groups", "Library Folder Rules", "FolderID", "GroupID", , "Caption")
            GroupSplit = Split(GroupList, "<br >", , vbTextCompare)
            For Ptr = 0 To UBound(GroupSplit)
                GroupID = 0
                IDPtr = genericController.vbInstr(1, GroupSplit(Ptr), "value=", vbTextCompare)
                If IDPtr > 0 Then
                    IDEndPtr = genericController.vbInstr(IDPtr, GroupSplit(Ptr), ">")
                    If IDEndPtr > 0 Then
                        GroupID = genericController.EncodeInteger(Mid(GroupSplit(Ptr), IDPtr + 6, IDEndPtr - IDPtr - 6))
                    End If
                End If
                If GroupID > 0 Then
                    ReportLink = "[<a href=""?" & RequestNameAdminForm & "=12&rid=35&recordid=" & GroupID & """ target=_blank>Group&nbsp;Report</a>]"
                Else
                    ReportLink = "&nbsp;"
                End If
                f.Add("<tr>" _
                    & "<td>&nbsp;</td>" _
                    & "<td class=""ccAdminEditField"" align=left>" & SpanClassAdminNormal & GroupSplit(Ptr) & "</span></td>" _
                    & "<td class=""ccAdminEditField"" align=center>" & ReportLink & "</td>" _
                    & "</tr>")
            Next
            Copy = "Select groups who have authoring access within this folder. This means if you are in this group you can upload files, delete files, create folders and delete folders within this folder and any subfolders."
            GetForm_Edit_LibraryFolderRules = Adminui.GetEditPanel((Not allowAdminTabs), "Folder Permissions", Copy, Adminui.EditTableOpen & f.Text & Adminui.EditTableClose)
            EditSectionPanelCount = EditSectionPanelCount + 1
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Edit_LibraryFolderRules")
        End Function
        '
        '========================================================================
        ' Print the Group Rules section for Content Edit form
        '   Group rules show which groups have authoring rights to a content
        '
        '   adminContent.id is the ContentID of the Content Definition being edited
        '   EditRecord.ContentID is the ContentControlID of the Edit Record
        '========================================================================
        '
        Private Function GetForm_Edit_GroupRules(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_GroupRules")
            '
            Dim SQL As String
            Dim CS As Integer
            Dim GroupRulesCount As Integer
            Dim GroupRulesSize As Integer
            Dim GroupRulesPointer As Integer
            Dim SectionName As String
            Dim GroupName As String
            Dim GroupCount As Integer
            Dim GroupFound As Boolean
            Dim GroupRules() As GroupRuleType = {}
            Dim FastString As stringBuilderLegacyController
            Dim Adminui As New adminUIController(cpCore)
            '
            ' ----- Open the panel
            '
            FastString = New stringBuilderLegacyController
            '
            'Call cpCore.main_PrintPanelTop("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
            'Call call FastString.Add(adminui.EditTableOpen)
            '
            ' ----- Gather all the groups which have authoring rights to the content
            '
            GroupRulesCount = 0
            GroupRulesSize = 0
            If editRecord.id <> 0 Then
                SQL = "SELECT ccGroups.ID AS ID, ccGroupRules.AllowAdd as allowadd, ccGroupRules.AllowDelete as allowdelete" _
                    & " FROM ccGroups LEFT JOIN ccGroupRules ON ccGroups.ID = ccGroupRules.GroupID" _
                    & " WHERE (((ccGroupRules.ContentID)=" & editRecord.id & ") AND ((ccGroupRules.Active)<>0) AND ((ccGroups.Active)<>0))"
                CS = cpCore.db.csOpenSql_rev("Default", SQL)
                If cpCore.db.csOk(CS) Then
                    If True Then
                        GroupRulesSize = 100
                        ReDim GroupRules(GroupRulesSize)
                        Do While cpCore.db.csOk(CS)
                            If GroupRulesCount >= GroupRulesSize Then
                                GroupRulesSize = GroupRulesSize + 100
                                ReDim Preserve GroupRules(GroupRulesSize)
                            End If
                            GroupRules(GroupRulesCount).GroupID = cpCore.db.csGetInteger(CS, "ID")
                            GroupRules(GroupRulesCount).AllowAdd = cpCore.db.csGetBoolean(CS, "AllowAdd")
                            GroupRules(GroupRulesCount).AllowDelete = cpCore.db.csGetBoolean(CS, "AllowDelete")
                            GroupRulesCount = GroupRulesCount + 1
                            Call cpCore.db.csGoNext(CS)
                        Loop
                    End If
                End If
            End If
            cpCore.db.csClose(CS)
            '
            ' ----- Gather all the groups, sorted by ContentName
            '
            SQL = "SELECT ccGroups.ID AS ID, ccContent.Name AS SectionName, ccGroups.Name AS GroupName, ccGroups.Caption AS GroupCaption, ccGroups.SortOrder" _
                & " FROM ccGroups LEFT JOIN ccContent ON ccGroups.ContentControlID = ccContent.ID" _
                & " Where (((ccGroups.Active) <> " & SQLFalse & ") And ((ccContent.Active) <> " & SQLFalse & "))" _
                & " GROUP BY ccGroups.ID, ccContent.Name, ccGroups.Name, ccGroups.Caption, ccGroups.SortOrder" _
                & " ORDER BY ccContent.Name, ccGroups.Caption, ccGroups.SortOrder"
            CS = cpCore.db.csOpenSql_rev("Default", SQL)
            If Not cpCore.db.csOk(CS) Then
                Call FastString.Add(vbCrLf & "<tr><td colspan=""3"">" & SpanClassAdminSmall & "There are no active groups</span></td></tr>")
            Else
                If True Then
                    'Call FastString.Add(vbCrLf & "<tr><td colspan=""3"" class=""ccAdminEditSubHeader"">Groups with authoring access</td></tr>")
                    SectionName = ""
                    GroupCount = 0
                    Do While cpCore.db.csOk(CS)
                        GroupName = cpCore.db.csGet(CS, "GroupCaption")
                        If GroupName = "" Then
                            GroupName = cpCore.db.csGet(CS, "GroupName")
                        End If
                        Call FastString.Add("<tr>")
                        If SectionName <> cpCore.db.csGet(CS, "SectionName") Then
                            '
                            ' ----- create the next section
                            '
                            SectionName = cpCore.db.csGet(CS, "SectionName")
                            Call FastString.Add("<td valign=""top"" align=""right"">" & SpanClassAdminSmall & SectionName & "</td>")
                        Else
                            Call FastString.Add("<td valign=""top"" align=""right"">&nbsp;</td>")
                        End If
                        Call FastString.Add("<td class=""ccAdminEditField"" align=""left"" colspan=""2"">" & SpanClassAdminSmall)
                        GroupFound = False
                        If GroupRulesCount <> 0 Then
                            For GroupRulesPointer = 0 To GroupRulesCount - 1
                                If GroupRules(GroupRulesPointer).GroupID = cpCore.db.csGetInteger(CS, "ID") Then
                                    GroupFound = True
                                    Exit For
                                End If
                            Next
                        End If
                        Call FastString.Add("<input type=""hidden"" name=""GroupID" & GroupCount & """ value=""" & cpCore.db.csGet(CS, "ID") & """>")
                        Call FastString.Add("<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""400""><tr>")
                        If GroupFound Then
                            Call FastString.Add("<td width=""200"">" & SpanClassAdminSmall & cpCore.html.html_GetFormInputCheckBox2("Group" & GroupCount, True) & GroupName & "</span></td>")
                            Call FastString.Add("<td width=""100"">" & SpanClassAdminSmall & cpCore.html.html_GetFormInputCheckBox2("GroupRuleAllowAdd" & GroupCount, GroupRules(GroupRulesPointer).AllowAdd) & " Allow Add</span></td>")
                            Call FastString.Add("<td width=""100"">" & SpanClassAdminSmall & cpCore.html.html_GetFormInputCheckBox2("GroupRuleAllowDelete" & GroupCount, GroupRules(GroupRulesPointer).AllowDelete) & " Allow Delete</span></td>")
                        Else
                            Call FastString.Add("<td width=""200"">" & SpanClassAdminSmall & cpCore.html.html_GetFormInputCheckBox2("Group" & GroupCount, False) & GroupName & "</span></td>")
                            Call FastString.Add("<td width=""100"">" & SpanClassAdminSmall & cpCore.html.html_GetFormInputCheckBox2("GroupRuleAllowAdd" & GroupCount, False) & " Allow Add</span></td>")
                            Call FastString.Add("<td width=""100"">" & SpanClassAdminSmall & cpCore.html.html_GetFormInputCheckBox2("GroupRuleAllowDelete" & GroupCount, False) & " Allow Delete</span></td>")
                        End If
                        Call FastString.Add("</tr></table>")
                        Call FastString.Add("</span></td>")
                        Call FastString.Add("</tr>")
                        GroupCount = GroupCount + 1
                        Call cpCore.db.csGoNext(CS)
                    Loop
                    Call FastString.Add(vbCrLf & "<input type=""hidden"" name=""GroupCount"" value=""" & GroupCount & """>")
                End If
            End If
            cpCore.db.csClose(CS)
            '
            ' ----- close the panel
            '
            'Call FastString.Add(adminui.EditTableClose)
            'Call cpCore.main_PrintPanelBottom("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
            '
            GetForm_Edit_GroupRules = Adminui.GetEditPanel((Not allowAdminTabs), "Authoring Permissions", "The following groups can edit this content.", Adminui.EditTableOpen & FastString.Text & Adminui.EditTableClose)
            EditSectionPanelCount = EditSectionPanelCount + 1
            FastString = Nothing
            Exit Function
            '
ErrorTrap:
            FastString = Nothing
            Call handleLegacyClassError3("GetForm_Edit_GroupRules")
        End Function
        '
        '========================================================================
        '   Get all content authorable by the current group
        '========================================================================
        '
        Private Function GetForm_Edit_ContentGroupRules(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_ContentGroupRules")
            '
            Dim SQL As String
            Dim CS As Integer
            Dim ContentGroupRulesCount As Integer
            Dim ContentGroupRulesSize As Integer
            Dim ContentGroupRulesPointer As Integer
            Dim ContentName As String
            Dim ContentCount As Integer
            Dim ContentFound As Boolean
            Dim ContentGroupRules() As ContentGroupRuleType = {}
            Dim FastString As stringBuilderLegacyController
            Dim Adminui As New adminUIController(cpCore)
            '
            If cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                '
                ' ----- Open the panel
                '
                FastString = New stringBuilderLegacyController
                '
                'Call cpCore.main_PrintPanelTop("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                'Call call FastString.Add(adminui.EditTableOpen)
                '
                ' ----- Gather all the groups which have authoring rights to the content
                '
                ContentGroupRulesCount = 0
                ContentGroupRulesSize = 0
                If editRecord.id <> 0 Then
                    SQL = "SELECT ccContent.ID AS ID, ccGroupRules.AllowAdd as allowadd, ccGroupRules.AllowDelete as allowdelete" _
                        & " FROM ccContent LEFT JOIN ccGroupRules ON ccContent.ID = ccGroupRules.ContentID" _
                        & " WHERE (((ccGroupRules.GroupID)=" & editRecord.id & ") AND ((ccGroupRules.Active)<>0) AND ((ccContent.Active)<>0))"
                    CS = cpCore.db.csOpenSql_rev("Default", SQL)
                    If cpCore.db.csOk(CS) Then
                        ContentGroupRulesSize = 100
                        ReDim ContentGroupRules(ContentGroupRulesSize)
                        Do While cpCore.db.csOk(CS)
                            If ContentGroupRulesCount >= ContentGroupRulesSize Then
                                ContentGroupRulesSize = ContentGroupRulesSize + 100
                                ReDim Preserve ContentGroupRules(ContentGroupRulesSize)
                            End If
                            ContentGroupRules(ContentGroupRulesCount).ContentID = cpCore.db.csGetInteger(CS, "ID")
                            ContentGroupRules(ContentGroupRulesCount).AllowAdd = cpCore.db.csGetBoolean(CS, "AllowAdd")
                            ContentGroupRules(ContentGroupRulesCount).AllowDelete = cpCore.db.csGetBoolean(CS, "AllowDelete")
                            ContentGroupRulesCount = ContentGroupRulesCount + 1
                            Call cpCore.db.csGoNext(CS)
                        Loop
                    End If
                End If
                cpCore.db.csClose(CS)
                '
                ' ----- Gather all the content, sorted by ContentName
                '
                SQL = "SELECT ccContent.ID AS ID, ccContent.Name AS ContentName, ccContent.SortOrder" _
                    & " FROM ccContent" _
                    & " Where ccContent.Active<>0" _
                    & " ORDER BY ccContent.Name"
                CS = cpCore.db.csOpenSql_rev("Default", SQL)
                If Not cpCore.db.csOk(CS) Then
                    Call FastString.Add(vbCrLf & "<tr><td colspan=""3"">" & SpanClassAdminSmall & "There are no active groups</span></td></tr>")
                Else
                    ContentCount = 0
                    Do While cpCore.db.csOk(CS)
                        ContentName = cpCore.db.csGet(CS, "ContentName")
                        Call FastString.Add("<tr>")
                        Call FastString.Add("<td valign=""top"" align=""right"">&nbsp;</td>")
                        Call FastString.Add("<td class=""ccAdminEditField"" align=""left"" colspan=""2"">" & SpanClassAdminSmall)
                        ContentFound = False
                        If ContentGroupRulesCount <> 0 Then
                            For ContentGroupRulesPointer = 0 To ContentGroupRulesCount - 1
                                If ContentGroupRules(ContentGroupRulesPointer).ContentID = cpCore.db.csGetInteger(CS, "ID") Then
                                    ContentFound = True
                                    Exit For
                                End If
                            Next
                        End If
                        Call FastString.Add("<input type=""hidden"" name=""ContentID" & ContentCount & """ value=""" & cpCore.db.csGet(CS, "ID") & """>")
                        Call FastString.Add("<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""400""><tr>")
                        If ContentFound Then
                            Call FastString.Add("<td width=""200"">" & SpanClassAdminSmall & cpCore.html.html_GetFormInputCheckBox2("Content" & ContentCount, True) & ContentName & "</span></td>")
                            Call FastString.Add("<td width=""100"">" & SpanClassAdminSmall & cpCore.html.html_GetFormInputCheckBox2("ContentGroupRuleAllowAdd" & ContentCount, ContentGroupRules(ContentGroupRulesPointer).AllowAdd) & " Allow Add</span></td>")
                            Call FastString.Add("<td width=""100"">" & SpanClassAdminSmall & cpCore.html.html_GetFormInputCheckBox2("ContentGroupRuleAllowDelete" & ContentCount, ContentGroupRules(ContentGroupRulesPointer).AllowDelete) & " Allow Delete</span></td>")
                        Else
                            Call FastString.Add("<td width=""200"">" & SpanClassAdminSmall & cpCore.html.html_GetFormInputCheckBox2("Content" & ContentCount, False) & ContentName & "</span></td>")
                            Call FastString.Add("<td width=""100"">" & SpanClassAdminSmall & cpCore.html.html_GetFormInputCheckBox2("ContentGroupRuleAllowAdd" & ContentCount, False) & " Allow Add</span></td>")
                            Call FastString.Add("<td width=""100"">" & SpanClassAdminSmall & cpCore.html.html_GetFormInputCheckBox2("ContentGroupRuleAllowDelete" & ContentCount, False) & " Allow Delete</span></td>")
                        End If
                        Call FastString.Add("</tr></table>")
                        Call FastString.Add("</span></td>")
                        Call FastString.Add("</tr>")
                        ContentCount = ContentCount + 1
                        Call cpCore.db.csGoNext(CS)
                    Loop
                    Call FastString.Add(vbCrLf & "<input type=""hidden"" name=""ContentCount"" value=""" & ContentCount & """>")
                End If
                cpCore.db.csClose(CS)
                '
                ' ----- close the panel
                '
                'Call FastString.Add(adminui.EditTableClose)
                'Call cpCore.main_PrintPanelBottom("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                '
                GetForm_Edit_ContentGroupRules = Adminui.GetEditPanel((Not allowAdminTabs), "Authoring Permissions", "This group can edit the following content.", Adminui.EditTableOpen & FastString.Text & Adminui.EditTableClose)
                EditSectionPanelCount = EditSectionPanelCount + 1
                FastString = Nothing
            End If
            Exit Function
            '
ErrorTrap:
            FastString = Nothing
            Call handleLegacyClassError3("GetForm_Edit_ContentGroupRules")
        End Function
        '        '
        '        '========================================================================
        '        '   Gets the fields pointer if it exists, otherwise -1
        '        '
        '        '   Does not report an error
        '        '========================================================================
        '        '
        '        Private Function GetFieldPtrNoError(adminContent As appServices_metaDataClass.CDefClass, editRecord As editRecordClass, ByVal TargetField As String) As Integer

        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetFieldPtrNoError")
        '            '
        '            Dim UcaseTargetField As String
        '            ''Dim arrayOfFields() As appServices_metaDataClass.CDefFieldClass
        '            '
        '            GetFieldPtrNoError = -1
        '            UcaseTargetField = genericController.vbUCase(TargetField)
        '            If adminContent.fields.Count > 0 Then
        '                arrayOfFields = adminContent.fields
        '                For GetFieldPtrNoError = 0 To adminContent.fields.Count - 1

        '                    If genericController.vbUCase(arrayOfFields(GetFieldPtrNoError).Name) = UcaseTargetField Then
        '                        Exit For
        '                    End If
        '                Next
        '                If GetFieldPtrNoError >= adminContent.fields.Count Then
        '                    GetFieldPtrNoError = -1
        '                End If
        '            End If
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call handleLegacyClassError3("GetFieldPtrNoError")
        '            '
        '        End Function
        '        '
        '        '========================================================================
        '        ' Get FieldPointer from its FieldName
        '        '   Returns -1 if not found
        '        '========================================================================
        '        '
        '        Private Function GetFieldPtr(adminContent As appServices_metaDataClass.CDefClass, editRecord As editRecordClass, ByVal TargetField As String) As Integer
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetFieldPtr")
        '            '
        '            GetFieldPtr = GetFieldPtrNoError(TargetField)
        '            If GetFieldPtr = -1 Then
        '                Call handleLegacyClassError("AdminClass.GetFieldPtr", "Could not find content field [" & adminContent.Name & "].[" & TargetField & "]")
        '            End If
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call handleLegacyClassError3("GetFieldPtr")
        '            '
        '        End Function
        '
        '========================================================================
        ' MakeButton
        '   Prints the currently selected Button Type
        '   ButtonName is the ID field name given to the button object
        '   ButtonLabel is the words that appear on the button
        '   ButtonHref is the Link for the button
        '   ButtonWidth, if provided, is the width of a trans spacer.gif put under the ButtonLabel
        '   ButtonColors, colors used for the button, duh.
        '========================================================================
        '
        Private Function MakeButton(ByVal ButtonName As String, ByVal ButtonLabel As String, ByVal ButtonHref As String, ByVal ButtonWidth As String, ByVal ButtonColorBase As String, ByVal ButtonColorHilite As String, ByVal ButtonColorShadow As String, ByVal NewWindow As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("MakeButton")
            '
            MakeButton = ""
            MakeButton = MakeButton & MakeButtonFlat(ButtonName, ButtonLabel, ButtonHref, ButtonWidth, ButtonColorBase, ButtonColorHilite, ButtonColorShadow, NewWindow)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("MakeButton")
            '
        End Function
        '
        '========================================================================
        ' MakeButtonFlat
        '   Returns a Flat button string
        '   Button is a normal color, rollover changes background color only
        '========================================================================
        '
        Private Function MakeButtonFlat(ByVal ButtonName As String, ByVal ButtonLabel As String, ByVal ButtonHref As String, ByVal ButtonWidth As String, ByVal ButtonColorBase As String, ByVal ButtonColorHilite As String, ByVal ButtonColorShadow As String, ByVal NewWindow As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("MakeButtonFlat")
            '
            Dim IncludeWidth As Boolean
            '
            MakeButtonFlat = ""
            MakeButtonFlat = MakeButtonFlat & "<div" _
                & " ID=""" & ButtonName & """" _
                & " class=""ccAdminButton""" _
                & ">"
            '
            ' --- the IncludeWidth test
            '
            IncludeWidth = False
            If ButtonWidth <> "" Then
                If genericController.vbIsNumeric(ButtonWidth) Then
                    IncludeWidth = True
                End If
            End If
            '
            ' --- put guts in layer so Netscape can change colors (with mouseover and mouseout)
            '
            MakeButtonFlat = MakeButtonFlat & "<a" _
                & " href=""" & ButtonHref & """" _
                & " class=""ccAdminButton""" _
                & ""
            If NewWindow Then
                MakeButtonFlat = MakeButtonFlat & " target=""_blank"""
            End If
            MakeButtonFlat = MakeButtonFlat & ">"
            MakeButtonFlat = MakeButtonFlat & ButtonLabel & "</A>"
            If IncludeWidth Then
                MakeButtonFlat = MakeButtonFlat & "<br ><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""" & ButtonWidth & """ height=""1"" >"
            End If
            '
            ' --- close table
            '
            MakeButtonFlat = MakeButtonFlat & "</div>"
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("MakeButtonFlat")
            '
        End Function
        ''
        ''========================================================================
        '' GetMenuLeftMode()
        ''   Prints the menu section of the admin page
        ''========================================================================
        ''
        'Private Function deprecate_menu_getLeftMode() As String
        '    Dim returnString As String = ""
        '    Try
        '        '
        '        Const MenuEntryContentName = cnNavigatorEntries
        '        '
        '        Dim HeaderNameCurrent As String
        '        Dim MenuName As String
        '        Dim MenuID As Integer
        '        Dim MenuPage As String
        '        Dim MenuContentID As Integer
        '        Dim MenuNewWindow As Boolean
        '        Dim MenuItemCount As Integer
        '        Dim CS As Integer
        '        Dim Panel As String
        '        Dim ContentManagementList As New List(Of Integer)
        '        Dim IsAdminLocal As Boolean
        '        '
        '        ' Start the menu panel
        '        '
        '        Panel = "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">"
        '        Panel = Panel & "<tr><td width=""10""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""10"" height=""1"" ></td><td width=""100%""></td></tr>"
        '        '
        '        ' --- Load CCMenu
        '        '
        '        CS = GetMenuCSPointer("(ccMenuEntries.ParentID is null)or(ccMenuEntries.ParentID=0)", MenuEntryContentName)
        '        If cpCore.db.csOk(CS) Then
        '            IsAdminLocal = cpCore.doc.authContext.user.user_isAdmin
        '            If Not IsAdminLocal Then
        '                ContentManagementList.AddRange(cpCore.metaData.getEditableCdefIdList())
        '            End If
        '            HeaderNameCurrent = ""
        '            MenuItemCount = 0
        '            Do While cpCore.db.csOk(CS)
        '                MenuName = cpCore.db.cs_get(CS, "Name")
        '                MenuPage = cpCore.db.cs_get(CS, "LinkPage")
        '                MenuContentID = cpCore.db.cs_getInteger(CS, "ContentID")
        '                MenuNewWindow = cpCore.db.cs_getBoolean(CS, "NewWindow")
        '                MenuID = cpCore.db.cs_getInteger(CS, "ID")
        '                HeaderNameCurrent = MenuName
        '                '
        '                ' --- new header
        '                '
        '                If MenuItemCount <> 0 Then
        '                    Panel = Panel & "<tr><td colspan=""2""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""4"" ></td></tr>"
        '                    Panel = Panel & "<tr><td colspan=""2"" class=""ccPanelShadow""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td></tr>"
        '                    Panel = Panel & "<tr><td colspan=""2"" class=""ccPanelHilite""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td></tr>"
        '                    Panel = Panel & "<tr><td colspan=""2""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""4"" ></td></tr>"
        '                End If
        '                Panel = Panel & "<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>" & MenuName & "</b></span></td></tr>"
        '                MenuItemCount = MenuItemCount + 1
        '                Panel = Panel & deprecate_menu_getLeftModeBranch(MenuID, "", ContentManagementList, IsAdminLocal, MenuEntryContentName)
        '                Call cpCore.db.csGoNext(CS)
        '            Loop
        '        End If
        '        Call cpCore.db.csClose(CS)
        '        '
        '        ' Close the menu panel
        '        '
        '        Panel = Panel & "<tr><td width=""10""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""10"" height=""1"" ></td><td width=""1""></td><td width=""100%""></td><td width=""1""></td></tr>"
        '        Panel = Panel & "</table>"
        '        deprecate_menu_getLeftMode = cpcore.htmldoc.main_GetPanel(Panel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "150", 10)
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndContinue(ex) : Throw
        '    End Try
        '    Return returnString
        'End Function
        '        '
        '        '========================================================================
        '        ' GetMenuLeftModeBranch()
        '        '   Prints the menu section of the admin page
        '        '========================================================================
        '        '
        '        Private Function deprecate_menu_getLeftModeBranch(ByVal ParentID As Integer, ByVal ParentHeaderName As String, ByVal ContentManagementList As List(Of Integer), ByVal IsAdminLocal As Boolean, ByVal MenuEntryContentName As String) As String

        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetMenuLeftModeBranch")
        '            '
        '            Dim HeaderName As String
        '            Dim HeaderNameCurrent As String
        '            Dim MenuName As String
        '            Dim MenuID As Integer
        '            Dim MenuPage As String
        '            Dim MenuContentID As Integer
        '            Dim MenuNewWindow As Boolean
        '            Dim ButtonObject As String
        '            Dim MenuItemCount As Integer
        '            Dim SQL As String
        '            Dim CS As Integer
        '            Dim ImageID As Integer
        '            Dim ButtonGuts As String = ""
        '            Dim ButtonHref As String
        '            Dim ParentIDCurrent As Integer
        '            Dim MenuNameDisplay As String
        '            '
        '            ' --- Load CCMenu
        '            '
        '            CS = GetMenuCSPointer("(ccMenuEntries.ParentID=" & ParentID & ")", MenuEntryContentName)
        '            'SQL = GetMenuSQLNew("(ccMenuEntries.ParentID=" & parentid & ")")
        '            'CS = cpCore.app_openCsSql_Rev_Internal("Default", SQL)
        '            If cpCore.db.csOk(CS) Then
        '                HeaderNameCurrent = ""
        '                MenuItemCount = 0
        '                Do While cpCore.db.csOk(CS)
        '                    MenuName = cpCore.db.cs_get(CS, "Name")
        '                    MenuPage = cpCore.db.cs_get(CS, "LinkPage")
        '                    MenuContentID = cpCore.db.cs_getInteger(CS, "ContentID")
        '                    MenuNewWindow = cpCore.db.cs_getBoolean(CS, "NewWindow")
        '                    MenuID = cpCore.db.cs_getInteger(CS, "ID")
        '                    If ParentHeaderName = "" Then
        '                        MenuNameDisplay = MenuName
        '                    Else
        '                        MenuNameDisplay = ParentHeaderName & ":" & MenuName
        '                    End If
        '                    If (IsAdminLocal) Or ((MenuPage <> "") Or ((MenuContentID > 0) And (ContentManagementList.Contains(MenuContentID)))) Then
        '                        'If ((MenuPage <> "") Or (MenuContentID > 0)) Then
        '                        '
        '                        ' output the button
        '                        '
        '                        If MenuName = "" Then
        '                            MenuName = "[Link]"
        '                        End If
        '                        If MenuPage = "" Then
        '                            MenuPage = cpCore.siteProperties.serverPageDefault
        '                        End If
        '                        If MenuContentID > 0 Then
        '                            MenuPage = modifyLinkQuery(MenuPage, "cid", CStr(MenuContentID), True)
        '                            'MenuPage = MenuPage & "&cid=" & MenuContentID
        '                        End If
        '                        ButtonHref = MenuPage
        '                        If MenuNewWindow Then
        '                            ButtonGuts = ButtonGuts & " target=""_blank"""
        '                        End If
        '                        ButtonGuts = SpanClassAdminNormal & MenuNameDisplay & "</span>"
        '                        ' 9-28-02 ButtonGuts = SpanClassAdminNormal & "<nobr>" & MenuNameDisplay & "</nobr></span>"
        '                        ButtonObject = "Button" & ButtonObjectCount
        '                        ButtonObjectCount = ButtonObjectCount + 1
        '                        deprecate_menu_getLeftModeBranch = deprecate_menu_getLeftModeBranch & "<tr><td></td><td>" & MakeButton(ButtonObject, ButtonGuts, ButtonHref, "150", "ccPanel", "ccPanelHilite", "ccPanelShadow", MenuNewWindow) & "</td></tr>"
        '                    End If
        '                    MenuItemCount = MenuItemCount + 1
        '                    deprecate_menu_getLeftModeBranch = deprecate_menu_getLeftModeBranch & deprecate_menu_getLeftModeBranch(MenuID, MenuNameDisplay, ContentManagementList, IsAdminLocal, MenuEntryContentName)
        '                    Call cpCore.db.csGoNext(CS)
        '                Loop
        '            End If
        '            Call cpCore.db.csClose(CS)
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call handleLegacyClassError3("GetMenuLeftModeBranch")
        '            '
        '        End Function
        '        '
        '        '========================================================================
        '        ' GetMenuLeftMode()
        '        '   Prints the menu section of the admin page
        '        '========================================================================
        '        '
        '        Private Function menu_getLeftModeOld(ByVal MenuEntryContentName As String) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetMenuLeftModeOld")
        '            '
        '            Dim HeaderName As String
        '            Dim HeaderNameCurrent As String
        '            Dim MenuName As String
        '            Dim MenuID As Integer
        '            Dim MenuPage As String
        '            Dim MenuContentID As Integer
        '            Dim MenuNewWindow As Boolean
        '            Dim ButtonObject As String
        '            Dim MenuItemCount As Integer
        '            Dim SQL As String
        '            Dim CS As Integer
        '            Dim ImageID As Integer
        '            Dim Panel As String
        '            Dim ButtonGuts As String
        '            Dim ButtonHref As String
        '            Dim ParentID As Integer
        '            Dim ParentIDCurrent As Integer
        '            '
        '            ' --- Left Menu Mode
        '            '
        '            If AdminMenuModeID = AdminMenuModeLeft Then
        '                CS = GetMenuCSPointer("", MenuEntryContentName)
        '                'SQL = GetMenuSQLNew()
        '                ''
        '                '' --- Load CCMenu
        '                ''
        '                'CS = cpCore.app_openCsSql_Rev_Internal("Default", SQL)
        '                If cpCore.db.csOk(CS) Then
        '                    HeaderNameCurrent = ""
        '                    MenuItemCount = 0
        '                    Panel = "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">"
        '                    Panel = Panel & "<tr><td width=""10""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""10"" height=""1"" ></td><td width=""100%""></td></tr>"
        '                    Do While cpCore.db.csOk(CS)
        '                        ParentID = cpCore.db.cs_getInteger(CS, "ParentID")
        '                        'HeaderName = cpCore.app.cs_get(CS, "HeaderName")
        '                        MenuName = cpCore.db.cs_get(CS, "Name")
        '                        MenuPage = cpCore.db.cs_get(CS, "LinkPage")
        '                        MenuContentID = cpCore.db.cs_getInteger(CS, "ContentID")
        '                        MenuNewWindow = cpCore.db.cs_getBoolean(CS, "NewWindow")
        '                        MenuID = cpCore.db.cs_getInteger(CS, "ID")
        '                        '
        '                        ' --- draw menu line
        '                        '
        '                        If ParentID = 0 Then
        '                            HeaderNameCurrent = MenuName
        '                            '
        '                            ' --- new header
        '                            '
        '                            'cpCore.writeAltBufferComment ("Menu new header")
        '                            If MenuItemCount <> 0 Then
        '                                Panel = Panel & "<tr><td colspan=""2""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""4"" ></td></tr>"
        '                                Panel = Panel & "<tr><td colspan=""2"" class=""ccPanelShadow""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td></tr>"
        '                                Panel = Panel & "<tr><td colspan=""2"" class=""ccPanelHilite""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td></tr>"
        '                                Panel = Panel & "<tr><td colspan=""2""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""4"" ></td></tr>"
        '                            End If
        '                            Panel = Panel & "<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>" & MenuName & "</b></span></td></tr>"
        '                        End If
        '                        If ((MenuPage <> "") Or (MenuContentID > 0)) Then
        '                            '
        '                            ' output the button
        '                            '
        '                            If MenuName = "" Then
        '                                MenuName = "[Link]"
        '                            End If
        '                            If MenuPage = "" Then
        '                                MenuPage = cpCore.siteProperties.serverPageDefault
        '                            End If
        '                            If genericController.vbInstr(MenuPage, "?") = 0 Then
        '                                MenuPage = MenuPage & "?s=0"
        '                                'Else
        '                                '    MenuPage = MenuPage
        '                            End If
        '                            If MenuContentID > 0 Then
        '                                MenuPage = MenuPage & "&cid=" & MenuContentID
        '                            End If
        '                            ButtonHref = MenuPage
        '                            If MenuNewWindow Then
        '                                ' ButtonGuts = ButtonGuts & " target=""_blank"""
        '                            End If
        '                            ButtonGuts = SpanClassAdminNormal & MenuName & "</span>"
        '                            ButtonObject = "Button" & ButtonObjectCount
        '                            ButtonObjectCount = ButtonObjectCount + 1
        '                            Panel = Panel & "<tr><td></td><td>" & MakeButton(ButtonObject, ButtonGuts, ButtonHref, "150", "ccPanel", "ccPanelHilite", "ccPanelShadow", MenuNewWindow) & "</td></tr>"
        '                        End If
        '                        MenuItemCount = MenuItemCount + 1
        '                        Call cpCore.db.csGoNext(CS)
        '                    Loop
        '                    Panel = Panel & "<tr><td width=""10""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""10"" height=""1"" ></td><td width=""1""></td><td width=""100%""></td><td width=""1""></td></tr>"
        '                    Panel = Panel & "</table>"
        '                    menu_getLeftModeOld = cpcore.htmldoc.main_GetPanel(Panel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "150", 10)
        '                End If
        '            End If
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call handleLegacyClassError3("GetMenuLeftMode")
        '            '
        '        End Function
        '
        '========================================================================
        ' GetForm_Top
        '   Prints the admin page before the content form window.
        '   After this, print the content window, then PrintFormBottom()
        '========================================================================
        '
        Private Function GetForm_Top(Optional ByVal BackgroundColor As String = "") As String
            Dim return_formTop As String = ""
            Try
                Const AdminNavigatorGuid = "{5168964F-B6D2-4E9F-A5A8-BB1CF908A2C9}"
                Dim AdminNavFull As String
                Dim Stream As New stringBuilderLegacyController
                Dim LeftSide As String
                Dim RightSide As String
                Dim QS As String
                Dim Adminui As New adminUIController(cpCore)
                '
                ' create the with-menu version
                '
                LeftSide = cpCore.siteProperties.getText("AdminHeaderHTML", "Contensive Administration Site")
                RightSide = cpCore.doc.profileStartTime & "&nbsp;"
                '
                ' AdminTabs
                '
                QS = cpCore.doc.refreshQueryString
                If allowAdminTabs Then
                    QS = genericController.ModifyQueryString(QS, "tabs", "0", True)
                    RightSide = RightSide & getActiveImage(cpCore.serverConfig.appConfig.adminRoute & "?" & QS, "Disable Tabs", "LibButtonNoTabs.GIF", "LibButtonNoTabsRev.GIF", "Disable Tabs", "16", "16", "", "", "")
                Else
                    QS = genericController.ModifyQueryString(QS, "tabs", "1", True)
                    RightSide = RightSide & getActiveImage(cpCore.serverConfig.appConfig.adminRoute & "?" & QS, "Enable Tabs", "LibButtonTabs.GIF", "LibButtonTabsRev.GIF", "Enable Tabs", "16", "16", "", "", "")
                End If
                '
                ' Menu Mode
                '
                QS = cpCore.doc.refreshQueryString
                If MenuDepth = 0 Then
                    RightSide = RightSide & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""16"" >"
                    If AdminMenuModeID = AdminMenuModeTop Then
                        QS = genericController.ModifyQueryString(QS, "mm", "1", True)
                        RightSide = RightSide & getActiveImage(cpCore.serverConfig.appConfig.adminRoute & "?" & QS, "Use Navigator", "LibButtonMenuTop.GIF", "LibButtonMenuTopOver.GIF", "Use Navigator", "16", "16", "", "", "")
                    Else
                        QS = genericController.ModifyQueryString(QS, "mm", "2", True)
                        RightSide = RightSide & getActiveImage(cpCore.serverConfig.appConfig.adminRoute & "?" & QS, "Use Dropdown Menus", "LibButtonMenuLeft.GIF", "LibButtonMenuLeftOver.GIF", "Use Dropdown Menus", "16", "16", "", "", "")
                    End If
                End If
                '
                ' Refresh Button
                '
                RightSide = RightSide & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""16"" >"
                RightSide = RightSide & getActiveImage(cpCore.serverConfig.appConfig.adminRoute & "?" & cpCore.doc.refreshQueryString, "Refresh", "LibButtonRefresh.GIF", "LibButtonRefreshOver.GIF", "Refresh", "16", "16", "", "", "")
                '
                ' Assemble header
                '
                Call Stream.Add(Adminui.GetHeader(LeftSide, RightSide))
                '
                ' Menuing
                '
                If ((MenuDepth = 0) And (AdminMenuModeID = AdminMenuModeTop)) Then
                    Call Stream.Add(GetMenuTopMode())
                End If
                '
                ' --- Rule to separate content
                '
                Stream.Add(cr & "<div style=""border-top:1px solid white;border-bottom:1px solid black;height:2px""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=1 height=1></div>")
                '
                ' --- Content Definition
                '
                AdminFormBottom = ""
                If Not ((MenuDepth = 0) And (AdminMenuModeID = AdminMenuModeLeft)) Then
                    '
                    ' #Content is full width, no Navigator
                    '
                    Stream.Add(cr & "<div id=""desktop"" class=""ccContentCon"">")
                    'Stream.Add( "<div id=""ccContentCon"">")
                    AdminFormBottom = AdminFormBottom & cr & "</div>"
                Else
                    '
                    ' -- Admin Navigator
                    AdminNavFull = cpCore.addon.execute(addonModel.create(cpCore, AdminNavigatorGuid), New BaseClasses.CPUtilsBaseClass.addonExecuteContext() With {.addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, .errorCaption = "Admin Navigator"})
                    'AdminNavFull = cpCore.addon.execute_legacy4(AdminNavigatorGuid)
                    Stream.Add("" _
                        & cr & "<table border=0 cellpadding=0 cellspacing=0><tr>" _
                        & cr & "<td class=""ccToolsCon"" valign=top>" _
                        & genericController.htmlIndent(AdminNavFull) _
                        & cr & "</td>" _
                        & cr & "<td id=""desktop"" class=""ccContentCon"" valign=top>")
                    AdminFormBottom = AdminFormBottom & "</td></tr></table>"
                End If
                '
                return_formTop = Stream.Text
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return return_formTop
        End Function
        '
        '========================================================================
        ' Create a string with an admin style button
        '========================================================================
        '
        Private Function getActiveImage(ByVal HRef As String, ByVal StatusText As String, ByVal Image As String, ByVal ImageOver As String, ByVal AltText As String, ByVal Width As String, ByVal Height As String, ByVal BGColor As String, ByVal BGColorOver As String, ByVal OnClick As String) As String
            Dim result As String = ""
            Try
                Dim ButtonObject As String = "Button" & ButtonObjectCount
                ButtonObjectCount = ButtonObjectCount + 1
                '
                ' ----- Output the button image
                '
                Dim Panel As String = ""
                'If BGColor <> "" Then
                '    Panel = Panel & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""" & Width & """ BGColor=""" & BGColor & """ align=""left""><tr><td>"
                '    End If
                If HRef <> "" Then
                    Panel = Panel & "<a href=""" & HRef & """ "
                    If OnClick <> "" Then
                        Panel = Panel & " onclick=""" & OnClick & """"
                    End If
                    Panel = Panel & " onmouseOver=""" _
                        & " document['" & ButtonObject & "'].imgRolln=document['" & ButtonObject & "'].src;" _
                        & " document['" & ButtonObject & "'].src=document['" & ButtonObject & "'].lowsrc;" _
                        & " window.status='" & StatusText & "';" _
                        & " return true;"""
                    Panel = Panel & " onmouseOut=""" _
                        & " document['" & ButtonObject & "'].src=document['" & ButtonObject & "'].imgRolln;" _
                        & " window.status='';" _
                        & " return true;"">"
                End If
                Panel = Panel & "<img" _
                    & " src=""/ccLib/images/" & Image & """" _
                    & " alt=""" & AltText & """" _
                    & " title=""" & AltText & """" _
                    & " id=""" & ButtonObject & """" _
                    & " name=""" & ButtonObject & """" _
                    & " lowsrc=""/ccLib/images/" & ImageOver & """" _
                    & " border=0" _
                    & " width=""" & Width & """" _
                    & " height=""" & Height & """ >"
                If HRef <> "" Then
                    Panel = Panel & "</A>"
                End If
                result = Panel
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        ''
        ''========================================================================
        ''   Preload an image, returns object
        ''========================================================================
        ''
        'Private Function PreloadImage(Image As String) As String
        '    On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.PreloadImage")
        '    '
        '    Dim ImageFound As Boolean
        '    Dim ImagePreloadPointer as integer
        '    '
        '    ImageFound = False
        '    If ImagePreloadCount > 0 Then
        '        For ImagePreloadPointer = 0 To ImagePreloadCount
        '            If ImagePreloads(0, ImagePreloadPointer) = Image Then
        '                ImageFound = True
        '                PreloadImage = ImagePreloads(0, ImagePreloadPointer)
        '                Exit For
        '                End If
        '            Next
        '        End If
        '    If Not ImageFound Then
        '        If ImagePreloadCount = 0 Then
        '            JavaScriptString = JavaScriptString & vbCrLf & "PreloadImages = new Array(); "
        '            End If
        '        '
        '        PreloadImage = "Image" & ImagePreloadCount
        '        ImagePreloads(0, ImagePreloadCount) = Image
        '        ImagePreloads(1, ImagePreloadCount) = PreloadImage
        '        ImagePreloadCount = ImagePreloadCount + 1
        '        '
        '        JavaScriptString = JavaScriptString & vbCrLf & "PreloadImages['" & PreloadImage & "'] = new Image(); "
        '        JavaScriptString = JavaScriptString & vbCrLf & "PreloadImages['" & PreloadImage & "'].src = '/ccLib/images/" & Image & "'; "
        '        End If
        '    '''Dim th as integer: Exit Function
        '    '
        '    ' ----- Error Trap
        '    '
        'ErrorTrap:
        '    Call HandleClassTrapErrorBubble("PreloadImage")
        '    '
        'End Function
        '
        '========================================================================
        ' Get sql for menu
        '   if MenuContentName is blank, it will select values from either cdef
        '========================================================================
        '
        Private Function GetMenuSQL(ByVal ParentCriteria As String, ByVal MenuContentName As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetMenuSQL")
            '
            Dim iParentCriteria As String
            Dim Criteria As String
            Dim SQL As String
            Dim ContentControlCriteria As String
            Dim SelectList As String
            Dim editableCdefIdList As List(Of Integer)
            '
            Criteria = "(Active<>0)"
            If MenuContentName <> "" Then
                'ContentControlCriteria = cpCore.csv_GetContentControlCriteria(MenuContentName)
                Criteria = Criteria & "AND" & Models.Complex.cdefModel.getContentControlCriteria(cpCore, MenuContentName)
            End If
            iParentCriteria = genericController.encodeEmptyText(ParentCriteria, "")
            If cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore) Then
                '
                ' ----- Developer
                '
            ElseIf cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                '
                ' ----- Administrator
                '
                Criteria = Criteria _
                    & "AND((DeveloperOnly is null)or(DeveloperOnly=0))" _
                    & "AND(ID in (" _
                    & " SELECT AllowedEntries.ID" _
                    & " FROM CCMenuEntries AllowedEntries LEFT JOIN ccContent ON AllowedEntries.ContentID = ccContent.ID" _
                    & " Where ((ccContent.Active<>0)And((ccContent.DeveloperOnly is null)or(ccContent.DeveloperOnly=0)))" _
                        & "OR(ccContent.ID Is Null)" _
                    & "))"
            Else
                '
                ' ----- Content Manager
                '
                Dim CMCriteria As String

                editableCdefIdList = Models.Complex.cdefModel.getEditableCdefIdList(cpCore)
                If editableCdefIdList.Count = 0 Then
                    CMCriteria = "(1=0)"
                ElseIf editableCdefIdList.Count = 1 Then
                    CMCriteria = "(ccContent.ID=" & editableCdefIdList(0) & ")"
                Else
                    CMCriteria = "(ccContent.ID in (" & String.Join(",", editableCdefIdList) & "))"
                End If

                Criteria = Criteria _
                    & "AND((DeveloperOnly is null)or(DeveloperOnly=0))" _
                    & "AND((AdminOnly is null)or(AdminOnly=0))" _
                    & "AND(ID in (" _
                    & " SELECT AllowedEntries.ID" _
                    & " FROM CCMenuEntries AllowedEntries LEFT JOIN ccContent ON AllowedEntries.ContentID = ccContent.ID" _
                    & " Where (" & CMCriteria & "and(ccContent.Active<>0)And((ccContent.DeveloperOnly is null)or(ccContent.DeveloperOnly=0))And((ccContent.AdminOnly is null)or(ccContent.AdminOnly=0)))" _
                        & "OR(ccContent.ID Is Null)" _
                    & "))"
            End If
            If iParentCriteria <> "" Then
                Criteria = "(" & iParentCriteria & ")AND" & Criteria
            End If
            SelectList = "ccMenuEntries.contentcontrolid, ccMenuEntries.Name, ccMenuEntries.ID, ccMenuEntries.LinkPage, ccMenuEntries.ContentID, ccMenuEntries.NewWindow, ccMenuEntries.ParentID, ccMenuEntries.AddonID, ccMenuEntries.NavIconType, ccMenuEntries.NavIconTitle, HelpAddonID,HelpCollectionID,0 as collectionid"
            GetMenuSQL = "select " & SelectList & " from ccMenuEntries where " & Criteria & " order by ccMenuEntries.Name"
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetMenuSQL")
            '
        End Function
        '
        '========================================================================
        ' Get sql for menu
        '========================================================================
        '
        Private Function GetMenuCSPointer(ByVal ParentCriteria As String) As Integer
            '
            Dim iParentCriteria As String
            iParentCriteria = genericController.encodeEmptyText(ParentCriteria, "")
            If iParentCriteria <> "" Then
                iParentCriteria = "(" & iParentCriteria & ")"
            End If
            GetMenuCSPointer = cpCore.db.csOpenSql_rev("default", GetMenuSQL(iParentCriteria, cnNavigatorEntries))
        End Function
        '
        '========================================================================
        ' Get Menu Link
        '========================================================================
        '
        Private Function GetMenuLink(ByVal LinkPage As String, ByVal LinkCID As Integer) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetMenuLink")
            '
            Dim ContentID As Integer
            '
            If LinkPage <> "" Or LinkCID <> 0 Then
                GetMenuLink = LinkPage
                If GetMenuLink <> "" Then
                    If Mid(GetMenuLink, 1, 1) = "?" Or Mid(GetMenuLink, 1, 1) = "#" Then
                        GetMenuLink = "/" & cpCore.serverConfig.appConfig.adminRoute & GetMenuLink
                    End If
                Else
                    GetMenuLink = "/" & cpCore.serverConfig.appConfig.adminRoute
                End If
                ContentID = genericController.EncodeInteger(LinkCID)
                If ContentID <> 0 Then
                    GetMenuLink = genericController.modifyLinkQuery(GetMenuLink, "cid", CStr(ContentID), True)
                End If
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetMenuLink")
            '
        End Function
        '
        '
        '
        Private Sub ProcessForms(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass)
            On Error GoTo ErrorTrap
            'Dim th as integer
            'th = profileLogAdminMethodEnter("ProcessForms")
            '
            'Dim innovaEditor As innovaEditorAddonClassFPO
            Dim StyleSN As Integer
            Dim ContentID As Integer
            Dim IndexConfig As indexConfigClass
            Dim CS As Integer
            Dim EditorStyleRulesFilename As String
            '
            If AdminSourceForm <> 0 Then
                Select Case AdminSourceForm
                    Case AdminFormReports
                        '
                        ' Reports form cancel button
                        '
                        Select Case AdminButton
                            Case ButtonCancel
                                AdminAction = AdminActionNop
                                AdminForm = AdminFormRoot
                        End Select
                    Case AdminFormQuickStats
                        Select Case AdminButton
                            Case ButtonCancel
                                AdminAction = AdminActionNop
                                AdminForm = AdminFormRoot
                        End Select
                    Case AdminFormPublishing
                        '
                        ' Publish Form
                        '
                        Select Case AdminButton
                            Case ButtonCancel
                                AdminAction = AdminActionNop
                                AdminForm = AdminFormRoot
                        End Select
                    Case AdminFormIndex
                        Select Case AdminButton
                            Case ButtonCancel
                                AdminAction = AdminActionNop
                                AdminForm = AdminFormRoot
                                adminContent.Id = 0
                            Case ButtonClose
                                AdminAction = AdminActionNop
                                AdminForm = AdminFormRoot
                                adminContent.Id = 0
                            Case ButtonAdd
                                AdminAction = AdminActionNop
                                AdminForm = AdminFormEdit
                            Case ButtonFind
                                AdminAction = AdminActionFind
                                AdminForm = AdminSourceForm
                            Case ButtonFirst
                                RecordTop = 0
                                AdminForm = AdminSourceForm
                            Case ButtonPrevious
                                RecordTop = RecordTop - RecordsPerPage
                                If RecordTop < 0 Then
                                    RecordTop = 0
                                End If
                                AdminAction = AdminActionNop
                                AdminForm = AdminSourceForm
                            Case ButtonNext
                                AdminAction = AdminActionNext
                                AdminForm = AdminSourceForm
                            Case ButtonDelete
                                AdminAction = AdminActionDeleteRows
                                AdminForm = AdminSourceForm
                        End Select
                        ' end case
                    Case AdminFormEdit
                        '
                        ' Edit Form
                        '
                        Select Case AdminButton
                            Case ButtonRefresh
                                '
                                ' this is a test operation. need this so the user can set editor preferences without saving the record
                                '   during refresh, the edit page is redrawn just was it was, but no save
                                '
                                AdminAction = AdminActionEditRefresh
                                AdminForm = AdminFormEdit
                            Case ButtonMarkReviewed
                                AdminAction = AdminActionMarkReviewed
                                AdminForm = GetForm_Close(MenuDepth, adminContent.Name, editRecord.id)
                            Case ButtonSaveandInvalidateCache
                                AdminAction = AdminActionReloadCDef
                                AdminForm = AdminFormEdit
                            Case ButtonDelete, ButtonDeletePage, ButtonDeletePerson, ButtonDeleteRecord, ButtonDeleteEmail
                                AdminAction = AdminActionDelete
                                AdminForm = GetForm_Close(MenuDepth, adminContent.Name, editRecord.id)
                                '                Case ButtonSetHTMLEdit
                                '                    AdminAction = AdminActionSetHTMLEdit
                                '                Case ButtonSetTextEdit
                                '                    AdminAction = AdminActionSetTextEdit
                            Case ButtonSave
                                AdminAction = AdminActionSave
                                AdminForm = AdminFormEdit
                            Case ButtonSaveAddNew
                                AdminAction = AdminActionSaveAddNew
                                AdminForm = AdminFormEdit
                            Case ButtonOK
                                AdminAction = AdminActionSave
                                AdminForm = GetForm_Close(MenuDepth, adminContent.Name, editRecord.id)
                            Case ButtonCancel
                                AdminAction = AdminActionNop
                                AdminForm = GetForm_Close(MenuDepth, adminContent.Name, editRecord.id)
                            Case ButtonSend
                                '
                                ' Send a Group Email
                                '
                                AdminAction = AdminActionSendEmail
                                AdminForm = AdminFormEdit
                            Case ButtonActivate
                                '
                                ' Activate (submit) a conditional Email
                                '
                                AdminAction = AdminActionActivateEmail
                                AdminForm = AdminFormEdit
                            Case ButtonDeactivate
                                '
                                ' Deactivate (clear submit) a conditional Email
                                '
                                AdminAction = AdminActionDeactivateEmail
                                AdminForm = AdminFormEdit
                            Case ButtonSendTest
                                '
                                ' Test an Email (Group, System, or Conditional)
                                '
                                AdminAction = AdminActionSendEmailTest
                                AdminForm = AdminFormEdit
                                '                Case ButtonSpellCheck
                                '                    SpellCheckRequest = True
                                '                    AdminAction = AdminActionSave
                                '                    AdminForm = AdminFormEdit
                            Case ButtonCreateDuplicate
                                '
                                ' Create a Duplicate record (for email)
                                '
                                AdminAction = AdminActionDuplicate
                                AdminForm = AdminFormEdit
                        End Select
                    Case AdminFormStyleEditor
                        '
                        ' Process actions
                        '
                        Select Case AdminButton
                            Case ButtonSave, ButtonOK
                                '
                                Call cpCore.siteProperties.setProperty("Allow CSS Reset", cpCore.docProperties.getBoolean(RequestNameAllowCSSReset))
                                Call cpCore.cdnFiles.saveFile(DynamicStylesFilename, cpCore.docProperties.getText("StyleEditor"))
                                If cpCore.docProperties.getBoolean(RequestNameInlineStyles) Then
                                    '
                                    ' Inline Styles
                                    '
                                    Call cpCore.siteProperties.setProperty("StylesheetSerialNumber", "0")
                                Else
                                    ' mark to rebuild next fetch
                                    Call cpCore.siteProperties.setProperty("StylesheetSerialNumber", "-1")
                                    ''
                                    '' Linked Styles
                                    '' Bump the Style Serial Number so next fetch is not cached
                                    ''
                                    'StyleSN = genericController.EncodeInteger(cpCore.main_GetSiteProperty2("StylesheetSerialNumber", "0"))
                                    'StyleSN = StyleSN + 1
                                    'Call cpCore.app.setSiteProperty("StylesheetSerialNumber", genericController.encodeText(StyleSN))
                                    ''
                                    '' Save new public stylesheet
                                    ''
                                    '' 11/24/3009 - style sheet processing deprecated
                                    'Call cpCore.app.virtualFiles.SaveFile("templates\Public" & StyleSN & ".css", cpCore.main_GetStyleSheet)
                                    ''Call cpCore.app.virtualFiles.SaveFile("templates\Public" & StyleSN & ".css", cpCore.main_GetStyleSheetProcessed)
                                    'Call cpCore.app.virtualFiles.SaveFile("templates\Admin" & StyleSN & ".css", cpCore.main_GetStyleSheetDefault)
                                End If
                                '
                                ' delete all templateid based editorstylerule files, build on-demand
                                '
                                EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", "0", 1, 99, vbTextCompare)
                                Call cpCore.cdnFiles.deleteFile(EditorStyleRulesFilename)
                                '
                                CS = cpCore.db.csOpenSql_rev("default", "select id from cctemplates")
                                Do While cpCore.db.csOk(CS)
                                    EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", cpCore.db.csGet(CS, "ID"), 1, 99, vbTextCompare)
                                    Call cpCore.cdnFiles.deleteFile(EditorStyleRulesFilename)
                                    Call cpCore.db.csGoNext(CS)
                                Loop
                                Call cpCore.db.csClose(CS)
                        End Select
                        '
                        ' Process redirects
                        '
                        Select Case AdminButton
                            Case ButtonCancel, ButtonOK
                                AdminForm = AdminFormRoot
                        End Select
                    Case Else
                        ' end case
                End Select
            End If
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("ProcessForms")
            '
        End Sub
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Function GetForm_EditTitle(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_EditTitle")
            '
            If (editRecord.id = 0) Then
                GetForm_EditTitle = "Add an entry to " & editRecord.contentControlId_Name & TitleExtension
            Else
                GetForm_EditTitle = "Editing Record " & editRecord.id & " in " & editRecord.contentControlId_Name & " " & TitleExtension
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_EditTitle")
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Function GetForm_EditTitleBar(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_EditTitleBar")
            '
            Dim Adminui As New adminUIController(cpCore)
            '
            GetForm_EditTitleBar = Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), "")
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_EditTitleBar")
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Function GetForm_EditFormStart(AdminFormID As Integer) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_EditFormStart")
            '
            Dim WhereCount As Integer
            Dim s As String
            Dim saveEmptyFieldListScript As String
            '
            ' --- output required hidden fields to preserve values through form submission
            '
            '
            'saveEmptyFieldListScript = "" _
            '    & "function adminEditSaveEmptyFields(){" _
            '        & "var e=document.getElementById('" &  "FormEmptyFieldList');" _
            '        & "var c=document.getElementsByTagName('input');" _
            '        & "for (i=0;i<c.length;i++){" _
            '            & "if(c[i].type=='checkbox'){" _
            '                & "if(c[i].checked==false){e.value+=c[i].name+','}" _
            '            & "} else if(c[i].type=='radio'){" _
            '                & "if(c[i].checked==false){e.value+=c[i].name+','}" _
            '            & "} else if(c[i].value==''){" _
            '                & "e.value+=c[i].name+','" _
            '            & "}" _
            '        & "}" _
            '        & "c=document.getElementsByTagName('select');" _
            '        & "for (i=0;i<c.length;i++){" _
            '            & "if(c[i].value==''){e.value+=c[i].name+','}" _
            '        & "}" _
            '    & "}" _
            '    & ""
            'Call cpCore.htmldoc.main_AddHeadScriptCode(saveEmptyFieldListScript, "Edit Page")
            '
            'saveEmptyFieldListScript = "" _
            '    & "cj.admin.saveEmptyFieldList('" &  "FormEmptyFieldList');" _
            '    & ""
            '    saveEmptyFieldListScript = "" _
            '        & "if(!docLoaded){" _
            '            & "alert('This page has not loaded completed. Please wait for the page to load before submitting the form. If the page has loaded, there may have been an error. Please refresh the page.')" _
            '            & ";return false" _
            '        & "}else{" _
            '            & "adminEditSaveEmptyFields();" _
            '        & "}" _
            '        & ""
            '
            Call cpCore.html.addScriptCode_head("var docLoaded=false", "Form loader")
            Call cpCore.html.addScriptCode_onLoad("docLoaded=true;", "Form loader")
            s = cpCore.html.html_GetUploadFormStart()
            s = genericController.vbReplace(s, ">", " onSubmit=""cj.admin.saveEmptyFieldList('" & "FormEmptyFieldList');"">")
            s = genericController.vbReplace(s, ">", " autocomplete=""off"">")
            s = genericController.vbReplace(s, ">", " id=""adminEditForm"">")
            s = s & vbCrLf & "<input TYPE=""hidden"" NAME=""" & RequestNameAdminSourceForm & """ VALUE=""" & AdminFormID.ToString & """>"
            s = s & vbCrLf & "<input TYPE=""hidden"" NAME=""" & RequestNameTitleExtension & """ VALUE=""" & TitleExtension & """>"
            s = s & vbCrLf & "<input TYPE=""hidden"" NAME=""" & RequestNameAdminDepth & """ VALUE=""" & MenuDepth & """>"
            s = s & vbCrLf & "<input TYPE=""hidden"" NAME=""" & "FormEmptyFieldList"" ID=""" & "FormEmptyFieldList"" VALUE="","">"
            If False Then
                '
                ' already added to refresh query string
                '
                If WherePairCount > 0 Then
                    For WhereCount = 0 To WherePairCount - 1
                        s = s & vbCrLf & "<input TYPE=""hidden"" NAME=""wl" & WhereCount & """ VALUE=""" & WherePair(0, WhereCount) & """><input TYPE=""hidden"" NAME=""wr" & WhereCount & """ VALUE=""" & WherePair(1, WhereCount) & """>"
                    Next
                End If
            End If
            '
            GetForm_EditFormStart = s
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_EditFormStart")
            '
        End Function
        '
        ' true if the field is a visible user field (can display on edit form)
        '
        Private Function IsVisibleUserField(AdminOnly As Boolean, DeveloperOnly As Boolean, Active As Boolean, Authorable As Boolean, Name As String, TableName As String) As Boolean
            'Private Function IsVisibleUserField( Field as CDefFieldClass, AdminOnly As Boolean, DeveloperOnly As Boolean, Active As Boolean, Authorable As Boolean) As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("IsVisibleUserField")
            '
            Dim HasEditRights As Boolean
            '
            IsVisibleUserField = False
            If (LCase(TableName) = "ccpagecontent") And (LCase(Name) = "linkalias") Then
                '
                ' ccpagecontent.linkalias is a control field that is not in control tab
                '
            Else
                Select Case genericController.vbUCase(Name)
                    Case "ACTIVE", "ID", "CONTENTCONTROLID", "CREATEDBY", "DATEADDED", "MODIFIEDBY", "MODIFIEDDATE", "CREATEKEY", "CCGUID"
                        '
                        ' ----- control fields are not editable user fields
                        '
                    Case Else
                        '
                        ' ----- test access
                        '
                        HasEditRights = True
                        If AdminOnly Or DeveloperOnly Then
                            '
                            ' field has some kind of restriction
                            '
                            If Not cpCore.doc.authContext.user.Developer Then
                                If Not cpCore.doc.authContext.user.Admin Then
                                    '
                                    ' you are not admin
                                    '
                                    HasEditRights = False
                                ElseIf DeveloperOnly Then
                                    '
                                    ' you are admin, and the record is developer
                                    '
                                    HasEditRights = False
                                End If
                            End If
                        End If
                        If (HasEditRights) And (Active) And (Authorable) Then
                            IsVisibleUserField = True
                        End If
                End Select
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("IsVisibleUserField")
            '
        End Function
        '
        '=============================================================================================
        ' true if the field is an editable user field (can edit on edit form and save to database)
        '=============================================================================================
        '
        Private Function IsFieldEditable(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, Field As Models.Complex.CDefFieldModel) As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("IsFieldEditable")
            '
            With Field
                IsFieldEditable = IsVisibleUserField(.adminOnly, .developerOnly, .active, .authorable, .nameLc, adminContent.ContentTableName) And (Not .ReadOnly) And (Not .NotEditable)
            End With
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("IsFieldEditable")
            '
        End Function
        '
        '=============================================================================================
        '   Get
        '=============================================================================================
        '
        Private Function GetForm_Close(MenuDepth As Integer, ContentName As String, RecordID As Integer) As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Close")
            '
            If MenuDepth > 0 Then
                GetForm_Close = AdminFormClose
            Else
                GetForm_Close = AdminFormIndex
            End If
            'Call cpCore.main_ClearAuthoringEditLock(ContentName, RecordID)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Close")
            '
        End Function
        '
        '=============================================================================================
        '
        '=============================================================================================
        '
        Private Sub ProcessActionSave(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, UseContentWatchLink As Boolean)
            Try
                Dim EditorStyleRulesFilename As String
                '
                If (True) Then
                    '
                    '
                    '
                    If Not (cpCore.doc.debug_iUserError <> "") Then
                        Select Case genericController.vbUCase(adminContent.ContentTableName)
                            Case genericController.vbUCase("ccMembers")
                                '
                                '
                                '

                                Call SaveEditRecord(adminContent, editRecord)
                                Call SaveMemberRules(editRecord.id)
                            'Call SaveTopicRules
                            Case "CCEMAIL"
                                '
                                '
                                '
                                Call SaveEditRecord(adminContent, editRecord)
                                ' NO - ignore wwwroot styles, and create it on the fly during send
                                'If cpCore.main_GetSiteProperty2("BuildVersion") >= "3.3.291" Then
                                '    Call cpCore.app.executeSql( "update ccEmail set InlineStyles=" & encodeSQLText(cpCore.main_GetStyleSheetProcessed) & " where ID=" & EditRecord.ID)
                                'End If
                                Call cpCore.html.main_ProcessCheckList("EmailGroups", "Group Email", genericController.encodeText(editRecord.id), "Groups", "Email Groups", "EmailID", "GroupID")
                                Call cpCore.html.main_ProcessCheckList("EmailTopics", "Group Email", genericController.encodeText(editRecord.id), "Topics", "Email Topics", "EmailID", "TopicID")
                            Case "CCCONTENT"
                                '
                                '
                                '
                                Call SaveEditRecord(adminContent, editRecord)
                                Call LoadAndSaveGroupRules(editRecord)
                            Case "CCPAGECONTENT"
                                '
                                '
                                '
                                Call SaveEditRecord(adminContent, editRecord)
                                Call LoadContentTrackingDataBase(adminContent, editRecord)
                                Call LoadContentTrackingResponse(adminContent, editRecord)
                                'Call LoadAndSaveMetaContent()
                                Call SaveLinkAlias(adminContent, editRecord)
                                'Call SaveTopicRules
                                Call SaveContentTracking(adminContent, editRecord)
                            Case "CCLIBRARYFOLDERS"
                                '
                                '
                                '
                                Call SaveEditRecord(adminContent, editRecord)
                                Call LoadContentTrackingDataBase(adminContent, editRecord)
                                Call LoadContentTrackingResponse(adminContent, editRecord)
                                'Call LoadAndSaveCalendarEvents
                                'Call LoadAndSaveMetaContent()
                                Call cpCore.html.main_ProcessCheckList("LibraryFolderRules", adminContent.Name, genericController.encodeText(editRecord.id), "Groups", "Library Folder Rules", "FolderID", "GroupID")
                                'call SaveTopicRules
                                Call SaveContentTracking(adminContent, editRecord)
                            Case "CCSETUP"
                                '
                                ' Site Properties
                                '
                                Call SaveEditRecord(adminContent, editRecord)
                                If (LCase(editRecord.nameLc) = "allowlinkalias") Then
                                    If (cpCore.siteProperties.getBoolean("AllowLinkAlias")) Then
                                        If False Then
                                            '
                                            ' Must upgrade
                                            '
                                            Call cpCore.siteProperties.setProperty("AllowLinkAlias", "0")
                                            Call errorController.error_AddUserError(cpCore, "Link Alias entries for your pages can not be created because your site database needs to be upgraded.")
                                        Else
                                            '
                                            ' Verify all page content records have a link alias
                                            '
                                            Call TurnOnLinkAlias(UseContentWatchLink)
                                        End If
                                    End If
                                End If
                            Case genericController.vbUCase("ccGroups")
                                'Case "CCGROUPS"
                                '
                                '
                                '
                                Call SaveEditRecord(adminContent, editRecord)
                                Call LoadContentTrackingDataBase(adminContent, editRecord)
                                Call LoadContentTrackingResponse(adminContent, editRecord)
                                Call LoadAndSaveContentGroupRules(editRecord.id)
                                'Call LoadAndSaveCalendarEvents
                                'Call LoadAndSaveMetaContent()
                                'call SaveTopicRules
                                Call SaveContentTracking(adminContent, editRecord)
                            'Dim EditorStyleRulesFilename As String
                            Case "CCTEMPLATES"
                                '
                                ' save and clear editorstylerules for this template
                                '
                                Call SaveEditRecord(adminContent, editRecord)
                                Call LoadContentTrackingDataBase(adminContent, editRecord)
                                Call LoadContentTrackingResponse(adminContent, editRecord)
                                'Call LoadAndSaveCalendarEvents
                                'Call LoadAndSaveMetaContent()
                                'call SaveTopicRules
                                Call SaveContentTracking(adminContent, editRecord)
                                '
                                EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", editRecord.id.ToString, 1, 99, vbTextCompare)
                                Call cpCore.privateFiles.deleteFile(EditorStyleRulesFilename)
                                'Case "CCSHAREDSTYLES"
                                '    '
                                '    ' save and clear editorstylerules for any template
                                '    '
                                '    Call SaveEditRecord(adminContent, editRecord)
                                '    Call LoadContentTrackingDataBase(adminContent, editRecord)
                                '    Call LoadContentTrackingResponse(adminContent, editRecord)
                                '    'Call LoadAndSaveCalendarEvents
                                '    Call LoadAndSaveMetaContent()
                                '    'call SaveTopicRules
                                '    Call SaveContentTracking(adminContent, editRecord)
                                '    '
                                '    EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", "0", 1, 99, vbTextCompare)
                                '    Call cpCore.cdnFiles.deleteFile(EditorStyleRulesFilename)
                                '    '
                                '    CS = cpCore.db.cs_openCsSql_rev("default", "select id from cctemplates")
                                '    Do While cpCore.db.cs_ok(CS)
                                '        EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", cpCore.db.cs_get(CS, "ID"), 1, 99, vbTextCompare)
                                '        Call cpCore.cdnFiles.deleteFile(EditorStyleRulesFilename)
                                '        Call cpCore.db.cs_goNext(CS)
                                '    Loop
                                '    Call cpCore.db.cs_Close(CS)


                            Case Else
                                '
                                '
                                '
                                Call SaveEditRecord(adminContent, editRecord)
                                Call LoadContentTrackingDataBase(adminContent, editRecord)
                                Call LoadContentTrackingResponse(adminContent, editRecord)
                                'Call LoadAndSaveCalendarEvents
                                'Call LoadAndSaveMetaContent()
                                'call SaveTopicRules
                                Call SaveContentTracking(adminContent, editRecord)
                        End Select
                    End If
                End If
                '
                ' If the content supports datereviewed, mark it
                '
                If (cpCore.doc.debug_iUserError <> "") Then
                    AdminForm = AdminSourceForm
                End If
                AdminAction = AdminActionNop ' convert so action can be used in as a refresh
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
    End Class
End Namespace
