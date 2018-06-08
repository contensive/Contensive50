
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Controllers {
    public class menuComboTabController {

        private struct TabType {
            public string Caption;
            public string Link;
            public string AjaxLink;
            public string ContainerClass;
            public bool IsHit;
            public string LiveBody;
        }
        private TabType[] Tabs;
        private int TabsCnt;
        private int TabsSize;
        //
        //
        //
        public void AddEntry(string Caption, string Link, string AjaxLink, string LiveBody, bool IsHit, string ContainerClass) {
            try {
                if (TabsCnt <= TabsSize) {
                    TabsSize = TabsSize + 10;
                    Array.Resize(ref Tabs, TabsSize + 1);
                }
                Tabs[TabsCnt].Caption = Caption;
                Tabs[TabsCnt].Link = Link;
                Tabs[TabsCnt].AjaxLink = AjaxLink;
                Tabs[TabsCnt].IsHit = IsHit;
                if (string.IsNullOrEmpty(ContainerClass)) {
                    Tabs[TabsCnt].ContainerClass = "ccLiveTab";
                } else {
                    Tabs[TabsCnt].ContainerClass = ContainerClass;
                }
                Tabs[TabsCnt].LiveBody = encodeEmpty(LiveBody, "");
                TabsCnt = TabsCnt + 1;
            } catch (Exception) {
                throw;
            }
        }
        //
        //
        //
        public string GetTabs(coreController core ) {
            string result = "";
            try {
                //
                string TabBodyCollectionWrapStyle = "";
                string TabStyle = null;
                string TabHitStyle = null;
                string TabLinkStyle = null;
                string TabHitLinkStyle = null;
                string TabBodyWrapHideStyle = null;
                string TabBodyWrapShowStyle = null;
                string TabEndStyle = "";
                string TabEdgeStyle = null;
                //
                int TabPtr = 0;
                int HitPtr = 0;
                string TabBody = "";
                string TabLink = null;
                string TabAjaxLink = null;
                string TabID = null;
                string LiveBodyID = null;
                bool FirstLiveBodyShown = false;
                string TabBodyStyle = null;
                string JSClose = "";
                string TabWrapperID = null;
                string TabBlank = null;
                int IDNumber = 0;
                //
                if (TabsCnt > 0) {
                    HitPtr = 0;
                    //
                    // Create TabBar
                    //
                    TabWrapperID = "TabWrapper" + genericController.GetRandomInteger(core);
                    TabBlank = GetTabBlank();
                    result += "<script language=\"JavaScript\" src=\"/ccLib/clientside/ccDynamicTab.js\" type=\"text/javascript\"></script>\r\n";
                    result += "<table border=0 cellspacing=0 cellpadding=0 width=\"100%\"><tr>";
                    for (TabPtr = 0; TabPtr < TabsCnt; TabPtr++) {
                        TabStyle = Tabs[TabPtr].ContainerClass;
                        TabLink = Tabs[TabPtr].Link;
                        TabAjaxLink = Tabs[TabPtr].AjaxLink;
                        TabHitStyle = TabStyle + "Hit";
                        TabLinkStyle = TabStyle + "Link";
                        TabHitLinkStyle = TabStyle + "HitLink";
                        TabEndStyle = TabStyle + "End";
                        TabEdgeStyle = TabStyle + "Edge";
                        TabBodyStyle = TabStyle + "Body";
                        TabBodyWrapShowStyle = TabStyle + "BodyWrapShow";
                        TabBodyWrapHideStyle = TabStyle + "BodyWrapHide";
                        TabBodyCollectionWrapStyle = TabStyle + "BodyCollectionWrap";
                        IDNumber = genericController.GetRandomInteger(core);
                        LiveBodyID = "TabContent" + IDNumber;
                        TabID = "Tab" + IDNumber;
                        //
                        // This tab is hit
                        //
                        result += "<td valign=bottom>" + TabBlank + "</td>";
                        result = genericController.vbReplace(result, "Replace-TabID", TabID);
                        result = genericController.vbReplace(result, "Replace-StyleEdge", TabEdgeStyle);
                        if (!string.IsNullOrEmpty(TabAjaxLink)) {
                            //
                            // Ajax tab
                            //
                            result = genericController.vbReplace(result, "Replace-HotSpot", "<a href=# Class=\"" + TabLinkStyle + "\" name=tabLink onClick=\"if(document.getElementById('unloaded_" + LiveBodyID + "')){GetURLAjax('" + TabAjaxLink + "','','" + LiveBodyID + "','','')};switchLiveTab2('" + LiveBodyID + "', this,'" + TabID + "','" + TabStyle + "','" + TabWrapperID + "');return false;\">" + Tabs[TabPtr].Caption + "</a>");
                            result = genericController.vbReplace(result, "Replace-StyleHit", TabStyle);
                            TabBody = TabBody + "<div id=\"" + LiveBodyID + "\" class=\"" + TabBodyStyle + "\" style=\"display:none;text-align:center\"><div id=\"unloaded_" + LiveBodyID + "\"  style=\"text-align:center;padding-top:50px;\"><img src=\"/ccLib/images/ajax-loader-big.gif\" border=0 width=32 height=32></div></div>";
                            //TabBody = TabBody & "<div onload=""alert('" & LiveBodyID & " onload');"" id=""" & LiveBodyID & """ class=""" & TabBodyStyle & """ style=""display:none;text-align:center""><div id=""unloaded_" & LiveBodyID & """  style=""text-align:center;padding-top:50px;""><img src=""/ccLib/images/ajax-loader-big.gif"" border=0 width=32 height=32></div></div>"
                        } else if (!string.IsNullOrEmpty(TabLink)) {
                            //
                            // Link back to server tab
                            //
                            result = genericController.vbReplace(result, "Replace-HotSpot", "<a href=\"" + TabLink + "\" Class=\"" + TabHitLinkStyle + "\">" + Tabs[TabPtr].Caption + "</a>");
                            //result = genericController.vbReplace(result, "Replace-HotSpot", "<a href=# Class=""" & TabLinkStyle & """ name=tabLink onClick=""switchLiveTab2('" & LiveBodyID & "', this,'" & TabID & "','" & TabStyle & "','" & TabWrapperID & "');return false;"">" & Tabs(TabPtr).Caption & "</a>")
                            result = genericController.vbReplace(result, "Replace-StyleHit", TabStyle);
                        } else {
                            //
                            // Live Tab
                            //
                            if (!FirstLiveBodyShown) {
                                FirstLiveBodyShown = true;
                                result = genericController.vbReplace(result, "Replace-HotSpot", "<a href=# Class=\"" + TabHitLinkStyle + "\" name=tabLink onClick=\"switchLiveTab2('" + LiveBodyID + "', this,'" + TabID + "','" + TabStyle + "','" + TabWrapperID + "');return false;\">" + Tabs[TabPtr].Caption + "</a>");
                                result = genericController.vbReplace(result, "Replace-StyleHit", TabHitStyle);
                                JSClose = JSClose + "ActiveTabTableID=\"" + TabID + "\";ActiveContentDivID=\"" + LiveBodyID + "\";";
                                TabBody = TabBody + "<div id=\"" + LiveBodyID + "\" class=\"" + TabBodyWrapShowStyle + "\">"
                                + "<div class=\"" + TabBodyStyle + "\">"
                                + Tabs[TabPtr].LiveBody + "</div>"
                                + "</div>"
                                + "";
                            } else {
                                result = genericController.vbReplace(result, "Replace-HotSpot", "<a href=# Class=\"" + TabLinkStyle + "\" name=tabLink onClick=\"switchLiveTab2('" + LiveBodyID + "', this,'" + TabID + "','" + TabStyle + "','" + TabWrapperID + "');return false;\">" + Tabs[TabPtr].Caption + "</a>");
                                result = genericController.vbReplace(result, "Replace-StyleHit", TabStyle);
                                TabBody = TabBody + "<div id=\"" + LiveBodyID + "\" class=\"" + TabBodyWrapHideStyle + "\">"
                                + "<div class=\"" + TabBodyStyle + "\">"
                                + Tabs[TabPtr].LiveBody + "</div>"
                                + "</div>"
                                + "";
                            }
                        }
                        HitPtr = TabPtr;
                    }
                    result += "<td width=\"100%\" class=\"" + TabEndStyle + "\">&nbsp;</td></tr></table>";
                    result += "<div ID=\"" + TabWrapperID + "\" class=\"" + TabBodyCollectionWrapStyle + "\">" + TabBody + "</div>";
                    result += "<script type=text/javascript>" + JSClose + "</script>\r\n";
                    TabsCnt = 0;
                }
            } catch (Exception) {
                throw;
            }
            return result;
        }
        //
        //
        //
        public string GetTabBlank() {
            string result = "";
            try {
                result += "<!--\r\nTab Replace-TabID\r\n-->"
                + "<table cellspacing=0 cellPadding=0 border=0 id=Replace-TabID>";
                result += "\r\n<tr>"
                    + "\r\n<td id=Replace-TabIDR00 colspan=2 class=\"\" height=1 width=2></td>"
                    + "\r\n<td id=Replace-TabIDR01 colspan=1 class=\"Replace-StyleEdge\" height=1></td>"
                    + "\r\n<td id=Replace-TabIDR02 colspan=3 class=\"\" height=1 width=3></td>"
                    + "\r\n</tr>";
                //result = result _
                //    & vbCrLf & "<tr>" _
                //    & vbCrLf & "<td id=Replace-TabIDR00 colspan=2 class="""" height=1 width=2><img src=""/ccLib/images/spacer.gif"" width=2 height=1></td>" _
                //    & vbCrLf & "<td id=Replace-TabIDR01 colspan=1 class=""Replace-StyleEdge"" height=1></td>" _
                //    & vbCrLf & "<td id=Replace-TabIDR02 colspan=3 class="""" height=1 width=3><img src=""/ccLib/images/spacer.gif"" width=3 height=1></td>" _
                //    & vbCrLf & "</tr>"
                result += "\r\n<tr>"
                + "\r\n<td id=Replace-TabIDR10 colspan=1 class=\"\" height=1 width=1></td>"
                + "\r\n<td id=Replace-TabIDR11 colspan=1 class=\"Replace-StyleEdge\" height=1 width=1></td>"
                + "\r\n<td id=Replace-TabIDR12 colspan=1 class=\"Replace-StyleHit\" height=1></td>"
                + "\r\n<td id=Replace-TabIDR13 colspan=1 class=\"Replace-StyleEdge\" height=1 width=1></td>"
                + "\r\n<td id=Replace-TabIDR14 colspan=2 class=\"\" height=1 width=2></td>"
                + "\r\n</tr>";
                result += "\r\n<tr>"
                + "\r\n<td id=Replace-TabIDR20 colspan=1 height=2 class=\"Replace-StyleEdge\"></td>"
                + "\r\n<td id=Replace-TabIDR21 colspan=1 height=2 Class=\"Replace-StyleHit\"></td>"
                + "\r\n<td id=Replace-TabIDR22 colspan=1 height=2 Class=\"Replace-StyleHit\"></td>"
                + "\r\n<td id=Replace-TabIDR23 colspan=1 height=2 Class=\"Replace-StyleHit\"></td>"
                + "\r\n<td id=Replace-TabIDR24 colspan=1 height=2 width=1 class=\"Replace-StyleEdge\"></td>"
                + "\r\n<td id=Replace-TabIDR25 colspan=1 height=2 width=1 Class=\"\"></td>"
                + "\r\n</tr>";
                result += "\r\n<tr>"
                + "\r\n<td id=Replace-TabIDR30 class=\"Replace-StyleEdge\"></td>"
                + "\r\n<td id=Replace-TabIDR31 Class=\"Replace-StyleHit\"></td>"
                + "\r\n<td id=Replace-TabIDR32 Class=\"Replace-StyleHit\" style=\"padding-right:10px;padding-left:10px;padding-bottom:2px;\">Replace-HotSpot</td>"
                + "\r\n<td id=Replace-TabIDR33 Class=\"Replace-StyleHit\"></td>"
                + "\r\n<td id=Replace-TabIDR34 class=\"Replace-StyleEdge\"></td>"
                + "\r\n<td id=Replace-TabIDR35 class=\"\"></td>"
                + "\r\n</tr>";
                result += "\r\n<tr>"
                + "\r\n<td id=Replace-TabIDR40 class=\"Replace-StyleEdge\"></td>"
                + "\r\n<td id=Replace-TabIDR41 Class=\"Replace-StyleHit\"></td>"
                + "\r\n<td id=Replace-TabIDR42 Class=\"Replace-StyleHit\"></td>"
                + "\r\n<td id=Replace-TabIDR43 Class=\"Replace-StyleHit\"></td>"
                + "\r\n<td id=Replace-TabIDR44 class=\"Replace-StyleEdge\"></td>"
                + "\r\n<td id=Replace-TabIDR45 class=\"\" ></td>"
                + "\r\n</tr>"
                + "\r\n</table>";
            } catch (Exception) {
                throw;
            }
            return result;
        }
    }
}
