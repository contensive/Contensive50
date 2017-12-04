using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

//

using System.Xml;
using Contensive.Core;
using Contensive.BaseClasses;
//
namespace Contensive.Addons
{
	//
	public class addon_toolConfigureEditClass : Contensive.BaseClasses.AddonBaseClass
	{
		//
		//====================================================================================================
		/// <summary>
		/// addon method, deliver complete Html admin site
		/// </summary>
		/// <param name="cp"></param>
		/// <returns></returns>
		public override object Execute(CPBaseClass cp)
		{
			string result = "";
			try
			{
				//result = GetForm_ConfigureEdit(cp)
			}
			catch (Exception ex)
			{
				cp.Site.ErrorReport(ex);
			}
			return result;
		}
	}
}

