
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.BaseClasses;
using Contensive.Processor;

using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
using System.IO;
using static Contensive.BaseClasses.CPFileSystemBaseClass;
using Contensive.Processor.Exceptions;
using Contensive.Processor.Models.Domain;
//
namespace Contensive.Addons.Housekeeping {
    /// <summary>
    /// support for housekeeping functions
    /// </summary>
    public class HouseKeepClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// addon interface
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(CPBaseClass cp) {
            string result = "";
            try {
                CoreController core = ((CPClass)cp).core;
                var env = new HouseKeepEnvironmentModel(core);
                int TimeoutSave = core.db.sqlCommandTimeout;
                core.db.sqlCommandTimeout = 1800;
                //
                // -- hourly tasks
                HourlyTasksClass.housekeep(core, env);
                //
                // -- daily tasks
                if (env.force || env.runDailyTasks) {
                    DailyTasksClass.housekeep(core, env);
                }
                core.db.sqlCommandTimeout = TimeoutSave;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
