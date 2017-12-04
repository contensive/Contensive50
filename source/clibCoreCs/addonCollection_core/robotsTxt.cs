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
	public class robotsTxtClass : Contensive.BaseClasses.AddonBaseClass
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
				//
				// -- Robots.txt
				string Filename = "config/RobotsTxtBase.txt";
				// 
				// set this way because the preferences page needs a filename in a site property (enhance later)
				cpCore.siteProperties.setProperty("RobotsTxtFilename", Filename);
				result = cpCore.cdnFiles.readFile(Filename);
				if (string.IsNullOrEmpty(result))
				{
					//
					// save default robots.txt
					//
					result = "User-agent: *" + Environment.NewLine + "Disallow: /admin/" + Environment.NewLine + "Disallow: /images/";
					cpCore.appRootFiles.saveFile(Filename, result);
				}
				result = result + cpCore.addonCache.robotsTxt;
				cpCore.webServer.setResponseContentType("text/plain");
				cpCore.doc.continueProcessing = false;
			}
			catch (Exception ex)
			{
				cp.Site.ErrorReport(ex);
			}
			return result;
		}
	}
}
