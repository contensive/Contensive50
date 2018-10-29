
using System;
using Contensive.Processor.Controllers;

namespace Contensive.Processor {
    public class CPVisitClass : BaseClasses.CPVisitBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "3562FB08-178D-4AD1-A923-EAEAAF33FE84";
        public const string InterfaceId = "A1CC6FCB-810B-46C4-8232-D3166CACCBAD";
        public const string EventsId = "2AFEB1A8-5B27-45AC-A9DF-F99849BE1FAE";
        #endregion
        //
        private Contensive.Processor.Controllers.CoreController core;
        private CPClass cp;
        protected bool disposed = false;
        //
        public CPVisitClass(Contensive.Processor.Controllers.CoreController coreObj, CPClass cpParent) : base() {
            this.core = coreObj;
            cp = cpParent;
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
                    cp = null;
                    core = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }

        public override bool CookieSupport
        {
            get {
                return core.session.visit.cookieSupport;
            }
        }
        //
        //
        //
        public override string GetProperty(string PropertyName, string DefaultValue = "", int TargetVisitId = 0) {
            if (TargetVisitId == 0) {
                return core.visitProperty.getText(PropertyName, DefaultValue);
            } else {
                return core.visitProperty.getText(PropertyName, DefaultValue, TargetVisitId);
            }
        }
        //
        //
        //
        public override int Id {
            get {
                return core.session.visit.id;
            }
        }
        //
        //=======================================================================================================
        //
        public override DateTime LastTime
        {
            get {
                return core.session.visit.lastVisitTime;
            }
        }
        //
        //=======================================================================================================
        //
        public override int LoginAttempts
        {
            get {
                return core.session.visit.loginAttempts;
            }
        }
        //
        //=======================================================================================================
        //
        public override string Name
        {
            get {
                return core.session.visit.name;
            }
        }
        //
        //=======================================================================================================
        //
        public override int Pages
        {
            get {
                return core.session.visit.pageVisits;
            }
        }
        //
        //=======================================================================================================
        //
        public override string Referer
        {
            get {
                return core.session.visit.http_referer;
            }
        }
        //
        //=======================================================================================================
        //
        public override void SetProperty(string PropertyName, string Value, int TargetVisitId = 0) {
            if (TargetVisitId == 0) {
                core.visitProperty.setProperty(PropertyName, Value);
            } else {
                core.visitProperty.setProperty(PropertyName, Value, TargetVisitId);
            }
        }
        //
        //=======================================================================================================
        //
        public override bool GetBoolean(string PropertyName, string DefaultValue = "") {
            return GenericController.encodeBoolean(GetProperty(PropertyName, DefaultValue));
        }
        //
        //=======================================================================================================
        //
        //=======================================================================================================
        //
        public override DateTime GetDate(string PropertyName, string DefaultValue = "") {
            return GenericController.encodeDate(GetProperty(PropertyName, DefaultValue));
        }
        //
        //=======================================================================================================
        //
        //=======================================================================================================
        //
        public override int GetInteger(string PropertyName, string DefaultValue = "") {
            return cp.Utils.EncodeInteger(GetProperty(PropertyName, DefaultValue));
        }
        //
        //=======================================================================================================
        //
        //=======================================================================================================
        //
        public override double GetNumber(string PropertyName, string DefaultValue = "") {
            return cp.Utils.EncodeNumber(GetProperty(PropertyName, DefaultValue));
        }
        //
        //=======================================================================================================
        //
        //=======================================================================================================
        //
        public override string GetText(string FieldName, string DefaultValue = "") {
            return GetProperty(FieldName, DefaultValue);
        }

        public override int StartDateValue
        {
            get {
                return core.session.visit.startDateValue;
            }
        }

        public override DateTime StartTime
        {
            get {
                return core.session.visit.startTime;
            }
        }
        //
        //
        //
        private void appendDebugLog(string copy) {
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
        ~CPVisitClass() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}