
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
        public static pageTemplateModel add(coreController core) {
            return add<pageTemplateModel>(core);
        }
        //
        //====================================================================================================
        public static pageTemplateModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<pageTemplateModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static pageTemplateModel create(coreController core, int recordId) {
            return create<pageTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static pageTemplateModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<pageTemplateModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static pageTemplateModel create(coreController core, string recordGuid) {
            return create<pageTemplateModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static pageTemplateModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<pageTemplateModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static pageTemplateModel createByName(coreController core, string recordName) {
            return createByName<pageTemplateModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static pageTemplateModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<pageTemplateModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<pageTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<pageTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<pageTemplateModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<pageTemplateModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<pageTemplateModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<pageTemplateModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<pageTemplateModel> createList(coreController core, string sqlCriteria) {
            return createList<pageTemplateModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<pageTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<pageTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<pageTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<pageTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static pageTemplateModel createDefault(coreController core) {
            return createDefault<pageTemplateModel>(core);
        }
    }
}
