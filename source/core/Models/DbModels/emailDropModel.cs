
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
    public class emailDropModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Email Drops";
        public const string contentTableName = "ccEmailDrops";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int EmailID { get; set; }
        //
        //====================================================================================================
        public static emailDropModel add(CoreController core) {
            return add<emailDropModel>(core);
        }
        //
        //====================================================================================================
        public static emailDropModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<emailDropModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailDropModel create(CoreController core, int recordId) {
            return create<emailDropModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static emailDropModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<emailDropModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailDropModel create(CoreController core, string recordGuid) {
            return create<emailDropModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static emailDropModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<emailDropModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailDropModel createByName(CoreController core, string recordName) {
            return createByName<emailDropModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static emailDropModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<emailDropModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<emailDropModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<emailDropModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<emailDropModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<emailDropModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<emailDropModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<emailDropModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<emailDropModel> createList(CoreController core, string sqlCriteria) {
            return createList<emailDropModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCacheSingleRecord<emailDropModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<emailDropModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<emailDropModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<emailDropModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static emailDropModel createDefault(CoreController core) {
            return createDefault<emailDropModel>(core);
        }
    }
}
