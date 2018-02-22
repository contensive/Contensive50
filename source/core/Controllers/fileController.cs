
using System;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading.Tasks;
using Contensive.Core.Models;
using Contensive.Core.Models.Context;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
using Amazon.Runtime;
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
        /// <summary>
        /// core object
        /// </summary>
        private coreController core;
        /// <summary>
        /// true if the filesystem is local, false if files transfered through the local system to the remote system
        /// </summary>
        private bool isLocal { get; set; }
        /// <summary>
        /// local location for files accessed by this filesystem, starts with drive-letter, ends with dos slash \
        /// </summary>
        public string localAbsRootPath { get; set; }
        /// <summary>
        /// For remote files, this path is prefixed to the content. starts with subfolder name, ends in uniz slash /
        /// </summary>
        private string remotePathPrefix { get; set;  }
        /// <summary>
        /// list of files to delete when this object is disposed
        /// </summary>
         public List<string> deleteOnDisposeFileList { get; set; } = new List<string>();
        /// <summary>
        /// for remote filesystem, a lazy created s3 client
        /// </summary>
        internal AmazonS3Client s3Client {
            get {
                if (_s3Client == null) {
                    Amazon.RegionEndpoint awsRegionEndpoint = RegionEndpoint.GetBySystemName(core.serverConfig.awsBucketRegionName);
                    _s3Client = new AmazonS3Client(core.serverConfig.awsAccessKey, core.serverConfig.awsSecretAccessKey, awsRegionEndpoint);
                };
                return _s3Client;
            }
        }
        private AmazonS3Client _s3Client { get; set; } = null;
        //
        //==============================================================================================================
        /// <summary>
        /// Create a filesystem
        /// </summary>
        /// <param name="core"></param>
        /// <param name="isLocal">If true, thie object reads/saves to the local filesystem</param>
        /// <param name="rootLocalPath"></param>
        /// <param name="remotePathPrefix">If not isLocal, this is added to the remote content path. Ex a\ with content b\c.txt = a\b\c.txt</param>
        public fileController(coreController core, bool isLocal, string rootLocalPath, string remotePathPrefix) {
            if (string.IsNullOrEmpty(rootLocalPath)) {
                core.handleException(new ArgumentException("Blank file system root path not permitted."));
            } else {
                this.core = core;
                this.isLocal = isLocal;
                this.localAbsRootPath = normalizeDosPath(rootLocalPath);
                this.remotePathPrefix = normalizeDosPath(remotePathPrefix);
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// join two paths together to make a single path or filename. changes / to \, and makes sure there is one and only one at the joint
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pathFilename"></param>
        /// <returns></returns>
        public string joinPath(string path, string pathFilename) {
            string returnPath = "";
            try {
                returnPath = normalizeDosPath(path);
                pathFilename = normalizePathFilename(pathFilename);
                if (pathFilename != "\\") {
                    returnPath = Path.Combine(returnPath, pathFilename);
                }
            } catch (Exception ex) {
                core.handleException(ex);
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
                        path = pathFilename.Left(lastSlashPos + 1);
                        filename = pathFilename.Substring(lastSlashPos + 1);
                    } else {
                        path = "";
                        filename = pathFilename;
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        // ====================================================================================================
        //   Read in a file from a given PathFilename, return content
        //
        public string readFileText(string pathFilename) {
            string returnContent = "";
            try {
                if (!string.IsNullOrEmpty(pathFilename)) {
                    if (!isLocal) {
                        // -- copy remote file to local
                        copyRemoteToLocal(pathFilename);
                    }
                    if (fileExists(pathFilename)) {
                        using (StreamReader sr = File.OpenText(convertToLocalAbsPath(pathFilename))) {
                            returnContent = sr.ReadToEnd();
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnContent;
        }
        //
        //==============================================================================================================
        /// <summary>
        /// reads a binary file and returns a byye array
        /// </summary>
        /// <param name="PathFilename"></param>
        /// <returns></returns>
        internal byte[] readFileBinary(string PathFilename) {
            byte[] returnContent = { };
            int bytesRead = 0;
            try {
                if (!string.IsNullOrEmpty(PathFilename)) {
                    if (!isLocal) {
                        // -- copy remote file to local
                        copyRemoteToLocal(PathFilename);
                    }
                    if (fileExists(PathFilename)) {
                        using (FileStream sr = File.OpenRead(convertToLocalAbsPath(PathFilename))) {
                            bytesRead = sr.Read(returnContent, 0, 1000000000);
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnContent;
        }
        //
        //==============================================================================================================
        /// <summary>
        /// save text file
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <param name="FileContent"></param>
        public void saveFile(string pathFilename, string FileContent) {
            saveFile_TextBinary(pathFilename, FileContent, null, false);
        }
        //
        //==============================================================================================================
        /// <summary>
        /// save binary file
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <param name="FileContent"></param>
        public void saveFile(string pathFilename, byte[] FileContent) {
            saveFile_TextBinary(pathFilename, null, FileContent, true);
        }
        //
        //==============================================================================================================
        /// <summary>
        /// save binary or text file
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <param name="textContent"></param>
        /// <param name="binaryContent"></param>
        /// <param name="isBinary"></param>
        private void saveFile_TextBinary(string pathFilename, string textContent, byte[] binaryContent, bool isBinary) {
            try {
                string path = "";
                string filename = "";
                //
                pathFilename = normalizePathFilename(pathFilename);
                if (!isValidDosPathFilename(pathFilename)) {
                    throw new ArgumentException("PathFilename argument is not valid [" + pathFilename + "]");
                } else {
                    //
                    // -- write local file
                    splitPathFilename(pathFilename, ref path, ref filename);
                    if (!pathExists(path)) {
                        createPath(path);
                    }
                    try {
                        if (isBinary) {
                            File.WriteAllBytes(convertToLocalAbsPath(pathFilename), binaryContent);
                        } else {
                            File.WriteAllText(convertToLocalAbsPath(pathFilename), textContent);
                        }
                    } catch (Exception ex) {
                        core.handleException(ex);
                        throw;
                    }
                    if (!isLocal) {
                        // copy to remote
                        copyLocalToRemote(pathFilename);
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// append file async
        /// </summary>
        /// <param name="PathFilename"></param>
        /// <param name="fileContent"></param>
        public void appendFile(string PathFilename, string fileContent) {
            try {
                if (string.IsNullOrEmpty(PathFilename)) {
                    throw new ArgumentException("appendFile called with blank pathname.");
                } else {
                    appendFileBackground(PathFilename, fileContent);
                    //await Task.Run(() => appendFileBackground(PathFilename, fileContent));
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// background task to append files
        /// </summary>
        /// <param name="PathFilename"></param>
        /// <param name="fileContent"></param>
        private void appendFileBackground(string PathFilename, string fileContent) {
            try {
                //
                // -- verify local path
                string absFilename = convertToLocalAbsPath(PathFilename);
                string path = "";
                string filename = "";
                splitPathFilename(PathFilename, ref path, ref filename);
                if (!pathExists(path)) {
                    createPath(path);
                }
                if (!isLocal) {
                    //
                    // -- non-local, copy remote file to local
                    copyRemoteToLocal(PathFilename);
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
                if (!isLocal) {
                    //
                    // -- non-local, copy local file to remote
                    copyLocalToRemote(PathFilename);
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Creates a file folder if it does not exist
        /// </summary>
        /// <param name="absPath"></param>
        private void createPath_local(string absPath) {
            try {
                string PartialPath = null;
                int Position = 0;
                string WorkingPath = null;
                //
                if (string.IsNullOrEmpty(absPath)) {
                    throw new ArgumentException("CreateLocalFileFolder called with blank path.");
                } else {
                    WorkingPath = normalizeDosPath(absPath);
                    if (!Directory.Exists(WorkingPath)) {
                        Position = genericController.vbInstr(1, WorkingPath, "\\");
                        while (Position != 0) {
                            PartialPath = WorkingPath.Left(Position - 1);
                            if (!Directory.Exists(PartialPath)) {
                                Directory.CreateDirectory(PartialPath);
                            }
                            Position = genericController.vbInstr(Position + 1, WorkingPath, "\\");
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Creates a file folder if it does not exist
        /// </summary>
        /// <param name="path"></param>
        public void createPath(string path) {
            try {
                if (isLocal) {
                    string absPath = convertToLocalAbsPath(path);
                    createPath_local(absPath);
                } else {
                    verifyPath_remote(path);
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Deletes a file if it exists
        /// </summary>
        /// <param name="pathFilename"></param>
        public void deleteFile(string pathFilename) {
            try {
                if (!string.IsNullOrEmpty(pathFilename)) {
                    if (isLocal) {
                        deleteFile_local(pathFilename);
                    } else {
                        deleteFile_remote(pathFilename);
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Deletes a file if it exists
        /// </summary>
        /// <param name="pathFilename"></param>
        public void deleteFile_remote(string pathFilename) {
            try {
                // https://aws.amazon.com/blogs/developer/the-three-different-apis-for-amazon-s3/
                // https://docs.aws.amazon.com/sdk-for-net/v2/developer-guide/s3-apis-intro.html
                if (!string.IsNullOrWhiteSpace(pathFilename)) {
                    string remoteUnixPathFilename = genericController.convertToUnixSlash(joinPath(remotePathPrefix, pathFilename));
                    if ( fileExists_remote( remoteUnixPathFilename )) {
                        DeleteObjectRequest deleteObjectRequest = new DeleteObjectRequest {
                            BucketName = core.serverConfig.awsBucketName,
                            Key = remoteUnixPathFilename
                        };
                        s3Client.DeleteObject(deleteObjectRequest);
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Delete a local file if it exists
        /// </summary>
        /// <param name="pathFilename"></param>
        private void deleteFile_local(string pathFilename) {
            try {
                if (!string.IsNullOrEmpty(pathFilename)) {
                    if (fileExists_local(pathFilename)) {
                        File.Delete(convertToLocalAbsPath(pathFilename));
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Delete a folder recursively
        /// </summary>
        /// <param name="PathName"></param>
        public void deleteFolder(string PathName) {
            try {
                if (!string.IsNullOrEmpty(PathName)) {
                    if (!isLocal) {
                        // todo - rewrite using lowlevel + transfer, not file io
                        // https://aws.amazon.com/blogs/developer/the-three-different-apis-for-amazon-s3/
                        string unixPathName = convertToUnixSlash(PathName).Trim();
                        if ((unixPathName.Length > 1) & (unixPathName.Substring(0, 1) == "\\")) {
                            unixPathName = unixPathName.Substring(1);
                        }
                        if (!string.IsNullOrEmpty(unixPathName)) {
                            var parentFolderInfo = new Amazon.S3.IO.S3DirectoryInfo(s3Client, core.serverConfig.awsBucketName, unixPathName);
                            parentFolderInfo.Delete(true);
                        }
                    } else {
                        string localPath = joinPath(localAbsRootPath, PathName);
                        if (localPath.Substring(localPath.Length - 1) == "\\") {
                            localPath = localPath.Left(localPath.Length - 1);
                        }
                        if (pathExists(PathName)) {
                            Directory.Delete(localPath, true);
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Copies a file to another location
        /// </summary>
        /// <param name="srcPathFilename"></param>
        /// <param name="dstPathFilename"></param>
        /// <param name="dstFileSystem"></param>
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
                        // todo - rewrite using lowlevel + transfer, not file io
                        // https://aws.amazon.com/blogs/developer/the-three-different-apis-for-amazon-s3/
                        //
                        // remote file copy
                        verifyPath_remote(getPath(srcPathFilename));
                        string srcRemoteUnixPathFilename = genericController.convertToUnixSlash("/" + joinPath(remotePathPrefix, srcPathFilename));
                        var s3FileInfo = new Amazon.S3.IO.S3FileInfo(s3Client, core.serverConfig.awsBucketName, convertToDosSlash(srcRemoteUnixPathFilename.Substring(1)));
                        if ( s3FileInfo.Exists ) {
                            string dstRemoteUnixPathFilename = genericController.convertToUnixSlash("/" + joinPath(remotePathPrefix, dstPathFilename));
                            Amazon.S3.IO.S3DirectoryInfo remoteDirectoryInfo = new Amazon.S3.IO.S3DirectoryInfo(s3Client, core.serverConfig.awsBucketName, dstRemoteUnixPathFilename);
                            s3FileInfo.CopyTo(remoteDirectoryInfo);
                        }
                    } else {
                        //
                        // -- local file copy
                        if (fileExists(srcPathFilename)) {
                            splitPathFilename(dstPathFilename, ref dstPath, ref dstFilename);
                            if (!dstFileSystem.pathExists(dstPath)) {
                                dstFileSystem.createPath(dstPath);
                            }
                            srcFullPathFilename = joinPath(localAbsRootPath, srcPathFilename);
                            DstFullPathFilename = joinPath(dstFileSystem.localAbsRootPath, dstPathFilename);
                            if (dstFileSystem.fileExists(dstPathFilename)) {
                                dstFileSystem.deleteFile(dstPathFilename);
                            }
                            File.Copy(srcFullPathFilename, DstFullPathFilename);
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// list of files, each row is delimited by a comma
        /// </summary>
        /// <param name="FolderPath"></param>
        /// <returns></returns>
        public FileInfo[] getFileList(string FolderPath) {
            FileInfo[] returnFileInfoList = { };
            try {
                if (!isLocal) {
                    // todo implement remote getFileList
                    throw new NotImplementedException("remote getFileList not implemented");
                } else {
                    if (pathExists(FolderPath)) {
                        string localPath = convertToLocalAbsPath(FolderPath);
                        DirectoryInfo di = new DirectoryInfo(localPath);
                        returnFileInfoList = di.GetFiles();
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnFileInfoList;
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Returns a list of folders in a path, comma delimited
        /// </summary>
        /// <param name="FolderPath"></param>
        /// <returns></returns>
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
                core.handleException(ex);
                throw;
            }
            return returnList;
        }
        //
        //==============================================================================================================
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
                        string localPath = convertToLocalAbsPath(FolderPath);
                        DirectoryInfo di = new DirectoryInfo(localPath);
                        returnFolders = di.GetDirectories();
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnFolders;
        }
        //
        //==============================================================================================================
        /// <summary>
        /// return true if the file exists in the selected file system (local or remote)
        /// </summary>
        /// <param name="pathFilename"></param>
        public bool fileExists(string pathFilename) {
            bool returnOK = false;
            try {
                if (!isLocal) {
                    returnOK = fileExists_remote(pathFilename);
                } else {
                    returnOK = fileExists_local(pathFilename);
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnOK;
        }
        //
        //==============================================================================================================
        /// <summary>
        /// internal method, true if the file exists in the local file system
        /// </summary>
        /// <param name="pathFilename"></param>
        private bool fileExists_local(string pathFilename) {
            bool returnOK = false;
            try {
                string absPathFilename = convertToLocalAbsPath(pathFilename);
                returnOK = File.Exists(absPathFilename);
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnOK;
        }
        //
        //==============================================================================================================
        /// <summary>
        /// internal method, true if the file exists in the remote file system
        /// </summary>
        /// <param name="remoteUnixPathFilename"></param>
        private bool fileExists_remote(string remoteUnixPathFilename) {
            try {
                GetObjectMetadataResponse response = s3Client.GetObjectMetadata(core.serverConfig.awsBucketName, remoteUnixPathFilename);
                return true;
            } catch (Amazon.S3.AmazonS3Exception ex) {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return false;
                //status wasn't not found, so throw the exception
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Returns true if the folder exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool pathExists(string path) {
            bool returnOK = false;
            try {
                if (!isLocal) {
                    throw new NotImplementedException();
                } else {
                    string absPath = convertToLocalAbsPath(path);
                    returnOK = Directory.Exists(absPath);
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnOK;
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Rename a file
        /// </summary>
        /// <param name="SourcePathFilename"></param>
        /// <param name="DestinationFilename"></param>
        public void renameFile(string SourcePathFilename, string DestinationFilename) {
            try {
                if (!isLocal) {
                    // todo remote file case
                    throw new NotImplementedException();
                } else {
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
                            srcFullPathFilename = joinPath(localAbsRootPath, SourcePathFilename);
                            Pos = SourcePathFilename.LastIndexOf("\\") + 1;
                            if (Pos >= 0) {
                                sourceFullPath = SourcePathFilename.Left(Pos);
                            }
                            if (string.IsNullOrEmpty(DestinationFilename)) {
                                throw new ApplicationException("Invalid destination file []");
                            } else if (DestinationFilename.IndexOf("\\") != -1) {
                                throw new ApplicationException("Invalid '\\' character in destination filename [" + DestinationFilename + "]");
                            } else if (DestinationFilename.IndexOf("/") != -1) {
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
                core.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// getDriveFreeSpace
        /// </summary>
        public double getDriveFreeSpace() {
            double returnSize = 0;
            try {
                if (!isLocal) {
                    // todo remote file case
                    throw new NotImplementedException();
                } else {
                    DriveInfo scriptingDrive = null;
                    string driveLetter;
                    //
                    // Drive Space
                    //
                    driveLetter = localAbsRootPath.Left(1);
                    scriptingDrive = new DriveInfo(driveLetter);
                    if (scriptingDrive.IsReady) {
                        returnSize = scriptingDrive.AvailableFreeSpace;
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnSize;
        }
        //
        //==============================================================================================================
        /// <summary>
        /// copy one folder to another, include subfolders
        /// </summary>
        /// <param name="absSrcFolder"></param>
        /// <param name="absDstFolder"></param>
        private void copyFolder_local(string absSrcFolder, string absDstFolder) {
            try {
                if (Directory.Exists(absSrcFolder)) {
                    //
                    // -- create destination folder
                    if (!Directory.Exists(absDstFolder)) {
                        createPath_local(absDstFolder);
                    }
                    //
                    DirectoryInfo srcDirectoryInfo = new DirectoryInfo(absSrcFolder);
                    DirectoryInfo dstDiretoryInfo = new DirectoryInfo(absDstFolder);
                    DirectoryInfo dstCopy = null;
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
                        copyFolder_local(srcCopy.FullName, dstCopy.FullName);
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// copy one folder to another, include subfolders
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="dstPath"></param>
        /// <param name="dstFileSystem"></param>
        public void copyFolder(string srcPath, string dstPath, fileController dstFileSystem = null) {
            try {
                if (!isLocal) {
                    //
                    // -- remote src filesystem
                    // todo - implement remote copyFolder
                    throw new NotImplementedException("remote copyFolder not implemented");
                } else {
                    //
                    // -- create destination folder
                    if (dstFileSystem == null) {
                        dstFileSystem = this;
                    }
                    //
                    // -- copy files
                    copyFolder_local(joinPath(localAbsRootPath, srcPath), joinPath(dstFileSystem.localAbsRootPath, dstPath));
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// return true if pathFilename is a valid DOS local path
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <returns></returns>
        private bool isValidDosPathFilename(string pathFilename) {
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
        /// <summary>
        /// convert fileInfo array to parsable string [filename-attributes-creationTime-lastAccessTime-lastWriteTime-length-entension]
        /// </summary>
        /// <param name="FileInfo"></param>
        /// <returns></returns>
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
        /// <summary>
        /// convert directoryInfo object to parsable string [filename-attributes-creationTime-lastAccessTime-lastWriteTime-extension]
        /// </summary>
        /// <param name="DirectoryInfo"></param>
        /// <returns></returns>
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
        //=========================================================================================================
        /// <summary>
        /// saveHttpRequestToFile
        /// </summary>
        /// <param name="Link"></param>
        /// <param name="pathFilename"></param>
        public void saveHttpRequestToFile(string Link, string pathFilename) {
            try {
                //
                httpRequestController HTTP = new httpRequestController();
                string URLLink = null;
                //
                if ((!string.IsNullOrEmpty(pathFilename)) & (!string.IsNullOrEmpty(Link))) {
                    pathFilename = normalizePathFilename(pathFilename);
                    URLLink = genericController.vbReplace(Link, " ", "%20");
                    HTTP.timeout = 600;
                    HTTP.getUrlToFile(encodeText(URLLink), convertToLocalAbsPath(pathFilename));
                    //
                    if (!isLocal) {
                        copyLocalToRemote(pathFilename);
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Unzip a zipfile
        /// </summary>
        /// <param name="pathFilename"></param>
        public void UnzipFile(string pathFilename) {
            try {
                if ( !isLocal ) {
                    copyRemoteToLocal(pathFilename);
                }
                string absPathFilename = convertToLocalAbsPath(pathFilename);
                string path = "";
                string filename = "";
                splitPathFilename(absPathFilename, ref path, ref filename);
                string fileFilter = null;
                FastZip fastZip = new FastZip();
                fastZip.ExtractZip(absPathFilename, path, fileFilter);
                //
                if (!isLocal) {
                    //
                    // -- copy files back to remote
                    using (var fs = new FileStream(absPathFilename, FileMode.Open, FileAccess.Read)) {
                        using (var zf = new ZipFile(fs)) {
                            foreach (ZipEntry ze in zf) {
                                if (ze.IsDirectory) {
                                    verifyPath_remote(getPath(ze.Name));
                                } else {
                                    copyLocalToRemote(ze.Name);
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
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
                fastZip.CreateZip(convertToLocalAbsPath(archivePathFilename), convertToLocalAbsPath(addPathFilename), recurse, fileFilter);
            } catch (Exception ex) {
                core.handleException(ex);
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
        private string convertToLocalAbsPath(string pathFilename) {
            string result = pathFilename;
            try {
                string normalizedPathFilename = normalizePathFilename(pathFilename);
                if (string.IsNullOrEmpty(normalizedPathFilename)) {
                    result = localAbsRootPath;
                } else if (isinLocalAbsDosPath(normalizedPathFilename)) {
                    result = normalizedPathFilename;
                } else if (normalizedPathFilename.IndexOf(":\\") >= 0) {
                    throw new ApplicationException("Attempt to access an invalid path [" + normalizedPathFilename + "] that is not within the allowed path [" + localAbsRootPath + "].");
                } else {
                    result = joinPath(localAbsRootPath, normalizedPathFilename);
                }
            } catch (Exception ex) {
                core.handleException(ex);
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
        /// <param name="dosPath"></param>
        /// <returns></returns>
        public static string normalizeDosPath(string dosPath) {
            if (string.IsNullOrEmpty(dosPath)) {
                return string.Empty;
            } else {
                dosPath = normalizePathFilename(dosPath);
                if (dosPath.Left(1) == "\\") {
                    dosPath = dosPath.Substring(1);
                }
                if (dosPath.Substring(dosPath.Length - 1, 1) != "\\") {
                    return dosPath + "\\";
                } else {
                    return dosPath;
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns true only if the path is an absolute path and it is within the filesystems root path. False if not absolute path or absolute path not in filesystem
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool isinLocalAbsDosPath(string path) {
            return (normalizeDosPath(path).ToLower().IndexOf(localAbsRootPath.ToLower()) == 0);
        }
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
                if (core.docProperties.containsKey(key)) {
                    var docProperty = core.docProperties.getProperty(key);
                    if ((docProperty.IsFile) && (docProperty.Name.ToLower() == key)) {
                        string dosPathFilename = fileController.normalizeDosPath(path);
                        returnFilename = encodeFilename(docProperty.Value);
                        dosPathFilename += returnFilename;
                        deleteFile(dosPathFilename);
                        if (docProperty.tempfilename != "") {
                            //
                            // copy tmp private files to the appropriate folder in the destination file system
                            //
                            core.tempFiles.copyFile(docProperty.tempfilename, dosPathFilename, this);
                            //
                            if (!isLocal) {
                                copyLocalToRemote(dosPathFilename);
                            }
                            success = true;
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return success;
        }
        //
        //========================================================================
        //
        public static string getVirtualTableFieldPath(string TableName, string FieldName) {
            string result = TableName + "/" + FieldName + "/";
            return result.Replace(" ", "_").Replace(".", "_");
        }
        //
        //========================================================================
        //
        public static string getVirtualTableFieldIdPath(string TableName, string FieldName, int RecordID) {
            return getVirtualTableFieldPath(TableName, FieldName) + RecordID.ToString().PadLeft(12, '0') + "/";
        }
        //
        //========================================================================
        /// <summary>
        /// Create a filename for the Virtual Directory for a fieldtypeFile or Image (an uploaded file)
        /// </summary>
        public static string getVirtualRecordPathFilename(string TableName, string FieldName, int RecordID, string OriginalFilename) {
            string iOriginalFilename = OriginalFilename.Replace(" ", "_").Replace(".", "_");
            return getVirtualTableFieldIdPath(TableName, FieldName, RecordID) + OriginalFilename;
        }
        //
        //========================================================================
        /// <summary>
        /// Create a filename for the virtual directory for field types not associated to upload files
        /// </summary>
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
        public int getFileSize(string pathFilename) {
            int fileSize = 0;
            try {
                if (!isLocal) {
                    // todo implement remote getFileList
                    throw new NotImplementedException("remote getFileSize not implemented");
                } else {
                    FileInfo[] files = getFileList(pathFilename);
                    fileSize = (int)(files[0].Length);
                }
            } catch (Exception ex ) {
                core.handleException(ex);
            }
            return fileSize;
        }
        //
        //====================================================================================================
        /// <summary>
        /// copy a file (object) up to s3
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <param name="dstS3UnixPathFilename"></param>
        /// <returns></returns>
        public bool copyLocalToRemote(string pathFilename) {
            bool result = false;
            try {
                string localDosPathFilename = genericController.convertToDosSlash(pathFilename);
                string remoteUnixPathFilename = genericController.convertToUnixSlash(joinPath(remotePathPrefix, pathFilename));
                verifyPath_remote(getPath(pathFilename));
                //
                // -- Setup request for putting an object in S3.
                PutObjectRequest request = new PutObjectRequest();
                request.BucketName = core.serverConfig.awsBucketName;
                request.Key = remoteUnixPathFilename;
                request.FilePath = joinPath(localAbsRootPath, localDosPathFilename);
                //
                // -- Make service call and get back the response.
                PutObjectResponse response = s3Client.PutObject(request);
                result = true;
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// copy a file (object) down from s3. The localDosPath must exist, and the file should NOT exist. If remote file does
        /// not exist
        /// </summary>
        public bool copyRemoteToLocal(string pathFilename) {
            bool result = false;
            try {
                // todo - rewrite using lowlevel + transfer, not file io
                // https://aws.amazon.com/blogs/developer/the-three-different-apis-for-amazon-s3/
                string remoteUnixPathFilename = genericController.convertToUnixSlash("/" + joinPath(remotePathPrefix, pathFilename));
                verifyPath_remote(getPath( pathFilename ));
                var s3FileInfo = new Amazon.S3.IO.S3FileInfo(s3Client, core.serverConfig.awsBucketName, convertToDosSlash(remoteUnixPathFilename.Substring(1)));
                if (s3FileInfo.Exists) {
                    string localDosPathFilename = genericController.convertToDosSlash(pathFilename);
                    //
                    // -- remote file exists, delete local version and copy remote to local
                    if (fileExists(localDosPathFilename)) {
                        //
                        // -- local file exists, delete it first
                        deleteFile(localDosPathFilename);
                    }
                    GetObjectRequest request = new GetObjectRequest {
                        BucketName = core.serverConfig.awsBucketName,
                        Key = remoteUnixPathFilename.Substring(1)
                    };
                    try {
                        using (GetObjectResponse response = s3Client.GetObject(request)) {
                            response.WriteResponseStreamToFile(joinPath(localAbsRootPath, localDosPathFilename));
                        }
                    } catch (Exception) {

                        throw;
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// verify a path exist within the local filesystem
        /// </summary>
        private void verifyPath_local(string path) {
            try {
                if (!pathExists(path)) {
                    createPath(path);
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// if remote path does not exist, it is created
        /// </summary>
        private void verifyPath_remote(string path) {
            try {
                string remoteUnixPathFilename = genericController.convertToUnixSlash("/" + joinPath(remotePathPrefix, path));
                var url = genericController.splitUrl(remoteUnixPathFilename);
                var parentFolderInfo = new Amazon.S3.IO.S3DirectoryInfo(s3Client, core.serverConfig.awsBucketName, "");
                string pathFromLeft = "";
                foreach (string segment in url.pathSegments) {
                    pathFromLeft += "/" + segment;
                    var subFolderInfo = new Amazon.S3.IO.S3DirectoryInfo(s3Client, core.serverConfig.awsBucketName, pathFromLeft.Substring(1));
                    if (!subFolderInfo.Exists) {
                        parentFolderInfo.CreateSubdirectory(segment);
                    }
                    parentFolderInfo = subFolderInfo;
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// copy a file (object) down from s3. The localDosPath must exist, and the file should NOT exist. If remote file does
        /// not exist
        /// </summary>
        private void verifyPath(string path) {
            try {
                if (isLocal) {
                    // -- files stored locally
                    verifyPath_local(path);
                } else {
                    // -- files transfered through local to remote
                    verifyPath_local(path);
                    verifyPath_remote(path);
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //====================================================================================================
        // dispose
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
                    if (_s3Client != null) {
                        _s3Client.Dispose();
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
