
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
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
namespace Contensive.Processor.Models.Db {
    public class TemplateDomainRuleModel : DbModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Template Domain Rules";
        public const string contentTableName = "ccDomainTemplateRules";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public int domainId { get; set; }
        public int templateId { get; set; }
        // 
        //====================================================================================================
        public static TemplateDomainRuleModel addEmpty(CoreController core) {
            return addEmpty<TemplateDomainRuleModel>(core);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel addDefault(CoreController core, Domain.MetaModel metaData) {
            return addDefault<TemplateDomainRuleModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) {
            return addDefault<TemplateDomainRuleModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel create(CoreController core, int recordId) {
            return create<TemplateDomainRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<TemplateDomainRuleModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel create(CoreController core, string recordGuid) {
            return create<TemplateDomainRuleModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<TemplateDomainRuleModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<TemplateDomainRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static TemplateDomainRuleModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<TemplateDomainRuleModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<TemplateDomainRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<TemplateDomainRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<TemplateDomainRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<TemplateDomainRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<TemplateDomainRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<TemplateDomainRuleModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<TemplateDomainRuleModel> createList(CoreController core, string sqlCriteria) {
            return createList<TemplateDomainRuleModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<TemplateDomainRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return DbModel.getRecordName<TemplateDomainRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return DbModel.getRecordName<TemplateDomainRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return DbModel.getRecordId<TemplateDomainRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<TemplateDomainRuleModel>(core);
        }
    }
}
