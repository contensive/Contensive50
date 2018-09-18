﻿
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
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
using Contensive.BaseClasses;
using System.Runtime.InteropServices;
using System.IO;
using static Contensive.Processor.Controllers.FileController;
//
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
        public override void append(string filename, string fileContent) {
            fileSystem.appendFile(filename, fileContent);
        }
        //
        //==========================================================================================
        public override void copy(string sourceFilename, string destinationFilename) {
            fileSystem.copyFile(sourceFilename, destinationFilename);
        }
        //
        //==========================================================================================
        public override void createFolder(string folderPath) {
            fileSystem.createPath(folderPath);
        }
        //
        //==========================================================================================
        public override void deleteFile(string filename) {
            fileSystem.deleteFile(filename);
        }
        //
        //==========================================================================================
        public override string read(string filename) {
            return fileSystem.readFileText(filename);
        }
        //
        //==========================================================================================
        public override byte[] readBinary(string filename) {
            return fileSystem.readFileBinary(filename);
        }
        //
        //==========================================================================================
        public override void save(string filename, string fileContent) {
            fileSystem.saveFile(filename, fileContent);
        }
        //
        //==========================================================================================
        public override void saveBinary(string filename, byte[] fileContent) {
            fileSystem.saveFile(filename, fileContent);
        }
        //
        //==========================================================================================
        public override bool fileExists(string pathFileName) {
            return fileSystem.fileExists(pathFileName);
        }
        //
        //==========================================================================================
        public override bool folderExists(string folderName) {
            return fileSystem.pathExists(folderName);
        }
        //
        //==========================================================================================
        public override List<FileDetail> fileList(string folderName, int pageSize = 0, int pageNumber = 1) {
            return fileSystem.getFileList(folderName);
        }
        //
        //==========================================================================================
        public override List<FolderDetail> folderList(string folderName) {
            return fileSystem.getFolderList(folderName);
        }
        //
        //==========================================================================================
        public override void deleteFolder(string folderPath) {
            fileSystem.deleteFolder(folderPath);
        }
        //
        //==========================================================================================
        //
        public override bool saveUpload(string htmlformName, ref string returnFilename) {
            return fileSystem.upload(htmlformName, "\\upload", ref returnFilename);
        }
        //
        //==========================================================================================
        //
        public override bool saveUpload(string htmlformName, string folderpath, ref string returnFilename) {
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
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}