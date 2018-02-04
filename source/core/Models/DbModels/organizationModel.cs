
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
        public int ContentCategoryID { get; set; }
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
        public static organizationModel add(coreController core) {
            return add<organizationModel>(core);
        }
        //
        //====================================================================================================
        public static organizationModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<organizationModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static organizationModel create(coreController core, int recordId) {
            return create<organizationModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static organizationModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<organizationModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static organizationModel create(coreController core, string recordGuid) {
            return create<organizationModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static organizationModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<organizationModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static organizationModel createByName(coreController core, string recordName) {
            return createByName<organizationModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static organizationModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<organizationModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<organizationModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<organizationModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<organizationModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<organizationModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<organizationModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<organizationModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<organizationModel> createList(coreController core, string sqlCriteria) {
            return createList<organizationModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<organizationModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<organizationModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<organizationModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<organizationModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static organizationModel createDefault(coreController core) {
            return createDefault<organizationModel>(core);
        }
    }
}
