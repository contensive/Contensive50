
using System;
using System.Text.RegularExpressions;

namespace Controllers {
    
    // 
    // ====================================================================================================
    // '' <summary>
    // '' static class controller
    // '' </summary>
    public class _blankController : IDisposable {
        
        // 
        //  this class must implement System.IDisposable
        //  never throw an exception in dispose
        //  Do not change or add Overridable to these methods.
        //  Put cleanup code in Dispose(ByVal disposing As Boolean).
        // ====================================================================================================
        // 
        protected bool disposed = false;
        
        public void Dispose() {
            //  do not add code here. Use the Dispose(disposing) overload
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        // 
        protected override void Finalize() {
            //  do not add code here. Use the Dispose(disposing) overload
            this.Dispose(false);
            base.Finalize();
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' dispose.
        // '' </summary>
        // '' <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
                    // If (cacheClient IsNot Nothing) Then
                    //     cacheClient.Dispose()
                    // End If
                }
                
                // 
                //  cleanup non-managed objects
                // 
            }
            
        }
    }
}