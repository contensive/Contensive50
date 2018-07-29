
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
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.DbModels {
    public class addonIncludeRuleModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Add-on Include Rules";
        public const string contentTableName = "ccAddonIncludeRules";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int AddonID { get; set; }
        public int IncludedAddonID { get; set; }
        //
        //====================================================================================================
        public static addonIncludeRuleModel add(CoreController core) {
            return add<addonIncludeRuleModel>(core);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<addonIncludeRuleModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel create(CoreController core, int recordId) {
            return create<addonIncludeRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<addonIncludeRuleModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel create(CoreController core, string recordGuid) {
            return create<addonIncludeRuleModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<addonIncludeRuleModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel createByName(CoreController core, string recordName) {
            return createByName<addonIncludeRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<addonIncludeRuleModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<addonIncludeRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<addonIncludeRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<addonIncludeRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<addonIncludeRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<addonIncludeRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<addonIncludeRuleModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<addonIncludeRuleModel> createList(CoreController core, string sqlCriteria) {
            return createList<addonIncludeRuleModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCacheSingleRecord<addonIncludeRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<addonIncludeRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<addonIncludeRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<addonIncludeRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel createDefault(CoreController core) {
            return createDefault<addonIncludeRuleModel>(core);
        }
    }
}
