
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
using static Contensive.Processor.constants;
using Contensive.Processor.Models.Domain;
//
namespace Contensive.Processor {
    public class CPUserClass : BaseClasses.CPUserBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "08EF64C6-9C51-4B32-84D9-0D3BDAF42A28";
        public const string InterfaceId = "B1A95B1F-A00D-4AC6-B3A0-B1619568C2EA";
        public const string EventsId = "DBE2B6CB-6339-4FFB-92D7-BE37AEA841CC";
        #endregion
        //
        private Contensive.Processor.Controllers.CoreController core;
        private CPClass CP;
        protected bool disposed = false;
        //
        //====================================================================================================
        //
        public CPUserClass(Contensive.Processor.Controllers.CoreController coreObj, CPClass CPParent) : base() {
            core = coreObj;
            CP = CPParent;
        }
        //
        //====================================================================================================
        //
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                appendDebugLog(".dispose, dereference cp, main, csv");
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    CP = null;
                    core = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        //====================================================================================================
        //
        public override string Email
        {
            get {
                return CP.core.session.user.Email;
            }
        }
        //
        //====================================================================================================
        //
        public override int GetIdByLogin(string Username, string Password) {
            return CP.core.session.getUserIdForCredentials(core, Username, Password);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Returns the id of the user in the current session context. If 0, this action will create a user.
        /// This trigger allows sessions with guest detection disabled that will enable if used.
        /// </summary>
        public override int Id {
            get {
                if (CP.core.session.user.id==0) {
                    var user = PersonModel.add(core);
                    user.CreatedByVisit = true;
                    user.save(core);
                    SessionController.recognizeById(core, user.id, ref CP.core.session);
                }
                return core.session.user.id;
                //int localId = CP.core.session.user.id;
                //if (localId == 0) {
                //    localId = CP.core.db.insertContentRecordGetID("people", 0);
                //    sessionController.recognizeById(core, localId, ref CP.core.session);
                //}
                //return localId;
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsAdmin {
            get {
                return CP.core.session.isAuthenticatedAdmin(core);
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsAdvancedEditing(string ContentName) {
            return CP.core.session.isAdvancedEditing(core, ContentName);
        }
        //
        //====================================================================================================
        //
        public override bool IsAuthenticated {
            get {
                return (CP.core.session.isAuthenticated);
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsAuthoring(string ContentName) {
            return CP.core.session.isEditing(ContentName);
        }
        //
        //====================================================================================================
        //
        public override bool IsContentManager(string ContentName = "Page Content") {
            return CP.core.session.isAuthenticatedContentManager(core, ContentName);
        }
        //
        //====================================================================================================
        //
        public override bool IsDeveloper {
            get {
                return CP.core.session.isAuthenticatedDeveloper(core);
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsEditing(string ContentName) {
            return CP.core.session.isEditing(ContentName);
        }
        //
        //====================================================================================================
        //
        public override bool IsEditingAnything {
            get {
                return CP.core.session.isEditingAnything();
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsGuest {
            get {
                return CP.core.session.isGuest(core);
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsInGroup(string groupName, int userId = 0) {
            int groupId = 0;
            bool result = false;
            //
            try {
                groupId = CP.Group.GetId(groupName);
                if (userId == 0) {
                    userId = Id;
                }
                if (groupId == 0) {
                    result = false;
                } else {
                    result = IsInGroupList(groupId.ToString(), userId);
                }
            } catch (Exception ex) {
                LogController.handleError(CP.core,ex);
                result = false;
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public override bool IsInGroupList(string GroupIDList, int userId = 0) //Inherits BaseClasses.CPUserBaseClass.IsInGroup
        {
            bool result = false;
            //
            try {
                if (userId == 0) {
                    userId = Id;
                }
                result = CP.core.session.isMemberOfGroupIdList(core, userId, IsAuthenticated, GroupIDList, false);
            } catch (Exception ex) {
                LogController.handleError(CP.core,ex);
                result = false;
            }
            return result;
            //
        }
        //
        //====================================================================================================
        //
        public override bool IsMember {
            get {
                return CP.core.session.isAuthenticatedMember(core);
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsQuickEditing(string ContentName) {
            return CP.core.session.isQuickEditing(core, ContentName);
        }
        //
        //====================================================================================================
        //
        public override bool IsRecognized {
            get {
                return CP.core.session.isRecognized(core);
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsWorkflowRendering {
            get {
                return CP.core.session.isWorkflowRendering();
            }
        }
        //
        //====================================================================================================
        //
        public override string Language {
            get {
                if (CP.core.session.userLanguage != null) {
                    return CP.core.session.userLanguage.name;
                }
                return string.Empty;
            }
        }
        //
        //====================================================================================================
        //
        public override int LanguageID {
            get {
                return CP.core.session.user.LanguageID;
            }
        }
        //
        //====================================================================================================
        //
        public override bool Login(string UsernameOrEmail, string Password, bool SetAutoLogin = false) {
            return CP.core.session.authenticate(core, UsernameOrEmail, Password, SetAutoLogin);
        }
        //
        //====================================================================================================
        //
        [Obsolete("Use LoginById(integer) instead", false)]
        public override bool LoginByID(string RecordID, bool SetAutoLogin = false) {
            return SessionController.authenticateById(core, encodeInteger(RecordID), CP.core.session);
        }
        //
        //====================================================================================================
        //
        public override bool LoginByID(int RecordID) {
            return SessionController.authenticateById(core, RecordID, CP.core.session);
        }
        //
        //====================================================================================================
        //
        public override bool LoginByID(int RecordID, bool SetAutoLogin) {
            bool result = SessionController.authenticateById(core, RecordID, CP.core.session);
            if (result) {
                CP.core.session.user.AutoLogin = SetAutoLogin;
                CP.core.session.user.save(core);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public override bool LoginIsOK(string UsernameOrEmail, string Password) {
            return CP.core.session.isLoginOK(core, UsernameOrEmail, Password);
        }
        //
        //====================================================================================================
        //
        public override void Logout() {
            CP.core.session.logout(core);
        }
        //
        //====================================================================================================
        //
        public override string Name {
            get {
                return CP.core.session.user.name;
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsNew {
            get {
                return CP.core.session.visit.memberNew;
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsNewLoginOK(string Username, string Password) {
            string errorMessage = "";
            int errorCode = 0;
            return CP.core.session.isNewCredentialOK(core, Username, Password, ref errorMessage, ref errorCode);
        }
        //
        //====================================================================================================
        //
        public override int OrganizationID {
            get {
                return CP.core.session.user.OrganizationID;
            }
        }
        //
        //====================================================================================================
        //
        public override string Password {
            get {
                return CP.core.session.user.Password;
            }
        }
        //
        //====================================================================================================
        //
        public override bool Recognize(int UserID) {
            return SessionController.recognizeById(core, UserID, ref CP.core.session);
        }
        //
        //====================================================================================================
        //
        public override string Username {
            get {
                return CP.core.session.user.Username;
            }
        }
        //
        //=======================================================================================================
        //
        public override string GetProperty(string PropertyName, string DefaultValue = "", int TargetMemberId = 0) {
            if (TargetMemberId == 0) {
                return CP.core.userProperty.getText(PropertyName, DefaultValue);
            } else {
                return CP.core.userProperty.getText(PropertyName, DefaultValue, TargetMemberId);
            }
        }
        //
        //=======================================================================================================
        //
        public override void SetProperty(string PropertyName, string Value, int TargetMemberId = 0) {
            if (TargetMemberId == 0) {
                CP.core.userProperty.setProperty(PropertyName, Value);
            } else {
                CP.core.userProperty.setProperty(PropertyName, Value, TargetMemberId);
            }
        }
        //
        //=======================================================================================================
        // todo REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
        //=======================================================================================================
        //
        public override bool GetBoolean(string PropertyName, string DefaultValue = "") {
            return CP.core.userProperty.getBoolean(PropertyName, GenericController.encodeBoolean(DefaultValue));
        }
        //
        //=======================================================================================================
        //  todo REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
        //=======================================================================================================
        //
        public override DateTime GetDate(string PropertyName, string DefaultValue = "") {
            return CP.core.userProperty.getDate(PropertyName, GenericController.encodeDate(DefaultValue));
        }
        //
        //=======================================================================================================
        // todo  REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
        //=======================================================================================================
        //
        public override int GetInteger(string PropertyName, string DefaultValue = "") {
            return CP.core.userProperty.getInteger(PropertyName, GenericController.encodeInteger(DefaultValue));
        }
        //
        //=======================================================================================================
        // todo  REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
        //=======================================================================================================
        //
        public override double GetNumber(string PropertyName, string DefaultValue = "") {
            return CP.core.userProperty.getNumber(PropertyName, encodeNumber(DefaultValue));
        }
        //
        //=======================================================================================================
        //
        public override string GetText(string PropertyName, string DefaultValue = "") {
            return CP.core.userProperty.getText(PropertyName, DefaultValue);
        }
        //
        //====================================================================================================
        // todo  obsolete
        //
        public override void Track() {
            int localId = Id;
        }
        //
        //====================================================================================================
        //
        private void appendDebugLog(string copy) {
            //
        }
        //
        //====================================================================================================
        //
        private void tp(string msg) {}
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
        ~CPUserClass() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }

}