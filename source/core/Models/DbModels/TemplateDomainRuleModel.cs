
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
    public class TemplateDomainRuleModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Template Domain Rules";
        public const string contentTableName = "ccDomainTemplateRules";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int domainId { get; set; }
        public int templateId { get; set; }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel add(coreController cpCore) {
            return add<TemplateDomainRuleModel>(cpCore);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<TemplateDomainRuleModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel create(coreController cpCore, int recordId) {
            return create<TemplateDomainRuleModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<TemplateDomainRuleModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel create(coreController cpCore, string recordGuid) {
            return create<TemplateDomainRuleModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<TemplateDomainRuleModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel createByName(coreController cpCore, string recordName) {
            return createByName<TemplateDomainRuleModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<TemplateDomainRuleModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<TemplateDomainRuleModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<TemplateDomainRuleModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<TemplateDomainRuleModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<TemplateDomainRuleModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<TemplateDomainRuleModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<TemplateDomainRuleModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<TemplateDomainRuleModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<TemplateDomainRuleModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<TemplateDomainRuleModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<TemplateDomainRuleModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<TemplateDomainRuleModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<TemplateDomainRuleModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel createDefault(coreController cpcore) {
            return createDefault<TemplateDomainRuleModel>(cpcore);
        }
    }
}
