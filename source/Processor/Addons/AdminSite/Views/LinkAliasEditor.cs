﻿
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
    public class LinkAliasEditor {
        //
        //========================================================================
        //
        public static string GetForm_Edit_LinkAliases( CoreController core, AdminDataModel adminData, bool readOnlyField) {
            string tempGetForm_Edit_LinkAliases = null;
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
                //
                //
                int LinkCnt = 0;
                string LinkList = "";
                StringBuilderLegacyController f = new StringBuilderLegacyController();
                //adminUIController Adminui = new adminUIController(core);
                string linkAlias = null;
                string Link = null;
                int CS = 0;
                string tabContent = null;
                string TabDescription;
                //
                //
                // Link Alias value from the admin data
                //
                TabDescription = "Link Aliases are URLs used for this content that are more friendly to users and search engines. If you set the Link Alias field, this name will be used on the URL for this page. If you leave the Link Alias blank, the page name will be used. Below is a list of names that have been used previously and are still active. All of these entries when used in the URL will resolve to this page. The first entry in this list will be used to create menus on the site. To move an entry to the top, type it into the Link Alias field and save.";
                if (!core.siteProperties.allowLinkAlias) {
                    //
                    // Disabled
                    //
                    tabContent = "&nbsp;";
                    TabDescription = "<p>The Link Alias feature is currently disabled. To enable Link Aliases, check the box marked 'Allow Link Alias' on the Page Settings page found on the Navigator under 'Settings'.</p><p>" + TabDescription + "</p>";
                } else {
                    //
                    // Link Alias Field
                    //
                    linkAlias = "";
                    if (adminData.adminContent.fields.ContainsKey("linkalias")) {
                        linkAlias = GenericController.encodeText(editRecord.fieldsLc["linkalias"].value);
                    }
                    f.Add("<tr><td class=\"ccAdminEditCaption\">" + SpanClassAdminSmall + "Link Alias</td>");
                    f.Add("<td class=\"ccAdminEditField\" align=\"left\" colspan=\"2\">" + SpanClassAdminNormal);
                    if (readOnlyField) {
                        f.Add(linkAlias);
                    } else {
                        f.Add(HtmlController.inputText(core, "LinkAlias", linkAlias));
                    }
                    f.Add("</span></td></tr>");
                    //
                    // Override Duplicates
                    //
                    f.Add("<tr><td class=\"ccAdminEditCaption\">" + SpanClassAdminSmall + "Override Duplicates</td>");
                    f.Add("<td class=\"ccAdminEditField\" align=\"left\" colspan=\"2\">" + SpanClassAdminNormal);
                    if (readOnlyField) {
                        f.Add("No");
                    } else {
                        f.Add(HtmlController.checkbox("OverrideDuplicate", false));
                    }
                    f.Add("</span></td></tr>");
                    //
                    // Table of old Link Aliases
                    //
                    Link = core.doc.main_GetPageDynamicLink(editRecord.id, false);
                    CS = core.db.csOpen("Link Aliases", "pageid=" + editRecord.id, "ID Desc", true, 0, false, false, "name");
                    while (core.db.csOk(CS)) {
                        LinkList = LinkList + "<div style=\"margin-left:4px;margin-bottom:4px;\">" + HtmlController.encodeHtml(core.db.csGetText(CS, "name")) + "</div>";
                        LinkCnt = LinkCnt + 1;
                        core.db.csGoNext(CS);
                    }
                    core.db.csClose(ref CS);
                    if (LinkCnt > 0) {
                        f.Add("<tr><td class=\"ccAdminEditCaption\">" + SpanClassAdminSmall + "Previous Link Alias List</td>");
                        f.Add("<td class=\"ccAdminEditField\" align=\"left\" colspan=\"2\">" + SpanClassAdminNormal);
                        f.Add(LinkList);
                        f.Add("</span></td></tr>");
                    }
                    tabContent = AdminUIController.editTable( f.Text );
                }
                //
                tempGetForm_Edit_LinkAliases = AdminUIController.getEditPanel(core, (!adminData.allowAdminTabs), "Link Aliases", TabDescription, tabContent);
                adminData.EditSectionPanelCount = adminData.EditSectionPanelCount + 1;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetForm_Edit_LinkAliases;
        }

    }
}