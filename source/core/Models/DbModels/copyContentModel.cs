
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
    public class copyContentModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "copy content";
        public const string contentTableName = "cccopy";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public string copy { get; set; }
        //
        //====================================================================================================
        public static copyContentModel add(coreController core) {
            return add<copyContentModel>(core);
        }
        //
        //====================================================================================================
        public static copyContentModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<copyContentModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static copyContentModel create(coreController core, int recordId) {
            return create<copyContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static copyContentModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<copyContentModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static copyContentModel create(coreController core, string recordGuid) {
            return create<copyContentModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static copyContentModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<copyContentModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static copyContentModel createByName(coreController core, string recordName) {
            return createByName<copyContentModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static copyContentModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<copyContentModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<copyContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<copyContentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<copyContentModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<copyContentModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<copyContentModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<copyContentModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<copyContentModel> createList(coreController core, string sqlCriteria) {
            return createList<copyContentModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<copyContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<copyContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<copyContentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<copyContentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static copyContentModel createDefault(coreController core) {
            return createDefault<copyContentModel>(core);
        }
    }
}
