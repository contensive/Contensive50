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
	//
	public class setAjaxVisitPropertyClass : Contensive.BaseClasses.AddonBaseClass
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

				string ArgList = cpCore.docProperties.getText("args");
				string[] Args = ArgList.Split('&');
				GoogleDataType gd = new GoogleDataType();
				gd.IsEmpty = true;
				for (var Ptr = 0; Ptr <= Args.GetUpperBound(0); Ptr++)
				{
					string[] ArgNameValue = Args[Ptr].Split('=');
					string PropertyName = ArgNameValue[0];
					string PropertyValue = "";
					if (ArgNameValue.GetUpperBound(0) > 0)
					{
						PropertyValue = ArgNameValue[1];
					}
					cpCore.visitProperty.setProperty(PropertyName, PropertyValue);
				}
				result = remoteQueryController.main_FormatRemoteQueryOutput(cpCore, gd, RemoteFormatEnum.RemoteFormatJsonNameValue);
				result = cpCore.html.main_encodeHTML(result);
			}
			catch (Exception ex)
			{
				cp.Site.ErrorReport(ex);
			}
			return result;
		}
	}
}
