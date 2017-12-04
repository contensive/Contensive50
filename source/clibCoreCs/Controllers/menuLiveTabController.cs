using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

//
namespace Contensive.Core.Controllers
{
	public class menuLiveTabController
	{
		//
		private struct TabType
		{
			public string Caption;
			public string StylePrefix;
			public string LiveBody;
		}
		private TabType[] Tabs;
		private int TabsCnt;
		private int TabsSize;
		//
		private menuComboTabController comboTab = new menuComboTabController();
		//
		public void AddEntry(string Caption, string LiveBody, string StylePrefix = "")
		{
			comboTab.AddEntry(Caption, "", "", LiveBody, false, "ccAdminTab");
		}
		//
		public string GetTabs()
		{
			return comboTab.GetTabs();
		}
		//
		public string GetTabBlank()
		{
			return comboTab.GetTabBlank();
		}
	}
}
