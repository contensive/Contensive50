
using System;
using System.Collections.Generic;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Controllers;
using Nustache.Core;
using Contensive.Processor.Properties;
using Contensive.Processor.Addons.AdminSite.Controllers;
using Contensive.Models.Db;

namespace Contensive.Processor.Addons.AdminSite {
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
                        var memberRuleList = DbBaseModel.createList<MemberRuleModel>(core.cpParent, "memberid=" + adminData.editRecord.id);
                        int membershipSize = 0;
                        foreach ( var memberRule in memberRuleList) {
                            if (membershipCount >= membershipSize) {
                                membershipSize = membershipSize + 100;
                                Array.Resize(ref membershipListGroupId, membershipSize + 1);
                                Array.Resize(ref membershipListActive, membershipSize + 1);
                                Array.Resize(ref membershipListDateExpires, membershipSize + 1);
                            }
                            membershipListGroupId[membershipCount] = memberRule.groupId;
                            membershipListDateExpires[membershipCount] = GenericController.encodeDate(memberRule.dateExpires);
                            membershipListActive[membershipCount] = memberRule.active;
                            membershipCount = membershipCount + 1;
                        }
                    }
                    //
                    // ----- read in all the groups, sorted by ContentName
                    using (var csGroups = new CsModel(core)) {
                        bool canSeeHiddenGroups = core.session.isAuthenticatedDeveloper();
                        csGroups.openSql("select id,name as groupName,caption as groupCaption from ccgroups where (active>0) order by caption,name,id");
                        while (csGroups.ok()) {
                            string GroupName = csGroups.getText("GroupName");
                            if ((GroupName.left(1) != "_") || canSeeHiddenGroups) {
                                string GroupCaption = csGroups.getText("GroupCaption");
                                int GroupID = csGroups.getInteger("ID");
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
                                relatedButtonList += AdminUIController.getButtonPrimaryAnchor("Edit", "?af=4&cid=" + Processor.Models.Domain.ContentMetadataModel.getContentId(core, "Groups") + "&id=" + GroupID);
                                relatedButtonList += AdminUIController.getButtonPrimaryAnchor("Members", "?af=1&cid=" + Processor.Models.Domain.ContentMetadataModel.getContentId(core, "people") + "&IndexFilterAddGroup=" + GenericController.encodeURL(GroupName));
                                //
                                var row = new GroupRuleEditorRowModel() {
                                    idHidden = HtmlController.inputHidden("Memberrules." + GroupCount + ".ID", GroupID),
                                    checkboxInput = HtmlController.checkbox("MemberRules." + GroupCount, GroupActive),
                                    groupCaption = GroupCaption,
                                    expiresInput = HtmlController.inputText_Legacy(core, "MemberRules." + GroupCount + ".DateExpires", DateExpireValue, 1, 20, "", false, false, "text form-control", -1, false, "expires"),
                                    relatedButtonList = relatedButtonList,
                                };
                                groupRuleEditor.rowList.Add(row);
                                GroupCount += 1;
                            }
                            csGroups.goNext();
                        }
                    }
                }
                //
                // -- add a row for group count and Add Group button
                groupRuleEditor.rowList.Add(new GroupRuleEditorRowModel() {
                    idHidden = HtmlController.inputHidden("MemberRules.RowCount", GroupCount),
                    checkboxInput = AdminUIController.getButtonPrimaryAnchor("Add Group", "?af=4&cid=" + ContentMetadataModel.getContentId(core, "Groups")),
                    groupCaption = "",
                    expiresInput = "",
                    relatedButtonList = "",
                });
                body.Add(Render.StringToString(Resources.GroupRuleEditorRow2, groupRuleEditor));
                result = AdminUIController.getEditPanel(core, (!adminData.allowAdminTabs), "Group Membership", "", body.Text );
                adminData.editSectionPanelCount = adminData.editSectionPanelCount + 1;
            } catch (Exception ex) {
                LogController.logError(core, ex);
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
