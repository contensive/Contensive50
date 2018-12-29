
using System;
using Contensive.Addons.AdminSite.Controllers;
using Contensive.Processor;
using Contensive.Processor.Controllers;

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
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
                string HTMLFieldString = null;
                int RecordCount = 0;
                int ContentWatchListID = 0;
                StringBuilderLegacyController FastString = null;
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
                    using (var CSLists = new CsModel(core)) {
                        CSLists.csOpen("Content Watch Lists", "name<>" + DbController.encodeSQLText(""), "ID");
                        if (CSLists.csOk()) {
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
                            while (CSLists.csOk()) {
                                ContentWatchListID = CSLists.csGetInteger("id");
                                //
                                if (adminData.ContentWatchRecordID != 0) {
                                    using (var CSRules = new CsModel(core)) {
                                        CSRules.csOpen("Content Watch List Rules", "(ContentWatchID=" + adminData.ContentWatchRecordID + ")AND(ContentWatchListID=" + ContentWatchListID + ")");
                                        if (editRecord.userReadOnly) {
                                            HTMLFieldString = GenericController.encodeText(CSRules.csOk());
                                        } else {
                                            HTMLFieldString = HtmlController.checkbox("ContentWatchList." + CSLists.csGet("ID"), CSRules.csOk());
                                        }
                                    }
                                } else {
                                    if (editRecord.userReadOnly) {
                                        HTMLFieldString = GenericController.encodeText(false);
                                    } else {
                                        HTMLFieldString = HtmlController.checkbox("ContentWatchList." + CSLists.csGet("ID"), false);
                                    }
                                }
                                //
                                FastString.Add(AdminUIController.getEditRowLegacy(core, HTMLFieldString, "Include in " + CSLists.csGet("name"), "When true, this Content Record can be included in the '" + CSLists.csGet("name") + "' list", false, false, ""));
                                CSLists.csGoNext();
                                RecordCount = RecordCount + 1;
                            }
                            //
                            // ----- Whats New Headline (editable)
                            //
                            if (editRecord.userReadOnly) {
                                HTMLFieldString = HtmlController.encodeHtml(adminData.ContentWatchLinkLabel);
                            } else {
                                HTMLFieldString = HtmlController.inputText(core, "ContentWatchLinkLabel", adminData.ContentWatchLinkLabel, 1, core.siteProperties.defaultFormInputWidth);
                                //HTMLFieldString = "<textarea rows=""1"" name=""ContentWatchLinkLabel"" cols=""" & core.app.SiteProperty_DefaultFormInputWidth & """>" & ContentWatchLinkLabel & "</textarea>"
                            }
                            FastString.Add(AdminUIController.getEditRowLegacy(core, HTMLFieldString, "Caption", "This caption is displayed on all Content Watch Lists, linked to the location on the web site where this content is displayed. RSS feeds created from Content Watch Lists will use this caption as the record title if not other field is selected in the Content Definition.", false, true, "ContentWatchLinkLabel"));
                            //
                            // ----- Whats New Expiration
                            //
                            if (editRecord.userReadOnly) {
                                HTMLFieldString = AdminUIController.getDefaultEditor_DateTime(core, "ContentWatchExpires", adminData.ContentWatchExpires, true, "", false, "");
                            } else {
                                HTMLFieldString = AdminUIController.getDefaultEditor_DateTime(core, "ContentWatchExpires", adminData.ContentWatchExpires, false, "", false, "");
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
                    }
                    FastString = null;
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return tempGetForm_Edit_ContentTracking;
        }


    }
}
