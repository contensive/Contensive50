using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

namespace Contensive.Core.Controllers
{
	public class stringBuilderLegacyController
	{
		//
		private int iSize;
		private const int iChunk = 100;
		private int iCount;
		private string[] Holder;
		//
		//==========================================================================================
		/// <summary>
		/// add a string to the stringbuilder
		/// </summary>
		/// <param name="NewString"></param>
		public void Add(string NewString)
		{
			try
			{
				if (iCount >= iSize)
				{
					iSize = iSize + iChunk;
					Array.Resize(ref Holder, iSize + 1);
				}
				Holder[iCount] = NewString;
				iCount = iCount + 1;
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Exception in coreFastString.Add()", ex);
			}
		}
		//
		//==========================================================================================
		/// <summary>
		/// read the string out of the string builder
		/// </summary>
		/// <returns></returns>
		public string Text
		{
			get
			{
				return string.Join("", Holder) + "";
			}
		}
	}
}