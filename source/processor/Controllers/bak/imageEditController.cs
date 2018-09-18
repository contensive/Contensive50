

using System.Drawing;
// 

namespace Controllers {
    
    public class imageEditController : IDisposable {
        
        // 
        private bool loaded = false;
        
        private string src = "";
        
        private System.Drawing.Image srcImage;
        
        private int setWidth = 0;
        
        private int setHeight = 0;
        
        // 
        //  dispose
        // 
        protected bool disposed = false;
        
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    // 
                    //  call .dispose for managed objects
                    // 
                    if (loaded) {
                        srcImage.Dispose();
                        srcImage = null;
                    }
                    
                }
                
                // 
                //  Add code here to release the unmanaged resource.
                // 
            }
            
            this.disposed = true;
        }
        
        // 
        // 
        // 
        public bool load(string physicalFilePath) {
            // Implements IimageClass.load
            bool returnOk = false;
            try {
                if (System.IO.File.Exists(physicalFilePath)) {
                    src = physicalFilePath;
                    srcImage = System.Drawing.Image.FromFile(src);
                    setWidth = srcImage.Width;
                    setHeight = srcImage.Height;
                    loaded = true;
                }
                
            }
            catch (Exception ex) {
            }
            
            return returnOk;
        }
        
        // 
        // 
        // 
        public bool save(string physicalFilePath) {
            bool returnOk = false;
            try {
                if (loaded) {
                    if ((src == physicalFilePath)) {
                        if (System.IO.File.Exists(src)) {
                            System.IO.File.Delete(src);
                        }
                        
                    }
                    
                    Bitmap imgOutput = new Bitmap(srcImage, setWidth, setHeight);
                    object imgFormat = srcImage.RawFormat;
                    imgOutput.Save(physicalFilePath, imgFormat);
                    imgOutput.Dispose();
                    returnOk = true;
                }
                
            }
            catch (Exception ex) {
            }
            
            return returnOk;
        }
        
        // 
        // 
        // 
        public int width {
            get {
                return setWidth;
            }
            set {
                setWidth = value;
            }
        }
        
        public int height {
            get {
                return setWidth;
            }
            set {
                setHeight = value;
            }
        }
        
        //  Do not change or add Overridable to these methods.
        //  Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected override void Finalize() {
            this.Dispose(false);
            base.Finalize();
        }
    }
}