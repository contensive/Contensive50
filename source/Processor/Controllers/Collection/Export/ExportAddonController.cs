
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualBasic;
using Contensive.BaseClasses;
using Contensive.Models.Db;

namespace Contensive.Processor.Controllers {
    public static class ExportAddonController {
        // 
        // ====================================================================================================

        public static string getAddonNode(CPBaseClass cp, int addonid, ref string Return_IncludeModuleGuidList, ref string Return_IncludeSharedStyleGuidList) {
            string result = "";
            try {
                using (CPCSBaseClass CS = cp.CSNew()) {
                    if (CS.OpenRecord("Add-ons", addonid)) {
                        string addonName = CS.GetText("name");
                        bool processRunOnce = CS.GetBoolean("ProcessRunOnce");
                        if (((Strings.LCase(addonName) == "oninstall") | (Strings.LCase(addonName) == "_oninstall")))
                            processRunOnce = true;
                        // 
                        // ActiveX DLL node is being deprecated. This should be in the collection resource section
                        result += ExportController.getNodeText(cp, "Copy", CS.GetText("Copy"));
                        result += ExportController.getNodeText(cp, "CopyText", CS.GetText("CopyText"));
                        // 
                        // DLL
                        result += ExportController.getNodeText(cp, "ActiveXProgramID", CS.GetText("objectprogramid"), true);
                        result += ExportController.getNodeText(cp, "DotNetClass", CS.GetText("DotNetClass"));
                        // 
                        // Features
                        result += ExportController.getNodeText(cp, "ArgumentList", CS.GetText("ArgumentList"));
                        result += ExportController.getNodeBoolean(cp, "AsAjax", CS.GetBoolean("AsAjax"));
                        result += ExportController.getNodeBoolean(cp, "Filter", CS.GetBoolean("Filter"));
                        result += ExportController.getNodeText(cp, "Help", CS.GetText("Help"));
                        result += ExportController.getNodeText(cp, "HelpLink", CS.GetText("HelpLink"));
                        result += "\r\n" + "\t" + "<Icon Link=\"" + CS.GetText("iconfilename") + "\" width=\"" + CS.GetInteger("iconWidth") + "\" height=\"" + CS.GetInteger("iconHeight") + "\" sprites=\"" + CS.GetInteger("iconSprites") + "\" />";
                        result += ExportController.getNodeBoolean(cp, "InIframe", CS.GetBoolean("InFrame"));
                        result += CS.FieldOK("BlockEditTools") ? ExportController.getNodeBoolean(cp, "BlockEditTools", CS.GetBoolean("BlockEditTools")) : "";
                        result += CS.FieldOK("aliasList") ? ExportController.getNodeText(cp, "AliasList", CS.GetText("aliasList")) : "";
                        // 
                        // -- Form XML
                        result += ExportController.getNodeText(cp, "FormXML", CS.GetText("FormXML"));
                        // 
                        // -- addon dependencies
                        using (CPCSBaseClass CS2 = cp.CSNew()) {
                            CS2.Open("Add-on Include Rules", "addonid=" + addonid);
                            while (CS2.OK()) {
                                int IncludedAddonID = CS2.GetInteger("IncludedAddonID");
                                using (CPCSBaseClass CS3 = cp.CSNew()) {
                                    CS3.Open("Add-ons", "ID=" + IncludedAddonID);
                                    if (CS3.OK()) {
                                        string Guid = CS3.GetText("ccGuid");
                                        if (Guid == "") {
                                            Guid = cp.Utils.CreateGuid();
                                            CS3.SetField("ccGuid", Guid);
                                        }
                                        result += "\r\n" + "\t" + "<IncludeAddon name=\"" + System.Net.WebUtility.HtmlEncode(CS3.GetText("name")) + "\" guid=\"" + Guid + "\"/>";
                                    }
                                    CS3.Close();
                                }
                                CS2.GoNext();
                            }
                            CS2.Close();
                        }
                        // 
                        // -- is inline/block
                        result += ExportController.getNodeBoolean(cp, "IsInline", CS.GetBoolean("IsInline"));
                        // 
                        // -- javascript (xmlnode may not match Db filename)
                        result += ExportController.getNodeText(cp, "JavascriptInHead", CS.GetText("JSFilename"));
                        result += ExportController.getNodeBoolean(cp, "javascriptForceHead", CS.GetBoolean("javascriptForceHead"));
                        result += ExportController.getNodeText(cp, "JSHeadScriptSrc", CS.GetText("JSHeadScriptSrc"));
                        // 
                        // -- javascript deprecated
                        result += ExportController.getNodeText(cp, "JSBodyScriptSrc", CS.GetText("JSBodyScriptSrc"), true);
                        result += ExportController.getNodeText(cp, "JavascriptBodyEnd", CS.GetText("JavascriptBodyEnd"), true);
                        result += ExportController.getNodeText(cp, "JavascriptOnLoad", CS.GetText("JavascriptOnLoad"), true);
                        // 
                        // -- Placements
                        result += ExportController.getNodeBoolean(cp, "Content", CS.GetBoolean("Content"));
                        result += ExportController.getNodeBoolean(cp, "Template", CS.GetBoolean("Template"));
                        result += ExportController.getNodeBoolean(cp, "Email", CS.GetBoolean("Email"));
                        result += ExportController.getNodeBoolean(cp, "Admin", CS.GetBoolean("Admin"));
                        result += ExportController.getNodeBoolean(cp, "OnPageEndEvent", CS.GetBoolean("OnPageEndEvent"));
                        result += ExportController.getNodeBoolean(cp, "OnPageStartEvent", CS.GetBoolean("OnPageStartEvent"));
                        result += ExportController.getNodeBoolean(cp, "OnBodyStart", CS.GetBoolean("OnBodyStart"));
                        result += ExportController.getNodeBoolean(cp, "OnBodyEnd", CS.GetBoolean("OnBodyEnd"));
                        result += ExportController.getNodeBoolean(cp, "RemoteMethod", CS.GetBoolean("RemoteMethod"));
                        result += CS.FieldOK("Diagnostic") ? ExportController.getNodeBoolean(cp, "Diagnostic", CS.GetBoolean("Diagnostic")) : "";
                        // 
                        // -- Process
                        result += ExportController.getNodeBoolean(cp, "ProcessRunOnce", processRunOnce);
                        result += ExportController.GetNodeInteger(cp, "ProcessInterval", CS.GetInteger("ProcessInterval"));
                        // 
                        // Meta
                        // 
                        result += ExportController.getNodeText(cp, "MetaDescription", CS.GetText("MetaDescription"));
                        result += ExportController.getNodeText(cp, "OtherHeadTags", CS.GetText("OtherHeadTags"));
                        result += ExportController.getNodeText(cp, "PageTitle", CS.GetText("PageTitle"));
                        result += ExportController.getNodeText(cp, "RemoteAssetLink", CS.GetText("RemoteAssetLink"));
                        // 
                        // Styles
                        string Styles = "";
                        if (!CS.GetBoolean("BlockDefaultStyles"))
                            Styles = CS.GetText("StylesFilename").Trim();
                        string StylesTest = CS.GetText("CustomStylesFilename").Trim();
                        if (StylesTest != "") {
                            if (Styles != "")
                                Styles = Styles + "\r\n" + StylesTest;
                            else
                                Styles = StylesTest;
                        }
                        result += ExportController.getNodeText(cp, "Styles", Styles);
                        result += ExportController.getNodeText(cp, "styleslinkhref", CS.GetText("styleslinkhref"));
                        // 
                        // 
                        // Scripting
                        // 
                        string NodeInnerText = CS.GetText("ScriptingCode").Trim();
                        if (NodeInnerText != "")
                            NodeInnerText = "\r\n" + "\t" + "\t" + "<Code>" + ExportController.EncodeCData(cp, NodeInnerText) + "</Code>";
                        using (CPCSBaseClass CS2 = cp.CSNew()) {
                            CS2.Open("Add-on Scripting Module Rules", "addonid=" + addonid);
                            while (CS2.OK()) {
                                int ScriptingModuleID = CS2.GetInteger("ScriptingModuleID");
                                using (CPCSBaseClass CS3 = cp.CSNew()) {
                                    CS3.Open("Scripting Modules", "ID=" + ScriptingModuleID);
                                    if (CS3.OK()) {
                                        string Guid = CS3.GetText("ccGuid");
                                        if (Guid == "") {
                                            Guid = cp.Utils.CreateGuid();
                                            CS3.SetField("ccGuid", Guid);
                                        }
                                        Return_IncludeModuleGuidList = Return_IncludeModuleGuidList + "\r\n" + Guid;
                                        NodeInnerText = NodeInnerText + "\r\n" + "\t" + "\t" + "<IncludeModule name=\"" + System.Net.WebUtility.HtmlEncode(CS3.GetText("name")) + "\" guid=\"" + Guid + "\"/>";
                                    }
                                    CS3.Close();
                                }
                                CS2.GoNext();
                            }
                            CS2.Close();
                        }
                        if (NodeInnerText == "")
                            result += "\r\n" + "\t" + "<Scripting Language=\"" + CS.GetText("ScriptingLanguageID") + "\" EntryPoint=\"" + CS.GetText("ScriptingEntryPoint") + "\" Timeout=\"" + CS.GetText("ScriptingTimeout") + "\"/>";
                        else
                            result += "\r\n" + "\t" + "<Scripting Language=\"" + CS.GetText("ScriptingLanguageID") + "\" EntryPoint=\"" + CS.GetText("ScriptingEntryPoint") + "\" Timeout=\"" + CS.GetText("ScriptingTimeout") + "\">" + NodeInnerText + "\r\n" + "\t" + "</Scripting>";
                        // 
                        // Shared Styles
                        // 
                        using (CPCSBaseClass CS2 = cp.CSNew()) {
                            CS2.Open("Shared Styles Add-on Rules", "addonid=" + addonid);
                            while (CS2.OK()) {
                                int styleId = CS2.GetInteger("styleId");
                                using (CPCSBaseClass CS3 = cp.CSNew()) {
                                    CS3.Open("shared styles", "ID=" + styleId);
                                    if (CS3.OK()) {
                                        string Guid = CS3.GetText("ccGuid");
                                        if (Guid == "") {
                                            Guid = cp.Utils.CreateGuid();
                                            CS3.SetField("ccGuid", Guid);
                                        }
                                        Return_IncludeSharedStyleGuidList = Return_IncludeSharedStyleGuidList + "\r\n" + Guid;
                                        result += "\r\n" + "\t" + "<IncludeSharedStyle name=\"" + System.Net.WebUtility.HtmlEncode(CS3.GetText("name")) + "\" guid=\"" + Guid + "\"/>";
                                    }
                                    CS3.Close();
                                }
                                CS2.GoNext();
                            }
                            CS2.Close();
                        }
                        // 
                        // Process Triggers
                        // 
                        NodeInnerText = "";
                        using (CPCSBaseClass CS2 = cp.CSNew()) {
                            CS2.Open("Add-on Content Trigger Rules", "addonid=" + addonid);
                            while (CS2.OK()) {
                                int TriggerContentID = CS2.GetInteger("ContentID");
                                using (CPCSBaseClass CS3 = cp.CSNew()) {
                                    CS3.Open("content", "ID=" + TriggerContentID);
                                    if (CS3.OK()) {
                                        string Guid = CS3.GetText("ccGuid");
                                        if (Guid == "") {
                                            Guid = cp.Utils.CreateGuid();
                                            CS3.SetField("ccGuid", Guid);
                                        }
                                        NodeInnerText = NodeInnerText + "\r\n" + "\t" + "\t" + "<ContentChange name=\"" + System.Net.WebUtility.HtmlEncode(CS3.GetText("name")) + "\" guid=\"" + Guid + "\"/>";
                                    }
                                    CS3.Close();
                                }
                                CS2.GoNext();
                            }
                            CS2.Close();
                        }
                        if (NodeInnerText != "")
                            result += "\r\n" + "\t" + "<ProcessTriggers>" + NodeInnerText + "\r\n" + "\t" + "</ProcessTriggers>";
                        // 
                        // Editors
                        // 
                        if (cp.Content.IsField("Add-on Content Field Type Rules", "id")) {
                            NodeInnerText = "";
                            using (CPCSBaseClass CS2 = cp.CSNew()) {
                                CS2.Open("Add-on Content Field Type Rules", "addonid=" + addonid);
                                while (CS2.OK()) {
                                    int fieldTypeID = CS2.GetInteger("contentFieldTypeID");
                                    string fieldType = cp.Content.GetRecordName("Content Field Types", fieldTypeID);
                                    if (fieldType != "")
                                        NodeInnerText = NodeInnerText + "\r\n" + "\t" + "\t" + "<type>" + fieldType + "</type>";
                                    CS2.GoNext();
                                }
                                CS2.Close();
                            }
                            if (NodeInnerText != "")
                                result += "\r\n" + "\t" + "<Editors>" + NodeInnerText + "\r\n" + "\t" + "</Editors>";
                        }
                        // 
                        string addonGuid = CS.GetText("ccGuid");
                        if ((string.IsNullOrWhiteSpace(addonGuid))) {
                            addonGuid = cp.Utils.CreateGuid();
                            CS.SetField("ccGuid", addonGuid);
                        }
                        string NavType = CS.GetText("NavTypeID");
                        if ((NavType == ""))
                            NavType = "Add-on";
                        result = ""
                        + "\r\n" + "\t" + "<Addon name=\"" + System.Net.WebUtility.HtmlEncode(addonName) + "\" guid=\"" + addonGuid + "\" type=\"" + NavType + "\">"
                        + ExportController.tabIndent(cp, result)
                        + "\r\n" + "\t" + "</Addon>";
                    }
                    CS.Close();
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "GetAddonNode");
            }
            return result;
        }
        // 
    }
}
