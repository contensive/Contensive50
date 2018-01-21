﻿
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
    public class layoutModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "layouts";
        public const string contentTableName = "ccLayouts";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public string Layout { get; set; }
        public string StylesFilename { get; set; }
        //
        //====================================================================================================
        public static layoutModel add(coreController cpCore) {
            return add<layoutModel>(cpCore);
        }
        //
        //====================================================================================================
        public static layoutModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<layoutModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static layoutModel create(coreController cpCore, int recordId) {
            return create<layoutModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static layoutModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<layoutModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static layoutModel create(coreController cpCore, string recordGuid) {
            return create<layoutModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static layoutModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<layoutModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static layoutModel createByName(coreController cpCore, string recordName) {
            return createByName<layoutModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static layoutModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<layoutModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<layoutModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<layoutModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<layoutModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<layoutModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<layoutModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<layoutModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<layoutModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<layoutModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<layoutModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<layoutModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<layoutModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<layoutModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static layoutModel createDefault(coreController cpcore) {
            return createDefault<layoutModel>(cpcore);
        }
    }
}
