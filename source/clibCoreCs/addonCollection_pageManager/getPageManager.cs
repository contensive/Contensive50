using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.BaseClasses;
using Contensive.Core;
using Models.Entity;
using Contensive.Core.Controllers;

//
namespace Contensive.Addons.PageManager
{
	//
	public class getPageManagerClass : Contensive.BaseClasses.AddonBaseClass
	{
		//
		//====================================================================================================
		/// <summary>
		/// pageManager addon interface
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
				returnHtml = "<div class=\"ccBodyWeb\">" + pageContentController.getHtmlBody(cpCore) + "</div>";
				//returnHtml = cpCore.html.getHtmlDoc(htmlBody, TemplateDefaultBodyTag, True, True, False)
			}
			catch (Exception ex)
			{
				cp.Site.ErrorReport(ex);
			}
			return returnHtml;
		}
	}
}
