
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
        public static languageModel add(coreController cpCore) {
            return add<languageModel>(cpCore);
        }
        //
        //====================================================================================================
        public static languageModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<languageModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static languageModel create(coreController cpCore, int recordId) {
            return create<languageModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static languageModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<languageModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static languageModel create(coreController cpCore, string recordGuid) {
            return create<languageModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static languageModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<languageModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static languageModel createByName(coreController cpCore, string recordName) {
            return createByName<languageModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static languageModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<languageModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<languageModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<languageModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<languageModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<languageModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<languageModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<languageModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<languageModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<languageModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<languageModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<languageModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<languageModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<languageModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static languageModel createDefault(coreController cpcore) {
            return createDefault<languageModel>(cpcore);
        }
    }
}
