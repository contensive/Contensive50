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
		public abstract System.IO.FileInfo[] fileList(string folderName, int pageSize = 0, int pageNumber = 1);
		public abstract System.IO.DirectoryInfo[] folderList(string folderName);
		public abstract bool fileExists(string pathFileName);
		public abstract bool folderExists(string folderName);
		public abstract bool saveUpload(string htmlformName, ref string returnFilename);
		public abstract bool saveUpload(string htmlformName, string folderpath, ref string returnFilename);
	}
}

