﻿using System;

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
	public class processAddonStyleEditorClass : Contensive.BaseClasses.AddonBaseClass
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
				// save custom styles
				if (cpCore.doc.authContext.isAuthenticated & cpCore.doc.authContext.isAuthenticatedAdmin(cpCore))
				{
					int addonId = cpCore.docProperties.getInteger("AddonID");
					if (addonId > 0)
					{
						Models.Entity.addonModel styleAddon = Models.Entity.addonModel.create(cpCore, addonId);
						if (styleAddon.StylesFilename.content != cpCore.docProperties.getText("CustomStyles"))
						{
							styleAddon.StylesFilename.content = cpCore.docProperties.getText("CustomStyles");
							styleAddon.save(cpCore);
							//
							// Clear Caches
							//
							cpCore.cache.invalidateAllObjectsInContent(addonModel.contentName);
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