using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

//
using Contensive.Core.Controllers;
using Contensive.Core.Controllers.genericController;

using System.Xml;
using Contensive.Core;
using Contensive.Core.Models.Entity;
//
namespace Contensive.Addons
{
	//
	public class loginPageClass : Contensive.BaseClasses.AddonBaseClass
	{
		//
		//====================================================================================================
		/// <summary>
		/// addon method, deliver complete Html admin site
		/// </summary>
		/// <param name="cp"></param>
		/// <returns></returns>
		public override object Execute(Contensive.BaseClasses.CPBaseClass cp)
		{
			string returnHtml = "";
			try
			{
				CPClass processor = (CPClass)cp;
				coreClass cpCore = processor.core;
				returnHtml = Controllers.loginController.getLoginPage(cpCore, false);
			}
			catch (Exception ex)
			{
				cp.Site.ErrorReport(ex);
			}
			return returnHtml;
		}
	}
}
