
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
        public static copyContentModel add(coreController cpCore) {
            return add<copyContentModel>(cpCore);
        }
        //
        //====================================================================================================
        public static copyContentModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<copyContentModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static copyContentModel create(coreController cpCore, int recordId) {
            return create<copyContentModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static copyContentModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<copyContentModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static copyContentModel create(coreController cpCore, string recordGuid) {
            return create<copyContentModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static copyContentModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<copyContentModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static copyContentModel createByName(coreController cpCore, string recordName) {
            return createByName<copyContentModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static copyContentModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<copyContentModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<copyContentModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<copyContentModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<copyContentModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<copyContentModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<copyContentModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<copyContentModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<copyContentModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<copyContentModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<copyContentModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<copyContentModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<copyContentModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<copyContentModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static copyContentModel createDefault(coreController cpcore) {
            return createDefault<copyContentModel>(cpcore);
        }
    }
}
