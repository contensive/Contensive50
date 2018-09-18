

using Controllers;

using System.Xml;
using Contensive.Core;
using Models.Entity;
// 

namespace Contensive.Addons.AdminSite {
    
    public partial class getAdminSiteClass : Contensive.BaseClasses.AddonBaseClass {
        
        // 
        // 
        // ========================================================================
        //    Print the root form
        // ========================================================================
        // 
        private string GetForm_Root() {
            string returnHtml = "";
            try {
                int CS;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                int addonId;
                string AddonIDText;
                // 
                //  This is really messy -- there must be a better way
                // 
                addonId = 0;
                if ((cpCore.doc.authContext.visit.id == cpCore.docProperties.getInteger(RequestNameDashboardReset))) {
                    // $$$$$ cache this
                    CS = cpCore.db.csOpen(cnAddons, ("ccguid=" + cpCore.db.encodeSQLText(addonGuidDashboard)));
                    if (cpCore.db.csOk(CS)) {
                        addonId = cpCore.db.csGetInteger(CS, "id");
                        cpCore.siteProperties.setProperty("AdminRootAddonID", genericController.encodeText(addonId));
                    }
                    
                    cpCore.db.csClose(CS);
                }
                
                if ((addonId == 0)) {
                    // 
                    //  Get AdminRootAddon
                    // 
                    AddonIDText = cpCore.siteProperties.getText("AdminRootAddonID", "");
                    if ((AddonIDText == "")) {
                        // 
                        //  the desktop is likely unset, auto set it to dashboard
                        // 
                        addonId = -1;
                    }
                    else if ((AddonIDText == "0")) {
                        // 
                        //  the desktop has been set to none - go with default desktop
                        // 
                        addonId = 0;
                    }
                    else if (genericController.vbIsNumeric(AddonIDText)) {
                        // 
                        //  it has been set to a non-zero number
                        // 
                        addonId = genericController.EncodeInteger(AddonIDText);
                        // 
                        //  Verify it so there is no error when it runs
                        // 
                        CS = cpCore.db.csOpenRecord(cnAddons, addonId);
                        if (!cpCore.db.csOk(CS)) {
                            // 
                            //  it was set, but the add-on is not available, auto set to dashboard
                            // 
                            addonId = -1;
                            cpCore.siteProperties.setProperty("AdminRootAddonID", "");
                        }
                        
                        cpCore.db.csClose(CS);
                    }
                    
                    if ((addonId == -1)) {
                        // 
                        //  This has never been set, try to get the dashboard ID
                        // 
                        // $$$$$ cache this
                        CS = cpCore.db.csOpen(cnAddons, ("ccguid=" + cpCore.db.encodeSQLText(addonGuidDashboard)));
                        if (cpCore.db.csOk(CS)) {
                            addonId = cpCore.db.csGetInteger(CS, "id");
                            cpCore.siteProperties.setProperty("AdminRootAddonID", genericController.encodeText(addonId));
                        }
                        
                        cpCore.db.csClose(CS);
                    }
                    
                }
                
                if ((addonId != 0)) {
                    // 
                    //  Display the Addon
                    // 
                    if ((cpCore.doc.debug_iUserError != "")) {
                        returnHtml = (returnHtml + ("<div style=\"clear:both;margin-top:20px;\"> </div>" + ("<div style=\"clear:both;margin-top:20px;\">" 
                                    + (errorController.error_GetUserError(cpCore) + "</div>"))));
                    }
                    
                    cpCore.addon.execute(addonModel.create(cpCore, addonId), new BaseClasses.CPUtilsBaseClass.addonExecuteContext(), With, {, ., addonType=BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, ., errorCaption=id:&addonId);
                    // returnHtml = returnHtml & cpCore.addon.execute_legacy4(CStr(addonId), "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin)
                }
                
                if ((returnHtml == "")) {
                    // 
                    //  Nothing Displayed, show default root page
                    // 
                    returnHtml = (returnHtml + ("\r\n" + ("<div style=\"padding:20px;height:450px\">" + ("\r\n" + ("<div><a href=http://www.Contensive.com target=_blank><img style=\"border:1px solid #000;\" src=\"/ccLib/" +
                    "images/ContensiveAdminLogo.GIF\" border=0 ></A></div>" + ("\r\n" + ("<div><strong>Contensive/" 
                                + (cpCore.codeVersion + ("</strong></div>" + ("\r\n" + ("<div style=\"clear:both;height:18px;margin-top:10px\"><div style=\"float:left;width:200px;\">Domain Name<" +
                                "/div><div style=\"float:left;\">" 
                                + (cpCore.webServer.requestDomain + ("</div></div>" + ("\r\n" + ("<div style=\"clear:both;height:18px;\"><div style=\"float:left;width:200px;\">Login Member Name</div><div" +
                                " style=\"float:left;\">" 
                                + (cpCore.doc.authContext.user.name + ("</div></div>" + ("\r\n" + ("<div style=\"clear:both;height:18px;\"><div style=\"float:left;width:200px;\">Quick Reports</div><div sty" +
                                "le=\"float:left;\"><a Href=\"?" 
                                + (RequestNameAdminForm + ("=" 
                                + (AdminFormQuickStats + ("\">Real-Time Activity</A></div></div>" + ("\r\n" + ("<div style=\"clear:both;height:18px;\"><div style=\"float:left;width:200px;\"><a Href=\"?" 
                                + (RequestNameDashboardReset + ("=" 
                                + (cpCore.doc.authContext.visit.id + ("\">Run Dashboard</A></div></div>" + ("\r\n" + ("<div style=\"clear:both;height:18px;\"><div style=\"float:left;width:200px;\"><a Href=\"?addonguid=" 
                                + (addonGuidAddonManager + "\">Add-on Manager</A></div></div>"))))))))))))))))))))))))))))))));
                    if ((cpCore.doc.debug_iUserError != "")) {
                        returnHtml = (returnHtml + ("<div style=\"clear:both;margin-top:20px;\"> </div>" + ("<div style=\"clear:both;margin-top:20px;\">" 
                                    + (errorController.error_GetUserError(cpCore) + "</div>"))));
                    }
                    
                    // 
                    returnHtml = (returnHtml + ("\r\n" + ("</div>" + "")));
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnHtml;
        }
        
        // 
        // ========================================================================
        //    Print the root form
        // ========================================================================
        // 
        private string GetForm_QuickStats() {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_QuickStats")
            // 
            string SQL;
            int CS;
            string RowColor;
            string Panel;
            int VisitID;
            int VisitCount;
            double PageCount;
            stringBuilderLegacyController Stream = new stringBuilderLegacyController();
            // 
            //  --- Start a form to make a refresh button
            // 
            Stream.Add(cpCore.html.html_GetFormStart);
            Stream.Add(cpCore.html.main_GetPanelButtons((ButtonCancel + ("," + ButtonRefresh)), ("" 
                                + (RequestNameButton + ""))));
            Stream.Add(("<input TYPE=\"hidden\" NAME=\"asf\" VALUE=\"" 
                            + (AdminFormQuickStats + "\">")));
            Stream.Add(cpCore.html.main_GetPanel(" "));
            // 
            //  --- Indented part (Title Area plus page)
            // 
            Stream.Add(("<table border=\"0\" cellpadding=\"20\" cellspacing=\"0\" width=\"100%\"><tr><td>" + SpanClassAdminNormal));
            Stream.Add("<h1>Real-Time Activity Report</h1>");
            // 
            //  --- set column width
            // 
            Stream.Add("<h2>Visits Today</h2>");
            Stream.Add("<table border=\"0\" cellpadding=\"3\" cellspacing=\"0\" width=\"100%\" style=\"background-color:white;border-t" +
                "op:1px solid #888;\">");
            // Stream.Add( "<tr"">")
            // Stream.Add( "<td width=""150""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""140"" height=""1"" ></td>")
            // Stream.Add( "<td width=""150""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""140"" height=""1"" ></td>")
            // Stream.Add( "<td width=""100%""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""1"" ></td>")
            // Stream.Add( "</tr>")
            // 
            //  ----- All Visits Today
            // 
            SQL = ("SELECT Count(ccVisits.ID) AS VisitCount, Avg(ccVisits.PageVisits) AS PageCount FROM ccVisits WHERE ((" +
            "ccVisits.StartTime)>" 
                        + (cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime.Date) + ");"));
            CS = cpCore.db.csOpenSql(SQL);
            if (cpCore.db.csOk(CS)) {
                VisitCount = cpCore.db.csGetInteger(CS, "VisitCount");
                PageCount = cpCore.db.csGetNumber(CS, "pageCount");
                Stream.Add("<tr>");
                Stream.Add(("<td style=\"border-bottom:1px solid #888;\" valign=top>" 
                                + (SpanClassAdminNormal + "All Visits</span></td>")));
                Stream.Add(("<td style=\"width:150px;border-bottom:1px solid #888;\" valign=top>" 
                                + (SpanClassAdminNormal + ("<a target=\"_blank\" href=\"/" 
                                + (genericController.encodeHTML((cpCore.serverConfig.appConfig.adminRoute + ("?" 
                                    + (RequestNameAdminForm + ("=" 
                                    + (AdminFormReports + ("&rid=3&DateFrom=" 
                                    + (cpCore.doc.profileStartTime + ("&DateTo=" + cpCore.doc.profileStartTime.ToShortDateString))))))))) + ("\">" 
                                + (VisitCount + ("</A>, " 
                                + (FormatNumber(PageCount, 2) + " pages/visit.</span></td>")))))))));
                Stream.Add(("<td style=\"border-bottom:1px solid #888;\" valign=top>" 
                                + (SpanClassAdminNormal + "This includes all visitors to the website, including guests, bots and administrators. Pages/visit inc" +
                                "ludes page hits and not ajax or remote method hits.</span></td>")));
                Stream.Add("</tr>");
            }
            
            cpCore.db.csClose(CS);
            // 
            //  ----- Non-Bot Visits Today
            // 
            SQL = ("SELECT Count(ccVisits.ID) AS VisitCount, Avg(ccVisits.PageVisits) AS PageCount FROM ccVisits WHERE (c" +
            "cVisits.CookieSupport=1)and((ccVisits.StartTime)>" 
                        + (cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime.Date) + ");"));
            CS = cpCore.db.csOpenSql(SQL);
            if (cpCore.db.csOk(CS)) {
                VisitCount = cpCore.db.csGetInteger(CS, "VisitCount");
                PageCount = cpCore.db.csGetNumber(CS, "pageCount");
                Stream.Add("<tr>");
                Stream.Add(("<td style=\"border-bottom:1px solid #888;\" valign=top>" 
                                + (SpanClassAdminNormal + "Non-bot Visits</span></td>")));
                Stream.Add(("<td style=\"border-bottom:1px solid #888;\" valign=top>" 
                                + (SpanClassAdminNormal + ("<a target=\"_blank\" href=\"/" 
                                + (genericController.encodeHTML((cpCore.serverConfig.appConfig.adminRoute + ("?" 
                                    + (RequestNameAdminForm + ("=" 
                                    + (AdminFormReports + ("&rid=3&DateFrom=" 
                                    + (cpCore.doc.profileStartTime.ToShortDateString + ("&DateTo=" + cpCore.doc.profileStartTime.ToShortDateString))))))))) + ("\">" 
                                + (VisitCount + ("</A>, " 
                                + (FormatNumber(PageCount, 2) + " pages/visit.</span></td>")))))))));
                Stream.Add(("<td style=\"border-bottom:1px solid #888;\" valign=top>" 
                                + (SpanClassAdminNormal + "This excludes hits from visitors identified as bots. Pages/visit includes page hits and not ajax or r" +
                                "emote method hits.</span></td>")));
                Stream.Add("</tr>");
            }
            
            cpCore.db.csClose(CS);
            // 
            //  ----- Visits Today by new visitors
            // 
            SQL = ("SELECT Count(ccVisits.ID) AS VisitCount, Avg(ccVisits.PageVisits) AS PageCount FROM ccVisits WHERE (c" +
            "cVisits.CookieSupport=1)and(ccVisits.StartTime>" 
                        + (cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime.Date) + ")AND(ccVisits.VisitorNew<>0);"));
            CS = cpCore.db.csOpenSql(SQL);
            if (cpCore.db.csOk(CS)) {
                VisitCount = cpCore.db.csGetInteger(CS, "VisitCount");
                PageCount = cpCore.db.csGetNumber(CS, "pageCount");
                Stream.Add("<tr>");
                Stream.Add(("<td style=\"border-bottom:1px solid #888;\" valign=top>" 
                                + (SpanClassAdminNormal + "Visits by New Visitors</span></td>")));
                Stream.Add(("<td style=\"border-bottom:1px solid #888;\" valign=top>" 
                                + (SpanClassAdminNormal + ("<a target=\"_blank\" href=\"/" 
                                + (genericController.encodeHTML((cpCore.serverConfig.appConfig.adminRoute + ("?" 
                                    + (RequestNameAdminForm + ("=" 
                                    + (AdminFormReports + ("&rid=3&ExcludeOldVisitors=1&DateFrom=" 
                                    + (cpCore.doc.profileStartTime.ToShortDateString + ("&DateTo=" + cpCore.doc.profileStartTime.ToShortDateString))))))))) + ("\">" 
                                + (VisitCount + ("</A>, " 
                                + (FormatNumber(PageCount, 2) + " pages/visit.</span></td>")))))))));
                Stream.Add(("<td style=\"border-bottom:1px solid #888;\" valign=top>" 
                                + (SpanClassAdminNormal + "This includes only new visitors not identified as bots. Pages/visit includes page hits and not ajax o" +
                                "r remote method hits.</span></td>")));
                Stream.Add("</tr>");
            }
            
            cpCore.db.csClose(CS);
            // 
            Stream.Add("</table>");
            // 
            //  ----- Visits currently online
            // 
            if (true) {
                Panel = "";
                Stream.Add("<h2>Current Visits</h2>");
                SQL = ("SELECT ccVisits.HTTP_REFERER as referer,ccVisits.remote_addr as Remote_Addr, ccVisits.LastVisitTime a" +
                "s LastVisitTime, ccVisits.PageVisits as PageVisits, ccMembers.Name as MemberName, ccVisits.ID as Vis" +
                "itID, ccMembers.ID as MemberID" + (" FROM ccVisits LEFT JOIN ccMembers ON ccVisits.MemberID = ccMembers.ID" + (" WHERE (((ccVisits.LastVisitTime)>" 
                            + (cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime.AddHours(-1)) + ("))" + " ORDER BY ccVisits.LastVisitTime DESC;")))));
                CS = cpCore.db.csOpenSql(SQL);
                if (cpCore.db.csOk(CS)) {
                    Panel = (Panel + "<table width=\"100%\" border=\"0\" cellspacing=\"1\" cellpadding=\"2\">");
                    Panel = (Panel + "<tr bgcolor=\"#B0B0B0\">");
                    Panel = (Panel + ("<td width=\"20%\" align=\"left\">" 
                                + (SpanClassAdminNormal + "User</td>")));
                    Panel = (Panel + ("<td width=\"20%\" align=\"left\">" 
                                + (SpanClassAdminNormal + "IP Address</td>")));
                    Panel = (Panel + ("<td width=\"20%\" align=\"left\">" 
                                + (SpanClassAdminNormal + "Last Page Hit</td>")));
                    Panel = (Panel + ("<td width=\"10%\" align=\"right\">" 
                                + (SpanClassAdminNormal + "Page Hits</td>")));
                    Panel = (Panel + ("<td width=\"10%\" align=\"right\">" 
                                + (SpanClassAdminNormal + "Visit</td>")));
                    Panel = (Panel + ("<td width=\"30%\" align=\"left\">" 
                                + (SpanClassAdminNormal + "Referer</td>")));
                    Panel = (Panel + "</tr>");
                    RowColor = "ccPanelRowEven";
                    while (cpCore.db.csOk(CS)) {
                        VisitID = cpCore.db.csGetInteger(CS, "VisitID");
                        Panel = (Panel + ("<tr class=\"" 
                                    + (RowColor + "\">")));
                        Panel = (Panel + ("<td align=\"left\">" 
                                    + (SpanClassAdminNormal + ("<a target=\"_blank\" href=\"/" 
                                    + (genericController.encodeHTML((cpCore.serverConfig.appConfig.adminRoute + ("?" 
                                        + (RequestNameAdminForm + ("=" 
                                        + (AdminFormReports + ("&rid=16&MemberID=" + cpCore.db.csGetInteger(CS, "MemberID")))))))) + ("\">" 
                                    + (cpCore.db.csGet(CS, "MemberName") + "</A></span></td>")))))));
                        Panel = (Panel + ("<td align=\"left\">" 
                                    + (SpanClassAdminNormal 
                                    + (cpCore.db.csGet(CS, "Remote_Addr") + "</span></td>"))));
                        Panel = (Panel + ("<td align=\"left\">" 
                                    + (SpanClassAdminNormal 
                                    + (FormatDateTime(cpCore.db.csGetDate(CS, "LastVisitTime"), vbLongTime) + "</span></td>"))));
                        Panel = (Panel + ("<td align=\"right\">" 
                                    + (SpanClassAdminNormal + ("<a target=\"_blank\" href=\"/" 
                                    + (cpCore.serverConfig.appConfig.adminRoute + ("?" 
                                    + (RequestNameAdminForm + ("=" 
                                    + (AdminFormReports + ("&rid=10&VisitID=" 
                                    + (VisitID + ("\">" 
                                    + (cpCore.db.csGet(CS, "PageVisits") + "</A></span></td>")))))))))))));
                        Panel = (Panel + ("<td align=\"right\">" 
                                    + (SpanClassAdminNormal + ("<a target=\"_blank\" href=\"/" 
                                    + (cpCore.serverConfig.appConfig.adminRoute + ("?" 
                                    + (RequestNameAdminForm + ("=" 
                                    + (AdminFormReports + ("&rid=17&VisitID=" 
                                    + (VisitID + ("\">" 
                                    + (VisitID + "</A></span></td>")))))))))))));
                        Panel = (Panel + ("<td align=\"left\">" 
                                    + (SpanClassAdminNormal + (" " 
                                    + (cpCore.db.csGetText(CS, "referer") + "</span></td>")))));
                        Panel = (Panel + "</tr>");
                        if ((RowColor == "ccPanelRowEven")) {
                            RowColor = "ccPanelRowOdd";
                        }
                        else {
                            RowColor = "ccPanelRowEven";
                        }
                        
                        cpCore.db.csGoNext(CS);
                    }
                    
                    Panel = (Panel + "</table>");
                }
                
                cpCore.db.csClose(CS);
                Stream.Add(cpCore.html.main_GetPanel(Panel, "ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 0));
            }
            
            Stream.Add("</td></tr></table>");
            Stream.Add(cpCore.html.html_GetFormEnd);
            // 
            GetForm_QuickStats = Stream.Text;
            cpCore.html.addTitle("Quick Stats");
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            handleLegacyClassError3("GetForm_QuickStats");
            // 
        }
        
        // '
        // '========================================================================
        // '   Print the Topic Rules section of any edit form
        // '========================================================================
        // '
        // Private Function GetForm_Edit_TopicRules() As String
        //     On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.GetForm_Edit_TopicRules")
        //     '
        //     Dim SQL As String
        //     Dim CS as integer
        //     Dim MembershipCount as integer
        //     Dim MembershipSize as integer
        //     Dim MembershipPointer as integer
        //     Dim SectionName As String
        //     Dim TopicCount as integer
        //     Dim Membership() as integer
        //     Dim f As New fastStringClass
        //     Dim Checked As Boolean
        //     Dim TableID as integer
        //     Dim Adminui As New adminUIclass(cpcore)
        //     '
        //     If AdminContent.AllowTopicRules Then
        //         '
        //         ' ----- can not use common call
        //         '       problem, TopicRules has 2 primary content keys (ContentID and RecordID)
        //         '       if we changed it to only use ContentRecordKey, we could use that as the only primary key.
        //         '
        //         ' ----- Gather all the topics to which this member belongs
        //         '
        //         MembershipCount = 0
        //         MembershipSize = 0
        //         If EditRecord.ID <> 0 Then
        //             SQL = "SELECT ccTopicRules.TopicID AS TopicID FROM (ccContent LEFT JOIN ccTopicRules ON ccContent.ID = ccTopicRules.ContentID) LEFT JOIN ccTables ON ccContent.ContentTableID = ccTables.ID WHERE (((ccTables.Name)=" & encodeSQLText(AdminContent.ContentTableName) & ") AND ((ccTopicRules.RecordID)=" & EditRecord.ID & ") AND ((ccContent.Active)<>0) AND ((ccTopicRules.Active)<>0));"
        // 
        //             'SQL = "SELECT ccTopicRules.TopicID as ID" _
        //              '   & " FROM ccContent LEFT JOIN ccTopicRules ON ccContent.ID = ccTopicRules.ContentID" _
        //               '  & " WHERE (((ccContent.ContentTablename)=" & encodeSQLText(AdminContent.ContentTableName) & ") AND ((ccTopicRules.RecordID)=" & EditRecord.ID & ") AND ((ccContent.Active)<>0) AND ((ccTopicRules.Active)<>0))"
        //             CS = cpCore.app_openCsSql_Rev_Internal("Default", SQL)
        //             If cpCore.app.csv_IsCSOK(CS) Then
        //                 If True Then
        //                     MembershipSize = 10
        //                     ReDim Membership(MembershipSize)
        //                     Do While cpCore.app.csv_IsCSOK(CS)
        //                         If MembershipCount >= MembershipSize Then
        //                             MembershipSize = MembershipSize + 10
        //                             ReDim Preserve Membership(MembershipSize)
        //                             End If
        //                         Membership(MembershipCount) = cpCore.app.cs_getInteger(CS, "TopicID")
        //                         MembershipCount = MembershipCount + 1
        //                         Call cpCore.app.nextCSRecord(CS)
        //                         Loop
        //                     End If
        //                 End If
        //             cpCore.main_CloseCS (CS)
        //             End If
        //         '
        //         ' ----- Gather all the topics, sorted by ContentName (no topics, skip section)
        //         '
        //         SQL = "SELECT ccTopics.ID AS ID, ccContent.Name AS SectionName, ccTopics.Name AS TopicName, ccTopics.SortOrder" _
        //             & " FROM ccTopics LEFT JOIN ccContent ON ccTopics.ContentControlID = ccContent.ID" _
        //             & " Where (((ccTopics.Active) <> " & SQLFalse & ") And ((ccContent.Active) <> " & SQLFalse & "))" _
        //             & " GROUP BY ccTopics.ID, ccContent.Name, ccTopics.Name, ccTopics.SortOrder" _
        //             & " ORDER BY ccContent.Name, ccTopics.SortOrder"
        //         CS = cpCore.app_openCsSql_Rev_Internal("Default", SQL)
        //         If cpCore.app.csv_IsCSOK(CS) Then
        //             If True Then
        //                 '
        //                 ' ----- Open the panel
        //                 '
        //                 Call f.Add(AdminUI.EditTableOpen)
        //                 SectionName = ""
        //                 TopicCount = 0
        //                 Do While cpCore.app.csv_IsCSOK(CS)
        //                     f.Add( "<tr>"
        //                     If SectionName <> cpCore.app.cs_get(CS, "SectionName") Then
        //                         '
        //                         ' ----- create the next content Topic row
        //                         '
        //                         SectionName = cpCore.app.cs_get(CS, "SectionName")
        //                         Call f.Add("<td class=""ccAdminEditCaption"">" & SectionName & "</td>")
        //                     Else
        //                         Call f.Add("<td class=""ccAdminEditCaption""> </td>")
        //                     End If
        //                     Call f.Add("<td class=""ccAdminEditField"">")
        //                     Checked = False
        //                     If MembershipCount <> 0 Then
        //                         For MembershipPointer = 0 To MembershipCount - 1
        //                             If Membership(MembershipPointer) = cpCore.app.cs_getInteger(CS, "ID") Then
        //                                 Checked = True
        //                                 Exit For
        //                             End If
        //                         Next
        //                     End If
        //                     If editrecord.read_only And Not Checked Then
        //                         f.Add( "<input type=""checkbox"" disabled>"
        //                     ElseIf editrecord.read_only Then
        //                         f.Add( "<input type=""checkbox"" disabled checked>"
        //                         f.Add( "<input type=hidden name=""Topic" & TopicCount & """ value=1>"
        //                     ElseIf Checked Then
        //                         f.Add( "<input type=""checkbox"" name=""Topic" & TopicCount & """ checked>"
        //                     Else
        //                         f.Add( "<input type=""checkbox"" name=""Topic" & TopicCount & """>"
        //                     End If
        //                     f.Add( "<input type=""hidden"" name=""TopicID" & TopicCount & """ value=""" & cpCore.app.cs_get(CS, "ID") & """>"
        //                     f.Add( SpanClassAdminNormal & cpCore.app.cs_get(CS, "TopicName") & "</span></td>"
        //                     f.Add( "</tr>"
        //                     '
        //                     TopicCount = TopicCount + 1
        //                     Call cpCore.app.nextCSRecord(CS)
        //                 Loop
        //                 f.Add( vbCrLf & "<input type=""hidden"" name=""TopicCount"" value=""" & TopicCount & """>"
        //                 f.Add( AdminUI.EditTableClose
        //                 '
        //                 ' ----- close the panel
        //                 '
        //                 GetForm_Edit_TopicRules = AdminUI.GetEditPanel( (Not AllowAdminTabs), "Topic Rules", "This content is associated with the following topics", f.Text)
        //                 EditSectionPanelCount = EditSectionPanelCount + 1
        //                 '
        //                 End If
        //             End If
        //         Call cpCore.app.closeCS(CS)
        //     End If
        //     '''Dim th as integer: Exit Function
        //     '
        // ErrorTrap:
        //     Call HandleClassTrapErrorBubble("GetForm_Edit_TopicRules")
        // End Function
        // 
        // ========================================================================
        //    Print the Topic Rules section of any edit form
        // ========================================================================
        // 
        private string GetForm_Edit_LinkAliases(Models.Complex.cdefModel adminContent, editRecordClass editRecord, bool readOnlyField) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_LinkAliases")
            // 
            int LinkCnt;
            string LinkList = "";
            stringBuilderLegacyController f = new stringBuilderLegacyController();
            adminUIController Adminui = new adminUIController(cpCore);
            int Ptr;
            string linkAlias;
            bool AllowLinkAliasInTab;
            string Link;
            int CS;
            string tabContent;
            string TabDescription;
            // 
            // 
            //  Link Alias value from the admin data
            // 
            TabDescription = @"Link Aliases are URLs used for this content that are more friendly to users and search engines. If you set the Link Alias field, this name will be used on the URL for this page. If you leave the Link Alias blank, the page name will be used. Below is a list of names that have been used previously and are still active. All of these entries when used in the URL will resolve to this page. The first entry in this list will be used to create menus on the site. To move an entry to the top, type it into the Link Alias field and save.";
            if (!cpCore.siteProperties.allowLinkAlias) {
                // 
                //  Disabled
                // 
                tabContent = " ";
                TabDescription = ("<p>The Link Alias feature is currently disabled. To enable Link Aliases, check the box marked \'Allow " +
                "Link Alias\' on the Page Settings page found on the Navigator under \'Settings\'.</p><p>" 
                            + (TabDescription + "</p>"));
            }
            else {
                // 
                //  Link Alias Field
                // 
                linkAlias = "";
                if (adminContent.fields.ContainsKey("linkalias")) {
                    linkAlias = genericController.encodeText(editRecord.fieldsLc.Item["linkalias"].value);
                }
                
                f.Add(("<tr><td class=\"ccAdminEditCaption\">" 
                                + (SpanClassAdminSmall + "Link Alias</td>")));
                f.Add(("<td class=\"ccAdminEditField\" align=\"left\" colspan=\"2\">" + SpanClassAdminNormal));
                if (readOnlyField) {
                    f.Add(linkAlias);
                }
                else {
                    f.Add(cpCore.html.html_GetFormInputText2("LinkAlias", linkAlias));
                }
                
                f.Add("</span></td></tr>");
                // 
                //  Override Duplicates
                // 
                f.Add(("<tr><td class=\"ccAdminEditCaption\">" 
                                + (SpanClassAdminSmall + "Override Duplicates</td>")));
                f.Add(("<td class=\"ccAdminEditField\" align=\"left\" colspan=\"2\">" + SpanClassAdminNormal));
                if (readOnlyField) {
                    f.Add("No");
                }
                else {
                    f.Add(cpCore.html.html_GetFormInputCheckBox2("OverrideDuplicate", false));
                }
                
                f.Add("</span></td></tr>");
                // 
                //  Table of old Link Aliases
                // 
                Link = cpCore.doc.main_GetPageDynamicLink(editRecord.id, false);
                CS = cpCore.db.csOpen("Link Aliases", ("pageid=" + editRecord.id), "ID Desc", ,, ,, "name");
                while (cpCore.db.csOk(CS)) {
                    LinkList = (LinkList + ("<div style=\"margin-left:4px;margin-bottom:4px;\">" 
                                + (genericController.encodeHTML(cpCore.db.csGetText(CS, "name")) + "</div>")));
                    LinkCnt = (LinkCnt + 1);
                    cpCore.db.csGoNext(CS);
                }
                
                cpCore.db.csClose(CS);
                if ((LinkCnt > 0)) {
                    f.Add(("<tr><td class=\"ccAdminEditCaption\">" 
                                    + (SpanClassAdminSmall + "Previous Link Alias List</td>")));
                    f.Add(("<td class=\"ccAdminEditField\" align=\"left\" colspan=\"2\">" + SpanClassAdminNormal));
                    f.Add(LinkList);
                    f.Add("</span></td></tr>");
                }
                
                tabContent = (Adminui.EditTableOpen 
                            + (f.Text + Adminui.EditTableClose));
            }
            
            // 
            GetForm_Edit_LinkAliases = Adminui.GetEditPanel(!allowAdminTabs, "Link Aliases", TabDescription, tabContent);
            EditSectionPanelCount = (EditSectionPanelCount + 1);
            // 
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
        ErrorTrap:
            handleLegacyClassError3("GetForm_Edit_LinkAliases");
        }
        
