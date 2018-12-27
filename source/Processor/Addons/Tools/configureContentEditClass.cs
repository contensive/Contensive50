
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
using Contensive.Addons.AdminSite.Controllers;
//
namespace Contensive.Addons.Tools {
    //
    public class configureContentEditClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// addon method, deliver complete Html admin site
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cpBase) {
            return configureContentEdit((CPClass)cpBase);
        }
        //
        //=============================================================================
        //   Get the Configure Edit
        //=============================================================================
        //
        public static string configureContentEdit( CPClass cp) {
            string result = "";
            CoreController core = cp.core;
            try {
                KeyPtrController Index = new KeyPtrController();
                string ButtonList = ButtonCancel + "," + ButtonSelect;
                string ToolButton = cp.Doc.GetText("Button");
                bool ReloadCDef = cp.Doc.GetBoolean("ReloadCDef");
                int ContentID = cp.Doc.GetInteger("" + RequestNameToolContentID + "");
                string dataSourceName = "default";
                string ContentName = "";
                Processor.Models.Domain.CDefDomainModel CDef = null;
                string TableName = "";
                if (ContentID > 0) {
                    ContentName = cp.Content.GetRecordName("content", ContentID);
                    if (!string.IsNullOrEmpty(ContentName)) {
                        TableName = cp.Content.GetTable(ContentName);
                        dataSourceName = cp.Content.GetDataSource(ContentName);
                        CDef = Processor.Models.Domain.CDefDomainModel.create(core, ContentID, true, true);
                    }
                }
                int RecordCount = 0;
                int CSPointer = 0;
                int formFieldId = 0;
                string StatusMessage = "";
                string ErrorMessage = "";
                if (CDef != null) {
                    //
                    if (!string.IsNullOrEmpty(ToolButton)) {
                        bool AllowContentAutoLoad = false;
                        if (ToolButton != ButtonCancel) {
                            //
                            // Save the form changes
                            //
                            AllowContentAutoLoad = cp.Site.GetBoolean("AllowContentAutoLoad", true);
                            cp.Site.SetProperty("AllowContentAutoLoad", "false");
                            //
                            // ----- Save the input
                            //
                            RecordCount = GenericController.encodeInteger(cp.Doc.GetInteger("dtfaRecordCount"));
                            if (RecordCount > 0) {
                                int RecordPointer = 0;
                                for (RecordPointer = 0; RecordPointer < RecordCount; RecordPointer++) {
                                    //
                                    string formFieldName = cp.Doc.GetText("dtfaName." + RecordPointer);
                                    int formFieldTypeId = cp.Doc.GetInteger("dtfaType." + RecordPointer);
                                    formFieldId = GenericController.encodeInteger(cp.Doc.GetInteger("dtfaID." + RecordPointer));
                                    bool formFieldInherited = cp.Doc.GetBoolean("dtfaInherited." + RecordPointer);
                                    //
                                    // problem - looking for the name in the Db using the form's name, but it could have changed.
                                    // have to look field up by id
                                    //
                                    foreach (KeyValuePair<string, Processor.Models.Domain.CDefFieldModel> cdefFieldKvp in CDef.fields) {
                                        if (cdefFieldKvp.Value.id == formFieldId) {
                                            //
                                            // Field was found in CDef
                                            //
                                            if (cdefFieldKvp.Value.inherited && (!formFieldInherited)) {
                                                //
                                                // Was inherited, but make a copy of the field
                                                //
                                                int CSTarget = core.db.csInsertRecord("Content Fields");
                                                if (core.db.csOk(CSTarget)) {
                                                    int CSSource = core.db.csOpenContentRecord("Content Fields", formFieldId);
                                                    if (core.db.csOk(CSSource)) {
                                                        core.db.csCopyRecord(CSSource, CSTarget);
                                                    }
                                                    core.db.csClose(ref CSSource);
                                                    formFieldId = core.db.csGetInteger(CSTarget, "ID");
                                                    core.db.csSet(CSTarget, "ContentID", ContentID);
                                                }
                                                core.db.csClose(ref CSTarget);
                                                ReloadCDef = true;
                                            } else if ((!cdefFieldKvp.Value.inherited) && (formFieldInherited)) {
                                                //
                                                // Was a field, make it inherit from it's parent
                                                //
                                                //CSTarget = CSTarget;
                                                core.db.deleteContentRecord("Content Fields", formFieldId);
                                                ReloadCDef = true;
                                            } else if ((!cdefFieldKvp.Value.inherited) && (!formFieldInherited)) {
                                                //
                                                // not inherited, save the field values and mark for a reload
                                                //
                                                if (true) {
                                                    if (formFieldName.IndexOf(" ") != -1) {
                                                        //
                                                        // remoave spaces from new name
                                                        //
                                                        StatusMessage = StatusMessage + "<LI>Field [" + formFieldName + "] was renamed [" + GenericController.vbReplace(formFieldName, " ", "") + "] because the field name can not include spaces.</LI>";
                                                        formFieldName = GenericController.vbReplace(formFieldName, " ", "");
                                                    }
                                                    //
                                                    string SQL = null;
                                                    //
                                                    if ((!string.IsNullOrEmpty(formFieldName)) && (formFieldTypeId != 0) && ((cdefFieldKvp.Value.nameLc == "") || (cdefFieldKvp.Value.fieldTypeId == 0))) {
                                                        //
                                                        // Create Db field, Field is good but was not before
                                                        //
                                                        core.db.createSQLTableField(dataSourceName, TableName, formFieldName, formFieldTypeId);
                                                        StatusMessage = StatusMessage + "<LI>Field [" + formFieldName + "] was saved to this content definition and a database field was created in [" + CDef.tableName + "].</LI>";
                                                    } else if ((string.IsNullOrEmpty(formFieldName)) || (formFieldTypeId == 0)) {
                                                        //
                                                        // name blank or type=0 - do nothing but tell them
                                                        //
                                                        if (string.IsNullOrEmpty(formFieldName) && formFieldTypeId == 0) {
                                                            ErrorMessage += "<LI>Field number " + (RecordPointer + 1) + " was saved to this content definition but no database field was created because a name and field type are required.</LI>";
                                                        } else if (formFieldName == "unnamedfield" + formFieldId.ToString()) {
                                                            ErrorMessage += "<LI>Field number " + (RecordPointer + 1) + " was saved to this content definition but no database field was created because a field name is required.</LI>";
                                                        } else {
                                                            ErrorMessage += "<LI>Field [" + formFieldName + "] was saved to this content definition but no database field was created because a field type are required.</LI>";
                                                        }
                                                    } else if ((formFieldName == cdefFieldKvp.Value.nameLc) && (formFieldTypeId != cdefFieldKvp.Value.fieldTypeId)) {
                                                        //
                                                        // Field Type changed, must be done manually
                                                        //
                                                        ErrorMessage += "<LI>Field [" + formFieldName + "] changed type from [" + core.db.getRecordName("content Field Types", cdefFieldKvp.Value.fieldTypeId) + "] to [" + core.db.getRecordName("content Field Types", formFieldTypeId) + "]. This may have caused a problem converting content.</LI>";
                                                        int DataSourceTypeID = core.db.getDataSourceType(dataSourceName);
                                                        switch (DataSourceTypeID) {
                                                            case DataSourceTypeODBCMySQL:
                                                                SQL = "alter table " + CDef.tableName + " change " + cdefFieldKvp.Value.nameLc + " " + cdefFieldKvp.Value.nameLc + " " + core.db.getSQLAlterColumnType(dataSourceName, formFieldTypeId) + ";";
                                                                break;
                                                            default:
                                                                SQL = "alter table " + CDef.tableName + " alter column " + cdefFieldKvp.Value.nameLc + " " + core.db.getSQLAlterColumnType(dataSourceName, formFieldTypeId) + ";";
                                                                break;
                                                        }
                                                        core.db.executeQuery(SQL, dataSourceName);
                                                    }
                                                    SQL = "Update ccFields"
                                                + " Set name=" + DbController.encodeSQLText(formFieldName) + ",type=" + formFieldTypeId + ",caption=" + DbController.encodeSQLText(cp.Doc.GetText("dtfaCaption." + RecordPointer)) + ",DefaultValue=" + DbController.encodeSQLText(cp.Doc.GetText("dtfaDefaultValue." + RecordPointer)) + ",EditSortPriority=" + DbController.encodeSQLText(GenericController.encodeText(cp.Doc.GetInteger("dtfaEditSortPriority." + RecordPointer))) + ",Active=" + DbController.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaActive." + RecordPointer)) + ",ReadOnly=" + DbController.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaReadOnly." + RecordPointer)) + ",Authorable=" + DbController.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaAuthorable." + RecordPointer)) + ",Required=" + DbController.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaRequired." + RecordPointer)) + ",UniqueName=" + DbController.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaUniqueName." + RecordPointer)) + ",TextBuffered=" + DbController.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaTextBuffered." + RecordPointer)) + ",Password=" + DbController.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaPassword." + RecordPointer)) + ",HTMLContent=" + DbController.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaHTMLContent." + RecordPointer)) + ",EditTab=" + DbController.encodeSQLText(cp.Doc.GetText("dtfaEditTab." + RecordPointer)) + ",Scramble=" + DbController.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaScramble." + RecordPointer)) + "";
                                                    if (core.session.isAuthenticatedAdmin(core)) {
                                                        SQL += ",adminonly=" + DbController.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaAdminOnly." + RecordPointer));
                                                    }
                                                    if (core.session.isAuthenticatedDeveloper(core)) {
                                                        SQL += ",DeveloperOnly=" + DbController.encodeSQLBoolean(cp.Doc.GetBoolean("dtfaDeveloperOnly." + RecordPointer));
                                                    }
                                                    SQL += " where ID=" + formFieldId;
                                                    core.db.executeQuery(SQL);
                                                    ReloadCDef = true;
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            core.cache.invalidateAll();
                            core.clearMetaData();
                        }
                        if (ToolButton == ButtonAdd) {
                            //
                            // ----- Insert a blank Field
                            //
                            CSPointer = core.db.csInsertRecord("Content Fields");
                            if (core.db.csOk(CSPointer)) {
                                core.db.csSet(CSPointer, "name", "unnamedField" + core.db.csGetInteger(CSPointer, "id").ToString());
                                core.db.csSet(CSPointer, "ContentID", ContentID);
                                core.db.csSet(CSPointer, "EditSortPriority", 0);
                                ReloadCDef = true;
                            }
                            core.db.csClose(ref CSPointer);
                        }
                        //
                        // ----- Button Reload CDef
                        //
                        if (ToolButton == ButtonSaveandInvalidateCache) {
                            core.cache.invalidateAll();
                            core.clearMetaData();
                        }
                        //
                        // ----- Restore Content Autoload site property
                        //
                        if (AllowContentAutoLoad) {
                            cp.Site.SetProperty("AllowContentAutoLoad", AllowContentAutoLoad.ToString());
                        }
                        //
                        // ----- Cancel or Save, reload CDef and go
                        //
                        if ((ToolButton == ButtonCancel) || (ToolButton == ButtonOK)) {
                            //
                            // ----- Exit back to menu
                            //
                            return core.webServer.redirect(core.webServer.requestProtocol + core.webServer.requestDomain + core.webServer.requestPath + core.webServer.requestPage + "?af=" + AdminFormTools);
                        }
                    }
                }
                //
                //   Print Output
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                Stream.Add(AdminUIController.getToolFormTitle("Manage Admin Edit Fields", "Use this tool to add or modify content definition fields. Contensive uses a caching system for content definitions that is not automatically reloaded. Change you make will not take effect until the next time the system is reloaded. When you create a new field, the database field is created automatically when you have saved both a name and a field type. If you change the field type, you may have to manually change the database field."));
                //
                // -- status of last operation
                if (!string.IsNullOrEmpty(StatusMessage)) {
                    Stream.Add(AdminUIController.getToolFormRow(core, "<UL>" + StatusMessage + "</UL>"));
                }
                //
                // -- errors with last operations
                if (!string.IsNullOrEmpty(ErrorMessage)) {
                    Stream.Add( HtmlController.div( "There was a problem saving these changes" + "<UL>" + ErrorMessage + "</UL>","ccError"));
                }
                if (ReloadCDef) {
                    CDef = Processor.Models.Domain.CDefDomainModel.create(core, ContentID, true, true);
                }
                if (ContentID == 0) {
                    //
                    // content tables that have edit forms to Configure
                    bool isEmptyList = false;
                    Stream.Add(AdminUIController.getToolFormInputRow(core, "Select a Content Definition to Configure", AdminUIController.getDefaultEditor_LookupContent(core,RequestNameToolContentID, ContentID, CdefController.getContentId( core,"Content" ), ref isEmptyList)));
                } else {
                    //
                    // Configure edit form
                    Stream.Add(HtmlController.inputHidden(RequestNameToolContentID, ContentID));
Stream.Add(core.html.getPanelTop());
                    ContentName = CdefController.getContentNameByID(core, ContentID);
                    ButtonList = ButtonCancel + "," + ButtonSave + "," + ButtonOK + "," + ButtonAdd;
                    //
                    // Get a new copy of the content definition
                    //
                    Stream.Add(SpanClassAdminNormal + "<P><B>" + ContentName + "</b></P>");
                    Stream.Add("<table border=\"0\" cellpadding=\"1\" cellspacing=\"1\" width=\"100%\">");
                    //
                    int ParentContentID = CDef.parentID;
                    bool AllowCDefInherit = false;
                    Processor.Models.Domain.CDefDomainModel ParentCDef = null;
                    if (ParentContentID == -1) {
                        AllowCDefInherit = false;
                    } else {
                        AllowCDefInherit = true;
                        string ParentContentName = CdefController.getContentNameByID(core, ParentContentID);
                        ParentCDef = Processor.Models.Domain.CDefDomainModel.create(core, ParentContentID, true, true);
                    }
                    bool NeedFootNote1 = false;
                    bool NeedFootNote2 = false;
                    if (CDef.fields.Count > 0) {
                        //
                        // -- header row
                        Stream.Add("<tr>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\"></td>");
                        if (!AllowCDefInherit) {
                            Stream.Add("<td valign=\"bottom\" width=\"100\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b><br>Inherited*</b></span></td>");
                            NeedFootNote1 = true;
                        } else {
                            Stream.Add("<td valign=\"bottom\" width=\"100\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b><br>Inherited</b></span></td>");
                        }
                        Stream.Add("<td valign=\"bottom\" width=\"100\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b><br>Field</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"100\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b><br>Caption</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"100\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b><br>Edit Tab</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"100\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b><br>Default</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b><br>Type</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b>Edit<br>Order</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b><br>Active</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b>Read<br>Only</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b><br>Auth</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b><br>Req</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b><br>Unique</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b>Text<br>Buffer</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b><br>Pass</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "<b>Text<br>Scrm</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b><br>HTML</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b>Admin<br>Only</b></span></td>");
                        Stream.Add("<td valign=\"bottom\" width=\"50\" class=\"ccPanelInput\" align=\"left\">" + SpanClassAdminSmall + "<b>Dev<br>Only</b></span></td>");
                        Stream.Add("</tr>");
                        RecordCount = 0;
                        //
                        // Build a select template for Type
                        //
                        string TypeSelectTemplate = core.html.selectFromContent("menuname", -1, "Content Field Types", "", "unknown");
                        //
                        // Index the sort order
                        //
                        List<fieldSortClass> fieldList = new List<fieldSortClass>();
                        int FieldCount = CDef.fields.Count;
                        foreach (var keyValuePair in CDef.fields) {
                            fieldSortClass fieldSort = new fieldSortClass();
                            //Dim field As New appServices_metaDataClass.CDefFieldClass
                            string sortOrder = "";
                            fieldSort.field = keyValuePair.Value;
                            sortOrder = "";
                            if (fieldSort.field.active) {
                                sortOrder += "0";
                            } else {
                                sortOrder += "1";
                            }
                            if (fieldSort.field.authorable) {
                                sortOrder += "0";
                            } else {
                                sortOrder += "1";
                            }
                            sortOrder += fieldSort.field.editTabName + getIntegerString(fieldSort.field.editSortPriority, 10) + getIntegerString(fieldSort.field.id, 10);
                            fieldSort.sort = sortOrder;
                            fieldList.Add(fieldSort);
                        }
                        fieldList.Sort((p1, p2) => p1.sort.CompareTo(p2.sort));
                        StringBuilderLegacyController StreamValidRows = new StringBuilderLegacyController();
                        var contentFieldsCdef = Processor.Models.Domain.CDefDomainModel.create(core, "content fields");
                        foreach (fieldSortClass fieldsort in fieldList) {
                            StringBuilderLegacyController streamRow = new StringBuilderLegacyController();
                            bool rowValid = true;
                            //
                            // If Field has name and type, it is locked and can not be changed
                            //
                            bool FieldLocked = (fieldsort.field.nameLc != "") && (fieldsort.field.fieldTypeId != 0);
                            //
                            // put the menu into the current menu format
                            //
                            formFieldId = fieldsort.field.id;
                            streamRow.Add(HtmlController.inputHidden("dtfaID." + RecordCount, formFieldId));
                            streamRow.Add("<tr>");
                            //
                            // edit button
                            //
                            streamRow.Add("<td class=\"ccPanelInput\" align=\"left\">" + AdminUIController.getIconEditAdminLink(core, contentFieldsCdef, formFieldId) + "</td>");
                            //
                            // Inherited
                            //
                            if (!AllowCDefInherit) {
                                //
                                // no parent
                                //
                                streamRow.Add("<td class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "False</span></td>");
                            } else if (fieldsort.field.inherited) {
                                //
                                // inherited property
                                //
                                streamRow.Add("<td class=\"ccPanelInput\" align=\"center\">" + HtmlController.checkbox("dtfaInherited." + RecordCount, fieldsort.field.inherited) + "</td>");
                            } else {
                                Processor.Models.Domain.CDefFieldModel parentField = null;
                                //
                                // CDef has a parent, but the field is non-inherited, test for a matching Parent Field
                                //
                                if (ParentCDef == null) {
                                    foreach (KeyValuePair<string, Processor.Models.Domain.CDefFieldModel> kvp in ParentCDef.fields) {
                                        if (kvp.Value.nameLc == fieldsort.field.nameLc) {
                                            parentField = kvp.Value;
                                            break;
                                        }
                                    }
                                }
                                if (parentField == null) {
                                    streamRow.Add("<td class=\"ccPanelInput\" align=\"center\">" + SpanClassAdminSmall + "False**</span></td>");
                                    NeedFootNote2 = true;
                                } else {
                                    streamRow.Add("<td class=\"ccPanelInput\" align=\"center\">" + HtmlController.checkbox("dtfaInherited." + RecordCount, fieldsort.field.inherited) + "</td>");
                                }
                            }
                            //
                            // name
                            //
                            bool tmpValue = string.IsNullOrEmpty(fieldsort.field.nameLc);
                            rowValid = rowValid && !tmpValue;
                            streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
                            if (fieldsort.field.inherited) {
                                streamRow.Add(SpanClassAdminSmall + fieldsort.field.nameLc + "&nbsp;</SPAN>");
                            } else if (FieldLocked) {
                                streamRow.Add(SpanClassAdminSmall + fieldsort.field.nameLc + "&nbsp;</SPAN><input type=hidden name=dtfaName." + RecordCount + " value=\"" + fieldsort.field.nameLc + "\">");
                            } else {
                                streamRow.Add(HtmlController.inputText(core, "dtfaName." + RecordCount, fieldsort.field.nameLc, 1, 10));
                            }
                            streamRow.Add("</nobr></td>");
                            //
                            // caption
                            //
                            streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
                            if (fieldsort.field.inherited) {
                                streamRow.Add(SpanClassAdminSmall + fieldsort.field.caption + "</SPAN>");
                            } else {
                                streamRow.Add(HtmlController.inputText(core, "dtfaCaption." + RecordCount, fieldsort.field.caption, 1, 10));
                            }
                            streamRow.Add("</nobr></td>");
                            //
                            // Edit Tab
                            //
                            streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
                            if (fieldsort.field.inherited) {
                                streamRow.Add(SpanClassAdminSmall + fieldsort.field.editTabName + "</SPAN>");
                            } else {
                                streamRow.Add(HtmlController.inputText(core, "dtfaEditTab." + RecordCount, fieldsort.field.editTabName, 1, 10));
                            }
                            streamRow.Add("</nobr></td>");
                            //
                            // default
                            //
                            streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
                            if (fieldsort.field.inherited) {
                                streamRow.Add(SpanClassAdminSmall + GenericController.encodeText(fieldsort.field.defaultValue) + "</SPAN>");
                            } else {
                                streamRow.Add(HtmlController.inputText(core, "dtfaDefaultValue." + RecordCount, GenericController.encodeText(fieldsort.field.defaultValue), 1, 10));
                            }
                            streamRow.Add("</nobr></td>");
                            //
                            // type
                            //
                            rowValid = rowValid && (fieldsort.field.fieldTypeId > 0);
                            streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
                            if (fieldsort.field.inherited) {
                                CSPointer = core.db.csOpenRecord("Content Field Types", fieldsort.field.fieldTypeId);
                                if (!core.db.csOk(CSPointer)) {
                                    streamRow.Add(SpanClassAdminSmall + "Unknown[" + fieldsort.field.fieldTypeId + "]</SPAN>");
                                } else {
                                    streamRow.Add(SpanClassAdminSmall + core.db.csGetText(CSPointer, "Name") + "</SPAN>");
                                }
                                core.db.csClose(ref CSPointer);
                            } else if (FieldLocked) {
                                streamRow.Add(core.db.getRecordName("content field types", fieldsort.field.fieldTypeId) + HtmlController.inputHidden("dtfaType." + RecordCount, fieldsort.field.fieldTypeId));
                            } else {
                                string TypeSelect = TypeSelectTemplate;
                                TypeSelect = GenericController.vbReplace(TypeSelect, "menuname", "dtfaType." + RecordCount, 1, 99, 1);
                                TypeSelect = GenericController.vbReplace(TypeSelect, "=\"" + fieldsort.field.fieldTypeId + "\"", "=\"" + fieldsort.field.fieldTypeId + "\" selected", 1, 99, 1);
                                streamRow.Add(TypeSelect);
                            }
                            streamRow.Add("</nobr></td>");
                            //
                            // sort priority
                            //
                            streamRow.Add("<td class=\"ccPanelInput\" align=\"left\"><nobr>");
                            if (fieldsort.field.inherited) {
                                streamRow.Add(SpanClassAdminSmall + fieldsort.field.editSortPriority + "</SPAN>");
                            } else {
                                streamRow.Add(HtmlController.inputText(core, "dtfaEditSortPriority." + RecordCount, fieldsort.field.editSortPriority.ToString(), 1, 10));
                            }
                            streamRow.Add("</nobr></td>");
                            //
                            // active
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaActive." + RecordCount, fieldsort.field.active, fieldsort.field.inherited));
                            //
                            // read only
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaReadOnly." + RecordCount, fieldsort.field.readOnly, fieldsort.field.inherited));
                            //
                            // authorable
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaAuthorable." + RecordCount, fieldsort.field.authorable, fieldsort.field.inherited));
                            //
                            // required
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaRequired." + RecordCount, fieldsort.field.required, fieldsort.field.inherited));
                            //
                            // UniqueName
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaUniqueName." + RecordCount, fieldsort.field.uniqueName, fieldsort.field.inherited));
                            //
                            // text buffered
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaTextBuffered." + RecordCount, fieldsort.field.textBuffered, fieldsort.field.inherited));
                            //
                            // password
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaPassword." + RecordCount, fieldsort.field.password, fieldsort.field.inherited));
                            //
                            // scramble
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaScramble." + RecordCount, fieldsort.field.Scramble, fieldsort.field.inherited));
                            //
                            // HTML Content
                            //
                            streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaHTMLContent." + RecordCount, fieldsort.field.htmlContent, fieldsort.field.inherited));
                            //
                            // Admin Only
                            //
                            if (core.session.isAuthenticatedAdmin(core)) {
                                streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaAdminOnly." + RecordCount, fieldsort.field.adminOnly, fieldsort.field.inherited));
                            }
                            //
                            // Developer Only
                            //
                            if (core.session.isAuthenticatedDeveloper(core)) {
                                streamRow.Add(GetForm_ConfigureEdit_CheckBox("dtfaDeveloperOnly." + RecordCount, fieldsort.field.developerOnly, fieldsort.field.inherited));
                            }
                            //
                            streamRow.Add("</tr>");
                            RecordCount = RecordCount + 1;
                            //
                            // rows are built - put the blank rows at the top
                            //
                            if (!rowValid) {
                                Stream.Add(streamRow.Text);
                            } else {
                                StreamValidRows.Add(streamRow.Text);
                            }
                        }
                        Stream.Add(StreamValidRows.Text);
                        Stream.Add(HtmlController.inputHidden("dtfaRecordCount", RecordCount));
                    }
                    Stream.Add("</table>");
                    //Stream.Add( core.htmldoc.main_GetPanelButtons(ButtonList, "Button"))
                    //
                    Stream.Add(core.html.getPanelBottom());
                    //Call Stream.Add(core.main_GetFormEnd())
                    if (NeedFootNote1) {
                        Stream.Add("<br>*Field Inheritance is not allowed because this Content Definition has no parent.");
                    }
                    if (NeedFootNote2) {
                        Stream.Add("<br>**This field can not be inherited because the Parent Content Definition does not have a field with the same name.");
                    }
                }
                Stream.Add(HtmlController.inputHidden("ReloadCDef", ReloadCDef));
                //
                // -- assemble form
                result = AdminUIController.getToolForm(core, Stream.Text, ButtonList);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        //
        //
        public static string GetForm_ConfigureEdit_CheckBox(string htmlName, bool selected, bool Inherited) {
            string result = "<td class=\"ccPanelInput\" align=\"center\"><nobr>";
            if (Inherited) {
                result += SpanClassAdminSmall + getYesNo(selected) + "</SPAN>";
            } else {
                result += HtmlController.checkbox(htmlName, selected);
            }
            return result + "</nobr></td>";
        }
        //
        private class fieldSortClass {
            public string sort;
            public Processor.Models.Domain.CDefFieldModel field;
        }
    }
}

