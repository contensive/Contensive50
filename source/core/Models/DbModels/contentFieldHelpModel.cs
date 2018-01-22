
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
        public static ContentFieldHelpModel add(coreController core) {
            return add<ContentFieldHelpModel>(core);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<ContentFieldHelpModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel create(coreController core, int recordId) {
            return create<ContentFieldHelpModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<ContentFieldHelpModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel create(coreController core, string recordGuid) {
            return create<ContentFieldHelpModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<ContentFieldHelpModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel createByName(coreController core, string recordName) {
            return createByName<ContentFieldHelpModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<ContentFieldHelpModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<ContentFieldHelpModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<ContentFieldHelpModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<ContentFieldHelpModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<ContentFieldHelpModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<ContentFieldHelpModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<ContentFieldHelpModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<ContentFieldHelpModel> createList(coreController core, string sqlCriteria) {
            return createList<ContentFieldHelpModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<ContentFieldHelpModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<ContentFieldHelpModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<ContentFieldHelpModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<ContentFieldHelpModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static ContentFieldHelpModel createDefault(coreController core) {
            return createDefault<ContentFieldHelpModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// get the first field help for a field, digard the rest
        /// </summary>
        /// <param name="core"></param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        public static ContentFieldHelpModel createByFieldId(coreController core, int fieldId) {
            List<ContentFieldHelpModel> helpList = createList(core, "(fieldId=" + fieldId + ")", "id");
            if (helpList.Count == 0) {
                return null;
            } else {
                return helpList.First();
            }
        }
    }
}
