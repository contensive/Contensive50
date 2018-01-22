
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
        public static linkForwardModel add(coreController core) {
            return add<linkForwardModel>(core);
        }
        //
        //====================================================================================================
        public static linkForwardModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<linkForwardModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static linkForwardModel create(coreController core, int recordId) {
            return create<linkForwardModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static linkForwardModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<linkForwardModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static linkForwardModel create(coreController core, string recordGuid) {
            return create<linkForwardModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static linkForwardModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<linkForwardModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static linkForwardModel createByName(coreController core, string recordName) {
            return createByName<linkForwardModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static linkForwardModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<linkForwardModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<linkForwardModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<linkForwardModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<linkForwardModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<linkForwardModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<linkForwardModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<linkForwardModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<linkForwardModel> createList(coreController core, string sqlCriteria) {
            return createList<linkForwardModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<linkForwardModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<linkForwardModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<linkForwardModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<linkForwardModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static linkForwardModel createDefault(coreController core) {
            return createDefault<linkForwardModel>(core);
        }
    }
}
