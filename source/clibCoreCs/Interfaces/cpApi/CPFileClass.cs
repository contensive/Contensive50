
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
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
using System.IO;
//
namespace Contensive.Core {
    //
    // comVisible to be activeScript and compatible
    //
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
        private Contensive.Core.coreClass cpCore;
        protected bool disposed = false;
        //
        //==========================================================================================
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="core"></param>
        public CPFileClass(Contensive.Core.coreClass core) : base() {
            this.cpCore = core;
        }
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
                    cpCore = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        //==========================================================================================
        /// <summary>
        /// Convert a filepath in the cdnFiles store to a URL
        /// </summary>
        /// <param name="virtualFilename"></param>
        /// <returns></returns>
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", false)]
        public override string getVirtualFileLink(string virtualFilename) {
            return genericController.getCdnFileLink(cpCore, virtualFilename);
        }
        //
        //==========================================================================================
        /// <summary>
        /// Append a file in the cdnFiles store. Deprecated, use cp.file.cdn.appendFile
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="fileContent"></param>
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", false)]
        public override void AppendVirtual(string filename, string fileContent) {
            cpCore.cdnFiles.appendFile(filename, fileContent);
        }
        //
        //==========================================================================================
        /// <summary>
        /// Copy a file within cdnFiles.
        /// </summary>
        /// <param name="sourceFilename"></param>
        /// <param name="destinationFilename"></param>
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", false)]
        public override void CopyVirtual(string sourceFilename, string destinationFilename) {
            cpCore.cdnFiles.copyFile(sourceFilename, destinationFilename);
        }
        //
        //==========================================================================================
        /// <summary>
        /// Create a folder anywhere on the physical file space of the hosting server. Deprecated, use with cp.file.cdnFiles, cp.file.appRootFiles, or cp.file.privateFiles
        /// </summary>
        /// <param name="folderPath"></param>
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", false)]
        public override void CreateFolder(string folderPath) {
            if (cpCore.appRootFiles.isinPhysicalPath(folderPath)) {
                cpCore.appRootFiles.createPath(folderPath);
            } else if (cpCore.privateFiles.isinPhysicalPath(folderPath)) {
                cpCore.privateFiles.createPath(folderPath);
            } else if (cpCore.cdnFiles.isinPhysicalPath(folderPath)) {
                cpCore.cdnFiles.createPath(folderPath);
            } else {
                throw (new ApplicationException("Application cannot access this path [" + folderPath + "]"));
            }
        }
        //
        //==========================================================================================
        /// <summary>
        /// Delete a file anywhere on the physical file space of the hosting server.
        /// </summary>
        /// <param name="pathFilename"></param>
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", false)]
        public override void Delete(string pathFilename) {
            if (cpCore.appRootFiles.isinPhysicalPath(pathFilename)) {
                cpCore.appRootFiles.deleteFile(pathFilename);
            } else if (cpCore.privateFiles.isinPhysicalPath(pathFilename)) {
                cpCore.privateFiles.deleteFile(pathFilename);
            } else if (cpCore.cdnFiles.isinPhysicalPath(pathFilename)) {
                cpCore.cdnFiles.deleteFile(pathFilename);
            } else {
                throw (new ApplicationException("Application cannot access this path [" + pathFilename + "]"));
            }
        }
        //
        //==========================================================================================
        /// <summary>
        /// Delete a file in the cdnFiles store.
        /// </summary>
        /// <param name="pathFilename"></param>
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", false)]
        public override void DeleteVirtual(string pathFilename) {
            cpCore.cdnFiles.deleteFile(pathFilename);
        }
        //
        //==========================================================================================
        /// <summary>
        /// Save a file anywhere on the physical file space of the hosting server.
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <returns></returns>
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", false)]
        public override string Read(string pathFilename) {
            if (cpCore.appRootFiles.isinPhysicalPath(pathFilename)) {
                return cpCore.appRootFiles.readFile(pathFilename);
            } else if (cpCore.privateFiles.isinPhysicalPath(pathFilename)) {
                return cpCore.privateFiles.readFile(pathFilename);
            } else if (cpCore.cdnFiles.isinPhysicalPath(pathFilename)) {
                return cpCore.cdnFiles.readFile(pathFilename);
            } else {
                throw (new ApplicationException("Application cannot access this path [" + pathFilename + "]"));
            }
            return string.Empty;
        }
        //
        //==========================================================================================
        /// <summary>
        /// Read a file from the cdnFiles store.
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <returns></returns>
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", false)]
        public override string ReadVirtual(string pathFilename) {
            return cpCore.cdnFiles.readFile(pathFilename);
        }
        //
        //==========================================================================================
        /// <summary>
        /// Save a file anywhere on the physical file space of the hosting server.
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <param name="fileContent"></param>
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", false)]
        public override void Save(string pathFilename, string fileContent) {
            if (cpCore.appRootFiles.isinPhysicalPath(pathFilename)) {
                cpCore.appRootFiles.saveFile(pathFilename, fileContent);
            } else if (cpCore.privateFiles.isinPhysicalPath(pathFilename)) {
                cpCore.privateFiles.saveFile(pathFilename, fileContent);
            } else if (cpCore.cdnFiles.isinPhysicalPath(pathFilename)) {
                cpCore.cdnFiles.saveFile(pathFilename, fileContent);
            } else {
                throw (new ApplicationException("Application cannot access this path [" + pathFilename + "]"));
            }
        }
        //
        //==========================================================================================
        /// <summary>
        /// Save a file in the cdnFiles store.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="fileContent"></param>
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", false)]
        public override void SaveVirtual(string filename, string fileContent) {
            cpCore.cdnFiles.saveFile(filename, fileContent);
        }
        //
        //==========================================================================================
        /// <summary>
        /// Test if a file exists anywhere on the physical file space of the hosting server.
        /// </summary>
        /// <param name="pathFileName"></param>
        /// <returns></returns>
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", false)]
        public override bool fileExists(string pathFileName) {
            bool result = false;
            if (cpCore.appRootFiles.isinPhysicalPath(pathFileName)) {
                result = cpCore.appRootFiles.fileExists(pathFileName);
            } else if (cpCore.privateFiles.isinPhysicalPath(pathFileName)) {
                result = cpCore.privateFiles.fileExists(pathFileName);
            } else if (cpCore.cdnFiles.isinPhysicalPath(pathFileName)) {
                result = cpCore.cdnFiles.fileExists(pathFileName);
            } else {
                throw (new ApplicationException("Application cannot access this path [" + pathFileName + "]"));
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
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", false)]
        public override bool folderExists(string pathFolderName) {
            bool result = false;
            if (cpCore.appRootFiles.isinPhysicalPath(pathFolderName)) {
                result = cpCore.appRootFiles.pathExists(pathFolderName);
            } else if (cpCore.privateFiles.isinPhysicalPath(pathFolderName)) {
                result = cpCore.privateFiles.pathExists(pathFolderName);
            } else if (cpCore.cdnFiles.isinPhysicalPath(pathFolderName)) {
                result = cpCore.cdnFiles.pathExists(pathFolderName);
            } else {
                throw (new ApplicationException("Application cannot access this path [" + pathFolderName + "]"));
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
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", false)]
        public override string fileList(string pathFolderName, int pageSize = 0, int pageNumber = 1) {
            string result = "";
            if (cpCore.appRootFiles.isinPhysicalPath(pathFolderName)) {
                FileInfo[] fi = cpCore.appRootFiles.getFileList(pathFolderName);
                result = cpCore.appRootFiles.convertFileINfoArrayToParseString(fi);
            } else if (cpCore.privateFiles.isinPhysicalPath(pathFolderName)) {
                FileInfo[] fi = cpCore.privateFiles.getFileList(pathFolderName);
                result = cpCore.privateFiles.convertFileINfoArrayToParseString(fi);
            } else if (cpCore.cdnFiles.isinPhysicalPath(pathFolderName)) {
                IOileInfo[] fi = cpCore.cdnFiles.getFileList(pathFolderName);
                result = cpCore.cdnFiles.convertFileINfoArrayToParseString(fi);
            } else {
                throw (new ApplicationException("Application cannot access this path [" + pathFolderName + "]"));
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
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", false)]
        public override string folderList(string pathFolderName) {
            string result = "";
            if (cpCore.appRootFiles.isinPhysicalPath(pathFolderName)) {
                IO.DirectoryInfo[] fi = cpCore.appRootFiles.getFolderList(pathFolderName);
                result = cpCore.appRootFiles.convertDirectoryInfoArrayToParseString(fi);
            } else if (cpCore.privateFiles.isinPhysicalPath(pathFolderName)) {
                IO.DirectoryInfo[] fi = cpCore.privateFiles.getFolderList(pathFolderName);
                result = cpCore.privateFiles.convertDirectoryInfoArrayToParseString(fi);
            } else if (cpCore.cdnFiles.isinPhysicalPath(pathFolderName)) {
                IO.DirectoryInfo[] fi = cpCore.cdnFiles.getFolderList(pathFolderName);
                result = cpCore.cdnFiles.convertDirectoryInfoArrayToParseString(fi);
            } else {
                throw (new ApplicationException("Application cannot access this path [" + pathFolderName + "]"));
            }
            return result;
        }
        //
        //==========================================================================================
        /// <summary>
        /// Delete a folder anywhere on the physical file space of the hosting server.
        /// </summary>
        /// <param name="pathFolderName"></param>
        [Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", false)]
        public override void DeleteFolder(string pathFolderName) {
            if (cpCore.appRootFiles.isinPhysicalPath(pathFolderName)) {
                cpCore.appRootFiles.DeleteFileFolder(pathFolderName);
            } else if (cpCore.privateFiles.isinPhysicalPath(pathFolderName)) {
                cpCore.appRootFiles.DeleteFileFolder(pathFolderName);
            } else if (cpCore.cdnFiles.isinPhysicalPath(pathFolderName)) {
                cpCore.appRootFiles.DeleteFileFolder(pathFolderName);
            } else {
                throw (new ApplicationException("Application cannot access this path [" + pathFolderName + "]"));
            }
        }
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPFileClass() {
            Dispose(false);
            //INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}