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
	public class processSiteExplorerMethodClass : Contensive.BaseClasses.AddonBaseClass
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
				cpCore.doc.addRefreshQueryString(RequestNameHardCodedPage, HardCodedPageSiteExplorer);
				string LinkObjectName = cpCore.docProperties.getText("LinkObjectName");
				if (!string.IsNullOrEmpty(LinkObjectName))
				{
					//
					// Open a page compatible with a dialog
					//
					cpCore.doc.addRefreshQueryString("LinkObjectName", LinkObjectName);
					cpCore.html.addTitle("Site Explorer");
					cpCore.doc.setMetaContent(0, 0);
					string copy = cpCore.addon.execute(addonModel.createByName(cpCore, "Site Explorer"), new CPUtilsBaseClass.addonExecuteContext() {addonType = CPUtilsBaseClass.addonContext.ContextPage});
					cpCore.html.addScriptCode_onLoad("document.body.style.overflow='scroll';", "Site Explorer");
					string htmlBodyTag = "<body class=\"ccBodyAdmin ccCon\" style=\"overflow:scroll\">";
					string htmlBody = ""
						+ genericController.htmlIndent(cpCore.html.main_GetPanelHeader("Contensive Site Explorer")) + "\r" + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\"><tr><td>"
						+ genericController.htmlIndent(copy) + "\r" + "</td></tr></table>"
						+ "";
					result = cpCore.html.getHtmlDoc(htmlBody, htmlBodyTag, false, false);
					cpCore.doc.continueProcessing = false;
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
