
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

using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.BaseClasses;
using Contensive.Models.Db;
//
//
namespace Contensive.Processor.Controllers {
    //
    /// <summary>
    ///   A LinkAlias name is a unique string that identifies a page on the site.
    ///   A page on the site is generated from the PageID, and the QueryStringSuffix
    ///   PageID - obviously, this is the ID of the page
    ///   QueryStringSuffix - other things needed on the Query to display the correct content.
    ///   The Suffix is needed in cases like when an Add-on is embedded in a page. The URL to that content becomes the pages
    ///   Link, plus the suffix needed to find the content.
    ///   When you make the menus, look up the most recent Link Alias entry with the pageID, and a blank QueryStringSuffix
    ///   The Link Alias table no longer needs the Link field.
    /// </summary>
    public class LinkAliasController {
        //
        //====================================================================================================
        /// <summary>
        /// Returns the Alias link (SourceLink) from the actual link (DestinationLink)
        /// </summary>
        public static string getLinkAlias(CoreController core, int PageID, string QueryStringSuffix, string DefaultLink) {
            string linkAlias = DefaultLink;
            List<LinkAliasModel> linkAliasList = LinkAliasModel.createPageList(core.cpParent, PageID, QueryStringSuffix);
            if (linkAliasList.Count > 0) {
                linkAlias = linkAliasList.First().name;
                if (linkAlias.Left(1) != "/") {
                    linkAlias = "/" + linkAlias;
                }
            }
            return linkAlias;
        }
        //
        //====================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="core"></param>
        /// <param name="linkAlias"></param>
        /// <param name="PageID"></param>
        /// <param name="QueryStringSuffix"></param>
        /// <param name="OverRideDuplicate">Should alway be true except in admin edit page where the user may not realize</param>
        /// <param name="DupCausesWarning">Always false except in admin edit where the user needs a warning</param>
        public static void addLinkAlias(CoreController core, string linkAlias, int PageID, string QueryStringSuffix, bool OverRideDuplicate, bool DupCausesWarning) {
            string tempVar = "";
            addLinkAlias(core, linkAlias, PageID, QueryStringSuffix, OverRideDuplicate, DupCausesWarning, ref tempVar);
        }
        //
        //====================================================================================================
        //
        public static void addLinkAlias(CoreController core, string linkAlias, int PageID, string QueryStringSuffix) {
            string tempVar = "";
            addLinkAlias(core, linkAlias, PageID, QueryStringSuffix, true, false, ref tempVar);
        }
        //
        //====================================================================================================
        /// <summary>
        /// add a link alias to a page as the primary
        /// </summary>
        public static void addLinkAlias(CoreController core, string linkAlias, int pageID, string queryStringSuffix, bool overRideDuplicate, bool dupCausesWarning, ref string return_WarningMessage) {
            string hint = "";
            try {
                //
                LogController.logTrace(core, "addLinkAlias, enter, linkAlias [" + linkAlias + "], pageID [" + pageID + "], queryStringSuffix [" + queryStringSuffix + "], overRideDuplicate [" + overRideDuplicate + "], dupCausesWarning [" + dupCausesWarning + "]");
                //
                const string SafeStringLc = "0123456789abcdefghijklmnopqrstuvwxyz-_/";
                bool AllowLinkAlias = core.siteProperties.getBoolean("allowLinkAlias", true);
                //
                string normalizedLinkAlias = linkAlias;
                if (!string.IsNullOrEmpty(normalizedLinkAlias)) {
                    //
                    // remove nonsafe URL characters
                    string Src = normalizedLinkAlias.Replace('\t', ' ');
                    normalizedLinkAlias = "";
                    for (int srcPtr = 0; srcPtr < Src.Length; srcPtr++) {
                        string TestChr = Src.Substring(srcPtr, 1).ToLowerInvariant();
                        if (!SafeStringLc.Contains(TestChr)) {
                            TestChr = "\t";
                        }
                        normalizedLinkAlias += TestChr;
                    }
                    int Ptr = 0;
                    while (normalizedLinkAlias.Contains("\t\t") && (Ptr < 100)) {
                        normalizedLinkAlias = GenericController.vbReplace(normalizedLinkAlias, "\t\t", "\t");
                        Ptr = Ptr + 1;
                    }
                    if (normalizedLinkAlias.Substring(normalizedLinkAlias.Length - 1) == "\t") {
                        normalizedLinkAlias = normalizedLinkAlias.Left(normalizedLinkAlias.Length - 1);
                    }
                    if (normalizedLinkAlias.Left(1) == "\t") {
                        normalizedLinkAlias = normalizedLinkAlias.Substring(1);
                    }
                    normalizedLinkAlias = GenericController.vbReplace(normalizedLinkAlias, "\t", "-");
                    if (!string.IsNullOrEmpty(normalizedLinkAlias)) {
                        if (normalizedLinkAlias.Left(1) != "/") {
                            normalizedLinkAlias = "/" + normalizedLinkAlias;
                        }
                        //
                        LogController.logTrace(core, "addLinkAlias, normalized normalizedLinkAlias [" + normalizedLinkAlias + "]");
                        //
                        // Make sure there is not a folder or page in the wwwroot that matches this Alias
                        //
                        if (GenericController.vbLCase(normalizedLinkAlias) == GenericController.vbLCase("/" + core.appConfig.name)) {
                            //
                            // This alias points to the cclib folder
                            //
                            if (AllowLinkAlias) {
                                return_WarningMessage = ""
                                    + "The Link Alias being created (" + normalizedLinkAlias + ") can not be used because there is a virtual directory in your website directory that already uses this name."
                                    + " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page.";
                            }
                        } else if (GenericController.vbLCase(normalizedLinkAlias) == "/cclib") {
                            //
                            // This alias points to the cclib folder
                            //
                            if (AllowLinkAlias) {
                                return_WarningMessage = ""
                                    + "The Link Alias being created (" + normalizedLinkAlias + ") can not be used because there is a virtual directory in your website directory that already uses this name."
                                    + " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page.";
                            }
                        } else if (core.wwwFiles.pathExists(core.appConfig.localWwwPath + "\\" + normalizedLinkAlias.Substring(1))) {
                            //
                            // This alias points to a different link, call it an error
                            //
                            if (AllowLinkAlias) {
                                return_WarningMessage = ""
                                    + "The Link Alias being created (" + normalizedLinkAlias + ") can not be used because there is a folder in your website directory that already uses this name."
                                    + " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page.";
                            }
                        } else {
                            //
                            // Make sure there is one here for this
                            //
                            int linkAliasId = 0;
                            using (var csData = new CsModel(core)) {
                                csData.open("Link Aliases", "name=" + DbController.encodeSQLText(normalizedLinkAlias), "", false, 0, "Name,PageID,QueryStringSuffix");
                                if (!csData.ok()) {
                                    //
                                    LogController.logTrace(core, "addLinkAlias, not found in Db, add");
                                    //
                                    // Alias not found, create a Link Aliases
                                    //
                                    csData.close();
                                    csData.insert("Link Aliases");
                                    if (csData.ok()) {
                                        csData.set("Name", normalizedLinkAlias);
                                        csData.set("Pageid", pageID);
                                        csData.set("QueryStringSuffix", queryStringSuffix);
                                    }
                                } else {
                                    //
                                    LogController.logTrace(core, "addLinkAlias, this linkalias found by its name...");
                                    //
                                    // Alias found, verify the pageid & QueryStringSuffix
                                    //
                                    int CurrentLinkAliasID = 0;
                                    bool resaveLinkAlias = false;
                                    int LinkAliasPageID = csData.getInteger("pageID");
                                    if ((csData.getText("QueryStringSuffix").ToLowerInvariant() == queryStringSuffix.ToLowerInvariant()) && (pageID == LinkAliasPageID)) {
                                        CurrentLinkAliasID = csData.getInteger("id");
                                        //
                                        LogController.logTrace(core, "addLinkAlias, linkalias matches name, pageid, and querystring of linkalias [" + CurrentLinkAliasID + "]");
                                        //
                                        // it maches a current entry for this link alias, if the current entry is not the highest number id,
                                        //   remove it and add this one
                                        //
                                        string sql = "select top 1 id from ccLinkAliases where (pageid=" + LinkAliasPageID + ")and(QueryStringSuffix=" + DbController.encodeSQLText(queryStringSuffix) + ") order by id desc";
                                        using (var CS3 = new CsModel(core)) {
                                            CS3.openSql(sql);
                                            if (CS3.ok()) {
                                                resaveLinkAlias = (CurrentLinkAliasID != CS3.getInteger("id"));
                                            }
                                        }
                                        if (resaveLinkAlias) {
                                            //
                                            LogController.logTrace(core, "addLinkAlias, another link alias matches this pageId and QS. Move this to the top position");
                                            //
                                            core.db.executeQuery("delete from ccLinkAliases where id=" + CurrentLinkAliasID);
                                            using (var CS3 = new CsModel(core)) {
                                                CS3.insert("Link Aliases");
                                                if (CS3.ok()) {
                                                    CS3.set("Name", normalizedLinkAlias);
                                                    CS3.set("Pageid", pageID);
                                                    CS3.set("QueryStringSuffix", queryStringSuffix);
                                                }
                                            }
                                        }
                                    } else {
                                        //
                                        // link alias matches, but id/qs does not -- this is either a change, or a duplicate that needs to be blocked
                                        //
                                        if (overRideDuplicate) {
                                            //
                                            // change the Link Alias to the new link
                                            csData.set("Pageid", pageID);
                                            csData.set("QueryStringSuffix", queryStringSuffix);
                                        } else if (AllowLinkAlias) {
                                            //
                                            // This alias points to a different link, and link aliasing is in use, call it an error (but save record anyway)
                                            //
                                            if (dupCausesWarning) {
                                                if (LinkAliasPageID == 0) {
                                                    int PageContentCID = Models.Domain.ContentMetadataModel.getContentId(core, "Page Content");
                                                    return_WarningMessage = ""
                                                        + "This page has been saved, but the Link Alias could not be created (" + normalizedLinkAlias + ") because it is already in use for another page."
                                                        + " To use Link Aliasing (friendly page names) for this page, the Link Alias value must be unique on this site. To set or change the Link Alias, clicke the Link Alias tab and select a name not used by another page or a folder in your website.";
                                                } else {
                                                    int PageContentCID = Models.Domain.ContentMetadataModel.getContentId(core, "Page Content");
                                                    return_WarningMessage = ""
                                                        + "This page has been saved, but the Link Alias could not be created (" + normalizedLinkAlias + ") because it is already in use for another page (<a href=\"?af=4&cid=" + PageContentCID + "&id=" + LinkAliasPageID + "\">edit</a>)."
                                                        + " To use Link Aliasing (friendly page names) for this page, the Link Alias value must be unique. To set or change the Link Alias, click the Link Alias tab and select a name not used by another page or a folder in your website.";
                                                }
                                            }
                                        }
                                    }
                                }
                                linkAliasId = csData.getInteger("id");
                                csData.close();
                            }
                            core.cache.invalidateDbRecord(linkAliasId, LinkAliasModel.contentTableNameLowerCase);
                            //
                            // -- force route reload if this is a webserver page
                            Models.Domain.RouteMapModel.invalidateCache(core);
                            core.routeMapCacheClear();
                        }
                    }
                }
                //
                LogController.logTrace(core, "addLinkAlias, exit");
                //
            } catch (Exception ex) {
                LogController.logError(core, ex, "addLinkAlias exception, hint [" + hint + "]");
                throw;
            }
        }
        //
    }
}

