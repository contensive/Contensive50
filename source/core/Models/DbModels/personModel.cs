
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
        public static personModel add(coreController core) {
            return add<personModel>(core);
        }
        //
        //====================================================================================================
        public static personModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<personModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static personModel create(coreController core, int recordId) {
            return create<personModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static personModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<personModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static personModel create(coreController core, string recordGuid) {
            return create<personModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static personModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<personModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static personModel createByName(coreController core, string recordName) {
            return createByName<personModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static personModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<personModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<personModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<personModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<personModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<personModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<personModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<personModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<personModel> createList(coreController core, string sqlCriteria) {
            return createList<personModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<personModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<personModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<personModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<personModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static personModel createDefault(coreController core) {
            return createDefault<personModel>(core);
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
        public static List<personModel> createListFromGroupList( coreController core, List<string> groupList, bool requireBulkEmail ) {
            var personList = new List<personModel> { };
            try {
                string sqlGroups = "";
                foreach (string group in groupList) {
                    if (!string.IsNullOrWhiteSpace(group)) {
                        if (!group.Equals(groupList.First<string>())) {
                            sqlGroups += "or";
                        }
                        sqlGroups += "(ccgroups.Name=" + core.db.encodeSQLText(group) + ")";
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
                    + " and((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" + core.db.encodeSQLDate(core.doc.profileStartTime) + "))";
                if (requireBulkEmail) {
                    sqlCriteria += "and(ccMembers.AllowBulkEmail>0)and(ccgroups.AllowBulkEmail>0)";
                }
                if (!string.IsNullOrEmpty(sqlGroups)) {
                    sqlCriteria += "and(" + sqlGroups + ")";
                }
                personList = createList(core, sqlCriteria);
            } catch (Exception) {
                throw;
            }
            return personList;
        }
        //
        public static List<int> createidListForEmail(coreController core, int emailId) {
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
                    + " and((mr.DateExpires is null)OR(mr.DateExpires>" + core.db.encodeSQLDate(DateTime.Now)  + ")) "
                    + " "
                    + " group by "
                    + " u.ID, u.Name, u.Email "
                    + " "
                    + " having ((u.Email Is Not Null) and(u.Email<>'')) "
                    + " "
                    + " order by u.Email,u.ID"
                    + " ";
            var cs = new csController(core);
            if (cs.openSQL(sqlCriteria)) {
                do {
                    result.Add(cs.getInteger("id"));
                    cs.goNext();
                } while (cs.ok());
            }
            cs.close();
            return result;
        }
    }
}
