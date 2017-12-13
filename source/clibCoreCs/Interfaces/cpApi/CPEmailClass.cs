
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
namespace Contensive.Core {
    public class CPEmailClass : BaseClasses.CPEmailBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "7D2901F1-B5E8-4293-9373-909FDA6C7749";
        public const string InterfaceId = "2DC385E8-C4E7-4BBF-AE6D-F0FC5E2AA3C1";
        public const string EventsId = "32E893C5-165B-4088-8D9E-CE82524A5000";
        #endregion
        //
        private Contensive.Core.coreClass cpCore;
        protected bool disposed = false;
        //
        public CPEmailClass(Contensive.Core.coreClass cpCoreObj) : base() {
            cpCore = cpCoreObj;
        }
        //
        // dispose
        //
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                appendDebugLog(".dispose, dereference main, csv");
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    cpCore = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }

        public override string fromAddressDefault {
            get {
                return cpCore.siteProperties.getText("EMAILFROMADDRESS");
            }
        }
        //==========================================================================================
        /// <summary>
        /// Send email to an email address.
        /// </summary>
        /// <param name="ToAddress"></param>
        /// <param name="FromAddress"></param>
        /// <param name="Subject"></param>
        /// <param name="Body"></param>
        /// <param name="SendImmediately"></param>
        /// <param name="BodyIsHTML"></param>
        public override void send(string ToAddress, string FromAddress, string Subject, string Body, bool SendImmediately = true, bool BodyIsHTML = true) {
            try {
                cpCore.email.send(ToAddress, FromAddress, Subject, Body, "", "", "", SendImmediately, BodyIsHTML, 0);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //====================================================================================================
        /// <summary>
        /// Send submitted form within an email
        /// </summary>
        /// <param name="ToAddress"></param>
        /// <param name="FromAddress"></param>
        /// <param name="Subject"></param>
        public override void sendForm(string ToAddress, string FromAddress, string Subject) {
            try {
                cpCore.email.sendForm(ToAddress, FromAddress, Subject);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //====================================================================================================
        /// <summary>
        /// Send email to a list of groups
        /// </summary>
        /// <param name="GroupList"></param>
        /// <param name="FromAddress"></param>
        /// <param name="Subject"></param>
        /// <param name="Body"></param>
        /// <param name="SendImmediately"></param>
        /// <param name="BodyIsHTML"></param>
        public override void sendGroup(string GroupList, string FromAddress, string Subject, string Body, bool SendImmediately = true, bool BodyIsHTML = true) {
            try {
                cpCore.email.sendGroup(GroupList, FromAddress, Subject, Body, SendImmediately, BodyIsHTML);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }

        public override void sendPassword(string UserEmailAddress) //Inherits BaseClasses.CPEmailBaseClass.SendPassword
        {
            if (true) {
                cpCore.email.sendPassword(UserEmailAddress);
            }
        }

        public override void sendSystem(string EmailName, string AdditionalCopy = "", int AdditionalUserID = 0) {
            cpCore.email.sendSystem(EmailName, AdditionalCopy, AdditionalUserID);
        }

        public override void sendUser(string toUserId, string FromAddress, string Subject, string Body, bool SendImmediately = true, bool BodyIsHTML = true) //Inherits BaseClasses.CPEmailBaseClass.SendUser
        {
            int userId = 0;
            if (toUserId.IsNumeric()) {
                userId = int.Parse(toUserId);
                cpCore.email.sendPerson(userId, FromAddress, Subject, Body, SendImmediately, BodyIsHTML, 0, "", false);
            }
        }
        private void appendDebugLog(string copy) {
            //My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.email, " & copy & vbCrLf, True)
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
        ~CPEmailClass() {
            Dispose(false);
            //INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }

}