
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
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
namespace Contensive.Processor.Models.Db {
    public class OrganizationModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "organizations";
        public const string contentTableName = "organizations";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string briefFilename { get; set; }
        public string city { get; set; }
        public int clicks { get; set; }
        public int contactMemberID { get; set; }
        
        public string copyFilename { get; set; }
        public string country { get; set; }
        public string email { get; set; }
        public string fax { get; set; }
        public string imageFilename { get; set; }
        public string link { get; set; }
        public string phone { get; set; }
        public string state { get; set; }
        public string thumbNailFilename { get; set; }
        public int viewings { get; set; }
        public string web { get; set; }
        public string zip { get; set; }
        // 
        //====================================================================================================
        public static OrganizationModel addEmpty(CoreController core) {
            return addEmpty<OrganizationModel>(core);
        }
        //
        //====================================================================================================
        public static OrganizationModel addDefault(CoreController core, Domain.ContentMetaDomainModel cdef) {
            return addDefault<OrganizationModel>(core, cdef);
        }
        //
        //====================================================================================================
        public static OrganizationModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.ContentMetaDomainModel cdef) {
            return addDefault<OrganizationModel>(core, cdef, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static OrganizationModel create(CoreController core, int recordId) {
            return create<OrganizationModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static OrganizationModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<OrganizationModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static OrganizationModel create(CoreController core, string recordGuid) {
            return create<OrganizationModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static OrganizationModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<OrganizationModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static OrganizationModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<OrganizationModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static OrganizationModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<OrganizationModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<OrganizationModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<OrganizationModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<OrganizationModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<OrganizationModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<OrganizationModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<OrganizationModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<OrganizationModel> createList(CoreController core, string sqlCriteria) {
            return createList<OrganizationModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<OrganizationModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<OrganizationModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<OrganizationModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<OrganizationModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        //public static OrganizationModel createDefault(CoreController core) {
        //    return createDefault<OrganizationModel>(core);
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<OrganizationModel>(core);
        }
    }
}
