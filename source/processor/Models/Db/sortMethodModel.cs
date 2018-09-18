
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
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.Db {
    public class sortMethodModel : BaseModel {
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
        public static sortMethodModel add(CoreController core) {
            return add<sortMethodModel>(core);
        }
        //
        //====================================================================================================
        public static sortMethodModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<sortMethodModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static sortMethodModel create(CoreController core, int recordId) {
            return create<sortMethodModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static sortMethodModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<sortMethodModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static sortMethodModel create(CoreController core, string recordGuid) {
            return create<sortMethodModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static sortMethodModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<sortMethodModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static sortMethodModel createByName(CoreController core, string recordName) {
            return createByName<sortMethodModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static sortMethodModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<sortMethodModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<sortMethodModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<sortMethodModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<sortMethodModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<sortMethodModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<sortMethodModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<sortMethodModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<sortMethodModel> createList(CoreController core, string sqlCriteria) {
            return createList<sortMethodModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidateCacheSingleRecord(CoreController core, int recordId) {
            invalidateCache<sortMethodModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<sortMethodModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<sortMethodModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<sortMethodModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static sortMethodModel createDefault(CoreController core) {
            return createDefault<sortMethodModel>(core);
        }
    }
}
