
using System;

namespace Contensive.BaseClasses {
    public abstract class CPVisitorBaseClass {
        public abstract bool ForceBrowserMobile { get; }
        public abstract string GetProperty(string PropertyName, string DefaultValue = "", int TargetVisitorId = 0);
        public abstract string GetText(string PropertyName, string DefaultValue = "");
        public abstract bool GetBoolean(string PropertyName, string DefaultValue = "");
        public abstract DateTime GetDate(string PropertyName, string DefaultValue = "");
        public abstract int GetInteger(string PropertyName, string DefaultValue = "");
        public abstract double GetNumber(string PropertyName, string DefaultValue = "");
        //Public MustOverride Function IsProperty(ByVal PropertyName As String) As Boolean
        public abstract int Id { get; }
        public abstract bool IsNew { get; }
        public abstract void SetProperty(string PropertyName, string Value, int TargetVisitorid = 0);
        public abstract int UserId { get; }
        //
        //====================================================================================================
        // deprecated
        //
    }
}

