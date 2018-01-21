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
    public class ContentFieldHelpModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "content field help";
        public const string contentTableName = "ccFieldHelp";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int FieldID { get; set; }
        public string HelpCustom { get; set; }
        public string HelpDefault { get; set; }
        //
        //====================================================================================================
        public static ContentFieldHelpModel add(coreController cpCore) {
            return add<ContentFieldHelpModel>(cpCore);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<ContentFieldHelpModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel create(coreController cpCore, int recordId) {
            return create<ContentFieldHelpModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<ContentFieldHelpModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel create(coreController cpCore, string recordGuid) {
            return create<ContentFieldHelpModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<ContentFieldHelpModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel createByName(coreController cpCore, string recordName) {
            return createByName<ContentFieldHelpModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<ContentFieldHelpModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<ContentFieldHelpModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<ContentFieldHelpModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<ContentFieldHelpModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<ContentFieldHelpModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<ContentFieldHelpModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<ContentFieldHelpModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<ContentFieldHelpModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<ContentFieldHelpModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<ContentFieldHelpModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<ContentFieldHelpModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<ContentFieldHelpModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<ContentFieldHelpModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel createDefault(coreController cpcore) {
            return createDefault<ContentFieldHelpModel>(cpcore);
        }
        //
        //====================================================================================================
        /// <summary>
        /// get the first field help for a field, digard the rest
        /// </summary>
        /// <param name="cpCore"></param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        public static ContentFieldHelpModel createByFieldId(coreController cpCore, int fieldId) {
            List<ContentFieldHelpModel> helpList = createList(cpCore, "(fieldId=" + fieldId + ")", "id");
            if (helpList.Count == 0) {
                return null;
            } else {
                return helpList.First();
            }
        }
    }
}
