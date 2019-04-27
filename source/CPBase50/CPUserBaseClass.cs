
using System;

namespace Contensive.BaseClasses {
    public abstract class CPUserBaseClass {
        public abstract int GetIdByLogin(string Username, string Password); //Implements BaseClasses.CPUserBaseClass.GetIdByLogin
        public abstract void Track();
        public abstract bool IsAdvancedEditing(string ContentName);
        public abstract bool IsAuthenticated { get; }
        public abstract bool IsAuthoring(string ContentName);
        public abstract bool IsEditing(string ContentName);
        public abstract bool IsEditingAnything { get; }
        public abstract bool IsGuest { get; }
        public abstract bool IsQuickEditing(string ContentName);
        public abstract bool IsRecognized { get; }
        public abstract bool IsWorkflowRendering { get; }
        public abstract bool IsNew { get; }
        //
        public abstract bool IsAdmin { get; }
        public abstract bool IsContentManager(string ContentName = "Page Content");
        public abstract bool IsDeveloper { get; }
        public abstract bool IsInGroup(string GroupName, int CheckUserID = 0);
        public abstract bool IsInGroupList(string GroupIDList, int CheckUserID = 0);
        public abstract bool IsMember { get; }
        public abstract bool Recognize(int UserID);
        //
        public abstract bool Login(string UsernameOrEmail, string Password, bool SetAutoLogin = false);
        [Obsolete("Use LoginById(integer) instead", false)] public abstract bool LoginByID(string RecordID, bool SetAutoLogin = false);
        public abstract bool LoginByID(int RecordID);
        public abstract bool LoginByID(int RecordID, bool SetAutoLogin);
        public abstract bool LoginIsOK(string UsernameOrEmail, string Password);
        public abstract void Logout();
        public abstract bool IsNewLoginOK(string Username, string Password);
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
        public abstract string GetProperty(string PropertyName, string DefaultValue = "", int TargetVisitId = 0);
        public abstract string GetText(string PropertyName);
        public abstract string GetText(string PropertyName, string DefaultValue);
        public abstract bool GetBoolean(string PropertyName);
        public abstract bool GetBoolean(string PropertyName, bool DefaultValue);
        public abstract DateTime GetDate(string PropertyName);
        public abstract DateTime GetDate(string PropertyName, DateTime DefaultValue);
        public abstract int GetInteger(string PropertyName);
        public abstract int GetInteger(string PropertyName, int DefaultValue);
        public abstract double GetNumber(string PropertyName);
        public abstract double GetNumber(string PropertyName, double DefaultValue);
        public abstract void SetProperty(string PropertyName, string Value, int TargetVisitId = 0);
        //
        // -- deprecated
        //
        [Obsolete("correct default type", false)]
        public abstract bool GetBoolean(string PropertyName, string DefaultValue);
        //
        [Obsolete("correct default type", false)]
        public abstract DateTime GetDate(string PropertyName, string DefaultValue);
        //
        [Obsolete("correct default type", false)]
        public abstract int GetInteger(string PropertyName, string DefaultValue);
        //
        [Obsolete("correct default type", false)]
        public abstract double GetNumber(string PropertyName, string DefaultValue);
        //
        //====================================================================================================
        // deprecated
        //
    }
}