        //         '
        //         '========================================================================
        //         '   Print the Topic Rules section of any edit form
        //         '========================================================================
        //         '
        //         Private Function GetForm_Edit_MetaContent(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, ByVal readOnlyField As Boolean) As String
        //             On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_MetaContent")
        //             '
        //             Dim s As String
        //             Dim SQL As String
        //             Dim FastString As New stringBuilderLegacyController
        //             Dim Checked As Boolean
        //             Dim TableID As Integer
        //             Dim MetaContentID As Integer
        //             Dim CS As Integer
        //             Dim PageTitle As String = ""
        //             Dim MetaDescription As String = ""
        //             Dim MetaKeywordList As String = ""
        //             Dim OtherHeadTags As String = ""
        //             Dim Adminui As New adminUIController(cpCore)
        //             '
        //             If adminContent.AllowMetaContent Then
        //                 CS = cpCore.db.cs_open("Meta Content", "(ContentID=" & editRecord.contentControlId & ")and(RecordID=" & editRecord.id & ")")
        //                 If Not cpCore.db.cs_ok(CS) Then
        //                     CS = cpCore.db.cs_insertRecord("Meta Content")
        //                     Call cpCore.db.cs_set(CS, "ContentID", editRecord.contentControlId)
        //                     Call cpCore.db.cs_set(CS, "RecordID", editRecord.id)
        //                     Call cpCore.db.cs_save2(CS)
        //                 End If
        //                 If cpCore.db.cs_ok(CS) Then
        //                     MetaContentID = cpCore.db.cs_getInteger(CS, "ID")
        //                     PageTitle = cpCore.db.cs_get(CS, "Name")
        //                     MetaDescription = cpCore.db.cs_get(CS, "MetaDescription")
        //                     If True Then ' 3.3.930" Then
        //                         MetaKeywordList = cpCore.db.cs_get(CS, "MetaKeywordList")
        //                         OtherHeadTags = cpCore.db.cs_get(CS, "OtherHeadTags")
        //                     ElseIf cpCore.db.cs_isFieldSupported(CS, "OtherHeadTags") Then
        //                         OtherHeadTags = cpCore.db.cs_get(CS, "OtherHeadTags")
        //                     End If
        //                 End If
        //                 Call cpCore.db.cs_Close(CS)
        //                 '
        //                 'Call FastString.Add(cpCore.main_GetFormInputHidden("MetaContent.MetaContentID", MetaContentID))
        //                 '
        //                 ' Page Title
        //                 '
        //                 Call FastString.Add("<tr><td class=""ccAdminEditCaption"">" & SpanClassAdminSmall & "Page Title</td>")
        //                 Call FastString.Add("<td class=""ccAdminEditField"" align=""left"" colspan=""2"">" & SpanClassAdminNormal)
        //                 If readOnlyField Then
        //                     Call FastString.Add(PageTitle)
        //                 Else
        //                     Call FastString.Add(cpCore.html.html_GetFormInputText2("MetaContent.PageTitle", PageTitle))
        //                 End If
        //                 Call FastString.Add("</span></td></tr>")
        //                 '
        //                 ' Meta Description
        //                 '
        //                 Call FastString.Add("<tr><td class=""ccAdminEditCaption"">" & SpanClassAdminSmall & "Meta Description</td>")
        //                 Call FastString.Add("<td class=""ccAdminEditField"" align=""left"" colspan=""2"">" & SpanClassAdminNormal)
        //                 If readOnlyField Then
        //                     Call FastString.Add(MetaDescription)
        //                 Else
        //                     Call FastString.Add(cpCore.html.html_GetFormInputTextExpandable("MetaContent.MetaDescription", MetaDescription, 10))
        //                 End If
        //                 Call FastString.Add("</span></td></tr>")
        //                 '
        //                 ' Meta Keyword List
        //                 '
        //                 Call FastString.Add("<tr><td class=""ccAdminEditCaption"">" & SpanClassAdminSmall & "Meta Keyword List</td>")
        //                 Call FastString.Add("<td class=""ccAdminEditField"" align=""left"" colspan=""2"">" & SpanClassAdminNormal)
        //                 If readOnlyField Then
        //                     Call FastString.Add(MetaKeywordList)
        //                 Else
        //                     Call FastString.Add(cpCore.html.html_GetFormInputTextExpandable("MetaContent.MetaKeywordList", MetaKeywordList, 10))
        //                 End If
        //                 Call FastString.Add("</span></td></tr>")
        //                 '
        //                 ' Meta Keywords, Shared
        //                 '
        //                 Call FastString.Add("<tr><td class=""ccAdminEditCaption"">" & SpanClassAdminSmall & "Shared Meta Keywords</td>")
        //                 Call FastString.Add("<td class=""ccAdminEditField"" colspan=""2"">")
        //                 Call FastString.Add(cpCore.html.getInputCheckList("MetaContent.KeywordList", "Meta Content", MetaContentID, "Meta Keywords", "Meta Keyword Rules", "MetaContentID", "MetaKeywordID", , "Name", readOnlyField))
        //                 'Call FastString.Add(cpCore.html.getInputCheckListCategories("MetaContent.KeywordList", "Meta Content", MetaContentID, "Meta Keywords", "Meta Keyword Rules", "MetaContentID", "MetaKeywordID", , "Name", readOnlyField, "Meta Keywords"))
        //                 Call FastString.Add("</td></tr>")
        //                 '
        //                 ' Other Head Tags
        //                 '
        //                 Call FastString.Add("<tr><td class=""ccAdminEditCaption"">" & SpanClassAdminSmall & "Other Head Tags</td>")
        //                 Call FastString.Add("<td class=""ccAdminEditField"" colspan=""2"">" & SpanClassAdminNormal)
        //                 If readOnlyField Then
        //                     Call FastString.Add(OtherHeadTags)
        //                 Else
        //                     Call FastString.Add(cpCore.html.html_GetFormInputTextExpandable("MetaContent.OtherHeadTags", OtherHeadTags, 10))
        //                 End If
        //                 Call FastString.Add("</span></td></tr>")
        //                 '
        //                 s = "" _
        //                     & Adminui.EditTableOpen & FastString.Text & Adminui.EditTableClose _
        //                     & cpCore.html.html_GetFormInputHidden("MetaContent.MetaContentID", MetaContentID) _
        //                     & ""
        //                 '
        //                 GetForm_Edit_MetaContent = Adminui.GetEditPanel((Not allowAdminTabs), "Meta Content", "Meta Tags available for pages using this content", s)
        //                 EditSectionPanelCount = EditSectionPanelCount + 1
        //                 '
        //                 FastString = Nothing
        //             End If
        //             '
        //             Exit Function
        //             '
        // ErrorTrap:
        //             FastString = Nothing
        //             Call handleLegacyClassError3("GetForm_Edit_MetaContent")
        //         End Function
        // 
        // ========================================================================
        //  Print the Email form Group associations
        // 
        //    Content must conform to ccMember fields
        // ========================================================================
        // 
        private string GetForm_Edit_EmailRules(Models.Complex.cdefModel adminContent, editRecordClass editRecord, bool readOnlyField) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_EmailRules")
            // 
            stringBuilderLegacyController f = new stringBuilderLegacyController();
            string GroupList;
            string[] GroupSplit;
            int Ptr;
            int IDPtr;
            int IDEndPtr;
            int GroupID;
            string ReportLink;
            int Cnt;
            adminUIController Adminui = new adminUIController(cpCore);
            string s;
            // 
            s = cpCore.html.getCheckList("EmailGroups", "Group Email", editRecord.id, "Groups", "Email Groups", "EmailID", "GroupID", ,, "Caption");
            // s = cpCore.html.getInputCheckListCategories("EmailGroups", "Group Email", editRecord.id, "Groups", "Email Groups", "EmailID", "GroupID", , "Caption", readOnlyField, "Groups")
            s = ("<tr>" + ("<td class=\"ccAdminEditCaption\">Groups</td>" + ("<td class=\"ccAdminEditField\" colspan=2>" 
                        + (SpanClassAdminNormal 
                        + (s + ("</span></td>" + ("</tr><tr>" + ("<td class=\"ccAdminEditCaption\"> </td>" + ("<td class=\"ccAdminEditField\" colspan=2>" 
                        + (SpanClassAdminNormal + ("[<a href=?cid=" 
                        + (Models.Complex.cdefModel.getContentId(cpCore, "Groups") + (" target=_blank>Manage Groups</a>]</span></td>" + "</tr>")))))))))))));
            s = (Adminui.EditTableOpen 
                        + (s + Adminui.EditTableClose));
            s = Adminui.GetEditPanel(!allowAdminTabs, "Email Rules", "Send email to people in these groups", s);
            return s;
            
