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
	public class processLoginMethodClass : Contensive.BaseClasses.AddonBaseClass
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
				// -- login
				cpCore.doc.continueProcessing = false;
				Dictionary<string, string> addonArguments = new Dictionary<string, string>();
				addonArguments.Add("Force Default Login", "false");
				return cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginPage), new CPUtilsBaseClass.addonExecuteContext()
				{
					addonType = CPUtilsBaseClass.addonContext.ContextPage,
					instanceArguments = addonArguments,
					forceHtmlDocument = true
				});
			}
			catch (Exception ex)
			{
				cp.Site.ErrorReport(ex);
			}
			return result;
		}
	}
}
