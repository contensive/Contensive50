
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
//
namespace Contensive.Addons.Tools {
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
            return GetForm_DefineContentFieldsFromTable(((CPClass)cpBase).core);
        }
        //
        //=============================================================================
        /// <summary>
        /// Remove all Content Fields and rebuild them from the fields found in a table
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        //
        private string GetForm_DefineContentFieldsFromTable(CoreController core) {
            string result = "";
            try {
                string Button = core.docProperties.getText("Button");
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                //
                Stream.Add(SpanClassAdminNormal + "<strong><A href=\"" + core.webServer.requestPage + "?af=" + AdminFormToolRoot + "\">Tools</A></strong></SPAN>");
                Stream.Add(SpanClassAdminNormal + ":Create Content Fields from Table</SPAN>");
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
                int CSPointer = core.db.csOpen("Content", "", "name");
                while (core.db.csOk(CSPointer)) {
                    Stream.Add("<option value=\"" + core.db.csGetText(CSPointer, "name") + "\">" + core.db.csGetText(CSPointer, "name") + "</option>");
                    ItemCount = ItemCount + 1;
                    core.db.csGoNext(CSPointer);
                }
                if (ItemCount == 0) {
                    Stream.Add("<option value=\"-1\">System</option>");
                }
                Stream.Add("</select></td>");
                Stream.Add("</tr>");
                //
                Stream.Add("<tr>");
                Stream.Add("<TD>&nbsp;</td>");
                Stream.Add("<TD>" + HtmlController.getHtmlInputSubmit(ButtonCreateFields) + "</td>");
                //Stream.Add("<TD><INPUT type=\"submit\" value=\"" + ButtonCreateFields + "\" name=\"Button\"></td>");
                Stream.Add("</tr>");
                //
                Stream.Add("<tr>");
                Stream.Add("<td width=\"150\"><IMG alt=\"\" src=\"/ccLib/Images/spacer.gif\" width=\"150\" height=\"1\"></td>");
                Stream.Add("<td width=\"99%\"><IMG alt=\"\" src=\"/ccLib/Images/spacer.gif\" width=\"100%\" height=\"1\"></td>");
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
                        int ContentID = Processor.Models.Domain.CDefModel.getContentId(core, ContentName);
                        if (ContentID == 0) {
                            Stream.Add("GetContentID failed. Fields were not changed.");
                        } else {
                            core.db.deleteContentRecords("Content Fields", "ContentID=" + core.db.encodeSQLNumber(ContentID));
                            //
                            // todo -- looks like the tool code did not come with the migration ?
                            //
                        }
                    }
                }
                string ButtonList = "";
                result = AdminUIController.getToolForm(core, Stream.Text, ButtonList);
                //result = adminUIController.getToolFormOpen(core, ButtonList) + Stream.Text + adminUIController.getToolFormClose(core, ButtonList);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
    }
}

