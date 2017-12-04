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
using static Contensive.Core.constants;
using System.Collections.Generic;
//
namespace Contensive.Addons.AdminSite
{
	//
	public class getAjaxVisitPropertyClass : Contensive.BaseClasses.AddonBaseClass
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
                gd.col = new List<ColsType>();
                gd.row = new List<RowsType>();
				gd.IsEmpty = false;
                for (var ptr = 0; ptr <= Args.GetUpperBound(0); ptr++)
				{
                    ColsType col = new ColsType();
                    CellType cell = new CellType();
					string[] ArgNameValue = Args[ptr].Split('=');
					col.Id = ArgNameValue[0];
                    col.Label = ArgNameValue[0];
					col.Type = "string";
					string PropertyValue = "";
					if (ArgNameValue.GetUpperBound(0) > 0)
					{
						PropertyValue = ArgNameValue[1];
					}
                    cell.v = cpCore.visitProperty.getText(ArgNameValue[0], PropertyValue);
                    gd.row[0].Cell.Add(cell);
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
