
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
        public static sortMethodModel add(coreController core) {
            return add<sortMethodModel>(core);
        }
        //
        //====================================================================================================
        public static sortMethodModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<sortMethodModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static sortMethodModel create(coreController core, int recordId) {
            return create<sortMethodModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static sortMethodModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<sortMethodModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static sortMethodModel create(coreController core, string recordGuid) {
            return create<sortMethodModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static sortMethodModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<sortMethodModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static sortMethodModel createByName(coreController core, string recordName) {
            return createByName<sortMethodModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static sortMethodModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<sortMethodModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<sortMethodModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<sortMethodModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<sortMethodModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<sortMethodModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<sortMethodModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<sortMethodModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<sortMethodModel> createList(coreController core, string sqlCriteria) {
            return createList<sortMethodModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidateCacheSingleRecord(coreController core, int recordId) {
            invalidateCacheSingleRecord<sortMethodModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<sortMethodModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<sortMethodModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<sortMethodModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static sortMethodModel createDefault(coreController core) {
            return createDefault<sortMethodModel>(core);
        }
    }
}
