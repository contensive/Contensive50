
using System;
using System.Collections.Generic;
using static Contensive.Processor.Controllers.GenericController;

namespace Contensive.Processor.Controllers {
    /// <summary>
    /// create admin tab system
    /// use https://www.w3schools.com/bootstrap4/bootstrap_navs.asp
    /// </summary>
    public class MenuComboTabController {
        private struct TabType {
            public string caption;
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
        //====================================================================================================
        /// <summary>
        /// add a tab to the object
        /// </summary>
        /// <param name="Caption"></param>
        /// <param name="Link"></param>
        /// <param name="AjaxLink"></param>
        /// <param name="LiveBody"></param>
        /// <param name="IsHit"></param>
        /// <param name="ContainerClass"></param>
        public void AddEntry(string Caption, string Link, string AjaxLink, string LiveBody, bool IsHit, string ContainerClass) {
            try {
                if (TabsCnt <= TabsSize) {
                    TabsSize = TabsSize + 10;
                    Array.Resize(ref Tabs, TabsSize + 1);
                }
                Tabs[TabsCnt].caption = Caption;
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
        //====================================================================================================
        /// <summary>
        /// retrieve the html for the tabs in the object
        /// change to https://www.w3schools.com/bootstrap4/bootstrap_navs.asp
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public string GetTabs(CoreController core ) {
            string result = "";
            try {
                if (TabsCnt > 0) {
                    //
                    // -- bootstrap 4 version
                    string tabHtmlClass = "nav-link active";
                    var tabList = new List<string>();
                    string containerHtmlClass = "tab-pane container-full active";
                    var containerList = new List<string>();
                    for (int TabPtr = 0; TabPtr < TabsCnt; TabPtr++) {
                        int containerSN = GenericController.GetRandomInteger(core);
                        string containerHtmlId = "TabContent" + containerSN;
                        //
                        // -- tab
                        // <a class="nav-link active" data-toggle="tab" href="#home">x</a>
                        string item = HtmlController.a(Tabs[TabPtr].caption, "#" + containerHtmlId, tabHtmlClass).Replace(">", " data-toggle=\"tab\">");
                        // <li class="nav-item">x</li>
                        tabList.Add( HtmlController.li(item, "nav-item"));
                        //
                        // -- container
                        // <div class="tab-pane container active" id="home">x</div>
                        string wrappedLiveBody = HtmlController.div(Tabs[TabPtr].LiveBody, Tabs[TabPtr].ContainerClass + "Body");
                        containerList.Add(HtmlController.div(wrappedLiveBody, containerHtmlClass, containerHtmlId));
                        tabHtmlClass = "nav-link";
                        containerHtmlClass = "tab-pane container-full";
                    }
                    // <ul class="nav nav-tabs">x</ul>
                    string tabBar = HtmlController.ul(String.Join("", tabList.ToArray()), "nav nav-tabs flex-nowrap");
                    result += HtmlController.div(tabBar, "tab-bar");
                    result += HtmlController.div(String.Join("", containerList.ToArray()), "tab-content");


                    ////
                    //// -- old version
                    ////
                    //// Create TabBar
                    //string TabWrapperID = "TabWrapper" + GenericController.GetRandomInteger(core);
                    //string TabBlank = GetTabBlank();
                    //result += "<table border=0 cellspacing=0 cellpadding=0 width=\"100%\"><tr>";
                    ////
                    //string TabBodyCollectionWrapStyle = "";
                    //string TabEndStyle = "";
                    ////
                    //string TabBody = "";
                    //string JSClose = "";
                    //for (int TabPtr = 0; TabPtr < TabsCnt; TabPtr++) {
                    //    string TabStyle = Tabs[TabPtr].ContainerClass;
                    //    string TabLink = Tabs[TabPtr].Link;
                    //    string TabAjaxLink = Tabs[TabPtr].AjaxLink;
                    //    string TabHitStyle = TabStyle + "Hit";
                    //    string TabLinkStyle = TabStyle + "Link";
                    //    string TabHitLinkStyle = TabStyle + "HitLink";
                    //    TabEndStyle = TabStyle + "End";
                    //    string TabEdgeStyle = TabStyle + "Edge";
                    //    string TabBodyStyle = TabStyle + "Body";
                    //    string TabBodyWrapShowStyle = TabStyle + "BodyWrapShow";
                    //    string TabBodyWrapHideStyle = TabStyle + "BodyWrapHide";
                    //    TabBodyCollectionWrapStyle = TabStyle + "BodyCollectionWrap";
                    //    int IDNumber = GenericController.GetRandomInteger(core);
                    //    string LiveBodyID = "TabContent" + IDNumber;
                    //    string TabID = "Tab" + IDNumber;
                    //    //
                    //    // This tab is hit
                    //    //
                    //    result += "<td valign=bottom>" + TabBlank + "</td>";
                    //    result = GenericController.vbReplace(result, "Replace-TabID", TabID);
                    //    result = GenericController.vbReplace(result, "Replace-StyleEdge", TabEdgeStyle);
                    //    if (!string.IsNullOrEmpty(TabAjaxLink)) {
                    //        //
                    //        // Ajax tab
                    //        //
                    //        result = GenericController.vbReplace(result, "Replace-HotSpot", "<a href=# Class=\"" + TabLinkStyle + "\" name=tabLink onClick=\"if(document.getElementById('unloaded_" + LiveBodyID + "')){GetURLAjax('" + TabAjaxLink + "','','" + LiveBodyID + "','','')};switchLiveTab2('" + LiveBodyID + "', this,'" + TabID + "','" + TabStyle + "','" + TabWrapperID + "');return false;\">" + Tabs[TabPtr].caption + "</a>");
                    //        result = GenericController.vbReplace(result, "Replace-StyleHit", TabStyle);
                    //        TabBody = TabBody + "<div id=\"" + LiveBodyID + "\" class=\"" + TabBodyStyle + "\" style=\"display:none;text-align:center\"><div id=\"unloaded_" + LiveBodyID + "\"  style=\"text-align:center;padding-top:50px;\"><img src=\"/ContensiveBase/images/ajax-loader-big.gif\" border=0 width=32 height=32></div></div>";
                    //        //TabBody = TabBody & "<div onload=""alert('" & LiveBodyID & " onload');"" id=""" & LiveBodyID & """ class=""" & TabBodyStyle & """ style=""display:none;text-align:center""><div id=""unloaded_" & LiveBodyID & """  style=""text-align:center;padding-top:50px;""><img src=""/ContensiveBase/images/ajax-loader-big.gif"" border=0 width=32 height=32></div></div>"
                    //    } else if (!string.IsNullOrEmpty(TabLink)) {
                    //        //
                    //        // Link back to server tab
                    //        //
                    //        result = GenericController.vbReplace(result, "Replace-HotSpot", "<a href=\"" + TabLink + "\" Class=\"" + TabHitLinkStyle + "\">" + Tabs[TabPtr].caption + "</a>");
                    //        //result = genericController.vbReplace(result, "Replace-HotSpot", "<a href=# Class=""" & TabLinkStyle & """ name=tabLink onClick=""switchLiveTab2('" & LiveBodyID & "', this,'" & TabID & "','" & TabStyle & "','" & TabWrapperID & "');return false;"">" & Tabs(TabPtr).Caption & "</a>")
                    //        result = GenericController.vbReplace(result, "Replace-StyleHit", TabStyle);
                    //    } else {
                    //        bool FirstLiveBodyShown = false;
                    //        //
                    //        // Live Tab
                    //        //
                    //        if (!FirstLiveBodyShown) {
                    //            FirstLiveBodyShown = true;
                    //            result = GenericController.vbReplace(result, "Replace-HotSpot", "<a href=# Class=\"" + TabHitLinkStyle + "\" name=tabLink onClick=\"switchLiveTab2('" + LiveBodyID + "', this,'" + TabID + "','" + TabStyle + "','" + TabWrapperID + "');return false;\">" + Tabs[TabPtr].caption + "</a>");
                    //            result = GenericController.vbReplace(result, "Replace-StyleHit", TabHitStyle);
                    //            JSClose = JSClose + "var ActiveTabTableID=\"" + TabID + "\";var ActiveContentDivID=\"" + LiveBodyID + "\";";
                    //            TabBody = TabBody + "<div id=\"" + LiveBodyID + "\" class=\"" + TabBodyWrapShowStyle + "\">"
                    //            + "<div class=\"" + TabBodyStyle + "\">"
                    //            + Tabs[TabPtr].LiveBody + "</div>"
                    //            + "</div>"
                    //            + "";
                    //        } else {
                    //            result = GenericController.vbReplace(result, "Replace-HotSpot", "<a href=# Class=\"" + TabLinkStyle + "\" name=tabLink onClick=\"switchLiveTab2('" + LiveBodyID + "', this,'" + TabID + "','" + TabStyle + "','" + TabWrapperID + "');return false;\">" + Tabs[TabPtr].caption + "</a>");
                    //            result = GenericController.vbReplace(result, "Replace-StyleHit", TabStyle);
                    //            TabBody = TabBody + "<div id=\"" + LiveBodyID + "\" class=\"" + TabBodyWrapHideStyle + "\">"
                    //            + "<div class=\"" + TabBodyStyle + "\">"
                    //            + Tabs[TabPtr].LiveBody + "</div>"
                    //            + "</div>"
                    //            + "";
                    //        }
                    //    }
                    //}
                    //result += "<td width=\"100%\" class=\"" + TabEndStyle + "\">&nbsp;</td></tr></table>";
                    //result += "<div ID=\"" + TabWrapperID + "\" class=\"" + TabBodyCollectionWrapStyle + "\">" + TabBody + "</div>";
                    //result += "<script type=text/javascript>" + JSClose + "</script>\r\n";
                    //TabsCnt = 0;
                }
            } catch (Exception) {
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public string GetTabBlank() {
            string result = "";
            try {
                result += "<!--Tab Replace-TabID-->"
                    + "<table cellspacing=0 cellPadding=0 border=0 id=Replace-TabID>";
                result += "<tr>"
                    + "<td id=Replace-TabIDR00 colspan=2 class=\"\" height=1 width=2></td>"
                    + "<td id=Replace-TabIDR01 colspan=1 class=\"Replace-StyleEdge\" height=1></td>"
                    + "<td id=Replace-TabIDR02 colspan=3 class=\"\" height=1 width=3></td>"
                    + "</tr>";
                result += "<tr>"
                    + "<td id=Replace-TabIDR10 colspan=1 class=\"\" height=1 width=1></td>"
                    + "<td id=Replace-TabIDR11 colspan=1 class=\"Replace-StyleEdge\" height=1 width=1></td>"
                    + "<td id=Replace-TabIDR12 colspan=1 class=\"Replace-StyleHit\" height=1></td>"
                    + "<td id=Replace-TabIDR13 colspan=1 class=\"Replace-StyleEdge\" height=1 width=1></td>"
                    + "<td id=Replace-TabIDR14 colspan=2 class=\"\" height=1 width=2></td>"
                    + "</tr>";
                result += "<tr>"
                    + "<td id=Replace-TabIDR20 colspan=1 height=2 class=\"Replace-StyleEdge\"></td>"
                    + "<td id=Replace-TabIDR21 colspan=1 height=2 Class=\"Replace-StyleHit\"></td>"
                    + "<td id=Replace-TabIDR22 colspan=1 height=2 Class=\"Replace-StyleHit\"></td>"
                    + "<td id=Replace-TabIDR23 colspan=1 height=2 Class=\"Replace-StyleHit\"></td>"
                    + "<td id=Replace-TabIDR24 colspan=1 height=2 width=1 class=\"Replace-StyleEdge\"></td>"
                    + "<td id=Replace-TabIDR25 colspan=1 height=2 width=1 Class=\"\"></td>"
                    + "</tr>";
                result += "<tr>"
                    + "<td id=Replace-TabIDR30 class=\"Replace-StyleEdge\"></td>"
                    + "<td id=Replace-TabIDR31 Class=\"Replace-StyleHit\"></td>"
                    + "<td id=Replace-TabIDR32 Class=\"Replace-StyleHit\" style=\"padding-right:10px;padding-left:10px;padding-bottom:2px;\">Replace-HotSpot</td>"
                    + "<td id=Replace-TabIDR33 Class=\"Replace-StyleHit\"></td>"
                    + "<td id=Replace-TabIDR34 class=\"Replace-StyleEdge\"></td>"
                    + "<td id=Replace-TabIDR35 class=\"\"></td>"
                    + "</tr>";
                result += "<tr>"
                    + "<td id=Replace-TabIDR40 class=\"Replace-StyleEdge\"></td>"
                    + "<td id=Replace-TabIDR41 Class=\"Replace-StyleHit\"></td>"
                    + "<td id=Replace-TabIDR42 Class=\"Replace-StyleHit\"></td>"
                    + "<td id=Replace-TabIDR43 Class=\"Replace-StyleHit\"></td>"
                    + "<td id=Replace-TabIDR44 class=\"Replace-StyleEdge\"></td>"
                    + "<td id=Replace-TabIDR45 class=\"\" ></td>"
                    + "</tr>"
                    + "</table>";
            } catch (Exception) {
                throw;
            }
            return result;
        }
    }
}
