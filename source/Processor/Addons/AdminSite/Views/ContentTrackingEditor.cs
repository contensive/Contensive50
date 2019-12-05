
using System;
using Contensive.Processor.Addons.AdminSite.Controllers;
using Contensive.Processor.Controllers;

namespace Contensive.Processor.Addons.AdminSite {
    public class ContentTrackingEditor {
        //
        //========================================================================
        /// <summary>
        /// content tracking edit tab
        /// </summary>
        /// <param name="core"></param>
        /// <param name="adminData"></param>
        /// <returns></returns>
        public static string get(CoreController core, AdminDataModel adminData, EditorEnvironmentModel editorEnv) {
            string tempGetForm_Edit_ContentTracking = null;
            try {
                Contensive.Processor.Addons.AdminSite.Models.EditRecordModel editRecord = adminData.editRecord;
                string HTMLFieldString = null;
                int RecordCount = 0;
                int ContentWatchListId = 0;
                StringBuilderLegacyController FastString = null;
                //
                if (adminData.adminContent.allowContentTracking) {
                    FastString = new StringBuilderLegacyController();
                    //
                    if (!adminData.contentWatchLoaded) {
                        //
                        // ----- Load in the record to print
                        //
                        adminData.loadContentTrackingDataBase(core);
                        adminData.loadContentTrackingResponse(core);
                        //        Call LoadAndSaveCalendarEvents
                    }
                    using (var CSLists = new CsModel(core)) {
                        CSLists.open("Content Watch Lists", "name<>" + DbController.encodeSQLText(""), "ID");
                        if (CSLists.ok()) {
                            //
                            // ----- Content Watch Lists, checking the ones that have active rules
                            RecordCount = 0;
                            while (CSLists.ok()) {
                                ContentWatchListId = CSLists.getInteger("id");
                                //
                                if (adminData.contentWatchRecordID != 0) {
                                    using (var CSRules = new CsModel(core)) {
                                        CSRules.open("Content Watch List Rules", "(ContentWatchID=" + adminData.contentWatchRecordID + ")AND(ContentWatchListID=" + ContentWatchListId + ")");
                                        if (editRecord.userReadOnly) {
                                            HTMLFieldString = GenericController.encodeText(CSRules.ok());
                                        } else {
                                            HTMLFieldString = HtmlController.checkbox("ContentWatchList." + CSLists.getText("ID"), CSRules.ok());
                                        }
                                    }
                                } else {
                                    if (editRecord.userReadOnly) {
                                        HTMLFieldString = GenericController.encodeText(false);
                                    } else {
                                        HTMLFieldString = HtmlController.checkbox("ContentWatchList." + CSLists.getText("ID"), false);
                                    }
                                }
                                //
                                FastString.Add(AdminUIController.getEditRowLegacy(core, HTMLFieldString, "Include in " + CSLists.getText("name"), "When true, this Content Record can be included in the '" + CSLists.getText("name") + "' list", false, false, ""));
                                CSLists.goNext();
                                RecordCount = RecordCount + 1;
                            }
                            //
                            // ----- Whats New Headline (editable)
                            //
                            if (editRecord.userReadOnly) {
                                HTMLFieldString = HtmlController.encodeHtml(adminData.contentWatchLinkLabel);
                            } else {
                                HTMLFieldString = HtmlController.inputText_Legacy(core, "ContentWatchLinkLabel", adminData.contentWatchLinkLabel, 1, core.siteProperties.defaultFormInputWidth);
                            }
                            FastString.Add(AdminUIController.getEditRowLegacy(core, HTMLFieldString, "Caption", "This caption is displayed on all Content Watch Lists, linked to the location on the web site where this content is displayed. RSS feeds created from Content Watch Lists will use this caption as the record title if not other field is selected in the Content Definition.", false, true, "ContentWatchLinkLabel"));
                            //
                            // ----- Whats New Expiration
                            //
                            if (editRecord.userReadOnly) {
                                HTMLFieldString = AdminUIController.getDefaultEditor_dateTime(core, "ContentWatchExpires", adminData.contentWatchExpires, true, "", false, "");
                            } else {
                                HTMLFieldString = AdminUIController.getDefaultEditor_dateTime(core, "ContentWatchExpires", adminData.contentWatchExpires, false, "", false, "");
                            }
                            FastString.Add(AdminUIController.getEditRowLegacy(core, HTMLFieldString, "Expires", "When this record is included in a What's New list, this record is blocked from the list after this date.", false, false, ""));
                            //
                            // ----- Public Link (read only)
                            //
                            HTMLFieldString = adminData.contentWatchLink;
                            if (string.IsNullOrEmpty(HTMLFieldString)) {
                                HTMLFieldString = "(must first be viewed on public site)";
                            }
                            FastString.Add(AdminUIController.getEditRowLegacy(core, HTMLFieldString, "Location on Site", "The public site URL where this content was last viewed.", false, false, ""));
                            //
                            string s = ""
                                + AdminUIController.editTable(FastString.Text)
                                + HtmlController.inputHidden("WhatsNewResponse", "-1")
                                + HtmlController.inputHidden("contentwatchrecordid", adminData.contentWatchRecordID.ToString());
                            tempGetForm_Edit_ContentTracking = AdminUIController.getEditPanel(core, (!adminData.allowAdminTabs), "Content Tracking", "Include in Content Watch Lists", s);
                            adminData.editSectionPanelCount = adminData.editSectionPanelCount + 1;
                            //
                        }
                    }
                    FastString = null;
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return tempGetForm_Edit_ContentTracking;
        }


    }
}
