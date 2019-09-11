
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

using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
namespace Contensive.Addons.PageManager {
    //
    public class getHtmlBodyClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// pageManager addon interface
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            try {
                // removed "<div class=\"ccBodyWeb\">" + PageContentController.getHtmlBody(core) + "</div>";
                return PageContentController.getHtmlBody(((CPClass)cp).core);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return "<div style=\"width:600px;margin:20px auto;\"><h1>Server Error</h1><p>There was an issue on this site that blocked your content. Thank you for your patience.</p></div>";
            }
        }
    }
}   