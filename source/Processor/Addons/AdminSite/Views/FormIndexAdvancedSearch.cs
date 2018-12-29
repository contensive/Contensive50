
using System;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
using static Contensive.Addons.AdminSite.Controllers.AdminUIController;
using Contensive.Addons.AdminSite.Controllers;

namespace Contensive.Addons.AdminSite {
    public class FormIndexAdvancedSearchClass {
        //
        //=================================================================================
        //
        //=================================================================================
        //
        public static string get(CPClass cp, CoreController core, AdminDataModel adminData) {
            string returnForm = "";
            try {
                //
                string SearchValue = null;
                FindWordMatchEnum MatchOption = 0;
                int FormFieldPtr = 0;
                int FormFieldCnt = 0;
                MetaModel CDef = null;
                string FieldName = null;
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                int FieldPtr = 0;
                bool RowEven = false;
                string RQS = null;
                string[] FieldNames = null;
                string[] FieldCaption = null;
                int[] fieldId = null;
                int[] fieldTypeId = null;
                string[] FieldValue = null;
                int[] FieldMatchOptions = null;
                int FieldMatchOption = 0;
                string[] FieldLookupContentName = null;
                string[] FieldLookupList = null;
                int ContentID = 0;
                int FieldCnt = 0;
                int FieldSize = 0;
                int RowPointer = 0;
                //adminUIController Adminui = new adminUIController(core);
                string LeftButtons = "";
                string ButtonBar = null;
                string Title = null;
                string TitleBar = null;
                string Content = null;

                //
                // Process last form
                //
                string Button = core.docProperties.getText("button");
                IndexConfigClass IndexConfig = null;
                if (!string.IsNullOrEmpty(Button)) {
                    switch (Button) {
                        case ButtonSearch:
                            IndexConfig = IndexConfigClass.get(core, adminData);
                            FormFieldCnt = core.docProperties.getInteger("fieldcnt");
                            if (FormFieldCnt > 0) {
                                for (FormFieldPtr = 0; FormFieldPtr < FormFieldCnt; FormFieldPtr++) {
                                    FieldName = GenericController.vbLCase(core.docProperties.getText("fieldname" + FormFieldPtr));
                                    MatchOption = (FindWordMatchEnum)core.docProperties.getInteger("FieldMatch" + FormFieldPtr);
                                    switch (MatchOption) {
                                        case FindWordMatchEnum.MatchEquals:
                                        case FindWordMatchEnum.MatchGreaterThan:
                                        case FindWordMatchEnum.matchincludes:
                                        case FindWordMatchEnum.MatchLessThan:
                                            SearchValue = core.docProperties.getText("FieldValue" + FormFieldPtr);
                                            break;
                                        default:
                                            SearchValue = "";
                                            break;
                                    }
                                    if (!IndexConfig.findWords.ContainsKey(FieldName)) {
                                        //
                                        // fieldname not found, save if not FindWordMatchEnum.MatchIgnore
                                        //
                                        if (MatchOption != FindWordMatchEnum.MatchIgnore) {
                                            IndexConfig.findWords.Add(FieldName, new IndexConfigClass.IndexConfigFindWordClass {
                                                Name = FieldName,
                                                MatchOption = MatchOption,
                                                Value = SearchValue
                                            });
                                        }
                                    } else {
                                        //
                                        // fieldname was found
                                        //
                                        IndexConfig.findWords[FieldName].MatchOption = MatchOption;
                                        IndexConfig.findWords[FieldName].Value = SearchValue;
                                    }
                                }
                            }
                            GetHtmlBodyClass.setIndexSQL_SaveIndexConfig(cp, core, IndexConfig);
                            return string.Empty;
                        case ButtonCancel:
                            return string.Empty;
                    }
                }
                IndexConfig = IndexConfigClass.get(core, adminData);
                Button = "CriteriaSelect";
                RQS = core.doc.refreshQueryString;
                //
                // ----- ButtonBar
                //
                if (adminData.ignore_legacyMenuDepth > 0) {
                    LeftButtons += AdminUIController.getButtonPrimary(ButtonClose, "window.close();");
                } else {
                    LeftButtons += AdminUIController.getButtonPrimary(ButtonCancel);
                    //LeftButtons &= core.main_GetFormButton(ButtonCancel, , , "return processSubmit(this)")
                }
                LeftButtons += AdminUIController.getButtonPrimary(ButtonSearch);
                //LeftButtons &= core.main_GetFormButton(ButtonSearch, , , "return processSubmit(this)")
                ButtonBar = AdminUIController.getButtonBar(core, LeftButtons, "");
                //
                // ----- TitleBar
                //
                Title = adminData.adminContent.name;
                Title = Title + " Advanced Search";
                Title = "<strong>" + Title + "</strong>";
                Title = SpanClassAdminNormal + Title + "</span>";
                //Title = Title & core.main_GetHelpLink(46, "Using the Advanced Search Page", BubbleCopy_AdminIndexPage)
                string TitleDescription = "<div>Enter criteria for each field to identify and select your results. The results of a search will have to have all of the criteria you enter.</div>";
                TitleBar = AdminUIController.getTitleBar(core, Title, TitleDescription);
                //
                // ----- List out all fields
                //
                CDef = MetaModel.createByUniqueName(core, adminData.adminContent.name);
                FieldSize = 100;
                Array.Resize(ref FieldNames, FieldSize + 1);
                Array.Resize(ref FieldCaption, FieldSize + 1);
                Array.Resize(ref fieldId, FieldSize + 1);
                Array.Resize(ref fieldTypeId, FieldSize + 1);
                Array.Resize(ref FieldValue, FieldSize + 1);
                Array.Resize(ref FieldMatchOptions, FieldSize + 1);
                Array.Resize(ref FieldLookupContentName, FieldSize + 1);
                Array.Resize(ref FieldLookupList, FieldSize + 1);
                foreach (KeyValuePair<string, MetaFieldModel> keyValuePair in adminData.adminContent.fields) {
                    MetaFieldModel field = keyValuePair.Value;
                    if (FieldPtr >= FieldSize) {
                        FieldSize = FieldSize + 100;
                        Array.Resize(ref FieldNames, FieldSize + 1);
                        Array.Resize(ref FieldCaption, FieldSize + 1);
                        Array.Resize(ref fieldId, FieldSize + 1);
                        Array.Resize(ref fieldTypeId, FieldSize + 1);
                        Array.Resize(ref FieldValue, FieldSize + 1);
                        Array.Resize(ref FieldMatchOptions, FieldSize + 1);
                        Array.Resize(ref FieldLookupContentName, FieldSize + 1);
                        Array.Resize(ref FieldLookupList, FieldSize + 1);
                    }
                    FieldName = GenericController.vbLCase(field.nameLc);
                    FieldNames[FieldPtr] = FieldName;
                    FieldCaption[FieldPtr] = field.caption;
                    fieldId[FieldPtr] = field.id;
                    fieldTypeId[FieldPtr] = field.fieldTypeId;
                    if (fieldTypeId[FieldPtr] == fieldTypeIdLookup) {
                        ContentID = field.lookupContentID;
                        if (ContentID > 0) {
                            FieldLookupContentName[FieldPtr] = MetaController.getContentNameByID(core, ContentID);
                        }
                        FieldLookupList[FieldPtr] = field.lookupList;
                    }
                    //
                    // set prepoplate value from indexconfig
                    //
                    if (IndexConfig.findWords.ContainsKey(FieldName)) {
                        FieldValue[FieldPtr] = IndexConfig.findWords[FieldName].Value;
                        FieldMatchOptions[FieldPtr] = (int)IndexConfig.findWords[FieldName].MatchOption;
                    }
                    FieldPtr += 1;
                }
                //        Criteria = "(active<>0)and(ContentID=" & adminContext.content.id & ")and(authorable<>0)"
                //        CS = core.app.csOpen("Content Fields", Criteria, "EditSortPriority")
                //        FieldPtr = 0
                //        Do While core.app.csv_IsCSOK(CS)
                //            If FieldPtr >= FieldSize Then
                //                FieldSize = FieldSize + 100
                //                ReDim Preserve FieldNames(FieldSize)
                //                ReDim Preserve FieldCaption(FieldSize)
                //                ReDim Preserve FieldID(FieldSize)
                //                ReDim Preserve FieldType(FieldSize)
                //                ReDim Preserve FieldValue(FieldSize)
                //                ReDim Preserve FieldMatchOptions(FieldSize)
                //                ReDim Preserve FieldLookupContentName(FieldSize)
                //                ReDim Preserve FieldLookupList(FieldSize)
                //            End If
                //            FieldName = genericController.vbLCase(csXfer.cs_getText(CS, "name"))
                //            FieldNames(FieldPtr) = FieldName
                //            FieldCaption(FieldPtr) = csXfer.cs_getText(CS, "Caption")
                //            FieldID(FieldPtr) = core.app.cs_getInteger(CS, "ID")
                //            FieldType(FieldPtr) = core.app.cs_getInteger(CS, "Type")
                //            If FieldType(FieldPtr) = 7 Then
                //                ContentID = core.app.cs_getInteger(CS, "LookupContentID")
                //                If ContentID > 0 Then
                //                    FieldLookupContentName(FieldPtr) = CdefController.getContentNameByID(core,ContentID)
                //                End If
                //                FieldLookupList(FieldPtr) = csXfer.cs_getText(CS, "LookupList")
                //            End If
                //            '
                //            ' set prepoplate value from indexconfig
                //            '
                //            With IndexConfig
                //                If .findwords.count > 0 Then
                //                    For Ptr = 0 To .findwords.count - 1
                //                        If .FindWords[Ptr].Name = FieldName Then
                //                            FieldValue(FieldPtr) = .FindWords[Ptr].Value
                //                            FieldMatchOptions(FieldPtr) = .FindWords[Ptr].MatchOption
                //                            Exit For
                //                        End If
                //                    Next
                //                End If
                //            End With
                //            If CriteriaCount > 0 Then
                //                For CriteriaPointer = 0 To CriteriaCount - 1
                //                    FieldMatchOptions(FieldPtr) = 0
                //                    If genericController.vbInstr(1, CriteriaValues(CriteriaPointer), FieldNames(FieldPtr) & "=", vbTextCompare) = 1 Then
                //                        NameValues = Split(CriteriaValues(CriteriaPointer), "=")
                //                        FieldValue(FieldPtr) = NameValues(1)
                //                        FieldMatchOptions(FieldPtr) = 1
                //                    ElseIf genericController.vbInstr(1, CriteriaValues(CriteriaPointer), FieldNames(FieldPtr) & ">", vbTextCompare) = 1 Then
                //                        NameValues = Split(CriteriaValues(CriteriaPointer), ">")
                //                        FieldValue(FieldPtr) = NameValues(1)
                //                        FieldMatchOptions(FieldPtr) = 2
                //                    ElseIf genericController.vbInstr(1, CriteriaValues(CriteriaPointer), FieldNames(FieldPtr) & "<", vbTextCompare) = 1 Then
                //                        NameValues = Split(CriteriaValues(CriteriaPointer), "<")
                //                        FieldValue(FieldPtr) = NameValues(1)
                //                        FieldMatchOptions(FieldPtr) = 3
                //                    End If
                //                Next
                //            End If
                //            FieldPtr = FieldPtr + 1
                //            Call core.app.nextCSRecord(CS)
                //        Loop
                //        Call core.app.closeCS(CS)
                FieldCnt = FieldPtr;
                //
                // Add headers to stream
                //
                returnForm = returnForm + "<table border=0 width=100% cellspacing=0 cellpadding=4>";
                //
                RowPointer = 0;
                for (FieldPtr = 0; FieldPtr < FieldCnt; FieldPtr++) {
                    returnForm = returnForm + HtmlController.inputHidden("fieldname" + FieldPtr, FieldNames[FieldPtr]);
                    RowEven = ((RowPointer % 2) == 0);
                    FieldMatchOption = FieldMatchOptions[FieldPtr];
                    switch (fieldTypeId[FieldPtr]) {
                        case _fieldTypeIdDate:
                            //
                            // Date

                            returnForm = returnForm + "<tr>"
                                + "<td class=\"ccAdminEditCaption\">" + FieldCaption[FieldPtr] + "</td>"
                                + "<td class=\"ccAdminEditField\">"
                                + "<div style=\"display:block;float:left;width:800px;\">"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchIgnore).ToString(), FieldMatchOption.ToString(), "") + "ignore</div>"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchEmpty).ToString(), FieldMatchOption.ToString(), "") + "empty</div>"
                                + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchNotEmpty).ToString(), FieldMatchOption.ToString(), "") + "not&nbsp;empty</div>"
                                + "<div style=\"display:block;float:left;width:50px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchEquals).ToString(), FieldMatchOption.ToString(), "") + "=</div>"
                                + "<div style=\"display:block;float:left;width:50px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchGreaterThan).ToString(), FieldMatchOption.ToString(), "") + "&gt;</div>"
                                + "<div style=\"display:block;float:left;width:50px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, encodeInteger(FindWordMatchEnum.MatchLessThan).ToString(), FieldMatchOption.ToString(), "") + "&lt;</div>"
                                + "<div style=\"display:block;float:left;width:300px;\">" + HtmlController.inputDate(core, "fieldvalue" + FieldPtr, encodeDate(FieldValue[FieldPtr])).Replace(">", " onFocus=\"ccAdvSearchText\">") + "</div>"
                                + "</div>"
                                + "</td>"
                                + "</tr>";
                            break;


                        //genericController.vbReplace(result, ">", ">")
                        case _fieldTypeIdCurrency:
                        case _fieldTypeIdFloat:
                        case _fieldTypeIdInteger:
                        case _fieldTypeIdAutoIdIncrement:
                            //
                            // -- Numeric - changed FindWordMatchEnum.MatchEquals to MatchInclude to be compatible with Find Search
                            returnForm = returnForm + "<tr>"
                            + "<td class=\"ccAdminEditCaption\">" + FieldCaption[FieldPtr] + "</td>"
                            + "<td class=\"ccAdminEditField\">"
                            + "<div style=\"display:block;float:left;width:800px;\">"
                            + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.MatchIgnore).ToString(), FieldMatchOption.ToString(), "") + "ignore</div>"
                            + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.MatchEmpty).ToString(), FieldMatchOption.ToString(), "") + "empty</div>"
                            + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.MatchNotEmpty).ToString(), FieldMatchOption.ToString(), "") + "not&nbsp;empty</div>"
                            + "<div style=\"display:block;float:left;width:50px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.matchincludes).ToString(), FieldMatchOption.ToString(), "n" + FieldPtr) + "=</div>"
                            + "<div style=\"display:block;float:left;width:50px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.MatchGreaterThan).ToString(), FieldMatchOption.ToString(), "") + "&gt;</div>"
                            + "<div style=\"display:block;float:left;width:50px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.MatchLessThan).ToString(), FieldMatchOption.ToString(), "") + "&lt;</div>"
                            + "<div style=\"display:block;float:left;width:300px;\">" + HtmlController.inputText(core, "fieldvalue" + FieldPtr, FieldValue[FieldPtr], 1, 5, "",false,false, "ccAdvSearchText").Replace(">", " onFocus=\"var e=getElementById('n" + FieldPtr + "');e.checked=1;\">") + "</div>"
                            + "</div>"
                            + "</td>"
                            + "</tr>";
                            RowPointer += 1;
                            break;
                        case _fieldTypeIdFile:
                        case _fieldTypeIdFileImage:
                            //
                            // File
                            //
                            returnForm = returnForm + "<tr>"
                            + "<td class=\"ccAdminEditCaption\">" + FieldCaption[FieldPtr] + "</td>"
                            + "<td class=\"ccAdminEditField\">"
                            + "<div style=\"display:block;float:left;width:800px;\">"
                            + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.MatchIgnore).ToString(), FieldMatchOption.ToString(), "") + "ignore</div>"
                            + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.MatchEmpty).ToString(), FieldMatchOption.ToString(), "") + "empty</div>"
                            + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.MatchNotEmpty).ToString(), FieldMatchOption.ToString(), "") + "not&nbsp;empty</div>"
                            + "</div>"
                            + "</td>"
                            + "</tr>";
                            RowPointer = RowPointer + 1;
                            break;
                        case _fieldTypeIdBoolean:
                            //
                            // Boolean
                            //
                            returnForm = returnForm + "<tr>"
                            + "<td class=\"ccAdminEditCaption\">" + FieldCaption[FieldPtr] + "</td>"
                            + "<td class=\"ccAdminEditField\">"
                            + "<div style=\"display:block;float:left;width:800px;\">"
                            + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.MatchIgnore).ToString(), FieldMatchOption.ToString(), "") + "ignore</div>"
                            + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.MatchTrue).ToString(), FieldMatchOption.ToString(), "") + "true</div>"
                            + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.MatchFalse).ToString(), FieldMatchOption.ToString(), "") + "false</div>"
                            + "</div>"
                            + "</td>"
                            + "</tr>";
                            break;
                        case _fieldTypeIdText:
                        case _fieldTypeIdLongText:
                        case _fieldTypeIdHTML:
                        case _fieldTypeIdFileHTML:
                        case _fieldTypeIdFileCSS:
                        case _fieldTypeIdFileJavascript:
                        case _fieldTypeIdFileXML:
                            //
                            // Text
                            //
                            returnForm = returnForm + "<tr>"
                            + "<td class=\"ccAdminEditCaption\">" + FieldCaption[FieldPtr] + "</td>"
                            + "<td class=\"ccAdminEditField\">"
                            + "<div style=\"display:block;float:left;width:800px;\">"
                            + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.MatchIgnore).ToString(), FieldMatchOption.ToString(), "") + "ignore</div>"
                            + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.MatchEmpty).ToString(), FieldMatchOption.ToString(), "") + "empty</div>"
                            + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.MatchNotEmpty).ToString(), FieldMatchOption.ToString(), "") + "not&nbsp;empty</div>"
                            + "<div style=\"display:block;float:left;width:150px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.matchincludes).ToString(), FieldMatchOption.ToString(), "t" + FieldPtr) + "includes</div>"
                            + "<div style=\"display:block;float:left;width:300px;\">" + HtmlController.inputText(core, "fieldvalue" + FieldPtr, FieldValue[FieldPtr], 1, 5, "", false, false, "ccAdvSearchText").Replace(">", " onFocus=\"var e=getElementById('t" + FieldPtr + "');e.checked=1;\">") + "</div>"
                            + "</div>"
                            + "</td>"
                            + "</tr>";
                            RowPointer = RowPointer + 1;
                            break;
                        case _fieldTypeIdLookup:
                        case _fieldTypeIdMemberSelect:
                            //
                            // Lookup
                            returnForm = returnForm + "<tr>"
                            + "<td class=\"ccAdminEditCaption\">" + FieldCaption[FieldPtr] + "</td>"
                            + "<td class=\"ccAdminEditField\">"
                            + "<div style=\"display:block;float:left;width:800px;\">"
                            + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.MatchIgnore).ToString(), FieldMatchOption.ToString(), "") + "ignore</div>"
                            + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.MatchEmpty).ToString(), FieldMatchOption.ToString(), "") + "empty</div>"
                            + "<div style=\"display:block;float:left;width:100px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.MatchNotEmpty).ToString(), FieldMatchOption.ToString(), "") + "not&nbsp;empty</div>"
                            + "<div style=\"display:block;float:left;width:150px;\">" + core.html.inputRadio("FieldMatch" + FieldPtr, ((int)FindWordMatchEnum.matchincludes).ToString(), FieldMatchOption.ToString(), "t" + FieldPtr) + "includes</div>"
                            + "<div style=\"display:block;float:left;width:300px;\">" + HtmlController.inputText( core, "fieldvalue" + FieldPtr, FieldValue[FieldPtr], 1, 5, "",false, false, "ccAdvSearchText").Replace(">", " onFocus=\"var e=getElementById('t" + FieldPtr + "'); e.checked= 1;\">") + "</div>"
                            + "</div>"
                            + "</td>"
                            + "</tr>";
                            RowPointer = RowPointer + 1;
                            break;
                    }
                }
                returnForm = returnForm + HtmlController.tableRowStart();
                returnForm = returnForm + HtmlController.tableCellStart("120", 1, RowEven, "right") + "<img src=/ContensiveBase/images/spacer.gif width=120 height=1></td>";
                returnForm = returnForm + HtmlController.tableCellStart("99%", 1, RowEven, "left") + "<img src=/ContensiveBase/images/spacer.gif width=1 height=1></td>";
                returnForm = returnForm + kmaEndTableRow;
                returnForm = returnForm + "</table>";
                Content = returnForm;
                //
                // Assemble LiveWindowTable
                //Stream.Add("\r\n" + htmlController.form_start(core));
                Stream.Add(ButtonBar);
                Stream.Add(TitleBar);
                Stream.Add(Content);
                Stream.Add(ButtonBar);
                Stream.Add("<input type=hidden name=fieldcnt VALUE=" + FieldCnt + ">");
                //Stream.Add( "<input type=hidden name=af VALUE=" & AdminFormIndex & ">")
                Stream.Add("<input type=hidden name=" + RequestNameAdminSubForm + " VALUE=" + AdminFormIndex_SubFormAdvancedSearch + ">");
                //Stream.Add("</form>");
                //        Stream.Add( CloseLiveWindowTable)
                //
                returnForm = HtmlController.form(core, Stream.Text);
                core.html.addTitle(adminData.adminContent.name + " Advanced Search");
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnForm;
        }
    }
}
