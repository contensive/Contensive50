using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.BaseClasses;
using Contensive.Core;
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
//
namespace Contensive.Addons.AdminSite
{
	//
	public class getAjaxDefaultAddonOptionStringClass : Contensive.BaseClasses.AddonBaseClass
	{
		//
		//====================================================================================================
		/// <summary>
		/// getFieldEditorPreference remote method
		/// </summary>
		/// <param name="cp"></param>
		/// <returns></returns>
		public override object Execute(CPBaseClass cp)
		{
			string returnHtml = "";
			try
			{
				CPClass processor = (CPClass)cp;
				coreClass cpCore = processor.core;
				//
				// return the addons defult AddonOption_String
				// used in wysiwyg editor - addons in select list have no defaultOption_String
				// because created it is expensive (lookuplists, etc). This is only called
				// when the addon is double-clicked in the editor after being dropped
				//
				string AddonGuid = cpCore.docProperties.getText("guid");
				//$$$$$ cache this
				int CS = cpCore.db.csOpen(cnAddons, "ccguid=" + cpCore.db.encodeSQLText(AddonGuid));
				string addonArgumentList = "";
				bool addonIsInline = false;
				if (cpCore.db.csOk(CS))
				{
					addonArgumentList = cpCore.db.csGetText(CS, "argumentlist");
					addonIsInline = cpCore.db.csGetBoolean(CS, "IsInline");
					returnHtml = addonController.main_GetDefaultAddonOption_String(cpCore, addonArgumentList, AddonGuid, addonIsInline);
				}
				cpCore.db.csClose(CS);
			}
			catch (Exception ex)
			{
				cp.Site.ErrorReport(ex);
			}
			return returnHtml;
		}
	}
}
