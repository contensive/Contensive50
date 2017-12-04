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
namespace Contensive.Addons.Core
{
	public class faviconIcoClass : Contensive.BaseClasses.AddonBaseClass
	{
		//
		//====================================================================================================
		/// <summary>
		/// getFieldEditorPreference remote method
		/// </summary>
		/// <param name="cp"></param>
		/// <returns></returns>
		public override object Execute(Contensive.BaseClasses.CPBaseClass cp)
		{
			string result = "";
			try
			{
				CPClass processor = (CPClass)cp;
				coreClass cpCore = processor.core;
				string Filename = cpCore.siteProperties.getText("FaviconFilename", "");
				if (string.IsNullOrEmpty(Filename))
				{
					//
					// no favicon, 404 the call
					//
					cpCore.webServer.setResponseStatus("404 Not Found");
					cpCore.webServer.setResponseContentType("image/gif");
					cpCore.doc.continueProcessing = false;
					return string.Empty;
				}
				else
				{
					cpCore.doc.continueProcessing = false;
					return cpCore.webServer.redirect(genericController.getCdnFileLink(cpCore, Filename), "favicon request", false, false);
				}
			}
			catch (Exception ex)
			{
				cp.Site.ErrorReport(ex);
			}
			return result;
		}
	}
}
