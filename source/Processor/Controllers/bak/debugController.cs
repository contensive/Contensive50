

using System.Text.RegularExpressions;
// 

namespace Controllers {
    
    // 
    // ====================================================================================================
    // '' <summary>
    // '' static class controller
    // '' </summary>
    public class debugController : IDisposable {
        
        // 
        // 
        // ========================================================================
        //    Test Point
        //        If main_PageTestPointPrinting print a string, value paior
        // ========================================================================
        // 
        public static void testPoint(coreClass cpcore, string Message) {
            // 
            double ElapsedTime;
            string iMessage;
            // 
            if (cpcore.doc.visitPropertyAllowDebugging) {
                // 
                //  write to stream
                // 
                ElapsedTime = (float.Parse(cpcore.doc.appStopWatch.ElapsedMilliseconds) / 1000);
                iMessage = genericController.encodeText(Message);
                iMessage = (Format(ElapsedTime, "00.000") + (" - " + iMessage));
                cpcore.doc.testPointMessage = (cpcore.doc.testPointMessage + ("<nobr>" 
                            + (iMessage + "</nobr><br >")));
            }
            
            if (cpcore.siteProperties.allowTestPointLogging) {
                // 
                //  write to debug log in virtual files - to read from a test verbose viewer
                // 
                iMessage = genericController.encodeText(Message);
                iMessage = genericController.vbReplace(iMessage, "\r\n", " ");
                iMessage = genericController.vbReplace(iMessage, "\r", " ");
                iMessage = genericController.vbReplace(iMessage, "\n", " ");
                iMessage = (FormatDateTime(Now, vbShortTime) + ('\t' 
                            + (Format(ElapsedTime, "00.000") + ('\t' 
                            + (cpCore.doc.authContext.visit.id + ('\t' + iMessage))))));
                // 
                logController.appendLog(cpcore, iMessage, "", ("testPoints_" + cpcore.serverConfig.appConfig.name));
            }
            
        }
        
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