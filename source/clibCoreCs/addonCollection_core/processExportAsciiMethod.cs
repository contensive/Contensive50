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
	public class processExportAsciiMethodClass : Contensive.BaseClasses.AddonBaseClass
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
				// -- Should be a remote method in commerce
				if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore))
				{
					//
					// Administrator required
					//
					cpCore.doc.userErrorList.Add("Error: You must be an administrator to use the ExportAscii method");
				}
				else
				{
					string ContentName = cpCore.docProperties.getText("content");
					int PageSize = cpCore.docProperties.getInteger("PageSize");
					if (PageSize == 0)
					{
						PageSize = 20;
					}
					int PageNumber = cpCore.docProperties.getInteger("PageNumber");
					if (PageNumber == 0)
					{
						PageNumber = 1;
					}
					if (string.IsNullOrEmpty(ContentName))
					{
						cpCore.doc.userErrorList.Add("Error: ExportAscii method requires ContentName");
					}
					else
					{
						result = Controllers.exportAsciiController.exportAscii_GetAsciiExport(cpCore, ContentName, PageSize, PageNumber);
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
