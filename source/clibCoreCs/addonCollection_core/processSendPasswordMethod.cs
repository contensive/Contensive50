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
	public class processSendPasswordMethodClass : Contensive.BaseClasses.AddonBaseClass
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
				// -- send password
				string Emailtext = cpCore.docProperties.getText("email");
				if (!string.IsNullOrEmpty(Emailtext))
				{
					cpCore.email.sendPassword(Emailtext);
					result += ""
						+ "<div style=\"width:300px;margin:100px auto 0 auto;\">"
						+ "<p>An attempt to send login information for email address '" + Emailtext + "' has been made.</p>"
						+ "<p><a href=\"?" + cpCore.doc.refreshQueryString + "\">Return to the Site.</a></p>"
						+ "</div>";
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
