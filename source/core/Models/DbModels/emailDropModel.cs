
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
        public static emailDropModel add(coreController core) {
            return add<emailDropModel>(core);
        }
        //
        //====================================================================================================
        public static emailDropModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<emailDropModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailDropModel create(coreController core, int recordId) {
            return create<emailDropModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static emailDropModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<emailDropModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailDropModel create(coreController core, string recordGuid) {
            return create<emailDropModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static emailDropModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<emailDropModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailDropModel createByName(coreController core, string recordName) {
            return createByName<emailDropModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static emailDropModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<emailDropModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<emailDropModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<emailDropModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<emailDropModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<emailDropModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<emailDropModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<emailDropModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<emailDropModel> createList(coreController core, string sqlCriteria) {
            return createList<emailDropModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<emailDropModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<emailDropModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<emailDropModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<emailDropModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static emailDropModel createDefault(coreController core) {
            return createDefault<emailDropModel>(core);
        }
    }
}
