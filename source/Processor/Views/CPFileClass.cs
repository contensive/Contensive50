
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
            core.fileCdn.appendFile(filename, fileContent);
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
            core.fileCdn.copyFile(sourceFilename, destinationFilename);
        }
        //
        //==========================================================================================
        /// <summary>
        /// Create a folder anywhere on the physical file space of the hosting server. Deprecated, use with cp.file.cdnFiles, cp.file.appRootFiles, or cp.file.privateFiles
        /// </summary>
        /// <param name="folderPath"></param>
        [Obsolete("Deprecated, please use cp.FileCdn, cp.FilePrivate, cp.FileAppRoot, or cp.FileTemp instead.", true)]
        public override void CreateFolder(string folderPath) {
            if (core.fileAppRoot.isinLocalAbsDosPath(folderPath)) {
                core.fileAppRoot.createPath(folderPath);
            } else if (core.filePrivate.isinLocalAbsDosPath(folderPath)) {
                core.filePrivate.createPath(folderPath);
            } else if (core.fileCdn.isinLocalAbsDosPath(folderPath)) {
                core.fileCdn.createPath(folderPath);
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
            if (core.fileAppRoot.isinLocalAbsDosPath(pathFilename)) {
                core.fileAppRoot.deleteFile(pathFilename);
            } else if (core.filePrivate.isinLocalAbsDosPath(pathFilename)) {
                core.filePrivate.deleteFile(pathFilename);
            } else if (core.fileCdn.isinLocalAbsDosPath(pathFilename)) {
                core.fileCdn.deleteFile(pathFilename);
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
            core.fileCdn.deleteFile(pathFilename);
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
            if (core.fileAppRoot.isinLocalAbsDosPath(pathFilename)) {
                return core.fileAppRoot.readFileText(pathFilename);
            } else if (core.filePrivate.isinLocalAbsDosPath(pathFilename)) {
                return core.filePrivate.readFileText(pathFilename);
            } else if (core.fileCdn.isinLocalAbsDosPath(pathFilename)) {
                return core.fileCdn.readFileText(pathFilename);
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
            return core.fileCdn.readFileText(pathFilename);
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
            if (core.fileAppRoot.isinLocalAbsDosPath(pathFilename)) {
                core.fileAppRoot.saveFile(pathFilename, fileContent);
            } else if (core.filePrivate.isinLocalAbsDosPath(pathFilename)) {
                core.filePrivate.saveFile(pathFilename, fileContent);
            } else if (core.fileCdn.isinLocalAbsDosPath(pathFilename)) {
                core.fileCdn.saveFile(pathFilename, fileContent);
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
            core.fileCdn.saveFile(filename, fileContent);
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
            if (core.fileAppRoot.isinLocalAbsDosPath(pathFileName)) {
                result = core.fileAppRoot.fileExists(pathFileName);
            } else if (core.filePrivate.isinLocalAbsDosPath(pathFileName)) {
                result = core.filePrivate.fileExists(pathFileName);
            } else if (core.fileCdn.isinLocalAbsDosPath(pathFileName)) {
                result = core.fileCdn.fileExists(pathFileName);
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
            if (core.fileAppRoot.isinLocalAbsDosPath(pathFolderName)) {
                result = core.fileAppRoot.pathExists(pathFolderName);
            } else if (core.filePrivate.isinLocalAbsDosPath(pathFolderName)) {
                result = core.filePrivate.pathExists(pathFolderName);
            } else if (core.fileCdn.isinLocalAbsDosPath(pathFolderName)) {
                result = core.fileCdn.pathExists(pathFolderName);
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
        public override string fileList(string pathFolderName, int pageSize = 0, int pageNumber = 1) {
            string result = "";
            if (core.fileAppRoot.isinLocalAbsDosPath(pathFolderName)) {
                List<FileDetail> fi = core.fileAppRoot.getFileList(pathFolderName);
                result = UpgradeController.Upgrade51ConvertFileInfoArrayToParseString(fi);
            } else if (core.filePrivate.isinLocalAbsDosPath(pathFolderName)) {
                List<FileDetail> fi = core.filePrivate.getFileList(pathFolderName);
                result = UpgradeController.Upgrade51ConvertFileInfoArrayToParseString(fi);
            } else if (core.fileCdn.isinLocalAbsDosPath(pathFolderName)) {
                List<FileDetail> fi = core.fileCdn.getFileList(pathFolderName);
                result = UpgradeController.Upgrade51ConvertFileInfoArrayToParseString(fi);
            } else {
                throw (new GenericException("Application cannot access this path [" + pathFolderName + "]"));
            }
            return result;
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
            if (core.fileAppRoot.isinLocalAbsDosPath(pathFolderName)) {
                List<FolderDetail> fi = core.fileAppRoot.getFolderList(pathFolderName);
                result = UpgradeController.Upgrade51ConvertDirectoryInfoArrayToParseString(fi);
            } else if (core.filePrivate.isinLocalAbsDosPath(pathFolderName)) {
                List<FolderDetail> fi = core.filePrivate.getFolderList(pathFolderName);
                result = UpgradeController.Upgrade51ConvertDirectoryInfoArrayToParseString(fi);
            } else if (core.fileCdn.isinLocalAbsDosPath(pathFolderName)) {
                List<FolderDetail> fi = core.fileCdn.getFolderList(pathFolderName);
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
            if (core.fileAppRoot.isinLocalAbsDosPath(pathFolderName)) {
                core.fileAppRoot.deleteFolder(pathFolderName);
            } else if (core.filePrivate.isinLocalAbsDosPath(pathFolderName)) {
                core.fileAppRoot.deleteFolder(pathFolderName);
            } else if (core.fileCdn.isinLocalAbsDosPath(pathFolderName)) {
                core.fileAppRoot.deleteFolder(pathFolderName);
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