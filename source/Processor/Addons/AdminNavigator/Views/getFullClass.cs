
using System;
using System.Collections.Generic;
using System.Text;
using Contensive.BaseClasses;
using Contensive.Processor.Controllers;

namespace Contensive.Addons.AdminNavigator {
	//
	public class getFullClass : AddonBaseClass {
		//
		//=====================================================================================
		//
		public override object Execute(CPBaseClass CP) {
			string returnHtml = "";
			try {
				returnHtml = GetFullNavigator(CP);
			} catch (Exception ex) {
				errorReport(CP, ex, "execute");
			}
			return (string)returnHtml;
		}
		//
		//=================================================================================
		//   Get the Full Navigator, headers and all
		//=================================================================================
		//
		public string GetFullNavigator(CPBaseClass cp) {
				string tempGetFullNavigator = null;
			try {
				//
				const string NavigatorClosedLabel = "<div style=\"font-size:9px;text-align:center;\">&nbsp;<BR>N<BR>a<BR>v<BR>i<BR>g<BR>a<BR>t<BR>o<BR>r</div>";
				//
				string OpenNodeList = null;
				bool AdminNavOpen = false;
				string AdminNavContent = null;
				string AdminNavHead = null;
				string AdminNavJS = null;
				string AdminNavHeadOpened = null;
				string AdminNavHeadClosed = null;
				string AdminNavContentOpened = null;
				string AdminNavContentClosed = null;
				string AdminNav = null;
				getNodeClass GetNode = null;
				//
				OpenNodeList = cp.Visit.GetText("AdminNavOpenNodeList", "");
				AdminNavOpen = cp.Utils.EncodeBoolean(cp.Visit.GetText("AdminNavOpen", "1"));
				if (AdminNavOpen) {
					//
					// draw the page with Nav Opened
					//
					GetNode = new getNodeClass();
					cp.Doc.SetProperty("nodeid", "0");
					AdminNav = GetNode.Execute(cp).ToString();
					AdminNavHeadOpened = ""
						 + "<div id=\"AdminNavHeadOpened\" class=\"opened\">"
						 + "\t" + "<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr>"
						 + "\t" + "\t" + "<td valign=Middle class=\"left\">Navigator</td>"
						 + "\t" + "\t" + "<td valign=Middle class=\"right\"><a href=\"#\" onClick=\"CloseAdminNav();return false\"><img alt=\"Close Navigator\" title=\"Close Navigator\" src=\"/cclib/images/ClosexRev1313.gif\" width=13 height=13 border=0></a></td>"
						 + "\t" + "</tr></table>"
						 + "</div>"
						+ "";
					AdminNavHeadClosed = ""
						 + "<div id=\"AdminNavHeadClosed\" class=\"closed\" style=\"display:none;\">"
						 + "\t" + "<a href=\"#\" onClick=\"OpenAdminNav();return false\"><img title=\"Open Navigator\" alt=\"Open Navigator\" src=\"/cclib/images/OpenRightRev1313.gif\" width=13 height=13 border=0 style=\"text-align:right;\"></a>"
						 + "</div>"
						+ "";
					AdminNavHead = ""
						 + "<div class=\"ccHeaderCon\">"
						+ GenericController.nop(AdminNavHeadOpened) + GenericController.nop(AdminNavHeadClosed)  + "</div>"
						+ "";
					AdminNavContentOpened = ""
						 + "<div id=\"AdminNavContentOpened\" class=\"opened\">"
						+ GenericController.nop(AdminNav + "<img alt=\"space\" src=\"/cclib/images/spacer.gif\" width=\"200\" height=\"1\" style=\"clear:both\">")  + "</div>"
						+ "";
					AdminNavContentClosed = ""
						 + "<div id=\"AdminNavContentClosed\" class=\"closed\" style=\"display:none;\">" + NavigatorClosedLabel + "</div>"
						+ "";
					AdminNavContent = ""
						 + "<div class=\"ccContentCon\">"
						+ GenericController.nop(AdminNavContentOpened) + GenericController.nop(AdminNavContentClosed)  + "</div>"
						+ "";
					//                & CR & "<div class=""ccContentCon"">" _
					//                & CR & "<div id=""AdminNavContentOpened"" class=""opened"">" _
					//                & KmaIndent(GetNavigator(Main, "0", OpenNodeList) & "<img alt=""space"" src=""/cclib/images/spacer.gif"" width=""200"" height=""1"" style=""clear:both"">") _
					//                & CR & "</div>" _
					//                & CR & "<div id=""AdminNavContentClosed"" class=""closed"" style=""display:none;"">" & NavigatorClosedLabel & "</div>" _
					//                & CR & "</div>"
					AdminNavJS = ""
						 + "function CloseAdminNav() {SetDisplay('AdminNavHeadOpened','none');SetDisplay('AdminNavContentOpened','none');SetDisplay('AdminNavHeadClosed','block');SetDisplay('AdminNavContentClosed','block');cj.ajax.setVisitProperty('','AdminNavOpen','0')}"
						 + "function OpenAdminNav() {SetDisplay('AdminNavHeadOpened','block');SetDisplay('AdminNavContentOpened','block');SetDisplay('AdminNavHeadClosed','none');SetDisplay('AdminNavContentClosed','none');cj.ajax.setVisitProperty('','AdminNavOpen','1')}"
						 + "";
					//        AdminNavJS = "" _
					//            & CR & "function CloseAdminNav() {SetDisplay('AdminNavHeadOpened','none');SetDisplay('AdminNavContentOpened','none');SetDisplay('AdminNavHeadClosed','block');SetDisplay('AdminNavContentClosed','block');cj.ajax.qs('" & RequestNameAjaxFunction & "=" & AjaxCloseAdminNav & "','','')}" _
					//            & CR & "function OpenAdminNav() {SetDisplay('AdminNavHeadOpened','block');SetDisplay('AdminNavContentOpened','block');SetDisplay('AdminNavHeadClosed','none');SetDisplay('AdminNavContentClosed','none');cj.ajax.qs('" & RequestNameAjaxFunction & "=" & AjaxOpenAdminNav & "','','')}" _
					//            & CR & ""
				} else {
					//
					// draw the page with Nav Closed
					//
					AdminNavHeadOpened = ""
						 + "<div id=\"AdminNavHeadOpened\" class=\"opened\" style=\"display:none;\">"
						 + "\t" + "<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr>"
						 + "\t" + "\t" + "<td valign=Middle class=\"left\">Navigator</td>"
						 + "\t" + "\t" + "<td valign=Middle class=\"right\"><a href=\"#\" onClick=\"CloseAdminNav();return false\"><img alt=\"Close Navigator\" title=\"Close Navigator\" src=\"/cclib/images/ClosexRev1313.gif\" width=13 height=13 border=0></a></td>"
						 + "\t" + "</tr></table>"
						 + "</div>"
						+ "";
					AdminNavHeadClosed = ""
						 + "<div id=\"AdminNavHeadClosed\" class=\"closed\">"
						 + "\t" + "<a href=\"#\" onClick=\"OpenAdminNav();return false\"><img title=\"Open Navigator\" alt=\"Open Navigator\" src=\"/cclib/images/OpenRightRev1313.gif\" width=13 height=13 border=0 style=\"text-align:right;\"></a>"
						 + "</div>"
						+ "";
					AdminNavHead = ""
						 + "<div class=\"ccHeaderCon\">"
						+ GenericController.nop(AdminNavHeadOpened) + GenericController.nop(AdminNavHeadClosed)  + "</div>"
						+ "";
					AdminNavContent = ""
						 + "<div class=\"ccContentCon\">"
						 + "<div id=\"AdminNavContentOpened\" class=\"opened\" style=\"display:none;\"><div style=\"text-align:center;\"><img src=\"/cclib/images/ajax-loader-small.gif\" width=16 height=16></div></div>"
						 + "<div id=\"AdminNavContentClosed\" class=\"closed\">" + NavigatorClosedLabel + "</div>"
						 + "<div id=\"AdminNavContentMinWidth\" style=\"display:none;\"><img alt=\"space\" src=\"/cclib/images/spacer.gif\" width=\"200\" height=\"1\" style=\"clear:both\"></div>"
						 + "</div>";
					AdminNavJS = ""
						 + "var AdminNavPop=false;"
						 + "function CloseAdminNav() {SetDisplay('AdminNavHeadOpened','none');SetDisplay('AdminNavHeadClosed','block');SetDisplay('AdminNavContentOpened','none');SetDisplay('AdminNavContentMinWidth','none');SetDisplay('AdminNavContentClosed','block');cj.ajax.setVisitProperty('','AdminNavOpen','0')}"
						 + "function OpenAdminNav() {SetDisplay('AdminNavHeadOpened','block');SetDisplay('AdminNavHeadClosed','none');SetDisplay('AdminNavContentOpened','block');SetDisplay('AdminNavContentMinWidth','block');SetDisplay('AdminNavContentClosed','none');cj.ajax.setVisitProperty('','AdminNavOpen','1');if(!AdminNavPop){cj.ajax.addon('AdminNavigatorGetNode','','','AdminNavContentOpened');AdminNavPop=true;}else{cj.ajax.addon('AdminNavigatorOpenNode');}}"
						 + "";
				}
				cp.Doc.AddHeadJavascript(AdminNavJS);
				//
				//
				//
				tempGetFullNavigator = ""
					 + "<div>"
					+ GenericController.nop(AdminNavHead) + GenericController.nop(AdminNavContent)  + "</div>"
					+ "";
				//
				return tempGetFullNavigator;
			} catch {
				goto ErrorTrap;
			}
ErrorTrap:
			//HandleError
return tempGetFullNavigator;
		}

		//
		//
		//
		//
		//
		//=====================================================================================
		// common report for this class
		//=====================================================================================
		//
		private void errorReport(CPBaseClass cp, Exception ex, string method) {
			try {
				cp.Site.ErrorReport(ex, "Unexpected error in sampleClass." + method);
			} catch (Exception exLost) {
				//
				// stop anything thrown from cp errorReport
				//
			}
		}
	}
}
