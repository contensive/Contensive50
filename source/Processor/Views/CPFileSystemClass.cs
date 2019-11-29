
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;

namespace Contensive.Processor {
    public class CPFileSystemClass : BaseClasses.CPFileSystemBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "0B73809E-F149-4262-A548-FA1E11DF63A6";
        public const string InterfaceId = "4F8288A4-2854-4B60-9281-9A776DC101D0";
        public const string EventsId = "987E6DDE-E9E6-46C5-9467-BAE79A129A15";
        #endregion
        //
        private Contensive.Processor.Controllers.CoreController core;
        /// <summary>
        /// The instance of the controller used to implement this instance. Either core.TempFiles, core.wwwFiles, core.cdnFiles, or core.appRootFiles
        /// </summary>
        private FileController fileSystemController;
        //
        //==========================================================================================
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="core"></param>
        public CPFileSystemClass(CPClass cp, FileController fileSystemController) {
            core = cp.core;
            this.fileSystemController = fileSystemController;
        }
        //
        //==========================================================================================
        /// <summary>
        /// The physical file path to the local storage used for this file system resource. 
        /// </summary>
        public override string PhysicalFilePath {
            get { return fileSystemController.localAbsRootPath; }
        }
        //
        //==========================================================================================
        /// <summary>
        /// Append a file with content. NOTE: avoid with all remote file systems
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="fileContent"></param>
        public override void Append(string filename, string fileContent) {
            fileSystemController.appendFile(filename, fileContent);
        }
        //
        //==========================================================================================
        /// <summary>
        /// Copy a file within the same filesystem
        /// </summary>
        /// <param name="sourceFilename"></param>
        /// <param name="destinationFilename"></param>
        public override void Copy(string sourceFilename, string destinationFilename) {
            fileSystemController.copyFile(sourceFilename, destinationFilename);
        }
        //
        //==========================================================================================
        /// <summary>
        /// Copy a file to another file system
        /// </summary>
        /// <param name="sourceFilename"></param>
        /// <param name="destinationFilename"></param>
        /// <param name="destinationFileSystem"></param>
        public override void Copy(string sourceFilename, string destinationFilename, BaseClasses.CPFileSystemBaseClass destinationFileSystem) {
            fileSystemController.copyFile(sourceFilename, destinationFilename, ((CPFileSystemClass)destinationFileSystem).fileSystemController);
        }
        //
        //==========================================================================================
        public override void CopyLocalToRemote(string pathFilename) {
            fileSystemController.copyFileLocalToRemote(pathFilename);
        }
        //
        //==========================================================================================
        public override void CopyRemoteToLocal(string pathFilename) {
            fileSystemController.copyFileRemoteToLocal(pathFilename);
        }
        //
        //==========================================================================================
        /// <summary>
        /// Create a folder in a path. Path arguments should have no leading slash.
        /// </summary>
        /// <param name="pathFolder"></param>
        public override void CreateFolder(string pathFolder) {
            fileSystemController.createPath(pathFolder);
        }
        //
        //==========================================================================================
        public override void DeleteFile(string filename) {
            fileSystemController.deleteFile(filename);
        }
        //
        //==========================================================================================
        public override string Read(string filename) {
            return fileSystemController.readFileText(filename);
        }
        //
        //==========================================================================================
        public override byte[] ReadBinary(string filename) {
            return fileSystemController.readFileBinary(filename);
        }
        //
        //==========================================================================================
        public override void Save(string filename, string fileContent) {
            fileSystemController.saveFile(filename, fileContent);
        }
        //
        //==========================================================================================
        public override void SaveBinary(string filename, byte[] fileContent) {
            fileSystemController.saveFile(filename, fileContent);
        }
        //
        //==========================================================================================
        public override bool FileExists(string pathFileName) {
            return fileSystemController.fileExists(pathFileName);
        }
        //
        //==========================================================================================
        public override bool FolderExists(string folderName) {
            return fileSystemController.pathExists(folderName);
        }
        //
        //==========================================================================================
        public override List<FileDetail> FileList(string folderName, int pageSize, int pageNumber) {
            return fileSystemController.getFileList(folderName);
        }
        //
        //==========================================================================================
        public override List<FileDetail> FileList(string folderName, int pageSize) {
            return fileSystemController.getFileList(folderName);
        }
        //
        //==========================================================================================
        public override List<FileDetail> FileList(string folderName) {
            return fileSystemController.getFileList(folderName);
        }
        //
        //==========================================================================================
        public override List<FolderDetail> FolderList(string folderName) {
            return fileSystemController.getFolderList(folderName);
        }
        //
        //==========================================================================================
        public override void DeleteFolder(string folderPath) {
            fileSystemController.deleteFolder(folderPath);
        }
        //
        //==========================================================================================
        //
        public override bool SaveUpload(string htmlformName, ref string returnFilename) {
            return fileSystemController.upload(htmlformName, "\\upload", ref returnFilename);
        }
        //
        //==========================================================================================
        //
        public override bool SaveUpload(string htmlformName, string folderpath, ref string returnFilename) {
            return fileSystemController.upload(htmlformName, folderpath, ref  returnFilename);
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

        ~CPFileSystemClass() {
            Dispose(false);
        }
        #endregion
    }
}