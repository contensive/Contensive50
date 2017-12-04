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
using Contensive.Core.Controllers.genericController;
//
namespace Contensive.Addons.AdminSite
{
	public class openAjaxIndexFilterGetContentClass : Contensive.BaseClasses.AddonBaseClass
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
				cpCore.visitProperty.setProperty("IndexFilterOpen", "1");
				Contensive.Addons.AdminSite.getAdminSiteClass adminSite = new Contensive.Addons.AdminSite.getAdminSiteClass(cpCore.cp_forAddonExecutionOnly);
				int ContentID = cpCore.docProperties.getInteger("cid");
				if (ContentID == 0)
				{
					result = "No filter is available";
				}
				else
				{
					Models.Complex.cdefModel cdef = Models.Complex.cdefModel.getCdef(cpCore, ContentID);
					result = adminSite.GetForm_IndexFilterContent(cdef);
				}
				adminSite = null;


			}
			catch (Exception ex)
			{
				cp.Site.ErrorReport(ex);
			}
			return result;
		}
	}
}
