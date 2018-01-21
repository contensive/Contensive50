
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
    public class pageTemplateModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "page templates";
        public const string contentTableName = "cctemplates";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public string bodyHTML { get; set; }
        // Public Property BodyTag As String
        public bool isSecure { get; set; }
        // Public Property JSEndBody As String
        // Public Property JSFilename As String
        // Public Property JSHead As String
        // Public Property JSOnLoad As String
        // Public Property Link As String
        // Public Property MobileBodyHTML As String
        // Public Property OtherHeadTags As String
        //
        //====================================================================================================
        public static pageTemplateModel add(coreController cpCore) {
            return add<pageTemplateModel>(cpCore);
        }
        //
        //====================================================================================================
        public static pageTemplateModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<pageTemplateModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static pageTemplateModel create(coreController cpCore, int recordId) {
            return create<pageTemplateModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static pageTemplateModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<pageTemplateModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static pageTemplateModel create(coreController cpCore, string recordGuid) {
            return create<pageTemplateModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static pageTemplateModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<pageTemplateModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static pageTemplateModel createByName(coreController cpCore, string recordName) {
            return createByName<pageTemplateModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static pageTemplateModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<pageTemplateModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<pageTemplateModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<pageTemplateModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<pageTemplateModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<pageTemplateModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<pageTemplateModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<pageTemplateModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<pageTemplateModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<pageTemplateModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<pageTemplateModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<pageTemplateModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<pageTemplateModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<pageTemplateModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static pageTemplateModel createDefault(coreController cpcore) {
            return createDefault<pageTemplateModel>(cpcore);
        }
    }
}
