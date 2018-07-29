
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
    public class organizationModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "organizations";
        public const string contentTableName = "organizations";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string BriefFilename { get; set; }
        public string City { get; set; }
        public int Clicks { get; set; }
        public int ContactMemberID { get; set; }
        
        public string CopyFilename { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string Fax { get; set; }
        public string ImageFilename { get; set; }
        public string Link { get; set; }
        public string Phone { get; set; }
        public string State { get; set; }
        public string ThumbNailFilename { get; set; }
        public int Viewings { get; set; }
        public string Web { get; set; }
        public string Zip { get; set; }
        //
        //====================================================================================================
        public static organizationModel add(CoreController core) {
            return add<organizationModel>(core);
        }
        //
        //====================================================================================================
        public static organizationModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<organizationModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static organizationModel create(CoreController core, int recordId) {
            return create<organizationModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static organizationModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<organizationModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static organizationModel create(CoreController core, string recordGuid) {
            return create<organizationModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static organizationModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<organizationModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static organizationModel createByName(CoreController core, string recordName) {
            return createByName<organizationModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static organizationModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<organizationModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<organizationModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<organizationModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<organizationModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<organizationModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<organizationModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<organizationModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<organizationModel> createList(CoreController core, string sqlCriteria) {
            return createList<organizationModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCacheSingleRecord<organizationModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<organizationModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<organizationModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<organizationModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static organizationModel createDefault(CoreController core) {
            return createDefault<organizationModel>(core);
        }
    }
}
