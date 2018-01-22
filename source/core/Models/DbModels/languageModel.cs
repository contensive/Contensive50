
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
    public class languageModel : baseModel {
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
        public static languageModel add(coreController core) {
            return add<languageModel>(core);
        }
        //
        //====================================================================================================
        public static languageModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<languageModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static languageModel create(coreController core, int recordId) {
            return create<languageModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static languageModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<languageModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static languageModel create(coreController core, string recordGuid) {
            return create<languageModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static languageModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<languageModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static languageModel createByName(coreController core, string recordName) {
            return createByName<languageModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static languageModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<languageModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<languageModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<languageModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<languageModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<languageModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<languageModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<languageModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<languageModel> createList(coreController core, string sqlCriteria) {
            return createList<languageModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<languageModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<languageModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<languageModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<languageModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static languageModel createDefault(coreController core) {
            return createDefault<languageModel>(core);
        }
    }
}
