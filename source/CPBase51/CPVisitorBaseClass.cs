
using System;

namespace Contensive.BaseClasses {
    public abstract class CPVisitorBaseClass {
        public abstract bool ForceBrowserMobile { get; }
        public abstract string GetProperty(string key, string DefaultValue = "", int TargetVisitorId = 0);
        public abstract string GetText(string key, string defaultValue = "");
        public abstract bool GetBoolean(string key, string defaultValue = "");
        public abstract DateTime GetDate(string key, string defaultValue = "");
        public abstract int GetInteger(string key, string defaultValue = "");
        public abstract double GetNumber(string key, string defaultValue = "");
        public abstract int Id { get; }
        public abstract bool IsNew { get; }
        public abstract void SetProperty(string key, string value, int targetVisitorid = 0);
        public abstract int UserId { get; }
        //
        //====================================================================================================
        // deprecated
        //
    }
}

