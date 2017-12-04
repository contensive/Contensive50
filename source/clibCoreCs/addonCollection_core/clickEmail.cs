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
	public class clickEmailClass : Contensive.BaseClasses.AddonBaseClass
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
				// -- Email click detected
				emailDropModel emailDrop = emailDropModel.create(cpCore, cpCore.docProperties.getInteger(rnEmailClickFlag));
				if (emailDrop != null)
				{
					personModel recipient = personModel.create(cpCore, cpCore.docProperties.getInteger(rnEmailMemberID), new List<string>());
					if (recipient != null)
					{
						Models.Entity.emailLogModel log = new Models.Entity.emailLogModel()
						{
							name = "User " + recipient.name + " clicked link from email drop " + emailDrop.name + " at " + cpCore.doc.profileStartTime.ToString(),
							EmailDropID = emailDrop.id,
							MemberID = recipient.id,
							LogType = EmailLogTypeOpen
						};
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
