//========================================================================



//========================================================================

//
// documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
//


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Contensive.BaseClasses
{
	public abstract class CPFileSystemBaseClass
	{
        /// <summary>
        /// argument details for file and folder methods
        /// </summary>
        public class FileDetail {
            public string Name { get; set; }
            public int Attributes { get; set; }
            public DateTime DateCreated { get; set; }
            public DateTime DateLastAccessed { get; set; }
            public DateTime DateLastModified { get; set; }
            public long Size { get; set; }
            public string Type { get; set; }
            public string Extension {
                get {
                    if (string.IsNullOrEmpty(_extension)) {
                        if (!string.IsNullOrEmpty(Name)) {
                            int pos = Name.LastIndexOf(".");
                            if ((pos >= 0) && (pos < Name.Length)) {
                                _extension = Name.Substring(pos);
                            }
                        }
                    }
                    return _extension;
                }
            } string _extension = "";
        }
        /// <summary>
        /// argument details for file and folder methods
        /// </summary>
        public class FolderDetail {
            public string Name { get; set; }
            public int Attributes { get; set; }
            public DateTime DateCreated { get; set; }
            public DateTime DateLastAccessed { get; set; }
            public DateTime DateLastModified { get; set; }
            public string Type { get; set; }
        }
        //
        public abstract void append(string Filename, string FileContent);
		public abstract void copy(string SourceFilename, string DestinationFilename);
		public abstract void createFolder(string FolderPath);
		public abstract void deleteFile(string Filename);
		public abstract void deleteFolder(string folderPath);
		public abstract string read(string Filename);
		public abstract byte[] readBinary(string Filename);
		public abstract void save(string Filename, string FileContent);
		public abstract void saveBinary(string Filename, byte[] FileContent);
		public abstract List<FileDetail> fileList(string folderName, int pageSize = 0, int pageNumber = 1);
		public abstract List<FolderDetail> folderList(string folderName);
		public abstract bool fileExists(string pathFileName);
		public abstract bool folderExists(string folderName);
		public abstract bool saveUpload(string htmlformName, ref string returnFilename);
		public abstract bool saveUpload(string htmlformName, string folderpath, ref string returnFilename);
	}
}

