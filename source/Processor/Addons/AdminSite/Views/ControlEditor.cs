
using System;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Exceptions;

namespace Contensive.Addons.AdminSite {
    public class ControlEditor {
        //
        //========================================================================
        /// <summary>
        /// Control edit tab
        /// </summary>
        /// <param name="core"></param>
        /// <param name="adminData"></param>
        /// <returns></returns>
        public static string get(CoreController core, AdminDataModel adminData) {
            string result = null;
            try {
                // todo
                AdminUIController.EditRecordClass editRecord = adminData.editRecord;
                //
                var tabPanel = new StringBuilderLegacyController();
                if (string.IsNullOrEmpty(adminData.adminContent.name)) {
                    //
                    // Content not found or not loaded
                    if (adminData.adminContent.id == 0) {
                        //
                        LogController.handleError(core, new GenericException("No content definition was specified for this page"));
                        return HtmlController.p("No content was specified.");
                    } else {
                        //
                        // Content Definition was not specified
                        LogController.handleError(core, new GenericException( "The content definition specified for this page [" + adminData.adminContent.id + "] was not found"));
                        return HtmlController.p("No content was specified.");
                    }
                }
                //
                // ----- Authoring status
                bool FieldRequired = false;
                //
                // ----- RecordID
                {
                    string fieldValue = (editRecord.id == 0) ? "(available after save)" : editRecord.id.ToString();
                    string fieldEditor = AdminUIController.getDefaultEditor_Text(core, "ignore", fieldValue, true, "");
                    string fieldHelp = "This is the unique number that identifies this record within this content.";
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Record Number", fieldHelp, true, false, ""));
                }
                //
                // -- Active
                {
                    string fieldEditor = HtmlController.checkbox("active", editRecord.active);
                    string fieldHelp = "When unchecked, add-ons can ignore this record as if it was temporarily deleted.";
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Active", fieldHelp, false, false, ""));
                }
                ////
                //// ----- If Page Content , check if this is the default PageNotFound page
                //if (adminContext.adminContent.contentTableName.ToLowerInvariant() == "ccpagecontent") {
                //    //
                //    // Landing Page
                //    {
                //        string fieldHelp = "If selected, this page will be displayed when a user comes to your website with just your domain name and no other page is requested. This is called your default Landing Page. Only one page on the site can be the default Landing Page. If you want a unique Landing Page for a specific domain name, add it in the 'Domains' content and the default will not be used for that docore.main_";
                //        bool Checked = ((editRecord.id != 0) && (editRecord.id == (core.siteProperties.getInteger("LandingPageID", 0))));
                //        string fieldEditor = (core.session.isAuthenticatedAdmin(core)) ? htmlController.checkbox("LandingPageID", Checked) : "<b>" + genericController.getYesNo(Checked) + "</b>" + htmlController.inputHidden("LandingPageID", Checked);
                //        tabPanel.Add(adminUIController.getEditRow(core, fieldEditor, "Default Landing Page", fieldHelp, false, false, ""));
                //    }
                //    //
                //    // Page Not Found
                //    {
                //        string fieldHelp = "If selected, this content will be displayed when a page can not be found. Only one page on the site can be marked.";
                //        bool isPageNotFoundRecord = ((editRecord.id != 0) && (editRecord.id == (core.siteProperties.getInteger("PageNotFoundPageID", 0))));
                //        string fieldEditor = (core.session.isAuthenticatedAdmin(core)) ? htmlController.checkbox("PageNotFound", isPageNotFoundRecord) : "<b>" + genericController.getYesNo(isPageNotFoundRecord) + "</b>" + htmlController.inputHidden("PageNotFound", isPageNotFoundRecord);
                //        tabPanel.Add(adminUIController.getEditRow(core, fieldEditor, "Default Page Not Found", fieldHelp, false, false, ""));
                //    }
                //    //
                //    // ----- Last Known Public Site URL
                //    {
                //        string FieldHelp = "This is the URL where this record was last displayed on the site. It may be blank if the record has not been displayed yet.";
                //        string fieldValue = linkAliasController.getLinkAlias(core, editRecord.id, "", "");
                //        string fieldEditor = (string.IsNullOrEmpty(fieldValue)) ? "unknown" : "<a href=\"" + htmlController.encodeHtml(fieldValue) + "\" target=\"_blank\">" + fieldValue + "</a>";
                //        tabPanel.Add(adminUIController.getEditRow(core, fieldEditor, "Last Known Public URL", FieldHelp, false, false, ""));
                //    }
                //}
                //
                // -- GUID
                {
                    string fieldValue = GenericController.encodeText(editRecord.fieldsLc["ccguid"].value);
                    string FieldHelp = "This is a unique number that identifies this record globally. A GUID is not required, but when set it should never be changed. GUIDs are used to synchronize records. When empty, you can create a new guid. Only Developers can modify the guid.";
                    string fieldEditor = "";
                    if (string.IsNullOrEmpty(fieldValue)) {
                        //
                        // add a set button
                        string fieldId = "setGuid" + GenericController.GetRandomInteger(core).ToString();
                        string buttonCell = HtmlController.div(AdminUIController.getButtonPrimary("Set", "var e=document.getElementById('" + fieldId + "');if(e){e.value='{" + GenericController.getGUIDString() + "}';this.disabled=true;}"), "col-xs-1");
                        string inputCell = HtmlController.div(AdminUIController.getDefaultEditor_Text(core, "ccguid", "", false, fieldId), "col-xs-11");
                        fieldEditor = HtmlController.div(HtmlController.div(buttonCell + inputCell, "row"), "container-fluid");
                    } else {
                        //
                        // field is read-only except for developers
                        fieldEditor = AdminUIController.getDefaultEditor_Text(core, "ccguid", fieldValue, !core.session.isAuthenticatedDeveloper(core), "");
                    }
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "GUID", FieldHelp, false, false, ""));
                }
                //
                // ----- EID (Encoded ID)
                {
                    if (GenericController.vbUCase(adminData.adminContent.tableName) == GenericController.vbUCase("ccMembers")) {
                        bool AllowEID = (core.siteProperties.getBoolean("AllowLinkLogin", true)) || (core.siteProperties.getBoolean("AllowLinkRecognize", true));
                        string fieldHelp = "";
                        string fieldEditor = "";
                        if (!AllowEID) {
                            fieldEditor = "(link login and link recognize are disabled in security preferences)";
                        } else if (editRecord.id == 0) {
                            fieldEditor = "(available after save)";
                        } else {
                            string eidQueryString = "eid=" + Processor.Controllers.SecurityController.encodeToken(core, editRecord.id, core.doc.profileStartTime);
                            string sampleUrl = core.webServer.requestProtocol + core.webServer.requestDomain + appRootPath + core.siteProperties.serverPageDefault + "?" + eidQueryString;
                            if (core.siteProperties.getBoolean("AllowLinkLogin", true)) {
                                fieldHelp = "If " + eidQueryString + " is added to a url querystring for this site, the user be logged in as this person.";
                            } else {
                                fieldHelp = "If " + eidQueryString + " is added to a url querystring for this site, the user be recognized in as this person, but not logged in.";
                            }
                            fieldHelp += " To enable, disable or modify this feature, use the security tab on the Preferences page.";
                            fieldHelp += "<br>For example: " + sampleUrl;
                            fieldEditor = AdminUIController.getDefaultEditor_Text(core, "ignore_eid", eidQueryString, true, "");
                        }
                        tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Member Link Login Querystring", fieldHelp, true, false, ""));
                    }
                }
                //
                // ----- Controlling Content
                {
                    string HTMLFieldString = "";
                    string FieldHelp = "The content in which this record is stored. This is similar to a database table.";
                    CDefFieldModel field = null;
                    if (adminData.adminContent.fields.ContainsKey("contentcontrolid")) {
                        field = adminData.adminContent.fields["contentcontrolid"];
                        //
                        // if this record has a parent id, only include CDefs compatible with the parent record - otherwise get all for the table
                        FieldHelp = GenericController.encodeText(field.helpMessage);
                        FieldRequired = GenericController.encodeBoolean(field.required);
                        int FieldValueInteger = (editRecord.contentControlId.Equals(0)) ? adminData.adminContent.id : editRecord.contentControlId;
                        if (!core.session.isAuthenticatedAdmin(core)) {
                            HTMLFieldString = HTMLFieldString + HtmlController.inputHidden("ContentControlID", FieldValueInteger);
                        } else {
                            string RecordContentName = editRecord.contentControlId_Name;
                            string TableName2 = CdefController.getContentTablename(core, RecordContentName);
                            int TableID = core.db.getRecordID("Tables", TableName2);
                            //
                            // Test for parentid
                            int ParentID = 0;
                            bool ContentSupportsParentID = false;
                            if (editRecord.id > 0) {
                                int CS = core.db.csOpenRecord(RecordContentName, editRecord.id);
                                if (core.db.csOk(CS)) {
                                    ContentSupportsParentID = core.db.csIsFieldSupported(CS, "ParentID");
                                    if (ContentSupportsParentID) {
                                        ParentID = core.db.csGetInteger(CS, "ParentID");
                                    }
                                }
                                core.db.csClose(ref CS);
                            }
                            //
                            int LimitContentSelectToThisID = 0;
                            if (ContentSupportsParentID) {
                                //
                                // Parentid - restrict CDefs to those compatible with the parentid
                                if (ParentID != 0) {
                                    //
                                    // This record has a parent, set LimitContentSelectToThisID to the parent's CID
                                    int CSPointer = core.db.csOpenRecord(RecordContentName, ParentID, false, false, "ContentControlID");
                                    if (core.db.csOk(CSPointer)) {
                                        LimitContentSelectToThisID = core.db.csGetInteger(CSPointer, "ContentControlID");
                                    }
                                    core.db.csClose(ref CSPointer);
                                }

                            }
                            bool IsEmptyList = false;
                            if (core.session.isAuthenticatedAdmin(core) && (LimitContentSelectToThisID == 0)) {
                                //
                                // administrator, and either ( no parentid or does not support it), let them select any content compatible with the table
                                string sqlFilter = "(ContentTableID=" + TableID + ")";
                                int contentCID = core.db.getRecordID(Processor.Models.Db.ContentModel.contentName, Processor.Models.Db.ContentModel.contentName);
                                HTMLFieldString += AdminUIController.getDefaultEditor_LookupContent(core, "contentcontrolid", FieldValueInteger, contentCID, ref IsEmptyList, false, "", "", true, sqlFilter);
                                FieldHelp = FieldHelp + " (Only administrators have access to this control. Changing the Controlling Content allows you to change who can author the record, as well as how it is edited.)";
                            } else {
                                //
                                // Limit the list to only those cdefs that are within the record's parent contentid
                                RecordContentName = editRecord.contentControlId_Name;
                                TableName2 = CdefController.getContentTablename(core, RecordContentName);
                                TableID = core.db.getRecordID("Tables", TableName2);
                                int CSPointer = core.db.csOpen("Content", "ContentTableID=" + TableID, "", true, 0, false, false, "ContentControlID");
                                string CIDList = "";
                                while (core.db.csOk(CSPointer)) {
                                    int ChildCID = core.db.csGetInteger(CSPointer, "ID");
                                    if (CdefController.isWithinContent(core, ChildCID, LimitContentSelectToThisID)) {
                                        if ((core.session.isAuthenticatedAdmin(core)) || (core.session.isAuthenticatedContentManager(core, CdefController.getContentNameByID(core, ChildCID)))) {
                                            CIDList = CIDList + "," + ChildCID;
                                        }
                                    }
                                    core.db.csGoNext(CSPointer);
                                }
                                core.db.csClose(ref CSPointer);
                                if (!string.IsNullOrEmpty(CIDList)) {
                                    CIDList = CIDList.Substring(1);
                                    string sqlFilter = "(id in (" + CIDList + "))";
                                    int contentCID = core.db.getRecordID(Processor.Models.Db.ContentModel.contentName, Processor.Models.Db.ContentModel.contentName);
                                    HTMLFieldString += AdminUIController.getDefaultEditor_LookupContent(core, "contentcontrolid", FieldValueInteger, contentCID, ref IsEmptyList, false, "", "", true, sqlFilter);
                                    FieldHelp = FieldHelp + " (Only administrators have access to this control. Changing the Controlling Content allows you to change who can author the record, as well as how it is edited. This record includes a Parent field, so your choices for controlling content are limited to those compatible with the parent of this record.)";
                                }
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(HTMLFieldString)) {
                        HTMLFieldString = editRecord.contentControlId_Name;
                    }
                    tabPanel.Add(AdminUIController.getEditRow(core, HTMLFieldString, "Controlling Content", FieldHelp, FieldRequired, false, ""));
                }
                //
                // ----- Created By
                {
                    string FieldHelp = "The people account of the user who created this record.";
                    string fieldValue = "";
                    if (editRecord.id == 0) {
                        fieldValue = "(available after save)";
                    } else {
                        int FieldValueInteger = editRecord.createdBy.id;
                        if (FieldValueInteger == 0) {
                            fieldValue = "(not set)";
                        } else {
                            int CSPointer = core.db.csOpen("people", "(id=" + FieldValueInteger + ")", "name,active", false);
                            if (!core.db.csOk(CSPointer)) {
                                fieldValue = "#" + FieldValueInteger + ", (deleted)";
                            } else {
                                fieldValue = "#" + FieldValueInteger + ", " + core.db.csGet(CSPointer, "name");
                                if (!core.db.csGetBoolean(CSPointer, "active")) {
                                    fieldValue += " (inactive)";
                                }
                            }
                            core.db.csClose(ref CSPointer);
                        }
                    }
                    string fieldEditor = AdminUIController.getDefaultEditor_Text(core, "ignore_createdBy", fieldValue, true, "");
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Created By", FieldHelp, FieldRequired, false, ""));
                }
                //
                // ----- Created Date
                {
                    string FieldHelp = "The date and time when this record was originally created.";
                    string fieldValue = "";
                    if (editRecord.id == 0) {
                        fieldValue = "(available after save)";
                    } else {
                        if (encodeDateMinValue(editRecord.dateAdded) == DateTime.MinValue) {
                            fieldValue = "(not set)";
                        } else {
                            fieldValue = editRecord.dateAdded.ToString();
                        }
                    }
                    string fieldEditor = AdminUIController.getDefaultEditor_Text(core, "ignore_createdDate", fieldValue, true, "");
                    //string fieldEditor = htmlController.inputText( core,"ignore", fieldValue, -1, -1, "", false, true);
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Created Date", FieldHelp, FieldRequired, false, ""));
                }
                //
                // ----- Modified By
                {
                    string FieldHelp = "The people account of the last user who modified this record.";
                    string fieldValue = "";
                    if (editRecord.id == 0) {
                        fieldValue = "(available after save)";
                    } else {
                        int FieldValueInteger = editRecord.modifiedBy.id;
                        if (FieldValueInteger == 0) {
                            fieldValue = "(not set)";
                        } else {
                            int CSPointer = core.db.csOpen("people", "(id=" + FieldValueInteger + ")", "name,active", false);
                            if (!core.db.csOk(CSPointer)) {
                                fieldValue = "#" + FieldValueInteger + ", (deleted)";
                            } else {
                                fieldValue = "#" + FieldValueInteger + ", " + core.db.csGet(CSPointer, "name");
                                if (!core.db.csGetBoolean(CSPointer, "active")) {
                                    fieldValue += " (inactive)";
                                }
                            }
                            core.db.csClose(ref CSPointer);
                        }
                    }
                    string fieldEditor = AdminUIController.getDefaultEditor_Text(core, "ignore_modifiedBy", fieldValue, true, "");
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Modified By", FieldHelp, FieldRequired, false, ""));
                }
                //
                // ----- Modified Date
                {
                    string FieldHelp = "The date and time when this record was last modified.";
                    string fieldValue = "";
                    if (editRecord.id == 0) {
                        fieldValue = "(available after save)";
                    } else {
                        if (encodeDateMinValue(editRecord.modifiedDate) == DateTime.MinValue) {
                            fieldValue = "(not set)";
                        } else {
                            fieldValue = editRecord.modifiedDate.ToString();
                        }
                    }
                    string fieldEditor = AdminUIController.getDefaultEditor_Text(core, "ignore_modifiedBy", fieldValue, true, "");
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Modified Date", FieldHelp, false, false, ""));
                }
                string s = AdminUIController.editTable( tabPanel.Text );
                result = AdminUIController.getEditPanel(core, (!adminData.allowAdminTabs), "Control Information", "", s);
                adminData.EditSectionPanelCount = adminData.EditSectionPanelCount + 1;
                tabPanel = null;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }

    }
}
