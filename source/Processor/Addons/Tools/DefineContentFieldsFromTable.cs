
using System;
using Contensive.Processor;

using Contensive.Processor.Models.Domain;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Addons.AdminSite.Controllers;
//
namespace Contensive.Processor.Addons.Tools {
    //
    public class DefineContentFieldsFromTableClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// addon method, deliver complete Html admin site
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cpBase) {
            return get(((CPClass)cpBase).core);
        }
        //
        //=============================================================================
        /// <summary>
        /// Remove all Content Fields and rebuild them from the fields found in a table
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        //
        public static string get(CoreController core) {
            string result = "";
            try {
                string Button = core.docProperties.getText("Button");
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                //
                //   print out the submit form
                //
                Stream.Add("<table border=\"0\" cellpadding=\"11\" cellspacing=\"0\" width=\"100%\">");
                //
                Stream.Add("<tr><td colspan=\"2\">" + SpanClassAdminNormal);
                Stream.Add("Delete the current content field definitions for this Content Definition, and recreate them from the table referenced by this content.");
                Stream.Add("</SPAN></td></tr>");
                //
                Stream.Add("<tr>");
                Stream.Add("<TD>" + SpanClassAdminNormal + "Content Name</SPAN></td>");
                Stream.Add("<TD><Select name=\"ContentName\">");
                int ItemCount = 0;
                using (var csData = new CsModel(core)) {
                    csData.open("Content", "", "name");
                    while (csData.ok()) {
                        Stream.Add("<option value=\"" + csData.getText("name") + "\">" + csData.getText("name") + "</option>");
                        ItemCount = ItemCount + 1;
                        csData.goNext();
                    }

                }
                if (ItemCount == 0) {
                    Stream.Add("<option value=\"-1\">System</option>");
                }
                Stream.Add("</select></td>");
                Stream.Add("</tr>");
                //
                Stream.Add("<tr>");
                Stream.Add("<TD>&nbsp;</td>");
                Stream.Add("<TD>" + HtmlController.inputSubmit(ButtonCreateFields) + "</td>");
                Stream.Add("</tr>");
                //
                Stream.Add("<tr>");
                Stream.Add("<td width=\"150\"><IMG alt=\"\" src=\"" + cdnPrefix + "Images/spacer.gif\" width=\"150\" height=\"1\"></td>");
                Stream.Add("<td width=\"99%\"><IMG alt=\"\" src=\"" + cdnPrefix + "Images/spacer.gif\" width=\"100%\" height=\"1\"></td>");
                Stream.Add("</tr>");
                Stream.Add("</TABLE>");
                Stream.Add("</form>");
                //
                //   process the button if present
                //
                if (Button == ButtonCreateFields) {
                    string ContentName = core.docProperties.getText("ContentName");
                    if (string.IsNullOrEmpty(ContentName)) {
                        Stream.Add("Select a content before submitting. Fields were not changed.");
                    } else {
                        int ContentId = ContentMetadataModel.getContentId(core, ContentName);
                        if (ContentId == 0) {
                            Stream.Add("GetContentID failed. Fields were not changed.");
                        } else {
                            MetadataController.deleteContentRecords(core, "Content Fields", "ContentID=" + DbController.encodeSQLNumber(ContentId));
                            //
                            // todo -- looks like the tool code did not come with the migration ?
                            //
                        }
                    }
                }
                string ButtonList = "";
                result = AdminUIController.getToolForm(core, Stream.Text, ButtonList);
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }
        //
    }
}

