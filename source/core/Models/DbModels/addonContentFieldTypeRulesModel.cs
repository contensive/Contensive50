
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
    public class addonContentFieldTypeRulesModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "add-on Content Field Type Rules";
        public const string contentTableName = "ccAddonContentFieldTypeRules";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int addonID { get; set; }
        public int contentFieldTypeID { get; set; }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel add(coreClass cpCore) {
            return add<addonContentFieldTypeRulesModel>(cpCore);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel add(coreClass cpCore, ref List<string> callersCacheNameList) {
            return add<addonContentFieldTypeRulesModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel create(coreClass cpCore, int recordId) {
            return create<addonContentFieldTypeRulesModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel create(coreClass cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<addonContentFieldTypeRulesModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel create(coreClass cpCore, string recordGuid) {
            return create<addonContentFieldTypeRulesModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel create(coreClass cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<addonContentFieldTypeRulesModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel createByName(coreClass cpCore, string recordName) {
            return createByName<addonContentFieldTypeRulesModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel createByName(coreClass cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<addonContentFieldTypeRulesModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreClass cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreClass cpCore, int recordId) {
            delete<addonContentFieldTypeRulesModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreClass cpCore, string ccGuid) {
            delete<addonContentFieldTypeRulesModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<addonContentFieldTypeRulesModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<addonContentFieldTypeRulesModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<addonContentFieldTypeRulesModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<addonContentFieldTypeRulesModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<addonContentFieldTypeRulesModel> createList(coreClass cpCore, string sqlCriteria) {
            return createList<addonContentFieldTypeRulesModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreClass cpCore, int recordId) {
            invalidateCacheSingleRecord<addonContentFieldTypeRulesModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreClass cpcore, int recordId) {
            return baseModel.getRecordName<addonContentFieldTypeRulesModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreClass cpcore, string ccGuid) {
            return baseModel.getRecordName<addonContentFieldTypeRulesModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreClass cpcore, string ccGuid) {
            return baseModel.getRecordId<addonContentFieldTypeRulesModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel createDefault(coreClass cpcore) {
            return createDefault<addonContentFieldTypeRulesModel>(cpcore);
        }
    }
}
