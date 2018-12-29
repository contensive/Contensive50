
using System;
using System.Data;
using static Contensive.Processor.Constants;
using Contensive.Processor.Exceptions;
using System.Linq;
using static Contensive.Processor.Controllers.GenericController;

namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class GroupController : IDisposable {
        //
        //====================================================================================================
        /// <summary>
        /// Create a group, set the name and caption, return its Id. If the group already exists, the groups Id is returned.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static int add(CoreController core, string groupName, string groupCaption ) => Models.Db.GroupModel.verify(core, groupName, groupCaption).id;
        //
        //====================================================================================================
        /// <summary>
        /// Create a group and return its Id. If the group already exists, the groups Id is returned. If the group cannot be added a 0 is returned.
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="groupCaption"></param>
        /// <returns></returns>
        public static int add(CoreController core, string groupName) => add(core, groupName, groupName);
        //
        //====================================================================================================
        /// <summary>
        /// Add a user to a group with an expiration date
        /// </summary>
        /// <param name="core"></param>
        /// <param name="group"></param>
        /// <param name="user"></param>
        /// <param name="dateExpires"></param>
        public static void addUser(CoreController core, Models.Db.GroupModel group, Models.Db.PersonModel user, DateTime dateExpires) {
            try {
                var ruleList = Models.Db.MemberRuleModel.createList(core, "(MemberID=" + user.id.ToString() + ")and(GroupID=" + group.id.ToString() + ")");
                if ( ruleList.Count==0) {
                    // -- add new rule
                    var groupsCdef = Models.Domain.MetaModel.createByUniqueName(core, "groups");
                    var rule = Models.Db.MemberRuleModel.addDefault(core, groupsCdef ) ;
                    rule.groupId = group.id;
                    rule.MemberID = user.id;
                    rule.dateExpires = dateExpires;
                    rule.save(core);
                    return;
                }
                // at least one rule found, set expire date, delete the rest
                var ruleFirst = ruleList.First();
                if (ruleFirst.dateExpires != dateExpires) {
                    ruleFirst.dateExpires = dateExpires;
                    ruleFirst.save(core);
                }
                if (ruleList.Count > 1) {
                    foreach (var rule in ruleList) {
                        if (!rule.Equals(ruleFirst)) Models.Db.MemberRuleModel.delete(core, rule.id);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Add a user to a group with no expiration date
        /// </summary>
        /// <param name="core"></param>
        /// <param name="group"></param>
        /// <param name="user"></param>
        public static void addUser(CoreController core, Models.Db.GroupModel group, Models.Db.PersonModel user) => addUser(core, group, user, DateTime.MinValue);
        //
        //====================================================================================================
        /// <summary>
        /// Add a user to a group. If gorupId doesnt exist, an error is logged.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="groupId"></param>
        /// <param name="userid"></param>
        /// <param name="dateExpires"></param>
        public static void addUser(CoreController core, int groupId, int userid, DateTime dateExpires) {
            var group = Models.Db.GroupModel.create(core, groupId);
            if (group == null) {
                LogController.handleError(core, new GenericException("addUser called with invalid groupId"));
            } else {
                var user = Models.Db.PersonModel.create(core, userid);
                if ( user != null ) {
                    addUser(core, group, user, dateExpires);
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Add a user to a group. if group is missing and argument is name or guid, it is created.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="groupNameIdOrGuid"></param>
        /// <param name="userid"></param>
        /// <param name="dateExpires"></param>
        public static void addUser(CoreController core, string groupNameIdOrGuid, int userid, DateTime dateExpires) {
            Models.Db.GroupModel group = null;
            if ( groupNameIdOrGuid.IsNumeric()) {
                group = Models.Db.GroupModel.create(core, GenericController.encodeInteger(groupNameIdOrGuid));
                if (group == null) {
                    LogController.handleError(core, new GenericException("addUser called with invalid groupId"));
                    return;
                }
            } else if ( GenericController.isGuid( groupNameIdOrGuid )) {
                group = Models.Db.GroupModel.create(core, groupNameIdOrGuid);
                if (group == null) {
                    var groupCdef = Models.Domain.MetaModel.createByUniqueName(core, "groups");
                    group = Models.Db.GroupModel.addDefault(core, groupCdef);
                    group.ccguid = groupNameIdOrGuid;
                    group.name = groupNameIdOrGuid;
                    group.caption = groupNameIdOrGuid;
                    group.save(core);
                }
            } else {
                group = Models.Db.GroupModel.createByUniqueName(core, groupNameIdOrGuid);
                if (group == null) {
                    var groupCdef = Models.Domain.MetaModel.createByUniqueName(core, "groups");
                    group = Models.Db.GroupModel.addDefault(core, groupCdef);
                    group.ccguid = groupNameIdOrGuid;
                    group.name = groupNameIdOrGuid;
                    group.caption = groupNameIdOrGuid;
                    group.save(core);
                }

            }
            if ( group == null ) {
                // -- create group if not found
            }
            if (group != null) {
                var user = Models.Db.PersonModel.create(core, userid);
                if (user != null) addUser(core, group, user, dateExpires);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Add a user to a group. if group is missing and argument is name or guid, it is created.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="groupNameIdOrGuid"></param>
        /// <param name="userid"></param>
        public static void addUser(CoreController core, string groupNameIdOrGuid, int userid) => addUser(core, groupNameIdOrGuid, userid, DateTime.MinValue);
        //
        //====================================================================================================
        /// <summary>
        /// Add the current user to a group. if group is missing and argument is name or guid, it is created.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="groupNameIdOrGuid"></param>
        public static void addUser(CoreController core, string groupNameIdOrGuid) => addUser(core, groupNameIdOrGuid, core.session.user.id, DateTime.MinValue);
        //
        //====================================================================================================
        /// <summary>
        /// Get a group Id
        /// </summary>
        /// <param name="core"></param>
        /// <param name="GroupName"></param>
        /// <returns></returns>
        public static int getGroupId(CoreController core, string GroupName) {
            var group = Models.Db.GroupModel.createByUniqueName(core, GroupName);
            if (group != null) return group.id;
            return 0;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Get a group Name
        /// </summary>
        /// <param name="core"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static string getGroupName(CoreController core, int groupId) {
            var group = Models.Db.GroupModel.create(core, groupId);
            if (group != null) return group.name;
            return String.Empty;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Remove a user from a group.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="group"></param>
        /// <param name="user"></param>
        public static void removeUser(CoreController core, Models.Db.GroupModel group, Models.Db.PersonModel user) {
            if ((group != null) && (user != null)) {
                Models.Db.MemberRuleModel.deleteSelection(core, "(MemberID=" + DbController.encodeSQLNumber(user.id) + ")AND(groupid=" + DbController.encodeSQLNumber(group.id) + ")");
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Remove a user from a group. 
        /// </summary>
        /// <param name="core"></param>
        /// <param name="groupName"></param>
        /// <param name="userId"></param>
        public static void removeUser(CoreController core, string groupName, int userId) {
            var group = Models.Db.GroupModel.createByUniqueName(core, groupName);
            if ( group != null ) {
                var user = Models.Db.PersonModel.create(core, userId);
                if (user != null) {
                    removeUser(core, group, user);
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Remove the current user from a group
        /// </summary>
        /// <param name="core"></param>
        /// <param name="groupName"></param>
        public static void removeUser(CoreController core, string groupName) => removeUser(core, groupName, core.session.user.id);
        //
        //========================================================================
        /// <summary>
        /// Returns true if the user is an admin, or authenticated and in the group named
        /// </summary>
        /// <param name="core"></param>
        /// <param name="GroupName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        //
        public static bool isMemberOfGroup(CoreController core, string GroupName, int userId) {
            bool result = false;
            try {
                result = isInGroupList(core, "," + GroupController.getGroupId(core, GroupName), userId, true);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return result;
        }
        //
        public static bool isMemberOfGroup(CoreController core, string GroupName) => isMemberOfGroup(core, GroupName, core.session.user.id);
        //
        //========================================================================
        // ----- Returns true if the visitor is an admin, or authenticated and in the group list
        //========================================================================
        //
        public static bool isInGroupList(CoreController core, string GroupIDList, int checkMemberID = 0, bool adminReturnsTrue = false) {
            bool result = false;
            try {
                if (checkMemberID == 0) {
                    checkMemberID = core.session.user.id;
                }
                result = isInGroupList(core, checkMemberID, core.session.isAuthenticated, GroupIDList, adminReturnsTrue);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return result;
        }
        //
        //===============================================================================================================================
        //   Is Group Member of a GroupIDList
        //   admins are always returned true
        //===============================================================================================================================
        //
        public static bool isInGroupList(CoreController core, int MemberID, bool isAuthenticated, string GroupIDList) {
            return isInGroupList(core, MemberID, isAuthenticated, GroupIDList, true);
        }
        //
        //===============================================================================================================================
        //   Is Group Member of a GroupIDList
        //===============================================================================================================================
        //
        public static bool isInGroupList(CoreController core, int MemberID, bool isAuthenticated, string GroupIDList, bool adminReturnsTrue) {
            bool returnREsult = false;
            try {
                //
                string sql = null;
                string Criteria = null;
                string WorkingIDList = null;
                //
                returnREsult = false;
                if (isAuthenticated) {
                    WorkingIDList = GroupIDList;
                    WorkingIDList = GenericController.vbReplace(WorkingIDList, " ", "");
                    while (GenericController.vbInstr(1, WorkingIDList, ",,") != 0) {
                        WorkingIDList = GenericController.vbReplace(WorkingIDList, ",,", ",");
                    }
                    if (!string.IsNullOrEmpty(WorkingIDList)) {
                        if (vbMid(WorkingIDList, 1) == ",") {
                            if (vbLen(WorkingIDList) <= 1) {
                                WorkingIDList = "";
                            } else {
                                WorkingIDList = vbMid(WorkingIDList, 2);
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(WorkingIDList)) {
                        if (WorkingIDList.Right(1) == ",") {
                            if (vbLen(WorkingIDList) <= 1) {
                                WorkingIDList = "";
                            } else {
                                WorkingIDList = GenericController.vbMid(WorkingIDList, 1, vbLen(WorkingIDList) - 1);
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(WorkingIDList)) {
                        if (adminReturnsTrue) {
                            //
                            // check if memberid is admin
                            //
                            sql = "select top 1 m.id"
                                + " from ccmembers m"
                                + " where"
                                + " (m.id=" + MemberID + ")"
                                + " and(m.active<>0)"
                                + " and("
                                + " (m.admin<>0)"
                                + " or(m.developer<>0)"
                                + " )"
                                + " ";
                            using (var csXfer = new CsModel(core)) {
                                returnREsult = csXfer.csOpenSql(sql);
                            }
                        }
                    } else {
                        //
                        // check if they are admin or in the group list
                        //
                        if (GenericController.vbInstr(1, WorkingIDList, ",") != 0) {
                            Criteria = "r.GroupID in (" + WorkingIDList + ")";
                        } else {
                            Criteria = "r.GroupID=" + WorkingIDList;
                        }
                        Criteria = ""
                            + "(" + Criteria + ")"
                            + " and(r.id is not null)"
                            + " and((r.DateExpires is null)or(r.DateExpires>" + DbController.encodeSQLDate(DateTime.Now) + "))"
                            + " ";
                        if (adminReturnsTrue) {
                            Criteria = "(" + Criteria + ")or(m.admin<>0)or(m.developer<>0)";
                        }
                        Criteria = ""
                            + "(" + Criteria + ")"
                            + " and(m.active<>0)"
                            + " and(m.id=" + MemberID + ")";
                        //
                        sql = "select top 1 m.id"
                            + " from ccmembers m"
                            + " left join ccMemberRules r on r.Memberid=m.id"
                            + " where" + Criteria;
                        using (var csXfer = new CsModel(core)) {
                            csXfer.csOpenSql(sql, "Default");
                            returnREsult = csXfer.csOk();
                        }
                    }
                }

            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnREsult;
        }



        //
        //====================================================================================================
        #region  IDisposable Support 
        //
        // this class must implement System.IDisposable
        // never throw an exception in dispose
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        //====================================================================================================
        //
        protected bool disposed = false;
        //
        public void Dispose() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        ~GroupController() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(false);
        }
        //
        //====================================================================================================
        /// <summary>
        /// dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
                    //If (cacheClient IsNot Nothing) Then
                    //    cacheClient.Dispose()
                    //End If
                }
                //
                // cleanup non-managed objects
                //
            }
        }
        #endregion
    }
    //
}