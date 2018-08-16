
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
    public class linkAliasModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "link aliases";
        public const string contentTableName = "cclinkaliases";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        //Public Property Link As String
        public int PageID { get; set; }
        public string QueryStringSuffix { get; set; }
        //
        //====================================================================================================
        public static linkAliasModel add(CoreController core) {
            return add<linkAliasModel>(core);
        }
        //
        //====================================================================================================
        public static linkAliasModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<linkAliasModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static linkAliasModel create(CoreController core, int recordId) {
            return create<linkAliasModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static linkAliasModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<linkAliasModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static linkAliasModel create(CoreController core, string recordGuid) {
            return create<linkAliasModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static linkAliasModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<linkAliasModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static linkAliasModel createByName(CoreController core, string recordName) {
            return createByName<linkAliasModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static linkAliasModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<linkAliasModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<linkAliasModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<linkAliasModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<linkAliasModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<linkAliasModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<linkAliasModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<linkAliasModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<linkAliasModel> createList(CoreController core, string sqlCriteria) {
            return createList<linkAliasModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateCache(CoreController core, int recordId) {
            invalidateCache<linkAliasModel>(core, recordId);
            Models.Complex.routeDictionaryModel.invalidateCache(core);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<linkAliasModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<linkAliasModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<linkAliasModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static linkAliasModel createDefault(CoreController core) {
            return createDefault<linkAliasModel>(core);
        }
        //
        //====================================================================================================
        public static List<linkAliasModel> createList(CoreController core, int pageId, string queryStringSuffix) {
            if (string.IsNullOrEmpty(queryStringSuffix)) {
                return createList<linkAliasModel>(core, "(pageId=" + pageId + ")", "id desc");
            } else {
                return createList<linkAliasModel>(core, "(pageId=" + pageId + ")and(QueryStringSuffix=" + core.db.encodeSQLText(queryStringSuffix) + ")", "id desc");
            }
        }
    }
}
