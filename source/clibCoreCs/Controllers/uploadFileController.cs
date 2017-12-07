using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.Core.Models.Context;
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;

//
namespace Contensive.Core.Controllers
{
	public class uploadFileController
	{
		internal byte[] binaryHeader {get; set;}
		private object BinaryHeaderLocal;
		private Dictionary<string, uploadFileModel> ItemStorageCollection;
		private string[] ItemNames;
		private int ItemCount;
		//
		//
		//
//INSTANT C# NOTE: C# does not support parameterized properties - the following property has been rewritten as a function:
//ORIGINAL LINE: Public ReadOnly Property Form(ByVal Key As String) As uploadFileModel
		public uploadFileModel get_Form(string Key)
		{
			uploadFileModel returnForm = null;
			if (ItemStorageCollection != null)
			{
				if (ItemStorageCollection.Count > 0)
				{
					if (FieldExists(Key))
					{
						returnForm = ItemStorageCollection(Key);
					}
					else
					{
						returnForm = ItemStorageCollection("EMPTY");
					}
				}
			}
			return returnForm;
		}
		//
		// Get the Count of Form collection
		//
		public int Count
		{
			get
			{
				int tempCount = 0;
				if (ItemStorageCollection == null)
				{
					tempCount = 0;
				}
				else
				{
					tempCount = ItemCount;
					//Count = ItemStorageCollection.Count
				}

				return tempCount;
			}
		}
		//
		// Test if a key exists in the form collection
		//
		public bool FieldExists(string Key)
		{
			bool tempFieldExists = false;
			bool result = false;
			try
			{
				int ItemPointer = 0;
				string UcaseKey = null;
				//
				tempFieldExists = false;
				if ((ItemCount > 0) && !IsNull(Key))
				{
					UcaseKey = genericController.vbUCase(Key);
					for (ItemPointer = 0; ItemPointer < ItemCount; ItemPointer++)
					{
						if (ItemNames[ItemPointer] == UcaseKey)
						{
							tempFieldExists = true;
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw;
			}
			return tempFieldExists;
		}


		public string Key(int Index)
		{
			string tempKey = null;
			string result = "";
			try
			{
				if (Index < ItemCount)
				{
					tempKey = ItemNames[Index];
				}
			}
			catch (Exception ex)
			{
				throw;
			}
			return result;
		}
	}
}
