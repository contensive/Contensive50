
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
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
namespace Contensive.Core {
    public class CPUserErrorClass : BaseClasses.CPUserErrorBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "C175C292-0130-409E-9621-B618F89F4EEC";
        public const string InterfaceId = "C06DB080-41AE-4F1B-A477-B3CF74F61708";
        public const string EventsId = "B784BFEF-127B-48D5-8C99-B075984227DB";
        #endregion
        //
        private Contensive.Core.Controllers.coreController cpCore;
        protected bool disposed = false;
        //
        //====================================================================================================
        //
        public CPUserErrorClass(Contensive.Core.Controllers.coreController cpCoreObj) : base() {
            cpCore = cpCoreObj;
        }
        //
        //====================================================================================================
        //
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
               if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
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
        public override void Add(string Message) {
            errorController.addUserError(cpCore, Message);
        }
        //
        //====================================================================================================
        //
        public override string GetList()  {
            return errorController.getUserError(cpCore);
        }
        //
        //====================================================================================================
        //
        public override bool OK() {
            return !(cpCore.doc.debug_iUserError != "");
        }
        //
        //====================================================================================================
        //
        private void appendDebugLog(string copy) {
            logController.appendLogDebug(cpCore, copy);
        }
        //
        //====================================================================================================
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
        ~CPUserErrorClass() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}