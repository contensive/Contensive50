using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

//Imports Contensive.Core.cpCommonUtilsClass
//Imports Interop.adodb

//
// findReplace as integer to as integer
// just the document -- replace out 
// if 'Imports Interop.adodb, replace in ObjectStateEnum.adState...
// findreplace encode to encode
// findreplace ''DoEvents to '''DoEvents
// runProcess becomes runProcess
// Sleep becomes Threading.Thread.Sleep(
// as object to as object
//
namespace Contensive.Core.Models.Context
{
	public class uploadFileModel
	{

		private string FileNameLocal;
		private int FileSizeLocal;
		private byte[] ValueLocal;
		private string ContentTypeLocal;
		//
		//
		//
		internal string Filename
		{
			get
			{
				return FileNameLocal;
			}
			set
			{
				FileNameLocal = value;
			}
		}
		//
		//
		//
		public int FileSize
		{
			get
			{
				return FileSizeLocal;
			}
			set
			{
				FileSizeLocal = value;
			}
		}
		//
		//
		//
		public byte[] Value
		{
			get
			{
				return ValueLocal;
			}
			set
			{
				ValueLocal = value;
			}
		}
		//
		//
		//
		public string ContentType
		{
			get
			{
				return ContentTypeLocal;
			}
			set
			{
				ContentTypeLocal = value;
			}
		}
		//
		//
		//
		public bool IsFile
		{
			get
			{
				bool tempIsFile = false;
				tempIsFile = false;
				if (!string.IsNullOrEmpty(FileNameLocal))
				{
					tempIsFile = true;
				}

				return tempIsFile;
			}
		}
	}
}
