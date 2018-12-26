
using System;

namespace Contensive.BaseClasses {
    public abstract class CPVisitBaseClass {
        public abstract bool CookieSupport { get; }
        public abstract string GetProperty(string key, string defaultValue = "", int targetVisitId = 0);
        public abstract string GetText(string key, string defaultValue = "");
        public abstract bool GetBoolean(string key, string defaultValue = "");
        public abstract DateTime GetDate(string key, string defaultValue = "");
        public abstract int GetInteger(string key, string defaultValue = "");
        public abstract double GetNumber(string key, string defaultValue = "");
        public abstract int Id { get; }
        public abstract DateTime LastTime { get; }
        public abstract int LoginAttempts { get; }
        public abstract string Name { get; }
        public abstract int Pages { get; }
        public abstract string Referer { get; }
        public abstract void SetProperty(string key, string value, int targetVisitId = 0);
        public abstract int StartDateValue { get; }
        public abstract DateTime StartTime { get; }
        //
        //====================================================================================================
        // deprecated
        //
    }
}

