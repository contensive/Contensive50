
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
    public class getAjaxVisitPropertyClass : Contensive.BaseClasses.AddonBaseClass {
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
                gd.col = new List<ColsType>();
                gd.row = new List<RowsType>();
                gd.IsEmpty = false;
                for (var ptr = 0; ptr <= Args.GetUpperBound(0); ptr++) {
                    ColsType col = new ColsType();
                    CellType cell = new CellType();
                    string[] ArgNameValue = Args[ptr].Split('=');
                    col.Id = ArgNameValue[0];
                    col.Label = ArgNameValue[0];
                    col.Type = "string";
                    string PropertyValue = "";
                    if (ArgNameValue.GetUpperBound(0) > 0) {
                        PropertyValue = ArgNameValue[1];
                    }
                    cell.v = core.visitProperty.getText(ArgNameValue[0], PropertyValue);
                    gd.row[0].Cell.Add(cell);
                }
                result = remoteQueryController.main_FormatRemoteQueryOutput(core, gd, RemoteFormatEnum.RemoteFormatJsonNameValue);
                result = core.html.encodeHTML(result);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
