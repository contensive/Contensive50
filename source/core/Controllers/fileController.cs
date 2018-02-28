
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
using System.Linq;
using static Contensive.BaseClasses.CPFileSystemBaseClass;
//
namespace Contensive.Core.Controllers {
    //
    //==============================================================================================================
    /// <summary>
    /// Basic file access class for all scaling targets (cden, private, approot, etc).
    /// set isLocal true and all files are handled on the local server
    /// set isLocal false:
    /// - the local filesystem becomes a mirror for the remote system
    /// - after a transaction is complete (read,write,copy) the local file can be deleted
    /// - on remote system, ALL filenames will be converted lowercase
    /// 
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
        /// <summary>
        /// list of paths verified during the scope of this execution. If a path is deleted, it must be removed from this list
        /// </summary>
        private List<string> verifiedRemotePathList = new List<string>();
        //
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
                pathFilename = normalizeDosPathFilename(pathFilename);
                returnPath = Path.Combine(returnPath, pathFilename);
                //if (pathFilename != "\\") {
                //    returnPath = Path.Combine(returnPath, pathFilename);
                //}
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
                core.handleException(ex);
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
                        if (!copyRemoteToLocal(pathFilename)) {
                            //
                            // -- if remote file does not exist, delete local mirror
                            deleteFile_local(pathFilename);
                        }
                    }
                    if (fileExists_local(pathFilename)) {
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
                        if (!copyRemoteToLocal(pathFilename)) {
                            //
                            // -- if remote file does not exist, delete local mirror
                            deleteFile_local(pathFilename);
                        }
                    }
                    if (fileExists(pathFilename)) {
                        using (FileStream sr = File.OpenRead(convertToLocalAbsPath(pathFilename))) {
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
        /// <param name="pathFilename"></param>
        /// <param name="fileContent"></param>
        public void appendFile(string pathFilename, string fileContent) {
            try {
                if (string.IsNullOrWhiteSpace(pathFilename)) {
                    throw new ArgumentException("appendFile called with blank pathname.");
                } else if (!string.IsNullOrEmpty(fileContent)) {
                    //
                    // -- verify local path
                    string absFilename = convertToLocalAbsPath(pathFilename);
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
                        if (!copyRemoteToLocal(pathFilename)) {
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
                        copyLocalToRemote(pathFilename);
                    }
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
        /// <param name="path"></param>
        private void createPath_local(string path) {
            createPathAbs_local(convertToLocalAbsPath(path));
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
                    createPath_local(path);
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
                        deleteFile_local(pathFilename);
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
                pathFilename = normalizeDosPathFilename(pathFilename);
                if (!string.IsNullOrWhiteSpace(pathFilename)) {
                    string remoteUnixPathFilename = genericController.convertToUnixSlash(joinPath(remotePathPrefix, pathFilename));
                    if ( fileExists_remote(pathFilename)) {
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
                pathFilename = normalizeDosPathFilename(pathFilename);
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
        /// <param name="path"></param>
        public void deleteFolder(string path) {
            try {
                path = normalizeDosPath(path);
                if (!string.IsNullOrEmpty(path)) {
                    if (!isLocal) {
                        // todo - rewrite using lowlevel + transfer, not file io
                        // https://aws.amazon.com/blogs/developer/the-three-different-apis-for-amazon-s3/
                        string unixPathName = joinPath(remotePathPrefix , path).Trim();
                        if ((unixPathName.Length > 1) & (unixPathName.Substring(0, 1) == "\\")) {
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
                if (dstFileSystem == null) {
                    dstFileSystem = this;
                }
                if (string.IsNullOrEmpty(srcPathFilename)) {
                    throw new ArgumentException("Invalid source file.");
                } else if (string.IsNullOrEmpty(dstPathFilename)) {
                    throw new ArgumentException("Invalid destination file.");
                } else {
                    srcPathFilename = normalizeDosPathFilename(srcPathFilename);
                    dstPathFilename = normalizeDosPathFilename(dstPathFilename);
                    if (!isLocal) {
                        //
                        // src is remote file - copy file to local mirror
                        if (fileExists_remote(srcPathFilename)) {
                            verifyPath_remote(getPath(srcPathFilename));
                            copyRemoteToLocal(srcPathFilename);
                            //
                            // -- copy src to dst on local mirror
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
                                dstFileSystem.copyLocalToRemote(dstPathFilename);
                            }
                        }
                        //string srcRemoteUnixPathFilename = genericController.convertToUnixSlash("/" + joinPath(remotePathPrefix, srcPathFilename));
                        //var s3FileInfo = new Amazon.S3.IO.S3FileInfo(s3Client, core.serverConfig.awsBucketName, convertToDosSlash(srcRemoteUnixPathFilename.Substring(1)));
                        //if ( s3FileInfo.Exists ) {
                        //    string dstRemoteUnixPathFilename = genericController.convertToUnixSlash("/" + joinPath(remotePathPrefix, dstPathFilename));
                        //    string dstRemoteUnixPath = "";
                        //    string dstRemoteUnixFilename = "";
                        //    splitUnixPathFilename(dstPathFilename, ref dstRemoteUnixPath, ref dstRemoteUnixFilename);
                        //    verifyPath_remote(dstRemoteUnixPath);
                        //    Amazon.S3.IO.S3DirectoryInfo remoteDirectoryInfo = new Amazon.S3.IO.S3DirectoryInfo(s3Client, core.serverConfig.awsBucketName, dstRemoteUnixPathFilename);
                        //    s3FileInfo.CopyTo(remoteDirectoryInfo);
                        //}
                    } else {
                        //
                        // -- src is local file, copy to dst local
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
                                dstFileSystem.copyLocalToRemote(dstPathFilename);
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
        /// list of files, each row is delimited by a comma
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<BaseClasses.CPFileSystemBaseClass.FileDetail> getFileList(string path) {
            var returnFileList = new List<FileDetail>();
            try {
                path = normalizeDosPath(path);
                string unixPath = convertToUnixSlash(joinPath(remotePathPrefix, path));
                if (!isLocal) {
                    ListObjectsRequest request = new ListObjectsRequest {
                        BucketName = core.serverConfig.awsBucketName,
                        Prefix = unixPath
                    };
                    // Build your call out to S3 and store the response
                    ListObjectsResponse response = s3Client.ListObjects(request);

                    IEnumerable<S3Object> fileList = response.S3Objects.Where(x => !x.Key.EndsWith(@"/") );
                    foreach (var file in fileList) {
                        //
                        // -- create a fileDetail for each file found
                        string fileName = file.Key;
                        string keyPath = "";
                        int pos = fileName.LastIndexOf("/");
                        if (pos > -1) {
                            keyPath = fileName.Substring(0, pos+1);
                            fileName = fileName.Substring(pos + 1);
                        }
                        if ( unixPath.Equals(keyPath)) {
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


                } else {
                    if (pathExists_local(path)) {
                        string localPath = convertToLocalAbsPath(path);
                        DirectoryInfo di = new DirectoryInfo(localPath);
                        foreach(var file in di.GetFiles()) {
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
                }
            } catch (Exception ex) {
                core.handleException(ex);
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
                    // todo implement remote
                    throw new NotImplementedException("remote mode not implemented");
                } else {
                    List<FolderDetail> di = getFolderList(path);
                    foreach (FolderDetail d in di) {
                        returnList += "," + d.Name;
                    }
                    if (!string.IsNullOrEmpty(returnList)) {
                        returnList = returnList.Substring(1);
                    }
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
        public List<BaseClasses.CPFileSystemBaseClass.FolderDetail> getFolderList(string path) {
            var returnFolders = new List<FolderDetail>();
            try {
                path = normalizeDosPath(path);
                if (!isLocal) {
                    //var DirectoryList = new List<DirectoryInfo>();
                    ListObjectsRequest request = new ListObjectsRequest {
                        BucketName = core.serverConfig.awsBucketName,
                        Prefix = convertToUnixSlash(path)
                    };
                    // Build your call out to S3 and store the response
                    ListObjectsResponse response = s3Client.ListObjects(request);
                    IEnumerable<S3Object> folderList = response.S3Objects.Where(x =>x.Key.EndsWith(@"/") && x.Size == 0);
                    foreach (var folder in folderList) {
                        returnFolders.Add(new FolderDetail() {
                            Attributes = 0,
                            Type = "",
                            DateCreated = folder.LastModified,
                            DateLastAccessed = folder.LastModified,
                            DateLastModified = folder.LastModified,
                            Name = folder.Key
                        });
                    };
                } else {
                    if (pathExists_local(path)) {
                        string localPath = convertToLocalAbsPath(path);
                        DirectoryInfo di = new DirectoryInfo(localPath);
                        foreach( var folder in di.GetDirectories()) {
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
                pathFilename = normalizeDosPathFilename(pathFilename);
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
        /// <param name="dosPathFilename"></param>
        private bool fileExists_local(string dosPathFilename) {
            bool returnOK = false;
            try {
                string absDosPathFilename = convertToLocalAbsPath(dosPathFilename);
                returnOK = File.Exists(absDosPathFilename);
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
        /// <param name="pathFilename"></param>
        private bool fileExists_remote(string pathFilename) {
            try {
                string unixAbsPathFilename = convertToUnixSlash(joinPath(remotePathPrefix, pathFilename));
                string path = "";
                string filename = "";
                // no, cannot change case here. paths should be lcase before the call, file case should be preserved.
                splitUnixPathFilename(unixAbsPathFilename, ref path, ref filename);
                //splitUnixPathFilename(unixAbsPathFilename.ToLower(), ref pathLowercase, ref filenameLowercase);
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
                core.handleException(ex);
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
                core.handleException(ex);
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
        private bool pathExists_local(string path) {
            bool returnOk = false; 
            try {
                string absPath = convertToLocalAbsPath(path);
                returnOk = Directory.Exists(absPath);
            } catch (Exception ex) {
                core.handleException(ex);
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
        private bool pathExists_remote(string path) {
            bool returnOK = false;
            try {
                //
                // -- remote
                path = normalizeDosPath(path);
                string remoteUnixPathFilename = genericController.convertToUnixSlash("/" + joinPath(remotePathPrefix, path));
                var url = genericController.splitUrl(remoteUnixPathFilename);
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
        /// <param name="srcPathFilename"></param>
        /// <param name="dstFilename"></param>
        public void renameFile(string srcPathFilename, string dstFilename) {
            try {
                srcPathFilename = normalizeDosPathFilename(srcPathFilename);
                if (string.IsNullOrEmpty(srcPathFilename)) {
                    throw new ApplicationException("Invalid source file");
                } else {
                    if (!isLocal) {
                        // todo remote file case
                        throw new NotImplementedException("remote rename file not implemented yet");
                    } else {
                        string srcFullPathFilename = joinPath(localAbsRootPath, srcPathFilename);
                        int Pos = srcPathFilename.LastIndexOf("\\") + 1;
                        string sourceFullPath = "";
                        if (Pos >= 0) {
                            sourceFullPath = srcPathFilename.Left(Pos);
                        }
                        if (string.IsNullOrEmpty(dstFilename)) {
                            throw new ApplicationException("Invalid destination file []");
                        } else if (dstFilename.IndexOf("\\") != -1) {
                            throw new ApplicationException("Invalid '\\' character in destination filename [" + dstFilename + "]");
                        } else if (dstFilename.IndexOf("/") != -1) {
                            throw new ApplicationException("Invalid '/' character in destination filename [" + dstFilename + "]");
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
        /// <param name="srcAbsDosPath"></param>
        /// <param name="absDstFolder"></param>
        private void copyFolder_srcLocal(string srcAbsDosPath, string dstDosPath, fileController dstFileSystem = null) {
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
                            dstFileSystem.copyLocalToRemote(joinPath(dstDosPath, srcFile.Name));
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
                core.handleException(ex);
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
        private void copyFolder_srcRemote(string srcAbsDosPath, string dstDosPath, fileController dstFileSystem = null) {
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
                            dstFileSystem.copyLocalToRemote(joinPath(dstDosPath, srcFile.Name));
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
                core.handleException(ex);
                throw;
            }
        }
        //
        //==============================================================================================================
        /// <summary>
        /// convert fileInfo array to parsable string [filename-attributes-creationTime-lastAccessTime-lastWriteTime-length-entension]
        /// </summary>
        /// <param name="FileInfo"></param>
        /// <returns></returns>
        public string convertFileInfoArrayToParseString( List<FileDetail> FileInfo) {
            var result = new System.Text.StringBuilder();
            if (FileInfo.Count > 0) {
                foreach (FileDetail fi in FileInfo) {
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
        public string convertDirectoryInfoArrayToParseString(List<FolderDetail> DirectoryInfo) {
            var result = new System.Text.StringBuilder();
            if (DirectoryInfo.Count > 0) {
                foreach (FolderDetail di in DirectoryInfo) {
                    result.Append( "\r\n" + di.Name + "\t" + (int)di.Attributes + "\t" + di.DateCreated + "\t" + di.DateLastAccessed + "\t" + di.DateLastModified + "\t0\t");
                }
            }
            return result.ToString();
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
                if ((!string.IsNullOrEmpty(pathFilename)) & (!string.IsNullOrEmpty(Link))) {
                    string URLLink = genericController.vbReplace(Link, " ", "%20");
                    httpRequestController HTTP = new httpRequestController();
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
                pathFilename = normalizeDosPathFilename(pathFilename);
                bool processLocalFile = true;
                if ( !isLocal ) {
                    if (!copyRemoteToLocal(pathFilename)) {
                        processLocalFile = false;
                        deleteFile_local(pathFilename);
                    }
                }
                if (processLocalFile) {
                    string path = "";
                    string filename = "";
                    splitDosPathFilename(pathFilename, ref path, ref filename);
                    string absPathFilename = convertToLocalAbsPath(pathFilename);
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
                                        copyLocalToRemote(joinPath(path, ze.Name));
                                    }
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
                archivePathFilename = normalizeDosPathFilename(archivePathFilename);
                addPathFilename = normalizeDosPathFilename(addPathFilename);
                //
                string archivepath = "";
                string archiveFilename = "";
                splitDosPathFilename(archivePathFilename, ref archivepath, ref archiveFilename);
                FastZip fastZip = new FastZip();
                string fileFilter = null;
                bool recurse = true;
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
                string normalizedPathFilename = normalizeDosPathFilename(pathFilename);
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
        /// Result dos-slashed, can be empty (), a path (mypath\), a filename (myfile.bin), or a pathFilename (mypath\myFile.bin)
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <returns></returns>
        public static string normalizeDosPathFilename(string pathFilename) {
            //
            // -- convert to dos slash and lowercase()
            // no, should not lowercase the filenames, just the path. An uploaded image to S3 must match the link saved for it so any case change must happen before call to fileController.
            string returnPathFilename = pathFilename.Replace("/", "\\");
            //string returnPath = path.Replace("/", "\\").ToLower();
            //
            // -- remove accidental double slashes
            while (returnPathFilename.IndexOf("\\\\") >= 0) {
                returnPathFilename = returnPathFilename.Replace("\\\\", "\\");
            }
            if (string.IsNullOrEmpty(returnPathFilename) | (returnPathFilename == "\\")) {
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
            return (normalizeDosPath(path).ToLower().IndexOf(localAbsRootPath.ToLower()) == 0);
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
        /// <summary>
        /// return the standard tablename fieldname path -- always lowercase.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string getVirtualTableFieldUnixPath(string tableName, string fieldName) {
            string result = tableName + "/" + fieldName + "/";
            return result.ToLower().Replace(" ", "_").Replace(".", "_");
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
        public static string getVirtualRecordUnixPathFilename(string tableName, string fieldName, int recordID, int fieldType) {
            string result = "";
            string idFilename = recordID.ToString();
            if (recordID == 0) {
                idFilename = getGUID().Replace("{", "").Replace("}", "").Replace("-", "");
            } else {
                idFilename = recordID.ToString().PadLeft(12, '0');
            }
            switch (fieldType) {
                case FieldTypeIdFileCSS:
                    result = getVirtualTableFieldUnixPath(tableName, fieldName) + idFilename + ".css";
                    break;
                case FieldTypeIdFileXML:
                    result = getVirtualTableFieldUnixPath(tableName, fieldName) + idFilename + ".xml";
                    break;
                case FieldTypeIdFileJavascript:
                    result = getVirtualTableFieldUnixPath(tableName, fieldName) + idFilename + ".js";
                    break;
                case FieldTypeIdFileHTML:
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
                    throw new NotImplementedException("remote getFileSize not implemented");
                } else {
                    List<FileDetail> files = getFileList(pathFilename);
                    if (files.Count>0) {
                        fileSize = (int)(files[0].Size);
                    }
                }
            } catch (Exception ex ) {
                core.handleException(ex);
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
        public bool copyLocalToRemote(string pathFilename) {
            bool result = false;
            try {
                pathFilename = normalizeDosPathFilename(pathFilename);
                if (isLocal) {
                    // not exception, can be used regardless of isLocal //throw new ApplicationException("copyLocalToRemote is not valid in a local File system [" + localAbsRootPath + "]");
                } else {
                    if (fileExists_local(pathFilename)) {
                        string localDosPathFilename = genericController.convertToDosSlash(pathFilename);
                        // no, cannot change the case here
                        string remoteUnixPathFilenameLowercase = genericController.convertToUnixSlash(joinPath(remotePathPrefix, pathFilename));
                        //string remoteUnixPathFilenameLowercase = genericController.convertToUnixSlash(joinPath(remotePathPrefix, pathFilename)).ToLower();
                        verifyPath_remote(getPath(pathFilename));
                        //
                        // -- Setup request for putting an object in S3.
                        PutObjectRequest request = new PutObjectRequest();
                        request.BucketName = core.serverConfig.awsBucketName;
                        request.Key = remoteUnixPathFilenameLowercase;
                        request.FilePath = joinPath(localAbsRootPath, localDosPathFilename);
                        //
                        // -- Make service call and get back the response.
                        PutObjectResponse response = s3Client.PutObject(request);
                        result = true;
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
        /// copy a file (object) from remote to local. Returns false if the remote file does not exist. The localDosPath must exist.
        /// not exist
        /// </summary>
        public bool copyRemoteToLocal(string pathFilename) {
            bool result = false;
            try {
                pathFilename = normalizeDosPathFilename(pathFilename);
                if (isLocal) {
                    // not exception, can be used regardless of isLocal //throw new ApplicationException("copyRemoteToLocal is not valid in a local File system [" + localAbsRootPath + "]");
                } else {
                    verifyPath_remote(getPath(pathFilename));
                    string remoteUnixAbsPathFilename = genericController.convertToUnixSlash(joinPath(remotePathPrefix, pathFilename));
                    string localDosPathFilename = genericController.convertToDosSlash(pathFilename);
                    //
                    // -- delete local file (for both cases, remote exists and remote does not)
                    deleteFile_local(localDosPathFilename);
                    if (fileExists_remote(pathFilename)) {
                        //
                        // -- remote file exists, delete local version and copy remote to local
                        GetObjectRequest request = new GetObjectRequest {
                            BucketName = core.serverConfig.awsBucketName,
                            Key = remoteUnixAbsPathFilename
                        };
                        using (GetObjectResponse response = s3Client.GetObject(request)) {
                            response.WriteResponseStreamToFile(joinPath(localAbsRootPath, localDosPathFilename));
                        }
                        result = true;
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
                path = normalizeDosPath(path);
                if (!pathExists_local(path)) {
                    createPath_local(path);
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
                path = normalizeDosPath(path);
                string remoteUnixPathLowercase = genericController.convertToUnixSlash("/" + joinPath(remotePathPrefix, path));
                if ( !verifiedRemotePathList.Contains(remoteUnixPathLowercase)) {
                    var urlLowercase = genericController.splitUrl(remoteUnixPathLowercase);
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
                core.handleException(ex);
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
                core.handleException(ex);
            }
        }
        //
        // ========================================================================================================================
        /// <summary>
        /// return the actual filename, or blank if the file is not found
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <returns></returns>
        public string correctFilenameCase( string pathFilename ) {
            string path = "";
            string filename = "";
            splitDosPathFilename(pathFilename, ref path, ref filename);
            filename = filename.ToLower();
            FileDetail resultFile = getFileList(path).Find( x => x.Name.ToLower() == filename );
            if ( resultFile != null) {
                filename = resultFile.Name;
            }
            return filename;
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
