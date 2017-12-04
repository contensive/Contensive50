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
	public class processStatusMethodClass : Contensive.BaseClasses.AddonBaseClass
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
				// test default data connection
				//
				bool InsertTestOK = false;
				int TrapID = 0;
				int CS = cpCore.db.csInsertRecord("Trap Log");
				if (!cpCore.db.csOk(CS))
				{
					throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError10(ignoreInteger, "dll", "Error during Status. Called InsertCSRecord to insert 'Trap Log' test, record set was not OK.", "Init", False, True)
				}
				else
				{
					InsertTestOK = true;
					TrapID = cpCore.db.csGetInteger(CS, "ID");
				}
				cpCore.db.csClose(CS);
				if (InsertTestOK)
				{
					if (TrapID == 0)
					{
						throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError10(ignoreInteger, "dll", "Error during Status. Called InsertCSRecord to insert 'Trap Log' test, record set was OK, but ID=0.", "Init", False, True)
					}
					else
					{
						cpCore.db.deleteContentRecord("Trap Log", TrapID);
					}
				}
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
				if (Microsoft.VisualBasic.Information.Err().Number != 0)
				{
					throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError10(ignoreInteger, "dll", "Error during Status. After traplog insert, " & genericController.GetErrString(Err), "Init", False, True)
//INSTANT C# TODO TASK: Calls to the VB 'Err' function are not converted by Instant C#:
					Microsoft.VisualBasic.Information.Err().Clear();
				}
				//
				// Close page
				//
				cpCore.webServer.clearResponseBuffer();
				if (cpCore.doc.errorCount == 0)
				{
					result = "Contensive OK";
				}
				else
				{
					result = "Contensive Error Count = " + cpCore.doc.errorCount;
				}
				result = cpCore.html.getHtmlDoc_beforeEndOfBodyHtml(false, false);
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
