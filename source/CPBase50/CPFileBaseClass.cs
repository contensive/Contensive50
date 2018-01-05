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
	public abstract class CPFileBaseClass
	{
		//
		/// <summary>
		/// Append content to a text file in the content files. If the file does not exist it will be created.
		/// </summary>
		/// <param name="Filename">The filename of the file to be appended. May include subfolders in the content file area. It should not include a leading slash. Folder slashes should be \.</param>
		/// <param name="FileContent">Test appended to the file</param>
		/// <remarks></remarks>
		[Obsolete("Deprecated, please use cp.File.cdn, cp.File.private, cp.File.appRoot, or cp.File.server instead.", false)]
		public abstract void AppendVirtual(string Filename, string FileContent);
		/// <summary>
		/// Copies a file in the content file area to another. If the destination does not exist it is created. Filenames may include subfolders but should not include a leading slash.
		/// </summary>
		/// <param name="SourceFilename"></param>
		/// <param name="DestinationFilename"></param>
		/// <remarks></remarks>
		[Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", false)]
		public abstract void CopyVirtual(string SourceFilename, string DestinationFilename);
		/// <summary>
		/// Create a folder given a physical folder path.
		/// </summary>
		/// <param name="FolderPath"></param>
		/// <remarks></remarks>
		[Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", false)]
		public abstract void CreateFolder(string FolderPath);
		/// <summary>
		/// Delete a file within the file space.
		/// </summary>
		/// <param name="Filename"></param>
		/// <remarks></remarks>
		[Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", false)]
		public abstract void Delete(string Filename);
		/// <summary>
		/// Delete a folder within the file space
		/// </summary>
		/// <param name="folderPath"></param>
		[Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", false)]
		public abstract void DeleteFolder(string folderPath);
		/// <summary>
		/// Delete a file in the content file area. The filename may contain subfolders and should not begin with a leading slash.
		/// </summary>
		/// <param name="Filename"></param>
		/// <remarks></remarks>
		[Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", false)]
		public abstract void DeleteVirtual(string Filename);
		/// <summary>
		/// Read a text file within the file space.
		/// </summary>
		/// <param name="Filename"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		[Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", false)]
		public abstract string Read(string Filename);
		/// <summary>
		/// Read a text file in the content file area. The filename may contain subfolders and should not begin with a leading slash.
		/// </summary>
		/// <param name="Filename"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		[Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", false)]
		public abstract string ReadVirtual(string Filename);
		/// <summary>
		/// Save or create a text file within the file space.
		/// </summary>
		/// <param name="Filename"></param>
		/// <param name="FileContent"></param>
		/// <remarks></remarks>
		[Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", false)]
		public abstract void Save(string Filename, string FileContent);
		/// <summary>
		/// Save a text file in the content file area. The filename may contain subfolders and should not begin with a leading slash.
		/// </summary>
		/// <param name="Filename"></param>
		/// <param name="FileContent"></param>
		/// <remarks></remarks>
		[Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", false)]
		public abstract void SaveVirtual(string Filename, string FileContent);
		/// <summary>
		/// Get a crlf delimited list of files in a given path. Each row is a tab delimited list of attributes for each file. The attributes are:
		/// FileName
		/// Attributes
		/// DateCreated
		/// DateLastAccessed
		/// DateLastModified
		/// Size
		/// Type
		/// </summary>
		/// <param name="folderName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		[Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", false)]
		public abstract string fileList(string folderName, int pageSize = 0, int pageNumber = 1);
		/// <summary>
		/// Get a crlf delimited list of folders in a given path. Each row is a tab delimited list of attributes for each folder. The attributes are:
		/// Name
		/// Attributes
		/// DateCreated
		/// DateLastAccessed
		/// DateLastModified
		/// Type
		/// </summary>
		/// <param name="folderName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		[Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", false)]
		public abstract string folderList(string folderName);
		/// <summary>
		/// Returns true if a file exists
		/// </summary>
		/// <param name="pathFileName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		[Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", false)]
		public abstract bool fileExists(string pathFileName);
		/// <summary>
		/// Returns true if a folder exists
		/// </summary>
		/// <param name="folderName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		[Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", false)]
		public abstract bool folderExists(string folderName);
		/// <summary>
		/// Returns a URL to a file in the File.cdn store
		/// </summary>
		/// <param name="virtualFilename"></param>
		/// <returns></returns>
		[Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", false)]
		public abstract string getVirtualFileLink(string virtualFilename);
	}

}

