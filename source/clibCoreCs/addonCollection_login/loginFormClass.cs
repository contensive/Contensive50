using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

//
using Contensive.Core;
//
namespace Contensive.Addons
{
	public class loginFormClass : Contensive.BaseClasses.AddonBaseClass
	{
		//
		//====================================================================================================
		/// <summary>
		/// Login Form
		/// </summary>
		/// <param name="cp"></param>
		/// <returns></returns>
		public override object Execute(Contensive.BaseClasses.CPBaseClass cp)
		{
			string returnHtml = "";
			try
			{
				CPClass processor = (CPClass)cp;
				bool forceDefaultLogin = cp.Doc.GetBoolean("Force Default Login");
				returnHtml = Controllers.loginController.getLoginForm(processor.core, forceDefaultLogin);
			}
			catch (Exception ex)
			{
				cp.Site.ErrorReport(ex);
			}
			return returnHtml;
		}
	}
}
