
using System;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;
using static Contensive.Processor.Constants;

namespace Contensive.Processor.Addons.AdminSite {
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
                Contensive.Processor.Addons.AdminSite.Models.EditRecordModel editRecord = adminData.editRecord;
                //
                StringBuilderLegacyController f = new StringBuilderLegacyController();
                //
                s = core.html.getCheckList("EmailTopics", "Group Email", editRecord.id, "Topics", "Email Topics", "EmailID", "TopicID", "", "Name");
                s = "<tr>"
                    + "<td class=\"ccAdminEditCaption\">Topics</td>"
                    + "<td class=\"ccAdminEditField\" colspan=2>" + SpanClassAdminNormal + s + "</span></td>"
                    + "</tr><tr>"
                    + "<td class=\"ccAdminEditCaption\">&nbsp;</td>"
                    + "<td class=\"ccAdminEditField\" colspan=2>" + SpanClassAdminNormal + "[<a href=?cid=" + ContentMetadataModel.getContentId(core, "Topics") + " target=_blank>Manage Topics</a>]</span></td>"
                    + "</tr>";
                s = AdminUIController.editTable( s );
                s = AdminUIController.getEditPanel(core, true, "Email Rules", "Send email to people in these groups", s);
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return s;
        }

    }
}
