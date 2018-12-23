
using System;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using System.Collections.Generic;

namespace Contensive.Processor {
    public class CPEmailClass : BaseClasses.CPEmailBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "7D2901F1-B5E8-4293-9373-909FDA6C7749";
        public const string InterfaceId = "2DC385E8-C4E7-4BBF-AE6D-F0FC5E2AA3C1";
        public const string EventsId = "32E893C5-165B-4088-8D9E-CE82524A5000";
        #endregion
        /// <summary>
        /// dependencies
        /// </summary>
        private CPClass cp;
        //
        //==========================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cp"></param>
        public CPEmailClass(CPClass cp) : base() {
            this.cp = cp;
        }
        //
        //==========================================================================================
        //
        public override string FromAddressDefault {
            get {
                return cp.core.siteProperties.getText("EMAILFROMADDRESS");
            }
        }
        //
        //==========================================================================================
        /// <summary>
        /// Send email to an email address.
        /// </summary>
        /// <param name="toAddress"></param>
        /// <param name="fromAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="sendImmediately"></param>
        /// <param name="bodyIsHTML"></param>
        public override bool Send(string toAddress, string fromAddress, string subject, string body, bool sendImmediately, bool bodyIsHTML, ref string userErrorMessage) {
            try {
                return EmailController.queueAdHocEmail(cp.core, toAddress, fromAddress, subject, body, fromAddress, fromAddress, "", sendImmediately, bodyIsHTML,0 , ref userErrorMessage);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        public override bool Send(string toAddress, string fromAddress, string subject, string body, bool sendImmediately, bool bodyIsHtml ) {
            string userErrorMessage = "";
            return Send(toAddress, fromAddress, subject, body, sendImmediately, bodyIsHtml, ref userErrorMessage);
        }
        //
        public override bool Send(string toAddress, string fromAddress, string subject, string body, bool sendImmediately) {
            string userErrorMessage = "";
            return Send(toAddress, fromAddress, subject, body, sendImmediately, true, ref userErrorMessage);
        }
        //
        public override bool Send(string toAddress, string fromAddress, string subject, string body) {
            string userErrorMessage = "";
            return Send(toAddress, fromAddress, subject, body, true, true, ref userErrorMessage);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Send submitted form within an email
        /// </summary>
        /// <param name="toAddress"></param>
        /// <param name="fromAddress"></param>
        /// <param name="subject"></param>
        public override bool SendForm(string toAddress, string fromAddress, string subject, ref string userErrorMessage) {
            try {
                return EmailController.queueFormEmail(cp.core, toAddress, fromAddress, subject, ref userErrorMessage);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        public override bool SendForm(string toAddress, string fromAddress, string subject) {
            string userErrorMessage = "";
            return SendForm(toAddress, fromAddress, subject, ref userErrorMessage);
        }
        //
        //====================================================================================================
        //
        public override bool SendPassword(string UserEmailAddress, ref string userErrorMessage) {
            return LoginController.sendPassword(cp.core, UserEmailAddress, ref userErrorMessage);
        }
        //
        public override bool SendPassword(string UserEmailAddress) {
            string userErrorMessage = "";
            return SendPassword(UserEmailAddress, ref userErrorMessage);
        }
        //
        //====================================================================================================
        //
        public override bool SendSystem(string EmailName, string AdditionalCopy, int AdditionalUserID, ref string userErrorMessage) {
            return EmailController.queueSystemEmail(cp.core, EmailName, AdditionalCopy, AdditionalUserID, ref userErrorMessage);
        }
        //
        public override bool SendSystem(string EmailName, string AdditionalCopy, int AdditionalUserID) {
            return EmailController.queueSystemEmail(cp.core, EmailName, AdditionalCopy, AdditionalUserID);
        }
        //
        public override bool SendSystem(string EmailName, string AdditionalCopy) {
            return EmailController.queueSystemEmail(cp.core, EmailName, AdditionalCopy);
        }
        //
        public override bool SendSystem(string EmailName) {
            return EmailController.queueSystemEmail(cp.core, EmailName);
        }
        //
        public override bool SendSystem(int emailId, string additionalCopy, int additionalUserID, ref string userErrorMessage) {
            return EmailController.queueSystemEmail(cp.core, emailId, additionalCopy, additionalUserID);
        }
        //
        public override bool SendSystem(int emailId, string additionalCopy, int additionalUserID) {
            return EmailController.queueSystemEmail(cp.core, emailId, additionalCopy, additionalUserID);
        }
        //
        public override bool SendSystem(int emailId, string additionalCopy) {
            return EmailController.queueSystemEmail(cp.core, emailId, additionalCopy);
        }
        //
        public override bool SendSystem(int emailId) {
            return EmailController.queueSystemEmail(cp.core, emailId);
        }
        //
        //====================================================================================================
        //
        public override bool SendToGroup(string groupName, string fromAddress, string subject, string body, bool sendImmediately, bool bodyIsHtml, ref string userErrorMessage) {
            return EmailController.queueGroupEmail(cp.core, new List<string>() { groupName }, fromAddress, subject, body, sendImmediately, bodyIsHtml, ref userErrorMessage);
        }
        //
        public override bool SendToGroup(string groupName, string fromAddress, string subject, string body, bool sendImmediately, bool bodyIsHtml) {
            string userErrorMessage = "";
            return EmailController.queueGroupEmail(cp.core, new List<string>() { groupName }, fromAddress, subject, body, sendImmediately, bodyIsHtml, ref userErrorMessage);
        }
        //
        public override bool SendToGroup(string groupName, string fromAddress, string subject, string body, bool sendImmediately) {
            string userErrorMessage = "";
            return EmailController.queueGroupEmail(cp.core, new List<string>() { groupName }, fromAddress, subject, body, sendImmediately, true, ref userErrorMessage);
        }
        //
        public override bool SendToGroup(string groupName, string fromAddress, string subject, string body) {
            string userErrorMessage = "";
            return EmailController.queueGroupEmail(cp.core, new List<string>() { groupName }, fromAddress, subject, body, true, true, ref userErrorMessage);
        }
        //
        //====================================================================================================
        //
        public override bool SendToGroup(int groupId, string fromAddress, string subject, string body, bool sendImmediately, bool bodyIsHtml, ref string userErrorMessage) {
            return EmailController.queueGroupEmail(cp.core, new List<int>() { groupId }, fromAddress, subject, body, sendImmediately, bodyIsHtml, ref userErrorMessage);
        }
        //
        public override bool SendToGroup(int groupId, string fromAddress, string subject, string body, bool sendImmediately, bool bodyIsHtml) {
            string userErrorMessage = "";
            return EmailController.queueGroupEmail(cp.core, new List<int>() { groupId }, fromAddress, subject, body, sendImmediately, bodyIsHtml, ref userErrorMessage);
        }
        //
        public override bool SendToGroup(int groupId, string fromAddress, string subject, string body, bool sendImmediately) {
            string userErrorMessage = "";
            return EmailController.queueGroupEmail(cp.core, new List<int>() { groupId }, fromAddress, subject, body, sendImmediately, true, ref userErrorMessage);
        }
        //
        public override bool SendToGroup(int groupId, string fromAddress, string subject, string body) {
            string userErrorMessage = "";
            return EmailController.queueGroupEmail(cp.core, new List<int>() { groupId }, fromAddress, subject, body, true, true, ref userErrorMessage);
        }
        //
        //====================================================================================================
        //
        public override bool SendToGroup(List<string> groupNameList, string fromAddress, string subject, string body, bool sendImmediately, bool bodyIsHtml, ref string userErrorMessage) {
            return EmailController.queueGroupEmail(cp.core, groupNameList, fromAddress, subject, body, sendImmediately, bodyIsHtml, ref userErrorMessage);
        }
        //
        public override bool SendToGroup(List<string> groupNameList, string fromAddress, string subject, string body, bool sendImmediately, bool bodyIsHtml) {
            string userErrorMessage = "";
            return EmailController.queueGroupEmail(cp.core, groupNameList, fromAddress, subject, body, sendImmediately, bodyIsHtml, ref userErrorMessage);
        }
        //
        public override bool SendToGroup(List<string> groupNameList, string fromAddress, string subject, string body, bool sendImmediately) {
            string userErrorMessage = "";
            return EmailController.queueGroupEmail(cp.core, groupNameList, fromAddress, subject, body, sendImmediately, true, ref userErrorMessage);
        }
        //
        public override bool SendToGroup(List<string> groupNameList, string fromAddress, string subject, string body) {
            string userErrorMessage = "";
            return EmailController.queueGroupEmail(cp.core, groupNameList, fromAddress, subject, body, true, true, ref userErrorMessage);
        }
        //
        //====================================================================================================
        //
        public override bool SendToGroup(List<int> groupIdList, string fromAddress, string subject, string body, bool sendImmediately, bool bodyIsHtml, ref string userErrorMessage) {
            return EmailController.queueGroupEmail(cp.core, groupIdList, fromAddress, subject, body, sendImmediately, bodyIsHtml, ref userErrorMessage);
        }
        //
        public override bool SendToGroup(List<int> groupIdList, string fromAddress, string subject, string body, bool sendImmediately, bool bodyIsHtml) {
            string userErrorMessage = "";
            return EmailController.queueGroupEmail(cp.core, groupIdList, fromAddress, subject, body, sendImmediately, bodyIsHtml, ref userErrorMessage);
        }
        //
        public override bool SendToGroup(List<int> groupIdList, string fromAddress, string subject, string body, bool sendImmediately) {
            string userErrorMessage = "";
            return EmailController.queueGroupEmail(cp.core, groupIdList, fromAddress, subject, body, sendImmediately, true, ref userErrorMessage);
        }
        //
        public override bool SendToGroup(List<int> groupIdList, string fromAddress, string subject, string body) {
            string userErrorMessage = "";
            return EmailController.queueGroupEmail(cp.core, groupIdList, fromAddress, subject, body, true, true, ref userErrorMessage);
        }
        //
        //====================================================================================================
        //
        public override bool SendUser(int toUserId, string fromAddress, string subject, string body, bool sendImmediately, bool bodyIsHtml, ref string userErrorMessage) {
            PersonModel person = PersonModel.create(cp.core, toUserId);
            if ( person == null ) {
                userErrorMessage = "An email could not be sent because the user could not be located.";
                return false;
            }
            return EmailController.queuePersonEmail(cp.core, person, fromAddress, subject, body, "", "", sendImmediately, bodyIsHtml, 0,"",false, ref userErrorMessage);
        }
        //
        public override bool SendUser(int toUserId, string fromAddress, string subject, string body, bool sendImmediately, bool bodyIsHtml) {
            string userErrorMessage = "";
            return SendUser(toUserId, fromAddress, subject, body, sendImmediately, bodyIsHtml, ref userErrorMessage);

        }
        //
        public override bool SendUser(int toUserId, string fromAddress, string subject, string body, bool sendImmediately) {
            string userErrorMessage = "";
            return SendUser(toUserId, fromAddress, subject, body, sendImmediately, true, ref userErrorMessage);
        }
        //
        public override bool SendUser(int toUserId, string fromAddress, string subject, string body) {
            string userErrorMessage = "";
            return SendUser(toUserId, fromAddress, subject, body, true, true, ref userErrorMessage);
        }
        //
        //====================================================================================================
        //
        [Obsolete("Use uppercase method", true)]
        public override string fromAddressDefault => FromAddressDefault;
        //
        [Obsolete("Use SendToGroup", true)]
        public override void sendGroup(string GroupCommaList, string FromAddress, string Subject, string Body, bool SendImmediately, bool BodyIsHTML) {
            EmailController.queueGroupEmail(cp.core, GroupCommaList, FromAddress, Subject, Body, SendImmediately, BodyIsHTML);
        }
        //
        [Obsolete("Use SendToGroup", true)]
        public override void sendGroup(string GroupCommaList, string FromAddress, string Subject, string Body, bool SendImmediately) {
            EmailController.queueGroupEmail(cp.core, GroupCommaList, FromAddress, Subject, Body, SendImmediately, true);
        }
        //
        [Obsolete("Use SendToGroup", true)]
        public override void sendGroup(string GroupCommaList, string FromAddress, string Subject, string Body) {
            EmailController.queueGroupEmail(cp.core, GroupCommaList, FromAddress, Subject, Body, true, true);
        }
        //
        [Obsolete("Use uppercase method", true)]
        public override void sendPassword(string userEmailAddress) 
            => SendPassword(userEmailAddress);
        //
        [Obsolete("Use uppercase method", true)]
        public override void send(string toAddress, string fromAddress, string subject, string body, bool sendImmediately, bool bodyIsHTML)
            => Send(toAddress, fromAddress, subject, body, sendImmediately, bodyIsHTML);
        //
        [Obsolete("Use uppercase method", true)]
        public override void send(string toAddress, string fromAddress, string subject, string body, bool sendImmediately)
            => Send(toAddress, fromAddress, subject, body, sendImmediately, true);
        //
        [Obsolete("Use uppercase method", true)]
        public override void send(string toAddress, string fromAddress, string subject, string body)
            => Send(toAddress, fromAddress, subject, body, true, true);
        //
        [Obsolete("Use uppercase method", true)]
        public override void sendForm(string toAddress, string fromAddress, string subject)
            => SendForm(toAddress, fromAddress, subject);
        //
        [Obsolete("Use uppercase method", true)]
        public override void sendSystem(string emailIdOrName, string additionalCopy = "", int additionalUserID = 0)
            => SendSystem(emailIdOrName, additionalCopy, additionalUserID);
        //
        [Obsolete("Use uppercase method, SendUser(int,string,string,string,bool,bool)", true)]
        public override void sendUser(string toUserID, string fromAddress, string subject, string body, bool sendImmediately, bool bodyIsHTML)
            => SendUser(GenericController.encodeInteger(toUserID), fromAddress, subject, body, sendImmediately, bodyIsHTML);
        //
        [Obsolete("Use uppercase method, SendUser(int,string,string,string,bool)", true)]
        public override void sendUser(string toUserID, string fromAddress, string subject, string body, bool sendImmediately)
            => SendUser(GenericController.encodeInteger(toUserID), fromAddress, subject, body, sendImmediately, true);
        //
        [Obsolete("Use uppercase method, SendUser(int,string,string,string,bool,bool)", true)]
        public override void sendUser(string toUserID, string fromAddress, string subject, string body)
            => SendUser(GenericController.encodeInteger(toUserID), fromAddress, subject, body, true, true);
        //
        //====================================================================================================
        //
        #region  IDisposable Support 
        //
        //==========================================================================================
        //
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
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
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPEmailClass() {
            Dispose(false);
        }
        protected bool disposed = false;
        #endregion
    }

}