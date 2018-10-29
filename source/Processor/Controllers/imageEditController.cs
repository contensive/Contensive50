
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
using System.Drawing;
//
namespace Contensive.Processor.Controllers {
    public class ImageEditController : IDisposable {
        private bool loaded = false;
        private string src = "";
        private System.Drawing.Image srcImage;
        private int setWidth = 0;
        private int setHeight = 0;
        //
        // dispose
        protected bool disposed = false;
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    if (loaded) {
                        srcImage.Dispose();
                        srcImage = null;
                    }
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        //
        //
        public bool load(string pathFilename, FileController fileSystem) {
            bool returnOk = false;
            try {
                fileSystem.copyRemoteToLocal(pathFilename);
                src = fileSystem.localAbsRootPath + pathFilename;
                if (System.IO.File.Exists(src)) {
                    srcImage = System.Drawing.Image.FromFile(src);
                    setWidth = srcImage.Width;
                    setHeight = srcImage.Height;
                    loaded = true;
                }
            } catch (Exception) {

            }
            return returnOk;
        }
        //
        //
        //
        public bool save(string pathFilename, FileController fileSystem) {
            bool returnOk = false;
            try {
                if (loaded) {
                    if (src == pathFilename) {
                        if (fileSystem.fileExists(src)) {
                            fileSystem.deleteFile(src);
                        }
                    }
                    Bitmap imgOutput = new Bitmap(srcImage, setWidth, setHeight);
                    System.Drawing.Imaging.ImageFormat imgFormat = srcImage.RawFormat;
                    imgOutput.Save(fileSystem.localAbsRootPath + pathFilename, imgFormat);
                    fileSystem.copyLocalToRemote(pathFilename);
                    imgOutput.Dispose();
                    returnOk = true;
                }
            } catch (Exception) {

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
        //
        //
        //
        public int height {
            get {
                return setWidth;
            }
            set {
                setHeight = value;
            }
        }
        //
        //
        //=======================================================================================================
        //
        // IDisposable support
        //
        //=======================================================================================================
        //
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~ImageEditController() {
            Dispose(false);
            
            
        }
        #endregion
    }
}