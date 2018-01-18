
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
namespace Contensive.Core.Models.DbModels {
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
        public static addonIncludeRuleModel add(coreClass cpCore) {
            return add<addonIncludeRuleModel>(cpCore);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel add(coreClass cpCore, ref List<string> callersCacheNameList) {
            return add<addonIncludeRuleModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel create(coreClass cpCore, int recordId) {
            return create<addonIncludeRuleModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel create(coreClass cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<addonIncludeRuleModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel create(coreClass cpCore, string recordGuid) {
            return create<addonIncludeRuleModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel create(coreClass cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<addonIncludeRuleModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel createByName(coreClass cpCore, string recordName) {
            return createByName<addonIncludeRuleModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel createByName(coreClass cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<addonIncludeRuleModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreClass cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreClass cpCore, int recordId) {
            delete<addonIncludeRuleModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreClass cpCore, string ccGuid) {
            delete<addonIncludeRuleModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<addonIncludeRuleModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<addonIncludeRuleModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<addonIncludeRuleModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<addonIncludeRuleModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<addonIncludeRuleModel> createList(coreClass cpCore, string sqlCriteria) {
            return createList<addonIncludeRuleModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreClass cpCore, int recordId) {
            invalidateCacheSingleRecord<addonIncludeRuleModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreClass cpcore, int recordId) {
            return baseModel.getRecordName<addonIncludeRuleModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreClass cpcore, string ccGuid) {
            return baseModel.getRecordName<addonIncludeRuleModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreClass cpcore, string ccGuid) {
            return baseModel.getRecordId<addonIncludeRuleModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static addonIncludeRuleModel createDefault(coreClass cpcore) {
            return createDefault<addonIncludeRuleModel>(cpcore);
        }
    }
}
