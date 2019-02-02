
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
using static Contensive.BaseClasses.CPFileSystemBaseClass;
using Contensive.Processor.Exceptions;

namespace Contensive.Processor {
    //
    //==========================================================================================
    /// <summary>
    /// cpFileClass is a legacy implementation replaced with cdnFiles, appRootFiles and privateFiles. Non-Virtual calls do not limit file destination so are not scale-mode compatible
    /// </summary>
    //[ComVisible(true), Microsoft.VisualBasic.ComClass(CPFileClass.ClassId, CPFileClass.InterfaceId, CPFileClass.EventsId)]
    public class CPFileClass : BaseClasses.CPFileBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "E3310DFA-0ABF-4DC7-ABB5-4D294D30324B";
        public const string InterfaceId = "44C305D8-A8C3-490D-8E79-E17F9B3D34CE";
        public const string EventsId = "8757DE11-C04D-4765-B46B-458E281BAE19";
        #endregion
        //
        private Contensive.Processor.Controllers.CoreController core;
        //
        //==========================================================================================
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="core"></param>
        public CPFileClass(CPClass cp) {
            this.core = cp.core;
        }
        //
        //==========================================================================================
        /// <summary>
        /// Convert a filepath in the cdnFiles store to a URL
        /// </summary>
        /// <param name="virtualFilename"></param>
        /// <returns></returns>
        [Obsolete("Deprecated, please use cp.FileCdn, cp.FilePrivate, cp.FileAppRoot, or cp.FileTemp instead.", true)]
        public override string getVirtualFileLink(string virtualFilename) {
            return GenericController.getCdnFileLink(core, virtualFilename);
        }
        //
        //==========================================================================================
        /// <summary>
        /// Append a file in the cdnFiles store. Deprecated, use cp.file.cdn.appendFile
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="fileContent"></param>
        [Obsolete("Deprecated, please use cp.FileCdn, cp.FilePrivate, cp.FileAppRoot, or cp.FileTemp instead.", true)]
        public override void AppendVirtual(string filename, string fileContent) {
            core.cdnFiles.appendFile(filename, fileContent);
        }
        //
        //==========================================================================================
        /// <summary>
        /// Copy a file within cdnFiles.
        /// </summary>
        /// <param name="sourceFilename"></param>
        /// <param name="destinationFilename"></param>
        [Obsolete("Deprecated, please use cp.FileCdn, cp.FilePrivate, cp.FileAppRoot, or cp.FileTemp instead.", true)]
        public override void CopyVirtual(string sourceFilename, string destinationFilename) {
            core.cdnFiles.copyFile(sourceFilename, destinationFilename);
        }
        //
        //==========================================================================================
        /// <summary>
        /// Create a folder anywhere on the physical file space of the hosting server. Deprecated, use with cp.file.cdnFiles, cp.file.appRootFiles, or cp.file.privateFiles
        /// </summary>
        /// <param name="folderPath"></param>
        [Obsolete("Deprecated, please use cp.FileCdn, cp.FilePrivate, cp.FileAppRoot, or cp.FileTemp instead.", true)]
        public override void CreateFolder(string folderPath) {
            if (core.wwwfiles.isinLocalAbsDosPath(folderPath)) {
                core.wwwfiles.createPath(folderPath);
            } else if (core.privateFiles.isinLocalAbsDosPath(folderPath)) {
                core.privateFiles.createPath(folderPath);
            } else if (core.cdnFiles.isinLocalAbsDosPath(folderPath)) {
                core.cdnFiles.createPath(folderPath);
            } else {
                throw (new GenericException("Application cannot access this path [" + folderPath + "]"));
            }
        }
        //
        //==========================================================================================
        /// <summary>
        /// Delete a file anywhere on the physical file space of the hosting server.
        /// </summary>
        /// <param name="pathFilename"></param>
        [Obsolete("Deprecated, please use cp.FileCdn, cp.FilePrivate, cp.FileAppRoot, or cp.FileTemp instead.", true)]
        public override void Delete(string pathFilename) {
            if (core.wwwfiles.isinLocalAbsDosPath(pathFilename)) {
                core.wwwfiles.deleteFile(pathFilename);
            } else if (core.privateFiles.isinLocalAbsDosPath(pathFilename)) {
                core.privateFiles.deleteFile(pathFilename);
            } else if (core.cdnFiles.isinLocalAbsDosPath(pathFilename)) {
                core.cdnFiles.deleteFile(pathFilename);
            } else {
                throw (new GenericException("Application cannot access this path [" + pathFilename + "]"));
            }
        }
        //
        //==========================================================================================
        /// <summary>
        /// Delete a file in the cdnFiles store.
        /// </summary>
        /// <param name="pathFilename"></param>
        [Obsolete("Deprecated, please use cp.FileCdn, cp.FilePrivate, cp.FileAppRoot, or cp.FileTemp instead.", true)]
        public override void DeleteVirtual(string pathFilename) {
            core.cdnFiles.deleteFile(pathFilename);
        }
        //
        //==========================================================================================
        /// <summary>
        /// Save a file anywhere on the physical file space of the hosting server.
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <returns></returns>
        [Obsolete("Deprecated, please use cp.FileCdn, cp.FilePrivate, cp.FileAppRoot, or cp.FileTemp instead.", true)]
        public override string Read(string pathFilename) {
            if (core.wwwfiles.isinLocalAbsDosPath(pathFilename)) {
                return core.wwwfiles.readFileText(pathFilename);
            } else if (core.privateFiles.isinLocalAbsDosPath(pathFilename)) {
                return core.privateFiles.readFileText(pathFilename);
            } else if (core.cdnFiles.isinLocalAbsDosPath(pathFilename)) {
                return core.cdnFiles.readFileText(pathFilename);
            } else {
                throw (new GenericException("Application cannot access this path [" + pathFilename + "]"));
            }
        }
        //
        //==========================================================================================
        /// <summary>
        /// Read a file from the cdnFiles store.
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <returns></returns>
        [Obsolete("Deprecated, please use cp.FileCdn, cp.FilePrivate, cp.FileAppRoot, or cp.FileTemp instead.", true)]
        public override string ReadVirtual(string pathFilename) {
            return core.cdnFiles.readFileText(pathFilename);
        }
        //
        //==========================================================================================
        /// <summary>
        /// Save a file anywhere on the physical file space of the hosting server.
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <param name="fileContent"></param>
        [Obsolete("Deprecated, please use cp.FileCdn, cp.FilePrivate, cp.FileAppRoot, or cp.FileTemp instead.", true)]
        public override void Save(string pathFilename, string fileContent) {
            if (core.wwwfiles.isinLocalAbsDosPath(pathFilename)) {
                core.wwwfiles.saveFile(pathFilename, fileContent);
            } else if (core.privateFiles.isinLocalAbsDosPath(pathFilename)) {
                core.privateFiles.saveFile(pathFilename, fileContent);
            } else if (core.cdnFiles.isinLocalAbsDosPath(pathFilename)) {
                core.cdnFiles.saveFile(pathFilename, fileContent);
            } else {
                throw (new GenericException("Application cannot access this path [" + pathFilename + "]"));
            }
        }
        //
        //==========================================================================================
        /// <summary>
        /// Save a file in the cdnFiles store.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="fileContent"></param>
        [Obsolete("Deprecated, please use cp.FileCdn, cp.FilePrivate, cp.FileAppRoot, or cp.FileTemp instead.", true)]
        public override void SaveVirtual(string filename, string fileContent) {
            core.cdnFiles.saveFile(filename, fileContent);
        }
        //
        //==========================================================================================
        /// <summary>
        /// Test if a file exists anywhere on the physical file space of the hosting server.
        /// </summary>
        /// <param name="pathFileName"></param>
        /// <returns></returns>
        [Obsolete("Deprecated, please use cp.FileCdn, cp.FilePrivate, cp.FileAppRoot, or cp.FileTemp instead.", true)]
        public override bool fileExists(string pathFileName) {
            bool result = false;
            if (core.wwwfiles.isinLocalAbsDosPath(pathFileName)) {
                result = core.wwwfiles.fileExists(pathFileName);
            } else if (core.privateFiles.isinLocalAbsDosPath(pathFileName)) {
                result = core.privateFiles.fileExists(pathFileName);
            } else if (core.cdnFiles.isinLocalAbsDosPath(pathFileName)) {
                result = core.cdnFiles.fileExists(pathFileName);
            } else {
                throw (new GenericException("Application cannot access this path [" + pathFileName + "]"));
            }
            return result;
        }
        //
        //==========================================================================================
        /// <summary>
        /// Test if a folder exists anywhere on the physical file space of the hosting server.
        /// </summary>
        /// <param name="pathFolderName"></param>
        /// <returns></returns>
        [Obsolete("Deprecated, please use cp.FileCdn, cp.FilePrivate, cp.FileAppRoot, or cp.FileTemp instead.", true)]
        public override bool folderExists(string pathFolderName) {
            bool result = false;
            if (core.wwwfiles.isinLocalAbsDosPath(pathFolderName)) {
                result = core.wwwfiles.pathExists(pathFolderName);
            } else if (core.privateFiles.isinLocalAbsDosPath(pathFolderName)) {
                result = core.privateFiles.pathExists(pathFolderName);
            } else if (core.cdnFiles.isinLocalAbsDosPath(pathFolderName)) {
                result = core.cdnFiles.pathExists(pathFolderName);
            } else {
                throw (new GenericException("Application cannot access this path [" + pathFolderName + "]"));
            }
            return result;
        }
        //
        //==========================================================================================
        /// <summary>
        /// Return a parsable comma,crlf delimited string of the files available anywhere on the physical file space of the hosting server.
        /// </summary>
        /// <param name="pathFolderName"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        [Obsolete("Deprecated, please use cp.FileCdn, cp.FilePrivate, cp.FileAppRoot, or cp.FileTemp instead.", true)]
        public override string fileList(string pathFolderName, int pageSize, int pageNumber) {
            string result = "";
            if (core.wwwfiles.isinLocalAbsDosPath(pathFolderName)) {
                List<FileDetail> fi = core.wwwfiles.getFileList(pathFolderName);
                result = UpgradeController.Upgrade51ConvertFileInfoArrayToParseString(fi);
            } else if (core.privateFiles.isinLocalAbsDosPath(pathFolderName)) {
                List<FileDetail> fi = core.privateFiles.getFileList(pathFolderName);
                result = UpgradeController.Upgrade51ConvertFileInfoArrayToParseString(fi);
            } else if (core.cdnFiles.isinLocalAbsDosPath(pathFolderName)) {
                List<FileDetail> fi = core.cdnFiles.getFileList(pathFolderName);
                result = UpgradeController.Upgrade51ConvertFileInfoArrayToParseString(fi);
            } else {
                throw (new GenericException("Application cannot access this path [" + pathFolderName + "]"));
            }
            return result;
        }
        //
        [Obsolete("Deprecated, please use cp.FileCdn, cp.FilePrivate, cp.FileAppRoot, or cp.FileTemp instead.", true)]
        public override string fileList(string pathFolderName, int pageSize) {
            return fileList(pathFolderName, pageSize, 1);
        }
        //
        [Obsolete("Deprecated, please use cp.FileCdn, cp.FilePrivate, cp.FileAppRoot, or cp.FileTemp instead.", true)]
        public override string fileList(string pathFolderName) {
            return fileList(pathFolderName, 9999, 1);
        }
        //
        //==========================================================================================
        /// <summary>
        /// Return a parsable comma,crlf delimited string of the folders available anywhere on the physical file space of the hosting server.
        /// </summary>
        /// <param name="pathFolderName"></param>
        /// <returns></returns>
        [Obsolete("Deprecated, please use cp.FileCdn, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", true)]
        public override string folderList(string pathFolderName) {
            string result = "";
            if (core.wwwfiles.isinLocalAbsDosPath(pathFolderName)) {
                List<FolderDetail> fi = core.wwwfiles.getFolderList(pathFolderName);
                result = UpgradeController.Upgrade51ConvertDirectoryInfoArrayToParseString(fi);
            } else if (core.privateFiles.isinLocalAbsDosPath(pathFolderName)) {
                List<FolderDetail> fi = core.privateFiles.getFolderList(pathFolderName);
                result = UpgradeController.Upgrade51ConvertDirectoryInfoArrayToParseString(fi);
            } else if (core.cdnFiles.isinLocalAbsDosPath(pathFolderName)) {
                List<FolderDetail> fi = core.cdnFiles.getFolderList(pathFolderName);
                result = UpgradeController.Upgrade51ConvertDirectoryInfoArrayToParseString(fi);
            } else {
                throw (new GenericException("Application cannot access this path [" + pathFolderName + "]"));
            }
            return result;
        }
        //
        //==========================================================================================
        /// <summary>
        /// Delete a folder anywhere on the physical file space of the hosting server.
        /// </summary>
        /// <param name="pathFolderName"></param>
        [Obsolete("Deprecated, please use cp.FileCdn, cp.FilePrivate, cp.FileAppRoot, or cp.FileTemp instead.", true)]
        public override void DeleteFolder(string pathFolderName) {
            if (core.wwwfiles.isinLocalAbsDosPath(pathFolderName)) {
                core.wwwfiles.deleteFolder(pathFolderName);
            } else if (core.privateFiles.isinLocalAbsDosPath(pathFolderName)) {
                core.wwwfiles.deleteFolder(pathFolderName);
            } else if (core.cdnFiles.isinLocalAbsDosPath(pathFolderName)) {
                core.wwwfiles.deleteFolder(pathFolderName);
            } else {
                throw (new GenericException("Application cannot access this path [" + pathFolderName + "]"));
            }
        }
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        protected bool disposed = false;
        //
        //==========================================================================================
        /// <summary>
        /// dispose
        /// </summary>
        /// <param name="disposing"></param>
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
            disposed = true;
        }
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPFileClass() {
            Dispose(false);
        }
        #endregion
    }
}