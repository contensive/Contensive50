﻿
using System;
using Contensive.Addons.AdminSite.Controllers;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Constants;

namespace Contensive.Addons.AdminSite {
    public class EmailTopicEditor {
        //
        //========================================================================
        /// <summary>
        /// Topics tab
        /// </summary>
        /// <param name="core"></param>
        /// <param name="adminData"></param>
        /// <param name="readOnlyField"></param>
        /// <returns></returns>
        public static string get( CoreController core, AdminDataModel adminData, bool readOnlyField) {
            string s = "";
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
                //
                //
                StringBuilderLegacyController f = new StringBuilderLegacyController();
                //adminUIController Adminui = new adminUIController(core);
                //
                s = core.html.getCheckList("EmailTopics", "Group Email", editRecord.id, "Topics", "Email Topics", "EmailID", "TopicID", "", "Name");
                s = "<tr>"
                    + "<td class=\"ccAdminEditCaption\">Topics</td>"
                    + "<td class=\"ccAdminEditField\" colspan=2>" + SpanClassAdminNormal + s + "</span></td>"
                    + "</tr><tr>"
                    + "<td class=\"ccAdminEditCaption\">&nbsp;</td>"
                    + "<td class=\"ccAdminEditField\" colspan=2>" + SpanClassAdminNormal + "[<a href=?cid=" + CdefController.getContentId(core, "Topics") + " target=_blank>Manage Topics</a>]</span></td>"
                    + "</tr>";
                s = AdminUIController.editTable( s );
                s = AdminUIController.getEditPanel(core, (!adminData.allowAdminTabs), "Email Rules", "Send email to people in these groups", s);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return s;
        }

    }
}
