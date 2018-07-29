
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
        public static addonContentFieldTypeRulesModel add(CoreController core) {
            return add<addonContentFieldTypeRulesModel>(core);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<addonContentFieldTypeRulesModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel create(CoreController core, int recordId) {
            return create<addonContentFieldTypeRulesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<addonContentFieldTypeRulesModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel create(CoreController core, string recordGuid) {
            return create<addonContentFieldTypeRulesModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<addonContentFieldTypeRulesModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel createByName(CoreController core, string recordName) {
            return createByName<addonContentFieldTypeRulesModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<addonContentFieldTypeRulesModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<addonContentFieldTypeRulesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<addonContentFieldTypeRulesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<addonContentFieldTypeRulesModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<addonContentFieldTypeRulesModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<addonContentFieldTypeRulesModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<addonContentFieldTypeRulesModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<addonContentFieldTypeRulesModel> createList(CoreController core, string sqlCriteria) {
            return createList<addonContentFieldTypeRulesModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCacheSingleRecord<addonContentFieldTypeRulesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<addonContentFieldTypeRulesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<addonContentFieldTypeRulesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<addonContentFieldTypeRulesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static addonContentFieldTypeRulesModel createDefault(CoreController core) {
            return createDefault<addonContentFieldTypeRulesModel>(core);
        }
    }
}
