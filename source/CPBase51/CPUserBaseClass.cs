
using System;

namespace Contensive.BaseClasses {
    public abstract class CPUserBaseClass {
        public abstract int GetIdByLogin(string username, string password); 
        public abstract void Track();
        public abstract bool IsAdvancedEditing(string contentName);
        public abstract bool IsAuthenticated { get; }
        public abstract bool IsAuthoring(string contentName);
        public abstract bool IsEditing(string contentName);
        public abstract bool IsEditingAnything { get; }
        public abstract bool IsGuest { get; }
        public abstract bool IsQuickEditing(string contentName);
        public abstract bool IsRecognized { get; }
        public abstract bool IsWorkflowRendering { get; }
        public abstract bool IsNew { get; }
        //
        public abstract bool IsAdmin { get; }
        public abstract bool IsContentManager(string contentName = "Page Content");
        public abstract bool IsDeveloper { get; }
        public abstract bool IsInGroup(string groupName, int checkUserID = 0);
        public abstract bool IsInGroupList(string groupIdList, int checkUserID = 0);
        public abstract bool IsMember { get; }
        public abstract bool Recognize(int userID);
        //
        public abstract bool Login(string usernameOrEmail, string password, bool setAutoLogin = false);
        public abstract bool LoginByID(int recordId);
        public abstract bool LoginByID(int recordId, bool setAutoLogin);
        public abstract bool LoginIsOK(string usernameOrEmail, string password);
        public abstract void Logout();
        public abstract bool IsNewLoginOK(string username, string password);
        //
        public abstract string Language { get; }
        public abstract int LanguageID { get; }
        public abstract string Email { get; }
        public abstract int Id { get; }
        public abstract string Name { get; }
        public abstract int OrganizationID { get; }
        public abstract string Password { get; }
        public abstract string Username { get; }
        //
        public abstract string GetProperty(string key, string defaultValue = "", int targetVisitId = 0);
        public abstract string GetText(string key);
        public abstract string GetText(string key, string defaultValue);
        public abstract bool GetBoolean(string key);
        public abstract bool GetBoolean(string key, bool defaultValue);
        public abstract DateTime GetDate(string key);
        public abstract DateTime GetDate(string key, DateTime defaultValue);
        public abstract int GetInteger(string key);
        public abstract int GetInteger(string key, int defaultValue);
        public abstract double GetNumber(string key);
        public abstract double GetNumber(string key, double defaultValue);
        public abstract void SetProperty(string key, string value, int targetVisitId = 0);
        //
        // -- deprecated
        //
        [Obsolete("Use LoginById(integer) instead", false)]
        public abstract bool LoginByID(string RecordID, bool SetAutoLogin = false);
        //
        [Obsolete("correct default type", true)]
        public abstract bool GetBoolean(string PropertyName, string DefaultValue);
        //
        [Obsolete("correct default type", true)]
        public abstract DateTime GetDate(string PropertyName, string DefaultValue);
        //
        [Obsolete("correct default type", true)]
        public abstract int GetInteger(string PropertyName, string DefaultValue);
        //
        [Obsolete("correct default type", true)]
        public abstract double GetNumber(string PropertyName, string DefaultValue);
        //
        //====================================================================================================
        // deprecated
        //
    }
}