            // GroupList = cpCore.htmldoc.main_GetFormInputCheckList("EmailGroups", "Group Email", EditRecord.ID, "Groups", "Email Groups", "EmailID", "GroupID", , "Caption", readOnlyField)
            GroupSplit = Split(GroupList, "<br >", ,, vbTextCompare);
            Cnt = (UBound(GroupSplit) + 1);
            if ((Cnt == 0)) {
                f.Add("<tr>");
                f.Add("<td class=\"ccAdminEditCaption\"> </td>");
                f.Add(("<td class=\"ccAdminEditField\" colspan=2>" 
                                + (SpanClassAdminNormal + "There are no currently no groups defined.</span></td>")));
                f.Add("</tr>");
            }
            else {
                for (Ptr = 0; (Ptr 
                            <= (Cnt - 1)); Ptr++) {
                    GroupID = 0;
                    int HiddenPos;
                    HiddenPos = genericController.vbInstr(1, GroupSplit[Ptr], "hidden", vbTextCompare);
                    if ((HiddenPos > 0)) {
                        IDPtr = genericController.vbInstr(1, GroupSplit[Ptr], "value=", vbTextCompare);
                        // IDPtr = genericController.vbInstr(HiddenPos, GroupSplit(Ptr), "value=", vbTextCompare)
                        if ((IDPtr > 0)) {
                            IDEndPtr = genericController.vbInstr(IDPtr, GroupSplit[Ptr], ">");
                            if ((IDEndPtr > 0)) {
                                GroupID = genericController.EncodeInteger(GroupSplit[Ptr].Substring((IDPtr + 5), (IDEndPtr 
                                                    - (IDPtr - 6))));
                            }
                            
                        }
                        
                        if ((GroupID > 0)) {
                            ReportLink = ("[<a href=\"?" 
                                        + (RequestNameAdminForm + ("=12&rid=35&recordid=" 
                                        + (GroupID + "\" target=_blank>Group Report</a>]"))));
                        }
                        else {
                            ReportLink = " ";
                        }
                        
                    }
                    
                    f.Add(("<tr>" + ("<td class=\"ccAdminEditCaption\"> </td>" + ("<td class=\"ccAdminEditField\" colspan=2>" 
                                    + (SpanClassAdminNormal 
                                    + (GroupSplit[Ptr] + (" " 
                                    + (ReportLink + ("</span></td>" + "</tr>")))))))));
                }
                
            }
            
            f.Add("<tr>");
            f.Add("<td class=\"ccAdminEditCaption\"> </td>");
            f.Add(("<td class=\"ccAdminEditCaption\" colspan=2>" 
                            + (SpanClassAdminNormal + ("[<a href=?cid=" 
                            + (Models.Complex.cdefModel.getContentId(cpCore, "Groups") + " target=_blank>Manage Groups</a>]</span></td>")))));
            f.Add("</tr>");
            GetForm_Edit_EmailRules = Adminui.GetEditPanel(!allowAdminTabs, "Email Rules", "Send email to people in these groups", (Adminui.EditTableOpen 
                            + (f.Text + Adminui.EditTableClose)));
            EditSectionPanelCount = (EditSectionPanelCount + 1);
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
        ErrorTrap:
            handleLegacyClassError3("GetForm_Edit_EmailRules");
        }
        
        // 
        // ========================================================================
        //  Print the Email for Topic associations
        // 
        //    Content must conform to ccMember fields
        // ========================================================================
        // 
        private string GetForm_Edit_EmailTopics(Models.Complex.cdefModel adminContent, editRecordClass editRecord, bool readOnlyField) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_EmailTopics")
            // 
            stringBuilderLegacyController f = new stringBuilderLegacyController();
            string GroupList;
            string[] GroupSplit;
            int Ptr;
            int IDPtr;
            int IDEndPtr;
            int GroupID;
            string ReportLink;
            int Cnt;
            adminUIController Adminui = new adminUIController(cpCore);
            // 
            string s;
            // 
            s = cpCore.html.getCheckList("EmailTopics", "Group Email", editRecord.id, "Topics", "Email Topics", "EmailID", "TopicID", ,, "Name");
            // s = cpCore.html.getInputCheckListCategories("EmailTopics", "Group Email", editRecord.id, "Topics", "Email Topics", "EmailID", "TopicID", , "Name", readOnlyField, "Topics")
            s = ("<tr>" + ("<td class=\"ccAdminEditCaption\">Topics</td>" + ("<td class=\"ccAdminEditField\" colspan=2>" 
                        + (SpanClassAdminNormal 
                        + (s + ("</span></td>" + ("</tr><tr>" + ("<td class=\"ccAdminEditCaption\"> </td>" + ("<td class=\"ccAdminEditField\" colspan=2>" 
                        + (SpanClassAdminNormal + ("[<a href=?cid=" 
                        + (Models.Complex.cdefModel.getContentId(cpCore, "Topics") + (" target=_blank>Manage Topics</a>]</span></td>" + "</tr>")))))))))))));
            s = (Adminui.EditTableOpen 
                        + (s + Adminui.EditTableClose));
            s = Adminui.GetEditPanel(!allowAdminTabs, "Email Rules", "Send email to people in these groups", s);
            return s;
            
