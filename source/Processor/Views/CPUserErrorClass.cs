
using System;
using Contensive.Processor.Controllers;

namespace Contensive.Processor {
    public class CPUserErrorClass : BaseClasses.CPUserErrorBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "C175C292-0130-409E-9621-B618F89F4EEC";
        public const string InterfaceId = "C06DB080-41AE-4F1B-A477-B3CF74F61708";
        public const string EventsId = "B784BFEF-127B-48D5-8C99-B075984227DB";
        #endregion
        //
        private CPClass cp;
        protected bool disposed = false;
        //
        //====================================================================================================
        //
        public CPUserErrorClass(CPClass cp) {
            this.cp = cp;
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
        public override void Add(string message) {
            ErrorController.addUserError(cp.core, message);
        }
        //
        //====================================================================================================
        //
        public override string GetList()  {
            return ErrorController.getUserError(cp.core);
        }
        //
        //====================================================================================================
        //
        public override bool OK() {
            return !(cp.core.doc.debug_iUserError != "");
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
            
            
        }
        #endregion
    }
}