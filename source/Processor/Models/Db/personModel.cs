
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
        public string address { get; set; }
        public string address2 { get; set; }
        public bool admin { get; set; }
        // -- 20181101 remove top menu
        //public int AdminMenuModeID { get; set; }
        public bool allowBulkEmail { get; set; }
        public bool allowToolsPanel { get; set; }
        public bool autoLogin { get; set; }
        public string billAddress { get; set; }
        public string billAddress2 { get; set; }
        public string billCity { get; set; }
        public string billCompany { get; set; }
        public string billCountry { get; set; }
        public string billEmail { get; set; }
        public string billFax { get; set; }
        public string billName { get; set; }
        public string billPhone { get; set; }
        public string billState { get; set; }
        public string billZip { get; set; }
        public int birthdayDay { get; set; }
        public int birthdayMonth { get; set; }
        public int birthdayYear { get; set; }
        public string city { get; set; }
        public string company { get; set; }
        public string country { get; set; }
        public bool createdByVisit { get; set; }
        public DateTime dateExpires { get; set; }
        public bool developer { get; set; }
        public string email { get; set; }
        public bool excludeFromAnalytics { get; set; }
        public string fax { get; set; }
        public string firstName { get; set; }
        public string imageFilename { get; set; }
        public int languageID { get; set; }
        public string lastName { get; set; }
        public DateTime LastVisit { get; set; }
        public string nickName { get; set; }
        public string notesFilename { get; set; }
        public int organizationID { get; set; }
        public string password { get; set; }
        public string phone { get; set; }
        public string resumeFilename { get; set; }
        public string shipAddress { get; set; }
        public string shipAddress2 { get; set; }
        public string shipCity { get; set; }
        public string shipCompany { get; set; }
        public string shipCountry { get; set; }
        public string shipName { get; set; }
        public string shipPhone { get; set; }
        public string shipState { get; set; }
        public string shipZip { get; set; }
        public string state { get; set; }
        public string thumbnailFilename { get; set; }
        public string title { get; set; }
        public string username { get; set; }
        public int visits { get; set; }
        public string zip { get; set; }
        //
        //====================================================================================================
        public static PersonModel addEmpty(CoreController core) {
            return addEmpty<PersonModel>(core);
        }
        //
        //====================================================================================================
        public static PersonModel addDefault(CoreController core, Domain.MetaModel cdef) {
            return addDefault<PersonModel>(core, cdef);
        }
        //
        //====================================================================================================
        public static PersonModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel cdef) {
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
        /// <param name="groupNameList"></param>
        /// <param name="requireBulkEmail"></param>
        /// <returns></returns>
        public static List<PersonModel> createListFromGroupNameList(CoreController core, List<string> groupNameList, bool requireBulkEmail) {
            try {
                string sqlGroups = "";
                foreach (string group in groupNameList) {
                    if (!string.IsNullOrWhiteSpace(group)) {
                        if (!group.Equals(groupNameList.First<string>())) {
                            sqlGroups += "or";
                        }
                        sqlGroups += "(ccgroups.Name=" + DbController.encodeSQLText(group) + ")";
                    }
                }
                return createListFromGroupSql(core, sqlGroups, requireBulkEmail);
            } catch (Exception) {
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public static List<PersonModel> createListFromGroupIdList(CoreController core, List<int> groupIdList, bool requireBulkEmail) {
            try {
                string sqlGroups = "";
                foreach (int groupId in groupIdList) {
                    if (groupId>0) {
                        if (!groupId.Equals(groupIdList.First<int>())) {
                            sqlGroups += "or";
                        }
                        sqlGroups += "(ccgroups.id=" + groupId + ")";
                    }
                }
                return createListFromGroupSql(core, sqlGroups, requireBulkEmail);
            } catch (Exception) {
                throw;
            }
        }
        //
        //====================================================================================================
        //
        internal static List<PersonModel> createListFromGroupSql(CoreController core, string sqlGroups, bool requireBulkEmail) {
            try {
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
                return createList(core, "(id in (" + sqlCriteria + "))");
            } catch (Exception) {
                throw;
            }
        }
        //
        //====================================================================================================
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
            var cs = new CsModel(core);
            if (cs.openSql(sqlCriteria)) {
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
