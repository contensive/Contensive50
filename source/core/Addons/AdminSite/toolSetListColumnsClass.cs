
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
using Contensive.Core.Models.Complex;
using Contensive.Core.Addons.Tools;
using static Contensive.Core.adminUIController;
//
namespace Contensive.Core.Addons.AdminSite {
    public class toolSetListColumnsClass{
        //
        //=============================================================================
        //   Print the Configure Index Form
        //=============================================================================
        //
        public static string GetForm_Index_SetColumns(coreController core, adminContextClass adminContext) {
            string result = "";
            try {
                // todo refactor out
                cdefModel adminContent = adminContext.adminContent;
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
                    core.userProperty.setProperty(adminContextClass.IndexConfigPrefix + adminContent.id.ToString(), "");
                }
                indexConfigClass IndexConfig = getHtmlBodyClass.LoadIndexConfig(core, adminContext);
                string Title = adminContent.name + " Columns";
                string Description = "Use the icons to add, remove and modify your personal column prefernces for this content (" + adminContent.name + "). Hit OK when complete. Hit Reset to restore your column preferences for this content to the site's default column preferences.";
                int ToolsAction = core.docProperties.getInteger("dta");
                int TargetFieldID = core.docProperties.getInteger("fi");
                string TargetFieldName = core.docProperties.getText("FieldName");
                int ColumnPointer = core.docProperties.getInteger("dtcn");
                const string RequestNameAddField = "addfield";
                string FieldNameToAdd = genericController.vbUCase(core.docProperties.getText(RequestNameAddField));
                const string RequestNameAddFieldID = "addfieldID";
                int FieldIDToAdd = core.docProperties.getInteger(RequestNameAddFieldID);
                bool NeedToReloadConfig = core.docProperties.getBoolean("NeedToReloadConfig");
                bool AllowContentAutoLoad = false;
                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                //
                //--------------------------------------------------------------------------------
                // Process actions
                //--------------------------------------------------------------------------------
                //
                if (adminContent.id != 0) {
                    cdefModel CDef = cdefModel.getCdef(core, adminContent.id);
                    int ColumnWidthTotal = 0;
                    if (ToolsAction != 0) {
                        //
                        // Block contentautoload, then force a load at the end
                        //
                        AllowContentAutoLoad = (core.siteProperties.getBoolean("AllowContentAutoLoad", true));
                        core.siteProperties.setProperty("AllowContentAutoLoad", false);
                        bool NeedToReloadCDef = false;
                        int CSSource = 0;
                        int CSTarget = 0;
                        int SourceContentID = 0;
                        string SourceName = null;
                        //
                        // Make sure the FieldNameToAdd is not-inherited, if not, create new field
                        //
                        if (FieldIDToAdd != 0) {
                            foreach (KeyValuePair<string, cdefFieldModel> keyValuePair in adminContent.fields) {
                                cdefFieldModel field = keyValuePair.Value;
                                if (field.id == FieldIDToAdd) {
                                    //If CDef.fields(FieldPtr).Name = FieldNameToAdd Then
                                    if (field.inherited) {
                                        SourceContentID = field.contentId;
                                        SourceName = field.nameLc;
                                        CSSource = core.db.csOpen("Content Fields", "(ContentID=" + SourceContentID + ")and(Name=" + core.db.encodeSQLText(SourceName) + ")");
                                        if (core.db.csOk(CSSource)) {
                                            CSTarget = core.db.csInsertRecord("Content Fields");
                                            if (core.db.csOk(CSTarget)) {
                                                core.db.csCopyRecord(CSSource, CSTarget);
                                                core.db.csSet(CSTarget, "ContentID", adminContent.id);
                                                NeedToReloadCDef = true;
                                            }
                                            core.db.csClose(ref CSTarget);
                                        }
                                        core.db.csClose(ref CSSource);
                                    }
                                    break;
                                }
                            }
                        }
                        //
                        // Make sure all fields are not-inherited, if not, create new fields
                        //
                        foreach (var kvp in IndexConfig.Columns) {
                            indexConfigColumnClass column = kvp.Value;
                            cdefFieldModel field = adminContent.fields[column.Name.ToLower()];
                            if (field.inherited) {
                                SourceContentID = field.contentId;
                                SourceName = field.nameLc;
                                CSSource = core.db.csOpen("Content Fields", "(ContentID=" + SourceContentID + ")and(Name=" + core.db.encodeSQLText(SourceName) + ")");
                                if (core.db.csOk(CSSource)) {
                                    CSTarget = core.db.csInsertRecord("Content Fields");
                                    if (core.db.csOk(CSTarget)) {
                                        core.db.csCopyRecord(CSSource, CSTarget);
                                        core.db.csSet(CSTarget, "ContentID", adminContent.id);
                                        NeedToReloadCDef = true;
                                    }
                                    core.db.csClose(ref CSTarget);
                                }
                                core.db.csClose(ref CSSource);
                            }
                        }
                        //
                        // get current values for Processing
                        //
                        foreach (var kvp in IndexConfig.Columns) {
                            indexConfigColumnClass column = kvp.Value;
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
                                        indexConfigColumnClass column = null;
                                        foreach (var kvp in IndexConfig.Columns) {
                                            column = kvp.Value;
                                            column.Width = encodeInteger((column.Width * 80) / (double)ColumnWidthTotal);
                                        }
                                        column = new indexConfigColumnClass();
                                        int CSPointer = core.db.csOpenRecord("Content Fields", FieldIDToAdd, false, false);
                                        if (core.db.csOk(CSPointer)) {
                                            column.Name = core.db.csGet(CSPointer, "name");
                                            column.Width = 20;
                                        }
                                        core.db.csClose(ref CSPointer);
                                        IndexConfig.Columns.Add(column.Name.ToLower(), column);
                                        NeedToReloadConfig = true;
                                    }
                                    //
                                    break;
                                }
                            case ToolsActionRemoveField: {
                                    //
                                    // Remove a field to the index form
                                    //
                                    indexConfigColumnClass column = null;
                                    if (IndexConfig.Columns.ContainsKey(TargetFieldName.ToLower())) {
                                        column = IndexConfig.Columns[TargetFieldName.ToLower()];
                                        ColumnWidthTotal = ColumnWidthTotal + column.Width;
                                        IndexConfig.Columns.Remove(TargetFieldName.ToLower());
                                        //
                                        // Normalize the widths of the remaining columns
                                        //
                                        foreach (var kvp in IndexConfig.Columns) {
                                            column = kvp.Value;
                                            column.Width = encodeInteger((1000 * column.Width) / (double)ColumnWidthTotal);
                                        }
                                        NeedToReloadConfig = true;
                                    }
                                    break;
                                }
                            case ToolsActionMoveFieldLeft: {
                                    //
                                    // Move column field left
                                    //
                                    //If IndexConfig.Columns.Count > 1 Then
                                    //    MoveNextColumn = False
                                    //    For ColumnPointer = 1 To IndexConfig.Columns.Count - 1
                                    //        If TargetFieldName = IndexConfig.Columns(ColumnPointer).Name Then
                                    //            With IndexConfig.Columns(ColumnPointer)
                                    //                FieldPointerTemp = .FieldId
                                    //                NameTemp = .Name
                                    //                WidthTemp = .Width
                                    //                .FieldId = IndexConfig.Columns(ColumnPointer - 1).FieldId
                                    //                .Name = IndexConfig.Columns(ColumnPointer - 1).Name
                                    //                .Width = IndexConfig.Columns(ColumnPointer - 1).Width
                                    //            End With
                                    //            With IndexConfig.Columns(ColumnPointer - 1)
                                    //                .FieldId = FieldPointerTemp
                                    //                .Name = NameTemp
                                    //                .Width = WidthTemp
                                    //            End With
                                    //        End If
                                    //    Next
                                    //    NeedToReloadConfig = True
                                    //End If
                                    // end case
                                    break;
                                }
                            case ToolsActionMoveFieldRight: {
                                    //
                                    // Move Index column field right
                                    //
                                    //If IndexConfig.Columns.Count > 1 Then
                                    //    MoveNextColumn = False
                                    //    For ColumnPointer = 0 To IndexConfig.Columns.Count - 2
                                    //        If TargetFieldName = IndexConfig.Columns(ColumnPointer).Name Then
                                    //            With IndexConfig.Columns(ColumnPointer)
                                    //                FieldPointerTemp = .FieldId
                                    //                NameTemp = .Name
                                    //                WidthTemp = .Width
                                    //                .FieldId = IndexConfig.Columns(ColumnPointer + 1).FieldId
                                    //                .Name = IndexConfig.Columns(ColumnPointer + 1).Name
                                    //                .Width = IndexConfig.Columns(ColumnPointer + 1).Width
                                    //            End With
                                    //            With IndexConfig.Columns(ColumnPointer + 1)
                                    //                .FieldId = FieldPointerTemp
                                    //                .Name = NameTemp
                                    //                .Width = WidthTemp
                                    //            End With
                                    //        End If
                                    //    Next
                                    //    NeedToReloadConfig = True
                                    //End If
                                    // end case
                                    break;
                                }
                            case ToolsActionExpand: {
                                    //
                                    // Expand column
                                    //
                                    //ColumnWidthBalance = 0
                                    //If IndexConfig.Columns.Count > 1 Then
                                    //    '
                                    //    ' Calculate the total width of the non-target columns
                                    //    '
                                    //    ColumnWidthIncrease = CInt(ColumnWidthTotal * 0.1)
                                    //    For ColumnPointer = 0 To IndexConfig.Columns.Count - 1
                                    //        If TargetFieldName <> IndexConfig.Columns(ColumnPointer).Name Then
                                    //            ColumnWidthBalance = ColumnWidthBalance + IndexConfig.Columns(ColumnPointer).Width
                                    //        End If
                                    //    Next
                                    //    '
                                    //    ' Adjust all columns
                                    //    '
                                    //    If ColumnWidthBalance > 0 Then
                                    //        For ColumnPointer = 0 To IndexConfig.Columns.Count - 1
                                    //            With IndexConfig.Columns(ColumnPointer)
                                    //                If TargetFieldName = .Name Then
                                    //                    '
                                    //                    ' Target gets 10% increase
                                    //                    '
                                    //                    .Width = Int(.Width + ColumnWidthIncrease)
                                    //                Else
                                    //                    '
                                    //                    ' non-targets get their share of the shrinkage
                                    //                    '
                                    //                    .Width = CInt(.Width - ((ColumnWidthIncrease * .Width) / ColumnWidthBalance))
                                    //                End If
                                    //            End With
                                    //        Next
                                    //        NeedToReloadConfig = True
                                    //    End If
                                    //End If

                                    // end case
                                    break;
                                }
                            case ToolsActionContract: {
                                    //
                                    // Contract column
                                    //
                                    //ColumnWidthBalance = 0
                                    //If IndexConfig.Columns.Count > 0 Then
                                    //    '
                                    //    ' Calculate the total width of the non-target columns
                                    //    '
                                    //    ColumnWidthIncrease = CInt(-(ColumnWidthTotal * 0.1))
                                    //    For ColumnPointer = 0 To IndexConfig.Columns.Count - 1
                                    //        With IndexConfig.Columns(ColumnPointer)
                                    //            If TargetFieldName <> .Name Then
                                    //                ColumnWidthBalance = ColumnWidthBalance + IndexConfig.Columns(ColumnPointer).Width
                                    //            End If
                                    //        End With
                                    //    Next
                                    //    '
                                    //    ' Adjust all columns
                                    //    '
                                    //    If (ColumnWidthBalance > 0) And (ColumnWidthIncrease <> 0) Then
                                    //        For ColumnPointer = 0 To IndexConfig.Columns.Count - 1
                                    //            With IndexConfig.Columns(ColumnPointer)
                                    //                If TargetFieldName = .Name Then
                                    //                    '
                                    //                    ' Target gets 10% increase
                                    //                    '
                                    //                    .Width = Int(.Width + ColumnWidthIncrease)
                                    //                Else
                                    //                    '
                                    //                    ' non-targets get their share of the shrinkage
                                    //                    '
                                    //                    .Width = CInt(.Width - ((ColumnWidthIncrease * FieldWidth) / ColumnWidthBalance))
                                    //                End If
                                    //            End With
                                    //        Next
                                    //        NeedToReloadConfig = True
                                    //    End If
                                    //End If
                                    break;
                                }
                        }
                        //
                        // Reload CDef if it changed
                        //
                        if (NeedToReloadCDef) {
                            core.doc.clearMetaData();
                            core.cache.invalidateAll();
                            CDef = cdefModel.getCdef(core, adminContent.name);
                        }
                        //
                        // save indexconfig
                        //
                        if (NeedToReloadConfig) {
                            getHtmlBodyClass.SetIndexSQL_SaveIndexConfig(core, IndexConfig);
                            IndexConfig = getHtmlBodyClass.LoadIndexConfig(core, adminContext);
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
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                    Stream.Add("</tr></table>");
                    //
                    // print the column headers
                    //
                    ColumnWidthTotal = 0;
                    int InheritedFieldCount = 0;
                    if (IndexConfig.Columns.Count > 0) {
                        //
                        // Calc total width
                        //
                        foreach (var kvp in IndexConfig.Columns) {
                            indexConfigColumnClass column = kvp.Value;
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
                            foreach (var kvp in IndexConfig.Columns) {
                                indexConfigColumnClass column = kvp.Value;
                                //
                                // print column headers - anchored so they sort columns
                                //
                                ColumnWidth = encodeInteger(100 * (column.Width / (double)ColumnWidthTotal));
                                cdefFieldModel field = adminContent.fields[column.Name.ToLower()];
                                fieldId = field.id;
                                Caption = field.caption;
                                if (field.inherited) {
                                    Caption = Caption + "*";
                                    InheritedFieldCount = InheritedFieldCount + 1;
                                }
                                //AStart = "<a href=\"?" + core.doc.refreshQueryString + "&FieldName=" + htmlController.encodeHTML(field.nameLc) + "&fi=" + fieldId + "&dtcn=" + ColumnPtr + "&" + RequestNameAdminSubForm + "=" + AdminFormIndex_SubFormSetColumns;
                                Stream.Add("<td width=\"" + ColumnWidth + "%\" valign=\"top\" align=\"left\">" + Caption + "</td>");
                                //Stream.Add("<img src=\"/ccLib/images/black.GIF\" width=\"100%\" height=\"1\" >");
                                //Stream.Add(AStart + "&dta=" + ToolsActionRemoveField + "\"><img src=\"/ccLib/images/LibButtonDeleteUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A><br>");
                                //Stream.Add(AStart + "&dta=" + ToolsActionMoveFieldRight + "\"><img src=\"/ccLib/images/LibButtonMoveRightUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A><br>");
                                //Stream.Add(AStart + "&dta=" + ToolsActionMoveFieldLeft + "\"><img src=\"/ccLib/images/LibButtonMoveLeftUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A><br>");
                                ////Call Stream.Add(AStart & "&dta=" & ToolsActionSetAZ & """><img src=""/ccLib/images/LibButtonSortazUp.gif"" width=""50"" height=""15"" border=""0"" ></A><br>")
                                ////Call Stream.Add(AStart & "&dta=" & ToolsActionSetZA & """><img src=""/ccLib/images/LibButtonSortzaUp.gif"" width=""50"" height=""15"" border=""0"" ></A><br>")
                                //Stream.Add(AStart + "&dta=" + ToolsActionExpand + "\"><img src=\"/ccLib/images/LibButtonOpenUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A><br>");
                                //Stream.Add(AStart + "&dta=" + ToolsActionContract + "\"><img src=\"/ccLib/images/LibButtonCloseUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A>");
                                //Stream.Add("</span></td>");
                            }
                            Stream.Add("</tr>");
                            //
                            // -- body
                            Stream.Add("<tr>");
                            foreach (var kvp in IndexConfig.Columns) {
                                indexConfigColumnClass column = kvp.Value;
                                //
                                // print column headers - anchored so they sort columns
                                //
                                ColumnWidth = encodeInteger(100 * (column.Width / (double)ColumnWidthTotal));
                                cdefFieldModel field = adminContent.fields[column.Name.ToLower()];
                                fieldId = field.id;
                                Caption = field.caption;
                                if (field.inherited) {
                                    Caption = Caption + "*";
                                    InheritedFieldCount = InheritedFieldCount + 1;
                                }
                                //adminUIController Adminui = new adminUIController(core);
                                int ColumnPtr = 0;
                                string AStart = "<a href=\"?" + core.doc.refreshQueryString + "&FieldName=" + htmlController.encodeHtml(field.nameLc) + "&fi=" + fieldId + "&dtcn=" + ColumnPtr + "&" + RequestNameAdminSubForm + "=" + AdminFormIndex_SubFormSetColumns;
                                Stream.Add("<td width=\"" + ColumnWidth + "%\" valign=\"top\" align=\"left\">");
                                //Stream.Add("<td width=\"" + ColumnWidth + "%\" valign=\"top\" align=\"left\">" + SpanClassAdminNormal + Caption + "<br>");
                                Stream.Add("<img src=\"/ccLib/images/black.GIF\" width=\"100%\" height=\"1\" >");
                                Stream.Add(AStart + "&dta=" + ToolsActionRemoveField + "\"><img src=\"/ccLib/images/LibButtonDeleteUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionMoveFieldRight + "\"><img src=\"/ccLib/images/LibButtonMoveRightUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionMoveFieldLeft + "\"><img src=\"/ccLib/images/LibButtonMoveLeftUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A><br>");
                                //Call Stream.Add(AStart & "&dta=" & ToolsActionSetAZ & """><img src=""/ccLib/images/LibButtonSortazUp.gif"" width=""50"" height=""15"" border=""0"" ></A><br>")
                                //Call Stream.Add(AStart & "&dta=" & ToolsActionSetZA & """><img src=""/ccLib/images/LibButtonSortzaUp.gif"" width=""50"" height=""15"" border=""0"" ></A><br>")
                                Stream.Add(AStart + "&dta=" + ToolsActionExpand + "\"><img src=\"/ccLib/images/LibButtonOpenUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A><br>");
                                Stream.Add(AStart + "&dta=" + ToolsActionContract + "\"><img src=\"/ccLib/images/LibButtonCloseUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A>");
                                Stream.Add("</td>");
                                //Stream.Add("</span></td>");
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
                        Stream.Add(SpanClassAdminNormal + "<br>");
                        foreach (KeyValuePair<string, cdefFieldModel> keyValuePair in adminContent.fields) {
                            cdefFieldModel field = keyValuePair.Value;
                            //
                            // display the column if it is not in use
                            //
                            if (!IndexConfig.Columns.ContainsKey(field.nameLc)) {
                                if (field.fieldTypeId == FieldTypeIdFile) {
                                    //
                                    // file can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (file field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdFileText) {
                                    //
                                    // filename can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (text file field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdFileHTML) {
                                    //
                                    // filename can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (html file field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdFileCSS) {
                                    //
                                    // css filename can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (css file field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdFileXML) {
                                    //
                                    // xml filename can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (xml file field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdFileJavascript) {
                                    //
                                    // javascript filename can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (javascript file field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdLongText) {
                                    //
                                    // long text can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (long text field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdHTML) {
                                    //
                                    // long text can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (long text field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdFileImage) {
                                    //
                                    // long text can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (image field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdRedirect) {
                                    //
                                    // long text can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (redirect field)<br>");
                                } else if (field.fieldTypeId == FieldTypeIdManyToMany) {
                                    //
                                    // many to many can not be search
                                    //
                                    Stream.Add("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " + field.caption + " (many-to-many field)<br>");
                                } else {
                                    //
                                    // can be used as column header
                                    //
                                    Stream.Add("<a href=\"?" + core.doc.refreshQueryString + "&fi=" + field.id + "&dta=" + ToolsActionAddField + "&" + RequestNameAddFieldID + "=" + field.id + "&" + RequestNameAdminSubForm + "=" + AdminFormIndex_SubFormSetColumns + "\"><img src=\"/ccLib/images/LibButtonAddUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A> " + field.caption + "<br>");
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
                core.siteProperties.setProperty("AllowContentAutoLoad", genericController.encodeText(AllowContentAutoLoad));
                //Stream.Add( core.main_GetFormInputHidden("NeedToReloadConfig", NeedToReloadConfig))

                string Content = ""
                    + Stream.Text
                    + htmlController.inputHidden("cid",adminContent.id.ToString())
                    + htmlController.inputHidden(rnAdminForm, "1")
                    + htmlController.inputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormSetColumns)
                    + "";
                result = adminUIController.getBody(core, Title, ButtonOK + "," + ButtonReset, "", false, false, Description, "", 10, Content);
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
                logController.handleError(core, ex);
            }
            return result;
        }
    }
}
