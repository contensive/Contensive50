
using System;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using System.Linq;
using static Contensive.BaseClasses.CPFileSystemBaseClass;
using Contensive.Processor.Exceptions;
using Contensive.BaseClasses;

namespace Contensive.Processor.Controllers {
    //
    //==============================================================================================================
    /// <summary>
    /// Basic file access class for all scaling targets (cdn, private, appRoot, etc).
    /// set isLocal true and all files are handled on the local server
    /// set isLocal false:
    /// - the local filesystem becomes a mirror for the remote system
    /// - after a transaction is complete (read,write,copy) the local file can be deleted
    /// - on remote system, ALL filenames will be converted lowercase
    /// 
    /// </summary>
    public class FileController : IDisposable {
        /// <summary>
        /// core object
        /// </summary>
        private CoreController core;
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
        /// <summary>
        /// list of paths verified during the scope of this execution. If a path is deleted, it must be removed from this list
        /// </summary>
        private List<string> verifiedRemotePathList = new List<string>();
        //
        //==============================================================================================================
        /// <summary>
        /// Create a filesystem
        /// </summary>
        /// <param name="core"></param>
        /// <param name="isLocal">If true, thie object reads/saves to the local filesystem</param>
        /// <param name="rootLocalPath"></param>
        /// <param name="remotePathPrefix">If not isLocal, this is added to the remote content path. Ex a\ with content b\c.txt = a\b\c.txt</param>
        public FileController(CoreController core, bool isLocal, string rootLocalPath, string remotePathPrefix) {
            if (string.IsNullOrEmpty(rootLocalPath)) {
                LogController.logError( core,new ArgumentException("Blank file system root path not permitted."));
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
                pathFilename = normalizeDosPathFilename(pathFilename);
                returnPath = Path.Combine(returnPath, pathFilename);
                //if (pathFilename != "\\") {
                //    returnPath = Path.Combine(returnPath, pathFilename);
                //}
            } catch (Exception ex) {
                LogController.logError( core,ex);
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
        public void splitDosPathFilename(string pathFilename, ref string path, ref string filename) {
            try {
                path = "";
                filename = "";
                if (!string.IsNullOrWhiteSpace(pathFilename)) {
                    pathFilename = normalizeDosPathFilename(pathFilename);
                    int lastSlashPos = pathFilename.LastIndexOf("\\");
                    if (lastSlashPos >= 0) {
                        path = pathFilename.Left(lastSlashPos + 1);
                        filename = pathFilename.Substring(lastSlashPos + 1);
                    } else {
                        filename = pathFilename;
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// return the path and filename with unix slashes
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        public void splitUnixPathFilename(string pathFilename, ref string path, ref string filename) {
            splitDosPathFilename(pathFilename, ref path, ref filename);
            path = convertToUnixSlash(path);
        }
        //
        // ====================================================================================================
        //   Read in a file from a given PathFilename, return content
        //
        public string readFileText(string pathFilename) {
            string returnContent = "";
            try {
                if (!string.IsNullOrEmpty(pathFilename)) {
                    pathFilename = normalizeDosPathFilename(pathFilename);
                    if (!isLocal) {
                        //
                        // -- copy remote file to local
                        if (!copyFileRemoteToLocal(pathFilename)) {
                            //
                            // -- if remote file does not exist, delete local mirror
                            deleteFile_local(pathFilename);
                        }
                    }
                    if (fileExists_local(pathFilename)) {
                        using (StreamReader sr = File.OpenText(convertRelativeToLocalAbsPath(pathFilename))) {
                            returnContent = sr.ReadToEnd();
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
            return returnContent;
        }
        //
        //==============================================================================================================
        /// <summary>
        /// reads a binary file and returns a byye array
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <returns></returns>
        internal byte[] readFileBinary(string pathFilename) {
            byte[] returnContent = { };
            int bytesRead = 0;
            try {
                if (!string.IsNullOrEmpty(pathFilename)) {
                    pathFilename = normalizeDosPathFilename(pathFilename);
                    if (!isLocal) {
                        //
                        // -- copy remote file to local
                        if (!copyFileRemoteToLocal(pathFilename)) {
                            //
                            // -- if remote file does not exist, delete local mirror
                            deleteFile_local(pathFilename);
                        }
                    }
                    if (fileExists(pathFilename)) {
                        using (FileStream sr = File.OpenRead(convertRelativeToLocalAbsPath(pathFilename))) {
                            bytesRead = sr.Read(returnContent, 0, 1000000000);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
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
        /// <param name="pathFilename">Path and filename. Path can be empty or start with folder and end in dos or uniz slash.</param>
        /// <param name="textContent"></param>
        /// <param name="binaryContent"></param>
        /// <param name="isBinary"></param>
        private void saveFile_TextBinary(string pathFilename, string textContent, byte[] binaryContent, bool isBinary) {
            try {
                pathFilename = normalizeDosPathFilename(pathFilename);
                //
                // -- write local file
                string path = "";
                string filename = "";
                splitDosPathFilename(pathFilename, ref path, ref filename);
                verifyPath(path);
                //if (!pathExists(path)) {
                //    createPath(path);
                //}
                try {
                    if (isBinary) {
                        File.WriteAllBytes(convertRelativeToLocalAbsPath(pathFilename), binaryContent);
                    } else {
                        File.WriteAllText(convertRelativeToLocalAbsPath(pathFilename), textContent);
                    }
                } catch (Exception ex) {
                    LogController.logError( core,ex);
                    throw;
                }
                if (!isLocal) {
                    // copy to remote
                    copyFileLocalToRemote(pathFilename);
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// background task to append files
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <param name="fileContent"></param>
        public void appendFile(string pathFilename, string fileContent) {
            try {
                if (string.IsNullOrWhiteSpace(pathFilename)) {
                    throw new ArgumentException("appendFile called with blank pathname.");
                } else if (!string.IsNullOrEmpty(fileContent)) {
                    //
                    // -- verify local path
                    string absFilename = convertRelativeToLocalAbsPath(pathFilename);
                    string path = "";
                    string filename = "";
                    splitDosPathFilename(pathFilename, ref path, ref filename);
                    verifyPath(path);
                    //if (!pathExists(path)) {
                    //    createPath(path);
                    //}
                    if (!isLocal) {
                        //
                        // -- non-local, copy remote file to local
                        if (!copyFileRemoteToLocal(pathFilename)) {
                            deleteFile_local(pathFilename);
                        }
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
                        copyFileLocalToRemote(pathFilename);
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Creates a file folder if it does not exist
        /// </summary>
        /// <param name="path"></param>
        private void createPath_local(string path) {
            createPathAbs_local(convertRelativeToLocalAbsPath(path));
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Creates a file folder if it does not exist
        /// </summary>
        /// <param name="path"></param>
        private void createPathAbs_local(string absPath) {
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
                        Position = GenericController.vbInstr(1, WorkingPath, "\\");
                        while (Position != 0) {
                            PartialPath = WorkingPath.Left(Position - 1);
                            if (!Directory.Exists(PartialPath)) {
                                Directory.CreateDirectory(PartialPath);
                            }
                            Position = GenericController.vbInstr(Position + 1, WorkingPath, "\\");
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Creates a file folder if it does not exist
        /// </summary>
        /// <param name="pathFolder"></param>
        public void createPath(string pathFolder) {
            try {
                if (!isLocal) {
                    //
                    // -- veriofy remote path only for remote mode
                    verifyPath_remote(pathFolder);
                }
                //
                // todo - consider making a different method that verifies the local path for cases like this...
                // -- always verify local path. Added for collection folder case so developer will see path they need to work in.
                createPath_local(pathFolder);
            } catch (Exception ex) {
                LogController.logError( core,ex);
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
                        deleteFile_local(pathFilename);
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
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
                pathFilename = normalizeDosPathFilename(pathFilename);
                if (!string.IsNullOrWhiteSpace(pathFilename)) {
                    string remoteUnixPathFilename = convertToUnixSlash(joinPath(remotePathPrefix, pathFilename));
                    if ( fileExists_remote(pathFilename)) {
                        DeleteObjectRequest deleteObjectRequest = new DeleteObjectRequest {
                            BucketName = core.serverConfig.awsBucketName,
                            Key = remoteUnixPathFilename
                        };
                        s3Client.DeleteObject(deleteObjectRequest);
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
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
                pathFilename = normalizeDosPathFilename(pathFilename);
                if (!string.IsNullOrEmpty(pathFilename)) {
                    if (fileExists_local(pathFilename)) {
                        File.Delete(convertRelativeToLocalAbsPath(pathFilename));
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Delete a folder recursively
        /// </summary>
        /// <param name="path"></param>
        public void deleteFolder(string path) {
            try {
                path = normalizeDosPath(path);
                if (!string.IsNullOrEmpty(path)) {
                    if (!isLocal) {
                        // todo - rewrite using lowlevel + transfer, not file io
                        // https://aws.amazon.com/blogs/developer/the-three-different-apis-for-amazon-s3/
                        string unixPathName = joinPath(remotePathPrefix , path).Trim();
                        if ((unixPathName.Length > 1) && (unixPathName.Substring(0, 1) == "\\")) {
                            unixPathName = unixPathName.Substring(1);
                        }
                        if (!string.IsNullOrEmpty(unixPathName)) {
                            var parentFolderInfo = new Amazon.S3.IO.S3DirectoryInfo(s3Client, core.serverConfig.awsBucketName, unixPathName);
                            parentFolderInfo.Delete(true);
                        }
                    } else {
                    }
                    string localPath = joinPath(localAbsRootPath, path);
                    if (localPath.Substring(localPath.Length - 1) == "\\") {
                        localPath = localPath.Left(localPath.Length - 1);
                    }
                    if (pathExists_local(path)) {
                        Directory.Delete(localPath, true);
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Copies a file to a different file system (FileAppRoot, FileTemp, FileCdn, FilePrivate)
        /// </summary>
        /// <param name="srcPathFilename"></param>
        /// <param name="dstPathFilename"></param>
        /// <param name="dstFileSystem"></param>
        public void copyFile(string srcPathFilename, string dstPathFilename, FileController dstFileSystem) {
            string hint = "src [" + srcPathFilename + "], dst [" + dstPathFilename + "]";
            try {
                if (string.IsNullOrEmpty(srcPathFilename)) {
                    throw new ArgumentException("Invalid source file.");
                } else if (string.IsNullOrEmpty(dstPathFilename)) {
                    throw new ArgumentException("Invalid destination file.");
                } else {
                    hint += ",normalize";
                    srcPathFilename = normalizeDosPathFilename(srcPathFilename);
                    dstPathFilename = normalizeDosPathFilename(dstPathFilename);
                    if (!isLocal) {
                        //
                        // src is remote file - copy file to local mirror
                        hint += ",!isLocal";
                        if (fileExists_remote(srcPathFilename)) {
                            verifyPath_remote(getPath(srcPathFilename));
                            hint += ",copyRemoteToLocal";
                            copyFileRemoteToLocal(srcPathFilename);
                            //
                            // -- copy src to dst on local mirror
                            string dstPath = "";
                            string dstFilename = "";
                            splitDosPathFilename(dstPathFilename, ref dstPath, ref dstFilename);
                            if (!dstFileSystem.pathExists_local(dstPath)) {
                                dstFileSystem.createPath_local(dstPath);
                            }
                            string srcFullPathFilename = joinPath(localAbsRootPath, srcPathFilename);
                            string dstFullPathFilename = joinPath(dstFileSystem.localAbsRootPath, dstPathFilename);
                            if (!dstFileSystem.fileExists_local(dstPathFilename)) {
                                hint += ",does not exist on local dst [" + dstPathFilename + "]";
                            } else { 
                                hint += ",delete local dst [" + dstPathFilename + "]";
                                dstFileSystem.deleteFile_local(dstPathFilename);
                            }
                            hint += ",File.copy";
                            File.Copy(srcFullPathFilename, dstFullPathFilename);
                            if (!dstFileSystem.isLocal) {
                                //
                                // -- dst is remote, copy file to remote source
                                dstFileSystem.copyFileLocalToRemote(dstPathFilename);
                            }
                        }
                    } else {
                        //
                        // -- src is local file, copy to dst local
                        hint += "isLocal";
                        if (fileExists_local(srcPathFilename)) {
                            string dstPath = "";
                            string dstFilename = "";
                            splitDosPathFilename(dstPathFilename, ref dstPath, ref dstFilename);
                            if (!dstFileSystem.pathExists_local(dstPath)) {
                                dstFileSystem.createPath_local(dstPath);
                            }
                            string srcFullPathFilename = joinPath(localAbsRootPath, srcPathFilename);
                            string DstFullPathFilename = joinPath(dstFileSystem.localAbsRootPath, dstPathFilename);
                            if (dstFileSystem.fileExists(dstPathFilename)) {
                                dstFileSystem.deleteFile(dstPathFilename);
                            }
                            File.Copy(srcFullPathFilename, DstFullPathFilename);
                            if (!dstFileSystem.isLocal) {
                                //
                                // -- dst is remote, copy file to remote source
                                dstFileSystem.copyFileLocalToRemote(dstPathFilename);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex, hint);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Copy a file within the same filesystem (FileAppRoot, FileTemp, FileCdn, FilePrivate)
        /// </summary>
        /// <param name="srcPathFilename"></param>
        /// <param name="dstPathFilename"></param>
        public void copyFile(string srcPathFilename, string dstPathFilename) {
            copyFile(srcPathFilename, dstPathFilename, this);
        }
        //
        //==============================================================================================================
        /// <summary>
        /// list of files from the appropriate local/remote filesystem
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<BaseClasses.CPFileSystemBaseClass.FileDetail> getFileList(string path) {
            var returnFileList = new List<FileDetail>();
            try {
                if (!isLocal) {
                    return getFileList_remote(path);
                } else {
                    return getFileList_local(path);
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// list of files from the remote server
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<BaseClasses.CPFileSystemBaseClass.FileDetail> getFileList_remote(string path) {
            try {
                path = normalizeDosPath(path);
                string unixPath = convertToUnixSlash(joinPath(remotePathPrefix, path));
                ListObjectsRequest request = new ListObjectsRequest {
                    BucketName = core.serverConfig.awsBucketName,
                    Prefix = unixPath
                };
                // Build your call out to S3 and store the response
                ListObjectsResponse response = s3Client.ListObjects(request);
                IEnumerable<S3Object> fileList = response.S3Objects.Where(x => !x.Key.EndsWith(@"/"));
                var returnFileList = new List<FileDetail>();
                foreach (var file in fileList) {
                    //
                    // -- create a fileDetail for each file found
                    string fileName = file.Key;
                    string keyPath = "";
                    int pos = fileName.LastIndexOf("/");
                    if (pos > -1) {
                        keyPath = fileName.Substring(0, pos + 1);
                        fileName = fileName.Substring(pos + 1);
                    }
                    if (unixPath.Equals(keyPath)) {
                        returnFileList.Add(new FileDetail() {
                            Attributes = 0,
                            Type = "",
                            DateCreated = file.LastModified,
                            DateLastAccessed = file.LastModified,
                            DateLastModified = file.LastModified,
                            Name = fileName,
                            Size = file.Size
                        });

                    }
                };
                return returnFileList;
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// list of files from the local server
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<BaseClasses.CPFileSystemBaseClass.FileDetail> getFileList_local(string path) {
            var returnFileList = new List<FileDetail>();
            try {
                path = normalizeDosPath(path);
                if (pathExists_local(path)) {
                    string localPath = convertRelativeToLocalAbsPath(path);
                    DirectoryInfo di = new DirectoryInfo(localPath);
                    foreach (var file in di.GetFiles()) {
                        //
                        // -- create a fileDetail for each file found
                        returnFileList.Add(new FileDetail() {
                            Attributes = (int)file.Attributes,
                            DateCreated = file.CreationTime,
                            DateLastAccessed = file.LastAccessTime,
                            DateLastModified = file.LastWriteTime,
                            Name = file.Name,
                            Size = file.Length,
                            Type = ""
                        });
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return returnFileList;
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Returns a list of folders in a path, comma delimited
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string getFolderNameList(string path) {
            string returnList = "";
            try {
                path = normalizeDosPath(path);
                if (!isLocal) {
                    foreach (var folder in getFolderList_remote(path)) {
                        returnList += "," + folder.Name;
                    }
                    if (!string.IsNullOrEmpty(returnList)) {
                        returnList = returnList.Substring(1);
                    }
                } else {
                    foreach (FolderDetail folder in getFolderList_local(path)) {
                        returnList += "," + folder.Name;
                    }
                    if (!string.IsNullOrEmpty(returnList)) {
                        returnList = returnList.Substring(1);
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
            return returnList;
        }
        //
        //==============================================================================================================
        //
        public List<BaseClasses.CPFileSystemBaseClass.FolderDetail> getFolderList(string path) {
            var returnFolders = new List<FolderDetail>();
            try {
                if (!isLocal) {
                    return getFolderList_remote(path);
                } else {
                    return getFolderList_local(path);
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        //
        public List<BaseClasses.CPFileSystemBaseClass.FolderDetail> getFolderList_remote(string path) {
            try {
                path = normalizeDosPath(path);
                string unixPath = convertToUnixSlash(joinPath(remotePathPrefix, path));
                //path = normalizeDosPath(path);
                ListObjectsRequest request = new ListObjectsRequest {
                    BucketName = core.serverConfig.awsBucketName,
                    Prefix = unixPath,
                    Delimiter = @"/"                     
                };
                int prefixLength = unixPath.Length;
                // Build your call out to S3 and store the response
                var returnFolders = new List<FolderDetail>();
                ListObjectsResponse response = s3Client.ListObjects(request);
                foreach( var commonPrefix in response.CommonPrefixes) {
                    string subFolder = commonPrefix.Substring(prefixLength);
                    if (string.IsNullOrWhiteSpace(subFolder)) { continue; }
                    // -- remove trailing slash as this returns folder names, not paths (path ends in slash, folder name ends in the name)
                    subFolder = subFolder.Substring(0, subFolder.Length - 1);
                    // -- skip subfolders as they match the ends-with-a-slash query
                    if (subFolder.Contains("/")) { continue; }
                    //if (subFolder.IndexOf("/") > -1) { continue; }
                    returnFolders.Add(new FolderDetail() {
                        Attributes = 0,
                        Type = "",
                        DateCreated = DateTime.MinValue,
                        DateLastAccessed = DateTime.MinValue,
                        DateLastModified = DateTime.MinValue,
                        Name = subFolder
                    });
                }
                //IEnumerable<S3Object> folderList = response.S3Objects;
                ////IEnumerable<S3Object> folderList = response.S3Objects.Where(x => x.Key.EndsWith(@"/") && x.Size == 0);
                //var returnFolders = new List<FolderDetail>();
                //int startIndex = unixPath.Length;
                //foreach (var folder in folderList) {
                //    string subFolder = folder.Key.Substring(startIndex);
                //    // -- skip the path being queried (blank subfolder)
                //    if (string.IsNullOrWhiteSpace(subFolder)) { continue; }
                //    // -- remove trailing slash as this returns folder names, not paths (path ends in slash, folder name ends in the name)
                //    subFolder = subFolder.Substring(0,subFolder.Length - 1);
                //    // -- skip subfolders as they match the ends-with-a-slash query
                //    if (subFolder.Contains("/")) { continue; }
                //    //if (subFolder.IndexOf("/") > -1) { continue; }
                //    returnFolders.Add(new FolderDetail() {
                //        Attributes = 0,
                //        Type = "",
                //        DateCreated = folder.LastModified,
                //        DateLastAccessed = folder.LastModified,
                //        DateLastModified = folder.LastModified,
                //        Name = subFolder
                //    });
                //};
                return returnFolders;
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        //
        public List<BaseClasses.CPFileSystemBaseClass.FolderDetail> getFolderList_local(string path) {
            try {
                path = normalizeDosPath(path);
                var returnFolders = new List<FolderDetail>();
                if (pathExists_local(path)) {
                    string localPath = convertRelativeToLocalAbsPath(path);
                    DirectoryInfo di = new DirectoryInfo(localPath);
                    foreach (var folder in di.GetDirectories()) {
                        returnFolders.Add(new FolderDetail() {
                            Attributes = (int)folder.Attributes,
                            Type = "",
                            DateCreated = folder.CreationTime,
                            DateLastAccessed = folder.LastWriteTime,
                            DateLastModified = folder.LastWriteTime,
                            Name = folder.Name
                        });
                    }
                }
                return returnFolders;
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
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
                pathFilename = normalizeDosPathFilename(pathFilename);
                if (!isLocal) {
                    returnOK = fileExists_remote(pathFilename);
                } else {
                    returnOK = fileExists_local(pathFilename);
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
            return returnOK;
        }
        //
        //==============================================================================================================
        /// <summary>
        /// internal method, true if the file exists in the local file system
        /// </summary>
        /// <param name="dosPathFilename"></param>
        private bool fileExists_local(string dosPathFilename) {
            bool returnOK = false;
            try {
                string absDosPathFilename = convertRelativeToLocalAbsPath(dosPathFilename);
                returnOK = File.Exists(absDosPathFilename);
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
            return returnOK;
        }
        //
        //==============================================================================================================
        /// <summary>
        /// internal method, true if the file exists in the remote file system
        /// </summary>
        /// <param name="pathFilename"></param>
        private bool fileExists_remote(string pathFilename) {
            try {
                string unixAbsPathFilename = convertToUnixSlash(joinPath(remotePathPrefix, pathFilename));
                string path = "";
                string filename = "";
                // no, cannot change case here. paths should be lcase before the call, file case should be preserved.
                splitUnixPathFilename(unixAbsPathFilename, ref path, ref filename);
                //splitUnixPathFilename(unixAbsPathFilename.ToLowerInvariant(), ref pathLowercase, ref filenameLowercase);
                string s3Key = convertToDosSlash(path);
                Amazon.S3.IO.S3DirectoryInfo s3DirectoryInfo = new Amazon.S3.IO.S3DirectoryInfo(s3Client, core.serverConfig.awsBucketName, s3Key);
                return s3DirectoryInfo.GetFiles(filename).Any();
            } catch (Amazon.S3.AmazonS3Exception ex) {
                //
                // -- support this unwillingly
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound) {
                    return false;
                }
                throw;
            } catch (Exception ex ) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        
        //
        //==============================================================================================================
        /// <summary>
        /// Returns true if the folder exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool pathExists(string path) {
            try {
                if (!isLocal) {
                    //
                    // -- remote
                    return pathExists_remote(path);
                } else {
                    //
                    // -- local
                    return pathExists_local(path);
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Returns true if the local folder exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool pathExists_local(string path) {
            bool returnOk = false; 
            try {
                string absPath = convertRelativeToLocalAbsPath(path);
                returnOk = Directory.Exists(absPath);
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
            return returnOk;
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Returns true if the remote folder exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool pathExists_remote(string path) {
            try {
                //
                // -- remote
                path = normalizeDosPath(path);
                string remoteUnixPathFilename = convertToUnixSlash("/" + joinPath(remotePathPrefix, path));
                var url = GenericController.splitUrl(remoteUnixPathFilename);
                var parentFolderInfo = new Amazon.S3.IO.S3DirectoryInfo(s3Client, core.serverConfig.awsBucketName, "");
                string dosPathFromLeft = "";
                foreach (string segment in url.pathSegments) {
                    dosPathFromLeft += segment + "\\";
                    var subFolderInfo = new Amazon.S3.IO.S3DirectoryInfo(s3Client, core.serverConfig.awsBucketName, dosPathFromLeft.Substring(0, (dosPathFromLeft.Length - 1)));
                    if (!subFolderInfo.Exists) {
                        return false;
                    }
                    parentFolderInfo = subFolderInfo;
                }
                return true;
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// Rename a file
        /// </summary>
        /// <param name="srcPathFilename"></param>
        /// <param name="dstFilename"></param>
        public void renameFile(string srcPathFilename, string dstFilename) {
            try {
                srcPathFilename = normalizeDosPathFilename(srcPathFilename);
                if (string.IsNullOrEmpty(srcPathFilename)) {
                    throw new GenericException("Invalid source file");
                } else {
                    if (!isLocal) {
                        string dstPath = getPath(srcPathFilename);
                        copyFile(srcPathFilename, dstPath + dstFilename);
                        deleteFile(srcPathFilename);
                    } else {
                        string srcFullPathFilename = joinPath(localAbsRootPath, srcPathFilename);
                        int Pos = srcPathFilename.LastIndexOf("\\") + 1;
                        string sourceFullPath = "";
                        if (Pos >= 0) {
                            sourceFullPath = srcPathFilename.Left(Pos);
                        }
                        if (string.IsNullOrEmpty(dstFilename)) {
                            throw new GenericException("Invalid destination file []");
                        } else if (dstFilename.IndexOf("\\") != -1) {
                            throw new GenericException("Invalid '\\' character in destination filename [" + dstFilename + "]");
                        } else if (dstFilename.IndexOf("/") != -1) {
                            throw new GenericException("Invalid '/' character in destination filename [" + dstFilename + "]");
                        } else if (!fileExists(srcPathFilename)) {
                            //
                            // not an error, to minimize file use, empty files are not created, so missing files are just empty
                            //
                        } else {
                            File.Move(srcFullPathFilename, joinPath(sourceFullPath, dstFilename));
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
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
                    returnSize = 1000000000;
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
                LogController.logError( core,ex);
                throw;
            }
            return returnSize;
        }
        //
        //==============================================================================================================
        /// <summary>
        /// copy one folder to another, include subfolders
        /// </summary>
        /// <param name="srcAbsDosPath"></param>
        /// <param name="absDstFolder"></param>
        private void copyFolder_srcLocal(string srcAbsDosPath, string dstDosPath, FileController dstFileSystem = null) {
            try {
                if (Directory.Exists(srcAbsDosPath)) {
                    if (dstFileSystem == null ) {
                        dstFileSystem = this;
                    }
                    string dstAbsDstPath = joinPath(dstFileSystem.localAbsRootPath, dstDosPath);
                    //
                    // -- create destination folder
                    if (!Directory.Exists(dstAbsDstPath)) {
                        createPathAbs_local(dstAbsDstPath);
                    }
                    DirectoryInfo srcDirectoryInfo = new DirectoryInfo(srcAbsDosPath);
                    DirectoryInfo dstDiretoryInfo = new DirectoryInfo(dstAbsDstPath);
                    //
                    // -- copy each file
                    foreach (FileInfo srcFile in srcDirectoryInfo.GetFiles()) {
                        srcFile.CopyTo(joinPath(dstDiretoryInfo.ToString(), srcFile.Name), true);
                        if (!dstFileSystem.isLocal) {
                            //
                            // -- now copy the dst file to the remote
                            dstFileSystem.copyFileLocalToRemote(joinPath(dstDosPath, srcFile.Name));
                        }
                    }
                    //
                    // -- copy each folder
                    foreach (DirectoryInfo srcSubDirectory in srcDirectoryInfo.GetDirectories()) {
                        string dstFolder = srcSubDirectory.Name;
                        string dstSubPath = dstDosPath + dstFolder + "\\";
                        DirectoryInfo dstSubDirectory = dstDiretoryInfo.CreateSubdirectory(dstFolder);
                        copyFolder_srcLocal(srcSubDirectory.FullName, dstSubPath, dstFileSystem);
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// copy one folder to another, include subfolders
        /// </summary>
        /// <param name="srcAbsDosPath"></param>
        /// <param name="absDstFolder"></param>
        private void copyFolder_srcRemote(string srcAbsDosPath, string dstDosPath, FileController dstFileSystem = null) {
            try {
                if (Directory.Exists(srcAbsDosPath)) {
                    if (dstFileSystem == null) {
                        dstFileSystem = this;
                    }
                    string dstAbsDstPath = joinPath(dstFileSystem.localAbsRootPath, dstDosPath);
                    //
                    // -- create destination folder
                    if (!Directory.Exists(dstAbsDstPath)) {
                        createPathAbs_local(dstAbsDstPath);
                    }
                    DirectoryInfo srcDirectoryInfo = new DirectoryInfo(srcAbsDosPath);
                    DirectoryInfo dstDiretoryInfo = new DirectoryInfo(dstAbsDstPath);
                    //
                    // -- copy each file
                    foreach (FileInfo srcFile in srcDirectoryInfo.GetFiles()) {
                        srcFile.CopyTo(joinPath(dstDiretoryInfo.ToString(), srcFile.Name), true);
                        if (!dstFileSystem.isLocal) {
                            //
                            // -- now copy the dst file to the remote
                            dstFileSystem.copyFileLocalToRemote(joinPath(dstDosPath, srcFile.Name));
                        }
                    }
                    //
                    // -- copy each folder
                    foreach (DirectoryInfo srcSubDirectory in srcDirectoryInfo.GetDirectories()) {
                        string dstFolder = srcSubDirectory.Name;
                        string dstSubPath = dstDosPath + dstFolder + "\\";
                        DirectoryInfo dstSubDirectory = dstDiretoryInfo.CreateSubdirectory(dstFolder);
                        copyFolder_srcLocal(srcSubDirectory.FullName, dstSubPath, dstFileSystem);
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
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
        public void copyFolder(string srcPath, string dstPath, FileController dstFileSystem = null) {
            try {
                srcPath = normalizeDosPath(srcPath);
                dstPath = normalizeDosPath(dstPath);
                if (!isLocal) {
                    //
                    // -- src remote
                    copyFolder_srcRemote(joinPath(localAbsRootPath, srcPath), dstPath, dstFileSystem);
                } else {
                    //
                    // -- src local
                    copyFolder_srcLocal(joinPath(localAbsRootPath, srcPath), dstPath, dstFileSystem);
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
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
                pathFilename = normalizeDosPathFilename(pathFilename);
                if ((!string.IsNullOrEmpty(pathFilename)) && (!string.IsNullOrEmpty(Link))) {
                    string URLLink = GenericController.vbReplace(Link, " ", "%20");
                    HttpRequestController HTTP = new HttpRequestController();
                    HTTP.timeout = 600;
                    HTTP.getUrlToFile(encodeText(URLLink), convertRelativeToLocalAbsPath(pathFilename));
                    //
                    if (!isLocal) {
                        copyFileLocalToRemote(pathFilename);
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
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
                pathFilename = normalizeDosPathFilename(pathFilename);
                bool processLocalFile = true;
                if ( !isLocal ) {
                    if (!copyFileRemoteToLocal(pathFilename)) {
                        processLocalFile = false;
                        deleteFile_local(pathFilename);
                    }
                }
                if (processLocalFile) {
                    string path = "";
                    string filename = "";
                    splitDosPathFilename(pathFilename, ref path, ref filename);
                    string absPathFilename = convertRelativeToLocalAbsPath(pathFilename);
                    string absPath = "";
                    splitDosPathFilename(absPathFilename, ref absPath, ref filename);
                    string fileFilter = null;
                    FastZip fastZip = new FastZip();
                    fastZip.ExtractZip(absPathFilename, absPath, fileFilter);
                    //
                    if (!isLocal) {
                        //
                        // -- copy files back to remote
                        using (var fs = new FileStream(absPathFilename, FileMode.Open, FileAccess.Read)) {
                            using (var zf = new ZipFile(fs)) {
                                foreach (ZipEntry ze in zf) {
                                    if (ze.IsDirectory) {
                                        verifyPath_remote(getPath( joinPath(path, ze.Name)));
                                    } else {
                                        copyFileLocalToRemote(joinPath(path, ze.Name));
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        //
        public void zipFile(string archivePathFilename, string addPathFilename) {
            try {
                archivePathFilename = normalizeDosPathFilename(archivePathFilename);
                addPathFilename = normalizeDosPathFilename(addPathFilename);
                //
                string archivepath = "";
                string archiveFilename = "";
                splitDosPathFilename(archivePathFilename, ref archivepath, ref archiveFilename);
                FastZip fastZip = new FastZip();
                string fileFilter = null;
                bool recurse = true;
                fastZip.CreateZip(convertRelativeToLocalAbsPath(archivePathFilename), convertRelativeToLocalAbsPath(addPathFilename), recurse, fileFilter);
            } catch (Exception ex) {
                LogController.logError( core,ex);
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
        public string convertRelativeToLocalAbsPath(string pathFilename) {
            string result = pathFilename;
            try {
                string normalizedPathFilename = normalizeDosPathFilename(pathFilename);
                if (string.IsNullOrEmpty(normalizedPathFilename)) {
                    result = localAbsRootPath;
                } else if (isinLocalAbsDosPath(normalizedPathFilename)) {
                    result = normalizedPathFilename;
                } else if (normalizedPathFilename.IndexOf(":\\") >= 0) {
                    throw new GenericException("Attempt to access an invalid path [" + normalizedPathFilename + "] that is not within the allowed path [" + localAbsRootPath + "].");
                } else {
                    result = joinPath(localAbsRootPath, normalizedPathFilename);
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Convert an absolute pathFilename to a relative pathFilename
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <returns></returns>
        public string convertLocalAbsToRelativePath(string pathFilename) {
            if (pathFilename.ToLower().IndexOf(localAbsRootPath.ToLower()).Equals(0)) { return pathFilename.Substring(localAbsRootPath.Length); }
            return pathFilename;
        }

        //
        //====================================================================================================
        /// <summary>
        /// Result dos-slashed, can be empty (), a path (mypath\), a filename (myfile.bin), or a pathFilename (mypath\myFile.bin)
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <returns></returns>
        public static string normalizeDosPathFilename(string pathFilename) {
            //
            // -- convert to dos slash and lowercase()
            // no, should not lowercase the filenames, just the path. An uploaded image to S3 must match the link saved for it so any case change must happen before call to fileController.
            string returnPathFilename = pathFilename.Replace("/", "\\");
            //string returnPath = path.Replace("/", "\\").ToLowerInvariant();
            //
            // -- remove accidental double slashes
            while (returnPathFilename.IndexOf("\\\\") >= 0) {
                returnPathFilename = returnPathFilename.Replace("\\\\", "\\");
            }
            if (string.IsNullOrEmpty(returnPathFilename) || (returnPathFilename == "\\")) {
                //
                // -- return empty if result is empty or just a slash
                return string.Empty;
            } else if (returnPathFilename.Substring(0, 1) == "\\") {
                //
                // -- if path starts with a slash, return string without slash
                return returnPathFilename.Substring(1);
            };
            return returnPathFilename;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Result dos-slashed, can be empty (), a path that starts with a foldername and ends with a slash (mypath\) (mypath\another\)
        /// </summary>
        /// <param name="dosPath"></param>
        /// <returns></returns>
        public static string normalizeDosPath(string dosPath) {
            if (!string.IsNullOrWhiteSpace(dosPath)) {
                //
                // -- normalize, allowing for a trailing filename
                dosPath = normalizeDosPathFilename(dosPath);
                if (!string.IsNullOrWhiteSpace(dosPath)) {
                    //
                    // -- verify the trailing string is a path, not a file
                    if (dosPath.Substring(dosPath.Length - 1, 1) != "\\") {
                        return dosPath + "\\";
                    } else {
                        return dosPath;
                    }
                }
            }
            return string.Empty;
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns true only if the path is an absolute path and it is within the filesystems root path. False if not absolute path or absolute path not in filesystem
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool isinLocalAbsDosPath(string path) {
            return (normalizeDosPath(path).ToLowerInvariant().IndexOf(localAbsRootPath.ToLowerInvariant()) == 0);
        }
        //
        //========================================================================
        /// <summary>
        /// save a file uploaded to the website. Path is where to store it, returnFilename is the resulting file
        /// </summary>
        /// <param name="TagName"></param>
        /// <param name="files"></param>
        /// <param name="filePath"></param>
        /// <param name="returnFilename"></param>
        /// <returns></returns>
        public bool upload(string htmlTagName, string path, ref string returnFilename) {
            bool success = false;
            returnFilename = "";
            try {
                string key = htmlTagName.ToLowerInvariant();
                if (core.docProperties.containsKey(key)) {
                    var docProperty = core.docProperties.getProperty(key);
                    if ((docProperty.propertyType == DocPropertyController.DocPropertyTypesEnum.file) && (docProperty.Name.ToLowerInvariant() == key)) {
                        string dosPathFilename = FileController.normalizeDosPath(path);
                        returnFilename = encodeDosFilename(docProperty.Value);
                        dosPathFilename += returnFilename;
                        deleteFile(dosPathFilename);
                        if (docProperty.tempfilename != "") {
                            //
                            // copy tmp private files to the appropriate folder in the destination file system
                            //
                            core.tempFiles.copyFile(docProperty.tempfilename, dosPathFilename, this);
                            //
                            if (!isLocal) {
                                copyFileLocalToRemote(dosPathFilename);
                            }
                            success = true;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
                throw;
            }
            return success;
        }
        //
        //========================================================================
        /// <summary>
        /// return the standard tablename fieldname path -- always lowercase.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string getVirtualTableFieldUnixPath(string tableName, string fieldName) {
            string result = tableName + "/" + fieldName + "/";
            return result.ToLowerInvariant().Replace(" ", "_").Replace(".", "_");
        }
        //
        //========================================================================
        //
        public static string getVirtualTableFieldIdUnixPath(string tableName, string fieldName, int recordID) {
            return getVirtualTableFieldUnixPath(tableName, fieldName) + recordID.ToString().PadLeft(12, '0') + "/";
        }
        //
        //========================================================================
        /// <summary>
        /// Create a filename for the Virtual Directory for a fieldtypeFile or Image (an uploaded file)
        /// </summary>
        public static string getVirtualRecordUnixPathFilename(string tableName, string fieldName, int recordID, string originalFilename) {
            string iOriginalFilename = originalFilename.Replace(" ", "_").Replace(".", "_");
            return getVirtualTableFieldIdUnixPath(tableName, fieldName, recordID) + originalFilename;
        }
        //
        //========================================================================
        /// <summary>
        /// Create a filename for the virtual directory for field types not associated to upload files
        /// </summary>
        public static string getVirtualRecordUnixPathFilename(string tableName, string fieldName, int recordID, CPContentClass.fileTypeIdEnum fieldType) {
            string result = "";
            string idFilename = recordID.ToString();
            if (recordID == 0) {
                idFilename = getGUID().Replace("{", "").Replace("}", "").Replace("-", "");
            } else {
                idFilename = recordID.ToString().PadLeft(12, '0');
            }
            switch (fieldType) {
                case CPContentBaseClass.fileTypeIdEnum.FileCSS:
                    result = getVirtualTableFieldUnixPath(tableName, fieldName) + idFilename + ".css";
                    break;
                case CPContentBaseClass.fileTypeIdEnum.FileXML:
                    result = getVirtualTableFieldUnixPath(tableName, fieldName) + idFilename + ".xml";
                    break;
                case CPContentBaseClass.fileTypeIdEnum.FileJavascript:
                    result = getVirtualTableFieldUnixPath(tableName, fieldName) + idFilename + ".js";
                    break;
                case CPContentBaseClass.fileTypeIdEnum.FileHTML:
                    result = getVirtualTableFieldUnixPath(tableName, fieldName) + idFilename + ".html";
                    break;
                default:
                    result = getVirtualTableFieldUnixPath(tableName, fieldName) + idFilename + ".txt";
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
                pathFilename = normalizeDosPathFilename(pathFilename);
                if (!isLocal) {
                    // todo implement remote getFileList
                    throw new NotImplementedException("getFileSize, remote mode not implemented");
                } else {
                    List<FileDetail> files = getFileList(pathFilename);
                    if (files.Count>0) {
                        fileSize = (int)(files[0].Size);
                    }
                }
            } catch (Exception ex ) {
                LogController.logError( core,ex);
            }
            return fileSize;
        }
        //
        //====================================================================================================
        /// <summary>
        /// copy a file (object) up to s3. Returns false if the local file does not exist.
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <param name="dstS3UnixPathFilename"></param>
        /// <returns></returns>
        public bool copyFileLocalToRemote(string pathFilename) {
            bool result = false;
            try {
                pathFilename = normalizeDosPathFilename(pathFilename);
                if (isLocal) {
                    // not exception, if this method is called and the object is local, this should not be an error, but it should not execute all the code
                } else {
                    if (fileExists_local(pathFilename)) {
                        string localDosPathFilename = convertToDosSlash(pathFilename);
                        //
                        // -- no, cannot change the case here
                        string remoteUnixPathFilenameLowercase = convertToUnixSlash(joinPath(remotePathPrefix, pathFilename));
                        verifyPath_remote(getPath(pathFilename));
                        //
                        // -- Setup request for putting an object in S3.
                        PutObjectRequest request = new PutObjectRequest();
                        request.BucketName = core.serverConfig.awsBucketName;
                        request.Key = remoteUnixPathFilenameLowercase;
                        request.FilePath = joinPath(localAbsRootPath, localDosPathFilename);
                        //
                        // -- Make service call and get back the response.
                        LogController.logInfo(core, "copyFileLocalToRemote(from [" + request.FilePath + "], to bucket [" + request.BucketName + "], to file [" + request.Key + "])");
                        PutObjectResponse response = s3Client.PutObject(request);
                        result = true;
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// copy a file (object) from remote to local. Returns false if the remote file does not exist. The localDosPath must exist.
        /// not exist
        /// </summary>
        public bool copyFileRemoteToLocal(string dosPathFilename) {
            bool result = false;
            try {
                if (!isLocal) {
                    dosPathFilename = normalizeDosPathFilename(dosPathFilename);
                    //
                    // -- check if local mirror has an up-to-date copy of the file
                    FileDetail localFile = getFileDetails_local(dosPathFilename);
                    if ( localFile != null ) {
                        FileDetail remoteFile = getFileDetails_remote(dosPathFilename);
                        if ( remoteFile != null ) {
                            if (( remoteFile.Size == localFile.Size ) && (remoteFile.DateLastModified <= localFile.DateLastModified)) {
                                //
                                // -- remote and local files are the same size and modification date, or the remote is older, dont copy
                                return true;
                            }
                        }
                    }
                    //
                    // note: local call is not exception, can be used regardless of isLocal
                    verifyPath_remote(getPath(dosPathFilename));
                    string remoteUnixAbsPathFilename = convertToUnixSlash(joinPath(remotePathPrefix, dosPathFilename));
                    string localDosPathFilename = convertToDosSlash(dosPathFilename);
                    //
                    // -- delete local file (for both cases, remote exists and remote does not)
                    deleteFile_local(localDosPathFilename);
                    if (fileExists_remote(dosPathFilename)) {
                        //
                        // -- remote file exists, verify local folder (or AWS returns error)
                        verifyPath_local(getPath(dosPathFilename));
                        //
                        // -- remote file exists, copy remote to local
                        GetObjectRequest request = new GetObjectRequest {
                            BucketName = core.serverConfig.awsBucketName,
                            Key = remoteUnixAbsPathFilename
                        };
                        using (GetObjectResponse response = s3Client.GetObject(request)) {
                            try {
                                response.WriteResponseStreamToFile(joinPath(localAbsRootPath, localDosPathFilename));
                            } catch (System.IO.IOException) {
                                // -- pause 1 second and retry
                                System.Threading.Thread.Sleep(1000);
                                response.WriteResponseStreamToFile(joinPath(localAbsRootPath, localDosPathFilename));
                            }
                        }
                        result = true;
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Copy a remote path to the local file system
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="remoteFileSystem"></param>
        /// <param name="localFileSystem"></param>
        /// <param name="path"></param>
        public void copyPathLocalToRemote(string path) {
            path = normalizeDosPath(path);
            foreach (var folder in getFolderList_local(path)) {
                copyPathLocalToRemote(path + folder.Name + "\\");
            }
            foreach (var file in getFileList_local(path)) {
                copyFileLocalToRemote(path + file.Name);
            }
        }
        //
        public void copyPathRemoteToLocal(string path) {
            path = normalizeDosPath(path);
            foreach (var folder in getFolderList_remote(path)) {
                copyPathRemoteToLocal(path + folder.Name + "\\");
            }
            foreach (var file in getFileList_remote(path)) {
                copyFileRemoteToLocal(path + file.Name);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// verify a path exist within the local filesystem
        /// </summary>
        private void verifyPath_local(string path) {
            try {
                path = normalizeDosPath(path);
                if (!pathExists_local(path)) {
                    createPath_local(path);
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// if remote path does not exist, it is created
        /// </summary>
        private void verifyPath_remote(string path) {
            try {
                path = normalizeDosPath(path);
                string remoteUnixPathLowercase = convertToUnixSlash("/" + joinPath(remotePathPrefix, path));
                if ( !verifiedRemotePathList.Contains(remoteUnixPathLowercase)) {
                    var urlLowercase = GenericController.splitUrl(remoteUnixPathLowercase);
                    var parentFolderInfo = new Amazon.S3.IO.S3DirectoryInfo(s3Client, core.serverConfig.awsBucketName, "");
                    string bucketKeyLowercase = "";
                    foreach (string subPathLowercase in urlLowercase.pathSegments) {
                        bucketKeyLowercase += subPathLowercase + "\\";
                        string verifiedRemotePath = "/" + convertToUnixSlash(bucketKeyLowercase);
                        var subFolderInfo = new Amazon.S3.IO.S3DirectoryInfo(s3Client, core.serverConfig.awsBucketName, bucketKeyLowercase.Substring(0, (bucketKeyLowercase.Length - 1)));
                        if (!verifiedRemotePathList.Contains(verifiedRemotePath)) {
                            if (!subFolderInfo.Exists) {
                                parentFolderInfo.CreateSubdirectory(subPathLowercase);
                            }
                            verifiedRemotePathList.Add(verifiedRemotePath);
                        }
                        parentFolderInfo = subFolderInfo;
                    }
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// verify the path exists. If it does not it is created
        /// </summary>
        public void verifyPath(string path) {
            try {
                path = normalizeDosPath(path);
                if (isLocal) {
                    // -- files stored locally
                    verifyPath_local(path);
                } else {
                    // -- files transfered through local to remote
                    verifyPath_local(path);
                    verifyPath_remote(path);
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the actual filename, or blank if the file is not found
        /// </summary>
        /// <param name="pathFilename">A case-insensative path and filename</param>
        /// <returns>The correct case for the path and filename</returns>
        public string correctFilenameCase( string pathFilename ) {
            string filename = "";
            try {
                string path = "";
                splitDosPathFilename(pathFilename, ref path, ref filename);
                filename = filename.ToLowerInvariant();
                FileDetail resultFile = getFileList(path).Find(x => x.Name.ToLowerInvariant() == filename);
                if (resultFile != null) {
                    filename = resultFile.Name;
                }
            } catch (Exception ex) {
                LogController.logError( core,ex);
            }
            return filename;
        }
        //
        //====================================================================================================
        /// <summary>
        /// convert a string a valid Dos filename. Replace all non-allowed characters with underscore.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string encodeDosFilename(string filename) {
            const string allowed = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ^&'@{}[],$-#()%.+~_";
            return encodeFilename(filename, allowed);
        }
        //
        //====================================================================================================
        /// <summary>
        /// convert a string a valid Dos filename. Replace all non-allowed characters with underscore.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string encodeDosPathFilename(string filename) {
            const string allowed = @"0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ^&'@{}[],$-#()%.+~_\";
            return encodeFilename(filename, allowed);
        }
        //
        //====================================================================================================
        /// <summary>
        /// convert a string a valid Unix filename. Replace all non-allowed characters with underscore.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string encodeUnixFilename(string filename) {
            const string allowed = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-._";
            return encodeFilename(filename, allowed);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Replace all non-allowed characters with underscore.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private static string encodeFilename(string filename, string allowedCharacters) {
            string result = "";
            int Cnt = filename.Length;
            if (Cnt > 254) Cnt = 254;
            for (int Ptr = 1; Ptr <= Cnt; Ptr++) {
                string chr = filename.Substring(Ptr - 1, 1);
                if (allowedCharacters.IndexOf(chr) + 1 >= 0) {
                    result += chr;
                } else {
                    result += "_";
                }
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// sample, use to get space available
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        double getSpaceUsedMB(string bucketName) {
            ListObjectsRequest request = new ListObjectsRequest();
            request.BucketName = bucketName;
            ListObjectsResponse response = s3Client.ListObjects(request);
            long totalSize = 0;
            foreach (S3Object o in response.S3Objects) {
                totalSize += o.Size;
            }
            return Math.Round(totalSize / 1024.0 / 1024.0, 2);
        }
        //
        //==============================================================================================================
        /// <summary>
        /// return a FileDetail object if the file is found, else return null
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FileDetail getFileDetails(string dosPathFilename) {
            try {
                if (!isLocal) {
                    return getFileDetails_remote(dosPathFilename);
                } else {
                    return getFileDetails_local(dosPathFilename);
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// internal method, return a FileDetail object if the file is found, else return null
        /// </summary>
        /// <param name="dosPathFilename"></param>
        private FileDetail getFileDetails_local(string dosPathFilename) {
            try {
                //
                // Create new FileInfo object and get the Length.
                string absDosPathFilename = convertRelativeToLocalAbsPath(dosPathFilename);
                FileInfo fileInfo = new FileInfo(absDosPathFilename);
                if ( !fileInfo.Exists ) { return null;  }
                return new FileDetail() {
                    Attributes = (int)fileInfo.Attributes,
                    DateCreated = fileInfo.CreationTime,
                    DateLastAccessed = fileInfo.LastAccessTime,
                    DateLastModified = fileInfo.LastWriteTime,
                    Name = fileInfo.Name,
                    Size = fileInfo.Length,
                    Type = ""
                };
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// internal method, return a FileDetail object if the file is found, else return null
        /// </summary>
        /// <param name="pathFilename"></param>
        private FileDetail getFileDetails_remote(string pathFilename) {
            try {
                if ( string.IsNullOrWhiteSpace(pathFilename)) { return null; }
                pathFilename = normalizeDosPathFilename(pathFilename);
                string filename = getFilename(pathFilename);
                if (string.IsNullOrWhiteSpace(filename)) { return null; }
                string unixPathFilename = convertToUnixSlash(joinPath(remotePathPrefix, pathFilename));
                string unixPath = getPath( unixPathFilename );
                ListObjectsRequest request = new ListObjectsRequest {
                    BucketName = core.serverConfig.awsBucketName,
                    Prefix = unixPath                     
                };
                // Build your call out to S3 and store the response
                ListObjectsResponse response = s3Client.ListObjects(request);
                IEnumerable<S3Object> s3fileList = response.S3Objects.Where(x => x.Key== unixPathFilename);
                foreach (var s3File in s3fileList) {
                    //
                    // -- create a fileDetail for each file found
                    string fileName = s3File.Key;
                    string keyPath = "";
                    int pos = fileName.LastIndexOf("/");
                    if (pos > -1) {
                        keyPath = fileName.Substring(0, pos + 1);
                        fileName = fileName.Substring(pos + 1);
                    }
                    if (unixPath.Equals(keyPath)) {
                        return new FileDetail() {
                            Attributes = 0,
                            Type = "",
                            DateCreated = s3File.LastModified,
                            DateLastAccessed = s3File.LastModified,
                            DateLastModified = s3File.LastModified,
                            Name = fileName,
                            Size = s3File.Size
                        };
                    }
                };
                return null;
            } catch (Amazon.S3.AmazonS3Exception ex) {
                //
                // -- support this unwillingly
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound) {
                    return null;
                }
                throw;
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the path of a pathFilename.
        /// myfilename.txt returns empty
        /// mypath\ returns mypath\
        /// mypath\myfilename returns mypath
        /// mypath\more\myfilename returns mypath\more\
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <returns></returns>
        public static string getPath(string pathFilename) {
            string result = pathFilename;
            if (string.IsNullOrEmpty(result)) {
                return "";
            } else {
                int slashpos = convertToDosSlash(pathFilename).LastIndexOf("\\");
                if (slashpos < 0) {
                    //
                    // -- pathFilename is all filename
                    return "";
                }
                if (slashpos == pathFilename.Length) {
                    //
                    // -- pathfilename is all path
                    return pathFilename;
                } else {
                    //
                    // -- divide path and filename and return just path
                    return pathFilename.Left(slashpos + 1);
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the filename of a pathFilename.
        /// myfilename.txt returns myfilename.txt
        /// mypath\ returns empty
        /// mypath\myfilename returns myfilename
        /// mypath\more\myfilename returns mypath\more\
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <returns></returns>
        public static string getFilename(string pathFilename) {
            string result = pathFilename;
            if (string.IsNullOrEmpty(result)) {
                return string.Empty;
            } else {
                int slashpos = convertToDosSlash(pathFilename).LastIndexOf("\\");
                if (slashpos < 0) {
                    //
                    // -- pathFilename is all filename
                    return result;
                }
                if (slashpos == pathFilename.Length) {
                    //
                    // -- pathfilename is all path
                    return string.Empty;
                } else {
                    //
                    // -- divide path and filename and return just path
                    return pathFilename.Substring(slashpos + 1);
                }
            }
        }
        //
        //====================================================================================================
        //
        public static string convertToDosSlash(string path) {
            return path.Replace("/", "\\");
        }
        //
        //====================================================================================================
        //
        public static string convertToUnixSlash(string path) {
            return path.Replace("\\", "/");
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
        ~FileController() {
            Dispose(false);
            
            
        }
        #endregion
    }
}
