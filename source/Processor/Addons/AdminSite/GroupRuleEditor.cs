
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.constants;
using Contensive.Processor.Models.Domain;
using Contensive.Addons.Tools;
using static Contensive.Processor.AdminUIController;
//
namespace Contensive.Addons.AdminSite {
    public class GroupRuleEditor {
        //
        //========================================================================
        //
        public static string GetForm_Edit_MemberGroups(CoreController core, AdminDataModel adminData) {
            string result = null;
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
                //
                //
                StringBuilderLegacyController body = new StringBuilderLegacyController();
                //
                // ----- Gather all the SecondaryContent that associates to the PrimaryContent
                int PeopleContentID = CdefController.getContentId(core, "People");
                int GroupContentID = CdefController.getContentId(core, "Groups");
                //
                int MembershipCount = 0;
                int MembershipSize = 0;
                int GroupCount = 0;
                {
                    //
                    // ----- read in the groups that this member has subscribed (exclude new member records)
                    int[] Membership = { };
                    DateTime[] DateExpires = { };
                    bool[] Active = { };
                    if (editRecord.id != 0) {
                        string SQL = "SELECT Active,GroupID,DateExpires"
                            + " FROM ccMemberRules"
                            + " WHERE MemberID=" + editRecord.id;
                        int CS = core.db.csOpenSql(SQL, "Default");
                        while (core.db.csOk(CS)) {
                            if (MembershipCount >= MembershipSize) {
                                MembershipSize = MembershipSize + 10;
                                Array.Resize(ref Membership, MembershipSize + 1);
                                Array.Resize(ref Active, MembershipSize + 1);
                                Array.Resize(ref DateExpires, MembershipSize + 1);
                            }
                            Membership[MembershipCount] = core.db.csGetInteger(CS, "GroupID");
                            DateExpires[MembershipCount] = core.db.csGetDate(CS, "DateExpires");
                            Active[MembershipCount] = core.db.csGetBoolean(CS, "Active");
                            MembershipCount = MembershipCount + 1;
                            core.db.csGoNext(CS);
                        }
                        core.db.csClose(ref CS);
                    }
                    //
                    // ----- read in all the groups, sorted by ContentName
                    string SQL2 = "SELECT ccGroups.ID AS ID, ccContent.Name AS SectionName, ccGroups.Caption AS GroupCaption, ccGroups.name AS GroupName, ccGroups.SortOrder"
                        + " FROM ccGroups LEFT JOIN ccContent ON ccGroups.ContentControlID = ccContent.ID"
                        + " Where (((ccGroups.Active) <> " + SQLFalse + ") And ((ccContent.Active) <> " + SQLFalse + "))";
                    SQL2 += ""
                        + " GROUP BY ccGroups.ID, ccContent.Name, ccGroups.Caption, ccGroups.name, ccGroups.SortOrder"
                        + " ORDER BY ccGroups.Caption";
                    int CS2 = core.db.csOpenSql(SQL2, "Default");
                    //
                    // Output all the groups, with the active and dateexpires from those joined
                    //body.Add(adminUIController.EditTableOpen);
                    bool CanSeeHiddenGroups = core.session.isAuthenticatedDeveloper(core);
                    while (core.db.csOk(CS2)) {
                        string GroupName = core.db.csGet(CS2, "GroupName");
                        if ((GroupName.Left(1) != "_") || CanSeeHiddenGroups) {
                            string GroupCaption = core.db.csGet(CS2, "GroupCaption");
                            int GroupID = core.db.csGetInteger(CS2, "ID");
                            if (string.IsNullOrEmpty(GroupCaption)) {
                                GroupCaption = GroupName;
                                if (string.IsNullOrEmpty(GroupCaption)) {
                                    GroupCaption = "Group&nbsp;" + GroupID;
                                }
                            }
                            bool GroupActive = false;
                            string DateExpireValue = "";
                            if (MembershipCount != 0) {
                                for (int MembershipPointer = 0; MembershipPointer < MembershipCount; MembershipPointer++) {
                                    if (Membership[MembershipPointer] == GroupID) {
                                        GroupActive = Active[MembershipPointer];
                                        if (DateExpires[MembershipPointer] > DateTime.MinValue) {
                                            DateExpireValue = GenericController.encodeText(DateExpires[MembershipPointer]);
                                        }
                                        break;
                                    }
                                }
                            }
                            string ReportLink = "";
                            ReportLink = ReportLink + "[<a href=\"?af=4&cid=" + GroupContentID + "&id=" + GroupID + "\">Edit&nbsp;Group</a>]";
                            if (GroupID > 0) {
                                ReportLink = ReportLink + "&nbsp;[<a href=\"?" + rnAdminForm + "=12&rid=35&recordid=" + GroupID + "\">Group&nbsp;Report</a>]";
                            }
                            //
                            string Caption = "";
                            if (GroupCount == 0) {
                                Caption = SpanClassAdminSmall + "Groups</span>";
                            } else {
                                Caption = "&nbsp;";
                            }
                            body.Add("<tr><td class=\"ccAdminEditCaption\">" + Caption + "</td>");
                            body.Add("<td class=\"ccAdminEditField\">");
                            body.Add("<table border=0 cellpadding=0 cellspacing=0 width=\"100%\" ><tr>");
                            body.Add("<td width=\"40%\">" + HtmlController.inputHidden("Memberrules." + GroupCount + ".ID", GroupID) + HtmlController.checkbox("MemberRules." + GroupCount, GroupActive) + GroupCaption + "</td>");
                            body.Add("<td width=\"30%\"> Expires " + HtmlController.inputText(core, "MemberRules." + GroupCount + ".DateExpires", DateExpireValue, 1, 20) + "</td>");
                            body.Add("<td width=\"30%\">" + ReportLink + "</td>");
                            body.Add("</tr></table>");
                            body.Add("</td></tr>");
                            GroupCount = GroupCount + 1;
                        }
                        core.db.csGoNext(CS2);
                    }
                    core.db.csClose(ref CS2);
                }
                if (GroupCount == 0) {
                    body.Add("<tr><td valign=middle align=right>" + SpanClassAdminSmall + "Groups</span></td><td>" + SpanClassAdminNormal + "There are currently no groups defined</span></td></tr>");
                } else {
                    body.Add("<input type=\"hidden\" name=\"MemberRules.RowCount\" value=\"" + GroupCount + "\">");
                }
                body.Add("<tr>");
                body.Add("<td class=\"ccAdminEditCaption\">&nbsp;</td>");
                body.Add("<td class=\"ccAdminEditField\">" + SpanClassAdminNormal + "[<a href=?cid=" + CdefController.getContentId(core, "Groups") + " target=_blank>Manage Groups</a>]</span></td>");
                body.Add("</tr>");

                result = AdminUIController.getEditPanel(core, (!adminData.allowAdminTabs), "Group Membership", "This person is a member of these groups", AdminUIController.editTableOpen + body.Text + AdminUIController.editTableClose);
                adminData.EditSectionPanelCount = adminData.EditSectionPanelCount + 1;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
    }
}
