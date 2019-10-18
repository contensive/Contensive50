﻿
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using System;
using Contensive.Processor.Models.Domain;

namespace Contensive.Addons.Housekeeping {
    //
    public static class DailyTasksClass {
        //====================================================================================================
        //
        public static void housekeep(CoreController core, HouseKeepEnvironmentModel env) {
            try {
                //
                // -- Download Updates
                SoftwareUpdatesClass.DownloadAndInstall(core);
                //
                // -- Addons
                AddonFolderClass.housekeep(core);
                //
                // -- content
                PageContentClass.housekeep(core, env);
                AddonContentFieldTypeRuleClass.housekeep(core, env);
                AddonContentTriggerRuleClass.housekeep(core, env);
                ContentWatchClass.housekeep(core, env);
                EmailDropClass.housekeep(core, env);
                EmailLogClass.housekeep(core, env);
                FieldHelpClass.housekeep(core, env);
                GroupRulesClass.housekeep(core, env);
                MemberRuleClass.housekeep(core, env);
                MetadataClass.housekeep(core, env);
                LinkAliasClass.housekeep(core, env);
                //
                // -- Properties
                UserProperyClass.housekeep(core);
                VisitPropertyClass.housekeep(core);
                VisitorPropertyClass.housekeep(core);
                //
                // -- visits, visitors, viewings
                VisitSummaryClass.summarizeDaily(core, env);
                VisitClass.housekeep(core, env);
                VisitorClass.housekeep(core, env);
                ViewingsClass.housekeep(core, env);
                //
                // -- logs
                ActivityLogClass.housekeep(core, env);
                //
                // -- people
                PersonClass.housekeep(core, env);
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
    }
}