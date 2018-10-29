
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
namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class GroupController : IDisposable {
        //
        // ----- constants
        //
        //Private Const invalidationDaysDefault As Double = 365
        //
        // ----- objects constructed that must be disposed
        //
        //Private cacheClient As Enyim.Caching.MemcachedClient
        //
        // ----- private instance storage
        //
        //Private remoteCacheDisabled As Boolean

        //
        //====================================================================================================
        /// <summary>
        /// Create a group and return its Id. If the group already exists, the groups Id is returned. If the group cannot be added a 0 is returned.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static int group_add(CoreController core, string groupName) {
            int returnGroupId = 0;
            try {
                DataTable dt = null;
                string sql = null;
                int createkey = 0;
                int cid = 0;
                string sqlGroupName = core.db.encodeSQLText(groupName);
                //
                dt = core.db.executeQuery("SELECT ID FROM CCGROUPS WHERE NAME=" + sqlGroupName + "");
                if (dt.Rows.Count > 0) {
                    returnGroupId = GenericController.encodeInteger(dt.Rows[0]["ID"]);
                } else {
                    cid = CdefController.getContentId(core, "groups");
                    createkey = GenericController.GetRandomInteger(core);
                    sql = "insert into ccgroups (contentcontrolid,active,createkey,name,caption) values (" + cid + ",1," + createkey + "," + sqlGroupName + "," + sqlGroupName + ")";
                    core.db.executeQuery(sql);
                    //
                    sql = "select top 1 id from ccgroups where createkey=" + createkey + " order by id desc";
                    dt = core.db.executeQuery(sql);
                    if (dt.Rows.Count > 0) {
                        returnGroupId = GenericController.encodeInteger(dt.Rows[0][0]);
                    }
                }
                dt.Dispose();
            } catch (Exception ex) {
                throw (ex);
            }
            return returnGroupId;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Create a group and return its Id. If the group already exists, the groups Id is returned. If the group cannot be added a 0 is returned.
        /// </summary>
        /// <param name="GroupNameOrGuid"></param>
        /// <param name="groupCaption"></param>
        /// <returns></returns>
        public static int group_add2(CoreController core, string GroupNameOrGuid, string groupCaption = "") {
            int returnGroupId = 0;
            try {
                //
                CsController cs = new CsController(core);
                bool IsAlreadyThere = false;
                string sqlCriteria = core.db.getNameIdOrGuidSqlCriteria(GroupNameOrGuid);
                string groupName = null;
                string groupGuid = null;
                //
                if (string.IsNullOrEmpty(GroupNameOrGuid)) {
                    throw (new ApplicationException("A group cannot be added with a blank name"));
                } else {
                    cs.open("Groups", sqlCriteria, "", false, "id");
                    IsAlreadyThere = cs.ok();
                    cs.close();
                    if (!IsAlreadyThere) {
                        cs.insert("Groups");
                        if (!cs.ok()) {
                            throw (new ApplicationException("There was an error inserting a new group record"));
                        } else {
                            returnGroupId = cs.getInteger("id");
                            if (GenericController.isGuid(GroupNameOrGuid)) {
                                groupName = "Group " + cs.getInteger("id");
                                groupGuid = GroupNameOrGuid;
                            } else {
                                groupName = GroupNameOrGuid;
                                groupGuid = GenericController.getGUID();
                            }
                            if (string.IsNullOrEmpty(groupCaption)) {
                                groupCaption = groupName;
                            }
                            cs.setField("name", groupName);
                            cs.setField("caption", groupCaption);
                            cs.setField("ccGuid", groupGuid);
                            cs.setField("active", "1");
                        }
                        cs.close();
                    }
                }
            } catch (Exception ex) {
                throw new ApplicationException("Unexpected error in cp.group.add()", ex);
            }
            return returnGroupId;
        }
        //
        //====================================================================================================
        //
        // Add User
        //
        public static void group_addUser(CoreController core, int groupId, int userid, DateTime dateExpires) {
            try {
                //
                string groupName = null;
                //
                if (true) {
                    if (groupId < 1) {
                        throw (new ApplicationException("Could not find or create the group with id [" + groupId + "]"));
                    } else {
                        if (userid == 0) {
                            userid = core.session.user.id;
                        }
                        using (CsController cs = new CsController(core)) {
                            cs.open("Member Rules", "(MemberID=" + userid.ToString() + ")and(GroupID=" + groupId.ToString() + ")", "", false);
                            if (!cs.ok()) {
                                cs.close();
                                cs.insert("Member Rules");
                            }
                            if (!cs.ok()) {
                                groupName = core.db.getRecordName("groups", groupId);
                                throw (new ApplicationException("Could not find or create the Member Rule to add this member [" + userid + "] to the Group [" + groupId + ", " + groupName + "]"));
                            } else {
                                cs.setField("active", "1");
                                cs.setField("memberid", userid.ToString());
                                cs.setField("groupid", groupId.ToString());
                                if (dateExpires != Convert.ToDateTime("12:00:00 AM")) {
                                    cs.setField("DateExpires", dateExpires.ToString());
                                } else {
                                    cs.setField("DateExpires", "");
                                }
                            }
                            cs.close();
                        }
                    }
                }
            } catch (Exception ex) {
                throw (ex);
            }
        }
        public static void group_addUser(CoreController core, int groupId, int userid) { group_addUser(core, groupId, userid, DateTime.MinValue ); }
        public static void group_addUser(CoreController core, int groupId) { group_addUser(core, groupId, 0, DateTime.MinValue); }
        //
        //====================================================================================================
        //
        public static void group_AddUser(CoreController core, string groupNameOrGuid, int userid, DateTime dateExpires) {
            try {
                //
                int GroupID = 0;
                //
                if (!string.IsNullOrEmpty(groupNameOrGuid)) {
                    GroupID = core.db.getRecordID("groups", groupNameOrGuid);
                    if (GroupID < 1) {
                        group_add2(core, groupNameOrGuid);
                        GroupID = core.db.getRecordID("groups", groupNameOrGuid);
                    }
                    if (GroupID < 1) {
                        throw (new ApplicationException("Could not find or create the group [" + groupNameOrGuid + "]"));
                    } else {
                        if (userid == 0) {
                            userid = core.session.user.id;
                        }
                        using (CsController cs = new CsController(core)) {
                            cs.open("Member Rules", "(MemberID=" + userid.ToString() + ")and(GroupID=" + GroupID.ToString() + ")", "", false);
                            if (!cs.ok()) {
                                cs.close();
                                cs.insert("Member Rules");
                            }
                            if (!cs.ok()) {
                                throw (new ApplicationException("Could not find or create the Member Rule to add this member [" + userid + "] to the Group [" + GroupID + ", " + groupNameOrGuid + "]"));
                            } else {
                                cs.setField("active", "1");
                                cs.setField("memberid", userid.ToString());
                                cs.setField("groupid", GroupID.ToString());
                                if (dateExpires != Convert.ToDateTime("12:00:00 AM")) {
                                    cs.setField("DateExpires", dateExpires.ToString());
                                } else {
                                    cs.setField("DateExpires", "");
                                }
                            }
                            cs.close();
                        }
                    }
                }
            } catch (Exception ex) {
                throw (ex);
            }
        }
        public static void group_AddUser(CoreController core, string groupNameOrGuid, int userid = 0) { var tmpDate = DateTime.MinValue; group_AddUser(core, groupNameOrGuid, userid, tmpDate); }
        public static void group_AddUser(CoreController core, string groupNameOrGuid) { var tmpDate = DateTime.MinValue; group_AddUser(core, groupNameOrGuid, 0, tmpDate); }

        //
        //=============================================================================
        // main_Get the GroupID from iGroupName
        //=============================================================================
        //
        public static int group_GetGroupID(CoreController core, string GroupName) {
            int tempgroup_GetGroupID = 0;
            DataTable dt = null;
            //
            string iGroupName = GenericController.encodeText(GroupName);
            tempgroup_GetGroupID = 0;
            if (!string.IsNullOrEmpty(iGroupName)) {
                //
                // ----- main_Get the Group ID
                //
                dt = core.db.executeQuery("select top 1 id from ccGroups where name=" + core.db.encodeSQLText(iGroupName));
                if (dt.Rows.Count > 0) {
                    tempgroup_GetGroupID = GenericController.encodeInteger(dt.Rows[0][0]);
                }
            }
            return tempgroup_GetGroupID;
        }
        //
        //=============================================================================
        // main_Get the GroupName from iGroupID
        //=============================================================================
        //
        public static string group_GetGroupName(CoreController core, int GroupID) {
            string tempgroup_GetGroupName = null;
            //
            int CS = 0;
            int iGroupID = GenericController.encodeInteger(GroupID);
            //
            tempgroup_GetGroupName = "";
            if (iGroupID > 0) {
                //
                // ----- main_Get the Group name
                //
                CS = core.db.csOpen2("Groups", iGroupID);
                if (core.db.csOk(CS)) {
                    tempgroup_GetGroupName = GenericController.encodeText(core.db.csGetValue(CS, "Name"));
                }
                core.db.csClose(ref CS);
            }
            return tempgroup_GetGroupName;
        }
        //
        //=============================================================================
        // Add a new group, return its GroupID
        //=============================================================================
        //
        public static int group_Add(CoreController core, string GroupName, string GroupCaption = "") {
            int tempgroup_Add = 0;
            int CS = 0;
            string iGroupName = null;
            string iGroupCaption = null;
            //
            iGroupName = GenericController.encodeText(GroupName);
            iGroupCaption = GenericController.encodeEmpty(GroupCaption, iGroupName);
            //
            tempgroup_Add = -1;
            DataTable dt = core.db.executeQuery("SELECT ID FROM ccgroups WHERE NAME=" + core.db.encodeSQLText(iGroupName));
            if (dt.Rows.Count > 0) {
                tempgroup_Add = GenericController.encodeInteger(dt.Rows[0][0]);
            } else {
                CS = core.db.csInsertRecord("Groups", SystemMemberID);
                if (core.db.csOk(CS)) {
                    tempgroup_Add = GenericController.encodeInteger(core.db.csGetValue(CS, "ID"));
                    core.db.csSet(CS, "name", iGroupName);
                    core.db.csSet(CS, "caption", iGroupCaption);
                    core.db.csSet(CS, "active", true);
                }
                core.db.csClose(ref CS);
            }
            return tempgroup_Add;
        }

        //
        //=============================================================================
        // Add a new group, return its GroupID
        //=============================================================================
        //
        public static void group_DeleteGroup(CoreController core, string GroupName) {
            core.db.deleteContentRecords("Groups", "name=" + core.db.encodeSQLText(GroupName));
        }
        //
        //=============================================================================
        // Add a member to a group
        //=============================================================================
        //
        public static void group_AddGroupMember(CoreController core, string GroupName, int NewMemberID = SystemMemberID, DateTime DateExpires = default(DateTime)) {
            //
            int CS = 0;
            int GroupID = 0;
            string iGroupName = null;
            DateTime iDateExpires = default(DateTime);
            //
            iGroupName = GenericController.encodeText(GroupName);
            iDateExpires = DateExpires;
            if (!string.IsNullOrEmpty(iGroupName)) {
                GroupID = group_GetGroupID(core, iGroupName);
                if (GroupID < 1) {
                    GroupID = group_Add(core, GroupName, GroupName);
                }
                if (GroupID < 1) {
                    throw (new ApplicationException("main_AddGroupMember could not find or add Group [" + GroupName + "]")); // handleLegacyError14(MethodName, "")
                } else {
                    CS = core.db.csOpen("Member Rules", "(MemberID=" + core.db.encodeSQLNumber(NewMemberID) + ")and(GroupID=" + core.db.encodeSQLNumber(GroupID) + ")", "", false);
                    if (!core.db.csOk(CS)) {
                        core.db.csClose(ref CS);
                        CS = core.db.csInsertRecord("Member Rules");
                    }
                    if (!core.db.csOk(CS)) {
                        throw (new ApplicationException("main_AddGroupMember could not add this member to the Group [" + GroupName + "]")); // handleLegacyError14(MethodName, "")
                    } else {
                        core.db.csSet(CS, "active", true);
                        core.db.csSet(CS, "memberid", NewMemberID);
                        core.db.csSet(CS, "groupid", GroupID);
                        if (iDateExpires != DateTime.MinValue) {
                            core.db.csSet(CS, "DateExpires", iDateExpires);
                        } else {
                            core.db.csSet(CS, "DateExpires", "");
                        }
                    }
                    core.db.csClose(ref CS);
                }
            }
        }
        //
        //=============================================================================
        // Delete a member from a group
        //=============================================================================
        //
        public static void group_DeleteGroupMember(CoreController core, string GroupName, int NewMemberID = SystemMemberID) {
            //
            int GroupID = 0;
            string iGroupName;
            //
            iGroupName = GenericController.encodeText(GroupName);
            //
            if (!string.IsNullOrEmpty(iGroupName)) {
                GroupID = group_GetGroupID(core, iGroupName);
                if (GroupID < 1) {
                } else if (NewMemberID < 1) {
                    throw (new ApplicationException("Member ID is invalid")); // handleLegacyError14(MethodName, "")
                } else {
                    core.db.deleteContentRecords("Member Rules", "(MemberID=" + core.db.encodeSQLNumber(NewMemberID) + ")AND(groupid=" + core.db.encodeSQLNumber(GroupID) + ")");
                }
            }
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
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
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