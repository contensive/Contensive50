
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
        public static linkAliasModel add(coreController cpCore) {
            return add<linkAliasModel>(cpCore);
        }
        //
        //====================================================================================================
        public static linkAliasModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<linkAliasModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static linkAliasModel create(coreController cpCore, int recordId) {
            return create<linkAliasModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static linkAliasModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<linkAliasModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static linkAliasModel create(coreController cpCore, string recordGuid) {
            return create<linkAliasModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static linkAliasModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<linkAliasModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static linkAliasModel createByName(coreController cpCore, string recordName) {
            return createByName<linkAliasModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static linkAliasModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<linkAliasModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<linkAliasModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<linkAliasModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<linkAliasModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<linkAliasModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<linkAliasModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<linkAliasModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<linkAliasModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<linkAliasModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<linkAliasModel>(cpCore, recordId);
            Models.Complex.routeDictionaryModel.invalidateCache(cpCore);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<linkAliasModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<linkAliasModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<linkAliasModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static linkAliasModel createDefault(coreController cpcore) {
            return createDefault<linkAliasModel>(cpcore);
        }
        //
        //====================================================================================================
        public static List<linkAliasModel> createList(coreController cpCore, int pageId, string queryStringSuffix) {
            if (string.IsNullOrEmpty(queryStringSuffix)) {
                return createList<linkAliasModel>(cpCore, "(pageId=" + pageId + ")", "id desc");
            } else {
                return createList<linkAliasModel>(cpCore, "(pageId=" + pageId + ")and(QueryStringSuffix=" + cpCore.db.encodeSQLText(queryStringSuffix) + ")", "id desc");
            }
        }
    }
}
