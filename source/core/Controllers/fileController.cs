
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
using ICSharpCode.SharpZipLib.Zip;
using Contensive.Core.Models;
using Contensive.Core.Models.Context;
using System.Threading.Tasks;
//
namespace Contensive.Core.Controllers {
    //
    //==============================================================================================================
    /// <summary>
    /// Basic file access class for all scaling targets (cden, private, approot, etc)
    /// modes:
    ///   ephemeralFiles - local file access
    ///       Access to the local server relative to the rootLocalFolder
    ///       If no rootLocalFolder provided, full drive:filepath available
    ///       
    ///   passiveSync - local copy is updated only when needed (privateFiles, cdnFiles)
    ///       save - saves to a local filesystem      mirror, copied to S3 folder without public access
    ///       read - test s3 source for update, conditionally copied, read from local copy
    ///       
    ///   activeSync - local copy is updated on change (appRoot Files)
    ///       save - saves to a local folder, copied to S3 folder without public access, other webRoles copy changed files
    ///       read - read from local folder
    /// </summary>
    public class fileController : IDisposable {
        //
        //====================================================================================================
        /// <summary>
        /// 
        /// </summary>
        public enum fileSyncModeEnum {
            // noSync = file is written locally and read locallay
            noSync = 1,
            // passiveSync = (slow read, always consistent) FileSave - is written locally and uploaded to s3. FileRead - a check is made for current version and downloaded if neede, then read
            passiveSync = 2,
            // activeSync = (fast read, eventual consistency) FileSave - files written locally and uploaded to s3, then automatically downloaded to the other app clients for read. FileRead - read locally
            activeSync = 3
        }
        //
        private coreClass cpCore;
        private bool isLocal { get; set; }
        public string rootLocalPath { get; set; } // path ends in \, folder ends in foldername
        private string clusterFileEndpoint { get; set; }
        private fileSyncModeEnum fileSyncMode { get; set; }
        public List<string> deleteOnDisposeFileList { get; set; } = new List<string>(); // tmp file list of files that need to be deleted during dispose
                                                                                        //
                                                                                        //==============================================================================================================
                                                                                        /// <summary>
                                                                                        /// Create a filesystem
                                                                                        /// </summary>
                                                                                        /// <param name="cpCore"></param>
                                                                                        /// <param name="isLocal">If true, thie object reads/saves to the local filesystem</param>
                                                                                        /// <param name="rootLocalPath"></param>
                                                                                        /// <param name="remoteFileEndpoint">If not isLocal, this endpoint is used for file sync</param>
        public fileController(coreClass cpCore, bool isLocal, fileSyncModeEnum fileSyncMode, string rootLocalPath, string remoteFileEndpoint = "") {
            if (string.IsNullOrEmpty(rootLocalPath)) {
                cpCore.handleException(new ArgumentException("Blank file system root path not permitted."));
            } else {
                this.cpCore = cpCore;
                this.isLocal = isLocal;
                this.clusterFileEndpoint = remoteFileEndpoint;
                this.fileSyncMode = fileSyncMode;
                this.rootLocalPath = normalizePath(rootLocalPath);
                //CreatefullPath(rootLocalPath)
            }
        }
        //
        //==============================================================================================================
        //
        // join two paths together to make a single path or filename
        //   changes / to \, and makes sure there is one and only one at the joint
        //
        public string joinPath(string path, string pathFilename) {
            string returnPath = "";
            try {
                returnPath = normalizePath(path);
                pathFilename = normalizePathFilename(pathFilename);
                if (pathFilename != "\\") {
                    returnPath =Path.Combine(returnPath, pathFilename);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnPath;
        }
        //
        //==============================================================================================================
        /// <summary>
        /// return a path and a filename from a pathFilename
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        public void splitPathFilename(string pathFilename, ref string path, ref string filename) {
            try {
                if (string.IsNullOrEmpty(pathFilename)) {
                    throw new ArgumentException("pathFilename cannot be blank");
                } else {
                    pathFilename = normalizePathFilename(pathFilename);
                    int lastSlashPos = pathFilename.LastIndexOf("\\");
                    if (lastSlashPos >= 0) {
                        path = pathFilename.Left( lastSlashPos + 1);
                        filename = pathFilename.Substring(lastSlashPos + 1);
                    } else {
                        path = "";
                        filename = pathFilename;
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        //
        //
        //========================================================================
        //   Read in a file from a given PathFilename, return content
        //
        //   Do error trapping alittle different, always fixme-- cpCore.handleException(New ApplicationException("")) ' -----ccObjectError_UnderlyingObject, , ErrorDescription) and print
        //   something out if there was a problem.
        //
        //========================================================================
        //
        public string readFile(string PathFilename) {
            string returnContent = "";
            try {
                if (string.IsNullOrEmpty(PathFilename)) {
                    //
                    // Not an error because an empty pathname returns an empty result
                    //
                } else {
                    if (!isLocal) {
                        // check local cache, download if needed
                    }
                    if (fileExists(PathFilename)) {
                        using (StreamReader sr = File.OpenText(convertToAbsPath(PathFilename))) {
                            returnContent = sr.ReadToEnd();
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnContent;
        }
        //
        //========================================================================
        //
        public byte[] ReadBinaryFile(string PathFilename) {
            byte[] returnContent = { };
            int bytesRead = 0;
            try {
                if (string.IsNullOrEmpty(PathFilename)) {
                    //
                    // Not an error because an empty pathname returns an empty result
                    //
                } else {
                    if (!isLocal) {
                        // check local cache, download if needed
                    }
                    if (fileExists(PathFilename)) {
                        using (FileStream sr = File.OpenRead(convertToAbsPath(PathFilename))) {
                            bytesRead = sr.Read(returnContent, 0, 1000000000);
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnContent;
        }
        //
        //==============================================================================================================
        //
        //========================================================================
        //   Save data to a file
        //========================================================================
        //
        public void saveFile(string pathFilename, string FileContent) {
            SaveDualFile(pathFilename, FileContent, null, false);
        }
        //
        //==============================================================================================================
        //
        public void SaveFile(string pathFilename, byte[] FileContent) {
            SaveDualFile(pathFilename, null, FileContent, true);
        }
        //
        //==============================================================================================================
        //
        private void SaveDualFile(string pathFilename, string textContent, byte[] binaryContent, bool isBinary) {
            try {
                string path = "";
                string filename = "";
                //
                pathFilename = normalizePathFilename(pathFilename);
                if (!isValidPathFilename(pathFilename)) {
                    throw new ArgumentException("PathFilename argument is not valid [" + pathFilename + "]");
                } else {
                    splitPathFilename(pathFilename, ref path, ref filename);
                    if (!pathExists(path)) {
                        createPath(path);
                    }
                    try {
                        if (isBinary) {
                            File.WriteAllBytes(convertToAbsPath(pathFilename), binaryContent);
                        } else {
                            File.WriteAllText(convertToAbsPath(pathFilename), textContent);
                        }
                    } catch (Exception ex) {
                        cpCore.handleException(ex);
                        throw;
                    }
                    if (!isLocal) {
                        // s3 transfer
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        //
        //========================================================================
        //   append data to a file
        //       problem -- you cannot append with s3
        //           - move logging to simploeDb
        //           - rename this appendLocalFile + add syncLocalFile (moves it to s3)
        //========================================================================
        //
        public void appendFile(string PathFilename, string fileContent) {
            try {
                if (string.IsNullOrEmpty(PathFilename)) {
                    throw new ArgumentException("appendFile called with blank pathname.");
                } else {
                    Task t = Task.Run(() => appendFileBackground(PathFilename, fileContent));
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        private void appendFileBackground( string PathFilename, string fileContent) {
            string absFilename = convertToAbsPath(PathFilename);
            string path = "";
            string filename = "";
            splitPathFilename(PathFilename, ref path, ref filename);
            if (!pathExists(path)) {
                createPath(path);
            }
            if (!File.Exists(absFilename)) {
                using (StreamWriter sw = File.CreateText(absFilename)) {
                    sw.Write(fileContent);
                }
            } else {
                using (StreamWriter sw = File.AppendText(absFilename)) {
                    sw.Write(fileContent);
                }
            }
            //If Not clusterConfig.isLocal Then
            //    ' s3 transfer
            //End If
        }
        //
        //==============================================================================================================
        //
        //========================================================================
        //   append data to a file
        //       problem -- you cannot append with s3
        //           - move logging to simploeDb
        //           - rename this appendLocalFile + add syncLocalFile (moves it to s3)
        //========================================================================
        //
        public void syncLocalFile(string PathFilename, string FileContent) {
            if (!isLocal) {
                // s3 transfer
            }
        }
        //
        //==============================================================================================================
        //
        //========================================================================
        // ----- Creates a file folder if it does not exist
        //========================================================================
        //
        public void CreatefullPath(string physicalFolderPath) {
            try {
                string PartialPath = null;
                int Position = 0;
                string WorkingPath = null;
                //
                if (string.IsNullOrEmpty(physicalFolderPath)) {
                    throw new ArgumentException("CreateLocalFileFolder called with blank path.");
                } else {
                    WorkingPath = normalizePath(physicalFolderPath);
                    if (!Directory.Exists(WorkingPath)) {
                        Position = genericController.vbInstr(1, WorkingPath, "\\");
                        while (Position != 0) {
                            PartialPath = WorkingPath.Left( Position - 1);
                            if (!Directory.Exists(PartialPath)) {
                                Directory.CreateDirectory(PartialPath);
                            }
                            Position = genericController.vbInstr(Position + 1, WorkingPath, "\\");
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        //
        //========================================================================
        // ----- Creates a file folder if it does not exist
        //========================================================================
        //
        public void createPath(string FolderPath) {
            try {
                string abspath = convertToAbsPath(FolderPath);
                CreatefullPath(abspath);
                if (!isLocal) {
                    // s3 transfer
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        //
        //========================================================================
        //   Deletes a file if it exists
        //========================================================================
        //
        public void deleteFile(string PathFilename) {
            try {
                if (string.IsNullOrEmpty(PathFilename)) {
                    //
                    // not an error because the pathfile already does not exist
                    //
                } else {
                    if (fileExists(PathFilename)) {
                        File.Delete(convertToAbsPath(PathFilename));
                    }
                    if (!isLocal) {
                        // s3 transfer
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        //
        //========================================================================
        //   Deletes a file if it exists
        //========================================================================
        //
        public void deleteFolder(string PathName) {
            try {
                if (!string.IsNullOrEmpty(PathName)) {
                    string localPath = joinPath(rootLocalPath, PathName);
                    if (localPath.Substring(localPath.Length - 1) == "\\") {
                        localPath = localPath.Left( localPath.Length - 1);
                    }
                    if (pathExists(PathName)) {
                        Directory.Delete(localPath, true);
                    }
                    if (!isLocal) {
                        // s3 transfer
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        //
        //========================================================================
        //   Copies a file to another location
        //========================================================================
        //
        public void copyFile(string srcPathFilename, string dstPathFilename, fileController dstFileSystem = null) {
            try {
                string dstPath = "";
                string dstFilename = "";
                string srcFullPathFilename = null;
                string DstFullPathFilename = null;
                //
                if (dstFileSystem == null) {
                    dstFileSystem = this;
                }
                if (string.IsNullOrEmpty(srcPathFilename)) {
                    throw new ArgumentException("Invalid source file.");
                } else if (string.IsNullOrEmpty(dstPathFilename)) {
                    throw new ArgumentException("Invalid destination file.");
                } else {
                    srcPathFilename = normalizePathFilename(srcPathFilename);
                    dstPathFilename = normalizePathFilename(dstPathFilename);
                    if (!isLocal) {
                        // s3 transfer
                    } else {
                        if (!fileExists(srcPathFilename)) {
                            //
                            // not an error, to minimize file use, empty files are not created, so missing files are just empty
                            //
                        } else {
                            splitPathFilename(dstPathFilename, ref dstPath, ref dstFilename);
                            if (!dstFileSystem.pathExists(dstPath)) {
                                dstFileSystem.createPath(dstPath);
                            }
                            srcFullPathFilename = joinPath(rootLocalPath, srcPathFilename);
                            DstFullPathFilename = joinPath(dstFileSystem.rootLocalPath, dstPathFilename);
                            if (dstFileSystem.fileExists(dstPathFilename)) {
                                dstFileSystem.deleteFile(dstPathFilename);
                            }
                            File.Copy(srcFullPathFilename, DstFullPathFilename);
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        //
        // list of files, each row is delimited by a comma
        //
        public FileInfo[] getFileList(string FolderPath) {
            FileInfo[] returnFileInfoList = { };
            try {
                if (!isLocal) {
                    // s3 transfer
                } else {
                    if (pathExists(FolderPath)) {
                        string localPath = convertToAbsPath(FolderPath);
                        DirectoryInfo di = new DirectoryInfo(localPath);
                        returnFileInfoList = di.GetFiles();
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnFileInfoList;
        }
        //
        //==============================================================================================================
        //
        //========================================================================
        //   Returns a list of folders in a path, comma delimited
        //========================================================================
        //
        public string getFolderNameList(string FolderPath) {
            string returnList = "";
            try {
                System.IO.DirectoryInfo[] di = getFolderList(FolderPath);
                foreach (System.IO.DirectoryInfo d in di) {
                    returnList += "," + d.Name;
                }
                if (!string.IsNullOrEmpty(returnList)) {
                    returnList = returnList.Substring(1);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnList;
        }
        //
        //==============================================================================================================
        //
        //
        //
        public DirectoryInfo[] getFolderList(string FolderPath) {
            DirectoryInfo[] returnFolders = { };
            try {
                if (!isLocal) {
                    returnFolders = new DirectoryInfo[0];
                } else {
                    if (!pathExists(FolderPath)) {
                        returnFolders = new DirectoryInfo[0];
                    } else {
                        string localPath = convertToAbsPath(FolderPath);
                        DirectoryInfo di = new DirectoryInfo(localPath);
                        returnFolders = di.GetDirectories();
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnFolders;
        }
        //
        //==============================================================================================================
        //
        //   Returns true if the file exists
        //
        public bool fileExists(string pathFilename) {
            bool returnOK = false;
            try {
                if (!isLocal) {
                } else {
                    string localPathFilename = convertToAbsPath(pathFilename);
                    returnOK = File.Exists(localPathFilename);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnOK;
        }
        //
        //==============================================================================================================
        //
        //   Returns true if the folder exists
        //
        public bool pathExists(string path) {
            bool returnOK = false;
            try {
                if (!isLocal) {
                } else {
                    string absPath = convertToAbsPath(path);
                    returnOK = Directory.Exists(absPath);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnOK;
        }
        //
        //==============================================================================================================
        //
        //   Rename a file
        //
        public void renameFile(string SourcePathFilename, string DestinationFilename) {
            try {
                int Pos = 0;
                string sourceFullPath = "";
                string srcFullPathFilename = null;
                //
                if (string.IsNullOrEmpty(SourcePathFilename)) {
                    throw new ApplicationException("Invalid source file");
                } else {
                    if (!isLocal) {
                    } else {
                        SourcePathFilename = normalizePathFilename(SourcePathFilename);
                        srcFullPathFilename = joinPath(rootLocalPath, SourcePathFilename);
                        Pos = SourcePathFilename.LastIndexOf("\\") + 1;
                        if (Pos >= 0) {
                            sourceFullPath = SourcePathFilename.Left( Pos);
                        }
                        if (true) {
                            if (string.IsNullOrEmpty(DestinationFilename)) {
                                throw new ApplicationException("Invalid destination file []");
                            } else if (DestinationFilename.IndexOf("\\")  != -1) {
                                throw new ApplicationException("Invalid '\\' character in destination filename [" + DestinationFilename + "]");
                            } else if (DestinationFilename.IndexOf("/")  != -1) {
                                throw new ApplicationException("Invalid '/' character in destination filename [" + DestinationFilename + "]");
                            } else if (!fileExists(SourcePathFilename)) {
                                //
                                // not an error, to minimize file use, empty files are not created, so missing files are just empty
                                //
                            } else {
                                File.Move(srcFullPathFilename, joinPath(sourceFullPath, DestinationFilename));
                            }
                        }
                    }
                }

            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        //
        //
        //
        public double getDriveFreeSpace() {
            double returnSize = 0;
            try {
                DriveInfo scriptingDrive = null;
                string driveLetter;
                //
                // Drive Space
                //
                driveLetter = rootLocalPath.Left( 1);
                scriptingDrive = new DriveInfo(driveLetter);
                if (scriptingDrive.IsReady) {
                    returnSize = scriptingDrive.AvailableFreeSpace;
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnSize;
        }
        //
        //==============================================================================================================
        //
        // copy one folder to another, include subfolders
        //
        private void copyLocalFileFolder(string physicalSrc, string physicalDst) {
            try {
                if (!Directory.Exists(physicalSrc)) {
                    //
                    // -- source does not exist
                } else {
                    //
                    // -- create destination folder
                    if (!Directory.Exists(physicalDst)) {
                        CreatefullPath(physicalDst);
                    }
                    //
                    DirectoryInfo srcDirectoryInfo = new DirectoryInfo(physicalSrc);
                    DirectoryInfo dstDiretoryInfo = new DirectoryInfo(physicalDst);
                    DirectoryInfo dstCopy = null;
                    //todo  NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                    //					DirectoryInfo srcCopy = null;
                    //
                    // copy each file into destination
                    //
                    foreach (FileInfo srcFile in srcDirectoryInfo.GetFiles()) {
                        srcFile.CopyTo(joinPath(dstDiretoryInfo.ToString(), srcFile.Name), true);
                    }
                    //
                    // recurse through folders
                    //
                    foreach (DirectoryInfo srcCopy in srcDirectoryInfo.GetDirectories()) {
                        dstCopy = dstDiretoryInfo.CreateSubdirectory(srcCopy.Name);
                        copyLocalFileFolder(srcCopy.FullName, dstCopy.FullName);
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        //
        // copy one folder to another, include subfolders
        //
        public void copyFolder(string srcPath, string dstPath, fileController dstFileSystem = null) {
            try {
                if (!isLocal) {
                    //
                    // s3
                    //
                } else {
                    //
                    // create destination folder
                    //
                    if (dstFileSystem == null) {
                        dstFileSystem = this;
                    }
                    copyLocalFileFolder(joinPath(rootLocalPath, srcPath), joinPath(dstFileSystem.rootLocalPath, dstPath));
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        //
        //
        //
        private bool isValidPathFilename(string pathFilename) {
            bool returnValid = true;
            if (string.IsNullOrEmpty(pathFilename)) {
                returnValid = false;
            } else if (pathFilename.IndexOf("/") >= 0) {
                returnValid = false;
            }
            return returnValid;
        }
        //
        //==============================================================================================================
        public string convertFileINfoArrayToParseString(FileInfo[] FileInfo) {
            string returnString = "";
            if (FileInfo.Length > 0) {
                foreach (FileInfo fi in FileInfo) {
                    returnString += "\r\n" + fi.Name + "\t" + (int)fi.Attributes + "\t" + fi.CreationTime + "\t" + fi.LastAccessTime + "\t" + fi.LastWriteTime + "\t" + fi.Length + "\t" + fi.Extension;
                }
                returnString = returnString.Substring(2);
            }
            return returnString;
        }
        //
        //==============================================================================================================
        public string convertDirectoryInfoArrayToParseString(DirectoryInfo[] DirectoryInfo) {
            string returnString = "";
            if (DirectoryInfo.Length > 0) {
                foreach (DirectoryInfo di in DirectoryInfo) {
                    returnString += "\r\n" + di.Name + "\t" + (int)di.Attributes + "\t" + di.CreationTime + "\t" + di.LastAccessTime + "\t" + di.LastWriteTime + "\t0\t" + di.Extension;
                }
                returnString = returnString.Substring(2);
            }
            return returnString;
        }
        //
        //==============================================================================================================
        //
        //=========================================================================================================
        //
        //=========================================================================================================
        //
        public void SaveRemoteFile(string Link, string pathFilename) {
            try {
                //
                httpRequestController HTTP = new httpRequestController();
                string URLLink = null;
                //
                if ((!string.IsNullOrEmpty(pathFilename)) & (!string.IsNullOrEmpty(Link))) {
                    pathFilename = normalizePathFilename(pathFilename);
                    URLLink = genericController.vbReplace(Link, " ", "%20");
                    HTTP.timeout = 600;
                    HTTP.getUrlToFile(encodeText(URLLink), convertToAbsPath(pathFilename));
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        //
        //=======================================================================================
        //   Unzip a zipfile
        //
        //   Must be called from a process running as admin
        //   This can be done using the command queue, which kicks off the ccCmd process from the Server
        //=======================================================================================
        //
        public void UnzipFile(string PathFilename) {
            try {
                //
          FastZip fastZip = new FastZip();
                string fileFilter = null;
                string absPathFilename = null;
                string path = "";
                string filename = "";
                //
                absPathFilename = convertToAbsPath(PathFilename);
                splitPathFilename(absPathFilename, ref path, ref filename);
                fastZip.ExtractZip(absPathFilename, path, fileFilter);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        //
        //=======================================================================================
        //   Unzip a zipfile
        //
        //   Must be called from a process running as admin
        //   This can be done using the command queue, which kicks off the ccCmd process from the Server
        //=======================================================================================
        //
        public void zipFile(string archivePathFilename, string addPathFilename) {
            try {
                //
                FastZip fastZip = new FastZip();
                string fileFilter = null;
                bool recurse = true;
                string archivepath = "";
                string archiveFilename = "";
                //
                splitPathFilename(archivePathFilename, ref archivepath, ref archiveFilename);
                fastZip.CreateZip(convertToAbsPath(archivePathFilename), convertToAbsPath(addPathFilename), recurse, fileFilter);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// convert a path argument (relative to rootPath) into a full absolute path. Allow for the case where the path is incorrectly a full path within the rootpath
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <returns></returns>
        private string convertToAbsPath(string pathFilename) {
            string result = pathFilename;
            try {
                string normalizedPathFilename = normalizePathFilename(pathFilename);
                if (string.IsNullOrEmpty(normalizedPathFilename)) {
                    result = rootLocalPath;
                } else if (isinPhysicalPath(normalizedPathFilename)) {
                    result = normalizedPathFilename;
                } else if (normalizedPathFilename.IndexOf(":\\") >= 0) {
                    throw new ApplicationException("Attempt to access an invalid path [" + normalizedPathFilename + "] that is not within the allowed path [" + rootLocalPath + "].");
                } else {
                    result = joinPath(rootLocalPath, normalizedPathFilename);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Argument can be a path (myPath/), a filename (myfile.bin), or a pathFilename (myPath/myFile.bin)
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <returns></returns>
        public static string normalizePathFilename(string path) {
            if (string.IsNullOrEmpty(path)) {
                return string.Empty;
            } else {
                string returnPath = path;
                returnPath = returnPath.Replace("/", "\\");
                while (returnPath.IndexOf("\\\\") >= 0) {
                    returnPath = returnPath.Replace("\\\\", "\\");
                }
                return returnPath;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Ensures a path uses the correct file delimiter "\", and ends in a "\"
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string normalizePath(string path) {
            if (string.IsNullOrEmpty(path)) {
                return string.Empty;
            } else {
                path = normalizePathFilename(path);
                if (path.Left( 1) == "\\") {
                    path = path.Substring(1);
                }
                if (path.Substring(path.Length - 1, 1) != "\\") {
                    return path + "\\";
                } else {
                    return path;
                }
            }
        }

        //
        //====================================================================================================
        public bool isinPhysicalPath(string path) {
            return (normalizePath(path).ToLower().IndexOf(rootLocalPath.ToLower()) == 0);
        }
        //
        // save uploaded file (used to be in html_ classes)
        //
        //
        //========================================================================
        // <summary>
        // process the request for an input file, storing the file system provided, in an optional filePath. Return the pathFilename uploaded. The filename is returned as a byref argument.
        // </summary>
        // <param name="TagName"></param>
        // <param name="files"></param>
        // <param name="filePath"></param>
        // <returns></returns>
        //Public Function saveUpload(ByVal TagName As String, ByVal filePath As String) As String
        //    Dim returnFilename As String = ""
        //    Return web_processFormInputFile(TagName, files, filePath, returnFilename)
        //End Function
        //
        //========================================================================
        /// <summary>
        /// save an uploaded file to a path, and return the uploaded filename
        /// </summary>
        /// <param name="TagName"></param>
        /// <param name="files"></param>
        /// <param name="filePath"></param>
        /// <param name="returnFilename"></param>
        /// <returns></returns>
        public bool upload(string htmlTagName, string path, ref string returnFilename) {
            bool success = false;
            try {
                returnFilename = "";
                //
                string key = htmlTagName.ToLower();
                if (cpCore.docProperties.containsKey(key)) {
                    var tempVar = cpCore.docProperties.getProperty(key);
                    if ((tempVar.IsFile) && (tempVar.Name.ToLower() == key)) {
                        string returnPathFilename = fileController.normalizePath(path);
                        returnFilename = encodeFilename(tempVar.Value);
                        returnPathFilename += returnFilename;
                        deleteFile(returnPathFilename);
                        if (tempVar.tempfilename != "") {
                            //
                            // copy tmp private files to the appropriate folder in the destination file system
                            //
                            cpCore.tempFiles.copyFile(tempVar.tempfilename, returnPathFilename, this);
                            success = true;
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return success;
        }
        //
        public static string getVirtualTableFieldPath(string TableName, string FieldName) {
            string result = TableName + "/" + FieldName + "/";
            return result.Replace(" ", "_").Replace(".", "_");
        }
        public static string getVirtualTableFieldIdPath(string TableName, string FieldName, int RecordID) {
            return getVirtualTableFieldPath(TableName, FieldName) + RecordID.ToString().PadLeft(12, '0') + "/";
        }
        //
        //========================================================================
        /// <summary>
        /// Create a filename for the Virtual Directory for a fieldtypeFile or Image (an uploaded file)
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="FieldName"></param>
        /// <param name="RecordID"></param>
        /// <param name="OriginalFilename"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static string getVirtualRecordPathFilename(string TableName, string FieldName, int RecordID, string OriginalFilename) {
            string result = "";
            //
            string iOriginalFilename = OriginalFilename.Replace(" ", "_").Replace(".", "_");
            return getVirtualTableFieldIdPath(TableName, FieldName, RecordID) + OriginalFilename;
        }
        //
        //========================================================================
        /// <summary>
        /// Create a filename for the virtual directory for field types not associated to upload files
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="FieldName"></param>
        /// <param name="RecordID"></param>
        /// <param name="OriginalFilename"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static string getVirtualRecordPathFilename(string TableName, string FieldName, int RecordID, int fieldType) {
            string result = "";
            string IdFilename = RecordID.ToString();
            if (RecordID == 0) {
                IdFilename = getGUID().Replace("{", "").Replace("}", "").Replace("-", "");
            } else {
                IdFilename = RecordID.ToString().PadLeft(12, '0');
            }
            switch (fieldType) {
                case FieldTypeIdFileCSS:
                    result = getVirtualTableFieldPath(TableName, FieldName) + IdFilename + ".css";
                    break;
                case FieldTypeIdFileXML:
                    result = getVirtualTableFieldPath(TableName, FieldName) + IdFilename + ".xml";
                    break;
                case FieldTypeIdFileJavascript:
                    result = getVirtualTableFieldPath(TableName, FieldName) + IdFilename + ".js";
                    break;
                case FieldTypeIdFileHTML:
                    result = getVirtualTableFieldPath(TableName, FieldName) + IdFilename + ".html";
                    break;
                default:
                    result = getVirtualTableFieldPath(TableName, FieldName) + IdFilename + ".txt";
                    break;
            }
            return result;
        }
        //
        //========================================================================
        //
        public int main_GetFileSize(string VirtualFilePathPage) {
            FileInfo[] files = getFileList(VirtualFilePathPage);
            return (int)(files[0].Length);
        }
        //====================================================================================================
        // dispose
        //====================================================================================================
        //
        #region  IDisposable Support 
        //
        protected bool disposed = false;
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                //Call appendDebugLog(".dispose, dereference main, csv")
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    if (deleteOnDisposeFileList.Count > 0) {
                        foreach (string filename in deleteOnDisposeFileList) {
                            deleteFile(filename);
                        }
                    }
                }
                //
                // Add code here to release the unmanaged resource.
                //
                //FileSystem = Nothing
            }
            this.disposed = true;
        }
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~fileController() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}
