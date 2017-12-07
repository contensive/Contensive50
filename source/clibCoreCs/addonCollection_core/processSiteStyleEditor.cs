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
	public class processSiteStyleEditorClass : Contensive.BaseClasses.AddonBaseClass
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
				if (cpCore.doc.authContext.isAuthenticated & cpCore.doc.authContext.isAuthenticatedAdmin(cpCore))
				{
					//
					// Save the site sites
					//
					cpCore.appRootFiles.saveFile(DynamicStylesFilename, cpCore.docProperties.getText("SiteStyles"));
					if (cpCore.docProperties.getBoolean(RequestNameInlineStyles))
					{
						//
						// Inline Styles
						//
						cpCore.siteProperties.setProperty("StylesheetSerialNumber", "0");
					}
					else
					{
						//
						// Linked Styles
						// Bump the Style Serial Number so next fetch is not cached
						//
						int StyleSN = cpCore.siteProperties.getinteger("StylesheetSerialNumber", 0);
						StyleSN = StyleSN + 1;
						cpCore.siteProperties.setProperty("StylesheetSerialNumber", genericController.encodeText(StyleSN));
						//
						// Save new public stylesheet
						//
						//Call appRootFiles.saveFile("templates\Public" & StyleSN & ".css", html.html_getStyleSheet2(0, 0))
						//Call appRootFiles.saveFile("templates\Admin" & StyleSN & ".css", html.getStyleSheetDefault())
					}
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
