
using Contensive.BaseClasses;
using Contensive.Processor.Controllers;
using System;
//
namespace Contensive.Processor.Addons.Housekeeping {
    /// <summary>
    /// support for housekeeping functions
    /// </summary>
    public class HouseKeepClass : AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// execute hourly tasks
        /// </summary>
        /// <param name="core"></param>
        public static void executeHourlyTasks(CoreController core) {
            try {
                //
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
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
                //
                LogController.logInfo(core, "Housekeep");
                //
                var env = new HouseKeepEnvironmentModel(core);
                int TimeoutSave = core.db.sqlCommandTimeout;
                core.db.sqlCommandTimeout = 1800;
                //
                // -- hourly tasks
                HourlyTasksClass.executeHourlyTasks(core, env);
                //
                // -- daily tasks
                if (env.forceHousekeep || env.runDailyTasks) {
                    DailyTasksClass.executeDailyTasks(core, env);
                }
                core.db.sqlCommandTimeout = TimeoutSave;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