            // GroupList = cpCore.htmldoc.main_GetFormInputCheckList("EmailTopics", "Group Email", EditRecord.ID, "Topics", "Email Topics", "EmailID", "TopicID", , "Name", readOnlyField)
            // GroupSplit = Split(GroupList, "<br >", , vbTextCompare)
            // Cnt = UBound(GroupSplit) + 1
            // If Cnt = 0 Then
            //     f.Add("<tr>")
            //     f.Add("<td class=""ccAdminEditCaption""> </td>")
            //     f.Add("<td class=""ccAdminEditField"" colspan=2>" & SpanClassAdminNormal & "There are no currently no topics defined.</span></td>")
            //     f.Add("</tr>")
            // Else
            //     For Ptr = 0 To UBound(GroupSplit)
            //         GroupID = 0
            //         IDPtr = genericController.vbInstr(1, GroupSplit(Ptr), "value=", vbTextCompare)
            //         If IDPtr > 0 Then
            //             IDEndPtr = genericController.vbInstr(IDPtr, GroupSplit(Ptr), ">")
            //             If IDEndPtr > 0 Then
            //                 GroupID = genericController.EncodeInteger(Mid(GroupSplit(Ptr), IDPtr + 6, IDEndPtr - IDPtr - 6))
            //             End If
            //         End If
            //         If GroupID > 0 Then
            //             ReportLink = " "
            //             'ReportLink = "<a href=""?" & RequestNameAdminForm & "=12&rid=35&recordid=" & GroupID & """ target=_blank>Group Report</a>"
            //         Else
            //             ReportLink = " "
            //         End If
            //         f.Add("<tr>" _
            //             & "<td class=""ccAdminEditCaption""> </td>" _
            //             & "<td class=""ccAdminEditField"" colspan=2>" & SpanClassAdminNormal & GroupSplit(Ptr) & ReportLink & "</span></td>" _
            //             & "</tr>")
            //     Next
            // End If
            // f.Add("<tr>")
            // f.Add("<td class=""ccAdminEditCaption""> </td>")
            // f.Add("<td class=""ccAdminEditField"" colspan=2>" & SpanClassAdminNormal & "[<a href=?cid=" & cpCore.main_GetContentID("Topics") & " target=_blank>Manage Topics</a>]</span></td>")
            // f.Add("</tr>")
            // GetForm_Edit_EmailTopics = Adminui.GetEditPanel((Not AllowAdminTabs), "Email Topics", "Send email to people who are associated with these topics", Adminui.EditTableOpen & f.Text & Adminui.EditTableClose)
            // EditSectionPanelCount = EditSectionPanelCount + 1
            // Exit Function
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            handleLegacyClassError3("GetForm_Edit_EmailTopics");
            // 
        }
        
        // 
        // ========================================================================
        // 
        // ========================================================================
        // 
        private string GetForm_Edit_EmailBounceStatus() {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_EmailBounceStatus")
            // 
            stringBuilderLegacyController f = new stringBuilderLegacyController();
            string Copy;
            adminUIController Adminui = new adminUIController(cpCore);
            // 
            f.Add(Adminui.GetEditRow(("<a href=?" 
                                + (RequestNameAdminForm + "=28 target=_blank>Open in New Window</a>")), "Email Control", "The settings in this section can be modified with the Email Control page."));
            f.Add(Adminui.GetEditRow(cpCore.siteProperties.getText("EmailBounceAddress", ""), "Bounce Email Address", "All bounced emails will be sent to this address automatically. This must be a valid email account, an" +
                    "d you should either use Contensive Bounce processing to capture the emails, or manually remove them " +
                    "from the account yourself."));
            f.Add(Adminui.GetEditRow(genericController.GetYesNo(genericController.EncodeBoolean(cpCore.siteProperties.getBoolean("AllowEmailBounceProcessing", false))), "Allow Bounce Email Processing", "If checked, Contensive will periodically retrieve all the email from the POP email account and take a" +
                    "ction on the membefr account that sent the email."));
            switch (cpCore.siteProperties.getText("EMAILBOUNCEPROCESSACTION", "0")) {
                case "1":
                    Copy = "Clear the Allow Group Email field for all members with a matching Email address";
                    break;
                case "2":
                    Copy = "Clear all member Email addresses that match the Email address";
                    break;
                case "3":
                    Copy = "Delete all Members with a matching Email address";
                    break;
                default:
                    Copy = "Do Nothing";
                    break;
            }
            f.Add(Adminui.GetEditRow(Copy, "Bounce Email Action", "When an email is determined to be a bounce, this action will taken against member with that email add" +
                    "ress."));
            f.Add(Adminui.GetEditRow(cpCore.siteProperties.getText("POPServerStatus"), "Last Email Retrieve Status", "This is the status of the last POP email retrieval attempted."));
            // 
            GetForm_Edit_EmailBounceStatus = Adminui.GetEditPanel(!allowAdminTabs, "Bounced Email Handling", "", (Adminui.EditTableOpen 
                            + (f.Text + Adminui.EditTableClose)));
            EditSectionPanelCount = (EditSectionPanelCount + 1);
            // TODO: Exit Function: Warning!!! Need to return the value
            return;
            // 
            //  ----- Error Trap
            // 
        ErrorTrap:
            handleLegacyClassError3("GetForm_Edit_EmailBounceStatus");
            // 
        }
        
        // 
        // ========================================================================
        //  Print the Member Edit form
        // 
        //    Content must conform to ccMember fields
        // ========================================================================
        // 
        private string GetForm_Edit_MemberGroups(Models.Complex.cdefModel adminContent, editRecordClass editRecord) {
            // TODO: On Error GoTo Warning!!!: The statement is not translatable 
            // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_MemberGroups")
            // 
            stringBuilderLegacyController f = new stringBuilderLegacyController();
            string Copy;
            string SQL;
            int CS;
            int MembershipCount;
            int MembershipSize;
            int MembershipPointer;
            string SectionName;
            int PeopleContentID;
            int GroupContentID;
            bool CanSeeHiddenGroups;
            string DateExpireValue;
            int GroupCount;
            int GroupID;
            string GroupName;
            string GroupCaption;
            bool GroupActive;
            int[] Membership;
            DateTime[] DateExpires;
            bool[] Active;
            string Caption;
            string MethodName;
            string ReportLink;
            adminUIController Adminui = new adminUIController(cpCore);
            // 
            MethodName = "GetForm_Edit_MemberGroups";
            PeopleContentID = Models.Complex.cdefModel.getContentId(cpCore, "People");
            GroupContentID = Models.Complex.cdefModel.getContentId(cpCore, "Groups");
            // 
            MembershipCount = 0;
            MembershipSize = 0;
            if (true) {
                // If EditRecord.ID <> 0 Then
                // 
                //  ----- read in the groups that this member has subscribed (exclude new member records)
                // 
                if ((editRecord.id != 0)) {
                    SQL = ("SELECT Active,GroupID,DateExpires" + (" FROM ccMemberRules" + (" WHERE MemberID=" + editRecord.id)));
                    CS = cpCore.db.csOpenSql_rev("Default", SQL);
                    while (cpCore.db.csOk(CS)) {
                        if ((MembershipCount >= MembershipSize)) {
                            MembershipSize = (MembershipSize + 10);
                            object Preserve;
                            Membership[MembershipSize];
                            object Preserve;
                            Active[MembershipSize];
                            object Preserve;
                            DateExpires[MembershipSize];
                        }
                        
                        Membership[MembershipCount] = cpCore.db.csGetInteger(CS, "GroupID");
                        DateExpires[MembershipCount] = cpCore.db.csGetDate(CS, "DateExpires");
                        Active[MembershipCount] = cpCore.db.csGetBoolean(CS, "Active");
                        MembershipCount = (MembershipCount + 1);
                        cpCore.db.csGoNext(CS);
                    }
                    
                    cpCore.db.csClose(CS);
                }
                
                // 
                //  ----- read in all the groups, sorted by ContentName
                // 
                SQL = ("SELECT ccGroups.ID AS ID, ccContent.Name AS SectionName, ccGroups.Caption AS GroupCaption, ccGroups.n" +
                "ame AS GroupName, ccGroups.SortOrder" + (" FROM ccGroups LEFT JOIN ccContent ON ccGroups.ContentControlID = ccContent.ID" + (" Where (((ccGroups.Active) <> " 
                            + (SQLFalse + (") And ((ccContent.Active) <> " 
                            + (SQLFalse + "))"))))));
                ("" + (" GROUP BY ccGroups.ID, ccContent.Name, ccGroups.Caption, ccGroups.name, ccGroups.SortOrder" + " ORDER BY ccGroups.Caption"));
                CS = cpCore.db.csOpenSql_rev("Default", SQL);
                // 
                //  Output all the groups, with the active and dateexpires from those joined
                // 
                f.Add(Adminui.EditTableOpen);
                SectionName = "";
                GroupCount = 0;
                CanSeeHiddenGroups = cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore);
                while (cpCore.db.csOk(CS)) {
                    GroupName = cpCore.db.csGet(CS, "GroupName");
                    if (((GroupName.Substring(0, 1) != "_") 
                                || CanSeeHiddenGroups)) {
                        GroupCaption = cpCore.db.csGet(CS, "GroupCaption");
                        GroupID = cpCore.db.csGetInteger(CS, "ID");
                        if ((GroupCaption == "")) {
                            GroupCaption = GroupName;
                            if ((GroupCaption == "")) {
                                GroupCaption = ("Group " + GroupID);
                            }
                            
                        }
                        
                        GroupActive = false;
                        DateExpireValue = "";
                        if ((MembershipCount != 0)) {
                            for (MembershipPointer = 0; (MembershipPointer 
                                        <= (MembershipCount - 1)); MembershipPointer++) {
                                if ((Membership[MembershipPointer] == GroupID)) {
                                    GroupActive = Active[MembershipPointer];
                                    MinValue;
                                    DateExpireValue = genericController.encodeText(DateExpires[MembershipPointer]);
                                }
                                
                                break;
                                if ((ReportLink == "")) {
                                    ReportLink = (ReportLink + ("[<a href=\"?af=4&cid=" 
                                                + (GroupContentID + ("&id=" 
                                                + (GroupID + "\">Edit Group</a>]")))));
                                    if ((GroupID > 0)) {
                                        ReportLink = (ReportLink + (" [<a href=\"?" 
                                                    + (RequestNameAdminForm + ("=12&rid=35&recordid=" 
                                                    + (GroupID + "\">Group Report</a>]")))));
                                    }
                                    
                                    // 
                                    if ((GroupCount == 0)) {
                                        Caption = (SpanClassAdminSmall + "Groups</span>");
                                    }
                                    else {
                                        Caption = " ";
                                    }
                                    
                                    f.Add(("<tr><td class=\"ccAdminEditCaption\">" 
                                                    + (Caption + "</td>")));
                                    f.Add("<td class=\"ccAdminEditField\">");
                                    f.Add("<table border=0 cellpadding=0 cellspacing=0 width=\"100%\" ><tr>");
                                    f.Add(("<td width=\"40%\">" 
                                                    + (cpCore.html.html_GetFormInputHidden(("Memberrules." 
                                                        + (GroupCount + ".ID")), GroupID) 
                                                    + (cpCore.html.html_GetFormInputCheckBox2(("MemberRules." + GroupCount), GroupActive) 
                                                    + (GroupCaption + "</td>")))));
                                    f.Add(("<td width=\"30%\"> Expires " 
                                                    + (cpCore.html.html_GetFormInputText2(("MemberRules." 
                                                        + (GroupCount + ".DateExpires")), DateExpireValue, 1, 20) + "</td>")));
                                    f.Add(("<td width=\"30%\">" 
                                                    + (ReportLink + "</td>")));
                                    f.Add("</tr></table>");
                                    f.Add("</td></tr>");
                                    GroupCount = (GroupCount + 1);
                                }
                                
                                cpCore.db.csGoNext(CS);
                                cpCore.db.csClose(CS);
                                GroupCount = 0;
                                // If EditRecord.ID = 0 Then
                                //     F.Add( "<tr>" _
                                //         & "<td valign=middle align=right>" & SpanClassAdminSmall & "Groups</span></td>" _
                                //         & "<td>" & SpanClassAdminNormal & "Groups will be available after this record is saved</span></td>" _
                                //         & "</tr>"
                                // ElseIf GroupCount = 0 Then
                                f.Add(("<tr>" + ("<td valign=middle align=right>" 
                                                + (SpanClassAdminSmall + ("Groups</span></td>" + ("<td>" 
                                                + (SpanClassAdminNormal + ("There are currently no groups defined</span></td>" + "</tr>"))))))));
                                f.Add(("<input type=\"hidden\" name=\"MemberRules.RowCount\" value=\"" 
                                                + (GroupCount + "\">")));
                                if (f.Add("<tr>")) {
                                    f.Add("<td class=\"ccAdminEditCaption\"> </td>");
                                    f.Add(("<td class=\"ccAdminEditField\">" 
                                                    + (SpanClassAdminNormal + ("[<a href=?cid=" 
                                                    + (Models.Complex.cdefModel.getContentId(cpCore, "Groups") + " target=_blank>Manage Groups</a>]</span></td>")))));
                                    f.Add("</tr>");
                                    GetForm_Edit_MemberGroups = Adminui.GetEditPanel(!allowAdminTabs, "Group Membership", "This person is a member of these groups", (Adminui.EditTableOpen 
                                                    + (f.Text + Adminui.EditTableClose)));
                                    EditSectionPanelCount = (EditSectionPanelCount + 1);
                                    // TODO: Exit Function: Warning!!! Need to return the value
                                    return;
                                    // 
                                ErrorTrap:
                                    handleLegacyClassError3("GetForm_Edit_MemberGroups");
                                }
                                
                                // 
                                // ========================================================================
                                //    Special case tab for Layout records
                                // ========================================================================
                                // 
                                ((string)(GetForm_Edit_LayoutReports(((Models.Complex.cdefModel)(adminContent)), ((editRecordClass)(editRecord)))));
                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_LayoutReports")
                                // 
                                stringBuilderLegacyController FastString;
                                adminUIController Adminui = new adminUIController(cpCore);
                                // 
                                FastString = new stringBuilderLegacyController();
                                FastString.Add("<tr>");
                                FastString.Add("<td valign=\"top\" align=\"right\"> </td>");
                                FastString.Add(("<td colspan=\"2\" class=\"ccAdminEditField\" align=\"left\">" + SpanClassAdminNormal));
                                FastString.Add("<ul class=\"ccList\">");
                                FastString.Add(("<li class=\"ccListItem\"><a target=\"_blank\" href=\"/preview?layout=" 
                                                + (editRecord.id + "\">Preview this layout</A></LI>")));
                                FastString.Add("</ul>");
                                FastString.Add("</span></td></tr>");
                                GetForm_Edit_LayoutReports = Adminui.GetEditPanel(!allowAdminTabs, "Contensive Reporting", "", (Adminui.EditTableOpen 
                                                + (FastString.Text + Adminui.EditTableClose)));
                                EditSectionPanelCount = (EditSectionPanelCount + 1);
                                FastString = null;
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                                // 
                            ErrorTrap:
                                FastString = null;
                                handleLegacyClassError3("GetForm_Edit_LayoutReports");
                                // 
                                // ========================================================================
                                //    Special case tab for People records
                                // ========================================================================
                                // 
                                ((string)(GetForm_Edit_MemberReports(((Models.Complex.cdefModel)(adminContent)), ((editRecordClass)(editRecord)))));
                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_MemberReports")
                                // 
                                stringBuilderLegacyController FastString;
                                adminUIController Adminui = new adminUIController(cpCore);
                                // 
                                FastString = new stringBuilderLegacyController();
                                FastString.Add("<tr>");
                                FastString.Add("<td valign=\"top\" align=\"right\"> </td>");
                                FastString.Add(("<td colspan=\"2\" class=\"ccAdminEditField\" align=\"left\">" + SpanClassAdminNormal));
                                FastString.Add("<ul class=\"ccList\">");
                                FastString.Add(("<li class=\"ccListItem\"><a target=\"_blank\" href=\"/" 
                                                + (cpCore.serverConfig.appConfig.adminRoute + ("?" 
                                                + (RequestNameAdminForm + ("=" 
                                                + (AdminFormReports + ("&rid=3&MemberID=" 
                                                + (editRecord.id + ("&DateTo=" 
                                                + (Int(cpCore.doc.profileStartTime.ToOADate) + ("&DateFrom=" 
                                                + ((Int(cpCore.doc.profileStartTime.ToOADate) - 365) 
                                                + "\">All visits from this person</A></LI>")))))))))))));
                                FastString.Add(("<li class=\"ccListItem\"><a target=\"_blank\" href=\"/" 
                                                + (cpCore.serverConfig.appConfig.adminRoute + ("?" 
                                                + (RequestNameAdminForm + ("=" 
                                                + (AdminFormReports + ("&rid=13&MemberID=" 
                                                + (editRecord.id + ("&DateTo=" 
                                                + (Int(double.Parse(cpCore.doc.profileStartTime.ToOADate)) + ("&DateFrom=" 
                                                + (Int((double.Parse(cpCore.doc.profileStartTime.ToOADate) - 365)) + "\">All orders from this person</A></LI>")))))))))))));
                                FastString.Add("</ul>");
                                FastString.Add("</span></td></tr>");
                                GetForm_Edit_MemberReports = Adminui.GetEditPanel(!allowAdminTabs, "Contensive Reporting", "", (Adminui.EditTableOpen 
                                                + (FastString.Text + Adminui.EditTableClose)));
                                EditSectionPanelCount = (EditSectionPanelCount + 1);
                                FastString = null;
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                                // 
                            ErrorTrap:
                                FastString = null;
                                handleLegacyClassError3("GetForm_Edit_MemberReports");
                                //         '
                                //         '========================================================================
                                //         '   Print the path Rules section of the path edit form
                                //         '========================================================================
                                //         '
                                //         Private Function GetForm_Edit_PathRules(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
                                //             On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_PathRules")
                                //             '
                                //             Dim FastString As stringBuilderLegacyController
                                //             Dim Adminui As New adminUIController(cpCore)
                                //             '
                                //             FastString = New stringBuilderLegacyController
                                //             Call FastString.Add("<tr>")
                                //             Call FastString.Add("<td valign=""top"" align=""right"">" & SpanClassAdminSmall & "Groups</td>")
                                //             Call FastString.Add("<td colspan=""2"" class=""ccAdminEditField"" align=""left"">" & SpanClassAdminNormal & cpCore.html.getINputChecList2("PathRules", "Paths", editRecord.id, "Groups", "Path Rules", "PathID", "GroupID", , "Caption") & "</span></td>")
                                //             Call FastString.Add("</tr>")
                                //             'Call FastString.Add(adminui.EditTableClose)
                                //             GetForm_Edit_PathRules = Adminui.GetEditPanel((Not allowAdminTabs), "Path Permissions", "Groups that have access to this path", Adminui.EditTableOpen & FastString.Text & Adminui.EditTableClose)
                                //             EditSectionPanelCount = EditSectionPanelCount + 1
                                //             FastString = Nothing
                                //             Exit Function
                                //             '
                                // ErrorTrap:
                                //             FastString = Nothing
                                //             Call handleLegacyClassError3("GetForm_Edit_PathRules")
                                //         End Function
                                // 
                                // ========================================================================
                                //    Print the path Rules section of the path edit form
                                // ========================================================================
                                // 
                                ((string)(GetForm_Edit_PageContentBlockRules(((Models.Complex.cdefModel)(adminContent)), ((editRecordClass)(editRecord)))));
                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_PageContentBlockRules")
                                // 
                                stringBuilderLegacyController f = new stringBuilderLegacyController();
                                string GroupList;
                                string[] GroupSplit;
                                int Ptr;
                                int IDPtr;
                                int IDEndPtr;
                                int GroupID;
                                string ReportLink;
                                adminUIController Adminui = new adminUIController(cpCore);
                                // 
                                GroupList = cpCore.html.getCheckList2("PageContentBlockRules", adminContent.Name, editRecord.id, "Groups", "Page Content Block Rules", "RecordID", "GroupID", ,, "Caption", false);
                                GroupSplit = Split(GroupList, "<br >", ,, vbTextCompare);
                                for (Ptr = 0; (Ptr <= UBound(GroupSplit)); Ptr++) {
                                    GroupID = 0;
                                    IDPtr = genericController.vbInstr(1, GroupSplit[Ptr], "value=", vbTextCompare);
                                    if ((IDPtr > 0)) {
                                        IDEndPtr = genericController.vbInstr(IDPtr, GroupSplit[Ptr], ">");
                                        if ((IDEndPtr > 0)) {
                                            GroupID = genericController.EncodeInteger(GroupSplit[Ptr].Substring((IDPtr + 5), (IDEndPtr 
                                                                - (IDPtr - 6))));
                                        }
                                        
                                    }
                                    
                                    if ((GroupID > 0)) {
                                        ReportLink = ("[<a href=\"?" 
                                                    + (RequestNameAdminForm + ("=12&rid=35&recordid=" 
                                                    + (GroupID + "\" target=_blank>Group Report</a>]"))));
                                    }
                                    else {
                                        ReportLink = " ";
                                    }
                                    
                                    f.Add(("<tr>" + ("<td> </td>" + ("<td class=\"ccAdminEditField\" align=left>" 
                                                    + (SpanClassAdminNormal 
                                                    + (GroupSplit[Ptr] + ("</span></td>" + ("<td class=\"ccAdminEditField\" align=center>" 
                                                    + (ReportLink + ("</td>" + "</tr>"))))))))));
                                }
                                
                                GetForm_Edit_PageContentBlockRules = Adminui.GetEditPanel(!allowAdminTabs, "Content Blocking", "If content is blocked, select groups that have access to this content", (Adminui.EditTableOpen 
                                                + (f.Text + Adminui.EditTableClose)));
                                EditSectionPanelCount = (EditSectionPanelCount + 1);
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                                // 
                            ErrorTrap:
                                handleLegacyClassError3("GetForm_Edit_PageContentBlockRules");
                                // 
                                // ========================================================================
                                //    Print the path Rules section of the path edit form
                                // ========================================================================
                                // 
                                ((string)(GetForm_Edit_LibraryFolderRules(((Models.Complex.cdefModel)(adminContent)), ((editRecordClass)(editRecord)))));
                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_LibraryFolderRules")
                                // 
                                string Copy;
                                stringBuilderLegacyController f = new stringBuilderLegacyController();
                                string GroupList;
                                string[] GroupSplit;
                                int Ptr;
                                int IDPtr;
                                int IDEndPtr;
                                int GroupID;
                                string ReportLink;
                                adminUIController Adminui = new adminUIController(cpCore);
                                // 
                                GroupList = cpCore.html.getCheckList2("LibraryFolderRules", adminContent.Name, editRecord.id, "Groups", "Library Folder Rules", "FolderID", "GroupID", ,, "Caption");
                                GroupSplit = Split(GroupList, "<br >", ,, vbTextCompare);
                                for (Ptr = 0; (Ptr <= UBound(GroupSplit)); Ptr++) {
                                    GroupID = 0;
                                    IDPtr = genericController.vbInstr(1, GroupSplit[Ptr], "value=", vbTextCompare);
                                    if ((IDPtr > 0)) {
                                        IDEndPtr = genericController.vbInstr(IDPtr, GroupSplit[Ptr], ">");
                                        if ((IDEndPtr > 0)) {
                                            GroupID = genericController.EncodeInteger(GroupSplit[Ptr].Substring((IDPtr + 5), (IDEndPtr 
                                                                - (IDPtr - 6))));
                                        }
                                        
                                    }
                                    
                                    if ((GroupID > 0)) {
                                        ReportLink = ("[<a href=\"?" 
                                                    + (RequestNameAdminForm + ("=12&rid=35&recordid=" 
                                                    + (GroupID + "\" target=_blank>Group Report</a>]"))));
                                    }
                                    else {
                                        ReportLink = " ";
                                    }
                                    
                                    f.Add(("<tr>" + ("<td> </td>" + ("<td class=\"ccAdminEditField\" align=left>" 
                                                    + (SpanClassAdminNormal 
                                                    + (GroupSplit[Ptr] + ("</span></td>" + ("<td class=\"ccAdminEditField\" align=center>" 
                                                    + (ReportLink + ("</td>" + "</tr>"))))))))));
                                }
                                
                                Copy = "Select groups who have authoring access within this folder. This means if you are in this group you c" +
                                "an upload files, delete files, create folders and delete folders within this folder and any subfolde" +
                                "rs.";
                                GetForm_Edit_LibraryFolderRules = Adminui.GetEditPanel(!allowAdminTabs, "Folder Permissions", Copy, (Adminui.EditTableOpen 
                                                + (f.Text + Adminui.EditTableClose)));
                                EditSectionPanelCount = (EditSectionPanelCount + 1);
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                                // 
                            ErrorTrap:
                                handleLegacyClassError3("GetForm_Edit_LibraryFolderRules");
                                // 
                                // ========================================================================
                                //  Print the Group Rules section for Content Edit form
                                //    Group rules show which groups have authoring rights to a content
                                // 
                                //    adminContent.id is the ContentID of the Content Definition being edited
                                //    EditRecord.ContentID is the ContentControlID of the Edit Record
                                // ========================================================================
                                // 
                                ((string)(GetForm_Edit_GroupRules(((Models.Complex.cdefModel)(adminContent)), ((editRecordClass)(editRecord)))));
                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_GroupRules")
                                // 
                                string SQL;
                                int CS;
                                int GroupRulesCount;
                                int GroupRulesSize;
                                int GroupRulesPointer;
                                string SectionName;
                                string GroupName;
                                int GroupCount;
                                bool GroupFound;
                                GroupRuleType[] GroupRules;
                                stringBuilderLegacyController FastString;
                                adminUIController Adminui = new adminUIController(cpCore);
                                // 
                                //  ----- Open the panel
                                // 
                                FastString = new stringBuilderLegacyController();
                                // 
                                // Call cpCore.main_PrintPanelTop("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                                // Call call FastString.Add(adminui.EditTableOpen)
                                // 
                                //  ----- Gather all the groups which have authoring rights to the content
                                // 
                                GroupRulesCount = 0;
                                GroupRulesSize = 0;
                                if ((editRecord.id != 0)) {
                                    SQL = ("SELECT ccGroups.ID AS ID, ccGroupRules.AllowAdd as allowadd, ccGroupRules.AllowDelete as allowdelete" + (" FROM ccGroups LEFT JOIN ccGroupRules ON ccGroups.ID = ccGroupRules.GroupID" + (" WHERE (((ccGroupRules.ContentID)=" 
                                                + (editRecord.id + ") AND ((ccGroupRules.Active)<>0) AND ((ccGroups.Active)<>0))"))));
                                    CS = cpCore.db.csOpenSql_rev("Default", SQL);
                                    if (cpCore.db.csOk(CS)) {
                                        if (true) {
                                            GroupRulesSize = 100;
                                            object GroupRules;
                                            while (cpCore.db.csOk(CS)) {
                                                if ((GroupRulesCount >= GroupRulesSize)) {
                                                    GroupRulesSize = (GroupRulesSize + 100);
                                                    object Preserve;
                                                    GroupRules[GroupRulesSize];
                                                }
                                                
                                                GroupRules[GroupRulesCount].GroupID = cpCore.db.csGetInteger(CS, "ID");
                                                GroupRules[GroupRulesCount].AllowAdd = cpCore.db.csGetBoolean(CS, "AllowAdd");
                                                GroupRules[GroupRulesCount].AllowDelete = cpCore.db.csGetBoolean(CS, "AllowDelete");
                                                GroupRulesCount = (GroupRulesCount + 1);
                                                cpCore.db.csGoNext(CS);
                                            }
                                            
                                        }
                                        
                                    }
                                    
                                }
                                
                                cpCore.db.csClose(CS);
                                // 
                                //  ----- Gather all the groups, sorted by ContentName
                                // 
                                SQL = ("SELECT ccGroups.ID AS ID, ccContent.Name AS SectionName, ccGroups.Name AS GroupName, ccGroups.Caption" +
                                " AS GroupCaption, ccGroups.SortOrder" + (" FROM ccGroups LEFT JOIN ccContent ON ccGroups.ContentControlID = ccContent.ID" + (" Where (((ccGroups.Active) <> " 
                                            + (SQLFalse + (") And ((ccContent.Active) <> " 
                                            + (SQLFalse + ("))" + (" GROUP BY ccGroups.ID, ccContent.Name, ccGroups.Name, ccGroups.Caption, ccGroups.SortOrder" + " ORDER BY ccContent.Name, ccGroups.Caption, ccGroups.SortOrder"))))))));
                                CS = cpCore.db.csOpenSql_rev("Default", SQL);
                                if (!cpCore.db.csOk(CS)) {
                                    FastString.Add(("\r\n" + ("<tr><td colspan=\"3\">" 
                                                    + (SpanClassAdminSmall + "There are no active groups</span></td></tr>"))));
                                }
                                else if (true) {
                                    // Call FastString.Add(vbCrLf & "<tr><td colspan=""3"" class=""ccAdminEditSubHeader"">Groups with authoring access</td></tr>")
                                    SectionName = "";
                                    GroupCount = 0;
                                    while (cpCore.db.csOk(CS)) {
                                        GroupName = cpCore.db.csGet(CS, "GroupCaption");
                                        if ((GroupName == "")) {
                                            GroupName = cpCore.db.csGet(CS, "GroupName");
                                        }
                                        
                                        FastString.Add("<tr>");
                                        if ((SectionName != cpCore.db.csGet(CS, "SectionName"))) {
                                            // 
                                            //  ----- create the next section
                                            // 
                                            SectionName = cpCore.db.csGet(CS, "SectionName");
                                            FastString.Add(("<td valign=\"top\" align=\"right\">" 
                                                            + (SpanClassAdminSmall 
                                                            + (SectionName + "</td>"))));
                                        }
                                        else {
                                            FastString.Add("<td valign=\"top\" align=\"right\"> </td>");
                                        }
                                        
                                        FastString.Add(("<td class=\"ccAdminEditField\" align=\"left\" colspan=\"2\">" + SpanClassAdminSmall));
                                        GroupFound = false;
                                        if ((GroupRulesCount != 0)) {
                                            for (GroupRulesPointer = 0; (GroupRulesPointer 
                                                        <= (GroupRulesCount - 1)); GroupRulesPointer++) {
                                                if ((GroupRules[GroupRulesPointer].GroupID == cpCore.db.csGetInteger(CS, "ID"))) {
                                                    GroupFound = true;
                                                    break;
                                                }
                                                
                                            }
                                            
                                        }
                                        
                                        FastString.Add(("<input type=\"hidden\" name=\"GroupID" 
                                                        + (GroupCount + ("\" value=\"" 
                                                        + (cpCore.db.csGet(CS, "ID") + "\">")))));
                                        FastString.Add("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"400\"><tr>");
                                        if (GroupFound) {
                                            FastString.Add(("<td width=\"200\">" 
                                                            + (SpanClassAdminSmall 
                                                            + (cpCore.html.html_GetFormInputCheckBox2(("Group" + GroupCount), true) 
                                                            + (GroupName + "</span></td>")))));
                                            FastString.Add(("<td width=\"100\">" 
                                                            + (SpanClassAdminSmall 
                                                            + (cpCore.html.html_GetFormInputCheckBox2(("GroupRuleAllowAdd" + GroupCount), GroupRules[GroupRulesPointer].AllowAdd) + " Allow Add</span></td>"))));
                                            FastString.Add(("<td width=\"100\">" 
                                                            + (SpanClassAdminSmall 
                                                            + (cpCore.html.html_GetFormInputCheckBox2(("GroupRuleAllowDelete" + GroupCount), GroupRules[GroupRulesPointer].AllowDelete) + " Allow Delete</span></td>"))));
                                        }
                                        else {
                                            FastString.Add(("<td width=\"200\">" 
                                                            + (SpanClassAdminSmall 
                                                            + (cpCore.html.html_GetFormInputCheckBox2(("Group" + GroupCount), false) 
                                                            + (GroupName + "</span></td>")))));
                                            FastString.Add(("<td width=\"100\">" 
                                                            + (SpanClassAdminSmall 
                                                            + (cpCore.html.html_GetFormInputCheckBox2(("GroupRuleAllowAdd" + GroupCount), false) + " Allow Add</span></td>"))));
                                            FastString.Add(("<td width=\"100\">" 
                                                            + (SpanClassAdminSmall 
                                                            + (cpCore.html.html_GetFormInputCheckBox2(("GroupRuleAllowDelete" + GroupCount), false) + " Allow Delete</span></td>"))));
                                        }
                                        
                                        FastString.Add("</tr></table>");
                                        FastString.Add("</span></td>");
                                        FastString.Add("</tr>");
                                        GroupCount = (GroupCount + 1);
                                        cpCore.db.csGoNext(CS);
                                    }
                                    
                                    FastString.Add(("\r\n" + ("<input type=\"hidden\" name=\"GroupCount\" value=\"" 
                                                    + (GroupCount + "\">"))));
                                }
                                
                                cpCore.db.csClose(CS);
                                // 
                                //  ----- close the panel
                                // 
                                // Call FastString.Add(adminui.EditTableClose)
                                // Call cpCore.main_PrintPanelBottom("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                                // 
                                GetForm_Edit_GroupRules = Adminui.GetEditPanel(!allowAdminTabs, "Authoring Permissions", "The following groups can edit this content.", (Adminui.EditTableOpen 
                                                + (FastString.Text + Adminui.EditTableClose)));
                                EditSectionPanelCount = (EditSectionPanelCount + 1);
                                FastString = null;
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                                // 
                            ErrorTrap:
                                FastString = null;
                                handleLegacyClassError3("GetForm_Edit_GroupRules");
                                // 
                                // ========================================================================
                                //    Get all content authorable by the current group
                                // ========================================================================
                                // 
                                ((string)(GetForm_Edit_ContentGroupRules(((Models.Complex.cdefModel)(adminContent)), ((editRecordClass)(editRecord)))));
                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_ContentGroupRules")
                                // 
                                string SQL;
                                int CS;
                                int ContentGroupRulesCount;
                                int ContentGroupRulesSize;
                                int ContentGroupRulesPointer;
                                string ContentName;
                                int ContentCount;
                                bool ContentFound;
                                ContentGroupRuleType[] ContentGroupRules;
                                stringBuilderLegacyController FastString;
                                adminUIController Adminui = new adminUIController(cpCore);
                                // 
                                if (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                                    // 
                                    //  ----- Open the panel
                                    // 
                                    FastString = new stringBuilderLegacyController();
                                    // 
                                    // Call cpCore.main_PrintPanelTop("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                                    // Call call FastString.Add(adminui.EditTableOpen)
                                    // 
                                    //  ----- Gather all the groups which have authoring rights to the content
                                    // 
                                    ContentGroupRulesCount = 0;
                                    ContentGroupRulesSize = 0;
                                    if ((editRecord.id != 0)) {
                                        SQL = ("SELECT ccContent.ID AS ID, ccGroupRules.AllowAdd as allowadd, ccGroupRules.AllowDelete as allowdelete" +
                                        "" + (" FROM ccContent LEFT JOIN ccGroupRules ON ccContent.ID = ccGroupRules.ContentID" + (" WHERE (((ccGroupRules.GroupID)=" 
                                                    + (editRecord.id + ") AND ((ccGroupRules.Active)<>0) AND ((ccContent.Active)<>0))"))));
                                        CS = cpCore.db.csOpenSql_rev("Default", SQL);
                                        if (cpCore.db.csOk(CS)) {
                                            ContentGroupRulesSize = 100;
                                            object ContentGroupRules;
                                            while (cpCore.db.csOk(CS)) {
                                                if ((ContentGroupRulesCount >= ContentGroupRulesSize)) {
                                                    ContentGroupRulesSize = (ContentGroupRulesSize + 100);
                                                    object Preserve;
                                                    ContentGroupRules[ContentGroupRulesSize];
                                                }
                                                
                                                ContentGroupRules[ContentGroupRulesCount].ContentID = cpCore.db.csGetInteger(CS, "ID");
                                                ContentGroupRules[ContentGroupRulesCount].AllowAdd = cpCore.db.csGetBoolean(CS, "AllowAdd");
                                                ContentGroupRules[ContentGroupRulesCount].AllowDelete = cpCore.db.csGetBoolean(CS, "AllowDelete");
                                                ContentGroupRulesCount = (ContentGroupRulesCount + 1);
                                                cpCore.db.csGoNext(CS);
                                            }
                                            
                                        }
                                        
                                    }
                                    
                                    cpCore.db.csClose(CS);
                                    // 
                                    //  ----- Gather all the content, sorted by ContentName
                                    // 
                                    SQL = ("SELECT ccContent.ID AS ID, ccContent.Name AS ContentName, ccContent.SortOrder" + (" FROM ccContent" + (" Where ccContent.Active<>0" + " ORDER BY ccContent.Name")));
                                    CS = cpCore.db.csOpenSql_rev("Default", SQL);
                                    if (!cpCore.db.csOk(CS)) {
                                        FastString.Add(("\r\n" + ("<tr><td colspan=\"3\">" 
                                                        + (SpanClassAdminSmall + "There are no active groups</span></td></tr>"))));
                                    }
                                    else {
                                        ContentCount = 0;
                                        while (cpCore.db.csOk(CS)) {
                                            ContentName = cpCore.db.csGet(CS, "ContentName");
                                            FastString.Add("<tr>");
                                            FastString.Add("<td valign=\"top\" align=\"right\"> </td>");
                                            FastString.Add(("<td class=\"ccAdminEditField\" align=\"left\" colspan=\"2\">" + SpanClassAdminSmall));
                                            ContentFound = false;
                                            if ((ContentGroupRulesCount != 0)) {
                                                for (ContentGroupRulesPointer = 0; (ContentGroupRulesPointer 
                                                            <= (ContentGroupRulesCount - 1)); ContentGroupRulesPointer++) {
                                                    if ((ContentGroupRules[ContentGroupRulesPointer].ContentID == cpCore.db.csGetInteger(CS, "ID"))) {
                                                        ContentFound = true;
                                                        break;
                                                    }
                                                    
                                                }
                                                
                                            }
                                            
                                            FastString.Add(("<input type=\"hidden\" name=\"ContentID" 
                                                            + (ContentCount + ("\" value=\"" 
                                                            + (cpCore.db.csGet(CS, "ID") + "\">")))));
                                            FastString.Add("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"400\"><tr>");
                                            if (ContentFound) {
                                                FastString.Add(("<td width=\"200\">" 
                                                                + (SpanClassAdminSmall 
                                                                + (cpCore.html.html_GetFormInputCheckBox2(("Content" + ContentCount), true) 
                                                                + (ContentName + "</span></td>")))));
                                                FastString.Add(("<td width=\"100\">" 
                                                                + (SpanClassAdminSmall 
                                                                + (cpCore.html.html_GetFormInputCheckBox2(("ContentGroupRuleAllowAdd" + ContentCount), ContentGroupRules[ContentGroupRulesPointer].AllowAdd) + " Allow Add</span></td>"))));
                                                FastString.Add(("<td width=\"100\">" 
                                                                + (SpanClassAdminSmall 
                                                                + (cpCore.html.html_GetFormInputCheckBox2(("ContentGroupRuleAllowDelete" + ContentCount), ContentGroupRules[ContentGroupRulesPointer].AllowDelete) + " Allow Delete</span></td>"))));
                                            }
                                            else {
                                                FastString.Add(("<td width=\"200\">" 
                                                                + (SpanClassAdminSmall 
                                                                + (cpCore.html.html_GetFormInputCheckBox2(("Content" + ContentCount), false) 
                                                                + (ContentName + "</span></td>")))));
                                                FastString.Add(("<td width=\"100\">" 
                                                                + (SpanClassAdminSmall 
                                                                + (cpCore.html.html_GetFormInputCheckBox2(("ContentGroupRuleAllowAdd" + ContentCount), false) + " Allow Add</span></td>"))));
                                                FastString.Add(("<td width=\"100\">" 
                                                                + (SpanClassAdminSmall 
                                                                + (cpCore.html.html_GetFormInputCheckBox2(("ContentGroupRuleAllowDelete" + ContentCount), false) + " Allow Delete</span></td>"))));
                                            }
                                            
                                            FastString.Add("</tr></table>");
                                            FastString.Add("</span></td>");
                                            FastString.Add("</tr>");
                                            ContentCount = (ContentCount + 1);
                                            cpCore.db.csGoNext(CS);
                                        }
                                        
                                        FastString.Add(("\r\n" + ("<input type=\"hidden\" name=\"ContentCount\" value=\"" 
                                                        + (ContentCount + "\">"))));
                                    }
                                    
                                    cpCore.db.csClose(CS);
                                    // 
                                    //  ----- close the panel
                                    // 
                                    // Call FastString.Add(adminui.EditTableClose)
                                    // Call cpCore.main_PrintPanelBottom("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                                    // 
                                    GetForm_Edit_ContentGroupRules = Adminui.GetEditPanel(!allowAdminTabs, "Authoring Permissions", "This group can edit the following content.", (Adminui.EditTableOpen 
                                                    + (FastString.Text + Adminui.EditTableClose)));
                                    EditSectionPanelCount = (EditSectionPanelCount + 1);
                                    FastString = null;
                                }
                                
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                                // 
                            ErrorTrap:
                                FastString = null;
                                handleLegacyClassError3("GetForm_Edit_ContentGroupRules");
                                //         '
                                //         '========================================================================
                                //         '   Gets the fields pointer if it exists, otherwise -1
                                //         '
                                //         '   Does not report an error
                                //         '========================================================================
                                //         '
                                //         Private Function GetFieldPtrNoError(adminContent As appServices_metaDataClass.CDefClass, editRecord As editRecordClass, ByVal TargetField As String) As Integer
                                //             On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetFieldPtrNoError")
                                //             '
                                //             Dim UcaseTargetField As String
                                //             ''Dim arrayOfFields() As appServices_metaDataClass.CDefFieldClass
                                //             '
                                //             GetFieldPtrNoError = -1
                                //             UcaseTargetField = genericController.vbUCase(TargetField)
                                //             If adminContent.fields.Count > 0 Then
                                //                 arrayOfFields = adminContent.fields
                                //                 For GetFieldPtrNoError = 0 To adminContent.fields.Count - 1
                                //                     If genericController.vbUCase(arrayOfFields(GetFieldPtrNoError).Name) = UcaseTargetField Then
                                //                         Exit For
                                //                     End If
                                //                 Next
                                //                 If GetFieldPtrNoError >= adminContent.fields.Count Then
                                //                     GetFieldPtrNoError = -1
                                //                 End If
                                //             End If
                                //             Exit Function
                                //             '
                                //             ' ----- Error Trap
                                //             '
                                // ErrorTrap:
                                //             Call handleLegacyClassError3("GetFieldPtrNoError")
                                //             '
                                //         End Function
                                //         '
                                //         '========================================================================
                                //         ' Get FieldPointer from its FieldName
                                //         '   Returns -1 if not found
                                //         '========================================================================
                                //         '
                                //         Private Function GetFieldPtr(adminContent As appServices_metaDataClass.CDefClass, editRecord As editRecordClass, ByVal TargetField As String) As Integer
                                //             On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetFieldPtr")
                                //             '
                                //             GetFieldPtr = GetFieldPtrNoError(TargetField)
                                //             If GetFieldPtr = -1 Then
                                //                 Call handleLegacyClassError("AdminClass.GetFieldPtr", "Could not find content field [" & adminContent.Name & "].[" & TargetField & "]")
                                //             End If
                                //             Exit Function
                                //             '
                                //             ' ----- Error Trap
                                //             '
                                // ErrorTrap:
                                //             Call handleLegacyClassError3("GetFieldPtr")
                                //             '
                                //         End Function
                                // 
                                // ========================================================================
                                //  MakeButton
                                //    Prints the currently selected Button Type
                                //    ButtonName is the ID field name given to the button object
                                //    ButtonLabel is the words that appear on the button
                                //    ButtonHref is the Link for the button
                                //    ButtonWidth, if provided, is the width of a trans spacer.gif put under the ButtonLabel
                                //    ButtonColors, colors used for the button, duh.
                                // ========================================================================
                                // 
                                ((string)(MakeButton(((string)(ButtonName)), ((string)(ButtonLabel)), ((string)(ButtonHref)), ((string)(ButtonWidth)), ((string)(ButtonColorBase)), ((string)(ButtonColorHilite)), ((string)(ButtonColorShadow)), ((bool)(NewWindow)))));
                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                // 'Dim th as integer : th = profileLogAdminMethodEnter("MakeButton")
                                // 
                                MakeButton = "";
                                MakeButton = (MakeButton + MakeButtonFlat(ButtonName, ButtonLabel, ButtonHref, ButtonWidth, ButtonColorBase, ButtonColorHilite, ButtonColorShadow, NewWindow));
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                                // 
                                //  ----- Error Trap
                                // 
                            ErrorTrap:
                                handleLegacyClassError3("MakeButton");
                                // 
                                // 
                                // ========================================================================
                                //  MakeButtonFlat
                                //    Returns a Flat button string
                                //    Button is a normal color, rollover changes background color only
                                // ========================================================================
                                // 
                                ((string)(MakeButtonFlat(((string)(ButtonName)), ((string)(ButtonLabel)), ((string)(ButtonHref)), ((string)(ButtonWidth)), ((string)(ButtonColorBase)), ((string)(ButtonColorHilite)), ((string)(ButtonColorShadow)), ((bool)(NewWindow)))));
                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                // 'Dim th as integer : th = profileLogAdminMethodEnter("MakeButtonFlat")
                                // 
                                bool IncludeWidth;
                                // 
                                MakeButtonFlat = "";
                                MakeButtonFlat = (MakeButtonFlat + ("<div" + (" ID=\"" 
                                            + (ButtonName + ("\"" + (" class=\"ccAdminButton\"" + ">"))))));
                                IncludeWidth = false;
                                if ((ButtonWidth != "")) {
                                    if (genericController.vbIsNumeric(ButtonWidth)) {
                                        IncludeWidth = true;
                                    }
                                    
                                }
                                
                                // 
                                //  --- put guts in layer so Netscape can change colors (with mouseover and mouseout)
                                // 
                                MakeButtonFlat = (MakeButtonFlat + ("<a" + (" href=\"" 
                                            + (ButtonHref + ("\"" + (" class=\"ccAdminButton\"" + ""))))));
                                if (NewWindow) {
                                    MakeButtonFlat = (MakeButtonFlat + " target=\"_blank\"");
                                }
                                
                                MakeButtonFlat = (MakeButtonFlat + ">");
                                MakeButtonFlat = (MakeButtonFlat 
                                            + (ButtonLabel + "</A>"));
                                if (IncludeWidth) {
                                    MakeButtonFlat = (MakeButtonFlat + ("<br ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"" 
                                                + (ButtonWidth + "\" height=\"1\" >")));
                                }
                                
                                // 
                                //  --- close table
                                // 
                                MakeButtonFlat = (MakeButtonFlat + "</div>");
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                                // 
                                //  ----- Error Trap
                                // 
                            ErrorTrap:
                                handleLegacyClassError3("MakeButtonFlat");
                                // 
                                // '
                                // '========================================================================
                                // ' GetMenuLeftMode()
                                // '   Prints the menu section of the admin page
                                // '========================================================================
                                // '
                                // Private Function deprecate_menu_getLeftMode() As String
                                //     Dim returnString As String = ""
                                //     Try
                                //         '
                                //         Const MenuEntryContentName = cnNavigatorEntries
                                //         '
                                //         Dim HeaderNameCurrent As String
                                //         Dim MenuName As String
                                //         Dim MenuID As Integer
                                //         Dim MenuPage As String
                                //         Dim MenuContentID As Integer
                                //         Dim MenuNewWindow As Boolean
                                //         Dim MenuItemCount As Integer
                                //         Dim CS As Integer
                                //         Dim Panel As String
                                //         Dim ContentManagementList As New List(Of Integer)
                                //         Dim IsAdminLocal As Boolean
                                //         '
                                //         ' Start the menu panel
                                //         '
                                //         Panel = "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">"
                                //         Panel = Panel & "<tr><td width=""10""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""10"" height=""1"" ></td><td width=""100%""></td></tr>"
                                //         '
                                //         ' --- Load CCMenu
                                //         '
                                //         CS = GetMenuCSPointer("(ccMenuEntries.ParentID is null)or(ccMenuEntries.ParentID=0)", MenuEntryContentName)
                                //         If cpCore.db.csOk(CS) Then
                                //             IsAdminLocal = cpCore.doc.authContext.user.user_isAdmin
                                //             If Not IsAdminLocal Then
                                //                 ContentManagementList.AddRange(cpCore.metaData.getEditableCdefIdList())
                                //             End If
                                //             HeaderNameCurrent = ""
                                //             MenuItemCount = 0
                                //             Do While cpCore.db.csOk(CS)
                                //                 MenuName = cpCore.db.cs_get(CS, "Name")
                                //                 MenuPage = cpCore.db.cs_get(CS, "LinkPage")
                                //                 MenuContentID = cpCore.db.cs_getInteger(CS, "ContentID")
                                //                 MenuNewWindow = cpCore.db.cs_getBoolean(CS, "NewWindow")
                                //                 MenuID = cpCore.db.cs_getInteger(CS, "ID")
                                //                 HeaderNameCurrent = MenuName
                                //                 '
                                //                 ' --- new header
                                //                 '
                                //                 If MenuItemCount <> 0 Then
                                //                     Panel = Panel & "<tr><td colspan=""2""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""4"" ></td></tr>"
                                //                     Panel = Panel & "<tr><td colspan=""2"" class=""ccPanelShadow""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td></tr>"
                                //                     Panel = Panel & "<tr><td colspan=""2"" class=""ccPanelHilite""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td></tr>"
                                //                     Panel = Panel & "<tr><td colspan=""2""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""4"" ></td></tr>"
                                //                 End If
                                //                 Panel = Panel & "<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>" & MenuName & "</b></span></td></tr>"
                                //                 MenuItemCount = MenuItemCount + 1
                                //                 Panel = Panel & deprecate_menu_getLeftModeBranch(MenuID, "", ContentManagementList, IsAdminLocal, MenuEntryContentName)
                                //                 Call cpCore.db.csGoNext(CS)
                                //             Loop
                                //         End If
                                //         Call cpCore.db.csClose(CS)
                                //         '
                                //         ' Close the menu panel
                                //         '
                                //         Panel = Panel & "<tr><td width=""10""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""10"" height=""1"" ></td><td width=""1""></td><td width=""100%""></td><td width=""1""></td></tr>"
                                //         Panel = Panel & "</table>"
                                //         deprecate_menu_getLeftMode = cpcore.htmldoc.main_GetPanel(Panel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "150", 10)
                                //     Catch ex As Exception
                                //         cpCore.handleExceptionAndContinue(ex) : Throw
                                //     End Try
                                //     Return returnString
                                // End Function
                                //         '
                                //         '========================================================================
                                //         ' GetMenuLeftModeBranch()
                                //         '   Prints the menu section of the admin page
                                //         '========================================================================
                                //         '
                                //         Private Function deprecate_menu_getLeftModeBranch(ByVal ParentID As Integer, ByVal ParentHeaderName As String, ByVal ContentManagementList As List(Of Integer), ByVal IsAdminLocal As Boolean, ByVal MenuEntryContentName As String) As String
                                //             On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetMenuLeftModeBranch")
                                //             '
                                //             Dim HeaderName As String
                                //             Dim HeaderNameCurrent As String
                                //             Dim MenuName As String
                                //             Dim MenuID As Integer
                                //             Dim MenuPage As String
                                //             Dim MenuContentID As Integer
                                //             Dim MenuNewWindow As Boolean
                                //             Dim ButtonObject As String
                                //             Dim MenuItemCount As Integer
                                //             Dim SQL As String
                                //             Dim CS As Integer
                                //             Dim ImageID As Integer
                                //             Dim ButtonGuts As String = ""
                                //             Dim ButtonHref As String
                                //             Dim ParentIDCurrent As Integer
                                //             Dim MenuNameDisplay As String
                                //             '
                                //             ' --- Load CCMenu
                                //             '
                                //             CS = GetMenuCSPointer("(ccMenuEntries.ParentID=" & ParentID & ")", MenuEntryContentName)
                                //             'SQL = GetMenuSQLNew("(ccMenuEntries.ParentID=" & parentid & ")")
                                //             'CS = cpCore.app_openCsSql_Rev_Internal("Default", SQL)
                                //             If cpCore.db.csOk(CS) Then
                                //                 HeaderNameCurrent = ""
                                //                 MenuItemCount = 0
                                //                 Do While cpCore.db.csOk(CS)
                                //                     MenuName = cpCore.db.cs_get(CS, "Name")
                                //                     MenuPage = cpCore.db.cs_get(CS, "LinkPage")
                                //                     MenuContentID = cpCore.db.cs_getInteger(CS, "ContentID")
                                //                     MenuNewWindow = cpCore.db.cs_getBoolean(CS, "NewWindow")
                                //                     MenuID = cpCore.db.cs_getInteger(CS, "ID")
                                //                     If ParentHeaderName = "" Then
                                //                         MenuNameDisplay = MenuName
                                //                     Else
                                //                         MenuNameDisplay = ParentHeaderName & ":" & MenuName
                                //                     End If
                                //                     If (IsAdminLocal) Or ((MenuPage <> "") Or ((MenuContentID > 0) And (ContentManagementList.Contains(MenuContentID)))) Then
                                //                         'If ((MenuPage <> "") Or (MenuContentID > 0)) Then
                                //                         '
                                //                         ' output the button
                                //                         '
                                //                         If MenuName = "" Then
                                //                             MenuName = "[Link]"
                                //                         End If
                                //                         If MenuPage = "" Then
                                //                             MenuPage = cpCore.siteProperties.serverPageDefault
                                //                         End If
                                //                         If MenuContentID > 0 Then
                                //                             MenuPage = modifyLinkQuery(MenuPage, "cid", CStr(MenuContentID), True)
                                //                             'MenuPage = MenuPage & "&cid=" & MenuContentID
                                //                         End If
                                //                         ButtonHref = MenuPage
                                //                         If MenuNewWindow Then
                                //                             ButtonGuts = ButtonGuts & " target=""_blank"""
                                //                         End If
                                //                         ButtonGuts = SpanClassAdminNormal & MenuNameDisplay & "</span>"
                                //                         ' 9-28-02 ButtonGuts = SpanClassAdminNormal & "<nobr>" & MenuNameDisplay & "</nobr></span>"
                                //                         ButtonObject = "Button" & ButtonObjectCount
                                //                         ButtonObjectCount = ButtonObjectCount + 1
                                //                         deprecate_menu_getLeftModeBranch = deprecate_menu_getLeftModeBranch & "<tr><td></td><td>" & MakeButton(ButtonObject, ButtonGuts, ButtonHref, "150", "ccPanel", "ccPanelHilite", "ccPanelShadow", MenuNewWindow) & "</td></tr>"
                                //                     End If
                                //                     MenuItemCount = MenuItemCount + 1
                                //                     deprecate_menu_getLeftModeBranch = deprecate_menu_getLeftModeBranch & deprecate_menu_getLeftModeBranch(MenuID, MenuNameDisplay, ContentManagementList, IsAdminLocal, MenuEntryContentName)
                                //                     Call cpCore.db.csGoNext(CS)
                                //                 Loop
                                //             End If
                                //             Call cpCore.db.csClose(CS)
                                //             Exit Function
                                //             '
                                //             ' ----- Error Trap
                                //             '
                                // ErrorTrap:
                                //             Call handleLegacyClassError3("GetMenuLeftModeBranch")
                                //             '
                                //         End Function
                                //         '
                                //         '========================================================================
                                //         ' GetMenuLeftMode()
                                //         '   Prints the menu section of the admin page
                                //         '========================================================================
                                //         '
                                //         Private Function menu_getLeftModeOld(ByVal MenuEntryContentName As String) As String
                                //             On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetMenuLeftModeOld")
                                //             '
                                //             Dim HeaderName As String
                                //             Dim HeaderNameCurrent As String
                                //             Dim MenuName As String
                                //             Dim MenuID As Integer
                                //             Dim MenuPage As String
                                //             Dim MenuContentID As Integer
                                //             Dim MenuNewWindow As Boolean
                                //             Dim ButtonObject As String
                                //             Dim MenuItemCount As Integer
                                //             Dim SQL As String
                                //             Dim CS As Integer
                                //             Dim ImageID As Integer
                                //             Dim Panel As String
                                //             Dim ButtonGuts As String
                                //             Dim ButtonHref As String
                                //             Dim ParentID As Integer
                                //             Dim ParentIDCurrent As Integer
                                //             '
                                //             ' --- Left Menu Mode
                                //             '
                                //             If AdminMenuModeID = AdminMenuModeLeft Then
                                //                 CS = GetMenuCSPointer("", MenuEntryContentName)
                                //                 'SQL = GetMenuSQLNew()
                                //                 ''
                                //                 '' --- Load CCMenu
                                //                 ''
                                //                 'CS = cpCore.app_openCsSql_Rev_Internal("Default", SQL)
                                //                 If cpCore.db.csOk(CS) Then
                                //                     HeaderNameCurrent = ""
                                //                     MenuItemCount = 0
                                //                     Panel = "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">"
                                //                     Panel = Panel & "<tr><td width=""10""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""10"" height=""1"" ></td><td width=""100%""></td></tr>"
                                //                     Do While cpCore.db.csOk(CS)
                                //                         ParentID = cpCore.db.cs_getInteger(CS, "ParentID")
                                //                         'HeaderName = cpCore.app.cs_get(CS, "HeaderName")
                                //                         MenuName = cpCore.db.cs_get(CS, "Name")
                                //                         MenuPage = cpCore.db.cs_get(CS, "LinkPage")
                                //                         MenuContentID = cpCore.db.cs_getInteger(CS, "ContentID")
                                //                         MenuNewWindow = cpCore.db.cs_getBoolean(CS, "NewWindow")
                                //                         MenuID = cpCore.db.cs_getInteger(CS, "ID")
                                //                         '
                                //                         ' --- draw menu line
                                //                         '
                                //                         If ParentID = 0 Then
                                //                             HeaderNameCurrent = MenuName
                                //                             '
                                //                             ' --- new header
                                //                             '
                                //                             'cpCore.writeAltBufferComment ("Menu new header")
                                //                             If MenuItemCount <> 0 Then
                                //                                 Panel = Panel & "<tr><td colspan=""2""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""4"" ></td></tr>"
                                //                                 Panel = Panel & "<tr><td colspan=""2"" class=""ccPanelShadow""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td></tr>"
                                //                                 Panel = Panel & "<tr><td colspan=""2"" class=""ccPanelHilite""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td></tr>"
                                //                                 Panel = Panel & "<tr><td colspan=""2""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""4"" ></td></tr>"
                                //                             End If
                                //                             Panel = Panel & "<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>" & MenuName & "</b></span></td></tr>"
                                //                         End If
                                //                         If ((MenuPage <> "") Or (MenuContentID > 0)) Then
                                //                             '
                                //                             ' output the button
                                //                             '
                                //                             If MenuName = "" Then
                                //                                 MenuName = "[Link]"
                                //                             End If
                                //                             If MenuPage = "" Then
                                //                                 MenuPage = cpCore.siteProperties.serverPageDefault
                                //                             End If
                                //                             If genericController.vbInstr(MenuPage, "?") = 0 Then
                                //                                 MenuPage = MenuPage & "?s=0"
                                //                                 'Else
                                //                                 '    MenuPage = MenuPage
                                //                             End If
                                //                             If MenuContentID > 0 Then
                                //                                 MenuPage = MenuPage & "&cid=" & MenuContentID
                                //                             End If
                                //                             ButtonHref = MenuPage
                                //                             If MenuNewWindow Then
                                //                                 ' ButtonGuts = ButtonGuts & " target=""_blank"""
                                //                             End If
                                //                             ButtonGuts = SpanClassAdminNormal & MenuName & "</span>"
                                //                             ButtonObject = "Button" & ButtonObjectCount
                                //                             ButtonObjectCount = ButtonObjectCount + 1
                                //                             Panel = Panel & "<tr><td></td><td>" & MakeButton(ButtonObject, ButtonGuts, ButtonHref, "150", "ccPanel", "ccPanelHilite", "ccPanelShadow", MenuNewWindow) & "</td></tr>"
                                //                         End If
                                //                         MenuItemCount = MenuItemCount + 1
                                //                         Call cpCore.db.csGoNext(CS)
                                //                     Loop
                                //                     Panel = Panel & "<tr><td width=""10""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""10"" height=""1"" ></td><td width=""1""></td><td width=""100%""></td><td width=""1""></td></tr>"
                                //                     Panel = Panel & "</table>"
                                //                     menu_getLeftModeOld = cpcore.htmldoc.main_GetPanel(Panel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "150", 10)
                                //                 End If
                                //             End If
                                //             Exit Function
                                //             '
                                //             ' ----- Error Trap
                                //             '
                                // ErrorTrap:
                                //             Call handleLegacyClassError3("GetMenuLeftMode")
                                //             '
                                //         End Function
                                // 
                                // ========================================================================
                                //  GetForm_Top
                                //    Prints the admin page before the content form window.
                                //    After this, print the content window, then PrintFormBottom()
                                // ========================================================================
                                // 
                                ((string)(GetForm_Top(Optional, BackgroundColorAsString=)));
                                string return_formTop = "";
                                try {
                                    const object AdminNavigatorGuid = "{5168964F-B6D2-4E9F-A5A8-BB1CF908A2C9}";
                                    string AdminNavFull;
                                    stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                                    string LeftSide;
                                    string RightSide;
                                    string QS;
                                    adminUIController Adminui = new adminUIController(cpCore);
                                    // 
                                    //  create the with-menu version
                                    // 
                                    LeftSide = cpCore.siteProperties.getText("AdminHeaderHTML", "Contensive Administration Site");
                                    RightSide = (cpCore.doc.profileStartTime + " ");
                                    QS = cpCore.doc.refreshQueryString;
                                    if (allowAdminTabs) {
                                        QS = genericController.ModifyQueryString(QS, "tabs", "0", true);
                                        RightSide = (RightSide + getActiveImage((cpCore.serverConfig.appConfig.adminRoute + ("?" + QS)), "Disable Tabs", "LibButtonNoTabs.GIF", "LibButtonNoTabsRev.GIF", "Disable Tabs", "16", "16", "", "", ""));
                                    }
                                    else {
                                        QS = genericController.ModifyQueryString(QS, "tabs", "1", true);
                                        RightSide = (RightSide + getActiveImage((cpCore.serverConfig.appConfig.adminRoute + ("?" + QS)), "Enable Tabs", "LibButtonTabs.GIF", "LibButtonTabsRev.GIF", "Enable Tabs", "16", "16", "", "", ""));
                                    }
                                    
                                    // 
                                    //  Menu Mode
                                    // 
                                    QS = cpCore.doc.refreshQueryString;
                                    if ((MenuDepth == 0)) {
                                        RightSide = (RightSide + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"1\" height=\"16\" >");
                                        if ((AdminMenuModeID == AdminMenuModeTop)) {
                                            QS = genericController.ModifyQueryString(QS, "mm", "1", true);
                                            RightSide = (RightSide + getActiveImage((cpCore.serverConfig.appConfig.adminRoute + ("?" + QS)), "Use Navigator", "LibButtonMenuTop.GIF", "LibButtonMenuTopOver.GIF", "Use Navigator", "16", "16", "", "", ""));
                                        }
                                        else {
                                            QS = genericController.ModifyQueryString(QS, "mm", "2", true);
                                            RightSide = (RightSide + getActiveImage((cpCore.serverConfig.appConfig.adminRoute + ("?" + QS)), "Use Dropdown Menus", "LibButtonMenuLeft.GIF", "LibButtonMenuLeftOver.GIF", "Use Dropdown Menus", "16", "16", "", "", ""));
                                        }
                                        
                                    }
                                    
                                    // 
                                    //  Refresh Button
                                    // 
                                    RightSide = (RightSide + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"1\" height=\"16\" >");
                                    RightSide = (RightSide + getActiveImage((cpCore.serverConfig.appConfig.adminRoute + ("?" + cpCore.doc.refreshQueryString)), "Refresh", "LibButtonRefresh.GIF", "LibButtonRefreshOver.GIF", "Refresh", "16", "16", "", "", ""));
                                    // 
                                    //  Assemble header
                                    // 
                                    Stream.Add(Adminui.GetHeader(LeftSide, RightSide));
                                    // 
                                    //  Menuing
                                    // 
                                    if (((MenuDepth == 0) 
                                                && (AdminMenuModeID == AdminMenuModeTop))) {
                                        Stream.Add(GetMenuTopMode());
                                    }
                                    
                                    // 
                                    //  --- Rule to separate content
                                    // 
                                    Stream.Add((cr + "<div style=\"border-top:1px solid white;border-bottom:1px solid black;height:2px\"><img alt=\"space\" src" +
                                        "=\"/ccLib/images/spacer.gif\" width=1 height=1></div>"));
                                    // 
                                    //  --- Content Definition
                                    // 
                                    AdminFormBottom = "";
                                    if (!((MenuDepth == 0) 
                                                && (AdminMenuModeID == AdminMenuModeLeft))) {
                                        // 
                                        //  #Content is full width, no Navigator
                                        // 
                                        Stream.Add((cr + "<div id=\"desktop\" class=\"ccContentCon\">"));
                                        // Stream.Add( "<div id=""ccContentCon"">")
                                        AdminFormBottom = (AdminFormBottom 
                                                    + (cr + "</div>"));
                                    }
                                    else {
                                        // 
                                        //  -- Admin Navigator
                                        AdminNavFull = cpCore.addon.execute(addonModel.create(cpCore, AdminNavigatorGuid), new BaseClasses.CPUtilsBaseClass.addonExecuteContext(), With, {, ., addonType=BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, ., errorCaption=Admin Navigator);
                                        // AdminNavFull = cpCore.addon.execute_legacy4(AdminNavigatorGuid)
                                        Stream.Add(("" 
                                                        + (cr + ("<table border=0 cellpadding=0 cellspacing=0><tr>" 
                                                        + (cr + ("<td class=\"ccToolsCon\" valign=top>" 
                                                        + (genericController.htmlIndent(AdminNavFull) 
                                                        + (cr + ("</td>" 
                                                        + (cr + "<td id=\"desktop\" class=\"ccContentCon\" valign=top>"))))))))));
                                        AdminFormBottom = (AdminFormBottom + "</td></tr></table>");
                                    }
                                    
                                    // 
                                    return_formTop = Stream.Text;
                                }
                                catch (Exception ex) {
                                    cpCore.handleException(ex);
                                    throw;
                                }
                                
                                return return_formTop;
                                // 
                                // ========================================================================
                                //  Create a string with an admin style button
                                // ========================================================================
                                // 
                                ((string)(getActiveImage(((string)(HRef)), ((string)(StatusText)), ((string)(Image)), ((string)(ImageOver)), ((string)(AltText)), ((string)(Width)), ((string)(Height)), ((string)(BGColor)), ((string)(BGColorOver)), ((string)(OnClick)))));
                                string result = "";
                                try {
                                    string ButtonObject = ("Button" + ButtonObjectCount);
                                    ButtonObjectCount = (ButtonObjectCount + 1);
                                    // 
                                    //  ----- Output the button image
                                    // 
                                    string Panel = "";
                                    if ((HRef != "")) {
                                        Panel = (Panel + ("<a href=\"" 
                                                    + (HRef + "\" ")));
                                        if ((OnClick != "")) {
                                            Panel = (Panel + (" onclick=\"" 
                                                        + (OnClick + "\"")));
                                        }
                                        
                                        Panel = (Panel + (" onmouseOver=\"" + (" document[\'" 
                                                    + (ButtonObject + ("\'].imgRolln=document[\'" 
                                                    + (ButtonObject + ("\'].src;" + (" document[\'" 
                                                    + (ButtonObject + ("\'].src=document[\'" 
                                                    + (ButtonObject + ("\'].lowsrc;" + (" window.status=\'" 
                                                    + (StatusText + ("\';" + " return true;\"")))))))))))))));
                                        Panel = (Panel + (" onmouseOut=\"" + (" document[\'" 
                                                    + (ButtonObject + ("\'].src=document[\'" 
                                                    + (ButtonObject + ("\'].imgRolln;" + (" window.status=\'\';" + " return true;\">"))))))));
                                    }
                                    
                                    Panel = (Panel + ("<img" + (" src=\"/ccLib/images/" 
                                                + (Image + ("\"" + (" alt=\"" 
                                                + (AltText + ("\"" + (" title=\"" 
                                                + (AltText + ("\"" + (" id=\"" 
                                                + (ButtonObject + ("\"" + (" name=\"" 
                                                + (ButtonObject + ("\"" + (" lowsrc=\"/ccLib/images/" 
                                                + (ImageOver + ("\"" + (" border=0" + (" width=\"" 
                                                + (Width + ("\"" + (" height=\"" 
                                                + (Height + "\" >"))))))))))))))))))))))))));
                                    if ((HRef != "")) {
                                        Panel = (Panel + "</A>");
                                    }
                                    
                                    result = Panel;
                                }
                                catch (Exception ex) {
                                    cpCore.handleException(ex);
                                    throw;
                                }
                                
                                return result;
                                // '
                                // '========================================================================
                                // '   Preload an image, returns object
                                // '========================================================================
                                // '
                                // Private Function PreloadImage(Image As String) As String
                                //     On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.PreloadImage")
                                //     '
                                //     Dim ImageFound As Boolean
                                //     Dim ImagePreloadPointer as integer
                                //     '
                                //     ImageFound = False
                                //     If ImagePreloadCount > 0 Then
                                //         For ImagePreloadPointer = 0 To ImagePreloadCount
                                //             If ImagePreloads(0, ImagePreloadPointer) = Image Then
                                //                 ImageFound = True
                                //                 PreloadImage = ImagePreloads(0, ImagePreloadPointer)
                                //                 Exit For
                                //                 End If
                                //             Next
                                //         End If
                                //     If Not ImageFound Then
                                //         If ImagePreloadCount = 0 Then
                                //             JavaScriptString = JavaScriptString & vbCrLf & "PreloadImages = new Array(); "
                                //             End If
                                //         '
                                //         PreloadImage = "Image" & ImagePreloadCount
                                //         ImagePreloads(0, ImagePreloadCount) = Image
                                //         ImagePreloads(1, ImagePreloadCount) = PreloadImage
                                //         ImagePreloadCount = ImagePreloadCount + 1
                                //         '
                                //         JavaScriptString = JavaScriptString & vbCrLf & "PreloadImages['" & PreloadImage & "'] = new Image(); "
                                //         JavaScriptString = JavaScriptString & vbCrLf & "PreloadImages['" & PreloadImage & "'].src = '/ccLib/images/" & Image & "'; "
                                //         End If
                                //     '''Dim th as integer: Exit Function
                                //     '
                                //     ' ----- Error Trap
                                //     '
                                // ErrorTrap:
                                //     Call HandleClassTrapErrorBubble("PreloadImage")
                                //     '
                                // End Function
                                // 
                                // ========================================================================
                                //  Get sql for menu
                                //    if MenuContentName is blank, it will select values from either cdef
                                // ========================================================================
                                // 
                                ((string)(GetMenuSQL(((string)(ParentCriteria)), ((string)(MenuContentName)))));
                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                // 'Dim th as integer : th = profileLogAdminMethodEnter("GetMenuSQL")
                                // 
                                string iParentCriteria;
                                string Criteria;
                                string SQL;
                                string ContentControlCriteria;
                                string SelectList;
                                List<int> editableCdefIdList;
                                Criteria = "(Active<>0)";
                                if ((MenuContentName != "")) {
                                    // ContentControlCriteria = cpCore.csv_GetContentControlCriteria(MenuContentName)
                                    Criteria = (Criteria + ("AND" + Models.Complex.cdefModel.getContentControlCriteria(cpCore, MenuContentName)));
                                }
                                
                                iParentCriteria = genericController.encodeEmptyText(ParentCriteria, "");
                                if (cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore)) {
                                    // 
                                    //  ----- Developer
                                    // 
                                }
                                else if (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                                    // 
                                    //  ----- Administrator
                                    // 
                                    Criteria = (Criteria + ("AND((DeveloperOnly is null)or(DeveloperOnly=0))" + ("AND(ID in (" + (" SELECT AllowedEntries.ID" + (" FROM CCMenuEntries AllowedEntries LEFT JOIN ccContent ON AllowedEntries.ContentID = ccContent.ID" + (" Where ((ccContent.Active<>0)And((ccContent.DeveloperOnly is null)or(ccContent.DeveloperOnly=0)))" + ("OR(ccContent.ID Is Null)" + "))")))))));
                                }
                                else {
                                    // 
                                    //  ----- Content Manager
                                    // 
                                    string CMCriteria;
                                    editableCdefIdList = Models.Complex.cdefModel.getEditableCdefIdList(cpCore);
                                    if ((editableCdefIdList.Count == 0)) {
                                        CMCriteria = "(1=0)";
                                    }
                                    else if ((editableCdefIdList.Count == 1)) {
                                        CMCriteria = ("(ccContent.ID=" 
                                                    + (editableCdefIdList[0] + ")"));
                                    }
                                    else {
                                        CMCriteria = ("(ccContent.ID in (" 
                                                    + (string.Join(",", editableCdefIdList) + "))"));
                                    }
                                    
                                    Criteria = (Criteria + ("AND((DeveloperOnly is null)or(DeveloperOnly=0))" + ("AND((AdminOnly is null)or(AdminOnly=0))" + ("AND(ID in (" + (" SELECT AllowedEntries.ID" + (" FROM CCMenuEntries AllowedEntries LEFT JOIN ccContent ON AllowedEntries.ContentID = ccContent.ID" + (" Where (" 
                                                + (CMCriteria + ("and(ccContent.Active<>0)And((ccContent.DeveloperOnly is null)or(ccContent.DeveloperOnly=0))And((ccCon" +
                                                "tent.AdminOnly is null)or(ccContent.AdminOnly=0)))" + ("OR(ccContent.ID Is Null)" + "))"))))))))));
                                }
                                
                                if ((iParentCriteria != "")) {
                                    Criteria = ("(" 
                                                + (iParentCriteria + (")AND" + Criteria)));
                                }
                                
                                SelectList = @"ccMenuEntries.contentcontrolid, ccMenuEntries.Name, ccMenuEntries.ID, ccMenuEntries.LinkPage, ccMenuEntries.ContentID, ccMenuEntries.NewWindow, ccMenuEntries.ParentID, ccMenuEntries.AddonID, ccMenuEntries.NavIconType, ccMenuEntries.NavIconTitle, HelpAddonID,HelpCollectionID,0 as collectionid";
                                GetMenuSQL = ("select " 
                                            + (SelectList + (" from ccMenuEntries where " 
                                            + (Criteria + " order by ccMenuEntries.Name"))));
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                                // 
                                //  ----- Error Trap
                                // 
                            ErrorTrap:
                                handleLegacyClassError3("GetMenuSQL");
                                // 
                                // 
                                // ========================================================================
                                //  Get sql for menu
                                // ========================================================================
                                // 
                                ((int)(GetMenuCSPointer(((string)(ParentCriteria)))));
                                // 
                                string iParentCriteria;
                                iParentCriteria = genericController.encodeEmptyText(ParentCriteria, "");
                                if ((iParentCriteria != "")) {
                                    iParentCriteria = ("(" 
                                                + (iParentCriteria + ")"));
                                }
                                
                                GetMenuCSPointer = cpCore.db.csOpenSql_rev("default", GetMenuSQL(iParentCriteria, cnNavigatorEntries));
                                // 
                                // ========================================================================
                                //  Get Menu Link
                                // ========================================================================
                                // 
                                ((string)(GetMenuLink(((string)(LinkPage)), ((int)(LinkCID)))));
                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                // 'Dim th as integer : th = profileLogAdminMethodEnter("GetMenuLink")
                                // 
                                int ContentID;
                                // 
                                if (((LinkPage != "") 
                                            || (LinkCID != 0))) {
                                    GetMenuLink = LinkPage;
                                    if ((GetMenuLink != "")) {
                                        if (((GetMenuLink.Substring(0, 1) == "?") 
                                                    || (GetMenuLink.Substring(0, 1) == "#"))) {
                                            GetMenuLink = ("/" 
                                                        + (cpCore.serverConfig.appConfig.adminRoute + GetMenuLink));
                                        }
                                        
                                    }
                                    else {
                                        GetMenuLink = ("/" + cpCore.serverConfig.appConfig.adminRoute);
                                    }
                                    
                                    ContentID = genericController.EncodeInteger(LinkCID);
                                    if ((ContentID != 0)) {
                                        GetMenuLink = genericController.modifyLinkQuery(GetMenuLink, "cid", ContentID.ToString(), true);
                                    }
                                    
                                }
                                
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                                // 
                                //  ----- Error Trap
                                // 
                            ErrorTrap:
                                handleLegacyClassError3("GetMenuLink");
                                // 
                                // 
                                // 
                                // 
                                ProcessForms(((Models.Complex.cdefModel)(adminContent)), ((editRecordClass)(editRecord)));
                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                // Dim th as integer
                                // th = profileLogAdminMethodEnter("ProcessForms")
                                // 
                                // Dim innovaEditor As innovaEditorAddonClassFPO
                                int StyleSN;
                                int ContentID;
                                indexConfigClass IndexConfig;
                                int CS;
                                string EditorStyleRulesFilename;
                                // 
                                if ((AdminSourceForm != 0)) {
                                    switch (AdminSourceForm) {
                                        case AdminFormReports:
                                            // 
                                            //  Reports form cancel button
                                            // 
                                            switch (AdminButton) {
                                                case ButtonCancel:
                                                    AdminAction = AdminActionNop;
                                                    AdminForm = AdminFormRoot;
                                                    break;
                                            }
                                            break;
                                        case AdminFormQuickStats:
                                            switch (AdminButton) {
                                                case ButtonCancel:
                                                    AdminAction = AdminActionNop;
                                                    AdminForm = AdminFormRoot;
                                                    break;
                                            }
                                            break;
                                        case AdminFormPublishing:
                                            // 
                                            //  Publish Form
                                            // 
                                            switch (AdminButton) {
                                                case ButtonCancel:
                                                    AdminAction = AdminActionNop;
                                                    AdminForm = AdminFormRoot;
                                                    break;
                                            }
                                            break;
                                        case AdminFormIndex:
                                            switch (AdminButton) {
                                                case ButtonCancel:
                                                    AdminAction = AdminActionNop;
                                                    AdminForm = AdminFormRoot;
                                                    adminContent.Id = 0;
                                                    break;
                                                case ButtonClose:
                                                    AdminAction = AdminActionNop;
                                                    AdminForm = AdminFormRoot;
                                                    adminContent.Id = 0;
                                                    break;
                                                case ButtonAdd:
                                                    AdminAction = AdminActionNop;
                                                    AdminForm = AdminFormEdit;
                                                    break;
                                                case ButtonFind:
                                                    AdminAction = AdminActionFind;
                                                    AdminForm = AdminSourceForm;
                                                    break;
                                                case ButtonFirst:
                                                    RecordTop = 0;
                                                    AdminForm = AdminSourceForm;
                                                    break;
                                                case ButtonPrevious:
                                                    RecordTop = (RecordTop - RecordsPerPage);
                                                    if ((RecordTop < 0)) {
                                                        RecordTop = 0;
                                                    }
                                                    
                                                    AdminAction = AdminActionNop;
                                                    AdminForm = AdminSourceForm;
                                                    break;
                                                case ButtonNext:
                                                    AdminAction = AdminActionNext;
                                                    AdminForm = AdminSourceForm;
                                                    break;
                                                case ButtonDelete:
                                                    AdminAction = AdminActionDeleteRows;
                                                    AdminForm = AdminSourceForm;
                                                    break;
                                            }
                                            //  end case
                                            break;
                                        case AdminFormEdit:
                                            // 
                                            //  Edit Form
                                            // 
                                            switch (AdminButton) {
                                                case ButtonRefresh:
                                                    // 
                                                    //  this is a test operation. need this so the user can set editor preferences without saving the record
                                                    //    during refresh, the edit page is redrawn just was it was, but no save
                                                    // 
                                                    AdminAction = AdminActionEditRefresh;
                                                    AdminForm = AdminFormEdit;
                                                    break;
                                                case ButtonMarkReviewed:
                                                    AdminAction = AdminActionMarkReviewed;
                                                    AdminForm = GetForm_Close(MenuDepth, adminContent.Name, editRecord.id);
                                                    break;
                                                case ButtonSaveandInvalidateCache:
                                                    AdminAction = AdminActionReloadCDef;
                                                    AdminForm = AdminFormEdit;
                                                    break;
                                                case ButtonDelete:
                                                case ButtonDeletePage:
                                                case ButtonDeletePerson:
                                                case ButtonDeleteRecord:
                                                case ButtonDeleteEmail:
                                                    AdminAction = AdminActionDelete;
                                                    AdminForm = GetForm_Close(MenuDepth, adminContent.Name, editRecord.id);
                                                    //                 Case ButtonSetHTMLEdit
                                                    //                     AdminAction = AdminActionSetHTMLEdit
                                                    //                 Case ButtonSetTextEdit
                                                    //                     AdminAction = AdminActionSetTextEdit
                                                    break;
                                                case ButtonSave:
                                                    AdminAction = AdminActionSave;
                                                    AdminForm = AdminFormEdit;
                                                    break;
                                                case ButtonSaveAddNew:
                                                    AdminAction = AdminActionSaveAddNew;
                                                    AdminForm = AdminFormEdit;
                                                    break;
                                                case ButtonOK:
                                                    AdminAction = AdminActionSave;
                                                    AdminForm = GetForm_Close(MenuDepth, adminContent.Name, editRecord.id);
                                                    break;
                                                case ButtonCancel:
                                                    AdminAction = AdminActionNop;
                                                    AdminForm = GetForm_Close(MenuDepth, adminContent.Name, editRecord.id);
                                                    break;
                                                case ButtonSend:
                                                    // 
                                                    //  Send a Group Email
                                                    // 
                                                    AdminAction = AdminActionSendEmail;
                                                    AdminForm = AdminFormEdit;
                                                    break;
                                                case ButtonActivate:
                                                    // 
                                                    //  Activate (submit) a conditional Email
                                                    // 
                                                    AdminAction = AdminActionActivateEmail;
                                                    AdminForm = AdminFormEdit;
                                                    break;
                                                case ButtonDeactivate:
                                                    // 
                                                    //  Deactivate (clear submit) a conditional Email
                                                    // 
                                                    AdminAction = AdminActionDeactivateEmail;
                                                    AdminForm = AdminFormEdit;
                                                    break;
                                                case ButtonSendTest:
                                                    // 
                                                    //  Test an Email (Group, System, or Conditional)
                                                    // 
                                                    AdminAction = AdminActionSendEmailTest;
                                                    AdminForm = AdminFormEdit;
                                                    //                 Case ButtonSpellCheck
                                                    //                     SpellCheckRequest = True
                                                    //                     AdminAction = AdminActionSave
                                                    //                     AdminForm = AdminFormEdit
                                                    break;
                                                case ButtonCreateDuplicate:
                                                    // 
                                                    //  Create a Duplicate record (for email)
                                                    // 
                                                    AdminAction = AdminActionDuplicate;
                                                    AdminForm = AdminFormEdit;
                                                    break;
                                            }
                                            break;
                                        case AdminFormStyleEditor:
                                            // 
                                            //  Process actions
                                            // 
                                            switch (AdminButton) {
                                                case ButtonSave:
                                                case ButtonOK:
                                                    // 
                                                    cpCore.siteProperties.setProperty("Allow CSS Reset", cpCore.docProperties.getBoolean(RequestNameAllowCSSReset));
                                                    cpCore.cdnFiles.saveFile(DynamicStylesFilename, cpCore.docProperties.getText("StyleEditor"));
                                                    if (cpCore.docProperties.getBoolean(RequestNameInlineStyles)) {
                                                        // 
                                                        //  Inline Styles
                                                        // 
                                                        cpCore.siteProperties.setProperty("StylesheetSerialNumber", "0");
                                                    }
                                                    else {
                                                        //  mark to rebuild next fetch
                                                        cpCore.siteProperties.setProperty("StylesheetSerialNumber", "-1");
                                                        // '
                                                        // ' Linked Styles
                                                        // ' Bump the Style Serial Number so next fetch is not cached
                                                        // '
                                                        // StyleSN = genericController.EncodeInteger(cpCore.main_GetSiteProperty2("StylesheetSerialNumber", "0"))
                                                        // StyleSN = StyleSN + 1
                                                        // Call cpCore.app.setSiteProperty("StylesheetSerialNumber", genericController.encodeText(StyleSN))
                                                        // '
                                                        // ' Save new public stylesheet
                                                        // '
                                                        // ' 11/24/3009 - style sheet processing deprecated
                                                        // Call cpCore.app.virtualFiles.SaveFile("templates\Public" & StyleSN & ".css", cpCore.main_GetStyleSheet)
                                                        // 'Call cpCore.app.virtualFiles.SaveFile("templates\Public" & StyleSN & ".css", cpCore.main_GetStyleSheetProcessed)
                                                        // Call cpCore.app.virtualFiles.SaveFile("templates\Admin" & StyleSN & ".css", cpCore.main_GetStyleSheetDefault)
                                                    }
                                                    
                                                    // 
                                                    //  delete all templateid based editorstylerule files, build on-demand
                                                    // 
                                                    EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", "0", 1, 99, vbTextCompare);
                                                    cpCore.cdnFiles.deleteFile(EditorStyleRulesFilename);
                                                    // 
                                                    CS = cpCore.db.csOpenSql_rev("default", "select id from cctemplates");
                                                    while (cpCore.db.csOk(CS)) {
                                                        EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", cpCore.db.csGet(CS, "ID"), 1, 99, vbTextCompare);
                                                        cpCore.cdnFiles.deleteFile(EditorStyleRulesFilename);
                                                        cpCore.db.csGoNext(CS);
                                                    }
                                                    
                                                    cpCore.db.csClose(CS);
                                                    break;
                                            }
                                            // 
                                            //  Process redirects
                                            // 
                                            switch (AdminButton) {
                                                case ButtonCancel:
                                                case ButtonOK:
                                                    AdminForm = AdminFormRoot;
                                                    break;
                                            }
                                            break;
                                    }
                                    // 
                                    //  ----- Error Trap
                                    // 
                                ErrorTrap:
                                    handleLegacyClassError3("ProcessForms");
                                    // 
                                }
                                
                                // 
                                // ========================================================================
                                // 
                                // ========================================================================
                                // 
                                ((string)(GetForm_EditTitle(((Models.Complex.cdefModel)(adminContent)), ((editRecordClass)(editRecord)))));
                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_EditTitle")
                                // 
                                if ((editRecord.id == 0)) {
                                    GetForm_EditTitle = ("Add an entry to " 
                                                + (editRecord.contentControlId_Name + TitleExtension));
                                }
                                else {
                                    GetForm_EditTitle = ("Editing Record " 
                                                + (editRecord.id + (" in " 
                                                + (editRecord.contentControlId_Name + (" " + TitleExtension)))));
                                }
                                
                                // 
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                                // 
                                //  ----- Error Trap
                                // 
                            ErrorTrap:
                                handleLegacyClassError3("GetForm_EditTitle");
                                // 
                                // ========================================================================
                                // 
                                // ========================================================================
                                // 
                                ((string)(GetForm_EditTitleBar(((Models.Complex.cdefModel)(adminContent)), ((editRecordClass)(editRecord)))));
                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_EditTitleBar")
                                // 
                                adminUIController Adminui = new adminUIController(cpCore);
                                // 
                                GetForm_EditTitleBar = Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), "");
                                // 
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                                // 
                                //  ----- Error Trap
                                // 
                            ErrorTrap:
                                handleLegacyClassError3("GetForm_EditTitleBar");
                                // 
                                // ========================================================================
                                // 
                                // ========================================================================
                                // 
                                ((string)(GetForm_EditFormStart(((int)(AdminFormID)))));
                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_EditFormStart")
                                // 
                                int WhereCount;
                                string s;
                                string saveEmptyFieldListScript;
                                // 
                                //  --- output required hidden fields to preserve values through form submission
                                // 
                                // 
                                // saveEmptyFieldListScript = "" _
                                //     & "function adminEditSaveEmptyFields(){" _
                                //         & "var e=document.getElementById('" &  "FormEmptyFieldList');" _
                                //         & "var c=document.getElementsByTagName('input');" _
                                //         & "for (i=0;i<c.length;i++){" _
                                //             & "if(c[i].type=='checkbox'){" _
                                //                 & "if(c[i].checked==false){e.value+=c[i].name+','}" _
                                //             & "} else if(c[i].type=='radio'){" _
                                //                 & "if(c[i].checked==false){e.value+=c[i].name+','}" _
                                //             & "} else if(c[i].value==''){" _
                                //                 & "e.value+=c[i].name+','" _
                                //             & "}" _
                                //         & "}" _
                                //         & "c=document.getElementsByTagName('select');" _
                                //         & "for (i=0;i<c.length;i++){" _
                                //             & "if(c[i].value==''){e.value+=c[i].name+','}" _
                                //         & "}" _
                                //     & "}" _
                                //     & ""
                                // Call cpCore.htmldoc.main_AddHeadScriptCode(saveEmptyFieldListScript, "Edit Page")
                                // 
                                // saveEmptyFieldListScript = "" _
                                //     & "cj.admin.saveEmptyFieldList('" &  "FormEmptyFieldList');" _
                                //     & ""
                                //     saveEmptyFieldListScript = "" _
                                //         & "if(!docLoaded){" _
                                //             & "alert('This page has not loaded completed. Please wait for the page to load before submitting the form. If the page has loaded, there may have been an error. Please refresh the page.')" _
                                //             & ";return false" _
                                //         & "}else{" _
                                //             & "adminEditSaveEmptyFields();" _
                                //         & "}" _
                                //         & ""
                                // 
                                cpCore.html.addScriptCode_head("var docLoaded=false", "Form loader");
                                cpCore.html.addScriptCode_onLoad("docLoaded=true;", "Form loader");
                                s = cpCore.html.html_GetUploadFormStart();
                                s = genericController.vbReplace(s, ">", (" onSubmit=\"cj.admin.saveEmptyFieldList(\'" + "FormEmptyFieldList\');\">"));
                                s = genericController.vbReplace(s, ">", " autocomplete=\"off\">");
                                s = genericController.vbReplace(s, ">", " id=\"adminEditForm\">");
                                s = (s + ("\r\n" + ("<input TYPE=\"hidden\" NAME=\"" 
                                            + (RequestNameAdminSourceForm + ("\" VALUE=\"" 
                                            + (AdminFormID.ToString + "\">"))))));
                                s = (s + ("\r\n" + ("<input TYPE=\"hidden\" NAME=\"" 
                                            + (RequestNameTitleExtension + ("\" VALUE=\"" 
                                            + (TitleExtension + "\">"))))));
                                s = (s + ("\r\n" + ("<input TYPE=\"hidden\" NAME=\"" 
                                            + (RequestNameAdminDepth + ("\" VALUE=\"" 
                                            + (MenuDepth + "\">"))))));
                                s = (s + ("\r\n" + ("<input TYPE=\"hidden\" NAME=\"" + ("FormEmptyFieldList\" ID=\"" + "FormEmptyFieldList\" VALUE=\",\">"))));
                                if (false) {
                                    // 
                                    //  already added to refresh query string
                                    // 
                                    if ((WherePairCount > 0)) {
                                        for (WhereCount = 0; (WhereCount 
                                                    <= (WherePairCount - 1)); WhereCount++) {
                                            s = (s + ("\r\n" + ("<input TYPE=\"hidden\" NAME=\"wl" 
                                                        + (WhereCount + ("\" VALUE=\"" 
                                                        + (WherePair(0, WhereCount) + ("\"><input TYPE=\"hidden\" NAME=\"wr" 
                                                        + (WhereCount + ("\" VALUE=\"" 
                                                        + (WherePair(1, WhereCount) + "\">"))))))))));
                                        }
                                        
                                    }
                                    
                                }
                                
                                // 
                                GetForm_EditFormStart = s;
                                // 
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                                // 
                                //  ----- Error Trap
                                // 
                            ErrorTrap:
                                handleLegacyClassError3("GetForm_EditFormStart");
                                // 
                                // 
                                //  true if the field is a visible user field (can display on edit form)
                                // 
                                ((bool)(IsVisibleUserField(((bool)(AdminOnly)), ((bool)(DeveloperOnly)), ((bool)(Active)), ((bool)(Authorable)), ((string)(Name)), ((string)(TableName)))));
                                // Private Function IsVisibleUserField( Field as CDefFieldClass, AdminOnly As Boolean, DeveloperOnly As Boolean, Active As Boolean, Authorable As Boolean) As Boolean
                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                // 'Dim th as integer : th = profileLogAdminMethodEnter("IsVisibleUserField")
                                // 
                                bool HasEditRights;
                                // 
                                IsVisibleUserField = false;
                                if (((TableName.ToLower() == "ccpagecontent") 
                                            && (Name.ToLower() == "linkalias"))) {
                                    // 
                                    //  ccpagecontent.linkalias is a control field that is not in control tab
                                    // 
                                }
                                else {
                                    switch (genericController.vbUCase(Name)) {
                                        case "ACTIVE":
                                        case "ID":
                                        case "CONTENTCONTROLID":
                                        case "CREATEDBY":
                                        case "DATEADDED":
                                        case "MODIFIEDBY":
                                        case "MODIFIEDDATE":
                                        case "CREATEKEY":
                                        case "CCGUID":
                                            break;
                                        default:
                                            HasEditRights = true;
                                            if ((AdminOnly || DeveloperOnly)) {
                                                // 
                                                //  field has some kind of restriction
                                                // 
                                                if (!cpCore.doc.authContext.user.Developer) {
                                                    if (!cpCore.doc.authContext.user.Admin) {
                                                        // 
                                                        //  you are not admin
                                                        // 
                                                        HasEditRights = false;
                                                    }
                                                    else if (DeveloperOnly) {
                                                        // 
                                                        //  you are admin, and the record is developer
                                                        // 
                                                        HasEditRights = false;
                                                    }
                                                    
                                                }
                                                
                                            }
                                            
                                            if ((HasEditRights 
                                                        && (Active && Authorable))) {
                                                IsVisibleUserField = true;
                                            }
                                            
                                            break;
                                    }
                                }
                                
                                // 
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                                // 
                                //  ----- Error Trap
                                // 
                            ErrorTrap:
                                handleLegacyClassError3("IsVisibleUserField");
                                // 
                                // 
                                // =============================================================================================
                                //  true if the field is an editable user field (can edit on edit form and save to database)
                                // =============================================================================================
                                // 
                                ((bool)(IsFieldEditable(((Models.Complex.cdefModel)(adminContent)), ((editRecordClass)(editRecord)), ((Models.Complex.CDefFieldModel)(Field)))));
                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                // 'Dim th as integer : th = profileLogAdminMethodEnter("IsFieldEditable")
                                // 
                                // With...
                                IsFieldEditable = (IsVisibleUserField(Field.adminOnly, Field.developerOnly, Field.active, Field.authorable, Field.nameLc, adminContent.ContentTableName) 
                                            & (!Field.ReadOnly 
                                            & !Field.NotEditable));
                                // 
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                                // 
                                //  ----- Error Trap
                                // 
                            ErrorTrap:
                                handleLegacyClassError3("IsFieldEditable");
                                // 
                                // 
                                // =============================================================================================
                                //    Get
                                // =============================================================================================
                                // 
                                ((int)(GetForm_Close(((int)(MenuDepth)), ((string)(ContentName)), ((int)(RecordID)))));
                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Close")
                                // 
                                if ((MenuDepth > 0)) {
                                    GetForm_Close = AdminFormClose;
                                }
                                else {
                                    GetForm_Close = AdminFormIndex;
                                }
                                
                                // Call cpCore.main_ClearAuthoringEditLock(ContentName, RecordID)
                                // 
                                // TODO: Exit Function: Warning!!! Need to return the value
                                return;
                                // 
                                //  ----- Error Trap
                                // 
                            ErrorTrap:
                                handleLegacyClassError3("GetForm_Close");
                                // 
                                // 
                                // =============================================================================================
                                // 
                                // =============================================================================================
                                // 
                                ProcessActionSave(((Models.Complex.cdefModel)(adminContent)), ((editRecordClass)(editRecord)), ((bool)(UseContentWatchLink)));
                                try {
                                    string EditorStyleRulesFilename;
                                    // 
                                    if (true) {
                                        // 
                                        // 
                                        // 
                                        if (!(cpCore.doc.debug_iUserError != "")) {
                                            switch (genericController.vbUCase(adminContent.ContentTableName)) {
                                                case genericController.vbUCase("ccMembers"):
                                                    // 
                                                    // 
                                                    // 
                                                    SaveEditRecord(adminContent, editRecord);
                                                    SaveMemberRules(editRecord.id);
                                                    // Call SaveTopicRules
                                                    break;
                                                case "CCEMAIL":
                                                    SaveEditRecord(adminContent, editRecord);
                                                    //  NO - ignore wwwroot styles, and create it on the fly during send
                                                    // If cpCore.main_GetSiteProperty2("BuildVersion") >= "3.3.291" Then
                                                    //     Call cpCore.app.executeSql( "update ccEmail set InlineStyles=" & encodeSQLText(cpCore.main_GetStyleSheetProcessed) & " where ID=" & EditRecord.ID)
                                                    // End If
                                                    cpCore.html.main_ProcessCheckList("EmailGroups", "Group Email", genericController.encodeText(editRecord.id), "Groups", "Email Groups", "EmailID", "GroupID");
                                                    cpCore.html.main_ProcessCheckList("EmailTopics", "Group Email", genericController.encodeText(editRecord.id), "Topics", "Email Topics", "EmailID", "TopicID");
                                                    break;
                                                case "CCCONTENT":
                                                    SaveEditRecord(adminContent, editRecord);
                                                    LoadAndSaveGroupRules(editRecord);
                                                    break;
                                                case "CCPAGECONTENT":
                                                    SaveEditRecord(adminContent, editRecord);
                                                    LoadContentTrackingDataBase(adminContent, editRecord);
                                                    LoadContentTrackingResponse(adminContent, editRecord);
                                                    // Call LoadAndSaveMetaContent()
                                                    SaveLinkAlias(adminContent, editRecord);
                                                    // Call SaveTopicRules
                                                    SaveContentTracking(adminContent, editRecord);
                                                    break;
                                                case "CCLIBRARYFOLDERS":
                                                    SaveEditRecord(adminContent, editRecord);
                                                    LoadContentTrackingDataBase(adminContent, editRecord);
                                                    LoadContentTrackingResponse(adminContent, editRecord);
                                                    // Call LoadAndSaveCalendarEvents
                                                    // Call LoadAndSaveMetaContent()
                                                    cpCore.html.main_ProcessCheckList("LibraryFolderRules", adminContent.Name, genericController.encodeText(editRecord.id), "Groups", "Library Folder Rules", "FolderID", "GroupID");
                                                    // call SaveTopicRules
                                                    SaveContentTracking(adminContent, editRecord);
                                                    break;
                                                case "CCSETUP":
                                                    SaveEditRecord(adminContent, editRecord);
                                                    if ((editRecord.nameLc.ToLower() == "allowlinkalias")) {
                                                        if (cpCore.siteProperties.getBoolean("AllowLinkAlias")) {
                                                            if (false) {
                                                                // 
                                                                //  Must upgrade
                                                                // 
                                                                cpCore.siteProperties.setProperty("AllowLinkAlias", "0");
                                                                errorController.error_AddUserError(cpCore, "Link Alias entries for your pages can not be created because your site database needs to be upgraded." +
                                                                    "");
                                                            }
                                                            else {
                                                                // 
                                                                //  Verify all page content records have a link alias
                                                                // 
                                                                TurnOnLinkAlias(UseContentWatchLink);
                                                            }
                                                            
                                                        }
                                                        
                                                    }
                                                    
                                                    break;
                                                case genericController.vbUCase("ccGroups"):
                                                    // Case "CCGROUPS"
                                                    // 
                                                    // 
                                                    // 
                                                    SaveEditRecord(adminContent, editRecord);
                                                    LoadContentTrackingDataBase(adminContent, editRecord);
                                                    LoadContentTrackingResponse(adminContent, editRecord);
                                                    LoadAndSaveContentGroupRules(editRecord.id);
                                                    // Call LoadAndSaveCalendarEvents
                                                    // Call LoadAndSaveMetaContent()
                                                    // call SaveTopicRules
                                                    SaveContentTracking(adminContent, editRecord);
                                                    // Dim EditorStyleRulesFilename As String
                                                    break;
                                                case "CCTEMPLATES":
                                                    SaveEditRecord(adminContent, editRecord);
                                                    LoadContentTrackingDataBase(adminContent, editRecord);
                                                    LoadContentTrackingResponse(adminContent, editRecord);
                                                    // Call LoadAndSaveCalendarEvents
                                                    // Call LoadAndSaveMetaContent()
                                                    // call SaveTopicRules
                                                    SaveContentTracking(adminContent, editRecord);
                                                    // 
                                                    EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", editRecord.id.ToString, 1, 99, vbTextCompare);
                                                    cpCore.privateFiles.deleteFile(EditorStyleRulesFilename);
                                                    // Case "CCSHAREDSTYLES"
                                                    //     '
                                                    //     ' save and clear editorstylerules for any template
                                                    //     '
                                                    //     Call SaveEditRecord(adminContent, editRecord)
                                                    //     Call LoadContentTrackingDataBase(adminContent, editRecord)
                                                    //     Call LoadContentTrackingResponse(adminContent, editRecord)
                                                    //     'Call LoadAndSaveCalendarEvents
                                                    //     Call LoadAndSaveMetaContent()
                                                    //     'call SaveTopicRules
                                                    //     Call SaveContentTracking(adminContent, editRecord)
                                                    //     '
                                                    //     EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", "0", 1, 99, vbTextCompare)
                                                    //     Call cpCore.cdnFiles.deleteFile(EditorStyleRulesFilename)
                                                    //     '
                                                    //     CS = cpCore.db.cs_openCsSql_rev("default", "select id from cctemplates")
                                                    //     Do While cpCore.db.cs_ok(CS)
                                                    //         EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", cpCore.db.cs_get(CS, "ID"), 1, 99, vbTextCompare)
                                                    //         Call cpCore.cdnFiles.deleteFile(EditorStyleRulesFilename)
                                                    //         Call cpCore.db.cs_goNext(CS)
                                                    //     Loop
                                                    //     Call cpCore.db.cs_Close(CS)
                                                    break;
                                                default:
                                                    SaveEditRecord(adminContent, editRecord);
                                                    LoadContentTrackingDataBase(adminContent, editRecord);
                                                    LoadContentTrackingResponse(adminContent, editRecord);
                                                    // Call LoadAndSaveCalendarEvents
                                                    // Call LoadAndSaveMetaContent()
                                                    // call SaveTopicRules
                                                    SaveContentTracking(adminContent, editRecord);
                                                    break;
                                            }
                                        }
                                        
                                    }
                                    
                                    // 
                                    //  If the content supports datereviewed, mark it
                                    // 
                                    if ((cpCore.doc.debug_iUserError != "")) {
                                        AdminForm = AdminSourceForm;
                                    }
                                    
                                    AdminAction = AdminActionNop;
                                    //  convert so action can be used in as a refresh
                                }
                                catch (Exception ex) {
                                    cpCore.handleException(ex);
                                }
                                
                                // 
                            }
                            
                        }
                        
                    }
                    
                }
                
            }
            
        }
    }
}