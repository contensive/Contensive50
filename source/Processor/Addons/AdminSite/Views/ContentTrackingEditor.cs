
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
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
using Contensive.Addons.Tools;
using static Contensive.Processor.AdminUIController;
//
namespace Contensive.Addons.AdminSite {
    public class ContentTrackingEditor {
        //
        //========================================================================
        /// <summary>
        /// content tracking edit tab
        /// </summary>
        /// <param name="core"></param>
        /// <param name="adminData"></param>
        /// <returns></returns>
        public static string get(CoreController core, AdminDataModel adminData) {
            string tempGetForm_Edit_ContentTracking = null;
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
                //
                //
                int CSRules = 0;
                string HTMLFieldString = null;
                int CSLists = 0;
                int RecordCount = 0;
                int ContentWatchListID = 0;
                StringBuilderLegacyController FastString = null;
                //string Copy = null;
                //adminUIController Adminui = new adminUIController(core);
                //
                if (adminData.adminContent.allowContentTracking) {
                    FastString = new StringBuilderLegacyController();
                    //
                    if (!adminData.ContentWatchLoaded) {
                        //
                        // ----- Load in the record to print
                        //
                        adminData.LoadContentTrackingDataBase(core);
                        adminData.LoadContentTrackingResponse(core);
                        //        Call LoadAndSaveCalendarEvents
                    }
                    CSLists = core.db.csOpen("Content Watch Lists", "name<>" + core.db.encodeSQLText(""), "ID");
                    if (core.db.csOk(CSLists)) {
                        //
                        // ----- Open the panel
                        //
                        //Call core.main_PrintPanelTop("ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)
                        //Call FastString.Add(adminUIController.EditTableOpen)
                        //Call FastString.Add(vbCrLf & "<tr><td colspan=""3"" class=""ccAdminEditSubHeader"">Content Tracking</td></tr>")
                        //            '
                        //            ' ----- Print matching Content Watch fields
                        //            '
                        //            Call FastString.Add(core.main_GetFormInputHidden("WhatsNewResponse", -1))
                        //            Call FastString.Add(core.main_GetFormInputHidden("contentwatchrecordid", ContentWatchRecordID))
                        //
                        // ----- Content Watch Lists, checking the ones that have active rules
                        //
                        RecordCount = 0;
                        while (core.db.csOk(CSLists)) {
                            ContentWatchListID = core.db.csGetInteger(CSLists, "id");
                            //
                            if (adminData.ContentWatchRecordID != 0) {
                                CSRules = core.db.csOpen("Content Watch List Rules", "(ContentWatchID=" + adminData.ContentWatchRecordID + ")AND(ContentWatchListID=" + ContentWatchListID + ")");
                                if (editRecord.Read_Only) {
                                    HTMLFieldString = GenericController.encodeText(core.db.csOk(CSRules));
                                } else {
                                    HTMLFieldString = HtmlController.checkbox("ContentWatchList." + core.db.csGet(CSLists, "ID"), core.db.csOk(CSRules));
                                }
                                core.db.csClose(ref CSRules);
                            } else {
                                if (editRecord.Read_Only) {
                                    HTMLFieldString = GenericController.encodeText(false);
                                } else {
                                    HTMLFieldString = HtmlController.checkbox("ContentWatchList." + core.db.csGet(CSLists, "ID"), false);
                                }
                            }
                            //
                            FastString.Add(AdminUIController.getEditRowLegacy(core, HTMLFieldString, "Include in " + core.db.csGet(CSLists, "name"), "When true, this Content Record can be included in the '" + core.db.csGet(CSLists, "name") + "' list", false, false, ""));
                            core.db.csGoNext(CSLists);
                            RecordCount = RecordCount + 1;
                        }
                        //
                        // ----- Whats New Headline (editable)
                        //
                        if (editRecord.Read_Only) {
                            HTMLFieldString = HtmlController.encodeHtml(adminData.ContentWatchLinkLabel);
                        } else {
                            HTMLFieldString = HtmlController.inputText(core, "ContentWatchLinkLabel", adminData.ContentWatchLinkLabel, 1, core.siteProperties.defaultFormInputWidth);
                            //HTMLFieldString = "<textarea rows=""1"" name=""ContentWatchLinkLabel"" cols=""" & core.app.SiteProperty_DefaultFormInputWidth & """>" & ContentWatchLinkLabel & "</textarea>"
                        }
                        FastString.Add(AdminUIController.getEditRowLegacy(core, HTMLFieldString, "Caption", "This caption is displayed on all Content Watch Lists, linked to the location on the web site where this content is displayed. RSS feeds created from Content Watch Lists will use this caption as the record title if not other field is selected in the Content Definition.", false, true, "ContentWatchLinkLabel"));
                        //
                        // ----- Whats New Expiration
                        //
                        if (editRecord.Read_Only) {
                            HTMLFieldString = AdminUIController.getDefaultEditor_Date(core, "ContentWatchExpires", adminData.ContentWatchExpires, true, "", false, "");
                        } else {
                            HTMLFieldString = AdminUIController.getDefaultEditor_Date(core, "ContentWatchExpires", adminData.ContentWatchExpires, false, "", false, "");
                        }
                        FastString.Add(AdminUIController.getEditRowLegacy(core, HTMLFieldString, "Expires", "When this record is included in a What's New list, this record is blocked from the list after this date.", false, false, ""));
                        //
                        // ----- Public Link (read only)
                        //
                        HTMLFieldString = adminData.ContentWatchLink;
                        if (string.IsNullOrEmpty(HTMLFieldString)) {
                            HTMLFieldString = "(must first be viewed on public site)";
                        }
                        FastString.Add(AdminUIController.getEditRowLegacy(core, HTMLFieldString, "Location on Site", "The public site URL where this content was last viewed.", false, false, ""));
                        //
                        // removed 11/27/07 - RSS clicks not counted, rc/ri method of counting link clicks is not reliable.
                        //            '
                        //            ' ----- Clicks (read only)
                        //            '
                        //            HTMLFieldString = ContentWatchClicks
                        //            If HTMLFieldString = "" Then
                        //                HTMLFieldString = 0
                        //                End If
                        //            Call FastString.Add(adminUIController.GetEditRow(core, HTMLFieldString, "Clicks", "The number of site users who have clicked this link in what's new lists", False, False, ""))
                        //
                        // ----- close the panel
                        //
                        string s = ""
                            + AdminUIController.editTable(FastString.Text) 
                            + HtmlController.inputHidden("WhatsNewResponse", "-1") 
                            + HtmlController.inputHidden("contentwatchrecordid", adminData.ContentWatchRecordID.ToString());
                        tempGetForm_Edit_ContentTracking = AdminUIController.getEditPanel(core, (!adminData.allowAdminTabs), "Content Tracking", "Include in Content Watch Lists", s);
                        adminData.EditSectionPanelCount = adminData.EditSectionPanelCount + 1;
                        //
                    }
                    core.db.csClose(ref CSLists);
                    FastString = null;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetForm_Edit_ContentTracking;
        }


    }
}
