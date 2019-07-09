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
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using System.Text;
//
namespace Contensive.Addons.Diagnostics {
    //
    public class StatusClass : Contensive.BaseClasses.AddonBaseClass {
        //
        private const string asdf = "";
        //
        //====================================================================================================
        /// <summary>
        /// Returns OK on success
        /// + available drive space
        /// + log size
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            try {
                var resultList = new StringBuilder();
                var core = ((CPClass)(cp)).core;
                string pauseHint = " To pause alarm " + ((cp.User.IsAdmin) ? "set site property 'Diagnostics Pause Until Date' or [/status?pauseUntil=" + DateTime.Now.AddHours(1).ToString() + "]." : "login as administrator.");
                cp.Response.SetType("text/plain");
                if (cp.Site.GetDate("Diagnostics pause until date") > DateTime.Now) {
                    return "ok, diagnostics paused until " + cp.Site.GetDate("Diagnostics pause until date").ToString() + "." + Environment.NewLine + resultList.ToString();
                }
                if (cp.User.IsAdmin) {
                    resultList.Append("ok, ." + Environment.NewLine + resultList.ToString());
                }
                foreach (var addon in DbBaseModel.createList<AddonModel>(core, "(diagnostic>0)")) {
                    string testResult = core.addon.execute(addon, new BaseClasses.CPUtilsBaseClass.addonExecuteContext());
                    if (string.IsNullOrWhiteSpace(testResult)) { return "ERROR, diagnostic [" + addon.name + "] failed, it returned an empty result." + pauseHint; }
                    if (testResult.Length < 2) { return "ERROR, diagnostic [" + addon.name + "] failed, it returned an invalid result." + pauseHint; }
                    if (testResult.Left(2).ToLower() != "ok") { return "ERROR, diagnostic [" + addon.name + "] failed, it returned [" + testResult + "]" + pauseHint; }
                    resultList.AppendLine(testResult);
                }
                return "ok, all tests passes." + Environment.NewLine + resultList.ToString();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return "ERROR, unexpected exception during diagnostics";
            }
        }
    }
}