
using System;
using System.Data;
using static Contensive.Processor.Constants;
using Contensive.Processor.Exceptions;
using System.Linq;

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
                    var groupsCdef = Models.Domain.CDefDomainModel.create(core, "groups");
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
                    var groupCdef = Models.Domain.CDefDomainModel.create(core, "groups");
                    group = Models.Db.GroupModel.addDefault(core, groupCdef);
                    group.ccguid = groupNameIdOrGuid;
                    group.name = groupNameIdOrGuid;
                    group.caption = groupNameIdOrGuid;
                    group.save(core);
                }
            } else {
                group = Models.Db.GroupModel.createByUniqueName(core, groupNameIdOrGuid);
                if (group == null) {
                    var groupCdef = Models.Domain.CDefDomainModel.create(core, "groups");
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