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
        public abstract void Append(string Filename, string FileContent);
		public abstract void Copy(string SourceFilename, string DestinationFilename);
		public abstract void CreateFolder(string FolderPath);
		public abstract void DeleteFile(string Filename);
		public abstract void DeleteFolder(string folderPath);
		public abstract string Read(string Filename);
		public abstract byte[] ReadBinary(string Filename);
		public abstract void Save(string Filename, string FileContent);
		public abstract void SaveBinary(string Filename, byte[] FileContent);
		public abstract List<FileDetail> FileList(string folderName, int pageSize = 0, int pageNumber = 1);
		public abstract List<FolderDetail> FolderList(string folderName);
		public abstract bool FileExists(string pathFileName);
		public abstract bool FolderExists(string folderName);
		public abstract bool SaveUpload(string htmlformName, ref string returnFilename);
		public abstract bool SaveUpload(string htmlformName, string folderpath, ref string returnFilename);
	}
}

