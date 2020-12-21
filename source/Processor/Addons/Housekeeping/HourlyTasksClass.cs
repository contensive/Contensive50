
using Contensive.Processor.Controllers;
using System;

namespace Contensive.Processor.Addons.Housekeeping {
    /// <summary>
    /// exeute housekeep tasks hourly
    /// </summary>
    public static class HourlyTasksClass {
        //
        //====================================================================================================
        /// <summary>
        /// exeute housekeep tasks hourly
        /// </summary>
        /// <param name="core"></param>
        /// <param name="env"></param>
        public static void executeHourlyTasks(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                LogController.logInfo(core, "executeHourlyTasks");
                //
                // -- delete temp files
                TempFilesClass.deleteFiles(core);
                //
                // -- Addon folder
                AddonFolderClass.executeHourlyTasks(core);
                //
                // -- metadata
                ContentFieldClass.executeHourlyTasks(core);
                //
                // -- content
                MenuEntryClass.executeHourlyTasks(core);
                RemoteQueryClass.executeHourlyTasks(core);
                PageContentClass.executeHourlyTasks(core);
                AddonContentFieldTypeRuleClass.executeHourlyTasks(core);
                AddonContentTriggerRuleClass.executeHourlyTasks(core);
                ContentWatchClass.executeHourlyTasks(core);
                EmailDropClass.executeHourlyTasks(core);
                EmailLogClass.executeHourlyTasks(core);
                FieldHelpClass.executeHourlyTasks(core);
                GroupRulesClass.executeHourlyTasks(core);
                MemberRuleClass.executeHourlyTasks(core);
                MetadataClass.executeHourlyTasks(core);
                LinkAliasClass.executeHourlyTasks(core);
                //
                // -- Properties
                UserProperyClass.executeHourlyTasks(core);
                VisitPropertyClass.executeHourlyTasks(core);
                VisitorPropertyClass.executeHourlyTasks(core);
                //
                // -- visits, visitors, viewings
                VisitClass.executeHourlyTasks(core);
                VisitorClass.executeHourlyTasks(core);
                ViewingsClass.executeHourlyTasks(core);
                //
                // -- summary
                VisitSummaryClass.executeHourlyTasks(core);
                ViewingSummaryClass.executeHourlyTasks(core);
                //
                // -- logs
                ActivityLogClass.executeHourlyTasks(core);
                //
                // -- people
                PersonClass.executeHourlyTasks(core);
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
    }
}