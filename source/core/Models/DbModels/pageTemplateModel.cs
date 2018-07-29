
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.DbModels {
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
        public static pageTemplateModel add(CoreController core) {
            return add<pageTemplateModel>(core);
        }
        //
        //====================================================================================================
        public static pageTemplateModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<pageTemplateModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static pageTemplateModel create(CoreController core, int recordId) {
            return create<pageTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static pageTemplateModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<pageTemplateModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static pageTemplateModel create(CoreController core, string recordGuid) {
            return create<pageTemplateModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static pageTemplateModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<pageTemplateModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static pageTemplateModel createByName(CoreController core, string recordName) {
            return createByName<pageTemplateModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static pageTemplateModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<pageTemplateModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<pageTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<pageTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<pageTemplateModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<pageTemplateModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<pageTemplateModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<pageTemplateModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<pageTemplateModel> createList(CoreController core, string sqlCriteria) {
            return createList<pageTemplateModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCacheSingleRecord<pageTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<pageTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<pageTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<pageTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static pageTemplateModel createDefault(CoreController core) {
            return createDefault<pageTemplateModel>(core);
        }
    }
}
