
using System;
using System.Linq;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
using Contensive.Addons.AdminSite.Controllers;
using Contensive.BaseClasses;

namespace Contensive.Addons.AdminSite {
    public class ToolSetListColumnsClass {
        //
        //=============================================================================
        //   Print the Configure Index Form
        //=============================================================================
        //
        public static string GetForm_Index_SetColumns(CPClass cp, CoreController core, AdminDataModel adminData) {
            string result = "";
            try {
                // todo refactor out
                ContentMetadataModel adminContent = adminData.adminContent;
                string Button = core.docProperties.getText(RequestNameButton);
                if (Button == ButtonOK) {
                    //
                    //   Process OK
                    return result;
                }
                //
                //   Load Request
                if (Button == ButtonReset) {
                    //
                    //   Process reset
                    core.userProperty.setProperty(AdminDataModel.IndexConfigPrefix + adminContent.id.ToString(), "");
                }
                IndexConfigClass IndexConfig = IndexConfigClass.get(core, adminData);
                int ToolsAction = core.docProperties.getInteger("dta");
                int TargetFieldID = core.docProperties.getInteger("fi");
                string TargetFieldName = core.docProperties.getText("FieldName");
                int ColumnPointer = core.docProperties.getInteger("dtcn");
                const string RequestNameAddField = "addfield";
                string FieldNameToAdd = GenericController.vbUCase(core.docProperties.getText(RequestNameAddField));
                const string RequestNameAddFieldID = "addfieldID";
                int FieldIDToAdd = core.docProperties.getInteger(RequestNameAddFieldID);
                bool normalizeSaveLoad = core.docProperties.getBoolean("NeedToReloadConfig");
                bool AllowContentAutoLoad = false;
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                string Title = "Set Columns: " + adminContent.name;
                string Description = "Use the icons to add, remove and modify your personal column prefernces for this content (" + adminContent.name + "). Hit OK when complete. Hit Reset to restore your column preferences for this content to the site's default column preferences.";
                Stream.Add(AdminUIController.getToolFormTitle(Title, Description));
                //
                //--------------------------------------------------------------------------------
                // Process actions
                //--------------------------------------------------------------------------------
                //
                if (adminContent.id != 0) {
                    var CDef = ContentMetadataModel.create(core, adminContent.id);
                    int ColumnWidthTotal = 0;
                    if (ToolsAction != 0) {
                        //
                        // Block contentautoload, then force a load at the end
                        //
                        AllowContentAutoLoad = (core.siteProperties.getBoolean("AllowContentAutoLoad", true));
                        core.siteProperties.setProperty("AllowContentAutoLoad", false);
                        bool reloadMetadata = false;
                        int SourceContentID = 0;
                        string SourceName = null;
                        //
                        // Make sure the FieldNameToAdd is not-inherited, if not, create new field
                        //
                        if (FieldIDToAdd != 0) {
                            foreach (KeyValuePair<string, ContentFieldMetadataModel> keyValuePair in adminContent.fields) {
                                ContentFieldMetadataModel field = keyValuePair.Value;
                                if (field.id == FieldIDToAdd) {
                                    if (field.inherited) {
                                        SourceContentID = field.contentId;
                                        SourceName = field.nameLc;
                                        //
                                        // -- copy the field
                                        using (var CSSource = new CsModel(core)) {
                                            if (CSSource.open("Content Fields", "(ContentID=" + SourceContentID + ")and(Name=" + DbController.encodeSQLText(SourceName) + ")")) {
                                                using (var CSTarget = new CsModel(core)) {
                                                    if (CSTarget.insert("Content Fields")) {
                                                        CSSource.copyRecord(CSTarget);
                                                        CSTarget.set("ContentID", adminContent.id);
                                                        reloadMetadata = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        //
                        // Make sure all fields are not-inherited, if not, create new fields
                        //
                        foreach (var column in IndexConfig.columns) {
                            ContentFieldMetadataModel field = adminContent.fields[column.Name.ToLowerInvariant()];
                            if (field.inherited) {
                                SourceContentID = field.contentId;
                                SourceName = field.nameLc;
                                using (var CSSource = new CsModel(core)) {
                                    if (CSSource.open("Content Fields", "(ContentID=" + SourceContentID + ")and(Name=" + DbController.encodeSQLText(SourceName) + ")")) {
                                        using (var CSTarget = new CsModel(core)) {
                                            if (CSTarget.insert("Content Fields")) {
                                                CSSource.copyRecord(CSTarget);
                                                CSTarget.set("ContentID", adminContent.id);
                                                reloadMetadata = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //
                        // get current values for Processing
                        //
                        foreach (var column in IndexConfig.columns) {
                            ColumnWidthTotal += column.Width;
                        }
                        //
                        // ----- Perform any actions first
                        //
                        switch (ToolsAction) {
                            case ToolsActionAddField: {
                                    //
                                    // Add a field to the index form
                                    //
                                    if (FieldIDToAdd != 0) {
                                        IndexConfigClass.IndexConfigColumnClass column = null;
                                        foreach (var columnx in IndexConfig.columns) {
                                            columnx.Width = encodeInteger((columnx.Width * 80) / (double)ColumnWidthTotal);
                                        }
                                        {
                                            column = new IndexConfigClass.IndexConfigColumnClass();
                                            using (var csData = new CsModel(core)) {
                                                if (csData.openRecord("Content Fields", FieldIDToAdd)) {
                                                    column.Name = csData.getText("name");
                                                    column.Width = 20;
                                                }
                                            }
                                            IndexConfig.columns.Add(column);
                                            normalizeSaveLoad = true;
                                        }
                                    }
                                    //
                                    break;
                                }
                            case ToolsActionRemoveField: {
                                    //
                                    // Remove a field to the index form
                                    int columnWidthTotal = 0;
                                    var dstColumns = new List<IndexConfigClass.IndexConfigColumnClass>();
                                    foreach (var column in IndexConfig.columns) {
                                        if (column.Name != TargetFieldName.ToLowerInvariant()) {
                                            dstColumns.Add(column);
                                            columnWidthTotal += column.Width;
                                        }
                                    }
                                    IndexConfig.columns = dstColumns;
                                    normalizeSaveLoad = true;
                                    break;
                                }
                            case ToolsActionMoveFieldLeft: {
                                    if(IndexConfig.columns.First().Name != TargetFieldName.ToLowerInvariant()) {
                                        int listIndex = 0;
                                        foreach (var column in IndexConfig.columns) {
                                            if (column.Name == TargetFieldName.ToLowerInvariant()) {
                                                break;
                                            }
                                            listIndex += 1;
                                        }
                                        IndexConfig.columns.Swap(listIndex, listIndex - 1);
                                        normalizeSaveLoad = true;
                                    }
                                    break;
                                }
                            case ToolsActionMoveFieldRight: {
                                    if (IndexConfig.columns.Last().Name != TargetFieldName.ToLowerInvariant()) {
                                        int listIndex = 0;
                                        foreach (var column in IndexConfig.columns) {
                                            if (column.Name == TargetFieldName.ToLowerInvariant()) {
                                                break;
                                            }
                                            listIndex += 1;
                                        }
                                        IndexConfig.columns.Swap(listIndex, listIndex + 1);
                                        normalizeSaveLoad = true;
                                    }
                                    break;
                                }
                            case ToolsActionExpand: {
                                    foreach (var column in IndexConfig.columns) {
                                        if (column.Name == TargetFieldName.ToLowerInvariant()) {
                                            column.Width = Convert.ToInt32(Convert.ToDouble(column.Width) * 1.1);
                                        } else {
                                            column.Width = Convert.ToInt32(Convert.ToDouble(column.Width) * 0.9);
                                        }
                                    }
                                    normalizeSaveLoad = true;
                                    break;
                                }
                            case ToolsActionContract: {
                                    foreach (var column in IndexConfig.columns) {
                                        if (column.Name != TargetFieldName.ToLowerInvariant()) {
                                            column.Width = Convert.ToInt32(Convert.ToDouble(column.Width) * 1.1);
                                        } else {
                                            column.Width = Convert.ToInt32(Convert.ToDouble(column.Width) * 0.9);
                                        }
                                    }
                                    normalizeSaveLoad = true;
                                    break;
                                }
                        }
                        //
                        // Reload CDef if it changed
                        //
                        if (reloadMetadata) {
                            core.clearMetaData();
                            core.cache.invalidateAll();
                            CDef = ContentMetadataModel.createByUniqueName(core, adminContent.name);
                        }
                        //
                        // save indexconfig
                        //
                        if (normalizeSaveLoad) {
                            //
                            // Normalize the widths of the remaining columns
                            ColumnWidthTotal = 0;
                            foreach (var column in IndexConfig.columns) {
                                ColumnWidthTotal += column.Width;
                            }
                            foreach (var column in IndexConfig.columns) {
                                column.Width = encodeInteger((1000 * column.Width) / (double)ColumnWidthTotal);
                            }
                            GetHtmlBodyClass.setIndexSQL_SaveIndexConfig(cp, core, IndexConfig);
                            IndexConfig = IndexConfigClass.get(core, adminData);
                        }
                    }
                    //
                    //--------------------------------------------------------------------------------
                    //   Display the form
                    //--------------------------------------------------------------------------------
                    //
                    Stream.Add("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"99%\"><tr>");
                    Stream.Add("<td width=\"5%\">&nbsp;</td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>10%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>20%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>30%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>40%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>50%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>60%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>70%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>80%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>90%</nobr></td>");
                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>100%</nobr></td>");
                    Stream.Add("<td width=\"4%\" align=\"center\">&nbsp;</td>");
                    Stream.Add("</tr></table>");
                    //
                    Stream.Add("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"99%\"><tr>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ContensiveBase/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ContensiveBase/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ContensiveBase/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ContensiveBase/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ContensiveBase/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ContensiveBase/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ContensiveBase/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ContensiveBase/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ContensiveBase/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ContensiveBase/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ContensiveBase/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ContensiveBase/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("</tr></table>");
                    //
                    // print the column headers
                    //
                    ColumnWidthTotal = 0;
                    int InheritedFieldCount = 0;
                    if (IndexConfig.columns.Count > 0) {
                        //
                        // Calc total width
                        //
                        foreach (var column in IndexConfig.columns) {
                            ColumnWidthTotal += column.Width;
                        }
                        if (ColumnWidthTotal > 0) {
                            Stream.Add("<table border=\"0\" cellpadding=\"5\" cellspacing=\"0\" width=\"90%\">");
                            //
                            // -- header
                            Stream.Add("<tr>");
                            int ColumnWidth = 0;
                            int fieldId = 0;
                            string Caption = null;
                            foreach (var column in IndexConfig.columns) {
                                //
                                // print column headers - anchored so they sort columns
                                //
                                ColumnWidth = encodeInteger(100 * (column.Width / (double)ColumnWidthTotal));
                                ContentFieldMetadataModel field = adminContent.fields[column.Name.ToLowerInvariant()];
                                fieldId = field.id;
                                Caption = field.caption;
                                if (field.inherited) {
                                    Caption = Caption + "*";
                                    InheritedFieldCount = InheritedFieldCount + 1;
                                }
                                Stream.Add("<td class=\"small\" width=\"" + ColumnWidth + "%\" valign=\"top\" align=\"left\" style=\"background-color:white;border: 1px solid #555;\">" + Caption + "</td>");
                            }
                            Stream.Add("</tr>");
                            //
                            // -- body
                            Stream.Add("<tr>");
                            foreach (var column in IndexConfig.columns) {
                                //
                                // print column headers - anchored so they sort columns
                                //
                                ColumnWidth = encodeInteger(100 * (column.Width / (double)ColumnWidthTotal));
                                ContentFieldMetadataModel field = adminContent.fields[column.Name.ToLowerInvariant()];
                                fieldId = field.id;
                                Caption = field.caption;
                                if (field.inherited) {
                                    Caption = Caption + "*";
                                    InheritedFieldCount = InheritedFieldCount + 1;
                                }
                                //adminUIController Adminui = new adminUIController(core);
                                int ColumnPtr = 0;
                                string link = "?" + core.doc.refreshQueryString + "&FieldName=" + HtmlController.encodeHtml(field.nameLc) + "&fi=" + fieldId + "&dtcn=" + ColumnPtr + "&" + RequestNameAdminSubForm + "=" + AdminFormIndex_SubFormSetColumns; 
                                //string AStart = "<a href=\"?" + core.doc.refreshQueryString + "&FieldName=" + htmlController.encodeHtml(field.nameLc) + "&fi=" + fieldId + "&dtcn=" + ColumnPtr + "&" + RequestNameAdminSubForm + "=" + AdminFormIndex_SubFormSetColumns;
                                Stream.Add("<td width=\"" + ColumnWidth + "%\" valign=\"top\" align=\"left\">");
                                //Stream.Add("<img src=\"/ContensiveBase/images/black.GIF\" width=\"100%\" height=\"1\" >");
                                Stream.Add(HtmlController.div(AdminUIController.getIconDeleteLink(link + "&dta=" + ToolsActionRemoveField),"text-center"));
                                Stream.Add(HtmlController.div(AdminUIController.getIconArrowRightLink(link + "&dta=" + ToolsActionMoveFieldRight), "text-center"));
                                Stream.Add(HtmlController.div(AdminUIController.getIconArrowLeftLink(link + "&dta=" + ToolsActionMoveFieldLeft), "text-center"));
                                Stream.Add(HtmlController.div(AdminUIController.getIconExpandLink(link + "&dta=" + ToolsActionExpand), "text-center"));
                                Stream.Add(HtmlController.div(AdminUIController.getIconContractLink(link + "&dta=" + ToolsActionContract), "text-center"));
                                Stream.Add("</td>");
                            }
                            Stream.Add("</tr>");
                            //
                            Stream.Add("</table>");
                        }
                    }
                    //
                    // ----- If anything was inherited, put up the message
                    //
                    if (InheritedFieldCount > 0) {
                        Stream.Add("<p class=\"ccNormal\">* This field was inherited from the Content Definition's Parent. Inherited fields will automatically change when the field in the parent is changed. If you alter these settings, this connection will be broken, and the field will no longer inherit it's properties.</P class=\"ccNormal\">");
                    }
                    //
                    // ----- now output a list of fields to add
                    //
                    if (CDef.fields.Count == 0) {
                        Stream.Add(SpanClassAdminNormal + "This Content Definition has no fields</span><br>");
                    } else {
                        foreach (KeyValuePair<string, ContentFieldMetadataModel> keyValuePair in adminContent.fields) {
                            ContentFieldMetadataModel field = keyValuePair.Value;
                            //
                            // display the column if it is not in use
                            if ((IndexConfig.columns.Find(x => x.Name == field.nameLc) == null)) {
                                if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.File) {
                                    //
                                    // file can not be search
                                    Stream.Add(HtmlController.div(iconNotAvailable + "&nbsp;" + field.caption + " (file field)"));
                                } else if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.FileText) {
                                    //
                                    // filename can not be search
                                    Stream.Add(HtmlController.div(iconNotAvailable + "&nbsp;" + field.caption + " (text file field)"));
                                } else if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.FileHTML) {
                                    //
                                    // filename can not be search
                                    Stream.Add(HtmlController.div(iconNotAvailable + "&nbsp;" + field.caption + " (html file field)"));
                                } else if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.FileCSS) {
                                    //
                                    // css filename can not be search
                                    Stream.Add(HtmlController.div(iconNotAvailable + "&nbsp;" + field.caption + " (css file field)"));
                                } else if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.FileXML) {
                                    //
                                    // xml filename can not be search
                                    Stream.Add(HtmlController.div(iconNotAvailable + "&nbsp;" + field.caption + " (xml file field)"));
                                } else if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.FileJavascript) {
                                    //
                                    // javascript filename can not be search
                                    Stream.Add(HtmlController.div(iconNotAvailable + "&nbsp;" + field.caption + " (javascript file field)"));
                                } else if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.LongText) {
                                    //
                                    // long text can not be search
                                    Stream.Add(HtmlController.div(iconNotAvailable + "&nbsp;" + field.caption + " (long text field)"));
                                } else if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.HTML) {
                                    //
                                    // long text can not be search
                                    Stream.Add(HtmlController.div(iconNotAvailable + "&nbsp;" + field.caption + " (long text field)"));
                                } else if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.FileImage) {
                                    //
                                    // long text can not be search
                                    Stream.Add(HtmlController.div(iconNotAvailable + "&nbsp;" + field.caption + " (image field)"));
                                } else if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.Redirect) {
                                    //
                                    // long text can not be search
                                    Stream.Add(HtmlController.div(iconNotAvailable + "&nbsp;" + field.caption + " (redirect field)"));
                                } else if (field.fieldTypeId == CPContentBaseClass.fileTypeIdEnum.ManyToMany) {
                                    //
                                    // many to many can not be search
                                    Stream.Add(HtmlController.div( iconNotAvailable + "&nbsp;" + field.caption + " (many-to-many field)"));
                                } else {
                                    //
                                    // can be used as column header
                                    string link = "?" + core.doc.refreshQueryString + "&fi=" + field.id + "&dta=" + ToolsActionAddField + "&" + RequestNameAddFieldID + "=" + field.id + "&" + RequestNameAdminSubForm + "=" + AdminFormIndex_SubFormSetColumns;
                                    Stream.Add(HtmlController.div(AdminUIController.getIconPlusLink(link, "&nbsp;" + field.caption)));
                                }
                            }
                        }
                    }
                }
                //
                //--------------------------------------------------------------------------------
                // print the content tables that have index forms to Configure
                //--------------------------------------------------------------------------------
                //
                //FormPanel = FormPanel & SpanClassAdminNormal & "Select a Content Definition to Configure its index form<br>"
                //FormPanel = FormPanel & core.main_GetFormInputHidden("af", AdminFormToolConfigureIndex)
                //FormPanel = FormPanel & core.htmldoc.main_GetFormInputSelect2("ContentID", ContentID, "Content")
                //Call Stream.Add(core.htmldoc.main_GetPanel(FormPanel))
                //
                core.siteProperties.setProperty("AllowContentAutoLoad", GenericController.encodeText(AllowContentAutoLoad));
                //Stream.Add( core.main_GetFormInputHidden("NeedToReloadConfig", NeedToReloadConfig))
                string Content = ""
                    + Stream.Text
                    + HtmlController.inputHidden("cid", adminContent.id.ToString())
                    + HtmlController.inputHidden(rnAdminForm, "1")
                    + HtmlController.inputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormSetColumns)
                    + "";
                //
                // -- assemble form

                result = AdminUIController.getToolForm(core, Content, ButtonOK + "," + ButtonReset);
                //result = adminUIController.getBody(core, Title, ButtonOK + "," + ButtonReset, "", false, false, Description, "", 10, Content);
                //
                //
                //    ButtonBar = adminUIController.GetButtonsFromList( ButtonList, True, True, "button")
                //    ButtonBar = adminUIController.GetButtonBar(ButtonBar, "")
                //    Stream = New FastStringClass
                //
                //    GetForm_Index_SetColumns = "" _
                //        & ButtonBar _
                //        & adminUIController.EditTableOpen _
                //        & Stream.Text _
                //        & adminUIController.EditTableClose _
                //        & ButtonBar _
                //    '
                //    '
                //    ' Assemble LiveWindowTable
                //    '
                //    Stream.Add( OpenLiveWindowTable)
                //    Stream.Add( vbCrLf & core.main_GetFormStart()
                //    Stream.Add( ButtonBar)
                //    Stream.Add( TitleBar)
                //    Stream.Add( Content)
                //    Stream.Add( ButtonBar)
                //    Stream.Add( "<input type=hidden name=asf VALUE=" & AdminFormIndex_SubFormSetColumns & ">")
                //    Stream.Add( "</form>")
                //    Stream.Add( CloseLiveWindowTable)
                //    '
                //    GetForm_Index_SetColumns = Stream.Text
                core.html.addTitle(Title);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
    }
}
