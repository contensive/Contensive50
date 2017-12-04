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
	public class blockEmailClass : Contensive.BaseClasses.AddonBaseClass
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
				// -- click spam block detected
				if (true)
				{
					//
					string recipientEmailToBlock = cpCore.docProperties.getText(rnEmailBlockRecipientEmail);
					if (string.IsNullOrEmpty(recipientEmailToBlock))
					{
						List<personModel> recipientList = personModel.createList(cpCore, "(email=" + cpCore.db.encodeSQLText(recipientEmailToBlock) + ")");
						foreach (var recipient in recipientList)
						{
							recipient.AllowBulkEmail = false;
							recipient.save(cpCore);
							//
							// -- Email spam footer was clicked, clear the AllowBulkEmail field
							cpCore.email.addToBlockList(recipientEmailToBlock);
							//
							// -- log entry to track the result of this email drop
							int emailDropId = cpCore.docProperties.getInteger(rnEmailBlockRequestDropID);
							if (emailDropId != 0)
							{
								emailDropModel emailDrop = emailDropModel.create(cpCore, emailDropId);
								if (emailDrop != null)
								{
									Models.Entity.emailLogModel log = new Models.Entity.emailLogModel()
									{
										name = "User " + recipient.name + " clicked linked spam block from email drop " + emailDrop.name + " at " + cpCore.doc.profileStartTime.ToString(),
										EmailDropID = emailDrop.id,
										MemberID = recipient.id,
										LogType = EmailLogTypeBlockRequest
									};
								}
							}
							return cpCore.webServer.redirect(cpCore.webServer.requestProtocol + cpCore.webServer.requestDomain + "/ccLib/popup/EmailBlocked.htm", "Group Email Spam Block hit. Redirecting to EmailBlocked page.");
						}
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
