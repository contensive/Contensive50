
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
namespace Contensive.Core {
    public class CPEmailClass : BaseClasses.CPEmailBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "7D2901F1-B5E8-4293-9373-909FDA6C7749";
        public const string InterfaceId = "2DC385E8-C4E7-4BBF-AE6D-F0FC5E2AA3C1";
        public const string EventsId = "32E893C5-165B-4088-8D9E-CE82524A5000";
        #endregion
        //
        private Contensive.Core.Controllers.coreController core;
        protected bool disposed = false;
        //
        //==========================================================================================
        //
        public CPEmailClass(Contensive.Core.Controllers.coreController coreObj) : base() {
            core = coreObj;
        }
        //
        //==========================================================================================
        //
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    core = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        //==========================================================================================
        //
        public override string fromAddressDefault {
            get {
                return core.siteProperties.getText("EMAILFROMADDRESS");
            }
        }
        //
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
                string sendStatus = "";
                emailController.queueAdHocEmail(core, ToAddress, FromAddress, Subject, Body, "", "", "", SendImmediately, BodyIsHTML, 0, ref sendStatus);
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Send submitted form within an email
        /// </summary>
        /// <param name="ToAddress"></param>
        /// <param name="FromAddress"></param>
        /// <param name="Subject"></param>
        public override void sendForm(string ToAddress, string FromAddress, string Subject) {
            try {
                emailController.queueFormEmail(core, ToAddress, FromAddress, Subject);
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
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
                emailController.queueGroupEmail(core, GroupList, FromAddress, Subject, Body, SendImmediately, BodyIsHTML);
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void sendPassword(string UserEmailAddress) {
            string sendStatus = "";
            loginController.sendPassword(core, UserEmailAddress, ref sendStatus);
        }
        //
        //====================================================================================================
        //
        public override void sendSystem(string EmailName, string AdditionalCopy = "", int AdditionalUserID = 0) {
            emailController.queueSystemEmail(core, EmailName, AdditionalCopy, AdditionalUserID);
        }
        //
        //====================================================================================================
        //
        public override void sendUser(string toUserId, string FromAddress, string Subject, string Body, bool SendImmediately = true, bool BodyIsHTML = true) {
            int userId = 0;
            if (toUserId.IsNumeric()) {
                userId = int.Parse(toUserId);
                personModel person = personModel.create(core, userId);
                if ( person != null ) {
                    string sendStatus = "";
                    string queryStringForLinkAppend = "";
                    emailController.queuePersonEmail(core, person, FromAddress, Subject, Body, "", "", SendImmediately, BodyIsHTML, 0, "", false, ref sendStatus, queryStringForLinkAppend);
                }
            }
        }
        //
        //====================================================================================================
        //
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPEmailClass() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }

}