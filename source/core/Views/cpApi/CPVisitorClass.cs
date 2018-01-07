
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
//
namespace Contensive.Core {
    public class CPVisitorClass : BaseClasses.CPVisitorBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "77CCF761-0656-4B75-81F4-5AD2456F6D0F";
        public const string InterfaceId = "07665D81-5DCD-437A-9E33-16F56DA66B29";
        public const string EventsId = "835C660E-92B5-4055-B620-64268319E31B";
        #endregion
        //
        private Contensive.Core.coreClass cpCore;
        private CPClass cp;
        protected bool disposed = false;
        //
        public CPVisitorClass(Contensive.Core.coreClass cpCoreObj, CPClass cpParent) : base() {
            this.cpCore = cpCoreObj;
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
                    cpCore = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }

        public override bool ForceBrowserMobile
        {
            get {
                return genericController.encodeBoolean(cpCore.doc.sessionContext.visitor.ForceBrowserMobile);
            }
        }

        public override string GetProperty(string PropertyName, string DefaultValue = "", int TargetVisitorId = 0) {
            if (TargetVisitorId == 0) {
                return cpCore.visitorProperty.getText(PropertyName, DefaultValue);
            } else {
                return cpCore.visitorProperty.getText(PropertyName, DefaultValue, TargetVisitorId);
            }
        }
        //
        //=======================================================================================================
        //
        //=======================================================================================================
        //
        public override bool GetBoolean(string PropertyName, string DefaultValue = "") {
            return genericController.encodeBoolean(GetProperty(PropertyName, DefaultValue));
        }
        //
        //=======================================================================================================
        //
        //=======================================================================================================
        //
        public override DateTime GetDate(string PropertyName, string DefaultValue = "") {
            return genericController.encodeDate(GetProperty(PropertyName, DefaultValue));
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

        public override int Id
        {
            get {
                return cpCore.doc.sessionContext.visitor.id;
            }
        }

        public override bool IsNew //Inherits BaseClasses.CPVisitorBaseClass.IsNew
        {
            get {
                return cpCore.doc.sessionContext.visit.VisitorNew;
            }
        }

        public override void SetProperty(string PropertyName, string Value, int TargetVisitorid = 0) //Inherits BaseClasses.CPVisitorBaseClass.SetProperty
        {
            if (TargetVisitorid == 0) {
                cpCore.visitorProperty.setProperty(PropertyName, Value);
            } else {
                cpCore.visitorProperty.setProperty(PropertyName, Value, TargetVisitorid);
            }
        }

        public override int UserId {
            get {
                return cpCore.doc.sessionContext.visitor.MemberID;
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
        ~CPVisitorClass() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}