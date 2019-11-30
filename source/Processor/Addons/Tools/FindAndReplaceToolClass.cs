
using System;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Addons.AdminSite.Controllers;
using System.Text;
using System.Collections.Generic;
using Contensive.Models.Db;
//
namespace Contensive.Processor.Addons.Tools {
    //
    public class FindAndReplaceToolClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// addon method, deliver complete Html admin site
        /// blank return on OK or cancel button
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cpBase) {
            return GetForm_FindAndReplace(((CPClass)cpBase).core);
        }
        //
        //=============================================================================
        // Find and Replace launch tool
        //=============================================================================
        //
        private string GetForm_FindAndReplace(CoreController core) {
            string result = "";
            try {
                //
                bool IsDeveloper = false;
                string QS = null;
                string RecordName = null;
                int RowCnt = 0;
                string TopHalf = "";
                string BottomHalf = "";
                int RowPtr = 0;
                int RecordId = 0;
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                // Dim runAtServer As New runAtServerClass(core)
                string CDefList = "";
                string FindText = "";
                string ReplaceText = "";
                string Button = null;
                int ReplaceRows = 0;
                int FindRows = 0;
                string lcName = null;
                //
                Stream.Add(AdminUIController.getHeaderTitleDescription("Find and Replace", "This tool runs a find and replace operation on content throughout the site."));
                //
                // Process the form
                //
                Button = core.docProperties.getText("button");
                //
                IsDeveloper = core.session.isAuthenticatedDeveloper();
                if (Button == ButtonFindAndReplace) {
                    RowCnt = core.docProperties.getInteger("CDefRowCnt");
                    if (RowCnt > 0) {
                        for (RowPtr = 0; RowPtr < RowCnt; RowPtr++) {
                            if (core.docProperties.getBoolean("Cdef" + RowPtr)) {
                                lcName = GenericController.toLCase(core.docProperties.getText("CDefName" + RowPtr));
                                if (IsDeveloper || (lcName == "page content") || (lcName == "copy content") || (lcName == "page templates")) {
                                    CDefList = CDefList + "," + lcName;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(CDefList)) {
                            CDefList = CDefList.Substring(1);
                        }
                        FindText = core.docProperties.getText("FindText");
                        ReplaceText = core.docProperties.getText("ReplaceText");
                        QS = "app=" + encodeNvaArgument(core.appConfig.name) + "&FindText=" + encodeNvaArgument(FindText) + "&ReplaceText=" + encodeNvaArgument(ReplaceText) + "&CDefNameList=" + encodeNvaArgument(CDefList);
                        var cmdDetail = new TaskModel.CmdDetailClass {
                            addonId = 0,
                            addonName = "GetForm_FindAndReplace",
                            args = GenericController.convertQSNVAArgumentstoDocPropertiesList(core, QS)
                        };
                        TaskSchedulerController.addTaskToQueue(core, cmdDetail, false);
                        Stream.Add("Find and Replace has been requested for content definitions [" + CDefList + "], finding [" + FindText + "] and replacing with [" + ReplaceText + "]");
                    }
                } else {
                    CDefList = "Page Content,Copy Content,Page Templates";
                    FindText = "";
                    ReplaceText = "";
                }
                //
                // Display form
                //
                FindRows = core.docProperties.getInteger("SQLRows");
                if (FindRows == 0) {
                    FindRows = core.userProperty.getInteger("FindAndReplaceFindRows", 1);
                } else {
                    core.userProperty.setProperty("FindAndReplaceFindRows", FindRows.ToString());
                }
                ReplaceRows = core.docProperties.getInteger("ReplaceRows");
                if (ReplaceRows == 0) {
                    ReplaceRows = core.userProperty.getInteger("FindAndReplaceReplaceRows", 1);
                } else {
                    core.userProperty.setProperty("FindAndReplaceReplaceRows", ReplaceRows.ToString());
                }
                //
                Stream.Add("<div>Find</div>");
                Stream.Add("<TEXTAREA NAME=\"FindText\" ROWS=\"" + FindRows + "\" ID=\"FindText\" STYLE=\"width: 800px;\">" + FindText + "</TEXTAREA>");
                Stream.Add("&nbsp;<INPUT TYPE=\"Text\" TabIndex=-1 NAME=\"FindTextRows\" SIZE=\"3\" VALUE=\"" + FindRows + "\" ID=\"\"  onchange=\"FindText.rows=FindTextRows.value; return true\"> Rows");
                Stream.Add("<br><br>");
                //
                Stream.Add("<div>Replace it with</div>");
                Stream.Add("<TEXTAREA NAME=\"ReplaceText\" ROWS=\"" + ReplaceRows + "\" ID=\"ReplaceText\" STYLE=\"width: 800px;\">" + ReplaceText + "</TEXTAREA>");
                Stream.Add("&nbsp;<INPUT TYPE=\"Text\" TabIndex=-1 NAME=\"ReplaceTextRows\" SIZE=\"3\" VALUE=\"" + ReplaceRows + "\" ID=\"\"  onchange=\"ReplaceText.rows=ReplaceTextRows.value; return true\"> Rows");
                Stream.Add("<br><br>");
                //
                using (var csData = new CsModel(core)) {
                    csData.open("Content");
                    while (csData.ok()) {
                        RecordName = csData.getText("Name");
                        lcName = GenericController.toLCase(RecordName);
                        if (IsDeveloper || (lcName == "page content") || (lcName == "copy content") || (lcName == "page templates")) {
                            RecordId = csData.getInteger("ID");
                            if (GenericController.strInstr(1, "," + CDefList + ",", "," + RecordName + ",") != 0) {
                                TopHalf = TopHalf + "<div>" + HtmlController.checkbox("Cdef" + RowPtr, true) + HtmlController.inputHidden("CDefName" + RowPtr, RecordName) + "&nbsp;" + csData.getText("Name") + "</div>";
                            } else {
                                BottomHalf = BottomHalf + "<div>" + HtmlController.checkbox("Cdef" + RowPtr, false) + HtmlController.inputHidden("CDefName" + RowPtr, RecordName) + "&nbsp;" + csData.getText("Name") + "</div>";
                            }
                        }
                        csData.goNext();
                        RowPtr = RowPtr + 1;
                    }
                }
                Stream.Add(TopHalf + BottomHalf + HtmlController.inputHidden("CDefRowCnt", RowPtr));
                //
                result = AdminUIController.getToolForm(core, Stream.Text, ButtonCancel + "," + ButtonFindAndReplace);
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }
        //
    }
}

