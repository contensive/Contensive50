
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
    public class linkForwardModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "link forwards";
        public const string contentTableName = "cclinkforwards";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public string DestinationLink;
        public int GroupID;
        public string SourceLink;
        public int Viewings;
        //
        //====================================================================================================
        public static linkForwardModel add(coreController cpCore) {
            return add<linkForwardModel>(cpCore);
        }
        //
        //====================================================================================================
        public static linkForwardModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<linkForwardModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static linkForwardModel create(coreController cpCore, int recordId) {
            return create<linkForwardModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static linkForwardModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<linkForwardModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static linkForwardModel create(coreController cpCore, string recordGuid) {
            return create<linkForwardModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static linkForwardModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<linkForwardModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static linkForwardModel createByName(coreController cpCore, string recordName) {
            return createByName<linkForwardModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static linkForwardModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<linkForwardModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<linkForwardModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<linkForwardModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<linkForwardModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<linkForwardModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<linkForwardModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<linkForwardModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<linkForwardModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<linkForwardModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<linkForwardModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<linkForwardModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<linkForwardModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<linkForwardModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static linkForwardModel createDefault(coreController cpcore) {
            return createDefault<linkForwardModel>(cpcore);
        }
    }
}
