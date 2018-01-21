
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
    public class sortMethodModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "sort methods";
        public const string contentTableName = "ccSortMethods";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public string OrderByClause { get; set; }
        //
        //====================================================================================================
        public static sortMethodModel add(coreController cpCore) {
            return add<sortMethodModel>(cpCore);
        }
        //
        //====================================================================================================
        public static sortMethodModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<sortMethodModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static sortMethodModel create(coreController cpCore, int recordId) {
            return create<sortMethodModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static sortMethodModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<sortMethodModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static sortMethodModel create(coreController cpCore, string recordGuid) {
            return create<sortMethodModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static sortMethodModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<sortMethodModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static sortMethodModel createByName(coreController cpCore, string recordName) {
            return createByName<sortMethodModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static sortMethodModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<sortMethodModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<sortMethodModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<sortMethodModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<sortMethodModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<sortMethodModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<sortMethodModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<sortMethodModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<sortMethodModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<sortMethodModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidateCacheSingleRecord(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<sortMethodModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<sortMethodModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<sortMethodModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<sortMethodModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static sortMethodModel createDefault(coreController cpcore) {
            return createDefault<sortMethodModel>(cpcore);
        }
    }
}
