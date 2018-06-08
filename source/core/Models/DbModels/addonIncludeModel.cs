
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
        public static addonIncludeRuleModel add(coreController core) {
            return add<addonIncludeRuleModel>(core);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<addonIncludeRuleModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel create(coreController core, int recordId) {
            return create<addonIncludeRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<addonIncludeRuleModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel create(coreController core, string recordGuid) {
            return create<addonIncludeRuleModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<addonIncludeRuleModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel createByName(coreController core, string recordName) {
            return createByName<addonIncludeRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<addonIncludeRuleModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<addonIncludeRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<addonIncludeRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<addonIncludeRuleModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<addonIncludeRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<addonIncludeRuleModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<addonIncludeRuleModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<addonIncludeRuleModel> createList(coreController core, string sqlCriteria) {
            return createList<addonIncludeRuleModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<addonIncludeRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<addonIncludeRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<addonIncludeRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<addonIncludeRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel createDefault(coreController core) {
            return createDefault<addonIncludeRuleModel>(core);
        }
    }
}
