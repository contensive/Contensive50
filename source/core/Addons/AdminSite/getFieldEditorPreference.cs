
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
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Addons.AdminSite {
    //
    public class getFieldEditorPreference : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// getFieldEditorPreference remote method - cut-paste from legacy init()
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            string result = "";
            try {
                CPClass processor = (CPClass)cp;
                coreClass cpCore = processor.core;

                //
                // When editing in admin site, if a field has multiple editors (addons as editors), you main_Get an icon
                //   to click to select the editor. When clicked, a fancybox opens to display a form. The onStart of
                //   he fancybox calls this ajax call and puts the return in the div that is displayed. Return a list
                //   of addon editors compatible with the field type.
                //
                string addonDefaultEditorName = "";
                int addonDefaultEditorId = 0;
                int fieldId = cpCore.docProperties.getInteger("fieldid");
                //
                // main_Get name of default editor
                //
                string Sql = "select top 1"
                                        + " a.name,a.id"
                                        + " from ccfields f left join ccAggregateFunctions a on a.id=f.editorAddonId"
                                        + " where"
                                        + " f.ID = " + fieldId + "";
                DataTable dt = cpCore.db.executeQuery(Sql);
                if (dt.Rows.Count > 0) {
                    foreach (DataRow rsDr in dt.Rows) {
                        addonDefaultEditorName = "&nbsp;(" + genericController.encodeText(rsDr["name"]) + ")";
                        addonDefaultEditorId = genericController.encodeInteger(rsDr["id"]);
                    }
                }
                //
                string radioGroupName = "setEditorPreference" + fieldId;
                int currentEditorAddonId = cpCore.docProperties.getInteger("currentEditorAddonId");
                int submitFormId = cpCore.docProperties.getInteger("submitFormId");
                Sql = "select f.name,c.name,r.addonid,a.name as addonName"
                                        + " from (((cccontent c"
                                        + " left join ccfields f on f.contentid=c.id)"
                                        + " left join ccAddonContentFieldTypeRules r on r.contentFieldTypeID=f.type)"
                                        + " left join ccAggregateFunctions a on a.id=r.AddonId)"
                                        + " where f.id=" + fieldId;

                dt = cpCore.db.executeQuery(Sql);
                if (dt.Rows.Count > 0) {
                    foreach (DataRow rsDr in dt.Rows) {
                        int addonId = genericController.encodeInteger(rsDr["addonid"]);
                        if ((addonId != 0) & (addonId != addonDefaultEditorId)) {
                            result = result + "\r\n\t<div class=\"radioCon\">" + cpCore.html.inputRadio(radioGroupName, genericController.encodeText(addonId), currentEditorAddonId.ToString()) + "&nbsp;Use " + genericController.encodeText(rsDr["addonName"]) + "</div>";
                        }

                    }
                }

                string OnClick = ""
                                        + "var a=document.getElementsByName('" + radioGroupName + "');"
                                        + "for(i=0;i<a.length;i++) {"
                                        + "if(a[i].checked){var v=a[i].value}"
                                        + "}"
                                        + "document.getElementById('fieldEditorPreference').value='" + fieldId + ":'+v;"
                                        + "cj.admin.saveEmptyFieldList('FormEmptyFieldList');"
                                        + "document.getElementById('adminEditForm').submit();"
                                        + "";

                result = ""
                                        + "\r\n\t<h1>Editor Preference</h1>"
                                        + "\r\n\t<p>Select the editor you will use for this field. Select default if you want to use the current system default.</p>"
                                        + "\r\n\t<div class=\"radioCon\">" + cpCore.html.inputRadio("setEditorPreference" + fieldId, "0", "0") + "&nbsp;Use Default Editor" + addonDefaultEditorName + "</div>"
                                        + "\r\n\t" + result + "\r\n\t<div class=\"buttonCon\">"
                                        + "\r\n\t<button type=\"button\" onclick=\"" + OnClick + "\">Select</button>"
                                        + "\r\n\t</div>"
                                        + "";


            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
