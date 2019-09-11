
using System;
using System.Collections.Generic;

namespace Contensive.Processor {
    public class CPJSONClass : BaseClasses.CPJSONBaseClass, IDisposable {
        //
        /// <summary>
        /// dependencies
        /// </summary>
        private CPClass cp;
        //
        // ====================================================================================================
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cpParent"></param>
        //
        public CPJSONClass(CPClass cpParent) : base() {
            cp = cpParent;
        }
        //
        // ====================================================================================================
        //
        public override string Serialize(object obj) {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }
        //
        // ====================================================================================================
        //
        public override T Deserialize<T>(string JSON) {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(JSON);
        }
        //
        // ====================================================================================================
        //
        #region  IDisposable Support 
        protected bool disposed = false;
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
        // ====================================================================================================
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        // ====================================================================================================
        //
        ~CPJSONClass() {
            Dispose(false);


        }
        #endregion
    }
}