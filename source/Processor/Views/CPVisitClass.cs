
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
        private CPClass cp;
        //
        //=======================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cp"></param>
        public CPVisitClass(CPClass cp) {
            this.cp = cp;
        }
        //
        //=======================================================================================================
        /// <summary>
        /// return true if cookies supported
        /// </summary>
        public override bool CookieSupport {
            get {
                return cp.core.session.visit.cookieSupport;
            }
        }
        //
        //=======================================================================================================
        /// <summary>
        /// return the visit id
        /// </summary>
        public override int Id {
            get {
                return cp.core.session.visit.id;
            }
        }
        //
        //=======================================================================================================
        /// <summary>
        /// return the time of the last visit
        /// </summary>
        public override DateTime LastTime {
            get {
                return cp.core.session.visit.lastVisitTime;
            }
        }
        //
        //=======================================================================================================
        /// <summary>
        /// return the login attempts
        /// </summary>
        public override int LoginAttempts {
            get {
                return cp.core.session.visit.loginAttempts;
            }
        }
        //
        //=======================================================================================================
        /// <summary>
        /// return the name of the visit
        /// </summary>
        public override string Name {
            get {
                return cp.core.session.visit.name;
            }
        }
        //
        //=======================================================================================================
        /// <summary>
        /// return the number of hits
        /// </summary>
        public override int Pages {
            get {
                return cp.core.session.visit.pageVisits;
            }
        }
        //
        //=======================================================================================================
        /// <summary>
        /// return the referer
        /// </summary>
        public override string Referer {
            get {
                return cp.core.session.visit.http_referer;
            }
        }
        //
        //=======================================================================================================
        /// <summary>
        /// Set a key value pair for this visit
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public override void SetProperty(string key, string value) {
            cp.core.visitProperty.setProperty(key, value);
        }
        //
        public override void SetProperty(string key, int value) {
            cp.core.visitProperty.setProperty(key, value);
        }
        //
        public override void SetProperty(string key, double value) {
            cp.core.visitProperty.setProperty(key, value);
        }
        //
        public override void SetProperty(string key, bool value) {
            cp.core.visitProperty.setProperty(key, value);
        }
        //
        public override void SetProperty(string key, DateTime value) {
            cp.core.visitProperty.setProperty(key, value);
        }
        //
        //=======================================================================================================
        //
        public override bool GetBoolean(string key, bool defaultValue) {
            return cp.core.visitProperty.getBoolean(key, defaultValue);
        }
        //
        public override bool GetBoolean(string key) {
            return cp.core.visitProperty.getBoolean(key);
        }
        //
        //=======================================================================================================
        //
        public override DateTime GetDate(string key, DateTime defaultValue) {
            return cp.core.visitProperty.getDate(key, defaultValue);
        }
        //
        public override DateTime GetDate(string key) {
            return cp.core.visitProperty.getDate(key);
        }
        //
        //=======================================================================================================
        //
        public override int GetInteger(string key, int defaultValue) {
            return cp.core.visitProperty.getInteger(key, defaultValue);
        }
        //
        public override int GetInteger(string key) {
            return cp.core.visitProperty.getInteger(key);
        }
        //
        //=======================================================================================================
        //
        public override double GetNumber(string key, double defaultValue) {
            return cp.core.visitProperty.getNumber(key, defaultValue);
        }
        //
        public override double GetNumber(string key) {
            return cp.core.visitProperty.getNumber(key);
        }
        //
        //=======================================================================================================
        //
        public override string GetText(string key, string defaultValue) {
            return cp.core.visitProperty.getText(key, defaultValue);
        }
        //
        public override string GetText(string key) {
            return cp.core.visitProperty.getText(key);
        }
        //
        //=======================================================================================================
        //
        public override int StartDateValue {
            get {
                return cp.core.session.visit.startDateValue;
            }
        }
        //
        //=======================================================================================================
        //
        public override DateTime StartTime {
            get {
                return cp.core.session.visit.startTime;
            }
        }
        //
        //=======================================================================================================
        // deprecated
        //
        //
        //
        //
        [Obsolete("Deprecated", true )]
        public override string GetProperty(string key, string defaultValue, int TargetVisitId) {
            if (TargetVisitId == 0) {
                return cp.core.visitProperty.getText(key, defaultValue);
            } else {
                return cp.core.visitProperty.getText(key, defaultValue, TargetVisitId);
            }
        }
        //
        [Obsolete("Deprecated", true)]
        public override string GetProperty(string key, string defaultValue) {
            return cp.core.visitProperty.getText(key, defaultValue);
        }
        //
        [Obsolete("Deprecated", true)]
        public override string GetProperty(string key) {
            return cp.core.visitProperty.getText(key);
        }
        //
        [Obsolete("Deprecated", true)]
        public override void SetProperty(string key, string value, int TargetVisitId) {
            if (TargetVisitId == 0) {
                cp.core.visitProperty.setProperty(key, value);
            } else {
                cp.core.visitProperty.setProperty(key, value, TargetVisitId);
            }
        }
        //
        [Obsolete("Deprecated", true)]
        public override bool GetBoolean(string key, string defaultValue) {
            return GetBoolean(key, GenericController.encodeBoolean(defaultValue));
        }
        //
        [Obsolete("Deprecated", true)]
        public override DateTime GetDate(string key, string defaultValue) {
            return GetDate(key, GenericController.encodeDate(defaultValue));
        }
        //
        [Obsolete("Deprecated", true)]
        public override int GetInteger(string key, string defaultValue) {
            return GetInteger(key, GenericController.encodeInteger(defaultValue));
        }
        //
        [Obsolete("Deprecated", true)]
        public override double GetNumber(string key, string defaultValue) {
            return GetNumber(key, GenericController.encodeNumber(defaultValue));
        }
        //
        //=======================================================================================================
        // dispose
        //
        #region  IDisposable Support 
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
        protected bool disposed = false;
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPVisitClass() {
            Dispose(false);
        }
        #endregion
    }
}