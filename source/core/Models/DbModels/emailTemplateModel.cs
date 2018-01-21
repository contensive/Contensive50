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
    public class emailTemplateModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "email templates";
        public const string contentTableName = "ccemailtemplates";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public string BodyHTML { get; set; }
        public string Source { get; set; }
        //
        //====================================================================================================
        public static emailTemplateModel add(coreController cpCore) {
            return add<emailTemplateModel>(cpCore);
        }
        //
        //====================================================================================================
        public static emailTemplateModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<emailTemplateModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailTemplateModel create(coreController cpCore, int recordId) {
            return create<emailTemplateModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static emailTemplateModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<emailTemplateModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailTemplateModel create(coreController cpCore, string recordGuid) {
            return create<emailTemplateModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static emailTemplateModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<emailTemplateModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailTemplateModel createByName(coreController cpCore, string recordName) {
            return createByName<emailTemplateModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static emailTemplateModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<emailTemplateModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<emailTemplateModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<emailTemplateModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<emailTemplateModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<emailTemplateModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<emailTemplateModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<emailTemplateModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<emailTemplateModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<emailTemplateModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<emailTemplateModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<emailTemplateModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<emailTemplateModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<emailTemplateModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static emailTemplateModel createDefault(coreController cpcore) {
            return createDefault<emailTemplateModel>(cpcore);
        }
    }
}
