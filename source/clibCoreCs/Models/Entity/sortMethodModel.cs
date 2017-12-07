
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
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Models.Entity {
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
        public static sortMethodModel add(coreClass cpCore) {
            return add<sortMethodModel>(cpCore);
        }
        //
        //====================================================================================================
        public static sortMethodModel add(coreClass cpCore, ref List<string> callersCacheNameList) {
            return add<sortMethodModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static sortMethodModel create(coreClass cpCore, int recordId) {
            return create<sortMethodModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static sortMethodModel create(coreClass cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<sortMethodModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static sortMethodModel create(coreClass cpCore, string recordGuid) {
            return create<sortMethodModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static sortMethodModel create(coreClass cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<sortMethodModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static sortMethodModel createByName(coreClass cpCore, string recordName) {
            return createByName<sortMethodModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static sortMethodModel createByName(coreClass cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<sortMethodModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public void save(coreClass cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreClass cpCore, int recordId) {
            delete<sortMethodModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreClass cpCore, string ccGuid) {
            delete<sortMethodModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<sortMethodModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<sortMethodModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<sortMethodModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<sortMethodModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<sortMethodModel> createList(coreClass cpCore, string sqlCriteria) {
            return createList<sortMethodModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidateCacheSingleRecord(coreClass cpCore, int recordId) {
            invalidateCacheSingleRecord<sortMethodModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreClass cpcore, int recordId) {
            return baseModel.getRecordName<sortMethodModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreClass cpcore, string ccGuid) {
            return baseModel.getRecordName<sortMethodModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreClass cpcore, string ccGuid) {
            return baseModel.getRecordId<sortMethodModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static sortMethodModel createDefault(coreClass cpcore) {
            return createDefault<sortMethodModel>(cpcore);
        }
    }
}
