
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
using Nustache.Core;
using Contensive.Processor.Properties;
//
namespace Contensive.Addons.AdminSite {
    public class GroupRuleEditor {
        //
        //========================================================================
        //
        public static string get(CoreController core, AdminDataModel adminData) {
            string result = null;
            try {
                //
                var groupRuleEditor = new GroupRuleEditorModel {
                    rowList = new List<GroupRuleEditorRowModel>()
                };
                //
                StringBuilderLegacyController body = new StringBuilderLegacyController();
                int GroupCount = 0;
                {
                    //
                    // ----- read in the groups that this member has subscribed (exclude new member records)
                    int[] membershipListGroupId = { };
                    DateTime[] membershipListDateExpires = { };
                    bool[] membershipListActive = { };
                    //
                    int membershipCount = 0;
                    if (adminData.editRecord.id != 0) {
                        var memberRuleList = MemberRuleModel.createList(core, "memberid=" + adminData.editRecord.id);
                        int membershipSize = 0;
                        foreach ( var memberRule in memberRuleList) {
                            if (membershipCount >= membershipSize) {
                                membershipSize = membershipSize + 100;
                                Array.Resize(ref membershipListGroupId, membershipSize + 1);
                                Array.Resize(ref membershipListActive, membershipSize + 1);
                                Array.Resize(ref membershipListDateExpires, membershipSize + 1);
                            }
                            membershipListGroupId[membershipCount] = memberRule.groupId;
                            membershipListDateExpires[membershipCount] = memberRule.dateExpires;
                            membershipListActive[membershipCount] = memberRule.active;
                            membershipCount = membershipCount + 1;
                        }
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
                            if (membershipCount != 0) {
                                for (int MembershipPointer = 0; MembershipPointer < membershipCount; MembershipPointer++) {
                                    if (membershipListGroupId[MembershipPointer] == GroupID) {
                                        GroupActive = membershipListActive[MembershipPointer];
                                        if (membershipListDateExpires[MembershipPointer] > DateTime.MinValue) {
                                            DateExpireValue = GenericController.encodeText(membershipListDateExpires[MembershipPointer]);
                                        }
                                        break;
                                    }
                                }
                            }
                            string ReportLink = "";
                            int GroupContentID = CdefController.getContentId(core, "Groups");
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
                            groupRuleEditor.rowList.Add(new GroupRuleEditorRowModel() {
                                rowCaption = Caption,
                                hiddenId = "Memberrules." + GroupCount + ".ID",
                                hiddenValue = GroupID.ToString(),
                                checkboxName = "MemberRules." + GroupCount.ToString(),
                                checkboxChecked = (GroupActive) ? " checked" : "",
                                expiresName = "MemberRules." + GroupCount + ".DateExpires",
                                expiresValue = DateExpireValue,
                                groupCaption = GroupCaption
                            });
                            body.Add( Render.StringToString(Resources.GroupRuleEditorRow, groupRuleEditor));
                            //body.Add("<tr><td class=\"ccAdminEditCaption\">" + Caption + "</td>");
                            //body.Add("<td class=\"ccAdminEditField\">");
                            //body.Add("<table border=0 cellpadding=0 cellspacing=0 width=\"100%\" ><tr>");
                            //body.Add("<td width=\"40%\">" + HtmlController.inputHidden("Memberrules." + GroupCount + ".ID", GroupID) + HtmlController.checkbox("MemberRules." + GroupCount, GroupActive) + GroupCaption + "</td>");
                            //body.Add("<td width=\"30%\"> Expires " + HtmlController.inputText(core, "MemberRules." + GroupCount + ".DateExpires", DateExpireValue, 1, 20) + "</td>");
                            //body.Add("<td width=\"30%\">" + ReportLink + "</td>");
                            //body.Add("</tr></table>");
                            //body.Add("</td></tr>");
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
        //
        public class GroupRuleEditorRowModel {
            public string rowCaption;
            public string hiddenId;
            public string hiddenValue;
            public string checkboxName;
            public string checkboxChecked;
            public string groupCaption;
            public string expiresName;
            public string expiresValue;
        }
        public class GroupRuleEditorModel {
            public List<GroupRuleEditorRowModel> rowList;
        }
    }
}
