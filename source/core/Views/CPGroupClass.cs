
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
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
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
        private Contensive.Processor.Controllers.coreController core;
        private CPClass cp;
        protected bool disposed = false;
        //
        // Constructor
        //
        public CPGroupClass(CPClass cpParent) : base() {
            cp = cpParent;
            core = cp.core;
        }
        //
        // dispose
        //
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                appendDebugLog(".dispose, dereference cp, main, csv");
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    cp = null;
                    core = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        // Add
        //
        public override void Add(string GroupNameOrGuid, string groupCaption = "") {
            try {
                groupController.group_add2(core, GroupNameOrGuid, groupCaption);
            } catch (Exception ex) {
                logController.handleError( core,ex, "Unexpected error in cp.group.add()");
            }
        }
        //
        // Add User
        //
        public override void AddUser(string GroupNameIdOrGuid) {
            groupController.group_AddUser(core, GroupNameIdOrGuid, 0, DateTime.MinValue);
        }
        public override void AddUser(string GroupNameIdOrGuid, int UserId) {
            groupController.group_AddUser(core, GroupNameIdOrGuid, UserId, DateTime.MinValue);
        }
        public override void AddUser(string GroupNameIdOrGuid, int UserId, DateTime DateExpires) {
            try {
                groupController.group_AddUser(core, GroupNameIdOrGuid, UserId, DateExpires);
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
        }
        //
        // Delete Group
        //
        public override void Delete(string GroupNameIdOrGuid) {
            try {
                groupModel.delete(core, GroupNameIdOrGuid);
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
        }
        //
        // Get Group ID
        //
        public override int GetId(string GroupNameIdOrGuid) {
            int returnInteger = 0;
            try {
                returnInteger = core.db.getRecordID("groups", GroupNameIdOrGuid);
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return returnInteger;
        }
        //
        // Get Group Name
        //
        public override string GetName(string GroupIdOrGuid) {
            string returnText = "";
            try {
                if (GroupIdOrGuid.IsNumeric()) {
                    //
                    // -- record Id
                    returnText = core.db.getRecordName("groups", genericController.encodeInteger(GroupIdOrGuid));
                } else {
                    //
                    // -- record guid
                    returnText = core.db.getRecordName("groups", GroupIdOrGuid);
                }
            } catch (Exception) {

            }
            return returnText;
        }
        //
        // Remove User from Group
        //
        public override void RemoveUser(string GroupNameIdOrGuid, int removeUserId = 0) //Inherits BaseClasses.CPGroupBaseClass.RemoveUser
        {
            int GroupID = 0;
            int userId = removeUserId;
            //
            GroupID = GetId(GroupNameIdOrGuid);
            if (GroupID != 0) {
                if (userId == 0) {
                    userId = cp.User.Id;
                }
                cp.Content.Delete("Member Rules", "((memberid=" + removeUserId.ToString() + ")and(groupid=" + GroupID.ToString() + "))");
            }
        }
        //
        //
        //
        private void appendDebugLog(string copy) {
            //My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.group, " & copy & vbCrLf, True)
            // 'My.Computer.FileSystem.WriteAllText(System.AppDocmc.main_CurrentDocmc.main_BaseDirectory() & "cpLog.txt", Now & " - " & copy & vbCrLf, True)
        }
        //
        // testpoint
        //
        private void tp(string msg) {
            //Call appendDebugLog(msg)
        }
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPGroupClass() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}