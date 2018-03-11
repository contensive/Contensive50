
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
//
namespace Contensive.Core.Addons.AdminSite {
    //
    public class setAjaxVisitPropertyClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// getFieldEditorPreference remote method
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            string result = "";
            try {
                CPClass processor = (CPClass)cp;
                coreController core = processor.core;

                string ArgList = core.docProperties.getText("args");
                string[] Args = ArgList.Split('&');
                GoogleDataType gd = new GoogleDataType();
                gd.IsEmpty = true;
                for (var Ptr = 0; Ptr <= Args.GetUpperBound(0); Ptr++) {
                    string[] ArgNameValue = Args[Ptr].Split('=');
                    string PropertyName = ArgNameValue[0];
                    string PropertyValue = "";
                    if (ArgNameValue.GetUpperBound(0) > 0) {
                        PropertyValue = ArgNameValue[1];
                    }
                    core.visitProperty.setProperty(PropertyName, PropertyValue);
                }
                result = remoteQueryController.main_FormatRemoteQueryOutput(core, gd, RemoteFormatEnum.RemoteFormatJsonNameValue);
                result = htmlController.encodeHtml(result);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
