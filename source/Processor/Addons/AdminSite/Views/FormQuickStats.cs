
using System;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Constants;

namespace Contensive.Addons.AdminSite {
    public class FormQuickStats {
        //
        //========================================================================
        //
        //========================================================================
        //   Print the root form
        //========================================================================
        //
        public static string GetForm_QuickStats( CoreController core )  {
            string tempGetForm_QuickStats = null;
            try {
                //
                string SQL = null;
                int CS = 0;
                string RowColor = null;
                string Panel = null;
                int VisitID = 0;
                int VisitCount = 0;
                double PageCount = 0;
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                //
                // --- Start a form to make a refresh button
                //
                //Stream.Add(htmlController.form_start(core));
                Stream.Add(core.html.getPanelButtons(ButtonCancel + "," + ButtonRefresh, "" + RequestNameButton + ""));
                Stream.Add("<input TYPE=\"hidden\" NAME=\"asf\" VALUE=\"" + AdminFormQuickStats + "\">");
                Stream.Add(core.html.getPanel(" "));
                //

                // --- Indented part (Title Area plus page)
                //
                Stream.Add("<table border=\"0\" cellpadding=\"20\" cellspacing=\"0\" width=\"100%\"><tr><td>" + SpanClassAdminNormal);
                Stream.Add("<h1>Real-Time Activity Report</h1>");
                //
                // --- set column width
                //
                Stream.Add("<h2>Visits Today</h2>");
                Stream.Add("<table border=\"0\" cellpadding=\"3\" cellspacing=\"0\" width=\"100%\" style=\"background-color:white;border-top:1px solid #888;\">");
                //Stream.Add( "<tr"">")
                //Stream.Add( "<td width=""150""><img alt=""space"" src=""/ContensiveBase/images/spacer.gif"" width=""140"" height=""1"" ></td>")
                //Stream.Add( "<td width=""150""><img alt=""space"" src=""/ContensiveBase/images/spacer.gif"" width=""140"" height=""1"" ></td>")
                //Stream.Add( "<td width=""100%""><img alt=""space"" src=""/ContensiveBase/images/spacer.gif"" width=""100%"" height=""1"" ></td>")
                //Stream.Add( "</tr>")
                //
                // ----- All Visits Today
                //
                SQL = "SELECT Count(ccVisits.ID) AS VisitCount, Avg(ccVisits.PageVisits) AS PageCount FROM ccVisits WHERE ((ccVisits.StartTime)>" + DbController.encodeSQLDate(core.doc.profileStartTime.Date) + ");";
                csXfer.csOpenSql(SQL);
                if (csXfer.csOk()) {
                    VisitCount = csXfer.csGetInteger(CS, "VisitCount");
                    PageCount = csXfer.csGetNumber(CS, "pageCount");
                    Stream.Add("<tr>");
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "All Visits</span></td>");
                    Stream.Add("<td style=\"width:150px;border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + HtmlController.encodeHtml(core.appConfig.adminRoute + "?" + rnAdminForm + "=" + AdminFormReports + "&rid=3&DateFrom=" + core.doc.profileStartTime + "&DateTo=" + core.doc.profileStartTime.ToShortDateString()) + "\">" + VisitCount + "</A>, " + string.Format("{0:N2}", PageCount) + " pages/visit.</span></td>");
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "This includes all visitors to the website, including guests, bots and administrators. Pages/visit includes page hits and not ajax or remote method hits.</span></td>");
                    Stream.Add("</tr>");
                }
                csXfer.csClose();
                //
                // ----- Non-Bot Visits Today
                //
                SQL = "SELECT Count(ccVisits.ID) AS VisitCount, Avg(ccVisits.PageVisits) AS PageCount FROM ccVisits WHERE (ccVisits.CookieSupport=1)and((ccVisits.StartTime)>" + DbController.encodeSQLDate(core.doc.profileStartTime.Date) + ");";
                csXfer.csOpenSql(SQL);
                if (csXfer.csOk()) {
                    VisitCount = csXfer.csGetInteger(CS, "VisitCount");
                    PageCount = csXfer.csGetNumber(CS, "pageCount");
                    Stream.Add("<tr>");
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "Non-bot Visits</span></td>");
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + HtmlController.encodeHtml(core.appConfig.adminRoute + "?" + rnAdminForm + "=" + AdminFormReports + "&rid=3&DateFrom=" + core.doc.profileStartTime.ToShortDateString() + "&DateTo=" + core.doc.profileStartTime.ToShortDateString()) + "\">" + VisitCount + "</A>, " + string.Format("{0:N2}", PageCount) + " pages/visit.</span></td>");
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "This excludes hits from visitors identified as bots. Pages/visit includes page hits and not ajax or remote method hits.</span></td>");
                    Stream.Add("</tr>");
                }
                csXfer.csClose();
                //
                // ----- Visits Today by new visitors
                //
                SQL = "SELECT Count(ccVisits.ID) AS VisitCount, Avg(ccVisits.PageVisits) AS PageCount FROM ccVisits WHERE (ccVisits.CookieSupport=1)and(ccVisits.StartTime>" + DbController.encodeSQLDate(core.doc.profileStartTime.Date) + ")AND(ccVisits.VisitorNew<>0);";
                csXfer.csOpenSql(SQL);
                if (csXfer.csOk()) {
                    VisitCount = csXfer.csGetInteger(CS, "VisitCount");
                    PageCount = csXfer.csGetNumber(CS, "pageCount");
                    Stream.Add("<tr>");
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "Visits by New Visitors</span></td>");
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + HtmlController.encodeHtml(core.appConfig.adminRoute + "?" + rnAdminForm + "=" + AdminFormReports + "&rid=3&ExcludeOldVisitors=1&DateFrom=" + core.doc.profileStartTime.ToShortDateString() + "&DateTo=" + core.doc.profileStartTime.ToShortDateString()) + "\">" + VisitCount + "</A>, " + string.Format("{0:N2}", PageCount) + " pages/visit.</span></td>");
                    Stream.Add("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "This includes only new visitors not identified as bots. Pages/visit includes page hits and not ajax or remote method hits.</span></td>");
                    Stream.Add("</tr>");
                }
                csXfer.csClose();
                //
                Stream.Add("</table>");
                //
                // ----- Visits currently online
                //
                if (true) {
                    Panel = "";
                    Stream.Add("<h2>Current Visits</h2>");
                    SQL = "SELECT ccVisits.HTTP_REFERER as referer,ccVisits.remote_addr as Remote_Addr, ccVisits.LastVisitTime as LastVisitTime, ccVisits.PageVisits as PageVisits, ccMembers.Name as MemberName, ccVisits.ID as VisitID, ccMembers.ID as MemberID"
                        + " FROM ccVisits LEFT JOIN ccMembers ON ccVisits.MemberID = ccMembers.ID"
                        + " WHERE (((ccVisits.LastVisitTime)>" + DbController.encodeSQLDate(core.doc.profileStartTime.AddHours(-1)) + "))"
                        + " ORDER BY ccVisits.LastVisitTime DESC;";
                    csXfer.csOpenSql(SQL);
                    if (csXfer.csOk()) {
                        Panel = Panel + "<table width=\"100%\" border=\"0\" cellspacing=\"1\" cellpadding=\"2\">";
                        Panel = Panel + "<tr bgcolor=\"#B0B0B0\">";
                        Panel = Panel + "<td width=\"20%\" align=\"left\">" + SpanClassAdminNormal + "User</td>";
                        Panel = Panel + "<td width=\"20%\" align=\"left\">" + SpanClassAdminNormal + "IP&nbsp;Address</td>";
                        Panel = Panel + "<td width=\"20%\" align=\"left\">" + SpanClassAdminNormal + "Last&nbsp;Page&nbsp;Hit</td>";
                        Panel = Panel + "<td width=\"10%\" align=\"right\">" + SpanClassAdminNormal + "Page&nbsp;Hits</td>";
                        Panel = Panel + "<td width=\"10%\" align=\"right\">" + SpanClassAdminNormal + "Visit</td>";
                        Panel = Panel + "<td width=\"30%\" align=\"left\">" + SpanClassAdminNormal + "Referer</td>";
                        Panel = Panel + "</tr>";
                        RowColor = "ccPanelRowEven";
                        while (csXfer.csOk()) {
                            VisitID = csXfer.csGetInteger(CS, "VisitID");
                            Panel = Panel + "<tr class=\"" + RowColor + "\">";
                            Panel = Panel + "<td align=\"left\">" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + HtmlController.encodeHtml(core.appConfig.adminRoute + "?" + rnAdminForm + "=" + AdminFormReports + "&rid=16&MemberID=" + csXfer.csGetInteger(CS, "MemberID")) + "\">" + csXfer.csGet(CS, "MemberName") + "</A></span></td>";
                            Panel = Panel + "<td align=\"left\">" + SpanClassAdminNormal + csXfer.csGet(CS, "Remote_Addr") + "</span></td>";
                            Panel = Panel + "<td align=\"left\">" + SpanClassAdminNormal + csXfer.csGetDate(CS, "LastVisitTime").ToString("") + "</span></td>";
                            Panel = Panel + "<td align=\"right\">" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + core.appConfig.adminRoute + "?" + rnAdminForm + "=" + AdminFormReports + "&rid=10&VisitID=" + VisitID + "\">" + csXfer.csGet(CS, "PageVisits") + "</A></span></td>";
                            Panel = Panel + "<td align=\"right\">" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + core.appConfig.adminRoute + "?" + rnAdminForm + "=" + AdminFormReports + "&rid=17&VisitID=" + VisitID + "\">" + VisitID + "</A></span></td>";
                            Panel = Panel + "<td align=\"left\">" + SpanClassAdminNormal + "&nbsp;" + csXfer.csGetText(CS, "referer") + "</span></td>";
                            Panel = Panel + "</tr>";
                            if (RowColor == "ccPanelRowEven") {
                                RowColor = "ccPanelRowOdd";
                            } else {
                                RowColor = "ccPanelRowEven";
                            }
                            csXfer.csGoNext(CS);
                        }
                        Panel = Panel + "</table>";
                    }
                    csXfer.csClose();
                    Stream.Add(core.html.getPanel(Panel, "ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 0));
                }
                Stream.Add("</td></tr></table>");
                //Stream.Add(htmlController.form_end());
                //
                tempGetForm_QuickStats = HtmlController.form(core, Stream.Text);
                core.html.addTitle("Quick Stats");
                return tempGetForm_QuickStats;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetForm_QuickStats;
        }
        //

    }
}
