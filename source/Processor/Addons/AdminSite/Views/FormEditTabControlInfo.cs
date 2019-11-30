﻿
using System;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Exceptions;
using Contensive.Processor.Addons.AdminSite.Controllers;
using System.Net;
using Contensive.Models.Db;

namespace Contensive.Processor.Addons.AdminSite {
    public class FormEditTabControlInfo {
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
                bool disabled = false;
                //
                var tabPanel = new StringBuilderLegacyController();
                if (string.IsNullOrEmpty(adminData.adminContent.name)) {
                    //
                    // Content not found or not loaded
                    if (adminData.adminContent.id == 0) {
                        //
                        LogController.logError(core, new GenericException("No content definition was specified for this page"));
                        return HtmlController.p("No content was specified.");
                    } else {
                        //
                        // Content Definition was not specified
                        LogController.logError(core, new GenericException("The content definition specified for this page [" + adminData.adminContent.id + "] was not found"));
                        return HtmlController.p("No content was specified.");
                    }
                }
                //
                // ----- Authoring status
                bool FieldRequired = false;
                //
                // ----- RecordID
                {
                    string fieldValue = (adminData.editRecord.id == 0) ? "(available after save)" : adminData.editRecord.id.ToString();
                    string fieldEditor = AdminUIController.getDefaultEditor_text(core, "ignore", fieldValue, true, "");
                    string fieldHelp = "This is the unique number that identifies this record within this content.";
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Record Number", fieldHelp, true, false, ""));
                }
                //
                // -- Active
                {
                    string htmlId = "fieldActive";
                    string fieldEditor = HtmlController.checkbox("active", adminData.editRecord.active, htmlId, disabled, "", adminData.editRecord.userReadOnly);
                    string fieldHelp = "When unchecked, add-ons can ignore this record as if it was temporarily deleted.";
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Active", fieldHelp, false, false, htmlId));
                }
                //
                // -- GUID
                {
                    string htmlId = "fieldGuid";
                    string fieldValue = GenericController.encodeText(adminData.editRecord.fieldsLc["ccguid"].value);
                    string FieldHelp = "This is a unique number that identifies this record globally. A GUID is not required, but when set it should never be changed. GUIDs are used to synchronize records. When empty, you can create a new guid. Only Developers can modify the guid.";
                    string fieldEditor = "";
                    if (adminData.editRecord.userReadOnly) {
                        //
                        // -- readonly
                        fieldEditor = AdminUIController.getDefaultEditor_text(core, "ignore", fieldValue, true, "");
                    } else if (string.IsNullOrEmpty(fieldValue)) {
                        //
                        // add a set button
                        string fieldId = "setGuid" + GenericController.getRandomInteger(core).ToString();
                        string buttonCell = HtmlController.div(AdminUIController.getButtonPrimary("Set", "var e=document.getElementById('" + fieldId + "');if(e){e.value='" + GenericController.getGUID(true) + "';this.disabled=true;}"), "col-xs-1");
                        string inputCell = HtmlController.div(AdminUIController.getDefaultEditor_text(core, "ccguid", "", false, htmlId), "col-xs-11");
                        fieldEditor = HtmlController.div(HtmlController.div(buttonCell + inputCell, "row"), "container-fluid");
                    } else {
                        //
                        // field is read-only except for developers
                        fieldEditor = AdminUIController.getDefaultEditor_text(core, "ccguid", fieldValue, !core.session.isAuthenticatedDeveloper(), htmlId);
                    }
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "GUID", FieldHelp, false, false, htmlId));
                }
                //
                // ----- EID (Encoded ID)
                {
                    if (GenericController.toUCase(adminData.adminContent.tableName) == GenericController.toUCase("ccMembers")) {
                        string htmlId = "fieldGuid";
                        bool AllowEId = (core.siteProperties.getBoolean("AllowLinkLogin", true)) || (core.siteProperties.getBoolean("AllowLinkRecognize", true));
                        string fieldHelp = "";
                        string fieldEditor = "";
                        if (!AllowEId) {
                            fieldEditor = "(link login and link recognize are disabled in security preferences)";
                        } else if (adminData.editRecord.id == 0) {
                            fieldEditor = "(available after save)";
                        } else {
                            string eidQueryString = "eid=" + WebUtility.UrlEncode(Processor.Controllers.SecurityController.encodeToken(core, adminData.editRecord.id, core.doc.profileStartTime));
                            string sampleUrl = core.webServer.requestProtocol + core.webServer.requestDomain + "/" + core.siteProperties.serverPageDefault + "?" + eidQueryString;
                            if (core.siteProperties.getBoolean("AllowLinkLogin", true)) {
                                fieldHelp = "If " + eidQueryString + " is added to a url querystring for this site, the user be logged in as this person.";
                            } else {
                                fieldHelp = "If " + eidQueryString + " is added to a url querystring for this site, the user be recognized in as this person, but not logged in.";
                            }
                            fieldHelp += " To enable, disable or modify this feature, use the security tab on the Preferences page.";
                            fieldHelp += "<br>For example: " + sampleUrl;
                            fieldEditor = AdminUIController.getDefaultEditor_text(core, "ignore_eid", eidQueryString, true, htmlId);
                        }
                        tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Member Link Login Querystring", fieldHelp, true, false, htmlId));
                    }
                }
                //
                // ----- Controlling Content
                {
                    string HTMLFieldString = "";
                    string FieldHelp = "The content in which this record is stored. This is similar to a database table.";
                    ContentFieldMetadataModel field = null;
                    if (adminData.adminContent.fields.ContainsKey("contentcontrolid")) {
                        field = adminData.adminContent.fields["contentcontrolid"];
                        //
                        // if this record has a parent id, only include CDefs compatible with the parent record - otherwise get all for the table
                        FieldHelp = GenericController.encodeText(field.helpMessage);
                        FieldRequired = GenericController.encodeBoolean(field.required);
                        int FieldValueInteger = (adminData.editRecord.contentControlId.Equals(0)) ? adminData.adminContent.id : adminData.editRecord.contentControlId;
                        if (!core.session.isAuthenticatedAdmin()) {
                            HTMLFieldString = HTMLFieldString + HtmlController.inputHidden("contentControlId", FieldValueInteger);
                        } else {
                            string RecordContentName = adminData.editRecord.contentControlId_Name;
                            string TableName2 = MetadataController.getContentTablename(core, RecordContentName);
                            int TableId = MetadataController.getRecordIdByUniqueName(core, "Tables", TableName2);
                            //
                            // Test for parentid
                            int ParentId = 0;
                            bool ContentSupportsParentId = false;
                            if (adminData.editRecord.id > 0) {
                                using (var csData = new CsModel(core)) {
                                    if (csData.openRecord(RecordContentName, adminData.editRecord.id)) {
                                        ContentSupportsParentId = csData.isFieldSupported("ParentID");
                                        if (ContentSupportsParentId) {
                                            ParentId = csData.getInteger("ParentID");
                                        }
                                    }
                                }
                            }
                            bool IsEmptyList = false;
                            if (core.session.isAuthenticatedAdmin()) {
                                //
                                // administrator, and either ( no parentid or does not support it), let them select any content compatible with the table
                                string sqlFilter = "(ContentTableID=" + TableId + ")";
                                int contentCId = MetadataController.getRecordIdByUniqueName(core, ContentModel.tableMetadata.contentName, ContentModel.tableMetadata.contentName);
                                HTMLFieldString += AdminUIController.getDefaultEditor_lookupContent(core, "contentcontrolid", FieldValueInteger, contentCId, ref IsEmptyList, adminData.editRecord.userReadOnly, "", "", true, sqlFilter);
                                FieldHelp = FieldHelp + " (Only administrators have access to this control. Changing the Controlling Content allows you to change who can author the record, as well as how it is edited.)";
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(HTMLFieldString)) {
                        HTMLFieldString = adminData.editRecord.contentControlId_Name;
                    }
                    tabPanel.Add(AdminUIController.getEditRow(core, HTMLFieldString, "Controlling Content", FieldHelp, FieldRequired, false, ""));
                }
                //
                // ----- Created By
                {
                    string FieldHelp = "The people account of the user who created this record.";
                    string fieldValue = "";
                    if (adminData.editRecord == null) {
                        fieldValue = "(not set)";
                    } else if (adminData.editRecord.id == 0 )  {
                        fieldValue = "(available after save)";
                    } else if (adminData.editRecord.createdBy == null ) {
                        fieldValue = "(not set)";
                    } else { 
                        int FieldValueInteger = adminData.editRecord.createdBy.id;
                        if (FieldValueInteger == 0) {
                            fieldValue = "(not set)";
                        } else {
                            using (var csData = new CsModel(core)) {
                                csData.open("people", "(id=" + FieldValueInteger + ")", "name,active", false);
                                if (!csData.ok()) {
                                    fieldValue = "#" + FieldValueInteger + ", (deleted)";
                                } else {
                                    fieldValue = "#" + FieldValueInteger + ", " + csData.getText("name");
                                    if (!csData.getBoolean("active")) {
                                        fieldValue += " (inactive)";
                                    }
                                }
                                csData.close();
                            }
                        }
                    }
                    string fieldEditor = AdminUIController.getDefaultEditor_text(core, "ignore_createdBy", fieldValue, true, "");
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Created By", FieldHelp, FieldRequired, false, ""));
                }
                //
                // ----- Created Date
                {
                    string FieldHelp = "The date and time when this record was originally created.";
                    string fieldValue = "";
                    if (adminData.editRecord == null) {
                        fieldValue = "(not set)";
                    } else if (adminData.editRecord.id == 0) {
                        fieldValue = "(available after save)";
                    } else {
                        if (GenericController.encodeDateMinValue(adminData.editRecord.dateAdded) == DateTime.MinValue) {
                            fieldValue = "(not set)";
                        } else {
                            fieldValue = adminData.editRecord.dateAdded.ToString();
                        }
                    }
                    string fieldEditor = AdminUIController.getDefaultEditor_text(core, "ignore_createdDate", fieldValue, true, "");
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Created Date", FieldHelp, FieldRequired, false, ""));
                }
                //
                // ----- Modified By
                {
                    string FieldHelp = "The people account of the last user who modified this record.";
                    string fieldValue = "";
                    if (adminData.editRecord == null) {
                        fieldValue = "(not set)";
                    } else if (adminData.editRecord.id == 0) {
                        fieldValue = "(available after save)";
                    } else if (adminData.editRecord.modifiedBy == null) {
                        fieldValue = "(not set)";
                    } else {
                        int FieldValueInteger = adminData.editRecord.modifiedBy.id;
                        if (FieldValueInteger == 0) {
                            fieldValue = "(not set)";
                        } else {
                            using (var csData = new CsModel(core)) {
                                csData.open("people", "(id=" + FieldValueInteger + ")", "name,active", false);
                                if (!csData.ok()) {
                                    fieldValue = "#" + FieldValueInteger + ", (deleted)";
                                } else {
                                    fieldValue = "#" + FieldValueInteger + ", " + csData.getText("name");
                                    if (!csData.getBoolean("active")) {
                                        fieldValue += " (inactive)";
                                    }
                                }
                                csData.close();
                            }
                        }
                    }
                    string fieldEditor = AdminUIController.getDefaultEditor_text(core, "ignore_modifiedBy", fieldValue, true, "");
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Modified By", FieldHelp, FieldRequired, false, ""));
                }
                //
                // ----- Modified Date
                {
                    string FieldHelp = "The date and time when this record was last modified.";
                    string fieldValue = "";
                    if (adminData.editRecord == null) {
                        fieldValue = "(not set)";
                    } else if (adminData.editRecord.id == 0) {
                        fieldValue = "(available after save)";
                    } else {
                        if (GenericController.encodeDateMinValue(adminData.editRecord.modifiedDate) == DateTime.MinValue) {
                            fieldValue = "(not set)";
                        } else {
                            fieldValue = adminData.editRecord.modifiedDate.ToString();
                        }
                    }
                    string fieldEditor = AdminUIController.getDefaultEditor_text(core, "ignore_modifiedBy", fieldValue, true, "");
                    tabPanel.Add(AdminUIController.getEditRow(core, fieldEditor, "Modified Date", FieldHelp, false, false, ""));
                }
                string s = AdminUIController.editTable(tabPanel.Text);
                result = AdminUIController.getEditPanel(core, (!adminData.allowAdminTabs), "Control Information", "", s);
                adminData.editSectionPanelCount = adminData.editSectionPanelCount + 1;
                tabPanel = null;
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }

    }
}
