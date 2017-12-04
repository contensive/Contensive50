using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.BaseClasses;
using Contensive.Core.Controllers;
using System.Runtime.InteropServices;

namespace Contensive.Core
{
	//
	// comVisible to be activeScript and compatible
	//
	/// <summary>
	/// cpFileSystemClass is the api wrapper that implements cpFileSystemBaseClass using the cpCore_fileSystemClass 
	/// </summary>
	[ComVisible(true), Microsoft.VisualBasic.ComClass(CPFileSystemClass.ClassId, CPFileSystemClass.InterfaceId, CPFileSystemClass.EventsId)]
	public class CPFileSystemClass : BaseClasses.CPFileSystemBaseClass, IDisposable
	{
		//
#region COM GUIDs
		public const string ClassId = "0B73809E-F149-4262-A548-FA1E11DF63A6";
		public const string InterfaceId = "4F8288A4-2854-4B60-9281-9A776DC101D0";
		public const string EventsId = "987E6DDE-E9E6-46C5-9467-BAE79A129A15";
#endregion
		//
		private Contensive.Core.coreClass core;
		protected bool disposed = false;
		//
		private fileController fileSystem;
		//
		//==========================================================================================
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="core"></param>
		public CPFileSystemClass(Contensive.Core.coreClass core, fileController fileSystem) : base()
		{
			this.core = core;
			this.fileSystem = fileSystem;
		}
		//
		//==========================================================================================
		/// <summary>
		/// dispose
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
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
		public override void append(string filename, string fileContent)
		{
			fileSystem.appendFile(filename, fileContent);
		}
		//
		//==========================================================================================
		public override void copy(string sourceFilename, string destinationFilename)
		{
			fileSystem.copyFile(sourceFilename, destinationFilename);
		}
		//
		//==========================================================================================
		public override void createFolder(string folderPath)
		{
			fileSystem.createPath(folderPath);
		}
		//
		//==========================================================================================
		public override void deleteFile(string filename)
		{
			fileSystem.deleteFile(filename);
		}
		//
		//==========================================================================================
		public override string read(string filename)
		{
			return fileSystem.readFile(filename);
		}
		//
		//==========================================================================================
		public override byte[] readBinary(string filename)
		{
			return fileSystem.ReadBinaryFile(filename);
		}
		//
		//==========================================================================================
		public override void save(string filename, string fileContent)
		{
			fileSystem.saveFile(filename, fileContent);
		}
		//
		//==========================================================================================
		public override void saveBinary(string filename, byte[] fileContent)
		{
			fileSystem.SaveFile(filename, fileContent);
		}
		//
		//==========================================================================================
		public override bool fileExists(string pathFileName)
		{
			return fileSystem.fileExists(pathFileName);
		}
		//
		//==========================================================================================
		public override bool folderExists(string folderName)
		{
			return fileSystem.pathExists(folderName);
		}
		//
		//==========================================================================================
		public override IO.FileInfo[] fileList(string folderName, int pageSize = 0, int pageNumber = 1)
		{
			return fileSystem.getFileList(folderName);
		}
		//
		//==========================================================================================
		public override IO.DirectoryInfo[] folderList(string folderName)
		{
			return fileSystem.getFolderList(folderName);
		}
		//
		//==========================================================================================
		public override void deleteFolder(string folderPath)
		{
			fileSystem.DeleteFileFolder(folderPath);
		}
		//
		//==========================================================================================
		//
		public override bool saveUpload(string htmlformName, ref string returnFilename)
		{
			return fileSystem.upload(htmlformName, "\\upload", returnFilename);
		}
		//
		//==========================================================================================
		//
		public override bool saveUpload(string htmlformName, string folderpath, ref string returnFilename)
		{
			return fileSystem.upload(htmlformName, folderpath, returnFilename);
		}
#region  IDisposable Support 
		// Do not change or add Overridable to these methods.
		// Put cleanup code in Dispose(ByVal disposing As Boolean).
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		~CPFileSystemClass()
		{
			Dispose(false);
//INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
			//base.Finalize();
		}
#endregion
	}
}