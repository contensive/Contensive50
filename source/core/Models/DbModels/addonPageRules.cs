
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
    public class addonPageRuleModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "add-on page rules";
        public const string contentTableName = "ccAddonPageRules";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int addonId { get; set; }
        public int pageId { get; set; }
        //
        //====================================================================================================
        public static addonPageRuleModel add(coreController cpCore) {
            return add<addonPageRuleModel>(cpCore);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<addonPageRuleModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel create(coreController cpCore, int recordId) {
            return create<addonPageRuleModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<addonPageRuleModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel create(coreController cpCore, string recordGuid) {
            return create<addonPageRuleModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<addonPageRuleModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel createByName(coreController cpCore, string recordName) {
            return createByName<addonPageRuleModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<addonPageRuleModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<addonPageRuleModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<addonPageRuleModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<addonPageRuleModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<addonPageRuleModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<addonPageRuleModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<addonPageRuleModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<addonPageRuleModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<addonPageRuleModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<addonPageRuleModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<addonPageRuleModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<addonPageRuleModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<addonPageRuleModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static addonPageRuleModel createDefault(coreController cpcore) {
            return createDefault<addonPageRuleModel>(cpcore);
        }
    }
}
