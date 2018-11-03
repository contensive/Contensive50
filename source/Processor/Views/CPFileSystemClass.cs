
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
        protected bool disposed = false;
        //
        private FileController fileSystem;
        //
        //==========================================================================================
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="core"></param>
        public CPFileSystemClass(Contensive.Processor.Controllers.CoreController core, FileController fileSystem) : base() {
            this.core = core;
            this.fileSystem = fileSystem;
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
                    core = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        //==========================================================================================
        public override void Append(string filename, string fileContent) {
            fileSystem.appendFile(filename, fileContent);
        }
        //
        //==========================================================================================
        public override void Copy(string sourceFilename, string destinationFilename) {
            fileSystem.copyFile(sourceFilename, destinationFilename);
        }
        //
        //==========================================================================================
        public override void CreateFolder(string folderPath) {
            fileSystem.createPath(folderPath);
        }
        //
        //==========================================================================================
        public override void DeleteFile(string filename) {
            fileSystem.deleteFile(filename);
        }
        //
        //==========================================================================================
        public override string Read(string filename) {
            return fileSystem.readFileText(filename);
        }
        //
        //==========================================================================================
        public override byte[] ReadBinary(string filename) {
            return fileSystem.readFileBinary(filename);
        }
        //
        //==========================================================================================
        public override void Save(string filename, string fileContent) {
            fileSystem.saveFile(filename, fileContent);
        }
        //
        //==========================================================================================
        public override void SaveBinary(string filename, byte[] fileContent) {
            fileSystem.saveFile(filename, fileContent);
        }
        //
        //==========================================================================================
        public override bool FileExists(string pathFileName) {
            return fileSystem.fileExists(pathFileName);
        }
        //
        //==========================================================================================
        public override bool FolderExists(string folderName) {
            return fileSystem.pathExists(folderName);
        }
        //
        //==========================================================================================
        public override List<FileDetail> FileList(string folderName, int pageSize = 0, int pageNumber = 1) {
            return fileSystem.getFileList(folderName);
        }
        //
        //==========================================================================================
        public override List<FolderDetail> FolderList(string folderName) {
            return fileSystem.getFolderList(folderName);
        }
        //
        //==========================================================================================
        public override void DeleteFolder(string folderPath) {
            fileSystem.deleteFolder(folderPath);
        }
        //
        //==========================================================================================
        //
        public override bool SaveUpload(string htmlformName, ref string returnFilename) {
            return fileSystem.upload(htmlformName, "\\upload", ref returnFilename);
        }
        //
        //==========================================================================================
        //
        public override bool SaveUpload(string htmlformName, string folderpath, ref string returnFilename) {
            return fileSystem.upload(htmlformName, folderpath, ref  returnFilename);
        }
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
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