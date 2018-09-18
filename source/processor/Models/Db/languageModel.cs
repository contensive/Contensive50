
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
    public class languageModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "languages";
        public const string contentTableName = "cclanguages";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public string HTTP_Accept_Language { get; set; }
        //
        //====================================================================================================
        public static languageModel add(CoreController core) {
            return add<languageModel>(core);
        }
        //
        //====================================================================================================
        public static languageModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<languageModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static languageModel create(CoreController core, int recordId) {
            return create<languageModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static languageModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<languageModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static languageModel create(CoreController core, string recordGuid) {
            return create<languageModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static languageModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<languageModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static languageModel createByName(CoreController core, string recordName) {
            return createByName<languageModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static languageModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<languageModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<languageModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<languageModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<languageModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<languageModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<languageModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<languageModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<languageModel> createList(CoreController core, string sqlCriteria) {
            return createList<languageModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<languageModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<languageModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<languageModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<languageModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static languageModel createDefault(CoreController core) {
            return createDefault<languageModel>(core);
        }
    }
}
