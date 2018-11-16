
using System;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;

namespace Contensive.Processor {
    //
    // comVisible to be activeScript compatible
    //
    //[ComVisible(true), Microsoft.VisualBasic.ComClass(CPGroupClass.ClassId, CPGroupClass.InterfaceId, CPGroupClass.EventsId)]
    public class CPGroupClass : BaseClasses.CPGroupBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "C0000B6E-5ABF-4C67-9F22-EF52D73FC54B";
        public const string InterfaceId = "DB74B7D9-73BE-40C1-B488-ACC098E8B9C1";
        public const string EventsId = "B9E3C450-CDC4-4590-8BCD-FEDDF7338D4B";
        #endregion
        //
        private Contensive.Processor.Controllers.CoreController core;
        private CPClass cp;
        protected bool disposed = false;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cpParent"></param>
        public CPGroupClass(CPClass cpParent) : base() {
            cp = cpParent;
            core = cp.core;
        }
        /// <summary>
        /// dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                appendDebugLog(".dispose, dereference cp, main, csv");
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        /// <summary>
        /// Add a group
        /// </summary>
        /// <param name="GroupName"></param>
        public override void Add(string GroupName) => GroupController.add2(core, GroupName, GroupName);
        /// <summary>
        /// Add a group
        /// </summary>
        /// <param name="GroupName"></param>
        /// <param name="GroupCaption"></param>
        public override void Add(string GroupName, string GroupCaption) => GroupController.add2(core, GroupName, GroupCaption);
        /// <summary>
        /// Add current user to a group
        /// </summary>
        /// <param name="GroupNameIdOrGuid"></param>
        public override void AddUser(string GroupNameIdOrGuid) => GroupController.addUser(core, GroupNameIdOrGuid, 0, DateTime.MinValue);
        /// <summary>
        /// Add current user to a group
        /// </summary>
        /// <param name="groupId"></param>
        public override void AddUser(int groupId) => GroupController.addUser(core, groupId.ToString(), 0, DateTime.MinValue);
        //
        public override void AddUser(string GroupNameIdOrGuid, int UserId) => GroupController.addUser(core, GroupNameIdOrGuid, UserId, DateTime.MinValue);
        //
        public override void AddUser(string GroupNameIdOrGuid, int UserId, DateTime DateExpires) => GroupController.addUser(core, GroupNameIdOrGuid, UserId, DateExpires);
        //
        public override void AddUser(int GroupId, int UserId) => GroupController.addUser(core, GroupId.ToString(), UserId, DateTime.MinValue);

        public override void AddUser(int GroupId, int UserId, DateTime DateExpires) => GroupController.addUser(core, GroupId.ToString(), UserId, DateExpires);

        public override void Delete(string GroupNameIdOrGuid) => GroupModel.delete(core, GroupNameIdOrGuid);
        //
        public override void Delete(int GroupId) => GroupModel.delete(core, GroupId);
        //
        public override int GetId(string GroupNameIdOrGuid) => core.db.getRecordID("groups", GroupNameIdOrGuid);
        //
        public override string GetName(string GroupIdOrGuid) {
            if (GroupIdOrGuid.IsNumeric()) {
                return core.db.getRecordName("groups", GenericController.encodeInteger(GroupIdOrGuid));
            } else {
                return core.db.getRecordName("groups", GroupIdOrGuid);
            }
        }
        public override string GetName(int GroupId) => core.db.getRecordName("groups", GroupId);
        //
        // Remove User from Group
        //
        public override void RemoveUser(string GroupNameIdOrGuid, int removeUserId)
        {
            int groupID = GetId(GroupNameIdOrGuid);
            int userId = removeUserId;
            if (groupID != 0) {
                if (userId == 0) {
                    cp.Content.Delete("Member Rules", "((memberid=" + cp.User.Id.ToString() + ")and(groupid=" + groupID.ToString() + "))");
                } else {
                    cp.Content.Delete("Member Rules", "((memberid=" + removeUserId.ToString() + ")and(groupid=" + groupID.ToString() + "))");
                }
            }
        }
        //
        public override void RemoveUser(string GroupNameIdOrGuid) => RemoveUser(GroupNameIdOrGuid, 0);
        //
        private void appendDebugLog(string copy) => LogController.logDebug(core, copy);
        //
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPGroupClass() {
            Dispose(false);
            
            
        }
        #endregion
    }
}