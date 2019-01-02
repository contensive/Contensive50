
using Contensive.BaseClasses;
using System;
using System.Collections.Generic;

namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class UpgradeController : IDisposable {
        //
        //==============================================================================================================
        /// <summary>
        /// convert fileInfo array to parsable string [filename-attributes-creationTime-lastAccessTime-lastWriteTime-length-entension]
        /// </summary>
        /// <param name="FileInfo"></param>
        /// <returns></returns>
        public static string Upgrade51ConvertFileInfoArrayToParseString(List<CPFileSystemBaseClass.FileDetail> FileInfo) {
            var result = new System.Text.StringBuilder();
            if (FileInfo.Count > 0) {
                foreach (CPFileSystemBaseClass.FileDetail fi in FileInfo) {
                    result.Append("\r\n" + fi.Name + "\t" + fi.Attributes + "\t" + fi.DateCreated + "\t" + fi.DateLastAccessed + "\t" + fi.DateLastModified + "\t" + fi.Size + "\t" + fi.Extension);
                }
            }
            return result.ToString();
        }
        //
        //==============================================================================================================
        /// <summary>
        /// convert directoryInfo object to parsable string [filename-attributes-creationTime-lastAccessTime-lastWriteTime-extension]
        /// </summary>
        /// <param name="DirectoryInfo"></param>
        /// <returns></returns>
        public static string Upgrade51ConvertDirectoryInfoArrayToParseString(List<CPFileSystemBaseClass.FolderDetail> DirectoryInfo) {
            var result = new System.Text.StringBuilder();
            if (DirectoryInfo.Count > 0) {
                foreach (CPFileSystemBaseClass.FolderDetail di in DirectoryInfo) {
                    result.Append("\r\n" + di.Name + "\t" + (int)di.Attributes + "\t" + di.DateCreated + "\t" + di.DateLastAccessed + "\t" + di.DateLastModified + "\t0\t");
                }
            }
            return result.ToString();
        }
        //
        //====================================================================================================
        #region  IDisposable Support 
        //
        // this class must implement System.IDisposable
        // never throw an exception in dispose
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        //====================================================================================================
        //
        protected bool disposed = false;
        //
        public void Dispose() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        ~UpgradeController() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(false);
        }
        //
        //====================================================================================================
        /// <summary>
        /// dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
                    //If (cacheClient IsNot Nothing) Then
                    //    cacheClient.Dispose()
                    //End If
                }
                //
                // cleanup non-managed objects
                //
            }
        }
        #endregion
    }
    //
}