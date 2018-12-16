
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
    public class PersonModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "people";
        public const string contentTableName = "ccmembers";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public string Address { get; set; }
        public string Address2 { get; set; }
        public bool Admin { get; set; }
        // -- 20181101 remove top menu
        //public int AdminMenuModeID { get; set; }
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
        public static PersonModel addEmpty(CoreController core) {
            return addEmpty<PersonModel>(core);
        }
        //
        //====================================================================================================
        public static PersonModel addDefault(CoreController core, Domain.CDefDomainModel cdef) {
            return addDefault<PersonModel>(core, cdef);
        }
        //
        //====================================================================================================
        public static PersonModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.CDefDomainModel cdef) {
            return addDefault<PersonModel>(core, cdef, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static PersonModel create(CoreController core, int recordId) {
            return create<PersonModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static PersonModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<PersonModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static PersonModel create(CoreController core, string recordGuid) {
            return create<PersonModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static PersonModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<PersonModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static PersonModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<PersonModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static PersonModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<PersonModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<PersonModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<PersonModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<PersonModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<PersonModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<PersonModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<PersonModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<PersonModel> createList(CoreController core, string sqlCriteria) {
            return createList<PersonModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<PersonModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return getRecordName<PersonModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return getRecordName<PersonModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return getRecordId<PersonModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        //public static PersonModel createDefault(CoreController core) {
        //    return createDefault<PersonModel>(core);
        //}
        //
        //====================================================================================================
        //
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<PersonModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// return a list of people in a any one of a list of groups. If requireBuldEmail true, the list only includes those with allowBulkEmail.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="groupList"></param>
        /// <param name="requireBulkEmail"></param>
        /// <returns></returns>
        public static List<PersonModel> createListFromGroupList( CoreController core, List<string> groupList, bool requireBulkEmail ) {
            var personList = new List<PersonModel> { };
            try {
                string sqlGroups = "";
                foreach (string group in groupList) {
                    if (!string.IsNullOrWhiteSpace(group)) {
                        if (!group.Equals(groupList.First<string>())) {
                            sqlGroups += "or";
                        }
                        sqlGroups += "(ccgroups.Name=" + DbController.encodeSQLText(group) + ")";
                    }
                }
                string sqlCriteria = ""
                    + "SELECT DISTINCT ccMembers.ID"
                    + " FROM ((ccMembers"
                    + " LEFT JOIN ccMemberRules ON ccMembers.ID = ccMemberRules.MemberID)"
                    + " LEFT JOIN ccgroups ON ccMemberRules.GroupID = ccgroups.ID)"
                    + " WHERE (ccMembers.Active>0)"
                    + " and(ccMemberRules.Active>0)"
                    + " and(ccgroups.Active>0)"
                    + " and((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" + DbController.encodeSQLDate(core.doc.profileStartTime) + "))";
                if (requireBulkEmail) {
                    sqlCriteria += "and(ccMembers.AllowBulkEmail>0)and(ccgroups.AllowBulkEmail>0)";
                }
                if (!string.IsNullOrEmpty(sqlGroups)) {
                    sqlCriteria += "and(" + sqlGroups + ")";
                }
                personList = createList(core, "(id in (" + sqlCriteria + "))");
            } catch (Exception) {
                throw;
            }
            return personList;
        }
        //
        public static List<int> createidListForEmail(CoreController core, int emailId) {
            var result = new List<int> { };
            string sqlCriteria = ""
                    + " select"
                    + " u.id as id"
                    + " "
                    + " from "
                    + " (((ccMembers u"
                    + " left join ccMemberRules mr on mr.memberid=u.id)"
                    + " left join ccGroups g on g.id=mr.groupid)"
                    + " left join ccEmailGroups r on r.groupid=g.id)"
                    + " "
                    + " where "
                    + " (r.EmailID=" + emailId.ToString() + ")"
                    + " and(r.Active<>0)"
                    + " and(g.Active<>0)"
                    + " and(g.AllowBulkEmail<>0)"
                    + " and(mr.Active<>0)"
                    + " and(u.Active<>0)"
                    + " and(u.AllowBulkEmail<>0)"
                    + " and((mr.DateExpires is null)OR(mr.DateExpires>" + DbController.encodeSQLDate(DateTime.Now)  + ")) "
                    + " "
                    + " group by "
                    + " u.ID, u.Name, u.Email "
                    + " "
                    + " having ((u.Email Is Not Null) and(u.Email<>'')) "
                    + " "
                    + " order by u.Email,u.ID"
                    + " ";
            var cs = new CsController(core);
            if (cs.openSQL(sqlCriteria)) {
                do {
                    result.Add(cs.getInteger("id"));
                    cs.goNext();
                } while (cs.ok());
            }
            cs.close();
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the best name available for this record
        /// </summary>
        /// <returns></returns>
        public string getDisplayName() {
            if (string.IsNullOrWhiteSpace(name)) {
                return "unnamed #" + id.ToString();
            } else {
                return name;
            }
        }
    }
}
