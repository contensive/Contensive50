
using System;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Constants;
using Nustache.Core;
using Contensive.Processor.Properties;
using Contensive.Addons.AdminSite.Controllers;

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
                    listCaption = "Groups",
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

                    using (var csData = new CsModel(core)) {
                        //
                        // Output all the groups, with the active and dateexpires from those joined
                        //body.Add(adminUIController.EditTableOpen);
                        bool CanSeeHiddenGroups = core.session.isAuthenticatedDeveloper(core);
                        csData.openSql(SQL2, "Default");
                        while (csData.ok()) {
                            string GroupName = csData.getText("GroupName");
                            if ((GroupName.Left(1) != "_") || CanSeeHiddenGroups) {
                                string GroupCaption = csData.getText("GroupCaption");
                                int GroupID = csData.getInteger("ID");
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
                                string relatedButtonList = "";
                                relatedButtonList += AdminUIController.getButtonPrimaryAnchor("Edit", "?af=4&cid=" + Processor.Models.Domain.MetaModel.getContentId(core, "Groups") + "&id=" + GroupID);
                                relatedButtonList += AdminUIController.getButtonPrimaryAnchor("Members", "?af=1&cid=" + Processor.Models.Domain.MetaModel.getContentId(core, "people") + "&IndexFilterAddGroup=" + GenericController.encodeURL(GroupName));
                                //
                                var row = new GroupRuleEditorRowModel() {
                                    idHidden = HtmlController.inputHidden("Memberrules." + GroupCount + ".ID", GroupID),
                                    checkboxInput = HtmlController.checkbox("MemberRules." + GroupCount, GroupActive),
                                    groupCaption = GroupCaption,
                                    expiresInput = HtmlController.inputText(core, "MemberRules." + GroupCount + ".DateExpires", DateExpireValue, 1, 20, "", false, false, "text form-control", -1, false, "expires"),
                                    relatedButtonList = relatedButtonList,
                                };
                                groupRuleEditor.rowList.Add(row);
                                GroupCount += 1;
                            }
                            csData.goNext();
                        }
                    }
                }
                //
                // -- add a row for group count and Add Group button
                groupRuleEditor.rowList.Add(new GroupRuleEditorRowModel() {
                    idHidden = HtmlController.inputHidden("MemberRules.RowCount", GroupCount),
                    checkboxInput = AdminUIController.getButtonPrimaryAnchor("Add Group", "?af=4&cid=" + MetaModel.getContentId(core, "Groups")),
                    groupCaption = "",
                    expiresInput = "",
                    relatedButtonList = "",
                });
                body.Add(Render.StringToString(Resources.GroupRuleEditorRow2, groupRuleEditor));
                //if (GroupCount == 0) {
                //    body.Add("<tr><td valign=middle align=right>" + SpanClassAdminSmall + "Groups</span></td><td>" + SpanClassAdminNormal + "There are currently no groups defined</span></td></tr>");
                //} else {
                //    body.Add("<input type=\"hidden\" name=\"MemberRules.RowCount\" value=\"" + GroupCount + "\">");
                //}
                //body.Add("<tr>");
                //body.Add("<td class=\"ccAdminEditCaption\">&nbsp;</td>");
                //body.Add("<td class=\"ccAdminEditField\">" + SpanClassAdminNormal + "[<a href=?cid=" + Models.Domain.MetaModel.getContentId(core, "Groups") + " target=_blank>Manage Groups</a>]</span></td>");
                //body.Add("</tr>");

                result = AdminUIController.getEditPanel(core, (!adminData.allowAdminTabs), "Group Membership", "", body.Text );
                adminData.EditSectionPanelCount = adminData.EditSectionPanelCount + 1;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        public class GroupRuleEditorRowModel {
            public string idHidden;
            public string checkboxInput;
            public string groupCaption;
            public string expiresInput;
            public string relatedButtonList;
        }
        public class GroupRuleEditorModel {
            public string listCaption;
            public string helpText;
            public List<GroupRuleEditorRowModel> rowList;
        }
    }
}
