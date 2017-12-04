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
	public class processRedirectMethodClass : Contensive.BaseClasses.AddonBaseClass
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
				// ----- Redirect with RC and RI
				//
				cpCore.doc.redirectContentID = cpCore.docProperties.getInteger(rnRedirectContentId);
				cpCore.doc.redirectRecordID = cpCore.docProperties.getInteger(rnRedirectRecordId);
				if (cpCore.doc.redirectContentID != 0 & cpCore.doc.redirectRecordID != 0)
				{
					string ContentName = models.complex.cdefmodel.getContentNameByID(cpCore,cpCore.doc.redirectContentID);
					if (!string.IsNullOrEmpty(ContentName))
					{
						iisController.main_RedirectByRecord_ReturnStatus(cpCore, ContentName, cpCore.doc.redirectRecordID);
						result = "";
						cpCore.doc.continueProcessing = false;
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
