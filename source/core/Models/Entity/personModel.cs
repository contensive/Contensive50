
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
        //
        //====================================================================================================
        /// <summary>
        /// return a list of people in a any one of a list of groups. If requireBuldEmail true, the list only includes those with allowBulkEmail.
        /// </summary>
        /// <param name="cpcore"></param>
        /// <param name="groupList"></param>
        /// <param name="requireBulkEmail"></param>
        /// <returns></returns>
        public static List<personModel> createListFromGroupList( coreClass cpcore, List<string> groupList, bool requireBulkEmail ) {
            var personList = new List<personModel> { };
            try {
                string sqlGroups = "";
                foreach (string group in groupList) {
                    if (!string.IsNullOrWhiteSpace(group)) {
                        if (!group.Equals(groupList.First<string>())) {
                            sqlGroups += "or";
                        }
                        sqlGroups += "(ccgroups.Name=" + cpcore.db.encodeSQLText(group) + ")";
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
                    + " and((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" + cpcore.db.encodeSQLDate(cpcore.doc.profileStartTime) + "))";
                if (requireBulkEmail) {
                    sqlCriteria += "and(ccMembers.AllowBulkEmail>0)and(ccgroups.AllowBulkEmail>0)";
                }
                if (!string.IsNullOrEmpty(sqlGroups)) {
                    sqlCriteria += "and(" + sqlGroups + ")";
                }
                personList = createList(cpcore, sqlCriteria);
            } catch (Exception) {
                throw;
            }
            return personList;
        }
        //
        public static List<int> createidListForEmail(coreClass cpcore, int emailId) {
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
                    + " and((mr.DateExpires is null)OR(mr.DateExpires>" + cpcore.db.encodeSQLDate(DateTime.Now)  + ")) "
                    + " "
                    + " group by "
                    + " u.ID, u.Name, u.Email "
                    + " "
                    + " having ((u.Email Is Not Null) and(u.Email<>'')) "
                    + " "
                    + " order by u.Email,u.ID"
                    + " ";
            var cs = new csController(cpcore);
            if (cs.openSQL(sqlCriteria)) {
                do {
                    result.Add(cs.getInteger("id"));
                    cs.goNext();
                } while (cs.ok());
            }
            cs.Close();
            return result;
        }
    }
}
