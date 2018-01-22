
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
        public static addonContentFieldTypeRulesModel add(coreController core) {
            return add<addonContentFieldTypeRulesModel>(core);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<addonContentFieldTypeRulesModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel create(coreController core, int recordId) {
            return create<addonContentFieldTypeRulesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<addonContentFieldTypeRulesModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel create(coreController core, string recordGuid) {
            return create<addonContentFieldTypeRulesModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<addonContentFieldTypeRulesModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel createByName(coreController core, string recordName) {
            return createByName<addonContentFieldTypeRulesModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<addonContentFieldTypeRulesModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<addonContentFieldTypeRulesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<addonContentFieldTypeRulesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<addonContentFieldTypeRulesModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<addonContentFieldTypeRulesModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<addonContentFieldTypeRulesModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<addonContentFieldTypeRulesModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<addonContentFieldTypeRulesModel> createList(coreController core, string sqlCriteria) {
            return createList<addonContentFieldTypeRulesModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<addonContentFieldTypeRulesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<addonContentFieldTypeRulesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<addonContentFieldTypeRulesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<addonContentFieldTypeRulesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel createDefault(coreController core) {
            return createDefault<addonContentFieldTypeRulesModel>(core);
        }
    }
}
