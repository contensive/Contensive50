﻿
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
using Contensive.Core.Models.Context;
//
namespace Contensive.Core {
    public class CPUserClass : BaseClasses.CPUserBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "08EF64C6-9C51-4B32-84D9-0D3BDAF42A28";
        public const string InterfaceId = "B1A95B1F-A00D-4AC6-B3A0-B1619568C2EA";
        public const string EventsId = "DBE2B6CB-6339-4FFB-92D7-BE37AEA841CC";
        #endregion
        //
        private Contensive.Core.coreClass cpCore;
        private CPClass CP;
        protected bool disposed = false;
        //
        //====================================================================================================
        //
        public CPUserClass(Contensive.Core.coreClass cpCoreObj, CPClass CPParent) : base() {
            cpCore = cpCoreObj;
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
                    cpCore = null;
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
                return CP.core.doc.sessionContext.user.Email;
            }
        }
        //
        //====================================================================================================
        //
        public override int GetIdByLogin(string Username, string Password) {
            return CP.core.doc.sessionContext.authenticateGetId(cpCore, Username, Password);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Returns the id of the user in the current session context. If 0, this action will create a user.
        /// This trigger allows sessions with guest detection disabled that will enable if used.
        /// </summary>
        public override int Id {
            get {
                int localId = CP.core.doc.sessionContext.user.id;
                if (localId == 0) {
                    localId = CP.core.db.insertContentRecordGetID("people", 0);
                    CP.core.doc.sessionContext.recognizeById(cpCore, localId, ref CP.core.doc.sessionContext);
                }
                return localId;
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsAdmin {
            get {
                return CP.core.doc.sessionContext.isAuthenticatedAdmin(cpCore);
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsAdvancedEditing(string ContentName) {
            return CP.core.doc.sessionContext.isAdvancedEditing(cpCore, ContentName);
        }
        //
        //====================================================================================================
        //
        public override bool IsAuthenticated {
            get {
                return (CP.core.doc.sessionContext.isAuthenticated);
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsAuthoring(string ContentName) {
            return CP.core.doc.sessionContext.isEditing(ContentName);
        }
        //
        //====================================================================================================
        //
        public override bool IsContentManager(string ContentName = "Page Content") {
            return CP.core.doc.sessionContext.isAuthenticatedContentManager(cpCore, ContentName);
        }
        //
        //====================================================================================================
        //
        public override bool IsDeveloper {
            get {
                return CP.core.doc.sessionContext.isAuthenticatedDeveloper(cpCore);
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsEditing(string ContentName) {
            return CP.core.doc.sessionContext.isEditing(ContentName);
        }
        //
        //====================================================================================================
        //
        public override bool IsEditingAnything {
            get {
                return CP.core.doc.sessionContext.isEditingAnything();
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsGuest {
            get {
                return CP.core.doc.sessionContext.isGuest(cpCore);
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
                CP.core.handleException(ex);
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
                result = CP.core.doc.sessionContext.isMemberOfGroupIdList(cpCore, userId, IsAuthenticated, GroupIDList, false);
            } catch (Exception ex) {
                CP.core.handleException(ex);
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
                return CP.core.doc.sessionContext.isAuthenticatedMember(cpCore);
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsQuickEditing(string ContentName) {
            return CP.core.doc.sessionContext.isQuickEditing(cpCore, ContentName);
        }
        //
        //====================================================================================================
        //
        public override bool IsRecognized {
            get {
                return CP.core.doc.sessionContext.isRecognized(cpCore);
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsWorkflowRendering {
            get {
                return CP.core.doc.sessionContext.isWorkflowRendering();
            }
        }
        //
        //====================================================================================================
        //
        public override string Language {
            get {
                if (CP.core.doc.sessionContext.userLanguage != null) {
                    return CP.core.doc.sessionContext.userLanguage.name;
                }
                return string.Empty;
            }
        }
        //
        //====================================================================================================
        //
        public override int LanguageID {
            get {
                return CP.core.doc.sessionContext.user.LanguageID;
            }
        }
        //
        //====================================================================================================
        //
        public override bool Login(string UsernameOrEmail, string Password, bool SetAutoLogin = false) {
            return CP.core.doc.sessionContext.authenticate(cpCore, UsernameOrEmail, Password, SetAutoLogin);
        }
        //
        //====================================================================================================
        //
        [Obsolete("Use LoginById(integer) instead", false)]
        public override bool LoginByID(string RecordID, bool SetAutoLogin = false) {
            return CP.core.doc.sessionContext.authenticateById(cpCore, encodeInteger(RecordID), CP.core.doc.sessionContext);
        }
        //
        //====================================================================================================
        //
        public override bool LoginByID(int RecordID) {
            return CP.core.doc.sessionContext.authenticateById(cpCore, RecordID, CP.core.doc.sessionContext);
        }
        //
        //====================================================================================================
        //
        public override bool LoginByID(int RecordID, bool SetAutoLogin) {
            bool result = CP.core.doc.sessionContext.authenticateById(cpCore, RecordID, CP.core.doc.sessionContext);
            if (result) {
                CP.core.doc.sessionContext.user.AutoLogin = SetAutoLogin;
                CP.core.doc.sessionContext.user.save(cpCore);
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public override bool LoginIsOK(string UsernameOrEmail, string Password) {
            return CP.core.doc.sessionContext.isLoginOK(cpCore, UsernameOrEmail, Password);
        }
        //
        //====================================================================================================
        //
        public override void Logout() {
            CP.core.doc.sessionContext.logout(cpCore);
        }
        //
        //====================================================================================================
        //
        public override string Name {
            get {
                return CP.core.doc.sessionContext.user.name;
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsNew {
            get {
                return CP.core.doc.sessionContext.visit.MemberNew;
            }
        }
        //
        //====================================================================================================
        //
        public override bool IsNewLoginOK(string Username, string Password) {
            string errorMessage = "";
            int errorCode = 0;
            return CP.core.doc.sessionContext.isNewLoginOK(cpCore, Username, Password, ref errorMessage, ref errorCode);
        }
        //
        //====================================================================================================
        //
        public override int OrganizationID {
            get {
                return CP.core.doc.sessionContext.user.OrganizationID;
            }
        }
        //
        //====================================================================================================
        //
        public override string Password {
            get {
                return CP.core.doc.sessionContext.user.Password;
            }
        }
        //
        //====================================================================================================
        //
        public override bool Recognize(int UserID) {
            sessionContextModel authContext = CP.core.doc.sessionContext;
            return CP.core.doc.sessionContext.recognizeById(cpCore, UserID, ref authContext);
        }
        //
        //====================================================================================================
        //
        public override string Username {
            get {
                return CP.core.doc.sessionContext.user.Username;
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
            return CP.core.userProperty.getBoolean(PropertyName, genericController.encodeBoolean(DefaultValue));
        }
        //
        //=======================================================================================================
        //  todo REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
        //=======================================================================================================
        //
        public override DateTime GetDate(string PropertyName, string DefaultValue = "") {
            return CP.core.userProperty.getDate(PropertyName, genericController.encodeDate(DefaultValue));
        }
        //
        //=======================================================================================================
        // todo  REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
        //=======================================================================================================
        //
        public override int GetInteger(string PropertyName, string DefaultValue = "") {
            return CP.core.userProperty.getInteger(PropertyName, genericController.encodeInteger(DefaultValue));
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