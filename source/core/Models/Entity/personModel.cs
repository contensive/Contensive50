
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
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Models.Entity {
    public class personModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "people";
        public const string contentTableName = "ccmembers";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public string Address { get; set; }
        public string Address2 { get; set; }
        public bool Admin { get; set; }
        public int AdminMenuModeID { get; set; }
        public bool AllowBulkEmail { get; set; }
        public bool AllowToolsPanel { get; set; }
        public bool AutoLogin { get; set; }
        public string BillAddress { get; set; }
        public string BillAddress2 { get; set; }
        public string BillCity { get; set; }
        public string BillCompany { get; set; }
        public string BillCountry { get; set; }
        public string BillEmail { get; set; }
        public string BillFax { get; set; }
        public string BillName { get; set; }
        public string BillPhone { get; set; }
        public string BillState { get; set; }
        public string BillZip { get; set; }
        public int BirthdayDay { get; set; }
        public int BirthdayMonth { get; set; }
        public int BirthdayYear { get; set; }
        public string City { get; set; }
        public string Company { get; set; }
        public string Country { get; set; }
        public bool CreatedByVisit { get; set; }
        public DateTime DateExpires { get; set; }
        public bool Developer { get; set; }
        public string Email { get; set; }
        public bool ExcludeFromAnalytics { get; set; }
        public string Fax { get; set; }
        public string FirstName { get; set; }
        public string ImageFilename { get; set; }
        public int LanguageID { get; set; }
        public string LastName { get; set; }
        public DateTime LastVisit { get; set; }
        public string nickName { get; set; }
        public string NotesFilename { get; set; }
        public int OrganizationID { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string ResumeFilename { get; set; }
        public string ShipAddress { get; set; }
        public string ShipAddress2 { get; set; }
        public string ShipCity { get; set; }
        public string ShipCompany { get; set; }
        public string ShipCountry { get; set; }
        public string ShipName { get; set; }
        public string ShipPhone { get; set; }
        public string ShipState { get; set; }
        public string ShipZip { get; set; }
        public string State { get; set; }
        public string ThumbnailFilename { get; set; }
        public string Title { get; set; }
        public string Username { get; set; }
        public int Visits { get; set; }
        public string Zip { get; set; }
        //
        //====================================================================================================
        public static personModel add(coreClass cpCore) {
            return add<personModel>(cpCore);
        }
        //
        //====================================================================================================
        public static personModel add(coreClass cpCore, ref List<string> callersCacheNameList) {
            return add<personModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static personModel create(coreClass cpCore, int recordId) {
            return create<personModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static personModel create(coreClass cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<personModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static personModel create(coreClass cpCore, string recordGuid) {
            return create<personModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static personModel create(coreClass cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<personModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static personModel createByName(coreClass cpCore, string recordName) {
            return createByName<personModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static personModel createByName(coreClass cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<personModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreClass cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreClass cpCore, int recordId) {
            delete<personModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreClass cpCore, string ccGuid) {
            delete<personModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<personModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<personModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<personModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<personModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<personModel> createList(coreClass cpCore, string sqlCriteria) {
            return createList<personModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreClass cpCore, int recordId) {
            invalidateCacheSingleRecord<personModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreClass cpcore, int recordId) {
            return baseModel.getRecordName<personModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreClass cpcore, string ccGuid) {
            return baseModel.getRecordName<personModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreClass cpcore, string ccGuid) {
            return baseModel.getRecordId<personModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static personModel createDefault(coreClass cpcore) {
            return createDefault<personModel>(cpcore);
        }
    }
}
