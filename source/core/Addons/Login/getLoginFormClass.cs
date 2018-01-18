﻿
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
namespace Contensive.Core.Addons.Login {
    public class getLoginFormClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// Login Form
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            string returnHtml = "";
            try {
                CPClass processor = (CPClass)cp;
                bool forceDefaultLogin = cp.Doc.GetBoolean("Force Default Login");
                returnHtml = loginController.getLoginForm(processor.core, forceDefaultLogin);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return returnHtml;
        }
    }
}
